using System.Collections.Generic;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // First home (spec 2026-11-13, slice 2a; home revision spec 2026-11-14 sections 1 and 4). Tapping the player's house on the
    // street (after the plot choice) or the settlement list's Home icon glides the street camera to the plot, then cuts into
    // the interior (200 m below the street). Every house action is on the inside HUD: Return above the inventory, Store next
    // to it, Upgrade next to the user menu (greyed), Pet centred from house level 3.
    public sealed partial class CollectionPrototype
    {
        public enum HomeView { None, Interior, BirdsEye }
        private HomeView homeView;
        // Between the tap and the cut: the street camera is still gliding to the plot (OpenHome). Counts as open, so the
        // street's list, drag and taps stay off while it glides.
        private bool homeEntering;
        private float homeEnterStarted;
        private const float HomeGlideMaxSeconds = 3f;   // the cut happens at the latest after this, even if the glide still moves
        private bool HomeOpen => homeView != HomeView.None || homeEntering;
        // Home revision section 7: while a neighbour's look-only room is open (or being glided to), its own HomeState from
        // NeighbourHome.Furnish; null for the player's home. The views read it instead of `home`, which they never touch then.
        private HomeState neighbourHome;
        private bool NeighbourView => neighbourHome != null;

        // ---------------------------------------------------------------- texts and rules (checked by HomeValidation.InsideHudChecks)

        // The inside HUD (spec section 4), in the HUD squares' grid (PlaceSquare: columns inward from the screen edge, rows up
        // from the bar line). Pet is centred between the two sides, outside the grid.
        public static readonly string[] HomeInsideButtonNames = { "Home: Return", "Home: Store", "Home: Upgrade", "Home: Pet" };
        public static readonly string[] HomeInsideCaptions = { "Return", "Store", "Upgrade", "Pet" };
        public static readonly string[] HomeInsideIcons = { "return", "store", "upgrade", "pet" };
        public static readonly (string Name, bool Right, int Column, int Row)[] HomeInsideButtonSlots =
        {
            ("Home: Return", false, 0, 1),   // directly above the inventory (the trail's mission tracker spot, hidden here)
            ("Home: Store", false, 1, 0),    // next to the inventory
            ("Home: Upgrade", true, 1, 0),   // next to the user menu (the drop pocket's spot, hidden in the settlement)
        };
        public const string HomePetNote = "The backyard opens with pets.";
        public static bool HomePetButtonVisible(int houseLevel) => houseLevel >= 3;
        // Which inside button shows: all in the player's home (Pet from level 3), only Return in a neighbour's look-only room.
        public static bool HomeInsideButtonVisible(string name, int houseLevel, bool neighbour)
        {
            if (name == "Home: Return") return true;
            if (neighbour) return false;
            return name != "Home: Pet" || HomePetButtonVisible(houseLevel);
        }
        // The user menu's CRAFT: at a camp, and inside the house (an overlay over the room).
        public static bool CraftAvailable(bool atCamp, bool insideHouse) => atCamp || insideHouse;
        private bool CraftHere => CraftAvailable(AtCamp, !NeighbourView && (homeView == HomeView.Interior || homeView == HomeView.BirdsEye));

        public const float HomeHoldSeconds = 0.4f;

        // The furniture touch boxes share the street selection boxes' layer (Default: those boxes sit on fresh roots).
        public const int HomeTouchLayer = 0;
        // Home revision 3: every interior has a 2 m entry strip in front of the door (z 0.2-2.2); ScrollMin sits on its front
        // edge (z 2.2), ScrollMax on the back wall and BackWallTop on the top of the back wall.
        // Framing (user, November 14, spec section 3): a closer, steeper camera computed at runtime for the actual aspect.
        // It looks InteriorPitch degrees down, the room's outer width (grid plus both side walls) spans the screen width edge to
        // edge at the room's front (the door line, GridOrigin z; the strip in front is slightly wider than the screen), and
        // the strip's front edge sits InteriorStripBottom of the screen height above the bottom (over the HUD row).
        // The camera scrolls back along the room's Z only when the top of the back wall would project above InteriorBackTop;
        // the far end puts it exactly there. Offset 0 is the front pose; the range is [Lo, 0].
        public const float EntryDepth = 2.0f;
        public const float InteriorPitch = 65f, InteriorStripBottom = 0.12f, InteriorBackTop = 0.95f, InteriorSideWall = 0.16f;

        public struct InteriorFrame
        {
            public Vector3 Eye;            // interior-root local, offset 0
            public Quaternion Rotation;    // interior-root local
            public float Lo, Hi;           // scroll range along the root's Z (Hi = 0, the front)
            public bool Scrolls => Lo < Hi;
        }

        public static InteriorFrame InteriorFraming(float roomWidth, float roomFrontZ, float stripFrontZ, Vector3 backWallTop, float fieldOfView, float aspect)
        {
            float pitch = InteriorPitch * Mathf.Deg2Rad, sin = Mathf.Sin(pitch), cos = Mathf.Cos(pitch);
            float t = Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            Vector3 forward = new Vector3(0f, -sin, -cos), up = new Vector3(0f, cos, -sin);
            // Depth (along the view) of the strip's front edge; the room's front lies (stripFrontZ - roomFrontZ)·cos further.
            float depth = (roomWidth + 2f * InteriorSideWall) / (2f * t * Mathf.Max(0.05f, aspect)) - (stripFrontZ - roomFrontZ) * cos;
            float vertical = (2f * InteriorStripBottom - 1f) * depth * t;
            Vector3 front = new Vector3(-roomWidth / 2f, 0f, stripFrontZ);
            var frame = new InteriorFrame { Eye = front - forward * depth - up * vertical, Rotation = Quaternion.LookRotation(forward, up), Hi = 0f };
            // Camera moved by T along Z: the back-wall top lands at InteriorBackTop when (vb + T sin) = k t (db + T cos).
            Vector3 d = backWallTop - frame.Eye;
            float db = Vector3.Dot(d, forward), vb = Vector3.Dot(d, up), k = 2f * InteriorBackTop - 1f;
            frame.Lo = Mathf.Min(0f, (k * t * db - vb) / (sin - k * t * cos));
            return frame;
        }

        // From the prefab's anchors: width from EntryArea (-w/2), room front from GridOrigin, strip front from ScrollMin,
        // BackWallTop (fallback: 3 m over ScrollMax).
        public static InteriorFrame InteriorFraming(Transform root, float fieldOfView, float aspect)
        {
            Transform origin = FindChild(root, "GridOrigin"), entry = FindChild(root, "EntryArea"), min = FindChild(root, "ScrollMin"), max = FindChild(root, "ScrollMax"), top = FindChild(root, "BackWallTop");
            float width = entry != null ? -2f * entry.localPosition.x : 4f;
            float front = min != null ? min.localPosition.z : 0f;
            Vector3 back = top != null ? top.localPosition : new Vector3(-width / 2f, 3f, max != null ? max.localPosition.z : -2.5f);
            return InteriorFraming(width, origin != null ? origin.localPosition.z : 0f, front, back, fieldOfView, aspect);
        }

        // Viewport position (0-1, origin bottom-left) of a root-local point with the camera at the given scroll offset.
        public static Vector2 InteriorViewport(InteriorFrame frame, float offset, Vector3 local, float fieldOfView, float aspect)
        {
            Vector3 d = Quaternion.Inverse(frame.Rotation) * (local - frame.Eye - new Vector3(0f, 0f, offset));
            float t = Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            return new Vector2(0.5f + 0.5f * d.x / (d.z * t * aspect), 0.5f + 0.5f * d.y / (d.z * t));
        }

        // Instances every placed piece under the interior root at its cell: local = GridOrigin + X·GridXUnit + Z·GridDepthUnit
        // (pivot = front-left footprint corner, the art contract). Each view keeps exactly one collider, the prefab's footprint
        // BoxCollider on its root (added from the footprint when missing), on HomeTouchLayer: the touch box for taps and holds.
        public static Dictionary<string, GameObject> InstancePieces(Transform interior, HomeState state)
        {
            var views = new Dictionary<string, GameObject>();
            if (interior == null || state == null) return views;
            GridAnchors(interior, out Vector3 o, out Vector3 ax, out Vector3 az);
            foreach (FurniturePiece piece in state.Pieces)
                if (piece.Placed) views[piece.InstanceId] = InstancePiece(interior, piece, o, ax, az);
            return views;
        }

        // One placed piece's view under the interior root (see InstancePieces); also used for a piece bought or taken from
        // storage while the room is open.
        private static GameObject InstancePiece(Transform interior, FurniturePiece piece, Vector3 o, Vector3 ax, Vector3 az)
        {
            FurnitureDefinition definition = FurnitureCatalog.Find(piece.DefinitionId);
            GameObject prefab = Resources.Load<GameObject>("Settlement/Furniture/" + definition.Asset);
            GameObject view;
            if (prefab != null) view = Instantiate(prefab, interior, false);
            else
            {
                Debug.LogWarning("[Home] Missing furniture art " + definition.Asset + "; using a placeholder block.");
                view = new GameObject();
                view.transform.SetParent(interior, false);
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.SetParent(view.transform, false);
                block.transform.localScale = new Vector3(definition.Width * 0.5f, 0.8f, definition.Depth * 0.5f);
                block.transform.localPosition = new Vector3(-definition.Width * 0.25f, 0.4f, -definition.Depth * 0.25f);
            }
            view.name = "Piece " + piece.InstanceId;
            PieceLocalPose(definition, piece.Rotation, out Vector3 corner, out Quaternion turn);
            view.transform.localRotation = turn;
            view.transform.localPosition = PieceLocalPosition(o, ax, az, corner, piece.X, piece.Z);
            BoxCollider box = view.GetComponent<BoxCollider>();
            foreach (Collider c in view.GetComponentsInChildren<Collider>(true)) if (c != box) DestroyImmediate(c);
            if (box == null)
            {
                box = view.AddComponent<BoxCollider>();
                box.size = new Vector3(definition.Width * 0.5f, 1f, definition.Depth * 0.5f);
                box.center = new Vector3(-definition.Width * 0.25f, 0.5f, -definition.Depth * 0.25f);
            }
            view.layer = HomeTouchLayer;
            return view;
        }

        // A piece's view pose at a rotation (home revision, Task 6): the model turned 90 degrees per step, clockwise seen from
        // above (+yaw about the interior's up: the bird's-eye camera has screen-up = room back (-z) and screen-right = grid +X
        // (-x), and +90 yaw takes -z to -x), and the pivot moved by cornerOffset (in cells: x along GridXUnit, z along
        // GridDepthUnit) so the turned footprint fills the cells whose front-left corner is the recorded cell. In grid cells a
        // clockwise quarter turn maps (a, b) to (b, -a); the offset brings the turned rectangle's low corner back to (0, 0).
        public static void PieceLocalPose(FurnitureDefinition d, int rotation, out Vector3 cornerOffset, out Quaternion turn)
        {
            int r = ((rotation % 4) + 4) % 4;
            turn = Quaternion.Euler(0f, 90f * r, 0f);
            float minX = 0f, minZ = 0f;
            foreach (Vector2 c in new[] { new Vector2(d.Width, 0f), new Vector2(0f, d.Depth), new Vector2(d.Width, d.Depth) })
            {
                Vector2 t = c;
                for (int i = 0; i < r; i++) t = new Vector2(t.y, -t.x);
                minX = Mathf.Min(minX, t.x); minZ = Mathf.Min(minZ, t.y);
            }
            cornerOffset = new Vector3(-minX, 0f, -minZ);
        }
        // The view's local position for a piece whose recorded (front-left) cell is (x, z), given its pose's corner offset.
        public static Vector3 PieceLocalPosition(Vector3 o, Vector3 ax, Vector3 az, Vector3 cornerOffset, int x, int z) => o + ax * (x + cornerOffset.x) + az * (z + cornerOffset.z);

        // The grid in the interior's local space: the origin cell corner and one cell along X and along the depth.
        private static void GridAnchors(Transform interior, out Vector3 o, out Vector3 ax, out Vector3 az)
        {
            Transform origin = FindChild(interior, "GridOrigin"), xUnit = FindChild(interior, "GridXUnit"), depthUnit = FindChild(interior, "GridDepthUnit");
            o = origin != null ? interior.InverseTransformPoint(origin.position) : Vector3.zero;
            ax = xUnit != null ? interior.InverseTransformPoint(xUnit.position) - o : new Vector3(-0.5f, 0f, 0f);
            az = depthUnit != null ? interior.InverseTransformPoint(depthUnit.position) - o : new Vector3(0f, 0f, -0.5f);
        }

        // The instance ID whose view owns this collider, or null.
        public static string PieceAt(Dictionary<string, GameObject> views, Collider hit)
        {
            if (views == null || hit == null) return null;
            foreach (KeyValuePair<string, GameObject> pair in views)
                if (pair.Value != null && (hit.gameObject == pair.Value || hit.transform.IsChildOf(pair.Value.transform))) return pair.Key;
            return null;
        }

        public enum StreetTapAction { ChoosePlot, OpenHome, OpenNeighbour, Note }
        // What a tap on a street building does: any house during the plot choice asks for the plot; the player's own plot
        // afterwards opens the home; a neighbour's house at level 1-8 opens its look-only room (neighbourLevel 0 when the
        // plot has no neighbour); everything else (neighbours above 8, shops, warehouse, lumber) shows its paper note.
        public static StreetTapAction SettlementTapAction(StreetBuilding b, bool plotChoiceOpen, int playerPlot, int neighbourLevel)
        {
            if (b.Kind != StreetBuildingKind.House) return StreetTapAction.Note;
            if (plotChoiceOpen) return StreetTapAction.ChoosePlot;
            if (playerPlot >= 0 && b.Plot == playerPlot) return StreetTapAction.OpenHome;
            if (neighbourLevel >= 1 && neighbourLevel <= HomeState.MaxLevel) return StreetTapAction.OpenNeighbour;
            return StreetTapAction.Note;
        }

        // ---------------------------------------------------------------- HUD

        private RectTransform homeInsideHud;
        private readonly RectTransform[] homeInsideButtons = new RectTransform[4];
        private float homeInsideBarLine = float.NaN;

        // Built from CreateSettlementHud: the inside HUD's icon squares (laid out on the current bar line while shown).
        private Text homeUpgradeLabel;
        private void CreateHomeHud()
        {
            homeInsideHud = Rect("Home inside HUD", safe, 0, 0, 1, 1);
            var actions = new UnityEngine.Events.UnityAction[]
            {
                LeaveHome,
                () => { if (homeView == HomeView.Interior && !NeighbourView) OpenStoreMenu(StoreSection.Furniture); },   // the store menu, on Furniture
                () => { if (homeView == HomeView.Interior && !NeighbourView) OpenUpgradeSheet(); },   // the upgrade sheet; greyed at the cap, see below
                () => { if (homeView == HomeView.Interior && !NeighbourView) ShowSettlementNote(HomePetNote); },
            };
            for (int i = 0; i < HomeInsideButtonNames.Length; i++)
            {
                Text label = Button(homeInsideHud, HomeInsideCaptions[i], 0, 0, 0, 0, actions[i]);
                var rect = (RectTransform)label.transform.parent; rect.name = HomeInsideButtonNames[i];
                rect.sizeDelta = new Vector2(HudSquare, HudSquare);
                HudIconStyle.Apply(label, HomeInsideIcons[i]);
                homeInsideButtons[i] = rect;
                if (i != 2) continue;
                // Upgrade: greyed out at the cap (a disabled Button ignores clicks), but a tap still explains why; LayoutHomeInsideHud keeps it current.
                homeUpgradeLabel = label;
                var trigger = rect.gameObject.AddComponent<EventTrigger>();
                var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                click.callback.AddListener(_ => { if (HomeUpgradeTapShowsCapNote(homeView == HomeView.Interior, NeighbourView, home.Level)) ShowSettlementNote(HomeUpgradeCapNote); });
                trigger.triggers.Add(click);
            }
            homeInsideHud.gameObject.SetActive(false);
            CreateHomeBirdsEyeHud();
        }

        // Places the inside squares on the bar line (the HUD squares' current one) and shows Pet from house level 3. In a
        // neighbour's room only Return shows (HomeInsideButtonVisible).
        private void LayoutHomeInsideHud()
        {
            int level = NeighbourView ? neighbourHome.Level : home.Level;
            for (int i = 1; i < homeInsideButtons.Length; i++)
            {
                bool show = HomeInsideButtonVisible(HomeInsideButtonNames[i], level, NeighbourView);
                if (homeInsideButtons[i].gameObject.activeSelf != show) homeInsideButtons[i].gameObject.SetActive(show);
            }
            if (!NeighbourView && homeUpgradeLabel != null)
            {
                bool opens = HomeUpgradeOpensSheet(home.Level);
                var upgrade = homeInsideButtons[2].GetComponent<UnityEngine.UI.Button>();
                if (upgrade.interactable != opens) { upgrade.interactable = opens; homeUpgradeLabel.color = opens ? Color.white : new Color(1f, 1f, 1f, 0.55f); }
            }
            RectTransform petButton = homeInsideButtons[3];
            if (homeInsideBarLine == menuBarLine) return;
            homeInsideBarLine = menuBarLine;
            foreach ((string name, bool right, int column, int row) in HomeInsideButtonSlots)
                PlaceSquare(homeInsideButtons[System.Array.IndexOf(HomeInsideButtonNames, name)], right, menuBarLine, column, row);
            petButton.anchorMin = petButton.anchorMax = new Vector2(0.5f, menuBarLine); petButton.pivot = new Vector2(0.5f, 0f);
            petButton.sizeDelta = new Vector2(HudSquare, HudSquare); petButton.anchoredPosition = Vector2.zero;
        }

        // The bird's-eye view's row (Store piece, Storage, Return), in the same place.
        private RectTransform homeBirdsEyeHud;
        private void CreateHomeBirdsEyeHud()
        {
            homeBirdsEyeHud = Rect("Home bird's-eye HUD", safe, 0, 0, 1, 1);
            var actions = new UnityEngine.Events.UnityAction[]
            {
                StoreSelectedPieceFromView,
                () => { if (homeView == HomeView.BirdsEye) OpenStoreMenu(StoreSection.Storage); },   // the store menu, on Storage
                ReturnToInterior,
            };
            for (int i = 0; i < HomeBirdsEyeButtonNames.Length; i++)
            {
                Text label = Button(homeBirdsEyeHud, HomeBirdsEyeCaptions[i], 0, 0, 0, 0, actions[i]);
                var rect = (RectTransform)label.transform.parent; rect.name = HomeBirdsEyeButtonNames[i];
                rect.anchorMin = rect.anchorMax = new Vector2(1f, SettlementListTop); rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(HudSquare, HudSquare);
                rect.anchoredPosition = new Vector2(-HudMargin - (HomeBirdsEyeButtonNames.Length - 1 - i) * (HudSquare + HudGap), 0f);
                HudIconStyle.Apply(label, HomeBirdsEyeIcons[i]);
            }
            homeBirdsEyeHud.gameObject.SetActive(false);
            CreateStoreMenu();
        }

        // Per frame from UpdateSettlement: the glide before the cut, and each home HUD in its own view unless a panel (bag,
        // craft panel, map), a home sheet or the faint covers it; a covering panel also cancels the home's touch and leaves
        // the room, its pieces, the camera and the scroll as they are.
        private void UpdateHome(bool panelsCovered)
        {
            if (homeInsideHud == null) return;
            if (HomeOpen && stamina != null && stamina.Fainted) { ResetHomeView(); return; }   // a faint drops back to the street
            if (homeEntering) UpdateHomeEntering();
            panelsCovered |= HomeSheetsOpen;
            bool showInside = homeView == HomeView.Interior && !panelsCovered;
            if (homeInsideHud.gameObject.activeSelf != showInside) homeInsideHud.gameObject.SetActive(showInside);
            if (showInside) LayoutHomeInsideHud();
            bool showBirdsEye = homeView == HomeView.BirdsEye && !panelsCovered;
            if (homeBirdsEyeHud != null && homeBirdsEyeHud.gameObject.activeSelf != showBirdsEye) homeBirdsEyeHud.gameObject.SetActive(showBirdsEye);
            if (homeView == HomeView.Interior) UpdateHomeInterior(panelsCovered);
            else if (homeView == HomeView.BirdsEye) UpdateBirdsEye(panelsCovered);
        }

        // The debug HOUSES button hides while a home view is open (UpdateSettlement is its only visibility authority).
        public static bool HouseDebugButtonVisible(bool homeOpen) => !homeOpen;

        // Any path that leaves the home other than the return icon (a new settlement context, the phase leaving Settlement, a
        // faint), also mid-glide: no home view, the house torn down, and the camera snapped to the street pose.
        private void ResetHomeView()
        {
            CloseContainer();   // an open chest or rack closes on every exit path, also outside a home view
            if (!HomeOpen) return;
            homeEntering = false;
            homeView = HomeView.None;
            TearDownInterior();
            settlementListLayout = -1;
            StreetView?.SnapToStreet();
            SetMenu(menuOpen);   // CRAFT leaves the user menu with the house
        }

        // ---------------------------------------------------------------- going in and out (home revision, section 1)

        // The street tap on the player's house and the settlement list's Home icon: glide the street camera to the plot, then
        // (UpdateHomeEntering, once the glide settles) cut into the interior.
        private void OpenHome()
        {
            if (phase != TravelPhase.Settlement || saveBlocked || settlement.PlotIndex < 0 || HomeOpen) return;
            StreetBuilding b = Street?.Find("plot." + settlement.PlotIndex);
            if (b == null || StreetView == null) return;
            neighbourHome = null;
            GlideIntoHouse(b);
        }

        // A street tap on a neighbour's house at level 1-8 (home revision section 7): the same glide and cut as the player's
        // home, into the same level's interior furnished by NeighbourHome.Furnish from the persistent settlement seed. Look
        // only: Return and the scroll; nothing is saved, and `home` is never read or written.
        private void OpenNeighbourHome(int plot)
        {
            if (phase != TravelPhase.Settlement || saveBlocked || HomeOpen || plot == settlement.PlotIndex) return;
            int level = NeighbourHouseLevel(plot);
            if (level < 1 || level > HomeState.MaxLevel) return;
            StreetBuilding b = Street?.Find("plot." + plot);
            if (b == null || StreetView == null) return;
            neighbourHome = NeighbourHome.Furnish(settlement.Seed, plot, level);
            GlideIntoHouse(b);
        }

        // The neighbour's level on this plot, 0 when the plot has no neighbour (before the plot choice, or the player's own).
        // The only neighbour-level lookup: the street note shows at least level 1 (NoteFor).
        private int NeighbourHouseLevel(int plot)
        {
            foreach (var n in settlement.Neighbours) if (n.Plot == plot) return n.Level;
            return 0;
        }

        private void GlideIntoHouse(StreetBuilding b)
        {
            StreetCamera view = StreetView;
            CancelSettlementPointer();
            settlementListOpen = false; settlementListLayout = -1;
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            homeEntering = true; homeEnterStarted = Time.unscaledTime;
            view.GlideTo(b.CentreX);
        }

        private void UpdateHomeEntering()
        {
            StreetScroll scroll = StreetView != null ? StreetView.Scroll : null;
            if (scroll != null && scroll.Moving && Time.unscaledTime - homeEnterStarted < HomeGlideMaxSeconds) return;
            homeEntering = false;
            OpenHomeInterior();
        }

        // The return icon: tear the house down and cut back to the street camera at the plot (the street scroll still stands
        // on the plot's X).
        private void LeaveHome()
        {
            if (homeView != HomeView.Interior) return;
            homeEntering = false;
            homeView = HomeView.None;
            TearDownInterior();
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            settlementListLayout = -1;
            StreetView?.SnapToStreet();
            SetMenu(menuOpen);
        }

        private static Transform FindChild(Transform parent, string name)
        {
            if (parent == null) return null;
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindChild(child, name);
                if (found != null) return found;
            }
            return null;
        }

        // ---------------------------------------------------------------- interior (task 6)

        private Transform interiorRoot;
        private InteriorFrame interiorFrame;
        private Dictionary<string, GameObject> pieceViews = new Dictionary<string, GameObject>();
        private StreetScroll interiorScroll;   // null for rooms that fit the screen (InteriorFrame.Scrolls false)

        // After the glide: build the home level's interior (or the neighbour's, from neighbourHome) below the street, instance the placed pieces and cut the camera to
        // the runtime framing (InteriorFraming, fitted to this screen's aspect; a cut: the interior is 200 m below, an ease would
        // fly through the ground).
        private void OpenHomeInterior()
        {
            if (homeView != HomeView.None || StreetView == null) return;
            if (world == null) world = GetComponent<MovementPrototype>();
            HomeState shown = neighbourHome ?? home;   // a neighbour's look-only room, else the player's home
            Transform root = world != null ? world.BuildInterior(shown.Level) : null;
            if (root == null) { Debug.LogWarning("[Home] No interior for house level " + shown.Level + "; staying on the street."); neighbourHome = null; settlementListLayout = -1; return; }
            CancelSettlementPointer();
            if (houseFireworks != null) houseFireworks.Stop();   // going in ends the upgrade fireworks
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            interiorRoot = root;
            pieceViews = InstancePieces(root, shown);
            if (neighbourHome == null) RebuildAllRackPegs();   // hung items on every rack's pegs (CollectionPrototype.HomeStorage.cs)
            float fov = cameraView != null ? cameraView.fieldOfView : StreetCamera.FieldOfView;
            float aspect = cameraView != null ? cameraView.aspect : (Screen.height > 0 ? (float)Screen.width / Screen.height : 1116f / 2480f);
            interiorFrame = InteriorFraming(root, fov, aspect);
            interiorScroll = interiorFrame.Scrolls ? new StreetScroll(interiorFrame.Lo, interiorFrame.Hi, interiorFrame.Hi) : null;
            homeView = HomeView.Interior;
            ApplyInteriorCamera();
            SetMenu(menuOpen);   // CRAFT joins the user menu inside the house (not in a neighbour's: CraftHere)
        }

        // The interior camera: the framed front pose moved back along the room's Z by the scroll offset (0 = the front).
        private void ApplyInteriorCamera()
        {
            if (interiorRoot == null || StreetView == null) return;
            float offset = interiorScroll != null ? interiorScroll.X : 0f;
            StreetView.HoldAt(interiorRoot.TransformPoint(interiorFrame.Eye + new Vector3(0f, 0f, offset)), interiorRoot.rotation * interiorFrame.Rotation);
        }

        // Per frame in the interior: the scroll's glide and spring, a panel cancelling the touch, and the hold timer.
        private void UpdateHomeInterior(bool panelsCovered)
        {
            if (panelsCovered && interiorPointer.HasValue) CancelInteriorPointer();
            if (interiorScroll != null && interiorScroll.Moving)
            {
                interiorScroll.Tick(Time.unscaledDeltaTime, Time.unscaledTime);
                ApplyInteriorCamera();
            }
            if (interiorPointer.HasValue && !interiorDragging && !interiorCaught && !interiorHeld && interiorPiece != null && !NeighbourView
                && Time.unscaledTime - interiorDownAt >= HomeHoldSeconds)
            {
                interiorHeld = true;
                interiorScroll?.Cancel();
                EnterBirdsEye(interiorPiece);
            }
        }


        // Every exit from the house or a neighbour's room (LeaveHome, ResetHomeView incl. a faint): no sheet or buy confirmation, no touch, no
        // scroll, no views, no prefab, HUD hidden.
        private void TearDownInterior()
        {
            CloseContainer();
            CloseHomeSheets();
            TearDownBirdsEye();
            CancelInteriorPointer();
            interiorScroll = null;
            pieceViews = new Dictionary<string, GameObject>();
            interiorRoot = null;
            neighbourHome = null;   // a neighbour's room is never kept or saved
            if (world == null) world = GetComponent<MovementPrototype>();
            if (world != null) world.ClearInterior();
            if (homeInsideHud != null) homeInsideHud.gameObject.SetActive(false);
        }

        // ---------------------------------------------------------------- interior touch

        private int? interiorPointer;
        private Vector2 interiorStart, interiorLast;
        private float interiorDownAt;
        private bool interiorDragging, interiorCaught, interiorHeld;
        private string interiorPiece;

        // One finger: a vertical drag scrolls the room (StreetScroll on Z), a hold of HomeHoldSeconds on a piece enters
        // the bird's-eye view with it, a short tap on a chest or the tool rack opens it above the bag; nothing else reacts.
        private bool InteriorPointerBegin(int pointer, Vector2 position)
        {
            if (interiorPointer.HasValue) return true;
            if (homeView == HomeView.BirdsEye) return BirdsEyePointerBegin(pointer, position);
            interiorPointer = pointer; interiorStart = interiorLast = position; interiorDownAt = Time.unscaledTime;
            interiorDragging = false; interiorHeld = false;
            interiorCaught = interiorScroll != null && interiorScroll.Begin(Time.unscaledTime);
            interiorPiece = NeighbourView ? null : TapPiece(position);   // a neighbour's room: holds and chest taps do nothing
            return true;
        }
        private void InteriorPointerMove(Vector2 position)
        {
            if (homeView == HomeView.BirdsEye) { BirdsEyePointerMove(position); return; }
            if (!interiorDragging && (position - interiorStart).sqrMagnitude > Mathf.Pow(Screen.height * 0.012f, 2)) { interiorDragging = true; interiorLast = interiorStart; }
            if (interiorDragging && !interiorHeld && interiorScroll != null)
            {
                interiorScroll.Drag(InteriorDragMetres(position.y - interiorLast.y), Time.unscaledTime);
                ApplyInteriorCamera();
            }
            interiorLast = position;
        }
        private void InteriorPointerEnd(Vector2 position, bool canceled)
        {
            if (homeView == HomeView.BirdsEye) { BirdsEyePointerEnd(position, canceled); return; }
            if (!canceled) InteriorPointerMove(position);
            bool tap = !canceled && !interiorDragging && !interiorCaught && !interiorHeld;
            string piece = interiorPiece;
            bool held = interiorHeld;
            interiorPointer = null; interiorDragging = false; interiorCaught = false; interiorHeld = false; interiorPiece = null;
            if (interiorScroll != null) { if (canceled || held) interiorScroll.Cancel(); else interiorScroll.End(Time.unscaledTime); }
            if (tap && piece != null && IsContainerPiece(piece)) OpenContainer(piece);
        }
        private void CancelInteriorPointer()
        {
            interiorPointer = null; interiorDragging = false; interiorCaught = false; interiorHeld = false; interiorPiece = null;
            interiorScroll?.Cancel();
            CancelBirdsEyeTouch();
        }

        // Screen pixels to metres of floor: the floor under the finger follows it. A finger moving up (positive dy) brings the
        // front into view, so the camera moves toward the front (+offset); down reveals the back.
        private float InteriorDragMetres(float pixels)
        {
            if (Screen.height <= 0 || cameraView == null || interiorRoot == null) return 0f;
            float pitch = InteriorPitch * Mathf.Deg2Rad;
            float height = Mathf.Max(1f, interiorFrame.Eye.y);
            float distance = height / Mathf.Sin(pitch);
            float floorPerScreen = 2f * distance * Mathf.Tan(cameraView.fieldOfView * 0.5f * Mathf.Deg2Rad) / Mathf.Sin(pitch);
            return pixels / Screen.height * floorPerScreen;
        }

        private readonly RaycastHit[] interiorHits = new RaycastHit[8];
        private string TapPiece(Vector2 screen)
        {
            if (cameraView == null || pieceViews.Count == 0) return null;
            int count = Physics.RaycastNonAlloc(cameraView.ScreenPointToRay(screen), interiorHits, cameraView.farClipPlane, ~0, QueryTriggerInteraction.Collide);
            string found = null; float nearest = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                string id = PieceAt(pieceViews, interiorHits[i].collider);
                if (id != null && interiorHits[i].distance < nearest) { found = id; nearest = interiorHits[i].distance; }
            }
            return found;
        }
        private bool IsContainerPiece(string instanceId)
        {
            foreach (FurniturePiece p in home.Pieces) if (p.InstanceId == instanceId) return HomeStorage.IsContainerDefinition(FurnitureCatalog.Find(p.DefinitionId));
            return false;
        }
    }
}
