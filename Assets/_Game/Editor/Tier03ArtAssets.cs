using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Tier 3 settlement integration, slice A: palette materials and display prefabs for the staged tier 3 item models
    // (Tools/Art/stage_tier03_slice_a.py writes the manifest). Passive art only: no catalogs, rules or saves change.
    public static class Tier03ArtAssets
    {
        public const string ManifestPath = "Assets/_Game/Art/Tier03/Tier03SliceAManifest.json";
        public const string MaterialPath = "Assets/_Game/Resources/Tier03/ArtMaterials/";
        public const string DetailAtlas = "Assets/_Game/Art/Shared/Detail/DetailAtlas.png";

        [Serializable] public sealed class Item
        {
            public string Name, Model, SourcePath, PalettePath, PrefabPath, OriginSource, Packet, SourceSHA256, PaletteSHA256;
            public string[] ItemIds;
            public int Triangles;
            public bool Readable;
        }
        [Serializable] public sealed class Manifest { public Item[] Items; }
        public static Manifest Load() => JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
        public static string PrefabAssetPath(Item row) => "Assets/_Game/Resources/" + row.PrefabPath + ".prefab";
        public static string MaterialAssetPath(Item row) => MaterialPath + row.Model + ".mat";

        [MenuItem("The Trail Walker/Tier 3/Build slice A item prefabs")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh(); Directory.CreateDirectory(MaterialPath);
            var shader = Shader.Find("TheTrailWalker/FacetedDetail");
            if (shader == null) throw new InvalidOperationException("FacetedDetail shader unavailable.");
            var detail = AssetDatabase.LoadAssetAtPath<Texture2D>(DetailAtlas);
            foreach (Item row in Load().Items)
            {
                var texture = (TextureImporter)AssetImporter.GetAtPath(row.PalettePath);
                if (texture == null) throw new InvalidOperationException("Missing palette " + row.PalettePath);
                texture.alphaSource = TextureImporterAlphaSource.FromInput; texture.alphaIsTransparency = false;
                texture.textureCompression = TextureImporterCompression.Uncompressed; texture.mipmapEnabled = false;
                texture.filterMode = FilterMode.Point; texture.wrapMode = TextureWrapMode.Clamp; texture.SaveAndReimport();
                var importer = (ModelImporter)AssetImporter.GetAtPath(row.SourcePath);
                if (importer == null) throw new InvalidOperationException("Missing model " + row.SourcePath);
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false;
                importer.importLights = false; importer.addCollider = false; importer.isReadable = row.Readable;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0;
                importer.SaveAndReimport();

                string materialFile = MaterialAssetPath(row);
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialFile);
                if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, materialFile); }
                material.shader = shader; material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(row.PalettePath));
                material.SetColor("_BaseColor", Color.white); material.SetTexture("_DetailMap", detail);
                material.SetFloat("_DetailScale", 5); material.SetFloat("_DetailStrength", .40f); material.enableInstancing = true;
                EditorUtility.SetDirty(material);

                string prefabFile = PrefabAssetPath(row);
                Directory.CreateDirectory(Path.GetDirectoryName(prefabFile));
                var root = new GameObject(row.Model);
                try
                {
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(row.SourcePath);
                    if (model == null) throw new InvalidOperationException("Missing model " + row.SourcePath);
                    var visual = UnityEngine.Object.Instantiate(model, root.transform, false); visual.name = "Visual";
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
            AssetDatabase.SaveAssets();
            Debug.Log("Tier 3 slice A: built " + Load().Items.Length + " item prefabs.");
        }
    }
}
