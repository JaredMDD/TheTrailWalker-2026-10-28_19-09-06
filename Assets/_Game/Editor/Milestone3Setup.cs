using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace TheTrailWalker.Editor
{
    public static class Milestone3Setup
    {
        [MenuItem("The Trail Walker/Milestone 3/1 - Prepare and Validate")]
        public static void Prepare() { Milestone2Setup.Prepare(); Milestone1Validation.Run(); Milestone2Validation.Run(); Milestone3Validation.Run(); }
        [MenuItem("The Trail Walker/Milestone 3/3 - Build Development APK")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new InvalidOperationException("Activate Android first.");
            Prepare(); Directory.CreateDirectory("Builds/Android");
            BuildReport result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { Milestone2Setup.BootstrapPath, Milestone2Setup.TrailPath }, target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android, locationPathName = "Builds/Android/TheTrailWalker-Milestone3.apk", options = BuildOptions.Development });
            if (result.summary.result != BuildResult.Succeeded) throw new Exception("Milestone 3 build failed: " + result.summary.result);
        }
    }
    [InitializeOnLoad] internal static class Milestone3Request
    {
        static Milestone3Request() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            const string request = "Temp/Milestone3.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
            string command = File.ReadAllText(request).Trim(); File.Delete(request);
            try
            {
                File.WriteAllText("Temp/Milestone3.result", "RUNNING " + command);
                if (command == "build") Milestone3Setup.Build(); else if (command == "prepare") Milestone3Setup.Prepare(); else throw new ArgumentException(command);
                File.WriteAllText("Temp/Milestone3.result", "SUCCEEDED " + DateTime.UtcNow.ToString("O"));
            }
            catch (Exception error) { UnityEngine.Debug.LogException(error); File.WriteAllText("Temp/Milestone3.result", "FAILED " + error); }
        }
    }
}
