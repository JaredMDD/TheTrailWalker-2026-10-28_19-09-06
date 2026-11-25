using System.Collections.Generic;
using TheTrailWalker.Domain.Items;
using UnityEngine;

namespace TheTrailWalker
{
    // Resources paths for the shop and bakery goods that no other lookup covers.
    public static class Tier2ShopVisuals
    {
        private static readonly Dictionary<string, string> Paths = new Dictionary<string, string>
        {
            { "item.wheat_bread", "Tier01/Crafted/WheatBread" },
            { "item.pine_nut_oat_cookies", "Tier01/Crafted/PineNutOatCookies" },
            { "item.strawberry_cream_roll", "Tier01/Crafted/StrawberryCreamRoll" },
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
            { "item.barley_bread", "Tier02/Food/BarleyBread" },
            { "item.pike_barley_cakes", "Tier02/Food/PikeBarleyCakes" },
            { "item.honey_brioche", "Tier02/Food/HoneyBrioche" },
            { "item.blackberry_cream_pastry", "Tier02/Food/BlackberryCreamPastry" },
            { "item.blueberry_muffins", "Tier02/Food/BlueberryMuffins" },
            { "item.flax_oil_biscuits", "Tier02/Food/FlaxOilBiscuits" },
            // Mystery-box recipe products: their BalloonExclusiveArtCatalog visual; ranch and keepsake products the catalog lacks use tier-two art.
            { "item.creamy_crab_soup", "BalloonExclusive/Items/CreamyCrabSoup" }, { "item.honey_blackberry_biscuits", "BalloonExclusive/Items/HoneyBlackberryBiscuits" }, { "item.squid_rice_bowl", "BalloonExclusive/Items/SquidRiceBowl" },
            { "item.mint_cow_cheese_flatbread", "BalloonExclusive/Items/MintCowCheeseFlatbread" }, { "item.lobster_rice_bowl", "BalloonExclusive/Items/LobsterRiceBowl" }, { "item.cacao_coconut_tart", "BalloonExclusive/Items/CacaoCoconutTart" },
            { "item.beaver_barley_hotpot", "BalloonExclusive/Items/BeaverBarleyHotpot" }, { "item.blackberry_honey_oats", "BalloonExclusive/Items/BlackberryHoneyOats" }, { "item.turkey_bean_bowl", "BalloonExclusive/Items/TurkeyBeanBowl" },
            { "item.pumpkin_chickpea_pot", "BalloonExclusive/Items/PumpkinChickpeaPot" }, { "item.boar_potato_chili", "BalloonExclusive/Items/BoarPotatoChili" }, { "item.coconut_cacao_pudding", "BalloonExclusive/Items/CoconutCacaoPudding" },
            { "item.birch_wax_bee", "BalloonExclusive/Items/BirchWaxBee" }, { "item.painted_turkey", "BalloonExclusive/Items/PaintedTurkey" }, { "item.silver_sun_brooch", "BalloonExclusive/Items/SilverSunBrooch" },
            { "item.obsidian_boar", "BalloonExclusive/Items/ObsidianBoar" }, { "item.truffle_cow_cheese_flatbread", "BalloonExclusive/Items/TruffleCowCheeseFlatbread" }, { "item.turkey_truffle_bean_pot", "BalloonExclusive/Items/TurkeyTruffleBeanPot" },
            { "item.apple_cider_barrel", "BalloonExclusive/Items/AppleCiderBarrel" }, { "item.apple_cider", "BalloonExclusive/Items/AppleCiderBottle" }, { "item.strawberry_wine_barrel", "BalloonExclusive/Items/StrawberryWineBarrel" },
            { "item.strawberry_wine", "BalloonExclusive/Items/StrawberryWineBottle" }, { "item.blackberry_wine_barrel", "BalloonExclusive/Items/BlackberryWineBarrel" }, { "item.blackberry_wine", "BalloonExclusive/Items/BlackberryWineBottle" },
            { "item.blueberry_wine_barrel", "BalloonExclusive/Items/BlueberryWineBarrel" }, { "item.blueberry_wine", "BalloonExclusive/Items/BlueberryWineBottle" }, { "item.honey_mead_barrel", "BalloonExclusive/Items/HoneyMeadBarrel" },
            { "item.honey_mead", "BalloonExclusive/Items/HoneyMeadBottle" }, { "item.barley_ale_barrel", "BalloonExclusive/Items/BarleyAleBarrel" }, { "item.barley_ale", "BalloonExclusive/Items/BarleyAleBottle" },
            { "item.wheat_beer_barrel", "BalloonExclusive/Items/WheatBeerBarrel" }, { "item.wheat_beer", "BalloonExclusive/Items/WheatBeerBottle" }, { "item.cane_rum_barrel", "BalloonExclusive/Items/CaneRumBarrel" },
            { "item.cane_rum", "BalloonExclusive/Items/CaneRumBottle" }, { "item.corn_whiskey_barrel", "BalloonExclusive/Items/CornWhiskeyBarrel" }, { "item.corn_whiskey", "BalloonExclusive/Items/CornWhiskeyBottle" },
            { "item.truffle_butter_crab", "BalloonExclusive/Items/TruffleButterCrab" }, { "item.crispy_mint_squid", "BalloonExclusive/Items/CrispyMintSquid" }, { "item.coconut_chili_lobster", "BalloonExclusive/Items/CoconutChiliLobster" },
            { "item.honey_roast_duck", "Tier02/Food/HoneyRoastDuck" }, { "item.duck_blueberry_rice", "Tier02/Food/DuckBlueberryRice" },
            { "item.quartz_prism_lantern", "Tier02/Misc/QuartzPrismLantern" }, { "item.tin_quartz_music_box", "Tier02/Misc/TinQuartzMusicBox" },
            // The tier-one ranch dishes (not in the catalog): generate_ranch_dishes.py, BakeryFoodAssets.BuildRanchDishes.
            { "item.chicken_mushroom_pie", "Tier01/Crafted/ChickenMushroomPie" }, { "item.apple_glazed_chicken", "Tier01/Crafted/AppleGlazedChicken" },
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
