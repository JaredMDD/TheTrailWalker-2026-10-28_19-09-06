using System;
using UnityEngine;

namespace TheTrailWalker
{
    [Serializable] public sealed class RegionalSceneryArt
    {
        public string Id, Name, Category, ResourcePath;
        public GameObject Prefab;
        public Bounds Bounds;
        public int Triangles;
        public float FootprintRadius, RecommendedRoadClearance;
        public bool CastsShadows, DistantOnly, TerrainReplacement;
    }
    [Serializable] public sealed class RegionalEnvironmentKit
    {
        public int Number, PaletteSize;
        public string Name;
        public Material SharedMaterial, TrailMaterial;
        public RegionalSceneryArt[] Models;
    }
    // Art data only. A future trail-design/assembly pass chooses region-specific pieces and actual positions.
    public sealed class Tier01EnvironmentLibrary : ScriptableObject
    {
        public int Revision;
        [TextArea(3,8)] public string DrawingContract;
        public RegionalEnvironmentKit[] Regions;
    }
}
