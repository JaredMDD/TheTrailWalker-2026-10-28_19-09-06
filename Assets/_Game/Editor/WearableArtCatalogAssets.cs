using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.Domain.Equipment;
using TheTrailWalker.Domain.Items;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Builds Resources/Characters/WearableArtCatalog.asset from Assets/_Game/Art/Characters/WearableArtMap.json
    // (Tools/Art/build_wearable_art_map.py) and renders the fitted-garment review through the real equipment avatar.
    // Equipment redesign (user, November 7, 2026). No item, recipe or save changes.
    public static class WearableArtCatalogAssets
    {
        const string MapPath = "Assets/_Game/Art/Characters/WearableArtMap.json";
        const string CatalogPath = "Assets/_Game/Resources/Characters/WearableArtCatalog.asset";
        const string Docs = "Docs/Art/Characters/Wearables/";

        [Serializable] sealed class Entry { public string itemId, slot, anchor, male, female, source; }
        [Serializable] sealed class Map { public string[] maleHatHair, femaleHatHair; public Entry[] items; }

        [MenuItem("The Trail Walker/Art/Build Wearable Art Catalog")]
        public static void Build()
        {
            var map = JsonUtility.FromJson<Map>(File.ReadAllText(MapPath));
            var catalog = AssetDatabase.LoadAssetAtPath<WearableArtCatalog>(CatalogPath);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<WearableArtCatalog>(); AssetDatabase.CreateAsset(catalog, CatalogPath); }
            var items = new List<WearableArt>();
            foreach (var e in map.items)
                items.Add(new WearableArt
                {
                    ItemId = e.itemId, Slot = (GearSlot)Enum.Parse(typeof(GearSlot), e.slot), Anchor = e.anchor, Source = e.source,
                    Male = Load(e.male), Female = Load(e.female)
                });
            catalog.Items = items.ToArray();
            catalog.MaleHatHair = Array.ConvertAll(map.maleHatHair, Load);
            catalog.FemaleHatHair = Array.ConvertAll(map.femaleHatHair, Load);
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            Debug.Log($"Wearable art catalog: {catalog.Items.Length} fitted garments.");
        }

        static GameObject Load(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException("Missing wearable prefab " + path);
            return prefab;
        }

        // Contract checks; returns the passed check names (throws on the first failure).
        public static List<string> Validate()
        {
            var passed = new List<string>();
            void Check(bool ok, string name) { if (!ok) throw new Exception("Wearable art: " + name); passed.Add(name); }
            var catalog = AssetDatabase.LoadAssetAtPath<WearableArtCatalog>(CatalogPath);
            Check(catalog != null && WearableArtCatalog.Current != null, "The wearable art catalog is in Resources");
            var ids = new HashSet<string>();
            var body = EquipmentAvatarView.Catalog.MaleBody.transform;
            foreach (var w in catalog.Items)
            {
                Check(ids.Add(w.ItemId), w.ItemId + ": one entry");
                Check(w.Male != null && w.Female != null, w.ItemId + ": fitted on both bodies");
                Check(w.Anchor == "Root" || body.Find(w.Anchor) != null, w.ItemId + $": anchor {w.Anchor} exists on the body");
                Check(w.Anchor == (w.Slot == GearSlot.Hat ? "HeadAnchor/HatAnchor" : w.Slot == GearSlot.Backpack ? "BackpackAnchor" : "Root"), w.ItemId + $": {w.Slot} uses its slot's frame");
            }
            foreach (var d in PrototypeItems.All)
                if (d.GearSlot.HasValue && !d.Id.StartsWith("debug.", StringComparison.Ordinal))
                    Check(catalog.Find(ItemArt.ModelFor(d).Id) != null, d.Id + ": every live wearable has fitted art");
            foreach (string id in new[] { "item.beaver_fur_cap", "item.marsh_boots", "item.flax_work_shirt", "item.flax_trousers", "item.marsh_pack",
                                          "item.taiga_fur_hat", "item.taiga_boots", "item.taiga_waxed_shirt", "item.taiga_trousers", "item.taiga_frame_pack" })
                Check(catalog.Find(id) != null, id + ": tier-two garment is bound");
            for (int t = 1; t <= 4; t++)
                foreach (string slot in new[] { "hat", "boots", "shirt", "pants", "pack" })
                    Check(catalog.Find($"special.t{t}.{slot}") != null, $"special.t{t}.{slot}: balloon special gear is bound");
            for (int s = 0; s < 4; s++) Check(catalog.HatHair(false, s) != null && catalog.HatHair(true, s) != null, $"hat hair exists for style {s} on both bodies");
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Wearable-art-checks.txt", passed);
            return passed;
        }

        // Review: each outfit dressed through EquipmentAvatarView.Dress (the live binding), male and female, front and back.
        [MenuItem("The Trail Walker/Art/Capture Wearable Review")]
        public static void Capture()
        {
            Directory.CreateDirectory(Docs);
            var catalog = WearableArtCatalog.Current;
            var sets = new List<(string name, string[] ids)>
            {
                ("tier-one", new[] { "item.rabbit_hat", "item.rabbit_boots", "item.hemp_shirt", "item.hemp_pants", "item.starter_backpack" }),
                ("tier-two-marsh", new[] { "item.beaver_fur_cap", "item.marsh_boots", "item.flax_work_shirt", "item.flax_trousers", "item.marsh_pack" }),
                ("tier-two-taiga", new[] { "item.taiga_fur_hat", "item.taiga_boots", "item.taiga_waxed_shirt", "item.taiga_trousers", "item.taiga_frame_pack" }),
            };
            for (int t = 1; t <= 4; t++) sets.Add(($"special-t{t}", new[] { $"special.t{t}.hat", $"special.t{t}.boots", $"special.t{t}.shirt", $"special.t{t}.pants", $"special.t{t}.pack" }));
            foreach (bool back in new[] { false, true })
            {
                var preview = new PreviewRenderUtility(); var views = new List<EquipmentAvatarView>();
                try
                {
                    int column = 0;
                    foreach (var set in sets)
                        foreach (bool female in new[] { false, true })
                        {
                            var stage = new GameObject("stage", typeof(RectTransform)).GetComponent<RectTransform>();
                            var view = EquipmentAvatarView.Create(stage, new PlayerAppearance { Body = female ? 1 : 0, Skin = 2, HairStyle = female ? 3 : 1, HairColor = 1 });
                            UnityEngine.Object.DestroyImmediate(stage.gameObject);
                            ItemDefinition Def(int i) { var w = catalog.Find(set.ids[i]); return new ItemDefinition(w.ItemId, w.ItemId, 1000, 1000, false, w.Slot, 2); }
                            view.Dress(Def(0), Def(4), Def(2), Def(3), Def(1));
                            var copy = UnityEngine.Object.Instantiate(view.StageAvatar);
                            copy.transform.position = new Vector3(column * .9f, 0, 0); copy.transform.rotation = Quaternion.Euler(0, back ? 180 : 0, 0);
                            foreach (var t in copy.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 0;
                            preview.AddSingleGO(copy); views.Add(view); column++;
                        }
                    float width = (column - 1) * .9f;
                    var cam = preview.camera; cam.orthographic = true; cam.orthographicSize = 1.05f;
                    cam.transform.position = new Vector3(width / 2, .95f, 10); cam.transform.LookAt(new Vector3(width / 2, .95f, 0));
                    cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.19f, .22f, .23f); cam.nearClipPlane = .1f; cam.farClipPlane = 30;
                    preview.lights[0].intensity = 1.2f; preview.lights[0].transform.rotation = Quaternion.Euler(30, back ? 20 : 200, 0);
                    preview.lights[1].intensity = .6f; preview.ambientColor = new Color(.6f, .6f, .59f);
                    preview.BeginStaticPreview(new Rect(0, 0, 220 * column, 520)); preview.Render(true);
                    var image = preview.EndStaticPreview();
                    try { File.WriteAllBytes(Docs + (back ? "all-fitted-sets-back.png" : "all-fitted-sets-front.png"), image.EncodeToPNG()); }
                    finally { UnityEngine.Object.DestroyImmediate(image); }
                }
                finally { preview.Cleanup(); foreach (var v in views) v.Destroy(); }
            }
        }
    }
}
