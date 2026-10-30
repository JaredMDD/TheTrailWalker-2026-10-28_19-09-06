using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Equipment;
using TheTrailWalker.Input;
using TheTrailWalker.Movement;
using TheTrailWalker.Persistence;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheTrailWalker
{
    [DefaultExecutionOrder(50)]
    public sealed class CollectionPrototype : MonoBehaviour, IResourceGestures
    {
        private sealed class Bundle
        {
            public ItemEntry Item; public GameObject View; public Collider Collider; public Text Badge;
        }
        private sealed class Contact
        {
            public ItemEntry Item; public Bundle Bundle; public Vector2 Start, Position, Offset;
            public float Started; public bool World, Side, Dragging; public Text Ghost;
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
        private Text status, feedback, inventoryLabel;
        private readonly List<Bundle> bundles = new List<Bundle>();
        private readonly Dictionary<int, Contact> contacts = new Dictionary<int, Contact>();
        private readonly RaycastHit[] occlusionHits = new RaycastHit[32];
        private float nextHud, nextReport;
        private int revision = -1, accepted, rejected;
        private string fixtureCycle;
        private bool inventoryOpen;
        private SessionSaveStore saveStore;
        private bool saveBlocked, resetPending, initialized;
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
        private RectTransform speedGauge, speedFill;
        private Text speedValue;
        // Thermometer sits just above the closed bag button; its full height represents the maximum boots cap.
        private const float ThermometerBase = 0.095f, ThermometerFull = 0.30f, MaximumSpeedCap = MovementState.MaximumSpeedCap;
        // Authored drain for the test leg: region-1 opening rate of 12 hearts/km, raised from 8 after phone testing found it too slow.
        private const double LegDrainPerMetre = 0.012;
        private Text slotGhost;
        private Text detailsText;
        private Text gearSummary;
        private readonly List<Text> gearLabels = new List<Text>();
        private Rect lastSafe;
        private Vector2Int lastSize;

        public void Bind(MovementState state, TrailMotor trailMotor, ITrailPath trail, CameraOrbitController cameraOrbit, GestureRouter gestures, Material sharedMaterial, bool enableMilestone4 = false)
        {
            movement = state; motor = trailMotor; path = trail; orbit = cameraOrbit; router = gestures; material = sharedMaterial;
            milestone4 = enableMilestone4;
            if (milestone4) { equipment = new EquipmentState(); stamina = new StaminaState(); motor.LimitMovementSeconds = LimitMovementSeconds; }
            cameraView = cameraOrbit.GetComponent<Camera>(); router.Resources = this; router.SpeedBlocked += ExplainSpeedBlocked;
            CreateHud();
            string directory = Path.Combine(Application.persistentDataPath, "PrototypeSession");
            saveStore = new SessionSaveStore(directory, new SystemClock(), milestone4 ? 2 : 1);
            LoadStatus result = saveStore.Load(path.Length, out SessionSnapshot snapshot, out string message);
            if (milestone4 && result == LoadStatus.Fresh)
            {
                var oldStore = new SessionSaveStore(directory, new SystemClock());
                LoadStatus oldResult = oldStore.Load(path.Length, out SessionSnapshot oldSnapshot, out string oldMessage);
                if (oldResult == LoadStatus.Loaded || oldResult == LoadStatus.Recovered)
                { snapshot = SessionSaveStore.MigrateV1(oldSnapshot, path.Length); result = oldResult; message = oldResult == LoadStatus.Recovered ? oldMessage : "Migrated a copy of the Milestone 3 profile."; }
                else if (oldResult != LoadStatus.Fresh) { result = oldResult; message = oldMessage; }
            }
            if (result == LoadStatus.Fresh) { ResetFixture(0); initialized = true; SaveNow(); }
            else if (result == LoadStatus.Loaded || result == LoadStatus.Recovered)
            {
                RestoreSnapshot(snapshot); initialized = true;
                Say(string.IsNullOrEmpty(message) ? "Session restored." : message);
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
            speedGauge = Rect("Speed thermometer", safe, 0.05f, ThermometerBase, 0.09f, ThermometerBase + ThermometerFull);
            Image tube = speedGauge.gameObject.AddComponent<Image>(); tube.color = new Color(0.05f, 0.08f, 0.09f, 0.75f); tube.raycastTarget = false;
            speedFill = Rect("Speed fill", speedGauge, 0.15f, 0.01f, 0.85f, 0.5f);
            Image fill = speedFill.gameObject.AddComponent<Image>(); fill.color = new Color(0.93f, 0.45f, 0.2f); fill.raycastTarget = false;
            speedValue = Label(safe, "", 0.01f, 0, 0.30f, 0, 20); speedValue.alignment = TextAnchor.LowerLeft;
            speedGauge.gameObject.SetActive(false); speedValue.gameObject.SetActive(false);
            Label(safe, "THE TRAIL WALKER / COLLECTION", 0.02f, 0.94f, 0.98f, 0.99f, 27);
            status = Label(safe, "", 0.02f, 0.865f, 0.98f, 0.94f, 23);
            Button(safe, "EMPTY", 0.02f, 0.81f, 0.24f, 0.86f, () => ResetFixture(0));
            Button(safe, "MIXED", 0.26f, 0.81f, 0.48f, 0.86f, () => ResetFixture(1));
            Button(safe, "FULL BAG", 0.5f, 0.81f, 0.74f, 0.86f, () => ResetFixture(2));
            Button(safe, "RESET SAVE", 0.76f, 0.81f, 0.98f, 0.86f, ConfirmReset);
            feedback = Label(safe, "Swipe a pickup DOWN quickly, or hold then drag into the bag.", 0.02f, 0.735f, 0.98f, 0.81f, 23);
            Button(safe, "TEST HARVEST", 0.20f, 0.67f, 0.45f, 0.72f, () =>
            {
                if (saveBlocked || inventoryOpen || (milestone4 && stamina.Fainted)) return;
                router.CancelAll(); movement.BeginHarvest(); movement.EndHarvest();
                Say("Harvest ended. Swipe UP on the centre line to restart."); SaveNow();
            });
            Button(safe, "TRAIL START", 0.47f, 0.67f, 0.72f, 0.72f, ReturnToTrailStart);
            if (milestone4)
            {
                Button(safe, "GEAR / SLOTS", 0.20f, 0.61f, 0.45f, 0.66f, ToggleGearPanel);
                Button(safe, "EAT SELECTED", 0.47f, 0.61f, 0.72f, 0.66f, EatSelected);
                Button(safe, "TEST EMPTY", 0.74f, 0.61f, 0.98f, 0.66f, DebugEmptyStamina);
                Button(safe, "ADD SPARE GEAR", 0.47f, 0.55f, 0.72f, 0.60f, GrantSpareGear);
                Button(safe, "CLAIM OVERFLOW", 0.74f, 0.55f, 0.98f, 0.60f, ClaimOverflow);
                CreateGearPanel();
            }
            inventoryLabel = Button(safe, "BAG", 0.02f, 0.04f, 0.22f, 0.085f, ToggleInventory);
            inventoryButton = (RectTransform)inventoryLabel.transform.parent;
            // Discard pocket opposite CLOSE: dragging an item here drops it back onto the trail.
            dropPocket = Rect("Drop pocket", safe, 0.78f, 0.04f, 0.98f, 0.085f);
            Image dropImage = dropPocket.gameObject.AddComponent<Image>(); dropImage.color = new Color(0.42f, 0.2f, 0.16f); dropImage.raycastTarget = false;
            Label(dropPocket, "DROP", 0, 0, 1, 1, 22);
            // The backpack opening doubles as the quick-storage aperture; the full bag shows only while open.
            aperture = Rect("Backpack opening", safe, 0, 0, 1, 0.035f);
            aperture.gameObject.AddComponent<Image>().color = new Color(0.36f, 0.25f, 0.15f);
            // Full-width viewport at the bottom edge; its content scrolls sideways: hooks | bag | pockets.
            bagFrame = Rect("Bag viewport", safe, 0, 0, 1, 0.3f);
            bagFrame.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.08f, 0.09f, 0.97f);
            bagFrame.gameObject.AddComponent<RectMask2D>();
            bagFrame.gameObject.AddComponent<BagPointerSurface>().Owner = this;
            bagContent = Pinned("Bag content", bagFrame);
            hookPanel = Pinned("Hooks", bagContent);
            bag = Pinned("Backpack", bagContent);
            Image bagImage = bag.gameObject.AddComponent<Image>(); bagImage.color = new Color(0.06f, 0.12f, 0.14f, 0.96f); bagImage.raycastTarget = false;
            pocketPanel = Pinned("Pockets", bagContent);
            itemsLayer = Rect("Item views", bag, 0, 0, 1, 1);
            if (milestone4)
            {
                detailsPanel = Rect("Item details", safe, 0.10f, 0.40f, 0.90f, 0.60f).gameObject;
                detailsPanel.AddComponent<Image>().color = new Color(0.04f, 0.10f, 0.11f, 0.99f);
                detailsText = Label(detailsPanel.transform, "", 0.03f, 0.37f, 0.97f, 0.96f, 23);
                detailEat = Button(detailsPanel.transform, "EAT", 0.05f, 0.05f, 0.47f, 0.32f, EatSelected).transform.parent.gameObject;
                // Food and wearables never overlap, so EQUIP shares EAT's position.
                detailEquip = Button(detailsPanel.transform, "EQUIP", 0.05f, 0.05f, 0.47f, 0.32f, () => EquipItem(selected)).transform.parent.gameObject;
                Button(detailsPanel.transform, "CLOSE", 0.53f, 0.05f, 0.95f, 0.32f, () => detailsPanel.SetActive(false));
                detailsPanel.SetActive(false);
                faintOverlay = Rect("Faint recovery", safe, 0, 0, 1, 1).gameObject;
                faintOverlay.AddComponent<Image>().color = new Color(0.02f, 0.03f, 0.04f, 0.92f);
                faintMessage = Label(faintOverlay.transform, "", 0.08f, 0.3f, 0.92f, 0.7f, 32);
                faintOverlay.SetActive(false);
            }
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
            bagFrame.gameObject.SetActive(inventoryOpen);
            if (inventoryOpen) LayoutBag(true);
            float top = inventoryOpen ? bagFrame.anchorMax.y : 0;
            aperture.anchorMin = new Vector2(0, top); aperture.anchorMax = new Vector2(1, top + 0.035f);
            inventoryButton.anchorMin = new Vector2(0.02f, top + 0.04f); inventoryButton.anchorMax = new Vector2(0.22f, top + 0.085f);
            dropPocket.anchorMin = new Vector2(0.78f, top + 0.04f); dropPocket.anchorMax = new Vector2(0.98f, top + 0.085f);
            dropPocket.gameObject.SetActive(inventoryOpen);
            inventoryLabel.text = inventoryOpen ? "CLOSE" : "BAG";
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
            // Cells fill the width in portrait but shrink on short screens so the bag never exceeds 45% of the height.
            float cell = Mathf.Min(safe.rect.width / VisibleColumns, safe.rect.height * 0.45f * 1000f / ledger.Height), height = cell * ledger.Height / 1000f;
            if (cell <= 0) return;
            bagFrame.anchorMax = new Vector2(1, height / safe.rect.height);
            float hooks = ledger.HookCount > 0 ? cell * (ledger.HookCount * (HookSlot + SideGap) + SideGap) : 0;
            float pockets = ledger.PocketCount > 0 ? cell * ((ledger.PocketCount + 1) / 2 * (PocketSlot + SideGap) + SideGap) : 0;
            float width = cell * ledger.Width / 1000f;
            Place(hookPanel, 0, hooks, height); Place(bag, hooks, width, height); Place(pocketPanel, hooks + width, pockets, height);
            contentWidth = hooks + width + pockets; bagContent.sizeDelta = new Vector2(contentWidth, height);
            laidOutWidth = safe.rect.width;
            RebuildSidePanels(cell, height);
            SetScroll(resetScroll ? -hooks : bagScroll);
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
                // Placeholder belt; final art should read as a real tool belt with hooks holding each tool.
                RectTransform belt = Pinned("Tool belt", hookPanel); Place(belt, 0, panelWidth, cell * 0.12f);
                belt.anchoredPosition = new Vector2(0, height - cell * 0.12f);
                belt.gameObject.AddComponent<Image>().color = new Color(0.36f, 0.25f, 0.15f);
            }
            for (int i = 0; i < ledger.HookCount; i++)
            {
                float x = panelWidth - (i + 1) * (HookSlot + SideGap) * cell;
                RectTransform hook = Pinned("Hook nub", hookPanel); Place(hook, x + cell * (HookSlot * 0.5f - 0.08f), cell * 0.16f, cell * 0.3f);
                hook.anchoredPosition = new Vector2(hook.anchoredPosition.x, height - cell * 0.4f);
                hook.gameObject.AddComponent<Image>().color = new Color(0.72f, 0.72f, 0.68f);
                hookCells.Add(SideCell(hookPanel, ItemOwner.Hook, i, "H", x, cell * 0.15f, cell * HookSlot, height - cell * 0.6f));
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
            Image image = cellRect.gameObject.AddComponent<Image>(); image.raycastTarget = false;
            image.color = occupant == null ? new Color(0.1f, 0.2f, 0.2f, 0.8f) : ColorFor(occupant.Definition);
            Text label = Label(cellRect, prefix + (index + 1) + (occupant == null ? "" : $"\n{occupant.Definition.Name} x{occupant.Quantity}"), 0.03f, 0.03f, 0.97f, 0.97f, 16);
            label.resizeTextForBestFit = true; label.resizeTextMinSize = 9; label.resizeTextMaxSize = 16;
            return cellRect;
        }
        private bool SideCellAt(Vector2 screen, out bool hook, out int index)
        {
            for (index = 0; index < hookCells.Count; index++) if (RectTransformUtility.RectangleContainsScreenPoint(hookCells[index], screen)) { hook = true; return true; }
            for (index = 0; index < pocketCells.Count; index++) if (RectTransformUtility.RectangleContainsScreenPoint(pocketCells[index], screen)) { hook = false; return true; }
            hook = false; index = -1; return false;
        }
        private void ToggleInventory()
        {
            if (saveBlocked || (milestone4 && stamina.Fainted)) return;
            router.CancelAll(); inventoryOpen = !inventoryOpen;
            if (!inventoryOpen && gearPanel != null) { gearPanel.SetActive(false); detailsPanel.SetActive(false); }
            if (inventoryOpen) movement.OpenInventory(); else movement.CloseInventory();
            LayoutBackpack();
            Say(inventoryOpen ? "Hold a bag item, then drag to move or touch a compatible stack." : "Walking intent restored. World pickups remain in range only.");
            SaveNow();
        }
        private void ConfirmReset()
        {
            if (!resetPending || Time.unscaledTime > resetDeadline) { resetPending = true; resetDeadline = Time.unscaledTime + 5f; Say("Tap RESET SAVE again within five seconds to archive this profile and start over."); return; }
            resetPending = false;
            try
            {
                saveStore.ArchiveForReset(); saveBlocked = false; lastSavedState = null;
                if (inventoryOpen) { movement.CloseInventory(); inventoryOpen = false; LayoutBackpack(); }
                router.CancelAll(); movement.Reset();
                if (milestone4) { equipment = new EquipmentState(); stamina = new StaminaState(); router.InteractionLocked = false; movement.SetSpeedCap(0.8f); faintEventId = null; faintLoss.Clear(); if (gearPanel != null) gearPanel.SetActive(false); }
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
                for (int y = 0; y < 4; y++) for (int x = 0; x < 6; x++) AddBag("full-" + x + "-" + y, PrototypeItems.Stone, 1, 5, x * 1000, y * 1000);
            // Debug fixture, not a regional spawn table: logs/tools are explicit test grants.
            for (int i = 0; i < 18; i++)
            {
                ItemDefinition definition = PrototypeItems.All[i % 6];
                float distance = Mathf.Clamp(movement.Distance + 2 + (i / 2) * 6f, 0, path.Length);
                var item = ledger.AddWorld("fixture-" + fixtureCycle + "-" + i, definition, definition.Stackable ? 1000 : 1, i % 5 + 1,
                    definition.Stackable ? new ItemProfile(100) : new ItemProfile(100, 30, 30), distance: distance);
                path.Evaluate(distance, out Vector3 p, out Vector3 t); Vector3 side = Vector3.Cross(Vector3.up, t);
                AddBundle(item, p + side * (i % 2 == 0 ? -1.9f : 1.9f) + Vector3.up * 0.45f);
            }
            Say(preset == 0 ? "Empty bag. Every stackable trail bundle contains 1,000." : preset == 1 ? "Mixed test: 1 / 999 / 1,000, Q1-Q5, different values and one unique axe." : "Bag full. Failed pickups must remain on the trail.");
            Redraw(); WriteReport("fixture");
            if (initialized) SaveNow();
        }
        // The item lands about 2.5 m in front of the current look direction, so it is visible and can be picked up again.
        // It stays until collected; removal at the next camp-return drop cycle arrives with segment cycles (Milestones 5-6).
        private void DropToTrail(int pointer, Contact contact)
        {
            ItemEntry item = contact.Item;
            bool dropped = ledger.TryDropToTrail(item, pointer, movement.Distance, out string reason);
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
            var badge = Label(safe, "", 0, 0, 0, 0, 19); badge.rectTransform.sizeDelta = new Vector2(170, 55);
            bundles.Add(new Bundle { Item = item, View = model, Collider = model.GetComponent<Collider>(), Badge = badge });
        }
        private void AddBag(string id, ItemDefinition definition, int quantity, int quality, int x, int y, ItemProfile? profile = null)
        {
            ItemEntry item = ledger.AddWorld(fixtureCycle + "-" + id, definition, quantity, quality, profile);
            ledger.Reserve(item, int.MinValue);
            if (!ledger.TryPlace(item, int.MinValue, x, y, false, out string reason)) throw new InvalidOperationException("Invalid debug fixture: " + reason);
            ledger.Release(item, int.MinValue);
        }
        private bool Reachable(Bundle bundle, out string reason)
        {
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
            if (saveBlocked || (milestone4 && stamina.Fainted)) return false;
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
            if (SideCellAt(position, out bool hook, out int index))
            {
                ItemOwner owner = hook ? ItemOwner.Hook : ItemOwner.Pocket;
                foreach (ItemEntry item in ledger.Entries)
                {
                    if (item.Owner != owner || item.X != index) continue;
                    if (!ledger.Reserve(item, pointer)) return;
                    selected = item;
                    contacts[pointer] = new Contact { Item = item, Side = true, Start = position, Position = position, Started = Time.unscaledTime };
                    Say($"{item.Definition.Name} / Q{item.Quality} / {item.Quantity}"); return;
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
                    Say($"{item.Definition.Name} / Q{item.Quality} / {item.Quantity} / {(item.Ready ? "Ready" : "Pending")} / Value {item.Profile.UnitValue}" + (item.Definition.Stackable ? "" : $" / Durability {item.Profile.Durability}/{item.Profile.MaxDurability}")); return;
                }
            }
            // Touching the container rather than an item scrolls it sideways.
            scrollPointer = pointer; scrollStartX = position.x; scrollStartOffset = bagScroll;
        }
        public void Move(int pointer, Vector2 position)
        {
            if (scrollPointer == pointer)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(safe, position, null, out Vector2 now);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(safe, new Vector2(scrollStartX, position.y), null, out Vector2 start);
                SetScroll(scrollStartOffset + now.x - start.x); return;
            }
            if (!contacts.TryGetValue(pointer, out Contact contact)) return;
            // Holding a dragged item near either screen edge scrolls toward the hooks or pockets.
            if (contact.Dragging && !contact.World && inventoryOpen)
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
                if (contact.Ghost == null) contact.Ghost = Label(safe, contact.Item.Definition.Name + " x" + contact.Item.Quantity, 0, 0, 0, 0, 24);
                contact.Ghost.rectTransform.sizeDelta = new Vector2(260, 70); contact.Ghost.rectTransform.position = position;
                contact.Ghost.color = new Color(1, 0.9f, 0.3f);
            }
        }
        public void End(int pointer, Vector2 position, bool canceled)
        {
            if (scrollPointer == pointer) { if (!canceled) Move(pointer, position); scrollPointer = null; return; }
            if (!contacts.TryGetValue(pointer, out Contact contact)) return;
            if (!canceled) Move(pointer, position);
            if (!contacts.ContainsKey(pointer)) return; // Quick storage already committed.
            if (!canceled && contact.Dragging && !contact.World && inventoryOpen && RectTransformUtility.RectangleContainsScreenPoint(dropPocket, position))
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
            if (!canceled && contact.Dragging) Commit(pointer, contact, RectTransformUtility.RectangleContainsScreenPoint(aperture, position), position);
            else
            {
                if (!canceled && milestone4 && !contact.World && Time.unscaledTime - contact.Started >= 0.25f) ShowDetails(contact.Item);
                Release(pointer, contact);
            }
        }
        private void Commit(int pointer, Contact contact, bool quick, Vector2 position)
        {
            bool success = false; string reason = "Drop inside the backpack; source unchanged";
            if (contact.World && !Reachable(contact.Bundle, out reason)) { }
            else if (quick && contact.World) success = ledger.TryQuickStore(contact.Item, pointer, out reason);
            else if (quick && contact.Side) success = ledger.TryReturnToBackpack(contact.Item, pointer, out reason);
            else if (inventoryOpen && RectTransformUtility.RectangleContainsScreenPoint(bag, position))
            {
                Vector2 point = BagPoint(position) - (contact.World || contact.Side ? new Vector2(contact.Item.Width, contact.Item.Height) * 0.5f : contact.Offset);
                success = ledger.TryPlace(contact.Item, pointer, Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), true, out reason);
            }
            if (success) accepted++; else rejected++;
            Release(pointer, contact); Say(reason); Redraw(); if (success) SaveNow(); WriteReport(success ? "transfer" : "rejected");
        }
        private Vector2 BagPoint(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bag, screen, null, out Vector2 local);
            return new Vector2((local.x - bag.rect.xMin) / bag.rect.width * ledger.Width, (local.y - bag.rect.yMin) / bag.rect.height * ledger.Height);
        }
        private void Release(int pointer, Contact contact)
        {
            ledger.Release(contact.Item, pointer); if (contact.Ghost != null) Destroy(contact.Ghost.gameObject); contacts.Remove(pointer);
        }
        public void CancelAll()
        {
            foreach (var pair in contacts) { ledger.Release(pair.Value.Item, pair.Key); if (pair.Value.Ghost != null) Destroy(pair.Value.Ghost.gameObject); }
            contacts.Clear(); scrollPointer = null;
        }
        private void Redraw()
        {
            foreach (Transform child in itemsLayer) Destroy(child.gameObject);
            foreach (ItemEntry item in ledger.Entries)
            {
                if (item.Owner != ItemOwner.Backpack) continue;
                RectTransform rect = Rect(item.Id, itemsLayer, (float)item.X / ledger.Width, (float)item.Y / ledger.Height, (float)(item.X + item.Width) / ledger.Width, (float)(item.Y + item.Height) / ledger.Height);
                Image image = rect.gameObject.AddComponent<Image>(); image.color = ColorFor(item.Definition); image.raycastTarget = false;
                Text label = Label(rect, $"{item.Definition.Name}\nQ{item.Quality} x{item.Quantity}", 0.02f, 0.02f, 0.98f, 0.98f, 18);
                label.resizeTextForBestFit = true; label.resizeTextMinSize = 10; label.resizeTextMaxSize = 19;
            }
            if (inventoryOpen) LayoutBag(false);
            revision = ledger.Revision;
        }
        private void Say(string text) { feedback.text = text; }
        // Tube length follows the current boots cap; the fill shows requested speed within it.
        private void UpdateSpeedGauge()
        {
            bool show = !inventoryOpen && (router.AdjustingSpeed || Time.unscaledTime - router.LastSpeedInput < 0.6f);
            speedGauge.gameObject.SetActive(show); speedValue.gameObject.SetActive(show);
            if (!show) return;
            float top = ThermometerBase + ThermometerFull * Mathf.Clamp01(movement.SpeedCap / MaximumSpeedCap);
            speedGauge.anchorMax = new Vector2(speedGauge.anchorMax.x, top);
            speedFill.anchorMax = new Vector2(speedFill.anchorMax.x, movement.SpeedCap > 0 ? Mathf.Max(0.01f, movement.RequestedSpeed / movement.SpeedCap) : 0.01f);
            speedValue.rectTransform.anchorMin = new Vector2(0.01f, top + 0.003f); speedValue.rectTransform.anchorMax = new Vector2(0.30f, top + 0.035f);
            speedValue.text = $"{movement.RequestedSpeed:F1}/{movement.SpeedCap:F1}";
        }
        private void ExplainSpeedBlocked(MovementPauseReasons pauses)
        {
            if (milestone4 && stamina.Fainted) Say("Recovering from fainting; walking resumes once hearts refill.");
            else if (inventoryOpen || (pauses & MovementPauseReasons.Inventory) != 0) Say("Walking is paused while the backpack is open. Tap CLOSE to resume.");
            else if ((pauses & MovementPauseReasons.Camp) != 0) Say("You are at camp; the next segment arrives in a later milestone. Use RESET SAVE or fixtures to test again.");
            else if ((pauses & MovementPauseReasons.Exhausted) != 0) Say("No stamina. Eat to recover before walking.");
            else Say("Walking is paused: " + pauses);
        }
        private void Update()
        {
            if (ledger == null) return;
            if (milestone4) AdvanceSurvival(Time.deltaTime);
            if (milestone4 && faintOverlay != null)
            {
                bool show = stamina.Fainted && !saveBlocked; faintOverlay.SetActive(show);
                if (show) faintMessage.text = $"FAINTED\n\n{faintLoss.Count} carried entries lost\n\nRecovering... {stamina.Hearts:F2} / {stamina.Maximum:F0} hearts";
            }
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
                    bundle.View.transform.localScale = new Vector3(bundle.Item.Width / 2000f, 0.3f * scale, bundle.Item.Height / 1500f);
                    Vector3 screen = cameraView.WorldToScreenPoint(bundle.View.transform.position + Vector3.up * 0.55f);
                    bundle.Badge.gameObject.SetActive(screen.z > 0 && screen.x >= 0 && screen.x <= Screen.width && screen.y > Screen.height * (inventoryOpen ? bagFrame.anchorMax.y + 0.09f : 0.09f) && screen.y < Screen.height * 0.74f);
                    bundle.Badge.rectTransform.position = screen;
                    bundle.Badge.text = $"{bundle.Item.Definition.Name}\nQ{bundle.Item.Quality} x{bundle.Item.Quantity}";
                }
                else bundle.Badge.gameObject.SetActive(false);
            }
            if (Time.unscaledTime >= nextHud)
            {
                nextHud = Time.unscaledTime + 0.15f;
                status.text = $"{movement.Distance:F1}/{path.Length:F1} m | {movement.ActualSpeed:F2} m/s | Camera {orbit.Yaw:F0}°\n{movement.Pauses} | Bag {ledger.Width / 1000} x {ledger.Height / 1000} | Pickups x1,000" + (milestone4 ? $"\nHearts {stamina.Hearts:F2}/{stamina.Maximum:F0}  Food delivering {stamina.PendingFood:F2}" : "");
                if (milestone4 && gearPanel != null && gearPanel.activeSelf) RefreshGearPanel();
            }
            if (Time.unscaledTime >= nextReport) { nextReport = Time.unscaledTime + 3; WriteReport("sample"); }
            if (Time.unscaledTime >= nextSave) { nextSave = Time.unscaledTime + 2; SaveNow(); }
        }
        private void ApplySafeArea()
        {
            lastSafe = Screen.safeArea; lastSize = new Vector2Int(Screen.width, Screen.height);
            safe.anchorMin = lastSafe.min / new Vector2(Screen.width, Screen.height); safe.anchorMax = lastSafe.max / new Vector2(Screen.width, Screen.height);
        }
        private void OnApplicationPause(bool paused) { if (paused) { CancelAll(); SaveNow(); } }
        private void OnApplicationFocus(bool focused) { if (!focused) { CancelAll(); SaveNow(); } }
        private void OnApplicationQuit() => SaveNow();
        private void OnDestroy() { CancelAll(); if (router != null) { router.Resources = null; router.SpeedBlocked -= ExplainSpeedBlocked; } }
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
        private bool TryAutoEatPreparedFood()
        {
            const int token = -834;
            foreach (ItemEntry item in ledger.Entries)
            {
                if ((item.Owner != ItemOwner.Backpack && item.Owner != ItemOwner.Pocket) || !item.Definition.PreparedFood || !item.Ready || item.Quantity <= 0) continue;
                if (!ledger.Reserve(item, token)) continue;
                bool consumed = ledger.TryConsumeOne(item, token, out _); ledger.Release(item, token);
                if (!consumed) continue;
                stamina.QueueFood(item.Definition.FoodHearts); Redraw(); SaveNow();
                if (!saveBlocked) Say("Out of stamina: ate " + item.Definition.Name + " automatically.");
                return true;
            }
            return false;
        }
        private void AdvanceSurvival(float delta)
        {
            if (saveBlocked || !initialized) return;
            bool foreground = (movement.Pauses & (MovementPauseReasons.Suspended | MovementPauseReasons.Unfocused | MovementPauseReasons.Loading)) == 0;
            float walked = Mathf.Max(0, movement.Distance - lastDistance); lastDistance = movement.Distance;
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
            bool safeArea = (movement.Pauses & MovementPauseReasons.Camp) != 0;
            stamina.Advance(delta, walked, safeArea, foreground, LegDrainPerMetre);
            if (!foreground) return;
            if (wasFainted && !stamina.Fainted)
            {
                router.InteractionLocked = false;
                movement.SetPause(MovementPauseReasons.Exhausted, false);
                SaveNow(); if (!saveBlocked) Say("Recovered. Swipe UP on the centre line to resume.");
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
            if (inventoryOpen) { movement.CloseInventory(); inventoryOpen = false; LayoutBackpack(); }
            if (gearPanel != null) gearPanel.SetActive(false);
            if (detailsPanel != null) detailsPanel.SetActive(false);
            router.CancelAll(); string id = Guid.NewGuid().ToString("N");
            var plan = ledger.PlanFaintLoss(id, maximum => faintRandom.Next(maximum));
            if (!ledger.CommitFaintLoss(plan)) { saveBlocked = true; Say("Faint transaction failed; progress is paused."); return; }
            faintEventId = id; faintLoss.Clear();
            foreach (var lost in plan.Entries) faintLoss.Add(new SavedLoss { itemId = lost.Id, quantity = lost.Quantity });
            selected = null; selectedGearSlot = null; stamina.BeginFaint(); router.InteractionLocked = true;
            // A fainted walker is stopped and needs a fresh upward gesture, matching the saved faint invariants.
            movement.BeginHarvest(); movement.EndHarvest(); movement.SetPause(MovementPauseReasons.Exhausted, true);
            SaveNow();
            if (!saveBlocked) { Redraw(); Say("Fainted: " + faintLoss.Count + " carried entries lost. Recovering in place."); }
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
            if (!ledger.Reserve(selected, token)) { Say("Item is in use."); return; }
            int hearts = selected.Definition.FoodHearts;
            bool consumed = ledger.TryConsumeOne(selected, token, out string reason); ledger.Release(selected, token);
            if (!consumed) { Say(reason); return; }
            stamina.QueueFood(hearts); // Provisional debug values, not regional food balance.
            if (selected.Owner == ItemOwner.Consumed) selected = null;
            Redraw(); if (detailsPanel != null) detailsPanel.SetActive(false);
            SaveNow(); if (!saveBlocked) Say("A test serving queued one heart of gradual recovery.");
        }
        private void ShowDetails(ItemEntry item)
        {
            if (detailsPanel == null || item == null || item.Owner == ItemOwner.Consumed) return;
            selected = item;
            string durability = item.Definition.Stackable ? "" : item.HasWearRemaining ? $"\nDurability {item.WearRemaining:F2}/{item.Profile.MaxDurability}" : $"\nDurability {item.Profile.Durability}/{item.Profile.MaxDurability}";
            detailsText.text = $"{item.Definition.Name}\nQuality Q{item.Quality} | Quantity {item.Quantity}\n{(item.Ready ? "Ready" : "Pending")} | Value {item.Profile.UnitValue}{durability}";
            detailEat.SetActive(item.Definition.FoodHearts > 0 && item.Ready);
            detailEquip.SetActive(item.Definition.GearSlot.HasValue && item.Owner == ItemOwner.Backpack);
            detailsPanel.SetActive(true);
            // Auto-close when no action is taken; EAT and EQUIP close it immediately.
            detailsCloseAt = Time.unscaledTime + 4f;
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
            gearPanel.SetActive(true); RefreshGearPanel();
            Say("Hold a wearable in the bag and drag it here to equip. Drag a slot down into the bag to take it off.");
        }
        private void GrantTestGear()
        {
            if (saveBlocked || stamina.Fainted) return;
            router.CancelAll();
            // Real inventory items, so taking them off returns them to the bag like any other wearable.
            foreach (ItemDefinition definition in PrototypeItems.TestWearables)
            {
                GearSlot slot = definition.GearSlot.Value;
                if (equipment.Get(slot) != null) continue;
                ItemEntry item = ledger.AddWorld("gear-" + Guid.NewGuid().ToString("N"), definition, 1, 5, new ItemProfile(100, 180, 180));
                if (!ledger.RestoreStored(item, ItemOwner.Worn, 0, 0)) throw new InvalidOperationException("Could not equip test gear");
                equipment.Equip(new WornGear(item.Id, slot, definition.GearRank, item.Quality, item.WearRemaining));
            }
            if (ApplyCapabilities()) { SaveNow(); Say("Rank-four test gear equipped. Wear is measured in metres."); }
        }
        private void GrantSpareGear()
        {
            if (saveBlocked || stamina.Fainted) return;
            router.CancelAll(); int overflowCount = 0;
            foreach (ItemDefinition definition in PrototypeItems.TestWearables)
            {
                var item = ledger.AddWorld("spare-" + Guid.NewGuid().ToString("N"), definition, 1, 5, new ItemProfile(100, 180, 180));
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
            Say(equipment.Get(slot) == null ? "Selected " + slot + " (default gear)." : "Selected " + slot + ". Drag it down into the bag or tap TAKE OFF.");
        }
        // Equips a backpack wearable into its own slot, swapping out any real item already worn there.
        private bool EquipItem(ItemEntry item)
        {
            if (saveBlocked || stamina.Fainted || !inventoryOpen) return false;
            if (item == null || item.Owner != ItemOwner.Backpack || !item.Definition.GearSlot.HasValue) { Say("Choose a spare wearable from the backpack."); return false; }
            if (item.Profile.MaxDurability > 0 && item.Profile.Durability <= 0) { Say("That wearable has no remaining durability."); return false; }
            GearSlot slot = item.Definition.GearSlot.Value;
            if (equipment.Get(slot) != null && !TakeOffSlot(slot, null)) return false;
            // A swapped-out backpack can shrink the bag and push the new item to overflow; bring it back first.
            const int token = -831;
            if (item.Owner == ItemOwner.Overflow && ledger.Reserve(item, token)) { ledger.TryReturnToBackpack(item, token, out _); ledger.Release(item, token); }
            router.CancelAll();
            if (!ledger.Reserve(item, token)) { Say("Item is in use."); return false; }
            int oldX = item.X, oldY = item.Y;
            try
            {
                WornGear gear = item.HasWearRemaining
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
        // Returns the worn item to the bag, at the drop point when given; overflow only when the bag has no room.
        private bool TakeOffSlot(GearSlot slot, Vector2? drop)
        {
            if (saveBlocked || stamina.Fainted || !inventoryOpen) return false;
            WornGear gear = equipment.Get(slot);
            if (gear == null) { Say("That slot already has its default gear."); return false; }
            if (gear.Infinite || gear.InstanceId.StartsWith("debug-", StringComparison.Ordinal)) { Say("Older virtual test gear has no bag item; use EXPIRE SLOT."); return false; }
            ItemEntry item = null; foreach (ItemEntry candidate in ledger.Entries) if (candidate.Id == gear.InstanceId && candidate.Owner == ItemOwner.Worn) { item = candidate; break; }
            if (item == null) { saveBlocked = true; movement.SetPause(MovementPauseReasons.Exhausted, true); Say("Worn item is missing; progress is paused."); return false; }
            router.CancelAll();
            if (!ledger.TakeOff(item.Id, gear.RemainingDistance)) { Say("Could not take off that wearable."); return false; }
            equipment.Unequip(slot);
            if (!ApplyCapabilities()) return false;
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
            selected = item; Redraw(); SaveNow();
            if (!saveBlocked) Say(placed ? item.Definition.Name + " returned to the backpack." : reason + "; " + item.Definition.Name + " is in overflow (CLAIM OVERFLOW).");
            return true;
        }
        public void BeginSlotDrag(GearSlot slot, Vector2 position)
        {
            WornGear gear = equipment.Get(slot);
            if (gear == null || !inventoryOpen || saveBlocked || stamina.Fainted) return;
            selectedGearSlot = slot;
            if (slotGhost == null) slotGhost = Label(safe, "", 0, 0, 0, 0, 24);
            slotGhost.text = slot.ToString().ToUpperInvariant(); slotGhost.color = new Color(1, 0.9f, 0.3f);
            slotGhost.rectTransform.sizeDelta = new Vector2(260, 70); slotGhost.rectTransform.position = position;
        }
        public void DragSlot(Vector2 position) { if (slotGhost != null) slotGhost.rectTransform.position = position; }
        public void EndSlotDrag(GearSlot slot, Vector2 position)
        {
            if (slotGhost == null) return;
            Destroy(slotGhost.gameObject); slotGhost = null;
            if (RectTransformUtility.RectangleContainsScreenPoint(bag, position) || RectTransformUtility.RectangleContainsScreenPoint(aperture, position)) TakeOffSlot(slot, position);
            else Say("Drop the slot inside the backpack to take it off.");
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
        private void CreateGearPanel()
        {
            // Pockets and hooks live beside the bag (scroll sideways), so this panel only holds worn equipment.
            gearPanel = Rect("Equipment", safe, 0.02f, 0.56f, 0.98f, 0.80f).gameObject;
            gearPanel.AddComponent<Image>().color = new Color(0.04f, 0.10f, 0.11f, 0.98f);
            gearSummary = Label(gearPanel.transform, "", 0.01f, 0.66f, 0.99f, 0.99f, 20);
            string[] actions = { "R4 TEST GEAR", "EXPIRE SLOT", "EXPIRE ALL", "TAKE OFF", "CLOSE" };
            UnityEngine.Events.UnityAction[] handlers = { GrantTestGear, ExpireSelectedSlot, () => ExpireTestGear(null), UnequipSelectedGear, ToggleGearPanel };
            for (int i = 0; i < actions.Length; i++)
            {
                Text action = Button(gearPanel.transform, actions[i], 0.01f + i * 0.198f, 0.36f, 0.19f + i * 0.198f, 0.62f, handlers[i]);
                action.resizeTextForBestFit = true; action.resizeTextMinSize = 10; action.resizeTextMaxSize = 20;
            }
            for (int i = 0; i < 5; i++)
            {
                int slot = i; float x0 = 0.01f + i * 0.198f;
                Text gearLabel = Button(gearPanel.transform, ((GearSlot)i).ToString().ToUpperInvariant(), x0, 0.03f, x0 + 0.19f, 0.33f, () => GearSlotAction((GearSlot)slot));
                gearLabel.fontSize = 15; gearLabels.Add(gearLabel);
                var drag = gearLabel.transform.parent.gameObject.AddComponent<GearSlotDragSurface>(); drag.Owner = this; drag.Slot = (GearSlot)slot;
            }
            gearPanel.SetActive(false);
        }
        private void RefreshGearPanel()
        {
            int overflow = 0; foreach (ItemEntry item in ledger.Entries) if (item.Owner == ItemOwner.Overflow) overflow++;
            gearSummary.text = $"Hat {equipment.MaxHearts} hearts | Boots {equipment.SpeedCap:F1} m/s\nPockets {equipment.Pockets}/10 | Hooks {equipment.Hooks}/10 | Overflow {overflow}\nDrag bag gear here to equip; drag a slot to the bag to take off" + (selectedGearSlot.HasValue ? " | Selected " + selectedGearSlot.Value : "");
            for (int i = 0; i < gearLabels.Count; i++)
            {
                WornGear item = equipment.Get((GearSlot)i);
                gearLabels[i].text = ((GearSlot)i).ToString().ToUpperInvariant() + (item == null ? "\nDEFAULT" : item.Infinite ? "\nINFINITE" : $"\n{item.RemainingDistance:F1} m");
            }
        }
        private void SaveNow()
        {
            if (!initialized || saveBlocked || saveStore == null || ledger == null) return;
            try
            {
                var snapshot = new SessionSnapshot { version = milestone4 ? 2 : 1, distance = movement.Distance, requestedSpeed = movement.RequestedSpeed,
                    speedCap = movement.SpeedCap, inventoryResume = movement.InventoryResumeSpeed, inventoryOpen = inventoryOpen,
                    postHarvest = movement.PostHarvestStop, camp = (movement.Pauses & MovementPauseReasons.Camp) != 0, yaw = orbit.Yaw };
                if (milestone4)
                {
                    snapshot.survival = new SavedSurvival { hearts = stamina.Hearts, maximum = stamina.Maximum, pendingFood = stamina.PendingFood,
                        fainted = stamina.Fainted, faintElapsed = stamina.FaintElapsed, faintEventId = faintEventId, loss = new List<SavedLoss>(faintLoss) };
                    foreach (GearSlot slot in Enum.GetValues(typeof(GearSlot)))
                    {
                        WornGear gear = equipment.Get(slot); if (gear == null) continue;
                        snapshot.survival.gear.Add(new SavedGear { id = gear.InstanceId, slot = gear.Slot.ToString(), rank = gear.Rank,
                            quality = gear.Quality, remainingDistance = gear.RemainingDistance, infinite = gear.Infinite });
                    }
                }
                foreach (ItemEntry item in ledger.Entries)
                {
                    if (item.Owner == ItemOwner.Consumed) continue;
                    Bundle bundle = bundles.Find(b => b.Item == item);
                    Vector3 position = bundle == null ? Vector3.zero : bundle.View.transform.position;
                    WornGear worn = item.Owner == ItemOwner.Worn && item.Definition.GearSlot.HasValue ? equipment?.Get(item.Definition.GearSlot.Value) : null;
                    snapshot.items.Add(new SavedItem { id = item.Id, definitionId = item.Definition.Id, owner = item.Owner.ToString(), quantity = item.Quantity,
                        quality = item.Quality, ready = item.Ready, x = item.X, y = item.Y, unitValue = item.Profile.UnitValue,
                        maxDurability = item.Profile.MaxDurability, durability = item.Profile.Durability, variant = item.Profile.Variant,
                        hasWear = worn != null ? !worn.Infinite : item.HasWearRemaining, wearRemaining = worn != null ? worn.RemainingDistance : item.WearRemaining,
                        dropped = item.Owner == ItemOwner.World && item.Dropped, trailDistance = item.TrailDistance, worldX = position.x, worldY = position.y, worldZ = position.z });
                }
                SessionSaveStore.Validate(snapshot, path.Length, out _);
                string state = JsonUtility.ToJson(snapshot);
                if (state == lastSavedState) return;
                snapshot.transactionId = Guid.NewGuid().ToString("N");
                saveStore.Save(snapshot);
                lastSavedState = state;
            }
            catch (Exception error)
            {
                saveBlocked = true; movement.SetPause(MovementPauseReasons.Exhausted, true);
                Say("Save failed. Progress is paused; do not close the app. " + error.Message);
                Debug.LogException(error);
            }
        }
        private void RestoreSnapshot(SessionSnapshot snapshot)
        {
            SessionSaveStore.Validate(snapshot, path.Length, out ledger);
            if (milestone4)
            {
                equipment = new EquipmentState();
                foreach (SavedGear gear in snapshot.survival.gear) equipment.Equip(new WornGear(gear.id, (GearSlot)Enum.Parse(typeof(GearSlot), gear.slot), gear.rank, gear.quality, gear.remainingDistance, gear.infinite));
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
                AddBundle(item, new Vector3(saved.worldX, saved.worldY, saved.worldZ));
            }
            Redraw();
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
