using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Settlement storefronts (user request, November 2, 2026): street front + counter view per shop, art only.
    // The counter prefab places the shop's keeper (Resources/Characters/Shopkeepers) and named offer anchors.
    public static class StorefrontArtAssets
    {
        const string Source = "Assets/_Game/Art/Settlement/Storefronts/";
        const string Target = "Assets/_Game/Resources/Settlement/Storefronts/";
        const string Docs = "Docs/Art/Settlement/Storefronts/";
        [Serializable] class Shop { public string Name, Front, Counter, Keeper; public float Width, Depth; public float[] OfferAnchorsFlat; }
        [Serializable] class Manifest { public float CounterTop; public Shop[] Shops; }

        static Manifest Load() => JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "StorefrontManifest.json"));
        static Vector3[] Offers(Shop shop)
        {
            var f = shop.OfferAnchorsFlat ?? new float[0]; var result = new Vector3[f.Length / 3];
            for (int i = 0; i < result.Length; i++) result[i] = new Vector3(f[i * 3], f[i * 3 + 1], f[i * 3 + 2]);
            return result;
        }

        [MenuItem("The Trail Walker/Art/Build Storefronts")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var manifest = Load();
            var palette = Source + "StorefrontPalette.png";
            var tex = (TextureImporter)AssetImporter.GetAtPath(palette);
            tex.filterMode = FilterMode.Point; tex.mipmapEnabled = false; tex.wrapMode = TextureWrapMode.Clamp;
            tex.textureCompression = TextureImporterCompression.Uncompressed; tex.npotScale = TextureImporterNPOTScale.None; tex.SaveAndReimport();
            Directory.CreateDirectory(Target);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Target + "StorefrontPalette.mat");
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, Target + "StorefrontPalette.mat"); }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(palette)); mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", .05f); mat.enableInstancing = true; EditorUtility.SetDirty(mat);
            foreach (var path in Directory.GetFiles(Source, "*.obj"))
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false; importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0; importer.SaveAndReimport();
            }
            foreach (var shop in manifest.Shops)
            {
                Save(Front(shop, mat), shop.Name + "Front");
                Save(Counter(shop, mat), shop.Name + "Counter");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Storefronts: " + manifest.Shops.Length + " shop(s), street front and counter view prefabs (art only).");
        }

        static void Save(GameObject root, string name)
        {
            try { PrefabUtility.SaveAsPrefabAsset(root, Target + name + ".prefab"); } finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        static GameObject Front(Shop shop, Material mat)
        {
            var root = new GameObject(shop.Name + "Front"); Part(root.transform, shop.Front, mat);
            // Selection marker for the side-scrolling street (art marker only).
            var select = new GameObject("SelectAnchor").transform; select.SetParent(root.transform, false); select.localPosition = new Vector3(0, 1.4f, .2f);
            return root;
        }

        static GameObject Counter(Shop shop, Material mat)
        {
            var root = new GameObject(shop.Name + "Counter"); Part(root.transform, shop.Counter, mat);
            var keeperPrefab = Resources.Load<GameObject>("Characters/Shopkeepers/" + shop.Keeper);
            if (keeperPrefab != null) { var keeper = (GameObject)PrefabUtility.InstantiatePrefab(keeperPrefab, root.transform); keeper.name = "Shopkeeper"; }
            var offers = Offers(shop);
            for (int i = 0; i < offers.Length; i++)
            {
                var anchor = new GameObject("OfferAnchor" + (i + 1)).transform; anchor.SetParent(root.transform, false); anchor.localPosition = offers[i];
            }
            var eye = new GameObject("CustomerCameraAnchor").transform; eye.SetParent(root.transform, false);
            eye.localPosition = new Vector3(0, 1.55f, 3.0f); eye.localRotation = Quaternion.LookRotation(new Vector3(0, 1.35f, 0) - eye.localPosition);
            return root;
        }

        static void Part(Transform parent, string name, Material mat)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".obj");
            if (source == null) throw new InvalidOperationException("Missing storefront mesh " + name);
            var part = UnityEngine.Object.Instantiate(source, parent, false); part.name = name;
            foreach (var r in part.GetComponentsInChildren<MeshRenderer>()) r.sharedMaterial = mat;
        }

        [MenuItem("The Trail Walker/Art/Capture Storefronts")]
        public static void Capture()
        {
            var manifest = Load(); Directory.CreateDirectory(Docs);
            foreach (var shop in manifest.Shops)
            {
                var front = AssetDatabase.LoadAssetAtPath<GameObject>(Target + shop.Name + "Front.prefab");
                var counter = AssetDatabase.LoadAssetAtPath<GameObject>(Target + shop.Name + "Counter.prefab");
                Render(front, shop.Name.ToLowerInvariant() + "-street-front.png", true, new Vector3(0, 4.6f, 30), new Vector3(0, 4.6f, 0), 4.9f, 1600, 1300);
                Render(front, shop.Name.ToLowerInvariant() + "-three-quarter.png", false, new Vector3(9, 5.5f, 14), new Vector3(0, 3.6f, -1.5f), 34, 1600, 1300);
                Render(counter, shop.Name.ToLowerInvariant() + "-counter-view.png", false, new Vector3(0, 1.55f, 3.6f), new Vector3(0, 1.35f, 0), 50, 1600, 1000);
            }
            // Street overview: every front side by side in manifest order (the proposed street order), 1.2 m apart. The OBJ
            // import mirrors X, so the row is laid out toward -X to read left to right on screen.
            var street = new GameObject("Street overview");
            try
            {
                float total = 0; foreach (var shop in manifest.Shops) total += shop.Width + 1.2f;
                float x = total / 2;
                foreach (var shop in manifest.Shops)
                {
                    var front = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Target + shop.Name + "Front.prefab"), street.transform);
                    x -= shop.Width / 2 + .6f; front.transform.localPosition = new Vector3(x, 0, 0); x -= shop.Width / 2 + .6f;
                }
                Render(street, "settlement-street.png", true, new Vector3(0, 5.2f, 40), new Vector3(0, 5.2f, 0), 6.0f, 4000, 800, 8);
            }
            finally { UnityEngine.Object.DestroyImmediate(street); }
            Debug.Log("Storefront sheets saved to " + Docs);
        }

        static void Render(GameObject prefab, string file, bool ortho, Vector3 eye, Vector3 target, float sizeOrFov, int width, int height, float groundScale = 3)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                var view = UnityEngine.Object.Instantiate(prefab); preview.AddSingleGO(view);
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>());
                var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit")); groundMat.SetColor("_BaseColor", new Color(.48f, .55f, .36f));
                ground.GetComponent<Renderer>().sharedMaterial = groundMat; ground.transform.position = new Vector3(0, -.01f, 0); ground.transform.localScale = new Vector3(groundScale, 1, 3);
                preview.AddSingleGO(ground);
                var cam = preview.camera; cam.orthographic = ortho;
                if (ortho) cam.orthographicSize = sizeOrFov; else cam.fieldOfView = sizeOrFov;
                cam.transform.position = eye; cam.transform.LookAt(target);
                cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.66f, .76f, .82f);
                cam.nearClipPlane = .05f; cam.farClipPlane = 80;
                preview.lights[0].intensity = 1.25f; preview.lights[0].transform.rotation = Quaternion.Euler(38, 210, 0);
                preview.lights[1].intensity = .5f; preview.ambientColor = new Color(.55f, .56f, .58f);
                preview.BeginStaticPreview(new Rect(0, 0, width, height)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + file, image.EncodeToPNG()); } finally { UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(groundMat); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
