using System.IO;
using UnityEditor;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Region03ResourceAssets
    {
        private const string Source = "Assets/_Game/Art/Items/Region03/";
        private const string Target = "Assets/_Game/Resources/Region03/";
        private static readonly string[] Names = { "NettleFiber", "Clay", "RockSalt" };
        static Region03ResourceAssets() { EditorApplication.update += EnsureImported; }

        private static void EnsureImported()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= EnsureImported;
            foreach (string name in Names)
                if (!File.Exists(Target + name + ".prefab")) { Build(); return; }
        }

        [MenuItem("The Trail Walker/Art/Rebuild Region 3 Resource Prefabs")]
        public static void Build() => Region01ResourceAssets.BuildRegion(Source, Target, "Region03Palette", "Region03", Names);

        [MenuItem("The Trail Walker/Art/Capture Region 3 Resource Preview")]
        public static void CapturePreview() => Region01ResourceAssets.CaptureRegionPreview(Target, Names, "Docs/Art/Region03/resource-meshes-unity-v1.png");
    }
}
