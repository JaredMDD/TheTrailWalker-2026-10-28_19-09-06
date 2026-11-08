using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Milestone 6: harvesting rules (plan sections 7.1-7.3, 8.1-8.2.3). Gesture and presentation behaviour is
    // verified on the device; these checks cover the rules the interactions rely on.
    public static class Milestone6Validation
    {
        private static readonly List<string> passed = new List<string>();
        private static void Check(bool value, string name) { if (!value) throw new Exception("Milestone 6: " + name); passed.Add(name); }

        // Animal movement (plan 8.2.2, user, November 7, 2026): simulated on a copy of the live rabbit prefab on flat ground.
        private static void AnimalMotionChecks()
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/Tier01/Interactables/Rabbit.prefab");
            Check(prefab != null && prefab.GetComponent<TheTrailWalker.Harvest.Tier1RabbitRig>() != null, "The live rabbit prefab keeps its rig");
            var rabbit = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var rig = rabbit.GetComponent<TheTrailWalker.Harvest.Tier1RabbitRig>();
                Vector3 home = new Vector3(3, 0, 5); rabbit.transform.position = home;
                var motion = new TheTrailWalker.Harvest.AnimalWanderer(new TheTrailWalker.Harvest.AnimalAnimator(rig.Parts()), home, 1234);
                const float dt = 1f / 30f; float farthest = 0, lowestFoot = 0, maxHop = 0; bool finite = true, moved = false, grazed = false, looked = false;
                float headRest = rig.HeadPivot.localPosition.y;
                for (int i = 0; i < 30 * 120; i++)
                {
                    float t = 100f + i * dt; Vector3 before = rabbit.transform.position;
                    motion.UpdateIdle(rabbit.transform, dt, t, at => 0f);
                    Vector3 p = rabbit.transform.position; finite &= !float.IsNaN(p.x + p.z) && !float.IsNaN(rig.MotionRoot.localPosition.y);
                    farthest = Mathf.Max(farthest, new Vector2(p.x - home.x, p.z - home.z).magnitude);
                    moved |= (p - before).sqrMagnitude > 1e-8f; maxHop = Mathf.Max(maxHop, rig.MotionRoot.localPosition.y);
                    grazed |= rig.HeadPivot.localPosition.y < headRest - .1f; looked |= Mathf.Abs(rig.HeadPivot.localEulerAngles.y) > 10f && Mathf.Abs(rig.HeadPivot.localEulerAngles.y) < 350f;
                    foreach (var r in new[] { rig.FrontLeftPivot, rig.FrontRightPivot, rig.HindLeftPivot, rig.HindRightPivot })
                        foreach (var renderer in r.GetComponentsInChildren<Renderer>()) lowestFoot = Mathf.Min(lowestFoot, renderer.bounds.min.y);
                }
                Check(finite && moved && grazed && looked, "An idle animal grazes (head down), looks around and moves about its spawn point");
                Check(farthest <= 1.0f + .25f, $"An idle animal stays inside its encounter area (farthest {farthest:0.00} m of 1 m)");
                Check(maxHop > .03f && maxHop < .2f, $"Rabbits travel in hops that leave the ground (peak {maxHop:0.000} m)");
                Check(lowestFoot > -.02f, $"Feet stay on or above the ground while hopping and grazing (lowest {lowestFoot:0.000} m)");

                // Hunted: alert travel toward the hunt's point; faster after a miss (plan 8.2.2: about 0.55 and 1.7 m/s before).
                float Speed(bool boosted)
                {
                    rabbit.transform.SetPositionAndRotation(home, Quaternion.identity); motion.Stop();
                    Vector3 goal = home + new Vector3(0, 0, 40); float t0 = 500f; int frames = 30 * 6;
                    for (int i = 0; i < frames; i++) motion.UpdateHunted(rabbit.transform, goal, boosted, dt, t0 + i * dt, at => 0f);
                    return (rabbit.transform.position.z - home.z) / (frames * dt);
                }
                float calm = Speed(false), fleeing = Speed(true);
                Check(calm > .35f && calm < .8f && fleeing > 1.2f && fleeing < 2.4f, $"A hunted rabbit hops at {calm:0.00} m/s, {fleeing:0.00} m/s after a miss");
            }
            finally { UnityEngine.Object.DestroyImmediate(rabbit); }
        }

        public static void Run()
        {
            passed.Clear();
            var axe1 = new ToolSpec(ToolFamily.Axe, 1, false); var axe2 = new ToolSpec(ToolFamily.Axe, 2, false); var axe2e = new ToolSpec(ToolFamily.Axe, 2, true);
            var bow2 = new ToolSpec(ToolFamily.Hunting, 2, false); var bow2e = new ToolSpec(ToolFamily.Hunting, 2, true);
            var rod2 = new ToolSpec(ToolFamily.Rod, 2, false); var rod2e = new ToolSpec(ToolFamily.Rod, 2, true);

            Check(HarvestRules.Actions(axe2, 2) == 6 && HarvestRules.Actions(axe2e, 2) == 3 && HarvestRules.Actions(bow2, 2) == 3 && HarvestRules.Actions(bow2e, 2) == 1
                && HarvestRules.Actions(rod2, 2) == 3 && HarvestRules.Actions(rod2e, 2) == 1,
                "Matching tier: Normal axe 6 / Elite 3; hunting and fishing Normal 3 / Elite 1");
            Check(HarvestRules.Actions(axe2, 1) == 3 && HarvestRules.Actions(axe2e, 1) == 2 && HarvestRules.Actions(bow2, 1) == 1 && HarvestRules.Actions(bow2e, 1) == 1 && HarvestRules.Actions(rod2e, 1) == 1,
                "Lower-tier targets: axe 3 / Elite 2; hunting and fishing stay at the one-action floor");
            Check(HarvestRules.Actions(axe2, 3) == 12 && HarvestRules.Actions(axe2e, 3) == 12 && HarvestRules.Actions(axe2, 4) == 24 && HarvestRules.Actions(axe2e, 4) == 24
                && HarvestRules.Actions(bow2, 3) == 6 && HarvestRules.Actions(bow2e, 4) == 12,
                "+1 and +2 targets stay eligible with the Normal counts for both variants");
            Check(HarvestRules.Actions(axe1, 4) == null && HarvestRules.Actions(bow2e, 5) == null, "A target three or more tiers above the tool cannot start");
            Check(HarvestRules.Boosted(axe2e, 1) && HarvestRules.Boosted(axe2e, 2) && HarvestRules.Boosted(axe2e, 3) && HarvestRules.Boosted(axe2e, 4)
                && !HarvestRules.Boosted(axe2, 3) && !HarvestRules.Boosted(axe2, 2) && !HarvestRules.Boosted(new ToolSpec(ToolFamily.Axe, 1, true), 4),
                "Elite boosts drops on every eligible target (including +1/+2); Normal never does; no boost when ineligible");
            Check(HarvestRules.Compare(axe2, 3, axe2e, 30, 2) < 0 && HarvestRules.Compare(axe2e, 30, axe2, 3, 2) > 0 && HarvestRules.Compare(new ToolSpec(ToolFamily.Axe, 1, false), 2, axe2e, 40, 1) < 0,
                "Tool choice uses the tool with the fewest remaining uses first, whatever its tier or variant");
            Check(HarvestRules.Compare(axe2e, 10, axe2, 10, 2) < 0 && HarvestRules.Compare(new ToolSpec(ToolFamily.Axe, 3, false), 10, axe2, 10, 2) < 0 && HarvestRules.Compare(axe2, 10, new ToolSpec(ToolFamily.Axe, 3, false), 10, 1) < 0,
                "Equal remaining uses: fewer actions, then Elite, then the lower tier");

            int Uses(ToolFamily f, int t, bool e) => new ToolSpec(f, t, e).BaseUses;
            Check(Uses(ToolFamily.Axe, 1, false) == 10 && Uses(ToolFamily.Hunting, 1, false) == 6 && Uses(ToolFamily.Rod, 1, false) == 10
                && Uses(ToolFamily.Axe, 2, true) == 40 && Uses(ToolFamily.Hunting, 2, true) == 32 && Uses(ToolFamily.Hunting, 4, false) == 36 && Uses(ToolFamily.Rod, 4, true) == 80,
                "Tool baselines: axe/rod 10 + 10 (T - 1), slingshot 6 + 10 (T - 1), Elite double");

            bool tools = HarvestTools.TryGet(PrototypeItems.Tool, out ToolSpec pineAxe) && pineAxe.Family == ToolFamily.Axe && pineAxe.Tier == 1 && !pineAxe.Elite;
            foreach (ToolFamily family in new[] { ToolFamily.Axe, ToolFamily.Hunting, ToolFamily.Rod })
                for (int tier = 1; tier <= 4; tier++)
                    foreach (bool elite in tier == 1 ? new[] { false } : new[] { false, true })
                    {
                        ItemDefinition definition = HarvestTools.Definition(family, tier, elite);
                        tools &= definition != null && HarvestTools.TryGet(definition, out ToolSpec spec) && spec.Family == family && spec.Tier == tier && spec.Elite == elite && !definition.Stackable;
                    }
            Check(tools && PrototypeItems.DebugHarvestTools.Length == 18, "Pine tools and the 18 debug tier tools resolve to their family, tier and variant");

            // Manifests: wood plus the mandatory food ingredient, authored Normal/Elite ranges, deterministic.
            int Count(IReadOnlyList<HarvestBundle> m, string id) => m.Count(b => b.ItemId == id);
            bool treeNormal = true, treeElite = true, rabbit = true, fish = true, qualities = true;
            for (int i = 0; i < 40; i++)
            {
                var n = HarvestManifest.Roll(InteractableKind.Tree, 1, "segment.01.01", 1 + i % 3, "tree" + i, false);
                treeNormal &= Count(n, "material.pine_branch") is int b && b >= 2 && b <= 5 && Count(n, "material.pine_log") is int l && l >= 2 && l <= 5 && Count(n, "ingredient.pine_nuts") is int nuts && nuts >= 2 && nuts <= 4;
                var e = HarvestManifest.Roll(InteractableKind.Tree, 1, "segment.01.01", 1, "tree" + i, true);
                treeElite &= Count(e, "material.pine_branch") is int eb && eb >= 4 && eb <= 10 && Count(e, "ingredient.pine_nuts") is int en && en >= 4 && en <= 6;
                var r = HarvestManifest.Roll(InteractableKind.AnimalSpawn, 1, "segment.01.01", 1, "rabbit" + i, false);
                rabbit &= new[] { "animal.rabbit.pelt", "animal.rabbit.meat", "material.bone" }.All(id => Count(r, id) >= 2 && Count(r, id) <= 5) && Count(r, "ingredient.animal_fat") is int fat && fat >= 1 && fat <= 3;
                var f = HarvestManifest.Roll(InteractableKind.FishingPoint, 1, "segment.01.01", 1, "pond" + i, false);
                var fe = HarvestManifest.Roll(InteractableKind.FishingPoint, 2, "segment.02.01", 1, "pond" + i, true);
                fish &= f.Count >= 2 && f.Count <= 5 && f.All(x => x.ItemId == "fish.rainbow_trout" || x.ItemId == "fish.bluegill")
                     && fe.Count >= 4 && fe.Count <= 10 && fe.All(x => x.ItemId == "fish.largemouth_bass" || x.ItemId == "fish.yellow_perch");
                qualities &= n.Concat(e).Concat(r).Concat(f).All(x => x.Quality >= 1 && x.Quality <= 5 && Array.Exists(PrototypeItems.All, d => d.Id == x.ItemId));
            }
            Check(treeNormal && treeElite, "Trees always yield branches, logs and pine nuts within the Normal (2-5/2-5/2-4) and Elite (4-10/4-10/4-6) ranges");
            Check(rabbit, "Rabbits yield pelt, meat and bone (2-5 each) and fat (1-3)");
            Check(fish, "Fishing catches 2-5 bundles (Elite 4-10) only from the region's fish pair");
            Check(qualities, "Every rolled bundle is a known item at Q1-Q5");
            var a1 = HarvestManifest.Roll(InteractableKind.Tree, 1, "segment.01.01", 2, "segment.01.01.tree.3", true);
            var a2 = HarvestManifest.Roll(InteractableKind.Tree, 1, "segment.01.01", 2, "segment.01.01.tree.3", true);
            Check(a1.Count == a2.Count && a1.Zip(a2, (x, y) => x.ItemId == y.ItemId && x.Quality == y.Quality).All(v => v), "The same interactable and cycle always roll the same manifest (no reroll on reload)");

            // Tool wear: one use per committed action; the tool breaks and leaves the inventory at zero.
            var ledger = new SpatialInventory(8000, 4000);
            ItemEntry axe = ledger.AddWorld("axe", PrototypeItems.Tool, 1, 3, new ItemProfile(100, 10, 2));
            ledger.Reserve(axe, 1); bool stored = ledger.TryQuickStore(axe, 1, out _); ledger.Release(axe, 1);
            bool first = ledger.TryUseTool(axe, out bool broke1);
            bool second = ledger.TryUseTool(axe, out bool broke2);
            Check(stored && first && !broke1 && second && broke2 && axe.Owner == ItemOwner.Consumed && !ledger.TryUseTool(axe, out _),
                "Each action spends exactly one use; the tool breaks at zero after that action and cannot be used again");
            ItemEntry spare = ledger.AddWorld("spare-axe", PrototypeItems.Tool, 1, 3, new ItemProfile(100, 10, 10));
            Check(!ledger.TryUseTool(spare, out _), "A tool lying on the trail (not carried) cannot be used");
            ledger.Reserve(spare, 2); ledger.TryQuickStore(spare, 2, out _);
            Check(!ledger.TryUseTool(spare, out _), "A tool held by a drag gesture cannot be used");
            ledger.Release(spare, 2);
            Check(ledger.TryUseTool(spare, out _) && spare.Profile.Durability == 9 && spare.Profile.MaxDurability == 10, "Wear keeps the maximum and lowers the remaining uses");

            // Hooks hold any tool, one per hook (user, October 31, 2026).
            var hooks = new SpatialInventory(8000, 4000); hooks.Reconfigure(8000, 4000, 2, 3, out _);
            ItemEntry Stored(string id, ItemDefinition definition, ItemProfile profile)
            {
                ItemEntry e = hooks.AddWorld(id, definition, 1, 1, profile); hooks.Reserve(e, 7); hooks.TryQuickStore(e, 7, out _); hooks.Release(e, 7); return e;
            }
            ItemDefinition sling = HarvestTools.Definition(ToolFamily.Hunting, 1, false), rod = HarvestTools.Definition(ToolFamily.Rod, 1, false), eliteAxe = HarvestTools.Definition(ToolFamily.Axe, 3, true);
            bool OnHook(ItemEntry e, int index) { hooks.Reserve(e, 8); bool ok = hooks.TryMoveToSide(e, 8, true, index, out _); hooks.Release(e, 8); return ok; }
            ItemEntry s1 = Stored("sling", sling, new ItemProfile(100, 6, 6)), r1 = Stored("rod", rod, new ItemProfile(100, 10, 10)), axeHook = Stored("elite-axe", eliteAxe, new ItemProfile(100, 60, 60));
            Check(OnHook(s1, 0) && OnHook(r1, 1) && OnHook(axeHook, 2) && s1.Owner == ItemOwner.Hook && r1.Owner == ItemOwner.Hook, "Every tool family (slingshot, rod, axe, debug tiers) can hang on a hook");
            ItemEntry r2 = Stored("rod-2", rod, new ItemProfile(100, 10, 10));
            Check(!OnHook(r2, 1), "One tool per hook: an occupied hook refuses a second tool");
            ItemEntry pelt = hooks.AddWorld("pelt", PrototypeItems.RabbitPelt, 10); hooks.Reserve(pelt, 9); hooks.TryQuickStore(pelt, 9, out _); hooks.Release(pelt, 9);
            Check(!OnHook(pelt, 1) && hooks.TryUseTool(s1, out _), "Stacks cannot hang on hooks; a tool on a hook can still be used for harvesting");

            // Harvest state persists per drop cycle; malformed entries are rejected.
            string key = HarvestManifest.HarvestKey("segment.01.01", 2, "segment.01.01.tree.3");
            Check(key == "segment.01.01|2|segment.01.01.tree.3" && HarvestManifest.BundleId("segment.01.01", 2, "segment.01.01.tree.3", 4) == "harvest-segment.01.01-c2-segment.01.01.tree.3-b4",
                "Harvest keys and bundle IDs carry segment, cycle and anchor");
            var snapshot = new SessionSnapshot { version = 1, harvested = new List<string> { "bad-entry" } };
            bool rejected; try { SessionSaveStore.Validate(snapshot, float.MaxValue, out _); rejected = false; } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "A malformed harvested entry makes the save invalid");

            AnimalMotionChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Milestone6-checks.txt", passed);
            Debug.Log($"[Milestone6] PASS: {passed.Count} harvest checks.");
        }
    }
}
