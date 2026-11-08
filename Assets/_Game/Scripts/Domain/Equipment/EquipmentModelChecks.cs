using System;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Equipment
{
    public static class EquipmentModelChecks
    {
        public static int Run()
        {
            int count = 0;
            void Check(bool value, string name) { if (!value) throw new InvalidOperationException("Milestone 4: " + name); count++; }
            var gear = new EquipmentState();
            Check(gear.MaxHearts == 3 && gear.Pockets == 0 && gear.Hooks == 0 && gear.BackpackWidth == 6 && Math.Abs(gear.SpeedCap - 1.5) < 1e-8, "starter capabilities");
            gear.Equip(new WornGear("hat", GearSlot.Hat, 4, 5, 10));
            gear.Equip(new WornGear("boots", GearSlot.Boots, 4, 5, 10));
            gear.Equip(new WornGear("shirt", GearSlot.Shirt, 4, 5, 10));
            gear.Equip(new WornGear("pants", GearSlot.Pants, 4, 5, 10));
            gear.Equip(new WornGear("bag", GearSlot.Backpack, 4, 5, 10));
            Check(gear.MaxHearts == 12 && gear.Pockets == 10 && gear.Hooks == 10 && gear.BackpackWidth == 14 && Math.Abs(gear.SpeedCap - 4.5) < 1e-8, "maximum capabilities");
            Check(gear.ApplyDistance(0).Count == 0 && gear.Get(GearSlot.Hat).RemainingDistance == 10, "zero distance does not wear equipment");
            Check(gear.ApplyDistance(9).Count == 0 && Math.Abs(gear.Get(GearSlot.Hat).RemainingDistance - 1) < 1e-8, "partial distance wear");
            Check(gear.ApplyDistance(1).Count == 5 && gear.MaxHearts == 3 && gear.Pockets == 0 && gear.Hooks == 0 && gear.BackpackWidth == 6 && Math.Abs(gear.SpeedCap - 1.5) < 1e-8, "simultaneous expiration restores defaults");
            gear.Equip(new WornGear("infinite", GearSlot.Hat, 1, 1, 0, true));
            Check(gear.ApplyDistance(100000).Count == 0 && gear.MaxHearts == 4, "infinite gear does not expire");
            int[] hearts = { 3, 4, 6, 9, 12 }, slots = { 0, 2, 4, 7, 10 }, widths = { 6, 8, 10, 12, 14 };
            for (int rank = 1; rank <= 4; rank++)
            {
                var ranked = new EquipmentState();
                foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot))) ranked.Equip(new WornGear("rank" + rank + slot, slot, rank, 1, 100));
                Check(ranked.MaxHearts == hearts[rank] && ranked.Pockets == slots[rank] && ranked.Hooks == slots[rank] && ranked.BackpackWidth == widths[rank] && Math.Abs(ranked.SpeedCap - (1.5 + rank * 0.75)) < 1e-8, "rank " + rank + " capabilities");
            }
            Check(Math.Abs(WornGear.CreateTestGear("q1", GearSlot.Boots, 1, 1, 100).RemainingDistance - 100) < 1e-8 && Math.Abs(WornGear.CreateTestGear("q5", GearSlot.Boots, 1, 5, 100).RemainingDistance - 180) < 1e-8, "finished quality affects test durability");
            bool rejected = false; try { gear.Equip(new WornGear("second", GearSlot.Hat, 1, 1, 1)); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected && gear.Get(GearSlot.Hat).InstanceId == "infinite", "occupied slot cannot discard existing gear");
            var stamina = new StaminaState(); stamina.QueueFood(6); stamina.Advance(9, 0, false, true);
            Check(stamina.Hearts == 3 && Math.Abs(stamina.PendingFood - 3) < 1e-8, "food delivered to a full stationary meter is lost, not stored");
            stamina.Advance(10, 500, false, true);
            Check(Math.Abs(stamina.Hearts - 2.9) < 1e-8 && stamina.PendingFood == 0, "at the cap food only balances walking drain until the serving is spent");
            stamina.QueueFood(5);
            stamina.Advance(100, 0, false, false);
            Check(Math.Abs(stamina.PendingFood - 5) < 1e-8 && Math.Abs(stamina.Hearts - 2.9) < 1e-8, "background time freezes recovery");
            stamina.Restore(0, 3, 0, false, 0);
            Check(!stamina.ShouldFaint(false, true) && stamina.ShouldFaint(false, false), "accessible food prevents faint");
            stamina.BeginFaint(); stamina.Advance(4.5, 0, false, true);
            Check(stamina.Fainted && Math.Abs(stamina.Hearts - 1.5) < 1e-8, "faint recovery is progressive");
            stamina.Advance(4.5, 0, false, true);
            Check(!stamina.Fainted && Math.Abs(stamina.Hearts - 3) < 1e-8, "faint recovery completes");
            stamina.Restore(0, 12, 0, false, 0); stamina.BeginFaint(); stamina.Advance(9, 0, false, true);
            Check(stamina.Fainted && Math.Abs(stamina.Hearts - 3) < 1e-8, "faint recovery remains one third heart per second at twelve-heart cap");
            stamina.Advance(27, 0, false, true); Check(!stamina.Fainted && stamina.Hearts == 12, "twelve hearts recover in 36 seconds");
            stamina.Restore(2, 3, 1, false, 0); stamina.Advance(1.5, 0, true, true);
            Check(Math.Abs(stamina.Hearts - 3) < 1e-8 && Math.Abs(stamina.PendingFood - 0.5) < 1e-8, "safe-area regeneration and food fill the meter together");
            stamina.Advance(2.5, 0, true, true);
            Check(Math.Abs(stamina.Hearts - 3) < 1e-8 && stamina.PendingFood == 0, "food beyond the cap is lost during safe-area regeneration");
            stamina.Restore(1, 3, 0, false, 0); stamina.Advance(3, 0, true, true);
            Check(Math.Abs(stamina.Hearts - 2) < 1e-8, "safe-area recovery is one third heart per second");
            var whole = new StaminaState(); whole.Restore(1, 3, 3, false, 0); whole.Advance(10, 400, false, true);
            var split = new StaminaState(); split.Restore(1, 3, 3, false, 0); for (int i = 0; i < 10; i++) split.Advance(1, 40, false, true);
            Check(Math.Abs(whole.Hearts - split.Hearts) < 1e-8 && Math.Abs(whole.PendingFood - split.PendingFood) < 1e-8, "food integration agrees across frame sizes");
            var boundary = new StaminaState(); boundary.Restore(0.001, 3, 0, false, 0);
            Check(Math.Abs(boundary.LimitWalkingSeconds(1, 1) - 0.5) < 1e-5, "walking is clipped at the zero-stamina boundary");
            var hungry = new StaminaState(); hungry.Restore(.15, 3, 0, false, 0);
            Check(!hungry.ShouldAutoEat(false), "no auto-eat above 0.1 hearts");
            hungry.Restore(.1, 3, 0, false, 0);
            Check(hungry.ShouldAutoEat(false) && !hungry.ShouldAutoEat(true) && !hungry.IsEmpty, "auto-eat triggers at 0.1 hearts on the trail (not at camp), before the meter is empty");
            hungry.Restore(.1, 3, 2, false, 0);
            Check(!hungry.ShouldAutoEat(false), "no second auto-eat while food is still being delivered");
            var bag = new SpatialInventory();
            Check(bag.PocketCount == 0 && bag.HookCount == 0, "starter side slots are zero");
            Check(bag.Reconfigure(14000, 4000, 10, 10, out _), "upgrade storage capacities");
            var berry = bag.AddWorld("berry", PrototypeItems.Berries, 1000);
            var axe = bag.AddWorld("axe", PrototypeItems.Tool, 1);
            var log = bag.AddWorld("log", PrototypeItems.Log, 1000);
            bag.Reserve(berry, 1); Check(bag.TryPlace(berry, 1, 0, 0, false, out _), "place pocket candidate");
            Check(!bag.Reconfigure(6000, 4000, 0, 0, out _), "capacity change refuses held item");
            Check(bag.TryMoveToSide(berry, 1, false, 9, out _), "resource goes into eligible pocket"); bag.Release(berry, 1);
            bag.Reserve(axe, 1); Check(bag.TryPlace(axe, 1, 2000, 0, false, out _), "place hook candidate");
            Check(bag.TryMoveToSide(axe, 1, true, 9, out _), "tool goes into hook"); bag.Release(axe, 1);
            bag.Reserve(log, 1); Check(bag.TryPlace(log, 1, 11000, 0, false, out _), "place item in expanded backpack"); bag.Release(log, 1);
            Check(bag.Reconfigure(6000, 4000, 0, 0, out _) && bag.Width == 6000 && bag.PocketCount == 0 && bag.HookCount == 0, "simultaneous capacity downgrade");
            Check(berry.Owner == ItemOwner.Backpack && axe.Owner == ItemOwner.Backpack && log.Owner == ItemOwner.Backpack && berry.Quantity == 1000 && log.Quantity == 1000, "downgrade conserves all displaced entries when space exists");
            Check(bag.Fits(berry, berry.Quantity, berry.X, berry.Y) && bag.Fits(axe, axe.Quantity, axe.X, axe.Y) && bag.Fits(log, log.Quantity, log.X, log.Y), "downgrade creates valid nonoverlapping placements");
            // Gear swaps (user, November 7, 2026): pocket/hook contents stay when the new count is equal or greater.
            var side = new SpatialInventory(); side.Reconfigure(6000, 4000, 2, 2, out _);
            var pocketed = side.AddWorld("pocketed", PrototypeItems.Berries, 1000); var hooked = side.AddWorld("hooked", PrototypeItems.Tool, 1);
            side.Reserve(pocketed, 1); side.TryPlace(pocketed, 1, 0, 0, false, out _); side.TryMoveToSide(pocketed, 1, false, 1, out _); side.Release(pocketed, 1);
            side.Reserve(hooked, 1); side.TryPlace(hooked, 1, 2000, 0, false, out _); side.TryMoveToSide(hooked, 1, true, 1, out _); side.Release(hooked, 1);
            Check(side.Reconfigure(6000, 4000, 2, 2, out _) && pocketed.Owner == ItemOwner.Pocket && hooked.Owner == ItemOwner.Hook, "equal pocket/hook count keeps their contents in place");
            Check(side.Reconfigure(6000, 4000, 3, 3, out _) && pocketed.Owner == ItemOwner.Pocket && pocketed.X == 1 && hooked.Owner == ItemOwner.Hook && hooked.X == 1, "greater pocket/hook count keeps their contents in place");
            Check(side.Reconfigure(6000, 4000, 1, 1, out _) && pocketed.Owner == ItemOwner.Backpack && hooked.Owner == ItemOwner.Backpack, "fewer slots move only the contents of the removed slots");
            var fullBag = new SpatialInventory();
            // Single units are ~1.3x base (stack growth 0.15): a grid of single stones plus a strip of single berries fills the bag.
            int cell = (int)Math.Ceiling(PrototypeItems.Stone.Width * ItemEntry.Scale(1)), nut = (int)Math.Ceiling(PrototypeItems.Berries.Height * ItemEntry.Scale(1)), stripX = 6000 / cell * cell;
            void Tile(string id, ItemDefinition definition, int x, int y)
            {
                var tile = fullBag.AddWorld(id, definition, 1);
                fullBag.Reserve(tile, 1); if (!fullBag.TryPlace(tile, 1, x, y, false, out _)) throw new InvalidOperationException("Test fixture"); fullBag.Release(tile, 1);
            }
            for (int y = 0; (y + 1) * cell <= 4000; y++) for (int x = 0; (x + 1) * cell <= 6000; x++) Tile("tile-" + x + "-" + y, PrototypeItems.Stone, x * cell, y * cell);
            for (int y = 0; (y + 1) * nut <= 4000 && stripX + nut <= 6000; y++) Tile("strip-" + y, PrototypeItems.Berries, stripX, y * nut);
            Check(fullBag.Reconfigure(8000, 4000, 1, 1, out _), "storage upgrade with full bag");
            var extra = fullBag.AddWorld("extra", PrototypeItems.Berries, 1000);
            fullBag.Reserve(extra, 1); Check(fullBag.TryPlace(extra, 1, 6000, 0, false, out _) && fullBag.TryMoveToSide(extra, 1, false, 0, out _), "pocket stores full stack"); fullBag.Release(extra, 1);
            Check(fullBag.Reconfigure(6000, 4000, 0, 0, out _) && extra.Owner == ItemOwner.Overflow && extra.Quantity == 1000, "unplaceable displaced stack is retained in overflow");
            fullBag.Reserve(extra, 1); Check(!fullBag.TryReturnToBackpack(extra, 1, out _) && extra.Owner == ItemOwner.Overflow, "overflow claim failure retains item"); fullBag.Release(extra, 1);
            var plan = fullBag.PlanFaintLoss("faint-1", maximum => 0);
            Check(plan.Entries.Count == 2 && plan.Entries[0].Id != plan.Entries[1].Id, "faint plan selects distinct eligible entries");
            Check(fullBag.CommitFaintLoss(plan) && !fullBag.CommitFaintLoss(plan), "faint loss commits exactly once");
            Check(extra.Owner == ItemOwner.Overflow && extra.Quantity == 1000, "faint loss excludes overflow");
            var scarce = new SpatialInventory(); var carried = scarce.AddWorld("only", PrototypeItems.Berries, 999);
            scarce.Reserve(carried, 1); if (!scarce.TryPlace(carried, 1, 0, 0, false, out _)) throw new InvalidOperationException("Test fixture"); scarce.Release(carried, 1);
            var singlePlan = scarce.PlanFaintLoss("faint-2", maximum => maximum - 1);
            Check(singlePlan.Entries.Count == 1 && singlePlan.Entries[0].Quantity == 999 && scarce.CommitFaintLoss(singlePlan) && carried.Quantity == 0, "faint with fewer eligible entries loses only those entries, including whole stacks");
            var emptyPlan = scarce.PlanFaintLoss("faint-3", maximum => 0);
            Check(emptyPlan.Entries.Count == 0 && scarce.CommitFaintLoss(emptyPlan) && !scarce.CommitFaintLoss(emptyPlan), "empty-inventory faint is committed once without invented loss");
            var stale = new SpatialInventory(); var staleItem = stale.AddWorld("candidate", PrototypeItems.Stone, 1);
            stale.Reserve(staleItem, 1); stale.TryPlace(staleItem, 1, 0, 0, false, out _); stale.Release(staleItem, 1);
            var stalePlan = stale.PlanFaintLoss("faint-4", maximum => 0); stale.AddWorld("new-world", PrototypeItems.Berries, 1);
            Check(!stale.CommitFaintLoss(stalePlan) && staleItem.Owner == ItemOwner.Backpack && staleItem.Quantity == 1, "stale faint plan cannot mutate changed inventory");
            var foodBag = new SpatialInventory(); var food = foodBag.AddWorld("food", PrototypeItems.Berries, 2); foodBag.Reserve(food, 1);
            Check(!foodBag.TryConsumeOne(food, 1, out _) && food.Quantity == 2, "world food is not accessible to eat");
            Check(foodBag.TryPlace(food, 1, 0, 0, false, out _) && foodBag.TryConsumeOne(food, 1, out _) && food.Quantity == 1, "one serving is consumed from carried food");
            Check(foodBag.TryConsumeOne(food, 1, out _) && food.Owner == ItemOwner.Consumed && !foodBag.TryConsumeOne(food, 1, out _), "last serving is consumed exactly once"); foodBag.Release(food, 1);
            var wearBag = new SpatialInventory(); var hat = wearBag.AddWorld("hat-instance", PrototypeItems.TestHat, 1, 5);
            var wearableSlots = new System.Collections.Generic.HashSet<GearSlot>();
            foreach (ItemDefinition definition in PrototypeItems.TestWearables)
                if (!definition.Stackable && definition.GearSlot.HasValue && definition.GearRank == 4) wearableSlots.Add(definition.GearSlot.Value);
            Check(wearableSlots.Count == 5, "all five wearable test definitions are unique unstackable slots");
            wearBag.Reserve(hat, 1); Check(wearBag.TryPlace(hat, 1, 0, 0, false, out _) && wearBag.TryWear(hat, 1, out _) && hat.Owner == ItemOwner.Worn, "spare wearable transfers to a worn owner"); wearBag.Release(hat, 1);
            var protectedPlan = wearBag.PlanFaintLoss("faint-protected", maximum => 0);
            Check(protectedPlan.Entries.Count == 0 && hat.Owner == ItemOwner.Worn, "worn gear is excluded from faint loss");
            Check(wearBag.TakeOff(hat.Id, 123.25) && hat.Owner == ItemOwner.Overflow && hat.HasWearRemaining && Math.Abs(hat.WearRemaining - 123.25) < 1e-8, "taking off retains exact durability in overflow");
            wearBag.Reserve(hat, 1);
            Check(wearBag.TryReturnToBackpack(hat, 1, out _) && wearBag.TryWear(hat, 1, out _) && Math.Abs(hat.WearRemaining - 123.25) < 1e-8, "spare can be reclaimed and re-equipped without restoring wear"); wearBag.Release(hat, 1);
            Check(wearBag.ExpireWorn(hat.Id) && hat.Owner == ItemOwner.Consumed && !wearBag.ExpireWorn(hat.Id), "expired worn instance disappears exactly once");
            return count;
        }
    }
}
