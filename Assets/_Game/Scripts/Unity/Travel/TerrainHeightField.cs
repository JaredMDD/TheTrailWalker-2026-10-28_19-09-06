using System.Collections.Generic;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Ground height lookup over the built terrain mesh, without a collider (user, November 5, 2026: trail pickups
    // floated above the redrawn ground). A collider would block the pickup and harvest touch raycasts, which use every
    // layer, so the terrain triangles are bucketed on a coarse XZ grid and the height is interpolated inside the triangle.
    public sealed class TerrainHeightField
    {
        private const float Bucket = 4f;
        private readonly Vector3[] vertices;
        private readonly int[] triangles;
        private readonly Dictionary<long, List<int>> buckets = new Dictionary<long, List<int>>();

        public TerrainHeightField(MeshFilter terrain)
        {
            Mesh mesh = terrain.sharedMesh;
            Vector3[] local = mesh.vertices; vertices = new Vector3[local.Length];
            for (int i = 0; i < local.Length; i++) vertices[i] = terrain.transform.TransformPoint(local[i]);
            triangles = mesh.triangles;
            for (int t = 0; t < triangles.Length; t += 3)
            {
                Vector3 a = vertices[triangles[t]], b = vertices[triangles[t + 1]], c = vertices[triangles[t + 2]];
                int x0 = Cell(Mathf.Min(a.x, Mathf.Min(b.x, c.x))), x1 = Cell(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
                int z0 = Cell(Mathf.Min(a.z, Mathf.Min(b.z, c.z))), z1 = Cell(Mathf.Max(a.z, Mathf.Max(b.z, c.z)));
                for (int x = x0; x <= x1; x++)
                    for (int z = z0; z <= z1; z++)
                    {
                        if (!buckets.TryGetValue(Key(x, z), out List<int> list)) buckets[Key(x, z)] = list = new List<int>();
                        list.Add(t);
                    }
            }
        }

        private static int Cell(float value) => Mathf.FloorToInt(value / Bucket);
        private static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;

        // Ground height under the point, or false outside the terrain.
        public bool TryHeight(Vector3 at, out float height)
        {
            height = 0;
            if (!buckets.TryGetValue(Key(Cell(at.x), Cell(at.z)), out List<int> list)) return false;
            foreach (int t in list)
            {
                Vector3 a = vertices[triangles[t]], b = vertices[triangles[t + 1]], c = vertices[triangles[t + 2]];
                float d = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
                if (Mathf.Abs(d) < 1e-8f) continue;
                float u = ((b.z - c.z) * (at.x - c.x) + (c.x - b.x) * (at.z - c.z)) / d;
                float v = ((c.z - a.z) * (at.x - c.x) + (a.x - c.x) * (at.z - c.z)) / d;
                float w = 1f - u - v;
                const float edge = -1e-4f;
                if (u < edge || v < edge || w < edge) continue;
                height = u * a.y + v * b.y + w * c.y;
                return true;
            }
            return false;
        }

        // Lowest ground under a round footprint (centre plus eight points on its rim), like the interactables use, so
        // the downhill edge of a pickup on a slope never hangs in the air.
        public bool TryLowest(Vector3 centre, float radius, out float lowest)
        {
            if (!TryHeight(centre, out lowest)) return false;
            for (int i = 0; i < 8; i++)
                if (TryHeight(centre + Quaternion.AngleAxis(i * 45f, Vector3.up) * Vector3.forward * radius, out float h)) lowest = Mathf.Min(lowest, h);
            return true;
        }
    }
}
