using TheTrailWalker.Domain.Equipment;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker
{
    // Which model represents an item in the UI. Debug test gear (R4 test/spare gear) has no art of its own, so it borrows
    // the tier-1 wearable of its slot while keeping its own debug stats and durability (user decision, October 31,
    // 2026). Empty equipment slots show that tier-1 item as a faint silhouette.
    public static class ItemArt
    {
        public static ItemDefinition ModelFor(ItemDefinition definition)
        {
            if (definition != null && definition.GearSlot.HasValue && definition.Id.StartsWith("debug.gear."))
                return SlotSilhouette(definition.GearSlot.Value) ?? definition;
            // Debug harvest tools borrow the pine tool of their family; tier 3 ones show the mesquite tool through
            // Tier3Visuals instead (Tier 3 integration slice A, user November 24, 2026).
            if (definition != null && definition.Id.StartsWith("debug.tool.") && !Tier3Visuals.Has(definition.Id) && TheTrailWalker.Domain.Harvest.HarvestTools.TryGet(definition, out var spec))
                return System.Array.Find(PrototypeItems.All, d => d.Id == TheTrailWalker.Domain.Harvest.HarvestTools.ModelId(spec.Family)) ?? definition;
            return definition;
        }

        public static ItemDefinition SlotSilhouette(GearSlot slot)
        {
            foreach (ItemDefinition item in Tier1CraftedItems.NewDefinitions)
                if (item.GearSlot == slot && item.GearRank == 1) return item;
            return null;
        }
    }
}
