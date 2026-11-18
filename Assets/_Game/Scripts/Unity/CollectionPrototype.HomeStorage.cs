using System.Collections.Generic;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Settlement;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Home storage, the container panel (Milestone 11 spec section 2). Tapping a placed chest or the tool rack in the angled
    // interior opens its contents above the bag, which opens with it; both use the bag's cell size, so an item looks the same
    // on either side. Items are dragged between the panel and the bag (grid, hooks, pockets) or moved within the panel. The
    // room, camera and scroll stay as they are underneath. The bag button's return icon closes the panel and the bag.
    public sealed partial class CollectionPrototype
    {
        // The domain's refusal texts, shown on the paper note when a drop fails (the domain returns exactly these).
        public const string ChestNoSpaceNote = "No space in the chest";
        public const string RackSpotTakenNote = "Spot taken";
        public const string RackOnlyGearNote = "Only tools, clothes and backpacks go on the rack";
        public const string EmptyItFirstNote = "Empty it first";
        // The ceiling on chest entries intersecting any visible range (6 columns plus the margin) in the densest real fill:
        // a 48 x 6 chest of single stacks of the smallest-footprint stackable item (checked by ContainerViewChecks). Drawing is
        // never capped: every entry in the visible columns is drawn.
        public const int ChestViewBudget = 360;
        private const int ChestRows = 6, RackColumns = 4, RackRows = 3;
        private const float ChestViewMargin = 1f, ApertureHeight = 0.035f;
        private const string PegViewName = "Peg item";

        private string openContainerId;
        public bool ContainerOpen => openContainerId != null;
        private RectTransform containerFrame, containerContent, containerGrid, containerItems;
        private readonly List<RectTransform> rackCells = new List<RectTransform>();
        private float containerScroll, containerContentWidth;
        private int? containerScrollPointer;
        private float containerScrollStartX, containerScrollStartOffset;
        private (int first, int last) chestDrawn = (-1, -1);
        private SpatialInventory OpenChest => ContainerOpen ? homeStorage.Chest(openContainerId) : null;
        private ToolRack OpenRack => ContainerOpen ? homeStorage.Rack(openContainerId) : null;
        // The bag's cell (LayoutBag sizes the bag grid as ledger width x cell).
        private float BagCell => bag != null && ledger != null && ledger.Width > 0 ? bag.sizeDelta.x * 1000f / ledger.Width : 0f;
        private float ContainerTop => containerFrame != null && ContainerOpen ? containerFrame.anchorMax.y : 0f;

        // ---------------------------------------------------------------- rules (checked by HomeValidation.ContainerViewChecks)

        // Chest columns to draw for a view scrolled scrollUnits from the chest's left edge, viewUnits wide, with a margin on
        // both sides; inclusive, clamped to the chest.
        public static (int first, int last) ChestVisibleColumns(float scrollUnits, float viewUnits, float marginUnits, int widthUnits)
        {
            if (widthUnits <= 0) return (0, -1);
            int first = Mathf.Clamp(Mathf.FloorToInt(Mathf.Max(0f, scrollUnits) - marginUnits), 0, widthUnits - 1);
            int last = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(0f, scrollUnits) + viewUnits + marginUnits), 0, widthUnits - 1);
            return (first, last);
        }

        // Every chest entry intersecting columns first..last: the ones the panel draws (no cap, so nothing visible is missing).
        public static IEnumerable<ItemEntry> ChestEntriesInColumns(SpatialInventory chest, int first, int last)
        {
            if (chest == null || last < first) yield break;
            int from = first * 1000, to = (last + 1) * 1000;
            foreach (ItemEntry e in chest.Entries)
                if (e.Owner == ItemOwner.Backpack && e.X < to && e.X + e.Width > from) yield return e;
        }

        // The container cancel path: a held chest entry is released and stays where it was (a rack spot holds no reservation).
        public static void CancelContainerHold(SpatialInventory source, ItemEntry item, int pointer) { if (source != null && item != null) source.Release(item, pointer); }

        // Every path that leaves the panel's context closes it (CloseContainer): the bag button's return icon and the house's
        // Return ("return", "bag"; ToggleInventory), TearDownInterior ("leave-house"), ResetHomeView (called by CommitTravel:
        // "travel"; by UpdateHome and BeginFaint: "faint"; by UpdateTravel and the settlement context: "leave-settlement"), and EnterBirdsEye ("birds-eye").
        private static readonly string[] ContainerExitPaths = { "return", "bag", "leave-house", "travel", "faint", "leave-settlement", "birds-eye" };
        public static bool ContainerClosesOn(string exitPath) => System.Array.IndexOf(ContainerExitPaths, exitPath) >= 0;

        // ---------------------------------------------------------------- open and close

        private void OpenContainer(string pieceId)
        {
            if (pieceId == null || homeView != HomeView.Interior || NeighbourView || saveBlocked) return;
            if (homeStorage.Chest(pieceId) == null && homeStorage.Rack(pieceId) == null) homeStorage.Sync(home);
            bool chest = homeStorage.Chest(pieceId) != null;
            if (!chest && homeStorage.Rack(pieceId) == null) return;
            if (ContainerOpen) CloseContainer();
            // Nothing else shares the space above the bag: an equip or craft drop over an overlap would land in the container.
            if (gearPanel != null) gearPanel.SetActive(false);
            if (craftPanel != null) craftPanel.SetActive(false);
            if (detailsPanel != null) detailsPanel.SetActive(false);
            openContainerId = pieceId; containerScroll = 0f; chestDrawn = (-1, -1);
            if (!inventoryOpen) { ToggleInventory(); if (!inventoryOpen) { openContainerId = null; return; } }
            else LayoutBackpack();
            SetContainerScroll(0f);
            Say(chest ? "Drag items between the chest and the bag." : "Hang tools, clothes and backpacks on the rack.");
        }

        // Cancels and releases any container contact first, then hides the panel; a closed rack refreshes its pegs in the room.
        private void CloseContainer()
        {
            if (!ContainerOpen) return;
            string closing = openContainerId;
            CancelContainerContacts();
            openContainerId = null; containerScrollPointer = null; chestDrawn = (-1, -1);
            if (containerFrame != null) { ClearContainerContent(); containerFrame.gameObject.SetActive(false); }
            if (homeStorage.Rack(closing) != null) RebuildRackPegs(closing);
            if (inventoryOpen) LayoutBackpack();
        }

        private void CancelContainerContacts()
        {
            var held = new List<int>();
            foreach (var pair in contacts) if (pair.Value.Source != null || pair.Value.RackSpot >= 0) held.Add(pair.Key);
            foreach (int pointer in held)
            {
                Contact c = contacts[pointer];
                CancelContainerHold(c.Source, c.Item, pointer);
                if (c.Ghost != null) Destroy(c.Ghost.gameObject);
                contacts.Remove(pointer);
            }
        }

        // ---------------------------------------------------------------- layout and drawing

        private void EnsureContainerFrame()
        {
            if (containerFrame != null) return;
            containerFrame = Rect("Container viewport", safe, 0, 0, 1, 0.3f);
            containerFrame.SetSiblingIndex(bagFrame.GetSiblingIndex() + 1);   // under the drag ghosts and notes (added last)
            containerFrame.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.105f, 0.085f, 0.97f);
            containerFrame.gameObject.AddComponent<RectMask2D>();
            containerFrame.gameObject.AddComponent<BagPointerSurface>().Owner = this;
            containerContent = Pinned("Container content", containerFrame);
            fillButton = Button(containerFrame, "FILL", 0.82f, 0.86f, 0.98f, 0.98f, DebugFillChest).transform.parent.gameObject;   // debug (test phase), chests only
        }

        private GameObject fillButton;

        // Debug FILL (Milestone 11 test phase): packs the open chest with Q5 stacks of 1,000 so every size can be tested full.
        private void DebugFillChest()
        {
            if (!ContainerOpen || OpenChest == null || saveBlocked) return;
            int stacks = homeStorage.DebugFill(openContainerId, () => "debug-fill-" + System.Guid.NewGuid().ToString("N"));
            SaveNow(true);
            chestDrawn = (-1, -1); DrawChestEntries(true);
            Redraw();
            Say(stacks > 0 ? "Debug: added " + stacks + " stacks." : "The chest is full.");
        }

        private void ClearContainerContent()
        {
            if (containerContent == null) return;
            foreach (Transform child in containerContent) Destroy(child.gameObject);
            rackCells.Clear(); containerGrid = null; containerItems = null; chestDrawn = (-1, -1);
        }

        // From LayoutBag (the bag already laid out): the panel sits on top of the bag's opening, inside the safe area, in the
        // bag's cell size; a chest is its width x 6 cells, the rack 4 x 3 spots that fill the view width (no sideways scroll)
        // in the same height.
        private void LayoutContainer()
        {
            if (!ContainerOpen || safe == null || bagFrame == null) return;
            float cell = BagCell, view = safe.rect.width;
            if (cell <= 0 || safe.rect.height <= 0) return;
            EnsureContainerFrame();
            containerFrame.gameObject.SetActive(true);
            if (fillButton != null) { fillButton.SetActive(OpenChest != null); fillButton.transform.SetAsLastSibling(); }
            float height = ChestRows * cell;
            float bottom = bagFrame.anchorMax.y + ApertureHeight;
            containerFrame.anchorMin = new Vector2(0, bottom); containerFrame.anchorMax = new Vector2(1, Mathf.Min(1f, bottom + height / safe.rect.height));
            ClearContainerContent();
            SpatialInventory chest = OpenChest; ToolRack rack = OpenRack;
            if (chest != null)
            {
                containerContentWidth = cell * chest.Width / 1000f;
                containerContent.sizeDelta = new Vector2(containerContentWidth, height);
                containerGrid = Pinned("Chest grid", containerContent); Place(containerGrid, 0, containerContentWidth, height);
                containerGrid.gameObject.AddComponent<Image>().color = new Color(0.42f, 0.29f, 0.17f, 1f);   // pine boards
                var line = new Color(0.22f, 0.14f, 0.07f, 0.55f);
                for (int x = 1; x < chest.Width / 1000; x++) { RectTransform v = Pinned("Board", containerGrid); Place(v, x * cell - 1f, 2f, height); v.gameObject.AddComponent<Image>().color = line; }
                for (int y = 1; y < chest.Height / 1000; y++) { RectTransform h = Pinned("Board", containerGrid); Place(h, 0, containerContentWidth, 2f); h.anchoredPosition = new Vector2(0, y * height / (chest.Height / 1000f) - 1f); h.gameObject.AddComponent<Image>().color = line; }
                containerItems = Rect("Chest items", containerGrid, 0, 0, 1, 1);
            }
            else if (rack != null)
            {
                containerContentWidth = view;   // four spots and five gaps span the view exactly
                containerContent.sizeDelta = new Vector2(containerContentWidth, height);
                RectTransform board = Pinned("Rack board", containerContent); Place(board, 0, containerContentWidth, height);
                board.gameObject.AddComponent<Image>().color = new Color(0.47f, 0.33f, 0.19f, 1f);
                float gap = SideGap * cell;
                float spotWidth = (view - (RackColumns + 1) * gap) / RackColumns, spotHeight = (height - (RackRows + 1) * gap) / RackRows;
                for (int i = 0; i < ToolRack.Spots; i++)
                {
                    int column = i % RackColumns, row = i / RackColumns;   // row-major from the top-left
                    float x = gap + column * (spotWidth + gap), y = height - (row + 1) * (gap + spotHeight);
                    rackCells.Add(RackCell(board, i, rack[i], x, y, spotWidth, spotHeight));
                }
            }
            SetContainerScroll(containerScroll);
        }

        // A rack spot drawn like a hook cell: the loop skin, the item scaled into it with its durability bar, a timer if pending.
        private RectTransform RackCell(RectTransform parent, int spot, ItemEntry occupant, float x, float y, float width, float height)
        {
            RectTransform cellRect = Pinned("Spot" + spot.ToString("00"), parent); Place(cellRect, x, width, height); cellRect.anchoredPosition = new Vector2(x, y);
            string skin = InventoryContainerStyle.LoopSkin(null);
            InventoryContainerStyle.Apply(cellRect, skin);
            if (occupant == null) { Text number = Label(cellRect, (spot + 1).ToString(), 0.1f, 0.08f, 0.9f, 0.2f, 13); number.color = new Color(.78f, .7f, .53f, .65f); }
            else DrawItemView(Rect("Item", cellRect, 0.08f, 0.08f, 0.92f, 0.98f), occupant, 16);
            InventoryContainerStyle.Apply(Rect("Front retaining loop", cellRect, 0, 0, 1, 1), skin, true);
            if (occupant != null && !occupant.Ready) AddPendingBadge(Rect("Pending", cellRect, 0, 0, 1, 1), occupant);
            return cellRect;
        }

        // Like SetScroll: a narrow container is centred, a wide one clamps; a chest redraws its entries when the visible
        // column range changes.
        private void SetContainerScroll(float offset)
        {
            if (containerContent == null || safe == null) return;
            float view = safe.rect.width;
            containerScroll = containerContentWidth <= view ? (view - containerContentWidth) * 0.5f : Mathf.Clamp(offset, view - containerContentWidth, 0);
            containerContent.anchoredPosition = new Vector2(containerScroll, 0);
            DrawChestEntries(false);
        }

        private void DrawChestEntries(bool force)
        {
            SpatialInventory chest = OpenChest; float cell = BagCell;
            if (chest == null || containerItems == null || cell <= 0) return;
            (int first, int last) range = ChestVisibleColumns(-containerScroll / cell, safe.rect.width / cell, ChestViewMargin, chest.Width / 1000);
            if (!force && range == chestDrawn) return;
            chestDrawn = range;
            foreach (Transform child in containerItems) Destroy(child.gameObject);
            foreach (ItemEntry item in ChestEntriesInColumns(chest, range.first, range.last))
            {
                RectTransform rect = Rect(item.Id, containerItems, (float)item.X / chest.Width, (float)item.Y / chest.Height, (float)(item.X + item.Width) / chest.Width, (float)(item.Y + item.Height) / chest.Height);
                DrawItemView(rect, item, 18);
                if (!item.Ready) AddPendingBadge(rect, item);
            }
        }

        // The selection after a move: cleared when the selected entry has left the bag ledger (into a chest or the rack).
        public static ItemEntry SelectionAfterMove(ItemEntry selected, ItemEntry moved, SpatialInventory bag) =>
            selected != null && selected == moved && (selected.Owner == ItemOwner.Consumed || bag == null || !bag.ContainsId(selected.Id)) ? null : selected;

        // Equipping takes from the bag only: a stale reference to an item that now lives in a chest is refused before anything is taken off.
        public static bool BagOwnsForEquip(SpatialInventory bag, ItemEntry item) => item != null && bag != null && item.Owner == ItemOwner.Backpack && bag.ContainsId(item.Id);

        // A chest drop counts only when the pointer is over the grid rectangle (chest units); the dark frame beside a narrow chest does not.
        public static bool ChestDropAttempted(Vector2 chestPoint, int chestWidth, int chestHeight) =>
            chestPoint.x >= 0 && chestPoint.x <= chestWidth && chestPoint.y >= 0 && chestPoint.y <= chestHeight;

        private Vector2 ChestPoint(Vector2 screen)
        {
            SpatialInventory chest = OpenChest;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(containerGrid, screen, null, out Vector2 local);
            Rect r = containerGrid.rect;
            return new Vector2((local.x - r.xMin) / r.width * chest.Width, (local.y - r.yMin) / r.height * chest.Height);
        }

        private int RackSpotAt(Vector2 screen)
        {
            for (int i = 0; i < rackCells.Count; i++) if (rackCells[i] != null && RectTransformUtility.RectangleContainsScreenPoint(rackCells[i], screen)) return i;
            return -1;
        }

        // The equipment or craft panel, if one is somehow open over the container, takes its own drops first.
        private bool OverOtherPanel(Vector2 screen) =>
            (gearPanel != null && gearPanel.activeSelf && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)gearPanel.transform, screen))
            || (craftPanel != null && craftPanel.activeSelf && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)craftPanel.transform, screen));
        private bool OverContainer(Vector2 screen) => ContainerOpen && containerFrame != null && containerFrame.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(containerFrame, screen);

        // ---------------------------------------------------------------- touch (routed from BeginBag, Move and End)

        // A touch on the panel: an entry is held (reserved in the chest) or a rack spot is held; elsewhere it scrolls the panel.
        private bool ContainerPointerBegin(int pointer, Vector2 position)
        {
            if (!OverContainer(position)) return false;
            SpatialInventory chest = OpenChest; ToolRack rack = OpenRack;
            if (chest != null && containerGrid != null && RectTransformUtility.RectangleContainsScreenPoint(containerGrid, position))
            {
                Vector2 point = ChestPoint(position);
                foreach (ItemEntry item in chest.Entries)
                {
                    if (item.Owner != ItemOwner.Backpack || point.x < item.X || point.x > item.X + item.Width || point.y < item.Y || point.y > item.Y + item.Height) continue;
                    if (!chest.Reserve(item, pointer)) return true;
                    contacts[pointer] = new Contact { Item = item, Source = chest, Start = position, Position = position, Started = Time.unscaledTime, Offset = point - new Vector2(item.X, item.Y) };
                    Say($"{item.Definition.Name} / {item.Quantity} / {(item.Ready ? "Ready" : "Pending")}"); return true;
                }
            }
            else if (rack != null)
            {
                int spot = RackSpotAt(position);
                if (spot >= 0 && rack[spot] != null)
                {
                    foreach (Contact other in contacts.Values) if (other.RackSpot == spot) return true;   // one finger per spot
                    contacts[pointer] = new Contact { Item = rack[spot], RackSpot = spot, Start = position, Position = position, Started = Time.unscaledTime };
                    Say($"{rack[spot].Definition.Name} / {(rack[spot].Ready ? "Ready" : "Pending")}"); return true;
                }
            }
            containerScrollPointer = pointer; containerScrollStartX = position.x; containerScrollStartOffset = containerScroll;
            return true;
        }

        private bool ContainerScrollMove(int pointer, Vector2 position)
        {
            if (containerScrollPointer != pointer) return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(safe, position, null, out Vector2 now);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(safe, new Vector2(containerScrollStartX, position.y), null, out Vector2 start);
            SetContainerScroll(containerScrollStartOffset + now.x - start.x);
            return true;
        }

        private bool ContainerScrollEnd(int pointer, Vector2 position, bool canceled)
        {
            if (containerScrollPointer != pointer) return false;
            if (!canceled) ContainerScrollMove(pointer, position);
            containerScrollPointer = null; return true;
        }

        // A dragged item held over the panel near either screen edge scrolls the panel (not the bag).
        private bool ContainerEdgeScroll(Vector2 position)
        {
            if (!ContainerOpen || containerFrame == null || !containerFrame.gameObject.activeSelf) return false;
            var corners = new Vector3[4]; containerFrame.GetWorldCorners(corners);
            if (position.y < corners[0].y || position.y > corners[1].y) return false;
            float step = safe.rect.width * Time.unscaledDeltaTime;
            if (position.x < Screen.width * 0.08f) SetContainerScroll(containerScroll + step);
            else if (position.x > Screen.width * 0.92f) SetContainerScroll(containerScroll - step);
            return true;
        }

        // Ends a contact that started in the panel, or a bag contact dropped over it. False leaves the contact to the bag.
        private bool EndContainerContact(int pointer, Contact contact, Vector2 position, bool canceled)
        {
            bool fromContainer = contact.Source != null || contact.RackSpot >= 0;
            bool overPanel = !canceled && contact.Dragging && !contact.World && OverContainer(position) && !OverOtherPanel(position);
            if (!fromContainer && !overPanel) return false;
            if (canceled || !contact.Dragging) { Release(pointer, contact); return true; }
            ItemEntry item = contact.Item;
            SpatialInventory chest = OpenChest; ToolRack rack = OpenRack;
            Vector2 half = new Vector2(item.Width, item.Height) * 0.5f;
            bool success = false, attempted = true; string reason = null;
            if (overPanel && chest != null && containerGrid != null)
            {
                Vector2 raw = ChestPoint(position);
                Vector2 point = raw - (contact.Side || contact.RackSpot >= 0 ? half : contact.Offset);
                int x = Mathf.RoundToInt(point.x), y = Mathf.RoundToInt(point.y);
                if (!ChestDropAttempted(raw, chest.Width, chest.Height)) attempted = false;   // the dark frame beside a narrow chest: back silently
                else if (contact.Source == chest) success = chest.TryPlace(item, pointer, x, y, true, out reason);
                else if (!fromContainer) success = ledger.TryTransfer(item, pointer, chest, ItemOwner.Backpack, x, y, true, out reason);
                else attempted = false;
                if (!success && contact.Source == chest && reason != null && reason.StartsWith("No space")) reason = ChestNoSpaceNote;
            }
            else if (overPanel && rack != null)
            {
                int spot = RackSpotAt(position);
                if (spot < 0 || spot == contact.RackSpot) attempted = false;
                else if (contact.RackSpot >= 0) { success = rack.TryMoveSpot(contact.RackSpot, spot); reason = success ? "Moved " + item.Definition.Name : RackSpotTakenNote; }
                else success = rack.TryPut(ledger, item, pointer, spot, out reason);
            }
            else if (fromContainer && inventoryOpen && RectTransformUtility.RectangleContainsScreenPoint(bag, position))
            {
                Vector2 point = BagPoint(position) - (contact.RackSpot >= 0 ? half : contact.Offset);
                int x = Mathf.RoundToInt(point.x), y = Mathf.RoundToInt(point.y);
                if (contact.Source != null) success = contact.Source.TryTransfer(item, pointer, ledger, ItemOwner.Backpack, x, y, true, out reason);
                else success = rack != null && rack.TryTake(contact.RackSpot, ledger, ItemOwner.Backpack, x, y, out reason);
            }
            else if (fromContainer && inventoryOpen && SideCellAt(position, out bool hook, out int index))
            {
                ItemOwner owner = hook ? ItemOwner.Hook : ItemOwner.Pocket;
                if (contact.Source != null) success = contact.Source.TryTransfer(item, pointer, ledger, owner, index, 0, false, out reason);
                else success = rack != null && rack.TryTake(contact.RackSpot, ledger, owner, index, 0, out reason);
            }
            else attempted = false;   // anywhere else: back to where it came from
            Release(pointer, contact);
            if (success) { selected = SelectionAfterMove(selected, item, ledger); Say(reason); Redraw(); SaveNow(); }
            else
            {
                if (attempted) ShowSettlementNote(string.IsNullOrEmpty(reason) ? "Item stays where it was" : reason);
                Redraw();
            }
            return true;
        }

        // ---------------------------------------------------------------- the rack in the room

        // Every placed rack's pegs, when the room is built.
        private void RebuildAllRackPegs()
        {
            if (NeighbourView) return;
            foreach (string id in new List<string>(pieceViews.Keys)) if (homeStorage.Rack(id) != null) RebuildRackPegs(id);
        }

        // One rack's peg views: each occupied spot shows the item's display model under its SpotNN anchor, fitted to the peg box.
        private void RebuildRackPegs(string pieceId)
        {
            ToolRack rack = homeStorage.Rack(pieceId);
            if (rack == null || !pieceViews.TryGetValue(pieceId, out GameObject view) || view == null) return;
            for (int i = 0; i < ToolRack.Spots; i++)
            {
                Transform anchor = FindChild(view.transform, "Spot" + i.ToString("00"));
                if (anchor == null) continue;
                for (int c = anchor.childCount - 1; c >= 0; c--) if (anchor.GetChild(c).name == PegViewName) Destroy(anchor.GetChild(c).gameObject);
                if (rack[i] != null) BuildPegView(anchor, rack[i].Definition);
            }
        }

        // The item's display model (the one the bag picture and item details are rendered from), turned upright when the peg
        // box is taller than the model, scaled uniformly into the anchor's PegBox and centred on it; no colliders, no scripts.
        public static GameObject BuildPegView(Transform anchor, ItemDefinition definition)
        {
            var holder = new GameObject(PegViewName);
            holder.SetActive(false);
            holder.transform.SetParent(anchor, false);
            var fit = new GameObject("Fit", typeof(MeshRenderer), typeof(MeshFilter));
            fit.transform.SetParent(holder.transform, false);
            ItemDefinition model = ItemArt.ModelFor(definition);
            GameObject visual = ResourceVisualTuning.Attach(fit, model);
            if (visual == null) { DestroyImmediate(holder); return null; }
            foreach (Collider c in holder.GetComponentsInChildren<Collider>(true)) DestroyImmediate(c);
            foreach (MonoBehaviour b in holder.GetComponentsInChildren<MonoBehaviour>(true)) DestroyImmediate(b);
            visual.transform.localRotation = ResourceVisualTuning.IconRotation(model) * visual.transform.localRotation;
            FurnitureSpotAnchor spot = anchor.GetComponent<FurnitureSpotAnchor>();
            Vector3 box = spot != null ? spot.PegBox : new Vector3(.22f, .40f, .12f);
            Bounds bounds = LocalMeshBounds(fit.transform, visual);
            if (bounds.size.x > bounds.size.y && box.y > box.x)
            {
                visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f) * visual.transform.localRotation;
                bounds = LocalMeshBounds(fit.transform, visual);
            }
            Vector3 size = Vector3.Max(bounds.size, Vector3.one * 0.001f);
            float scale = Mathf.Min(box.x / size.x, Mathf.Min(box.y / size.y, box.z / size.z));
            fit.transform.localScale = Vector3.one * scale;
            fit.transform.localPosition = -bounds.center * scale;
            holder.SetActive(true);
            return holder;
        }
    }
}
