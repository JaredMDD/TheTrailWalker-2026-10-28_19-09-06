using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Harvest;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Massive lumber-field trees (Claude, November 8, 2026; PROJECT_RECOMENDATIONS.md section 13.4). Builds
    // Resources/Settlement/LumberField/Massive<Species><Variant>.prefab from Tools/Art/generate_massive_trees.py and
    // renders the review sheets. Art only: lumber field placement, rewards and timers belong to Milestone 13.
    public static class MassiveTreeAssets
    {
        public const string Source = "Assets/_Game/Art/Harvest/Massive/";
        public const string Target = "Assets/_Game/Resources/Settlement/LumberField/";
        const string Docs = "Docs/Art/Settlement/LumberField/";
        public static readonly string[] Species = { "Pine", "Birch", "Mesquite", "Oak" };
        public static readonly string[] Variants = { "A", "B", "C" };

        [Serializable] sealed class Selection { public float[] Centre, Size; }
        [Serializable] sealed class Tree
        {
            public string Name, Species, Variant, LowerMesh, UpperMesh; public int WoodTier, LowerTriangles, UpperTriangles;
            public float CutHeight, CutRadius, BaseRadius, Height, CrownRadius; public float[] CutArea, AxeHint; public Selection Selection;
        }
        [Serializable] sealed class Manifest { public string Palette; public Tree[] Trees; }

        static Manifest Load() => JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "MassiveTreesManifest.json"));
        static Vector3 V(float[] a) => new Vector3(a[0], a[1], a[2]);

        static Material Material()
        {
            string path = Source + "MassiveTreesPalette.mat";
            var palette = AssetDatabase.LoadAssetAtPath<Texture2D>(Source + "MassiveTreesPalette.png");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            mat.SetTexture("_BaseMap", palette); mat.SetColor("_BaseColor", Color.white); mat.SetFloat("_Smoothness", .05f);
            FacetedDetailMaterials.Configure(mat, palette); EditorUtility.SetDirty(mat);
            return mat;
        }

        [MenuItem("The Trail Walker/Art/Build Massive Trees")]
        public static void Build()
        {
            var manifest = Load(); var material = Material();
            Directory.CreateDirectory(Target);
            foreach (var t in manifest.Trees)
            {
                var root = new GameObject(t.Name);
                try
                {
                    var rig = root.AddComponent<Tier1PineRig>();
                    rig.LowerTrunk = Pivot(root.transform, "LowerTrunk", Vector3.zero); Part(rig.LowerTrunk, t.LowerMesh, material);
                    rig.FallPivot = Pivot(root.transform, "FallPivot", new Vector3(0, t.CutHeight, 0)); Part(rig.FallPivot, t.UpperMesh, material);
                    rig.CutArea = Pivot(root.transform, "CutArea", V(t.CutArea));
                    rig.HintAnchor = Pivot(root.transform, "AxeHintAnchor", V(t.AxeHint));
                    var visual = root.AddComponent<Tier1HarvestVisual>(); visual.Kind = InteractableKind.Tree;
                    var box = root.AddComponent<BoxCollider>(); box.center = V(t.Selection.Centre); box.size = V(t.Selection.Size); box.enabled = false;
                    visual.SelectionBounds = box; visual.FocusAnchor = rig.CutArea; visual.HintAnchor = rig.HintAnchor;
                    var art = root.AddComponent<MassiveTreeArt>();
                    art.Species = t.Species; art.Variant = t.Variant; art.WoodTier = t.WoodTier;
                    art.CutHeight = t.CutHeight; art.CutRadius = t.CutRadius; art.Height = t.Height; art.CrownRadius = t.CrownRadius;
                    PrefabUtility.SaveAsPrefabAsset(root, Target + t.Name + ".prefab");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Massive trees: {manifest.Trees.Length} prefabs.");
        }

        static Transform Pivot(Transform parent, string name, Vector3 position)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = position; return t;
        }
        static void Part(Transform parent, string mesh, Material material)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source + mesh + ".obj");
            if (source == null) throw new InvalidOperationException("Missing massive tree mesh " + mesh);
            var part = UnityEngine.Object.Instantiate(source, parent, false); part.name = mesh;
            foreach (var r in part.GetComponentsInChildren<MeshRenderer>()) r.sharedMaterial = material;
        }

        public static GameObject Prefab(string species, string variant) => AssetDatabase.LoadAssetAtPath<GameObject>(Target + "Massive" + species + variant + ".prefab");

        // Contract checks; returns the passed check names (throws on the first failure).
        public static List<string> Validate()
        {
            var passed = new List<string>();
            void Check(bool ok, string name) { if (!ok) throw new Exception("Massive trees: " + name); passed.Add(name); }
            var manifest = Load();
            Check(manifest.Trees.Length == 12, "four woods x three trunk shapes");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Source + "MassiveTreesPalette.mat");
            Check(mat != null && mat.shader.name == FacetedDetailMaterials.ShaderName, "the palette material uses the surface-detail shader");
            foreach (var t in manifest.Trees)
            {
                var prefab = Prefab(t.Species, t.Variant);
                Check(prefab != null, t.Name + ": prefab exists");
                var rig = prefab.GetComponent<Tier1PineRig>();
                Check(rig != null && rig.LowerTrunk != null && rig.FallPivot != null && rig.CutArea != null && rig.HintAnchor != null, t.Name + ": chopping rig points are wired");
                Check(Mathf.Abs(rig.FallPivot.localPosition.y - t.CutHeight) < .001f, t.Name + ": the fall pivot sits at the cut");
                var art = prefab.GetComponent<MassiveTreeArt>();
                Check(art != null && art.WoodTier == Array.IndexOf(Species, t.Species) + 1, t.Name + ": wood tier metadata");
                var visual = prefab.GetComponent<Tier1HarvestVisual>();
                Check(visual != null && visual.Kind == InteractableKind.Tree && visual.SelectionBounds != null && !visual.SelectionBounds.enabled, t.Name + ": selection bounds (disabled until the lumber field enables them)");
                Check(t.LowerTriangles + t.UpperTriangles <= 3200, t.Name + ": at most 3,200 triangles");
                foreach (var f in prefab.GetComponentsInChildren<MeshFilter>())
                {
                    Check(f.sharedMesh != null, t.Name + ": " + f.name + " mesh imported");
                    Check(f.GetComponent<MeshRenderer>().sharedMaterial == mat, t.Name + ": " + f.name + " uses the massive tree palette");
                }
                var lower = rig.LowerTrunk.GetComponentInChildren<MeshFilter>().sharedMesh.bounds;
                Check(lower.max.y <= t.CutHeight + .01f, t.Name + ": the stump ends at the cut");
                Check(lower.min.y > -.3f, t.Name + ": roots sink at most 30 cm into the ground");
                var upper = rig.FallPivot.GetComponentInChildren<MeshFilter>().sharedMesh.bounds;
                Check(upper.min.y > -.05f, t.Name + ": the falling part starts at the cut");
            }
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Massive-tree-checks.txt", passed);
            return passed;
        }

        // Review: per wood, the three shapes standing plus shape A felled (stump and fallen crown); and a lineup of all four.
        [MenuItem("The Trail Walker/Art/Capture Massive Trees")]
        public static void Capture()
        {
            Directory.CreateDirectory(Docs);
            foreach (var species in Species)
            {
                var items = new List<(string variant, bool felled, float x)>();
                float x = 0;
                foreach (var v in Variants) { items.Add((v, false, x)); x += Spacing(species); }
                items.Add(("A", true, x));
                Render(Docs + species.ToLowerInvariant() + "-shapes.png", items, species, 1800, 900, false);
                Render(Docs + species.ToLowerInvariant() + "-stump-closeup.png", new List<(string, bool, float)> { ("A", true, 0) }, species, 900, 900, true);
            }
            var line = new List<(string, string, bool, float)>(); float px = 0;
            foreach (var s in Species) { line.Add((s, "A", false, px)); px += Spacing(s); }
            RenderLineup(Docs + "all-woods-lineup.png", line);
        }

        static float Spacing(string species) => species == "Oak" ? 15f : species == "Mesquite" ? 13f : 10f;

        static GameObject Place(PreviewRenderUtility preview, string species, string variant, bool felled, float x, bool stumpOnly = false)
        {
            var go = UnityEngine.Object.Instantiate(Prefab(species, variant));
            go.transform.position = new Vector3(x, 0, 0);
            if (felled) go.GetComponent<Tier1PineRig>().FallPivot.localRotation = Quaternion.Euler(85, 0, 0);   // falls away from the camera
            if (stumpOnly) go.GetComponent<Tier1PineRig>().FallPivot.gameObject.SetActive(false);   // the regrowth stump
            preview.AddSingleGO(go); return go;
        }

        static void Setup(PreviewRenderUtility preview, Vector3 target, float size, float yaw, float pitch)
        {
            var cam = preview.camera; cam.orthographic = true; cam.orthographicSize = size;
            cam.transform.position = target + Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -60); cam.transform.LookAt(target);
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.62f, .72f, .78f); cam.nearClipPlane = .5f; cam.farClipPlane = 200;
            preview.lights[0].intensity = 1.25f; preview.lights[0].transform.rotation = Quaternion.Euler(40, 150, 0);
            preview.lights[1].intensity = .55f; preview.ambientColor = new Color(.58f, .6f, .6f);
        }

        // A 1.75 m stand-in person for scale (review only).
        static void Figure(PreviewRenderUtility preview, float x)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Capsule); g.transform.position = new Vector3(x, .875f, -1.5f); g.transform.localScale = new Vector3(.45f, .875f, .45f);
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.SetColor("_BaseColor", new Color(.85f, .45f, .2f)); g.GetComponent<MeshRenderer>().sharedMaterial = m;
            preview.AddSingleGO(g);
        }

        static GameObject Ground(PreviewRenderUtility preview, float width, float centre)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Plane); g.transform.position = new Vector3(centre, 0, 0); g.transform.localScale = new Vector3(width / 10, 1, 4);
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.SetColor("_BaseColor", new Color(.42f, .5f, .3f)); g.GetComponent<MeshRenderer>().sharedMaterial = m;
            preview.AddSingleGO(g); return g;
        }

        static void Save(PreviewRenderUtility preview, string path, int w, int h)
        {
            preview.BeginStaticPreview(new Rect(0, 0, w, h)); preview.Render(true);
            var image = preview.EndStaticPreview();
            try { File.WriteAllBytes(path, image.EncodeToPNG()); } finally { UnityEngine.Object.DestroyImmediate(image); }
        }

        static void Render(string path, List<(string variant, bool felled, float x)> items, string species, int w, int h, bool closeup)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                float last = items[items.Count - 1].x, centre = last / 2;
                Ground(preview, last + 30, centre);
                foreach (var it in items) Place(preview, species, it.variant, it.felled, it.x, closeup);
                if (!closeup) foreach (var it in items) Figure(preview, it.x + 2.4f);
                if (closeup) Setup(preview, new Vector3(0, 1.0f, 0), 2.6f, 25, 32);
                else Setup(preview, new Vector3(centre, 6.5f, 0), Mathf.Max(8.5f, (last + 16) * h / w / 2f), 12, 12);
                Save(preview, path, w, h);
            }
            finally { preview.Cleanup(); }
        }

        static void RenderLineup(string path, List<(string species, string variant, bool felled, float x)> items)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                float last = items[items.Count - 1].x, centre = last / 2;
                Ground(preview, last + 30, centre);
                foreach (var it in items) Place(preview, it.species, it.variant, it.felled, it.x);
                foreach (var it in items) Figure(preview, it.x + 2.4f);
                Setup(preview, new Vector3(centre, 6.5f, 0), 11f, 12, 10);
                Save(preview, path, 2000, 900);
            }
            finally { preview.Cleanup(); }
        }
    }

    sealed class MassiveTreeImport : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(MassiveTreeAssets.Source, StringComparison.Ordinal)) return;
            var m = (ModelImporter)assetImporter; m.globalScale = 1; m.importAnimation = false; m.importCameras = false; m.importLights = false;
            m.addCollider = false; m.importNormals = ModelImporterNormals.Calculate; m.normalSmoothingAngle = 0; m.materialImportMode = ModelImporterMaterialImportMode.None;
        }
    }
}
