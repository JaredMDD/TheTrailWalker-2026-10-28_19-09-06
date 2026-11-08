using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Persistence;
using TheTrailWalker.Domain.Travel;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    public static class Milestone5Validation
    {
        private static readonly List<string> passed = new List<string>();
        private static void CampNpcChecks()
        {
            var none = new HashSet<string>();
            NpcAppearance[] start = CampNpcs.Roll("camp.01.01", 1, none);
            Check(start.Length == 3 && start.All(n => n.ClothingSetId == "clothing.tier0.rags" && n.BackpackId == "backpack.tier0.rags"),
                "With no wearable recipes known, every camp trader wears tier 0 rags and the rag pack");
            Check(start.Select(n => (n.Body, n.SkinTone)).Distinct().Count() == 3 && start.All(n => n.SkinTone >= 0 && n.SkinTone < CampNpcs.SkinTones),
                "The three traders of a camp differ in body or skin tone");
            Check(CampNpcs.Roll("camp.01.01", 1, none).SequenceEqual(start) && !CampNpcs.Roll("camp.01.01", 2, none).SequenceEqual(start),
                "Trader looks stay fixed for a visit (and reload) and change on the next visit");
            var oneShirt = new HashSet<string> { "recipe.wearable.hemp_shirt" };
            Check(CampNpcs.DiscoveredSets(oneShirt).Count == 2 && CampNpcs.DiscoveredBackpacks(oneShirt).Count == 1,
                "One known recipe of a set discovers that clothing set; backpacks need their own recipe");
            var packOnly = new HashSet<string> { "recipe.wearable.starter_backpack" };
            Check(CampNpcs.DiscoveredSets(packOnly).Count == 1 && CampNpcs.DiscoveredBackpacks(packOnly).Count == 2,
                "A known backpack recipe does not discover a clothing set");
            var both = new HashSet<string> { "recipe.wearable.rabbit_boots", "recipe.wearable.starter_backpack" };
            var looks = Enumerable.Range(1, 60).SelectMany(v => CampNpcs.Roll("camp.01.02", v, both)).ToList();
            Check(looks.Any(n => n.ClothingSetId == "clothing.tier1.hemp_rabbit" && n.BackpackId == "backpack.tier0.rags")
                && looks.Any(n => n.ClothingSetId == "clothing.tier0.rags" && n.BackpackId == "backpack.tier1.starter")
                && looks.All(n => n.IdleSpeed >= 0.85f && n.IdleSpeed <= 1.15f) && looks.Select(n => n.IdlePhase).Distinct().Count() > 30,
                "Clothing set and backpack are chosen independently among discovered ones; idle motion is desynchronised");
            // Faceted revision 4 (November 2, 2026): each trader rolls a hair style and, for men only, a beard or none.
            Check(looks.All(n => n.HairStyle >= 0 && n.HairStyle < CampNpcs.HairStyles) && looks.Select(n => n.HairStyle).Distinct().Count() == CampNpcs.HairStyles
                && looks.Where(n => n.Body == NpcBody.Female).All(n => !n.Beard) && looks.Any(n => n.Body == NpcBody.Male && n.Beard)
                && looks.Any(n => n.Body == NpcBody.Male && !n.Beard),
                "Traders roll any of the four hair styles; men roll a beard or none, women never have one");
        }

        // Each shirt and backpack is drawn with its own container skins (the belt and tool loops are universal); the
        // neutral fallback is only for debug gear.
        private static bool StorageGearHasOwnArt()
        {
            InventoryContainerSkin[] skins = Resources.LoadAll<InventoryContainerSkin>("UI/Inventory");
            bool Has(string id, InventoryContainerKind kind) => skins.Any(skin => skin.EquipmentDefinitionId == id && skin.Kind == kind);
            foreach (ItemDefinition item in Tier1CraftedItems.NewDefinitions)
            {
                if (item.GearSlot == TheTrailWalker.Domain.Equipment.GearSlot.Shirt && !Has(item.Id, InventoryContainerKind.Pocket)) return false;
                if (item.GearSlot == TheTrailWalker.Domain.Equipment.GearSlot.Backpack && !(Has(item.Id, InventoryContainerKind.Backpack) && Has(item.Id, InventoryContainerKind.Opening))) return false;
            }
            return true;
        }

        // Camp ground (November 2, 2026): every camp clearing (both segment ends, either side) sits on the ground. The
        // terrain under the clearing disc must stay between the clearing top and the bottom of its 0.3 m base.
        public static List<string> FloatingCamps(TravelCatalog catalog)
        {
            var floating = new List<string>();
            var segments = catalog.Regions.SelectMany(r => r.Camps).SelectMany(c => new[] { catalog.Outgoing(c.Id), catalog.Incoming(c.Id) })
                .Where(x => x != null).Distinct().ToList();
            foreach (var segment in segments)
                {
                    var path = new TheTrailWalker.Movement.SampledTrailPath(TheTrailWalker.Travel.TravelScenery.KnotsFor(segment));
                    path.Evaluate(0, out Vector3 start, out Vector3 startTangent); path.Evaluate(path.Length, out Vector3 end, out Vector3 endTangent);
                    var clearings = new List<Vector3>();
                    if (!segment.Landing) clearings.Add(TheTrailWalker.Travel.TravelScenery.ClearingCentre(path, 0, catalog.Camp(segment.FromCampId)));
                    if (!segment.RiverApproach) clearings.Add(TheTrailWalker.Travel.TravelScenery.ClearingCentre(path, path.Length, catalog.Camp(segment.ToCampId)));
                    Mesh mesh = TheTrailWalker.Travel.TravelScenery.TerrainMesh(path, 70f, 3f, TheTrailWalker.Travel.TravelScenery.Undulation(segment), true, clearings.ToArray());
                    var probe = new GameObject("Camp ground probe"); var collider = probe.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
                    try
                    {
                        foreach (var (road, tangent, campId, present) in new[] { (start, startTangent, segment.FromCampId, !segment.Landing), (end, endTangent, segment.ToCampId, !segment.RiverApproach) })
                        {
                            if (!present) continue;
                            var camp = catalog.Camp(campId);
                            Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized * (camp.Side == CampSide.Right ? 1f : -1f);
                            Vector3 centre = road + side * TheTrailWalker.Travel.TravelScenery.ClearingOffset;
                            for (float r = 0; r <= 4.3f; r += 1.075f)
                                for (int a = 0; a < (r == 0 ? 1 : 12); a++)
                                {
                                    Vector3 q = centre + Quaternion.AngleAxis(a * 30f, Vector3.up) * Vector3.forward * r;
                                    if (!collider.Raycast(new Ray(q + Vector3.up * 50f, Vector3.down), out RaycastHit hit, 100f)) continue;
                                    float gap = centre.y - hit.point.y;
                                    // Above the clearing by up to 4 cm only where the ground meets its road-side rim; never floating (base 0.3 m).
                                    if (gap < -0.04f || gap > 0.32f) { floating.Add($"{segment.Id}/{camp.Name} gap {gap:0.00}"); a = 99; r = 99; }
                                }
                        }
                    }
                    finally { UnityEngine.Object.DestroyImmediate(probe); UnityEngine.Object.DestroyImmediate(mesh); }
                }
            return floating;
        }

        // Trail pickups (November 5, 2026): loose drops lie 1.9 m beside the road centre and used to rest at road height,
        // floating over the lower terrain. They now rest on TerrainHeightField; it must agree with a real collider at every
        // pickup spot (both sides, every 2 m, on the terrain built with the camp clearings) and never miss the ground.
        // oldGap reports how far above the ground the old rule left them.
        public static List<string> PickupGroundMismatches(TravelCatalog catalog, out float oldGap)
        {
            var wrong = new List<string>(); oldGap = 0;
            var segments = catalog.Regions.SelectMany(r => r.Camps).SelectMany(c => new[] { catalog.Outgoing(c.Id), catalog.Incoming(c.Id) })
                .Where(x => x != null).Distinct().ToList();
            foreach (var segment in segments)
            {
                var path = new TheTrailWalker.Movement.SampledTrailPath(TheTrailWalker.Travel.TravelScenery.KnotsFor(segment));
                var clearings = new List<Vector3>();
                if (!segment.Landing) clearings.Add(TheTrailWalker.Travel.TravelScenery.ClearingCentre(path, 0, catalog.Camp(segment.FromCampId)));
                if (!segment.RiverApproach) clearings.Add(TheTrailWalker.Travel.TravelScenery.ClearingCentre(path, path.Length, catalog.Camp(segment.ToCampId)));
                Mesh mesh = TheTrailWalker.Travel.TravelScenery.TerrainMesh(path, 70f, 3f, TheTrailWalker.Travel.TravelScenery.Undulation(segment), true, clearings.ToArray());
                var probe = new GameObject("Pickup ground probe", typeof(MeshFilter)); probe.GetComponent<MeshFilter>().sharedMesh = mesh;
                var collider = probe.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
                try
                {
                    var field = new TheTrailWalker.Travel.TerrainHeightField(probe.GetComponent<MeshFilter>());
                    for (float d = 1f; d <= path.Length - 1f; d += 2f)
                    {
                        path.Evaluate(d, out Vector3 p, out Vector3 t);
                        Vector3 side = Vector3.Cross(Vector3.up, t).normalized * 1.9f;
                        foreach (Vector3 q in new[] { p - side, p + side })
                        {
                            bool hit = collider.Raycast(new Ray(q + Vector3.up * 50f, Vector3.down), out RaycastHit h, 100f);
                            bool found = field.TryHeight(q, out float y);
                            if (!hit || !found || Mathf.Abs(h.point.y - y) > 0.005f) { wrong.Add($"{segment.Id} at {d:0} m"); break; }
                            oldGap = Mathf.Max(oldGap, p.y + 0.02f - y);
                        }
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(probe); UnityEngine.Object.DestroyImmediate(mesh); }
            }
            return wrong;
        }

        // Highest point of the road above the terrain at its edges (1.7 m) along the whole leg.
        private static float RoadAboveGround(SegmentDefinition segment)
        {
            var path = new TheTrailWalker.Movement.SampledTrailPath(TheTrailWalker.Travel.TravelScenery.KnotsFor(segment));
            Mesh mesh = TheTrailWalker.Travel.TravelScenery.TerrainMesh(path, 70f, 3f, TheTrailWalker.Travel.TravelScenery.Undulation(segment), true);
            var probe = new GameObject("Terrain probe"); var collider = probe.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
            float worst = 0;
            try
            {
                for (float d = 0; d <= path.Length; d += 1f)
                {
                    path.Evaluate(d, out Vector3 p, out Vector3 t);
                    Vector3 side = Vector3.Cross(Vector3.up, t).normalized * 1.7f;
                    foreach (Vector3 q in new[] { p - side, p + side })
                        if (collider.Raycast(new Ray(q + Vector3.up * 60f, Vector3.down), out RaycastHit hit, 200f)) worst = Mathf.Max(worst, p.y - hit.point.y);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(probe); UnityEngine.Object.DestroyImmediate(mesh); }
            return worst;
        }

        // The first and last 12 m of a leg are level (grade under 3%), so camps sit on a flat approach.
        private static bool LevelEnds(SegmentDefinition segment)
        {
            var path = new TheTrailWalker.Movement.SampledTrailPath(TheTrailWalker.Travel.TravelScenery.KnotsFor(segment));
            float span = Mathf.Min(12f, path.Length / 3f);
            path.Evaluate(0, out Vector3 a, out _); path.Evaluate(span, out Vector3 b, out _);
            path.Evaluate(path.Length - span, out Vector3 c, out _); path.Evaluate(path.Length, out Vector3 e, out _);
            return Mathf.Abs(b.y - a.y) / span < 0.03f && Mathf.Abs(e.y - c.y) / span < 0.03f;
        }

        // Samples the road centre and both edges every half metre against the built terrain mesh.
        private static bool TerrainClearsRoad(SegmentDefinition segment)
        {
            var path = new TheTrailWalker.Movement.SampledTrailPath(TheTrailWalker.Travel.TravelScenery.KnotsFor(segment));
            Mesh mesh = TheTrailWalker.Travel.TravelScenery.TerrainMesh(path, 70f, 3f, TheTrailWalker.Travel.TravelScenery.Undulation(segment), true);
            var probe = new GameObject("Terrain probe"); var collider = probe.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
            try
            {
                for (float d = 0; d <= path.Length; d += 0.5f)
                {
                    path.Evaluate(d, out Vector3 p, out Vector3 t);
                    Vector3 side = Vector3.Cross(Vector3.up, t) * 1.7f;
                    foreach (Vector3 q in new[] { p - side, p, p + side })
                        if (!collider.Raycast(new Ray(q + Vector3.up * 50f, Vector3.down), out RaycastHit hit, 100f) || hit.point.y > q.y - 0.02f) return false;
                }
                return true;
            }
            finally { UnityEngine.Object.DestroyImmediate(probe); UnityEngine.Object.DestroyImmediate(mesh); }
        }

        // October 31, 2026 art integration: each leg is dressed from its own region's tier-one kit, with the kit's material,
        // readable meshes (device CombineMeshes), separate shadow groups and the road kept clear; special NPCs have art.
        private static void ArtIntegrationChecks(TravelCatalog catalog)
        {
            bool readable = true, kits = true;
            foreach (RegionDefinition region in catalog.Regions)
            {
                RegionalEnvironmentKit kit = TheTrailWalker.Travel.SceneryScatter.Kit(region.Number);
                kits &= kit != null && kit.SharedMaterial != null && kit.PaletteSize == 32 && kit.Models != null && kit.Models.Length == 20;
                if (kit?.Models != null) foreach (RegionalSceneryArt art in kit.Models)
                    foreach (MeshFilter filter in art.Prefab != null ? art.Prefab.GetComponentsInChildren<MeshFilter>() : new MeshFilter[0])
                        readable &= filter.sharedMesh != null && filter.sharedMesh.isReadable;
            }
            Check(kits, "Every authored region has its own 20-piece tier-one environment kit with a 32-colour material");
            Check(readable, "Every regional kit mesh is readable, so device-side mesh combining draws it");
            // Tier-two kits (regions 7-12, November 6, 2026): six kits of 20 readable pieces, found by the live lookup, with
            // birch trees only (tier two's wood) and structures kept out of the scatter roles.
            bool tierTwo = true;
            for (int number = 7; number <= 12; number++)
            {
                RegionalEnvironmentKit kit = TheTrailWalker.Travel.SceneryScatter.Kit(number);
                tierTwo &= kit != null && kit.SharedMaterial != null && kit.PaletteSize == 32 && kit.Models != null && kit.Models.Length == 20
                    && kit.Models.All(a => a.Prefab != null && a.Id.StartsWith("env.tier02.region" + number.ToString("00") + ".", StringComparison.Ordinal)
                        && a.Prefab.GetComponentsInChildren<MeshFilter>().All(f => f.sharedMesh != null && f.sharedMesh.isReadable))
                    && kit.Models.Where(a => a.Category == "Tree").All(a => a.Name.Contains("Birch"))
                    && kit.Models.Any(a => a.Category == "Tree") && kit.Models.Any(a => a.Category == "Shrub") && kit.Models.Any(a => a.Category == "GroundCover");
            }
            Check(tierTwo, "Regions 7-12 each have a 20-piece tier-two kit of readable meshes with birch trees, found by the scenery lookup");
            // Tier-two camp (November 6, 2026): regions 7-12 get the burning birch fire (unlit, shadowless, flickering flames)
            // and the birch log seat; tier-one regions keep the static burned fire.
            var campProps = Resources.Load<CampPropArtCatalog>("Environment/CampProps/CampPropArtCatalog");
            var birchFire = campProps?.Campfires?.FirstOrDefault(f => f != null && f.WoodTier == 2);
            Transform flames = birchFire?.Prefab != null ? birchFire.Prefab.transform.Find("Flames") : null;
            bool campTwo = birchFire != null && birchFire.FirstRegion == 7 && birchFire.LastRegion == 12 && flames != null
                && birchFire.Prefab.GetComponent<TheTrailWalker.Travel.CampfireFlicker>()?.Flames == flames
                && flames.GetComponentsInChildren<MeshRenderer>().All(r => r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off)
                && campProps.LogSeats.Any(s => s.Wood == "Birch" && s.FirstRegion == 7 && s.LastRegion == 12 && s.Prefab != null)
                && !campProps.Campfires.Any(f => f != null && f.FirstRegion <= 6);
            Check(campTwo, "Tier-two camps (regions 7-12) use the burning birch campfire and birch log seats; tier-one camps keep the burned fire");
            // Tier-two loose resources (November 6, 2026): 18 designed meshes, three per region 7-12, built as prefabs with
            // a palette material that carries surface detail. Runtime item definitions do not exist yet (design step only).
            bool looseTwo = true;
            for (int number = 7; number <= 12; number++)
                foreach (string name in TheTrailWalker.Editor.Tier02ResourceAssets.Names[number - 7])
                {
                    var prefab = Resources.Load<GameObject>("Region" + number.ToString("00") + "/" + name);
                    var renderer = prefab != null ? prefab.GetComponentInChildren<MeshRenderer>() : null;
                    looseTwo &= renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.shader.name == "TheTrailWalker/FacetedDetail"
                        && prefab.GetComponentsInChildren<MeshFilter>().All(f => f.sharedMesh != null && f.sharedMesh.triangles.Length / 3 <= 1500);
                }
            Check(looseTwo, "Regions 7-12 each have three designed tier-two loose-resource prefabs with surface-detail palette materials");

            foreach (string id in new[] { "segment.01.01", "segment.02.01" })
            {
                SegmentDefinition segment = catalog.Segment(id);
                RegionalEnvironmentKit kit = TheTrailWalker.Travel.SceneryScatter.Kit(catalog.RegionOf(segment).Number);
                var path = new TheTrailWalker.Movement.SampledTrailPath(TheTrailWalker.Travel.TravelScenery.KnotsFor(segment));
                var parent = new GameObject("Scenery probe");
                try
                {
                    TheTrailWalker.Travel.SceneryScatter.Build(parent.transform, new[] { (TheTrailWalker.Movement.ITrailPath)path }, new[] { catalog.RegionOf(segment).Number },
                        new Vector3[0], new Vector3[0], p => 0f, segment.Id);
                    MeshRenderer[] parts = parent.GetComponentsInChildren<MeshRenderer>();
                    bool ownMaterial = parts.Length > 0 && parts.All(r => r.sharedMaterial == kit.SharedMaterial);
                    bool shadows = parts.Any(r => r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On) && parts.Any(r => r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off);
                    var road = new List<Vector2>(); for (float d = 0; d <= path.Length; d += 0.5f) { path.Evaluate(d, out Vector3 p, out _); road.Add(new Vector2(p.x, p.z)); }
                    bool clear = true;
                    foreach (MeshFilter filter in parent.GetComponentsInChildren<MeshFilter>())
                        foreach (Vector3 v in filter.sharedMesh.vertices) { var flat = new Vector2(v.x, v.z); clear &= road.All(r => (r - flat).sqrMagnitude >= 2.5f * 2.5f); if (!clear) break; }
                    Check(ownMaterial && shadows, $"{segment.Id} is dressed only from region {catalog.RegionOf(segment).Number}'s kit, with tall casters separate from non-casting cover");
                    Check(clear, $"{segment.Id} scenery keeps at least 2.5 m clear of the road");
                }
                finally
                {
                    foreach (MeshFilter filter in parent.GetComponentsInChildren<MeshFilter>()) UnityEngine.Object.DestroyImmediate(filter.sharedMesh);
                    UnityEngine.Object.DestroyImmediate(parent);
                }
            }
            SpecialNpcArtCatalog special = Resources.Load<SpecialNpcArtCatalog>("Characters/CampSpecial/SpecialNpcArtCatalog");
            Check(special != null && special.MissionLadySideways != null && special.FerrymanStatic != null,
                "The camp mission lady and the river ferryman have authored art replacing the capsules");
        }

        // Equipment design v1 (October 31, 2026): durability colour bands and slot art.
        private static void EquipmentUiChecks()
        {
            Check(DurabilityStyle.Band(31, 60) == DurabilityBand.Healthy && DurabilityStyle.Band(30, 60) == DurabilityBand.Warning
                && DurabilityStyle.Band(6, 60) == DurabilityBand.Warning && DurabilityStyle.Band(5.9, 60) == DurabilityBand.Critical && DurabilityStyle.Band(0, 60) == DurabilityBand.Critical,
                "Durability is green above 50%, orange from 10% through 50% (both boundaries orange) and red below 10%, from remaining / maximum");
            Check(DurabilityStyle.Band(5, 0) == DurabilityBand.Neutral && DurabilityStyle.Fill(5, 0) == 1f && DurabilityStyle.Fill(-3, 60) == 0f && DurabilityStyle.Fill(90, 60) == 1f,
                "A missing maximum never divides by zero (neutral badge) and the bar fill is clamped to 0-100%");
            bool art = true;
            foreach (TheTrailWalker.Domain.Equipment.GearSlot slot in Enum.GetValues(typeof(TheTrailWalker.Domain.Equipment.GearSlot)))
            {
                ItemDefinition tier1 = ItemArt.SlotSilhouette(slot);
                art &= tier1 != null && Resources.Load<GameObject>(Tier1CraftedItems.VisualPath(tier1.Id)) != null;
            }
            foreach (ItemDefinition debug in PrototypeItems.TestWearables) art &= ItemArt.ModelFor(debug) == ItemArt.SlotSilhouette(debug.GearSlot.Value);
            Check(art, "Every equipment slot has a tier-1 item model; debug test gear borrows its slot's tier-1 model");
            PlayerAvatarArtCatalog avatars = EquipmentAvatarView.Catalog;
            Check(avatars != null && avatars.MaleBody != null && avatars.FemaleBody != null && avatars.MaleHair.Length == 4 && avatars.FemaleHair.Length == 4
                && avatars.MaleHair.All(h => h != null) && avatars.FemaleHair.All(h => h != null) && avatars.SkinMaterials.Length == 6 && avatars.HairMaterials.Length == 5
                && avatars.SkinMaterials.All(m => m != null) && avatars.HairMaterials.All(m => m != null)
                && avatars.MaleBody.transform.Find("HeadAnchor/HairAnchor") != null && avatars.MaleBody.transform.Find("HeadAnchor/HatAnchor") != null && avatars.FemaleBody.transform.Find("BackpackAnchor") != null,
                "The equipment avatar catalog holds both bodies, 4 hair styles each, 6 skin and 5 hair colours, and the hair, hat and backpack anchors");
            // Fitted wearables (November 5, 2026): every tier-one shirt, pants and boots item has a male and a female shell
            // with meshes; debug test gear borrows its slot's tier-one shell; hats and backpacks keep their item models.
            bool fitted = true;
            foreach (ItemDefinition item in Tier1CraftedItems.NewDefinitions.Concat(PrototypeItems.TestWearables))
            {
                var slot = item.GearSlot;
                if (slot != TheTrailWalker.Domain.Equipment.GearSlot.Shirt && slot != TheTrailWalker.Domain.Equipment.GearSlot.Pants && slot != TheTrailWalker.Domain.Equipment.GearSlot.Boots) continue;
                foreach (bool female in new[] { false, true })
                {
                    GameObject shell = EquipmentAvatarView.WearableFor(avatars, item, female);
                    fitted &= shell != null && shell.GetComponentsInChildren<MeshFilter>().Any(f => f.sharedMesh != null && f.sharedMesh.vertexCount > 0);
                }
            }
            Check(fitted, "Every tier-one shirt, pants and boots item (and its debug stand-in) has a fitted shell on both player bodies");
            // Fitted hat and backpack (tier-one art pass, November 5, 2026): both bodies, and a hat-compatible hair
            // variant for every saved hairstyle on both bodies.
            bool accessories = true;
            foreach (ItemDefinition item in Tier1CraftedItems.NewDefinitions.Concat(PrototypeItems.TestWearables))
            {
                var slot = item.GearSlot;
                if (slot != TheTrailWalker.Domain.Equipment.GearSlot.Hat && slot != TheTrailWalker.Domain.Equipment.GearSlot.Backpack) continue;
                foreach (bool female in new[] { false, true })
                {
                    GameObject worn = EquipmentAvatarView.FittedAccessory(ItemArt.ModelFor(item).Id, female);
                    accessories &= worn != null && worn.GetComponentsInChildren<MeshFilter>().Any(f => f.sharedMesh != null && f.sharedMesh.vertexCount > 0);
                }
            }
            for (int style = 0; style < 4; style++)
                foreach (bool female in new[] { false, true }) accessories &= EquipmentAvatarView.HatHair(female, style) != null;
            Check(accessories, "The tier-one hat and backpack have fitted worn meshes on both bodies, with hat-compatible hair for all four styles");
        }

        private static void Check(bool value, string name) { if (!value) throw new Exception("Milestone 5: " + name); passed.Add(name); }

        // Regression (user report, November 7, 2026): leftovers of every walked leg stayed in the save until it passed the
        // 1,024-entry cap near Stone Gate, so every later save failed ("Invalid session header") and arrivals were undone.
        private static void StaleWorldItemChecks()
        {
            var ledger = new SpatialInventory();
            ItemEntry World(string id, string segment) { var e = ledger.AddWorld(id, PrototypeItems.Branch, 10); ledger.AssignSegment(e, segment); return e; }
            ItemEntry a = World("a", "segment.A"), b = World("b", "segment.B"), c = World("c", "segment.C");
            ItemEntry carried = ledger.AddWorld("carried", PrototypeItems.Branch, 5); ledger.Reserve(carried, 1); ledger.TryQuickStore(carried, 1, out _); ledger.Release(carried, 1);
            int removed = ledger.RemoveWorldItemsExcept("segment.B");
            Check(removed == 2 && a.Owner == ItemOwner.Consumed && c.Owner == ItemOwner.Consumed && b.Owner == ItemOwner.World && carried.Owner != ItemOwner.World && carried.Owner != ItemOwner.Consumed,
                "Arriving on a leg drops the leftovers of every other leg and keeps carried items");
            // Walk 25 legs of 70 drops each with the arrival roll (RemoveWorldItems + RemoveWorldItemsExcept): the save stays bounded.
            var walk = new SpatialInventory(); int peak = 0;
            for (int leg = 0; leg < 25; leg++)
            {
                string segment = "segment.walk." + leg;
                walk.RemoveWorldItems(segment); walk.RemoveWorldItemsExcept(segment);
                for (int i = 0; i < 70; i++) walk.AssignSegment(walk.AddWorld($"{segment}-{i}", PrototypeItems.Branch, 1000), segment);
                int live = 0; foreach (var e in walk.Entries) if (e.Owner == ItemOwner.World) live++;
                peak = Mathf.Max(peak, live);
            }
            Check(peak == 70, $"World entries stay at one leg's worth over a 25-leg walk (peak {peak}; 1,750 before the fix, cap 1,024)");
        }
        private static bool Throws(Action action) { try { action(); return false; } catch (ArgumentException) { return true; } catch (InvalidOperationException) { return true; } }
        private static TravelState Reload(TravelState state) => TravelState.Restore(state.Catalog, state.CurrentCampId, state.Seated, state.LastTrailCampId, state.VisitNumber, state.Cash,
            state.VisitedCamps.ToList(), state.DiscoveredRegions.ToList(), state.PaidTolls.ToList(), state.DropCycles.ToList(), state.Gold, state.OnFarBank);

        private sealed class FixedClock : IClock { public DateTime UtcNow => new DateTime(2026, 10, 30, 0, 0, 0, DateTimeKind.Utc); }
        private static bool Invalid(SessionSnapshot snapshot) { try { SessionSaveStore.Validate(snapshot, float.MaxValue, out _); return false; } catch (InvalidDataException) { return true; } }
        private static SessionSnapshot Snapshot(TravelState travel) => new SessionSnapshot { version = 3, segmentId = travel.CurrentSegment.Id, survival = new SavedSurvival(), travel = SessionSaveStore.SaveTravel(travel) };

        private static void PersistenceChecks(TravelCatalog catalog)
        {
            string directory = Path.Combine(Path.GetTempPath(), "TrailWalkerM5-" + Guid.NewGuid().ToString("N"));
            try
            {
                var store = new SessionSaveStore(directory, new FixedClock(), 3);
                TravelState travel = TravelState.Start(catalog); travel.GrantTestCash(520);
                SessionSnapshot snapshot = Snapshot(travel);
                snapshot.items.Add(new SavedItem { id = "loose-a", definitionId = PrototypeItems.Apples.Id, owner = "World", quantity = 1000, quality = 2, ready = true, trailDistance = 24, segmentId = "segment.01.01" });
                snapshot.items.Add(new SavedItem { id = "loose-b", definitionId = PrototypeItems.Flint.Id, owner = "World", quantity = 1000, quality = 3, ready = true, trailDistance = 38, segmentId = "segment.02.01" });
                snapshot.items.Add(new SavedItem { id = "carried", definitionId = PrototypeItems.Branch.Id, owner = "Backpack", quantity = 400, quality = 1, ready = true });
                store.Save(snapshot);
                Check(store.Load(float.MaxValue, out SessionSnapshot loaded, out _) == LoadStatus.Loaded && loaded.version == 3 && loaded.travel.campId == "camp.01.01" && loaded.travel.cash == 520 && loaded.items.Count == 3, "A schema 3 save round-trips travel state and items");
                SessionSaveStore.Validate(loaded, float.MaxValue, out var inventory);
                bool segments = true; foreach (ItemEntry entry in inventory.Entries) segments &= entry.Owner == ItemOwner.World ? entry.SegmentId == (entry.Id == "loose-a" ? "segment.01.01" : "segment.02.01") : entry.SegmentId == null;
                Check(segments, "Trail items keep their segment through reload; carried items have none");

                snapshot.items[2].segmentId = "segment.01.01"; Check(Invalid(snapshot), "A carried item cannot claim a trail segment"); snapshot.items[2].segmentId = null;
                snapshot.items[0].segmentId = null; Check(Invalid(snapshot), "A trail item needs a segment"); snapshot.items[0].segmentId = "segment.01.01";
                snapshot.items[0].segmentId = "segment.99"; Check(Invalid(snapshot), "A trail item's segment must exist"); snapshot.items[0].segmentId = "segment.01.01";
                snapshot.distance = 5; Check(Invalid(snapshot), "Seated saves cannot carry walking progress"); snapshot.distance = 0;
                snapshot.segmentId = "segment.01.02"; Check(Invalid(snapshot), "The saved segment must be the current camp's outgoing segment"); snapshot.segmentId = "segment.01.01";
                snapshot.travel.discovered.Clear(); Check(Invalid(snapshot), "Travel state must be internally consistent"); snapshot.travel = SessionSaveStore.SaveTravel(travel);

                // Background writer: rapid queued saves land in order and a load reads the newest; a rejected snapshot is
                // reported without touching the file.
                TravelState queuedTravel = TravelState.Start(catalog); queuedTravel.GrantTestCash(70);
                for (int cash = 71; cash <= 90; cash++) { queuedTravel.GrantTestCash(1); SessionSnapshot queued = Snapshot(queuedTravel); queued.items.AddRange(snapshot.items); store.SaveInBackground(queued, float.MaxValue); }
                Check(store.Load(float.MaxValue, out SessionSnapshot newest, out _) == LoadStatus.Loaded && newest.travel.cash == 90 && newest.items.Count == 3 && SessionSaveStore.TakeBackgroundFailure() == null,
                    "Queued background saves are written in order; loading waits for them and reads the newest");
                SessionSnapshot rejected = Snapshot(queuedTravel); rejected.distance = 5;
                store.SaveInBackground(rejected, float.MaxValue); SessionSaveStore.FlushBackgroundSaves();
                Check(SessionSaveStore.TakeBackgroundFailure() is InvalidDataException && store.Load(float.MaxValue, out SessionSnapshot kept, out _) == LoadStatus.Loaded && kept.travel.cash == 90 && kept.distance == 0,
                    "A background save that fails validation is reported and leaves the last good save in place");
                store.Save(snapshot);

                // Interrupted transition: the arrival is committed to the save before the scene changes, so a reload resumes it once.
                travel.StandUp(); travel.ArriveAtSegmentEnd();
                SessionSnapshot arrived = Snapshot(travel); store.Save(arrived);
                store.Load(float.MaxValue, out SessionSnapshot resumed, out _);
                TravelState again = SessionSaveStore.RestoreTravel(resumed, catalog);
                Check(again.CurrentCampId == "camp.01.02" && again.VisitNumber == 2 && again.CycleOf("segment.01.02") == 1, "An interrupted arrival resumes at its destination without a second visit or roll");

                // Toll paid before a restart stays paid.
                while (!catalog.IsFinalCamp(again.CurrentCampId)) { again.StandUp(); again.ArriveAtSegmentEnd(); } again.StandUp(); again.PayToll(TollCurrency.Cash); store.Save(Snapshot(again));
                store.Load(float.MaxValue, out SessionSnapshot afterToll, out _);
                Check(SessionSaveStore.RestoreTravel(afterToll, catalog).PaidTolls.Contains("region.01") && afterToll.travel.cash == 20, "A paid toll and the charged cash survive a restart");

                // Migration from the Milestone 4 profile keeps carried items, gear and stamina and starts seated at the first camp.
                var v2 = new SessionSnapshot { version = 2, distance = 40, requestedSpeed = 0.5f, speedCap = 0.8f, yaw = 30, survival = new SavedSurvival { hearts = 2.5, pendingFood = 1 } };
                v2.items.Add(new SavedItem { id = "old-trail", definitionId = PrototypeItems.Stone.Id, owner = "World", quantity = 1000, quality = 1, ready = true, trailDistance = 20 });
                v2.items.Add(new SavedItem { id = "old-bag", definitionId = PrototypeItems.Berries.Id, owner = "Backpack", quantity = 999, quality = 2, ready = true });
                SessionSnapshot migrated = SessionSaveStore.MigrateV2(v2);
                Check(migrated.version == 3 && migrated.travel.campId == "camp.01.01" && migrated.travel.seated && migrated.distance == 0 && migrated.requestedSpeed == 0 && migrated.items.Count == 1 && migrated.items[0].id == "old-bag" && Math.Abs(migrated.survival.hearts - 2.5) < 1e-9 && Math.Abs(migrated.survival.pendingFood - 1) < 1e-9,
                    "Migration keeps carried items and stamina, leaves old trail items behind, and starts seated at Dawn Meadow");
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }

        [MenuItem("The Trail Walker/Milestone 5/Validate Travel Rules")]
        public static void Run()
        {
            passed.Clear();
            TravelCatalog catalog = TravelCatalog.Prototype();
            string finalDawn = catalog.Region("region.01").FinalCamp.Id;
            Check(catalog.Region("region.01").Camps.Count == 10 && finalDawn == "camp.01.10" && catalog.Region("region.01").Camps.Select(c => c.Id).Distinct().Count() == 10,
                "Dawn Plains has its ten proposed camps, ending at Last Light before the river");
            Check(catalog.Regions.Count == 2 && catalog.Outgoing("camp.01.01").ToCampId == "camp.01.02" && catalog.Outgoing(finalDawn).RiverApproach && !catalog.Outgoing("camp.01.03").RiverApproach && catalog.IsFinalCamp("camp.02.12") && !catalog.IsFinalCamp("camp.02.02"), "Catalog links each camp to its outgoing segment; final camps own the river approach");
            bool loose = true; foreach (RegionDefinition region in catalog.Regions) foreach (string id in region.LooseItemIds) loose &= Array.Exists(PrototypeItems.All, d => d.Id == id);
            Check(loose, "Every region loose resource has an item definition");
            Check(Throws(() => new TravelCatalog(catalog.Regions, new[] { new SegmentDefinition("bad", "camp.01.01", "camp.01.03", 10) })), "Segments must follow camp order and every camp needs an outgoing leg");

            TravelState state = TravelState.Start(catalog);
            Check(state.Seated && state.CurrentCampId == "camp.01.01" && state.VisitNumber == 1 && state.DiscoveredRegions.SequenceEqual(new[] { "region.01" }) && state.CycleOf("segment.01.01") == 1, "A new profile starts seated at the first camp with its first drop cycle");
            Check(state.CanStandUp(out _), "Standing up is allowed at a camp with a road onward");
            state.StandUp();
            Check(!state.Seated && state.CycleOf("segment.01.01") == 1 && Throws(() => state.StandUp()), "Standing up does not roll the drop cycle again");
            Check(!state.CanTravelToCamp("camp.01.01", false, out _), "The map is unavailable while walking");
            string rolled = state.ArriveAtSegmentEnd();
            Check(state.Seated && state.CurrentCampId == "camp.01.02" && rolled == "segment.01.02" && state.CycleOf("segment.01.02") == 1 && state.VisitNumber == 2 && state.LastTrailCampId == "camp.01.02", "Reaching the segment end seats the player at the next camp and starts its outgoing cycle");

            Check(!state.CanTravelToCamp("camp.01.02", false, out _), "Selecting the current camp is a no-op");
            Check(!state.CanTravelToCamp("camp.01.03", false, out _), "An unreached camp is locked on the map");
            Check(!state.CanTravelToCamp("camp.01.01", true, out _), "Travel is blocked while trading or recovering");
            Check(!state.CanTravelToRegion("region.02", false, out _), "An undiscovered region cannot be travelled to");
            state.TravelToCamp("camp.01.01", false);
            Check(state.CurrentCampId == "camp.01.01" && state.Seated && state.CycleOf("segment.01.01") == 2 && state.VisitNumber == 3, "Returning to a segment's start camp by map rolls a new drop cycle once");
            TravelState reloaded = Reload(state);
            Check(reloaded.CycleOf("segment.01.01") == 2 && reloaded.VisitNumber == 3 && reloaded.Seated, "Reloading keeps the prepared cycle and visit without rolling again");

            state.StandUp();
            int visitBefore = state.VisitNumber, cycleBefore = state.CycleOf("segment.01.01");
            Check(state.ResumeAtCampAfterRestart() == "segment.01.01" && state.Seated && state.CurrentCampId == "camp.01.01" && state.VisitNumber == visitBefore + 1 && state.CycleOf("segment.01.01") == cycleBefore + 1 && state.ResumeAtCampAfterRestart() == null,
                "Reopening after a mid-segment close returns seated to the previous camp as a camp visit that rerolls its resources");
            while (!catalog.IsFinalCamp(state.CurrentCampId)) { state.StandUp(); state.ArriveAtSegmentEnd(); }
            Check(state.CurrentCampId == finalDawn && catalog.IsFinalCamp(state.CurrentCampId) && state.CanStandUp(out _) && !state.CanPayToll(TollCurrency.Cash, out _) && !state.CanCrossRiver(false, out _),
                "At the region's last camp the road continues to the river; the toll is asked there, not at the seat");
            state.StandUp();
            Check(!state.CanPayToll(TollCurrency.Cash, out _) && !state.CanPayToll(TollCurrency.Gold, out _) && !state.CanCrossRiver(false, out _), "At the river, without enough cash or gold the toll cannot be paid and the ferry cannot cross");
            Check(TravelState.TollGold(catalog.Region("region.01")) == 1 && TravelState.TollGold(catalog.Region("region.02")) == 1 && catalog.Region("region.01").TollCost == 500 && catalog.Region("region.02").TollCost == 1000, "Tolls are 500 / 1,000 cash; the gold toll is cash / 1,000 rounded down, at least 1");
            state.GrantTestCash(550); state.PayToll(TollCurrency.Cash);
            Check(state.Cash == 50 && state.PaidTolls.Contains("region.01") && !state.CanPayToll(TollCurrency.Cash, out _), "The toll is paid once and charged once");
            Check(Reload(state).PaidTolls.Contains("region.01"), "A paid toll stays paid after a reload");
            Check(state.CrossRiver(false) == "segment.02.landing" && state.OnFarBank && !state.Seated && state.CurrentSegment.Landing && state.CycleOf("segment.02.landing") == 1
                && catalog.RegionOf(state.CurrentSegment).Id == "region.02" && !state.DiscoveredRegions.Contains("region.02"),
                "The ferry lands on the far bank at the start of a whole landing leg (with its own resources), not at the next camp");
            TravelState farBank = Reload(state);
            Check(farBank.OnFarBank && farBank.CurrentSegment.Id == "segment.02.landing", "Walking the landing leg is a valid saved state (scene reloads continue it)");
            int visitsBefore = farBank.VisitNumber;
            Check(farBank.ResumeAtCampAfterRestart() == "segment.01.river" && farBank.CurrentCampId == finalDawn && farBank.Seated && !farBank.OnFarBank
                && farBank.PaidTolls.Contains("region.01") && farBank.Cash == 50 && farBank.VisitNumber == visitsBefore + 1,
                "Closing the game on the landing leg returns to the previous camp; the unlock and the money spent persist");
            state.ArriveAtSegmentEnd();
            Check(state.CurrentCampId == "camp.02.01" && state.Seated && !state.OnFarBank && state.DiscoveredRegions.SequenceEqual(new[] { "region.01", "region.02" }) && state.CycleOf("segment.02.01") == 1,
                "Reaching the end of the landing leg seats the player at the next region's first camp and discovers it");

            state.TravelToRegion("region.01", false);
            Check(state.CurrentCampId == "camp.01.01", "Cross-region travel arrives at the region's first camp, not its last visited camp");

            // Debug UNLOCK CAMPS (November 2, 2026): every camp of the current region only, persisted, nothing else changes.
            var fresh = TravelState.Start(catalog);
            string lastDawn = catalog.StartRegion.FinalCamp.Id;
            int cyclesBefore = fresh.DropCycles.Count, freshVisit = fresh.VisitNumber;
            Check(!fresh.CanTravelToCamp(lastDawn, false, out _) && fresh.UnlockRegionCampsForTest() == catalog.StartRegion.Camps.Count - 1 && fresh.UnlockRegionCampsForTest() == 0
                && fresh.CanTravelToCamp(lastDawn, false, out _) && fresh.DiscoveredRegions.SequenceEqual(new[] { "region.01" }) && !fresh.VisitedCamps.Contains("camp.02.01")
                && fresh.DropCycles.Count == cyclesBefore && fresh.VisitNumber == freshVisit && fresh.Seated,
                "UNLOCK CAMPS opens every camp of the current region on the map without discovering other regions, rolling drops or moving the player");
            Check(Reload(fresh).CanTravelToCamp(lastDawn, false, out _), "Unlocked camps persist through save and reload");
            state.TravelToRegion("region.02", false);
            Check(state.CurrentCampId == "camp.02.01" && !state.CanTravelToRegion("region.03", false, out _), "Region list returns to discovered regions only; the next region stays locked");
            state.StandUp(); state.ArriveAtSegmentEnd();
            Check(state.CurrentCampId == "camp.02.03" && !catalog.IsFinalCamp("camp.02.03"), "Windward Hills gains its second camp, Quiet Hilltop, between the existing camps");
            // Walk the rest of Windward Hills to its final camp (Far Lookout), then up to the river.
            while (!catalog.IsFinalCamp(state.CurrentCampId)) { state.StandUp(); state.ArriveAtSegmentEnd(); }
            Check(state.CurrentCampId == "camp.02.12" && catalog.Region("region.02").Camps.Count == 12, "Windward Hills runs through its twelve camps to Far Lookout");
            state.StandUp(); state.GrantTestGold(2); state.PayToll(TollCurrency.Gold);
            Check(state.Gold == 1 && state.PaidTolls.Contains("region.02") && Reload(state).Gold == 1, "The toll can be paid in gold instead, and the gold wallet persists");
            Check(!state.CanCrossRiver(false, out string notYet) && notYet.Contains("later milestone"), "The ferry beyond the last authored region does not cross");

            Check(!Throws(() => TravelState.Restore(catalog, finalDawn, false, finalDawn, 1, 0, new[] { finalDawn }, new[] { "region.01" }, new string[0], new[] { new KeyValuePair<string, int>("segment.01.river", 1) })), "A save can hold the walk from the last camp up to the river (the ferry asks for the toll there)");
            // Layout migration: a profile at Low Meadow from when it was the last camp (river segment) is reseated there.
            var legacy = new SessionSnapshot { version = 3, segmentId = "segment.01.river", distance = 12, travel = new SavedTravel { campId = "camp.01.03", lastTrailCampId = "camp.01.03", seated = false, visitNumber = 4,
                visited = new List<string> { "camp.01.01", "camp.01.02", "camp.01.03" }, discovered = new List<string> { "region.01" }, paidTolls = new List<string>(), cycles = new List<SavedCycle> { new SavedCycle { segmentId = "segment.01.river", cycle = 2 } } } };
            SessionSaveStore.MigrateCatalogLayout(legacy);
            Check(legacy.segmentId == "segment.01.03" && legacy.travel.seated && legacy.distance == 0 && legacy.travel.cycles.Exists(c => c.segmentId == "segment.01.03" && c.cycle == 1)
                && !Throws(() => SessionSaveStore.RestoreTravel(legacy, catalog)), "Saves from the three-camp Dawn Plains are reseated at their camp with a drop cycle for its new leg");
            // Same migration for the three-camp Windward Hills: a walk from Low Hilltop to its old river is reseated there.
            var hills = new SessionSnapshot { version = 3, segmentId = "segment.02.river", distance = 20, travel = new SavedTravel { campId = "camp.02.02", lastTrailCampId = "camp.02.02", seated = false, visitNumber = 9,
                visited = new List<string> { "camp.01.01", "camp.02.01", "camp.02.03", "camp.02.02" }, discovered = new List<string> { "region.01", "region.02" }, paidTolls = new List<string> { "region.01", "region.02" },
                cycles = new List<SavedCycle> { new SavedCycle { segmentId = "segment.02.river", cycle = 1 }, new SavedCycle { segmentId = "segment.01.01", cycle = 1 } } } };
            SessionSaveStore.MigrateCatalogLayout(hills);
            TravelState migrated = Throws(() => SessionSaveStore.RestoreTravel(hills, catalog)) ? null : SessionSaveStore.RestoreTravel(hills, catalog);
            Check(hills.segmentId == "segment.02.02" && hills.travel.seated && hills.distance == 0 && migrated != null && migrated.CurrentCampId == "camp.02.02"
                && migrated.PaidTolls.Contains("region.02") && !catalog.IsFinalCamp("camp.02.02"), "Saves from the three-camp Windward Hills are reseated at Low Hilltop facing Windy Crest; the paid toll is kept");
            Check(Throws(() => TravelState.Restore(catalog, "camp.02.01", true, "camp.02.01", 1, 0, new[] { "camp.02.01" }, new[] { "region.01" }, new string[0], new[] { new KeyValuePair<string, int>("segment.02.01", 1) })), "A save cannot hold a camp in an undiscovered region");
            Check(Throws(() => TravelState.Restore(catalog, "camp.01.01", true, "camp.01.01", 1, 0, new[] { "camp.01.01" }, new[] { "region.01" }, new string[0], new KeyValuePair<string, int>[0])), "A save must carry the current segment's drop cycle");

            RegionDefinition dawn = catalog.Region("region.01"); SegmentDefinition first = catalog.Segment("segment.01.01");
            var rollA = LooseDrops.Roll(dawn, first, 1); var rollB = LooseDrops.Roll(dawn, first, 1);
            bool same = rollA.Count == rollB.Count; for (int i = 0; same && i < rollA.Count; i++) same = rollA[i].ItemId == rollB[i].ItemId && rollA[i].Quality == rollB[i].Quality && rollA[i].Distance == rollB[i].Distance;
            Check(rollA.Count > 0 && same, "The same segment and cycle always roll the same drops");
            bool valid = true; foreach (var drop in rollA) valid &= dawn.LooseItemIds.Contains(drop.ItemId) && drop.Quality >= 1 && drop.Quality <= 5 && drop.Distance > 0 && drop.Distance < first.Length;
            Check(valid, "Drops use only the region's three loose resources, Q1-Q5, inside the segment");
            var rollC = LooseDrops.Roll(dawn, first, 2); bool positions = rollC.Count == rollA.Count; for (int i = 0; positions && i < rollC.Count; i++) positions = rollC[i].Distance == rollA[i].Distance && rollC[i].LeftSide == rollA[i].LeftSide;
            Check(positions, "A new cycle keeps the fixed point positions and only rerolls identity/quality");
            Check(LooseDrops.Roll(dawn, catalog.Segment("segment.01.river"), 1).Count == 0, "The river approach has no loose drops");
            bool density = true, trees = true, animals = true, fishing = true, inside = true;
            foreach (RegionDefinition region in catalog.Regions)
                foreach (CampDefinition camp in region.Camps)
                {
                    SegmentDefinition leg = catalog.Outgoing(camp.Id); if (leg.RiverApproach) continue;
                    density &= LooseDrops.Roll(region, leg, 1).Count >= 20;
                    var layout = SegmentLayout.Build(region, leg);
                    trees &= layout.Count(a => a.Kind == InteractableKind.Tree) >= 4;
                    var spawns = layout.Where(a => a.Kind == InteractableKind.AnimalSpawn).ToList();
                    animals &= spawns.Count >= 2 && spawns.Count == SegmentLayout.AnimalSpawnCount(leg) && (leg.Length >= 450 || spawns.Count <= 3) && spawns.All(a => a.Animals >= 1 && a.Animals <= 4);
                    int ponds = layout.Count(a => a.Kind == InteractableKind.FishingPoint);
                    fishing &= region.Number == 1 ? ponds == 1 : ponds >= 2 && ponds == SegmentLayout.FishingCount(region, leg) && (leg.Length >= 500 || ponds <= 3);
                    inside &= layout.All(a => a.Distance > 10 && a.Distance < leg.Length - 10 && Math.Abs(a.Lateral) >= 6);
                }
            Check(density, "Every walkable segment has at least 20 gathering points");
            Check(trees && animals && fishing, "Each segment has at least 4 trees, 2-3 animal spawns of 1-4 animals (more from 450 m), and 1 fishing point in region 1 or 2-3 elsewhere (more from 500 m)");
            Check(inside, "Interactables stay inside the segment, clear of the camps and off the pickup rows");
            var longLeg = new SegmentDefinition("long", "camp.01.01", "camp.01.02", 900);
            bool flatPlains = catalog.Region("region.01").Camps.All(c => catalog.Outgoing(c.Id).Relief == 0);
            Check(flatPlains && catalog.Segment("segment.02.01").Relief > 0 && catalog.Segment("segment.02.03").Relief > catalog.Segment("segment.02.01").Relief,
                "Dawn Plains is flat; Windward Hills starts with a gentle transition, then a hillier leg");
            var allLegs = catalog.Regions.SelectMany(r => r.Camps).Select(c => catalog.Outgoing(c.Id)).Where(x => x != null).ToList();
            Check(allLegs.All(TerrainClearsRoad), "The terrain stays below the whole road surface on every leg (flat legs, climbs and descents)");
            // November 2, 2026 (user): the road looked raised above the ground on hill legs and dipped into camps in a V.
            var raised = allLegs.Where(x => RoadAboveGround(x) > 0.25f).Select(x => $"{x.Id} {RoadAboveGround(x):0.00} m").ToList();
            Check(raised.Count == 0, "The road sits on the ground on every leg: at most 0.25 m above the terrain at its edges" + (raised.Count > 0 ? ": " + string.Join(", ", raised) : ""));
            var unlevel = allLegs.Where(x => !LevelEnds(x)).Select(x => x.Id).ToList();
            Check(unlevel.Count == 0, "Every leg arrives at and leaves its camps level (no V-shaped dip at a camp)" + (unlevel.Count > 0 ? ": " + string.Join(", ", unlevel) : ""));
            var floating = FloatingCamps(catalog);
            Check(floating.Count == 0, "Every camp clearing sits on levelled ground at both segment ends (no floating camps)" + (floating.Count > 0 ? ": " + string.Join(", ", floating) : ""));
            var pickupGround = PickupGroundMismatches(catalog, out float oldGap);
            Debug.Log($"[Milestone5] Pickup spots: the old road-height rule left them up to {oldGap:0.00} m above the terrain.");
            Check(pickupGround.Count == 0, "The pickup ground lookup matches the drawn terrain at every pickup spot of every leg, camps included (pickups rest on the ground)"
                + (pickupGround.Count > 0 ? ": " + string.Join(", ", pickupGround.Take(8)) : ""));
            CampNpcChecks();
            Check(StorageGearHasOwnArt(), "Every craftable shirt and backpack has its own inventory container art (pockets; bag and opening); the belt is universal");
            Check(SegmentLayout.TreeCount(longLeg) > 4 && SegmentLayout.AnimalSpawnCount(longLeg) >= 3 && LooseDrops.PointCount(longLeg) > 20, "Longer legs between camps get more resources and interactables");

            ArtIntegrationChecks(catalog);
            EquipmentUiChecks();
            PersistenceChecks(catalog);
            StaleWorldItemChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Milestone5-checks.txt", passed);
            Debug.Log($"[Milestone5] PASS: {passed.Count} travel checks.");
        }
    }

    [InitializeOnLoad]
    internal static class Milestone5CheckRequest
    {
        static Milestone5CheckRequest() { EditorApplication.update += Poll; }
        private static void Poll()
        {
            const string request = "Temp/Milestone5-check.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(request);
            try { Milestone5Validation.Run(); File.WriteAllText("Temp/Milestone5-check.result", "PASS"); }
            catch (Exception error) { Debug.LogException(error); File.WriteAllText("Temp/Milestone5-check.result", "FAILED " + error); }
        }
    }
}
