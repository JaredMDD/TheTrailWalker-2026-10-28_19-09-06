using UnityEngine;

namespace TheTrailWalker.CameraControl
{
    [DefaultExecutionOrder(100)]
    public sealed class FollowCameraRig : MonoBehaviour
    {
        public Transform Target;
        public CameraOrbitController Orbit;
        public const float BasePitch = 15f;
        // Extra pitch that follows the road's slope in the viewing direction, so climbs and descents keep the same view
        // distance (and pickup reach) as flat ground. Smoothed so knots in the path do not jolt the view.
        public float SlopePitch { get; private set; }
        private float slopeVelocity;
        // Harvest interactions focus the view on their target (section 8.2.1) with camera control off; the camera eases
        // to the focus pose and back. Clearing the focus returns to the follow view at the orbit's yaw.
        private bool focused; private Vector3 focusEye, focusLook; private float focusBlend;
        public void Focus(Vector3 eye, Vector3 look) { focused = true; focusEye = eye; focusLook = look; }
        public void ClearFocus() => focused = false;
        // Faint pose (section 4.4.2, November 2, 2026): 0 = standing eyes, 1 = collapsed on the ground (eyes low, rolled
        // and tipped towards the road, with slow breathing). Gameplay owns the timing; the rig only applies the pose.
        public float Collapse { get; set; }
        public const float CollapsedEye = 0.32f, CollapsedRoll = 24f, CollapsedPitch = 32f;
        private Vector3 CollapseEye(Vector3 eye) => eye + Vector3.down * ((1.65f - CollapsedEye) * Ease(Collapse)) + Vector3.up * (Breath * 0.03f);
        private Quaternion CollapseTurn(Quaternion turn) => Collapse <= 0f ? turn : turn * Quaternion.Euler(CollapsedPitch * Ease(Collapse) + Breath, 0f, CollapsedRoll * Ease(Collapse));
        private float Breath => Collapse > 0.98f ? Mathf.Sin(Time.unscaledTime * 1.6f) * 1.2f : 0f;
        private static float Ease(float t) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
        private void LateUpdate()
        {
            if (Target == null) return;
            if (Collapse > 0f)
            {
                SlopePitch = Mathf.SmoothDamp(SlopePitch, 0f, ref slopeVelocity, 0.35f);
                transform.SetPositionAndRotation(CollapseEye(Target.position + Vector3.up * 1.65f), CollapseTurn(Quaternion.Euler(BasePitch + SlopePitch, Orbit.Yaw, 0f)));
                return;
            }
            focusBlend = Mathf.MoveTowards(focusBlend, focused ? 1f : 0f, Time.deltaTime / 0.45f);
            if (focusBlend > 0f)
            {
                Vector3 followEye = Target.position + Vector3.up * 1.65f;
                Quaternion followTurn = Quaternion.Euler(BasePitch + SlopePitch, Orbit.Yaw, 0f);
                float t = Mathf.SmoothStep(0f, 1f, focusBlend);
                transform.SetPositionAndRotation(Vector3.Lerp(followEye, focusEye, t),
                    Quaternion.Slerp(followTurn, Quaternion.LookRotation(focusLook - focusEye), t));
                return;
            }
            Vector3 forward = Target.forward; var flat = new Vector2(forward.x, forward.z); float run = flat.magnitude;
            float target = 0f;
            // Seated or walking to a seat: the camp flow poses the player, so the view stays level.
            bool detached = Target.TryGetComponent(out Movement.TrailMotor motor) && motor.Detached;
            if (run > 0.001f && !detached)
            {
                Vector3 view = Quaternion.Euler(0f, Orbit.Yaw, 0f) * Vector3.forward;
                float along = Vector2.Dot(new Vector2(view.x, view.z), flat / run);
                target = -Mathf.Atan(forward.y / run * along) * Mathf.Rad2Deg;
            }
            SlopePitch = Mathf.SmoothDamp(SlopePitch, target, ref slopeVelocity, 0.35f);
            transform.SetPositionAndRotation(Target.position + Vector3.up * 1.65f,
                Quaternion.Euler(BasePitch + SlopePitch, Orbit.Yaw, 0f));
        }
    }
}
