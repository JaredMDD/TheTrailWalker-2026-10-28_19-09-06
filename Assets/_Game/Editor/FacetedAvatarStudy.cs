using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Art study only (November 2, 2026): imports the faceted avatar study meshes with flat normals and renders review
    // sheets. No prefabs, scenes or gameplay assets are changed.
    public static class FacetedAvatarStudy
    {
        const string Source = "Assets/_Game/Art/Characters/FacetedStudy/";
        const string Docs = "Docs/Art/Characters/FacetedStudy/";
        [Serializable] class Manifest { public float[] headAnchor; public AvatarInfo[] avatars; }
        [Serializable] class AvatarInfo { public string name; public string[] body; public string[] head; }

        [MenuItem("The Trail Walker/Art/Faceted Avatar Study")]
        public static void BuildAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run the study outside Play mode.");
            AssetDatabase.Refresh();
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "FacetedStudyManifest.json"));
            var palettePath = Source + "StudyPalette.png";
            var texImporter = (TextureImporter)AssetImporter.GetAtPath(palettePath);
            texImporter.filterMode = FilterMode.Point; texImporter.mipmapEnabled = false; texImporter.wrapMode = TextureWrapMode.Clamp;
            texImporter.textureCompression = TextureImporterCompression.Uncompressed; texImporter.npotScale = TextureImporterNPOTScale.None; texImporter.SaveAndReimport();
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Source + "StudyPalette.mat");
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, Source + "StudyPalette.mat"); }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(palettePath)); mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", .05f); EditorUtility.SetDirty(mat);
            foreach (var path in Directory.GetFiles(Source, "*.obj"))
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                // Flat facets: no smoothing across faces (the live avatars use 65 degrees, which rounds them).
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0; importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory(Docs);
            var anchor = new Vector3(manifest.headAnchor[0], manifest.headAnchor[1], manifest.headAnchor[2]);
            foreach (var avatar in manifest.avatars)
            {
                Capture(avatar.name + "-turnaround.png", 4, .70f, 1.0f, 1600, 1100, col => Assemble(avatar, anchor, mat, new[] { 0f, 35f, 90f, 180f }[col]));
                Capture(avatar.name + "-portrait.png", 3, .55f, 1.57f, 1500, 600, col => Assemble(avatar, anchor, mat, new[] { 0f, 30f, 75f }[col]), .29f);
            }
            Debug.Log("Faceted avatar study captured to " + Docs);
        }

        static GameObject Assemble(AvatarInfo info, Vector3 anchor, Material mat, float yaw)
        {
            var root = new GameObject(info.name);
            foreach (var name in info.body) Part(root.transform, name, mat, Vector3.zero);
            var head = new GameObject("HeadAnchor").transform; head.SetParent(root.transform, false); head.localPosition = anchor;
            foreach (var name in info.head) Part(head, name, mat, Vector3.zero);
            root.transform.rotation = Quaternion.Euler(0, yaw, 0);
            return root;
        }

        static void Part(Transform parent, string name, Material mat, Vector3 offset)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".obj");
            if (source == null) throw new InvalidOperationException("Missing study mesh " + name);
            var part = UnityEngine.Object.Instantiate(source, parent, false); part.name = name; part.transform.localPosition = offset;
            foreach (var r in part.GetComponentsInChildren<MeshRenderer>()) r.sharedMaterials = new[] { mat };
        }

        static void Capture(string file, int columns, float pitch, float centerY, int width, int height, Func<int, GameObject> make, float size = 0f)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                for (int col = 0; col < columns; col++)
                {
                    var a = make(col);
                    a.transform.position = new Vector3(((columns - 1) * .5f - col) * pitch, 0, 0);
                    preview.AddSingleGO(a);
                }
                preview.camera.orthographic = true; preview.camera.orthographicSize = size > 0 ? size : 1.0f;
                preview.camera.transform.position = new Vector3(0, centerY, 8); preview.camera.transform.LookAt(new Vector3(0, centerY, 0));
                preview.camera.clearFlags = CameraClearFlags.SolidColor; preview.camera.backgroundColor = new Color(.62f, .66f, .64f);
                preview.camera.nearClipPlane = .01f; preview.camera.farClipPlane = 30;
                preview.lights[0].intensity = 1.25f; preview.lights[0].transform.rotation = Quaternion.Euler(32, 200, 0);
                preview.lights[1].intensity = .55f; preview.ambientColor = new Color(.52f, .54f, .56f);
                preview.BeginStaticPreview(new Rect(0, 0, width, height)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + file, image.EncodeToPNG()); } finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
