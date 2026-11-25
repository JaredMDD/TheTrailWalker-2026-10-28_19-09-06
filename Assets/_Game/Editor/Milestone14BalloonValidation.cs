using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Equipment;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Missions;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Movement;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEngine;
using M14 = TheTrailWalker.Editor.Milestone14Validation;

namespace TheTrailWalker.Editor
{
    // Milestone 14 slice 3: the balloon merchant. Each task adds a group of checks called from Run().
    public static class Milestone14BalloonValidation
    {
        public static void Run()
        {
            HostGaps();
            HostsExistInCatalog();
            SceneryOnlyAtHosts();
            SpecialGearTable();
            SpecialStatsApply();
            SpecialNeverWears();
            SpecialResale();
            SpecialArtBound();
            SpecialTakeOffSwapSave();
            SpecialLegacyAndDetails();
            BoxPoolCount();
            BoxRecipesExact();
            BoxProductsHaveArt();
            BottlingNeedsFilling();
            BoxInWindow();
            GateRawRegion();
            GateRefined();
            GateBarrel();
            GateIgnoresBagAndGold();
            BakeryUnchanged();
            OfferShape();
            SessionRerollKeepsPurchases();
            GearFullBagRefused();
            BoxFullBagWaits();
            BoxPrice();
            ManifestShape();
            ReservedAcrossBoxesAndBakery();
            RecipeKnownBeforeClaimGivesFallback();
            ClaimOnceByTxn();
            StaleOfferRefused();
            BoxFollowsPlayerTier();
            BoxChargeAndManifestAtomic();
            ClaimSurvivesReload();
            OpenSessionSurvivesReload();
            OldSaveLoads();
            MalformedBalloonRejected();
            SessionIdFloor();
            MissingItemsStillLoad();
            ZeroProfileGuard();
            InteriorPrefab();
            RanchDishModels();
            BalloonViewChecks();
            BalloonEntryOnlyAtHostSeated();
            BalloonTeardownPaths();
            BoxSheetClaims();
            FinalFixWave();
            GearSheetBuys();
            Debug.Log("Milestone 14 balloon: host checks passed.");
        }

        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Milestone 14 balloon: " + name); }

        // Regions 1-4 camps in route order (each region's NextRegionId).
        private static List<string> Route()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            var route = new List<string>();
            for (RegionDefinition r = catalog.StartRegion; r != null && r.Number <= 4; r = catalog.Region(r.NextRegionId)) route.AddRange(r.Camps.Select(c => c.Id));
            return route;
        }

        private static void HostGaps()
        {
            List<string> route = Route();
            int[] at = BalloonHosts.All.Select(id => route.IndexOf(id)).ToArray();
            Check(at.Length > 0 && at.All(i => i >= 0) && at.SequenceEqual(at.OrderBy(i => i)), "HostGaps: hosts are in route order");
            Check(at[0] <= 5, "HostGaps: the first host is within the first 6 camps (index " + at[0] + ")");
            for (int i = 1; i < at.Length; i++) Check(at[i] - at[i - 1] >= 4 && at[i] - at[i - 1] <= 6, "HostGaps: gap " + (at[i] - at[i - 1]) + " between hosts " + at[i - 1] + " and " + at[i]);
            Check(route.Count - 1 - at[at.Length - 1] <= 6, "HostGaps: no stretch over 6 camps after the last host");
            Check(BalloonHosts.RegionTier("camp.01.05") == 1 && BalloonHosts.RegionTier("camp.04.08") == 1 && BalloonHosts.RegionTier("camp.01.01") == 0, "HostGaps: RegionTier is the shops' formula, 0 for non-hosts");
        }

        private static void HostsExistInCatalog()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            Check(BalloonHosts.All.All(id => catalog.Camp(id) != null) && BalloonHosts.All.Distinct().Count() == BalloonHosts.All.Count, "HostsExistInCatalog: every listed id is a distinct real camp");
        }

        private static void SceneryOnlyAtHosts()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            var path = new SampledTrailPath(MovementPrototype.DefaultKnots());
            var parent = new GameObject("balloon host check").transform;
            try
            {
                foreach (string id in new[] { "camp.01.05", "camp.01.04" })
                {
                    CampAnchor anchor = TravelScenery.BuildCamp(parent, catalog.Camp(id), path, 0, null, null, null);
                    bool has = anchor != null && parent.Find("Camp " + catalog.Camp(id).Name).Cast<Transform>().Any(t => t.name.StartsWith("Balloon merchant", StringComparison.Ordinal));
                    Check(has == BalloonHosts.IsHost(id), "SceneryOnlyAtHosts: " + id + " balloon present=" + has + " host=" + BalloonHosts.IsHost(id));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(parent.gameObject); }
        }

        private static readonly string[] Parts = { "hat", "boots", "shirt", "pants", "pack" };
        private static readonly GearSlot[] Slots = { GearSlot.Hat, GearSlot.Boots, GearSlot.Shirt, GearSlot.Pants, GearSlot.Backpack };
        // Section 14.1's tables, row by row: stat then gold per tier.
        private static readonly double[][] Stat14 = { new[] { 4, 2.25, 2, 2, 8 }, new[] { 5, 2.6, 3, 3, 9 }, new[] { 7, 3.35, 5, 5, 11 }, new[] { 10, 4.1, 8, 8, 13 } };
        private static readonly long[][] Gold14 = { new long[] { 15, 20, 15, 15, 25 }, new long[] { 50, 65, 50, 50, 80 }, new long[] { 110, 140, 110, 110, 180 }, new long[] { 200, 260, 200, 200, 320 } };

        private static void SpecialGearTable()
        {
            for (int t = 1; t <= 4; t++)
                for (int p = 0; p < 5; p++)
                {
                    string id = "special.t" + t + "." + Parts[p];
                    ItemDefinition d = TheTrailWalker.Domain.Crafting.Items.Find(id);
                    Check(d != null && SpecialGear.Is(id) && d.GearSlot == Slots[p] && d.GearRank == t && !d.Stackable, "SpecialGearTable: " + id + " is registered gear of its tier");
                    Check(Math.Abs(SpecialGear.Stat(id) - Stat14[t - 1][p]) < 1e-9 && SpecialGear.GoldPrice(id) == Gold14[t - 1][p], "SpecialGearTable: " + id + " stat/gold");
                }
            Check(!SpecialGear.Is("special.t5.hat") && !SpecialGear.Is("item.rabbit_hat") && !SpecialGear.Is(null), "SpecialGearTable: Is rejects other ids");
        }

        private static EquipmentState Wear(int tier, int part) { var e = new EquipmentState(); e.Equip(new WornGear("w" + part, Slots[part], tier, 5, 0, true, SpecialGear.Stat(SpecialGear.Id(tier, part)))); return e; }

        private static void SpecialStatsApply()
        {
            Check(Wear(2, 0).MaxHearts == 5, "SpecialStatsApply: t2 hat gives 5 hearts, not the rank-2 value");
            Check(Math.Abs(Wear(2, 1).SpeedCap - 2.6) < 1e-9, "SpecialStatsApply: t2 boots 2.6 m/s");
            Check(Wear(2, 2).Pockets == 3, "SpecialStatsApply: t2 shirt 3 pockets");
            Check(Wear(2, 3).Hooks == 3, "SpecialStatsApply: t2 pants 3 hooks");
            Check(Wear(2, 4).BackpackWidth == 9 && Wear(2, 4).BackpackHeight == 4, "SpecialStatsApply: t2 pack 9 x 4");
            var plain = new EquipmentState(); plain.Equip(new WornGear("r", GearSlot.Hat, 2, 5, 100));
            Check(plain.MaxHearts == 6, "SpecialStatsApply: ordinary rank-2 gear keeps the rank table");
        }

        private static void SpecialNeverWears()
        {
            var e = new EquipmentState();
            for (int p = 0; p < 5; p++) e.Equip(new WornGear("n" + p, Slots[p], 2, 5, 0, true, SpecialGear.Stat(SpecialGear.Id(2, p))));
            Check(e.ApplyDistance(1e9).Count == 0 && e.Get(GearSlot.Hat) != null && e.MaxHearts == 5, "SpecialNeverWears: 1e9 m wears nothing");
            // Reload: the save holds slot, rank, quality, remaining and infinite; the override is re-derived from them.
            WornGear saved = e.Get(GearSlot.Boots);
            var reloaded = new EquipmentState(); reloaded.Equip(new WornGear(saved.InstanceId, saved.Slot, saved.Rank, saved.Quality, saved.RemainingDistance, saved.Infinite, saved.StatOverride));
            Check(reloaded.Get(GearSlot.Boots).Infinite && Math.Abs(reloaded.SpeedCap - 2.6) < 1e-9, "SpecialNeverWears: reload keeps infinite and the stat");
        }

        private static void SpecialResale()
        {
            long[] clothing = { 440, 1320, 2640, 4400 }, packs = { 700, 2100, 4200, 7000 };
            for (int t = 1; t <= 4; t++)
                for (int p = 0; p < 5; p++)
                {
                    string id = "special.t" + t + "." + Parts[p];
                    ItemProfile profile = ItemPrices.Profile(TheTrailWalker.Domain.Crafting.Items.Find(id), 5);
                    Check(profile.UnitValue == Money.Minor(p == 4 ? packs[t - 1] : clothing[t - 1]) && profile.MaxDurability == 0 && SpecialGear.ResaleCash(id) == (p == 4 ? packs[t - 1] : clothing[t - 1]), "SpecialResale: " + id + " is the fixed resale cash with no durability premium");
                }
        }

        private static void SpecialArtBound()
        {
            var catalog = Resources.Load<BalloonGoodsArtCatalog>("BalloonGoods/BalloonGoodsArtCatalog");
            Check(catalog != null, "SpecialArtBound: catalog loads");
            foreach (ItemDefinition d in SpecialGear.All)
            {
                BalloonSpecialItemArt art = catalog.SpecialItems.FirstOrDefault(x => x.Id == d.Id);
                Check(art != null && art.Display != null && art.MaleWearable != null && art.FemaleWearable != null, "SpecialArtBound: " + d.Id);
            }
        }

        private static void SpecialTakeOffSwapSave()
        {
            var bag = new TheTrailWalker.Domain.Inventory.SpatialInventory(8000, 4000);
            ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find("special.t2.hat");
            ItemEntry hat = bag.AddWorld("sp-hat", def, 1, 5, ItemPrices.Profile(def, 5));
            Check(bag.RestoreStored(hat, ItemOwner.Worn, 0, 0), "SpecialTakeOff: item worn");
            var eq = new EquipmentState(); eq.Equip(new WornGear(hat.Id, GearSlot.Hat, 2, 5, 0, true, SpecialGear.Stat(def.Id)));
            Check(bag.TakeOff(hat.Id, eq.Get(GearSlot.Hat).RemainingDistance) && hat.Owner == ItemOwner.Overflow && !hat.HasWearRemaining, "SpecialTakeOff: take-off works with no wear state");
            eq.Unequip(GearSlot.Hat);
            // Swap: a normal hat goes on, then the special one is re-equipped keeping infinite and its stat.
            eq.Equip(new WornGear("normal", GearSlot.Hat, 2, 5, 100));
            Check(eq.MaxHearts == 6 && eq.Unequip(GearSlot.Hat) != null, "SpecialTakeOff: swap with normal gear");
            Check(bag.RestoreWear(hat, false, 0), "SpecialTakeOff: a special bag item restores without wear");
            var back = new EquipmentState(); back.Equip(new WornGear(hat.Id, GearSlot.Hat, 2, 5, 0, true, SpecialGear.Stat(def.Id)));
            Check(back.MaxHearts == 5 && back.Get(GearSlot.Hat).Infinite && back.ApplyDistance(1e9).Count == 0, "SpecialTakeOff: re-equip keeps special stat and infinite");
            // The save field: a stat of 0 (absent) means none; the stat is checked against the table.
            var saved = JsonUtility.FromJson<TheTrailWalker.Persistence.SavedGear>(JsonUtility.ToJson(new TheTrailWalker.Persistence.SavedGear { id = "x", slot = "Hat", rank = 2, quality = 5, infinite = true, statOverride = 5 }));
            Check(saved.statOverride == 5 && SpecialGear.Matches(GearSlot.Hat, 2, saved.statOverride) && !SpecialGear.Matches(GearSlot.Hat, 2, 6), "SpecialTakeOff: saved stat round trips and is validated");
        }

        private static void SpecialLegacyAndDetails()
        {
            var rankHat = new EquipmentState(); rankHat.Equip(new WornGear("legacy", GearSlot.Hat, 2, 5, 0, true));
            Check(rankHat.MaxHearts == 6 && !rankHat.Get(GearSlot.Hat).StatOverride.HasValue, "Legacy: a non-special infinite hat keeps rank-table hearts");
            Check(!CollectionPrototype.TakeOffAllowed(rankHat.Get(GearSlot.Hat), true) && !CollectionPrototype.TakeOffAllowed(rankHat.Get(GearSlot.Hat), false), "Legacy: infinite gear without a stat override refuses take-off");
            var special = new WornGear("s", GearSlot.Hat, 2, 5, 0, true, 5);
            Check(CollectionPrototype.TakeOffAllowed(special, true) && !CollectionPrototype.TakeOffAllowed(special, false), "Legacy: special gear needs a bag item to be taken off (no save block)");
            Check(!CollectionPrototype.ShowsDurability(SpecialGear.All[0]) && CollectionPrototype.ShowsDurability(TheTrailWalker.Domain.Crafting.Items.Find("item.rabbit_hat")), "Details: special gear has no durability line, ordinary gear keeps it");
        }

        // ---- Task 3: the 59-recipe box pool, transcribed from PROJECT_RECOMENDATIONS 9.4.2-9.4.3, 9.4.5, 13.4.2 and 9.5.1.
        // Row: id | F(ood) or M(isc) | tier | output | yield | hearts | inputs.
        private static readonly string[] BoxRows = {
            // 9.4.2 and 9.4.3: 21 Food + 5 Misc.
            "recipe.food.strawberry_glazed_shrimp_skewers|F|1|item.strawberry_glazed_shrimp_skewers|1|8|ingredient.shrimp:1,food.strawberries:1,ingredient.rock_salt:1",
            "recipe.food.shrimp_pie|F|1|item.shrimp_pie|1|10|ingredient.shrimp:1,ingredient.wheat_flour:1,ingredient.cow_butter:1",
            "recipe.food.truffle_butter_crab|F|2|item.truffle_butter_crab|1|9|fishery.crab:1,ingredient.truffle:1,ingredient.cow_butter:1",
            "recipe.food.creamy_crab_soup|F|2|item.creamy_crab_soup|1|10|fishery.crab:1,ingredient.cow_cream:1,ingredient.carrots:1",
            "recipe.food.honey_blackberry_biscuits|F|2|item.honey_blackberry_biscuits|2|9|ingredient.wheat_flour:2,ingredient.honey:1,food.blackberries:1",
            "recipe.food.crispy_mint_squid|F|3|item.crispy_mint_squid|1|10|fishery.squid:1,ingredient.sunflower_oil:1,ingredient.wild_mint:1",
            "recipe.food.squid_rice_bowl|F|3|item.squid_rice_bowl|1|11|fishery.squid:1,ingredient.rice:1,ingredient.green_chili:1",
            "recipe.food.mint_cow_cheese_flatbread|F|3|item.mint_cow_cheese_flatbread|1|10|ingredient.wheat_flour:1,ingredient.cow_cheese:1,ingredient.wild_mint:1",
            "recipe.food.coconut_chili_lobster|F|4|item.coconut_chili_lobster|1|11|fishery.lobster:1,food.coconuts:1,ingredient.green_chili:1",
            "recipe.food.lobster_rice_bowl|F|4|item.lobster_rice_bowl|1|12|fishery.lobster:1,ingredient.rice:1,ingredient.seaweed:1",
            "recipe.food.cacao_coconut_tart|F|4|item.cacao_coconut_tart|1|12|ingredient.cacao_powder:1,food.coconuts:1,ingredient.wheat_flour:1",
            "recipe.food.apple_oat_compote|F|1|item.apple_oat_compote|1|10|food.apples:3,ingredient.oats:2,food.milk.cow:1",
            "recipe.food.red_berry_oat_porridge|F|1|item.red_berry_oat_porridge|1|10|food.berries:3,ingredient.oats:2,food.milk.cow:1",
            "recipe.food.beaver_barley_hotpot|F|2|item.beaver_barley_hotpot|1|10|animal.beaver.meat:2,ingredient.barley:2,ingredient.carrots:1",
            "recipe.food.blackberry_honey_oats|F|2|item.blackberry_honey_oats|1|10|food.blackberries:3,ingredient.oats:2,ingredient.honey:1",
            "recipe.food.turkey_bean_bowl|F|3|item.turkey_bean_bowl|1|11|animal.turkey.meat:2,ingredient.beans:2,ingredient.corn_flour:1",
            "recipe.food.pumpkin_chickpea_pot|F|3|item.pumpkin_chickpea_pot|1|11|ingredient.pumpkins:2,ingredient.chickpeas:2,ingredient.cow_cheese:1",
            "recipe.food.boar_potato_chili|F|4|item.boar_potato_chili|1|12|animal.boar.meat:2,ingredient.potatoes:2,ingredient.green_chili:1",
            "recipe.food.coconut_cacao_pudding|F|4|item.coconut_cacao_pudding|1|12|food.coconuts:2,ingredient.cacao_powder:1,food.milk.cow:1",
            "recipe.misc.pinecone_rabbit|M|1|item.pinecone_rabbit|1|0|material.pine_cone:1,material.pine_log:1,material.hemp:1",
            "recipe.misc.birch_wax_bee|M|2|item.birch_wax_bee|1|0|item.birch_plank:1,item.beeswax:1,animal.beaver.pelt:1",
            "recipe.misc.painted_turkey|M|3|item.painted_turkey|1|0|item.mesquite_plank:1,animal.turkey.feathers:1,material.yellow_ochre:1",
            "recipe.misc.silver_sun_brooch|M|3|item.silver_sun_brooch|1|0|item.silver_ingot:1,item.cut_turquoise:1,item.flax_cord:1",
            "recipe.misc.obsidian_boar|M|4|item.obsidian_boar|1|0|item.oak_plank:1,material.obsidian:1,ingredient.sunflower_oil:1",
            "recipe.food.truffle_cow_cheese_flatbread|F|2|item.truffle_cow_cheese_flatbread|1|10|ingredient.truffle:2,ingredient.wheat_flour:1,ingredient.cow_cheese:1",
            "recipe.food.turkey_truffle_bean_pot|F|3|item.turkey_truffle_bean_pot|1|11|ingredient.truffle:1,animal.turkey.meat:2,ingredient.beans:2",
            // 13.4.2: premium ranch recipes.
            "recipe.food.chicken_mushroom_pie|F|1|item.chicken_mushroom_pie|1|10|animal.chicken.meat:1,ingredient.mushrooms:2,ingredient.wheat_flour:1",
            "recipe.food.apple_glazed_chicken|F|1|item.apple_glazed_chicken|1|9|animal.chicken.meat:1,food.apples:2,material.pine_branch:1",
            "recipe.food.honey_roast_duck|F|2|item.honey_roast_duck|1|10|animal.duck.meat:1,ingredient.honey:1,material.birch_branch:2",
            "recipe.food.duck_blueberry_rice|F|2|item.duck_blueberry_rice|1|9|animal.duck.meat:1,food.blueberries:1,ingredient.rice:1",
            "recipe.food.mint_lamb_stew|F|3|item.mint_lamb_stew|1|11|animal.lamb.meat:2,ingredient.wild_mint:1,ingredient.chickpeas:2",
            "recipe.food.lamb_chili_flatbread|F|3|item.lamb_chili_flatbread|1|10|animal.lamb.meat:1,item.corn_flatbread:1,ingredient.green_chili:1",
            "recipe.food.coffee_braised_beef|F|4|item.coffee_braised_beef|1|12|animal.beef.meat:1,item.roasted_coffee:1,ingredient.potatoes:2",
            "recipe.food.beef_cacao_chili|F|4|item.beef_cacao_chili|1|11|animal.beef.meat:1,ingredient.cacao_powder:1,ingredient.green_chili:1",
            // 9.5.1: precious-metal and gem keepsakes.
            "recipe.misc.quartz_prism_lantern|M|2|item.quartz_prism_lantern|1|0|item.cut_quartz:1,item.copper_fittings:2,item.glass:1",
            "recipe.misc.tin_quartz_music_box|M|2|item.tin_quartz_music_box|1|0|item.tin_ingot:1,item.cut_quartz:1,item.birch_plank:1",
            "recipe.misc.silver_turquoise_buckle|M|3|item.silver_turquoise_buckle|1|0|item.silver_ingot:1,item.cut_turquoise:1,item.fine_beaver_leather:1",
            "recipe.misc.bronze_turquoise_compass|M|3|item.bronze_turquoise_compass|1|0|item.bronze_ingot:1,item.cut_turquoise:1,item.glass:1",
            "recipe.misc.diamond_pocket_watch|M|4|item.diamond_pocket_watch|1|0|item.gold_ingot:1,item.cut_diamond:1,item.steel_fittings:1",
            "recipe.misc.royal_gem_chalice|M|4|item.royal_gem_chalice|1|0|item.gold_ingot:1,item.cut_ruby:1,item.cut_sapphire:1",
            "recipe.misc.lapis_jewel_box|M|4|item.lapis_jewel_box|1|0|item.oak_plank:2,item.polished_lapis:1,item.gold_ingot:1",
        };
        // 9.4.5: drink | hearts per bottle | aging ingredients. Filling = 4 Oak planks + 2 Iron fittings + these -> 1 barrel (tier 4);
        // bottling = 1 barrel + 8 Glass bottles -> 8 (tier 4 Food).
        private static readonly string[] DrinkRows = {
            "apple_cider|11|food.apples:12", "strawberry_wine|12|food.strawberries:12,ingredient.sugar:2", "blackberry_wine|12|food.blackberries:12,ingredient.sugar:2",
            "blueberry_wine|12|food.blueberries:12,ingredient.honey:2", "honey_mead|12|ingredient.honey:10,ingredient.wild_mint:1", "barley_ale|11|ingredient.barley:12,ingredient.honey:2",
            "wheat_beer|11|ingredient.wheat:8,ingredient.barley:4", "cane_rum|12|ingredient.sugar_cane:12,ingredient.sugar:2",
            "corn_whiskey|12|ingredient.corn:8,ingredient.barley:4,item.mesquite_charcoal:1" };
        private static IEnumerable<string> AllBoxRows() => BoxRows.Concat(DrinkRows.SelectMany(d =>
        {
            string[] f = d.Split('|');
            return new[] { $"recipe.barrel.{f[0]}|M|4|item.{f[0]}_barrel|1|0|item.oak_plank:4,item.iron_fittings:2,{f[2]}", $"recipe.food.{f[0]}|F|4|item.{f[0]}|8|{f[1]}|item.{f[0]}_barrel:1,item.glass_bottle:8" };
        }));
        private static List<TheTrailWalker.Domain.Crafting.RecipeDefinition> BoxPool() => TheTrailWalker.Domain.Crafting.RecipeBook.All.Where(r => r.MysteryBoxEligible).ToList();

        private static void BoxPoolCount()
        {
            List<TheTrailWalker.Domain.Crafting.RecipeDefinition> pool = BoxPool();
            var missions = new HashSet<string>(TheTrailWalker.Domain.Missions.MissionBook.All.Select(m => m.RewardRecipeId));
            Check(pool.Count == 59 && pool.Select(r => r.Id).Distinct().Count() == 59, "BoxPoolCount: exactly 59 distinct eligible recipes (" + pool.Count + ")");
            Check(pool.All(r => !missions.Contains(r.Id) && !r.Retired), "BoxPoolCount: no eligible recipe is a mission reward or retired");
            Check(new HashSet<string>(AllBoxRows().Select(row => row.Split('|')[0])).SetEquals(pool.Select(r => r.Id)), "BoxPoolCount: the pool is exactly the source tables' 59 ids");
        }

        private static void BoxRecipesExact()
        {
            foreach (string row in AllBoxRows())
            {
                string[] f = row.Split('|');
                TheTrailWalker.Domain.Crafting.RecipeDefinition r = TheTrailWalker.Domain.Crafting.RecipeBook.Find(f[0]);
                bool food = f[1] == "F";
                string want = string.Join(",", f[6].Split(',').OrderBy(x => x)), got = r == null ? "" : string.Join(",", r.Inputs.Select(i => i.ItemId + ":" + i.Quantity).OrderBy(x => x));
                ItemDefinition output = r?.Output;
                Check(r != null && r.MysteryBoxEligible && r.Role == (food ? TheTrailWalker.Domain.Crafting.RecipeRole.Food : TheTrailWalker.Domain.Crafting.RecipeRole.Misc)
                    && r.Tier == int.Parse(f[2]) && r.OutputId == f[3] && r.Yield == int.Parse(f[4]) && want == got && r.BaseSeconds == (food ? 3f : 10f)
                    && output != null && output.FoodHearts == int.Parse(f[5]) && output.PreparedFood == food && TheTrailWalker.Domain.Crafting.ItemTiers.Of(f[3]) == r.Tier,
                    "BoxRecipesExact: " + f[0] + " (inputs " + got + ")");
            }
        }

        // Products resolve, through the bag/trail lookup chain, to the BalloonExclusiveArtCatalog visual. The catalog has no
        // ranch or keepsake entries: tier 1-2 ones still need a model, tier 3-4 ones may wait for art.
        private static void BoxProductsHaveArt()
        {
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<BalloonExclusiveArtCatalog>("Assets/_Game/Resources/BalloonExclusive/BalloonExclusiveArtCatalog.asset");
            Check(catalog != null, "BoxProductsHaveArt: catalog loads");
            var attach = typeof(Tier2ShopVisuals).Assembly.GetType("TheTrailWalker.ResourceVisualTuning").GetMethod("Attach", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (TheTrailWalker.Domain.Crafting.RecipeDefinition r in BoxPool())
            {
                var probe = new GameObject("box art probe", typeof(MeshRenderer), typeof(MeshFilter));
                string model;
                try { var visual = (GameObject)attach.Invoke(null, new object[] { probe, r.Output }); model = visual == null ? null : visual.name.Replace("(Clone)", ""); }
                finally { UnityEngine.Object.DestroyImmediate(probe); }
                BalloonExclusiveOutputArt art = catalog.Outputs.FirstOrDefault(o => o.RecipeId == r.Id);
                if (art != null) Check(art.OutputId == r.OutputId && art.Visual != null && model == art.Visual.name, "BoxProductsHaveArt: " + r.OutputId + " shows its catalog visual (" + model + ")");
                else Check(r.Tier >= 3 || model != null, "BoxProductsHaveArt: tier " + r.Tier + " " + r.OutputId + " has a model");
            }
        }

        private static void BottlingNeedsFilling()
        {
            var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<BalloonExclusiveArtCatalog>("Assets/_Game/Resources/BalloonExclusive/BalloonExclusiveArtCatalog.asset");
            foreach (string d in DrinkRows.Select(x => x.Split('|')[0]))
            {
                TheTrailWalker.Domain.Crafting.RecipeDefinition bottling = TheTrailWalker.Domain.Crafting.RecipeBook.Find("recipe.food." + d);
                // The bottling recipe names its filling recipe through its barrel input: that barrel has exactly one maker.
                string barrel = bottling?.Inputs.FirstOrDefault(i => i.ItemId.EndsWith("_barrel")).ItemId;
                var makers = TheTrailWalker.Domain.Crafting.RecipeBook.All.Where(r => r.OutputId == barrel).Select(r => r.Id).ToArray();
                string required = catalog.Outputs.FirstOrDefault(o => o.RecipeId == "recipe.food." + d)?.RequiredRecipeId;
                Check(makers.Length == 1 && makers[0] == "recipe.barrel." + d && required == makers[0], "BottlingNeedsFilling: recipe.food." + d + " needs recipe.barrel." + d);
            }
        }

        private static void BoxInWindow()
        {
            Check(BoxPool().All(r => r.Inputs.All(i => TheTrailWalker.Domain.Crafting.ItemTiers.InWindow(r, i.ItemId))), "InWindow: every box recipe input is inside its tier window: "
                + string.Join(", ", BoxPool().SelectMany(r => r.Inputs.Where(i => !TheTrailWalker.Domain.Crafting.ItemTiers.InWindow(r, i.ItemId)).Select(i => r.Id + "/" + i.ItemId))));
            // The barrel exception (section 9.0.1) covers only a filling recipe's aging produce: not its wood, nor ordinary Misc.
            var barrel = TheTrailWalker.Domain.Crafting.RecipeBook.Find("recipe.barrel.apple_cider");
            var misc = new TheTrailWalker.Domain.Crafting.RecipeDefinition("t.misc", "t", TheTrailWalker.Domain.Crafting.RecipeRole.Misc, 4, "item.obsidian_boar", 1, 10, "", new[] { new TheTrailWalker.Domain.Crafting.RecipeInput("food.apples", 1) });
            Check(barrel != null && !TheTrailWalker.Domain.Crafting.ItemTiers.InWindow(barrel, "item.pine_plank") && !TheTrailWalker.Domain.Crafting.ItemTiers.InWindow(misc, "food.apples"),
                "InWindow: the barrel exception admits only aging produce in filling recipes");
        }

        private static TheTrailWalker.Domain.Crafting.CraftingState KnowIds(IEnumerable<string> ids) { var c = new TheTrailWalker.Domain.Crafting.CraftingState(); foreach (string i in ids) c.Learn(i); return c; }
        private static readonly int[] Regions1To4 = { 1, 2, 3, 4 };
        private static int[] AllRegions() => Enumerable.Range(1, 24).ToArray();
        private static IEnumerable<string> AllMakers() => TheTrailWalker.Domain.Crafting.RecipeBook.All.Where(r => !r.Retired && !r.MysteryBoxEligible).Select(r => r.Id);
        private static bool Reach(TheTrailWalker.Domain.Crafting.RecipeDefinition r, ICollection<int> regions, int tier, TheTrailWalker.Domain.Crafting.CraftingState c, out string why) =>
            TheTrailWalker.Domain.Crafting.RecipeSources.Reachable(r, regions, tier, c, out why);
        private static bool Reach(string id, ICollection<int> regions, int tier, TheTrailWalker.Domain.Crafting.CraftingState c, out string why) =>
            TheTrailWalker.Domain.Crafting.RecipeSources.Reachable(TheTrailWalker.Domain.Crafting.RecipeBook.Find(id), regions, tier, c, out why);

        private static void GateRawRegion()
        {
            var c = KnowIds(AllMakers());
            Check(Reach("recipe.food.apple_glazed_chicken", new[] { 1 }, 1, c, out string a) && a == "", "GateRawRegion: reachable with its regions");
            Check(!Reach("recipe.food.apple_glazed_chicken", new[] { 2, 3, 4 }, 1, c, out string b) && b.Contains("food.apples"), "GateRawRegion: unreachable with region 1 removed: " + b);
            Check(!Reach("recipe.food.honey_roast_duck", AllRegions(), 1, c, out string t) && t.StartsWith("Tier 2"), "GateRawRegion: the tier is checked first: " + t);
        }

        private static void GateRefined()
        {
            string wf = "recipe.material.wheat_flour";
            var without = KnowIds(AllMakers().Where(x => x != wf));
            Check(Reach("recipe.food.chicken_mushroom_pie", Regions1To4, 1, KnowIds(AllMakers()), out _), "GateRefined: reachable with the maker recipe known");
            Check(!Reach("recipe.food.chicken_mushroom_pie", Regions1To4, 1, without, out string why) && why.Contains("wheat_flour"), "GateRefined: unreachable without it: " + why);
            // Hemp cord has a maker and the tailor sells it from region 2: a shop stands in for the maker recipe.
            var cordUser = Synthetic("recipe.misc.test_cord", 1, "item.hemp_cord");
            string cord = TheTrailWalker.Domain.Economy.ItemPrices.Maker("item.hemp_cord").Id;
            var noCord = KnowIds(AllMakers().Where(x => x != cord));
            Check(!Reach(cordUser, new[] { 1 }, 1, noCord, out string w2) && w2.Contains("hemp_cord"), "GateRefined: no maker and no shop: " + w2);
            Check(Reach(cordUser, new[] { 2 }, 1, noCord, out _), "GateRefined: reachable again when a shop sells the input");
        }

        private static TheTrailWalker.Domain.Crafting.RecipeDefinition Synthetic(string id, int tier, params string[] inputs) =>
            new TheTrailWalker.Domain.Crafting.RecipeDefinition(id, "t", TheTrailWalker.Domain.Crafting.RecipeRole.Misc, tier, "item.test_output", 1, 10, "", inputs.Select(i => new TheTrailWalker.Domain.Crafting.RecipeInput(i, 1)).ToArray());

        private static void GateBarrel()
        {
            // Synthetic recipes isolate each rule: the real filling inputs (oak planks, fittings) have no makers yet.
            string fill = "recipe.barrel.apple_cider";
            var bottling = Synthetic("recipe.food.test_bottling", 1, "item.apple_cider_barrel");
            Check(Reach(bottling, AllRegions(), 4, KnowIds(AllMakers().Append(fill)), out _), "GateBarrel: bottling reachable with its filling known");
            Check(!Reach(bottling, AllRegions(), 4, KnowIds(AllMakers()), out string a) && a.Contains("apple_cider_barrel"), "GateBarrel: bottling needs its filling: " + a);
            var filling = Synthetic("recipe.barrel.test_filling", 1, "food.apples");
            Check(!Reach(filling, AllRegions(), 4, KnowIds(AllMakers()), out string b) && b.Contains("recipe.material.oak_plank"), "GateBarrel: filling needs the oak plank recipe: " + b);
            Check(!Reach("recipe.barrel.apple_cider", AllRegions(), 4, KnowIds(AllMakers()), out string c) && c != "", "GateBarrel: the real filling is unreachable while its inputs have no source: " + c);
        }

        private static void GateIgnoresBagAndGold()
        {
            // Reachable takes no bag or wallet, so held items or gold cannot matter; only a known maker or a shop counts.
            var none = new TheTrailWalker.Domain.Crafting.CraftingState();
            Check(!Reach("recipe.food.chicken_mushroom_pie", Regions1To4, 1, none, out string why) && why.Contains("wheat_flour"), "GateIgnoresBagAndGold: a held refined input does not count: " + why);
        }

        private static void BakeryUnchanged()
        {
            var c = new TheTrailWalker.Domain.Crafting.CraftingState();
            Check(!TheTrailWalker.Domain.Settlement.BakeryGate.Eligible("recipe.food.premium_apple_pie", new[] { 1, 6 }, 4, c, out string why) && why.Contains("wheat_flour"), "BakeryUnchanged: " + why);
        }
            // ---- Task 5: balloon session, offers, purchases and box manifests.
        private static readonly int[] R1To6 = { 1, 2, 3, 4, 5, 6 };
        private const string Host1 = "camp.01.05";
        public static string OfferTierSplit = "", SlotSplit = "";
        private static SpatialInventory FullBag() => new SpatialInventory(100, 100);
        private static SpatialInventory BigBag() => new SpatialInventory(200000, 200000);
        private static string BagSnap(SpatialInventory bag) => string.Join(";", bag.Entries.Select(e => e.Id + "/" + e.Definition.Id + "/" + e.Quantity + "/" + e.Owner + "/" + e.X + "/" + e.Y));
        private static Func<string> NewIds(string prefix) { int n = 0; return () => prefix + n++; }
        private static CraftingState Know(params string[] ids) => KnowIds(ids);
        private static int TierOf(string specialId) => specialId[9] - '0';
        private static MysteryBoxContext Ctx(CraftingState c, int seed, ICollection<int> regions = null, int tier = 1, params string[] missions) =>
            new MysteryBoxContext { Regions = regions ?? R1To6, ReachedTier = tier, Crafting = c, MissionRequests = missions, Rng = new System.Random(seed) };

        // A new session at the tier-one host, then the box offer bought.
        private static BoxManifest BuyBox(BalloonState s, MysteryBoxContext ctx, Wallet w = null)
        {
            BalloonShop.Close(s); BalloonShop.Open(s, Host1, ctx.Regions, ctx.ReachedTier, ctx.Rng);
            Check(BalloonShop.TryBuyBox(s, 1, s.Offers[1].Revision, w ?? new Wallet(0, 1000), ctx, out BoxManifest box, out string why), "BuyBox: " + why);
            return box;
        }

        private static void OfferShape()
        {
            var s = new BalloonState(); var rng = new System.Random(5); int[] parts = new int[4]; int top = 0, packTop = 0, n = 500;
            string[] names = { "hat", "boots", "shirt", "pants" };
            int[] regions = Enumerable.Range(1, 18).ToArray();
            for (int i = 0; i < n; i++)
            {
                BalloonShop.Close(s); BalloonShop.Open(s, Host1, regions, 3, rng);
                BalloonOffer g = s.Offers[0], b = s.Offers[1], p = s.Offers[2];
                Check(s.Offers.Length == 3 && g.Kind == "garment" && SpecialGear.Is(g.ItemId) && !g.ItemId.EndsWith(".pack") && g.GoldPrice == SpecialGear.GoldPrice(g.ItemId), "OfferShape: slot 0 is a priced special garment");
                Check(b.Kind == "box" && b.BoxTier == BalloonHosts.RegionTier(Host1) && b.GoldPrice == 20, "OfferShape: slot 1 is a box at the host tier");
                Check(p.Kind == "pack" && p.ItemId.EndsWith(".pack") && p.GoldPrice == SpecialGear.GoldPrice(p.ItemId), "OfferShape: slot 2 is a special pack");
                parts[Array.IndexOf(names, g.ItemId.Substring(11))]++;
                if (TierOf(g.ItemId) == 3) top++;
                if (TierOf(p.ItemId) == 3) packTop++;
                Check(TierOf(g.ItemId) <= 3 && TierOf(p.ItemId) <= 3, "OfferShape: never above the reached tier");
            }
            OfferTierSplit = $"garment top-tier {top}/{n}, pack top-tier {packTop}/{n}, garment slots hat/boots/shirt/pants {string.Join("/", parts)}";
            Check(parts.All(c => c >= 95 && c <= 155), "OfferShape: garment slots near-uniform " + OfferTierSplit);
            Check(top >= 300 && top <= 400 && packTop >= 300 && packTop <= 400, "OfferShape: 70/30 tier split " + OfferTierSplit);
            for (int i = 0; i < 50; i++)
            {
                BalloonShop.Close(s); BalloonShop.Open(s, Host1, new[] { 7, 8 }, 2, rng);
                Check(TierOf(s.Offers[0].ItemId) == 2 && TierOf(s.Offers[2].ItemId) == 2, "OfferShape: no earlier tier discovered falls back to the top tier");
                BalloonShop.Close(s); BalloonShop.Open(s, Host1, R1To6, 1, rng);
                Check(TierOf(s.Offers[0].ItemId) == 1 && TierOf(s.Offers[2].ItemId) == 1, "OfferShape: tier one only");
            }
        }

        private static void SessionRerollKeepsPurchases()
        {
            var s = new BalloonState(); var rng = new System.Random(11); var bag = new SpatialInventory(); var w = new Wallet(0, 100000);
            BalloonShop.Open(s, Host1, R1To6, 1, rng);
            long session = s.SessionId; BalloonOffer[] first = s.Offers; string garment = first[0].ItemId; int shown = first[0].Revision;
            BalloonShop.Open(s, Host1, R1To6, 1, rng);
            Check(s.SessionId == session && ReferenceEquals(s.Offers, first), "SessionReroll: a duplicate open keeps the session and offers");
            Check(BalloonShop.TryBuyGear(s, 0, shown, w, bag, NewIds("g"), out string why) && why == "" && first[0].Sold, "SessionReroll: garment bought: " + why);
            Check(bag.Entries.Count(e => e.Definition.Id == garment && e.Owner == ItemOwner.Backpack) == 1 && w.Gold == 100000 - SpecialGear.GoldPrice(garment), "SessionReroll: delivered and charged");
            BalloonShop.Open(s, Host1, R1To6, 1, rng);
            Check(!BalloonShop.TryBuyGear(s, 0, first[0].Revision, w, bag, NewIds("h"), out why) && why == "Sold out", "SessionReroll: the slot stays sold out in the session: " + why);
            BalloonShop.Close(s);
            Check(s.Offers == null && s.OpenCampId == null, "SessionReroll: close ends the session");
            BalloonShop.Open(s, Host1, R1To6, 1, rng);
            Check(s.SessionId == session + 1 && !s.Offers[0].Sold && s.Offers[0].Revision != shown && s.Offers[0].Revision != first[0].Revision, "SessionReroll: reopening rolls a fresh garment with a new revision");
            Check(!BalloonShop.TryBuyGear(s, 0, shown, w, bag, NewIds("i"), out why) && why == "The stock changed; check the offer again", "SessionReroll: an old session's revision cannot buy the new offer: " + why);
            Check(bag.Entries.Count(e => e.Definition.Id == garment && e.Owner == ItemOwner.Backpack) == 1, "SessionReroll: the bought gear is unaffected");
        }

        private static void GearFullBagRefused()
        {
            var s = new BalloonState(); BalloonShop.Open(s, Host1, R1To6, 1, new System.Random(3));
            var bag = FullBag(); var w = new Wallet(0, 100000); string snap = BagSnap(bag);
            foreach (int slot in new[] { 0, 2 })
                Check(!BalloonShop.TryBuyGear(s, slot, s.Offers[slot].Revision, w, bag, NewIds("f"), out string why) && why == "Not enough room in the bag" && w.Gold == 100000 && !s.Offers[slot].Sold && BagSnap(bag) == snap, "GearFullBagRefused: slot " + slot + " " + why);
            var poor = new Wallet(0, s.Offers[0].GoldPrice - 1);
            Check(!BalloonShop.TryBuyGear(s, 0, s.Offers[0].Revision, poor, new SpatialInventory(), NewIds("p"), out string r2) && r2 == "Not enough gold" && poor.Gold == s.Offers[0].GoldPrice - 1, "GearFullBagRefused: gold short " + r2);
            Check(!BalloonShop.TryBuyGear(s, 1, s.Offers[1].Revision, w, new SpatialInventory(), NewIds("p"), out _) && !s.Offers[1].Sold, "GearFullBagRefused: the box slot is not gear");
        }

        private static void BoxFullBagWaits()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers()); var w = new Wallet(0, 20);
            BoxManifest box = BuyBox(s, Ctx(c, 7), w);
            Check(w.Gold == 0 && s.Boxes.Count == 1 && s.Boxes[0] == box && box.Entries.Length == 6 && s.Offers[1].Sold, "BoxFullBagWaits: box bought for 20 gold");
            var bag = FullBag(); string snap = BagSnap(bag);
            for (int i = 0; i < 6; i++)
            {
                if (box.Entries[i].Kind != "item") continue;
                Check(!MysteryBoxes.Claim(s, box.BoxId, i, bag, c, NewIds("c"), out string note) && note == "Reward waiting in the box: make room in the bag" && !box.Entries[i].Claimed && box.Entries[i].ClaimTxn == null, "BoxFullBagWaits: entry " + i + " waits: " + note);
            }
            Check(BagSnap(bag) == snap, "BoxFullBagWaits: the bag is untouched");
            Check(!BalloonShop.TryBuyBox(s, 1, s.Offers[1].Revision, new Wallet(0, 100), Ctx(c, 8), out _, out string again) && again == "Sold out" && s.Boxes.Count == 1, "BoxFullBagWaits: one box per session: " + again);
        }

        private static void BoxPrice()
        {
            long[] want = { 20, 20, 25, 30 };
            for (int t = 1; t <= 4; t++) Check(BalloonShop.BoxPrice(t) == want[t - 1], "BoxPrice: tier " + t);
            // Hosts are all tier one today, so a tier-four box offer is set by hand on an open session.
            var s = new BalloonState(); var c = KnowIds(AllMakers());
            BalloonShop.Open(s, Host1, AllRegions(), 4, new System.Random(1));
            int rev = s.Offers[1].Revision;
            s.Offers[1] = new BalloonOffer { Kind = "box", BoxTier = 4, GoldPrice = BalloonShop.BoxPrice(4), Revision = rev };
            var w = new Wallet(0, 29);
            Check(!BalloonShop.TryBuyBox(s, 1, rev, w, Ctx(c, 2, AllRegions(), 4), out _, out string why) && why == "Not enough gold" && s.Boxes.Count == 0 && w.Gold == 29, "BoxPrice: 29 gold cannot buy a tier-4 box " + why);
            w = new Wallet(0, 30);
            Check(BalloonShop.TryBuyBox(s, 1, rev, w, Ctx(c, 2, AllRegions(), 4), out BoxManifest box, out why) && w.Gold == 0 && box.Tier == 4, "BoxPrice: 30 gold buys it " + why);
        }

        private static void ManifestShape()
        {
            var c = KnowIds(AllMakers());
            BoxManifest m = BuyBox(new BalloonState(), Ctx(c, 4, null, 1, "material.hemp"));
            Check(m.Entries[0].Kind == "item" && m.Entries[0].ItemId == "material.hemp" && m.Entries[0].Units == 1000, "ManifestShape: slot 1 is the mission item " + m.Entries[0].ItemId);
            var regionOne = new HashSet<string>(ItemSources.LooseIds.Where(id => ItemSources.LooseRegions(id).Contains(1))
                .Concat(ItemSources.HarvestIds.Where(id => ItemSources.TryHarvest(id, out _, out int[] r) && r.Contains(1))));
            foreach (string seafood in new[] { "ingredient.shrimp", "fishery.crab" })
            {
                BoxManifest f = BuyBox(new BalloonState(), Ctx(c, 4, null, 1, seafood));
                Check(regionOne.Contains(f.Entries[0].ItemId) && Items.Find(f.Entries[0].ItemId).Category == ItemCategory.Material && f.Entries[0].Units == 1000, "ManifestShape: fishery seafood falls back to a region material: " + f.Entries[0].ItemId);
            }
            for (int i = 0; i < 20; i++)
                Check(regionOne.Contains(BuyBox(new BalloonState(), Ctx(c, 100 + i)).Entries[0].ItemId), "ManifestShape: no mission means a current-region material");

            var fishery = new HashSet<string>(ShopCatalog.All.Where(x => x.ShopId == ShopCatalog.Fishery && x.Role == ShopSlotRole.Exclusive).Select(x => x.ItemId));
            int mat = 0, tool = 0, gear = 0, recipe = 0; var shared = new System.Random(1000); // one stream: consecutive seeds correlate in System.Random
            for (int i = 0; i < 2000; i++)
            {
                var s = new BalloonState(); MysteryBoxContext bc = Ctx(c, 0); bc.Rng = shared; BoxManifest b = BuyBox(s, bc);
                Check(b.Entries.Length == 6 && b.Tier == 1, "ManifestShape: six entries");
                var ids = b.Entries.Where(e => e.Kind == "recipe").Select(e => e.RecipeId).ToList();
                Check(ids.Distinct().Count() == ids.Count, "ManifestShape: no recipe twice in a box");
                foreach (BoxEntry e in b.Entries)
                {
                    if (e.Kind == "recipe")
                    {
                        RecipeDefinition r = RecipeBook.Find(e.RecipeId); ItemDefinition fb = Items.Find(e.FallbackItemId);
                        // Regions 1-6 at reached tier 1: the player's top tier is 1, so no recipe above it.
                        Check(r != null && r.MysteryBoxEligible && r.Tier <= 1 && !c.Knows(r.Id) && fb != null && fb.Stackable && !fishery.Contains(fb.Id) && e.FallbackUnits == 1000 && e.ItemId == null
                            && e.FallbackProfile.Equals(ItemPrices.Profile(fb, 5)), "ManifestShape: recipe entry with a persisted Q5 fallback " + e.RecipeId);
                        continue;
                    }
                    ItemDefinition d = Items.Find(e.ItemId);
                    Check(e.Kind == "item" && d != null && !SpecialGear.Is(d.Id) && e.Units == (d.Stackable ? 1000 : 1) && !d.Id.StartsWith("debug.") && e.RecipeId == null && e.Profile.Equals(ItemPrices.Profile(d, 5)), "ManifestShape: physical entry with its Q5 profile " + e.ItemId);
                    Check(!fishery.Contains(d.Id), "ManifestShape: never fishery seafood " + d.Id);
                }
                for (int k = 1; k < 6; k++)
                {
                    BoxEntry e = b.Entries[k];
                    if (e.Kind == "recipe") recipe++;
                    else { ItemCategory cat = Items.Find(e.ItemId).Category; if (cat == ItemCategory.Tool) tool++; else if (cat == ItemCategory.Wearable) gear++; else mat++; }
                }
                if (i >= 30) continue;
                var big = BigBag(); var claimer = KnowIds(AllMakers());
                for (int k = 0; k < 6; k++)
                {
                    Check(MysteryBoxes.Claim(s, b.BoxId, k, big, claimer, NewIds("q" + k + "_"), out string note), "ManifestShape: claim " + k + " " + note);
                    if (b.Entries[k].Kind == "recipe") continue;
                    ItemEntry got = big.Entries.Last(x => x.Definition.Id == b.Entries[k].ItemId && x.Owner == ItemOwner.Backpack);
                    Check(got.Quality == 5 && got.Profile.Equals(ItemPrices.Profile(got.Definition, 5)), "ManifestShape: Q5 " + got.Definition.Id);
                }
            }
            SlotSplit = $"materials/food {mat}, tool {tool}, clothing/backpack {gear}, recipe {recipe} of 10000 slots";
            Check(mat >= 7700 && mat <= 8300 && tool >= 850 && tool <= 1150 && gear >= 400 && gear <= 600 && recipe >= 400 && recipe <= 600, "ManifestShape: 80/10/5/5 " + SlotSplit);
        }

        private static void ReservedAcrossBoxesAndBakery()
        {
            // Leave exactly one tier-one box recipe eligible, then roll until a box reserves it.
            var c = KnowIds(AllMakers());
            List<TheTrailWalker.Domain.Crafting.RecipeDefinition> open = BoxPool().Where(r => r.Tier == 1 && Reach(r, R1To6, 1, c, out _)).ToList();
            Check(open.Count >= 2, "Reserved: setup has eligible tier-one recipes (" + open.Count + ")");
            string target = open[0].Id;
            foreach (var r in open.Skip(1)) c.Learn(r.Id);
            var s = new BalloonState(); BoxManifest first = null;
            for (int i = 0; i < 400 && first == null; i++) { BoxManifest b = BuyBox(s, Ctx(c, 5000 + i)); if (b.Entries.Any(e => e.RecipeId == target)) first = b; }
            Check(first != null && MysteryBoxes.Reserved(s).SetEquals(new[] { target }), "Reserved: a box reserves the recipe");
            int before = s.Boxes.Count;
            for (int i = 0; i < 300; i++) BuyBox(s, Ctx(c, 9000 + i));
            Check(s.Boxes.Skip(before).All(b => b.Entries.All(e => e.RecipeId != target)), "Reserved: later boxes exclude the unclaimed reservation");
            int idx = Array.FindIndex(first.Entries, e => e.RecipeId == target);
            Check(MysteryBoxes.Claim(s, first.BoxId, idx, FullBag(), c, NewIds("r"), out string note) && c.Knows(target) && c.UnitsOf(target) == 0 && note == "Recipe learned: " + RecipeBook.Find(target).Name && !MysteryBoxes.Reserved(s).Contains(target),
                "Reserved: claiming learns it once with no bag room: " + note);

            // The bakery excludes recipes reserved in unclaimed boxes. The data has no overlap yet, so a hand-built box pins the rule.
            string pie = "recipe.food.premium_apple_pie";
            var bake = Know("recipe.material.wheat_flour", "recipe.food.cow_butter", "recipe.food.cow_cream");
            var held = new BalloonState();
            held.Boxes.Add(new BoxManifest { BoxId = "box.t", Tier = 1, Entries = new[] { new BoxEntry { Kind = "recipe", RecipeId = pie, FallbackItemId = "food.apples", FallbackUnits = 1000 } } });
            ISet<string> reserved = MysteryBoxes.Reserved(held); string why = "";
            Check(BakeryGate.Eligible(pie, new[] { 1, 6 }, 1, bake, out _) && !BakeryGate.Eligible(pie, new[] { 1, 6 }, 1, bake, out why, reserved) && why == "Reserved in a mystery box", "Reserved: the bakery gate excludes it: " + why);
            bool offered = false, offeredFree = false;
            for (int v = 0; v < 600; v++)
            {
                offered |= ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Bakery, R1To6, bake, reserved)[2].RecipeId == pie;
                offeredFree |= ShopService.Offers(new ShopState { VisitId = v }, ShopCatalog.Bakery, R1To6, bake)[2].RecipeId == pie;
            }
            Check(!offered && offeredFree, "Reserved: the bakery roll never offers a reserved recipe (unreserved offered: " + offeredFree + ")");
            var fs = new ShopState { VisitId = 1 }; var fake = new ShopOffer { ItemId = "item.premium_apple_pie", RecipeId = pie, UnitPriceMinor = 80, UnitsLeft = 1, Currency = Currency.Gold };
            fs.Stock[ShopCatalog.Bakery] = (1, new[] { new ShopOffer(), new ShopOffer(), fake }); var rich = new Wallet(0, 1000);
            Check(!ShopService.TryBuyRecipe(fs, 2, 0, rich, bake, R1To6, out why, reserved) && why == "This recipe isn't available yet" && rich.Gold == 1000 && !bake.Knows(pie), "Reserved: PAY refuses a reserved bakery recipe: " + why);
        }

        private static void RecipeKnownBeforeClaimGivesFallback()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers()); BoxManifest box = null; int idx = -1;
            for (int i = 0; i < 400 && idx < 0; i++) { box = BuyBox(s, Ctx(c, 20000 + i)); idx = Array.FindIndex(box.Entries, e => e.Kind == "recipe"); }
            Check(idx >= 0, "RecipeKnownBeforeClaim: setup rolled a recipe");
            BoxEntry e0 = box.Entries[idx]; c.Learn(e0.RecipeId); var bag = BigBag();
            Check(!MysteryBoxes.Claim(s, box.BoxId, idx, FullBag(), c, NewIds("x"), out string note) && note == "Reward waiting in the box: make room in the bag" && !e0.Claimed, "RecipeKnownBeforeClaim: the fallback needs room: " + note);
            Check(MysteryBoxes.Claim(s, box.BoxId, idx, bag, c, NewIds("y"), out note) && e0.Claimed && e0.ClaimTxn != null, "RecipeKnownBeforeClaim: claim " + note);
            Check(bag.Entries.Where(x => x.Definition.Id == e0.FallbackItemId && x.Owner == ItemOwner.Backpack).Sum(x => x.Quantity) == e0.FallbackUnits && c.UnitsOf(e0.RecipeId) == 0, "RecipeKnownBeforeClaim: the persisted fallback is granted");
        }

        private static void ClaimOnceByTxn()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers()); BoxManifest box = BuyBox(s, Ctx(c, 31));
            var bag = BigBag(); string note = "";
            for (int i = 0; i < 6; i++)
            {
                Check(MysteryBoxes.Claim(s, box.BoxId, i, bag, c, NewIds("t" + i + "_"), out note) && box.Entries[i].Claimed && box.Entries[i].ClaimTxn != null, "ClaimOnceByTxn: first claim " + i + " " + note);
                string snap = BagSnap(bag); int known = c.Known.Count; string txn = box.Entries[i].ClaimTxn;
                Check(!MysteryBoxes.Claim(s, box.BoxId, i, bag, c, NewIds("u"), out _) && BagSnap(bag) == snap && c.Known.Count == known && box.Entries[i].ClaimTxn == txn, "ClaimOnceByTxn: second claim grants nothing " + i);
            }
            Check(note == "All rewards claimed" || note.StartsWith("Recipe learned: "), "ClaimOnceByTxn: last note " + note);
            Check(!MysteryBoxes.Claim(s, box.BoxId, 0, bag, c, NewIds("v"), out note) && note == "All rewards claimed", "ClaimOnceByTxn: all claimed note " + note);
            var txnOnly = new BalloonState();
            txnOnly.Boxes.Add(new BoxManifest { BoxId = "b", Tier = 1, Entries = new[] { new BoxEntry { Kind = "item", ItemId = "food.apples", Units = 1000, ClaimTxn = "done" } } });
            string snap2 = BagSnap(bag);
            Check(!MysteryBoxes.Claim(txnOnly, "b", 0, bag, c, NewIds("w"), out _) && BagSnap(bag) == snap2, "ClaimOnceByTxn: a recorded txn alone blocks a grant");
            // The claim stores the profile persisted at roll time, not today's price.
            var priced = new BalloonState();
            priced.Boxes.Add(new BoxManifest { BoxId = "p", Tier = 1, Entries = new[] { new BoxEntry { Kind = "item", ItemId = "food.apples", Units = 1000, Profile = new ItemProfile(12345) } } });
            var pbag = BigBag();
            Check(MysteryBoxes.Claim(priced, "p", 0, pbag, c, NewIds("pp"), out _) && pbag.Entries.Single().Profile.UnitValue == 12345 && pbag.Entries.Single().Quality == 5, "ClaimOnceByTxn: the persisted profile is granted");
        }

        private static void StaleOfferRefused()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers()); BalloonShop.Open(s, Host1, R1To6, 1, new System.Random(9)); var w = new Wallet(0, 100000);
            Check(!BalloonShop.TryBuyGear(s, 0, s.Offers[0].Revision + 1, w, new SpatialInventory(), NewIds("s"), out string why) && why == "The stock changed; check the offer again" && w.Gold == 100000, "StaleOfferRefused: gear " + why);
            Check(!BalloonShop.TryBuyBox(s, 1, s.Offers[1].Revision + 1, w, Ctx(c, 1), out _, out why) && why == "The stock changed; check the offer again" && s.Boxes.Count == 0 && w.Gold == 100000, "StaleOfferRefused: box " + why);
            int rev = s.Offers[0].Revision; BalloonShop.Close(s);
            Check(!BalloonShop.TryBuyGear(s, 0, rev, w, new SpatialInventory(), NewIds("s"), out why) && why == "The stock changed; check the offer again", "StaleOfferRefused: a closed session " + why);
        }
            public static string PlayerTierSplit = "";

        // The box colour/price tier is the host's (1 today); contents follow the player's current tier, 70% at it.
        private static void BoxFollowsPlayerTier()
        {
            int[] r12 = Enumerable.Range(1, 12).ToArray();
            var c = KnowIds(AllMakers()); var shared = new System.Random(77);
            var tierOne = new HashSet<string>(ItemSources.LooseIds.Concat(ItemSources.HarvestIds).Concat(ShopCatalog.All.Where(e => e.Tier == 1).Select(e => e.ItemId)));
            var tierTwoRecipes = new HashSet<string>(BoxPool().Where(r => r.Tier == 2 && Reach(r, r12, 2, c, out _)).Select(r => r.Id));
            int hi = 0, lo = 0, t2Recipes = 0;
            for (int i = 0; i < 2000; i++)
            {
                var s = new BalloonState(); MysteryBoxContext bc = Ctx(c, 0, r12, 2); bc.Rng = shared; var w = new Wallet(0, 20);
                BoxManifest b = BuyBox(s, bc, w);
                Check(b.Tier == 1 && w.Gold == 0, "BoxFollowsPlayerTier: still a tier-1 box at 20 gold");
                for (int k = 1; k < 6; k++)
                {
                    BoxEntry e = b.Entries[k];
                    if (e.Kind == "recipe") { if (tierTwoRecipes.Contains(e.RecipeId)) t2Recipes++; continue; }
                    if (!Items.Find(e.ItemId).Stackable) continue;
                    if (tierOne.Contains(e.ItemId)) lo++; else hi++;
                }
            }
            PlayerTierSplit = $"tier-2 stackables {hi}, earlier {lo} ({100.0 * hi / (hi + lo):F1}%), tier-2 recipes {t2Recipes} (eligible tier-2 recipes {tierTwoRecipes.Count})";
            Check(hi * 100 >= (hi + lo) * 65 && hi * 100 <= (hi + lo) * 75, "BoxFollowsPlayerTier: about 70% tier-2 stackables " + PlayerTierSplit);
            Check(tierTwoRecipes.Count == 0 || t2Recipes > 0, "BoxFollowsPlayerTier: tier-2 recipes can drop " + PlayerTierSplit);
        }

        // ---- Task 6: the balloon save block ----
        private static BalloonState Back(BalloonState s) =>
            SessionSaveStore.RestoreBalloon(JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(new SessionSnapshot { balloon = SessionSaveStore.SaveBalloon(s) })));
        private static bool Throws(Action<SavedBalloon> tamper, BalloonState s)
        {
            SavedBalloon sv = SessionSaveStore.SaveBalloon(s); tamper(sv);
            try { SessionSaveStore.RestoreBalloon(new SessionSnapshot { balloon = sv }); return false; } catch (System.IO.InvalidDataException) { return true; }
        }
        private static string EntrySig(BoxEntry e) => string.Join("|", e.Kind, e.ItemId, e.Units, e.RecipeId, e.FallbackItemId, e.FallbackUnits, e.Claimed, e.ClaimTxn,
            e.Profile.UnitValue, e.Profile.MaxDurability, e.Profile.Durability, e.Profile.Variant, e.FallbackProfile.UnitValue, e.FallbackProfile.MaxDurability, e.FallbackProfile.Durability, e.FallbackProfile.Variant);
        private static string BoxSig(BoxManifest b) => b.BoxId + "/" + b.Tier + ":" + string.Join(";", b.Entries.Select(EntrySig));

        private static void BoxChargeAndManifestAtomic()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers()); var w = new Wallet(0, 100);
            BalloonShop.Open(s, Host1, R1To6, 1, new System.Random(5));
            BalloonState before = Back(s);   // what a kill before the save would leave: neither the charge nor the box
            Check(BalloonShop.TryBuyBox(s, 1, s.Offers[1].Revision, w, Ctx(c, 5), out BoxManifest box, out _) && w.Gold == 80 && s.Boxes.Count == 1, "BoxChargeAndManifestAtomic: charge and box together");
            BalloonState after = Back(s);
            Check(before.Boxes.Count == 0 && !before.Offers[1].Sold && after.Boxes.Count == 1 && after.Offers[1].Sold && BoxSig(after.Boxes[0]) == BoxSig(box) && after.Boxes[0].Entries.Length == 6, "BoxChargeAndManifestAtomic: the reload has the whole manifest or none");
            var poor = new Wallet(0, 5);
            Check(!BalloonShop.TryBuyBox(before, 1, before.Offers[1].Revision, poor, Ctx(c, 6), out _, out _) && poor.Gold == 5 && before.Boxes.Count == 0, "BoxChargeAndManifestAtomic: a refusal changes nothing");
        }

        private static void ClaimSurvivesReload()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers()); BoxManifest box = null;
            for (int i = 0; i < 400; i++) { s = new BalloonState(); box = BuyBox(s, Ctx(c, 40000 + i)); if (box.Entries.Any(e => e.Kind == "recipe")) break; }
            Check(box.Entries.Any(e => e.Kind == "recipe"), "ClaimSurvivesReload: setup rolled a recipe");
            var bag = BigBag();
            for (int i = 0; i < 3; i++) Check(MysteryBoxes.Claim(s, box.BoxId, i, bag, c, NewIds("k" + i + "_"), out _), "ClaimSurvivesReload: claim " + i);
            BalloonState back = Back(s); BoxManifest b2 = back.Boxes[0];
            Check(BoxSig(b2) == BoxSig(box), "ClaimSurvivesReload: every field survives");
            string snap = BagSnap(bag);
            for (int i = 0; i < 3; i++) Check(!MysteryBoxes.Claim(back, b2.BoxId, i, bag, c, NewIds("again"), out _), "ClaimSurvivesReload: a claimed entry is not repeated " + i);
            Check(BagSnap(bag) == snap, "ClaimSurvivesReload: nothing granted again");
            for (int i = 3; i < 6; i++) Check(!b2.Entries[i].Claimed && MysteryBoxes.Claim(back, b2.BoxId, i, bag, c, NewIds("n" + i + "_"), out _), "ClaimSurvivesReload: unclaimed stays claimable " + i);
            Check(back.Boxes[0].Entries.All(e => e.Claimed && e.ClaimTxn != null), "ClaimSurvivesReload: all claimed");
        }

        private static string SessionSig(BalloonState s) => s.SessionId + "/" + s.OpenCampId + "/" + (s.Offers == null ? "-" : string.Join(";", s.Offers.Select(o => o.Kind + "," + o.ItemId + "," + o.BoxTier + "," + o.GoldPrice + "," + o.Sold + "," + o.Revision)));

        private static void OpenSessionSurvivesReload()
        {
            var s = new BalloonState();
            BalloonShop.Open(s, Host1, R1To6, 1, new System.Random(12));
            Check(BalloonShop.TryBuyGear(s, 0, s.Offers[0].Revision, new Wallet(0, 100000), BigBag(), NewIds("g"), out _), "OpenSessionSurvivesReload: setup bought gear");
            string sig = SessionSig(s); BalloonState back = Back(s);
            Check(SessionSig(back) == sig && back.Offers[0].Sold, "OpenSessionSurvivesReload: same offers and revisions");
            BalloonShop.Open(back, Host1, R1To6, 1, new System.Random(99));
            Check(SessionSig(back) == sig, "OpenSessionSurvivesReload: reopening does not reroll");
            BalloonShop.Close(s); BalloonState closed = Back(s);
            Check(closed.Offers == null && closed.OpenCampId == null && closed.SessionId == s.SessionId, "OpenSessionSurvivesReload: a closed session stays closed");
        }

        private static void OldSaveLoads()
        {
            BalloonState s = SessionSaveStore.RestoreBalloon(new SessionSnapshot());
            Check(s != null && s.SessionId == 0 && s.Offers == null && s.Boxes.Count == 0, "OldSaveLoads: a save without balloon");
            s = SessionSaveStore.RestoreBalloon(new SessionSnapshot { balloon = new SavedBalloon() });
            Check(s.SessionId == 0 && s.Boxes.Count == 0, "OldSaveLoads: block 0");
        }

        private static void MalformedBalloonRejected()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers());
            BalloonShop.Open(s, Host1, R1To6, 1, new System.Random(3));
            Check(BalloonShop.TryBuyBox(s, 1, s.Offers[1].Revision, new Wallet(0, 100), Ctx(c, 3), out _, out _), "MalformedBalloonRejected: setup");
            Check(Throws(v => v.offers[0].goldPrice = -1, s), "MalformedBalloonRejected: negative gold");
            Check(Throws(v => v.boxes[0].entries[0].units = -1, s), "MalformedBalloonRejected: negative units");
            Check(Throws(v => v.boxes[0].entries[0].fallbackUnits = -1, s), "MalformedBalloonRejected: negative fallback units");
            Check(Throws(v => v.boxes[0].entries[0].profile.unitValue = -1, s), "MalformedBalloonRejected: negative value");
            Check(Throws(v => { v.boxes[0].entries[0].claimed = true; v.boxes[0].entries[0].claimTxn = ""; }, s), "MalformedBalloonRejected: claimed without txn");
            Check(Throws(v => v.boxes.Add(v.boxes[0]), s), "MalformedBalloonRejected: duplicate box id");
            Check(Throws(v => v.sessionId = -1, s), "MalformedBalloonRejected: negative session");
            Check(Throws(v => v.boxes[0].entries.RemoveAt(5), s), "MalformedBalloonRejected: entry count");
        }

        private static void SessionIdFloor()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers());
            BuyBox(s, Ctx(c, 1)); BuyBox(s, Ctx(c, 2));
            SavedBalloon sv = SessionSaveStore.SaveBalloon(s); sv.sessionId = 0; sv.offers.Clear(); sv.openCampId = null;
            BalloonState back = SessionSaveStore.RestoreBalloon(new SessionSnapshot { balloon = sv });
            long top = back.Boxes.Max(b => long.Parse(b.BoxId.Substring(4)));
            Check(back.SessionId >= top && back.Boxes.Count == 2, "SessionIdFloor: raised on load, not failed");
            BalloonShop.Open(back, Host1, R1To6, 1, new System.Random(1));
            Check(BalloonShop.TryBuyBox(back, 1, back.Offers[1].Revision, new Wallet(0, 100), Ctx(c, 8), out _, out _) && back.Boxes.Select(b => b.BoxId).Distinct().Count() == 3, "SessionIdFloor: a new box never reuses an id");
        }

        private static void MissingItemsStillLoad()
        {
            var s = new BalloonState(); var c = KnowIds(AllMakers()); BoxManifest box = null;
            for (int i = 0; i < 400; i++) { s = new BalloonState(); box = BuyBox(s, Ctx(c, 50000 + i)); if (box.Entries.Any(e => e.Kind == "recipe")) break; }
            int r = Array.FindIndex(box.Entries, e => e.Kind == "recipe");
            Check(r >= 0 && r != 1, "MissingItemsStillLoad: setup");
            // All profile fields survive.
            box.Entries[1].Profile = new ItemProfile(777, 30, 12, "v1"); box.Entries[r].FallbackProfile = new ItemProfile(555, 9, 3, "v2");
            Check(BoxSig(Back(s).Boxes[0]) == BoxSig(box), "MissingItemsStillLoad: profile fields round-trip");
            long total = box.Entries[1].Profile.UnitValue * box.Entries[1].Units;
            SavedBalloon sv = SessionSaveStore.SaveBalloon(s);
            sv.boxes[0].entries[1].itemId = "gone.item"; sv.boxes[0].entries[r].fallbackItemId = "gone.too"; sv.boxes[0].entries[r].recipeId = "gone.recipe";
            BalloonState back = SessionSaveStore.RestoreBalloon(new SessionSnapshot { balloon = sv }); BoxManifest b = back.Boxes[0];
            BoxEntry mapped = b.Entries[1], fb = b.Entries[r];
            // One region-1 stack of 1-1,000 units that fits an empty bag; the value is at least the saved value only when the 1,000 cap did not bite.
            bool Stand(string id, ItemProfile p, int u, long saved) { ItemDefinition d = Items.Find(id); return d != null && d.Stackable && ItemSources.LooseRegions(id).Contains(1) && u >= 1 && u <= 1000 && p.UnitValue > 0
                && ShopService.StoreFits(new SpatialInventory(), d, p, u) && (u == 1000 || p.UnitValue * u >= saved); }
            Check(Stand(mapped.ItemId, mapped.Profile, mapped.Units, total), "MissingItemsStillLoad: item mapped to a region-1 stack " + mapped.ItemId + " x" + mapped.Units);
            Check(Stand(fb.FallbackItemId, fb.FallbackProfile, fb.FallbackUnits, 555 * 1000) && fb.RecipeId == "gone.recipe", "MissingItemsStillLoad: fallback mapped, recipe kept");
            Check(fb.FallbackUnits == 1000 || fb.FallbackProfile.UnitValue * fb.FallbackUnits >= 555 * 1000, "MissingItemsStillLoad: capped case is the only shortfall");
            SavedBalloon zv = SessionSaveStore.SaveBalloon(s); zv.boxes[0].entries[1].itemId = "gone.item"; zv.boxes[0].entries[1].profile = new SavedProfile(); zv.boxes[0].entries[1].units = 1000;
            Check(SessionSaveStore.RestoreBalloon(new SessionSnapshot { balloon = zv }).Boxes[0].Entries[1].Units == 1, "MissingItemsStillLoad: a zero saved value maps to exactly 1 unit");
            Check(BoxSig(Back(back).Boxes[0]) == BoxSig(b), "MissingItemsStillLoad: the mapping is saved");
            var bag = BigBag();
            Check(MysteryBoxes.Claim(back, b.BoxId, 1, bag, c, NewIds("m"), out _) && MysteryBoxes.Claim(back, b.BoxId, r, bag, c, NewIds("f"), out _), "MissingItemsStillLoad: mapped rewards claim");
            // An unknown garment in the open session is sold out, not a failed load.
            var o = new BalloonState(); BalloonShop.Open(o, Host1, R1To6, 1, new System.Random(4));
            SavedBalloon ov = SessionSaveStore.SaveBalloon(o); ov.offers[0].itemId = "gone.gear";
            BalloonState ob = SessionSaveStore.RestoreBalloon(new SessionSnapshot { balloon = ov });
            Check(ob.Offers[0].Sold && !ob.Offers[1].Sold && !ob.Offers[2].Sold, "MissingItemsStillLoad: unknown offer item is sold out");
        }

        private static void ZeroProfileGuard()
        {
            var c = KnowIds(AllMakers());
            var s = new BalloonState();
            s.Boxes.Add(new BoxManifest { BoxId = "box.1", Tier = 1, Entries = new[] { new BoxEntry { Kind = "item", ItemId = "food.apples", Units = 1000 },
                new BoxEntry { Kind = "recipe", RecipeId = "gone", FallbackItemId = "food.apples", FallbackUnits = 1000 } } });
            var bag = BigBag();
            Check(MysteryBoxes.Claim(s, "box.1", 0, bag, c, NewIds("z"), out _) && MysteryBoxes.Claim(s, "box.1", 1, bag, c, NewIds("y"), out _), "ZeroProfileGuard: claims");
            Check(bag.Entries.Count > 0 && bag.Entries.All(e => e.Profile.UnitValue > 0), "ZeroProfileGuard: nothing stored at zero");
        }

        private static Bounds RendererBox(Component c)
        {
            Renderer[] rs = c.GetComponentsInChildren<Renderer>();
            Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        // Projected bounds in screen pixels, from the viewport so the result matches `screen` whatever the Editor's view size.
        private static Rect ScreenBox(Camera cam, Bounds b, Vector2 screen)
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            for (int c = 0; c < 8; c++)
            {
                Vector3 v = cam.WorldToViewportPoint(b.center + Vector3.Scale(b.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1)));
                x0 = Mathf.Min(x0, v.x * screen.x); x1 = Mathf.Max(x1, v.x * screen.x); y0 = Mathf.Min(y0, v.y * screen.y); y1 = Mathf.Max(y1, v.y * screen.y);
            }
            return Rect.MinMaxRect(x0, y0, x1, y1);
        }

        // Task 7: the balloon interior uses the storefront counter conventions (CustomerCameraAnchor, OfferAnchor1-3, the keeper
        // baked at the origin as Shopkeeper), so the counter pose, tags, lamps and wallet labels carry over. The counter top under
        // every offer anchor is about 1.02 m, and no offer (every special garment and pack, every mystery box) covers the merchant's
        // face from the customer camera: the raw anchor view and the solved counter pose on both phone screens.
        private static void InteriorPrefab()
        {
            var prefab = Resources.Load<GameObject>("Settlement/Balloon/BalloonInterior");
            Check(prefab != null, "InteriorPrefab: Settlement/Balloon/BalloonInterior loads");
            var holder = new GameObject("interior check").transform; holder.position = MovementPrototype.InteriorWorldPosition;
            var cam = new GameObject("interior check camera").AddComponent<Camera>();
            var colliders = new List<Collider>();
            try
            {
                Transform room = UnityEngine.Object.Instantiate(prefab, holder, false).transform;
                Transform eye = room.Find("CustomerCameraAnchor"), keeper = room.Find("Shopkeeper");
                Transform[] anchors = Enumerable.Range(1, 3).Select(i => room.Find("OfferAnchor" + i)).ToArray();
                Check(eye != null && keeper != null && anchors.All(a => a != null), "InteriorPrefab: CustomerCameraAnchor, OfferAnchor1-3 and Shopkeeper");
                Transform head = keeper.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "BalloonMerchantHead");
                Check(head != null && head.GetComponentInChildren<Renderer>() != null && keeper.localPosition == Vector3.zero && keeper.localRotation == Quaternion.identity, "InteriorPrefab: the balloon merchant stands at the origin facing the customer");
                int tris = room.GetComponentsInChildren<MeshFilter>().Where(f => !f.transform.IsChildOf(keeper)).Sum(f => f.sharedMesh.triangles.Length / 3);
                Check(tris > 0 && tris <= 12000, "InteriorPrefab: room triangles " + tris + " (cap 12000)");

                foreach (MeshFilter f in room.GetComponentsInChildren<MeshFilter>())
                    if (!f.transform.IsChildOf(keeper)) { var mc = f.gameObject.AddComponent<MeshCollider>(); mc.sharedMesh = f.sharedMesh; colliders.Add(mc); }
                Physics.SyncTransforms();
                foreach (Transform a in anchors)
                {
                    bool hit = Physics.Raycast(a.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit h, 1f) && colliders.Contains(h.collider);
                    float top = hit ? h.point.y - holder.position.y : -1f;
                    Check(hit && top >= 0.97f && top <= 1.07f && Mathf.Abs(a.position.y - h.point.y) <= 0.02f, "InteriorPrefab: counter top under " + a.name + " " + top.ToString("0.000") + " m, anchor at " + a.localPosition.y.ToString("0.000"));
                }
                foreach (Collider c in colliders) UnityEngine.Object.DestroyImmediate(c);
                colliders.Clear();

                var catalog = Resources.Load<BalloonGoodsArtCatalog>("BalloonGoods/BalloonGoodsArtCatalog");
                Check(catalog != null && catalog.MysteryBoxes.Length > 0, "InteriorPrefab: mystery box art loads");
                foreach (Vector2 screen in new[] { new Vector2(1116, 2480), new Vector2(720, 1680) })
                    foreach (bool solved in new[] { false, true })
                    {
                        cam.fieldOfView = StreetCamera.FieldOfView; cam.aspect = screen.x / screen.y;
                        cam.transform.SetPositionAndRotation(eye.position, eye.rotation);
                        if (solved) { Pose pose = CollectionPrototype.ShopCounterPose(cam, room); cam.transform.SetPositionAndRotation(pose.position, pose.rotation); }
                        Rect face = ScreenBox(cam, RendererBox(head), screen);
                        string view = (solved ? "counter pose" : "anchor view") + " " + screen.x + "x" + screen.y;
                        foreach (Transform a in anchors)
                        {
                            foreach (ItemDefinition d in SpecialGear.All)
                            {
                                GameObject model = CollectionPrototype.PlaceOfferModel(a, d);
                                try { Check(!ScreenBox(cam, RendererBox(model.transform), screen).Overlaps(face), "InteriorPrefab: " + d.Id + " at " + a.name + " covers the merchant's face (" + view + ")"); }
                                finally { UnityEngine.Object.DestroyImmediate(model); }
                            }
                            foreach (BalloonMysteryBoxArt art in catalog.MysteryBoxes)
                            {
                                var box = UnityEngine.Object.Instantiate(art.Prefab, a, false);   // the box at its own size on the anchor
                                try { Check(!ScreenBox(cam, RendererBox(box.transform), screen).Overlaps(face), "InteriorPrefab: tier " + art.Tier + " box at " + a.name + " covers the merchant's face (" + view + ")"); }
                                finally { UnityEngine.Object.DestroyImmediate(box); }
                            }
                        }
                    }
            }
            finally
            {
                foreach (Collider c in colliders) if (c != null) UnityEngine.Object.DestroyImmediate(c);
                UnityEngine.Object.DestroyImmediate(cam.gameObject); UnityEngine.Object.DestroyImmediate(holder.gameObject);
            }
        }

        // Ruling 3: the two tier 1 ranch dishes show their own models (no longer the apple pie and roast rabbit stand-ins), sized
        // like Berry oat cakes (0.8-1.25x its largest side) and within 1.25x Premium apple pie's triangles, as the slice 2 bakes.
        private static void RanchDishModels()
        {
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
            var oat = measure(Resources.Load<GameObject>("Tier01/Crafted/BerryOatCakes")); var pie = measure(Resources.Load<GameObject>("Tier01/Crafted/PremiumApplePie"));
            var attach = typeof(Tier2ShopVisuals).Assembly.GetType("TheTrailWalker.ResourceVisualTuning").GetMethod("Attach", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var (id, path) in new[] { ("item.chicken_mushroom_pie", "Tier01/Crafted/ChickenMushroomPie"), ("item.apple_glazed_chicken", "Tier01/Crafted/AppleGlazedChicken") })
            {
                var prefab = Resources.Load<GameObject>(path);
                Check(prefab != null && Tier2ShopVisuals.PathFor(id) == path, "RanchDishModels: " + id + " maps to its own prefab " + path + " (now " + Tier2ShopVisuals.PathFor(id) + ")");
                var probe = new GameObject("ranch dish probe", typeof(MeshRenderer), typeof(MeshFilter));
                string model;
                try { var visual = (GameObject)attach.Invoke(null, new object[] { probe, Items.Find(id) }); model = visual == null ? null : visual.name.Replace("(Clone)", ""); }
                finally { UnityEngine.Object.DestroyImmediate(probe); }
                Check(model == prefab.name, "RanchDishModels: " + id + " shows " + model + " through the item lookup");
                var m = measure(prefab);
                Check(m.size >= oat.size * .8f && m.size <= oat.size * 1.25f && m.tris <= pie.tris * 1.25f,
                    "RanchDishModels: " + id + " " + m.size.ToString("0.000") + " vs oat cakes " + oat.size.ToString("0.000") + ", " + m.tris + " triangles vs apple pie " + pie.tris);
            }
        }
        // ---------------------------------------------------------------- Task 8: the balloon counter, box opening and reward sheet

        private static void Set(object o, string field, object value) => o.GetType().GetField(field, M14.AnyMember).SetValue(o, value);
        private static T Get<T>(object o, string field) => (T)M14.Member(o, field);
        private static bool Alive(UnityEngine.Object o) => o != null;
        public static string BalloonLayoutReport = "";

        // An edit-mode CollectionPrototype seated at a camp (Milestone14Validation.CampHarness) with a camera on a follow rig, the counter
        // chrome (title, status and the four debug buttons), a stamina state and the user menu. SaveNow is a no-op there (never initialized).
        private static CollectionPrototype BalloonHarness(GameObject root, string campId, out Camera cam)
        {
            CollectionPrototype proto = M14.CampHarness(root, SettlementState.New(7));
            Get<TravelState>(proto, "travel").ArriveAtCamp(campId);
            cam = new GameObject("balloon check camera").AddComponent<Camera>(); cam.transform.SetParent(root.transform, false);
            cam.gameObject.AddComponent<TheTrailWalker.CameraControl.FollowCameraRig>();
            cam.fieldOfView = StreetCamera.FieldOfView; cam.aspect = 1116f / 2480f;
            Set(proto, "cameraView", cam); Set(proto, "stamina", new StaminaState());
            var safe = Get<RectTransform>(proto, "safe");
            Func<string, UnityEngine.UI.Text> line = n => { var t = new GameObject(n, typeof(RectTransform)).AddComponent<UnityEngine.UI.Text>(); t.transform.SetParent(safe, false); return t; };
            Set(proto, "titleLabel", line("title")); Set(proto, "status", line("status"));
            var buttons = Get<List<GameObject>>(proto, "counterHiddenButtons");
            foreach (string b in new[] { "UNLOCK CAMPS", "LEARN T1 RECIPES", "AUTO SORT", "CLAIM OVERFLOW" }) { var go = new GameObject(b, typeof(RectTransform)); go.transform.SetParent(safe, false); buttons.Add(go); }
            M14.Call(proto, "CreateUserMenu");
            return proto;
        }
        private static bool ChromeShown(CollectionPrototype p, bool all)
        {
            var shown = new[] { Get<UnityEngine.UI.Text>(p, "titleLabel").gameObject.activeSelf, Get<UnityEngine.UI.Text>(p, "status").gameObject.activeSelf }.Concat(Get<List<GameObject>>(p, "counterHiddenButtons").Select(g => g.activeSelf));
            return all ? shown.All(x => x) : shown.Any(x => x);
        }

        // Tags, the offer fit rule, framing and the all-pairs layout (Milestone14Validation.CounterLayoutChecks' approach) at both phone sizes,
        // for every special garment with the same tier's box and pack, plus the merchant's face; then the two lantern lights (Ruling 8).
        private static void BalloonViewChecks()
        {
            string hat = Items.Find("special.t1.hat").Name;
            Check(CollectionPrototype.BalloonTagText(new BalloonOffer { Kind = "garment", ItemId = "special.t1.hat", GoldPrice = 150 }) == hat + "\n150 gold"
                && CollectionPrototype.BalloonTagText(new BalloonOffer { Kind = "box", BoxTier = 1, GoldPrice = 20 }) == "Mystery box\n20 gold"
                && CollectionPrototype.BalloonTagText(new BalloonOffer { Kind = "pack", ItemId = "special.t4.pack", GoldPrice = 3200 }) == Items.Find("special.t4.pack").Name + "\n3,200 gold"
                && CollectionPrototype.BalloonTagText(new BalloonOffer { Kind = "pack", ItemId = "special.t4.pack", GoldPrice = 3200, Sold = true }) == "Sold out"
                && CollectionPrototype.BalloonTagText(new BalloonOffer { Kind = "box", BoxTier = 1, GoldPrice = 20, Sold = true }) == "Sold out", "BalloonTagText: <name> and <n> gold, or Sold out");

            Func<string, float> constant = n => Convert.ToSingle(typeof(CollectionPrototype).GetField(n, M14.AnyMember).GetValue(null));
            float barGap = constant("BarLineGap"), square = constant("HudSquare"), margin = constant("HudMargin"), gap = constant("HudGap"), maxSide = CollectionPrototype.BalloonOfferMaxSide;
            var prefab = Resources.Load<GameObject>("Settlement/Balloon/BalloonInterior");
            var report = new List<string>();
            foreach (var (screen, dpi) in new[] { (new Vector2(1116, 2480), 401f), (new Vector2(720, 1680), 320f) })
            {
                float scale = Mathf.Sqrt(screen.x / 720f * screen.y / 1280f), dp = dpi / 160f;
                Rect safe = Rect.MinMaxRect(16f * dp, 24f * dp, screen.x - 16f * dp, screen.y - 48f * dp);
                float sw = safe.width / scale, sh = safe.height / scale, barLine = barGap, bagTop = 0.035f;
                Func<Rect, Rect> px = r => new Rect(safe.x + r.x * scale, safe.y + r.y * scale, r.width * scale, r.height * scale);
                Func<Rect, Rect> band = r => Rect.MinMaxRect(safe.x + r.xMin * safe.width, safe.y + r.yMin * safe.height, safe.x + r.xMax * safe.width, safe.y + r.yMax * safe.height);
                float closest23 = float.MaxValue, closestFace = float.MaxValue; string worst = "";
                for (int tier = 1; tier <= 4; tier++)
                    for (int part = 0; part < 4; part++)
                    {
                        var holder = new GameObject("balloon layout").transform; holder.position = MovementPrototype.InteriorWorldPosition;
                        var cam = new GameObject("balloon layout camera").AddComponent<Camera>();
                        try
                        {
                            Transform room = UnityEngine.Object.Instantiate(prefab, holder, false).transform;
                            cam.fieldOfView = StreetCamera.FieldOfView; cam.aspect = screen.x / screen.y;
                            Pose pose = CollectionPrototype.ShopCounterPose(cam, room); cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                            string g = SpecialGear.Id(tier, part), p = SpecialGear.Id(tier, 4);
                            var offers = new[] { new BalloonOffer { Kind = "garment", ItemId = g, GoldPrice = SpecialGear.GoldPrice(g) }, new BalloonOffer { Kind = "box", BoxTier = tier, GoldPrice = BalloonShop.BoxPrice(tier) }, new BalloonOffer { Kind = "pack", ItemId = p, GoldPrice = SpecialGear.GoldPrice(p) } };
                            string at = " " + g + " at " + screen.x + "x" + screen.y;
                            var seen = new List<(string what, Rect rect)> { ("feedback line", band(CollectionPrototype.FeedbackBand)), ("RESET SAVE", band(CollectionPrototype.ResetSaveBand)) };
                            var tops = new Vector2[3]; var chars = new int[3]; var modelRects = new Rect[3]; var meshRects = new Rect[3];
                            for (int i = 0; i < 3; i++)
                            {
                                Transform anchor = room.Find("OfferAnchor" + (i + 1));
                                Vector3 v = cam.WorldToViewportPoint(anchor.position);
                                Check(v.z > 0f && v.x > 0.05f && v.x < 0.95f && v.y > 0.12f && v.y < 0.48f, "BalloonView anchors in the band OfferAnchor" + (i + 1) + at + ": " + v.ToString("0.000"));
                                GameObject model = CollectionPrototype.BalloonOfferModel(anchor, offers[i], (tier - 1) * 6 + 1);
                                Check(model != null, "BalloonView offer model " + i + at);
                                Bounds mb = RendererBox(model.transform);
                                Check(Mathf.Max(mb.size.x, mb.size.y, mb.size.z) <= maxSide + 0.002f && Mathf.Abs(mb.min.y - anchor.position.y) < 0.002f && Mathf.Abs(mb.center.x - anchor.position.x) < 0.002f,
                                    "BalloonView fit rule (largest side <= " + maxSide + " m, standing centred on the anchor) " + i + at + ": " + mb.size.ToString("0.000"));
                                mb.Encapsulate(anchor.position); modelRects[i] = ScreenBox(cam, mb, screen);
                                meshRects[i] = M14.MeshScreenRect(cam, model.transform, screen);
                                seen.Add(("offer " + (i + 1), meshRects[i]));
                                tops[i] = Vector2.Scale(cam.WorldToViewportPoint(new Vector3(anchor.position.x, mb.max.y, anchor.position.z)), screen);
                                chars[i] = CollectionPrototype.LongestLine(CollectionPrototype.BalloonTagText(offers[i]));
                            }
                            Rect face = ScreenBox(cam, RendererBox(room.GetComponentsInChildren<Transform>().First(t => t.name == "BalloonMerchantHead")), screen);
                            seen.Add(("merchant face", face));
                            var tags = new Rect[3]; Rect area = M14.TagArea(screen, out float h, out float tagGap);
                            CollectionPrototype.ShopTagLayout(tops, chars, area, h, tagGap, tags, modelRects);
                            for (int i = 0; i < 3; i++)
                            {
                                seen.Add(("price tag " + (i + 1), tags[i]));
                                Check(tags[i].width > 0f && tags[i].xMin >= area.xMin - 0.01f && tags[i].xMax <= area.xMax + 0.01f && tags[i].yMin >= area.yMin && tags[i].yMax <= area.yMax + 0.01f, "BalloonView tag inside the area " + i + at);
                                Check(tags[i].yMin >= tops[i].y, "BalloonView tag above its offer " + i + at);
                            }
                            seen.Add(("bag opening", band(Rect.MinMaxRect(0f, 0f, 1f, bagTop))));
                            seen.Add(("Return square", px(new Rect(margin, barLine * sh, square, square))));
                            seen.Add(("mission tracker", px(new Rect(margin - 0.05f * square, barLine * sh + square + gap - 0.06f * square, 1.1f * square, 1.16f * square))));
                            seen.Add(("cash label", px(CollectionPrototype.WalletCashRect(sw, sh, barLine)))); seen.Add(("gold label", px(CollectionPrototype.WalletGoldRect(sw, sh))));
                            // Unlike the shop counters, the balloon's offers must not overlap one another either (anchors 2 and 3 sit 0.75 m apart).
                            var clashes = new List<string>();
                            for (int i = 0; i < seen.Count; i++)
                                for (int j = i + 1; j < seen.Count; j++)
                                    if (seen[i].rect.Overlaps(seen[j].rect)) clashes.Add(seen[i].what + " " + seen[i].rect.ToString("0") + " x " + seen[j].what + " " + seen[j].rect.ToString("0"));
                            Check(clashes.Count == 0, "BalloonLayout nothing visible overlaps" + at + (clashes.Count > 0 ? ": " + string.Join("; ", clashes) : ""));
                            float gap23 = Mathf.Max(meshRects[2].xMin - meshRects[1].xMax, meshRects[1].xMin - meshRects[2].xMax);   // the camera faces -Z, so anchor 3 shows left of anchor 2
                            if (gap23 < closest23) { closest23 = gap23; worst = g; }
                            foreach (Rect r in meshRects.Concat(tags)) closestFace = Mathf.Min(closestFace, Mathf.Max(r.xMin - face.xMax, face.xMin - r.xMax, r.yMin - face.yMax, face.yMin - r.yMax));
                            if (tier == 1 && part == 0)
                                report.Add(screen.x + "x" + screen.y + " (t1 hat, box, pack): offers " + string.Join(" ", meshRects.Select(r => r.ToString("0"))) + "; tags " + string.Join(" ", tags.Select(r => r.ToString("0"))) + "; face " + face.ToString("0"));
                        }
                        finally { UnityEngine.Object.DestroyImmediate(cam.gameObject); UnityEngine.Object.DestroyImmediate(holder.gameObject); }
                    }
                report.Add(screen.x + "x" + screen.y + ": smallest offer 2-3 gap " + closest23.ToString("0") + " px (" + worst + "), smallest clearance to the face " + closestFace.ToString("0") + " px");
            }
            BalloonLayoutReport = string.Join("\n", report);

            // Ruling 8: no storefront lamps; at most two warm, shadowless, short-range point lights at the paper lanterns, under the holder.
            Check(!System.IO.File.ReadAllText(BalloonSource).Contains("CounterLamps.Hang"), "BalloonLights: the balloon never hangs the storefront lamps");
            var lit = new GameObject("balloon light check").transform; lit.position = MovementPrototype.InteriorWorldPosition;
            try
            {
                UnityEngine.Object.Instantiate(prefab, lit, false);
                CollectionPrototype.HangLanternLights(lit);
                Light[] lights = lit.GetComponentsInChildren<Light>(true);
                Check(lights.Length >= 1 && lights.Length <= 2, "BalloonLights: 1-2 lights, found " + lights.Length);
                Vector3[] lanterns = { new Vector3(-1.6f, 2.72f, 0.4f), new Vector3(1.6f, 2.72f, 0.4f) };   // festoon lanterns (generate_balloon_interior.lanterns, x symmetric)
                foreach (Light l in lights)
                {
                    Color c = l.color; Vector3 local = lit.InverseTransformPoint(l.transform.position);
                    Check(l.type == LightType.Point && l.shadows == LightShadows.None && l.range > 0f && l.range <= CounterLamps.Range && l.intensity <= CounterLamps.Intensity && c.r >= c.g && c.g >= c.b && c.r > c.b,
                        "BalloonLights: warm shadowless point light within the counter-lamp budget (" + l.type + ", " + l.shadows + ", range " + l.range + ", intensity " + l.intensity + ")");
                    Check(l.transform.parent == lit && lanterns.Any(a => (a - local).magnitude < 0.01f), "BalloonLights: parented to the holder at a paper lantern, at " + local.ToString("0.00"));
                }
                Check(!lit.GetComponentsInChildren<Transform>(true).Any(t => t.name == CounterLamps.LampName), "BalloonLights: no counter lamp in the interior");
            }
            finally { UnityEngine.Object.DestroyImmediate(lit.gameObject); }
        }

        private const string BalloonSource = "Assets/_Game/Scripts/Unity/CollectionPrototype.Balloon.cs";

        // The interior opens only seated at a host camp (BalloonHosts.IsHost(travel.CurrentCampId)); the exterior balloon takes the tap.
        private static void BalloonEntryOnlyAtHostSeated()
        {
            Check(CollectionPrototype.BalloonEntryAllowed(true, Host1, false) && !CollectionPrototype.BalloonEntryAllowed(true, Host1, true) && !CollectionPrototype.BalloonEntryAllowed(false, Host1, false)
                && !CollectionPrototype.BalloonEntryAllowed(true, "camp.01.04", false) && !CollectionPrototype.BalloonEntryAllowed(true, null, false), "BalloonEntryOnlyAtHostSeated: the rule");
            foreach (var (camp, phase, want) in new[] { (Host1, "Seated", true), (Host1, "Walking", false), (Host1, "Approaching", false), (Host1, "Settlement", false), ("camp.01.04", "Seated", false) })
            {
                var root = new GameObject("balloon entry check", typeof(RectTransform));
                try
                {
                    CollectionPrototype proto = BalloonHarness(root, camp, out _);
                    M14.SetPhase(proto, phase);
                    M14.Call(proto, "OpenBalloon");
                    bool open = Alive(Get<Transform>(proto, "balloonHolder"));
                    Check(open == want, "BalloonEntryOnlyAtHostSeated: " + camp + " " + phase + " opened=" + open);
                    if (open) M14.Call(proto, "TearDownBalloon");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            TravelCatalog catalog = TravelCatalog.Prototype();
            var path = new SampledTrailPath(MovementPrototype.DefaultKnots());
            var parent = new GameObject("balloon exterior check").transform;
            try
            {
                CampAnchor host = TravelScenery.BuildCamp(parent, catalog.Camp(Host1), path, 0, null, null, null), other = TravelScenery.BuildCamp(parent, catalog.Camp("camp.01.04"), path, 0, null, null, null);
                Check(host.Balloon != null && host.Balloon.name == "Balloon merchant" && host.Balloon.GetComponent<BoxCollider>() != null && host.Balloon.GetComponent<BoxCollider>().isTrigger && other.Balloon == null,
                    "BalloonEntryOnlyAtHostSeated: the host camp's balloon is the renamed 'Balloon merchant' with a tap trigger; none elsewhere");
                Check(!parent.GetComponentsInChildren<Transform>(true).Any(t => t.name.Contains("placement test")), "BalloonEntryOnlyAtHostSeated: no '(placement test)' name left");
            }
            finally { UnityEngine.Object.DestroyImmediate(parent.gameObject); }
            Check(System.IO.File.ReadAllText("Assets/_Game/Scripts/Unity/CollectionPrototype.cs").Contains("TryTapBalloon(position)"), "BalloonEntryOnlyAtHostSeated: a tap reaches the balloon");
        }

        // Every exit tears the view down and restores the camp: Return, ResetHomeView (travel and context changes call it), the phase leaving
        // Seated, a faint and another view opening. While open the chrome, camp debug buttons, orbit rig and speed line are off.
        private static void BalloonTeardownPaths()
        {
            var paths = new (string name, bool seated, Action<CollectionPrototype> leave)[]
            {
                ("Return", true, p => M14.Call(p, "CloseBalloon")),
                ("ResetHomeView (travel, context change)", true, p => M14.Call(p, "ResetHomeView")),
                ("phase left Seated", false, p => { M14.SetPhase(p, "Walking"); M14.Call(p, "UpdateBalloon"); }),
                ("faint", true, p => { var st = Get<StaminaState>(p, "stamina"); typeof(StaminaState).GetField("<Fainted>k__BackingField", M14.AnyMember).SetValue(st, true); M14.Call(p, "UpdateBalloon"); }),
                ("another view (map)", true, p => { Get<GameObject>(p, "mapPanel").SetActive(true); M14.Call(p, "UpdateBalloon"); }),
                ("another view (screen return)", true, p => { var other = new GameObject("other view"); other.transform.SetParent(p.transform, false); M14.Call(p, "ShowScreenReturn", other, (UnityEngine.Events.UnityAction)(() => { })); M14.Call(p, "UpdateBalloon"); }),
            };
            foreach (var (name, seated, leave) in paths)
            {
                var root = new GameObject("balloon teardown check", typeof(RectTransform));
                try
                {
                    CollectionPrototype proto = BalloonHarness(root, Host1, out Camera cam);
                    var rig = cam.GetComponent<TheTrailWalker.CameraControl.FollowCameraRig>();
                    var router = Get<TheTrailWalker.Input.GestureRouter>(proto, "router");
                    GameObject gold = Get<GameObject>(proto, "goldButton");
                    M14.Call(proto, "OpenBalloon");
                    Transform holder = Get<Transform>(proto, "balloonHolder");
                    Check(Alive(holder) && !ChromeShown(proto, false) && !gold.activeSelf && !rig.enabled && !router.SpeedAreaEnabled && cam.transform.position.y < -150f
                        && Get<RectTransform>(proto, "shopHud").gameObject.activeSelf && Get<BalloonState>(proto, "balloon").Offers != null,
                        "BalloonTeardownPaths: open (" + name + "): holder, chrome hidden, debug grants hidden, rig off, speed line off, camera at the counter, HUD up, session open");
                    leave(proto);
                    Check(!Alive(Get<Transform>(proto, "balloonHolder")) && !Alive(holder) && ChromeShown(proto, true) && gold.activeSelf == seated && rig.enabled && router.SpeedAreaEnabled
                        && !Get<RectTransform>(proto, "shopHud").gameObject.activeSelf && Get<BalloonState>(proto, "balloon").Offers == null,
                        "BalloonTeardownPaths: " + name + " closes the view and restores the chrome, rig, speed line and HUD, and ends the session");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            Check(System.IO.File.ReadAllText("Assets/_Game/Scripts/Unity/CollectionPrototype.Home.cs").Contains("TearDownBalloon();"), "BalloonTeardownPaths: ResetHomeView tears the balloon down");
        }

        // Final fix wave: LEARN T1 RECIPES skips the box pool; the paper-note timeout is the balloon's own; a pickup in front of the balloon
        // gets the tap; the removed copy stays removed.
        private static void FinalFixWave()
        {
            var root = new GameObject("final fix wave check", typeof(RectTransform));
            try
            {
                CollectionPrototype proto = BalloonHarness(root, Host1, out Camera cam);
                var known = new CraftingState(); Set(proto, "crafting", known);
                M14.Call(proto, "LearnAllRecipes");
                Check(!RecipeBook.All.Any(r => r.MysteryBoxEligible && known.Knows(r.Id)) && RecipeBook.All.Any(r => !r.Retired && !r.MysteryBoxEligible && known.Knows(r.Id)),
                    "FinalFixWave: LEARN T1 RECIPES teaches no mystery-box recipe (and still teaches the rest)");

                // M1: a camp note outside the balloon waits for a tap; inside the balloon it times out, and Return hides it.
                var note = new GameObject("note"); note.transform.SetParent(root.transform, false);
                Set(proto, "settlementNote", note); Set(proto, "settlementNoteHideAt", -100f);
                M14.Call(proto, "UpdateBalloon");
                Check(note.activeSelf, "FinalFixWave: a camp note outside the balloon does not time out");
                M14.Call(proto, "OpenBalloon");
                note.SetActive(true); Set(proto, "settlementNoteHideAt", -100f);
                M14.Call(proto, "UpdateBalloon");
                Check(!note.activeSelf, "FinalFixWave: inside the balloon the note times out");
                note.SetActive(true); Set(proto, "settlementNoteHideAt", 1e9f);
                M14.Call(proto, "CloseBalloon");
                Check(!note.activeSelf, "FinalFixWave: Return hides a counter note");

                // M2 and copy 5: the balloon takes the tap only when no pickup is nearer along the ray; unseated it is silent and falls through.
                var balloon = new GameObject("tap balloon"); balloon.transform.SetParent(root.transform, false); balloon.transform.localPosition = new Vector3(0, 0, 10);
                var trigger = balloon.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.size = Vector3.one * 4f;
                Get<TravelSceneInfo>(proto, "scene").StartCamp = new CampAnchor { Balloon = balloon };
                var pickup = new GameObject("tap pickup"); pickup.transform.SetParent(root.transform, false);
                var box = pickup.AddComponent<BoxCollider>();
                var bag = new SpatialInventory();
                ItemEntry entry = bag.AddWorld("tap-pickup", Items.Find("special.t1.hat"), 1);
                Type bundleType = typeof(CollectionPrototype).GetNestedType("Bundle", M14.AnyMember);
                object bundle = Activator.CreateInstance(bundleType, true);
                bundleType.GetField("Item").SetValue(bundle, entry); bundleType.GetField("Collider").SetValue(bundle, box);
                var bundles = Get<System.Collections.IList>(proto, "bundles");
                Func<float, bool> tap = z =>
                {
                    cam.transform.SetPositionAndRotation(root.transform.position, root.transform.rotation); pickup.transform.localPosition = new Vector3(0, 0, z); Physics.SyncTransforms();   // the counter view moved the camera
                    Vector2 at = cam.WorldToScreenPoint(new Vector3(0, 0, 5));
                    return (bool)M14.Call(proto, "TryTapBalloon", at) ;
                };
                Func<bool> opened = () => Alive(Get<Transform>(proto, "balloonHolder"));
                Check(tap(500f) && opened(), "FinalFixWave: the balloon alone opens");
                M14.Call(proto, "TearDownBalloon");
                bundles.Add(bundle);
                Check(!tap(5f) && !opened(), "FinalFixWave: a pickup in front of the balloon gets the tap");
                Check(tap(15f) && opened(), "FinalFixWave: a pickup behind the balloon does not");
                M14.Call(proto, "TearDownBalloon");
                M14.SetPhase(proto, "Walking");
                string before = Get<UnityEngine.UI.Text>(proto, "feedback").text;
                Check(!tap(500f) && Get<UnityEngine.UI.Text>(proto, "feedback").text == before, "FinalFixWave: unseated, the balloon tap is silent and returns false");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            string src = System.IO.File.ReadAllText(BalloonSource);
            Check(!src.Contains("never wears out") && !src.Contains("Sit at the camp to visit") && !src.Contains("Six rewards") && !src.Contains("rolled and kept"), "FinalFixWave: the removed balloon copy is absent");
        }

        private static string NoteText(CollectionPrototype p) => (Get<UnityEngine.UI.Text>(p, "settlementNoteText") ?? Get<UnityEngine.UI.Text>(p, "feedback")).text;   // no settlement HUD: ShowSettlementNote says it

        // Decision 5 and 1: the sheet lists the six entries, claims what fits in order (a recipe learns at once), leaves the rest waiting
        // with the note, and the MENU's Boxes entry shows the waiting count until everything is claimed.
        private static void BoxSheetClaims()
        {
            var c = KnowIds(AllMakers()); BalloonState s = null; BoxManifest box = null;
            for (int i = 0; i < 400; i++) { s = new BalloonState(); box = BuyBox(s, Ctx(c, 40000 + i)); if (box.Entries.Any(e => e.Kind == "recipe")) break; }
            Check(box.Entries.Any(e => e.Kind == "recipe"), "BoxSheetClaims: setup rolled a recipe");
            var root = new GameObject("box sheet check", typeof(RectTransform));
            try
            {
                CollectionPrototype proto = BalloonHarness(root, Host1, out _);
                Set(proto, "balloon", s); Set(proto, "crafting", c); Set(proto, "ledger", FullBag());
                M14.Call(proto, "OpenBoxSheet", box.BoxId);
                var page = Get<RectTransform>(proto, "shopSheetPage");
                var rows = page.Cast<Transform>().Where(t => t.name.StartsWith("Reward row", StringComparison.Ordinal)).ToList();
                Check(Get<GameObject>(proto, "shopSheet").activeSelf && rows.Count == 6, "BoxSheetClaims: the sheet lists the six entries (" + rows.Count + " rows)");
                BoxEntry recipe = box.Entries.First(e => e.Kind == "recipe");
                Check(recipe.Claimed && c.Knows(recipe.RecipeId) && box.Entries.Where(e => e.Kind == "item").All(e => !e.Claimed), "BoxSheetClaims: a full bag learns the recipe and leaves every physical reward waiting");
                string note = NoteText(proto);
                Check(note.Contains("Recipe learned: " + RecipeBook.Find(recipe.RecipeId).Name) && note.Contains("Reward waiting in the box: make room in the bag"), "BoxSheetClaims: notes '" + note + "'");
                Check(rows.Count(r => r.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t => t.text == "Waiting")) == box.Entries.Count(e => !e.Claimed), "BoxSheetClaims: waiting rows say so");
                int waiting = box.Entries.Count(e => !e.Claimed);
                M14.Call(proto, "SetMenu", true);
                var boxes = Get<RectTransform>(proto, "boxesMenuButton");
                Check(CollectionPrototype.WaitingRewards(s) == waiting && boxes.gameObject.activeSelf && boxes.Find("Count").GetComponent<UnityEngine.UI.Text>().text == waiting.ToString(), "BoxSheetClaims: MENU Boxes shows " + waiting);
                M14.Call(proto, "CloseShopSheet");
                Set(proto, "ledger", BigBag());
                M14.Call(proto, "OpenWaitingBox");
                rows = page.Cast<Transform>().Where(t => t.name.StartsWith("Reward row", StringComparison.Ordinal)).ToList();
                Check(box.Entries.All(e => e.Claimed) && rows.Count == 6 && NoteText(proto).Contains("All rewards claimed") && Get<SpatialInventory>(proto, "ledger").Entries.Count >= box.Entries.Count(e => e.Kind == "item"),
                    "BoxSheetClaims: with room, the MENU entry claims the rest in order: " + NoteText(proto));
                M14.Call(proto, "SetMenu", true);
                Check(CollectionPrototype.WaitingRewards(s) == 0 && !boxes.gameObject.activeSelf, "BoxSheetClaims: MENU Boxes hides when nothing waits");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        // A purchase records the gold (TradeRecord.gold, cash 0) with the debit and the item or manifest, and the PAY tap saves exactly once,
        // on the line right after the purchase (carried requirement 1). The bakery honours box reservations (carried requirement 2).
        private static void GearSheetBuys()
        {
            var c = KnowIds(AllMakers()); var s = new BalloonState(); BalloonShop.Open(s, Host1, R1To6, 1, new System.Random(9));
            var log = new List<TradeRecord>(); var bag = new SpatialInventory(); var w = new Wallet(0, 100000); var when = new DateTime(2026, 11, 21, 9, 0, 0, DateTimeKind.Utc);
            string garment = s.Offers[0].ItemId; long price = s.Offers[0].GoldPrice;
            Check(CollectionPrototype.BalloonBuyAndRecord(s, 0, s.Offers[0].Revision, w, bag, Ctx(c, 3), NewIds("g"), log, when, out BoxManifest none, out string why) && none == null && log.Count == 1
                && log[0].gold == -price && log[0].cash == 0 && log[0].itemId == garment && log[0].quantity == 1 && log[0].quality == 5 && log[0].totalMinor == price && log[0].result == CollectionPrototype.BalloonBoughtResult
                && log[0].settlementId == "balloon" && log[0].campId == Host1 && log[0].utc == when.ToString("O", System.Globalization.CultureInfo.InvariantCulture)
                && w.Gold == 100000 - price && bag.Entries.Count(e => e.Definition.Id == garment) == 1, "GearSheetBuys: gear bought and recorded: " + why);
            Check(CollectionPrototype.BalloonBuyAndRecord(s, 1, s.Offers[1].Revision, w, bag, Ctx(c, 4), NewIds("b"), log, when, out BoxManifest box, out why) && box != null && s.Boxes.Contains(box)
                && log.Count == 2 && log[1].gold == -20 && log[1].itemId == box.BoxId && log[1].quality == 0 && w.Gold == 100000 - price - 20, "GearSheetBuys: box bought and recorded: " + why);
            Check(!CollectionPrototype.BalloonBuyAndRecord(s, 0, s.Offers[0].Revision, w, bag, Ctx(c, 5), NewIds("x"), log, when, out _, out why) && why == "Sold out" && log.Count == 2, "GearSheetBuys: a refusal records nothing: " + why);
            var back = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(new SessionSnapshot { warehouseLog = log }));
            Check(back.warehouseLog.Count == 2 && back.warehouseLog[0].gold == -price && back.warehouseLog[1].gold == -20, "GearSheetBuys: the records survive a save round trip");

            // Call sites: the domain purchases run only inside BalloonBuyAndRecord, and BalloonPayTapped saves once, on the next line.
            string all = string.Concat(System.IO.Directory.GetFiles("Assets/_Game/Scripts/Unity", "*.cs", System.IO.SearchOption.AllDirectories).Select(System.IO.File.ReadAllText));
            string src = System.IO.File.ReadAllText(BalloonSource);
            Func<string, string, int> count = (text, what) => { int n = 0, at = 0; while ((at = text.IndexOf(what, at, StringComparison.Ordinal)) >= 0) { n++; at += what.Length; } return n; };
            Func<string, string> body = name => { int at = src.IndexOf(name + "(", StringComparison.Ordinal); int open = src.IndexOf('{', at), depth = 0, i = open; do { if (src[i] == '{') depth++; else if (src[i] == '}') depth--; i++; } while (depth > 0); return src.Substring(open, i - open); };
            string buy = body("public static bool BalloonBuyAndRecord"), pay = body("private void BalloonPayTapped");
            Check(count(all, "BalloonShop.TryBuyGear(") == 1 && count(all, "BalloonShop.TryBuyBox(") == 1 && buy.Contains("BalloonShop.TryBuyGear(") && buy.Contains("BalloonShop.TryBuyBox(") && !buy.Contains("SaveNow"),
                "GearSheetBuys: TryBuyGear/TryBuyBox are called only inside BalloonBuyAndRecord, which never saves");
            string[] lines = pay.Split('\n'); int call = Array.FindIndex(lines, l => l.Contains("BalloonBuyAndRecord("));
            Check(count(pay, "SaveNow()") == 1 && call >= 0 && call + 1 < lines.Length && lines[call + 1].Contains("SaveNow();") && count(pay, "BalloonBuyAndRecord(") == 1,
                "GearSheetBuys: BalloonPayTapped saves exactly once, on the line right after the purchase");

            string shops = System.IO.File.ReadAllText("Assets/_Game/Scripts/Unity/CollectionPrototype.Shops.cs");
            Check(shops.Contains("ShopService.Offers(shops, shopId, ShopRegions(), crafting, MysteryBoxes.Reserved(balloon))") && shops.Contains("ShopService.TryBuyRecipe(s, slot, revision, wallet, crafting, regions, out reason, boxReserved)")
                && count(shops, "MysteryBoxes.Reserved(balloon)") >= 2, "GearSheetBuys: the bakery's offers and recipe purchase pass MysteryBoxes.Reserved(balloon)");
        }
    }
}
