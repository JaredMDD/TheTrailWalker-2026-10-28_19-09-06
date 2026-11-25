namespace TheTrailWalker.Domain.Items
{
    // Mystery-box recipe products and the inputs that had no item definition yet (milestone 14 slice 3, Decision 3). Like
    // ShopItems these resolve through Items.Find only. Footprints copy the nearest existing item; placeholders until art sizes land.
    public static class BoxItems
    {
        private static ItemDefinition Dish(string snake, string name, int hearts) => new ItemDefinition("item." + snake, name, 900, 456, foodHearts: hearts, preparedFood: true); // Apple-oat compote
        private static ItemDefinition Bottle(string snake, string name, int hearts) => new ItemDefinition("item." + snake, name, 485, 520, foodHearts: hearts, preparedFood: true); // Apple juice
        // A filled barrel is a trade good and bottling input: no hearts, never eaten (9.4.5).
        private static ItemDefinition Barrel(string snake, string name) => new ItemDefinition("item." + snake + "_barrel", name + " barrel", 900, 900);
        private static ItemDefinition Keepsake(string snake, string name) => new ItemDefinition("item." + snake, name, 900, 757); // Pine bird
        private static ItemDefinition Plank(string id, string name) => new ItemDefinition(id, name, 1100, 550); // Pine plank
        private static ItemDefinition Metal(string id, string name) => new ItemDefinition(id, name, 480, 306); // Flint
        private static ItemDefinition Refined(string id, string name) => new ItemDefinition(id, name, 900, 444, category: ItemCategory.RefinedIngredient); // Wheat flour

        public static readonly ItemDefinition[] All = {
            // 9.4.2 and 9.4.3 products.
            Dish("truffle_butter_crab", "Truffle-butter crab", 9), Dish("creamy_crab_soup", "Creamy crab soup", 10), Dish("honey_blackberry_biscuits", "Honey-blackberry biscuits", 9),
            Dish("crispy_mint_squid", "Crispy mint squid", 10), Dish("squid_rice_bowl", "Squid rice bowl", 11), Dish("mint_cow_cheese_flatbread", "Mint cow-cheese flatbread", 10),
            Dish("coconut_chili_lobster", "Coconut-chili lobster", 11), Dish("lobster_rice_bowl", "Lobster rice bowl", 12), Dish("cacao_coconut_tart", "Cacao-coconut tart", 12),
            Dish("beaver_barley_hotpot", "Beaver-barley hotpot", 10), Dish("blackberry_honey_oats", "Blackberry-honey oats", 10), Dish("turkey_bean_bowl", "Turkey-bean bowl", 11),
            Dish("pumpkin_chickpea_pot", "Pumpkin-chickpea pot", 11), Dish("boar_potato_chili", "Boar-potato chili", 12), Dish("coconut_cacao_pudding", "Coconut-cacao pudding", 12),
            Dish("truffle_cow_cheese_flatbread", "Truffle cow-cheese flatbread", 10), Dish("turkey_truffle_bean_pot", "Turkey-truffle bean pot", 11),
            Keepsake("birch_wax_bee", "Birch-wax bee toy"), Keepsake("painted_turkey", "Painted turkey carving"), Keepsake("silver_sun_brooch", "Silver sun brooch"),
            Keepsake("obsidian_boar", "Obsidian boar carving"),
            // 9.4.5 barrels and bottles: ciders and beers 11 hearts; wines, mead, rum and whiskey 12.
            Barrel("apple_cider", "Apple cider"), Bottle("apple_cider", "Apple cider", 11), Barrel("strawberry_wine", "Strawberry wine"), Bottle("strawberry_wine", "Strawberry wine", 12),
            Barrel("blackberry_wine", "Blackberry wine"), Bottle("blackberry_wine", "Blackberry wine", 12), Barrel("blueberry_wine", "Blueberry wine"), Bottle("blueberry_wine", "Blueberry wine", 12),
            Barrel("honey_mead", "Spiced honey mead"), Bottle("honey_mead", "Spiced honey mead", 12), Barrel("barley_ale", "Barley ale"), Bottle("barley_ale", "Barley ale", 11),
            Barrel("wheat_beer", "Wheat beer"), Bottle("wheat_beer", "Wheat beer", 11), Barrel("cane_rum", "Cane rum"), Bottle("cane_rum", "Cane rum", 12),
            Barrel("corn_whiskey", "Charred-oak whiskey"), Bottle("corn_whiskey", "Charred-oak whiskey", 12),
            // 13.4.2 ranch dishes.
            Dish("chicken_mushroom_pie", "Chicken mushroom pie", 10), Dish("apple_glazed_chicken", "Apple-glazed chicken", 9), Dish("honey_roast_duck", "Honey-roast duck", 10),
            Dish("duck_blueberry_rice", "Duck blueberry rice", 9), Dish("mint_lamb_stew", "Mint lamb stew", 11), Dish("lamb_chili_flatbread", "Lamb chili flatbread", 10),
            Dish("coffee_braised_beef", "Coffee-braised beef", 12), Dish("beef_cacao_chili", "Beef cacao chili", 11),
            // 9.5.1 keepsakes.
            Keepsake("quartz_prism_lantern", "Quartz prism lantern"), Keepsake("tin_quartz_music_box", "Tin-and-quartz music box"), Keepsake("silver_turquoise_buckle", "Silver turquoise belt buckle"),
            Keepsake("bronze_turquoise_compass", "Bronze turquoise compass"), Keepsake("diamond_pocket_watch", "Diamond pocket watch"), Keepsake("royal_gem_chalice", "Royal gem chalice"),
            Keepsake("lapis_jewel_box", "Lapis jewel box"),
            // Inputs with no definition yet.
            new ItemDefinition("ingredient.truffle", "Truffle", 480, 322, category: ItemCategory.RawIngredient), // Mushrooms
            Refined("ingredient.sunflower_oil", "Sunflower oil"), Refined("ingredient.corn_flour", "Corn flour"), Refined("item.roasted_coffee", "Roasted coffee"),
            new ItemDefinition("item.beeswax", "Beeswax", 900, 470), // Cow butter
            Plank("item.birch_plank", "Birch plank"), Plank("item.mesquite_plank", "Mesquite plank"), Plank("item.oak_plank", "Oak plank"),
            new ItemDefinition("item.mesquite_charcoal", "Mesquite charcoal", 900, 481), // Pine charcoal
            new ItemDefinition("item.fine_beaver_leather", "Fine beaver leather", 600, 420), // Fine rabbit leather
            new ItemDefinition("item.glass", "Glass", 485, 520), new ItemDefinition("item.glass_bottle", "Glass bottle", 485, 520), // Apple juice
            Metal("item.tin_ingot", "Tin ingot"), Metal("item.bronze_ingot", "Bronze ingot"), Metal("item.silver_ingot", "Silver ingot"), Metal("item.gold_ingot", "Gold ingot"),
            Metal("item.copper_fittings", "Copper fittings"), Metal("item.iron_fittings", "Iron fittings"), Metal("item.steel_fittings", "Steel fittings"),
            Metal("item.cut_quartz", "Cut quartz"), Metal("item.cut_turquoise", "Cut turquoise"), Metal("item.cut_ruby", "Cut ruby"), Metal("item.cut_sapphire", "Cut sapphire"),
            Metal("item.cut_diamond", "Cut diamond"), Metal("item.polished_lapis", "Polished lapis"),
        };
    }
}
