using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Crafting
{
    // The craft transaction (section 10): validate, consume ingredients atomically, create the outputs (stackables split
    // into legal stacks of at most 1,000; tools and gear one unique instance), place them in the bag without merging
    // (or the overflow tray when the bag is full), and record the timed batch. Instant material conversions create ready
    // outputs and no batch. Nothing is spent when any check fails.
    public static class CraftingService
    {
        public sealed class Outcome
        {
            public bool Success; public string Reason = "";
            public CraftResult Result; public CraftBatch Batch;
            public readonly List<ItemEntry> Outputs = new List<ItemEntry>();
            public int ToOverflow;
        }

        public static Outcome Preview(SpatialInventory ledger, CraftingState state, RecipeDefinition recipe, int executions)
        {
            var outcome = new Outcome();
            if (recipe == null) { outcome.Reason = "Unknown recipe"; return outcome; }
            outcome.Result = CraftStats.Compute(recipe, state.UnitsOf(recipe.Id), Math.Max(1, executions));
            if (!state.Knows(recipe.Id)) { outcome.Reason = "Recipe not learned yet"; return outcome; }
            if (!CraftStats.BatchAllowed(recipe, executions)) { outcome.Reason = recipe.BulkAllowed ? "Choose x1, x10, x100 or x1000" : "Tools, clothing and backpacks are crafted one at a time"; return outcome; }
            foreach (RecipeInput input in recipe.Inputs)
                if (ledger.CountIngredient(input.ItemId) < (long)input.Quantity * executions) { outcome.Reason = "Missing " + (Items.Find(input.ItemId)?.Name ?? input.ItemId); return outcome; }
            if (recipe.Output == null) { outcome.Reason = "Recipe output is missing"; return outcome; }
            outcome.Success = true;
            return outcome;
        }

        public static Outcome Craft(SpatialInventory ledger, CraftingState state, RecipeDefinition recipe, int executions, DateTime now, Func<string> newId)
        {
            Outcome outcome = Preview(ledger, state, recipe, executions);
            if (!outcome.Success) return outcome;
            CraftResult result = outcome.Result;
            var needs = new List<KeyValuePair<string, long>>();
            foreach (RecipeInput input in recipe.Inputs) needs.Add(new KeyValuePair<string, long>(input.ItemId, (long)input.Quantity * executions));
            if (!ledger.TryConsumeIngredients(needs)) { outcome.Success = false; outcome.Reason = "Ingredients changed; nothing was spent"; return outcome; }

            bool timed = result.Seconds > 0;
            ItemDefinition output = recipe.Output;
            var profile = new ItemProfile(result.UnitValue, result.MaxDurability, result.MaxDurability);
            CraftBatch batch = timed ? state.Add(recipe, result, now, newId()) : null;
            // Level up at the moment of crafting (user, November 2, 2026): the next craft of this recipe, even while this
            // batch is still on its timer, already uses the new mastery level and its bonus stats.
            state.CreditMastery(recipe, result.OutputUnits);
            if (batch != null) batch.MasteryCredited = true;
            int remaining = result.OutputUnits;
            while (remaining > 0)
            {
                int quantity = output.Stackable ? Math.Min(1000, remaining) : 1;
                remaining -= quantity;
                ItemEntry item = ledger.AddWorld("craft-" + newId(), output, quantity, result.Quality, profile, ready: !timed);
                if (!ledger.TryStoreCrafted(item)) { ledger.RestoreStored(item, ItemOwner.Overflow, 0, 0); outcome.ToOverflow++; }
                if (output.GearSlot.HasValue && result.MaxDurability > 0) ledger.RestoreWear(item, true, result.MaxDurability);
                outcome.Outputs.Add(item);
                batch?.ItemIds.Add(item.Id);
            }
            outcome.Batch = batch;
            return outcome;
        }

        // Finished batches: outputs become ready (wherever they are) and the batch closes (mastery was counted at craft start).
        public static List<CraftBatch> Finish(SpatialInventory ledger, CraftingState state, DateTime now)
        {
            List<CraftBatch> ready = state.CompleteReady(now);
            foreach (CraftBatch batch in ready)
            {
                foreach (ItemEntry item in ledger.Entries)
                    if (batch.ItemIds.Contains(item.Id)) ledger.MarkReady(item);
                state.Remove(batch);
            }
            return ready;
        }
    }
}
