using System;
using UnityEngine;

namespace TheTrailWalker.Movement
{
    public interface ITrailPath
    {
        float Length { get; }
        void Evaluate(float distance, out Vector3 position, out Vector3 tangent);
    }

    // Catmull-Rom authoring, followed by a cached arc-length lookup. No per-frame allocations.
    public sealed class SampledTrailPath : ITrailPath
    {
        private readonly Vector3[] points;
        private readonly float[] distances;
        public float Length => distances[distances.Length - 1];

        public SampledTrailPath(Vector3[] knots, int subdivisions = 40)
        {
            if (knots == null || knots.Length < 2 || subdivisions < 2) throw new ArgumentException("A trail requires at least two knots and two subdivisions.");
            points = new Vector3[(knots.Length - 1) * subdivisions + 1];
            distances = new float[points.Length];
            points[0] = knots[0];
            int index = 1;
            for (int segment = 0; segment < knots.Length - 1; segment++)
            {
                Vector3 a = knots[Math.Max(0, segment - 1)], b = knots[segment];
                Vector3 c = knots[segment + 1], d = knots[Math.Min(knots.Length - 1, segment + 2)];
                for (int step = 1; step <= subdivisions; step++, index++)
                {
                    float t = (float)step / subdivisions;
                    points[index] = 0.5f * ((2f * b) + (-a + c) * t + (2f * a - 5f * b + 4f * c - d) * t * t + (-a + 3f * b - 3f * c + d) * t * t * t);
                    distances[index] = distances[index - 1] + Vector3.Distance(points[index - 1], points[index]);
                }
            }
            if (Length < 0.01f) throw new ArgumentException("Trail has no traversable length.");
        }

        public void Evaluate(float distance, out Vector3 position, out Vector3 tangent)
        {
            distance = Mathf.Clamp(distance, 0f, Length);
            int lo = 1, hi = distances.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (distances[mid] < distance) lo = mid + 1;
                else hi = mid;
            }
            float span = distances[lo] - distances[lo - 1];
            position = Vector3.Lerp(points[lo - 1], points[lo], span > 0f ? (distance - distances[lo - 1]) / span : 0f);
            tangent = (points[lo] - points[lo - 1]).normalized;
            if (tangent.sqrMagnitude < 0.5f) tangent = Vector3.forward;
        }
    }
}
