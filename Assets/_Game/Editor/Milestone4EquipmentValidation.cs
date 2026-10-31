using System;
using System.IO;
using TheTrailWalker.Domain.Equipment;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    public static class Milestone4EquipmentValidation
    {
        [MenuItem("The Trail Walker/Milestone 4/Validate Equipment Model")]
        public static void Run()
        {
            var state = new EquipmentState();
            Check(state.MaxHearts == 3 && state.Pockets == 0 && state.Hooks == 0 && state.BackpackWidth == 6 && Math.Abs(state.SpeedCap - 1.0) < 0.0001, "starter defaults");
            state.Equip(new WornGear("hat", GearSlot.Hat, 4, 5, 10));
            state.Equip(new WornGear("boots", GearSlot.Boots, 4, 5, 10));
            state.Equip(new WornGear("shirt", GearSlot.Shirt, 4, 5, 10));
            state.Equip(new WornGear("pants", GearSlot.Pants, 4, 5, 10));
            state.Equip(new WornGear("bag", GearSlot.Backpack, 4, 5, 10));
            Check(state.MaxHearts == 12 && state.Pockets == 10 && state.Hooks == 10 && state.BackpackWidth == 14 && Math.Abs(state.SpeedCap - 3.0) < 0.0001, "maximum caps");
            Check(state.ApplyDistance(0).Count == 0 && state.Get(GearSlot.Hat).RemainingDistance == 10, "stationary equipment does not wear");
            Check(state.ApplyDistance(10).Count == 5 && state.MaxHearts == 3 && state.Pockets == 0 && state.Hooks == 0 && state.BackpackWidth == 6 && Math.Abs(state.SpeedCap - 1.0) < 0.0001, "simultaneous expiry removes capabilities");
            state.Equip(new WornGear("infinite", GearSlot.Hat, 1, 1, 0, true));
            Check(state.ApplyDistance(100000).Count == 0 && state.MaxHearts == 4, "infinite durability exemption");
            var stamina = new StaminaState(); stamina.QueueFood(6); stamina.Advance(9, 0, false, true);
            Check(stamina.Hearts == 3 && Math.Abs(stamina.PendingFood - 3) < 0.0001, "food delivered to a full stationary meter is lost, not stored");
            stamina.Advance(10, 500, false, true);
            Check(Math.Abs(stamina.Hearts - 2.9) < 0.0001 && stamina.PendingFood == 0, "at the cap food only balances walking drain");
            stamina.QueueFood(5); stamina.Advance(100, 0, false, false);
            Check(Math.Abs(stamina.PendingFood - 5) < 0.0001 && Math.Abs(stamina.Hearts - 2.9) < 0.0001, "suspension freezes food recovery");
            stamina.Restore(0, 3, 0, false, 0);
            Check(!stamina.ShouldFaint(false, true) && stamina.ShouldFaint(false, false), "accessible food prevents fainting");
            stamina.BeginFaint(); stamina.Advance(4.5, 0, false, true);
            Check(stamina.Fainted && Math.Abs(stamina.Hearts - 1.5) < 0.0001, "faint recovery progresses");
            stamina.Advance(4.5, 0, false, true);
            Check(!stamina.Fainted && Math.Abs(stamina.Hearts - 3) < 0.0001, "faint recovery completes without walking intent");
            var residue = new StaminaState(); residue.Restore(1e-11, 3, 0, false, 0); residue.Advance(0.1, 0, false, true);
            Check(residue.IsEmpty && residue.ShouldFaint(false, false), "boundary residue counts as empty so walking can faint");
            residue.BeginFaint(); Check(residue.Hearts == 0 && residue.Fainted, "fainting from a residue starts recovery from exactly zero");
            var walker = new StaminaState(); double clipped = walker.LimitWalkingSeconds(1000, 2, 0.012); walker.Advance(clipped, clipped * 2, false, true, 0.012);
            Check(walker.IsEmpty && walker.ShouldFaint(false, false) && walker.LimitWalkingSeconds(1, 2, 0.012) == 0, "walking to the zero boundary stops and can faint");
            // Section 4.4.3 (revised): food never builds a reserve; at the cap it at most balances drain and the rest is lost.
            var capped = new StaminaState(); capped.QueueFood(12); capped.Advance(36, 0, false, true);
            Check(capped.Hearts == 3 && capped.PendingFood == 0, "a twelve-heart serving eaten at 3/3 while standing is lost, not stored");
            var rising = new StaminaState(); rising.Restore(1, 12, 0, false, 0); rising.QueueFood(12); rising.Advance(10, 100, false, true, 0.012);
            Check(Math.Abs(rising.Hearts - (1 + 10.0 / 3 - 1.2)) < 1e-6 && Math.Abs(rising.PendingFood - (12 - 10.0 / 3)) < 1e-6, "below the cap walking receives food progressively at one third heart per second");
            var balanced = new StaminaState(); balanced.Restore(12, 12, 0, false, 0); balanced.QueueFood(1); balanced.Advance(3, 75, false, true, 0.012);
            Check(balanced.Hearts == 12 && balanced.PendingFood == 0, "at the cap food at most balances walking drain");
            var overflow = new StaminaState(); overflow.Restore(11.9, 12, 0, false, 0); overflow.QueueFood(12); overflow.Advance(36, 0, false, true);
            Check(overflow.Hearts == 12 && overflow.PendingFood == 0, "recovery that catches up with the maximum loses the extra");
            var stacked = new StaminaState(); stacked.Restore(0.5, 12, 0, false, 0); stacked.QueueFood(12); stacked.QueueFood(12); stacked.Advance(3, 0, false, true);
            Check(Math.Abs(stacked.Hearts - 1.5) < 1e-9 && Math.Abs(stacked.PendingFood - 23) < 1e-9, "several servings queue without multiplying the delivery rate");
            var hatLoss = new StaminaState(); hatLoss.Restore(12, 12, 0, false, 0); hatLoss.QueueFood(12); hatLoss.SetMaximum(3);
            Check(hatLoss.Hearts == 3 && hatLoss.PendingFood == 12, "hat expiry clamps the meter; the remaining serving keeps delivering and is not deleted instantly");
            var pendingFirst = new StaminaState(); pendingFirst.Restore(0, 3, 2, false, 0);
            Check(pendingFirst.IsEmpty && !pendingFirst.ShouldFaint(false, false), "pending recovery prevents fainting before any auto-eat");
            Debug.Log("[Milestone4] Equipment model checks passed.");
        }
        private static void Check(bool condition, string name) { if (!condition) throw new InvalidOperationException("Milestone 4 equipment model: " + name); }
    }

    [InitializeOnLoad]
    internal static class Milestone4EquipmentCheckRequest
    {
        static Milestone4EquipmentCheckRequest() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            const string request = "Temp/Milestone4-equipment-check.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(request);
            try { Milestone4EquipmentValidation.Run(); File.WriteAllText("Temp/Milestone4-equipment-check.result", "PASS"); }
            catch (Exception error) { Debug.LogException(error); File.WriteAllText("Temp/Milestone4-equipment-check.result", "FAILED " + error); }
        }
    }
}
