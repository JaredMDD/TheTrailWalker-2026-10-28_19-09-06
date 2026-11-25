using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Pets
{
    // One reward waiting in a pet's collection queue: generated once, kept until claimed (no expiry, no merging).
    public sealed class PetFind
    {
        public string EntryId, ItemId;
        public int Quantity, Quality;
        public ulong Seed;
    }

    // What the pet earns while the app is closed (spec 2026-11-24 section 4; plan 13.5). Pure and deterministic: the caller
    // passes the clock, the discovered regions and a save seed, so the checks can drive it with an injected clock.
    //
    // * BeginOffline (app paused, unfocused or quitting): remember when the absence started and snapshot the active pet and
    //   the discovered regions. With no active pet nothing accrues (no catch-up later). A second Begin keeps the first start.
    // * Settle (app resumed or loaded): elapsed = max(0, now - start), capped once at 8 h; whole 30-minute steps become rolls
    //   for the snapshot pet until its queue holds 16; the remainder is carried. The carry keeps the pet and regions it was
    //   earned with, so the roll that completes it uses them even after a switch. A backward clock gives nothing, and the
    //   last-seen watermark never moves back. Each absence is settled once (repeated resumes cannot replay it).
    public sealed class PetIdle
    {
        public const int RollMinutes = 30, CapHours = 8, QueueSize = 16;
        public static readonly TimeSpan Roll = TimeSpan.FromMinutes(RollMinutes), Cap = TimeSpan.FromHours(CapHours);
        // Development builds give 1,000 units per roll (the confirmed 1,000-unit testing rule); the friends-test / release
        // build switches this to 1 (spec decision 2).
        public const int DevelopmentUnits = 1000, ReleaseUnits = 1;
        public static int UnitsPerRoll = DevelopmentUnits;

        private readonly Dictionary<string, List<PetFind>> queues = new Dictionary<string, List<PetFind>>();
        public long OfflineStartedTicks;      // 0 = not away
        public long LastSeenTicks;            // monotonic watermark
        public long CarriedTicks;             // < one roll
        public string CarryPetId;             // the pet the carried time was earned with
        public List<int> CarryRegions = new List<int>();
        public string SnapshotPetId;          // the pet of the current absence
        public List<int> SnapshotRegions = new List<int>();
        public long IntervalId;               // absences settled so far
        public long NextEntry;                // entry ids p<NextEntry>

        public IReadOnlyList<PetFind> Queue(string petId) => petId != null && queues.TryGetValue(petId, out List<PetFind> q) ? q : (IReadOnlyList<PetFind>)Array.Empty<PetFind>();
        public IEnumerable<string> QueuedPets => queues.Keys;
        public int Waiting(string petId) => Queue(petId).Count;
        private List<PetFind> QueueOf(string petId) { if (!queues.TryGetValue(petId, out List<PetFind> q)) queues[petId] = q = new List<PetFind>(); return q; }
        // Save restore only: a find goes back in its pet's queue as saved (no more than QueueSize per pet).
        public void RestoreFind(string petId, PetFind f) { List<PetFind> q = QueueOf(petId); if (q.Count < QueueSize && q.All(x => x.EntryId != f.EntryId)) q.Add(f); }

        public void BeginOffline(string activePetId, ICollection<int> regions, DateTime utcNow)
        {
            long now = Math.Max(utcNow.Ticks, LastSeenTicks);
            if (OfflineStartedTicks != 0) return;              // already away (pause and focus loss both fire)
            if (activePetId == null) return;                   // no active pet: nothing accrues, nothing caught up later
            OfflineStartedTicks = now;
            SnapshotPetId = activePetId;
            SnapshotRegions = regions?.OrderBy(r => r).ToList() ?? new List<int>();
        }

        // Returns the number of new finds. saveSeed mixes into every roll (a different save never repeats another's luck).
        public int Settle(DateTime utcNow, ulong saveSeed)
        {
            long now = utcNow.Ticks;
            if (OfflineStartedTicks == 0) { LastSeenTicks = Math.Max(LastSeenTicks, now); return 0; }
            long start = OfflineStartedTicks;
            OfflineStartedTicks = 0;
            IntervalId++;
            if (now < LastSeenTicks || now <= start) return 0;   // a backward clock earns nothing; the watermark stays
            LastSeenTicks = now;
            long elapsed = Math.Min(now - start, Cap.Ticks);
            long total = CarriedTicks + elapsed;
            int rolls = (int)Math.Min(total / Roll.Ticks, CapHours * 60 / RollMinutes);
            long remainder = total - rolls * Roll.Ticks;
            int made = 0;
            for (int i = 0; i < rolls; i++)
            {
                bool completesCarry = i == 0 && CarriedTicks > 0 && CarryPetId != null;
                string pet = completesCarry ? CarryPetId : SnapshotPetId;
                ICollection<int> regions = completesCarry ? (ICollection<int>)CarryRegions : SnapshotRegions;
                List<PetFind> q = QueueOf(pet);
                if (q.Count >= QueueSize) { if (pet == SnapshotPetId) { remainder = 0; break; } continue; }   // full: stop, bank nothing
                PetFind f = RollFind(PetCatalog.Find(pet), regions, Mix(saveSeed, (ulong)IntervalId, (ulong)i));
                if (f == null) continue;                                                                       // nothing eligible: no reroll banked
                f.EntryId = "p" + (++NextEntry);
                q.Add(f); made++;
            }
            if (QueueOf(SnapshotPetId).Count >= QueueSize || now - start >= Cap.Ticks) remainder = 0;   // time beyond the cap or a full queue is discarded
            CarriedTicks = remainder;
            CarryPetId = remainder > 0 ? SnapshotPetId : null;
            CarryRegions = remainder > 0 ? new List<int>(SnapshotRegions) : new List<int>();
            return made;
        }

        // One roll: a supply group by the effective weights, then 70% the highest discovered tier / 30% earlier tiers (either
        // pool alone when the other is empty), then an equal chance per item; quality Q1-Q5 uniform like loose trail drops.
        public static PetFind RollFind(PetDefinition pet, ICollection<int> regions, ulong seed)
        {
            if (pet == null) return null;
            var rng = new Rng(seed);
            Dictionary<SupplyGroup, double> w = PetSupplies.Effective(pet, regions);
            double pick = rng.NextDouble(), acc = 0; SupplyGroup? group = null;
            foreach (SupplyGroup g in Enum.GetValues(typeof(SupplyGroup)))
            {
                if (w[g] <= 0) continue;
                acc += w[g];
                if (pick < acc) { group = g; break; }
            }
            if (group == null) group = w.Where(kv => kv.Value > 0).Select(kv => (SupplyGroup?)kv.Key).LastOrDefault();
            if (group == null) return null;
            List<string> ids = PetSupplies.Eligible(group.Value, regions);
            int top = ids.Max(id => PetSupplies.TierOf(PetSupplies.RegionOf(id)));
            List<string> high = ids.Where(id => PetSupplies.TierOf(PetSupplies.RegionOf(id)) == top).ToList();
            List<string> low = ids.Where(id => PetSupplies.TierOf(PetSupplies.RegionOf(id)) < top).ToList();
            List<string> pool = low.Count == 0 || rng.NextDouble() < 0.7 ? high : low;
            string item = pool[(int)(rng.Next() % (ulong)pool.Count)];
            return new PetFind { ItemId = item, Quantity = UnitsPerRoll, Quality = 1 + (int)(rng.Next() % 5), Seed = seed };
        }

        // Claims move a whole find into the bag when it fits completely (each find is one stack); otherwise it stays queued.
        public bool Claim(string petId, string entryId, SpatialInventory bag)
        {
            if (!queues.TryGetValue(petId ?? "", out List<PetFind> q)) return false;
            PetFind f = q.FirstOrDefault(x => x.EntryId == entryId);
            ItemDefinition def = f != null ? Crafting.Items.Find(f.ItemId) : null;
            if (def == null) return false;
            ItemProfile profile = ItemPrices.Profile(def, f.Quality);
            if (!Fits(bag, def, f.Quality, profile, f.Quantity)) return false;
            if (!Store(bag, f.EntryId, def, f.Quality, profile, f.Quantity)) throw new InvalidOperationException("Pet find passed the fit check but did not store");
            q.Remove(f);
            return true;
        }
        // In queue order, as far as the bag allows; finds that do not fit stay, untouched. Returns how many were claimed.
        public int ClaimAll(string petId, SpatialInventory bag)
        {
            int n = 0;
            foreach (PetFind f in Queue(petId).ToList()) if (Claim(petId, f.EntryId, bag)) n++;
            return n;
        }

        // ShopService.StoreFits/Store with the find's own quality (store goods are always Q5).
        private const int Token = int.MinValue + 15, Probe = int.MinValue + 16;
        internal static bool Store(SpatialInventory bag, string id, ItemDefinition def, int quality, ItemProfile profile, int units)
        {
            ItemEntry item = bag.AddWorld(id, def, units, quality, profile);
            bool ok = bag.Reserve(item, Token) && bag.TryQuickStore(item, Token, out _);
            bag.Release(item, Token);
            return ok && (item.Owner == ItemOwner.Backpack || item.Owner == ItemOwner.Consumed);
        }
        public static bool Fits(SpatialInventory bag, ItemDefinition def, int quality, ItemProfile profile, int units)
        {
            var scratch = new SpatialInventory(bag.Width, bag.Height);
            int n = 0;
            foreach (ItemEntry e in bag.Entries)
            {
                if (e.Owner != ItemOwner.Backpack) continue;
                ItemEntry copy = scratch.AddWorld("s" + n++, e.Definition, e.Quantity, e.Quality, e.Profile, e.Ready);
                if (!scratch.RestoreStored(copy, ItemOwner.Backpack, e.X, e.Y)) return false;
                if (bag.Reserve(e, Probe)) bag.Release(e, Probe); else scratch.Reserve(copy, Probe);
            }
            return Store(scratch, "p", def, quality, profile, units);
        }

        private static ulong Mix(ulong a, ulong b, ulong c)
        {
            ulong z = a ^ (b * 0x9E3779B97F4A7C15UL) ^ (c * 0xC2B2AE3D27D4EB4FUL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL; z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
        private struct Rng
        {
            private ulong state;
            public Rng(ulong seed) { state = seed ^ 0x5065745265776172UL; }   // "PetRewar"
            public ulong Next() { state += 0x9E3779B97F4A7C15UL; ulong z = state; z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL; z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL; return z ^ (z >> 31); }
            public double NextDouble() => (Next() >> 11) * (1.0 / (1UL << 53));
        }
    }
}
