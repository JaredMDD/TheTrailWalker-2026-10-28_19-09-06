using System;
using UnityEngine;

namespace TheTrailWalker
{
    [Serializable] public sealed class CampLogSeatArt
    {
        public int WoodTier, FirstRegion, LastRegion;
        public string Wood;
        public GameObject Prefab;
        public float SittingHeight=.43f;
    }
    // Art references only; future camp assembly chooses by region wood tier, independently of NPC clothing.
    // Campfire per wood tier (tier two: burning birch fire, November 6, 2026). Regions without an entry keep the static
    // burned-wood fire.
    [Serializable] public sealed class CampfireArt
    {
        public int WoodTier, FirstRegion, LastRegion;
        public string Wood;
        public GameObject Prefab;
    }
    public sealed class CampPropArtCatalog : ScriptableObject
    {
        public GameObject StaticBurnedWoodCampfire;
        public CampLogSeatArt[] LogSeats;
        public CampfireArt[] Campfires;
    }
}
