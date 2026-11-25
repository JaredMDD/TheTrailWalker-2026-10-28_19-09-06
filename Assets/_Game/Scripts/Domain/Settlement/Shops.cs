using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Settlement
{
    // One of a shop's three slots for the current visit. An empty slot (short pool) has no item and no units.
    public sealed class ShopOffer
    {
        public string ItemId;
        public long UnitPriceMinor;
        public int UnitsLeft;
        public int Revision;
        // Gold offers hold whole gold per serving (per recipe for a recipe offer) in UnitPriceMinor; RecipeId is null for a food.
        public Currency Currency;
        public string RecipeId;
        public bool Empty => ItemId == null;
    }

    // Shop stock lives for one settlement visit: it is rolled once per shop per visit and kept until the next visit.
    public sealed class ShopState
    {
        public int VisitId;
        public int DebugTier;
        public Dictionary<string, (int Visit, ShopOffer[] Offers)> Stock = new Dictionary<string, (int Visit, ShopOffer[] Offers)>();
    }

    public static class ShopService
    {
        public const int StartUnits = 1000, Q = 5, Probe = int.MinValue + 15;

        public static int ReachedTier(ShopState s, ICollection<int> discoveredRegions)
        {
            int tier = Math.Max(1, s.DebugTier);
            foreach (int r in discoveredRegions) tier = Math.Max(tier, (r - 1) / 6 + 1);
            return tier;
        }

        // Debug tier unlocks every region of its tiers; a normal game (DebugTier 0) returns the discovered set untouched.
        public static ICollection<int> EffectiveRegions(ShopState s, ICollection<int> discoveredRegions)
        {
            if (s.DebugTier <= 0) return discoveredRegions;
            var set = new HashSet<int>(discoveredRegions);
            for (int r = 1; r <= 6 * s.DebugTier; r++) set.Add(r);
            return set;
        }

        public static void AdvanceVisit(ShopState s) => s.VisitId++;

        public static ShopOffer[] Offers(ShopState s, string shopId, ICollection<int> discoveredRegions, CraftingState crafting, ISet<string> boxReserved = null)
        {
            // Stock rolls lazily on a shop's first open per visit: one first opened after a mid-visit discovery or TIER +1 rolls with the newer regions; already-opened shops keep theirs.
            if (s.Stock.TryGetValue(shopId, out var held) && held.Visit == s.VisitId) return held.Offers;
            ShopOffer[] rolled = Roll(s, shopId, discoveredRegions, crafting, boxReserved);
            s.Stock[shopId] = (s.VisitId, rolled);
            return rolled;
        }

        private static ShopOffer[] Roll(ShopState s, string shopId, ICollection<int> discoveredRegions, CraftingState crafting, ISet<string> boxReserved)
        {
            var eligible = ShopCatalog.Eligible(shopId, EffectiveRegions(s, discoveredRegions), ReachedTier(s, discoveredRegions)).ToList();
            var random = new Random(Seed(s.VisitId, shopId));
            List<ShopEntry> Pool(ShopSlotRole role) => eligible.Where(e => e.Role == role).ToList();
            // Draws without replacement so two slots of one role never repeat a good.
            ShopEntry Take(List<ShopEntry> pool)
            {
                if (pool.Count == 0) return null;
                int i = random.Next(pool.Count); ShopEntry e = pool[i]; pool.RemoveAt(i); return e;
            }
            var picks = new ShopEntry[3];
            if (shopId == ShopCatalog.Tailor)
            {
                List<ShopEntry> raw = Pool(ShopSlotRole.RawFibre), processed = Pool(ShopSlotRole.Processed), fine = Pool(ShopSlotRole.ProcessedFine);
                picks[0] = Take(raw); picks[1] = Take(raw);
                bool wantFine = random.Next(100) < 20;
                picks[2] = Take(wantFine && fine.Count > 0 ? fine : processed.Count > 0 ? processed : fine);
            }
            else if (shopId == ShopCatalog.Fishery)
            {
                List<ShopEntry> fish = Pool(ShopSlotRole.Fish);
                picks[0] = Take(fish); picks[1] = Take(fish); picks[2] = Take(Pool(ShopSlotRole.Exclusive));
            }
            else if (shopId == ShopCatalog.Bakery)
            {
                List<ShopEntry> common = Pool(ShopSlotRole.Common);
                picks[0] = Take(common); picks[1] = Take(common); picks[2] = Take(Pool(ShopSlotRole.Premium));
            }
            else
            {
                List<ShopEntry> any = Pool(ShopSlotRole.Any);
                for (int i = 0; i < 3; i++) picks[i] = Take(any);
            }
            ShopOffer[] offers = picks.Select(e => e == null ? new ShopOffer() : e.GoldPerServing.HasValue
                ? new ShopOffer { ItemId = e.ItemId, UnitPriceMinor = e.GoldPerServing.Value, UnitsLeft = StartUnits, Currency = Currency.Gold }
                : new ShopOffer { ItemId = e.ItemId, UnitPriceMinor = ShopCatalog.UnitPriceMinor(e), UnitsLeft = StartUnits }).ToArray();
            if (shopId == ShopCatalog.Bakery && picks[2] != null)
            {
                // The draw happens even when the gate fails, so seeds stay stable. One premium slot per stock, so a recipe cannot repeat inside it.
                bool hit = random.Next(100) < 5;
                string recipeId = ShopCatalog.RecipeFor(picks[2].ItemId);
                if (hit && BakeryGate.Eligible(recipeId, EffectiveRegions(s, discoveredRegions), ReachedTier(s, discoveredRegions), crafting, out _, boxReserved))
                { offers[2].RecipeId = recipeId; offers[2].UnitsLeft = 1; offers[2].UnitPriceMinor = picks[2].GoldPerServing.Value * 10; }
            }
            return offers;
        }

        // FNV-1a, as HarvestManifest.Seed.
        private static int Seed(int visitId, string shopId)
        {
            uint hash = 2166136261;
            foreach (char c in visitId + "|" + shopId) { hash ^= c; hash *= 16777619; }
            return (int)(hash & 0x7fffffff);
        }

        // Dry run on a private copy of the bag: the purchase fits only when it ends fully merged or placed, never half on the trail.
        public static bool StoreFits(SpatialInventory bag, ItemDefinition def, ItemProfile profile, int units)
        {
            var scratch = new SpatialInventory(bag.Width, bag.Height);
            int n = 0;
            foreach (ItemEntry e in bag.Entries)
            {
                if (e.Owner != ItemOwner.Backpack) continue;
                ItemEntry copy = scratch.AddWorld("s" + n++, e.Definition, e.Quantity, e.Quality, e.Profile, e.Ready);
                if (!scratch.RestoreStored(copy, ItemOwner.Backpack, e.X, e.Y)) return false;
                // No public "is reserved" query: a failed probe reservation means held by someone; mirror it so merges skip the same stacks.
                if (bag.Reserve(e, Probe)) bag.Release(e, Probe); else scratch.Reserve(copy, Probe);
            }
            return Store(scratch, "p", def, profile, units);
        }

        // Shared with the balloon shop (gear purchases and box claims).
        internal static bool Store(SpatialInventory bag, string id, ItemDefinition def, ItemProfile profile, int units)
        {
            ItemEntry item = bag.AddWorld(id, def, units, Q, profile);
            const int token = int.MinValue + 14;
            bool ok = bag.Reserve(item, token) && bag.TryQuickStore(item, token, out _);
            bag.Release(item, token);
            return ok && (item.Owner == ItemOwner.Backpack || item.Owner == ItemOwner.Consumed);
        }

        // Whole gold for a purchase. The 10% stack discount applies only to exactly 1,000 units in one purchase; a recipe never gets one.
        public static long GoldTotal(ShopOffer o, int units) =>
            o.RecipeId != null ? o.UnitPriceMinor : units == StartUnits ? o.UnitPriceMinor * StartUnits * 90 / 100 : checked(o.UnitPriceMinor * units);

        public static bool TryBuyRecipe(ShopState s, int slot, int revision, Wallet wallet, CraftingState crafting, ICollection<int> regions, out string reason, ISet<string> boxReserved = null)
        {
            reason = "Sold out";
            if (!s.Stock.TryGetValue(ShopCatalog.Bakery, out var held) || held.Visit != s.VisitId || slot < 0 || slot >= held.Offers.Length) return false;
            ShopOffer offer = held.Offers[slot];
            if (offer.Revision != revision) { reason = "The stock changed; check the offer again"; return false; }
            if (offer.Empty || offer.RecipeId == null || offer.UnitsLeft < 1) return false;
            if (crafting.Knows(offer.RecipeId)) { reason = "You already know this recipe"; return false; }
            if (!BakeryGate.Eligible(offer.RecipeId, regions, ReachedTier(s, regions), crafting, out _, boxReserved)) { reason = "This recipe isn't available yet"; return false; }
            long cost = GoldTotal(offer, 1);
            if (!wallet.CanDebit(Currency.Gold, cost)) { reason = "Not enough gold"; return false; }
            crafting.Learn(offer.RecipeId); wallet.Debit(Currency.Gold, cost);
            offer.UnitsLeft = 0; offer.Revision++;
            reason = ""; return true;
        }

        public static bool TryBuy(ShopState s, string shopId, int slot, int revision, int units, Wallet wallet, SpatialInventory bag, Func<string> newId, out string reason)
        {
            reason = "Sold out";
            if (!s.Stock.TryGetValue(shopId, out var held) || held.Visit != s.VisitId || slot < 0 || slot >= held.Offers.Length) return false;
            ShopOffer offer = held.Offers[slot];
            if (offer.Revision != revision) { reason = "The stock changed; check the offer again"; return false; }
            if (offer.Empty || offer.RecipeId != null || units < 1 || units > offer.UnitsLeft) return false;
            bool gold = offer.Currency == Currency.Gold;
            long cost = gold ? GoldTotal(offer, units) : Money.Charge(checked(units * offer.UnitPriceMinor));
            if (!wallet.CanDebit(offer.Currency, cost)) { reason = gold ? "Not enough gold" : "Not enough cash"; return false; }
            ItemDefinition def = Crafting.Items.Find(offer.ItemId);
            ItemProfile profile = ItemPrices.Profile(def, Q);
            if (!StoreFits(bag, def, profile, units)) { reason = "Not enough room in the bag"; return false; }
            // Deliver first: the dry run matched the real store, so a failure here is a bug and must never charge the player.
            if (!Store(bag, newId(), def, profile, units)) throw new InvalidOperationException("Shop purchase passed the fit check but did not store");
            wallet.Debit(offer.Currency, cost);
            offer.UnitsLeft -= units; offer.Revision++;
            reason = ""; return true;
        }
    }
}
