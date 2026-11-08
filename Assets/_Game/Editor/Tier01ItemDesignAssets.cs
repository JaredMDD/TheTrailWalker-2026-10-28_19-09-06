using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Targeted visual-only build: no recipe regeneration, gameplay, scene edits or tests.
    public static class Tier01ItemDesignAssets
    {
        const string ManifestPath = "Assets/_Game/Art/Items/Tier01Design/Tier01ItemDesignManifest.json";
        const string Materials = "Assets/_Game/Resources/Tier01/ItemDesignMaterials/";
        const string Docs = "Docs/Art/Tier01/ItemDesign/";
        [Serializable] sealed class Item
        {
            public string ItemId, Name, Model, Key, Category, SourcePath, PrefabPath, RecipeId, PalettePath, PaletteFamily;
            public int Triangles;
            public float[] BoundsMin, BoundsMax;
        }
        [Serializable] sealed class Profile { public string Name, Path; public float DetailScale, DetailStrength; }
        [Serializable] sealed class Manifest { public Item[] Items; public Profile[] PaletteFamilies; }
        static DateTime cacheTime;
        static Dictionary<string, string> materialPaths;
        static Manifest Load() => JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));

        // Existing prefab generators call this to retain the semantic material on later rebuilds.
        public static Material ExistingMaterial(string sourcePath, string model)
        {
            if (!File.Exists(ManifestPath)) return null;
            DateTime stamp = File.GetLastWriteTimeUtc(ManifestPath);
            if (materialPaths == null || stamp != cacheTime)
            {
                materialPaths = new Dictionary<string, string>(); cacheTime = stamp;
                foreach (var row in Load().Items) materialPaths[row.SourcePath] = Materials + row.PaletteFamily + ".mat";
            }
            return materialPaths.TryGetValue(sourcePath + model + ".obj", out string path)
                ? AssetDatabase.LoadAssetAtPath<Material>(path) : null;
        }

        [MenuItem("The Trail Walker/Art/Build Tier 1 Item Designs")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh(); Directory.CreateDirectory(Materials);
            var data = Load();
            foreach (var profile in data.PaletteFamilies)
            {
                var texture = (TextureImporter)AssetImporter.GetAtPath(profile.Path);
                texture.sRGBTexture = true; texture.alphaSource = TextureImporterAlphaSource.FromInput;
                texture.alphaIsTransparency = false; texture.filterMode = FilterMode.Point;
                texture.mipmapEnabled = false; texture.wrapMode = TextureWrapMode.Clamp;
                texture.npotScale = TextureImporterNPOTScale.None;
                texture.textureCompression = TextureImporterCompression.Uncompressed; texture.SaveAndReimport();
                string path = Materials + profile.Name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path); bool created = material == null;
                if (created)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    AssetDatabase.CreateAsset(material, path);
                }
                FacetedDetailMaterials.Configure(material, AssetDatabase.LoadAssetAtPath<Texture2D>(profile.Path));
                if (created)
                {
                    material.SetFloat("_DetailScale", profile.DetailScale);
                    material.SetFloat("_DetailStrength", profile.DetailStrength);
                }
                EditorUtility.SetDirty(material);
            }
            foreach (var row in data.Items)
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(row.SourcePath);
                importer.globalScale = 1; importer.importAnimation = false;
                importer.importCameras = false; importer.importLights = false; importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0;
                importer.SaveAndReimport();
                string prefabPath = "Assets/_Game/Resources/" + row.PrefabPath + ".prefab";
                bool exists = File.Exists(prefabPath);
                var root = exists ? PrefabUtility.LoadPrefabContents(prefabPath) : new GameObject(row.Model);
                try
                {
                    if (!exists)
                    {
                        var model = AssetDatabase.LoadAssetAtPath<GameObject>(row.SourcePath);
                        var visual = UnityEngine.Object.Instantiate(model, root.transform, false); visual.name = "Visual";
                    }
                    var material = AssetDatabase.LoadAssetAtPath<Material>(Materials + row.PaletteFamily + ".mat");
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    {
                        var slots = new Material[renderer.sharedMaterials.Length];
                        for (int i = 0; i < slots.Length; i++) slots[i] = material;
                        renderer.sharedMaterials = slots;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                finally
                {
                    if (exists) PrefabUtility.UnloadPrefabContents(root); else UnityEngine.Object.DestroyImmediate(root);
                }
            }
            AssetDatabase.SaveAssets(); Directory.CreateDirectory("Temp");
            File.WriteAllText("Temp/Tier01ItemDesign-build.result", "SUCCEEDED: " + data.Items.Length + " stable-path item prefabs, " + data.PaletteFamilies.Length + " shared semantic materials. " + DateTime.UtcNow.ToString("O"));
            Debug.Log("Tier-one item designs built. Recipe data and stats were not regenerated. Gameplay/device tests pending.");
        }

        [MenuItem("The Trail Walker/Art/Capture Tier 1 Item Designs")]
        public static void Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Capture outside Play mode.");
            Directory.CreateDirectory(Docs);
            foreach (var row in Load().Items)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/" + row.PrefabPath + ".prefab");
                Render(prefab, row.Model + "-front.png", false, row.ItemId == "item.pine_plank");
                Render(prefab, row.Model + "-study.png", true, false);
            }
            File.WriteAllText("Temp/Tier01ItemDesign-capture.result", "SUCCEEDED: front and three-quarter actual-mesh art renders. " + DateTime.UtcNow.ToString("O"));
        }
        static void Render(GameObject prefab, string file, bool angled, bool plank)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                var view = UnityEngine.Object.Instantiate(prefab);
                if (plank) view.transform.rotation = Quaternion.Euler(-62, 0, 0);
                preview.AddSingleGO(view);
                var renderers = view.GetComponentsInChildren<Renderer>(); Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                var cam = preview.camera; cam.orthographic = true;
                cam.transform.position = bounds.center + (angled ? new Vector3(2.3f, 1.3f, -4) : new Vector3(0, 0, -4));
                cam.transform.LookAt(bounds.center);
                // Frame projected bounds, preserving the mesh's proportions in every view.
                float maxX = 0, maxY = 0;
                for (int i = 0; i < 8; i++)
                {
                    var p = bounds.center + Vector3.Scale(bounds.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var q = cam.transform.InverseTransformPoint(p); maxX = Mathf.Max(maxX, Mathf.Abs(q.x)); maxY = Mathf.Max(maxY, Mathf.Abs(q.y));
                }
                cam.orthographicSize = Mathf.Max(.06f, Mathf.Max(maxX, maxY) * 1.18f);
                cam.nearClipPlane = .01f; cam.farClipPlane = 30;
                cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.20f, .23f, .24f);
                preview.lights[0].intensity = 1.2f; preview.lights[0].transform.rotation = Quaternion.Euler(35, 0, 0);
                preview.lights[1].intensity = .6f; preview.ambientColor = new Color(.55f, .55f, .55f);
                preview.BeginStaticPreview(new Rect(0, 0, 800, 800)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + file, image.EncodeToPNG()); }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
