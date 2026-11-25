using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Pets;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Milestone 15 (spec 2026-11-24): pets and offline reconciliation. Each slice task adds a group of checks called from Run().
    public static class Milestone15Validation
    {
        public static void Run()
        {
            CatalogRows();
            SupplyLists();
            EffectiveWeights();
            SlotsAndPurchases();
            LoadRules();
            SaveRoundTrip();
            PetArt();
            SheetWords();
            BackyardScene();
            EntryAnchors();
            IdleTiming();
            IdleRolls();
            IdleClaims();
            IdleSave();
            IdleWords();
            Debug.Log("Milestone 15: pet checks passed.");
        }

        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Milestone 15: " + name); }

        private static void CatalogRows()
        {
            Check(PetCatalog.All.Count == 6, "CatalogRows: six pets");
            Check(PetCatalog.All.Select(p => p.Id).SequenceEqual(new[] { "pet.dog", "pet.cat", "pet.rabbit", "pet.pig", "pet.squirrel", "pet.tortoise" }), "CatalogRows: plan order and IDs");
            foreach (PetDefinition p in PetCatalog.All)
            {
                Check(p.Weights.Values.Sum() == 100, "CatalogRows: " + p.Id + " weights sum to 100");
                Check(p.PriceCash == 1000, "CatalogRows: " + p.Id + " costs 1,000 cash");
                Check(Resources.Load<GameObject>(p.StandingModel) != null && Resources.Load<GameObject>(p.SecondModel) != null, "CatalogRows: " + p.Id + " has both poses (" + p.StandingModel + ", " + p.SecondModel + ")");
            }
            Check(PetCatalog.All.Select(p => p.Specialty).Distinct().Count() == 6, "CatalogRows: distinct specialties");
            PetDefinition dog = PetCatalog.Find("pet.dog");
            Check(dog.Weights[SupplyGroup.Wood] + dog.Weights[SupplyGroup.StoneOre] == 85, "CatalogRows: Dog finds 85% building supplies");
            Check(PetCatalog.All.Count(p => p.Weights[SupplyGroup.Truffle] > 0) == 1 && PetCatalog.Find("pet.pig").Weights[SupplyGroup.Truffle] == 40, "CatalogRows: only the Pig finds truffles (40%)");
            // Each pet's largest group is its specialty's group, and no two pets share the same largest group.
            var largest = PetCatalog.All.Select(p => p.Weights.OrderByDescending(w => w.Value).First().Key).ToList();
            Check(largest.Distinct().Count() == 6, "CatalogRows: every pet leads with a different supply group (" + string.Join(",", largest) + ")");
        }

        // Every featured loose resource of RegionPlan is in exactly one group, except pelts and feathers; nothing else is.
        private static void SupplyLists()
        {
            var featured = RegionPlan.All.SelectMany(r => r.Featured).Distinct().ToList();
            foreach (string id in featured)
            {
                bool grouped = PetSupplies.TryGroup(id, out _);
                Check(grouped != PetSupplies.Excluded.Contains(id), "SupplyLists: " + id + (grouped ? " is grouped" : " is not grouped") + " (excluded: " + PetSupplies.Excluded.Contains(id) + ")");
            }
            foreach (string id in PetSupplies.All)
            {
                int region = PetSupplies.RegionOf(id);
                Check(region > 0, "SupplyLists: " + id + " is a RegionPlan featured resource");
                // Items of regions 1-18 exist at runtime (tiers 1-3); tier 4 items arrive with their regions.
                if (region <= 18) Check(Items.Find(id) != null, "SupplyLists: " + id + " (region " + region + ") is a runtime item");
            }
            foreach (string banned in new[] { "animal.rabbit.meat", "food.milk.cow", "ingredient.lavender", "fishery.squid", "item.pine_plank", "material.log" })
                Check(!PetSupplies.TryGroup(banned, out _), "SupplyLists: " + banned + " is never a pet find");
            Check(PetSupplies.Eligible(SupplyGroup.Seeds, new[] { 1, 2, 3, 4 }).Count == 0 && PetSupplies.Eligible(SupplyGroup.Truffle, new[] { 10 }).SequenceEqual(new[] { "ingredient.truffle" }), "SupplyLists: seeds and truffle wait for their regions");
            Check(PetSupplies.Eligible(SupplyGroup.StoneOre, new[] { 1, 2, 3, 4 }).SequenceEqual(new[] { "material.flint", "material.clay", "ingredient.rock_salt", "material.limestone" }), "SupplyLists: regions 1-4 stone/ore");
        }

        private static void EffectiveWeights()
        {
            var early = new[] { 1, 2, 3, 4 };
            foreach (PetDefinition p in PetCatalog.All)
            {
                var w = PetSupplies.Effective(p, early);
                Check(Math.Abs(w.Values.Sum() - 1) < 1e-9, "EffectiveWeights: " + p.Id + " sums to 1 in regions 1-4");
                Check(w[SupplyGroup.Seeds] == 0 && w[SupplyGroup.Truffle] == 0, "EffectiveWeights: " + p.Id + " finds no seeds or truffle in regions 1-4");
            }
            var pig = PetSupplies.Effective(PetCatalog.Find("pet.pig"), early);
            Check(Math.Abs(pig[SupplyGroup.StoneOre] - 15.0 / 50) < 1e-9, "EffectiveWeights: Pig's stone share renormalises (15 of the 50 eligible points)");
            var pigLater = PetSupplies.Effective(PetCatalog.Find("pet.pig"), Enumerable.Range(1, 10).ToList());
            Check(Math.Abs(pigLater[SupplyGroup.Truffle] - 0.4) < 1e-9, "EffectiveWeights: Pig finds truffles at 40% once region 10 is discovered");
            Check(PetSupplies.Effective(PetCatalog.Find("pet.dog"), new int[0]).Values.All(v => v == 0), "EffectiveWeights: nothing discovered gives all zero");
        }

        private static void SlotsAndPurchases()
        {
            int[] expected = { 0, 0, 1, 2, 2, 3, 3, 4 };
            for (int level = 1; level <= 8; level++) Check(PetState.Slots(level) == expected[level - 1], "SlotsAndPurchases: house level " + level + " holds " + expected[level - 1] + " pets");
            var now = new DateTime(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);
            PetDefinition dog = PetCatalog.Find("pet.dog"), cat = PetCatalog.Find("pet.cat"), pig = PetCatalog.Find("pet.pig");
            var s = new PetState(); var wallet = new Wallet(5000);
            Check(!s.TryBuy(dog, 2, wallet, now, out string r) && r == "Pets arrive at house level 3" && wallet.Cash == 5000, "SlotsAndPurchases: no pets below house level 3");
            Check(s.TryBuy(dog, 3, wallet, now, out _) && wallet.Cash == 4000 && s.Owns("pet.dog") && s.Active == null, "SlotsAndPurchases: buying never makes a pet active");
            Check(!s.TryBuy(dog, 8, wallet, now, out r) && r == "Already yours", "SlotsAndPurchases: a pet is bought once");
            Check(!s.TryBuy(cat, 3, wallet, now, out r) && r == "More room at house level 4" && wallet.Cash == 4000, "SlotsAndPurchases: a full pet home names the next level and charges nothing");
            Check(s.TryBuy(cat, 4, wallet, now, out _), "SlotsAndPurchases: the house upgrade raises the cap");
            Check(s.SetActive("pet.cat") && s.Active == "pet.cat" && !s.SetActive("pet.pig") && s.Active == "pet.cat", "SlotsAndPurchases: only an owned pet can be active");
            var poor = new Wallet(999);
            Check(!s.TryBuy(pig, 6, poor, now, out r) && r == "Not enough cash" && !s.Owns("pet.pig") && poor.Cash == 999, "SlotsAndPurchases: not enough cash leaves nothing changed");
            Check(s.TryBuy(pig, 6, wallet, now, out _) && s.Active == "pet.cat", "SlotsAndPurchases: a purchase keeps the active pet");
            Check(!s.TryBuy(PetCatalog.Find("pet.rabbit"), 6, wallet, now, out r) && r == "More room at house level 8", "SlotsAndPurchases: three pets fill a level 6 home");
            Check(s.Purchases.Count == 3 && s.Purchases.All(p => p.UtcTicks == now.Ticks), "SlotsAndPurchases: each purchase is recorded");
        }

        private static void LoadRules()
        {
            var s = PetState.Load(new[] { "pet.dog", "pet.unicorn", "pet.dog", "pet.cat" }, null, "pet.cat", out List<string> dropped);
            Check(s.Owned.SequenceEqual(new[] { "pet.dog", "pet.cat" }) && s.Active == "pet.cat" && dropped.SequenceEqual(new[] { "pet.unicorn", "pet.dog" }), "LoadRules: unknown and duplicate pets are dropped");
            var t = PetState.Load(new[] { "pet.dog" }, null, "pet.pig", out dropped);
            Check(t.Active == null && dropped.SequenceEqual(new[] { "pet.pig" }), "LoadRules: an active pet that is not owned is dropped");
            var u = PetState.Load(null, null, null, out dropped);
            Check(u.Owned.Count == 0 && u.Active == null && dropped.Count == 0, "LoadRules: no block means no pets");
        }

        private static void SaveRoundTrip()
        {
            var now = new DateTime(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);
            var s = new PetState(); var wallet = new Wallet(10000);
            s.TryBuy(PetCatalog.Find("pet.tortoise"), 4, wallet, now, out _); s.TryBuy(PetCatalog.Find("pet.dog"), 4, wallet, now.AddMinutes(5), out _); s.SetActive("pet.dog");
            var snapshot = new SessionSnapshot { pets = SessionSaveStore.SavePets(s) };
            var back = SessionSaveStore.RestorePets(snapshot);
            Check(back.Owned.SequenceEqual(s.Owned) && back.Active == "pet.dog" && back.Purchases.Select(p => p.PetId + p.UtcTicks).SequenceEqual(s.Purchases.Select(p => p.PetId + p.UtcTicks)), "SaveRoundTrip: owned, purchases and the active pet survive");
            var json = UnityEngine.JsonUtility.ToJson(snapshot);
            var reread = SessionSaveStore.RestorePets(UnityEngine.JsonUtility.FromJson<SessionSnapshot>(json));
            Check(reread.Owned.SequenceEqual(s.Owned) && reread.Active == "pet.dog", "SaveRoundTrip: through JSON");
            Check(SessionSaveStore.RestorePets(new SessionSnapshot()).Owned.Count == 0, "SaveRoundTrip: a save without the block has no pets");
            var odd = new SessionSnapshot { pets = new SavedPets { block = 1, owned = new List<string> { "pet.dragon", "pet.cat" }, active = "pet.dragon" } };
            var restored = SessionSaveStore.RestorePets(odd);
            Check(restored.Owned.SequenceEqual(new[] { "pet.cat" }) && restored.Active == null, "SaveRoundTrip: an unknown pet is dropped and the save still loads");
        }

        // Task 3: the shared pet bed holds the largest pet (the Dog) within 300 triangles, and each pet has its shop picture.
        private static void PetArt()
        {
            var bed = Resources.Load<GameObject>(PetArtAssets.BedResource);
            Check(bed != null && bed.transform.Find("PetAnchor") != null, "PetArt: the pet bed prefab loads with its PetAnchor");
            int tris = 0; foreach (var f in bed.GetComponentsInChildren<MeshFilter>(true)) for (int i = 0; i < f.sharedMesh.subMeshCount; i++) tris += (int)f.sharedMesh.GetIndexCount(i) / 3;
            Check(tris > 0 && tris <= 300, "PetArt: the pet bed stays within 300 triangles (" + tris + ")");
            Bounds Size(GameObject prefab)
            {
                var go = UnityEngine.Object.Instantiate(prefab);
                try { var rs = go.GetComponentsInChildren<Renderer>(); Bounds b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b; }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }
            Bounds bedBounds = Size(bed);
            foreach (PetDefinition p in PetCatalog.All)
            {
                Bounds pet = Size(Resources.Load<GameObject>(p.SecondModel));
                Check(pet.size.x <= bedBounds.size.x && pet.size.z <= bedBounds.size.z, "PetArt: " + p.Name + " (" + p.SecondModel + ") fits on the bed");
                var icon = Resources.Load<Sprite>(PetArtAssets.IconResource(p));
                Check(icon != null && icon.texture.width == 256, "PetArt: " + p.Name + " has its 256 px shop picture");
            }
        }

        // Task 5: what the pet shop and pet home sheets say.
        private static void SheetWords()
        {
            var early = new[] { 1, 2, 3, 4 };
            Check(CollectionPrototype.PetEffectiveLine(PetCatalog.Find("pet.dog"), early) == "Wood 47% · Stone 42% · Fibre 5% · Produce 5%", "SheetWords: Dog's line in regions 1-4 (" + CollectionPrototype.PetEffectiveLine(PetCatalog.Find("pet.dog"), early) + ")");
            Check(!CollectionPrototype.PetEffectiveLine(PetCatalog.Find("pet.pig"), early).Contains("Truffle"), "SheetWords: no truffle before region 10");
            Check(CollectionPrototype.PetEffectiveLine(PetCatalog.Find("pet.pig"), Enumerable.Range(1, 12).ToList()).StartsWith("Truffle 40%"), "SheetWords: truffle leads once region 10 is known");
            Check(CollectionPrototype.PetEffectiveLine(PetCatalog.Find("pet.cat"), new int[0]) == "Nothing to find yet", "SheetWords: nothing discovered");
            var now = new DateTime(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);
            var s = new PetState(); var rich = new Wallet(5000); var poor = new Wallet(10);
            PetDefinition dog = PetCatalog.Find("pet.dog"), cat = PetCatalog.Find("pet.cat");
            Check(CollectionPrototype.PetSlotsLine(s, 3) == "0 of 1" && CollectionPrototype.PetRowStatus(s, dog, 3, rich) == "1,000 cash", "SheetWords: an empty level 3 home");
            Check(CollectionPrototype.PetRowStatus(s, dog, 3, poor) == "1,000 cash", "SheetWords: an unaffordable pet still shows its price");
            Check(CollectionPrototype.PetBuyPrompt(s, dog, 3) == "Dog stays with you. 1 of 1 slots used.", "SheetWords: the buy prompt (" + CollectionPrototype.PetBuyPrompt(s, dog, 3) + ")");
            s.TryBuy(dog, 3, rich, now, out _);
            Check(CollectionPrototype.PetRowStatus(s, dog, 3, rich) == "Owned" && CollectionPrototype.PetRowStatus(s, cat, 3, rich) == "More room at house level 4" && CollectionPrototype.PetSlotsLine(s, 3) == "1 of 1",
                "SheetWords: owned, and a full home names the next level");
            Check(CollectionPrototype.PetRowStatus(new PetState(), dog, 2, rich) == "Pets arrive at house level 3", "SheetWords: below level 3");
        }

        // Task 4: the backyard holds the house tier's pet home and the active pet on its anchor; the interior teardown clears it.
        private static void BackyardScene()
        {
            Check(MovementPrototype.PetHomeFor(3) == "Settlement/Houses/PetHomeT2" && MovementPrototype.PetHomeFor(4) == "Settlement/Houses/PetHomeT2" && MovementPrototype.PetHomeFor(5) == "Settlement/Houses/PetHomeT3"
                && MovementPrototype.PetHomeFor(6) == "Settlement/Houses/PetHomeT3" && MovementPrototype.PetHomeFor(7) == "Settlement/Houses/PetHomeT4" && MovementPrototype.PetHomeFor(8) == "Settlement/Houses/PetHomeT4", "BackyardScene: pet home look by house level");
            foreach (var (resource, _, _) in MovementPrototype.BackyardProps) Check(Resources.Load<GameObject>(resource) != null, "BackyardScene: yard prop " + resource + " loads");
            var host = new GameObject("backyard check", typeof(MovementPrototype));
            try
            {
                var world = host.GetComponent<MovementPrototype>();
                foreach (int level in new[] { 3, 5, 7 })
                {
                    Transform yard = world.BuildBackyard(level, PetCatalog.Find("pet.tortoise").StandingModel);
                    string expected = MovementPrototype.PetHomeFor(level).Substring("Settlement/Houses/".Length);
                    Transform home = yard.Find(expected);
                    Check(home != null, "BackyardScene: level " + level + " shows " + expected);
                    Transform pet = yard.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ActivePet");
                    Check(pet != null && pet.parent.name == "ActivePetAnchor", "BackyardScene: the active pet stands on the ActivePetAnchor");
                    Check(yard.GetComponentsInChildren<Collider>(true).Length == 0, "BackyardScene: passive (no colliders)");
                }
                Transform empty = world.BuildBackyard(3, null);
                Check(empty.GetComponentsInChildren<Transform>(true).All(t => t.name != "ActivePet"), "BackyardScene: no active pet, no pet shown");
                world.ClearInterior();
                Check(host.transform.Cast<Transform>().All(t => t.name != MovementPrototype.InteriorHolderName), "BackyardScene: ClearInterior removes the backyard");
            }
            finally { UnityEngine.Object.DestroyImmediate(host); }
        }

        // Task 6: every interior with pets (house level 3 and up) has the bed anchor below the door.
        private static void EntryAnchors()
        {
            for (int level = 3; level <= 8; level++)
            {
                var interior = Resources.Load<GameObject>("Settlement/Houses/HouseL" + level.ToString("00") + "Interior");
                Check(interior != null && interior.GetComponentsInChildren<Transform>(true).Any(t => t.name == "PetBedAnchor"), "EntryAnchors: house level " + level + " interior has PetBedAnchor");
            }
            Check(CollectionPrototype.PetModelFor("pet.cat", false) == PetCatalog.Find("pet.cat").SecondModel && CollectionPrototype.PetModelFor("pet.none", true) == null, "EntryAnchors: the bed shows the active pet's second pose");
        }

        // ---------------------------------------------------------------- slice 2: offline idle (injected clock)

        private static readonly DateTime T0 = new DateTime(2026, 11, 24, 8, 0, 0, DateTimeKind.Utc);
        private static readonly int[] Early = { 1, 2, 3, 4 };

        private static PetIdle Away(PetIdle idle, string pet, int[] regions, DateTime from, TimeSpan away, ulong seed, out int made)
        {
            idle.BeginOffline(pet, regions, from);
            made = idle.Settle(from + away, seed);
            return idle;
        }

        private static void IdleTiming()
        {
            Check(PetIdle.DevelopmentUnits == 1000 && PetIdle.ReleaseUnits == 1 && PetIdle.UnitsPerRoll == PetIdle.DevelopmentUnits, "IdleTiming: 1,000 per roll in development, 1 in release");
            var idle = new PetIdle();
            Away(idle, "pet.dog", Early, T0, TimeSpan.FromMinutes(29), 7, out int made);
            Check(made == 0 && idle.CarriedTicks == TimeSpan.FromMinutes(29).Ticks && idle.CarryPetId == "pet.dog", "IdleTiming: 29 minutes give no roll and keep the carry");
            Away(idle, "pet.dog", Early, T0.AddHours(1), TimeSpan.FromMinutes(2), 7, out made);
            Check(made == 1 && idle.Waiting("pet.dog") == 1 && idle.CarriedTicks == TimeSpan.FromMinutes(1).Ticks, "IdleTiming: the carry plus 2 minutes completes one roll (31 minutes)");
            int again = idle.Settle(T0.AddHours(5), 7);
            Check(again == 0 && idle.Waiting("pet.dog") == 1, "IdleTiming: a repeated resume replays nothing");

            var capped = new PetIdle();
            Away(capped, "pet.tortoise", Early, T0, TimeSpan.FromHours(20), 7, out made);
            Check(made == 16 && capped.Waiting("pet.tortoise") == 16 && capped.CarriedTicks == 0, "IdleTiming: 20 hours away give the 8-hour cap of 16 rolls, nothing carried");
            Away(capped, "pet.tortoise", Early, T0.AddDays(1), TimeSpan.FromHours(3), 7, out made);
            Check(made == 0 && capped.Waiting("pet.tortoise") == 16 && capped.CarriedTicks == 0, "IdleTiming: a full queue stops generation and banks no time");

            var back = new PetIdle();
            back.Settle(T0.AddHours(10), 1);
            long watermark = back.LastSeenTicks;
            Away(back, "pet.cat", Early, T0.AddHours(10), TimeSpan.FromHours(-3), 1, out made);
            Check(made == 0 && back.LastSeenTicks == watermark && back.OfflineStartedTicks == 0, "IdleTiming: a backward clock earns nothing and the watermark does not move back");

            var none = new PetIdle();
            none.BeginOffline(null, Early, T0);
            made = none.Settle(T0.AddHours(6), 1);
            Check(made == 0 && none.OfflineStartedTicks == 0 && none.QueuedPets.Count() == 0, "IdleTiming: no active pet, no accrual and no catch-up");

            var twice = new PetIdle();
            twice.BeginOffline("pet.dog", Early, T0);
            twice.BeginOffline("pet.dog", Early, T0.AddMinutes(20));   // focus loss after pause: the first start stands
            Check(twice.Settle(T0.AddMinutes(61), 3) == 2, "IdleTiming: a second Begin keeps the first start");

            // Switch at 29/31: the carried 29 minutes finish with the Dog even though the Cat is active now.
            var swap = new PetIdle();
            Away(swap, "pet.dog", Early, T0, TimeSpan.FromMinutes(29), 5, out _);
            Away(swap, "pet.cat", Early, T0.AddHours(1), TimeSpan.FromMinutes(2), 5, out made);
            Check(made == 1 && swap.Waiting("pet.dog") == 1 && swap.Waiting("pet.cat") == 0, "IdleTiming: a partial interval completes with the pet it was earned with");
            Away(swap, "pet.cat", Early, T0.AddHours(2), TimeSpan.FromMinutes(31), 5, out made);
            Check(made == 1 && swap.Waiting("pet.cat") == 1, "IdleTiming: later rolls use the new pet");

            var empty = new PetIdle();
            Away(empty, "pet.dog", new int[0], T0, TimeSpan.FromHours(2), 9, out made);
            Check(made == 0 && empty.CarriedTicks == 0 && empty.Waiting("pet.dog") == 0, "IdleTiming: nothing eligible produces nothing and banks no reroll");
        }

        private static void IdleRolls()
        {
            var wood = 0; var birch = 0; var woodLater = 0; int n = 4000;
            for (int i = 0; i < n; i++)
            {
                PetFind f = PetIdle.RollFind(PetCatalog.Find("pet.dog"), Early, (ulong)i * 2654435761UL + 11);
                SupplyGroup g = SupplyGroup.Truffle;
                Check(f != null && PetSupplies.TryGroup(f.ItemId, out g) && PetSupplies.RegionOf(f.ItemId) <= 4, "IdleRolls: Dog finds only regions 1-4 supplies (" + f?.ItemId + ")");
                Check(f.Quality >= 1 && f.Quality <= 5 && f.Quantity == PetIdle.UnitsPerRoll, "IdleRolls: Q1-Q5 and the roll quantity");
                if (g == SupplyGroup.Wood) wood++;
                PetFind later = PetIdle.RollFind(PetCatalog.Find("pet.dog"), Enumerable.Range(1, 12).ToList(), (ulong)i * 40503UL + 3);
                if (PetSupplies.TryGroup(later.ItemId, out SupplyGroup lg) && lg == SupplyGroup.Wood) { woodLater++; if (later.ItemId == "material.birch_branch") birch++; }
            }
            double share = wood / (double)n;
            Check(Math.Abs(share - 45.0 / 95) < 0.03, "IdleRolls: Dog's wood share in regions 1-4 is about 47% (" + share.ToString("0.000") + ")");
            double high = birch / (double)woodLater;
            Check(Math.Abs(high - 0.7) < 0.04, "IdleRolls: 70% of wood comes from the highest discovered tier (" + high.ToString("0.000") + ")");
            Check(PetIdle.RollFind(PetCatalog.Find("pet.pig"), Early, 1) != null && Enumerable.Range(0, 500).All(i => PetIdle.RollFind(PetCatalog.Find("pet.pig"), Early, (ulong)i).ItemId != "ingredient.truffle"), "IdleRolls: no truffle before region 10");
        }

        private static void IdleClaims()
        {
            var idle = new PetIdle();
            Away(idle, "pet.dog", Early, T0, TimeSpan.FromHours(2), 21, out int made);
            Check(made == 4, "IdleClaims: two hours give four finds");
            var tiny = new SpatialInventory(100, 100);
            Check(!idle.Claim("pet.dog", idle.Queue("pet.dog")[0].EntryId, tiny) && idle.Waiting("pet.dog") == 4, "IdleClaims: a find that does not fit stays, untouched");
            Check(idle.ClaimAll("pet.dog", tiny) == 0 && idle.Waiting("pet.dog") == 4, "IdleClaims: Claim all stops at the bag's space");
            var bag = new SpatialInventory(8000, 8000);
            PetFind first = idle.Queue("pet.dog")[0];
            Check(idle.Claim("pet.dog", first.EntryId, bag) && idle.Waiting("pet.dog") == 3, "IdleClaims: a find that fits moves to the bag");
            ItemEntry stored = bag.Entries.FirstOrDefault(e => e.Definition.Id == first.ItemId && e.Owner == ItemOwner.Backpack);
            Check(stored != null && stored.Quality == first.Quality && stored.Quantity >= first.Quantity, "IdleClaims: the bag keeps the find's quality and quantity");
            Check(!idle.Claim("pet.dog", first.EntryId, bag), "IdleClaims: a find is claimed once");
            Check(idle.ClaimAll("pet.dog", bag) == 3 && idle.Waiting("pet.dog") == 0, "IdleClaims: Claim all takes the rest when it fits");
        }

        private static void IdleSave()
        {
            var s = new PetState(); var wallet = new Wallet(5000);
            s.TryBuy(PetCatalog.Find("pet.dog"), 3, wallet, T0, out _); s.SetActive("pet.dog");
            Away(s.Idle, "pet.dog", Early, T0, TimeSpan.FromMinutes(100), 33, out _);
            s.Idle.BeginOffline("pet.dog", Early, T0.AddHours(3));
            var json = UnityEngine.JsonUtility.ToJson(new SessionSnapshot { pets = SessionSaveStore.SavePets(s) });
            PetState back = SessionSaveStore.RestorePets(UnityEngine.JsonUtility.FromJson<SessionSnapshot>(json));
            Check(back.Idle.Queue("pet.dog").Select(f => f.EntryId + f.ItemId + f.Quality + f.Quantity).SequenceEqual(s.Idle.Queue("pet.dog").Select(f => f.EntryId + f.ItemId + f.Quality + f.Quantity)), "IdleSave: queued finds survive a save");
            Check(back.Idle.OfflineStartedTicks == s.Idle.OfflineStartedTicks && back.Idle.CarriedTicks == s.Idle.CarriedTicks && back.Idle.CarryPetId == "pet.dog" && back.Idle.IntervalId == s.Idle.IntervalId
                && back.Idle.NextEntry == s.Idle.NextEntry && back.Idle.SnapshotRegions.SequenceEqual(Early), "IdleSave: the open absence, carry and counters survive a save");
            int a = s.Idle.Settle(T0.AddHours(4), 33), b = back.Idle.Settle(T0.AddHours(4), 33);
            Check(a == b && back.Idle.Queue("pet.dog").Select(f => f.ItemId + f.Quality).SequenceEqual(s.Idle.Queue("pet.dog").Select(f => f.ItemId + f.Quality)), "IdleSave: settling after a reload gives the same finds (no reroll)");
            var older = new SessionSnapshot { pets = new SavedPets { block = 1, owned = new List<string> { "pet.cat" }, active = "pet.cat" } };
            PetState old = SessionSaveStore.RestorePets(older);
            Check(old.Active == "pet.cat" && old.Idle.OfflineStartedTicks == 0 && old.Idle.QueuedPets.Count() == 0, "IdleSave: a slice 1 save loads with nothing earned yet");
        }

        private static void IdleWords()
        {
            Check(CollectionPrototype.CollectionTitle(12) == "Collection · 12 / 16", "IdleWords: the collection title");
            Check(CollectionPrototype.FindLine(new PetFind { Quantity = 1000, Quality = 3 }) == "1,000 · Q3", "IdleWords: a find line");
            Check(CollectionPrototype.PetHomeLine(true, 0) == "With you" && CollectionPrototype.PetHomeLine(false, 3) == "At home · 3 waiting", "IdleWords: the pet home line");
        }
    }
}
