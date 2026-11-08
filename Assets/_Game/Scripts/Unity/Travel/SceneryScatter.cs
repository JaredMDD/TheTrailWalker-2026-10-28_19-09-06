using System;
using System.Collections.Generic;
using TheTrailWalker.Movement;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Segment dressing from the tier-one regional environment library (October 31, 2026 art pass): each road is dressed
    // with its own region's kit, so Windward Hills no longer reuses the Dawn Plains pieces. Two passes:
    //  - roadside: ground cover, shrubs, a few pines and rocks every few metres along each side of each road;
    //  - field: a jittered grid out to 65 m with grass near the road, shrubs/rocks in the middle distance and pine groves
    //    further out.
    // Visual only: no colliders, deterministic per segment. Pieces are combined per material and shadow group (tall
    // casters separate from non-casting cover), never across region palettes (each kit has its own 32-colour material).
    // Terrain-replacement and distant-only kit pieces are never scattered: they need authored stitching.
    public static class SceneryScatter
    {
        private const float Cell = 5f, PickupClear = 6f, CampClear = 12f, AnchorClear = 4.5f, Reach = 65f, RoadsideSpacing = 5.5f;

        private static Tier01EnvironmentLibrary library;
        public static Tier01EnvironmentLibrary Library => library != null ? library : library = Resources.Load<Tier01EnvironmentLibrary>("Environment/Tier01/Tier01EnvironmentLibrary");

        // Tier-two kits (regions 7-12, November 6, 2026) live in their own library asset of the same type.
        private static Tier01EnvironmentLibrary tierTwo;
        public static Tier01EnvironmentLibrary TierTwoLibrary => tierTwo != null ? tierTwo : tierTwo = Resources.Load<Tier01EnvironmentLibrary>("Environment/Tier02/Tier02EnvironmentLibrary");

        public static RegionalEnvironmentKit Kit(int regionNumber)
        {
            foreach (Tier01EnvironmentLibrary source in new[] { Library, TierTwoLibrary })
            {
                if (source == null || source.Regions == null) continue;
                foreach (RegionalEnvironmentKit kit in source.Regions) if (kit != null && kit.Number == regionNumber) return kit;
            }
            return null;
        }

        // Kit pieces sorted into the roles the scatter fills. Tall shrubs are the shadow-casting ones.
        private sealed class Palette
        {
            public readonly List<RegionalSceneryArt> Cover = new List<RegionalSceneryArt>(), LowShrubs = new List<RegionalSceneryArt>(),
                TallShrubs = new List<RegionalSceneryArt>(), Rocks = new List<RegionalSceneryArt>(), Trees = new List<RegionalSceneryArt>();
            public static Palette Of(RegionalEnvironmentKit kit)
            {
                var palette = new Palette();
                if (kit?.Models == null) return palette;
                foreach (RegionalSceneryArt art in kit.Models)
                {
                    if (art == null || art.Prefab == null || art.TerrainReplacement || art.DistantOnly) continue;
                    switch (art.Category)
                    {
                        case "GroundCover": palette.Cover.Add(art); break;
                        case "Shrub": (art.CastsShadows ? palette.TallShrubs : palette.LowShrubs).Add(art); break;
                        case "Rock": case "Deadwood": palette.Rocks.Add(art); break;
                        case "Tree": palette.Trees.Add(art); break;
                    }
                }
                if (palette.TallShrubs.Count == 0) palette.TallShrubs.AddRange(palette.LowShrubs);
                return palette;
            }
            public bool Empty => Cover.Count + LowShrubs.Count + Rocks.Count + Trees.Count == 0;
        }

        // roads[i] is dressed with the kit of regionNumbers[i].
        public static void Build(Transform parent, IReadOnlyList<ITrailPath> roads, IReadOnlyList<int> regionNumbers, IReadOnlyList<Vector3> camps,
            IReadOnlyList<Vector3> anchors, Func<Vector3, float> groundHeight, string seedName)
        {
            if (roads.Count == 0) return;
            var palettes = new Palette[roads.Count];
            for (int i = 0; i < roads.Count; i++) palettes[i] = Palette.Of(Kit(regionNumbers[i]));

            // Road samples every 2 m (with the road each belongs to); never past the open ends (rivers, map edge).
            var samples = new List<Vector2>(); var heights = new List<float>(); var owner = new List<int>();
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            for (int r = 0; r < roads.Count; r++)
            {
                ITrailPath road = roads[r];
                int count = Mathf.CeilToInt(road.Length / 2f) + 1;
                for (int i = 0; i < count; i++)
                {
                    road.Evaluate(road.Length * i / (count - 1), out Vector3 p, out _);
                    var flat = new Vector2(p.x, p.z); samples.Add(flat); heights.Add(p.y); owner.Add(r);
                    min = Vector2.Min(min, flat); max = Vector2.Max(max, flat);
                }
            }
            uint seed = 2166136261; foreach (char c in seedName) { seed ^= c; seed *= 16777619; }
            var random = new System.Random((int)(seed & 0x7fffffff));
            var groups = new Dictionary<(Material, bool), List<CombineInstance>>();
            var root = new GameObject("Regional scenery"); root.transform.SetParent(parent, false);

            float RoadDistance(Vector2 here, out int nearest)
            {
                nearest = 0; float best = float.MaxValue;
                for (int i = 0; i < samples.Count; i++) { float d = (samples[i] - here).sqrMagnitude; if (d < best) { best = d; nearest = i; } }
                return Mathf.Sqrt(best);
            }
            // Cheaper pieces are picked more often (weight ~200/triangles, 1-9): a carpet of grass with some flowers and
            // ferns, as before, instead of letting 500-800-triangle clumps dominate the ground cover.
            RegionalSceneryArt Pick(List<RegionalSceneryArt> list)
            {
                if (list.Count == 0) return null;
                int Weight(RegionalSceneryArt art) => Mathf.Clamp(Mathf.RoundToInt(200f / Mathf.Max(1, art.Triangles)), 1, 9);
                int total = 0; foreach (RegionalSceneryArt art in list) total += Weight(art);
                int roll = random.Next(total);
                foreach (RegionalSceneryArt art in list) { roll -= Weight(art); if (roll < 0) return art; }
                return list[list.Count - 1];
            }

            // Places a piece if it clears every road by its catalog clearance (scaled), the camps and the interactables.
            void Place(RegionalSceneryArt art, Vector3 position, float scale)
            {
                if (art == null) return;
                var here = new Vector2(position.x, position.z);
                float clearance = art.RecommendedRoadClearance + art.FootprintRadius * (scale - 1f);
                if (RoadDistance(here, out _) < clearance) return;
                if (Near(position, camps, CampClear) || Near(position, anchors, AnchorClear)) return;
                position.y = groundHeight(position);
                Matrix4x4 placement = Matrix4x4.TRS(position, Quaternion.Euler(0, (float)random.NextDouble() * 360f, 0), Vector3.one * scale);
                foreach (MeshFilter filter in art.Prefab.GetComponentsInChildren<MeshFilter>())
                {
                    var renderer = filter.GetComponent<MeshRenderer>();
                    if (filter.sharedMesh == null || renderer == null || renderer.sharedMaterial == null) continue;
                    var key = (renderer.sharedMaterial, art.CastsShadows);
                    if (!groups.TryGetValue(key, out List<CombineInstance> list)) groups[key] = list = new List<CombineInstance>();
                    list.Add(new CombineInstance { mesh = filter.sharedMesh, transform = root.transform.worldToLocalMatrix * placement * filter.transform.localToWorldMatrix });
                }
            }
            float Range(float from, float to) => from + (float)random.NextDouble() * (to - from);

            // Roadside pass along each road and both sides.
            for (int r = 0; r < roads.Count; r++)
            {
                Palette kit = palettes[r]; if (kit.Empty) continue;
                ITrailPath road = roads[r];
                for (float d = 25f; d < road.Length - 25f; d += RoadsideSpacing)
                {
                    road.Evaluate(d, out Vector3 p, out Vector3 t); Vector3 side = Vector3.Cross(Vector3.up, t).normalized;
                    for (int sign = -1; sign <= 1; sign += 2)
                    {
                        Place(Pick(random.Next(3) == 0 ? kit.TallShrubs : kit.LowShrubs), p + side * sign * Range(7f, 13f), Range(0.75f, 1.2f));
                        RegionalSceneryArt cover = Pick(kit.Cover);
                        if (cover != null) Place(cover, p + side * sign * Range(cover.RecommendedRoadClearance + 0.3f, cover.RecommendedRoadClearance + 1.6f), 1f);
                        if (random.Next(3) == 0) Place(Pick(kit.Trees), p + side * sign * Range(19f, 31f), Range(0.8f, 1.15f));
                        if (random.Next(9) == 0) Place(Pick(kit.Rocks), p + side * sign * Range(14f, 16f), 0.8f);
                        if (random.Next(8) == 0) Place(Pick(kit.LowShrubs), p + side * sign * Range(12f, 14f), 0.8f);
                    }
                }
            }

            // Field pass: each grid point uses the kit of the road it is nearest to.
            for (float x = min.x - Reach; x <= max.x + Reach; x += Cell)
                for (float z = min.y - Reach; z <= max.y + Reach; z += Cell)
                {
                    var here = new Vector2(x + (float)random.NextDouble() * Cell, z + (float)random.NextDouble() * Cell);
                    float distance = RoadDistance(here, out int nearest);
                    if (distance < PickupClear || distance > Reach || nearest == 0 || nearest == samples.Count - 1) continue;
                    Palette kit = palettes[owner[nearest]];
                    var position = new Vector3(here.x, heights[nearest], here.y);
                    float grove = Mathf.PerlinNoise(here.x * 0.03f + 7f, here.y * 0.03f + 3f), roll = (float)random.NextDouble();
                    float scale = 0.8f + (float)random.NextDouble() * 0.5f;
                    List<RegionalSceneryArt> role;
                    if (distance < 14f)
                        role = roll < 0.63f ? kit.Cover : roll < 0.84f ? kit.LowShrubs : null;
                    else if (distance < 32f)
                        role = roll < 0.30f ? kit.Cover : roll < 0.48f ? kit.LowShrubs : roll < 0.60f ? kit.TallShrubs : roll < 0.68f ? kit.LowShrubs
                             : roll < 0.68f + 0.18f * grove ? kit.Trees : roll < 0.90f ? kit.Rocks : null;
                    else if (grove > 0.55f)
                        role = roll < 0.75f ? kit.Trees : roll < 0.9f ? kit.TallShrubs : null;
                    else
                        role = roll < 0.25f ? kit.TallShrubs : roll < 0.40f ? kit.Cover : roll < 0.52f ? kit.Rocks : roll < 0.58f ? kit.LowShrubs : null;
                    if (role != null) Place(Pick(role), position, scale);
                }

            foreach (var group in groups)
                Combine(root.transform, $"{group.Key.Item1.name} ({(group.Key.Item2 ? "shadow casters" : "no shadows")})", group.Value, group.Key.Item1, group.Key.Item2);
        }

        private static void Combine(Transform parent, string name, List<CombineInstance> instances, Material material, bool castShadows)
        {
            if (instances.Count == 0) return;
            var part = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); part.transform.SetParent(parent, false);
            var mesh = new Mesh { name = name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.CombineMeshes(instances.ToArray(), true, true); mesh.RecalculateBounds();
            part.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static bool Near(Vector3 position, IReadOnlyList<Vector3> points, float radius)
        {
            foreach (Vector3 point in points)
            {
                float dx = point.x - position.x, dz = point.z - position.z;
                if (dx * dx + dz * dz < radius * radius) return true;
            }
            return false;
        }
    }
}
