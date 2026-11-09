using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Movement;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Regions 3-4 and the settlement route (spec 2026-11-08): Mistwood and Fern Hollow catalogue data, the settlement
    // approach and arch return, region looks (fog, valley) and the approach art. Chained after Milestone 9 in the suite.
    public static class TierOneRegionsValidation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Tier-one regions: " + name); passed.Add(name); }
        private static bool InvalidData(Action action) { try { action(); return false; } catch (InvalidDataException) { return true; } }
        private static bool Throws(Action action) { try { action(); return false; } catch (ArgumentException) { return true; } catch (InvalidOperationException) { return true; } }

        [MenuItem("The Trail Walker/Regions 3-4/Validate")]
        public static void Run()
        {
            passed.Clear();
            CatalogueChecks();
            SettlementRouteChecks();
            LookChecks();
            ValleyChecks();
            RiverChecks();
            SettlementArchAssets.Validate(Check);
            ApproachSceneChecks();
            MapTabChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/TierOne-regions-checks.txt", passed);
        }

        private static List<SegmentDefinition> Legs(TravelCatalog catalog, RegionDefinition region) =>
            region.Camps.Select(c => catalog.Outgoing(c.Id)).Where(s => !s.RiverApproach).ToList();

        private static void RegionShape(TravelCatalog catalog, string id, string name, int number, string[] names, CampSide first, float[] lengths, float[] relief, string[] featured, long toll)
        {
            RegionDefinition region = catalog.Region(id);
            Check(region != null && region.Name == name && region.Number == number, $"{name} is region {number}");
            Check(region.Camps.Select(c => c.Name).SequenceEqual(names), $"{name} has its {names.Length} camps in order");
            bool ids = true, sides = true;
            for (int i = 0; i < region.Camps.Count; i++)
            {
                ids &= region.Camps[i].Id == $"camp.{number:00}.{i + 1:00}";
                sides &= region.Camps[i].Side == (i % 2 == 0 ? first : (first == CampSide.Left ? CampSide.Right : CampSide.Left));
            }
            Check(ids && sides, $"{name} camps use IDs camp.{number:00}.NN and alternate sides starting {first}");
            List<SegmentDefinition> legs = Legs(catalog, region);
            bool legIds = true; for (int i = 0; i < legs.Count; i++) legIds &= legs[i].Id == $"segment.{number:00}.{i + 1:00}";
            Check(legIds && legs.Select(s => s.Length).SequenceEqual(lengths) && legs.Select(s => s.Relief).SequenceEqual(relief), $"{name} leg IDs, lengths and relief follow the spec");
            bool rising = true; for (int i = 1; i < legs.Count; i++) rising &= RouteBudget.Hearts(legs[i]) > RouteBudget.Hearts(legs[i - 1]);
            Check(rising && legs.All(s => s.Length >= 350 && s.Length <= 600 && RouteBudget.Hearts(s) <= RouteBudget.MaximumLegHearts), $"{name} legs rise, stay inside 350-600 m and cost at most 26 hearts");
            SegmentDefinition river = catalog.Outgoing(region.FinalCamp.Id);
            Check(river.RiverApproach && river.Id == $"segment.{number:00}.river" && river.Length == 30, $"{name}'s river (30 m) follows its final camp");
            Check(region.LooseItemIds.SequenceEqual(featured) && region.TollCost == toll && TravelState.TollGold(region) == toll / 1000, $"{name} features {string.Join(", ", featured)}; toll {toll} cash / {toll / 1000} gold");
            bool drops = true;
            foreach (SegmentDefinition leg in legs.Append(catalog.LandingInto(id)))
                for (int cycle = 1; cycle <= 4; cycle++) { var roll = LooseDrops.Roll(region, leg, cycle); drops &= featured.All(f => roll.Any(d => d.ItemId == f)); }
            Check(drops, $"Every {name} drop roll offers all three featured items");
            bool layout = true;
            foreach (SegmentDefinition leg in legs)
            {
                var anchors = SegmentLayout.Build(region, leg);
                int fish = anchors.Count(a => a.Kind == InteractableKind.FishingPoint), animals = anchors.Count(a => a.Kind == InteractableKind.AnimalSpawn);
                layout &= fish >= 3 && animals >= 3 && (animals >= 4) == (leg.Length >= 450) && (fish >= 4) == (leg.Length >= 500);
            }
            Check(layout, $"{name} legs get 3+ fishing points and animal spawns (fourth spawn from 450 m, fourth fishing point from 500 m)");
        }

        private static void CatalogueChecks()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            Check(catalog.Regions.Count == 4, "Regions count is four (Dawn Plains, Windward Hills, Mistwood, Fern Hollow)");
            RegionShape(catalog, "region.03", "Mistwood", 3,
                new[] { "Dawn Woodland", "Quiet Woodland", "Mossbank", "Owl's Perch", "Nettle Ford", "Clay Pits", "Salt Lick", "Fogbound", "Grey Thicket", "Hushwood", "Distant Woodland" },
                CampSide.Right, new float[] { 400, 410, 420, 430, 440, 450, 470, 490, 500, 520 }, new float[] { 3, 2, 4, 3, 5, 4, 3, 5, 4, 2 },
                new[] { "material.nettle_fiber", "material.clay", "ingredient.rock_salt" }, 2000);
            RegionShape(catalog, "region.04", "Fern Hollow", 4,
                new[] { "Dawn Hollow", "Quiet Hollow", "Fernbank", "Lily Pond", "Strawberry Patch", "Mushroom Ring", "Limestone Steps", "Brookside", "Bracken Dell", "Frog's Rest", "Dragonfly Bend", "Greenway", "Bright Hollow" },
                CampSide.Left, new float[] { 420, 430, 440, 450, 460, 470, 480, 500, 520, 540, 550, 560 }, new float[] { 2, 1, 3, 2, 1, 4, 2, 3, 1, 2, 3, 2 },
                new[] { "material.limestone", "food.strawberries", "ingredient.mushrooms" }, 4000);
            Check(catalog.Region("region.02").TollCost == 1000 && TravelState.TollGold(catalog.Region("region.02")) == 1 && catalog.Region("region.02").NextRegionId == "region.03"
                && catalog.Region("region.03").NextRegionId == "region.04", "Tolls 1,000 / 2,000 / 4,000 (gold 1 / 2 / 4): Windward opens Mistwood, Mistwood opens Fern Hollow");
            SegmentDefinition intoMist = catalog.LandingInto("region.03"), intoFern = catalog.LandingInto("region.04");
            Check(intoMist != null && intoMist.Id == "segment.03.landing" && intoMist.FromCampId == "camp.02.12" && intoMist.ToCampId == "camp.03.01" && intoMist.Length == 120 && intoMist.Relief == 1.5f,
                "Mistwood's landing joins Far Lookout's river to Dawn Woodland (120 m)");
            Check(intoFern != null && intoFern.Id == "segment.04.landing" && intoFern.FromCampId == "camp.03.11" && intoFern.ToCampId == "camp.04.01" && intoFern.Length == 120 && intoFern.Relief == 1f,
                "Fern Hollow's landing joins Distant Woodland's river to Dawn Hollow (120 m)");
            Check(catalog.RegionOf(intoMist).Id == "region.03" && catalog.RegionOf(intoFern).Id == "region.04", "A landing belongs to the region it leads into");

            // Review Focus 1: an old save waiting at Far Lookout's river with the region-2 toll paid now crosses into Mistwood.
            var visited = new[] { "camp.01.01", "camp.02.01", "camp.02.12" };
            TravelState old = TravelState.Restore(catalog, "camp.02.12", false, "camp.02.12", 30, 0, visited, new[] { "region.01", "region.02" }, new[] { "region.02" },
                new[] { new KeyValuePair<string, int>("segment.02.river", 2) });
            Check(old.CanCrossRiver(false, out _) && old.CrossRiver(false) == "segment.03.landing" && old.OnFarBank, "A save at Far Lookout's river with the toll paid crosses to Mistwood's landing");
            Check(old.ArriveAtSegmentEnd() == "segment.03.01" && old.CurrentCampId == "camp.03.01" && old.Seated && old.DiscoveredRegions.Contains("region.03"), "The landing reaches Dawn Woodland and discovers Mistwood");

            // Walk-through: every Mistwood camp, the toll and ferry, every Fern Hollow camp.
            int reached = 1;
            while (!catalog.IsFinalCamp(old.CurrentCampId)) { old.StandUp(); old.ArriveAtSegmentEnd(); reached++; }
            Check(old.CurrentCampId == "camp.03.11" && reached == 11, "Walking Mistwood reaches all eleven camps to Distant Woodland");
            old.StandUp();
            Check(!old.CanPayToll(Domain.Travel.TollCurrency.Cash, out string short3) && short3.Contains("2000"), "Mistwood's toll asks 2000 cash");
            old.GrantTestGold(2); old.PayToll(TollCurrency.Gold);
            Check(old.Gold == 0 && old.CrossRiver(false) == "segment.04.landing" && old.ArriveAtSegmentEnd() == "segment.04.01" && old.CurrentCampId == "camp.04.01", "Two gold pays Mistwood's toll and the ferry lands at Dawn Hollow");
            reached = 1;
            while (!catalog.IsFinalCamp(old.CurrentCampId)) { old.StandUp(); old.ArriveAtSegmentEnd(); reached++; }
            Check(old.CurrentCampId == "camp.04.13" && reached == 13 && old.DiscoveredRegions.SequenceEqual(new[] { "region.01", "region.02", "region.03", "region.04" }), "Walking Fern Hollow reaches all thirteen camps to Bright Hollow");
        }

        private static bool Near(float[] a, params float[] b) => a != null && a.Length == b.Length && a.Zip(b, (x, y) => Math.Abs(x - y) < 1e-4f).All(ok => ok);

        private static void LookChecks()
        {
            RegionLook mist = RegionLooks.For(3, false);
            Check(mist.Fog && Near(mist.FogColor, 0.70f, 0.76f, 0.80f) && mist.FogStart == 25f && mist.FogEnd == 90f && mist.SunIntensity == 0.9f && Near(mist.SunTint, 0.92f, 0.96f, 1.0f)
                && Near(mist.Ambient, 0.46f, 0.49f, 0.52f) && !mist.Valley, "Mistwood: linear fog (0.70, 0.76, 0.80) 25-90 m, sun 0.9 cool tint, cooler ambient");
            Check(new[] { 1, 2, 4 }.All(n => !RegionLooks.For(n, false).Fog) && !RegionLooks.For(4, true).Fog, "Fog only in Mistwood");
            RegionLook fern = RegionLooks.For(4, false), approach = RegionLooks.For(4, true), dawn = RegionLooks.For(1, false);
            Check(fern.Valley && fern.ValleyFlat == 16f && fern.ValleyHeight == 7f && fern.ValleyRiseEnd == 40f && fern.SlopeScatter == 2f && fern.FloorScatter == 0.5f, "Fern Hollow: valley floor 16 m, +7 m by 40 m; ferns 2x on slopes, 0.5x on the floor");
            Check(!approach.Valley && approach.SunIntensity == fern.SunIntensity && !dawn.Valley && dawn.SunIntensity == 1.1f && Near(dawn.Ambient, 0.5f, 0.5f, 0.5f) && Near(dawn.SunTint, 1f, 1f, 1f),
                "The settlement approach uses Fern Hollow's light without the valley; other regions keep today's look");
            TravelCatalog catalog = TravelCatalog.Prototype();
            Check(catalog.RegionOf(catalog.LandingInto("region.03")).Number == 3 && catalog.RegionOf(catalog.LandingInto("region.04")).Number == 4, "Mistwood's landing is foggy; Fern Hollow's is not (looks follow RegionOf)");

            // Review Focus 5: applying Mistwood then Fern Hollow (a scene reload or map travel) leaves no fog behind.
            bool fog = RenderSettings.fog; FogMode mode = RenderSettings.fogMode; Color fogColor = RenderSettings.fogColor, ambient = RenderSettings.ambientLight;
            float start = RenderSettings.fogStartDistance, end = RenderSettings.fogEndDistance;
            var sunObject = new GameObject("Look check sun", typeof(Light)); var cameraObject = new GameObject("Look check camera", typeof(Camera));
            try
            {
                Light sun = sunObject.GetComponent<Light>(); Camera camera = cameraObject.GetComponent<Camera>();
                MovementPrototype.ApplyLook(mist, sun, camera);
                Check(RenderSettings.fog && RenderSettings.fogMode == FogMode.Linear && Math.Abs(RenderSettings.fogEndDistance - 90f) < 1e-3f && Math.Abs(sun.intensity - 0.9f) < 1e-4f
                    && Math.Abs(camera.backgroundColor.g - 0.76f) < 1e-3f, "Applying Mistwood turns on linear fog, dims the sun and matches the sky to the fog");
                MovementPrototype.ApplyLook(fern, sun, camera);
                Check(!RenderSettings.fog && Math.Abs(sun.intensity - 1.1f) < 1e-4f && sun.color == Color.white && Math.Abs(RenderSettings.ambientLight.r - 0.5f) < 1e-3f,
                    "Applying Fern Hollow after Mistwood leaves no fog, cool sun or cool ambient behind");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sunObject); UnityEngine.Object.DestroyImmediate(cameraObject);
                RenderSettings.fog = fog; RenderSettings.fogMode = mode; RenderSettings.fogColor = fogColor; RenderSettings.ambientLight = ambient;
                RenderSettings.fogStartDistance = start; RenderSettings.fogEndDistance = end;
            }
            // Fog is enabled only at runtime, so the build must keep the linear fog shader variants (Automatic stripping drops them).
            var graphics = new SerializedObject(UnityEngine.Rendering.GraphicsSettings.GetGraphicsSettings());
            Check(graphics.FindProperty("m_FogStripping").intValue == 1 && graphics.FindProperty("m_FogKeepLinear").boolValue, "Graphics settings keep the linear fog shader variants for the device build");
        }

        // User report (November 8, 2026): every ferry stood on grass. The river box sat 4 cm under the ground (no channel), and
        // in Fern Hollow the valley walls also rose behind the river. Water must show at every river, with the valley open there.
        private static void RiverChecks()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            foreach ((string id, bool landing) in new[] { ("segment.02.river", false), ("segment.04.river", false), ("segment.04.landing", true), ("segment.03.landing", true) })
            {
                SegmentDefinition leg = catalog.Segment(id);
                RegionLook look = RegionLooks.For(catalog.RegionOf(leg).Number, false);
                var path = new SampledTrailPath(TravelScenery.KnotsFor(leg));
                TravelScenery.RiverCut cut = TravelScenery.RiverCutFor(path, landing);
                Vector3[] ground = TravelScenery.TerrainMesh(path, 70f, 3f, TravelScenery.Undulation(leg), false, null, null, look, new[] { cut }).vertices;
                Vector3 across = Vector3.Cross(Vector3.up, cut.Along).normalized;
                float waterTop = cut.Centre.y - TravelScenery.RiverWaterDrop, worstWater = float.MinValue, worstBank = float.MinValue;
                foreach (Vector3 v in ground)
                {
                    Vector3 local = v - cut.Centre; float along = Mathf.Abs(Vector3.Dot(local, cut.Along)), side = Mathf.Abs(Vector3.Dot(local, across));
                    if (side > 40f) continue;
                    if (along < TravelScenery.RiverHalfWidth - 1f) worstWater = Mathf.Max(worstWater, v.y);
                    else if (along < 25f) worstBank = Mathf.Max(worstBank, v.y - cut.Centre.y);
                }
                Check(worstWater <= waterTop - 0.05f, $"{id}: the ground under the river lies below the water ({worstWater:0.00} vs water top {waterTop:0.00})");
                Check(!look.Valley || worstBank < 1.5f, $"{id}: the valley opens at the river, with no wall rising on its banks (highest bank {worstBank:0.00} m)");
            }
        }

        private static void ValleyChecks()
        {
            RegionLook fern = RegionLooks.For(4, false), dawn = RegionLooks.For(1, false), approachLook = RegionLooks.For(4, true);
            bool monotonic = true; float last = 0f;
            for (float d = 0f; d <= 60f; d += 0.5f) { float lift = TravelScenery.ValleyLift(fern, d); monotonic &= lift >= last - 1e-5f; last = lift; }
            Check(TravelScenery.ValleyLift(fern, 10f) == 0f && TravelScenery.ValleyLift(fern, 16f) == 0f && Math.Abs(TravelScenery.ValleyLift(fern, 40f) - 7f) < 0.01f
                && Math.Abs(TravelScenery.ValleyLift(fern, 60f) - 7f) < 0.01f && monotonic, "Valley lift: 0 inside the 16 m floor, rising smoothly to +7 m at 40 m, flat beyond");
            Check(TravelScenery.ValleyLift(dawn, 40f) == 0f && TravelScenery.ValleyLift(approachLook, 40f) == 0f, "No valley outside Fern Hollow (nor on the settlement approach)");
            Check(SceneryScatter.FernBushWeight(fern, 10f) == 0.5f && SceneryScatter.FernBushWeight(fern, 25f) == 2f && SceneryScatter.FernBushWeight(dawn, 25f) == 1f,
                "Ferns and bushes: half as many on the valley floor, twice as many on its slopes; unchanged elsewhere");

            // A Fern Hollow leg's terrain: lifted only beyond the floor, with every clearing and pond still level.
            TravelCatalog catalog = TravelCatalog.Prototype();
            RegionDefinition region = catalog.Region("region.04");
            SegmentDefinition leg = catalog.Segment("segment.04.06");
            var path = new SampledTrailPath(TravelScenery.KnotsFor(leg));
            var clearings = new[] { TravelScenery.ClearingCentre(path, 0, catalog.Camp(leg.FromCampId)), TravelScenery.ClearingCentre(path, path.Length, catalog.Camp(leg.ToCampId)) };
            var ponds = new List<Vector3>(); TravelScenery.AddPonds(ponds, path, SegmentLayout.Build(region, leg));
            float undulation = TravelScenery.Undulation(leg);
            Vector3[] flat = TravelScenery.TerrainMesh(path, 70f, 3f, undulation, false, clearings, ponds.ToArray()).vertices;
            Vector3[] valley = TravelScenery.TerrainMesh(path, 70f, 3f, undulation, false, clearings, ponds.ToArray(), fern).vertices;
            var road = new List<Vector2>(); for (float d = 0; d <= path.Length; d += 1f) { path.Evaluate(d, out Vector3 p, out _); road.Add(new Vector2(p.x, p.z)); }
            float RoadDistance(Vector3 v) { float best = float.MaxValue; var h = new Vector2(v.x, v.z); foreach (Vector2 r in road) best = Mathf.Min(best, (r - h).sqrMagnitude); return Mathf.Sqrt(best); }
            bool floorKept = true, bounded = true; float highest = 0f;
            for (int i = 0; i < flat.Length; i++)
            {
                float diff = valley[i].y - flat[i].y, distance = RoadDistance(flat[i]);
                if (distance < 15f) floorKept &= Math.Abs(diff) < 1e-3f;
                bounded &= diff > -1e-3f && diff < 7f + 1e-3f;
                highest = Mathf.Max(highest, diff);
            }
            Check(floorKept && bounded && highest > 6.9f, "Fern Hollow terrain keeps the road and its floor unchanged and rises up to +7 m on the valley sides");
            bool level = true;
            foreach (Vector3 centre in clearings.Concat(ponds))
            {
                int nearest = 0; float best = float.MaxValue;
                for (int i = 0; i < valley.Length; i++) { float d = new Vector2(valley[i].x - centre.x, valley[i].z - centre.z).sqrMagnitude; if (d < best) { best = d; nearest = i; } }
                level &= Math.Abs(valley[nearest].y - flat[nearest].y) < 0.05f;
            }
            Check(level, "Camp clearings and fishing ponds stay level on the valley floor (within 0.05 m)");
        }

        // The approach stretch as the scene builds it: a straight road, the arch at its end, no interactables or pickups.
        private static void ApproachSceneChecks()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            SegmentDefinition approach = catalog.SettlementApproach;
            Vector3[] knots = TravelScenery.KnotsFor(approach);
            Check(knots.All(k => Mathf.Abs(k.x) < 1e-4f && Mathf.Abs(k.y) < 1e-4f), "The approach road runs straight and level to the arch");
            var path = new SampledTrailPath(knots);
            var root = new GameObject("Approach check");
            try
            {
                Vector3 arch = TravelScenery.BuildSettlementApproach(root.transform, path, null);
                path.Evaluate(path.Length, out Vector3 end, out Vector3 tangent);
                Transform archPiece = root.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Entrance arch");
                Check(Vector3.Distance(arch, end) <= 0.5f && archPiece != null && new Vector2(archPiece.position.x - arch.x, archPiece.position.z - arch.z).magnitude < 1e-3f && Mathf.Abs(archPiece.position.y - arch.y) < 0.1f && Vector3.Angle(archPiece.forward, tangent) < 1f,
                    "The entrance arch spans the road at its end (within 0.5 m), facing along it");
                int palisade = root.GetComponentsInChildren<Transform>().Count(t => t.name == "Palisade"), fences = root.GetComponentsInChildren<Transform>().Count(t => t.name == "Fence");
                int backdrop = root.GetComponentsInChildren<Transform>().Count(t => t.name == "Settlement backdrop");
                Check(palisade >= 10 && fences >= 10 && backdrop >= 4, $"Palisade across the view, fences along the road and settlement roofs behind ({palisade} / {fences} / {backdrop})");
                Check(root.GetComponentsInChildren<Harvest.Tier1HarvestVisual>(true).Length == 0 && root.GetComponentsInChildren<Collider>(true).Length == 0,
                    "The approach dressing has no interactables and no colliders");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            Check(SegmentLayout.Build(catalog.RegionOf(approach), approach).Count == 0 && LooseDrops.PointCount(approach) == 0, "An approach scene gets zero interactables and zero loose drops");
            // Review fix: the palisade and backdrop sit at road height, so the approach ground must not roll under them.
            Vector3[] ground = TravelScenery.TerrainMesh(path, 70f, 3f, TravelScenery.Undulation(approach)).vertices;
            float worst = ground.Max(v => Mathf.Abs(v.y));
            Check(worst < 0.25f, $"The approach ground is flat farmland, so the palisade and settlement roofs rest on it (largest offset {worst:0.00} m)");
        }

        // Review fix (Important 2): no two camp tabs overlap on any region's sheet (all 24 planned regions, alternating sides).
        private static void MapTabChecks()
        {
            var overlaps = new List<string>();
            foreach (RegionPlanEntry plan in RegionPlan.All)
            {
                List<Vector2> nodes = CollectionPrototype.CampNodes(plan.Camps, plan.Number);
                for (int i = 0; i < nodes.Count; i++)
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        Rect a = CollectionPrototype.CampTabRect(nodes[i], i % 2 == 0), b = CollectionPrototype.CampTabRect(nodes[j], j % 2 == 0);
                        if (a.Overlaps(b)) overlaps.Add($"region {plan.Number} camps {i + 1}/{j + 1}");
                    }
            }
            Check(overlaps.Count == 0, "No camp tabs overlap on any region map (11 and 13 camps included)" + (overlaps.Count > 0 ? ": " + string.Join(", ", overlaps.Take(6)) : ""));
        }

        // A profile seated at Bright Hollow with every tier-one region discovered and Mistwood's toll paid.
        private static TravelState AtBrightHollow(TravelCatalog catalog, long cash, long gold)
        {
            var visited = new[] { "camp.01.01", "camp.02.01", "camp.03.01", "camp.04.01", "camp.04.13" };
            return TravelState.Restore(catalog, "camp.04.13", true, "camp.04.13", 40, cash, visited, new[] { "region.01", "region.02", "region.03", "region.04" }, new[] { "region.03" },
                new[] { new KeyValuePair<string, int>("segment.04.river", 1) }, gold);
        }
        private static TravelState Reload(TravelState s) => TravelState.Restore(s.Catalog, s.CurrentCampId, s.Seated, s.LastTrailCampId, s.VisitNumber, s.Cash,
            s.VisitedCamps.ToList(), s.DiscoveredRegions.ToList(), s.PaidTolls.ToList(), s.DropCycles.ToList(), s.Gold, s.OnFarBank, s.OnSettlementApproach);

        private static void SettlementRouteChecks()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            SettlementDefinition settlement = catalog.Settlement("settlement.01");
            SegmentDefinition approach = catalog.SettlementApproach;
            Check(settlement != null && settlement.Name == "Settlement" && catalog.Regions.All(r => r.Camps.All(c => c.Id != settlement.Id)), "The settlement (settlement.01) is a catalogue stop that belongs to no region");
            Check(approach != null && approach.Id == "segment.settlement.approach" && approach.FromCampId == "camp.04.13" && approach.ToCampId == null && approach.ToSettlementId == "settlement.01"
                && approach.Length == 160 && approach.Relief == 0 && approach.SettlementApproach && !approach.RiverApproach && !approach.Landing && catalog.SettlementApproachFrom("camp.04.13") == approach,
                "The approach leg (160 m, flat) runs from Bright Hollow's river to the settlement");
            Check(catalog.Outgoing("camp.04.13").RiverApproach && catalog.Segment(approach.Id) == approach && catalog.RegionOf(approach).Id == "region.04", "Bright Hollow keeps its river leg; the approach is looked up separately and uses Fern Hollow");
            Check(LooseDrops.PointCount(approach) == 0 && LooseDrops.Roll(catalog.Region("region.04"), approach, 1).Count == 0 && SegmentLayout.Build(catalog.Region("region.04"), approach).Count == 0,
                "No loose drops and no interactables on the approach");

            // Review Focus 3: the 4,000 toll with exactly enough, and one short.
            TravelState s = AtBrightHollow(catalog, 3999, 3);
            s.StandUp();
            Check(!s.CanPayToll(TollCurrency.Cash, out string shortCash) && shortCash.Contains("4000") && !s.CanPayToll(TollCurrency.Gold, out string shortGold) && shortGold.Contains("4 gold"),
                "3,999 cash or 3 gold is refused with the 4,000 / 4 toll in the reason");
            Check(!s.CanCrossRiver(false, out string unpaid) && unpaid.Contains("toll"), "The ferry to the settlement needs the toll first");
            s.GrantTestCash(1);
            Check(s.CanPayToll(TollCurrency.Cash, out _), "Exactly 4,000 cash pays the toll");
            TravelState byGold = AtBrightHollow(catalog, 0, 4); byGold.StandUp();
            Check(byGold.CanPayToll(TollCurrency.Gold, out _), "Exactly 4 gold pays the toll");
            s.PayToll(TollCurrency.Cash);
            int visit = s.VisitNumber;
            Check(s.Cash == 0 && s.PaidTolls.Contains("region.04") && s.CanCrossRiver(false, out _), "Paying the 4,000 toll opens the ferry from Bright Hollow");
            Check(s.CrossRiver(false) == approach.Id && s.OnSettlementApproach && !s.OnFarBank && !s.Seated && s.CurrentSegment == approach && s.CurrentCampId == "camp.04.13",
                "Crossing from Bright Hollow lands on the settlement approach");
            Check(Throws(() => s.ArriveAtSegmentEnd()), "The approach has no camp at its end (the arch handles it)");
            TravelState reloaded = Reload(s);
            Check(reloaded.OnSettlementApproach && reloaded.CurrentSegment == approach, "Walking the approach is a valid saved state");
            Check(s.ReachSettlementArch() == "segment.04.river" && s.Seated && s.CurrentCampId == "camp.04.13" && !s.OnSettlementApproach && s.VisitNumber == visit + 1
                && s.PaidTolls.Contains("region.04") && s.CycleOf("segment.04.river") == 2, "Reaching the arch returns to Bright Hollow as a new visit; the toll stays paid");
            Check(Throws(() => s.ReachSettlementArch()), "The arch can only be reached while walking the approach");
            s.StandUp();
            Check(s.CanCrossRiver(false, out _) && !s.CanPayToll(TollCurrency.Cash, out _), "Later crossings to the settlement need no second toll");

            // Review Focus 2: a save written mid-approach restores, through the restart path, seated at Bright Hollow.
            s.CrossRiver(false);
            var snapshot = new SessionSnapshot { version = 3, segmentId = s.CurrentSegment.Id, distance = 80, survival = new SavedSurvival(), travel = SessionSaveStore.SaveTravel(s) };
            Check(snapshot.travel.settlementApproach && !snapshot.travel.seated && !snapshot.travel.farBank, "The save records the approach walk");
            SessionSnapshot copy = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(snapshot));
            SessionSaveStore.MigrateCatalogLayout(copy);
            TravelState restored = SessionSaveStore.RestoreTravel(copy, catalog);
            Check(copy.segmentId == approach.Id && copy.distance == 80 && restored.OnSettlementApproach, "Layout migration leaves a valid approach save alone");
            int before = restored.VisitNumber;
            Check(restored.ResumeAtCampAfterRestart() == "segment.04.river" && restored.Seated && restored.CurrentCampId == "camp.04.13" && !restored.OnSettlementApproach
                && restored.PaidTolls.Contains("region.04") && restored.VisitNumber == before + 1, "A restart mid-approach returns seated to Bright Hollow with the toll kept");
            var legacy = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(snapshot).Replace("\"settlementApproach\":true,", ""));
            Check(!legacy.travel.settlementApproach, "Older saves without the field read it as false");

            // Review fix (Critical 1): on a real app start the scene shows the start camp's own leg (here Bright Hollow's
            // ~28 m river), and the store validated the saved distance against that path. A save written far along the
            // approach (or a ferry landing) must still load, through the store, with that short path length.
            string directory = Path.Combine(Path.GetTempPath(), "TrailWalkerRegions-" + Guid.NewGuid().ToString("N"));
            try
            {
                var store = new SessionSaveStore(directory, new SystemClock(), 3);
                float riverPath = new SampledTrailPath(TravelScenery.KnotsFor(catalog.Outgoing("camp.04.13"))).Length;
                SessionSnapshot farAlong = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(snapshot)); farAlong.distance = 150f; farAlong.camp = true; farAlong.requestedSpeed = 0;
                store.Save(farAlong);
                Check(store.Load(riverPath, out SessionSnapshot reopened, out string why) == LoadStatus.Loaded && reopened.travel.settlementApproach,
                    $"A save at the arch (150 m) loads on restart although the scene shows the {riverPath:0} m river leg ({why})");
                var landing = new SessionSnapshot { version = 3, segmentId = "segment.03.landing", distance = 110f, survival = new SavedSurvival(),
                    travel = new SavedTravel { campId = "camp.02.12", lastTrailCampId = "camp.02.12", seated = false, farBank = true, visitNumber = 5,
                        visited = new List<string> { "camp.01.01", "camp.02.01", "camp.02.12" }, discovered = new List<string> { "region.01", "region.02" }, paidTolls = new List<string> { "region.02" },
                        cycles = new List<SavedCycle> { new SavedCycle { segmentId = "segment.02.river", cycle = 1 }, new SavedCycle { segmentId = "segment.03.landing", cycle = 1 } } } };
                store.Save(landing);
                float farLookoutRiver = new SampledTrailPath(TravelScenery.KnotsFor(catalog.Outgoing("camp.02.12"))).Length;
                Check(store.Load(farLookoutRiver, out SessionSnapshot landed, out string whyLanding) == LoadStatus.Loaded && landed.travel.farBank,
                    $"A save 110 m along Mistwood's landing loads on restart with Far Lookout's river leg shown ({whyLanding})");
                SessionSnapshot midway = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(farAlong)); midway.distance = 80f; midway.camp = false;
                store.Save(midway);
                Check(store.Load(riverPath, out SessionSnapshot midReopened, out string whyMid) == LoadStatus.Loaded && midReopened.distance == 80f,
                    $"A save 80 m into the approach (not at the arch) also loads on restart ({whyMid})");
                var tooFar = JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(farAlong)); tooFar.distance = 400f;
                Check(InvalidData(() => SessionSaveStore.Validate(tooFar, riverPath, out _)),
                    "A distance beyond the saved leg's own length is still rejected");
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
            Check(Throws(() => TravelState.Restore(catalog, "camp.03.11", false, "camp.03.11", 1, 0, new[] { "camp.03.11" }, new[] { "region.03" }, new[] { "region.03" },
                new[] { new KeyValuePair<string, int>("segment.03.river", 1) }, 0, false, true)), "Only Bright Hollow's river leads to the settlement approach");
        }
    }
}
