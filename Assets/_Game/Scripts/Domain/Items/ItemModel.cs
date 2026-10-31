using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Equipment;

namespace TheTrailWalker.Domain.Items
{
    public enum ItemOwner { World, Backpack, Consumed, Pocket, Hook, Overflow, Worn }
    // Raw/refined ingredients are recipe inputs only; raw food can be eaten manually; only prepared food is auto-eaten.
    public enum ItemCategory { Material, RawIngredient, RefinedIngredient, RawFood, PreparedFood, Tool, Wearable }
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
        public readonly ItemCategory Category;
        public ItemDefinition(string id, string name, int width, int height, bool stackable = true, GearSlot? gearSlot = null, int gearRank = 0, int foodHearts = 0, bool preparedFood = false, ItemCategory? category = null)
        {
            if (string.IsNullOrWhiteSpace(id) || width <= 0 || height <= 0 || (gearSlot.HasValue && (!Enum.IsDefined(typeof(GearSlot), gearSlot.Value) || stackable || gearRank < 1 || gearRank > 4)) || (!gearSlot.HasValue && gearRank != 0) ||
                foodHearts < 0 || (foodHearts > 0 && (!stackable || gearSlot.HasValue)) || (preparedFood && foodHearts == 0)) throw new ArgumentException("Invalid definition");
            ItemCategory derived = gearSlot.HasValue ? ItemCategory.Wearable : !stackable ? ItemCategory.Tool : preparedFood ? ItemCategory.PreparedFood : foodHearts > 0 ? ItemCategory.RawFood : ItemCategory.Material;
            ItemCategory chosen = category ?? derived;
            // An explicit category may only refine a non-food stackable material into a raw/refined ingredient.
            if (chosen != derived && !(derived == ItemCategory.Material && (chosen == ItemCategory.RawIngredient || chosen == ItemCategory.RefinedIngredient))) throw new ArgumentException("Category contradicts definition");
            Id = id; Name = name; Width = width; Height = height; Stackable = stackable; GearSlot = gearSlot; GearRank = gearRank; FoodHearts = foodHearts; PreparedFood = preparedFood; Category = chosen;
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
    // One quality/profile portion of a stack. A stack is identified by item ID (and readiness) and keeps its
    // units as a pile of portions, lowest quality first, so consumption can take the lowest quality first.
    public readonly struct PilePortion
    {
        public readonly int Quality, Quantity;
        public readonly ItemProfile Profile;
        public PilePortion(int quality, ItemProfile profile, int quantity) { Quality = quality; Profile = profile; Quantity = quantity; }
    }
    public sealed class ItemEntry
    {
        public string Id { get; }
        public ItemDefinition Definition { get; }
        private readonly List<PilePortion> pile = new List<PilePortion>();
        private readonly PilePortion initial;
        public IReadOnlyList<PilePortion> Pile => pile;
        // Lowest quality/profile in the pile; for unstackable tools and gear this is their only portion.
        public int Quality => pile.Count > 0 ? pile[0].Quality : initial.Quality;
        public int HighestQuality => pile.Count > 0 ? pile[pile.Count - 1].Quality : initial.Quality;
        public ItemProfile Profile => pile.Count > 0 ? pile[0].Profile : initial.Profile;
        public string QualityLabel => Quality == HighestQuality ? "Q" + Quality : $"Q{Quality}-Q{HighestQuality}";
        public bool Ready { get; }
        public int Quantity { get; private set; }
        public ItemOwner Owner { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public float TrailDistance { get; internal set; }
        // Set when the player drops the entry onto the trail; such entries are removed by the next drop cycle.
        public bool Dropped { get; internal set; }
        // Trail segment a world item lies on; only that segment's scene shows it and its drop cycle replaces it.
        public string SegmentId { get; internal set; }
        public bool HasWearRemaining { get; internal set; }
        public double WearRemaining { get; internal set; }
        internal ItemEntry(string id, ItemDefinition definition, int quantity, int quality, ItemProfile profile, bool ready, float trailDistance)
        {
            if (string.IsNullOrWhiteSpace(id) || definition == null || quantity < 1 || quantity > (definition.Stackable ? 1000 : 1) || quality < 1 || quality > 5 ||
                (definition.GearSlot.HasValue && (profile.MaxDurability < 0 || profile.Durability < 0 || (profile.MaxDurability > 0 && profile.Durability > profile.MaxDurability))))
                throw new ArgumentException("Invalid item identity, quantity or quality");
            Id = id; Definition = definition; Ready = ready; TrailDistance = trailDistance;
            initial = new PilePortion(quality, profile, quantity); pile.Add(initial); Quantity = quantity;
            if (definition.GearSlot.HasValue && profile.MaxDurability > 0 && profile.Durability > 0) { HasWearRemaining = true; WearRemaining = profile.Durability; }
        }
        // Stacks combine by item ID and readiness only; quality and price profile live inside the pile.
        public bool CanMerge(ItemEntry other) => other != this && Definition.Stackable && other.Definition.Stackable && Definition.Id == other.Definition.Id && Ready == other.Ready;
        internal void AddPortion(int quality, ItemProfile profile, int quantity)
        {
            for (int i = 0; i < pile.Count; i++)
                if (pile[i].Quality == quality && pile[i].Profile.Equals(profile)) { pile[i] = new PilePortion(quality, profile, pile[i].Quantity + quantity); Quantity += quantity; return; }
            int at = 0;
            while (at < pile.Count && (pile[at].Quality < quality || (pile[at].Quality == quality && pile[at].Profile.UnitValue <= profile.UnitValue))) at++;
            pile.Insert(at, new PilePortion(quality, profile, quantity)); Quantity += quantity;
        }
        // Takes units from the lowest-quality portions first; returns what was removed.
        internal List<PilePortion> TakeLowest(int count)
        {
            var taken = new List<PilePortion>();
            while (count > 0 && pile.Count > 0)
            {
                PilePortion lowest = pile[0]; int used = Math.Min(count, lowest.Quantity);
                taken.Add(new PilePortion(lowest.Quality, lowest.Profile, used));
                if (used == lowest.Quantity) pile.RemoveAt(0); else pile[0] = new PilePortion(lowest.Quality, lowest.Profile, lowest.Quantity - used);
                Quantity -= used; count -= used;
            }
            return taken;
        }
        internal void MoveLowestTo(ItemEntry target, int count) { foreach (PilePortion portion in TakeLowest(count)) target.AddPortion(portion.Quality, portion.Profile, portion.Quantity); }
        internal void Clear() { pile.Clear(); Quantity = 0; }
        internal bool ReplacePile(IReadOnlyList<PilePortion> portions)
        {
            if (portions == null || portions.Count == 0 || (!Definition.Stackable && portions.Count != 1)) return false;
            int total = 0;
            foreach (PilePortion portion in portions)
            { if (portion.Quality < 1 || portion.Quality > 5 || portion.Quantity < 1) return false; total += portion.Quantity; }
            if (total > (Definition.Stackable ? 1000 : 1)) return false;
            pile.Clear(); Quantity = 0;
            foreach (PilePortion portion in portions) AddPortion(portion.Quality, portion.Profile, portion.Quantity);
            return true;
        }
        public static double Scale(int quantity) => quantity <= 0 || quantity > 1000 ? throw new ArgumentOutOfRangeException(nameof(quantity)) : 1 + 0.5 * Math.Log(quantity) / Math.Log(1000);
        public int WidthFor(int quantity) => Definition.Stackable ? (int)Math.Ceiling(Definition.Width * Scale(quantity)) : Definition.Width;
        public int HeightFor(int quantity) => Definition.Stackable ? (int)Math.Ceiling(Definition.Height * Scale(quantity)) : Definition.Height;
        public int Width => WidthFor(Quantity);
        public int Height => HeightFor(Quantity);
    }
    public static class PrototypeItems
    {
        public static readonly ItemDefinition Branch = new ItemDefinition("material.pine_branch", "Pine branch", 1200, 200);
        public static readonly ItemDefinition Log = new ItemDefinition("material.pine_log", "Pine log", 1000, 500);
        public static readonly ItemDefinition Stone = new ItemDefinition("material.stone", "Stone", 1000, 1000);
        public static readonly ItemDefinition Hemp = new ItemDefinition("material.hemp", "Hemp", 850, 300);
        public static readonly ItemDefinition Berries = new ItemDefinition("food.berries", "Red berries", 420, 480, foodHearts: 1); // apple-size rule
        // Debug twelve-heart prepared-food fixture for section 4.4.3 until crafting/bakery exist; not a recipe or bakery item.
        public static readonly ItemDefinition TestBread = new ItemDefinition("debug.food.bread", "Test premium food", 900, 700, foodHearts: 12, preparedFood: true);
        // Tools are among the largest items (user, October 31, 2026), portrait like their upright models.
        public static readonly ItemDefinition Tool = new ItemDefinition("tool.pine_axe.normal", "Pine axe", 1000, 2000, false);
        public static readonly ItemDefinition TestHat = new ItemDefinition("debug.gear.hat.r4", "Test hat", 1000, 1000, false, Equipment.GearSlot.Hat, 4);
        public static readonly ItemDefinition TestBoots = new ItemDefinition("debug.gear.boots.r4", "Test boots", 1000, 1000, false, Equipment.GearSlot.Boots, 4);
        public static readonly ItemDefinition TestShirt = new ItemDefinition("debug.gear.shirt.r4", "Test shirt", 1000, 1000, false, Equipment.GearSlot.Shirt, 4);
        public static readonly ItemDefinition TestPants = new ItemDefinition("debug.gear.pants.r4", "Test pants", 1000, 1000, false, Equipment.GearSlot.Pants, 4);
        public static readonly ItemDefinition TestBackpack = new ItemDefinition("debug.gear.pack.r4", "Test pack", 1000, 1000, false, Equipment.GearSlot.Backpack, 4);
        public static readonly ItemDefinition[] TestWearables = { TestHat, TestBoots, TestShirt, TestPants, TestBackpack };
        // Section 7.1.1 active-tier tree food items: catalog definitions only. Harvesting arrives in Milestone 6 and the
        // recipes/conversions that make the refined items and dishes in Milestone 7; nothing here grants recipe knowledge.
        public static readonly ItemDefinition PineNuts = new ItemDefinition("ingredient.pine_nuts", "Pine nuts", 600, 600, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition BirchSap = new ItemDefinition("ingredient.birch_sap", "Birch sap", 700, 800, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition MesquitePods = new ItemDefinition("ingredient.mesquite_pods", "Mesquite pods", 900, 500, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition Acorns = new ItemDefinition("ingredient.acorns", "Acorns", 600, 600, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition MesquiteFlour = new ItemDefinition("ingredient.mesquite_flour", "Mesquite flour", 800, 800, category: ItemCategory.RefinedIngredient);
        public static readonly ItemDefinition RoastedAcornMeal = new ItemDefinition("ingredient.roasted_acorn_meal", "Roasted acorn meal", 800, 800, category: ItemCategory.RefinedIngredient);
        public static readonly ItemDefinition ToastedPineNuts = new ItemDefinition("item.toasted_pine_nuts", "Toasted pine nuts", 800, 800, foodHearts: 2, preparedFood: true);
        public static readonly ItemDefinition BirchSapMilkDrink = new ItemDefinition("item.birch_sap_milk_drink", "Birch sap milk drink", 700, 900, foodHearts: 3, preparedFood: true);
        public static readonly ItemDefinition MesquitePodCakes = new ItemDefinition("item.mesquite_pod_cakes", "Mesquite pod cakes", 800, 800, foodHearts: 4, preparedFood: true);
        public static readonly ItemDefinition AcornBiscuits = new ItemDefinition("item.acorn_biscuits", "Acorn biscuits", 800, 800, foodHearts: 5, preparedFood: true);
        // Region loose resources used by the Milestone 5 test world (section 6.2.2). Footprints of the six meshed region-1/2
        // resources follow their mesh proportions (front view), keeping the earlier largest dimension, minimum 300 units.
        public static readonly ItemDefinition RabbitPelt = new ItemDefinition("animal.rabbit.pelt", "Rabbit pelt", 850, 300);
        public static readonly ItemDefinition Apples = new ItemDefinition("food.apples", "Apples", 420, 480, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition Flint = new ItemDefinition("material.flint", "Flint", 480, 306);
        public static readonly ItemDefinition PineCone = new ItemDefinition("material.pine_cone", "Pine cone", 413, 480);
        // Mistwood loose resources. Front footprints follow the authored Region 3 mesh proportions.
        public static readonly ItemDefinition NettleFiber = new ItemDefinition("material.nettle_fiber", "Nettle fiber", 850, 300);
        public static readonly ItemDefinition Clay = new ItemDefinition("material.clay", "Clay", 480, 279);
        public static readonly ItemDefinition RockSalt = new ItemDefinition("ingredient.rock_salt", "Rock salt", 480, 332, category: ItemCategory.RawIngredient);
        // Fern Hollow loose resources; footprints follow their authored front-view mesh bounds.
        public static readonly ItemDefinition Limestone = new ItemDefinition("material.limestone", "Limestone", 480, 280);
        public static readonly ItemDefinition Strawberries = new ItemDefinition("food.strawberries", "Strawberries", 443, 480, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition TeaHerbs = new ItemDefinition("ingredient.tea_herbs", "Tea herbs", 850, 593, category: ItemCategory.RawIngredient);
        // Tier-one interaction outputs, not additional featured loose resources.
        public static readonly ItemDefinition RawRabbitMeat = new ItemDefinition("animal.rabbit.meat", "Raw rabbit meat", 900, 558, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition Bone = new ItemDefinition("material.bone", "Bone", 700, 300);
        public static readonly ItemDefinition AnimalFat = new ItemDefinition("ingredient.animal_fat", "Animal fat", 650, 377, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition RainbowTrout = new ItemDefinition("fish.rainbow_trout", "Rainbow trout", 1100, 427, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition LargemouthBass = new ItemDefinition("fish.largemouth_bass", "Largemouth bass", 1100, 581, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition Bluegill = new ItemDefinition("fish.bluegill", "Bluegill", 900, 657, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition YellowPerch = new ItemDefinition("fish.yellow_perch", "Yellow perch", 1100, 427, category: ItemCategory.RawIngredient);
        // Cloudstep Mountains loose resources. Red berries reuse the existing Berries ID above.
        public static readonly ItemDefinition Oats = new ItemDefinition("ingredient.oats", "Oats", 652, 850, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition Pebbles = new ItemDefinition("material.pebbles", "Pebbles", 480, 240);
        // Reedwater Delta loose resources; Wheat is the raw grain, not crafted Wheat flour.
        public static readonly ItemDefinition Reeds = new ItemDefinition("material.reeds", "Reeds", 672, 850);
        public static readonly ItemDefinition Wheat = new ItemDefinition("ingredient.wheat", "Wheat", 422, 850, category: ItemCategory.RawIngredient);
        public static readonly ItemDefinition Sand = new ItemDefinition("material.sand", "Sand", 480, 240);
        public static readonly ItemDefinition[] TreeIngredients = { PineNuts, BirchSap, MesquitePods, Acorns };
        public static readonly ItemDefinition[] TreeRefinements = { MesquiteFlour, RoastedAcornMeal };
        public static readonly ItemDefinition[] TreeDishes = { ToastedPineNuts, BirchSapMilkDrink, MesquitePodCakes, AcornBiscuits };
        // Milestone 6 debug tools for tier comparisons (removed with the debug cleanup): tiers 2-4, Normal and Elite, per
        // family. They borrow the pine models; family/tier/variant come from Domain.Harvest.HarvestTools.
        public static readonly ItemDefinition[] DebugHarvestTools = BuildDebugTools();
        private static ItemDefinition[] BuildDebugTools()
        {
            var tools = new List<ItemDefinition>();
            for (int tier = 2; tier <= 4; tier++)
                foreach (bool elite in new[] { false, true })
                {
                    string variant = elite ? "elite" : "normal", label = elite ? "Elite" : "Normal";
                    tools.Add(new ItemDefinition($"debug.tool.axe.t{tier}.{variant}", $"Test axe T{tier} {label}", 1000, 2000, false));
                    tools.Add(new ItemDefinition($"debug.tool.hunting.t{tier}.{variant}", $"Test {(tier >= 3 ? "bow" : "slingshot")} T{tier} {label}", 1100, 1600, false));
                    tools.Add(new ItemDefinition($"debug.tool.rod.t{tier}.{variant}", $"Test rod T{tier} {label}", 600, 2400, false));
                }
            return tools.ToArray();
        }
        // Fixture spawns rely on the first six entries keeping their order; append new definitions at the end.
        public static readonly ItemDefinition[] All = Tier1CraftedItems.AppendTo(new ItemDefinition[] { Branch, Log, Stone, Hemp, Berries, Tool, TestHat, TestBoots, TestShirt, TestPants, TestBackpack, TestBread,
            PineNuts, BirchSap, MesquitePods, Acorns, MesquiteFlour, RoastedAcornMeal, ToastedPineNuts, BirchSapMilkDrink, MesquitePodCakes, AcornBiscuits,
            RabbitPelt, Apples, Flint, PineCone, NettleFiber, Clay, RockSalt, Limestone, Strawberries, TeaHerbs,
            RawRabbitMeat, Bone, AnimalFat, RainbowTrout, LargemouthBass, Bluegill, YellowPerch, Oats, Pebbles, Reeds, Wheat, Sand }).Concat(DebugHarvestTools).ToArray();
    }
}
