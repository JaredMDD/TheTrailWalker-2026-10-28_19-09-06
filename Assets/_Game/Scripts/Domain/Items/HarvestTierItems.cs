namespace TheTrailWalker.Domain.Items
{
    // Tier 2-4 harvest and fishery items (plan sections 6.2.5, 6.2.7, 7.1.1). Deliberately NOT part of PrototypeItems.All, which
    // is also the world-fixture spawn set; these resolve through Items.Find. Birch sap, mesquite pods and acorns already live in
    // PrototypeItems and are reused by the harvest tables.
    public static class HarvestTierItems
    {
        private static ItemDefinition Branch(string wood, string name) => new ItemDefinition($"material.{wood}_branch", name + " branch", 1200, 200);
        private static ItemDefinition Log(string wood, string name) => new ItemDefinition($"material.{wood}_log", name + " log", 1000, 500);
        private static ItemDefinition Pelt(string id, string name) => new ItemDefinition(id, name, 480, 480);
        private static ItemDefinition Meat(string species) => new ItemDefinition($"animal.{species}.meat", $"Raw {species} meat", 900, 558, category: ItemCategory.RawIngredient);
        private static ItemDefinition Fish(string id, string name) => new ItemDefinition(id, name, 1100, 427, category: ItemCategory.RawIngredient);
        private static ItemDefinition Shellfish(string id, string name) => new ItemDefinition(id, name, 900, 427, category: ItemCategory.RawIngredient);

        public static readonly ItemDefinition[] All = {
            Branch("birch", "Birch"), Log("birch", "Birch"), Branch("mesquite", "Mesquite"), Log("mesquite", "Mesquite"), Branch("oak", "Oak"), Log("oak", "Oak"),
            Pelt("animal.beaver.pelt", "Beaver pelt"), Pelt("animal.turkey.feathers", "Turkey feathers"), Pelt("animal.boar.hide", "Boar hide"),
            Meat("beaver"), Meat("turkey"), Meat("boar"),
            new ItemDefinition("animal.boar.tusk", "Boar tusk", 700, 300),
            Fish("fish.common_carp", "Common carp"), Fish("fish.channel_catfish", "Channel catfish"), Fish("fish.northern_pike", "Northern pike"), Fish("fish.walleye", "Walleye"),
            Fish("fish.salmon", "Salmon"), Fish("fish.sturgeon", "Sturgeon"), Fish("fish.brown_trout", "Brown trout"), Fish("fish.lake_trout", "Lake trout"),
            Fish("fish.longnose_gar", "Longnose gar"), Fish("fish.red_drum", "Red drum"), Fish("fish.red_snapper", "Red snapper"), Fish("fish.tuna", "Tuna"),
            Shellfish("fishery.shrimp", "Shrimp"), Shellfish("fishery.crab", "Crab"), Shellfish("fishery.squid", "Squid"), Shellfish("fishery.lobster", "Lobster"),
        };
    }
}
