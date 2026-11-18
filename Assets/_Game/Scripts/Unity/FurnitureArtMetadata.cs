using UnityEngine;

namespace TheTrailWalker
{
    // Passive art contract for store furniture (PROJECT_RECOMENDATIONS.md section 13.2). It grants no buff, item or
    // storage by itself: buffs come from placed pieces (once per type), chests open by touch, everything else moves by hold.
    // The prefab pivot is the front-left footprint corner; the body spans x in [-width, 0] and z in [-depth, 0].
    public sealed class FurnitureArtMetadata : MonoBehaviour
    {
        public string FurnitureId, DisplayName, BuffCategory;
        public int HouseLevel, PriceCash;
        public Vector2Int Footprint;          // grid cells: columns x rows toward the back
        public float GridCellMetres = .5f;
        public bool IsChest;                  // only chests have a touch action (open the chest inventory)
        public bool IsRack;                   // the tool rack: a peg board with twelve FurnitureSpotAnchor children (Spot00-Spot11)
    }
}
