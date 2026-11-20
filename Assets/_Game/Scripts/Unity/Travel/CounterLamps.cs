using System.Collections.Generic;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Milestone 14 phone round R2: every counter interior (five shops, warehouse, lumber) hangs two lanterns under its roof, each
    // with one warm, shadowless, short-range point light, so a visible lamp explains the light. Placed at runtime under the
    // counter (never baked into its prefab), so the counter's own teardown removes them. No RenderSettings change.
    public static class CounterLamps
    {
        public const string LampName = "Counter lamp";
        public const float Range = 5f, Intensity = 2.2f;   // tuned on the counter render sheet
        public static readonly Color Warm = new Color(1f, 0.82f, 0.58f);
        const float Gap = 0.01f;   // lamp top below the roof underside

        // Counter-space hang points (x values, z) and the roof underside's depth below the room's top (the counter's renderer
        // bounds max). The shared storefront room has beams along x at z -1.6, -0.6 and 0.4 whose undersides sit 0.25 m below
        // its top; the lumber room has a flat ceiling 0.10 m below, and its camera looks left of centre, so its lamps shift left.
        // Milestone14Validation.CounterLampChecks probes the real roof with mesh colliders.
        static readonly (float[] xs, float z, float drop) Shop = (new[] { -1.3f, 1.3f }, -0.6f, 0.25f);
        static readonly Dictionary<string, (float[] xs, float z, float drop)> Overrides = new Dictionary<string, (float[], float, float)>
        {
            { "Lumber", (new[] { -2.0f, 0.2f }, -0.6f, 0.10f) },
        };

        public static void Hang(Transform counter, string counterName)
        {
            var prefab = Resources.Load<GameObject>("Settlement/Storefronts/CounterLamp");
            if (prefab == null) Debug.LogWarning("[Settlement] Missing art Settlement/Storefronts/CounterLamp; the counter hangs no lamps.");
            if (counter == null || prefab == null) return;
            float top = float.MinValue;
            foreach (Renderer r in counter.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
            if (top == float.MinValue) return;
            var spot = counterName != null && Overrides.TryGetValue(counterName, out var o) ? o : Shop;
            float y = top - counter.position.y - spot.drop - Gap;   // counters stand upright and unscaled
            foreach (float x in spot.xs)
            {
                Transform lamp = Object.Instantiate(prefab, counter, false).transform;
                lamp.name = LampName; lamp.localPosition = new Vector3(x, y, spot.z);
                Transform anchor = lamp.Find("LightAnchor");
                Light light = (anchor != null ? anchor.gameObject : lamp.gameObject).AddComponent<Light>();
                light.type = LightType.Point; light.shadows = LightShadows.None; light.range = Range; light.intensity = Intensity; light.color = Warm;
            }
        }
    }
}
