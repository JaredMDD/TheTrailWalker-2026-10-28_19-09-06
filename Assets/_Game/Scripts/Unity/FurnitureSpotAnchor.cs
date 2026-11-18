using UnityEngine;

namespace TheTrailWalker
{
    // One hang spot on a tool rack (row-major from the top-left). The anchor sits at the centre of the volume a hung piece occupies.
    public sealed class FurnitureSpotAnchor : MonoBehaviour
    {
        public Vector3 PegBox = new Vector3(.22f, .40f, .12f);   // metres
    }
}
