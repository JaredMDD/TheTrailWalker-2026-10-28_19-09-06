using System;
using System.IO;
using TheTrailWalker.Prototypes;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheTrailWalker.Editor
{
    public static class Milestone0Setup
    {
        public const string ScenePath = "Assets/_Game/Scenes/Prototypes/AndroidBaseline.unity";
        private const string MaterialPath = "Assets/_Game/Materials/AndroidBaseline.mat";
        private const string PrefabPath = "Assets/_Game/Prefabs/Player/AndroidBaselineCube.prefab";
        private const string MobilePipelinePath = "Assets/Settings/Mobile_RPAsset.asset";

        [MenuItem("The Trail Walker/Milestone 0/1 - Prepare Android Baseline")]
        public static void Prepare()
        {
            ConfigureAndroid();
            if (!File.Exists(ScenePath)) CreateScene();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[Milestone0] Prepared. Open " + ScenePath + " and press Play, or use The Trail Walker > Milestone 0 > 2 - Build Development APK.");
        }

        private static void ConfigureAndroid()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.thetrailwalker.prototype");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.optimizedFramePacing = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.connectProfiler = false; // Runtime writes a bounded binary capture instead.

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobilePipelinePath);
            if (pipeline == null) throw new InvalidOperationException("Mobile URP asset is missing: " + MobilePipelinePath);
            pipeline.supportsHDR = false;
            pipeline.shadowDistance = 25f;
            pipeline.supportsCameraDepthTexture = false;
            pipeline.supportsCameraOpaqueTexture = false;
            EditorUtility.SetDirty(pipeline);
            int mobile = Array.IndexOf(QualitySettings.names, "Mobile");
            if (mobile < 0) throw new InvalidOperationException("Mobile quality level is missing.");
            QualitySettings.SetQualityLevel(mobile, true);
            QualitySettings.renderPipeline = pipeline;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadowDistance = 25f;
        }

        private static void CreateScene()
        {
            // Author additively so an unsaved scene already open in the editor is never overwritten.
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                if (material == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
                    material = new Material(shader);
                    material.SetColor("_BaseColor", new Color(0.2f, 0.65f, 0.9f));
                    AssetDatabase.CreateAsset(material, MaterialPath);
                }

                GameObject cubeSource = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubeSource.name = "Android Baseline Cube";
                cubeSource.GetComponent<Renderer>().sharedMaterial = material;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                if (prefab == null) prefab = PrefabUtility.SaveAsPrefabAsset(cubeSource, PrefabPath);
                UnityEngine.Object.DestroyImmediate(cubeSource);
                var cube = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                cube.transform.position = new Vector3(0f, 0.75f, 0f);
                cube.transform.localScale = Vector3.one * 1.5f;

                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.localScale = Vector3.one * 0.6f;
                ground.GetComponent<Renderer>().sharedMaterial = material;

                var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 3f, -7f);
                cameraObject.transform.LookAt(new Vector3(0f, 0.7f, 0f));
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.08f, 0.12f, 0.17f);
                camera.fieldOfView = 45f;
                camera.allowHDR = false;
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;

                var lightObject = new GameObject("Sun", typeof(Light));
                Light sun = lightObject.GetComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.2f;
                sun.shadows = LightShadows.Hard;
                lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
                RenderSettings.sun = sun;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.35f, 0.4f, 0.45f);

                var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(720f, 1280f);
                scaler.matchWidthOrHeight = 0.5f;
                RectTransform safe = Rect("Safe Area", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                Text title = Label("Title", safe, "THE TRAIL WALKER\nAndroid baseline", 42);
                Place(title.rectTransform, new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.97f));
                Text status = Label("Status", safe, "Tap the button to test touch input.", 28);
                Place(status.rectTransform, new Vector2(0.06f, 0.17f), new Vector2(0.94f, 0.32f));
                RectTransform buttonRect = Rect("Touch Test", safe, new Vector2(0.12f, 0.05f), new Vector2(0.88f, 0.14f), Vector2.zero, Vector2.zero);
                Image image = buttonRect.gameObject.AddComponent<Image>();
                image.color = new Color(0.15f, 0.45f, 0.3f);
                Button button = buttonRect.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                Text buttonLabel = Label("Label", buttonRect, "TAP TO CHANGE COLOR", 28);
                Place(buttonLabel.rectTransform, Vector2.zero, Vector2.one);

                var eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                var probe = new GameObject("Android Baseline", typeof(AndroidBaseline));
                probe.GetComponent<AndroidBaseline>().Configure(cube.GetComponent<Renderer>(), button, status, safe);
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save baseline scene.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        private static Text Label(string name, Transform parent, string value, int size)
        {
            RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void Place(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        [MenuItem("The Trail Walker/Milestone 0/2 - Build Development APK")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new InvalidOperationException("Activate the Android Build Profile before building.");
            Prepare();
            Directory.CreateDirectory("Builds/Android");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Android/TheTrailWalker-Milestone0.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Milestone 0 build failed: " + report.summary.result);
            Debug.Log($"[Milestone0] APK ready: {report.summary.outputPath} ({report.summary.totalSize} bytes)");
        }
    }
}
