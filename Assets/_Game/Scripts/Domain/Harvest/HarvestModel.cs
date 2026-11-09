using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;

namespace TheTrailWalker.Domain.Harvest
{
    public enum ToolFamily { Axe, Hunting, Rod }

    // A harvesting tool's identity: family, tier and Normal/Elite variant (plan section 8.1).
    public readonly struct ToolSpec
    {
        public readonly ToolFamily Family; public readonly int Tier; public readonly bool Elite;
        public ToolSpec(ToolFamily family, int tier, bool elite) { Family = family; Tier = tier; Elite = elite; }
        // Confirmed level-one baselines (section 8.2): Normal axe/rod 10 + 10 (T - 1), slingshot 6 + 10 (T - 1); Elite doubles.
        public int BaseUses
        {
            get { int normal = (Family == ToolFamily.Hunting ? 6 : 10) + 10 * (Tier - 1); return Elite ? normal * 2 : normal; }
        }
    }

    public static class HarvestTools
    {
        public static bool TryGet(ItemDefinition definition, out ToolSpec spec)
        {
            spec = default;
            if (definition == null) return false;
            switch (definition.Id)
            {
                case "tool.pine_axe.normal": spec = new ToolSpec(ToolFamily.Axe, 1, false); return true;
                case "item.pine_slingshot_normal": spec = new ToolSpec(ToolFamily.Hunting, 1, false); return true;
                case "item.pine_rod_normal": spec = new ToolSpec(ToolFamily.Rod, 1, false); return true;
            }
            // debug.tool.{axe|hunting|rod}.t{2-4}.{normal|elite}
            string[] parts = definition.Id.Split('.');
            if (parts.Length != 5 || parts[0] != "debug" || parts[1] != "tool" || parts[3].Length != 2 || parts[3][0] != 't') return false;
            if (!int.TryParse(parts[3].Substring(1), out int tier) || tier < 2 || tier > 4) return false;
            ToolFamily family;
            if (parts[2] == "axe") family = ToolFamily.Axe; else if (parts[2] == "hunting") family = ToolFamily.Hunting; else if (parts[2] == "rod") family = ToolFamily.Rod; else return false;
            if (parts[4] != "normal" && parts[4] != "elite") return false;
            spec = new ToolSpec(family, tier, parts[4] == "elite");
            return true;
        }

        // The pine tool whose model a debug tool borrows.
        public static string ModelId(ToolFamily family) => family == ToolFamily.Axe ? "tool.pine_axe.normal" : family == ToolFamily.Hunting ? "item.pine_slingshot_normal" : "item.pine_rod_normal";

        public static ItemDefinition Definition(ToolFamily family, int tier, bool elite)
        {
            if (tier == 1 && !elite) return Array.Find(PrototypeItems.All, d => d.Id == ModelId(family));
            string name = family == ToolFamily.Axe ? "axe" : family == ToolFamily.Hunting ? "hunting" : "rod";
            return Array.Find(PrototypeItems.All, d => d.Id == $"debug.tool.{name}.t{tier}.{(elite ? "elite" : "normal")}");
        }
    }

    public static class HarvestRules
    {
        public static ToolFamily FamilyFor(InteractableKind kind) => kind == InteractableKind.Tree ? ToolFamily.Axe : kind == InteractableKind.AnimalSpawn ? ToolFamily.Hunting : ToolFamily.Rod;

        // Action counts (section 8.2, proposed test values). Null when the target is three or more tiers above the tool.
        public static int? Actions(ToolSpec tool, int targetTier)
        {
            int difference = targetTier - tool.Tier;
            if (difference >= 3) return null;
            bool axe = tool.Family == ToolFamily.Axe;
            if (difference == 2) return axe ? 24 : 12;
            if (difference == 1) return axe ? 12 : 6;
            if (difference == 0) return axe ? (tool.Elite ? 3 : 6) : (tool.Elite ? 1 : 3);
            return axe ? (tool.Elite ? 2 : 3) : 1; // lower-tier target: one-action floor
        }

        // Elite selects the boosted drop table on every eligible harvest, including +1 and +2 targets.
        public static bool Boosted(ToolSpec tool, int targetTier) => tool.Elite && Actions(tool, targetTier).HasValue;

        // Tool choice (user, October 31, 2026): the eligible tool with the FEWEST remaining uses is used first, so worn
        // tools are used up before fresh ones. Ties: fewest actions, then Elite (drop boost), then the lower tier.
        public static int Compare(ToolSpec a, int usesA, ToolSpec b, int usesB, int targetTier)
        {
            if (usesA != usesB) return usesA.CompareTo(usesB);
            int byActions = Actions(a, targetTier).Value.CompareTo(Actions(b, targetTier).Value);
            if (byActions != 0) return byActions;
            if (a.Elite != b.Elite) return a.Elite ? -1 : 1;
            return a.Tier.CompareTo(b.Tier);
        }
    }

    // One rolled reward bundle (1,000 units in the mechanics-test preset).
    public readonly struct HarvestBundle
    {
        public readonly string ItemId; public readonly int Quality;
        public HarvestBundle(string itemId, int quality) { ItemId = itemId; Quality = quality; }
    }

    // Harvest outputs per tier (section 7.1 / 7.1.1 and the authored art catalog): tier one rolls PrototypeItems; tiers two to four
    // roll their own wood, hunt and fish tables from HarvestTierItems (resolved through Items.Find).
    public static class HarvestManifest
    {
        private readonly struct Output { public readonly string ItemId; public readonly int NormalMin, NormalMax, EliteMin, EliteMax;
            public Output(string id, int nMin, int nMax, int eMin, int eMax) { ItemId = id; NormalMin = nMin; NormalMax = nMax; EliteMin = eMin; EliteMax = eMax; } }

        private static Output[] TreeOf(string wood, string ingredient) => new[] {
            new Output($"material.{wood}_branch", 2, 5, 4, 10), new Output($"material.{wood}_log", 2, 5, 4, 10), new Output(ingredient, 2, 4, 4, 6) };
        private static Output[] HuntOf(string primary, string meat, params Output[] extra)
        {
            var list = new List<Output> { new Output(primary, 2, 5, 4, 10), new Output(meat, 2, 5, 4, 10), new Output("material.bone", 2, 5, 4, 10), new Output("ingredient.animal_fat", 1, 3, 1, 6) };
            list.AddRange(extra);
            return list.ToArray();
        }

        // Per-tier tables indexed tier - 1 (section 7.1.1). Tier one keeps its original order so its rolls never change.
        private static readonly Output[][] Tree = {
            new[] { new Output("material.pine_branch", 2, 5, 4, 10), new Output("material.pine_log", 2, 5, 4, 10), new Output("ingredient.pine_nuts", 2, 4, 4, 6) },
            TreeOf("birch", "ingredient.birch_sap"), TreeOf("mesquite", "ingredient.mesquite_pods"), TreeOf("oak", "ingredient.acorns") };
        private static readonly Output[][] Hunt = {
            new[] { new Output("animal.rabbit.pelt", 2, 5, 4, 10), new Output("animal.rabbit.meat", 2, 5, 4, 10), new Output("material.bone", 2, 5, 4, 10), new Output("ingredient.animal_fat", 1, 3, 1, 6) },
            HuntOf("animal.beaver.pelt", "animal.beaver.meat"), HuntOf("animal.turkey.feathers", "animal.turkey.meat"),
            HuntOf("animal.boar.hide", "animal.boar.meat", new Output("animal.boar.tusk", 1, 3, 1, 6)) };
        // Regional fish pairs (section 6.2.5) per tier, indexed [tier - 1, (region - 1) % 6]; the TOTAL catch is 2-5 bundles (Elite 4-10) drawn from the pair.
        private static readonly (string a, string b)[,] FishPairs = {
            { ("fish.rainbow_trout", "fish.bluegill"), ("fish.largemouth_bass", "fish.yellow_perch"), ("fish.rainbow_trout", "fish.largemouth_bass"),
              ("fish.bluegill", "fish.yellow_perch"), ("fish.rainbow_trout", "fish.yellow_perch"), ("fish.largemouth_bass", "fish.bluegill") },
            { ("fish.common_carp", "fish.channel_catfish"), ("fish.northern_pike", "fish.walleye"), ("fish.common_carp", "fish.northern_pike"),
              ("fish.channel_catfish", "fish.walleye"), ("fish.common_carp", "fish.walleye"), ("fish.northern_pike", "fish.channel_catfish") },
            { ("fish.salmon", "fish.brown_trout"), ("fish.sturgeon", "fish.lake_trout"), ("fish.salmon", "fish.sturgeon"),
              ("fish.brown_trout", "fish.lake_trout"), ("fish.salmon", "fish.lake_trout"), ("fish.sturgeon", "fish.brown_trout") },
            { ("fish.longnose_gar", "fish.red_drum"), ("fish.longnose_gar", "fish.red_snapper"), ("fish.red_drum", "fish.red_snapper"),
              ("fish.longnose_gar", "fish.tuna"), ("fish.red_drum", "fish.tuna"), ("fish.red_snapper", "fish.tuna") } };

        private static int Tier(int regionNumber) => Math.Min(FishPairs.GetLength(0), (regionNumber - 1) / 6 + 1);
        public static (string a, string b) FishPair(int regionNumber)
        {
            if (regionNumber < 1) throw new ArgumentException("Invalid region number");
            return FishPairs[Tier(regionNumber) - 1, (regionNumber - 1) % 6];
        }

        public static IReadOnlyList<HarvestBundle> Roll(InteractableKind kind, int regionNumber, string segmentId, int cycle, string anchorId, bool boosted)
        {
            if (string.IsNullOrEmpty(segmentId) || string.IsNullOrEmpty(anchorId) || cycle < 1 || regionNumber < 1) throw new ArgumentException("Invalid harvest roll");
            var random = new Random(Seed(segmentId, cycle, anchorId));
            var bundles = new List<HarvestBundle>();
            if (kind == InteractableKind.FishingPoint)
            {
                (string a, string b) pair = FishPair(regionNumber);
                int total = boosted ? random.Next(4, 11) : random.Next(2, 6);
                for (int i = 0; i < total; i++) bundles.Add(new HarvestBundle(random.Next(2) == 0 ? pair.a : pair.b, 1 + random.Next(5)));
                return bundles;
            }
            foreach (Output output in kind == InteractableKind.Tree ? Tree[Tier(regionNumber) - 1] : Hunt[Tier(regionNumber) - 1])
            {
                int count = boosted ? random.Next(output.EliteMin, output.EliteMax + 1) : random.Next(output.NormalMin, output.NormalMax + 1);
                for (int i = 0; i < count; i++) bundles.Add(new HarvestBundle(output.ItemId, 1 + random.Next(5)));
            }
            return bundles;
        }

        public static string BundleId(string segmentId, int cycle, string anchorId, int index) => $"harvest-{segmentId}-c{cycle}-{anchorId}-b{index}";
        public static string HarvestKey(string segmentId, int cycle, string anchorId) => $"{segmentId}|{cycle}|{anchorId}";

        // FNV-1a over the stable segment, cycle and anchor: the same harvest always rolls the same manifest.
        private static int Seed(string segmentId, int cycle, string anchorId)
        {
            uint hash = 2166136261;
            foreach (char c in segmentId + "|" + cycle + "|" + anchorId) { hash ^= c; hash *= 16777619; }
            return (int)(hash & 0x7fffffff);
        }
    }
}
