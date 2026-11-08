using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using ItemCatalog = TheTrailWalker.Domain.Crafting.Items;

namespace TheTrailWalker.Domain.Missions
{
    // Milestone 8 (plan section 11): four independent ordered branches, one active mission globally.
    // Branch-to-clothing roles: Gatherer hats, Lumberjack boots, Hunter shirts, Explorer pants.
    public enum MissionBranch { Gatherer, Lumberjack, Hunter, Explorer }

    // One authored mission: consume an exact item ID (units, or unique instances for tools/gear) and claim a recipe.
    public sealed class MissionDefinition
    {
        public readonly string Id, Code, RequestedItemId, RewardRecipeId, Note;
        public readonly MissionBranch Branch;
        public readonly int Tier, Index, Quantity;
        // Earliest region whose sources make the mission (and its reward) achievable; documentation checked by validation.
        public readonly int EarliestRegion;
        // Mission catalog version that added this mission (1: the original book). Older saves are validated against the
        // order of their own catalog (missions added later left out), then migrated once.
        public readonly int AddedInCatalog;
        // A request changed by a later catalog (E5: Pine branches -> Bone in catalog 3). A mission accepted under an older
        // catalog finishes on its old request (MissionState.RequestedFor); new acceptances use RequestedItemId.
        public readonly string OldRequestedItemId;
        public readonly int RequestChangedInCatalog;
        public MissionDefinition(string id, string code, MissionBranch branch, int tier, int index, string requestedItemId, int quantity, string rewardRecipeId, int earliestRegion, string note = "", int addedInCatalog = 1,
            string oldRequestedItemId = null, int requestChangedInCatalog = 0)
        {
            if (addedInCatalog < 1 || addedInCatalog > MissionBook.CatalogVersion) throw new ArgumentException("Invalid mission catalog " + id);
            if ((oldRequestedItemId == null) != (requestChangedInCatalog == 0) || (requestChangedInCatalog != 0 && (requestChangedInCatalog <= addedInCatalog || requestChangedInCatalog > MissionBook.CatalogVersion)))
                throw new ArgumentException("Invalid request change " + id);
            AddedInCatalog = addedInCatalog; OldRequestedItemId = oldRequestedItemId; RequestChangedInCatalog = requestChangedInCatalog;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(code) || tier < 1 || tier > 4 || index < 1 || string.IsNullOrWhiteSpace(requestedItemId)
                || quantity < 1 || quantity > 150 || string.IsNullOrWhiteSpace(rewardRecipeId) || earliestRegion < 1 || earliestRegion > 24) throw new ArgumentException("Invalid mission " + id);
            Id = id; Code = code; Branch = branch; Tier = tier; Index = index; RequestedItemId = requestedItemId; Quantity = quantity; RewardRecipeId = rewardRecipeId;
            EarliestRegion = earliestRegion; Note = note ?? "";
        }
        public ItemDefinition Requested => ItemCatalog.Find(RequestedItemId);
        public RecipeDefinition Reward => RecipeBook.Find(RewardRecipeId);
        public bool CountsInstances => Requested != null && !Requested.Stackable;
        // The request as the given catalog defined it.
        public string RequestedItemIdIn(int catalog) => RequestChangedInCatalog != 0 && catalog < RequestChangedInCatalog ? OldRequestedItemId : RequestedItemId;
    }

    public static class MissionBook
    {
        // Section 11.4: tier one asks for ten units (or ten tool/gear instances).
        public const int TierOneQuantity = 10;
        // Catalog 2 (user, November 6, 2026): Lumberjack L6 Pine charcoal and L7 Fired bricks (region 3) and L8 Limestone
        // tiles (region 4) run before L5 Mortar, so the first house's materials unlock before the first settlement.
        // Catalog 3 (user, November 7, 2026; plan section 11.6.1.1): the bridge missions G6, H5, H6, E9 and E10 and twenty
        // coverage missions, so every tier-one gatherable item has a collection mission and every material a use. E5 asks for
        // Bone, and E8 (farm Tea herbs) moves to the end of Explorer. Missions that need a settlement listing (ranch dairy, farm
        // Tea herbs) come last in their branch so they never block the rest before settlements exist (Milestone 10).
        // Rule for inserts and moves: missions of older catalogs keep their relative order, so Branch(branch, oldCatalog)
        // still lists each old catalog's exact order and older saves validate unchanged.
        public const int CatalogVersion = 3;
        private static MissionDefinition M(string code, MissionBranch branch, int index, string item, string reward, int region, string note = "", int catalog = 1,
            string oldItem = null, int changedIn = 0) =>
            new MissionDefinition("mission.t1." + code.ToLowerInvariant(), code, branch, 1, index, item, TierOneQuantity, reward, region, note, catalog, oldItem, changedIn);

        // The twelve specified starters (section 11.1), the tier-one extensions (section 11.3), the bridges (section 11.6.1)
        // and the coverage missions (section 11.6.1.1). Execution order is list order within a branch, not the code number;
        // Index is the position shown on the paper.
        public static readonly IReadOnlyList<MissionDefinition> All = new[] {
            M("G1", MissionBranch.Gatherer, 1, "material.pine_branch", "recipe.material.pine_log", 1),
            M("G2", MissionBranch.Gatherer, 2, "food.apples", "recipe.food.apple_juice", 1),
            M("G3", MissionBranch.Gatherer, 3, "item.pine_bird", "recipe.wearable.rabbit_hat", 1, "Needs the Pine bird sculpture recipe from Lumberjack L2"),
            M("G4", MissionBranch.Gatherer, 4, "material.pine_log", "recipe.material.pine_plank", 1),
            M("G7", MissionBranch.Gatherer, 5, "material.pine_cone", "recipe.misc.pinecone_ornament", 2, "Pine cones are found in region 2", 3),
            M("G8", MissionBranch.Gatherer, 6, "ingredient.pine_nuts", "recipe.food.toasted_pine_nuts", 2, "Pine nuts drop when chopping pine trees", 3),
            M("G12", MissionBranch.Gatherer, 7, "food.berries", "recipe.food.berry_oat_cakes", 5, "The cakes also need region-5 Oats", 3),
            M("G5", MissionBranch.Gatherer, 8, "material.reeds", "recipe.material.reed_braid", 6),
            M("G6", MissionBranch.Gatherer, 9, "ingredient.wheat", "recipe.material.wheat_flour", 6, "Pantry flour for later Food", 3),
            M("G9", MissionBranch.Gatherer, 10, "ingredient.rock_salt", "recipe.food.cow_cheese", 5, "Cheese needs ranch Cow milk (settlement)", 3),
            M("G10", MissionBranch.Gatherer, 11, "ingredient.oats", "recipe.food.cow_yogurt", 5, "Yogurt needs ranch Cow milk (settlement)", 3),
            M("G11", MissionBranch.Gatherer, 12, "food.strawberries", "recipe.food.strawberry_cow_yogurt_drink", 5, "The drink needs Cow yogurt (G10)", 3),
            M("G13", MissionBranch.Gatherer, 13, "ingredient.cow_yogurt", "recipe.food.cow_cream", 5, "Cream needs ranch Cow milk (settlement)", 3),
            M("G14", MissionBranch.Gatherer, 14, "ingredient.cow_cream", "recipe.food.cow_butter", 5, "Butter churns from Cow cream (G13)", 3),
            M("G15", MissionBranch.Gatherer, 15, "ingredient.cow_cheese", "recipe.food.cow_cheese_tart", 6, "The tart needs Wheat flour (G6) and ranch Eggs", 3),
            M("L1", MissionBranch.Lumberjack, 1, "material.flint", "recipe.tool.pine_axe_normal", 2),
            M("L2", MissionBranch.Lumberjack, 2, "material.pine_log", "recipe.misc.pine_bird", 1, "Logs from the G1 recipe or from chopping"),
            M("L3", MissionBranch.Lumberjack, 3, "tool.pine_axe.normal", "recipe.wearable.rabbit_boots", 2, "Ten crafted Normal pine axes"),
            M("L4", MissionBranch.Lumberjack, 4, "item.pine_plank", "recipe.material.pine_beam", 2, "The beam also needs the Hemp cord recipe (Explorer E4)"),
            M("L6", MissionBranch.Lumberjack, 5, "material.pine_log", "recipe.material.pine_charcoal", 3, "Charcoal burns from Pine branches; fuel for Fired bricks", 2),
            M("L7", MissionBranch.Lumberjack, 6, "material.clay", "recipe.material.fired_bricks", 3, "Bricks need Pine charcoal (L6)", 2),
            M("L8", MissionBranch.Lumberjack, 7, "material.limestone", "recipe.material.limestone_tiles", 4, "", 2),
            M("L9", MissionBranch.Lumberjack, 8, "material.pebbles", "recipe.material.polished_pebble", 5, "Polishing also needs region-2 Flint", 3),
            M("L10", MissionBranch.Lumberjack, 9, "item.polished_pebble", "recipe.misc.cord_charm", 5, "The charm needs Hemp cord (Explorer E4)", 3),
            M("L5", MissionBranch.Lumberjack, 10, "material.sand", "recipe.material.mortar", 6),
            M("L11", MissionBranch.Lumberjack, 11, "item.pine_plank", "recipe.material.pine_panel", 6, "Panels for the level-two house", 3),
            M("H1", MissionBranch.Hunter, 1, "animal.rabbit.pelt", "recipe.tool.pine_slingshot_normal", 1),
            M("H2", MissionBranch.Hunter, 2, "item.pine_slingshot_normal", "recipe.misc.rabbit_toy", 2, "Ten crafted Normal pine slingshots; the toy needs Hemp fiber"),
            M("H3", MissionBranch.Hunter, 3, "item.rabbit_toy", "recipe.wearable.hemp_shirt", 2),
            M("H4", MissionBranch.Hunter, 4, "animal.rabbit.pelt", "recipe.material.rabbit_leather", 3, "Leather needs Rock salt (region 3)"),
            M("H5", MissionBranch.Hunter, 5, "ingredient.animal_fat", "recipe.material.cured_rabbit_pelt", 6, "Rabbits drop Animal fat; curing softens the fur with it", 3),
            M("H8", MissionBranch.Hunter, 6, "material.nettle_fiber", "recipe.material.plant_stuffing", 6, "Stuffing for the tier-two Beaver toy", 3),
            M("H6", MissionBranch.Hunter, 7, "item.cured_rabbit_pelt", "recipe.material.fine_rabbit_pelt", 6, "Needs Hemp thread (Explorer E9)", 3),
            M("H7", MissionBranch.Hunter, 8, "item.fine_rabbit_pelt", "recipe.material.fine_rabbit_leather", 6, "Needs Nettle thread (Explorer E16)", 3),
            M("E1", MissionBranch.Explorer, 1, "material.hemp", "recipe.material.hemp_cloth", 2),
            M("E2", MissionBranch.Explorer, 2, "animal.rabbit.meat", "recipe.food.roasted_rabbit", 1, "Raw meat comes from hunting (Hunter H1 slingshot)"),
            M("E3", MissionBranch.Explorer, 3, "item.hemp_cloth", "recipe.wearable.hemp_pants", 2),
            M("E4", MissionBranch.Explorer, 4, "material.hemp", "recipe.material.hemp_cord", 2),
            M("E5", MissionBranch.Explorer, 5, "material.bone", "recipe.tool.pine_rod_normal", 2, "Bone comes from hunting rabbits; the rod also needs Hemp cord", 1, "material.pine_branch", 3),
            M("E15", MissionBranch.Explorer, 6, "material.nettle_fiber", "recipe.material.nettle_cloth", 3, "Nettle fiber is found in region 3", 3),
            M("E6", MissionBranch.Explorer, 7, "ingredient.mushrooms", "recipe.food.roasted_mushrooms", 4),
            M("E7", MissionBranch.Explorer, 8, "item.hemp_cloth", "recipe.wearable.starter_backpack", 3, "The backpack needs Rabbit leather (Hunter H4)"),
            M("E11", MissionBranch.Explorer, 9, "fish.rainbow_trout", "recipe.food.trout_mushroom_skillet", 4, "The skillet needs region-4 Mushrooms", 3),
            M("E12", MissionBranch.Explorer, 10, "fish.largemouth_bass", "recipe.food.salt_baked_bass", 4, "Baked in a crust of region-3 Rock salt", 3),
            M("E13", MissionBranch.Explorer, 11, "fish.bluegill", "recipe.food.pine_nut_bluegill", 4, "Crusted with Pine nuts from chopping", 3),
            M("E14", MissionBranch.Explorer, 12, "fish.yellow_perch", "recipe.food.apple_stuffed_perch", 4, "Stuffed with region-1 Apples", 3),
            M("E9", MissionBranch.Explorer, 13, "item.hemp_cord", "recipe.material.hemp_thread", 6, "", 3),
            M("E10", MissionBranch.Explorer, 14, "item.hemp_thread", "recipe.material.fine_hemp_cloth", 6, "", 3),
            M("E16", MissionBranch.Explorer, 15, "item.nettle_cloth", "recipe.material.nettle_thread", 6, "Thread for Fine rabbit leather", 3),
            M("E8", MissionBranch.Explorer, 16, "fish.rainbow_trout", "recipe.food.herb_trout", 4, "Offered once farm Tea herbs are buyable"),
        };
        public static MissionDefinition Find(string id)
        {
            if (id == null) return null;
            foreach (MissionDefinition m in All) if (m.Id == id) return m;
            return null;
        }
        public static IEnumerable<MissionDefinition> Branch(MissionBranch branch) => Branch(branch, CatalogVersion);
        // A branch as an older catalog ordered it: the same list without the missions added after that version.
        public static IEnumerable<MissionDefinition> Branch(MissionBranch branch, int catalog)
        {
            foreach (MissionDefinition m in All) if (m.Branch == branch && m.AddedInCatalog <= catalog) yield return m;
        }
    }

    // What the player can currently reach, for mission eligibility. Region numbers are the discovered ones.
    public sealed class MissionContext
    {
        public Func<int, bool> RegionDiscovered = n => false;
        public Func<string, bool> KnowsRecipe = id => false;
        public Func<ToolFamily, bool> CarriesTool = f => false;
        public bool SettlementReachable; // ranch/farm/fishery listings (Milestone 10+)
    }

    // Exact item sources for tier one (plan section 6.2.2, harvest tables and Tier01Recipes.json "Sources").
    public static class ItemSources
    {
        private static readonly Dictionary<string, int[]> Loose = new Dictionary<string, int[]> {
            { "material.pine_branch", new[] { 1 } }, { "animal.rabbit.pelt", new[] { 1 } }, { "food.apples", new[] { 1 } },
            { "material.hemp", new[] { 2 } }, { "material.flint", new[] { 2 } }, { "material.pine_cone", new[] { 2 } },
            { "material.nettle_fiber", new[] { 3 } }, { "material.clay", new[] { 3 } }, { "ingredient.rock_salt", new[] { 3 } },
            { "material.limestone", new[] { 4 } }, { "food.strawberries", new[] { 4 } }, { "ingredient.mushrooms", new[] { 4 } },
            { "ingredient.oats", new[] { 5 } }, { "material.pebbles", new[] { 5 } }, { "food.berries", new[] { 5 } },
            { "material.reeds", new[] { 6 } }, { "ingredient.wheat", new[] { 6 } }, { "material.sand", new[] { 6 } } };
        private static readonly int[] TierOne = { 1, 2, 3, 4, 5, 6 };
        private static readonly Dictionary<string, (ToolFamily family, int[] regions)> Harvest = new Dictionary<string, (ToolFamily, int[])> {
            { "material.pine_branch", (ToolFamily.Axe, TierOne) }, { "material.pine_log", (ToolFamily.Axe, TierOne) }, { "ingredient.pine_nuts", (ToolFamily.Axe, TierOne) },
            { "animal.rabbit.pelt", (ToolFamily.Hunting, TierOne) }, { "animal.rabbit.meat", (ToolFamily.Hunting, TierOne) },
            { "material.bone", (ToolFamily.Hunting, TierOne) }, { "ingredient.animal_fat", (ToolFamily.Hunting, TierOne) },
            { "fish.rainbow_trout", (ToolFamily.Rod, new[] { 1, 3, 5 }) }, { "fish.largemouth_bass", (ToolFamily.Rod, new[] { 2, 3, 6 }) },
            { "fish.bluegill", (ToolFamily.Rod, new[] { 1, 4, 6 }) }, { "fish.yellow_perch", (ToolFamily.Rod, new[] { 2, 4, 5 }) } };
        // Settlement shop listings (ranch milk/eggs, farm tea herbs after region 4, fishery shrimp): not reachable until settlements exist.
        private static readonly Dictionary<string, int> Settlement = new Dictionary<string, int> {
            { "food.milk.cow", 0 }, { "food.milk.goat", 0 }, { "ingredient.egg", 0 }, { "ingredient.tea_herbs", 4 }, { "ingredient.shrimp", 0 } };

        public static IReadOnlyCollection<string> LooseIds => Loose.Keys;
        // Interaction drops (tree, hunt and fishing outputs) for the coverage check: every gatherable item has a mission.
        public static IReadOnlyCollection<string> HarvestIds => Harvest.Keys;
        public static bool IsKnownSource(string itemId) => Loose.ContainsKey(itemId) || Harvest.ContainsKey(itemId) || Settlement.ContainsKey(itemId);
        public static bool IsHarvestSource(string itemId) => Harvest.ContainsKey(itemId);
        public static int[] LooseRegions(string itemId) => Loose.TryGetValue(itemId, out int[] r) ? r : new int[0];
        public static bool TryHarvest(string itemId, out ToolFamily family, out int[] regions)
        {
            if (Harvest.TryGetValue(itemId, out var h)) { family = h.family; regions = h.regions; return true; }
            family = ToolFamily.Axe; regions = null; return false;
        }

        public static bool Obtainable(string itemId, MissionContext context) => Obtainable(itemId, context, new HashSet<string>());

        private static bool AnyDiscovered(int[] regions, MissionContext context) { foreach (int r in regions) if (context.RegionDiscovered(r)) return true; return false; }

        private static bool Obtainable(string itemId, MissionContext context, HashSet<string> visiting)
        {
            if (itemId == null || !visiting.Add(itemId)) return false;
            try
            {
                if (Loose.TryGetValue(itemId, out int[] regions) && AnyDiscovered(regions, context)) return true;
                if (Harvest.TryGetValue(itemId, out var harvest) && AnyDiscovered(harvest.regions, context) && FamilyAvailable(harvest.family, context, visiting)) return true;
                if (Settlement.TryGetValue(itemId, out int gate) && context.SettlementReachable && (gate == 0 || context.RegionDiscovered(gate))) return true;
                foreach (RecipeDefinition recipe in RecipeBook.All)
                    if (recipe.OutputId == itemId && context.KnowsRecipe(recipe.Id) && InputsObtainable(recipe, context, visiting)) return true;
                return false;
            }
            finally { visiting.Remove(itemId); }
        }
        private static bool InputsObtainable(RecipeDefinition recipe, MissionContext context, HashSet<string> visiting)
        {
            foreach (RecipeInput input in recipe.Inputs) if (!Obtainable(input.ItemId, context, visiting)) return false;
            return true;
        }
        // A harvest family is usable with a carried tool of that family, or a known, craftable recipe for one.
        private static bool FamilyAvailable(ToolFamily family, MissionContext context, HashSet<string> visiting)
        {
            if (context.CarriesTool(family)) return true;
            foreach (RecipeDefinition recipe in RecipeBook.All)
            {
                ItemDefinition output = recipe.Output;
                if (output != null && HarvestTools.TryGet(output, out ToolSpec spec) && spec.Family == family && context.KnowsRecipe(recipe.Id) && InputsObtainable(recipe, context, visiting)) return true;
            }
            return false;
        }

        // Why an item cannot be obtained yet: the first missing link, in player terms.
        public static string Explain(string itemId, MissionContext context)
        {
            ItemDefinition item = ItemCatalog.Find(itemId); string name = item?.Name ?? itemId;
            if (Obtainable(itemId, context)) return name + " is available";
            int[] loose = LooseRegions(itemId);
            if (loose.Length > 0) return $"{name} is found in region {loose[0]}";
            if (Harvest.TryGetValue(itemId, out var harvest))
            {
                if (!AnyDiscovered(harvest.regions, context)) return $"{name} comes from region {harvest.regions[0]}";
                return $"{name} needs a {(harvest.family == ToolFamily.Axe ? "pine axe" : harvest.family == ToolFamily.Hunting ? "slingshot" : "fishing rod")} recipe or tool";
            }
            if (Settlement.ContainsKey(itemId)) return $"{name} is sold at a settlement (later milestone)";
            foreach (RecipeDefinition recipe in RecipeBook.All)
                if (recipe.OutputId == itemId)
                {
                    if (!context.KnowsRecipe(recipe.Id)) return $"Needs the {recipe.Name} recipe";
                    foreach (RecipeInput input in recipe.Inputs) if (!Obtainable(input.ItemId, context)) return Explain(input.ItemId, context);
                }
            return name + " has no source yet";
        }
    }

    public readonly struct MissionOffer
    {
        public readonly MissionBranch Branch; public readonly MissionDefinition Mission; public readonly bool Eligible; public readonly string Reason;
        public MissionOffer(MissionBranch branch, MissionDefinition mission, bool eligible, string reason) { Branch = branch; Mission = mission; Eligible = eligible; Reason = reason; }
    }

    // Persistent mission progress: claimed missions per branch (in order), the single active mission and its delivered count.
    public sealed class MissionState
    {
        private readonly List<string> claimed = new List<string>();
        public IReadOnlyList<string> Claimed => claimed;
        public string ActiveId { get; private set; }
        public int Delivered { get; private set; }
        public MissionDefinition Active => MissionBook.Find(ActiveId);
        public bool ActiveComplete => Active != null && Delivered >= Active.Quantity;
        public int Remaining => Active == null ? 0 : Math.Max(0, Active.Quantity - Delivered);
        public bool IsClaimed(string missionId) => claimed.Contains(missionId);
        // Mission region gate (user, October 31, 2026): missions open by mission region, not by the regions the player has
        // reached. Region 1 missions first; once none of them is left to offer, region 2's open (even before the player gets
        // there), and so on. It only moves forward, so there is always a mission to choose while any remain.
        public int UnlockedRegion { get; private set; } = 1;

        // Each branch keeps its own next mission; switching branches never resets another.
        public MissionDefinition NextFor(MissionBranch branch) => NextFor(branch, MissionBook.CatalogVersion);
        private MissionDefinition NextFor(MissionBranch branch, int catalog)
        {
            foreach (MissionDefinition m in MissionBook.Branch(branch, catalog)) if (!claimed.Contains(m.Id)) return m;
            return null;
        }

        // Catalog migration (November 6, generalised November 7, 2026). Every claim records the mission catalog it was made
        // under and follows that catalog's order; the active mission records the catalog it was accepted under. So a save
        // keeps its claims across catalog changes, its active mission may finish before newly inserted predecessors (e.g. L5
        // while L6-L8 wait) and on its old request (E5 Pine branches), and afterwards each branch continues with its first
        // unclaimed mission in the current order. Nothing is granted and no claim is inferred.
        private readonly List<int> claimCatalogs = new List<int>();
        public IReadOnlyList<int> ClaimCatalogs => claimCatalogs;
        public int ActiveCatalog { get; private set; } = MissionBook.CatalogVersion;
        // Leading claims made under the original catalog (the catalog-2 save field, kept for older readers and checks).
        public int LegacyClaims { get { int n = 0; while (n < claimCatalogs.Count && claimCatalogs[n] == 1) n++; return n; } }
        // The active mission was accepted under an older catalog and the current order would not offer it yet.
        public bool ActiveLegacy => Active != null && ActiveCatalog < MissionBook.CatalogVersion && NextFor(Active.Branch) != Active;
        // What a mission asks for: an active mission accepted under an older catalog keeps that catalog's request.
        public string RequestedFor(MissionDefinition mission) =>
            mission == null ? null : mission.RequestedItemIdIn(mission.Id == ActiveId ? ActiveCatalog : MissionBook.CatalogVersion);
        public ItemDefinition RequestedItemFor(MissionDefinition mission) => ItemCatalog.Find(RequestedFor(mission));

        // Offerable: the branch's next mission, its requested item obtainable now, and its reward recipe craftable once
        // learned. Otherwise it shows as locked with the first missing link (never an active impossible mission).
        public static bool Available(MissionDefinition mission, MissionContext context, out string reason)
        {
            reason = "";
            if (mission.Requested == null || mission.Reward == null) { reason = "Mission data is incomplete"; return false; }
            if (!ItemSources.Obtainable(mission.RequestedItemId, context)) { reason = ItemSources.Explain(mission.RequestedItemId, context); return false; }
            var withReward = new MissionContext { RegionDiscovered = context.RegionDiscovered, CarriesTool = context.CarriesTool, SettlementReachable = context.SettlementReachable,
                KnowsRecipe = id => id == mission.RewardRecipeId || context.KnowsRecipe(id) };
            foreach (RecipeInput input in mission.Reward.Inputs)
                if (!ItemSources.Obtainable(input.ItemId, withReward)) { reason = "Reward needs: " + ItemSources.Explain(input.ItemId, withReward); return false; }
            return true;
        }

        // Sources count as reachable up to the unlocked mission region; recipe, tool and settlement links still apply.
        private MissionContext Gated(MissionContext context) => new MissionContext {
            RegionDiscovered = n => n >= 1 && n <= UnlockedRegion, KnowsRecipe = context.KnowsRecipe, CarriesTool = context.CarriesTool, SettlementReachable = context.SettlementReachable };

        // G1 is the first mission (user, October 31, 2026): until it is claimed, the papers offer only Gatherer G1, so the
        // opening always runs G1 -> G2 -> apple juice before the first food leg. A save that already holds another active
        // or claimed mission keeps it; only new offers wait for G1.
        public const string FirstMissionCode = "G1";
        public bool FirstMissionClaimed => claimed.Contains("mission.t1." + FirstMissionCode.ToLowerInvariant());

        public bool Offerable(MissionDefinition mission, MissionContext context, out string reason)
        {
            if (mission.Code != FirstMissionCode && !FirstMissionClaimed) { reason = "Opens after the first mission (G1 Pine branches)"; return false; }
            if (mission.EarliestRegion > UnlockedRegion) { reason = $"Opens after the region {UnlockedRegion} missions"; return false; }
            return Available(mission, Gated(context), out reason);
        }

        private IReadOnlyList<MissionOffer> OffersAtCurrentRegion(MissionContext context)
        {
            var offers = new List<MissionOffer>();
            foreach (MissionBranch branch in (MissionBranch[])Enum.GetValues(typeof(MissionBranch)))
            {
                MissionDefinition next = NextFor(branch);
                if (next == null) { offers.Add(new MissionOffer(branch, null, false, "All tier-one missions done")); continue; }
                bool eligible = Offerable(next, context, out string reason);
                offers.Add(new MissionOffer(branch, next, eligible, reason));
            }
            return offers;
        }

        // Opens the next mission region whenever nothing is offerable in the current one (and later missions exist).
        public bool AdvanceRegion(MissionContext context)
        {
            bool advanced = false;
            while (true)
            {
                bool any = false; foreach (MissionOffer o in OffersAtCurrentRegion(context)) any |= o.Eligible;
                bool later = false; foreach (MissionDefinition m in MissionBook.All) if (!claimed.Contains(m.Id) && m.EarliestRegion > UnlockedRegion) later = true;
                if (any || !later || UnlockedRegion >= 24) return advanced;
                UnlockedRegion++; advanced = true;
            }
        }

        public IReadOnlyList<MissionOffer> Offers(MissionContext context)
        {
            if (ActiveId == null) AdvanceRegion(context);
            return OffersAtCurrentRegion(context);
        }

        public bool AnyOffer(MissionContext context) { foreach (MissionOffer o in Offers(context)) if (o.Eligible) return true; return false; }

        // Section 12.1: the mission lady stands at the camp for a claimable completion, or for the next selection while no
        // mission is active (the first selection at the starting camp, and after a claim) and something is offerable.
        public bool LadyPresent(MissionContext context) => ActiveComplete || (ActiveId == null && AnyOffer(context));

        public bool CanAccept(string missionId, MissionContext context, out string reason)
        {
            MissionDefinition mission = MissionBook.Find(missionId);
            if (mission == null) { reason = "Unknown mission"; return false; }
            if (ActiveId != null) { reason = ActiveComplete ? "Claim the completed mission first" : "Only one mission can be active"; return false; }
            if (NextFor(mission.Branch) != mission) { reason = "Not this branch's next mission"; return false; }
            AdvanceRegion(context);
            return Offerable(mission, context, out reason);
        }
        public void Accept(string missionId, MissionContext context)
        {
            if (!CanAccept(missionId, context, out string reason)) throw new InvalidOperationException(reason);
            ActiveId = missionId; Delivered = 0; ActiveCatalog = MissionBook.CatalogVersion;
        }

        // Automatic delivery from carried items (on acceptance and after any acquisition or transfer). Returns units taken.
        public int Deliver(SpatialInventory ledger)
        {
            if (ledger == null || Active == null || ActiveComplete) return 0;
            int taken = ledger.ConsumeForMission(RequestedFor(Active), Remaining);
            Delivered += taken;
            return taken;
        }

        // Claim at a camp: the reward recipe is granted once (the caller learns it); progress is monotonic.
        public MissionDefinition Claim()
        {
            if (!ActiveComplete) throw new InvalidOperationException("No completed mission to claim");
            MissionDefinition done = Active;
            claimed.Add(done.Id); claimCatalogs.Add(ActiveCatalog);
            ActiveId = null; Delivered = 0; ActiveCatalog = MissionBook.CatalogVersion;
            return done;
        }

        // catalog: the save's mission catalog version (0 or 1 in older saves: the original book, all claims legacy).
        // Catalog-2 saves describe migration with legacyClaims (leading claims made under catalog 1) and activeLegacy (the
        // active mission was accepted under catalog 1). From catalog 3 the save stores claimCatalogs (one catalog per claim)
        // and activeCatalog; when present they take precedence.
        public static MissionState Restore(IEnumerable<string> claimedIds, string activeId, int delivered, int unlockedRegion = 1, int catalog = 0, int legacyClaims = 0, bool activeLegacy = false,
            IList<int> claimCatalogs = null, int activeCatalog = 0)
        {
            var state = new MissionState();
            if (claimedIds == null || unlockedRegion < 0 || unlockedRegion > 24 || catalog < 0 || catalog > MissionBook.CatalogVersion) throw new ArgumentException("Invalid missions");
            state.UnlockedRegion = Math.Max(1, unlockedRegion); // older saves without the field start at region 1 and advance as needed
            var ids = new List<string>(claimedIds);
            int saved = Math.Max(1, catalog);
            var catalogs = new List<int>();
            if (claimCatalogs != null && (claimCatalogs.Count > 0 || activeCatalog > 0))
            {
                // Catalog-3 form: one catalog per claim, never decreasing (claims are made one at a time, in order), and the
                // active mission accepted no earlier than the last claim.
                if (catalog < 3 || claimCatalogs.Count != ids.Count) throw new ArgumentException("Invalid mission migration");
                for (int i = 0; i < claimCatalogs.Count; i++)
                    if (claimCatalogs[i] < 1 || claimCatalogs[i] > saved || (i > 0 && claimCatalogs[i] < claimCatalogs[i - 1])) throw new ArgumentException("Invalid mission migration");
                catalogs.AddRange(claimCatalogs);
                int last = catalogs.Count > 0 ? catalogs[catalogs.Count - 1] : 1;
                if (activeId == null ? activeCatalog != 0 : activeCatalog < last || activeCatalog > saved) throw new ArgumentException("Invalid mission migration");
            }
            else
            {
                // Catalog 0-2 form: the first legacyClaims claims were made under catalog 1, the rest under the saved catalog.
                bool original = saved == 1;
                int legacy = original ? ids.Count : legacyClaims;
                if (legacy < 0 || legacy > ids.Count || (activeLegacy && (original || activeId == null || legacy != ids.Count))) throw new ArgumentException("Invalid mission migration");
                for (int i = 0; i < ids.Count; i++) catalogs.Add(i < legacy ? 1 : saved);
                activeCatalog = activeId == null ? 0 : original || activeLegacy ? 1 : saved;
            }
            for (int i = 0; i < ids.Count; i++)
            {
                MissionDefinition mission = MissionBook.Find(ids[i]);
                // Each claim follows its own catalog's branch order: a claimed mission was its branch's next one at that point.
                if (mission == null || state.NextFor(mission.Branch, catalogs[i]) != mission) throw new ArgumentException("Invalid claimed mission " + ids[i]);
                state.claimed.Add(ids[i]); state.claimCatalogs.Add(catalogs[i]);
            }
            if (activeId != null)
            {
                MissionDefinition active = MissionBook.Find(activeId);
                if (active == null || state.NextFor(active.Branch, activeCatalog) != active || delivered < 0 || delivered > active.Quantity) throw new ArgumentException("Invalid active mission");
                state.ActiveId = activeId; state.Delivered = delivered; state.ActiveCatalog = activeCatalog;
            }
            else if (delivered != 0) throw new ArgumentException("Delivered units without an active mission");
            return state;
        }
    }
}
