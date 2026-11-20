using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Harvest;
using TheTrailWalker.Persistence;
using TheTrailWalker.Travel;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Milestone 13: the lumber field. Each task adds a group of checks called from Run().
    public static class Milestone13Validation
    {
        private static readonly List<string> passed = new List<string>();
        private static readonly DateTime T0 = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

        [MenuItem("The Trail Walker/Milestone 13/Validate Lumber Field")]
        public static void Run()
        {
            passed.Clear();
            DomainChecks();
            SaveChecks();
            CounterChecks();
            ZoneChecks();
            ChopChecks();
            PickupChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Lumber-checks.txt", passed);
            Debug.Log($"Milestone 13: {passed.Count} checks passed.");
        }

        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Milestone 13: " + name); passed.Add(name); }

        private static LumberFieldState Unlocked(int tier = 1) { return new LumberFieldState { UnlockedTier = tier, LastZone = 1 }; }
        private static string Pending(MassiveTree t) { return string.Join("|", t.Pending.Select(p => p.ItemId + "/" + p.Quality + "/" + p.Units)); }
        private static string Expected(int tier, int index, int cycle, bool boosted)
        {
            var rolled = HarvestManifest.Roll(InteractableKind.Tree, (tier - 1) * 6 + 1, "lumber.t" + tier, cycle, "tree" + index, boosted);
            return string.Join("|", rolled.GroupBy(b => (b.ItemId, b.Quality)).Select(g => g.Key.ItemId + "/" + g.Key.Quality + "/" + g.Count() * 50 * 1000L));
        }

        private static string Dump(LumberFieldState s)
        {
            var sb = new System.Text.StringBuilder(s.UnlockedTier + "/" + s.LastZone + "/" + s.DebugWoodTier + "/" + (long)s.ClockOffset.TotalMinutes);
            for (int t = 1; t <= 4; t++) for (int i = 0; i < 6; i++) { var tr = s.Tree(t, i); sb.Append(";" + tr.FellUtc?.Ticks + "," + tr.FellCount + "," + Pending(tr)); }
            return sb.ToString();
        }
        private static SessionSnapshot Snap(LumberFieldState s) { return new SessionSnapshot { version = 2, speedCap = 1.5f, survival = new SavedSurvival(), economy = 1, lumber = SessionSaveStore.SaveLumber(s) }; }
        private static SessionSnapshot Clone(SessionSnapshot s) { return JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(s)); }
        private static LumberFieldState Reload(LumberFieldState s) { return SessionSaveStore.RestoreLumber(Clone(Snap(s))); }
        private static bool Refused(SessionSnapshot s)
        {
            try { SessionSaveStore.Validate(Clone(s), float.MaxValue, out _); return false; } catch (InvalidDataException) { return true; }
        }

        private static bool IsPile(ItemEntry e) { return e.Id.StartsWith(SessionSaveStore.LumberPilePrefix, StringComparison.Ordinal); }
        private static int WorldPiles(SpatialInventory l) { return l.Entries.Count(e => e.Owner == ItemOwner.World && IsPile(e)); }
        private static int LivePiles(SpatialInventory l) { return l.Entries.Count(e => e.Owner != ItemOwner.Consumed && IsPile(e)); }
        private static ItemEntry Placed(SpatialInventory bag, ItemDefinition def, int quantity, int quality)
        {
            ItemEntry e = bag.AddWorld("bag-" + Guid.NewGuid().ToString("N"), def, quantity, quality, ItemPrices.Profile(def, quality));
            bag.Reserve(e, 7); bool ok = bag.TryPlace(e, 7, 0, 0, false, out _); bag.Release(e, 7);
            if (!ok) throw new Exception("Milestone 13: pickup fixture did not fit");
            return e;
        }
        // The quick swipe of the trail pickup path (Commit: TryQuickStore), returning the units the bag took.
        private static int Store(SpatialInventory bag, ItemEntry pile)
        {
            int before = pile.Quantity; bag.Reserve(pile, 7);
            bool ok = bag.TryQuickStore(pile, 7, out _); bag.Release(pile, 7);
            return ok ? CollectionPrototype.Stored(pile, before) : 0;
        }

        private static void PickupChecks()
        {
            Check(CollectionPrototype.PileId(1, 3, 2, "material.pine_log", 5) == "lumber-t1-tree3-c2-material.pine_log-q5", "PileId reads lumber-t1-tree3-c2-material.pine_log-q5");
            Check(CollectionPrototype.PileLabel("Pine log", 147, 5) == "Pine log ×147 · Q5", "PileLabel reads Pine log ×147 · Q5");

            string why; ItemDefinition log = TheTrailWalker.Domain.Crafting.Items.Find("material.pine_log");
            string Tops(List<ItemEntry> l) { return string.Join("|", l.Select(p => p.Definition.Id + "/" + p.Quality + "/" + p.Quantity)); }

            // Materializing: one World entry per item, its top bundle.
            var s = Unlocked(); LumberFieldService.Fell(s, 1, 3, false, T0, out why); MassiveTree tree = s.Tree(1, 3);
            var ledger = new SpatialInventory(8000, 4000);
            List<ItemEntry> piles = CollectionPrototype.MaterializePiles(ledger, tree, 1, 3);
            bool tops = piles.All(p => LumberFieldService.Top(tree, p.Definition.Id, out int q, out int u) && p.Quality == q && p.Quantity == u && p.Owner == ItemOwner.World
                && p.Id.StartsWith(CollectionPrototype.PileId(1, 3, 1, p.Definition.Id, q), StringComparison.Ordinal));
            Check(piles.Count == 3 && WorldPiles(ledger) == 3 && tops, "a felled pine materializes three lumber- entries, each its item's top bundle");
            string held = Pending(tree), first = Tops(piles);
            CollectionPrototype.ClearLumberPiles(ledger);
            Check(LivePiles(ledger) == 0, "closing the pickup removes every lumber- entry");
            List<ItemEntry> again = CollectionPrototype.MaterializePiles(ledger, tree, 1, 3);
            Check(LivePiles(ledger) == 3 && Tops(again) == first && Pending(tree) == held, "reopening rebuilds the same three tops from the counts");

            // A whole bundle into a bag with room: Bundles drops by one, the next top is the next quality.
            var t = new MassiveTree { FellUtc = T0, FellCount = 1 };
            t.Pending.Add(new PendingReward { ItemId = log.Id, Quality = 5, Units = 1000 });
            t.Pending.Add(new PendingReward { ItemId = log.Id, Quality = 3, Units = 5000 });
            var bag = new SpatialInventory(8000, 4000);
            ItemEntry pile = CollectionPrototype.MaterializePile(bag, t, 1, 0, log.Id);
            long bundles = LumberFieldService.Bundles(t, log.Id);
            int stored = Store(bag, pile);
            ItemEntry next = CollectionPrototype.TakePile(bag, t, 1, 0, pile, stored);
            ItemEntry carried = bag.Entries.SingleOrDefault(e => e.Owner == ItemOwner.Backpack);
            Check(pile.Quality == 5 && stored == 1000 && LumberFieldService.Bundles(t, log.Id) == bundles - 1 && next != null && next != pile && next.Quality == 3 && next.Quantity == 1000 && WorldPiles(bag) == 1,
                "storing a whole Q5 pile lowers Bundles by one and the next top is Q3");
            Check(carried != null && carried.Quantity == 1000 && carried.Quality == 5 && !IsPile(carried), "the stored bundle sits in the bag as 1,000 Q5 logs under a non-lumber id");

            // A bag that fits only 400: one 1,000-log footprint holding a 600 stack of the same item and quality.
            ItemEntry probe = new SpatialInventory(8000, 4000).AddWorld("probe", log, 1000);
            var tight = new SpatialInventory(probe.WidthFor(1000), probe.HeightFor(1000));
            ItemEntry stack = Placed(tight, log, 600, 5);
            var p4 = new MassiveTree { FellUtc = T0, FellCount = 1 }; p4.Pending.Add(new PendingReward { ItemId = log.Id, Quality = 5, Units = 1000 });
            pile = CollectionPrototype.MaterializePile(tight, p4, 1, 0, log.Id);
            stored = Store(tight, pile);
            next = CollectionPrototype.TakePile(tight, p4, 1, 0, pile, stored);
            Check(stored == 400 && stack.Quantity == 1000 && next == pile && pile.Owner == ItemOwner.World && pile.Quantity == 1000 - stored && p4.Pending.Single().Units == 1000 - stored,
                "a bag with room for 400 (a 600 stack of the same item and quality) stores 400 and the pile keeps 600");

            // A full bag: nothing stored, nothing taken.
            var full = new SpatialInventory(probe.WidthFor(1000), probe.HeightFor(1000));
            ItemEntry block = Placed(full, log, 1000, 1);
            var f = new MassiveTree { FellUtc = T0, FellCount = 1 }; f.Pending.Add(new PendingReward { ItemId = log.Id, Quality = 5, Units = 3000 });
            pile = CollectionPrototype.MaterializePile(full, f, 1, 0, log.Id); held = Pending(f);
            stored = Store(full, pile);
            Check(stored == 0 && pile.Owner == ItemOwner.World && pile.Quantity == 1000 && block.Quantity == 1000 && Pending(f) == held, "a full bag stores nothing and pending is unchanged");

            // A failed Rekey leaves no wood- World entry and the pile as it was; the lumber refusal text.
            var rk = new SpatialInventory(8000, 4000);
            ItemEntry rkPile = rk.AddWorld("lumber-rk", log, 1000, 5, ItemPrices.Profile(log, 5), true);
            rk.Reserve(rkPile, 7); rk.TryQuickStore(rkPile, 7, out _); rk.Release(rkPile, 7);
            Check(!CollectionPrototype.RekeyInto(rk, rkPile, true) && !rk.Entries.Any(e => e.Owner == ItemOwner.World && e.Id.StartsWith("wood-", StringComparison.Ordinal)) && rkPile.Owner == ItemOwner.Backpack && rkPile.Quantity == 1000,
                "a failed Rekey leaves no wood- World entry and the pile unchanged");
            Check(CollectionPrototype.PileRefusal(rkPile, "Backpack full: pickup remains on the trail") == "Backpack full: the rest stays on the pile"
                && CollectionPrototype.PileRefusal(rk.AddWorld("trail-x", log, 5), "Backpack full: pickup remains on the trail") == "Backpack full: pickup remains on the trail",
                "a lumber pile's full-bag refusal says the rest stays on the pile; the trail text is unchanged");

            // The tree regrows while its pickup is open.
            var r = Unlocked(); LumberFieldService.Fell(r, 1, 2, false, T0, out why);
            var open = new SpatialInventory(8000, 4000); CollectionPrototype.MaterializePiles(open, r.Tree(1, 2), 1, 2);
            Check(CollectionPrototype.RefreshLumber(r, T0.AddHours(71), open, 1, 2, out bool changed) == null && !changed && WorldPiles(open) == 3, "a refresh before 72 h leaves the pickup open");
            string note = CollectionPrototype.RefreshLumber(r, T0.AddHours(72), open, 1, 2, out changed);
            Check(note == "The tree regrew; its leftovers expired." && changed && r.Tree(1, 2).Standing && LivePiles(open) == 0, "a refresh that regrows the open tree closes it with its note and leaves no lumber- entry");

            // ResetHomeView with the pickup open (TearDownLumber: ClosePickup, then ClearLumberScene).
            var holder = new GameObject("Milestone 13 pickup holder");
            try
            {
                var home = new SpatialInventory(8000, 4000); CollectionPrototype.MaterializePiles(home, tree, 1, 3);
                Transform left = CollectionPrototype.ClearLumberScene(home, holder.transform);
                Check(left == null && holder == null && LivePiles(home) == 0, "the ResetHomeView teardown leaves no lumber- entry and no zone or counter holder");
            }
            finally { if (holder != null) UnityEngine.Object.DestroyImmediate(holder); }
        }

        private static void ChopChecks()
        {
            Check(CollectionPrototype.UsesCheck(20, 14) == "Needs 20 axe uses; you carry 14." && CollectionPrototype.UsesCheck(20, 20) == null, "the uses check refuses 14 of 20 with its text and passes 20 of 20");

            ToolSpec normal = new ToolSpec(ToolFamily.Axe, 1, false), elite = new ToolSpec(ToolFamily.Axe, 1, true);
            var bag = new List<(ToolSpec, int)> { (elite, 20), (normal, 10) };
            int pick = CollectionPrototype.PickTool(bag, true, 1, out int carried);
            Check(pick == 1 && CollectionPrototype.ActionsFor(normal, true, 1) == 20 && carried == 30 && CollectionPrototype.UsesCheck(20, carried) == null, "in pine the Normal axe (fewest uses) is picked, needs 20 cuts and the 30 carried uses let it start");
            Check(CollectionPrototype.PickTool(bag, true, 4, out carried) == -1 && carried == 0, "in oak (+3) no tier-1 axe is eligible");
            Check(CollectionPrototype.ActionsFor(normal, false, 1) == HarvestRules.Actions(normal, 1) && CollectionPrototype.ActionsFor(normal, false, 1) == 6, "a trail tree still uses the trail table (6 for a matching Normal axe)");
        }

        private static void ZoneChecks()
        {
            Check(Enumerable.Range(0, 6).Select(i => CollectionPrototype.MassiveVariant(2, i)).SequenceEqual(new[] { "MassiveBirchA", "MassiveBirchB", "MassiveBirchC", "MassiveBirchC", "MassiveBirchA", "MassiveBirchB" }), "tier 2 trees in anchor order are birch A, B, C, C, A, B");

            var s = Unlocked(); string why;
            LumberFieldService.Fell(s, 1, 0, false, T0, out why);
            MassiveTree felled = s.Tree(1, 0), stump = new MassiveTree { FellUtc = T0, FellCount = 1 };
            Check(CollectionPrototype.RegrowLabel(felled, T0.AddHours(9).AddMinutes(20)) == "Regrows in 2d 14h · leftovers expire then", "a felled tree with leftovers reads Regrows in 2d 14h · leftovers expire then");
            Check(CollectionPrototype.RegrowLabel(stump, T0.AddHours(66).AddMinutes(20)) == "Regrows in 5h 40m", "a stump under a day reads Regrows in 5h 40m");
            Check(CollectionPrototype.RegrowLabel(stump, T0.AddHours(48).AddSeconds(30)) == "Regrows in 23h 59m", "the label truncates (23 h 59 min 30 s reads 23h 59m)");
            Check(CollectionPrototype.RegrowLabel(s.Tree(1, 1), T0) == null, "a standing tree has no label");
            Check(CollectionPrototype.RegrowLabel(felled, T0.AddHours(-3)) == "Regrows in 3d 0h · leftovers expire then", "a clock before the fell reads the full 3d 0h");

            for (int tier = 1; tier <= 4; tier++)
            {
                var zone = Resources.Load<GameObject>("Settlement/LumberField/LumberZoneT" + tier);
                var names = zone == null ? new List<string>() : zone.GetComponentsInChildren<Transform>(true).Select(t => t.name).ToList();
                bool anchors = zone != null && names.Count(n => n.StartsWith("TreeAnchor", StringComparison.Ordinal)) == 6 && Enumerable.Range(1, 6).All(i => names.Contains("TreeAnchor" + i))
                    && new[] { "ZoneCameraAnchor", "PickupCameraAnchor", "ExitSelectAnchor" }.All(names.Contains);
                bool backdrop = tier < 4 ? Enumerable.Range(1, 3).All(i => names.Contains("BackdropAnchor" + i)) : !names.Any(n => n.StartsWith("BackdropAnchor", StringComparison.Ordinal));
                Check(anchors && backdrop, "LumberZoneT" + tier + " has six tree anchors, its camera, pickup and exit anchors and " + (tier < 4 ? "three backdrop anchors" : "no backdrop anchors"));
            }

            var root = new GameObject("Milestone 13 zone check");
            try
            {
                Transform zone = CollectionPrototype.BuildZone(root.transform, 1);
                Transform Anchor(string name) { return zone == null ? null : zone.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name); }
                var trees = Enumerable.Range(0, 6).Select(i => Anchor("TreeAnchor" + (i + 1))).Select(a => a != null && a.childCount == 1 ? a.GetChild(0) : null).ToList();
                Check(trees.All(t => t != null && t.GetComponent<Tier1PineRig>() != null && t.GetComponent<MassiveTreeArt>() != null) && trees.Select(t => t.name).SequenceEqual(Enumerable.Range(0, 6).Select(i => CollectionPrototype.MassiveVariant(1, i)))
                    && zone.GetComponentsInChildren<Tier1PineRig>(true).Length == 6, "zone 1 builds six live pine trees in variant order");
                var backdrop = Enumerable.Range(1, 3).Select(i => Anchor("BackdropAnchor" + i)).Select(a => a != null && a.childCount == 1 ? a.GetChild(0) : null).ToList();
                Check(backdrop.All(b => b != null && b.GetComponentsInChildren<Collider>(true).Length == 0 && b.GetComponentsInChildren<MonoBehaviour>(true).Length == 0 && b.GetComponentsInChildren<MeshRenderer>(true).Length > 0), "zone 1 has three static backdrop trees without colliders or scripts");
                Check(backdrop.All(b => b != null && b.name.Contains("Birch")), "zone 1's backdrop is birch");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static void CounterChecks()
        {
            var e = CollectionPrototype.LedgerEntries(Unlocked(1), new[] { 1, 2, 3, 4 });
            Check(e.Count == 2 && e[1].Tier == 2 && e[1].Line == "300 gold" && e[1].State == CollectionPrototype.LedgerEntryState.Buyable, "a discovered next tier shows its price, and unlocked 1 never lists a tier past the next");
            e = CollectionPrototype.LedgerEntries(new LumberFieldState(), new[] { 1 });
            Check(e.Count == 1 && e[0].Tier == 1 && e[0].Name == "Pine" && e[0].Line == "50 gold" && e[0].State == CollectionPrototype.LedgerEntryState.Buyable, "a fresh ledger page has one buyable pine entry at 50 gold");
            e = CollectionPrototype.LedgerEntries(Unlocked(1), new[] { 1 });
            Check(e.Count == 2 && e[0].State == CollectionPrototype.LedgerEntryState.Open && e[0].Line == "Open →" && e[1].Tier == 2 && e[1].State == CollectionPrototype.LedgerEntryState.Locked && e[1].Line == "Reach a birch region first", "an unlocked tier opens and an undiscovered next tier is locked with its reason");
            Check(CollectionPrototype.LedgerEntries(Unlocked(4), new[] { 1, 2, 3, 4 }).All(x => x.State == CollectionPrototype.LedgerEntryState.Open), "all four unlocked are four open entries");
            var cells = Enumerable.Range(0, 4).Select(CollectionPrototype.LedgerCell).ToArray();
            Check(cells.All(c => c.xMin >= 0f && c.yMin >= 0f && c.xMax <= 1f && c.yMax <= 1f), "ledger cells stay inside the page");
            Check(cells[0].center.x < 0.5f && cells[0].center.y > 0.5f && cells[1].center.x > 0.5f && cells[2].center.y < 0.5f && cells[3].center.x > 0.5f && cells[3].center.y < 0.5f, "ledger cells fill top left, top right, bottom left, bottom right");
            Check(!Enumerable.Range(0, 4).Any(i => Enumerable.Range(i + 1, 3 - i).Any(j => cells[i].Overlaps(cells[j]))), "ledger cells never overlap");
            for (int t = 1; t <= LumberFieldState.Tiers; t++)
                Check(Resources.Load<Sprite>(CollectionPrototype.LedgerArtPath(t)) != null, "the " + CollectionPrototype.LedgerArtPath(t) + " ledger picture loads as a sprite");
            Check(LumberLedgerArt.Sources[0] == "Tier01/Drops/PineLog" && LumberLedgerArt.Sources[1] == "Tier02/Drops/BirchLog", "pine and birch pictures render from the real drop logs");
            Check(LumberLedgerArt.Sources[2] == "Environment/CampProps/MesquiteLogSeat" && LumberLedgerArt.Sources[3] == "Environment/CampProps/OakLogSeat", "mesquite and oak pictures are still the log seat placeholders (swap when the other tool's logs land)");
            Check(CollectionPrototype.LumberDoorRotation(0) == Quaternion.identity && CollectionPrototype.LumberDoorRotation(1) == Quaternion.Euler(0f, 100f, 0f), "the door is closed at 0 and open at 100 degrees");
            Check(CollectionPrototype.LedgerPayRow(2, true, false) == -1 && CollectionPrototype.LedgerPayRow(2, true, true) == 1 && CollectionPrototype.LedgerPayRow(2, false, true) == -1, "PAY shows only on the price row after it is tapped");
            Check(CollectionPrototype.NextDebugWoodTier(0) == 2 && CollectionPrototype.NextDebugWoodTier(2) == 3 && CollectionPrototype.NextDebugWoodTier(3) == 4 && CollectionPrototype.NextDebugWoodTier(4) == 4, "WOOD T2-T4 goes 0 to 2, then 3, 4 and stays at 4");
            var prefab = Resources.Load<GameObject>("Settlement/Storefronts/LumberCounter");
            Check(prefab != null && new[] { "LedgerAnchor", "LedgerSelectAnchor", "DoorPivot", "DoorSelectAnchor", "CustomerCameraAnchor", "Shopkeeper" }.All(n => prefab.GetComponentsInChildren<Transform>(true).Any(t => t.name == n)), "the LumberCounter prefab has its anchors");
            foreach (Vector2 screen in new[] { new Vector2(1116, 2480), new Vector2(720, 1680) })
            {
                var counter = UnityEngine.Object.Instantiate(prefab);
                var cam = new GameObject("lumber view check camera").AddComponent<Camera>();
                try
                {
                    cam.fieldOfView = StreetCamera.FieldOfView; cam.aspect = screen.x / screen.y;
                    Pose pose = CollectionPrototype.LumberCounterPose(cam, counter.transform);
                    cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                    foreach (string n in new[] { "LedgerSelectAnchor", "DoorSelectAnchor" })
                    {
                        Vector3 v = cam.WorldToViewportPoint(counter.transform.Find(n).position);
                        Check(v.z > 0f && v.x > 0.05f && v.x < 0.95f && v.y > 0.12f && v.y < 0.48f, n + " is on screen in the tap band below the debug buttons at " + screen.x + "x" + screen.y + ": " + v.ToString("0.000"));
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(cam.gameObject); UnityEngine.Object.DestroyImmediate(counter); }
            }
            // Phone round 2: the chop screen frames the trunk at the cut, not the buttress roots. The swipe band (the cut box's
            // lower 55%, above the 12% gesture inset, as EvaluateCut reads it) holds the cut, and a swipe across the trunk there
            // covers the 70% of the box width that EvaluateCut asks for.
            var chopCam = new GameObject("massive chop check camera").AddComponent<Camera>();
            try
            {
                foreach (string species in new[] { "Pine", "Birch", "Mesquite", "Oak" })
                    foreach (char variant in "ABC")
                    {
                        var tree = Resources.Load<GameObject>("Settlement/LumberField/Massive" + species + variant);
                        var art = tree.GetComponent<MassiveTreeArt>();
                        Bounds box = CollectionPrototype.MassiveCutBounds(tree.transform);
                        foreach (Vector2 screen in new[] { new Vector2(1116, 2480), new Vector2(720, 1680) })
                        {
                            chopCam.fieldOfView = StreetCamera.FieldOfView; chopCam.aspect = screen.x / screen.y;
                            Pose pose = CollectionPrototype.MassiveTrunkPose(box, Vector3.back, chopCam.fieldOfView, chopCam.aspect);
                            chopCam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                            float xMin = 1f, xMax = 0f, yMin = 1f, yMax = 0f;
                            for (int i = 0; i < 8; i++)
                            {
                                Vector3 v = chopCam.WorldToViewportPoint(box.center + Vector3.Scale(box.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1)));
                                xMin = Mathf.Min(xMin, v.x); xMax = Mathf.Max(xMax, v.x); yMin = Mathf.Min(yMin, v.y); yMax = Mathf.Max(yMax, v.y);
                            }
                            float zoneBottom = Mathf.Max(yMin, 0.12f), zoneTop = yMin + (yMax - yMin) * 0.55f;
                            Vector3 cut = tree.transform.position + Vector3.up * art.CutHeight, side = chopCam.transform.right * art.CutRadius;
                            float cutY = chopCam.WorldToViewportPoint(cut).y, trunk = chopCam.WorldToViewportPoint(cut + side).x - chopCam.WorldToViewportPoint(cut - side).x;
                            float margin = (zoneTop - zoneBottom) * 0.2f;   // the cut sits inside the band, not on its edge
                            Check(zoneTop - zoneBottom >= 0.15f && cutY >= zoneBottom + margin && cutY <= zoneTop - margin && trunk >= 0.7f * (xMax - xMin) && xMin >= 0f && xMax <= 1f,
                                tree.name + " chop view at " + screen.x + "x" + screen.y + ": the swipe band holds the cut and a swipe across the trunk is wide enough (band " + zoneBottom.ToString("0.00") + "-" + zoneTop.ToString("0.00") + ", cut " + cutY.ToString("0.00") + ", trunk " + trunk.ToString("0.00") + " of " + (xMax - xMin).ToString("0.00") + ")");
                        }
                    }
            }
            finally { UnityEngine.Object.DestroyImmediate(chopCam.gameObject); }
            Check(CollectionPrototype.OpensLumberCounter(null, StreetBuildingKind.LumberField) && CollectionPrototype.OpensLumberCounter("lumber", StreetBuildingKind.House) && !CollectionPrototype.OpensLumberCounter("home", StreetBuildingKind.House), "the lumber building and list entry open the counter");
            Vector4 band = CollectionPrototype.LumberNoteBand;
            Check(Mathf.Abs((band.x + band.z) / 2f - 0.5f) <= 0.05f && Mathf.Abs((band.y + band.w) / 2f - 0.5f) <= 0.05f && band.z - band.x >= 0.6f, "the lumber paper note is centred and wide");
        }

        private static void SaveChecks()
        {
            string why;
            var s = new LumberFieldState { UnlockedTier = 2, LastZone = 2, DebugWoodTier = 3, ClockOffset = TimeSpan.FromMinutes(1440) };
            LumberFieldService.Fell(s, 1, 3, true, T0, out why);
            LumberFieldService.Fell(s, 2, 1, false, T0.AddHours(2), out why);
            s.Tree(2, 1).Pending.Clear();
            s.Tree(2, 1).Pending.Add(new PendingReward { ItemId = "material.birch_log", Quality = 5, Units = 1500 });
            s.Tree(2, 1).Pending.Add(new PendingReward { ItemId = "material.birch_log", Quality = 2, Units = 50000 });
            s.Tree(2, 1).Pending.Add(new PendingReward { ItemId = "ingredient.birch_sap", Quality = 3, Units = 1000 });
            Check(!s.Tree(1, 3).Standing && s.Tree(1, 3).Pending.Count > 0 && s.Tree(2, 0).Standing, "round trip fixture: tree 1-3 felled, 2-0 standing");
            LumberFieldState back = Reload(s);
            Check(Dump(back) == Dump(s) && back.ClockOffset == TimeSpan.FromMinutes(1440) && back.Tree(1, 3).FellUtc == T0 && back.Tree(1, 3).FellUtc.Value.Kind == DateTimeKind.Utc && back.Tree(2, 0).Standing, "a lumber field saves and reloads equal");

            Check(Dump(SessionSaveStore.RestoreLumber(new SessionSnapshot())) == Dump(new LumberFieldState()) && Dump(new LumberFieldState()).StartsWith("0/0/0/0;,0,"), "a save without lumber restores a fresh state");

            SessionSnapshot good = Snap(s);
            Check(!Refused(good), "the round trip snapshot validates");
            SessionSnapshot x;
            x = Clone(good); x.lumber.trees.RemoveAt(23); Check(Refused(x), "23 trees are refused");
            x = Clone(good); x.lumber.trees.Add(new SavedMassiveTree()); Check(Refused(x), "25 trees are refused");
            x = Clone(good); x.lumber.unlockedTier = 5; Check(Refused(x), "unlockedTier 5 is refused");
            x = Clone(good); x.lumber.unlockedTier = -1; Check(Refused(x), "unlockedTier -1 is refused");
            x = Clone(good); x.lumber.lastZone = 3; Check(Refused(x), "lastZone above unlockedTier is refused");
            x = Clone(good); x.lumber.lastZone = 0; Check(Refused(x), "lastZone 0 with a tier unlocked is refused");
            x = Clone(good); x.lumber.debugWoodTier = 5; Check(Refused(x), "debugWoodTier 5 is refused");
            x = Clone(good); x.lumber.clockOffsetMinutes = -1; Check(Refused(x), "a negative clock offset is refused");
            x = Clone(good); x.lumber.trees[0].fellCount = -1; Check(Refused(x), "a negative fellCount is refused");
            x = Clone(good); x.lumber.trees[0].pending.Add(new SavedPendingReward { itemId = "material.pine_log", quality = 1, units = 1000 }); Check(Refused(x), "a standing tree with pending rows is refused");
            x = Clone(good); x.lumber.trees[3].fellCount = 0; Check(Refused(x), "a felled tree with fellCount 0 is refused");
            x = Clone(good); x.lumber.trees[3].pending[0].itemId = "material.oak_log"; Check(Refused(x), "an item of another tier's tree table is refused");
            x = Clone(good); x.lumber.trees[3].pending[0].quality = 6; Check(Refused(x), "quality 6 is refused");
            x = Clone(good); x.lumber.trees[3].pending[0].quality = 0; Check(Refused(x), "quality 0 is refused");
            x = Clone(good); x.lumber.trees[3].pending[0].units = 0; Check(Refused(x), "zero units are refused");
            x = Clone(good); x.lumber.trees[3].pending.Add(new SavedPendingReward { itemId = x.lumber.trees[3].pending[0].itemId, quality = x.lumber.trees[3].pending[0].quality, units = 1000 }); Check(Refused(x), "a repeated item and quality is refused");

            // Take then reload: the pending units drop by exactly what was taken, each time.
            var t = new LumberFieldState { UnlockedTier = 1, LastZone = 1 };
            LumberFieldService.Fell(t, 1, 0, false, T0, out why);
            LumberFieldService.Top(t.Tree(1, 0), "material.pine_log", out int q, out int u);
            long Units() { return t.Tree(1, 0).Pending.Where(p => p.ItemId == "material.pine_log" && p.Quality == q).Sum(p => p.Units); }
            long start = Units();
            LumberFieldService.Take(t.Tree(1, 0), "material.pine_log", q, 1000); t = Reload(t);
            Check(Units() == start - 1000, "after taking 1,000 and reloading, the units drop by 1,000");
            LumberFieldService.Take(t.Tree(1, 0), "material.pine_log", q, 400); t = Reload(t);
            Check(Units() == start - 1400, "after taking 400 more and reloading, the units drop by 1,400 in all");

            // Pile entries are never saved and never loaded.
            var ledger = new SpatialInventory(8000, 4000);
            const string PileId = "lumber-t1-tree0-c1-material.pine_log-q5";
            var logDef = TheTrailWalker.Domain.Crafting.Items.Find("material.pine_log");
            ItemEntry pile = ledger.AddWorld(PileId, logDef, 1, 5, ItemPrices.Profile(logDef, 5), true);
            ItemEntry plain = ledger.AddWorld("world-log", logDef, 1, 1, ItemPrices.Profile(logDef, 1), true);
            ItemEntry packed = ledger.AddWorld("pack-log", logDef, 1, 1, ItemPrices.Profile(logDef, 1), true);
            ledger.RestoreStored(packed, ItemOwner.Backpack, 0, 0);
            Check(plain.Owner == ItemOwner.World && packed.Owner == ItemOwner.Backpack && pile.Owner == ItemOwner.World, "pile fixture: two World entries and one Backpack entry");
            var written = new SessionSnapshot();
            CollectionPrototype.AppendSavedItems(written, ledger, true, "seg", e => Vector3.zero, e => null);
            var kept = written.items.Select(e => e.id).ToList();
            Check(kept.Count == 2 && kept.Contains("world-log") && kept.Contains("pack-log") && !kept.Any(id => id.StartsWith(SessionSaveStore.LumberPilePrefix, StringComparison.Ordinal)), "the snapshot item list skips only the lumber World pile");
            var loaded = new SessionSnapshot();
            loaded.items.Add(new SavedItem { id = PileId, owner = "World", definitionId = "material.pine_log" });
            loaded.items.Add(new SavedItem { id = "world-log", owner = "World", definitionId = "material.pine_log" });
            loaded.items.Add(new SavedItem { id = "pack-log", owner = "Backpack", definitionId = "material.pine_log" });
            CollectionPrototype.PrepareRestore(loaded, false);
            Check(loaded.items.Count == 2 && loaded.items.All(i => i.id != PileId), "the restore preparation removes the lumber pile and keeps the others");
        }

        private static void DomainChecks()
        {
            Check(Enumerable.Range(1, 4).Select(t => LumberFieldService.UnlockPrice(t)).SequenceEqual(new long[] { 50, 300, 750, 1500 }), "unlock prices are 50, 300, 750 and 1,500 gold");
            Check(new[] { 1, 2, 3, 4 }.Select(LumberFieldService.WoodName).SequenceEqual(new[] { "pine", "birch", "mesquite", "oak" }), "the four woods are pine, birch, mesquite and oak");

            // Unlocking.
            var s = new LumberFieldState(); var wallet = new Wallet(0, 50); string why;
            Check(LumberFieldService.NextTier(s) == 1, "a fresh field offers tier 1 next");
            Check(!LumberFieldService.Unlock(s, wallet, 2, new[] { 1, 2 }, out why) && s.UnlockedTier == 0 && wallet.Gold == 50, "tier 2 cannot be bought before tier 1");
            wallet = new Wallet(0, 49);
            Check(!LumberFieldService.Unlock(s, wallet, 1, new[] { 1 }, out why) && why == "Not enough gold" && wallet.Gold == 49 && s.UnlockedTier == 0 && s.LastZone == 0, "49 gold refuses tier 1 and changes nothing");
            wallet = new Wallet(0, 50);
            Check(LumberFieldService.Unlock(s, wallet, 1, new[] { 1 }, out why) && wallet.Gold == 0 && s.UnlockedTier == 1 && s.LastZone == 1 && LumberFieldService.NextTier(s) == 2, "50 gold unlocks tier 1 and sets the zone");
            wallet = new Wallet(0, 300);
            Check(!LumberFieldService.Unlock(s, wallet, 2, new[] { 1 }, out why) && why == "Reach a birch region first" && wallet.Gold == 300 && s.UnlockedTier == 1, "an undiscovered birch refuses tier 2");
            s.DebugWoodTier = 2;
            Check(LumberFieldService.Unlock(s, wallet, 2, new[] { 1 }, out why) && wallet.Gold == 0 && s.UnlockedTier == 2 && s.LastZone == 1, "the debug wood tier allows tier 2");
            Check(LumberFieldService.NextTier(new LumberFieldState { UnlockedTier = 4 }) == 0, "NextTier is 0 when all four are unlocked");

            // Massive actions: Normal / Elite by target minus tool tier.
            var table = new Dictionary<int, (int normal, int elite)> { { 2, (80, 80) }, { 1, (40, 40) }, { 0, (20, 10) }, { -1, (10, 5) }, { -2, (5, 3) }, { -3, (3, 2) } };
            bool tableOk = true;
            for (int tool = 1; tool <= 4; tool++)
                for (int target = 1; target <= 4; target++)
                {
                    int d = target - tool;
                    if (d > 2) { tableOk &= LumberFieldService.MassiveActions(new ToolSpec(ToolFamily.Axe, tool, false), target) == null && LumberFieldService.MassiveActions(new ToolSpec(ToolFamily.Axe, tool, true), target) == null; continue; }
                    var row = table[Math.Max(d, -3)];
                    tableOk &= LumberFieldService.MassiveActions(new ToolSpec(ToolFamily.Axe, tool, false), target) == row.normal && LumberFieldService.MassiveActions(new ToolSpec(ToolFamily.Axe, tool, true), target) == row.elite;
                }
            Check(tableOk, "the massive action table matches for every tool and target tier");
            Check(LumberFieldService.MassiveActions(new ToolSpec(ToolFamily.Axe, 1, false), 4) == null, "a +3 target is ineligible");

            // Felling: x50, stored as units.
            var f = Unlocked(); MassiveTree tree = f.Tree(1, 0);
            Check(LumberFieldService.Fell(f, 1, 0, false, T0, out why) && tree.FellCount == 1 && tree.FellUtc == T0 && !tree.Standing, "a fell marks the tree felled with count 1");
            Check(Pending(tree) == Expected(1, 0, 1, false), "pending is the roll x50 x1,000 units per item and quality");
            var items = tree.Pending.Select(p => p.ItemId).Distinct().ToList();
            Check(items.Count == 3 && items.All(i => i == "material.pine_branch" || i == "material.pine_log" || i == "ingredient.pine_nuts"), "a pine tree holds branch, log and nuts");
            Check(LumberFieldService.PileItems(tree).SequenceEqual(new[] { "material.pine_branch", "material.pine_log", "ingredient.pine_nuts" }), "the piles list branch, log, ingredient");
            string before = Pending(tree);
            Check(!LumberFieldService.Fell(f, 1, 0, false, T0.AddHours(1), out why) && Pending(tree) == before && tree.FellCount == 1, "a felled tree cannot be felled again");
            Check(!LumberFieldService.Fell(f, 2, 0, false, T0, out why) && f.Tree(2, 0).Standing && f.Tree(2, 0).FellCount == 0, "a locked tier cannot be felled");

            var e = Unlocked();
            Check(LumberFieldService.Fell(e, 1, 0, true, T0, out why) && Pending(e.Tree(1, 0)) == Expected(1, 0, 1, true), "a boosted fell uses the Elite roll x50");
            var twin = Unlocked(); LumberFieldService.Fell(twin, 1, 3, false, T0, out why); var twin2 = Unlocked(); LumberFieldService.Fell(twin2, 1, 3, false, T0.AddHours(5), out why);
            Check(Pending(twin.Tree(1, 3)) == Pending(twin2.Tree(1, 3)) && Pending(twin.Tree(1, 3)).Length > 0, "the same tree and count roll the same pending");
            var higher = Unlocked(2); LumberFieldService.Fell(higher, 2, 1, false, T0, out why);
            Check(Pending(higher.Tree(2, 1)) == Expected(2, 1, 1, false) && higher.Tree(2, 1).Pending.All(p => p.ItemId.Contains("birch")), "a birch tree rolls birch");

            // Taking: highest quality first, one bundle at a time.
            var tk = new MassiveTree();
            tk.Pending.Add(new PendingReward { ItemId = "material.pine_log", Quality = 2, Units = 50000 });
            tk.Pending.Add(new PendingReward { ItemId = "material.pine_log", Quality = 5, Units = 1500 });
            tk.Pending.Add(new PendingReward { ItemId = "material.pine_branch", Quality = 3, Units = 1000 });
            Check(LumberFieldService.Top(tk, "material.pine_log", out int q, out int units) && q == 5 && units == 1000, "Top gives the best quality and a 1,000 unit bundle");
            Check(!LumberFieldService.Top(tk, "ingredient.pine_nuts", out _, out _), "Top of an absent item is false");
            long bundles = LumberFieldService.Bundles(tk, "material.pine_log");
            Check(bundles == 52, "Bundles rounds the units up (" + bundles + ")");
            LumberFieldService.Take(tk, "material.pine_log", 5, 1000);
            Check(LumberFieldService.Bundles(tk, "material.pine_log") == bundles - 1 && LumberFieldService.Top(tk, "material.pine_log", out q, out units) && q == 5 && units == 500, "taking 1,000 lowers Bundles by one and leaves 500");
            LumberFieldService.Untake(tk, "material.pine_log", 5, 1000);
            Check(LumberFieldService.Bundles(tk, "material.pine_log") == bundles && LumberFieldService.Top(tk, "material.pine_log", out q, out units) && q == 5 && units == 1000, "untaking restores the row exactly");
            var partial = new MassiveTree(); partial.Pending.Add(new PendingReward { ItemId = "material.pine_log", Quality = 4, Units = 1000 });
            LumberFieldService.Take(partial, "material.pine_log", 4, 400);
            Check(LumberFieldService.Top(partial, "material.pine_log", out q, out units) && q == 4 && units == 600, "a partial take leaves 600 at the same quality");
            LumberFieldService.Untake(partial, "material.pine_log", 4, 400);
            Check(LumberFieldService.Top(partial, "material.pine_log", out q, out units) && q == 4 && units == 1000 && partial.Pending.Count == 1, "untaking 400 restores 1,000");
            LumberFieldService.Take(partial, "material.pine_log", 4, 1000);
            Check(partial.Pending.Count == 0 && LumberFieldService.Bundles(partial, "material.pine_log") == 0, "a row is removed at zero");

            // Six independent timers.
            var t6 = Unlocked();
            for (int i = 0; i < 6; i++) LumberFieldService.Fell(t6, 1, i, false, T0.AddHours(10 * i), out why);
            var changed = LumberFieldService.Refresh(t6, T0.AddHours(85));
            Check(changed.SequenceEqual(new[] { (1, 0), (1, 1) }), "at 85 h only trees 0 and 1 regrow (tree 1 fell at 10 h, so 80 h is 8 h too early)");
            Check(t6.Tree(1, 0).Standing && t6.Tree(1, 0).Pending.Count == 0 && t6.Tree(1, 1).Standing && t6.Tree(1, 1).Pending.Count == 0, "regrown trees stand with empty pending");
            Check(Enumerable.Range(2, 4).All(i => !t6.Tree(1, i).Standing && t6.Tree(1, i).Pending.Count > 0), "trees 2 to 5 keep their pending");
            Check(LumberFieldService.ExpiresAt(t6.Tree(1, 2)) == T0.AddHours(20 + 72) && LumberFieldService.Remaining(t6.Tree(1, 2), T0.AddHours(85)) == TimeSpan.FromHours(7), "ExpiresAt and Remaining agree");

            // A clock before the fell counts as zero elapsed.
            var back = Unlocked(); LumberFieldService.Fell(back, 1, 0, false, T0, out why); string kept = Pending(back.Tree(1, 0));
            Check(LumberFieldService.Refresh(back, T0.AddHours(-5)).Count == 0 && !back.Tree(1, 0).Standing && Pending(back.Tree(1, 0)) == kept, "a backward clock changes nothing");
            Check(LumberFieldService.Remaining(back.Tree(1, 0), T0.AddHours(-5)) == TimeSpan.FromHours(72), "a backward clock leaves 72 h, not more");
            Check(LumberFieldService.Refresh(back, T0.AddHours(72)).Count == 1 && back.Tree(1, 0).Standing, "exactly 72 h regrows the tree");
        }
    }
}
