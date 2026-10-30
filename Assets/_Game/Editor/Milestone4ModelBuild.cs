using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTrailWalker.Editor
{
    public static class Milestone4ModelBuild
    {
        private const string ScenePath = "Assets/_Game/Scenes/Prototypes/Milestone4ModelCheck.unity";
        [MenuItem("The Trail Walker/Milestone 4/Build Model Test APK")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("Activate the Android profile first.");
            int passed = TheTrailWalker.Domain.Equipment.EquipmentModelChecks.Run();
            if (!File.Exists(ScenePath))
            {
                Scene old = SceneManager.GetActiveScene();
                Scene test = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); SceneManager.SetActiveScene(test);
                try
                {
                    new GameObject("Milestone 4 model check", typeof(Milestone4ModelProbe));
                    if (!EditorSceneManager.SaveScene(test, ScenePath)) throw new IOException("Could not save model test scene.");
                }
                finally { if (old.IsValid() && old.isLoaded) SceneManager.SetActiveScene(old); EditorSceneManager.CloseScene(test, true); }
            }
            Directory.CreateDirectory("Builds/Android");
            string originalId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            try
            {
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.thetrailwalker.milestone4.modeltest");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android, locationPathName = "Builds/Android/TheTrailWalker-Milestone4-ModelTest.apk", options = BuildOptions.Development });
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Model test APK failed: " + report.summary.result);
            }
            finally { PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, originalId); AssetDatabase.SaveAssets(); }
            Debug.Log($"[Milestone4] Model test APK built; {passed} checks passed.");
        }
    }
    [InitializeOnLoad] internal static class Milestone4ModelRequest
    {
        static Milestone4ModelRequest() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            const string request = "Temp/Milestone4-model-build.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            File.Delete(request);
            try { File.WriteAllText("Temp/Milestone4-model-build.result", "RUNNING"); Milestone4ModelBuild.Build(); File.WriteAllText("Temp/Milestone4-model-build.result", "PASS"); }
            catch (Exception error) { Debug.LogException(error); File.WriteAllText("Temp/Milestone4-model-build.result", "FAILED " + error); }
        }
    }
}
