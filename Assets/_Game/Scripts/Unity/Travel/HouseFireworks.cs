using UnityEngine;

namespace TheTrailWalker.Travel
{
    // The house upgrade's fireworks (slice 2b task 4; spec section 5): one runtime ParticleSystem over the finished house,
    // four coloured bursts in the game's palette, on unscaled time so a paused or slowed game still shows them. It destroys
    // itself shortly after the last burst; entering the house stops it early.
    public sealed class HouseFireworks : MonoBehaviour
    {
        private static readonly float[] BurstTimes = { 0f, 0.7f, 1.4f, 2.1f };
        private const float LingerSeconds = 1.5f;
        private static readonly Color32[] Palette =
        {
            new Color32(249, 211, 90, 255), new Color32(222, 153, 73, 255), new Color32(185, 48, 42, 255), new Color32(241, 232, 205, 255),
        };

        private ParticleSystem system;
        private float born;

        // Unity's Destroy(go, delay) counts scaled time while the particles run on unscaled time, so the lifetime is tracked here.
        private void Update()
        {
            if (Time.unscaledTime - born >= BurstTimes[BurstTimes.Length - 1] + LingerSeconds) Destroy(gameObject);
        }

        public static HouseFireworks Play(Vector3 worldPosition)
        {
            var go = new GameObject("House fireworks");
            go.transform.position = worldPosition;
            var fireworks = go.AddComponent<HouseFireworks>();
            fireworks.born = Time.unscaledTime;
            fireworks.Build();
            return fireworks;
        }

        public void Stop()
        {
            if (system != null) system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Build()
        {
            system = gameObject.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.useUnscaledTime = true;
            main.loop = false; main.playOnAwake = false;
            main.duration = BurstTimes[BurstTimes.Length - 1] + 0.1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.1f, 1.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.32f);
            main.gravityModifier = 0.9f;
            main.maxParticles = 800;
            main.startColor = new ParticleSystem.MinMaxGradient(PaletteGradient()) { mode = ParticleSystemGradientMode.RandomColor };

            var emission = system.emission;
            emission.rateOverTime = 0f;
            var bursts = new ParticleSystem.Burst[BurstTimes.Length];
            for (int i = 0; i < bursts.Length; i++) bursts[i] = new ParticleSystem.Burst(BurstTimes[i], 70);
            emission.SetBursts(bursts);

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.4f;

            var fade = system.colorOverLifetime;
            fade.enabled = true;
            var alpha = new Gradient();
            alpha.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            fade.color = alpha;

            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material material = Resources.Load<Material>("Effects/FireworkParticle");
            if (material != null) renderer.sharedMaterial = material;
            system.Play(true);
        }

        // A fixed-mode gradient of the four palette colours; the start colour picks one of them at random.
        private static Gradient PaletteGradient()
        {
            var g = new Gradient();
            var keys = new GradientColorKey[Palette.Length];
            for (int i = 0; i < Palette.Length; i++) keys[i] = new GradientColorKey(Palette[i], i / (float)(Palette.Length - 1));
            g.mode = GradientMode.Fixed;
            g.SetKeys(keys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }
    }
}
