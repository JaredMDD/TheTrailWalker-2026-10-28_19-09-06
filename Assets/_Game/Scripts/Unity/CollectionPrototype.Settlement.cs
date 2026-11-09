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
    // list (Home, Warehouse, Lumber, Return), paper notes for tapped buildings, the first-visit plot choice and Return.
    public sealed partial class CollectionPrototype
    {
        private SettlementState settlement = SettlementState.New(0);

        private SavedSettlement SaveSettlementForSnapshot() => SessionSaveStore.SaveSettlement(settlement);

        // A save without a settlement block gets a fresh state whose seed is derived from the save, so it is deterministic per
        // profile: FNV-1a over the travel visit number and the current camp id.
        private void RestoreSettlementFromSnapshot(SessionSnapshot snapshot) => settlement = SessionSaveStore.RestoreSettlement(snapshot, SettlementSeed(snapshot));

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
                case StreetBuildingKind.House: return isPlayerPlot ? "Your home opens in the next part of Milestone 10" : $"A neighbour's home (level {level})";
                case StreetBuildingKind.Warehouse: return "The warehouse opens in Milestone 12";
                case StreetBuildingKind.LumberField: return "The lumber field opens in Milestone 13";
                default: return ShopDisplayName(b.Id) + " opens in Milestone 14";
            }
        }
        // "shop.bakery" -> "Bakery" (the manifest names: Bakery, Ranch, Farm, Fishery, Mine, Tailor).
        private static string ShopDisplayName(string id)
        {
            string name = id != null && id.StartsWith("shop.", System.StringComparison.Ordinal) ? id.Substring(5) : id ?? "";
            return name.Length == 0 ? "The shop" : char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        // Speed control, pickups and harvest, the route card, the trade note, Craft and the debug cash/gold grants are trail-only.
        public static bool TrailControlsVisible(bool settlement) => !settlement;

        // Until a plot is chosen only Return shows.
        public static string[] SettlementListKeys(bool plotChosen) => plotChosen ? new[] { "home", "warehouse", "lumber", "return" } : new[] { "return" };
        // Slot (row from the top) of a list button, or -1 when hidden: visible buttons stack contiguously from the corner.
        public static int SettlementListSlot(string key, bool plotChosen) => System.Array.IndexOf(SettlementListKeys(plotChosen), key);

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
        private bool InSettlementScene => milestone5 && scene != null && scene.Settlement;
        private SettlementStreet Street { get { if (world == null) world = GetComponent<MovementPrototype>(); return world != null ? world.Street : null; } }
        private StreetCamera StreetView { get { if (world == null) world = GetComponent<MovementPrototype>(); return world != null ? world.StreetView : null; } }

        private void ApplySettlementArrival() => settlement = SettlementArrival(settlement, travel.VisitNumber, travel.CurrentCampId, travel.DiscoveredRegions);

        // ApplyTravelPose in the settlement: no walker, seated-style stamina regeneration (the Seated pause marks a safe area).
        private void EnterSettlementContext()
        {
            phase = TravelPhase.Settlement; motor.Detached = true;
            movement.SetPause(MovementPauseReasons.Seated, true);
            if (settlementEntered) return;
            settlementEntered = true;
            ApplySettlementArrival(); lastSavedState = null; SaveNow();
            if (settlementWelcomePending) { settlementWelcomePending = false; ShowSettlementNote(SettlementWelcome); }
        }

        // Walking into the arch goes straight in; the arrival is applied before the save, so the next scene's house levels are current.
        private void EnterSettlementThroughArch()
        {
            if (archShown) return;
            archShown = true;
            settlementWelcomePending = true;
            CommitTravel(() => { string rolled = travel.ReachSettlementArch(); ApplySettlementArrival(); return rolled; }, SettlementWelcome);
            if (saveBlocked) settlementWelcomePending = false;
        }

        // ---------------------------------------------------------------- HUD

        private RectTransform settlementList, plotOverlay;
        private readonly Dictionary<string, RectTransform> settlementButtons = new Dictionary<string, RectTransform>();
        private readonly List<(StreetBuilding Building, RectTransform Marker)> plotMarkers = new List<(StreetBuilding, RectTransform)>();
        private GameObject settlementNote, settlementConfirm;
        private Text settlementNoteText, settlementConfirmText;
        private System.Action settlementConfirmed;
        private float settlementNoteHideAt;
        private const float SettlementNoteSeconds = 3.5f, SettlementListTop = 0.73f; // just below the feedback line
        private int settlementListLayout = -1; // the chosen state the list was last laid out for (-1: never)
        // Paper and strips stop this far (canvas units) from the safe area's right edge, clear of the list column at any aspect.
        private static float SettlementListClearance => HudMargin + HudSquare + HudGap;
        private static void ClearOfSettlementList(RectTransform rect) { rect.anchorMax = new Vector2(1f, rect.anchorMax.y); rect.offsetMax = new Vector2(-SettlementListClearance, rect.offsetMax.y); }

        private void CreateSettlementHud()
        {
            if (!InSettlementScene) return;
            router.SpeedAreaEnabled = false;   // no walking: the street scrolls instead (see SettlementPointerBegin)
            // The debug CLAIM OVERFLOW button would sit under the list column; in the settlement it moves one column left.
            if (safe.Find("CLAIM OVERFLOW") is RectTransform claim) { claim.anchorMin = new Vector2(0.47f, claim.anchorMin.y); claim.anchorMax = new Vector2(0.72f, claim.anchorMax.y); }

            // Icon-only list from the upper-right corner downward; captions stay on the hidden labels (accessibility names).
            settlementList = Rect("Settlement list", safe, 0, 0, 1, 1);
            var captions = new Dictionary<string, string> { { "home", "Home" }, { "warehouse", "Warehouse" }, { "lumber", "Lumber" }, { "return", "Return" } };
            foreach (string key in SettlementListKeys(true))
            {
                string captured = key;
                Text label = Button(settlementList, captions[key], 0, 0, 0, 0, () => SettlementListTapped(captured));
                HudIconStyle.Apply(label, key);
                var rect = (RectTransform)label.transform.parent; rect.name = "Settlement list: " + captions[key];
                rect.anchorMin = rect.anchorMax = new Vector2(1f, SettlementListTop); rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(HudSquare, HudSquare);
                settlementButtons[key] = rect;
            }
            settlementList.gameObject.SetActive(false);

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
            RectTransform noteRect = Rect("Settlement note", safe, 0.04f, 0.40f, 1f, 0.55f); ClearOfSettlementList(noteRect);   // taps on the list never land on the note
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
        }

        private bool SettlementCovered => inventoryOpen || saveBlocked || (stamina != null && stamina.Fainted)
            || (gearPanel != null && gearPanel.activeSelf) || (mapPanel != null && mapPanel.activeSelf) || (missionPanel != null && missionPanel.activeSelf)
            || (craftPanel != null && craftPanel.activeSelf) || (settlementConfirm != null && settlementConfirm.activeSelf);
        private bool PlotChoiceOpen => settlement.Discovered && settlement.PlotIndex < 0;

        // Per frame while phase == Settlement (from UpdateTravel).
        private void UpdateSettlement()
        {
            if (settlementList == null) return;
            bool covered = SettlementCovered;
            settlementList.gameObject.SetActive(!covered);
            if (!covered)
            {
                bool chosen = settlement.PlotIndex >= 0;
                if (settlementListLayout != (chosen ? 1 : 0))
                {
                    // Re-lay out whenever the visible set changes, so the shown buttons stack from slot 0 (a lone Return takes slot 1).
                    settlementListLayout = chosen ? 1 : 0;
                    foreach (KeyValuePair<string, RectTransform> pair in settlementButtons)
                    {
                        int slot = SettlementListSlot(pair.Key, chosen);
                        pair.Value.gameObject.SetActive(slot >= 0);
                        if (slot >= 0) pair.Value.anchoredPosition = new Vector2(-HudMargin, -slot * (HudSquare + HudGap));
                    }
                }
            }
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
            if (settlementNote.activeSelf && Time.unscaledTime >= settlementNoteHideAt) settlementNote.SetActive(false);
        }

        private void ShowSettlementNote(string text)
        {
            if (settlementNote == null) { Say(text); return; }
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

        private int NeighbourLevel(int plot)
        {
            foreach (SettlementState.Neighbour n in settlement.Neighbours) if (n.Plot == plot) return n.Level;
            return 1;
        }
        private string NoteFor(StreetBuilding b) => SettlementNote(b, NeighbourLevel(b.Plot), b.Kind == StreetBuildingKind.House && b.Plot == settlement.PlotIndex);

        private void SettlementListTapped(string key)
        {
            if (phase != TravelPhase.Settlement || saveBlocked) return;
            if (key == "return") { AskReturn(); return; }
            StreetBuilding b = Street?.Find(key == "home" ? "plot." + settlement.PlotIndex : key);
            if (b == null) return;
            StreetView?.CentreOn(b.CentreX);
            ShowSettlementNote(NoteFor(b));
        }

        private void SettlementTap(StreetBuilding b)
        {
            if (phase != TravelPhase.Settlement || saveBlocked || (stamina != null && stamina.Fainted)) return;
            if (b.Kind == StreetBuildingKind.House && PlotChoiceOpen) { AskPlot(b); return; }
            ShowSettlementNote(NoteFor(b));
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
                StreetView?.CentreOn(b.CentreX);
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
        private bool streetDragging;

        // Every touch in the settlement scene is the street's: a horizontal drag scrolls, a short tap selects a building.
        private bool SettlementPointerBegin(int pointer, Vector2 position)
        {
            if (streetPointer.HasValue) return true;   // one finger scrolls; extra fingers are swallowed (no orbit or pinch)
            streetPointer = pointer; streetStart = streetLast = position; streetDragging = false;
            return true;
        }
        private bool SettlementPointerMove(int pointer, Vector2 position)
        {
            if (streetPointer != pointer) return false;
            if (!streetDragging && (position - streetStart).sqrMagnitude > Mathf.Pow(Screen.height * 0.012f, 2)) streetDragging = true;
            if (streetDragging && StreetView != null && cameraView != null)
                StreetView.ScrollBy(StreetDragMetres(position.x - streetLast.x, Screen.width, cameraView.fieldOfView, cameraView.aspect, StreetCamera.Distance));
            streetLast = position;
            return true;
        }
        private bool SettlementPointerEnd(int pointer, Vector2 position, bool canceled)
        {
            if (streetPointer != pointer) return false;
            if (!canceled) SettlementPointerMove(pointer, position);
            bool tap = !canceled && !streetDragging;
            streetPointer = null; streetDragging = false;
            if (tap && TapStreet(position, out StreetBuilding b)) SettlementTap(b);
            return true;
        }
        private void CancelSettlementPointer() { streetPointer = null; streetDragging = false; }

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
