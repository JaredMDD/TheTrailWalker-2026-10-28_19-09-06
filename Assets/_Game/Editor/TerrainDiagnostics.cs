using System.Collections.Generic;
using System.IO;
using System.Text;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Movement;
using TheTrailWalker.Travel;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Terrain report (user, November 2, 2026: roads and interactables looked raised above the ground on hill legs, and
    // the road dipped into camps in a V). Per segment: steepest grade, road height above the ground beside it, and the
    // grade arriving at / leaving each camp. Written to Builds/Validation/terrain-report.txt.
    public static class TerrainDiagnostics
    {
        [MenuItem("The Trail Walker/Validation/Terrain Report")]
        public static void Report()
        {
            var catalog = TravelCatalog.Prototype();
            var sb = new StringBuilder("segment | relief | max grade | road above ground: edge 1.7 m / 3 m / 6 m (max) | grade in at end | next leg grade out\n");
            foreach (var region in catalog.Regions)
                foreach (var camp in region.Camps)
                {
                    SegmentDefinition segment = catalog.Outgoing(camp.Id);
                    if (segment == null) continue;
                    var path = new SampledTrailPath(TravelScenery.KnotsFor(segment));
                    Mesh mesh = TravelScenery.TerrainMesh(path, 70f, 3f, TravelScenery.Undulation(segment), true);
                    var probe = new GameObject("Terrain probe"); var collider = probe.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
                    float maxGrade = 0, edge = 0, three = 0, six = 0, sum6 = 0, sumC = 0; int n6 = 0, nC = 0; float worstAt = 0, centreWorst = 0;
                    try
                    {
                        for (float d = 1; d < path.Length - 1; d += 1f)
                        {
                            path.Evaluate(d - 1, out Vector3 a, out _); path.Evaluate(d + 1, out Vector3 b, out _);
                            maxGrade = Mathf.Max(maxGrade, Mathf.Abs(b.y - a.y) / 2f);
                            path.Evaluate(d, out Vector3 p, out Vector3 t);
                            Vector3 side = Vector3.Cross(Vector3.up, t).normalized;
                            if (collider.Raycast(new Ray(p + Vector3.up * 60f, Vector3.down), out RaycastHit centre, 200f)) { sumC += p.y - centre.point.y; nC++; centreWorst = Mathf.Max(centreWorst, p.y - centre.point.y); }
                            foreach (var (offset, slot) in new[] { (1.7f, 0), (3f, 1), (6f, 2) })
                                foreach (float s in new[] { -1f, 1f })
                                {
                                    Vector3 q = p + side * offset * s;
                                    if (!collider.Raycast(new Ray(q + Vector3.up * 60f, Vector3.down), out RaycastHit hit, 200f)) continue;
                                    float above = p.y - hit.point.y;
                                    if (slot == 2) { sum6 += above; n6++; if (above > six) worstAt = d; }
                                    if (slot == 0) edge = Mathf.Max(edge, above); else if (slot == 1) three = Mathf.Max(three, above); else six = Mathf.Max(six, above);
                                }
                        }
                    }
                    finally { Object.DestroyImmediate(probe); Object.DestroyImmediate(mesh); }
                    path.Evaluate(path.Length - 10, out Vector3 e0, out _); path.Evaluate(path.Length, out Vector3 e1, out _);
                    float gradeIn = (e1.y - e0.y) / 10f, gradeOut = 0;
                    SegmentDefinition next = segment.RiverApproach ? null : catalog.Outgoing(segment.ToCampId);
                    if (next != null)
                    {
                        var nextPath = new SampledTrailPath(TravelScenery.KnotsFor(next));
                        nextPath.Evaluate(0, out Vector3 s0, out _); nextPath.Evaluate(10, out Vector3 s1, out _); gradeOut = (s1.y - s0.y) / 10f;
                    }
                    sb.AppendLine($"{segment.Id} | {segment.Relief:0.#} | {maxGrade:0.00} | {edge:0.00} / {three:0.00} / {six:0.00} | {gradeIn:+0.00;-0.00} | {gradeOut:+0.00;-0.00} | mean 6 m {sum6 / Mathf.Max(1, n6):0.00} worst at {worstAt:0} m of {path.Length:0} | centre mean {sumC / Mathf.Max(1, nC):0.00} max {centreWorst:0.00}");
                }
            Directory.CreateDirectory("Builds/Validation");
            File.WriteAllText("Builds/Validation/terrain-report.txt", sb.ToString());
            Debug.Log("Terrain report written to Builds/Validation/terrain-report.txt");
        }
    }
}
