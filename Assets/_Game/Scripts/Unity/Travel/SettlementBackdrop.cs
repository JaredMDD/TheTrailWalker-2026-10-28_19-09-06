using System.Collections.Generic;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Settlement backdrop, phase A (proposal Docs/Art/Settlement/settlement-backdrop-proposal-2026-11-09.md, approved by the user
    // November 9, 2026): the street stands on the border between Fern Hollow (region 4) and Cloudstep Mountains (region 5).
    // Layers from the camera outward: the street verge, back yards, the palisade with the entrance arch and the closed departure
    // gate, a woodland slope rising behind the wall, and region 5's mountains on the horizon. Region 4's kit dominates at the
    // entrance end (left, low X) and near the wall; region 5's kit takes over toward the departure end and higher up the slope.
    // Visual only: no colliders (they are stripped from instantiated prefabs), deterministic for a seed, and every scattered piece
    // is combined per material, shadow group and 30 m chunk so the narrow street camera still frustum-culls most of it.
    public static class SettlementBackdrop
    {
        // Layout (metres; the fronts stand at z = 0 and face the camera on the -Z side).
        // The palisade and the slope moved back 4 m with the house setback (front gardens, November 13, 2026).
        public const float RoadNear = -9.8f, RoadFar = -2.0f, VergeNear = -14.3f, PalisadeZ = 32f, SlopeStart = 36f, SlopeEnd = 128f,
            SlopeHeight = 20f, GroundBack = 160f, GroundFront = -30f, EndMargin = 9f, MountainZ = 235f, Chunk = 30f;
        // Tallest piece allowed between the camera and the fronts (verge cover only, never shrubs that hide a door).
        public const float VergeMaxHeight = 1.0f;

        // Ground height: flat street and yards, then a smooth rise behind the palisade with gentle undulation on the slope.
        public static float Height(float x, float z)
        {
            if (z <= SlopeStart) return 0f;
            float rise = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(SlopeStart, SlopeEnd, z));
            float wobble = (Mathf.PerlinNoise(x * 0.045f + 11f, z * 0.05f + 5f) - 0.5f) * 6f;
            return rise * (SlopeHeight + wobble);
        }

        // Share of region 5 at a point: 0 at the entrance end, 1 at the departure end (blending across the middle third of the
        // street), plus up to 0.55 more the higher up the slope.
        public static float Region5Share(float x, float z, float minX, float maxX)
        {
            float along = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minX + (maxX - minX) * 0.33f, minX + (maxX - minX) * 0.67f, x));
            float up = 0.55f * Mathf.InverseLerp(SlopeStart + 10f, SlopeEnd, z);
            return Mathf.Clamp01(along + up);
        }

        public sealed class Result
        {
            public Transform Root;
            public float PalisadeMinX, PalisadeMaxX;
            public int PalisadeSegments, EndWallSegments, Pieces;
            // Pieces whose meshes are not Read/Write enabled are instanced, never combined: a combined mesh built from unreadable
            // meshes renders nothing on the device (it still works in the Editor).
            public int Instanced; public bool CombinedUnreadable;
            public Transform EntranceArch, DepartureGate;
        }

        // minX/maxX: the first and last plot centre. buildingBacks: each building's centre X, half width and back edge (z).
        public static Result Build(Transform parent, float minX, float maxX, IReadOnlyList<(float CentreX, float HalfWidth, float BackZ, bool House)> buildingBacks,
            int seed, System.Func<string, GameObject> load, bool yardKit = false)
        {
            var result = new Result { Root = new GameObject("Settlement backdrop").transform };
            result.Root.SetParent(parent, false);
            var random = new System.Random(seed);
            float Range(float a, float b) => a + (float)random.NextDouble() * (b - a);
            var groups = new Dictionary<(Material, bool, int), List<CombineInstance>>();

            BuildGround(result.Root, minX - 90f, maxX + 90f, minX, maxX);

            // Kit pieces by role, per region.
            Roles r4 = Roles.Of(SceneryScatter.Kit(4)), r5 = Roles.Of(SceneryScatter.Kit(5));
            Roles KitAt(float x, float z) => random.NextDouble() < Region5Share(x, z, minX, maxX) ? r5 : r4;

            void Add(GameObject prefab, Vector3 position, float yaw, float scale, bool shadows)
            {
                if (prefab == null) return;
                bool readable = true;
                foreach (MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>()) if (filter.sharedMesh != null && !filter.sharedMesh.isReadable) readable = false;
                if (!readable)
                {
                    Transform piece = PlaceWhole(result.Root, prefab, prefab.name, position, yaw, scale);
                    foreach (Renderer r in piece.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    result.Instanced++; result.Pieces++;
                    return;
                }
                Matrix4x4 placement = Matrix4x4.TRS(position, Quaternion.Euler(0f, yaw, 0f), Vector3.one * scale);
                int chunk = Mathf.FloorToInt(position.x / Chunk);
                foreach (MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>())
                {
                    var renderer = filter.GetComponent<MeshRenderer>();
                    if (filter.sharedMesh == null || renderer == null || renderer.sharedMaterial == null) continue;
                    if (!filter.sharedMesh.isReadable) result.CombinedUnreadable = true;
                    var key = (renderer.sharedMaterial, shadows, chunk);
                    if (!groups.TryGetValue(key, out List<CombineInstance> list)) groups[key] = list = new List<CombineInstance>();
                    list.Add(new CombineInstance { mesh = filter.sharedMesh, transform = result.Root.worldToLocalMatrix * placement * filter.transform.localToWorldMatrix });
                }
                result.Pieces++;
            }
            void AddArt(RegionalSceneryArt art, float x, float z, float scale)
            {
                if (art == null) return;
                Add(art.Prefab, new Vector3(x, Height(x, z), z), Range(0f, 360f), scale, art.CastsShadows);
            }

            // 1. Street verge (only z = -14.3 .. -10.4 is in view): low cover, never taller than VergeMaxHeight.
            for (float x = minX - 30f; x <= maxX + 30f; x += 0.9f)
            {
                float z = Range(VergeNear, RoadNear - 1.2f);   // footprints up to about 1 m wide stay off the road
                Roles kit = KitAt(x, 0f);
                RegionalSceneryArt art = random.NextDouble() < 0.85 ? kit.PickLow(random, VergeMaxHeight) : null;
                if (art != null) AddArt(art, x + Range(-0.6f, 0.6f), z, Range(0.8f, 1.1f));
            }
            for (float x = minX - 30f; x <= maxX + 30f; x += Range(18f, 30f))
            {
                Roles kit = KitAt(x, 0f);
                AddArt(kit.PickRock(random, VergeMaxHeight), x, Range(VergeNear, RoadNear - 1.5f), 0.38f);
            }

            // 2. Back yards: behind each building, a little cover and a shrub; behind houses also a fence corner and a log seat.
            //    Once the yard kit exists (SettlementStreetProps, phase B) the fence and seat stand-ins go and the cover moves
            //    behind the yard props (z from yard + 6.5), so nothing grows through a garden bed or a shed.
            GameObject fence = load("Settlement/Route/FenceSection"), seat = load("Environment/CampProps/PineLogSeat");
            foreach ((float cx, float half, float back, bool house) in buildingBacks)
            {
                Roles kit = KitAt(cx, 0f);
                float yard = back + 1.2f;
                if (house && fence != null && !yardKit)
                {
                    Add(fence, new Vector3(cx - half + 2.1f, 0f, yard + 6f), 0f, 1f, true);
                    Add(fence, new Vector3(cx - half + 0.1f, 0f, yard + 4f), 90f, 1f, true);
                    Add(seat, new Vector3(cx + 1.5f, 0f, yard + 2.2f), Range(-15f, 15f), 1f, true);
                }
                float shrubFrom = yardKit ? 6.5f : 0.5f, shrubTo = yardKit ? Mathf.Min(10f, PalisadeZ - 1f - yard) : 7f;
                float lowFrom = yardKit ? 6.5f : 0f, lowTo = yardKit ? shrubTo : 8f;
                for (int i = 0; i < 3; i++) AddArt(kit.PickShrub(random), cx + Range(-half, half), yard + Range(shrubFrom, shrubTo), Range(0.8f, 1.2f));
                for (int i = 0; i < 4; i++) AddArt(kit.PickLow(random, 1.2f), cx + Range(-half, half), yard + Range(lowFrom, lowTo), 1f);
            }

            // 3. Palisade: a continuous wall behind the yards, turning forward at both ends; the entrance arch spans the road
            //    at the left end and a closed gate (the future departure arch) at the right end.
            GameObject palisade = load("Settlement/Route/PalisadeSegment");
            float left = minX - EndMargin, right = maxX + EndMargin;
            result.PalisadeMinX = left; result.PalisadeMaxX = right;
            const float SegmentLength = 4f;
            if (palisade != null)
            {
                for (float x = left + SegmentLength / 2f; x < right; x += SegmentLength) { Add(palisade, new Vector3(x, 0f, PalisadeZ), 0f, 1f, true); result.PalisadeSegments++; }
                // End walls from the back wall down to the arch posts (the gap for the road stays open), and past the road.
                foreach (float x in new[] { left, right })
                {
                    for (float z = PalisadeZ - SegmentLength / 2f; z > RoadFar + 2f; z -= SegmentLength) { Add(palisade, new Vector3(x, 0f, z), 90f, 1f, true); result.EndWallSegments++; }
                    for (float z = RoadNear - 4f; z > -26f; z -= SegmentLength) { Add(palisade, new Vector3(x, 0f, z), 90f, 1f, true); result.EndWallSegments++; }
                }
            }
            float roadMid = (RoadNear + RoadFar) / 2f;
            result.EntranceArch = PlaceWhole(result.Root, load("Settlement/Route/EntranceArch"), "Entrance arch", new Vector3(left, 0f, roadMid), 90f, 1f);
            result.DepartureGate = PlaceWhole(result.Root, load("Settlement/Route/EntranceArch"), "Departure gate (closed)", new Vector3(right, 0f, roadMid), 90f, 1f);
            if (result.DepartureGate != null && fence != null)
                for (int i = 0; i < 2; i++)   // the closed gate: fence rails across the opening
                    Add(fence, new Vector3(right, 0f, roadMid + (i - 0.5f) * 3.4f), 90f, 0.85f, true);

            // 4. Woodland-to-mountain slope behind the palisade: dense trees in groves, shrubs and rocks between, more rock
            //    higher up and toward region 5. Ferns only near the wall (they are the costly pieces).
            for (float x = minX - 70f; x <= maxX + 70f; x += 4.5f)
                for (float z = SlopeStart - 2f; z <= SlopeEnd + 25f; z += 4.5f)
                {
                    float px = x + Range(0f, 4.5f), pz = z + Range(0f, 4.5f);
                    Roles kit = KitAt(px, pz);
                    float share = Region5Share(px, pz, minX, maxX), grove = Mathf.PerlinNoise(px * 0.035f + 3f, pz * 0.035f + 9f);
                    double roll = random.NextDouble();
                    if (pz > 90f && random.NextDouble() < 0.35) continue;   // the far slope is thinner (and bigger trees carry it)
                    float scale = Range(0.85f, 1.35f) * (pz > 70f ? 1.25f : 1f);   // far trees read at distance
                    if (roll < 0.30f + 0.45f * grove) AddArt(kit.PickTree(random), px, pz, scale);
                    else if (roll < 0.30f + 0.45f * grove + 0.08f + 0.10f * share) AddArt(kit.PickRock(random, pz < 70f ? 2f : 99f), px, pz, Range(0.8f, 1.5f));   // no spires near the wall
                    else if (roll < 0.92f && pz < 45f) AddArt(kit.PickShrub(random), px, pz, Range(0.9f, 1.3f));
                    else if (roll < 0.97f && pz < 40f) AddArt(kit.PickLow(random, 99f), px, pz, 1f);
                }
            // Landmarks: three massive pines from the lumber field rising above the wall on the Fern Hollow side.
            string[] massive = { "MassivePineA", "MassivePineB", "MassivePineC" };
            for (int i = 0; i < massive.Length; i++)
            {
                float x = Mathf.Lerp(minX - 10f, minX + (maxX - minX) * 0.35f, i / 2f) + Range(-4f, 4f), z = Range(36f, 48f);
                Add(load("Settlement/LumberField/" + massive[i]), new Vector3(x, Height(x, z), z), Range(0f, 360f), Range(0.9f, 1.1f), true);
            }

            // 5. Region 5's mountains on the horizon: two rows of ridges with peaks, highest behind the departure end. The slope's
            //    crest hides their bases.
            GameObject ridge = r5.Find("DistantMountainRidge"), peak = r5.Find("MountainRidgePeak"), spire = r5.Find("RockSpire");
            for (float x = minX - 200f; x <= maxX + 200f; x += Range(90f, 120f))
            {
                float lift = Mathf.Lerp(0.85f, 1.25f, Mathf.InverseLerp(minX, maxX, x));
                Add(ridge, new Vector3(x, 0f, MountainZ + Range(-10f, 15f)), random.Next(2) * 180f, Range(3.6f, 4.2f) * lift, false);
                Add(ridge, new Vector3(x + 40f, -4f, MountainZ - 40f + Range(-8f, 8f)), random.Next(2) * 180f, Range(2.6f, 3.0f) * lift, false);
                if (random.NextDouble() < 0.35 + 0.5 * Mathf.InverseLerp(minX, maxX, x))
                    Add(peak, new Vector3(x + Range(-20f, 20f), 10f, MountainZ - 10f), Range(0f, 360f), Range(3.8f, 4.6f) * lift, false);
            }
            if (spire != null) Add(spire, new Vector3(maxX + 25f, Height(maxX + 25f, 60f), 60f), Range(0f, 360f), 3f, false);

            foreach (var group in groups)
                Combine(result.Root, $"{group.Key.Item1.name} chunk {group.Key.Item3} ({(group.Key.Item2 ? "shadow casters" : "no shadows")})", group.Value, group.Key.Item1, group.Key.Item2);
            return result;
        }

        internal static Transform PlaceWhole(Transform parent, GameObject prefab, string name, Vector3 position, float yaw, float scale)
        {
            if (prefab == null) return null;
            // Instantiated under an inactive holder so no script wakes up (the massive pines carry the chopping rig), then
            // stripped to meshes: scripts and colliders go before the piece activates. Visual only.
            var holder = new GameObject("inactive holder"); holder.SetActive(false);
            GameObject piece = Object.Instantiate(prefab, holder.transform, false);
            foreach (MonoBehaviour script in piece.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(script);
            foreach (Collider c in piece.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            foreach (Rigidbody body in piece.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(body);
            piece.transform.SetParent(parent, false);
            Object.DestroyImmediate(holder);
            piece.name = name; piece.transform.localPosition = position; piece.transform.localRotation = Quaternion.Euler(0f, yaw, 0f); piece.transform.localScale = Vector3.one * scale;
            return piece.transform;
        }

        internal static void Combine(Transform parent, string name, List<CombineInstance> instances, Material material, bool castShadows)
        {
            if (instances.Count == 0) return;
            var part = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); part.transform.SetParent(parent, false);
            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.CombineMeshes(instances.ToArray(), true, true); mesh.RecalculateBounds();
            part.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // Ground: a 4 m grid over the street, the yards and the slope. Colours from the Dawn Plains palette (24 swatches):
        // meadow greens on the flat, darker woodland greens on the slope, and stone greys and dry grass high up and toward
        // region 5.
        private static void BuildGround(Transform parent, float fromX, float toX, float minX, float maxX)
        {
            Material palette = Region01Environment.GroundMaterial;
            const float cell = 4f; const int paletteSize = 24;
            int columns = Mathf.CeilToInt((toX - fromX) / cell), rows = Mathf.CeilToInt((GroundBack - GroundFront) / cell);
            var vertices = new List<Vector3>(columns * rows * 4); var uvs = new List<Vector2>(columns * rows * 4); var triangles = new List<int>(columns * rows * 6);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    float x0 = fromX + c * cell, z0 = GroundFront + r * cell, cx = x0 + cell / 2f, cz = z0 + cell / 2f;
                    float noise = Mathf.PerlinNoise(c * 0.37f + 3f, r * 0.41f + 7f);
                    float share = Region5Share(cx, cz, minX, maxX), height = Height(cx, cz) / SlopeHeight;
                    int color;
                    if (cz < SlopeStart) color = noise < 0.35f ? 4 : noise > 0.7f ? 6 : 5;                       // meadow
                    else if (height + share * 0.4f + (noise - 0.5f) * 0.3f > 0.85f) color = noise < 0.5f ? 17 : noise < 0.8f ? 16 : 7;   // stone and dry grass
                    else color = noise < 0.33f ? 8 : noise < 0.66f ? 13 : 9;                                     // woodland floor
                    int v = vertices.Count;
                    vertices.Add(new Vector3(x0, Height(x0, z0), z0)); vertices.Add(new Vector3(x0, Height(x0, z0 + cell), z0 + cell));
                    vertices.Add(new Vector3(x0 + cell, Height(x0 + cell, z0 + cell), z0 + cell)); vertices.Add(new Vector3(x0 + cell, Height(x0 + cell, z0), z0));
                    for (int k = 0; k < 4; k++) uvs.Add(new Vector2((color + 0.5f) / paletteSize, 0.5f));
                    triangles.AddRange(new[] { v, v + 1, v + 2, v, v + 2, v + 3 });
                }
            var ground = new GameObject("Street meadow", typeof(MeshFilter), typeof(MeshRenderer)); ground.transform.SetParent(parent, false);
            var mesh = new Mesh { name = "Street meadow", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            ground.GetComponent<MeshFilter>().sharedMesh = mesh; if (palette != null) ground.GetComponent<MeshRenderer>().sharedMaterial = palette;
        }

        // A region kit's scatterable pieces by role (terrain replacements and distant-only pieces are kept for Find).
        private sealed class Roles
        {
            private readonly List<RegionalSceneryArt> low = new List<RegionalSceneryArt>(), shrubs = new List<RegionalSceneryArt>(),
                rocks = new List<RegionalSceneryArt>(), trees = new List<RegionalSceneryArt>(), all = new List<RegionalSceneryArt>();
            public static Roles Of(RegionalEnvironmentKit kit)
            {
                var roles = new Roles();
                if (kit?.Models == null) return roles;
                foreach (RegionalSceneryArt art in kit.Models)
                {
                    if (art == null || art.Prefab == null) continue;
                    roles.all.Add(art);
                    if (art.TerrainReplacement || art.DistantOnly) continue;
                    switch (art.Category)
                    {
                        case "GroundCover": roles.low.Add(art); break;
                        case "Shrub": roles.shrubs.Add(art); break;
                        case "Rock": case "Deadwood": roles.rocks.Add(art); break;
                        case "Tree": roles.trees.Add(art); break;
                    }
                }
                return roles;
            }
            public GameObject Find(string name) { foreach (RegionalSceneryArt art in all) if (art.Prefab.name == name) return art.Prefab; return null; }
            public RegionalSceneryArt PickTree(System.Random random) => Pick(trees, random, 99f);
            public RegionalSceneryArt PickShrub(System.Random random) => Pick(shrubs, random, 99f);
            public RegionalSceneryArt PickRock(System.Random random, float maxHeight) => Pick(rocks, random, maxHeight);
            public RegionalSceneryArt PickLow(System.Random random, float maxHeight) => Pick(low, random, maxHeight);
            // Cheaper pieces more often (as SceneryScatter), and only pieces no taller than maxHeight (by their prefab bounds).
            private static RegionalSceneryArt Pick(List<RegionalSceneryArt> list, System.Random random, float maxHeight)
            {
                int Weight(RegionalSceneryArt art) => Mathf.Clamp(Mathf.RoundToInt(200f / Mathf.Max(1, art.Triangles)), 1, 9);
                int total = 0; foreach (RegionalSceneryArt art in list) if (TallestOf(art) <= maxHeight) total += Weight(art);
                if (total == 0) return null;
                int roll = random.Next(total);
                foreach (RegionalSceneryArt art in list) { if (TallestOf(art) > maxHeight) continue; roll -= Weight(art); if (roll < 0) return art; }
                return null;
            }
            private static readonly Dictionary<GameObject, float> heights = new Dictionary<GameObject, float>();
            public static float TallestOf(RegionalSceneryArt art)
            {
                if (heights.TryGetValue(art.Prefab, out float h)) return h;
                h = 0f; foreach (MeshFilter f in art.Prefab.GetComponentsInChildren<MeshFilter>()) if (f.sharedMesh != null) h = Mathf.Max(h, f.sharedMesh.bounds.max.y * f.transform.lossyScale.y);
                heights[art.Prefab] = h; return h;
            }
        }
    }
}
