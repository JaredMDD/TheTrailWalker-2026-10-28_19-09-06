using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Input;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    public static class Milestone2Validation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool value, string name) { if (!value) throw new Exception("Milestone 2: " + name); passed.Add(name); }
        private static bool Rejects(Action action) { try { action(); return false; } catch (ArgumentException) { return true; } }
        private static void Put(SpatialInventory bag, ItemEntry item, int x, int y)
        { if (!bag.Reserve(item, 99) || !bag.TryPlace(item, 99, x, y, false, out _)) throw new Exception("Invalid validation fixture"); bag.Release(item, 99); }
        [MenuItem("The Trail Walker/Milestone 2/2 - Validate Inventory Rules")]
        public static void Run()
        {
            passed.Clear();
            var safe = new Rect(0, 80, 1080, 2160);
            Check(GestureRouter.IsSystemGestureArea(2300, 2400, safe), "Top system gesture region excluded");
            Check(!GestureRouter.IsSystemGestureArea(2000, 2400, safe), "Touch area below top exclusion remains usable");
            Check(GestureRouter.IsSpeedArea(new Vector2(540, 836), safe) && !GestureRouter.IsSpeedArea(new Vector2(100, 836), safe) && !GestureRouter.IsSpeedArea(new Vector2(540, 2000), safe), "Centre-line speed band is distinct from world surface and debug buttons");
            Check(CollectionPrototype.InVisibleView(new Vector3(0.5f, 0.5f, 100), 0.3f, 200), "Visible unobstructed targets can be considered beyond old eight-metre radius");
            Check(!CollectionPrototype.InVisibleView(new Vector3(0.5f, 0.5f, -1), 0.3f, 200) && !CollectionPrototype.InVisibleView(new Vector3(1.1f, 0.5f, 30), 0.3f, 200), "Behind-camera and offscreen targets rejected");
            var bag = new SpatialInventory();
            Check(Rejects(() => bag.AddWorld("zero", PrototypeItems.Berries, 0)), "Zero quantity rejected");
            Check(Rejects(() => bag.AddWorld("overflow", PrototypeItems.Berries, 1001)), "1001 stack rejected");
            Check(Rejects(() => bag.AddWorld("tools", PrototypeItems.Tool, 2)), "Tools never stack");
            Check(Rejects(() => bag.AddWorld("bad-q", PrototypeItems.Stone, 1, 6)), "Quality above Q5 rejected");
            Check(Rejects(() => bag.AddWorld("bad-q0", PrototypeItems.Stone, 1, 0)), "Quality below Q1 rejected");
            var one = bag.AddWorld("one", PrototypeItems.Berries, 1);
            var almost = bag.AddWorld("almost", PrototypeItems.Berries, 999);
            var full = bag.AddWorld("full", PrototypeItems.Berries, 1000);
            Check(one.Width == 800 && full.Width == 1200 && almost.Width <= 1200, "Quantities 1/999/1000 share bounded linear growth");
            Check(Math.Abs(ItemEntry.Scale(1000) - 1.5) < 0.000001, "Full stack scale exactly 1.5, area at most 2.25");
            Check(Rejects(() => bag.AddWorld("one", PrototypeItems.Stone, 1)), "Duplicate identity rejected");
            Check(bag.Reserve(one, 1) && !bag.Reserve(one, 2), "Two fingers cannot reserve one item");
            Check(!bag.TryQuickStore(one, 2, out _) && one.Owner == ItemOwner.World, "Wrong pointer cannot transfer");
            bag.Release(one, 2); Check(bag.ReservedBy(one, 1), "Wrong pointer cannot release reservation");
            Check(bag.TryQuickStore(one, 1, out _) && one.Owner == ItemOwner.Backpack, "Quick storage commits ownership once");
            Check(!bag.TryQuickStore(one, 1, out _) && one.Quantity == 1, "Repeated pickup cannot duplicate");
            Check(!bag.TryDropToTrail(one, 2, 5f, out _) && one.Owner == ItemOwner.Backpack, "Wrong pointer cannot drop an item");
            Check(bag.TryDropToTrail(one, 1, 5f, out _) && one.Owner == ItemOwner.World && one.TrailDistance == 5f && one.Quantity == 1, "Drop pocket returns the whole entry to the trail");
            Check(!bag.TryDropToTrail(one, 1, 5f, out _), "A trail item cannot be dropped again");
            Check(bag.TryQuickStore(one, 1, out _) && one.Owner == ItemOwner.Backpack && one.Quantity == 1, "Dropped entry can be picked up again without duplication");
            var worn = new SpatialInventory(); var spare = worn.AddWorld("spare", PrototypeItems.TestBoots, 1, 4, new ItemProfile(130, 180, 180));
            worn.Reserve(spare, 7); worn.TryQuickStore(spare, 7, out _); worn.RestoreWear(spare, true, 61.5); worn.TryDropToTrail(spare, 7, 3f, out _); worn.TryQuickStore(spare, 7, out _); worn.Release(spare, 7);
            Check(spare.Owner == ItemOwner.Backpack && spare.Quality == 4 && spare.Profile.UnitValue == 130 && spare.Profile.MaxDurability == 180 && Math.Abs(spare.WearRemaining - 61.5) < 1e-9, "Re-picked drop keeps quality, profile and remaining wear");
            var cycle = new SpatialInventory(); var kept = cycle.AddWorld("kept", PrototypeItems.Stone, 1000); var tossed = cycle.AddWorld("tossed", PrototypeItems.Log, 700, 3);
            cycle.Reserve(tossed, 6); cycle.TryQuickStore(tossed, 6, out _); cycle.TryDropToTrail(tossed, 6, 2f, out _); cycle.Release(tossed, 6);
            Check(cycle.RemoveUncollectedDrops() == 1 && tossed.Owner == ItemOwner.Consumed && kept.Owner == ItemOwner.World && kept.Quantity == 1000, "New drop cycle removes only uncollected player drops");
            bag.Release(one, 1);
            Put(bag, almost, 1500, 0); bag.Reserve(one, 1);
            Check(bag.TryMerge(one, almost, 1, out _) && almost.Quantity == 1000 && one.Quantity == 0 && one.Owner == ItemOwner.Consumed, "1 plus 999 consumes source exactly once");
            Check(!bag.TryMerge(one, almost, 1, out _), "Consumed source cannot merge twice"); bag.Release(one, 1);
            Check(!bag.Fits(full, 1000, -1, 0) && !bag.Fits(full, 1000, 5000, 0), "Bounds reject negative and protruding footprints");
            Check(!bag.Fits(full, 1000, almost.X, almost.Y), "Overlap rejected");
            bag.Reserve(full, 3); Check(bag.TryQuickStore(full, 3, out _) && full.Owner == ItemOwner.Backpack, "Full compatible stack gets separate placement"); bag.Release(full, 3);
            var q2 = bag.AddWorld("q2", PrototypeItems.Berries, 1, 2);
            var priced = bag.AddWorld("priced", PrototypeItems.Berries, 1, 1, new ItemProfile(200));
            var pending = bag.AddWorld("pending", PrototypeItems.Berries, 1, ready: false);
            Check(!q2.CanMerge(full) && !priced.CanMerge(full) && !pending.CanMerge(full), "Quality, functional price profile and readiness partition stacks");
            var same = bag.AddWorld("equivalent", PrototypeItems.Berries, 1);
            Check(same.CanMerge(full), "Equivalent profiles with distinct identities can merge");
            var axe1 = bag.AddWorld("axe1", PrototypeItems.Tool, 1); var axe2 = bag.AddWorld("axe2", PrototypeItems.Tool, 1);
            Check(!axe1.CanMerge(axe2) && axe1.Id != axe2.Id && axe1.Width == 1800, "Equipment remains unique with fixed footprint");
            var partialBag = new SpatialInventory(); var target = partialBag.AddWorld("target", PrototypeItems.Berries, 999); Put(partialBag, target, 0, 0);
            var source = partialBag.AddWorld("source", PrototypeItems.Berries, 1000); partialBag.Reserve(source, 4);
            Check(partialBag.TryQuickStore(source, 4, out _) && target.Quantity == 1000 && source.Quantity == 999 && source.Owner == ItemOwner.World, "Partial merge retains world remainder");
            partialBag.Release(source, 4);
            var small = new SpatialInventory(1000, 1000); var smallTarget = small.AddWorld("tiny-target", PrototypeItems.Berries, 1); Put(small, smallTarget, 0, 0);
            var largeSource = small.AddWorld("large-source", PrototypeItems.Berries, 999); small.Reserve(largeSource, 1);
            Check(!small.TryMerge(largeSource, smallTarget, 1, out _) && smallTarget.Quantity == 1 && largeSource.Quantity == 999, "Merge growth beyond boundary leaves both unchanged");
            var crowded = new SpatialInventory(); var grows = crowded.AddWorld("grows", PrototypeItems.Berries, 1); Put(crowded, grows, 0, 0);
            var neighbor = crowded.AddWorld("neighbor", PrototypeItems.Stone, 1); Put(crowded, neighbor, 800, 0);
            var growthSource = crowded.AddWorld("growth-source", PrototypeItems.Berries, 999); crowded.Reserve(growthSource, 1);
            Check(!crowded.TryMerge(growthSource, grows, 1, out _) && grows.Quantity == 1 && growthSource.Quantity == 999, "Merge growth cannot overlap neighbor");
            crowded.Release(growthSource, 1); crowded.Reserve(grows, 2);
            Check(!crowded.TryPlace(grows, 2, 800, 0, false, out _) && grows.X == 0, "Rejected bag move retains original position");
            Check(crowded.TryPlace(grows, 2, 1234, 2345, false, out _) && grows.X == 1234 && grows.Y == 2345, "Free placement supports fixed precision without slot snapping");
            var touchBag = new SpatialInventory(); var touchTarget = touchBag.AddWorld("touch-target", PrototypeItems.Berries, 1); Put(touchBag, touchTarget, 0, 0);
            var touchSource = touchBag.AddWorld("touch-source", PrototypeItems.Berries, 1); touchBag.Reserve(touchSource, 1);
            Check(touchBag.TryPlace(touchSource, 1, 800, 0, true, out _) && touchTarget.Quantity == 2, "Touching compatible footprints merge");
            var fullBag = new SpatialInventory();
            for (int y = 0; y < 4; y++) for (int x = 0; x < 6; x++) Put(fullBag, fullBag.AddWorld(x + ":" + y, PrototypeItems.Stone, 1, 5), x * 1000, y * 1000);
            var refused = fullBag.AddWorld("refused", PrototypeItems.Branch, 1000); fullBag.Reserve(refused, 1);
            Check(!fullBag.TryQuickStore(refused, 1, out _) && refused.Owner == ItemOwner.World && refused.Quantity == 1000, "Full bag rejection preserves full world stack");
            var busy = new SpatialInventory(); var busyTarget = busy.AddWorld("busy-target", PrototypeItems.Berries, 1); Put(busy, busyTarget, 0, 0);
            var busySource = busy.AddWorld("busy-source", PrototypeItems.Berries, 1); busy.Reserve(busyTarget, 1); busy.Reserve(busySource, 2);
            Check(!busy.TryMerge(busySource, busyTarget, 2, out _), "Reserved merge target cannot be modified by a second pointer");
            var rng = new System.Random(271828); var randomBag = new SpatialInventory(); long originalTotal = 0;
            for (int i = 0; i < 100; i++)
            {
                var def = PrototypeItems.All[rng.Next(PrototypeItems.All.Length)]; int quantity = def.Stackable ? rng.Next(1, 1001) : 1;
                randomBag.AddWorld("random-" + i, def, quantity, rng.Next(1, 6)); originalTotal += quantity;
            }
            for (int step = 0; step < 1000; step++)
            {
                var item = randomBag.Entries[rng.Next(randomBag.Entries.Count)];
                if (randomBag.Reserve(item, 1))
                {
                    if (item.Owner == ItemOwner.World) randomBag.TryQuickStore(item, 1, out _);
                    else randomBag.TryPlace(item, 1, rng.Next(-200, 6100), rng.Next(-200, 4100), true, out _);
                    randomBag.Release(item, 1);
                }
                long sum = 0;
                foreach (ItemEntry entry in randomBag.Entries)
                {
                    sum += entry.Quantity;
                    if (entry.Owner == ItemOwner.Backpack && !randomBag.Fits(entry, entry.Quantity, entry.X, entry.Y)) throw new Exception("Randomized fit invariant failed");
                    if ((entry.Owner == ItemOwner.Consumed) != (entry.Quantity == 0)) throw new Exception("Randomized ownership invariant failed");
                }
                if (sum != originalTotal) throw new Exception("Randomized quantity conservation failed");
            }
            Check(true, "1,000 seeded transfers/moves preserve quantities, unique ownership, bounds and nonoverlap");
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Milestone2-checks.txt", passed);
            Debug.Log($"[Milestone2] PASS: {passed.Count} inventory checks, including 1,000 randomized operations.");
        }
    }
}
