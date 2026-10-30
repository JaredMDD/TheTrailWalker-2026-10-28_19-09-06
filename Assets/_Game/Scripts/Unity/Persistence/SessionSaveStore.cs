using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TheTrailWalker.Domain;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Equipment;
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
    }
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
    }
    [Serializable] internal sealed class SaveEnvelope { public int version = 1; public string payload, sha256; }
    public enum LoadStatus { Fresh, Loaded, Recovered, Invalid, Unsupported }

    public sealed class SessionSaveStore
    {
        private readonly string path, backup, temporary;
        private readonly IClock clock;
        private readonly int latestSchema;
        private bool recovered;
        public SessionSaveStore(string directory, IClock time, int schemaVersion = 1)
        {
            if (schemaVersion != 1 && schemaVersion != 2) throw new ArgumentOutOfRangeException(nameof(schemaVersion));
            latestSchema = schemaVersion; path = Path.Combine(directory, "session-v" + schemaVersion + ".json"); backup = path + ".bak"; temporary = path + ".tmp"; clock = time ?? throw new ArgumentNullException(nameof(time));
        }
        public string MainPath => path;
        public string BackupPath => backup;
        public void ArchiveForReset()
        {
            string suffix = ".reset-" + clock.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N");
            if (File.Exists(path)) File.Move(path, path + suffix);
            if (File.Exists(backup)) File.Move(backup, backup + suffix);
            recovered = false;
        }

        private static string Hash(string value)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }
        public void Save(SessionSnapshot snapshot)
        {
            if (snapshot == null || snapshot.version != latestSchema) throw new InvalidDataException("Unexpected save schema for this store");
            Validate(snapshot, float.MaxValue, out _);
            snapshot.savedUtc = clock.UtcNow.ToString("O");
            string payload = JsonUtility.ToJson(snapshot);
            string json = JsonUtility.ToJson(new SaveEnvelope { payload = payload, sha256 = Hash(payload) });
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None)) { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (File.Exists(path)) File.Replace(temporary, path, recovered ? null : backup);
            else File.Move(temporary, path);
            recovered = false;
        }
        public LoadStatus Load(float pathLength, out SessionSnapshot snapshot, out string message)
        {
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
                Validate(snapshot, length, out _); return LoadStatus.Loaded;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is InvalidDataException || e is FormatException || e is InvalidOperationException)
            { snapshot = null; message = e.Message; return LoadStatus.Invalid; }
        }
        public static void Validate(SessionSnapshot data, float pathLength, out SpatialInventory inventory)
        {
            if (data == null || (data.version != 1 && data.version != 2) || data.segmentId != "prototype.trail.1" || data.items == null || data.items.Count > 1024 || float.IsNaN(data.yaw) || float.IsInfinity(data.yaw) || data.distance > pathLength) throw new InvalidDataException("Invalid session header");
            var movement = new MovementState(); movement.Restore(data.distance, data.requestedSpeed, data.speedCap, data.inventoryResume, data.inventoryOpen, data.postHarvest, data.camp);
            if (data.camp != (data.distance >= pathLength) && pathLength != float.MaxValue) throw new InvalidDataException("Invalid camp position");
            var gear = new EquipmentState(); var wornIds = new HashSet<string>(); var wornRecords = new Dictionary<string, SavedGear>(); var wornItems = new HashSet<string>();
            if (data.version == 2)
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
            if (data.version == 2 && !inventory.Reconfigure(gear.BackpackWidth * 1000, gear.BackpackHeight * 1000, gear.Pockets, gear.Hooks, out _)) throw new InvalidDataException("Invalid storage capabilities");
            foreach (SavedItem item in data.items)
            {
                if (item == null || !Enum.TryParse(item.owner, false, out ItemOwner owner) || (owner != ItemOwner.World && owner != ItemOwner.Backpack && (data.version == 1 || (owner != ItemOwner.Pocket && owner != ItemOwner.Hook && owner != ItemOwner.Overflow && owner != ItemOwner.Worn))) || (wornIds.Contains(item.id) && owner != ItemOwner.Worn) || float.IsNaN(item.trailDistance) || float.IsInfinity(item.trailDistance) || item.trailDistance < 0 || item.trailDistance > pathLength || float.IsNaN(item.worldX) || float.IsInfinity(item.worldX) || float.IsNaN(item.worldY) || float.IsInfinity(item.worldY) || float.IsNaN(item.worldZ) || float.IsInfinity(item.worldZ)) throw new InvalidDataException("Invalid item record");
                ItemDefinition def = Array.Find(PrototypeItems.All, d => d.Id == item.definitionId);
                if (def == null) throw new InvalidDataException("Unknown item definition: " + item.definitionId);
                if (owner == ItemOwner.Worn)
                {
                    if (!wornRecords.TryGetValue(item.id, out SavedGear worn) || !def.GearSlot.HasValue || def.GearSlot.Value.ToString() != worn.slot || def.GearRank != worn.rank || item.quality != worn.quality || !wornItems.Add(item.id)) throw new InvalidDataException("Worn item and gear slot disagree");
                    if (item.hasWear && !worn.infinite && Math.Abs(item.wearRemaining - worn.remainingDistance) > 0.0001) throw new InvalidDataException("Worn item durability and gear slot disagree");
                    if (!worn.infinite && item.maxDurability > 0 && worn.remainingDistance > item.maxDurability + 0.0001) throw new InvalidDataException("Worn item exceeds its maximum durability");
                }
                var entry = inventory.AddWorld(item.id, def, item.quantity, item.quality, new ItemProfile(item.unitValue, item.maxDurability, item.durability, item.variant), item.ready, item.trailDistance);
                if (!inventory.RestoreWear(entry, item.hasWear, item.wearRemaining)) throw new InvalidDataException("Invalid spare wear state");
                if (owner != ItemOwner.World && !inventory.RestoreStored(entry, owner, item.x, item.y)) throw new InvalidDataException("Invalid or overlapping item placement");
                if (item.dropped && !inventory.RestoreDropped(entry)) throw new InvalidDataException("Only trail items can be marked as dropped");
            }
            foreach (var record in wornRecords)
                if (!record.Key.StartsWith("debug-", StringComparison.Ordinal) && !wornItems.Contains(record.Key)) throw new InvalidDataException("Worn item is missing from inventory ledger");
        }

        public static SessionSnapshot MigrateV1(SessionSnapshot source, float pathLength)
        {
            if (source == null || source.version != 1) throw new InvalidDataException("Expected a version 1 snapshot");
            Validate(source, pathLength, out _);
            var migrated = new SessionSnapshot { version = 2, segmentId = source.segmentId, distance = source.distance,
                requestedSpeed = Math.Min(source.requestedSpeed, 0.8f), speedCap = 0.8f, inventoryResume = Math.Min(source.inventoryResume, 0.8f),
                yaw = source.yaw, inventoryOpen = source.inventoryOpen, postHarvest = source.postHarvest, camp = source.camp,
                items = new List<SavedItem>(source.items), survival = new SavedSurvival() };
            Validate(migrated, pathLength, out _); return migrated;
        }
    }
}
