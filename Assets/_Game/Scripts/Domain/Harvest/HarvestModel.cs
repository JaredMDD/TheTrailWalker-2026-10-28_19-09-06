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

    // Tier-one harvest outputs (section 7.1 / 7.1.1 and the authored art catalog). Higher debug target tiers reuse these
    // outputs because only tier-one resources exist in the test world.
    public static class HarvestManifest
    {
        private readonly struct Output { public readonly string ItemId; public readonly int NormalMin, NormalMax, EliteMin, EliteMax;
            public Output(string id, int nMin, int nMax, int eMin, int eMax) { ItemId = id; NormalMin = nMin; NormalMax = nMax; EliteMin = eMin; EliteMax = eMax; } }

        private static readonly Output[] Tree = {
            new Output("material.pine_branch", 2, 5, 4, 10), new Output("material.pine_log", 2, 5, 4, 10), new Output("ingredient.pine_nuts", 2, 4, 4, 6) };
        private static readonly Output[] Rabbit = {
            new Output("animal.rabbit.pelt", 2, 5, 4, 10), new Output("animal.rabbit.meat", 2, 5, 4, 10), new Output("material.bone", 2, 5, 4, 10), new Output("ingredient.animal_fat", 1, 3, 1, 6) };
        // Regional fish pairs (section 6.2.5); the TOTAL catch is 2-5 bundles (Elite 4-10) drawn from the pair.
        private static readonly string[,] FishPairs = {
            { "fish.rainbow_trout", "fish.bluegill" }, { "fish.largemouth_bass", "fish.yellow_perch" }, { "fish.rainbow_trout", "fish.largemouth_bass" },
            { "fish.bluegill", "fish.yellow_perch" }, { "fish.rainbow_trout", "fish.yellow_perch" }, { "fish.largemouth_bass", "fish.bluegill" } };

        public static IReadOnlyList<HarvestBundle> Roll(InteractableKind kind, int regionNumber, string segmentId, int cycle, string anchorId, bool boosted)
        {
            if (string.IsNullOrEmpty(segmentId) || string.IsNullOrEmpty(anchorId) || cycle < 1 || regionNumber < 1) throw new ArgumentException("Invalid harvest roll");
            var random = new Random(Seed(segmentId, cycle, anchorId));
            var bundles = new List<HarvestBundle>();
            if (kind == InteractableKind.FishingPoint)
            {
                int pair = (regionNumber - 1) % FishPairs.GetLength(0);
                int total = boosted ? random.Next(4, 11) : random.Next(2, 6);
                for (int i = 0; i < total; i++) bundles.Add(new HarvestBundle(FishPairs[pair, random.Next(2)], 1 + random.Next(5)));
                return bundles;
            }
            foreach (Output output in kind == InteractableKind.Tree ? Tree : Rabbit)
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
