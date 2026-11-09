using UnityEngine;

namespace TheTrailWalker
{
    // Every shirt (pocket skin) and backpack (bag and opening skins) beyond tier one, by exact item ID. Colours follow the
    // recipe materials only (Phase 1 consistency, Task 7): Cloth is the main cloth, Binding (the bag's outer rim) the leather or pelt, Patch (the top band) the fittings or cord, Thread the cord or thread. Rows read Cloth, Binding, Patch, Thread. Shade is derived as Cloth * 0.45. Merchant gear is Sampled: the
    // Editor builder reads its colours from the item's display prefab, so the colours here are placeholders for it.
    public static class InventoryStorageGear
    {
        public readonly struct Entry
        {
            public readonly string ItemId;
            public readonly bool Backpack, Sampled;
            public readonly Color Cloth, Binding, Patch, Thread;
            public Entry(string itemId, bool backpack, Color cloth, Color binding, Color patch, Color thread, bool sampled = false)
            { ItemId = itemId; Backpack = backpack; Cloth = cloth; Binding = binding; Patch = patch; Thread = thread; Sampled = sampled; }
            public Color Shade => new Color(Cloth.r * 0.45f, Cloth.g * 0.45f, Cloth.b * 0.45f, 1f);
        }

        private static Color C(float r, float g, float b) => new Color(r, g, b, 1f);
        private static readonly Color HempCloth = C(.55f, .50f, .36f), FlaxCloth = C(.74f, .69f, .55f), FineFlaxCloth = C(.80f, .76f, .62f),
            CottonCloth = C(.85f, .82f, .74f), FineCottonCloth = C(.91f, .89f, .82f), BambooCloth = C(.72f, .70f, .50f),
            HempThread = C(.72f, .63f, .44f), CattailCord = C(.62f, .53f, .35f), AgaveCord = C(.70f, .66f, .48f), CottonThread = C(.88f, .86f, .80f),
            BeaverLeather = C(.45f, .30f, .18f), FineBeaver = C(.38f, .25f, .15f), FineBoar = C(.33f, .26f, .20f), TurkeyFeathers = C(.50f, .38f, .28f),
            CopperFittings = C(.72f, .43f, .25f), IronFittings = C(.42f, .42f, .44f), SteelFittings = C(.62f, .64f, .68f), SamplePlaceholder = C(.5f, .5f, .5f);

        private static Entry Special(string id, bool backpack) => new Entry(id, backpack, SamplePlaceholder, SamplePlaceholder, SamplePlaceholder, HempThread, true);

        public static readonly Entry[] All =
        {
            new Entry("item.flax_work_shirt", false, FlaxCloth, BeaverLeather, HempThread, HempThread),
            new Entry("item.taiga_waxed_shirt", false, FineFlaxCloth, FineBeaver, HempThread, HempThread),
            new Entry("item.marsh_pack", true, HempCloth, BeaverLeather, CopperFittings, HempThread),
            new Entry("item.taiga_frame_pack", true, FineFlaxCloth, FineBeaver, CattailCord, CattailCord),
            new Entry("item.prairie_feather_shirt", false, CottonCloth, BeaverLeather, TurkeyFeathers, CottonThread),
            new Entry("item.highland_shirt", false, FineCottonCloth, FineBeaver, AgaveCord, CottonThread),
            new Entry("item.canyon_pack", true, CottonCloth, BeaverLeather, IronFittings, CottonThread),
            new Entry("item.highland_frame_pack", true, FineCottonCloth, FineBeaver, AgaveCord, AgaveCord),
            new Entry("item.savanna_shirt", false, FineCottonCloth, FineBoar, CottonThread, CottonThread),
            new Entry("item.coastal_shirt", false, BambooCloth, FineBoar, CottonThread, CottonThread),
            new Entry("item.savanna_pack", true, FineCottonCloth, FineBoar, SteelFittings, CottonThread),
            new Entry("item.coastal_pack", true, BambooCloth, FineBoar, SteelFittings, CottonThread),
            Special("special.t1.shirt", false), Special("special.t1.pack", true),
            Special("special.t2.shirt", false), Special("special.t2.pack", true),
            Special("special.t3.shirt", false), Special("special.t3.pack", true),
            Special("special.t4.shirt", false), Special("special.t4.pack", true),
        };

        // "item.marsh_pack" -> "MarshPack", "special.t1.pack" -> "SpecialT1Pack"; skin assets are <Name>Pocket, or <Name>Backpack and <Name>Opening.
        public static string PascalName(string itemId)
        {
            var sb = new System.Text.StringBuilder();
            string trimmed = itemId.StartsWith("item.") ? itemId.Substring(5) : itemId; // "special.t1.pack" keeps its prefix: SpecialT1Pack
            foreach (string part in trimmed.Split('.', '_'))
                if (part.Length > 0) sb.Append(char.ToUpperInvariant(part[0])).Append(part.Substring(1));
            return sb.ToString();
        }
    }
}
