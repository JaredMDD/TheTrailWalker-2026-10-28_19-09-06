using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Items;

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
        // Set only for special balloon gear (its own stat instead of the rank table); saved with the gear. Other infinite gear keeps rank stats.
        public double? StatOverride { get; }

        public WornGear(string instanceId, GearSlot slot, int rank, int quality, double remainingDistance, bool infinite = false, double? statOverride = null)
        {
            if (string.IsNullOrWhiteSpace(instanceId) || !Enum.IsDefined(typeof(GearSlot), slot) || rank < 1 || rank > 4 || quality < 1 || quality > 5 || double.IsNaN(remainingDistance) || double.IsInfinity(remainingDistance) || (!infinite && remainingDistance <= 0) || (statOverride.HasValue && (!infinite || double.IsNaN(statOverride.Value) || statOverride.Value <= 0)))
                throw new ArgumentException("Invalid worn equipment");
            InstanceId = instanceId; Slot = slot; Rank = rank; Quality = quality;
            RemainingDistance = remainingDistance; Infinite = infinite;
            StatOverride = statOverride;
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
        public double SpeedCap => Get(GearSlot.Boots)?.StatOverride ?? MovementState.StarterSpeedCap + Rank(GearSlot.Boots) * 0.75; // 1.5 m/s starter up to 4.5 m/s at rank four (1.5x, October 31, 2026)
        public int MaxHearts => (int)(Get(GearSlot.Hat)?.StatOverride ?? Hearts[Rank(GearSlot.Hat)]);
        public int Pockets => (int)(Get(GearSlot.Shirt)?.StatOverride ?? Capacity[Rank(GearSlot.Shirt)]);
        public int Hooks => (int)(Get(GearSlot.Pants)?.StatOverride ?? Capacity[Rank(GearSlot.Pants)]);
        public int BackpackWidth => (int)(Get(GearSlot.Backpack)?.StatOverride ?? BagWidth[Rank(GearSlot.Backpack)]);
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
