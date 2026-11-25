using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Narrow builder (Milestone 14 slice 3): only the balloon interior prefab and its own palette material, set up the way
    // StorefrontArtAssets.BuildTier2Fronts does (the storefront material's shader and detail settings on this strip). Same
    // counter conventions as the storefront counters: the keeper at the origin as "Shopkeeper", OfferAnchor1-3 from the
    // manifest and the CustomerCameraAnchor. Source: Tools/Art/generate_balloon_interior.py.
    public static class BalloonInteriorAssets
    {
        const string Source = "Assets/_Game/Art/Settlement/Balloon/";
        const string Target = "Assets/_Game/Resources/Settlement/Balloon/";
        [Serializable] class Manifest { public float CounterTop; public string Mesh, Keeper; public float[] OfferAnchorsFlat; }

        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "BalloonInteriorManifest.json"));
            var palette = Source + "BalloonInteriorPalette.png";
            var tex = (TextureImporter)AssetImporter.GetAtPath(palette);
            if (tex == null) throw new InvalidOperationException("Missing palette " + palette);
            tex.filterMode = FilterMode.Point; tex.mipmapEnabled = false; tex.wrapMode = TextureWrapMode.Clamp; tex.npotScale = TextureImporterNPOTScale.None;
            // Alpha holds the surface-detail type (the FacetedDetail shader): keep it exact.
            tex.alphaSource = TextureImporterAlphaSource.FromInput; tex.alphaIsTransparency = false; tex.textureCompression = TextureImporterCompression.Uncompressed; tex.SaveAndReimport();
            Directory.CreateDirectory(Target);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Target + "BalloonInteriorPalette.mat");
            if (mat == null) { mat = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Resources/Settlement/Storefronts/StorefrontPalette.mat")); AssetDatabase.CreateAsset(mat, Target + "BalloonInteriorPalette.mat"); }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(palette)); EditorUtility.SetDirty(mat);

            string model = Source + manifest.Mesh + ".obj";
            var importer = (ModelImporter)AssetImporter.GetAtPath(model);
            if (importer == null) throw new InvalidOperationException("Missing model " + model);
            importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false; importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0; importer.SaveAndReimport();

            var root = new GameObject("BalloonInterior");
            try
            {
                var part = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(model), root.transform, false); part.name = manifest.Mesh;
                foreach (var r in part.GetComponentsInChildren<MeshRenderer>()) r.sharedMaterial = mat;
                var keeperPrefab = Resources.Load<GameObject>("Characters/Shopkeepers/" + manifest.Keeper);
                if (keeperPrefab == null) throw new InvalidOperationException("Missing keeper " + manifest.Keeper);
                ((GameObject)PrefabUtility.InstantiatePrefab(keeperPrefab, root.transform)).name = "Shopkeeper";   // a nested instance: the keeper prefab is not written
                var f = manifest.OfferAnchorsFlat;
                for (int i = 0; i < f.Length / 3; i++)
                {
                    var anchor = new GameObject("OfferAnchor" + (i + 1)).transform; anchor.SetParent(root.transform, false); anchor.localPosition = new Vector3(f[i * 3], f[i * 3 + 1], f[i * 3 + 2]);
                }
                var eye = new GameObject("CustomerCameraAnchor").transform; eye.SetParent(root.transform, false);
                eye.localPosition = new Vector3(0, 1.55f, 3.0f); eye.localRotation = Quaternion.LookRotation(new Vector3(0, 1.35f, 0) - eye.localPosition);
                PrefabUtility.SaveAsPrefabAsset(root, Target + "BalloonInterior.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssetIfDirty(mat);   // only this material: never flush other dirty assets
        }
    }
}
