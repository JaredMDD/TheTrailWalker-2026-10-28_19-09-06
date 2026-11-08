using System;
using System.Collections.Generic;

namespace TheTrailWalker.Domain.Travel
{
    // Data-only plan for all 24 regions (plan section 6.2.2; Milestone 8 item 2, November 2, 2026): name, tier, the three
    // featured loose item IDs and the rolled camp count. Only tier one is authored as runtime content; tiers two to four
    // stay as validated IDs until their milestones (no runtime loot, recipes or art is implied here).
    public sealed class RegionPlanEntry
    {
        public readonly int Number, Camps;
        public readonly string Name;
        public readonly IReadOnlyList<string> Featured;
        public int Tier => (Number - 1) / 6 + 1;
        public RegionPlanEntry(int number, string name, int camps, params string[] featured) { Number = number; Name = name; Camps = camps; Featured = featured; }
    }

    public static class RegionPlan
    {
        // Confirmed camp ranges per tier (October 31, 2026): random within the range, not incremental.
        public static (int min, int max) CampRange(int tier) => tier switch { 1 => (10, 14), 2 => (12, 16), 3 => (14, 18), 4 => (16, 20), _ => throw new ArgumentOutOfRangeException(nameof(tier)) };

        // Per tier, the branch and the pelt-family entry (tier three: turkey feathers, confirmed) are also interaction
        // rewards (tree / passive animal); those are the permitted overlaps between loose and interaction sources.
        public static readonly IReadOnlyDictionary<int, (string branch, string animal)> TierOverlaps = new Dictionary<int, (string, string)> {
            { 1, ("material.pine_branch", "animal.rabbit.pelt") }, { 2, ("material.birch_branch", "animal.beaver.pelt") },
            { 3, ("material.mesquite_branch", "animal.turkey.feathers") }, { 4, ("material.oak_branch", "animal.boar.hide") } };

        public static readonly IReadOnlyList<RegionPlanEntry> All = new[] {
            new RegionPlanEntry(1, "Dawn Plains", 10, "material.pine_branch", "animal.rabbit.pelt", "food.apples"),
            new RegionPlanEntry(2, "Windward Hills", 12, "material.hemp", "material.flint", "material.pine_cone"),
            new RegionPlanEntry(3, "Mistwood", 11, "material.nettle_fiber", "material.clay", "ingredient.rock_salt"),
            new RegionPlanEntry(4, "Fern Hollow", 13, "material.limestone", "food.strawberries", "ingredient.mushrooms"),
            new RegionPlanEntry(5, "Cloudstep Mountains", 10, "ingredient.oats", "material.pebbles", "food.berries"),
            new RegionPlanEntry(6, "Reedwater Delta", 14, "material.reeds", "ingredient.wheat", "material.sand"),
            new RegionPlanEntry(7, "Whispering Marsh", 12, "material.birch_branch", "animal.beaver.pelt", "food.blackberries"),
            new RegionPlanEntry(8, "Amber Riverbanks", 13, "material.flax_fiber", "material.copper_ore", "ingredient.carrots"),
            new RegionPlanEntry(9, "Bluewater Basin", 13, "ingredient.barley", "material.quartz", "food.blueberries"),
            new RegionPlanEntry(10, "Sunlit Uplands", 15, "material.sandstone", "ingredient.truffle", "ingredient.flax_seeds"),
            new RegionPlanEntry(11, "Mirror Lakes", 13, "ingredient.rice", "ingredient.tomatoes", "ingredient.honey"),
            new RegionPlanEntry(12, "Frostveil Taiga", 13, "material.tin_ore", "material.yellow_ochre", "material.cattail_fiber"),
            new RegionPlanEntry(13, "Golden Grasslands", 14, "material.mesquite_branch", "animal.turkey.feathers", "ingredient.corn"),
            new RegionPlanEntry(14, "Ember Canyons", 18, "material.cotton_fiber", "material.iron_ore", "ingredient.beans"),
            new RegionPlanEntry(15, "Red Dune Desert", 18, "material.granite", "ingredient.chickpeas", "ingredient.pumpkins"),
            new RegionPlanEntry(16, "Hidden Oasis", 14, "material.slate", "material.turquoise", "ingredient.soybeans"),
            new RegionPlanEntry(17, "Windswept Plateau", 16, "ingredient.sugar_cane", "ingredient.green_chili", "ingredient.sunflower_seeds"),
            new RegionPlanEntry(18, "Snowmelt Highlands", 15, "material.silver_ore", "ingredient.wild_mint", "material.agave_fiber"),
            new RegionPlanEntry(19, "Sunset Savanna", 16, "material.oak_branch", "animal.boar.hide", "ingredient.potatoes"),
            new RegionPlanEntry(20, "Stormwatch Cliffs", 17, "material.marble", "material.gold_ore", "ingredient.cacao_beans"),
            new RegionPlanEntry(21, "Crescent Beaches", 20, "material.basalt", "material.emerald", "ingredient.coffee_beans"),
            new RegionPlanEntry(22, "Rainforest Coast", 19, "material.obsidian", "material.ruby", "ingredient.vanilla_pods"),
            new RegionPlanEntry(23, "Silverwater Estuary", 17, "material.sapphire", "material.lapis_lazuli", "ingredient.seaweed"),
            new RegionPlanEntry(24, "Coral Shores", 19, "material.diamond", "food.coconuts", "material.bamboo_fiber") };

        public static RegionPlanEntry Find(int number) => number >= 1 && number <= All.Count ? All[number - 1] : null;
    }
}
