using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;

namespace TheTrailWalker.Domain.Settlement
{
    // One of the three gold offers. Kind garment|box|pack; a box offer has no ItemId, only its BoxTier.
    public sealed class BalloonOffer
    {
        public string Kind, ItemId;
        public int BoxTier;
        public long GoldPrice;
        public bool Sold;
        public int Revision;
    }

    // The balloon merchant's saved state: the open session (Offers is null while closed) and every bought box.
    public sealed class BalloonState
    {
        public long SessionId;
        public string OpenCampId;
        public BalloonOffer[] Offers;
        public List<BoxManifest> Boxes = new List<BoxManifest>();
    }

    // Sessions and purchases (PROJECT_RECOMENDATIONS section 14 refresh, 14.1 offer eligibility).
    public static class BalloonShop
    {
        public const string Stale = "The stock changed; check the offer again", SoldOut = "Sold out";
        private static readonly long[] BoxPrices = { 20, 20, 25, 30 };

        public static long BoxPrice(int tier) => BoxPrices[tier - 1];

        // A session already open at this camp is kept (repaints, duplicate opens, a reload); otherwise a new one rolls.
        public static void Open(BalloonState s, string campId, ICollection<int> regions, int reachedTier, Random rng)
        {
            if (s.Offers != null && s.OpenCampId == campId) return;
            int boxTier = BalloonHosts.RegionTier(campId);
            if (boxTier == 0) throw new ArgumentException("Not a balloon host: " + campId);
            s.SessionId++; s.OpenCampId = campId;
            int top = TopTier(regions, reachedTier);
            int[] earlier = regions.Select(Tier).Where(t => t < top).Distinct().ToArray();
            int RollTier() => earlier.Length == 0 || rng.Next(100) < 70 ? top : earlier[rng.Next(earlier.Length)];
            // Revisions are unique per session (2 x id, +1 once sold), so a stale tap from an older session never matches.
            int rev = checked((int)s.SessionId * 2);
            string garment = SpecialGear.Id(RollTier(), rng.Next(4)), pack = SpecialGear.Id(RollTier(), 4);
            s.Offers = new[] {
                new BalloonOffer { Kind = "garment", ItemId = garment, GoldPrice = SpecialGear.GoldPrice(garment), Revision = rev },
                new BalloonOffer { Kind = "box", BoxTier = boxTier, GoldPrice = BoxPrice(boxTier), Revision = rev },
                new BalloonOffer { Kind = "pack", ItemId = pack, GoldPrice = SpecialGear.GoldPrice(pack), Revision = rev } };
        }

        private static int Tier(int region) => (region - 1) / 6 + 1;

        // The player's current unlocked tier: it leads both the gear tier roll and box contents (the box tier is price and colour only).
        private static int TopTier(ICollection<int> regions, int reachedTier) => Math.Min(4, Math.Max(reachedTier, regions.Select(Tier).DefaultIfEmpty(1).Max()));

        public static void Close(BalloonState s) { s.Offers = null; s.OpenCampId = null; }

        private static BalloonOffer Offer(BalloonState s, int slot, int revision, out string reason)
        {
            reason = Stale;
            if (s.Offers == null || slot < 0 || slot >= s.Offers.Length) return null;
            BalloonOffer o = s.Offers[slot];
            if (o.Revision != revision) return null;
            if (o.Sold) { reason = SoldOut; return null; }
            reason = ""; return o;
        }

        // Slice 1 order: gold, then the whole item must fit (dry run), then deliver before debiting.
        public static bool TryBuyGear(BalloonState s, int slot, int revision, Wallet w, SpatialInventory bag, Func<string> newId, out string reason)
        {
            BalloonOffer o = Offer(s, slot, revision, out reason);
            if (o == null) return false;
            if (o.Kind == "box") { reason = SoldOut; return false; }
            if (!w.CanDebit(Currency.Gold, o.GoldPrice)) { reason = "Not enough gold"; return false; }
            ItemDefinition def = Crafting.Items.Find(o.ItemId);
            ItemProfile profile = ItemPrices.Profile(def, ShopService.Q);
            if (!ShopService.StoreFits(bag, def, profile, 1)) { reason = "Not enough room in the bag"; return false; }
            if (!ShopService.Store(bag, newId(), def, profile, 1)) throw new InvalidOperationException("Balloon gear passed the fit check but did not store");
            w.Debit(Currency.Gold, o.GoldPrice);
            o.Sold = true; o.Revision++;
            reason = ""; return true;
        }

        // The bag never matters: the manifest is rolled, gold debited and the box appended in one in-memory step (saved by the caller).
        public static bool TryBuyBox(BalloonState s, int slot, int revision, Wallet w, MysteryBoxContext ctx, out BoxManifest box, out string reason)
        {
            box = null;
            BalloonOffer o = Offer(s, slot, revision, out reason);
            if (o == null) return false;
            if (o.Kind != "box") { reason = SoldOut; return false; }
            if (!w.CanDebit(Currency.Gold, o.GoldPrice)) { reason = "Not enough gold"; return false; }
            box = MysteryBoxes.Roll("box." + s.SessionId, o.BoxTier, TopTier(ctx.Regions, ctx.ReachedTier), BalloonHosts.Region(s.OpenCampId), ctx, MysteryBoxes.Reserved(s));
            w.Debit(Currency.Gold, o.GoldPrice);
            s.Boxes.Add(box);
            o.Sold = true; o.Revision++;
            reason = ""; return true;
        }
    }
}
