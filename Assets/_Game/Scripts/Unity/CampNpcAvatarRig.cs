using UnityEngine;

namespace TheTrailWalker
{
    // Art-only rigid-part rig. No selection, unlocking, spawning, trade or animation behavior.
    public sealed class CampNpcAvatarRig : MonoBehaviour
    {
        public Transform GroundAnchor, SeatAnchor, HipsPivot, TorsoPivot, HeadPivot;
        public Transform LeftShoulder, RightShoulder, LeftElbow, RightElbow, LeftWrist, RightWrist;
        public Transform LeftHip, RightHip, LeftKnee, RightKnee, LeftAnkle, RightAnkle;
        public Transform HatAnchor, BackpackAnchor;
        public Renderer[] SkinSurfaces, HairSurfaces, ClothingSurfaces, BackpackSurfaces;
        // Faceted revision 4: every hair style is built in (Crop, Sweep, Bob, Tied); men also carry an optional beard.
        public GameObject[] HairStyles;
        public GameObject Beard;
        public string BodyVariant, OutfitStudy;
    }
}
