using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Economy
{
    // NotStarted: the visit's offer window is open (Missed when it runs out untouched; appended to keep saved values).
    public enum TradeState { NotStarted, Active, Completed, Canceled, Missed }
    public enum ListingOwner { Player, Npc }
    // How a listing left the belt. Player listings pay cash; NPC listings either go to the player (bought) or vanish.
    public enum ListingResult { OnBelt, SoldToTrader, Shredded, Expired, Canceled, BoughtByPlayer, NpcShredded, NpcExpired }

    // One belt entry, persisted as is (plain fields so the save can serialize it).
    [Serializable] public sealed class TradeListing
    {
        public string id;
        public ListingOwner owner;
        public int trader = -1;           // NPC listing: the trader who placed it; player listing: the trader who bought it
        public string itemId;             // item definition
        public int quantity, quality;
        public long valueMinor;           // player: undiscounted quote of the whole listing; NPC: one unit's intrinsic value
        public int maxDurability, durability; public string variant;   // NPC goods' generated profile
        public string escrowId;           // player listing: the escrow inventory entry holding the listed units
        public float enterTime;           // session seconds when it entered the belt
        public ListingResult result;
        public float settledAt;
        public long cash;                 // player listing: cash paid to the player; NPC listing bought: cash charged
        public string settlementId;       // set once when settled, so a payout can never apply twice
        public ItemProfile Profile => new ItemProfile(valueMinor, maxDurability, durability, variant);
        public bool Settled => result != ListingResult.OnBelt;
    }

    [Serializable] public sealed class TradeSessionData
    {
        public string campId; public int visit; public int seed;
        public TradeState state;
        public float elapsed;
        public float offerElapsed;        // seconds the offer window has been open (while seated, app in the foreground)
        public List<TradeListing> listings = new List<TradeListing>();
        public int nextListing;
    }

    // A settled player listing or an NPC purchase the caller must apply (wallet, escrow, statistics, report).
    public readonly struct TradeSettlement
    {
        public readonly TradeListing Listing; public readonly int Percent;
        public TradeSettlement(TradeListing listing, int percent) { Listing = listing; Percent = percent; }
    }

    // The conveyor session (section 12.1; Milestone 9; 30 s and a 12 s belt since the user's device test, November 8, 2026). Engine-free and deterministic: Advance moves the belt,
    // settles every listing at most once in time order, and finishes with the natural expiry. Rules:
    //   - player listing bought by a trader as it passes (seeded chance): 100% of its quote;
    //   - player listing reaching the shredder, or still on the belt at expiry: 80%;
    //   - Return before expiry: every still-unsold player listing at 70%, then the session is Canceled;
    //   - NPC goods never pay the player; the player buys them at the position price (110% at entry to 80% at the end).
    // One opportunity per camp visit: the session is keyed by camp and visit number and never restarts.
    public sealed class TradeSession
    {
        public const float Duration = 30f, BeltSeconds = 12f, ListingGap = 0.5f, BuyChance = 0.35f;
        // Offer window (user, November 8, 2026): sitting at a camp opens the traders' offer for 60 s; START TRADING begins the
        // session, otherwise the offer is missed for this visit.
        public const float OfferSeconds = 60f;
        public static readonly float[] TraderPositions = { 0.25f, 0.5f, 0.75f };
        public const int SoldPercent = 100, ShredPercent = 80, ExpiryPercent = 80, CancelPercent = 70;

        public TradeSessionData Data { get; }
        public TradeSession(TradeSessionData data) { Data = data ?? throw new ArgumentNullException(nameof(data)); }
        public TradeState State => Data.state;
        public float Remaining => Math.Max(0, Duration - Data.elapsed);
        public bool Active => Data.state == TradeState.Active;
        public bool Ended => Data.state == TradeState.Completed || Data.state == TradeState.Canceled || Data.state == TradeState.Missed;
        public bool Offering => Data.state == TradeState.NotStarted;
        public float OfferRemaining => Math.Max(0, OfferSeconds - Data.offerElapsed);
        public bool Matches(string campId, int visit) => Data.campId == campId && Data.visit == visit;

        public static int SeedFor(string campId, int visit)
        {
            unchecked { int h = 17; foreach (char c in campId ?? "") h = h * 31 + c; return h * 397 ^ visit * 7919; }
        }

        // The visit's offer: open, not started.
        public static TradeSession Offer(string campId, int visit) =>
            new TradeSession(new TradeSessionData { campId = campId, visit = visit, seed = SeedFor(campId, visit), state = TradeState.NotStarted });
        // Counts the offer window down; true when it has just run out (the offer is then Missed).
        public bool AdvanceOffer(float seconds)
        {
            if (!Offering || seconds <= 0) return false;
            Data.offerElapsed = Math.Min(OfferSeconds, Data.offerElapsed + seconds);
            if (Data.offerElapsed < OfferSeconds) return false;
            Data.state = TradeState.Missed; return true;
        }
        public static TradeSession Start(string campId, int visit, IReadOnlyList<TradeOffer> offers)
        {
            TradeSession session = Offer(campId, visit); session.Begin(offers); return session;
        }
        // Starts the visit's single session with the traders' seeded offers scheduled over all but the last 4 seconds.
        public void Begin(IReadOnlyList<TradeOffer> offers)
        {
            if (!Offering) throw new InvalidOperationException("The trade offer is no longer open");
            TradeSessionData data = Data; data.state = TradeState.Active; data.elapsed = 0;
            int n = offers.Count;
            for (int i = 0; i < n; i++)
            {
                TradeOffer offer = offers[i];
                float time = n <= 1 ? 0 : (Duration - 4f) * i / (n - 1) * 0.92f + Hash01(data.seed, i, 99) * 0.6f;
                data.listings.Add(new TradeListing { id = "npc-" + i, owner = ListingOwner.Npc, trader = offer.Trader, itemId = offer.ItemId, quantity = 1, quality = offer.Quality,
                    valueMinor = offer.Profile.UnitValue, maxDurability = offer.Profile.MaxDurability, durability = offer.Profile.Durability, variant = offer.Profile.Variant, enterTime = time });
            }
            data.listings.Sort((a, b) => a.enterTime.CompareTo(b.enterTime));
        }

        public static float Position(TradeListing listing, float time) => (time - listing.enterTime) / BeltSeconds;
        public float PositionNow(TradeListing listing) => Position(listing, Data.elapsed);
        public bool OnBeltNow(TradeListing listing) => !listing.Settled && Data.elapsed >= listing.enterTime && PositionNow(listing) < 1f;
        // Section 12.1 curve: 110% at the entry, 80% just before the shredder.
        public static double BuyMultiplier(float position) => 1.1 - 0.3 * Math.Max(0, Math.Min(1, position));
        // Whole cash charged for an NPC unit now (rounded down once, at least one cash).
        public long PriceNow(TradeListing listing) => Math.Max(1, Money.Charge((long)Math.Floor(listing.valueMinor * listing.quantity * BuyMultiplier(PositionNow(listing)) + 1e-6)));

        // A player listing enters at the belt start, queued at least half a second behind the previous one.
        public bool CanList(out string reason)
        {
            reason = "";
            if (!Active) { reason = "Trading has ended for this camp visit"; return false; }
            if (Data.elapsed >= Duration) { reason = "Time is up"; return false; }
            return true;
        }
        public TradeListing AddPlayerListing(string escrowId, ItemDefinition definition, int quantity, int quality, long quoteMinor)
        {
            if (!CanList(out string reason)) throw new InvalidOperationException(reason);
            float enter = Data.elapsed;
            foreach (TradeListing other in Data.listings) if (other.owner == ListingOwner.Player) enter = Math.Max(enter, other.enterTime + ListingGap);
            var listing = new TradeListing { id = "player-" + Data.nextListing++, owner = ListingOwner.Player, itemId = definition.Id, quantity = quantity, quality = quality,
                valueMinor = quoteMinor, escrowId = escrowId, enterTime = Math.Min(enter, Duration) };
            Data.listings.Add(listing);
            return listing;
        }

        // Buying an NPC listing on the belt: returns the cash to charge; the caller checks the wallet and delivers the item.
        public bool CanBuy(TradeListing listing, long cash, out long price, out string reason)
        {
            price = 0; reason = "";
            if (!Active || listing == null || listing.owner != ListingOwner.Npc || !Data.listings.Contains(listing)) { reason = "Not for sale"; return false; }
            if (!OnBeltNow(listing)) { reason = "That item is no longer on the belt"; return false; }
            price = PriceNow(listing);
            if (cash < price) { reason = $"Costs {Money.Cash(price)} cash; you have {Money.Cash(cash)}."; return false; }
            return true;
        }
        public TradeSettlement Buy(TradeListing listing, long cash)
        {
            if (!CanBuy(listing, cash, out long price, out string reason)) throw new InvalidOperationException(reason);
            Settle(listing, ListingResult.BoughtByPlayer, Data.elapsed, price);
            return new TradeSettlement(listing, 0);
        }

        // Moves the belt by `seconds` and returns every settlement in time order, ending with the natural expiry.
        public List<TradeSettlement> Advance(float seconds)
        {
            var settled = new List<TradeSettlement>();
            if (!Active || seconds <= 0) return settled;
            float from = Data.elapsed, to = Math.Min(Duration, from + seconds);
            var events = new List<(float time, int order, TradeListing listing, int trader)>();
            for (int i = 0; i < Data.listings.Count; i++)
            {
                TradeListing listing = Data.listings[i];
                if (listing.Settled) continue;
                if (listing.owner == ListingOwner.Player)
                    for (int t = 0; t < TraderPositions.Length; t++)
                    {
                        float time = listing.enterTime + TraderPositions[t] * BeltSeconds;
                        if (time > from && time <= to && Hash01(Data.seed, i, t) < BuyChance) events.Add((time, i * 4 + t, listing, t));
                    }
                float end = listing.enterTime + BeltSeconds;
                if (end > from && end <= to) events.Add((end, i * 4 + 3, listing, -1));
            }
            events.Sort((a, b) => a.time != b.time ? a.time.CompareTo(b.time) : a.order.CompareTo(b.order));
            foreach (var e in events)
            {
                if (e.listing.Settled) continue;   // already bought by an earlier trader
                if (e.listing.owner == ListingOwner.Npc) { Settle(e.listing, ListingResult.NpcShredded, e.time, 0); continue; }
                if (e.trader >= 0) { e.listing.trader = e.trader; settled.Add(Pay(e.listing, ListingResult.SoldToTrader, e.time, SoldPercent)); }
                else settled.Add(Pay(e.listing, ListingResult.Shredded, e.time, ShredPercent));
            }
            Data.elapsed = to;
            if (to >= Duration) settled.AddRange(Finish(TradeState.Completed, ListingResult.Expired, ExpiryPercent));
            return settled;
        }

        // Return: before expiry, settle unsold player listings at 70% and cancel; at or after expiry, finish naturally
        // instead (a late Return can never settle again). The opportunity is spent either way.
        public List<TradeSettlement> Cancel()
        {
            if (!Active) return new List<TradeSettlement>();
            if (Data.elapsed >= Duration) return Finish(TradeState.Completed, ListingResult.Expired, ExpiryPercent);
            return Finish(TradeState.Canceled, ListingResult.Canceled, CancelPercent);
        }

        private List<TradeSettlement> Finish(TradeState state, ListingResult playerResult, int percent)
        {
            var settled = new List<TradeSettlement>();
            foreach (TradeListing listing in Data.listings)
            {
                if (listing.Settled) continue;
                if (listing.owner == ListingOwner.Player) settled.Add(Pay(listing, playerResult, Data.elapsed, percent));
                else Settle(listing, ListingResult.NpcExpired, Data.elapsed, 0);
            }
            Data.state = state;
            return settled;
        }

        private TradeSettlement Pay(TradeListing listing, ListingResult result, float time, int percent)
        {
            Settle(listing, result, time, Money.Payout(listing.valueMinor, percent));
            return new TradeSettlement(listing, percent);
        }
        private void Settle(TradeListing listing, ListingResult result, float time, long cash)
        {
            if (listing.Settled) throw new InvalidOperationException("Listing already settled");
            listing.result = result; listing.settledAt = time; listing.cash = cash;
            listing.settlementId = Data.campId + "/" + Data.visit + "/" + listing.id;
        }

        // Deterministic [0, 1) from the session seed, listing index and channel (so a reload replays the same rolls).
        public static float Hash01(int seed, int index, int channel)
        {
            unchecked
            {
                uint h = (uint)seed * 2654435761u ^ (uint)(index + 1) * 2246822519u ^ (uint)(channel + 7) * 3266489917u;
                h ^= h >> 15; h *= 2246822519u; h ^= h >> 13; h *= 3266489917u; h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }
    }

    // One trader offer generated for a visit.
    public readonly struct TradeOffer
    {
        public readonly int Trader, Quality; public readonly string ItemId; public readonly ItemProfile Profile;
        public TradeOffer(int trader, string itemId, int quality, ItemProfile profile) { Trader = trader; ItemId = itemId; Quality = quality; Profile = profile; }
    }

    // Seeded trader offers (section 12.1): each of the three traders brings 3-5 entries from the discovered regions'
    // resources, tier tools and clothing. Stackable goods are one unit; NPC gear is creation level one, uniform random
    // Q1-Q5, full condition, no furniture bonus. Purchases never grant recipes or mastery.
    public static class TradeOffers
    {
        public static readonly string[] Tools = { "tool.pine_axe.normal", "item.pine_slingshot_normal", "item.pine_rod_normal" };
        public static readonly string[] Clothing = { "item.rabbit_hat", "item.rabbit_boots", "item.hemp_shirt", "item.hemp_pants", "item.starter_backpack" };
        // Interaction outputs every tier-one region offers alongside its featured loose resources.
        public static readonly string[] Interaction = { "material.pine_log", "ingredient.pine_nuts", "animal.rabbit.meat", "material.bone", "ingredient.animal_fat", "fish.rainbow_trout", "fish.bluegill" };

        public static List<TradeOffer> Generate(int seed, IEnumerable<string> resourceIds)
        {
            var resources = new List<string>();
            foreach (string id in resourceIds) if (Crafting.Items.Find(id) != null && !resources.Contains(id)) resources.Add(id);
            foreach (string id in Interaction) if (!resources.Contains(id)) resources.Add(id);
            var random = new Random(seed);
            var offers = new List<TradeOffer>();
            for (int trader = 0; trader < 3; trader++)
            {
                int count = 3 + random.Next(3);
                for (int k = 0; k < count; k++)
                {
                    double roll = random.NextDouble();
                    string id = roll < 0.6 ? resources[random.Next(resources.Count)] : roll < 0.8 ? Tools[random.Next(Tools.Length)] : Clothing[random.Next(Clothing.Length)];
                    ItemDefinition definition = Crafting.Items.Find(id);
                    int quality = 1 + random.Next(5);
                    offers.Add(new TradeOffer(trader, id, quality, ItemPrices.Profile(definition, quality, 1)));
                }
            }
            return offers;
        }
    }
}
