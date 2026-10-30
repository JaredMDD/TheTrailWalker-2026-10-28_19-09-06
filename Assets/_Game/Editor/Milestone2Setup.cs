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
    public static class Milestone2Setup
    {
        public const string BootstrapPath = "Assets/_Game/Scenes/CollectionBootstrap.unity";
        public const string TrailPath = "Assets/_Game/Scenes/Prototypes/CollectionTrail.unity";
        [MenuItem("The Trail Walker/Milestone 2/1 - Prepare Collection Prototype")]
        public static void Prepare()
        {
            Milestone1Setup.Prepare();
            Create(BootstrapPath, () => new GameObject("Session", typeof(GameBootstrapper)).GetComponent<GameBootstrapper>().Configure("CollectionTrail"));
            Create(TrailPath, () =>
            {
                var prototype = new GameObject("Collection prototype", typeof(MovementPrototype)).GetComponent<MovementPrototype>();
                prototype.Configure(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementTerrain.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementTrail.mat"), AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Materials/MovementAvatar.mat"));
                prototype.EnableCollection();
            });
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootstrapPath, true), new EditorBuildSettingsScene(TrailPath, true) }; AssetDatabase.SaveAssets();
        }
        private static void Create(string path, Action create)
        {
            if (File.Exists(path)) return;
            Scene previous = SceneManager.GetActiveScene(); Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); SceneManager.SetActiveScene(scene);
            try { create(); if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException(path); }
            finally { if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true); }
        }
        [MenuItem("The Trail Walker/Milestone 2/3 - Build Development APK")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("Activate Android first");
            Prepare(); Milestone1Validation.Run(); Milestone2Validation.Run(); Directory.CreateDirectory("Builds/Android");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { BootstrapPath, TrailPath }, target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android, locationPathName = "Builds/Android/TheTrailWalker-Milestone2.apk", options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new Exception("Milestone 2 build failed: " + report.summary.result);
        }
    }
    [InitializeOnLoad] internal static class Milestone2Request
    {
        static Milestone2Request() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            const string request = "Temp/Milestone2.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            string command = File.ReadAllText(request).Trim(); File.Delete(request);
            try
            {
                File.WriteAllText("Temp/Milestone2.result", "RUNNING " + command);
                if (command == "build") Milestone2Setup.Build();
                else if (command == "prepare") { Milestone2Setup.Prepare(); Milestone1Validation.Run(); Milestone2Validation.Run(); }
                else throw new ArgumentException(command);
                File.WriteAllText("Temp/Milestone2.result", "SUCCEEDED " + DateTime.UtcNow.ToString("O"));
            }
            catch (Exception error) { Debug.LogException(error); File.WriteAllText("Temp/Milestone2.result", "FAILED " + error); }
        }
    }
}
