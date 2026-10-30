using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Explicit one-shot local build request; never runs just because the project is opened.
    [InitializeOnLoad]
    internal static class Milestone0BuildRequest
    {
        private const string Request = "Temp/Milestone0-build.request";
        private const string Result = "Temp/Milestone0-build.result";

        static Milestone0BuildRequest()
        {
            if (File.Exists(Request)) EditorApplication.update += RunWhenReady;
        }

        private static void RunWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            EditorApplication.update -= RunWhenReady;
            if (!File.Exists(Request)) return;
            File.Delete(Request); // Consume first: reload cannot silently start another build.
            try
            {
                File.WriteAllText(Result, "RUNNING " + DateTime.UtcNow.ToString("O"));
                Milestone0Setup.Build();
                File.WriteAllText(Result, "SUCCEEDED " + DateTime.UtcNow.ToString("O"));
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                File.WriteAllText(Result, "FAILED " + error);
            }
        }
    }
}
