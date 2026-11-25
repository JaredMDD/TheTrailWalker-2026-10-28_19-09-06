namespace TheTrailWalker.Domain.Crafting
{
    // The balloon mystery box's optional recipes (PROJECT_RECOMENDATIONS 9.4.2-9.4.3, 9.4.5, 13.4.2, 9.5.1): 54 here plus the 5
    // tier-one ones in Tier1Recipes make the 59-recipe pool. None is a mission reward. The tables give no base times, so Food
    // uses the tier-one 3 s and Misc (barrel filling included) the tier-one 10 s. Catalog entries grant no knowledge.
    public static class BoxRecipes
    {
        private const string Source = "Optional balloon recipe; exact ingredient sources must be discovered and accessible";
        private static RecipeDefinition Food(string name, string snake, int tier, int yield, params RecipeInput[] inputs) =>
            new RecipeDefinition("recipe.food." + snake, name, RecipeRole.Food, tier, "item." + snake, yield, 3.0f, Source, inputs, mysteryBoxEligible: true);
        private static RecipeDefinition Misc(string name, string snake, int tier, params RecipeInput[] inputs) =>
            new RecipeDefinition("recipe.misc." + snake, name, RecipeRole.Misc, tier, "item." + snake, 1, 10.0f, Source, inputs, mysteryBoxEligible: true);
        private static RecipeInput I(string id, int quantity) => new RecipeInput(id, quantity);

        // 9.4.5: filling = 4 Oak planks + 2 Iron fittings + the aging produce -> 1 filled barrel (levelled like Misc); bottling =
        // the barrel + 8 Glass bottles -> 8 (ordinary Food). Both tier 4. The barrel input ties a bottling recipe to its filling.
        private static RecipeDefinition Filling(string name, string snake, params RecipeInput[] produce)
        {
            var inputs = new RecipeInput[produce.Length + 2];
            inputs[0] = I("item.oak_plank", 4); inputs[1] = I("item.iron_fittings", 2); produce.CopyTo(inputs, 2);
            return new RecipeDefinition("recipe.barrel." + snake, name + " barrel", RecipeRole.Misc, 4, "item." + snake + "_barrel", 1, 10.0f, Source, inputs, mysteryBoxEligible: true);
        }
        private static RecipeDefinition Bottling(string name, string snake) => Food(name, snake, 4, 8, I("item." + snake + "_barrel", 1), I("item.glass_bottle", 8));

        public static readonly RecipeDefinition[] All = {
            // 9.4.2 and 9.4.3 (the other five are in Tier1Recipes).
            Food("Truffle-butter crab", "truffle_butter_crab", 2, 1, I("fishery.crab", 1), I("ingredient.truffle", 1), I("ingredient.cow_butter", 1)),
            Food("Creamy crab soup", "creamy_crab_soup", 2, 1, I("fishery.crab", 1), I("ingredient.cow_cream", 1), I("ingredient.carrots", 1)),
            Food("Honey-blackberry biscuits", "honey_blackberry_biscuits", 2, 2, I("ingredient.wheat_flour", 2), I("ingredient.honey", 1), I("food.blackberries", 1)),
            Food("Crispy mint squid", "crispy_mint_squid", 3, 1, I("fishery.squid", 1), I("ingredient.sunflower_oil", 1), I("ingredient.wild_mint", 1)),
            Food("Squid rice bowl", "squid_rice_bowl", 3, 1, I("fishery.squid", 1), I("ingredient.rice", 1), I("ingredient.green_chili", 1)),
            Food("Mint cow-cheese flatbread", "mint_cow_cheese_flatbread", 3, 1, I("ingredient.wheat_flour", 1), I("ingredient.cow_cheese", 1), I("ingredient.wild_mint", 1)),
            Food("Coconut-chili lobster", "coconut_chili_lobster", 4, 1, I("fishery.lobster", 1), I("food.coconuts", 1), I("ingredient.green_chili", 1)),
            Food("Lobster rice bowl", "lobster_rice_bowl", 4, 1, I("fishery.lobster", 1), I("ingredient.rice", 1), I("ingredient.seaweed", 1)),
            Food("Cacao-coconut tart", "cacao_coconut_tart", 4, 1, I("ingredient.cacao_powder", 1), I("food.coconuts", 1), I("ingredient.wheat_flour", 1)),
            Food("Beaver-barley hotpot", "beaver_barley_hotpot", 2, 1, I("animal.beaver.meat", 2), I("ingredient.barley", 2), I("ingredient.carrots", 1)),
            Food("Blackberry-honey oats", "blackberry_honey_oats", 2, 1, I("food.blackberries", 3), I("ingredient.oats", 2), I("ingredient.honey", 1)),
            Food("Turkey-bean bowl", "turkey_bean_bowl", 3, 1, I("animal.turkey.meat", 2), I("ingredient.beans", 2), I("ingredient.corn_flour", 1)),
            Food("Pumpkin-chickpea pot", "pumpkin_chickpea_pot", 3, 1, I("ingredient.pumpkins", 2), I("ingredient.chickpeas", 2), I("ingredient.cow_cheese", 1)),
            Food("Boar-potato chili", "boar_potato_chili", 4, 1, I("animal.boar.meat", 2), I("ingredient.potatoes", 2), I("ingredient.green_chili", 1)),
            Food("Coconut-cacao pudding", "coconut_cacao_pudding", 4, 1, I("food.coconuts", 2), I("ingredient.cacao_powder", 1), I("food.milk.cow", 1)),
            Misc("Birch-wax bee toy", "birch_wax_bee", 2, I("item.birch_plank", 1), I("item.beeswax", 1), I("animal.beaver.pelt", 1)),
            Misc("Painted turkey carving", "painted_turkey", 3, I("item.mesquite_plank", 1), I("animal.turkey.feathers", 1), I("material.yellow_ochre", 1)),
            Misc("Silver sun brooch", "silver_sun_brooch", 3, I("item.silver_ingot", 1), I("item.cut_turquoise", 1), I("item.flax_cord", 1)),
            Misc("Obsidian boar carving", "obsidian_boar", 4, I("item.oak_plank", 1), I("material.obsidian", 1), I("ingredient.sunflower_oil", 1)),
            Food("Truffle cow-cheese flatbread", "truffle_cow_cheese_flatbread", 2, 1, I("ingredient.truffle", 2), I("ingredient.wheat_flour", 1), I("ingredient.cow_cheese", 1)),
            Food("Turkey-truffle bean pot", "turkey_truffle_bean_pot", 3, 1, I("ingredient.truffle", 1), I("animal.turkey.meat", 2), I("ingredient.beans", 2)),
            // 9.4.5: oak-barrel drinks.
            Filling("Apple cider", "apple_cider", I("food.apples", 12)), Bottling("Apple cider", "apple_cider"),
            Filling("Strawberry wine", "strawberry_wine", I("food.strawberries", 12), I("ingredient.sugar", 2)), Bottling("Strawberry wine", "strawberry_wine"),
            Filling("Blackberry wine", "blackberry_wine", I("food.blackberries", 12), I("ingredient.sugar", 2)), Bottling("Blackberry wine", "blackberry_wine"),
            Filling("Blueberry wine", "blueberry_wine", I("food.blueberries", 12), I("ingredient.honey", 2)), Bottling("Blueberry wine", "blueberry_wine"),
            Filling("Spiced honey mead", "honey_mead", I("ingredient.honey", 10), I("ingredient.wild_mint", 1)), Bottling("Spiced honey mead", "honey_mead"),
            Filling("Barley ale", "barley_ale", I("ingredient.barley", 12), I("ingredient.honey", 2)), Bottling("Barley ale", "barley_ale"),
            Filling("Wheat beer", "wheat_beer", I("ingredient.wheat", 8), I("ingredient.barley", 4)), Bottling("Wheat beer", "wheat_beer"),
            Filling("Cane rum", "cane_rum", I("ingredient.sugar_cane", 12), I("ingredient.sugar", 2)), Bottling("Cane rum", "cane_rum"),
            Filling("Charred-oak whiskey", "corn_whiskey", I("ingredient.corn", 8), I("ingredient.barley", 4), I("item.mesquite_charcoal", 1)), Bottling("Charred-oak whiskey", "corn_whiskey"),
            // 13.4.2: premium ranch recipes.
            Food("Chicken mushroom pie", "chicken_mushroom_pie", 1, 1, I("animal.chicken.meat", 1), I("ingredient.mushrooms", 2), I("ingredient.wheat_flour", 1)),
            Food("Apple-glazed chicken", "apple_glazed_chicken", 1, 1, I("animal.chicken.meat", 1), I("food.apples", 2), I("material.pine_branch", 1)),
            Food("Honey-roast duck", "honey_roast_duck", 2, 1, I("animal.duck.meat", 1), I("ingredient.honey", 1), I("material.birch_branch", 2)),
            Food("Duck blueberry rice", "duck_blueberry_rice", 2, 1, I("animal.duck.meat", 1), I("food.blueberries", 1), I("ingredient.rice", 1)),
            Food("Mint lamb stew", "mint_lamb_stew", 3, 1, I("animal.lamb.meat", 2), I("ingredient.wild_mint", 1), I("ingredient.chickpeas", 2)),
            Food("Lamb chili flatbread", "lamb_chili_flatbread", 3, 1, I("animal.lamb.meat", 1), I("item.corn_flatbread", 1), I("ingredient.green_chili", 1)),
            Food("Coffee-braised beef", "coffee_braised_beef", 4, 1, I("animal.beef.meat", 1), I("item.roasted_coffee", 1), I("ingredient.potatoes", 2)),
            Food("Beef cacao chili", "beef_cacao_chili", 4, 1, I("animal.beef.meat", 1), I("ingredient.cacao_powder", 1), I("ingredient.green_chili", 1)),
            // 9.5.1: precious-metal and gem keepsakes.
            Misc("Quartz prism lantern", "quartz_prism_lantern", 2, I("item.cut_quartz", 1), I("item.copper_fittings", 2), I("item.glass", 1)),
            Misc("Tin-and-quartz music box", "tin_quartz_music_box", 2, I("item.tin_ingot", 1), I("item.cut_quartz", 1), I("item.birch_plank", 1)),
            Misc("Silver turquoise belt buckle", "silver_turquoise_buckle", 3, I("item.silver_ingot", 1), I("item.cut_turquoise", 1), I("item.fine_beaver_leather", 1)),
            Misc("Bronze turquoise compass", "bronze_turquoise_compass", 3, I("item.bronze_ingot", 1), I("item.cut_turquoise", 1), I("item.glass", 1)),
            Misc("Diamond pocket watch", "diamond_pocket_watch", 4, I("item.gold_ingot", 1), I("item.cut_diamond", 1), I("item.steel_fittings", 1)),
            Misc("Royal gem chalice", "royal_gem_chalice", 4, I("item.gold_ingot", 1), I("item.cut_ruby", 1), I("item.cut_sapphire", 1)),
            Misc("Lapis jewel box", "lapis_jewel_box", 4, I("item.oak_plank", 2), I("item.polished_lapis", 1), I("item.gold_ingot", 1)),
        };
    }
}
