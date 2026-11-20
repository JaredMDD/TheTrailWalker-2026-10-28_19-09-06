using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Narrow builder (Milestone 14): only ChickenMeat.prefab and ChickenMeat.mat, same importer/material/prefab setup as Tier02Batch03Assets.
    // The detail type lives in the palette alpha, so no FacetedDetailMaterials entry is needed.
    public static class ChickenMeatAssets
    {
        const string Model = "Assets/_Game/Art/Items/Tier01Design/ShopInputs/ChickenMeat.obj";
        const string Palette = "Assets/_Game/Art/Items/Tier01Design/Palettes/ChickenMeatPalette.png";
        const string MaterialFile = "Assets/_Game/Resources/Tier01/ArtMaterials/ChickenMeat.mat";
        const string PrefabFile = "Assets/_Game/Resources/Tier01/ShopInputs/ChickenMeat.prefab";

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var shader = Shader.Find("TheTrailWalker/FacetedDetail");
            if (shader == null) throw new InvalidOperationException("FacetedDetail shader unavailable.");
            var texture = (TextureImporter)AssetImporter.GetAtPath(Palette);
            texture.alphaSource = TextureImporterAlphaSource.FromInput; texture.alphaIsTransparency = false;
            texture.textureCompression = TextureImporterCompression.Uncompressed; texture.mipmapEnabled = false;
            texture.filterMode = FilterMode.Point; texture.wrapMode = TextureWrapMode.Clamp; texture.SaveAndReimport();
            var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
            importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false;
            importer.importLights = false; importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0;
            importer.SaveAndReimport();
            Directory.CreateDirectory(Path.GetDirectoryName(MaterialFile));
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabFile));
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFile);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, MaterialFile); }
            material.shader = shader; material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Palette));
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_DetailMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Shared/Detail/DetailAtlas.png"));
            material.SetFloat("_DetailScale", 5); material.SetFloat("_DetailStrength", .40f); material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            var root = new GameObject("ChickenMeat");
            try
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
                if (model == null) throw new InvalidOperationException("Missing model " + Model);
                var visual = UnityEngine.Object.Instantiate(model, root.transform, false); visual.name = "Visual";
                foreach (var renderer in root.GetComponentsInChildren<Renderer>())
                {
                    var slots = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < slots.Length; i++) slots[i] = material;
                    renderer.sharedMaterials = slots;
                }
                PrefabUtility.SaveAsPrefabAsset(root, PrefabFile);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();
        }
    }
}
