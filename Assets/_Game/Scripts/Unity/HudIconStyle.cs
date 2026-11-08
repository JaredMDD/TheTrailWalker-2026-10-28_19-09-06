using System.Collections.Generic;
using UnityEngine;

namespace TheTrailWalker
{
    public static class HudIconStyle
    {
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static void Apply(UnityEngine.UI.Text label, string key)
        {
            if (ApplyTo((RectTransform)label.transform.parent, key, true)) label.enabled = false;
        }

        public static bool ApplyTo(RectTransform target, string key, bool plate)
        {
            if (!Sprites.TryGetValue(key, out Sprite sprite) || sprite == null)
            {
                sprite = Resources.Load<Sprite>("UI/Icons/" + key);
                if (sprite != null) Sprites[key] = sprite;
            }
            if (sprite == null) return false; // Keep text if import has not finished.
            UnityEngine.UI.Image background = target.GetComponent<UnityEngine.UI.Image>();
            if (background != null) background.color = plate ? new Color(.91f, .84f, .70f, .97f) : Color.clear; // beige plate
            if (plate)
            {
                UnityEngine.UI.Outline border = target.GetComponent<UnityEngine.UI.Outline>();
                if (border == null) border = target.gameObject.AddComponent<UnityEngine.UI.Outline>();
                border.effectColor = new Color(0, 0, 0, .7f);
                border.effectDistance = new Vector2(1, -1);
            }
            Transform existing = target.Find("Silhouette");
            RectTransform rect = existing != null ? (RectTransform)existing : new GameObject("Silhouette", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(target, false);
            rect.anchorMin = new Vector2(.08f, .08f);
            rect.anchorMax = new Vector2(.92f, .92f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            UnityEngine.UI.Image icon = rect.GetComponent<UnityEngine.UI.Image>();
            if (icon == null) icon = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            icon.sprite = sprite;
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return true;
        }
    }
}
