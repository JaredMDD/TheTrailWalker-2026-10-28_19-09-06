using System;
using System.IO;
using TheTrailWalker.Domain.Pets;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Milestone 15 slice 1: the shared pet bed prefab (Tools/Art/generate_pet_bed.py, house material) and one transparent
    // 256 px shop picture per pet, rendered from its standing companion prefab. Passive art only.
    public static class PetArtAssets
    {
        public const string BedSource = "Assets/_Game/Art/Settlement/Pets/PetBed.obj";
        public const string BedPrefab = "Assets/_Game/Resources/Settlement/Pets/PetBed.prefab";
        public const string BedResource = "Settlement/Pets/PetBed";
        public const string Icons = "Assets/_Game/Resources/UI/Pets/";
        const string HouseMaterial = "Assets/_Game/Resources/Settlement/Houses/HousePalette.mat";

        public static string IconName(PetDefinition pet) => pet.Name;   // UI/Pets/<Name>.png
        public static string IconResource(PetDefinition pet) => "UI/Pets/" + IconName(pet);

        [MenuItem("The Trail Walker/Pets/Build Pet Art")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build outside Play mode.");
            AssetDatabase.Refresh();
            var importer = (ModelImporter)AssetImporter.GetAtPath(BedSource);
            if (importer == null) throw new InvalidOperationException("Run Tools/Art/generate_pet_bed.py first: " + BedSource);
            importer.globalScale = 1; importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
            importer.addCollider = false; importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importNormals = ModelImporterNormals.Calculate; importer.normalSmoothingAngle = 0; importer.SaveAndReimport();
            var material = AssetDatabase.LoadAssetAtPath<Material>(HouseMaterial);
            if (material == null) throw new InvalidOperationException("Missing the shared house material " + HouseMaterial);
            Directory.CreateDirectory(Path.GetDirectoryName(BedPrefab));
            var root = new GameObject("PetBed");
            try
            {
                var visual = (GameObject)UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(BedSource), root.transform, false);
                visual.name = "Visual";
                foreach (var r in visual.GetComponentsInChildren<Renderer>()) r.sharedMaterial = material;
                var top = new GameObject("PetAnchor").transform; top.SetParent(root.transform, false); top.localPosition = new Vector3(0, .095f, 0);   // the cushion top
                PrefabUtility.SaveAsPrefabAsset(root, BedPrefab);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }

            // Shop pictures: a three-quarter view of each pet's standing pose, framed on its bounds.
            Directory.CreateDirectory(Icons);
            foreach (PetDefinition pet in PetCatalog.All)
            {
                var prefab = Resources.Load<GameObject>(pet.StandingModel);
                if (prefab == null) throw new InvalidOperationException("Missing companion prefab " + pet.StandingModel);
                var view = UnityEngine.Object.Instantiate(prefab);
                try
                {
                    var renderers = view.GetComponentsInChildren<Renderer>(); Bounds b = renderers[0].bounds;
                    foreach (var r in renderers) b.Encapsulate(r.bounds);
                    float size = Mathf.Max(b.size.y, Mathf.Max(b.size.x, b.size.z)) * .62f + .03f;
                    FurnitureArtAssets.Render(view, Icons + IconName(pet) + ".png", b.center + new Vector3(6, 4.5f, 8).normalized * 12, b.center, size, 256, 256, new Color(0, 0, 0, 0));
                }
                finally { UnityEngine.Object.DestroyImmediate(view); }
            }
            AssetDatabase.Refresh();
            Debug.Log("Pet art: bed prefab and " + PetCatalog.All.Count + " shop pictures built.");
        }

        // Review renders (Docs only): the backyard as the game builds and frames it, one per pet-home look, and each level's
        // entry area with the bed and a pet, as PlaceEntryPet puts them.
        public const string Docs = "Docs/Art/Settlement/Pets/";
        [MenuItem("The Trail Walker/Pets/Capture Pet Review")]
        public static void Capture()
        {
            Directory.CreateDirectory(Docs);
            var host = new GameObject("pet review", typeof(MovementPrototype));
            try
            {
                var world = host.GetComponent<MovementPrototype>();
                foreach (var (level, pet) in new[] { (3, "pet.dog"), (5, "pet.tortoise"), (7, "pet.pig") })
                {
                    Transform yard = world.BuildBackyard(level, PetCatalog.Find(pet).StandingModel);
                    Perspective(yard.gameObject, Docs + "backyard-level" + level + ".png", yard.TransformPoint(CollectionPrototype.BackyardEye), yard.TransformPoint(CollectionPrototype.BackyardLook), 1080, 1920);
                }
                world.ClearInterior();
            }
            finally { UnityEngine.Object.DestroyImmediate(host); }
            foreach (var (level, pet) in new[] { (3, "pet.cat"), (6, "pet.dog"), (8, "pet.rabbit") })
            {
                var interior = (GameObject)UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Settlement/Houses/HouseL" + level.ToString("00") + "Interior"));
                try
                {
                    Transform anchor = null;
                    foreach (var t in interior.GetComponentsInChildren<Transform>(true)) if (t.name == "PetBedAnchor") anchor = t;
                    var bed = (GameObject)UnityEngine.Object.Instantiate(Resources.Load<GameObject>(BedResource), anchor, false);
                    UnityEngine.Object.Instantiate(Resources.Load<GameObject>(PetCatalog.Find(pet).SecondModel), bed.transform.Find("PetAnchor"), false);
                    foreach (var t in interior.GetComponentsInChildren<Transform>(true))
                        if (t.name == "CollectionBoxAnchor") UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Settlement/Houses/PetCollectionContainer"), t, false);
                    Perspective(interior, Docs + "entry-level" + level + ".png", anchor.position + new Vector3(1.6f, 1.7f, 2.6f), anchor.position + new Vector3(0, .3f, 0), 1200, 900);
                }
                finally { UnityEngine.Object.DestroyImmediate(interior); }
            }
            Debug.Log("Pet review renders saved to " + Docs);
        }

        static void Perspective(GameObject subject, string file, Vector3 eye, Vector3 target, int width, int height)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                var copy = UnityEngine.Object.Instantiate(subject); copy.transform.SetPositionAndRotation(subject.transform.position, subject.transform.rotation);
                preview.AddSingleGO(copy);
                var cam = preview.camera; cam.orthographic = false; cam.fieldOfView = 50;
                cam.transform.position = eye; cam.transform.LookAt(target);
                cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.66f, .76f, .82f); cam.nearClipPlane = .05f; cam.farClipPlane = 80;
                preview.lights[0].intensity = 1.2f; preview.lights[0].transform.rotation = Quaternion.Euler(40, 210, 0);
                preview.lights[1].intensity = .6f; preview.ambientColor = new Color(.6f, .6f, .6f);
                preview.BeginStaticPreview(new Rect(0, 0, width, height)); preview.Render(true);
                var image = preview.EndStaticPreview();
                try { File.WriteAllBytes(file, image.EncodeToPNG()); } finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
