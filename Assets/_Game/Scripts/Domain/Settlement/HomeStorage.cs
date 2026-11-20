using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Settlement
{
    // One container per placed chest or tool rack piece, keyed by the piece's instance id. Crafting, missions, trading and
    // faint loss only ever see the bag, so nothing in here counts for them.
    public sealed class HomeStorage
    {
        public const int MaxItemsPerContainer = 1024;
        private readonly Dictionary<string, SpatialInventory> chests = new Dictionary<string, SpatialInventory>();
        private readonly Dictionary<string, ToolRack> racks = new Dictionary<string, ToolRack>();

        public static bool IsContainerDefinition(FurnitureDefinition d) => d != null && (d.IsChest || IsRackDefinition(d.Id));
        public static bool IsRackDefinition(string definitionId) => FurnitureCatalog.All.Any(d => d.Id == definitionId && d.IsRack);

        // Logical units (width, height); the chest ledger itself uses thousandths of a unit.
        public static (int width, int height) ChestSize(string definitionId)
        {
            FurnitureDefinition d = FurnitureCatalog.Find(definitionId);
            if (!d.IsChest) throw new ArgumentException("Not a chest: " + definitionId);
            switch (d.Id)
            {
                case "furniture.l2.pine_chest": return (6, 6);
                case "furniture.l4.birch_chest": return (12, 6);
                case "furniture.l6.mesquite_chest": return (24, 6);
                case "furniture.l8.reinforced_oak_chest": return (48, 6);
                default: throw new ArgumentException("Unknown chest size: " + definitionId);
            }
        }

        public SpatialInventory Chest(string pieceId) => pieceId != null && chests.TryGetValue(pieceId, out SpatialInventory c) ? c : null;
        public ToolRack Rack(string pieceId) => pieceId != null && racks.TryGetValue(pieceId, out ToolRack r) ? r : null;

        public IEnumerable<string> PieceIds => chests.Keys.Concat(racks.Keys).ToList();
        public IEnumerable<ItemEntry> Entries => chests.Values.SelectMany(c => LiveEntries(c)).Concat(racks.Values.SelectMany(r => r.Entries)).ToList();

        private static IEnumerable<ItemEntry> LiveEntries(SpatialInventory chest) => chest.Entries.Where(e => e.Owner != ItemOwner.Consumed);
        private static bool ChestIsEmpty(SpatialInventory chest) => !LiveEntries(chest).Any();

        // Creates an empty container for every placed chest or rack without one; drops empty containers whose piece is stored or gone.
        public void Sync(HomeState home)
        {
            foreach (FurniturePiece piece in home.Pieces)
            {
                if (!piece.Placed) continue;
                FurnitureDefinition d = piece.Definition;
                if (d.IsChest && !chests.ContainsKey(piece.InstanceId))
                {
                    (int w, int h) = ChestSize(d.Id);
                    AddChest(piece.InstanceId, new SpatialInventory(w * 1000, h * 1000));
                }
                else if (IsRackDefinition(d.Id) && !racks.ContainsKey(piece.InstanceId)) AddRack(piece.InstanceId, new ToolRack());
            }
            var placed = new HashSet<string>(home.Pieces.Where(p => p.Placed).Select(p => p.InstanceId));
            foreach (string id in chests.Where(kv => !placed.Contains(kv.Key) && ChestIsEmpty(kv.Value)).Select(kv => kv.Key).ToList()) chests.Remove(id);
            foreach (string id in racks.Where(kv => !placed.Contains(kv.Key) && kv.Value.IsEmpty).Select(kv => kv.Key).ToList()) racks.Remove(id);
        }

        public bool IsEmpty(string pieceId)
        {
            SpatialInventory chest = Chest(pieceId); if (chest != null) return ChestIsEmpty(chest);
            ToolRack rack = Rack(pieceId); return rack == null || rack.IsEmpty;
        }

        public bool TryStorePiece(HomeState home, string pieceId, out string reason)
        {
            if (!IsEmpty(pieceId)) { reason = "Empty it first"; return false; }
            home.Store(pieceId); Sync(home); reason = null; return true;
        }

        private static readonly string[] FillMaterials = { "material.pine_branch", "material.clay", "material.flint", "material.hemp", "food.apples", "material.pebbles" };

        // Debug (test phase): fills a chest with Q5 stacks of 1,000 cycling through the tier-one materials, each at the bag's
        // bottom-left free spot, until none of them fits. Returns the stacks added; 0 for a rack or an unknown piece.
        public int DebugFill(string pieceId, Func<string> newId)
        {
            SpatialInventory chest = Chest(pieceId);
            if (chest == null) return 0;
            var scratch = new SpatialInventory();   // probes that size a stack without touching the chest's ledger
            var probes = new List<ItemEntry>();
            foreach (string id in FillMaterials)
            {
                ItemDefinition definition = TheTrailWalker.Domain.Crafting.Items.Find(id);
                if (definition != null) probes.Add(scratch.AddWorld("probe-" + id, definition, 1000, 5, TheTrailWalker.Domain.Economy.ItemPrices.Profile(definition, 5)));
            }
            int added = 0, live = chest.Entries.Count(e => e.Owner != ItemOwner.Consumed);
            // The bag's bottom-left search: candidate corners are the container's origin and every entry's far edges. The corners
            // are kept across turns, and since a fill only adds stacks, a corner that refused a probe refuses it for the rest of the
            // fill: skipping those keeps the same placement without re-testing them (a 1,024-stack fill took minutes, Nov 19, 2026).
            var xs = new SortedSet<int> { 0 }; var ys = new SortedSet<int> { 0 };
            foreach (ItemEntry other in chest.Entries)
                if (other.Owner == ItemOwner.Backpack) { xs.Add(other.X + other.Width); ys.Add(other.Y + other.Height); }
            var refused = new HashSet<(int, int, int)>();
            for (int turn = 0, failed = 0; probes.Count > 0 && failed < probes.Count; turn++)
            {
                if (live >= MaxItemsPerContainer) break;
                int p = turn % probes.Count; ItemEntry probe = probes[p];
                if (!TryFindFreeSpot(chest, probe, p, xs, ys, refused, out int x, out int y)) { failed++; continue; }
                ItemEntry item = chest.AddWorld(newId(), probe.Definition, 1000, 5, probe.Profile);
                if (!chest.RestoreStored(item, ItemOwner.Backpack, x, y)) { item.Owner = ItemOwner.Consumed; failed++; continue; }
                xs.Add(item.X + item.Width); ys.Add(item.Y + item.Height);
                added++; live++; failed = 0;
            }
            return added;
        }

        private static bool TryFindFreeSpot(SpatialInventory chest, ItemEntry probe, int p, SortedSet<int> xs, SortedSet<int> ys, HashSet<(int, int, int)> refused, out int spotX, out int spotY)
        {
            foreach (int y in ys) foreach (int x in xs)
            {
                if (refused.Contains((p, x, y))) continue;
                if (chest.Fits(probe, probe.Quantity, x, y)) { spotX = x; spotY = y; return true; }
                refused.Add((p, x, y));
            }
            spotX = spotY = 0; return false;
        }

        internal void AddChest(string pieceId, SpatialInventory chest) { chest.IsContainer = true; chests[pieceId] = chest; }
        internal void AddRack(string pieceId, ToolRack rack) { racks[pieceId] = rack; }
    }
}
