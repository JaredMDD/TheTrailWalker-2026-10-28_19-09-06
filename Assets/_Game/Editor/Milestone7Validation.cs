using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Persistence;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Milestone 7: recipes, mastery, stats, timers, bulk crafting and the craft transaction (plan sections 6.1, 9, 10).
    public static class Milestone7Validation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool value, string name) { if (!value) throw new Exception("Milestone 7: " + name); passed.Add(name); }

        private static SpatialInventory Bag(params (ItemDefinition item, int quantity, int quality)[] stacks)
        {
            var bag = new SpatialInventory(14000, 4000); int n = 0;
            foreach (var (item, quantity, quality) in stacks)
            {
                ItemEntry e = bag.AddWorld("s" + n++, item, quantity, quality, item.Stackable ? new ItemProfile(1) : new ItemProfile(100, 10, 10));
                bag.Reserve(e, 1); bag.TryQuickStore(e, 1, out _); bag.Release(e, 1);
            }
            return bag;
        }
        private static int ids;
        private static string NewId() => "id" + (++ids);

        public static void Run()
        {
            passed.Clear();
            // Catalog integrity.
            bool resolve = RecipeBook.All.All(r => r.Output != null && r.Inputs.All(i => Items.Find(i.ItemId) != null && i.Quantity > 0));
            Check(Tier1Recipes.All.Length == 63 && RecipeBook.All.Count == 66 && resolve && RecipeBook.All.Select(r => r.Id).Distinct().Count() == 66, "The 63 tier-one recipes (four retired roasts included) and 3 tier-two Elite upgrades resolve every exact input and output item and have unique IDs");
            Check(RecipeBook.All.All(r => r.Inputs.All(i => ItemTiers.InWindow(r, i.ItemId))), "Every recipe's direct inputs respect its tier window (or the Food pantry exception)");
            var birchTest = new RecipeDefinition("t.birch", "t", RecipeRole.Food, 1, "item.apple_juice", 1, 3, "", new[] { new RecipeInput("ingredient.birch_sap", 1) });
            var futureMaterial = new RecipeDefinition("t.m", "t", RecipeRole.Material, 2, "item.pine_plank", 1, 1, "", new[] { new RecipeInput("ingredient.acorns", 1) });
            Check(!ItemTiers.InWindow(birchTest, "ingredient.birch_sap") && !ItemTiers.InWindow(futureMaterial, "ingredient.acorns") && ItemTiers.InWindow(birchTest, "food.milk.cow"),
                "Future-tier inputs are rejected; older pantry staples are allowed in Food only");
            RecipeDefinition log = RecipeBook.Find("recipe.material.pine_log"), plank = RecipeBook.Find("recipe.material.pine_plank"), juice = RecipeBook.Find("recipe.food.apple_juice"),
                axe = RecipeBook.Find("recipe.tool.pine_axe_normal"), hat = RecipeBook.Find("recipe.wearable.rabbit_hat"), bird = RecipeBook.Find("recipe.misc.pine_bird");
            Check(!log.Levelled && CraftStats.Seconds(log, 1, 1000) == 0 && CraftStats.Seconds(plank, 1, 1) == 10 && CraftStats.Seconds(plank, 1, 1) == CraftStats.Seconds(plank, 9, 1),
                "Gatherable-output conversions are instant; crafted materials keep a fixed timer with no mastery");
            Check(RecipeBook.All.Where(r => r.Role == RecipeRole.Tool || r.Role == RecipeRole.Food).All(r => CraftStats.Seconds(r, 1, r.BulkAllowed ? 1000 : 1) < 3600),
                "Every Tool and Food craft stays below one hour, even at x1000");

            // Mastery and stats.
            Check(CraftingMastery.LevelFor(0) == 1 && CraftingMastery.LevelFor(3) == 2 && CraftingMastery.LevelFor(19) == 4 && CraftingMastery.LevelFor(20) == 5 && CraftingMastery.LevelFor(115) == 10 && CraftingMastery.LevelFor(100000) == 10,
                "Mastery levels follow the cumulative finished-unit thresholds up to level ten");
            CraftResult hat1 = CraftStats.Compute(hat, 0, 1), hat5 = CraftStats.Compute(hat, 20, 1), hat10 = CraftStats.Compute(hat, 115, 1);
            Check(hat1.Quality == 1 && hat5.Quality == 5 && hat10.Quality == 5 && hat10.UnitValue > hat5.UnitValue && hat10.MaxDurability > hat5.MaxDurability && hat10.Seconds > hat5.Seconds,
                "Wearables: level five and ten are both Q5; level ten is worth more, lasts longer and takes longer");
            CraftResult juice5 = CraftStats.Compute(juice, 20, 1), juice10 = CraftStats.Compute(juice, 115, 1);
            Check(juice5.Quality == 5 && juice10.Quality == 5 && juice10.UnitValue > juice5.UnitValue && juice10.Seconds < juice5.Seconds && juice10.MaxDurability == 0,
                "Food: level five and ten are both Q5; level ten is worth more and is faster, with no durability");
            CraftResult axe1 = CraftStats.Compute(axe, 0, 1);
            Check(axe1.MaxDurability == 10 && hat1.MaxDurability == 1600 && Math.Abs(CraftStats.Compute(hat, 115, 1).MaxDurability - 1600 * 1.72 * 1.8) < 1,
                "Base durability: pine axe 10 uses; tier-one wearables 1.6 km, L10 Q5 = base x 1.72 x 1.8");
            CraftResult furnished = CraftStats.Compute(hat, 20, 1, 0, 0.2);
            Check(furnished.MaxDurability > hat5.MaxDurability && furnished.UnitValue > hat5.UnitValue && furnished.Seconds == hat5.Seconds && CraftStats.Compute(log, 0, 1, 2).Quality == 3,
                "Furniture raises durability/value (and material quality) but never changes the timer");
            Check(CraftStats.Compute(bird, 0, 1).UnitValue == (long)(600 * 0.8) && CraftStats.Compute(bird, 115, 1).UnitValue == 600 * 2,
                "Misc price: level-one Q1 is 80% of the level-ten reference; level-ten Q5 doubles it");
            Check(CraftStats.Seconds(juice, 1, 1000) > CraftStats.Seconds(juice, 1, 1) && Math.Abs(CraftStats.Seconds(juice, 1, 1000) / CraftStats.Seconds(juice, 1, 1) - (1 + 0.2 * Math.Log(1000, 2))) < 1e-6,
                "Batch factor: 1 + 0.2 log2(executions), applied once");

            // Batch rules.
            Check(CraftStats.BatchAllowed(juice, 1000) && !CraftStats.BatchAllowed(juice, 7) && CraftStats.BatchAllowed(axe, 1) && !CraftStats.BatchAllowed(axe, 10) && !CraftStats.BatchAllowed(hat, 100),
                "Stackables allow x1/x10/x100/x1000; tools, clothing and backpacks are x1 only");
            var branches = Bag((PrototypeItems.Branch, 1000, 1));
            Check(CraftStats.MaxExecutions(log, id => branches.CountIngredient(id)) == 333, "1,000 branches allow at most 333 log conversions (no rounding up)");

            // The craft transaction.
            var state = new CraftingState();
            var wood = Bag((PrototypeItems.Branch, 1000, 3), (PrototypeItems.Branch, 500, 1));
            Check(!CraftingService.Craft(wood, state, log, 100, DateTime.UtcNow, NewId).Success && wood.CountIngredient(PrototypeItems.Branch.Id) == 1500, "An unknown recipe crafts nothing and spends nothing");
            state.Learn(log.Id); state.Learn(plank.Id); state.Learn(juice.Id); state.Learn(axe.Id); state.Learn(hat.Id);
            var instant = CraftingService.Craft(wood, state, log, 100, DateTime.UtcNow, NewId);
            ItemEntry lowLeft = wood.Entries.FirstOrDefault(e => e.Definition == PrototypeItems.Branch && e.Owner == ItemOwner.Backpack && e.Quality == 1);
            Check(instant.Success && instant.Batch == null && instant.Outputs.All(o => o.Ready) && instant.Outputs.Sum(o => o.Quantity) == 100 && wood.CountIngredient(PrototypeItems.Branch.Id) == 1200
                && lowLeft != null && lowLeft.Quantity == 200 && state.UnitsOf(log.Id) == 0,
                "Instant conversion: 300 branches consumed lowest quality first, 100 ready logs, no batch and no mastery");
            Check(!CraftingService.Craft(wood, state, plank, 1000, DateTime.UtcNow, NewId).Success && wood.CountIngredient(PrototypeItems.Log.Id) == 100, "A shortage crafts nothing and spends nothing");

            var t0 = new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc);
            var apples = Bag((PrototypeItems.Apples, 1000, 2), (PrototypeItems.Apples, 1000, 4), (PrototypeItems.Apples, 1000, 1));
            var juiced = CraftingService.Craft(apples, state, juice, 1000, t0, NewId);
            Check(juiced.Success && juiced.Batch != null && juiced.Outputs.Count == 1 && juiced.Outputs[0].Quantity == 1000 && !juiced.Outputs[0].Ready && apples.CountIngredient(PrototypeItems.Apples.Id) == 0,
                "x1000 juice: 3,000 apples consumed atomically; 1,000 pending juice in one legal stack with one shared timer");
            // November 2, 2026 (user): mastery counts at the moment of crafting, so the next craft already uses the new level.
            Check(state.UnitsOf(juice.Id) == 1000 && juiced.Batch.MasteryCredited, "Mastery is counted when the craft starts, while the batch is still on its timer");
            var juiced2 = CraftingService.Craft(Bag((PrototypeItems.Apples, 30, 1)), state, juice, 10, t0, NewId);
            Check(!juiced.Outputs[0].CanMerge(juiced2.Outputs[0]), "Pending outputs never merge, even with another pending batch of the same item");
            Check(juiced2.Result.Level > juiced.Result.Level && state.UnitsOf(juice.Id) == 1010,
                "A second craft started while the first is still pending already uses the level the first one earned");
            Check(CraftingService.Finish(apples, state, t0.AddSeconds(1)).Count == 0 && !juiced.Outputs[0].Ready && state.UnitsOf(juice.Id) == 1010, "Nothing is ready before the timer ends");
            DateTime done = juiced.Batch.ReadyAtUtc;
            var finished = CraftingService.Finish(apples, state, done);
            Check(finished.Count == 2 && juiced.Outputs[0].Ready && state.UnitsOf(juice.Id) == 1010 && state.LevelOf(juice.Id) == 10 && CraftingService.Finish(apples, state, done.AddHours(1)).Count == 0,
                "At readiness the outputs become usable and mastery is not counted a second time (1,010 units -> level ten)");
            // A batch saved before the change (mastery not yet counted) is counted once when it finishes.
            var legacy = new CraftingState(); legacy.Learn(juice.Id);
            legacy.Restore(new[] { juice.Id }, new KeyValuePair<string, long>[0],
                new[] { new CraftBatch { Id = "legacy", RecipeId = juice.Id, OutputUnits = 100, StartedUtc = t0, ReadyAtUtc = t0.AddMinutes(1) } });
            legacy.CompleteReady(t0.AddMinutes(2)); legacy.CompleteReady(t0.AddMinutes(3));
            Check(legacy.UnitsOf(juice.Id) == 100, "A pending batch from an older save is counted once when it finishes");

            var gear = Bag((PrototypeItems.RabbitPelt, 10, 3));
            var made = CraftingService.Craft(gear, state, hat, 1, t0, NewId);
            Check(made.Success && made.Outputs.Count == 1 && made.Outputs[0].Quantity == 1 && made.Outputs[0].Profile.MaxDurability == 1600 && made.Outputs[0].Quality == 1 && !made.Outputs[0].Ready
                && gear.CountIngredient(PrototypeItems.RabbitPelt.Id) == 8, "A wearable is one unique instance with its snapshotted quality and durability");
            // A full bag: a stone fills it and the pelts wait in a pocket, so the crafted hat must go to overflow.
            int stoneSide = new SpatialInventory().AddWorld("probe", PrototypeItems.Stone, 1, 1).Width; // a single stone fills this bag exactly
            var full = new SpatialInventory(stoneSide, stoneSide); full.Reconfigure(stoneSide, stoneSide, 1, 0, out _);
            ItemEntry pelts = full.AddWorld("p", PrototypeItems.RabbitPelt, 2, 1); full.Reserve(pelts, 1); full.TryQuickStore(pelts, 1, out _); full.TryMoveToSide(pelts, 1, false, 0, out _); full.Release(pelts, 1);
            ItemEntry stone = full.AddWorld("stone", PrototypeItems.Stone, 1, 1); full.Reserve(stone, 2); full.TryQuickStore(stone, 2, out _); full.Release(stone, 2);
            var spill = CraftingService.Craft(full, state, hat, 1, t0, NewId);
            Check(pelts.Owner == ItemOwner.Consumed && stone.Owner == ItemOwner.Backpack && spill.Success && spill.ToOverflow == 1 && spill.Outputs[0].Owner == ItemOwner.Overflow,
                "Without bag room the output goes to the overflow tray instead of being lost (ingredients from a pocket count)");
            var slots = new CraftingState(); slots.Learn(juice.Id);
            var many = Bag((PrototypeItems.Apples, 1000, 1));
            bool allStarted = true;
            for (int i = 0; i < 12; i++) allStarted &= CraftingService.Craft(many, slots, juice, 10, t0, NewId).Success;
            Check(allStarted && slots.Batches.Count == 12 && many.CountIngredient(PrototypeItems.Apples.Id) == 1000 - 12 * 30,
                "No craft-slot limit: twelve timed batches run at once, each spending exactly its ingredients");

            // Elite upgrade (section 8.4): consumes one Normal instance (any condition with uses left) + two previous-tier
            // advanced materials; the Elite gets its own recipe snapshot (double base uses), not the input's stats.
            RecipeDefinition elite = RecipeBook.Find("recipe.tool.elite_axe_t2");
            var forge = new SpatialInventory(14000, 4000);
            ItemDefinition normalAxe = Items.Find("debug.tool.axe.t2.normal");
            ItemEntry worn = forge.AddWorld("worn-axe", normalAxe, 1, 5, new ItemProfile(999, 20, 3)); forge.Reserve(worn, 1); forge.TryQuickStore(worn, 1, out _); forge.Release(worn, 1);
            ItemEntry planks = forge.AddWorld("planks", Items.Find("item.pine_plank"), 2, 1); forge.Reserve(planks, 2); forge.TryQuickStore(planks, 2, out _); forge.Release(planks, 2);
            var upgrades = new CraftingState(); upgrades.Learn(elite.Id);
            var upgraded = CraftingService.Craft(forge, upgrades, elite, 1, t0, NewId);
            Check(upgraded.Success && worn.Owner == ItemOwner.Consumed && planks.Owner == ItemOwner.Consumed && upgraded.Outputs.Count == 1 && upgraded.Outputs[0].Definition.Id == "debug.tool.axe.t2.elite"
                && upgraded.Outputs[0].Profile.MaxDurability == 40 && upgraded.Outputs[0].Quality == 1 && !CraftStats.BatchAllowed(elite, 10),
                "Elite upgrade consumes one worn Normal tool plus two pine planks and creates one Elite with its own stats (40 uses, Q1), x1 only");
            Check(!CraftingService.Craft(forge, upgrades, elite, 1, t0, NewId).Success && ItemTiers.InWindow(elite, "item.pine_plank") && ItemTiers.Of("debug.tool.axe.t2.normal") == 2,
                "Without another Normal tool the upgrade crafts nothing; tier-two upgrades may use tier-one materials");

            // Persistence round trip of the crafting state.
            var saved = new SavedCrafting();
            saved.known.AddRange(state.Known);
            foreach (var pair in state.Units) saved.mastery.Add(new SavedMastery { recipeId = pair.Key, units = pair.Value });
            var restored = new CraftingState();
            restored.Restore(saved.known, saved.mastery.Select(m => new KeyValuePair<string, long>(m.recipeId, m.units)), new CraftBatch[0]);
            Check(restored.Knows(juice.Id) && restored.UnitsOf(juice.Id) == 1010 && restored.LevelOf(juice.Id) == 10, "Known recipes and mastery survive a save round trip");
            var bad = new SessionSnapshot { version = 1, crafting = new SavedCrafting { batches = new List<SavedBatch> { new SavedBatch { id = "b", recipeId = juice.Id, executions = 0, outputUnits = 1, readyAtUtc = "x" } } } };
            bool rejected; try { SessionSaveStore.Validate(bad, float.MaxValue, out _); rejected = false; } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "A malformed crafting batch makes the save invalid");
            VarietyUpdateChecks();

            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Milestone7-checks.txt", passed);
            Debug.Log($"[Milestone7] PASS: {passed.Count} crafting checks.");
        }

        // November 7, 2026 (plan sections 9.2, 9.4.1 and 11.6.1.1): fish dishes replace the plain roasts, which stay registered
        // as retired recipes so older saves keep their knowledge, mastery and pending batches; two refined-hide inputs change.
        private static void VarietyUpdateChecks()
        {
            string[] roasts = { "rainbow_trout", "largemouth_bass", "bluegill", "yellow_perch" };
            var dishes = new (string id, string fish, string extra, int quantity)[] {
                ("trout_mushroom_skillet", "fish.rainbow_trout", "ingredient.mushrooms", 1), ("salt_baked_bass", "fish.largemouth_bass", "ingredient.rock_salt", 2),
                ("pine_nut_bluegill", "fish.bluegill", "ingredient.pine_nuts", 1), ("apple_stuffed_perch", "fish.yellow_perch", "food.apples", 1) };
            Check(RecipeBook.All.Where(r => r.Retired).Select(r => r.Id).OrderBy(x => x).SequenceEqual(roasts.Select(f => "recipe.food.roasted_" + f).OrderBy(x => x))
                && roasts.All(f => Items.Find("item.roasted_" + f) is ItemDefinition legacy && legacy.FoodHearts == 2),
                "Exactly the four plain fish roasts are retired, and their items remain as legacy food");
            Check(dishes.All(d => RecipeBook.Find("recipe.food." + d.id) is RecipeDefinition r && !r.Retired && r.Role == RecipeRole.Food && r.Tier == 1 && r.Yield == 1
                    && r.Output != null && r.Output.FoodHearts == 2 && r.Output.PreparedFood && r.Inputs.Length == 3
                    && r.Inputs.Any(i => i.ItemId == d.fish && i.Quantity == 1) && r.Inputs.Any(i => i.ItemId == d.extra && i.Quantity == d.quantity)
                    && r.Inputs.Any(i => i.ItemId == "material.pine_branch" && i.Quantity == 1)
                    && TheTrailWalker.Domain.Items.Tier1CraftedItems.VisualPath(r.OutputId) != null),
                "Each fish has a different dish: its fish + a tier-one ingredient + a Pine branch -> 1 serving of 2 hearts, with a stand-in visual");
            Check(RecipeBook.All.Where(r => r.Role == RecipeRole.Food && !r.Retired && r.Inputs.Length == 2 && r.Inputs[0].ItemId.StartsWith("fish.") && r.Inputs[1].ItemId == "material.pine_branch").Count() == 0,
                "No active recipe is a plain fish + wood roast");
            RecipeDefinition cured = RecipeBook.Find("recipe.material.cured_rabbit_pelt"), fineLeather = RecipeBook.Find("recipe.material.fine_rabbit_leather"), leather = RecipeBook.Find("recipe.material.rabbit_leather");
            Check(cured.Inputs.Select(i => (i.ItemId, i.Quantity)).SequenceEqual(new[] { ("animal.rabbit.pelt", 2), ("ingredient.animal_fat", 1) })
                && fineLeather.Inputs.Select(i => (i.ItemId, i.Quantity)).SequenceEqual(new[] { ("item.rabbit_leather", 2), ("item.nettle_thread", 1) })
                && !cured.Inputs.Select(i => i.ItemId).SequenceEqual(leather.Inputs.Select(i => i.ItemId)),
                "Cured rabbit pelt uses Animal fat (no longer Rabbit leather's inputs) and Fine rabbit leather uses Nettle thread");
            // Saves that know a retired roast keep it: knowledge, mastery and a pending batch survive a reload.
            string retired = "recipe.food.roasted_bluegill";
            var batch = new CraftBatch { Id = "old-roast", RecipeId = retired, Executions = 5, OutputUnits = 5, Level = 1, Quality = 1, StartedUtc = DateTime.UtcNow, ReadyAtUtc = DateTime.UtcNow.AddMinutes(1), MasteryCredited = true };
            var reloaded = new CraftingState();
            reloaded.Restore(new[] { retired }, new[] { new KeyValuePair<string, long>(retired, 12) }, new[] { batch });
            Check(reloaded.Knows(retired) && reloaded.UnitsOf(retired) == 12 && reloaded.Batches.Count == 1 && reloaded.Batches[0].RecipeId == retired,
                "A save that knows a retired roast keeps the recipe, its mastery and its pending batch");
        }
    }
}
