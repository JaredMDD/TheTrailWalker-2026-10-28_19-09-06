using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Travel;

namespace TheTrailWalker.Domain.Settlement
{
    // Milestone 13 lumber field: four wood tiers bought with gold, six massive trees each. A felled tree holds its whole
    // manifest (x50, stored as units) until it regrows 72 h later, which also discards the leftovers.
    public sealed class PendingReward { public string ItemId; public int Quality; public long Units; }

    public sealed class MassiveTree
    {
        public DateTime? FellUtc;
        public int FellCount;
        public readonly List<PendingReward> Pending = new List<PendingReward>();
        public bool Standing => FellUtc == null;
    }

    public sealed class LumberFieldState
    {
        public const int Tiers = 4, TreesPerTier = 6;
        public int UnlockedTier, LastZone, DebugWoodTier;
        public TimeSpan ClockOffset;
        public readonly MassiveTree[,] Trees = new MassiveTree[Tiers, TreesPerTier];
        public LumberFieldState()
        {
            for (int t = 0; t < Tiers; t++) for (int i = 0; i < TreesPerTier; i++) Trees[t, i] = new MassiveTree();
        }
        // tier is 1-based, index 0-5.
        public MassiveTree Tree(int tier, int index) { return Trees[tier - 1, index]; }
    }

    public static class LumberFieldService
    {
        public static readonly TimeSpan Regrow = TimeSpan.FromHours(72);
        private const int Multiplier = 50, BundleUnits = 1000;
        private static readonly long[] Prices = { 50, 300, 750, 1500 };
        private static readonly string[] Woods = { "pine", "birch", "mesquite", "oak" };

        public static long UnlockPrice(int tier) { return Prices[tier - 1]; }
        public static int NextTier(LumberFieldState s) { return s.UnlockedTier >= LumberFieldState.Tiers ? 0 : s.UnlockedTier + 1; }
        public static string WoodName(int tier) { return Woods[tier - 1]; }
        public static bool WoodDiscovered(int tier, ICollection<int> discoveredTiers, int debugWoodTier)
        { return tier <= debugWoodTier || (discoveredTiers != null && discoveredTiers.Contains(tier)); }

        // Everything is checked before anything changes.
        public static bool Unlock(LumberFieldState s, Wallet wallet, int tier, ICollection<int> discoveredTiers, out string reason)
        {
            reason = null;
            if (tier < 1 || tier > LumberFieldState.Tiers || tier != s.UnlockedTier + 1) { reason = "Unlock the tiers in order"; return false; }
            if (!WoodDiscovered(tier, discoveredTiers, s.DebugWoodTier)) { reason = "Reach a " + WoodName(tier) + " region first"; return false; }
            if (!wallet.CanDebit(Currency.Gold, UnlockPrice(tier))) { reason = "Not enough gold"; return false; }
            wallet.Debit(Currency.Gold, UnlockPrice(tier));
            s.UnlockedTier = tier;
            if (tier == 1) s.LastZone = 1;
            return true;
        }

        public static DateTime? ExpiresAt(MassiveTree t) { return t.FellUtc + Regrow; }

        public static TimeSpan Remaining(MassiveTree t, DateTime utcNow)
        {
            if (t.FellUtc == null) return TimeSpan.Zero;
            TimeSpan left = Regrow - (utcNow - t.FellUtc.Value);   // a clock before the fell counts as zero elapsed
            return left < TimeSpan.Zero ? TimeSpan.Zero : left > Regrow ? Regrow : left;
        }

        public static List<(int tier, int index)> Refresh(LumberFieldState s, DateTime utcNow)
        {
            var changed = new List<(int tier, int index)>();
            for (int tier = 1; tier <= LumberFieldState.Tiers; tier++)
                for (int i = 0; i < LumberFieldState.TreesPerTier; i++)
                {
                    MassiveTree t = s.Tree(tier, i);
                    if (t.FellUtc == null || Remaining(t, utcNow) > TimeSpan.Zero) continue;
                    t.FellUtc = null; t.Pending.Clear(); changed.Add((tier, i));
                }
            return changed;
        }

        // Massive table (target tier minus tool tier): +2 80/80, +1 40/40, 0 20/10, -1 10/5, -2 5/3, -3 or less 3/2; null above +2.
        public static int? MassiveActions(ToolSpec tool, int targetTier)
        {
            int d = targetTier - tool.Tier;
            if (d > 2) return null;
            if (d == 2) return 80;
            if (d == 1) return 40;
            if (d == 0) return tool.Elite ? 10 : 20;
            if (d == -1) return tool.Elite ? 5 : 10;
            if (d == -2) return tool.Elite ? 3 : 5;
            return tool.Elite ? 2 : 3;
        }

        public static bool Fell(LumberFieldState s, int tier, int index, bool boosted, DateTime utcNow, out string reason)
        {
            reason = null;
            if (tier < 1 || tier > LumberFieldState.Tiers || index < 0 || index >= LumberFieldState.TreesPerTier) { reason = "No such tree"; return false; }
            if (tier > s.UnlockedTier) { reason = "Unlock a tier at the ledger first"; return false; }
            MassiveTree t = s.Tree(tier, index);
            if (!t.Standing) { reason = "Already felled"; return false; }
            var rolled = HarvestManifest.Roll(InteractableKind.Tree, (tier - 1) * 6 + 1, "lumber.t" + tier, t.FellCount + 1, "tree" + index, boosted);
            t.FellCount++; t.FellUtc = utcNow; t.Pending.Clear();
            foreach (HarvestBundle b in rolled)
            {
                PendingReward row = t.Pending.Find(p => p.ItemId == b.ItemId && p.Quality == b.Quality);
                if (row == null) t.Pending.Add(row = new PendingReward { ItemId = b.ItemId, Quality = b.Quality });
                row.Units += (long)Multiplier * BundleUnits;
            }
            return true;
        }

        public static bool Top(MassiveTree t, string itemId, out int quality, out int units)
        {
            quality = 0; units = 0;
            PendingReward best = null;
            foreach (PendingReward p in t.Pending) if (p.ItemId == itemId && (best == null || p.Quality > best.Quality)) best = p;
            if (best == null) return false;
            quality = best.Quality; units = (int)Math.Min(BundleUnits, best.Units);
            return true;
        }

        public static void Take(MassiveTree t, string itemId, int quality, long units)
        {
            PendingReward row = t.Pending.Find(p => p.ItemId == itemId && p.Quality == quality);
            if (row == null || units < 0 || units > row.Units) throw new ArgumentException("Cannot take " + units + " of " + itemId + " Q" + quality);
            row.Units -= units;
            if (row.Units == 0) t.Pending.Remove(row);
        }

        // shortcut: a row removed at zero comes back at the end of Pending, so PileItems order can shift; fine until the pile order is shown.
        public static void Untake(MassiveTree t, string itemId, int quality, long units)
        {
            PendingReward row = t.Pending.Find(p => p.ItemId == itemId && p.Quality == quality);
            if (row == null) t.Pending.Add(row = new PendingReward { ItemId = itemId, Quality = quality });
            row.Units += units;
        }

        public static long Bundles(MassiveTree t, string itemId)
        {
            long units = t.Pending.Where(p => p.ItemId == itemId).Sum(p => p.Units);
            return (units + BundleUnits - 1) / BundleUnits;
        }

        public static IEnumerable<string> PileItems(MassiveTree t) { return t.Pending.Select(p => p.ItemId).Distinct(); }
    }
}
