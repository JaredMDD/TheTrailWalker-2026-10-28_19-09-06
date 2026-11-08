using System;
using System.Collections;
using System.Collections.Generic;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Harvest;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 6: tree chopping, hunting and fishing on the current trail segment (plan sections 7.1-7.3, 8.2-8.2.3).
    // An interaction starts by touching the interactable's model or the animated tool icon above it. It focuses the
    // camera, stops walking (the harvest stop), replaces the bag button with RETURN and shows the tool in use with its
    // remaining uses. Every committed action spends one use. Completion rolls and saves the reward manifest once, then
    // the rewards appear beside the road a few at a time. Each interactable can be harvested once per drop cycle.
    public sealed partial class CollectionPrototype
    {
        private sealed class HarvestTarget
        {
            public string AnchorId; public InteractableKind Kind; public Transform Root, HintAnchor;
            public Tier1PineRig Pine; public Tier1RabbitRig Rabbit;
            public float Distance, Lateral; public Vector3 Home, RoadPoint; public Quaternion HomeTurn, FallBase;
            public RectTransform Hint; public bool Harvested, SpawnReady;
            public Vector3 Wander, DropCentre; public float NextWander, Boost, Phase;
            public AnimalWanderer Motion;   // animals: graze, look around and hop about the spawn point
        }
        private sealed class CutLine { public float Y; public int Segments; public RectTransform View, Fill; }

        private readonly List<HarvestTarget> harvestTargets = new List<HarvestTarget>();
        private readonly HashSet<string> harvestedKeys = new HashSet<string>();
        private bool harvestReady;
        private float nextHarvestSpawn;
        private const int VisiblePerAnchor = 6;
        private const float HarvestReach = 30f;

        // Active interaction.
        private HarvestTarget active;
        private ItemEntry activeTool; private ToolSpec activeSpec;
        private int actionsNeeded, progress;
        private float savedYaw;
        private readonly List<GameObject> hiddenHud = new List<GameObject>();
        private readonly List<CutLine> cutLines = new List<CutLine>();
        private RectTransform harvestRoot, harvestInput, toolPicture, heartRoot, projectile, bobber, cutLayer;
        private Image heartFill, stripIcon;
        private Text stripText;
        private readonly List<RectTransform> aimDots = new List<RectTransform>();
        private int swipePointer = int.MinValue; private float swipeMinX, swipeMaxX, swipeSumY; private int swipeSamples;
        private Vector2 drawStart; private bool drawing, flying, cast; private float drawAmount, nextTap;
        // Drawn slingshot band (two strands and a leather pouch) and fishing line.
        private UiPolyline bandLeft, bandRight, fishLine;
        private RectTransform pouch;
        private Vector2 lastDrag, releasePoint, snapFrom;
        private float snapStart = -1f, lineSlack = 1f;
        private bool pulling;
        // Points on the tool pictures (fraction of the picture, from bottom-left): slingshot fork tips and rod tip.
        private static readonly Vector2 ForkLeft = new Vector2(0.08f, 0.96f), ForkRight = new Vector2(0.92f, 0.96f), RodTip = new Vector2(0.39f, 0.99f);

        private float CanvasScale => safe != null && safe.GetComponentInParent<Canvas>() != null ? safe.GetComponentInParent<Canvas>().scaleFactor : 1f;
        private FollowCameraRig CameraRig => cameraView != null ? cameraView.GetComponent<FollowCameraRig>() : null;
        private int RegionTier => SegmentLayout.Tier(scene.Catalog.RegionOf(scene.Segment));
        private int TargetTier => RegionTier;   // the TARGET T1-T4 debug override went with the debug cleanup (November 7, 2026)

        // ---------------------------------------------------------------- setup

        private void TryInitHarvest()
        {
            if (harvestReady || !milestone5 || !initialized || scene == null || travel == null || scene.Segment == null) return;
            harvestReady = true;
            string segmentId = scene.Segment.Id; int cycle = travel.CycleOf(segmentId);
            RegionDefinition region = scene.Catalog.RegionOf(scene.Segment);
            IReadOnlyList<InteractableAnchor> anchors = SegmentLayout.Build(region, scene.Segment);
            foreach (Tier1HarvestVisual visual in FindObjectsByType<Tier1HarvestVisual>(FindObjectsSortMode.None))
            {
                // Only the current leg's interactables (the preview and walked legs are scenery).
                if (visual.transform.parent == null || visual.transform.parent.name != "Interactables") continue;
                string baseId = visual.AnchorId; int marker = baseId.IndexOf(".animal", StringComparison.Ordinal);
                if (marker >= 0) baseId = baseId.Substring(0, marker);
                InteractableAnchor anchor = default; bool found = false;
                foreach (InteractableAnchor a in anchors) if (a.Id == baseId) { anchor = a; found = true; break; }
                if (!found) continue;
                path.Evaluate(Mathf.Clamp(anchor.Distance, 0, path.Length), out Vector3 road, out _);
                var target = new HarvestTarget
                {
                    AnchorId = visual.AnchorId, Kind = anchor.Kind, Root = visual.transform, Distance = anchor.Distance, Lateral = anchor.Lateral,
                    Home = visual.transform.position, HomeTurn = visual.transform.rotation, RoadPoint = road,
                    Pine = visual.GetComponent<Tier1PineRig>(), Rabbit = visual.GetComponent<Tier1RabbitRig>(), Phase = UnityEngine.Random.value * 10f
                };
                target.HintAnchor = visual.HintAnchor != null ? visual.HintAnchor : target.Pine != null && target.Pine.HintAnchor != null ? target.Pine.HintAnchor
                    : target.Rabbit != null && target.Rabbit.HintAnchor != null ? target.Rabbit.HintAnchor : visual.transform;
                if (target.Pine != null && target.Pine.FallPivot != null) target.FallBase = target.Pine.FallPivot.rotation;
                if (target.Rabbit != null && target.Rabbit.MotionRoot != null && target.Rabbit.HeadPivot != null)
                    target.Motion = new AnimalWanderer(new AnimalAnimator(target.Rabbit.Parts()), target.Home, target.AnchorId.GetHashCode());
                target.Wander = target.Home;
                target.Harvested = harvestedKeys.Contains(HarvestManifest.HarvestKey(segmentId, cycle, target.AnchorId));
                target.SpawnReady = target.Harvested; target.DropCentre = target.Home;
                // The art prefabs ship their selection collider disabled ("no input"); live interactables need it for
                // touches on the model. Harvested ones switch it off again in ShowHarvested.
                if (visual.SelectionBounds != null) visual.SelectionBounds.enabled = true;
                foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = true;
                if (target.Harvested) ShowHarvested(target);
                target.Hint = CreateHint(target);
                harvestTargets.Add(target);
            }
            CreateHarvestOverlay();
        }

        private void ShowHarvested(HarvestTarget target)
        {
            foreach (Collider collider in target.Root.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            if (target.Kind == InteractableKind.Tree && target.Pine != null && target.Pine.FallPivot != null) target.Pine.FallPivot.rotation = FallRotation(target, 85f);
            else if (target.Kind == InteractableKind.AnimalSpawn) target.Root.gameObject.SetActive(false);
        }

        private Quaternion FallRotation(HarvestTarget target, float degrees)
        {
            Vector3 away = Vector3.ProjectOnPlane(target.Home - target.RoadPoint, Vector3.up).normalized;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
            return Quaternion.AngleAxis(degrees, Vector3.Cross(Vector3.up, away)) * target.FallBase;
        }

        private static ItemDefinition ToolModel(ToolFamily family) => Array.Find(PrototypeItems.All, d => d.Id == HarvestTools.ModelId(family));

        private RectTransform CreateHint(HarvestTarget target)
        {
            RectTransform hint = Rect("Harvest hint " + target.AnchorId, safe, 0, 0, 0, 0);
            hint.sizeDelta = new Vector2(78f, 78f);
            Image image = hint.gameObject.AddComponent<Image>(); image.preserveAspect = true;
            Sprite icon = ItemIconRenderer.For(ToolModel(HarvestRules.FamilyFor(target.Kind)));
            if (icon != null) image.sprite = icon; else image.color = new Color(1f, 0.85f, 0.4f);
            var button = hint.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => TryStartHarvest(target));
            var shadow = hint.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(0, 0, 0, 0.6f); shadow.effectDistance = new Vector2(2, -2);
            hint.gameObject.SetActive(false);
            return hint;
        }

        private void CreateHarvestOverlay()
        {
            harvestRoot = Rect("Harvest overlay", safe, 0, 0, 1, 1);
            cutLayer = Rect("Cut lines", harvestRoot, 0, 0, 1, 1);
            // The input surface stays well above the bottom system-gesture/navigation area.
            harvestInput = Rect("Harvest input", harvestRoot, 0, 0.10f, 1, 0.90f);
            Image input = harvestInput.gameObject.AddComponent<Image>(); input.color = new Color(0, 0, 0, 0);
            harvestInput.gameObject.AddComponent<HarvestInputSurface>().Owner = this;

            RectTransform strip = Rect("Tool strip", harvestRoot, 0.02f, 0.915f, 0.98f, 0.99f);
            Image stripBack = strip.gameObject.AddComponent<Image>(); stripBack.sprite = DurabilityStyle.RoundedSprite; stripBack.type = Image.Type.Sliced; stripBack.color = DurabilityStyle.Badge; stripBack.raycastTarget = false;
            RectTransform iconRect = Rect("Tool", strip, 0.01f, 0.05f, 0.15f, 0.95f);
            stripIcon = iconRect.gameObject.AddComponent<Image>(); stripIcon.preserveAspect = true; stripIcon.raycastTarget = false;
            stripText = Label(strip, "", 0.17f, 0.04f, 0.99f, 0.96f, 20); stripText.alignment = TextAnchor.MiddleLeft; stripText.color = DurabilityStyle.Text;
            stripText.resizeTextForBestFit = true; stripText.resizeTextMinSize = 11; stripText.resizeTextMaxSize = 20;

            // Return icon in the closed bag button's place, as in every menu (user, November 5, 2026).
            PlaceSquare(ReturnSquare(harvestRoot, CancelHarvest), false, ClosedBarLine, 0, 0);

            toolPicture = Rect("Tool in hand", harvestRoot, 0.5f, 0.2f, 0.5f, 0.2f);
            toolPicture.sizeDelta = new Vector2(150f, 150f);
            Image tool = toolPicture.gameObject.AddComponent<Image>(); tool.preserveAspect = true; tool.raycastTarget = false;
            bandLeft = Line("Band left", new Color(0.36f, 0.2f, 0.12f)); bandRight = Line("Band right", new Color(0.36f, 0.2f, 0.12f));
            fishLine = Line("Fishing line", new Color(0.96f, 0.93f, 0.82f, 0.95f));
            pouch = Rect("Pouch", harvestRoot, 0, 0, 0, 0); pouch.sizeDelta = new Vector2(36f, 22f);
            Image pouchImage = pouch.gameObject.AddComponent<Image>(); pouchImage.sprite = DurabilityStyle.RoundedSprite; pouchImage.type = Image.Type.Sliced;
            pouchImage.color = new Color(0.62f, 0.42f, 0.24f); pouchImage.raycastTarget = false;
            RectTransform stone = Rect("Stone", pouch, 0.28f, 0.2f, 0.72f, 0.95f);
            Image stoneImage = stone.gameObject.AddComponent<Image>(); stoneImage.sprite = DurabilityStyle.RoundedSprite; stoneImage.color = new Color(0.45f, 0.42f, 0.38f); stoneImage.raycastTarget = false;
            for (int i = 0; i < 10; i++)
            {
                RectTransform dot = Rect("Aim", harvestRoot, 0, 0, 0, 0); dot.sizeDelta = new Vector2(12f, 12f);
                Image dotImage = dot.gameObject.AddComponent<Image>(); dotImage.sprite = DurabilityStyle.RoundedSprite; dotImage.color = new Color(1f, 1f, 1f, 0.8f); dotImage.raycastTarget = false;
                dot.gameObject.SetActive(false); aimDots.Add(dot);
            }
            projectile = Dot("Stone", 22f, new Color(0.45f, 0.42f, 0.38f));
            bobber = Dot("Bobber", 26f, new Color(0.9f, 0.2f, 0.18f));
            heartRoot = Rect("Animal heart", harvestRoot, 0, 0, 0, 0); heartRoot.sizeDelta = new Vector2(64f, 58f);
            Image heartBack = heartRoot.gameObject.AddComponent<Image>(); heartBack.sprite = HeartSprite; heartBack.color = new Color(0.12f, 0.12f, 0.12f, 0.85f); heartBack.raycastTarget = false;
            RectTransform fill = Rect("Fill", heartRoot, 0, 0, 1, 1);
            heartFill = fill.gameObject.AddComponent<Image>(); heartFill.sprite = HeartSprite; heartFill.color = new Color(0.9f, 0.16f, 0.2f); heartFill.raycastTarget = false;
            heartFill.type = Image.Type.Filled; heartFill.fillMethod = Image.FillMethod.Horizontal; heartFill.fillOrigin = 0;
            harvestRoot.gameObject.SetActive(false);
        }

        private UiPolyline Line(string name, Color color)
        {
            RectTransform rect = Rect(name, harvestRoot, 0, 0, 1, 1);
            rect.gameObject.AddComponent<CanvasRenderer>(); UiPolyline line = rect.gameObject.AddComponent<UiPolyline>(); line.color = color; line.raycastTarget = false;
            rect.gameObject.SetActive(false); return line;
        }

        // A point on the tool picture (fraction from its bottom-left), following its offset and rotation.
        private Vector2 ToolPoint(Vector2 fraction)
        {
            Sprite sprite = toolPicture.GetComponent<Image>().sprite;
            float side = Mathf.Min(toolPicture.rect.width, toolPicture.rect.height);
            float aspect = sprite != null ? sprite.rect.width / sprite.rect.height : 1f;
            float w = aspect < 1f ? side * aspect : side, h = aspect < 1f ? side : side / aspect;
            return toolPicture.TransformPoint(new Vector3((fraction.x - 0.5f) * w, (fraction.y - 0.5f) * h, 0f));
        }

        private RectTransform Dot(string name, float size, Color color)
        {
            RectTransform dot = Rect(name, harvestRoot, 0, 0, 0, 0); dot.sizeDelta = new Vector2(size, size);
            Image image = dot.gameObject.AddComponent<Image>(); image.sprite = DurabilityStyle.RoundedSprite; image.color = color; image.raycastTarget = false;
            dot.gameObject.SetActive(false); return dot;
        }

        // ---------------------------------------------------------------- per frame

        private void UpdateHarvest()
        {
            TryInitHarvest();
            if (!harvestReady) return;
            bool hintsAllowed = active == null && !inventoryOpen && !AtCamp && !TravelInputLocked && !saveBlocked && !(milestone4 && stamina.Fainted);
            float t = Time.time;
            foreach (HarvestTarget target in harvestTargets)
            {
                if (target.Kind == InteractableKind.AnimalSpawn && !target.Harvested && target.Motion != null && target != active) target.Motion.UpdateIdle(target.Root, Time.deltaTime, t, AnimalGround);
                if (target.Hint == null) continue;
                bool show = hintsAllowed && !target.Harvested && Vector3.Distance(motor.transform.position, target.Root.position) <= HarvestReach;
                Vector3 screen = Vector3.zero;
                if (show)
                {
                    screen = cameraView.WorldToScreenPoint(target.HintAnchor.position + Vector3.up * 0.35f);
                    show = screen.z > 0 && screen.x > 0 && screen.x < Screen.width && screen.y > Screen.height * 0.12f && screen.y < Screen.height * 0.86f;
                }
                if (target.Hint.gameObject.activeSelf != show) target.Hint.gameObject.SetActive(show);
                if (!show) continue;
                // Animated tool silhouette: the axe chops, the slingshot draws, the rod sways.
                float wave = Mathf.Sin(t * 4f + target.Phase);
                target.Hint.position = new Vector3(screen.x, screen.y + (target.Kind == InteractableKind.AnimalSpawn ? Mathf.Abs(wave) * 8f : 0f), 0);
                target.Hint.localRotation = Quaternion.Euler(0, 0, target.Kind == InteractableKind.Tree ? wave * 28f : target.Kind == InteractableKind.FishingPoint ? wave * 12f : 0f);
                target.Hint.localScale = Vector3.one * (target.Kind == InteractableKind.AnimalSpawn ? 1f + 0.08f * wave : 1f);
            }
            if (active != null) UpdateActiveHarvest();
            if (Time.unscaledTime >= nextHarvestSpawn) { nextHarvestSpawn = Time.unscaledTime + 0.25f; SpawnPendingHarvest(); }
        }

        // Animals stand on the drawn terrain while they move about (the spawn point itself was seated in DressSegment).
        private float AnimalGround(Vector3 at)
        {
            if (world == null) world = GetComponent<MovementPrototype>();
            return world != null && world.Ground != null && world.Ground.TryLowest(at, 0.2f, out float ground) ? ground : at.y;
        }

        // ---------------------------------------------------------------- starting and ending

        // A touch on an interactable's model (its selection collider, trigger or not, anywhere along the ray) starts it.
        private readonly RaycastHit[] harvestHits = new RaycastHit[16];
        private bool TryStartHarvestAt(Vector2 screen)
        {
            if (!harvestReady) return false;
            int count = Physics.RaycastNonAlloc(cameraView.ScreenPointToRay(screen), harvestHits, cameraView.farClipPlane, ~0, QueryTriggerInteraction.Collide);
            HarvestTarget best = null; float nearest = float.MaxValue;
            for (int i = 0; i < count; i++)
                foreach (HarvestTarget target in harvestTargets)
                    if (!target.Harvested && harvestHits[i].transform.IsChildOf(target.Root) && harvestHits[i].distance < nearest) { best = target; nearest = harvestHits[i].distance; }
            if (best == null) return false;
            // A pickup in front of the interactable takes the touch instead.
            for (int i = 0; i < count; i++)
                if (harvestHits[i].distance < nearest && bundles.Exists(x => x.Collider == harvestHits[i].collider && x.Item.Owner == ItemOwner.World)) return false;
            TryStartHarvest(best);
            return true;
        }

        private void TryStartHarvest(HarvestTarget target)
        {
            if (active != null || target.Harvested || saveBlocked || (milestone4 && stamina.Fainted) || TravelInputLocked) return;
            if (AtCamp) { Say("Stand up and walk on to harvest."); return; }
            if (inventoryOpen) { Say("Close the bag to harvest."); return; }
            if (Vector3.Distance(motor.transform.position, target.Root.position) > HarvestReach) { Say("Walk closer to harvest."); return; }
            ToolFamily family = HarvestRules.FamilyFor(target.Kind);
            if (!FindTool(family, out ItemEntry tool, out ToolSpec spec))
            {
                string name = family == ToolFamily.Axe ? "an axe" : family == ToolFamily.Hunting ? "a slingshot or bow" : "a fishing rod";
                Say($"Needs {name} of tier {Mathf.Max(1, TargetTier - 2)} or higher, with uses left, in the bag or on a hook.");
                return;
            }
            active = target; activeTool = tool; activeSpec = spec;
            actionsNeeded = HarvestRules.Actions(spec, TargetTier).Value; progress = 0;
            drawing = flying = cast = false; swipePointer = int.MinValue; nextTap = 0f;
            router.CancelAll(); CancelAll(); movement.BeginHarvest(); savedYaw = orbit.Yaw;
            Vector3 away = Vector3.ProjectOnPlane(target.Home - target.RoadPoint, Vector3.up).normalized;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
            Vector3 eye, look;
            if (target.Kind == InteractableKind.Tree) { eye = target.Home - away * 3.4f + Vector3.up * 1.25f; look = target.Home + Vector3.up * 0.95f; }
            else if (target.Kind == InteractableKind.AnimalSpawn) { eye = target.Home - away * 3.0f + Vector3.up * 1.3f; look = target.Home + Vector3.up * 0.15f; }
            else { eye = target.Home - away * 4.2f + Vector3.up * 2.4f; look = target.Home; }
            CameraRig?.Focus(eye, look);
            hiddenHud.Clear();
            foreach (Transform child in safe)
                if (child != harvestRoot && child.gameObject.activeSelf) { hiddenHud.Add(child.gameObject); child.gameObject.SetActive(false); }
            harvestRoot.gameObject.SetActive(true); harvestRoot.SetAsLastSibling();
            bool shooting = target.Kind != InteractableKind.Tree;
            toolPicture.gameObject.SetActive(shooting);
            if (shooting) { toolPicture.GetComponent<Image>().sprite = ItemIconRenderer.For(ToolModel(spec.Family)); toolPicture.localRotation = Quaternion.identity; }
            heartRoot.gameObject.SetActive(target.Kind == InteractableKind.AnimalSpawn);
            heartFill.fillAmount = 1f;
            bobber.gameObject.SetActive(false); projectile.gameObject.SetActive(false); SetAimDots(0, Vector2.zero, Vector2.zero);
            bool hunting = target.Kind == InteractableKind.AnimalSpawn;
            bandLeft.gameObject.SetActive(hunting); bandRight.gameObject.SetActive(hunting); pouch.gameObject.SetActive(hunting);
            fishLine.gameObject.SetActive(false); snapStart = -1f; lineSlack = 1f; pulling = false;
            RefreshStrip();
        }

        private void CancelHarvest()
        {
            if (active == null) return;
            if (active.Kind == InteractableKind.AnimalSpawn) { active.Root.position = active.Home; active.Root.rotation = active.HomeTurn; active.Motion?.Stop(); }
            EndInteraction();
            Say("Harvest cancelled. Uses already spent are not refunded. Swipe UP on the centre line to walk on.");
        }

        private void EndInteraction()
        {
            foreach (CutLine line in cutLines) Destroy(line.View.gameObject);
            cutLines.Clear();
            harvestRoot.gameObject.SetActive(false);
            foreach (GameObject hidden in hiddenHud) if (hidden != null) hidden.SetActive(true);
            hiddenHud.Clear();
            CameraRig?.ClearFocus(); orbit.Restore(savedYaw);
            movement.EndHarvest(); // the harvest stop stays until a fresh upward speed gesture
            active = null; activeTool = null; drawing = flying = false;
        }

        // Eligible carried tool with the fewest remaining uses (ties: fewest actions, Elite, lower tier).
        private bool FindTool(ToolFamily family, out ItemEntry best, out ToolSpec bestSpec)
        {
            best = null; bestSpec = default; int tier = TargetTier;
            foreach (ItemEntry entry in ledger.Entries)
            {
                if ((entry.Owner != ItemOwner.Backpack && entry.Owner != ItemOwner.Hook) || !entry.Ready || entry.Profile.MaxDurability <= 0 || entry.Profile.Durability <= 0) continue;
                if (!HarvestTools.TryGet(entry.Definition, out ToolSpec spec) || spec.Family != family || !HarvestRules.Actions(spec, tier).HasValue) continue;
                if (best == null || HarvestRules.Compare(spec, entry.Profile.Durability, bestSpec, best.Profile.Durability, tier) < 0) { best = entry; bestSpec = spec; }
            }
            return best != null;
        }

        private void RefreshStrip()
        {
            if (activeTool == null) return;
            stripIcon.sprite = ItemIconRenderer.For(ItemArt.ModelFor(activeTool.Definition));
            int uses = activeTool.Owner == ItemOwner.Consumed ? 0 : activeTool.Profile.Durability;
            string step = active.Kind == InteractableKind.Tree ? $"Cut {(cutLines.Count > 0 ? cutLines[cutLines.Count - 1].Segments : 0)}/{actionsNeeded}: swipe across the lower trunk"
                : active.Kind == InteractableKind.AnimalSpawn ? $"Hits {progress}/{actionsNeeded}: drag down, aim, release"
                : cast ? $"Reel {progress}/{actionsNeeded}: tap the rod" : "Drag the rod down and release to cast";
            stripText.text = $"{activeTool.Definition.Name}  {uses} uses left\n{step}";
        }

        // One committed action: spend a use, then (after the action resolves) handle a broken tool.
        private bool SpendUse(out bool broke)
        {
            broke = false;
            if (activeTool == null || !ledger.TryUseTool(activeTool, out broke)) { if (!SwitchTool(false)) return false; return ledger.TryUseTool(activeTool, out broke); }
            return true;
        }

        // After a tool breaks: continue with another valid carried tool (with a warning) or end the interaction.
        private bool SwitchTool(bool warnBroken)
        {
            string old = activeTool != null ? activeTool.Definition.Name : "tool";
            if (!FindTool(activeSpec.Family, out ItemEntry tool, out ToolSpec spec))
            {
                if (active.Kind == InteractableKind.AnimalSpawn) { active.Root.position = active.Home; active.Root.rotation = active.HomeTurn; active.Motion?.Stop(); }
                EndInteraction();
                Say($"Your {old} broke and no other eligible tool is carried. Progress was lost.");
                return false;
            }
            activeTool = tool; activeSpec = spec; actionsNeeded = HarvestRules.Actions(spec, TargetTier).Value;
            if (warnBroken) Say($"Your {old} broke. Continuing with {tool.Definition.Name}.");
            RefreshStrip();
            return true;
        }

        private void AfterAction(bool broke, bool complete)
        {
            if (complete) { CompleteHarvest(); return; }
            if (broke) SwitchTool(true); else RefreshStrip();
        }

        // ---------------------------------------------------------------- input

        public void HarvestDown(int pointer, Vector2 position)
        {
            if (active == null || flying) return;
            if (active.Kind == InteractableKind.Tree)
            {
                swipePointer = pointer; swipeMinX = swipeMaxX = position.x; swipeSumY = position.y; swipeSamples = 1;
                return;
            }
            if (active.Kind == InteractableKind.FishingPoint && cast)
            {
                // Reeling: taps on the rod, debounced to one per line-tensing animation.
                if (RectTransformUtility.RectangleContainsScreenPoint(toolPicture, position) || Vector2.Distance(position, toolPicture.position) < Screen.height * 0.12f) Reel();
                return;
            }
            if (position.y > Screen.height * 0.55f) return;
            drawing = true; drawStart = lastDrag = position; drawAmount = 0f; swipePointer = pointer; snapStart = -1f;
        }

        public void HarvestDrag(int pointer, Vector2 position)
        {
            if (active == null || pointer != swipePointer) return;
            if (active.Kind == InteractableKind.Tree)
            {
                swipeMinX = Mathf.Min(swipeMinX, position.x); swipeMaxX = Mathf.Max(swipeMaxX, position.x); swipeSumY += position.y; swipeSamples++;
                return;
            }
            if (!drawing) return;
            lastDrag = position;
            Vector2 pull = drawStart - position;
            drawAmount = Mathf.Clamp01(pull.magnitude / (Screen.height * 0.25f));
            toolPicture.anchoredPosition = -pull.normalized * drawAmount * 18f;
            if (active.Kind == InteractableKind.AnimalSpawn && drawAmount > 0.05f) SetAimDots(drawAmount, ToolCentre, pull.normalized);
        }

        public void HarvestUp(int pointer, Vector2 position)
        {
            if (active == null || pointer != swipePointer) return;
            swipePointer = int.MinValue;
            if (active.Kind == InteractableKind.Tree) { EvaluateCut(); return; }
            if (!drawing) return;
            releasePoint = PouchPosition();
            drawing = false; toolPicture.anchoredPosition = Vector2.zero; SetAimDots(0, Vector2.zero, Vector2.zero);
            snapFrom = releasePoint; snapStart = Time.time;
            Vector2 pull = drawStart - position;
            if (drawAmount < 0.25f) { Say(active.Kind == InteractableKind.FishingPoint ? "Pull the rod further down to cast." : "Draw further to shoot; that shot was cancelled."); return; }
            if (active.Kind == InteractableKind.FishingPoint) StartCoroutine(Cast());
            else StartCoroutine(Shoot(pull.normalized, drawAmount));
        }

        private Vector2 ToolCentre => toolPicture.position;

        private void SetAimDots(float amount, Vector2 from, Vector2 direction)
        {
            int shown = Mathf.RoundToInt(amount * aimDots.Count);
            float spacing = Screen.height * 0.075f;
            for (int i = 0; i < aimDots.Count; i++)
            {
                bool on = i < shown; aimDots[i].gameObject.SetActive(on);
                if (on) aimDots[i].position = from + direction * spacing * (i + 1);
            }
        }

        // ---------------------------------------------------------------- chopping

        private UnityEngine.Rect ScreenRect(Transform root)
        {
            bool first = true; float xMin = 0, xMax = 0, yMin = 0, yMax = 0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                Bounds b = renderer.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 s = cameraView.WorldToScreenPoint(corner);
                    if (s.z <= 0) continue;
                    if (first) { xMin = xMax = s.x; yMin = yMax = s.y; first = false; }
                    else { xMin = Mathf.Min(xMin, s.x); xMax = Mathf.Max(xMax, s.x); yMin = Mathf.Min(yMin, s.y); yMax = Mathf.Max(yMax, s.y); }
                }
            }
            return first ? new UnityEngine.Rect(Screen.width * 0.4f, Screen.height * 0.3f, Screen.width * 0.2f, Screen.height * 0.4f) : UnityEngine.Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private UnityEngine.Rect TrunkRect => ScreenRect(active.Pine != null && active.Pine.LowerTrunk != null ? active.Pine.LowerTrunk : active.Root);
        private float CutZoneBottom => Screen.safeArea.yMin + Screen.safeArea.height * 0.12f;

        private void EvaluateCut()
        {
            UnityEngine.Rect trunk = TrunkRect; float y = swipeSumY / Mathf.Max(1, swipeSamples);
            float overlap = Mathf.Min(swipeMaxX, trunk.xMax) - Mathf.Max(swipeMinX, trunk.xMin);
            // Lower part of the trunk, always above the system-gesture area; at least 70% of the trunk's width.
            float zoneTop = trunk.yMin + trunk.height * 0.55f, zoneBottom = Mathf.Max(trunk.yMin, CutZoneBottom);
            if (overlap < trunk.width * 0.7f || y < zoneBottom || y > zoneTop) { if (swipeMaxX - swipeMinX > Screen.width * 0.08f) Say("Swipe across most of the lower trunk."); return; }
            if (!SpendUse(out bool broke)) return;
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
            float tolerance = Mathf.Max(trunk.height * 0.06f, 48f * dpi / 160f);
            float relative = (y - trunk.yMin) / Mathf.Max(1f, trunk.height);
            CutLine last = cutLines.Count > 0 ? cutLines[cutLines.Count - 1] : null;
            if (last == null || Mathf.Abs((last.Y - relative) * trunk.height) > tolerance)
            {
                if (last != null) Say("That cut was off the line: a new cut line started.");
                last = new CutLine { Y = relative, Segments = 0 }; MakeCutView(last); cutLines.Add(last);
            }
            last.Segments++;
            AfterAction(broke, last.Segments >= actionsNeeded);
        }

        private void MakeCutView(CutLine line)
        {
            line.View = Rect("Cut line", cutLayer, 0, 0, 0, 0);
            Image back = line.View.gameObject.AddComponent<Image>(); back.color = new Color(0.12f, 0.08f, 0.05f, 0.75f); back.raycastTarget = false;
            line.Fill = Rect("Cut", line.View, 0, 0, 0, 1);
            Image fill = line.Fill.gameObject.AddComponent<Image>(); fill.color = new Color(0.98f, 0.86f, 0.55f); fill.raycastTarget = false;
        }

        private void UpdateActiveHarvest()
        {
            float scale = CanvasScale;
            if (active.Kind == InteractableKind.Tree)
            {
                UnityEngine.Rect trunk = TrunkRect;
                foreach (CutLine line in cutLines)
                {
                    line.View.position = new Vector3(trunk.center.x, trunk.yMin + line.Y * trunk.height, 0);
                    line.View.sizeDelta = new Vector2(trunk.width * 1.1f / scale, 9f);
                    line.Fill.anchorMax = new Vector2(Mathf.Clamp01((float)line.Segments / actionsNeeded), 1);
                }
                return;
            }
            if (active.Kind == InteractableKind.AnimalSpawn) { MoveHuntedAnimal(); UnityEngine.Rect body = ScreenRect(active.Root); heartRoot.position = new Vector3(body.center.x, body.yMax + 46f, 0); DrawBand(); return; }
            if (cast && !pulling) bobber.position = cameraView.WorldToScreenPoint(active.Home + Vector3.up * 0.05f);
            DrawFishingLine();
        }

        // ---------------------------------------------------------------- drawn band and line

        // Pouch rest point between the fork tips; while drawing it follows the finger (limited to the full draw); after
        // a release it snaps back with a small overshoot.
        private Vector2 PouchRest => Vector2.Lerp(ToolPoint(ForkLeft), ToolPoint(ForkRight), 0.5f) + Vector2.down * Screen.height * 0.012f;
        private Vector2 PouchPosition()
        {
            Vector2 rest = PouchRest;
            if (drawing) return rest + Vector2.ClampMagnitude(lastDrag - drawStart, Screen.height * 0.25f);
            if (snapStart >= 0f)
            {
                float t = (Time.time - snapStart) / 0.16f;
                if (t >= 1f) { snapStart = -1f; return rest; }
                float overshoot = Mathf.Sin(t * Mathf.PI) * 0.18f;
                return Vector2.LerpUnclamped(snapFrom, rest, t + overshoot);
            }
            return rest;
        }

        private void DrawBand()
        {
            Vector2 left = ToolPoint(ForkLeft), right = ToolPoint(ForkRight), at = PouchPosition();
            float stretch = Mathf.Clamp01(Vector2.Distance(at, PouchRest) / (Screen.height * 0.25f));
            float width = Mathf.Lerp(8f, 4f, stretch);
            Color rubber = Color.Lerp(new Color(0.42f, 0.24f, 0.14f), new Color(0.26f, 0.13f, 0.08f), stretch);
            bandLeft.color = bandRight.color = rubber;
            bandLeft.SetScreenPoints(new[] { left, at + Vector2.left * 12f }, width);
            bandRight.SetScreenPoints(new[] { right, at + Vector2.right * 12f }, width);
            pouch.position = at;
            Vector2 toward = (left + right) * 0.5f - at;
            pouch.localRotation = Quaternion.Euler(0, 0, toward.sqrMagnitude > 1f ? Mathf.Atan2(toward.y, toward.x) * Mathf.Rad2Deg - 90f : 0f);
            // The stone is in the pouch until it is shot.
            pouch.GetChild(0).gameObject.SetActive(!flying);
        }

        // Rod tip to bobber, sagging while slack; reel taps tighten it briefly; the final pull lifts the catch out.
        private void DrawFishingLine()
        {
            bool show = active != null && active.Kind == InteractableKind.FishingPoint && bobber.gameObject.activeSelf;
            if (fishLine.gameObject.activeSelf != show) fishLine.gameObject.SetActive(show);
            if (!show) return;
            Vector2 tip = ToolPoint(RodTip), end = bobber.position;
            // The slack bows perpendicular to the line, toward the lower side (or to the right when the line is
            // vertical on screen), so it reads as a loose line whatever the camera angle.
            Vector2 along = (end - tip).normalized, bow = new Vector2(-along.y, along.x);
            if (bow.y > 0.05f || (Mathf.Abs(bow.y) <= 0.05f && bow.x < 0f)) bow = -bow;
            float sag = Vector2.Distance(tip, end) * 0.13f * lineSlack;
            Vector2 middle = (tip + end) * 0.5f + bow * sag;
            var points = new Vector2[18];
            for (int i = 0; i < points.Length; i++)
            {
                float t = i / (points.Length - 1f);
                points[i] = (1 - t) * (1 - t) * tip + 2 * (1 - t) * t * middle + t * t * end;
            }
            fishLine.SetScreenPoints(points, Mathf.Lerp(2.6f, 2f, 1f - lineSlack));
        }

        // ---------------------------------------------------------------- hunting

        // The animal keeps moving inside its encounter area and on screen; after a miss it briefly moves faster.
        private void MoveHuntedAnimal()
        {
            Transform body = active.Root; float now = Time.time;
            Vector3 screen = cameraView.WorldToViewportPoint(body.position);
            bool offScreen = screen.x < 0.15f || screen.x > 0.85f || screen.y < 0.32f || screen.y > 0.82f;
            Vector3 flat = body.position - active.Wander; flat.y = 0f;
            if (now >= active.NextWander || offScreen || flat.magnitude < 0.05f)
            {
                Vector2 jitter = UnityEngine.Random.insideUnitCircle * 1.1f;
                active.Wander = offScreen ? active.Home : active.Home + new Vector3(jitter.x, 0, jitter.y);
                active.NextWander = now + UnityEngine.Random.Range(1.0f, 2.2f) * (now < active.Boost ? 0.5f : 1f);
            }
            // Hops (or walks, for walking species) toward the point, alert; faster and bigger strides after a miss.
            if (active.Motion != null) { active.Motion.UpdateHunted(body, active.Wander, now < active.Boost, Time.deltaTime, now, AnimalGround); return; }
            float speed = now < active.Boost ? 1.7f : 0.55f;
            Vector3 step = Vector3.MoveTowards(body.position, active.Wander, speed * Time.deltaTime);
            Vector3 heading = step - body.position;
            if (heading.sqrMagnitude > 1e-6f) body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(heading.normalized), Time.deltaTime * 8f);
            body.position = step;
        }

        // A committed shot spends one use, hit or miss; the stone flies opposite to the drag, farther for longer draws.
        private IEnumerator Shoot(Vector2 direction, float amount)
        {
            if (!SpendUse(out bool broke)) yield break;
            flying = true;
            Vector2 from = releasePoint, to = from + direction * amount * Screen.height * 0.8f;
            projectile.gameObject.SetActive(true);
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.3f) { projectile.position = Vector2.Lerp(from, to, t); yield return null; }
            projectile.gameObject.SetActive(false); flying = false;
            if (active == null) yield break;
            UnityEngine.Rect body = ScreenRect(active.Root);
            float radius = Mathf.Max(Screen.height * 0.035f, Mathf.Max(body.width, body.height) * 0.6f);
            bool hit = DistanceToSegment(body.center, from, to) <= radius;
            if (hit) { progress++; heartFill.fillAmount = 1f - (float)progress / actionsNeeded; Say(progress >= actionsNeeded ? "" : "Hit!"); }
            else { active.Boost = Time.time + 1.5f; active.NextWander = 0f; Say("Missed. The animal darts away but stays close."); }
            AfterAction(broke, progress >= actionsNeeded);
        }

        private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
            return Vector2.Distance(point, a + ab * t);
        }

        // ---------------------------------------------------------------- fishing

        // The cast spends no use and always lands in the water point.
        private IEnumerator Cast()
        {
            flying = true;
            Vector2 from = ToolCentre + Vector2.up * Screen.height * 0.06f;
            bobber.gameObject.SetActive(true);
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.5f)
            {
                Vector2 to = cameraView.WorldToScreenPoint(active.Home);
                bobber.position = Vector2.Lerp(from, to, t) + Vector2.up * Mathf.Sin(t * Mathf.PI) * Screen.height * 0.1f;
                yield return null;
            }
            flying = false; cast = true; RefreshStrip();
        }

        private void Reel()
        {
            if (Time.unscaledTime < nextTap || flying) return;
            nextTap = Time.unscaledTime + 0.45f;
            if (!SpendUse(out bool broke)) return;
            progress++;
            if (progress >= actionsNeeded) { StartCoroutine(PullCatch()); return; }
            StartCoroutine(TenseLine(false));
            AfterAction(broke, false);
        }

        private IEnumerator TenseLine(bool final)
        {
            RectTransform rod = toolPicture;
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.35f)
            {
                if (rod == null || !rod.gameObject.activeInHierarchy) yield break;
                float pulse = Mathf.Sin(t * Mathf.PI);
                rod.localRotation = Quaternion.Euler(0, 0, (final ? 30f : 10f) * pulse);
                bobber.localScale = Vector3.one * (1f - 0.35f * pulse);
                lineSlack = 1f - 0.9f * pulse; // the line goes taut, then slackens again
                yield return null;
            }
            if (rod != null) rod.localRotation = Quaternion.identity;
            bobber.localScale = Vector3.one; lineSlack = 1f;
        }

        // Final tap: the rod lifts, the line goes taut and the bobber comes out of the water toward the rod tip.
        private IEnumerator PullCatch()
        {
            pulling = true; flying = true;
            Vector2 from = bobber.position;
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.45f)
            {
                if (active == null) yield break;
                toolPicture.localRotation = Quaternion.Euler(0, 0, 28f * Mathf.SmoothStep(0, 1, t));
                lineSlack = 0f;
                bobber.position = Vector2.Lerp(from, ToolPoint(RodTip) + Vector2.down * Screen.height * 0.08f, t * t) + Vector2.up * Mathf.Sin(t * Mathf.PI) * Screen.height * 0.05f;
                yield return null;
            }
            pulling = false; flying = false;
            toolPicture.localRotation = Quaternion.identity;
            AfterAction(false, true);
        }

        // ---------------------------------------------------------------- completion and rewards

        private void CompleteHarvest()
        {
            HarvestTarget target = active;
            bool boosted = HarvestRules.Boosted(activeSpec, TargetTier);
            string segmentId = scene.Segment.Id; int cycle = travel.CycleOf(segmentId);
            int regionNumber = scene.Catalog.RegionOf(scene.Segment).Number;
            IReadOnlyList<HarvestBundle> manifest = HarvestManifest.Roll(target.Kind, regionNumber, segmentId, cycle, target.AnchorId, boosted);
            // Commit the manifest (world items in the save) before anything appears.
            for (int i = 0; i < manifest.Count; i++)
            {
                ItemDefinition definition = Array.Find(PrototypeItems.All, d => d.Id == manifest[i].ItemId);
                if (definition == null) continue;
                ItemEntry item = ledger.AddWorld(HarvestManifest.BundleId(segmentId, cycle, target.AnchorId, i), definition, 1000, manifest[i].Quality, TheTrailWalker.Domain.Economy.ItemPrices.Profile(definition, manifest[i].Quality), true, target.Distance);
                ledger.AssignSegment(item, segmentId);
            }
            harvestedKeys.Add(HarvestManifest.HarvestKey(segmentId, cycle, target.AnchorId));
            statistics.AddInteraction(target.Kind.ToString());
            target.Harvested = true; target.SpawnReady = false; target.DropCentre = target.Root.position;
            foreach (Collider collider in target.Root.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            if (target.Hint != null) target.Hint.gameObject.SetActive(false);
            SaveNow();
            EndInteraction();
            Say($"Harvest complete: {manifest.Count} bundles{(boosted ? " (Elite boost)" : "")}. Swipe UP on the centre line to walk on.");
            StartCoroutine(FinishPresentation(target));
        }

        private IEnumerator FinishPresentation(HarvestTarget target)
        {
            if (target.Kind == InteractableKind.Tree && target.Pine != null && target.Pine.FallPivot != null)
            {
                for (float t = 0; t < 1f; t += Time.deltaTime / 1.3f) { target.Pine.FallPivot.rotation = FallRotation(target, 85f * t * t); yield return null; }
                target.Pine.FallPivot.rotation = FallRotation(target, 85f);
            }
            else if (target.Kind == InteractableKind.AnimalSpawn)
            {
                Quaternion start = target.Root.rotation, fallen = Quaternion.AngleAxis(90f, target.Root.forward) * start;
                for (float t = 0; t < 1f; t += Time.deltaTime / 0.45f) { target.Root.rotation = Quaternion.Slerp(start, fallen, t); yield return null; }
                target.Root.rotation = fallen;
                yield return new WaitForSeconds(0.6f);
                target.Root.gameObject.SetActive(false);
            }
            target.SpawnReady = true;
            SpawnPendingHarvest();
        }

        // Up to six reward bundles per harvested interactable lie beside the road at once; the rest wait in the saved
        // queue and appear as earlier ones are picked up.
        private void SpawnPendingHarvest()
        {
            if (!harvestReady || scene == null || travel == null) return;
            string segmentId = scene.Segment.Id; int cycle = travel.CycleOf(segmentId);
            foreach (HarvestTarget target in harvestTargets)
            {
                if (!target.Harvested || !target.SpawnReady) continue;
                string prefix = HarvestManifest.BundleId(segmentId, cycle, target.AnchorId, 0); prefix = prefix.Substring(0, prefix.Length - 1);
                var occupied = new List<Vector3>(); int visible = 0;
                foreach (Bundle bundle in bundles)
                    if (bundle.Item.Owner == ItemOwner.World && bundle.Item.Id.StartsWith(prefix, StringComparison.Ordinal)) { visible++; occupied.Add(bundle.View.transform.position); }
                if (visible >= VisiblePerAnchor) continue;
                foreach (ItemEntry item in ledger.Entries)
                {
                    if (visible >= VisiblePerAnchor) break;
                    if (item.Owner != ItemOwner.World || item.SegmentId != segmentId || !item.Id.StartsWith(prefix, StringComparison.Ordinal)) continue;
                    if (bundles.Exists(b => b.Item == item)) continue;
                    Vector3 slot = FreeSlot(target, occupied);
                    AddBundle(item, slot); occupied.Add(slot); visible++;
                }
            }
        }

        // Rewards appear on the harvested interactable itself (user feedback): along the felled trunk, in a ring where the
        // animal fell, or on the pond's bank nearest the road.
        private Vector3 FreeSlot(HarvestTarget target, List<Vector3> occupied)
        {
            Vector3 away = Vector3.ProjectOnPlane(target.Home - target.RoadPoint, Vector3.up).normalized;
            if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
            Vector3 across = Vector3.Cross(Vector3.up, away);
            Vector3 centre = target.DropCentre;
            for (int s = 0; s < 18; s++)
            {
                Vector3 offset;
                if (target.Kind == InteractableKind.Tree) offset = away * (0.9f + (s / 2) * 0.85f) + across * ((s % 2 == 0 ? -1f : 1f) * 0.55f);
                else if (target.Kind == InteractableKind.AnimalSpawn) offset = Quaternion.AngleAxis(s * 60f + (s / 6) * 30f, Vector3.up) * away * (0.55f + (s / 6) * 0.5f);
                else offset = -away * 1.9f + across * ((s % 6) - 2.5f) * 0.7f - away * (s / 6) * 0.7f;
                Vector3 position = new Vector3(centre.x, target.Home.y, centre.z) + offset + Vector3.up * 0.45f;
                if (!occupied.Exists(p => Vector3.Distance(p, position) < 0.5f)) return position;
            }
            return target.Home + Vector3.up * 0.45f;
        }

        private List<string> HarvestedForSave()
        {
            var keys = new List<string>();
            foreach (string key in harvestedKeys)
            {
                string[] parts = key.Split('|');
                if (parts.Length == 3 && int.TryParse(parts[1], out int cycle) && travel != null && scene.Catalog.Segment(parts[0]) != null && travel.CycleOf(parts[0]) == cycle) keys.Add(key);
            }
            return keys;
        }

        private void RestoreHarvested(List<string> saved)
        {
            harvestedKeys.Clear();
            if (saved == null) return;
            foreach (string key in saved) harvestedKeys.Add(key);
        }

        // ---------------------------------------------------------------- heart sprite

        private static Sprite heart;
        private static Sprite HeartSprite
        {
            get
            {
                if (heart != null) return heart;
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Harvest heart" };
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float u = (x + 0.5f) / size * 2.6f - 1.3f, v = (y + 0.5f) / size * 2.5f - 1.15f;
                        float f = Mathf.Pow(u * u + v * v - 1f, 3f) - u * u * v * v * v;
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(f <= 0 ? 255 : 0));
                    }
                texture.SetPixels32(pixels); texture.Apply(false, true);
                return heart = Sprite.Create(texture, new UnityEngine.Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            }
        }
    }

    // Full-screen input surface while a harvest interaction is open (above the system-gesture area).
    public sealed class HarvestInputSurface : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public CollectionPrototype Owner;
        public void OnPointerDown(PointerEventData data) => Owner.HarvestDown(data.pointerId, data.position);
        public void OnDrag(PointerEventData data) => Owner.HarvestDrag(data.pointerId, data.position);
        public void OnPointerUp(PointerEventData data) => Owner.HarvestUp(data.pointerId, data.position);
    }
}
