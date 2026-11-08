using System;
using UnityEngine;

namespace TheTrailWalker
{
    [Serializable] public sealed class CampNpcOutfitArt
    {
        public string Name;
        public int VisualTier;
        public GameObject Male, Female, Backpack;
        public Material ClothingMaterial;
    }
    public sealed class CampNpcArtCatalog : ScriptableObject
    {
        public GameObject MaleBase, FemaleBase;
        public Material[] SkinTones, HairColors;
        public CampNpcOutfitArt[] OutfitStudies;
        [TextArea(3,8)] public string FutureSelectionRule;
    }
}
