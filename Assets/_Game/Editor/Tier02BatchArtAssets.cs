using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Passive art only: no item/recipe catalogs, source gates, scenes or test saves are modified.
    public static class Tier02BatchArtAssets
    {
        static string ManifestPath, MaterialPath, Docs;
        static void SelectBatch(int batch)
        {
            if (batch < 9 || batch > 45) throw new ArgumentOutOfRangeException(nameof(batch));
            string code = batch.ToString("D2");
            ManifestPath = "Assets/_Game/Art/Crafting/Tier02/Tier02Batch" + code + "Manifest.json";
            MaterialPath = "Assets/_Game/Resources/Tier02/Batch" + code + "Materials/";
            Docs = "Docs/Art/Tier02/Batch" + code + "/";
        }
        [Serializable] sealed class Item { public string Name, Model, SourcePath, PalettePath, PrefabPath, ItemId, Representation; public int Triangles; }
        [Serializable] sealed class Manifest { public Item[] Items; }
        static Manifest Load() => JsonUtility.FromJson<Manifest>(File.ReadAllText(ManifestPath));

        public static void Build(int batch)
        {
            SelectBatch(batch);
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh(); Directory.CreateDirectory(MaterialPath);
            var shader = Shader.Find("TheTrailWalker/FacetedDetail");
            if (shader == null) throw new InvalidOperationException("FacetedDetail shader unavailable.");
            foreach (var row in Load().Items)
            {
                var texture = (TextureImporter)AssetImporter.GetAtPath(row.PalettePath);
                texture.alphaSource = TextureImporterAlphaSource.FromInput; texture.alphaIsTransparency = false;
                texture.textureCompression = TextureImporterCompression.Uncompressed; texture.mipmapEnabled = false;
                texture.filterMode = FilterMode.Point; texture.wrapMode = TextureWrapMode.Clamp; texture.SaveAndReimport();
                var importer = (ModelImporter)AssetImporter.GetAtPath(row.SourcePath);
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false;
                importer.importLights = false; importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0;
                importer.SaveAndReimport();
                string materialFile = MaterialPath + row.Model + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialFile);
                if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, materialFile); }
                material.shader = shader; material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(row.PalettePath));
                material.SetColor("_BaseColor", Color.white);
                material.SetTexture("_DetailMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Shared/Detail/DetailAtlas.png"));
                material.SetFloat("_DetailScale", 5); material.SetFloat("_DetailStrength", .40f); material.enableInstancing = true;
                EditorUtility.SetDirty(material);
                string prefabFile = "Assets/_Game/Resources/" + row.PrefabPath + ".prefab";
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
            File.WriteAllText(Docs + "unity-build-result.txt", "SUCCEEDED: passive batch prefabs/materials. No runtime bindings or gameplay tests. " + DateTime.UtcNow.ToString("O"));
        }

        public static void Capture(int batch)
        {
            SelectBatch(batch);
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Capture outside Play mode.");
            foreach (var row in Load().Items)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/" + row.PrefabPath + ".prefab");
                float forward = row.Representation == "worn" ? 1f : -1f;
                Render(prefab, row.Model + "-unity-front.png", new Vector3(0,0,4*forward));
                Render(prefab, row.Model + "-unity-study.png", new Vector3(2.3f,1.3f,4*forward));
            }
            File.WriteAllText(Docs + "unity-capture-result.txt", "SUCCEEDED: actual imported-prefab front/study images. Device/gameplay validation pending. " + DateTime.UtcNow.ToString("O"));
        }

        static void Render(GameObject prefab, string filename, Vector3 direction)
        {
            if (prefab == null) throw new InvalidOperationException("Build batch art before capture.");
            var preview = new PreviewRenderUtility();
            try
            {
                var view = UnityEngine.Object.Instantiate(prefab); preview.AddSingleGO(view);
                var renderers = view.GetComponentsInChildren<Renderer>(); Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                var camera = preview.camera; camera.orthographic = true;
                camera.transform.position = bounds.center + direction; camera.transform.LookAt(bounds.center);
                float maxX=0, maxY=0;
                for (int i=0;i<8;i++)
                {
                    var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    var point=camera.transform.InverseTransformPoint(corner);
                    maxX=Mathf.Max(maxX,Mathf.Abs(point.x));maxY=Mathf.Max(maxY,Mathf.Abs(point.y));
                }
                camera.orthographicSize=Mathf.Max(.06f,Mathf.Max(maxX,maxY)*1.18f);
                camera.nearClipPlane=.01f;camera.farClipPlane=30;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.20f,.23f,.24f);
                preview.lights[0].intensity=1.2f;preview.lights[0].transform.rotation=Quaternion.Euler(35,0,0);
                preview.lights[1].intensity=.6f;preview.ambientColor=new Color(.55f,.55f,.55f);
                preview.BeginStaticPreview(new Rect(0,0,800,800));preview.Render(true);
                var result=preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs+filename,result.EncodeToPNG()); }
                finally { UnityEngine.Object.DestroyImmediate(result); }
            }
            finally { preview.Cleanup(); }
        }
    }
}

