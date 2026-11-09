using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheTrailWalker.Editor
{
    // Isolated art authoring. Never calls another builder or changes an existing catalog/material/prefab.
    public static class Tier01FittedAccessoriesAssets
    {
        const string Source = "Assets/_Game/Art/Characters/Tier01Accessories/";
        const string Target = "Assets/_Game/Resources/Characters/Tier01Accessories/";
        const string ReviewTarget = "Assets/_Game/ArtReviews/Characters/Tier01Accessories/";
        const string Existing = "Assets/_Game/Art/Characters/PlayerDefault/";
        const string Docs = "Docs/Art/Characters/Tier01Accessories/";
        static readonly string[] Bodies = { "Male", "Female" };
        static readonly string[] Styles = { "Crop", "Sweep", "Bob", "Tied" };
        [Serializable] class Part { public string name, role, body, style, frame; public string[] materials; }
        [Serializable] class ColorInfo { public string name, hex; }
        [Serializable] class Manifest { public Part[] parts; public ColorInfo[] materials; }
        static Manifest Read(string folder, string file) => JsonUtility.FromJson<Manifest>(File.ReadAllText(folder + file));
        static Manifest Own => Read(Source, "Tier01AccessoriesManifest.json");
        static Manifest Avatar => Read(Existing, "PlayerAvatarManifest.json");

        [MenuItem("The Trail Walker/Art/Tier 1 Accessories/Build Isolated Art")]
        public static void Build()
        {
            Guard();
            Directory.CreateDirectory(Target + "Parts");
            Directory.CreateDirectory(Target + "Materials");
            Directory.CreateDirectory(ReviewTarget + "ReviewOnly");
            AssetDatabase.Refresh();
            var manifest = Own;
            foreach (var part in manifest.parts)
            {
                string path = Source + part.name + ".obj";
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) throw new FileNotFoundException("Missing accessory OBJ importer: " + path);
                importer.globalScale = 1;
                importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
                importer.addCollider = false; importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0;
                importer.SaveAndReimport();
            }
            // Only genuinely new material groups are created here. All existing material assets are read-only.
            foreach (var color in manifest.materials)
            {
                if (color.name == "Hair" || color.name == "Skin" || AssetDatabase.LoadAssetAtPath<Material>(Existing + "Unity/Materials/" + color.name + ".mat") != null) continue;
                string path = Target + "Materials/" + color.name + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null) throw new InvalidOperationException("URP/Lit shader required for isolated art.");
                    mat = new Material(shader); AssetDatabase.CreateAsset(mat, path);
                }
                ColorUtility.TryParseHtmlString(color.hex, out Color value);
                mat.SetColor("_BaseColor", value); mat.SetFloat("_Smoothness", .08f); EditorUtility.SetDirty(mat);
            }
            foreach (var part in manifest.parts)
            {
                var root = new GameObject(part.name);
                try
                {
                    Add(root.transform, Source, part, 1);
                    PrefabUtility.SaveAsPrefabAsset(root, Target + "Parts/" + part.name + ".prefab");
                }
                finally { Object.DestroyImmediate(root); }
            }
            // Review compositions are passive, editor-built arrangements, not equip/runtime binding.
            foreach (string body in Bodies) foreach (string style in Styles) foreach (bool seated in new[] { false, true })
            {
                var root = Review(body, style, 1, seated);
                try { PrefabUtility.SaveAsPrefabAsset(root, ReviewTarget + "ReviewOnly/" + root.name + ".prefab"); }
                finally { DestroyReview(root); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Tier01 accessory art built: 20 source-part prefabs (6 representations + 8 hat hair + 6 seated studies), 16 review-only compositions. Binding unchanged.");
        }

        static void Guard()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Accessory authoring is Edit-mode only.");
        }

        static Material MaterialFor(string key, int hair)
        {
            string name = key == "Hair" || key == "Brows" ? "Hair" + (hair + 1).ToString("00") : key == "Skin" ? "Skin03" : key;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Existing + "Unity/Materials/" + name + ".mat");
            if (mat == null) mat = AssetDatabase.LoadAssetAtPath<Material>(Target + "Materials/" + name + ".mat");
            if (mat == null) throw new FileNotFoundException("Build current avatar art first; missing read-only material: " + name);
            return mat;
        }

        static GameObject Add(Transform parent, string folder, Part part, int hair)
        {
            if (part == null) throw new InvalidOperationException("Required part missing from source manifest.");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(folder + part.name + ".obj");
            if (model == null) throw new FileNotFoundException(folder + part.name + ".obj");
            var instance = Object.Instantiate(model, parent, false); instance.name = part.name;
            var materials = Array.ConvertAll(part.materials, key => MaterialFor(key, hair));
            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount != materials.Length)
                    throw new InvalidOperationException("Unexpected OBJ material grouping: " + part.name);
                renderer.sharedMaterials = materials;
            }
            return instance;
        }
        static GameObject AddOwn(Transform parent, string name, int hair) => Add(parent, Source, Array.Find(Own.parts, p => p.name == name), hair);
        static GameObject AddAvatar(Transform parent, string name, int hair) => Add(parent, Existing, Array.Find(Avatar.parts, p => p.name == name), hair);
        static Transform Pivot(Transform parent, string name, Vector3 position)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false); pivot.localPosition = position; return pivot;
        }

        static GameObject Review(string body, string style, int hair, bool seated)
        {
            var root = new GameObject(body + style + (seated ? "SeatedArtStudy" : "StandingArtStudy"));
            try
            {
                float drop = seated ? -.38f : 0;
                var upper = Pivot(root.transform, "UpperBodyArtStudy", new Vector3(0, drop, 0));
                if (seated) AddOwn(upper, body + "SeatedUpperSkinStudy", hair);
                else AddAvatar(upper, body + "Skin", hair);
                AddAvatar(upper, body + "WearHempShirt", hair);
                if (seated)
                {
                    AddOwn(root.transform, body + "SeatedHempPantsStudy", hair);
                    AddOwn(root.transform, body + "SeatedRabbitBootsStudy", hair);
                }
                else
                {
                    AddAvatar(root.transform, body + "WearHempPants", hair);
                    AddAvatar(root.transform, body + "WearRabbitBoots", hair);
                }
                var head = Pivot(upper, "HeadAnchor", new Vector3(0, 1.43f, 0));
                AddAvatar(head, body + "Head", hair); AddAvatar(head, body + "Eyes", hair);
                AddOwn(Pivot(head, "HairAnchor", Vector3.zero), body + "HatHair" + style, hair);
                AddOwn(Pivot(head, "HatAnchor", Vector3.zero), body + "WearRabbitHat", hair);
                AddOwn(Pivot(upper, "BackpackAnchor", new Vector3(0, 1.14f, -.20f)), body + "WearStarterBackpack", hair);
                return root;
            }
            catch { DestroyReview(root); throw; }
        }

        static void DestroyReview(GameObject root) { Object.DestroyImmediate(root); }

        [MenuItem("The Trail Walker/Art/Tier 1 Accessories/Capture Static Worn Review")]
        public static void CaptureWornReview()
        {
            Guard(); Directory.CreateDirectory(Docs);
            foreach (string body in Bodies)
            {
                foreach (var angle in new[] { 0, 90, 180, 35 })
                    Capture(body + "-standing-" + angle + ".png", Review(body, "Tied", 1, false), angle, false, false);
                foreach (string style in Styles) foreach (var angle in new[] { 0, 90, 180 })
                    Capture(body + "-hat-" + style + "-" + angle + ".png", Review(body, style, 1, false), angle, true, false);
                for (int tone = 0; tone < 5; tone++)
                    Capture(body + "-hair-palette-" + (tone + 1) + ".png", Review(body, "Sweep", tone, false), 25, true, false);
            }
            File.WriteAllText(Docs + "worn-capture-status.txt", "CaptureWornReview completed in Edit mode. Static art only; no equip, animation or deformation validation.\n");
        }

        [MenuItem("The Trail Walker/Art/Tier 1 Accessories/Capture Static Pose Review")]
        public static void CapturePoseReview()
        {
            Guard(); Directory.CreateDirectory(Docs);
            foreach (string body in Bodies) foreach (string style in Styles) foreach (var angle in new[] { 0, 90, 180, 35 })
                Capture(body + "-seated-" + style + "-" + angle + ".png", Review(body, style, 1, true), angle, false, true);
            File.WriteAllText(Docs + "pose-capture-status.txt", "CapturePoseReview completed. Rigid segmented seated art studies; not animated/skinned equipment. Joint transitions remain a later deformation task.\n");
        }

        [MenuItem("The Trail Walker/Art/Tier 1 Accessories/Capture Display Review")]
        public static void CaptureDisplayReview()
        {
            Guard(); Directory.CreateDirectory(Docs);
            foreach (string name in new[] { "DisplayRabbitHat", "DisplayStarterBackpack" }) foreach (var angle in new[] { 35, 145, 215 })
            {
                var root = new GameObject(name); AddOwn(root.transform, name, 1);
                Capture(name + "-" + angle + ".png", root, angle, false, false, true);
            }
            File.WriteAllText(Docs + "display-capture-status.txt", "CaptureDisplayReview completed. Separate loose studies; existing Tier01/Crafted item prefabs and footprint definitions unchanged.\n");
        }

        [MenuItem("The Trail Walker/Art/Tier 1 Accessories/Capture All Static Art")]
        public static void CaptureAll()
        {
            CaptureDisplayReview(); CaptureWornReview(); CapturePoseReview();
        }

        static void Capture(string file, GameObject root, float yaw, bool portrait, bool seated, bool display = false)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                root.transform.rotation = Quaternion.Euler(0, yaw, 0);
                preview.AddSingleGO(root);
                float center = display ? 0 : portrait ? 1.60f : seated ? .70f : .88f;
                preview.camera.orthographic = true;
                preview.camera.orthographicSize = display ? .43f : portrait ? .245f : seated ? .79f : .99f;
                preview.camera.transform.position = new Vector3(0, center + (display ? .10f : 0), 5);
                preview.camera.transform.LookAt(new Vector3(0, center, 0));
                preview.camera.nearClipPlane = .01f; preview.camera.farClipPlane = 20;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(.18f, .22f, .24f);
                preview.lights[0].intensity = 1.25f; preview.lights[0].transform.rotation = Quaternion.Euler(30, 210, 0);
                preview.lights[1].intensity = .8f; preview.lights[1].transform.rotation = Quaternion.Euler(10, 20, 0);
                preview.ambientColor = new Color(.5f, .5f, .5f);
                preview.BeginStaticPreview(new Rect(0, 0, 900, 1100)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + file, image.EncodeToPNG()); }
                finally { Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
