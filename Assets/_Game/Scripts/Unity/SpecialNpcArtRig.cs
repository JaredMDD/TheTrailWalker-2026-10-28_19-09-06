using UnityEngine;

namespace TheTrailWalker
{
    // Art authoring references only. No animation/interaction methods or Update behavior.
    public sealed class SpecialNpcArtRig : MonoBehaviour
    {
        public string Role, PoseStudy;
        public Transform GroundAnchor, FacingPivot, HipsPivot, TorsoPivot, HeadPivot, HatAnchor;
        public Transform LeftShoulder, RightShoulder, LeftElbow, RightElbow, LeftWrist, RightWrist;
        public Transform RecipeGripAnchor, OarAnchor;
        public GameObject RecipeVisual;
    }
}
