using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain;
using TheTrailWalker.Movement;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Runs inside Unity so vector math and camera transforms use the real engine implementation.
    public static class Milestone1Validation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool condition, string name)
        { if (!condition) throw new InvalidOperationException("Milestone 1 check failed: " + name); passed.Add(name); }
        private static bool Near(float a, float b) => Mathf.Abs(a - b) < 0.0001f;
        [MenuItem("The Trail Walker/Milestone 1/2 - Validate Movement Rules")]
        public static void Run()
        {
            passed.Clear();
            var state = new MovementState();
            Check(Near(state.SpeedCap, MovementState.StarterSpeedCap) && state.ActualSpeed == 0, "Starter cap and stopped intent");
            state.SpeedGesture(10); Check(Near(state.RequestedSpeed, MovementState.StarterSpeedCap), "Requested speed clamps to boots cap"); state.SetSpeedCap(0.8f); // the sequence below uses a fixed 0.8 m/s cap
            state.Advance(10, 150); Check(Near(state.Distance, 8), "Distance uses metres per second");
            state.SpeedGesture(-10); state.Advance(5, 150); Check(Near(state.Distance, 8) && state.ActualSpeed == 0, "Downward gesture cannot reverse");
            state.SpeedGesture(0.6f); state.OpenInventory(); state.OpenInventory();
            state.Advance(10, 150); Check(Near(state.Distance, 8) && Near(state.InventoryResumeSpeed, 0.6f), "Nested inventory snapshots once and freezes travel");
            state.CloseInventory(); Check(state.ActualSpeed == 0, "Nested close keeps pause");
            state.CloseInventory(); Check(Near(state.ActualSpeed, 0.6f), "Final inventory close restores intent");
            state.CloseInventory(); Check(state.InventoryDepth == 0, "Extra close is harmless");
            state.SetSpeedCap(1.6f); state.SpeedGesture(1); state.OpenInventory(); state.SetSpeedCap(0.8f); state.CloseInventory();
            Check(Near(state.ActualSpeed, 0.8f), "Cap reduction overrides saved speed");
            state.BeginHarvest(); state.SpeedGesture(1); state.EndHarvest();
            Check(state.ActualSpeed == 0 && state.PostHarvestStop, "Upward gesture during committed harvest is ignored");
            state.OpenInventory(); state.CloseInventory(); Check(state.ActualSpeed == 0, "Inventory cannot clear post-harvest stop");
            state.SpeedGesture(-1); Check(state.PostHarvestStop, "Downward gesture cannot clear harvest stop");
            state.SpeedGesture(0.4f); Check(Near(state.ActualSpeed, 0.4f) && !state.PostHarvestStop, "Fresh upward gesture resumes after harvest");
            state.OpenInventory(); state.BeginHarvest(); state.EndHarvest(); state.CloseInventory();
            Check(state.ActualSpeed == 0 && state.InventoryResumeSpeed == 0, "Harvest invalidates even an earlier inventory snapshot");
            state.SpeedGesture(0.8f); state.OpenInventory(); state.SetPause(MovementPauseReasons.Exhausted, true); state.CloseInventory();
            Check(state.ActualSpeed == 0 && (state.Pauses & MovementPauseReasons.Exhausted) != 0, "Inventory close preserves other pause reasons");
            state.SetPause(MovementPauseReasons.Exhausted, false);
            float before = state.Distance;
            state.SetPause(MovementPauseReasons.Suspended, true); state.Advance(100, 150);
            Check(Near(before, state.Distance), "Suspension cannot advance distance");
            state.SetPause(MovementPauseReasons.Unfocused, true); state.SetPause(MovementPauseReasons.Suspended, false);
            Check(state.ActualSpeed == 0, "Focus and suspension pauses remain independent");
            state.SetPause(MovementPauseReasons.Unfocused, false); Check(Near(state.ActualSpeed, 0.8f), "Foreground restores valid intent");
            state.Advance(10000, 150); Check(Near(state.Distance, 150) && state.ActualSpeed == 0, "Large frame clamps exactly to camp");
            state.OpenInventory(); state.CloseInventory(); state.SpeedGesture(1); state.Advance(5, 150);
            Check(Near(state.Distance, 150) && state.RequestedSpeed == 0, "Inventory and gestures cannot walk past camp");
            state.Reset(); state.OpenInventory(); state.CloseInventory(); Check(state.ActualSpeed == 0, "Opening inventory while stopped stays stopped");
            Check(GestureOwnership.Resolve(true, 30, 10, 10) == GestureOwner.Orbit, "Horizontal avatar gesture chooses orbit");
            Check(GestureOwnership.Resolve(true, 10, 30, 10) == GestureOwner.Speed, "Vertical avatar gesture chooses speed");
            Check(GestureOwnership.Resolve(false, 10, 30, 10) == GestureOwner.Blocked, "World vertical cannot change speed");
            Check(GestureOwnership.Resolve(false, 30, 10, 10) == GestureOwner.Orbit, "World horizontal chooses orbit");
            Check(GestureOwnership.Resolve(true, 20, 20, 10) == GestureOwner.Pending, "Diagonal gesture waits for dominant axis");
            Check(GestureOwnership.Resolve(true, 4, 1, 10) == GestureOwner.Pending, "Tap jitter cannot claim gesture");
            var path = new SampledTrailPath(MovementPrototype.DefaultKnots());
            Check(path.Length >= 100 && path.Length <= 250, "Prototype trail length is within 100-250 metres");
            path.Evaluate(-1, out Vector3 start, out _); path.Evaluate(path.Length + 20, out Vector3 end, out _);
            Check(Vector3.Distance(start, Vector3.zero) < 0.001f && Vector3.Distance(end, new Vector3(-5, 0, 140)) < 0.001f, "Path clamps endpoints");
            float worst = 0;
            for (float distance = 0.1f; distance < path.Length; distance += 0.1f)
            {
                path.Evaluate(distance - 0.1f, out Vector3 a, out _); path.Evaluate(distance, out Vector3 b, out Vector3 t);
                worst = Mathf.Max(worst, Mathf.Abs(Vector3.Distance(a, b) - 0.1f));
                if (Mathf.Abs(t.magnitude - 1) > 0.001f) throw new Exception("Non-unit trail tangent");
            }
            Check(worst < 0.002f, "Arc-length lookup keeps curved walking speed uniform");
            var cameraObject = new GameObject("Validation camera"); var player = new GameObject("Validation walker");
            try
            {
                var orbit = cameraObject.AddComponent<CameraOrbitController>();
                var motor = player.AddComponent<TrailMotor>(); state.Reset(); state.SpeedGesture(0.8f); motor.Bind(state, path);
                for (int i = 0; i < 8; i++) orbit.Rotate(90);
                Check(Near(orbit.Yaw, 0) && Near(state.ActualSpeed, 0.8f), "Two full orbits preserve walking intent");
                orbit.Rotate(-45); Check(Near(orbit.Yaw, 315), "Negative yaw wraps through 360 degrees");
                state.Advance(1, path.Length); motor.Present(); path.Evaluate(0.8f, out Vector3 p, out Vector3 tangent);
                Check(Vector3.Distance(player.transform.position, p) < 0.001f && Vector3.Dot(player.transform.forward, tangent) > 0.999f, "Camera orbit cannot steer path motor");
                float yaw = orbit.Yaw; state.OpenInventory(); state.CloseInventory();
                Check(Near(orbit.Yaw, yaw), "Inventory round trip preserves chosen yaw");
            }
            finally { UnityEngine.Object.DestroyImmediate(cameraObject); UnityEngine.Object.DestroyImmediate(player); }
            Directory.CreateDirectory("Builds/Validation");
            File.WriteAllLines("Builds/Validation/Milestone1-checks.txt", passed);
            Debug.Log($"[Milestone1] PASS: {passed.Count} movement/path/gesture checks. Trail length {path.Length:F2} m.");
        }
    }
}
