using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    /// <summary>Tier-two loose resources (regions 7-12, November 6, 2026): prefabs and previews from
    /// Tools/Art/generate_tier02_loose_resources.py, built with the tier-one region builder.</summary>
    [InitializeOnLoad]
    public static class Tier02ResourceAssets
    {
        public static readonly string[][] Names =
        {
            new[] { "BirchBranch", "BeaverPelt", "Blackberries" },
            new[] { "FlaxFiber", "CopperOre", "Carrots" },
            new[] { "Barley", "Quartz", "Blueberries" },
            new[] { "Sandstone", "Truffle", "FlaxSeeds" },
            new[] { "Rice", "Tomatoes", "Honey" },
            new[] { "TinOre", "YellowOchre", "CattailFiber" },
        };
        public static string Source(int region) => $"Assets/_Game/Art/Items/Region{region:00}/";
        public static string Target(int region) => $"Assets/_Game/Resources/Region{region:00}/";

        static Tier02ResourceAssets() { EditorApplication.update += EnsureImported; }

        private static void EnsureImported()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= EnsureImported;
            for (int region = 7; region <= 12; region++)
                foreach (string name in Names[region - 7])
                    if (File.Exists(Source(region) + name + ".obj") && !File.Exists(Target(region) + name + ".prefab")) { Build(); return; }
        }

        [MenuItem("The Trail Walker/Art/Rebuild Tier 2 Resource Prefabs")]
        public static void Build()
        {
            for (int region = 7; region <= 12; region++)
                Region01ResourceAssets.BuildRegion(Source(region), Target(region), $"Region{region:00}Palette", $"Region{region:00}", Names[region - 7]);
        }

        [MenuItem("The Trail Walker/Art/Capture Tier 2 Resource Previews")]
        public static void CapturePreviews()
        {
            for (int region = 7; region <= 12; region++)
                Region01ResourceAssets.CaptureRegionPreview(Target(region), Names[region - 7], $"Docs/Art/Region{region:00}/resource-meshes-unity-v1.png");
        }
    }

    internal sealed class Tier02ResourceImport : AssetPostprocessor
    {
        private static bool InTier02(string path)
        {
            for (int region = 7; region <= 12; region++)
                if (path.StartsWith(Tier02ResourceAssets.Source(region), StringComparison.Ordinal)) return true;
            return false;
        }

        private void OnPreprocessModel()
        {
            if (!InTier02(assetPath)) return;
            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.addCollider = false;
            importer.importNormals = ModelImporterNormals.Calculate;
            importer.normalSmoothingAngle = 0;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
        }

        private void OnPreprocessTexture()
        {
            if (!InTier02(assetPath) || !assetPath.EndsWith("Palette.png", StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
        }
    }
}
