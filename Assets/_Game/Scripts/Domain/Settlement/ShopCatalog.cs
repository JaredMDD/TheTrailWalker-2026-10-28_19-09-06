using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;

namespace TheTrailWalker.Domain.Settlement
{
    public enum ShopSlotRole { Any, RawFibre, Processed, ProcessedFine, Fish, Exclusive }

    // One good a shop can stock. Region 0 means tier-gated (a shop exclusive); otherwise it unlocks when that region is discovered.
    public sealed class ShopEntry
    {
        public string ShopId, ItemId;
        public int Tier, Region;
        public ShopSlotRole Role;
        public long? CashOverride;
    }

    // Shop pools (shop listings spec 5.1-5.5). Price per unit is 4 x the level-one Q5 value unless the entry authors its own.
    public static class ShopCatalog
    {
        public const string Farm = "shop.farm", Mine = "shop.mine", Tailor = "shop.tailor", Ranch = "shop.ranch", Fishery = "shop.fishery";
        private static readonly List<ShopEntry> all = Build();
        public static IReadOnlyList<ShopEntry> All => all;

        public static IEnumerable<ShopEntry> Eligible(string shopId, ICollection<int> discoveredRegions, int reachedTier) =>
            all.Where(e => e.ShopId == shopId && (e.Region == 0 ? e.Tier <= reachedTier : discoveredRegions.Contains(e.Region)));

        public static long UnitPriceMinor(ShopEntry e) => e.CashOverride.HasValue ? Money.Minor(e.CashOverride.Value) : 4 * ItemPrices.Profile(Crafting.Items.Find(e.ItemId), 5).UnitValue;

        private static List<ShopEntry> Build()
        {
            var list = new List<ShopEntry>();
            void Add(string shop, string id, int tier, int region, ShopSlotRole role = ShopSlotRole.Any, long? cash = null) =>
                list.Add(new ShopEntry { ShopId = shop, ItemId = id, Tier = tier, Region = region, Role = role, CashOverride = cash });
            void Farm_(int tier, params (string id, int region)[] rows) { foreach (var r in rows) Add(Farm, r.id, tier, r.region); }
            void Mine_(int tier, params (string id, int region)[] rows) { foreach (var r in rows) Add(Mine, r.id, tier, r.region); }

            Farm_(1, ("food.apples", 1), ("ingredient.rock_salt", 3), ("food.strawberries", 4), ("ingredient.mushrooms", 4), ("ingredient.tea_herbs", 0), ("ingredient.oats", 5), ("food.berries", 5), ("ingredient.wheat", 6));
            Farm_(2, ("food.blackberries", 7), ("ingredient.carrots", 8), ("ingredient.barley", 9), ("food.blueberries", 9), ("ingredient.flax_seeds", 10), ("ingredient.tomatoes", 11),
                ("ingredient.rice", 11), ("ingredient.honey", 11), ("ingredient.lavender", 0));
            Farm_(3, ("ingredient.corn", 13), ("ingredient.beans", 14), ("ingredient.chickpeas", 15), ("ingredient.soybeans", 16), ("ingredient.sugar_cane", 17), ("ingredient.sunflower_seeds", 17),
                ("ingredient.pumpkins", 15), ("ingredient.green_chili", 17), ("ingredient.wild_mint", 18));
            Farm_(4, ("ingredient.potatoes", 19), ("ingredient.seaweed", 23), ("food.coconuts", 24), ("ingredient.cacao_beans", 20), ("ingredient.coffee_beans", 21), ("ingredient.vanilla_pods", 22));

            Mine_(1, ("material.clay", 3), ("material.pebbles", 5), ("material.sand", 6), ("material.flint", 2), ("material.limestone", 4));
            Mine_(2, ("material.sandstone", 10), ("material.yellow_ochre", 12), ("material.copper_ore", 8), ("material.quartz", 9), ("material.tin_ore", 12));
            Mine_(3, ("material.granite", 15), ("material.slate", 16), ("material.iron_ore", 14), ("material.silver_ore", 18), ("material.turquoise", 16));
            Mine_(4, ("material.marble", 20), ("material.basalt", 21), ("material.obsidian", 22), ("material.gold_ore", 20), ("material.lapis_lazuli", 23),
                ("material.emerald", 21), ("material.ruby", 22), ("material.sapphire", 23), ("material.diamond", 24));

            const ShopSlotRole Raw = ShopSlotRole.RawFibre, Proc = ShopSlotRole.Processed, Fine = ShopSlotRole.ProcessedFine;
            foreach (var (id, tier, region) in new[] { ("material.hemp", 1, 2), ("material.nettle_fiber", 1, 3), ("material.reeds", 1, 6), ("material.flax_fiber", 2, 8), ("material.cattail_fiber", 2, 12),
                ("material.cotton_fiber", 3, 14), ("material.agave_fiber", 3, 18), ("material.bamboo_fiber", 4, 24) }) Add(Tailor, id, tier, region, Raw);
            // Tier-one textiles price from their recipes; later ones have no recipe yet, so they carry the spec's 8 x input value.
            Add(Tailor, "item.hemp_cord", 1, 2, Proc); Add(Tailor, "item.hemp_thread", 1, 2, Proc); Add(Tailor, "item.hemp_cloth", 1, 2, Proc);
            Add(Tailor, "item.nettle_thread", 1, 3, Proc); Add(Tailor, "item.nettle_cloth", 1, 3, Proc); Add(Tailor, "item.fine_hemp_cloth", 1, 2, Fine);
            Add(Tailor, "item.flax_cord", 2, 8, Proc, 36); Add(Tailor, "item.flax_stuffing", 2, 8, Proc, 36); Add(Tailor, "item.flax_cloth", 2, 8, Proc, 48);
            Add(Tailor, "item.cattail_cord", 2, 12, Proc, 72); Add(Tailor, "item.fine_flax_cloth", 2, 8, Fine, 152);
            Add(Tailor, "item.cotton_cord", 3, 14, Proc, 72); Add(Tailor, "item.cotton_thread", 3, 14, Proc, 48); Add(Tailor, "item.cotton_stuffing", 3, 14, Proc, 72);
            Add(Tailor, "item.cotton_cloth", 3, 14, Proc, 96); Add(Tailor, "item.agave_cord", 3, 18, Proc, 144);
            // Assumed recipe 3 Cotton cloth + 1 Beeswax (2 Honey -> 1): (3 x 12 + 8) x 8. The spec leaves the price open.
            Add(Tailor, "item.fine_cotton_cloth", 3, 14, Fine, 352);
            Add(Tailor, "item.bamboo_cloth", 4, 24, Proc, 160);

            Add(Ranch, "ingredient.egg", 1, 0); Add(Ranch, "food.milk.cow", 1, 0); Add(Ranch, "food.milk.goat", 1, 0); Add(Ranch, "animal.chicken.meat", 1, 0, ShopSlotRole.Any, 50);
            Add(Ranch, "animal.duck.meat", 2, 0, ShopSlotRole.Any, 100); Add(Ranch, "animal.lamb.meat", 3, 0, ShopSlotRole.Any, 180); Add(Ranch, "animal.beef.meat", 4, 0, ShopSlotRole.Any, 300);

            // Fish unlock in the first region whose fish pair holds them (HarvestModel region pairs).
            foreach (var (id, tier, region) in new[] { ("fish.rainbow_trout", 1, 1), ("fish.bluegill", 1, 1), ("fish.largemouth_bass", 1, 2), ("fish.yellow_perch", 1, 2),
                ("fish.common_carp", 2, 7), ("fish.channel_catfish", 2, 7), ("fish.northern_pike", 2, 8), ("fish.walleye", 2, 8),
                ("fish.salmon", 3, 13), ("fish.brown_trout", 3, 13), ("fish.sturgeon", 3, 14), ("fish.lake_trout", 3, 14),
                ("fish.longnose_gar", 4, 19), ("fish.red_drum", 4, 19), ("fish.red_snapper", 4, 20), ("fish.tuna", 4, 22) }) Add(Fishery, id, tier, region, ShopSlotRole.Fish);
            Add(Fishery, "ingredient.shrimp", 1, 0, ShopSlotRole.Exclusive); Add(Fishery, "fishery.crab", 2, 0, ShopSlotRole.Exclusive);
            Add(Fishery, "fishery.squid", 3, 0, ShopSlotRole.Exclusive); Add(Fishery, "fishery.lobster", 4, 0, ShopSlotRole.Exclusive);
            return list;
        }
    }
}
