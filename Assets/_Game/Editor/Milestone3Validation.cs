using System;
using System.IO;
using TheTrailWalker.Domain;
using TheTrailWalker.Persistence;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    public static class Milestone3Validation
    {
        private sealed class FixedClock : IClock { public DateTime UtcNow => new DateTime(2026, 10, 30, 0, 0, 0, DateTimeKind.Utc); }
        private static int checks;
        private static void Check(bool value, string message) { if (!value) throw new Exception("Milestone 3: " + message); checks++; }
        [MenuItem("The Trail Walker/Milestone 3/2 - Validate Save and Recovery")]
        public static void Run()
        {
            checks = 0;
            string directory = Path.Combine("Builds", "Validation", "Milestone3-" + Guid.NewGuid().ToString("N"));
            var store = new SessionSaveStore(directory, new FixedClock());
            Check(store.Load(158, out _, out _) == LoadStatus.Fresh, "Fresh profile");
            var state = new SessionSnapshot { distance = 12, speedCap = 0.8f, requestedSpeed = 0.7f, yaw = 315, transactionId = "a" };
            state.items.Add(new SavedItem { id = "world-1", definitionId = "material.stone", owner = "World", quantity = 1000, quality = 1, ready = true, trailDistance = 5, worldX = 2, worldY = 0.5f, worldZ = 5, unitValue = 100 });
            state.items.Add(new SavedItem { id = "bag-1", definitionId = "food.berries", owner = "Backpack", quantity = 999, quality = 5, ready = true, x = 120, y = 340, unitValue = 200 });
            store.Save(state);
            Check(store.Load(158, out SessionSnapshot loaded, out _) == LoadStatus.Loaded, "Roundtrip load");
            Check(loaded.items.Count == 2 && loaded.items[0].quantity == 1000 && loaded.items[1].quantity == 999 && loaded.items[1].x == 120 && loaded.items[1].unitValue == 200, "Item identity, owners, quantities, position and profile restored");
            Check(loaded.distance == 12 && loaded.requestedSpeed == 0.7f && loaded.yaw == 315, "Distance, walking intent and first-person yaw restored");
            state.inventoryOpen = true; state.inventoryResume = 0.7f; state.distance = 13; state.transactionId = "b"; store.Save(state);
            Check(File.Exists(store.BackupPath), "Previous save retained");
            Check(store.Load(158, out loaded, out _) == LoadStatus.Loaded && loaded.inventoryOpen && loaded.inventoryResume == 0.7f, "Inventory pause and resume snapshot survive load");
            File.WriteAllText(store.MainPath, "corrupted");
            Check(store.Load(158, out loaded, out _) == LoadStatus.Recovered && loaded.transactionId == "a", "Corrupt primary recovers previous backup");
            store.Save(loaded);
            Check(store.Load(158, out _, out _) == LoadStatus.Loaded && new SessionSaveStore(directory, new FixedClock()).Load(158, out _, out _) == LoadStatus.Loaded, "Recovered snapshot can be safely saved again");
            File.WriteAllText(store.MainPath, "{\"version\":99}");
            Check(store.Load(158, out _, out _) == LoadStatus.Unsupported && File.ReadAllText(store.MainPath).Contains("99"), "Newer schema is never replaced by old backup");
            var stop = new MovementState(); stop.Restore(4, 0, 0.8f, 0, false, true, false); stop.OpenInventory(); stop.CloseInventory();
            Check(stop.PostHarvestStop && stop.ActualSpeed == 0, "Post-harvest stop survives inventory close after load");
            stop.SpeedGesture(0.3f); Check(!stop.PostHarvestStop && stop.ActualSpeed > 0, "Fresh upward gesture resumes restored stop");
            bool rejected = false;
            try { state.items.Add(state.items[0]); SessionSaveStore.Validate(state, 158, out _); }
            catch (ArgumentException) { rejected = true; }
            Check(rejected, "Duplicate instance identities rejected");
            var randomA = new SeededRandom(42); var randomB = new SeededRandom(42);
            Check(randomA.Next(10000) == randomB.Next(10000), "Injectable seeded random source is reproducible");
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllText("Builds/Validation/Milestone3-checks.txt", $"PASS: {checks} persistence and recovery checks.\n");
            Debug.Log($"[Milestone3] PASS: {checks} persistence and recovery checks.");
        }
    }
}
