using System.Collections.Generic;
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
    // Milestone 14 slice 1 (spec 2026-11-20 section 6.1): the five shop counters. Tapping a shop building or its list entry builds
    // its counter 200 m below the street like the lumber counter, framed from CustomerCameraAnchor with the bag closed, and puts
    // the visit's three offers on OfferAnchor1-3. Tapping an offer opens a paper sheet to buy it. Counts as HomeOpen, so
    // ResetHomeView tears it down.
    public sealed partial class CollectionPrototype
    {
        public const string ShopSoldOutText = "Sold out";
        private static readonly int[] ShopAmounts = { 1, 10, 100, 1000 };
        private Transform shopHolder;
        private string shopId;
        private readonly Transform[] shopAnchors = new Transform[3];
        private readonly GameObject[] shopModels = new GameObject[3];
        private RectTransform shopHud, shopSheetPage;
        private GameObject shopSheet;
        private Text shopWalletText;
        private readonly Text[] shopSoldOutLabels = new Text[3];   // the price tags (a live offer or Sold out)
        private readonly Rect[] shopTagRects = new Rect[3];       // screen px, width 0 = hidden
        private readonly Vector3[] shopSafeCorners = new Vector3[4];
        private readonly RectTransform[] shopTagPlates = new RectTransform[3];
        private readonly Vector2[] shopTops = new Vector2[3]; private readonly bool[] shopShown = new bool[3];   // per-frame scratch
        private readonly int[] shopTagChars = new int[3]; private readonly float[] shopModelTopY = new float[3];   // set once in RefreshShopSlot
        private int shopSheetSlot = -1, shopSheetRevision, shopAmount;   // the open sheet's slot (-1 closed), the revision captured when it opened, the chosen units

        private bool ShopOpen => shopHolder != null;

        // ---------------------------------------------------------------- pure rules (checked by Milestone14Validation.ViewChecks)

        // The settlement list keys and the street building names ("shop." + key) of the five shops; null for anything else.
        public static string ShopIdForKey(string listKey)
        {
            switch (listKey)
            {
                case "farm": return ShopCatalog.Farm;
                case "mine": return ShopCatalog.Mine;
                case "tailor": return ShopCatalog.Tailor;
                case "ranch": return ShopCatalog.Ranch;
                case "fishery": return ShopCatalog.Fishery;
                default: return null;
            }
        }

        // Of 1, 10, 100 and 1,000: the amounts still in stock that the cash covers.
        public static int[] AmountChoices(int unitsLeft, long cashMinor, long unitPriceMinor)
        {
            var choices = new List<int>();
            foreach (int n in ShopAmounts) if (n <= unitsLeft && unitPriceMinor <= cashMinor / n) choices.Add(n);   // n·price <= cash without overflow
            return choices.ToArray();
        }

        public static string OfferLine(ShopOffer o) => "Q" + ShopService.Q + " · " + Money.Cash(Money.Charge(o.UnitPriceMinor)) + " cash each";
        public static string LeftLine(ShopOffer o) => "Left: " + Money.Cash(o.UnitsLeft);

        // The price tag over a counter slot: name line, then the price; Sold out when bought out or empty.
        public static string OfferTagText(ShopOffer o, string name) =>
            o.Empty || o.UnitsLeft <= 0 ? ShopSoldOutText : name + "\n" + Money.Cash(Money.Charge(o.UnitPriceMinor)) + " cash";

        // Tag rects (screen px, y up) just above each model top, as wide as their text needs but never wider than the gap to the nearest other tag, inside the area.
        // Allocation-free: longestLine[i] is the char count of tag i's longest line; the result goes into the caller's rects.
        public static void ShopTagLayout(Vector2[] tops, int[] longestLine, Rect area, float height, float gap, Rect[] rects)
        {
            float spacing = float.MaxValue;
            for (int i = 0; i < tops.Length; i++) for (int j = i + 1; j < tops.Length; j++) spacing = Mathf.Min(spacing, Mathf.Abs(tops[i].x - tops[j].x));
            for (int i = 0; i < tops.Length; i++)
            {
                float width = Mathf.Min(Mathf.Clamp(0.1f + 0.016f * longestLine[i], 0.34f, 0.8f) * area.width, Mathf.Max(0f, spacing - gap));   // = WarehouseLabelWidth for that many chars
                rects[i] = new Rect(WarehouseLabelLeft(tops[i].x, width, area.xMin, area.xMax), Mathf.Clamp(tops[i].y + gap, area.yMin, area.yMax - height), width, height);
            }
        }

        public static int LongestLine(string text) { int best = 0, run = 0; foreach (char c in text) { if (c == '\n') run = 0; else best = Mathf.Max(best, ++run); } return best; }

        // A tap belongs to the slot whose anchor or tag centre is nearest, within reach (a tag sits above its model, so its centre counts too).
        public static int ShopTapPick(Vector2 tap, Vector2?[] anchors, Rect[] tags, float reach)
        {
            float best = reach; int pick = -1;
            for (int i = 0; i < anchors.Length; i++)
            {
                float d = Mathf.Min(anchors[i].HasValue ? Vector2.Distance(tap, anchors[i].Value) : float.MaxValue, tags[i].width > 0f ? Vector2.Distance(tap, tags[i].center) : float.MaxValue);
                if (d <= best) { best = d; pick = i; }
            }
            return pick;
        }

        public const string ShopBoughtResult = "shop.bought";
        // The PAY button's purchase: TryBuy, then one record in the settlement's trade log (spec 6.1: "record the trade").
        // cash is the whole cash charged (negative: paid by the player), as the camp trade records it.
        public static bool BuyAndRecord(ShopState s, string shopId, int slot, int revision, int units, Wallet wallet, SpatialInventory bag, System.Func<string> newId, List<TradeRecord> log, System.DateTime utcNow, out string reason)
        {
            if (!ShopService.TryBuy(s, shopId, slot, revision, units, wallet, bag, newId, out reason)) return false;
            ShopOffer o = s.Stock[shopId].Offers[slot];
            long total = units * o.UnitPriceMinor;
            TradeLog.Add(log, new TradeRecord { utc = utcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture), settlementId = shopId, itemId = o.ItemId, result = ShopBoughtResult,
                quantity = units, quality = ShopService.Q, unitMinor = o.UnitPriceMinor, totalMinor = total, cash = -Money.Charge(total) });
            return true;
        }

        // The raw CustomerCameraAnchor view crops the outer offers on a portrait screen: turn toward the three offer anchors, then
        // the warehouse solver pitches and pulls back until all three sit in the band below the debug buttons (LumberCounterPose).
        public static Pose ShopCounterPose(Camera cam, Transform counter)
        {
            Transform eye = FindChild(counter, "CustomerCameraAnchor");
            if (eye == null) return new Pose(counter.position + new Vector3(0f, 1.55f, 3f), Quaternion.LookRotation(Vector3.back));
            var targets = new List<Bounds>();
            for (int i = 1; i <= 3; i++)
            {
                Transform a = FindChild(counter, "OfferAnchor" + i);
                if (a != null) targets.Add(new Bounds(a.position, Vector3.one * 0.8f));   // ~a tap's reach around each
            }
            if (targets.Count == 0) return new Pose(eye.position, eye.rotation);
            Bounds all = targets[0]; foreach (Bounds b in targets) all.Encapsulate(b);
            Vector3 flat = Vector3.ProjectOnPlane(all.center - eye.position, Vector3.up);   // the anchors sit off centre: face the middle of their span
            Transform t = cam.transform; Vector3 p = t.position; Quaternion r = t.rotation;
            t.SetPositionAndRotation(eye.position, Quaternion.LookRotation(flat));
            try { return WarehouseCameraPose(cam, targets.ToArray(), 0.1f, 0.48f); }
            finally { t.SetPositionAndRotation(p, r); }
        }

        // ---------------------------------------------------------------- opening and closing

        private static readonly Dictionary<string, string> ShopArt = new Dictionary<string, string>
        {
            { ShopCatalog.Farm, "Farm|ShopkeeperFarmer" }, { ShopCatalog.Mine, "Mine|ShopkeeperMiner" }, { ShopCatalog.Tailor, "Tailor|ShopkeeperTailor" },
            { ShopCatalog.Ranch, "Ranch|ShopkeeperRancher" }, { ShopCatalog.Fishery, "Fishery|ShopkeeperFisher" },
        };

        private List<int> ShopRegions()
        {
            var regions = new List<int>();
            foreach (string id in travel.DiscoveredRegions) { RegionDefinition r = scene.Catalog.Region(id); if (r != null) regions.Add(r.Number); }
            return regions;
        }
        private ShopOffer[] ShopOffers() => ShopService.Offers(shops, shopId, ShopRegions());

        private void OpenShop(string id)
        {
            if (phase != TravelPhase.Settlement || saveBlocked || HomeOpen || StreetView == null || cameraView == null || (stamina != null && stamina.Fainted)) return;
            if (id == null || !ShopArt.TryGetValue(id, out string art)) return;
            if (world == null) world = GetComponent<MovementPrototype>();
            string[] names = art.Split('|');
            var prefab = Resources.Load<GameObject>("Settlement/Storefronts/" + names[0] + "Counter");
            if (world == null || prefab == null) { Debug.LogWarning("[Shops] Counter art is missing; staying on the street."); return; }
            CancelSettlementPointer();
            settlementListOpen = false; settlementListLayout = -1;
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            if (inventoryOpen) ToggleInventory();   // the bag is closed at the counter

            var holder = new GameObject("Shop counter").transform;
            holder.SetParent(world.transform, false);
            holder.SetPositionAndRotation(MovementPrototype.InteriorWorldPosition, Quaternion.identity);
            Transform counter = Instantiate(prefab, holder, false).transform;
            CounterLamps.Hang(counter, names[0]);
            // The shop counters carry no keeper (the lumber counter does): stand the shop's keeper at the counter origin, where the art builder puts it.
            var keeper = FindChild(counter, "Shopkeeper") == null ? Resources.Load<GameObject>("Characters/Shopkeepers/" + names[1]) : null;
            if (keeper != null) Instantiate(keeper, counter, false).name = "Shopkeeper";
            foreach (Collider c in counter.GetComponentsInChildren<Collider>(true)) Destroy(c);
            for (int i = 0; i < 3; i++)
            {
                shopAnchors[i] = FindChild(counter, "OfferAnchor" + (i + 1));
                if (shopAnchors[i] != null) foreach (Transform baked in shopAnchors[i]) baked.gameObject.SetActive(false);   // baked display props make way for the offers
            }
            shopHolder = holder; shopId = id;

            Pose pose = ShopCounterPose(cameraView, counter);
            StreetView.HoldAt(pose.position, pose.rotation);

            if (shopHud == null) CreateShopHud();
            shopHud.gameObject.SetActive(true); CloseShopSheet();
            ShowScreenReturn(shopHud.gameObject, CloseShop);
            for (int i = 0; i < 3; i++) RefreshShopSlot(i);
            shopWalletText.text = "Cash " + Money.Cash(travel.Cash);
        }

        // The Return square: back to the street and one save.
        private void CloseShop()
        {
            if (!ShopOpen) return;
            TearDownShop();
            settlementListLayout = -1;
            StreetView?.SnapToStreet();
            SetMenu(menuOpen);
            SaveNow();
        }

        // Every exit (Return, and ResetHomeView).
        private void TearDownShop()
        {
            if (!ShopOpen) return;
            CloseShopSheet();
            if (Application.isPlaying) Destroy(shopHolder.gameObject); else DestroyImmediate(shopHolder.gameObject);
            shopHolder = null; shopId = null; lumberPointer = null;
            System.Array.Clear(shopAnchors, 0, 3); System.Array.Clear(shopModels, 0, 3);
            if (shopHud != null) shopHud.gameObject.SetActive(false);
            if (shopHud != null && screenReturnOwner == shopHud.gameObject) HideScreenReturn();
        }

        // The slot's item model on its anchor, or the Sold out label when the slot is empty or bought out.
        private void RefreshShopSlot(int slot)
        {
            if (shopModels[slot] != null) { Destroy(shopModels[slot]); shopModels[slot] = null; }
            ShopOffer o = ShopOffers()[slot];
            bool soldOut = o.Empty || o.UnitsLeft <= 0;
            ItemDefinition def = o.Empty ? null : TheTrailWalker.Domain.Crafting.Items.Find(o.ItemId);
            Text tag = shopSoldOutLabels[slot];
            string text = OfferTagText(o, def != null ? def.Name : o.ItemId);
            int nl = text.IndexOf('\n');
            tag.text = nl < 0 ? text : "<b>" + text.Substring(0, nl) + "</b>" + text.Substring(nl);
            shopTagChars[slot] = LongestLine(text); shopModelTopY[slot] = float.NaN;
            tag.color = soldOut ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
            tag.transform.parent.gameObject.SetActive(shopAnchors[slot] != null);
            if (soldOut || shopAnchors[slot] == null) return;
            if (def == null) return;
            shopModels[slot] = PlaceOfferModel(shopAnchors[slot], def);
            var bounds = new Bounds(shopAnchors[slot].position, Vector3.zero);
            foreach (Renderer r in shopModels[slot].GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            shopModelTopY[slot] = bounds.max.y;   // the model never moves while the counter is open
        }

        // The trail's model at the trail's size (largest front side = the footprint), standing on the anchor.
        public static GameObject PlaceOfferModel(Transform anchor, ItemDefinition def)
        {
            var probe = new GameObject("Offer " + def.Id, typeof(MeshRenderer), typeof(MeshFilter));   // AttachPath hides this renderer
            probe.transform.SetParent(anchor, false);
            GameObject visual = ResourceVisualTuning.Attach(probe, def);
            if (visual == null) return probe;   // no art yet: nothing to show (the sheet still sells it)
            Bounds b = LocalMeshBounds(probe.transform, visual);
            float scale = ResourceVisualTuning.World(def) * Mathf.Max(def.Width, def.Height) / 1000f / Mathf.Max(0.01f, Mathf.Max(b.size.x, b.size.y));
            probe.transform.localScale = Vector3.one * scale;
            probe.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z) * scale;
            return probe;
        }

        // ---------------------------------------------------------------- HUD and purchase sheet

        private static readonly Color ShopTagAccent = new Color(0.788f, 0.635f, 0.153f);   // #C9A227

        private void CreateShopHud()
        {
            shopHud = Rect("Shop HUD", safe, 0, 0, 1, 1);
            shopHud.SetAsFirstSibling();
            shopWalletText = HudLine(shopHud, 0.925f, 0.97f);
            for (int i = 0; i < 3; i++)
            {
                RectTransform plate = Rect("Offer tag " + i, shopHud, 0, 0, 0, 0); plate.pivot = Vector2.zero; plate.sizeDelta = new Vector2(150f, 76f);
                Image back = plate.gameObject.AddComponent<Image>(); back.color = new Color(0.12f, 0.105f, 0.085f, 0.8f); back.raycastTarget = false;
                Image stripe = Rect("Price stripe", plate, 0, 0.9f, 1, 1).gameObject.AddComponent<Image>(); stripe.color = ShopTagAccent; stripe.raycastTarget = false;   // the brass edge marks it a price tag
                Text label = Label(plate, ShopSoldOutText, 0.04f, 0f, 0.96f, 0.9f, 26);
                label.raycastTarget = false; label.resizeTextForBestFit = true; label.resizeTextMinSize = 12; label.resizeTextMaxSize = 26;
                shopSoldOutLabels[i] = label; shopTagPlates[i] = plate;
                plate.gameObject.SetActive(false);
            }
            shopHud.gameObject.SetActive(false);
            CreateShopSheet();
        }

        // The lumber ledger's paper sheet over a dim backdrop, with a Close square; the page is rebuilt on every refresh.
        private void CreateShopSheet()
        {
            RectTransform root = Rect("Shop sheet", safe, 0, 0, 1, 1);
            RectTransform backdrop = Rect("Backdrop", root, 0, 0, 1, 1);
            backdrop.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);   // blocks every touch behind the sheet
            var away = backdrop.gameObject.AddComponent<UnityEngine.UI.Button>(); away.transition = Selectable.Transition.None; away.onClick.AddListener(CloseShopSheet);
            Transform p = Sheet("Sheet", root, "paper_1", 0.05f, 0.13f, 0.95f, 0.87f, 0f).transform;
            Text close = Button(p, "Close", 0, 0, 0, 0, CloseShopSheet);
            var closeRect = (RectTransform)close.transform.parent; closeRect.name = "Shop: Close sheet";
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.97f, 0.975f); closeRect.pivot = new Vector2(1f, 1f);
            closeRect.sizeDelta = new Vector2(HudSquare, HudSquare); closeRect.anchoredPosition = Vector2.zero;
            HudIconStyle.Apply(close, "return");
            shopSheetPage = Rect("Page", p, 0.05f, 0.04f, 0.95f, 0.87f);
            shopSheet = root.gameObject;
            shopSheet.SetActive(false);
        }

        private long CashMinor => travel.Cash > long.MaxValue / Money.MinorPerCash ? long.MaxValue : Money.Minor(travel.Cash);

        private void OpenShopSheet(int slot)
        {
            ShopOffer o = ShopOffers()[slot];
            if (o.Empty) return;
            shopSheetSlot = slot; shopSheetRevision = o.Revision; shopAmount = 0;
            shopSheet.transform.SetAsLastSibling(); shopSheet.SetActive(true);
            RefreshShopSheet();
        }
        private void CloseShopSheet()
        {
            shopSheetSlot = -1;
            if (shopSheet != null) shopSheet.SetActive(false);
        }

        private void RefreshShopSheet()
        {
            if (shopSheetSlot < 0 || !ShopOpen) return;
            ShopOffer o = ShopOffers()[shopSheetSlot];
            int[] choices = AmountChoices(o.UnitsLeft, CashMinor, o.UnitPriceMinor);
            if (System.Array.IndexOf(choices, shopAmount) < 0) shopAmount = choices.Length > 0 ? choices[0] : 0;   // default: the smallest
            ClearContent(shopSheetPage);
            ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(o.ItemId);
            Image icon = Rect("Picture", shopSheetPage, 0.2f, 0.56f, 0.8f, 0.98f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
            Sprite sprite = def != null ? ItemIconRenderer.For(def) : null;
            if (sprite != null) icon.sprite = sprite; else icon.color = Color.clear;
            Text name = Label(shopSheetPage, def != null ? def.Name : o.ItemId, 0.04f, 0.46f, 0.96f, 0.55f, 34); name.color = Ink; name.fontStyle = FontStyle.Bold;
            name.resizeTextForBestFit = true; name.resizeTextMinSize = 14; name.resizeTextMaxSize = 34;
            Label(shopSheetPage, OfferLine(o), 0.04f, 0.39f, 0.96f, 0.46f, 26).color = FadedInk;
            Label(shopSheetPage, LeftLine(o), 0.04f, 0.32f, 0.96f, 0.39f, 26).color = FadedInk;
            if (choices.Length == 0)
            {
                Text none = Label(shopSheetPage, o.UnitsLeft <= 0 ? ShopSoldOutText : "Not enough cash", 0.04f, 0.10f, 0.96f, 0.26f, 28);
                none.color = FadedInk; none.fontStyle = FontStyle.Italic;
                return;
            }
            const float x0 = 0.04f, x1 = 0.96f, gap = 0.03f;
            float w = (x1 - x0 - gap * (choices.Length - 1)) / choices.Length;
            for (int i = 0; i < choices.Length; i++)
            {
                int units = choices[i];
                Text b = Button(shopSheetPage, units.ToString("N0", System.Globalization.CultureInfo.InvariantCulture), x0 + i * (w + gap), 0.17f, x0 + i * (w + gap) + w, 0.27f, () => { shopAmount = units; RefreshShopSheet(); });
                if (units == shopAmount) { b.transform.parent.GetComponent<Image>().color = new Color(0.72f, 0.46f, 0.12f); b.fontStyle = FontStyle.Bold; }   // the chosen amount
            }
            Text pay = Button(shopSheetPage, "PAY", 0.06f, 0f, 0.94f, 0.12f, ShopPayTapped);
            pay.text = "PAY " + Money.Cash(Money.Charge(shopAmount * o.UnitPriceMinor)) + " CASH";
        }

        private void ShopPayTapped()
        {
            if (shopSheetSlot < 0 || !ShopOpen || saveBlocked) return;
            int slot = shopSheetSlot;
            long cashBefore = travel.Cash;
            if (!BuyAndRecord(shops, shopId, slot, shopSheetRevision, shopAmount, travel.Wallet, ledger, () => "shop-" + System.Guid.NewGuid().ToString("N"), warehouseLog, System.DateTime.UtcNow, out string reason)) { ShowSettlementNote(reason); return; }
            statistics.SpendCash(cashBefore - travel.Cash);
            shopSheetRevision = ShopOffers()[slot].Revision;   // the sheet now shows the new stock
            Redraw(); SaveNow();
            RefreshShopSlot(slot); RefreshShopSheet();
            shopWalletText.text = "Cash " + Money.Cash(travel.Cash);
        }

        // Per frame from UpdateSettlement: the wallet line and the Sold out labels follow their anchors on screen.
        private void UpdateShops()
        {
            if (!ShopOpen || cameraView == null) return;
            shopWalletText.text = "Cash " + Money.Cash(travel.Cash);
            safe.GetWorldCorners(shopSafeCorners);   // overlay canvas: screen pixels
            float scale = CanvasScale;
            for (int i = 0; i < 3; i++)
            {
                shopShown[i] = false;
                if (shopAnchors[i] == null) continue;
                Vector3 a = shopAnchors[i].position;
                Vector3 p = cameraView.WorldToScreenPoint(float.IsNaN(shopModelTopY[i]) ? new Vector3(a.x, a.y + 0.05f, a.z) : new Vector3(a.x, shopModelTopY[i], a.z));
                shopShown[i] = p.z > 0f; shopTops[i] = p;
            }
            ShopTagLayout(shopTops, shopTagChars, UnityEngine.Rect.MinMaxRect(shopSafeCorners[0].x, shopSafeCorners[0].y, shopSafeCorners[2].x, shopSafeCorners[2].y), 76f * scale, Screen.height * 0.01f, shopTagRects);
            for (int i = 0; i < 3; i++)
            {
                RectTransform plate = shopTagPlates[i];
                plate.localScale = shopShown[i] ? Vector3.one : Vector3.zero;
                if (!shopShown[i]) { shopTagRects[i] = default; continue; }
                plate.sizeDelta = new Vector2(shopTagRects[i].width / scale, 76f);
                plate.position = new Vector3(shopTagRects[i].x, shopTagRects[i].y, 0f);
            }
        }

        // ---------------------------------------------------------------- touch: a short tap near an offer (the lumber counter's pointer)

        // Begin and move go through LumberPointerBegin/Move (the lumber counter's tap tracking); the end picks an offer instead.
        private bool ShopPointerEnd(int pointer, Vector2 position, bool canceled)
        {
            if (!ShopOpen || lumberPointer != pointer) return false;
            lumberPointer = null;
            if (!canceled && !lumberMoved) ShopTap(position);
            return true;
        }

        private void ShopTap(Vector2 screen)
        {
            var anchors = new Vector2?[3];
            for (int i = 0; i < 3; i++) if (LumberAnchorDistance(shopAnchors[i], screen) < float.MaxValue) anchors[i] = cameraView.WorldToScreenPoint(shopAnchors[i].position);
            int pick = ShopTapPick(screen, anchors, shopTagRects, LumberReach);
            if (pick >= 0) OpenShopSheet(pick);
        }

        // Debug TIER +1 (street HUD): raises the shops' tier by one, up to 4.
        private void DebugShopTier()
        {
            if (saveBlocked || HomeOpen) return;
            shops.DebugTier = System.Math.Min(4, System.Math.Max(ShopService.ReachedTier(shops, ShopRegions()), shops.DebugTier) + 1);   // debug (test phase), removed with HOUSE L8
            SaveNow();
            RebuildStreetWith(SettlementScenery.HouseLevels(settlement, home.Level));   // the storefronts switch looks at once
            Say("Debug: shops at tier " + shops.DebugTier + ".");
        }
    }
}
