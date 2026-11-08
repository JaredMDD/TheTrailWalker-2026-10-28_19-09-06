using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain;
using TheTrailWalker.Persistence;

namespace TheTrailWalker
{
    // Statistics tracking (user, November 2, 2026): counters only, saved with the session. No page shows them yet: the
    // statistics page stays a pending decision until phase 1 (24 regions) is complete.
    public sealed partial class CollectionPrototype
    {
        private PlayerStatistics statistics = new PlayerStatistics();
        public PlayerStatistics Statistics => statistics;

        private static List<SavedCount> Counts(IReadOnlyDictionary<string, long> map) => map.Select(p => new SavedCount { key = p.Key, value = p.Value }).ToList();
        private static IEnumerable<KeyValuePair<string, long>> Pairs(List<SavedCount> list) =>
            list == null ? Enumerable.Empty<KeyValuePair<string, long>>() : list.Where(c => c != null).Select(c => new KeyValuePair<string, long>(c.key, c.value));

        private SavedStatistics SaveStatistics() => new SavedStatistics
        {
            distanceMetres = statistics.DistanceMetres, cashEarned = statistics.CashEarned, cashSpent = statistics.CashSpent,
            goldEarned = statistics.GoldEarned, goldSpent = statistics.GoldSpent, faints = statistics.Faints,
            gathered = Counts(statistics.Gathered), craftedUnits = Counts(statistics.CraftedUnits), crafts = Counts(statistics.Crafts), interactions = Counts(statistics.Interactions)
        };

        // Older saves have no statistics section and start from zero.
        private void RestoreStatistics(SavedStatistics saved)
        {
            statistics = new PlayerStatistics();
            if (saved == null) return;
            statistics.Restore(saved.distanceMetres, saved.cashEarned, saved.cashSpent, saved.goldEarned, saved.goldSpent, saved.faints,
                Pairs(saved.gathered), Pairs(saved.craftedUnits), Pairs(saved.crafts), Pairs(saved.interactions));
        }
    }
}
