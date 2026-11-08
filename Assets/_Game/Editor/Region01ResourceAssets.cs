using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Bakes source OBJs into small shared-mesh prefabs after imports finish, never during Play.
    [InitializeOnLoad]
    public static class Region01ResourceAssets
    {
        private const string Source = "Assets/_Game/Art/Items/Region01/";
        private const string Target = "Assets/_Game/Resources/Region01/";
        private static readonly string[] Names = { "PineBranch", "RabbitPelt", "Apple" };
        static Region01ResourceAssets() { EditorApplication.update += EnsureImported; }

        private static void EnsureImported()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= EnsureImported;
            foreach (string name in Names)
                if (!File.Exists(Target + name + ".prefab")) { Build(); return; }
        }

        [MenuItem("The Trail Walker/Art/Rebuild Region 1 Resource Prefabs")]
        public static void Build()
        {
            BuildRegion(Source, Target, "Region01Palette", "Region01", Names);
        }

        public static void BuildRegion(string sourcePath, string targetPath, string paletteName, string region, string[] names)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Build resource assets outside Play mode.");
            Directory.CreateDirectory(targetPath);
            AssetDatabase.Refresh();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath + paletteName + ".png");
            if (texture == null) throw new InvalidOperationException(region + " source palette has not imported.");
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("The URP Lit shader is required.");
            string materialPath = targetPath + paletteName + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, materialPath); }
            // Keep the surface-detail shader once applied (FacetedDetailMaterials), so rebuilds do not strip texture.
            if (material.shader == null || material.shader.name != FacetedDetailMaterials.ShaderName) material.shader = shader;
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0.08f);
            material.SetFloat("_Metallic", 0);
            FacetedDetailMaterials.Configure(material, texture);
            EditorUtility.SetDirty(material);
            foreach (string name in names)
            {
                Material modelMaterial = Tier01ItemDesignAssets.ExistingMaterial(sourcePath, name) ?? material;
                if (name == "Mushrooms" && sourcePath == "Assets/_Game/Art/Crafting/Tier01/")
                {
                    var mushroomPalette = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Items/Region04/MushroomsPalette.png");
                    if (mushroomPalette == null) throw new InvalidOperationException("Generate loose resource detail palettes first.");
                    string isolatedPath = targetPath + "MushroomsDetail.mat";
                    modelMaterial = AssetDatabase.LoadAssetAtPath<Material>(isolatedPath);
                    if (modelMaterial == null) { modelMaterial = new Material(shader); AssetDatabase.CreateAsset(modelMaterial, isolatedPath); }
                    FacetedDetailMaterials.Configure(modelMaterial, mushroomPalette);
                }
                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath + name + ".obj");
                if (source == null) throw new InvalidOperationException("Model not imported: " + name);
                GameObject root = new GameObject(name);
                try
                {
                    GameObject visual = UnityEngine.Object.Instantiate(source, root.transform, false);
                    visual.name = "Visual";
                    foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
                    {
                        var materials = new Material[renderer.sharedMaterials.Length];
                        for (int i = 0; i < materials.Length; i++) materials[i] = modelMaterial;
                        renderer.sharedMaterials = materials;
                    }
                    // Pickup ownership/collider live on the existing Bundle parent, not inside this visual prefab.
                    foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
                    PrefabUtility.SaveAsPrefabAsset(root, targetPath + name + ".prefab");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssets();
            File.WriteAllText("Temp/" + region + "-art.result", "SUCCEEDED: " + names.Length + " resource prefabs and shared palette material imported. " + DateTime.UtcNow.ToString("O"));
            Debug.Log(region + " resource meshes integrated: " + string.Join(", ", names));
        }

        [MenuItem("The Trail Walker/Art/Capture Region 1 Resource Preview")]
        public static void CapturePreview()
        {
            CaptureRegionPreview(Target, Names, "Docs/Art/Region01/resource-meshes-unity-v1.png");
        }

        public static void CaptureRegionPreview(string targetPath, string[] names, string destination, string[] prefabPaths = null)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                for (int i = 0; i < names.Length; i++)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths == null ? targetPath + names[i] + ".prefab" : prefabPaths[i]);
                    if (prefab == null) throw new InvalidOperationException("Build the resource prefabs first.");
                    GameObject model = UnityEngine.Object.Instantiate(prefab);
                    model.transform.position = new Vector3((i - 1) * 1.8f, 0, 0);
                    preview.AddSingleGO(model);
                }
                preview.camera.orthographic = true;
                preview.camera.orthographicSize = 1.3f;
                preview.camera.transform.position = new Vector3(0, 3.5f, -6);
                preview.camera.transform.LookAt(new Vector3(0, 0.05f, 0));
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(0.19f, 0.22f, 0.25f);
                preview.camera.nearClipPlane = 0.01f;
                preview.camera.farClipPlane = 30;
                preview.lights[0].intensity = 1.2f;
                preview.lights[0].transform.rotation = Quaternion.Euler(40, 30, 0);
                preview.lights[1].intensity = 0.6f;
                preview.ambientColor = new Color(0.4f, 0.4f, 0.4f);
                preview.BeginStaticPreview(new Rect(0, 0, 1536, 768));
                preview.Render(true);
                Texture2D image = preview.EndStaticPreview();
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    File.WriteAllBytes(destination, image.EncodeToPNG());
                }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }

    internal sealed class Region01ResourceImport : AssetPostprocessor
    {
        private const string Source = "Assets/_Game/Art/Items/Region01/";
        private const string Source2 = "Assets/_Game/Art/Items/Region02/";
        private const string Source3 = "Assets/_Game/Art/Items/Region03/";
        private const string Source4 = "Assets/_Game/Art/Items/Region04/";
        private const string Source5 = "Assets/_Game/Art/Items/Region05/";
        private const string Source6 = "Assets/_Game/Art/Items/Region06/";
        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(Source, StringComparison.Ordinal) && !assetPath.StartsWith(Source2, StringComparison.Ordinal) && !assetPath.StartsWith(Source3, StringComparison.Ordinal) && !assetPath.StartsWith(Source4, StringComparison.Ordinal) && !assetPath.StartsWith(Source5, StringComparison.Ordinal) && !assetPath.StartsWith(Source6, StringComparison.Ordinal)) return;
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
            if (assetPath != Source + "Region01Palette.png" && assetPath != Source2 + "Region02Palette.png" && assetPath != Source3 + "Region03Palette.png" && assetPath != Source4 + "Region04Palette.png" && assetPath != Source5 + "Region05Palette.png" && assetPath != Source6 + "Region06Palette.png") return;
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
