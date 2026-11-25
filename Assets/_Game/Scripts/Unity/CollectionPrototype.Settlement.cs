using System.Collections.Generic;
using System.Text;
using TheTrailWalker.Domain;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Settlement shell (milestone 10 part 1, spec 2026-11-08): the HUD controller owns the settlement state and saves/restores it
    // with the session. In the settlement scene it runs the settlement context: the street camera's drag and tap, the icon
    // list (Home, Warehouse, Lumber, the five shops, Return), paper notes for tapped buildings, the first-visit plot choice and Return.
    public sealed partial class CollectionPrototype
    {
        private SettlementState settlement = SettlementState.New(0);

        // First home (spec 2026-11-13): the player's house level and furniture, saved and restored with the session.
        private TheTrailWalker.Domain.Settlement.HomeState home = TheTrailWalker.Domain.Settlement.HomeState.New();
        private TheTrailWalker.Domain.Settlement.HomeStorage homeStorage = new TheTrailWalker.Domain.Settlement.HomeStorage();

        // Set by the load-time migration; the startup code saves and tells the player once the prototype is initialized.
        private bool houseMigrationPending;

        private SavedSettlement SaveSettlementForSnapshot() => SessionSaveStore.SaveSettlement(settlement);
        private SavedHome SaveHomeForSnapshot()
        {
            SavedHome saved = SessionSaveStore.SaveHome(home);
            saved.containers = SessionSaveStore.SaveContainers(homeStorage);
            return saved;
        }

        // A save without a settlement block gets a fresh state whose seed is derived from the save, so it is deterministic per
        // profile: FNV-1a over the travel visit number and the current camp id.
        private void RestoreSettlementFromSnapshot(SessionSnapshot snapshot)
        {
            settlement = SessionSaveStore.RestoreSettlement(snapshot, SettlementSeed(snapshot));
            home = SessionSaveStore.RestoreHome(snapshot);
            homeStorage = SessionSaveStore.RestoreContainers(snapshot, home);
            // Slice 2b (spec section 6): deposits the next level can't use (unrequired, above the requirement, at the cap) go
            // to the overflow as whole entries instead of refusing the save. During the startup restore SaveNow is a no-op
            // (not yet initialized), so the move is flagged and the startup code saves and tells the player right after initialization.
            int moved = TheTrailWalker.Domain.Settlement.HouseUpgradeService.MigrateSurplus(home, ledger);
            if (moved > 0) { lastSavedState = null; houseMigrationPending = true; }
        }

        public static int SettlementSeed(SessionSnapshot snapshot) => SettlementSeed(snapshot?.travel?.visitNumber ?? 0, snapshot?.travel?.campId);

        // The same derivation from the live travel state (visit counter and current camp), used on the first arrival.
        public static int SettlementSeed(int visit, string campId)
        {
            uint hash = 2166136261u;
            string key = visit + "|" + (campId ?? "");
            foreach (byte b in Encoding.UTF8.GetBytes(key)) { hash ^= b; hash *= 16777619u; }
            return (int)(hash & 0x7FFFFFFF);
        }

        // ---------------------------------------------------------------- texts (verbatim, spec Global Constraints)

        public const string SettlementPlotPrompt = "Build your home here? This can't be changed.";
        public const string SettlementWelcome = "Welcome to the settlement.";
        public static string SettlementReturnPrompt(string camp) => "Return to " + camp + "?";

        // Public (not internal) so the Editor checks assembly can call it, like SettlementSeed.
        public static string SettlementNote(StreetBuilding b, int level, bool isPlayerPlot)
        {
            switch (b.Kind)
            {
                case StreetBuildingKind.House: return isPlayerPlot ? "Your home" : $"A neighbour's home (level {level})";   // the player's plot opens the home instead (task 5)
                case StreetBuildingKind.Warehouse: return "The warehouse";
                case StreetBuildingKind.LumberField: return "The lumber field";   // the building and its list entry open the counter instead (OpensLumberCounter)
                default: return ShopDisplayName(b.Id) + " opens in Milestone 14";
            }
        }
        // "shop.bakery" -> "Bakery" (the manifest names: Bakery, Ranch, Farm, Fishery, Mine, Tailor).
        private static string ShopDisplayName(string id)
        {
            string name = id != null && id.StartsWith("shop.", System.StringComparison.Ordinal) ? id.Substring(5) : id ?? "";
            return name.Length == 0 ? "The shop" : char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        // Speed control, pickups and harvest, the route card, the trade note, Craft and the debug gold grant are trail-only.
        public static bool TrailControlsVisible(bool settlement) => !settlement;

        // Until a plot is chosen only Return shows.
        public static string[] SettlementListKeys(bool plotChosen) => plotChosen ? new[] { "home", "warehouse", "lumber", "bakery", "farm", "mine", "tailor", "ranch", "fishery", "return" } : new[] { "return" };
        // Slot (row from the top) of a list button, or -1 when hidden: visible buttons stack contiguously from the corner.
        public static int SettlementListSlot(string key, bool plotChosen) => System.Array.IndexOf(SettlementListKeys(plotChosen), key);
        // At a camp (phone round R3) the list travels there: the same places minus Return, or a lone Settlement entry before the plot choice.
        public static string[] CampListKeys(bool plotChosen) => plotChosen ? System.Array.FindAll(SettlementListKeys(true), k => k != "return") : new[] { "settlement" };
        // The map card's old conditions: seated at a camp with the settlement discovered, never in it, and hidden under the camp's panels.
        public static bool CampListVisible(bool discovered, bool seated, bool inSettlement, bool covered) => discovered && seated && !inSettlement && !covered;
        // The place tapped at a camp opens on arrival only onto a clear street; otherwise the player stays on the street.
        public static bool SettlementOpensOnEntry(bool noteShowing, bool plotChoice, bool covered) => !noteShowing && !plotChoice && !covered;

        // Arrival (arch, map shortcut, restart inside): a never-discovered state (a new profile's placeholder seed 0) is replaced
        // by one seeded from the live visit and camp, then discovered; regions found since the last visit are applied (idempotent).
        public static SettlementState SettlementArrival(SettlementState state, int visit, string campId, IEnumerable<string> discoveredRegions)
        {
            if (state == null || !state.Discovered) state = SettlementState.New(SettlementSeed(visit, campId));
            state.Discover();
            state.ApplyNewRegions(discoveredRegions);
            return state;
        }

        // Metres the street camera moves for a horizontal drag: the facade plane follows the finger.
        public static float StreetDragMetres(float dragPixels, float screenWidth, float verticalFov, float aspect, float distance)
        {
            if (screenWidth <= 0f) return 0f;
            float visibleWidth = 2f * distance * Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad) * aspect;
            return -dragPixels / screenWidth * visibleWidth;
        }

        // ---------------------------------------------------------------- context

        // Set before the reload that enters through the arch (or the map shortcut), so the next scene shows the welcome note.
        // A real app start (a restart inside the settlement) begins false: no welcome note, but arrival still runs.
        private static bool settlementWelcomePending;
        private bool settlementEntered;
        private string settlementOpenNext;   // the camp list's place for this scene's first settlement frame (null: the street)
        private bool InSettlementScene => milestone5 && scene != null && scene.Settlement;
        private SettlementStreet Street { get { if (world == null) world = GetComponent<MovementPrototype>(); return world != null ? world.Street : null; } }
        private StreetCamera StreetView { get { if (world == null) world = GetComponent<MovementPrototype>(); return world != null ? world.StreetView : null; } }

        private void ApplySettlementArrival() => settlement = SettlementArrival(settlement, travel.VisitNumber, travel.CurrentCampId, travel.DiscoveredRegions);

        // ApplyTravelPose in the settlement: no walker, seated-style stamina regeneration (the Seated pause marks a safe area).
        private void EnterSettlementContext()
        {
            phase = TravelPhase.Settlement; motor.Detached = true;
            movement.SetPause(MovementPauseReasons.Seated, true);
            ResetHomeView();   // a context (re)entry always starts on the street
            if (settlementEntered) return;
            settlementEntered = true;
            settlementOpenNext = TakeSettlementOpen();   // the camp list's place, opened on the first settlement frame (UpdateSettlement)
            ApplySettlementArrival(); lastSavedState = null; SaveNow();
            if (settlementWelcomePending) { settlementWelcomePending = false; ShowSettlementNote(SettlementWelcome); }
        }

        // Walking into the arch goes straight in; the arrival is applied before the save, so the next scene's house levels are current.
        private void EnterSettlementThroughArch()
        {
            if (archShown) return;
            archShown = true;
            settlementWelcomePending = true;
            CommitTravel(() => { string rolled = travel.ReachSettlementArch(); ApplySettlementArrival(); AdvanceShopVisit(); return rolled; }, SettlementWelcome);
            if (saveBlocked) settlementWelcomePending = false;
        }

        // ---------------------------------------------------------------- HUD

        private RectTransform settlementList, plotOverlay;
        private readonly Dictionary<string, RectTransform> settlementButtons = new Dictionary<string, RectTransform>();
        private readonly List<(StreetBuilding Building, RectTransform Marker)> plotMarkers = new List<(StreetBuilding, RectTransform)>();

        // Debug house-level preview: 0 shows the save's levels, 1..MaxLevel shows every plot at that level.
        private int houseLevelPreview;
        private Text houseLevelButton;
        private string HouseLevelCaption => houseLevelPreview == 0 ? "HOUSES: SAVE" : "HOUSES: L" + houseLevelPreview;
        private void CycleHouseLevelPreview()
        {
            if (world == null) world = GetComponent<MovementPrototype>();
            if (world == null || world.Street == null) return;
            houseLevelPreview = (houseLevelPreview + 1) % (SettlementState.MaxLevel + 1);
            int[] levels = SettlementScenery.HouseLevels(settlement, home.Level);
            if (houseLevelPreview > 0) for (int i = 0; i < levels.Length; i++) levels[i] = houseLevelPreview;
            RebuildStreetWith(levels);
            if (houseLevelButton != null) houseLevelButton.text = HouseLevelCaption;
            Say(houseLevelPreview == 0 ? "Debug: houses show the saved levels again." : $"Debug preview: every house at level {houseLevelPreview} (not saved).");
        }

        // Rebuilds the street with these house levels and re-points the plot markers at the new houses (by plot). Shared by the
        // debug level preview and the house upgrade's completion.
        private void RebuildStreetWith(int[] levels)
        {
            if (world == null) world = GetComponent<MovementPrototype>();
            if (world == null || world.Street == null) return;
            world.RebuildStreet(levels, SettlementScenery.PlayerPlot(settlement), Domain.Settlement.ShopService.ReachedTier(shops, ShopRegions()));
            for (int i = 0; i < plotMarkers.Count; i++)
                foreach (StreetBuilding b in world.Street.Buildings)
                    if (b.Kind == StreetBuildingKind.House && b.Plot == plotMarkers[i].Building.Plot) { plotMarkers[i] = (b, plotMarkers[i].Marker); break; }
        }

        // Debug deposit (slice 2b): fills the next level's missing materials from nothing, so the whole upgrade can be walked through.
        public const string DebugDepositCaption = "+ DEPOSIT";
        private Text houseDepositButton;
        private void DebugDeposit()
        {
            if (saveBlocked) return;
            var grants = TheTrailWalker.Domain.Settlement.HouseUpgradeService.DebugGrant(home, ledger);
            if (grants.Count == 0) { Say("The house is at its highest level for now."); return; }
            SaveNow(true);
            var parts = new List<string>();
            foreach (var g in grants)
            {
                TheTrailWalker.Domain.Items.ItemDefinition d = TheTrailWalker.Domain.Crafting.Items.Find(g.itemId);
                parts.Add(g.quantity + " " + (d != null ? d.Name : g.itemId));
            }
            Say("Debug: added " + string.Join(", ", parts) + ".");
        }
        // Debug HOUSE L8 (Milestone 11 test phase): returns the deposits to the bag and grows the house to level 8 so every chest size can
        // be bought. The button is visible exactly when + DEPOSIT is (never inside a home view), so only the street needs rebuilding.
        public const string DebugJumpCaption = "HOUSE L8";
        private Text houseJumpButton, shopTierButton;
        private void DebugJumpHouse()
        {
            if (saveBlocked || HomeOpen) return;
            TheTrailWalker.Domain.Settlement.HouseUpgradeService.DebugJumpToLevel(home, ledger, TheTrailWalker.Domain.Settlement.HomeState.MaxLevel);
            SaveNow(true);
            if (UpgradeSheetOpen) CloseUpgradeSheet();
            houseLevelPreview = 0;
            if (houseLevelButton != null) houseLevelButton.text = HouseLevelCaption;
            RebuildStreetWith(SettlementScenery.HouseLevels(settlement, home.Level));
            Say("Debug: house set to level 8.");
        }
        private GameObject settlementNote, settlementConfirm;
        private Text settlementNoteText, settlementConfirmText;
        private System.Action settlementConfirmed;
        private float settlementNoteHideAt;
        private const float SettlementNoteSeconds = 3.5f, SettlementListTop = 0.73f; // just below the feedback line for now; the final position is the top-right corner once the debug HUD goes (user, November 9, 2026)
        private int settlementListLayout = -1; // the (open, chosen) state the list was last laid out for (-1: never)
        private float settlementListHeight;    // and the safe-area height it was laid out for
        // Settlement menu (user, November 9, 2026): like the user menu, one settlement button opens the list downward.
        private RectTransform settlementMenuButton;
        private bool settlementListOpen;
        public static float SettlementListRowY(int slot, float size = HudSquare, float step = HudSquare + HudGap) => -(HudSquare + step - size + slot * step);   // a row's top below the settlement button's top
        // Row size and pitch (canvas units) for `rows` entries in a safe area `safeHeight` tall: the full 84 + 10 rows when they
        // fit above the closed-bag MENU square, otherwise the pitch shrinks evenly (the gap first, down to 6) so the lowest row
        // still ends a gap above it (nine entries on the 1116x2480 phone, Milestone 14). A row never shrinks below minSize (the
        // 48 dp tap target, canvas units) while the gaps can give way: then the gaps share what is left, down to touching (ten
        // entries on a 720x1680 320 dpi screen, slice 2).
        public static void SettlementListRows(float safeHeight, int rows, out float size, out float step, float minSize = 0f)
        {
            float room = (SettlementListTop - ClosedBarLine) * safeHeight - 2f * HudSquare;   // settlement button's bottom to the MENU square's top
            rows = Mathf.Max(1, rows);
            step = Mathf.Min(HudSquare + HudGap, (room - HudGap) / rows);
            size = Mathf.Min(HudSquare, step - 6f);
            if (size >= minSize) return;
            size = Mathf.Min(HudSquare, minSize, room / rows);
            step = size + (room - rows * size) / (rows + 1);   // equal gaps above the first row, between rows and above the MENU square
        }
        // A list row's and the MENU square's rects in safe-area canvas units (origin bottom left), for the overlap check.
        public static Rect SettlementListRowRect(float safeWidth, float safeHeight, int slot, int rows, float minSize = 0f)
        {
            SettlementListRows(safeHeight, rows, out float size, out float step, minSize);
            float top = SettlementListTop * safeHeight + SettlementListRowY(slot, size, step);
            return new Rect(safeWidth - HudMargin - size, top - size, size, size);
        }
        public static Rect UserMenuSquareRect(float safeWidth, float safeHeight) => new Rect(safeWidth - HudMargin - HudSquare, ClosedBarLine * safeHeight, HudSquare, HudSquare);   // PlaceSquare(userButton, right, ClosedBarLine, 0, 0)
        // Paper and strips stop this far (canvas units) from the safe area's right edge, clear of the list column at any aspect.
        private static float SettlementListClearance => HudMargin + HudSquare + HudGap;
        private static void ClearOfSettlementList(RectTransform rect) { rect.anchorMax = new Vector2(1f, rect.anchorMax.y); rect.offsetMax = new Vector2(-SettlementListClearance, rect.offsetMax.y); }

        // Icon-only list from the upper-right corner downward; captions stay on the hidden labels (accessibility names). Built in the
        // settlement and at camps (phone round R3), where a lone Settlement entry with the house icon stands in before the plot choice.
        private void CreateSettlementList()
        {
            claimOverflow = safe.Find("CLAIM OVERFLOW") as RectTransform;
            settlementList = Rect("Settlement list", safe, 0, 0, 1, 1);
            // The settlement square opens the list and closes the user menu: the two share the right-hand column (LayoutSettlementList).
            Text menuLabel = Button(settlementList, "Settlement", 0, 0, 0, 0, () => { settlementListOpen = !settlementListOpen; if (settlementListOpen) SetMenu(false); settlementListLayout = -1; });
            HudIconStyle.Apply(menuLabel, "settlement");
            settlementMenuButton = (RectTransform)menuLabel.transform.parent; settlementMenuButton.name = "Settlement menu";
            settlementMenuButton.anchorMin = settlementMenuButton.anchorMax = new Vector2(1f, SettlementListTop); settlementMenuButton.pivot = new Vector2(1f, 1f);
            settlementMenuButton.sizeDelta = new Vector2(HudSquare, HudSquare); settlementMenuButton.anchoredPosition = new Vector2(-HudMargin, 0f);
            var captions = new Dictionary<string, string> { { "home", "Home" }, { "warehouse", "Warehouse" }, { "lumber", "Lumber" }, { "bakery", "Bakery" },
                { "farm", "Farm" }, { "mine", "Mine" }, { "tailor", "Tailor" }, { "ranch", "Ranch" }, { "fishery", "Fishery" }, { "return", "Return" }, { "settlement", "Settlement" } };
            foreach (string key in captions.Keys)
            {
                string captured = key;
                Text label = Button(settlementList, captions[key], 0, 0, 0, 0, () => SettlementListTapped(captured));
                HudIconStyle.Apply(label, key == "settlement" ? "home" : key);
                var rect = (RectTransform)label.transform.parent; rect.name = "Settlement list: " + captions[key];
                rect.anchorMin = rect.anchorMax = new Vector2(1f, SettlementListTop); rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(HudSquare, HudSquare);
                settlementButtons[key] = rect;
            }
            settlementList.gameObject.SetActive(false);
        }

        private void CreateSettlementHud()
        {
            CreateSettlementList();
            if (!InSettlementScene) return;
            router.SpeedAreaEnabled = false;   // no walking: the street scrolls instead (see SettlementPointerBegin)
            // The debug CLAIM OVERFLOW button would sit under the list column; in the settlement it moves one column left.
            if (safe.Find("CLAIM OVERFLOW") is RectTransform claim) { claim.anchorMin = new Vector2(0.47f, claim.anchorMin.y); claim.anchorMax = new Vector2(0.72f, claim.anchorMax.y); }
            // Debug house-level preview (user, November 13, 2026): cycles every plot through levels 1..MaxLevel, then back to
            // the save's levels. View only: nothing is saved, and leaving the settlement shows the saved street again.
            houseLevelButton = Button(safe, HouseLevelCaption, 0.47f, 0.61f, 0.72f, 0.66f, CycleHouseLevelPreview);
            houseDepositButton = Button(safe, DebugDepositCaption, 0.47f, 0.67f, 0.72f, 0.72f, DebugDeposit);
            houseJumpButton = Button(safe, DebugJumpCaption, 0.47f, 0.49f, 0.72f, 0.54f, DebugJumpHouse);   // the next free row (0.55 holds CLAIM OVERFLOW)
            shopTierButton = Button(safe, "TIER +1", 0.47f, 0.43f, 0.72f, 0.48f, DebugShopTier);   // debug (test phase), removed with HOUSE L8

            // Plot choice overlay: a marker over each plot and a paper strip with the instruction.
            plotOverlay = Rect("Plot choice", safe, 0, 0, 1, 1);
            RectTransform strip = Rect("Plot choice note", plotOverlay, 0.04f, 0.655f, 1f, 0.725f); ClearOfSettlementList(strip);
            Sheet("Paper", strip, "paper_1", 0f, 0f, 1f, 1f, 0.8f).raycastTarget = false;
            Text hint = Label(strip, "Choose a plot for your home: tap one of the marked houses.", 0.06f, 0.08f, 0.94f, 0.92f, 22);
            hint.color = Ink; hint.fontStyle = FontStyle.Bold; hint.resizeTextForBestFit = true; hint.resizeTextMinSize = 11; hint.resizeTextMaxSize = 22;
            SettlementStreet street = Street;
            if (street != null)
                foreach (StreetBuilding b in street.Buildings)
                {
                    if (b.Kind != StreetBuildingKind.House) continue;
                    RectTransform marker = Rect("Plot marker " + b.Plot, plotOverlay, 0, 0, 0, 0);
                    marker.sizeDelta = new Vector2(HudSquare, HudSquare);
                    Image plate = marker.gameObject.AddComponent<Image>(); plate.raycastTarget = false;   // taps reach the house behind it
                    HudIconStyle.ApplyTo(marker, "home", true);
                    marker.gameObject.SetActive(false);
                    plotMarkers.Add((b, marker));
                }
            plotOverlay.gameObject.SetActive(false);

            // Paper note for a tapped building or the welcome; it closes by itself or when tapped.
            RectTransform noteRect = Rect("Settlement note", safe, StreetNoteBand.x, StreetNoteBand.y, StreetNoteBand.z, StreetNoteBand.w); ClearOfSettlementList(noteRect);   // taps on the list never land on the note (PlaceSettlementNote moves it per view)
            settlementNote = noteRect.gameObject;
            Sheet("Paper", settlementNote.transform, "paper_0", 0f, 0f, 1f, 1f, -1.2f);
            settlementNoteText = Label(settlementNote.transform, "", 0.08f, 0.12f, 0.92f, 0.88f, 26);
            settlementNoteText.color = Ink; settlementNoteText.fontStyle = FontStyle.Bold;
            settlementNoteText.resizeTextForBestFit = true; settlementNoteText.resizeTextMinSize = 12; settlementNoteText.resizeTextMaxSize = 26;
            var dismiss = settlementNote.AddComponent<UnityEngine.UI.Button>(); dismiss.transition = Selectable.Transition.None;
            dismiss.onClick.AddListener(() => settlementNote.SetActive(false));
            settlementNote.SetActive(false);

            // Paper confirmation (plot choice and Return), like the ferry and arch notes.
            settlementConfirm = Rect("Settlement confirmation", safe, 0.10f, 0.38f, 0.90f, 0.62f).gameObject;
            Sheet("Paper", settlementConfirm.transform, "paper_0", 0f, 0f, 1f, 1f, -1.5f);
            settlementConfirmText = Label(settlementConfirm.transform, "", 0.10f, 0.42f, 0.90f, 0.88f, 26);
            settlementConfirmText.color = Ink; settlementConfirmText.fontStyle = FontStyle.Bold;
            settlementConfirmText.resizeTextForBestFit = true; settlementConfirmText.resizeTextMinSize = 12; settlementConfirmText.resizeTextMaxSize = 26;
            Button(settlementConfirm.transform, "YES", 0.12f, 0.10f, 0.46f, 0.34f, () => CloseSettlementConfirm(true));
            Button(settlementConfirm.transform, "NO", 0.54f, 0.10f, 0.88f, 0.34f, () => CloseSettlementConfirm(false));
            settlementConfirm.SetActive(false);

            CreateHomeHud();   // the inside-house HUD (CollectionPrototype.Home.cs)
        }

        // The settlement list, plot overlay and street taps hide behind panels, the faint, a confirmation and the home views;
        // the home HUD itself only behind the panels (SettlementPanelsCovered).
        private bool SettlementCovered => SettlementPanelsCovered || HomeOpen;
        private bool SettlementPanelsCovered => inventoryOpen || saveBlocked || (stamina != null && stamina.Fainted)
            || (gearPanel != null && gearPanel.activeSelf) || (mapPanel != null && mapPanel.activeSelf) || (missionPanel != null && missionPanel.activeSelf)
            || (craftPanel != null && craftPanel.activeSelf) || (settlementConfirm != null && settlementConfirm.activeSelf);
        private bool PlotChoiceOpen => settlement.Discovered && settlement.PlotIndex < 0;

        // Per frame while phase == Settlement (from UpdateTravel).
        private void UpdateSettlement()
        {
            if (settlementList == null) return;
            bool covered = SettlementCovered;
            settlementList.gameObject.SetActive(!covered);
            if (houseLevelButton != null)
            {
                GameObject debug = houseLevelButton.transform.parent.gameObject;
                bool visible = HouseDebugButtonVisible(HomeOpen);
                if (debug.activeSelf != visible) debug.SetActive(visible);
                if (houseDepositButton != null) { GameObject deposit = houseDepositButton.transform.parent.gameObject; if (deposit.activeSelf != visible) deposit.SetActive(visible); }
                if (houseJumpButton != null) { GameObject jump = houseJumpButton.transform.parent.gameObject; if (jump.activeSelf != visible) jump.SetActive(visible); }
                if (shopTierButton != null) { GameObject tier = shopTierButton.transform.parent.gameObject; if (tier.activeSelf != visible) tier.SetActive(visible); }
            }
            if (!covered) LayoutSettlementList(false, settlement.PlotIndex >= 0);
            bool overlay = PlotChoiceOpen && !covered;
            if (plotOverlay.gameObject.activeSelf != overlay) plotOverlay.gameObject.SetActive(overlay);
            if (overlay)
            {
                float pulse = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 4f);
                foreach ((StreetBuilding b, RectTransform marker) in plotMarkers)
                {
                    var box = b.Root.GetComponent<BoxCollider>();
                    Vector3 anchor = box != null ? b.Root.TransformPoint(box.center) : b.Root.position + Vector3.up * 3f;
                    Vector3 screen = cameraView.WorldToScreenPoint(anchor);
                    bool show = screen.z > 0f && screen.x > 0f && screen.x < Screen.width && screen.y > 0f && screen.y < Screen.height;
                    if (marker.gameObject.activeSelf != show) marker.gameObject.SetActive(show);
                    if (show) { marker.position = new Vector3(screen.x, screen.y, 0f); marker.localScale = Vector3.one * pulse; }
                }
            }
            if (settlementOpenNext != null)
            {
                string place = settlementOpenNext; settlementOpenNext = null;   // once: a welcome or migration note, the plot choice or a panel keeps the street
                if (SettlementOpensOnEntry(settlementNote.activeSelf, PlotChoiceOpen, covered)) SettlementListTapped(place);
            }
            UpdateHome(SettlementPanelsCovered);
            UpdateWarehouse();
            UpdateLumber();
            UpdateShops();
            if (settlementNote.activeSelf && Time.unscaledTime >= settlementNoteHideAt) settlementNote.SetActive(false);
        }

        // Re-lays out the list whenever the visible set changes: the open list stacks under the settlement button from slot 0 (the
        // settlement's keys, or the camp's; before the plot choice a lone Return or Settlement entry takes the first row).
        private void LayoutSettlementList(bool camp, bool chosen)
        {
            if (menuOpen && settlementListOpen) settlementListOpen = false;   // the user menu took the right-hand column
            int layout = (chosen ? 1 : 0) + (settlementListOpen ? 2 : 0);
            if (settlementListLayout == layout && settlementListHeight == safe.rect.height) return;
            settlementListLayout = layout; settlementListHeight = safe.rect.height;
            string[] keys = camp ? CampListKeys(chosen) : SettlementListKeys(chosen);
            SettlementListRows(settlementListHeight, keys.Length, out float size, out float step, 48f * (Screen.dpi > 0f ? Screen.dpi : 160f) / 160f / CanvasScale);
            foreach (KeyValuePair<string, RectTransform> pair in settlementButtons)
            {
                int slot = settlementListOpen ? System.Array.IndexOf(keys, pair.Key) : -1;
                pair.Value.gameObject.SetActive(slot >= 0);
                if (slot >= 0) { pair.Value.sizeDelta = new Vector2(size, size); pair.Value.anchoredPosition = new Vector2(-HudMargin, SettlementListRowY(slot, size, step)); }
            }
        }

        // ---------------------------------------------------------------- the list at camps (phone round R3)

        // Hidden under the same panels as the rest of the camp HUD: the bag, map, crafting, gear, mission papers, the trade, the toll, a faint.
        private bool CampListCovered => SettlementPanelsCovered || TradePanelOpen || (tollPanel != null && tollPanel.activeSelf) || (missionShow != null && missionShow.activeSelf) || BalloonOpen;
        // The debug CLAIM OVERFLOW button steps one column left while the list holds the right column, as in the settlement.
        public static readonly UnityEngine.Rect ClaimOverflowAsideBand = UnityEngine.Rect.MinMaxRect(0.47f, 0.55f, 0.72f, 0.60f);
        private RectTransform claimOverflow;

        // Per frame outside the settlement (from UpdateTravel).
        private void UpdateCampList()
        {
            if (settlementList == null) return;
            bool here = CampListVisible(settlement.Discovered, phase == TravelPhase.Seated, phase == TravelPhase.Settlement, false), shown = here && !CampListCovered;
            if (settlementList.gameObject.activeSelf != shown) settlementList.gameObject.SetActive(shown);
            UnityEngine.Rect claim = here ? ClaimOverflowAsideBand : ClaimOverflowBand;
            if (claimOverflow != null && claimOverflow.anchorMin != claim.min) { claimOverflow.anchorMin = claim.min; claimOverflow.anchorMax = claim.max; }
            if (shown) LayoutSettlementList(true, settlement.PlotIndex >= 0);
        }

        // Inside the house (room and bird's-eye) the note sits in the top band like the trade note, clear of the corner squares
        // and away from the drags and taps on the room (user, November 14, 2026); on the street it stays mid-screen.
        public static readonly Vector4 HomeNoteBand = new Vector4(0.17f, 0.875f, 0.83f, 0.975f);   // x0, y0, x1, y1 of the safe area
        public static readonly Vector4 StreetNoteBand = new Vector4(0.04f, 0.40f, 1f, 0.55f);
        public static readonly Vector4 LumberNoteBand = new Vector4(0.15f, 0.42f, 0.85f, 0.58f);   // centred over the lumber counter and zone, and the shop counters
        private void PlaceSettlementNote(bool insideHouse)
        {
            var rect = (RectTransform)settlementNote.transform;
            bool lumber = LumberOpen || ShopOpen || BalloonOpen;   // the shop and balloon counters share the centred band (spec 6.1: refusals on the centred note)
            Vector4 band = lumber ? LumberNoteBand : insideHouse ? HomeNoteBand : StreetNoteBand;
            rect.anchorMin = new Vector2(band.x, band.y); rect.anchorMax = new Vector2(band.z, band.w);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            if (!lumber && !insideHouse) ClearOfSettlementList(rect);   // taps on the list never land on the note
        }

        private void ShowSettlementNote(string text)
        {
            if (settlementNote == null) { Say(text); return; }
            PlaceSettlementNote(homeView == HomeView.Interior || homeView == HomeView.BirdsEye);
            settlementNoteText.text = text; settlementNoteHideAt = Time.unscaledTime + SettlementNoteSeconds;
            settlementNote.transform.SetAsLastSibling(); settlementNote.SetActive(true);
        }

        private void AskSettlement(string question, System.Action confirmed)
        {
            if (settlementConfirm == null || saveBlocked) return;
            router.CancelAll(); router.InteractionLocked = true;
            if (settlementNote.activeSelf) settlementNote.SetActive(false);
            settlementConfirmText.text = question; settlementConfirmed = confirmed;
            settlementConfirm.transform.SetAsLastSibling(); settlementConfirm.SetActive(true);
        }
        private void CloseSettlementConfirm(bool yes)
        {
            System.Action confirmed = settlementConfirmed; settlementConfirmed = null;
            settlementConfirm.SetActive(false); router.InteractionLocked = stamina != null && stamina.Fainted;
            if (yes) confirmed?.Invoke();
        }

        // A plot without a neighbour (before the plot choice) stands at level 1 on the street, so its note says level 1.
        private string NoteFor(StreetBuilding b) => SettlementNote(b, Mathf.Max(1, NeighbourHouseLevel(b.Plot)), b.Kind == StreetBuildingKind.House && b.Plot == settlement.PlotIndex);

        private void SettlementListTapped(string key)
        {
            if (phase == TravelPhase.Seated) { CampListTapped(key); return; }   // at a camp the list travels (Travel.cs)
            if (phase != TravelPhase.Settlement || saveBlocked) return;
            if (HomeOpen) return;
            if (key == "return") { AskReturn(); return; }
            if (key == "home") { OpenHome(); return; }
            if (key == "warehouse") { OpenWarehouse(); return; }
            if (OpensLumberCounter(key, StreetBuildingKind.House)) { OpenLumberCounter(); return; }
            if (ShopIdForKey(key) != null) { OpenShop(ShopIdForKey(key)); return; }
            StreetBuilding b = Street?.Find(key);
            if (b == null) return;
            StreetView?.GlideTo(b.CentreX);
            ShowSettlementNote(NoteFor(b));
        }

        private void SettlementTap(StreetBuilding b)
        {
            if (phase != TravelPhase.Settlement || saveBlocked || (stamina != null && stamina.Fainted)) return;
            if (HomeOpen) return;
            if (b.Kind == StreetBuildingKind.Warehouse) { OpenWarehouse(); return; }
            if (OpensLumberCounter(null, b.Kind)) { OpenLumberCounter(); return; }
            if (b.Kind == StreetBuildingKind.Shop && b.Id.StartsWith("shop.", System.StringComparison.Ordinal) && ShopIdForKey(b.Id.Substring(5)) != null) { OpenShop(b.Id); return; }   // all six shops, the bakery included (slice 2)
            switch (SettlementTapAction(b, PlotChoiceOpen, settlement.PlotIndex, b.Kind == StreetBuildingKind.House ? NeighbourHouseLevel(b.Plot) : 0))
            {
                case StreetTapAction.ChoosePlot: AskPlot(b); return;
                case StreetTapAction.OpenHome: OpenHome(); return;
                case StreetTapAction.OpenNeighbour: OpenNeighbourHome(b.Plot); return;
                default: ShowSettlementNote(NoteFor(b)); return;
            }
        }

        private void AskPlot(StreetBuilding b)
        {
            int plot = b.Plot;
            AskSettlement(SettlementPlotPrompt, () =>
            {
                if (!settlement.CanChoosePlot(plot, out string reason)) { Say(reason); return; }
                settlement.ChoosePlot(plot, travel.DiscoveredRegions);
                lastSavedState = null; SaveNow(true);   // saved immediately on confirm (review focus 1)
                if (saveBlocked) return;
                StreetView?.GlideTo(b.CentreX);
                Say("Your home will stand on plot " + (plot + 1) + ".");
            });
        }

        private void AskReturn()
        {
            if (!travel.CanReturnFromSettlement(out string reason)) { Say(reason); return; }
            string camp = scene.Catalog.Camp(travel.SettlementReturnCampId)?.Name ?? travel.CurrentCamp.Name;
            AskSettlement(SettlementReturnPrompt(camp), () =>
            {
                if (TravelBusy) { Say("You can't travel right now."); return; }
                if (!travel.CanReturnFromSettlement(out string why)) { Say(why); return; }
                if (inventoryOpen) ToggleInventory();
                settlementList.gameObject.SetActive(false); plotOverlay.gameObject.SetActive(false); settlementNote.SetActive(false);
                // The same path as camp travel: a new visit, fresh drops and a new trade offer at the camp.
                CommitTravel(() => travel.ReturnFromSettlement(), "Back at " + camp + ".");
            });
        }

        // ---------------------------------------------------------------- street input (GestureRouter -> IResourceGestures)

        private int? streetPointer;
        private Vector2 streetStart, streetLast;
        private bool streetDragging, streetCaught;

        // Every touch in the settlement scene is the street's: a horizontal drag scrolls (and a flick glides on, see
        // StreetScroll), a short tap selects a building. A touch that stops a gliding street only stops it.
        private bool SettlementPointerBegin(int pointer, Vector2 position)
        {
            if (homeView == HomeView.Interior || homeView == HomeView.BirdsEye) return InteriorPointerBegin(pointer, position);   // the interior's and bird's-eye view's own touch (Home.cs)
            if (LumberOpen || ShopOpen) return LumberPointerBegin(pointer, position);   // the shop counter shares the lumber counter's tap tracking
            if (HomeOpen) return true;   // the home views own the screen: no street drag or tap
            if (streetPointer.HasValue) return true;   // one finger scrolls; extra fingers are swallowed (no orbit or pinch)
            streetPointer = pointer; streetStart = streetLast = position; streetDragging = false;
            streetCaught = StreetView != null && StreetView.BeginDrag();
            return true;
        }
        private bool SettlementPointerMove(int pointer, Vector2 position)
        {
            if (interiorPointer == pointer) { InteriorPointerMove(position); return true; }
            if (LumberPointerMove(pointer, position)) return true;
            if (streetPointer != pointer) return false;
            // A mostly horizontal move past a small slop starts the drag; the slop is then applied too, so nothing jumps.
            if (!streetDragging && Mathf.Abs(position.x - streetStart.x) > Screen.height * 0.008f) { streetDragging = true; streetLast = streetStart; }
            if (!streetDragging && (position - streetStart).sqrMagnitude > Mathf.Pow(Screen.height * 0.012f, 2)) streetDragging = true;
            if (streetDragging && StreetView != null && cameraView != null)
                StreetView.DragBy(StreetDragMetres(position.x - streetLast.x, Screen.width, cameraView.fieldOfView, cameraView.aspect, StreetCamera.Distance));
            streetLast = position;
            return true;
        }
        private bool SettlementPointerEnd(int pointer, Vector2 position, bool canceled)
        {
            if (interiorPointer == pointer) { InteriorPointerEnd(position, canceled); return true; }
            if (ShopPointerEnd(pointer, position, canceled)) return true;
            if (BalloonPointerEnd(pointer, position, canceled)) return true;
            if (LumberPointerEnd(pointer, position, canceled)) return true;
            if (streetPointer != pointer) return false;
            if (!canceled) SettlementPointerMove(pointer, position);
            bool tap = !canceled && !streetDragging && !streetCaught;
            streetPointer = null; streetDragging = false; streetCaught = false;
            if (canceled) StreetView?.CancelDrag(); else StreetView?.EndDrag();
            if (tap && TapStreet(position, out StreetBuilding b)) SettlementTap(b);
            return true;
        }
        private void CancelSettlementPointer() { lumberPointer = null; streetPointer = null; streetDragging = false; streetCaught = false; StreetView?.CancelDrag(); CancelInteriorPointer(); }

        private readonly RaycastHit[] streetHits = new RaycastHit[8];
        private bool TapStreet(Vector2 screen, out StreetBuilding found)
        {
            found = null;
            SettlementStreet street = Street;
            if (street == null || cameraView == null) return false;
            int count = Physics.RaycastNonAlloc(cameraView.ScreenPointToRay(screen), streetHits, cameraView.farClipPlane, ~0, QueryTriggerInteraction.Collide);
            float nearest = float.MaxValue;
            for (int i = 0; i < count; i++)
                foreach (StreetBuilding b in street.Buildings)
                    if (streetHits[i].distance < nearest && (streetHits[i].transform == b.Root || streetHits[i].transform.IsChildOf(b.Root))) { found = b; nearest = streetHits[i].distance; }
            return found != null;
        }
    }
}
