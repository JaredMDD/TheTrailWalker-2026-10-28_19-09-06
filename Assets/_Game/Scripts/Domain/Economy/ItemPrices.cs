using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Economy
{
    // Price category order (section 12.2.1): Misc > Backpack > Clothing > Tool > Advanced material > Food > Other > Basic.
    public enum PriceCategory { Basic, OtherMaterial, AdvancedMaterial, Food, Tool, Clothing, Backpack, Misc }

    // Price references and intrinsic item values (Milestone 9; section 12.2). All values are minor units (1/100 cash).
    // Authored test data: final economic balance is a later phase. Levelled crafts (food, tools, clothing, backpacks, misc)
    // snapshot their value at craft time (CraftStats.Compute); everything else is a pure function of item and quality.
    public static class ItemPrices
    {
        // Gathered and source-only goods, whole cash per Q1 unit. Section 12.2.1 anchors: the simplest tier-one material is
        // one cash, pine log three (3 branches), pine plank nine (3 logs, derived). Raw foods, animal and fish goods sit in
        // the "other material/ingredient" band below its 20-cash tier-one reference.
        private static readonly Dictionary<string, long> Gathered = new Dictionary<string, long> {
            { "material.pine_branch", 1 }, { "material.stone", 1 }, { "material.hemp", 1 }, { "material.nettle_fiber", 1 }, { "material.pebbles", 1 },
            { "material.pine_cone", 1 }, { "material.reeds", 1 }, { "material.sand", 1 }, { "material.clay", 1 }, { "material.flint", 2 }, { "material.limestone", 2 },
            { "material.pine_log", 3 },
            { "food.apples", 2 }, { "food.berries", 2 }, { "food.strawberries", 2 }, { "ingredient.oats", 2 }, { "ingredient.wheat", 2 },
            { "ingredient.mushrooms", 3 }, { "ingredient.rock_salt", 3 }, { "ingredient.tea_herbs", 3 }, { "ingredient.pine_nuts", 3 },
            { "ingredient.birch_sap", 6 }, { "ingredient.mesquite_pods", 12 }, { "ingredient.acorns", 20 },
            { "animal.rabbit.pelt", 8 }, { "animal.rabbit.meat", 6 }, { "material.bone", 4 }, { "ingredient.animal_fat", 5 },
            { "fish.rainbow_trout", 10 }, { "fish.largemouth_bass", 12 }, { "fish.bluegill", 8 }, { "fish.yellow_perch", 10 },
            { "food.milk.cow", 6 }, { "food.milk.goat", 8 }, { "ingredient.egg", 4 }, { "ingredient.shrimp", 12 },
            // Shop pool goods (shop listings spec 5.1-5.5); the shop price is 4 x the Q5 value, so 8 x these bases.
            { "food.blackberries", 5 }, { "ingredient.carrots", 5 }, { "ingredient.barley", 5 }, { "food.blueberries", 5 }, { "ingredient.flax_seeds", 5 }, { "ingredient.tomatoes", 5 },
            { "ingredient.rice", 6 }, { "ingredient.honey", 8 }, { "ingredient.lavender", 6 },
            { "ingredient.corn", 10 }, { "ingredient.beans", 10 }, { "ingredient.chickpeas", 10 }, { "ingredient.soybeans", 10 }, { "ingredient.sugar_cane", 10 }, { "ingredient.sunflower_seeds", 10 },
            { "ingredient.pumpkins", 12 }, { "ingredient.green_chili", 12 }, { "ingredient.wild_mint", 12 }, { "ingredient.potatoes", 18 }, { "ingredient.seaweed", 18 },
            { "food.coconuts", 20 }, { "ingredient.cacao_beans", 22 }, { "ingredient.coffee_beans", 22 }, { "ingredient.vanilla_pods", 25 },
            { "material.sandstone", 3 }, { "material.yellow_ochre", 3 }, { "material.copper_ore", 4 }, { "material.quartz", 4 }, { "material.tin_ore", 4 },
            { "material.granite", 6 }, { "material.slate", 6 }, { "material.iron_ore", 7 }, { "material.silver_ore", 9 }, { "material.turquoise", 10 },
            { "material.marble", 10 }, { "material.basalt", 10 }, { "material.obsidian", 12 }, { "material.gold_ore", 14 }, { "material.lapis_lazuli", 15 },
            { "material.emerald", 20 }, { "material.ruby", 20 }, { "material.sapphire", 20 }, { "material.diamond", 25 },
            { "material.flax_fiber", 3 }, { "material.cattail_fiber", 3 }, { "material.cotton_fiber", 6 }, { "material.agave_fiber", 6 }, { "material.bamboo_fiber", 10 },
            { "fish.common_carp", 25 }, { "fish.channel_catfish", 28 }, { "fish.northern_pike", 30 }, { "fish.walleye", 30 },
            { "fish.brown_trout", 55 }, { "fish.lake_trout", 58 }, { "fish.salmon", 60 }, { "fish.sturgeon", 65 },
            { "fish.longnose_gar", 90 }, { "fish.red_drum", 95 }, { "fish.red_snapper", 100 }, { "fish.tuna", 110 },
            // Override-priced shop goods: floor(shop price / 8), so selling never beats the shop price.
            { "animal.chicken.meat", 6 }, { "animal.duck.meat", 12 }, { "animal.lamb.meat", 22 }, { "animal.beef.meat", 37 },
            { "item.flax_cord", 4 }, { "item.flax_stuffing", 4 }, { "item.flax_cloth", 6 }, { "item.cattail_cord", 9 }, { "item.fine_flax_cloth", 19 },
            { "item.cotton_cord", 9 }, { "item.cotton_thread", 6 }, { "item.cotton_stuffing", 9 }, { "item.cotton_cloth", 12 }, { "item.agave_cord", 18 },
            { "item.fine_cotton_cloth", 44 }, { "item.bamboo_cloth", 20 },
            { "fishery.crab", 36 }, { "fishery.squid", 70 }, { "fishery.lobster", 120 } };
        private static readonly long[] Basic = { 1, 3, 6, 10 }, Other = { 20, 60, 120, 200 };
        private static readonly long[] Misc = { 600, 1800, 3600, 6000 }, Backpack = { 350, 1050, 2100, 3500 }, Clothing = { 220, 660, 1320, 2200 },
            Tool = { 140, 420, 840, 1400 }, Food = { 60, 180, 360, 600 };
        public static readonly double[] QualityMultiplier = { 1.00, 1.15, 1.35, 1.60, 2.00 };

        public static RecipeDefinition Maker(string itemId)
        {
            foreach (RecipeDefinition recipe in RecipeBook.All) if (recipe.OutputId == itemId && !recipe.Retired) return recipe;
            foreach (RecipeDefinition recipe in RecipeBook.All) if (recipe.OutputId == itemId) return recipe;
            return null;
        }
        // True for outputs whose value depends on the crafter's recipe level (snapshotted at craft time).
        public static bool Levelled(ItemDefinition definition) { RecipeDefinition maker = definition == null ? null : Maker(definition.Id); return maker != null && maker.Levelled; }

        public static PriceCategory CategoryOf(ItemDefinition definition)
        {
            RecipeDefinition maker = Maker(definition.Id);
            if (definition.GearSlot.HasValue) return definition.GearSlot == Equipment.GearSlot.Backpack ? PriceCategory.Backpack : PriceCategory.Clothing;
            if (!definition.Stackable) return PriceCategory.Tool;
            if (maker != null && maker.Role == RecipeRole.Misc) return PriceCategory.Misc;
            if (definition.PreparedFood) return PriceCategory.Food;
            if (maker != null && maker.Role == RecipeRole.Material && maker.BaseSeconds > 0) return PriceCategory.AdvancedMaterial;
            return Gathered.TryGetValue(definition.Id, out long cash) && cash <= 3 ? PriceCategory.Basic : PriceCategory.OtherMaterial;
        }

        // Level-ten Q1-equivalent reference of a levelled category (section 12.2.1 table).
        public static long ReferenceAtLevel10(RecipeRole role, int tier, bool backpack)
        {
            int t = Math.Max(1, Math.Min(4, tier)) - 1;
            switch (role)
            {
                case RecipeRole.Misc: return Misc[t];
                case RecipeRole.Tool: return Tool[t];
                case RecipeRole.Food: return Food[t];
                case RecipeRole.Wearable: return backpack ? Backpack[t] : Clothing[t];
                default: return 0;
            }
        }

        // Fixed Q1 base of a non-levelled item: authored gathered price, otherwise its recipe inputs divided by the yield
        // (no value lost to crafting), otherwise the tier's basic or "other ingredient" reference.
        private static readonly Dictionary<string, long> memo = new Dictionary<string, long>();
        public static long MaterialBaseMinor(string itemId)
        {
            if (memo.TryGetValue(itemId, out long cached)) return cached;
            memo[itemId] = Money.Minor(1); // cycle guard
            long price;
            RecipeDefinition maker = Maker(itemId);
            ItemDefinition definition = Crafting.Items.Find(itemId);
            int tier = ItemTiers.Of(itemId);
            if (Gathered.TryGetValue(itemId, out long cash)) price = Money.Minor(cash);
            else if (maker != null && maker.Role == RecipeRole.Material)
            {
                long inputs = 0; foreach (RecipeInput input in maker.Inputs) inputs += MaterialBaseMinor(input.ItemId) * input.Quantity;
                price = Math.Max(Money.Minor(1), (inputs + maker.Yield - 1) / maker.Yield);
            }
            else if (definition != null && definition.PreparedFood) price = Money.Minor(Food[tier - 1] * 0.8); // level-one food reference
            else if (definition != null && definition.Category == ItemCategory.RefinedIngredient) price = Money.Minor(Other[tier - 1]);
            else price = Money.Minor(Basic[tier - 1]);
            return memo[itemId] = price;
        }

        // Intrinsic unit value of a non-levelled stackable at a quality (gathered drops, crafted materials, store goods).
        public static long IntrinsicMinor(ItemDefinition definition, int quality) =>
            (long)Math.Floor(MaterialBaseMinor(definition.Id) * QualityMultiplier[Clamp(quality) - 1] + 1e-9);

        // A source-generated profile (trail drops, harvest rewards, NPC goods): value plus, for gear and tools, a level-one
        // creation profile with the quality durability bonus and no furniture (section 12.1, NPC gear rule).
        public static ItemProfile Profile(ItemDefinition definition, int quality, int level = 1)
        {
            if (SpecialGear.Is(definition.Id)) return new ItemProfile(Money.Minor(SpecialGear.ResaleCash(definition.Id))); // fixed resale, no durability premium
            if (definition.Stackable && !Levelled(definition)) return new ItemProfile(IntrinsicMinor(definition, quality));
            RecipeDefinition maker = Maker(definition.Id);
            int q = Clamp(quality), tier = maker != null ? maker.Tier : Math.Max(1, definition.GearRank > 0 ? definition.GearRank : ItemTiers.Of(definition.Id));
            RecipeRole role = maker != null ? maker.Role : definition.GearSlot.HasValue ? RecipeRole.Wearable : !definition.Stackable ? RecipeRole.Tool : definition.PreparedFood ? RecipeRole.Food : RecipeRole.Misc;
            double price = ReferenceAtLevel10(role, tier, definition.GearSlot == Equipment.GearSlot.Backpack) * CraftStats.PriceLevelFactor(level) * QualityMultiplier[q - 1];
            int baseDurability = maker != null ? CraftStats.BaseDurability(maker)
                : definition.GearSlot.HasValue ? new[] { 1600, 2400, 3200, 4500 }[tier - 1] : HarvestTools.TryGet(definition, out ToolSpec spec) ? spec.BaseUses : 0;
            int durability = 0;
            if (baseDurability > 0 && !definition.Stackable)
            {
                double extra = CraftStats.QualityDurabilityBonus[q - 1];
                durability = (int)Math.Round(baseDurability * CraftStats.DurabilityLevelMultiplier(level) * (1 + extra));
                price *= 1 + extra;
            }
            return new ItemProfile(Math.Max(1, Money.Minor(price)), durability, durability);
        }

        // Economy 0 -> 1 save migration: levelled crafts snapshot whole cash (x100); the rest is re-derived.
        public static long MigrateLegacyValue(ItemDefinition definition, int quality, long legacy)
        {
            if (Levelled(definition)) return Math.Max(1, legacy) * Money.MinorPerCash;
            return definition.Stackable ? IntrinsicMinor(definition, quality) : Profile(definition, quality).UnitValue;
        }

        private static int Clamp(int quality) => Math.Max(1, Math.Min(5, quality));
    }
}
