using UnityEngine;

namespace TheTrailWalker.CameraControl
{
    [DefaultExecutionOrder(100)]
    public sealed class FollowCameraRig : MonoBehaviour
    {
        public Transform Target;
        public CameraOrbitController Orbit;
        private void LateUpdate()
        {
            if (Target == null) return;
            transform.SetPositionAndRotation(Target.position + Vector3.up * 1.65f,
                Quaternion.Euler(15f, Orbit.Yaw, 0f));
        }
    }
}
