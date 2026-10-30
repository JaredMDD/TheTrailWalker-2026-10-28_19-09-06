using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Equipment;

namespace TheTrailWalker.Domain.Items
{
    public enum ItemOwner { World, Backpack, Consumed, Pocket, Hook, Overflow, Worn }
    public sealed class ItemDefinition
    {
        public readonly string Id, Name;
        public readonly int Width, Height;
        public readonly bool Stackable;
        public readonly GearSlot? GearSlot;
        public readonly int GearRank;
        // Hearts one serving queues; zero means not food. Only prepared (crafted/bought) food is auto-eaten at zero stamina.
        public readonly int FoodHearts;
        public readonly bool PreparedFood;
        public ItemDefinition(string id, string name, int width, int height, bool stackable = true, GearSlot? gearSlot = null, int gearRank = 0, int foodHearts = 0, bool preparedFood = false)
        {
            if (string.IsNullOrWhiteSpace(id) || width <= 0 || height <= 0 || (gearSlot.HasValue && (!Enum.IsDefined(typeof(GearSlot), gearSlot.Value) || stackable || gearRank < 1 || gearRank > 4)) || (!gearSlot.HasValue && gearRank != 0) ||
                foodHearts < 0 || (foodHearts > 0 && (!stackable || gearSlot.HasValue)) || (preparedFood && foodHearts == 0)) throw new ArgumentException("Invalid definition");
            Id = id; Name = name; Width = width; Height = height; Stackable = stackable; GearSlot = gearSlot; GearRank = gearRank; FoodHearts = foodHearts; PreparedFood = preparedFood;
        }
    }
    // Integer fixed-point statistics: equal functional profiles merge regardless of provenance.
    public readonly struct ItemProfile : IEquatable<ItemProfile>
    {
        public readonly long UnitValue;
        public readonly int MaxDurability, Durability;
        public readonly string Variant;
        public ItemProfile(long value, int maximum = 0, int durability = 0, string variant = "")
        { UnitValue = value; MaxDurability = maximum; Durability = durability; Variant = variant ?? ""; }
        public bool Equals(ItemProfile other) => UnitValue == other.UnitValue && MaxDurability == other.MaxDurability && Durability == other.Durability && (Variant ?? "") == (other.Variant ?? "");
        public override bool Equals(object obj) => obj is ItemProfile other && Equals(other);
        public override int GetHashCode() => UnitValue.GetHashCode() ^ MaxDurability ^ Durability ^ (Variant ?? "").GetHashCode();
    }
    public sealed class ItemEntry
    {
        public string Id { get; }
        public ItemDefinition Definition { get; }
        public int Quality { get; }
        public ItemProfile Profile { get; }
        public bool Ready { get; }
        public int Quantity { get; internal set; }
        public ItemOwner Owner { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public float TrailDistance { get; internal set; }
        // Set when the player drops the entry onto the trail; such entries are removed by the next drop cycle.
        public bool Dropped { get; internal set; }
        public bool HasWearRemaining { get; internal set; }
        public double WearRemaining { get; internal set; }
        internal ItemEntry(string id, ItemDefinition definition, int quantity, int quality, ItemProfile profile, bool ready, float trailDistance)
        {
            if (string.IsNullOrWhiteSpace(id) || definition == null || quantity < 1 || quantity > (definition.Stackable ? 1000 : 1) || quality < 1 || quality > 5 ||
                (definition.GearSlot.HasValue && (profile.MaxDurability < 0 || profile.Durability < 0 || (profile.MaxDurability > 0 && profile.Durability > profile.MaxDurability))))
                throw new ArgumentException("Invalid item identity, quantity or quality");
            Id = id; Definition = definition; Quantity = quantity; Quality = quality; Profile = profile; Ready = ready; TrailDistance = trailDistance;
            if (definition.GearSlot.HasValue && profile.MaxDurability > 0 && profile.Durability > 0) { HasWearRemaining = true; WearRemaining = profile.Durability; }
        }
        public bool CanMerge(ItemEntry other) => other != this && Definition.Stackable && other.Definition.Stackable && Definition.Id == other.Definition.Id && Quality == other.Quality && Ready == other.Ready && Profile.Equals(other.Profile);
        public static double Scale(int quantity) => quantity <= 0 || quantity > 1000 ? throw new ArgumentOutOfRangeException(nameof(quantity)) : 1 + 0.5 * Math.Log(quantity) / Math.Log(1000);
        public int WidthFor(int quantity) => Definition.Stackable ? (int)Math.Ceiling(Definition.Width * Scale(quantity)) : Definition.Width;
        public int HeightFor(int quantity) => Definition.Stackable ? (int)Math.Ceiling(Definition.Height * Scale(quantity)) : Definition.Height;
        public int Width => WidthFor(Quantity);
        public int Height => HeightFor(Quantity);
    }
    public static class PrototypeItems
    {
        public static readonly ItemDefinition Branch = new ItemDefinition("material.pine_branch", "Pine branch", 1600, 600);
        public static readonly ItemDefinition Log = new ItemDefinition("material.pine_log", "Pine log", 1800, 900);
        public static readonly ItemDefinition Stone = new ItemDefinition("material.stone", "Stone", 1000, 1000);
        public static readonly ItemDefinition Hemp = new ItemDefinition("material.hemp", "Hemp", 1000, 700);
        public static readonly ItemDefinition Berries = new ItemDefinition("food.berries", "Berries", 800, 800, foodHearts: 1);
        // Debug twelve-heart prepared-food fixture for section 4.4.3 until crafting/bakery exist; not a recipe or bakery item.
        public static readonly ItemDefinition TestBread = new ItemDefinition("debug.food.bread", "Test premium food", 900, 700, foodHearts: 12, preparedFood: true);
        public static readonly ItemDefinition Tool = new ItemDefinition("tool.pine_axe.normal", "Pine axe", 1800, 700, false);
        public static readonly ItemDefinition TestHat = new ItemDefinition("debug.gear.hat.r4", "Test hat", 1000, 1000, false, Equipment.GearSlot.Hat, 4);
        public static readonly ItemDefinition TestBoots = new ItemDefinition("debug.gear.boots.r4", "Test boots", 1000, 1000, false, Equipment.GearSlot.Boots, 4);
        public static readonly ItemDefinition TestShirt = new ItemDefinition("debug.gear.shirt.r4", "Test shirt", 1000, 1000, false, Equipment.GearSlot.Shirt, 4);
        public static readonly ItemDefinition TestPants = new ItemDefinition("debug.gear.pants.r4", "Test pants", 1000, 1000, false, Equipment.GearSlot.Pants, 4);
        public static readonly ItemDefinition TestBackpack = new ItemDefinition("debug.gear.pack.r4", "Test pack", 1000, 1000, false, Equipment.GearSlot.Backpack, 4);
        public static readonly ItemDefinition[] TestWearables = { TestHat, TestBoots, TestShirt, TestPants, TestBackpack };
        public static readonly ItemDefinition[] All = { Branch, Log, Stone, Hemp, Berries, Tool, TestHat, TestBoots, TestShirt, TestPants, TestBackpack, TestBread };
    }
}
