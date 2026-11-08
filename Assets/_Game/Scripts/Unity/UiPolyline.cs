using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // A thin line through screen-space points, drawn as one UI mesh (slingshot bands, fishing line). Points are given
    // in screen pixels for an overlay canvas and converted to this graphic's local space; thickness is in pixels.
    public sealed class UiPolyline : MaskableGraphic
    {
        private readonly List<Vector2> points = new List<Vector2>();
        private float thickness = 3f;

        public void SetScreenPoints(IList<Vector2> screen, float thicknessPixels)
        {
            points.Clear();
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            foreach (Vector2 p in screen) points.Add(rectTransform.InverseTransformPoint(p));
            thickness = thicknessPixels / Mathf.Max(0.01f, scale);
            SetVerticesDirty();
        }

        // Points as 0-1 fractions of this graphic's rect (map road); they follow the rect when it is resized.
        private readonly List<Vector2> normalized = new List<Vector2>();
        public void SetNormalizedPoints(IList<Vector2> fractions, float thicknessUnits)
        {
            normalized.Clear(); normalized.AddRange(fractions); thickness = thicknessUnits;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (normalized.Count > 0)
            {
                Rect r = rectTransform.rect; points.Clear();
                foreach (Vector2 f in normalized) points.Add(new Vector2(r.xMin + f.x * r.width, r.yMin + f.y * r.height));
            }
            if (points.Count < 2) return;
            float half = thickness * 0.5f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i], b = points[i + 1], along = b - a;
                if (along.sqrMagnitude < 1e-6f) continue;
                Vector2 normal = new Vector2(-along.y, along.x).normalized * half;
                // Extend each quad by half a thickness so joints between segments have no gaps.
                Vector2 extend = along.normalized * half * 0.5f;
                int start = mesh.currentVertCount;
                mesh.AddVert(a - extend - normal, color, Vector2.zero); mesh.AddVert(a - extend + normal, color, Vector2.zero);
                mesh.AddVert(b + extend + normal, color, Vector2.zero); mesh.AddVert(b + extend - normal, color, Vector2.zero);
                mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
            }
        }
    }
}
