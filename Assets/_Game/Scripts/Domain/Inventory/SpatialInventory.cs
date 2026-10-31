using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Inventory
{
    // Coordinates are thousandths of a logical unit. No physics, rotations or weight budget.
    public sealed class SpatialInventory
    {
        public sealed class LossEntry
        {
            public string Id { get; internal set; }
            public int Quantity { get; internal set; }
            public ItemOwner Owner { get; internal set; }
        }
        public sealed class LossPlan
        {
            public string EventId { get; internal set; }
            public int InventoryRevision { get; internal set; }
            public IReadOnlyList<LossEntry> Entries { get; internal set; }
        }
        private readonly List<ItemEntry> entries = new List<ItemEntry>();
        private readonly Dictionary<string, ItemEntry> identities = new Dictionary<string, ItemEntry>();
        private readonly Dictionary<string, int> reservations = new Dictionary<string, int>();
        public IReadOnlyList<ItemEntry> Entries => entries;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PocketCount { get; private set; }
        public int HookCount { get; private set; }
        public int Revision { get; private set; }
        public SpatialInventory(int width = 6000, int height = 4000)
        { if (width <= 0 || height <= 0) throw new ArgumentException("Invalid container"); Width = width; Height = height; }
        public ItemEntry AddWorld(string id, ItemDefinition definition, int quantity, int quality = 1, ItemProfile? profile = null, bool ready = true, float distance = 0)
        {
            var item = new ItemEntry(id, definition, quantity, quality, profile ?? new ItemProfile(100), ready, distance);
            identities.Add(id, item); entries.Add(item); Revision++; return item;
        }
        public bool Reserve(ItemEntry item, int pointer)
        {
            if (!Owns(item) || item.Owner == ItemOwner.Consumed || reservations.ContainsKey(item.Id)) return false;
            reservations.Add(item.Id, pointer); return true;
        }
        public void Release(ItemEntry item, int pointer) { if (ReservedBy(item, pointer)) reservations.Remove(item.Id); }
        public bool ReservedBy(ItemEntry item, int pointer) => item != null && reservations.TryGetValue(item.Id, out int owner) && owner == pointer;
        private bool Owns(ItemEntry item) => item != null && identities.TryGetValue(item.Id, out ItemEntry existing) && ReferenceEquals(existing, item);
        public bool Fits(ItemEntry item, int quantity, int x, int y, ItemEntry ignoredSource = null)
        {
            int w = item.WidthFor(quantity), h = item.HeightFor(quantity);
            if (x < 0 || y < 0 || w > Width || h > Height || x > Width - w || y > Height - h) return false;
            foreach (ItemEntry other in entries)
                if (other.Owner == ItemOwner.Backpack && other != item && other != ignoredSource && Overlaps(x, y, w, h, other.X, other.Y, other.Width, other.Height)) return false;
            return true;
        }
        public static bool Overlaps(int x, int y, int w, int h, int ox, int oy, int ow, int oh) => x < ox + ow && x + w > ox && y < oy + oh && y + h > oy;
        private static bool Touches(int x, int y, int w, int h, ItemEntry other) => x <= other.X + other.Width && x + w >= other.X && y <= other.Y + other.Height && y + h >= other.Y;
        public bool TryPlace(ItemEntry item, int pointer, int x, int y, bool merge, out string reason)
        {
            reason = "Item is already in use";
            if (!Owns(item) || !ReservedBy(item, pointer) || item.Owner == ItemOwner.Consumed) return false;
            if (merge)
                foreach (ItemEntry other in entries)
                    if (other.Owner == ItemOwner.Backpack && other.Quantity < 1000 && item.CanMerge(other) && Touches(x, y, item.Width, item.Height, other))
                    {
                        if (!TryMerge(item, other, pointer, out reason)) return false;
                        // A pickup gives all of its units: any trail remainder becomes its own stack when space allows.
                        if (item.Owner == ItemOwner.World && item.Quantity > 0)
                            reason = TryPlaceFree(item, pointer, out _) ? "Merged and stored the rest as a new stack" : "Merged; no space for the rest, which stays on the trail";
                        return true;
                    }
            if (!Fits(item, item.Quantity, x, y)) { reason = "No space: item stays where it was"; return false; }
            item.X = x; item.Y = y; item.Owner = ItemOwner.Backpack; Revision++; reason = "Stored " + item.Definition.Name; return true;
        }
        public bool TryMerge(ItemEntry source, ItemEntry target, int pointer, out string reason)
        {
            reason = "Stacks differ or target is busy";
            if (!Owns(source) || !Owns(target) || !ReservedBy(source, pointer) || source.Owner == ItemOwner.Consumed || target.Owner != ItemOwner.Backpack || reservations.ContainsKey(target.Id) || !source.CanMerge(target)) return false;
            int moved = Math.Min(source.Quantity, 1000 - target.Quantity);
            if (moved <= 0) { reason = "Stack already has 1,000"; return false; }
            // A partial source remains in its original position and still blocks growth.
            ItemEntry ignore = moved == source.Quantity ? source : null;
            if (!Fits(target, target.Quantity + moved, target.X, target.Y, ignore))
            {
                // The grown stack does not fit in place: move just that stack to free space, otherwise top up only as far as it fits.
                if (TryFindSpot(target, target.Quantity + moved, ignore, out int freeX, out int freeY)) { target.X = freeX; target.Y = freeY; }
                else
                {
                    int lo = 0, hi = moved - 1;
                    while (lo < hi) { int mid = (lo + hi + 1) / 2; if (Fits(target, target.Quantity + mid, target.X, target.Y)) lo = mid; else hi = mid - 1; }
                    if (lo == 0) { reason = "Larger stack would overlap or leave the bag"; return false; }
                    moved = lo;
                }
            }
            source.MoveLowestTo(target, moved);
            if (source.Quantity == 0) source.Owner = ItemOwner.Consumed;
            Revision++; reason = "Merged " + moved + (source.Quantity > 0 ? "; remainder stays at source" : ""); return true;
        }
        public bool TryQuickStore(ItemEntry item, int pointer, out string reason)
        {
            reason = "Item is already in use";
            if (!Owns(item) || !ReservedBy(item, pointer) || item.Owner != ItemOwner.World) return false;
            // Top up every compatible stack first, then store whatever is left as a new stack: a pickup gives all its units.
            bool merged = false;
            foreach (ItemEntry other in entries)
            {
                if (other.Owner != ItemOwner.Backpack || other.Quantity >= 1000 || !item.CanMerge(other)) continue;
                if (TryMerge(item, other, pointer, out reason)) merged = true;
                if (item.Owner == ItemOwner.Consumed) { reason = "Stored " + item.Definition.Name; return true; }
            }
            if (TryPlaceFree(item, pointer, out reason)) { if (merged) reason = "Stored " + item.Definition.Name + "; the rest started a new stack"; return true; }
            reason = merged ? "Backpack full: the rest of the pickup remains on the trail" : "Backpack full: pickup remains on the trail"; return merged;
        }
        private bool TryFindSpot(ItemEntry item, int quantity, ItemEntry ignoredSource, out int spotX, out int spotY)
        {
            var xs = new SortedSet<int> { 0 }; var ys = new SortedSet<int> { 0 };
            foreach (ItemEntry other in entries)
                if (other.Owner == ItemOwner.Backpack && other != item && other != ignoredSource) { xs.Add(other.X + other.Width); ys.Add(other.Y + other.Height); }
            foreach (int y in ys) foreach (int x in xs)
                if (Fits(item, quantity, x, y, ignoredSource)) { spotX = x; spotY = y; return true; }
            spotX = spotY = 0; return false;
        }
        // Candidate edges give a deterministic bottom-left placement without visible slots.
        private bool TryPlaceFree(ItemEntry item, int pointer, out string reason)
        {
            var xs = new SortedSet<int> { 0 }; var ys = new SortedSet<int> { 0 };
            foreach (ItemEntry other in entries)
                if (other.Owner == ItemOwner.Backpack) { xs.Add(other.X + other.Width); ys.Add(other.Y + other.Height); }
            foreach (int y in ys) foreach (int x in xs)
                if (Fits(item, item.Quantity, x, y)) return TryPlace(item, pointer, x, y, false, out reason);
            reason = "No free backpack space"; return false;
        }
        // Lowest quality is used first; the removed portion is reported for callers that need its quality.
        public bool TryConsumeLowest(ItemEntry item, int pointer, out PilePortion eaten, out string reason)
        {
            eaten = default; reason = "No accessible ready food";
            if (!Owns(item) || !ReservedBy(item, pointer) || (item.Owner != ItemOwner.Backpack && item.Owner != ItemOwner.Pocket) || !item.Ready || item.Definition.FoodHearts <= 0) return false;
            eaten = item.TakeLowest(1)[0]; if (item.Quantity == 0) item.Owner = ItemOwner.Consumed;
            Revision++; reason = "Consumed one Q" + eaten.Quality + " serving"; return true;
        }

        public bool TryMoveToSide(ItemEntry item, int pointer, bool hook, int index, out string reason)
        {
            reason = "Item is unavailable or the slot is invalid";
            int count = hook ? HookCount : PocketCount;
            if (!Owns(item) || !ReservedBy(item, pointer) || item.Owner != ItemOwner.Backpack || index < 0 || index >= count) return false;
            if (hook ? !IsTool(item.Definition) : !item.Definition.Stackable)
            { reason = hook ? "Hooks hold tools (one per hook)" : "Pockets hold eligible resource or food stacks"; return false; }
            ItemOwner target = hook ? ItemOwner.Hook : ItemOwner.Pocket;
            foreach (ItemEntry existing in entries)
                if (existing.Owner == target && existing.X == index) { reason = "Slot is occupied"; return false; }
            item.Owner = target; item.X = index; item.Y = 0; Revision++; reason = "Stored in " + (hook ? "hook" : "pocket"); return true;
        }

        // Hooks hold any tool, one per hook (user, October 31, 2026): unique items that are not wearables.
        public static bool IsTool(ItemDefinition definition) => definition != null && !definition.Stackable && !definition.GearSlot.HasValue;

        public bool TryReturnToBackpack(ItemEntry item, int pointer, out string reason)
        {
            reason = "Item is unavailable";
            if (!Owns(item) || !ReservedBy(item, pointer) || (item.Owner != ItemOwner.Pocket && item.Owner != ItemOwner.Hook && item.Owner != ItemOwner.Overflow)) return false;
            if (TryPlaceFree(item, pointer, out reason)) return true;
            reason = "No backpack space; item remains in its current container"; return false;
        }
        public bool TryConsumeOne(ItemEntry item, int pointer, out string reason)
        {
            return TryConsumeLowest(item, pointer, out _, out reason);
        }
        // One committed harvest action (section 8.2): spends one use of a carried tool (backpack or hook). The tool breaks,
        // and leaves the inventory, when that action brings it to zero.
        public bool TryUseTool(ItemEntry tool, out bool broke)
        {
            broke = false;
            if (!Owns(tool) || tool.Definition.Stackable || tool.Definition.GearSlot.HasValue || (tool.Owner != ItemOwner.Backpack && tool.Owner != ItemOwner.Hook)
                || tool.Profile.MaxDurability <= 0 || tool.Profile.Durability <= 0 || reservations.ContainsKey(tool.Id)) return false;
            ItemProfile used = tool.Profile;
            int left = used.Durability - 1;
            tool.ReplacePile(new[] { new PilePortion(tool.Quality, new ItemProfile(used.UnitValue, used.MaxDurability, left, used.Variant), 1) });
            if (left == 0) { tool.Owner = ItemOwner.Consumed; broke = true; }
            Revision++;
            return true;
        }
        public bool TryWear(ItemEntry item, int pointer, out string reason)
        {
            reason = "Select an eligible spare wearable in the backpack";
            if (!Owns(item) || !ReservedBy(item, pointer) || item.Owner != ItemOwner.Backpack || !item.Ready || !item.Definition.GearSlot.HasValue || item.Quantity != 1) return false;
            item.Owner = ItemOwner.Worn; item.X = item.Y = 0; Revision++; reason = "Equipped " + item.Definition.Name; return true;
        }
        // Discard pocket: a carried entry goes back onto the trail as an ordinary pickup at the given distance.
        public bool TryDropToTrail(ItemEntry item, int pointer, float distance, out string reason, string segmentId = null)
        {
            reason = "Only carried bag, pocket, hook or overflow items can be dropped";
            if (!Owns(item) || !ReservedBy(item, pointer) || float.IsNaN(distance) || float.IsInfinity(distance) || distance < 0 ||
                (item.Owner != ItemOwner.Backpack && item.Owner != ItemOwner.Pocket && item.Owner != ItemOwner.Hook && item.Owner != ItemOwner.Overflow)) return false;
            item.Owner = ItemOwner.World; item.X = item.Y = 0; item.TrailDistance = distance; item.Dropped = true; item.SegmentId = segmentId; Revision++;
            reason = "Dropped " + item.Definition.Name + " on the trail"; return true;
        }
        // Used only while reconstructing a validated save: replaces the single default portion with the saved pile.
        public bool RestorePile(ItemEntry item, IReadOnlyList<PilePortion> portions)
        {
            if (!Owns(item) || item.Owner != ItemOwner.World || reservations.ContainsKey(item.Id) || portions == null) return false;
            int total = 0; foreach (PilePortion portion in portions) total += portion.Quantity;
            return total == item.Quantity && item.ReplacePile(portions);
        }
        public bool AssignSegment(ItemEntry item, string segmentId)
        {
            if (!Owns(item) || item.Owner != ItemOwner.World || string.IsNullOrWhiteSpace(segmentId)) return false;
            item.SegmentId = segmentId; return true;
        }
        // A new drop cycle for a segment replaces everything still lying on it: uncollected generated drops and player drops.
        public int RemoveWorldItems(string segmentId)
        {
            int removed = 0;
            foreach (ItemEntry item in entries)
            {
                if (item.Owner != ItemOwner.World || item.SegmentId != segmentId || reservations.ContainsKey(item.Id)) continue;
                item.Owner = ItemOwner.Consumed; item.Clear(); removed++;
            }
            if (removed > 0) Revision++;
            return removed;
        }
        public bool RestoreDropped(ItemEntry item)
        {
            if (!Owns(item) || item.Owner != ItemOwner.World) return false;
            item.Dropped = true; return true;
        }
        // A new drop cycle (return to the segment's start camp) deletes player-dropped entries still on the trail.
        public int RemoveUncollectedDrops()
        {
            int removed = 0;
            foreach (ItemEntry item in entries)
            {
                if (item.Owner != ItemOwner.World || !item.Dropped || reservations.ContainsKey(item.Id)) continue;
                item.Owner = ItemOwner.Consumed; item.Clear(); removed++;
            }
            if (removed > 0) Revision++;
            return removed;
        }
        public bool ExpireWorn(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !identities.TryGetValue(id, out ItemEntry item) || item.Owner != ItemOwner.Worn || reservations.ContainsKey(id)) return false;
            item.Owner = ItemOwner.Consumed; item.Clear(); Revision++; return true;
        }
        public bool TakeOff(string id, double remainingDistance)
        {
            if (string.IsNullOrWhiteSpace(id) || double.IsNaN(remainingDistance) || double.IsInfinity(remainingDistance) || remainingDistance <= 0 ||
                !identities.TryGetValue(id, out ItemEntry item) || item.Owner != ItemOwner.Worn || reservations.ContainsKey(id)) return false;
            item.HasWearRemaining = true; item.WearRemaining = remainingDistance;
            item.Owner = ItemOwner.Overflow; item.X = item.Y = 0; Revision++; return true;
        }
        public bool RestoreWear(ItemEntry item, bool hasWear, double remainingDistance)
        {
            if (!Owns(item) || (hasWear && (!item.Definition.GearSlot.HasValue || double.IsNaN(remainingDistance) || double.IsInfinity(remainingDistance) || remainingDistance <= 0 || (item.Profile.MaxDurability > 0 && remainingDistance > item.Profile.MaxDurability + 0.0001)))) return false;
            if (hasWear) { item.HasWearRemaining = true; item.WearRemaining = remainingDistance; }
            return true;
        }

        public LossPlan PlanFaintLoss(string eventId, Func<int, int> nextRandom)
        {
            if (string.IsNullOrWhiteSpace(eventId) || nextRandom == null) throw new ArgumentException("A faint event and random source are required");
            if (reservations.Count != 0) throw new InvalidOperationException("Release held items before fainting");
            var eligible = new List<ItemEntry>();
            foreach (ItemEntry item in entries)
                if (item.Owner == ItemOwner.Backpack || item.Owner == ItemOwner.Pocket || item.Owner == ItemOwner.Hook) eligible.Add(item);
            int roll = nextRandom(3);
            if (roll < 0 || roll >= 3) throw new ArgumentOutOfRangeException(nameof(nextRandom));
            int count = Math.Min(eligible.Count, 2 + roll);
            var selected = new List<LossEntry>(count);
            for (int i = 0; i < count; i++)
            {
                int index = nextRandom(eligible.Count);
                if (index < 0 || index >= eligible.Count) throw new ArgumentOutOfRangeException(nameof(nextRandom));
                ItemEntry item = eligible[index]; eligible.RemoveAt(index);
                selected.Add(new LossEntry { Id = item.Id, Quantity = item.Quantity, Owner = item.Owner });
            }
            return new LossPlan { EventId = eventId, InventoryRevision = Revision, Entries = selected.AsReadOnly() };
        }

        public bool CommitFaintLoss(LossPlan plan)
        {
            if (plan == null || string.IsNullOrWhiteSpace(plan.EventId) || plan.Entries == null || plan.InventoryRevision != Revision || reservations.Count != 0) return false;
            var unique = new HashSet<string>();
            foreach (LossEntry loss in plan.Entries)
            {
                if (loss == null || !unique.Add(loss.Id) || !identities.TryGetValue(loss.Id, out ItemEntry item) || item.Quantity != loss.Quantity || item.Owner != loss.Owner ||
                    (item.Owner != ItemOwner.Backpack && item.Owner != ItemOwner.Pocket && item.Owner != ItemOwner.Hook)) return false;
            }
            foreach (LossEntry loss in plan.Entries)
            { ItemEntry item = identities[loss.Id]; item.Clear(); item.Owner = ItemOwner.Consumed; }
            // Increment even for an empty bag so a committed zero-loss event cannot be replayed.
            Revision++; return true;
        }

        // Used only while reconstructing a validated save into an otherwise private inventory.
        public bool RestoreStored(ItemEntry item, ItemOwner owner, int x, int y)
        {
            if (!Owns(item) || item.Owner != ItemOwner.World || reservations.ContainsKey(item.Id)) return false;
            if (owner == ItemOwner.Backpack)
            {
                if (!Reserve(item, int.MinValue)) return false;
                bool placed = TryPlace(item, int.MinValue, x, y, false, out _); Release(item, int.MinValue); return placed;
            }
            if (owner == ItemOwner.Overflow) { item.Owner = owner; item.X = item.Y = 0; Revision++; return true; }
            if (owner == ItemOwner.Worn)
            {
                if (!item.Definition.GearSlot.HasValue || item.Quantity != 1 || x != 0 || y != 0) return false;
                item.Owner = owner; item.X = item.Y = 0; Revision++; return true;
            }
            if (owner != ItemOwner.Pocket && owner != ItemOwner.Hook) return false;
            int count = owner == ItemOwner.Pocket ? PocketCount : HookCount;
            if (x < 0 || x >= count || y != 0 || (owner == ItemOwner.Pocket ? !item.Definition.Stackable : !IsTool(item.Definition))) return false;
            foreach (ItemEntry existing in entries) if (existing.Owner == owner && existing.X == x) return false;
            item.Owner = owner; item.X = x; item.Y = 0; Revision++; return true;
        }

        private sealed class Placement
        {
            public ItemEntry Item; public ItemOwner Owner; public int X, Y;
        }

        // Plan all changes against a shadow layout before applying any ownership or capacity change.
        // Existing overflow never becomes active capacity without an explicit claim.
        public bool Reconfigure(int width, int height, int pockets, int hooks, out string reason)
        {
            reason = "Invalid container capacity";
            if (width < 1 || height < 1 || pockets < 0 || pockets > 10 || hooks < 0 || hooks > 10) return false;
            if (reservations.Count != 0) { reason = "Release held items before changing equipment"; return false; }
            var placed = new List<Placement>(); var displaced = new List<ItemEntry>();
            foreach (ItemEntry item in entries)
            {
                if (item.Owner == ItemOwner.Backpack)
                {
                    if (FitsPlanned(item, item.X, item.Y, width, height, placed)) placed.Add(new Placement { Item = item, Owner = ItemOwner.Backpack, X = item.X, Y = item.Y });
                    else displaced.Add(item);
                }
                else if ((item.Owner == ItemOwner.Pocket && item.X >= pockets) || (item.Owner == ItemOwner.Hook && item.X >= hooks)) displaced.Add(item);
            }
            foreach (ItemEntry item in displaced)
            {
                bool found = false;
                var xs = new SortedSet<int> { 0 }; var ys = new SortedSet<int> { 0 };
                foreach (Placement other in placed) { xs.Add(other.X + other.Item.Width); ys.Add(other.Y + other.Item.Height); }
                foreach (int y in ys)
                {
                    foreach (int x in xs)
                    {
                        if (!FitsPlanned(item, x, y, width, height, placed)) continue;
                        placed.Add(new Placement { Item = item, Owner = ItemOwner.Backpack, X = x, Y = y }); found = true; break;
                    }
                    if (found) break;
                }
                if (!found) placed.Add(new Placement { Item = item, Owner = ItemOwner.Overflow, X = 0, Y = 0 });
            }
            foreach (Placement placement in placed)
            { placement.Item.Owner = placement.Owner; placement.Item.X = placement.X; placement.Item.Y = placement.Y; }
            Width = width; Height = height; PocketCount = pockets; HookCount = hooks; Revision++;
            reason = "Capacity updated; unplaced items moved to overflow"; return true;
        }
        private static bool FitsPlanned(ItemEntry item, int x, int y, int width, int height, List<Placement> placements)
        {
            if (x < 0 || y < 0 || item.Width > width || item.Height > height || x > width - item.Width || y > height - item.Height) return false;
            foreach (Placement other in placements)
                if (other.Owner == ItemOwner.Backpack && Overlaps(x, y, item.Width, item.Height, other.X, other.Y, other.Item.Width, other.Item.Height)) return false;
            return true;
        }
    }
}
