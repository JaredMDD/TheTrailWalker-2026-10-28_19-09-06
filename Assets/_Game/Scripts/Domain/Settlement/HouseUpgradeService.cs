using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Settlement
{
    // House upgrade deposits (spec 2026-11-14 sections 2 and 6). Deposited units live in the shared item ledger as
    // ItemOwner.Escrow entries made by TryEscrow, like trade listings; the house only keeps their entry ids. Deposits are
    // final: nothing here moves house escrow back to the bag, and only the load-time migration moves it to the overflow.
    // Every mutation changes the ledger and the house list together in memory, so the caller's single save holds both.
    public static class HouseUpgradeService
    {
        public const string AtCapReason = "The house is at its highest level for now.";
        public const string NotNeededReason = "The house doesn't need this.";
        public const string CompleteReason = "Already complete.";
        public const string EmptyBagReason = "None in your bag.";

        // Reserve/Release probe for "is someone holding this entry"; the token is never left behind.
        private const int ProbeToken = int.MinValue + 23;

        private static UpgradeRequirement Requirement(HomeState home, string itemId)
        {
            return HouseUpgrades.For(home.Level + 1).FirstOrDefault(r => r.ItemId == itemId);
        }

        private static Dictionary<string, ItemEntry> ById(SpatialInventory ledger)
        {
            var map = new Dictionary<string, ItemEntry>();
            foreach (ItemEntry e in ledger.Entries) map[e.Id] = e;
            return map;
        }

        // The house's escrow entries in deposit order; ids that name no Escrow entry are skipped (restore refuses those) and a
        // repeated id counts once, so no unit is ever counted or settled twice.
        private static List<ItemEntry> HouseEntries(HomeState home, SpatialInventory ledger)
        {
            Dictionary<string, ItemEntry> map = ById(ledger);
            var list = new List<ItemEntry>(); var seen = new HashSet<string>();
            foreach (string id in home.UpgradeEscrow)
                if (seen.Add(id) && map.TryGetValue(id, out ItemEntry e) && e.Owner == ItemOwner.Escrow) list.Add(e);
            return list;
        }

        public static int Deposited(HomeState home, SpatialInventory ledger, string itemId)
        {
            return HouseEntries(home, ledger).Where(e => e.Definition.Id == itemId).Sum(e => e.Quantity);
        }

        public static int Remaining(HomeState home, SpatialInventory ledger, string itemId)
        {
            UpgradeRequirement r = Requirement(home, itemId);
            return r == null ? 0 : Math.Max(0, r.Quantity - Deposited(home, ledger, itemId));
        }

        private static bool Depositable(SpatialInventory ledger, ItemEntry e, string itemId)
        {
            if (e.Definition.Id != itemId || !e.Ready || e.Quantity < 1) return false;
            if (e.Owner != ItemOwner.Backpack && e.Owner != ItemOwner.Pocket && e.Owner != ItemOwner.Hook) return false;
            if (!ledger.Reserve(e, ProbeToken)) return false;   // already held by a pointer
            ledger.Release(e, ProbeToken);
            return true;
        }

        public static int Available(SpatialInventory ledger, string itemId)
        {
            return ledger.Entries.Where(e => Depositable(ledger, e, itemId)).Sum(e => e.Quantity);
        }

        public static int Deposit(HomeState home, SpatialInventory ledger, string itemId, Func<string> newEscrowId, out string reason)
        {
            if (!home.CanUpgrade) { reason = AtCapReason; return 0; }
            if (Requirement(home, itemId) == null) { reason = NotNeededReason; return 0; }
            int left = Remaining(home, ledger, itemId);
            if (left == 0) { reason = CompleteReason; return 0; }
            if (Available(ledger, itemId) == 0) { reason = EmptyBagReason; return 0; }
            reason = null;
            int moved = 0;
            // Lowest quality first so the player's best units stay in the bag; OrderBy is stable, so ties keep ledger order.
            foreach (ItemEntry source in ledger.Entries.Where(e => Depositable(ledger, e, itemId)).OrderBy(e => e.Quality).ToList())
            {
                if (left == 0) break;
                int take = Math.Min(left, source.Quantity);
                if (!ledger.TryEscrow(source, take, newEscrowId(), out ItemEntry escrow, out _)) continue;
                // Recorded in the same step as the ledger move: no path leaves escrowed units unnamed by the house.
                home.AddUpgradeEscrow(escrow.Id);
                left -= take; moved += take;
            }
            if (moved == 0) reason = EmptyBagReason;
            return moved;
        }

        // Every listed id is an Escrow entry of a required material, and each material is deposited exactly.
        public static bool Ready(HomeState home, SpatialInventory ledger)
        {
            if (!home.CanUpgrade) return false;
            IReadOnlyList<UpgradeRequirement> needs = HouseUpgrades.For(home.Level + 1);
            if (needs.Count == 0) return false;
            List<ItemEntry> entries = HouseEntries(home, ledger);
            if (entries.Count != home.UpgradeEscrow.Count || home.UpgradeEscrow.Distinct().Count() != home.UpgradeEscrow.Count) return false;
            if (entries.Any(e => needs.All(r => r.ItemId != e.Definition.Id))) return false;
            return needs.All(r => entries.Where(e => e.Definition.Id == r.ItemId).Sum(e => e.Quantity) == r.Quantity);
        }

        // All checks run before anything changes; settling a validated Escrow entry and raising a level below the cap cannot fail.
        public static bool Complete(HomeState home, SpatialInventory ledger)
        {
            if (!Ready(home, ledger)) return false;
            foreach (ItemEntry e in HouseEntries(home, ledger)) ledger.SettleEscrow(e);
            home.CompleteUpgrade();
            return true;
        }

        public static void CheckStructure(IReadOnlyList<string> houseEscrowIds, IEnumerable<ItemEntry> entries, IEnumerable<string> tradeEscrowIds)
        {
            if (houseEscrowIds == null || houseEscrowIds.Count == 0) return;
            var map = new Dictionary<string, ItemEntry>();
            if (entries != null) foreach (ItemEntry e in entries) if (e != null) map[e.Id] = e;
            var trade = new HashSet<string>();
            if (tradeEscrowIds != null) foreach (string id in tradeEscrowIds) if (!string.IsNullOrEmpty(id)) trade.Add(id);
            var seen = new HashSet<string>();
            foreach (string id in houseEscrowIds)
            {
                if (string.IsNullOrEmpty(id)) throw new ArgumentException("A house escrow id is empty.");
                if (!seen.Add(id)) throw new ArgumentException("Duplicate house escrow id: " + id);
                if (!map.TryGetValue(id, out ItemEntry e)) throw new ArgumentException("House escrow " + id + " names no item.");
                if (e.Owner != ItemOwner.Escrow) throw new ArgumentException("House escrow " + id + " is not held in escrow.");
                if (trade.Contains(id)) throw new ArgumentException("House escrow " + id + " is also a trade listing.");
            }
        }

        // Load-time migration (spec section 6): deposits the next level can't use go to the overflow as whole entries, never lost.
        public static int MigrateSurplus(HomeState home, SpatialInventory ledger)
        {
            List<ItemEntry> entries = HouseEntries(home, ledger);
            var surplus = new List<ItemEntry>();
            if (!home.CanUpgrade) surplus.AddRange(entries);
            else
            {
                IReadOnlyList<UpgradeRequirement> needs = HouseUpgrades.For(home.Level + 1);
                surplus.AddRange(entries.Where(e => needs.All(r => r.ItemId != e.Definition.Id)));
                foreach (UpgradeRequirement r in needs)
                {
                    List<ItemEntry> mine = entries.Where(e => e.Definition.Id == r.ItemId).ToList();
                    int total = mine.Sum(e => e.Quantity);
                    for (int i = mine.Count - 1; i >= 0 && total > r.Quantity; i--) { surplus.Add(mine[i]); total -= mine[i].Quantity; }
                }
            }
            int moved = 0;
            foreach (ItemEntry e in surplus)
            {
                e.Owner = ItemOwner.World;   // RestoreStored only re-homes World entries
                if (!ledger.RestoreStored(e, ItemOwner.Overflow, 0, 0)) { e.Owner = ItemOwner.Escrow; continue; }   // kept deposited, never dropped
                home.RemoveUpgradeEscrow(e.Id);
                moved += e.Quantity;
            }
            return moved;
        }

        // Debug: the units still missing for the next level, at Q5, into the bag (overflow when full). Never touches deposits.
        public static List<(string itemId, int quantity)> DebugGrant(HomeState home, SpatialInventory ledger)
        {
            var granted = new List<(string itemId, int quantity)>();
            if (!home.CanUpgrade) return granted;
            foreach (UpgradeRequirement r in HouseUpgrades.For(home.Level + 1))
            {
                int missing = Remaining(home, ledger, r.ItemId);
                ItemDefinition definition = TheTrailWalker.Domain.Crafting.Items.Find(r.ItemId);
                if (missing <= 0 || definition == null) continue;
                for (int left = missing; left > 0;)
                {
                    int quantity = Math.Min(1000, left); left -= quantity;
                    ItemEntry item = ledger.AddWorld("debug-deposit-" + Guid.NewGuid().ToString("N"), definition, quantity, 5, TheTrailWalker.Domain.Economy.ItemPrices.Profile(definition, 5));
                    if (!ledger.StoreBought(item, out _)) ledger.RestoreStored(item, ItemOwner.Overflow, 0, 0);
                }
                granted.Add((r.ItemId, missing));
            }
            return granted;
        }
    }
}
