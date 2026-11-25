using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 12: the warehouse counter view. Tapping the warehouse (street or settlement list) builds the counter 200 m below
    // the street (like the home interior), shows the keeper and the two deposit crates from CustomerCameraAnchor pulled back so
    // both crates clear the open bag, and opens the bag. Return (the screen return square) closes it. Counts as HomeOpen, so
    // every path that tears the home view down (faint, travel, context change: ResetHomeView) also tears this one down.
    // Task 5 adds the drag onto the crates; its seam is warehouseCrates / warehouseLids and RefreshWarehouseLabels.
    public sealed partial class CollectionPrototype
    {
        private const float WarehouseLabelMargin = 0.06f, WarehouseMaxPullback = 30f;   // metres of dolly
        private Transform warehouseHolder;
        private readonly GameObject[] warehouseCrates = new GameObject[2];   // the spawned crate instances (Task 5: drops, animation)
        private readonly Transform[] warehouseLids = new Transform[2];       // their Lid children (Task 5: the close animation)
        private readonly Text[] warehouseCrateLabels = new Text[2];
        private RectTransform warehouseHud;
        private Text warehouseCycleText, warehouseFeedbackText;
        private bool warehouseBagWasOpen;
        // Task 5: one CrateHold per dragging bag pointer, and each crate's close-and-carry animation (seconds in, -1 idle).
        private readonly System.Collections.Generic.Dictionary<int, CrateHold> warehouseHolds = new System.Collections.Generic.Dictionary<int, CrateHold>();
        private readonly float[] warehouseAnimation = { -1f, -1f };
        private readonly Vector3[] warehouseCrateHome = new Vector3[2];
        private readonly Quaternion[] warehouseLidOpen = new Quaternion[2];
        private const float LidCloseSeconds = 0.2f, SlideOffSeconds = 0.25f, SlideBackMetres = 0.6f, SlideDownMetres = 0.5f;   // then 0.2 s slide in (CrateHold.AnimationSeconds in all)

        private bool WarehouseOpen => warehouseHolder != null;
        // The crates (and their labels, inside the solver's margin) frame below the debug feedback line, the lowest of the lines on top.
        public static float WarehouseFramingTop(float safeBottomPx, float safeHeightPx) => safeBottomPx + FeedbackBand.yMin * safeHeightPx;
        public static readonly Vector2 WarehouseCycleBand = new Vector2(0.905f, 0.945f), WarehouseFeedbackBand = new Vector2(0.865f, 0.905f);   // y0, y1 of the safe area: under the gold label, above RESET SAVE

        // ---------------------------------------------------------------- texts and framing (checked by Milestone12Validation.ViewChecks)

        public static string WarehouseCrateLabel(WarehouseCrate crate) =>
            crate == null || crate.IsEmpty ? "Empty" : ItemName(crate.ItemId) + " " + crate.Count + "/" + WarehouseState.CrateUnits;
        public static string WarehouseCycleLine(WarehouseState s) =>
            "Crate " + s.CycleCrates + " of " + WarehouseState.CycleLength + " · " + Money.Cash(s.CycleCash) + " cash this cycle";

        // A crate label's width as a fraction of the screen width: grows with the text so a long item name still fits.
        public static float WarehouseLabelWidth(string text) => Mathf.Clamp(0.1f + 0.016f * text.Length, 0.34f, 0.8f);
        // A crate label's span: the safe width splits midway between the two crates (less a gap), and each label stays in its own
        // half, as wide as it wants up to that half, as centred on its crate as the half allows, so the two never overlap (R2 fix 1).
        public static void WarehouseLabelSpan(float centre, float other, float wanted, float gap, float min, float max, out float left, out float width)
        {
            float mid = (centre + other) * 0.5f;
            if (centre >= other) min = Mathf.Max(min, mid + gap * 0.5f); else max = Mathf.Min(max, mid - gap * 0.5f);
            width = Mathf.Clamp(wanted, 0f, Mathf.Max(0f, max - min));
            left = WarehouseLabelLeft(centre, width, min, max);
        }
        // Left edge of a label centred on `centre` (any unit), shifted inward to stay inside [min, max], keeping its width.
        public static float WarehouseLabelLeft(float centre, float width, float min, float max) => Mathf.Clamp(centre - width * 0.5f, min, Mathf.Max(min, max - width));
        // While the counter is open it owns the bag: an app pause leaves it open (the bag button is hidden there).
        public static bool WarehouseKeepsBagOnPause(bool warehouseOpen) => warehouseOpen;
        // The counter is never saved, so a save made inside it records the bag state the player returns to.
        public static bool SavedBagOpen(bool warehouseOpen, bool wasOpen, bool open) => warehouseOpen ? wasOpen : open;

        // Aim then dolly. Aim: only the pitch changes (about the camera's own right axis) so the centre of the targets' combined
        // bounds sits at the middle of the free area above the bag. Dolly: the camera backs away along the ray from that centre
        // through itself (the centre stays put on screen) to the smallest distance where every target's projected rect, with a label
        // margin (6% of the height), lies inside x [0,1] and y (bagTopViewport + margin, 1 - margin]. Capped at WarehouseMaxPullback.
        // The camera's transform is restored before returning.
        public static Pose WarehouseCameraPose(Camera cam, Bounds[] targets, float bagTopViewport, float topViewport = 1f)
        {
            Transform t = cam.transform; Vector3 startPosition = t.position; Quaternion startRotation = t.rotation;
            var all = targets[0]; for (int i = 1; i < targets.Length; i++) all.Encapsulate(targets[i]);
            float wanted = (bagTopViewport + topViewport) * 0.5f;
            bool Fits()
            {
                foreach (Bounds b in targets)
                {
                    float x0 = float.MaxValue, x1 = float.MinValue, y0 = float.MaxValue, y1 = float.MinValue;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 v = cam.WorldToViewportPoint(b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1)));
                        if (v.z <= 0f) return false;
                        x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
                    }
                    if (x0 < 0f || x1 > 1f || y0 - WarehouseLabelMargin <= bagTopViewport || y1 + WarehouseLabelMargin > topViewport) return false;
                }
                return true;
            }
            try
            {
                // Aim: pitching down (positive about the right axis) lifts things on screen, so the centre's y rises with the angle.
                float lo = -45f, hi = 45f;
                for (int i = 0; i < 30; i++)
                {
                    float mid = (lo + hi) * 0.5f;
                    t.rotation = Quaternion.AngleAxis(mid, startRotation * Vector3.right) * startRotation;
                    if (cam.WorldToViewportPoint(all.center).y < wanted) lo = mid; else hi = mid;
                }
                Quaternion aimed = Quaternion.AngleAxis((lo + hi) * 0.5f, startRotation * Vector3.right) * startRotation;
                t.rotation = aimed;
                Vector3 ray = startPosition - all.center; float distance = ray.magnitude; ray /= distance;
                float s = 0f, step = 0.05f;
                t.position = startPosition;
                while (!Fits())
                {
                    s += step; if (s > WarehouseMaxPullback) { s = WarehouseMaxPullback; break; }
                    t.position = startPosition + ray * s;
                }
                if (s > 0f && s < WarehouseMaxPullback)
                {
                    float a = s - step, b = s;
                    for (int i = 0; i < 20; i++) { float mid = (a + b) * 0.5f; t.position = startPosition + ray * mid; if (Fits()) b = mid; else a = mid; }
                    s = b;
                }
                return new Pose(startPosition + ray * s, aimed);
            }
            finally { t.SetPositionAndRotation(startPosition, startRotation); }
        }

        // ---------------------------------------------------------------- opening and closing

        private void OpenWarehouse()
        {
            if (phase != TravelPhase.Settlement || saveBlocked || HomeOpen || StreetView == null || cameraView == null || (stamina != null && stamina.Fainted)) return;
            if (world == null) world = GetComponent<MovementPrototype>();
            var counterPrefab = Resources.Load<GameObject>("Settlement/Storefronts/WarehouseCounter");
            var cratePrefab = Resources.Load<GameObject>("Settlement/Storefronts/DepositCrate");
            if (world == null || counterPrefab == null || cratePrefab == null) { Debug.LogWarning("[Warehouse] Counter art is missing; staying on the street."); return; }
            CancelSettlementPointer();
            settlementListOpen = false; settlementListLayout = -1;
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);

            var holder = new GameObject("Warehouse counter").transform;
            holder.SetParent(world.transform, false);
            holder.SetPositionAndRotation(MovementPrototype.InteriorWorldPosition, Quaternion.identity);
            Transform counter = Instantiate(counterPrefab, holder, false).transform;
            CounterLamps.Hang(counter, "Warehouse");
            foreach (Collider c in counter.GetComponentsInChildren<Collider>(true)) Destroy(c);
            var bounds = new Bounds[2];
            for (int i = 0; i < 2; i++)
            {
                Transform anchor = FindChild(counter, "OfferAnchor" + (i + 1));
                GameObject crate = Instantiate(cratePrefab, holder, false);
                crate.name = "Deposit crate " + (i + 1);
                crate.transform.position = anchor != null ? anchor.position : holder.position;
                warehouseCrates[i] = crate; warehouseLids[i] = FindChild(crate.transform, "Lid");
                warehouseCrateHome[i] = crate.transform.position; warehouseAnimation[i] = -1f;
                if (warehouseLids[i] != null) warehouseLidOpen[i] = warehouseLids[i].localRotation;
                bounds[i] = RendererBounds(crate);
            }
            warehouseHolder = holder;

            Transform eye = FindChild(counter, "CustomerCameraAnchor");
            Vector3 position = eye != null ? eye.position : holder.position + new Vector3(0f, 1.55f, 3f);
            Quaternion rotation = eye != null ? eye.rotation : Quaternion.LookRotation(Vector3.back);
            StreetView.HoldAt(position, rotation);

            warehouseBagWasOpen = inventoryOpen;
            if (!inventoryOpen) ToggleInventory();
            bagFrame.GetWorldCorners(warehouseCorners);   // overlay canvas: world corners are screen pixels
            float bagTop = Screen.height > 0 ? warehouseCorners[1].y / Screen.height : 0.3f;
            safe.GetWorldCorners(warehouseSafeCorners);
            float framingTop = Screen.height > 0 ? WarehouseFramingTop(warehouseSafeCorners[0].y, warehouseSafeCorners[1].y - warehouseSafeCorners[0].y) / Screen.height : 1f;
            Pose pose = WarehouseCameraPose(cameraView, bounds, bagTop, framingTop);
            StreetView.HoldAt(pose.position, pose.rotation);

            if (warehouseHud == null) CreateWarehouseHud();
            warehouseHud.gameObject.SetActive(true); SetCounterChrome(true);
            ShowScreenReturn(warehouseHud.gameObject, CloseWarehouse);   // hides the bag button and user menu (Craft, Equipment, map)
            RefreshWarehouseLabels();
            UpdateWarehouse();
        }
        private readonly Vector3[] warehouseCorners = new Vector3[4];

        private static Bounds RendererBounds(GameObject go)
        {
            var b = new Bounds(go.transform.position, Vector3.zero);
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
            return b;
        }

        // The Return square: back to the street, the bag as it was, and one save.
        private void CloseWarehouse()
        {
            if (!WarehouseOpen) return;
            TearDownWarehouse();
            settlementListLayout = -1;
            StreetView?.SnapToStreet();
            SetMenu(menuOpen);
            SaveNow();
        }

        // Every exit (Return, and ResetHomeView: a faint, travel, a context change): no counter, no HUD, the bag back to its entry state.
        private void TearDownWarehouse()
        {
            if (!WarehouseOpen) return;
            SetCounterChrome(false);
            if (Application.isPlaying) Destroy(warehouseHolder.gameObject); else DestroyImmediate(warehouseHolder.gameObject);
            warehouseHolder = null;
            System.Array.Clear(warehouseCrates, 0, warehouseCrates.Length); System.Array.Clear(warehouseLids, 0, warehouseLids.Length);
            warehouseHolds.Clear(); warehouseAnimation[0] = warehouseAnimation[1] = -1f;
            if (warehouseFeedbackText != null) warehouseFeedbackText.text = "";
            if (warehouseHud != null) warehouseHud.gameObject.SetActive(false);
            if (screenReturnOwner != null && warehouseHud != null && screenReturnOwner == warehouseHud.gameObject) HideScreenReturn();
            if (!warehouseBagWasOpen && inventoryOpen) { CloseContainer(); movement.CloseInventory(); inventoryOpen = false; LayoutBackpack(); }
        }

        // ---------------------------------------------------------------- HUD

        private void CreateWarehouseHud()
        {
            warehouseHud = Rect("Warehouse HUD", safe, 0, 0, 1, 1);
            warehouseHud.SetAsFirstSibling();   // behind the bag, squares, notes and drag ghosts
            warehouseCycleText = HudLine(warehouseHud, WarehouseCycleBand.x, WarehouseCycleBand.y);
            warehouseFeedbackText = HudLine(warehouseHud, WarehouseFeedbackBand.x, WarehouseFeedbackBand.y);
            CreateWalletLabels(warehouseHud);
            for (int i = 0; i < warehouseCrateLabels.Length; i++)
            {
                RectTransform box = Rect("Crate label " + (i + 1), warehouseHud, 0, 0, 0, 0);
                box.pivot = new Vector2(0f, 0f); box.sizeDelta = new Vector2(380f, 56f);
                box.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.105f, 0.085f, 0.8f);
                box.GetComponent<Image>().raycastTarget = false;
                Text text = Label(box, "", 0.04f, 0f, 0.96f, 1f, 26);
                text.raycastTarget = false; text.resizeTextForBestFit = true; text.resizeTextMinSize = 12; text.resizeTextMaxSize = 26;
                warehouseCrateLabels[i] = text;
            }
            warehouseHud.gameObject.SetActive(false);
        }
        private static Text HudLine(Transform parent, float y0, float y1)
        {
            Text line = Label(parent, "", 0.03f, y0, 0.97f, y1, 24);
            line.raycastTarget = false; line.alignment = TextAnchor.MiddleCenter;
            line.resizeTextForBestFit = true; line.resizeTextMinSize = 12; line.resizeTextMaxSize = 24;
            return line;
        }

        // The texts: crate labels, cycle line, wallet labels. Task 5 calls this after every deposit.
        private void RefreshWarehouseLabels()
        {
            if (warehouseHud == null) return;
            for (int i = 0; i < warehouseCrateLabels.Length; i++) warehouseCrateLabels[i].text = WarehouseCrateLabel(warehouse.Crates[i]);
            warehouseCycleText.text = WarehouseCycleLine(warehouse);
            RefreshWalletLabels();
        }

        // Per frame from UpdateSettlement: each label floats over the top of its crate, shifted inward to stay inside the safe area.
        private readonly Vector3[] warehouseSafeCorners = new Vector3[4], warehouseLabelAnchors = new Vector3[2];
        private void UpdateWarehouse()
        {
            if (!WarehouseOpen || cameraView == null) return;
            AnimateWarehouseCrates(Time.unscaledDeltaTime);
            TickWarehouseHolds();
            if (!WarehouseOpen) return;
            UpdateWalletLabels();
            safe.GetWorldCorners(warehouseSafeCorners);   // overlay canvas: screen pixels
            float left = warehouseSafeCorners[0].x, right = warehouseSafeCorners[2].x;
            for (int i = 0; i < warehouseCrates.Length; i++)
            {
                if (warehouseCrates[i] == null) { warehouseLabelAnchors[i] = Vector3.zero; continue; }
                Bounds b = RendererBounds(warehouseCrates[i]);
                warehouseLabelAnchors[i] = cameraView.WorldToScreenPoint(new Vector3(b.center.x, b.max.y, b.center.z));
            }
            for (int i = 0; i < warehouseCrates.Length; i++)
            {
                if (warehouseCrates[i] == null) continue;
                Vector3 screen = warehouseLabelAnchors[i];
                bool show = screen.z > 0f;
                var box = (RectTransform)warehouseCrateLabels[i].transform.parent;
                if (box.gameObject.activeSelf != show) box.gameObject.SetActive(show);
                if (!show) continue;
                WarehouseLabelSpan(screen.x, warehouseLabelAnchors[1 - i].x, WarehouseLabelWidth(warehouseCrateLabels[i].text) * (right - left), Screen.height * 0.01f, left, right, out float x, out float width);
                box.sizeDelta = new Vector2(width / box.lossyScale.x, 56f);
                box.position = new Vector3(x, screen.y + Screen.height * 0.01f, 0f);
            }
        }

        // ---------------------------------------------------------------- drops, hold-fill and the crate animation (Task 5)

        public static string WarehousePayoutLine(WarehouseDeposit d) =>
            "+" + Money.Cash(d.CashPaid) + " cash" + (d.GoldPaid > 0 ? "   +" + Money.Cash(d.GoldPaid) + " gold" : "");

        private bool WarehouseAnimating(int crate) => warehouseAnimation[crate] >= 0f;

        // The crate whose projected renderer bounds (padded to at least 48 dp) hold the screen point, or -1. The crates move
        // while animating, so this is projected fresh each call.
        private int WarehouseCrateAt(Vector2 screen)
        {
            if (!WarehouseOpen || cameraView == null) return -1;
            float minSize = 48f * (Screen.dpi > 0f ? Screen.dpi : 160f) / 160f;
            for (int i = 0; i < warehouseCrates.Length; i++)
            {
                if (warehouseCrates[i] == null) continue;
                Bounds b = RendererBounds(warehouseCrates[i]);
                float x0 = float.MaxValue, x1 = float.MinValue, y0 = float.MaxValue, y1 = float.MinValue; bool behind = false;
                for (int k = 0; k < 8; k++)
                {
                    Vector3 v = cameraView.WorldToScreenPoint(b.center + Vector3.Scale(b.extents, new Vector3((k & 1) == 0 ? -1 : 1, (k & 2) == 0 ? -1 : 1, (k & 4) == 0 ? -1 : 1)));
                    behind |= v.z <= 0f;
                    x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
                }
                if (behind) continue;
                float padX = Mathf.Max(0f, (minSize - (x1 - x0)) * 0.5f), padY = Mathf.Max(0f, (minSize - (y1 - y0)) * 0.5f);
                if (screen.x >= x0 - padX && screen.x <= x1 + padX && screen.y >= y0 - padY && screen.y <= y1 + padY) return i;
            }
            return -1;
        }

        // Per frame: lid shut (0.2 s), slide back and down off the counter (0.25 s), then a fresh open crate slides in from
        // 0.6 m back to its anchor (0.2 s). Visual only: the domain already paid when the crate completed, and the label
        // already reads the new, empty crate.
        private void AnimateWarehouseCrates(float deltaTime)
        {
            Vector3 back = Vector3.ProjectOnPlane(cameraView.transform.forward, Vector3.up).normalized * SlideBackMetres;
            for (int i = 0; i < warehouseCrates.Length; i++)
            {
                if (warehouseAnimation[i] < 0f || warehouseCrates[i] == null) continue;
                float t = warehouseAnimation[i] += deltaTime;
                Transform crate = warehouseCrates[i].transform, lid = warehouseLids[i];
                Vector3 home = warehouseCrateHome[i];
                if (t >= CrateHold.AnimationSeconds) { warehouseAnimation[i] = -1f; crate.position = home; if (lid != null) lid.localRotation = warehouseLidOpen[i]; continue; }
                if (t < LidCloseSeconds) { if (lid != null) lid.localRotation = Quaternion.Slerp(warehouseLidOpen[i], Quaternion.identity, t / LidCloseSeconds); continue; }
                bool leaving = t < LidCloseSeconds + SlideOffSeconds;
                if (lid != null) lid.localRotation = leaving ? Quaternion.identity : warehouseLidOpen[i];
                crate.position = leaving
                    ? Vector3.Lerp(home, home + back + Vector3.down * SlideDownMetres, (t - LidCloseSeconds) / SlideOffSeconds)
                    : Vector3.Lerp(home + back + Vector3.down * SlideDownMetres, home, (t - LidCloseSeconds - SlideOffSeconds) / (CrateHold.AnimationSeconds - LidCloseSeconds - SlideOffSeconds));
            }
        }

        // Every dragging bag contact is ticked each frame here: the bag only reports moves, and a held finger keeps filling.
        private readonly System.Collections.Generic.List<int> warehousePointers = new System.Collections.Generic.List<int>();
        private void TickWarehouseHolds()
        {
            warehousePointers.Clear(); warehousePointers.AddRange(warehouseHolds.Keys);
            foreach (int pointer in warehousePointers)   // contacts dropped without End (CancelAll): forget them, saving what they deposited
                if (!contacts.ContainsKey(pointer)) { if (warehouseHolds[pointer].Deposited) SaveNow(); warehouseHolds.Remove(pointer); }
            warehousePointers.Clear(); warehousePointers.AddRange(contacts.Keys);
            foreach (int pointer in warehousePointers)
                if (contacts.TryGetValue(pointer, out Contact contact) && contact.Dragging && !contact.World && contact.Source == null && contact.RackSpot < 0)
                    TickWarehouseHold(pointer, contact, contact.Position);
        }

        private void TickWarehouseHold(int pointer, Contact contact, Vector2 screen)
        {
            if (!warehouseHolds.TryGetValue(pointer, out CrateHold hold)) warehouseHolds[pointer] = hold = new CrateHold(contact.Item.Definition.Stackable);
            int crate = hold.Tick(WarehouseCrateAt(screen), WarehouseAnimating(0), WarehouseAnimating(1), Time.unscaledDeltaTime, ledger.ReservedBy(contact.Item, pointer));
            if (crate < 0 || !WarehouseDepositInto(pointer, contact, crate, hold) || ledger.ReservedBy(contact.Item, pointer)) return;
            // The stack ran out: the drag is over (nothing reserved, no ghost), and this is its one save.
            warehouseHolds.Remove(pointer); Release(pointer, contact); Redraw(); SaveNow();
        }

        // From End, beside EndContainerContact. True when the warehouse handled the contact: a release over a crate (one deposit
        // if the drag deposited nothing yet; otherwise, or when refused, the item snaps back), or any end of a drag that deposited
        // (snap back, one save).
        private bool EndWarehouseContact(int pointer, Contact contact, Vector2 position, bool canceled)
        {
            if (!WarehouseOpen || contact.World || contact.Source != null || contact.RackSpot >= 0) return false;
            if (!warehouseHolds.TryGetValue(pointer, out CrateHold hold)) hold = new CrateHold(contact.Item.Definition.Stackable);
            warehouseHolds.Remove(pointer);
            int over = canceled || !contact.Dragging ? -1 : WarehouseCrateAt(position);
            int crate = hold.Release(over, WarehouseAnimating(0), WarehouseAnimating(1), ledger.ReservedBy(contact.Item, pointer));
            if (crate >= 0) WarehouseDepositInto(pointer, contact, crate, hold);
            if (!hold.Deposited && over < 0) return false;   // not a warehouse drop: the bag's own end handles it
            Release(pointer, contact); Redraw();
            if (hold.Deposited) SaveNow();
            return true;
        }

        private bool WarehouseDepositInto(int pointer, Contact contact, int crate, CrateHold hold)
        {
            if (!WarehouseService.Deposit(ledger, warehouse, travel.Wallet, warehouseLog, contact.Item, pointer, crate,
                    () => "warehouse-" + System.Guid.NewGuid().ToString("N"), System.DateTime.UtcNow, out WarehouseDeposit d, out string reason))
            { warehouseFeedbackText.text = reason; return false; }
            hold.Deposited = true;
            statistics.AddCash(d.CashPaid);
            if (d.CrateCompleted) warehouseAnimation[crate] = 0f;
            warehouseFeedbackText.text = d.CrateCompleted ? WarehousePayoutLine(d) : "";
            RefreshWarehouseLabels(); Redraw();
            return true;
        }
    }
}
