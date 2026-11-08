using System.Collections.Generic;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Faint, recovery and wake presentation (section 4.4.2; Milestone 8 item 5, November 2, 2026). The loss manifest is
    // committed and saved before anything here plays; these are presentation objects only. The view collapses to the
    // ground, breathes while hearts refill, and rises again; input stays locked until the wake finishes, and walking
    // still needs a fresh upward gesture. A reload while fainted shows the collapsed view directly (no replayed loss).
    public sealed partial class CollectionPrototype
    {
        public const float FaintFallSeconds = 1.2f, FaintWakeSeconds = 1.5f, LossSlipSeconds = 1.3f, LossStagger = 0.3f;
        private float collapse;          // 0 standing .. 1 collapsed
        private bool animateFall;        // true only for a faint that happened in this session
        private bool waking;             // recovered, rising; unlocks input at the end
        private Image faintShade;
        private readonly List<(RectTransform rect, Image image, Text label, float start)> lossSlips = new List<(RectTransform, Image, Text, float)>();

        private void CreateFaintOverlay()
        {
            faintOverlay = Rect("Faint recovery", safe, 0, 0, 1, 1).gameObject;
            // Translucent: the collapsed view stays visible under the shade.
            faintShade = faintOverlay.AddComponent<Image>(); faintShade.color = new Color(0.02f, 0.03f, 0.04f, 0f);
            faintMessage = Label(faintOverlay.transform, "", 0.08f, 0.52f, 0.92f, 0.8f, 32);
            faintMessage.resizeTextForBestFit = true; faintMessage.resizeTextMinSize = 16; faintMessage.resizeTextMaxSize = 32;
            faintOverlay.SetActive(false);
        }

        // Called after the loss is committed and saved, with the lost entries captured before their removal.
        private void StartFaintPresentation(IReadOnlyList<(ItemDefinition definition, int quantity)> lost)
        {
            animateFall = true; waking = false;
            ClearLossSlips();
            for (int i = 0; i < lost.Count; i++)
            {
                float x = 0.5f + (i - (lost.Count - 1) / 2f) * 0.2f;
                RectTransform rect = Rect("Lost " + lost[i].definition.Name, faintOverlay.transform, x - 0.08f, 0.3f, x + 0.08f, 0.4f);
                Image image = rect.gameObject.AddComponent<Image>(); image.preserveAspect = true; image.raycastTarget = false;
                Sprite sprite = ItemIconRenderer.For(ItemArt.ModelFor(lost[i].definition));
                if (sprite != null) image.sprite = sprite; else image.color = ColorFor(lost[i].definition);
                Text label = Label(rect, lost[i].quantity > 1 ? "x" + lost[i].quantity.ToString("N0") : lost[i].definition.Name, -0.4f, -0.45f, 1.4f, -0.05f, 18);
                lossSlips.Add((rect, image, label, Time.time + FaintFallSeconds * 0.6f + i * LossStagger));
            }
        }

        private void UpdateFaintPresentation(float delta)
        {
            if (faintOverlay == null) return;
            bool fainted = stamina.Fainted && !saveBlocked;
            if (fainted)
            {
                waking = false;
                collapse = animateFall ? Mathf.MoveTowards(collapse, 1f, delta / FaintFallSeconds) : 1f;
            }
            else if (collapse > 0f)
            {
                waking = true;
                collapse = Mathf.MoveTowards(collapse, 0f, delta / FaintWakeSeconds);
                if (collapse <= 0f) FinishWake();
            }
            // Seated at a camp (a restart while fainted returns there) the view only slumps halfway, staying above the seat.
            FollowCameraRig rig = CameraRig; if (rig != null) rig.Collapse = collapse * (motor != null && motor.Detached ? 0.5f : 1f);

            bool show = fainted || collapse > 0f;
            faintOverlay.SetActive(show);
            if (!show) return;
            float shade = Mathf.SmoothStep(0f, 1f, collapse) * (waking ? 0.35f : 0.6f);
            faintShade.color = new Color(0.02f, 0.03f, 0.04f, shade);
            faintMessage.text = waking ? "Waking up..."
                : $"FAINTED\n\n{faintLoss.Count} carried {(faintLoss.Count == 1 ? "entry" : "entries")} lost\n\nRecovering... {stamina.Hearts:F2} / {stamina.Maximum:F0} hearts";
            for (int i = lossSlips.Count - 1; i >= 0; i--)
            {
                var slip = lossSlips[i];
                float t = (Time.time - slip.start) / LossSlipSeconds;
                if (t < 0f) { slip.image.color = new Color(slip.image.color.r, slip.image.color.g, slip.image.color.b, 0f); slip.label.color = Color.clear; continue; }
                if (t >= 1f) { Destroy(slip.rect.gameObject); lossSlips.RemoveAt(i); continue; }
                // Slips out of the bag downwards with a small tumble and fades away; never a collectible world drop.
                slip.rect.anchoredPosition = new Vector2(Mathf.Sin(t * 3f + i) * 18f, -t * t * 260f);
                slip.rect.localRotation = Quaternion.Euler(0, 0, (i % 2 == 0 ? 1 : -1) * t * 70f);
                float alpha = t < 0.15f ? t / 0.15f : 1f - Mathf.InverseLerp(0.45f, 1f, t);
                slip.image.color = new Color(slip.image.color.r, slip.image.color.g, slip.image.color.b, alpha);
                slip.label.color = new Color(1f, 0.92f, 0.8f, alpha);
            }
        }

        private void FinishWake()
        {
            waking = false; animateFall = false; ClearLossSlips();
            router.InteractionLocked = false;
            if (!saveBlocked) Say("Recovered. Swipe UP on the centre line to resume.");
        }

        private void ClearLossSlips()
        {
            foreach (var slip in lossSlips) if (slip.rect != null) Destroy(slip.rect.gameObject);
            lossSlips.Clear();
        }

        // The entries a committed plan will remove, captured before the commit so their art can be shown.
        private List<(ItemDefinition, int)> CaptureLoss(SpatialInventory.LossPlan plan)
        {
            var lost = new List<(ItemDefinition, int)>();
            foreach (var entry in plan.Entries)
                foreach (ItemEntry item in ledger.Entries)
                    if (item.Id == entry.Id) { lost.Add((item.Definition, entry.Quantity)); break; }
            return lost;
        }
    }
}
