using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Missions;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Milestone 8: four mission branches, tier-one missions, automatic delivery, claims, eligibility and featured sources
    // (plan sections 6.2.2, 11 and 12.1).
    public static class Milestone8Validation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool value, string name) { if (!value) throw new Exception("Milestone 8: " + name); passed.Add(name); }

        private static SpatialInventory Bag(params (ItemDefinition item, int quantity, long value)[] stacks)
        {
            var bag = new SpatialInventory(20000, 6000); bag.Reconfigure(20000, 6000, 2, 2, out _); int n = 0;
            foreach (var (item, quantity, value) in stacks)
            {
                ItemEntry e = bag.AddWorld("m" + n++, item, quantity, 1, item.Stackable ? new ItemProfile(value) : new ItemProfile(value, 10, 10));
                bag.Reserve(e, 1); bag.TryQuickStore(e, 1, out _); bag.Release(e, 1);
            }
            return bag;
        }
        private static MissionContext Context(int regions, Func<string, bool> knows = null, Func<ToolFamily, bool> carries = null) => new MissionContext {
            RegionDiscovered = n => n >= 1 && n <= regions, KnowsRecipe = knows ?? (id => false), CarriesTool = carries ?? (f => false) };
        private static MissionDefinition M(string code) => MissionBook.All.First(m => m.Code == code);
        // Missions whose request or reward needs a settlement listing (ranch dairy and eggs, farm Tea herbs): locked in the
        // prototype world until settlements exist (Milestone 10), and placed last in their branches (catalog 3).
        private static readonly HashSet<string> NeedsSettlement = new HashSet<string> { "G9", "G10", "G11", "G13", "G14", "G15", "E8" };

        // Items 2 and 5 (November 2, 2026): the 24-region data plan, route budgets on the real stamina model, and faint restarts.
        private static void RoutePlanChecks(TravelCatalog catalog)
        {
            // ---- Region plan as data: 24 regions x 3 featured IDs, 18 distinct per tier, 72 overall, camp counts in range.
            var plan = RegionPlan.All;
            Check(plan.Count == 24 && plan.Select((r, i) => r.Number == i + 1 && r.Featured.Count == 3 && r.Featured.All(id => !string.IsNullOrWhiteSpace(id) && id.Contains("."))).All(ok => ok),
                "The region plan lists 24 regions in order, each with three featured loose item IDs");
            Check(Enumerable.Range(1, 4).All(t => plan.Where(r => r.Tier == t).SelectMany(r => r.Featured).Distinct().Count() == 18)
                && plan.SelectMany(r => r.Featured).Distinct().Count() == 72, "Each tier has 18 distinct featured IDs and all four tiers use 72 distinct IDs");
            Check(plan.All(r => r.Camps >= RegionPlan.CampRange(r.Tier).min && r.Camps <= RegionPlan.CampRange(r.Tier).max),
                "Planned camp counts stay inside their tier's confirmed range (10-14, 12-16, 14-18, 16-20)");
            Check(RegionPlan.TierOverlaps.All(o => plan.Where(r => r.Tier == o.Key).SelectMany(r => r.Featured).Contains(o.Value.branch)
                && plan.Where(r => r.Tier == o.Key).SelectMany(r => r.Featured).Contains(o.Value.animal)) && RegionPlan.TierOverlaps[3].animal == "animal.turkey.feathers",
                "Every tier features its branch and pelt-family entry (tier three: turkey feathers, the confirmed shared source)");
            Check(catalog.Regions.All(r => RegionPlan.Find(r.Number) is RegionPlanEntry p && p.Name == r.Name && p.Camps == r.Camps.Count && p.Featured.SequenceEqual(r.LooseItemIds)
                && SegmentLayout.Tier(r) == p.Tier), "Authored regions match the plan: name, camp count, featured items and tier");
            Check(plan.Where(r => r.Tier == 1).All(r => r.Featured.All(id => ItemSources.LooseRegions(id).SequenceEqual(new[] { r.Number })))
                && ItemSources.LooseIds.All(id => plan.Any(r => r.Tier == 1 && r.Featured.Contains(id))), "Tier-one mission sources and the region plan agree exactly");
            var tierOneLoose = plan.Where(r => r.Tier == 1).SelectMany(r => r.Featured).ToList();
            Check(tierOneLoose.Where(id => ItemSources.IsHarvestSource(id)).OrderBy(id => id).SequenceEqual(new[] { "animal.rabbit.pelt", "material.pine_branch" }),
                "Only the branch and pelt overlap between tier-one loose spawns and interaction rewards");

            // ---- Map space (item 6): alternating sides keep camp tabs apart up to the largest planned region (20 camps).
            bool sides = catalog.Regions.All(r => Enumerable.Range(1, r.Camps.Count - 1).All(i => r.Camps[i].Side != r.Camps[i - 1].Side));
            Check(sides && plan.All(r => TheTrailWalker.CollectionPrototype.CampTabsFit(r.Camps)) && plan.Max(r => r.Camps) == 20,
                "Camps alternate road sides, so every planned region (up to 20 camps) fits the map sheet without overlapping tabs or scrolling");

            // ---- Route budgets on the real stamina model (RouteBudget.Simulate, shared with the departure preview).
            const double starterSpeed = 1.5, maximumSpeed = 4.5;
            var juices = Enumerable.Repeat(1.0, 400).ToList();
            var walkable = catalog.Regions.SelectMany(r => r.Camps).Select(c => catalog.Outgoing(c.Id)).Where(s => !s.RiverApproach)
                .Concat(catalog.Regions.Select(r => catalog.LandingInto(r.Id)).Where(s => s != null)).ToList();
            Check(walkable.All(s => { var o = RouteBudget.Simulate(s, 3, 3, 0, juices, starterSpeed); return o.Completed && o.LowestHearts > 0; }),
                "Every authored leg (and ferry landing) is walkable from a full starter meter with one-heart juices, without stopping");
            Check(walkable.Where(s => RouteBudget.Hearts(s) < RouteBudget.StarterHearts).All(s => RouteBudget.Simulate(s, 3, 3, 0, new double[0], starterSpeed).Completed)
                && walkable.Where(s => RouteBudget.Hearts(s) > RouteBudget.StarterHearts).All(s => !RouteBudget.Simulate(s, 3, 3, 0, new double[0], starterSpeed).Completed),
                "Opening legs need no food; every leg above three hearts cannot be finished without food");
            var under = RouteBudget.Simulate(249, RouteBudget.RegionOneDrainPerMetre, 3, 3, 0, new double[0], starterSpeed);
            var over = RouteBudget.Simulate(251, RouteBudget.RegionOneDrainPerMetre, 3, 3, 0, new double[0], starterSpeed);
            Check(under.Completed && Math.Abs(under.FinishHearts - 0.012) < 1e-6 && !over.Completed && Math.Abs(over.WalkedMetres - 250) < 0.01,
                "The simulator charges exactly the authored drain (three starter hearts last 250 m at 12 hearts/km)");
            var ceiling = RouteBudget.Simulate(1300, 0.02, 12, 12, 0, new double[] { 6, 6, 6 }, maximumSpeed);
            Check(ceiling.Completed && ceiling.ServingsEaten == 3 && ceiling.FinishHearts >= 3.9 && ceiling.FinishHearts <= 4.0 + 1e-6,
                "The maximum 26-heart leg (1,300 m at 20 hearts/km) works with twelve hearts plus three six-heart meals, finishing near four hearts");
            Check(!RouteBudget.Simulate(1300, 0.02, 12, 12, 0, new double[] { 6, 6 }, maximumSpeed).Completed, "Two ordinary meals are not enough for the 26-heart leg");
            Check(!RouteBudget.Simulate(1300, 0.02, 12, 6, 0, new double[] { 6, 6, 6 }, maximumSpeed).Completed
                && RouteBudget.Simulate(1300, 0.02, 12, 6, 0, new double[] { 6, 6, 6, 6 }, maximumSpeed).Completed,
                "Starting below full needs more food: six hearts plus three meals fail, four meals finish");
            Check(RouteBudget.Simulate(1300, 0.02, 12, 12, 0, new double[] { 6, 6, 6 }, 1.5).Completed, "Walking speed does not change a leg's food need (distance-only drain)");

            // ---- Faint restart: a reload or restart while fainted keeps the committed loss and never faints again.
            var bag = Bag((PrototypeItems.Branch, 1000, 1), (PrototypeItems.Apples, 990, 1), (PrototypeItems.Flint, 1000, 1));
            var lossPlan = bag.PlanFaintLoss("faint-reload", max => 0);
            bool committed = bag.CommitFaintLoss(lossPlan); int left = bag.Entries.Count;
            var stamina = new TheTrailWalker.Domain.Equipment.StaminaState(); stamina.Restore(0, 3, 0, false, 0); stamina.BeginFaint();
            var reloaded = new TheTrailWalker.Domain.Equipment.StaminaState(); reloaded.Restore(stamina.Hearts, stamina.Maximum, stamina.PendingFood, stamina.Fainted, stamina.FaintElapsed);
            var travel = TravelState.Start(catalog); travel.StandUp(); string rerolled = travel.ResumeAtCampAfterRestart();
            reloaded.Advance(3, 0, true, true);
            Check(committed && !bag.CommitFaintLoss(lossPlan) && bag.Entries.Count == left && reloaded.Fainted && !reloaded.ShouldFaint(false, false)
                && rerolled == catalog.Outgoing(catalog.StartRegion.FirstCamp.Id).Id && Math.Abs(reloaded.Hearts - 1.0) < 1e-6,
                "Restarting while fainted returns to the camp still fainted: the loss is not rolled or charged again and recovery continues");

            // ---- Debug auto sort: tops up stacks to 1,000, keeps every unit, packs without overlap, all or nothing.
            var messy = new SpatialInventory(6000, 4000); int sid = 0;
            ItemEntry Put(ItemDefinition d, int q, int x, int y, bool ready = true)
            {
                ItemEntry e = messy.AddWorld("as" + sid++, d, q, 1, null, ready); messy.Reserve(e, 7); messy.TryPlace(e, 7, x, y, false, out _); messy.Release(e, 7); return e;
            }
            Put(PrototypeItems.Branch, 600, 0, 0); Put(PrototypeItems.Stone, 5, 2500, 300); Put(PrototypeItems.Branch, 700, 0, 2600);
            Put(PrototypeItems.Branch, 300, 4200, 3400); Put(PrototypeItems.Apples, 40, 3000, 2000); ItemEntry raw = Put(PrototypeItems.Apples, 30, 4800, 0, false);
            var before = messy.Entries.Where(e => e.Owner == ItemOwner.Backpack).GroupBy(e => e.Definition.Id).ToDictionary(g => g.Key, g => g.Sum(e => e.Quantity));
            Check(messy.Entries.Count(e => e.Owner == ItemOwner.Backpack) == 6, "Auto sort fixture places six scattered stacks");
            ItemEntry held = messy.Entries.First(); messy.Reserve(held, 9); int revision = messy.Revision;
            Check(!messy.AutoSort(out _) && messy.Revision == revision, "Auto sort refuses while an item is held and changes nothing");
            messy.Release(held, 9);
            bool sorted = messy.AutoSort(out string sortReason);
            var stored = messy.Entries.Where(e => e.Owner == ItemOwner.Backpack).ToList();
            var after = stored.GroupBy(e => e.Definition.Id).ToDictionary(g => g.Key, g => g.Sum(e => e.Quantity));
            bool apart = stored.All(a => stored.All(b => a == b || !SpatialInventory.Overlaps(a.X, a.Y, a.Width, a.Height, b.X, b.Y, b.Width, b.Height)))
                && stored.All(e => e.X >= 0 && e.Y >= 0 && e.X + e.Width <= messy.Width && e.Y + e.Height <= messy.Height);
            var branches = stored.Where(e => e.Definition == PrototypeItems.Branch).Select(e => e.Quantity).OrderByDescending(q => q).ToArray();
            Check(sorted && before.Count == after.Count && before.All(p => after[p.Key] == p.Value) && apart && branches.SequenceEqual(new[] { 1000, 600 })
                && stored.Count(e => e.Definition == PrototypeItems.Apples) == 2 && raw.Owner == ItemOwner.Backpack && !raw.Ready,
                "Auto sort fills branch stacks to 1,000, keeps every unit, leaves unready food apart and packs without overlap (" + sortReason + ")");
            int resorted = messy.Revision; var spots = stored.Select(e => (e.X, e.Y)).ToList();
            Check(messy.AutoSort(out _) && stored.Select(e => (e.X, e.Y)).SequenceEqual(spots), "Auto sort is deterministic: sorting a sorted bag keeps the same layout");
        }

        // Statistics tracking (user, November 2, 2026): counters kept in the save for the possible statistics page.
        private static void StatisticsChecks()
        {
            var stats = new TheTrailWalker.Domain.PlayerStatistics();
            stats.AddDistance(120.5); stats.AddDistance(-5); stats.AddDistance(double.NaN);
            stats.AddGathered("material.pine_branch", 1000); stats.AddGathered("material.pine_branch", 250); stats.AddGathered("food.apples", 0); stats.AddGathered("", 9);
            stats.AddCraft("recipe.food.apple_juice", 1000); stats.AddCraft("recipe.food.apple_juice", 10); stats.AddCraft("recipe.tool.pine_axe", 1);
            stats.AddInteraction("Tree"); stats.AddInteraction("Tree"); stats.AddInteraction("AnimalSpawn");
            stats.AddCash(1000); stats.SpendCash(500); stats.AddGold(1); stats.SpendGold(0); stats.AddFaint();
            Check(System.Math.Abs(stats.DistanceMetres - 120.5) < 1e-9 && stats.GatheredOf("material.pine_branch") == 1250 && stats.TotalGathered == 1250
                && stats.CraftedOf("recipe.food.apple_juice") == 1010 && stats.CraftsOf("recipe.food.apple_juice") == 2 && stats.TotalCrafted == 1011
                && stats.InteractionsOf("Tree") == 2 && stats.InteractionsOf("AnimalSpawn") == 1 && stats.CashEarned == 1000 && stats.CashSpent == 500
                && stats.GoldEarned == 1 && stats.GoldSpent == 0 && stats.Faints == 1,
                "Statistics count walked distance, items gathered per item, units and crafts per recipe, harvests per kind, cash and gold earned and spent, and faints; invalid amounts are ignored");
            var saved = new SavedStatistics
            {
                distanceMetres = stats.DistanceMetres, cashEarned = stats.CashEarned, cashSpent = stats.CashSpent, goldEarned = stats.GoldEarned, goldSpent = stats.GoldSpent, faints = stats.Faints,
                gathered = stats.Gathered.Select(x => new SavedCount { key = x.Key, value = x.Value }).ToList(),
                craftedUnits = stats.CraftedUnits.Select(x => new SavedCount { key = x.Key, value = x.Value }).ToList(),
                crafts = stats.Crafts.Select(x => new SavedCount { key = x.Key, value = x.Value }).ToList(),
                interactions = stats.Interactions.Select(x => new SavedCount { key = x.Key, value = x.Value }).ToList()
            };
            var back = JsonUtility.FromJson<SavedStatistics>(JsonUtility.ToJson(saved));
            var restored = new TheTrailWalker.Domain.PlayerStatistics();
            restored.Restore(back.distanceMetres, back.cashEarned, back.cashSpent, back.goldEarned, back.goldSpent, back.faints,
                back.gathered.Select(x => new KeyValuePair<string, long>(x.key, x.value)), back.craftedUnits.Select(x => new KeyValuePair<string, long>(x.key, x.value)),
                back.crafts.Select(x => new KeyValuePair<string, long>(x.key, x.value)), back.interactions.Select(x => new KeyValuePair<string, long>(x.key, x.value)));
            Check(restored.GatheredOf("material.pine_branch") == 1250 && restored.CraftsOf("recipe.food.apple_juice") == 2 && restored.InteractionsOf("Tree") == 2
                && restored.CashSpent == 500 && System.Math.Abs(restored.DistanceMetres - 120.5) < 1e-9 && restored.Faints == 1,
                "Statistics survive a save round trip");
            var old = JsonUtility.FromJson<SessionSnapshot>("{\"version\":1}");
            Check(old.statistics == null || (old.statistics.gathered.Count == 0 && old.statistics.distanceMetres == 0), "Older saves without statistics load and start from zero");
        }

        public static void Run()
        {
            passed.Clear();
            StatisticsChecks();
            // ---- Data: twelve specified starters plus proposed tier-one extensions.
            var all = MissionBook.All;
            Check(all.Select(m => m.Id).Distinct().Count() == all.Count && all.Select(m => m.Code).Distinct().Count() == all.Count, "Mission IDs and codes are unique");
            Check(all.All(m => m.Requested != null && m.Reward != null && m.Tier == 1 && m.Quantity == 10), "Every mission names an existing item and reward recipe; tier one asks for ten");
            foreach (MissionBranch branch in Enum.GetValues(typeof(MissionBranch)))
            {
                var list = MissionBook.Branch(branch).ToList();
                Check(list.Select((m, i) => m.Index == i + 1).All(x => x), branch + " missions are numbered in order");
            }
            var table = new Dictionary<string, (string item, string reward)> {
                { "G1", ("material.pine_branch", "recipe.material.pine_log") }, { "G2", ("food.apples", "recipe.food.apple_juice") }, { "G3", ("item.pine_bird", "recipe.wearable.rabbit_hat") },
                { "L1", ("material.flint", "recipe.tool.pine_axe_normal") }, { "L2", ("material.pine_log", "recipe.misc.pine_bird") }, { "L3", ("tool.pine_axe.normal", "recipe.wearable.rabbit_boots") },
                { "H1", ("animal.rabbit.pelt", "recipe.tool.pine_slingshot_normal") }, { "H2", ("item.pine_slingshot_normal", "recipe.misc.rabbit_toy") }, { "H3", ("item.rabbit_toy", "recipe.wearable.hemp_shirt") },
                { "E1", ("material.hemp", "recipe.material.hemp_cloth") }, { "E2", ("animal.rabbit.meat", "recipe.food.roasted_rabbit") }, { "E3", ("item.hemp_cloth", "recipe.wearable.hemp_pants") } };
            Check(table.All(t => M(t.Key).RequestedItemId == t.Value.item && M(t.Key).RewardRecipeId == t.Value.reward), "The twelve starter missions match the section 11.1 table");
            Check(M("G3").Reward.Output.GearSlot == Domain.Equipment.GearSlot.Hat && M("L3").Reward.Output.GearSlot == Domain.Equipment.GearSlot.Boots
                && M("H3").Reward.Output.GearSlot == Domain.Equipment.GearSlot.Shirt && M("E3").Reward.Output.GearSlot == Domain.Equipment.GearSlot.Pants,
                "Clothing roles: Gatherer hat, Lumberjack boots, Hunter shirt, Explorer pants");
            Check(M("L3").CountsInstances && M("H2").CountsInstances && !M("G1").CountsInstances, "Tool requests count unique instances; materials count units");

            // ---- Featured sources: three per region, 18 distinct per tier, matching the travel catalog and the recipe data.
            Check(ItemSources.LooseIds.Count == 18 && Enumerable.Range(1, 6).All(r => ItemSources.LooseIds.Count(id => ItemSources.LooseRegions(id).Contains(r)) == 3),
                "Tier one has 18 distinct featured loose IDs, three per region");
            TravelCatalog catalog = TravelCatalog.Prototype();
            Check(catalog.Regions.All(r => r.LooseItemIds.All(id => ItemSources.LooseRegions(id).SequenceEqual(new[] { r.Number }))), "Region spawn tables use exactly their featured items");
            string json = File.ReadAllText("Assets/_Game/Art/Crafting/Tier01/Tier01Recipes.json");
            Check(ItemSources.LooseIds.All(id => json.Contains("\"ItemId\": \"" + id + "\"")), "Featured items agree with the recipe source data");
            Check(RecipeBook.All.Where(r => r.Tier == 1).SelectMany(r => r.Inputs).All(i => ItemSources.IsKnownSource(i.ItemId) || RecipeBook.All.Any(r => r.OutputId == i.ItemId)),
                "Every tier-one recipe input has a source or a recipe");

            // ---- Eligibility at the start: G1 is the first mission, so the very first selection offers only Gatherer G1.
            var state = new MissionState();
            var start = state.Offers(Context(1));
            Check(start.Count == 4 && start[0].Mission.Code == "G1" && start[0].Eligible && start.Skip(1).All(o => !o.Eligible && o.Reason.Contains("G1"))
                && state.UnlockedRegion == 1, "The first selection offers only G1; the other branches open after it");
            Check(!state.CanAccept(M("H1").Id, Context(1), out string beforeG1) && beforeG1.Contains("G1"), "H1 cannot be taken before G1");
            var legacy = MissionState.Restore(new string[0], M("H1").Id, 3);
            Check(legacy.Active.Code == "H1" && legacy.Delivered == 3, "A save with another mission already active keeps it");
            Check(state.LadyPresent(Context(1)), "The mission lady is present for the first selection");
            Check(!state.CanAccept(M("G2").Id, Context(1), out _), "A branch's later mission cannot be taken before its earlier one");

            // ---- G1: accept, automatic delivery of only the remaining ten units, claim once.
            var known = new HashSet<string>();
            Func<string, bool> knows = id => known.Contains(id);
            state.Accept(M("G1").Id, Context(1, knows));
            Check(!state.LadyPresent(Context(1, knows)) && !state.CanAccept(M("H1").Id, Context(1, knows), out string one) && one.Contains("one"), "Only one mission is active; the lady leaves until it is complete");
            var bag = Bag((PrototypeItems.Branch, 1000, 1));
            ItemEntry branches = bag.Entries[0];
            Check(state.Deliver(bag) == 10 && branches.Quantity == 990 && state.ActiveComplete && state.Deliver(bag) == 0 && branches.Quantity == 990,
                "Ten-unit missions take ten from a full stack (990 left) and nothing more once complete");
            Check(state.LadyPresent(Context(1, knows)), "A completed mission brings the lady back to the camp for the hand-off");
            MissionDefinition claimed = state.Claim();
            known.Add(claimed.RewardRecipeId);
            Check(claimed.Code == "G1" && state.ActiveId == null && state.IsClaimed(claimed.Id) && Throws(() => state.Claim()), "The claim grants the reward once");
            Check(state.NextFor(MissionBranch.Gatherer).Code == "G2" && state.Offers(Context(1, knows))[0].Eligible, "Gatherer continues with G2 (apples) in region one");
            var afterG1 = state.Offers(Context(1, knows));
            Check(afterG1[2].Mission.Code == "H1" && afterG1[2].Eligible && !afterG1[1].Eligible && afterG1[1].Reason.Contains("region 1 missions"),
                "After G1, H1 opens by the usual rules and the region-two branches still wait for the region-one missions");

            // ---- Mission region gate: once no region-one mission is left to offer, region two's open without reaching it.
            var gate = new MissionState(); var gateKnown = new HashSet<string>(); Func<string, bool> gateKnows = id => gateKnown.Contains(id);
            foreach (string code in new[] { "G1", "H1", "G2" })
            {
                Check(gate.Offers(Context(1, gateKnows)).Any(o => o.Eligible && o.Mission.Code == code) && gate.UnlockedRegion == 1, code + " is offered in the region-one phase");
                gate.Accept(M(code).Id, Context(1, gateKnows)); gate.Deliver(Supply(M(code))); gateKnown.Add(gate.Claim().RewardRecipeId);
            }
            var opened = gate.Offers(Context(1, gateKnows));
            Check(gate.UnlockedRegion == 2 && opened[1].Eligible && opened[1].Mission.Code == "L1" && opened[3].Eligible && opened[3].Mission.Code == "E1" && gate.LadyPresent(Context(1, gateKnows)),
                "With the region-one missions done, region-two missions (L1 Flint, E1 Hemp) open before region two is reached");
            Check(MissionState.Restore(gate.Claimed, null, 0, gate.UnlockedRegion).UnlockedRegion == 2, "The unlocked mission region is saved");

            // ---- Cross-branch dependencies show locked missions instead of impossible active ones.
            state = MissionState.Restore(new[] { M("G1").Id, M("G2").Id }, null, 0);
            known = new HashSet<string> { M("G1").RewardRecipeId, M("G2").RewardRecipeId };
            var offers = state.Offers(Context(2, knows));
            Check(offers[0].Mission.Code == "G3" && !offers[0].Eligible && offers[0].Reason.Contains("Pine bird"), "G3 stays locked until the Pine bird recipe (L2) is known");
            known.Add(M("L2").RewardRecipeId);
            Check(state.Offers(Context(2, knows))[0].Eligible, "G3 opens once the Pine bird recipe is known");
            Check(MissionState.Available(M("E2"), Context(1), out string meat) == false && meat.Contains("slingshot"), "E2 raw meat needs hunting: locked without a slingshot recipe");
            Check(MissionState.Available(M("E2"), Context(1, id => id == "recipe.tool.pine_slingshot_normal"), out _) && MissionState.Available(M("E2"), Context(1, null, f => f == ToolFamily.Hunting), out _),
                "E2 opens with the slingshot recipe (H1) or a carried slingshot");
            Check(!MissionState.Available(M("H2"), Context(1, id => id == "recipe.tool.pine_slingshot_normal"), out string toy) && toy.Contains("Reward") && MissionState.Available(M("H2"), Context(2, id => id == "recipe.tool.pine_slingshot_normal"), out _),
                "A mission whose reward cannot be crafted yet (toy needs region-two Hemp) stays locked");
            Check(!MissionState.Available(M("H4"), Context(2, id => true), out string salt) && salt.Contains("region 3") && !MissionState.Available(M("E8"), Context(6, id => true, f => true), out string tea) && tea.Contains("settlement"),
                "Region-three Rock salt and settlement Tea herbs keep H4 and E8 locked in the prototype world");

            // ---- Achievable progression: claim whatever is offerable until nothing changes; there is always an offer.
            state = new MissionState(); known = new HashSet<string>();
            for (bool progress = true; progress;)
            {
                progress = false;
                foreach (MissionOffer offer in state.Offers(Context(2, knows)))
                    if (offer.Eligible)
                    {
                        state.Accept(offer.Mission.Id, Context(2, knows));
                        state.Deliver(Supply(offer.Mission));
                        known.Add(state.Claim().RewardRecipeId); progress = true; break;
                    }
            }
            var reachable = new HashSet<string>(state.Claimed.Select(id => MissionBook.Find(id).Code));
            Check(all.Where(m => !NeedsSettlement.Contains(m.Code)).All(m => reachable.Contains(m.Code)) && !reachable.Overlaps(NeedsSettlement),
                "Every tier-one mission can be completed in mission-region order; only the settlement missions (ranch dairy, farm Tea herbs) wait");
            Check(all.All(m => MissionState.Available(m, WithSettlements(Context(m.EarliestRegion, id => true, f => true)), out _)),
                "Each mission's earliest region (with settlements) makes it achievable");

            // ---- Delivery rules: tools by instance, pending and worn items never count, cheapest first.
            var tools = Bag((PrototypeItems.Tool, 1, 300), (PrototypeItems.Tool, 1, 100), (PrototypeItems.Tool, 1, 200));
            Check(tools.ConsumeForMission("tool.pine_axe.normal", 2) == 2 && tools.Entries.Single(e => e.Owner == ItemOwner.Backpack).Profile.UnitValue == 300,
                "Tool missions take finished instances, cheapest quoted value first, and keep the rest");
            ItemDefinition hatDef = Items.Find("item.rabbit_hat");
            var gear = Bag((hatDef, 1, 100), (hatDef, 1, 100));
            ItemEntry worn = gear.Entries[0]; gear.Reserve(worn, 5); bool wore = gear.TryWear(worn, 5, out _); gear.Release(worn, 5);
            Check(wore && gear.ConsumeForMission(hatDef.Id, 2) == 1 && worn.Owner == ItemOwner.Worn, "Worn clothing is protected; a spare counts");
            var pending = new SpatialInventory(); ItemEntry crafted = pending.AddWorld("p", PrototypeItems.Tool, 1, 1, new ItemProfile(100, 10, 10), ready: false);
            pending.TryStoreCrafted(crafted);
            Check(pending.ConsumeForMission("tool.pine_axe.normal", 1) == 0 && pending.MarkReady(crafted) && pending.ConsumeForMission("tool.pine_axe.normal", 1) == 1,
                "A pending craft is ineligible until ready, then counts at once");
            // Competition with crafting: units taken by the mission are gone for an upgrade and vice versa (one shared ledger).
            var shared = Bag((PrototypeItems.Tool, 1, 100), (PrototypeItems.Tool, 1, 100));
            bool crafted1 = shared.TryConsumeIngredients(new[] { new KeyValuePair<string, long>("tool.pine_axe.normal", 1) });
            Check(crafted1 && shared.ConsumeForMission("tool.pine_axe.normal", 10) == 1 && !shared.TryConsumeIngredients(new[] { new KeyValuePair<string, long>("tool.pine_axe.normal", 1) }),
                "A tool used by a craft cannot also count for the mission, and the mission's tools cannot feed a later craft");

            // ---- Route stamina budget and resources in Dawn Plains (user, October 31, 2026).
            RegionDefinition dawn = catalog.Region("region.01");
            var legs = dawn.Camps.Select(c => catalog.Outgoing(c.Id)).Where(s => !s.RiverApproach).ToList();
            Check(legs.Count == 9 && RouteBudget.Hearts(legs[0]) < RouteBudget.StarterHearts && RouteBudget.Hearts(legs[1]) < RouteBudget.StarterHearts,
                "The two opening Dawn Plains legs cost less than the starter's three hearts (no food needed before apple juice)");
            Check(legs.Skip(2).All(s => RouteBudget.Hearts(s) >= RouteBudget.StarterHearts + 0.5) && catalog.Regions.SelectMany(r => r.Camps).All(c => RouteBudget.Hearts(catalog.Outgoing(c.Id)) <= RouteBudget.MaximumLegHearts),
                "From Low Meadow on every leg costs at least 3.5 hearts (food needed); no leg exceeds 26 hearts");
            // ---- Windward Hills (user, November 2, 2026): twelve camps, every leg needs food, costs climb 4.32 -> 6 hearts.
            RegionDefinition hills = catalog.Region("region.02");
            var hillLegs = hills.Camps.Select(c => catalog.Outgoing(c.Id)).Where(s => !s.RiverApproach).ToList();
            bool alternating = true; for (int i = 1; i < hills.Camps.Count; i++) alternating &= hills.Camps[i].Side != hills.Camps[i - 1].Side;
            bool rising = true; for (int i = 1; i < hillLegs.Count; i++) rising &= hillLegs[i].Length > hillLegs[i - 1].Length;
            Check(hills.Camps.Count == 12 && hillLegs.Count == 11 && hills.FinalCamp.Id == "camp.02.12" && catalog.Outgoing(hills.FinalCamp.Id).RiverApproach && alternating,
                "Windward Hills has twelve camps on alternating road sides, and the river follows Far Lookout");
            Check(rising && Math.Abs(RouteBudget.Hearts(hillLegs[0]) - 4.32) < 1e-6 && Math.Abs(RouteBudget.Hearts(hillLegs[10]) - 6.0) < 1e-6
                && hillLegs.All(s => RouteBudget.Hearts(s) > RouteBudget.StarterHearts && RouteBudget.Hearts(s) <= 6.0 && s.Relief > 0),
                "Every Windward Hills leg needs food (4.32 to 6 hearts, rising), within the region 2-6 band, and climbs through the hills");
            RoutePlanChecks(catalog);
            bool everyLeg = true;
            foreach (RegionDefinition r in catalog.Regions)
                foreach (CampDefinition c in r.Camps)
                {
                    SegmentDefinition leg = catalog.Outgoing(c.Id); if (leg.RiverApproach) continue;
                    for (int cycle = 1; cycle <= 40; cycle++) { var roll = LooseDrops.Roll(r, leg, cycle); everyLeg &= r.LooseItemIds.All(id => roll.Any(d => d.ItemId == id)); }
                }
            Check(everyLeg, "Every leg and drop cycle offers all three featured items (apples always on the opening legs)");
            var juiceRecipe = RecipeBook.Find("recipe.food.apple_juice");
            Check(M("G2").Quantity + 3 * juiceRecipe.Inputs.Single(i => i.ItemId == "food.apples").Quantity <= 1000,
                "One apple pickup (1,000) covers the G2 delivery and several apple juices");

            Check(PrototypeItems.Apples.FoodHearts == 1 && !PrototypeItems.Apples.PreparedFood && PrototypeItems.Apples.Category == ItemCategory.RawFood,
                "Apples are raw food: one heart, eaten manually, never auto-eaten at zero stamina");

            // ---- Persistence.
            state = MissionState.Restore(new[] { M("H1").Id, M("G1").Id }, M("G2").Id, 4);
            Check(state.Delivered == 4 && state.Active.Code == "G2" && state.IsClaimed(M("H1").Id), "Mission progress restores");
            Check(Throws(() => MissionState.Restore(new[] { M("G2").Id }, null, 0)) && Throws(() => MissionState.Restore(new string[0], M("G2").Id, 0))
                && Throws(() => MissionState.Restore(new string[0], M("G1").Id, 11)) && Throws(() => MissionState.Restore(new[] { "mission.nope" }, null, 0)),
                "Out-of-order claims, a non-next active mission or over-delivery are rejected");
            LumberjackOrderChecks();
            CoverageCatalogChecks();
            var bad = new SessionSnapshot { version = 1, missions = new SavedMissions { claimed = new List<string> { M("G3").Id } } };
            bool rejected; try { SessionSaveStore.Validate(bad, float.MaxValue, out _); rejected = false; } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "Invalid mission progress makes the save invalid");

            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Milestone8-checks.txt", passed);
            Debug.Log($"[Milestone8] PASS: {passed.Count} mission checks.");
        }

        // Catalog 2 (user, November 6, 2026): Lumberjack L6 charcoal, L7 bricks (region 3) and L8 tiles (region 4) run before
        // L5 Mortar, and saves from the original catalog migrate once without losing or inventing claims.
        private static void LumberjackOrderChecks()
        {
            string[] order = MissionBook.Branch(MissionBranch.Lumberjack).Select(m => m.Code).ToArray();
            Check(order.SequenceEqual(new[] { "L1", "L2", "L3", "L4", "L6", "L7", "L8", "L9", "L10", "L5", "L11" })
                && MissionBook.Branch(MissionBranch.Lumberjack, 2).Select(m => m.Code).SequenceEqual(new[] { "L1", "L2", "L3", "L4", "L6", "L7", "L8", "L5" })
                && MissionBook.Branch(MissionBranch.Lumberjack, 1).Select(m => m.Code).SequenceEqual(new[] { "L1", "L2", "L3", "L4", "L5" }),
                "Lumberjack runs L1-L4, L6, L7, L8, L9, L10, L5, L11 (catalog 2 keeps L1-L4, L6-L8, L5; the original keeps L1-L5)");
            Check(M("L6").RequestedItemId == "material.pine_log" && M("L6").RewardRecipeId == "recipe.material.pine_charcoal" && M("L6").EarliestRegion == 3
                && M("L7").RequestedItemId == "material.clay" && M("L7").RewardRecipeId == "recipe.material.fired_bricks" && M("L7").EarliestRegion == 3
                && M("L8").RequestedItemId == "material.limestone" && M("L8").RewardRecipeId == "recipe.material.limestone_tiles" && M("L8").EarliestRegion == 4
                && M("L5").EarliestRegion == 6, "L6 charcoal and L7 bricks are mission-region 3, L8 tiles region 4, L5 Mortar stays region 6");
            Check(MissionBook.All.Where(m => !NeedsSettlement.Contains(m.Code)).All(m => MissionState.Available(m, Context(m.EarliestRegion, id => true, f => true), out _))
                && !MissionState.Available(M("L7"), Context(2, id => true, f => true), out string clay) && clay.Contains("region 3"),
                "The new missions are achievable from their mission region and not before (Clay is region 3)");

            // First home: planks (G4), charcoal (L6) and bricks (L7) are all learned by the region-four missions, with no grants.
            var state = new MissionState(); var known = new HashSet<string>(); Func<string, bool> knows = id => known.Contains(id);
            var regionAtClaim = new Dictionary<string, int>();
            for (bool progress = true; progress;)
            {
                progress = false;
                foreach (MissionOffer offer in state.Offers(Context(6, knows)))
                    if (offer.Eligible)
                    {
                        state.Accept(offer.Mission.Id, Context(6, knows)); state.Deliver(Supply(offer.Mission));
                        MissionDefinition done = state.Claim(); known.Add(done.RewardRecipeId); regionAtClaim[done.Code] = state.UnlockedRegion; progress = true; break;
                    }
            }
            Check(new[] { "G4", "L6", "L7", "L8" }.All(c => regionAtClaim.TryGetValue(c, out int r) && r <= 4) && regionAtClaim["L5"] == 6
                && state.Claimed.ToList().IndexOf(M("L6").Id) < state.Claimed.ToList().IndexOf(M("L7").Id),
                "Pine planks, Pine charcoal and Fired bricks (and Limestone tiles) are learned by the region-four missions; L6 comes before L7");

            // Migration from the original catalog (no catalog field: 0).
            string[] l1to4 = { M("L1").Id, M("L2").Id, M("L3").Id, M("L4").Id };
            var done5 = MissionState.Restore(l1to4.Concat(new[] { M("L5").Id }), null, 0, 6);
            Check(done5.LegacyClaims == 5 && done5.NextFor(MissionBranch.Lumberjack).Code == "L6" && done5.IsClaimed(M("L5").Id) && !done5.IsClaimed(M("L6").Id),
                "An original-catalog save with L5 claimed keeps it and continues with L6 (nothing granted)");
            var again = MissionState.Restore(done5.Claimed, null, 0, 6, MissionBook.CatalogVersion, done5.LegacyClaims);
            Check(again.NextFor(MissionBranch.Lumberjack).Code == "L6", "A migrated save round-trips with the current catalog");
            Check(Throws(() => MissionState.Restore(done5.Claimed, null, 0, 6, MissionBook.CatalogVersion, 0)),
                "A current-catalog save cannot claim L5 before L6-L8 unless those claims were made under the original order");

            var active5 = MissionState.Restore(l1to4, M("L5").Id, 4, 6);
            Check(active5.ActiveLegacy && active5.Active.Code == "L5" && active5.Delivered == 4 && active5.NextFor(MissionBranch.Lumberjack).Code == "L6",
                "An original-catalog save with L5 active keeps it and its delivered units");
            var savedActive = MissionState.Restore(active5.Claimed, active5.ActiveId, active5.Delivered, 6, MissionBook.CatalogVersion, active5.LegacyClaims, active5.ActiveLegacy);
            Check(savedActive.ActiveLegacy && savedActive.Active.Code == "L5", "The legacy active mission survives a save with the current catalog");
            savedActive.Deliver(Supply(M("L5"))); MissionDefinition finished = savedActive.Claim();
            Check(finished.Code == "L5" && !savedActive.ActiveLegacy && savedActive.LegacyClaims == 5 && savedActive.NextFor(MissionBranch.Lumberjack).Code == "L6"
                && MissionState.Restore(savedActive.Claimed, null, 0, 6, MissionBook.CatalogVersion, savedActive.LegacyClaims).NextFor(MissionBranch.Lumberjack).Code == "L6",
                "The legacy L5 finishes before the inserted L6-L8, then Lumberjack continues with L6 and the save stays valid");
            var active4 = MissionState.Restore(new[] { M("L1").Id, M("L2").Id, M("L3").Id }, M("L4").Id, 2, 2);
            Check(!active4.ActiveLegacy && active4.Active.Code == "L4", "An active mission whose place did not change migrates as an ordinary one");

            Check(Throws(() => MissionState.Restore(l1to4.Concat(new[] { M("L6").Id }), null, 0, 3))
                && Throws(() => MissionState.Restore(l1to4, M("L5").Id, 0, 6, MissionBook.CatalogVersion))
                && Throws(() => MissionState.Restore(new[] { M("L1").Id }, M("L2").Id, 0, 1, MissionBook.CatalogVersion, 0, true))
                && Throws(() => MissionState.Restore(l1to4, null, 0, 6, MissionBook.CatalogVersion, 5))
                && Throws(() => MissionState.Restore(l1to4, null, 0, 6, MissionBook.CatalogVersion + 1)),
                "Rejected: inserted missions in an original-catalog save, a current-catalog L5 without migration, a forged legacy flag or count, an unknown catalog");

            var oldJson = JsonUtility.FromJson<SessionSnapshot>("{\"version\":1,\"missions\":{\"claimed\":[\"" + string.Join("\",\"", l1to4) + "\"],\"active\":\"" + M("L5").Id + "\",\"delivered\":4,\"region\":6}}");
            SavedMissions o = oldJson.missions;
            bool loads = !Throws(() => MissionState.Restore(o.claimed, o.active, o.delivered, o.region, o.catalog, o.legacyClaims, o.activeLegacy));
            Check(loads && o.catalog == 0 && o.legacyClaims == 0 && !o.activeLegacy, "Saved missions from before the catalog field load (L5 active, L6-L8 not yet claimed)");
        }

        // Catalog 3 (user, November 7, 2026; plan section 11.6.1.1): coverage missions and their migration. Fixtures use save
        // shapes like the device tests: a region-two round in progress and the November 6 phone save.
        private static void CoverageCatalogChecks()
        {
            string[] Codes(MissionBranch b, int catalog) => MissionBook.Branch(b, catalog).Select(m => m.Code).ToArray();
            Check(Codes(MissionBranch.Gatherer, 3).SequenceEqual(new[] { "G1", "G2", "G3", "G4", "G7", "G8", "G12", "G5", "G6", "G9", "G10", "G11", "G13", "G14", "G15" })
                && Codes(MissionBranch.Hunter, 3).SequenceEqual(new[] { "H1", "H2", "H3", "H4", "H5", "H8", "H6", "H7" })
                && Codes(MissionBranch.Explorer, 3).SequenceEqual(new[] { "E1", "E2", "E3", "E4", "E5", "E15", "E6", "E7", "E11", "E12", "E13", "E14", "E9", "E10", "E16", "E8" })
                && MissionBook.All.Count == 50, "Catalog 3 has 50 tier-one missions in the section 11.6.1.1 branch order");
            Check(new[] { 1, 2 }.All(c => Codes(MissionBranch.Gatherer, c).SequenceEqual(new[] { "G1", "G2", "G3", "G4", "G5" })
                    && Codes(MissionBranch.Hunter, c).SequenceEqual(new[] { "H1", "H2", "H3", "H4" })
                    && Codes(MissionBranch.Explorer, c).SequenceEqual(new[] { "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8" })),
                "Older catalogs keep their exact order (inserts and the E8 move never reorder older missions)");
            Check(Enum.GetValues(typeof(MissionBranch)).Cast<MissionBranch>().All(b => MissionBook.Branch(b).SkipWhile(m => !NeedsSettlement.Contains(m.Code)).All(m => NeedsSettlement.Contains(m.Code))),
                "Settlement-dependent missions (ranch dairy, farm Tea herbs) come last in their branches, so they never block the rest");

            // Coverage: every gatherable item has a collection mission; every reward unique and active; recipes placed.
            var requested = new HashSet<string>(MissionBook.All.Select(m => m.RequestedItemId));
            var gatherable = ItemSources.LooseIds.Concat(ItemSources.HarvestIds).Distinct().ToList();
            Check(gatherable.Count == 27 && gatherable.All(requested.Contains), "Every tier-one loose resource and interaction drop (logs, nuts, meat, bone, fat, four fish) has a collection mission");
            Check(MissionBook.All.Select(m => m.RewardRecipeId).Distinct().Count() == MissionBook.All.Count && MissionBook.All.All(m => !m.Reward.Retired),
                "No reward repeats and no mission teaches a retired recipe");
            var rewards = new HashSet<string>(MissionBook.All.Select(m => m.RewardRecipeId));
            var unplaced = Tier1Recipes.All.Where(r => !r.Retired && !rewards.Contains(r.Id)).Select(r => r.Id).OrderBy(x => x).ToArray();
            Check(unplaced.SequenceEqual(new[] { "recipe.food.apple_oat_compote", "recipe.food.goat_cheese", "recipe.food.mixed_milk_cheese", "recipe.food.premium_apple_pie",
                    "recipe.food.red_berry_oat_porridge", "recipe.food.shrimp_pie", "recipe.food.strawberry_glazed_shrimp_skewers", "recipe.material.clay_cup", "recipe.misc.pinecone_rabbit" }),
                "Every active tier-one recipe is a mission reward except the box/bakery recipes, Clay cup (tier-four T4-G9) and the two open-decision cheeses");
            // Every tier-one Material has a use: a recipe input in code, or a later use the plan names (house deposits, tier-two recipes).
            var planUses = new HashSet<string> { "item.pine_plank", "item.fired_bricks", "item.pine_beam", "item.nettle_cloth", "item.reed_braid", "item.pine_panel", "item.limestone_tiles",
                "item.mortar", "item.fine_hemp_cloth", "item.fine_rabbit_leather", "item.pine_charcoal", "item.clay_cup", "item.plant_stuffing", "item.fine_rabbit_pelt" };
            var inputs = new HashSet<string>(RecipeBook.All.Where(r => !r.Retired).SelectMany(r => r.Inputs).Select(i => i.ItemId));
            Check(Tier1Recipes.All.Where(r => r.Role == RecipeRole.Material).All(r => inputs.Contains(r.OutputId) || planUses.Contains(r.OutputId))
                && ItemSources.HarvestIds.All(id => inputs.Contains(id)),
                "Every tier-one Material is used by a recipe or a planned house deposit, and every interaction drop (Animal fat and Bone included) is a recipe input");

            // Region two: after G4 the Gatherer offers G7 (Pine cones); G8 (Pine nuts) needs a pine axe recipe or a carried axe.
            var known = new HashSet<string>(); Func<string, bool> knows = id => known.Contains(id);
            var state = new MissionState();
            foreach (string code in new[] { "G1", "H1", "G2", "G3", "G4" })
            {
                if (code == "G3") known.Add(M("L2").RewardRecipeId);
                state.Accept(M(code).Id, Context(2, knows)); state.Deliver(Supply(M(code))); known.Add(state.Claim().RewardRecipeId);
            }
            MissionOffer gatherer = state.Offers(Context(2, knows))[0];
            Check(state.UnlockedRegion == 2 && gatherer.Mission.Code == "G7" && gatherer.Eligible, "In region two the Gatherer continues with G7 (Pine cones -> Pine-cone ornament)");
            state.Accept(M("G7").Id, Context(2, knows)); state.Deliver(Supply(M("G7"))); known.Add(state.Claim().RewardRecipeId);
            MissionOffer nuts = state.Offers(Context(2, knows))[0];
            Check(nuts.Mission.Code == "G8" && !nuts.Eligible && nuts.Reason.Contains("pine axe"), "G8 Pine nuts waits for a pine axe recipe or tool");
            known.Add("recipe.tool.pine_axe_normal");
            Check(state.Offers(Context(2, knows))[0].Eligible, "G8 opens once the pine axe recipe is known (Pine nuts drop when chopping)");

            // Migration from catalog 2, shaped like the region-two round in progress: claims through G4, H2, E4 and L2.
            string[] round = new[] { "G1", "H1", "G2", "L1", "E1", "L2", "G3", "G4", "H2", "E2", "E3", "E4" }.Select(c => M(c).Id).ToArray();
            var e5 = MissionState.Restore(round, M("E5").Id, 4, 2, 2);
            Check(e5.ActiveCatalog == 2 && e5.RequestedFor(M("E5")) == "material.pine_branch" && e5.Delivered == 4 && e5.ClaimCatalogs.All(c => c == 2),
                "A catalog-2 save with E5 active keeps it on its old request (Pine branches) and its delivered units");
            var pine = Bag((PrototypeItems.Branch, 1000, 1));
            Check(e5.Deliver(pine) == 6 && e5.ActiveComplete && e5.Deliver(Bag((Items.Find("material.bone"), 1000, 1))) == 0, "The legacy E5 finishes with Pine branches, not Bone");
            e5.Claim();
            Check(e5.ClaimCatalogs.Last() == 2 && e5.NextFor(MissionBranch.Explorer).Code == "E15" && e5.NextFor(MissionBranch.Gatherer).Code == "G7"
                && MissionState.Restore(e5.Claimed, null, 0, 2, 3, e5.LegacyClaims, false, e5.ClaimCatalogs.ToList(), 0).NextFor(MissionBranch.Explorer).Code == "E15",
                "After the legacy E5 claim, Explorer continues with E15, Gatherer with G7, and the catalog-3 save round-trips");
            var fresh = MissionState.Restore(round, null, 0, 2, 2);
            Check(fresh.NextFor(MissionBranch.Explorer).Code == "E5" && fresh.RequestedFor(M("E5")) == "material.bone",
                "Without E5 active, a migrated save offers E5 with the new request (Bone)");
            fresh.Accept(M("E5").Id, Context(2, id => true, f => true));
            Check(fresh.RequestedFor(M("E5")) == "material.bone" && fresh.ActiveCatalog == 3, "E5 accepted after the update asks for Bone");
            var h3 = new[] { "G1", "H1", "G2", "L1", "E1", "L2", "G3", "G4", "H2", "E2", "E3", "E4", "H3" }.Select(c => M(c).Id).ToArray();
            var h4 = MissionState.Restore(h3, M("H4").Id, 3, 3, 2);
            Check(h4.Active.Code == "H4" && !h4.ActiveLegacy && h4.RequestedFor(M("H4")) == "animal.rabbit.pelt", "A catalog-2 save with H4 active migrates as an ordinary active mission");

            // The November 6 phone save shape: L4 claimed, E6 active, mission region 4.
            string[] phone = new[] { "G1", "H1", "G2", "L1", "E1", "L2", "G3", "G4", "H2", "E2", "E3", "E4", "L3", "H3", "E5", "L4" }.Select(c => M(c).Id).ToArray();
            var nov6 = MissionState.Restore(phone, M("E6").Id, 0, 4, 2);
            Check(nov6.Active.Code == "E6" && nov6.ActiveLegacy && nov6.NextFor(MissionBranch.Lumberjack).Code == "L6",
                "The November 6 phone save keeps E6 active although E15 now comes first; Lumberjack continues with L6");
            nov6.Deliver(Supply(M("E6"))); nov6.Claim();
            Check(nov6.NextFor(MissionBranch.Explorer).Code == "E15" && MissionState.Restore(nov6.Claimed, null, 0, 4, 3, 0, false, nov6.ClaimCatalogs.ToList(), 0).IsClaimed(M("E6").Id),
                "After E6, Explorer continues with the inserted E15 and the save stays valid");
            // E8 claimed before it moved to the end of Explorer (catalog 2 order E1-E8).
            string[] early8 = new[] { "G1", "H1", "G2", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8" }.Select(c => M(c).Id).ToArray();
            Check(MissionState.Restore(early8, null, 0, 4, 2).NextFor(MissionBranch.Explorer).Code == "E15", "An E8 claimed under catalog 2 stays claimed; Explorer continues with E15");

            // Forged or inconsistent catalog-3 data is rejected.
            List<int> twos(int n) => Enumerable.Repeat(2, n).ToList();
            Check(Throws(() => MissionState.Restore(round, null, 0, 2, 3, 0, false, twos(round.Length - 1), 0))
                && Throws(() => MissionState.Restore(round, null, 0, 2, 3, 0, false, Enumerable.Repeat(2, round.Length - 1).Concat(new[] { 1 }).ToList(), 0))
                && Throws(() => MissionState.Restore(round, null, 0, 2, 3, 0, false, Enumerable.Repeat(4, round.Length).ToList(), 0))
                && Throws(() => MissionState.Restore(round, null, 0, 2, 3, 0, false, twos(round.Length), 2))
                && Throws(() => MissionState.Restore(round, null, 0, 2, 2, 0, false, twos(round.Length), 0))
                && Throws(() => MissionState.Restore(round, M("E5").Id, 0, 2, 3, 0, false, twos(round.Length), 1))
                && Throws(() => MissionState.Restore(new[] { "G1", "H1", "G2", "G3", "G4", "G7" }.Select(c => M(c).Id), null, 0, 2, 2))
                && Throws(() => MissionState.Restore(new[] { M("G1").Id }, M("G2").Id, 0, 1, 3, 0, false, new List<int> { 3 }, 2)),
                "Rejected: a short, decreasing or future claim list, an active catalog without a mission, the catalog-3 form under catalog 2, an active older than its last claim, a catalog-3 mission claimed under catalog 2");
            var json = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(new SessionSnapshot { version = 3, missions = new SavedMissions {
                claimed = e5.Claimed.ToList(), region = 2, catalog = 3, legacyClaims = e5.LegacyClaims, claimCatalogs = e5.ClaimCatalogs.ToList(), activeCatalog = 0 } }));
            Check(json.missions.claimCatalogs.SequenceEqual(e5.ClaimCatalogs) && !Throws(() => MissionState.Restore(json.missions.claimed, null, 0, json.missions.region, json.missions.catalog,
                json.missions.legacyClaims, json.missions.activeLegacy, json.missions.claimCatalogs, json.missions.activeCatalog)), "Claim catalogs survive the JSON save round trip");
        }

        private static MissionContext WithSettlements(MissionContext context) { context.SettlementReachable = true; return context; }
        // A bag holding exactly what a mission asks for: a full stack, or ten finished instances.
        private static SpatialInventory Supply(MissionDefinition mission)
        {
            ItemDefinition item = mission.Requested;
            if (item.Stackable) return Bag((item, 1000, 1));
            var stacks = new (ItemDefinition, int, long)[mission.Quantity];
            for (int i = 0; i < stacks.Length; i++) stacks[i] = (item, 1, 100);
            return Bag(stacks);
        }
        private static bool Throws(Action action) { try { action(); return false; } catch (Exception) { return true; } }
    }
}
