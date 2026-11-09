using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Movement;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Where the player sits at a camp and where the camp meets the road.
    public sealed class CampAnchor
    {
        public CampDefinition Camp;
        public Vector3 RoadPoint, Seat, LookTarget;
        // The standing mission lady (Milestone 8): shown or hidden by the mission rules; a trigger collider takes taps.
        public GameObject MissionNpc;
        public float SeatYaw => Quaternion.LookRotation(Vector3.ProjectOnPlane(LookTarget - Seat, Vector3.up)).eulerAngles.y;
    }

    // Camp/river presentation for the Milestone 5 prototype: authored art where it exists, primitives where it does not.
    public static class TravelScenery
    {
        public const float ClearingOffset = 8f, SeatRadius = 2.3f;

        // Deterministic gentle curve per segment so a segment always looks the same; length roughly matches the catalog.
        public static Vector3[] KnotsFor(SegmentDefinition segment)
        {
            int count = Mathf.Max(2, Mathf.CeilToInt(segment.Length / 28f) + 1);
            var knots = new Vector3[count];
            float step = segment.Length / (count - 1) * 0.94f;
            uint hash = 2166136261; foreach (char c in segment.Id) { hash ^= c; hash *= 16777619; }
            for (int i = 0; i < count; i++)
            {
                // River legs and the settlement approach run straight (the approach ends square-on at the entrance arch).
                float swing = segment.RiverApproach || segment.SettlementApproach || i == 0 ? 0f : ((hash >> (i % 24)) % 17) - 8f;
                // Hills: alternating climbs and descents with a per-segment phase. Both ends are level (user, November 2,
                // 2026: the road dropped into a camp and climbed out again in a V): the first and last two knots sit at the
                // camp height and the hills ease in between, so every camp has a flat approach and departure.
                float ends = Mathf.Min(i, count - 1 - i);
                float envelope = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((ends - 1f) / 2f));
                // 85% of the relief keeps the steepest stretches near 40% instead of almost 50% (the road and its ground
                // stayed visibly apart on the steepest grades); route costs use the catalog relief, not this height.
                float height = segment.Relief * 0.85f * Mathf.Sin(i * 1.15f + (hash % 7)) * envelope;
                knots[i] = new Vector3(swing, height, i * step);
            }
            return knots;
        }

        // Ground grid around the road whose height follows the road (level across it). A road-following strip folds
        // over itself on tight bends, and the overlapping triangles flickered.
        public static void BuildTerrain(Transform parent, ITrailPath path, Material ground, float margin = 70f, float cell = 3f, float undulation = 0f, bool styled = false, Vector3[] campPoints = null, Vector3[] pondPoints = null, RegionLook look = null, RiverCut[] rivers = null)
        {
            var terrain = new GameObject("Terrain", typeof(MeshFilter), typeof(MeshRenderer));
            terrain.transform.SetParent(parent, false);
            terrain.GetComponent<MeshFilter>().sharedMesh = TerrainMesh(path, margin, cell, undulation, styled, campPoints, pondPoints, look, rivers);
            terrain.GetComponent<MeshRenderer>().sharedMaterial = ground;
        }

        // Rolling-ground height away from the road: gentle on the plains, larger where the segment has relief.
        // The settlement approach is flat farmland: its palisade and backdrop stand at road height (review fix, November 8, 2026).
        public static float Undulation(SegmentDefinition segment) => segment.SettlementApproach ? 0f : 0.7f + segment.Relief * 0.3f;
        // Fern Hollow valley floor (spec 2026-11-08, section 2): the ground stays flat for ValleyFlat metres either side of the
        // road, then rises smoothly to ValleyHeight by ValleyRiseEnd. Added on top of the rolling ground, before the clearing
        // and pond levelling, so camps and ponds (all inside the floor) stay level.
        public static float ValleyLift(RegionLook look, float distanceToRoad) =>
            look != null && look.Valley ? look.ValleyHeight * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(look.ValleyFlat, look.ValleyRiseEnd, distanceToRoad)) : 0f;
        // Rolling ground starts beyond the camp clearings (up to 12.5 m) and interactables (up to 14 m), and stays
        // flat near both camps at the segment ends.
        private const float FlatBand = 16f, RollFull = 30f, CampFlat = 25f;
        private const int PaletteSize = 24;

        // Camp ground (user, November 2, 2026: Quiet Hilltop floated slightly above the ground). The terrain follows the
        // nearest road point, so on hill legs the ground under a clearing could sit above or below it. A disc around each
        // clearing centre is levelled to the camp's height; within 1.6-2.8 m of the road the road keeps its own clearance.
        public const float CampLevelRadius = 6.5f, CampLevelFade = 9.5f, CampLevelRoadStart = 1.6f, CampLevelRoadFull = 2.8f;
        public static float CampLevelWeight(Vector2 here, Vector3 clearing, float distanceToRoad) =>
            (1f - Mathf.InverseLerp(CampLevelRadius, CampLevelFade, Vector2.Distance(here, new Vector2(clearing.x, clearing.z)))) * Mathf.InverseLerp(CampLevelRoadStart, CampLevelRoadFull, distanceToRoad);
        // Fishing ponds (user, November 6, 2026: on the hill leg after Gorse Saddle the pond's uphill half was buried; the
        // ground fell 1-1.6 m across its 3.1 x 2.4 m footprint). Like a clearing, a disc around each pond is levelled to its
        // height; 6.2 m covers every 3 m grid triangle that touches the pond, so the whole pond rests on flat ground.
        public const float PondLevelRadius = 6.2f, PondLevelFade = 9.5f;
        public static float PondLevelWeight(Vector2 here, Vector3 pond, float distanceToRoad) =>
            (1f - Mathf.InverseLerp(PondLevelRadius, PondLevelFade, Vector2.Distance(here, new Vector2(pond.x, pond.z)))) * Mathf.InverseLerp(CampLevelRoadStart, CampLevelRoadFull, distanceToRoad);
        // Where an interactable stands beside the road (shared by the builder and the terrain levelling).
        public static Vector3 AnchorCentre(ITrailPath path, InteractableAnchor anchor)
        {
            path.Evaluate(Mathf.Clamp(anchor.Distance, 0, path.Length), out Vector3 road, out Vector3 tangent);
            return road + Vector3.Cross(Vector3.up, tangent).normalized * anchor.Lateral;
        }
        public static void AddPonds(System.Collections.Generic.List<Vector3> ponds, ITrailPath path, System.Collections.Generic.IReadOnlyList<InteractableAnchor> anchors)
        {
            if (path == null || anchors == null) return;
            foreach (InteractableAnchor anchor in anchors) if (anchor.Kind == InteractableKind.FishingPoint) ponds.Add(AnchorCentre(path, anchor));
        }
        // Centre of a camp's clearing beside the road (shared by the camp builder, the terrain and the checks).
        public static Vector3 ClearingCentre(ITrailPath path, float distance, CampDefinition camp)
        {
            path.Evaluate(distance, out Vector3 road, out Vector3 tangent);
            return road + Vector3.Cross(Vector3.up, tangent).normalized * (camp.Side == CampSide.Right ? 1f : -1f) * ClearingOffset;
        }

        // River crossings (user report, November 8, 2026: every ferry stood on grass). The water box sat 4 cm under the ground
        // because the terrain had no channel. A river is a band across the road, RiverHalfWidth either side of its centre line
        // and RiverHalfLength along it. The ground there drops to a bed below the water, with banks up to the road's end. In a
        // valley the walls fall away near the river (flat to ValleyRiverOpen, back to full height by ValleyRiverFull), and the
        // rolling ground calms the same way, so the valley opens onto the river instead of walling it in.
        public readonly struct RiverCut
        {
            public readonly Vector3 Centre, Along; // Along: horizontal unit direction across the river (the crossing direction)
            public RiverCut(Vector3 centre, Vector3 along) { Centre = centre; Along = along; }
        }
        public const float RiverOffset = 9f, RiverHalfWidth = 7f, RiverHalfLength = 60f, RiverBank = 2f, RiverBed = 0.45f, RiverWaterDrop = 0.10f;
        public const float ValleyRiverOpen = 25f, ValleyRiverFull = 50f;
        // The river beyond a river leg's end, or behind a landing's start; shared by the water box and the terrain channel.
        public static RiverCut RiverCutFor(ITrailPath path, bool landing)
        {
            path.Evaluate(landing ? 0 : path.Length, out Vector3 end, out Vector3 tangent);
            if (landing) tangent = -tangent;
            tangent.y = 0; tangent.Normalize();
            return new RiverCut(end + tangent * RiverOffset, tangent);
        }

        public static Mesh TerrainMesh(ITrailPath path, float margin = 70f, float cell = 3f, float undulation = 0f, bool styled = false, Vector3[] campPoints = null, Vector3[] pondPoints = null, RegionLook look = null, RiverCut[] rivers = null)
        {
            int samples = Mathf.CeilToInt(path.Length) + 1;
            var points = new Vector3[samples]; var grade = new float[samples];
            Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
            for (int i = 0; i < samples; i++)
            {
                path.Evaluate(path.Length * i / (samples - 1), out points[i], out _);
                min = Vector3.Min(min, points[i]); max = Vector3.Max(max, points[i]);
            }
            // Bend of the road's height profile (second difference per metre). A flat grid triangle follows a straight
            // climb exactly; only crests and dips need clearance (user, November 2, 2026: the road looked raised up to
            // 0.8 m above the ground on steep legs because the clearance scaled with the slope).
            // Measured over 3 m either side (about a grid triangle), not per sample: the sampled road is made of short
            // straight pieces, and a per-metre second difference turned every joint into a sharp bend.
            const int span = 3;
            for (int i = 0; i < samples; i++)
            {
                Vector3 a = points[Mathf.Max(0, i - span)], m = points[i], b = points[Mathf.Min(samples - 1, i + span)];
                float back = Mathf.Max(0.01f, new Vector2(m.x - a.x, m.z - a.z).magnitude), ahead = Mathf.Max(0.01f, new Vector2(b.x - m.x, b.z - m.z).magnitude);
                grade[i] = Mathf.Abs((b.y - m.y) / ahead - (m.y - a.y) / back) / Mathf.Max(0.01f, (back + ahead) * 0.5f);
            }
            int columns = Mathf.CeilToInt((max.x - min.x + 2 * margin) / cell) + 1, rows = Mathf.CeilToInt((max.z - min.z + 2 * margin) / cell) + 1;
            var vertices = new Vector3[columns * rows]; var triangles = new int[(columns - 1) * (rows - 1) * 6];
            var roadDistance = new float[vertices.Length]; var rise = new float[vertices.Length];
            Vector2 start = new Vector2(points[0].x, points[0].z), end = new Vector2(points[samples - 1].x, points[samples - 1].z);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    float x = min.x - margin + c * cell, z = min.z - margin + r * cell;
                    var here = new Vector2(x, z);
                    float best = float.MaxValue, height = 0, slope = 0;
                    for (int i = 0; i < samples - 1; i++)
                    {
                        // Height of the closest point on the road polyline, not of the closest sample: a stepped height
                        // left the ground above the road on climbs.
                        Vector2 a = new Vector2(points[i].x, points[i].z), b = new Vector2(points[i + 1].x, points[i + 1].z), ab = b - a;
                        float t = Mathf.Clamp01(Vector2.Dot(here - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
                        float distance = (a + ab * t - here).sqrMagnitude;
                        if (distance < best) { best = distance; height = Mathf.Lerp(points[i].y, points[i + 1].y, t); slope = Mathf.Max(grade[i], grade[i + 1]); }
                    }
                    // Near the road, the ground also sinks by what a grid triangle can bulge above the road where its
                    // height profile bends (chord sag over a cell diagonal), so crests and dips never cover the road.
                    float distanceToRoad = Mathf.Sqrt(best);
                    float near = 1f - Mathf.InverseLerp(cell, cell * 2.5f, distanceToRoad);
                    // Rolling ground: two octaves of noise, faded in beyond the flat band and away from the end camps.
                    float roll = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(FlatBand, RollFull, distanceToRoad))
                               * Mathf.InverseLerp(CampFlat, CampFlat + 15f, Mathf.Min(Vector2.Distance(here, start), Vector2.Distance(here, end)));
                    float noise = (Mathf.PerlinNoise(x * 0.035f + 11.3f, z * 0.035f + 7.1f) - 0.5f) * 2f + (Mathf.PerlinNoise(x * 0.11f + 3.7f, z * 0.11f + 19.9f) - 0.5f) * 0.5f;
                    int index = r * columns + c;
                    float level = 0f, campHeight = 0f;
                    if (campPoints != null)
                        foreach (Vector3 camp in campPoints)
                        {
                            float w = CampLevelWeight(here, camp, distanceToRoad);
                            if (w > level) { level = w; campHeight = camp.y - 0.10f; }
                        }
                    if (pondPoints != null)
                        foreach (Vector3 pond in pondPoints)
                        {
                            float w = PondLevelWeight(here, pond, distanceToRoad);
                            if (w > level) { level = w; campHeight = pond.y - 0.06f; }
                        }
                    // Near a river the valley opens and the rolling ground calms; inside its band the ground drops to the bed.
                    float riverOpen = 1f, riverDepth = 0f, riverBed = 0f;
                    if (rivers != null)
                        foreach (RiverCut river in rivers)
                        {
                            Vector2 local = here - new Vector2(river.Centre.x, river.Centre.z);
                            Vector2 along2 = new Vector2(river.Along.x, river.Along.z), side2 = new Vector2(-river.Along.z, river.Along.x);
                            float along = Mathf.Abs(Vector2.Dot(local, along2)), side = Mathf.Abs(Vector2.Dot(local, side2));
                            if (side > RiverHalfLength) continue;
                            riverOpen = Mathf.Min(riverOpen, Mathf.InverseLerp(ValleyRiverOpen, ValleyRiverFull, along));
                            float depth = 1f - Mathf.InverseLerp(RiverHalfWidth, RiverHalfWidth + RiverBank, along);
                            if (depth > riverDepth) { riverDepth = depth; riverBed = river.Centre.y - RiverBed; }
                        }
                    roadDistance[index] = distanceToRoad; rise[index] = undulation * roll * noise * (1f - level) * riverOpen;
                    // Chord sag of a grid triangle over the bending road (diagonal^2 / 8), capped; plus a 6 cm base gap.
                    float diagonal = cell * 1.5f;
                    float terrainHeight = height - 0.06f - near * Mathf.Min(0.15f, 0.02f + slope * diagonal * diagonal * 0.125f) + rise[index] + ValleyLift(look, distanceToRoad) * riverOpen;
                    terrainHeight = Mathf.Lerp(terrainHeight, riverBed, riverDepth);
                    vertices[index] = new Vector3(x, Mathf.Lerp(terrainHeight, campHeight, level), z);
                }
            int k = 0;
            for (int r = 0; r < rows - 1; r++)
                for (int c = 0; c < columns - 1; c++)
                {
                    int v = r * columns + c;
                    triangles[k++] = v; triangles[k++] = v + columns; triangles[k++] = v + 1;
                    triangles[k++] = v + 1; triangles[k++] = v + columns; triangles[k++] = v + columns + 1;
                }
            var mesh = new Mesh { name = "Road-following terrain", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            if (!styled) { mesh.vertices = vertices; mesh.triangles = triangles; }
            else
            {
                // Faceted palette colours per triangle (unwelded so each facet keeps one colour): packed-earth shoulder
                // along the road, a dry-grass border, then meadow patches shaded by the rolling ground's height.
                var flat = new Vector3[triangles.Length]; var uv = new Vector2[triangles.Length]; var order = new int[triangles.Length];
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    int a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];
                    float d = (roadDistance[a] + roadDistance[b] + roadDistance[c]) / 3f, h = (rise[a] + rise[b] + rise[c]) / 3f;
                    Vector3 centre = (vertices[a] + vertices[b] + vertices[c]) / 3f;
                    float patch = Mathf.PerlinNoise(centre.x * 0.045f + 19f, centre.z * 0.045f + 41f), speck = Mathf.PerlinNoise(centre.x * 0.31f + 5f, centre.z * 0.31f + 2f);
                    int color;
                    if (d < 2.6f) color = (t / 3) % 3 == 0 ? 21 : 20;
                    else if (d < 4.2f) color = speck > 0.55f ? 7 : 5;
                    else if (h < -0.35f * undulation) color = speck > 0.6f ? 11 : 9;
                    else if (h > 0.45f * undulation) color = speck > 0.6f ? 7 : 6;
                    else color = patch < 0.37f ? 4 : patch > 0.66f ? 6 : speck > 0.72f ? 10 : 5;
                    for (int j = 0; j < 3; j++) { flat[t + j] = vertices[triangles[t + j]]; order[t + j] = t + j; uv[t + j] = new Vector2((color + 0.5f) / PaletteSize, 0.5f); }
                }
                mesh.vertices = flat; mesh.uv = uv; mesh.triangles = order;
            }
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        public static CampAnchor BuildCamp(Transform parent, CampDefinition camp, ITrailPath path, float distance, Material ground, Material wood, Material npc,
            int regionNumber = 1, NpcAppearance[] traders = null)
        {
            path.Evaluate(distance, out Vector3 road, out Vector3 tangent);
            Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized * (camp.Side == CampSide.Right ? 1f : -1f);
            Vector3 centre = road + side * ClearingOffset;
            var root = new GameObject("Camp " + camp.Name).transform; root.SetParent(parent, false);
            // A 0.3 m deep clearing base: its top stays at the camp height and its sides hide any small ground gap.
            Primitive(root, "Clearing", PrimitiveType.Cylinder, centre + Vector3.down * 0.17f, new Vector3(9f, 0.15f, 9f), ground);
            // A short footpath joins the clearing to the road so the camp reads as beside it, not on it.
            Vector3 footpath = (road + centre) * 0.5f;
            GameObject walk = Primitive(root, "Footpath", PrimitiveType.Cube, footpath + Vector3.down * 0.01f, new Vector3(1.6f, 0.03f, ClearingOffset), ground);
            walk.transform.rotation = Quaternion.LookRotation(side);
            if (!CampAssembly.Campfire(root, centre, regionNumber))
            {
                var fire = Primitive(root, "Campfire", PrimitiveType.Cylinder, centre + Vector3.up * 0.15f, new Vector3(0.9f, 0.15f, 0.9f), npc);
                fire.GetComponent<Renderer>().material.SetColor("_BaseColor", new Color(1f, 0.45f, 0.1f));
            }
            // Four log seats: the one nearest the road is the player's; three NPC traders sit on the others.
            Vector3 toRoad = -side; Vector3 seat = Vector3.zero;
            for (int i = 0; i < 4; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(i * 90f, Vector3.up) * toRoad;
                Vector3 position = centre + direction * SeatRadius;
                string seatName = i == 0 ? "Player seat" : "Trader seat " + i;
                Transform authored = CampAssembly.LogSeat(root, seatName, position, centre, regionNumber);
                if (authored == null)
                {
                    GameObject log = Primitive(root, seatName, PrimitiveType.Cylinder, position + Vector3.up * 0.2f, new Vector3(0.45f, 0.6f, 0.45f), wood);
                    log.transform.rotation = Quaternion.FromToRotation(Vector3.up, Vector3.Cross(Vector3.up, direction));
                }
                if (i == 0) { seat = position; continue; }
                if (traders == null || i - 1 >= traders.Length || !CampAssembly.Trader(root, "Trader " + i, authored, traders[i - 1]))
                    Primitive(root, "Trader " + i, PrimitiveType.Capsule, position + Vector3.up * 0.95f, new Vector3(0.6f, 0.7f, 0.6f), npc);
            }
            // The standing mission lady in her sideways resting pose, turned towards the player seat. Tapping her opens the
            // mission papers; whether she is present follows section 12.1 (set by CollectionPrototype).
            Vector3 lady = centre + side * 3.3f + tangent * 2.5f;
            if (!CampAssembly.SpecialNpc(root, "Mission NPC", CampAssembly.Special?.MissionLadySideways, lady + Vector3.down * 0.02f, seat - lady))
                Primitive(root, "Mission NPC", PrimitiveType.Capsule, lady + Vector3.up * 1f, new Vector3(0.6f, 1f, 0.6f), npc);
            // Balloon merchant PLACEMENT TEST (October 31, 2026): shown at every camp for now, just outside the clearing on
            // the far side from the road, door facing the campfire. Art and bobbing only; which camps host it, the shop
            // and its interaction are Milestone 14.
            CampAssembly.BalloonMerchant(root, centre + side * 7.2f - tangent * 3.2f + Vector3.down * 0.02f, centre);
            Transform ladyRoot = root.Find("Mission NPC");
            if (ladyRoot != null)
            {
                // Tap target only: a trigger, so walking, pickups and occlusion rays ignore it.
                var box = ladyRoot.gameObject.AddComponent<BoxCollider>(); box.isTrigger = true;
                Vector3 scale = ladyRoot.lossyScale; float sx = Mathf.Max(0.01f, scale.x), sy = Mathf.Max(0.01f, scale.y);
                box.center = new Vector3(0, 1.05f / sy, 0); box.size = new Vector3(1.1f / sx, 2.2f / sy, 1.1f / sx);
            }
            return new CampAnchor { Camp = camp, RoadPoint = road, Seat = seat, LookTarget = centre, MissionNpc = ladyRoot != null ? ladyRoot.gameObject : null };
        }

        // River at the end of a final camp's river leg (dock, ferry, toll NPC), or with landing set, the far bank
        // behind the start of a ferry landing leg (dock and the ferry that just arrived).
        // Settlement approach (regions 3-4 spec, November 8, 2026): calm farmland edge with fence lines along the road, a
        // palisade across the view, house and storefront roofs behind it, and the entrance arch spanning the road where it
        // ends. Visual only (no colliders, pickups or interactables). Returns the arch position on the road.
        public const float ArchBeforeEnd = 0.4f;
        public static Vector3 BuildSettlementApproach(Transform parent, ITrailPath path, Material wood)
        {
            var root = new GameObject("Settlement approach").transform; root.SetParent(parent, false);
            path.Evaluate(path.Length - ArchBeforeEnd, out Vector3 arch, out Vector3 tangent);
            tangent.y = 0; tangent.Normalize();
            Quaternion facing = Quaternion.LookRotation(tangent);
            Vector3 across = facing * Vector3.right;
            void Piece(string name, string resource, Vector3 position, Quaternion rotation, Vector3 fallbackSize)
            {
                var prefab = Resources.Load<GameObject>(resource);
                GameObject piece;
                if (prefab != null) piece = Object.Instantiate(prefab, root, false);
                else
                {
                    piece = GameObject.CreatePrimitive(PrimitiveType.Cube); piece.transform.SetParent(root, false);
                    piece.transform.localScale = fallbackSize; position += Vector3.up * fallbackSize.y * 0.5f;
                    if (wood != null) piece.GetComponent<Renderer>().sharedMaterial = wood;
                }
                foreach (Collider collider in piece.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
                piece.name = name; piece.transform.SetPositionAndRotation(position, rotation);
            }
            Vector3 ground = Vector3.down * 0.06f;
            Piece("Entrance arch", "Settlement/Route/EntranceArch", arch + ground, facing, new Vector3(6.6f, 0.4f, 0.4f));
            // Palisade either side of the arch posts, 4 m segments out past the field of view.
            for (int i = 0; i < 11; i++)
                for (int sign = -1; sign <= 1; sign += 2)
                    Piece("Palisade", "Settlement/Route/PalisadeSegment", arch + ground + across * sign * (5.2f + 4f * i), facing, new Vector3(4f, 2.8f, 0.3f));
            // Split-rail fences along both road sides, from past the ferry landing to just before the palisade.
            for (float d = 22f; d <= path.Length - 10f; d += 4f)
            {
                path.Evaluate(d, out Vector3 road, out Vector3 along);
                along.y = 0; along.Normalize();
                Quaternion lengthwise = Quaternion.LookRotation(along) * Quaternion.Euler(0, 90, 0);
                Vector3 side = Vector3.Cross(Vector3.up, along);
                for (int sign = -1; sign <= 1; sign += 2) Piece("Fence", "Settlement/Route/FenceSection", road + ground + side * sign * 4.6f, lengthwise, new Vector3(4f, 1.1f, 0.1f));
            }
            // Static backdrop: settlement fronts behind the palisade, facing the approaching walker (their +Z faces the street).
            (string resource, float lateral, float depth)[] backdrop = {
                ("Settlement/Houses/HouseL02Front", -15f, 13f), ("Settlement/Storefronts/BakeryFront", -6.5f, 16f), ("Settlement/Storefronts/WarehouseFront", 7f, 17f),
                ("Settlement/Houses/HouseL04Front", 16f, 12f), ("Settlement/Houses/HouseL06Front", -25f, 19f), ("Settlement/Storefronts/FarmFront", 26f, 18f) };
            Quaternion facingWalker = facing * Quaternion.Euler(0, 180, 0);
            foreach (var (resource, lateral, depth) in backdrop)
                Piece("Settlement backdrop", resource, arch + ground + across * lateral + tangent * depth, facingWalker, new Vector3(6f, 5f, 6f));
            return arch;
        }

        public static void BuildRiver(Transform parent, ITrailPath path, Material water, Material wood, Material npc, bool landing = false)
        {
            path.Evaluate(landing ? 0 : path.Length, out Vector3 end, out Vector3 tangent);
            if (landing) tangent = -tangent;
            Vector3 across = Vector3.Cross(Vector3.up, tangent).normalized;
            var root = new GameObject(landing ? "Ferry landing" : "River crossing").transform; root.SetParent(parent, false);
            RiverCut cut = RiverCutFor(path, landing);   // same centre as the terrain channel
            GameObject river = Primitive(root, "River", PrimitiveType.Cube, cut.Centre + Vector3.down * (RiverWaterDrop + 0.05f), new Vector3(2f * RiverHalfLength, 0.1f, 2f * RiverHalfWidth), water);
            river.transform.rotation = Quaternion.LookRotation(tangent);
            river.GetComponent<Renderer>().material.SetColor("_BaseColor", new Color(0.2f, 0.45f, 0.75f));
            // Ferry art (November 2, 2026): plank dock, cable ferry raft and the cable across the river; primitives remain
            // the fallback when the art is missing. All face the crossing direction; the raft rides 5 m out.
            Quaternion facing = Quaternion.LookRotation(tangent);
            if (!FerryArt(root, "Dock", "FerryDock", end, facing))
                Primitive(root, "Dock", PrimitiveType.Cube, end + tangent * 1.5f, new Vector3(3f, 0.2f, 3f), wood).transform.rotation = facing;
            if (!FerryArt(root, "Ferry boat", "FerryRaft", end + tangent * 5f - Vector3.up * 0.12f, facing))
                Primitive(root, "Ferry boat", PrimitiveType.Cube, end + tangent * 5f + Vector3.up * 0.1f, new Vector3(2.6f, 0.6f, 4.5f), wood).transform.rotation = facing;
            FerryArt(root, "Ferry cable", "FerryCable", end, facing);
            // The ferryman stands beside the dock facing the arriving walker (static; the toll prompt is the HUD's).
            Vector3 ferryman = end + across * 2.2f;
            if (!landing && !CampAssembly.SpecialNpc(root, "Toll NPC", CampAssembly.Special?.FerrymanStatic, ferryman + Vector3.down * 0.05f, -tangent - across * 0.6f))
                Primitive(root, "Toll NPC", PrimitiveType.Capsule, ferryman + Vector3.up * 1f, new Vector3(0.6f, 1f, 0.6f), npc);
        }

        // Authored tier-one art at fixed anchors; interaction and hint animations arrive in Milestone 6.
        public static void BuildInteractables(Transform parent, ITrailPath path, System.Collections.Generic.IReadOnlyList<InteractableAnchor> anchors, Material wood, Material npc, Material ground, int resourceTier = 1)
        {
            var root = new GameObject("Interactables").transform; root.SetParent(parent, false);
            foreach (InteractableAnchor anchor in anchors)
            {
                path.Evaluate(Mathf.Clamp(anchor.Distance, 0, path.Length), out Vector3 road, out Vector3 tangent);
                Vector3 centre = road + Vector3.Cross(Vector3.up, tangent).normalized * anchor.Lateral;
                if (anchor.Kind == InteractableKind.Tree)
                {
                    // A tree cut off partway up: no canopy, but taller than a stump.
                    if (resourceTier != 1 || HarvestArt(root, anchor.Id, "PineTrunk", centre, Quaternion.identity) == null)
                        Primitive(root, anchor.Id, PrimitiveType.Cylinder, centre + Vector3.up * 1.3f, new Vector3(0.55f, 1.3f, 0.55f), wood);
                }
                else if (anchor.Kind == InteractableKind.AnimalSpawn)
                {
                    for (int i = 0; i < anchor.Animals; i++)
                    {
                        Vector3 offset = Quaternion.AngleAxis(i * 137f, Vector3.up) * Vector3.forward * (0.6f + 0.4f * i);
                        if (resourceTier != 1 || HarvestArt(root, anchor.Id + ".animal" + i, "Rabbit", centre + offset, Quaternion.Euler(0, i * 70f, 0)) == null)
                        {
                            GameObject animal = Primitive(root, anchor.Id + ".animal" + i, PrimitiveType.Capsule, centre + offset + Vector3.up * 0.22f, new Vector3(0.3f, 0.22f, 0.45f), npc);
                            animal.transform.rotation = Quaternion.Euler(90f, i * 70f, 0f);
                        }
                    }
                }
                else
                {
                    if (resourceTier != 1 || HarvestArt(root, anchor.Id, "FishingPoint", centre, Quaternion.identity) == null)
                    {
                        GameObject pond = Primitive(root, anchor.Id, PrimitiveType.Cylinder, centre + Vector3.down * 0.03f, new Vector3(3.2f, 0.03f, 3.2f), ground);
                        pond.GetComponent<Renderer>().material.SetColor("_BaseColor", new Color(0.25f, 0.5f, 0.78f));
                    }
                }
            }
        }

        private static GameObject HarvestArt(Transform parent, string name, string prefabName, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = Resources.Load<GameObject>("Tier01/Interactables/" + prefabName);
            if (prefab == null) return null;
            GameObject view = Object.Instantiate(prefab, parent, false);
            view.name = name; view.transform.position = position; view.transform.rotation = rotation;
            Harvest.Tier1HarvestVisual marker = view.GetComponent<Harvest.Tier1HarvestVisual>();
            if (marker != null) marker.AnchorId = name;
            return view;
        }

        // River crossing art from Resources/Environment/Ferry; scenery only, so any collider is removed.
        private static bool FerryArt(Transform parent, string name, string prefab, Vector3 position, Quaternion rotation)
        {
            GameObject source = Resources.Load<GameObject>("Environment/Ferry/" + prefab);
            if (source == null) return false;
            GameObject view = Object.Instantiate(source, position, rotation, parent); view.name = name;
            foreach (Collider collider in view.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
            return true;
        }

        private static GameObject Primitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var item = GameObject.CreatePrimitive(type);
            item.name = name; item.transform.SetParent(parent, false);
            item.transform.position = position; item.transform.localScale = scale;
            if (material != null) item.GetComponent<Renderer>().sharedMaterial = material;
            // Scenery never blocks pickups or touch rays.
            Object.Destroy(item.GetComponent<Collider>());
            return item;
        }
    }
}

namespace TheTrailWalker.Travel
{
    // What the current trail scene presents: the segment leaving the current camp and its camp anchors.
    public sealed class TravelSceneInfo
    {
        public TheTrailWalker.Domain.Travel.TravelCatalog Catalog;
        public TheTrailWalker.Domain.Travel.SegmentDefinition Segment;
        public CampAnchor StartCamp, EndCamp;
        public System.Action RequestReload;
        // Visit number the start camp is shown for (a mid-segment restart counts as a new visit); the end camp is the next one.
        public int StartVisit = 1;
        // Settlement mode (spec 2026-11-08): the scene builds the street instead of a road. HouseLevels has six entries (index = plot).
        public bool Settlement;
        public int[] HouseLevels;
        public int PlayerPlot = -1;
    }
}
