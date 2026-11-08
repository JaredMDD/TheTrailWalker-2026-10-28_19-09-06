using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Art authoring only. No scene edits, player progression, tests or Play-mode calls.
    public static class HouseArtAssets
    {
        const string Source = "Assets/_Game/Art/Settlement/Houses/";
        const string Target = "Assets/_Game/Resources/Settlement/Houses/";
        const string Docs = "Docs/Art/Settlement/Houses/";

        [Serializable] sealed class House
        {
            public int Level, Tier, GridWidth, GridDepth;
            public string Name, Front, Interior, GridGuides, FurnitureStudy;
            public float Width, Depth, Height, WallHeight, GridMetres, PlotWidth, PlotDepth;
            public float[] DoorAnchor, SelectAnchor, GroundAnchor, PetHomeAnchor, CollectionAnchor, HomeCameraAnchor;
        }
        [Serializable] sealed class Manifest { public House[] Houses; }
        static Manifest Load() => JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "HouseManifest.json"));
        static Vector3 V(float[] p) => new Vector3(p[0], p[1], p[2]);

        [MenuItem("The Trail Walker/Art/Build Eight House Levels")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            Directory.CreateDirectory(Target);
            var texture = (TextureImporter)AssetImporter.GetAtPath(Source + "HousePalette.png");
            texture.sRGBTexture = true; texture.alphaSource = TextureImporterAlphaSource.FromInput;
            texture.alphaIsTransparency = false; texture.filterMode = FilterMode.Point;
            texture.mipmapEnabled = false; texture.wrapMode = TextureWrapMode.Clamp;
            texture.textureCompression = TextureImporterCompression.Uncompressed;
            texture.npotScale = TextureImporterNPOTScale.None; texture.SaveAndReimport();
            var material = AssetDatabase.LoadAssetAtPath<Material>(Target + "HousePalette.mat");
            if (material == null)
            {
                material = new Material(Shader.Find(FacetedDetailMaterials.ShaderName));
                AssetDatabase.CreateAsset(material, Target + "HousePalette.mat");
            }
            FacetedDetailMaterials.Configure(material, AssetDatabase.LoadAssetAtPath<Texture2D>(Source + "HousePalette.png"));
            foreach (var path in Directory.GetFiles(Source, "*.obj"))
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false;
                importer.importLights = false; importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0;
                importer.SaveAndReimport();
            }
            foreach (var house in Load().Houses)
            {
                Save(Front(house, material), house.Front);
                Save(Interior(house, material, false), house.Interior);
                Save(Interior(house, material, true), $"HouseL{house.Level:00}FurnishedStudy");
            }
            foreach (var name in new[] { "PlayerPetHome", "PetCollectionContainer" })
            {
                var root = new GameObject(name); Part(root.transform, name, material); Save(root, name);
            }
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Temp");
            File.WriteAllText("Temp/HouseArt-build.result", "SUCCEEDED: 8 fronts, 8 interiors, 8 optional furnishing studies, 2 attachments. " + DateTime.UtcNow.ToString("O"));
            Debug.Log("Eight house levels built. Art only; gameplay and device testing remain pending.");
        }

        static HouseArtMetadata Metadata(GameObject root, House house, bool preview = false)
        {
            var data = root.AddComponent<HouseArtMetadata>();
            data.Level = house.Level; data.MaterialTier = house.Tier;
            data.FurnishingGrid = new Vector2Int(house.GridWidth, house.GridDepth);   // columns x rows toward the back
            data.GridCellMetres = house.GridMetres; data.PlotWidth = house.PlotWidth; data.PlotDepth = house.PlotDepth; data.PreviewOnly = preview;
            data.GridAxisX = new Vector3(-house.GridMetres, 0, 0); data.GridAxisDepth = new Vector3(0, 0, -house.GridMetres);
            return data;
        }
        static GameObject Front(House house, Material material)
        {
            var root = new GameObject(house.Front); Part(root.transform, house.Front, material); Metadata(root, house);
            Anchor(root.transform, "DoorAnchor", V(house.DoorAnchor));
            Anchor(root.transform, "SelectAnchor", V(house.SelectAnchor));
            Anchor(root.transform, "GroundAnchor", V(house.GroundAnchor));
            Anchor(root.transform, "PetHomeAnchor", V(house.PetHomeAnchor));
            Anchor(root.transform, "CollectionAnchor", V(house.CollectionAnchor));
            var eye = Anchor(root.transform, "HomeCameraAnchor", V(house.HomeCameraAnchor));
            eye.localRotation = Quaternion.LookRotation(new Vector3(0, house.Height * .43f, 0) - eye.localPosition);
            return root;
        }
        static GameObject Interior(House house, Material material, bool furnished)
        {
            var root = new GameObject(furnished ? $"HouseL{house.Level:00}FurnishedStudy" : house.Interior);
            Part(root.transform, house.Interior, material); Metadata(root, house, furnished);
            Anchor(root.transform, "GridOrigin", Vector3.zero);
            Anchor(root.transform, "GridXUnit", new Vector3(-house.GridMetres, 0, 0));
            // Depth growth (user, November 7, 2026): rows run toward the back; deeper rooms scroll front to back.
            float width = house.GridWidth * house.GridMetres, depth = house.GridDepth * house.GridMetres;
            Anchor(root.transform, "GridDepthUnit", new Vector3(0, 0, -house.GridMetres));
            Anchor(root.transform, "ScrollMin", Vector3.zero);
            Anchor(root.transform, "ScrollMax", new Vector3(0, 0, -depth));
            var eye = Anchor(root.transform, "InteriorCameraAnchor", InteriorEye(width, Mathf.Min(depth, 4.5f)));
            eye.localRotation = Quaternion.LookRotation(new Vector3(-width / 2, .3f, -Mathf.Min(depth, 4.5f) / 2) - eye.localPosition);
            var guides = Part(root.transform, house.GridGuides, material); guides.name = "PlacementGuides_PreviewOnly"; guides.SetActive(false);
            if (furnished) Part(root.transform, house.FurnitureStudy, material).name = "FurnitureProxies_PreviewOnly";
            return root;
        }
        // Front, raised, looking down about 50 degrees across the cut front edge toward the centre of the shown floor.
        static readonly Vector3 InteriorView = new Vector3(0, Mathf.Sin(50 * Mathf.Deg2Rad), Mathf.Cos(50 * Mathf.Deg2Rad));
        static Vector3 InteriorEye(float width, float shownDepth) => new Vector3(-width / 2, .3f, -shownDepth / 2) + InteriorView * (width * 1.15f + 2f);
        static Transform Anchor(Transform root, string name, Vector3 position)
        {
            var anchor = new GameObject(name).transform; anchor.SetParent(root, false); anchor.localPosition = position; return anchor;
        }
        static GameObject Part(Transform root, string name, Material material)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".obj");
            if (model == null) throw new InvalidOperationException("Missing house source " + name);
            var part = UnityEngine.Object.Instantiate(model, root, false); part.name = name;
            // OBJ import flips X. Restore the authored coordinate contract before adding anchors.
            part.transform.localScale = new Vector3(-1, 1, 1);
            foreach (var renderer in part.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
            return part;
        }
        static void Save(GameObject root, string name)
        {
            try { PrefabUtility.SaveAsPrefabAsset(root, Target + name + ".prefab"); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        static GameObject Prefab(string name) => AssetDatabase.LoadAssetAtPath<GameObject>(Target + name + ".prefab");

        [MenuItem("The Trail Walker/Art/Capture Eight House Levels")]
        public static void Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Capture outside Play mode.");
            Directory.CreateDirectory(Docs);
            foreach (var house in Load().Houses)
            {
                string prefix = $"house-l{house.Level:00}";
                var front = Prefab(house.Front);
                Render(front, prefix + "-front.png", new Vector3(0, 3.0f, 22), new Vector3(0, 3.0f, 0), 4.1f, 1000, 1100, true);
                // Three-quarter from the front-left so the long side wall and its windows show the house depth.
                Render(front, prefix + "-three-quarter.png", new Vector3(13, house.Height * .66f + 3, 11 - house.Depth * .4f),
                    new Vector3(0, house.Height * .40f, -house.Depth * .5f + .6f), .5f * Mathf.Sqrt(house.Width * house.Width + house.Depth * house.Depth) + 1.0f, 1200, 1200, true);
                float width = house.GridWidth * house.GridMetres, depth = house.GridDepth * house.GridMetres;
                foreach (bool furnished in new[] { false, true })
                {
                    var prefab = Prefab(furnished ? $"HouseL{house.Level:00}FurnishedStudy" : house.Interior);
                    var view = UnityEngine.Object.Instantiate(prefab);
                    try
                    {
                        if (!furnished) view.transform.Find("PlacementGuides_PreviewOnly").gameObject.SetActive(true);
                        // The whole floor at the same angle as InteriorCameraAnchor (review sheet; the game view scrolls).
                        var centre = new Vector3(-width/2, house.WallHeight * .35f, -depth/2);
                        float size = Mathf.Max((depth * InteriorView.y + house.WallHeight * InteriorView.z + 1.0f) / 2, (width + 1.0f) / 2);
                        Render(view, prefix + (furnished ? "-furnished-study.png" : "-interior-grid.png"),
                            centre + InteriorView * 20, centre, size, 1100, 1100, false);
                    }
                    finally { UnityEngine.Object.DestroyImmediate(view); }
                }
            }
            Lineup(); Frontage(3); Frontage(8); MixedStreet();
            File.WriteAllText("Temp/HouseArt-capture.result", "SUCCEEDED: house art review renders, not gameplay tests. " + DateTime.UtcNow.ToString("O"));
        }

        static void Lineup()
        {
            var root = new GameObject("Eight level lineup");
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    var view = UnityEngine.Object.Instantiate(Prefab($"HouseL{i+1:00}Front"), root.transform);
                    view.transform.localPosition = new Vector3((3.5f-i)*7.7f, 0, 0);
                }
                // Slightly raised side-on angle so the depth growth reads along the row, not just the fronts.
                Render(root, "eight-level-lineup.png", new Vector3(14, 9f, 30), new Vector3(0, 2.0f, -3.5f), 8.0f, 4800, 1100, true);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        static void Frontage(int level)
        {
            var root = new GameObject("Player frontage art study");
            try
            {
                var house = Load().Houses[level-1];
                UnityEngine.Object.Instantiate(Prefab(house.Front), root.transform);
                var pet = UnityEngine.Object.Instantiate(Prefab("PlayerPetHome"), root.transform); pet.transform.localPosition = V(house.PetHomeAnchor);
                var box = UnityEngine.Object.Instantiate(Prefab("PetCollectionContainer"), root.transform); box.transform.localPosition = V(house.CollectionAnchor);
                Render(root, $"house-l{level:00}-player-frontage.png", new Vector3(0, house.Height*.48f, 25),
                    new Vector3(0, house.Height*.48f, 0), Mathf.Max(3.8f, house.Height*.58f), 1800, 1300, true);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        static void MixedStreet()
        {
            // Presentation study: three houses, existing shops, three houses. No settlement scene is written.
            var root = new GameObject("Six plot street art study");
            try
            {
                float x = 53;
                foreach (int level in new[] { 1, 4, 8 })
                {
                    var house = UnityEngine.Object.Instantiate(Prefab($"HouseL{level:00}Front"), root.transform);
                    house.transform.localPosition = new Vector3(x, 0, 0); x -= 11.2f;
                }
                foreach (string shop in new[] { "Warehouse", "Bakery", "Ranch", "Farm", "Fishery", "Mine", "Tailor" })
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/Settlement/Storefronts/" + shop + "Front.prefab");
                    if (prefab == null) continue;
                    var view = UnityEngine.Object.Instantiate(prefab, root.transform); view.transform.localPosition = new Vector3(x, 0, 0); x -= 8;
                }
                // Reserve the lumber-field frontage; its final entrance art is a separate task.
                x -= 6;
                foreach (int level in new[] { 2, 6, 7 })
                {
                    var house = UnityEngine.Object.Instantiate(Prefab($"HouseL{level:00}Front"), root.transform);
                    house.transform.localPosition = new Vector3(x, 0, 0); x -= 11.2f;
                }
                float centre = (53 + x + 11.2f)/2;
                // The street (three houses, seven shops, the lumber gap, three houses) is about 130 m long. At a 10:1
                // frame an orthographic size of 5.1 covered only 102 m and cut off the outer house on each side.
                float span = 53 - (x + 11.2f) + 11.2f + 8f; // street length plus a margin
                Render(root, "six-plot-street-study.png", new Vector3(centre,4.55f,40),new Vector3(centre,4.55f,0),Mathf.Max(5.1f, span / 20f),6000,600,true);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        static void Render(GameObject source, string file, Vector3 eye, Vector3 target, float size, int width, int height, bool ground)
        {
            var preview = new PreviewRenderUtility(); Material groundMaterial = null;
            try
            {
                preview.AddSingleGO(UnityEngine.Object.Instantiate(source));
                if (ground)
                {
                    var plane = GameObject.CreatePrimitive(PrimitiveType.Plane); UnityEngine.Object.DestroyImmediate(plane.GetComponent<Collider>());
                    groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")); groundMaterial.SetColor("_BaseColor",new Color(.40f,.47f,.34f));
                    plane.GetComponent<Renderer>().sharedMaterial = groundMaterial;
                    plane.transform.position = new Vector3(target.x,-.025f,-2); plane.transform.localScale = new Vector3(20,1,4); preview.AddSingleGO(plane);
                }
                var camera = preview.camera; camera.orthographic = true; camera.orthographicSize = size;
                camera.transform.position = eye; camera.transform.LookAt(target);
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = ground ? new Color(.67f,.76f,.79f) : new Color(.19f,.22f,.23f);
                camera.nearClipPlane = .02f; camera.farClipPlane = 160;
                preview.lights[0].intensity = 1.2f; preview.lights[0].transform.rotation = Quaternion.Euler(40,210,0);
                preview.lights[1].intensity = .6f; preview.ambientColor = new Color(.6f,.6f,.59f);
                preview.BeginStaticPreview(new Rect(0,0,width,height)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs + file, image.EncodeToPNG()); }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); if (groundMaterial != null) UnityEngine.Object.DestroyImmediate(groundMaterial); }
        }
    }
}
