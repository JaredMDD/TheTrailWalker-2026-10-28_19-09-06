using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Persistence;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    public static class Milestone4PersistenceValidation
    {
        private sealed class Clock : IClock { public DateTime UtcNow => new DateTime(2026, 10, 30, 0, 0, 0, DateTimeKind.Utc); }
        private static int checks;
        private static void Check(bool result, string name) { if (!result) throw new InvalidOperationException("Milestone 4 persistence: " + name); checks++; }
        private static bool Invalid(SessionSnapshot state)
        { try { SessionSaveStore.Validate(state, 158, out _); return false; } catch (Exception e) when (e is InvalidDataException || e is ArgumentException || e is InvalidOperationException) { return true; } }

        [MenuItem("The Trail Walker/Milestone 4/Validate Save Migration")]
        public static void Run()
        {
            checks = 0;
            string directory = Path.Combine("Builds", "Validation", "Milestone4-" + Guid.NewGuid().ToString("N"));
            var oldStore = new SessionSaveStore(directory, new Clock());
            var old = new SessionSnapshot { distance = 10, speedCap = 2.4f, requestedSpeed = 1.8f, inventoryResume = 1.8f, inventoryOpen = true, yaw = 100 };
            old.items.Add(new SavedItem { id = "stone", definitionId = PrototypeItems.Stone.Id, owner = "Backpack", quantity = 1000, quality = 3, ready = true, x = 0, y = 0, unitValue = 100 });
            oldStore.Save(old);
            byte[] original = File.ReadAllBytes(oldStore.MainPath);
            SessionSnapshot upgraded = SessionSaveStore.MigrateV1(old, 158);
            Check(upgraded.version == 2 && upgraded.survival.maximum == 3 && upgraded.survival.hearts == 3, "old profile migrates to starter stamina");
            Check(Math.Abs(upgraded.speedCap - TheTrailWalker.Domain.MovementState.StarterSpeedCap) < 0.0001 && Math.Abs(upgraded.requestedSpeed - TheTrailWalker.Domain.MovementState.StarterSpeedCap) < 0.0001 && Math.Abs(upgraded.inventoryResume - TheTrailWalker.Domain.MovementState.StarterSpeedCap) < 0.0001, "migration clamps speed to starter equipment");
            Check(upgraded.inventoryOpen && upgraded.yaw == old.yaw && upgraded.distance == old.distance && upgraded.items[0].id == "stone" && upgraded.items[0].quantity == 1000, "migration preserves position, pause, yaw and ownership");
            var newStore = new SessionSaveStore(directory, new Clock(), 2);
            newStore.Save(upgraded);
            Check(Convert.ToBase64String(File.ReadAllBytes(oldStore.MainPath)) == Convert.ToBase64String(original), "new schema does not overwrite original version 1 profile");
            Check(newStore.Load(158, out var restored, out _) == LoadStatus.Loaded && restored.version == 2, "new schema roundtrip");
            Check(oldStore.Load(158, out var prior, out _) == LoadStatus.Loaded && prior.version == 1 && prior.requestedSpeed == 1.8f, "version 1 profile remains usable by previous build");

            restored.survival.gear.Add(new SavedGear { id = "debug-shirt", slot = "Shirt", rank = 4, quality = 5, remainingDistance = 100 });
            restored.survival.gear.Add(new SavedGear { id = "debug-pants", slot = "Pants", rank = 4, quality = 5, remainingDistance = 100 });
            restored.items.Add(new SavedItem { id = "berry", definitionId = PrototypeItems.Berries.Id, owner = "Pocket", quantity = 1000, quality = 1, ready = true, x = 9 });
            restored.items.Add(new SavedItem { id = "axe", definitionId = PrototypeItems.Tool.Id, owner = "Hook", quantity = 1, quality = 1, ready = true, x = 9 });
            restored.items.Add(new SavedItem { id = "overflow", definitionId = PrototypeItems.Log.Id, owner = "Overflow", quantity = 1000, quality = 1, ready = true });
            SessionSaveStore.Validate(restored, 158, out SpatialInventory inventory);
            Check(inventory.PocketCount == 10 && inventory.HookCount == 10 && inventory.Entries[1].Owner == ItemOwner.Pocket && inventory.Entries[2].Owner == ItemOwner.Hook && inventory.Entries[3].Owner == ItemOwner.Overflow, "side storage and overflow restore with ownership");
            restored.survival.gear.Add(new SavedGear { id = "hat-instance", slot = "Hat", rank = 4, quality = 5, remainingDistance = 100 });
            restored.survival.maximum = 12;
            restored.items.Add(new SavedItem { id = "hat-instance", definitionId = PrototypeItems.TestHat.Id, owner = "Worn", quantity = 1, quality = 5, ready = true, hasWear = true, wearRemaining = 100.25 });
            Check(Invalid(restored), "worn item and slot durability must agree"); restored.items[4].wearRemaining = 100;
            SessionSaveStore.Validate(restored, 158, out inventory);
            Check(inventory.Entries[4].Owner == ItemOwner.Worn, "worn item and its matching gear slot restore once");
            restored.items[4].owner = "Backpack"; Check(Invalid(restored), "one item cannot be both backpack and worn"); restored.items[4].owner = "Worn";
            var hatGear = restored.survival.gear[2]; restored.survival.gear.RemoveAt(2); restored.survival.maximum = 3;
            restored.items[4].owner = "Overflow"; restored.items[4].wearRemaining = 99.25;
            newStore.Save(restored);
            Check(newStore.Load(158, out var unequipped, out _) == LoadStatus.Loaded && unequipped.items[4].owner == "Overflow" && Math.Abs(unequipped.items[4].wearRemaining - 99.25) < 0.00001, "unequipped overflow retains fractional durability through save");
            restored.survival.gear.Add(hatGear); restored.survival.maximum = 12; restored.items[4].owner = "Worn"; restored.items[4].wearRemaining = 100;
            restored.items.RemoveAt(4); restored.survival.gear.RemoveAt(2); restored.survival.maximum = 3;
            restored.items[1].x = 10; Check(Invalid(restored), "out-of-range pocket rejected"); restored.items[1].x = 9;
            restored.items[2].owner = "Pocket"; Check(Invalid(restored), "tool cannot restore into pocket"); restored.items[2].owner = "Hook";
            restored.survival.gear.Add(new SavedGear { id = "second-shirt", slot = "Shirt", rank = 1, quality = 1, remainingDistance = 1 });
            Check(Invalid(restored), "duplicate worn slot rejected"); restored.survival.gear.RemoveAt(2);
            restored.survival.fainted = true; restored.survival.hearts = 0; restored.survival.faintEventId = "faint-1";
            restored.requestedSpeed = restored.inventoryResume = 0; restored.inventoryOpen = false; restored.postHarvest = true;
            restored.survival.loss.Add(new SavedLoss { itemId = "lost-stack", quantity = 1000 });
            SessionSaveStore.Validate(restored, 158, out _); Check(true, "committed faint manifest validates");
            restored.survival.loss[0].itemId = "stone"; Check(Invalid(restored), "faint manifest cannot also retain lost item");
            restored.survival.loss[0].itemId = "debug-shirt"; Check(Invalid(restored), "worn equipment cannot be faint loss");
            restored.survival.loss[0].itemId = "lost-stack"; restored.survival.pendingFood = 1; Check(Invalid(restored), "faint cannot begin with pending food");
            // Pending food and player-dropped provenance survive a force-close without re-eating or re-rolling.
            restored.survival.fainted = false; restored.survival.hearts = 3; restored.survival.faintEventId = ""; restored.survival.loss.Clear(); restored.survival.pendingFood = 12;
            restored.items.Add(new SavedItem { id = "dropped-log", definitionId = PrototypeItems.Log.Id, owner = "World", quantity = 640, quality = 4, ready = true, dropped = true, trailDistance = 20 });
            newStore.Save(restored);
            Check(newStore.Load(158, out var reloaded, out _) == LoadStatus.Loaded && Math.Abs(reloaded.survival.pendingFood - 12) < 1e-9, "consumed food stays pending once through a save");
            SavedItem droppedLog = reloaded.items.Find(item => item.id == "dropped-log");
            SessionSaveStore.Validate(reloaded, 158, out SpatialInventory droppedInventory);
            ItemEntry droppedEntry = null; foreach (ItemEntry entry in droppedInventory.Entries) if (entry.Id == "dropped-log") droppedEntry = entry;
            Check(droppedLog != null && droppedLog.dropped && droppedEntry != null && droppedEntry.Dropped && droppedEntry.Quantity == 640 && droppedEntry.Quality == 4 && droppedEntry.Owner == ItemOwner.World, "dropped stack keeps provenance, quantity and quality through reload");
            // Retest flag for section 7.1.1: every tree raw/refined/prepared item survives a save with exact identity.
            int catalogStart = restored.items.Count; var trees = new List<ItemDefinition>(PrototypeItems.TreeIngredients); trees.AddRange(PrototypeItems.TreeRefinements); trees.AddRange(PrototypeItems.TreeDishes);
            for (int i = 0; i < trees.Count; i++)
                restored.items.Add(new SavedItem { id = "tree-" + i, definitionId = trees[i].Id, owner = i % 2 == 0 ? "Overflow" : "World", quantity = (i * 97 + 3) % 1000 + 1, quality = i % 5 + 1, ready = i != trees.Count - 1, unitValue = 100 + i, dropped = i % 2 == 1, trailDistance = i });
            newStore.Save(restored);
            Check(newStore.Load(158, out var treeLoad, out _) == LoadStatus.Loaded, "tree food catalog save reloads");
            SessionSaveStore.Validate(treeLoad, 158, out SpatialInventory treeInventory);
            bool treeExact = true;
            for (int i = 0; i < trees.Count; i++)
            {
                ItemEntry entry = null; foreach (ItemEntry candidate in treeInventory.Entries) if (candidate.Id == "tree-" + i) entry = candidate;
                treeExact &= entry != null && entry.Definition == trees[i] && entry.Quantity == (i * 97 + 3) % 1000 + 1 && entry.Quality == i % 5 + 1 && entry.Profile.UnitValue == 100 + i && entry.Ready == (i != trees.Count - 1) && entry.Dropped == (i % 2 == 1);
            }
            Check(treeExact, "raw, refined and prepared tree items keep ID, quantity, quality, profile, readiness and dropped marker through reload");
            // A mixed-quality stack restores its whole pile, lowest quality first.
            restored.items[catalogStart].quality = 1;
            restored.items[catalogStart].pile = new List<SavedPortion> { new SavedPortion { quality = 4, quantity = 3, unitValue = 100 }, new SavedPortion { quality = 1, quantity = 1, unitValue = 100 } };
            newStore.Save(restored);
            Check(newStore.Load(158, out var pileLoad, out _) == LoadStatus.Loaded, "mixed-quality pile save reloads");
            SessionSaveStore.Validate(pileLoad, 158, out SpatialInventory pileInventory);
            ItemEntry pileEntry = null; foreach (ItemEntry candidate in pileInventory.Entries) if (candidate.Id == "tree-0") pileEntry = candidate;
            Check(pileEntry != null && pileEntry.Quantity == 4 && pileEntry.Pile.Count == 2 && pileEntry.Pile[0].Quality == 1 && pileEntry.Pile[1].Quality == 4 && pileEntry.Pile[1].Quantity == 3, "a mixed-quality stack keeps its pile through reload");
            restored.items[catalogStart].pile[0].quantity = 9; Check(Invalid(restored), "a pile that disagrees with the stack quantity is rejected");
            restored.items[catalogStart].pile = null;
            restored.items.RemoveRange(catalogStart, trees.Count);
            restored.items[restored.items.Count - 1].owner = "Overflow"; Check(Invalid(restored), "only trail items can carry the dropped marker");
            restored.items.RemoveAt(restored.items.Count - 1); restored.survival.pendingFood = 0;
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllText("Builds/Validation/Milestone4-persistence-checks.txt", "PASS " + checks + "\n");
            Debug.Log("[Milestone4] PASS " + checks + " save/migration checks.");
        }
    }
    [InitializeOnLoad] internal static class Milestone4PersistenceRequest
    {
        static Milestone4PersistenceRequest() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            const string request = "Temp/Milestone4-persistence.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(request);
            try { Milestone3Validation.Run(); Milestone4PersistenceValidation.Run(); File.WriteAllText("Temp/Milestone4-persistence.result", "PASS"); }
            catch (Exception error) { Debug.LogException(error); File.WriteAllText("Temp/Milestone4-persistence.result", "FAILED " + error); }
        }
    }
}
