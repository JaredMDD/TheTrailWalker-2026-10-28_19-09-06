using System;
using System.IO;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain;
using TheTrailWalker.Input;
using TheTrailWalker.Movement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Disposable presentation for milestone 1. Domain movement and path evaluation are reusable.
    public sealed class MovementPrototype : MonoBehaviour
    {
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private Material trailMaterial;
        [SerializeField] private Material avatarMaterial;
        [SerializeField] private Vector3[] knots = DefaultKnots();
        [SerializeField] private bool collectionEnabled;
        [SerializeField] private bool milestone4Enabled;
        public void EnableCollection() => collectionEnabled = true;
        public void EnableMilestone4() { collectionEnabled = true; milestone4Enabled = true; }
        public static Vector3[] DefaultKnots() => new[] { Vector3.zero, new Vector3(8, 0, 25), new Vector3(-10, 0, 52), new Vector3(12, 0, 82), new Vector3(4, 0, 112), new Vector3(-5, 0, 140) };
        private MovementState state;
        private SampledTrailPath path;
        private TrailMotor motor;
        private CameraOrbitController orbit;
        private GestureRouter gestures;
        private Text status, instructions, harvestLabel, inventoryStatus;
        private GameObject inventory;
        private RectTransform safe;
        private float harvestRemaining, nextHud, nextReport;
        private bool wasAtCamp;
        private int reportSequence;
        private Rect lastSafe;
        private Vector2Int lastSize;

        public void Configure(Material terrain, Material trail, Material avatar)
        { terrainMaterial = terrain; trailMaterial = trail; avatarMaterial = avatar; }

        public void Bind(MovementState movement)
        {
            if (state != null) return;
            state = movement;
            path = new SampledTrailPath(knots);
            CreateWorld(); CreateHud();
            if (collectionEnabled)
            {
                safe.parent.gameObject.SetActive(false);
                gameObject.AddComponent<CollectionPrototype>().Bind(state, motor, path, orbit, gestures, avatarMaterial, milestone4Enabled);
            }
            Debug.Log($"[Milestone1] Ready: {path.Length:F2} m trail; cap {state.SpeedCap:F1} m/s.");
        }

        private GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var item = GameObject.CreatePrimitive(type);
            item.name = name; item.transform.SetParent(transform, false);
            item.transform.position = position; item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }

        private void CreateWorld()
        {
            Primitive("Ground", PrimitiveType.Cube, new Vector3(0, -0.3f, 70), new Vector3(110, 0.5f, 200), terrainMaterial);
            // Ribbon follows the exact distance lookup used by the motor.
            int count = Mathf.CeilToInt(path.Length) + 1;
            var vertices = new Vector3[count * 2]; var triangles = new int[(count - 1) * 6];
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < count; i++)
            {
                float distance = path.Length * i / (count - 1);
                path.Evaluate(distance, out Vector3 p, out Vector3 t);
                Vector3 side = Vector3.Cross(Vector3.up, t) * 1.7f;
                vertices[i * 2] = p - side; vertices[i * 2 + 1] = p + side;
                uv[i * 2] = new Vector2(0, distance); uv[i * 2 + 1] = new Vector2(1, distance);
                if (i == count - 1) continue;
                int v = i * 2, k = i * 6;
                triangles[k] = v; triangles[k + 1] = v + 2; triangles[k + 2] = v + 1;
                triangles[k + 3] = v + 1; triangles[k + 4] = v + 2; triangles[k + 5] = v + 3;
            }
            var ribbon = new GameObject("Curved trail", typeof(MeshFilter), typeof(MeshRenderer));
            ribbon.transform.SetParent(transform, false);
            var mesh = new Mesh { name = "Arc-length trail ribbon", vertices = vertices, triangles = triangles, uv = uv };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            ribbon.GetComponent<MeshFilter>().sharedMesh = mesh;
            ribbon.GetComponent<MeshRenderer>().sharedMaterial = trailMaterial;
            for (int i = 10; i < path.Length; i += 10)
            {
                path.Evaluate(i, out Vector3 p, out Vector3 t);
                Vector3 side = Vector3.Cross(Vector3.up, t);
                Primitive("Trail marker " + i + " m", PrimitiveType.Cube, p + side * 2.2f + Vector3.up * 0.4f, new Vector3(0.3f, 0.8f, 0.3f), avatarMaterial);
                // Low silhouettes provide readable parallax without camera obstructions.
                Primitive("Trail scenery", PrimitiveType.Sphere, p - side * 5f + Vector3.up * 0.4f, new Vector3(2f, 1f, 2f), terrainMaterial);
            }
            path.Evaluate(path.Length, out Vector3 camp, out _);
            Primitive("End camp anchor", PrimitiveType.Cylinder, camp + Vector3.down * 0.02f, new Vector3(7, 0.08f, 7), avatarMaterial);
            Primitive("Camp beacon", PrimitiveType.Cube, camp + new Vector3(3, 2, 0), new Vector3(0.4f, 4, 0.4f), trailMaterial);

            var player = new GameObject("Trail walker"); player.transform.SetParent(transform, false);
            GameObject body = Primitive("Avatar speed gesture target", PrimitiveType.Capsule, Vector3.up, Vector3.one, avatarMaterial);
            body.transform.SetParent(player.transform, false);
            // A nose indicates path facing independently of camera yaw.
            GameObject nose = Primitive("Forward marker", PrimitiveType.Cube, new Vector3(0, 1.4f, 0.5f), Vector3.one * 0.25f, trailMaterial);
            nose.transform.SetParent(player.transform, false);
            body.GetComponent<Renderer>().enabled = false;
            body.GetComponent<Collider>().enabled = false;
            nose.GetComponent<Renderer>().enabled = false;
            nose.GetComponent<Collider>().enabled = false;
            motor = player.AddComponent<TrailMotor>(); motor.Bind(state, path);

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera"; cameraObject.transform.SetParent(transform, false);
            Camera camera = cameraObject.GetComponent<Camera>(); camera.fieldOfView = 52; camera.farClipPlane = 200;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.44f, 0.64f, 0.7f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            orbit = cameraObject.AddComponent<CameraOrbitController>();
            var rig = cameraObject.AddComponent<FollowCameraRig>(); rig.Target = player.transform; rig.Orbit = orbit;
            var lightObject = new GameObject("Sun", typeof(Light)); lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(45, -30, 0);
            Light light = lightObject.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.1f; light.shadows = LightShadows.Hard;
            RenderSettings.sun = light; RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = Color.gray;
            gestures = gameObject.AddComponent<GestureRouter>(); gestures.Bind(state, camera, body.GetComponent<Collider>(), orbit);
        }

        private void CreateHud()
        {
            var canvas = new GameObject("Movement HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.transform.SetParent(transform, false); canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720, 1280); scaler.matchWidthOrHeight = 0.5f;
            safe = Rect("Safe area", canvas.transform, 0, 0, 1, 1);
            Label(safe, "SPEED\n↑ ↓", 0.40f, 0.36f, 0.60f, 0.46f, 22);
            Label(safe, "THE TRAIL WALKER  /  MOVEMENT", 0.03f, 0.93f, 0.97f, 0.985f, 28);
            status = Label(safe, "", 0.04f, 0.78f, 0.96f, 0.93f, 25);
            instructions = Label(safe, "Drag UP / DOWN on the walker to change speed.\nDrag LEFT / RIGHT to look around.", 0.04f, 0.22f, 0.96f, 0.32f, 26);
            Button(safe, "INVENTORY", 0.04f, 0.135f, 0.48f, 0.205f, OpenInventory);
            harvestLabel = Button(safe, "TEST HARVEST", 0.52f, 0.135f, 0.96f, 0.205f, Harvest);
            Button(safe, "RESTART TRAIL", 0.04f, 0.045f, 0.48f, 0.115f, Restart);
            Button(safe, "CAP: 0.8 / 2.0", 0.52f, 0.045f, 0.96f, 0.115f, () => state.SetSpeedCap(state.SpeedCap < 1 ? MovementState.MaximumSpeedCap : 0.8f));
            inventory = Rect("Inventory placeholder", safe, 0, 0, 1, 1).gameObject;
            inventory.AddComponent<Image>().color = new Color(0.06f, 0.11f, 0.13f, 0.98f);
            Label(inventory.transform, "BACKPACK\nMovement is paused", 0.07f, 0.66f, 0.93f, 0.85f, 36);
            inventoryStatus = Label(inventory.transform, "", 0.08f, 0.4f, 0.92f, 0.65f, 28);
            Button(inventory.transform, "CLOSE BACKPACK", 0.12f, 0.15f, 0.88f, 0.24f, CloseInventory);
            inventory.SetActive(false);
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false); events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            ApplySafeArea();
        }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        private static Text Label(Transform parent, string value, float x0, float y0, float x1, float y1, int size)
        {
            Text text = Rect("Label", parent, x0, y0, x1, y1).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.text = value;
            text.alignment = TextAnchor.MiddleCenter; text.color = new Color(0.96f, 0.95f, 0.86f); text.raycastTarget = false; return text;
        }
        private static Text Button(Transform parent, string value, float x0, float y0, float x1, float y1, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(value, parent, x0, y0, x1, y1); var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.13f, 0.28f, 0.25f); var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image; button.onClick.AddListener(action);
            return Label(rect, value, 0.02f, 0.04f, 0.98f, 0.96f, 25);
        }
        private void OpenInventory() { gestures.CancelAll(); state.OpenInventory(); inventory.SetActive(true); WriteReport("inventory-open"); }
        private void CloseInventory() { state.CloseInventory(); inventory.SetActive(false); WriteReport("inventory-close"); }
        private void Harvest()
        {
            gestures.CancelAll();
            if ((state.Pauses & MovementPauseReasons.Harvest) != 0) { harvestRemaining = 0; state.EndHarvest(); }
            else { state.BeginHarvest(); harvestRemaining = 2; }
            WriteReport("harvest");
        }
        private void Restart()
        {
            gestures.CancelAll(); state.Reset(); harvestRemaining = 0; inventory.SetActive(false); motor.Present(); wasAtCamp = false;
            WriteReport("restart");
        }
        private void Update()
        {
            if (state == null) return;
            if (lastSafe != Screen.safeArea || lastSize != new Vector2Int(Screen.width, Screen.height)) ApplySafeArea();
            if (harvestRemaining > 0 && (state.Pauses & (MovementPauseReasons.Suspended | MovementPauseReasons.Unfocused)) == 0)
            {
                harvestRemaining -= Time.deltaTime;
                if (harvestRemaining <= 0) { state.EndHarvest(); gestures.CancelAll(); WriteReport("harvest-finished"); }
            }
            bool atCamp = (state.Pauses & MovementPauseReasons.Camp) != 0;
            if (atCamp && !wasAtCamp) { wasAtCamp = true; gestures.CancelAll(); WriteReport("camp-arrival"); }
            if (Time.unscaledTime >= nextHud)
            {
                nextHud = Time.unscaledTime + 0.1f;
                status.text = $"{state.Distance:F1} / {path.Length:F1} m   |   Camera {orbit.Yaw:F0}°\nSpeed {state.ActualSpeed:F2}   Requested {state.RequestedSpeed:F2}   Cap {state.SpeedCap:F1} m/s\nPause: {state.Pauses}\n{(state.PostHarvestStop ? "Harvest stop: swipe UP to resume" : atCamp ? "Camp reached. Trail complete." : "Forward travel only")}";
                harvestLabel.text = harvestRemaining > 0 ? "CANCEL HARVEST" : "TEST HARVEST";
                inventoryStatus.text = $"Saved speed: {state.InventoryResumeSpeed:F2} m/s\nCurrent cap: {state.SpeedCap:F1} m/s\n\nClosing restores your saved speed only when other stops permit it.\n\nInventory items arrive in Milestone 2.";
                instructions.text = atCamp ? "Camp reached. Use RESTART TRAIL to try again." : "Drag UP / DOWN on the centre line to change speed.\nDrag LEFT / RIGHT to look around.";
            }
            if (Debug.isDebugBuild && Time.unscaledTime >= nextReport) { nextReport = Time.unscaledTime + 5; WriteReport("sample"); }
        }
        private void ApplySafeArea()
        {
            lastSafe = Screen.safeArea; lastSize = new Vector2Int(Screen.width, Screen.height);
            if (lastSize.x <= 0 || lastSize.y <= 0) return;
            safe.anchorMin = lastSafe.min / new Vector2(Screen.width, Screen.height);
            safe.anchorMax = lastSafe.max / new Vector2(Screen.width, Screen.height);
        }
        private void OnApplicationPause(bool paused) { if (state != null) WriteReport(paused ? "suspended" : "resumed"); }
        private void WriteReport(string reason)
        {
            if (!Debug.isDebugBuild) return;
            var report = new Report { sequence = ++reportSequence, utc = DateTime.UtcNow.ToString("O"), reason = reason,
                distance = state.Distance, pathLength = path.Length, speed = state.ActualSpeed, requested = state.RequestedSpeed,
                cap = state.SpeedCap, yaw = orbit.Yaw, pauses = state.Pauses.ToString(), postHarvestStop = state.PostHarvestStop,
                inventoryResume = state.InventoryResumeSpeed, orbitGestures = gestures.OrbitGestures, speedGestures = gestures.SpeedGestures };
            try { File.WriteAllText(Path.Combine(Application.persistentDataPath, "milestone1.json"), JsonUtility.ToJson(report, true)); }
            catch (IOException error) { Debug.LogWarning("[Milestone1] Could not write diagnostics: " + error.Message); }
        }
        [Serializable] private sealed class Report
        {
            public string utc, reason, pauses;
            public int sequence, orbitGestures, speedGestures;
            public float distance, pathLength, speed, requested, cap, yaw, inventoryResume;
            public bool postHarvestStop;
        }
    }
}
