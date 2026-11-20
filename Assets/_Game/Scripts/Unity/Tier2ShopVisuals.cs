using System.Collections.Generic;
using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    // Resources paths for the tier 2 shop goods whose prefabs are not covered by the region or tier 1 lookups.
    public static class Tier2ShopVisuals
    {
        private static readonly Dictionary<string, string> Paths = new Dictionary<string, string>
        {
            { "food.blackberries", "Region07/Blackberries" },
            { "ingredient.carrots", "Region08/Carrots" },
            { "material.copper_ore", "Region08/CopperOre" },
            { "material.flax_fiber", "Region08/FlaxFiber" },
            { "ingredient.barley", "Region09/Barley" },
            { "food.blueberries", "Region09/Blueberries" },
            { "material.quartz", "Region09/Quartz" },
            { "ingredient.flax_seeds", "Region10/FlaxSeeds" },
            { "material.sandstone", "Region10/Sandstone" },
            { "ingredient.honey", "Region11/Honey" },
            { "ingredient.rice", "Region11/Rice" },
            { "ingredient.tomatoes", "Region11/Tomatoes" },
            { "material.cattail_fiber", "Region12/CattailFiber" },
            { "material.tin_ore", "Region12/TinOre" },
            { "material.yellow_ochre", "Region12/YellowOchre" },
            { "fish.channel_catfish", "Tier02/Drops/ChannelCatfish" },
            { "fish.common_carp", "Tier02/Drops/CommonCarp" },
            { "fish.northern_pike", "Tier02/Drops/NorthernPike" },
            { "fish.walleye", "Tier02/Drops/Walleye" },
            { "fishery.crab", "Tier02/ShopInputs/Crab" },
            { "animal.duck.meat", "Tier02/ShopInputs/DuckMeat" },
            { "animal.chicken.meat", "Tier01/ShopInputs/ChickenMeat" },
            { "ingredient.lavender", "Tier02/ShopInputs/Lavender" },
            { "item.flax_cord", "Tier02/Materials/FlaxCord" },
            { "item.flax_stuffing", "Tier02/Materials/FlaxStuffing" },
            { "item.flax_cloth", "Tier02/Materials/FlaxCloth" },
            { "item.cattail_cord", "Tier02/Materials/CattailCord" },
            { "item.fine_flax_cloth", "Tier02/Materials/FineFlaxCloth" },
        };

        public static string PathFor(string itemId) => itemId != null && Paths.TryGetValue(itemId, out string path) ? path : null;

        // True when ResourceVisualTuning finds a model for the item. Probes on a throwaway object so editor checks can call it.
        public static bool HasModel(ItemDefinition definition)
        {
            var probe = new GameObject("Model probe", typeof(MeshRenderer), typeof(MeshFilter));
            try { return ResourceVisualTuning.Attach(probe, definition) != null; }
            finally { if (Application.isPlaying) Object.Destroy(probe); else Object.DestroyImmediate(probe); }
        }
    }
}
