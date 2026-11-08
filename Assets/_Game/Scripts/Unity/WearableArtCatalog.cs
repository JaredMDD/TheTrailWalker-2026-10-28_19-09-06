using System;
using TheTrailWalker.Domain.Equipment;
using UnityEngine;

namespace TheTrailWalker
{
    // One data-driven map of every fitted garment, any tier (equipment redesign, user, November 7, 2026). Replaces the
    // split between PlayerAvatarArtCatalog's tier-one shell arrays (shirt, pants, boots) and the tier-one accessory JSON
    // (hat, backpack). Built by The Trail Walker > Art > Build Wearable Art Catalog from every fitted source: tier-one
    // shells and accessories, the tier-two Tier02Batch*ArtMap.json files and the balloon special gear. New fitted
    // garments only need to appear in one of those sources (or a new one added to the builder).
    [Serializable] public sealed class WearableArt
    {
        public string ItemId;            // the item model ID the garment represents (ItemArt.ModelFor(item).Id)
        public GearSlot Slot;
        public string Anchor;            // "Root" (body-root shells), "HeadAnchor/HatAnchor", "BackpackAnchor"
        public GameObject Male, Female;  // worn prefabs, identity transform under the anchor
        public string Source;            // where the entry came from, for reviews
        public GameObject For(bool female) => female ? Female : Male;
    }

    public sealed class WearableArtCatalog : ScriptableObject
    {
        public const string ResourcePath = "Characters/WearableArtCatalog";
        public WearableArt[] Items = Array.Empty<WearableArt>();
        public GameObject[] MaleHatHair, FemaleHatHair;   // Crop, Sweep, Bob, Tied: the hair shown under any fitted hat

        private static WearableArtCatalog loaded;
        public static WearableArtCatalog Current => loaded != null ? loaded : (loaded = Resources.Load<WearableArtCatalog>(ResourcePath));

        public WearableArt Find(string itemId)
        {
            if (itemId == null || Items == null) return null;
            foreach (WearableArt entry in Items) if (entry != null && entry.ItemId == itemId) return entry;
            return null;
        }
        public GameObject HatHair(bool female, int style)
        {
            GameObject[] set = female ? FemaleHatHair : MaleHatHair;
            return set != null && style >= 0 && style < set.Length ? set[style] : null;
        }
    }
}
