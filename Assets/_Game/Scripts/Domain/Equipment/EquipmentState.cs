using System;
using System.Collections.Generic;

namespace TheTrailWalker.Domain.Equipment
{
    public enum GearSlot { Hat, Boots, Shirt, Pants, Backpack }

    public sealed class WornGear
    {
        public string InstanceId { get; }
        public GearSlot Slot { get; }
        public int Rank { get; }
        public int Quality { get; }
        public double RemainingDistance { get; private set; }
        public bool Infinite { get; }

        public WornGear(string instanceId, GearSlot slot, int rank, int quality, double remainingDistance, bool infinite = false)
        {
            if (string.IsNullOrWhiteSpace(instanceId) || !Enum.IsDefined(typeof(GearSlot), slot) || rank < 1 || rank > 4 || quality < 1 || quality > 5 || double.IsNaN(remainingDistance) || double.IsInfinity(remainingDistance) || (!infinite && remainingDistance <= 0))
                throw new ArgumentException("Invalid worn equipment");
            InstanceId = instanceId; Slot = slot; Rank = rank; Quality = quality;
            RemainingDistance = remainingDistance; Infinite = infinite;
        }
        // The base and quality increment are provisional test data; creation snapshots the result.
        public static WornGear CreateTestGear(string id, GearSlot slot, int rank, int quality, double baseDistance)
        {
            if (double.IsNaN(baseDistance) || double.IsInfinity(baseDistance) || baseDistance <= 0 || quality < 1 || quality > 5) throw new ArgumentException("Invalid test durability");
            return new WornGear(id, slot, rank, quality, baseDistance * (1 + 0.2 * (quality - 1)));
        }
        internal bool Wear(double distance)
        {
            if (Infinite) return false;
            RemainingDistance = Math.Max(0, RemainingDistance - distance);
            return RemainingDistance == 0;
        }
    }

    public sealed class EquipmentState
    {
        private readonly Dictionary<GearSlot, WornGear> worn = new Dictionary<GearSlot, WornGear>();
        private static readonly int[] Hearts = { 3, 4, 6, 9, 12 };
        private static readonly int[] Capacity = { 0, 2, 4, 7, 10 };
        private static readonly int[] BagWidth = { 6, 8, 10, 12, 14 };
        public WornGear Get(GearSlot slot) => worn.TryGetValue(slot, out WornGear gear) ? gear : null;
        private int Rank(GearSlot slot) => Get(slot)?.Rank ?? 0;
        public double SpeedCap => MovementState.StarterSpeedCap + Rank(GearSlot.Boots) * 0.5; // 1.0 m/s starter up to 3.0 m/s at rank four
        public int MaxHearts => Hearts[Rank(GearSlot.Hat)];
        public int Pockets => Capacity[Rank(GearSlot.Shirt)];
        public int Hooks => Capacity[Rank(GearSlot.Pants)];
        public int BackpackWidth => BagWidth[Rank(GearSlot.Backpack)];
        public int BackpackHeight => 4;

        public void Equip(WornGear gear)
        {
            if (gear == null) throw new ArgumentNullException(nameof(gear));
            if (worn.ContainsKey(gear.Slot)) throw new InvalidOperationException("Occupied equipment must be removed through an inventory transaction before replacement");
            foreach (var existing in worn.Values)
                if (existing.InstanceId == gear.InstanceId && existing.Slot != gear.Slot) throw new InvalidOperationException("Equipment instance is already worn");
            worn[gear.Slot] = gear;
        }

        // The caller must transfer contents and persist the full state atomically with this result.
        public IReadOnlyList<WornGear> ApplyDistance(double distance)
        {
            if (double.IsNaN(distance) || double.IsInfinity(distance) || distance < 0) throw new ArgumentOutOfRangeException(nameof(distance));
            var expired = new List<WornGear>();
            foreach (var gear in worn.Values) if (gear.Wear(distance)) expired.Add(gear);
            foreach (var gear in expired) worn.Remove(gear.Slot);
            return expired;
        }
        public WornGear ExpireForTest(GearSlot slot)
        {
            WornGear gear = Get(slot);
            if (gear == null || gear.Infinite) return null;
            gear.Wear(gear.RemainingDistance); worn.Remove(slot); return gear;
        }
        public WornGear Unequip(GearSlot slot)
        {
            WornGear gear = Get(slot); if (gear == null) return null;
            worn.Remove(slot); return gear;
        }
    }
}
