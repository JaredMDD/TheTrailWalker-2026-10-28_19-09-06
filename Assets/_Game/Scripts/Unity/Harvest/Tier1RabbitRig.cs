using UnityEngine;
namespace TheTrailWalker.Harvest
{
    // Rigid-part art rig. AnimalAnimator drives the pivots: hopping, grazing and looking around (plan 8.2.2).
    public sealed class Tier1RabbitRig : MonoBehaviour
    {
        public Transform MotionRoot, BodyPivot, HeadPivot, LeftEarPivot, RightEarPivot;
        public Transform FrontLeftPivot, FrontRightPivot, HindLeftPivot, HindRightPivot, TailPivot;
        public Transform GroundAnchor, HintAnchor;

        // Joint points match the faceted revision-4 parts (Tools/Art/generate_tier01_rabbit.py): the forelegs swing at the
        // chest, the haunches at the hip, and the body pitches about the middle of the torso.
        public AnimalRigParts Parts() => new AnimalRigParts
        {
            Gait = AnimalGait.Hop, MotionRoot = MotionRoot, Body = BodyPivot, Head = HeadPivot, Tail = TailPivot,
            Ears = new[] { LeftEarPivot, RightEarPivot }, FrontLegs = new[] { FrontLeftPivot, FrontRightPivot }, HindLegs = new[] { HindLeftPivot, HindRightPivot },
            BodyCentre = new Vector3(0, .27f, -.03f), ShoulderHeight = .25f, ShoulderZ = .13f, HipHeight = .28f, HipZ = -.20f,
            Stride = .22f, HopHeight = .06f, GrazeDrop = .13f, GrazeAngle = 55f
        };
    }
}
