using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain;
using TheTrailWalker.Movement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheTrailWalker
{
    // Device benchmark for choosing graphics limits. Runs only when "benchmark.txt" exists in persistentDataPath
    // (pushed with adb), then deletes it. Each stage applies a graphics profile with the frame cap lifted, slowly
    // turns the camera through a full circle, and records frame times; results go to benchmark-results.txt and the log.
    public sealed class PerformanceBenchmark : MonoBehaviour
    {
        public static string TriggerPath => Path.Combine(Application.persistentDataPath, "benchmark.txt");
        public static string ResultPath => Path.Combine(Application.persistentDataPath, "benchmark-results.txt");
        // Dawn Meadow -> Quiet Meadow: camp with traders, trees, rabbits and the full scenery kit.
        public const string SegmentId = "segment.01.02";
        private const float ViewDistance = 6f;
        private const float Warmup = 3f, Measure = 12f;
        private CameraOrbitController orbit;
        private Light sun;

        private struct Stage
        {
            public string Name; public float Scale, Distance; public int Msaa, Shadow, Cascades; public bool DoubleScenery;
            public Stage(string name, float scale, int msaa, int shadow, float distance, int cascades, bool doubleScenery = false)
            { Name = name; Scale = scale; Msaa = msaa; Shadow = shadow; Distance = distance; Cascades = cascades; DoubleScenery = doubleScenery; }
        }

        private static readonly Stage[] Stages = {
            new Stage("current (1.25x, MSAA4, 4096, 120 m, 4 cascades)", 1.25f, 4, 4096, 120, 4),
            new Stage("previous low (0.8x, no MSAA, 1024, 25 m, 1 cascade)", .8f, 1, 1024, 25, 1),
            new Stage("render scale 1.25x", 1.25f, 4, 2048, 70, 2),
            new Stage("render scale 1.5x", 1.5f, 4, 2048, 70, 2),
            new Stage("MSAA 8x", 1f, 8, 2048, 70, 2),
            new Stage("shadows 4096, 120 m, 4 cascades", 1f, 4, 4096, 120, 4),
            new Stage("double scenery", 1f, 4, 2048, 70, 2, true),
            new Stage("combined high (1.25x, 4096, 120 m, 4 cascades, double scenery)", 1.25f, 4, 4096, 120, 4, true),
            new Stage("current again (thermal check)", 1.25f, 4, 4096, 120, 4),
        };

        public void Bind(CameraOrbitController cameraOrbit, TrailMotor motor, MovementState movement)
        {
            orbit = cameraOrbit;
            // Fixed viewpoint a few metres along the road from the start camp, standing; every stage turns a full circle.
            movement.Restore(ViewDistance, 0, movement.SpeedCap, 0, false, false, false);
            motor.Present();
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            try { File.Delete(TriggerPath); } catch (IOException) { }
            var asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset == null) yield break;
            float scale = asset.renderScale, distance = asset.shadowDistance; int msaa = asset.msaaSampleCount, shadow = asset.mainLightShadowmapResolution, cascades = asset.shadowCascadeCount;
            int cap = Application.targetFrameRate;
            Application.targetFrameRate = 144; QualitySettings.vSyncCount = 0;
            sun = FindAnyObjectByType<Light>();
            var report = new StringBuilder();
            report.AppendLine($"TheTrailWalker benchmark | {SystemInfo.deviceModel} | {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType}) | " +
                              $"{Screen.width}x{Screen.height} @ {Screen.currentResolution.refreshRateRatio.value:F0} Hz | uncapped (target 144)");
            // Ask for the display's highest refresh rate so frames are not capped at 60 Hz when the GPU has headroom.
            Resolution best = Screen.currentResolution;
            foreach (Resolution r in Screen.resolutions) if (r.refreshRateRatio.value > best.refreshRateRatio.value) best = r;
            Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow, best.refreshRateRatio);
            for (float t = 0; t < 4f; t += Time.unscaledDeltaTime) yield return null; // let the scene settle after loading
            report.AppendLine($"Refresh requested {best.refreshRateRatio.value:F0} Hz, now {Screen.currentResolution.refreshRateRatio.value:F0} Hz. " +
                              "Frame budget at 60 FPS is 16.7 ms: GPU/CPU times well below it mean headroom even when FPS is capped.");
            File.WriteAllText(ResultPath, report.ToString());
            GameObject copy = null;
            foreach (Stage stage in Stages)
            {
                asset.renderScale = stage.Scale; asset.msaaSampleCount = stage.Msaa; asset.mainLightShadowmapResolution = stage.Shadow;
                asset.shadowDistance = stage.Distance; asset.shadowCascadeCount = stage.Cascades;
                if (stage.DoubleScenery && copy == null) copy = DuplicateScenery();
                if (!stage.DoubleScenery && copy != null) { Destroy(copy); copy = null; }
                for (float t = 0; t < Warmup; t += Time.unscaledDeltaTime) { Turn(); yield return null; }
                var frames = new List<float>(); double cpu = 0, gpu = 0; int timed = 0;
                for (float t = 0; t < Measure; t += Time.unscaledDeltaTime)
                {
                    Turn(); yield return null;
                    frames.Add(Time.unscaledDeltaTime);
                    FrameTimingManager.CaptureFrameTimings();
                    var timing = new FrameTiming[1];
                    if (FrameTimingManager.GetLatestTimings(1, timing) > 0 && timing[0].gpuFrameTime > 0) { cpu += timing[0].cpuFrameTime; gpu += timing[0].gpuFrameTime; timed++; }
                }
                frames.Sort();
                float total = 0; foreach (float f in frames) total += f;
                float avg = frames.Count / total, low = 1f / frames[Mathf.Min(frames.Count - 1, (int)(frames.Count * .99f))];
                string line = $"{stage.Name}: avg {avg:F1} FPS | 1% low {low:F1} FPS | frame {1000f * total / frames.Count:F1} ms" +
                              (timed > 0 ? $" | CPU {cpu / timed:F1} ms | GPU {gpu / timed:F1} ms" : " | CPU/GPU timing unavailable");
                Debug.Log("[Benchmark] " + line); report.AppendLine(line);
                File.WriteAllText(ResultPath, report.ToString()); // partial results survive an interruption
            }
            if (copy != null) Destroy(copy);
            asset.renderScale = scale; asset.msaaSampleCount = msaa; asset.mainLightShadowmapResolution = shadow; asset.shadowDistance = distance; asset.shadowCascadeCount = cascades;
            Application.targetFrameRate = cap;
            report.AppendLine("FINISHED"); File.WriteAllText(ResultPath, report.ToString());
            Debug.Log("[Benchmark] Finished; results written to benchmark-results.txt");
        }

        private void Turn() { if (orbit != null) orbit.Rotate(30f * Time.unscaledDeltaTime); }

        // A rotated copy of the segment's decorative scenery roughly doubles the visible decoration geometry.
        private GameObject DuplicateScenery()
        {
            GameObject source = GameObject.Find("Regional scenery");
            var renderers = source != null ? source.GetComponentsInChildren<MeshRenderer>() : null;
            if (renderers == null || renderers.Length == 0) return null; // nothing was scattered
            var bounds = renderers[0].bounds; foreach (MeshRenderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            GameObject copy = Instantiate(source, source.transform.parent);
            copy.transform.RotateAround(bounds.center, Vector3.up, 180f);
            return copy;
        }
    }
}
