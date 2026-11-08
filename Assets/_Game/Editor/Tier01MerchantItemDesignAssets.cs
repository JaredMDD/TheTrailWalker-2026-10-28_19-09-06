using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Surface assignments only. The existing special worn geometry is NOT a faceted-body refit.
    public static class Tier01MerchantItemDesignAssets
    {
        const string ManifestPath = "Assets/_Game/Art/Items/Tier01Design/MerchantItemDesignManifest.json";
        const string Materials = "Assets/_Game/Resources/Tier01/MerchantDesignMaterials/";
        const string Docs = "Docs/Art/Tier01/MerchantItemDesign/";
        const string FitStatus = "Ordinary T1 hats/pack fits are handled elsewhere. Special worn geometry was not refitted; old special-fit geometry remains for a later refit.";
        [Serializable] sealed class Item
        {
            public string Id, Name, Model, Role, PrefabPath, PaletteFamily;
            public string[] SourcePaths, SourceSHA256;
        }
        [Serializable] sealed class Profile
        {
            public string Name, Path, MaterialPath;
            public float DetailScale, DetailStrength;
        }
        [Serializable] sealed class Manifest { public Item[] Items; public Profile[] PaletteFamilies; }
        [Serializable] sealed class CaptureRow
        {
            public string Id, Name, PrefabPath, MaterialPath, FrontImage, StudyImage, TextLabel;
        }
        [Serializable] sealed class CaptureManifest
        {
            public string Scope = "Actual existing mesh art renders; five T1 displays and the closed T1 box. No avatar-fit evidence.";
            public string FitStatus;
            public CaptureRow[] Items;
        }

        static Manifest Load()
        {
            var data = JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));
            if (data == null || data.Items == null || data.Items.Length != 16 || data.PaletteFamilies == null || data.PaletteFamilies.Length != 6)
                throw new InvalidOperationException("Generate the isolated merchant manifest first.");
            var paths = new HashSet<string>();
            var expected = new HashSet<string>();
            foreach (string slot in new[] { "Hat", "Boots", "Shirt", "Pants", "Pack" })
            {
                string stem = "Assets/_Game/Resources/BalloonGoods/";
                expected.Add(stem + "Items/SpecialT01" + slot + "Item.prefab");
                foreach (string sex in new[] { "Male", "Female" })
                    expected.Add(stem + "Wearables/" + sex + "/SpecialT01" + slot + sex + "Worn.prefab");
            }
            expected.Add("Assets/_Game/Resources/BalloonGoods/MysteryBoxes/MysteryBoxTier01.prefab");
            foreach (var row in data.Items)
                if (!expected.Contains(row.PrefabPath) || !paths.Add(row.PrefabPath) || !File.Exists(row.PrefabPath))
                    throw new InvalidOperationException("Unexpected, duplicate or missing merchant prefab: " + row.PrefabPath);
            foreach (var profile in data.PaletteFamilies)
                if (profile.MaterialPath != Materials + profile.Name + ".mat" || !profile.Name.StartsWith("Merchant", StringComparison.Ordinal)
                    || profile.Path != "Assets/_Game/Art/Items/Tier01Design/Palettes/" + profile.Name + "Palette.png")
                    throw new InvalidOperationException("Merchant material/palette must stay in its isolated namespace.");
            return data;
        }

        [MenuItem("The Trail Walker/Art/Build Tier 1 Merchant Item Surfaces")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var data = Load();
            Directory.CreateDirectory(Materials); AssetDatabase.Refresh();
            var materials = new Dictionary<string, Material>();
            foreach (var profile in data.PaletteFamilies)
            {
                var importer = AssetImporter.GetAtPath(profile.Path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("Missing palette " + profile.Path);
                importer.sRGBTexture = true; importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = false; importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp; importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
                var material = AssetDatabase.LoadAssetAtPath<Material>(profile.MaterialPath);
                if (material == null)
                {
                    var shader = Shader.Find(FacetedDetailMaterials.ShaderName);
                    if (shader == null) throw new InvalidOperationException("Missing faceted detail shader.");
                    material = new Material(shader); AssetDatabase.CreateAsset(material, profile.MaterialPath);
                }
                var palette = AssetDatabase.LoadAssetAtPath<Texture2D>(profile.Path);
                FacetedDetailMaterials.Configure(material, palette);
                material.SetColor("_BaseColor", Color.white);
                material.SetFloat("_DetailScale", profile.DetailScale);
                material.SetFloat("_DetailStrength", profile.DetailStrength);
                EditorUtility.SetDirty(material); materials.Add(profile.Name, material);
            }
            foreach (var row in data.Items)
            {
                var root = PrefabUtility.LoadPrefabContents(row.PrefabPath);
                try
                {
                    var renderers = root.GetComponentsInChildren<Renderer>(true);
                    if (renderers.Length == 0) throw new InvalidOperationException("No renderers: " + row.PrefabPath);
                    foreach (var renderer in renderers)
                    {
                        var slots = new Material[renderer.sharedMaterials.Length];
                        for (int i = 0; i < slots.Length; i++) slots[i] = materials[row.PaletteFamily];
                        renderer.sharedMaterials = slots;
                    }
                    // No creation/deletion/reparenting, mesh changes, transform edits or animation writes.
                    PrefabUtility.SaveAsPrefabAsset(root, row.PrefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets(); Directory.CreateDirectory(Docs);
            File.WriteAllText(Docs + "Merchant-build.txt", "Assigned six semantic materials to five T1 display prefabs, ten existing T1 worn prefabs and MysteryBoxTier01.\n" + FitStatus + "\n");
            Debug.Log("Tier-one merchant surfaces assigned. " + FitStatus);
        }

        [MenuItem("The Trail Walker/Art/Capture Tier 1 Merchant Item Surfaces")]
        public static void Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Capture outside Play mode.");
            Directory.CreateDirectory(Docs);
            var records = new List<CaptureRow>();
            foreach (var row in Load().Items)
            {
                if (row.Role != "Display" && row.Role != "Box") continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(row.PrefabPath);
                if (prefab == null) throw new InvalidOperationException("Missing prefab " + row.PrefabPath);
                string front = "Merchant-" + row.Model + "-front.png";
                string study = "Merchant-" + row.Model + "-study.png";
                string label = "Merchant-" + row.Model + "-label.txt";
                Render(prefab, front, false); Render(prefab, study, true);
                File.WriteAllText(Docs + label, row.Name + "\n" + row.Id + "\n" + row.PrefabPath + "\nActual existing mesh; surface design only.\n" + FitStatus + "\n");
                records.Add(new CaptureRow { Id = row.Id, Name = row.Name, PrefabPath = row.PrefabPath,
                    MaterialPath = Materials + row.PaletteFamily + ".mat", FrontImage = front, StudyImage = study, TextLabel = label });
            }
            File.WriteAllText(Docs + "Merchant-capture-manifest.json", JsonUtility.ToJson(new CaptureManifest { FitStatus = FitStatus, Items = records.ToArray() }, true) + "\n");
        }

        static void Render(GameObject prefab, string filename, bool angled)
        {
            var preview = new PreviewRenderUtility();
            GameObject view = null;
            bool added = false;
            try
            {
                view = UnityEngine.Object.Instantiate(prefab);
                // Only the temporary preview is frozen; saved animator/hinge state is untouched.
                foreach (var animator in view.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
                preview.AddSingleGO(view); added = true;
                var renderers = view.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) throw new InvalidOperationException("No visible mesh: " + prefab.name);
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                var cam = preview.camera;
                cam.orthographic = true; cam.aspect = 1;
                float distance = Mathf.Max(4, bounds.size.magnitude * 3);
                var direction = (angled ? new Vector3(2.3f, 1.5f, -4) : new Vector3(0, 0, -4)).normalized;
                cam.transform.position = bounds.center + direction * distance;
                cam.transform.LookAt(bounds.center);
                float maxX = 0, maxY = 0;
                for (int i = 0; i < 8; i++)
                {
                    var p = bounds.center + Vector3.Scale(bounds.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var q = cam.transform.InverseTransformPoint(p);
                    maxX = Mathf.Max(maxX, Mathf.Abs(q.x)); maxY = Mathf.Max(maxY, Mathf.Abs(q.y));
                }
                cam.orthographicSize = Mathf.Max(.06f, Mathf.Max(maxX, maxY) * 1.16f);
                cam.nearClipPlane = .01f; cam.farClipPlane = distance + bounds.size.magnitude + 10;
                cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.20f, .23f, .24f);
                preview.lights[0].intensity = 1.2f; preview.lights[0].transform.rotation = Quaternion.Euler(35, 0, 0);
                preview.lights[1].intensity = .65f; preview.ambientColor = new Color(.55f, .55f, .55f);
                preview.BeginStaticPreview(new Rect(0, 0, 900, 900)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + filename, image.EncodeToPNG()); }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally
            {
                preview.Cleanup();
                if (!added && view != null) UnityEngine.Object.DestroyImmediate(view);
            }
        }
    }
}
