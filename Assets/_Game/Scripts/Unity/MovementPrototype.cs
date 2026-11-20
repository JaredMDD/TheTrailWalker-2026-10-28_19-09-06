using System;
using System.Collections.Generic;
using System.IO;
using TheTrailWalker.CameraControl;
using TheTrailWalker.Domain;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Input;
using TheTrailWalker.Movement;
using TheTrailWalker.Travel;
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
        [SerializeField] private bool milestone5Enabled;
        public void EnableCollection() => collectionEnabled = true;
        public void EnableMilestone4() { collectionEnabled = true; milestone4Enabled = true; }
        public void EnableMilestone5() { collectionEnabled = true; milestone4Enabled = true; milestone5Enabled = true; }
        private TravelSceneInfo travelScene;
        public static Vector3[] DefaultKnots() => new[] { Vector3.zero, new Vector3(8, 0, 25), new Vector3(-10, 0, 52), new Vector3(12, 0, 82), new Vector3(4, 0, 112), new Vector3(-5, 0, 140) };
        private MovementState state;
        private SampledTrailPath path;
        // Height of the drawn terrain (camp world only; null in the scenes without a styled segment).
        public TerrainHeightField Ground { get; private set; }
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

        public void Bind(MovementState movement, Action requestReload = null)
        {
            if (state != null) return;
            state = movement;
            // Benchmark mode shows a fixed, representative Dawn Plains leg and never loads or writes the session save.
            bool benchmark = milestone5Enabled && File.Exists(PerformanceBenchmark.TriggerPath);
            if (milestone5Enabled)
            {
                // The saved camp decides which segment this scene presents; the road starts at that camp.
                if (benchmark)
                {
                    TravelCatalog catalog = TravelCatalog.Prototype();
                    travelScene = new TravelSceneInfo { Catalog = catalog, Segment = PerformanceBenchmark.ChosenSegment(catalog) };
                    PerformanceBenchmark.MeasuredSegment = travelScene.Segment.Id;
                }
                else travelScene = CollectionPrototype.ResolveTravelScene();
                travelScene.RequestReload = requestReload;
                knots = TravelScenery.KnotsFor(travelScene.Segment);
            }
            path = new SampledTrailPath(knots);
            CreateWorld(); CreateHud();
            if (benchmark)
            {
                safe.parent.gameObject.SetActive(false);
                gameObject.AddComponent<PerformanceBenchmark>().Bind(orbit, motor, state);
                return;
            }
            if (collectionEnabled)
            {
                safe.parent.gameObject.SetActive(false);
                gameObject.AddComponent<CollectionPrototype>().Bind(state, motor, path, orbit, gestures, avatarMaterial, milestone4Enabled, travelScene);
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

        // Segment dressing: each leg's regional environment kit (ground cover, shrubs, decorative pines, rocks) placed on the
        // actual ground, clear of the road, camps and interactables; every region keeps the meadow ground and dirt trail for now.
        private void DressSegment(RegionDefinition region, IReadOnlyList<InteractableAnchor> anchors, IReadOnlyList<InteractableAnchor> nextAnchors, IReadOnlyList<InteractableAnchor> previousAnchors)
        {
            Transform terrain = transform.Find("Terrain");
            // Every region uses the palette-styled ground (built with the terrain) and the dirt trail for now.
            Material dirt = Region01Environment.TrailMaterial;
            // Both road ribbons, camp clearings and their footpaths are trodden earth.
            if (dirt != null) foreach (Transform piece in GetComponentsInChildren<Transform>())
                if (piece.name.StartsWith("Curved trail") || piece.name == "Clearing" || piece.name == "Footpath") piece.GetComponent<MeshRenderer>().sharedMaterial = dirt;
            if (terrain == null) return;
            // Kept for the trail pickups, which are placed after this and rest on the drawn ground (no collider needed).
            Ground = new TerrainHeightField(terrain.GetComponent<MeshFilter>());
            var ground = terrain.gameObject.AddComponent<MeshCollider>();
            ground.sharedMesh = terrain.GetComponent<MeshFilter>().sharedMesh;
            Func<Vector3, float> height = position =>
                ground.Raycast(new Ray(position + Vector3.up * 100f, Vector3.down), out RaycastHit hit, 300f) ? hit.point.y : position.y - 0.12f;
            // Interactables were placed at road height; rest each on the terrain under it (feet on the ground for animals,
            // whose rig marks a ground anchor), on every shown leg. Fixes rabbits floating above the ground.
            foreach (Harvest.Tier1HarvestVisual visual in GetComponentsInChildren<Harvest.Tier1HarvestVisual>(true))
            {
                var rig = visual.GetComponent<Harvest.Tier1RabbitRig>();
                float feet = rig != null && rig.GroundAnchor != null ? rig.GroundAnchor.position.y : visual.transform.position.y;
                // Seat on the lowest ground under the footprint (user, November 2, 2026: on hill slopes the downhill side
                // of a trunk base or a rabbit's feet hung in the air when only the centre touched the ground).
                Vector3 at = visual.transform.position; float radius = rig != null ? 0.2f : 0.45f, lowest = height(at);
                for (int a = 0; a < 8; a++)
                    lowest = Mathf.Min(lowest, height(at + Quaternion.AngleAxis(a * 45f, Vector3.up) * Vector3.forward * radius));
                visual.transform.position += Vector3.up * (lowest - feet);
            }
            // Roadside and field scenery along all shown legs, each from its own region's tier-one kit, clear of camps and interactables.
            var roads = new List<ITrailPath>(); var regions = new List<int>(); var anchorPoints = new List<Vector3>(); var campPoints = new List<Vector3>();
            void Add(ITrailPath road, SegmentDefinition leg, IReadOnlyList<InteractableAnchor> legAnchors)
            {
                if (road == null) return;
                roads.Add(road); regions.Add(travelScene.Catalog.RegionOf(leg).Number);
                if (legAnchors != null) foreach (InteractableAnchor anchor in legAnchors)
                {
                    road.Evaluate(Mathf.Clamp(anchor.Distance, 0, road.Length), out Vector3 p, out Vector3 t);
                    anchorPoints.Add(p + Vector3.Cross(Vector3.up, t).normalized * anchor.Lateral);
                }
            }
            Add(previousPath, previousSegment, previousAnchors); Add(path, travelScene.Segment, anchors); Add(nextPath, nextSegment, nextAnchors);
            if (travelScene.StartCamp != null) campPoints.Add(travelScene.StartCamp.LookTarget);
            if (travelScene.EndCamp != null) campPoints.Add(travelScene.EndCamp.LookTarget);
            if (archPoint.HasValue) campPoints.Add(archPoint.Value);   // keep scatter off the arch and its road end
            SceneryScatter.Build(transform, roads, regions, campPoints, anchorPoints, height, travelScene.Segment.Id, Look);
            Destroy(ground); // only for placement; pickups and touches never hit the ground
        }

        // Road ribbon that follows the exact distance lookup used by the motor.
        private void BuildRibbon(string name, ITrailPath trail)
        {
            int count = Mathf.CeilToInt(trail.Length) + 1;
            var vertices = new Vector3[count * 2]; var triangles = new int[(count - 1) * 6];
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < count; i++)
            {
                float distance = trail.Length * i / (count - 1);
                trail.Evaluate(distance, out Vector3 p, out Vector3 t);
                Vector3 side = Vector3.Cross(Vector3.up, t) * 1.7f;
                vertices[i * 2] = p - side; vertices[i * 2 + 1] = p + side;
                uv[i * 2] = new Vector2(0, distance); uv[i * 2 + 1] = new Vector2(1, distance);
                if (i == count - 1) continue;
                int v = i * 2, k = i * 6;
                triangles[k] = v; triangles[k + 1] = v + 2; triangles[k + 2] = v + 1;
                triangles[k + 3] = v + 1; triangles[k + 4] = v + 2; triangles[k + 5] = v + 3;
            }
            var ribbon = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            ribbon.transform.SetParent(transform, false);
            var mesh = new Mesh { name = "Arc-length trail ribbon", vertices = vertices, triangles = triangles, uv = uv };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            ribbon.GetComponent<MeshFilter>().sharedMesh = mesh;
            ribbon.GetComponent<MeshRenderer>().sharedMaterial = trailMaterial;
        }

        // The leg after the end camp, shown beyond it so the road visibly continues on arrival (no drops; visual only).
        private SegmentDefinition nextSegment;
        private ITrailPath nextPath;
        // The leg already walked into the start camp, shown behind it so the view back is not an empty expanse. Its
        // resources are not repopulated (they roll only when the player returns to that leg's camp).
        private SegmentDefinition previousSegment;
        private Vector3? archPoint;
        private ITrailPath previousPath;

        // The walked leg's region look (fog, light, valley); the settlement approach drops Fern Hollow's valley.
        private RegionLook Look => milestone5Enabled && travelScene != null && travelScene.Settlement ? RegionLooks.Settlement   // Fern Hollow's light with a far haze toward region 5
            : milestone5Enabled && travelScene?.Segment != null
            ? RegionLooks.For(travelScene.Catalog.RegionOf(travelScene.Segment).Number, travelScene.Segment.SettlementApproach) : RegionLooks.Default;

        private static Vector3 Heading(Vector3 tangent)
        {
            var flat = new Vector3(tangent.x, 0, tangent.z);
            return flat.sqrMagnitude > 1e-6f ? flat.normalized : Vector3.forward;
        }

        // Settlement mode (spec 2026-11-08, section 2): the street replaces road, camps and interactables; no pickups.
        public SettlementStreet Street { get; private set; }
        public StreetCamera StreetView { get; private set; }

        // Debug house-level preview: rebuilds the street with other house levels, keeping the camera where it is. View only;
        // the save is untouched.
        public void RebuildStreet(int[] houseLevels, int playerPlot, int shopTier)
        {
            if (Street == null) return;
            if (Street.Root != null) Destroy(Street.Root.gameObject);
            Street = SettlementScenery.Build(transform, houseLevels, playerPlot, shopTier);
            StreetView?.Rebind(Street);
        }

        // First home (task 6): the interior of the player's house, built inside the settlement scene but far below the street
        // (world y = -200), so the street stays built and nothing of it shows in the interior camera. One interior at a time:
        // building again replaces the previous one. The shell's own colliders (none today) are stripped: only the furniture
        // touch boxes (CollectionPrototype.InstancePieces) are hit by interior taps.
        public const string InteriorHolderName = "Home interior";
        public static readonly Vector3 InteriorWorldPosition = new Vector3(0f, -200f, 0f);
        public Transform BuildInterior(int level)
        {
            ClearInterior();
            GameObject prefab = Resources.Load<GameObject>($"Settlement/Houses/HouseL{level:00}Interior");
            if (prefab == null) { Debug.LogWarning($"[Settlement] HouseL{level:00}Interior is missing."); return null; }
            var holder = new GameObject(InteriorHolderName).transform;
            holder.SetParent(transform, false);
            holder.SetPositionAndRotation(InteriorWorldPosition, Quaternion.identity);
            GameObject interior = Instantiate(prefab, holder, false);
            interior.name = prefab.name;
            foreach (Collider c in interior.GetComponentsInChildren<Collider>(true)) DestroyImmediate(c);
            return interior.transform;
        }
        public void ClearInterior()
        {
            // Immediate, so a rebuild in the same frame (and the Editor checks) never sees the old holder.
            for (int i = transform.childCount - 1; i >= 0; i--)
                if (transform.GetChild(i).name == InteriorHolderName) DestroyImmediate(transform.GetChild(i).gameObject);
        }

        private void CreateWorld()
        {
            bool settlement = milestone5Enabled && travelScene.Settlement;
            if (settlement) Street = SettlementScenery.Build(transform, travelScene.HouseLevels, travelScene.PlayerPlot, travelScene.ShopTier);
            else if (milestone5Enabled)
            {
                ITrailPath ground = path;
                nextSegment = travelScene.Segment.RiverApproach ? null : travelScene.Catalog.Outgoing(travelScene.Segment.ToCampId);
                if (nextSegment != null)
                {
                    path.Evaluate(path.Length, out Vector3 end, out Vector3 tangent);
                    nextPath = new PlacedTrailPath(new SampledTrailPath(TravelScenery.KnotsFor(nextSegment)), Quaternion.LookRotation(Heading(tangent)), end);
                    ground = new JoinedTrailPath(path, nextPath);
                }
                // A landing already starts at the river; other legs show the leg that arrived at their start camp.
                previousSegment = travelScene.Segment.StartsAtRiver ? null : travelScene.Catalog.Incoming(travelScene.Segment.FromCampId);
                if (previousSegment != null)
                {
                    // Turn and move the previous leg so it ends exactly where this road starts, heading the same way.
                    var own = new SampledTrailPath(TravelScenery.KnotsFor(previousSegment));
                    own.Evaluate(own.Length, out Vector3 ownEnd, out Vector3 ownTangent);
                    path.Evaluate(0, out Vector3 start, out Vector3 startTangent);
                    Quaternion turn = Quaternion.FromToRotation(Heading(ownTangent), Heading(startTangent));
                    previousPath = new PlacedTrailPath(own, turn, start - turn * ownEnd);
                    ground = new JoinedTrailPath(previousPath, ground);
                }
                Material palette = Region01Environment.GroundMaterial;
                // Level the ground under this leg's camp clearings (no clearing at a ferry landing or the river end).
                var clearings = new List<Vector3>();
                if (!travelScene.Segment.StartsAtRiver) clearings.Add(TravelScenery.ClearingCentre(path, 0, travelScene.Catalog.Camp(travelScene.Segment.FromCampId)));
                if (travelScene.Segment.ToCampId != null) clearings.Add(TravelScenery.ClearingCentre(path, path.Length, travelScene.Catalog.Camp(travelScene.Segment.ToCampId)));
                // Level the ground under every shown leg's fishing ponds too (the same anchors BuildInteractables places below).
                var ponds = new List<Vector3>();
                TravelScenery.AddPonds(ponds, path, SegmentLayout.Build(travelScene.Catalog.RegionOf(travelScene.Segment), travelScene.Segment));
                if (nextSegment != null) TravelScenery.AddPonds(ponds, nextPath, SegmentLayout.Build(travelScene.Catalog.RegionOf(nextSegment), nextSegment));
                if (previousSegment != null) TravelScenery.AddPonds(ponds, previousPath, SegmentLayout.Build(travelScene.Catalog.RegionOf(previousSegment), previousSegment));
                // Every river shown in this scene gets a channel in the terrain (the same centres BuildRiver uses).
                var rivers = new List<TravelScenery.RiverCut>();
                if (travelScene.Segment.RiverApproach) rivers.Add(TravelScenery.RiverCutFor(path, false));
                if (travelScene.Segment.StartsAtRiver) rivers.Add(TravelScenery.RiverCutFor(path, true));
                if (nextSegment != null && nextSegment.RiverApproach) rivers.Add(TravelScenery.RiverCutFor(nextPath, false));
                if (previousSegment != null && previousSegment.Landing) rivers.Add(TravelScenery.RiverCutFor(previousPath, true));
                TravelScenery.BuildTerrain(transform, ground, palette != null ? palette : terrainMaterial, 70f, 3f, TravelScenery.Undulation(travelScene.Segment), palette != null,
                    clearings.ToArray(), ponds.ToArray(), Look, rivers.ToArray());
            }
            else Primitive("Ground", PrimitiveType.Cube, new Vector3(0, -0.3f, 70), new Vector3(110, 0.5f, 200), terrainMaterial);
            if (!settlement) BuildRibbon("Curved trail", path);
            if (nextPath != null) BuildRibbon("Curved trail (next leg)", nextPath);
            if (previousPath != null) BuildRibbon("Curved trail (previous leg)", previousPath);
            // Camp segments are dressed with the environment kit instead (DressSegment); markers remain for older prototypes.
            for (int i = 10; i < path.Length && !milestone5Enabled; i += 10)
            {
                path.Evaluate(i, out Vector3 p, out Vector3 t);
                Vector3 side = Vector3.Cross(Vector3.up, t);
                Primitive("Trail marker " + i + " m", PrimitiveType.Cube, p + side * 2.2f + Vector3.up * 0.4f, new Vector3(0.3f, 0.8f, 0.3f), avatarMaterial);
                // Low silhouettes provide readable parallax without camera obstructions.
                Primitive("Trail scenery", PrimitiveType.Sphere, p - side * 5f + Vector3.up * 0.4f, new Vector3(2f, 1f, 2f), terrainMaterial);
            }
            if (settlement) { /* the street was built above: no camps, interactables or scatter */ }
            else if (milestone5Enabled)
            {
                // Camps sit in clearings beside the road: the one this segment leaves from, and the next one (or the river ferry).
                TravelCatalog catalog = travelScene.Catalog;
                RegionDefinition region = catalog.RegionOf(travelScene.Segment);
                // Trader looks need learned wearable recipes, which arrive with Milestone 7; until then every set is tier 0.
                // Learned wearable recipes decide which outfits traders can wear (from the saved crafting state).
                HashSet<string> knownRecipes = CollectionPrototype.KnownRecipesFromSave();
                string fromCamp = travelScene.Segment.FromCampId, toCamp = travelScene.Segment.ToCampId;
                // A ferry landing starts on the far riverbank instead of at a camp.
                if (travelScene.Segment.StartsAtRiver) TravelScenery.BuildRiver(transform, path, terrainMaterial, trailMaterial, avatarMaterial, landing: true);
                else travelScene.StartCamp = TravelScenery.BuildCamp(transform, catalog.Camp(fromCamp), path, 0, terrainMaterial, trailMaterial, avatarMaterial,
                    region.Number, CampNpcs.Roll(fromCamp, travelScene.StartVisit, knownRecipes));
                if (travelScene.Segment.RiverApproach) TravelScenery.BuildRiver(transform, path, terrainMaterial, trailMaterial, avatarMaterial);
                // The settlement approach ends at the entrance arch instead of a camp (crossing it: CollectionPrototype.Travel).
                else if (travelScene.Segment.SettlementApproach) archPoint = TravelScenery.BuildSettlementApproach(transform, path, trailMaterial);
                else travelScene.EndCamp = TravelScenery.BuildCamp(transform, catalog.Camp(toCamp), path, path.Length, terrainMaterial, trailMaterial, avatarMaterial,
                    region.Number, CampNpcs.Roll(toCamp, travelScene.StartVisit + 1, knownRecipes));
                var anchors = SegmentLayout.Build(region, travelScene.Segment);
                TravelScenery.BuildInteractables(transform, path, anchors, trailMaterial, avatarMaterial, terrainMaterial, SegmentLayout.Tier(region));
                IReadOnlyList<InteractableAnchor> nextAnchors = null;
                if (nextSegment != null)
                {
                    // Preview of the next leg: its trees and animals (visual only) and, for a river leg, the river.
                    var preview = new GameObject("Next leg preview").transform; preview.SetParent(transform, false);
                    RegionDefinition nextRegion = catalog.RegionOf(nextSegment);
                    nextAnchors = SegmentLayout.Build(nextRegion, nextSegment);
                    TravelScenery.BuildInteractables(preview, nextPath, nextAnchors, trailMaterial, avatarMaterial, terrainMaterial, SegmentLayout.Tier(nextRegion));
                    if (nextSegment.RiverApproach) TravelScenery.BuildRiver(preview, nextPath, terrainMaterial, trailMaterial, avatarMaterial);
                }
                IReadOnlyList<InteractableAnchor> previousAnchors = null;
                if (previousSegment != null)
                {
                    // The walked leg behind the start camp: trees and animals (visual only) and, for a landing, the river it began at.
                    var behind = new GameObject("Previous leg").transform; behind.SetParent(transform, false);
                    RegionDefinition previousRegion = catalog.RegionOf(previousSegment);
                    previousAnchors = SegmentLayout.Build(previousRegion, previousSegment);
                    TravelScenery.BuildInteractables(behind, previousPath, previousAnchors, trailMaterial, avatarMaterial, terrainMaterial, SegmentLayout.Tier(previousRegion));
                    if (previousSegment.Landing) TravelScenery.BuildRiver(behind, previousPath, terrainMaterial, trailMaterial, avatarMaterial, landing: true);
                }
                DressSegment(region, anchors, nextAnchors, previousAnchors);
            }
            else
            {
                path.Evaluate(path.Length, out Vector3 camp, out _);
                Primitive("End camp anchor", PrimitiveType.Cylinder, camp + Vector3.down * 0.02f, new Vector3(7, 0.08f, 7), avatarMaterial);
                Primitive("Camp beacon", PrimitiveType.Cube, camp + new Vector3(3, 2, 0), new Vector3(0.4f, 4, 0.4f), trailMaterial);
            }

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
            if (settlement) motor.Detached = true;   // nobody walks the street: the camera scrolls along it

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera"; cameraObject.transform.SetParent(transform, false);
            Camera camera = cameraObject.GetComponent<Camera>(); camera.fieldOfView = 52; camera.farClipPlane = 320;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.44f, 0.64f, 0.7f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            orbit = cameraObject.AddComponent<CameraOrbitController>();
            if (settlement) { StreetView = cameraObject.AddComponent<StreetCamera>(); StreetView.Bind(Street); }
            else { var rig = cameraObject.AddComponent<FollowCameraRig>(); rig.Target = player.transform; rig.Orbit = orbit; }
            var lightObject = new GameObject("Sun", typeof(Light)); lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(45, -30, 0);
            Light light = lightObject.GetComponent<Light>(); light.type = LightType.Directional; light.shadows = LightShadows.Soft;
            RenderSettings.sun = light; RenderSettings.ambientMode = AmbientMode.Flat;
            // Region look (fog, sun, ambient, sky) of the walked leg's region; older prototypes keep the default look.
            ApplyLook(Look, light, camera);
            gestures = gameObject.AddComponent<GestureRouter>(); gestures.Bind(state, camera, body.GetComponent<Collider>(), orbit);
        }

        // Sets every look value each time (fog included), so nothing carries over from the previous scene's region.
        public static void ApplyLook(RegionLook look, Light sun, Camera camera)
        {
            Color Rgb(float[] c) => new Color(c[0], c[1], c[2]);
            RenderSettings.fog = look.Fog;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Rgb(look.FogColor);
            RenderSettings.fogStartDistance = look.FogStart; RenderSettings.fogEndDistance = look.FogEnd;
            RenderSettings.ambientLight = Rgb(look.Ambient);
            if (sun != null) { sun.intensity = look.SunIntensity; sun.color = Rgb(look.SunTint); }
            if (camera != null) camera.backgroundColor = Rgb(look.Sky);
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
            Button(safe, $"CAP: {MovementState.StarterSpeedCap:F1} / {MovementState.MaximumSpeedCap:F1}", 0.52f, 0.045f, 0.96f, 0.115f, () => state.SetSpeedCap(state.SpeedCap < MovementState.MaximumSpeedCap ? MovementState.MaximumSpeedCap : MovementState.StarterSpeedCap));
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
