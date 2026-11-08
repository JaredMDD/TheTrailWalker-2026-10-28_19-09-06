using System;
using UnityEngine;

namespace TheTrailWalker
{
    [Serializable] public sealed class BalloonSpecialItemArt
    {
        public string Id,Name,Slot,Anchor;
        public int Tier;
        public GameObject Display,MaleWearable,FemaleWearable;
    }
    [Serializable] public sealed class BalloonMysteryBoxArt
    {
        public int Tier,FirstRegion,LastRegion;
        public GameObject Prefab;
        public Material Material;
    }
    // Presentation references only. This catalog never rolls stock, grants items or opens paid boxes.
    public sealed class BalloonGoodsArtCatalog : ScriptableObject
    {
        public BalloonSpecialItemArt[] SpecialItems;
        public BalloonMysteryBoxArt[] MysteryBoxes;
        public GameObject[] MaleHatHair,FemaleHatHair;
        public BalloonMysteryBoxArt MysteryBoxForCampRegion(int regionNumber)
        {
            if(regionNumber<1||regionNumber>24)throw new ArgumentOutOfRangeException(nameof(regionNumber));
            foreach(var art in MysteryBoxes)if(regionNumber>=art.FirstRegion&&regionNumber<=art.LastRegion)return art;
            throw new InvalidOperationException("Missing mystery-box art for camp region "+regionNumber);
        }
    }
}
