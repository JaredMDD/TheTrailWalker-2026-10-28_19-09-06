using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Surface detail (user request, November 2, 2026): switches palette-strip materials to TheTrailWalker/FacetedDetail.
    // The palette alpha (written by Tools/Art/detail_textures.py and the palette writers) chooses each colour's
    // detail tile; colours without a type stay flat, so switching a material never changes untyped colours.
    public static class FacetedDetailMaterials
    {
        public const string ShaderName = "TheTrailWalker/FacetedDetail";
        const string Atlas = "Assets/_Game/Art/Shared/Detail/DetailAtlas.png";
        // Palette textures that carry detail types.
        static readonly string[] Palettes = {
            "Assets/_Game/Art/Settlement/Houses/HousePalette.png",
            "Assets/_Game/Art/Items/Region01/Region01Palette.png", "Assets/_Game/Art/Items/Region02/Region02Palette.png",
            "Assets/_Game/Art/Items/Region03/Region03Palette.png", "Assets/_Game/Art/Items/Region04/Region04Palette.png",
            "Assets/_Game/Art/Items/Region05/Region05Palette.png", "Assets/_Game/Art/Items/Region06/Region06Palette.png",
            "Assets/_Game/Art/Items/Region07/Region07Palette.png", "Assets/_Game/Art/Items/Region08/Region08Palette.png",
            "Assets/_Game/Art/Items/Region09/Region09Palette.png", "Assets/_Game/Art/Items/Region10/Region10Palette.png",
            "Assets/_Game/Art/Items/Region11/Region11Palette.png", "Assets/_Game/Art/Items/Region12/Region12Palette.png",
            "Assets/_Game/Art/Items/Region04/MushroomsPalette.png",
            "Assets/_Game/Art/Harvest/Tier01/Tier01Palette.png", "Assets/_Game/Art/Harvest/Tier01/RabbitPalette.png",
            "Assets/_Game/Art/Harvest/Massive/MassiveTreesPalette.png",   // massive lumber-field trees (November 8, 2026)
            // Item surface detail, November 7, 2026: balloon special gear and mystery boxes (tiers 2-4; tier one uses the merchant palettes).
            "Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsTier01Palette.png", "Assets/_Game/Art/Items/BalloonGoods/MysteryBoxTier01Palette.png", "Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsTier02Palette.png", "Assets/_Game/Art/Items/BalloonGoods/MysteryBoxTier02Palette.png", "Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsTier03Palette.png", "Assets/_Game/Art/Items/BalloonGoods/MysteryBoxTier03Palette.png", "Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsTier04Palette.png", "Assets/_Game/Art/Items/BalloonGoods/MysteryBoxTier04Palette.png",
            "Assets/_Game/Art/Environment/CampProps/CampPropsPalette.png", "Assets/_Game/Art/Environment/CampProps/Tier02/Tier02CampPalette.png",
            "Assets/_Game/Art/Environment/Region01/DawnPlainsPalette.png",
            "Assets/_Game/Art/Environment/Tier01/Region01/EnvironmentPalette.png", "Assets/_Game/Art/Environment/Tier01/Region02/EnvironmentPalette.png",
            "Assets/_Game/Art/Environment/Tier01/Region03/EnvironmentPalette.png", "Assets/_Game/Art/Environment/Tier01/Region04/EnvironmentPalette.png",
            "Assets/_Game/Art/Environment/Tier01/Region05/EnvironmentPalette.png", "Assets/_Game/Art/Environment/Tier01/Region06/EnvironmentPalette.png",
            "Assets/_Game/Art/Environment/Tier02/Region07/EnvironmentPalette.png", "Assets/_Game/Art/Environment/Tier02/Region08/EnvironmentPalette.png",
            "Assets/_Game/Art/Environment/Tier02/Region09/EnvironmentPalette.png", "Assets/_Game/Art/Environment/Tier02/Region10/EnvironmentPalette.png",
            "Assets/_Game/Art/Environment/Tier02/Region11/EnvironmentPalette.png", "Assets/_Game/Art/Environment/Tier02/Region12/EnvironmentPalette.png",
            "Assets/_Game/Art/Characters/CampCommon/Tier00RagsPalette.png", "Assets/_Game/Art/Characters/CampCommon/Tier01HempRabbitPalette.png",
            "Assets/_Game/Art/Characters/CampCommon/Tier02FlaxBeaverPalette.png", "Assets/_Game/Art/Characters/CampCommon/Tier03CottonFeatherPalette.png",
            "Assets/_Game/Art/Characters/CampCommon/Tier04CottonBoarPalette.png", "Assets/_Game/Art/Characters/CampSpecial/CampSpecialPalette.png",
            "Assets/_Game/Art/Settlement/Storefronts/StorefrontPalette.png", "Assets/_Game/Art/Characters/Shopkeepers/ShopkeeperPalette.png",
            "Assets/_Game/Art/Settlement/Route/SettlementRoutePalette.png" };   // settlement approach arch, palisade, fence (November 8, 2026)

        internal static bool IsDetailPalettePath(string path) => Array.IndexOf(Palettes, path) >= 0
            || (path != null && path.StartsWith("Assets/_Game/Art/Items/Tier01Design/Palettes/", StringComparison.Ordinal));
        public static bool IsDetailPalette(Texture texture) => texture != null && IsDetailPalettePath(AssetDatabase.GetAssetPath(texture));

        public static void Configure(Material material, Texture palette)
        {
            if (!IsDetailPalette(palette)) return;
            var shader = Shader.Find(ShaderName);
            if (shader == null) throw new InvalidOperationException("Shader " + ShaderName + " not found.");
            bool switched = material.shader != shader;
            var tint = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.white;
            material.shader = shader;
            material.SetTexture("_BaseMap", palette); material.SetColor("_BaseColor", tint);
            material.SetTexture("_DetailMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Atlas));
            bool item = AssetDatabase.GetAssetPath(palette).StartsWith("Assets/_Game/Art/Items/", StringComparison.Ordinal);
            float scale = material.GetFloat("_DetailScale");
            if (switched || scale <= 0 || (item && Mathf.Approximately(scale, 1))) material.SetFloat("_DetailScale", item ? 5 : 1);
            if (switched) material.SetFloat("_DetailStrength", item ? .55f : .75f);
            material.enableInstancing = true; EditorUtility.SetDirty(material);
        }

        [MenuItem("The Trail Walker/Art/Apply Surface Detail")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run outside Play mode.");
            AssetDatabase.Refresh();
            var atlasImporter = (TextureImporter)AssetImporter.GetAtPath(Atlas);
            atlasImporter.sRGBTexture = false; atlasImporter.mipmapEnabled = true; atlasImporter.wrapMode = TextureWrapMode.Repeat;
            atlasImporter.filterMode = FilterMode.Bilinear; atlasImporter.textureCompression = TextureImporterCompression.Uncompressed; atlasImporter.SaveAndReimport();
            foreach (var path in Palettes)
            {
                if (!(AssetImporter.GetAtPath(path) is TextureImporter t)) continue;
                // Alpha holds the detail type: keep it exact (no compression, no transparency processing).
                t.alphaSource = TextureImporterAlphaSource.FromInput; t.alphaIsTransparency = false;
                t.textureCompression = TextureImporterCompression.Uncompressed; t.filterMode = FilterMode.Point; t.mipmapEnabled = false; t.SaveAndReimport();
            }
            var shader = Shader.Find(ShaderName);
            if (shader == null) throw new InvalidOperationException("Shader " + ShaderName + " not found.");
            int switched = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/_Game" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (material == null || !material.HasProperty("_BaseMap") || !IsDetailPalette(material.GetTexture("_BaseMap"))) continue;
                var palette = material.GetTexture("_BaseMap");
                if (material.shader != shader) switched++;
                Configure(material, palette);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Surface detail: " + switched + " materials switched to " + ShaderName + ".");
        }
    }

    // Preserve the encoding on every reimport, including newly generated item palettes.
    internal sealed class FacetedDetailTextureImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            if (FacetedDetailMaterials.IsDetailPalettePath(assetPath))
            {
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            }
            if (assetPath == "Assets/_Game/Art/Shared/Detail/DetailAtlas.png")
            {
                importer.sRGBTexture = false; importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Repeat; importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
                importer.npotScale = TextureImporterNPOTScale.None;
            }
        }
    }
}
