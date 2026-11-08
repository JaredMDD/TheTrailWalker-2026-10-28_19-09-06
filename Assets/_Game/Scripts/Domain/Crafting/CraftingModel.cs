using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Crafting
{
    // Primary output role (section 9.2): decides mastery, timer direction and price category.
    public enum RecipeRole { Material, Food, Tool, Wearable, Misc }

    public readonly struct RecipeInput
    {
        public readonly string ItemId; public readonly int Quantity;
        public RecipeInput(string itemId, int quantity) { ItemId = itemId; Quantity = quantity; }
    }

    // One exact recipe (section 9.0): exact input IDs and quantities, one output ID and yield, tier and level-one base time.
    public sealed class RecipeDefinition
    {
        public readonly string Id, Name, OutputId, UnlockSource;
        public readonly RecipeRole Role;
        public readonly int Tier, Yield;
        public readonly float BaseSeconds;
        public readonly RecipeInput[] Inputs;
        // Retired (November 7, 2026, e.g. the plain tier-one fish roasts): kept registered so saves that know the recipe keep
        // its knowledge, mastery and pending batches, but never taught again (no mission reward, skipped by debug learn-all).
        public readonly bool Retired;
        public RecipeDefinition(string id, string name, RecipeRole role, int tier, string outputId, int yield, float baseSeconds, string unlockSource, RecipeInput[] inputs, bool retired = false)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(outputId) || tier < 1 || tier > 4 || yield < 1 || baseSeconds < 0 || inputs == null || inputs.Length == 0 || inputs.Length > 9)
                throw new ArgumentException("Invalid recipe " + id);
            Id = id; Name = name; Role = role; Tier = tier; OutputId = outputId; Yield = yield; BaseSeconds = baseSeconds; UnlockSource = unlockSource ?? ""; Inputs = inputs; Retired = retired;
        }
        public bool Levelled => Role != RecipeRole.Material;
        public ItemDefinition Output => Items.Find(OutputId);
        // Tools, wearables and backpacks are unique x1 crafts; stackable outputs allow x10/x100/x1000.
        public bool BulkAllowed => Output != null && Output.Stackable;
    }

    public static class Items
    {
        private static Dictionary<string, ItemDefinition> byId;
        public static ItemDefinition Find(string id)
        {
            if (byId == null) { byId = new Dictionary<string, ItemDefinition>(); foreach (ItemDefinition d in PrototypeItems.All) byId[d.Id] = d; }
            return id != null && byId.TryGetValue(id, out ItemDefinition definition) ? definition : null;
        }
    }

    public static class RecipeBook
    {
        // Tier-two Elite upgrades (section 8.4) using the debug tier-two tools until birch tools exist: one Normal tool
        // instance (any condition with uses left) + two tier-one advanced materials -> one new Elite instance with its own
        // recipe snapshot. Tier three and four upgrades need tier two/three materials that are not authored yet.
        public static readonly RecipeDefinition[] EliteUpgrades = {
            new RecipeDefinition("recipe.tool.elite_axe_t2", "Elite axe T2 (test upgrade)", RecipeRole.Tool, 2, "debug.tool.axe.t2.elite", 1, 20, "Debug upgrade until birch tools exist",
                new[] { new RecipeInput("debug.tool.axe.t2.normal", 1), new RecipeInput("item.pine_plank", 2) }),
            new RecipeDefinition("recipe.tool.elite_slingshot_t2", "Elite slingshot T2 (test upgrade)", RecipeRole.Tool, 2, "debug.tool.hunting.t2.elite", 1, 20, "Debug upgrade until birch tools exist",
                new[] { new RecipeInput("debug.tool.hunting.t2.normal", 1), new RecipeInput("item.fine_rabbit_pelt", 2) }),
            new RecipeDefinition("recipe.tool.elite_rod_t2", "Elite rod T2 (test upgrade)", RecipeRole.Tool, 2, "debug.tool.rod.t2.elite", 1, 20, "Debug upgrade until birch tools exist",
                new[] { new RecipeInput("debug.tool.rod.t2.normal", 1), new RecipeInput("item.fine_hemp_cloth", 2) }) };
        private static RecipeDefinition[] all;
        public static IReadOnlyList<RecipeDefinition> All => all ?? (all = Combine());
        private static RecipeDefinition[] Combine() { var list = new List<RecipeDefinition>(Tier1Recipes.All); list.AddRange(EliteUpgrades); return list.ToArray(); }
        public static RecipeDefinition Find(string id) { foreach (RecipeDefinition r in All) if (r.Id == id) return r; return null; }
        public static readonly int[] BatchSizes = { 1, 10, 100, 1000 };
    }

    // Recipe mastery 1-10 from cumulative finished output units (section 10.1, proposed thresholds).
    public static class CraftingMastery
    {
        public static readonly int[] Thresholds = { 0, 3, 12, 18, 20, 32, 48, 66, 88, 115 };
        public static int LevelFor(long units)
        {
            int level = 1;
            for (int i = 1; i < Thresholds.Length; i++) if (units >= Thresholds[i]) level = i + 1;
            return level;
        }
        public static long UnitsToNext(long units) { int level = LevelFor(units); return level >= 10 ? 0 : Thresholds[level] - units; }
    }

    // Material tiers and the cooking pantry (section 9.0.1). Every active test item is tier one unless listed.
    public static class ItemTiers
    {
        private static readonly Dictionary<string, int> Tiers = new Dictionary<string, int> {
            { "ingredient.birch_sap", 2 }, { "ingredient.mesquite_pods", 3 }, { "ingredient.mesquite_flour", 3 }, { "ingredient.acorns", 4 }, { "ingredient.roasted_acorn_meal", 4 } };
        public static int Of(string itemId)
        {
            if (itemId != null && itemId.StartsWith("debug.tool.") && itemId.Length > 0)
            {
                int marker = itemId.IndexOf(".t", "debug.tool.".Length, StringComparison.Ordinal);
                if (marker > 0 && marker + 2 < itemId.Length && char.IsDigit(itemId[marker + 2])) return itemId[marker + 2] - '0';
            }
            return itemId != null && Tiers.TryGetValue(itemId, out int tier) ? tier : 1;
        }
        private static readonly HashSet<string> Pantry = new HashSet<string> {
            "ingredient.wheat_flour", "ingredient.barley_flour", "ingredient.corn_flour", "ingredient.oats", "ingredient.rice", "ingredient.cow_cheese", "ingredient.goat_cheese",
            "ingredient.mixed_milk_cheese", "food.milk.cow", "food.milk.goat", "ingredient.cow_cream", "ingredient.cow_butter", "ingredient.cow_yogurt", "ingredient.egg",
            "ingredient.rock_salt", "ingredient.water", "ingredient.sugar", "ingredient.honey", "ingredient.tea_herbs", "ingredient.flax_oil", "ingredient.sunflower_oil",
            "item.clay_cup", "item.glass_bottle" };
        public static bool IsPantry(string itemId) => Pantry.Contains(itemId);
        // Direct ingredient tier window: tier one -> {1}; later tiers -> {R - 1, R}; Food may also use older pantry items.
        public static bool InWindow(RecipeDefinition recipe, string itemId)
        {
            int tier = Of(itemId), low = recipe.Tier == 1 ? 1 : recipe.Tier - 1;
            return (tier >= low && tier <= recipe.Tier) || (recipe.Role == RecipeRole.Food && IsPantry(itemId) && tier <= recipe.Tier);
        }
    }

    // Price references and material bases live in Domain.Economy.ItemPrices (Milestone 9).
    public static class CraftPrices
    {
        public static double[] QualityMultiplier => Economy.ItemPrices.QualityMultiplier;
        public static long ReferenceAtLevel10(RecipeDefinition recipe) =>
            Economy.ItemPrices.ReferenceAtLevel10(recipe.Role, recipe.Tier, recipe.Output?.GearSlot == Equipment.GearSlot.Backpack);
    }

    // The deterministic output profile of one batch (section 6.1.1), snapshotted at craft start.
    public readonly struct CraftResult
    {
        public readonly int Level, Quality, Executions, OutputUnits, MaxDurability;
        public readonly long UnitValue;   // minor units (1/100 cash), Milestone 9
        public readonly double Seconds;
        public CraftResult(int level, int quality, int executions, int outputUnits, long unitValue, int maxDurability, double seconds)
        { Level = level; Quality = quality; Executions = executions; OutputUnits = outputUnits; UnitValue = unitValue; MaxDurability = maxDurability; Seconds = seconds; }
    }

    public static class CraftStats
    {
        public static readonly double[] QualityDurabilityBonus = { 0.0, 0.2, 0.4, 0.6, 0.8 };
        public static double PriceLevelFactor(int level) => (level + 35) / 45.0;
        public static double DurabilityLevelMultiplier(int level) => 1 + 0.08 * (level - 1);

        // Base maximum durability: tool uses (section 8.2) or wearable walking distance in metres. Proposed wearable
        // calibration: 1.6 / 2.4 / 3.2 / 4.5 km so tier three L10 Q5 is about 10 km and tier four about 14 km.
        public static int BaseDurability(RecipeDefinition recipe)
        {
            ItemDefinition output = recipe.Output;
            if (output == null) return 0;
            if (output.GearSlot.HasValue) return new[] { 1600, 2400, 3200, 4500 }[recipe.Tier - 1];
            if (HarvestTools.TryGet(output, out ToolSpec spec)) return spec.BaseUses;
            return 0;
        }

        public static double Seconds(RecipeDefinition recipe, int level, int executions)
        {
            double single = recipe.BaseSeconds;
            if (recipe.Role == RecipeRole.Tool || recipe.Role == RecipeRole.Food) single = recipe.BaseSeconds / (1 + 0.08 * (level - 1));
            else if (recipe.Role == RecipeRole.Wearable || recipe.Role == RecipeRole.Misc) single = recipe.BaseSeconds * (1 + 0.10 * (level - 1));
            if (single <= 0) return 0; // gatherable-output materials are instant
            return single * (1 + 0.2 * Math.Log(Math.Max(1, executions), 2));
        }

        // furnitureQuality C (0-5) and furnitureDurability U are zero until furniture exists (Milestone 10+).
        public static CraftResult Compute(RecipeDefinition recipe, long masteryUnits, int executions, int furnitureQuality = 0, double furnitureDurability = 0)
        {
            int level = recipe.Levelled ? CraftingMastery.LevelFor(masteryUnits) : 1;
            int quality = Math.Max(1, Math.Min(5, (recipe.Levelled ? Math.Min(level, 5) : 1) + furnitureQuality));
            long value;
            int durability = 0;
            if (!recipe.Levelled) value = Economy.ItemPrices.IntrinsicMinor(recipe.Output, quality);
            else
            {
                double price = CraftPrices.ReferenceAtLevel10(recipe) * PriceLevelFactor(level) * CraftPrices.QualityMultiplier[quality - 1];
                int baseDurability = BaseDurability(recipe);
                if (baseDurability > 0)
                {
                    double dBase = baseDurability * DurabilityLevelMultiplier(level), extra = QualityDurabilityBonus[quality - 1] + furnitureDurability;
                    durability = (int)Math.Round(dBase * (1 + extra));
                    price *= 1 + extra;
                }
                value = Economy.Money.Minor(price);
            }
            return new CraftResult(level, quality, executions, recipe.Yield * executions, Math.Max(1, value), durability, Seconds(recipe, level, executions));
        }

        // How many executions the carried ingredients allow (whole executions only).
        public static int MaxExecutions(RecipeDefinition recipe, Func<string, long> carried)
        {
            long most = long.MaxValue;
            foreach (RecipeInput input in recipe.Inputs) most = Math.Min(most, carried(input.ItemId) / input.Quantity);
            return (int)Math.Min(most, recipe.BulkAllowed ? 1000 : 1);
        }

        public static bool BatchAllowed(RecipeDefinition recipe, int executions) =>
            executions >= 1 && (recipe.BulkAllowed ? Array.IndexOf(RecipeBook.BatchSizes, executions) >= 0 : executions == 1);
    }

    // One outstanding craft (section 10): outputs are pending until ReadyAtUtc; mastery is credited once when ready.
    public sealed class CraftBatch
    {
        public string Id, RecipeId; public int Executions, OutputUnits, Level, Quality; public long UnitValue; public int MaxDurability;
        public DateTime StartedUtc, ReadyAtUtc; public bool Credited; public List<string> ItemIds = new List<string>();
        // Mastery is counted when the craft starts (user, November 2, 2026), so the next craft already uses the new level.
        // Batches saved before that change carry false and are counted once when they finish.
        public bool MasteryCredited;
        public bool IsReady(DateTime now) => now >= ReadyAtUtc;
    }

    // Known recipes, mastery units and outstanding batches (persisted in the save).
    public sealed class CraftingState
    {
        private readonly HashSet<string> known = new HashSet<string>();
        private readonly List<string> knownOrder = new List<string>(); // learning order: the first unlocked recipe opens first
        public string LastCrafted { get; set; }
        public string FirstLearned => knownOrder.Count > 0 ? knownOrder[0] : null;
        public IReadOnlyList<string> KnownInOrder => knownOrder;
        private readonly Dictionary<string, long> units = new Dictionary<string, long>();
        private readonly List<CraftBatch> batches = new List<CraftBatch>();
        public IReadOnlyCollection<string> Known => known;
        public IReadOnlyDictionary<string, long> Units => units;
        public IReadOnlyList<CraftBatch> Batches => batches;

        public bool Learn(string recipeId) { if (RecipeBook.Find(recipeId) == null || !known.Add(recipeId)) return false; knownOrder.Add(recipeId); return true; }
        public bool Knows(string recipeId) => known.Contains(recipeId);
        public long UnitsOf(string recipeId) => units.TryGetValue(recipeId, out long u) ? u : 0;
        public int LevelOf(string recipeId) => CraftingMastery.LevelFor(UnitsOf(recipeId));
        // No craft-slot limit (user, October 31, 2026): any number of timed batches can run at once, so late-game x1 tools
        // and gear never wait for a free slot.

        public CraftBatch Add(RecipeDefinition recipe, CraftResult result, DateTime now, string id)
        {
            var batch = new CraftBatch { Id = id, RecipeId = recipe.Id, Executions = result.Executions, OutputUnits = result.OutputUnits, Level = result.Level, Quality = result.Quality,
                UnitValue = result.UnitValue, MaxDurability = result.MaxDurability, StartedUtc = now, ReadyAtUtc = now.AddSeconds(result.Seconds) };
            batches.Add(batch);
            return batch;
        }

        // Mastery units for a levelled recipe, counted at the moment of crafting.
        public void CreditMastery(RecipeDefinition recipe, int outputUnits)
        {
            if (recipe != null && recipe.Levelled && outputUnits > 0) units[recipe.Id] = UnitsOf(recipe.Id) + outputUnits;
        }

        // Batches that became ready since the last call. Mastery was counted at craft start; only batches from older saves
        // (MasteryCredited false) are counted here, once.
        public List<CraftBatch> CompleteReady(DateTime now)
        {
            var ready = new List<CraftBatch>();
            foreach (CraftBatch batch in batches)
            {
                if (batch.Credited || !batch.IsReady(now)) continue;
                batch.Credited = true;
                RecipeDefinition recipe = RecipeBook.Find(batch.RecipeId);
                if (!batch.MasteryCredited) { CreditMastery(recipe, batch.OutputUnits); batch.MasteryCredited = true; }
                ready.Add(batch);
            }
            return ready;
        }

        // A ready batch leaves the list once all its outputs are ready in the bag (or the batch had no pending outputs).
        public void Remove(CraftBatch batch) => batches.Remove(batch);

        public void Restore(IEnumerable<string> knownIds, IEnumerable<KeyValuePair<string, long>> unitPairs, IEnumerable<CraftBatch> outstanding)
        {
            known.Clear(); knownOrder.Clear(); units.Clear(); batches.Clear();
            foreach (string id in knownIds) Learn(id);
            foreach (var pair in unitPairs) if (RecipeBook.Find(pair.Key) != null && pair.Value >= 0) units[pair.Key] = pair.Value;
            foreach (CraftBatch batch in outstanding) if (RecipeBook.Find(batch.RecipeId) != null) batches.Add(batch);
        }
    }
}
