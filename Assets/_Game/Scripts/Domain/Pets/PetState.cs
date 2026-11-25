using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Economy;

namespace TheTrailWalker.Domain.Pets
{
    // Milestone 15 slice 1: owned pets and the one active pet. Purchases are permanent (no release, no refund) and never
    // switch the active pet; the pet home holds 1/2/3/4 pets at house levels 3/4/6/8 (plan 13.5).
    public sealed class PetState
    {
        public sealed class Purchase { public string PetId; public long UtcTicks; }

        private readonly List<string> owned = new List<string>();
        private readonly List<Purchase> purchases = new List<Purchase>();
        public IReadOnlyList<string> Owned => owned;
        public IReadOnlyList<Purchase> Purchases => purchases;
        public string Active { get; private set; }
        // Slice 2: what the pets earn while the app is closed, and their collection queues.
        public PetIdle Idle = new PetIdle();

        public static int Slots(int houseLevel) => houseLevel >= 8 ? 4 : houseLevel >= 6 ? 3 : houseLevel >= 4 ? 2 : houseLevel >= 3 ? 1 : 0;
        // The first house level that adds a slot above the current count (0 when the pet home is at its last level).
        public static int NextSlotLevel(int houseLevel) => houseLevel < 3 ? 3 : houseLevel < 4 ? 4 : houseLevel < 6 ? 6 : houseLevel < 8 ? 8 : 0;

        public bool Owns(string petId) => owned.Contains(petId);

        public bool CanBuy(PetDefinition pet, int houseLevel, Wallet wallet, out string reason)
        {
            if (pet == null || PetCatalog.Find(pet.Id) == null) { reason = "Unknown pet"; return false; }
            if (houseLevel < 3) { reason = "Pets arrive at house level 3"; return false; }
            if (Owns(pet.Id)) { reason = "Already yours"; return false; }
            if (owned.Count >= Slots(houseLevel))
            {
                int next = NextSlotLevel(houseLevel);
                reason = next > 0 ? "More room at house level " + next : "The pet home is full";
                return false;
            }
            if (wallet == null || !wallet.CanDebit(Currency.Cash, pet.PriceCash)) { reason = "Not enough cash"; return false; }
            reason = null;
            return true;
        }

        // Atomic: every check before the debit; the active pet never changes.
        public bool TryBuy(PetDefinition pet, int houseLevel, Wallet wallet, DateTime utcNow, out string reason)
        {
            if (!CanBuy(pet, houseLevel, wallet, out reason)) return false;
            wallet.Debit(Currency.Cash, pet.PriceCash);
            owned.Add(pet.Id);
            purchases.Add(new Purchase { PetId = pet.Id, UtcTicks = utcNow.Ticks });
            return true;
        }

        public bool SetActive(string petId)
        {
            if (petId == null || !Owns(petId)) return false;
            Active = petId;
            return true;
        }

        // Restore from a save: unknown and duplicate IDs are dropped, the active pet must be owned. A count above the current
        // slots is kept (house levels never go down), but CanBuy refuses more.
        public static PetState Load(IEnumerable<string> ownedIds, IEnumerable<Purchase> purchaseRecords, string activeId, out List<string> dropped)
        {
            var state = new PetState(); dropped = new List<string>();
            if (ownedIds != null)
                foreach (string id in ownedIds)
                {
                    if (PetCatalog.Find(id) == null || state.owned.Contains(id)) { dropped.Add(id); continue; }
                    state.owned.Add(id);
                }
            if (purchaseRecords != null)
                foreach (Purchase p in purchaseRecords) if (p != null && state.owned.Contains(p.PetId)) state.purchases.Add(new Purchase { PetId = p.PetId, UtcTicks = p.UtcTicks });
            if (activeId != null && !state.SetActive(activeId)) dropped.Add(activeId);
            return state;
        }
    }
}
