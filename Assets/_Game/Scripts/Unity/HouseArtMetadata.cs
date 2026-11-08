using UnityEngine;
using UnityEngine.Serialization;

namespace TheTrailWalker
{
    // Passive art contract only. It grants no house level, furniture, pet or storage.
    // Depth growth (user, November 7, 2026): FurnishingGrid is columns x rows on the floor; rows run toward the back.
    public sealed class HouseArtMetadata : MonoBehaviour
    {
        public int Level, MaterialTier;
        public Vector2Int FurnishingGrid;
        public float GridCellMetres = .5f, PlotWidth = 11.2f, PlotDepth = 10.6f;
        public bool PreviewOnly;
        public Vector3 GridAxisX = new Vector3(-.5f, 0, 0);
        [FormerlySerializedAs("GridAxisY")] public Vector3 GridAxisDepth = new Vector3(0, 0, -.5f);
    }
}
