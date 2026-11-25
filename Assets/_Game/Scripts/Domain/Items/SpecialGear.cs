using System;
using TheTrailWalker.Domain.Equipment;

namespace TheTrailWalker.Domain.Items
{
    // The balloon merchant's special gear (section 14.1): always Q5, never wears out, noncraftable. Stats come from the
    // per-item table, not the rank tables; ids are special.t{1-4}.{hat|boots|shirt|pants|pack}.
    public static class SpecialGear
    {
        private static readonly string[] Parts = { "hat", "boots", "shirt", "pants", "pack" };
        private static readonly GearSlot[] Slots = { GearSlot.Hat, GearSlot.Boots, GearSlot.Shirt, GearSlot.Pants, GearSlot.Backpack };
        private static readonly string[][] Names = {
            new[] { "Wayfarer's felt hat", "Wayfarer's trail boots", "Wayfarer's waxed shirt", "Wayfarer's field trousers", "Wayfarer's ridge pack" },
            new[] { "Heron-plume rain hat", "Bog-strider boots", "Lakeshore oilskin shirt", "Boardwalk trousers", "Trapper's fur-trimmed pack" },
            new[] { "Dune wrap", "Mesa sand boots", "Nomad linen robe", "Caravan wrap trousers", "Caravan saddle pack" },
            new[] { "Coral-shell sun hat", "Tidewalker boots", "Mariner's sailcloth shirt", "Reef-runner trousers", "Estuary sea-chest pack" } };
        // Hearts, m/s, pockets, hooks and backpack width (logical units) per tier.
        private static readonly double[][] Stats = {
            new[] { 4, 2.25, 2, 2, 8 }, new[] { 5, 2.6, 3, 3, 9 }, new[] { 7, 3.35, 5, 5, 11 }, new[] { 10, 4.1, 8, 8, 13 } };
        private static readonly long[][] Gold = {
            new long[] { 15, 20, 15, 15, 25 }, new long[] { 50, 65, 50, 50, 80 },
            new long[] { 110, 140, 110, 110, 180 }, new long[] { 200, 260, 200, 200, 320 } };
        private static readonly long[] ClothingCash = { 440, 1320, 2640, 4400 }, PackCash = { 700, 2100, 4200, 7000 };

        public static string Id(int tier, int part) => "special.t" + tier + "." + Parts[part];

        private static bool TryParse(string itemId, out int tier, out int part)
        {
            tier = part = -1;
            if (itemId == null || itemId.Length < 12 || !itemId.StartsWith("special.t", StringComparison.Ordinal) || itemId[10] != '.') return false;
            tier = itemId[9] - '0'; part = Array.IndexOf(Parts, itemId.Substring(11));
            return tier >= 1 && tier <= 4 && part >= 0;
        }

        public static bool Is(string itemId) => TryParse(itemId, out _, out _);
        public static double Stat(string itemId) => TryParse(itemId, out int t, out int p) ? Stats[t - 1][p] : throw new ArgumentException("Not special gear: " + itemId);
        // Saved gear stores only the stat: valid when it is the table value for that slot and tier.
        public static bool Matches(GearSlot slot, int tier, double stat) => tier >= 1 && tier <= 4 && Math.Abs(Stats[tier - 1][Array.IndexOf(Slots, slot)] - stat) < 1e-9;
        public static long GoldPrice(string itemId) => TryParse(itemId, out int t, out int p) ? Gold[t - 1][p] : throw new ArgumentException("Not special gear: " + itemId);
        public static long ResaleCash(string itemId) => TryParse(itemId, out int t, out int p) ? (p == 4 ? PackCash : ClothingCash)[t - 1] : throw new ArgumentException("Not special gear: " + itemId);

        // Footprints copy the crafted gear of the same slot (hat, boots, shirt, pants, starter backpack).
        private static readonly int[][] Sizes = { new[] { 900, 723 }, new[] { 900, 660 }, new[] { 900, 780 }, new[] { 698, 900 }, new[] { 939, 1350 } };

        public static readonly ItemDefinition[] All = Build();
        private static ItemDefinition[] Build()
        {
            var all = new ItemDefinition[20];
            for (int t = 1; t <= 4; t++)
                for (int p = 0; p < 5; p++)
                    all[(t - 1) * 5 + p] = new ItemDefinition(Id(t, p), Names[t - 1][p], Sizes[p][0], Sizes[p][1], false, Slots[p], t);
            return all;
        }
    }
}
