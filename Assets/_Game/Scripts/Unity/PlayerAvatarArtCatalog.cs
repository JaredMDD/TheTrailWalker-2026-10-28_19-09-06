using UnityEngine;

namespace TheTrailWalker
{
    // Runtime references to the default player avatar art (Docs/Art/Characters/PlayerDefault/player-avatar-designs.txt).
    // Built by The Trail Walker > Art > Build Player Avatar Catalog into Resources/Characters/PlayerDefault.
    // Bodies use the Skin01 art prefabs; skin, hair style and hair colour are swapped at runtime.
    public sealed class PlayerAvatarArtCatalog : ScriptableObject
    {
        public GameObject MaleBody, FemaleBody;
        public GameObject[] MaleHair, FemaleHair;          // Crop, Sweep, Bob, Tied
        public Material[] SkinMaterials, HairMaterials;     // Skin01-06; Hair01-05 (black, brown, auburn, blonde, gray)
        public static readonly string[] HairStyles = { "Crop", "Sweep", "Bob", "Tied" };
        // Fitted wearables (user, November 5, 2026): the item model ID each shell represents, with its male and female
        // shell prefabs (root space, same frame as the body). Items without an entry keep the basewear.
        public string[] WearableItemIds;
        public GameObject[] MaleWearables, FemaleWearables;

        public GameObject Wearable(string itemId, bool female)
        {
            if (WearableItemIds == null || itemId == null) return null;
            int i = System.Array.IndexOf(WearableItemIds, itemId);
            GameObject[] set = female ? FemaleWearables : MaleWearables;
            return i >= 0 && set != null && i < set.Length ? set[i] : null;
        }
    }
}
