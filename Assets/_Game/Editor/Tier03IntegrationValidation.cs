using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Travel;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Tier 3 settlement integration (spec 2026-11-24). Slice A: the staged item models are imported unchanged, built as
    // palette FacetedDetail prefabs, and every bound runtime item shows exactly its model.
    public static class Tier03IntegrationValidation
    {
        private const string Ledger = "Docs/Art/Tier03/BatchFINAL02/coverage-audit.json";

        public static void Run()
        {
            var rows = Tier03ArtAssets.Load().Items;
            Check(rows.Count(r => r.Manifest == "Tier03SliceAManifest.json") == 60, "SliceA: manifest has the audited 60 models");
            Check(rows.Count(r => r.Manifest == "Tier03SliceBManifest.json") == 15, "SliceB: manifest has 5 special t3 items x display/male/female");
            SourcesUnchanged(rows);
            PrefabsBuilt(rows);
            LookupMatchesManifest(rows);
            ItemsShowTheirModel(rows);
            NoLedgerItemLeftBlank();
            DebugToolArt();
            SpecialWornFits(rows);
            SpecialContainerSkins();
            StorefrontsTier3();
            SliceCRepairs();
            Debug.Log("Tier 3 integration: slice A-C checks passed (" + rows.Length + " models, " + SettlementScenery.Storefronts.Count + " tier 3 fronts).");
        }

        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Tier 3 integration: " + name); }

        private static string Sha(string path)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }

        // The copies under Assets/_Game/Art/Tier03 are byte-identical to the FINAL02 sources they came from.
        private static void SourcesUnchanged(Tier03ArtAssets.Item[] rows)
        {
            foreach (var r in rows)
            {
                Check(File.Exists(r.SourcePath) && Sha(r.SourcePath) == r.SourceSHA256, "SourcesUnchanged: " + r.SourcePath);
                Check(File.Exists(r.PalettePath) && Sha(r.PalettePath) == r.PaletteSHA256, "SourcesUnchanged: " + r.PalettePath);
                Check(r.SourcePath.StartsWith("Assets/_Game/Art/Tier03/", StringComparison.Ordinal) && !r.SourcePath.Contains("/Resources/"), "SourcesUnchanged: source stays outside Resources: " + r.SourcePath);
            }
        }

        private static void PrefabsBuilt(Tier03ArtAssets.Item[] rows)
        {
            var shader = Shader.Find("TheTrailWalker/FacetedDetail");
            foreach (var r in rows)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Tier03ArtAssets.PrefabAssetPath(r));
                Check(prefab != null && prefab.name == r.Model, "PrefabsBuilt: " + r.PrefabPath + " exists");
                Check(r.PrefabPath.StartsWith("Tier03/", StringComparison.Ordinal) && Resources.Load<GameObject>(r.PrefabPath) != null, "PrefabsBuilt: " + r.PrefabPath + " loads from Resources");
                var material = AssetDatabase.LoadAssetAtPath<Material>(Tier03ArtAssets.MaterialAssetPath(r));
                var palette = AssetDatabase.LoadAssetAtPath<Texture2D>(r.PalettePath);
                Check(material != null && material.shader == shader && material.GetTexture("_BaseMap") == palette, "PrefabsBuilt: " + r.Model + " material is FacetedDetail with its palette");
                long triangles = 0;
                foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
                    for (int s = 0; s < filter.sharedMesh.subMeshCount; s++) triangles += filter.sharedMesh.GetIndexCount(s) / 3;
                Check(triangles == r.Triangles, "PrefabsBuilt: " + r.Model + " has " + r.Triangles + " triangles (now " + triangles + ")");
                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                    Check(renderer.sharedMaterials.Length > 0 && renderer.sharedMaterials.All(m => m == material), "PrefabsBuilt: " + r.Model + " renderers use only its material");
                Check(prefab.GetComponentsInChildren<Collider>(true).Length == 0 && prefab.GetComponentsInChildren<MonoBehaviour>(true).Length == 0, "PrefabsBuilt: " + r.Model + " is passive art");
                var importer = (ModelImporter)AssetImporter.GetAtPath(r.SourcePath);
                Check(importer != null && importer.isReadable == r.Readable && importer.materialImportMode == ModelImporterMaterialImportMode.None, "PrefabsBuilt: " + r.Model + " import settings");
            }
        }

        // The generated lookup and the manifest agree pair for pair; no item is mapped twice.
        private static void LookupMatchesManifest(Tier03ArtAssets.Item[] rows)
        {
            var expected = rows.Where(r => r.IsDisplay).SelectMany(r => r.ItemIds.Select(id => id + "=" + r.PrefabPath)).OrderBy(x => x, StringComparer.Ordinal).ToList();
            var actual = Tier3Visuals.All.Select(p => p.Key + "=" + p.Value).OrderBy(x => x, StringComparer.Ordinal).ToList();
            Check(expected.SequenceEqual(actual), "LookupMatchesManifest: Tier3Visuals.cs is regenerated from the manifest");
            var ids = rows.Where(r => r.IsDisplay).SelectMany(r => r.ItemIds).ToList();
            Check(ids.Distinct().Count() == ids.Count, "LookupMatchesManifest: no item id is bound twice");
            foreach (string id in ids)
            {
                Check(Items.Find(id) != null, "LookupMatchesManifest: " + id + " is a runtime item");
                Check(Tier2ShopVisuals.PathFor(id) == null && Tier1CraftedItems.VisualPath(id) == null, "LookupMatchesManifest: " + id + " has no earlier-tier visual that would win");
            }
        }

        private static string Shown(ItemDefinition item)
        {
            var attach = typeof(Tier2ShopVisuals).Assembly.GetType("TheTrailWalker.ResourceVisualTuning").GetMethod("Attach", BindingFlags.Public | BindingFlags.Static);
            var probe = new GameObject("tier3 probe", typeof(MeshRenderer), typeof(MeshFilter));
            try { var visual = (GameObject)attach.Invoke(null, new object[] { probe, ItemArt.ModelFor(item) }); return visual == null ? null : visual.name.Replace("(Clone)", ""); }
            finally { UnityEngine.Object.DestroyImmediate(probe); }
        }

        // The real lookup (as the bag, shops, chests and the tool rack use it) shows each bound item's model.
        private static void ItemsShowTheirModel(Tier03ArtAssets.Item[] rows)
        {
            foreach (var r in rows.Where(x => x.IsDisplay))
                foreach (string id in r.ItemIds)
                    Check(Shown(Items.Find(id)) == r.Model, "ItemsShowTheirModel: " + id + " shows " + r.Model + " (now " + Shown(Items.Find(id)) + ")");
        }

        [Serializable] private class LedgerFile { public LedgerRow[] Displays; }
        [Serializable] private class LedgerRow { public string ItemId; }

        // Every runtime item that has Tier 3 ledger art shows some model: no placeholder is left in this scope.
        private static void NoLedgerItemLeftBlank()
        {
            if (!File.Exists(Ledger)) return; // Docs are not shipped; the check runs where the art handoff exists.
            foreach (LedgerRow l in JsonUtility.FromJson<LedgerFile>(File.ReadAllText(Ledger)).Displays)
            {
                ItemDefinition item = Items.Find(l.ItemId);
                if (item != null) Check(Shown(item) != null, "NoLedgerItemLeftBlank: " + l.ItemId);
            }
        }

        // Tier 3 debug tools show their mesquite tool; tier 1-2 debug tools still borrow the pine tool of their family.
        private static void DebugToolArt()
        {
            foreach (string family in new[] { "axe", "hunting", "rod" })
                foreach (string grade in new[] { "normal", "elite" })
                {
                    string word = family == "hunting" ? "Bow" : family == "axe" ? "Axe" : "Rod";
                    ItemDefinition t3 = Items.Find("debug.tool." + family + ".t3." + grade);
                    Check(t3 != null && ItemArt.ModelFor(t3) == t3 && Shown(t3) == (grade == "normal" ? "Normal" : "Elite") + "Mesquite" + word, "DebugToolArt: " + family + " t3 " + grade + " shows mesquite");
                    ItemDefinition t2 = Items.Find("debug.tool." + family + ".t2." + grade);
                    ItemDefinition pine = t2 != null ? ItemArt.ModelFor(t2) : null;
                    Check(pine != null && pine != t2 && !Tier3Visuals.Has(pine.Id) && Shown(t2) != null && Shown(t2) == Shown(pine), "DebugToolArt: " + family + " t2 " + grade + " still borrows the pine tool");
                }
        }

        // Slice B: the fitted-garment catalog wears the refined tier 3 special gear on both bodies (user, November 24, 2026),
        // at the anchors the originals used; the original SpecialT03* prefabs stay in the project, unused.
        private static void SpecialWornFits(Tier03ArtAssets.Item[] rows)
        {
            var catalog = WearableArtCatalog.Current;
            Check(catalog != null, "SpecialWornFits: wearable art catalog loads");
            foreach (string part in new[] { "hat", "boots", "shirt", "pants", "pack" })
            {
                string id = "special.t3." + part;
                WearableArt art = catalog.Find(id);
                Check(art != null, "SpecialWornFits: " + id + " is in the wearable catalog");
                string anchor = part == "hat" ? "HeadAnchor/HatAnchor" : part == "pack" ? "BackpackAnchor" : "Root";
                Check(art.Anchor == anchor, "SpecialWornFits: " + id + " keeps anchor " + anchor + " (now " + art.Anchor + ")");
                foreach (string body in new[] { "Male", "Female" })
                {
                    var row = rows.Single(r => r.Role == body + "Worn" && r.ItemIds.Contains(id));
                    GameObject worn = art.For(body == "Female");
                    Check(worn != null && AssetDatabase.GetAssetPath(worn) == Tier03ArtAssets.PrefabAssetPath(row), "SpecialWornFits: " + id + " " + body + " wears " + row.Model + " (now " + AssetDatabase.GetAssetPath(worn) + ")");
                }
            }
        }

        // The tier 3 special shirt pocket and pack bag/opening skins sample the displayed (refined) model's palette.
        private static void SpecialContainerSkins()
        {
            Check(InventoryContainerAssets.MerchantDisplayPath("special.t3.shirt") == Tier3Visuals.PathFor("special.t3.shirt"), "SpecialContainerSkins: the shirt pocket samples the refined display");
            Check(InventoryContainerAssets.MerchantDisplayPath("special.t3.pack") == Tier3Visuals.PathFor("special.t3.pack"), "SpecialContainerSkins: the pack skins sample the refined display");
            Check(InventoryContainerAssets.MerchantDisplayPath("special.t2.pack") == "BalloonGoods/Items/SpecialT02PackItem", "SpecialContainerSkins: tier 2 special gear still samples its balloon display");
            foreach (string name in new[] { "SpecialT3ShirtPocket", "SpecialT3PackBackpack", "SpecialT3PackOpening" })
            {
                string path = "Assets/_Game/Resources/UI/Inventory/" + name + ".asset";
                var skin = AssetDatabase.LoadAssetAtPath<InventoryContainerSkin>(path);
                Check(skin != null, "SpecialContainerSkins: " + path + " exists");
                InventoryContainerAssets.SampleMerchantPalette(skin.EquipmentDefinitionId, out Color cloth, out Color binding, out Color patch);
                Check(skin.Cloth == cloth && skin.Binding == binding && skin.Patch == patch, "SpecialContainerSkins: " + name + " uses the refined display's colours (rebuild the inventory container designs)");
            }
        }

        // Slice C (user, November 24, 2026: fronts only): every storefront has a mesquite <Front>T3 that fits its street slot
        // within 1.25x its tier 1 triangles. The street shows it from tier 3 (tier 4 keeps it until a tier 4 look exists) and
        // falls back to T2, then tier 1, when art is missing. Counters are unchanged.
        private static void StorefrontsTier3()
        {
            Check(SettlementScenery.FrontFor("FarmFront", 3) == "FarmFrontT3" && SettlementScenery.FrontFor("FarmFront", 4) == "FarmFrontT3" && SettlementScenery.FrontFor("FarmFront", 2) == "FarmFrontT2", "StorefrontsTier3: FrontFor");
            int Tris(GameObject prefab)
            {
                int n = 0;
                foreach (MeshFilter f in prefab.GetComponentsInChildren<MeshFilter>(true))
                    if (f.sharedMesh != null) for (int sm = 0; sm < f.sharedMesh.subMeshCount; sm++) n += (int)f.sharedMesh.GetIndexCount(sm) / 3;
                return n;
            }
            float Width(GameObject prefab)
            {
                var go = UnityEngine.Object.Instantiate(prefab);
                try { var rs = go.GetComponentsInChildren<Renderer>(); Bounds b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b.size.x; }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }
            for (int i = 0; i < SettlementScenery.Storefronts.Count; i++)
            {
                string front = SettlementScenery.Storefronts[i].Front;
                var t3 = Resources.Load<GameObject>("Settlement/Storefronts/" + front + "T3");
                Check(t3 != null, "StorefrontsTier3: " + front + "T3 loads");
                int t1Tris = Tris(Resources.Load<GameObject>("Settlement/Storefronts/" + front)), t3Tris = Tris(t3);
                Check(t3Tris <= 1.25f * t1Tris, "StorefrontsTier3: " + front + "T3 " + t3Tris + " triangles <= 1.25 x " + t1Tris);
                float width = Width(t3);
                Check(width <= SettlementScenery.StorefrontSlots[i], "StorefrontsTier3: " + front + "T3 " + width.ToString("0.00") + " m fits its " + SettlementScenery.StorefrontSlots[i] + " m slot");
                foreach (var r in t3.GetComponentsInChildren<Renderer>(true))
                    Check(r.sharedMaterials.All(m => m != null && (m.name.EndsWith("T3", StringComparison.Ordinal))), "StorefrontsTier3: " + front + "T3 uses the tier 3 palette material (" + string.Join(",", r.sharedMaterials.Select(m => m == null ? "null" : m.name)) + ")");
            }
            foreach (string tierOne in new[] { "Assets/_Game/Resources/Settlement/Storefronts/LumberArt.mat", "Assets/_Game/Resources/Settlement/Storefronts/StorefrontPalette.mat" })
            {
                string palette = AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Material>(tierOne).GetTexture("_BaseMap"));
                Check(!palette.Contains("T2") && !palette.Contains("T3"), "StorefrontsTier3: " + tierOne + " keeps its tier 1 palette (now " + palette + ")");
            }
            var parent = new GameObject("tier 3 storefront check street");
            try
            {
                int[] levels = { 1, 1, 1, 1, 1, 1 };
                SettlementStreet t3 = SettlementScenery.Build(parent.transform, levels, -1, null, false, Resources.Load<GameObject>, 3);
                SettlementStreet noT3 = SettlementScenery.Build(parent.transform, levels, -1, null, false, path => path.EndsWith("T3") ? null : Resources.Load<GameObject>(path), 3);
                SettlementStreet neither = SettlementScenery.Build(parent.transform, levels, -1, null, false, path => path.EndsWith("T3") || path.EndsWith("T2") ? null : Resources.Load<GameObject>(path), 3);
                foreach (var (name, front, _) in SettlementScenery.Storefronts)
                {
                    string id = name == "Lumber" ? "lumber" : name == "Warehouse" ? "warehouse" : "shop." + name.ToLowerInvariant();
                    Transform a = t3.Find(id).Root, b = noT3.Find(id).Root, c = neither.Find(id).Root;
                    Check(a.Find(front + "T3") != null && a.Find(front) == null && a.GetComponent<BoxCollider>() != null, "StorefrontsTier3: tier 3 street shows " + front + "T3 with its selection box");
                    Check(b.Find(front + "T2") != null && c.Find(front) != null, "StorefrontsTier3: " + id + " falls back to T2, then tier 1");
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(parent); }
        }

        // Slice C repairs (user, November 24, 2026): the SET04/05/08 furniture candidates (trophy plaque seated, cloth-press caps
        // sunk below the crossbeam, mesquite chest hollow) replace their sources in place, and the lumber-zone oak backdrop parts
        // lose their zero-area triangles (SET16, applied to the current files). Counters stay unchanged.
        private static void SliceCRepairs()
        {
            var models = new[] { "Assets/_Game/Art/Settlement/Furniture/FurnitureTrophyCabinet.obj", "Assets/_Game/Art/Settlement/Furniture/FurnitureClothPress.obj",
                "Assets/_Game/Art/Settlement/Furniture/FurnitureMesquiteChest.obj" }
                .Concat(new[] { "A", "B", "C" }.SelectMany(t => new[] { "Lower", "Upper" }.Select(part => "Assets/_Game/Art/Harvest/Massive/MassiveOak" + t + part + ".obj")));
            foreach (string path in models)
            {
                int fileTriangles = File.ReadAllLines(path).Count(l => l.StartsWith("f ", StringComparison.Ordinal));
                var mesh = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>().Single();
                Vector3[] v = mesh.vertices; int[] t = mesh.triangles; int zero = 0;
                for (int i = 0; i < t.Length; i += 3) if (Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]).sqrMagnitude < 1e-20f) zero++;
                Check(t.Length / 3 == fileTriangles, "SliceCRepairs: " + path + " imports its " + fileTriangles + " triangles (now " + t.Length / 3 + ")");
                Check(zero == 0, "SliceCRepairs: " + path + " has no zero-area triangles (" + zero + ")");
            }
            Check(File.ReadAllLines("Assets/_Game/Art/Settlement/Furniture/FurnitureMesquiteChest.obj").Count(l => l.StartsWith("v ", StringComparison.Ordinal)) == 172, "SliceCRepairs: the mesquite chest uses the hollow body (172 vertices)");
        }
    }
}
