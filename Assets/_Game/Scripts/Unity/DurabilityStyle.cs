using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    public enum DurabilityBand { Healthy, Warning, Critical, Neutral }

    // Shared durability presentation (equipment design v1, October 31, 2026; tokens in
    // Assets/_Game/Art/UI/Equipment/design-tokens.json). The band comes from remaining / maximum, never the raw value:
    // green above 50%, orange from 10% through 50% (both boundaries orange), red below 10%. No maximum (or infinite
    // gear) is Neutral and never divides by zero. Used by the equipment slots, bag items and tool hooks.
    public static class DurabilityStyle
    {
        public static readonly Color Panel = Hex(0x162A2C), Slot = Hex(0x19272A), Border = Hex(0x96744E), Text = Hex(0xF1D5AA),
            Selection = Hex(0xF0CE94), Badge = Hex(0x101C1E), Healthy = Hex(0x86ED69), Warning = Hex(0xFF9C3D), Critical = Hex(0xFF5555),
            Track = Hex(0x41483E), BadgeEdge = Hex(0x77745C);

        public static DurabilityBand Band(double remaining, double maximum)
        {
            if (maximum <= 0 || double.IsNaN(remaining) || double.IsInfinity(remaining) || double.IsNaN(maximum) || double.IsInfinity(maximum)) return DurabilityBand.Neutral;
            double fraction = remaining / maximum;
            return fraction > 0.5 ? DurabilityBand.Healthy : fraction >= 0.1 ? DurabilityBand.Warning : DurabilityBand.Critical;
        }

        public static float Fill(double remaining, double maximum) =>
            Band(remaining, maximum) == DurabilityBand.Neutral ? 1f : Mathf.Clamp01((float)(remaining / maximum));

        public static Color ColorOf(DurabilityBand band) =>
            band == DurabilityBand.Healthy ? Healthy : band == DurabilityBand.Warning ? Warning : band == DurabilityBand.Critical ? Critical : Text;

        // Badge on the lower edge of an item picture: dark backing, the value, and a short proportional bar (no bar for
        // Neutral values such as infinite or default gear). Rebuilt by the caller when the value changes.
        public static RectTransform AddBadge(RectTransform parent, string value, double remaining, double maximum, bool compact)
        {
            DurabilityBand band = Band(remaining, maximum);
            var badge = Child("Durability badge", parent);
            Image backing = badge.gameObject.AddComponent<Image>(); backing.sprite = RoundedSprite; backing.type = Image.Type.Sliced;
            backing.color = Badge; backing.raycastTarget = false; // opaque: partial alpha bleeds visibly in linear blending
            Outline edge = badge.gameObject.AddComponent<Outline>(); edge.effectColor = BadgeEdge; edge.effectDistance = new Vector2(1f, -1f);
            bool bar = band != DurabilityBand.Neutral;
            var label = Child("Value", badge); Stretch(label, 0.04f, bar ? 0.30f : 0.04f, 0.96f, 0.98f);
            Text text = label.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter; text.color = ColorOf(band); text.raycastTarget = false;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 8; text.resizeTextMaxSize = compact ? 26 : 30;
            if (bar)
            {
                var track = Child("Bar track", badge); Stretch(track, 0.08f, 0.10f, 0.92f, 0.24f);
                Image trackImage = track.gameObject.AddComponent<Image>(); trackImage.sprite = RoundedSprite; trackImage.type = Image.Type.Sliced; trackImage.color = Track; trackImage.raycastTarget = false;
                var fill = Child("Bar fill", track); Stretch(fill, 0, 0, Fill(remaining, maximum), 1);
                Image fillImage = fill.gameObject.AddComponent<Image>(); fillImage.sprite = RoundedSprite; fillImage.type = Image.Type.Sliced; fillImage.color = ColorOf(band); fillImage.raycastTarget = false;
            }
            return badge;
        }

        // Rounded rectangle (filled) and rounded frame (outline only) sprites, generated once and 9-sliced, so frames
        // and badges need no imported textures. The SVGs in Art/UI/Equipment remain the authoring reference.
        private static Sprite rounded, frame;
        public static Sprite RoundedSprite => rounded != null ? rounded : rounded = Generate(false);
        public static Sprite FrameSprite => frame != null ? frame : frame = Generate(true);

        private static Sprite Generate(bool outline)
        {
            const int size = 64; const float radius = 18f, stroke = 3.5f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = outline ? "UI frame" : "UI rounded" };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    // Signed distance to a rounded square inset by half a pixel.
                    float px = Mathf.Abs(x + 0.5f - size / 2f) - (size / 2f - radius - 0.5f), py = Mathf.Abs(y + 0.5f - size / 2f) - (size / 2f - radius - 0.5f);
                    float outside = new Vector2(Mathf.Max(px, 0), Mathf.Max(py, 0)).magnitude + Mathf.Min(Mathf.Max(px, py), 0) - radius;
                    float alpha = Mathf.Clamp01(0.5f - outside);
                    if (outline) alpha *= Mathf.Clamp01(outside + stroke + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius + 2, radius + 2, radius + 2, radius + 2));
        }

        private static RectTransform Child(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); return rect;
        }
        public static void Stretch(RectTransform rect, float x0, float y0, float x1, float y1)
        {
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static Color Hex(int rgb) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
    }
}
