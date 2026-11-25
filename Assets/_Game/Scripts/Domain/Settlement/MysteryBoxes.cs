using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Missions;

namespace TheTrailWalker.Domain.Settlement
{
    // One pre-rolled reward. Kind "item": ItemId x Units with its Q5 Profile. Kind "recipe": RecipeId, with the persisted fallback
    // (item, units, profile) granted instead when the recipe is already known at claim. ClaimTxn is set once, with Claimed, on grant.
    // Profiles are rolled with the manifest, so a later price retune never changes an unclaimed reward.
    public sealed class BoxEntry
    {
        public string Kind, ItemId, RecipeId, FallbackItemId, ClaimTxn;
        public int Units, FallbackUnits;
        public ItemProfile Profile, FallbackProfile;
        public bool Claimed;
    }

    // A bought box: six entries rolled once at purchase and never again.
    public sealed class BoxManifest
    {
        public string BoxId;
        public int Tier;
        public BoxEntry[] Entries;
    }

    // What a box roll reads. MissionRequests: the active mission's requested item ids (from MissionState), empty when none.
    public sealed class MysteryBoxContext
    {
        public ICollection<int> Regions;
        public int ReachedTier;
        public CraftingState Crafting;
        public IEnumerable<string> MissionRequests;
        public Random Rng;
    }

    // Box manifests, claims and recipe reservations (PROJECT_RECOMENDATIONS section 14).
    public static class MysteryBoxes
    {
        public const int StackUnits = 1000;
        public const string Waiting = "Reward waiting in the box: make room in the bag", AllClaimed = "All rewards claimed";

        public static ISet<string> Reserved(BalloonState s) =>
            new HashSet<string>(s.Boxes.SelectMany(b => b.Entries).Where(e => e.Kind == "recipe" && !Done(e)).Select(e => e.RecipeId));

        private static bool Done(BoxEntry e) => e.Claimed || e.ClaimTxn != null;

        // Slot 1 is the mission item (or a current-region material); slots 2-6 roll 80% materials/food, 10% tool, 5% clothing/backpack,
        // 5% recipe. tier is the player's current unlocked tier: the cap, with 70% at it and 30% below when both sides exist.
        // boxTier (the host's region tier) is only recorded on the manifest; it sets price and colour, never contents.
        internal static BoxManifest Roll(string boxId, int boxTier, int tier, int campRegion, MysteryBoxContext ctx, ISet<string> reserved)
        {
            Random rng = ctx.Rng;
            List<(string Id, int Tier)> goods = Goods(ctx.Regions, ctx.ReachedTier, tier);
            if (goods.Count == 0) throw new InvalidOperationException("No box goods for tier " + tier);
            var taken = new HashSet<string>(reserved);
            var entries = new BoxEntry[6];
            entries[0] = Item(MissionItem(ctx.MissionRequests) ?? RegionMaterial(campRegion, ctx.Regions, rng) ?? Pick(goods, tier, rng));
            for (int i = 1; i < 6; i++)
            {
                int roll = rng.Next(100);
                string id = roll < 80 ? null : roll < 90 ? Pick(Crafted(ItemCategory.Tool, tier), tier, rng) : roll < 95 ? Pick(Crafted(ItemCategory.Wearable, tier), tier, rng) : null;
                if (roll < 95) { entries[i] = Item(id ?? Pick(goods, tier, rng)); continue; }
                RecipeDefinition recipe = PickRecipe(ctx, tier, taken, rng);
                string fallback = Pick(goods, tier, rng);
                if (recipe == null) { entries[i] = Item(fallback); continue; }
                taken.Add(recipe.Id);
                entries[i] = new BoxEntry { Kind = "recipe", RecipeId = recipe.Id, FallbackItemId = fallback, FallbackUnits = StackUnits, FallbackProfile = Q5(fallback) };
            }
            return new BoxManifest { BoxId = boxId, Tier = boxTier, Entries = entries };
        }

        private static BoxEntry Item(string id) => new BoxEntry { Kind = "item", ItemId = id, Units = Crafting.Items.Find(id).Stackable ? StackUnits : 1, Profile = Q5(id) };
        private static ItemProfile Q5(string id) => ItemPrices.Profile(Crafting.Items.Find(id), ShopService.Q);

        // 70% the top tier, 30% earlier tiers; an empty side falls back to the other. Null when the pool is empty.
        private static string Pick(List<(string Id, int Tier)> pool, int top, Random rng)
        {
            var high = pool.Where(p => p.Tier == top).ToList(); var low = pool.Where(p => p.Tier < top).ToList();
            List<(string Id, int Tier)> side = rng.Next(100) < 70 ? (high.Count > 0 ? high : low) : (low.Count > 0 ? low : high);
            return side.Count == 0 ? null : side[rng.Next(side.Count)].Id;
        }

        private static bool FisheryOnly(string itemId) => ShopCatalog.All.Any(e => e.ShopId == ShopCatalog.Fishery && e.Role == ShopSlotRole.Exclusive && e.ItemId == itemId);

        private static bool Physical(ItemDefinition d) => d != null && d.Stackable && !SpecialGear.Is(d.Id) && !FisheryOnly(d.Id);

        // The mission exception admits any physical stackable request, never fishery-only seafood.
        private static string MissionItem(IEnumerable<string> requests) => requests?.FirstOrDefault(id => Physical(Crafting.Items.Find(id)));

        private static string RegionMaterial(int region, ICollection<int> regions, Random rng)
        {
            if (!regions.Contains(region)) return null;
            var pool = Goods(new[] { region }, 0, 4).Where(g => Crafting.Items.Find(g.Id).Category == ItemCategory.Material).ToList();
            return pool.Count == 0 ? null : pool[rng.Next(pool.Count)].Id;
        }

        // Materials and food from discovered sources: loose and harvest drops (tier one) and region- or tier-gated shop stock,
        // minus fishery-only seafood and gold-priced bakery goods. Each item at its lowest source tier, capped at the box tier.
        private static List<(string Id, int Tier)> Goods(ICollection<int> regions, int reachedTier, int cap)
        {
            var tiers = new Dictionary<string, int>();
            void Add(string id, int t) { if (t <= cap && Physical(Crafting.Items.Find(id)) && (!tiers.TryGetValue(id, out int had) || t < had)) tiers[id] = t; }
            int RegionTier(int r) => (r - 1) / 6 + 1;
            foreach (string id in ItemSources.LooseIds) foreach (int r in ItemSources.LooseRegions(id)) if (regions.Contains(r)) Add(id, RegionTier(r));
            foreach (string id in ItemSources.HarvestIds) { ItemSources.TryHarvest(id, out _, out int[] hr); foreach (int r in hr) if (regions.Contains(r)) Add(id, RegionTier(r)); }
            foreach (string shop in ShopCatalog.All.Select(e => e.ShopId).Distinct())
                foreach (ShopEntry e in ShopCatalog.Eligible(shop, regions, reachedTier)) if (!e.GoldPerServing.HasValue) Add(e.ItemId, e.Tier);
            return tiers.Select(kv => (kv.Key, kv.Value)).OrderBy(g => g.Key, StringComparer.Ordinal).ToList();
        }

        // Ordinary crafted tools or clothing/backpacks: live recipe outputs, never debug or special gear.
        private static List<(string Id, int Tier)> Crafted(ItemCategory category, int cap) =>
            RecipeBook.All.Where(r => !r.Retired && r.Tier <= cap && r.Output != null && r.Output.Category == category && !r.OutputId.StartsWith("debug.") && !SpecialGear.Is(r.OutputId))
                .Select(r => (r.OutputId, r.Tier)).Distinct().ToList();

        private static RecipeDefinition PickRecipe(MysteryBoxContext ctx, int cap, ISet<string> taken, Random rng)
        {
            var pool = RecipeBook.All.Where(r => r.MysteryBoxEligible && r.Tier <= cap && !ctx.Crafting.Knows(r.Id) && !taken.Contains(r.Id)
                && RecipeSources.Reachable(r, ctx.Regions, ctx.ReachedTier, ctx.Crafting, out _)).Select(r => (r.Id, r.Tier)).ToList();
            string id = Pick(pool, cap, rng);
            return id == null ? null : RecipeBook.Find(id);
        }

        // Grants one entry at most once. A recipe already known grants its fallback; a physical reward stores only when the whole
        // entry fits, otherwise it waits in the box. The txn is recorded with the grant, so a repeat claim grants nothing.
        public static bool Claim(BalloonState s, string boxId, int entry, SpatialInventory bag, CraftingState crafting, Func<string> newId, out string note)
        {
            note = "";
            BoxManifest box = s.Boxes.FirstOrDefault(b => b.BoxId == boxId);
            if (box == null || entry < 0 || entry >= box.Entries.Length) return false;
            BoxEntry e = box.Entries[entry];
            if (Done(e)) { if (box.Entries.All(Done)) note = AllClaimed; return false; }
            string txn;
            // A recipe id no longer in the book also takes the fallback (Task 6 restores such manifests as they are).
            if (e.Kind == "recipe" && RecipeBook.Find(e.RecipeId) != null && !crafting.Knows(e.RecipeId))
            {
                crafting.Learn(e.RecipeId);
                txn = newId();
                note = "Recipe learned: " + RecipeBook.Find(e.RecipeId).Name;
            }
            else
            {
                bool fallback = e.Kind == "recipe";
                ItemDefinition def = Crafting.Items.Find(fallback ? e.FallbackItemId : e.ItemId);
                int units = fallback ? e.FallbackUnits : e.Units;
                if (def == null || units < 1) return false;
                ItemProfile profile = fallback ? e.FallbackProfile : e.Profile;
                if (profile.UnitValue <= 0) profile = ItemPrices.Profile(def, ShopService.Q);   // never store a zero-valued reward: recompute at Q5
                if (!ShopService.StoreFits(bag, def, profile, units)) { note = Waiting; return false; }
                txn = newId();
                if (!ShopService.Store(bag, txn, def, profile, units)) throw new InvalidOperationException("Box reward passed the fit check but did not store");
            }
            e.Claimed = true; e.ClaimTxn = txn;
            if (note == "" && box.Entries.All(Done)) note = AllClaimed;
            return true;
        }
    }
}
