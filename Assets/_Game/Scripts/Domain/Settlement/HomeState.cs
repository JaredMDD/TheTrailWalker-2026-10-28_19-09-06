using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Economy;

namespace TheTrailWalker.Domain.Settlement
{
    // One owned furniture piece. X is the column from the room's front-left origin, Z the row toward the back.
    public sealed class FurniturePiece
    {
        public string InstanceId { get; internal set; }
        public string DefinitionId { get; internal set; }
        public bool Placed { get; internal set; }
        public int X { get; internal set; }
        public int Z { get; internal set; }
        public int Rotation { get; internal set; } // 0-3 quarter turns clockwise; only the index lives in the domain
        public FurnitureDefinition Definition { get { return FurnitureCatalog.Find(DefinitionId); } }
    }

    // First home (spec 2026-11-13): grid, placement, buying, storage. Pure domain; occupancy is recomputed from placed pieces.
    public sealed class HomeState
    {
        public const int MaxLevel = 8;
        // The player's own house stops at the last level HouseUpgrades prices (tier 2, user November 25, 2026); neighbours and the catalog go to MaxLevel.
        public const int PlayerMaxLevel = 4;

        private readonly List<FurniturePiece> pieces = new List<FurniturePiece>();
        private readonly List<string> firstPurchases = new List<string>();
        private readonly List<string> upgradeEscrow = new List<string>();

        public int Level { get; private set; }
        public int GridWidth { get; private set; }
        public int GridDepth { get; private set; }
        public IReadOnlyList<FurniturePiece> Pieces { get { return pieces; } }
        public IReadOnlyCollection<string> FirstPurchases { get { return firstPurchases; } }
        public bool CanUpgrade { get { return Level < PlayerMaxLevel; } }
        // Item ids the player has deposited toward the next upgrade, in deposit order.
        public IReadOnlyList<string> UpgradeEscrow { get { return upgradeEscrow; } }

        private HomeState(int level)
        {
            var grid = Grid(level);
            Level = level; GridWidth = grid.width; GridDepth = grid.depth;
        }

        public static (int width, int depth) Grid(int level)
        {
            if (level < 1 || level > 16) throw new ArgumentOutOfRangeException(nameof(level));
            switch (level)
            {
                case 1: return (8, 5);
                case 2: return (10, 6);
                default: return level <= 8 ? (12, 7 + 2 * (level - 3)) : (12, 19 + 2 * (level - 9));
            }
        }

        public static HomeState New() { return new HomeState(1); }

        // A home at the given level with no pieces (for checks and for restoring a save).
        public static HomeState AtLevel(int level) { return new HomeState(level); }

        // Buffs from the distinct types currently placed (a second copy of a type adds nothing).
        public FurnitureBuffs Buffs
        {
            get
            {
                int quality = 0; double food = 0, sale = 0;
                var durability = new double[Enum.GetValues(typeof(BuffCategory)).Length];
                foreach (string id in pieces.Where(p => p.Placed).Select(p => p.DefinitionId).Distinct())
                {
                    FurnitureDefinition d = FurnitureCatalog.Find(id);
                    switch (d.Category)
                    {
                        case BuffCategory.CraftQuality: quality += d.Strength; break;
                        case BuffCategory.FoodRecovery: food += FurnitureBuffs.FoodPerStrength * d.Strength; break;
                        case BuffCategory.SalePrice: sale += FurnitureBuffs.SalePerStrength * d.Strength; break;
                        case BuffCategory.Storage: break;
                        default: durability[(int)d.Category] += FurnitureBuffs.DurabilityPerStrength * d.Strength; break;
                    }
                }
                return new FurnitureBuffs(Math.Min(quality, 5), food, sale, durability);
            }
        }

        // Rebuilds a saved home. Piece cells are applied as stored; the data is validated, not re-placed.
        public static HomeState Restore(int level, IEnumerable<(string instanceId, string definitionId, bool placed, int x, int z)> savedPieces, IEnumerable<string> savedFirstPurchases)
        {
            return Restore(level, savedPieces?.Select(p => (p.instanceId, p.definitionId, p.placed, p.x, p.z, 0)), savedFirstPurchases);
        }

        public static HomeState Restore(int level, IEnumerable<(string instanceId, string definitionId, bool placed, int x, int z, int rotation)> savedPieces, IEnumerable<string> savedFirstPurchases)
        {
            return Restore(level, savedPieces, savedFirstPurchases, null);
        }

        public static HomeState Restore(int level, IEnumerable<(string instanceId, string definitionId, bool placed, int x, int z, int rotation)> savedPieces, IEnumerable<string> savedFirstPurchases, IEnumerable<string> upgradeEscrow)
        {
            if (level < 1 || level > MaxLevel) throw new ArgumentException("Home level out of range: " + level);
            var home = new HomeState(level);
            var ids = new HashSet<string>();
            if (savedPieces != null)
                foreach ((string instanceId, string definitionId, bool placed, int x, int z, int rotation) in savedPieces)
                {
                    if (string.IsNullOrEmpty(instanceId)) throw new ArgumentException("A furniture piece has no instance id.");
                    if (!ids.Add(instanceId)) throw new ArgumentException("Duplicate furniture instance: " + instanceId);
                    FurnitureDefinition d = FurnitureCatalog.All.FirstOrDefault(f => f.Id == definitionId);
                    if (d == null) throw new ArgumentException("Unknown furniture type: " + definitionId);
                    if (d.HouseLevel > level) throw new ArgumentException("Furniture type " + definitionId + " is above house level " + level);
                    if (rotation < 0 || rotation > 3) throw new ArgumentException("Furniture " + instanceId + " has a bad rotation: " + rotation);
                    if (placed)
                    {
                        if (!home.CanPlace(d, x, z, null, rotation)) throw new ArgumentException("Furniture " + instanceId + " does not fit at " + x + "," + z);
                        home.pieces.Add(new FurniturePiece { InstanceId = instanceId, DefinitionId = definitionId, Placed = true, X = x, Z = z, Rotation = rotation });
                    }
                    else
                    {
                        if (rotation != 0) throw new ArgumentException("Stored furniture " + instanceId + " has a rotation.");
                        home.pieces.Add(new FurniturePiece { InstanceId = instanceId, DefinitionId = definitionId });
                    }
                }
            if (savedFirstPurchases != null)
                foreach (string id in savedFirstPurchases)
                {
                    if (string.IsNullOrEmpty(id) || !FurnitureCatalog.All.Any(f => f.Id == id)) throw new ArgumentException("Unknown first purchase: " + id);
                    if (!home.firstPurchases.Contains(id)) home.firstPurchases.Add(id);
                }
            if (upgradeEscrow != null)
                foreach (string id in upgradeEscrow)
                {
                    if (string.IsNullOrEmpty(id)) throw new ArgumentException("An upgrade escrow id is empty.");
                    if (home.upgradeEscrow.Contains(id)) throw new ArgumentException("Duplicate upgrade escrow id: " + id);
                    home.upgradeEscrow.Add(id);
                }
            return home;
        }

        internal void AddUpgradeEscrow(string itemId) { upgradeEscrow.Add(itemId); }

        internal void RemoveUpgradeEscrow(string itemId) { upgradeEscrow.Remove(itemId); }

        // Grows the room one level: the new grid is wider, so placed pieces shift right to stay centred; escrow is spent.
        internal void CompleteUpgrade()
        {
            if (!CanUpgrade) throw new InvalidOperationException("The house cannot be upgraded further.");
            GrowOneLevel();
            upgradeEscrow.Clear();
        }

        // The growth step shared with the debug jump: one level up, the new grid, placed pieces shifted to stay centred.
        // It never touches the escrow and has no player cap (only MaxLevel).
        internal void GrowOneLevel()
        {
            if (Level >= MaxLevel) throw new InvalidOperationException("The house is at the highest level there is.");
            int oldWidth = GridWidth;
            Level++;
            var grid = Grid(Level);
            GridWidth = grid.width; GridDepth = grid.depth;
            int shift = (GridWidth - oldWidth) / 2;
            foreach (FurniturePiece p in pieces)
                if (p.Placed) p.X += shift;
        }

        // Places a piece outright (no wallet, no first-purchase record) when it fits; internal to the domain so only
        // NeighbourHome's look-only layouts use it, never the player's home.
        internal bool TryAddPlaced(FurnitureDefinition d, string instanceId, int x, int z, int rotation)
        {
            if (d == null || string.IsNullOrEmpty(instanceId) || rotation < 0 || rotation > 3 || d.HouseLevel > Level) return false;
            if (pieces.Any(p => p.InstanceId == instanceId) || !CanPlace(d, x, z, null, rotation)) return false;
            pieces.Add(new FurniturePiece { InstanceId = instanceId, DefinitionId = d.Id, Placed = true, X = x, Z = z, Rotation = rotation });
            return true;
        }

        // Odd quarter turns swap width and depth.
        public static (int width, int depth) Footprint(FurnitureDefinition d, int rotation)
        {
            return (rotation & 1) == 0 ? (d.Width, d.Depth) : (d.Depth, d.Width);
        }

        public bool CanPlace(FurnitureDefinition d, int x, int z, string ignoreInstanceId = null, int rotation = 0)
        {
            if (d == null) return false;
            (int width, int depth) = Footprint(d, rotation);
            if (x < 0 || z < 0 || x + width > GridWidth || z + depth > GridDepth) return false;
            foreach (FurniturePiece p in pieces)
            {
                if (!p.Placed || p.InstanceId == ignoreInstanceId) continue;
                (int ow, int od) = Footprint(p.Definition, p.Rotation);
                if (x < p.X + ow && p.X < x + width && z < p.Z + od && p.Z < z + depth) return false;
            }
            return true;
        }

        public bool TryNearestFree(FurnitureDefinition d, out int x, out int z, string ignoreInstanceId = null, int rotation = 0)
        {
            x = 0; z = 0;
            if (d == null) return false;
            (int width, int depth) = Footprint(d, rotation);
            for (int row = 0; row + depth <= GridDepth; row++)
            {
                int bestX = -1, bestDistance = int.MaxValue;
                for (int col = 0; col + width <= GridWidth; col++)
                {
                    if (!CanPlace(d, col, row, ignoreInstanceId, rotation)) continue;
                    int distance = Math.Abs(2 * col + width - GridWidth); // twice the centre offset; strict < keeps the lower x on ties
                    if (distance < bestDistance) { bestDistance = distance; bestX = col; }
                }
                if (bestX >= 0) { x = bestX; z = row; return true; }
            }
            return false;
        }

        // Nearest free cell at rotation 0, else at rotation 1.
        private bool TryNearestFreeAny(FurnitureDefinition d, out int x, out int z, out int rotation)
        {
            rotation = 0;
            if (TryNearestFree(d, out x, out z)) return true;
            rotation = 1;
            if (TryNearestFree(d, out x, out z, null, 1)) return true;
            rotation = 0;
            return false;
        }

        public bool CanBuy(FurnitureDefinition d, Wallet wallet, out string reason)
        {
            if (d == null) { reason = "Unknown furniture"; return false; }
            if (d.HouseLevel > Level) { reason = "Unlocks at house level " + d.HouseLevel; return false; }
            if (wallet == null || !wallet.CanDebit(Currency.Cash, d.PriceCash)) { reason = "Not enough cash"; return false; }
            reason = null;
            return true;
        }

        public FurniturePiece Buy(FurnitureDefinition d, Wallet wallet, string newInstanceId)
        {
            string reason;
            if (!CanBuy(d, wallet, out reason)) throw new InvalidOperationException(reason);
            if (string.IsNullOrEmpty(newInstanceId)) throw new ArgumentException("An instance id is required.", nameof(newInstanceId));
            if (pieces.Any(p => p.InstanceId == newInstanceId)) throw new ArgumentException("Duplicate instance id: " + newInstanceId, nameof(newInstanceId));
            wallet.Debit(Currency.Cash, d.PriceCash);
            if (!firstPurchases.Contains(d.Id)) firstPurchases.Add(d.Id);
            var piece = new FurniturePiece { InstanceId = newInstanceId, DefinitionId = d.Id };
            int x, z, rotation;
            if (TryNearestFreeAny(d, out x, out z, out rotation)) { piece.Placed = true; piece.X = x; piece.Z = z; piece.Rotation = rotation; }
            pieces.Add(piece);
            return piece;
        }

        public bool Move(string instanceId, int x, int z) => Move(instanceId, x, z, Get(instanceId).Rotation);

        // Moves a placed piece to a cell at a rotation in one step (a turn that did not fit where it was, dropped where it
        // does); false and nothing changes when it would not fit there or the piece is stored.
        public bool Move(string instanceId, int x, int z, int rotation)
        {
            FurniturePiece p = Get(instanceId);
            if (!p.Placed || rotation < 0 || rotation > 3 || !CanPlace(p.Definition, x, z, instanceId, rotation)) return false;
            p.X = x; p.Z = z; p.Rotation = rotation;
            return true;
        }

        public void Store(string instanceId)
        {
            FurniturePiece p = Get(instanceId);
            p.Placed = false; p.X = 0; p.Z = 0; p.Rotation = 0;
        }

        // Turns a placed piece one quarter turn clockwise (index + 1) in place; false when it would not fit or the piece is stored.
        public bool Rotate(string instanceId)
        {
            FurniturePiece p = Get(instanceId);
            if (!p.Placed) return false;
            int next = (p.Rotation + 1) % 4;
            if (!CanPlace(p.Definition, p.X, p.Z, instanceId, next)) return false;
            p.Rotation = next;
            return true;
        }

        public bool PlaceFromStorage(string instanceId)
        {
            FurniturePiece p = Get(instanceId);
            if (p.Placed) return true;
            int x, z, rotation;
            if (!TryNearestFreeAny(p.Definition, out x, out z, out rotation)) return false;
            p.Placed = true; p.X = x; p.Z = z; p.Rotation = rotation;
            return true;
        }

        private FurniturePiece Get(string instanceId)
        {
            FurniturePiece p = pieces.FirstOrDefault(q => q.InstanceId == instanceId);
            if (p == null) throw new ArgumentException("Unknown furniture instance: " + instanceId);
            return p;
        }
    }
}
