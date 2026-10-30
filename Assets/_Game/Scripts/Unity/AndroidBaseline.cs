using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace TheTrailWalker.Prototypes
{
    // Disposable milestone-0 probe, not the game's persistent session/bootstrap.
    public sealed class AndroidBaseline : MonoBehaviour
    {
        [SerializeField] private Renderer testCube;
        [SerializeField] private Button touchButton;
        [SerializeField] private Text statusText;
        [SerializeField] private RectTransform safeArea;
        [SerializeField] private int targetFrameRate = 30;
        [SerializeField] private float rotationDegreesPerSecond = 25f;

        private int taps;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private float activeSeconds;
        private float sampleSeconds;
        private int sampleFrames;
        private bool recording;
        private bool finished;
        private MaterialPropertyBlock colorBlock;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public void Configure(Renderer cube, Button button, Text label, RectTransform safeRect)
        {
            testCube = cube;
            touchButton = button;
            statusText = label;
            safeArea = safeRect;
        }

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
            colorBlock = new MaterialPropertyBlock();
            touchButton.onClick.AddListener(OnTap);
            statusText.text = "Tap the button. The cube should change color.\nCollecting a 10-second baseline after warmup...";
            ApplySafeArea();
            Debug.Log($"[Milestone0] Started on {SystemInfo.deviceModel}; {SystemInfo.operatingSystem}; {SystemInfo.graphicsDeviceType}; target {targetFrameRate} FPS.");
        }

        private void Update()
        {
            testCube.transform.Rotate(0f, rotationDegreesPerSecond * Time.deltaTime, 0f, Space.World);
            if (lastSafeArea != Screen.safeArea || lastScreenSize != new Vector2Int(Screen.width, Screen.height))
                ApplySafeArea();

            if (finished) return;
            activeSeconds += Time.unscaledDeltaTime;
            if (activeSeconds < 3f) return;
            if (!recording)
            {
                recording = true;
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
                Profiler.logFile = Path.Combine(Application.persistentDataPath, "milestone0.raw");
                Profiler.enableBinaryLog = true;
                Profiler.enabled = true;
#endif
                return;
            }
            sampleSeconds += Time.unscaledDeltaTime;
            sampleFrames++;
            if (sampleSeconds >= 10f) FinishBaseline();
        }

        private void ApplySafeArea()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            safeArea.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        }

        private void OnTap()
        {
            taps++;
            colorBlock.SetColor(BaseColor, taps % 2 == 0 ? new Color(0.2f, 0.65f, 0.9f) : new Color(1f, 0.55f, 0.15f));
            testCube.SetPropertyBlock(colorBlock);
            statusText.text = $"Touch OK — {taps} taps\n" + (finished ? "Baseline saved. See device log / persistent data." : "Baseline capture in progress...");
            Debug.Log($"[Milestone0] Touch OK: {taps}");
        }

        private void FinishBaseline()
        {
            StopCapture();
            finished = true;
            var data = new BaselineResult
            {
                utc = DateTime.UtcNow.ToString("O"),
                unityVersion = Application.unityVersion,
                device = SystemInfo.deviceModel,
                os = SystemInfo.operatingSystem,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                gpu = SystemInfo.graphicsDeviceName,
                resolution = $"{Screen.width}x{Screen.height}",
                quality = QualitySettings.names[QualitySettings.GetQualityLevel()],
                targetFps = targetFrameRate,
                measuredSeconds = sampleSeconds,
                frames = sampleFrames,
                averageFps = sampleFrames / sampleSeconds,
                averageFrameMilliseconds = 1000f * sampleSeconds / sampleFrames,
                allocatedMemoryBytes = Profiler.GetTotalAllocatedMemoryLong(),
                touchCountAtCaptureEnd = taps,
                developmentBuild = Debug.isDebugBuild,
                note = "Development baseline with profiler overhead. Frame interval is not CPU/GPU execution time or thermal validation."
            };
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "milestone0.json"), json);
            statusText.text = $"Baseline saved: {data.averageFps:F1} FPS / {data.averageFrameMilliseconds:F1} ms\nTouch test: {taps} taps. Tap to change cube color.";
            Debug.Log("[Milestone0] Baseline: " + JsonUtility.ToJson(data));
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused || finished) return;
            // Discard interrupted measurements rather than reporting background time as frame time.
            StopCapture();
            activeSeconds = sampleSeconds = 0f;
            sampleFrames = 0;
            recording = false;
        }

        private void OnDestroy()
        {
            if (touchButton != null) touchButton.onClick.RemoveListener(OnTap);
            StopCapture();
        }

        private void StopCapture()
        {
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
            if (recording)
            {
                Profiler.enabled = false;
                Profiler.enableBinaryLog = false;
            }
#endif
        }

        [Serializable]
        private sealed class BaselineResult
        {
            public string utc, unityVersion, device, os, graphicsApi, gpu, resolution, quality, note;
            public int targetFps, frames, touchCountAtCaptureEnd;
            public float measuredSeconds, averageFps, averageFrameMilliseconds;
            public long allocatedMemoryBytes;
            public bool developmentBuild;
        }
    }
}
