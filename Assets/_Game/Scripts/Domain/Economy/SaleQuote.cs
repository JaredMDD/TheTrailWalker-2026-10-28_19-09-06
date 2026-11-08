using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Economy
{
    // Ordinary sale quotes (section 12.2.4): sum of condition-adjusted intrinsic unit values, times (1 + eligible sale
    // bonus). The sale bonus is a player-crafted furniture buff that does not exist yet (Milestone 10+), so it is zero.
    // The camp factor (100% NPC purchase, 80% shredder/expiry, 70% Return) is applied once at payout (Money.Payout).
    public static class SaleQuote
    {
        // Used finite gear sells by its remaining condition (proposal; the craft profile itself is never changed).
        // Wearables carry their remaining walking distance outside the profile; tools carry remaining uses in it.
        public static double Condition(ItemEntry entry, PilePortion portion)
        {
            ItemProfile p = portion.Profile;
            if (entry.Definition.Stackable || p.MaxDurability <= 0) return 1.0;
            double remaining = entry.Definition.GearSlot.HasValue ? (entry.HasWearRemaining ? entry.WearRemaining : p.Durability) : p.Durability;
            return Math.Max(0, Math.Min(1, remaining / p.MaxDurability));
        }

        public static long PortionMinor(ItemEntry entry, PilePortion portion, double saleBonus = 0) =>
            (long)Math.Floor(portion.Profile.UnitValue * Condition(entry, portion) * (1 + saleBonus) * portion.Quantity + 1e-6);

        // Full quote of every unit in an entry (a listing's escrow entry holds exactly the listed units).
        public static long TotalMinor(ItemEntry entry, double saleBonus = 0)
        {
            long total = 0;
            foreach (PilePortion portion in entry.Pile) total = checked(total + PortionMinor(entry, portion, saleBonus));
            return total;
        }

        // Quote of the lowest-quality `quantity` units of a stack, before they are split off (listing preview).
        public static long LowestMinor(ItemEntry entry, int quantity, double saleBonus = 0)
        {
            long total = 0; int left = quantity;
            foreach (PilePortion portion in entry.Pile)
            {
                if (left <= 0) break;
                int take = Math.Min(left, portion.Quantity); left -= take;
                total = checked(total + PortionMinor(entry, new PilePortion(portion.Quality, portion.Profile, take), saleBonus));
            }
            return total;
        }
    }
}
