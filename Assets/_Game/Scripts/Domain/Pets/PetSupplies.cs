using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Travel;

namespace TheTrailWalker.Domain.Pets
{
    // Milestone 15 spec section 3: what a pet can find. Every featured loose resource of RegionPlan belongs to exactly one
    // supply group at its region's tier, except the pelts and feathers (excluded by plan 13.5). A pet only finds IDs whose
    // region the player has discovered.
    public static class PetSupplies
    {
        private static readonly Dictionary<string, SupplyGroup> Groups = new Dictionary<string, SupplyGroup>();
        public static readonly IReadOnlyCollection<string> Excluded = new HashSet<string> {
            "animal.rabbit.pelt", "animal.beaver.pelt", "animal.turkey.feathers", "animal.boar.hide" };

        static PetSupplies()
        {
            void Add(SupplyGroup g, params string[] ids) { foreach (string id in ids) Groups.Add(id, g); }
            Add(SupplyGroup.Wood, "material.pine_branch", "material.birch_branch", "material.mesquite_branch", "material.oak_branch");
            // Rock salt counts as a mineral (spec decision 10).
            Add(SupplyGroup.StoneOre, "material.flint", "material.clay", "ingredient.rock_salt", "material.limestone", "material.pebbles", "material.sand",
                "material.copper_ore", "material.quartz", "material.sandstone", "material.tin_ore", "material.yellow_ochre", "material.iron_ore",
                "material.granite", "material.slate", "material.turquoise", "material.silver_ore", "material.marble", "material.gold_ore",
                "material.basalt", "material.emerald", "material.obsidian", "material.ruby", "material.sapphire", "material.lapis_lazuli", "material.diamond");
            // Reeds count as a fibre plant (spec decision 10).
            Add(SupplyGroup.Fibers, "material.hemp", "material.nettle_fiber", "material.reeds", "material.flax_fiber", "material.cattail_fiber",
                "material.cotton_fiber", "material.agave_fiber", "material.bamboo_fiber");
            // Pine cone and honey count as gathered food (spec decision 10).
            Add(SupplyGroup.Produce, "food.apples", "material.pine_cone", "ingredient.mushrooms", "food.strawberries", "ingredient.oats", "food.berries",
                "ingredient.wheat", "food.blackberries", "ingredient.carrots", "ingredient.barley", "food.blueberries", "ingredient.rice",
                "ingredient.tomatoes", "ingredient.honey", "ingredient.corn", "ingredient.beans", "ingredient.chickpeas", "ingredient.pumpkins",
                "ingredient.soybeans", "ingredient.sugar_cane", "ingredient.green_chili", "ingredient.wild_mint", "ingredient.potatoes",
                "ingredient.cacao_beans", "ingredient.coffee_beans", "ingredient.vanilla_pods", "ingredient.seaweed", "food.coconuts");
            Add(SupplyGroup.Seeds, "ingredient.flax_seeds", "ingredient.sunflower_seeds");
            Add(SupplyGroup.Truffle, "ingredient.truffle");
        }

        public static bool TryGroup(string itemId, out SupplyGroup group) => Groups.TryGetValue(itemId ?? "", out group);
        public static IEnumerable<string> All => Groups.Keys;

        // The region whose featured list holds the ID (RegionPlan is the single source), or 0.
        public static int RegionOf(string itemId)
        {
            foreach (RegionPlanEntry r in RegionPlan.All) if (r.Featured.Contains(itemId)) return r.Number;
            return 0;
        }
        public static int TierOf(int region) => (region - 1) / 6 + 1;

        // The discovered IDs of a group, in RegionPlan order.
        public static List<string> Eligible(SupplyGroup group, ICollection<int> discoveredRegions)
        {
            var ids = new List<string>();
            foreach (RegionPlanEntry r in RegionPlan.All)
            {
                if (discoveredRegions == null || !discoveredRegions.Contains(r.Number)) continue;
                foreach (string id in r.Featured) if (Groups.TryGetValue(id, out SupplyGroup g) && g == group && !ids.Contains(id)) ids.Add(id);
            }
            return ids;
        }

        // A pet's authored weights over the groups that have at least one discovered ID, renormalised to sum to 1. All zero
        // when nothing is eligible (spec decision 8: no reward, nothing banked).
        public static Dictionary<SupplyGroup, double> Effective(PetDefinition pet, ICollection<int> discoveredRegions)
        {
            var result = new Dictionary<SupplyGroup, double>();
            double total = 0;
            foreach (SupplyGroup g in Enum.GetValues(typeof(SupplyGroup)))
            {
                int w = Eligible(g, discoveredRegions).Count > 0 ? pet.Weights[g] : 0;
                result[g] = w; total += w;
            }
            foreach (SupplyGroup g in result.Keys.ToList()) result[g] = total > 0 ? result[g] / total : 0;
            return result;
        }
    }
}
