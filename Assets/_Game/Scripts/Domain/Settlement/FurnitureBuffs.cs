using System;
using TheTrailWalker.Domain.Equipment;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Settlement
{
    // Buffs derived from the distinct furniture types placed in a home (spec 2026-11-13). Computed, never stored.
    public readonly struct FurnitureBuffs
    {
        public const double FoodPerStrength = 0.05, SalePerStrength = 0.02, DurabilityPerStrength = 0.05;

        public readonly int CraftQuality;
        public readonly double FoodRecovery;
        public readonly double SalePrice;
        private readonly double[] durability; // indexed by BuffCategory; null means all zero

        public static FurnitureBuffs None { get { return default(FurnitureBuffs); } }

        public FurnitureBuffs(int craftQuality, double foodRecovery, double salePrice, double[] durability)
        {
            CraftQuality = craftQuality; FoodRecovery = foodRecovery; SalePrice = salePrice; this.durability = durability;
        }

        public static double FoodHearts(int baseHearts, FurnitureBuffs b) { return baseHearts * (1 + b.FoodRecovery); }

        public double DurabilityOf(BuffCategory category)
        {
            if (durability == null) return 0;
            int i = (int)category;
            return i >= 0 && i < durability.Length ? durability[i] : 0;
        }

        // The life buff for a crafted output: wearables by slot, tools by family, anything else 0.
        public double DurabilityFor(ItemDefinition output)
        {
            if (output == null) return 0;
            if (output.GearSlot.HasValue)
            {
                switch (output.GearSlot.Value)
                {
                    case GearSlot.Hat: return DurabilityOf(BuffCategory.HatLife);
                    case GearSlot.Boots: return DurabilityOf(BuffCategory.BootsLife);
                    case GearSlot.Shirt: return DurabilityOf(BuffCategory.ShirtLife);
                    case GearSlot.Pants: return DurabilityOf(BuffCategory.PantsLife);
                    case GearSlot.Backpack: return DurabilityOf(BuffCategory.BackpackLife);
                    default: return 0;
                }
            }
            ToolSpec spec;
            if (HarvestTools.TryGet(output, out spec))
            {
                switch (spec.Family)
                {
                    case ToolFamily.Axe: return DurabilityOf(BuffCategory.AxeLife);
                    case ToolFamily.Hunting: return DurabilityOf(BuffCategory.HuntingToolLife);
                    case ToolFamily.Rod: return DurabilityOf(BuffCategory.RodLife);
                }
            }
            return 0;
        }
    }
}
