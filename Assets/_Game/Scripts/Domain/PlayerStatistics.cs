using System;
using System.Collections.Generic;

namespace TheTrailWalker.Domain
{
    // Player statistics (user, November 2, 2026): whether a statistics page is shown stays pending until phase 1
    // (24 regions) is complete, but the counters are kept in the save from now on so that page can show full history.
    // Current tier, house level, camps reached, missions claimed and current balances are derived from their own state
    // and are not duplicated here; this class keeps only what nothing else remembers.
    public sealed class PlayerStatistics
    {
        public double DistanceMetres { get; private set; }
        public long CashEarned { get; private set; }
        public long CashSpent { get; private set; }
        public long GoldEarned { get; private set; }
        public long GoldSpent { get; private set; }
        public int Faints { get; private set; }

        private readonly Dictionary<string, long> gathered = new Dictionary<string, long>();      // item id -> units picked up
        private readonly Dictionary<string, long> craftedUnits = new Dictionary<string, long>();  // recipe id -> units crafted
        private readonly Dictionary<string, long> crafts = new Dictionary<string, long>();        // recipe id -> craft actions
        private readonly Dictionary<string, long> interactions = new Dictionary<string, long>();  // interactable kind -> completed harvests

        public IReadOnlyDictionary<string, long> Gathered => gathered;
        public IReadOnlyDictionary<string, long> CraftedUnits => craftedUnits;
        public IReadOnlyDictionary<string, long> Crafts => crafts;
        public IReadOnlyDictionary<string, long> Interactions => interactions;

        public long TotalGathered { get { long t = 0; foreach (long v in gathered.Values) t += v; return t; } }
        public long TotalCrafted { get { long t = 0; foreach (long v in craftedUnits.Values) t += v; return t; } }

        // Walked trail distance only (teleports, ferry crossings and map travel are not walking).
        public void AddDistance(double metres) { if (metres > 0 && !double.IsNaN(metres) && !double.IsInfinity(metres)) DistanceMetres += metres; }
        public void AddGathered(string itemId, long units) => Add(gathered, itemId, units);
        public void AddCraft(string recipeId, long units) { if (Add(craftedUnits, recipeId, units)) Add(crafts, recipeId, 1); }
        public void AddInteraction(string kind) => Add(interactions, kind, 1);
        public void AddFaint() => Faints++;
        public void AddCash(long earned) { if (earned > 0) CashEarned = Sum(CashEarned, earned); }
        public void SpendCash(long spent) { if (spent > 0) CashSpent = Sum(CashSpent, spent); }
        public void AddGold(long earned) { if (earned > 0) GoldEarned = Sum(GoldEarned, earned); }
        public void SpendGold(long spent) { if (spent > 0) GoldSpent = Sum(GoldSpent, spent); }

        public long GatheredOf(string itemId) => gathered.TryGetValue(itemId ?? "", out long v) ? v : 0;
        public long CraftedOf(string recipeId) => craftedUnits.TryGetValue(recipeId ?? "", out long v) ? v : 0;
        public long CraftsOf(string recipeId) => crafts.TryGetValue(recipeId ?? "", out long v) ? v : 0;
        public long InteractionsOf(string kind) => interactions.TryGetValue(kind ?? "", out long v) ? v : 0;

        public void Restore(double distance, long cashEarned, long cashSpent, long goldEarned, long goldSpent, int faints,
            IEnumerable<KeyValuePair<string, long>> gatheredPairs, IEnumerable<KeyValuePair<string, long>> craftedPairs,
            IEnumerable<KeyValuePair<string, long>> craftPairs, IEnumerable<KeyValuePair<string, long>> interactionPairs)
        {
            DistanceMetres = distance > 0 && !double.IsNaN(distance) && !double.IsInfinity(distance) ? distance : 0;
            CashEarned = Math.Max(0, cashEarned); CashSpent = Math.Max(0, cashSpent); GoldEarned = Math.Max(0, goldEarned); GoldSpent = Math.Max(0, goldSpent);
            Faints = Math.Max(0, faints);
            Fill(gathered, gatheredPairs); Fill(craftedUnits, craftedPairs); Fill(crafts, craftPairs); Fill(interactions, interactionPairs);
        }

        private static bool Add(Dictionary<string, long> map, string key, long amount)
        {
            if (string.IsNullOrWhiteSpace(key) || amount <= 0) return false;
            map[key] = Sum(map.TryGetValue(key, out long v) ? v : 0, amount);
            return true;
        }
        private static long Sum(long a, long b) => a > long.MaxValue - b ? long.MaxValue : a + b;
        private static void Fill(Dictionary<string, long> map, IEnumerable<KeyValuePair<string, long>> pairs)
        {
            map.Clear();
            if (pairs == null) return;
            foreach (var pair in pairs) if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0) map[pair.Key] = pair.Value;
        }
    }
}
