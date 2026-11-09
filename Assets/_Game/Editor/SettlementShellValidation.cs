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
            HudChecks();
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

                Check(Mathf.Abs(Find(street, "plot.1").CentreX - Find(street, "plot.0").CentreX - 11.2f) < 0.001f && Mathf.Abs(Find(street, "plot.2").CentreX - Find(street, "plot.1").CentreX - 11.2f) < 0.001f
                    && Mathf.Abs(Find(street, "plot.4").CentreX - Find(street, "plot.3").CentreX - 11.2f) < 0.001f && Mathf.Abs(Find(street, "plot.5").CentreX - Find(street, "plot.4").CentreX - 11.2f) < 0.001f, "Plots are 11.2 m apart");

                var fronts = new[] { "warehouse", "shop.bakery", "shop.ranch", "shop.farm", "shop.fishery", "shop.mine", "shop.tailor", "lumber" };
                bool spacing = true;
                for (int i = 0; i + 1 < fronts.Length; i++)
                    spacing &= Mathf.Abs(Find(street, fronts[i + 1]).CentreX - Find(street, fronts[i]).CentreX - ((manifest.Shops[i].Width + manifest.Shops[i + 1].Width) / 2f + 1.4f)) < 0.001f;
                Check(spacing, "Each storefront sits its manifest width plus 1.4 m from the next, the lumber frontage included");

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

                var cameraObject = new GameObject("Street camera check", typeof(Camera));
                try
                {
                    var cam = cameraObject.AddComponent<StreetCamera>(); cam.Bind(street);
                    cam.CentreOn(street.MaxX + 50f);
                    bool high = Mathf.Approximately(cam.X, street.MaxX);
                    cam.CentreOn(street.MinX - 50f);
                    bool low = Mathf.Approximately(cam.X, street.MinX);
                    cam.CentreOn(0f); cam.ScrollBy(2.5f);
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
                Check(Mathf.Abs(Find(holder, "lumber").CentreX - Find(holder, "shop.tailor").CentreX - ((5.4f + 8f) / 2f + 1.4f)) < 0.001f && lumber.GetComponent<BoxCollider>() != null, "The placeholder is 8 m wide and selectable");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }

            // Live branch with a fake front width.
            root = new GameObject("Street check root");
            try
            {
                SettlementStreet fake = SettlementScenery.Build(root.transform, new[] { 1, 1, 1, 1, 1, 1 }, -1, 9f, false);
                Check(HasChild(Find(fake, "lumber").Root, "LumberFront") && Mathf.Abs(Find(fake, "lumber").CentreX - Find(fake, "shop.tailor").CentreX - ((5.4f + 9f) / 2f + 1.4f)) < 0.001f
                    && Mathf.Abs(Find(fake, "plot.3").CentreX - Find(fake, "lumber").CentreX - (9f / 2f + 1.4f + 5.6f)) < 0.001f, "A given lumber front width drives the lumber spacing and the plots beyond it");
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

        // ---- Settlement HUD, list, notes, plot choice and Return (Task 5) ----
        private static void HudChecks()
        {
            StreetBuilding Shop(string id) => new StreetBuilding { Id = id, Kind = StreetBuildingKind.Shop };
            Check(CollectionPrototype.SettlementNote(Shop("shop.bakery"), 1, false) == "Bakery opens in Milestone 14", "The Bakery note reads \"Bakery opens in Milestone 14\"");
            var shops = new[] { ("shop.bakery", "Bakery"), ("shop.ranch", "Ranch"), ("shop.farm", "Farm"), ("shop.fishery", "Fishery"), ("shop.mine", "Mine"), ("shop.tailor", "Tailor") };
            Check(shops.All(s => CollectionPrototype.SettlementNote(Shop(s.Item1), 1, false) == s.Item2 + " opens in Milestone 14"), "Every shop note names its shop and Milestone 14");
            Check(CollectionPrototype.SettlementNote(new StreetBuilding { Id = "warehouse", Kind = StreetBuildingKind.Warehouse }, 1, false) == "The warehouse opens in Milestone 12", "The warehouse note is verbatim");
            Check(CollectionPrototype.SettlementNote(new StreetBuilding { Id = "lumber", Kind = StreetBuildingKind.LumberField }, 1, false) == "The lumber field opens in Milestone 13", "The lumber field note is verbatim");
            var house = new StreetBuilding { Id = "plot.2", Kind = StreetBuildingKind.House, Plot = 2 };
            Check(CollectionPrototype.SettlementNote(house, 3, false) == "A neighbour's home (level 3)", "A neighbour's house at level 3 reads \"A neighbour's home (level 3)\"");
            Check(CollectionPrototype.SettlementNote(house, 1, true) == "Your home opens in the next part of Milestone 10", "The player's plot note is verbatim");
            Check(CollectionPrototype.SettlementPlotPrompt == "Build your home here? This can't be changed." && CollectionPrototype.SettlementWelcome == "Welcome to the settlement."
                && CollectionPrototype.SettlementReturnPrompt("Bright Hollow") == "Return to Bright Hollow?", "The plot, welcome and Return texts are verbatim");

            Check(new[] { "home", "warehouse", "lumber", "return" }.All(key => Resources.Load<Sprite>("UI/Icons/" + key) != null), "The home, warehouse, lumber and return icons load as sprites from UI/Icons");
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
            Check(!CollectionPrototype.MapTravelAllowed(true) && CollectionPrototype.MapTravelAllowed(false), "The map never travels in the settlement and travels at camps");
            Check(CollectionPrototype.MapReadOnlyNote == "Use Return to travel", "The read-only map note reads \"Use Return to travel\"");
            var undiscoveredState = SettlementState.New(0);
            Check(!CollectionPrototype.ShowSettlementCard(undiscoveredState.Discovered, true, false), "A fresh profile's map shows no Settlement card (non-settlement map unchanged)");

            // Drag: the street follows the finger (a drag to the right moves the camera to the left), proportional to the drag.
            float right = CollectionPrototype.StreetDragMetres(100f, 1000f, 52f, 0.5f, 22f), left = CollectionPrototype.StreetDragMetres(-200f, 1000f, 52f, 0.5f, 22f);
            Check(right < 0f && left > 0f && Mathf.Abs(left + 2f * right) < 0.001f, "A horizontal drag scrolls the street proportionally, following the finger");
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
            Check(late.ApplyNewRegions(Ids(1, 40)) == 40 && late.Neighbours.All(n => n.Level <= 8 && n.Level >= 1), "Levels never exceed 8 after 40 regions");
            Check(late.Neighbours.All(n => n.Level == 8), "Neighbours reach level 8 within 40 regions");

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
            Check(Throws(() => SettlementState.Restore(true, 4, 7, snapshot.Select((n, i) => i == 0 ? (n.Plot, 9, n.Countdown) : n), new string[0])), "Restore rejects level 9");
            Check(Throws(() => SettlementState.Restore(true, 7, 7, snapshot, new string[0])), "Restore rejects a plot outside 0-5");
        }
    }
}
