using System.Collections.Generic;

namespace TheTrailWalker.Domain.Settlement
{
    // One material the house upgrade asks for, with where the player earns it.
    public sealed class UpgradeRequirement
    {
        public string ItemId { get; }
        public int Quantity { get; }
        public string Source { get; }

        public UpgradeRequirement(string itemId, int quantity, string source)
        {
            ItemId = itemId; Quantity = quantity; Source = source;
        }
    }

    // What each house upgrade costs (PROJECT_RECOMENDATIONS section 13.1: 25 x tier per material, 30 x tier on levels that
    // add a pet slot). Levels 2-4 exist; the player's cap (HomeState.PlayerMaxLevel) is 4. Materials with no way to earn
    // them yet say so: the sheet still lists them so the player sees what the next level asks (user, November 25, 2026).
    public static class HouseUpgrades
    {
        private static readonly IReadOnlyList<UpgradeRequirement> LevelTwo = new List<UpgradeRequirement>
        {
            new UpgradeRequirement("item.pine_beam", 25, "From: Lumberjack mission L4"),
            new UpgradeRequirement("item.fired_bricks", 25, "From: Lumberjack mission L7"),
            new UpgradeRequirement("item.nettle_cloth", 25, "From: Explorer mission E15"),
            new UpgradeRequirement("item.reed_braid", 25, "From: Gatherer mission G5"),
            new UpgradeRequirement("item.pine_panel", 25, "From: Lumberjack mission L11"),
        }.AsReadOnly();

        public const string NoSourceYet = "No way to get it yet";

        // Level 3, tier 2 (50 each); pets unlock here, the first slot is free so it stays at 25 x tier.
        private static readonly IReadOnlyList<UpgradeRequirement> LevelThree = new List<UpgradeRequirement>
        {
            new UpgradeRequirement("item.birch_beam", 50, NoSourceYet),
            new UpgradeRequirement("item.limestone_tiles", 50, "From: Lumberjack mission L8"),
            new UpgradeRequirement("item.fine_beaver_leather", 50, NoSourceYet),
            new UpgradeRequirement("item.glass_panels", 50, NoSourceYet),
            new UpgradeRequirement("item.mortar", 50, "From: Lumberjack mission L5"),
        }.AsReadOnly();

        // Level 4, tier 2, adds the second pet slot (60 each).
        private static readonly IReadOnlyList<UpgradeRequirement> LevelFour = new List<UpgradeRequirement>
        {
            new UpgradeRequirement("item.birch_panel", 60, NoSourceYet),
            new UpgradeRequirement("item.sandstone_tiles", 60, NoSourceYet),
            new UpgradeRequirement("item.fine_hemp_cloth", 60, "From: Explorer mission E10"),
            new UpgradeRequirement("item.fine_rabbit_leather", 60, "From: Hunter mission H7"),
            new UpgradeRequirement("item.copper_fittings", 60, NoSourceYet),
        }.AsReadOnly();

        private static readonly IReadOnlyList<UpgradeRequirement> None = new List<UpgradeRequirement>().AsReadOnly();

        public static IReadOnlyList<UpgradeRequirement> For(int targetLevel)
        {
            switch (targetLevel)
            {
                case 2: return LevelTwo;
                case 3: return LevelThree;
                case 4: return LevelFour;
                default: return None;
            }
        }
    }
}
