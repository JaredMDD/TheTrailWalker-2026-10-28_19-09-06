using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 5 camp travel: seated camps beside the road, segment transitions, map, ferry toll and drop cycles.
    public sealed partial class CollectionPrototype
    {
        private enum TravelPhase { Walking, Approaching, Seated, Leaving, Transition }
        private const float SeatMoveSeconds = 1.6f, SeatedEyeDrop = 0.55f;
        private bool milestone5;
        private TravelSceneInfo scene;
        private TravelState travel;
        private TravelPhase phase;
        private Vector3 moveFrom, moveTo;
        private float moveFromYaw, moveToYaw, moveStarted, standSpeed;
        private GameObject mapButton, cashButton, goldButton, mapPanel, tollPanel;
        // Camp-only actions (map, crafting, test wallets) show only while seated; older prototypes have no camps to gate.
        private bool AtCamp => !milestone5 || phase == TravelPhase.Seated;
        private Text tollText;
        private Button tollCash, tollGold;
        private bool tollAsked; // the ferry NPC asks once per walk up to the river
        // Set before a scene reload that continues a walk (the ferry crossing onto its landing leg), so the next load
        // does not treat the unseated save as a closed game. A real app start always begins false.
        private static bool continueAfterReload;

        private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "PrototypeSession");

        // Read-only peek at the saved camp so the scene can build the right segment before the full session loads.
        public static TravelSceneInfo ResolveTravelScene()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            SegmentDefinition segment = catalog.Outgoing(catalog.StartRegion.FirstCamp.Id);
            int visit = 1;
            try
            {
                var store = new SessionSaveStore(SaveDirectory, new SystemClock(), 3);
                LoadStatus status = store.Load(float.MaxValue, out SessionSnapshot snapshot, out _);
                if (status == LoadStatus.Loaded || status == LoadStatus.Recovered)
                {
                    TravelState saved = SessionSaveStore.RestoreTravel(snapshot, catalog);
                    segment = saved.CurrentSegment;
                    // Reopening mid-segment counts as a new visit to the start camp (ResumeAtCampAfterRestart), which
                    // also leaves a ferry landing: the scene shows that camp's own leg. A continuing reload keeps the walk.
                    if (!saved.Seated && !continueAfterReload) segment = catalog.Outgoing(saved.CurrentCampId);
                    visit = Math.Max(1, saved.Seated || continueAfterReload ? saved.VisitNumber : saved.VisitNumber + 1);
                }
            }
            catch (Exception error) when (error is IOException || error is InvalidDataException) { Debug.LogWarning("[Milestone5] Travel peek failed: " + error.Message); }
            return new TravelSceneInfo { Catalog = catalog, Segment = segment, StartVisit = visit };
        }

        private string CurrentSegmentId => travel?.CurrentSegment.Id;
        // Trading (Milestone 9) will also make travel busy.
        private bool TravelBusy => saveBlocked || (stamina != null && stamina.Fainted);
        private bool TravelInputLocked => milestone5 && (phase == TravelPhase.Approaching || phase == TravelPhase.Leaving || phase == TravelPhase.Transition);

        private void StartTravelProfile()
        {
            travel = TravelState.Start(scene.Catalog);
            RollDrops(travel.CurrentSegment.Id);
        }

        // A new drop cycle replaces everything left on that segment and lays out the region's loose resources again.
        private void RollDrops(string segmentId)
        {
            ledger.RemoveWorldItems(segmentId);
            ledger.RemoveWorldItemsExcept(segmentId);   // walked legs' leftovers can never be reached again (see SpatialInventory)
            SegmentDefinition segment = scene.Catalog.Segment(segmentId);
            RegionDefinition region = scene.Catalog.RegionOf(segment);
            int cycle = travel.CycleOf(segmentId);
            foreach (LooseDrops.Drop drop in LooseDrops.Roll(region, segment, cycle))
            {
                ItemDefinition definition = Array.Find(PrototypeItems.All, d => d.Id == drop.ItemId);
                ItemEntry item = ledger.AddWorld(LooseDrops.ItemId(segmentId, cycle, drop.Point), definition, 1000, drop.Quality, new ItemProfile(100), true, drop.Distance);
                ledger.AssignSegment(item, segmentId);
                if (segmentId == scene.Segment.Id) SpawnSegmentBundle(item, Vector3.zero);
            }
        }

        // Saved positions are reused; items generated for another scene are placed from their trail distance and side.
        private void SpawnSegmentBundle(ItemEntry item, Vector3 saved)
        {
            if (saved == Vector3.zero)
            {
                path.Evaluate(Mathf.Clamp(item.TrailDistance, 1f, path.Length - 1f), out Vector3 point, out Vector3 tangent);
                int marker = item.Id.LastIndexOf("-p", StringComparison.Ordinal);
                bool left = marker < 0 || !int.TryParse(item.Id.Substring(marker + 2), out int index) || index % 2 == 0;
                saved = point + Vector3.Cross(Vector3.up, tangent) * (left ? -1.9f : 1.9f) + Vector3.up * 0.45f;
            }
            AddBundle(item, saved);
        }

        private void ApplyTravelPose()
        {
            if (travel.Seated)
            {
                phase = TravelPhase.Seated; motor.Detached = true;
                movement.SetPause(MovementPauseReasons.Seated, true);
                motor.transform.position = scene.StartCamp.Seat + Vector3.down * SeatedEyeDrop;
                orbit.Restore(scene.StartCamp.SeatYaw);
            }
            else { phase = TravelPhase.Walking; motor.Detached = false; movement.SetPause(MovementPauseReasons.Seated, false); motor.Present(); }
            RefreshTravelHud();
        }

        private void UpdateTravel()
        {
            if (phase == TravelPhase.Approaching || phase == TravelPhase.Leaving)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - moveStarted) / SeatMoveSeconds);
                float eased = t * t * (3f - 2f * t);
                motor.transform.position = Vector3.Lerp(moveFrom, moveTo, eased);
                orbit.Restore(Mathf.LerpAngle(moveFromYaw, moveToYaw, eased));
                if (t >= 1f) { if (phase == TravelPhase.Approaching) FinishApproach(); else FinishLeaving(); }
                return;
            }
            // Reaching the end of the road stops at the camp; the player then walks off the road to the seat.
            if (phase == TravelPhase.Walking && scene.EndCamp != null && (movement.Pauses & MovementPauseReasons.Camp) != 0 && !TravelBusy) BeginApproach();
            // The river leg ends at the ferry: once the toll is paid the ferry takes the player straight to the next
            // region's first camp; otherwise the ferry NPC asks for the one-time toll.
            if (phase == TravelPhase.Walking && scene.Segment.RiverApproach && (movement.Pauses & MovementPauseReasons.Camp) != 0 && !TravelBusy && !inventoryOpen)
            {
                if (travel.PaidTolls.Contains(travel.CurrentRegion.Id)) { if (travel.CanCrossRiver(false, out _)) TakeFerry(); else AskToll(); }
                else AskToll();
            }
        }

        private void AskToll()
        {
            if (tollAsked) return;
            tollAsked = true; router.CancelAll(); router.InteractionLocked = true;
            RegionDefinition region = travel.CurrentRegion, next = scene.Catalog.Region(region.NextRegionId);
            bool open = next != null;
            long gold = TravelState.TollGold(region);
            tollText.text = !open ? "Ferryman: \"The far bank isn't open yet. Come back in a later milestone.\""
                : travel.PaidTolls.Contains(region.Id) ? "Ferryman: \"Your crossing is paid.\""
                : $"Ferryman: \"Crossing to {next.Name} costs a one-time fee. After that, the ferry is yours whenever you come.\"\n\nYou have {travel.Cash} cash and {travel.Gold} gold.";
            tollCash.gameObject.SetActive(open && !travel.PaidTolls.Contains(region.Id));
            tollGold.gameObject.SetActive(open && !travel.PaidTolls.Contains(region.Id));
            tollCash.GetComponentInChildren<Text>().text = $"PAY {region.TollCost} CASH";
            tollGold.GetComponentInChildren<Text>().text = $"PAY {gold} GOLD";
            tollCash.interactable = travel.Cash >= region.TollCost; tollGold.interactable = travel.Gold >= gold;
            tollPanel.transform.SetAsLastSibling(); tollPanel.SetActive(true);
        }
        private void PayTollWith(TollCurrency currency)
        {
            if (!travel.CanPayToll(currency, out string reason)) { Say(reason); return; }
            long cashBefore = travel.Cash, goldBefore = travel.Gold;
            travel.PayToll(currency);
            statistics.SpendCash(cashBefore - travel.Cash); statistics.SpendGold(goldBefore - travel.Gold);
            SaveNow();
            if (saveBlocked) return;
            tollPanel.SetActive(false); router.InteractionLocked = stamina.Fainted;
            TakeFerry();
        }
        // Declining walks back to the camp the river leg started from (a real camp return, like BACK TO CAMP).
        private void DeclineToll()
        {
            tollPanel.SetActive(false); router.InteractionLocked = stamina.Fainted;
            CommitTravel(() => travel.ArriveAtCamp(travel.CurrentCampId), "Back at " + travel.CurrentCamp.Name + ". The ferry waits at the river.");
        }

        private void BeginApproach()
        {
            if (inventoryOpen) ToggleInventory();
            router.CancelAll(); router.InteractionLocked = true;
            phase = TravelPhase.Approaching; motor.Detached = true;
            moveFrom = motor.transform.position; moveTo = scene.EndCamp.Seat + Vector3.down * SeatedEyeDrop;
            moveFromYaw = orbit.Yaw; moveToYaw = scene.EndCamp.SeatYaw; moveStarted = Time.unscaledTime;
            Say("Arriving at " + scene.EndCamp.Camp.Name + ".");
        }
        private void FinishApproach() => CommitTravel(() => travel.ArriveAtSegmentEnd(), "Seated at " + scene.EndCamp.Camp.Name + ".");

        // Every arrival is saved before the scene changes, so an interrupted load resumes the destination exactly once.
        private void CommitTravel(Func<string> move, string arrival)
        {
            string rolled = move();
            RollDrops(rolled);
            movement.Reset(); movement.SetPause(MovementPauseReasons.Seated, true); lastDistance = 0;
            phase = TravelPhase.Transition; router.CancelAll(); router.InteractionLocked = true;
            lastSavedState = null; SaveNow();
            if (saveBlocked) return;
            Say(arrival);
            if (scene.RequestReload != null) scene.RequestReload();
            else Say(arrival + " Start from the Travel bootstrap scene to load the camp.");
        }

        // Upward speed gesture while seated: stand up and walk back to the road.
        private void TryStandUp(float drag)
        {
            if (phase != TravelPhase.Seated || TravelBusy) return;
            if (inventoryOpen) { Say("Close the bag first."); return; }
            if (!travel.CanStandUp(out string reason)) { Say(reason); return; }
            travel.StandUp();
            router.CancelAll(); router.InteractionLocked = true;
            phase = TravelPhase.Leaving; standSpeed = Mathf.Max(0.3f, drag * 4f / Screen.height);
            path.Evaluate(0, out Vector3 road, out Vector3 tangent);
            moveFrom = motor.transform.position; moveTo = road;
            moveFromYaw = orbit.Yaw; moveToYaw = Quaternion.LookRotation(tangent).eulerAngles.y; moveStarted = Time.unscaledTime;
            RefreshTravelHud(); Say("Heading back to the road.");
        }
        private void FinishLeaving()
        {
            motor.Detached = false; motor.Present();
            movement.SetPause(MovementPauseReasons.Seated, false);
            router.InteractionLocked = stamina.Fainted;
            phase = TravelPhase.Walking; movement.SpeedGesture(standSpeed);
            SaveNow(); RefreshTravelHud(); Say("Walking on from " + travel.CurrentCamp.Name + ".");
        }

        private void CreateTravelHud()
        {
            cashButton = Button(safe, "+1,000 CASH", 0.74f, 0.67f, 0.98f, 0.72f, () => { travel.GrantTestCash(1000); statistics.AddCash(1000); SaveNow(); RefreshTravelHud(); Say("Added 1,000 test cash (debug wallet until Milestone 9)."); }).transform.parent.gameObject;
            goldButton = Button(safe, "+1 GOLD", 0.74f, 0.725f, 0.98f, 0.775f, () => { travel.GrantTestGold(1); statistics.AddGold(1); SaveNow(); RefreshTravelHud(); Say("Added 1 test gold (debug wallet until Milestone 9)."); }).transform.parent.gameObject;
            // Ferry toll dialog: shown by the ferry NPC when the player reaches the river.
            tollPanel = Rect("Ferry toll", safe, 0.08f, 0.36f, 0.92f, 0.64f).gameObject;
            tollPanel.AddComponent<Image>().color = new Color(0.05f, 0.11f, 0.13f, 0.98f);
            tollText = Label(tollPanel.transform, "", 0.05f, 0.42f, 0.95f, 0.95f, 24);
            tollText.resizeTextForBestFit = true; tollText.resizeTextMinSize = 12; tollText.resizeTextMaxSize = 24;
            tollCash = Button(tollPanel.transform, "PAY CASH", 0.04f, 0.06f, 0.35f, 0.34f, () => PayTollWith(TollCurrency.Cash)).transform.parent.GetComponent<Button>();
            tollGold = Button(tollPanel.transform, "PAY GOLD", 0.36f, 0.06f, 0.65f, 0.34f, () => PayTollWith(TollCurrency.Gold)).transform.parent.GetComponent<Button>();
            Button(tollPanel.transform, "NOT NOW", 0.66f, 0.06f, 0.96f, 0.34f, DeclineToll);
            tollPanel.SetActive(false);
            CreateMapPanel(); // worn region map (CollectionPrototype.Map.cs)
            CreateRouteCard(); // departure preview and remaining leg (CollectionPrototype.Route.cs)
        }

        private void RefreshTravelHud()
        {
            if (cashButton == null || travel == null) return;
            bool seated = phase == TravelPhase.Seated;
            if (!seated && mapPanel.activeSelf) CloseMap();
            cashButton.SetActive(seated); goldButton.SetActive(seated);
            SetMenu(menuOpen);
        }

        private void SelectCamp(string campId)
        {
            if (!travel.CanTravelToCamp(campId, TravelBusy, out string reason)) { Say(reason); return; }
            CloseMap();
            CommitTravel(() => travel.TravelToCamp(campId, TravelBusy), "Travelled to " + scene.Catalog.Camp(campId).Name + ".");
        }
        private void SelectRegion(string regionId)
        {
            if (!travel.CanTravelToRegion(regionId, TravelBusy, out string reason)) { Say(reason); return; }
            CloseMap();
            RegionDefinition region = scene.Catalog.Region(regionId);
            CommitTravel(() => travel.TravelToRegion(regionId, TravelBusy), $"Travelled to {region.Name}: {region.FirstCamp.Name}.");
        }
        private void TakeFerry()
        {
            if (!travel.CanCrossRiver(TravelBusy, out string reason)) { Say(reason); return; }
            RegionDefinition next = scene.Catalog.Region(travel.CurrentRegion.NextRegionId);
            continueAfterReload = true;
            CommitTravel(() => travel.CrossRiver(TravelBusy), $"Crossed the river into {next.Name}. {next.FirstCamp.Name} lies ahead.");
        }
        // Debug: return to the camp this segment started from (a real camp return, so its drops reroll).
        private void BackToCamp()
        {
            if (phase != TravelPhase.Walking || TravelBusy) { Say("Only while walking a segment."); return; }
            CommitTravel(() => travel.ArriveAtCamp(travel.CurrentCampId), "Back at " + travel.CurrentCamp.Name + ".");
        }
        // Debug (user, November 2, 2026): unlock every camp of the current region on the camp map.
        private void UnlockRegionCamps()
        {
            if (saveBlocked || travel == null || stamina.Fainted) return;
            int unlocked = travel.UnlockRegionCampsForTest();
            if (unlocked == 0) { Say("Every " + travel.CurrentRegion.Name + " camp is already unlocked."); return; }
            SaveNow(); RefreshTravelHud();
            if (!saveBlocked) Say($"Unlocked {unlocked} {travel.CurrentRegion.Name} camp(s) on the map (debug). Travel from the map while seated at a camp.");
        }
        private static bool ContinuingReload() { bool continuing = continueAfterReload; continueAfterReload = false; return continuing; }
        private string TravelStatus() => travel == null ? "" :
            $"\nRegion {scene.Catalog.RegionOf(travel.CurrentSegment).Number} {scene.Catalog.RegionOf(travel.CurrentSegment).Name} | " +
            (travel.OnFarBank ? "Road to " + scene.Catalog.Camp(travel.CurrentSegment.ToCampId).Name : (phase == TravelPhase.Walking ? "Road from " : "") + travel.CurrentCamp.Name) +
            $" | Cash {travel.Cash} | Gold {travel.Gold}";
    }
}
