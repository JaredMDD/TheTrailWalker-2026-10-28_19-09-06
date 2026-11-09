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
        public Transform Root;
    }

    public sealed class SettlementStreet
    {
        public IReadOnlyList<StreetBuilding> Buildings;
        public float MinX, MaxX;   // the camera's scroll range: the first and last plot
        public float ShopsCentreX; // where the camera starts: the middle of the storefront row
        public StreetBuilding Find(string id) { foreach (StreetBuilding b in Buildings) if (b.Id == id) return b; return null; }
    }

    // The settlement street (spec 2026-11-08, section 2): six plots and the storefront row along the X axis, fronts facing the
    // camera on the -Z side. Visual only: selection boxes on building roots are the only colliders.
    public static class SettlementScenery
    {
        public const float PlotSpacing = 11.2f, FrontGap = 1.4f, PlaceholderLumberWidth = 8f;

        // Mirror of Assets/_Game/Art/Settlement/Storefronts/StorefrontManifest.json (the Art folder is not in a player build);
        // SettlementShellValidation asserts that this table equals the manifest. The last row is the lumber frontage.
        public static readonly IReadOnlyList<(string Name, string Front, float Width)> Storefronts = new[]
        {
            ("Warehouse", "WarehouseFront", 8.6f), ("Bakery", "BakeryFront", 6.4f), ("Ranch", "RanchFront", 6.6f), ("Farm", "FarmFront", 7.2f),
            ("Fishery", "FisheryFront", 6.0f), ("Mine", "MineFront", 8.2f), ("Tailor", "TailorFront", 5.4f), ("Lumber", "LumberFront", 7.8f),
        };

        // The art faces +Z at yaw 0; the camera looks along +Z from the street, so fronts turn 180 degrees to face -Z.
        public const float FrontYaw = 180f;
        // Placeholder lumber frontage layout (metres): fence sections along the street, pines behind them.
        private const float FenceLength = 4f, PineDepth = 5f;

        // Scene info from the save: before the plot choice all six houses show level 1; afterwards neighbours show their level
        // and the player's plot stays level 1 until the home is defined (part 2 of milestone 10).
        public static int[] HouseLevels(SettlementState state)
        {
            var levels = new[] { 1, 1, 1, 1, 1, 1 };
            if (state == null) return levels;
            foreach (SettlementState.Neighbour n in state.Neighbours) if (n.Plot >= 0 && n.Plot < levels.Length) levels[n.Plot] = Mathf.Clamp(n.Level, 1, SettlementState.MaxLevel);
            return levels;
        }
        public static int PlayerPlot(SettlementState state) => state == null ? -1 : state.PlotIndex;

        public static SettlementStreet Build(Transform parent, int[] houseLevels, int playerPlot) => Build(parent, houseLevels, playerPlot, null, false);

        // lumberFrontWidthOverride replaces the LumberFront width (so the live branch is testable with another size);
        // forcePlaceholder builds the 8 m fence-and-pines placeholder even though LumberFront exists.
        public static SettlementStreet Build(Transform parent, int[] houseLevels, int playerPlot, float? lumberFrontWidthOverride, bool forcePlaceholder)
            => Build(parent, houseLevels, playerPlot, lumberFrontWidthOverride, forcePlaceholder, Resources.Load<GameObject>);

        // The loader maps a Resources path to a prefab (null when missing); the checks inject one to force the missing-art path.
        // Missing art never throws (a save in the settlement must always load): it logs a warning and builds a placeholder cube.
        public static SettlementStreet Build(Transform parent, int[] houseLevels, int playerPlot, float? lumberFrontWidthOverride, bool forcePlaceholder, System.Func<string, GameObject> load)
        {
            if (houseLevels == null || houseLevels.Length != SettlementState.PlotCount) throw new System.ArgumentException("Six house levels are required.", nameof(houseLevels));
            var street = new GameObject("Settlement street").transform; street.SetParent(parent, false);
            var buildings = new List<StreetBuilding>();

            // Storefront row: left edge at x = 0, each front one gap after the last.
            GameObject lumberFront = forcePlaceholder ? null : load("Settlement/Storefronts/LumberFront");
            float x = 0f, rowStart = 0f, rowEnd = 0f, lumberCentre = 0f, lumberWidth = 0f;
            for (int i = 0; i < Storefronts.Count; i++)
            {
                (string name, string front, float width) = Storefronts[i];
                bool lumber = i == Storefronts.Count - 1;
                if (lumber) width = lumberFront != null ? (lumberFrontWidthOverride ?? width) : PlaceholderLumberWidth;
                float centre = x + width / 2f;
                string id = lumber ? "lumber" : i == 0 ? "warehouse" : "shop." + name.ToLowerInvariant();
                var root = new GameObject(id).transform; root.SetParent(street, false); root.localPosition = new Vector3(centre, 0f, 0f);
                if (lumber) { BuildLumber(root, lumberFront, width, load); lumberCentre = centre; lumberWidth = width; }
                else Place(root, front, "Settlement/Storefronts/" + front, load, width);
                AddSelectionBox(root);
                buildings.Add(new StreetBuilding { Id = id, Kind = lumber ? StreetBuildingKind.LumberField : i == 0 ? StreetBuildingKind.Warehouse : StreetBuildingKind.Shop, CentreX = centre, Root = root });
                if (i == 0) rowStart = centre;
                if (i == Storefronts.Count - 2) rowEnd = centre;
                x += width + FrontGap;   // centre to centre: (this width + next width) / 2 + gap
            }
            float warehouseLeft = buildings[0].CentreX - Storefronts[0].Width / 2f;
            float lumberRight = lumberCentre + lumberWidth / 2f;

            // Plots 0-2 sit left of the warehouse and 3-5 right of the lumber frontage; spacing is 11.2 m centre to centre.
            float plotHalf = PlotSpacing / 2f;
            for (int plot = 0; plot < SettlementState.PlotCount; plot++)
            {
                float centre = plot < 3 ? warehouseLeft - FrontGap - plotHalf - (2 - plot) * PlotSpacing : lumberRight + FrontGap + plotHalf + (plot - 3) * PlotSpacing;
                int level = Mathf.Clamp(houseLevels[plot], 1, SettlementState.MaxLevel);
                var root = new GameObject("plot." + plot).transform; root.SetParent(street, false); root.localPosition = new Vector3(centre, 0f, 0f);
                // A missing house front falls back to the highest lower level that exists, down to level 1, then to a cube.
                int shown = level;
                while (shown > 1 && load($"Settlement/Houses/HouseL{shown:00}Front") == null) shown--;
                Place(root, $"HouseL{shown:00}Front", $"Settlement/Houses/HouseL{shown:00}Front", load, PlotSpacing);
                if (shown != level) Debug.LogWarning($"[Settlement] HouseL{level:00}Front is missing; plot {plot} shows level {shown}.");
                AddSelectionBox(root);
                buildings.Add(new StreetBuilding { Id = "plot." + plot, Kind = StreetBuildingKind.House, Plot = plot, CentreX = centre, Root = root });
            }
            buildings.Sort((a, b) => a.CentreX.CompareTo(b.CentreX));

            float min = float.MaxValue, max = float.MinValue;
            foreach (StreetBuilding b in buildings) if (b.Kind == StreetBuildingKind.House) { min = Mathf.Min(min, b.CentreX); max = Mathf.Max(max, b.CentreX); }
            BuildGround(street, min - PlotSpacing * 8f, max + PlotSpacing * 8f);
            return new SettlementStreet { Buildings = buildings, MinX = min, MaxX = max, ShopsCentreX = (rowStart + rowEnd) / 2f };
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
        private static void BuildLumber(Transform root, GameObject front, float width, System.Func<string, GameObject> load)
        {
            if (front != null) { Place(root, "LumberFront", "Settlement/Storefronts/LumberFront", load, width); return; }
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

        // Flat strip: meadow behind and beside the buildings, a packed-earth road along the fronts.
        private static void BuildGround(Transform parent, float minX, float maxX)
        {
            Material palette = Region01Environment.GroundMaterial, dirt = Region01Environment.TrailMaterial;
            const float cell = 4f, backDepth = 110f, frontDepth = 18f, roadNear = -9f, roadFar = -1.2f;
            const int paletteSize = 24;
            var vertices = new List<Vector3>(); var uvs = new List<Vector2>(); var triangles = new List<int>();
            int columns = Mathf.CeilToInt((maxX - minX) / cell), rows = Mathf.CeilToInt((backDepth + frontDepth) / cell);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    float x0 = minX + c * cell, z0 = -frontDepth + r * cell;
                    float noise = Mathf.PerlinNoise(c * 0.37f + 3f, r * 0.41f + 7f);
                    int color = noise < 0.35f ? 4 : noise > 0.7f ? 6 : 5;
                    int v = vertices.Count;
                    vertices.Add(new Vector3(x0, 0f, z0)); vertices.Add(new Vector3(x0, 0f, z0 + cell)); vertices.Add(new Vector3(x0 + cell, 0f, z0 + cell)); vertices.Add(new Vector3(x0 + cell, 0f, z0));
                    for (int k = 0; k < 4; k++) uvs.Add(new Vector2((color + 0.5f) / paletteSize, 0.5f));
                    triangles.AddRange(new[] { v, v + 1, v + 2, v, v + 2, v + 3 });
                }
            var meadow = new GameObject("Street meadow", typeof(MeshFilter), typeof(MeshRenderer)); meadow.transform.SetParent(parent, false);
            var mesh = new Mesh { name = "Street meadow" }; mesh.SetVertices(vertices); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            meadow.GetComponent<MeshFilter>().sharedMesh = mesh; if (palette != null) meadow.GetComponent<MeshRenderer>().sharedMaterial = palette;

            var road = new GameObject("Street road", typeof(MeshFilter), typeof(MeshRenderer)); road.transform.SetParent(parent, false);
            var roadMesh = new Mesh { name = "Street road" };
            roadMesh.vertices = new[] { new Vector3(minX, 0.02f, roadNear), new Vector3(minX, 0.02f, roadFar), new Vector3(maxX, 0.02f, roadFar), new Vector3(maxX, 0.02f, roadNear) };
            roadMesh.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2((maxX - minX) / 3.4f, 1), new Vector2((maxX - minX) / 3.4f, 0) };
            roadMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 }; roadMesh.RecalculateNormals(); roadMesh.RecalculateBounds();
            road.GetComponent<MeshFilter>().sharedMesh = roadMesh; if (dirt != null) road.GetComponent<MeshRenderer>().sharedMaterial = dirt;
        }
    }

    // Street camera: perspective, facing the fronts from the street side and slightly raised. Scrolls along X, no orbit.
    public sealed class StreetCamera : MonoBehaviour
    {
        public const float Height = 5f, Distance = 22f, LookHeight = 2.6f;
        private SettlementStreet street;
        private float x;

        public float X => x;

        public void Bind(SettlementStreet s) { street = s; CentreOn(s.ShopsCentreX); }
        public void ScrollBy(float metres) => CentreOn(x + metres);
        public void CentreOn(float target)
        {
            x = street == null ? target : Mathf.Clamp(target, street.MinX, street.MaxX);
            Apply();
        }
        private void LateUpdate() { if (street != null) Apply(); }
        private void Apply()
        {
            Vector3 eye = new Vector3(x, Height, -Distance), look = new Vector3(x, LookHeight, 0f);
            transform.SetPositionAndRotation(eye, Quaternion.LookRotation(look - eye));
        }
    }
}
