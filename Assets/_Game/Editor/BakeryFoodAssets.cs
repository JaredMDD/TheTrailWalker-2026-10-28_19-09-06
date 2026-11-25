using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Narrow builder (Milestone 14 slice 2): only the five bakery food prefabs and their one shared material, same
    // importer/material/prefab setup as ChickenMeatAssets. Sources come from Tools/Art/generate_bakery_foods.py.
    // Slice 3 (Ruling 3) adds BuildRanchDishes: only the two tier 1 ranch dishes and their own material, from
    // Tools/Art/generate_ranch_dishes.py; each entry point leaves the other's prefabs and material untouched.
    public static class BakeryFoodAssets
    {
        const string Source = "Assets/_Game/Art/Items/Bakery/";
        const string Palette = Source + "BakeryFoodsPalette.png";
        const string MaterialFile = "Assets/_Game/Resources/Tier01/ArtMaterials/BakeryFoods.mat";
        const string RanchSource = "Assets/_Game/Art/Items/Ranch/";
        static readonly (string model, string prefab)[] RanchDishes = {
            ("ChickenMushroomPie", "Assets/_Game/Resources/Tier01/Crafted/ChickenMushroomPie.prefab"),
            ("AppleGlazedChicken", "Assets/_Game/Resources/Tier01/Crafted/AppleGlazedChicken.prefab"),
        };
        static readonly (string model, string prefab)[] Foods = {
            ("WheatBread", "Assets/_Game/Resources/Tier01/Crafted/WheatBread.prefab"),
            ("PineNutOatCookies", "Assets/_Game/Resources/Tier01/Crafted/PineNutOatCookies.prefab"),
            ("StrawberryCreamRoll", "Assets/_Game/Resources/Tier01/Crafted/StrawberryCreamRoll.prefab"),
            ("BlueberryMuffins", "Assets/_Game/Resources/Tier02/Food/BlueberryMuffins.prefab"),
            ("FlaxOilBiscuits", "Assets/_Game/Resources/Tier02/Food/FlaxOilBiscuits.prefab"),
        };

        public static void Build() => Build(Source, Palette, MaterialFile, Foods);
        public static void BuildRanchDishes() => Build(RanchSource, RanchSource + "RanchDishesPalette.png", "Assets/_Game/Resources/Tier01/ArtMaterials/RanchDishes.mat", RanchDishes);

        static void Build(string source, string palette, string materialFile, (string model, string prefab)[] foods)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var shader = Shader.Find("TheTrailWalker/FacetedDetail");
            if (shader == null) throw new InvalidOperationException("FacetedDetail shader unavailable.");
            var texture = (TextureImporter)AssetImporter.GetAtPath(palette);
            if (texture == null) throw new InvalidOperationException("Missing palette " + palette);
            texture.alphaSource = TextureImporterAlphaSource.FromInput; texture.alphaIsTransparency = false;
            texture.textureCompression = TextureImporterCompression.Uncompressed; texture.mipmapEnabled = false;
            texture.filterMode = FilterMode.Point; texture.wrapMode = TextureWrapMode.Clamp; texture.SaveAndReimport();
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialFile);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, materialFile); }
            material.shader = shader; material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(palette));
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_DetailMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Shared/Detail/DetailAtlas.png"));
            material.SetFloat("_DetailScale", 5); material.SetFloat("_DetailStrength", .40f); material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            foreach (var (name, prefabFile) in foods)
            {
                string model = source + name + ".obj";
                var importer = (ModelImporter)AssetImporter.GetAtPath(model);
                if (importer == null) throw new InvalidOperationException("Missing model " + model);
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false;
                importer.importLights = false; importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0;
                importer.SaveAndReimport();
                Directory.CreateDirectory(Path.GetDirectoryName(prefabFile));
                var root = new GameObject(name);
                try
                {
                    var visual = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(model), root.transform, false); visual.name = "Visual";
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>())
                    {
                        var slots = new Material[renderer.sharedMaterials.Length];
                        for (int i = 0; i < slots.Length; i++) slots[i] = material;
                        renderer.sharedMaterials = slots;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, prefabFile);
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssetIfDirty(material);   // only this material: never flush other dirty assets
        }
    }
}
