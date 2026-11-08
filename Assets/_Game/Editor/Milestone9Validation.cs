using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Persistence;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Milestone 9: camp trading and wallets (plan sections 12.1-12.2). Engine-free rules come from TradeModelChecks; this adds
    // the persistence side: a mid-session save/reload replays the same session, escrow survives a reload, and older saves
    // migrate their item values to minor units once.
    public static class Milestone9Validation
    {
        [MenuItem("The Trail Walker/Milestone 9/Validate Trading")]
        public static void Run()
        {
            var passed = new List<string>(TradeModelChecks.Run());
            void Check(bool ok, string name) { if (!ok) throw new Exception("Milestone 9: " + name); passed.Add(name); }

            // Restart mid-session: the JSON copy continues exactly where the original left off and cannot be refreshed.
            var offers = TradeOffers.Generate(TradeSession.SeedFor("camp.01.03", 4), new[] { "material.pine_branch", "food.apples" });
            var original = TradeSession.Start("camp.01.03", 4, offers);
            for (int i = 0; i < 4; i++) original.AddPlayerListing("esc" + i, PrototypeItems.Log, 100, 1, 30000);
            original.Advance(12.8f);
            var reloaded = new TradeSession(JsonUtility.FromJson<TradeSessionData>(JsonUtility.ToJson(original.Data)));
            Check(reloaded.State == TradeState.Active && Math.Abs(reloaded.Remaining - original.Remaining) < 1e-4 && reloaded.Data.listings.Count == original.Data.listings.Count
                && reloaded.Data.listings.Select(l => l.result + ":" + l.settlementId).SequenceEqual(original.Data.listings.Select(l => l.result + ":" + l.settlementId)), "a reload keeps the session, its remaining time and every settlement");
            original.Advance(20f); reloaded.Advance(20f);
            Check(reloaded.State == TradeState.Completed && original.Data.listings.Select(l => l.result + ":" + l.cash).SequenceEqual(reloaded.Data.listings.Select(l => l.result + ":" + l.cash)), "the reloaded session settles exactly like the original");
            Check(reloaded.Advance(5f).Count == 0 && reloaded.Cancel().Count == 0, "a finished session reloaded stays spent");

            // Escrow entries persist with their units and cannot reappear in the bag.
            var snapshot = new SessionSnapshot { version = 2, speedCap = 1.5f, survival = new SavedSurvival(), economy = 1 };
            snapshot.items.Add(new SavedItem { id = "listed", definitionId = PrototypeItems.Log.Id, owner = "Escrow", quantity = 250, quality = 2, ready = true, unitValue = 345 });
            snapshot.items.Add(new SavedItem { id = "kept", definitionId = PrototypeItems.Log.Id, owner = "Backpack", quantity = 750, quality = 2, ready = true, unitValue = 345 });
            SessionSaveStore.Validate(snapshot, float.MaxValue, out SpatialInventory bag);
            ItemEntry listed = bag.Entries.First(e => e.Id == "listed");
            Check(listed.Owner == ItemOwner.Escrow && listed.Quantity == 250 && bag.CountIngredient(PrototypeItems.Log.Id) == 750, "escrowed units reload out of the bag");
            snapshot.items[0].x = 3; bool rejected = false;
            try { SessionSaveStore.Validate(snapshot, float.MaxValue, out _); } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "an escrow record with a bag position is rejected");

            // Economy migration (older saves): drops re-derived from the price data, crafted snapshots x100, batches too.
            var legacy = new SessionSnapshot { version = 3 };
            legacy.items.Add(new SavedItem { id = "drop", definitionId = PrototypeItems.Branch.Id, owner = "Backpack", quantity = 1000, quality = 5, unitValue = 100,
                pile = new List<SavedPortion> { new SavedPortion { quality = 1, quantity = 400, unitValue = 100 }, new SavedPortion { quality = 5, quantity = 600, unitValue = 100 } } });
            legacy.items.Add(new SavedItem { id = "bird", definitionId = "item.pine_bird", owner = "Backpack", quantity = 3, quality = 5, unitValue = 1200 });
            legacy.crafting = new SavedCrafting(); legacy.crafting.batches.Add(new SavedBatch { recipeId = "recipe.misc.pine_bird", quality = 1, unitValue = 480 });
            SessionSaveStore.MigrateEconomy(legacy);
            Check(legacy.economy == 1 && legacy.items[0].pile[0].unitValue == 100 && legacy.items[0].pile[1].unitValue == 200 && legacy.items[1].unitValue == 120000 && legacy.crafting.batches[0].unitValue == 48000,
                "older saves migrate item and batch values to minor units");
            int before = (int)legacy.items[1].unitValue; SessionSaveStore.MigrateEconomy(legacy);
            Check(legacy.items[1].unitValue == before, "the economy migration runs once");

            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Milestone9-checks.txt", passed);
            Debug.Log($"Milestone 9: {passed.Count} checks passed.");
        }
    }
}
