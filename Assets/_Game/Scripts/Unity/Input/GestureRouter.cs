using System.Collections.Generic;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TheTrailWalker.Input
{
    public sealed class GestureRouter : MonoBehaviour
    {
        private sealed class Contact
        {
            public Vector2 Start, Previous;
            public bool Avatar;
            public GestureOwner Owner;
            public bool Resource;
        }
        private readonly Dictionary<int, Contact> contacts = new Dictionary<int, Contact>(10);
        private readonly List<RaycastResult> hits = new List<RaycastResult>(16);
        private PointerEventData pointer;
        private EventSystem pointerSystem;
        private MovementState movement;
        private Camera view;
        private Collider avatar;
        private CameraOrbitController orbit;
        public IResourceGestures Resources { get; set; }
        public bool InteractionLocked { get; set; }
        public int OrbitGestures { get; private set; }
        public int SpeedGestures { get; private set; }
        // Raised when a SPEED-area swipe is ignored, so the UI can explain why walking did not change.
        // The float is the vertical drag that resolved the gesture (positive = upward), or 0 when only a touch began.
        public event System.Action<MovementPauseReasons, float> SpeedBlocked;
        // Unscaled time of the last accepted speed input, so the HUD can show the gauge while it is being adjusted.
        public float LastSpeedInput { get; private set; } = float.NegativeInfinity;
        public bool AdjustingSpeed
        {
            get { foreach (Contact contact in contacts.Values) if (contact.Owner == GestureOwner.Speed) return true; return false; }
        }
        public void Bind(MovementState state, Camera camera, Collider avatarCollider, CameraOrbitController cameraOrbit)
        { movement = state; view = camera; avatar = avatarCollider; orbit = cameraOrbit; }

        private void OnEnable() => EnhancedTouchSupport.Enable();
        private void OnDisable() { contacts.Clear(); EnhancedTouchSupport.Disable(); }
        public void CancelAll() { contacts.Clear(); Resources?.CancelAll(); }
        private void OnApplicationPause(bool paused) { if (paused) CancelAll(); }
        private void OnApplicationFocus(bool focused) { if (!focused) CancelAll(); }

        private bool Blocked => InteractionLocked || movement.InventoryDepth > 0 || (movement.Pauses & (MovementPauseReasons.Loading | MovementPauseReasons.Suspended | MovementPauseReasons.Unfocused)) != 0;

        private void Update()
        {
            if (movement == null) return;
            if (Blocked)
            {
                if (contacts.Count > 0) CancelAll();
                foreach (Touch touch in Touch.activeTouches)
                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began && OverAvatar(touch.screenPosition) && !OverUi(touch.screenPosition)) SpeedBlocked?.Invoke(movement.Pauses, 0f);
                return;
            }
            foreach (Touch touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began) Begin(touch.touchId, touch.screenPosition);
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended) End(touch.touchId, touch.screenPosition, false);
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled) End(touch.touchId, touch.screenPosition, true);
                else Move(touch.touchId, touch.screenPosition);
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            Mouse mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 position = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame) Begin(-1, position);
            if (mouse.leftButton.isPressed) Move(-1, position);
            if (mouse.leftButton.wasReleasedThisFrame) End(-1, position, false);
            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0f && !OverUi(position) && OverAvatar(position)) { movement.SpeedGesture(Mathf.Sign(scroll) * 0.2f); LastSpeedInput = Time.unscaledTime; }
#endif
        }

        private bool OverUi(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            if (pointer == null || pointerSystem != EventSystem.current) { pointerSystem = EventSystem.current; pointer = new PointerEventData(pointerSystem); }
            pointer.position = position;
            hits.Clear();
            EventSystem.current.RaycastAll(pointer, hits);
            return hits.Count > 0;
        }

        private bool OverAvatar(Vector2 position)
        {
            return IsSpeedArea(position, Screen.safeArea);
        }
        // A narrow vertical centre line between the closed bag and the debug buttons.
        public static bool IsSpeedArea(Vector2 position, Rect safe) =>
            position.x >= safe.xMin + safe.width * 0.40f && position.x <= safe.xMin + safe.width * 0.60f &&
            position.y >= safe.yMin + safe.height * 0.17f && position.y <= safe.yMin + safe.height * 0.54f;
        public static bool IsSystemGestureArea(float y, float screenHeight, Rect safe) =>
            y > Mathf.Min(safe.yMax, screenHeight * 0.92f);

        private void Begin(int id, Vector2 position)
        {
            // Leave the system shade edge to Android even on devices reporting a full-screen safe area.
            if (IsSystemGestureArea(position.y, Screen.height, Screen.safeArea)) return;
            bool ui = OverUi(position);
            bool speedSurface = !ui && OverAvatar(position);
            bool resource = !ui && !speedSurface && Resources != null && Resources.Begin(id, position);
            contacts[id] = new Contact { Start = position, Previous = position, Avatar = speedSurface, Resource = resource, Owner = ui || resource ? GestureOwner.Blocked : GestureOwner.Pending };
        }

        private void End(int id, Vector2 position, bool canceled)
        {
            if (contacts.TryGetValue(id, out Contact contact) && contact.Resource) Resources?.End(id, position, canceled);
            else if (!canceled) Move(id, position);
            contacts.Remove(id);
        }

        private void Move(int id, Vector2 position)
        {
            if (!contacts.TryGetValue(id, out Contact contact)) return;
            if (contact.Resource) { Resources?.Move(id, position); return; }
            Vector2 delta = position - contact.Previous;
            contact.Previous = position;
            if (contact.Owner == GestureOwner.Pending)
            {
                Vector2 total = position - contact.Start;
                float dpi = Screen.dpi > 0f ? Screen.dpi : 160f;
                float threshold = Mathf.Clamp(10f * dpi / 160f, Screen.height * 0.008f, Screen.height * 0.025f);
                GestureOwner resolved = GestureOwnership.Resolve(contact.Avatar, total.x, total.y, threshold);
                if (resolved == GestureOwner.Pending) return;
                // One contact per action; a separate speed finger and orbit finger can coexist.
                foreach (Contact other in contacts.Values)
                    if (other != contact && other.Owner == resolved) { resolved = GestureOwner.Blocked; break; }
                contact.Owner = resolved;
                delta = total;
                if (resolved == GestureOwner.Orbit) OrbitGestures++;
                if (resolved == GestureOwner.Speed) SpeedGestures++;
                if (resolved == GestureOwner.Speed && movement.Pauses != MovementPauseReasons.None) SpeedBlocked?.Invoke(movement.Pauses, total.y);
            }
            if (contact.Owner == GestureOwner.Orbit) orbit.Rotate(delta.x * 360f / Screen.width);
            if (contact.Owner == GestureOwner.Speed) { movement.SpeedGesture(delta.y * 4f / Screen.height); LastSpeedInput = Time.unscaledTime; }
        }
    }
}
