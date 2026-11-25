using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Equipment;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Input;
using TheTrailWalker.Movement;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheTrailWalker
{
    [DefaultExecutionOrder(50)]
    public sealed partial class CollectionPrototype : MonoBehaviour, IResourceGestures
    {
        private sealed class Bundle
        {
            public ItemEntry Item; public GameObject View; public Collider Collider; public Text Badge;
            // Meshed resources scale evenly (no squashing); MeshScale is 0 for the placeholder cube.
            public Transform Visual; public float MeshScale, MeshBottom; public Vector3 MeshCentre, MeshSize;
            // Cached ground height under the pickup, recomputed when it moves or its stack size changes.
            public bool GroundKnown; public Vector3 GroundAt; public float GroundScale, GroundY;
        }
        private MovementPrototype world;
        // Pickups are placed 0.45 m above the road; their model rests on the drawn terrain under its footprint (user,
        // November 5, 2026: after the hill-leg terrain changes pickups beside the road floated above the ground). Without
        // a terrain (older test scenes) the ground is taken as the road height, as before.
        private float GroundUnder(Bundle bundle, float uniform)
        {
            Vector3 at = bundle.View.transform.position;
            if (bundle.GroundKnown && at == bundle.GroundAt && Mathf.Approximately(uniform, bundle.GroundScale)) return bundle.GroundY;
            if (world == null) world = GetComponent<MovementPrototype>();
            float radius = 0.5f * uniform * Mathf.Max(bundle.MeshSize.x, bundle.MeshSize.z);
            if (world == null || world.Ground == null || !world.Ground.TryLowest(at, radius, out float ground)) ground = at.y - 0.45f;
            bundle.GroundKnown = true; bundle.GroundAt = at; bundle.GroundScale = uniform; bundle.GroundY = ground;
            return ground;
        }
        private sealed class Contact
        {
            public ItemEntry Item; public Bundle Bundle; public Vector2 Start, Position, Offset;
            public float Started; public bool World, Side, Dragging; public RectTransform Ghost;
            // Home storage (CollectionPrototype.HomeStorage.cs): the chest the item is held in (null: the bag), or the rack spot.
            public SpatialInventory Source; public int RackSpot = -1;
        }
        private SpatialInventory ledger;
        private MovementState movement;
        private TrailMotor motor;
        private ITrailPath path;
        private CameraOrbitController orbit;
        private GestureRouter router;
        private Camera cameraView;
        private Material material;
        private RectTransform safe, bag, aperture, itemsLayer;
        private Text status, feedback, inventoryLabel, titleLabel;
        private readonly List<GameObject> counterHiddenButtons = new List<GameObject>();   // debug buttons a counter view hides (SetCounterChrome)
        // The always-on debug HUD on the safe area (x0, y0, x1, y1), shared with the counter layout checks (Milestone14Validation).
        public static readonly UnityEngine.Rect TitleBand = UnityEngine.Rect.MinMaxRect(0.02f, 0.94f, 0.98f, 0.99f), StatusBand = UnityEngine.Rect.MinMaxRect(0.02f, 0.865f, 0.98f, 0.94f),
            FeedbackBand = UnityEngine.Rect.MinMaxRect(0.02f, 0.735f, 0.98f, 0.81f), ResetSaveBand = UnityEngine.Rect.MinMaxRect(0.76f, 0.81f, 0.98f, 0.86f),
            UnlockCampsBand = UnityEngine.Rect.MinMaxRect(0.20f, 0.61f, 0.45f, 0.66f), LearnRecipesBand = UnityEngine.Rect.MinMaxRect(0.20f, 0.49f, 0.45f, 0.54f),
            AutoSortBand = UnityEngine.Rect.MinMaxRect(0.20f, 0.55f, 0.45f, 0.60f), ClaimOverflowBand = UnityEngine.Rect.MinMaxRect(0.74f, 0.55f, 0.98f, 0.60f);
        private readonly List<Bundle> bundles = new List<Bundle>();
        private readonly Dictionary<int, Contact> contacts = new Dictionary<int, Contact>();
        private readonly RaycastHit[] occlusionHits = new RaycastHit[32];
        private float nextHud, nextReport;
        private int revision = -1, accepted, rejected;
        private string fixtureCycle;
        private bool inventoryOpen;
        private SessionSaveStore saveStore;
        private bool saveBlocked, resetPending, initialized;
        private float fpsAverage = 60f; // smoothed frame rate shown in the status line while testing device limits
        private float nextSave, resetDeadline;
        private string lastSavedState;
        private bool milestone4;
        private EquipmentState equipment;
        private StaminaState stamina;
        private string faintEventId;
        private readonly List<SavedLoss> faintLoss = new List<SavedLoss>();
        private readonly System.Random faintRandom = new System.Random();
        private float lastDistance;
        private ItemEntry selected;
        private GearSlot? selectedGearSlot;
        private GameObject gearPanel;
        private GameObject faintOverlay;
        private Text faintMessage;
        private GameObject detailsPanel;
        private GameObject detailEat, detailEquip;
        private float detailsCloseAt;
        private RectTransform bagFrame, inventoryButton, bagContent, hookPanel, pocketPanel, dropPocket;
        private readonly List<RectTransform> hookCells = new List<RectTransform>(), pocketCells = new List<RectTransform>();
        private float bagScroll, contentWidth, laidOutWidth, scrollStartX, scrollStartOffset;
        private int? scrollPointer;
        private const int VisibleColumns = 6;
        // Gap below the open bag (share of the safe height) so dragging near its lower edge never hits the home gesture.
        private const float BagLift = 0.06f;
        private const float OpeningDropBand = 0.10f; // share of screen height above the opening that still counts as a drop
        private RectTransform speedGauge, speedFill;
        private Text speedValue;
        // Speed ruler (user, October 31, 2026): a worn wooden ruler with tick marks and no numbers, standing above the mission
        // square (which sits above the bag button). Its length follows the boots speed cap (the tier buff): the full height
        // represents the maximum cap, and the ruler grows by tiling, never stretching its ticks.
        private const float ThermometerFull = 0.30f, MaximumSpeedCap = MovementState.MaximumSpeedCap;
        private RectTransform speedMarker;
        private float ThermometerBase => menuBarLine + (safe != null && safe.rect.height > 0 ? (2f * (HudSquare + HudGap) + HudGap) / safe.rect.height : 0.15f);
        // Authored walking drain (12 hearts/km), shared with the route-budget checks.
        private const double LegDrainPerMetre = TheTrailWalker.Domain.Travel.RouteBudget.RegionOneDrainPerMetre;
        private RectTransform slotGhost;
        private Text detailsText;
        private QualityStarStrip detailsQuality;
        private Rect lastSafe;
        private Vector2Int lastSize;

        public void Bind(MovementState state, TrailMotor trailMotor, ITrailPath trail, CameraOrbitController cameraOrbit, GestureRouter gestures, Material sharedMaterial, bool enableMilestone4 = false, TravelSceneInfo travelScene = null)
        {
            movement = state; motor = trailMotor; path = trail; orbit = cameraOrbit; router = gestures; material = sharedMaterial;
            milestone4 = enableMilestone4 || travelScene != null; milestone5 = travelScene != null; scene = travelScene;
            if (milestone4) { equipment = new EquipmentState(); stamina = new StaminaState(); motor.LimitMovementSeconds = LimitMovementSeconds; }
            cameraView = cameraOrbit.GetComponent<Camera>(); router.Resources = this; router.SpeedBlocked += ExplainSpeedBlocked;
            CreateHud();
            string directory = Path.Combine(Application.persistentDataPath, "PrototypeSession");
            saveStore = new SessionSaveStore(directory, new SystemClock(), milestone5 ? 3 : milestone4 ? 2 : 1);
            LoadStatus result = saveStore.Load(path.Length, out SessionSnapshot snapshot, out string message);
            bool migratedToTravel = false;
            if (milestone5 && result == LoadStatus.Fresh)
            {
                // Copy the Milestone 4 profile (or the Milestone 3 one through it); the older files stay untouched.
                var v2Store = new SessionSaveStore(directory, new SystemClock(), 2);
                LoadStatus v2Result = v2Store.Load(float.MaxValue, out SessionSnapshot v2Snapshot, out string v2Message);
                if (v2Result == LoadStatus.Fresh)
                {
                    var v1Store = new SessionSaveStore(directory, new SystemClock());
                    LoadStatus v1Result = v1Store.Load(float.MaxValue, out SessionSnapshot v1Snapshot, out string v1Message);
                    if (v1Result == LoadStatus.Loaded || v1Result == LoadStatus.Recovered) { v2Snapshot = SessionSaveStore.MigrateV1(v1Snapshot, float.MaxValue); v2Result = v1Result; v2Message = v1Message; }
                    else if (v1Result != LoadStatus.Fresh) { v2Result = v1Result; v2Message = v1Message; }
                }
                if (v2Result == LoadStatus.Loaded || v2Result == LoadStatus.Recovered)
                { snapshot = SessionSaveStore.MigrateV2(v2Snapshot); result = v2Result; migratedToTravel = true; message = "Migrated a copy of your profile: seated at Dawn Meadow, carried items and gear kept."; }
                else if (v2Result != LoadStatus.Fresh) { result = v2Result; message = v2Message; }
            }
            else if (milestone4 && !milestone5 && result == LoadStatus.Fresh)
            {
                var oldStore = new SessionSaveStore(directory, new SystemClock());
                LoadStatus oldResult = oldStore.Load(path.Length, out SessionSnapshot oldSnapshot, out string oldMessage);
                if (oldResult == LoadStatus.Loaded || oldResult == LoadStatus.Recovered)
                { snapshot = SessionSaveStore.MigrateV1(oldSnapshot, path.Length); result = oldResult; message = oldResult == LoadStatus.Recovered ? oldMessage : "Migrated a copy of the Milestone 3 profile."; }
                else if (oldResult != LoadStatus.Fresh) { result = oldResult; message = oldMessage; }
            }
            if (result == LoadStatus.Fresh && milestone5)
            {
                ledger = new SpatialInventory(equipment.BackpackWidth * 1000, equipment.BackpackHeight * 1000);
                ledger.Reconfigure(ledger.Width, ledger.Height, equipment.Pockets, equipment.Hooks, out _);
                StartTravelProfile(); initialized = true; ApplyTravelPose(); LayoutBackpack(); Redraw(); SaveNow();
                Say("New test profile: seated at " + travel.CurrentCamp.Name + ". Swipe UP on the centre line to stand up.");
            }
            else if (result == LoadStatus.Fresh) { ResetFixture(0); initialized = true; SaveNow(); }
            else if (result == LoadStatus.Loaded || result == LoadStatus.Recovered)
            {
                RestoreSnapshot(snapshot); initialized = true;
                if (milestone5)
                {
                    // Saves written before November 7, 2026 still carry the leftovers of every walked leg; drop them on load.
                    if (travel != null && ledger.RemoveWorldItemsExcept(travel.CurrentSegment.Id) > 0) lastSavedState = null;
                    // A migrated profile starts its first drop cycle here; later loads never roll again.
                    if (migratedToTravel) { RollDrops(travel.CurrentSegment.Id); SaveNow(); }
                    // Closed mid-segment: return to the previous camp; saved resources stay, segment progress resets.
                    string restarted = ContinuingReload() ? null : travel.ResumeAtCampAfterRestart();
                    if (restarted != null)
                    {
                        RollDrops(restarted);
                        if (inventoryOpen) { movement.CloseInventory(); inventoryOpen = false; LayoutBackpack(); }
                        movement.Reset(); lastDistance = 0;
                        if (stamina.Fainted) { movement.BeginHarvest(); movement.EndHarvest(); }
                        movement.SetPause(MovementPauseReasons.Seated, true); lastSavedState = null;
                        message = "The game was closed on the road: back at " + travel.CurrentCamp.Name + ". Collected items were kept; the segment restarts with fresh resources.";
                        ApplyTravelPose(); SaveNow();
                    }
                    ApplyTravelPose();
                    if (travel.CurrentSegment.Id != scene.Segment.Id && scene.RequestReload != null) { scene.RequestReload(); return; }
                }
                Say(string.IsNullOrEmpty(message) ? "Session restored." : message);
                if (houseMigrationPending)
                {
                    houseMigrationPending = false; SaveNow(true);
                    const string returned = "Some deposited materials were returned to the overflow.";
                    if (settlementNote != null) ShowSettlementNote(returned); else Say(returned);
                }
            }
            else
            {
                ledger = new SpatialInventory(); saveBlocked = true; initialized = true;
                movement.SetPause(MovementPauseReasons.Exhausted, true);
                Say(message + " Tap RESET SAVE twice to archive and start a new test profile.");
                Debug.LogError("[Milestone3] " + message);
            }
            lastDistance = movement.Distance;
        }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        private static Text Label(Transform parent, string value, float x0, float y0, float x1, float y1, int size = 24)
        {
            Text text = Rect("Label", parent, x0, y0, x1, y1).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.text = value;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; return text;
        }
        private static Text Button(Transform parent, string value, float x0, float y0, float x1, float y1, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = Rect(value, parent, x0, y0, x1, y1); Image image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.1f, 0.25f, 0.24f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            return Label(rect, value, 0, 0, 1, 1, 22);
        }
        private void CreateHud()
        {
            var canvas = new GameObject("Collection HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); canvas.transform.SetParent(transform, false);
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(720, 1280); scaler.matchWidthOrHeight = 0.5f;
            safe = Rect("Safe area", canvas.transform, 0, 0, 1, 1);
            // Speed is dragged on the centre line (GestureRouter.IsSpeedArea); this thermometer appears only while adjusting.
            speedGauge = Rect("Speed ruler", safe, 0.035f, 0.2f, 0.115f, 0.5f);
            Image tube = speedGauge.gameObject.AddComponent<Image>(); tube.raycastTarget = false;
            Sprite ruler = Resources.Load<Sprite>("UI/Hud/ruler_tile");
            if (ruler != null) { tube.sprite = ruler; tube.type = Image.Type.Tiled; } else tube.color = new Color(0.6f, 0.44f, 0.27f, 0.95f);
            var rim = speedGauge.gameObject.AddComponent<Outline>(); rim.effectColor = new Color(0.16f, 0.09f, 0.03f, 0.9f); rim.effectDistance = new Vector2(1.5f, -1.5f);
            // Requested speed: a translucent stain up the ruler, topped by a dark marker line (visual only, no numbers).
            speedFill = Rect("Speed fill", speedGauge, 0f, 0f, 1f, 0.5f);
            Image fill = speedFill.gameObject.AddComponent<Image>(); fill.color = new Color(0.93f, 0.42f, 0.16f, 0.45f); fill.raycastTarget = false;
            speedMarker = Rect("Speed marker", speedFill, -0.12f, 1f, 1.12f, 1f); speedMarker.sizeDelta = new Vector2(0, 7f);
            Image marker = speedMarker.gameObject.AddComponent<Image>(); marker.color = new Color(0.35f, 0.08f, 0.04f, 1f); marker.raycastTarget = false;
            speedValue = Label(safe, "", 0.01f, 0, 0.30f, 0, 20); speedValue.alignment = TextAnchor.LowerLeft;
            speedGauge.gameObject.SetActive(false); speedValue.gameObject.SetActive(false);
            titleLabel = Label(safe, "THE TRAIL WALKER / COLLECTION", TitleBand.xMin, TitleBand.yMin, TitleBand.xMax, TitleBand.yMax, 27);
            status = Label(safe, "", StatusBand.xMin, StatusBand.yMin, StatusBand.xMax, StatusBand.yMax, 23);
            // Four status lines with travel info: shrink to fit instead of clipping.
            status.resizeTextForBestFit = true; status.resizeTextMinSize = 12; status.resizeTextMaxSize = 23;
            // Debug cleanup, second pass (user, November 7, 2026): the camp world drops EMPTY, EAT SELECTED (item details
            // has EAT), TARGET T1-T4, BACK TO CAMP and TEST EMPTY. It keeps +1 GOLD (+1,000 CASH removed November 14, 2026; no income until
            // Milestone 9 trading), AUTO SORT / CLAIM OVERFLOW (until the gravity bag), RESET SAVE / UNLOCK CAMPS and
            // LEARN T1 RECIPES / + INGREDIENTS (crafting mastery tests). The older scenes keep their buttons.
            if (!milestone5) Button(safe, "EMPTY", 0.02f, 0.81f, 0.24f, 0.86f, () => ResetFixture(0));
            // Debug cleanup, first pass (user, November 2, 2026): the camp world drops the MIXED / FULL BAG / TREE FOOD
            // fixtures, the test and spare gear and the tool kits; crafting now supplies real gear and tools for testing.
            // The older collection and survival scenes keep their fixtures because they have no crafting.
            if (!milestone5) Button(safe, "MIXED", 0.26f, 0.81f, 0.48f, 0.86f, () => ResetFixture(1));
            if (!milestone5) Button(safe, "FULL BAG", 0.5f, 0.81f, 0.74f, 0.86f, () => ResetFixture(2));
            Button(safe, "RESET SAVE", ResetSaveBand.xMin, ResetSaveBand.yMin, ResetSaveBand.xMax, ResetSaveBand.yMax, ConfirmReset);
            feedback = Label(safe, "Swipe a pickup DOWN quickly, or hold then drag into the bag.", FeedbackBand.xMin, FeedbackBand.yMin, FeedbackBand.xMax, FeedbackBand.yMax, 23);
            // Milestone 6 debug: interactable target tier and tool kits for Normal/Elite and tier comparisons.
            if (!milestone5) Button(safe, "TRAIL START", 0.47f, 0.67f, 0.72f, 0.72f, ReturnToTrailStart);
            if (milestone4)
            {
                if (!milestone5) Button(safe, "EAT SELECTED", 0.47f, 0.61f, 0.72f, 0.66f, EatSelected);
                if (milestone5) counterHiddenButtons.Add(Button(safe, "UNLOCK CAMPS", UnlockCampsBand.xMin, UnlockCampsBand.yMin, UnlockCampsBand.xMax, UnlockCampsBand.yMax, UnlockRegionCamps).transform.parent.gameObject);
                if (milestone5) counterHiddenButtons.Add(Button(safe, "LEARN T1 RECIPES", LearnRecipesBand.xMin, LearnRecipesBand.yMin, LearnRecipesBand.xMax, LearnRecipesBand.yMax, LearnAllRecipes).transform.parent.gameObject);
                if (!milestone5) Button(safe, "TEST EMPTY", 0.74f, 0.61f, 0.98f, 0.66f, DebugEmptyStamina);
                if (!milestone5) Button(safe, "TREE FOOD", 0.20f, 0.55f, 0.45f, 0.60f, () => ResetFixture(3));
                // Debug auto sort (November 2, 2026) until the gravity/collision bag replaces manual packing.
                if (milestone5) counterHiddenButtons.Add(Button(safe, "AUTO SORT", AutoSortBand.xMin, AutoSortBand.yMin, AutoSortBand.xMax, AutoSortBand.yMax, AutoSortBag).transform.parent.gameObject);
                if (!milestone5) Button(safe, "ADD SPARE GEAR", 0.47f, 0.55f, 0.72f, 0.60f, GrantSpareGear);
                counterHiddenButtons.Add(Button(safe, "CLAIM OVERFLOW", ClaimOverflowBand.xMin, ClaimOverflowBand.yMin, ClaimOverflowBand.xMax, ClaimOverflowBand.yMax, ClaimOverflow).transform.parent.gameObject);
                CreateGearPanel();
            }
            inventoryLabel = Button(safe, "BAG", 0.02f, 0.04f, 0.22f, 0.085f, BagButton);
            inventoryButton = (RectTransform)inventoryLabel.transform.parent;
            HudIconStyle.Apply(inventoryLabel, "inventory");
            // Discard pocket opposite CLOSE: dragging an item here drops it back onto the trail.
            dropPocket = Rect("Drop pocket", safe, 0.78f, 0.04f, 0.98f, 0.085f);
            Image dropImage = dropPocket.gameObject.AddComponent<Image>(); dropImage.color = new Color(0.42f, 0.2f, 0.16f); dropImage.raycastTarget = false;
            Text dropLabel = Label(dropPocket, "DROP", 0, 0, 1, 1, 22);
            if (HudIconStyle.ApplyTo(dropPocket, "trash", false)) dropLabel.enabled = false;
            // The backpack opening doubles as the quick-storage aperture; the full bag shows only while open.
            aperture = Rect("Backpack opening", safe, 0, 0, 1, 0.035f);
            InventoryContainerStyle.Apply(aperture, "BaseRagsOpening");
            // Full-width viewport at the bottom edge; its content scrolls sideways: hooks | bag | pockets.
            bagFrame = Rect("Bag viewport", safe, 0, 0, 1, 0.3f);
            bagFrame.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.105f, 0.085f, 0.97f);
            bagFrame.gameObject.AddComponent<RectMask2D>();
            bagFrame.gameObject.AddComponent<BagPointerSurface>().Owner = this;
            bagContent = Pinned("Bag content", bagFrame);
            hookPanel = Pinned("Hooks", bagContent);
            bag = Pinned("Backpack", bagContent);
            InventoryContainerStyle.Apply(bag, "BaseRags");
            pocketPanel = Pinned("Pockets", bagContent);
            itemsLayer = Rect("Item views", bag, 0, 0, 1, 1);
            CreateUserMenu();
            CreateMissionTracker();
            if (milestone4)
            {
                detailsPanel = Rect("Item details", safe, 0.10f, 0.40f, 0.90f, 0.60f).gameObject;
                detailsPanel.AddComponent<Image>().color = new Color(0.04f, 0.10f, 0.11f, 0.99f);
                detailsText = Label(detailsPanel.transform, "", 0.03f, 0.37f, 0.97f, 0.96f, 23);
                detailEat = Button(detailsPanel.transform, "EAT", 0.29f, 0.05f, 0.71f, 0.32f, EatSelected).transform.parent.gameObject;
                // Food and wearables never overlap, so EQUIP shares EAT's position. No CLOSE: the bag button's return
                // icon closes the details first (user, November 5, 2026); they also close by themselves after a while.
                detailEquip = Button(detailsPanel.transform, "EQUIP", 0.29f, 0.05f, 0.71f, 0.32f, () => EquipItem(selected)).transform.parent.gameObject;
                ItemDetailsStyle.Apply(detailsPanel, detailsText);
                detailsQuality = Rect("Quality breakdown", detailsPanel.transform, .08f, .33f, .92f, .48f).gameObject.AddComponent<QualityStarStrip>();
                detailsPanel.SetActive(false);
                CreateFaintOverlay(); // CollectionPrototype.Faint.cs
            }
            if (milestone5) CreateTravelHud();
            LayoutBackpack();
            ApplySafeArea();
        }
        // Debug only: back to 0 m keeping bag, gear and stamina, so wear and faint tests can continue past camp.
        private void ReturnToTrailStart()
        {
            if (saveBlocked || (milestone4 && stamina.Fainted)) return;
            router.CancelAll();
            if (inventoryOpen) ToggleInventory();
            movement.Reset(); motor.Present(); lastDistance = movement.Distance;
            // Stands in for returning to the segment's previous camp: the new drop cycle deletes uncollected drops.
            int removed = ledger.RemoveUncollectedDrops();
            Redraw(); SaveNow();
            if (!saveBlocked) Say("Back at the trail start." + (removed > 0 ? $" {removed} uncollected dropped item(s) were removed." : "") + " Swipe UP on the centre line to walk.");
        }
        // Closed: only the opening and a small BAG button above it. Open: the whole bag with the opening on top.
        private void LayoutBackpack()
        {
            RefreshContainerArt();
            bagFrame.gameObject.SetActive(inventoryOpen);
            if (inventoryOpen) LayoutBag(true);
            float top = inventoryOpen ? bagFrame.anchorMax.y : 0;
            aperture.anchorMin = new Vector2(0, top); aperture.anchorMax = new Vector2(1, top + 0.035f);
            LayoutSquares((inventoryOpen && ContainerOpen ? Mathf.Max(top, ContainerTop) : top) + BarLineGap);   // above an open container panel
            dropPocket.gameObject.SetActive(inventoryOpen && TrailControlsVisible(InSettlementScene)); // nothing is dropped onto the street
            inventoryLabel.text = inventoryOpen ? "RETURN" : "BAG"; // only shown until the icons import
            SyncBagButtonIcon();
        }
        private static RectTransform Pinned(string name, Transform parent)
        {
            RectTransform rect = Rect(name, parent, 0, 0, 0, 0); rect.pivot = Vector2.zero; return rect;
        }
        private static void Place(RectTransform rect, float x, float width, float height)
        {
            rect.anchoredPosition = new Vector2(x, 0); rect.sizeDelta = new Vector2(width, height);
        }
        // Six bag columns span the screen width; wider bags and the side panels are reached by scrolling.
        private void LayoutBag(bool resetScroll)
        {
            RefreshContainerArt();
            // Cells fill the width in portrait but shrink on short screens so the bag never exceeds 45% of the height.
            float cell = Mathf.Min(safe.rect.width / VisibleColumns, safe.rect.height * 0.45f * 1000f / ledger.Height), height = cell * ledger.Height / 1000f;
            if (cell <= 0) return;
            bagFrame.anchorMin = new Vector2(0, BagLift); bagFrame.anchorMax = new Vector2(1, BagLift + height / safe.rect.height);
            float hooks = ledger.HookCount > 0 ? cell * (ledger.HookCount * (HookSlot + SideGap) + SideGap) : 0;
            float pockets = ledger.PocketCount > 0 ? cell * ((ledger.PocketCount + 1) / 2 * (PocketSlot + SideGap) + SideGap) : 0;
            float width = cell * ledger.Width / 1000f;
            Place(hookPanel, 0, hooks, height); Place(bag, hooks, width, height); Place(pocketPanel, hooks + width, pockets, height);
            contentWidth = hooks + width + pockets; bagContent.sizeDelta = new Vector2(contentWidth, height);
            laidOutWidth = safe.rect.width;
            RebuildSidePanels(cell, height);
            SetScroll(resetScroll ? -hooks : bagScroll);
            LayoutContainer();   // an open chest or rack sits above the bag (CollectionPrototype.HomeStorage.cs)
            LayoutGearPanel(); LayoutCraftPanel();
        }
        private void SetScroll(float offset)
        {
            float view = safe.rect.width;
            bagScroll = contentWidth <= view ? (view - contentWidth) * 0.5f : Mathf.Clamp(offset, view - contentWidth, 0);
            bagContent.anchoredPosition = new Vector2(bagScroll, 0);
        }
        // Only unlocked positions are shown. Hooks: one spaced row running left from the bag (hook 1 nearest),
        // sized for tools. Pockets: two spaced rows running right from the bag. Sizes are in bag-cell units.
        private const float HookSlot = 2f, PocketSlot = 1.1f, SideGap = 0.25f;
        private void RebuildSidePanels(float cell, float height)
        {
            ClearSide(hookPanel, hookCells); ClearSide(pocketPanel, pocketCells);
            float panelWidth = hookPanel.sizeDelta.x;
            if (ledger.HookCount > 0)
            {
                RectTransform belt = Pinned("Pants tool belt", hookPanel); Place(belt, 0, panelWidth, cell * 0.26f);
                belt.anchoredPosition = new Vector2(0, height - cell * 0.26f);
                InventoryContainerStyle.Apply(belt, InventoryContainerStyle.BeltSkin(EquippedContainerDefinition(GearSlot.Pants)));
            }
            for (int i = 0; i < ledger.HookCount; i++)
            {
                float x = panelWidth - (i + 1) * (HookSlot + SideGap) * cell;
                hookCells.Add(SideCell(hookPanel, ItemOwner.Hook, i, "H", x, cell * 0.15f, cell * HookSlot, height - cell * 0.35f));
            }
            float rowHeight = (height - cell * SideGap * 3) / 2;
            for (int i = 0; i < ledger.PocketCount; i++)
            {
                int column = i / 2, row = i % 2;
                float x = cell * (SideGap + column * (PocketSlot + SideGap)), y = row == 0 ? height - cell * SideGap - rowHeight : cell * SideGap;
                pocketCells.Add(SideCell(pocketPanel, ItemOwner.Pocket, i, "P", x, y, cell * PocketSlot, rowHeight));
            }
        }
        private static void ClearSide(RectTransform panel, List<RectTransform> cells)
        {
            foreach (Transform child in panel) Destroy(child.gameObject);
            cells.Clear();
        }
        private RectTransform SideCell(RectTransform panel, ItemOwner owner, int index, string prefix, float x, float y, float width, float height)
        {
            RectTransform cellRect = Pinned(prefix + (index + 1), panel); Place(cellRect, x, width, height); cellRect.anchoredPosition = new Vector2(x, y);
            ItemEntry occupant = null; foreach (ItemEntry item in ledger.Entries) if (item.Owner == owner && item.X == index) { occupant = item; break; }
            bool isHook = owner == ItemOwner.Hook;
            string cellSkin = isHook ? InventoryContainerStyle.LoopSkin(EquippedContainerDefinition(GearSlot.Pants)) : InventoryContainerStyle.PocketSkin(EquippedContainerDefinition(GearSlot.Shirt));
            InventoryContainerStyle.Apply(cellRect, cellSkin);
            if (occupant == null)
            {
                Text number = Label(cellRect, (index + 1).ToString(), 0.1f, 0.08f, 0.9f, 0.2f, 13);
                number.color = new Color(.78f,.7f,.53f,.65f);
            }
            else DrawItemView(Rect("Item", cellRect, 0.08f, 0.08f, 0.92f, isHook ? 0.98f : 0.81f), occupant, 16);
            if (isHook)
                InventoryContainerStyle.Apply(Rect("Front retaining loop", cellRect, 0, 0, 1, 1), cellSkin, true);
            // Pending items keep their crafting timer in pockets and hooks too (drawn over the hook's front loop).
            if (occupant != null && !occupant.Ready) AddPendingBadge(Rect("Pending", cellRect, 0, 0, 1, 1), occupant);
            return cellRect;
        }
        private ItemDefinition EquippedContainerDefinition(GearSlot slot)
        {
            WornGear worn = equipment?.Get(slot);
            if (worn == null) return null;
            foreach (ItemEntry item in ledger.Entries) if (item.Id == worn.InstanceId) return item.Definition;
            // Legacy debug gear can lack a matching inventory entry; use its existing fixture identity for art only.
            return slot == GearSlot.Backpack ? PrototypeItems.TestBackpack : slot == GearSlot.Shirt ? PrototypeItems.TestShirt : PrototypeItems.TestPants;
        }
        private void RefreshContainerArt()
        {
            if (bag == null || aperture == null) return;
            ItemDefinition pack = EquippedContainerDefinition(GearSlot.Backpack);
            InventoryContainerStyle.Apply(bag, InventoryContainerStyle.BagSkin(pack));
            InventoryContainerStyle.Apply(aperture, InventoryContainerStyle.OpeningSkin(pack));
        }
        private bool SideCellAt(Vector2 screen, out bool hook, out int index)
        {
            for (index = 0; index < hookCells.Count; index++) if (RectTransformUtility.RectangleContainsScreenPoint(hookCells[index], screen)) { hook = true; return true; }
            for (index = 0; index < pocketCells.Count; index++) if (RectTransformUtility.RectangleContainsScreenPoint(pocketCells[index], screen)) { hook = false; return true; }
            hook = false; index = -1; return false;
        }
        private void ToggleInventory()
        {
            if (saveBlocked || (milestone4 && stamina.Fainted) || TravelInputLocked) return;
            if (inventoryOpen) CloseContainer();   // the return icon closes an open chest or rack with the bag
            router.CancelAll(); inventoryOpen = !inventoryOpen; SetMenu(false);
            if (!inventoryOpen && gearPanel != null) { gearPanel.SetActive(false); detailsPanel.SetActive(false); }
            if (!inventoryOpen && craftPanel != null) craftPanel.SetActive(false);
            if (inventoryOpen) movement.OpenInventory(); else movement.CloseInventory();
            LayoutBackpack();
            Say(inventoryOpen ? "Hold a bag item, then drag to move or touch a compatible stack." : "Walking intent restored. World pickups remain in range only.");
            SaveNow();
        }
        private TheTrailWalker.Domain.Settlement.LumberFieldState lumber = new TheTrailWalker.Domain.Settlement.LumberFieldState();
        private TheTrailWalker.Domain.Settlement.ShopState shops = new TheTrailWalker.Domain.Settlement.ShopState();
        private TheTrailWalker.Domain.Settlement.BalloonState balloon = new TheTrailWalker.Domain.Settlement.BalloonState();
        private TheTrailWalker.Domain.Pets.PetState pets = new TheTrailWalker.Domain.Pets.PetState();   // Milestone 15
        private void AdvanceShopVisit() => TheTrailWalker.Domain.Settlement.ShopService.AdvanceVisit(shops);

        private void ConfirmReset()
        {
            if (!resetPending || Time.unscaledTime > resetDeadline) { resetPending = true; resetDeadline = Time.unscaledTime + 5f; Say("Tap RESET SAVE again within five seconds to archive this profile and start over."); return; }
            resetPending = false;
            try
            {
                saveStore.ArchiveForReset(); saveBlocked = false; lastSavedState = null; crafting = new CraftingState(); missions = new TheTrailWalker.Domain.Missions.MissionState(); settlement = TheTrailWalker.Domain.Travel.SettlementState.New(0); home = TheTrailWalker.Domain.Settlement.HomeState.New(); homeStorage = new TheTrailWalker.Domain.Settlement.HomeStorage(); warehouse = new TheTrailWalker.Domain.Economy.WarehouseState(); warehouseLog = new List<TheTrailWalker.Domain.Economy.TradeRecord>(); lumber = new TheTrailWalker.Domain.Settlement.LumberFieldState(); shops = new TheTrailWalker.Domain.Settlement.ShopState(); balloon = new TheTrailWalker.Domain.Settlement.BalloonState(); pets = new TheTrailWalker.Domain.Pets.PetState();
                CloseContainer();
                if (inventoryOpen) { movement.CloseInventory(); inventoryOpen = false; LayoutBackpack(); }
                router.CancelAll(); movement.Reset();
                if (milestone4) { equipment = new EquipmentState(); stamina = new StaminaState(); router.InteractionLocked = false; movement.SetSpeedCap(MovementState.StarterSpeedCap); faintEventId = null; faintLoss.Clear(); if (gearPanel != null) gearPanel.SetActive(false); }
                if (milestone5)
                {
                    ledger = new SpatialInventory(equipment.BackpackWidth * 1000, equipment.BackpackHeight * 1000);
                    ledger.Reconfigure(ledger.Width, ledger.Height, equipment.Pockets, equipment.Hooks, out _);
                    StartTravelProfile(); movement.SetPause(MovementPauseReasons.Seated, true); lastDistance = 0; phase = TravelPhase.Transition;
                    SaveNow(); if (!saveBlocked && scene.RequestReload != null) scene.RequestReload();
                    return;
                }
                motor.Present(); lastDistance = movement.Distance; ResetFixture(0); SaveNow();
            }
            catch (Exception error) { saveBlocked = true; Say("Reset failed; existing data was preserved. " + error.Message); }
        }
        private static Color ColorFor(ItemDefinition definition)
        {
            if (definition == PrototypeItems.Branch) return new Color(0.61f, 0.36f, 0.16f);
            if (definition == PrototypeItems.Log) return new Color(0.43f, 0.23f, 0.1f);
            if (definition == PrototypeItems.Stone) return new Color(0.48f, 0.54f, 0.59f);
            if (definition == PrototypeItems.Hemp) return new Color(0.3f, 0.63f, 0.32f);
            if (definition == PrototypeItems.Berries) return new Color(0.67f, 0.17f, 0.31f);
            if (definition == PrototypeItems.TestBread) return new Color(0.82f, 0.62f, 0.36f);
            if (definition.Category == ItemCategory.RawIngredient) return new Color(0.55f, 0.47f, 0.3f);
            if (definition.Category == ItemCategory.RefinedIngredient) return new Color(0.76f, 0.7f, 0.56f);
            if (definition.Category == ItemCategory.PreparedFood) return new Color(0.86f, 0.45f, 0.22f);
            return new Color(0.85f, 0.65f, 0.2f);
        }
        public void ResetFixture(int preset)
        {
            if (saveBlocked || (milestone4 && stamina.Fainted)) return;
            router.CancelAll();
            var retainedWorn = new List<ItemEntry>();
            if (milestone4 && ledger != null)
                foreach (ItemEntry item in ledger.Entries)
                {
                    if (item.Owner != ItemOwner.Worn || !item.Definition.GearSlot.HasValue) continue;
                    WornGear worn = equipment.Get(item.Definition.GearSlot.Value);
                    if (worn != null && worn.InstanceId == item.Id) retainedWorn.Add(item);
                }
            foreach (Bundle bundle in bundles) { Destroy(bundle.View); Destroy(bundle.Badge.gameObject); }
            bundles.Clear(); ledger = milestone4 ? new SpatialInventory(equipment.BackpackWidth * 1000, equipment.BackpackHeight * 1000) : new SpatialInventory(); fixtureCycle = Guid.NewGuid().ToString("N"); revision = -1;
            if (milestone4) { ledger.Reconfigure(ledger.Width, ledger.Height, equipment.Pockets, equipment.Hooks, out _); faintEventId = null; faintLoss.Clear(); selected = null; selectedGearSlot = null; }
            foreach (ItemEntry retained in retainedWorn)
            {
                ItemEntry copy = ledger.AddWorld(retained.Id, retained.Definition, retained.Quantity, retained.Quality, retained.Profile, retained.Ready, retained.TrailDistance);
                if (!ledger.RestoreWear(copy, retained.HasWearRemaining, retained.WearRemaining)) throw new InvalidOperationException("Could not retain wearable durability during fixture reset");
                if (!ledger.RestoreStored(copy, ItemOwner.Worn, 0, 0)) throw new InvalidOperationException("Could not retain worn item during fixture reset");
            }
            if (milestone5) ResetCraftingBatches();
            if (preset == 1)
            {
                AddBag("mixed-1", PrototypeItems.Berries, 1, 1, 0, 0);
                AddBag("mixed-999", PrototypeItems.Berries, 999, 1, 1400, 0);
                AddBag("mixed-1000", PrototypeItems.Branch, 1000, 3, 0, 1700);
                AddBag("mixed-profile", PrototypeItems.Berries, 1, 1, 3000, 0, new ItemProfile(200));
                AddBag("mixed-tool", PrototypeItems.Tool, 1, 2, 3000, 2000, new ItemProfile(100, 30, 30));
                AddBag("mixed-quality", PrototypeItems.Berries, 1, 5, 4300, 0);
                if (milestone4) AddBag("mixed-hat", PrototypeItems.TestHat, 1, 5, 5000, 3000, new ItemProfile(100, 180, 180));
                if (milestone4) AddBag("mixed-bread", PrototypeItems.TestBread, 3, 5, 4300, 1000);
            }
            if (preset == 2)
            {
                // Single items are ~1.3x base (stack growth 0.15): a stone grid plus a strip of single berries fills the bag.
                int cell = (int)Math.Ceiling(PrototypeItems.Stone.Width * ItemEntry.Scale(1)), nut = (int)Math.Ceiling(PrototypeItems.Berries.Height * ItemEntry.Scale(1));
                for (int y = 0; (y + 1) * cell <= ledger.Height; y++) for (int x = 0; (x + 1) * cell <= ledger.Width; x++) AddBag("full-" + x + "-" + y, PrototypeItems.Stone, 1, 5, x * cell, y * cell);
                int stripX = ledger.Width / cell * cell;
                for (int y = 0; (y + 1) * nut <= ledger.Height && stripX + nut <= ledger.Width; y++) AddBag("full-strip-" + y, PrototypeItems.Berries, 1, 5, stripX, y * nut);
            }
            if (preset == 3)
            {
                // Tree-food retest fixture (section 7.1.1): dishes in several quantities, refined inputs, a pending (not ready)
                // dish, and a partial raw stack that a matching Q1 trail stack can top up. Not a recipe or harvest grant.
                AddBag("tree-dish-1000", PrototypeItems.ToastedPineNuts, 1000, 3, 0, 0);
                AddBag("tree-drink", PrototypeItems.BirchSapMilkDrink, 3, 3, 1300, 0);
                AddBag("tree-cakes", PrototypeItems.MesquitePodCakes, 5, 4, 2350, 0);
                AddBag("tree-biscuits", PrototypeItems.AcornBiscuits, 2, 5, 3550, 0);
                AddBag("tree-cakes-pending", PrototypeItems.MesquitePodCakes, 1, 4, 4750, 0, ready: false);
                AddBag("tree-flour-999", PrototypeItems.MesquiteFlour, 999, 2, 0, 1300);
                AddBag("tree-meal-1", PrototypeItems.RoastedAcornMeal, 1, 5, 1300, 1300);
                AddBag("tree-nuts-400", PrototypeItems.PineNuts, 400, 1, 2500, 1300);
            }
            // Debug fixture, not a regional spawn table: logs/tools are explicit test grants.
            ItemDefinition[] worldSet = preset == 3 ? TreeWorldFixture : PrototypeItems.All;
            int worldKinds = preset == 3 ? TreeWorldFixture.Length : 6;
            // In the camp world the trail holds only the region's own drops and harvest rewards (user feedback: Stone and
            // Red berries are not region-one gathering items), so fixtures only reset the bag there.
            for (int i = 0; i < (milestone5 ? 0 : 18); i++)
            {
                ItemDefinition definition = worldSet[i % worldKinds];
                float distance = Mathf.Clamp(movement.Distance + 2 + (i / 2) * 6f, 0, path.Length);
                var item = ledger.AddWorld("fixture-" + fixtureCycle + "-" + i, definition, definition.Stackable ? 1000 : 1, i % 5 + 1,
                    definition.Stackable ? new ItemProfile(100) : new ItemProfile(100, 30, 30), distance: distance);
                if (milestone5) ledger.AssignSegment(item, CurrentSegmentId);
                path.Evaluate(distance, out Vector3 p, out Vector3 t); Vector3 side = Vector3.Cross(Vector3.up, t);
                AddBundle(item, p + side * (i % 2 == 0 ? -1.9f : 1.9f) + Vector3.up * 0.45f);
            }
            Say(preset == 0 ? "Empty bag. Every stackable trail bundle contains 1,000." : preset == 1 ? "Mixed test: 1 / 999 / 1,000, Q1-Q5, different values and one unique axe." : preset == 2 ? "Bag full. Failed pickups must remain on the trail." : "Tree food test: raw tree ingredients and dishes on the trail; dishes, refined inputs and a pending dish in the bag.");
            Redraw(); WriteReport("fixture");
            if (initialized) SaveNow();
        }
        // The item lands about 2.5 m in front of the current look direction, so it is visible and can be picked up again.
        // It stays until collected; removal at the next camp-return drop cycle arrives with segment cycles (Milestones 5-6).
        private void DropToTrail(int pointer, Contact contact)
        {
            ItemEntry item = contact.Item;
            bool dropped = ledger.TryDropToTrail(item, pointer, movement.Distance, out string reason, CurrentSegmentId);
            Release(pointer, contact);
            if (dropped)
            {
                Vector3 look = Vector3.ProjectOnPlane(cameraView.transform.forward, Vector3.up).normalized;
                Vector3 position = motor.transform.position + look * 2.5f + Vector3.up * 0.45f;
                // A former trail pickup keeps its bundle view; reuse it so the item never has two views.
                Bundle existing = bundles.Find(b => b.Item == item);
                if (existing != null) existing.View.transform.position = position; else AddBundle(item, position);
                if (selected == item) selected = null;
                SaveNow();
            }
            Say(reason); Redraw(); WriteReport(dropped ? "dropped" : "rejected");
        }
        private void AddBundle(ItemEntry item, Vector3 position)
        {
            var model = GameObject.CreatePrimitive(PrimitiveType.Cube); model.name = item.Id; model.transform.SetParent(transform, false);
            model.transform.position = position;
            model.GetComponent<Renderer>().sharedMaterial = material;
            var color = new MaterialPropertyBlock(); color.SetColor("_BaseColor", ColorFor(item.Definition)); model.GetComponent<Renderer>().SetPropertyBlock(color);
            GameObject visual = ResourceVisualTuning.Attach(model, item.Definition);
            if (visual != null) visual.transform.localRotation = ResourceVisualTuning.TrailRotation(item.Definition);
            var badge = Label(safe, "", 0, 0, 0, 0, 19); badge.rectTransform.sizeDelta = new Vector2(170, 82);
            badge.alignment = TextAnchor.LowerCenter;
            var bundle = new Bundle { Item = item, View = model, Collider = model.GetComponent<Collider>(), Badge = badge };
            if (visual != null)
            {
                // Mesh bounds in the pickup's own space: the pick box matches the mesh, and the largest front dimension
                // matches the item's inventory footprint, so world and bag sizes agree.
                Bounds local = LocalMeshBounds(model.transform, visual);
                var box = (BoxCollider)bundle.Collider; box.center = local.center; box.size = local.size;
                bundle.Visual = visual.transform; bundle.MeshBottom = local.min.y; bundle.MeshCentre = local.center; bundle.MeshSize = local.size;
                bundle.MeshScale = ResourceVisualTuning.World(item.Definition) * Mathf.Max(item.Definition.Width, item.Definition.Height) / 1000f / Mathf.Max(0.01f, Mathf.Max(local.size.x, local.size.y));
            }
            bundles.Add(bundle);
        }
        private static Bounds LocalMeshBounds(Transform root, GameObject visual)
        {
            var bounds = new Bounds(); bool first = true;
            foreach (MeshFilter filter in visual.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                Bounds mesh = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = mesh.center + Vector3.Scale(mesh.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 point = root.InverseTransformPoint(filter.transform.TransformPoint(corner));
                    if (first) { bounds = new Bounds(point, Vector3.zero); first = false; } else bounds.Encapsulate(point);
                }
            }
            return bounds;
        }
        private static readonly ItemDefinition[] TreeWorldFixture = {
            PrototypeItems.PineNuts, PrototypeItems.BirchSap, PrototypeItems.MesquitePods, PrototypeItems.Acorns,
            PrototypeItems.ToastedPineNuts, PrototypeItems.BirchSapMilkDrink, PrototypeItems.MesquitePodCakes, PrototypeItems.AcornBiscuits };
        private void AddBag(string id, ItemDefinition definition, int quantity, int quality, int x, int y, ItemProfile? profile = null, bool ready = true)
        {
            ItemEntry item = ledger.AddWorld(fixtureCycle + "-" + id, definition, quantity, quality, profile, ready);
            ledger.Reserve(item, int.MinValue);
            if (!ledger.TryPlace(item, int.MinValue, x, y, false, out string reason)) throw new InvalidOperationException("Invalid debug fixture: " + reason);
            ledger.Release(item, int.MinValue);
        }
        private bool Reachable(Bundle bundle, out string reason)
        {
            reason = "";
            if (bundle.Item.Id.StartsWith(SessionSaveStore.LumberPilePrefix, StringComparison.Ordinal)) return true;   // lumber piles: framed by the pickup screen, the trunk may sit in front
            reason = "Pickup is outside the visible view";
            Vector3 viewport = cameraView.WorldToViewportPoint(bundle.Collider.bounds.center);
            if (!InVisibleView(viewport, cameraView.nearClipPlane, cameraView.farClipPlane)) return false;
            reason = "Pickup is blocked by scenery";
            return ClearLine(cameraView.transform.position, bundle);
        }
        public static bool InVisibleView(Vector3 viewport, float near, float far) =>
            viewport.z >= near && viewport.z <= far && viewport.x >= 0 && viewport.x <= 1 && viewport.y >= 0 && viewport.y <= 1;
        private bool ClearLine(Vector3 start, Bundle bundle)
        {
            Vector3 delta = bundle.Collider.bounds.center - start;
            int count = Physics.RaycastNonAlloc(start, delta.normalized, occlusionHits, delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
            if (count == occlusionHits.Length) return false;
            for (int i = 0; i < count; i++)
                if (occlusionHits[i].collider != bundle.Collider && !occlusionHits[i].transform.IsChildOf(motor.transform)) return false;
            return true;
        }
        public bool Begin(int pointer, Vector2 position)
        {
            if (InSettlementScene) return PickupPointerBegin(pointer, position) || SettlementPointerBegin(pointer, position); // lumber piles, else street drag and tap
            if (saveBlocked || (milestone4 && stamina.Fainted)) return false;
            // Interactables first: their selection colliders are triggers, which the pickup ray below ignores (and that
            // ray finds nothing at all when only an interactable is under the finger).
            if (BalloonOpen) return LumberPointerBegin(pointer, position);   // the balloon counter shares the counter tap tracking
            if (milestone5 && TryTapMissionLady(position)) return true; // the mission lady opens the mission papers
            if (milestone5 && TryTapBalloon(position)) return true;   // seated at a host camp, the balloon opens its interior
            if (milestone5 && TryStartHarvestAt(position)) return true;
            if (!Physics.Raycast(cameraView.ScreenPointToRay(position), out RaycastHit hit, cameraView.farClipPlane, ~0, QueryTriggerInteraction.Ignore)) return false;
            Bundle bundle = bundles.Find(b => b.Collider == hit.collider && b.Item.Owner == ItemOwner.World);
            if (bundle == null) return false;
            if (!Reachable(bundle, out string reason)) { rejected++; Say(reason); return true; }
            if (!ledger.Reserve(bundle.Item, pointer)) { Say("That pickup is already held"); return true; }
            contacts[pointer] = new Contact { Item = bundle.Item, Bundle = bundle, World = true, Start = position, Position = position, Started = Time.unscaledTime };
            return true;
        }
        public void BeginBag(int pointer, Vector2 position)
        {
            if (saveBlocked || (milestone4 && stamina.Fainted)) return;
            if (!inventoryOpen) { Say("Open the backpack to move its contents"); return; }
            if (ContainerPointerBegin(pointer, position)) return;   // the open chest or rack panel
            if (SideCellAt(position, out bool hook, out int index))
            {
                ItemOwner owner = hook ? ItemOwner.Hook : ItemOwner.Pocket;
                foreach (ItemEntry item in ledger.Entries)
                {
                    if (item.Owner != owner || item.X != index) continue;
                    if (!ledger.Reserve(item, pointer)) return;
                    selected = item;
                    contacts[pointer] = new Contact { Item = item, Side = true, Start = position, Position = position, Started = Time.unscaledTime };
                    Say($"{item.Definition.Name} / {item.Quantity}"); return;
                }
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(bag, position))
            {
                Vector2 point = BagPoint(position);
                foreach (ItemEntry item in ledger.Entries)
                {
                    if (item.Owner != ItemOwner.Backpack || point.x < item.X || point.x > item.X + item.Width || point.y < item.Y || point.y > item.Y + item.Height) continue;
                    if (!ledger.Reserve(item, pointer)) return;
                    selected = item;
                    contacts[pointer] = new Contact { Item = item, Start = position, Position = position, Started = Time.unscaledTime, Offset = point - new Vector2(item.X, item.Y) };
                    Say($"{item.Definition.Name} / {item.Quantity} / {(item.Ready ? "Ready" : "Pending")} / Value {TheTrailWalker.Domain.Economy.Money.Format(item.Profile.UnitValue)} cash each" + (item.Definition.Stackable ? "" : " / Durability " + DurabilityText(item.Definition, item.Profile.Durability, item.Profile.MaxDurability))); return;
                }
            }
            // Touching the container rather than an item scrolls it sideways.
            scrollPointer = pointer; scrollStartX = position.x; scrollStartOffset = bagScroll;
        }
        public void Move(int pointer, Vector2 position)
        {
            if (SettlementPointerMove(pointer, position)) return;
            if (ContainerScrollMove(pointer, position)) return;
            if (scrollPointer == pointer)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(safe, position, null, out Vector2 now);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(safe, new Vector2(scrollStartX, position.y), null, out Vector2 start);
                SetScroll(scrollStartOffset + now.x - start.x); return;
            }
            if (!contacts.TryGetValue(pointer, out Contact contact)) return;
            // Holding a dragged item near either screen edge scrolls toward the hooks or pockets.
            if (contact.Dragging && !contact.World && inventoryOpen && !ContainerEdgeScroll(position) && WarehouseCrateAt(position) < 0)   // no bag scroll over a crate
            {
                float step = safe.rect.width * Time.unscaledDeltaTime;
                if (position.x < Screen.width * 0.08f) SetScroll(bagScroll + step);
                else if (position.x > Screen.width * 0.92f) SetScroll(bagScroll - step);
            }
            contact.Position = position; float elapsed = Time.unscaledTime - contact.Started; Vector2 delta = position - contact.Start;
            if (contact.World && !contact.Dragging && elapsed <= 0.25f && delta.y <= -Screen.height * 0.1f && -delta.y >= Mathf.Abs(delta.x) * 1.5f)
            { Commit(pointer, contact, true, position); return; }
            if (elapsed >= 0.25f && delta.sqrMagnitude > Mathf.Pow(Screen.height * 0.008f, 2))
            {
                contact.Dragging = true;
                if (contact.Ghost == null) contact.Ghost = DragGhost(contact.Item);
                PlaceGhost(contact.Ghost, position);
            }
        }
        public void End(int pointer, Vector2 position, bool canceled)
        {
            if (SettlementPointerEnd(pointer, position, canceled)) return;
            if (scrollPointer == pointer) { if (!canceled) Move(pointer, position); scrollPointer = null; return; }
            if (ContainerScrollEnd(pointer, position, canceled)) return;
            if (!contacts.TryGetValue(pointer, out Contact contact)) return;
            if (!canceled) Move(pointer, position);
            if (!contacts.ContainsKey(pointer)) return; // Quick storage already committed.
            if (EndContainerContact(pointer, contact, position, canceled)) return;   // to, from or within the chest or rack
            if (EndWarehouseContact(pointer, contact, position, canceled)) return;   // onto a warehouse crate, or a drag that filled crates
            if (!canceled && contact.Dragging && !contact.World && inventoryOpen && dropPocket.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(dropPocket, position))
            { DropToTrail(pointer, contact); return; }
            if (!canceled && contact.Dragging && !contact.World && inventoryOpen && SideCellAt(position, out bool hook, out int index))
            {
                string reason = "Move side items back into the bag first";
                bool moved = contact.Item.Owner == ItemOwner.Backpack && ledger.TryMoveToSide(contact.Item, pointer, hook, index, out reason);
                Release(pointer, contact); Say(reason); Redraw(); if (moved) SaveNow(); return;
            }
            if (!canceled && contact.Dragging && !contact.World && milestone4 && gearPanel.activeSelf && contact.Item.Definition.GearSlot.HasValue &&
                RectTransformUtility.RectangleContainsScreenPoint((RectTransform)gearPanel.transform, position))
            { Release(pointer, contact); EquipItem(contact.Item); return; }
            if (!canceled && contact.Dragging) Commit(pointer, contact, OverOpening(position), position);
            else
            {
                if (milestone4 && ShowsDetailsOnRelease(canceled, contact.Dragging, contact.World, Time.unscaledTime - contact.Started)) ShowDetails(contact.Item);
                Release(pointer, contact);
            }
        }
        private void Commit(int pointer, Contact contact, bool quick, Vector2 position)
        {
            if (contact.Source != null || contact.RackSpot >= 0) { EndContainerContact(pointer, contact, position, false); return; }
            bool success = false; string reason = "Drop inside the backpack; source unchanged";
            string pickedId = contact.World ? contact.Item.Definition.Id : null; int pickedBefore = contact.Item.Quantity;
            if (contact.World && !Reachable(contact.Bundle, out reason)) { }
            else if (quick && contact.World) success = ledger.TryQuickStore(contact.Item, pointer, out reason);
            else if (quick && contact.Side) success = ledger.TryReturnToBackpack(contact.Item, pointer, out reason);
            else if (inventoryOpen && RectTransformUtility.RectangleContainsScreenPoint(bag, position))
            {
                Vector2 point = BagPoint(position) - (contact.World || contact.Side ? new Vector2(contact.Item.Width, contact.Item.Height) * 0.5f : contact.Offset);
                success = ledger.TryPlace(contact.Item, pointer, Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), true, out reason);
            }
            if (success) accepted++; else rejected++;
            if (success && pickedId != null) statistics.AddGathered(pickedId, Stored(contact.Item, pickedBefore));
            Release(pointer, contact); Say(PileRefusal(contact.Item, reason));
            bool pile = success && contact.World && contact.Item.Id.StartsWith(SessionSaveStore.LumberPilePrefix, StringComparison.Ordinal);
            if (pile) LumberTaken(contact.Item, Stored(contact.Item, pickedBefore));   // the Take and the bag change share its one save
            Redraw(); if (success && !pile) SaveNow(); WriteReport(success ? "transfer" : "rejected");
        }
        private Vector2 BagPoint(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bag, screen, null, out Vector2 local);
            return new Vector2((local.x - bag.rect.xMin) / bag.rect.width * ledger.Width, (local.y - bag.rect.yMin) / bag.rect.height * ledger.Height);
        }
        private void Release(int pointer, Contact contact)
        {
            if (contact.Source != null) CancelContainerHold(contact.Source, contact.Item, pointer); else ledger.Release(contact.Item, pointer);
            if (contact.Ghost != null) Destroy(contact.Ghost.gameObject); contacts.Remove(pointer);
        }
        public void CancelAll()
        {
            foreach (var pair in contacts) { if (pair.Value.Source != null) CancelContainerHold(pair.Value.Source, pair.Value.Item, pair.Key); else ledger.Release(pair.Value.Item, pair.Key); if (pair.Value.Ghost != null) Destroy(pair.Value.Ghost.gameObject); }
            contacts.Clear(); scrollPointer = null; CancelSettlementPointer();
        }
        private void Redraw()
        {
            RefreshContainerArt();
            foreach (Transform child in itemsLayer) Destroy(child.gameObject);
            pendingLabels.Clear();
            foreach (ItemEntry item in ledger.Entries)
            {
                if (item.Owner != ItemOwner.Backpack) continue;
                RectTransform rect = Rect(item.Id, itemsLayer, (float)item.X / ledger.Width, (float)item.Y / ledger.Height, (float)(item.X + item.Width) / ledger.Width, (float)(item.Y + item.Height) / ledger.Height);
                DrawItemView(rect, item, 18);
                if (!item.Ready) AddPendingBadge(rect, item);
            }
            if (inventoryOpen) LayoutBag(false);
            revision = ledger.Revision;
        }
        // Meshed items show their model (about 10% larger than the footprint, which acts as the collision box) and
        // only a quantity; items without a model yet keep a coloured placeholder with their name.
        private static void DrawItemView(RectTransform rect, ItemEntry item, int size)
        {
            Sprite icon = ItemIconRenderer.For(ItemArt.ModelFor(item.Definition));
            if (icon != null)
            {
                Image picture = Rect("Model", rect, -0.05f, -0.05f, 1.05f, 1.05f).gameObject.AddComponent<Image>();
                picture.sprite = icon; picture.preserveAspect = true; picture.raycastTarget = false;
            }
            else
            {
                Image box = rect.gameObject.AddComponent<Image>(); box.color = ColorFor(item.Definition); box.raycastTarget = false;
                Text name = Label(rect, item.Definition.Name, 0.02f, 0.35f, 0.98f, 0.98f, size);
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 8; name.resizeTextMaxSize = size;
            }
            // Tools and wearables carry their durability on the model: uses for tools, walking distance for wearables.
            if (!item.Definition.Stackable && item.Profile.MaxDurability > 0)
            {
                bool wearable = item.Definition.GearSlot.HasValue;
                double remaining = wearable && item.HasWearRemaining ? item.WearRemaining : item.Profile.Durability;
                string value = wearable ? Distance(remaining) : remaining.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
                RectTransform badge = DurabilityStyle.AddBadge(rect, value, remaining, item.Profile.MaxDurability, true);
                badge.anchorMin = badge.anchorMax = new Vector2(0.5f, 0f); badge.pivot = new Vector2(0.5f, 0f);
                badge.sizeDelta = new Vector2(wearable ? 92f : 64f, 40f); badge.anchoredPosition = new Vector2(0, 2f);
            }
            string amount = ItemIconRenderer.Quantity(item);
            if (amount.Length == 0) return;
            // Fixed readable size in the bottom-right corner, even on thin items such as branches.
            Text count = Label(rect, amount, 1f, 0f, 1f, 0f, size);
            count.rectTransform.pivot = new Vector2(1f, 0f); count.rectTransform.sizeDelta = new Vector2(70f, 26f);
            count.alignment = TextAnchor.LowerRight; count.fontStyle = FontStyle.Bold;
            count.horizontalOverflow = HorizontalWrapMode.Overflow; count.verticalOverflow = VerticalWrapMode.Overflow;
            Outline edge = count.gameObject.AddComponent<Outline>(); edge.effectColor = new Color(0, 0, 0, 0.85f); edge.effectDistance = new Vector2(1.2f, -1.2f);
        }
        // Wearable durability is walking distance: 100 m and above shows in km ("0.12 km", "0.2 km", "50 km"). Tool
        // durability counts uses and stays a plain number.
        private static string Distance(double metres) => metres >= 100 ? (metres / 1000).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + " km" : metres.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + " m";
        private static string DurabilityText(ItemDefinition definition, double remaining, double maximum) =>
            definition.GearSlot.HasValue ? Distance(remaining) + " / " + Distance(maximum) : $"{remaining:0.##}/{maximum:0}";
        private void Say(string text) { feedback.text = text; }
        // Tube length follows the current boots cap; the fill shows requested speed within it.
        private void UpdateSpeedGauge()
        {
            bool show = TrailControlsVisible(InSettlementScene) && !inventoryOpen && (router.AdjustingSpeed || Time.unscaledTime - router.LastSpeedInput < 0.6f);
            speedGauge.gameObject.SetActive(show); speedValue.gameObject.SetActive(false); // no numerical readout (user)
            if (!show) return;
            float bottom = ThermometerBase, top = bottom + ThermometerFull * Mathf.Clamp01(movement.SpeedCap / MaximumSpeedCap);
            speedGauge.anchorMin = new Vector2(speedGauge.anchorMin.x, bottom);
            speedGauge.anchorMax = new Vector2(speedGauge.anchorMax.x, top);
            // Tiles keep a fixed size: one 64 x 128 tile spans the ruler's width, so a longer cap shows more ticks.
            Image tube = speedGauge.GetComponent<Image>();
            if (tube.type == Image.Type.Tiled && speedGauge.rect.width > 1f) tube.pixelsPerUnitMultiplier = 64f / speedGauge.rect.width;
            speedFill.anchorMax = new Vector2(speedFill.anchorMax.x, movement.SpeedCap > 0 ? Mathf.Clamp01(movement.RequestedSpeed / movement.SpeedCap) : 0f);
        }
        private void ExplainSpeedBlocked(MovementPauseReasons pauses, float drag)
        {
            if (milestone5 && phase == TravelPhase.Seated && drag > 0) { TryStandUp(drag); return; }
            if (milestone5 && (pauses & MovementPauseReasons.Seated) != 0) { if (phase == TravelPhase.Seated) Say(travel.CanStandUp(out string why) ? "Seated at camp. Swipe UP on the centre line to stand up." : why); return; }
            if (milestone4 && stamina.Fainted) Say("Recovering from fainting; walking resumes once hearts refill.");
            else if (inventoryOpen || (pauses & MovementPauseReasons.Inventory) != 0) Say("Walking is paused while the backpack is open. Tap CLOSE to resume.");
            else if ((pauses & MovementPauseReasons.Camp) != 0) Say("You are at camp; the next segment arrives in a later milestone. Use RESET SAVE or fixtures to test again.");
            else if ((pauses & MovementPauseReasons.Exhausted) != 0) Say("No stamina. Eat to recover before walking.");
            else Say("Walking is paused: " + pauses);
        }
        private void Update()
        {
            if (ledger == null) return;
            if (petSettlePending && initialized && !saveBlocked) { petSettlePending = false; SettlePetAbsence(); }   // a save loaded after the app was closed (Milestone 15)
            if (milestone4) AdvanceSurvival(Time.deltaTime);
            if (milestone5 && initialized && travel != null) UpdateTravel();
            if (milestone5 && initialized && travel != null) UpdateTrade();
            if (milestone5 && (TrailControlsVisible(InSettlementScene) || active != null)) UpdateHarvest();   // active: a massive cut in the settlement
            if (milestone5) UpdateCrafting();
            SyncScreenReturn();
            if (milestone5 && initialized && travel != null) UpdateMissions();
            if (milestone5 && initialized && travel != null) UpdateRouteCard();
            if (milestone4) UpdateFaintPresentation(Time.deltaTime); // CollectionPrototype.Faint.cs
            if (lastSafe != Screen.safeArea || lastSize != new Vector2Int(Screen.width, Screen.height)) ApplySafeArea();
            if (revision != ledger.Revision) Redraw();
            if (detailsPanel != null && detailsPanel.activeSelf && Time.unscaledTime >= detailsCloseAt) detailsPanel.SetActive(false);
            UpdateSpeedGauge();
            // The canvas has no size on the first frame and can change with insets; re-lay the bag when it does.
            if (inventoryOpen && Mathf.Abs(safe.rect.width - laidOutWidth) > 0.5f) LayoutBackpack();
            foreach (Bundle bundle in bundles)
            {
                bool visible = bundle.Item.Owner == ItemOwner.World;
                bundle.View.SetActive(visible);
                if (visible)
                {
                    float scale = bundle.Item.Definition.Stackable ? (float)ItemEntry.Scale(bundle.Item.Quantity) : 1f;
                    if (bundle.MeshScale > 0)
                    {
                        float uniform = bundle.MeshScale * scale;
                        bundle.View.transform.localScale = Vector3.one * uniform;
                        // Rest the mesh 2 cm above the ground under it at any stack size (see GroundUnder).
                        float lift = GroundUnder(bundle, uniform) + 0.02f - bundle.View.transform.position.y;
                        bundle.Visual.localPosition = new Vector3(0, lift / uniform - bundle.MeshBottom, 0);
                        // The pick box moves with the lowered model, so touches land on what is drawn.
                        ((BoxCollider)bundle.Collider).center = bundle.MeshCentre + bundle.Visual.localPosition;
                    }
                    else bundle.View.transform.localScale = new Vector3(bundle.Item.Width / 2000f, 0.3f * scale, bundle.Item.Height / 1500f);
                    bundle.Badge.gameObject.SetActive(bundle.Badge.text.Length > 0); // trail items show only their model; lumber piles carry a label
                }
                else bundle.Badge.gameObject.SetActive(false);
            }
            if (Time.unscaledTime >= nextHud)
            {
                nextHud = Time.unscaledTime + 0.15f;
                fpsAverage = Mathf.Lerp(fpsAverage, 1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime), 0.05f);
                status.text = $"{movement.Distance:F1}/{path.Length:F1} m | {movement.ActualSpeed:F2} m/s | Camera {orbit.Yaw:F0}° | {fpsAverage:F0} FPS\n{movement.Pauses} | Bag {ledger.Width / 1000} x {ledger.Height / 1000} | Pickups x1,000" + (milestone4 ? $"\nHearts {stamina.Hearts:F2}/{stamina.Maximum:F0}  Food delivering {stamina.PendingFood:F2}" : "") + (milestone5 ? TravelStatus() : "");
                if (milestone4 && gearPanel != null && gearPanel.activeSelf) RefreshGearPanel();
            }
            if (Time.unscaledTime >= nextReport) { nextReport = Time.unscaledTime + 3; WriteReport("sample"); }
            if (Time.unscaledTime >= nextSave) { nextSave = Time.unscaledTime + 2; SaveNow(); }
            CheckBackgroundSave();
            SyncAvatarVisibility();
        }
        private void ApplySafeArea()
        {
            lastSafe = Screen.safeArea; lastSize = new Vector2Int(Screen.width, Screen.height);
            safe.anchorMin = lastSafe.min / new Vector2(Screen.width, Screen.height); safe.anchorMax = lastSafe.max / new Vector2(Screen.width, Screen.height);
        }
        // The bag and its menus never survive leaving the app (user, October 31, 2026): a resumed process opens with them closed.
        private void OnApplicationPause(bool paused) { if (paused) { CancelAll(); if (initialized) { if (inventoryOpen && !WarehouseKeepsBagOnPause(WarehouseOpen)) ToggleInventory(); SetMenu(false); } BeginPetAbsence(); SaveNow(true); } else SettlePetAbsence(); }
        private void OnApplicationFocus(bool focused) { if (!focused) { CancelAll(); BeginPetAbsence(); SaveNow(true); } else SettlePetAbsence(); }
        private void OnApplicationQuit() { BeginPetAbsence(); SaveNow(true); }
        private void OnDestroy() { avatarView?.Destroy(); CancelAll(); if (router != null) { router.Resources = null; router.SpeedBlocked -= ExplainSpeedBlocked; } }
        private float LimitMovementSeconds(float delta)
        {
            if (saveBlocked || stamina == null || movement.ActualSpeed <= 0) return delta;
            double seconds = stamina.LimitWalkingSeconds(delta, movement.ActualSpeed, LegDrainPerMetre);
            foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot)))
            {
                WornGear gear = equipment.Get(slot);
                if (gear != null && !gear.Infinite) seconds = Math.Min(seconds, gear.RemainingDistance / movement.ActualSpeed);
            }
            return (float)seconds;
        }
        // Raw food never saves the player; only carried prepared (crafted/bought) food is eaten automatically.
        private float nextAutoEatTry;
        private bool TryAutoEatPreparedFood()
        {
            const int token = -834;
            foreach (ItemEntry item in ledger.Entries)
            {
                if ((item.Owner != ItemOwner.Backpack && item.Owner != ItemOwner.Pocket) || !item.Definition.PreparedFood || !item.Ready || item.Quantity <= 0) continue;
                if (!ledger.Reserve(item, token)) continue;
                bool consumed = ledger.TryConsumeOne(item, token, out _); ledger.Release(item, token);
                if (!consumed) continue;
                stamina.QueueFood(TheTrailWalker.Domain.Settlement.FurnitureBuffs.FoodHearts(item.Definition.FoodHearts, home.Buffs)); Redraw(); SaveNow();
                if (!saveBlocked) Say("Low stamina: ate " + item.Definition.Name + " automatically.");
                return true;
            }
            return false;
        }
        private void AdvanceSurvival(float delta)
        {
            if (saveBlocked || !initialized) return;
            bool foreground = (movement.Pauses & (MovementPauseReasons.Suspended | MovementPauseReasons.Unfocused | MovementPauseReasons.Loading)) == 0;
            float walked = Mathf.Max(0, movement.Distance - lastDistance); lastDistance = movement.Distance;
            statistics.AddDistance(walked);
            bool expiring = false;
            foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot)))
            {
                WornGear current = equipment.Get(slot);
                if (current != null && !current.Infinite && current.RemainingDistance <= walked) expiring = true;
            }
            if (expiring) router.CancelAll();
            var expired = new List<WornGear>(equipment.ApplyDistance(walked));
            // Float path coordinates can stop changing within a few micrometres of the boundary.
            // Finish that boundary only when walking was requested, so idle time never causes wear.
            if (foreground && movement.ActualSpeed > 0 && walked == 0)
                foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot)))
                {
                    WornGear current = equipment.Get(slot);
                    if (current == null || current.Infinite || current.RemainingDistance > 0.00002) continue;
                    router.CancelAll(); WornGear reached = equipment.ExpireForTest(slot); if (reached != null) expired.Add(reached);
                }
            if (expired.Count > 0)
            {
                if (!RemoveExpiredInstances(expired)) return;
                if (!ApplyCapabilities()) return;
                SaveNow(); if (!saveBlocked) Say(expired.Count + " worn item(s) expired; contents were retained.");
            }
            bool wasFainted = stamina.Fainted;
            bool safeArea = (movement.Pauses & (MovementPauseReasons.Camp | MovementPauseReasons.Seated)) != 0;
            stamina.Advance(delta, walked, safeArea, foreground, LegDrainPerMetre);
            if (!foreground) return;
            if (wasFainted && !stamina.Fainted)
            {
                // Input unlocks when the wake animation finishes (FinishWake); walking still needs a fresh upward gesture.
                movement.SetPause(MovementPauseReasons.Exhausted, false);
                SaveNow(); if (!saveBlocked) Say("Waking up...");
            }
            // Auto-eat at 0.1 hearts without stopping the walker (user, November 7, 2026); with no prepared food the
            // meter keeps falling and the zero branch below stops and faints as before. Retried at most once a second.
            if (stamina.ShouldAutoEat(safeArea) && Time.unscaledTime >= nextAutoEatTry)
            {
                nextAutoEatTry = Time.unscaledTime + 1f;
                if (TryAutoEatPreparedFood()) return;
            }
            if (stamina.IsEmpty && !stamina.Fainted)
            {
                // Also clears intent restored from an older save, so a stopped walker never keeps a requested speed.
                if ((movement.Pauses & MovementPauseReasons.Exhausted) == 0 || movement.RequestedSpeed > 0 || movement.InventoryResumeSpeed > 0 || !movement.PostHarvestStop)
                {
                    movement.BeginHarvest(); movement.EndHarvest(); movement.SetPause(MovementPauseReasons.Exhausted, true);
                }
                if (!safeArea && stamina.PendingFood <= 0 && TryAutoEatPreparedFood()) return;
                if (stamina.ShouldFaint(safeArea, false)) BeginFaint();
            }
            else if (!stamina.Fainted && !stamina.IsEmpty && (movement.Pauses & MovementPauseReasons.Exhausted) != 0)
                movement.SetPause(MovementPauseReasons.Exhausted, false);
        }
        private void BeginFaint()
        {
            statistics.AddFaint();
            CloseContainer();
            if (inventoryOpen) { movement.CloseInventory(); inventoryOpen = false; LayoutBackpack(); }
            if (gearPanel != null) gearPanel.SetActive(false);
            if (detailsPanel != null) detailsPanel.SetActive(false);
            router.CancelAll(); string id = Guid.NewGuid().ToString("N");
            var plan = ledger.PlanFaintLoss(id, maximum => faintRandom.Next(maximum));
            var lostArt = CaptureLoss(plan);
            if (!ledger.CommitFaintLoss(plan)) { saveBlocked = true; Say("Faint transaction failed; progress is paused."); return; }
            faintEventId = id; faintLoss.Clear();
            foreach (var lost in plan.Entries) faintLoss.Add(new SavedLoss { itemId = lost.Id, quantity = lost.Quantity });
            selected = null; selectedGearSlot = null; stamina.BeginFaint(); router.InteractionLocked = true;
            // A fainted walker is stopped and needs a fresh upward gesture, matching the saved faint invariants.
            movement.BeginHarvest(); movement.EndHarvest(); movement.SetPause(MovementPauseReasons.Exhausted, true);
            SaveNow();
            if (!saveBlocked) { Redraw(); StartFaintPresentation(lostArt); Say("Fainted: " + faintLoss.Count + " carried entries lost. Recovering in place."); }
        }
        private bool ApplyCapabilities()
        {
            if (!ledger.Reconfigure(equipment.BackpackWidth * 1000, equipment.BackpackHeight * 1000, equipment.Pockets, equipment.Hooks, out string reason))
            { saveBlocked = true; movement.SetPause(MovementPauseReasons.Exhausted, true); Say("Capacity transfer failed: " + reason); return false; }
            movement.SetSpeedCap((float)equipment.SpeedCap); stamina.SetMaximum(equipment.MaxHearts);
            Redraw(); if (inventoryOpen) LayoutBackpack(); return true;
        }
        private void EatSelected()
        {
            if (saveBlocked || stamina.Fainted || !inventoryOpen || selected == null) { Say("Open backpack and select ready food first."); return; }
            const int token = -828;
            if (!selected.Ready) { Say("Still crafting: it can be eaten when its timer finishes."); return; }
            if (!ledger.Reserve(selected, token)) { Say("Item is in use."); return; }
            double hearts = TheTrailWalker.Domain.Settlement.FurnitureBuffs.FoodHearts(selected.Definition.FoodHearts, home.Buffs);
            bool consumed = ledger.TryConsumeOne(selected, token, out string reason); ledger.Release(selected, token);
            if (!consumed) { Say(reason); return; }
            stamina.QueueFood(hearts); // Provisional debug values, not regional food balance.
            if (selected.Owner == ItemOwner.Consumed) selected = null;
            Redraw(); if (detailsPanel != null) detailsPanel.SetActive(false);
            SaveNow(); if (!saveBlocked) Say("A test serving queued one heart of gradual recovery.");
        }
        public static bool DetailsShowsEat(ItemEntry item, bool inBag) => inBag && item.Definition.FoodHearts > 0 && item.Ready;
        public static bool DetailsShowsEquip(ItemEntry item, bool inBag) => inBag && item.Definition.GearSlot.HasValue && item.Owner == ItemOwner.Backpack;
        private void ShowDetails(ItemEntry item, bool inBag = true)
        {
            if (detailsPanel == null || item == null || item.Owner == ItemOwner.Consumed) return;
            if (inBag) selected = item;
            string durability = !ShowsDurability(item.Definition) ? "" : "\nDurability " + DurabilityText(item.Definition, item.HasWearRemaining ? item.WearRemaining : item.Profile.Durability, item.Profile.MaxDurability);
            detailsText.text = $"{item.Definition.Name}  ({CategoryLabel(item.Definition)})\nQuantity {item.Quantity} | {(item.Ready ? "Ready" : "Pending")} | Value {TheTrailWalker.Domain.Economy.Money.Format(item.Profile.UnitValue)} cash each{durability}";
            detailsQuality.Bind(item, true);
            detailEat.SetActive(DetailsShowsEat(item, inBag));
            detailEquip.SetActive(DetailsShowsEquip(item, inBag));
            detailsPanel.transform.SetAsLastSibling(); // in front of the crafting and equipment menus
            detailsPanel.SetActive(true);
            // Auto-close when no action is taken; EAT and EQUIP close it immediately.
            detailsCloseAt = Time.unscaledTime + 4f;
        }
        // The opening is a thin strip at the screen edge; releasing anywhere over it or in the band just above it stores the item.
        private bool OverOpening(Vector2 screen)
        {
            var corners = new Vector3[4]; aperture.GetWorldCorners(corners);
            return screen.x >= corners[0].x && screen.x <= corners[2].x && screen.y >= corners[0].y - Screen.height * 0.02f && screen.y <= corners[2].y + Screen.height * OpeningDropBand;
        }
        private static string CategoryLabel(ItemDefinition definition)
        {
            switch (definition.Category)
            {
                case ItemCategory.RawIngredient: return "Raw ingredient - recipe input";
                case ItemCategory.RefinedIngredient: return "Refined ingredient - recipe input";
                case ItemCategory.RawFood: return $"Raw food - {definition.FoodHearts} heart(s), eat manually";
                case ItemCategory.PreparedFood: return $"Prepared food - {definition.FoodHearts} heart(s)";
                case ItemCategory.Tool: return "Tool";
                case ItemCategory.Wearable: return "Wearable";
                default: return "Material";
            }
        }
        private void DebugEmptyStamina()
        {
            if (saveBlocked || stamina.Fainted) return;
            stamina.Restore(0, stamina.Maximum, 0, false, 0);
            movement.BeginHarvest(); movement.EndHarvest(); movement.SetPause(MovementPauseReasons.Exhausted, true);
            SaveNow(); if (!saveBlocked) Say("Test stamina emptied. Ready carried food prevents fainting.");
        }
        private void ToggleGearPanel()
        {
            if (saveBlocked || stamina.Fainted) return;
            if (gearPanel.activeSelf)
            {
                gearPanel.SetActive(false); selectedGearSlot = null;
                // The gear view always shows with the whole bag, so closing it closes the bag too.
                if (inventoryOpen) ToggleInventory();
                return;
            }
            if (!inventoryOpen) ToggleInventory();
            gearPanel.transform.SetAsLastSibling(); LayoutGearPanel(); gearPanel.SetActive(true); RefreshGearPanel();
            // The return (bag) button stays on top, over the panel's empty lower-left corner.
            if (inventoryButton != null) inventoryButton.SetAsLastSibling();
            Say("Hold a wearable in the bag and drag it here to equip. Drag a slot down into the bag to take it off.");
        }
        // Rank-four test and spare gear lasts 50 km so later segments can be explored without constant breakage;
        // EXPIRE SLOT / EXPIRE ALL and the MIXED fixture (180 m hat) still cover expiry tests.
        private const int TestGearWear = 50000;
        private void GrantTestGear()
        {
            if (saveBlocked || stamina.Fainted) return;
            router.CancelAll();
            // Real inventory items, so taking them off returns them to the bag like any other wearable.
            foreach (ItemDefinition definition in PrototypeItems.TestWearables)
            {
                GearSlot slot = definition.GearSlot.Value;
                if (equipment.Get(slot) != null) continue;
                ItemEntry item = ledger.AddWorld("gear-" + Guid.NewGuid().ToString("N"), definition, 1, 5, new ItemProfile(100, TestGearWear, TestGearWear));
                if (!ledger.RestoreStored(item, ItemOwner.Worn, 0, 0)) throw new InvalidOperationException("Could not equip test gear");
                equipment.Equip(new WornGear(item.Id, slot, definition.GearRank, item.Quality, item.WearRemaining));
            }
            if (ApplyCapabilities()) { SaveNow(); Say("Rank-four test gear equipped (lasts 50 km of walking)."); }
        }
        private void GrantSpareGear()
        {
            if (saveBlocked || stamina.Fainted) return;
            router.CancelAll(); int overflowCount = 0;
            foreach (ItemDefinition definition in PrototypeItems.TestWearables)
            {
                var item = ledger.AddWorld("spare-" + Guid.NewGuid().ToString("N"), definition, 1, 5, new ItemProfile(100, TestGearWear, TestGearWear));
                const int token = -832;
                if (!ledger.Reserve(item, token)) throw new InvalidOperationException("Could not reserve debug gear");
                bool stored = ledger.TryQuickStore(item, token, out _); ledger.Release(item, token);
                if (!stored) { if (!ledger.RestoreStored(item, ItemOwner.Overflow, 0, 0)) throw new InvalidOperationException("Could not preserve debug gear"); overflowCount++; }
            }
            selected = null; Redraw(); SaveNow();
            if (!saveBlocked) Say("Added five spare test items. Overflow: " + overflowCount + ".");
        }
        private void ExpireTestGear(GearSlot? slot)
        {
            if (saveBlocked || stamina.Fainted) return;
            router.CancelAll();
            var expired = new List<WornGear>();
            if (slot.HasValue) { WornGear gear = equipment.ExpireForTest(slot.Value); if (gear != null) expired.Add(gear); }
            else expired.AddRange(equipment.ApplyDistance(double.MaxValue));
            if (!RemoveExpiredInstances(expired)) return;
            if (ApplyCapabilities()) { SaveNow(); Say("Expired gear removed. Contents that did not fit are in overflow."); }
        }
        private void ExpireSelectedSlot()
        {
            if (!selectedGearSlot.HasValue || equipment.Get(selectedGearSlot.Value) == null) { Say("Select a worn slot to test its expiry."); return; }
            ExpireTestGear(selectedGearSlot.Value); selectedGearSlot = null;
        }
        private bool RemoveExpiredInstances(IReadOnlyList<WornGear> expired)
        {
            foreach (WornGear gear in expired)
            {
                if (gear.InstanceId.StartsWith("debug-", StringComparison.Ordinal)) continue;
                if (ledger.ExpireWorn(gear.InstanceId)) continue;
                saveBlocked = true; movement.SetPause(MovementPauseReasons.Exhausted, true);
                Say("Equipment ownership mismatch. Progress is paused to protect the save."); return false;
            }
            return true;
        }
        // Tapping a slot only selects it; expiry needs the explicit EXPIRE SLOT button.
        private void GearSlotAction(GearSlot slot)
        {
            if (selected != null && selected.Owner == ItemOwner.Backpack && selected.Definition.GearSlot == slot) { EquipItem(selected); return; }
            selectedGearSlot = slot;
            Say(equipment.Get(slot) == null ? "Selected " + slot + " (default gear)." : "Selected " + slot + ". Drag it down into the bag to take it off" + (milestone5 ? "." : " or tap TAKE OFF."));
        }
        // Equips a backpack wearable into its own slot, swapping out any real item already worn there.
        private bool EquipItem(ItemEntry item)
        {
            if (saveBlocked || stamina.Fainted || !inventoryOpen) return false;
            if (!BagOwnsForEquip(ledger, item) || !item.Definition.GearSlot.HasValue) { Say("Choose a spare wearable from the backpack."); return false; }
            if (item.Profile.MaxDurability > 0 && item.Profile.Durability <= 0) { Say("That wearable has no remaining durability."); return false; }
            if (!item.Ready) { Say("Still crafting: it can be equipped when its timer finishes."); return false; }
            GearSlot slot = item.Definition.GearSlot.Value;
            // Swap without an in-between capacity change (user, November 7, 2026): taking the old item off first used to
            // re-apply the default gear's pockets/hooks (usually none) and empty them into the bag or overflow. Now the only
            // reconfigure uses the new item's counts, so pocket/hook contents stay when the new count is equal or greater.
            if (equipment.Get(slot) != null && !TakeOffSlot(slot, null, swapping: true)) return false;
            // A swapped-out backpack can shrink the bag and push the new item to overflow; bring it back first.
            const int token = -831;
            if (item.Owner == ItemOwner.Overflow && ledger.Reserve(item, token)) { ledger.TryReturnToBackpack(item, token, out _); ledger.Release(item, token); }
            router.CancelAll();
            if (!ledger.Reserve(item, token)) { Say("Item is in use."); return false; }
            int oldX = item.X, oldY = item.Y;
            try
            {
                WornGear gear = SpecialGear.Is(item.Definition.Id) ? new WornGear(item.Id, slot, item.Definition.GearRank, 5, 0, true, SpecialGear.Stat(item.Definition.Id))
                    : item.HasWearRemaining
                    ? new WornGear(item.Id, slot, item.Definition.GearRank, item.Quality, item.WearRemaining)
                    : WornGear.CreateTestGear(item.Id, slot, item.Definition.GearRank, item.Quality, 100);
                if (!ledger.TryWear(item, token, out string reason)) { Say(reason); return false; }
                try { equipment.Equip(gear); }
                catch { ledger.TryPlace(item, token, oldX, oldY, false, out _); throw; }
            }
            finally { ledger.Release(item, token); }
            if (selected == item) selected = null;
            selectedGearSlot = slot;
            if (detailsPanel != null) detailsPanel.SetActive(false);
            if (!ApplyCapabilities()) return false;
            SaveNow(); if (!saveBlocked) Say(item.Definition.Name + " equipped in " + slot + ".");
            return true;
        }
        private void UnequipSelectedGear()
        {
            if (!selectedGearSlot.HasValue) { Say("Select a worn slot first, or drag it down into the bag."); return; }
            if (TakeOffSlot(selectedGearSlot.Value, null)) selectedGearSlot = null;
        }
        // Special gear has no durability to show. Infinite gear without a special stat or a bag item is legacy test gear.
        public static bool ShowsDurability(ItemDefinition definition) => !definition.Stackable && !SpecialGear.Is(definition.Id);
        public static bool TakeOffAllowed(WornGear gear, bool hasBagItem) =>
            !gear.InstanceId.StartsWith("debug-", StringComparison.Ordinal) && (!gear.Infinite || (gear.StatOverride.HasValue && hasBagItem));
        // Returns the worn item to the bag, at the drop point when given; overflow only when the bag has no room.
        private bool TakeOffSlot(GearSlot slot, Vector2? drop, bool swapping = false)
        {
            if (saveBlocked || stamina.Fainted || !inventoryOpen) return false;
            WornGear gear = equipment.Get(slot);
            if (gear == null) { Say("That slot already has its default gear."); return false; }
            ItemEntry item = null; foreach (ItemEntry candidate in ledger.Entries) if (candidate.Id == gear.InstanceId && candidate.Owner == ItemOwner.Worn) { item = candidate; break; }
            if (!TakeOffAllowed(gear, item != null)) { Say("Older virtual test gear has no bag item; use EXPIRE SLOT."); return false; }
            if (item == null) { saveBlocked = true; movement.SetPause(MovementPauseReasons.Exhausted, true); Say("Worn item is missing; progress is paused."); return false; }
            router.CancelAll();
            if (!ledger.TakeOff(item.Id, gear.RemainingDistance)) { Say("Could not take off that wearable."); return false; }
            equipment.Unequip(slot);
            if (!swapping && !ApplyCapabilities()) return false;   // a swap applies capacity once, after the new item is on
            const int token = -833; string reason = "No backpack space"; bool placed = false;
            if (ledger.Reserve(item, token))
            {
                if (drop.HasValue && RectTransformUtility.RectangleContainsScreenPoint(bag, drop.Value))
                {
                    Vector2 point = BagPoint(drop.Value) - new Vector2(item.Width, item.Height) * 0.5f;
                    placed = ledger.TryPlace(item, token, Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), false, out reason);
                }
                if (!placed) placed = ledger.TryReturnToBackpack(item, token, out reason);
                ledger.Release(item, token);
            }
            if (swapping) return true;   // the equip that follows saves and reports once
            selected = item; Redraw(); SaveNow();
            if (!saveBlocked) Say(placed ? item.Definition.Name + " returned to the backpack." : reason + "; " + item.Definition.Name + " is in overflow (CLAIM OVERFLOW).");
            return true;
        }
        public void BeginSlotDrag(GearSlot slot, Vector2 position)
        {
            WornGear gear = equipment.Get(slot);
            if (gear == null || !inventoryOpen || saveBlocked || stamina.Fainted) return;
            selectedGearSlot = slot;
            ItemEntry worn = null; foreach (ItemEntry entry in ledger.Entries) if (entry.Id == gear.InstanceId) { worn = entry; break; }
            if (slotGhost == null && worn != null) slotGhost = DragGhost(worn);
            if (slotGhost != null) PlaceGhost(slotGhost, position);
        }
        public void DragSlot(Vector2 position) { if (slotGhost != null) PlaceGhost(slotGhost, position); }
        // Dragged items show the item itself (its bag picture at its bag size, durability badge included), slightly
        // transparent and lifted above the finger; drop targets still use the touch position.
        private RectTransform DragGhost(ItemEntry item)
        {
            float cell = bag.rect.width * 1000f / Mathf.Max(1, ledger.Width);
            if (cell <= 0) cell = 60f;
            var size = new Vector2(item.Width / 1000f * cell, item.Height / 1000f * cell);
            float limit = 320f / Mathf.Max(size.x, size.y); if (limit < 1f) size *= limit;
            RectTransform ghost = Rect("Drag ghost", safe, 0, 0, 0, 0);
            ghost.sizeDelta = size; ghost.pivot = new Vector2(0.5f, 0f);
            var group = ghost.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0.9f; group.blocksRaycasts = false; group.interactable = false;
            DrawItemView(ghost, item, 16);
            ghost.SetAsLastSibling();
            return ghost;
        }
        private static void PlaceGhost(RectTransform ghost, Vector2 position) => ghost.position = position + new Vector2(0, Screen.height * 0.025f);
        public void EndSlotDrag(GearSlot slot, Vector2 position)
        {
            if (slotGhost == null) return;
            Destroy(slotGhost.gameObject); slotGhost = null;
            if (RectTransformUtility.RectangleContainsScreenPoint(bag, position) || OverOpening(position)) TakeOffSlot(slot, position);
            else Say("Drop the slot inside the backpack to take it off.");
        }
        private void AutoSortBag()
        {
            if (saveBlocked || stamina.Fainted) return;
            bool ok = ledger.AutoSort(out string reason); Say(reason);
            if (ok) { Redraw(); SaveNow(); }
        }
        private void ClaimOverflow()
        {
            if (saveBlocked || stamina.Fainted || !inventoryOpen) return;
            ItemEntry item = null; foreach (ItemEntry candidate in ledger.Entries) if (candidate.Owner == ItemOwner.Overflow) { item = candidate; break; }
            if (item == null) { Say("Overflow is empty."); return; }
            const int token = -830; if (!ledger.Reserve(item, token)) return;
            bool ok = ledger.TryReturnToBackpack(item, token, out string reason); ledger.Release(item, token); Say(reason);
            if (ok) { Redraw(); SaveNow(); }
        }
        // Equipment menu (design v1, October 31, 2026; Docs/Art/UI/Equipment): fills the space above the open bag, covering
        // the top debug buttons and stats. Avatar stage in the centre (the player avatar arrives with the default
        // designs), Hat/Boots/Shirt on the left, Pants/Backpack on the right, each slot showing the worn item's model
        // with its durability badge, and the equipment debug actions in two rows below. Drag bag gear onto the panel to
        // equip; drag a slot down into the bag to take it off.
        private sealed class GearSlotView { public GearSlot Slot; public Image Frame; public RectTransform Content; public string Shown; }
        private readonly List<GearSlotView> gearSlots = new List<GearSlotView>();
        private Text gearHint;
        private EquipmentAvatarView avatarView;
        private bool avatarShown;
        private static readonly GearSlot[] LeftSlots = { GearSlot.Hat, GearSlot.Boots, GearSlot.Shirt }, RightSlots = { GearSlot.Pants, GearSlot.Backpack };
        private void CreateGearPanel()
        {
            gearPanel = Rect("Equipment", safe, 0.02f, 0.56f, 0.98f, 0.995f).gameObject;
            Image panel = gearPanel.AddComponent<Image>(); panel.sprite = DurabilityStyle.RoundedSprite; panel.type = Image.Type.Sliced;
            // Fully opaque: the project blends in linear space, so even 1.5% transparency let the bright HUD show through.
            panel.color = DurabilityStyle.Panel;
            Text title = Label(gearPanel.transform, "EQUIPMENT", 0.05f, 0.925f, 0.6f, 0.99f, 30);
            title.alignment = TextAnchor.MiddleLeft; title.fontStyle = FontStyle.Bold; title.color = DurabilityStyle.Text;
            Image rule = Rect("Title rule", gearPanel.transform, 0.40f, 0.955f, 0.84f, 0.958f).gameObject.AddComponent<Image>(); rule.color = DurabilityStyle.Border; rule.raycastTarget = false;
            gearHint = Label(gearPanel.transform, "", 0.04f, 0.885f, 0.96f, 0.925f, 16); gearHint.color = new Color(DurabilityStyle.Text.r, DurabilityStyle.Text.g, DurabilityStyle.Text.b, 0.8f);
            gearHint.resizeTextForBestFit = true; gearHint.resizeTextMinSize = 10; gearHint.resizeTextMaxSize = 16;

            // Avatar stage (placeholder until the default player avatars are designed).
            // Without the action rows (camp world) the stage reaches down into their space, so the avatar shows larger.
            RectTransform stage = Rect("Avatar stage", gearPanel.transform, 0.33f, milestone5 ? 0.03f : 0.22f, 0.67f, 0.875f);
            Image stageImage = stage.gameObject.AddComponent<Image>(); stageImage.sprite = DurabilityStyle.RoundedSprite; stageImage.type = Image.Type.Sliced;
            stageImage.color = Color.Lerp(DurabilityStyle.Panel, DurabilityStyle.Slot, 0.5f) * 1.12f; stageImage.color = new Color(stageImage.color.r, stageImage.color.g, stageImage.color.b, 1f); stageImage.raycastTarget = false;
            // The default player avatar, picked at random until the start-of-game selection exists.
            avatarView = EquipmentAvatarView.Create(stage, PlayerAppearance.Current());
            if (avatarView == null)
            {
                Text stageNote = Label(stage, "PLAYER AVATAR\narrives with the default designs", 0.05f, 0.4f, 0.95f, 0.6f, 15);
                stageNote.color = new Color(DurabilityStyle.Text.r, DurabilityStyle.Text.g, DurabilityStyle.Text.b, 0.35f);
            }

            // Left column three slots, right column two, top-aligned like the concept.
            const float top = 0.875f, slotHeight = 0.205f, gap = 0.02f;
            for (int i = 0; i < LeftSlots.Length; i++) gearSlots.Add(CreateGearSlot(LeftSlots[i], 0.03f, top - (i + 1) * slotHeight - i * gap, 0.31f, top - i * (slotHeight + gap)));
            for (int i = 0; i < RightSlots.Length; i++) gearSlots.Add(CreateGearSlot(RightSlots[i], 0.69f, top - (i + 1) * slotHeight - i * gap, 0.97f, top - i * (slotHeight + gap)));

            // No CLOSE or X: the bag button shows the return icon and closes the menu with the bag (user, November 5, 2026).
            string[] actions = { "R4 TEST GEAR", "EXPIRE SLOT", "EXPIRE ALL", "TAKE OFF" };
            UnityEngine.Events.UnityAction[] handlers = { GrantTestGear, ExpireSelectedSlot, () => ExpireTestGear(null), UnequipSelectedGear };
            // The camp world has no equipment action buttons (user, November 5, 2026): gear is equipped and taken off by
            // dragging between the bag and the slots, and expiry follows real wear. The older test scenes without crafting
            // keep their test gear, expiry and TAKE OFF buttons for their checks.
            if (!milestone5)
            {
                for (int i = 0; i < 3; i++) GearButton(gearPanel.transform, actions[i], 0.03f + i * 0.32f, 0.115f, 0.33f + i * 0.32f, 0.195f, handlers[i]);
                GearButton(gearPanel.transform, actions[3], 0.31f, 0.02f, 0.69f, 0.10f, handlers[3]);
            }
            gearPanel.SetActive(false);
        }
        private GearSlotView CreateGearSlot(GearSlot slot, float x0, float y0, float x1, float y1)
        {
            RectTransform root = Rect(slot + " slot", gearPanel.transform, x0, y0, x1, y1);
            Image fill = root.gameObject.AddComponent<Image>(); fill.sprite = DurabilityStyle.RoundedSprite; fill.type = Image.Type.Sliced; fill.color = DurabilityStyle.Slot;
            var button = root.gameObject.AddComponent<Button>(); button.targetGraphic = fill; button.onClick.AddListener(() => GearSlotAction(slot));
            var drag = root.gameObject.AddComponent<GearSlotDragSurface>(); drag.Owner = this; drag.Slot = slot;
            Image frame = Rect("Frame", root, 0, 0, 1, 1).gameObject.AddComponent<Image>(); frame.sprite = DurabilityStyle.FrameSprite; frame.type = Image.Type.Sliced; frame.raycastTarget = false;
            return new GearSlotView { Slot = slot, Frame = frame, Content = Rect("Content", root, 0, 0, 1, 1) };
        }
        // Secondary action: dark backing with the border colour, as in the debug-action frame.
        private static Text GearButton(Transform parent, string value, float x0, float y0, float x1, float y1, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = Rect(value, parent, x0, y0, x1, y1);
            Image image = rect.gameObject.AddComponent<Image>(); image.sprite = DurabilityStyle.RoundedSprite; image.type = Image.Type.Sliced; image.color = DurabilityStyle.Slot;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            Image edge = Rect("Frame", rect, 0, 0, 1, 1).gameObject.AddComponent<Image>(); edge.sprite = DurabilityStyle.FrameSprite; edge.type = Image.Type.Sliced; edge.color = DurabilityStyle.Border; edge.raycastTarget = false;
            Text text = Label(rect, value, 0.04f, 0, 0.96f, 1, 20); text.color = DurabilityStyle.Text;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 10; text.resizeTextMaxSize = 20;
            return text;
        }
        private ItemDefinition WornDefinition(GearSlot slot)
        {
            WornGear gear = equipment.Get(slot); if (gear == null) return null;
            foreach (ItemEntry entry in ledger.Entries) if (entry.Id == gear.InstanceId) return entry.Definition;
            return null;
        }
        // The avatar camera renders only while the equipment menu is showing.
        private void SyncAvatarVisibility()
        {
            bool show = gearPanel != null && gearPanel.activeInHierarchy;
            if (avatarView == null || show == avatarShown) return;
            avatarShown = show; avatarView.SetVisible(show);
        }
        // The panel spans from just above the open bag to the top of the safe area.
        private void LayoutGearPanel()
        {
            if (gearPanel == null || bagFrame == null) return;
            var rect = (RectTransform)gearPanel.transform;
            rect.anchorMin = new Vector2(0.02f, Mathf.Min(0.7f, bagFrame.anchorMax.y + 0.008f)); rect.anchorMax = new Vector2(0.98f, 0.995f);
        }
        private void RefreshGearPanel()
        {
            gearHint.text = feedback.text;
            avatarView?.Dress(WornDefinition(GearSlot.Hat), WornDefinition(GearSlot.Backpack),
                WornDefinition(GearSlot.Shirt), WornDefinition(GearSlot.Pants), WornDefinition(GearSlot.Boots));
            foreach (GearSlotView view in gearSlots)
            {
                bool selectedSlot = selectedGearSlot == view.Slot;
                view.Frame.color = selectedSlot ? DurabilityStyle.Selection : DurabilityStyle.Border;
                WornGear gear = equipment.Get(view.Slot);
                ItemEntry worn = null;
                if (gear != null) foreach (ItemEntry entry in ledger.Entries) if (entry.Id == gear.InstanceId) { worn = entry; break; }
                double maximum = worn != null ? worn.Profile.MaxDurability : 0;
                bool finite = gear != null && !gear.Infinite;
                string value = gear == null ? "DEFAULT" : gear.Infinite ? "INFINITE" : Distance(gear.RemainingDistance);
                string shown = (worn != null ? worn.Definition.Id : "") + "|" + value + "|" + (finite ? DurabilityStyle.Fill(gear.RemainingDistance, maximum) : 0f);
                if (shown == view.Shown) continue;
                view.Shown = shown;
                foreach (Transform child in view.Content) Destroy(child.gameObject);
                // The worn item's model, or a faint silhouette of the slot's tier-1 item over the default gear.
                ItemDefinition model = worn != null ? ItemArt.ModelFor(worn.Definition) : ItemArt.SlotSilhouette(view.Slot);
                Sprite icon = model != null ? ItemIconRenderer.For(model) : null;
                if (icon != null)
                {
                    Image picture = Rect("Model", view.Content, 0.08f, worn != null ? 0.22f : 0.18f, 0.92f, 0.86f).gameObject.AddComponent<Image>();
                    picture.sprite = icon; picture.preserveAspect = true; picture.raycastTarget = false;
                    if (worn == null) picture.color = new Color(DurabilityStyle.Text.r, DurabilityStyle.Text.g, DurabilityStyle.Text.b, 0.14f);
                }
                else Label(view.Content, worn != null ? worn.Definition.Name : view.Slot.ToString().ToUpperInvariant(), 0.05f, 0.35f, 0.95f, 0.85f, 18).color = DurabilityStyle.Text;
                Text name = Label(view.Content, view.Slot.ToString().ToUpperInvariant(), 0.07f, 0.86f, 0.93f, 0.98f, 13);
                name.alignment = TextAnchor.UpperLeft; name.color = new Color(DurabilityStyle.Text.r, DurabilityStyle.Text.g, DurabilityStyle.Text.b, 0.55f);
                RectTransform badge = DurabilityStyle.AddBadge(view.Content, value, finite ? gear.RemainingDistance : 0, finite ? maximum : 0, false);
                DurabilityStyle.Stretch(badge, 0.12f, 0.04f, 0.88f, 0.25f);
            }
        }
        // Builds the snapshot on the main thread; validation, hashing and the atomic write run on the background writer
        // (immediate: written before returning, for pause/focus loss/quit, when the process may be killed next).
        private void SaveNow() => SaveNow(false);
        private void SaveNow(bool immediate)
        {
            if (!initialized || saveBlocked || saveStore == null || ledger == null) return;
            var watch = System.Diagnostics.Stopwatch.StartNew(); double built = 0, compared = 0;
            try
            {
                var snapshot = new SessionSnapshot { version = milestone5 ? 3 : milestone4 ? 2 : 1, distance = movement.Distance, requestedSpeed = movement.RequestedSpeed,
                    speedCap = movement.SpeedCap, inventoryResume = movement.InventoryResumeSpeed, inventoryOpen = SavedBagOpen(WarehouseOpen || PickupOpen, WarehouseOpen ? warehouseBagWasOpen : pickupBagWasOpen, inventoryOpen),
                    postHarvest = movement.PostHarvestStop, camp = (movement.Pauses & MovementPauseReasons.Camp) != 0, yaw = orbit.Yaw };
                if (milestone4)
                {
                    snapshot.survival = new SavedSurvival { hearts = stamina.Hearts, maximum = stamina.Maximum, pendingFood = stamina.PendingFood,
                        fainted = stamina.Fainted, faintElapsed = stamina.FaintElapsed, faintEventId = faintEventId, loss = new List<SavedLoss>(faintLoss) };
                    foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot)))
                    {
                        WornGear gear = equipment.Get(slot); if (gear == null) continue;
                        snapshot.survival.gear.Add(new SavedGear { id = gear.InstanceId, slot = gear.Slot.ToString(), rank = gear.Rank,
                            quality = gear.Quality, remainingDistance = gear.RemainingDistance, infinite = gear.Infinite, statOverride = gear.StatOverride ?? 0 });
                    }
                }
                if (milestone5) { snapshot.economy = 1; snapshot.trade = tradeSession?.Data; snapshot.tradeLog = new List<TheTrailWalker.Domain.Economy.TradeRecord>(tradeLog); snapshot.warehouse = SessionSaveStore.SaveWarehouse(warehouse); snapshot.lumber = SessionSaveStore.SaveLumber(lumber); snapshot.shops = SessionSaveStore.SaveShops(shops); snapshot.balloon = SessionSaveStore.SaveBalloon(balloon); snapshot.pets = SessionSaveStore.SavePets(pets); snapshot.warehouseLog = new List<TheTrailWalker.Domain.Economy.TradeRecord>(warehouseLog); snapshot.segmentId = travel.CurrentSegment.Id; snapshot.travel = SessionSaveStore.SaveTravel(travel); snapshot.settlement = SaveSettlementForSnapshot(); snapshot.home = SaveHomeForSnapshot(); snapshot.harvested = HarvestedForSave(); snapshot.crafting = SaveCrafting(); snapshot.missions = SaveMissions(); snapshot.statistics = SaveStatistics(); }
                AppendSavedItems(snapshot, ledger, milestone5, travel.CurrentSegment.Id,
                    item => { Bundle bundle = bundles.Find(b => b.Item == item); return bundle == null ? Vector3.zero : bundle.View.transform.position; },
                    item => item.Owner == ItemOwner.Worn && item.Definition.GearSlot.HasValue ? equipment?.Get(item.Definition.GearSlot.Value) : null);
                built = watch.Elapsed.TotalMilliseconds;
                string state = JsonUtility.ToJson(snapshot);
                compared = watch.Elapsed.TotalMilliseconds;
                if (state == lastSavedState) { if (immediate) SessionSaveStore.FlushBackgroundSaves(); LogSaveTiming(watch, built, compared, false); return; }
                snapshot.transactionId = Guid.NewGuid().ToString("N");
                if (immediate) saveStore.Save(snapshot, path.Length);
                else saveStore.SaveInBackground(snapshot, path.Length);
                lastSavedState = state;
                LogSaveTiming(watch, built, compared, true);
            }
            catch (Exception error) { SaveFailed(error); }
            if (immediate) CheckBackgroundSave();
        }
        private void CheckBackgroundSave()
        {
            Exception failure = SessionSaveStore.TakeBackgroundFailure();
            if (failure != null && !saveBlocked) SaveFailed(failure);
        }
        private void SaveFailed(Exception error)
        {
            saveBlocked = true; movement.SetPause(MovementPauseReasons.Exhausted, true);
            Say("Save failed. Progress is paused; do not close the app. " + error.Message);
            Debug.LogException(error);
        }
        // The item list of a save: lumber piles and consumed entries are left out (see SessionSaveStore.Persistable).
        public static void AppendSavedItems(SessionSnapshot snapshot, SpatialInventory ledger, bool milestone5, string segmentId, Func<ItemEntry, Vector3> positionOf, Func<ItemEntry, WornGear> wornOf)
        {
            foreach (ItemEntry item in SessionSaveStore.Persistable(ledger.Entries))
            {
                if (milestone5 && item.Owner == ItemOwner.World && item.SegmentId == null) ledger.AssignSegment(item, segmentId);
                Vector3 position = positionOf(item);
                WornGear worn = wornOf(item);
                snapshot.items.Add(new SavedItem { id = item.Id, definitionId = item.Definition.Id, owner = item.Owner.ToString(), quantity = item.Quantity,
                    quality = item.Quality, ready = item.Ready, x = item.X, y = item.Y, unitValue = item.Profile.UnitValue,
                    maxDurability = item.Profile.MaxDurability, durability = item.Profile.Durability, variant = item.Profile.Variant,
                    hasWear = worn != null ? !worn.Infinite : item.HasWearRemaining, wearRemaining = worn != null ? worn.RemainingDistance : item.WearRemaining,
                    dropped = item.Owner == ItemOwner.World && item.Dropped, trailDistance = item.TrailDistance, worldX = position.x, worldY = position.y, worldZ = position.z, pile = SavedPile(item),
                    segmentId = milestone5 && item.Owner == ItemOwner.World ? item.SegmentId : null });
            }
        }

        public static void PrepareRestore(SessionSnapshot snapshot, bool milestone5)
        {
            SessionSaveStore.DropLumberPiles(snapshot);   // never trust a saved lumber pile
            if (milestone5) SessionSaveStore.MigrateEconomy(snapshot);   // Milestone 9: item values in minor units
        }

        private void RestoreSnapshot(SessionSnapshot snapshot)
        {
            PrepareRestore(snapshot, milestone5);
            SessionSaveStore.Validate(snapshot, path.Length, out ledger);
            if (milestone5) { RestoreTrade(snapshot); travel = SessionSaveStore.RestoreTravel(snapshot, scene.Catalog); RestoreHarvested(snapshot.harvested); RestoreCrafting(snapshot.crafting); RestoreMissions(snapshot.missions); RestoreStatistics(snapshot.statistics); RestoreSettlementFromSnapshot(snapshot); warehouse = SessionSaveStore.RestoreWarehouse(snapshot, ledger.Entries); warehouseLog = snapshot.warehouseLog ?? new List<TheTrailWalker.Domain.Economy.TradeRecord>(); lumber = SessionSaveStore.RestoreLumber(snapshot); shops = SessionSaveStore.RestoreShops(snapshot); balloon = SessionSaveStore.RestoreBalloon(snapshot); pets = SessionSaveStore.RestorePets(snapshot); petSettlePending = true; }
            if (milestone4)
            {
                equipment = new EquipmentState();
                foreach (SavedGear gear in snapshot.survival.gear) equipment.Equip(new WornGear(gear.id, (GearSlot)Enum.Parse(typeof(GearSlot), gear.slot), gear.rank, gear.quality, gear.remainingDistance, gear.infinite, gear.statOverride != 0 ? gear.statOverride : (double?)null));
                stamina.Restore(snapshot.survival.hearts, snapshot.survival.maximum, snapshot.survival.pendingFood, snapshot.survival.fainted, snapshot.survival.faintElapsed);
                router.InteractionLocked = stamina.Fainted;
                faintEventId = snapshot.survival.faintEventId; faintLoss.Clear(); faintLoss.AddRange(snapshot.survival.loss);
            }
            movement.Restore(snapshot.distance, snapshot.requestedSpeed, snapshot.speedCap, snapshot.inventoryResume, snapshot.inventoryOpen, snapshot.postHarvest, snapshot.camp);
            // Re-derive the cap from worn boots so rule changes (such as a new maximum) apply to existing saves.
            if (milestone4) movement.SetSpeedCap((float)equipment.SpeedCap);
            if (milestone4 && (stamina.Fainted || stamina.IsEmpty)) movement.SetPause(MovementPauseReasons.Exhausted, true);
            inventoryOpen = snapshot.inventoryOpen;
            LayoutBackpack();
            orbit.Restore(snapshot.yaw); motor.Present();
            foreach (SavedItem saved in snapshot.items)
            {
                if (saved.owner != "World") continue;
                ItemEntry item = null; foreach (ItemEntry candidate in ledger.Entries) if (candidate.Id == saved.id) { item = candidate; break; }
                if (milestone5)
                {
                    var position = new Vector3(saved.worldX, saved.worldY, saved.worldZ);
                    // Harvest rewards not yet shown (saved at zero) stay in the queue; SpawnPendingHarvest places them.
                    bool queued = (saved.id.StartsWith("harvest-", StringComparison.Ordinal) && position == Vector3.zero) || saved.id.StartsWith("fixture-", StringComparison.Ordinal);
                    if (item.SegmentId == scene.Segment.Id && !queued) SpawnSegmentBundle(item, position);
                    continue;
                }
                AddBundle(item, new Vector3(saved.worldX, saved.worldY, saved.worldZ));
            }
            Redraw();
        }
        // Development builds: per-save stage timings, flushed to save-timing.txt every 10 saves (read over adb).
        private readonly System.Text.StringBuilder saveTimings = new System.Text.StringBuilder();
        private int timedSaves;
        private void LogSaveTiming(System.Diagnostics.Stopwatch watch, double built, double compared, bool written)
        {
            if (!Debug.isDebugBuild) return;
            double total = watch.Elapsed.TotalMilliseconds; double[] store = SessionSaveStore.LastStageMs;
            saveTimings.Append($"{Time.realtimeSinceStartup:F1}s main thread {total:F2} | build {built:F2} json {compared - built:F2}");
            if (written) saveTimings.Append($" | queued; previous write (background): validate {store[0]:F2} json {store[1]:F2} hash {store[2]:F2} write {store[3]:F2} replace {store[4]:F2}");
            else saveTimings.Append(" | unchanged");
            saveTimings.Append($" | items {ledger.Entries.Count}").Append('\n');
            if (++timedSaves % 10 != 0) return;
            try { File.AppendAllText(Path.Combine(Application.persistentDataPath, "save-timing.txt"), saveTimings.ToString()); } catch (IOException) { }
            saveTimings.Clear();
        }
        private static List<SavedPortion> SavedPile(ItemEntry item)
        {
            var portions = new List<SavedPortion>(item.Pile.Count);
            foreach (PilePortion portion in item.Pile)
                portions.Add(new SavedPortion { quality = portion.Quality, quantity = portion.Quantity, unitValue = portion.Profile.UnitValue,
                    maxDurability = portion.Profile.MaxDurability, durability = portion.Profile.Durability, variant = portion.Profile.Variant });
            return portions;
        }
        private void WriteReport(string reason)
        {
            if (!Debug.isDebugBuild) return;
            var data = new Report { reason = reason, distance = movement.Distance, speed = movement.ActualSpeed, yaw = orbit.Yaw, accepted = accepted, rejected = rejected, open = inventoryOpen, entries = new List<EntryReport>() };
            foreach (ItemEntry item in ledger.Entries) data.entries.Add(new EntryReport { id = item.Id, definition = item.Definition.Id, owner = item.Owner.ToString(), quality = item.Quality, quantity = item.Quantity, x = item.X, y = item.Y, width = item.Quantity > 0 ? item.Width : 0, height = item.Quantity > 0 ? item.Height : 0, value = item.Profile.UnitValue });
            try { File.WriteAllText(Path.Combine(Application.persistentDataPath, "milestone2.json"), JsonUtility.ToJson(data, true)); }
            catch (IOException error) { Debug.LogWarning(error.Message); }
        }
        [Serializable] private sealed class Report { public string reason; public float distance, speed, yaw; public int accepted, rejected; public bool open; public List<EntryReport> entries; }
        [Serializable] private sealed class EntryReport { public string id, definition, owner; public int quality, quantity, x, y, width, height; public long value; }
    }
    public sealed class BagPointerSurface : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IPointerUpHandler
    {
        public CollectionPrototype Owner;
        public void OnPointerDown(PointerEventData data) => Owner.BeginBag(data.pointerId, data.position);
        public void OnBeginDrag(PointerEventData data) { }
        public void OnDrag(PointerEventData data) => Owner.Move(data.pointerId, data.position);
        public void OnPointerUp(PointerEventData data) => Owner.End(data.pointerId, data.position, false);
    }
    // Lets a worn gear slot be dragged down into the backpack to take it off; a plain tap still reaches the Button.
    public sealed class GearSlotDragSurface : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public CollectionPrototype Owner;
        public GearSlot Slot;
        public void OnBeginDrag(PointerEventData data) => Owner.BeginSlotDrag(Slot, data.position);
        public void OnDrag(PointerEventData data) => Owner.DragSlot(data.position);
        public void OnEndDrag(PointerEventData data) => Owner.EndSlotDrag(Slot, data.position);
    }
}
