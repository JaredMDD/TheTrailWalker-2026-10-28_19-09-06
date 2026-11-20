using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEngine;

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
        private static string Ids(ShopState s, string shop, ICollection<int> regions) => string.Join(",", ShopService.Offers(s, shop, regions).Select(o => o.ItemId ?? "-"));
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
                foreach (string sh in Shops) { var ids = ShopService.Offers(s, sh, r14).Where(o => !o.Empty).Select(o => o.ItemId).ToList(); distinct &= ids.Distinct().Count() == ids.Count; }
            }
            Check(distinct, "OffersDistinct");
            var all12 = Enumerable.Range(1, 12).ToArray(); int fine = 0; bool roles = true;
            for (int v = 1; v <= 2000; v++)
            {
                var o = ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Tailor, all12);
                roles &= Entry(o[0].ItemId, ShopCatalog.Tailor).Role == ShopSlotRole.RawFibre && Entry(o[1].ItemId, ShopCatalog.Tailor).Role == ShopSlotRole.RawFibre;
                ShopSlotRole r2 = Entry(o[2].ItemId, ShopCatalog.Tailor).Role; roles &= r2 == ShopSlotRole.Processed || r2 == ShopSlotRole.ProcessedFine;
                if (r2 == ShopSlotRole.ProcessedFine) fine++;
            }
            Check(roles && Math.Abs(fine / 2000.0 - 0.20) <= 0.03, "TailorSlots");
            bool fish = true; var seen = new HashSet<string>();
            for (int v = 1; v <= 200; v++)
            {
                var o1 = ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Fishery, r14);
                fish &= o1[0].ItemId != o1[1].ItemId && Entry(o1[0].ItemId, ShopCatalog.Fishery).Role == ShopSlotRole.Fish && Entry(o1[1].ItemId, ShopCatalog.Fishery).Role == ShopSlotRole.Fish && o1[2].ItemId == "ingredient.shrimp";
                var o2 = ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Fishery, all12);
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
            var sh1 = new ShopState { VisitId = 1 }; var tail = ShopService.Offers(sh1, ShopCatalog.Tailor, new[] { 1, 2 });
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
            var s = new ShopState { VisitId = 1 }; ShopOffer o = ShopService.Offers(s, ShopCatalog.Farm, r14)[0];
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
                var sr = new ShopState { VisitId = 1 }; ShopOffer or = ShopService.Offers(sr, ShopCatalog.Farm, r14)[0]; var wr = new Wallet(100000000);
                var full = new SpatialInventory((int)Math.Ceiling(def.Width * 1.5), (int)Math.Ceiling(def.Height * 1.5));
                ItemEntry held = full.AddWorld("held", def, 600, 5, prof); full.RestoreStored(held, ItemOwner.Backpack, 0, 0); full.Reserve(held, 3);
                string snapR = Snap(full);
                Check(!ShopService.TryBuy(sr, ShopCatalog.Farm, 0, 0, 100, wr, full, id, out reason) && reason == "Not enough room in the bag" && wr.Cash == 100000000 && or.UnitsLeft == 1000 && or.Revision == 0 && Snap(full) == snapR, "ReservedStackRespected (refused)");
                var roomy = new SpatialInventory();
                ItemEntry held2 = roomy.AddWorld("held", def, 600, 5, prof); roomy.RestoreStored(held2, ItemOwner.Backpack, 0, 0); roomy.Reserve(held2, 3);
                Check(ShopService.TryBuy(sr, ShopCatalog.Farm, 0, 0, 100, wr, roomy, id, out reason) && held2.Quantity == 600 && roomy.Entries.Count(e => e.Owner == ItemOwner.Backpack) == 2 && or.UnitsLeft == 900, "ReservedStackRespected (new stack)");
            }
            var s2 = new ShopState { VisitId = 1 }; ShopOffer o2 = ShopService.Offers(s2, ShopCatalog.Farm, r14)[0]; var w2 = new Wallet(100000000); string snap = Snap(tiny);
            Check(!ShopService.TryBuy(s2, ShopCatalog.Farm, 0, o2.Revision, 1000, w2, tiny, id, out reason) && reason == "Not enough room in the bag" && w2.Cash == 100000000 && o2.UnitsLeft == 1000 && o2.Revision == 0 && Snap(tiny) == snap, "PartialFitRefused");
            Check(!ShopService.TryBuy(s2, ShopCatalog.Farm, 0, 0, 1, new Wallet(0), new SpatialInventory(), id, out reason) && reason == "Not enough cash" && o2.UnitsLeft == 1000 && o2.Revision == 0, "CashRefused");
            Check(!ShopService.TryBuy(s2, ShopCatalog.Farm, 0, 0, 1001, w2, new SpatialInventory(), id, out reason) && reason == "Sold out" && w2.Cash == 100000000 && o2.Revision == 0, "SoldOutRefused");
            var b3 = new SpatialInventory();
            Check(!ShopService.TryBuy(s2, ShopCatalog.Farm, 0, 5, 1, w2, b3, id, out reason) && reason == "The stock changed; check the offer again" && w2.Cash == 100000000 && o2.UnitsLeft == 1000 && b3.Entries.Count == 0, "StaleRevisionRefused");
        }

        private static SessionSnapshot Roundtrip(SessionSnapshot snap) => JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(snap));
        private static bool Rejects(Action<SavedShops> tamper)
        {
            var s = new ShopState { VisitId = 3 }; ShopService.Offers(s, ShopCatalog.Farm, new[] { 1, 2, 3, 4 });
            SavedShops saved = SessionSaveStore.SaveShops(s); tamper(saved);
            try { SessionSaveStore.RestoreShops(new SessionSnapshot { shops = saved }); return false; } catch (InvalidDataException) { return true; }
        }

        private static long SaveShopsPrice()
        {
            var t = new ShopState { VisitId = 3 }; ShopService.Offers(t, ShopCatalog.Farm, new[] { 1, 2, 3, 4 });
            return t.Stock[ShopCatalog.Farm].Offers[0].UnitPriceMinor;
        }

        private static void SaveChecks()
        {
            int[] r14 = { 1, 2, 3, 4 };
            var s = new ShopState { VisitId = 5, DebugTier = 2 };
            foreach (string sh in Shops) ShopService.Offers(s, sh, new[] { 1, 2 });
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
                var t = new ShopState { VisitId = 3 }; ShopService.Offers(t, ShopCatalog.Farm, new[] { 1, 2, 3, 4 });
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
            var shortS = new ShopState { VisitId = 1 }; ShopService.Offers(shortS, ShopCatalog.Tailor, new[] { 1, 2 });
            ShopState shortBack = SessionSaveStore.RestoreShops(Roundtrip(new SessionSnapshot { shops = SessionSaveStore.SaveShops(shortS) }));
            Check(shortBack.Stock[ShopCatalog.Tailor].Offers.Count(o => o.Empty) == 1, "EmptySlotRoundTrip");
            var held = ShopService.Offers(back, ShopCatalog.Farm, r14);
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
            var c = measure(chicken); var d = measure(duck);
            Check(c.tris <= d.tris * 1.25f, "ChickenMeatModel triangles " + c.tris + " vs duck " + d.tris);
            Check(c.size >= d.size * .8f && c.size <= d.size * 1.25f, "ChickenMeatModel size " + c.size + " vs duck " + d.size);
        }

        // Task 5: the counter view, the purchase sheet's rules and texts, and the settlement list.
        private static void ViewChecks()
        {
            foreach (string name in new[] { "Farm", "Mine", "Tailor", "Ranch", "Fishery" })
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
            Check(CollectionPrototype.ShopIdForKey("bakery") == null && CollectionPrototype.ShopIdForKey("lumber") == null && CollectionPrototype.ShopIdForKey(null) == null, "ShopIdForKey ignores other keys");
            Check(CollectionPrototype.SettlementListKeys(true).SequenceEqual(new[] { "home", "warehouse", "lumber", "farm", "mine", "tailor", "ranch", "fishery", "return" }), "ListKeys");
            {
                var live = new ShopOffer { ItemId = "food.apples", UnitPriceMinor = Money.Minor(16), UnitsLeft = 990 };
                Check(CollectionPrototype.OfferTagText(live, "Apples") == "Apples\n16 cash" && CollectionPrototype.OfferTagText(new ShopOffer { ItemId = "food.apples", UnitsLeft = 0 }, "Apples") == "Sold out"
                    && CollectionPrototype.OfferTagText(new ShopOffer(), "") == "Sold out", "TagText");
            }
            Check(CollectionPrototype.AmountChoices(15, Money.Minor(1000), Money.Minor(16)).SequenceEqual(new[] { 1, 10 })
                && CollectionPrototype.AmountChoices(1000, long.MaxValue, 1).SequenceEqual(new[] { 1, 10, 100, 1000 })
                && CollectionPrototype.AmountChoices(0, long.MaxValue, 1).Length == 0, "AmountChoicesCapped");
            var offer = new ShopOffer { ItemId = "food.apples", UnitPriceMinor = Money.Minor(16), UnitsLeft = 990 };
            Check(CollectionPrototype.OfferLine(offer) == "Q5 · 16 cash each" && CollectionPrototype.LeftLine(offer) == "Left: 990", "Texts");
            // Nine list entries clear the closed-bag MENU square, rows at least 48 dp tall (the phone at 401 dpi; 320 dpi assumed for 720x1680).
            foreach (var (w, h, dpi) in new[] { (1116f, 2480f, 401f), (720f, 1680f, 320f) })
            {
                float scale = Mathf.Sqrt(w / 720f * h / 1280f);   // CanvasScaler: 720x1280 reference, match 0.5
                float sw = w / scale, sh = h / scale; int rows = CollectionPrototype.SettlementListKeys(true).Length;
                Rect low = CollectionPrototype.SettlementListRowRect(sw, sh, rows - 1, rows), menu = CollectionPrototype.UserMenuSquareRect(sw, sh);
                Rect above = CollectionPrototype.SettlementListRowRect(sw, sh, rows - 2, rows);
                Check(!low.Overlaps(menu) && low.yMin > menu.yMax && above.yMin > low.yMax && low.height * scale >= 48f * dpi / 160f - 0.01f,
                    "ListClearsMenu at " + w + "x" + h + ": lowest row " + low.yMin.ToString("0.0") + "-" + low.yMax.ToString("0.0") + ", MENU " + menu.yMin.ToString("0.0") + "-" + menu.yMax.ToString("0.0") + ", row " + (low.height * scale).ToString("0.0") + " px");
            }
            Check(CollectionPrototype.SettlementListRowRect(720f, 1280f, 0, 1).height == 84f && CollectionPrototype.SettlementListRowY(0) == -94f, "A short list keeps the full 84 + 10 rows");
            {
                var s = new ShopState { VisitId = 1 }; ShopOffer o = ShopService.Offers(s, ShopCatalog.Farm, new[] { 1, 2, 3, 4 })[0];
                var log = new List<TradeRecord> { new TradeRecord { result = "WarehouseDeposit" } }; var when = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);
                bool bought = CollectionPrototype.BuyAndRecord(s, ShopCatalog.Farm, 0, 0, 10, new Wallet(1000000), new SpatialInventory(), () => "rec1", log, when, out string why);
                TradeRecord r = log.Last(); long total = 10 * o.UnitPriceMinor;
                Check(bought && log.Count == 2 && r.utc == when.ToString("O", System.Globalization.CultureInfo.InvariantCulture) && r.settlementId == ShopCatalog.Farm && r.itemId == o.ItemId && r.quantity == 10 && r.quality == 5
                    && r.unitMinor == o.UnitPriceMinor && r.totalMinor == total && r.cash == -total / Money.MinorPerCash && r.result == "shop.bought", "PurchaseRecorded");
                Check(!CollectionPrototype.BuyAndRecord(s, ShopCatalog.Farm, 0, 0, 10, new Wallet(1000000), new SpatialInventory(), () => "rec2", log, when, out why) && log.Count == 2, "PurchaseRecorded (a refused purchase adds no record)");
                var back = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(new SessionSnapshot { warehouseLog = log }));
                Check(back.warehouseLog.Count == 2 && back.warehouseLog[1].result == "shop.bought" && back.warehouseLog[1].cash == r.cash, "PurchaseRecorded (the record survives a save round trip)");
            }
        }

        // Task 7 R1: price tags. Text, then layout at the counter framing (tags clear each other, sit in the area, rest on the model tops), then taps at tag centres.
        internal static Rect[] LastTagRects; internal static Vector2[] LastTagTops;
        private static void TagChecks(string name, Vector2 screen, Camera cam, Transform counter)
        {
            var offers = ShopService.Offers(new ShopState { VisitId = 1 }, CollectionPrototype.ShopIdForKey(name.ToLowerInvariant()), new[] { 1, 2, 3, 4 });
            var tops = new Vector2[3]; var texts = new string[3]; var models = new List<GameObject>(); var anchors = new Vector2?[3];
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
                    tops[i] = top; anchors[i] = cam.WorldToScreenPoint(anchor.position);
                    texts[i] = CollectionPrototype.OfferTagText(offers[i], def.Name);
                }
                Rect area = TagArea(screen, out float h, out float gap);
                var chars = texts.Select(CollectionPrototype.LongestLine).ToArray(); var tags = new Rect[3];
                CollectionPrototype.ShopTagLayout(tops, chars, area, h, gap, tags);
                LastTagRects = tags; LastTagTops = tops;
                CollectionPrototype.ShopTagLayout(tops, chars, area, h, gap, tags);   // warm up
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int n = 0; n < 100; n++) CollectionPrototype.ShopTagLayout(tops, chars, area, h, gap, tags);
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
        private static Rect TagArea(Vector2 screen, out float h, out float gap)
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
            foreach (string name in new[] { "Farm", "Ranch", "Fishery", "Mine", "Tailor", "Warehouse", "Lumber" })
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
                            var offers = ShopService.Offers(new ShopState { VisitId = 1 }, CollectionPrototype.ShopIdForKey(name.ToLowerInvariant()), new[] { 1, 2, 3, 4 });
                            var tops = new Vector2[3]; var chars = new int[3];
                            for (int i = 0; i < 3; i++)
                            {
                                Transform anchor = counter.Find("OfferAnchor" + (i + 1));
                                foreach (Transform baked in anchor) baked.gameObject.SetActive(false);
                                ItemDefinition def = Items.Find(offers[i].ItemId);
                                GameObject model = CollectionPrototype.PlaceOfferModel(anchor, def); models.Add(model);
                                Bounds mb = RendererBounds(model.transform); mb.Encapsulate(anchor.position);
                                keep.Add(("offer " + (i + 1), ScreenRect(cam, mb, screen)));
                                tops[i] = Vector2.Scale(cam.WorldToViewportPoint(new Vector3(anchor.position.x, mb.max.y, anchor.position.z)), screen);
                                chars[i] = CollectionPrototype.LongestLine(CollectionPrototype.OfferTagText(offers[i], def.Name));
                            }
                            var tags = new Rect[3]; Rect area = TagArea(screen, out float h, out float gap);
                            CollectionPrototype.ShopTagLayout(tops, chars, area, h, gap, tags);
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
            Check(ShopCatalog.All.All(e => SpecPrices.ContainsKey(e.ItemId)) && SpecPrices.Keys.All(id => ShopCatalog.All.Any(e => e.ItemId == id)), "the price table and the catalog list the same goods");
            foreach (ShopEntry e in ShopCatalog.All)
            {
                long q5 = ItemPrices.Profile(Items.Find(e.ItemId), 5).UnitValue;
                Check(q5 > 0 && 4 * q5 <= ShopCatalog.UnitPriceMinor(e), e.ItemId + " sells for less than the shop charges");
                long rule = e.CashOverride ?? 4 * ItemPrices.Profile(Items.Find(e.ItemId), 5).UnitValue / Money.MinorPerCash;
                Check(rule == SpecPrices[e.ItemId] && ShopCatalog.UnitPriceMinor(e) == Money.Minor(SpecPrices[e.ItemId]), e.ShopId + " " + e.ItemId + " costs " + SpecPrices[e.ItemId]);
            }
        }
    }
}
