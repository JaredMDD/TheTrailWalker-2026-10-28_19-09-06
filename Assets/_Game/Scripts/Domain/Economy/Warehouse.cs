using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Economy
{
    // Milestone 12 settlement warehouse: two ten-unit crates, one exact item ID each. Deposits are final; the units sit in
    // bag-ledger escrow until the tenth unit completes the crate and pays its locked value in cash (gold every tenth crate).
    [Serializable] public sealed class WarehouseUnit { public string EscrowId; public int Quantity; public long ValueMinor; }

    public sealed class WarehouseCrate
    {
        public string ItemId;
        public readonly List<WarehouseUnit> Units = new List<WarehouseUnit>();
        public int Count => Units.Sum(u => u.Quantity);
        public bool IsEmpty => Units.Count == 0;
        public long ValueMinor => Units.Sum(u => u.ValueMinor);
    }

    public sealed class WarehouseState
    {
        public const int CrateUnits = 10, CycleLength = 10, CashPerGold = 1000;
        public readonly WarehouseCrate[] Crates = { new WarehouseCrate(), new WarehouseCrate() };
        public int CycleCrates;
        public long CycleCash;
    }

    public struct WarehouseDeposit { public int Units; public long ValueMinor, CashPaid, GoldPaid; public bool CrateCompleted; }

    public static class WarehouseService
    {
        public const string CampId = "warehouse";

        public static bool Deposit(SpatialInventory ledger, WarehouseState state, Wallet wallet, List<TradeRecord> log, ItemEntry item, int pointer, int crateIndex,
            Func<string> newEscrowId, DateTime utcNow, out WarehouseDeposit result, out string reason)
        {
            result = default; reason = "Item is unavailable";
            if (ledger == null || state == null || wallet == null || log == null || item == null || crateIndex < 0 || crateIndex >= state.Crates.Length) return false;
            if (!item.Ready) { reason = "Not ready yet"; return false; }
            if (item.Owner != ItemOwner.Backpack && item.Owner != ItemOwner.Pocket && item.Owner != ItemOwner.Hook) return false;
            WarehouseCrate crate = state.Crates[crateIndex];
            string itemId = item.Definition.Id;
            if (!crate.IsEmpty && crate.ItemId != itemId) { reason = "This crate takes " + (ledger.Entries.FirstOrDefault(e => e.Definition.Id == crate.ItemId)?.Definition.Name ?? crate.ItemId); return false; }

            bool stack = item.Definition.Stackable;
            int units = stack ? Math.Min(item.Quantity, WarehouseState.CrateUnits - crate.Count) : 1;
            if (units < 1) return false;
            long value, cash = 0, gold = 0, cycleCash = state.CycleCash; bool completes = crate.Count + units == WarehouseState.CrateUnits, cycleEnds = false;
            try
            {
                value = stack ? SaleQuote.LowestMinor(item, units) : SaleQuote.TotalMinor(item);
                if (completes)
                {
                    cash = Money.Payout(checked(crate.ValueMinor + value), 100);
                    cycleCash = checked(state.CycleCash + cash);
                    cycleEnds = state.CycleCrates + 1 >= WarehouseState.CycleLength;
                    if (cycleEnds) gold = cycleCash / WarehouseState.CashPerGold;
                    _ = checked(wallet.Cash + cash); _ = checked(wallet.Gold + gold);   // pre-check: Credit would throw after the units were escrowed
                }
            }
            catch (OverflowException) { reason = "Too much to store"; return false; }

            string escrowId = item.Id;
            if (stack) { escrowId = newEscrowId?.Invoke(); if (string.IsNullOrWhiteSpace(escrowId)) return false; }
            bool held = ledger.ReservedBy(item, pointer);
            if (held) ledger.Release(item, pointer);
            if (!ledger.TryEscrow(item, units, escrowId, out ItemEntry escrow, out reason))
            {
                if (held) ledger.Reserve(item, pointer);
                return false;
            }
            if (held && item.Owner != ItemOwner.Consumed && item.Owner != ItemOwner.Escrow && item.Quantity > 0) ledger.Reserve(item, pointer);

            crate.ItemId = itemId;
            crate.Units.Add(new WarehouseUnit { EscrowId = escrow.Id, Quantity = units, ValueMinor = value });
            string utc = utcNow.ToString("O", CultureInfo.InvariantCulture);
            int quality = escrow.Quality;
            TradeLog.Add(log, new TradeRecord { utc = utc, campId = CampId, settlementId = "crate" + (crateIndex + 1), itemId = itemId, result = "WarehouseDeposit", quantity = units, quality = quality, unitMinor = value / units, totalMinor = value });
            if (completes)
            {
                long crateValue = crate.ValueMinor, crateUnits = crate.Count;
                foreach (WarehouseUnit unit in crate.Units) ledger.SettleEscrow(ledger.Entries.First(e => e.Id == unit.EscrowId));
                crate.Units.Clear(); crate.ItemId = null;
                wallet.Credit(Currency.Cash, cash);
                state.CycleCrates++; state.CycleCash = cycleCash;
                TradeLog.Add(log, new TradeRecord { utc = utc, campId = CampId, settlementId = "crate" + (crateIndex + 1), itemId = itemId, result = "WarehouseCrate", quantity = (int)crateUnits, quality = quality, unitMinor = crateValue / crateUnits, totalMinor = crateValue, cash = cash, percent = 100 });
                if (cycleEnds)
                {
                    wallet.Credit(Currency.Gold, gold);
                    state.CycleCrates = 0; state.CycleCash = 0;
                    TradeLog.Add(log, new TradeRecord { utc = utc, campId = CampId, settlementId = "cycle", itemId = itemId, result = "WarehouseGold", quantity = (int)gold, totalMinor = cycleCash });
                }
            }
            result = new WarehouseDeposit { Units = units, ValueMinor = value, CashPaid = cash, GoldPaid = gold, CrateCompleted = completes };
            reason = completes ? "Crate sent" : "Stored";
            return true;
        }
    }
}
