using System;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Crafting UI sprites (recipe-level stars 1-10, Tools/Art/generate_level_stars.py): same import as the quality stars.
    internal sealed class CraftingUiImport : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            bool missions = assetPath.StartsWith("Assets/_Game/Resources/UI/Missions/", StringComparison.Ordinal) // worn papers and tabs (Milestone 8)
                || assetPath.StartsWith("Assets/_Game/Resources/UI/Hud/", StringComparison.Ordinal) // wooden speed ruler tile
                || assetPath.StartsWith("Assets/_Game/Resources/UI/Map/", StringComparison.Ordinal) // worn region maps
                || assetPath.StartsWith("Assets/_Game/Resources/UI/Furniture/", StringComparison.Ordinal); // furniture store icons (art batch A, November 7, 2026)
            if ((!assetPath.StartsWith("Assets/_Game/Resources/UI/Crafting/", StringComparison.Ordinal) && !missions) || !assetPath.EndsWith(".png", StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.spritePixelsPerUnit = 100;
            importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = missions ? 1024 : 256;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        }
    }
}
