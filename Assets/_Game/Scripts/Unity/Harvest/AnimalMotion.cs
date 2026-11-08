using System;
using UnityEngine;

namespace TheTrailWalker.Harvest
{
    public enum AnimalGait { Hop, Walk }

    // Describes an animal art rig for AnimalAnimator: rigid parts on pivots (no skinning). Legs swing around virtual
    // shoulder/hip joints at (leg pivot x, height, z) in MotionRoot space, so the art pivots can stay where the parts sit.
    public struct AnimalRigParts
    {
        public AnimalGait Gait;
        public Transform MotionRoot, Body, Head, Tail;
        public Transform[] Ears, FrontLegs, HindLegs;      // left first
        public Vector3 BodyCentre;                         // MotionRoot-local pitch/roll centre
        public float ShoulderHeight, ShoulderZ, HipHeight, HipZ;
        public float Stride, HopHeight;                    // metres per hop / per walk cycle at normal pace
        public float GrazeDrop, GrazeAngle;                // head lowering for grazing (metres, degrees)
    }

    // Plan 8.2.2 "interactable look": procedural poses for moving, grazing and looking around.
    // Hop (rabbit): crouch, push off with the hind legs while the forelegs reach, body arcs nose-up then nose-down,
    // the forepaws land first, the hind feet swing forward under the body, then a short pause.
    // Walk (later quadrupeds such as beaver or boar): four-beat lateral-sequence walk (LH, LF, RH, RF) with a light
    // body bob, roll and head nod. Angles: +X pitches the nose down for the body and swings a foot backwards for a leg.
    public sealed class AnimalAnimator
    {
        private readonly AnimalRigParts r;
        private readonly Vector3 rootPos, headPos;
        private readonly Quaternion rootRot, headRot, tailRot;
        private readonly Quaternion[] earRot, frontRot, hindRot;
        private readonly Vector3[] frontPos, hindPos;
        private readonly Renderer[] feet;          // leg renderers, for keeping the feet above the ground
        private readonly float soleHeight;         // lowest leg point above the animal root in the standing pose

        // Keyframes (phase, degrees) for one hop; the pose at phase 0 and 1 is the neutral standing pose.
        private static readonly float[] HopPitch = { 0, 0, .10f, 4, .20f, -12, .45f, -2, .68f, 12, .80f, 6, 1, 0 };
        private static readonly float[] HopFront = { 0, 0, .10f, 8, .30f, -38, .60f, -28, .74f, -4, .88f, 12, 1, 0 };
        private static readonly float[] HopHind = { 0, 0, .08f, -6, .20f, 40, .40f, 28, .62f, -24, .80f, -14, 1, 0 };
        public const float HopTakeoff = .10f, HopLanding = .75f;   // airborne part of the hop phase

        public AnimalAnimator(AnimalRigParts parts)
        {
            r = parts;
            rootPos = r.MotionRoot.localPosition; rootRot = r.MotionRoot.localRotation;
            headPos = r.Head.localPosition; headRot = r.Head.localRotation;
            tailRot = r.Tail != null ? r.Tail.localRotation : Quaternion.identity;
            earRot = Capture(r.Ears, out _); frontRot = Capture(r.FrontLegs, out frontPos); hindRot = Capture(r.HindLegs, out hindPos);
            var list = new System.Collections.Generic.List<Renderer>();
            foreach (var legs in new[] { r.FrontLegs, r.HindLegs }) if (legs != null) foreach (var leg in legs) if (leg != null) list.AddRange(leg.GetComponentsInChildren<Renderer>());
            feet = list.ToArray(); soleHeight = Lowest();
        }

        private float Lowest()
        {
            float lowest = float.MaxValue;
            foreach (var renderer in feet) lowest = Mathf.Min(lowest, renderer.bounds.min.y);
            return feet.Length == 0 ? 0f : lowest - r.MotionRoot.parent.position.y;
        }

        public AnimalGait Gait => r.Gait;
        public float Stride => r.Stride;

        private static Quaternion[] Capture(Transform[] parts, out Vector3[] positions)
        {
            parts = parts ?? Array.Empty<Transform>();
            var rotations = new Quaternion[parts.Length]; positions = new Vector3[parts.Length];
            for (int i = 0; i < parts.Length; i++) if (parts[i] != null) { rotations[i] = parts[i].localRotation; positions[i] = parts[i].localPosition; }
            return rotations;
        }

        // phase: 0-1 through the current hop or walk cycle. move: 0 standing, 1 travelling. energy: 1 calm, higher when
        // fleeing (bigger, flatter hops). graze/alert: 0-1 blend weights. lookYaw: head turn in degrees. time: seconds.
        public void Pose(float phase, float move, float energy, float graze, float alert, float lookYaw, float time)
        {
            float pitch = 0, roll = 0, lift = 0, headPitch = 0, earBack = 0, tailUp = 0;
            float frontL = 0, frontR = 0, hindL = 0, hindR = 0, liftFrontL = 0, liftFrontR = 0, liftHindL = 0, liftHindR = 0;
            if (move > 0f)
            {
                if (r.Gait == AnimalGait.Hop)
                {
                    float air = phase > HopTakeoff && phase < HopLanding ? Mathf.Sin(Mathf.PI * (phase - HopTakeoff) / (HopLanding - HopTakeoff)) : 0f;
                    lift = r.HopHeight * Mathf.Lerp(1f, 1.6f, Mathf.Clamp01(energy - 1f)) * air;
                    pitch = Keys(phase, HopPitch); frontL = frontR = Keys(phase, HopFront); hindL = hindR = Keys(phase, HopHind);
                    headPitch = -0.5f * pitch; earBack = 22f * air * energy; tailUp = 25f * air;
                }
                else
                {
                    float a = Mathf.PI * 2f * phase, swing = 22f;
                    hindL = -swing * Mathf.Sin(a); frontL = -swing * Mathf.Sin(a + Mathf.PI * .5f);
                    hindR = -swing * Mathf.Sin(a + Mathf.PI); frontR = -swing * Mathf.Sin(a + Mathf.PI * 1.5f);
                    // A foot lifts while it swings forward (its angle is decreasing).
                    liftHindL = .02f * Mathf.Max(0, -Mathf.Cos(a)); liftFrontL = .02f * Mathf.Max(0, -Mathf.Cos(a + Mathf.PI * .5f));
                    liftHindR = .02f * Mathf.Max(0, -Mathf.Cos(a + Mathf.PI)); liftFrontR = .02f * Mathf.Max(0, -Mathf.Cos(a + Mathf.PI * 1.5f));
                    lift = .012f * (1f - Mathf.Cos(2f * a)) * .5f; pitch = 1.5f * Mathf.Sin(2f * a); roll = 2.5f * Mathf.Sin(a);
                    headPitch = 4f * Mathf.Sin(2f * a + .6f); earBack = 4f; tailUp = 6f * Mathf.Sin(a);
                }
                pitch *= move; roll *= move; lift *= move; headPitch *= move; earBack *= move; tailUp *= move;
                frontL *= move; frontR *= move; hindL *= move; hindR *= move;
            }
            // Grazing: nose down to the grass, small nibbling nods, ears relaxed back.
            pitch += 7f * graze; earBack += 38f * graze;   // ears lie back along the neck while the head is down
            float nibble = graze * 4f * Mathf.Sin(time * 11f) * (0.5f + 0.5f * Mathf.Sin(time * 1.7f));
            // Alert: sits back a little, head up, ears upright, with an occasional ear flick.
            pitch -= 5f * alert; headPitch -= 8f * alert; earBack -= 10f * alert;
            float flick = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(time * 1.9f)), 30f) * 24f;

            var bend = Quaternion.Euler(pitch, 0, roll);
            r.MotionRoot.localRotation = rootRot * bend;
            r.MotionRoot.localPosition = rootPos + Vector3.up * lift + rootRot * (r.BodyCentre - bend * r.BodyCentre);
            Swing(r.FrontLegs, frontPos, frontRot, r.ShoulderHeight, r.ShoulderZ, frontL, frontR, liftFrontL, liftFrontR);
            Swing(r.HindLegs, hindPos, hindRot, r.HipHeight, r.HipZ, hindL, hindR, liftHindL, liftHindR);
            // Body pitch (moving, grazing, sitting up) and leg swings can push a foot below the standing sole line; lift the body just enough to keep
            // every foot on or above the ground (never lowers it, so planted feet stay planted).
            if (feet.Length > 0)
            {
                float sink = soleHeight - Lowest();
                if (sink > 0f) r.MotionRoot.localPosition += Vector3.up * sink;
            }
            r.Head.localPosition = headPos + Vector3.down * (r.GrazeDrop * graze);
            r.Head.localRotation = headRot * Quaternion.Euler(headPitch + r.GrazeAngle * graze + nibble, lookYaw, 0);
            if (r.Ears != null)
                for (int i = 0; i < r.Ears.Length; i++)
                    if (r.Ears[i] != null) r.Ears[i].localRotation = earRot[i] * Quaternion.Euler(-earBack - (i == 0 ? flick : 0f), 0, 0);
            if (r.Tail != null) r.Tail.localRotation = tailRot * Quaternion.Euler(-tailUp, 0, 0);
        }

        private static void Swing(Transform[] legs, Vector3[] pos, Quaternion[] rot, float height, float z, float left, float right, float liftLeft, float liftRight)
        {
            if (legs == null) return;
            for (int i = 0; i < legs.Length; i++)
            {
                if (legs[i] == null) continue;
                var joint = new Vector3(pos[i].x, height, z);
                var turn = Quaternion.Euler(i == 0 ? left : right, 0, 0);
                legs[i].localPosition = joint + turn * (pos[i] - joint) + Vector3.up * (i == 0 ? liftLeft : liftRight);
                legs[i].localRotation = turn * rot[i];
            }
        }

        // Piecewise curve through (phase, value) pairs with eased segments.
        public static float Keys(float p, float[] keys)
        {
            for (int i = 2; i < keys.Length; i += 2)
                if (p <= keys[i])
                {
                    float t = Mathf.InverseLerp(keys[i - 2], keys[i], p);
                    return Mathf.Lerp(keys[i - 1], keys[i + 1], t * t * (3f - 2f * t));
                }
            return keys[keys.Length - 1];
        }
    }

    // Behaviour around the fixed spawn point (plan 7.3 / 8.2.2): graze, look around, move to another spot inside the
    // encounter area, and travel toward a given point while hunted. The root transform is the source of truth for the
    // position, so external resets (back to the spawn point) are picked up; call Stop() after one to drop a hop in flight.
    public sealed class AnimalWanderer
    {
        private enum State { Graze, Look, Move }
        private readonly AnimalAnimator anim;
        private readonly System.Random rng;
        public Vector3 Home;
        public float Radius = 1f;

        private State state; private float stateEnd, nextLook;
        private Vector3 target;
        private float phase, pauseEnd, hopTime;
        private bool hopping; private Vector3 hopFrom, hopTo;
        private float graze, alert, lookYaw, lookGoal, energy = 1f, clock;
        public AnimalAnimator Animator => anim;

        public AnimalWanderer(AnimalAnimator animator, Vector3 home, int seed)
        {
            anim = animator; Home = home; rng = new System.Random(seed);
            state = Range(0f, 1f) < .6f ? State.Graze : State.Look; stateEnd = Range(0.5f, 4f); graze = state == State.Graze ? 1f : 0f;
        }

        private float Range(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        public void Stop() { hopping = false; phase = 0f; }

        // Idle: graze, look around and move about the spawn point. time is a running clock (seconds).
        public void UpdateIdle(Transform root, float dt, float time, Func<Vector3, float> ground)
        {
            clock = time;
            if (stateEnd <= 0f || (time >= stateEnd && state != State.Move))
                Next(root, time);
            bool moving = false;
            if (state == State.Graze) { Blend(1f, 0f, dt); Settle(root, dt, ground); }
            else if (state == State.Look)
            {
                Blend(0f, 1f, dt);
                if (time >= nextLook) { lookGoal = Range(-30f, 30f); nextLook = time + Range(.5f, 1.3f); }
                Settle(root, dt, ground);
            }
            else
            {
                Blend(0f, .6f, dt); lookGoal = 0f; energy = 1f; moving = true;
                bool arrived = Travel(root, target, dt, time, false, ground);
                if (arrived || time >= stateEnd) { Stop(); Next(root, time); }
            }
            lookYaw = Mathf.MoveTowards(lookYaw, lookGoal, 90f * dt);
            anim.Pose(phase, moving || hopping ? 1f : 0f, energy, graze, alert, lookYaw, time);
        }

        // Hunted: alert and travelling toward target (chosen by the hunt), faster after a miss.
        public void UpdateHunted(Transform root, Vector3 goal, bool boosted, float dt, float time, Func<Vector3, float> ground)
        {
            clock = time;
            state = State.Move; stateEnd = 0f;
            Blend(0f, 1f, dt); lookGoal = 0f; lookYaw = Mathf.MoveTowards(lookYaw, 0f, 120f * dt);
            energy = boosted ? 2.2f : 1f;
            Travel(root, goal, dt, time, true, ground);
            anim.Pose(phase, 1f, energy, graze, alert, lookYaw, time);
        }

        private void Next(Transform root, float time)
        {
            State previous = state;
            if (previous == State.Graze) state = State.Look;
            else if (previous == State.Look) state = Range(0f, 1f) < .65f ? State.Move : State.Graze;
            else state = Range(0f, 1f) < .5f ? State.Look : State.Graze;
            if (state == State.Move)
            {
                // A new spot inside the encounter area, not too close to the current one; drift back if far out.
                Vector3 here = Flat(root.position);
                for (int i = 0; i < 6; i++)
                {
                    float a = Range(0f, Mathf.PI * 2f), d = Radius * Mathf.Sqrt(Range(0f, 1f));
                    target = Flat(Home) + new Vector3(Mathf.Cos(a) * d, 0, Mathf.Sin(a) * d);
                    if ((target - here).magnitude >= .35f) break;
                }
                stateEnd = time + 7f;
            }
            else stateEnd = time + (state == State.Graze ? Range(3f, 7f) : Range(1f, 2.5f));
        }

        private void Blend(float grazeGoal, float alertGoal, float dt)
        {
            graze = Mathf.MoveTowards(graze, grazeGoal, dt * 2.2f); alert = Mathf.MoveTowards(alert, alertGoal, dt * 3f);
        }

        // Finishes a hop in flight before standing still.
        private void Settle(Transform root, float dt, Func<Vector3, float> ground)
        {
            if (hopping) StepHop(root, dt, ground);
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        // Moves toward goal with the rig's gait; returns true when there.
        private bool Travel(Transform root, Vector3 goal, float dt, float time, bool hunted, Func<Vector3, float> ground)
        {
            Vector3 here = Flat(root.position), to = Flat(goal) - here;
            float distance = to.magnitude;
            if (anim.Gait == AnimalGait.Hop)
            {
                if (hopping) { StepHop(root, dt, ground); return false; }
                if (distance < .04f) return true;
                // Turn toward the goal between hops (rabbits pivot on the spot), then push off once roughly facing it.
                Quaternion face = Quaternion.LookRotation(to / distance);
                root.rotation = Quaternion.RotateTowards(root.rotation, face, (hunted ? 540f : 300f) * energy * dt);
                if (time < pauseEnd || Quaternion.Angle(root.rotation, face) > 25f) return false;
                bool fast = energy > 1.5f;
                float stride = Mathf.Min(anim.Stride * (fast ? 2.2f : 1f), distance);
                hopTime = fast ? .28f : .36f;
                hopFrom = root.position; hopTo = root.position + root.forward * stride; hopping = true; phase = 0f;
                pauseEnd = 0f;
                return false;
            }
            // Walk: continuous travel, turning while moving; the cycle advances with the distance covered.
            if (distance < .04f) { phase = Mathf.MoveTowards(phase, Mathf.Round(phase * 2f) * .5f, dt); return true; }
            float speed = anim.Stride * (energy > 1.5f ? 2f : 1f) / (energy > 1.5f ? .55f : .9f);
            Quaternion look = Quaternion.LookRotation(to / distance);
            root.rotation = Quaternion.RotateTowards(root.rotation, look, 160f * energy * dt);
            float step = Mathf.Min(distance, speed * dt * Mathf.Clamp01(1.2f - Quaternion.Angle(root.rotation, look) / 90f));
            Vector3 next = root.position + root.forward * step;
            root.position = new Vector3(next.x, ground(next), next.z);
            phase = Mathf.Repeat(phase + step / anim.Stride, 1f);
            return false;
        }

        private void StepHop(Transform root, float dt, Func<Vector3, float> ground)
        {
            phase += dt / hopTime;
            if (phase >= 1f)
            {
                phase = 0f; hopping = false;
                pauseEnd = clock + (energy > 1.5f ? Range(0f, .04f) : state == State.Move && alert > .9f ? Range(.04f, .14f) : Range(.15f, .5f));
            }
            float s = Mathf.Clamp01((Mathf.Min(phase == 0f ? 1f : phase, 1f) - AnimalAnimator.HopTakeoff) / (AnimalAnimator.HopLanding - AnimalAnimator.HopTakeoff));
            s = s * s * (3f - 2f * s);
            Vector3 p = Vector3.Lerp(hopFrom, hopTo, s);
            root.position = new Vector3(p.x, ground(p), p.z);
        }
    }
}
