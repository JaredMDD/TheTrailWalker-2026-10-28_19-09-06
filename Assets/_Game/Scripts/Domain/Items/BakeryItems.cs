namespace TheTrailWalker.Domain.Items
{
    // Bakery foods and refined inputs with no item definition yet (milestone 14 slice 2). Like ShopItems these resolve through
    // Items.Find only. Footprints copy the nearest existing food: Berry oat cakes (900 x 300) for small bakes, Premium apple pie
    // (900 x 349) for pies, cakes and flatbreads; placeholders until art lands.
    public static class BakeryItems
    {
        private static ItemDefinition Small(string id, string name, int hearts) => new ItemDefinition(id, name, 900, 300, foodHearts: hearts, preparedFood: true);
        private static ItemDefinition Pie(string id, string name, int hearts) => new ItemDefinition(id, name, 900, 349, foodHearts: hearts, preparedFood: true);
        private static ItemDefinition Refined(string id, string name) => new ItemDefinition(id, name, 900, 444, category: ItemCategory.RefinedIngredient); // Wheat flour

        public static readonly ItemDefinition[] All = {
            Small("item.pine_nut_oat_cookies", "Pine-nut oat cookies", 2), Small("item.wheat_bread", "Wheat bread", 2),
            Small("item.barley_bread", "Barley bread", 3), Small("item.pike_barley_cakes", "Pike barley cakes", 3), Small("item.blueberry_muffins", "Blueberry muffins", 3), Small("item.flax_oil_biscuits", "Flax-oil biscuits", 3),
            Pie("item.corn_flatbread", "Corn flatbread", 2), Small("item.mesquite_nut_cakes", "Mesquite nut cakes", 3), Small("item.sunflower_corn_bread", "Sunflower corn bread", 4), Pie("item.corn_turkey_pie", "Corn-turkey pie", 4),
            Small("item.potato_butter_rolls", "Potato butter rolls", 5), Small("item.coconut_macaroons", "Coconut macaroons", 5),
            Small("item.strawberry_cream_roll", "Strawberry cream roll", 10), Small("item.honey_brioche", "Honey brioche", 8), Pie("item.blackberry_cream_pastry", "Blackberry cream pastry", 10),
            Pie("item.pumpkin_cow_cheese_galette", "Pumpkin cow-cheese galette", 10), Small("item.mint_sugar_rolls", "Mint sugar rolls", 11),
            Pie("item.vanilla_cacao_celebration_cake", "Vanilla cacao celebration cake", 12), Pie("item.coconut_cream_tart", "Coconut cream tart", 12),
            Refined("ingredient.sugar", "Sugar"), Refined("ingredient.flax_oil", "Flax oil"), Refined("ingredient.cacao_powder", "Cacao powder"),
        };
    }
}
