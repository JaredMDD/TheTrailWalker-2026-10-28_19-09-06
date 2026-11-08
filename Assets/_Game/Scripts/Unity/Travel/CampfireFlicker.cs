using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Gentle flame motion for the burning campfires (tier two, November 6, 2026): the opaque flame mesh breathes in
    // height and width and sways slightly, from smooth noise. No particles, lights or physics; it costs one transform
    // update per visible camp fire and stops while its renderer is not visible.
    public sealed class CampfireFlicker : MonoBehaviour
    {
        public Transform Flames;
        public float Speed = 2.2f, Height = .14f, Width = .06f, Sway = 4f;
        private Renderer view;
        private float seed;

        private void Awake()
        {
            seed = Random.value * 100f;
            if (Flames != null) view = Flames.GetComponentInChildren<Renderer>();
        }

        private void Update()
        {
            if (Flames == null || (view != null && !view.isVisible)) return;
            float t = Time.time * Speed + seed;
            float h = 1f + (Mathf.PerlinNoise(t, 0.3f) - .5f) * 2f * Height;
            float w = 1f + (Mathf.PerlinNoise(0.7f, t * 1.3f) - .5f) * 2f * Width;
            Flames.localScale = new Vector3(w, h, w);
            Flames.localRotation = Quaternion.Euler((Mathf.PerlinNoise(t * .8f, 2.1f) - .5f) * Sway, t * 9f % 360f, (Mathf.PerlinNoise(3.3f, t * .8f) - .5f) * Sway);
        }
    }
}
