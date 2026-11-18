using System.Collections.Generic;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Settlement
{
    // Twelve spots (row-major from the top-left, four per row), each holding one non-stackable item: a tool, spare clothing
    // or a spare backpack, ready or still on its timer. Items on the rack keep their durability, wear and profile.
    public sealed class ToolRack
    {
        public const int Spots = 12;
        private readonly ItemEntry[] spots = new ItemEntry[Spots];

        public ItemEntry this[int spot] => spot >= 0 && spot < Spots ? spots[spot] : null;
        public bool IsEmpty { get { foreach (ItemEntry e in spots) if (e != null) return false; return true; } }
        public IEnumerable<ItemEntry> Entries { get { foreach (ItemEntry e in spots) if (e != null) yield return e; } }

        public bool TryPut(SpatialInventory source, ItemEntry item, int pointer, int spot, out string reason)
        {
            reason = "Item is unavailable";
            if (source == null || item == null) return false;
            if (item.Definition.Stackable) { reason = "Only tools, clothes and backpacks go on the rack"; return false; }
            if (spot < 0 || spot >= Spots) { reason = "No such spot"; return false; }
            if (spots[spot] != null) { reason = "Spot taken"; return false; }
            if (!source.Detach(item, pointer)) return false;
            spots[spot] = item; reason = "Hung " + item.Definition.Name; return true;
        }

        // The spot empties only when the target accepts the item.
        public bool TryTake(int spot, SpatialInventory target, ItemOwner owner, int x, int y, out string reason)
        {
            reason = "Spot is empty";
            if (target == null || spot < 0 || spot >= Spots || spots[spot] == null) return false;
            ItemEntry item = spots[spot];
            if (!target.TryAttach(item, owner, x, y, out reason)) return false;
            spots[spot] = null; reason = "Took " + item.Definition.Name; return true;
        }

        public bool TryMoveSpot(int from, int to)
        {
            if (from < 0 || from >= Spots || to < 0 || to >= Spots || from == to || spots[from] == null || spots[to] != null) return false;
            spots[to] = spots[from]; spots[from] = null; return true;
        }

        // A finished timed craft becomes ready wherever it is stored.
        public bool MarkReady(ItemEntry item)
        {
            if (item == null || item.Ready || System.Array.IndexOf(spots, item) < 0) return false;
            item.Ready = true; return true;
        }

        // Used only while reconstructing a validated save.
        internal bool Restore(ItemEntry item, int spot)
        {
            if (item == null || item.Definition.Stackable || spot < 0 || spot >= Spots || spots[spot] != null || System.Array.IndexOf(spots, item) >= 0) return false;
            spots[spot] = item; return true;
        }
    }
}
