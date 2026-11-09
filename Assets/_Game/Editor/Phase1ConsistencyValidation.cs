using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using UnityEditor;

namespace TheTrailWalker.Editor
{
    // Phase 1 consistency (spec 2026-11-08): tier 2-4 harvest item IDs, per-tier fish pairs and harvest tables.
    // Chained after the tier-one regions checks in the suite.
    public static class Phase1ConsistencyValidation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Phase 1 consistency: " + name); passed.Add(name); }

        [MenuItem("The Trail Walker/Phase 1/Validate consistency")]
        public static void Run()
        {
            passed.Clear();
            GoldenTierOne();
            FishPairChecks();
            TierRollChecks();
            ItemDefinitionChecks();
            ResourcesHygieneChecks();
            StorageSkinChecks();
            TierItemPersistenceChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Phase1-consistency-checks.txt", passed);
        }

        // Tier-one rolls captured from the pre-change Roll (November 8, 2026): case i = (kind, region, boosted), segment.RR.01,
        // cycle 1 + i % 2, anchor.i. Tree/Hunt/Fish x regions 1, 3, 6 unboosted, then each kind boosted in region 1.
        private static readonly (InteractableKind kind, int region, bool boosted)[] GoldenCases = {
            (InteractableKind.Tree, 1, false), (InteractableKind.Tree, 3, false), (InteractableKind.Tree, 6, false),
            (InteractableKind.AnimalSpawn, 1, false), (InteractableKind.AnimalSpawn, 3, false), (InteractableKind.AnimalSpawn, 6, false),
            (InteractableKind.FishingPoint, 1, false), (InteractableKind.FishingPoint, 3, false), (InteractableKind.FishingPoint, 6, false),
            (InteractableKind.Tree, 1, true), (InteractableKind.AnimalSpawn, 1, true), (InteractableKind.FishingPoint, 1, true) };

        private static readonly string[] GoldenExpected = {
            "material.pine_branch:5,material.pine_branch:4,material.pine_log:3,material.pine_log:1,material.pine_log:3,material.pine_log:4,material.pine_log:1,ingredient.pine_nuts:5,ingredient.pine_nuts:4,ingredient.pine_nuts:3",
            "material.pine_branch:2,material.pine_branch:1,material.pine_branch:1,material.pine_branch:4,material.pine_branch:4,material.pine_log:4,material.pine_log:5,material.pine_log:4,ingredient.pine_nuts:2,ingredient.pine_nuts:4",
            "material.pine_branch:4,material.pine_branch:2,material.pine_branch:1,material.pine_branch:5,material.pine_log:3,material.pine_log:2,ingredient.pine_nuts:5,ingredient.pine_nuts:5",
            "animal.rabbit.pelt:4,animal.rabbit.pelt:2,animal.rabbit.pelt:4,animal.rabbit.meat:4,animal.rabbit.meat:3,material.bone:4,material.bone:2,material.bone:2,ingredient.animal_fat:2,ingredient.animal_fat:5,ingredient.animal_fat:4",
            "animal.rabbit.pelt:1,animal.rabbit.pelt:1,animal.rabbit.pelt:2,animal.rabbit.pelt:5,animal.rabbit.pelt:5,animal.rabbit.meat:5,animal.rabbit.meat:2,material.bone:4,material.bone:5,ingredient.animal_fat:5",
            "animal.rabbit.pelt:1,animal.rabbit.pelt:4,animal.rabbit.pelt:3,animal.rabbit.pelt:2,animal.rabbit.pelt:5,animal.rabbit.meat:4,animal.rabbit.meat:1,animal.rabbit.meat:4,material.bone:4,material.bone:2,material.bone:2,material.bone:1,material.bone:2,ingredient.animal_fat:4,ingredient.animal_fat:5",
            "fish.rainbow_trout:3,fish.bluegill:1,fish.rainbow_trout:1,fish.rainbow_trout:1,fish.rainbow_trout:2",
            "fish.largemouth_bass:2,fish.rainbow_trout:1",
            "fish.bluegill:2,fish.bluegill:4,fish.bluegill:5,fish.bluegill:3",
            "material.pine_branch:1,material.pine_branch:1,material.pine_branch:3,material.pine_branch:5,material.pine_branch:5,material.pine_log:4,material.pine_log:4,material.pine_log:5,material.pine_log:4,ingredient.pine_nuts:4,ingredient.pine_nuts:4,ingredient.pine_nuts:1,ingredient.pine_nuts:2,ingredient.pine_nuts:3,ingredient.pine_nuts:3",
            "animal.rabbit.pelt:4,animal.rabbit.pelt:4,animal.rabbit.pelt:1,animal.rabbit.pelt:2,animal.rabbit.pelt:2,animal.rabbit.meat:4,animal.rabbit.meat:5,animal.rabbit.meat:1,animal.rabbit.meat:2,animal.rabbit.meat:2,material.bone:3,material.bone:1,material.bone:5,material.bone:1,material.bone:1,material.bone:2,material.bone:1,material.bone:1,material.bone:3,material.bone:3,ingredient.animal_fat:3,ingredient.animal_fat:1,ingredient.animal_fat:2",
            "fish.bluegill:2,fish.rainbow_trout:1,fish.rainbow_trout:3,fish.rainbow_trout:2,fish.rainbow_trout:2,fish.bluegill:4,fish.rainbow_trout:2,fish.bluegill:4"
        };

        private static string Sequence(IReadOnlyList<HarvestBundle> rolled) => string.Join(",", rolled.Select(b => b.ItemId + ":" + b.Quality));

        private static void GoldenTierOne()
        {
            bool all = GoldenExpected.Length == GoldenCases.Length;
            for (int i = 0; i < GoldenCases.Length && all; i++)
            {
                var c = GoldenCases[i];
                string actual = Sequence(HarvestManifest.Roll(c.kind, c.region, $"segment.{c.region:00}.01", 1 + i % 2, "anchor." + i, c.boosted));
                if (actual != GoldenExpected[i]) throw new Exception($"Phase 1 consistency: golden case {i} ({c.kind} r{c.region} boosted={c.boosted}) rolled {actual}");
            }
            Check(all, "golden tier-one rolls: 12 cases (Tree/Hunt/Fish x regions 1, 3, 6 and boosted) equal the pre-change item-for-item sequences");
        }

        private static readonly string[] ExpectedPairs = {
            "fish.rainbow_trout+fish.bluegill", "fish.largemouth_bass+fish.yellow_perch", "fish.rainbow_trout+fish.largemouth_bass", "fish.bluegill+fish.yellow_perch", "fish.rainbow_trout+fish.yellow_perch", "fish.largemouth_bass+fish.bluegill",
            "fish.common_carp+fish.channel_catfish", "fish.northern_pike+fish.walleye", "fish.common_carp+fish.northern_pike", "fish.channel_catfish+fish.walleye", "fish.common_carp+fish.walleye", "fish.northern_pike+fish.channel_catfish",
            "fish.salmon+fish.brown_trout", "fish.sturgeon+fish.lake_trout", "fish.salmon+fish.sturgeon", "fish.brown_trout+fish.lake_trout", "fish.salmon+fish.lake_trout", "fish.sturgeon+fish.brown_trout",
            "fish.longnose_gar+fish.red_drum", "fish.longnose_gar+fish.red_snapper", "fish.red_drum+fish.red_snapper", "fish.longnose_gar+fish.tuna", "fish.red_drum+fish.tuna", "fish.red_snapper+fish.tuna" };

        private static void FishPairChecks()
        {
            bool ok = true;
            for (int r = 1; r <= 24; r++) { var p = HarvestManifest.FishPair(r); ok &= p.a + "+" + p.b == ExpectedPairs[r - 1]; }
            Check(ok, "FishPair equals the section 6.2.5 table for all 24 regions");
            Check(HarvestManifest.FishPair(7) == ("fish.common_carp", "fish.channel_catfish") && HarvestManifest.FishPair(13) == ("fish.salmon", "fish.brown_trout")
                && HarvestManifest.FishPair(19) == ("fish.longnose_gar", "fish.red_drum") && HarvestManifest.FishPair(24) == ("fish.red_snapper", "fish.tuna"), "FishPair spot checks: 7 carp/catfish, 13 salmon/brown trout, 19 longnose gar/red drum, 24 red snapper/tuna");
        }

        private static List<HarvestBundle> Rolls(InteractableKind kind, int region, bool boosted, int cycles = 30)
        {
            var list = new List<HarvestBundle>();
            for (int cycle = 1; cycle <= cycles; cycle++) list.AddRange(HarvestManifest.Roll(kind, region, $"segment.{region:00}.01", cycle, "anchor.x", boosted));
            return list;
        }

        private static List<string> Distinct(InteractableKind kind, int region, bool boosted) => Rolls(kind, region, boosted).Select(b => b.ItemId).Distinct().OrderBy(s => s, StringComparer.Ordinal).ToList();

        private static void TierRollChecks()
        {
            Check(Rolls(InteractableKind.FishingPoint, 7, false).Concat(Rolls(InteractableKind.FishingPoint, 7, true)).All(b => b.ItemId == "fish.common_carp" || b.ItemId == "fish.channel_catfish"), "region 7 fishing yields only common carp or channel catfish");
            Check(Distinct(InteractableKind.Tree, 10, false).SequenceEqual(new[] { "ingredient.birch_sap", "material.birch_branch", "material.birch_log" }), "region 10 tree roll yields birch branch, birch log and birch sap");
            foreach (bool boosted in new[] { false, true })
            {
                var hunt19 = HarvestManifest.Roll(InteractableKind.AnimalSpawn, 19, "segment.19.01", 1, "anchor.x", boosted);
                string[] needed = { "animal.boar.hide", "animal.boar.meat", "material.bone", "ingredient.animal_fat", "animal.boar.tusk" };
                Check(needed.All(id => hunt19.Any(b => b.ItemId == id)), $"region 19 hunt (boosted={boosted}) contains boar hide, meat, bone, fat and tusk");
                int max = boosted ? 6 : 3;
                var tusks = Enumerable.Range(1, 60).Select(c => HarvestManifest.Roll(InteractableKind.AnimalSpawn, 19, "segment.19.01", c, "anchor.y", boosted).Count(b => b.ItemId == "animal.boar.tusk")).ToList();
                Check(tusks.Min() >= 1 && tusks.Max() <= max && tusks.Max() > 1, $"boar tusk bundles stay 1-{max} ({(boosted ? "Elite" : "Normal")}) over 60 rolls");
            }
            var unresolved = new List<string>();
            foreach (InteractableKind kind in Enum.GetValues(typeof(InteractableKind)))
                for (int region = 7; region <= 24; region++)
                    foreach (bool boosted in new[] { false, true })
                        foreach (HarvestBundle b in Rolls(kind, region, boosted, 12)) if (Items.Find(b.ItemId) == null) unresolved.Add(b.ItemId);
            Check(unresolved.Count == 0, "every ID a tier 2-4 roll can produce resolves through Items.Find" + (unresolved.Count == 0 ? "" : ": " + unresolved.First()));
            Check(Distinct(InteractableKind.AnimalSpawn, 7, false).SequenceEqual(new[] { "animal.beaver.meat", "animal.beaver.pelt", "ingredient.animal_fat", "material.bone" }), "tier-two hunt is beaver pelt, meat, bone and fat (no byproduct)");
            Check(Distinct(InteractableKind.AnimalSpawn, 13, true).SequenceEqual(new[] { "animal.turkey.feathers", "animal.turkey.meat", "ingredient.animal_fat", "material.bone" }), "tier-three hunt is turkey feathers, meat, bone and fat (no byproduct)");
            Check(Distinct(InteractableKind.Tree, 13, false).SequenceEqual(new[] { "ingredient.mesquite_pods", "material.mesquite_branch", "material.mesquite_log" })
                && Distinct(InteractableKind.Tree, 19, false).SequenceEqual(new[] { "ingredient.acorns", "material.oak_branch", "material.oak_log" }), "tier-three trees give mesquite branch/log/pods and tier-four trees oak branch/log/acorns");
        }

        private static void ResourcesHygieneChecks()
        {
            const string root = "Assets/_Game/Resources";
            var review = Directory.GetFiles(root, "*.prefab", SearchOption.AllDirectories).Select(f => f.Replace("\\", "/")).Where(f => f.Contains("/Reviews/") || f.Contains("/ReviewOnly/")).ToList();
            Check(review.Count == 0, "no prefab under Resources is a review/evidence prefab (/Reviews/ or /ReviewOnly/)" + (review.Count == 0 ? "" : ": " + review.First()));

            var literal = new Regex(@"Resources\.(Load|LoadAll)\s*(<[^>]*>)?\(\s*(\$?)""([^""]*)""\s*([,)+])");
            var missing = new List<string>(); var skipped = new List<string>(); int resolved = 0;
            foreach (string file in Directory.GetFiles("Assets/_Game/Scripts", "*.cs", SearchOption.AllDirectories))
                foreach (Match m in literal.Matches(File.ReadAllText(file)))
                {
                    string path = m.Groups[4].Value, tag = Path.GetFileName(file) + ": " + path;
                    if (m.Groups[3].Value == "$" || m.Groups[5].Value == "+" || path.Contains("{")) { skipped.Add(tag); continue; }
                    bool ok = m.Groups[1].Value == "LoadAll" ? Directory.Exists(root + "/" + path)
                        : Directory.Exists(Path.GetDirectoryName(root + "/" + path)) && Directory.GetFiles(Path.GetDirectoryName(root + "/" + path), Path.GetFileName(path) + ".*").Any(f => !f.EndsWith(".meta"));
                    if (ok) resolved++; else missing.Add(tag);
                }
            Directory.CreateDirectory("Builds/Validation");
            File.WriteAllLines("Builds/Validation/resources-load-literals.txt", new[] { "resolved: " + resolved, "missing: " + missing.Count }.Concat(missing.Select(x => "MISSING " + x)).Concat(skipped.Select(x => "SKIPPED (interpolated/concatenated) " + x)));
            Check(missing.Count == 0, "every literal Resources.Load path in Scripts resolves (" + resolved + " resolved, " + skipped.Count + " interpolated skipped)" + (missing.Count == 0 ? "" : ": " + missing.First()));

            var reviewRows = Directory.GetFiles("Assets/_Game/Art", "Tier02Batch*Manifest.json", SearchOption.AllDirectories)
                .SelectMany(f => Regex.Matches(File.ReadAllText(f), @"""PrefabPath""\s*:\s*""(Tier02/Reviews/[^""]+)""").Cast<Match>().Select(m => m.Groups[1].Value)).ToList();
            var misplaced = reviewRows.Where(r => !File.Exists(Tier02BatchArtAssets.PrefabAssetPath(r)) || !Tier02BatchArtAssets.PrefabAssetPath(r).StartsWith("Assets/_Game/ArtReviews/") || File.Exists(root + "/" + r + ".prefab")).ToList();
            Check(reviewRows.Count > 0 && misplaced.Count == 0, "every Tier02Batch*Manifest review PrefabPath resolves under ArtReviews and not Resources (" + reviewRows.Count + " rows)" + (misplaced.Count == 0 ? "" : ": " + misplaced.First()));

            var sums = ApkBudget.ByResourcesFolder(new[] { ("Assets/_Game/Resources/Tier02/Food/A.mesh", 10UL), ("Assets/_Game/Resources/Tier02/Food/B.mat", 5UL), ("Assets/_Game/Art/X.png", 7UL) });
            Check(sums.Count == 1 && sums.TryGetValue("Tier02/Food", out ulong food) && food == 15, "ApkBudget.ByResourcesFolder sums Resources/<top>/<second> and ignores other paths");
        }

        private static readonly (string name, string guid)[] TierOneSkinGuids = {
            ("BaseRags", "cf59d7cf6b9cc624ca46ccf88027f769"), ("Tier01Backpack", "b9e4f8ac40aacdd4588a596f129b41e7"), ("Tier01ShirtPocket", "c0f507af8627312498f2ff44ce2849b1"),
            ("Tier01PantsBelt", "c0648bb0601eae445abe0aa225453db1"), ("Tier01ToolLoop", "d7092edb9cdf49e47b31f50cace511bd"), ("BaseRagsOpening", "69c1ffa5b4a12d04895ba5dcea2b2d98"),
            ("Tier01BackpackOpening", "dfdb94c267a50884c950b2de8393c34b") };

        // Task 7: every shirt and backpack in InventoryStorageGear.All has its own skins and baked meshes; tier-one assets are untouched.
        private static void StorageSkinChecks()
        {
            const string skins = "Assets/_Game/Resources/UI/Inventory/", meshes = "Assets/_Game/Art/UI/Inventory/";
            var wrongGuid = TierOneSkinGuids.Where(t => AssetDatabase.AssetPathToGUID(skins + t.name + ".asset") != t.guid).Select(t => t.name).ToList();
            Check(wrongGuid.Count == 0, "the seven tier-one container skins keep their GUIDs" + (wrongGuid.Count == 0 ? "" : ": " + wrongGuid.First()));
            var backpack = AssetDatabase.LoadAssetAtPath<TheTrailWalker.InventoryContainerSkin>(skins + "Tier01Backpack.asset");
            var pocket = AssetDatabase.LoadAssetAtPath<TheTrailWalker.InventoryContainerSkin>(skins + "Tier01ShirtPocket.asset");
            Check(backpack != null && pocket != null && backpack.Cloth == new UnityEngine.Color(.43f, .40f, .28f) && backpack.Patch == new UnityEngine.Color(.39f, .28f, .17f)
                && pocket.Cloth == new UnityEngine.Color(.43f, .40f, .28f) && backpack.EquipmentDefinitionId == "item.starter_backpack" && pocket.EquipmentDefinitionId == "item.hemp_shirt", "tier-one backpack and pocket skins keep their colours and item IDs");

            var all = TheTrailWalker.InventoryStorageGear.All;
            Check(all.Length == 20 && all.Select(e => e.ItemId).Distinct().Count() == 20 && all.Count(e => e.Backpack) == 10, "InventoryStorageGear.All lists 20 distinct items (10 shirts, 10 backpacks)");
            var problems = new List<string>();
            foreach (var gear in all)
            {
                string baseName = TheTrailWalker.InventoryStorageGear.PascalName(gear.ItemId);
                var kinds = gear.Backpack ? new[] { (baseName + "Backpack", TheTrailWalker.InventoryContainerKind.Backpack), (baseName + "Opening", TheTrailWalker.InventoryContainerKind.Opening) }
                                          : new[] { (baseName + "Pocket", TheTrailWalker.InventoryContainerKind.Pocket) };
                foreach (var (name, kind) in kinds)
                {
                    var skin = AssetDatabase.LoadAssetAtPath<TheTrailWalker.InventoryContainerSkin>(skins + name + ".asset");
                    if (skin == null) { problems.Add(name + " skin missing"); continue; }
                    if (skin.Kind != kind || skin.EquipmentDefinitionId != gear.ItemId) problems.Add(name + " wrong kind or item id");
                    if (AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(meshes + name + ".asset") == null) problems.Add(name + " baked mesh missing");
                    if (AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(skins + name + ".prefab") == null) problems.Add(name + " prefab missing");
                    var shade = new UnityEngine.Color(skin.Cloth.r * .45f, skin.Cloth.g * .45f, skin.Cloth.b * .45f, 1f);
                    if (skin.Shade != shade) problems.Add(name + " shade is not Cloth * 0.45");
                    if (!gear.Sampled && (skin.Cloth != gear.Cloth || skin.Patch != gear.Patch || skin.Binding != gear.Binding || skin.Thread != gear.Thread)) problems.Add(name + " colours differ from the table");
                    if (gear.Sampled && (skin.Cloth == gear.Cloth || skin.Thread != gear.Thread)) problems.Add(name + " merchant colours were not sampled");
                }
            }
            Check(problems.Count == 0, "every InventoryStorageGear shirt has a Pocket skin and every pack a Backpack and Opening skin, each with its own baked mesh, prefab and table colours" + (problems.Count == 0 ? "" : ": " + string.Join("; ", problems.Take(5))));
            var texts = all.Where(e => e.Sampled).Select(e => e.ItemId).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            Check(texts.SequenceEqual(new[] { "special.t1.pack", "special.t1.shirt", "special.t2.pack", "special.t2.shirt", "special.t3.pack", "special.t3.shirt", "special.t4.pack", "special.t4.shirt" }), "the eight merchant shirts and packs are sampled from their display prefabs");
        }

        private sealed class FixedClock : TheTrailWalker.Persistence.IClock { public DateTime UtcNow => new DateTime(2026, 11, 8, 0, 0, 0, DateTimeKind.Utc); }

        // Final review: tier 2-4 item IDs must survive a save (Validate + Save/Load) and Items.Find keeps PrototypeItems authoritative.
        private static void TierItemPersistenceChecks()
        {
            Check(ReferenceEquals(Items.Find("material.pine_log"), PrototypeItems.Log), "Items.Find returns the PrototypeItems instance for material.pine_log");
            string directory = Path.Combine(Path.GetTempPath(), "TrailWalkerP1-" + Guid.NewGuid().ToString("N"));
            try
            {
                TravelState travel = TravelState.Start(TravelCatalog.Prototype());
                var snapshot = new TheTrailWalker.Persistence.SessionSnapshot { version = 3, segmentId = travel.CurrentSegment.Id, survival = new TheTrailWalker.Persistence.SavedSurvival(), travel = TheTrailWalker.Persistence.SessionSaveStore.SaveTravel(travel) };
                snapshot.items.Add(new TheTrailWalker.Persistence.SavedItem { id = "tier-log", definitionId = "material.birch_log", owner = "Backpack", quantity = 400, quality = 1, ready = true });
                snapshot.items.Add(new TheTrailWalker.Persistence.SavedItem { id = "tier-fish", definitionId = "fish.walleye", owner = "Backpack", quantity = 100, quality = 1, ready = true });
                bool valid = true;
                try { TheTrailWalker.Persistence.SessionSaveStore.Validate(snapshot, float.MaxValue, out _); } catch (InvalidDataException) { valid = false; }
                Check(valid, "a save carrying material.birch_log and fish.walleye passes SessionSaveStore.Validate");
                var store = new TheTrailWalker.Persistence.SessionSaveStore(directory, new FixedClock(), 3);
                store.Save(snapshot);
                Check(store.Load(float.MaxValue, out var loaded, out _) == TheTrailWalker.Persistence.LoadStatus.Loaded && loaded.items.Count == 2 && loaded.items[0].definitionId == "material.birch_log" && loaded.items[1].definitionId == "fish.walleye", "tier 2-4 items round-trip through a SessionSaveStore Save/Load");
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }

        private static void ItemDefinitionChecks()
        {
            ItemDefinition[] tier = HarvestTierItems.All;
            Check(tier.Length == 29 && tier.Select(d => d.Id).Distinct().Count() == 29, "HarvestTierItems.All holds 29 distinct definitions (6 wood, 3 primary, 3 meat, tusk, 12 fish, 4 fishery)");
            var wood = new Regex(@"^material\.(birch|mesquite|oak)_(branch|log)$"); var animal = new Regex(@"^animal\.[a-z]+\.[a-z]+$");
            var fish = new Regex(@"^fish\.[a-z_]+$"); var fishery = new Regex(@"^fishery\.[a-z_]+$");
            Check(tier.All(d => wood.IsMatch(d.Id) || animal.IsMatch(d.Id) || fish.IsMatch(d.Id) || fishery.IsMatch(d.Id)), "every HarvestTierItems ID matches the section 6.2.7 pattern for its kind");
            var existing = new HashSet<string>(PrototypeItems.All.Select(d => d.Id));
            Check(!tier.Any(d => existing.Contains(d.Id)), "no HarvestTierItems ID also appears in PrototypeItems.All");
            Check(tier.All(d => Items.Find(d.Id) == d), "every HarvestTierItems definition resolves through Items.Find");
            uint hash = 2166136261; foreach (ItemDefinition d in PrototypeItems.All) foreach (char c in d.Id + "|") { hash ^= c; hash *= 16777619; }
            if (PrototypeItems.All.Length != 127 || hash != 644349003u)
                throw new Exception($"Phase 1 consistency: PrototypeItems.All changed (now {PrototypeItems.All.Length} IDs, hash {hash}). If adding/reordering items was intentional, update the snapshot count (127) and hash (644349003) in Phase1ConsistencyValidation.ItemDefinitionChecks.");
            Check(true, "PrototypeItems.All is unchanged: 127 IDs in the same order (FNV snapshot)");
            Check(Items.Find("material.pine_branch") == PrototypeItems.Branch && Items.Find("ingredient.mesquite_pods") == PrototypeItems.MesquitePods, "Items.Find still resolves PrototypeItems first");
        }
    }
}
