using System.IO;
using UnityEditor;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Region04ResourceAssets
    {
        private const string Source = "Assets/_Game/Art/Items/Region04/";
        private const string Target = "Assets/_Game/Resources/Region04/";
        private static readonly string[] Names = { "Limestone", "Strawberry", "TeaHerbs" };
        static Region04ResourceAssets() { EditorApplication.update += EnsureImported; }

        private static void EnsureImported()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= EnsureImported;
            foreach (string name in Names)
                if (!File.Exists(Target + name + ".prefab")) { Build(); return; }
        }

        [MenuItem("The Trail Walker/Art/Rebuild Region 4 Resource Prefabs")]
        public static void Build()
        {
            Region01ResourceAssets.BuildRegion(Source, Target, "Region04Palette", "Region04", Names);
            Region01ResourceAssets.BuildRegion("Assets/_Game/Art/Crafting/Tier01/", "Assets/_Game/Resources/Tier01/Crafted/",
                "Tier01CraftedPalette", "Region04Mushrooms", new[] { "Mushrooms" });
        }

        [MenuItem("The Trail Walker/Art/Capture Region 4 Resource Preview")]
        public static void CapturePreview() => Region01ResourceAssets.CaptureRegionPreview(Target,
            new[] { "Limestone", "Strawberry", "Mushrooms" }, "Docs/Art/Region04/resource-meshes-unity-v1.png",
            new[] { Target + "Limestone.prefab", Target + "Strawberry.prefab", "Assets/_Game/Resources/Tier01/Crafted/Mushrooms.prefab" });
    }
}
