using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TheTrailWalker.Travel;

namespace TheTrailWalker.Editor
{
    // Tier-two camp (November 6, 2026): the burning birch campfire, registered in the camp prop catalog for regions
    // 7-12. Seats are the existing BirchLogSeat. Source: Tools/Art/generate_tier02_camp_meshes.py.
    public static class Tier02CampAssets
    {
        private const string Source = "Assets/_Game/Art/Environment/CampProps/Tier02/";
        private const string Target = "Assets/_Game/Resources/Environment/CampProps/Tier02/";
        private const string Props = "Assets/_Game/Resources/Environment/CampProps/";
        private static readonly string[] Parts = { "BirchCampfireWood", "BirchCampfireFlames" };

        [MenuItem("The Trail Walker/Art/Build Tier 2 Camp")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            Region01ResourceAssets.BuildRegion(Source, Target, "Tier02CampPalette", "Tier02Camp", Parts);
            var palette = AssetDatabase.LoadAssetAtPath<Texture2D>(Source + "Tier02CampPalette.png");
            // Flames glow evenly: unlit palette colours, no shadows.
            string flamePath = Target + "Tier02CampFlame.mat";
            var flame = AssetDatabase.LoadAssetAtPath<Material>(flamePath);
            if (flame == null) { flame = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(flame, flamePath); }
            flame.SetTexture("_BaseMap", palette); flame.SetColor("_BaseColor", Color.white); flame.enableInstancing = true; EditorUtility.SetDirty(flame);

            var root = new GameObject("BirchCampfire");
            try
            {
                GameObject wood = Unpacked(Parts[0], root.transform), flames = Unpacked(Parts[1], root.transform);
                wood.name = "Wood"; flames.name = "Flames";
                foreach (var r in flames.GetComponentsInChildren<MeshRenderer>())
                {
                    r.sharedMaterial = flame; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                }
                foreach (var r in wood.GetComponentsInChildren<MeshRenderer>()) { r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; r.receiveShadows = true; }
                root.AddComponent<CampfireFlicker>().Flames = flames.transform;
                Anchor(root.transform, "GroundAnchor", Vector3.zero); Anchor(root.transform, "FireCentre", new Vector3(0, .12f, 0));
                PrefabUtility.SaveAsPrefabAsset(root, Target + "BirchCampfire.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }

            var catalog = AssetDatabase.LoadAssetAtPath<CampPropArtCatalog>(Props + "CampPropArtCatalog.asset");
            if (catalog == null) throw new InvalidOperationException("Build the camp props first (Rebuild Campfire and Log Seat Meshes).");
            var fires = (catalog.Campfires ?? new CampfireArt[0]).Where(f => f != null && f.WoodTier != 2).ToList();
            fires.Add(new CampfireArt { Wood = "Birch", WoodTier = 2, FirstRegion = 7, LastRegion = 12, Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Target + "BirchCampfire.prefab") });
            catalog.Campfires = fires.OrderBy(f => f.WoodTier).ToArray();
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            File.WriteAllText("Temp/Tier02Camp.result", "SUCCEEDED: BirchCampfire for regions 7-12 with unlit flickering flames; seats BirchLogSeat.");
        }

        private static GameObject Unpacked(string name, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Target + name + ".prefab");
            if (prefab == null) throw new InvalidOperationException("Missing " + name);
            var view = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            PrefabUtility.UnpackPrefabInstance(view, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            return view;
        }

        private static void Anchor(Transform root, string name, Vector3 position)
        {
            var anchor = new GameObject(name).transform; anchor.SetParent(root, false); anchor.localPosition = position;
        }

        [MenuItem("The Trail Walker/Art/Capture Tier 2 Camp")]
        public static void Capture()
        {
            Directory.CreateDirectory("Docs/Art/Environment/CampProps/Tier02");
            Render("birch-campfire", false, 1.0f, new Vector3(1.6f, 1.6f, -2.4f), new Vector3(0, .35f, 0), 1100, 900);
            Render("tier2-camp-ring", true, 3.0f, new Vector3(5f, 5.5f, -7f), new Vector3(0, .3f, 0), 1400, 1000);
        }

        private static void Render(string file, bool ring, float size, Vector3 eye, Vector3 target, int width, int height)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                preview.AddSingleGO(UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Target + "BirchCampfire.prefab")));
                if (ring)
                {
                    var seat = AssetDatabase.LoadAssetAtPath<GameObject>(Props + "BirchLogSeat.prefab");
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 position = Quaternion.AngleAxis(i * 90f + 45f, Vector3.up) * Vector3.forward * TravelScenery.SeatRadius;
                        preview.AddSingleGO(UnityEngine.Object.Instantiate(seat, position, Quaternion.LookRotation(-position)));
                    }
                }
                preview.camera.orthographic = true; preview.camera.orthographicSize = size;
                preview.camera.transform.position = eye; preview.camera.transform.LookAt(target);
                preview.camera.clearFlags = CameraClearFlags.SolidColor; preview.camera.backgroundColor = new Color(.20f, .23f, .25f);
                preview.camera.nearClipPlane = .01f; preview.camera.farClipPlane = 40;
                preview.lights[0].intensity = 1.3f; preview.lights[0].transform.rotation = Quaternion.Euler(45, 30, 0); preview.lights[1].intensity = .65f;
                preview.ambientColor = new Color(.45f, .45f, .45f);
                preview.BeginStaticPreview(new Rect(0, 0, width, height)); preview.Render(true); var image = preview.EndStaticPreview();
                try { File.WriteAllBytes("Docs/Art/Environment/CampProps/Tier02/" + file + ".png", image.EncodeToPNG()); } finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }

    internal sealed class Tier02CampPaletteImport : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath != "Assets/_Game/Art/Environment/CampProps/Tier02/Tier02CampPalette.png") return;
            var t = (TextureImporter)assetImporter; t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; t.mipmapEnabled = false;
            t.textureCompression = TextureImporterCompression.Uncompressed; t.npotScale = TextureImporterNPOTScale.None; t.sRGBTexture = true;
        }
    }
}
