using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Persistence;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // First home, slice 2a (spec 2026-11-13). Chained after the settlement shell suite.
    public static class HomeValidation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Home: " + name); passed.Add(name); }
        private static bool Throws(Action action) { try { action(); return false; } catch (ArgumentException) { return true; } catch (InvalidOperationException) { return true; } }

        public static void Run()
        {
            passed.Clear();
            CatalogChecks();
            PlacementChecks();
            BuyChecks();
            BuffChecks();
            HookChecks();
            SaveChecks();
            HudChecks();
            InsideHudChecks();
            InteriorChecks();
            EntryChecks();
            PlacementViewChecks();
            StoreChecks();
            RotationChecks();
            RotationViewChecks();
            NeighbourChecks();
            UpgradeDomainChecks();
            UpgradeEscrowChecks();
            UpgradeSheetChecks();
            UpgradeCompletionChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Home-checks.txt", passed);
        }

        private static void CatalogChecks()
        {
            var all = FurnitureCatalog.All;
            Check(all.Count == 48, "The catalog holds 48 furniture types");
            Check(Enumerable.Range(1, 8).All(l => all.Count(d => d.HouseLevel == l) == 6), "Six furniture types per house level 1-8");
            Check(Enum.GetValues(typeof(BuffCategory)).Cast<BuffCategory>().All(c => all.Count(d => d.Category == c) == 4) && Enum.GetValues(typeof(BuffCategory)).Length == 12, "Four types per category across twelve categories");
            Check(all.Select(d => d.Id).Distinct().Count() == all.Count, "Furniture ids are unique");
            Check(FurnitureCatalog.Find("furniture.l1.worktable").PriceCash == 3000, "The worktable costs 3000");
            Check(FurnitureCatalog.Find("furniture.l7.master_loom").Strength == 2, "The master loom has strength 2");
            Check(all.Where(d => d.Category == BuffCategory.CraftQuality && d.Id != "furniture.l7.master_loom").All(d => d.Strength == 1), "Every other craft quality type has strength 1");
            Check(all.Where(d => d.Category != BuffCategory.CraftQuality).All(d => d.Strength == 1), "Every non-quality type has strength 1");
            Check(Throws(() => FurnitureCatalog.Find("furniture.nope")), "An unknown furniture id is refused");
            foreach (FurnitureDefinition d in all)
            {
                var prefab = Resources.Load<GameObject>("Settlement/Furniture/" + d.Asset);
                Check(prefab != null, d.Id + " prefab loads");
                var meta = prefab.GetComponent<FurnitureArtMetadata>();
                Check(meta != null && meta.FurnitureId == d.Id && meta.HouseLevel == d.HouseLevel && meta.PriceCash == d.PriceCash && meta.Footprint == new Vector2Int(d.Width, d.Depth) && meta.IsChest == d.IsChest && meta.BuffCategory == d.Category.ToString(), d.Id + " prefab metadata matches the catalog");
            }
            Check(FurnitureCatalog.UnlockedAt(1).Count() == 6 && FurnitureCatalog.UnlockedAt(2).Count() == 12, "House level 1 unlocks six types and level 2 unlocks twelve");
        }

        private static FurnitureDefinition F(string id) { return FurnitureCatalog.Find(id); }
        private static FurniturePiece P(HomeState h, string id) { return h.Pieces.First(p => p.InstanceId == id); }

        private static void PlacementChecks()
        {
            Check(HomeState.Grid(1) == (8, 5) && HomeState.Grid(2) == (10, 6) && HomeState.Grid(8) == (12, 17), "Grids for levels 1, 2 and 8");
            Check(HomeState.Grid(3) == (12, 7) && HomeState.Grid(9) == (12, 19) && HomeState.Grid(16) == (12, 33), "Grids for levels 3, 9 and 16");
            Check(Throws(() => HomeState.Grid(0)) && Throws(() => HomeState.Grid(17)), "Grid outside 1-16 is refused");
            var home = HomeState.New();
            Check(home.Level == 1 && home.GridWidth == 8 && home.GridDepth == 5 && home.Pieces.Count == 0, "A new home is level 1, 8x5 and empty");
            var table = F("furniture.l1.worktable");
            Check(!home.CanPlace(table, 7, 0) && home.CanPlace(table, 6, 4) && !home.CanPlace(table, -1, 0) && !home.CanPlace(table, 0, 5), "A worktable must sit inside the 8x5 grid");
            var w = new Wallet(1000000);
            var a = home.Buy(table, w, "a");
            Check(a.Placed && a.X == 3 && a.Z == 0, "The first worktable lands at (3, 0)");
            Check(!home.CanPlace(table, 4, 0) && !home.CanPlace(table, 2, 0) && home.CanPlace(table, 1, 0), "Overlap with a placed piece is refused");
            Check(home.CanPlace(table, 3, 0, "a") && home.CanPlace(table, 4, 0, "a"), "A piece may overlap its own cells when ignored");
            var b = home.Buy(table, w, "b");
            Check(b.Placed && b.X == 1 && b.Z == 0, "The second worktable lands at (1, 0) by the lower-x tie");
            var peg = home.Buy(F("furniture.l1.hat_peg"), w, "peg");
            Check(peg.Placed && peg.X == 5 && peg.Z == 0, "The hat peg takes the free row-0 cell closest to centre");
            Check(Throws(() => home.Move("nope", 0, 0)) && Throws(() => home.Store("nope")) && Throws(() => home.PlaceFromStorage("nope")), "Unknown instance ids are refused");

            {
                var h = HomeState.New(); var wallet = new Wallet(1000000); int k = 0;
                var list = FurnitureCatalog.UnlockedAt(1).Select(t => h.Buy(t, wallet, "p" + k++)).ToList();
                Check(list.Count == 6 && list.All(p => p.Placed), "All six level-1 types fit the 8x5 room");
            }
            for (int level = 1; level <= 8; level++)
            {
                var h = HomeState.AtLevel(level); var wallet = new Wallet(100000000); int n = 0;
                var pieces = FurnitureCatalog.UnlockedAt(level).Select(t => h.Buy(t, wallet, "p" + n++)).ToList();
                Check(pieces.All(p => p.Placed), "Every type unlocked at level " + level + " is placed in that home");
            }

            var full = HomeState.New(); var fw = new Wallet(1000000);
            // Centre-out placement leaves one-cell gaps at the row ends, so 3 worktables fit per row (15 in 8x5); the one-column gaps then take four turned ones (19).
            int fitted = 0;
            while (fitted < 25 && full.Buy(table, fw, "t" + fitted).Placed) fitted++;
            Check(fitted == 19 && full.Pieces.Count == 20, "Nineteen worktables fit the 8x5 room: fifteen flat, four turned into the end gaps");
            Check(!full.Pieces[19].Placed && fw.Cash == 1000000 - 20 * 3000, "The worktable that does not fit is bought into storage and charged");

            var m = HomeState.New(); var mw = new Wallet(1000000);
            var m1 = m.Buy(table, mw, "m1"); var m2 = m.Buy(table, mw, "m2");
            Check(!m.Move("m2", 3, 0) && m2.X == 1 && m2.Z == 0, "Move onto an occupied cell fails and keeps the old cell");
            Check(m.Move("m2", 6, 4) && m2.X == 6 && m2.Z == 4, "Move to a free cell succeeds");
            m.Store("m1");
            Check(!m1.Placed, "Store takes a piece off the grid");
            Check(m.PlaceFromStorage("m1") && m1.Placed && m1.X == 3 && m1.Z == 0, "Storage returns the piece to the nearest free cell");
            Check(!full.PlaceFromStorage("t19") && !full.Pieces[19].Placed, "Placing from storage into a full room is refused");
            full.Store("t0");
            Check(full.PlaceFromStorage("t19") && full.Pieces[19].Placed, "A freed spot lets a stored piece be placed");
        }

        private static void BuyChecks()
        {
            var table = F("furniture.l1.worktable");
            var home = HomeState.New(); var w = new Wallet(3000);
            string reason;
            Check(home.CanBuy(table, w, out reason) && reason == null, "A worktable is buyable with 3000");
            home.Buy(table, w, "a");
            Check(w.Cash == 0 && home.Pieces.Count == 1, "Buying leaves 0 cash");
            var poor = new Wallet(2999); var h2 = HomeState.New();
            Check(!h2.CanBuy(table, poor, out reason) && reason == "Not enough cash", "2999 cash refuses with Not enough cash");
            bool threw = false; try { h2.Buy(table, poor, "x"); } catch (InvalidOperationException e) { threw = e.Message == "Not enough cash"; }
            Check(threw && poor.Cash == 2999 && h2.Pieces.Count == 0, "A refused buy changes nothing");
            var l2 = F("furniture.l2.pine_chest");
            Check(!h2.CanBuy(l2, new Wallet(1000000), out reason) && reason == "Unlocks at house level 2", "A level-2 type is refused in a level-1 home");
            Check(!h2.CanBuy(null, new Wallet(1000000), out reason) && reason == "Unknown furniture", "A null definition is unknown furniture");
            var h3 = HomeState.New(); var w3 = new Wallet(1000000);
            h3.Buy(table, w3, "a"); h3.Buy(table, w3, "b");
            Check(h3.FirstPurchases.Count(id => id == table.Id) == 1 && h3.FirstPurchases.Count == 1, "FirstPurchases holds the id once after two buys");
        }

        private static FurnitureBuffs bedroll4() { var h = HomeState.New(); h.Buy(F("furniture.l1.bedroll"), new Wallet(1000000), "br4"); return h.Buffs; }
        private static bool Near(double a, double b) { return Math.Abs(a - b) < 1e-9; }

        private static void BuffChecks()
        {
            var none = HomeState.New().Buffs;
            Check(none.CraftQuality == 0 && Near(none.FoodRecovery, 0) && Near(none.SalePrice, 0) && Near(none.DurabilityOf(BuffCategory.BootsLife), 0) && Near(none.DurabilityOf(BuffCategory.RodLife), 0), "An empty home has no buffs");
            var table = F("furniture.l1.worktable"); var w = new Wallet(100000000);
            var h = HomeState.New();
            h.Buy(table, w, "a");
            Check(h.Buffs.CraftQuality == 1, "A placed worktable gives craft quality 1");
            h.Buy(table, w, "b");
            Check(h.Buffs.CraftQuality == 1, "Two worktables still give craft quality 1");
            h.Store("a"); h.Store("b");
            Check(h.Buffs.CraftQuality == 0, "Stored worktables give nothing");

            var big = HomeState.AtLevel(7);
            foreach (string id in new[] { "furniture.l1.worktable", "furniture.l3.loom", "furniture.l5.artisan_bench", "furniture.l7.master_loom" }) big.Buy(F(id), w, "q" + id);
            Check(big.Pieces.All(p => p.Placed) && big.Buffs.CraftQuality == 5, "Worktable, loom, artisan bench and master loom give craft quality 5");

            var cook = HomeState.AtLevel(3);
            cook.Buy(F("furniture.l1.bedroll"), w, "c1"); cook.Buy(F("furniture.l3.clay_cookstove"), w, "c2");
            Check(Near(cook.Buffs.FoodRecovery, 0.10), "Bedroll and clay cookstove give food recovery 0.10");
            var sign = HomeState.New(); sign.Buy(F("furniture.l1.market_sign"), w, "s1");
            Check(Near(sign.Buffs.SalePrice, 0.02), "A market sign gives sale price 0.02");
            var boots = HomeState.AtLevel(3);
            boots.Buy(F("furniture.l1.boot_bench"), w, "b1"); boots.Buy(F("furniture.l3.sofa"), w, "b2");
            Check(Near(boots.Buffs.DurabilityOf(BuffCategory.BootsLife), 0.10), "Boot bench and sofa give boots life 0.10");
            Check(Near(boots.Buffs.DurabilityFor(TheTrailWalker.Domain.Items.PrototypeItems.TestBoots), 0.10), "Boots output gets the boots life buff");
            Check(Near(boots.Buffs.DurabilityFor(TheTrailWalker.Domain.Items.PrototypeItems.Tool), 0) && Near(boots.Buffs.DurabilityFor(TheTrailWalker.Domain.Items.PrototypeItems.TestHat), 0), "A pine axe and a hat get no boots buff");
            var axe = HomeState.AtLevel(2); axe.Buy(F("furniture.l2.chopping_block"), w, "x1");
            Check(Near(axe.Buffs.DurabilityFor(TheTrailWalker.Domain.Items.PrototypeItems.Tool), 0.05) && Near(FurnitureBuffs.None.DurabilityFor(null), 0), "A chopping block raises pine axe life by 0.05");
        }

        private static int hookIds;
        private static string HookId() { return "hk" + (++hookIds); }

        private static void HookChecks()
        {
            var w = new Wallet(100000000);
            var hat = TheTrailWalker.Domain.Crafting.RecipeBook.Find("recipe.wearable.rabbit_hat");
            var bootsRecipe = TheTrailWalker.Domain.Crafting.RecipeBook.Find("recipe.wearable.rabbit_boots");
            var worktable = HomeState.New(); worktable.Buy(F("furniture.l1.worktable"), w, "wt");
            var buffed = worktable.Buffs;

            // Craft quality, snapshotted in the batch.
            var state = new TheTrailWalker.Domain.Crafting.CraftingState(); state.Learn(hat.Id);
            var bag = new TheTrailWalker.Domain.Inventory.SpatialInventory(14000, 4000);
            var pelts = bag.AddWorld("pelts", TheTrailWalker.Domain.Items.PrototypeItems.RabbitPelt, 10, 1);
            bag.Reserve(pelts, 1); bag.TryQuickStore(pelts, 1, out _); bag.Release(pelts, 1);
            Check(TheTrailWalker.Domain.Crafting.CraftingService.Preview(bag, state, hat, 1).Result.Quality == 1, "An unbuffed level-1 craft has quality 1");
            Check(TheTrailWalker.Domain.Crafting.CraftingService.Preview(bag, state, hat, 1, buffed).Result.Quality == 2, "A worktable raises a level-1 craft to quality 2");
            var t0 = new DateTime(2026, 11, 13, 12, 0, 0, DateTimeKind.Utc);
            var made = TheTrailWalker.Domain.Crafting.CraftingService.Craft(bag, state, hat, 1, t0, HookId, buffed);
            Check(made.Success && made.Batch != null && made.Batch.Quality == 2 && made.Outputs[0].Quality == 2, "A buffed craft makes quality 2 and the batch records it");
            worktable.Store("wt");
            Check(made.Batch.Quality == 2 && made.Outputs[0].Quality == 2, "The pending batch keeps quality 2 after the worktable is stored");
            Check(TheTrailWalker.Domain.Crafting.CraftingService.Preview(bag, state, hat, 1, worktable.Buffs).Result.Quality == 1, "The next preview shows quality 1");
            Check(made.Outputs[0].Profile.Variant == TheTrailWalker.Domain.Crafting.CraftingService.CraftedVariant && made.Outputs[0].Profile.Variant == "crafted", "Crafted outputs carry the crafted variant");

            // Durability.
            var bench = HomeState.AtLevel(1); bench.Buy(F("furniture.l1.boot_bench"), w, "bb");
            var bootsPreview = TheTrailWalker.Domain.Crafting.CraftingService.Preview(bag, state, bootsRecipe, 1, bench.Buffs).Result;
            Check(bootsPreview.MaxDurability == TheTrailWalker.Domain.Crafting.CraftStats.Compute(bootsRecipe, 0, 1, 0, 0.05).MaxDurability
                && bootsPreview.MaxDurability > TheTrailWalker.Domain.Crafting.CraftStats.Compute(bootsRecipe, 0, 1).MaxDurability, "A boot bench raises boots max durability by 5 percent");

            // Food.
            var bedroll = HomeState.New(); bedroll.Buy(F("furniture.l1.bedroll"), w, "br");
            Check(Near(FurnitureBuffs.FoodHearts(4, bedroll.Buffs), 4.2) && Near(FurnitureBuffs.FoodHearts(4, FurnitureBuffs.None), 4), "A bedroll lifts 4 food hearts to 4.2");

            // Sale.
            var ledger = new TheTrailWalker.Domain.Inventory.SpatialInventory(14000, 4000);
            var crafted = ledger.AddWorld("c1", TheTrailWalker.Domain.Items.PrototypeItems.Branch, 1, 1, new TheTrailWalker.Domain.Items.ItemProfile(1000, 0, 0, TheTrailWalker.Domain.Crafting.CraftingService.CraftedVariant));
            var bought = ledger.AddWorld("b1", TheTrailWalker.Domain.Items.PrototypeItems.Branch, 1, 1, new TheTrailWalker.Domain.Items.ItemProfile(1000));
            Check(SaleQuote.PortionMinor(crafted, crafted.Pile.First(), 0.02) == 1020 && SaleQuote.PortionMinor(bought, bought.Pile.First(), 0.02) == 1000, "A crafted portion gets the sale bonus and a bought portion does not");
            Check(SaleQuote.TotalMinor(crafted, 0.02) == 1020 && SaleQuote.TotalMinor(bought, 0.02) == 1000 && SaleQuote.TotalMinor(crafted) == 1000, "Totals apply the bonus to crafted portions only");

            // The craft-quality cap.
            var top = HomeState.AtLevel(8);
            foreach (FurnitureDefinition d in FurnitureCatalog.UnlockedAt(8)) top.Buy(d, w, "top" + d.Id);
            Check(top.Buffs.CraftQuality <= 5, "Craft quality never exceeds the cap of 5");
        }

        private static SessionSnapshot Snap(SavedHome home) { return new SessionSnapshot { home = home }; }
        private static SavedHome Block(int level, params SavedPiece[] pieces)
        {
            return new SavedHome { block = 1, level = level, pieces = new List<SavedPiece>(pieces), firstPurchases = new List<string>() };
        }
        private static SavedPiece Pc(string id, string def, bool placed, int x, int z) { return new SavedPiece { id = id, definition = def, placed = placed, x = x, z = z }; }
        private static bool Refused(SavedHome block)
        {
            try { SessionSaveStore.RestoreHome(Snap(block)); return false; } catch (InvalidDataException) { return true; }
        }

        private static void SaveChecks()
        {
            var h = HomeState.AtLevel(3); var w = new Wallet(100000000);
            h.Buy(F("furniture.l1.worktable"), w, "a"); h.Buy(F("furniture.l3.sofa"), w, "b"); h.Buy(F("furniture.l1.hat_peg"), w, "c");
            h.Store("c");
            SavedHome saved = SessionSaveStore.SaveHome(h);
            HomeState back = SessionSaveStore.RestoreHome(Snap(saved));
            bool same = back.Level == 3 && back.Pieces.Count == 3 && back.FirstPurchases.SequenceEqual(h.FirstPurchases);
            for (int i = 0; i < 3 && same; i++)
            {
                FurniturePiece p = h.Pieces[i], q = back.Pieces[i];
                same = p.InstanceId == q.InstanceId && p.DefinitionId == q.DefinitionId && p.Placed == q.Placed && p.X == q.X && p.Z == q.Z;
            }
            Check(saved.block == 1 && same, "A home with two placed pieces and one stored round-trips");
            string json = JsonUtility.ToJson(Snap(saved));
            Check(SessionSaveStore.RestoreHome(JsonUtility.FromJson<SessionSnapshot>(json)).Pieces.Count == 3, "A home round-trips through JSON");
            HomeState fresh = SessionSaveStore.RestoreHome(JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(new SessionSnapshot())));
            Check(fresh.Level == 1 && fresh.Pieces.Count == 0 && SessionSaveStore.RestoreHome(new SessionSnapshot { home = null }).Level == 1, "A snapshot without a home restores a new home");

            Check(!Refused(Block(1, Pc("a", "furniture.l1.worktable", true, 0, 0))), "A valid forged-style block is accepted");
            Check(Refused(Block(1, Pc("a", "furniture.nope", true, 0, 0))), "An unknown definition is refused");
            Check(Refused(Block(1, Pc("a", "furniture.l2.trouser_rack", true, 0, 0))), "A level-2 type in a level-1 home is refused");
            Check(Refused(Block(1, Pc("a", "furniture.l1.worktable", true, 0, 0), Pc("b", "furniture.l1.worktable", true, 1, 0))), "Overlapping placed pieces are refused");
            Check(Refused(Block(1, Pc("a", "furniture.l1.hat_peg", true, 8, 0))), "A piece at x = 8 in 8x5 is refused");
            Check(Refused(Block(1, Pc("a", "furniture.l1.worktable", true, 7, 0))), "A piece running past the grid edge is refused");
            Check(Refused(Block(1, Pc("a", "furniture.l1.hat_peg", true, 0, 0), Pc("a", "furniture.l1.hat_peg", true, 2, 0))), "A duplicate instance id is refused");
            Check(Refused(Block(1, Pc("", "furniture.l1.hat_peg", true, 0, 0))), "An empty instance id is refused");
            Check(Refused(Block(9)) && Refused(Block(0)), "Level 9 and level 0 are refused");
            Check(!Refused(Block(1, Pc("a", "furniture.l1.worktable", true, 0, 0), Pc("b", "furniture.l1.worktable", false, 0, 0))), "Stored pieces never overlap");
            var nullPiece = Block(1); nullPiece.pieces.Add(null);
            Check(Refused(nullPiece), "A null piece entry is refused");

            var street = TheTrailWalker.Domain.Travel.SettlementState.New(1);
            street.ChoosePlot(4, new string[0]);
            int[] levels = TheTrailWalker.Travel.SettlementScenery.HouseLevels(street, 3);
            Check(levels[4] == 3 && levels.Where((l, i) => i != 4).All(l => l == 1), "The player's plot shows the home level on the street and neighbours are unchanged");
            Check(TheTrailWalker.Travel.SettlementScenery.HouseLevels(street)[4] == 1, "The one-argument overload shows level 1");
        }

        // Street taps and the street camera (2a task 5; the front view itself went in home revision 4, see InsideHudChecks).
        private static void HudChecks()
        {
            var neighbour = new TheTrailWalker.Travel.StreetBuilding { Id = "plot.2", Kind = TheTrailWalker.Travel.StreetBuildingKind.House, Plot = 2 };
            var mine = new TheTrailWalker.Travel.StreetBuilding { Id = "plot.4", Kind = TheTrailWalker.Travel.StreetBuildingKind.House, Plot = 4 };
            var bakery = new TheTrailWalker.Travel.StreetBuilding { Id = "shop.bakery", Kind = TheTrailWalker.Travel.StreetBuildingKind.Shop };
            Check(CollectionPrototype.SettlementTapAction(mine, false, 4, 0) == CollectionPrototype.StreetTapAction.OpenHome, "Tapping the player's plot after the plot choice opens the home");
            Check(CollectionPrototype.SettlementTapAction(neighbour, false, 4, 9) == CollectionPrototype.StreetTapAction.Note
                && CollectionPrototype.SettlementNote(neighbour, 9, false) == "A neighbour's home (level 9)", "Tapping a neighbour's house above level 8 still shows its paper note");
            Check(CollectionPrototype.SettlementTapAction(mine, true, -1, 0) == CollectionPrototype.StreetTapAction.ChoosePlot
                && CollectionPrototype.SettlementTapAction(neighbour, true, -1, 0) == CollectionPrototype.StreetTapAction.ChoosePlot, "During the plot choice a house tap asks for the plot");
            Check(CollectionPrototype.SettlementTapAction(bakery, false, 4, 0) == CollectionPrototype.StreetTapAction.Note && CollectionPrototype.SettlementTapAction(bakery, true, -1, 0) == CollectionPrototype.StreetTapAction.Note,
                "A shop tap shows its note before and after the plot choice");

            TheTrailWalker.Travel.StreetCamera.StreetPose(12f, out Vector3 eye, out Quaternion look);
            Check(Mathf.Approximately(eye.x, 12f) && Mathf.Approximately(eye.y, TheTrailWalker.Travel.StreetCamera.Height) && Mathf.Approximately(eye.z, -TheTrailWalker.Travel.StreetCamera.Distance)
                && Vector3.Angle(look * Vector3.forward, new Vector3(0f, TheTrailWalker.Travel.StreetCamera.LookHeight - TheTrailWalker.Travel.StreetCamera.Height, TheTrailWalker.Travel.StreetCamera.Distance)) < 0.01f,
                "The street pose at a plot's X is the usual street camera pose");

            Check(CollectionPrototype.HouseDebugButtonVisible(false) && !CollectionPrototype.HouseDebugButtonVisible(true), "The debug HOUSES button shows on the street and hides while a home view is open");
            var cameraObject = new GameObject("Home check camera");
            try
            {
                var cam = cameraObject.AddComponent<TheTrailWalker.Travel.StreetCamera>();
                cam.Bind(new TheTrailWalker.Travel.SettlementStreet { MinX = 0f, MaxX = 50f, ShopsCentreX = 10f });
                cam.HoldAt(new Vector3(3f, 2.4f, -6f), Quaternion.Euler(10f, 180f, 0f));
                cam.CentreOn(20f);
                bool heldAway = !cam.OnStreet && cameraObject.transform.position == new Vector3(3f, 2.4f, -6f);
                cam.SnapToStreet();
                TheTrailWalker.Travel.StreetCamera.StreetPose(20f, out Vector3 streetEye, out Quaternion streetLook);
                Check(heldAway && cam.OnStreet && cameraObject.transform.position == streetEye && Quaternion.Angle(cameraObject.transform.rotation, streetLook) < 0.01f,
                    "Held away (the interior cut), street moves leave the camera alone; a home reset snaps it back to the street pose");
            }
            finally { UnityEngine.Object.DestroyImmediate(cameraObject); }
        }

        // Slice 2b task 3: the upgrade sheet's pure parts.
        private static void UpgradeSheetChecks()
        {
            Check(CollectionPrototype.HomeUpgradeOpensSheet(1) && !CollectionPrototype.HomeUpgradeOpensSheet(2), "The Upgrade button opens the sheet below the player's cap (level 1) and not at it (level 2)");
            Check(!CollectionPrototype.HomeUpgradeTapShowsCapNote(true, false, 1) && CollectionPrototype.HomeUpgradeTapShowsCapNote(true, false, 2) && !CollectionPrototype.HomeUpgradeTapShowsCapNote(true, true, 2) && !CollectionPrototype.HomeUpgradeTapShowsCapNote(false, false, 2),
                "A tap on Upgrade shows the cap note only inside the player's own room at the cap, never when the sheet opens");
            Check(CollectionPrototype.HomeUpgradeCapNote == "Your house is as big as it gets for now.", "The cap note reads \"Your house is as big as it gets for now.\"");
            Check(CollectionPrototype.UpgradeTitle(1) == "House level 1  →  2", "The sheet title reads \"House level 1  →  2\"");
            Check(CollectionPrototype.UpgradeCount(12, 25) == "12 / 25", "A row's count reads \"12 / 25\"");
            Check(CollectionPrototype.DepositPrompt(12, "Pine beam") == "Deposit 12 Pine beam? Deposits can't be taken back.", "The deposit prompt names the units and the item and says deposits can't be taken back");
        }

        // Slice 2b task 4: the completion on screen (fireworks, their material, the debug deposit) and the completion data path.
        private static void UpgradeCompletionChecks()
        {
            Material mat = Resources.Load<Material>("Effects/FireworkParticle");
            Check(mat != null && mat.shader != null && mat.shader.name == "Universal Render Pipeline/Particles/Unlit", "The firework material loads from Resources/Effects and uses the URP particle unlit shader");

            TheTrailWalker.Travel.HouseFireworks fireworks = TheTrailWalker.Travel.HouseFireworks.Play(Vector3.zero);
            try
            {
                var ps = fireworks != null ? fireworks.GetComponent<ParticleSystem>() : null;
                Check(ps != null && ps.main.useUnscaledTime, "The fireworks run on unscaled time");
                var bursts = new ParticleSystem.Burst[8];
                int count = ps != null ? ps.emission.GetBursts(bursts) : 0;
                float[] expected = { 0f, 0.7f, 1.4f, 2.1f };
                bool times = count == 4;
                for (int i = 0; times && i < 4; i++) times &= Mathf.Abs(bursts[i].time - expected[i]) < 0.01f;
                Check(times, "The fireworks have four bursts at 0, 0.7, 1.4 and 2.1 seconds");
                fireworks.Stop();
                Check(!ps.emission.enabled || !ps.isEmitting, "Stop() ends the emission");
            }
            finally { if (fireworks != null) UnityEngine.Object.DestroyImmediate(fireworks.gameObject); }

            Check(CollectionPrototype.DebugDepositCaption == "+ DEPOSIT", "The debug button reads \"+ DEPOSIT\"");
            Check(CollectionPrototype.UpgradeOpenCompletes(false, true, true) && !CollectionPrototype.UpgradeOpenCompletes(true, true, true)
                && !CollectionPrototype.UpgradeOpenCompletes(false, false, true) && !CollectionPrototype.UpgradeOpenCompletes(false, true, false),
                "Opening the sheet on a fully deposited house completes it, unless the save is blocked, nothing is ready or the house is at its cap");

            // Completion data path: deposits, Complete, a save round trip.
            var settle = TheTrailWalker.Domain.Travel.SettlementState.New(77); settle.ChoosePlot(3, new string[0]);
            var home = HomeState.Restore(1, new[] { ("p1", "furniture.l1.worktable", true, 2, 0, 0) }, null);
            var ledger = UpgradeLedger();
            foreach (string m in UpgradeMaterials)
            {
                Put(ledger, "src-" + m, m, m == Beam ? 24 : 25, 2, ItemOwner.Backpack, 0, 0);
                HouseUpgradeService.Deposit(home, ledger, m, NewEscrowId, out _);
            }
            Put(ledger, "last", Beam, 1, 3, ItemOwner.Backpack, 0, 0);
            HouseUpgradeService.Deposit(home, ledger, Beam, NewEscrowId, out _);
            Check(HouseUpgradeService.Ready(home, ledger) && HouseUpgradeService.Complete(home, ledger), "Completion data path: a fully deposited level 1 home completes");
            // The stranded case: the last deposit was saved but completion was not, so the reloaded level 1 home holds every material at 25.
            {
                var home2 = HomeState.Restore(1, new[] { ("p1", "furniture.l1.worktable", true, 2, 0, 0) }, null);
                var ledger2 = UpgradeLedger();
                foreach (string m in UpgradeMaterials)
                {
                    Put(ledger2, "src2-" + m, m, 25, 2, ItemOwner.Backpack, 0, 0);
                    HouseUpgradeService.Deposit(home2, ledger2, m, NewEscrowId, out _);
                }
                SessionSnapshot stranded = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(UpgradeSnapshot(home2, ledger2)));
                SessionSaveStore.Validate(stranded, float.MaxValue, out var ledger3);
                HomeState home3 = SessionSaveStore.RestoreHome(stranded);
                Check(home3.Level == 1 && HouseUpgradeService.Ready(home3, ledger3) && HouseUpgradeService.Complete(home3, ledger3) && home3.Level == 2,
                    "A level 1 home restored from a save with all five materials at 25 is ready and completes");
            }
            SessionSnapshot back = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(UpgradeSnapshot(home, ledger)));
            SessionSaveStore.Validate(back, float.MaxValue, out _);
            HomeState saved = SessionSaveStore.RestoreHome(back);
            Check(saved.Level == 2 && saved.GridWidth == 10 && saved.GridDepth == 6 && saved.UpgradeEscrow.Count == 0 && saved.Pieces.Count == 1 && saved.Pieces[0].X == 3 && saved.Pieces[0].Z == 0,
                "The saved level 2 home is 10 x 6 with its piece shifted one cell and no deposits left");
            int[] levels = TheTrailWalker.Travel.SettlementScenery.HouseLevels(settle, saved.Level), others = TheTrailWalker.Travel.SettlementScenery.HouseLevels(settle, 1);
            Check(levels[3] == 2 && levels.Where((l, i) => i != 3).SequenceEqual(others.Where((l, i) => i != 3)), "The street shows level 2 at the player's plot and leaves the neighbours alone");
        }

        // Home revision 4 (spec sections 1 and 4): a tap goes straight inside; every house action is on the inside HUD.
        private static void InsideHudChecks()
        {
            Check(Enum.GetNames(typeof(CollectionPrototype.HomeView)).SequenceEqual(new[] { "None", "Interior", "BirdsEye" }), "The home views are None, Interior and BirdsEye (no front view)");
            const System.Reflection.BindingFlags all = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance;
            string[] gone = { "HomeFrontButtonNames", "HomeFrontCaptions", "HomeFrontIcons", "HomeInteriorSoonNote", "OpenHomeFront", "HomeCameraPose", "CloseHomeInterior",
                "HomeInteriorButtonNames", "HomeInteriorCaptions", "HomeInteriorIcons" };
            string left = string.Join(", ", gone.Where(n => typeof(CollectionPrototype).GetMember(n, all).Length > 0));
            Check(left.Length == 0, "The front view's and the old interior row's members are gone" + (left.Length > 0 ? " (still there: " + left + ")" : ""));
            Check(typeof(CollectionPrototype).GetMethod("OpenHome", all) != null && typeof(CollectionPrototype).GetMethod("LeaveHome", all) != null, "OpenHome (glide, then cut inside) and LeaveHome (the return icon) exist");

            Check(!CollectionPrototype.HomePetButtonVisible(1) && !CollectionPrototype.HomePetButtonVisible(2) && CollectionPrototype.HomePetButtonVisible(3) && CollectionPrototype.HomePetButtonVisible(8),
                "The Pet button is hidden below house level 3 and shown from level 3");
            Check(CollectionPrototype.CraftAvailable(false, true) && !CollectionPrototype.CraftAvailable(false, false) && CollectionPrototype.CraftAvailable(true, false),
                "CRAFT is available at a camp and inside the house, nowhere else");
            Check(CollectionPrototype.HomePetNote == "The backyard opens with pets.", "The Pet note reads \"The backyard opens with pets.\"");

            Check(CollectionPrototype.HomeInsideButtonNames.SequenceEqual(new[] { "Home: Return", "Home: Store", "Home: Upgrade", "Home: Pet" }),
                "The inside buttons are named Home: Return, Home: Store, Home: Upgrade and Home: Pet");
            var slots = CollectionPrototype.HomeInsideButtonSlots;
            Check(slots.Length == 3 && slots[0] == ("Home: Return", false, 0, 1) && slots[1] == ("Home: Store", false, 1, 0) && slots[2] == ("Home: Upgrade", true, 1, 0),
                "Return sits left column 0 row 1 (above the inventory), Store left column 1, Upgrade right column 1 (the drop pocket's spot)");
            Check(slots.All(s => s.Name != "Home: Pet"), "Pet is centred between them, outside the column grid");
            Check(CollectionPrototype.HomeInsideIcons.SequenceEqual(new[] { "return", "store", "upgrade", "pet" }), "The inside buttons use the return, store, upgrade and pet icons");
            Check(new[] { "return", "store", "upgrade", "pet" }.All(key => Resources.Load<Sprite>("UI/Icons/" + key) != null), "The return, store, upgrade and pet icons load as sprites from UI/Icons");

            var mine = new TheTrailWalker.Travel.StreetBuilding { Id = "plot.4", Kind = TheTrailWalker.Travel.StreetBuildingKind.House, Plot = 4 };
            Check(CollectionPrototype.SettlementTapAction(mine, false, 4, 0) == CollectionPrototype.StreetTapAction.OpenHome, "Tapping the player's house still opens the home (straight inside)");
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        // Interior view (task 6): the interior prefab under its holder, placed pieces at their cells, one touch box per piece,
        // the scroll rule and the interior HUD's names and notes.
        private static void InteriorChecks()
        {
            Check(CollectionPrototype.HomeChestNote == "Chests open in a later update.", "A short tap on a chest shows \"Chests open in a later update.\"");
            Check(Mathf.Approximately(CollectionPrototype.HomeHoldSeconds, 0.4f), "A hold of 0.4 s on a piece enters the bird's-eye view");
            // Home revision 3 framing (user, November 14): a closer, steeper camera fitted at runtime; rooms scroll only when the
            // back wall's top would leave the screen. Synthetic rooms here; the eight real ones are measured in EntryChecks.
            Check(Mathf.Approximately(CollectionPrototype.EntryDepth, 2f) && CollectionPrototype.InteriorPitch >= 60f && CollectionPrototype.InteriorPitch <= 70f,
                "The entry strip is 2 m deep and the interior camera looks " + CollectionPrototype.InteriorPitch + " degrees down (60-70)");
            {
                float aspect = 1116f / 2480f, fov = TheTrailWalker.Travel.StreetCamera.FieldOfView;
                var deep = CollectionPrototype.InteriorFraming(6f, 0f, 2.2f, new Vector3(-3f, 2.92f, -16.69f), fov, aspect);   // a 12 x 33 room (level 16 grid)
                float farTop = CollectionPrototype.InteriorViewport(deep, deep.Lo, new Vector3(-3f, 2.92f, -16.69f), fov, aspect).y;
                float nearTop = CollectionPrototype.InteriorViewport(deep, deep.Hi, new Vector3(-3f, 2.92f, -16.69f), fov, aspect).y;
                Check(deep.Scrolls && deep.Hi == 0f && nearTop > 1f && farTop >= 0.88f && farTop <= 1f,
                    "A room taller than the screen scrolls; at the far end the back wall's top sits in the top 12% (12 x 33: " + deep.Lo.ToString("0.00") + " m, top at " + farTop.ToString("0.000") + ")");
                var small = CollectionPrototype.InteriorFraming(4f, 0f, 2.2f, new Vector3(-2f, 2.22f, -2.69f), fov, aspect);
                Check(!small.Scrolls && small.Lo == 0f && small.Hi == 0f, "A room that fits the screen does not scroll");
            }

            var worldObject = new GameObject("Home interior check");
            try
            {
                var world = worldObject.AddComponent<MovementPrototype>();
                Transform root = world.BuildInterior(1);
                Check(root != null && FindDeep(root, "InteriorCameraAnchor") != null && FindDeep(root, "ScrollMin") != null && FindDeep(root, "ScrollMax") != null,
                    "BuildInterior(1) returns a root with InteriorCameraAnchor, ScrollMin and ScrollMax");
                Check(root.parent != null && root.parent.position == new Vector3(0f, -200f, 0f) && root.name.StartsWith("HouseL01Interior"), "The level-1 interior sits under a holder at world (0, -200, 0)");
                for (int level = 1; level <= 2; level++)
                {
                    Transform r = world.BuildInterior(level);
                    Check(!CollectionPrototype.InteriorFraming(r, TheTrailWalker.Travel.StreetCamera.FieldOfView, 1116f / 2480f).Scrolls, "The level-" + level + " room fits the reference phone without scrolling");
                }
                Check(worldObject.transform.Cast<Transform>().Count(t => t.name == MovementPrototype.InteriorHolderName) == 1, "Building an interior again replaces the previous one");
                root = world.BuildInterior(1);

                var home = HomeState.New(); var w = new Wallet(100000000); int n = 0;
                home.Buy(F("furniture.l1.worktable"), w, "a");
                foreach (FurnitureDefinition d in FurnitureCatalog.UnlockedAt(1)) home.Buy(d, w, "p" + n++);
                home.Store("p0");
                Dictionary<string, GameObject> views = CollectionPrototype.InstancePieces(root, home);
                Check(views != null && views.ContainsKey("a") && views["a"].transform.parent == root && (views["a"].transform.localPosition - new Vector3(-1.5f, 0f, 0f)).sqrMagnitude < 1e-8f,
                    "A piece at (3, 0) is instanced at local (-1.5, 0, 0)");
                Check(views.Count == home.Pieces.Count(p => p.Placed) && !views.ContainsKey("p0") && home.Pieces.Where(p => p.Placed).All(p => views.ContainsKey(p.InstanceId)), "Every placed piece has a view and stored pieces have none");
                var probe = new GameObject("Street tap box layer probe");   // the street selection boxes sit on a fresh root's layer
                int touchLayer = probe.layer; UnityEngine.Object.DestroyImmediate(probe);
                bool oneBox = views.Values.All(v =>
                {
                    Collider[] all = v.GetComponentsInChildren<Collider>(true);
                    return all.Length == 1 && all[0] is BoxCollider && all[0].gameObject == v && v.layer == touchLayer && v.layer == CollectionPrototype.HomeTouchLayer;
                });
                Check(oneBox && root.GetComponentsInChildren<Collider>(true).Length == views.Count, "Furniture views have no colliders except one box per piece, on the street tap boxes' layer");
                Check(CollectionPrototype.PieceAt(views, views["a"].GetComponent<Collider>()) == "a" && CollectionPrototype.PieceAt(views, null) == null, "A touch box maps back to its instance id");

                world.ClearInterior();
                Check(worldObject.transform.Cast<Transform>().All(t => t.name != MovementPrototype.InteriorHolderName), "ClearInterior removes the interior");
            }
            finally { UnityEngine.Object.DestroyImmediate(worldObject); }
        }

        // Bird's-eye placement (task 7): the drop, store and save rules the view runs through, the floor-to-cell mapping, the
        // drag clamp and the camera sizing. No drag or selection state is ever saved, so these are domain-level checks.
        // Home revision 3 (spec 2026-11-14 sections 2-3): a plain entry strip in front of the door in every interior, empty
        // anchors for the later pet bed and collection box, ScrollMin moved forward over the strip, the grid untouched.
        // SHA-256 of the eight interior OBJs recorded before the entry strip was added: the strip is a separate part.
        private static readonly string[] InteriorObjSha256 =
        {
            "01b6d828081815257c60cabc9bff8a8d43adb9caff1910994cd67d8921d0da3f", "04133bddbca8010caea73872d46539274e369623ddfdb5544067600250fa0846",
            "c8d4f4e6163fc75667938801d0e7348c7d617f134d7fe8bd1697ceb8794b9116", "9f07a32395a5abe8bf7d6aadcc957efe911c6c217bdbda96a88a3e546f39da35",
            "668fb1c8a3f38a2fc3c2d00dcd1b7cdf5bb7954a7821568d5a41cebed401f916", "21026929d2bd6ac047e9fa27d2ad6fb898e8b56b3c362d7f708b0dbe08a6b14e",
            "8ae6a5648cefa48b6769934c1786400459be7423d3743a0ffb3acba13c748f6a", "1bbfcaf155791c1d91cabaad45d79c29574d922771a57cd99a578db08e038c30",
        };

        private static bool Near(Transform t, Vector3 expected) { return t != null && (t.localPosition - expected).magnitude <= 0.01f; }

        private static void EntryChecks()
        {
            for (int n = 1; n <= 8; n++)
            {
                string name = $"HouseL{n:00}Interior";
                var prefab = Resources.Load<GameObject>("Settlement/Houses/" + name);
                Check(prefab != null, name + " loads");
                Transform root = prefab.transform;
                var (gridWidth, gridDepth) = HomeState.Grid(n);
                float w = gridWidth * 0.5f, depth = gridDepth * 0.5f;
                Check(Near(FindDeep(root, "EntryArea"), new Vector3(-w / 2, 0, 1.2f)) && Near(FindDeep(root, "PetBedAnchor"), new Vector3(-w * 0.25f, 0, 1.2f))
                    && Near(FindDeep(root, "CollectionBoxAnchor"), new Vector3(-w * 0.75f, 0, 1.2f)), name + " has EntryArea, PetBedAnchor and CollectionBoxAnchor on the entry strip");
                Check(Near(FindDeep(root, "ScrollMin"), new Vector3(0, 0, 2.2f)) && Near(FindDeep(root, "ScrollMax"), new Vector3(0, 0, -depth)), name + " scrolls from z 2.2 (entry front) to z -" + depth + " (back wall)");
                Check(Near(FindDeep(root, "GridOrigin"), Vector3.zero) && Near(FindDeep(root, "GridXUnit"), new Vector3(-0.5f, 0, 0)) && Near(FindDeep(root, "GridDepthUnit"), new Vector3(0, 0, -0.5f)), name + " keeps its grid anchors");
                // Framing (user, November 14): measured at the reference phone (1116 x 2480) with the street camera's FOV.
                float aspect = 1116f / 2480f, fov = TheTrailWalker.Travel.StreetCamera.FieldOfView;
                Transform top = FindDeep(root, "BackWallTop");
                Check(top != null && Mathf.Abs(top.localPosition.z + depth + 0.19f) < 0.01f && Mathf.Abs(top.localPosition.x + w / 2) < 0.01f && top.localPosition.y > 2.2f && top.localPosition.y < 3.0f,
                    name + " has BackWallTop on the back wall's top (y " + (top != null ? top.localPosition.y.ToString("0.00") : "-") + ")");
                var frame = CollectionPrototype.InteriorFraming(root, fov, aspect);
                Transform eye = FindDeep(root, "InteriorCameraAnchor");
                Check(eye != null && (eye.localPosition - frame.Eye).magnitude < 0.01f && Quaternion.Angle(eye.localRotation, frame.Rotation) < 0.1f
                    && Mathf.Abs(Vector3.Angle(eye.localRotation * Vector3.forward, Vector3.down) - (90f - CollectionPrototype.InteriorPitch)) < 0.1f,
                    name + " InteriorCameraAnchor holds the runtime framing at the reference aspect (" + CollectionPrototype.InteriorPitch + " degrees down)");
                Vector2 Port(Vector3 p, float offset) => CollectionPrototype.InteriorViewport(frame, offset, p, fov, aspect);
                float stripFront = Port(new Vector3(-w / 2, 0, 2.2f), 0f).y;
                float stripSpan = Mathf.Abs(Port(new Vector3(-w - 0.16f, 0, 2.2f), 0f).x - Port(new Vector3(0.16f, 0, 2.2f), 0f).x);
                float roomSpan = Mathf.Abs(Port(new Vector3(-w - 0.16f, 0, 0f), 0f).x - Port(new Vector3(0.16f, 0, 0f), 0f).x);
                float topNear = Port(top.localPosition, 0f).y, topFar = Port(top.localPosition, frame.Lo).y;
                Check(roomSpan >= 0.92f && roomSpan <= 1.001f && stripSpan >= roomSpan,
                    name + " fills the screen width: room front " + (roomSpan * 100).ToString("0.0") + "%, strip front " + (stripSpan * 100).ToString("0.0") + "%");
                Check(stripFront >= 0.08f && stripFront <= 0.16f, name + " strip front edge at " + (stripFront * 100).ToString("0.0") + "% of the screen height (8-16%)");
                if (n <= 4) Check(!frame.Scrolls, name + " fits without scrolling (levels 1-4)");
                Check(frame.Hi == 0f && (frame.Scrolls ? topFar >= 0.88f && topFar <= 1f : topNear <= CollectionPrototype.InteriorBackTop + 0.001f),
                    name + (frame.Scrolls ? " scrolls " + (-frame.Lo).ToString("0.00") + " m; the back wall's top reaches " + (topFar * 100).ToString("0.0") + "% at the far end"
                                          : " fits: the back wall's top at " + (topNear * 100).ToString("0.0") + "% of the screen height, no scroll"));

                Transform entry = FindDeep(root, $"HouseL{n:00}Entry");
                Check(entry != null, name + " holds the HouseL" + n.ToString("00") + "Entry part");
                Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity; int triangles = 0;
                foreach (MeshFilter filter in entry.GetComponentsInChildren<MeshFilter>(true))
                {
                    Mesh mesh = filter.sharedMesh; if (mesh == null) continue;
                    for (int s = 0; s < mesh.subMeshCount; s++) triangles += (int)mesh.GetIndexCount(s) / 3;
                    Matrix4x4 toRoot = root.worldToLocalMatrix * filter.transform.localToWorldMatrix; Bounds b = mesh.bounds;
                    for (int c = 0; c < 8; c++)
                    {
                        Vector3 p = toRoot.MultiplyPoint3x4(new Vector3((c & 1) == 0 ? b.min.x : b.max.x, (c & 2) == 0 ? b.min.y : b.max.y, (c & 4) == 0 ? b.min.z : b.max.z));
                        min = Vector3.Min(min, p); max = Vector3.Max(max, p);
                    }
                }
                Check(triangles == 12 * (gridWidth + 6) && triangles < 1500, name + " entry part is plain (slab, " + gridWidth + " boards, nosing, edging; no mat): " + triangles + " triangles (under 1,500)");
                Check(min.z >= 0.15f && max.z <= 2.25f && min.x >= -w - 0.3f && max.x <= 0.3f && max.y <= 0.3f,
                    name + " entry part lies on z 0.15-2.25, inside the house width and under 0.3 m (" + min.ToString("0.00") + " - " + max.ToString("0.00") + ")");
                Check(entry.GetComponentsInChildren<Collider>(true).Length == 0 && entry.GetComponentsInChildren<Renderer>(true).All(r => r.sharedMaterial != null), name + " entry part has no collider and the house material");

                byte[] hash; using (var sha = System.Security.Cryptography.SHA256.Create()) hash = sha.ComputeHash(File.ReadAllBytes($"Assets/_Game/Art/Settlement/Houses/{name}.obj"));
                Check(string.Concat(hash.Select(x => x.ToString("x2"))) == InteriorObjSha256[n - 1], name + ".obj is byte-identical to before the entry strip");

            }
        }

        private static void PlacementViewChecks()
        {
            Check(CollectionPrototype.HomeBirdsEyeButtonNames.SequenceEqual(new[] { "Home: Store piece", "Home: Storage", "Home: Return" }), "The bird's-eye buttons are named Home: Store piece, Home: Storage and Home: Return");
            Check(CollectionPrototype.HomeBirdsEyeIcons.Length == 3 && CollectionPrototype.HomeBirdsEyeIcons[2] == "return", "Return uses the return icon");

            var table = F("furniture.l1.worktable");
            var home = HomeState.New(); var w = new Wallet(1000000);
            home.Buy(table, w, "a"); home.Buy(table, w, "b");   // a at (3, 0), b at (1, 0)
            Check(!CollectionPrototype.CommitPieceDrop(home, "b", 3, 0) && P(home, "b").X == 1 && P(home, "b").Z == 0, "A drop on an occupied cell snaps back to the starting cell");
            HomeState back = SessionSaveStore.RestoreHome(Snap(SessionSaveStore.SaveHome(home)));
            Check(P(back, "b").Placed && P(back, "b").X == 1 && P(back, "b").Z == 0, "A home saved after a drag that ended on an invalid cell keeps the starting cell");
            Check(!CollectionPrototype.CommitPieceDrop(home, "b", 7, 0) && !CollectionPrototype.CommitPieceDrop(home, null, 0, 0) && P(home, "b").X == 1, "A drop past the grid edge or without a piece changes nothing");
            Check(CollectionPrototype.CommitPieceDrop(home, "b", 6, 4) && P(SessionSaveStore.RestoreHome(Snap(SessionSaveStore.SaveHome(home))), "b").X == 6
                && P(SessionSaveStore.RestoreHome(Snap(SessionSaveStore.SaveHome(home))), "b").Z == 4, "A drop on a free cell moves the piece and the save carries the new cell");

            // A piece selected and dragged over (5, 2) but not released: the save holds the committed cell only.
            CollectionPrototype.DragCell(5, 2, 0, 0, table.Width, table.Depth, home.GridWidth, home.GridDepth, out int dx, out int dz);
            Check(dx == 5 && dz == 2 && P(home, "a").X == 3, "Dragging over a cell does not move the piece in the home");
            SavedHome selected = SessionSaveStore.SaveHome(home);
            HomeState mid = SessionSaveStore.RestoreHome(JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(Snap(selected))));
            Check(P(mid, "a").X == 3 && P(mid, "a").Z == 0 && P(mid, "a").Placed, "A snapshot taken while a piece is selected restores the piece at its committed cell");
            string[] savedFields = typeof(SavedPiece).GetFields().Select(f => f.Name).OrderBy(n => n).ToArray();
            string[] homeFields = typeof(SavedHome).GetFields().Select(f => f.Name).OrderBy(n => n).ToArray();
            Check(savedFields.SequenceEqual(new[] { "definition", "id", "placed", "rotation", "x", "z" }) && homeFields.SequenceEqual(new[] { "block", "firstPurchases", "level", "pieces", "upgradeEscrow" }), "The save has no dragging or selection state");

            Check(CollectionPrototype.StoreSelectedPiece(home, "a") == null && !P(home, "a").Placed, "Store piece on the selected piece moves it to storage and deselects");
            Check(!P(SessionSaveStore.RestoreHome(Snap(SessionSaveStore.SaveHome(home))), "a").Placed, "The stored piece stays stored after a save");
            Check(CollectionPrototype.StoreSelectedPiece(home, null) == null && P(home, "b").Placed, "Store piece with nothing selected changes nothing");

            // The finger's cell minus the grab offset, clamped so the footprint stays inside the room.
            CollectionPrototype.DragCell(9, -3, 1, 0, 2, 1, 8, 5, out int cx, out int cz);
            Check(cx == 6 && cz == 0, "A drag past the room edge keeps the footprint inside the grid");
            CollectionPrototype.DragCell(4, 3, 1, 0, 2, 1, 8, 5, out cx, out cz);
            Check(cx == 3 && cz == 3, "The dragged piece keeps the finger's grab offset");

            // Floor point (interior local space) to cell, with the art contract's anchors (X runs -x, rows run -z, 0.5 m cells).
            Vector3 o = Vector3.zero, ax = new Vector3(-0.5f, 0f, 0f), az = new Vector3(0f, 0f, -0.5f);
            CollectionPrototype.CellFromLocal(new Vector3(-1.6f, 0f, -0.3f), o, ax, az, out int fx, out int fz);
            Check(fx == 3 && fz == 0, "The floor at local (-1.6, 0, -0.3) is cell (3, 0)");
            CollectionPrototype.CellFromLocal(new Vector3(-3.9f, 0f, -2.4f), o, ax, az, out fx, out fz);
            Check(fx == 7 && fz == 4 && CollectionPrototype.CellInGrid(fx, fz, 8, 5), "The back-right floor corner is cell (7, 4) in 8x5");
            CollectionPrototype.CellFromLocal(new Vector3(0.2f, 0f, -0.3f), o, ax, az, out fx, out fz);
            Check(fx == -1 && !CollectionPrototype.CellInGrid(fx, fz, 8, 5), "The floor outside the room gives a cell outside the grid");
            Check(CollectionPrototype.CellInGrid(7, 4, 8, 5) && !CollectionPrototype.CellInGrid(8, 0, 8, 5) && !CollectionPrototype.CellInGrid(0, -1, 8, 5), "Cells inside 8x5 are 0-7 by 0-4");

            // The top-down camera fits the room width; deep rooms scroll by what does not fit.
            float size = CollectionPrototype.BirdsEyeOrthoSize(6f, 0.5f);
            Check(Mathf.Approximately(size * 0.5f, 3f + CollectionPrototype.BirdsEyeMargin), "The top-down camera is sized to the room width");
            CollectionPrototype.BirdsEyeScrollRange(8.5f, 3f, out float lo, out float hi);
            CollectionPrototype.BirdsEyeScrollRange(2.5f, 14f, out float lo1, out float hi1);
            Check(Mathf.Approximately(lo, -5.5f) && Mathf.Approximately(hi, 0f) && Mathf.Approximately(lo1, 0f) && Mathf.Approximately(hi1, 0f), "A deep room scrolls by its depth beyond the view; a room that fits does not scroll");
        }

        // Store and house storage sheets (task 8): the purpose lines, the rows, the buy and storage outcomes the sheets run
        // through (the sheet code acts on exactly these results), and the stub notes gone.
        private static void StoreChecks()
        {
            var purposes = new Dictionary<BuffCategory, string>
            {
                { BuffCategory.CraftQuality, "Helps craft finer goods" }, { BuffCategory.FoodRecovery, "Meals restore more" }, { BuffCategory.SalePrice, "Crafted goods sell for more" },
                { BuffCategory.BootsLife, "New boots last longer" }, { BuffCategory.ShirtLife, "New shirts last longer" }, { BuffCategory.HatLife, "New hats last longer" },
                { BuffCategory.PantsLife, "New trousers last longer" }, { BuffCategory.AxeLife, "New axes last longer" }, { BuffCategory.HuntingToolLife, "New hunting tools last longer" },
                { BuffCategory.RodLife, "New rods last longer" }, { BuffCategory.BackpackLife, "New backpacks last longer" }, { BuffCategory.Storage, "Holds items (opens later)" },
            };
            var categories = Enum.GetValues(typeof(BuffCategory)).Cast<BuffCategory>().ToList();
            Check(categories.All(c => CollectionPrototype.PurposeLine(c) == purposes[c]), "Every category has its verbatim purpose line");
            Check(categories.All(c => !string.IsNullOrEmpty(CollectionPrototype.PurposeLine(c)) && !CollectionPrototype.PurposeLine(c).Any(char.IsDigit) && !CollectionPrototype.PurposeLine(c).Contains("%")),
                "No purpose line contains a digit or a percent sign");

            var levelOne = CollectionPrototype.StoreRows(1).ToList();
            Check(levelOne.Select(d => d.Id).SequenceEqual(FurnitureCatalog.All.Where(d => d.HouseLevel == 1).Select(d => d.Id)), "The level-1 store lists exactly the six level-1 types");
            Check(levelOne.All(d => Resources.Load<Sprite>(CollectionPrototype.StoreIconPath(d)) != null), "Every level-1 store icon loads as a Sprite");
            Check(FurnitureCatalog.All.All(d => Resources.Load<Sprite>(CollectionPrototype.StoreIconPath(d)) != null), "Every furniture icon loads as a Sprite");
            Check(CollectionPrototype.StoreRows(2).Count() == 12 && CollectionPrototype.StoreRows(8).Count() == 48, "Level 2 lists twelve types and level 8 all 48");
            var table = F("furniture.l1.worktable");
            Check(CollectionPrototype.StorePriceLabel(table) == "3,000 cash" && CollectionPrototype.StoreBuyPrompt(table) == "Buy Worktable for 3,000 cash?", "A row shows \"3,000 cash\" and the confirmation reads \"Buy Worktable for 3,000 cash?\"");
            Check(CollectionPrototype.StoreNoSpaceNote == "No space; it's in house storage." && CollectionPrototype.StorageNoSpaceNote == "No space for this piece", "The no-space notes read verbatim");

            var home = HomeState.New(); var w = new Wallet(3000);
            Check(!CollectionPrototype.StoreRowPlaced(home, table), "A type not placed shows no check");
            string id = CollectionPrototype.NewFurnitureInstanceId();
            CollectionPrototype.HomeSheetResult bought = CollectionPrototype.StoreBuy(home, table, w, id);
            Check(bought.Bought && bought.Select == id && bought.Note == null && P(home, id).Placed && w.Cash == 0, "A buy confirmed with enough cash opens the bird's-eye view with the new piece selected");
            Check(CollectionPrototype.StoreRowPlaced(home, table), "A placed type shows the check");
            Check(CollectionPrototype.NewFurnitureInstanceId() != CollectionPrototype.NewFurnitureInstanceId() && !string.IsNullOrEmpty(id), "New instance ids are unique");
            CollectionPrototype.HomeSheetResult poor = CollectionPrototype.StoreBuy(home, table, w, CollectionPrototype.NewFurnitureInstanceId());
            Check(!poor.Bought && poor.Select == null && poor.Note == "Not enough cash" && home.Pieces.Count == 1 && w.Cash == 0, "Short of cash the store says Not enough cash and changes nothing");

            var full = HomeState.New(); var fw = new Wallet(1000000);
            for (int i = 0; i < 19; i++) full.Buy(table, fw, "t" + i);   // nineteen worktables (fifteen flat, four turned) fill the 8x5 room
            CollectionPrototype.HomeSheetResult stored = CollectionPrototype.StoreBuy(full, table, fw, "extra");
            Check(stored.Bought && stored.Select == null && stored.Note == "No space; it's in house storage." && !P(full, "extra").Placed && fw.Cash == 1000000 - 20 * 3000,
                "With no free space the note \"No space; it's in house storage.\" appears and the piece is stored");
            Check(CollectionPrototype.StoredPieces(full).Select(p => p.InstanceId).SequenceEqual(new[] { "extra" }), "The storage sheet lists the stored piece");
            CollectionPrototype.HomeSheetResult blocked = CollectionPrototype.StorageTap(full, "extra");
            Check(blocked.Select == null && blocked.Note == "No space for this piece" && !P(full, "extra").Placed, "A stored piece with no space says No space for this piece");
            full.Store("t0");
            CollectionPrototype.HomeSheetResult placed = CollectionPrototype.StorageTap(full, "extra");
            Check(placed.Select == "extra" && placed.Note == null && P(full, "extra").Placed, "Tapping a stored piece places it and selects it in the bird's-eye view");

            var stats = new TheTrailWalker.Domain.PlayerStatistics(); var sh = HomeState.New(); var sw = new Wallet(3500);
            CollectionPrototype.StoreBuy(sh, table, sw, "s1", stats);
            Check(stats.CashSpent == 3000, "A furniture buy adds exactly its price to the lifetime cash spent");
            CollectionPrototype.StoreBuy(sh, table, sw, "s2", stats);
            Check(stats.CashSpent == 3000 && sh.Pieces.Count == 1, "A refused furniture buy does not change the cash spent");

            var fed = new TheTrailWalker.Domain.Equipment.StaminaState(); fed.Restore(2.5, 3, 0, false, 0);
            fed.QueueFood(FurnitureBuffs.FoodHearts(4, bedroll4()));
            fed.Advance(60, 0, true, true);
            Check(Near(fed.Hearts, fed.Maximum) && fed.PendingFood <= 1e-9, "Buffed food recovery is capped at maximum stamina");

            Check(typeof(CollectionPrototype).GetField("HomeStoreSoonNote") == null && typeof(CollectionPrototype).GetField("HomeStorageSoonNote") == null, "The store and storage stub notes are gone");

            // Store menu (home revision, Task 5): the Furniture catalog groups every type by house level, all eight levels
            // listed, those above the house level locked; the catalog never offers a buy the domain refuses.
            var one = CollectionPrototype.CatalogGroups(1).ToList();
            Check(one.Count == 8 && one.Select(g => g.level).SequenceEqual(Enumerable.Range(1, 8)) && one.All(g => g.types.Count == 6 && g.types.All(t => t.HouseLevel == g.level)),
                "The catalog lists eight level groups, 1 to 8 in order, six types each");
            Check(!one[0].locked && one.Skip(1).All(g => g.locked), "At house level 1 group 1 is buyable and groups 2-8 are locked");
            var three = CollectionPrototype.CatalogGroups(3).ToList();
            Check(three.Take(3).All(g => !g.locked) && three.Skip(3).All(g => g.locked), "At house level 3 groups 1-3 are buyable and groups 4-8 are locked");
            Check(CollectionPrototype.CatalogGroups(8).All(g => !g.locked), "At house level 8 every group is buyable");
            Check(CollectionPrototype.CatalogHeader(2) == "House level 2" && CollectionPrototype.LockedLine(4) == "Unlocks at house level 4", "The catalog header reads \"House level 2\" and a locked row \"Unlocks at house level 4\"");
            bool lockedRefused = true;
            foreach (int level in new[] { 1, 3 })
            {
                var lh = HomeState.AtLevel(level); var rich = new Wallet(100000000);
                foreach (var g in CollectionPrototype.CatalogGroups(level))
                    foreach (FurnitureDefinition d in g.types)
                    {
                        bool can = lh.CanBuy(d, rich, out string why);
                        if (g.locked ? (can || why != CollectionPrototype.LockedLine(g.level)) : !can) lockedRefused = false;
                    }
            }
            Check(lockedRefused, "Every locked catalog row is refused by CanBuy with \"Unlocks at house level N\" and every unlocked row is buyable with enough cash");
            Check(Enum.GetNames(typeof(CollectionPrototype.StoreSection)).SequenceEqual(new[] { "Furniture", "Storage" }), "The store menu has the sections Furniture and Storage");
            Check(new[] { "storage", "store_piece" }.All(key => Resources.Load<Sprite>("UI/Icons/" + key) != null), "The storage and store_piece icons load as sprites from UI/Icons");
            Check(CollectionPrototype.HomeBirdsEyeIcons.SequenceEqual(new[] { "store_piece", "storage", "return" }), "The bird's-eye Store piece, Storage and Return buttons use the store_piece, storage and return icons");
        }

        private static SavedPiece PcR(string id, string def, bool placed, int x, int z, int rotation) { return new SavedPiece { id = id, definition = def, placed = placed, x = x, z = z, rotation = rotation }; }

        private static void RotationChecks()
        {
            var table = F("furniture.l1.worktable"); var peg = F("furniture.l1.hat_peg");
            Check(HomeState.Footprint(table, 0) == (2, 1) && HomeState.Footprint(table, 1) == (1, 2) && HomeState.Footprint(table, 2) == (2, 1), "Footprint swaps width and depth on odd rotations");

            var home = HomeState.New(); var w = new Wallet(100000000);
            var a = home.Buy(table, w, "a");
            Check(a.Placed && a.X == 3 && a.Z == 0 && a.Rotation == 0, "A bought worktable lands at (3, 0) with rotation 0");
            Check(home.Rotate("a") && a.Rotation == 1 && a.X == 3 && a.Z == 0, "Rotate turns the worktable to rotation 1 in place");
            Check(!home.CanPlace(peg, 3, 0) && !home.CanPlace(peg, 3, 1) && home.CanPlace(peg, 4, 0), "A rotated worktable occupies (3,0) and (3,1) but not (4,0)");
            Check(home.Move("a", 0, 0) && a.Rotation == 1, "Move keeps the rotation");
            Check(!home.Move("a", 0, 4) && a.Z == 0, "A rotated worktable cannot move to the back row");
            Check(home.Rotate("a") && a.Rotation == 2 && home.Rotate("a") && a.Rotation == 3 && home.Rotate("a") && a.Rotation == 0, "Four turns come back to rotation 0");

            var back = HomeState.New();
            back.Buy(table, w, "b"); back.Move("b", 0, 4);
            Check(!back.Rotate("b") && P(back, "b").Rotation == 0 && P(back, "b").X == 0 && P(back, "b").Z == 4, "Rotating a back-row worktable is refused and changes nothing");

            var pair = HomeState.Restore(1, new[] { ("p1", table.Id, true, 2, 0), ("p2", table.Id, true, 4, 0), ("p3", peg.Id, true, 2, 1) }, null);
            Check(!pair.Rotate("p1") && P(pair, "p1").Rotation == 0 && P(pair, "p1").X == 2 && P(pair, "p1").Z == 0, "Rotating into another piece is refused and changes nothing");

            var st = HomeState.New();
            st.Buy(table, w, "s"); st.Rotate("s"); st.Store("s");
            Check(!P(st, "s").Placed && P(st, "s").Rotation == 0, "Store resets the rotation");
            Check(!st.Rotate("s"), "Rotate on a stored piece is false");
            Check(st.PlaceFromStorage("s") && P(st, "s").Placed && P(st, "s").Rotation == 0, "PlaceFromStorage places at rotation 0");
            Check(Throws(() => st.Rotate("nope")), "Rotate on an unknown id is refused");

            // A room full of hat pegs except (0,0) and (0,1): only a turned worktable fits.
            var pegs = new List<(string, string, bool, int, int, int)>();
            for (int x = 0; x < 8; x++) for (int z = 0; z < 5; z++) if (!(x == 0 && z < 2)) pegs.Add(("g" + x + "_" + z, peg.Id, true, x, z, 0));
            var tight = HomeState.Restore(1, pegs, null);
            var bought = tight.Buy(table, w, "t");
            Check(bought.Placed && bought.Rotation == 1 && bought.X == 0 && bought.Z == 0, "A worktable Buy in a one-column gap places at rotation 1");

            // Save round trip and older saves.
            var rot = HomeState.New();
            rot.Buy(table, w, "r"); rot.Rotate("r");
            HomeState back2 = SessionSaveStore.RestoreHome(JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(Snap(SessionSaveStore.SaveHome(rot)))));
            Check(P(back2, "r").Rotation == 1 && P(back2, "r").X == 3 && P(back2, "r").Z == 0 && P(back2, "r").Placed, "A rotated piece round-trips through the save");
            string old = "{\"home\":{\"block\":1,\"level\":1,\"firstPurchases\":[],\"pieces\":[{\"id\":\"o\",\"definition\":\"furniture.l1.worktable\",\"placed\":true,\"x\":3,\"z\":0}]}}";
            Check(P(SessionSaveStore.RestoreHome(JsonUtility.FromJson<SessionSnapshot>(old)), "o").Rotation == 0, "A save without a rotation key loads at rotation 0");

            // Forged rotations.
            string wt = "furniture.l1.worktable";
            Check(Refused(Block(1, PcR("a", wt, true, 0, 0, 4))) && Refused(Block(1, PcR("a", wt, true, 0, 0, -1))), "Rotation 4 and -1 are refused");
            Check(Refused(Block(1, PcR("a", wt, false, 0, 0, 1))), "A stored piece with a rotation is refused");
            Check(Refused(Block(1, PcR("a", wt, true, 0, 0, 1), PcR("b", wt, true, 0, 1, 0))), "A rotated piece overlapping another is refused");
            Check(Refused(Block(1, PcR("a", wt, true, 0, 4, 1))), "A rotated piece leaving the grid is refused");
            Check(!Refused(Block(1, PcR("a", wt, true, 0, 3, 1))), "A rotated piece that fits is accepted");
            Check(Throws(() => HomeState.Restore(1, new[] { ("a", wt, true, 0, 4, 1) }, null)), "Restore refuses a rotated piece outside the grid");
        }

        // Home revision Task 6 (spec section 6): tap the selected piece in the bird's-eye view to turn it; the view pose.
        private static void RotationViewChecks()
        {
            var R = CollectionPrototype.BirdsEyeRelease.Rotate; var N = CollectionPrototype.BirdsEyeRelease.None; var D = CollectionPrototype.BirdsEyeRelease.Drop;
            Check(CollectionPrototype.ClassifyRelease(true, false, false, 0.1f) == R, "A short tap on the selected piece turns it");
            Check(CollectionPrototype.ClassifyRelease(true, false, false, 0.5f) == N, "Releasing a hold on the selected piece does not turn it");
            Check(CollectionPrototype.ClassifyRelease(true, false, true, 0.1f) == D, "Releasing a drag drops the piece and does not turn it");
            Check(CollectionPrototype.ClassifyRelease(true, true, false, 0.1f) == N, "The finger that entered the view does not turn the piece");
            Check(CollectionPrototype.ClassifyRelease(false, false, false, 0.1f) == N, "A tap off the selected piece does not turn anything");
            Check(CollectionPrototype.TurnPendingNote == "No room for it here. Drag it to a free spot.", "A turn that does not fit in place notes \"No room for it here. Drag it to a free spot.\"");

            Vector4 band = CollectionPrototype.HomeNoteBand;
            Check(band.y >= 0.85f && band.w <= 1f && band.y < band.w, "Inside the house the note sits in the top band, away from the room's gestures");
            Check(band.x >= 0.15f && band.z <= 0.85f && band.x < band.z, "The home note stays clear of the top corner squares");
            Check(CollectionPrototype.StreetNoteBand == new Vector4(0.04f, 0.40f, 1f, 0.55f), "On the street the note keeps its mid-screen place");

            // A turn that does not fit in place is held by the view and committed by a drop where it fits (user request,
            // November 14, 2026): the home only ever stores a fitting cell and turn together.
            var wt = F("furniture.l1.worktable"); var hp = F("furniture.l1.hat_peg");
            var turnHome = HomeState.Restore(1, new[] { ("b", wt.Id, true, 0, 4), ("q", hp.Id, true, 5, 0) }, null);
            Check(!turnHome.Rotate("b"), "Precondition: a back-row worktable cannot turn in place");
            Check(!CollectionPrototype.CommitPieceDrop(turnHome, "b", 0, 4, 1) && P(turnHome, "b").Rotation == 0 && P(turnHome, "b").Z == 4, "Dropping a turned piece where the turn does not fit changes nothing");
            Check(!CollectionPrototype.CommitPieceDrop(turnHome, "b", 5, 0, 1) && P(turnHome, "b").Rotation == 0 && P(turnHome, "b").X == 0, "Dropping a turned piece onto another piece changes nothing");
            Check(CollectionPrototype.CommitPieceDrop(turnHome, "b", 0, 2, 1) && P(turnHome, "b").Rotation == 1 && P(turnHome, "b").X == 0 && P(turnHome, "b").Z == 2, "Dropping a turned piece where it fits commits the cell and the turn together");
            Check(P(SessionSaveStore.RestoreHome(JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(Snap(SessionSaveStore.SaveHome(turnHome))))), "b").Rotation == 1, "The dropped turn round-trips through the save");
            Check(!CollectionPrototype.CommitPieceDrop(turnHome, "b", 0, 0, 4) && !CollectionPrototype.CommitPieceDrop(turnHome, "b", 0, 0, -1), "A drop with a rotation outside 0-3 is refused");
            turnHome.Store("q");
            Check(!CollectionPrototype.CommitPieceDrop(turnHome, "q", 0, 0, 1) && !P(turnHome, "q").Placed, "A drop never places a stored piece");

            // The pose: turn the 2x1 worktable's local footprint rectangle (art contract: x runs -x, depth runs -z, 0.5 m cells)
            // and map it back to grid cells relative to the recorded corner.
            var table = F("furniture.l1.worktable");
            Vector3 ax = new Vector3(-0.5f, 0f, 0f), az = new Vector3(0f, 0f, -0.5f);
            for (int r = 0; r < 4; r++)
            {
                CollectionPrototype.PieceLocalPose(table, r, out Vector3 corner, out Quaternion turn);
                float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
                foreach (var c in new[] { (0, 0), (table.Width, 0), (0, table.Depth), (table.Width, table.Depth) })
                {
                    Vector3 local = ax * corner.x + az * corner.z + turn * (ax * c.Item1 + az * c.Item2);
                    float gx = Vector3.Dot(local, ax) / ax.sqrMagnitude, gz = Vector3.Dot(local, az) / az.sqrMagnitude;
                    minX = Mathf.Min(minX, gx); maxX = Mathf.Max(maxX, gx); minZ = Mathf.Min(minZ, gz); maxZ = Mathf.Max(maxZ, gz);
                }
                var (fw, fd) = HomeState.Footprint(table, r);
                bool covers = Mathf.Abs(minX) < 1e-4f && Mathf.Abs(minZ) < 1e-4f && Mathf.Abs(maxX - fw) < 1e-4f && Mathf.Abs(maxZ - fd) < 1e-4f;
                if (r == 0) covers &= corner.sqrMagnitude < 1e-8f && Quaternion.Angle(turn, Quaternion.identity) < 0.01f;
                Check(covers, "At rotation " + r + " the worktable's turned model fills cells [X, X+" + fw + ") x [Z, Z+" + fd + ")");
            }
            {
                CollectionPrototype.PieceLocalPose(table, 1, out _, out Quaternion turn);
                Vector3 up = new Vector3(0f, 0f, -1f), right = new Vector3(-1f, 0f, 0f);   // screen up (room back) and right (grid +X) seen from above
                Check((turn * up - right).sqrMagnitude < 1e-6f, "Rotation 1 turns screen-up to screen-right: clockwise seen from the bird's-eye camera");
            }

            var worldObject = new GameObject("Home rotation view check");
            try
            {
                var world = worldObject.AddComponent<MovementPrototype>();
                Transform root = world.BuildInterior(1);
                var home = HomeState.Restore(1, new[] { ("a", table.Id, true, 3, 0, 1) }, null);
                Dictionary<string, GameObject> views = CollectionPrototype.InstancePieces(root, home);
                Physics.SyncTransforms();
                Bounds b = views["a"].GetComponent<Collider>().bounds;
                Transform o = FindDeep(root, "GridOrigin"), gx = FindDeep(root, "GridXUnit"), gz = FindDeep(root, "GridDepthUnit");
                Vector3 wo = o.position, wx = gx.position - wo, wz = gz.position - wo;
                Vector3 c0 = wo + wx * 3 + wz * 0, c1 = wo + wx * 4 + wz * 2;
                bool near = Mathf.Abs(b.min.x - Mathf.Min(c0.x, c1.x)) <= 0.05f && Mathf.Abs(b.max.x - Mathf.Max(c0.x, c1.x)) <= 0.05f
                    && Mathf.Abs(b.min.z - Mathf.Min(c0.z, c1.z)) <= 0.05f && Mathf.Abs(b.max.z - Mathf.Max(c0.z, c1.z)) <= 0.05f;
                Check(near, "A rotation-1 worktable at (3, 0) in the level-1 interior covers cells (3,0)-(3,1) (collider x " + b.min.x.ToString("0.00") + ".." + b.max.x.ToString("0.00") + ", z " + b.min.z.ToString("0.00") + ".." + b.max.z.ToString("0.00") + ")");
                world.ClearInterior();
            }
            finally { UnityEngine.Object.DestroyImmediate(worldObject); }
        }

        // Home revision section 7: look-only neighbour interiors.
        private static string Layout(HomeState h) { return string.Join(";", h.Pieces.Select(p => p.DefinitionId + "@" + p.X + "," + p.Z + "r" + p.Rotation + (p.Placed ? "" : "s"))); }

        private static void NeighbourChecks()
        {
            string a = Layout(NeighbourHome.Furnish(1234, 2, 5));
            Check(a.Length > 0 && a == Layout(NeighbourHome.Furnish(1234, 2, 5)), "The same seed, plot and level furnish the same neighbour room (definition, cell, rotation, in order)");
            Check(Layout(NeighbourHome.Furnish(1234, 2, 6)) != a, "A neighbour's level-up re-rolls the room");
            Check(Layout(NeighbourHome.Furnish(1234, 3, 5)) != a, "Another plot at the same level gets a different room");

            bool levels = true, fits = true, stored = false, unlocked = true, firsts = true, ids = true; int total = 0, turned = 0;
            for (int seed = 1; seed <= 50; seed++)
                for (int plot = 0; plot < TheTrailWalker.Domain.Travel.SettlementState.PlotCount; plot++)
                    for (int level = 1; level <= HomeState.MaxLevel; level++)
                    {
                        HomeState h = NeighbourHome.Furnish(seed, plot, level);
                        var grid = HomeState.Grid(level);
                        levels &= h.Level == level && h.GridWidth == grid.width && h.GridDepth == grid.depth;
                        firsts &= h.FirstPurchases.Count == 0;
                        ids &= h.Pieces.Select(p => p.InstanceId).Distinct().Count() == h.Pieces.Count;
                        foreach (FurniturePiece p in h.Pieces)
                        {
                            if (!p.Placed) { stored = true; continue; }
                            total++; if (p.Rotation != 0) turned++;
                            var fp = HomeState.Footprint(p.Definition, p.Rotation);
                            fits &= h.CanPlace(p.Definition, p.X, p.Z, p.InstanceId, p.Rotation) && p.X >= 0 && p.Z >= 0 && p.X + fp.width <= grid.width && p.Z + fp.depth <= grid.depth;
                            unlocked &= p.Definition.HouseLevel <= level;
                        }
                    }
            Check(levels, "A neighbour room has its level's grid");
            Check(fits, "No neighbour piece overlaps another or leaves the grid (seeds 1-50, plots 0-5, levels 1-8)");
            Check(!stored, "A neighbour room has no stored pieces");
            Check(unlocked, "A neighbour room holds only types unlocked at or below its level");
            Check(firsts, "A neighbour room writes no first-purchase records");
            Check(ids, "Neighbour piece instance ids are unique within a room");
            Check(total > 0 && turned > 0, "Neighbour rooms are furnished and use turned pieces (" + total + " pieces, " + turned + " turned)");

            var home = HomeState.AtLevel(3); var w = new Wallet(100000000);
            home.Buy(F("furniture.l1.worktable"), w, "w1"); home.Buy(F("furniture.l1.bedroll"), w, "b1");
            var street = TheTrailWalker.Domain.Travel.SettlementState.New(1234); street.ChoosePlot(4, new string[0]);
            Func<string> snapshot = () => JsonUtility.ToJson(new SessionSnapshot { home = SessionSaveStore.SaveHome(home), settlement = SessionSaveStore.SaveSettlement(street) });
            string before = snapshot(); long cash = w.Cash, gold = w.Gold; string pieces = Layout(home); int lvl = home.Level;
            foreach (TheTrailWalker.Domain.Travel.SettlementState.Neighbour n in street.Neighbours) NeighbourHome.Furnish(street.Seed, n.Plot, Math.Min(n.Level, HomeState.MaxLevel));
            Check(snapshot() == before && w.Cash == cash && w.Gold == gold && Layout(home) == pieces && home.Level == lvl, "Building neighbour rooms leaves the player's home, wallet and save unchanged");

            var neighbour = new TheTrailWalker.Travel.StreetBuilding { Id = "plot.2", Kind = TheTrailWalker.Travel.StreetBuildingKind.House, Plot = 2 };
            var mine = new TheTrailWalker.Travel.StreetBuilding { Id = "plot.4", Kind = TheTrailWalker.Travel.StreetBuildingKind.House, Plot = 4 };
            var bakery = new TheTrailWalker.Travel.StreetBuilding { Id = "shop.bakery", Kind = TheTrailWalker.Travel.StreetBuildingKind.Shop };
            Check(CollectionPrototype.SettlementTapAction(neighbour, true, 4, 3) == CollectionPrototype.StreetTapAction.ChoosePlot, "A neighbour house tap during the plot choice asks for the plot");
            Check(CollectionPrototype.SettlementTapAction(neighbour, false, 4, 3) == CollectionPrototype.StreetTapAction.OpenNeighbour, "A level-3 neighbour's house opens its room");
            Check(CollectionPrototype.SettlementTapAction(neighbour, false, 4, 8) == CollectionPrototype.StreetTapAction.OpenNeighbour, "A level-8 neighbour's house opens its room");
            Check(CollectionPrototype.SettlementTapAction(neighbour, false, 4, 9) == CollectionPrototype.StreetTapAction.Note, "A level-9 neighbour's house shows the paper note");
            Check(CollectionPrototype.SettlementTapAction(neighbour, false, 4, 0) == CollectionPrototype.StreetTapAction.Note, "A plot with no neighbour shows the paper note");
            Check(CollectionPrototype.SettlementTapAction(mine, false, 4, 0) == CollectionPrototype.StreetTapAction.OpenHome, "The player's own house still opens the home");
            Check(CollectionPrototype.SettlementTapAction(bakery, false, 4, 3) == CollectionPrototype.StreetTapAction.Note, "A shop tap shows its note");

            Func<string, int, bool, bool> shows = CollectionPrototype.HomeInsideButtonVisible;
            Check(shows("Home: Return", 5, true) && !shows("Home: Store", 5, true) && !shows("Home: Upgrade", 5, true) && !shows("Home: Pet", 5, true),
                "A neighbour's room shows only the Return button (no Store, Upgrade or Pet)");
            Check(shows("Home: Return", 1, false) && shows("Home: Store", 1, false) && shows("Home: Upgrade", 1, false) && !shows("Home: Pet", 2, false) && shows("Home: Pet", 3, false),
                "The player's home still shows Return, Store, Upgrade, and Pet from level 3");

            bool threw9 = false, threw0 = false;
            try { NeighbourHome.Furnish(1234, 2, 9); } catch (ArgumentOutOfRangeException) { threw9 = true; }
            try { NeighbourHome.Furnish(1234, 2, 0); } catch (ArgumentOutOfRangeException) { threw0 = true; }
            Check(threw9 && threw0, "Furnishing a neighbour room outside levels 1-8 throws ArgumentOutOfRangeException");
        }

        // Slice 2b: upgrade requirements, the level cap and room growth.
        private static void UpgradeDomainChecks()
        {
            var rows = HouseUpgrades.For(2);
            var ids = new[] { "item.pine_beam", "item.fired_bricks", "item.nettle_cloth", "item.reed_braid", "item.pine_panel" };
            var sources = new[] { "From: Lumberjack mission L4", "From: Lumberjack mission L7", "From: Explorer mission E15", "From: Gatherer mission G5", "From: Lumberjack mission L11" };
            Check(rows.Count == 5 && rows.Select(r => r.ItemId).SequenceEqual(ids), "The level 2 upgrade asks for five materials in order");
            Check(rows.All(r => r.Quantity == 25), "Each upgrade material is 25");
            Check(rows.Select(r => r.Source).SequenceEqual(sources), "Each upgrade material names its source mission");
            Check(HouseUpgrades.For(1).Count == 0 && HouseUpgrades.For(3).Count == 0 && HouseUpgrades.For(9).Count == 0, "Other levels have no upgrade requirements");

            Check(HomeState.PlayerMaxLevel == 2 && HomeState.New().CanUpgrade && !HomeState.AtLevel(2).CanUpgrade, "The player's house caps at level 2");

            var table = F("furniture.l1.worktable"); var peg = F("furniture.l1.hat_peg");
            var home = HomeState.New(); var w = new Wallet(1000000);
            Check(home.Buy(table, w, "a").X == 3, "Growth setup: worktable at (3, 0)");
            Check(home.TryAddPlaced(peg, "peg", 0, 4, 0) && home.TryAddPlaced(table, "b", 6, 2, 1), "Growth setup: hat peg and a turned worktable");
            home.Buy(table, w, "s"); home.Store("s");
            var l2 = FurnitureCatalog.All.First(d => d.HouseLevel == 2);
            string reason;
            Check(!home.CanBuy(l2, new Wallet(1000000), out reason) && reason == "Unlocks at house level 2", "A level 2 type is locked before the upgrade");
            home.AddUpgradeEscrow("x");
            Check(home.UpgradeEscrow.SequenceEqual(new[] { "x" }), "Escrow ids keep deposit order");
            home.AddUpgradeEscrow("y"); home.RemoveUpgradeEscrow("x");
            Check(home.UpgradeEscrow.SequenceEqual(new[] { "y" }), "An escrow id can be removed");
            home.CompleteUpgrade();
            Check(home.Level == 2 && home.GridWidth == 10 && home.GridDepth == 6, "The upgraded room is 10x6");
            Check(P(home, "a").X == 4 && P(home, "a").Z == 0 && P(home, "a").Rotation == 0
                && P(home, "peg").X == 1 && P(home, "peg").Z == 4 && P(home, "peg").Rotation == 0
                && P(home, "b").X == 7 && P(home, "b").Z == 2 && P(home, "b").Rotation == 1, "Placed pieces shift one column right");
            Check(!P(home, "s").Placed, "A stored piece stays stored");
            Check(home.CanPlace(peg, 0, 5) && home.CanPlace(peg, 9, 0), "The new back row and right column are free");
            Check(home.UpgradeEscrow.Count == 0, "Completing the upgrade clears the escrow");
            Check(home.CanBuy(l2, new Wallet(1000000), out reason), "A level 2 type is buyable after the upgrade");
            Check(Throws(() => home.CompleteUpgrade()) && home.Level == 2, "A second upgrade is refused");

            var full = HomeState.New(); var fw = new Wallet(1000000);
            for (int i = 0; i < 19; i++) full.Buy(table, fw, "t" + i);
            full.CompleteUpgrade();
            var saved = full.Pieces.Select(p => (p.InstanceId, p.DefinitionId, p.Placed, p.X, p.Z, p.Rotation)).ToList();
            bool restored = true;
            try { HomeState.Restore(2, saved, null); } catch (ArgumentException) { restored = false; }
            Check(restored && full.Pieces.Count(p => p.Placed) == 19, "A full level 1 room grows without overlap");

            var none = new (string, string, bool, int, int, int)[0];
            Check(Throws(() => HomeState.Restore(1, none, null, new[] { "a", "a" })) && Throws(() => HomeState.Restore(1, none, null, new[] { "" })), "A forged escrow list is refused");
            Check(HomeState.Restore(1, none, null, new[] { "a", "b" }).UpgradeEscrow.SequenceEqual(new[] { "a", "b" }), "A saved escrow list is restored");
        }

        // ---------------------------------------------------------------- slice 2b task 2: escrow, deposits, completion, save

        private const string Beam = "item.pine_beam";
        private static readonly string[] UpgradeMaterials = { "item.pine_beam", "item.fired_bricks", "item.nettle_cloth", "item.reed_braid", "item.pine_panel" };
        private static readonly ItemOwner[] BagOwners = { ItemOwner.Backpack, ItemOwner.Pocket, ItemOwner.Hook };
        private static int escrowCounter;
        private static string NewEscrowId() { return "house-escrow-" + (++escrowCounter); }
        private static ItemDefinition Def(string id) { return TheTrailWalker.Domain.Crafting.Items.Find(id); }
        private static SpatialInventory UpgradeLedger()
        {
            var l = new SpatialInventory(8000, 4000);
            if (!l.Reconfigure(8000, 4000, 2, 2, out _)) throw new InvalidOperationException("Test fixture: pockets");
            return l;
        }
        private static ItemEntry Put(SpatialInventory l, string id, string item, int quantity, int quality, ItemOwner owner, int x = 0, int y = 0, bool ready = true)
        {
            ItemEntry e = l.AddWorld(id, Def(item), quantity, quality, ItemPrices.Profile(Def(item), quality), ready);
            if (owner != ItemOwner.World && !l.RestoreStored(e, owner, x, y)) throw new InvalidOperationException("Test fixture: " + id);
            return e;
        }
        private static int Units(SpatialInventory l, string item, params ItemOwner[] owners) { return l.Entries.Where(e => e.Definition.Id == item && owners.Contains(e.Owner)).Sum(e => e.Quantity); }
        // Every unit of each material that still exists in the bag, the overflow or escrow.
        private static string Totals(SpatialInventory l)
        {
            return string.Join(";", l.Entries.Where(e => e.Owner != ItemOwner.Consumed && e.Owner != ItemOwner.World).GroupBy(e => e.Definition.Id).OrderBy(g => g.Key)
                .Select(g => g.Key + "=" + g.Sum(e => e.Quantity)));
        }
        private static string Piles(HomeState h, SpatialInventory l)
        {
            return string.Join("|", h.UpgradeEscrow.Select(id => l.Entries.First(e => e.Id == id)).Select(e => e.Id + ":" + e.Owner + ":" + e.Definition.Id + ":"
                + string.Join(",", e.Pile.Select(p => p.Quality + "x" + p.Quantity + "@" + p.Profile.UnitValue + "/" + p.Profile.MaxDurability + "/" + p.Profile.Durability + "/" + p.Profile.Variant))));
        }
        private static string PileOf(ItemEntry e) { return string.Join(",", e.Pile.Select(p => p.Quality + "x" + p.Quantity + "@" + p.Profile.UnitValue)); }
        private static SessionSnapshot UpgradeSnapshot(HomeState h, SpatialInventory l, TradeSessionData trade = null)
        {
            var s = new SessionSnapshot { version = 2, speedCap = 1.5f, survival = new SavedSurvival(), economy = 1, home = SessionSaveStore.SaveHome(h), trade = trade };
            foreach (ItemEntry e in l.Entries)
            {
                if (e.Owner == ItemOwner.Consumed) continue;
                s.items.Add(new SavedItem { id = e.Id, definitionId = e.Definition.Id, owner = e.Owner.ToString(), quantity = e.Quantity, quality = e.Quality, ready = e.Ready, x = e.X, y = e.Y,
                    unitValue = e.Profile.UnitValue, maxDurability = e.Profile.MaxDurability, durability = e.Profile.Durability, variant = e.Profile.Variant,
                    pile = e.Pile.Select(p => new SavedPortion { quality = p.Quality, quantity = p.Quantity, unitValue = p.Profile.UnitValue, maxDurability = p.Profile.MaxDurability, durability = p.Profile.Durability, variant = p.Profile.Variant }).ToList() });
            }
            return s;
        }
        private static bool ValidateRefused(SessionSnapshot s)
        {
            try { SessionSaveStore.Validate(s, float.MaxValue, out _); return false; } catch (InvalidDataException) { return true; }
        }
        private static bool StructureRefused(IReadOnlyList<string> ids, IEnumerable<ItemEntry> entries, IEnumerable<string> trade)
        {
            try { HouseUpgradeService.CheckStructure(ids, entries, trade); return false; } catch (ArgumentException) { return true; }
        }

        private static void UpgradeEscrowChecks()
        {
            string reason;

            // Deposit: only from the bag, up to the remainder, lowest quality first.
            var home = HomeState.New(); var ledger = UpgradeLedger();
            Put(ledger, "b3", Beam, 10, 3, ItemOwner.Backpack, 0, 0);
            Put(ledger, "b1", Beam, 8, 1, ItemOwner.Backpack, 2000, 0);
            Put(ledger, "p2", Beam, 4, 2, ItemOwner.Pocket, 0, 0);
            Put(ledger, "o5", Beam, 50, 5, ItemOwner.Overflow);
            Check(HouseUpgradeService.Available(ledger, Beam) == 22 && HouseUpgradeService.Remaining(home, ledger, Beam) == 25 && HouseUpgradeService.Deposited(home, ledger, Beam) == 0, "Available counts only the bag; nothing is deposited yet");
            string before = Totals(ledger);
            int moved = HouseUpgradeService.Deposit(home, ledger, Beam, NewEscrowId, out reason);
            Check(moved == 22 && HouseUpgradeService.Deposited(home, ledger, Beam) == 22, "A deposit moves every bag unit up to the remainder");
            Check(Units(ledger, Beam, ItemOwner.Overflow) == 50 && Units(ledger, Beam, BagOwners) == 0, "A deposit never takes from the overflow and empties the bag");
            var escrowQualities = home.UpgradeEscrow.Select(id => ledger.Entries.First(e => e.Id == id)).SelectMany(e => e.Pile).GroupBy(p => p.Quality).OrderBy(g => g.Key).Select(g => g.Key + "x" + g.Sum(p => p.Quantity));
            Check(string.Join(",", escrowQualities) == "1x8,2x4,3x10", "The escrow piles hold Q1 x8, Q2 x4 and Q3 x10");
            Check(home.UpgradeEscrow.Count == 3 && ledger.Entries.First(e => e.Id == home.UpgradeEscrow[0]).Quality == 1 && ledger.Entries.First(e => e.Id == home.UpgradeEscrow[2]).Quality == 3, "Stacks are deposited lowest quality first");
            Check(home.UpgradeEscrow.All(id => ledger.Entries.First(e => e.Id == id).Owner == ItemOwner.Escrow), "Every house escrow id names an Escrow entry");
            Check(Totals(ledger) == before, "Bag, overflow and escrow totals are conserved by a deposit");
            Put(ledger, "b4", Beam, 10, 4, ItemOwner.Backpack, 4000, 0);
            Check(HouseUpgradeService.Remaining(home, ledger, Beam) == 3, "The remainder after 22 is 3");
            before = Totals(ledger);
            moved = HouseUpgradeService.Deposit(home, ledger, Beam, NewEscrowId, out reason);
            Check(moved == 3 && Units(ledger, Beam, BagOwners) == 7 && HouseUpgradeService.Deposited(home, ledger, Beam) == 25 && HouseUpgradeService.Remaining(home, ledger, Beam) == 0, "A second deposit takes only the remaining 3 and leaves 7 in the bag");
            Check(Totals(ledger) == before, "Totals are conserved by the second deposit");
            Check(HouseUpgradeService.Deposit(home, ledger, Beam, NewEscrowId, out reason) == 0 && reason == "Already complete." && Totals(ledger) == before, "A complete material refuses more");

            // Refusals.
            Put(ledger, "log", "material.pine_log", 5, 1, ItemOwner.Backpack, 6000, 0);
            Check(HouseUpgradeService.Deposit(home, ledger, "material.pine_log", NewEscrowId, out reason) == 0 && reason == "The house doesn't need this." && Units(ledger, "material.pine_log", BagOwners) == 5, "An unrequired item is refused");
            Check(HouseUpgradeService.Remaining(home, ledger, "material.pine_log") == 0, "An unrequired item has no remainder");
            var capped = HomeState.AtLevel(2); var capLedger = UpgradeLedger();
            Put(capLedger, "cb", Beam, 10, 1, ItemOwner.Backpack);
            Check(HouseUpgradeService.Deposit(capped, capLedger, Beam, NewEscrowId, out reason) == 0 && reason == "The house is at its highest level for now." && capped.UpgradeEscrow.Count == 0 && Units(capLedger, Beam, BagOwners) == 10, "A house at the cap refuses deposits");
            var emptyHome = HomeState.New(); var emptyLedger = UpgradeLedger();
            Put(emptyLedger, "eo", Beam, 30, 1, ItemOwner.Overflow);
            Check(HouseUpgradeService.Deposit(emptyHome, emptyLedger, Beam, NewEscrowId, out reason) == 0 && reason == "None in your bag." && emptyHome.UpgradeEscrow.Count == 0 && Units(emptyLedger, Beam, ItemOwner.Overflow) == 30, "An empty bag refuses (the overflow does not count)");
            var heldHome = HomeState.New(); var heldLedger = UpgradeLedger();
            ItemEntry held = Put(heldLedger, "held", "item.fired_bricks", 5, 1, ItemOwner.Backpack, 0, 0);
            ItemEntry pending = Put(heldLedger, "pending", "item.fired_bricks", 5, 1, ItemOwner.Backpack, 2000, 0, false);
            ItemEntry third = Put(heldLedger, "third", "item.fired_bricks", 6, 3, ItemOwner.Backpack, 4000, 0);
            Check(heldLedger.Reserve(held, 77), "Refusal setup: a held stack");
            Check(HouseUpgradeService.Available(heldLedger, "item.fired_bricks") == 6, "Held and pending stacks are not available");
            before = Totals(heldLedger);
            moved = HouseUpgradeService.Deposit(heldHome, heldLedger, "item.fired_bricks", NewEscrowId, out reason);
            Check(moved == 6 && held.Quantity == 5 && held.Owner == ItemOwner.Backpack && pending.Quantity == 5 && pending.Owner == ItemOwner.Backpack && third.Owner == ItemOwner.Consumed
                && HouseUpgradeService.Deposited(heldHome, heldLedger, "item.fired_bricks") == 6 && Totals(heldLedger) == before, "A held and a pending stack are skipped while a third deposits");
            heldLedger.Release(held, 77);

            // Round trip after each of three partial deposits.
            var tripHome = HomeState.New(); var tripLedger = UpgradeLedger();
            bool trips = true; int step = 0;
            foreach (int quality in new[] { 2, 4, 1 })
            {
                step++;
                Put(tripLedger, "trip" + step, UpgradeMaterials[step], 5, quality, ItemOwner.Backpack, 0, 0);
                Put(tripLedger, "tripbeam" + step, Beam, 4, quality, ItemOwner.Backpack, 2000, 0);
                HouseUpgradeService.Deposit(tripHome, tripLedger, UpgradeMaterials[step], NewEscrowId, out reason);
                HouseUpgradeService.Deposit(tripHome, tripLedger, Beam, NewEscrowId, out reason);
                string json = JsonUtility.ToJson(UpgradeSnapshot(tripHome, tripLedger));
                SessionSnapshot back = JsonUtility.FromJson<SessionSnapshot>(json);
                SessionSaveStore.Validate(back, float.MaxValue, out SpatialInventory restored);
                HomeState restoredHome = SessionSaveStore.RestoreHome(back);
                trips &= restoredHome.UpgradeEscrow.SequenceEqual(tripHome.UpgradeEscrow) && Piles(restoredHome, restored) == Piles(tripHome, tripLedger)
                    && UpgradeMaterials.All(m => HouseUpgradeService.Deposited(restoredHome, restored, m) == HouseUpgradeService.Deposited(tripHome, tripLedger, m));
            }
            Check(trips && tripHome.UpgradeEscrow.Count == 6, "Partial deposits round-trip through JSON with the same counts, piles and profiles after every deposit");
            string emptyJson = JsonUtility.ToJson(UpgradeSnapshot(HomeState.New(), UpgradeLedger()));
            SessionSnapshot noEscrow = JsonUtility.FromJson<SessionSnapshot>(emptyJson.Replace(",\"upgradeEscrow\":[]", ""));
            Check(emptyJson.Contains(",\"upgradeEscrow\":[]") && noEscrow.home.block == 1 && SessionSaveStore.RestoreHome(noEscrow).UpgradeEscrow.Count == 0, "A home block without upgradeEscrow has no deposits");

            // Structure: every house escrow id names an Escrow entry no trade listing names.
            var structLedger = UpgradeLedger();
            Put(structLedger, "bag", Beam, 5, 1, ItemOwner.Backpack);
            Put(structLedger, "house", Beam, 5, 1, ItemOwner.Escrow);
            Put(structLedger, "trade", Beam, 5, 1, ItemOwner.Escrow);
            var noTrade = new string[0];
            Check(!StructureRefused(new[] { "house" }, structLedger.Entries, new[] { "trade" }), "A valid house escrow passes the structure check");
            Check(StructureRefused(new[] { "nope" }, structLedger.Entries, noTrade), "An unknown house escrow id is refused");
            Check(StructureRefused(new[] { "bag" }, structLedger.Entries, noTrade), "A backpack-owned house escrow id is refused");
            Check(StructureRefused(new[] { "trade" }, structLedger.Entries, new[] { "trade" }), "A house escrow id shared with a trade listing is refused");
            Check(StructureRefused(new[] { "house", "house" }, structLedger.Entries, noTrade), "A duplicated house escrow id is refused");
            var structHome = HomeState.Restore(1, new (string, string, bool, int, int, int)[0], null, new[] { "house" });
            var listing = new TradeSessionData(); listing.listings.Add(new TradeListing { id = "l1", owner = ListingOwner.Player, itemId = Beam, quantity = 5, quality = 1, escrowId = "trade" });
            Check(!ValidateRefused(UpgradeSnapshot(structHome, structLedger, listing)), "A save with house and trade escrow is accepted");
            var forgedBag = UpgradeSnapshot(structHome, structLedger, listing); forgedBag.home.upgradeEscrow = new List<string> { "bag" };
            var forgedMissing = UpgradeSnapshot(structHome, structLedger, listing); forgedMissing.home.upgradeEscrow = new List<string> { "missing" };
            var forgedShared = UpgradeSnapshot(structHome, structLedger, listing); forgedShared.home.upgradeEscrow = new List<string> { "trade" };
            var forgedTwice = UpgradeSnapshot(structHome, structLedger, listing); forgedTwice.home.upgradeEscrow = new List<string> { "house", "house" };
            Check(ValidateRefused(forgedBag) && ValidateRefused(forgedMissing) && ValidateRefused(forgedShared) && ValidateRefused(forgedTwice), "Validate refuses a forged house escrow (not Escrow, missing, shared with trade, duplicated)");
            string message = null;
            try { SessionSaveStore.Validate(forgedBag, float.MaxValue, out _); } catch (InvalidDataException ex) { message = ex.Message; }
            Check(message != null && message.StartsWith("Invalid house deposits: "), "The refusal names the house deposits");

            // Migration: unrequired, surplus and capped deposits go to the overflow, whole entries, newest first.
            var migHome = HomeState.New(); var migLedger = UpgradeLedger();
            ItemEntry stray = Put(migLedger, "stray", "material.pine_log", 7, 2, ItemOwner.Escrow);
            ItemEntry first20 = Put(migLedger, "first20", Beam, 20, 1, ItemOwner.Escrow);
            ItemEntry newer10 = Put(migLedger, "newer10", Beam, 10, 4, ItemOwner.Escrow);
            migHome.AddUpgradeEscrow("stray"); migHome.AddUpgradeEscrow("first20"); migHome.AddUpgradeEscrow("newer10");
            string strayPile = PileOf(stray), newerPile = PileOf(newer10), migBefore = Totals(migLedger);
            moved = HouseUpgradeService.MigrateSurplus(migHome, migLedger);
            Check(moved == 17 && stray.Owner == ItemOwner.Overflow && newer10.Owner == ItemOwner.Overflow && first20.Owner == ItemOwner.Escrow && migHome.UpgradeEscrow.SequenceEqual(new[] { "first20" }),
                "Migration moves an unrequired entry and the newest surplus entry to the overflow; 20 stay deposited");
            Check(PileOf(stray) == strayPile && PileOf(newer10) == newerPile && stray.Quantity == 7 && newer10.Quantity == 10 && Totals(migLedger) == migBefore, "Migrated units keep their quantities and qualities");
            Check(HouseUpgradeService.MigrateSurplus(migHome, migLedger) == 0 && HouseUpgradeService.Deposited(migHome, migLedger, Beam) == 20, "A second migration moves nothing");
            var capHome = HomeState.Restore(2, new (string, string, bool, int, int, int)[0], null, new[] { "left" }); var capMig = UpgradeLedger();
            ItemEntry left = Put(capMig, "left", "item.reed_braid", 9, 3, ItemOwner.Escrow);
            string leftPile = PileOf(left);
            Check(HouseUpgradeService.MigrateSurplus(capHome, capMig) == 9 && left.Owner == ItemOwner.Overflow && left.Quantity == 9 && PileOf(left) == leftPile && capHome.UpgradeEscrow.Count == 0, "A level 2 home's leftover deposit moves to the overflow");
            var fineHome = HomeState.New(); var fineLedger = UpgradeLedger();
            Put(fineLedger, "fb", Beam, 12, 1, ItemOwner.Backpack); HouseUpgradeService.Deposit(fineHome, fineLedger, Beam, NewEscrowId, out reason);
            Check(HouseUpgradeService.MigrateSurplus(fineHome, fineLedger) == 0 && HouseUpgradeService.Deposited(fineHome, fineLedger, Beam) == 12, "Migration leaves valid deposits alone");

            // Complete: only when every material is exact; consumes exactly the house escrow.
            var doneHome = HomeState.New(); var doneLedger = UpgradeLedger();
            ItemEntry keep = Put(doneLedger, "keep", "material.stone", 7, 2, ItemOwner.Backpack, 6000, 2000);
            ItemEntry logs = Put(doneLedger, "logs", "material.pine_log", 6, 1, ItemOwner.Backpack, 6000, 0);
            Check(doneLedger.TryEscrow(logs, 3, "trade-escrow", out ItemEntry tradeEscrow, out _), "Complete setup: a trade listing's escrow");
            foreach (string m in UpgradeMaterials)
            {
                Put(doneLedger, "src-" + m, m, m == Beam ? 24 : 25, 2, ItemOwner.Backpack, 0, 0);
                HouseUpgradeService.Deposit(doneHome, doneLedger, m, NewEscrowId, out reason);
            }
            string doneBefore = Piles(doneHome, doneLedger);
            Check(!HouseUpgradeService.Ready(doneHome, doneLedger) && !HouseUpgradeService.Complete(doneHome, doneLedger) && doneHome.Level == 1 && Piles(doneHome, doneLedger) == doneBefore, "Completion is refused while one material is 24/25 and nothing changes");
            Put(doneLedger, "last", Beam, 1, 3, ItemOwner.Backpack, 0, 0);
            HouseUpgradeService.Deposit(doneHome, doneLedger, Beam, NewEscrowId, out reason);
            var houseEntries = doneHome.UpgradeEscrow.Select(id => doneLedger.Entries.First(e => e.Id == id)).ToList();
            Check(HouseUpgradeService.Ready(doneHome, doneLedger) && UpgradeMaterials.All(m => houseEntries.Where(e => e.Definition.Id == m).Sum(e => e.Quantity) == 25), "All five materials are at 25");
            Check(HouseUpgradeService.Complete(doneHome, doneLedger) && doneHome.Level == 2 && doneHome.GridWidth == 10 && doneHome.UpgradeEscrow.Count == 0, "Completion raises the house to level 2");
            Check(houseEntries.All(e => e.Owner == ItemOwner.Consumed), "Completion consumes every house escrow entry");
            Check(tradeEscrow.Owner == ItemOwner.Escrow && tradeEscrow.Quantity == 3 && keep.Owner == ItemOwner.Backpack && keep.Quantity == 7 && logs.Quantity == 3, "Completion leaves trade escrow and the bag untouched");
            Check(!HouseUpgradeService.Complete(doneHome, doneLedger) && doneHome.Level == 2, "A second completion is refused");
            Check(doneLedger.SettleEscrow(tradeEscrow) && tradeEscrow.Owner == ItemOwner.Consumed, "Trade escrow still settles after a house upgrade");
            var forgedDone = HomeState.New(); var forgedLedger = UpgradeLedger();
            foreach (string m in UpgradeMaterials) { Put(forgedLedger, "f-" + m, m, 25, 1, ItemOwner.Escrow); forgedDone.AddUpgradeEscrow("f-" + m); }
            Put(forgedLedger, "f-extra", "material.pine_log", 3, 1, ItemOwner.Escrow); forgedDone.AddUpgradeEscrow("f-extra");
            Check(!HouseUpgradeService.Complete(forgedDone, forgedLedger) && forgedDone.Level == 1 && forgedLedger.Entries.All(e => e.Owner == ItemOwner.Escrow), "Completion is refused when the house lists an unrequired entry");

            // Faint: house escrow is never carried, so a faint cannot take it.
            var faintHome = HomeState.New(); var faintLedger = UpgradeLedger();
            Put(faintLedger, "fa", Beam, 10, 1, ItemOwner.Backpack, 0, 0);
            HouseUpgradeService.Deposit(faintHome, faintLedger, Beam, NewEscrowId, out reason);
            Put(faintLedger, "fx", "material.stone", 2, 1, ItemOwner.Backpack, 0, 0);
            Put(faintLedger, "fy", "material.stone", 2, 1, ItemOwner.Backpack, 2000, 0);
            string faintPiles = Piles(faintHome, faintLedger);
            var plan = faintLedger.PlanFaintLoss("house-faint", max => 0);
            Check(faintLedger.CommitFaintLoss(plan) && plan.Entries.Count == 2 && Piles(faintHome, faintLedger) == faintPiles && HouseUpgradeService.Deposited(faintHome, faintLedger, Beam) == 10, "A faint never takes house deposits");

            // Debug grant: only what is still missing, at Q5, never touching deposits or the level.
            var grantHome = HomeState.New(); var grantLedger = UpgradeLedger();
            Put(grantLedger, "g", Beam, 12, 1, ItemOwner.Backpack, 0, 0);
            HouseUpgradeService.Deposit(grantHome, grantLedger, Beam, NewEscrowId, out reason);
            string grantPiles = Piles(grantHome, grantLedger);
            var granted = HouseUpgradeService.DebugGrant(grantHome, grantLedger);
            Check(granted.Select(g => g.itemId + "x" + g.quantity).SequenceEqual(new[] { "item.pine_beamx13", "item.fired_bricksx25", "item.nettle_clothx25", "item.reed_braidx25", "item.pine_panelx25" }), "The debug grant names 13 Pine beams and 25 of each other material");
            Check(UpgradeMaterials.All(m => grantLedger.Entries.Where(e => e.Definition.Id == m && (BagOwners.Contains(e.Owner) || e.Owner == ItemOwner.Overflow)).SelectMany(e => e.Pile).Where(p => p.Quality == 5).Sum(p => p.Quantity) == (m == Beam ? 13 : 25)), "The debug grant adds those units at Q5 to the bag or overflow");
            Check(Piles(grantHome, grantLedger) == grantPiles && grantHome.Level == 1 && HouseUpgradeService.Deposited(grantHome, grantLedger, Beam) == 12, "The debug grant never touches deposits or the level");
            var grantCap = HomeState.AtLevel(2); var grantCapLedger = UpgradeLedger();
            Check(HouseUpgradeService.DebugGrant(grantCap, grantCapLedger).Count == 0 && grantCapLedger.Entries.Count == 0, "The debug grant adds nothing at the cap");
        }
    }
}
