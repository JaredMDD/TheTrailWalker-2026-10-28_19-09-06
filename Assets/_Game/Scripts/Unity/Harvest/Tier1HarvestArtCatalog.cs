using System;
using UnityEngine;
namespace TheTrailWalker.Harvest
{
    [Serializable]
    public sealed class HarvestDropArt
    {
        public string ItemId;
        public GameObject Visual;
        public int NormalMinBundles, NormalMaxBundles, EliteMinBundles, EliteMaxBundles;
    }
    [Serializable]
    public sealed class Tier1FishPair
    {
        public int RegionNumber;
        public string FishA, FishB;
    }
    public sealed class Tier1HarvestArtCatalog : ScriptableObject
    {
        public GameObject PineTrunk, MassivePineTrunk, Rabbit, FishingPoint;
        public HarvestDropArt[] TreeDrops, HuntingDrops, FishVisuals;
        public Tier1FishPair[] RegionFishPairs;
        public int UnitsPerBundle=1000, MassiveTreeMultiplier=50;
        // TOTAL fish bundles drawn from a local pair, never these counts per species.
        public int FishNormalMinBundles=2, FishNormalMaxBundles=5;
        public int FishEliteMinBundles=4, FishEliteMaxBundles=10;
    }
}
