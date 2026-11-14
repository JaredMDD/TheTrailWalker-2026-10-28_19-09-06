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

    // What each house upgrade costs. Only level 2 exists this milestone (spec 2026-11-14 section 1).
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

        private static readonly IReadOnlyList<UpgradeRequirement> None = new List<UpgradeRequirement>().AsReadOnly();

        public static IReadOnlyList<UpgradeRequirement> For(int targetLevel)
        {
            return targetLevel == 2 ? LevelTwo : None;
        }
    }
}
