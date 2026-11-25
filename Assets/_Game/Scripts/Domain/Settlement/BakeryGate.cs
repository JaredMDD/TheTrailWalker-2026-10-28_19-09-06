using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Crafting;

namespace TheTrailWalker.Domain.Settlement
{
    // Decides whether a premium bakery recipe may be offered or bought (section 9.4.4): tier reached, regions discovered, refined inputs accessible.
    // Gold and the bag never matter. Checked in that order; reason names the first failure.
    public static class BakeryGate
    {
        private static readonly Dictionary<string, (int[] Regions, string[] Inputs)> Sources = new Dictionary<string, (int[], string[])>
        {
            { "premium_apple_pie", (new[] { 1, 6 }, new[] { "wheat_flour", "cow_butter" }) },
            { "strawberry_cream_roll", (new[] { 4, 6 }, new[] { "wheat_flour", "cow_cream", "egg" }) },
            { "honey_brioche", (new[] { 6, 11 }, new[] { "wheat_flour", "cow_butter" }) },
            { "blackberry_cream_pastry", (new[] { 6, 7 }, new[] { "wheat_flour", "cow_cream" }) },
            { "pumpkin_cow_cheese_galette", (new[] { 6, 15 }, new[] { "wheat_flour", "cow_cheese", "cow_butter" }) },
            { "mint_sugar_rolls", (new[] { 6, 17, 18 }, new[] { "wheat_flour", "sugar", "cow_butter" }) },
            { "vanilla_cacao_celebration_cake", (new[] { 6, 17, 20, 22 }, new[] { "wheat_flour", "cow_cream", "cacao_powder", "sugar" }) },
            { "coconut_cream_tart", (new[] { 6, 17, 24 }, new[] { "wheat_flour", "cow_cream", "sugar" }) },
        };

        // reserved: recipes held by unclaimed mystery boxes (MysteryBoxes.Reserved), never offered or sold here (slice 2, Decision 5).
        public static bool Eligible(string recipeId, ICollection<int> regions, int reachedTier, CraftingState crafting, out string reason, ISet<string> reserved = null)
        {
            reason = "Unknown recipe";
            RecipeDefinition recipe = RecipeBook.Find(recipeId);
            if (recipe == null || !Sources.TryGetValue(recipeId.Substring("recipe.food.".Length), out var src)) return false;
            if (crafting.Knows(recipeId)) { reason = "Already known"; return false; }
            if (reserved != null && reserved.Contains(recipeId)) { reason = "Reserved in a mystery box"; return false; }
            if (reachedTier < recipe.Tier) { reason = "Tier " + recipe.Tier + " not reached"; return false; }
            foreach (int r in src.Regions) if (!regions.Contains(r)) { reason = "Region " + r + " not discovered"; return false; }
            foreach (string input in src.Inputs)
                if (!Accessible("ingredient." + input, regions, reachedTier, crafting)) { reason = "Missing input " + input; return false; }
            reason = ""; return true;
        }

        // A refined input is accessible when a live recipe the player knows makes it, or any shop's pool sells it.
        private static bool Accessible(string itemId, ICollection<int> regions, int tier, CraftingState crafting) =>
            crafting.Known.Any(id => { RecipeDefinition r = RecipeBook.Find(id); return r != null && !r.Retired && r.OutputId == itemId; })
            || ShopCatalog.All.Any(e => e.ItemId == itemId && ShopCatalog.Eligible(e.ShopId, regions, tier).Contains(e));
    }
}
