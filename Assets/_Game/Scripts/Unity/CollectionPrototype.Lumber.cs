using System.Collections;
using System.Collections.Generic;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Harvest;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 13: the forester's counter (lumber field). Tapping the lumber building or the Lumber list entry builds the counter
    // 200 m below the street like the warehouse, framed from CustomerCameraAnchor with the bag closed. The ledger (tap the page)
    // lists next-tier-only unlocks and travel arrows; the door opens on the first unlock and leads to the last visited zone.
    // Counts as HomeOpen, so ResetHomeView tears it down. The door and the ledger arrows lead to a tier's zone (zone view below),
    // which reuses the same holder, so LumberOpen and every teardown cover both.
    public sealed partial class CollectionPrototype
    {
        public const string LumberDoorClosedText = "Unlock a tier at the ledger first";
        private const float LumberDoorSeconds = 0.6f, LumberTapMinDp = 48f;
        private Transform lumberHolder, lumberDoorPivot, lumberLedgerAnchor, lumberDoorAnchor;
        private float lumberDoorTime = -1f;   // seconds into the opening animation, -1 idle
        private RectTransform lumberHud, lumberLedgerPage;
        private GameObject lumberLedgerSheet;
        private int? lumberPointer; private Vector2 lumberStart, lumberLast; private bool lumberMoved;

        private bool lumberPayArmed;   // the price row was tapped: PAY shows (two taps to buy)

        private bool LumberOpen => lumberHolder != null;
        public static readonly UnityEngine.Rect LumberWoodButtonBand = UnityEngine.Rect.MinMaxRect(0.02f, 0.81f, 0.37f, 0.86f);   // debug WOOD T2-T4 (safe area): level with RESET SAVE, on the feedback line's top

        // ---------------------------------------------------------------- pure rules (checked by Milestone13Validation.CounterChecks)

        public enum LedgerEntryState { Open, Buyable, Locked }
        public sealed class LedgerEntry { public int Tier; public string Name, Line; public LedgerEntryState State; }

        // Entries in page order: every unlocked tier (Open), then the next tier only (Buyable once its wood is discovered, else Locked).
        public static List<LedgerEntry> LedgerEntries(LumberFieldState s, ICollection<int> discoveredTiers)
        {
            var entries = new List<LedgerEntry>();
            for (int t = 1; t <= s.UnlockedTier; t++) entries.Add(new LedgerEntry { Tier = t, Name = WoodTitle(t), Line = "Open →", State = LedgerEntryState.Open });
            int next = LumberFieldService.NextTier(s);
            if (next > 0)
            {
                bool found = LumberFieldService.WoodDiscovered(next, discoveredTiers, s.DebugWoodTier);
                entries.Add(new LedgerEntry
                {
                    Tier = next, Name = WoodTitle(next), State = found ? LedgerEntryState.Buyable : LedgerEntryState.Locked,
                    Line = found ? Money.Cash(LumberFieldService.UnlockPrice(next)) + " gold" : "Reach a " + LumberFieldService.WoodName(next) + " region first"
                });
            }
            return entries;
        }
        // Cell split (top to bottom, fractions of the cell): picture 0.44-0.98, name 0.31-0.44, line 0.20-0.31, PAY 0-0.19.
        private const float LedgerGutter = 0.02f;
        // The 2x2 page grid in normalized page units: 0 top left, 1 top right, 2 bottom left, 3 bottom right.
        public static Rect LedgerCell(int index)
        {
            float size = (1f - LedgerGutter) * 0.5f;
            return new Rect((index % 2) * (size + LedgerGutter), index < 2 ? size + LedgerGutter : 0f, size, size);
        }
        // Row index that shows PAY: the price row, only once armed; -1 otherwise.
        public static int LedgerPayRow(int rowCount, bool payable, bool armed) => payable && armed ? rowCount - 1 : -1;
        // WOOD T2-T4: the first press makes birch discoverable.
        public static int NextDebugWoodTier(int current) => System.Math.Min(LumberFieldState.Tiers, System.Math.Max(current, 1) + 1);
        private static string WoodTitle(int tier) { string n = LumberFieldService.WoodName(tier); return char.ToUpperInvariant(n[0]) + n.Substring(1); }
        public static string LedgerArtPath(int tier) => "UI/Lumber/" + WoodTitle(tier) + "Log";
        public static Quaternion LumberDoorRotation(int unlockedTier) => unlockedTier <= 0 ? Quaternion.identity : Quaternion.Euler(0f, 100f, 0f);
        // The lumber list entry and the lumber building both open the counter (everything else keeps its note).
        public static bool OpensLumberCounter(string listKey, StreetBuildingKind kind) => listKey == "lumber" || kind == StreetBuildingKind.LumberField;

        private List<int> LumberDiscoveredTiers()
        {
            var tiers = new List<int>();
            foreach (string id in travel.DiscoveredRegions) { RegionDefinition r = scene.Catalog.Region(id); if (r != null) tiers.Add(SegmentLayout.Tier(r)); }
            return tiers;
        }

        // ---------------------------------------------------------------- opening and closing

        private void OpenLumberCounter()
        {
            if (phase != TravelPhase.Settlement || saveBlocked || HomeOpen || StreetView == null || cameraView == null || (stamina != null && stamina.Fainted)) return;
            if (world == null) world = GetComponent<MovementPrototype>();
            var prefab = Resources.Load<GameObject>("Settlement/Storefronts/LumberCounter");
            if (world == null || prefab == null) { Debug.LogWarning("[Lumber] Counter art is missing; staying on the street."); return; }
            CancelSettlementPointer();
            settlementListOpen = false; settlementListLayout = -1;
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            if (inventoryOpen) ToggleInventory();   // the bag is closed at the counter

            var holder = new GameObject("Lumber counter").transform;
            holder.SetParent(world.transform, false);
            holder.SetPositionAndRotation(MovementPrototype.InteriorWorldPosition, Quaternion.identity);
            Transform counter = Instantiate(prefab, holder, false).transform;
            CounterLamps.Hang(counter, "Lumber");
            foreach (Collider c in counter.GetComponentsInChildren<Collider>(true)) Destroy(c);
            lumberDoorPivot = FindChild(counter, "DoorPivot"); lumberLedgerAnchor = FindChild(counter, "LedgerSelectAnchor"); lumberDoorAnchor = FindChild(counter, "DoorSelectAnchor");
            lumberDoorTime = -1f;
            if (lumberDoorPivot != null) lumberDoorPivot.localRotation = LumberDoorRotation(lumber.UnlockedTier);
            lumberHolder = holder;

            Pose pose = LumberCounterPose(cameraView, counter);
            StreetView.HoldAt(pose.position, pose.rotation);

            if (lumberHud == null) CreateLumberHud();
            lumberPayArmed = false; lumberHud.gameObject.SetActive(true); CloseLedgerSheet(); SetCounterChrome(true);
            ShowScreenReturn(lumberHud.gameObject, CloseLumber);
            RefreshLumberHud();
        }

        // The raw CustomerCameraAnchor view puts the ledger on a portrait screen's left edge and the door off screen: turn toward
        // both select anchors, then the warehouse solver pitches and pulls back until both sit in the band below the debug buttons.
        public static Pose LumberCounterPose(Camera cam, Transform counter)
        {
            Transform eye = FindChild(counter, "CustomerCameraAnchor"), ledger = FindChild(counter, "LedgerSelectAnchor"), door = FindChild(counter, "DoorSelectAnchor");
            if (eye == null) return new Pose(counter.position + new Vector3(0f, 1.55f, 3f), Quaternion.LookRotation(Vector3.back));
            if (ledger == null || door == null) return new Pose(eye.position, eye.rotation);
            var targets = new[] { new Bounds(ledger.position, Vector3.one * 0.8f), new Bounds(door.position, Vector3.one * 0.8f) };   // ~a tap's reach around each
            Vector3 flat = Vector3.ProjectOnPlane((ledger.position + door.position) * 0.5f - eye.position, Vector3.up);
            Transform t = cam.transform; Vector3 p = t.position; Quaternion r = t.rotation;
            t.SetPositionAndRotation(eye.position, Quaternion.LookRotation(flat));
            try { return WarehouseCameraPose(cam, targets, 0.1f, 0.48f); }
            finally { t.SetPositionAndRotation(p, r); }
        }

        // The Return square: back to the street and one save.
        private void CloseLumber()
        {
            if (!LumberOpen) return;
            TearDownLumber();
            settlementListLayout = -1;
            StreetView?.SnapToStreet();
            SetMenu(menuOpen);
            SaveNow();
        }

        // Every exit (Return, and ResetHomeView).
        private void TearDownLumber()
        {
            if (!LumberOpen) return;
            SetCounterChrome(false);
            if (active != null && active.MassiveTier > 0) EndInteraction();   // a cut in progress is discarded with the zone
            ClosePickup();
            lumberHolder = ClearLumberScene(ledger, lumberHolder); lumberDoorPivot = lumberLedgerAnchor = lumberDoorAnchor = null;
            lumberPointer = null; lumberDoorTime = -1f; CloseLedgerSheet();
            zoneTier = 0; zonePan = 0f; zoneCameraAnchor = zoneExitAnchor = null; System.Array.Clear(zoneTrees, 0, zoneTrees.Length);
            if (lumberHud != null) lumberHud.gameObject.SetActive(false);
            if (zoneHud != null) zoneHud.gameObject.SetActive(false);
            if (screenReturnOwner != null && (lumberHud != null && screenReturnOwner == lumberHud.gameObject || zoneHud != null && screenReturnOwner == zoneHud.gameObject)) HideScreenReturn();
        }

        // ---------------------------------------------------------------- HUD and ledger

        private void CreateLumberHud()
        {
            lumberHud = Rect("Lumber HUD", safe, 0, 0, 1, 1);
            lumberHud.SetAsFirstSibling();
            CreateWalletLabels(lumberHud);
            Button(lumberHud, "WOOD T2-T4", LumberWoodButtonBand.xMin, LumberWoodButtonBand.yMin, LumberWoodButtonBand.xMax, LumberWoodButtonBand.yMax, () =>
            {
                lumber.DebugWoodTier = NextDebugWoodTier(lumber.DebugWoodTier);   // debug (test phase), removed with HOUSE L8
                SaveNow(); RefreshLumberHud();
            });
            lumberHud.gameObject.SetActive(false);
            CreateLedgerSheet();
        }

        // The ledger as a paper sheet over a dim backdrop (the furniture store's pattern): a Close square and a page of up to four cells.
        private void CreateLedgerSheet()
        {
            RectTransform root = Rect("Lumber ledger sheet", safe, 0, 0, 1, 1);
            RectTransform backdrop = Rect("Backdrop", root, 0, 0, 1, 1);
            backdrop.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);   // blocks every touch behind the sheet
            var away = backdrop.gameObject.AddComponent<UnityEngine.UI.Button>(); away.transition = Selectable.Transition.None; away.onClick.AddListener(CloseLedgerSheet);
            Transform p = Sheet("Sheet", root, "paper_1", 0.05f, 0.13f, 0.95f, 0.87f, 0f).transform;
            Text close = Button(p, "Close", 0, 0, 0, 0, CloseLedgerSheet);
            var closeRect = (RectTransform)close.transform.parent; closeRect.name = "Lumber: Close ledger";
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.97f, 0.975f); closeRect.pivot = new Vector2(1f, 1f);
            closeRect.sizeDelta = new Vector2(HudSquare, HudSquare); closeRect.anchoredPosition = Vector2.zero;
            HudIconStyle.Apply(close, "return");
            lumberLedgerPage = Rect("Page", p, 0.05f, 0.04f, 0.95f, 0.87f);
            lumberLedgerSheet = root.gameObject;
            lumberLedgerSheet.SetActive(false);
        }

        private void OpenLedgerSheet()
        {
            lumberPayArmed = false;
            lumberLedgerSheet.transform.SetAsLastSibling(); lumberLedgerSheet.SetActive(true);
            RefreshLumberHud();
        }
        private void CloseLedgerSheet()
        {
            lumberPayArmed = false;
            if (lumberLedgerSheet != null) lumberLedgerSheet.SetActive(false);
        }

        private void RefreshLumberHud()
        {
            if (lumberHud == null) return;
            RefreshWalletLabels();
            List<int> tiers = LumberDiscoveredTiers();
            List<LedgerEntry> entries = LedgerEntries(lumber, tiers);
            int next = LumberFieldService.NextTier(lumber);
            bool payable = next > 0 && LumberFieldService.WoodDiscovered(next, tiers, lumber.DebugWoodTier);
            if (!payable) lumberPayArmed = false;
            if (!lumberLedgerSheet.activeSelf) return;
            int payRow = LedgerPayRow(entries.Count, payable, lumberPayArmed);
            ClearContent(lumberLedgerPage);
            for (int i = 0; i < entries.Count; i++) BuildLedgerCell(entries[i], i, i == payRow, next);
        }

        private void BuildLedgerCell(LedgerEntry e, int index, bool showPay, int nextTier)
        {
            Rect c = LedgerCell(index);
            bool locked = e.State == LedgerEntryState.Locked;
            RectTransform cell = Rect("Entry " + e.Name, lumberLedgerPage, c.xMin, c.yMin, c.xMax, c.yMax);
            Image band = cell.gameObject.AddComponent<Image>(); band.color = locked ? HomeLockedBand : HomeRowBand;
            if (locked) band.raycastTarget = false;
            else { var button = cell.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = band; button.onClick.AddListener(() => LedgerRowTapped(index)); }
            Image icon = Rect("Picture", cell, 0.05f, 0.44f, 0.95f, 0.98f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
            Sprite sprite = Resources.Load<Sprite>(LedgerArtPath(e.Tier));
            if (sprite != null) icon.sprite = sprite; else icon.color = Color.clear;
            if (locked && sprite != null) icon.color = new Color(1f, 1f, 1f, HomeLockedAlpha);
            Text name = Label(cell, e.Name, 0.04f, 0.31f, 0.96f, 0.44f, 30); name.color = Dim(Ink, locked); name.fontStyle = FontStyle.Bold;
            name.resizeTextForBestFit = true; name.resizeTextMinSize = 12; name.resizeTextMaxSize = 30;
            Text line = Label(cell, e.Line, 0.04f, 0.20f, 0.96f, 0.31f, 22); line.color = Dim(FadedInk, locked);
            if (locked) line.fontStyle = FontStyle.Italic;
            line.resizeTextForBestFit = true; line.resizeTextMinSize = 11; line.resizeTextMaxSize = 22;
            if (!showPay) return;
            Text pay = Button(cell, "PAY", 0.06f, 0f, 0.94f, 0.19f, PayTapped);
            pay.text = "PAY " + Money.Cash(LumberFieldService.UnlockPrice(nextTier)) + " GOLD";
        }

        private void LedgerRowTapped(int row)
        {
            if (row < lumber.UnlockedTier) { TravelToZone(row + 1); return; }
            if (row == lumber.UnlockedTier && !lumberPayArmed) { lumberPayArmed = true; RefreshLumberHud(); }   // the price row arms PAY
        }

        private void PayTapped()
        {
            int tier = LumberFieldService.NextTier(lumber);
            if (tier == 0) return;
            long goldBefore = travel.Gold; int before = lumber.UnlockedTier;
            lumberPayArmed = false;
            if (!LumberFieldService.Unlock(lumber, travel.Wallet, tier, LumberDiscoveredTiers(), out string reason)) { ShowSettlementNote(reason); return; }
            statistics.SpendGold(goldBefore - travel.Gold);
            if (before == 0) lumberDoorTime = 0f;   // the first unlock opens the door
            SaveNow(); RefreshLumberHud();
        }

        // Per frame from UpdateSettlement: the door swings open over LumberDoorSeconds.
        private void UpdateLumber()
        {
            if (!LumberOpen) return;
            if (lumberDoorTime >= 0f && lumberDoorPivot != null)
            {
                lumberDoorTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(lumberDoorTime / LumberDoorSeconds);
                lumberDoorPivot.localRotation = Quaternion.Slerp(Quaternion.identity, LumberDoorRotation(1), t);
                if (t >= 1f) lumberDoorTime = -1f;
            }
            if (lumberHud != null) UpdateWalletLabels();
            UpdateZone();
        }

        // ---------------------------------------------------------------- touch: a short tap near the ledger page or the doorway

        private bool LumberPointerBegin(int pointer, Vector2 position)
        {
            if (lumberPointer.HasValue || active != null) return true;   // a chop owns the screen: touches outside its input surface do nothing
            lumberPointer = pointer; lumberStart = lumberLast = position; lumberMoved = false;
            return true;
        }
        private bool LumberPointerMove(int pointer, Vector2 position)
        {
            if (lumberPointer != pointer) return false;
            if ((position - lumberStart).sqrMagnitude > Mathf.Pow(Screen.height * 0.012f, 2)) lumberMoved = true;
            if (lumberMoved && zoneTier > 0) PanZone(position.x - lumberLast.x);   // the zone drags sideways like the street
            lumberLast = position;
            return true;
        }
        private bool LumberPointerEnd(int pointer, Vector2 position, bool canceled)
        {
            if (lumberPointer != pointer) return false;
            lumberPointer = null;
            if (!canceled && !lumberMoved) LumberTap(position);
            return true;
        }

        // Screen distance (px) to the anchor, or max when it is missing or behind the camera.
        private float LumberAnchorDistance(Transform anchor, Vector2 screen)
        {
            if (anchor == null || cameraView == null) return float.MaxValue;
            Vector3 p = cameraView.WorldToScreenPoint(anchor.position);
            return p.z <= 0f ? float.MaxValue : Vector2.Distance(screen, p);
        }

        // The art has no colliders: a tap counts within 48 dp (or a tenth of the width) of a projected anchor, nearest wins.
        private static float LumberReach => Mathf.Max(LumberTapMinDp * (Screen.dpi > 0f ? Screen.dpi : 160f) / 160f, Screen.width * 0.1f);

        private void LumberTap(Vector2 screen)
        {
            if (zoneTier > 0) { ZoneTap(screen); return; }
            float reach = LumberReach;
            float ledger = LumberAnchorDistance(lumberLedgerAnchor, screen), door = LumberAnchorDistance(lumberDoorAnchor, screen);
            if (ledger <= reach && ledger <= door) { OpenLedgerSheet(); return; }
            if (door <= reach)
            {
                if (lumber.UnlockedTier < 1) ShowSettlementNote(LumberDoorClosedText);
                else if (lumberDoorTime < 0f) TravelToZone(lumber.LastZone);
                return;
            }
            lumberPayArmed = false;
        }

        // ---------------------------------------------------------------- zone view (a tier's six massive trees)

        // LumberZoneT{tier} 200 m below the street in place of the counter, the camera from ZoneCameraAnchor plus a sideways
        // pan (the fixed view cuts off the outer columns of T3 and T4 on the portrait phone). Refreshed on entry and once a
        // second (which also covers an app resume); tapping near a tree base starts the chop or reopens its pickup.
        private sealed class ZoneTree { public int Tier, Index; public Transform Root; public Tier1PineRig Rig; public RectTransform Hint; public Text Label; }
        private readonly ZoneTree[] zoneTrees = new ZoneTree[LumberFieldState.TreesPerTier];
        private int zoneTier;   // 0 at the counter
        private float zonePan, zonePanRange, zoneRefreshAt;
        private Transform zoneCameraAnchor, zoneExitAnchor;
        private RectTransform zoneHud;
        private Text zoneFeedbackText;
        private readonly RectTransform[] zoneHints = new RectTransform[LumberFieldState.TreesPerTier];
        private readonly Text[] zoneLabels = new Text[LumberFieldState.TreesPerTier];

        // shortcut: the session has no IClock field (everything reads DateTime.UtcNow); add one if the lumber clock needs faking in play mode.
        private System.DateTime LumberNow => System.DateTime.UtcNow + lumber.ClockOffset;

        public static string MassiveVariant(int tier, int anchorIndex) => "Massive" + WoodTitle(tier) + "ABCCAB"[anchorIndex];

        // "Regrows in 2d 14h · leftovers expire then" while rows are pending, "Regrows in 5h 40m" on a stump, null standing. Truncated.
        public static string RegrowLabel(MassiveTree t, System.DateTime now)
        {
            if (t.Standing) return null;
            System.TimeSpan left = LumberFieldService.Remaining(t, now);
            string time = left.TotalDays >= 1 ? (int)left.TotalDays + "d " + left.Hours + "h" : left.Hours + "h " + left.Minutes + "m";
            return "Regrows in " + time + (t.Pending.Count > 0 ? " · leftovers expire then" : "");
        }

        // The zone prefab with its six live trees (rig kept, yaw 0) and, for T1-T3, the next tier's three static backdrop trees.
        // Built under an inactive holder so no tree script wakes before it is placed. Also used by Milestone13Validation.
        public static Transform BuildZone(Transform parent, int tier)
        {
            const string Folder = "Settlement/LumberField/";
            var prefab = Resources.Load<GameObject>(Folder + "LumberZoneT" + tier);
            if (prefab == null) return null;
            var holder = new GameObject("inactive holder"); holder.SetActive(false);
            Transform zone = Instantiate(prefab, holder.transform, false).transform;
            zone.name = prefab.name;
            for (int i = 0; i < LumberFieldState.TreesPerTier; i++)
            {
                Transform anchor = FindChild(zone, "TreeAnchor" + (i + 1)); var tree = Resources.Load<GameObject>(Folder + MassiveVariant(tier, i));
                if (anchor != null && tree != null) Instantiate(tree, anchor, false).name = tree.name;
            }
            for (int i = 0; tier < LumberFieldState.Tiers && i < 3; i++)
            {
                string variant = "Massive" + WoodTitle(tier + 1) + "ABC"[i];
                Transform anchor = FindChild(zone, "BackdropAnchor" + (i + 1));
                if (anchor != null) SettlementBackdrop.PlaceWhole(anchor, Resources.Load<GameObject>(Folder + variant), variant, Vector3.zero, 0f, 1f);
            }
            zone.SetParent(parent, false);
            DestroyImmediate(holder);
            return zone;
        }

        private void TravelToZone(int tier)
        {
            if (tier < 1 || tier > lumber.UnlockedTier || world == null) return;
            lumber.LastZone = tier;
            TearDownLumber();
            var holder = new GameObject("Lumber zone").transform;
            holder.SetParent(world.transform, false);
            holder.SetPositionAndRotation(MovementPrototype.InteriorWorldPosition, Quaternion.identity);
            Transform zone = BuildZone(holder, tier);
            if (zone == null) { Debug.LogWarning("[Lumber] Zone art is missing; back to the counter."); Destroy(holder.gameObject); SaveNow(); OpenLumberCounter(); if (!LumberOpen) { StreetView?.SnapToStreet(); SetMenu(menuOpen); } return; }
            lumberHolder = holder; zoneTier = tier; zonePan = 0f;
            zoneCameraAnchor = FindChild(zone, "ZoneCameraAnchor"); zoneExitAnchor = FindChild(zone, "ExitSelectAnchor");
            Transform first = FindChild(zone, "TreeAnchor1");
            zonePanRange = first != null ? Mathf.Abs(first.localPosition.x) : 0f;   // the camera can centre on either outer column

            if (zoneHud == null) CreateZoneHud();
            for (int i = 0; i < zoneTrees.Length; i++)
            {
                Transform anchor = FindChild(zone, "TreeAnchor" + (i + 1));
                Transform root = anchor != null && anchor.childCount > 0 ? anchor.GetChild(0) : null;
                zoneTrees[i] = root == null ? null : new ZoneTree { Tier = tier, Index = i, Root = root, Rig = root.GetComponent<Tier1PineRig>(), Hint = zoneHints[i], Label = zoneLabels[i] };
                zoneHints[i].gameObject.SetActive(false); zoneLabels[i].transform.parent.gameObject.SetActive(false);
            }
            zoneFeedbackText.text = "";
            zoneHud.gameObject.SetActive(true);
            ShowScreenReturn(zoneHud.gameObject, LeaveZone);
            HoldZoneCamera();
            RefreshZone(true);   // also saves LastZone
        }

        // Return and the exit gate: back to the counter.
        private void LeaveZone()
        {
            if (zoneTier == 0) return;
            TearDownLumber();
            OpenLumberCounter();
            if (!LumberOpen) { StreetView?.SnapToStreet(); SetMenu(menuOpen); }   // the counter refused (save blocked): do not leave the camera held
        }

        private void HoldZoneCamera()
        {
            if (zoneCameraAnchor != null) StreetView?.HoldAt(zoneCameraAnchor.position + Vector3.right * zonePan, zoneCameraAnchor.rotation);
        }

        // Pixels to metres at the ground point the camera looks at. The camera faces -Z, so a drag to the right moves it to +X.
        private void PanZone(float dragPixels)
        {
            if (zoneCameraAnchor == null || cameraView == null) return;
            float distance = zoneCameraAnchor.localPosition.y / Mathf.Max(0.1f, -zoneCameraAnchor.forward.y);
            zonePan = Mathf.Clamp(zonePan - StreetDragMetres(dragPixels, Screen.width, cameraView.fieldOfView, cameraView.aspect, distance), -zonePanRange, zonePanRange);
            HoldZoneCamera();
        }

        // Respawns what is due (all tiers), saves when anything regrew (or when asked), then redraws the six trees.
        private void RefreshZone(bool save = false)
        {
            if (zoneTier == 0) return;
            zoneRefreshAt = Time.unscaledTime + 1f;
            string regrew = RefreshLumber(lumber, LumberNow, ledger, PickupOpen ? pickupTree.Tier : 0, PickupOpen ? pickupTree.Index : 0, out bool changed);
            if (regrew != null) { ClosePickup(); zoneFeedbackText.text = regrew; }
            if (changed || save) SaveNow();
            foreach (ZoneTree z in zoneTrees) if (z != null && !(active != null && active.Root == z.Root) && z != fallingTree) ApplyTreeVisual(z);   // the chopped tree falls on its own
        }

        // Standing: upright, selectable, axe hint. Felled with piles: FallPivot at +85 degrees local X (toward +Z), selectable,
        // label. Stump: bole and crown hidden, LowerTrunk stays, label, not selectable.
        private void ApplyTreeVisual(ZoneTree z)
        {
            MassiveTree t = lumber.Tree(z.Tier, z.Index);
            bool stump = !t.Standing && t.Pending.Count == 0;
            if (z.Rig != null && z.Rig.FallPivot != null)
            {
                z.Rig.FallPivot.gameObject.SetActive(!stump);
                z.Rig.FallPivot.localRotation = t.Standing ? Quaternion.identity : Quaternion.Euler(85f, 0f, 0f);
            }
            foreach (Collider c in z.Root.GetComponentsInChildren<Collider>(true)) c.enabled = !stump;
            z.Hint.gameObject.SetActive(t.Standing);
            z.Label.transform.parent.gameObject.SetActive(!t.Standing);
            if (!t.Standing) z.Label.text = RegrowLabel(t, LumberNow);
        }

        // Per frame: the once-a-second refresh, the bobbing hints and the labels follow their trees on screen.
        private void UpdateZone()
        {
            if (zoneTier == 0 || cameraView == null) return;
            if (Time.unscaledTime >= zoneRefreshAt) RefreshZone();
            UpdatePickup();
            float scale = CanvasScale, bob = Mathf.Sin(Time.unscaledTime * 3f) * 8f * scale;
            foreach (ZoneTree z in zoneTrees)
            {
                if (z == null) continue;
                Transform top = z.Rig != null && z.Rig.HintAnchor != null ? z.Rig.HintAnchor : z.Root;
                Vector3 p = cameraView.WorldToScreenPoint(top.position);
                bool visible = !PickupOpen && p.z > 0f && p.x > 0f && p.x < Screen.width;   // the pickup shows only its piles' labels
                if (z.Hint.gameObject.activeSelf) { z.Hint.localScale = visible ? Vector3.one : Vector3.zero; z.Hint.position = new Vector3(p.x, p.y + 40f * scale + bob, 0f); }
                var plate = (RectTransform)z.Label.transform.parent;
                if (plate.gameObject.activeSelf)
                {
                    float half = plate.rect.width * 0.5f * scale;
                    plate.localScale = visible ? Vector3.one : Vector3.zero;
                    plate.position = new Vector3(Mathf.Clamp(p.x, half, Mathf.Max(half, Screen.width - half)), p.y + 30f * scale, 0f);
                }
            }
        }

        private void CreateZoneHud()
        {
            zoneHud = Rect("Lumber zone HUD", safe, 0, 0, 1, 1);
            zoneHud.SetAsFirstSibling();
            zoneFeedbackText = HudLine(zoneHud, 0.88f, 0.925f);
            Button(zoneHud, "+24 H", 0.02f, 0.80f, 0.37f, 0.86f, () =>
            {
                lumber.ClockOffset += System.TimeSpan.FromHours(24);   // debug (test phase), removed with HOUSE L8
                RefreshZone(true);
            });
            Sprite axe = ItemIconRenderer.For(ToolModel(ToolFamily.Axe));
            for (int i = 0; i < zoneHints.Length; i++)
            {
                RectTransform hint = Rect("Axe hint " + i, zoneHud, 0, 0, 0, 0); hint.sizeDelta = new Vector2(78f, 78f);
                Image image = hint.gameObject.AddComponent<Image>(); image.preserveAspect = true; image.raycastTarget = false;   // taps go to the tree (ZoneTap)
                if (axe != null) image.sprite = axe; else image.color = new Color(1f, 0.85f, 0.4f);
                var shadow = hint.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(0, 0, 0, 0.6f); shadow.effectDistance = new Vector2(2, -2);
                zoneHints[i] = hint;
                RectTransform plate = Rect("Regrow label " + i, zoneHud, 0, 0, 0, 0); plate.sizeDelta = new Vector2(230f, 58f);
                Image back = plate.gameObject.AddComponent<Image>(); back.color = new Color(0.08f, 0.07f, 0.05f, 0.7f); back.raycastTarget = false;
                Text label = Label(plate, "", 0.04f, 0.04f, 0.96f, 0.96f, 18);
                label.resizeTextForBestFit = true; label.resizeTextMinSize = 11; label.resizeTextMaxSize = 18;
                zoneLabels[i] = label;
            }
            zoneHud.gameObject.SetActive(false);
        }

        private void ZoneTap(Vector2 screen)
        {
            float best = LumberReach; ZoneTree pick = null;
            foreach (ZoneTree z in zoneTrees)
            {
                if (z == null) continue;
                MassiveTree t = lumber.Tree(z.Tier, z.Index);
                if (!t.Standing && t.Pending.Count == 0) continue;   // a stump is not tappable
                float d = LumberAnchorDistance(z.Rig != null && z.Rig.CutArea != null ? z.Rig.CutArea : z.Root, screen);   // CutArea sits at the base
                if (d <= best) { best = d; pick = z; }
            }
            if (LumberAnchorDistance(zoneExitAnchor, screen) <= best) { LeaveZone(); return; }
            if (pick == null) return;
            if (lumber.Tree(pick.Tier, pick.Index).Standing) StartMassiveCut(pick); else OpenPickup(pick);
        }

        // ---------------------------------------------------------------- massive chop (the trail's cut line, Harvest.cs)

        // A standing tree: the trail's chopping screen with MassiveActions at the zone tier and the up-front uses check
        // (TryStartHarvest). Harsh by design: Return, an app kill or the last eligible axe breaking discards the cut lines.
        private void StartMassiveCut(ZoneTree z)
        {
            if (harvestRoot == null) CreateHarvestOverlay();   // the trail builds it lazily; the settlement may come first
            TryStartHarvest(new HarvestTarget { AnchorId = "lumber.t" + z.Tier + ".tree" + z.Index, Kind = InteractableKind.Tree, Root = z.Root, Pine = z.Rig, Home = z.Root.position, MassiveTier = z.Tier, MassiveIndex = z.Index });
        }

        // The massive cut box: the bole at the cut radius from the ground to three times the cut height, so the swipe band (its
        // lower 55%, EvaluateCut) runs from the stump to well above the cut. The buttress roots are left out: at 3.8 m across
        // (pine) they pushed the framing back to 16 m and asked for a swipe wider than the trunk (phone round 2, Nov 20, 2026).
        public static Bounds MassiveCutBounds(Transform root)
        {
            var art = root.GetComponent<MassiveTreeArt>();
            float cut = art != null ? art.CutHeight : 1f, radius = art != null ? art.CutRadius : 0.7f;
            return new Bounds(root.position + Vector3.up * cut * 1.5f, new Vector3(2f * radius, 3f * cut, 2f * radius));
        }

        // Fits the box to about half the screen height (and 60% of its width), a little below the middle, so the cut zone
        // sits well above the system-gesture inset. Looks along `forward` (the zone camera's heading).
        public static Pose MassiveTrunkPose(Bounds b, Vector3 forward, float fieldOfView, float aspect)
        {
            Vector3 heading = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
            float tan = Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            float span = Mathf.Max(b.size.y / 0.5f, Mathf.Max(b.size.x, b.size.z) / (0.6f * aspect));   // visible height needed
            float distance = span / (2f * tan) + Mathf.Max(b.extents.x, b.extents.z);
            Vector3 look = b.center + Vector3.up * b.size.y * 0.2f;
            Vector3 eye = look - heading * distance + Vector3.up * distance * 0.15f;
            return new Pose(eye, Quaternion.LookRotation(look - eye));
        }

        private void FrameMassiveTrunk(Transform root)
        {
            if (StreetView == null || cameraView == null) return;
            Pose pose = MassiveTrunkPose(MassiveCutBounds(root), zoneCameraAnchor != null ? zoneCameraAnchor.forward : Vector3.back, cameraView.fieldOfView, cameraView.aspect);
            StreetView.HoldAt(pose.position, pose.rotation);
        }

        // The line is complete: commit the fell and save, then the eased fall (about 2 s), then the pickup. The screen stays open.
        private void CompleteMassive()
        {
            HarvestTarget target = active; ZoneTree z = zoneTrees[target.MassiveIndex];
            bool boosted = HarvestRules.Boosted(activeSpec, target.MassiveTier);
            if (!LumberFieldService.Fell(lumber, target.MassiveTier, target.MassiveIndex, boosted, LumberNow, out string reason)) { EndInteraction(); HarvestNote(true, reason); return; }
            statistics.AddInteraction("MassiveTree");
            SaveNow();
            foreach (CutLine line in cutLines) Destroy(line.View.gameObject);
            cutLines.Clear();
            flying = true;   // no more swipes; EndInteraction clears it
            stripText.text = WoodTitle(target.MassiveTier) + " tree felled" + (boosted ? " (Elite boost)" : "");
            StartCoroutine(FellMassive(target, z));
        }

        private ZoneTree fallingTree;   // the tree whose 2 s fall is easing: RefreshZone leaves it alone

        private IEnumerator FellMassive(HarvestTarget target, ZoneTree z)
        {
            fallingTree = z;
            Transform pivot = z != null && z.Rig != null ? z.Rig.FallPivot : null;
            for (float t = 0f; t < 1f && pivot != null; t += Time.deltaTime / 2f) { pivot.localRotation = Quaternion.Euler(85f * t * t, 0f, 0f); yield return null; }
            fallingTree = null;
            if (z == null || z.Root == null) yield break;   // the zone closed meanwhile
            ApplyTreeVisual(z);
            if (active == target) OpenPickup(z);
        }

        // Massive messages go to the zone HUD (shown again once the chop screen closes); trail ones to the trail feedback.
        private void HarvestNote(bool massive, string text) { if (!massive) Say(text); else if (!string.IsNullOrEmpty(text)) ShowSettlementNote(text); }   // "" after a fell: nothing to say

        // ---------------------------------------------------------------- pickup screen (a felled tree's piles)

        // The bag opens and the camera frames up to three piles beside the trunk near the stump. Each pile is one World entry
        // (the top bundle of its item, at most 1,000 units, best quality first) taken through the trail pickup path (Begin,
        // Commit), then LumberTaken. The counts stay in the domain; pile entries are never saved and every exit removes them.
        public const string PickupRegrewText = "The tree regrew; its leftovers expired.";
        private const string RetiredPiles = "lumber-retired";   // segment id that marks pile entries for RemoveWorldItems
        private ZoneTree pickupTree;
        private bool pickupBagWasOpen;
        private readonly Dictionary<string, Vector3> pileSpots = new Dictionary<string, Vector3>();
        private bool PickupOpen => pickupTree != null;

        public static string PileId(int tier, int index, int fellCount, string itemId, int quality) =>
            SessionSaveStore.LumberPilePrefix + "t" + tier + "-tree" + index + "-c" + fellCount + "-" + itemId + "-q" + quality;
        public static string PileLabel(string itemName, long bundles, int quality) =>
            itemName + " ×" + bundles.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " · Q" + quality;
        private static bool IsLumberPile(ItemEntry e) => e.Id.StartsWith(SessionSaveStore.LumberPilePrefix, System.StringComparison.Ordinal);
        // Units a pickup put in the bag: the whole entry unless part of it stayed on the ground.
        public static int Stored(ItemEntry item, int before) => item.Owner == ItemOwner.World ? before - item.Quantity : before;

        // The item's top bundle as a World entry, or null when the item is all taken. The id is PileId plus a unique suffix:
        // a ledger never reuses an id (consumed entries keep theirs), and the same top is materialized again on every reopen.
        public static ItemEntry MaterializePile(SpatialInventory ledger, MassiveTree t, int tier, int index, string itemId)
        {
            if (!LumberFieldService.Top(t, itemId, out int quality, out int units)) return null;
            ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(itemId);
            return ledger.AddWorld(PileId(tier, index, t.FellCount, itemId, quality) + "-" + System.Guid.NewGuid().ToString("N"), def, units, quality, ItemPrices.Profile(def, quality), true);
        }
        public static List<ItemEntry> MaterializePiles(SpatialInventory ledger, MassiveTree t, int tier, int index)
        {
            var piles = new List<ItemEntry>();
            foreach (string id in LumberFieldService.PileItems(t)) if (piles.Count < 3) piles.Add(MaterializePile(ledger, t, tier, index, id));
            return piles;
        }

        // Exact once: called right after the bag took `stored` units of the pile, before the one save that records both.
        // A partly stored pile stays with its remainder; a stored one is rekeyed in the bag and the item's next top is returned.
        public static ItemEntry TakePile(SpatialInventory ledger, MassiveTree t, int tier, int index, ItemEntry pile, int stored)
        {
            LumberFieldService.Take(t, pile.Definition.Id, pile.Quality, stored);
            if (pile.Owner == ItemOwner.World) return pile;
            if (pile.Owner == ItemOwner.Backpack) Rekey(ledger, pile);
            return MaterializePile(ledger, t, tier, index, pile.Definition.Id);
        }

        // A stored pile must not keep its lumber- id: World lumber- entries are never saved, so dropping it on the trail later
        // would lose it. Swap it for an identical entry with a fresh id at the same spot (its own footprint, so it always fits).
        private static void Rekey(SpatialInventory ledger, ItemEntry pile)
        {
            if (!RekeyInto(ledger, pile)) Debug.LogError("[Lumber] Could not move a stored pile to a bag entry; it stays on the pile.");
        }

        // False when the swap failed: the fresh entry is retired again so no stray wood- World entry is left.
        public static bool RekeyInto(SpatialInventory ledger, ItemEntry pile, bool forceFail = false)
        {
            ItemEntry fresh = ledger.AddWorld("wood-" + System.Guid.NewGuid().ToString("N"), pile.Definition, pile.Quantity, pile.Quality, pile.Profile, pile.Ready);
            int x = pile.X, y = pile.Y;
            ledger.Reserve(pile, int.MinValue); bool dropped = !forceFail && ledger.TryDropToTrail(pile, int.MinValue, 0f, out _, RetiredPiles); ledger.Release(pile, int.MinValue);
            bool placed = false;
            if (dropped)
            {
                ledger.RemoveWorldItems(RetiredPiles);
                ledger.Reserve(fresh, int.MinValue); placed = ledger.TryPlace(fresh, int.MinValue, x, y, false, out _); ledger.Release(fresh, int.MinValue);
            }
            if (!placed) { ledger.AssignSegment(fresh, RetiredPiles); ledger.RemoveWorldItems(RetiredPiles); }
            return placed;
        }

        // The trail's full-bag refusal talks about the trail; a lumber pile stays on its pile.
        public static string PileRefusal(ItemEntry item, string reason) =>
            IsLumberPile(item) && reason.StartsWith("Backpack full", System.StringComparison.Ordinal) ? "Backpack full: the rest stays on the pile" : reason;

        // Every unreserved World pile entry leaves the ledger (callers release held piles first).
        public static void ClearLumberPiles(SpatialInventory ledger)
        {
            foreach (ItemEntry e in ledger.Entries) if (e.Owner == ItemOwner.World && IsLumberPile(e)) ledger.AssignSegment(e, RetiredPiles);
            ledger.RemoveWorldItems(RetiredPiles);
        }

        // The ledger and scene part of every lumber exit (TearDownLumber: Return, the exit gate, ResetHomeView). Returns null for the holder field.
        public static Transform ClearLumberScene(SpatialInventory ledger, Transform holder)
        {
            ClearLumberPiles(ledger);
            if (holder != null) { if (Application.isPlaying) Destroy(holder.gameObject); else DestroyImmediate(holder.gameObject); }
            return null;
        }

        // The zone refresh: respawns what is due (changed: any tree regrew); when the open pickup's tree regrew, its piles
        // leave the ledger and the note says why. pickupTier 0: no pickup open.
        public static string RefreshLumber(LumberFieldState s, System.DateTime now, SpatialInventory ledger, int pickupTier, int pickupIndex, out bool changed)
        {
            changed = LumberFieldService.Refresh(s, now).Count > 0;
            if (pickupTier == 0 || !s.Tree(pickupTier, pickupIndex).Standing) return null;
            ClearLumberPiles(ledger);
            return PickupRegrewText;
        }

        private void OpenPickup(ZoneTree z)
        {
            if (z == null || z.Root == null || PickupOpen || !LumberOpen) return;
            if (active != null) EndInteraction();   // the chopping screen turns into the pickup screen
            MassiveTree t = lumber.Tree(z.Tier, z.Index);
            if (t.Standing || t.Pending.Count == 0) return;
            CancelSettlementPointer();
            pickupTree = z; zoneFeedbackText.text = "";
            pickupBagWasOpen = inventoryOpen;
            if (!inventoryOpen) ToggleInventory();
            // Along the trunk (it lies toward +Z), a little to its side, in PileItems order (branch, log, ingredient).
            pileSpots.Clear();
            var bounds = new List<Bounds>();
            foreach (ItemEntry pile in MaterializePiles(ledger, t, z.Tier, z.Index))
            {
                Vector3 spot = z.Root.position + new Vector3(2.4f, 0.45f, 1.5f + 2.2f * pileSpots.Count);   // 0.45 m up: the trail's pickup height
                pileSpots[pile.Definition.Id] = spot; AddPile(pile);
                bounds.Add(new Bounds(spot + Vector3.up * 0.2f, Vector3.one * 1.4f));
            }
            FramePickup(bounds);
            ShowScreenReturn(zoneHud.gameObject, ClosePickup);
        }

        // From the trunk's side, the art's PickupCameraAnchor offset (it frames TreeAnchor2 looking down -Z) turned to look along
        // -X at the piles, then the warehouse fit: pitch and dolly so every pile sits above the open bag.
        private void FramePickup(List<Bounds> piles)
        {
            if (StreetView == null || cameraView == null || piles.Count == 0) return;
            Transform anchor = FindChild(lumberHolder, "PickupCameraAnchor"), origin = FindChild(lumberHolder, "TreeAnchor2");
            Vector3 offset = anchor != null && origin != null ? anchor.position - origin.position : new Vector3(0f, 1.6f, 7f);
            Quaternion look = anchor != null ? anchor.rotation : Quaternion.LookRotation(Vector3.back), turn = Quaternion.Euler(0f, 90f, 0f);
            Vector3 centre = Vector3.zero; foreach (Bounds b in piles) centre += b.center / piles.Count;
            StreetView.HoldAt(centre + turn * offset, turn * look);
            bagFrame.GetWorldCorners(warehouseCorners);   // overlay canvas: world corners are screen pixels
            Pose pose = WarehouseCameraPose(cameraView, piles.ToArray(), Screen.height > 0 ? warehouseCorners[1].y / Screen.height : 0.3f);
            StreetView.HoldAt(pose.position, pose.rotation);
        }

        private void AddPile(ItemEntry pile)
        {
            if (!pileSpots.TryGetValue(pile.Definition.Id, out Vector3 spot)) spot = pickupTree.Root.position;
            AddBundle(pile, spot);
            Bundle view = bundles[bundles.Count - 1];
            view.Badge.horizontalOverflow = HorizontalWrapMode.Overflow; view.Badge.fontStyle = FontStyle.Bold;
            Outline edge = view.Badge.gameObject.AddComponent<Outline>(); edge.effectColor = new Color(0, 0, 0, 0.85f); edge.effectDistance = new Vector2(1.2f, -1.2f);
            LabelPile(view);
        }

        // "Pine log ×147 · Q5": the item's bundles across qualities, the quality of the top one. A non-empty badge shows (Update).
        private void LabelPile(Bundle view)
        {
            if (view == null || pickupTree == null) return;
            view.Badge.text = PileLabel(view.Item.Definition.Name, LumberFieldService.Bundles(lumber.Tree(pickupTree.Tier, pickupTree.Index), view.Item.Definition.Id), view.Item.Quality);
        }

        private void RemoveBundle(Bundle view) { Destroy(view.View); Destroy(view.Badge.gameObject); bundles.Remove(view); }

        // After a successful Commit of a pile (CollectionPrototype.cs): Take, then the one save, then the pile's next top.
        private void LumberTaken(ItemEntry pile, int storedUnits)
        {
            if (pickupTree == null) { Debug.LogError("[Lumber] A pile was stored with no pickup open: " + pile.Id); return; }   // ClosePickup releases held piles first
            ZoneTree z = pickupTree;
            Bundle view = bundles.Find(b => b.Item == pile);
            ItemEntry next = TakePile(ledger, lumber.Tree(z.Tier, z.Index), z.Tier, z.Index, pile, storedUnits);
            SaveNow();
            if (next == pile) { LabelPile(view); return; }
            if (view != null) RemoveBundle(view);
            if (next != null) AddPile(next);
        }

        // Return (back to the zone), a regrow while open, and every lumber teardown.
        private void ClosePickup()
        {
            if (!PickupOpen) return;
            foreach (KeyValuePair<int, Contact> held in new List<KeyValuePair<int, Contact>>(contacts)) if (held.Value.World && IsLumberPile(held.Value.Item)) Release(held.Key, held.Value);
            foreach (Bundle view in bundles.FindAll(b => IsLumberPile(b.Item))) RemoveBundle(view);
            ClearLumberPiles(ledger);
            pickupTree = null; pileSpots.Clear();
            if (!pickupBagWasOpen && inventoryOpen) { CloseContainer(); movement.CloseInventory(); inventoryOpen = false; LayoutBackpack(); }
            if (zoneTier == 0) return;
            HoldZoneCamera();
            ShowScreenReturn(zoneHud.gameObject, LeaveZone);
        }

        // In the settlement, ahead of the street and zone touch: a touch within reach (48 dp) of a pile's projected centre holds it
        // as a World contact (swipe it down or drag it into the bag); anything else is swallowed while the pickup owns the screen.
        private bool PickupPointerBegin(int pointer, Vector2 position)
        {
            if (!PickupOpen) return false;
            if (saveBlocked || cameraView == null || (stamina != null && stamina.Fainted)) return true;
            Bundle pick = null; float best = LumberReach;
            foreach (Bundle b in bundles)
            {
                if (b.Item.Owner != ItemOwner.World || !IsLumberPile(b.Item)) continue;
                Vector3 p = cameraView.WorldToScreenPoint(b.Collider.bounds.center);
                float d = p.z <= 0f ? float.MaxValue : Vector2.Distance(position, p);
                if (d <= best) { best = d; pick = b; }
            }
            if (pick == null) return true;
            if (!ledger.Reserve(pick.Item, pointer)) { Say("That pickup is already held"); return true; }
            contacts[pointer] = new Contact { Item = pick.Item, Bundle = pick, World = true, Start = position, Position = position, Started = Time.unscaledTime };
            return true;
        }

        // Per frame (UpdateZone): the pile labels follow their piles.
        private void UpdatePickup()
        {
            if (!PickupOpen || cameraView == null) return;
            foreach (Bundle b in bundles)
            {
                if (!IsLumberPile(b.Item)) continue;
                Vector3 p = cameraView.WorldToScreenPoint(b.Collider.bounds.max);
                b.Badge.transform.localScale = p.z > 0f ? Vector3.one : Vector3.zero;
                b.Badge.transform.position = new Vector3(p.x, p.y + 50f * CanvasScale, 0f);   // the text sits at the bottom of its 82 px box
            }
        }
    }
}
