using UnityEngine;
namespace TheTrailWalker.Harvest
{
    // Art metadata on each massive lumber-field tree prefab (Claude, November 8, 2026; generate_massive_trees.py).
    // The chopping rig is the usual Tier1PineRig: LowerTrunk is the stump that stays for the 72-hour regrowth, and
    // FallPivot carries the bole and crown. Lumber field placement and timers are Milestone 13.
    public sealed class MassiveTreeArt : MonoBehaviour
    {
        public string Species;      // Pine, Birch, Mesquite, Oak
        public string Variant;      // A, B, C trunk shapes
        public int WoodTier;        // 1-4
        public float CutHeight, CutRadius, Height, CrownRadius;
    }
}
