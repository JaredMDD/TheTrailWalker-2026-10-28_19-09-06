using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Missions;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEngine;
using Text = UnityEngine.UI.Text;

namespace TheTrailWalker.Editor
{
    // Milestone 14: settlement shops. Each task adds a group of checks called from Run().
    public static class Milestone14Validation
    {
        private static readonly List<string> passed = new List<string>();

        public static void Run()
        {
            passed.Clear();
            CatalogChecks();
            StockChecks();
            PurchaseChecks();
            SaveChecks();
            ModelChecks();
            ViewChecks();
            StorefrontChecks();
            CounterLampChecks();
            BakeryDataChecks();
            BakeryStockChecks();
            WalletIconChecks();
            WalletLabelChecks();
            CounterLayoutChecks();
            CounterChromeChecks();
            SettlementListIconChecks();
            CampListChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Shop-checks.txt", passed);
            Debug.Log($"Milestone 14: {passed.Count} checks passed.");
        }

        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Milestone 14: " + name); passed.Add(name); }

        // Price/unit column of shop listings spec 5.1-5.5, whole cash.
        private static readonly Dictionary<string, long> SpecPrices = new Dictionary<string, long> {
            { "food.apples", 16 }, { "ingredient.rock_salt", 24 }, { "food.strawberries", 16 }, { "ingredient.mushrooms", 24 }, { "ingredient.tea_herbs", 24 }, { "ingredient.oats", 16 }, { "food.berries", 16 }, { "ingredient.wheat", 16 },
            { "food.blackberries", 40 }, { "ingredient.carrots", 40 }, { "ingredient.barley", 40 }, { "food.blueberries", 40 }, { "ingredient.flax_seeds", 40 }, { "ingredient.tomatoes", 40 },
            { "ingredient.rice", 48 }, { "ingredient.honey", 64 }, { "ingredient.lavender", 48 },
            { "ingredient.corn", 80 }, { "ingredient.beans", 80 }, { "ingredient.chickpeas", 80 }, { "ingredient.soybeans", 80 }, { "ingredient.sugar_cane", 80 }, { "ingredient.sunflower_seeds", 80 },
            { "ingredient.pumpkins", 96 }, { "ingredient.green_chili", 96 }, { "ingredient.wild_mint", 96 }, { "ingredient.potatoes", 144 }, { "ingredient.seaweed", 144 },
            { "food.coconuts", 160 }, { "ingredient.cacao_beans", 176 }, { "ingredient.coffee_beans", 176 }, { "ingredient.vanilla_pods", 200 },
            { "material.clay", 8 }, { "material.pebbles", 8 }, { "material.sand", 8 }, { "material.flint", 16 }, { "material.limestone", 16 },
            { "material.sandstone", 24 }, { "material.yellow_ochre", 24 }, { "material.copper_ore", 32 }, { "material.quartz", 32 }, { "material.tin_ore", 32 },
            { "material.granite", 48 }, { "material.slate", 48 }, { "material.iron_ore", 56 }, { "material.silver_ore", 72 }, { "material.turquoise", 80 },
            { "material.marble", 80 }, { "material.basalt", 80 }, { "material.obsidian", 96 }, { "material.gold_ore", 112 }, { "material.lapis_lazuli", 120 },
            { "material.emerald", 160 }, { "material.ruby", 160 }, { "material.sapphire", 160 }, { "material.diamond", 200 },
            { "material.hemp", 8 }, { "material.nettle_fiber", 8 }, { "material.reeds", 8 }, { "material.flax_fiber", 24 }, { "material.cattail_fiber", 24 },
            { "material.cotton_fiber", 48 }, { "material.agave_fiber", 48 }, { "material.bamboo_fiber", 80 },
            { "item.hemp_cord", 12 }, { "item.hemp_thread", 8 }, { "item.nettle_thread", 8 }, { "item.hemp_cloth", 16 }, { "item.nettle_cloth", 16 }, { "item.fine_hemp_cloth", 56 },
            { "item.flax_cord", 36 }, { "item.flax_stuffing", 36 }, { "item.flax_cloth", 48 }, { "item.cattail_cord", 72 }, { "item.fine_flax_cloth", 152 },
            { "item.cotton_cord", 72 }, { "item.cotton_thread", 48 }, { "item.cotton_stuffing", 72 }, { "item.cotton_cloth", 96 }, { "item.agave_cord", 144 }, { "item.fine_cotton_cloth", 352 },
            { "item.bamboo_cloth", 160 },
            { "ingredient.egg", 32 }, { "food.milk.cow", 48 }, { "food.milk.goat", 64 },
            { "animal.chicken.meat", 50 }, { "animal.duck.meat", 100 }, { "animal.lamb.meat", 180 }, { "animal.beef.meat", 300 },
            { "fish.bluegill", 64 }, { "fish.rainbow_trout", 80 }, { "fish.yellow_perch", 80 }, { "fish.largemouth_bass", 96 },
            { "fish.common_carp", 200 }, { "fish.channel_catfish", 224 }, { "fish.northern_pike", 240 }, { "fish.walleye", 240 },
            { "fish.brown_trout", 440 }, { "fish.lake_trout", 464 }, { "fish.salmon", 480 }, { "fish.sturgeon", 520 },
            { "fish.longnose_gar", 720 }, { "fish.red_drum", 760 }, { "fish.red_snapper", 800 }, { "fish.tuna", 880 },
            { "ingredient.shrimp", 96 }, { "fishery.crab", 288 }, { "fishery.squid", 560 }, { "fishery.lobster", 960 } };

        private static readonly string[] Shops = { ShopCatalog.Farm, ShopCatalog.Mine, ShopCatalog.Tailor, ShopCatalog.Ranch, ShopCatalog.Fishery };
        private static ShopOffer[] Offers(ShopState s, string shop, ICollection<int> regions) => ShopService.Offers(s, shop, regions, new CraftingState());
        private static string Ids(ShopState s, string shop, ICollection<int> regions) => string.Join(",", Offers(s, shop, regions).Select(o => o.ItemId ?? "-"));
        private static ShopEntry Entry(string id, string shop) => ShopCatalog.All.First(e => e.ItemId == id && e.ShopId == shop);

        private static void StockChecks()
        {
            int[] r14 = { 1, 2, 3, 4 };
            var a = new ShopState { VisitId = 7 }; var b = new ShopState { VisitId = 7 };
            Check(Shops.All(sh => Ids(a, sh, r14) == Ids(b, sh, r14)), "OffersDeterministic");
            bool distinct = true;
            for (int v = 1; v <= 200; v++)
            {
                var s = new ShopState { VisitId = v };
                foreach (string sh in Shops) { var ids = Offers(s, sh, r14).Where(o => !o.Empty).Select(o => o.ItemId).ToList(); distinct &= ids.Distinct().Count() == ids.Count; }
            }
            Check(distinct, "OffersDistinct");
            var all12 = Enumerable.Range(1, 12).ToArray(); int fine = 0; bool roles = true;
            for (int v = 1; v <= 2000; v++)
            {
                var o = Offers(new ShopState { VisitId = v }, ShopCatalog.Tailor, all12);
                roles &= Entry(o[0].ItemId, ShopCatalog.Tailor).Role == ShopSlotRole.RawFibre && Entry(o[1].ItemId, ShopCatalog.Tailor).Role == ShopSlotRole.RawFibre;
                ShopSlotRole r2 = Entry(o[2].ItemId, ShopCatalog.Tailor).Role; roles &= r2 == ShopSlotRole.Processed || r2 == ShopSlotRole.ProcessedFine;
                if (r2 == ShopSlotRole.ProcessedFine) fine++;
            }
            Check(roles && Math.Abs(fine / 2000.0 - 0.20) <= 0.03, "TailorSlots");
            bool fish = true; var seen = new HashSet<string>();
            for (int v = 1; v <= 200; v++)
            {
                var o1 = Offers(new ShopState { VisitId = v }, ShopCatalog.Fishery, r14);
                fish &= o1[0].ItemId != o1[1].ItemId && Entry(o1[0].ItemId, ShopCatalog.Fishery).Role == ShopSlotRole.Fish && Entry(o1[1].ItemId, ShopCatalog.Fishery).Role == ShopSlotRole.Fish && o1[2].ItemId == "ingredient.shrimp";
                var o2 = Offers(new ShopState { VisitId = v }, ShopCatalog.Fishery, all12);
                fish &= Entry(o2[2].ItemId, ShopCatalog.Fishery).Role == ShopSlotRole.Exclusive; seen.Add(o2[2].ItemId);
            }
            Check(fish && seen.SetEquals(new[] { "ingredient.shrimp", "fishery.crab" }), "FisherySlots");
            var st = new ShopState { VisitId = 3 };
            string before = string.Join("|", Shops.Select(sh => Ids(st, sh, r14)));
            string same = string.Join("|", Shops.Select(sh => Ids(st, sh, new[] { 1, 2, 3, 4, 5 })));
            Check(before == same, "StockFixedWithinVisit (same visit)");
            bool differs = false;
            for (int i = 0; i < 5 && !differs; i++) { ShopService.AdvanceVisit(st); differs = string.Join("|", Shops.Select(sh => Ids(st, sh, r14))) != before; }
            Check(differs, "StockFixedWithinVisit (later visit differs)");
            var sh1 = new ShopState { VisitId = 1 }; var tail = Offers(sh1, ShopCatalog.Tailor, new[] { 1, 2 });
            Check(tail.Count(o => o.Empty) == 1, "ShortPoolLeavesSoldOutSlot (one empty)");
            int empty = Array.FindIndex(tail, o => o.Empty);
            Check(!ShopService.TryBuy(sh1, ShopCatalog.Tailor, empty, 0, 1, new Wallet(1000000), new SpatialInventory(), () => "x", out string why) && why == "Sold out", "ShortPoolLeavesSoldOutSlot (refusal)");
            var dt = new ShopState { VisitId = 1, DebugTier = 2 }; int[] r4 = { 1, 2, 3, 4 };
            Check(ShopService.ReachedTier(dt, r4) == 2 && ShopService.EffectiveRegions(dt, r4).Contains(10) && ShopCatalog.Eligible(ShopCatalog.Farm, ShopService.EffectiveRegions(dt, r4), 2).Any(e => e.ItemId == "ingredient.lavender") && ShopCatalog.Eligible(ShopCatalog.Ranch, ShopService.EffectiveRegions(dt, r4), 2).Any(e => e.ItemId == "animal.duck.meat"), "DebugTierUnlocks");
        }

        private static string Snap(SpatialInventory bag) => string.Join(";", bag.Entries.Select(e => e.Id + "/" + e.Definition.Id + "/" + e.Quantity + "/" + e.Quality + "/" + e.Owner + "/" + e.X + "/" + e.Y));

        private static void PurchaseChecks()
        {
            int[] r14 = { 1, 2, 3, 4 }; int n = 0; Func<string> id = () => "buy" + (++n);
            var s = new ShopState { VisitId = 1 }; ShopOffer o = Offers(s, ShopCatalog.Farm, r14)[0];
            ItemDefinition def = Items.Find(o.ItemId);
            var bag = new SpatialInventory(); var wallet = new Wallet(1000000);
            long cashBefore = wallet.Cash; int rev = o.Revision;
            Check(ShopService.TryBuy(s, ShopCatalog.Farm, 0, rev, 10, wallet, bag, id, out string reason), "PurchaseHappyPath (bought)");
            Check(wallet.Cash == cashBefore - 10 * o.UnitPriceMinor / Money.MinorPerCash && o.UnitsLeft == 990 && o.Revision == rev + 1, "PurchaseHappyPath (wallet, stock, revision)");
            Check(bag.Entries.Where(e => e.Owner == ItemOwner.Backpack && e.Definition.Id == o.ItemId && e.Quality == 5).Sum(e => e.Quantity) == 10, "PurchaseHappyPath (bag)");
            // Partial fit: the bag holds one 600-unit stack and has no room for a second, so only 400 more units merge.
            ItemProfile prof = ItemPrices.Profile(def, 5);
            var tiny = new SpatialInventory((int)Math.Ceiling(def.Width * 1.5), (int)Math.Ceiling(def.Height * 1.5));
            tiny.RestoreStored(tiny.AddWorld("seed", def, 600, 5, prof), ItemOwner.Backpack, 0, 0);
            Check(ShopService.StoreFits(tiny, def, prof, 400) && !ShopService.StoreFits(tiny, def, prof, 401) && !ShopService.StoreFits(tiny, def, prof, 1000), "PartialFitRefused (setup)");
            // A reserved matching stack is skipped by the real store, so the dry run must skip it too.
            {
                var sr = new ShopState { VisitId = 1 }; ShopOffer or = Offers(sr, ShopCatalog.Farm, r14)[0]; var wr = new Wallet(100000000);
                var full = new SpatialInventory((int)Math.Ceiling(def.Width * 1.5), (int)Math.Ceiling(def.Height * 1.5));
                ItemEntry held = full.AddWorld("held", def, 600, 5, prof); full.RestoreStored(held, ItemOwner.Backpack, 0, 0); full.Reserve(held, 3);
                string snapR = Snap(full);
                Check(!ShopService.TryBuy(sr, ShopCatalog.Farm, 0, 0, 100, wr, full, id, out reason) && reason == "Not enough room in the bag" && wr.Cash == 100000000 && or.UnitsLeft == 1000 && or.Revision == 0 && Snap(full) == snapR, "ReservedStackRespected (refused)");
                var roomy = new SpatialInventory();
                ItemEntry held2 = roomy.AddWorld("held", def, 600, 5, prof); roomy.RestoreStored(held2, ItemOwner.Backpack, 0, 0); roomy.Reserve(held2, 3);
                Check(ShopService.TryBuy(sr, ShopCatalog.Farm, 0, 0, 100, wr, roomy, id, out reason) && held2.Quantity == 600 && roomy.Entries.Count(e => e.Owner == ItemOwner.Backpack) == 2 && or.UnitsLeft == 900, "ReservedStackRespected (new stack)");
            }
            var s2 = new ShopState { VisitId = 1 }; ShopOffer o2 = Offers(s2, ShopCatalog.Farm, r14)[0]; var w2 = new Wallet(100000000); string snap = Snap(tiny);
            Check(!ShopService.TryBuy(s2, ShopCatalog.Farm, 0, o2.Revision, 1000, w2, tiny, id, out reason) && reason == "Not enough room in the bag" && w2.Cash == 100000000 && o2.UnitsLeft == 1000 && o2.Revision == 0 && Snap(tiny) == snap, "PartialFitRefused");
            Check(!ShopService.TryBuy(s2, ShopCatalog.Farm, 0, 0, 1, new Wallet(0), new SpatialInventory(), id, out reason) && reason == "Not enough cash" && o2.UnitsLeft == 1000 && o2.Revision == 0, "CashRefused");
            Check(!ShopService.TryBuy(s2, ShopCatalog.Farm, 0, 0, 1001, w2, new SpatialInventory(), id, out reason) && reason == "Sold out" && w2.Cash == 100000000 && o2.Revision == 0, "SoldOutRefused");
            var b3 = new SpatialInventory();
            Check(!ShopService.TryBuy(s2, ShopCatalog.Farm, 0, 5, 1, w2, b3, id, out reason) && reason == "The stock changed; check the offer again" && w2.Cash == 100000000 && o2.UnitsLeft == 1000 && b3.Entries.Count == 0, "StaleRevisionRefused");
        }

        private static SessionSnapshot Roundtrip(SessionSnapshot snap) => JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(snap));
        private static bool Rejects(Action<SavedShops> tamper)
        {
            var s = new ShopState { VisitId = 3 }; Offers(s, ShopCatalog.Farm, new[] { 1, 2, 3, 4 });
            SavedShops saved = SessionSaveStore.SaveShops(s); tamper(saved);
            try { SessionSaveStore.RestoreShops(new SessionSnapshot { shops = saved }); return false; } catch (InvalidDataException) { return true; }
        }

        private static long SaveShopsPrice()
        {
            var t = new ShopState { VisitId = 3 }; Offers(t, ShopCatalog.Farm, new[] { 1, 2, 3, 4 });
            return t.Stock[ShopCatalog.Farm].Offers[0].UnitPriceMinor;
        }

        private static void SaveChecks()
        {
            int[] r14 = { 1, 2, 3, 4 };
            var s = new ShopState { VisitId = 5, DebugTier = 2 };
            foreach (string sh in Shops) Offers(s, sh, new[] { 1, 2 });
            ShopService.TryBuy(s, ShopCatalog.Farm, 0, 0, 10, new Wallet(1000000), new SpatialInventory(), () => "sv1", out _);
            ShopState back = SessionSaveStore.RestoreShops(Roundtrip(new SessionSnapshot { shops = SessionSaveStore.SaveShops(s) }));
            Func<ShopState, string> dump = st => st.VisitId + "/" + st.DebugTier + "|" + string.Join(";", st.Stock.OrderBy(k => k.Key).Select(k => k.Key + "@" + k.Value.Visit + ":" + string.Join(",", k.Value.Offers.Select(o => (o.ItemId ?? "-") + "/" + o.UnitPriceMinor + "/" + o.UnitsLeft + "/" + o.Revision))));
            Check(dump(s) == dump(back) && back.Stock[ShopCatalog.Farm].Offers[0].UnitsLeft == 990, "ShopsRoundTrip");
            ShopState fresh = SessionSaveStore.RestoreShops(new SessionSnapshot());
            Check(fresh.VisitId == 0 && fresh.Stock.Count == 0, "AbsentBlockIsFresh");
            Check(Rejects(x => x.stock[0].shopId = "shop.nope"), "InvalidRejected (shop id)");
            // A removed item restores as a reroll (shop row dropped); a retuned price keeps the saved price; price <= 0 is malformed.
            Func<Action<SavedShops>, ShopState> restoreTampered = tamper =>
            {
                var t = new ShopState { VisitId = 3 }; Offers(t, ShopCatalog.Farm, new[] { 1, 2, 3, 4 });
                SavedShops sv = SessionSaveStore.SaveShops(t); tamper(sv);
                return SessionSaveStore.RestoreShops(new SessionSnapshot { shops = sv });
            };
            Check(!restoreTampered(x => x.stock[0].offers[0].itemId = "item.nope").Stock.ContainsKey(ShopCatalog.Farm), "RemovedItemRestoresAsReroll");
            Check(restoreTampered(x => x.stock[0].offers[0].unitPriceMinor += 1).Stock[ShopCatalog.Farm].Offers[0].UnitPriceMinor == SaveShopsPrice() + 1, "ChangedPriceKeepsSavedPrice");
            Check(Rejects(x => x.stock[0].offers[0].unitPriceMinor = 0), "InvalidRejected (price zero)");
            Check(Rejects(x => x.stock[0].offers[0].unitPriceMinor = -5), "InvalidRejected (price negative)");
            Check(Rejects(x => x.stock[0].offers[0].unitsLeft = 1001), "InvalidRejected (units high)");
            Check(Rejects(x => x.stock[0].offers[0].unitsLeft = -1), "InvalidRejected (units low)");
            Check(Rejects(x => x.visitId = -1), "InvalidRejected (visit)");
            Check(Rejects(x => x.stock[0].offers[0].revision = -1), "InvalidRejected (revision)");
            Check(Rejects(x => x.debugTier = 5), "InvalidRejected (debug tier)");
            Check(Rejects(x => x.stock[0].offers.RemoveAt(2)), "InvalidRejected (slot count)");
            Check(Rejects(x => { x.stock[0].offers[2].itemId = null; x.stock[0].offers[2].unitsLeft = 5; }), "InvalidRejected (empty slot with units)");
            // Sold-out slot (short pool) survives a reload as an empty offer.
            var shortS = new ShopState { VisitId = 1 }; Offers(shortS, ShopCatalog.Tailor, new[] { 1, 2 });
            ShopState shortBack = SessionSaveStore.RestoreShops(Roundtrip(new SessionSnapshot { shops = SessionSaveStore.SaveShops(shortS) }));
            Check(shortBack.Stock[ShopCatalog.Tailor].Offers.Count(o => o.Empty) == 1, "EmptySlotRoundTrip");
            var held = Offers(back, ShopCatalog.Farm, r14);
            Check(ReferenceEquals(held, back.Stock[ShopCatalog.Farm].Offers) && held[0].UnitsLeft == 990, "ReloadKeepsVisit");
            string root = Path.GetFullPath("Assets/_Game/Scripts/Unity/");
            foreach (string f in new[] { "CollectionPrototype.Settlement.cs", "CollectionPrototype.Travel.cs" })
            {
                string src = File.ReadAllText(root + f); int at = 0, found = 0; bool ok = true;
                while ((at = src.IndexOf("CommitTravel(() =>", at, StringComparison.Ordinal)) >= 0)
                {
                    int end = src.IndexOf((char)10, at); string lambda = src.Substring(at, end - at); at = end;
                    if (lambda.Contains("ApplySettlementArrival()")) { found++; ok &= lambda.Contains("AdvanceShopVisit()"); }
                }
                Check(found == 1 && ok, "BothArrivalsAdvance " + f);
            }
        }

        private static void ModelChecks()
        {
            // Guard for future gaps: list a shop good whose art is not built yet; PendingArtStillMissing fails once its model lands.
            string[] PendingArt = { };
            Func<GameObject, (int tris, float size)> measure = go =>
            {
                int tris = 0; Bounds bounds = default; bool first = true;
                foreach (var f in go.GetComponentsInChildren<MeshFilter>())
                {
                    tris += f.sharedMesh.triangles.Length / 3;
                    var b = f.sharedMesh.bounds; if (first) { bounds = b; first = false; } else bounds.Encapsulate(b);
                }
                return (tris, Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z));
            };
            // Slice 2 task 5: every tier 1-2 bakery food (common and premium) has a model; the five new bakes match Berry oat cakes in size, Premium apple pie in triangles.
            string[] bakeryMissing = ShopCatalog.All.Where(e => e.ShopId == ShopCatalog.Bakery && e.Tier <= 2).Select(e => e.ItemId).Distinct().Where(id => !Tier2ShopVisuals.HasModel(Items.Find(id))).ToArray();
            Check(bakeryMissing.Length == 0, "BakeryModels " + string.Join(",", bakeryMissing));
            var oatCakes = measure(Resources.Load<GameObject>("Tier01/Crafted/BerryOatCakes")); var applePie = measure(Resources.Load<GameObject>("Tier01/Crafted/PremiumApplePie"));
            foreach (string id in new[] { "item.wheat_bread", "item.pine_nut_oat_cookies", "item.strawberry_cream_roll", "item.blueberry_muffins", "item.flax_oil_biscuits" })
            {
                var m = measure(Resources.Load<GameObject>(Tier1CraftedItems.VisualPath(id) ?? Tier2ShopVisuals.PathFor(id)));
                Check(m.size >= oatCakes.size * .8f && m.size <= oatCakes.size * 1.25f && m.tris <= applePie.tris * 1.25f,
                    "BakeryModels size/triangles " + id + ": " + m.size.ToString("0.000") + " vs oat cakes " + oatCakes.size.ToString("0.000") + ", " + m.tris + " vs apple pie " + applePie.tris);
            }
            string[] missing = ShopCatalog.All.Where(e => e.Tier <= 2 && !PendingArt.Contains(e.ItemId)).Select(e => e.ItemId).Distinct().Where(id => !Tier2ShopVisuals.HasModel(Items.Find(id))).ToArray();
            Check(missing.Length == 0, "PoolGoodsHaveModels " + string.Join(",", missing));
            string[] landed = PendingArt.Where(id => Tier2ShopVisuals.HasModel(Items.Find(id))).ToArray();
            Check(landed.Length == 0, "PendingArtStillMissing (art landed, remove from PendingArt) " + string.Join(",", landed));
            // Task 6b: the raw chicken leg is a distinct model, comparable to duck meat in size and triangle count.
            var chicken = Resources.Load<GameObject>("Tier01/ShopInputs/ChickenMeat");
            var duck = Resources.Load<GameObject>("Tier02/ShopInputs/DuckMeat");
            Check(chicken != null && duck != null, "ChickenMeatModel prefab loads");
            var chickenRenderer = chicken.GetComponentInChildren<MeshRenderer>();
            Check(chickenRenderer != null && chickenRenderer.sharedMaterial != null, "ChickenMeatModel has a material");
            var c = measure(chicken); var d = measure(duck);
            Check(c.tris <= d.tris * 1.25f, "ChickenMeatModel triangles " + c.tris + " vs duck " + d.tris);
            Check(c.size >= d.size * .8f && c.size <= d.size * 1.25f, "ChickenMeatModel size " + c.size + " vs duck " + d.size);
        }

        // Task 5: the counter view, the purchase sheet's rules and texts, and the settlement list.
        private static void ViewChecks()
        {
            foreach (string name in new[] { "Farm", "Mine", "Tailor", "Ranch", "Fishery", "Bakery" })
            {
                var prefab = Resources.Load<GameObject>("Settlement/Storefronts/" + name + "Counter");
                Check(prefab != null && CollectionPrototype.ShopIdForKey(name.ToLowerInvariant()) == "shop." + name.ToLowerInvariant(), "CounterPrefabAndShopId " + name);
                foreach (Vector2 screen in new[] { new Vector2(1116, 2480), new Vector2(720, 1680) })
                {
                    var counter = UnityEngine.Object.Instantiate(prefab);
                    var cam = new GameObject("shop view check camera").AddComponent<Camera>();
                    try
                    {
                        cam.fieldOfView = StreetCamera.FieldOfView; cam.aspect = screen.x / screen.y;
                        Pose pose = CollectionPrototype.ShopCounterPose(cam, counter.transform);
                        cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                        for (int i = 1; i <= 3; i++)
                        {
                            Vector3 v = cam.WorldToViewportPoint(counter.transform.Find("OfferAnchor" + i).position);
                            Check(v.z > 0f && v.x > 0.05f && v.x < 0.95f && v.y > 0.12f && v.y < 0.48f, "CounterAnchorsInBand " + name + " OfferAnchor" + i + " at " + screen.x + "x" + screen.y + ": " + v.ToString("0.000"));
                        }
                        TagChecks(name, screen, cam, counter.transform);
                    }
                    finally { UnityEngine.Object.DestroyImmediate(cam.gameObject); UnityEngine.Object.DestroyImmediate(counter); }
                }
            }
            Check(CollectionPrototype.ShopIdForKey("lumber") == null && CollectionPrototype.ShopIdForKey("warehouse") == null && CollectionPrototype.ShopIdForKey(null) == null, "ShopIdForKey ignores other keys");
            Check(CollectionPrototype.SettlementListKeys(true).SequenceEqual(new[] { "home", "warehouse", "lumber", "bakery", "farm", "mine", "tailor", "ranch", "fishery", "return" })
                && CollectionPrototype.SettlementListSlot("bakery", true) == 3, "ListKeys (10 entries, bakery in slot 3)");
            {
                var live = new ShopOffer { ItemId = "food.apples", UnitPriceMinor = Money.Minor(16), UnitsLeft = 990 };
                Check(CollectionPrototype.OfferTagText(live, "Apples") == "Apples\n16 cash" && CollectionPrototype.OfferTagText(new ShopOffer { ItemId = "food.apples", UnitsLeft = 0 }, "Apples") == "Sold out"
                    && CollectionPrototype.OfferTagText(new ShopOffer(), "") == "Sold out", "TagText");
            }
            {
                // Slice 2 task 4: bakery tags and sheet texts (cash food, gold food, recipe offer).
                var bread = new ShopOffer { ItemId = "item.wheat_bread", UnitPriceMinor = Money.Minor(384), UnitsLeft = 1000 };
                var brioche = new ShopOffer { ItemId = "item.honey_brioche", UnitPriceMinor = 5, UnitsLeft = 1000, Currency = Currency.Gold };
                var recipe = new ShopOffer { ItemId = "item.honey_brioche", RecipeId = "recipe.food.honey_brioche", UnitPriceMinor = 50, UnitsLeft = 1, Currency = Currency.Gold };
                var learned = new ShopOffer { ItemId = "item.honey_brioche", RecipeId = "recipe.food.honey_brioche", UnitPriceMinor = 50, UnitsLeft = 0, Currency = Currency.Gold };
                Check(CollectionPrototype.OfferTagText(bread, "Wheat bread") == "Wheat bread\n384 cash" && CollectionPrototype.OfferTagText(brioche, "Honey brioche") == "Honey brioche\n5 gold"
                    && CollectionPrototype.OfferTagText(recipe, "Honey brioche") == "Recipe: Honey brioche\n50 gold" && CollectionPrototype.OfferTagText(learned, "Honey brioche") == "Sold out", "BakeryTagText");
                Check(CollectionPrototype.OfferLine(bread, 1) == "384 cash each" && CollectionPrototype.OfferLine(brioche, 1) == "5 gold each" && CollectionPrototype.OfferLine(brioche, 999) == "5 gold each"
                    && CollectionPrototype.OfferLine(brioche, 1000) == "4,500 gold for the full stack (10% off)" && CollectionPrototype.OfferLine(recipe, 1) == "Learn the recipe"
                    && CollectionPrototype.LeftLine(brioche) == "Left: 1,000" && CollectionPrototype.LeftLine(recipe) == "50 gold", "BakerySheetTexts");
                Check(CollectionPrototype.PayText(bread, 10) == "PAY 3,840 CASH" && CollectionPrototype.PayText(brioche, 999) == "PAY 4,995 GOLD" && CollectionPrototype.PayText(brioche, 1000) == "PAY 4,500 GOLD"
                    && CollectionPrototype.PayText(recipe, 1) == "PAY 50 GOLD", "BakeryPayText");
                // The full stack is offered once the discounted total is covered, though 1,000 x the serving price is not.
                Check(CollectionPrototype.GoldAmountChoices(brioche, 4500).SequenceEqual(new[] { 1, 10, 100, 1000 }) && CollectionPrototype.GoldAmountChoices(brioche, 4499).SequenceEqual(new[] { 1, 10, 100 })
                    && CollectionPrototype.GoldAmountChoices(brioche, 4).Length == 0 && CollectionPrototype.GoldAmountChoices(new ShopOffer { ItemId = "item.honey_brioche", UnitPriceMinor = 5, UnitsLeft = 15, Currency = Currency.Gold }, 100000).SequenceEqual(new[] { 1, 10 }),
                    "GoldAmountChoices");
            }
            Check(CollectionPrototype.AmountChoices(15, Money.Minor(1000), Money.Minor(16)).SequenceEqual(new[] { 1, 10 })
                && CollectionPrototype.AmountChoices(1000, long.MaxValue, 1).SequenceEqual(new[] { 1, 10, 100, 1000 })
                && CollectionPrototype.AmountChoices(0, long.MaxValue, 1).Length == 0, "AmountChoicesCapped");
            var offer = new ShopOffer { ItemId = "food.apples", UnitPriceMinor = Money.Minor(16), UnitsLeft = 990 };
            Check(CollectionPrototype.OfferLine(offer) == "16 cash each" && CollectionPrototype.LeftLine(offer) == "Left: 990", "Texts");
            // Ten list entries (slice 2 adds the bakery) clear the closed-bag MENU square, rows at least 48 dp tall (the phone at 401 dpi; 320 dpi assumed for 720x1680).
            foreach (var (w, h, dpi) in new[] { (1116f, 2480f, 401f), (720f, 1680f, 320f) })
            {
                float scale = Mathf.Sqrt(w / 720f * h / 1280f);   // CanvasScaler: 720x1280 reference, match 0.5
                float sw = w / scale, sh = h / scale, tap = 48f * dpi / 160f / scale; int rows = CollectionPrototype.SettlementListKeys(true).Length;
                Rect low = CollectionPrototype.SettlementListRowRect(sw, sh, rows - 1, rows, tap), menu = CollectionPrototype.UserMenuSquareRect(sw, sh);
                Rect above = CollectionPrototype.SettlementListRowRect(sw, sh, rows - 2, rows, tap);
                Check(rows == 10 && !low.Overlaps(menu) && low.yMin > menu.yMax && above.yMin > low.yMax && low.height * scale >= 48f * dpi / 160f - 0.01f,
                    "ListClearsMenu at " + w + "x" + h + ": lowest row " + low.yMin.ToString("0.0") + "-" + low.yMax.ToString("0.0") + ", MENU " + menu.yMin.ToString("0.0") + "-" + menu.yMax.ToString("0.0") + ", row " + (low.height * scale).ToString("0.0") + " px");
            }
            Check(CollectionPrototype.SettlementListRowRect(720f, 1280f, 0, 1).height == 84f && CollectionPrototype.SettlementListRowY(0) == -94f, "A short list keeps the full 84 + 10 rows");
            {
                var s = new ShopState { VisitId = 1 }; ShopOffer o = Offers(s, ShopCatalog.Farm, new[] { 1, 2, 3, 4 })[0];
                var log = new List<TradeRecord> { new TradeRecord { result = "WarehouseDeposit" } }; var when = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);
                bool bought = CollectionPrototype.BuyAndRecord(s, ShopCatalog.Farm, 0, 0, 10, new Wallet(1000000), new SpatialInventory(), new CraftingState(), new[] { 1, 2, 3, 4 }, () => "rec1", log, when, out string why);
                TradeRecord r = log.Last(); long total = 10 * o.UnitPriceMinor;
                Check(bought && log.Count == 2 && r.utc == when.ToString("O", System.Globalization.CultureInfo.InvariantCulture) && r.settlementId == ShopCatalog.Farm && r.itemId == o.ItemId && r.quantity == 10 && r.quality == 5
                    && r.unitMinor == o.UnitPriceMinor && r.totalMinor == total && r.cash == -total / Money.MinorPerCash && r.result == "shop.bought", "PurchaseRecorded");
                Check(!CollectionPrototype.BuyAndRecord(s, ShopCatalog.Farm, 0, 0, 10, new Wallet(1000000), new SpatialInventory(), new CraftingState(), new[] { 1, 2, 3, 4 }, () => "rec2", log, when, out why) && log.Count == 2, "PurchaseRecorded (a refused purchase adds no record)");
                var back = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(new SessionSnapshot { warehouseLog = log }));
                Check(back.warehouseLog.Count == 2 && back.warehouseLog[1].result == "shop.bought" && back.warehouseLog[1].cash == r.cash, "PurchaseRecorded (the record survives a save round trip)");
            }
            {
                // Slice 2 task 4: a gold food records the gold charged (discounted for the full stack), cash 0.
                var when = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc); var log = new List<TradeRecord>();
                var gs = new ShopState { VisitId = 1 }; ShopOffer go = Offers(gs, ShopCatalog.Bakery, new[] { 1, 2, 3, 4 })[2]; long g = go.UnitPriceMinor; var gw = new Wallet(0, 900 * g);
                Check(CollectionPrototype.BuyAndRecord(gs, ShopCatalog.Bakery, 2, 0, 1000, gw, new SpatialInventory(), new CraftingState(), new[] { 1, 2, 3, 4 }, () => "gr1", log, when, out string why) && gw.Gold == 0 && log.Count == 1
                    && log[0].itemId == go.ItemId && log[0].quantity == 1000 && log[0].gold == -900 * g && log[0].cash == 0 && log[0].unitMinor == g && log[0].totalMinor == 900 * g && log[0].settlementId == ShopCatalog.Bakery, "GoldPurchaseRecorded");
                // A recipe bought through the UI's buy path: one record, the recipe id, quantity 1, gold = -price, cash 0; known, the bag untouched.
                var know = Know("recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter");
                ShopState rs = RecipeStock(know, out ShopOffer ro); long price = ro.UnitPriceMinor; var rw = new Wallet(500, price); var rlog = new List<TradeRecord>(); var bag = new SpatialInventory();
                Check(CollectionPrototype.BuyAndRecord(rs, ShopCatalog.Bakery, 2, ro.Revision, 1, rw, bag, know, R6, () => "rr1", rlog, when, out why) && rlog.Count == 1 && know.Knows(ro.RecipeId) && rw.Gold == 0 && rw.Cash == 500 && bag.Entries.Count == 0
                    && rlog[0].itemId == ro.RecipeId && rlog[0].quantity == 1 && rlog[0].gold == -price && rlog[0].cash == 0 && rlog[0].totalMinor == price && rlog[0].result == "shop.bought", "RecipePayRecordsGold");
                // Paying again (the recipe now known, the slot sold out): refused, nothing recorded or charged.
                var again = new Wallet(0, price);
                Check(!CollectionPrototype.BuyAndRecord(rs, ShopCatalog.Bakery, 2, ro.Revision, 1, again, bag, know, R6, () => "rr2", rlog, when, out why) && rlog.Count == 1 && again.Gold == price, "RecipePayRecordsGold (a refused recipe adds no record)");
                ShopState ks = RecipeStock(Know("recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter"), out ShopOffer ko); var kw = new Wallet(0, ko.UnitPriceMinor);
                Check(!CollectionPrototype.BuyAndRecord(ks, ShopCatalog.Bakery, 2, ko.Revision, 1, kw, bag, Know("recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter", ko.RecipeId), R6, () => "rr3", rlog, when, out why)
                    && why == "You already know this recipe" && rlog.Count == 1 && kw.Gold == ko.UnitPriceMinor, "RecipeKnownRefusedAtPay");
            }
        }

        // Task 7 R1: price tags. Text, then layout at the counter framing (tags clear each other, sit in the area, rest on the model tops), then taps at tag centres.
        internal static Rect[] LastTagRects; internal static Vector2[] LastTagTops;
        private static void TagChecks(string name, Vector2 screen, Camera cam, Transform counter)
        {
            var offers = Offers(new ShopState { VisitId = 1 }, CollectionPrototype.ShopIdForKey(name.ToLowerInvariant()), new[] { 1, 2, 3, 4 });
            var tops = new Vector2[3]; var texts = new string[3]; var models = new List<GameObject>(); var anchors = new Vector2?[3]; var modelRects = new Rect[3];
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    Transform anchor = counter.Find("OfferAnchor" + (i + 1));
                    foreach (Transform baked in anchor) baked.gameObject.SetActive(false);
                    ItemDefinition def = Items.Find(offers[i].ItemId);
                    GameObject model = CollectionPrototype.PlaceOfferModel(anchor, def); models.Add(model);
                    var b = new Bounds(anchor.position, Vector3.zero);
                    foreach (Renderer r in model.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
                    Vector3 top = cam.WorldToScreenPoint(new Vector3(anchor.position.x, b.max.y, anchor.position.z));
                    tops[i] = top; anchors[i] = cam.WorldToScreenPoint(anchor.position); modelRects[i] = CollectionPrototype.ScreenBoundsRect(cam, b);
                    texts[i] = CollectionPrototype.OfferTagText(offers[i], def.Name);
                }
                Rect area = TagArea(screen, out float h, out float gap);
                var chars = texts.Select(CollectionPrototype.LongestLine).ToArray(); var tags = new Rect[3];
                CollectionPrototype.ShopTagLayout(tops, chars, area, h, gap, tags, modelRects);
                LastTagRects = tags; LastTagTops = tops;
                CollectionPrototype.ShopTagLayout(tops, chars, area, h, gap, tags, modelRects);   // warm up
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int n = 0; n < 100; n++) CollectionPrototype.ShopTagLayout(tops, chars, area, h, gap, tags, modelRects);
                Check(GC.GetAllocatedBytesForCurrentThread() - before == 0, "ShopTagLayoutAllocatesNothing " + name + " at " + screen.x + "x" + screen.y);
                string at = " " + name + " at " + screen.x + "x" + screen.y + ": " + string.Join(" ", tags.Select(t => t.ToString("0")));
                Func<Rect, bool> inside = t => t.xMin >= area.xMin - 0.01f && t.xMax <= area.xMax + 0.01f && t.yMin >= area.yMin && t.yMax <= area.yMax + 0.01f && t.width > 0f;
                // Negative control: a tag pushed 1 px past each edge must fail the same predicate, so the inset check can fail.
                Check(!inside(new Rect(area.xMax - tags[0].width + 1f, tags[0].y, tags[0].width, h)) && !inside(new Rect(area.xMin - 1f, tags[0].y, tags[0].width, h)) && !inside(new Rect(tags[0].x, area.yMax - h + 1f, tags[0].width, h)) && inside(tags[0]), "InsetCheckCanFail" + at);
                for (int i = 0; i < 3; i++)
                {
                    Check(inside(tags[i]), "TagInsideArea " + i + at);
                    Check(tags[i].yMin >= tops[i].y, "TagAboveModelTop " + i + at + " top " + tops[i].y.ToString("0"));
                    for (int j = i + 1; j < 3; j++) Check(!tags[i].Overlaps(tags[j]), "TagsDoNotOverlap " + i + "," + j + at);
                    Check(CollectionPrototype.ShopTapPick(tags[i].center, anchors, tags, screen.x * 0.1f) == i, "TapAtTagCentre " + i + at);
                }
            }
            finally { foreach (GameObject m in models) if (m != null) UnityEngine.Object.DestroyImmediate(m); }
        }

        // The tag layout inputs: representative insets (16 dp side cutouts, 24 dp gesture bar below, 48 dp status bar / notch above;
        // 401 dpi phone, 320 dpi for 720x1680), the tag height and the gap above a model top.
        internal static Rect TagArea(Vector2 screen, out float h, out float gap)
        {
            float scale = Mathf.Sqrt(screen.x / 720f * screen.y / 1280f), px = (screen.x > 1000f ? 401f : 320f) / 160f;
            h = 76f * scale; gap = screen.y * 0.01f;
            return Rect.MinMaxRect(16f * px, 24f * px, screen.x - 16f * px, screen.y - 48f * px);
        }

        private static Bounds RendererBounds(Component c)
        {
            Renderer[] rs = c.GetComponentsInChildren<Renderer>();
            Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        // Projected bounds in screen pixels (y up), from the viewport so the result matches `screen` whatever the Editor's view size.
        private static Rect ScreenRect(Camera cam, Bounds b, Vector2 screen)
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            for (int c = 0; c < 8; c++)
            {
                Vector3 v = cam.WorldToViewportPoint(b.center + Vector3.Scale(b.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1)));
                x0 = Mathf.Min(x0, v.x * screen.x); x1 = Mathf.Max(x1, v.x * screen.x); y0 = Mathf.Min(y0, v.y * screen.y); y1 = Mathf.Max(y1, v.y * screen.y);
            }
            return Rect.MinMaxRect(x0, y0, x1, y1);
        }

        // Task 7 R2: ceiling lamps. Each counter is built as the game builds it (a holder 200 m down, the counter, its colliders gone,
        // the warehouse's two crates), then lit by CounterLamps.Hang. Lamps hang just under the roof, clear of it and above the keeper,
        // carry at most 2 warm shadowless point lights in all, and stay off the offers, crates, price tags and crate labels on screen.
        private static void CounterLampChecks()
        {
            string longest = PrototypeItems.All.Concat(HarvestTierItems.All).Concat(ShopItems.All).Select(d => d.Name).OrderByDescending(n => n.Length).First() + " 10/10";   // the widest crate label
            foreach (string name in new[] { "Farm", "Ranch", "Fishery", "Mine", "Tailor", "Bakery", "Warehouse", "Lumber" })
            {
                var holder = new GameObject("lamp check " + name).transform; holder.position = MovementPrototype.InteriorWorldPosition;
                Transform[] lamps; Light[] lights;
                var colliders = new List<Collider>(); var models = new List<GameObject>();
                var cam = new GameObject("lamp check camera").AddComponent<Camera>();
                try
                {
                    Transform counter = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Settlement/Storefronts/" + name + "Counter"), holder, false).transform;
                    foreach (Collider c in counter.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(c);
                    var crates = new Bounds[2];
                    if (name == "Warehouse")
                        for (int i = 0; i < 2; i++)
                        {
                            var crate = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Settlement/Storefronts/DepositCrate"), holder, false);
                            crate.transform.position = counter.Find("OfferAnchor" + (i + 1)).position; crates[i] = RendererBounds(crate.transform);
                        }
                    Transform keeper = counter.Find("Shopkeeper");
                    float headTop = RendererBounds(keeper).max.y;
                    CounterLamps.Hang(counter, name);
                    lamps = holder.GetComponentsInChildren<Transform>(true).Where(t => t.name == CounterLamps.LampName).ToArray();
                    lights = holder.GetComponentsInChildren<Light>(true);
                    Check(lamps.Length >= 1 && lights.Length >= 1 && lights.Length <= 2, "CounterLampCount " + name + ": " + lamps.Length + " lamp(s), " + lights.Length + " light(s)");
                    foreach (Light l in lights)
                    {
                        Color c = l.color;
                        Check(l.type == LightType.Point && l.shadows == LightShadows.None && l.range > 0f && l.range <= 6f && c.r >= c.g && c.g >= c.b && c.r > c.b,
                            "CounterLampLight " + name + ": " + l.type + ", shadows " + l.shadows + ", range " + l.range + ", colour " + c);
                        Check(lamps.Any(lamp => l.transform.IsChildOf(lamp)), "CounterLampExplainsLight " + name + " (every light sits in a lamp)");
                    }
                    Check(lamps.All(t => t.IsChildOf(counter)), "CounterLampsUnderHolder " + name);

                    // The roof: temporary mesh colliders on the counter art (not the keeper or the lamps).
                    foreach (MeshFilter f in counter.GetComponentsInChildren<MeshFilter>())
                        if (!f.transform.IsChildOf(keeper) && !lamps.Any(lamp => f.transform.IsChildOf(lamp))) { var mc = f.gameObject.AddComponent<MeshCollider>(); mc.sharedMesh = f.sharedMesh; colliders.Add(mc); }
                    Physics.SyncTransforms();
                    foreach (Transform lamp in lamps)
                    {
                        Bounds b = RendererBounds(lamp); string at = " " + name + " lamp at " + lamp.localPosition.ToString("0.00");
                        Check(!Physics.OverlapBox(b.center, b.extents, Quaternion.identity).Any(colliders.Contains), "CounterLampClearsRoof" + at);
                        Check(Physics.Raycast(new Vector3(b.center.x, b.max.y, b.center.z), Vector3.up, out RaycastHit hit, 0.05f) && colliders.Contains(hit.collider), "CounterLampHangsUnderRoof" + at + " (top " + b.max.y.ToString("0.000") + ")");
                        Check(b.min.y > headTop + 0.3f, "CounterLampAboveKeeper" + at + ": bottom " + b.min.y.ToString("0.00") + ", head " + headTop.ToString("0.00"));
                    }
                    foreach (Collider c in colliders) UnityEngine.Object.DestroyImmediate(c);

                    foreach (Vector2 screen in new[] { new Vector2(1116, 2480), new Vector2(720, 1680) })
                    {
                        cam.fieldOfView = StreetCamera.FieldOfView; cam.aspect = screen.x / screen.y;
                        var keep = new List<(string what, Rect rect)>();
                        Pose pose;
                        if (name == "Lumber")
                        {
                            pose = CollectionPrototype.LumberCounterPose(cam, counter);
                            cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                            foreach (string part in new[] { "Ledger", "DoorPivot" }) keep.Add((part, ScreenRect(cam, RendererBounds(counter.Find(part)), screen)));
                        }
                        else if (name == "Warehouse")
                        {
                            Transform eye = counter.Find("CustomerCameraAnchor");
                            cam.transform.SetPositionAndRotation(eye.position, eye.rotation);
                            pose = CollectionPrototype.WarehouseCameraPose(cam, crates, 0.3f);
                            cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                            float scale = Mathf.Sqrt(screen.x / 720f * screen.y / 1280f);
                            for (int i = 0; i < 2; i++)
                            {
                                keep.Add(("crate " + (i + 1), ScreenRect(cam, crates[i], screen)));
                                Vector3 top = cam.WorldToViewportPoint(new Vector3(crates[i].center.x, crates[i].max.y, crates[i].center.z));
                                float w = CollectionPrototype.WarehouseLabelWidth(longest) * screen.x;
                                keep.Add(("crate label " + (i + 1), new Rect(CollectionPrototype.WarehouseLabelLeft(top.x * screen.x, w, 0f, screen.x), top.y * screen.y + screen.y * 0.01f, w, 56f * scale)));
                            }
                        }
                        else
                        {
                            pose = CollectionPrototype.ShopCounterPose(cam, counter);
                            cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                            var offers = Offers(new ShopState { VisitId = 1 }, CollectionPrototype.ShopIdForKey(name.ToLowerInvariant()), new[] { 1, 2, 3, 4 });
                            var tops = new Vector2[3]; var chars = new int[3]; var modelRects = new Rect[3];
                            for (int i = 0; i < 3; i++)
                            {
                                Transform anchor = counter.Find("OfferAnchor" + (i + 1));
                                foreach (Transform baked in anchor) baked.gameObject.SetActive(false);
                                ItemDefinition def = Items.Find(offers[i].ItemId);
                                GameObject model = CollectionPrototype.PlaceOfferModel(anchor, def); models.Add(model);
                                Bounds mb = RendererBounds(model.transform); mb.Encapsulate(anchor.position);
                                keep.Add(("offer " + (i + 1), modelRects[i] = ScreenRect(cam, mb, screen)));
                                tops[i] = Vector2.Scale(cam.WorldToViewportPoint(new Vector3(anchor.position.x, mb.max.y, anchor.position.z)), screen);
                                chars[i] = CollectionPrototype.LongestLine(CollectionPrototype.OfferTagText(offers[i], def.Name));
                            }
                            var tags = new Rect[3]; Rect area = TagArea(screen, out float h, out float gap);
                            CollectionPrototype.ShopTagLayout(tops, chars, area, h, gap, tags, modelRects);
                            for (int i = 0; i < 3; i++) keep.Add(("price tag " + (i + 1), tags[i]));
                            foreach (GameObject m in models) UnityEngine.Object.DestroyImmediate(m);
                            models.Clear();
                        }
                        cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                        foreach (Transform lamp in lamps)
                        {
                            Rect r = ScreenRect(cam, RendererBounds(lamp), screen); string at = " " + name + " lamp at " + lamp.localPosition.ToString("0.00") + " at " + screen.x + "x" + screen.y + " " + r.ToString("0");
                            Check(r.center.x > 0f && r.center.x < screen.x && r.center.y > 0f && r.center.y < screen.y, "CounterLampInView" + at);
                            foreach (var (what, rect) in keep) Check(!r.Overlaps(rect), "CounterLampClearsView" + at + " vs " + what + " " + rect.ToString("0"));
                        }
                    }
                }
                finally
                {
                    foreach (GameObject m in models) if (m != null) UnityEngine.Object.DestroyImmediate(m);
                    UnityEngine.Object.DestroyImmediate(cam.gameObject); UnityEngine.Object.DestroyImmediate(holder.gameObject);
                }
                Check(lamps.All(t => t == null) && lights.All(l => l == null), "CounterLampsGoWithTheHolder " + name);
            }
        }

        // Task 6: tier 2 storefront looks. Each front has a birch <Front>T2 that fits its street slot within 1.25x the tier 1
        // triangles; the street shows it once the shops reach tier 2 and falls back to tier 1 when the prefab is missing.
        private static void StorefrontChecks()
        {
            Check(SettlementScenery.FrontFor("FarmFront", 1) == "FarmFront" && SettlementScenery.FrontFor("FarmFront", 2) == "FarmFrontT2", "FrontFor");
            for (int i = 0; i < SettlementScenery.Storefronts.Count; i++)
            {
                string front = SettlementScenery.Storefronts[i].Front;
                var t2 = Resources.Load<GameObject>("Settlement/Storefronts/" + front + "T2");
                Check(t2 != null, "T2FrontLoads " + front + "T2");
                int t1Tris = Triangles(Resources.Load<GameObject>("Settlement/Storefronts/" + front)), t2Tris = Triangles(t2);
                float width = Width(t2);
                Check(width <= SettlementScenery.StorefrontSlots[i], "T2FrontFitsSlot " + front + "T2 " + width.ToString("0.00") + " m <= " + SettlementScenery.StorefrontSlots[i]);
                Check(t2Tris <= 1.25f * t1Tris, "T2FrontTriangles " + front + "T2 " + t2Tris + " <= 1.25 x " + t1Tris);
            }
            var parent = new GameObject("storefront check street");
            try
            {
                int[] levels = { 1, 1, 1, 1, 1, 1 };
                SettlementStreet t1 = SettlementScenery.Build(parent.transform, levels, -1, 1), t2 = SettlementScenery.Build(parent.transform, levels, -1, 2);
                SettlementStreet missing = SettlementScenery.Build(parent.transform, levels, -1, null, false, path => path.EndsWith("T2") ? null : Resources.Load<GameObject>(path), 2);
                foreach (var (name, front, _) in SettlementScenery.Storefronts)
                {
                    string id = name == "Lumber" ? "lumber" : name == "Warehouse" ? "warehouse" : "shop." + name.ToLowerInvariant();
                    Transform a = t1.Find(id).Root, b = t2.Find(id).Root, c = missing.Find(id).Root;
                    Check(a.Find(front) != null && b.Find(front + "T2") != null && b.Find(front) == null && c.Find(front) != null && b.GetComponent<BoxCollider>() != null,
                        "StreetShowsTierLook " + id + " (tier 1 " + front + ", tier 2 " + front + "T2 with its selection box, tier 1 when T2 is missing)");
                }
                // Tier 2 with the lumber placeholder forced: still the fence-and-pines frontage, no LumberFront look of either tier.
                Transform lumber = SettlementScenery.Build(parent.transform, levels, -1, null, true, Resources.Load<GameObject>, 2).Find("lumber").Root;
                Check(lumber.Find("LumberFront") == null && lumber.Find("LumberFrontT2") == null && lumber.Find("FenceSection") != null && lumber.Find("MassivePineA") != null,
                    "StreetTier2Placeholder lumber (fence sections and pines, no LumberFront or LumberFrontT2)");
            }
            finally { UnityEngine.Object.DestroyImmediate(parent); }
        }

        private static int Triangles(GameObject prefab)
        {
            int n = 0;
            foreach (MeshFilter f in prefab.GetComponentsInChildren<MeshFilter>(true))
                if (f.sharedMesh != null) for (int s = 0; s < f.sharedMesh.subMeshCount; s++) n += (int)f.sharedMesh.GetIndexCount(s) / 3;
            return n;
        }

        // Renderer bounds width across the street (an instance: prefab assets have no world bounds).
        private static float Width(GameObject prefab)
        {
            var go = UnityEngine.Object.Instantiate(prefab);
            try
            {
                Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
                Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
                return b.size.x;
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        private static void CatalogChecks()
        {
            int[] r14 = { 1, 2, 3, 4 };
            Check(ShopCatalog.Eligible(ShopCatalog.Farm, r14, 1).Count() == 5, "farm opens with 5 goods");
            Check(ShopCatalog.Eligible(ShopCatalog.Mine, r14, 1).Count() == 3, "mine opens with Flint, Clay, Limestone");
            Check(ShopCatalog.Eligible(ShopCatalog.Tailor, r14, 1).Count(e => e.Role == ShopSlotRole.RawFibre) == 2, "tailor opens with Hemp and Nettle fibre");
            Check(ShopCatalog.Eligible(ShopCatalog.Ranch, r14, 1).Count() == 4, "ranch opens with 4 goods");
            Check(ShopCatalog.Eligible(ShopCatalog.Fishery, r14, 1).Count(e => e.Role == ShopSlotRole.Fish) == 4 && ShopCatalog.Eligible(ShopCatalog.Fishery, r14, 1).Single(e => e.Role == ShopSlotRole.Exclusive).ItemId == "ingredient.shrimp", "fishery opens with 4 fish + Shrimp");
            Check(ShopCatalog.Eligible(ShopCatalog.Tailor, r14, 1).Where(e => e.Role != ShopSlotRole.RawFibre).All(e => e.ItemId.Contains("hemp") || e.ItemId.Contains("nettle")), "tailor's opening textiles are hemp or nettle");
            // Totals with every region of tiers <= T discovered (spec counts).
            for (int t = 1; t <= 4; t++)
            {
                var all = Enumerable.Range(1, 6 * t).ToArray();
                Check(ShopCatalog.Eligible(ShopCatalog.Farm, all, t).Count() == new[] { 8, 17, 26, 32 }[t - 1], "farm pool at tier " + t);
                Check(ShopCatalog.Eligible(ShopCatalog.Mine, all, t).Count() == new[] { 5, 10, 15, 24 }[t - 1], "mine pool at tier " + t);
                Check(ShopCatalog.Eligible(ShopCatalog.Ranch, all, t).Count() == new[] { 4, 5, 6, 7 }[t - 1], "ranch pool at tier " + t);
                Check(ShopCatalog.Eligible(ShopCatalog.Tailor, all, t).Count(e => e.Role == ShopSlotRole.RawFibre) == new[] { 3, 5, 7, 8 }[t - 1], "tailor raw pool at tier " + t);
                Check(ShopCatalog.Eligible(ShopCatalog.Fishery, all, t).Count(e => e.Role == ShopSlotRole.Fish) == 4 * t, "fishery fish at tier " + t);
            }
            // Prices (cash per unit) pinned from the spec.
            foreach (var (id, cash) in new[] { ("food.apples", 16L), ("ingredient.rock_salt", 24L), ("material.copper_ore", 32L), ("material.clay", 8L), ("fish.bluegill", 64L), ("fish.common_carp", 200L), ("ingredient.shrimp", 96L) })
                Check(ShopCatalog.UnitPriceMinor(ShopCatalog.All.First(e => e.ItemId == id)) == Money.Minor(cash), id + " costs " + cash);
            Check(ShopCatalog.All.Where(e => e.ShopId == ShopCatalog.Ranch && e.CashOverride.HasValue).Select(e => e.CashOverride.Value).OrderBy(v => v).SequenceEqual(new long[] { 50, 100, 180, 300 }), "ranch meats keep their authored prices");
            Check(ShopCatalog.All.Count(e => e.ItemId == "ingredient.rock_salt") == 1 && ShopCatalog.All.Single(e => e.ItemId == "ingredient.rock_salt").ShopId == ShopCatalog.Farm, "Rock salt only at the farm");
            Check(ShopCatalog.All.All(e => Items.Find(e.ItemId) != null), "every pool good has an item definition");
            Check(ShopCatalog.All.Where(e => e.ShopId == ShopCatalog.Tailor).All(e => !e.ItemId.StartsWith("animal.")), "no hunted-animal goods at the tailor");
            Check(ShopCatalog.All.Select(e => e.ShopId + "/" + e.ItemId).Distinct().Count() == ShopCatalog.All.Count, "no duplicate catalog rows");
            Check(ShopCatalog.All.Where(e => e.ShopId != ShopCatalog.Bakery).All(e => SpecPrices.ContainsKey(e.ItemId)) && SpecPrices.Keys.All(id => ShopCatalog.All.Any(e => e.ItemId == id)), "the price table and the catalog list the same goods");
            foreach (ShopEntry e in ShopCatalog.All.Where(x => x.ShopId != ShopCatalog.Bakery))
            {
                long q5 = ItemPrices.Profile(Items.Find(e.ItemId), 5).UnitValue;
                Check(q5 > 0 && 4 * q5 <= ShopCatalog.UnitPriceMinor(e), e.ItemId + " sells for less than the shop charges");
                long rule = e.CashOverride ?? 4 * ItemPrices.Profile(Items.Find(e.ItemId), 5).UnitValue / Money.MinorPerCash;
                Check(rule == SpecPrices[e.ItemId] && ShopCatalog.UnitPriceMinor(e) == Money.Minor(SpecPrices[e.ItemId]), e.ShopId + " " + e.ItemId + " costs " + SpecPrices[e.ItemId]);
            }
        }

        // Slice 2 task 1: bakery pools, prices, recipes and the catalog-4 missions.
        private static IEnumerable<ShopEntry> Pool(ShopSlotRole role, int tier) => ShopCatalog.All.Where(e => e.ShopId == ShopCatalog.Bakery && e.Role == role && e.Tier == tier);

        private static void BakeryDataChecks()
        {
            Check(Enumerable.Range(1, 4).Select(t => Pool(ShopSlotRole.Common, t).Count()).SequenceEqual(new[] { 4, 4, 5, 3 })
                && Enumerable.Range(1, 4).All(t => Pool(ShopSlotRole.Premium, t).Count() == 2)
                && ShopCatalog.All.Count(e => e.ShopId == ShopCatalog.Bakery) == 24 && ShopCatalog.All.Where(e => e.ShopId == ShopCatalog.Bakery).Select(e => e.ItemId).Distinct().Count() == 24
                && ShopCatalog.All.Where(e => e.ShopId == ShopCatalog.Bakery).All(e => Items.Find(e.ItemId) != null && e.Region == 0), "BakeryPoolCounts");
            long[] cash = { 384, 1152, 2304, 3840 };
            Check(ShopCatalog.All.Where(e => e.ShopId == ShopCatalog.Bakery && e.Role == ShopSlotRole.Common).All(e => Money.Charge(ShopCatalog.UnitPriceMinor(e)) == cash[e.Tier - 1]), "CommonPrices");
            var gold = new Dictionary<string, long> { { "premium_apple_pie", 8 }, { "strawberry_cream_roll", 7 }, { "honey_brioche", 5 }, { "blackberry_cream_pastry", 7 },
                { "pumpkin_cow_cheese_galette", 8 }, { "mint_sugar_rolls", 9 }, { "vanilla_cacao_celebration_cake", 10 }, { "coconut_cream_tart", 10 } };
            var premium = ShopCatalog.All.Where(e => e.ShopId == ShopCatalog.Bakery && e.Role == ShopSlotRole.Premium).ToList();
            Check(premium.Count == 8 && premium.All(e => gold[e.ItemId.Substring(5)] == e.GoldPerServing && ShopCatalog.RecipeFor(e.ItemId) == "recipe.food." + e.ItemId.Substring(5) && RecipeBook.Find(ShopCatalog.RecipeFor(e.ItemId)) != null)
                && ShopCatalog.All.Where(e => e.Role != ShopSlotRole.Premium).All(e => !e.GoldPerServing.HasValue) && ShopCatalog.RecipeFor("item.wheat_bread") == null && ShopCatalog.RecipeFor("food.apples") == null, "PremiumGold");

            // id, tier, yield, hearts, inputs "id:qty,..."
            var spec = new (string id, int tier, int yield, int hearts, string inputs)[] {
                ("pine_nut_oat_cookies", 1, 2, 2, "ingredient.oats:2,ingredient.pine_nuts:1,ingredient.cow_butter:1"), ("wheat_bread", 1, 2, 2, "ingredient.wheat_flour:3,ingredient.rock_salt:1"),
                ("blueberry_muffins", 2, 2, 3, "ingredient.wheat_flour:2,food.blueberries:2,ingredient.egg:1"), ("flax_oil_biscuits", 2, 2, 3, "ingredient.wheat_flour:2,ingredient.flax_oil:1"),
                ("potato_butter_rolls", 4, 2, 5, "ingredient.wheat_flour:2,ingredient.potatoes:2,ingredient.cow_butter:1"), ("coconut_macaroons", 4, 2, 5, "food.coconuts:2,ingredient.egg:1,ingredient.sugar:1"),
                ("strawberry_cream_roll", 1, 1, 10, "ingredient.wheat_flour:2,food.strawberries:2,ingredient.cow_cream:1,ingredient.egg:1"),
                ("honey_brioche", 2, 1, 8, "ingredient.wheat_flour:2,ingredient.cow_butter:1,ingredient.honey:1"),
                ("blackberry_cream_pastry", 2, 1, 10, "ingredient.wheat_flour:2,food.blackberries:2,ingredient.cow_cream:1"),
                ("pumpkin_cow_cheese_galette", 3, 1, 10, "ingredient.wheat_flour:2,ingredient.pumpkins:2,ingredient.cow_cheese:1,ingredient.cow_butter:1"),
                ("mint_sugar_rolls", 3, 1, 11, "ingredient.wheat_flour:2,ingredient.wild_mint:1,ingredient.sugar:1,ingredient.cow_butter:1"),
                ("vanilla_cacao_celebration_cake", 4, 1, 12, "ingredient.wheat_flour:2,ingredient.cow_cream:1,ingredient.cacao_powder:1,ingredient.vanilla_pods:1,ingredient.sugar:1"),
                ("coconut_cream_tart", 4, 1, 12, "ingredient.wheat_flour:2,food.coconuts:2,ingredient.cow_cream:1,ingredient.sugar:1") };
            foreach (var r in spec)
            {
                RecipeDefinition def = RecipeBook.Find("recipe.food." + r.id);
                string got = def == null ? "" : string.Join(",", def.Inputs.Select(i => i.ItemId + ":" + i.Quantity));
                Check(def != null && def.Role == RecipeRole.Food && def.Tier == r.tier && def.Yield == r.yield && def.OutputId == "item." + r.id && got == r.inputs && def.BaseSeconds == 3.0f
                    && def.Output.FoodHearts == r.hearts && ItemTiers.Of(def.OutputId) == r.tier && def.Inputs.All(i => ItemTiers.InWindow(def, i.ItemId)), "RecipesExact " + r.id);
            }
            Check(ItemTiers.Of("ingredient.sugar") == 3 && ItemTiers.Of("ingredient.flax_oil") == 2 && ItemTiers.Of("ingredient.cacao_powder") == 4
                && Items.Find("ingredient.sugar").Category == ItemCategory.RefinedIngredient && ItemTiers.Of("food.blackberries") == 2 && ItemTiers.Of("ingredient.vanilla_pods") == 4, "RefinedInputTiers");

            MissionDefinition g16 = MissionBook.Find("mission.t1.g16"), g17 = MissionBook.Find("mission.t1.g17");
            Check(MissionBook.CatalogVersion == 4 && g16 != null && g17 != null && g16.RequestedItemId == "ingredient.cow_butter" && g16.RewardRecipeId == "recipe.food.pine_nut_oat_cookies" && g16.EarliestRegion == 5
                && g17.RequestedItemId == "ingredient.wheat_flour" && g17.RewardRecipeId == "recipe.food.wheat_bread" && g17.EarliestRegion == 6 && g16.AddedInCatalog == 4 && g17.AddedInCatalog == 4
                && g16.Branch == MissionBranch.Gatherer && g17.Branch == MissionBranch.Gatherer, "MissionsG16G17");
            string[] g3 = MissionBook.Branch(MissionBranch.Gatherer, 3).Select(m => m.Code).ToArray(), g4 = MissionBook.Branch(MissionBranch.Gatherer).Select(m => m.Code).ToArray();
            Check(string.Join(",", g4) == "G1,G2,G3,G4,G7,G8,G12,G5,G6,G17,G9,G10,G11,G13,G14,G16,G15" && g3.Length == 15 && !g3.Contains("G16") && !g3.Contains("G17"), "Catalog4GathererOrder");
            // A catalog-3 save (claims made under catalog 3, up to G14) still loads; the new missions are simply next.
            string[] claimed = g3.Take(Array.IndexOf(g3, "G14") + 1).Select(c => "mission.t1." + c.ToLowerInvariant()).ToArray();
            MissionState old = MissionState.Restore(claimed, null, 0, 6, 3, 0, false, claimed.Select(_ => 3).ToList(), 0);
            Check(old.NextFor(MissionBranch.Gatherer).Code == "G17", "Catalog3SaveStillValidates");
        }

        // Slice 2 task 2: bakery stock, recipe eligibility and gold purchases.
        private static CraftingState Know(params string[] ids) { var c = new CraftingState(); foreach (string i in ids) c.Learn(i); return c; }
        private static readonly string[] Conv = { "recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter", "recipe.food.cow_cheese" };
        private static readonly int[] R6 = { 1, 2, 3, 4, 5, 6 };

        // First visit whose bakery premium slot is a recipe offer (all tier-one sources met).
        private static ShopState RecipeStock(CraftingState c, out ShopOffer o)
        {
            for (int v = 1; v < 5000; v++)
            {
                var s = new ShopState { VisitId = v }; o = ShopService.Offers(s, ShopCatalog.Bakery, R6, c)[2];
                if (o.RecipeId != null) return s;
            }
            throw new Exception("no recipe offer in 5000 visits");
        }

        private static void BakeryStockChecks()
        {
            bool shape = true;
            for (int v = 1; v <= 200; v++)
            {
                var o = ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Bakery, new[] { 1, 2, 3, 4 }, new CraftingState());
                shape &= o[0].ItemId != o[1].ItemId && Entry(o[0].ItemId, ShopCatalog.Bakery).Role == ShopSlotRole.Common && Entry(o[1].ItemId, ShopCatalog.Bakery).Role == ShopSlotRole.Common
                    && o[0].Currency == Currency.Cash && o[1].Currency == Currency.Cash && Entry(o[2].ItemId, ShopCatalog.Bakery).Role == ShopSlotRole.Premium && o[2].Currency == Currency.Gold
                    && o[2].UnitPriceMinor == Entry(o[2].ItemId, ShopCatalog.Bakery).GoldPerServing && o[2].UnitsLeft == 1000 && o[2].RecipeId == null;
            }
            Check(shape, "BakeryRollShape");

            var met = Know("recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter");
            int chance = 0, none = 0; bool once = true;
            for (int v = 1; v <= 2000; v++)
            {
                var o = ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Bakery, R6, met);
                if (o[2].RecipeId != null) { chance++; once &= o[2].UnitsLeft == 1 && o[2].Currency == Currency.Gold && o[2].RecipeId == ShopCatalog.RecipeFor(o[2].ItemId) && o[2].UnitPriceMinor == Entry(o[2].ItemId, ShopCatalog.Bakery).GoldPerServing * 10; }
                if (ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Bakery, R6, new CraftingState())[2].RecipeId != null) none++;
            }
            Check(chance >= 60 && chance <= 140 && once, "RecipeChanceAboutFivePercent (" + chance + " of 2000)");
            Check(none == 0, "RecipeChanceAboutFivePercent (gate failing gives 0)");

            // id, regions, conversions the recipe's refined inputs need, the first input that nothing makes yet (null: all makeable)
            var table = new (string id, int[] regions, string[] conv, string missing)[] {
                ("premium_apple_pie", new[] { 1, 6 }, new[] { "recipe.material.wheat_flour", "recipe.food.cow_butter" }, null),
                ("strawberry_cream_roll", new[] { 4, 6 }, new[] { "recipe.material.wheat_flour", "recipe.food.cow_cream" }, null),
                ("honey_brioche", new[] { 6, 11 }, new[] { "recipe.material.wheat_flour", "recipe.food.cow_butter" }, null),
                ("blackberry_cream_pastry", new[] { 6, 7 }, new[] { "recipe.material.wheat_flour", "recipe.food.cow_cream" }, null),
                ("pumpkin_cow_cheese_galette", new[] { 6, 15 }, new[] { "recipe.material.wheat_flour", "recipe.food.cow_cheese", "recipe.food.cow_butter" }, null),
                ("mint_sugar_rolls", new[] { 6, 17, 18 }, Conv, "sugar"),
                ("vanilla_cacao_celebration_cake", new[] { 6, 17, 20, 22 }, Conv, "cacao_powder"),
                ("coconut_cream_tart", new[] { 6, 17, 24 }, Conv, "sugar") };
            bool gate = true; string why = "";
            foreach (var t in table)
            {
                string rid = "recipe.food." + t.id; int tier = RecipeBook.Find(rid).Tier; string reason;
                bool ok = BakeryGate.Eligible(rid, t.regions, tier, Know(t.conv), out reason);
                if (ok != (t.missing == null) || (t.missing != null && !reason.Contains(t.missing))) { gate = false; why += " all:" + t.id + "/" + reason; }
                if (BakeryGate.Eligible(rid, t.regions, tier - 1, Know(t.conv), out reason) || !reason.Contains("Tier")) { gate = false; why += " tier:" + t.id; }
                foreach (int r in t.regions)
                    if (BakeryGate.Eligible(rid, t.regions.Where(x => x != r).ToArray(), tier, Know(t.conv), out reason) || !reason.Contains("Region " + r)) { gate = false; why += " region" + r + ":" + t.id + "/" + reason; }
                if (t.missing == null)
                    foreach (string c in t.conv)
                        if (BakeryGate.Eligible(rid, t.regions, tier, Know(t.conv.Where(x => x != c).ToArray()), out reason) || !reason.Contains("Missing input")) { gate = false; why += " conv:" + c + ":" + t.id; }
                if (BakeryGate.Eligible(rid, t.regions, tier, Know(t.conv.Concat(new[] { rid }).ToArray()), out reason) || !reason.Contains("known")) { gate = false; why += " known:" + t.id; }
            }
            Check(gate, "GateEachSourceMissing" + why);

            // The gate reads only regions, tier and crafting: gold and a stocked bag cannot open it, even through PAY.
            var rich = new Wallet(0, 1000000); var stocked = new SpatialInventory(); string bagSnap = Snap(stocked);
            var fake = new ShopOffer { ItemId = "item.mint_sugar_rolls", RecipeId = "recipe.food.mint_sugar_rolls", UnitPriceMinor = 90, UnitsLeft = 1, Currency = Currency.Gold };
            var fs = new ShopState { VisitId = 1 }; fs.Stock[ShopCatalog.Bakery] = (1, new[] { new ShopOffer(), new ShopOffer(), fake });
            var allMet = Enumerable.Range(1, 24).ToArray(); var cf = Know(Conv);
            Check(!ShopService.TryBuyRecipe(fs, 2, 0, rich, cf, allMet, out why) && why == "This recipe isn't available yet" && rich.Gold == 1000000 && !cf.Knows(fake.RecipeId) && fake.UnitsLeft == 1 && Snap(stocked) == bagSnap, "GateIgnoresGoldAndInventory");

            // PAY paths on a rolled tier-one recipe offer.
            var know = Know("recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter");
            ShopState rs = RecipeStock(know, out ShopOffer ro); long price = ro.UnitPriceMinor; int rev = ro.Revision;
            var cl = Know("recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter", ro.RecipeId); var w1 = new Wallet(0, 1000);
            Check(!ShopService.TryBuyRecipe(rs, 2, rev, w1, cl, R6, out why) && why == "You already know this recipe" && w1.Gold == 1000 && ro.UnitsLeft == 1 && ro.Revision == rev, "RecipeLearnedBeforePayRefused");
            var lacking = Know("recipe.food.cow_cream");
            Check(!ShopService.TryBuyRecipe(rs, 2, rev, w1, lacking, R6, out why) && why == "This recipe isn't available yet" && w1.Gold == 1000 && ro.UnitsLeft == 1 && !lacking.Knows(ro.RecipeId), "IneligibleAtPayRefused");
            var wShort = new Wallet(0, price - 1);
            Check(!ShopService.TryBuyRecipe(rs, 2, rev, wShort, know, R6, out why) && why == "Not enough gold" && wShort.Gold == price - 1 && ro.UnitsLeft == 1 && ro.Revision == rev && !know.Knows(ro.RecipeId), "GoldExactAndShort (recipe short)");
            Check(!ShopService.TryBuyRecipe(rs, 2, rev + 1, new Wallet(0, price), know, R6, out why) && why == "The stock changed; check the offer again", "RecipeStaleRevisionRefused");
            ItemDefinition egg = Items.Find("ingredient.egg");
            var full = new SpatialInventory(1, 1); full.RestoreStored(full.AddWorld("f", egg, 5, 5, ItemPrices.Profile(egg, 5)), ItemOwner.Backpack, 0, 0); string fullSnap = Snap(full);
            var wExact = new Wallet(0, price);
            Check(ShopService.TryBuyRecipe(rs, 2, rev, wExact, know, R6, out why) && why == "" && wExact.Gold == 0 && know.Knows(ro.RecipeId) && know.UnitsOf(ro.RecipeId) == 0 && ro.UnitsLeft == 0 && ro.Revision == rev + 1 && Snap(full) == fullSnap, "RecipeIgnoresFullBag");
            Check(!ShopService.TryBuy(rs, ShopCatalog.Bakery, 2, ro.Revision, 1, new Wallet(0, 1000), full, () => "z", out why) && why == "Sold out" && Snap(full) == fullSnap, "RecipeOfferNotBoughtAsFood");

            // Gold foods: exact totals, and the stack discount only for exactly 1,000 units in one purchase.
            int[] r4 = { 1, 2, 3, 4 };
            var fo = Offers(new ShopState { VisitId = 1 }, ShopCatalog.Bakery, r4)[2]; long g = fo.UnitPriceMinor;
            Check(ShopService.GoldTotal(fo, 1) == g && ShopService.GoldTotal(fo, 999) == 999 * g && ShopService.GoldTotal(fo, 1000) == g * 900 && ShopService.GoldTotal(fo, 3) == 3 * g, "DiscountOnlyOnFullStack (totals)");
            int bn = 0; Func<string> bid = () => "gb" + (++bn);
            var gs = new ShopState { VisitId = 1 }; var go = Offers(gs, ShopCatalog.Bakery, r4)[2]; var gw = new Wallet(0, 1000 * g); var gb = new SpatialInventory();
            Check(ShopService.TryBuy(gs, ShopCatalog.Bakery, 2, go.Revision, 999, gw, gb, bid, out why) && gw.Gold == g && go.UnitsLeft == 1
                && ShopService.TryBuy(gs, ShopCatalog.Bakery, 2, go.Revision, 1, gw, gb, bid, out why) && gw.Gold == 0 && go.UnitsLeft == 0 && gb.Entries.Sum(e => e.Quantity) == 1000, "DiscountOnlyOnFullStack (999 then 1)");
            var ds = new ShopState { VisitId = 1 }; var dof = Offers(ds, ShopCatalog.Bakery, r4)[2]; var dw = new Wallet(0, 900 * g - 1); var db = new SpatialInventory(); string dSnap = Snap(db);
            Check(!ShopService.TryBuy(ds, ShopCatalog.Bakery, 2, dof.Revision, 1000, dw, db, bid, out why) && why == "Not enough gold" && dw.Gold == 900 * g - 1 && dof.UnitsLeft == 1000 && dof.Revision == 0 && Snap(db) == dSnap, "GoldExactAndShort (one short of discount)");
            dw.Credit(Currency.Gold, 1);
            Check(ShopService.TryBuy(ds, ShopCatalog.Bakery, 2, dof.Revision, 1000, dw, db, bid, out why) && dw.Gold == 0 && dof.UnitsLeft == 0 && dof.Revision == 1 && db.Entries.Sum(e => e.Quantity) == 1000, "GoldExactAndShort (exact discount)");
            var ps = new ShopState { VisitId = 1 }; var po = Offers(ps, ShopCatalog.Bakery, r4)[2]; var pw = new Wallet(0, 3 * g - 1); var pb = new SpatialInventory();
            Check(!ShopService.TryBuy(ps, ShopCatalog.Bakery, 2, 0, 3, pw, pb, bid, out why) && why == "Not enough gold" && pw.Gold == 3 * g - 1 && po.UnitsLeft == 1000 && pb.Entries.Count == 0, "GoldExactAndShort (short)");
            pw.Credit(Currency.Gold, 1);
            Check(ShopService.TryBuy(ps, ShopCatalog.Bakery, 2, 0, 3, pw, pb, bid, out why) && pw.Gold == 0 && po.UnitsLeft == 997 && pw.Cash == 0, "GoldExactAndShort (exact)");

            // A recipe is offered at most once per stock; a known recipe is never offered.
            bool twice = true, pieBack = false; var knowsPie = Know("recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter", "recipe.food.premium_apple_pie");
            for (int v = 1; v <= 1000; v++)
            {
                var ids = ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Bakery, R6, met).Where(x => x.RecipeId != null).Select(x => x.RecipeId).ToList();
                twice &= ids.Distinct().Count() == ids.Count && ids.Count <= 1;
                pieBack |= ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Bakery, R6, knowsPie).Any(x => x.RecipeId == "recipe.food.premium_apple_pie");
            }
            Check(twice && !pieBack, "RecipeNotOfferedTwice");
            RecipeSaveChecks();
        }

        // Slice 2 task 3: recipe offers, gold currency and gold purchase records in the save.
        private static void RecipeSaveChecks()
        {
            var know = Know("recipe.material.wheat_flour", "recipe.food.cow_cream", "recipe.food.cow_butter");
            ShopState rs = RecipeStock(know, out ShopOffer ro);
            ShopState back = SessionSaveStore.RestoreShops(Roundtrip(new SessionSnapshot { shops = SessionSaveStore.SaveShops(rs) }));
            ShopOffer bo = back.Stock[ShopCatalog.Bakery].Offers[2];
            Check(bo.RecipeId == ro.RecipeId && bo.Currency == Currency.Gold && bo.UnitsLeft == 1 && bo.UnitPriceMinor == ro.UnitPriceMinor && bo.ItemId == ro.ItemId, "RecipeOfferSurvivesReload");
            Check(ShopService.TryBuyRecipe(rs, 2, ro.Revision, new Wallet(0, ro.UnitPriceMinor), know, R6, out _), "RecipeOfferBought");
            ShopOffer sold = SessionSaveStore.RestoreShops(Roundtrip(new SessionSnapshot { shops = SessionSaveStore.SaveShops(rs) })).Stock[ShopCatalog.Bakery].Offers[2];
            Check(sold.RecipeId == ro.RecipeId && sold.UnitsLeft == 0 && sold.Revision == ro.Revision, "BoughtRecipeStaysSoldOut");

            // A slice 1 save has no currency/recipe fields; it restores as cash food offers (a gold food stays gold only when saved so).
            var one = new ShopState { VisitId = 3 }; foreach (string sh in Shops) Offers(one, sh, new[] { 1, 2, 3, 4 });
            SavedShops old = SessionSaveStore.SaveShops(one);
            foreach (var box in old.stock) foreach (var o in box.offers) { o.currency = null; o.recipe = null; }
            ShopState oldBack = SessionSaveStore.RestoreShops(Roundtrip(new SessionSnapshot { shops = old }));
            Check(oldBack.Stock.Count == one.Stock.Count && oldBack.Stock.Values.All(v => v.Offers.All(o => o.Currency == Currency.Cash && o.RecipeId == null)), "SliceOneSaveLoads");

            Func<Action<SavedShopOffer>, ShopState> tamper = t =>
            {
                SavedShops sv = SessionSaveStore.SaveShops(RecipeStock(know, out _)); t(sv.stock.Find(b => b.shopId == ShopCatalog.Bakery).offers[2]);
                return SessionSaveStore.RestoreShops(new SessionSnapshot { shops = sv });
            };
            Check(!tamper(o => o.recipe = "recipe.food.nope").Stock.ContainsKey(ShopCatalog.Bakery), "UnknownRecipeRerolls");
            Func<Action<SavedShopOffer>, bool> rejects = t => { try { tamper(t); return false; } catch (InvalidDataException) { return true; } };
            Check(rejects(o => o.unitsLeft = 2) && rejects(o => o.unitsLeft = 1000) && rejects(o => o.unitPriceMinor = 0) && rejects(o => o.unitPriceMinor = -1), "RecipeUnitsValid");
            Check(rejects(o => o.currency = "silver") && rejects(o => o.currency = "cash"), "InvalidRejected (currency)");

            var rec = new TradeRecord { utc = "2026-11-20T00:00:00Z", campId = "bakery", itemId = "food.apples", result = "Shop", quantity = 3, unitMinor = 0, totalMinor = 0, cash = 0, gold = -90 };
            var snap = new SessionSnapshot(); snap.warehouseLog.Add(rec);
            var rb = Roundtrip(snap).warehouseLog;
            Check(rb.Count == 1 && rb[0].gold == -90 && rb[0].cash == 0 && rb[0].quantity == 3 && new TradeRecord().gold == 0, "GoldRecordRoundTrips");
        }

        // ---------------------------------------------------------------- phone round R2: wallet labels and settlement list icons

        private static readonly string[] Counters = { "Farm", "Mine", "Tailor", "Ranch", "Fishery", "Bakery", "Warehouse", "Lumber" };
        internal const System.Reflection.BindingFlags AnyMember = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance;

        // Mean colour of an icon's opaque pixels and the widest channel spread among them, read from the PNG (sprites import unreadable).
        private static (Color mean, float spread) IconInk(string key)
        {
            var t = new Texture2D(2, 2);
            try
            {
                t.LoadImage(File.ReadAllBytes("Assets/_Game/Resources/UI/Icons/" + key + ".png"));
                Color sum = Color.clear; int n = 0; float spread = 0f;
                foreach (Color c in t.GetPixels())
                {
                    if (c.a < 0.5f) continue;
                    sum += c; n++; spread = Mathf.Max(spread, Mathf.Max(c.r, c.g, c.b) - Mathf.Min(c.r, c.g, c.b));
                }
                return (n > 0 ? sum / n : Color.clear, spread);
            }
            finally { UnityEngine.Object.DestroyImmediate(t); }
        }

        // The cash and gold icons load and are in colour: green notes, warm yellow gold.
        private static void WalletIconChecks()
        {
            foreach (string key in new[] { "cash", "gold" }) Check(Resources.Load<Sprite>("UI/Icons/" + key) != null, "WalletIconsExist " + key);
            var (cash, cashSpread) = IconInk("cash"); var (gold, goldSpread) = IconInk("gold");
            Check(cashSpread > 0.2f && cash.g > cash.r + 0.1f && cash.g > cash.b + 0.1f, "WalletIconsExist (cash is green, not grayscale) " + cash + " spread " + cashSpread);
            Check(goldSpread > 0.2f && gold.r > gold.b + 0.3f && gold.g > gold.b + 0.15f, "WalletIconsExist (gold is warm yellow, not grayscale) " + gold + " spread " + goldSpread);
        }

        internal static object Member(object o, string name) => o.GetType().GetField(name, AnyMember).GetValue(o);
        private static RectTransform OneChild(RectTransform hud, string name)
        {
            RectTransform[] found = hud.GetComponentsInChildren<RectTransform>(true).Where(r => r.name == name).ToArray();
            return found.Length == 1 ? found[0] : null;
        }
        // A label: one paper strip, the icon's silhouette on the left and one amount text right of it.
        private static Text Amount(RectTransform hud, string name, string icon)
        {
            RectTransform label = OneChild(hud, name);
            if (label == null) return null;
            Transform silhouette = label.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Silhouette");
            Text[] texts = label.GetComponentsInChildren<Text>(true);
            bool ok = label.GetComponentsInChildren<UnityEngine.UI.Image>(true).Any(i => i.name == "Paper") && silhouette != null && silhouette.GetComponent<UnityEngine.UI.Image>().sprite != null
                && silhouette.GetComponent<UnityEngine.UI.Image>().sprite.name == icon && texts.Length == 1 && ((RectTransform)silhouette.parent).anchorMax.x <= ((RectTransform)texts[0].transform).anchorMin.x;
            return ok ? texts[0] : null;
        }

        // The counter HUDs are built on an edit-mode CollectionPrototype (its Awake never runs): each carries both labels and no
        // old wallet line, and the amounts follow the wallet after a cash purchase, a gold purchase and a warehouse payout.
        private static void WalletLabelChecks()
        {
            string[] gone = { "shopWalletText", "ShopWalletLine", "warehouseWalletText", "lumberWalletText" };
            string left = string.Join(", ", gone.Where(n => typeof(CollectionPrototype).GetMember(n, AnyMember).Length > 0));
            Check(left.Length == 0, "WalletLabelsOnEveryCounter (the old wallet lines are gone" + (left.Length > 0 ? "; still there: " + left : "") + ")");
            var root = new GameObject("wallet label check", typeof(RectTransform));
            try
            {
                var proto = root.AddComponent<CollectionPrototype>();
                var safe = new GameObject("Safe area", typeof(RectTransform)).GetComponent<RectTransform>(); safe.SetParent(root.transform, false);
                var travel = TheTrailWalker.Domain.Travel.TravelState.Start(TheTrailWalker.Domain.Travel.TravelCatalog.Prototype());
                typeof(CollectionPrototype).GetField("safe", AnyMember).SetValue(proto, safe);
                typeof(CollectionPrototype).GetField("travel", AnyMember).SetValue(proto, travel);
                foreach (string create in new[] { "CreateShopHud", "CreateWarehouseHud", "CreateLumberHud" }) typeof(CollectionPrototype).GetMethod(create, AnyMember).Invoke(proto, null);
                var refresh = (Action)Delegate.CreateDelegate(typeof(Action), proto, typeof(CollectionPrototype).GetMethod("RefreshWalletLabels", AnyMember));
                var update = (Action)Delegate.CreateDelegate(typeof(Action), proto, typeof(CollectionPrototype).GetMethod("UpdateWalletLabels", AnyMember));
                Func<string, RectTransform> hudOf = c => (RectTransform)Member(proto, c == "Warehouse" ? "warehouseHud" : c == "Lumber" ? "lumberHud" : "shopHud");
                refresh();
                foreach (string counter in Counters)
                {
                    RectTransform hud = hudOf(counter);
                    Check(Amount(hud, "Cash label", "cash") != null && Amount(hud, "Gold label", "gold") != null, "WalletLabelsOnEveryCounter " + counter + " (both labels, icon left of the amount)");
                    Check(!hud.GetComponentsInChildren<Text>(true).Any(t => t.text.StartsWith("Cash", StringComparison.Ordinal) || t.text.StartsWith("Gold", StringComparison.Ordinal)), "WalletLabelsOnEveryCounter " + counter + " (no old text line)");
                }
                Func<string, bool> shows = what => new[] { "Farm", "Warehouse", "Lumber" }.All(c =>
                    Amount(hudOf(c), "Cash label", "cash").text == Money.Cash(travel.Cash) && Amount(hudOf(c), "Gold label", "gold").text == Money.Cash(travel.Gold));
                var when = new DateTime(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc); var log = new List<TradeRecord>(); var r4 = new[] { 1, 2, 3, 4 };
                travel.Wallet.Credit(Currency.Cash, 123456789); travel.Wallet.Credit(Currency.Gold, 1000000);
                var shops = new ShopState { VisitId = 1 };
                long cashBefore = travel.Cash; ShopOffer fo = Offers(shops, ShopCatalog.Farm, r4)[0];
                Check(CollectionPrototype.BuyAndRecord(shops, ShopCatalog.Farm, 0, fo.Revision, 10, travel.Wallet, new SpatialInventory(), new CraftingState(), r4, () => "wl1", log, when, out string why) && travel.Cash < cashBefore, "WalletLabelText (cash purchase setup) " + why);
                refresh();
                Check(shows("cash"), "WalletLabelText after a cash purchase: " + Money.Cash(travel.Cash));
                ShopOffer go = Offers(shops, ShopCatalog.Bakery, r4)[2]; long goldBefore = travel.Gold;
                Check(go.Currency == Currency.Gold && CollectionPrototype.BuyAndRecord(shops, ShopCatalog.Bakery, 2, go.Revision, 1, travel.Wallet, new SpatialInventory(), new CraftingState(), r4, () => "wl2", log, when, out why) && travel.Gold < goldBefore, "WalletLabelText (gold purchase setup) " + why);
                refresh();
                Check(shows("gold"), "WalletLabelText after a gold purchase: " + Money.Cash(travel.Gold));
                // A completed crate pays out; the per-frame sync picks the change up without an explicit refresh.
                var bag = new SpatialInventory(8000, 4000); ItemDefinition pine = Items.Find("material.pine_log");
                ItemEntry stack = bag.AddWorld("logs", pine, 10, 1, ItemPrices.Profile(pine, 1));
                if (!bag.RestoreStored(stack, ItemOwner.Backpack, 0, 0)) throw new InvalidOperationException("Test fixture: logs");
                cashBefore = travel.Cash;
                Check(WarehouseService.Deposit(bag, new WarehouseState(), travel.Wallet, log, stack, -1, 0, () => "wl3", when, out WarehouseDeposit d, out why) && d.CrateCompleted && travel.Cash > cashBefore, "WalletLabelText (payout setup) " + why);
                update();
                Check(shows("payout"), "WalletLabelText after a warehouse payout: " + Money.Cash(travel.Cash));
                update(); long allocated = GC.GetAllocatedBytesForCurrentThread();
                for (int n = 0; n < 100; n++) update();
                Check(GC.GetAllocatedBytesForCurrentThread() - allocated == 0, "WalletLabelsAllocateNothing (an unchanged wallet)");
                Check(new[] { "Farm", "Warehouse", "Lumber" }.All(c => OneChild(hudOf(c), "Cash label").IsChildOf(hudOf(c)) && OneChild(hudOf(c), "Gold label").IsChildOf(hudOf(c))), "WalletLabelsHideWithTheHud (children of each counter HUD)");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        // Everything visible at each counter, at both sizes (TagArea's insets as the safe area), as screen rects built from the game's
        // own anchors, framings and layout rules: nothing overlaps anything else. The cash label sits centred above the bag, the
        // gold label centred in the top band the hidden title leaves (the title and status step aside: CounterChromeChecks).
        // The transient refusal note and the purchase/ledger sheets are overlays above everything and are not part of the layout.
        private static void CounterLayoutChecks()
        {
            Func<string, float> constant = n => Convert.ToSingle(typeof(CollectionPrototype).GetField(n, AnyMember).GetValue(null));
            float bagLift = constant("BagLift"), barGap = constant("BarLineGap"), columns = constant("VisibleColumns"), square = constant("HudSquare"), margin = constant("HudMargin"), gap = constant("HudGap");
            string longest = PrototypeItems.All.Concat(HarvestTierItems.All).Concat(ShopItems.All).Select(d => d.Name).OrderByDescending(n => n.Length).First() + " 10/10";
            foreach (var (screen, dpi) in new[] { (new Vector2(1116, 2480), 401f), (new Vector2(720, 1680), 320f) })
            {
                float scale = Mathf.Sqrt(screen.x / 720f * screen.y / 1280f), dp = dpi / 160f;
                Rect safe = Rect.MinMaxRect(16f * dp, 24f * dp, screen.x - 16f * dp, screen.y - 48f * dp);
                float sw = safe.width / scale, sh = safe.height / scale;
                Func<Rect, Rect> px = r => new Rect(safe.x + r.x * scale, safe.y + r.y * scale, r.width * scale, r.height * scale);
                Func<Rect, Rect> band = r => Rect.MinMaxRect(safe.x + r.xMin * safe.width, safe.y + r.yMin * safe.height, safe.x + r.xMax * safe.width, safe.y + r.yMax * safe.height);
                Rect gold = px(CollectionPrototype.WalletGoldRect(sw, sh));
                foreach (string name in Counters)
                {
                    var holder = new GameObject("counter layout " + name).transform; holder.position = MovementPrototype.InteriorWorldPosition;
                    var cam = new GameObject("counter layout camera").AddComponent<Camera>();
                    var models = new List<GameObject>();
                    try
                    {
                        Transform counter = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Settlement/Storefronts/" + name + "Counter"), holder, false).transform;
                        cam.fieldOfView = StreetCamera.FieldOfView; cam.aspect = screen.x / screen.y;
                        // The debug HUD that stays on at a counter (CollectionPrototype.CreateHud's anchors); the title, status and the four
                        // mid-screen debug buttons are hidden there (CounterChromeChecks).
                        var seen = new List<(string what, Rect rect)> { ("feedback line", band(CollectionPrototype.FeedbackBand)), ("RESET SAVE", band(CollectionPrototype.ResetSaveBand)) };
                        float barLine = barGap, bagTop = 0.035f;   // closed bag: the opening at the bottom, the squares' bar line above it (LayoutBackpack)
                        if (name == "Lumber")
                        {
                            Pose pose = CollectionPrototype.LumberCounterPose(cam, counter); cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                            foreach (string part in new[] { "Ledger", "DoorPivot" }) seen.Add((part, MeshScreenRect(cam, counter.Find(part), screen)));
                            seen.Add(("WOOD T2-T4", band(CollectionPrototype.LumberWoodButtonBand)));
                        }
                        else if (name == "Warehouse")
                        {
                            const int rows = 4;   // every backpack is 4 rows (EquipmentState.BackpackHeight)
                            float cell = Mathf.Min(sw / columns, sh * 0.45f / rows), top = bagLift + cell * rows / sh;   // LayoutBag
                            barLine = top + barGap; bagTop = top + 0.035f;
                            var crates = new Bounds[2]; var crateObjects = new Transform[2];
                            for (int i = 0; i < 2; i++)
                            {
                                var crate = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Settlement/Storefronts/DepositCrate"), holder, false);
                                crate.transform.position = counter.Find("OfferAnchor" + (i + 1)).position; crates[i] = RendererBounds(crate.transform); crateObjects[i] = crate.transform;
                            }
                            Transform eye = counter.Find("CustomerCameraAnchor"); cam.transform.SetPositionAndRotation(eye.position, eye.rotation);
                            Pose pose = CollectionPrototype.WarehouseCameraPose(cam, crates, (safe.y + top * safe.height) / screen.y, CollectionPrototype.WarehouseFramingTop(safe.y, safe.height) / screen.y);   // OpenWarehouse's framing
                            cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                            var labelTops = new Vector3[2];
                            for (int i = 0; i < 2; i++) labelTops[i] = cam.WorldToViewportPoint(new Vector3(crates[i].center.x, crates[i].max.y, crates[i].center.z));
                            for (int i = 0; i < 2; i++)
                            {
                                seen.Add(("crate " + (i + 1), MeshScreenRect(cam, crateObjects[i], screen)));
                                Vector3 v = labelTops[i];
                                CollectionPrototype.WarehouseLabelSpan(v.x * screen.x, labelTops[1 - i].x * screen.x, CollectionPrototype.WarehouseLabelWidth(longest) * safe.width, screen.y * 0.01f, safe.xMin, safe.xMax, out float lx, out float w);   // UpdateWarehouse
                                seen.Add(("crate label " + (i + 1), new Rect(lx, v.y * screen.y + screen.y * 0.01f, w, 56f * scale)));
                            }
                            seen.Add(("cycle line", band(Rect.MinMaxRect(0.03f, CollectionPrototype.WarehouseCycleBand.x, 0.97f, CollectionPrototype.WarehouseCycleBand.y))));
                            seen.Add(("warehouse feedback line", band(Rect.MinMaxRect(0.03f, CollectionPrototype.WarehouseFeedbackBand.x, 0.97f, CollectionPrototype.WarehouseFeedbackBand.y))));
                        }
                        else
                        {
                            Pose pose = CollectionPrototype.ShopCounterPose(cam, counter); cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                            var offers = Offers(new ShopState { VisitId = 1 }, CollectionPrototype.ShopIdForKey(name.ToLowerInvariant()), new[] { 1, 2, 3, 4 });
                            var tops = new Vector2[3]; var chars = new int[3]; var modelRects = new Rect[3];
                            for (int i = 0; i < 3; i++)
                            {
                                Transform anchor = counter.Find("OfferAnchor" + (i + 1));
                                foreach (Transform baked in anchor) baked.gameObject.SetActive(false);
                                ItemDefinition def = Items.Find(offers[i].ItemId);
                                GameObject model = CollectionPrototype.PlaceOfferModel(anchor, def); models.Add(model);
                                Bounds mb = RendererBounds(model.transform); mb.Encapsulate(anchor.position); modelRects[i] = ScreenRect(cam, mb, screen);   // UpdateShops' layout input
                                seen.Add(("offer " + (i + 1), MeshScreenRect(cam, model.transform, screen)));
                                tops[i] = Vector2.Scale(cam.WorldToViewportPoint(new Vector3(anchor.position.x, mb.max.y, anchor.position.z)), screen);
                                chars[i] = CollectionPrototype.LongestLine(CollectionPrototype.OfferTagText(offers[i], def.Name));
                            }
                            var tags = new Rect[3]; Rect area = TagArea(screen, out float h, out float tagGap);
                            CollectionPrototype.ShopTagLayout(tops, chars, area, h, tagGap, tags, modelRects);
                            for (int i = 0; i < 3; i++) seen.Add(("price tag " + (i + 1), tags[i]));
                        }
                        seen.Add(bagTop > 0.05f ? ("open bag", band(Rect.MinMaxRect(0f, bagLift, 1f, bagTop))) : ("bag opening", band(Rect.MinMaxRect(0f, 0f, 1f, bagTop))));
                        seen.Add(("Return square", px(new Rect(margin, barLine * sh, square, square))));   // PlaceSquare(screenReturn, left, barLine, column 0, row 0)
                        // PlaceSquare(missionTrackerRect, left, barLine, column 0, row 1) with its model and count overhang (CreateMissionTracker).
                        seen.Add(("mission tracker", px(new Rect(margin - 0.05f * square, barLine * sh + square + gap - 0.06f * square, 1.1f * square, 1.16f * square))));
                        Rect cash = px(CollectionPrototype.WalletCashRect(sw, sh, barLine));
                        seen.Add(("cash label", cash)); seen.Add(("gold label", gold));
                        string at = " " + name + " at " + screen.x + "x" + screen.y;
                        Check(px(new Rect(sw * 0.5f - 42f, barLine * sh, 84f, 84f)).Overlaps(cash), "CounterLayout overlap test can fail" + at);
                        // Two counter models (offers, crates, ledger, door) may overlap in perspective: that is depth in the scene, not layout.
                        Func<string, bool> model3d = w => w.StartsWith("offer ", StringComparison.Ordinal) || w.StartsWith("crate ", StringComparison.Ordinal) && !w.StartsWith("crate label", StringComparison.Ordinal) || w == "Ledger" || w == "DoorPivot";
                        var clashes = new List<string>();
                        for (int i = 0; i < seen.Count; i++)
                            for (int j = i + 1; j < seen.Count; j++)
                                if (!(model3d(seen[i].what) && model3d(seen[j].what)) && seen[i].rect.Overlaps(seen[j].rect)) clashes.Add(seen[i].what + " " + seen[i].rect.ToString("0") + " x " + seen[j].what + " " + seen[j].rect.ToString("0"));
                        Check(clashes.Count == 0, "CounterLayout nothing visible overlaps" + at + (clashes.Count > 0 ? ": " + string.Join("; ", clashes) : ""));
                        Check(Mathf.Abs(cash.center.x - safe.center.x) < 0.5f && Mathf.Abs(gold.center.x - safe.center.x) < 0.5f, "WalletLabelPlacement centred" + at);
                        Check(cash.yMin >= safe.y + bagTop * safe.height && cash.yMax <= safe.y + 0.62f * safe.height, "WalletLabelPlacement cash in the lower band, above the bag" + at + ": " + cash.ToString("0"));
                        // The gold label holds the band the hidden title leaves: under the safe area's top edge, above the title's bottom line.
                        Check(Mathf.Abs(gold.yMax - (safe.yMax - CollectionPrototype.WalletTopGap * scale)) < 0.5f && gold.yMin >= safe.y + CollectionPrototype.TitleBand.yMin * safe.height
                            && gold.xMin >= safe.xMin && gold.xMax <= safe.xMax, "WalletLabelPlacement gold in the title band inside the safe area" + at + ": " + gold.ToString("0"));
                    }
                    finally
                    {
                        foreach (GameObject m in models) if (m != null) UnityEngine.Object.DestroyImmediate(m);
                        UnityEngine.Object.DestroyImmediate(cam.gameObject); UnityEngine.Object.DestroyImmediate(holder.gameObject);
                    }
                }
            }
        }

        // A model's on-screen extent in pixels (y up): its mesh vertices projected, tighter than its bounding box's corners.
        internal static Rect MeshScreenRect(Camera cam, Transform model, Vector2 screen)
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            foreach (MeshFilter f in model.GetComponentsInChildren<MeshFilter>())
            {
                if (f.sharedMesh == null || !f.GetComponent<Renderer>() || !f.GetComponent<Renderer>().enabled || !f.gameObject.activeInHierarchy) continue;
                foreach (Vector3 v in f.sharedMesh.vertices)
                {
                    Vector3 p = cam.WorldToViewportPoint(f.transform.TransformPoint(v));
                    x0 = Mathf.Min(x0, p.x * screen.x); x1 = Mathf.Max(x1, p.x * screen.x); y0 = Mathf.Min(y0, p.y * screen.y); y1 = Mathf.Max(y1, p.y * screen.y);
                }
            }
            return x0 <= x1 ? Rect.MinMaxRect(x0, y0, x1, y1) : new Rect(0f, 0f, 0f, 0f);
        }

        // The prototype title, status and four mid-screen debug buttons step aside while a counter is open (each Open* calls SetCounterChrome(true)) and come back
        // through every teardown (TearDownShop, TearDownWarehouse, TearDownLumber: Return, ResetHomeView, travel, faint).
        private static void CounterChromeChecks()
        {
            foreach (string file in new[] { "Shops", "Warehouse", "Lumber" })
                Check(File.ReadAllText("Assets/_Game/Scripts/Unity/CollectionPrototype." + file + ".cs").Contains("SetCounterChrome(true)"), "CounterChrome the " + file + " counter hides the title and status when it opens");
            var root = new GameObject("counter chrome check", typeof(RectTransform));
            try
            {
                var proto = root.AddComponent<CollectionPrototype>();
                var safe = new GameObject("Safe area", typeof(RectTransform)).GetComponent<RectTransform>(); safe.SetParent(root.transform, false);
                Func<string, Text> line = n => { var t = new GameObject(n, typeof(RectTransform)).AddComponent<Text>(); t.transform.SetParent(safe, false); return t; };
                Text title = line("title"), status = line("status");
                Action<string, object> set = (n, v) => typeof(CollectionPrototype).GetField(n, AnyMember).SetValue(proto, v);
                set("safe", safe); set("travel", TheTrailWalker.Domain.Travel.TravelState.Start(TheTrailWalker.Domain.Travel.TravelCatalog.Prototype()));
                set("ledger", new SpatialInventory()); set("titleLabel", title); set("status", status);
                var buttons = (List<GameObject>)typeof(CollectionPrototype).GetField("counterHiddenButtons", AnyMember).GetValue(proto);
                foreach (string b in new[] { "UNLOCK CAMPS", "LEARN T1 RECIPES", "AUTO SORT", "CLAIM OVERFLOW" }) { var go = new GameObject(b, typeof(RectTransform)); go.transform.SetParent(safe, false); buttons.Add(go); }
                Func<bool> anyShown = () => title.gameObject.activeSelf || status.gameObject.activeSelf || buttons.Any(g => g.activeSelf);
                Func<bool> allShown = () => title.gameObject.activeSelf && status.gameObject.activeSelf && buttons.All(g => g.activeSelf);
                foreach (string create in new[] { "CreateShopHud", "CreateWarehouseHud", "CreateLumberHud" }) typeof(CollectionPrototype).GetMethod(create, AnyMember).Invoke(proto, null);
                var chrome = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), proto, typeof(CollectionPrototype).GetMethod("SetCounterChrome", AnyMember));
                foreach (var (holder, teardown) in new[] { ("shopHolder", "TearDownShop"), ("warehouseHolder", "TearDownWarehouse"), ("lumberHolder", "TearDownLumber") })
                {
                    chrome(true);
                    Check(!anyShown(), "CounterChrome title, status and the four debug buttons hidden while open (" + teardown + ")");
                    set(holder, new GameObject("counter holder").transform);
                    typeof(CollectionPrototype).GetMethod(teardown, AnyMember).Invoke(proto, null);
                    Check(allShown() && typeof(CollectionPrototype).GetField(holder, AnyMember).GetValue(proto) == null, "CounterChrome title, status and the four debug buttons back after " + teardown);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        // Every settlement list entry gets a black/grayscale icon through the list's own path (Button + HudIconStyle.Apply): it loads,
        // sits inside its row at both sizes, and the caption stays on the label as its accessibility name. A missing icon keeps the caption.
        private static void SettlementListIconChecks()
        {
            System.Reflection.MethodInfo button = typeof(CollectionPrototype).GetMethod("Button", AnyMember, null,
                new[] { typeof(Transform), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(UnityEngine.Events.UnityAction) }, null);
            Func<Transform, string, Text> row = (parent, caption) => (Text)button.Invoke(null, new object[] { parent, caption, 0f, 0f, 0f, 0f, (UnityEngine.Events.UnityAction)(() => { }) });
            string[] keys = CollectionPrototype.SettlementListKeys(true);
            foreach (string key in keys)
            {
                Check(Resources.Load<Sprite>("UI/Icons/" + key) != null, "SettlementListIcons " + key + " loads");
                var (mean, spread) = IconInk(key);
                Check(spread <= 0.03f, "SettlementListIcons " + key + " is black/grayscale (channel spread " + spread.ToString("0.000") + ", mean " + mean + ")");
            }
            var root = new GameObject("list icon check", typeof(RectTransform));
            try
            {
                foreach (var (w, h, dpi) in new[] { (1116f, 2480f, 401f), (720f, 1680f, 320f) })
                {
                    float scale = Mathf.Sqrt(w / 720f * h / 1280f);
                    CollectionPrototype.SettlementListRows(h / scale, keys.Length, out float size, out float step, 48f * dpi / 160f / scale);
                    foreach (string key in keys)
                    {
                        Text label = row(root.transform, key);
                        var rect = (RectTransform)label.transform.parent; rect.sizeDelta = new Vector2(size, size);
                        HudIconStyle.Apply(label, key);
                        var icon = (RectTransform)rect.Find("Silhouette");
                        var r = new Vector3[4]; var ic = new Vector3[4]; rect.GetWorldCorners(r);
                        bool inside = icon != null && icon.GetComponent<UnityEngine.UI.Image>().sprite != null;
                        if (inside) { icon.GetWorldCorners(ic); inside = ic[0].x >= r[0].x && ic[0].y >= r[0].y && ic[2].x <= r[2].x && ic[2].y <= r[2].y; }
                        Check(inside && !label.enabled && label.text == key, "SettlementListIcons " + key + " fits its " + size.ToString("0.0") + " row at " + w + "x" + h + " (caption kept as the label's name, not drawn over the icon)");
                    }
                }
                Text missing = row(root.transform, "Nowhere");
                HudIconStyle.Apply(missing, "no_such_icon");
                Check(missing.enabled && missing.transform.parent.Find("Silhouette") == null, "SettlementListIcons a missing icon keeps the caption");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        // ---------------------------------------------------------------- phone round R3: the settlement list at camps

        // An edit-mode CollectionPrototype seated at the first camp with the camp HUD built (CreateTravelHud) and a CLAIM OVERFLOW button at
        // its CreateHud spot. Awake never runs and `initialized` stays false, so SaveNow is a no-op: nothing here touches the real save.
        internal static CollectionPrototype CampHarness(GameObject root, TheTrailWalker.Domain.Travel.SettlementState settlement)
        {
            var proto = root.AddComponent<CollectionPrototype>();
            var safe = new GameObject("Safe area", typeof(RectTransform)).GetComponent<RectTransform>(); safe.SetParent(root.transform, false); safe.sizeDelta = new Vector2(598f, 1431f);
            var feedback = new GameObject("Feedback", typeof(RectTransform)).AddComponent<Text>(); feedback.transform.SetParent(safe, false);
            var claim = new GameObject("CLAIM OVERFLOW", typeof(RectTransform)).GetComponent<RectTransform>(); claim.SetParent(safe, false);
            claim.anchorMin = CollectionPrototype.ClaimOverflowBand.min; claim.anchorMax = CollectionPrototype.ClaimOverflowBand.max;
            var catalog = TheTrailWalker.Domain.Travel.TravelCatalog.Prototype(); var travel = TheTrailWalker.Domain.Travel.TravelState.Start(catalog);
            Action<string, object> set = (n, v) => typeof(CollectionPrototype).GetField(n, AnyMember).SetValue(proto, v);
            set("safe", safe); set("feedback", feedback); set("travel", travel); set("settlement", settlement); set("ledger", new SpatialInventory());
            set("movement", new TheTrailWalker.Domain.MovementState()); set("router", root.AddComponent<TheTrailWalker.Input.GestureRouter>());
            set("scene", new TravelSceneInfo { Catalog = catalog, Segment = travel.CurrentSegment });
            SetPhase(proto, "Seated");
            Call(proto, "CreateTravelHud");
            return proto;
        }
        internal static void SetPhase(CollectionPrototype proto, string phase) { var f = typeof(CollectionPrototype).GetField("phase", AnyMember); f.SetValue(proto, Enum.Parse(f.FieldType, phase)); }
        internal static object Call(object o, string method, params object[] args) => typeof(CollectionPrototype).GetMethod(method, AnyMember).Invoke(o, args);

        // The settlement list at a seated camp (phone round R3): the settlement's places minus Return, the camp's visibility and covers,
        // a tap that travels by the camp path and opens the place after the reload, and a layout clear of every camp HUD element.
        private static void CampListChecks()
        {
            string[] camp = CollectionPrototype.CampListKeys(true);
            Check(camp.SequenceEqual(CollectionPrototype.SettlementListKeys(true).Where(k => k != "return")) && camp.Length == 9 && CollectionPrototype.CampListKeys(false).SequenceEqual(new[] { "settlement" }),
                "CampListKeys: a chosen plot gives the settlement's nine places in its order without Return, no plot a single Settlement entry (" + string.Join(",", camp) + ")");
            Check(CollectionPrototype.CampListVisible(true, true, false, false) && !CollectionPrototype.CampListVisible(false, true, false, false) && !CollectionPrototype.CampListVisible(true, false, false, false)
                && !CollectionPrototype.CampListVisible(true, true, true, false) && !CollectionPrototype.CampListVisible(true, true, false, true), "CampListVisibility rule: discovered, seated, not in the settlement, not covered");
            var root = new GameObject("camp list check", typeof(RectTransform));
            System.Reflection.FieldInfo welcome = typeof(CollectionPrototype).GetField("settlementWelcomePending", AnyMember);
            Func<string> take = () => (string)Call(null, "TakeSettlementOpen");
            try
            {
                var settlement = TheTrailWalker.Domain.Travel.SettlementState.New(7);
                CollectionPrototype proto = CampHarness(root, settlement);
                Action<string, object> set = (n, v) => typeof(CollectionPrototype).GetField(n, AnyMember).SetValue(proto, v);
                Action update = () => Call(proto, "UpdateCampList");
                Func<RectTransform> list = () => (RectTransform)Member(proto, "settlementList");
                Func<bool> shown = () => list() != null && list().gameObject.activeSelf;
                Func<RectTransform[]> rowRects = () => list() == null ? new RectTransform[0] : list().Cast<Transform>().Select(t => (RectTransform)t)
                    .Where(r => r.gameObject.activeSelf && r.name.StartsWith("Settlement list: ", StringComparison.Ordinal)).OrderByDescending(r => r.anchoredPosition.y).ToArray();
                Func<string> rows = () => string.Join(",", rowRects().Select(r => r.name.Substring(17)));
                update(); Check(!shown(), "CampListVisibility hidden while the settlement is undiscovered");
                settlement.Discover(); update(); Check(shown(), "CampListVisibility shown seated at a camp with the settlement discovered");
                var map = (GameObject)Member(proto, "mapPanel"); map.SetActive(true); update(); Check(!shown(), "CampListVisibility hidden with the map open"); map.SetActive(false);
                SetPhase(proto, "Walking"); update(); Check(!shown(), "CampListVisibility hidden while walking");
                SetPhase(proto, "Seated"); update(); Check(shown(), "CampListVisibility back once seated again");
                // The settlement square opens the list downward and closes the user menu (one right-hand column, never both at once).
                set("menuOpen", true);
                list().Find("Settlement menu").GetComponent<UnityEngine.UI.Button>().onClick.Invoke(); update();
                Transform houseIcon = rowRects().FirstOrDefault()?.Find("Silhouette");
                Check(rows() == "Settlement" && houseIcon != null && houseIcon.GetComponent<UnityEngine.UI.Image>().sprite != null && houseIcon.GetComponent<UnityEngine.UI.Image>().sprite.name == "home" && !(bool)Member(proto, "menuOpen"),
                    "CampListKeys before a plot: one Settlement row with the house icon, and opening the list closes the user menu (" + rows() + ")");
                var travel = (TheTrailWalker.Domain.Travel.TravelState)Member(proto, "travel");
                settlement.ChoosePlot(1, travel.DiscoveredRegions); update();
                Check(rows() == "Home,Warehouse,Lumber,Bakery,Farm,Mine,Tailor,Ranch,Fishery", "CampListKeys with a plot: the nine places top to bottom, no Return (" + rows() + ")");
                set("menuOpen", true); update();
                Check(rows() == "" && !(bool)Member(proto, "settlementListOpen"), "CampListLayout opening the user menu closes the list");
                set("menuOpen", false); set("settlementListOpen", true); update();

                // A tap travels by the camp path (busy rules, no toll, arrival, visit advance) and carries the place across the reload once.
                var shops = (ShopState)Member(proto, "shops"); int visit = shops.VisitId; long cash = travel.Cash, gold = travel.Gold;
                set("saveBlocked", true); Call(proto, "SettlementListTapped", "farm");
                Check(!travel.InSettlement && take() == null && !(bool)welcome.GetValue(null), "CampListTravelsAndOpens a refused trip (busy) carries nothing");
                set("saveBlocked", false); Call(proto, "SettlementListTapped", "farm");
                Check(travel.InSettlement && Member(proto, "phase").ToString() == "Transition" && shops.VisitId == visit + 1 && travel.Cash == cash && travel.Gold == gold,
                    "CampListTravelsAndOpens a Farm tap travels to the settlement by the camp path (no toll, the visit advances)");
                string carried = take();
                Check(carried == "farm" && take() == null && CollectionPrototype.ShopIdForKey(carried) == "shop.farm" && !(bool)welcome.GetValue(null),
                    "CampListTravelsAndOpens the farm crosses the reload once and opens shop.farm in place of the welcome note (carried " + (carried ?? "nothing") + ")");
                Check(CollectionPrototype.SettlementOpensOnEntry(false, false, false) && !CollectionPrototype.SettlementOpensOnEntry(true, false, false)
                    && !CollectionPrototype.SettlementOpensOnEntry(false, true, false) && !CollectionPrototype.SettlementOpensOnEntry(false, false, true),
                    "CampListTravelsAndOpens a showing note, the plot choice or a covering overlay keeps the player on the street");
                Check(typeof(CollectionPrototype).GetField("settlementOpenPending", AnyMember).IsStatic && File.ReadAllText("Assets/_Game/Scripts/Unity/CollectionPrototype.Settlement.cs").Contains("settlementOpenNext = TakeSettlementOpen();"),
                    "CampListTravelsAndOpens the place is a static handoff (never in the save) taken once by the settlement entry, so a later reload never re-opens it");
                // Before a plot the single entry lands on the street with the welcome note and the plot choice.
                var other = TheTrailWalker.Domain.Travel.SettlementState.New(8); other.Discover();
                set("settlement", other); set("travel", TheTrailWalker.Domain.Travel.TravelState.Start(TheTrailWalker.Domain.Travel.TravelCatalog.Prototype())); SetPhase(proto, "Seated");
                Call(proto, "SettlementListTapped", "settlement");
                Check(((TheTrailWalker.Domain.Travel.TravelState)Member(proto, "travel")).InSettlement && take() == null && (bool)welcome.GetValue(null),
                    "CampListTravelsAndOpens the Settlement entry travels and lands on the street with the welcome note");
            }
            finally { welcome.SetValue(null, false); take(); UnityEngine.Object.DestroyImmediate(root); }
            CampListLayoutChecks();
        }

        // Every camp HUD element visible while seated (closed bag), at both sizes with CounterLayoutChecks' safe-area insets, in two
        // states: the settlement list open, and the user menu column open instead (they exclude each other). No pair overlaps.
        private static void CampListLayoutChecks()
        {
            Func<string, float> constant = n => Convert.ToSingle(typeof(CollectionPrototype).GetField(n, AnyMember).GetValue(null));
            float square = constant("HudSquare"), margin = constant("HudMargin"), gap = constant("HudGap"), barLine = constant("ClosedBarLine"), listTop = constant("SettlementListTop");
            var root = new GameObject("camp layout check", typeof(RectTransform));
            try
            {
                var settlement = TheTrailWalker.Domain.Travel.SettlementState.New(7); settlement.Discover(); settlement.ChoosePlot(1, new[] { "region.01" });
                CollectionPrototype proto = CampHarness(root, settlement);
                Call(proto, "UpdateCampList");
                var hudRoot = (RectTransform)Member(proto, "safe");
                Func<string, Rect> anchors = name => { var r = (RectTransform)hudRoot.Find(name); return Rect.MinMaxRect(r.anchorMin.x, r.anchorMin.y, r.anchorMax.x, r.anchorMax.y); };
                int rows = CollectionPrototype.CampListKeys(true).Length;
                foreach (var (screen, dpi) in new[] { (new Vector2(1116, 2480), 401f), (new Vector2(720, 1680), 320f) })
                {
                    float scale = Mathf.Sqrt(screen.x / 720f * screen.y / 1280f), dp = dpi / 160f, tap = 48f * dp / scale;
                    Rect safe = Rect.MinMaxRect(16f * dp, 24f * dp, screen.x - 16f * dp, screen.y - 48f * dp);
                    float sw = safe.width / scale, sh = safe.height / scale;
                    Func<Rect, Rect> px = r => new Rect(safe.x + r.x * scale, safe.y + r.y * scale, r.width * scale, r.height * scale);
                    Func<Rect, Rect> band = r => Rect.MinMaxRect(safe.x + r.xMin * safe.width, safe.y + r.yMin * safe.height, safe.x + r.xMax * safe.width, safe.y + r.yMax * safe.height);
                    string at = " at " + screen.x + "x" + screen.y;
                    var hud = new List<(string what, Rect rect)>
                    {
                        ("title", band(CollectionPrototype.TitleBand)), ("status", band(CollectionPrototype.StatusBand)), ("feedback line", band(CollectionPrototype.FeedbackBand)),
                        ("RESET SAVE", band(CollectionPrototype.ResetSaveBand)), ("UNLOCK CAMPS", band(CollectionPrototype.UnlockCampsBand)), ("LEARN T1 RECIPES", band(CollectionPrototype.LearnRecipesBand)),
                        ("AUTO SORT", band(CollectionPrototype.AutoSortBand)), ("CLAIM OVERFLOW", band(anchors("CLAIM OVERFLOW"))), ("+1 GOLD", band(anchors("+1 GOLD"))),
                        ("SETTLEMENT (DEBUG)", band(anchors("SETTLEMENT (DEBUG)"))), ("AUDIT", band(CollectionPrototype.AuditBand)), ("trade offer note", band(anchors("Trade offer note"))), ("route card", band(anchors("Route card"))),
                        ("BAG", px(new Rect(margin, barLine * sh, square, square))), ("MENU", px(CollectionPrototype.UserMenuSquareRect(sw, sh))),
                        ("mission tracker", px(new Rect(margin - 0.05f * square, barLine * sh + square + gap - 0.06f * square, 1.1f * square, 1.16f * square))),   // CounterLayoutChecks' overhang
                        ("Settlement square", px(new Rect(sw - margin - square, listTop * sh - square, square, square))),
                    };
                    var listRows = Enumerable.Range(0, rows).Select(i => (what: "list row " + (i + 1), rect: px(CollectionPrototype.SettlementListRowRect(sw, sh, i, rows, tap)))).ToList();
                    var open = hud.Concat(listRows).ToList();
                    var menu = hud.Concat(Enumerable.Range(1, 4).Select(i => (what: "menu column " + i, rect: px(new Rect(sw - margin - square, barLine * sh + i * (square + gap), square, square))))).ToList();
                    Check(listRows.Any(r => r.rect.Overlaps(band(CollectionPrototype.ClaimOverflowBand))) && listRows.All(r => r.rect.height >= 48f * dp - 0.01f),
                        "CampListLayout overlap test can fail (CLAIM OVERFLOW's trail spot lies under the list) and rows stay 48 dp" + at);
                    // The offer note sits over the debug title and status lines by design (CreateTradeHud); every other pair must be apart.
                    Func<string, string, bool> allowed = (a, b) => (a == "trade offer note" || b == "trade offer note") && (a == "title" || b == "title" || a == "status" || b == "status");
                    foreach (var (state, seen) in new[] { ("list open", open), ("user menu open", menu) })
                    {
                        var clashes = new List<string>();
                        for (int i = 0; i < seen.Count; i++)
                            for (int j = i + 1; j < seen.Count; j++)
                                if (!allowed(seen[i].what, seen[j].what) && seen[i].rect.Overlaps(seen[j].rect)) clashes.Add(seen[i].what + " " + seen[i].rect.ToString("0") + " x " + seen[j].what + " " + seen[j].rect.ToString("0"));
                        Check(clashes.Count == 0, "CampListLayout nothing visible overlaps (" + state + ")" + at + (clashes.Count > 0 ? ": " + string.Join("; ", clashes) : ""));
                    }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
