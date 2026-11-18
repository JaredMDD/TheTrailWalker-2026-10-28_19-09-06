using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TheTrailWalker.Domain;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Equipment;
using TheTrailWalker.Domain.Travel;
using UnityEngine;

namespace TheTrailWalker.Persistence
{
    public interface IClock { DateTime UtcNow { get; } }
    public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }
    public interface IRandomSource { int Next(int exclusiveMaximum); }
    public sealed class SeededRandom : IRandomSource
    {
        private readonly System.Random random;
        public SeededRandom(int seed) { random = new System.Random(seed); }
        public int Next(int exclusiveMaximum) => random.Next(exclusiveMaximum);
    }

    [Serializable] public sealed class SessionSnapshot
    {
        public int version = 1;
        public string savedUtc, segmentId = "prototype.trail.1", transactionId;
        public float distance, requestedSpeed, speedCap, inventoryResume, yaw;
        public bool inventoryOpen, postHarvest, camp;
        public List<SavedItem> items = new List<SavedItem>();
        public SavedSurvival survival;
        // Schema 3: camp, discovery, toll and drop-cycle progress (Milestone 5).
        public SavedTravel travel;
        // Milestone 6 (optional, older saves load without it): interactables harvested in their segment's current drop
        // cycle, as "segmentId|cycle|anchorId". Entries from older cycles are ignored and dropped on the next save.
        public List<string> harvested = new List<string>();
        // Milestone 7 (optional): known recipes, mastery units and outstanding timed batches.
        public SavedCrafting crafting;
        // Milestone 8 (optional): claimed missions in claim order, the active mission and its delivered units.
        public SavedMissions missions;
        // Statistics (optional, November 2, 2026): counters for the possible statistics page; older saves start at zero.
        public SavedStatistics statistics;
        // Milestone 9 (optional): item values in minor units (economy 1; 0 = older whole-cash or placeholder values, migrated
        // on load), the camp trade session of the current or last visit, and the recent trade transactions.
        public int economy;
        public TheTrailWalker.Domain.Economy.TradeSessionData trade;
        // Settlement shell (November 8, 2026, optional): the discovered settlement, the player's plot and the neighbours. Older saves have none.
        public SavedSettlement settlement;
        // First home (November 13, 2026, optional): house level, furniture pieces and first purchases. Older saves have none.
        public SavedHome home;
        public List<TheTrailWalker.Domain.Economy.TradeRecord> tradeLog = new List<TheTrailWalker.Domain.Economy.TradeRecord>();
    }
    [Serializable] public sealed class SavedSettlement
    {
        // JsonUtility writes an absent block as a default object, so 0 marks "no settlement block" (older saves); saves write 1.
        public int block;
        public bool discovered;
        public int plotIndex = -1, seed;
        public List<SavedNeighbour> neighbours = new List<SavedNeighbour>();
        public List<string> countedRegions = new List<string>();
    }
    [Serializable] public sealed class SavedHome
    {
        // 0 marks "no home block" (older saves), as for the settlement; saves write 1.
        public int block;
        public int level = 1;
        public List<SavedPiece> pieces = new List<SavedPiece>();
        // Milestone 11 (optional): chest and rack contents; JsonUtility gives an empty list when absent (older saves).
        public List<SavedContainer> containers = new List<SavedContainer>();
        public List<string> firstPurchases = new List<string>();
        // Slice 2b (optional): the house's upgrade escrow entry ids in deposit order; JsonUtility gives an empty list when absent.
        public List<string> upgradeEscrow = new List<string>();
    }
    // Milestone 11 (optional): one placed chest or tool rack's contents. kind is "chest" or "rack"; chest items keep owner
    // "Backpack" with their x,y in the chest's thousandths, rack items use x = spot (0-11) and y = 0.
    [Serializable] public sealed class SavedContainer { public string pieceId, kind; public List<SavedItem> items = new List<SavedItem>(); }
    [Serializable] public sealed class SavedPiece { public string id, definition; public bool placed; public int x, z; public int rotation; }
    [Serializable] public sealed class SavedNeighbour { public int plot, level, countdown; }
    [Serializable] public sealed class SavedCount { public string key; public long value; }
    [Serializable] public sealed class SavedStatistics
    {
        public double distanceMetres; public long cashEarned, cashSpent, goldEarned, goldSpent; public int faints;
        public List<SavedCount> gathered = new List<SavedCount>(), craftedUnits = new List<SavedCount>(), crafts = new List<SavedCount>(), interactions = new List<SavedCount>();
    }
    [Serializable] public sealed class SavedMissions
    {
        public List<string> claimed = new List<string>();
        public string active; // null or empty: no active mission
        public int delivered;
        public int region; // unlocked mission region (0 in older saves: region 1)
        // Mission catalog (0 in older saves: the original book). legacyClaims: how many leading claims were made under the
        // original order; activeLegacy: the active mission was accepted under it (see MissionState.Restore).
        public int catalog;
        public int legacyClaims;
        public bool activeLegacy;
        // Catalog 3 (November 7, 2026): the catalog each claim was made under (parallel to claimed) and the catalog the active
        // mission was accepted under (0 without one). Older saves leave them empty and use legacyClaims/activeLegacy.
        public List<int> claimCatalogs = new List<int>();
        public int activeCatalog;
    }
    [Serializable] public sealed class SavedCrafting
    {
        public List<string> known = new List<string>(); // in learning order
        public List<SavedMastery> mastery = new List<SavedMastery>();
        public string lastRecipe; // the last crafted recipe opens the crafting menu
        public List<SavedBatch> batches = new List<SavedBatch>();
    }
    [Serializable] public sealed class SavedMastery { public string recipeId; public long units; }
    [Serializable] public sealed class SavedBatch
    {
        public string id, recipeId, startedUtc, readyAtUtc; public int executions, outputUnits, level, quality, maxDurability; public long unitValue; public bool credited, masteryCredited;
        public List<string> itemIds = new List<string>();
    }
    [Serializable] public sealed class SavedTravel
    {
        public string campId, lastTrailCampId;
        public bool seated;
        public int visitNumber;
        public long cash; public long gold; // debug gold wallet (absent in older schema 3 files: 0)
        public bool farBank; // walking the ferry landing leg
        public bool settlementApproach; // walking the settlement approach past Bright Hollow's ferry (absent in older saves: false)
        public bool inSettlement; public string settlementReturnCampId; // inside the settlement shell and the camp the Return button seats (absent in older saves: outside, none)
        public List<string> visited = new List<string>(), discovered = new List<string>(), paidTolls = new List<string>();
        public List<SavedCycle> cycles = new List<SavedCycle>();
    }
    [Serializable] public sealed class SavedCycle { public string segmentId; public int cycle; }
    [Serializable] public sealed class SavedSurvival
    {
        public double hearts = 3, maximum = 3, pendingFood, faintElapsed;
        public bool fainted;
        public string faintEventId;
        public List<SavedGear> gear = new List<SavedGear>();
        public List<SavedLoss> loss = new List<SavedLoss>();
    }
    [Serializable] public sealed class SavedGear
    {
        public string id, slot;
        public int rank, quality;
        public double remainingDistance;
        public bool infinite;
    }
    [Serializable] public sealed class SavedLoss { public string itemId; public int quantity; }
    [Serializable] public sealed class SavedItem
    {
        public string id, definitionId, owner, variant;
        public int quantity, quality, x, y, maxDurability, durability;
        public long unitValue;
        public bool ready;
        public bool dropped;
        public bool hasWear;
        public double wearRemaining;
        public float trailDistance, worldX, worldY, worldZ;
        // Quality/profile portions of the stack; absent in older saves, which restore as one portion from the fields above.
        public List<SavedPortion> pile;
        // Schema 3: the trail segment a world item lies on.
        public string segmentId;
    }
    [Serializable] public sealed class SavedPortion
    {
        public int quality, quantity, maxDurability, durability;
        public long unitValue;
        public string variant;
    }
    [Serializable] internal sealed class SaveEnvelope { public int version = 1; public string payload, sha256; }
    public enum LoadStatus { Fresh, Loaded, Recovered, Invalid, Unsupported }

    public sealed class SessionSaveStore
    {
        private readonly string path, backup, temporary;
        private readonly IClock clock;
        private readonly int latestSchema;
        private volatile bool recovered;
        public SessionSaveStore(string directory, IClock time, int schemaVersion = 1)
        {
            if (schemaVersion < 1 || schemaVersion > 3) throw new ArgumentOutOfRangeException(nameof(schemaVersion));
            latestSchema = schemaVersion; path = Path.Combine(directory, "session-v" + schemaVersion + ".json"); backup = path + ".bak"; temporary = path + ".tmp"; clock = time ?? throw new ArgumentNullException(nameof(time));
        }
        public string MainPath => path;
        public string BackupPath => backup;
        public void ArchiveForReset()
        {
            FlushBackgroundSaves();
            string suffix = ".reset-" + clock.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N");
            if (File.Exists(path)) File.Move(path, path + suffix);
            if (File.Exists(backup)) File.Move(backup, backup + suffix);
            recovered = false;
        }

        private static string Hash(string value)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }
        // Background writer (October 31, 2026): the walking autosave cost 18-31 ms of main-thread time on the test phone,
        // mostly validation, hashing and the flushed write. SaveInBackground hands a finished snapshot to one writer
        // thread shared by every store (it survives scene reloads); saves are written strictly in order, and while one
        // is being written only the newest waiting snapshot is kept. Load, ArchiveForReset and Save wait for pending
        // writes first, so a reload always reads the latest save and a synchronous save is never overtaken.
        private static readonly object queueGate = new object(), ioGate = new object();
        private static SessionSaveStore pendingStore;
        private static SessionSnapshot pendingSnapshot;
        private static float pendingPathLength;
        private static bool writerRunning;
        private static Exception writerFailure;

        // snapshot must not be modified after this call. A failure is reported through TakeBackgroundFailure.
        public void SaveInBackground(SessionSnapshot snapshot, float pathLength = float.MaxValue)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            lock (queueGate)
            {
                pendingStore = this; pendingSnapshot = snapshot; pendingPathLength = pathLength;
                if (writerRunning) return;
                writerRunning = true;
            }
            System.Threading.ThreadPool.QueueUserWorkItem(_ => DrainWriter());
        }

        private static void DrainWriter()
        {
            while (true)
            {
                SessionSaveStore store; SessionSnapshot snapshot; float length;
                lock (queueGate)
                {
                    if (pendingSnapshot == null) { writerRunning = false; System.Threading.Monitor.PulseAll(queueGate); return; }
                    store = pendingStore; snapshot = pendingSnapshot; length = pendingPathLength; pendingSnapshot = null; pendingStore = null;
                }
                try { store.WriteSnapshot(snapshot, length); }
                catch (Exception error)
                {
                    // Keep the first failure; drop anything queued behind it (the caller blocks further saves).
                    lock (queueGate) { if (writerFailure == null) writerFailure = error; pendingSnapshot = null; pendingStore = null; }
                }
            }
        }

        // Blocks until every queued background save has been written (or has failed).
        public static void FlushBackgroundSaves()
        {
            lock (queueGate) while (writerRunning) System.Threading.Monitor.Wait(queueGate);
        }

        // Returns (and clears) the first background save failure since the last call, or null.
        public static Exception TakeBackgroundFailure()
        {
            lock (queueGate) { Exception failure = writerFailure; writerFailure = null; return failure; }
        }

        // Milliseconds spent in each stage of the last write (validate, serialize, hash, write+flush, replace); diagnostics only.
        public static readonly double[] LastStageMs = new double[5];
        // Synchronous save (pause/quit, validation suites): waits for queued background saves, then writes this one.
        public void Save(SessionSnapshot snapshot, float pathLength = float.MaxValue)
        {
            FlushBackgroundSaves();
            WriteSnapshot(snapshot, pathLength);
        }

        private void WriteSnapshot(SessionSnapshot snapshot, float pathLength)
        {
            lock (ioGate) WriteSnapshotLocked(snapshot, pathLength);
        }

        private void WriteSnapshotLocked(SessionSnapshot snapshot, float pathLength)
        {
            if (snapshot == null || snapshot.version != latestSchema) throw new InvalidDataException("Unexpected save schema for this store");
            var clockwatch = System.Diagnostics.Stopwatch.StartNew();
            double Lap() { double ms = clockwatch.Elapsed.TotalMilliseconds; clockwatch.Restart(); return ms; }
            Validate(snapshot, pathLength, out _);
            LastStageMs[0] = Lap();
            snapshot.savedUtc = clock.UtcNow.ToString("O");
            string payload = JsonUtility.ToJson(snapshot);
            LastStageMs[1] = Lap();
            string hash = Hash(payload);
            LastStageMs[2] = Lap();
            string json = JsonUtility.ToJson(new SaveEnvelope { payload = payload, sha256 = hash });
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None)) { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            LastStageMs[3] = Lap();
            if (File.Exists(path)) File.Replace(temporary, path, recovered ? null : backup);
            else File.Move(temporary, path);
            LastStageMs[4] = Lap();
            recovered = false;
        }
        public LoadStatus Load(float pathLength, out SessionSnapshot snapshot, out string message)
        {
            FlushBackgroundSaves();
            snapshot = null; message = "";
            if (!File.Exists(path) && !File.Exists(backup)) return LoadStatus.Fresh;
            LoadStatus main = Read(path, pathLength, out snapshot, out message);
            if (main == LoadStatus.Loaded) return main;
            // Never downgrade a newer save to its older backup.
            if (main == LoadStatus.Unsupported) return main;
            LoadStatus second = Read(backup, pathLength, out snapshot, out string backupError);
            if (second == LoadStatus.Loaded) { recovered = true; message = "Recovered the previous backup; recent changes may be missing."; return LoadStatus.Recovered; }
            if (second == LoadStatus.Unsupported) { message = backupError; return second; }
            message = "Save and backup could not be loaded. Existing files were preserved. " + message + " " + backupError;
            return LoadStatus.Invalid;
        }
        private LoadStatus Read(string filename, float length, out SessionSnapshot snapshot, out string message)
        {
            snapshot = null; message = "";
            if (!File.Exists(filename)) { message = "File missing."; return LoadStatus.Invalid; }
            try
            {
                var envelope = JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(filename));
                if (envelope == null) throw new InvalidDataException("Empty envelope");
                if (envelope.version > 1) { message = "Save uses a newer envelope version."; return LoadStatus.Unsupported; }
                if (envelope.version != 1 || string.IsNullOrEmpty(envelope.payload) || envelope.sha256 != Hash(envelope.payload)) throw new InvalidDataException("Checksum mismatch");
                snapshot = JsonUtility.FromJson<SessionSnapshot>(envelope.payload);
                if (snapshot != null && snapshot.version > latestSchema) { snapshot = null; message = "Save uses a newer schema version."; return LoadStatus.Unsupported; }
                MigrateCatalogLayout(snapshot);
                Validate(snapshot, length, out _); return LoadStatus.Loaded;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is InvalidDataException || e is FormatException || e is InvalidOperationException)
            { snapshot = null; message = e.Message; return LoadStatus.Invalid; }
        }
        // When the authored camp layout changes (Dawn Plains grew from three to ten camps, October 31, 2026), a save whose
        // segment no longer matches its camp is seated back at that camp with a drop cycle for its new outgoing leg.
        // Carried items, progress and paid tolls are untouched.
        public static void MigrateCatalogLayout(SessionSnapshot data)
        {
            if (data == null || data.version != 3 || data.travel == null || data.travel.cycles == null || data.travel.inSettlement) return;
            TravelCatalog catalog = TravelCatalog.Prototype();
            CampDefinition camp = catalog.Camp(data.travel.campId);
            if (camp == null) return;
            SegmentDefinition outgoing = catalog.Outgoing(camp.Id);
            SegmentDefinition expected = data.travel.farBank ? catalog.LandingInto(catalog.Region(camp.RegionId).NextRegionId)
                : data.travel.settlementApproach ? catalog.SettlementApproachFrom(camp.Id) : outgoing;
            bool landingMoved = data.travel.farBank && (expected == null || expected.FromCampId != camp.Id);
            if (!landingMoved && expected != null && data.segmentId == expected.Id) return;
            data.segmentId = outgoing.Id; data.travel.seated = true; data.travel.farBank = false; data.travel.settlementApproach = false;
            data.distance = 0; data.requestedSpeed = 0; data.inventoryResume = 0; data.camp = false;
            if (!data.travel.cycles.Exists(c => c != null && c.segmentId == outgoing.Id)) data.travel.cycles.Add(new SavedCycle { segmentId = outgoing.Id, cycle = 1 });
        }
        // An unseated schema-3 save is a walk on its saved leg, but on a real app start the scene shows the start camp's own
        // leg (a ferry landing or the settlement approach start at a river, so that leg is the short river walk); the restart
        // then reseats the player and resets the distance. Bound such a distance by the saved leg's catalogue length instead
        // (review fix, November 8, 2026: closing the app at the settlement arch or far along a landing blocked the save).
        private static bool WalkOnSavedLeg(SessionSnapshot data) => data.version == 3 && data.travel != null && !data.travel.seated;
        private static float DistanceLimit(SessionSnapshot data, float pathLength)
        {
            if (!WalkOnSavedLeg(data)) return pathLength;
            SegmentDefinition saved = TravelCatalog.Prototype().Segment(data.segmentId);
            return saved == null ? pathLength : Math.Max(pathLength, saved.Length);
        }
        public static void Validate(SessionSnapshot data, float pathLength, out SpatialInventory inventory)
        {
            if (data == null || data.version < 1 || data.version > 3 || (data.version < 3 && data.segmentId != "prototype.trail.1") || data.items == null || data.items.Count > 1024 || (data.harvested != null && (data.harvested.Count > 512 || data.harvested.Exists(h => string.IsNullOrEmpty(h) || h.Split('|').Length != 3))) ||
                (data.crafting != null && (data.crafting.known == null || data.crafting.mastery == null || data.crafting.batches == null || data.crafting.batches.Count > 4096
                    || data.crafting.mastery.Exists(m => m == null || string.IsNullOrEmpty(m.recipeId) || m.units < 0)
                    || data.crafting.batches.Exists(b => b == null || string.IsNullOrEmpty(b.id) || string.IsNullOrEmpty(b.recipeId) || b.executions < 1 || b.outputUnits < 1 || b.itemIds == null
                        || !DateTime.TryParse(b.readyAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out _)))) || float.IsNaN(data.yaw) || float.IsInfinity(data.yaw) || data.distance > DistanceLimit(data, pathLength)) throw new InvalidDataException("Invalid session header");
            if (data.missions != null)
            {
                if (data.missions.claimed == null || data.missions.claimed.Count > 256) throw new InvalidDataException("Invalid session data");
                try { TheTrailWalker.Domain.Missions.MissionState.Restore(data.missions.claimed, string.IsNullOrEmpty(data.missions.active) ? null : data.missions.active, data.missions.delivered, data.missions.region,
                    data.missions.catalog, data.missions.legacyClaims, data.missions.activeLegacy, data.missions.claimCatalogs, data.missions.activeCatalog); }
                catch (ArgumentException e) { throw new InvalidDataException("Invalid mission progress: " + e.Message); }
            }
            if (data.settlement != null && data.settlement.block != 0) RestoreSettlement(data, 0);
            TheTrailWalker.Domain.Settlement.HomeState savedHome = data.home != null && data.home.block != 0 ? RestoreHome(data) : null;
            var movement = new MovementState(); movement.Restore(data.distance, data.requestedSpeed, data.speedCap, data.inventoryResume, data.inventoryOpen, data.postHarvest, data.camp);
            // The camp flag belongs to the saved leg too; an unseated schema-3 walk is reset on restart, so only check it otherwise.
            if (data.camp != (data.distance >= pathLength) && pathLength != float.MaxValue && !WalkOnSavedLeg(data)) throw new InvalidDataException("Invalid camp position");
            TravelCatalog catalog = TravelCatalog.Prototype();
            if (data.version == 3)
            {
                TravelState travel = RestoreTravel(data, catalog);
                if (data.segmentId != travel.CurrentSegment.Id) throw new InvalidDataException("Saved segment does not match the current camp");
                // Seated at a camp: at the start of its outgoing segment with no walking intent.
                if (travel.Seated && (data.distance != 0 || data.requestedSpeed != 0 || data.inventoryResume != 0 || data.camp)) throw new InvalidDataException("Seated state has walking progress");
            }
            var gear = new EquipmentState(); var wornIds = new HashSet<string>(); var wornRecords = new Dictionary<string, SavedGear>(); var wornItems = new HashSet<string>();
            if (data.version >= 2)
            {
                SavedSurvival survival = data.survival;
                if (survival == null || survival.gear == null || survival.loss == null || survival.gear.Count > 5 || survival.loss.Count > 4) throw new InvalidDataException("Invalid survival data");
                foreach (SavedGear saved in survival.gear)
                {
                    if (saved == null || !Enum.TryParse(saved.slot, false, out GearSlot slot) || !Enum.IsDefined(typeof(GearSlot), slot) || !wornIds.Add(saved.id)) throw new InvalidDataException("Invalid worn gear");
                    gear.Equip(new WornGear(saved.id, slot, saved.rank, saved.quality, saved.remainingDistance, saved.infinite));
                    wornRecords.Add(saved.id, saved);
                }
                var stamina = new StaminaState(); stamina.Restore(survival.hearts, survival.maximum, survival.pendingFood, survival.fainted, survival.faintElapsed);
                if (Math.Abs(survival.maximum - gear.MaxHearts) > 0.0001 || data.speedCap > gear.SpeedCap + 0.0001 || (survival.fainted && (survival.pendingFood > 0 || string.IsNullOrWhiteSpace(survival.faintEventId) || data.requestedSpeed != 0 || data.inventoryResume != 0 || data.inventoryOpen || !data.postHarvest))) throw new InvalidDataException("Survival capabilities or faint state are inconsistent");
                var lossIds = new HashSet<string>();
                foreach (SavedLoss lost in survival.loss)
                    if (lost == null || string.IsNullOrWhiteSpace(lost.itemId) || lost.quantity < 1 || lost.quantity > 1000 || !lossIds.Add(lost.itemId) || wornIds.Contains(lost.itemId)) throw new InvalidDataException("Invalid faint loss manifest");
                if (survival.loss.Count > 0 && string.IsNullOrWhiteSpace(survival.faintEventId)) throw new InvalidDataException("Loss manifest has no faint event");
                foreach (SavedItem remaining in data.items) if (remaining != null && lossIds.Contains(remaining.id)) throw new InvalidDataException("Faint loss still owned");
            }
            inventory = data.version == 1 ? new SpatialInventory() : new SpatialInventory(gear.BackpackWidth * 1000, gear.BackpackHeight * 1000);
            if (data.version >= 2 && !inventory.Reconfigure(gear.BackpackWidth * 1000, gear.BackpackHeight * 1000, gear.Pockets, gear.Hooks, out _)) throw new InvalidDataException("Invalid storage capabilities");
            foreach (SavedItem item in data.items)
            {
                if (item == null || !Enum.TryParse(item.owner, false, out ItemOwner owner) || (owner != ItemOwner.World && owner != ItemOwner.Backpack && (data.version == 1 || (owner != ItemOwner.Pocket && owner != ItemOwner.Hook && owner != ItemOwner.Overflow && owner != ItemOwner.Worn && owner != ItemOwner.Escrow))) || (wornIds.Contains(item.id) && owner != ItemOwner.Worn) || float.IsNaN(item.trailDistance) || float.IsInfinity(item.trailDistance) || item.trailDistance < 0 || item.trailDistance > (data.version == 3 ? 10000f : pathLength) || float.IsNaN(item.worldX) || float.IsInfinity(item.worldX) || float.IsNaN(item.worldY) || float.IsInfinity(item.worldY) || float.IsNaN(item.worldZ) || float.IsInfinity(item.worldZ)) throw new InvalidDataException("Invalid item record");
                ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(item.definitionId);
                if (def == null) throw new InvalidDataException("Unknown item definition: " + item.definitionId);
                if (owner == ItemOwner.Worn)
                {
                    if (!wornRecords.TryGetValue(item.id, out SavedGear worn) || !def.GearSlot.HasValue || def.GearSlot.Value.ToString() != worn.slot || def.GearRank != worn.rank || item.quality != worn.quality || !wornItems.Add(item.id)) throw new InvalidDataException("Worn item and gear slot disagree");
                    if (item.hasWear && !worn.infinite && Math.Abs(item.wearRemaining - worn.remainingDistance) > 0.0001) throw new InvalidDataException("Worn item durability and gear slot disagree");
                    if (!worn.infinite && item.maxDurability > 0 && worn.remainingDistance > item.maxDurability + 0.0001) throw new InvalidDataException("Worn item exceeds its maximum durability");
                }
                ItemEntry entry = BuildEntry(inventory, item);
                if (owner != ItemOwner.World && !inventory.RestoreStored(entry, owner, item.x, item.y)
                    // Footprint migration (October 31, 2026, larger tools): a backpack item that no longer fits its old spot
                    // goes to overflow (CLAIM OVERFLOW returns it) instead of invalidating the save.
                    && !(owner == ItemOwner.Backpack && inventory.RestoreStored(entry, ItemOwner.Overflow, 0, 0))) throw new InvalidDataException("Invalid or overlapping item placement");
                if (item.dropped && !inventory.RestoreDropped(entry)) throw new InvalidDataException("Only trail items can be marked as dropped");
                if (data.version == 3)
                {
                    bool onTrail = owner == ItemOwner.World;
                    if (onTrail != !string.IsNullOrEmpty(item.segmentId) || (onTrail && (catalog.Segment(item.segmentId) == null || !inventory.AssignSegment(entry, item.segmentId)))) throw new InvalidDataException("Trail items need a known segment; carried items have none");
                }
            }
            foreach (var record in wornRecords)
                if (!record.Key.StartsWith("debug-", StringComparison.Ordinal) && !wornItems.Contains(record.Key)) throw new InvalidDataException("Worn item is missing from inventory ledger");
            if (savedHome != null)
            {
                var takenIds = new HashSet<string>(); foreach (SavedItem bagItem in data.items) takenIds.Add(bagItem.id);
                RestoreContainers(data, savedHome, takenIds, PendingBatchItemIds(data));
            }
            // Slice 2b: every house escrow id names an Escrow item in this save that no trade listing also names.
            if (savedHome != null)
            {
                var tradeEscrow = new List<string>();
                if (data.trade?.listings != null) foreach (var listing in data.trade.listings) if (listing != null && !string.IsNullOrEmpty(listing.escrowId)) tradeEscrow.Add(listing.escrowId);
                try { TheTrailWalker.Domain.Settlement.HouseUpgradeService.CheckStructure(savedHome.UpgradeEscrow, inventory.Entries, tradeEscrow); }
                catch (ArgumentException error) { throw new InvalidDataException("Invalid house deposits: " + error.Message); }
            }
        }

        // Definition lookup, AddWorld, spare wear and pile for one saved item, into a private ledger. Callers place the entry.
        private static ItemEntry BuildEntry(SpatialInventory into, SavedItem item)
        {
            ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(item.definitionId);
            if (def == null) throw new InvalidDataException("Unknown item definition: " + item.definitionId);
            var entry = into.AddWorld(item.id, def, item.quantity, item.quality, new ItemProfile(item.unitValue, item.maxDurability, item.durability, item.variant), item.ready, item.trailDistance);
            if (!into.RestoreWear(entry, item.hasWear, item.wearRemaining)) throw new InvalidDataException("Invalid spare wear state");
            if (item.pile != null && item.pile.Count > 0)
            {
                var portions = new List<PilePortion>(item.pile.Count);
                foreach (SavedPortion saved in item.pile)
                {
                    if (saved == null) throw new InvalidDataException("Invalid stack pile");
                    portions.Add(new PilePortion(saved.quality, new ItemProfile(saved.unitValue, saved.maxDurability, saved.durability, saved.variant), saved.quantity));
                }
                if (!into.RestorePile(entry, portions)) throw new InvalidDataException("Stack pile does not match its quantity");
            }
            return entry;
        }

        private static HashSet<string> PendingBatchItemIds(SessionSnapshot data)
        {
            var ids = new HashSet<string>();
            if (data?.crafting?.batches != null) foreach (SavedBatch batch in data.crafting.batches) if (batch?.itemIds != null) foreach (string id in batch.itemIds) ids.Add(id);
            return ids;
        }

        // Milestone 11: chest and rack contents of a loaded save, with the taken and pending ids derived from the snapshot.
        public static TheTrailWalker.Domain.Settlement.HomeStorage RestoreContainers(SessionSnapshot data, TheTrailWalker.Domain.Settlement.HomeState home)
        {
            var taken = new HashSet<string>(); if (data?.items != null) foreach (SavedItem item in data.items) if (item != null) taken.Add(item.id);
            return RestoreContainers(data, home, taken, PendingBatchItemIds(data));
        }
        // The ids never overlap the bag's or each other's, and a not-ready item must belong to an outstanding batch. Nothing is
        // returned unless every container is valid; placed chests and racks without a record start empty.
        public static TheTrailWalker.Domain.Settlement.HomeStorage RestoreContainers(SessionSnapshot data, TheTrailWalker.Domain.Settlement.HomeState home, ISet<string> takenIds, ISet<string> pendingBatchItemIds)
        {
            var storage = new TheTrailWalker.Domain.Settlement.HomeStorage();
            List<SavedContainer> saved = data?.home != null && data.home.block != 0 ? data.home.containers : null;
            if (saved != null && saved.Count > 0)
            {
                var seenContainers = new HashSet<string>(); var seenItems = new HashSet<string>(takenIds ?? new HashSet<string>());
                try
                {
                    foreach (SavedContainer box in saved)
                    {
                        if (box == null || string.IsNullOrEmpty(box.pieceId) || box.items == null || !seenContainers.Add(box.pieceId)) throw new InvalidDataException("bad or duplicate container");
                        TheTrailWalker.Domain.Settlement.FurniturePiece piece = null;
                        foreach (var candidate in home.Pieces) if (candidate.InstanceId == box.pieceId) { piece = candidate; break; }
                        if (piece == null || !piece.Placed || !TheTrailWalker.Domain.Settlement.HomeStorage.IsContainerDefinition(piece.Definition)) throw new InvalidDataException("not a placed chest or rack");
                        bool chestPiece = piece.Definition.IsChest;
                        if (box.kind != (chestPiece ? "chest" : "rack")) throw new InvalidDataException("container kind does not match its piece");
                        if (box.items.Count > TheTrailWalker.Domain.Settlement.HomeStorage.MaxItemsPerContainer) throw new InvalidDataException("too many items");
                        SpatialInventory chest = null; TheTrailWalker.Domain.Settlement.ToolRack rack = null; var scratch = new SpatialInventory();
                        if (chestPiece)
                        {
                            (int width, int height) = TheTrailWalker.Domain.Settlement.HomeStorage.ChestSize(piece.DefinitionId);
                            chest = new SpatialInventory(width * 1000, height * 1000); chest.IsContainer = true;
                        }
                        else rack = new TheTrailWalker.Domain.Settlement.ToolRack();
                        foreach (SavedItem item in box.items)
                        {
                            if (item == null || string.IsNullOrWhiteSpace(item.id) || !seenItems.Add(item.id) || item.owner != nameof(ItemOwner.Backpack) || item.dropped || !string.IsNullOrEmpty(item.segmentId)
                                || float.IsNaN(item.trailDistance) || float.IsInfinity(item.trailDistance) || item.trailDistance < 0 || item.trailDistance > 10000f
                                || float.IsNaN(item.worldX) || float.IsInfinity(item.worldX) || float.IsNaN(item.worldY) || float.IsInfinity(item.worldY) || float.IsNaN(item.worldZ) || float.IsInfinity(item.worldZ)
                                || (!item.ready && (pendingBatchItemIds == null || !pendingBatchItemIds.Contains(item.id)))) throw new InvalidDataException("bad item record");
                            if (chestPiece)
                            {
                                ItemEntry entry = BuildEntry(chest, item);
                                if (!chest.RestoreStored(entry, ItemOwner.Backpack, item.x, item.y)) throw new InvalidDataException("item overlaps or leaves the chest");
                            }
                            else
                            {
                                if (item.y != 0) throw new InvalidDataException("bad rack spot");
                                ItemEntry entry = BuildEntry(scratch, item);
                                if (!rack.Restore(entry, item.x)) throw new InvalidDataException("bad, repeated or stackable rack item");
                            }
                        }
                        if (chestPiece) storage.AddChest(box.pieceId, chest); else storage.AddRack(box.pieceId, rack);
                    }
                }
                catch (ArgumentException error) { throw new InvalidDataException("Invalid home storage: " + error.Message); }
                catch (InvalidDataException error) when (!error.Message.StartsWith("Invalid home storage")) { throw new InvalidDataException("Invalid home storage: " + error.Message); }
            }
            storage.Sync(home);
            return storage;
        }

        public static List<SavedContainer> SaveContainers(TheTrailWalker.Domain.Settlement.HomeStorage storage)
        {
            var result = new List<SavedContainer>();
            if (storage == null) return result;
            var ids = new List<string>(storage.PieceIds); ids.Sort(StringComparer.Ordinal);
            foreach (string pieceId in ids)
            {
                SpatialInventory chest = storage.Chest(pieceId);
                var box = new SavedContainer { pieceId = pieceId, kind = chest != null ? "chest" : "rack" };
                if (chest != null)
                {
                    foreach (ItemEntry item in chest.Entries) if (item.Owner == ItemOwner.Backpack) box.items.Add(SaveContainerItem(item, item.X, item.Y));
                    box.items.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
                }
                else
                {
                    TheTrailWalker.Domain.Settlement.ToolRack rack = storage.Rack(pieceId);
                    for (int spot = 0; spot < TheTrailWalker.Domain.Settlement.ToolRack.Spots; spot++) if (rack[spot] != null) box.items.Add(SaveContainerItem(rack[spot], spot, 0));
                }
                result.Add(box);
            }
            return result;
        }
        private static SavedItem SaveContainerItem(ItemEntry item, int x, int y)
        {
            var saved = new SavedItem { id = item.Id, definitionId = item.Definition.Id, owner = nameof(ItemOwner.Backpack), quantity = item.Quantity,
                quality = item.Quality, ready = item.Ready, x = x, y = y, unitValue = item.Profile.UnitValue,
                maxDurability = item.Profile.MaxDurability, durability = item.Profile.Durability, variant = item.Profile.Variant,
                hasWear = item.HasWearRemaining, wearRemaining = item.WearRemaining, trailDistance = item.TrailDistance, pile = new List<SavedPortion>() };
            foreach (PilePortion p in item.Pile)
                saved.pile.Add(new SavedPortion { quality = p.Quality, quantity = p.Quantity, unitValue = p.Profile.UnitValue, maxDurability = p.Profile.MaxDurability, durability = p.Profile.Durability, variant = p.Profile.Variant });
            return saved;
        }

        // Economy 0 -> 1 (Milestone 9): unit values become minor units (1/100 cash). Levelled crafts keep their snapshot (x100);
        // everything else is re-derived from the new price data (older drops carried a placeholder value of 100).
        public static void MigrateEconomy(SessionSnapshot data)
        {
            if (data == null || data.economy >= 1) return;
            foreach (SavedItem item in data.items)
            {
                if (item == null) continue;
                ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(item.definitionId);
                if (def == null) continue;
                item.unitValue = TheTrailWalker.Domain.Economy.ItemPrices.MigrateLegacyValue(def, item.quality, item.unitValue);
                if (item.pile != null) foreach (SavedPortion portion in item.pile) if (portion != null) portion.unitValue = TheTrailWalker.Domain.Economy.ItemPrices.MigrateLegacyValue(def, portion.quality, portion.unitValue);
            }
            if (data.crafting != null)
                foreach (SavedBatch batch in data.crafting.batches)
                {
                    var recipe = TheTrailWalker.Domain.Crafting.RecipeBook.Find(batch?.recipeId);
                    if (recipe?.Output != null) batch.unitValue = TheTrailWalker.Domain.Economy.ItemPrices.MigrateLegacyValue(recipe.Output, batch.quality, batch.unitValue);
                }
            data.economy = 1;
        }

        // A missing block (older save) is an undiscovered settlement with the caller's seed.
        public static SettlementState RestoreSettlement(SessionSnapshot data, int newSeed)
        {
            SavedSettlement saved = data?.settlement;
            if (saved == null || saved.block == 0) return SettlementState.New(newSeed);
            if (saved.neighbours == null || saved.countedRegions == null || saved.neighbours.Exists(n => n == null)) throw new InvalidDataException("Invalid settlement block");
            try { return SettlementState.Restore(saved.discovered, saved.plotIndex, saved.seed, saved.neighbours.ConvertAll(n => (n.plot, n.level, n.countdown)), saved.countedRegions); }
            catch (ArgumentException error) { throw new InvalidDataException("Invalid settlement block: " + error.Message); }
        }
        public static SavedSettlement SaveSettlement(SettlementState s)
        {
            var saved = new SavedSettlement { block = 1, discovered = s.Discovered, plotIndex = s.PlotIndex, seed = s.Seed, countedRegions = new List<string>(s.CountedRegions) };
            foreach (var n in s.Neighbours) saved.neighbours.Add(new SavedNeighbour { plot = n.Plot, level = n.Level, countdown = n.Countdown });
            return saved;
        }

        // A missing block (older save) is a new level-1 home.
        public static TheTrailWalker.Domain.Settlement.HomeState RestoreHome(SessionSnapshot data)
        {
            SavedHome saved = data?.home;
            if (saved == null || saved.block == 0) return TheTrailWalker.Domain.Settlement.HomeState.New();
            if (saved.pieces == null || saved.firstPurchases == null || saved.pieces.Exists(p => p == null)) throw new InvalidDataException("Invalid home block");
            try { return TheTrailWalker.Domain.Settlement.HomeState.Restore(saved.level, saved.pieces.ConvertAll(p => (p.id, p.definition, p.placed, p.x, p.z, p.rotation)), saved.firstPurchases, saved.upgradeEscrow); }
            catch (ArgumentException error) { throw new InvalidDataException("Invalid home block: " + error.Message); }
        }
        public static SavedHome SaveHome(TheTrailWalker.Domain.Settlement.HomeState h)
        {
            var saved = new SavedHome { block = 1, level = h.Level, firstPurchases = new List<string>(h.FirstPurchases), upgradeEscrow = new List<string>(h.UpgradeEscrow) };
            foreach (var p in h.Pieces) saved.pieces.Add(new SavedPiece { id = p.InstanceId, definition = p.DefinitionId, placed = p.Placed, x = p.X, z = p.Z, rotation = p.Rotation });
            return saved;
        }

        public static TravelState RestoreTravel(SessionSnapshot data, TravelCatalog catalog)
        {
            SavedTravel saved = data?.travel;
            if (saved == null || saved.visited == null || saved.discovered == null || saved.paidTolls == null || saved.cycles == null) throw new InvalidDataException("Missing travel state");
            var cycles = new List<KeyValuePair<string, int>>();
            foreach (SavedCycle cycle in saved.cycles) { if (cycle == null) throw new InvalidDataException("Invalid drop cycle"); cycles.Add(new KeyValuePair<string, int>(cycle.segmentId, cycle.cycle)); }
            try { return TravelState.Restore(catalog, saved.campId, saved.seated, saved.lastTrailCampId, saved.visitNumber, saved.cash, saved.visited, saved.discovered, saved.paidTolls, cycles, saved.gold, saved.farBank, saved.settlementApproach, saved.inSettlement, saved.settlementReturnCampId); }
            catch (ArgumentException error) { throw new InvalidDataException(error.Message); }
        }
        public static SavedTravel SaveTravel(TravelState travel)
        {
            var saved = new SavedTravel { campId = travel.CurrentCampId, lastTrailCampId = travel.LastTrailCampId, seated = travel.Seated, visitNumber = travel.VisitNumber, cash = travel.Cash, gold = travel.Gold, farBank = travel.OnFarBank, settlementApproach = travel.OnSettlementApproach, inSettlement = travel.InSettlement, settlementReturnCampId = travel.SettlementReturnCampId,
                visited = new List<string>(travel.VisitedCamps), discovered = new List<string>(travel.DiscoveredRegions), paidTolls = new List<string>(travel.PaidTolls) };
            foreach (var cycle in travel.DropCycles) saved.cycles.Add(new SavedCycle { segmentId = cycle.Key, cycle = cycle.Value });
            return saved;
        }

        // Copies a Milestone 4 profile into the camp-based schema: seated at the first camp, carried items, gear and
        // stamina preserved. Items left on the old single prototype trail have no segment and are not carried over.
        public static SessionSnapshot MigrateV2(SessionSnapshot source)
        {
            if (source == null || source.version != 2) throw new InvalidDataException("Expected a version 2 snapshot");
            Validate(source, float.MaxValue, out _);
            TravelState travel = TravelState.Start(TravelCatalog.Prototype());
            var migrated = new SessionSnapshot { version = 3, segmentId = travel.CurrentSegment.Id, distance = 0, requestedSpeed = 0, speedCap = source.speedCap, inventoryResume = 0,
                yaw = source.yaw, inventoryOpen = false, postHarvest = source.survival.fainted, camp = false, survival = source.survival, travel = SaveTravel(travel) };
            foreach (SavedItem item in source.items) if (item.owner != nameof(ItemOwner.World)) migrated.items.Add(item);
            Validate(migrated, float.MaxValue, out _); return migrated;
        }

        public static SessionSnapshot MigrateV1(SessionSnapshot source, float pathLength)
        {
            if (source == null || source.version != 1) throw new InvalidDataException("Expected a version 1 snapshot");
            Validate(source, pathLength, out _);
            var migrated = new SessionSnapshot { version = 2, segmentId = source.segmentId, distance = source.distance,
                requestedSpeed = Math.Min(source.requestedSpeed, MovementState.StarterSpeedCap), speedCap = MovementState.StarterSpeedCap, inventoryResume = Math.Min(source.inventoryResume, MovementState.StarterSpeedCap),
                yaw = source.yaw, inventoryOpen = source.inventoryOpen, postHarvest = source.postHarvest, camp = source.camp,
                items = new List<SavedItem>(source.items), survival = new SavedSurvival() };
            Validate(migrated, pathLength, out _); return migrated;
        }
    }
}
