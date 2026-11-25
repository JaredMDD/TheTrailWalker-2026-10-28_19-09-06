using System;
using System.IO;
using TheTrailWalker.SceneLoading;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTrailWalker.Editor
{
    public static class Milestone5Setup
    {
        public const string BootstrapPath = "Assets/_Game/Scenes/TravelBootstrap.unity";
        public const string TrailPath = "Assets/_Game/Scenes/Prototypes/TravelTrail.unity";

        // Reruns every earlier milestone's checks, then the travel rules, before preparing the camp-travel scenes.
        [MenuItem("The Trail Walker/Milestone 5/Prepare and Validate")]
        public static void Prepare()
        {
            KeepFogVariants();
            Milestone4Setup.Prepare(); Milestone4EquipmentValidation.Run(); Milestone5Validation.Run(); Milestone6Validation.Run(); Milestone7Validation.Run(); Milestone8Validation.Run(); Milestone9Validation.Run(); TierOneRegionsValidation.Run(); Phase1ConsistencyValidation.Run(); SettlementShellValidation.Run(); HomeValidation.Run(); Milestone12Validation.Run(); Milestone13Validation.Run(); Milestone14Validation.Run(); Milestone14BalloonValidation.Run(); Tier03IntegrationValidation.Run();
            Create(BootstrapPath, () => new GameObject("Session", typeof(GameBootstrapper)).GetComponent<GameBootstrapper>().Configure("TravelTrail"));
            Create(TrailPath, () =>
            {
                var prototype = new GameObject("Travel prototype", typeof(MovementPrototype)).GetComponent<MovementPrototype>();
                prototype.Configure(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementTerrain.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementTrail.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementAvatar.mat"));
                prototype.EnableMilestone5();
            });
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootstrapPath, true), new EditorBuildSettingsScene(TrailPath, true) };
            AssetDatabase.SaveAssets();
        }
        // Mistwood fog is switched on at runtime (RegionLook), so Automatic fog stripping would remove the linear fog
        // shader variants from the device build. Custom stripping keeps them (regions 3-4, November 8, 2026).
        private static void KeepFogVariants()
        {
            var graphics = new SerializedObject(UnityEngine.Rendering.GraphicsSettings.GetGraphicsSettings());
            SerializedProperty stripping = graphics.FindProperty("m_FogStripping"), linear = graphics.FindProperty("m_FogKeepLinear");
            if (stripping.intValue == 1 && linear.boolValue) return;
            stripping.intValue = 1; linear.boolValue = true;
            graphics.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssets();
        }
        private static void Create(string path, Action create)
        {
            if (File.Exists(path)) return;
            Scene previous = SceneManager.GetActiveScene(); Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); SceneManager.SetActiveScene(scene);
            try { create(); if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException(path); }
            finally { if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }
        [MenuItem("The Trail Walker/Milestone 5/Build Travel Development APK")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("Activate Android first.");
            Prepare(); Directory.CreateDirectory("Builds/Android");
            // Gradle packs the debug APK incrementally into its previous output, which keeps dead space from replaced entries
            // (up to ~16 MB seen). Deleting that output makes every APK a fresh pack (November 14, 2026).
            const string gradleApk = "Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build/outputs/apk/debug/launcher-debug.apk";
            if (File.Exists(gradleApk)) File.Delete(gradleApk);
            BuildReport result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { BootstrapPath, TrailPath }, target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android, locationPathName = "Builds/Android/TheTrailWalker-Milestone5-Development.apk", options = BuildOptions.Development });
            if (result.summary.result != BuildResult.Succeeded) throw new Exception("Milestone 5 build failed: " + result.summary.result);
            ApkBudget.Enforce("Builds/Android/TheTrailWalker-Milestone5-Development.apk", result);
        }
    }
    [InitializeOnLoad] internal static class Milestone5Request
    {
        static Milestone5Request() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            const string request = "Temp/Milestone5.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            // Pick up script edits made outside the Editor first; otherwise validation runs the previous assemblies.
            // The request stays on disk across the recompile and domain reload, and runs on a later poll.
            AssetDatabase.Refresh();
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            string command = File.ReadAllText(request).Trim(); File.Delete(request);
            // Never validate or build the previous assemblies when the current scripts do not compile.
            if (EditorUtility.scriptCompilationFailed) { File.WriteAllText("Temp/Milestone5.result", "FAILED script compilation errors (see the Console)"); return; }
            try
            {
                File.WriteAllText("Temp/Milestone5.result", "RUNNING " + command);
                if (command == "build") Milestone5Setup.Build(); else if (command == "prepare") Milestone5Setup.Prepare();
                else if (command == "tier3-audit") Tier03IntegrationAudit.Run();
                else if (command == "tier3-build") Tier03ArtAssets.Build();
                else if (command == "wearables-build") WearableArtCatalogAssets.Build();
                else if (command == "wearables-capture") WearableArtCatalogAssets.Capture();
                else if (command == "containers-build") InventoryContainerAssets.Build(); else throw new ArgumentException(command);
                File.WriteAllText("Temp/Milestone5.result", "PASS " + DateTime.UtcNow.ToString("O"));
            }
            catch (Exception error) { Debug.LogException(error); File.WriteAllText("Temp/Milestone5.result", "FAILED " + error); }
        }
    }
}
