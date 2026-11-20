using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Settlement shopkeepers (user request, November 2, 2026): static waist-up NPCs, one per storefront. Art only: the
    // prefabs carry TorsoPivot / HeadPivot transforms for a later idle, and no shop logic, dialogue or animation.
    public static class ShopkeeperArtAssets
    {
        const string Source = "Assets/_Game/Art/Characters/Shopkeepers/";
        const string Target = "Assets/_Game/Resources/Characters/Shopkeepers/";
        const string Docs = "Docs/Art/Characters/Shopkeepers/";
        [Serializable] class Keeper { public string Name, Shop, Body, Head; }
        [Serializable] class Manifest { public float TorsoPivot, HeadUp, CounterTop; public Keeper[] Keepers; }

        [MenuItem("The Trail Walker/Art/Build Shopkeepers")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var manifest = Load();
            var palette = Source + "ShopkeeperPalette.png";
            var tex = (TextureImporter)AssetImporter.GetAtPath(palette);
            tex.filterMode = FilterMode.Point; tex.mipmapEnabled = false; tex.wrapMode = TextureWrapMode.Clamp;
            tex.textureCompression = TextureImporterCompression.Uncompressed; tex.npotScale = TextureImporterNPOTScale.None; tex.SaveAndReimport();
            Directory.CreateDirectory(Target);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Target + "ShopkeeperPalette.mat");
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, Target + "ShopkeeperPalette.mat"); }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(palette)); mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", .06f); mat.enableInstancing = true; EditorUtility.SetDirty(mat);
            foreach (var path in Directory.GetFiles(Source, "*.obj"))
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false; importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0; importer.SaveAndReimport();
            }
            foreach (var keeper in manifest.Keepers)
            {
                if (keeper.Name == "Forester") { LumberFieldArtAssets.BuildForester(); continue; }
                var root = Assemble(keeper, manifest, mat);
                try { PrefabUtility.SaveAsPrefabAsset(root, Target + "Shopkeeper" + keeper.Name + ".prefab"); }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Shopkeepers: " + manifest.Keepers.Length + " waist-up storefront NPC prefabs (art only).");
        }

        static Manifest Load() => JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "ShopkeeperManifest.json"));

        static GameObject Assemble(Keeper keeper, Manifest manifest, Material mat)
        {
            var root = new GameObject("Shopkeeper" + keeper.Name);
            var torso = new GameObject("TorsoPivot").transform; torso.SetParent(root.transform, false); torso.localPosition = new Vector3(0, manifest.TorsoPivot, 0);
            Part(torso, keeper.Body, mat);
            var head = new GameObject("HeadPivot").transform; head.SetParent(torso, false); head.localPosition = new Vector3(0, manifest.HeadUp, 0);
            Part(head, keeper.Head, mat);
            return root;
        }

        static void Part(Transform parent, string name, Material mat)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".obj");
            if (source == null) throw new InvalidOperationException("Missing shopkeeper mesh " + name);
            var part = UnityEngine.Object.Instantiate(source, parent, false); part.name = name;
            foreach (var r in part.GetComponentsInChildren<MeshRenderer>()) r.sharedMaterial = mat;
        }

        [MenuItem("The Trail Walker/Art/Capture Shopkeepers")]
        public static void Capture()
        {
            var manifest = Load(); Directory.CreateDirectory(Docs);
            var counter = new Material(Shader.Find("Universal Render Pipeline/Lit")); counter.SetColor("_BaseColor", new Color(.42f, .30f, .19f));
            try
            {
                Sheet(manifest, counter, "shopkeepers-behind-counter.png", 1.42f, false);
                Sheet(manifest, counter, "shopkeepers-turned.png", 1.42f, true);
            }
            finally { UnityEngine.Object.DestroyImmediate(counter); }
            Debug.Log("Shopkeeper sheets saved to " + Docs);
        }

        static void Sheet(Manifest manifest, Material counter, string file, float centerY, bool turned)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                int count = manifest.Keepers.Length;
                for (int i = 0; i < count; i++)
                {
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(Target + "ShopkeeperPalette.mat");
                    var keeper = Assemble(manifest.Keepers[i], manifest, mat);
                    float x = ((count - 1) * .5f - i) * .56f;
                    keeper.transform.position = new Vector3(x, 0, 0); keeper.transform.rotation = Quaternion.Euler(0, turned ? 35 : 0, 0);
                    preview.AddSingleGO(keeper);
                    // Stand-in counter (not a settlement asset): top at the manifest height, in front of the keeper.
                    var desk = GameObject.CreatePrimitive(PrimitiveType.Cube); UnityEngine.Object.DestroyImmediate(desk.GetComponent<Collider>());
                    desk.transform.position = new Vector3(x, manifest.CounterTop / 2f, .45f); desk.transform.localScale = new Vector3(.52f, manifest.CounterTop, .32f);
                    desk.GetComponent<Renderer>().sharedMaterial = counter; preview.AddSingleGO(desk);
                }
                preview.camera.orthographic = true; preview.camera.orthographicSize = .52f;
                preview.camera.transform.position = new Vector3(0, centerY + .25f, 8); preview.camera.transform.LookAt(new Vector3(0, centerY, 0));
                preview.camera.clearFlags = CameraClearFlags.SolidColor; preview.camera.backgroundColor = new Color(.62f, .66f, .64f);
                preview.camera.nearClipPlane = .01f; preview.camera.farClipPlane = 30;
                preview.lights[0].intensity = 1.25f; preview.lights[0].transform.rotation = Quaternion.Euler(32, 200, 0);
                preview.lights[1].intensity = .55f; preview.ambientColor = new Color(.52f, .54f, .56f);
                preview.BeginStaticPreview(new Rect(0, 0, 2400, 560)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + file, image.EncodeToPNG()); } finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
