using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.Domain;
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
        // Retest flag for section 7.1.1: new tree raw/refined/prepared definitions follow the existing item rules.
        private static void TreeFoodCatalogChecks()
        {
            var ids = new HashSet<string>(); bool unique = true;
            foreach (ItemDefinition definition in PrototypeItems.All) unique &= ids.Add(definition.Id);
            Check(unique, "Catalog item IDs are unique, including tree food items");
            bool rawOk = true;
            foreach (ItemDefinition raw in PrototypeItems.TreeIngredients) rawOk &= raw.Category == ItemCategory.RawIngredient && raw.Stackable && raw.FoodHearts == 0 && !raw.PreparedFood;
            Check(rawOk, "Tree drops are raw recipe ingredients, not food");
            bool refinedOk = true;
            foreach (ItemDefinition refined in PrototypeItems.TreeRefinements) refinedOk &= refined.Category == ItemCategory.RefinedIngredient && refined.FoodHearts == 0 && !refined.PreparedFood;
            Check(refinedOk, "Mesquite flour and roasted acorn meal are refined ingredients, not food");
            int[] hearts = { 2, 3, 4, 5 }; bool dishesOk = true;
            for (int i = 0; i < hearts.Length; i++) dishesOk &= PrototypeItems.TreeDishes[i].Category == ItemCategory.PreparedFood && PrototypeItems.TreeDishes[i].PreparedFood && PrototypeItems.TreeDishes[i].FoodHearts == hearts[i];
            Check(dishesOk, "Tree dishes are prepared food worth 2/3/4/5 hearts per serving");
            Check(PrototypeItems.MesquitePods.Id != PrototypeItems.MesquiteFlour.Id && PrototypeItems.Acorns.Id != PrototypeItems.RoastedAcornMeal.Id && PrototypeItems.PineNuts.Id != PrototypeItems.ToastedPineNuts.Id && PrototypeItems.BirchSap.Id != PrototypeItems.BirchSapMilkDrink.Id,
                "Raw, refined and cooked forms keep distinct IDs (no substitution)");
            Check(Rejects(() => new ItemDefinition("bad.category", "Bad", 800, 800, foodHearts: 2, preparedFood: true, category: ItemCategory.RawIngredient)), "A definition cannot claim a category that contradicts its food data");
            var bag = new SpatialInventory();
            ItemEntry nuts = bag.AddWorld("tf-nuts", PrototypeItems.PineNuts, 400, 1); ItemEntry trailNuts = bag.AddWorld("tf-trail-nuts", PrototypeItems.PineNuts, 1000, 1);
            ItemEntry otherQuality = bag.AddWorld("tf-nuts-q2", PrototypeItems.PineNuts, 1, 2); ItemEntry acorns = bag.AddWorld("tf-acorns", PrototypeItems.Acorns, 1, 1);
            ItemEntry dish = bag.AddWorld("tf-dish", PrototypeItems.AcornBiscuits, 999, 5); ItemEntry pending = bag.AddWorld("tf-dish-pending", PrototypeItems.AcornBiscuits, 1, 5, null, false);
            Check(Rejects(() => bag.AddWorld("tf-over", PrototypeItems.MesquiteFlour, 1001)) && bag.AddWorld("tf-full", PrototypeItems.MesquiteFlour, 1000).Quantity == 1000, "Tree food stacks accept 1 to 1,000 units only");
            Check(nuts.CanMerge(trailNuts) && nuts.CanMerge(otherQuality) && !nuts.CanMerge(acorns) && !dish.CanMerge(pending), "Stacks combine by item ID regardless of quality; a different ID or readiness stays separate");
            Put(bag, nuts, 0, 0); bag.Reserve(trailNuts, 8);
            Check(bag.TryQuickStore(trailNuts, 8, out _) && nuts.Quantity == 1000 && trailNuts.Quantity == 400 && trailNuts.Owner == ItemOwner.Backpack, "Topping up a partial ingredient stack stores the exact remainder as a new stack");
            bag.Release(trailNuts, 8);
            Put(bag, dish, 2000, 0); Put(bag, pending, 4000, 0);
            bag.Reserve(nuts, 9); Check(!bag.TryConsumeOne(nuts, 9, out _) && nuts.Quantity == 1000, "Raw ingredients cannot be eaten"); bag.Release(nuts, 9);
            bag.Reserve(pending, 9); Check(!bag.TryConsumeOne(pending, 9, out _), "A pending dish cannot be eaten"); bag.Release(pending, 9);
            bag.Reserve(dish, 9); Check(bag.TryConsumeOne(dish, 9, out _) && dish.Quantity == 998, "A ready dish is eaten one serving at a time"); bag.Release(dish, 9);
            // Piles: one stack per item ID tracks quantity by quality and is eaten from the lowest quality.
            var piles = new SpatialInventory();
            ItemEntry biscuits = piles.AddWorld("pile-q5", PrototypeItems.AcornBiscuits, 3, 5); Put(piles, biscuits, 0, 0);
            ItemEntry cheaper = piles.AddWorld("pile-q2", PrototypeItems.AcornBiscuits, 2, 2); ItemEntry middle = piles.AddWorld("pile-q3", PrototypeItems.AcornBiscuits, 1, 3);
            piles.Reserve(cheaper, 10); piles.TryQuickStore(cheaper, 10, out _); piles.Release(cheaper, 10);
            piles.Reserve(middle, 10); piles.TryQuickStore(middle, 10, out _); piles.Release(middle, 10);
            Check(biscuits.Quantity == 6 && biscuits.Pile.Count == 3 && biscuits.Pile[0].Quality == 2 && biscuits.Pile[0].Quantity == 2 && biscuits.Pile[2].Quality == 5 && biscuits.QualityLabel == "Q2-Q5" && cheaper.Owner == ItemOwner.Consumed,
                "Different qualities join one stack as a pile ordered from lowest quality");
            piles.Reserve(biscuits, 11);
            piles.TryConsumeLowest(biscuits, 11, out PilePortion first, out _); piles.TryConsumeLowest(biscuits, 11, out PilePortion second, out _); piles.TryConsumeLowest(biscuits, 11, out PilePortion third, out _);
            piles.Release(biscuits, 11);
            Check(first.Quality == 2 && second.Quality == 2 && third.Quality == 3 && biscuits.Quantity == 3 && biscuits.Pile.Count == 1 && biscuits.Quality == 5, "Edibles are eaten lowest quality first");
            ItemEntry topped = piles.AddWorld("pile-top", PrototypeItems.PineNuts, 900, 1); Put(piles, topped, 2000, 0);
            ItemEntry bigPickup = piles.AddWorld("pile-pickup", PrototypeItems.PineNuts, 1000, 4); piles.Reserve(bigPickup, 12);
            Check(piles.TryQuickStore(bigPickup, 12, out _) && topped.Quantity == 1000 && bigPickup.Quantity == 900 && bigPickup.Owner == ItemOwner.Backpack && topped.HighestQuality == 4 && bigPickup.Quality == 4,
                "A pickup gives all its units: top-up first, remainder as a new stack, with qualities tracked");
            piles.Release(bigPickup, 12);
            // Device case: a small Q3 stack wedged against a neighbour must still be filled by a Q1 pickup.
            var wedged = new SpatialInventory();
            ItemEntry milk = wedged.AddWorld("milk-q3", PrototypeItems.BirchSapMilkDrink, 3, 3); Put(wedged, milk, 1300, 0);
            ItemEntry neighbour = wedged.AddWorld("cakes", PrototypeItems.MesquitePodCakes, 5, 4); Put(wedged, neighbour, 2300, 0);
            ItemEntry milkPickup = wedged.AddWorld("milk-q1", PrototypeItems.BirchSapMilkDrink, 1000, 1); wedged.Reserve(milkPickup, 13);
            Check(wedged.TryQuickStore(milkPickup, 13, out _) && milk.Quantity == 1000 && milk.Pile[0].Quality == 1 && milk.HighestQuality == 3 && milkPickup.Quantity == 3 && milkPickup.Owner == ItemOwner.Backpack
                && neighbour.X == 2300 && neighbour.Y == 0 && !SpatialInventory.Overlaps(milk.X, milk.Y, milk.Width, milk.Height, neighbour.X, neighbour.Y, neighbour.Width, neighbour.Height),
                "A stack that cannot grow in place moves to free space and is filled; neighbours stay put");
            wedged.Release(milkPickup, 13);
        }
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
            Check(GestureRouter.IsBlocked(false, 1, false, MovementPauseReasons.None) && !GestureRouter.IsBlocked(false, 1, true, MovementPauseReasons.None)
                && GestureRouter.IsBlocked(false, 1, true, MovementPauseReasons.Suspended) && GestureRouter.IsBlocked(true, 0, true, MovementPauseReasons.None),
                "An open bag blocks world touches unless the screen opened it beside world pickups (lumber piles)");
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
            int berryWidth = PrototypeItems.Berries.Width;
            Check(one.Width == (int)Math.Ceiling(berryWidth * ItemEntry.Scale(1)) && full.Width == (int)Math.Ceiling(berryWidth * 1.5) && one.Width < almost.Width && almost.Width <= full.Width, "Quantities 1/999/1000 share bounded growth up to the 1.5x full stack");
            Check(Math.Abs(ItemEntry.Scale(1000) - 1.5) < 0.000001 && Math.Abs(ItemEntry.Scale(1) - 1.5 / 1.15) < 0.000001 && ItemEntry.Scale(1) < ItemEntry.Scale(10) && ItemEntry.Scale(10) < ItemEntry.Scale(1000), "Full stack scale exactly 1.5 (area at most 2.25); single items about 1.3x; size still grows with quantity");
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
            Check(!bag.Fits(full, 1000, -1, 0) && !bag.Fits(full, 1000, bag.Width - full.Width + 1, 0), "Bounds reject negative and protruding footprints");
            Check(!bag.Fits(full, 1000, almost.X, almost.Y), "Overlap rejected");
            bag.Reserve(full, 3); Check(bag.TryQuickStore(full, 3, out _) && full.Owner == ItemOwner.Backpack, "Full compatible stack gets separate placement"); bag.Release(full, 3);
            var q2 = bag.AddWorld("q2", PrototypeItems.Berries, 1, 2);
            var priced = bag.AddWorld("priced", PrototypeItems.Berries, 1, 1, new ItemProfile(200));
            var pending = bag.AddWorld("pending", PrototypeItems.Berries, 1, ready: false);
            Check(q2.CanMerge(full) && priced.CanMerge(full) && !pending.CanMerge(full), "Quality and price profile share one stack pile; readiness still separates stacks");
            var same = bag.AddWorld("equivalent", PrototypeItems.Berries, 1);
            Check(same.CanMerge(full), "Equivalent profiles with distinct identities can merge");
            var axe1 = bag.AddWorld("axe1", PrototypeItems.Tool, 1); var axe2 = bag.AddWorld("axe2", PrototypeItems.Tool, 1);
            Check(!axe1.CanMerge(axe2) && axe1.Id != axe2.Id && axe1.Width == PrototypeItems.Tool.Width && axe1.Height == PrototypeItems.Tool.Height && axe2.Width == axe1.Width, "Equipment remains unique with fixed footprint");
            var partialBag = new SpatialInventory(); var target = partialBag.AddWorld("target", PrototypeItems.Berries, 999); Put(partialBag, target, 0, 0);
            var source = partialBag.AddWorld("source", PrototypeItems.Berries, 1000); partialBag.Reserve(source, 4);
            Check(partialBag.TryQuickStore(source, 4, out _) && target.Quantity == 1000 && source.Quantity == 999 && source.Owner == ItemOwner.Backpack, "A pickup tops up the partial stack and stores the rest as a new stack");
            partialBag.Release(source, 4);
            var small = new SpatialInventory(PrototypeItems.Berries.Width * 7 / 5, PrototypeItems.Berries.Height * 7 / 5); var smallTarget = small.AddWorld("tiny-target", PrototypeItems.Berries, 1); Put(small, smallTarget, 0, 0);
            var largeSource = small.AddWorld("large-source", PrototypeItems.Berries, 999); small.Reserve(largeSource, 1);
            Check(small.TryMerge(largeSource, smallTarget, 1, out _) && smallTarget.Quantity > 1 && smallTarget.Quantity < 1000 && smallTarget.Quantity + largeSource.Quantity == 1000 && small.Fits(smallTarget, smallTarget.Quantity, smallTarget.X, smallTarget.Y),
                "A stack with no room to grow is topped up only as far as it fits; quantities are conserved");
            var crowded = new SpatialInventory(); var grows = crowded.AddWorld("grows", PrototypeItems.Berries, 1); Put(crowded, grows, 0, 0);
            var neighbor = crowded.AddWorld("neighbor", PrototypeItems.Stone, 1); Put(crowded, neighbor, 800, 0);
            var growthSource = crowded.AddWorld("growth-source", PrototypeItems.Berries, 999); crowded.Reserve(growthSource, 1);
            Check(crowded.TryMerge(growthSource, grows, 1, out _) && grows.Quantity == 1000 && growthSource.Quantity == 0 && neighbor.X == 800 && neighbor.Y == 0 && !SpatialInventory.Overlaps(grows.X, grows.Y, grows.Width, grows.Height, neighbor.X, neighbor.Y, neighbor.Width, neighbor.Height),
                "Merge growth never overlaps a neighbour: the grown stack moves to free space");
            crowded.Release(growthSource, 1); crowded.Reserve(grows, 2); int grownX = grows.X, grownY = grows.Y;
            Check(!crowded.TryPlace(grows, 2, 800, 0, false, out _) && grows.X == grownX && grows.Y == grownY, "Rejected bag move retains original position");
            Check(crowded.TryPlace(grows, 2, 1234, 2345, false, out _) && grows.X == 1234 && grows.Y == 2345, "Free placement supports fixed precision without slot snapping");
            var touchBag = new SpatialInventory(); var touchTarget = touchBag.AddWorld("touch-target", PrototypeItems.Berries, 1); Put(touchBag, touchTarget, 0, 0);
            var touchSource = touchBag.AddWorld("touch-source", PrototypeItems.Berries, 1); touchBag.Reserve(touchSource, 1);
            Check(touchBag.TryPlace(touchSource, 1, touchTarget.Width, 0, true, out _) && touchTarget.Quantity == 2, "Touching compatible footprints merge");
            var fullBag = new SpatialInventory();
            int cell = (int)Math.Ceiling(PrototypeItems.Stone.Width * ItemEntry.Scale(1)); for (int y = 0; (y + 1) * cell <= fullBag.Height; y++) for (int x = 0; (x + 1) * cell <= fullBag.Width; x++) Put(fullBag, fullBag.AddWorld(x + ":" + y, PrototypeItems.Stone, 1, 5), x * cell, y * cell);
            // Single stones are now ~1.3x base, so single berries fill the strip the stone grid leaves (no room for a 1,000-branch stack).
            int nut = (int)Math.Ceiling(PrototypeItems.Berries.Height * ItemEntry.Scale(1)); int stripX = fullBag.Width / cell * cell;
            for (int y = 0; (y + 1) * nut <= fullBag.Height && stripX + nut <= fullBag.Width; y++) Put(fullBag, fullBag.AddWorld("strip:" + y, PrototypeItems.Berries, 1, 5), stripX, y * nut);
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
            TreeFoodCatalogChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Milestone2-checks.txt", passed);
            Debug.Log($"[Milestone2] PASS: {passed.Count} inventory checks, including 1,000 randomized operations.");
        }
    }
}
