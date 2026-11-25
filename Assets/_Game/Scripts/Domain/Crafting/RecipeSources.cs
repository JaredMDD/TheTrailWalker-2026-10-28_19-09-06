using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Missions;
using TheTrailWalker.Domain.Settlement;

namespace TheTrailWalker.Domain.Crafting
{
    // Whether every ingredient source of a recipe is reachable (the balloon box gate, Decision 4). Order: tier, regions of raw inputs,
    // refined inputs, prerequisite recipes; reason names the first failure. Gold and the bag never matter. BakeryGate keeps its own table.
    public static class RecipeSources
    {
        public static bool Reachable(RecipeDefinition r, ICollection<int> regions, int reachedTier, CraftingState crafting, out string reason)
        {
            reason = "";
            if (reachedTier < r.Tier) { reason = "Tier " + r.Tier + " not reached"; return false; }
            var refined = new List<string>();
            foreach (RecipeInput input in r.Inputs)
            {
                if (Makers(input.ItemId).Any()) { refined.Add(input.ItemId); continue; }
                if (!RawReachable(input.ItemId, regions, reachedTier)) { reason = "Source of " + input.ItemId + " not discovered"; return false; }
            }
            foreach (string id in refined)
                if (!crafting.Known.Any(k => Makers(id).Any(m => m.Id == k)) && !Sold(id, regions, reachedTier)) { reason = "No maker recipe known for " + id; return false; }
            foreach (string need in Prerequisites(r))
                if (!crafting.Knows(need)) { reason = "Needs recipe " + need; return false; }
            return true;
        }

        private static IEnumerable<RecipeDefinition> Makers(string itemId) => RecipeBook.All.Where(m => !m.Retired && m.OutputId == itemId);

        // Filling needs oak planks made, bottling needs its barrel's filling recipe.
        private static IEnumerable<string> Prerequisites(RecipeDefinition r)
        {
            if (r.Id.StartsWith("recipe.barrel.")) yield return "recipe.material.oak_plank";
            foreach (RecipeInput i in r.Inputs)
                if (i.ItemId.EndsWith("_barrel")) foreach (RecipeDefinition m in Makers(i.ItemId)) yield return m.Id;
        }

        private static bool Sold(string itemId, ICollection<int> regions, int tier) =>
            ShopCatalog.All.Any(e => e.ItemId == itemId && ShopCatalog.Eligible(e.ShopId, regions, tier).Contains(e));

        private static bool RawReachable(string itemId, ICollection<int> regions, int tier)
        {
            if (ItemSources.LooseRegions(itemId).Any(regions.Contains)) return true;
            if (ItemSources.TryHarvest(itemId, out _, out int[] h) && h.Any(regions.Contains)) return true;
            return Sold(itemId, regions, tier);
        }
    }
}
