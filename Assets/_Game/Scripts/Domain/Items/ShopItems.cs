namespace TheTrailWalker.Domain.Items
{
    // Shop pool goods that have no item definition yet (milestone 14, shop listings spec 5.1-5.5). Like HarvestTierItems these
    // resolve through Items.Find only. Footprints copy the nearest existing item of the same kind (placeholder until art lands).
    public static class ShopItems
    {
        private static ItemDefinition Fruit(string id, string name) => new ItemDefinition(id, name, 420, 480, foodHearts: 1); // Red berries
        private static ItemDefinition Round(string id, string name) => new ItemDefinition(id, name, 420, 480, category: ItemCategory.RawIngredient); // Apples
        private static ItemDefinition Stalk(string id, string name) => new ItemDefinition(id, name, 652, 850, category: ItemCategory.RawIngredient); // Oats
        private static ItemDefinition Seed(string id, string name) => new ItemDefinition(id, name, 480, 332, category: ItemCategory.RawIngredient); // Rock salt
        private static ItemDefinition Herb(string id, string name) => new ItemDefinition(id, name, 850, 593, category: ItemCategory.RawIngredient); // Tea herbs
        private static ItemDefinition Stone(string id, string name) => new ItemDefinition(id, name, 480, 280); // Limestone
        private static ItemDefinition Ore(string id, string name) => new ItemDefinition(id, name, 480, 306); // Flint
        private static ItemDefinition Fibre(string id, string name) => new ItemDefinition(id, name, 850, 300); // Hemp
        private static ItemDefinition Cord(string id, string name) => new ItemDefinition(id, name, 900, 655); // Hemp cord
        private static ItemDefinition Thread(string id, string name) => new ItemDefinition(id, name, 609, 900); // Hemp thread
        private static ItemDefinition Cloth(string id, string name) => new ItemDefinition(id, name, 900, 357); // Hemp cloth
        private static ItemDefinition Meat(string id, string name) => new ItemDefinition(id, name, 900, 558, category: ItemCategory.RawIngredient); // Raw rabbit meat

        public static readonly ItemDefinition[] All = {
            Fruit("food.blackberries", "Blackberries"), Fruit("food.blueberries", "Blueberries"), Fruit("food.coconuts", "Coconuts"),
            Stalk("ingredient.carrots", "Carrots"), Stalk("ingredient.corn", "Corn"), Stalk("ingredient.sugar_cane", "Sugar cane"),
            Round("ingredient.tomatoes", "Tomatoes"), Round("ingredient.pumpkins", "Pumpkins"), Round("ingredient.green_chili", "Green chili"), Round("ingredient.potatoes", "Potatoes"),
            Seed("ingredient.barley", "Barley"), Seed("ingredient.rice", "Rice"), Seed("ingredient.flax_seeds", "Flax seeds"), Seed("ingredient.beans", "Beans"),
            Seed("ingredient.chickpeas", "Chickpeas"), Seed("ingredient.soybeans", "Soybeans"), Seed("ingredient.sunflower_seeds", "Sunflower seeds"),
            Seed("ingredient.cacao_beans", "Cacao beans"), Seed("ingredient.coffee_beans", "Coffee beans"), Seed("ingredient.vanilla_pods", "Vanilla pods"),
            Herb("ingredient.lavender", "Lavender"), Herb("ingredient.wild_mint", "Wild mint"), Herb("ingredient.seaweed", "Seaweed"),
            new ItemDefinition("ingredient.honey", "Honey", 892, 900, category: ItemCategory.RawIngredient), // Cow milk
            Stone("material.sandstone", "Sandstone"), Stone("material.quartz", "Quartz"), Stone("material.granite", "Granite"), Stone("material.slate", "Slate"),
            Stone("material.marble", "Marble"), Stone("material.basalt", "Basalt"),
            Ore("material.yellow_ochre", "Yellow ochre"), Ore("material.copper_ore", "Copper ore"), Ore("material.tin_ore", "Tin ore"), Ore("material.iron_ore", "Iron ore"),
            Ore("material.silver_ore", "Silver ore"), Ore("material.gold_ore", "Gold ore"), Ore("material.turquoise", "Turquoise"), Ore("material.obsidian", "Obsidian"),
            Ore("material.lapis_lazuli", "Lapis lazuli"), Ore("material.emerald", "Emerald"), Ore("material.ruby", "Ruby"), Ore("material.sapphire", "Sapphire"), Ore("material.diamond", "Diamond"),
            Fibre("material.flax_fiber", "Flax fiber"), Fibre("material.cattail_fiber", "Cattail fiber"), Fibre("material.cotton_fiber", "Cotton fiber"),
            Fibre("material.agave_fiber", "Agave fiber"), Fibre("material.bamboo_fiber", "Bamboo fiber"),
            Cord("item.flax_cord", "Flax cord"), Cord("item.flax_stuffing", "Flax stuffing"), Cloth("item.flax_cloth", "Flax cloth"), Cord("item.cattail_cord", "Cattail cord"),
            new ItemDefinition("item.fine_flax_cloth", "Fine flax cloth", 900, 386), // Fine hemp cloth
            Cord("item.cotton_cord", "Cotton cord"), Thread("item.cotton_thread", "Cotton thread"), Cord("item.cotton_stuffing", "Cotton stuffing"), Cloth("item.cotton_cloth", "Cotton cloth"),
            Cord("item.agave_cord", "Agave cord"), new ItemDefinition("item.fine_cotton_cloth", "Fine cotton cloth", 900, 386), Cloth("item.bamboo_cloth", "Bamboo cloth"),
            Meat("animal.chicken.meat", "Raw chicken meat"), Meat("animal.duck.meat", "Raw duck meat"), Meat("animal.lamb.meat", "Raw lamb meat"), Meat("animal.beef.meat", "Raw beef"),
        };
    }
}
