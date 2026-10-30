using UnityEngine;

namespace TheTrailWalker.CameraControl
{
    public sealed class CameraOrbitController : MonoBehaviour
    {
        public float Yaw { get; private set; }
        public void Restore(float yaw) { if (float.IsNaN(yaw) || float.IsInfinity(yaw)) throw new System.ArgumentException("Invalid yaw"); Yaw = Mathf.Repeat(yaw, 360f); }
        public void Rotate(float degrees) => Yaw = Mathf.Repeat(Yaw + degrees, 360f);
    }
}
