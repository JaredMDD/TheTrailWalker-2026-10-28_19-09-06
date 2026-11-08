using System.IO;
using UnityEditor;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Region02ResourceAssets
    {
        private const string Source = "Assets/_Game/Art/Items/Region02/";
        private const string Target = "Assets/_Game/Resources/Region02/";
        private static readonly string[] Names = { "HempFiber", "Flint", "PineCone" };
        static Region02ResourceAssets() { EditorApplication.update += EnsureImported; }

        private static void EnsureImported()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= EnsureImported;
            foreach (string name in Names)
                if (!File.Exists(Target + name + ".prefab")) { Build(); return; }
        }

        [MenuItem("The Trail Walker/Art/Rebuild Region 2 Resource Prefabs")]
        public static void Build() => Region01ResourceAssets.BuildRegion(Source, Target, "Region02Palette", "Region02", Names);

        [MenuItem("The Trail Walker/Art/Capture Region 2 Resource Preview")]
        public static void CapturePreview() => Region01ResourceAssets.CaptureRegionPreview(Target, Names, "Docs/Art/Region02/resource-meshes-unity-v1.png");
    }
}
