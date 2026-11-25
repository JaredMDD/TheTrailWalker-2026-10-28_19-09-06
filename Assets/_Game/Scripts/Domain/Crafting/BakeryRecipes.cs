namespace TheTrailWalker.Domain.Crafting
{
    // Bakery Food recipes (shop listings spec 6, PROJECT_RECOMENDATIONS 9.4.4). Common recipes are mission rewards; premium ones are
    // bakery recipe offers. Tier 2-4 mission sources are deferred (no tier 2-4 mission catalog yet). Catalog entries grant no knowledge.
    public static class BakeryRecipes
    {
        private static RecipeDefinition Food(string name, string snake, int tier, int yield, string source, params RecipeInput[] inputs) =>
            new RecipeDefinition("recipe.food." + snake, name, RecipeRole.Food, tier, "item." + snake, yield, 3.0f, source, inputs);
        private static RecipeInput I(string id, int quantity) => new RecipeInput(id, quantity);
        private const string Offer = "Bakery recipe offer; exact ingredient sources must be discovered and accessible";

        public static readonly RecipeDefinition[] All = {
            Food("Pine-nut oat cookies", "pine_nut_oat_cookies", 1, 2, "Gatherer G16", I("ingredient.oats", 2), I("ingredient.pine_nuts", 1), I("ingredient.cow_butter", 1)),
            Food("Wheat bread", "wheat_bread", 1, 2, "Gatherer G17", I("ingredient.wheat_flour", 3), I("ingredient.rock_salt", 1)),
            Food("Blueberry muffins", "blueberry_muffins", 2, 2, "Tier-two mission T2-G23 (not in the catalog yet)", I("ingredient.wheat_flour", 2), I("food.blueberries", 2), I("ingredient.egg", 1)),
            Food("Flax-oil biscuits", "flax_oil_biscuits", 2, 2, "Tier-two mission T2-G24 (not in the catalog yet)", I("ingredient.wheat_flour", 2), I("ingredient.flax_oil", 1)),
            Food("Potato butter rolls", "potato_butter_rolls", 4, 2, "Tier-four mission T4-G23 (not in the catalog yet)", I("ingredient.wheat_flour", 2), I("ingredient.potatoes", 2), I("ingredient.cow_butter", 1)),
            Food("Coconut macaroons", "coconut_macaroons", 4, 2, "Tier-four mission T4-G24 (not in the catalog yet)", I("food.coconuts", 2), I("ingredient.egg", 1), I("ingredient.sugar", 1)),
            Food("Strawberry cream roll", "strawberry_cream_roll", 1, 1, Offer, I("ingredient.wheat_flour", 2), I("food.strawberries", 2), I("ingredient.cow_cream", 1), I("ingredient.egg", 1)),
            Food("Honey brioche", "honey_brioche", 2, 1, Offer, I("ingredient.wheat_flour", 2), I("ingredient.cow_butter", 1), I("ingredient.honey", 1)),
            Food("Blackberry cream pastry", "blackberry_cream_pastry", 2, 1, Offer, I("ingredient.wheat_flour", 2), I("food.blackberries", 2), I("ingredient.cow_cream", 1)),
            Food("Pumpkin cow-cheese galette", "pumpkin_cow_cheese_galette", 3, 1, Offer, I("ingredient.wheat_flour", 2), I("ingredient.pumpkins", 2), I("ingredient.cow_cheese", 1), I("ingredient.cow_butter", 1)),
            Food("Mint sugar rolls", "mint_sugar_rolls", 3, 1, Offer, I("ingredient.wheat_flour", 2), I("ingredient.wild_mint", 1), I("ingredient.sugar", 1), I("ingredient.cow_butter", 1)),
            Food("Vanilla cacao celebration cake", "vanilla_cacao_celebration_cake", 4, 1, Offer, I("ingredient.wheat_flour", 2), I("ingredient.cow_cream", 1), I("ingredient.cacao_powder", 1), I("ingredient.vanilla_pods", 1), I("ingredient.sugar", 1)),
            Food("Coconut cream tart", "coconut_cream_tart", 4, 1, Offer, I("ingredient.wheat_flour", 2), I("food.coconuts", 2), I("ingredient.cow_cream", 1), I("ingredient.sugar", 1)),
        };
    }
}
