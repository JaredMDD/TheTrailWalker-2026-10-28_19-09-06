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
    public static class Milestone1Setup
    {
        public const string BootstrapPath = "Assets/_Game/Scenes/Bootstrap.unity";
        public const string TrailPath = "Assets/_Game/Scenes/Prototypes/MovementTrail.unity";

        [MenuItem("The Trail Walker/Milestone 1/1 - Prepare Movement Prototype")]
        public static void Prepare()
        {
            Milestone0Setup.Prepare();
            var terrain = Material("MovementTerrain", new Color(0.21f, 0.36f, 0.26f));
            var trail = Material("MovementTrail", new Color(0.59f, 0.43f, 0.26f));
            var avatar = Material("MovementAvatar", new Color(0.95f, 0.66f, 0.22f));
            CreateScene(BootstrapPath, () => new GameObject("Session", typeof(GameBootstrapper)));
            CreateScene(TrailPath, () =>
            {
                var root = new GameObject("Movement prototype", typeof(MovementPrototype));
                root.GetComponent<MovementPrototype>().Configure(terrain, trail, avatar);
                return root;
            });
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootstrapPath, true), new EditorBuildSettingsScene(TrailPath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[Milestone1] Prepared. Open Bootstrap.unity and press Play.");
        }
        private static Material Material(string name, Color color)
        {
            string path = "Assets/_Game/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path); return material;
        }
        private static void CreateScene(string path, Func<GameObject> create)
        {
            if (File.Exists(path)) return;
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                create();
                if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("Could not save " + path);
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }
        [MenuItem("The Trail Walker/Milestone 1/3 - Build Development APK")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("Activate the Android Build Profile first.");
            Prepare();
            Milestone1Validation.Run();
            Directory.CreateDirectory("Builds/Android");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { BootstrapPath, TrailPath }, target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android,
                locationPathName = "Builds/Android/TheTrailWalker-Milestone1.apk", options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Milestone 1 build: " + report.summary.result);
            Debug.Log("[Milestone1] APK ready: " + report.summary.outputPath);
        }
    }

    // Like milestone 0: explicit one-shot requests only, consumed before executing.
    [InitializeOnLoad]
    internal static class Milestone1Request
    {
        private const string Request = "Temp/Milestone1.request";
        static Milestone1Request() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            string command = File.ReadAllText(Request).Trim(); File.Delete(Request);
            try
            {
                File.WriteAllText("Temp/Milestone1.result", "RUNNING " + command);
                if (command == "build") Milestone1Setup.Build();
                else if (command == "prepare") { Milestone1Setup.Prepare(); Milestone1Validation.Run(); }
                else throw new ArgumentException("Unknown milestone 1 request: " + command);
                File.WriteAllText("Temp/Milestone1.result", "SUCCEEDED " + DateTime.UtcNow.ToString("O"));
            }
            catch (Exception error) { Debug.LogException(error); File.WriteAllText("Temp/Milestone1.result", "FAILED " + error); }
        }
    }
}
