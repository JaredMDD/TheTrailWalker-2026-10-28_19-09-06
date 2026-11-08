using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // River crossing art (user request, November 2, 2026): dock, cable ferry raft and cable posts. Art only.
    public static class FerryArtAssets
    {
        const string Source = "Assets/_Game/Art/Environment/Ferry/";
        const string Target = "Assets/_Game/Resources/Environment/Ferry/";
        static readonly string[] Parts = { "FerryDock", "FerryRaft", "FerryCable" };

        [MenuItem("The Trail Walker/Art/Build Ferry")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var palette = Source + "FerryPalette.png";
            var tex = (TextureImporter)AssetImporter.GetAtPath(palette);
            tex.filterMode = FilterMode.Point; tex.mipmapEnabled = false; tex.wrapMode = TextureWrapMode.Clamp; tex.alphaSource = TextureImporterAlphaSource.FromInput;
            tex.alphaIsTransparency = false; tex.textureCompression = TextureImporterCompression.Uncompressed; tex.npotScale = TextureImporterNPOTScale.None; tex.SaveAndReimport();
            Directory.CreateDirectory(Target);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Target + "FerryPalette.mat");
            if (mat == null) { mat = new Material(Shader.Find(FacetedDetailMaterials.ShaderName)); AssetDatabase.CreateAsset(mat, Target + "FerryPalette.mat"); }
            mat.shader = Shader.Find(FacetedDetailMaterials.ShaderName);
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(palette)); mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_DetailMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Shared/Detail/DetailAtlas.png"));
            mat.SetFloat("_DetailScale", 1); mat.SetFloat("_DetailStrength", .75f); mat.enableInstancing = true; EditorUtility.SetDirty(mat);
            foreach (var name in Parts)
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(Source + name + ".obj");
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false; importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0; importer.SaveAndReimport();
                var root = new GameObject(name);
                try
                {
                    var part = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".obj"), root.transform, false); part.name = name + "Mesh";
                    foreach (var r in part.GetComponentsInChildren<MeshRenderer>()) r.sharedMaterial = mat;
                    PrefabUtility.SaveAsPrefabAsset(root, Target + name + ".prefab");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Ferry art: dock, raft and cable prefabs.");
        }
    }
}
