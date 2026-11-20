using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheTrailWalker.Editor
{
    // Milestone 14 phone round R2: the counter lamp prefab only (Tools/Art/generate_counter_lamp.py). The iron frame uses the
    // storefront material; the panes use CounterLampGlass (URP Unlit on the same palette strip) so they read as lit glass.
    // CounterLamps.Hang places the lamp at runtime and adds the point light at LightAnchor. Leaves every other asset untouched.
    public static class CounterLampAssets
    {
        const string Source = "Assets/_Game/Art/Settlement/Storefronts/";
        const string Target = "Assets/_Game/Resources/Settlement/Storefronts/";
        const float LightY = -.35f;   // the glass centre (generate_counter_lamp.LIGHT_Y)

        [MenuItem("The Trail Walker/Art/Build Counter Lamp")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            foreach (var part in new[] { "CounterLamp", "CounterLampGlass" })
            {
                var importer = (ModelImporter)AssetImporter.GetAtPath(Source + part + ".obj");
                importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false; importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0; importer.SaveAndReimport();
            }
            var glass = AssetDatabase.LoadAssetAtPath<Material>(Target + "CounterLampGlass.mat");
            if (glass == null) { glass = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(glass, Target + "CounterLampGlass.mat"); }
            glass.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Source + "StorefrontPalette.png")); glass.SetColor("_BaseColor", Color.white);
            glass.enableInstancing = true; EditorUtility.SetDirty(glass);

            var root = new GameObject("CounterLamp");
            try
            {
                Part(root.transform, "CounterLamp", AssetDatabase.LoadAssetAtPath<Material>(Target + "StorefrontPalette.mat"));
                Part(root.transform, "CounterLampGlass", glass);
                var anchor = new GameObject("LightAnchor").transform; anchor.SetParent(root.transform, false); anchor.localPosition = new Vector3(0, LightY, 0);
                PrefabUtility.SaveAsPrefabAsset(root, Target + "CounterLamp.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();
        }

        static void Part(Transform parent, string name, Material mat)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".obj");
            if (source == null || mat == null) throw new InvalidOperationException("Missing counter lamp mesh or material for " + name);
            var part = UnityEngine.Object.Instantiate(source, parent, false); part.name = name;
            foreach (var r in part.GetComponentsInChildren<MeshRenderer>()) { r.sharedMaterial = mat; r.shadowCastingMode = ShadowCastingMode.Off; }
        }
    }
}
