using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Settlement shell, milestone 10 part 1 (spec 2026-11-08). Chained after Phase 1 consistency in the suite.
    public static class SettlementShellValidation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Settlement shell: " + name); passed.Add(name); }
        private static bool Throws(Action action) { try { action(); return false; } catch (ArgumentException) { return true; } catch (InvalidOperationException) { return true; } }

        public static void Run()
        {
            passed.Clear();
            StateChecks();
            TravelChecks();
            PersistenceChecks();
            StreetChecks();
            PropChecks();
            BlockoutChecks();
            HudChecks();
            SettlementMapTravelChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Settlement-shell-checks.txt", passed);
        }

        private static TravelState Camp(TravelCatalog c, string camp, bool seated, bool inSettlement = false, bool approach = false, bool approachCycle = true)
        {
            var visited = new[] { "camp.01.01", "camp.01.02", "camp.01.03", "camp.02.01", "camp.03.01", "camp.04.01", "camp.04.13" };
            var regions = new[] { "region.01", "region.02", "region.03", "region.04" };
            var cycles = new List<KeyValuePair<string, int>> { new KeyValuePair<string, int>(c.Outgoing(camp).Id, 1) };
            if (approachCycle) cycles.Add(new KeyValuePair<string, int>("segment.settlement.approach", 1));
            return TravelState.Restore(c, camp, seated, camp, 10, 5000, visited, regions, new[] { "region.03", "region.04" }, cycles, 0, false, approach, inSettlement, inSettlement ? camp : null);
        }

        // Task 7: map travel from the settlement goes to the region's first camp and becomes the last visited camp.
        private static void SettlementMapTravelChecks()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            TravelState state = Camp(catalog, "camp.03.01", true);
            state.TravelToSettlement(true, false);
            Check(state.InSettlement && state.CanTravelToRegion("region.01", false, out _) && state.CanTravelToRegion("region.03", false, out _), "In the settlement any discovered region (including the return camp's) can be travelled to");
            Check(!state.CanTravelToRegion("region.09", false, out _) && !state.CanTravelToRegion("region.01", true, out _), "In the settlement an unknown region or a busy player cannot travel");
            int visit = state.VisitNumber;
            state.TravelToRegion("region.01", false);
            Check(state.Seated && !state.InSettlement && state.SettlementReturnCampId == null && state.CurrentCampId == catalog.Region("region.01").FirstCamp.Id && state.LastTrailCampId == state.CurrentCampId && state.VisitNumber == visit + 1,
                "Settlement map travel lands seated at the region's first camp as the last visited camp");
            state.TravelToSettlement(true, false); state.ReturnFromSettlement();
            Check(state.Seated && state.CurrentCampId == "camp.01.01", "Return after map travel goes back to that region's first camp");
            TravelState outside = Camp(catalog, "camp.03.01", true);
            Check(!outside.CanTravelToRegion("region.03", false, out string why) && why == "Choose a camp in this region instead", "Outside the settlement the current region is still refused with the old reason");
            Check(state.VisitNumber == visit + 2 && CollectionPrototype.SettlementSeed(visit + 1, "camp.01.01") != CollectionPrototype.SettlementSeed(visit, "camp.03.01"), "Settlement arrival after map travel gets a new visit id (restock)");
        }

        private static void TravelChecks()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            const string approachId = "segment.settlement.approach";

            // Arch.
            TravelState arch = Camp(catalog, "camp.04.13", false, false, true);
            int archVisit = arch.VisitNumber;
            Check(arch.ReachSettlementArch() == approachId && arch.InSettlement && !arch.Seated && !arch.OnSettlementApproach && arch.SettlementReturnCampId == "camp.04.13" && arch.VisitNumber == archVisit,
                "The arch enters the settlement with Bright Hollow as the return camp");
            Check(arch.CurrentSegment.Id == approachId && !arch.CanStandUp(out string standReason) && standReason == "You are in the settlement", "In the settlement the segment is the approach and standing up is refused");

            // Shortcut.
            TravelState camp = Camp(catalog, "camp.01.03", true);
            Check(!camp.CanTravelToSettlement(false, false, out string undiscovered) && !string.IsNullOrEmpty(undiscovered) && Throws(() => camp.TravelToSettlement(false, false)), "The shortcut is refused while the settlement is undiscovered");
            Check(!camp.CanTravelToSettlement(true, true, out string busy) && busy.Contains("only available while seated"), "The shortcut is refused while busy");
            TravelState walking = Camp(catalog, "camp.01.03", false);
            Check(!walking.CanTravelToSettlement(true, false, out string walkReason) && walkReason.Contains("only available while seated") && Throws(() => walking.TravelToSettlement(true, false)), "The shortcut is refused while walking");
            Check(camp.CanTravelToSettlement(true, false, out string ok) && ok == "", "The shortcut is allowed while seated at camp.01.03");
            int visit = camp.VisitNumber, cycle = camp.CycleOf(catalog.Outgoing("camp.01.03").Id);
            Check(camp.TravelToSettlement(true, false) == approachId && camp.InSettlement && !camp.Seated && camp.SettlementReturnCampId == "camp.01.03", "The shortcut enters the settlement with the starting camp as the return camp");

            // Regression: a profile that never crossed the ferry has no approach cycle; entering must roll one so drops can be rolled.
            TravelState fresh = Camp(catalog, "camp.01.03", true, false, false, false);
            SegmentDefinition approachSeg = catalog.SettlementApproach;
            Check(fresh.CycleOf(approachId) == 0, "Fixture: no approach cycle before entering");
            fresh.TravelToSettlement(true, false);
            Check(fresh.CycleOf(approachId) >= 1 && !Throws(() => LooseDrops.Roll(catalog.RegionOf(approachSeg), approachSeg, fresh.CycleOf(approachId))), "The shortcut rolls an approach drop cycle that LooseDrops accepts");
            TravelState viaArch = Camp(catalog, "camp.04.13", false, false, true);
            int archCycle = viaArch.CycleOf(approachId); viaArch.ReachSettlementArch();
            Check(viaArch.CycleOf(approachId) == archCycle + 1 && viaArch.CycleOf(approachId) >= 1, "The arch bumps the approach drop cycle");

            // Restart in the settlement.
            Check(camp.ResumeAtCampAfterRestart() == null && camp.InSettlement && !camp.Seated, "A restart in the settlement stays in the settlement");

            // Restore.
            TravelState again = TravelState.Restore(catalog, camp.CurrentCampId, camp.Seated, camp.LastTrailCampId, camp.VisitNumber, camp.Cash, camp.VisitedCamps.ToList(), camp.DiscoveredRegions.ToList(), camp.PaidTolls.ToList(), camp.DropCycles.ToList(), camp.Gold, false, false, true, "camp.01.03");
            Check(again.InSettlement && again.SettlementReturnCampId == "camp.01.03" && !again.Seated, "Restore round-trips the settlement fields");
            var v = camp.VisitedCamps.ToList(); var d = camp.DiscoveredRegions.ToList(); var t = camp.PaidTolls.ToList(); var cy = camp.DropCycles.ToList();
            Check(Throws(() => TravelState.Restore(catalog, "camp.01.03", true, "camp.01.03", 10, 0, v, d, t, cy, 0, false, false, true, "camp.01.03")), "Restore rejects in-settlement while seated");
            Check(Throws(() => TravelState.Restore(catalog, "camp.01.03", false, "camp.01.03", 10, 0, v, d, t, cy, 0, false, false, true, "camp.99.99")), "Restore rejects an unknown return camp");
            Check(Throws(() => TravelState.Restore(catalog, "camp.01.03", false, "camp.01.03", 10, 0, v, d, t, cy, 0, false, false, true, "camp.01.04")), "Restore rejects a return camp never visited");

            // Return.
            Check(camp.CanReturnFromSettlement(out _) && camp.ReturnFromSettlement() == catalog.Outgoing("camp.01.03").Id && camp.Seated && !camp.InSettlement && camp.CurrentCampId == "camp.01.03"
                && camp.SettlementReturnCampId == null && camp.VisitNumber == visit + 1 && camp.CycleOf(catalog.Outgoing("camp.01.03").Id) == cycle + 1, "Return seats at the shortcut camp as a new visit with fresh drops");
            Check(!camp.CanReturnFromSettlement(out string notIn) && !string.IsNullOrEmpty(notIn) && Throws(() => camp.ReturnFromSettlement()), "Return is refused outside the settlement");
        }

        private sealed class FixedClock : IClock { public DateTime UtcNow => new DateTime(2026, 11, 8, 0, 0, 0, DateTimeKind.Utc); }
        private static bool Invalid(SessionSnapshot snapshot) { try { SessionSaveStore.Validate(snapshot, float.MaxValue, out _); return false; } catch (InvalidDataException) { return true; } }
        private static SessionSnapshot Snapshot(TravelState travel) => new SessionSnapshot { version = 3, segmentId = travel.CurrentSegment.Id, survival = new SavedSurvival(), travel = SessionSaveStore.SaveTravel(travel) };

        private static void PersistenceChecks()
        {
            TravelCatalog catalog = TravelCatalog.Prototype();
            string directory = Path.Combine(Path.GetTempPath(), "TrailWalkerSettlement-" + Guid.NewGuid().ToString("N"));
            try
            {
                var store = new SessionSaveStore(directory, new FixedClock(), 3);
                TravelState inside = Camp(catalog, "camp.01.03", true); inside.TravelToSettlement(true, false);
                SettlementState chosen = SettlementState.New(11); chosen.Discover(); chosen.ChoosePlot(4, new[] { "region.01", "region.02" });
                chosen.ApplyNewRegions(new[] { "region.05" });
                SessionSnapshot snap = Snapshot(inside); snap.settlement = SessionSaveStore.SaveSettlement(chosen);
                store.Save(snap);
                Check(store.Load(float.MaxValue, out SessionSnapshot loaded, out _) == LoadStatus.Loaded && loaded.travel.inSettlement && loaded.travel.settlementReturnCampId == "camp.01.03" && !loaded.travel.seated
                    && loaded.segmentId == "segment.settlement.approach", "An in-settlement snapshot round-trips through Save and Load, and migration leaves it alone");
                TravelState travelBack = SessionSaveStore.RestoreTravel(loaded, catalog);
                Check(travelBack.InSettlement && travelBack.SettlementReturnCampId == "camp.01.03" && travelBack.CurrentCampId == "camp.01.03", "The travel settlement fields restore");
                SettlementState back = SessionSaveStore.RestoreSettlement(loaded, 99);
                Check(back.Discovered && back.PlotIndex == 4 && back.Seed == 11 && back.Neighbours.Select(n => (n.Plot, n.Level, n.Countdown)).SequenceEqual(chosen.Neighbours.Select(n => (n.Plot, n.Level, n.Countdown)))
                    && back.CountedRegions.SequenceEqual(chosen.CountedRegions), "The settlement block round-trips every field");

                // Migration must not reseat an in-settlement save (its segment is the approach, not the camp's outgoing leg).
                SessionSnapshot migrate = Snapshot(inside); SessionSaveStore.MigrateCatalogLayout(migrate);
                Check(!migrate.travel.seated && migrate.travel.inSettlement && migrate.segmentId == "segment.settlement.approach", "MigrateCatalogLayout leaves an in-settlement save unchanged");

                // An older save without the fields.
                TravelState approach = Camp(catalog, "camp.04.13", false, false, true);
                SessionSnapshot old = Snapshot(approach); old.settlement = null; old.travel.inSettlement = false; old.travel.settlementReturnCampId = null;
                string oldJson = UnityEngine.JsonUtility.ToJson(old);
                oldJson = oldJson.Replace(",\"inSettlement\":false", "").Replace(",\"settlementReturnCampId\":\"\"", "");
                SessionSnapshot parsed = UnityEngine.JsonUtility.FromJson<SessionSnapshot>(oldJson);
                SessionSaveStore.MigrateCatalogLayout(parsed);
                Check(!Invalid(parsed) && parsed.travel.settlementApproach && !parsed.travel.inSettlement && string.IsNullOrEmpty(parsed.travel.settlementReturnCampId), "An approach save from the earlier build still loads, outside the settlement");
                TravelState oldTravel = SessionSaveStore.RestoreTravel(parsed, catalog);
                Check(oldTravel.OnSettlementApproach && !oldTravel.InSettlement && oldTravel.ReachSettlementArch() == "segment.settlement.approach", "The earlier approach save can still reach the arch");
                SettlementState none = SessionSaveStore.RestoreSettlement(parsed, 42);
                Check(!none.Discovered && none.PlotIndex == -1 && none.Seed == 42 && none.Neighbours.Count == 0, "A save without a settlement block loads as undiscovered with the given seed");

                // Discovered, plot unchosen.
                SettlementState found = SettlementState.New(5); found.Discover();
                SessionSnapshot unchosen = Snapshot(inside); unchosen.settlement = SessionSaveStore.SaveSettlement(found);
                SettlementState foundBack = SessionSaveStore.RestoreSettlement(unchosen, 1);
                Check(!Invalid(unchosen) && foundBack.Discovered && foundBack.PlotIndex == -1 && foundBack.CanChoosePlot(2, out _), "A discovered state with plot -1 restores as discovered and unchosen");

                // Validation.
                SessionSnapshot bad = Snapshot(inside); bad.settlement = SessionSaveStore.SaveSettlement(chosen);
                bad.settlement.plotIndex = 9; Check(Invalid(bad), "Validate rejects a settlement block with an invalid plot");
                bad.settlement = SessionSaveStore.SaveSettlement(chosen); bad.settlement.neighbours[0].level = 0; Check(Invalid(bad), "Validate rejects a neighbour at level 0");
                bad.settlement = SessionSaveStore.SaveSettlement(chosen); Check(!Invalid(bad), "Validate accepts a good settlement block");

                // The seed derived for a new state is stable per profile.
                SessionSnapshot a = Snapshot(inside), b = Snapshot(inside);
                Check(TheTrailWalker.CollectionPrototype.SettlementSeed(a) == TheTrailWalker.CollectionPrototype.SettlementSeed(b) && TheTrailWalker.CollectionPrototype.SettlementSeed(a) >= 0, "The derived settlement seed is deterministic");
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }

        // ---- Street scene (Task 4) ----
        [Serializable] private sealed class ManifestShop { public string Name; public string Front; public float Width; }
        [Serializable] private sealed class Manifest { public ManifestShop[] Shops; }

        private static StreetBuilding Find(SettlementStreet street, string id) => street.Buildings.First(b => b.Id == id);
        private static bool HasChild(Transform root, string name) => Enumerable.Range(0, root.childCount).Any(i => root.GetChild(i).name == name);
        private static int CountChildren(Transform root, string prefix) => Enumerable.Range(0, root.childCount).Count(i => root.GetChild(i).name.StartsWith(prefix));

        private static void StreetChecks()
        {
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/_Game/Art/Settlement/Storefronts/StorefrontManifest.json"));
            Check(manifest.Shops.Length == SettlementScenery.Storefronts.Count && manifest.Shops.Select((m, i) => m.Name == SettlementScenery.Storefronts[i].Name && m.Front == SettlementScenery.Storefronts[i].Front
                && Mathf.Approximately(m.Width, SettlementScenery.Storefronts[i].Width)).All(x => x), "The runtime storefront table equals the manifest names, fronts and widths");

            var root = new GameObject("Street check root");
            try
            {
                int[] levels = { 1, 2, 3, 4, 5, 6 };
                SettlementStreet street = SettlementScenery.Build(root.transform, levels, 4);
                var ids = street.Buildings.Select(b => b.Id).ToArray();
                Check(ids.Length == 14 && street.Buildings.Count(b => b.Kind == StreetBuildingKind.House) == 6 && street.Buildings.Count(b => b.Kind == StreetBuildingKind.Shop) == 6
                    && street.Buildings.Count(b => b.Kind == StreetBuildingKind.Warehouse) == 1 && street.Buildings.Count(b => b.Kind == StreetBuildingKind.LumberField) == 1, "The street has 6 houses, 6 shops, 1 warehouse and 1 lumber frontage");
                var order = street.Buildings.OrderBy(b => b.CentreX).Select(b => b.Id).ToArray();
                Check(order.SequenceEqual(new[] { "plot.0", "plot.1", "plot.2", "warehouse", "shop.bakery", "shop.ranch", "shop.farm", "shop.fishery", "shop.mine", "shop.tailor", "lumber", "plot.3", "plot.4", "plot.5" }),
                    "The street runs plots 0-2, Warehouse, Bakery, Ranch, Farm, Fishery, Mine, Tailor, lumber, plots 3-5 left to right");
                Check(ids.Distinct().Count() == ids.Length && Find(street, "plot.4").Plot == 4 && Find(street, "warehouse").Plot == -1, "Building ids are unique and only houses carry a plot");

                Check(Mathf.Abs(Find(street, "plot.1").CentreX - Find(street, "plot.0").CentreX - SettlementScenery.PlotSpacing) < 0.001f && Mathf.Abs(Find(street, "plot.2").CentreX - Find(street, "plot.1").CentreX - SettlementScenery.PlotSpacing) < 0.001f
                    && Mathf.Abs(Find(street, "plot.4").CentreX - Find(street, "plot.3").CentreX - SettlementScenery.PlotSpacing) < 0.001f && Mathf.Abs(Find(street, "plot.5").CentreX - Find(street, "plot.4").CentreX - SettlementScenery.PlotSpacing) < 0.001f, $"Plots are {SettlementScenery.PlotSpacing:0.0} m apart (15.2 m plots and a {SettlementScenery.PlotLane:0.0} m lane)");

                var fronts = new[] { "warehouse", "shop.bakery", "shop.ranch", "shop.farm", "shop.fishery", "shop.mine", "shop.tailor", "lumber" };
                bool spacing = true;
                float Slot(int i) => SettlementScenery.SlotWidth(i, manifest.Shops[i].Width);
                for (int i = 0; i + 1 < fronts.Length; i++)
                    spacing &= Mathf.Abs(Find(street, fronts[i + 1]).CentreX - Find(street, fronts[i]).CentreX - ((Slot(i) + Slot(i + 1)) / 2f + SettlementScenery.SlotGap)) < 0.001f;
                Check(spacing, "Each storefront is centred in its fixed slot and slots stand 1 m apart, the lumber frontage included");

                // Space planning (user, November 9, 2026): today's art sits inside the envelopes reserved for the future looks
                // (storefront tier looks, house levels 9-16), so later art can grow without moving the street.
                bool inSlots = true, behindLine = true, housesInEnvelope = true; string worst = "";
                for (int i = 0; i < fronts.Length; i++)
                {
                    var (l, r, _) = SettlementStreetProps.Extent(Find(street, fronts[i]));
                    float spare = Slot(i) - (r - l);
                    if (spare < 0.6f) { inSlots = false; worst += $" {fronts[i]} {r - l:0.00}/{Slot(i):0.00}"; }
                }
                foreach (StreetBuilding b in street.Buildings)
                {
                    var box = b.Root.GetComponent<BoxCollider>();
                    if (box.center.z - box.size.z / 2f < -SettlementScenery.FrontLine - 0.01f) behindLine = false;
                    if (b.Kind == StreetBuildingKind.House) { var (l, r, _) = SettlementStreetProps.Extent(b); housesInEnvelope &= r - l <= SettlementScenery.HouseEnvelopeWidth; }
                }
                Check(inSlots, "Every storefront today leaves at least 0.6 m of its slot for its later tier looks" + worst);
                Check(behindLine && SettlementBackdrop.RoadFar <= -SettlementScenery.FrontLine - 0.4f, "No front reaches past the 1.5 m front line, and the road starts beyond it");
                Check(housesInEnvelope && Mathf.Abs(SettlementScenery.PlotWidth - 2f * SettlementScenery.SideStripOuter - 0.4f) < 0.01f
                    && Mathf.Abs(SettlementScenery.HouseEnvelopeWidth / 2f + SettlementScenery.SideStripWidth - SettlementScenery.SideStripOuter) < 0.01f
                    && SettlementScenery.WingWidth <= SettlementScenery.PlotWidth - 0.39f && SettlementScenery.PlotSpacing - SettlementScenery.WingWidth >= 3f,
                    "Houses fit the 11 m front envelope; the side strips hug an 11 m front and stop 0.2 m inside the 15.2 m plot; wings stay inside the plot, and the widest neighbours' wings stand at least 3 m apart");
                // House exteriors (every level in the manifest) stay inside the envelope, and from level 3 (pets) the pet home and
                // the collection container stand at their per-level anchors inside 1.9 m strips hugging the house front.
                var houseManifest = JsonUtility.FromJson<HouseManifestRows>(File.ReadAllText("Assets/_Game/Art/Settlement/Houses/HouseManifest.json"));
                bool exteriorsFit = houseManifest.Houses.Length >= 8, sidesFit = true; string sideReport = "";
                foreach (HouseRow row in houseManifest.Houses)
                {
                    GameObject front = Resources.Load<GameObject>("Settlement/Houses/" + row.Front);
                    if (front != null && !WingRule(front, out string frontWings)) { exteriorsFit = false; passed.Add($"Wing rule broken by {row.Front}: {frontWings}"); }
                    if (front != null) { Bounds fb = PrefabBounds(front); exteriorsFit &= fb.size.x <= SettlementScenery.WingWidth + 0.01f && fb.max.y <= SettlementScenery.MaxHouseHeight + 0.01f && -fb.min.z <= SettlementScenery.EnvelopeDepth + 0.01f && TopShare(front) <= 1f - 2f * SettlementScenery.TopMarginShare; }
                    exteriorsFit &= row.Width <= SettlementScenery.HouseEnvelopeWidth + 0.01f;
                    if (row.Level < 3) continue;
                    foreach ((string prefab, float[] anchor) in new[] { ("PlayerPetHome", row.PetHomeAnchor), ("PetCollectionContainer", row.CollectionAnchor) })
                    {
                        GameObject side = Resources.Load<GameObject>("Settlement/Houses/" + prefab);
                        if (side == null || anchor == null || anchor.Length < 1) continue;
                        Bounds sb = PrefabBounds(side); float half = Mathf.Max(-sb.min.x, sb.max.x), at = Mathf.Abs(anchor[0]);
                        bool ok = at - half >= row.Width / 2f - 0.05f && at + half <= row.Width / 2f + SettlementScenery.SideStripWidth + 0.05f;
                        sidesFit &= ok; if (!ok) sideReport += $" L{row.Level} {prefab} at {at:0.00}";
                    }
                }
                Check(exteriorsFit, "Every house exterior fits the plot envelope (front ≤ 11 m, wings ≤ 14.8 m, height ≤ 13 m, depth ≤ 19 m) and its full height shows below the title line (top 5.5% of the street screen)");
                Check(sidesFit, "The pet home and the collection container fit the strips hugging each house front (levels 3+)" + sideReport);
                // Pet home looks (handoff pet-home-levels-handoff-2026-11-10.md): seven passive prefabs on the shared house palette,
                // each inside the 1.9 m strip, at most 3 m deep and 1.5 m in front of the front face at its anchor, with an
                // ActivePetAnchor; the house level picks the look by tier.
                Check(SettlementScenery.PetHomeLook(1) == null && SettlementScenery.PetHomeLook(2) == null && SettlementScenery.PetHomeLook(3) == "PetHomeT2" && SettlementScenery.PetHomeLook(4) == "PetHomeT2"
                    && SettlementScenery.PetHomeLook(8) == "PetHomeT4" && SettlementScenery.PetHomeLook(9) == "PetHomeT5" && SettlementScenery.PetHomeLook(16) == "PetHomeT8",
                    "The pet home look follows the house tier (none before level 3, kennel at 3-4, ebony pavilion at 15-16)");
                Material housePalette = Resources.Load<Material>("Settlement/Houses/HousePalette");
                float anchorZ = houseManifest.Houses.Where(h => h.Level >= 3 && h.PetHomeAnchor != null && h.PetHomeAnchor.Length >= 3).Select(h => h.PetHomeAnchor[2]).DefaultIfEmpty(0.25f).Max();
                var petRows = houseManifest.PetHomes ?? new PetHomeRow[0];
                for (int tier = 2; tier <= 8; tier++)
                {
                    string look = "PetHomeT" + tier;
                    GameObject home = Resources.Load<GameObject>("Settlement/Houses/" + look);
                    if (home == null) { passed.Add("WAITING (not delivered yet): Settlement/Houses/" + look); continue; }
                    var filters = home.GetComponentsInChildren<MeshFilter>(true);
                    int triangles = filters.Sum(f => f.sharedMesh != null ? f.sharedMesh.triangles.Length / 3 : 0);
                    Bounds hb = PrefabBounds(home);
                    Transform pet = home.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ActivePetAnchor");
                    PetHomeRow row = petRows.FirstOrDefault(r => r.Name == look);
                    Check(home.GetComponentsInChildren<MonoBehaviour>(true).Length == 0 && home.GetComponentsInChildren<Collider>(true).Length == 0 && filters.All(f => f.sharedMesh != null && f.sharedMesh.isReadable)
                        && home.GetComponentsInChildren<MeshRenderer>(true).All(r => r.sharedMaterials.All(m => m == housePalette)), look + " is passive, readable and uses the shared house palette");
                    Check(triangles <= 1500 && hb.size.x <= SettlementScenery.SideStripWidth + 0.01f && hb.size.z <= 3.01f && anchorZ + hb.max.z <= SettlementScenery.FrontLine + 0.01f && Mathf.Abs(hb.min.y) <= 0.05f,
                        $"{look} fits its strip ({hb.size.x:0.00} m wide, {hb.size.z:0.00} m deep, {anchorZ + hb.max.z:0.00} m front reach, {triangles} triangles)");
                    Check(pet != null && Mathf.Abs(pet.position.y) <= 0.05f && row != null && row.Triangles == triangles, look + " has a grounded ActivePetAnchor and matches its manifest row");
                }
                Check(SettlementBackdrop.PalisadeZ >= SettlementScenery.HouseSetback + SettlementScenery.EnvelopeDepth + SettlementStreetProps.YardStart + 6f + 1f, "The palisade leaves room behind a 19 m level-16 house, set back behind its front garden, for its yard");
                Check(street.Buildings.All(b => Mathf.Abs(b.Root.localPosition.z - b.FrontZ) < 0.001f && Mathf.Abs(b.FrontZ - (b.Kind == StreetBuildingKind.House ? SettlementScenery.HouseSetback : 0f)) < 0.001f),
                    $"Houses stand {SettlementScenery.HouseSetback:0} m back behind front gardens; the storefront row stays on the street line");
                var sortedRow = street.Buildings.OrderBy(b => b.CentreX).ToList(); bool separated = true;
                for (int i = 0; i + 1 < sortedRow.Count; i++) separated &= Separated(sortedRow[i], sortedRow[i + 1]);
                Check(separated, "Neighbouring buildings never touch: at least 1 m between fronts and 0.4 m between rear wings (no overlapping tap boxes)");

                bool housesOk = true;
                for (int plot = 0; plot < 6; plot++) housesOk &= Find(street, "plot." + plot).Root.childCount == 1 && Find(street, "plot." + plot).Root.GetChild(0).name == $"HouseL0{levels[plot]}Front";
                Check(housesOk, "Each house shows the front for its level");
                bool frontsOk = true;
                for (int i = 0; i < 7; i++) frontsOk &= HasChild(Find(street, fronts[i]).Root, manifest.Shops[i].Front);
                Check(frontsOk, "Each storefront instantiates its manifest front");

                Check(root.GetComponentsInChildren<TheTrailWalker.Harvest.Tier1HarvestVisual>(true).Length == 0, "The street has no harvest interactables");
                bool boxes = street.Buildings.All(b => b.Root != null && b.Root.GetComponent<BoxCollider>() != null && b.Root.GetComponent<BoxCollider>().size.x > 1f);
                Check(boxes && root.GetComponentsInChildren<Collider>(true).Length == street.Buildings.Count, "Every building root has one selection BoxCollider and nothing else collides, so there are no pickups");

                Transform lumberRoot = Find(street, "lumber").Root;
                Check(HasChild(lumberRoot, "LumberFront") && CountChildren(lumberRoot, "FenceSection") == 0 && CountChildren(lumberRoot, "MassivePine") == 0, "The lumber frontage uses LumberFront when the art exists");

                // Backdrop (proposal 2026-11-09, phase A): palisade, arch and gate, the slope and mountains; nothing on the road or
                // tall in front of the doors within the scroll range; deterministic.
                SettlementBackdrop.Result backdrop = street.Backdrop;
                Check(backdrop != null && backdrop.PalisadeMinX < street.MinX && backdrop.PalisadeMaxX > street.MaxX && backdrop.PalisadeSegments * 4f >= backdrop.PalisadeMaxX - backdrop.PalisadeMinX - 4f
                    && backdrop.EndWallSegments >= 4, "The palisade spans the whole street behind the yards and turns forward at both ends");
                Check(backdrop.EntranceArch != null && backdrop.EntranceArch.position.x < street.MinX && backdrop.DepartureGate != null && backdrop.DepartureGate.position.x > street.MaxX,
                    "The entrance arch closes the left end and the departure gate the right end");
                bool roadClear = true, vergeLow = true, mountains = false, slope = false; int vertices = 0;
                foreach (MeshFilter filter in backdrop.Root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null) continue;
                    bool ground = filter.name == "Street meadow";
                    Matrix4x4 m = filter.transform.localToWorldMatrix;
                    foreach (Vector3 local in filter.sharedMesh.vertices)
                    {
                        Vector3 v = m.MultiplyPoint3x4(local); vertices++;
                        if (v.z > 180f && v.y > 30f) mountains = true;
                        if (ground && v.z > 100f && v.y > 12f) slope = true;
                        if (ground || v.x < street.MinX - 5f || v.x > street.MaxX + 5f) continue;
                        if (v.z > SettlementBackdrop.RoadNear + 0.05f && v.z < SettlementBackdrop.RoadFar - 0.05f) roadClear = false;
                        if (v.z <= SettlementBackdrop.RoadNear && v.y > SettlementBackdrop.VergeMaxHeight + 0.05f) vergeLow = false;
                    }
                }
                Check(roadClear, "No backdrop piece stands on the road within the scroll range");
                Check(vergeLow, "Verge pieces between the camera and the road stay below 1 m, so no front door is hidden");
                Check(slope && mountains, "The ground rises behind the palisade and region 5's mountains stand on the horizon");
                var again = new GameObject("Street check root (again)");
                try
                {
                    SettlementStreet twin = SettlementScenery.Build(again.transform, levels, 4);
                    int twinVertices = twin.Backdrop.Root.GetComponentsInChildren<MeshFilter>(true).Sum(f => f.sharedMesh != null ? f.sharedMesh.vertexCount : 0);
                    Check(twin.Backdrop.Pieces == backdrop.Pieces && twinVertices == vertices, "The backdrop is the same on every build (fixed seed)");
                Check(!backdrop.CombinedUnreadable && backdrop.Instanced >= 3, "Backdrop pieces without Read/Write meshes (massive pines, log seats) are instanced, never combined, so they render on the device");
                }
                finally { UnityEngine.Object.DestroyImmediate(again); }
                Check(RegionLooks.Settlement.Fog && RegionLooks.Settlement.FogEnd > 300f, "The settlement look adds a far haze so the mountains sit back");

                var cameraObject = new GameObject("Street camera check", typeof(Camera));
                try
                {
                    var cam = cameraObject.AddComponent<StreetCamera>(); cam.Bind(street);
                    cam.CentreOn(street.MaxX + 50f);
                    bool high = Mathf.Approximately(cam.X, street.MaxX);
                    cam.CentreOn(street.MinX - 50f);
                    bool low = Mathf.Approximately(cam.X, street.MinX);
                    cam.CentreOn(0f); cam.BeginDrag(); cam.DragBy(2.5f); cam.CancelDrag();
                    Check(high && low && Mathf.Approximately(cam.X, 2.5f) && street.MinX <= Find(street, "plot.0").CentreX && street.MaxX >= Find(street, "plot.5").CentreX, "The street camera clamps to the street and scrolls by metres");
                }
                finally { UnityEngine.Object.DestroyImmediate(cameraObject); }
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }

            // Placeholder branch (8 m).
            root = new GameObject("Street check root");
            try
            {
                SettlementStreet holder = SettlementScenery.Build(root.transform, new[] { 1, 1, 1, 1, 1, 1 }, -1, null, true);
                Transform lumber = Find(holder, "lumber").Root;
                Check(CountChildren(lumber, "FenceSection") == 2 && CountChildren(lumber, "MassivePine") == 3 && !HasChild(lumber, "LumberFront"), "The placeholder lumber frontage is two fence sections and three massive pines");
                Check(Mathf.Abs(Find(holder, "lumber").CentreX - Find(holder, "shop.tailor").CentreX - ((SettlementScenery.StorefrontSlots[6] + SettlementScenery.SlotWidth(7, 8f)) / 2f + SettlementScenery.SlotGap)) < 0.001f
                    && lumber.GetComponent<BoxCollider>() != null, "The 8 m placeholder sits in the lumber slot and is selectable");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }

            // Live branch with a fake front width.
            root = new GameObject("Street check root");
            try
            {
                // A front wider than its slot widens the slot (art outgrowing its envelope never overlaps a neighbour).
                SettlementStreet fake = SettlementScenery.Build(root.transform, new[] { 1, 1, 1, 1, 1, 1 }, -1, 12f, false);
                Check(HasChild(Find(fake, "lumber").Root, "LumberFront") && Mathf.Abs(Find(fake, "lumber").CentreX - Find(fake, "shop.tailor").CentreX - ((SettlementScenery.StorefrontSlots[6] + 12f) / 2f + SettlementScenery.SlotGap)) < 0.001f
                    && Mathf.Abs(Find(fake, "plot.3").CentreX - Find(fake, "lumber").CentreX - (12f / 2f + 1.4f + SettlementScenery.PlotWidth / 2f)) < 0.001f, "A lumber front wider than its slot widens the slot and moves the plots beyond it");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }

            // Missing art: the street still builds all 14 buildings with colliders, with placeholder blocks (no throw).
            root = new GameObject("Street check root");
            try
            {
                SettlementStreet bare = SettlementScenery.Build(root.transform, new[] { 1, 2, 3, 4, 5, 6 }, 2, null, false, path => null);
                var allIds = new[] { "plot.0", "plot.1", "plot.2", "plot.3", "plot.4", "plot.5", "warehouse", "shop.bakery", "shop.ranch", "shop.farm", "shop.fishery", "shop.mine", "shop.tailor", "lumber" };
                Check(bare.Buildings.Count == 14 && allIds.All(id => bare.Buildings.Any(b => b.Id == id)) && bare.Buildings.All(b => b.Root.GetComponent<BoxCollider>() != null && b.Root.childCount >= 1),
                    "With no art loaded the street still builds 14 buildings, each with a block and a selection collider");
                Check(bare.Buildings.Count(b => b.Kind == StreetBuildingKind.House) == 6 && Find(bare, "lumber").Kind == StreetBuildingKind.LumberField && bare.MinX < bare.MaxX, "Placeholder blocks keep kinds and the street range");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            root = new GameObject("Street check root");
            try
            {
                // Only levels 1-3 exist: a level 5 house shows level 3, and level 2 shows itself.
                SettlementStreet partial = SettlementScenery.Build(root.transform, new[] { 5, 2, 1, 8, 3, 4 }, -1, null, false,
                    path => path.StartsWith("Settlement/Houses/HouseL0") && (path.EndsWith("L01Front") || path.EndsWith("L02Front") || path.EndsWith("L03Front")) ? Resources.Load<GameObject>(path) : null);
                Check(Find(partial, "plot.0").Root.GetChild(0).name == "HouseL03Front" && Find(partial, "plot.1").Root.GetChild(0).name == "HouseL02Front" && Find(partial, "plot.3").Root.GetChild(0).name == "HouseL03Front"
                    && Find(partial, "plot.5").Root.GetChild(0).name == "HouseL03Front", "A house level without art falls back to the highest lower level that exists");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }

            // Scene info from the save.
            var unchosen = SettlementState.New(3); unchosen.Discover();
            Check(SettlementScenery.HouseLevels(unchosen).SequenceEqual(new[] { 1, 1, 1, 1, 1, 1 }) && SettlementScenery.PlayerPlot(unchosen) == -1, "Before the plot choice all six houses show level 1 and there is no player plot");
            var picked = SettlementState.New(9); picked.Discover(); picked.ChoosePlot(2, new[] { "region.01" });
            Check(SettlementScenery.HouseLevels(picked).Length == 6 && SettlementScenery.HouseLevels(picked)[2] == 1 && SettlementScenery.PlayerPlot(picked) == 2
                && picked.Neighbours.All(n => SettlementScenery.HouseLevels(picked)[n.Plot] == n.Level), "House levels follow the neighbours and the player's plot is level 1");
        }

        // ---- Street and yard props (backdrop phase B, handoff settlement-street-props-handoff-2026-11-09.md) ----
        // Placement runs on synthetic boxes at the handoff's sizes, so it is checked before the art lands; the delivered prefabs
        // are checked against the handoff's conventions as they arrive (a missing one is reported as WAITING, not a failure).
        private static readonly Dictionary<string, Vector3> GuideSizes = new Dictionary<string, Vector3>
        {
            { "StreetLampPost", new Vector3(0.4f, 3.2f, 0.4f) }, { "VillageWell", new Vector3(2.2f, 2.6f, 2.2f) }, { "NoticeBoard", new Vector3(1.8f, 2.2f, 0.5f) },
            { "StreetBench", new Vector3(1.6f, 0.9f, 0.5f) }, { "BarrelCrateCluster", new Vector3(1.4f, 1.2f, 1.4f) }, { "GardenBed", new Vector3(2.4f, 0.5f, 1.2f) },
            { "YardFenceCorner", new Vector3(3f, 1f, 2f) }, { "Woodpile", new Vector3(1.8f, 1.6f, 1f) }, { "GardenShed", new Vector3(2f, 2.4f, 2f) },
            { "HayTrough", new Vector3(2.4f, 1.4f, 1.4f) }, { "CropRows", new Vector3(3f, 0.6f, 2f) }, { "DryingRack", new Vector3(2.2f, 1.8f, 0.8f) },
            { "OreCart", new Vector3(2.2f, 1.6f, 1.6f) }, { "WashingLine", new Vector3(3f, 2f, 0.3f) },
        };
        [Serializable] private sealed class HouseRow { public int Level; public string Front; public float Width; public float[] PetHomeAnchor; public float[] CollectionAnchor; }
        [Serializable] private sealed class PetHomeRow { public string Name; public int Triangles; }
        [Serializable] private sealed class HouseManifestRows { public HouseRow[] Houses; public PetHomeRow[] PetHomes; }
        [Serializable] private sealed class PropRow { public string Name; public float Height; public int Triangles; public float[] Footprint; }
        [Serializable] private sealed class PropManifest { public PropRow[] Props; }

        private static void PropChecks()
        {
            // 1. Delivered art against the handoff's conventions.
            var materials = new Dictionary<string, HashSet<Material>> { { "street", new HashSet<Material>() }, { "yards", new HashSet<Material>() } };
            int delivered = 0;
            foreach (string prop in SettlementStreetProps.StreetProps.Concat(SettlementStreetProps.YardProps))
            {
                GameObject prefab = Resources.Load<GameObject>(SettlementStreetProps.PathOf(prop));
                if (prefab == null) { passed.Add("WAITING (not delivered yet): " + SettlementStreetProps.PathOf(prop)); continue; }
                delivered++;
                var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
                Check(filters.Length > 0 && filters.All(f => f.sharedMesh != null), prop + " has meshes");
                Check(prefab.GetComponentsInChildren<MonoBehaviour>(true).Length == 0 && prefab.GetComponentsInChildren<Collider>(true).Length == 0 && prefab.GetComponentsInChildren<Rigidbody>(true).Length == 0,
                    prop + " is passive (no scripts, colliders or rigidbodies)");
                Check(filters.All(f => f.sharedMesh.isReadable), prop + " meshes have Read/Write enabled (combined meshes render nothing on the device otherwise)");
                int triangles = filters.Sum(f => f.sharedMesh.triangles.Length / 3);
                Check(triangles <= SettlementStreetProps.TriangleBudget[prop], $"{prop} is within its triangle budget ({triangles} of {SettlementStreetProps.TriangleBudget[prop]})");
                Bounds b = PrefabBounds(prefab);
                Check(Mathf.Abs(b.min.y) <= 0.05f && Mathf.Abs(b.center.x) <= 0.25f * b.size.x + 0.05f && Mathf.Abs(b.center.z) <= 0.25f * b.size.z + 0.05f,
                    $"{prop} pivots at ground centre (bounds min y {b.min.y:0.00}, centre x {b.center.x:0.00} z {b.center.z:0.00})");
                bool yard = Array.IndexOf(SettlementStreetProps.YardProps, prop) >= 0;
                if (yard) Check(b.max.y <= SettlementStreetProps.YardMaxHeight + 0.05f, $"{prop} stays at or under 2.4 m ({b.max.y:0.00} m)");
                else if (prop == "StreetLampPost") Check(b.max.y > 2.6f && b.max.y < 3.8f, $"The lamp post is about 3.2 m tall ({b.max.y:0.00} m)");
                foreach (MeshRenderer r in prefab.GetComponentsInChildren<MeshRenderer>(true)) materials[yard ? "yards" : "street"].UnionWith(r.sharedMaterials);
            }
            if (materials["street"].Count > 0) Check(materials["street"].Count == 1, "The street props share one palette material");
            if (materials["yards"].Count > 0) Check(materials["yards"].Count == 1, "The yard kit shares one palette material");
            string manifestPath = Directory.Exists("Assets/_Game/Art/Settlement") ? Directory.GetFiles("Assets/_Game/Art/Settlement", "SettlementPropsManifest.json", SearchOption.AllDirectories).FirstOrDefault() : null;
            if (manifestPath == null) passed.Add("WAITING (not delivered yet): SettlementPropsManifest.json");
            else
            {
                PropManifest manifest = JsonUtility.FromJson<PropManifest>(File.ReadAllText(manifestPath));
                if (manifest?.Props == null || manifest.Props.Length == 0) passed.Add("NOTE: " + manifestPath + " has no \"Props\" rows this check can read (expected Name, Footprint [x, z], Height, Triangles)");
                else foreach (PropRow row in manifest.Props)
                {
                    GameObject prefab = Resources.Load<GameObject>(SettlementStreetProps.PathOf(row.Name));
                    if (prefab == null) continue;
                    int triangles = prefab.GetComponentsInChildren<MeshFilter>(true).Sum(f => f.sharedMesh != null ? f.sharedMesh.triangles.Length / 3 : 0);
                    Check(triangles == row.Triangles && Mathf.Abs(PrefabBounds(prefab).max.y - row.Height) <= 0.06f, $"The props manifest matches {row.Name} (triangles and height)");
                }
            }

            // 2. Placement on synthetic props at the handoff sizes, at the narrowest and widest houses and a mix.
            var synthetic = new Dictionary<string, GameObject>();
            Material shared = Region01Environment.GroundMaterial;
            var holder = new GameObject("Synthetic props"); holder.SetActive(false);
            var meshes = new List<Mesh>();
            try
            {
                foreach (var pair in GuideSizes)
                {
                    var go = new GameObject(pair.Key, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(holder.transform, false);
                    Mesh mesh = GroundBox(pair.Value); meshes.Add(mesh);
                    go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = shared;
                    synthetic[SettlementStreetProps.PathOf(pair.Key)] = go;
                }
                // Synthetic boxes always; the delivered prefabs too once all 14 exist (their real footprints, e.g. the lamp arm).
                bool useReal = false;
                GameObject Load(string path) => !useReal && synthetic.TryGetValue(path, out GameObject go) ? go : Resources.Load<GameObject>(path);

                foreach (bool real in delivered == 14 ? new[] { false, true } : new[] { false })
                foreach (int[] levels in new[] { new[] { 1, 1, 1, 1, 1, 1 }, new[] { 1, 2, 3, 4, 5, 6 }, new[] { 8, 8, 8, 8, 8, 8 } })
                {
                    useReal = real;
                    string tag = $"({(real ? "delivered art" : "stand-in boxes")}, house levels {string.Join("", levels)})";
                    var root = new GameObject("Props check root");
                    try
                    {
                        SettlementStreet street = SettlementScenery.Build(root.transform, levels, 4, null, false, Load);
                        SettlementStreetProps.Result props = street.Props;
                        var sorted = street.Buildings.OrderBy(b => b.CentreX).ToList();
                        if (levels[0] == 8)
                            for (int i = 0; i + 1 < sorted.Count; i++)
                                passed.Add($"INFO gap {sorted[i].Id} -> {sorted[i + 1].Id}: {SettlementStreetProps.Extent(sorted[i + 1]).Left - SettlementStreetProps.Extent(sorted[i]).Right:0.00} m at house level 8");
                        int gaps = sorted.Count - 1;
                        Check(props.Missing.Count == 0 && props.YardKit, "Synthetic props: every prop loads " + tag);
                        Check(props.Pieces.Count(p => p.Prop == "StreetLampPost") == gaps - 2, $"A lamp post stands in each of the {gaps - 2} ordinary gaps {tag}");
                        Check(props.Pieces.Count(p => p.Prop == "VillageWell") == 1 && props.Pieces.Single(p => p.Prop == "VillageWell").Building == "plot.2|warehouse", "The village well stands in the gap before the Warehouse " + tag);
                        Check(props.Pieces.Count(p => p.Prop == "NoticeBoard") == 1 && props.Pieces.Single(p => p.Prop == "NoticeBoard").Building == "lumber|plot.3", "The notice board stands in the gap after the lumber frontage " + tag);
                        Check(props.Pieces.Count(p => p.Prop == "StreetBench") + props.Pieces.Count(p => p.Prop == "BarrelCrateCluster" && p.Building.Contains("|")) == 4, "Each of the four house-to-house gaps has a bench or a barrel cluster " + tag);

                        bool clearOfBuildings = true, offRoad = true, setBack = true;
                        foreach (SettlementStreetProps.Placed piece in props.Pieces)
                        {
                            foreach (StreetBuilding building in street.Buildings)
                            {
                                if (OverlapsBuilding(piece.Bounds, building)) clearOfBuildings = false;
                            }
                            if (piece.Prop == "StreetLampPost") { if (piece.Bounds.center.z <= SettlementBackdrop.RoadFar || piece.Bounds.center.z >= 0f) offRoad = false; }
                            else if (piece.Bounds.min.z < SettlementBackdrop.RoadFar) offRoad = false;
                            if (piece.Building.Contains("|") && piece.Prop != "StreetLampPost" && piece.Bounds.min.z < SettlementStreetProps.SetBack - 0.02f) setBack = false;
                        }
                        Check(clearOfBuildings, "No prop overlaps a building's footprint or tap box " + tag);
                        Check(offRoad, "Lamp posts stand between the road and the front line and nothing else reaches the road " + tag);
                        Check(setBack, "The well, the board, benches and clusters stand behind the front line " + tag);

                        // Front gardens: a fence on each side of the gate and at least one bed each side of the path, all low,
                        // inside the house's front band, off the road and short of the porch, and the gate and path left clear.
                        bool gardens = true; string gardenReport = "";
                        foreach (StreetBuilding house in street.Buildings.Where(b => b.Kind == StreetBuildingKind.House))
                        {
                            var own = props.Pieces.Where(p => p.Building == SettlementStreetProps.GardenLabel(house.Id)).ToList();
                            bool ok = own.Count(p => p.Prop == SettlementStreetProps.GardenFence && p.Bounds.center.x < house.CentreX) >= 1 && own.Count(p => p.Prop == SettlementStreetProps.GardenFence && p.Bounds.center.x > house.CentreX) >= 1
                                && own.Count(p => p.Prop == "GardenBed" && p.Bounds.center.x < house.CentreX) >= 1 && own.Count(p => p.Prop == "GardenBed" && p.Bounds.center.x > house.CentreX) >= 1;
                            foreach (var p in own)
                                ok &= p.Bounds.max.y <= SettlementScenery.GardenMaxHeight + 0.01f && p.Bounds.min.x >= house.FrontLeft - 0.05f && p.Bounds.max.x <= house.FrontRight + 0.05f
                                    && p.Bounds.min.z >= SettlementBackdrop.RoadFar && p.Bounds.max.z <= house.FrontZ - SettlementScenery.FrontLine
                                    && (p.Bounds.max.x <= house.CentreX - SettlementStreetProps.GateWidth / 2f + 0.05f || p.Bounds.min.x >= house.CentreX + SettlementStreetProps.GateWidth / 2f - 0.05f);
                            gardens &= ok; if (!ok) gardenReport += " " + house.Id;
                        }
                        Check(gardens, "Every house has a front garden: fence either side of the gate, beds either side of the path, all under 1 m, inside the front band, off the road and short of the porch " + tag + gardenReport);

                        bool yards = true, low = true, inside = true, apart = true;
                        foreach (StreetBuilding building in street.Buildings)
                        {
                            var own = props.Pieces.Where(p => p.Building == building.Id).ToList();
                            string[] expected = building.Kind == StreetBuildingKind.House ? new[] { "GardenBed", "YardFenceCorner", building.Plot % 2 == 0 ? "Woodpile" : "GardenShed" }
                                : SettlementStreetProps.ShopYards.TryGetValue(building.Id, out string themed) ? new[] { themed } : new string[0];
                            yards &= own.Select(p => p.Prop).OrderBy(n => n).SequenceEqual(expected.OrderBy(n => n));
                            float back = SettlementStreetProps.Extent(building).Back;
                            foreach (var p in own)
                            {
                                low &= p.Bounds.max.y <= SettlementStreetProps.YardMaxHeight + 0.01f;
                                inside &= p.Bounds.min.z >= back && p.Bounds.max.z <= SettlementBackdrop.PalisadeZ - 0.5f;
                                foreach (var q in own) if (p != q && p.Bounds.max.x > q.Bounds.min.x && p.Bounds.min.x < q.Bounds.max.x && p.Bounds.max.z > q.Bounds.min.z && p.Bounds.min.z < q.Bounds.max.z) apart = false;
                            }
                        }
                        Check(yards, "Each house has a garden bed, a fence corner and a woodpile or shed, and each shop its themed yard " + tag);
                        Check(low && inside && apart, "Yard props stay under 2.4 m, between the building's back and the palisade, without overlapping " + tag);
                        Check(!props.CombinedUnreadable && props.Instanced == 0 && props.Root.GetComponentsInChildren<MeshRenderer>().Length >= 1, "Readable props are combined into chunks " + tag);
                        Check(street.Backdrop.Instanced == 3 && !street.Backdrop.Root.GetComponentsInChildren<Transform>(true).Any(t => t.name.StartsWith("PineLogSeat")),
                            "With the yard kit the backdrop drops its log-seat and fence stand-ins (only the three massive pines are instanced) " + tag);
                        Check(root.GetComponentsInChildren<Collider>(true).Length == street.Buildings.Count, "Props add no colliders " + tag);

                        var again = new GameObject("Props check root (again)");
                        try
                        {
                            SettlementStreetProps.Result twin = SettlementScenery.Build(again.transform, levels, 4, null, false, Load).Props;
                            Check(twin.Pieces.Count == props.Pieces.Count && twin.Pieces.Zip(props.Pieces, (x, y) => x.Prop == y.Prop && (x.Bounds.center - y.Bounds.center).sqrMagnitude < 1e-6f).All(s => s),
                                "Props are placed the same on every build " + tag);
                        }
                        finally { UnityEngine.Object.DestroyImmediate(again); }
                    }
                    finally { UnityEngine.Object.DestroyImmediate(root); }
                }

                // Before the art lands: no props and the phase A stand-ins stay.
                var bareRoot = new GameObject("Props check root (none)");
                try
                {
                    SettlementStreet bare = SettlementScenery.Build(bareRoot.transform, new[] { 1, 1, 1, 1, 1, 1 }, 4, null, false,
                        path => path.StartsWith(SettlementStreetProps.StreetFolder) || path.StartsWith(SettlementStreetProps.YardFolder) ? null : Resources.Load<GameObject>(path));
                    Check(bare.Props.Pieces.Count == 0 && bare.Props.Missing.Count == 14 && !bare.Props.YardKit && bare.Backdrop.Instanced == 9,
                        "Without the prop art nothing is placed and the phase A stand-ins (six log seats) stay");
                }
                finally { UnityEngine.Object.DestroyImmediate(bareRoot); }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
                foreach (Mesh mesh in meshes) UnityEngine.Object.DestroyImmediate(mesh);
            }
            passed.Add($"INFO street props delivered: {delivered} of 14");
        }

        // Two neighbours: fronts (the band up to WingFromZ) at least 1 m apart, anything behind (rear wings) at least 0.4 m.
        private static bool Separated(StreetBuilding a, StreetBuilding b)
            => b.FrontLeft - a.FrontRight >= 0.99f && SettlementStreetProps.Extent(b).Left - SettlementStreetProps.Extent(a).Right >= 0.39f;

        // A prop footprint overlaps a building: its front band (front line to WingFromZ) or, behind that, its full width to the back.
        private static bool OverlapsBuilding(Bounds piece, StreetBuilding building)
        {
            var (l, r, back) = SettlementStreetProps.Extent(building);
            float z0 = building.FrontZ, wing = building.FrontZ + SettlementScenery.WingFromZ;
            bool front = piece.max.x > building.FrontLeft + 0.01f && piece.min.x < building.FrontRight - 0.01f && piece.max.z > z0 + 0.01f && piece.min.z < wing;
            bool rear = piece.max.x > l + 0.01f && piece.min.x < r - 0.01f && piece.max.z > wing && piece.min.z < back - 0.01f;
            return front || rear;
        }

        // ---- House exterior redesign, stage 2 blockouts (handoff house-exterior-redesign-handoff-2026-11-10.md) ----
        // Review-only prefabs: each fits the envelope, and the live street holds them (mixed levels, and level 16 on every plot)
        // with the street dressing and yards still placed and clear.
        private static void BlockoutChecks()
        {
            var blocks = new Dictionary<int, GameObject>();
            for (int level = 1; level <= 16; level++)
            {
                GameObject block = Resources.Load<GameObject>($"Settlement/Houses/Blockouts/HouseL{level:00}Block");
                if (block == null) { passed.Add($"WAITING (not delivered yet): Settlement/Houses/Blockouts/HouseL{level:00}Block"); continue; }
                blocks[level] = block;
                Bounds b = PrefabBounds(block); float frontWidth = 0f;
                foreach (MeshFilter f in block.GetComponentsInChildren<MeshFilter>(true))
                {
                    Matrix4x4 m = block.transform.worldToLocalMatrix * f.transform.localToWorldMatrix;
                    foreach (Vector3 v in f.sharedMesh.vertices) { Vector3 p = m.MultiplyPoint3x4(v); if (p.z >= -SettlementScenery.WingFromZ) frontWidth = Mathf.Max(frontWidth, 2f * Mathf.Abs(p.x)); }
                }
                int triangles = block.GetComponentsInChildren<MeshFilter>(true).Sum(f => f.sharedMesh.triangles.Length / 3);
                Check(frontWidth <= SettlementScenery.HouseEnvelopeWidth + 0.01f && b.size.x <= SettlementScenery.WingWidth + 0.01f && b.max.y <= SettlementScenery.MaxHouseHeight + 0.01f
                    && -b.min.z <= SettlementScenery.EnvelopeDepth + 0.01f && b.max.z <= SettlementScenery.FrontLine + 0.01f && triangles <= (level <= 8 ? 8000 : 12000)
                    && block.GetComponentsInChildren<Collider>(true).Length == 0 && block.GetComponentsInChildren<MonoBehaviour>(true).Length == 0,
                    $"Blockout L{level} fits the envelope (front {frontWidth:0.0} m, overall {b.size.x:0.0} m, height {b.max.y:0.0} m, depth {-b.min.z:0.0} m, reach {b.max.z:0.00} m)");
                float top = TopShare(block);
                Check(top <= 1f - 2f * SettlementScenery.TopMarginShare, $"Blockout L{level} shows its full height on the street screen (top at {(1f - top) * 50f:0.0}% below the screen top; at least {SettlementScenery.TopMarginShare * 100f:0.0}%, below the title line)");
            }
            foreach (var pair in blocks.OrderBy(k => k.Key))
                if (!WingRule(pair.Value, out string wings)) passed.Add($"PENDING (art fix requested, low lean-to wings): blockout L{pair.Key} {wings}");
            if (blocks.Count < 16) return;
            var heights = blocks.OrderBy(k => k.Key).Select(k => PrefabBounds(k.Value).max.y).ToList();
            Check(heights.Zip(heights.Skip(1), (x, y) => y >= x - 0.01f).All(ok => ok), "Blockout heights never drop from one level to the next");
            // Blockout revision 1 (handoff blockout-revision-handoff-2026-11-10.md): level 10 must stand clear of level 9.
            if (heights[9] - heights[8] < 0.39f) passed.Add($"PENDING (art fix requested, level 10 above level 9): L9 {heights[8]:0.00} m, L10 {heights[9]:0.00} m");
            else passed.Add($"Blockout L10 stands {heights[9] - heights[8]:0.00} m above L9");

            // The street with blockouts: plots show the given levels by mapping the six house paths onto blockout levels.
            foreach (int[] shown in new[] { new[] { 1, 16, 6, 12, 9, 4 }, new[] { 16, 16, 16, 16, 16, 16 }, new[] { 15, 16, 14, 13, 15, 16 } })
            {
                string tag = $"(blockouts {string.Join(",", shown)})";
                GameObject Load(string path)
                {
                    for (int i = 0; i < 6; i++) if (path.EndsWith($"/HouseL{i + 1:00}Front")) return blocks[shown[i]];
                    return Resources.Load<GameObject>(path);
                }
                var root = new GameObject("Blockout street check");
                try
                {
                    SettlementStreet street = SettlementScenery.Build(root.transform, new[] { 1, 2, 3, 4, 5, 6 }, 4, null, false, Load);
                    var sorted = street.Buildings.OrderBy(b => b.CentreX).ToList();
                    bool apart = true; for (int i = 0; i + 1 < sorted.Count; i++) apart &= Separated(sorted[i], sorted[i + 1]);
                    Check(apart, "Blockout houses keep 1 m between fronts and 0.4 m between wings " + tag);
                    SettlementStreetProps.Result props = street.Props;
                    Check(props.Pieces.Count(p => p.Prop == "StreetLampPost") == sorted.Count - 3 && props.Pieces.Count(p => p.Prop == "VillageWell") == 1 && props.Pieces.Count(p => p.Prop == "NoticeBoard") == 1
                        && props.Pieces.Count(p => (p.Prop == "StreetBench" || p.Prop == "BarrelCrateCluster") && p.Building.Contains("|")) == 4, "Every gap keeps its lamp, well, board and bench or cluster beside blockout houses " + tag);
                    Check(props.Pieces.All(p => street.Buildings.All(bd => !OverlapsBuilding(p.Bounds, bd))) && props.Pieces.Where(p => !p.Building.Contains("|")).All(p => p.Bounds.max.z <= SettlementBackdrop.PalisadeZ - 0.5f),
                        "No prop overlaps a blockout house and every yard stays inside the palisade " + tag);
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
        }

        // Wing rule (user decision, November 10, 2026: low lean-to wings): any geometry wider than the 11 m front stays at or under
        // 5.5 m, starts at least 4 m behind the front face, and is at least 1.2 m wide beyond the front (no slivers).
        private static bool WingRule(GameObject house, out string report)
        {
            float half = SettlementScenery.HouseEnvelopeWidth / 2f, top = 0f, setback = float.MaxValue, beyond = 0f; bool any = false;
            foreach (MeshFilter f in house.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.sharedMesh == null || !f.sharedMesh.isReadable) continue;
                Matrix4x4 m = house.transform.worldToLocalMatrix * f.transform.localToWorldMatrix;
                foreach (Vector3 v in f.sharedMesh.vertices)
                {
                    Vector3 p = m.MultiplyPoint3x4(v);
                    if (Mathf.Abs(p.x) <= half + 0.05f) continue;
                    any = true; top = Mathf.Max(top, p.y); setback = Mathf.Min(setback, -p.z); beyond = Mathf.Max(beyond, Mathf.Abs(p.x) - half);
                }
            }
            report = any ? $"wing {beyond:0.0} m beyond the front, top {top:0.0} m, from {setback:0.0} m behind the front" : "no wings";
            return !any || (top <= 5.51f && setback >= 3.99f && beyond >= 1.19f);
        }

        // Highest screen position (-1 bottom .. 1 top) of a house prefab standing on a plot, with the street camera centred on
        // it. Prefab +Z is the front; on the street it turns to face the camera, so local z maps to HouseSetback - z.
        private static float TopShare(GameObject house)
        {
            float top = -1f;
            foreach (MeshFilter f in house.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.sharedMesh == null || !f.sharedMesh.isReadable) continue;
                Matrix4x4 m = house.transform.worldToLocalMatrix * f.transform.localToWorldMatrix;
                foreach (Vector3 v in f.sharedMesh.vertices) { Vector3 p = m.MultiplyPoint3x4(v); top = Mathf.Max(top, SettlementScenery.ScreenHeightShare(p.y, SettlementScenery.HouseSetback - p.z)); }
            }
            return top;
        }

        private static Bounds PrefabBounds(GameObject prefab)
        {
            bool any = false; var bounds = new Bounds();
            Matrix4x4 toRoot = prefab.transform.worldToLocalMatrix;
            foreach (MeshFilter f in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.sharedMesh == null) continue;
                Matrix4x4 m = toRoot * f.transform.localToWorldMatrix;
                foreach (Vector3 v in f.sharedMesh.vertices) { Vector3 p = m.MultiplyPoint3x4(v); if (!any) { bounds = new Bounds(p, Vector3.zero); any = true; } else bounds.Encapsulate(p); }
            }
            return bounds;
        }

        // A box with its pivot at ground centre (readable, like the delivered props must be).
        private static Mesh GroundBox(Vector3 size)
        {
            float x = size.x / 2f, z = size.z / 2f, y = size.y;
            var mesh = new Mesh { name = "Synthetic prop" };
            mesh.vertices = new[] { new Vector3(-x, 0, -z), new Vector3(x, 0, -z), new Vector3(x, y, -z), new Vector3(-x, y, -z), new Vector3(-x, 0, z), new Vector3(x, 0, z), new Vector3(x, y, z), new Vector3(-x, y, z) };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7, 0, 7, 3, 0, 4, 7, 1, 2, 6, 1, 6, 5, 3, 7, 6, 3, 6, 2, 0, 1, 5, 0, 5, 4 };
            mesh.uv = new Vector2[8]; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        // ---- Settlement HUD, list, notes, plot choice and Return (Task 5) ----
        private static void HudChecks()
        {
            StreetBuilding Shop(string id) => new StreetBuilding { Id = id, Kind = StreetBuildingKind.Shop };
            Check(CollectionPrototype.SettlementNote(Shop("shop.bakery"), 1, false) == "Bakery opens in Milestone 14", "The Bakery note reads \"Bakery opens in Milestone 14\"");
            var shops = new[] { ("shop.bakery", "Bakery"), ("shop.ranch", "Ranch"), ("shop.farm", "Farm"), ("shop.fishery", "Fishery"), ("shop.mine", "Mine"), ("shop.tailor", "Tailor") };
            Check(shops.All(s => CollectionPrototype.SettlementNote(Shop(s.Item1), 1, false) == s.Item2 + " opens in Milestone 14"), "Every shop note names its shop and Milestone 14");
            Check(CollectionPrototype.SettlementNote(new StreetBuilding { Id = "warehouse", Kind = StreetBuildingKind.Warehouse }, 1, false) == "The warehouse", "The warehouse note no longer points at Milestone 12");
            Check(CollectionPrototype.OpensLumberCounter(null, StreetBuildingKind.LumberField) && CollectionPrototype.OpensLumberCounter("lumber", StreetBuildingKind.House) && !CollectionPrototype.OpensLumberCounter("warehouse", StreetBuildingKind.Warehouse), "Tapping the lumber building or the Lumber list entry opens the counter (OpenLumberCounter)");
            var house = new StreetBuilding { Id = "plot.2", Kind = StreetBuildingKind.House, Plot = 2 };
            Check(CollectionPrototype.SettlementNote(house, 3, false) == "A neighbour's home (level 3)", "A neighbour's house at level 3 reads \"A neighbour's home (level 3)\"");
            // The player's plot no longer shows a note: a tap opens the home (first home, task 5; HomeValidation.HudChecks).
            Check(CollectionPrototype.SettlementPlotPrompt == "Build your home here? This can't be changed." && CollectionPrototype.SettlementWelcome == "Welcome to the settlement."
                && CollectionPrototype.SettlementReturnPrompt("Bright Hollow") == "Return to Bright Hollow?", "The plot, welcome and Return texts are verbatim");

            Check(new[] { "home", "warehouse", "lumber", "return" }.All(key => Resources.Load<Sprite>("UI/Icons/" + key) != null), "The home, warehouse, lumber and return icons load as sprites from UI/Icons");
            Check(Resources.Load<Sprite>("UI/Icons/settlement") != null && CollectionPrototype.SettlementListRowY(0) < 0f && CollectionPrototype.SettlementListRowY(1) < CollectionPrototype.SettlementListRowY(0),
                "The settlement menu button has its icon, and the open list stacks downward below it");
            Check(!CollectionPrototype.TrailControlsVisible(true) && CollectionPrototype.TrailControlsVisible(false), "Trail-only controls are hidden in the settlement and shown on the trail");
            Check(CollectionPrototype.SettlementListKeys(false).SequenceEqual(new[] { "return" }) && CollectionPrototype.SettlementListKeys(true).SequenceEqual(new[] { "home", "warehouse", "lumber", "return" }),
                "The settlement list shows only Return before the plot choice, then Home, Warehouse, Lumber and Return");
            var allKeys = new[] { "home", "warehouse", "lumber", "return" };
            bool SlotsContiguous(bool chosen)
            {
                int[] slots = allKeys.Select(k => CollectionPrototype.SettlementListSlot(k, chosen)).Where(s => s >= 0).OrderBy(s => s).ToArray();
                return slots.Length == CollectionPrototype.SettlementListKeys(chosen).Length && slots.Select((s, i) => s == i).All(x => x);
            }
            Check(CollectionPrototype.SettlementListSlot("return", false) == 0 && CollectionPrototype.SettlementListSlot("home", false) == -1 && SlotsContiguous(false) && SlotsContiguous(true)
                && CollectionPrototype.SettlementListSlot("home", true) == 0 && CollectionPrototype.SettlementListSlot("return", true) == 3,
                "Visible list buttons take contiguous slots from the top (a lone Return takes slot 1)");

            // Arrival: a fresh profile's placeholder state is reseeded from the live visit and camp, then discovered; never reseeded once discovered.
            SettlementState placeholder = SettlementState.New(0);
            SettlementState arrived = CollectionPrototype.SettlementArrival(placeholder, 12, "camp.04.13", new[] { "region.01", "region.02", "region.03", "region.04" });
            int derived = CollectionPrototype.SettlementSeed(12, "camp.04.13");
            Check(arrived.Discovered && arrived.PlotIndex == -1 && arrived.Seed == derived && derived != 0, "The first arrival reseeds from the visit and camp, then discovers the settlement without a plot");
            var snapshot = new SessionSnapshot { travel = new SavedTravel { visitNumber = 12, campId = "camp.04.13" } };
            Check(CollectionPrototype.SettlementSeed(snapshot) == derived, "The live seed derivation matches the snapshot derivation");
            SettlementState again = CollectionPrototype.SettlementArrival(arrived, 30, "camp.01.01", new[] { "region.01" });
            Check(again.Seed == derived && again.Discovered, "A discovered settlement is never reseeded");
            again.ChoosePlot(1, new[] { "region.01", "region.02", "region.03", "region.04" });
            var before = again.Neighbours.Select(n => (n.Plot, n.Level, n.Countdown)).ToArray();
            CollectionPrototype.SettlementArrival(again, 31, "camp.01.01", new[] { "region.01", "region.02", "region.03", "region.04" });
            Check(again.Neighbours.Select(n => (n.Plot, n.Level, n.Countdown)).SequenceEqual(before), "Arrival with no new regions changes nothing (idempotent)");
            CollectionPrototype.SettlementArrival(again, 32, "camp.01.01", new[] { "region.01", "region.02", "region.03", "region.04", "region.05" });
            Check(again.CountedRegions.Contains("region.05") && before.All(b => again.Neighbours.Any(n => n.Plot == b.Plot
                && (b.Countdown > 1 ? n.Level == b.Level && n.Countdown == b.Countdown - 1 : n.Level == b.Level + 1))),
                "Arrival applies regions discovered since the last visit");

            // Map shortcut (Task 6). Review Focus 2 (the shortcut is refused while busy) is pinned in TravelChecks:
            // "The shortcut is refused while busy" (CanTravelToSettlement(true, busy: true, ...) is false).
            Check(CollectionPrototype.ShowSettlementCard(true, true, false), "The Settlement card shows when discovered and seated at a camp");
            Check(!CollectionPrototype.ShowSettlementCard(false, true, false) && !CollectionPrototype.ShowSettlementCard(true, false, false) && !CollectionPrototype.ShowSettlementCard(true, true, true),
                "The Settlement card is hidden when undiscovered, not seated, or in the settlement");
            Check(CollectionPrototype.MapRegionTravelAllowed(true) && CollectionPrototype.MapRegionTravelAllowed(false), "Region travel is allowed on the map in the settlement and at camps");
            Check(CollectionPrototype.MapCampTravelAllowed(false) && !CollectionPrototype.MapCampTravelAllowed(true), "Camp travel on the map is refused in the settlement and allowed at camps");
            Check(CollectionPrototype.SettlementCampPageNote == "From the settlement, travel goes to the region's first camp.", "The settlement camp page note reads the first-camp text");
            var undiscoveredState = SettlementState.New(0);
            Check(!CollectionPrototype.ShowSettlementCard(undiscoveredState.Discovered, true, false), "A fresh profile's map shows no Settlement card (non-settlement map unchanged)");

            // Drag: the street follows the finger (a drag to the right moves the camera to the left), proportional to the drag.
            float right = CollectionPrototype.StreetDragMetres(100f, 1000f, 52f, 0.5f, 22f), left = CollectionPrototype.StreetDragMetres(-200f, 1000f, 52f, 0.5f, 22f);
            Check(right < 0f && left > 0f && Mathf.Abs(left + 2f * right) < 0.001f, "A horizontal drag scrolls the street proportionally, following the finger");
            ScrollChecks();
        }

        // Fluid street scrolling (user, November 13, 2026): StreetScroll run with explicit 60 fps times.
        private static void ScrollChecks()
        {
            const float dt = 1f / 60f;
            float Run(StreetScroll s, ref float t, float seconds) { for (float e = 0f; e < seconds; e += dt) { t += dt; s.Tick(dt, t); } return s.X; }
            // A drag at a steady speed for 0.2 s, released while moving: v metres per second of street.
            float Flick(StreetScroll s, ref float t, float v) { s.Begin(t); for (int i = 0; i < 12; i++) { t += dt; s.Drag(v * dt, t); } s.End(t); return s.Velocity; }

            float time = 0f;
            var slow = new StreetScroll(-500f, 500f, 0f);
            slow.Begin(time); for (int i = 0; i < 30; i++) { time += dt; slow.Drag(0.1f, time); }
            Check(Mathf.Abs(slow.X - 3f) < 0.001f, "While dragging, the street follows the finger 1:1 (30 moves of 0.1 m = 3 m)");
            time += 0.2f; slow.End(time); Run(slow, ref time, 1f);
            Check(Mathf.Abs(slow.X - 3f) < 0.001f && slow.Velocity == 0f, "A finger that stops before lifting leaves the street where it is (no glide)");

            time = 0f; var fling = new StreetScroll(-500f, 500f, 0f);
            float start = fling.X; Flick(fling, ref time, 12f); float dragged = fling.X - start, released = fling.X;
            Run(fling, ref time, 4f);
            float glide = fling.X - released, expected = 12f * StreetScroll.FlingBoost * StreetScroll.GlideTau;
            Check(fling.Velocity == 0f && Mathf.Abs(glide - expected) < 0.6f && glide > dragged, $"A flick glides on and slows to a stop ({glide:0.0} m after a {dragged:0.0} m flick at 12 m/s)");

            time = 0f; var chain = new StreetScroll(-1000f, 1000f, 0f);
            float v1 = Flick(chain, ref time, 20f); Run(chain, ref time, 0.15f);
            float v2 = Flick(chain, ref time, 20f); Run(chain, ref time, 0.15f);
            float v3 = Flick(chain, ref time, 20f);
            Check(v2 > v1 * 1.4f && v3 > v2 * 1.4f && v3 <= StreetScroll.MaxFling, $"Repeated flicks the same way speed up ({v1:0} → {v2:0} → {v3:0} m/s) for fast trips along the street");
            Run(chain, ref time, 1f); float v4 = Flick(chain, ref time, -20f);
            Check(Mathf.Abs(v4 + v1) < 0.5f, "A flick the other way starts again at normal speed");

            time = 0f; var edge = new StreetScroll(0f, 100f, 90f);
            Flick(edge, ref time, 40f); float peak = edge.X;   // released at 98 m, gliding at 64 m/s toward the end
            for (int i = 0; i < 240; i++) { time += dt; edge.Tick(dt, time); peak = Mathf.Max(peak, edge.X); }
            Check(peak > 100f && peak <= 100f + StreetScroll.Overscroll + 0.001f && Mathf.Abs(edge.X - 100f) < 0.02f && !edge.Moving, $"A glide past the end overshoots softly ({peak - 100f:0.0} m) and springs back to the last plot");
            edge.Begin(time); for (int i = 0; i < 20; i++) { time += dt; edge.Drag(1f, time); }
            Check(edge.X > 100f && edge.X - 100f < 20f * 0.5f && edge.X <= 100f + StreetScroll.Overscroll, "Dragging past an end resists (rubber band) and stops at the overscroll limit");
            time += 0.2f; edge.End(time); Run(edge, ref time, 2f);
            Check(Mathf.Abs(edge.X - 100f) < 0.02f, "Released past an end, the street springs back");

            time = 0f; var caught = new StreetScroll(-500f, 500f, 0f);
            Flick(caught, ref time, 30f); Run(caught, ref time, 0.1f);
            bool wasCaught = caught.Begin(time); float held = caught.X; Run(caught, ref time, 0.5f);
            Check(wasCaught && caught.X == held && caught.Velocity == 0f, "A finger landing on a gliding street stops it, and that touch is not a tap");
            caught.End(time + 0.3f);
            Check(!new StreetScroll(-500f, 500f, 0f).Begin(0f), "A touch on a still street can be a tap");

            time = 0f; var ease = new StreetScroll(-500f, 500f, 0f);
            ease.GlideTo(60f); float mid = Run(ease, ref time, 0.1f); Run(ease, ref time, 2f);
            Check(mid > 5f && mid < 55f && Mathf.Abs(ease.X - 60f) < 0.001f, "The list buttons ease the camera to a building instead of jumping");
            ease.GlideTo(900f); Run(ease, ref time, 3f);
            Check(Mathf.Abs(ease.X - 500f) < 0.001f, "An eased move stops at the end of the street");
        }

        private static string[] Ids(int from, int to) => Enumerable.Range(from, to - from + 1).Select(i => $"region.{i:00}").ToArray();

        private static void StateChecks()
        {
            SettlementState fresh = SettlementState.New(7);
            Check(!fresh.Discovered && fresh.PlotIndex == -1 && fresh.Neighbours.Count == 0, "A new settlement is undiscovered with no plot");
            fresh.Discover();
            Check(fresh.Discovered && fresh.PlotIndex == -1, "Discover marks the settlement found without choosing a plot");

            var s = SettlementState.New(7);
            s.ChoosePlot(4, new[] { "region.01", "region.02" });
            Check(s.PlotIndex == 4 && s.Neighbours.Count == 5, "Choosing plot 4 leaves five neighbours");
            Check(s.Neighbours.Select(n => n.Plot).OrderBy(p => p).SequenceEqual(new[] { 0, 1, 2, 3, 5 }), "Neighbours sit on plots 0, 1, 2, 3 and 5");
            Check(s.Neighbours.All(n => n.Level == 1 && n.Countdown >= 1 && n.Countdown <= 3), "Neighbours start at level 1 with countdowns of 1-3");

            Check(!s.CanChoosePlot(2, out string again) && !string.IsNullOrEmpty(again) && Throws(() => s.ChoosePlot(2, new string[0])) && s.PlotIndex == 4, "Choosing again is refused and the plot stays");
            var other = SettlementState.New(7);
            Check(!other.CanChoosePlot(-1, out string low) && !string.IsNullOrEmpty(low) && Throws(() => other.ChoosePlot(-1, new string[0])), "Plot -1 is refused");
            Check(!other.CanChoosePlot(6, out string high) && !string.IsNullOrEmpty(high) && Throws(() => other.ChoosePlot(6, new string[0])) && other.PlotIndex == -1, "Plot 6 is refused");
            Check(other.CanChoosePlot(0, out string none) && none == null && other.CanChoosePlot(5, out none), "Plots 0 and 5 are allowed");

            var twin = SettlementState.New(7);
            twin.ChoosePlot(4, new[] { "region.01", "region.02" });
            Check(s.Neighbours.Select(n => n.Countdown).SequenceEqual(twin.Neighbours.Select(n => n.Countdown)), "The same seed and plot give identical countdowns");

            Check(s.ApplyNewRegions(new[] { "region.01", "region.02" }) == 0 && s.Neighbours.All(n => n.Level == 1), "Regions discovered before the choice never count");
            Check(s.CountedRegions.SequenceEqual(new[] { "region.01", "region.02" }), "Earlier regions are recorded as counted");

            // Find a seed whose plot-4 settlement has a neighbour due to level up on the very next region.
            SettlementState probe = null;
            for (int seed = 0; seed < 200 && probe == null; seed++)
            {
                var t = SettlementState.New(seed); t.ChoosePlot(4, new string[0]);
                if (t.Neighbours.Any(n => n.Countdown == 1) && t.Neighbours.Any(n => n.Countdown > 1)) probe = t;
            }
            Check(probe != null, "A seed with a countdown of 1 exists");
            var before = probe.Neighbours.Select(n => (n.Plot, n.Level, n.Countdown)).ToList();
            int applied = probe.ApplyNewRegions(new[] { "region.05", "region.06" });
            Check(applied == 2, "Two new regions are applied");
            var one = SettlementState.Restore(true, 4, probe.Seed, before, new string[0]);
            Check(one.ApplyNewRegions(new[] { "region.05" }) == 1, "Regions apply one at a time");
            bool onceOk = true, slowOk = true;
            for (int i = 0; i < before.Count; i++)
            {
                SettlementState.Neighbour n = one.Neighbours[i];
                if (before[i].Countdown == 1) { onceOk &= n.Level == 2 && n.Countdown >= 1 && n.Countdown <= 3; }
                else { slowOk &= n.Level == 1 && n.Countdown == before[i].Countdown - 1; }
            }
            Check(onceOk, "A neighbour with countdown 1 gains exactly one level from region.05 and recounts");
            Check(slowOk, "Other neighbours only tick down");
            Check(probe.CountedRegions.SequenceEqual(new[] { "region.05", "region.06" }), "Applied regions are recorded");

            var snapshot = probe.Neighbours.Select(n => (n.Plot, n.Level, n.Countdown)).ToList();
            Check(probe.ApplyNewRegions(new[] { "region.05", "region.06" }) == 0 && probe.Neighbours.Select(n => (n.Plot, n.Level, n.Countdown)).SequenceEqual(snapshot), "Applying the same regions again changes nothing");

            var late = SettlementState.New(3); late.ChoosePlot(0, new string[0]);
            // Fifteen level-ups at most 3 regions each: every neighbour reaches 16 within 45 regions (the game has 48).
            Check(late.ApplyNewRegions(Ids(1, 60)) == 60 && late.Neighbours.All(n => n.Level <= SettlementState.MaxLevel && n.Level >= 1), "Levels never exceed 16 after 60 regions");
            Check(late.Neighbours.All(n => n.Level == 16), "Neighbours reach level 16 within 60 regions");

            var undecided = SettlementState.New(1);
            Check(undecided.ApplyNewRegions(Ids(1, 3)) == 0 && undecided.CountedRegions.Count == 0, "Regions before any plot choice apply nothing");

            // Restore.
            var round = SettlementState.Restore(probe.Discovered, probe.PlotIndex, probe.Seed, snapshot, probe.CountedRegions);
            Check(round.Discovered == probe.Discovered && round.PlotIndex == probe.PlotIndex && round.Seed == probe.Seed
                  && round.Neighbours.Select(n => (n.Plot, n.Level, n.Countdown)).SequenceEqual(snapshot) && round.CountedRegions.SequenceEqual(probe.CountedRegions), "Restore round-trips every field");
            var blank = SettlementState.Restore(false, -1, 9, new (int, int, int)[0], new string[0]);
            Check(!blank.Discovered && blank.PlotIndex == -1 && blank.Seed == 9 && blank.Neighbours.Count == 0, "Restore accepts an undecided settlement");
            Check(Throws(() => SettlementState.Restore(true, 4, 7, snapshot.Select((n, i) => i == 1 ? snapshot[0] : n), new string[0])), "Restore rejects duplicate neighbour plots");
            Check(Throws(() => SettlementState.Restore(true, 0, 7, snapshot, new string[0])), "Restore rejects a neighbour on the player's plot");
            Check(Throws(() => SettlementState.Restore(true, 4, 7, snapshot.Select((n, i) => i == 0 ? (n.Plot, 0, n.Countdown) : n), new string[0])), "Restore rejects level 0");
            Check(Throws(() => SettlementState.Restore(true, 4, 7, snapshot.Select((n, i) => i == 0 ? (n.Plot, 17, n.Countdown) : n), new string[0])), "Restore rejects level 17");
            Check(Throws(() => SettlementState.Restore(true, 7, 7, snapshot, new string[0])), "Restore rejects a plot outside 0-5");
        }
    }
}
