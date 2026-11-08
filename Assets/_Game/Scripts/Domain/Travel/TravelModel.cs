using System;
using System.Collections.Generic;

namespace TheTrailWalker.Domain.Travel
{
    public enum TollCurrency { Cash, Gold }

    // A camp clearing sits beside the road on one side, never on the path itself.
    public enum CampSide { Left, Right }

    public sealed class CampDefinition
    {
        public readonly string Id, Name, RegionId;
        public readonly CampSide Side;
        public CampDefinition(string id, string name, string regionId, CampSide side)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(regionId)) throw new ArgumentException("Invalid camp");
            Id = id; Name = name; RegionId = regionId; Side = side;
        }
    }

    // A walkable leg from one camp to the next. The final camp of a region instead owns a short river
    // approach (ToCampId null) that ends at the ferry; it cannot be walked past without the ferry.
    public sealed class SegmentDefinition
    {
        public readonly string Id, FromCampId, ToCampId;
        public readonly float Length;
        // Largest rise or fall from the starting camp along the leg, in metres (0 = flat); hill regions climb and descend.
        public readonly float Relief;
        public bool RiverApproach => ToCampId == null;
        // Ferry landing leg: from the far riverbank to the next region's first camp. FromCampId is the previous region's
        // final camp, where the journey started (closing the game on this leg returns there).
        public readonly bool Landing;
        public SegmentDefinition(string id, string fromCampId, string toCampId, float length, float relief = 0, bool landing = false)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(fromCampId) || length <= 0 || float.IsNaN(length) || float.IsInfinity(length) || relief < 0 || float.IsNaN(relief) || (landing && toCampId == null)) throw new ArgumentException("Invalid segment");
            Id = id; FromCampId = fromCampId; ToCampId = toCampId; Length = length; Relief = relief; Landing = landing;
        }
    }

    public sealed class RegionDefinition
    {
        public readonly string Id, Name, NextRegionId;
        public readonly int Number;
        public readonly long TollCost;
        public readonly IReadOnlyList<CampDefinition> Camps;
        // The region's three featured loose resource IDs (section 6.2.2).
        public readonly IReadOnlyList<string> LooseItemIds;
        public RegionDefinition(string id, int number, string name, IReadOnlyList<CampDefinition> camps, IReadOnlyList<string> looseItemIds, long tollCost, string nextRegionId)
        {
            if (string.IsNullOrWhiteSpace(id) || number < 1 || string.IsNullOrWhiteSpace(name) || camps == null || camps.Count < 1 || looseItemIds == null || looseItemIds.Count != 3 || tollCost < 0) throw new ArgumentException("Invalid region");
            Id = id; Number = number; Name = name; Camps = camps; LooseItemIds = looseItemIds; TollCost = tollCost; NextRegionId = nextRegionId;
        }
        public CampDefinition FirstCamp => Camps[0];
        public CampDefinition FinalCamp => Camps[Camps.Count - 1];
    }

    public sealed class TravelCatalog
    {
        private readonly Dictionary<string, RegionDefinition> regions = new Dictionary<string, RegionDefinition>();
        private readonly Dictionary<string, CampDefinition> camps = new Dictionary<string, CampDefinition>();
        private readonly Dictionary<string, SegmentDefinition> segments = new Dictionary<string, SegmentDefinition>();
        private readonly Dictionary<string, SegmentDefinition> outgoing = new Dictionary<string, SegmentDefinition>();
        private readonly Dictionary<string, SegmentDefinition> landings = new Dictionary<string, SegmentDefinition>(); // by destination region
        public IReadOnlyList<RegionDefinition> Regions { get; }
        public RegionDefinition StartRegion => Regions[0];

        public TravelCatalog(IReadOnlyList<RegionDefinition> regionList, IReadOnlyList<SegmentDefinition> segmentList)
        {
            if (regionList == null || regionList.Count == 0 || segmentList == null) throw new ArgumentException("Invalid catalog");
            Regions = regionList;
            foreach (RegionDefinition region in regionList)
            {
                regions.Add(region.Id, region);
                foreach (CampDefinition camp in region.Camps) { if (camp.RegionId != region.Id) throw new ArgumentException("Camp region mismatch"); camps.Add(camp.Id, camp); }
            }
            foreach (SegmentDefinition segment in segmentList)
            {
                if (!camps.ContainsKey(segment.FromCampId) || (segment.ToCampId != null && !camps.ContainsKey(segment.ToCampId))) throw new ArgumentException("Segment references an unknown camp");
                segments.Add(segment.Id, segment);
                if (!segment.Landing) { outgoing.Add(segment.FromCampId, segment); continue; }
                // A landing runs from a final camp's river to the first camp of that region's next region.
                RegionDefinition from = regions[camps[segment.FromCampId].RegionId], to = regions[camps[segment.ToCampId].RegionId];
                if (from.FinalCamp.Id != segment.FromCampId || from.NextRegionId != to.Id || to.FirstCamp.Id != segment.ToCampId) throw new ArgumentException("Landing does not join a river to the next region's first camp: " + segment.Id);
                landings.Add(to.Id, segment);
            }
            foreach (RegionDefinition region in regionList)
                for (int i = 0; i < region.Camps.Count; i++)
                {
                    // Every camp owns exactly one outgoing leg: to the next camp, or the final camp's river approach.
                    if (!outgoing.TryGetValue(region.Camps[i].Id, out SegmentDefinition leg)) throw new ArgumentException("Camp has no outgoing segment: " + region.Camps[i].Id);
                    bool final = i == region.Camps.Count - 1;
                    if (final != leg.RiverApproach || (!final && leg.ToCampId != region.Camps[i + 1].Id)) throw new ArgumentException("Segment order does not follow camp order: " + leg.Id);
                }
        }
        public RegionDefinition Region(string id) => id != null && regions.TryGetValue(id, out RegionDefinition region) ? region : null;
        public CampDefinition Camp(string id) => id != null && camps.TryGetValue(id, out CampDefinition camp) ? camp : null;
        public SegmentDefinition Segment(string id) => id != null && segments.TryGetValue(id, out SegmentDefinition segment) ? segment : null;
        public SegmentDefinition Outgoing(string campId) => campId != null && outgoing.TryGetValue(campId, out SegmentDefinition segment) ? segment : null;
        public SegmentDefinition LandingInto(string regionId) => regionId != null && landings.TryGetValue(regionId, out SegmentDefinition segment) ? segment : null;
        // The leg that arrives at a camp: the previous camp's leg, or for a region's first camp its ferry landing.
        public SegmentDefinition Incoming(string campId)
        {
            foreach (SegmentDefinition segment in segments.Values) if (segment.ToCampId == campId) return segment;
            return null;
        }
        // The region a leg belongs to (resources, scenery, tier): a landing belongs to the region it leads into.
        public RegionDefinition RegionOf(SegmentDefinition segment) => Region(Camp(segment.Landing ? segment.ToCampId : segment.FromCampId).RegionId);
        public bool IsFinalCamp(string campId) { CampDefinition camp = Camp(campId); return camp != null && Region(camp.RegionId).FinalCamp.Id == campId; }

        // Prototype world: Dawn Plains (ten camps) and Windward Hills (twelve camps) complete; region 3 is the locked next silhouette.
        public static TravelCatalog Prototype()
        {
            var dawn = new RegionDefinition("region.01", 1, "Dawn Plains", new[] {
                new CampDefinition("camp.01.01", "Dawn Meadow", "region.01", CampSide.Right),
                new CampDefinition("camp.01.02", "Quiet Meadow", "region.01", CampSide.Left),
                new CampDefinition("camp.01.03", "Low Meadow", "region.01", CampSide.Right),
                // Complete Dawn Plains (October 31, 2026): the region's ten proposed camps (section 6.2.2); the river now
                // follows the tenth. IDs 01-03 are unchanged, so older saves keep their camps.
                new CampDefinition("camp.01.04", "Clover Bend", "region.01", CampSide.Left),
                new CampDefinition("camp.01.05", "Lark Rise", "region.01", CampSide.Right),
                new CampDefinition("camp.01.06", "Dew Hollow", "region.01", CampSide.Left),
                new CampDefinition("camp.01.07", "Amber Field", "region.01", CampSide.Right),
                new CampDefinition("camp.01.08", "Hare's Run", "region.01", CampSide.Left),
                new CampDefinition("camp.01.09", "Willow Rest", "region.01", CampSide.Right),
                new CampDefinition("camp.01.10", "Last Light", "region.01", CampSide.Left) },
                new[] { "material.pine_branch", "animal.rabbit.pelt", "food.apples" }, 500, "region.02");
            var windward = new RegionDefinition("region.02", 2, "Windward Hills", new[] {
                new CampDefinition("camp.02.01", "Dawn Hilltop", "region.02", CampSide.Left),
                // Added after camp.02.02 existed; IDs are stable, so saves keep working even though the order differs.
                new CampDefinition("camp.02.03", "Quiet Hilltop", "region.02", CampSide.Right),
                new CampDefinition("camp.02.02", "Low Hilltop", "region.02", CampSide.Left),
                // Complete Windward Hills (November 2, 2026): the region's twelve planned camps (section 6.2.2), strictly
                // alternating sides so neighbouring camp tabs never overlap on the map; the river follows Far Lookout.
                new CampDefinition("camp.02.04", "Windy Crest", "region.02", CampSide.Right),
                new CampDefinition("camp.02.05", "Heather Brow", "region.02", CampSide.Left),
                new CampDefinition("camp.02.06", "Gorse Saddle", "region.02", CampSide.Right),
                new CampDefinition("camp.02.07", "Cairn Top", "region.02", CampSide.Left),
                new CampDefinition("camp.02.08", "Shepherd's Fold", "region.02", CampSide.Right),
                new CampDefinition("camp.02.09", "Weathervane", "region.02", CampSide.Left),
                new CampDefinition("camp.02.10", "Stone Gate", "region.02", CampSide.Right),
                new CampDefinition("camp.02.11", "Kestrel Point", "region.02", CampSide.Left),
                new CampDefinition("camp.02.12", "Far Lookout", "region.02", CampSide.Right) },
                new[] { "material.hemp", "material.flint", "material.pine_cone" }, 1000, "region.03");
            return new TravelCatalog(new[] { dawn, windward }, new[] {
                new SegmentDefinition("segment.01.01", "camp.01.01", "camp.01.02", 150),
                new SegmentDefinition("segment.01.02", "camp.01.02", "camp.01.03", 180),
                // Stamina pacing (user, October 31, 2026): the two opening legs stay under the starter's 3 hearts (G1 is
                // claimed at Quiet Meadow, G2's apples unlock apple juice at Low Meadow); from Low Meadow on every leg costs
                // at least half a heart more than 3, so food is needed (12 hearts/km: 300 m = 3.6 hearts).
                new SegmentDefinition("segment.01.03", "camp.01.03", "camp.01.04", 300),
                new SegmentDefinition("segment.01.04", "camp.01.04", "camp.01.05", 310),
                new SegmentDefinition("segment.01.05", "camp.01.05", "camp.01.06", 320),
                new SegmentDefinition("segment.01.06", "camp.01.06", "camp.01.07", 330),
                new SegmentDefinition("segment.01.07", "camp.01.07", "camp.01.08", 340),
                new SegmentDefinition("segment.01.08", "camp.01.08", "camp.01.09", 350),
                new SegmentDefinition("segment.01.09", "camp.01.09", "camp.01.10", 360),
                new SegmentDefinition("segment.01.river", "camp.01.10", null, 30),
                // Ferry landing: from the far bank of Dawn Plains' river through rolling ground to Dawn Hilltop.
                new SegmentDefinition("segment.02.landing", "camp.01.10", "camp.02.01", 120, 1.5f, landing: true),
                // Windward Hills pacing (user, November 2, 2026): first region of the 350-600 m / 3.5-6 heart band, so every
                // leg needs food; lengths climb from 360 m (4.32 hearts) to 500 m (6 hearts) at the same 12 hearts/km. The
                // player is expected to stock food from Dawn Plains (full stacks) to cross. Relief rolls 4-12 m per leg.
                new SegmentDefinition("segment.02.01", "camp.02.01", "camp.02.03", 360, 4f),
                new SegmentDefinition("segment.02.03", "camp.02.03", "camp.02.02", 370, 8f),
                new SegmentDefinition("segment.02.02", "camp.02.02", "camp.02.04", 380, 6f),
                new SegmentDefinition("segment.02.04", "camp.02.04", "camp.02.05", 390, 7f),
                new SegmentDefinition("segment.02.05", "camp.02.05", "camp.02.06", 400, 10f),
                new SegmentDefinition("segment.02.06", "camp.02.06", "camp.02.07", 410, 12f),
                new SegmentDefinition("segment.02.07", "camp.02.07", "camp.02.08", 420, 8f),
                new SegmentDefinition("segment.02.08", "camp.02.08", "camp.02.09", 440, 9f),
                new SegmentDefinition("segment.02.09", "camp.02.09", "camp.02.10", 460, 12f),
                new SegmentDefinition("segment.02.10", "camp.02.10", "camp.02.11", 480, 8f),
                new SegmentDefinition("segment.02.11", "camp.02.11", "camp.02.12", 500, 5f),
                new SegmentDefinition("segment.02.river", "camp.02.12", null, 30) });
        }
    }

    // Persistent travel progress. The player is either seated at CurrentCampId or walking its outgoing segment.
    public sealed class TravelState
    {
        private readonly TravelCatalog catalog;
        private readonly HashSet<string> visited = new HashSet<string>();
        private readonly List<string> discovered = new List<string>();
        private readonly HashSet<string> paidTolls = new HashSet<string>();
        private readonly Dictionary<string, int> dropCycles = new Dictionary<string, int>();
        public string CurrentCampId { get; private set; }
        public bool Seated { get; private set; }
        public string LastTrailCampId { get; private set; }
        public int VisitNumber { get; private set; }
        // Cash and gold wallets (Milestone 9, section 12.2); gold can pay the ferry toll at the exchange rate (cash / 1,000, at least 1).
        public Economy.Wallet Wallet { get; private set; } = new Economy.Wallet();
        public long Cash => Wallet.Cash;
        public long Gold => Wallet.Gold;
        public IReadOnlyCollection<string> VisitedCamps => visited;
        public IReadOnlyList<string> DiscoveredRegions => discovered;
        public IReadOnlyCollection<string> PaidTolls => paidTolls;
        public IReadOnlyDictionary<string, int> DropCycles => dropCycles;
        public CampDefinition CurrentCamp => catalog.Camp(CurrentCampId);
        public RegionDefinition CurrentRegion => catalog.Region(CurrentCamp.RegionId);
        // True after the ferry crossing, while walking the landing leg to the next region's first camp.
        public bool OnFarBank { get; private set; }
        public SegmentDefinition CurrentSegment => OnFarBank ? catalog.LandingInto(CurrentRegion.NextRegionId) : catalog.Outgoing(CurrentCampId);
        public TravelCatalog Catalog => catalog;

        private TravelState(TravelCatalog travelCatalog) { catalog = travelCatalog ?? throw new ArgumentNullException(nameof(travelCatalog)); }

        // A new profile starts seated at the first camp of the first region, which initialises its first drop cycle.
        public static TravelState Start(TravelCatalog catalog)
        {
            var state = new TravelState(catalog);
            state.ArriveAtCamp(catalog.StartRegion.FirstCamp.Id);
            return state;
        }

        // Arrival seats the player, starts a new camp visit, records discovery and rolls a new drop cycle for the
        // camp's outgoing segment (section 7.3). Standing up later does not roll again.
        public string ArriveAtCamp(string campId)
        {
            CampDefinition camp = catalog.Camp(campId) ?? throw new ArgumentException("Unknown camp");
            CurrentCampId = camp.Id; Seated = true; OnFarBank = false; LastTrailCampId = camp.Id; VisitNumber++;
            visited.Add(camp.Id);
            if (!discovered.Contains(camp.RegionId)) discovered.Add(camp.RegionId);
            string segmentId = catalog.Outgoing(camp.Id).Id;
            dropCycles[segmentId] = dropCycles.TryGetValue(segmentId, out int cycle) ? cycle + 1 : 1;
            return segmentId;
        }
        public int CycleOf(string segmentId) => dropCycles.TryGetValue(segmentId, out int cycle) ? cycle : 0;

        // Upward speed gesture while seated. A region's final camp has no walkable road onward: the ferry is the way on.
        public bool CanStandUp(out string reason)
        {
            reason = "";
            if (!Seated) { reason = "Already walking"; return false; }
            return true;
        }
        public void StandUp() { if (!CanStandUp(out string reason)) throw new InvalidOperationException(reason); Seated = false; }

        // Reopening the app after it closed mid-segment returns to the camp the segment started from. Confirmed: this
        // counts as a camp visit, so it starts a new visit and rerolls that segment's resources. Returns the rolled segment.
        public string ResumeAtCampAfterRestart() => Seated ? null : ArriveAtCamp(CurrentCampId);

        // Walking to the end of the current segment arrives at its destination camp.
        public string ArriveAtSegmentEnd()
        {
            SegmentDefinition segment = CurrentSegment;
            if (Seated || segment.RiverApproach) throw new InvalidOperationException("Not walking a segment");
            return ArriveAtCamp(segment.ToCampId);
        }

        // Camp map: unlocked camps of the current region by exact selection; other discovered regions arrive at their first camp.
        public bool CanTravelToCamp(string campId, bool busy, out string reason)
        {
            reason = "";
            CampDefinition camp = catalog.Camp(campId);
            if (camp == null) { reason = "Unknown camp"; return false; }
            if (!Seated || busy) { reason = "Travel is only available while seated at a camp and not trading or recovering"; return false; }
            if (camp.Id == CurrentCampId) { reason = "You are already here"; return false; }
            if (camp.RegionId != CurrentCamp.RegionId) { reason = "Choose another region from the region list"; return false; }
            if (!visited.Contains(camp.Id)) { reason = camp.Name + " has not been reached yet"; return false; }
            return true;
        }
        public bool CanTravelToRegion(string regionId, bool busy, out string reason)
        {
            reason = "";
            RegionDefinition region = catalog.Region(regionId);
            if (region == null) { reason = "This region is not available yet"; return false; }
            if (!Seated || busy) { reason = "Travel is only available while seated at a camp and not trading or recovering"; return false; }
            if (region.Id == CurrentCamp.RegionId) { reason = "Choose a camp in this region instead"; return false; }
            if (!discovered.Contains(region.Id)) { reason = region.Name + " has not been discovered"; return false; }
            return true;
        }
        public string TravelToCamp(string campId, bool busy) { if (!CanTravelToCamp(campId, busy, out string reason)) throw new InvalidOperationException(reason); return ArriveAtCamp(campId); }
        public string TravelToRegion(string regionId, bool busy) { if (!CanTravelToRegion(regionId, busy, out string reason)) throw new InvalidOperationException(reason); return ArriveAtCamp(catalog.Region(regionId).FirstCamp.Id); }

        // Border toll: asked once by the ferry NPC when the player walks up to the river from the region's last camp,
        // payable in cash or gold, and persisted before any crossing. The gold price follows the warehouse exchange
        // (1 gold per 1,000 cash, revised October 31, 2026), rounded down, at least 1. Toll table: plan section 12.3.
        public static long TollGold(RegionDefinition region) => Math.Max(1, region.TollCost / 1000);
        public bool CanPayToll(TollCurrency currency, out string reason)
        {
            reason = "";
            if (Seated || !CurrentSegment.RiverApproach) { reason = "The ferry NPC waits at the river past the region's last camp"; return false; }
            if (paidTolls.Contains(CurrentRegion.Id)) { reason = "Toll already paid"; return false; }
            long price = currency == TollCurrency.Gold ? TollGold(CurrentRegion) : CurrentRegion.TollCost, have = currency == TollCurrency.Gold ? Gold : Cash;
            if (have < price) { reason = $"The toll is {price} {currency.ToString().ToLowerInvariant()}; you have {have}."; return false; }
            return true;
        }
        public void PayToll(TollCurrency currency)
        {
            if (!CanPayToll(currency, out string reason)) throw new InvalidOperationException(reason);
            if (currency == TollCurrency.Gold) Wallet.Debit(Economy.Currency.Gold, TollGold(CurrentRegion)); else Wallet.Debit(Economy.Currency.Cash, CurrentRegion.TollCost);
            paidTolls.Add(CurrentRegion.Id);
        }
        // Once the toll is paid, reaching the ferry takes the player straight to the next region's first camp.
        public bool CanCrossRiver(bool busy, out string reason)
        {
            reason = "";
            if (Seated || busy || OnFarBank || !CurrentSegment.RiverApproach) { reason = "Walk up to the ferry at the river"; return false; }
            if (!paidTolls.Contains(CurrentRegion.Id)) { reason = "Pay the ferry toll first"; return false; }
            if (catalog.Region(CurrentRegion.NextRegionId) == null) { reason = "The next region arrives in a later milestone"; return false; }
            return true;
        }
        // Crossing lands the player on the far bank at the start of the landing leg (rolling that leg's resources);
        // the next region is discovered when its first camp is reached.
        public string CrossRiver(bool busy)
        {
            if (!CanCrossRiver(busy, out string reason)) throw new InvalidOperationException(reason);
            OnFarBank = true;
            string segmentId = CurrentSegment.Id;
            dropCycles[segmentId] = dropCycles.TryGetValue(segmentId, out int cycle) ? cycle + 1 : 1;
            return segmentId;
        }

        // Debug (user, November 2, 2026): marks every camp of the current region as reached so the camp map can travel to
        // any of them. Other regions, discovery, tolls and drop cycles are untouched. Returns how many camps were newly unlocked.
        public int UnlockRegionCampsForTest()
        {
            int unlocked = 0;
            foreach (CampDefinition camp in CurrentRegion.Camps) if (visited.Add(camp.Id)) unlocked++;
            return unlocked;
        }

        // Debug wallet grants (kept until Milestone 18).
        public void GrantTestCash(long amount) { if (amount <= 0 || Cash > long.MaxValue - amount) throw new ArgumentOutOfRangeException(nameof(amount)); Wallet.Credit(Economy.Currency.Cash, amount); }
        public void GrantTestGold(long amount) { if (amount <= 0 || Gold > long.MaxValue - amount) throw new ArgumentOutOfRangeException(nameof(amount)); Wallet.Credit(Economy.Currency.Gold, amount); }

        public static TravelState Restore(TravelCatalog catalog, string campId, bool seated, string lastTrailCampId, int visitNumber, long cash,
            IEnumerable<string> visitedCamps, IEnumerable<string> discoveredRegions, IEnumerable<string> tolls, IEnumerable<KeyValuePair<string, int>> cycles, long gold = 0, bool farBank = false)
        {
            var state = new TravelState(catalog);
            CampDefinition camp = catalog.Camp(campId);
            if (camp == null || catalog.Camp(lastTrailCampId) == null || visitNumber < 1 || cash < 0 || gold < 0 || visitedCamps == null || discoveredRegions == null || tolls == null || cycles == null) throw new ArgumentException("Invalid travel state");
            foreach (string id in visitedCamps) if (catalog.Camp(id) == null || !state.visited.Add(id)) throw new ArgumentException("Invalid visited camp");
            foreach (string id in discoveredRegions) if (catalog.Region(id) == null || state.discovered.Contains(id)) throw new ArgumentException("Invalid discovered region"); else state.discovered.Add(id);
            foreach (string id in tolls) if (catalog.Region(id) == null || !state.discovered.Contains(id) || !state.paidTolls.Add(id)) throw new ArgumentException("Invalid paid toll");
            foreach (var cycle in cycles) if (catalog.Segment(cycle.Key) == null || cycle.Value < 1 || state.dropCycles.ContainsKey(cycle.Key)) throw new ArgumentException("Invalid drop cycle"); else state.dropCycles.Add(cycle.Key, cycle.Value);
            if (!state.visited.Contains(camp.Id) || !state.discovered.Contains(camp.RegionId) || !state.visited.Contains(lastTrailCampId) || state.CycleOf(catalog.Outgoing(camp.Id).Id) < 1) throw new ArgumentException("Travel state is inconsistent");
            foreach (string id in state.visited) if (!state.discovered.Contains(catalog.Camp(id).RegionId)) throw new ArgumentException("Visited camp in an undiscovered region");
            state.CurrentCampId = camp.Id; state.Seated = seated; state.LastTrailCampId = lastTrailCampId; state.VisitNumber = visitNumber; state.Wallet = new Economy.Wallet(cash, gold);
            if (farBank)
            {
                SegmentDefinition landing = catalog.LandingInto(state.CurrentRegion.NextRegionId);
                if (seated || landing == null || !state.paidTolls.Contains(camp.RegionId) || state.CycleOf(landing.Id) < 1) throw new ArgumentException("Invalid far-bank state");
                state.OnFarBank = true;
            }
            return state;
        }
    }

    // Deterministic loose-resource roll for one drop cycle: fixed point positions, identity and quality from the cycle seed.
    public static class LooseDrops
    {
        public readonly struct Drop
        {
            public readonly int Point, Quality; public readonly string ItemId; public readonly float Distance; public readonly bool LeftSide;
            public Drop(int point, string itemId, int quality, float distance, bool leftSide) { Point = point; ItemId = itemId; Quality = quality; Distance = distance; LeftSide = leftSide; }
        }
        // Density (confirmed October 30, 2026): at least 20 gathering points per segment, about one every 7 m on longer
        // legs, alternating sides and clear of the camps at both ends.
        public const float EndClearance = 6f, MetresPerPoint = 7f;
        public const int MinimumPoints = 20;
        public static int PointCount(SegmentDefinition segment) => segment.RiverApproach ? 0 : Math.Max(MinimumPoints, (int)Math.Ceiling(segment.Length / MetresPerPoint));
        public static IReadOnlyList<Drop> Roll(RegionDefinition region, SegmentDefinition segment, int cycle)
        {
            if (region == null || segment == null || cycle < 1) throw new ArgumentException("Invalid drop roll");
            var drops = new List<Drop>();
            int count = PointCount(segment);
            float usable = segment.Length - 2 * EndClearance;
            for (int point = 0; point < count; point++)
            {
                uint hash = Hash(segment.Id, cycle, point);
                float distance = EndClearance + usable * (point + 0.5f) / count;
                drops.Add(new Drop(point, region.LooseItemIds[(int)(hash % 3)], 1 + (int)((hash / 3) % 5), distance, point % 2 == 0));
            }
            // Every leg offers each of the region's three featured items at least once (for example apples on the opening
            // legs, so the G2 delivery always leaves enough to craft the first apple juice). A missing item takes over the
            // first point of the most common one, deterministically.
            for (int k = 0; k < region.LooseItemIds.Count && count >= region.LooseItemIds.Count; k++)
            {
                string wanted = region.LooseItemIds[k];
                if (drops.Exists(d => d.ItemId == wanted)) continue;
                string common = null; int most = 0;
                foreach (string id in region.LooseItemIds) { int n = drops.FindAll(d => d.ItemId == id).Count; if (n > most) { most = n; common = id; } }
                int at = drops.FindIndex(d => d.ItemId == common);
                Drop old = drops[at];
                drops[at] = new Drop(old.Point, wanted, old.Quality, old.Distance, old.LeftSide);
            }
            return drops;
        }
        public static string ItemId(string segmentId, int cycle, int point) => $"loose-{segmentId}-c{cycle}-p{point}";
        // FNV-1a over the stable segment ID, cycle and point index: same inputs always give the same roll.
        internal static uint Hash(string segmentId, int cycle, int point)
        {
            uint hash = 2166136261;
            foreach (char c in segmentId + "|" + cycle + "|" + point) { hash ^= c; hash *= 16777619; }
            return hash;
        }
    }
}

namespace TheTrailWalker.Domain.Travel
{
    public enum InteractableKind { Tree, AnimalSpawn, FishingPoint }

    // Fixed authored interactable anchors for a segment (section 7.3). Interaction itself arrives in Milestone 6.
    public readonly struct InteractableAnchor
    {
        public readonly InteractableKind Kind; public readonly string Id; public readonly float Distance, Lateral; public readonly int Animals;
        public InteractableAnchor(InteractableKind kind, string id, float distance, float lateral, int animals) { Kind = kind; Id = id; Distance = distance; Lateral = lateral; Animals = animals; }
    }

    // Route stamina budget (section 4.4): the authored walking drain per metre and the hearts a leg costs.
    public static class RouteBudget
    {
        // Region-one opening rate of 12 hearts/km (raised from 8 after phone testing found it too slow).
        public const double RegionOneDrainPerMetre = 0.012;
        public const double StarterHearts = 3, MaximumLegHearts = 26;
        public static double Hearts(SegmentDefinition segment) => segment.RiverApproach ? 0 : segment.Length * RegionOneDrainPerMetre;

        // Milestone 8 item 5 (November 2, 2026): walks a leg against the real stamina model (fixed-rate food delivery, no
        // reserve beyond the cap) eating the given servings in order. A serving is eaten when it fits under the cap
        // without waste, or as a last resort when the meter is nearly empty. Shared by the route checks and the departure
        // preview, so both always agree with the in-game drain.
        public readonly struct LegOutcome
        {
            public readonly bool Completed; public readonly int ServingsEaten; public readonly double FoodHeartsEaten, FinishHearts, LowestHearts, WalkedMetres, Seconds;
            public LegOutcome(bool completed, int eaten, double food, double finish, double lowest, double walked, double seconds)
            { Completed = completed; ServingsEaten = eaten; FoodHeartsEaten = food; FinishHearts = finish; LowestHearts = lowest; WalkedMetres = walked; Seconds = seconds; }
        }
        public const double LastResortHearts = 0.25;
        public static LegOutcome Simulate(double lengthMetres, double drainPerMetre, double maximumHearts, double startHearts, double pendingFood,
            IReadOnlyList<double> servings, double metresPerSecond, double step = 0.25)
        {
            if (lengthMetres < 0 || drainPerMetre < 0 || metresPerSecond <= 0 || step <= 0 || servings == null) throw new ArgumentOutOfRangeException(nameof(lengthMetres));
            var stamina = new Equipment.StaminaState();
            stamina.Restore(Math.Min(startHearts, maximumHearts), maximumHearts, pendingFood, false, 0);
            int next = 0; double food = 0, walked = 0, seconds = 0, lowest = stamina.Hearts;
            while (walked < lengthMetres - 1e-6)
            {
                if (stamina.PendingFood <= 1e-9 && next < servings.Count
                    && (stamina.Hearts + servings[next] <= maximumHearts + 1e-9 || stamina.Hearts <= LastResortHearts))
                { stamina.QueueFood(servings[next]); food += servings[next]; next++; }
                double dt = Math.Min(step, (lengthMetres - walked) / metresPerSecond);
                double allowed = stamina.LimitWalkingSeconds(dt, metresPerSecond, drainPerMetre);
                stamina.Advance(allowed, allowed * metresPerSecond, false, true, drainPerMetre);
                walked += allowed * metresPerSecond; seconds += allowed;
                lowest = Math.Min(lowest, stamina.Hearts);
                if (allowed < dt - 1e-9)
                {
                    // The meter emptied: the walker stops; pending food still arrives while standing (as in the game).
                    if (stamina.PendingFood <= 1e-9 && next >= servings.Count) return new LegOutcome(false, next, food, 0, 0, walked, seconds);
                    stamina.Advance(dt - allowed, 0, false, true, drainPerMetre); seconds += dt - allowed;
                }
            }
            return new LegOutcome(true, next, food, stamina.Hearts, lowest, lengthMetres, seconds);
        }
        public static LegOutcome Simulate(SegmentDefinition segment, double maximumHearts, double startHearts, double pendingFood, IReadOnlyList<double> servings, double metresPerSecond) =>
            Simulate(segment.RiverApproach ? 0 : segment.Length, RegionOneDrainPerMetre, maximumHearts, startHearts, pendingFood, servings, metresPerSecond);
    }

    public static class SegmentLayout
    {
        public static int Tier(RegionDefinition region) => (region.Number - 1) / 6 + 1;
        // Confirmed starting counts: at least four chopping trees, two to three animal spawns of one to four animals
        // (at most two for large animals from tier three), and two to three fishing points (one per segment in region 1).
        // Longer legs between camps, typical from tier three, get more.
        public static int TreeCount(SegmentDefinition segment) => Math.Max(4, (int)Math.Round(segment.Length / 40f));
        public static int AnimalSpawnCount(SegmentDefinition segment) => segment.Length < 200 ? 2 : Math.Max(3, 3 + (int)((segment.Length - 300) / 150));
        public static int FishingCount(RegionDefinition region, SegmentDefinition segment) => region.Number == 1 ? 1 : segment.Length < 200 ? 2 : Math.Max(3, 3 + (int)((segment.Length - 300) / 200));
        public static int MaxAnimals(RegionDefinition region) => Tier(region) >= 3 ? 2 : 4;

        public static IReadOnlyList<InteractableAnchor> Build(RegionDefinition region, SegmentDefinition segment)
        {
            var anchors = new List<InteractableAnchor>();
            if (segment.RiverApproach) return anchors;
            Add(anchors, region, segment, InteractableKind.Tree, TreeCount(segment), 6.5f, 0.0f);
            Add(anchors, region, segment, InteractableKind.AnimalSpawn, AnimalSpawnCount(segment), 12f, 0.33f);
            Add(anchors, region, segment, InteractableKind.FishingPoint, FishingCount(region, segment), 10f, 0.66f);
            return anchors;
        }
        private static void Add(List<InteractableAnchor> anchors, RegionDefinition region, SegmentDefinition segment, InteractableKind kind, int count, float lateral, float phase)
        {
            float usable = segment.Length - 2 * 14f;
            for (int i = 0; i < count; i++)
            {
                uint hash = LooseDrops.Hash(segment.Id + kind, 0, i);
                float distance = 14f + usable * ((i + 0.25f + phase * 0.5f) / count);
                // Alternate sides, offset from the drop rows so anchors never stack on a pickup.
                float side = ((i + (int)kind) % 2 == 0) ? -1f : 1f;
                int animals = kind == InteractableKind.AnimalSpawn ? 1 + (int)(hash % (uint)MaxAnimals(region)) : 0;
                anchors.Add(new InteractableAnchor(kind, $"{segment.Id}.{kind}.{i}", distance, side * (lateral + hash % 3), animals));
            }
        }
    }
}
