using System.Collections.Generic;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheTrailWalker
{
    public sealed partial class CollectionPrototype
    {
        // Bird's-eye placement view (task 7), checked by HomeValidation.PlacementViewChecks. Its Storage opens the store
        // menu on its Storage section (home revision, Task 5).
        public static readonly string[] HomeBirdsEyeButtonNames = { "Home: Store piece", "Home: Storage", "Home: Return" };
        public static readonly string[] HomeBirdsEyeCaptions = { "Store", "Storage", "Return" };
        public static readonly string[] HomeBirdsEyeIcons = { "store_piece", "storage", "return" };
        public const string HomeSelectFirstNote = "Hold a piece to select it.";
        public const float BirdsEyeMargin = 0.5f;    // metres of floor shown beyond each side wall
        public const float BirdsEyeHeight = 12f;     // the top-down camera's height above the floor (orthographic: only clipping)
        public const string TurnPendingNote = "No room for it here. Drag it to a free spot.";

        // What lifting the bird's-eye finger does (home revision, Task 6): a drag drops; a short press on the already
        // selected piece (not the finger that entered the view, under the hold threshold, not dragged) turns it; anything
        // else (a hold-release, a tap elsewhere) does nothing.
        public enum BirdsEyeRelease { None, Rotate, Drop }
        public static BirdsEyeRelease ClassifyRelease(bool onSelected, bool inert, bool dragged, float heldSeconds)
        {
            if (dragged) return BirdsEyeRelease.Drop;
            return onSelected && !inert && heldSeconds < HomeHoldSeconds ? BirdsEyeRelease.Rotate : BirdsEyeRelease.None;
        }

        // A release over a cell: the piece moves only when it fits there; otherwise nothing changes (it snaps back).
        public static bool CommitPieceDrop(HomeState home, string instanceId, int x, int z) => home != null && instanceId != null && home.Move(instanceId, x, z);
        // The same at the view's rotation: a piece turned where it did not fit commits its turn only where it is dropped and fits.
        public static bool CommitPieceDrop(HomeState home, string instanceId, int x, int z, int rotation) => home != null && instanceId != null && home.Move(instanceId, x, z, rotation);
        // Store piece: the selected piece goes to house storage and the selection clears (the new selection is returned).
        public static string StoreSelectedPiece(HomeState home, string selected)
        {
            if (home != null && selected != null) home.Store(selected);
            return null;
        }
        // With house storage: a chest or rack that still holds items stays placed and selected, and the reason is returned.
        public static string StoreSelectedPiece(HomeState home, HomeStorage storage, string selected, out string reason)
        {
            reason = null;
            if (home == null || selected == null) return null;
            if (storage == null) { home.Store(selected); return null; }
            return storage.TryStorePiece(home, selected, out reason) ? null : selected;
        }
        public static bool CellInGrid(int x, int z, int gridWidth, int gridDepth) => x >= 0 && z >= 0 && x < gridWidth && z < gridDepth;
        // The dragged piece's cell: the finger's cell minus where on the piece it was grabbed, clamped so the footprint stays
        // inside the room (it can still be red over other pieces).
        public static void DragCell(int fingerX, int fingerZ, int grabX, int grabZ, int width, int depth, int gridWidth, int gridDepth, out int x, out int z)
        {
            x = Mathf.Clamp(fingerX - grabX, 0, Mathf.Max(0, gridWidth - width));
            z = Mathf.Clamp(fingerZ - grabZ, 0, Mathf.Max(0, gridDepth - depth));
        }
        // A floor point in the interior's local space to its cell (the inverse of InstancePieces' mapping); may be outside the grid.
        public static void CellFromLocal(Vector3 local, Vector3 origin, Vector3 xUnit, Vector3 depthUnit, out int x, out int z)
        {
            Vector3 d = local - origin;
            x = Mathf.FloorToInt(Vector3.Dot(d, xUnit) / Mathf.Max(1e-6f, xUnit.sqrMagnitude));
            z = Mathf.FloorToInt(Vector3.Dot(d, depthUnit) / Mathf.Max(1e-6f, depthUnit.sqrMagnitude));
        }
        // The top-down camera fits the room's width (plus a margin each side) across the screen.
        public static float BirdsEyeOrthoSize(float roomWidth, float aspect) => (roomWidth * 0.5f + BirdsEyeMargin) / Mathf.Max(0.01f, aspect);
        // A room deeper than the view scrolls front (offset 0) to back (lo) by the depth that does not fit, as in Task 6.
        public static void BirdsEyeScrollRange(float roomDepth, float visibleDepth, out float lo, out float hi)
        {
            hi = 0f; lo = Mathf.Min(0f, visibleDepth - roomDepth);
        }

        // ---------------------------------------------------------------- bird's-eye placement (task 7)

        private string selectedPiece;              // the piece the view moves, or null (after Store piece)
        private int selectedX, selectedZ;          // its committed cell (the home's)
        private int selectedRotation;              // its rotation in the view: the home's, or a turn not yet placed (red footprint)
        private int dragX, dragZ, grabX, grabZ;    // the drag's target cell and where on the footprint the finger holds it
        private bool birdsDragging, birdsInert, birdsScrolling, birdsCaught, birdsHeld, birdsMoved;
        private string birdsTouched;
        private StreetScroll birdsScroll;          // null when the room fits the view
        private float birdsOrthoSize, birdsRoomWidth, birdsRoomDepth;
        private Vector3 birdsOrigin, birdsXUnit, birdsDepthUnit;   // GridAnchors of the interior
        private GameObject footprintQuad;
        private MaterialPropertyBlock footprintTint;
        private static readonly Color FootprintFits = new Color(0.32f, 0.8f, 0.36f), FootprintBlocked = new Color(0.86f, 0.24f, 0.2f);

        // A 0.4 s hold on a piece in the interior: the top-down orthographic view over the room with that piece selected and
        // the grid shown. The hold's finger stays the home's but does nothing until it lifts.
        private void EnterBirdsEye(string instanceId)
        {
            if (homeView != HomeView.Interior || instanceId == null || interiorRoot == null || cameraView == null || StreetView == null || !pieceViews.ContainsKey(instanceId)) return;
            interiorScroll?.Cancel();
            CloseContainer();   // the chest or rack panel never stays open over the bird's-eye view
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            homeView = HomeView.BirdsEye;
            birdsInert = interiorPointer.HasValue;
            birdsDragging = birdsScrolling = birdsCaught = birdsHeld = birdsMoved = false; birdsTouched = null;
            GridAnchors(interiorRoot, out birdsOrigin, out birdsXUnit, out birdsDepthUnit);
            birdsRoomWidth = birdsXUnit.magnitude * home.GridWidth;
            birdsRoomDepth = birdsDepthUnit.magnitude * home.GridDepth;
            birdsOrthoSize = BirdsEyeOrthoSize(birdsRoomWidth, cameraView.aspect);
            BirdsEyeScrollRange(birdsRoomDepth, 2f * birdsOrthoSize, out float lo, out float hi);
            birdsScroll = lo < hi ? new StreetScroll(lo, hi, hi) : null;
            Transform guides = FindChild(interiorRoot, "PlacementGuides_PreviewOnly");
            if (guides != null) guides.gameObject.SetActive(true);
            cameraView.orthographic = true; cameraView.orthographicSize = birdsOrthoSize;
            ApplyBirdsEyeCamera();
            SelectPiece(instanceId);
        }

        // Straight down over the room, screen up = the back wall, screen right = the grid's X. A room that fits sits centred;
        // a deeper one shows the front at offset 0 and the back at the scroll's lower bound.
        private void ApplyBirdsEyeCamera()
        {
            if (interiorRoot == null || StreetView == null) return;
            Vector3 across = birdsXUnit.normalized, back = birdsDepthUnit.normalized;
            float depthCentre = birdsScroll != null ? birdsOrthoSize - birdsScroll.X : birdsRoomDepth * 0.5f;
            Vector3 centre = birdsOrigin + across * (birdsRoomWidth * 0.5f) + back * depthCentre + Vector3.up * BirdsEyeHeight;
            StreetView.HoldAt(interiorRoot.TransformPoint(centre), interiorRoot.rotation * Quaternion.LookRotation(Vector3.down, back));
        }

        // Return: back to the angled interior at its scroll offset.
        private void ReturnToInterior()
        {
            if (homeView != HomeView.BirdsEye) return;
            CancelInteriorPointer();
            TearDownBirdsEye();
            homeView = HomeView.Interior;
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            ApplyInteriorCamera();
        }

        // Every exit from the bird's-eye view (Return, and through TearDownInterior: LeaveHome, ResetHomeView incl. a faint): a
        // drag in progress is discarded (the piece stays at its committed cell), the camera is perspective again, no footprint,
        // no selection, the grid hidden and the HUD hidden.
        private void TearDownBirdsEye()
        {
            CancelBirdsEyeTouch();
            DiscardPendingTurn();
            selectedPiece = null; birdsScroll = null;
            if (footprintQuad != null) DestroyImmediate(footprintQuad);
            footprintQuad = null;
            if (cameraView != null && cameraView.orthographic) cameraView.orthographic = false;
            Transform guides = interiorRoot != null ? FindChild(interiorRoot, "PlacementGuides_PreviewOnly") : null;
            if (guides != null) guides.gameObject.SetActive(false);
            if (homeBirdsEyeHud != null) homeBirdsEyeHud.gameObject.SetActive(false);
        }

        // Per frame in the bird's-eye view: a panel cancelling the touch, the scroll's glide and spring, and the hold that
        // selects another piece (and picks it up under the same finger).
        private void UpdateBirdsEye(bool panelsCovered)
        {
            if (panelsCovered && interiorPointer.HasValue) CancelInteriorPointer();
            if (birdsScroll != null && birdsScroll.Moving)
            {
                birdsScroll.Tick(Time.unscaledDeltaTime, Time.unscaledTime);
                ApplyBirdsEyeCamera();
            }
            if (interiorPointer.HasValue && !birdsInert && !birdsDragging && !birdsScrolling && !birdsCaught && !birdsHeld
                && birdsTouched != null && birdsTouched != selectedPiece && pieceViews.ContainsKey(birdsTouched)
                && Time.unscaledTime - interiorDownAt >= HomeHoldSeconds)
            {
                birdsHeld = true;
                birdsScroll?.Cancel();
                SelectPiece(birdsTouched);
                StartPieceDrag(interiorLast);
            }
        }

        private FurniturePiece HomePiece(string instanceId)
        {
            if (instanceId == null) return null;
            foreach (FurniturePiece p in home.Pieces) if (p.InstanceId == instanceId) return p;
            return null;
        }

        private void SelectPiece(string instanceId)
        {
            if (birdsDragging) SnapSelectedBack();
            birdsDragging = false;
            DiscardPendingTurn();
            FurniturePiece piece = HomePiece(instanceId);
            if (piece == null || !piece.Placed || !pieceViews.ContainsKey(instanceId)) { selectedPiece = null; UpdateFootprint(); return; }
            selectedPiece = instanceId; selectedX = dragX = piece.X; selectedZ = dragZ = piece.Z; selectedRotation = piece.Rotation;
            UpdateFootprint();
        }

        // Store piece: the selected piece goes to house storage, its view leaves the room, the selection clears; saved.
        private void StoreSelectedPieceFromView()
        {
            if (homeView != HomeView.BirdsEye) return;
            if (selectedPiece == null) { ShowSettlementNote(HomeSelectFirstNote); return; }
            CancelInteriorPointer();
            string id = selectedPiece;
            string next = StoreSelectedPiece(home, homeStorage, id, out string refusal);
            if (refusal != null) { ShowSettlementNote(EmptyItFirstNote); return; }   // the only refusal: the chest or rack still holds items
            selectedPiece = next;
            if (pieceViews.TryGetValue(id, out GameObject view)) { if (view != null) DestroyImmediate(view); pieceViews.Remove(id); }
            UpdateFootprint();
            SaveNow();
        }

        // The floor cell under a screen point (a ray onto the room's floor plane, the cell from the grid origin), possibly
        // outside the grid; false only when the ray misses the floor plane.
        private bool FloorCellAt(Vector2 screen, out int x, out int z)
        {
            x = z = -1;
            if (cameraView == null || interiorRoot == null) return false;
            Ray ray = cameraView.ScreenPointToRay(screen);
            var floor = new Plane(interiorRoot.up, interiorRoot.TransformPoint(birdsOrigin));
            if (!floor.Raycast(ray, out float distance)) return false;
            CellFromLocal(interiorRoot.InverseTransformPoint(ray.GetPoint(distance)), birdsOrigin, birdsXUnit, birdsDepthUnit, out x, out z);
            return true;
        }
        // The grid cell under a screen point; false when the point is off the room's grid.
        private bool TryCellAt(Vector2 screen, out int x, out int z) => FloorCellAt(screen, out x, out z) && CellInGrid(x, z, home.GridWidth, home.GridDepth);

        // The selected piece's view at a cell, turned to the view's rotation (the pose InstancePiece uses).
        private void PlaceSelectedView(int x, int z)
        {
            FurniturePiece piece = HomePiece(selectedPiece);
            if (piece == null || !pieceViews.TryGetValue(selectedPiece, out GameObject view) || view == null) return;
            PieceLocalPose(piece.Definition, selectedRotation, out Vector3 corner, out Quaternion turn);
            view.transform.localRotation = turn;
            view.transform.localPosition = PieceLocalPosition(birdsOrigin, birdsXUnit, birdsDepthUnit, corner, x, z);
        }

        // A tap on the selected piece always turns it a quarter clockwise in the view. Where the turn fits in place it is
        // committed and saved; otherwise the piece stays turned over a red footprint, uncommitted, until a drop where it fits
        // commits cell and turn together (further taps keep turning it; changing the selection or leaving the view discards it).
        private void RotateSelectedPiece()
        {
            FurniturePiece piece = HomePiece(selectedPiece);
            if (piece == null || !piece.Placed) return;
            selectedRotation = (selectedRotation + 1) % 4;
            dragX = selectedX; dragZ = selectedZ;
            bool committed = selectedRotation == piece.Rotation || home.Move(selectedPiece, selectedX, selectedZ, selectedRotation);
            PlaceSelectedView(selectedX, selectedZ);
            UpdateFootprint();
            if (!committed) { ShowSettlementNote(TurnPendingNote); return; }
            HideSettlementNote();
            SaveNow();
        }
        // A turn never placed goes back to the home's rotation at the committed cell.
        private void DiscardPendingTurn()
        {
            FurniturePiece piece = HomePiece(selectedPiece);
            if (piece == null || selectedRotation == piece.Rotation) return;
            selectedRotation = piece.Rotation;
            dragX = selectedX; dragZ = selectedZ;
            PlaceSelectedView(selectedX, selectedZ);
        }
        private void HideSettlementNote() { if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false); }
        private void SnapSelectedBack()
        {
            dragX = selectedX; dragZ = selectedZ;
            PlaceSelectedView(selectedX, selectedZ);
            UpdateFootprint();
        }

        // The footprint quad under the selected piece at its drag cell: green where it fits (or where it already is), red where
        // it is blocked; hidden with no selection. It lives under the interior root, so ClearInterior removes it too.
        private void UpdateFootprint()
        {
            FurniturePiece piece = HomePiece(selectedPiece);
            if (piece == null || interiorRoot == null) { if (footprintQuad != null) footprintQuad.SetActive(false); return; }
            if (footprintQuad == null)
            {
                footprintQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                footprintQuad.name = "Placement footprint";
                DestroyImmediate(footprintQuad.GetComponent<Collider>());
                footprintQuad.transform.SetParent(interiorRoot, false);
                footprintQuad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);   // face up
                var renderer = footprintQuad.GetComponent<Renderer>();
                if (material != null) renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                footprintTint = new MaterialPropertyBlock();
            }
            FurnitureDefinition d = piece.Definition;
            var (w, dp) = HomeState.Footprint(d, selectedRotation);
            bool fits = (dragX == selectedX && dragZ == selectedZ && selectedRotation == piece.Rotation) || home.CanPlace(d, dragX, dragZ, selectedPiece, selectedRotation);
            footprintQuad.SetActive(true);
            footprintQuad.transform.localPosition = birdsOrigin + birdsXUnit * (dragX + w * 0.5f) + birdsDepthUnit * (dragZ + dp * 0.5f) + Vector3.up * 0.02f;
            footprintQuad.transform.localScale = new Vector3(birdsXUnit.magnitude * w, birdsDepthUnit.magnitude * dp, 1f);
            Color tint = fits ? FootprintFits : FootprintBlocked;
            footprintTint.SetColor("_BaseColor", tint); footprintTint.SetColor("_Color", tint);
            footprintQuad.GetComponent<Renderer>().SetPropertyBlock(footprintTint);
        }

        // ---------------------------------------------------------------- bird's-eye touch (the interior's pointer id)

        // One finger: on the selected piece it drags the piece (snapped to whole cells); elsewhere a vertical drag scrolls deep
        // rooms; a hold of HomeHoldSeconds on another piece selects it and picks it up.
        private bool BirdsEyePointerBegin(int pointer, Vector2 position)
        {
            interiorPointer = pointer; interiorStart = interiorLast = position; interiorDownAt = Time.unscaledTime;
            birdsInert = birdsScrolling = birdsHeld = birdsDragging = birdsCaught = birdsMoved = false;
            birdsTouched = TapPiece(position);
            if (birdsTouched != null && birdsTouched == selectedPiece) { StartPieceDrag(position); if (birdsDragging) return true; }
            birdsCaught = birdsScroll != null && birdsScroll.Begin(Time.unscaledTime);
            return true;
        }
        private void StartPieceDrag(Vector2 position)
        {
            FurniturePiece piece = HomePiece(selectedPiece);
            if (piece == null || !TryCellAt(position, out int fx, out int fz)) return;
            var (w, dp) = HomeState.Footprint(piece.Definition, selectedRotation);
            grabX = Mathf.Clamp(fx - selectedX, 0, w - 1); grabZ = Mathf.Clamp(fz - selectedZ, 0, dp - 1);
            dragX = selectedX; dragZ = selectedZ;
            birdsDragging = true;
            birdsScroll?.Cancel();
        }
        private void BirdsEyePointerMove(Vector2 position)
        {
            if (birdsInert) { interiorLast = position; return; }
            if (!birdsMoved && (position - interiorStart).sqrMagnitude > Mathf.Pow(Screen.height * 0.012f, 2)) birdsMoved = true;   // the drag threshold
            if (birdsDragging)
            {
                FurniturePiece piece = HomePiece(selectedPiece);
                if (piece != null && FloorCellAt(position, out int fx, out int fz))
                {
                    var (w, dp) = HomeState.Footprint(piece.Definition, selectedRotation);
                    DragCell(fx, fz, grabX, grabZ, w, dp, home.GridWidth, home.GridDepth, out int x, out int z);
                    if (x != dragX || z != dragZ) { dragX = x; dragZ = z; birdsMoved = true; PlaceSelectedView(x, z); UpdateFootprint(); }
                }
                interiorLast = position;
                return;
            }
            if (!birdsScrolling && (position - interiorStart).sqrMagnitude > Mathf.Pow(Screen.height * 0.012f, 2)) { birdsScrolling = true; interiorLast = interiorStart; }
            if (birdsScrolling && !birdsHeld && birdsScroll != null && Screen.height > 0)
            {
                // The floor under the finger follows it: a finger moving up brings the front into view (+offset).
                birdsScroll.Drag((position.y - interiorLast.y) * 2f * birdsOrthoSize / Screen.height, Time.unscaledTime);
                ApplyBirdsEyeCamera();
            }
            interiorLast = position;
        }
        private void BirdsEyePointerEnd(Vector2 position, bool canceled)
        {
            if (!canceled) BirdsEyePointerMove(position);
            bool dragging = birdsDragging, quiet = birdsInert || birdsHeld || dragging;
            // A hold that selected this piece under the same finger never turns it (birdsHeld), whatever its timing.
            BirdsEyeRelease release = canceled ? BirdsEyeRelease.None
                : ClassifyRelease(birdsTouched != null && birdsTouched == selectedPiece && !birdsHeld, birdsInert, birdsMoved, Time.unscaledTime - interiorDownAt);
            interiorPointer = null; interiorDragging = false; interiorCaught = false; interiorHeld = false; interiorPiece = null;
            birdsInert = birdsScrolling = birdsCaught = birdsHeld = birdsMoved = false; birdsTouched = null;
            if (birdsScroll != null) { if (canceled || quiet) birdsScroll.Cancel(); else birdsScroll.End(Time.unscaledTime); }
            if (dragging)
            {
                birdsDragging = false;
                if (canceled || selectedPiece == null) { SnapSelectedBack(); return; }
                FurniturePiece piece = HomePiece(selectedPiece);
                bool turned = piece != null && selectedRotation != piece.Rotation;
                if ((dragX != selectedX || dragZ != selectedZ || turned) && CommitPieceDrop(home, selectedPiece, dragX, dragZ, selectedRotation))
                {
                    selectedX = dragX; selectedZ = dragZ;
                    if (turned) HideSettlementNote();
                    SaveNow();
                }
                SnapSelectedBack();   // onto the committed cell: the new one, or the start when the drop did not fit (a pending turn stays turned)
            }
            if (release == BirdsEyeRelease.Rotate) RotateSelectedPiece();
        }
        // A cancelled touch (a panel, a teardown, Return, Store piece): a drag in progress snaps back; nothing is committed.
        private void CancelBirdsEyeTouch()
        {
            if (birdsDragging) { birdsDragging = false; SnapSelectedBack(); }
            birdsInert = birdsScrolling = birdsCaught = birdsHeld = birdsMoved = false; birdsTouched = null;
            birdsScroll?.Cancel();
        }
    }
}
