using System.Collections.Generic;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Settlement street and yard props, phase B of the backdrop plan (handoff
    // Docs/Art/Settlement/settlement-street-props-handoff-2026-11-09.md). Placed deterministically from the buildings' actual
    // extents (their selection boxes), so a house that grows a level never swallows a prop:
    // - a lamp post in every gap between buildings (at the edge of the house gaps, centred in the 1.4 m shop gaps), between the
    //   road and the front line;
    // - the village well in the gap before the Warehouse (at least 3.6 m at house level 8) and the notice board in the gap after
    //   the lumber frontage (the Warehouse gap cannot hold both side by side);
    // - a bench or a barrel-and-crate cluster set back in each house-to-house gap, when it fits;
    // - one yard set behind each house (garden bed, fence corner, and a woodpile or a shed) and a themed yard behind each shop.
    // A prop whose prefab is missing is skipped (the phase A stand-ins stay), so the street works before and after the art lands.
    // Visual only: scripts and colliders are never kept, and readable meshes are combined per material and 30 m chunk.
    public static class SettlementStreetProps
    {
        public const string StreetFolder = "Settlement/Street/", YardFolder = "Settlement/Yards/";
        public static readonly string[] StreetProps = { "StreetLampPost", "VillageWell", "NoticeBoard", "StreetBench", "BarrelCrateCluster" };
        public static readonly string[] YardProps = { "GardenBed", "YardFenceCorner", "Woodpile", "GardenShed", "HayTrough", "CropRows", "DryingRack", "OreCart", "WashingLine" };
        // Handoff budgets (triangles) and heights (metres; the yard kit stays at or under 2.4 m so roofs keep their silhouette).
        public static readonly IReadOnlyDictionary<string, int> TriangleBudget = new Dictionary<string, int>
        {
            { "StreetLampPost", 300 }, { "VillageWell", 900 }, { "NoticeBoard", 400 }, { "StreetBench", 250 }, { "BarrelCrateCluster", 500 },
            { "GardenBed", 400 }, { "YardFenceCorner", 250 }, { "Woodpile", 400 }, { "GardenShed", 500 }, { "HayTrough", 450 },
            { "CropRows", 500 }, { "DryingRack", 350 }, { "OreCart", 450 }, { "WashingLine", 300 },
        };
        public const float YardMaxHeight = 2.4f;
        // The themed yard behind each storefront (the lumber frontage has its own field behind it).
        public static readonly IReadOnlyDictionary<string, string> ShopYards = new Dictionary<string, string>
        {
            { "warehouse", "BarrelCrateCluster" }, { "shop.bakery", "Woodpile" }, { "shop.ranch", "HayTrough" }, { "shop.farm", "CropRows" },
            { "shop.fishery", "DryingRack" }, { "shop.mine", "OreCart" }, { "shop.tailor", "WashingLine" },
        };
        // Street-side depth: the road ends at z = -2.0 and the front line is z = 0 (porches may reach -1.5). Lamps stand in that strip; set-back props
        // (well, bench, cluster) stand behind the front line so none reaches the road.
        public const float LampZ = -1.0f, SetBack = 0.35f, YardStart = 1.2f, Margin = 0.15f;
        // Front gardens (user, November 13, 2026): each house's front band gets a low fence along the road with a gate gap at
        // the path, and garden beds either side of the path. Pieces are labelled "<house id>:garden".
        public const string GardenFence = "Settlement/Route/FenceSection";
        public const float GardenFenceZ = -1.3f, GardenFenceScale = 0.85f, GateWidth = 1.8f, GardenBedZ = 0.3f;
        public static string GardenLabel(string houseId) => houseId + ":garden";

        public static string PathOf(string prop) => (System.Array.IndexOf(YardProps, prop) >= 0 ? YardFolder : StreetFolder) + prop;

        public sealed class Placed { public string Prop, Building; public Bounds Bounds; }   // bounds in street space
        public sealed class Result
        {
            public Transform Root;
            public readonly List<Placed> Pieces = new List<Placed>();
            public readonly List<string> Missing = new List<string>();
            public int Instanced; public bool CombinedUnreadable;
            // The house yard set exists (garden bed and fence corner), so the backdrop drops its phase A yard stand-ins.
            public bool YardKit;
        }

        public static Result Build(Transform street, IReadOnlyList<StreetBuilding> buildings, int seed, System.Func<string, GameObject> load)
        {
            var result = new Result { Root = new GameObject("Settlement street props").transform };
            result.Root.SetParent(street, false);
            var random = new System.Random(seed);
            var prefabs = new Dictionary<string, GameObject>();
            foreach (string name in StreetProps) prefabs[name] = load(PathOf(name));
            foreach (string name in YardProps) prefabs[name] = load(PathOf(name));
            foreach (var pair in prefabs) if (pair.Value == null) result.Missing.Add(pair.Key);
            prefabs[GardenFence] = load(GardenFence);   // route art, not part of the 14 street and yard props
            result.YardKit = prefabs["GardenBed"] != null && prefabs["YardFenceCorner"] != null;
            if (result.Missing.Count == StreetProps.Length + YardProps.Length) return result;   // nothing has landed yet

            var groups = new Dictionary<(Material, int), List<CombineInstance>>();
            var sorted = new List<StreetBuilding>(buildings); sorted.Sort((a, b) => a.CentreX.CompareTo(b.CentreX));

            // Footprint of a prop as placed (yaw 180 plus a small jitter): half width in X, the near and far Z, and height.
            Bounds Local(GameObject prefab)
            {
                bool any = false; var bounds = new Bounds();
                Matrix4x4 toRoot = prefab.transform.worldToLocalMatrix;
                foreach (MeshFilter f in prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (f.sharedMesh == null) continue;
                    Bounds b = f.sharedMesh.bounds; Matrix4x4 m = toRoot * f.transform.localToWorldMatrix;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 p = m.MultiplyPoint3x4(new Vector3(i % 2 == 0 ? b.min.x : b.max.x, i / 2 % 2 == 0 ? b.min.y : b.max.y, i / 4 == 0 ? b.min.z : b.max.z));
                        if (!any) { bounds = new Bounds(p, Vector3.zero); any = true; } else bounds.Encapsulate(p);
                    }
                }
                return bounds;
            }
            float HalfWidth(GameObject prefab) { Bounds b = Local(prefab); return Mathf.Max(Mathf.Abs(b.min.x), Mathf.Abs(b.max.x), Mathf.Abs(b.min.z), Mathf.Abs(b.max.z)); }

            void Add(string prop, string building, Vector3 position, float yaw, Vector3? scale = null)
            {
                GameObject prefab = prefabs[prop];
                if (prefab == null) return;
                Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
                Matrix4x4 placement = Matrix4x4.TRS(position, rotation, scale ?? Vector3.one);
                Bounds local = Local(prefab), world = new Bounds(placement.MultiplyPoint3x4(local.center), Vector3.zero);
                for (int i = 0; i < 8; i++)
                    world.Encapsulate(placement.MultiplyPoint3x4(new Vector3(i % 2 == 0 ? local.min.x : local.max.x, i / 2 % 2 == 0 ? local.min.y : local.max.y, i / 4 == 0 ? local.min.z : local.max.z)));
                result.Pieces.Add(new Placed { Prop = prop, Building = building, Bounds = world });

                bool readable = true;
                foreach (MeshFilter f in prefab.GetComponentsInChildren<MeshFilter>(true)) if (f.sharedMesh != null && !f.sharedMesh.isReadable) readable = false;
                if (!readable) { Transform whole = SettlementBackdrop.PlaceWhole(result.Root, prefab, prop, position, yaw, 1f); if (whole != null && scale.HasValue) whole.localScale = scale.Value; result.Instanced++; return; }
                int chunk = Mathf.FloorToInt(position.x / SettlementBackdrop.Chunk);
                foreach (MeshFilter f in prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    var renderer = f.GetComponent<MeshRenderer>();
                    if (f.sharedMesh == null || renderer == null || renderer.sharedMaterial == null) continue;
                    if (!f.sharedMesh.isReadable) result.CombinedUnreadable = true;
                    var key = (renderer.sharedMaterial, chunk);
                    if (!groups.TryGetValue(key, out List<CombineInstance> list)) groups[key] = list = new List<CombineInstance>();
                    list.Add(new CombineInstance { mesh = f.sharedMesh, transform = result.Root.worldToLocalMatrix * street.localToWorldMatrix * placement * prefab.transform.worldToLocalMatrix * f.transform.localToWorldMatrix });
                }
            }
            float Jitter(float degrees) => (float)(random.NextDouble() * 2.0 - 1.0) * degrees;

            // Street dressing, gap by gap.
            int houseGap = 0;
            for (int i = 0; i + 1 < sorted.Count; i++)
            {
                StreetBuilding a = sorted[i], b = sorted[i + 1];
                float left = a.FrontRight, right = b.FrontLeft, width = right - left, mid = (left + right) / 2f;   // front band: rear wings don't count
                string label = a.Id + "|" + b.Id;
                if (a.Id == "plot.2" && b.Id == "warehouse")
                {
                    if (Fits("VillageWell", width)) Add("VillageWell", label, new Vector3(mid, 0f, SetBack + Local(prefabs["VillageWell"]).extents.z), 180f);
                    continue;
                }
                if (a.Id == "lumber" && b.Id == "plot.3")
                {
                    if (Fits("NoticeBoard", width)) Add("NoticeBoard", label, new Vector3(mid, 0f, SetBack + Local(prefabs["NoticeBoard"]).extents.z), 180f);
                    continue;
                }
                bool houses = a.Kind == StreetBuildingKind.House && b.Kind == StreetBuildingKind.House;
                GameObject lamp = prefabs["StreetLampPost"];
                if (!houses) { if (Fits("StreetLampPost", width)) Add("StreetLampPost", label, new Vector3(mid, 0f, LampZ), 180f); continue; }
                // House gaps (4.4-6.6 m): the lamp at the left edge, a bench or a cluster centred in what is left, set back.
                float lampHalf = lamp != null ? HalfWidth(lamp) : 0f;
                if (lamp != null && width >= 2f * (lampHalf + Margin)) Add("StreetLampPost", label, new Vector3(left + Margin + lampHalf, 0f, LampZ), 180f);
                string seat = houseGap++ % 2 == 0 ? "StreetBench" : "BarrelCrateCluster";
                float from = left + (lamp != null ? 2f * (lampHalf + Margin) : 0f);
                if (prefabs[seat] != null && right - from >= 2f * (HalfWidth(prefabs[seat]) + Margin))
                    Add(seat, label, new Vector3((from + right) / 2f, 0f, SetBack + Local(prefabs[seat]).extents.z), 180f);
            }

            // Yards behind the buildings.
            foreach (StreetBuilding building in sorted)
            {
                (float l, float r, float back) = Extent(building);
                float cx = (l + r) / 2f, yard = back + YardStart;
                if (building.Kind == StreetBuildingKind.House)
                {
                    Add("GardenBed", building.Id, new Vector3(cx - 1.3f, 0f, yard + 1.6f), 180f + Jitter(4f));
                    Add("YardFenceCorner", building.Id, new Vector3(cx - 1.8f, 0f, yard + 4.4f), 180f);
                    Add(building.Plot % 2 == 0 ? "Woodpile" : "GardenShed", building.Id, new Vector3(cx + 2.0f, 0f, yard + 2.2f), 180f + Jitter(8f));
                }
                else if (ShopYards.TryGetValue(building.Id, out string prop))
                    Add(prop, building.Id, new Vector3(cx, 0f, yard + 2.0f), 180f + Jitter(8f));
            }

            // Front gardens between the road and each house, inside the house's front band.
            GameObject fencePrefab = prefabs[GardenFence], bed = prefabs["GardenBed"];
            float fenceLength = 0f;
            if (fencePrefab != null) { Bounds fb = Local(fencePrefab); fenceLength = fb.size.x; }
            foreach (StreetBuilding house in sorted)
            {
                if (house.Kind != StreetBuildingKind.House) continue;
                string label = GardenLabel(house.Id);
                float gateLeft = house.CentreX - GateWidth / 2f, gateRight = house.CentreX + GateWidth / 2f;
                if (fencePrefab != null)
                    foreach ((float from, float to) in new[] { (house.FrontLeft, gateLeft), (gateRight, house.FrontRight) })
                    {
                        // Equal sections no longer than the art (stretched slightly in X to fit), so a run always ends on a post.
                        float run = to - from;
                        if (run < 0.4f) continue;
                        int count = Mathf.CeilToInt(run / (fenceLength * GardenFenceScale) - 0.01f);
                        float length = run / count;
                        for (int i = 0; i < count; i++)
                            Add(GardenFence, label, new Vector3(from + length * (i + 0.5f), 0f, GardenFenceZ), 0f, new Vector3(length / fenceLength, GardenFenceScale, 1f));
                    }
                if (bed != null)
                {
                    // Beds turned side-on (1.3 m wide, 2 m deep) in rows either side of the path.
                    Bounds bb = Local(bed); float bedWidth = bb.size.z, step = bedWidth + 0.35f;
                    float bedZ = Mathf.Min(GardenBedZ + bb.extents.x, house.FrontZ - SettlementScenery.FrontLine - 0.1f - bb.extents.x);   // clear of the porch
                    foreach (int sign in new[] { -1, 1 })
                    {
                        float inner = house.CentreX + sign * (SettlementScenery.GardenPathWidth / 2f + 0.3f), outer = sign < 0 ? house.FrontLeft : house.FrontRight;
                        int n = Mathf.FloorToInt((Mathf.Abs(outer - inner) + step - bedWidth + 0.01f) / step);
                        for (int i = 0; i < n; i++)
                            Add("GardenBed", label, new Vector3(inner + sign * (bedWidth / 2f + i * step), 0f, bedZ), 90f);
                    }
                }
            }

            foreach (var group in groups)
                SettlementBackdrop.Combine(result.Root, $"{group.Key.Item1.name} props chunk {group.Key.Item2}", group.Value, group.Key.Item1, true);
            return result;

            bool Fits(string prop, float gap) => prefabs[prop] != null && gap >= 2f * (HalfWidth(prefabs[prop]) + Margin);
        }

        // A building's X extent and back edge in street space, from its selection box.
        public static (float Left, float Right, float Back) Extent(StreetBuilding building)
        {
            var box = building.Root.GetComponent<BoxCollider>();
            if (box == null) return (building.CentreX - 2f, building.CentreX + 2f, building.FrontZ + 4f);
            float cx = building.CentreX + box.center.x, half = box.size.x / 2f;
            return (cx - half, cx + half, building.FrontZ + box.center.z + box.size.z / 2f);
        }
    }
}
