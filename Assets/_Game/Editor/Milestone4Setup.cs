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
    public static class Milestone4Setup
    {
        public const string BootstrapPath = "Assets/_Game/Scenes/SurvivalBootstrap.unity";
        public const string TrailPath = "Assets/_Game/Scenes/Prototypes/SurvivalTrail.unity";
        [MenuItem("The Trail Walker/Milestone 4/Prepare Survival Prototype")]
        public static void Prepare()
        {
            Milestone3Setup.Prepare();
            TheTrailWalker.Domain.Equipment.EquipmentModelChecks.Run(); Milestone4PersistenceValidation.Run();
            Create(BootstrapPath, () => new GameObject("Session", typeof(GameBootstrapper)).GetComponent<GameBootstrapper>().Configure("SurvivalTrail"));
            Create(TrailPath, () =>
            {
                var prototype = new GameObject("Survival prototype", typeof(MovementPrototype)).GetComponent<MovementPrototype>();
                prototype.Configure(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementTerrain.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementTrail.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementAvatar.mat"));
                prototype.EnableMilestone4();
            });
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootstrapPath, true), new EditorBuildSettingsScene(TrailPath, true) };
            AssetDatabase.SaveAssets();
        }
        private static void Create(string path, Action create)
        {
            if (File.Exists(path)) return;
            Scene previous = SceneManager.GetActiveScene(); Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); SceneManager.SetActiveScene(scene);
            try { create(); if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException(path); }
            finally { if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }
        [MenuItem("The Trail Walker/Milestone 4/Build Survival Development APK")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("Activate Android first.");
            Prepare(); Directory.CreateDirectory("Builds/Android");
            BuildReport result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { BootstrapPath, TrailPath }, target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android, locationPathName = "Builds/Android/TheTrailWalker-Milestone4-Development.apk", options = BuildOptions.Development });
            if (result.summary.result != BuildResult.Succeeded) throw new Exception("Milestone 4 build failed: " + result.summary.result);
        }
    }
    [InitializeOnLoad] internal static class Milestone4SurvivalRequest
    {
        static Milestone4SurvivalRequest() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            const string request = "Temp/Milestone4-survival.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            string command = File.ReadAllText(request).Trim(); File.Delete(request);
            try
            {
                File.WriteAllText("Temp/Milestone4-survival.result", "RUNNING " + command);
                if (command == "build") Milestone4Setup.Build(); else if (command == "prepare") Milestone4Setup.Prepare(); else throw new ArgumentException(command);
                File.WriteAllText("Temp/Milestone4-survival.result", "PASS " + DateTime.UtcNow.ToString("O"));
            }
            catch (Exception error) { Debug.LogException(error); File.WriteAllText("Temp/Milestone4-survival.result", "FAILED " + error); }
        }
    }
}
