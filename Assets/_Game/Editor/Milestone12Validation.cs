using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Persistence;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Milestone 12: the settlement warehouse. Each task adds a group of checks called from Run().
    public static class Milestone12Validation
    {
        private static readonly List<string> passed = new List<string>();
        private static int escrowCounter;
        private static readonly DateTime Now = new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

        [MenuItem("The Trail Walker/Milestone 12/Validate Warehouse")]
        public static void Run()
        {
            passed.Clear();
            DomainChecks();
            SaveChecks();
            ArtChecks();
            ViewChecks();
            HoldChecks();
            Directory.CreateDirectory("Builds/Validation"); File.WriteAllLines("Builds/Validation/Warehouse-checks.txt", passed);
            Debug.Log($"Milestone 12: {passed.Count} checks passed.");
        }

        private static void Check(bool ok, string name) { if (!ok) throw new Exception("Milestone 12: " + name); passed.Add(name); }
        private static ItemDefinition Def(string id) { return TheTrailWalker.Domain.Crafting.Items.Find(id); }
        private static SpatialInventory Bag()
        {
            var l = new SpatialInventory(8000, 4000);
            if (!l.Reconfigure(8000, 4000, 2, 2, out _)) throw new InvalidOperationException("Test fixture: pockets");
            return l;
        }
        private static string NewId() { return "wh-escrow-" + (++escrowCounter); }
        private static ItemEntry Put(SpatialInventory l, string id, string item, int quantity, int quality = 1, ItemOwner owner = ItemOwner.Backpack, bool ready = true, ItemProfile? profile = null, int x = 0)
        {
            ItemEntry e = l.AddWorld(id, Def(item), quantity, quality, profile ?? ItemPrices.Profile(Def(item), quality), ready);
            bool placed = l.RestoreStored(e, owner, x, 0);
            for (int step = 1; !placed && owner == ItemOwner.Backpack && step < 12; step++) placed = l.RestoreStored(e, owner, step * 600, 0);   // first free spot
            if (!placed) throw new InvalidOperationException("Test fixture: " + id);
            return e;
        }
        private static bool Dep(SpatialInventory l, WarehouseState s, Wallet w, List<TradeRecord> log, ItemEntry item, int pointer, int crate, out WarehouseDeposit r, out string reason)
        { return WarehouseService.Deposit(l, s, w, log, item, pointer, crate, NewId, Now, out r, out reason); }

        private static void ArtChecks()
        {
            const string Dir = "Settlement/Storefronts/";
            var crate = Resources.Load<GameObject>(Dir + "DepositCrate");
            Check(crate != null, "the deposit crate prefab loads");
            var body = crate.transform.Find("Body"); var lid = crate.transform.Find("Lid");
            Check(body != null && lid != null, "the deposit crate has Body and Lid");
            var bodyMesh = body.GetComponentInChildren<MeshFilter>()?.sharedMesh; var lidMesh = lid.GetComponentInChildren<MeshFilter>()?.sharedMesh;
            Check(bodyMesh != null && lidMesh != null, "Body and Lid both carry a mesh");
            var top = new Vector3(0, bodyMesh.bounds.max.y, bodyMesh.bounds.min.z);
            Check(Mathf.Abs(lid.localPosition.y - top.y) < .01f && Mathf.Abs(lid.localPosition.z - top.z) < .01f && Mathf.Abs(lid.localPosition.x) < .01f, "the lid pivot sits on the body's top-back edge");
            Check(crate.GetComponentsInChildren<Collider>(true).Length == 0 && crate.GetComponentsInChildren<MonoBehaviour>(true).Length == 0, "the deposit crate has no colliders or scripts");
            var size = bodyMesh.bounds.size;
            Check(Mathf.Abs(size.x - .46f) <= .05f && Mathf.Abs(size.y - .34f) <= .05f && Mathf.Abs(size.z - .36f) <= .05f, "the crate body is about the old crate size");
            Check(Quaternion.Angle(lid.localRotation, Quaternion.Euler(-100, 0, 0)) < .5f, "the lid rests open at -100 degrees");
            var counter = Resources.Load<GameObject>(Dir + "WarehouseCounter");
            Check(counter.transform.Find("OfferAnchor1") && counter.transform.Find("OfferAnchor2") && counter.transform.Find("CustomerCameraAnchor"), "the warehouse counter keeps its anchors");
            int tris = counter.transform.Find("WarehouseCounter").GetComponentInChildren<MeshFilter>().sharedMesh.triangles.Length / 3;
            Check(tris <= 1792 - 72, "the warehouse counter no longer draws the two crates (" + tris + " triangles, was 1792)");
        }

        private static void ViewChecks()
        {
            const string Pine = "material.pine_log";
            var three = new WarehouseCrate { ItemId = Pine }; three.Units.Add(new WarehouseUnit { EscrowId = "v1", Quantity = 3, ValueMinor = 300 });
            Check(CollectionPrototype.WarehouseCrateLabel(three) == "Pine log 3/10", "a crate with three pine logs reads \"Pine log 3/10\" (" + CollectionPrototype.WarehouseCrateLabel(three) + ")");
            Check(CollectionPrototype.WarehouseCrateLabel(new WarehouseCrate()) == "Empty", "an empty crate reads \"Empty\"");
            Check(CollectionPrototype.WarehouseCycleLine(new WarehouseState { CycleCrates = 4, CycleCash = 3200 }) == "Crate 4 of 10 · 3,200 cash this cycle", "the cycle line reads \"Crate 4 of 10 · 3,200 cash this cycle\"");
            Check(CollectionPrototype.WarehouseCycleLine(new WarehouseState()) == "Crate 0 of 10 · 0 cash this cycle", "a fresh cycle reads crate 0 of 10");
            Check(CollectionPrototype.SettlementNote(new TheTrailWalker.Travel.StreetBuilding { Id = "warehouse", Kind = TheTrailWalker.Travel.StreetBuildingKind.Warehouse }, 1, false).IndexOf("Milestone 12", StringComparison.Ordinal) < 0, "the warehouse note no longer says Milestone 12");

            Check(CollectionPrototype.SavedBagOpen(true, false, true) == false && CollectionPrototype.SavedBagOpen(true, true, true) && CollectionPrototype.SavedBagOpen(false, true, false) == false && CollectionPrototype.SavedBagOpen(false, false, true), "a save made in the counter view records the bag state from before entering");
            Check(CollectionPrototype.WarehouseKeepsBagOnPause(true) && !CollectionPrototype.WarehouseKeepsBagOnPause(false), "an app pause keeps the bag open only while the warehouse counter is open");
            foreach (Vector2 screen in new[] { new Vector2(1116, 2480), new Vector2(720, 1680) })
            {
                var counter = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Settlement/Storefronts/WarehouseCounter"));
                var cam = new GameObject("view check camera").AddComponent<Camera>();
                var crates = new List<GameObject>();
                try
                {
                    var bounds = new Bounds[2];
                    for (int i = 0; i < 2; i++)
                    {
                        var crate = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Settlement/Storefronts/DepositCrate"));
                        crate.transform.position = counter.transform.Find("OfferAnchor" + (i + 1)).position;
                        crates.Add(crate);
                        bounds[i] = new Bounds(crate.transform.position, Vector3.zero);
                        foreach (Renderer r in crate.GetComponentsInChildren<Renderer>()) bounds[i].Encapsulate(r.bounds);
                    }
                    Transform anchor = counter.transform.Find("CustomerCameraAnchor");
                    cam.fieldOfView = TheTrailWalker.Travel.StreetCamera.FieldOfView; cam.aspect = screen.x / screen.y;
                    cam.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
                    Pose pose = CollectionPrototype.WarehouseCameraPose(cam, bounds, 0.3f);
                    Check(cam.transform.position == anchor.position && cam.transform.rotation == anchor.rotation, "the camera pose call leaves the camera where it was");
                    cam.transform.SetPositionAndRotation(pose.position, pose.rotation);
                    float dolly = Vector3.Distance(pose.position, anchor.position);
                    string where = " at " + screen.x + "x" + screen.y + " (dolly " + dolly.ToString("0.00") + " m)";
                    Check(dolly < 29.9f, "the framing is found within the dolly cap" + where);
                    Renderer head = counter.transform.Find("TraderHead") != null ? counter.transform.Find("TraderHead").GetComponentInChildren<Renderer>() : counter.GetComponentsInChildren<Renderer>().First(r => r.name == "default" && r.transform.parent.name == "TraderHead");
                    float headTop = cam.WorldToViewportPoint(new Vector3(head.bounds.center.x, head.bounds.max.y, head.bounds.center.z)).y;
                    Check(headTop < 1f && headTop > 0.3f, "the keeper's head stays in view" + where + ": y " + headTop.ToString("0.000"));
                    string longest = PrototypeItems.All.Concat(HarvestTierItems.All).Select(d => d.Name).OrderByDescending(n => n.Length).First() + " 10/10";
                    for (int j = 0; j < 2; j++)
                    {
                        float cx = cam.WorldToViewportPoint(new Vector3(bounds[j].center.x, bounds[j].max.y, bounds[j].center.z)).x;
                        float w = CollectionPrototype.WarehouseLabelWidth(longest), left = CollectionPrototype.WarehouseLabelLeft(cx, w, 0f, 1f);
                        Check(left >= 0f && left + w <= 1f, "the label \"" + longest + "\" over crate " + (j + 1) + " stays inside the screen horizontally" + where + ": " + left.ToString("0.000") + " + " + w.ToString("0.000"));
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        float x0 = 9, x1 = -9, y0 = 9;
                        for (int c = 0; c < 8; c++)
                        {
                            Vector3 v = cam.WorldToViewportPoint(bounds[i].center + Vector3.Scale(bounds[i].extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1)));
                            x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y);
                        }
                        Check(y0 > 0.36f, "crate " + (i + 1) + " clears the bag with its label margin" + where + ": yMin " + y0.ToString("0.000"));
                        Check(x0 >= 0f && x1 <= 1f, "crate " + (i + 1) + " is inside the screen horizontally" + where);
                    }
                }
                finally
                {
                    foreach (GameObject g in crates) UnityEngine.Object.DestroyImmediate(g);
                    UnityEngine.Object.DestroyImmediate(cam.gameObject); UnityEngine.Object.DestroyImmediate(counter);
                }
            }
        }

        private static void DomainChecks()
        {
            const string Pine = "material.pine_log";
            ProfileFor(out ItemProfile rich);

            // 990: a mixed 1,000 stack gives its ten lowest-quality units, locks their value and completes the crate.
            {
                var bag = Bag(); var state = new WarehouseState(); var wallet = new Wallet(); var log = new List<TradeRecord>();
                ItemEntry stack = bag.AddWorld("logs", Def(Pine), 1000, 1, ItemPrices.Profile(Def(Pine), 1));
                bag.RestorePile(stack, Enumerable.Range(1, 5).Select(q => new PilePortion(q, ItemPrices.Profile(Def(Pine), q), 200)).ToList());
                if (!bag.RestoreStored(stack, ItemOwner.Backpack, 0, 0)) throw new InvalidOperationException("Test fixture: stack");
                long expected = SaleQuote.LowestMinor(stack, 10);
                bool ok = Dep(bag, state, wallet, log, stack, -1, 0, out WarehouseDeposit r, out string why);
                Check(ok && r.Units == 10 && stack.Quantity == 990 && r.ValueMinor == expected && r.CrateCompleted, "a 1,000 stack deposits ten units and completes the crate");
                Check(r.CashPaid == Money.Payout(expected, 100) && wallet.Cash == r.CashPaid && state.CycleCrates == 1 && state.Crates[0].IsEmpty && state.CycleCash == r.CashPaid, "the crate pays its locked value once and empties");
            }

            // Partial: 3 + 4 leave 7 with no pay, then 5 moves only the 3 that fit.
            {
                var bag = Bag(); var state = new WarehouseState(); var wallet = new Wallet(); var log = new List<TradeRecord>();
                ItemEntry a = Put(bag, "a", Pine, 3), b = Put(bag, "b", Pine, 4, 1, ItemOwner.Pocket), c = Put(bag, "c", Pine, 5, 1, ItemOwner.Pocket, true, null, 1);
                Dep(bag, state, wallet, log, a, -1, 1, out _, out _); Dep(bag, state, wallet, log, b, -1, 1, out _, out _);
                Check(state.Crates[1].Count == 7 && state.Crates[1].ItemId == Pine && wallet.Cash == 0 && state.CycleCrates == 0, "partial deposits fill a crate without paying");
                bool ok = Dep(bag, state, wallet, log, c, -1, 1, out WarehouseDeposit r, out _);
                Check(ok && r.Units == 3 && c.Quantity == 2 && r.CrateCompleted && wallet.Cash > 0, "a stack moves only what fits and completes the crate");
            }

            // Mismatch and not-ready change nothing.
            {
                var bag = Bag(); var state = new WarehouseState(); var wallet = new Wallet(); var log = new List<TradeRecord>();
                Dep(bag, state, wallet, log, Put(bag, "p", Pine, 2), -1, 0, out _, out _);
                ItemEntry sling = Put(bag, "s", "item.pine_slingshot_normal", 1, 1, ItemOwner.Backpack);
                int rev = bag.Revision, logCount = log.Count;
                bool ok = Dep(bag, state, wallet, log, sling, -1, 0, out _, out string why);
                Check(!ok && why == "This crate takes Pine log" && bag.Revision == rev && wallet.Cash == 0 && state.Crates[0].Count == 2 && log.Count == logCount, "a different item is refused and nothing changes");
                ItemEntry pending = Put(bag, "pend", Pine, 5, 1, ItemOwner.Backpack, false); rev = bag.Revision;
                ok = Dep(bag, state, wallet, log, pending, -1, 0, out _, out why);
                Check(!ok && why == "Not ready yet" && bag.Revision == rev && state.Crates[0].Count == 2, "a pending craft is refused");
            }

            // Used tool: one unit at half its value, keeping its id in escrow.
            {
                var bag = Bag(); var state = new WarehouseState(); var wallet = new Wallet(); var log = new List<TradeRecord>();
                ItemEntry axe = Put(bag, "axe1", "tool.pine_axe.normal", 1, 1, ItemOwner.Backpack, true, new ItemProfile(1000, 100, 50));
                long half = SaleQuote.TotalMinor(axe);
                bool ok = Dep(bag, state, wallet, log, axe, -1, 0, out WarehouseDeposit r, out _);
                Check(ok && r.Units == 1 && r.ValueMinor == half && half == 500 && axe.Owner == ItemOwner.Escrow && state.Crates[0].Units[0].EscrowId == "axe1" && state.Crates[0].Count == 1, "a used tool counts one unit at its remaining condition and keeps its id");
            }

            // Gold: the tenth crate of a cycle pays cycleCash / 1000 gold and resets the cycle.
            {
                var bag = Bag(); var state = new WarehouseState { CycleCrates = 9, CycleCash = 11500 }; var wallet = new Wallet(); var log = new List<TradeRecord>();
                ItemEntry stack = Put(bag, "rich", Pine, 10, 1, ItemOwner.Backpack, true, rich);
                bool ok = Dep(bag, state, wallet, log, stack, -1, 0, out WarehouseDeposit r, out _);
                Check(ok && r.CashPaid == 1250 && r.GoldPaid == 12 && wallet.Gold == 12 && wallet.Cash == 1250 && state.CycleCrates == 0 && state.CycleCash == 0, "the tenth crate pays 12 gold and resets the cycle");
                Check(log.Select(x => x.result).SequenceEqual(new[] { "WarehouseDeposit", "WarehouseCrate", "WarehouseGold" }), "a gold deposit logs deposit, crate and gold in order");
                TradeRecord goldRec = log.Last(); Check(goldRec.cash == 0 && goldRec.quantity == 12, "the gold record carries the gold in quantity and no cash");
            }

            // Held stacks keep their remainder reserved; a fully consumed one ends unreserved.
            {
                var bag = Bag(); var state = new WarehouseState(); var wallet = new Wallet(); var log = new List<TradeRecord>();
                ItemEntry big = Put(bag, "big", Pine, 20); bag.Reserve(big, 7);
                bool ok = Dep(bag, state, wallet, log, big, 7, 0, out WarehouseDeposit r, out _);
                Check(ok && r.Units == 10 && big.Quantity == 10 && bag.ReservedBy(big, 7), "a held stack deposits and its remainder stays held");
                ItemEntry six = Put(bag, "six", Pine, 6); bag.Reserve(six, 8);
                ok = Dep(bag, state, wallet, log, six, 8, 1, out _, out _);
                Check(ok && six.Owner == ItemOwner.Consumed && !bag.ReservedBy(six, 8), "a fully deposited held stack ends consumed and unreserved");
            }

            // Overflow refuses with no change.
            {
                var bag = Bag(); var state = new WarehouseState(); var wallet = new Wallet(long.MaxValue - 1); var log = new List<TradeRecord>();
                ItemEntry stack = Put(bag, "rich", Pine, 10, 1, ItemOwner.Backpack, true, rich); int rev = bag.Revision;
                bool ok = Dep(bag, state, wallet, log, stack, -1, 0, out _, out _);
                Check(!ok && bag.Revision == rev && stack.Quantity == 10 && wallet.Cash == long.MaxValue - 1 && state.Crates[0].IsEmpty && state.CycleCrates == 0 && log.Count == 0, "a wallet overflow refuses the deposit with no change");
            }
        }

        private static SessionSnapshot Snap(SpatialInventory l, WarehouseState w)
        {
            var s = new SessionSnapshot { version = 2, speedCap = 1.5f, survival = new SavedSurvival(), economy = 1, warehouse = w == null ? null : SessionSaveStore.SaveWarehouse(w) };
            foreach (ItemEntry e in l.Entries)
            {
                if (e.Owner == ItemOwner.Consumed) continue;
                s.items.Add(new SavedItem { id = e.Id, definitionId = e.Definition.Id, owner = e.Owner.ToString(), quantity = e.Quantity, quality = e.Quality, ready = e.Ready, x = e.X, y = e.Y,
                    unitValue = e.Profile.UnitValue, maxDurability = e.Profile.MaxDurability, durability = e.Profile.Durability, variant = e.Profile.Variant,
                    pile = e.Pile.Select(p => new SavedPortion { quality = p.Quality, quantity = p.Quantity, unitValue = p.Profile.UnitValue, maxDurability = p.Profile.MaxDurability, durability = p.Profile.Durability, variant = p.Profile.Variant }).ToList() });
            }
            return s;
        }
        private static SessionSnapshot Clone(SessionSnapshot s) { return JsonUtility.FromJson<SessionSnapshot>(JsonUtility.ToJson(s)); }
        private static string Text(WarehouseState w)
        {
            return w.CycleCrates + "/" + w.CycleCash + ";" + string.Join("|", w.Crates.Select(c => c.ItemId + ":" + string.Join(",", c.Units.Select(u => u.EscrowId + "x" + u.Quantity + "@" + u.ValueMinor))));
        }
        private static bool Refused(SessionSnapshot s)
        {
            try { SessionSaveStore.Validate(Clone(s), float.MaxValue, out _); return false; } catch (InvalidDataException) { return true; }
        }

        private static void SaveChecks()
        {
            const string Pine = "material.pine_log", Axe = "tool.pine_axe.normal";
            var bag = Bag(); var state = new WarehouseState(); var wallet = new Wallet(); var log = new List<TradeRecord>();
            Dep(bag, state, wallet, log, Put(bag, "logs", Pine, 7), -1, 0, out _, out _);
            Dep(bag, state, wallet, log, Put(bag, "axe", Axe, 1), -1, 1, out _, out _);
            state.CycleCrates = 4; state.CycleCash = 3200;
            SessionSnapshot good = Snap(bag, state); string crate0Id = state.Crates[0].Units[0].EscrowId;

            SessionSnapshot back = Clone(good);
            SessionSaveStore.Validate(back, float.MaxValue, out SpatialInventory restored);
            WarehouseState again = SessionSaveStore.RestoreWarehouse(back, restored.Entries);
            Check(Text(again) == Text(state) && again.Crates[0].Count == 7 && again.Crates[1].Count == 1 && again.CycleCrates == 4 && again.CycleCash == 3200, "a warehouse saves and reloads with ids, quantities, values and cycle");

            SessionSnapshot old = Clone(good); old.warehouse = null; old.items.RemoveAll(i => i.owner == nameof(ItemOwner.Escrow));
            SessionSaveStore.Validate(old, float.MaxValue, out SpatialInventory oldLedger);
            WarehouseState fresh = SessionSaveStore.RestoreWarehouse(old, oldLedger.Entries);
            Check(fresh.Crates.All(c => c.IsEmpty && c.ItemId == null) && fresh.CycleCrates == 0 && fresh.CycleCash == 0, "a save without a warehouse restores empty crates and a fresh cycle");

            SessionSnapshot s = Clone(good); s.items.RemoveAll(i => i.id == crate0Id);
            Check(Refused(s), "a unit without its escrow entry is refused");
            s = Clone(good); s.warehouse.crates[0].itemId = Axe;
            Check(Refused(s), "an escrow entry of another item is refused");
            s = Clone(good); s.home = new SavedHome { block = 1, upgradeEscrow = new List<string> { crate0Id } };
            bool shared = false; try { SessionSaveStore.RestoreWarehouse(s, restored.Entries); } catch (InvalidDataException) { shared = true; }
            Check(shared, "an escrow id shared with the house upgrade is refused");
            s = Clone(good); s.warehouse.crates[1].units[0].escrowId = crate0Id;
            Check(Refused(s), "an escrow id repeated across crates is refused");
            s = Clone(good); s.warehouse.cycleCrates = 10;
            Check(Refused(s), "cycleCrates 10 is refused");
            s = Clone(good); s.warehouse.cycleCash = -1;
            Check(Refused(s), "negative cycleCash is refused");
            s = Clone(good); s.warehouse.crates[0].units[0].valueMinor = -1;
            Check(Refused(s), "a negative unit value is refused");
            s = Clone(good); s.warehouse.crates.RemoveAt(1);
            Check(Refused(s), "one crate only is refused");
            {
                var full = Bag(); ItemEntry ten = Put(full, "ten", Pine, 10);
                full.TryEscrow(ten, 10, "wh-ten", out _, out _);
                var tenState = new WarehouseState(); tenState.Crates[0].ItemId = Pine; tenState.Crates[0].Units.Add(new WarehouseUnit { EscrowId = "wh-ten", Quantity = 10, ValueMinor = 100 });
                Check(Refused(Snap(full, tenState)), "a crate holding 10 is refused");
            }

            // 25 units in four steps, reloading after each: nothing is lost or paid twice.
            {
                var b = Bag(); var st = new WarehouseState(); var w = new Wallet(); var lg = new List<TradeRecord>();
                var steps = new[] { 7, 6, 6, 6 }.Select((n, i) => Put(b, "m" + i, Pine, n)).ToList();   // the default gear has no pockets, so all four stay in the backpack
                for (int i = 0; i < steps.Count; i++)
                {
                    Dep(b, st, w, lg, steps[i], -1, i % 2, out _, out _);
                    SessionSnapshot step = Clone(Snap(b, st));
                    SessionSaveStore.Validate(step, float.MaxValue, out SpatialInventory l2);
                    WarehouseState s2 = SessionSaveStore.RestoreWarehouse(step, l2.Entries);
                    int inBag = l2.Entries.Where(e => e.Definition.Id == Pine && (e.Owner == ItemOwner.Backpack || e.Owner == ItemOwner.Pocket || e.Owner == ItemOwner.Hook || e.Owner == ItemOwner.Overflow)).Sum(e => e.Quantity);
                    int total = inBag + s2.Crates.Sum(c => c.Count) + s2.CycleCrates * WarehouseState.CrateUnits;
                    Check(Text(s2) == Text(st) && total == 25 && s2.CycleCash == w.Cash, "step " + (i + 1) + " reloads with 25 units accounted for and no double payout");
                }
            }
        }

        // Task 5: CrateHold driven frame by frame (60 fps), no scene. Every index it returns counts as a deposit here.
        private static void HoldChecks()
        {
            const float Dt = 1f / 60f;
            int Frames(CrateHold h, float seconds, int crate, bool a0 = false, bool a1 = false, bool live = true, List<int> into = null)
            {
                int n = 0;
                for (float s = 0f; s < seconds; s += Dt) { int c = h.Tick(crate, a0, a1, Dt, live); if (c >= 0) { n++; h.Deposited = true; into?.Add(c); } }
                return n;
            }
            var pass = new CrateHold(true);
            Check(Frames(pass, 0.25f, 0) + Frames(pass, 1f, -1) == 0 && pass.Release(-1, false, false, true) < 0, "pass-over: 0.25 s over crate 0, then off, deposits nothing");

            var hold = new CrateHold(true); var into = new List<int>();
            Check(Frames(hold, 0.31f, 0, into: into) == 1 && into[0] == 0, "rest then fill: 0.31 s over crate 0 deposits once into crate 0");
            Check(Frames(hold, CrateHold.AnimationSeconds, 0, a0: true) == 0, "still over crate 0 while it animates: nothing");
            Check(hold.Tick(0, false, false, Dt, true) == 0, "the frame crate 0's animation ends deposits into it again");

            var alt = new CrateHold(true);
            Frames(alt, 0.31f, 0);
            Check(alt.Tick(1, true, false, Dt, true) == 1, "alternating: after the first deposit, entering crate 1 (not animating) deposits on the same frame");
            Check(Frames(alt, 0.65f, 1, true, true, false) == 0 && alt.Tick(1, false, false, Dt, false) < 0 && alt.Tick(0, false, false, Dt, false) < 0 && alt.Release(0, false, false, false) < 0,
                "stack gone: with the stack no longer live, nothing more is deposited");
            Check(alt.Release(1, false, false, true) < 0, "a release after the drag deposited does not deposit again");

            var tool = new CrateHold(false);
            Check(Frames(tool, 2f, 0) == 0 && tool.Release(0, false, false, true) == 0, "non-stackable: held over a crate for 2 s nothing, one deposit on release");
            var drop = new CrateHold(true);
            Check(Frames(drop, 0.1f, 1) == 0 && drop.Release(1, false, false, true) == 1 && new CrateHold(true).Release(0, true, false, true) < 0, "a quick drop deposits on release; never into an animating crate");

            // 1,000 stack swung between the crates every 0.35 s; a completed crate animates for 0.65 s.
            var run = new CrateHold(true); int stack = 1000, crates = 0, maxAnimating = 0; float t = 0f; bool intoAnimating = false;
            var count = new int[2]; var anim = new float[2];
            for (; t < 40f && stack > 0; t += Dt)
            {
                for (int i = 0; i < 2; i++) anim[i] = Mathf.Max(0f, anim[i] - Dt);
                maxAnimating = Mathf.Max(maxAnimating, (anim[0] > 0f ? 1 : 0) + (anim[1] > 0f ? 1 : 0));
                int c = run.Tick((int)(t / 0.35f) % 2, anim[0] > 0f, anim[1] > 0f, Dt, stack > 0);
                if (c < 0) continue;
                intoAnimating |= anim[c] > 0f;
                int units = Math.Min(stack, WarehouseState.CrateUnits - count[c]); stack -= units; count[c] += units; run.Deposited = true;
                if (count[c] == WarehouseState.CrateUnits) { count[c] = 0; anim[c] = CrateHold.AnimationSeconds; crates++; }
            }
            Check(!intoAnimating, "the run never deposits into an animating crate");
            Check(stack == 0 && crates == 100 && maxAnimating <= 2, "full run: a 1,000 stack swung every 0.35 s empties into 100 crates in " + t.ToString("0.0") + " s (limit 40), at most " + maxAnimating + " animating");

            Check(CollectionPrototype.WarehousePayoutLine(new WarehouseDeposit { CashPaid = 1240 }) == "+1,240 cash", "payout feedback reads +1,240 cash");
            Check(CollectionPrototype.WarehousePayoutLine(new WarehouseDeposit { CashPaid = 1240, GoldPaid = 12 }) == "+1,240 cash   +12 gold", "a gold payout reads +1,240 cash then +12 gold");
        }

        private static void ProfileFor(out ItemProfile rich) { rich = new ItemProfile(12500); }   // 10 units = 125,000 minor = 1,250 cash
    }
}
