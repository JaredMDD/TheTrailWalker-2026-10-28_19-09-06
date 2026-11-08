using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Store furniture art (section 13.2), built from Tools/Art/generate_furniture.py. Art and review only: no store,
    // buff, placement or save code. Batch A (November 7, 2026): level 1; batch B (same day): levels 2-3.
    // Chests get a LidPivot at the back top edge with the lid under it, so the touch action can open it.
    public static class FurnitureArtAssets
    {
        const string Source = "Assets/_Game/Art/Settlement/Furniture/";
        const string Target = "Assets/_Game/Resources/Settlement/Furniture/";
        const string Icons = "Assets/_Game/Resources/UI/Furniture/";
        const string Docs = "Docs/Art/Settlement/Furniture/";
        const string HouseMaterial = "Assets/_Game/Resources/Settlement/Houses/HousePalette.mat";

        [Serializable] public sealed class Piece
        {
            public string Id, Name, Asset, Category, LidAsset; public int Level, FootprintWidth, FootprintDepth, PriceCash, Triangles; public bool IsChest;
            public float[] BoundsMin, BoundsMax, LidHinge;
        }
        [Serializable] sealed class Manifest { public float GridMetres; public Piece[] Furniture; }
        static Manifest Load() => JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "FurnitureManifest.json"));

        [MenuItem("The Trail Walker/Art/Build Furniture")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var material = AssetDatabase.LoadAssetAtPath<Material>(HouseMaterial);
            if (material == null) throw new InvalidOperationException("Build the houses first (shared HousePalette material).");
            Directory.CreateDirectory(Target);
            foreach (var path in Directory.GetFiles(Source, "*.obj"))
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
                importer.addCollider = false; importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0; importer.SaveAndReimport();
            }
            var manifest = Load();
            foreach (var piece in manifest.Furniture)
            {
                var root = new GameObject(piece.Asset);
                try
                {
                    Part(root.transform, piece.Asset, material);
                    if (!string.IsNullOrEmpty(piece.LidAsset))
                    {
                        var hinge = new GameObject("LidPivot").transform; hinge.SetParent(root.transform, false);
                        hinge.localPosition = new Vector3(piece.LidHinge[0], piece.LidHinge[1], piece.LidHinge[2]);
                        Part(hinge, piece.LidAsset, material);   // closed; rotate LidPivot about -X to open
                    }
                    var data = root.AddComponent<FurnitureArtMetadata>();
                    data.FurnitureId = piece.Id; data.DisplayName = piece.Name; data.BuffCategory = piece.Category; data.HouseLevel = piece.Level;
                    data.PriceCash = piece.PriceCash; data.Footprint = new Vector2Int(piece.FootprintWidth, piece.FootprintDepth);
                    data.GridCellMetres = manifest.GridMetres; data.IsChest = piece.IsChest;
                    // Selection volume for the hold (or chest touch) gesture: the footprint box up to the model height.
                    var box = root.AddComponent<BoxCollider>();
                    float w = piece.FootprintWidth * manifest.GridMetres, d = piece.FootprintDepth * manifest.GridMetres, h = Mathf.Max(.3f, piece.BoundsMax[1]);
                    box.center = new Vector3(-w / 2, h / 2, -d / 2); box.size = new Vector3(w, h, d);
                    PrefabUtility.SaveAsPrefabAsset(root, Target + piece.Asset + ".prefab");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log($"Furniture built: {manifest.Furniture.Length} prefabs. Art only; store, buffs and placement are not implemented.");
        }

        static GameObject Part(Transform root, string name, Material material)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".obj");
            if (model == null) throw new InvalidOperationException("Missing furniture source " + name);
            var part = UnityEngine.Object.Instantiate(model, root, false); part.name = name;
            part.transform.localScale = new Vector3(-1, 1, 1);   // OBJ import flips X (same contract as the houses)
            foreach (var renderer in part.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
            return part;
        }

        // Batch contract checks (section 13.2): returns the list of passed checks, throws on the first failure.
        public static List<string> Validate()
        {
            var passed = new List<string>();
            void Check(bool ok, string name) { if (!ok) throw new Exception("Furniture art: " + name); passed.Add(name); }
            var manifest = Load();
            var ids = new HashSet<string>(); var perLevel = new Dictionary<int, int>();
            foreach (var p in manifest.Furniture)
            {
                Check(ids.Add(p.Id) && p.Id.StartsWith($"furniture.l{p.Level}.", StringComparison.Ordinal), $"{p.Id}: unique ID with its house level");
                perLevel[p.Level] = (perLevel.TryGetValue(p.Level, out int n) ? n : 0) + 1;
                Check(p.FootprintWidth >= 1 && p.FootprintDepth >= 1 && p.FootprintWidth * p.FootprintDepth <= 6 && Mathf.Max(p.FootprintWidth, p.FootprintDepth) <= 3,
                    $"{p.Name}: footprint {p.FootprintWidth}x{p.FootprintDepth} within 3x2 (15% of the level-1 room)");
                Check(p.PriceCash > 0, $"{p.Name}: has a store price ({p.PriceCash:N0} cash) and no recipe");
                Check(p.Triangles <= 600, $"{p.Name}: {p.Triangles} triangles within the 600 budget");
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Target + p.Asset + ".prefab");
                Check(prefab != null && prefab.GetComponent<FurnitureArtMetadata>() is FurnitureArtMetadata m && m.FurnitureId == p.Id && m.Footprint == new Vector2Int(p.FootprintWidth, p.FootprintDepth) && m.IsChest == p.IsChest,
                    $"{p.Name}: prefab carries its metadata");
                if (p.IsChest) Check(prefab.transform.Find("LidPivot") is Transform lid && lid.childCount == 1, $"{p.Name}: chest has a hinged lid (LidPivot) for the touch-to-open action");
                var view = UnityEngine.Object.Instantiate(prefab);
                try
                {
                    var bounds = new Bounds(); bool first = true;
                    foreach (var r in view.GetComponentsInChildren<Renderer>()) { if (first) { bounds = r.bounds; first = false; } else bounds.Encapsulate(r.bounds); }
                    float w = p.FootprintWidth * manifest.GridMetres, d = p.FootprintDepth * manifest.GridMetres;
                    Check(bounds.min.x >= -w - .011f && bounds.max.x <= .011f && bounds.min.z >= -d - .011f && bounds.max.z <= .011f && bounds.min.y >= -.002f && bounds.max.y <= 2.0f,
                        $"{p.Name}: mesh inside its {w:0.0} x {d:0.0} m footprint, on the floor, {bounds.max.y:0.00} m tall (max 2.0)");
                }
                finally { UnityEngine.Object.DestroyImmediate(view); }
            }
            foreach (var pair in perLevel) Check(pair.Value == 6, $"House level {pair.Key} has six furniture types");
            foreach (var pair in perLevel)
            {
                var wood = new[] { "", "Pine", "Pine", "Birch", "Birch", "Mesquite", "Mesquite", "Oak", "Oak" }[pair.Key];
                var cats = new HashSet<string>(); foreach (var p in manifest.Furniture) if (p.Level == pair.Key) cats.Add(p.Category);
                Check(cats.Count == 6, $"House level {pair.Key}: six different buff categories ({wood} level)");
            }
            // Every type owned so far must fit in its room at once (buffs only count while placed).
            var rooms = new Dictionary<int, Vector2Int> { { 1, new Vector2Int(8, 5) }, { 2, new Vector2Int(10, 6) }, { 3, new Vector2Int(12, 7) }, { 4, new Vector2Int(12, 9) },
                { 5, new Vector2Int(12, 11) }, { 6, new Vector2Int(12, 13) }, { 7, new Vector2Int(12, 15) }, { 8, new Vector2Int(12, 17) } };
            foreach (var level in perLevel.Keys)
            {
                var owned = Array.FindAll(manifest.Furniture, p => p.Level <= level);
                Check(Layout(owned, rooms[level], out _), $"All {owned.Length} types owned at level {level} fit its {rooms[level].x}x{rooms[level].y} room together");
            }
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Furniture-art-checks.txt", passed);
            return passed;
        }

        // The confirmed "nearest free space" rule: positions nearest the door (front centre), searched row by row from the
        // front; whole cells; no overlap. Places pieces in catalog order. Returns false if one does not fit.
        public static bool Layout(Piece[] pieces, Vector2Int room, out Vector2Int[] cells)
        {
            var used = new bool[room.x, room.y]; cells = new Vector2Int[pieces.Length];
            for (int i = 0; i < pieces.Length; i++)
            {
                int fw = pieces[i].FootprintWidth, fd = pieces[i].FootprintDepth; bool placed = false;
                for (int z = 0; z + fd <= room.y && !placed; z++)
                {
                    var xs = new List<int>(); for (int x = 0; x + fw <= room.x; x++) xs.Add(x);
                    xs.Sort((a, b) => Mathf.Abs(a + fw / 2f - room.x / 2f).CompareTo(Mathf.Abs(b + fw / 2f - room.x / 2f)));
                    foreach (int x in xs)
                    {
                        bool free = true;
                        for (int dx = 0; dx < fw && free; dx++) for (int dz = 0; dz < fd && free; dz++) free = !used[x + dx, z + dz];
                        if (!free) continue;
                        for (int dx = 0; dx < fw; dx++) for (int dz = 0; dz < fd; dz++) used[x + dx, z + dz] = true;
                        cells[i] = new Vector2Int(x, z); placed = true; break;
                    }
                }
                if (!placed) return false;
            }
            return true;
        }

        [MenuItem("The Trail Walker/Art/Capture Furniture")]
        public static void Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Capture outside Play mode.");
            Directory.CreateDirectory(Docs); Directory.CreateDirectory(Icons);
            var manifest = Load(); var pieces = manifest.Furniture; float cell = manifest.GridMetres;
            var levels = new SortedSet<int>(); foreach (var p in pieces) levels.Add(p.Level);
            var rooms = new[] { Vector2Int.zero, new Vector2Int(8, 5), new Vector2Int(10, 6), new Vector2Int(12, 7), new Vector2Int(12, 9), new Vector2Int(12, 11), new Vector2Int(12, 13), new Vector2Int(12, 15), new Vector2Int(12, 17) };
            foreach (int level in levels)
            {
                var ofLevel = Array.FindAll(pieces, p => p.Level == level);
                // 1. Lineup of the level's six pieces at three-quarter view, each on its footprint plate.
                var line = new GameObject("Furniture lineup"); float x = 0;
                try
                {
                    foreach (var p in ofLevel)
                    {
                        var v = UnityEngine.Object.Instantiate(Prefab(p), line.transform);
                        v.transform.localPosition = new Vector3(-x, 0, p.FootprintDepth * cell / 2);
                        Plate(line.transform, v.transform.localPosition, p, cell);
                        x += p.FootprintWidth * cell + .55f;
                    }
                    var centre = new Vector3(-(x - .55f) / 2, .6f, 0);
                    Render(line, Docs + $"level{level}-lineup.png", centre + new Vector3(4, 5, 9), centre, Mathf.Max(1.5f, x * .24f), 2400, 900, new Color(.19f, .22f, .23f));
                }
                finally { UnityEngine.Object.DestroyImmediate(line); }
                // 2. The level's room furnished with every type owned so far (nearest-free-space rule), angled and bird's-eye.
                var room = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Game/Resources/Settlement/Houses/HouseL{level:00}Interior.prefab");
                var owned = Array.FindAll(pieces, p => p.Level <= level);
                if (room != null && Layout(owned, rooms[level], out var cells))
                {
                    var scene = UnityEngine.Object.Instantiate(room);
                    try
                    {
                        for (int i = 0; i < owned.Length; i++)
                        {
                            var v = UnityEngine.Object.Instantiate(Prefab(owned[i]), scene.transform);
                            v.transform.localPosition = new Vector3(-cells[i].x * cell, 0, -cells[i].y * cell);
                        }
                        float w = rooms[level].x * cell, d = rooms[level].y * cell;
                        var centre = new Vector3(-w / 2, .4f, -d / 2);
                        var angled = new Vector3(0, Mathf.Sin(50 * Mathf.Deg2Rad), Mathf.Cos(50 * Mathf.Deg2Rad));
                        Render(scene, Docs + $"level{level}-room-angled.png", centre + angled * 20, centre, Mathf.Max(w * .33f, d * .45f + 1.2f), 1400, 1100, new Color(.19f, .22f, .23f));
                        var guides = scene.transform.Find("PlacementGuides_PreviewOnly"); if (guides != null) guides.gameObject.SetActive(true);
                        Render(scene, Docs + $"level{level}-room-birds-eye.png", new Vector3(-w / 2, 20, -d / 2), new Vector3(-w / 2, 0, -d / 2 - .0001f), Mathf.Max(w * .32f, d * .52f), 1400, 1100, new Color(.19f, .22f, .23f));
                    }
                    finally { UnityEngine.Object.DestroyImmediate(scene); }
                }
            }
            // 3. Store icons: 256 px, transparent, three-quarter view framed on each piece; one contact sheet per level.
            foreach (var p in pieces)
            {
                var v = UnityEngine.Object.Instantiate(Prefab(p));
                try
                {
                    float w = p.FootprintWidth * cell, d = p.FootprintDepth * cell, h = p.BoundsMax[1];
                    var centre = new Vector3(-w / 2, h / 2, -d / 2);
                    float size = Mathf.Max(h * .62f, Mathf.Max(w, d) * .62f) + .05f;
                    Render(v, Icons + p.Asset + ".png", centre + new Vector3(-6, 4.5f, 8).normalized * 12, centre, size, 256, 256, new Color(0, 0, 0, 0));
                }
                finally { UnityEngine.Object.DestroyImmediate(v); }
            }
            foreach (int level in levels) IconSheet(Array.FindAll(pieces, p => p.Level == level), Docs + $"level{level}-store-icons.png");
            AssetDatabase.Refresh();
            Debug.Log("Furniture review renders and store icons written.");
        }

        static void IconSheet(Piece[] pieces, string file)
        {
            var sheet = new Texture2D(16 + pieces.Length * 270, 300, TextureFormat.RGBA32, false);
            var paper = new Color(.913f, .886f, .831f, 1); var fill = new Color[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++) fill[i] = paper; sheet.SetPixels(fill);
            for (int n = 0; n < pieces.Length; n++)
            {
                var icon = new Texture2D(2, 2); icon.LoadImage(File.ReadAllBytes(Icons + pieces[n].Asset + ".png"));
                for (int y = 0; y < icon.height; y++)
                    for (int x = 0; x < icon.width; x++)
                    {
                        var c = icon.GetPixel(x, y); int sx = 16 + n * 270 + x, sy = 22 + y;
                        sheet.SetPixel(sx, sy, Color.Lerp(paper, new Color(c.r, c.g, c.b, 1), c.a));
                    }
                UnityEngine.Object.DestroyImmediate(icon);
            }
            sheet.Apply(); File.WriteAllBytes(file, sheet.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(sheet);
        }

        static GameObject Prefab(Piece p) => AssetDatabase.LoadAssetAtPath<GameObject>(Target + p.Asset + ".prefab");

        static void Plate(Transform parent, Vector3 at, Piece p, float cell)
        {
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube); UnityEngine.Object.DestroyImmediate(plate.GetComponent<Collider>());
            plate.transform.SetParent(parent, false);
            plate.transform.localScale = new Vector3(p.FootprintWidth * cell, .01f, p.FootprintDepth * cell);
            plate.transform.localPosition = at + new Vector3(-p.FootprintWidth * cell / 2, -.006f, -p.FootprintDepth * cell / 2);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.SetColor("_BaseColor", new Color(.36f, .40f, .37f));
            plate.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static void Render(GameObject source, string file, Vector3 eye, Vector3 target, float size, int width, int height, Color background)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                preview.AddSingleGO(UnityEngine.Object.Instantiate(source));
                var camera = preview.camera; camera.orthographic = true; camera.orthographicSize = size;
                camera.transform.position = eye; // Straight-down views keep the back wall at the top of the screen (door at the bottom), +X screen-left as elsewhere.
                camera.transform.LookAt(target, Mathf.Abs(Vector3.Dot((target - eye).normalized, Vector3.up)) > .99f ? Vector3.back : Vector3.up);
                // The preview render has no alpha channel: transparent requests render on a magenta key that is removed below.
                bool transparent = background.a < 1f;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = transparent ? Color.magenta : background;
                camera.nearClipPlane = .02f; camera.farClipPlane = 60;
                preview.lights[0].intensity = 1.2f; preview.lights[0].transform.rotation = Quaternion.Euler(45, 200, 0);
                preview.lights[1].intensity = .6f; preview.ambientColor = new Color(.6f, .6f, .59f);
                preview.BeginStaticPreview(new Rect(0, 0, width, height)); preview.Render(true);
                var image = preview.EndStaticPreview();
                Texture2D output = image;
                if (transparent)
                {
                    output = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
                    var pixels = image.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        var c = pixels[i];
                        // Distance from magenta: full key -> transparent; edge blends get partial alpha with the tint removed.
                        float key = Mathf.Clamp01(Mathf.Min(c.r, c.b) - c.g);
                        float alpha = Mathf.Clamp01(1f - key * 1.15f);
                        if (alpha <= .02f) { pixels[i] = new Color(0, 0, 0, 0); continue; }
                        float spill = Mathf.Min(c.r, c.b) - c.g; if (spill > 0) { c.r -= spill * (1 - alpha); c.b -= spill * (1 - alpha); }
                        pixels[i] = new Color(c.r, c.g, c.b, alpha);
                    }
                    output.SetPixels(pixels); output.Apply();
                }
                try { File.WriteAllBytes(file, output.EncodeToPNG()); }
                finally { if (output != image) UnityEngine.Object.DestroyImmediate(output); UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
