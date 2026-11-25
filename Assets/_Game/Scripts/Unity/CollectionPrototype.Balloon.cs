using System.Collections;
using System.Collections.Generic;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 14 slice 3 (spec 2026-11-20 section 6.3): the balloon merchant. Seated at a host camp, a tap on the balloon builds its
    // interior 200 m down (like the shop counters) and frames it from CustomerCameraAnchor; the session's three gold offers stand on
    // OfferAnchor1-3 with the shop counters' tags, purchase sheet and wallet labels (the shop HUD is reused: the balloon only opens at
    // camps, the shops only in the settlement). A bought box plays its open animation on the counter, then the reward sheet claims what
    // fits. Waiting rewards stay in the saved box and reopen from the MENU's Boxes entry anywhere (Decision 1). Not a HomeOpen view:
    // the camp tears it down itself (UpdateBalloon), and ResetHomeView (travel, context changes) does too.
    public sealed partial class CollectionPrototype
    {
        public const float BalloonOfferMaxSide = 0.55f;   // m: the fit rule for every balloon offer (anchors 2 and 3 sit 0.75 m apart)
        public const string BalloonBoughtResult = "balloon.bought", BalloonLightName = "Lantern light";
        private const float BoxOpenSeconds = 1.1f;   // the Opening clip lasts 1 s
        // Holder-space centres of the two festoon paper lanterns nearest the offers (generate_balloon_interior.lanterns, x symmetric).
        private static readonly Vector3[] BalloonLanterns = { new Vector3(-1.6f, 2.72f, 0.4f), new Vector3(1.6f, 2.72f, 0.4f) };
        private Transform balloonHolder;
        private int balloonRegion;
        private RectTransform boxesMenuButton;
        private Text boxesMenuCount;
        private static BalloonGoodsArtCatalog balloonArt;
        private bool BalloonOpen => balloonHolder != null;

        // ---------------------------------------------------------------- pure rules (checked by Milestone14BalloonValidation)

        public static bool BalloonEntryAllowed(bool seated, string campId, bool busy) => seated && !busy && BalloonHosts.IsHost(campId);
        public static bool BalloonMustClose(bool seated, bool fainted, bool covered) => !seated || fainted || covered;

        public static string BalloonOfferName(BalloonOffer o)
        {
            if (o.Kind == "box") return "Mystery box";
            ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(o.ItemId);
            return def != null ? def.Name : o.ItemId;
        }
        // The price tag: <name> then <n> gold, or Sold out.
        public static string BalloonTagText(BalloonOffer o) => o.Sold ? ShopSoldOutText : BalloonOfferName(o) + "\n" + Money.Cash(o.GoldPrice) + " gold";

        // The offer's model on its anchor: special gear at its trail size, the box from the host region's art, then the one fit rule.
        public static GameObject BalloonOfferModel(Transform anchor, BalloonOffer o, int campRegion)
        {
            GameObject model;
            if (o.Kind == "box")
            {
                if (balloonArt == null) balloonArt = Resources.Load<BalloonGoodsArtCatalog>("BalloonGoods/BalloonGoodsArtCatalog");
                BalloonMysteryBoxArt art = balloonArt != null ? balloonArt.MysteryBoxForCampRegion(campRegion) : null;
                if (art == null || art.Prefab == null) return null;
                model = Instantiate(art.Prefab, anchor, false);
                model.name = "Offer mystery box";
                if (art.Material != null) foreach (Renderer r in model.GetComponentsInChildren<Renderer>()) r.sharedMaterial = art.Material;
                Animator animator = model.GetComponent<Animator>();
                if (animator != null) animator.updateMode = AnimatorUpdateMode.UnscaledTime;   // timed by a realtime wait
            }
            else
            {
                ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(o.ItemId);
                if (def == null) return null;
                model = PlaceOfferModel(anchor, def);
            }
            FitBalloonOffer(model.transform, anchor);
            return model;
        }

        // The fit rule: shrink (never grow) so the largest side of the visible renderers' bounds is at most BalloonOfferMaxSide, then
        // stand the model centred on the anchor.
        public static void FitBalloonOffer(Transform model, Transform anchor)
        {
            if (!VisibleBounds(model, out Bounds b)) return;
            float side = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (side > BalloonOfferMaxSide) { model.localScale *= BalloonOfferMaxSide / side; VisibleBounds(model, out b); }
            model.position += new Vector3(anchor.position.x - b.center.x, anchor.position.y - b.min.y, anchor.position.z - b.center.z);
        }
        private static bool VisibleBounds(Transform model, out Bounds b)
        {
            b = default; bool any = false;
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled) continue;   // PlaceOfferModel's probe hides its own empty renderer
                if (any) b.Encapsulate(r.bounds); else { b = r.bounds; any = true; }
            }
            return any;
        }

        // Ruling 8: no storefront lamps here. Two warm, shadowless, short-range point lights at the festoon paper lanterns, on the
        // counter-lamp budget, under the holder (gone with it).
        public static void HangLanternLights(Transform holder)
        {
            foreach (Vector3 at in BalloonLanterns)
            {
                var go = new GameObject(BalloonLightName);
                go.transform.SetParent(holder, false); go.transform.localPosition = at;
                Light light = go.AddComponent<Light>();
                light.type = LightType.Point; light.shadows = LightShadows.None; light.range = CounterLamps.Range; light.intensity = CounterLamps.Intensity; light.color = CounterLamps.Warm;
            }
        }

        // The PAY button's purchase (the shop counters' BuyAndRecord pattern): TryBuyGear or TryBuyBox, then one trade record with the
        // gold charged (cash 0). Everything stays in memory: the caller saves once, right after.
        public static bool BalloonBuyAndRecord(BalloonState s, int slot, int revision, Wallet wallet, SpatialInventory bag, MysteryBoxContext ctx, System.Func<string> newId, List<TradeRecord> log, System.DateTime utcNow, out BoxManifest box, out string reason)
        {
            box = null;
            bool isBox = s.Offers != null && slot >= 0 && slot < s.Offers.Length && s.Offers[slot].Kind == "box";
            if (isBox ? !BalloonShop.TryBuyBox(s, slot, revision, wallet, ctx, out box, out reason) : !BalloonShop.TryBuyGear(s, slot, revision, wallet, bag, newId, out reason)) return false;
            BalloonOffer o = s.Offers[slot];
            TradeLog.Add(log, new TradeRecord { utc = utcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture), campId = s.OpenCampId, settlementId = "balloon", itemId = isBox ? box.BoxId : o.ItemId,
                result = BalloonBoughtResult, quantity = 1, quality = isBox ? 0 : ShopService.Q, unitMinor = o.GoldPrice, totalMinor = o.GoldPrice, gold = -o.GoldPrice });
            return true;
        }

        private static bool Waiting(BoxEntry e) => !e.Claimed && e.ClaimTxn == null;
        // Rewards still in bought boxes (the MENU's Boxes count).
        public static int WaitingRewards(BalloonState s)
        {
            int n = 0;
            foreach (BoxManifest b in s.Boxes) foreach (BoxEntry e in b.Entries) if (Waiting(e)) n++;
            return n;
        }
        private static BoxManifest WaitingBox(BalloonState s)
        {
            foreach (BoxManifest b in s.Boxes) foreach (BoxEntry e in b.Entries) if (Waiting(e)) return b;
            return null;
        }

        // Claims every entry in order (MysteryBoxes.Claim grants each at most once); returns the notes to show, without repeats.
        public static List<string> ClaimBox(BalloonState s, string boxId, SpatialInventory bag, CraftingState crafting, System.Func<string> newId, out bool changed)
        {
            var notes = new List<string>(); changed = false;
            BoxManifest box = s.Boxes.Find(b => b.BoxId == boxId);
            if (box == null) return notes;
            for (int i = 0; i < box.Entries.Length; i++)
            {
                if (!Waiting(box.Entries[i])) continue;
                changed |= MysteryBoxes.Claim(s, boxId, i, bag, crafting, newId, out string note);
                if (note != "" && !notes.Contains(note)) notes.Add(note);
            }
            notes.Remove(MysteryBoxes.AllClaimed);
            if (System.Array.TrueForAll(box.Entries, e => !Waiting(e))) notes.Add(MysteryBoxes.AllClaimed);
            return notes;
        }

        // A reward sheet row: the item and amount, or the recipe (its fallback item once the recipe is known elsewhere).
        public static string BoxRewardLine(BoxEntry e, CraftingState crafting)
        {
            RecipeDefinition recipe = e.Kind == "recipe" ? RecipeBook.Find(e.RecipeId) : null;
            if (recipe != null && (e.Claimed || !crafting.Knows(recipe.Id))) return "Recipe: " + recipe.Name;
            bool fallback = e.Kind == "recipe";
            ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(fallback ? e.FallbackItemId : e.ItemId);
            int units = fallback ? e.FallbackUnits : e.Units;
            return (def != null ? def.Name : fallback ? e.FallbackItemId : e.ItemId) + (units > 1 ? " x" + Money.Cash(units) : "");
        }

        // ---------------------------------------------------------------- opening and closing

        private readonly RaycastHit[] balloonHits = new RaycastHit[16];
        // A tap on the camp's balloon: seated at its host camp it opens the interior; otherwise a hint.
        private bool TryTapBalloon(Vector2 screen)
        {
            GameObject balloonObject = scene != null && scene.StartCamp != null ? scene.StartCamp.Balloon : null;
            if (balloonObject == null || !balloonObject.activeInHierarchy || cameraView == null) return false;
            int count = Physics.RaycastNonAlloc(cameraView.ScreenPointToRay(screen), balloonHits, cameraView.farClipPlane, ~0, QueryTriggerInteraction.Collide);
            bool hit = false;
            float near = float.MaxValue;
            for (int i = 0; i < count; i++) if (balloonHits[i].transform.IsChildOf(balloonObject.transform) && balloonHits[i].distance < near) { hit = true; near = balloonHits[i].distance; }
            if (!hit) return false;
            // A pickup or harvest target in front of the balloon takes the tap (as TryStartHarvestAt does for pickups).
            for (int i = 0; i < count; i++)
            {
                if (balloonHits[i].distance >= near) continue;
                if (bundles.Exists(x => x.Collider == balloonHits[i].collider && x.Item.Owner == ItemOwner.World)) return false;
                if (harvestReady && harvestTargets.Exists(t => !t.Harvested && balloonHits[i].transform.IsChildOf(t.Root))) return false;
            }
            if (phase != TravelPhase.Seated) return false;   // silent: the tap falls through to pickups and harvest
            OpenBalloon();
            return true;
        }

        private ICollection<int> BalloonRegions() => ShopService.EffectiveRegions(shops, ShopRegions());   // debug TIER +1 counts as discovery
        private MysteryBoxContext BalloonBoxContext() => new MysteryBoxContext
        {
            Regions = BalloonRegions(), ReachedTier = ShopService.ReachedTier(shops, ShopRegions()), Crafting = crafting, Rng = new System.Random(),
            MissionRequests = missions.Active != null ? new[] { missions.RequestedFor(missions.Active) } : new string[0],
        };

        private void OpenBalloon()
        {
            if (BalloonOpen || travel == null || cameraView == null) return;
            if (!BalloonEntryAllowed(phase == TravelPhase.Seated, travel.CurrentCampId, TravelBusy || missionHandoff || active != null)) return;
            var prefab = Resources.Load<GameObject>("Settlement/Balloon/BalloonInterior");
            if (prefab == null) { Debug.LogWarning("[Balloon] Interior art is missing; staying at the camp."); return; }
            if (world == null) world = GetComponent<MovementPrototype>();
            if (inventoryOpen) ToggleInventory();   // the bag is closed at the counter
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);

            var holder = new GameObject("Balloon interior").transform;
            holder.SetParent(world != null ? world.transform : transform, false);
            holder.SetPositionAndRotation(MovementPrototype.InteriorWorldPosition, Quaternion.identity);
            Transform room = Instantiate(prefab, holder, false).transform;   // the merchant is baked in as Shopkeeper
            foreach (Collider c in room.GetComponentsInChildren<Collider>(true)) { if (Application.isPlaying) Destroy(c); else DestroyImmediate(c); }
            HangLanternLights(holder);
            for (int i = 0; i < 3; i++) shopAnchors[i] = FindChild(room, "OfferAnchor" + (i + 1));
            balloonHolder = holder; balloonRegion = BalloonHosts.Region(travel.CurrentCampId);
            BalloonShop.Open(balloon, travel.CurrentCampId, BalloonRegions(), ShopService.ReachedTier(shops, ShopRegions()), new System.Random());   // keeps an open session

            // The follow rig would pull the camera back to the seat: it rests while the interior holds the view.
            if (CameraRig != null) CameraRig.enabled = false;
            Pose pose = ShopCounterPose(cameraView, room);
            cameraView.transform.SetPositionAndRotation(pose.position, pose.rotation);
            if (router != null) router.SpeedAreaEnabled = false;   // no stand-up swipe on the centre line: every touch is the counter's

            if (shopHud == null) CreateShopHud();
            shopHud.gameObject.SetActive(true); CloseShopSheet(); SetCounterChrome(true);
            ShowScreenReturn(shopHud.gameObject, CloseBalloon);
            for (int i = 0; i < 3; i++) RefreshBalloonSlot(i);
            RefreshWalletLabels(); RefreshTravelHud();
            SaveNow();   // the session's offers survive a reload
        }

        // The Return square: back to the camp seat and one save.
        private void CloseBalloon()
        {
            if (!BalloonOpen) return;
            TearDownBalloon();
            SetMenu(menuOpen);
            SaveNow();
            ReleaseUnusedArt();
        }

        // Every exit (Return, UpdateBalloon's checks, ResetHomeView). Closing the interior ends the session.
        private void TearDownBalloon()
        {
            if (!BalloonOpen) return;
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            SetCounterChrome(false);
            CloseShopSheet();
            if (Application.isPlaying) Destroy(balloonHolder.gameObject); else DestroyImmediate(balloonHolder.gameObject);
            balloonHolder = null; lumberPointer = null;
            System.Array.Clear(shopAnchors, 0, 3); System.Array.Clear(shopModels, 0, 3);
            BalloonShop.Close(balloon);
            if (CameraRig != null) CameraRig.enabled = true;
            if (router != null) router.SpeedAreaEnabled = true;   // the balloon only opens at camps
            if (shopHud != null) shopHud.gameObject.SetActive(false);
            if (shopHud != null && screenReturnOwner == shopHud.gameObject) HideScreenReturn();
            RefreshTravelHud();
        }

        // Another view over the counter (the bag, map, crafting, gear, mission papers, a trade, the toll, a hand-off, any other Return owner).
        private bool BalloonCovered => inventoryOpen || TradePanelOpen || (shopHud != null && screenReturnOwner != shopHud.gameObject)
            || (mapPanel != null && mapPanel.activeSelf) || (missionPanel != null && missionPanel.activeSelf) || (craftPanel != null && craftPanel.activeSelf)
            || (gearPanel != null && gearPanel.activeSelf) || (tollPanel != null && tollPanel.activeSelf) || (missionShow != null && missionShow.activeSelf);

        // Per frame outside the settlement (from UpdateTravel): notes time out as in the settlement; the view closes when the player is no
        // longer seated, faints or another view opens; otherwise the price tags follow their anchors.
        private void UpdateBalloon()
        {
            if (!BalloonOpen) return;
            if (settlementNote != null && settlementNote.activeSelf && Time.unscaledTime >= settlementNoteHideAt) settlementNote.SetActive(false);
            if (BalloonMustClose(phase == TravelPhase.Seated, stamina != null && stamina.Fainted, BalloonCovered)) { TearDownBalloon(); SetMenu(menuOpen); SaveNow(); return; }
            if (cameraView != null) UpdateOfferTags();
        }

        private void RefreshBalloonSlot(int slot)
        {
            BalloonOffer o = balloon.Offers != null && slot < balloon.Offers.Length ? balloon.Offers[slot] : null;
            if (o == null) { SetOfferSlot(slot, ShopSoldOutText, true, null); return; }
            int region = balloonRegion;
            SetOfferSlot(slot, BalloonTagText(o), o.Sold, a => BalloonOfferModel(a, o, region));
        }

        // ---------------------------------------------------------------- touch (the shop counters' tap tracking)

        private bool BalloonPointerEnd(int pointer, Vector2 position, bool canceled)
        {
            if (!BalloonOpen || lumberPointer != pointer) return false;
            lumberPointer = null;
            if (!canceled && !lumberMoved) { int pick = OfferTapPick(position); if (pick >= 0) OpenBalloonSheet(pick); }
            return true;
        }

        // ---------------------------------------------------------------- the purchase sheet (the shop sheet, refilled)

        private void OpenBalloonSheet(int slot)
        {
            if (balloon.Offers == null || slot < 0 || slot >= balloon.Offers.Length) return;
            shopSheetSlot = slot; shopSheetRevision = balloon.Offers[slot].Revision;
            shopSheet.transform.SetAsLastSibling(); shopSheet.SetActive(true);
            RefreshBalloonSheet();
        }

        private void RefreshBalloonSheet()
        {
            if (shopSheetSlot < 0 || !BalloonOpen || balloon.Offers == null) return;
            BalloonOffer o = balloon.Offers[shopSheetSlot];
            ClearContent(shopSheetPage);
            ItemDefinition def = o.ItemId != null ? TheTrailWalker.Domain.Crafting.Items.Find(o.ItemId) : null;
            Image icon = Rect("Picture", shopSheetPage, 0.2f, 0.56f, 0.8f, 0.98f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
            Sprite sprite = def != null ? ItemIconRenderer.For(def) : null;
            if (sprite != null) icon.sprite = sprite; else icon.color = Color.clear;
            Text name = Label(shopSheetPage, BalloonOfferName(o), 0.04f, 0.46f, 0.96f, 0.55f, 34); name.color = Ink; name.fontStyle = FontStyle.Bold;
            name.resizeTextForBestFit = true; name.resizeTextMinSize = 14; name.resizeTextMaxSize = 34;
            Label(shopSheetPage, Money.Cash(o.GoldPrice) + " gold", 0.04f, 0.32f, 0.96f, 0.39f, 26).color = FadedInk;
            if (o.Sold || travel.Gold < o.GoldPrice)
            {
                Text none = Label(shopSheetPage, o.Sold ? ShopSoldOutText : "Not enough gold", 0.04f, 0.10f, 0.96f, 0.26f, 28);
                none.color = FadedInk; none.fontStyle = FontStyle.Italic;
                return;
            }
            Text pay = Button(shopSheetPage, "PAY", 0.06f, 0f, 0.94f, 0.12f, BalloonPayTapped);
            pay.text = "PAY " + Money.Cash(o.GoldPrice) + " GOLD";
        }

        // One save per purchase, on the line right after it: the gold, the item or the box manifest, and the record land together.
        private void BalloonPayTapped()
        {
            if (shopSheetSlot < 0 || !BalloonOpen || saveBlocked) return;
            int slot = shopSheetSlot; long goldBefore = travel.Gold;
            if (!BalloonBuyAndRecord(balloon, slot, shopSheetRevision, travel.Wallet, ledger, BalloonBoxContext(), () => "balloon-" + System.Guid.NewGuid().ToString("N"), warehouseLog, System.DateTime.UtcNow, out BoxManifest box, out string reason)) { ShowSettlementNote(reason); return; }
            statistics.SpendGold(goldBefore - travel.Gold); SaveNow();
            RefreshWalletLabels();
            if (box != null) { CloseShopSheet(); StartCoroutine(OpenBoxOnCounter(slot, box.BoxId)); return; }
            shopSheetRevision = balloon.Offers[slot].Revision;
            RefreshBalloonSlot(slot); RefreshBalloonSheet();
        }

        // Decision 5: the bought box opens on the counter (its Animator's Open trigger), then the reward sheet claims what fits.
        private IEnumerator OpenBoxOnCounter(int slot, string boxId)
        {
            Animator animator = shopModels[slot] != null ? shopModels[slot].GetComponentInChildren<Animator>() : null;
            if (animator != null) animator.SetTrigger("Open");
            yield return new WaitForSecondsRealtime(BoxOpenSeconds);
            if (!BalloonOpen) yield break;   // left mid-animation: the saved box waits under MENU, Boxes
            RefreshBalloonSlot(slot);
            OpenBoxSheet(boxId);
        }

        // ---------------------------------------------------------------- the reward sheet (the shop sheet, refilled)

        // The MENU's Boxes entry: the oldest box with a reward still waiting.
        private void OpenWaitingBox()
        {
            BoxManifest box = WaitingBox(balloon);
            SetMenu(false);
            if (box != null) OpenBoxSheet(box.BoxId);
        }

        // Claims what fits (one save when anything changed), then lists the six entries with their state; notes on the paper note.
        private void OpenBoxSheet(string boxId)
        {
            BoxManifest box = balloon.Boxes.Find(b => b.BoxId == boxId);
            if (box == null || saveBlocked) return;
            List<string> notes = ClaimBox(balloon, boxId, ledger, crafting, () => "box-" + System.Guid.NewGuid().ToString("N"), out bool changed);
            if (changed) SaveNow();
            if (shopHud == null) CreateShopHud();
            shopSheetSlot = -1;
            shopSheet.transform.SetAsLastSibling(); shopSheet.SetActive(true);
            ClearContent(shopSheetPage);
            Text title = Label(shopSheetPage, "Mystery box", 0.04f, 0.90f, 0.96f, 0.99f, 34); title.color = Ink; title.fontStyle = FontStyle.Bold;
            for (int i = 0; i < box.Entries.Length; i++)
            {
                BoxEntry e = box.Entries[i];
                float top = 0.88f - i * 0.145f;
                RectTransform row = Rect("Reward row " + (i + 1), shopSheetPage, 0f, top - 0.13f, 1f, top);
                string itemId = e.Kind == "recipe" ? RecipeBook.Find(e.RecipeId)?.OutputId ?? e.FallbackItemId : e.ItemId;
                ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(itemId);
                Image icon = Rect("Picture", row, 0.02f, 0.05f, 0.2f, 0.95f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
                Sprite sprite = def != null ? ItemIconRenderer.For(def) : null;
                if (sprite != null) icon.sprite = sprite; else icon.color = Color.clear;
                Text line = Label(row, BoxRewardLine(e, crafting), 0.23f, 0.05f, 0.74f, 0.95f, 26); line.color = Ink; line.alignment = TextAnchor.MiddleLeft;
                line.resizeTextForBestFit = true; line.resizeTextMinSize = 14; line.resizeTextMaxSize = 26;
                Text state = Label(row, Waiting(e) ? "Waiting" : "Claimed", 0.75f, 0.05f, 0.98f, 0.95f, 24); state.alignment = TextAnchor.MiddleRight;
                state.color = Waiting(e) ? RedInk : FadedInk; state.fontStyle = Waiting(e) ? FontStyle.Bold : FontStyle.Italic;
            }
            SetMenu(menuOpen);   // the Boxes count follows the claims
            if (notes.Count > 0) ShowSettlementNote(string.Join("\n", notes));
        }

        // The MENU's Boxes entry: shown with its waiting count while any reward waits (SetMenu).
        private bool BoxesMenuShown(bool open)
        {
            int waiting = WaitingRewards(balloon);
            if (boxesMenuCount != null) boxesMenuCount.text = waiting.ToString();
            return open && waiting > 0;
        }
        private void CreateBoxesMenuButton()
        {
            boxesMenuButton = AddMenuButton("BOXES", "storage", OpenWaitingBox);
            boxesMenuButton.name = "Boxes";
            boxesMenuCount = Label(boxesMenuButton, "", 0.45f, 0.02f, 0.98f, 0.42f, 26);
            boxesMenuCount.gameObject.name = "Count"; boxesMenuCount.alignment = TextAnchor.LowerRight; boxesMenuCount.fontStyle = FontStyle.Bold;
            Outline edge = boxesMenuCount.gameObject.AddComponent<Outline>(); edge.effectColor = new Color(0f, 0f, 0f, 0.85f); edge.effectDistance = new Vector2(1.2f, -1.2f);
        }
    }
}
