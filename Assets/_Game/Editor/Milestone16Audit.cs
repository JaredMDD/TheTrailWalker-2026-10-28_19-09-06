using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Missions;
using TheTrailWalker.Domain.Pets;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TheTrailWalker.Editor
{
    // Milestone 16 (spec 2026-11-25): the limit matrix as deterministic audit cases (fixed seeds, injected clock). Each
    // row ends Verified, Defective or Pending (definition or implementation missing; owner milestone named). Writes
    // Builds/Validation/Milestone16-audit.json and Docs/Testing/milestone-16-audit.md. A Defective row fails the run (a
    // defect against a defined rule is fixed in this milestone); Pending rows do not. Numbers are never tuned here.
    public static class Milestone16Audit
    {
        public const string JsonReport = "Builds/Validation/Milestone16-audit.json";
        public const string MarkdownReport = "Docs/Testing/milestone-16-audit.md";

        [Serializable] public sealed class Row
        {
            public int number; public string item, status, evidence, owner;
            public List<string> checks = new List<string>();
        }
        [Serializable] private sealed class Report { public string build, generatedUtc, mode; public List<Row> rows = new List<Row>(); }

        private static Row current;
        private static void Check(bool ok, string name)
        {
            if (!ok) throw new AuditFailure(name);
            current.checks.Add(name);
        }
        private sealed class AuditFailure : Exception { public AuditFailure(string m) : base(m) { } }

        // chained: true from Milestone5Setup.Prepare, where every earlier suite has already passed in the same run.
        public static List<Row> Run(bool chained = true)
        {
            var rows = new List<Row>();
            void Case(int n, string item, string evidence, Action body, string pendingNote = null, string owner = null)
            {
                current = new Row { number = n, item = item, evidence = evidence, owner = owner };
                try { body(); current.status = pendingNote == null ? "Verified" : "Pending"; if (pendingNote != null) current.checks.Add("PENDING: " + pendingNote); }
                catch (AuditFailure e) { current.status = "Defective"; current.checks.Add("FAILED: " + e.Message); }
                catch (Exception e) { current.status = "Defective"; current.checks.Add("ERROR: " + e.GetType().Name + ": " + e.Message); }
                rows.Add(current);
            }
            Case(1, "Instant material x1000", "New case: a real x1000 craft through CraftingService", MaterialBatch);
            Case(2, "Mastery 1/5/6/10", "New case: CraftStats at the threshold counts", Mastery);
            Case(3, "Timer directions", "New case: CraftStats.Seconds by role, batch and furniture", TimerDirections);
            Case(4, "Hour-scale Wearable/Misc", "New case: a real level 10 craft stretched to two hours, finished by an injected clock (authored timers are test-scale seconds: an observation, not tuned); batch save/load by the chained Home and Milestone 7 suites", HourScale);
            Case(5, "Rejected gear bulk", "New case: every unstackable output refuses batches; a refused craft spends nothing", GearBulk);
            Case(6, "Normal/Elite action and drop differences", "New case: HarvestRules action table and HarvestManifest rolls", NormalElite);
            Case(7, "All nine Elite upgrades", "New case: the authored Elite recipes against the rule", EliteUpgrades,
                "3 of 9 exist (tier 2, debug tools); tier 3-4 Elite recipes need the tier 2-4 tool recipes", "Milestone 17");
            Case(8, "Mission quantities 10/50/100/150", "New case: the tier 1 catalog and MissionState.Deliver", MissionQuantities,
                "tier 1 asks 10 and is verified; 50/100/150 need the tier 2-4 mission catalog", "Milestone 17");
            Case(9, "Mission-versus-upgrade tool ownership", "New case: delivery takes only ready, unreserved bag/pocket/hook units", MissionOwnership);
            Case(10, "+2/+3 boundaries", "New case: HarvestRules.Actions for every family and tier pair", TierReach);
            Case(11, "Trading and regen", chained ? "Chained suites (Milestone 4, 5, 9) passed earlier in this run; TradeModelChecks re-run here" : "TradeModelChecks re-run here", Trading);
            Case(12, "Massive manifests", "New case: a full level 8 home, full bag and full pet queues saved and loaded through SessionSaveStore", MassiveSave);
            Case(13, "Item/currency conservation", "New harness: crafting, mission delivery, pet claims and the save round trip; shops, balloon, warehouse, chests and upgrades by their chained suites (Milestones 12, 14, Home)", Conservation);
            RecorderChecks();
            Write(rows, chained);
            Debug.Log("Milestone 16 audit: " + rows.Count(r => r.status == "Verified") + " verified, " + rows.Count(r => r.status == "Pending") + " pending, " + rows.Count(r => r.status == "Defective") + " defective.");
            Row bad = rows.FirstOrDefault(r => r.status == "Defective");
            if (bad != null) throw new Exception("Milestone 16 audit: row " + bad.number + " (" + bad.item + ") is defective: " + bad.checks.Last());
            return rows;
        }

        // ---------------------------------------------------------------- helpers

        private static readonly DateTime T0 = new DateTime(2026, 11, 25, 9, 0, 0, DateTimeKind.Utc);
        private static int ids;
        private static string NewId() => "audit-" + (++ids);

        private static SpatialInventory BigBag() => new SpatialInventory(60000, 60000);

        // Puts units into the bag as legal stacks (1,000 per stack; one instance per unstackable unit).
        private static void Give(SpatialInventory bag, ItemDefinition def, long units, int quality = 1)
        {
            ItemProfile profile = ItemPrices.Profile(def, quality);
            while (units > 0)
            {
                int q = def.Stackable ? (int)Math.Min(1000, units) : 1;
                ItemEntry e = bag.AddWorld(NewId(), def, q, quality, profile);
                const int token = int.MinValue + 31;
                bool ok = bag.Reserve(e, token) && bag.TryQuickStore(e, token, out _);
                bag.Release(e, token);
                if (!ok || (e.Owner != ItemOwner.Backpack && e.Owner != ItemOwner.Consumed)) throw new InvalidOperationException("Audit fixture: " + def.Id + " did not fit");
                units -= q;
            }
        }
        private static long Held(SpatialInventory bag, string itemId) =>
            bag.Entries.Where(e => e.Definition.Id == itemId && (e.Owner == ItemOwner.Backpack || e.Owner == ItemOwner.Pocket || e.Owner == ItemOwner.Hook || e.Owner == ItemOwner.Overflow)).Sum(e => (long)e.Quantity);
        private static Dictionary<string, long> Totals(SpatialInventory bag) =>
            bag.Entries.Where(e => e.Owner != ItemOwner.World && e.Owner != ItemOwner.Consumed).GroupBy(e => e.Definition.Id).ToDictionary(g => g.Key, g => g.Sum(e => (long)e.Quantity));

        // The first mission of a branch whose request is stackable, active with nothing delivered (catalog 4 form).
        private static MissionState FirstMission(int delivered, out MissionDefinition mission)
        {
            foreach (MissionBranch b in Enum.GetValues(typeof(MissionBranch)))
            {
                MissionDefinition m = MissionBook.Branch(b).First();
                MissionState s = MissionState.Restore(new string[0], m.Id, delivered, 1, MissionBook.CatalogVersion);
                if (Items.Find(s.RequestedFor(m))?.Stackable == true) { mission = m; return s; }
            }
            throw new InvalidOperationException("Audit fixture: no stackable first mission");
        }

        private static void Stock(SpatialInventory bag, RecipeDefinition r, int executions)
        {
            foreach (RecipeInput i in r.Inputs) Give(bag, Items.Find(i.ItemId), (long)i.Quantity * executions);
        }

        // ---------------------------------------------------------------- the cases

        private static void MaterialBatch()
        {
            RecipeDefinition r = RecipeBook.All.First(x => x.Role == RecipeRole.Material && x.BulkAllowed && !x.Retired && x.Inputs.All(i => Items.Find(i.ItemId)?.Stackable == true)
                && CraftStats.Seconds(x, 1, 1) <= 0);
            foreach (int n in RecipeBook.BatchSizes) Check(CraftStats.BatchAllowed(r, n), "batch x" + n + " is allowed for " + r.Id);
            Check(!CraftStats.BatchAllowed(r, 2) && !CraftStats.BatchAllowed(r, 1001) && !CraftStats.BatchAllowed(r, 0), "only x1/x10/x100/x1000 are allowed");
            var bag = BigBag(); var state = new CraftingState(); state.Learn(r.Id);
            Stock(bag, r, 1000);
            var before = Totals(bag);
            CraftingService.Outcome o = CraftingService.Craft(bag, state, r, 1000, T0, NewId);
            Check(o.Success && o.Batch == null && o.Outputs.All(e => e.Ready), r.Id + " x1000 is instant (no batch, outputs ready)");
            Check(Held(bag, r.OutputId) - (before.TryGetValue(r.OutputId, out long had) ? had : 0) == (long)r.Yield * 1000, "x1000 gives yield x 1000 outputs");
            foreach (RecipeInput i in r.Inputs) Check(Held(bag, i.ItemId) == before[i.ItemId] - (long)i.Quantity * 1000 || (i.ItemId == r.OutputId), "x1000 spends exactly " + i.Quantity + " x 1000 of " + i.ItemId);
            Check(o.Outputs.All(e => e.Quantity <= 1000), "outputs are legal stacks of at most 1,000");
            Check(CraftStats.Compute(r, 1_000_000, 1).Level == 1, "material recipes have no mastery (level stays 1)");
        }

        private static void Mastery()
        {
            Check(CraftingMastery.LevelFor(0) == 1 && CraftingMastery.LevelFor(2) == 1 && CraftingMastery.LevelFor(3) == 2, "level 1 until 3 units");
            Check(CraftingMastery.LevelFor(19) == 4 && CraftingMastery.LevelFor(20) == 5, "level 5 at 20 units");
            Check(CraftingMastery.LevelFor(31) == 5 && CraftingMastery.LevelFor(32) == 6, "level 6 at 32 units");
            Check(CraftingMastery.LevelFor(114) == 9 && CraftingMastery.LevelFor(115) == 10 && CraftingMastery.LevelFor(1_000_000) == 10 && CraftingMastery.UnitsToNext(115) == 0, "level 10 at 115 units, capped");
            RecipeDefinition gear = RecipeBook.All.First(x => x.Levelled && !x.Retired && CraftStats.BaseDurability(x) > 0);
            CraftResult l1 = CraftStats.Compute(gear, 0, 1), l5 = CraftStats.Compute(gear, 20, 1), l6 = CraftStats.Compute(gear, 32, 1), l10 = CraftStats.Compute(gear, 115, 1);
            Check(l1.Quality == 1 && l5.Quality == 5 && l6.Quality == 5 && l10.Quality == 5, "quality follows the level and caps at Q5 from level 5 (" + gear.Id + ")");
            Check(l6.UnitValue > l5.UnitValue && l10.UnitValue > l6.UnitValue, "levels 6-10 keep raising the price");
            Check(l6.MaxDurability > l5.MaxDurability && l10.MaxDurability > l6.MaxDurability, "levels 6-10 keep raising durability");
            Check(CraftStats.Compute(gear, 115, 1, 5).Quality == 5 && CraftStats.Compute(gear, 0, 1, 2).Quality == 3, "furniture craft quality adds to the level and never passes Q5");
        }

        private static void TimerDirections()
        {
            int faster = 0, slower = 0;
            foreach (RecipeDefinition r in RecipeBook.All.Where(x => x.Levelled && !x.Retired && x.BaseSeconds > 0))
            {
                double s1 = CraftStats.Seconds(r, 1, 1), s10 = CraftStats.Seconds(r, 10, 1);
                if (r.Role == RecipeRole.Tool || r.Role == RecipeRole.Food) { Check(s10 < s1, r.Id + " (" + r.Role + ") gets faster with mastery"); faster++; }
                else if (r.Role == RecipeRole.Wearable || r.Role == RecipeRole.Misc) { Check(s10 > s1, r.Id + " (" + r.Role + ") gets slower with mastery"); slower++; }
                Check(CraftStats.Compute(r, 50, 1, 5, 0.2).Seconds == CraftStats.Compute(r, 50, 1).Seconds, r.Id + ": furniture never changes the timer");
                if (r.BulkAllowed) { double b10 = CraftStats.Seconds(r, 1, 10); Check(b10 > s1 && b10 < 10 * s1, r.Id + ": a x10 batch takes longer than one but less than ten"); }
            }
            Check(faster > 0 && slower > 0, "both directions are exercised (" + faster + " faster, " + slower + " slower)");
        }

        private static void HourScale()
        {
            RecipeDefinition r = RecipeBook.All.Where(x => x.Levelled && !x.Retired && (x.Role == RecipeRole.Wearable || x.Role == RecipeRole.Misc)
                && x.Inputs.All(i => Items.Find(i.ItemId) != null)).OrderByDescending(x => CraftStats.Seconds(x, 10, 1)).First();
            var bag = BigBag(); var state = new CraftingState(); state.Learn(r.Id);
            // Level 10 (the slowest Wearable/Misc timer): credit 115 units the way crafting does.
            state.CreditMastery(r, 115);
            Stock(bag, r, 1);
            CraftingService.Outcome o = CraftingService.Craft(bag, state, r, 1, T0, NewId);
            Check(o.Success && o.Batch != null, r.Id + " starts a timed craft");
            TimeSpan wait = o.Batch.ReadyAtUtc - T0;
            Check(Math.Abs(wait.TotalSeconds - CraftStats.Seconds(r, 10, 1)) < 1, r.Id + " (the slowest Wearable/Misc timer) at level 10 runs its rule time, " + wait.TotalSeconds.ToString("0.0") + " s");
            // Authored timers are test-scale (seconds); the release hour-scale is a balancing number (not tuned here). The
            // mechanism is driven at hour scale by stretching this real batch to two hours.
            o.Batch.ReadyAtUtc = T0.AddHours(2);

            Check(o.Outputs.All(e => !e.Ready), "the outputs wait while the timer runs");
            Check(CraftingService.Finish(bag, state, o.Batch.ReadyAtUtc.AddSeconds(-1)).Count == 0, "one second early, nothing finishes");
            Check(CraftingService.Finish(bag, state, o.Batch.ReadyAtUtc.AddHours(30)).Count == 1 && o.Outputs.All(e => e.Ready), "a long absence finishes it exactly once");
            Check(CraftingService.Finish(bag, state, o.Batch.ReadyAtUtc.AddHours(60)).Count == 0 && state.Batches.Count == 0, "it never finishes twice");
            Check(state.UnitsOf(r.Id) == 115 + o.Result.OutputUnits, "mastery was credited once, at the start");
        }

        private static void GearBulk()
        {
            var gear = RecipeBook.All.Where(x => !x.Retired && x.Output != null && !x.Output.Stackable).ToList();
            Check(gear.Count > 0, "unstackable outputs exist (" + gear.Count + ")");
            foreach (RecipeDefinition r in gear) Check(CraftStats.BatchAllowed(r, 1) && !CraftStats.BatchAllowed(r, 10) && !CraftStats.BatchAllowed(r, 1000), r.Id + " is crafted one at a time");
            RecipeDefinition pick = gear.First(x => x.Inputs.All(i => Items.Find(i.ItemId)?.Stackable == true));
            var bag = BigBag(); var state = new CraftingState(); state.Learn(pick.Id);
            Stock(bag, pick, 10);
            var before = Totals(bag);
            CraftingService.Outcome o = CraftingService.Craft(bag, state, pick, 10, T0, NewId);
            Check(!o.Success && o.Reason == "Tools, clothing and backpacks are crafted one at a time", "a x10 " + pick.Id + " is refused with the reason");
            Check(Totals(bag).OrderBy(k => k.Key).SequenceEqual(before.OrderBy(k => k.Key)) && state.Batches.Count == 0, "a refused craft spends nothing");
        }

        private static void NormalElite()
        {
            foreach (ToolFamily f in Enum.GetValues(typeof(ToolFamily)))
                for (int tool = 2; tool <= 4; tool++)
                    for (int target = 1; target <= tool + 2; target++)
                    {
                        var n = new ToolSpec(f, tool, false); var e = new ToolSpec(f, tool, true);
                        int an = HarvestRules.Actions(n, target).Value, ae = HarvestRules.Actions(e, target).Value;
                        if (target <= tool) Check(ae <= an && ae >= 1, f + " t" + tool + " vs target t" + target + ": Elite needs no more actions (" + ae + " vs " + an + "), floor 1");
                        else Check(ae == an, f + " t" + tool + " vs target t" + target + ": above its tier Elite and Normal need the same actions");
                        Check(HarvestRules.Boosted(e, target) && !HarvestRules.Boosted(n, target), f + " t" + tool + " vs t" + target + ": only Elite boosts drops, on every eligible target");
                    }
            Check(new ToolSpec(ToolFamily.Axe, 2, true).BaseUses == 2 * new ToolSpec(ToolFamily.Axe, 2, false).BaseUses, "Elite doubles the base uses");
            foreach (InteractableKind kind in Enum.GetValues(typeof(InteractableKind)))
            {
                long normal = 0, boosted = 0; int minN = int.MaxValue, minB = int.MaxValue;
                for (int c = 1; c <= 200; c++)
                {
                    int a = HarvestManifest.Roll(kind, 1, "audit", c, "a", false).Count, b = HarvestManifest.Roll(kind, 1, "audit", c, "a", true).Count;
                    normal += a; boosted += b; minN = Math.Min(minN, a); minB = Math.Min(minB, b);
                }
                Check(boosted > normal && minB >= minN, kind + ": boosted rolls give more bundles (" + boosted + " vs " + normal + " over 200 cycles) and never fewer at minimum");
            }
        }

        private static void EliteUpgrades()
        {
            Check(RecipeBook.EliteUpgrades.Length == 3, "three Elite upgrades are authored today (tier 2)");
            foreach (RecipeDefinition r in RecipeBook.EliteUpgrades)
            {
                Check(HarvestTools.TryGet(r.Output, out ToolSpec outSpec) && outSpec.Elite && outSpec.Tier == 2 && r.Yield == 1 && r.Tier == 2, r.Id + " makes one tier 2 Elite");
                ItemDefinition normal = Items.Find(r.Inputs[0].ItemId);
                Check(HarvestTools.TryGet(normal, out ToolSpec inSpec) && !inSpec.Elite && inSpec.Tier == 2 && inSpec.Family == outSpec.Family && r.Inputs[0].Quantity == 1, r.Id + " consumes one same-family tier 2 Normal");
                Check(r.Inputs.Skip(1).All(i => ItemTiers.Of(i.ItemId) == 1 && Items.Find(i.ItemId)?.Stackable == true), r.Id + " adds previous-tier (tier 1) advanced material");
                Check(!CraftStats.BatchAllowed(r, 10), r.Id + " is x1 only");
            }
        }

        private static void MissionQuantities()
        {
            var tierOne = MissionBook.All.Where(m => m.Tier == 1).ToList();
            Check(tierOne.Count > 0 && tierOne.All(m => m.Quantity == MissionBook.TierOneQuantity) && MissionBook.TierOneQuantity == 10, "every tier 1 mission asks for 10 (" + tierOne.Count + " missions)");
            MissionState state = FirstMission(0, out MissionDefinition mission);
            var bag = BigBag(); Give(bag, Items.Find(state.RequestedFor(mission)), 25);
            int taken = state.Deliver(bag);
            Check(taken == 10 && Held(bag, state.RequestedFor(mission)) == 15 && state.ActiveComplete, "delivery takes only the 10 requested and leaves the rest");
            Check(state.Deliver(bag) == 0 && Held(bag, state.RequestedFor(mission)) == 15, "a complete mission takes nothing more");
        }

        private static void MissionOwnership()
        {
            string item = FirstMission(0, out MissionDefinition mission).RequestedFor(mission);
            ItemDefinition def = Items.Find(item);
            var bag = BigBag();
            Give(bag, def, 4);
            // A pending (not ready) craft output and a reserved stack (an upgrade deposit or a move in progress) are not deliverable.
            ItemEntry pending = bag.AddWorld("pending", def, 6, 1, ItemPrices.Profile(def, 1), ready: false);
            bag.TryStoreCrafted(pending);
            ItemEntry held = bag.AddWorld("held", def, 6, 1, ItemPrices.Profile(def, 1));
            bag.TryStoreCrafted(held);   // its own stack (no merge), then held by a pointer
            const int token = int.MinValue + 33;
            bag.Reserve(held, token);
            Check(held.Owner == ItemOwner.Backpack && pending.Owner == ItemOwner.Backpack && !pending.Ready, "fixture: a reserved stack and a pending stack in the bag");
            MissionState state = FirstMission(0, out _);
            Check(state.Deliver(bag) == 4 && held.Quantity == 6 && pending.Quantity == 6, "delivery takes only ready, unreserved units (4), never the reserved or pending stacks");
            bag.Release(held, token);
            Check(state.Deliver(bag) == 6 && pending.Quantity == 6, "once released, the stack is deliverable; the pending craft still is not");
        }

        private static void TierReach()
        {
            foreach (ToolFamily f in Enum.GetValues(typeof(ToolFamily)))
                foreach (bool elite in new[] { false, true })
                    for (int tool = 1; tool <= 4; tool++)
                    {
                        if (tool == 1 && elite) continue;   // no tier 1 Elite
                        var spec = new ToolSpec(f, tool, elite);
                        for (int target = 1; target <= 4; target++)
                        {
                            int? a = HarvestRules.Actions(spec, target);
                            if (target - tool >= 3) Check(a == null, f + (elite ? " Elite" : " Normal") + " t" + tool + " refuses a t" + target + " target (+" + (target - tool) + ")");
                            else Check(a.HasValue && a.Value >= 1, f + (elite ? " Elite" : " Normal") + " t" + tool + " works on a t" + target + " target");
                        }
                    }
            Check(HarvestRules.Actions(new ToolSpec(ToolFamily.Axe, 1, false), 3) == 24 && HarvestRules.Actions(new ToolSpec(ToolFamily.Axe, 1, false), 4) == null, "the axe at +2 needs 24 actions; +3 is refused");
        }

        private static void Trading()
        {
            List<string> passed = TradeModelChecks.Run();
            Check(passed.Count > 0, "TradeModelChecks pass (" + passed.Count + " checks: wallets, payouts, conveyor, Return at 70%)");
        }

        private static void MassiveSave()
        {
            var home = HomeState.AtLevel(8); var wallet = new Wallet(100_000_000_000L);
            var chest = FurnitureCatalog.Find("furniture.l8.reinforced_oak_chest");
            for (int i = 0; i < 4; i++) home.Buy(chest, wallet, "oak" + i);
            var storage = new HomeStorage(); storage.Sync(home);
            ItemDefinition[] fill = { PrototypeItems.Branch, PrototypeItems.Stone, PrototypeItems.Apples, PrototypeItems.Hemp };
            long inChests = 0;
            for (int i = 0; i < 4; i++)
            {
                SpatialInventory c = storage.Chest("oak" + i);
                for (int k = 0; k < 400; k++)
                {
                    ItemDefinition d = fill[k % fill.Length];
                    ItemProfile profile = ItemPrices.Profile(d, 1 + k % 5);
                    if (!ShopService.StoreFits(c, d, profile, 1000)) break;
                    ItemEntry e = c.AddWorld("oak" + i + "-" + k, d, 1000, 1 + k % 5, profile);
                    const int token = int.MinValue + 35;
                    bool ok = c.Reserve(e, token) && c.TryQuickStore(e, token, out _); c.Release(e, token);
                    if (!ok) throw new InvalidOperationException("Audit fixture: a fitting stack did not store");
                    inChests += 1000;
                }
            }
            var pets = new PetState(); var petWallet = new Wallet(10_000);
            string[] four = { "pet.dog", "pet.cat", "pet.pig", "pet.tortoise" };
            for (int i = 0; i < four.Length; i++)
            {
                // Each absence starts after the previous one settled (the watermark never moves back).
                pets.TryBuy(PetCatalog.Find(four[i]), 8, petWallet, T0, out _); pets.SetActive(four[i]);
                pets.Idle.BeginOffline(four[i], Enumerable.Range(1, 18).ToList(), T0.AddDays(i)); pets.Idle.Settle(T0.AddDays(i).AddHours(9), 77);
            }
            var snapshot = new SessionSnapshot { version = 2, speedCap = 1.5f, survival = new SavedSurvival(), economy = 1, home = SessionSaveStore.SaveHome(home), pets = SessionSaveStore.SavePets(pets) };
            snapshot.home.containers = SessionSaveStore.SaveContainers(storage);
            string dir = Path.Combine("Temp", "Milestone16Save");
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
            var store = new SessionSaveStore(dir, new SystemClock(), 2);
            var watch = Stopwatch.StartNew();
            store.Save(snapshot);
            long saveMs = watch.ElapsedMilliseconds; watch.Restart();
            LoadStatus status = new SessionSaveStore(dir, new SystemClock(), 2).Load(float.MaxValue, out SessionSnapshot back, out string message);
            long loadMs = watch.ElapsedMilliseconds;
            Check(status == LoadStatus.Loaded, "the massive save loads (" + status + ": " + message + ")");
            long size = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Max(f => new FileInfo(f).Length);
            HomeStorage restored = SessionSaveStore.RestoreContainers(back, SessionSaveStore.RestoreHome(back), new HashSet<string>(), new HashSet<string>());
            long backUnits = restored.PieceIds.Select(restored.Chest).Where(c => c != null).Sum(c => c.Entries.Where(e => e.Owner == ItemOwner.Backpack).Sum(e => (long)e.Quantity));
            Check(inChests >= 4 * 50 * 1000, "fixture: four level 8 chests hold " + inChests + " units");
            Check(backUnits == inChests, "every chest unit survives the save (" + backUnits + ")");
            PetState petsBack = SessionSaveStore.RestorePets(back);
            Check(PetCatalog.All.Sum(p => petsBack.Idle.Waiting(p.Id)) == 64 && petsBack.Owned.Count == 4, "four full pet queues (64 finds) survive the save");
            Check(saveMs + loadMs < 2000, "save and reload take " + (saveMs + loadMs) + " ms (< 2,000 ms); file " + (size / 1024) + " KB");
        }

        // Before/after totals for the transaction paths this harness drives: every change equals the rule's amount.
        private static void Conservation()
        {
            // Crafting: inputs leave by quantity x executions, outputs arrive by yield x executions, nothing else moves.
            RecipeDefinition r = RecipeBook.All.First(x => x.Role == RecipeRole.Food && x.Levelled && !x.Retired && x.BulkAllowed && x.Inputs.All(i => Items.Find(i.ItemId)?.Stackable == true));
            var bag = BigBag(); var state = new CraftingState(); state.Learn(r.Id);
            Stock(bag, r, 100);
            Give(bag, PrototypeItems.Stone, 777);
            var before = Totals(bag);
            CraftingService.Outcome o = CraftingService.Craft(bag, state, r, 100, T0, NewId);
            var after = Totals(bag);
            Check(o.Success, "fixture: " + r.Id + " x100 crafts");
            foreach (string id in before.Keys.Union(after.Keys))
            {
                long delta = (after.TryGetValue(id, out long a) ? a : 0) - (before.TryGetValue(id, out long b) ? b : 0);
                long expected = (id == r.OutputId ? (long)r.Yield * 100 : 0) - r.Inputs.Where(i => i.ItemId == id).Sum(i => (long)i.Quantity * 100);
                Check(delta == expected, "crafting moves " + id + " by exactly " + expected + " (moved " + delta + ")");
            }
            // Mission delivery: the bag loses exactly what the mission records as delivered.
            MissionState ms = FirstMission(3, out MissionDefinition mission);
            var mbag = BigBag(); string want = ms.RequestedFor(mission); Give(mbag, Items.Find(want), 50);
            int taken = ms.Deliver(mbag);
            Check(taken == 7 && Held(mbag, want) == 43 && ms.Delivered == 10, "delivery after 3 of 10 takes exactly 7 and records it");
            // Pet claims: each claimed find leaves the queue and arrives whole, with its quality.
            var pets = new PetState(); var w = new Wallet(5000);
            pets.TryBuy(PetCatalog.Find("pet.tortoise"), 3, w, T0, out _); pets.SetActive("pet.tortoise");
            pets.Idle.BeginOffline("pet.tortoise", new[] { 1, 2, 3, 4 }, T0); pets.Idle.Settle(T0.AddHours(3), 5);
            var pbag = BigBag(); var queued = pets.Idle.Queue("pet.tortoise").ToList();
            long queuedUnits = queued.Sum(f => (long)f.Quantity);
            Check(pets.Idle.ClaimAll("pet.tortoise", pbag) == queued.Count && pets.Idle.Waiting("pet.tortoise") == 0, "every queued find is claimed once");
            Check(Totals(pbag).Values.Sum() == queuedUnits && queued.GroupBy(f => f.ItemId).All(g => Held(pbag, g.Key) == g.Sum(f => (long)f.Quantity)), "claims move exactly the queued units by item");
            // Wallet: a refused purchase never moves money; a pet purchase moves exactly its price.
            var wallet = new Wallet(1500); var p2 = new PetState();
            p2.TryBuy(PetCatalog.Find("pet.dog"), 3, wallet, T0, out _);
            Check(wallet.Cash == 500 && !p2.TryBuy(PetCatalog.Find("pet.cat"), 4, wallet, T0, out _) && wallet.Cash == 500, "a purchase moves exactly its price; a refused one moves nothing");
        }

        // Slice 2: the device recorder's statistics and context names (not a matrix row; a failure fails the run).
        private static void RecorderChecks()
        {
            var frames = Enumerable.Range(1, 100).Select(i => (float)i).ToList();
            AuditRecorder.ContextStats st = AuditRecorder.Summarise("trail", frames, new List<float> { 200, 210, 205 }, 12f, 400f, 3);
            if (st.frames != 100 || st.medianMs != 51 || st.p95Ms != 95 || st.worstMs != 100 || st.allocatedMinMb != 200 || st.allocatedMaxMb != 210 || Math.Abs(st.allocatedMeanMb - 205) > 1e-3 || st.gcCount != 3)
                throw new Exception("Milestone 16 recorder: Summarise gives " + JsonUtility.ToJson(st));
            if (AuditRecorder.Summarise("camp", new List<float>(), new List<float>(), 0, 0, 0).frames != 0) throw new Exception("Milestone 16 recorder: an empty context must summarise to zero");
            var names = new[]
            {
                CollectionPrototype.AuditContextName(true, true, CollectionPrototype.HomeView.Backyard, true, true), CollectionPrototype.AuditContextName(false, true, CollectionPrototype.HomeView.Interior, true, false),
                CollectionPrototype.AuditContextName(false, false, CollectionPrototype.HomeView.Backyard, true, false), CollectionPrototype.AuditContextName(false, false, CollectionPrototype.HomeView.BirdsEye, true, false),
                CollectionPrototype.AuditContextName(false, false, CollectionPrototype.HomeView.None, true, false), CollectionPrototype.AuditContextName(false, false, CollectionPrototype.HomeView.None, false, true),
                CollectionPrototype.AuditContextName(false, false, CollectionPrototype.HomeView.None, false, false)
            };
            if (!names.SequenceEqual(new[] { "counters", "sheets", "backyard", "house interior", "settlement street", "camp", "trail" }))
                throw new Exception("Milestone 16 recorder: context names " + string.Join(", ", names));
        }

        // ---------------------------------------------------------------- report

        public static string BuildId() => Build();
        private static string Build()
        {
            try
            {
                string head = File.ReadAllText(".git/HEAD").Trim();
                if (!head.StartsWith("ref: ")) return head.Substring(0, Math.Min(12, head.Length));
                string refPath = Path.Combine(".git", head.Substring(5));
                if (File.Exists(refPath)) return head.Substring(16) + " " + File.ReadAllText(refPath).Trim().Substring(0, 12);
                foreach (string line in File.ReadAllLines(".git/packed-refs")) if (line.EndsWith(" " + head.Substring(5))) return head.Substring(16) + " " + line.Substring(0, 12);
                return head.Substring(16);
            }
            catch (Exception) { return "unknown"; }
        }

        // Testing-phase values the audit noticed; balancing belongs to the release milestones, never to this audit.
        private static readonly string[] Observations =
        {
            "Craft timers are test-scale: the slowest Wearable/Misc recipe runs seconds, not hours, even at level 10. Row 4 drives the mechanism at two hours.",
            "Pet idle finds give 1,000 units per roll in development (PetIdle.DevelopmentUnits); release is 1.",
            "Trail pickups give 1,000 units (MechanicsTest), so instant x1000 material batches are reachable within minutes."
        };

        private static void Write(List<Row> rows, bool chained)
        {
            var report = new Report { build = Build() + " + working tree", generatedUtc = DateTime.UtcNow.ToString("O"), mode = chained ? "full chain" : "standalone", rows = rows };
            Directory.CreateDirectory(Path.GetDirectoryName(JsonReport));
            File.WriteAllText(JsonReport, JsonUtility.ToJson(report, true));
            Directory.CreateDirectory(Path.GetDirectoryName(MarkdownReport));
            var md = new List<string>
            {
                "# Milestone 16 audit report",
                "",
                "Generated " + report.generatedUtc + " from build " + report.build + " (" + report.mode + "), by `Editor/Milestone16Audit.cs`.",
                "Device measurements are a separate section (slice 2). Numbers are never tuned by this audit.",
                "",
                "| # | Matrix item | Status | Evidence | Pending owner |",
                "| --- | --- | --- | --- | --- |"
            };
            foreach (Row r in rows) md.Add("| " + r.number + " | " + r.item + " | **" + r.status + "** | " + r.evidence + " | " + (r.owner ?? "") + " |");
            md.Add("");
            md.Add("## Economy and pacing observations (recorded, not changed)");
            md.Add("");
            foreach (string o in Observations) md.Add("- " + o);
            md.Add("");
            md.Add("## Checks per row");
            foreach (Row r in rows)
            {
                md.Add("");
                md.Add("### " + r.number + ". " + r.item + " (" + r.status + ")");
                md.Add("");
                foreach (string c in r.checks) md.Add("- " + c);
            }
            File.WriteAllText(MarkdownReport, string.Join("\n", md) + "\n");
        }
    }
}
