using System.Collections.Generic;
using TheTrailWalker.Domain.Travel;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    public enum StreetBuildingKind { House, Shop, Warehouse, LumberField }

    public sealed class StreetBuilding
    {
        public string Id;
        public StreetBuildingKind Kind;
        public int Plot = -1;   // 0-5 for houses, otherwise -1
        public float CentreX;
        // Street-space Z of the building's front face line: 0 for the storefront row, SettlementScenery.HouseSetback for houses
        // (they stand behind a front garden). FrontLine and WingFromZ are measured from here.
        public float FrontZ;
        public Transform Root;
        // X extent of the front band (z from the front line to WingFromZ), in street space: where street dressing must stay
        // clear. Rear wings may be wider (up to WingWidth); they only matter behind the band.
        public float FrontLeft, FrontRight;
    }

    public sealed class SettlementStreet
    {
        public IReadOnlyList<StreetBuilding> Buildings;
        public float MinX, MaxX;   // the camera's scroll range: the first and last plot
        public float ShopsCentreX; // where the camera starts: the middle of the storefront row
        public SettlementBackdrop.Result Backdrop;
        public SettlementStreetProps.Result Props;
        public Transform Root;     // the "Settlement street" object holding everything above (debug rebuilds destroy it)
        public StreetBuilding Find(string id) { foreach (StreetBuilding b in Buildings) if (b.Id == id) return b; return null; }
    }

    // The settlement street (spec 2026-11-08, section 2): six plots and the storefront row along the X axis, fronts facing the
    // camera on the -Z side. Visual only: selection boxes on building roots are the only colliders.
    public static class SettlementScenery
    {
        // A plot is PlotWidth of land (the house envelope, side strips and wings); neighbouring plots stand PlotLane apart
        // (user, November 13, 2026: top-level houses stood too close), so centres are PlotSpacing apart. Next to the storefront
        // row a plot keeps FrontGap, as before.
        public const float PlotWidth = 15.2f, PlotLane = 2.8f, PlotSpacing = PlotWidth + PlotLane, FrontGap = 1.4f, PlaceholderLumberWidth = 8f;
        public const int BackdropSeed = 91126;

        // Mirror of Assets/_Game/Art/Settlement/Storefronts/StorefrontManifest.json (the Art folder is not in a player build);
        // SettlementShellValidation asserts that this table equals the manifest. The last row is the lumber frontage.
        public static readonly IReadOnlyList<(string Name, string Front, float Width)> Storefronts = new[]
        {
            ("Warehouse", "WarehouseFront", 8.6f), ("Bakery", "BakeryFront", 6.4f), ("Ranch", "RanchFront", 6.6f), ("Farm", "FarmFront", 7.2f),
            ("Fishery", "FisheryFront", 6.0f), ("Mine", "MineFront", 8.2f), ("Tailor", "TailorFront", 5.4f), ("Lumber", "LumberFront", 7.8f),
        };

        // Space planning (user, November 9, 2026): the street is laid out on fixed slots sized for the largest future look, not
        // for today's art, so nothing moves when a house reaches levels 9-16 (Part 2 section 16.1) or a storefront changes to
        // any of its eight tier looks (pine to ebony; Part 1 section 13.4, Part 2 section 16.3).
        // - Storefront slot: today's measured footprint (awnings, signs and porches included) plus at least 1.2 m of growth,
        //   rounded up to 0.5 m. Every tier look of that shop must fit inside it. Slots stand SlotGap apart, so the narrowest
        //   gap between two shops is 1 m (a lamp post) whatever the looks.
        // - House envelope (exterior redesign, user, November 10, 2026; Docs/Art/Settlement/house-exterior-redesign-handoff-
        //   2026-11-10.md): 15.2 m plots. The house front (its first 3 m of depth) may be up to HouseEnvelopeWidth (11 m) wide;
        //   the pet home and the collection container hug it in 1.9 m side strips (anchor at front width / 2 + 0.95 m, per
        //   level), never beyond SideStripOuter (7.4 m) from the plot centre; behind z = 3 m, wings may reach WingWidth (14.8 m).
        //   Exteriors may rise to MaxHouseHeight (the top of the portrait street frame at the house line). The interior grid
        //   (12 columns, rows added at the back) is unaffected: upper floors and towers are visual only.
        // - Depth: nothing reaches more than FrontLine in front of z = 0 (fronts that do are pushed back to it), and houses
        //   may grow back to EnvelopeDepth (level 16 is about 17 m of wall plus eaves and terraces), with the yard behind
        //   them inside the palisade.
        // - Front gardens (user, November 13, 2026: "the houses should be fully visible in height"): house plots stand
        //   HouseSetback behind the storefront line, so a 13 m house tops out just below the title line, the future
        //   stamina/hearts bar (user: "the max size in height should reach below the title"; on the test phone the title ends
        //   4.1% below the screen top, so TopMarginShare keeps 5.5%). The debug lines under it go in the release. At 4 m the
        //   test phone also sees about 11.4 m across, so an 11 m front fits; narrower phones scroll sideways.
        //   The garden between the road and the house (fence, gate, path, beds) stays low so it never hides a door.
        public static readonly float[] StorefrontSlots = { 11.0f, 8.5f, 8.5f, 9.5f, 9.5f, 11.5f, 7.5f, 9.0f };
        public const float SlotGap = 1.0f, HouseEnvelopeWidth = 11.0f, SideStripWidth = 1.9f, SideStripOuter = 7.4f, WingWidth = 14.8f, WingFromZ = 3f,
            MaxHouseHeight = 13f, FrontLine = 1.5f, EnvelopeDepth = 19f, HouseSetback = 4f, TopMarginShare = 0.055f,
            GardenPathWidth = 1.4f, GardenMaxHeight = 1.0f;

        // Where a point (street space, the camera centred on its X) lands on the screen, vertically: -1 bottom, 0 centre, 1 top.
        // The camera keeps a fixed vertical FOV, so this holds for every screen aspect.
        public static float ScreenHeightShare(float y, float z)
        {
            float pitch = Mathf.Atan2(StreetCamera.Height - StreetCamera.LookHeight, StreetCamera.Distance);
            float angle = Mathf.Atan2(y - StreetCamera.Height, z + StreetCamera.Distance) + pitch;
            return Mathf.Tan(angle) / Mathf.Tan(StreetCamera.FieldOfView * 0.5f * Mathf.Deg2Rad);
        }
        public static float SlotWidth(int index, float artWidth) => Mathf.Max(StorefrontSlots[index], artWidth);

        // Pet home look by house tier (Part 1 section 13.5, user, November 10, 2026): none before house level 3 (pets unlock),
        // then PetHomeT2 (kennel, houses 3-4) up to PetHomeT8 (ebony pavilion, houses 15-16). Prefabs in Settlement/Houses.
        public static string PetHomeLook(int houseLevel) => houseLevel < 3 ? null : "PetHomeT" + Mathf.Clamp((houseLevel + 1) / 2, 2, 8);

        // The art faces +Z at yaw 0; the camera looks along +Z from the street, so fronts turn 180 degrees to face -Z.
        public const float FrontYaw = 180f;
        // Placeholder lumber frontage layout (metres): fence sections along the street, pines behind them.
        private const float FenceLength = 4f, PineDepth = 5f;

        // Scene info from the save: before the plot choice all six houses show level 1; afterwards neighbours show their level
        // and the player's plot stays level 1 until the home is defined (part 2 of milestone 10).
        // The player's plot shows the player's own house level once the plot is chosen.
        public static int[] HouseLevels(SettlementState state) => HouseLevels(state, 1);
        public static int[] HouseLevels(SettlementState state, int playerLevel)
        {
            var levels = new[] { 1, 1, 1, 1, 1, 1 };
            if (state == null) return levels;
            foreach (SettlementState.Neighbour n in state.Neighbours) if (n.Plot >= 0 && n.Plot < levels.Length) levels[n.Plot] = Mathf.Clamp(n.Level, 1, SettlementState.MaxLevel);
            if (state.PlotIndex >= 0 && state.PlotIndex < levels.Length) levels[state.PlotIndex] = Mathf.Clamp(playerLevel, 1, SettlementState.MaxLevel);
            return levels;
        }
        public static int PlayerPlot(SettlementState state) => state == null ? -1 : state.PlotIndex;

        public static SettlementStreet Build(Transform parent, int[] houseLevels, int playerPlot) => Build(parent, houseLevels, playerPlot, null, false);
        public static SettlementStreet Build(Transform parent, int[] houseLevels, int playerPlot, int reachedTier)
            => Build(parent, houseLevels, playerPlot, null, false, Resources.Load<GameObject>, reachedTier);

        // Storefront look by the shops' reached tier (Milestone 14, Task 6): tier 2 and up show the birch <Front>T2 (the only
        // upgraded look so far); Build falls back to the tier 1 front when that prefab is missing.
        // The tier look a front shows: tier 2 birch (Milestone 14), tier 3 mesquite (Tier 3 integration slice C); tier 4 has
        // no look yet and keeps tier 3. Missing art falls back one tier at a time (LookWithFallback).
        public static string FrontFor(string front, int reachedTier) => reachedTier >= 3 ? front + "T3" : reachedTier >= 2 ? front + "T2" : front;
        public static string LookWithFallback(string front, int reachedTier, System.Func<string, GameObject> load)
        {
            for (int tier = System.Math.Min(reachedTier, 3); tier >= 2; tier--)
            {
                string look = FrontFor(front, tier);
                if (load("Settlement/Storefronts/" + look) != null) return look;
            }
            return front;
        }

        // lumberFrontWidthOverride replaces the LumberFront width (so the live branch is testable with another size);
        // forcePlaceholder builds the 8 m fence-and-pines placeholder even though LumberFront exists.
        public static SettlementStreet Build(Transform parent, int[] houseLevels, int playerPlot, float? lumberFrontWidthOverride, bool forcePlaceholder)
            => Build(parent, houseLevels, playerPlot, lumberFrontWidthOverride, forcePlaceholder, Resources.Load<GameObject>);

        // The loader maps a Resources path to a prefab (null when missing); the checks inject one to force the missing-art path.
        // Missing art never throws (a save in the settlement must always load): it logs a warning and builds a placeholder cube.
        public static SettlementStreet Build(Transform parent, int[] houseLevels, int playerPlot, float? lumberFrontWidthOverride, bool forcePlaceholder, System.Func<string, GameObject> load, int reachedTier = 1)
        {
            if (houseLevels == null || houseLevels.Length != SettlementState.PlotCount) throw new System.ArgumentException("Six house levels are required.", nameof(houseLevels));
            var street = new GameObject("Settlement street").transform; street.SetParent(parent, false);
            var buildings = new List<StreetBuilding>();

            // Storefront row: left slot edge at x = 0, each slot SlotGap after the last; each front centred in its slot.
            GameObject lumberFront = forcePlaceholder ? null : load("Settlement/Storefronts/LumberFront");
            float x = 0f, rowStart = 0f, rowEnd = 0f, lumberCentre = 0f, lumberWidth = 0f;
            for (int i = 0; i < Storefronts.Count; i++)
            {
                (string name, string front, float width) = Storefronts[i];
                bool lumber = i == Storefronts.Count - 1;
                if (lumber) width = lumberFront != null ? (lumberFrontWidthOverride ?? width) : PlaceholderLumberWidth;
                float slot = SlotWidth(i, width);
                float centre = x + slot / 2f;
                string id = lumber ? "lumber" : i == 0 ? "warehouse" : "shop." + name.ToLowerInvariant();
                var root = new GameObject(id).transform; root.SetParent(street, false); root.localPosition = new Vector3(centre, 0f, 0f);
                string look = LookWithFallback(front, reachedTier, load);
                if (lumber) { BuildLumber(root, lumberFront, width, load, look); lumberCentre = centre; lumberWidth = slot; }
                else Place(root, look, "Settlement/Storefronts/" + look, load, width);
                KeepBehindFrontLine(root);
                AddSelectionBox(root);
                buildings.Add(new StreetBuilding { Id = id, Kind = lumber ? StreetBuildingKind.LumberField : i == 0 ? StreetBuildingKind.Warehouse : StreetBuildingKind.Shop, CentreX = centre, Root = root });
                if (i == 0) rowStart = centre;
                if (i == Storefronts.Count - 2) rowEnd = centre;
                x += slot + SlotGap;   // centre to centre: (this slot + next slot) / 2 + gap
            }
            float warehouseLeft = buildings[0].CentreX - SlotWidth(0, Storefronts[0].Width) / 2f;
            float lumberRight = lumberCentre + lumberWidth / 2f;

            // Plots 0-2 sit left of the warehouse and 3-5 right of the lumber frontage; spacing is 11.2 m centre to centre.
            float plotHalf = PlotWidth / 2f;
            for (int plot = 0; plot < SettlementState.PlotCount; plot++)
            {
                float centre = plot < 3 ? warehouseLeft - FrontGap - plotHalf - (2 - plot) * PlotSpacing : lumberRight + FrontGap + plotHalf + (plot - 3) * PlotSpacing;
                int level = Mathf.Clamp(houseLevels[plot], 1, SettlementState.MaxLevel);
                var root = new GameObject("plot." + plot).transform; root.SetParent(street, false); root.localPosition = new Vector3(centre, 0f, HouseSetback);
                // A missing house front falls back to the highest lower level that exists, down to level 1, then to a cube.
                int shown = level;
                while (shown > 1 && load($"Settlement/Houses/HouseL{shown:00}Front") == null) shown--;
                Place(root, $"HouseL{shown:00}Front", $"Settlement/Houses/HouseL{shown:00}Front", load, PlotWidth);
                if (shown != level) Debug.LogWarning($"[Settlement] HouseL{level:00}Front is missing; plot {plot} shows level {shown}.");
                KeepBehindFrontLine(root);
                AddSelectionBox(root);
                buildings.Add(new StreetBuilding { Id = "plot." + plot, Kind = StreetBuildingKind.House, Plot = plot, CentreX = centre, FrontZ = HouseSetback, Root = root });
            }
            foreach (StreetBuilding b in buildings) (b.FrontLeft, b.FrontRight) = FrontBand(street, b);
            buildings.Sort((a, b) => a.CentreX.CompareTo(b.CentreX));

            float min = float.MaxValue, max = float.MinValue;
            foreach (StreetBuilding b in buildings) if (b.Kind == StreetBuildingKind.House) { min = Mathf.Min(min, b.CentreX); max = Mathf.Max(max, b.CentreX); }
            BuildRoad(street, min - 90f, max + 90f);
            foreach (StreetBuilding b in buildings) if (b.Kind == StreetBuildingKind.House) BuildGardenPath(street, b);
            // Backdrop (verge, yards, palisade, woodland-to-mountain slope, far mountains): fixed seed, so every visit looks the same.
            var backs = new List<(float, float, float, bool)>();
            foreach (StreetBuilding b in buildings)
            {
                var box = b.Root.GetComponent<BoxCollider>();
                float half = box != null ? box.size.x / 2f : 4f, back = b.FrontZ + (box != null ? box.center.z + box.size.z / 2f : 4f);
                backs.Add((b.CentreX, half, back, b.Kind == StreetBuildingKind.House));
            }
            // Street and yard props (phase B) first: when the house yard kit exists the backdrop drops its yard stand-ins.
            SettlementStreetProps.Result props = SettlementStreetProps.Build(street, buildings, BackdropSeed + 1, load);
            SettlementBackdrop.Result backdrop = SettlementBackdrop.Build(street, min, max, backs, BackdropSeed, load, props.YardKit);
            return new SettlementStreet { Root = street, Buildings = buildings, MinX = min, MaxX = max, ShopsCentreX = (rowStart + rowEnd) / 2f, Backdrop = backdrop, Props = props };
        }

        private static Transform Place(Transform root, string name, string resource, System.Func<string, GameObject> load, float slotWidth)
        {
            GameObject prefab = load(resource);
            if (prefab == null)
            {
                Debug.LogWarning("[Settlement] Missing art " + resource + "; using a placeholder block.");
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(block.GetComponent<Collider>());
                block.name = name; block.transform.SetParent(root, false);
                block.transform.localScale = new Vector3(slotWidth, 4f, 4f); block.transform.localPosition = Vector3.up * 2f;
                Material material = Region01Environment.GroundMaterial;
                if (material != null) block.GetComponent<Renderer>().sharedMaterial = material;
                return block.transform;
            }
            GameObject piece = Object.Instantiate(prefab, root, false);
            piece.name = name; piece.transform.localPosition = Vector3.zero; piece.transform.localRotation = Quaternion.Euler(0f, FrontYaw, 0f);
            return piece.transform;
        }

        // Lumber frontage: LumberFront when it exists, otherwise two fence sections with MassivePineA/B/C behind them.
        private static void BuildLumber(Transform root, GameObject front, float width, System.Func<string, GameObject> load, string look)
        {
            if (front != null) { Place(root, look, "Settlement/Storefronts/" + look, load, width); return; }
            for (int i = 0; i < 2; i++)
            {
                Transform fence = Place(root, "FenceSection", "Settlement/Route/FenceSection", load, width / 2f);
                fence.localPosition = new Vector3((i - 0.5f) * width / 2f, 0f, -0.5f);
            }
            string[] pines = { "MassivePineA", "MassivePineB", "MassivePineC" };
            for (int i = 0; i < pines.Length; i++)
            {
                Transform pine = Place(root, pines[i], "Settlement/LumberField/" + pines[i], load, width / 3f);
                pine.localPosition = new Vector3((i - 1) * width / 3f, 0f, PineDepth);
            }
        }

        // A front whose porch, steps or stock display reaches more than FrontLine toward the road moves back so it stops there
        // (the road begins half a metre further out).
        private static void KeepBehindFrontLine(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            float nearest = float.MaxValue;
            foreach (Renderer r in renderers) nearest = Mathf.Min(nearest, r.bounds.min.z);
            float over = root.position.z - FrontLine - nearest;
            if (over <= 0f) return;
            for (int i = 0; i < root.childCount; i++) root.GetChild(i).localPosition += Vector3.forward * over;
        }

        // The front band's X extent from the actual mesh vertices (falls back to the selection box when there are none).
        private static (float, float) FrontBand(Transform street, StreetBuilding building)
        {
            float left = float.MaxValue, right = float.MinValue;
            foreach (MeshFilter filter in building.Root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                Matrix4x4 toStreet = street.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                Bounds mb = filter.sharedMesh.bounds;
                Vector3[] points = filter.sharedMesh.isReadable ? filter.sharedMesh.vertices
                    : new[] { mb.min, mb.max, new Vector3(mb.min.x, mb.min.y, mb.max.z), new Vector3(mb.max.x, mb.max.y, mb.min.z) };   // unreadable: its bounds
                foreach (Vector3 v in points)
                {
                    Vector3 p = toStreet.MultiplyPoint3x4(v);
                    if (p.z > building.FrontZ + WingFromZ) continue;
                    left = Mathf.Min(left, p.x); right = Mathf.Max(right, p.x);
                }
            }
            if (left <= right) return (left, right);
            var box = building.Root.GetComponent<BoxCollider>();
            float cx = building.CentreX + (box != null ? box.center.x : 0f), half = box != null ? box.size.x / 2f : 2f;
            return (cx - half, cx + half);
        }

        // The selection box: a BoxCollider on the root sized to its renderers (selection only; nothing else collides).
        private static void AddSelectionBox(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var box = root.gameObject.AddComponent<BoxCollider>();
            if (renderers.Length == 0) { box.size = new Vector3(4f, 3f, 4f); box.center = Vector3.up * 1.5f; return; }
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
            box.center = root.InverseTransformPoint(bounds.center);
            box.size = bounds.size;
        }

        // A house's garden path: packed earth from the road to under the porch, centred on the door (doors stay centred).
        private static void BuildGardenPath(Transform parent, StreetBuilding house)
        {
            Material dirt = Region01Environment.TrailMaterial;
            float x0 = house.CentreX - GardenPathWidth / 2f, x1 = house.CentreX + GardenPathWidth / 2f, z0 = SettlementBackdrop.RoadFar - 0.05f, z1 = house.FrontZ - 0.2f;
            var path = new GameObject("Garden path " + house.Id, typeof(MeshFilter), typeof(MeshRenderer)); path.transform.SetParent(parent, false);
            var mesh = new Mesh { name = "Garden path" };
            mesh.vertices = new[] { new Vector3(x0, 0.025f, z0), new Vector3(x0, 0.025f, z1), new Vector3(x1, 0.025f, z1), new Vector3(x1, 0.025f, z0) };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(0, (z1 - z0) / 3.4f), new Vector2(GardenPathWidth / 3.4f, (z1 - z0) / 3.4f), new Vector2(GardenPathWidth / 3.4f, 0) };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 }; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            path.GetComponent<MeshFilter>().sharedMesh = mesh; if (dirt != null) path.GetComponent<MeshRenderer>().sharedMaterial = dirt;
        }

        // Packed-earth road along the fronts (the backdrop builds the ground around it).
        private static void BuildRoad(Transform parent, float minX, float maxX)
        {
            Material dirt = Region01Environment.TrailMaterial;
            const float roadNear = SettlementBackdrop.RoadNear, roadFar = SettlementBackdrop.RoadFar;
            var road = new GameObject("Street road", typeof(MeshFilter), typeof(MeshRenderer)); road.transform.SetParent(parent, false);
            var roadMesh = new Mesh { name = "Street road" };
            roadMesh.vertices = new[] { new Vector3(minX, 0.02f, roadNear), new Vector3(minX, 0.02f, roadFar), new Vector3(maxX, 0.02f, roadFar), new Vector3(maxX, 0.02f, roadNear) };
            roadMesh.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2((maxX - minX) / 3.4f, 1), new Vector2((maxX - minX) / 3.4f, 0) };
            roadMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 }; roadMesh.RecalculateNormals(); roadMesh.RecalculateBounds();
            road.GetComponent<MeshFilter>().sharedMesh = roadMesh; if (dirt != null) road.GetComponent<MeshRenderer>().sharedMaterial = dirt;
        }
    }

    // Street scrolling physics (user, November 13, 2026: "the sideway scroll feels rigid, make it more fluid and allow for fast
    // gestures"). Plain C#, driven with explicit times so the checks can run it without frames:
    // - while dragging, the street follows the finger 1:1; past either end it moves at RubberFactor, up to Overscroll;
    // - on release, the finger's speed over the last VelocityWindow seconds becomes a glide (amplified by FlingBoost) that
    //   decays exponentially (time constant GlideTau), so a flick travels about speed × GlideTau;
    // - flicks in the same direction within ChainWindow of the last glide stack up to MaxChain × faster, for long trips;
    // - a glide that runs past an end is braked and springs back; a finger that lands on a moving street catches it (Begin
    //   reports it, so that touch is not a tap);
    // - GlideTo eases the camera to a building (list buttons, plot choice) instead of jumping.
    public sealed class StreetScroll
    {
        public const float Overscroll = 3.5f, RubberFactor = 0.35f, VelocityWindow = 0.09f, StaleRelease = 0.07f, MinFling = 1.5f, MaxFling = 140f,
            FlingBoost = 1.6f, GlideTau = 0.55f, StopSpeed = 0.25f, ChainWindow = 0.6f, ChainStep = 1.5f, MaxChain = 4f, EdgeBrake = 18f, EdgeSpring = 11f,
            EaseRate = 7f, CatchSpeed = 2f;
        public float X { get; private set; }
        public float Velocity { get; private set; }
        public float Min { get; private set; }
        public float Max { get; private set; }
        public bool Dragging { get; private set; }
        public bool Moving => Dragging || Mathf.Abs(Velocity) > 0f || easing || OutOfBounds;
        private bool OutOfBounds => X < Min - 0.001f || X > Max + 0.001f;
        private readonly List<(float Time, float X)> samples = new List<(float, float)>();
        private bool easing; private float easeTarget;
        private float lastGlideTime = -10f, lastGlideSign, chain = 1f;

        public StreetScroll(float min, float max, float x) { SetBounds(min, max); X = Mathf.Clamp(x, Min, Max); }
        public void SetBounds(float min, float max) { Min = Mathf.Min(min, max); Max = Mathf.Max(min, max); }

        // Jump (no animation): the first frame and restored state.
        public void Snap(float x) { X = Mathf.Clamp(x, Min, Max); Velocity = 0f; easing = false; }
        public void GlideTo(float target) { if (Dragging) return; easeTarget = Mathf.Clamp(target, Min, Max); easing = true; Velocity = 0f; }

        // A finger lands: stop any glide. True when it caught a street moving fast enough that the touch must not be a tap.
        public bool Begin(float time)
        {
            bool caught = Mathf.Abs(Velocity) >= CatchSpeed || easing;
            Dragging = true; Velocity = 0f; easing = false;
            samples.Clear(); samples.Add((time, X));
            return caught;
        }

        // The finger moved by `metres` of street (positive scrolls the view to +X).
        public void Drag(float metres, float time)
        {
            if (!Dragging) return;
            float next = X + metres;
            if (next < Min && metres < 0f) next = X + metres * (X < Min ? RubberFactor : 1f);
            if (next > Max && metres > 0f) next = X + metres * (X > Max ? RubberFactor : 1f);
            X = Mathf.Clamp(next, Min - Overscroll, Max + Overscroll);
            samples.Add((time, X));
            while (samples.Count > 2 && samples[0].Time < time - VelocityWindow) samples.RemoveAt(0);
        }

        public void End(float time)
        {
            if (!Dragging) return;
            Dragging = false;
            float speed = 0f;
            if (samples.Count >= 2 && time - samples[samples.Count - 1].Time <= StaleRelease)
            {
                var first = samples[0]; var last = samples[samples.Count - 1];
                float span = Mathf.Max(last.Time - first.Time, 1f / 120f);
                speed = (last.X - first.X) / span;
            }
            samples.Clear();
            if (Mathf.Abs(speed) < MinFling || OutOfBounds) { Velocity = 0f; return; }
            float sign = Mathf.Sign(speed);
            chain = time - lastGlideTime <= ChainWindow && sign == lastGlideSign ? Mathf.Min(chain * ChainStep, MaxChain) : 1f;
            Velocity = Mathf.Clamp(speed * FlingBoost * chain, -MaxFling, MaxFling);
            lastGlideSign = sign;
        }

        public void Cancel() { Dragging = false; samples.Clear(); }

        // Advances a glide, an ease or a spring back by dt seconds.
        public void Tick(float dt, float time)
        {
            if (Dragging || dt <= 0f) return;
            if (easing)
            {
                X += (easeTarget - X) * (1f - Mathf.Exp(-dt * EaseRate));
                if (Mathf.Abs(easeTarget - X) < 0.01f) { X = easeTarget; easing = false; }
                return;
            }
            if (Velocity != 0f)
            {
                lastGlideTime = time;
                X += Velocity * GlideTau * (1f - Mathf.Exp(-dt / GlideTau));   // exact integral of the decaying speed
                Velocity *= Mathf.Exp(-dt / GlideTau);
                if (OutOfBounds) Velocity *= Mathf.Exp(-dt * EdgeBrake);
                if (Mathf.Abs(Velocity) < StopSpeed) Velocity = 0f;
                X = Mathf.Clamp(X, Min - Overscroll, Max + Overscroll);
            }
            if (Velocity == 0f && OutOfBounds)
            {
                float bound = X < Min ? Min : Max;
                X += (bound - X) * (1f - Mathf.Exp(-dt * EdgeSpring));
                if (Mathf.Abs(bound - X) < 0.01f) X = bound;
            }
        }
    }

    // Street camera: perspective, facing the fronts from the street side and slightly raised. Scrolls along X, no orbit.
    public sealed class StreetCamera : MonoBehaviour
    {
        public const float Height = 5f, Distance = 22f, LookHeight = 2.6f, FieldOfView = 52f;
        private SettlementStreet street;
        private StreetScroll scroll;

        public float X => scroll != null ? scroll.X : 0f;
        public StreetScroll Scroll => scroll;

        // Home views: the camera is held at a fixed world pose (HoldAt: the interior and its bird's-eye view, 200 m below the
        // street) until SnapToStreet; the scroll keeps ticking meanwhile.
        private enum Mode { Street, Away }
        private Mode mode = Mode.Street;
        private Vector3 awayPosition;
        private Quaternion awayRotation = Quaternion.identity;
        public bool OnStreet => mode == Mode.Street;

        public void Bind(SettlementStreet s) { street = s; scroll = new StreetScroll(s.MinX, s.MaxX, s.ShopsCentreX); mode = Mode.Street; Apply(); }
        // A rebuilt street (debug house-level preview) keeps the current scroll position instead of returning to the shops.
        public void Rebind(SettlementStreet s) { float x = X; street = s; scroll = new StreetScroll(s.MinX, s.MaxX, x); Apply(); }
        // Immediate move (kept for callers that must not animate).
        public void CentreOn(float target) { scroll?.Snap(target); Apply(); }
        // Eased move to a building: the list buttons and the plot choice.
        public void GlideTo(float target) { scroll?.GlideTo(target); }
        public bool BeginDrag() => scroll != null && scroll.Begin(Time.unscaledTime);
        public void DragBy(float metres) { scroll?.Drag(metres, Time.unscaledTime); Apply(); }
        public void EndDrag() { scroll?.End(Time.unscaledTime); }
        public void CancelDrag() { scroll?.Cancel(); }
        // Jumps straight to a fixed world pose and holds it (the home interior and its bird's-eye view; a cut, not an ease).
        public void HoldAt(Vector3 position, Quaternion rotation)
        {
            awayPosition = position; awayRotation = rotation; mode = Mode.Away;
            transform.SetPositionAndRotation(position, rotation);
        }
        // Immediately back on the street pose at the current scroll X.
        public void SnapToStreet() { mode = Mode.Street; Apply(); }
        private void LateUpdate()
        {
            if (street == null || scroll == null) return;
            scroll.Tick(Time.unscaledDeltaTime, Time.unscaledTime);
            if (mode == Mode.Street) Apply();
            else transform.SetPositionAndRotation(awayPosition, awayRotation);
        }
        private void Apply()
        {
            if (mode != Mode.Street) return;   // held away: LateUpdate keeps the held pose
            StreetPose(X, out Vector3 eye, out Quaternion look);
            transform.SetPositionAndRotation(eye, look);
        }

        // The street camera pose at scroll X.
        public static void StreetPose(float x, out Vector3 eye, out Quaternion rotation)
        {
            eye = new Vector3(x, Height, -Distance);
            Vector3 look = new Vector3(x, LookHeight, 0f);
            rotation = Quaternion.LookRotation(look - eye);
        }
    }
}
