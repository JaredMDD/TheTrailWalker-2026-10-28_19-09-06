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
        // Slot arrays hold the most offers a stock can have; the per-frame scratch below is sized to the open counter's count
        // (ShopTagLayout spaces the tags over every entry it is given).
        private readonly Transform[] shopAnchors = new Transform[ShopService.MaxOffers];
        private readonly GameObject[] shopModels = new GameObject[ShopService.MaxOffers];
        private int offerSlots = 3; private float offerMaxWidth;   // the open counter's slot count; the model width cap on a spread counter (0 = none)
        private RectTransform shopHud, shopSheetPage;
        private GameObject shopSheet;
        private readonly Text[] shopSoldOutLabels = new Text[ShopService.MaxOffers];   // the price tags (a live offer or Sold out)
        private Rect[] shopTagRects = new Rect[3];       // screen px, width 0 = hidden
        private readonly Vector3[] shopSafeCorners = new Vector3[4];
        private readonly RectTransform[] shopTagPlates = new RectTransform[ShopService.MaxOffers];
        private Vector2[] shopTops = new Vector2[3]; private bool[] shopShown = new bool[3];   // per-frame scratch
        private int[] shopTagChars = new int[3]; private float[] shopModelTopY = new float[3];   // set once in RefreshShopSlot
        private Bounds[] shopModelBounds = new Bounds[3]; private Rect[] shopModelRects = new Rect[3];   // bounds set in RefreshShopSlot; screen rects per frame

        // A counter opens with its stock's slot count (shops 3/5/6 by tier, the balloon 3); tags past it stay hidden.
        private void SetOfferSlotCount(int n)
        {
            offerSlots = n;
            if (shopTops.Length != n)
            {
                shopTagRects = new Rect[n]; shopTops = new Vector2[n]; shopShown = new bool[n];
                shopTagChars = new int[n]; shopModelTopY = new float[n]; shopModelBounds = new Bounds[n]; shopModelRects = new Rect[n];
            }
            for (int i = n; i < shopTagPlates.Length; i++) if (shopTagPlates[i] != null) shopTagPlates[i].gameObject.SetActive(false);
        }

        // More than three offers: N spots evenly along the counter top, centred between the art's first and last anchors at
        // their height and depth (counter space). The half span keeps the row inside the narrowest counter (Tailor, 5.4 m).
        public const float OfferRowHalfSpan = 1.9f;
        public static Vector3[] OfferSpots(Vector3 first, Vector3 last, int n)
        {
            var spots = new Vector3[n];
            float centre = (first.x + last.x) * 0.5f, y = (first.y + last.y) * 0.5f, z = (first.z + last.z) * 0.5f;
            for (int i = 0; i < n; i++) spots[i] = new Vector3(n == 1 ? centre : centre - OfferRowHalfSpan + 2f * OfferRowHalfSpan * i / (n - 1), y, z);
            return spots;
        }
        // The widest a model may be on a spread counter: most of the gap between neighbouring spots (0 = no cap).
        public static float OfferMaxWidth(int n) => n <= 3 ? 0f : 0.75f * 2f * OfferRowHalfSpan / (n - 1);
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
                case "bakery": return ShopCatalog.Bakery;
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

        // A gold food's amounts: the full stack counts as affordable at its discounted total.
        public static int[] GoldAmountChoices(ShopOffer o, long gold) => System.Array.FindAll(ShopAmounts, n => n <= o.UnitsLeft && ShopService.GoldTotal(o, n) <= gold);

        // Whole cash or gold an offer's price shows (gold offers hold whole gold already).
        private static string PriceText(ShopOffer o) => o.Currency == Currency.Gold ? Money.Cash(o.UnitPriceMinor) + " gold" : Money.Cash(Money.Charge(o.UnitPriceMinor)) + " cash";
        public static string PayText(ShopOffer o, int units) => o.Currency == Currency.Gold
            ? "PAY " + Money.Cash(ShopService.GoldTotal(o, units)) + " GOLD" : "PAY " + Money.Cash(Money.Charge(units * o.UnitPriceMinor)) + " CASH";

        // The sheet's price line; a gold food buying exactly the full stack shows the discounted total, a recipe offer its heading.
        public static string OfferLine(ShopOffer o, int units = 0) =>
            o.RecipeId != null ? "Learn the recipe"
            : o.Currency == Currency.Gold && units == ShopService.StartUnits ? Money.Cash(ShopService.GoldTotal(o, units)) + " gold for the full stack (10% off)"
            : PriceText(o) + " each";
        // Units left, or a recipe offer's gold price.
        public static string LeftLine(ShopOffer o) => o.RecipeId != null ? PriceText(o) : "Left: " + Money.Cash(o.UnitsLeft);

        // The price tag over a counter slot: name line (Recipe: <food> for a recipe offer), then the price; Sold out when bought out or empty.
        public static string OfferTagText(ShopOffer o, string name) =>
            o.Empty || o.UnitsLeft <= 0 ? ShopSoldOutText : (o.RecipeId != null ? "Recipe: " + name : name) + "\n" + PriceText(o);

        // Tag rects (screen px, y up) just above each model top, as wide as their text needs but never wider than the gap to the nearest other tag, inside the area.
        // With models (each slot's model on screen, width 0 = none), a tag also rises above a neighbouring model it would cover (R2 fix 1).
        // Allocation-free: longestLine[i] is the char count of tag i's longest line; the result goes into the caller's rects.
        private static float Want(int chars, float areaWidth) => Mathf.Clamp(0.1f + 0.016f * chars, 0.34f, 0.8f) * areaWidth;   // = WarehouseLabelWidth for that many chars
        public static void ShopTagLayout(Vector2[] tops, int[] longestLine, Rect area, float height, float gap, Rect[] rects, Rect[] models = null)
        {
            float spacing = float.MaxValue;
            for (int i = 0; i < tops.Length; i++) for (int j = i + 1; j < tops.Length; j++) spacing = Mathf.Min(spacing, Mathf.Abs(tops[i].x - tops[j].x));
            // Tags that would be squeezed below their wanted width alternate rows (user, November 25, 2026): the first up, the
            // next down, the next up again. Same-row neighbours are then two slots apart, so each tag gets about twice the room.
            bool stagger = false;
            // Only spread counters (more than three offers) stagger: the three-offer counters keep their tested single row clear of the lamps.
            for (int i = 0; i < tops.Length && tops.Length > 3; i++) if (Want(longestLine[i], area.width) > spacing - gap) stagger = true;
            float room = Mathf.Max(0f, (stagger ? 2f * spacing : spacing) - gap);
            float lower = float.MinValue;   // staggered: two shared rows, the lower one just clear of the tallest model or its top
            if (stagger)
            {
                int perRow = (tops.Length + 1) / 2;   // the upper row holds the extra tag when the count is odd
                room = Mathf.Min(room, Mathf.Max(0f, (area.width - (perRow - 1) * gap) / perRow));   // a full row always fits the screen
                for (int i = 0; i < tops.Length; i++) lower = Mathf.Max(lower, Mathf.Max(tops[i].y, models != null && i < models.Length && models[i].width > 0f ? models[i].yMax : float.MinValue) + gap);
                lower = Mathf.Min(lower, area.yMax - 2f * height - gap);
            }
            for (int i = 0; i < tops.Length; i++)
            {
                float width = Mathf.Min(Want(longestLine[i], area.width), room);
                float lift = stagger && i % 2 == 0 ? height + gap : 0f;   // slots 1, 3, 5 (index 0, 2, 4) take the upper row
                float y = stagger ? lower + lift : tops[i].y + gap;
                rects[i] = new Rect(WarehouseLabelLeft(tops[i].x, width, area.xMin, area.xMax), Mathf.Clamp(y, area.yMin, area.yMax - height), width, height);
            }
            // An edge tag held inside the screen can slide over its row neighbour: pack each row apart, left to right, then back
            // from the right edge. The slots run along the counter, so index order is screen order (either direction).
            if (stagger && tops.Length > 2)
            {
                bool rising = tops[tops.Length - 1].x >= tops[0].x;
                for (int row = 0; row < 2; row++)
                {
                    int first = rising ? row : LastOfRow(tops.Length, row), step = rising ? 2 : -2;
                    for (int i = first + step, prev = first; i >= 0 && i < tops.Length; prev = i, i += step)
                        rects[i].x = Mathf.Max(rects[i].x, rects[prev].xMax + gap);
                    int last = rising ? LastOfRow(tops.Length, row) : row;
                    rects[last].x = Mathf.Min(rects[last].x, area.xMax - rects[last].width);
                    for (int i = last - step, next = last; i >= 0 && i < tops.Length; next = i, i -= step)
                        rects[i].x = Mathf.Max(area.xMin, Mathf.Min(rects[i].x, rects[next].x - gap - rects[i].width));
                }
            }
            for (int i = 0; i < tops.Length && models != null && !stagger; i++)
                for (int j = 0; j < models.Length; j++)
                    if (j != i && models[j].width > 0f && models[j].Overlaps(rects[i])) rects[i].y = Mathf.Min(models[j].yMax + gap, area.yMax - height);
        }
        private static int LastOfRow(int count, int row) => (count - 1 - row) % 2 == 0 ? count - 1 : count - 2;

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
        // cash is the whole cash charged (negative: paid by the player), as the camp trade records it. A gold offer records gold
        // instead (cash 0; unitMinor/totalMinor in whole gold); a recipe offer learns the recipe (regions: the effective set) and records its id once.
        // boxReserved: recipes held by unclaimed mystery boxes (MysteryBoxes.Reserved), never sold here.
        public static bool BuyAndRecord(ShopState s, string shopId, int slot, int revision, int units, Wallet wallet, SpatialInventory bag, TheTrailWalker.Domain.Crafting.CraftingState crafting, ICollection<int> regions, System.Func<string> newId, List<TradeRecord> log, System.DateTime utcNow, out string reason, ISet<string> boxReserved = null)
        {
            bool recipe = s.Stock.TryGetValue(shopId, out var held) && slot >= 0 && slot < held.Offers.Length && held.Offers[slot].RecipeId != null;
            if (recipe) units = 1;
            if (recipe ? !ShopService.TryBuyRecipe(s, slot, revision, wallet, crafting, regions, out reason, boxReserved) : !ShopService.TryBuy(s, shopId, slot, revision, units, wallet, bag, newId, out reason)) return false;
            ShopOffer o = s.Stock[shopId].Offers[slot];
            var r = new TradeRecord { utc = utcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture), settlementId = shopId, itemId = recipe ? o.RecipeId : o.ItemId, result = ShopBoughtResult,
                quantity = units, quality = recipe ? 0 : ShopService.Q, unitMinor = o.UnitPriceMinor };
            if (o.Currency == Currency.Gold) { r.totalMinor = ShopService.GoldTotal(o, units); r.gold = -r.totalMinor; }
            else { r.totalMinor = units * o.UnitPriceMinor; r.cash = -Money.Charge(r.totalMinor); }
            TradeLog.Add(log, r);
            return true;
        }

        // The raw CustomerCameraAnchor view crops the outer offers on a portrait screen: turn toward the three offer anchors, then
        // the warehouse solver pitches and pulls back until all three sit in the band below the debug buttons (LumberCounterPose).
        // With spots (a spread counter), the first count spots are framed instead of OfferAnchor1-3.
        public static Pose ShopCounterPose(Camera cam, Transform counter, Transform[] spots = null, int count = 0)
        {
            Transform eye = FindChild(counter, "CustomerCameraAnchor");
            if (eye == null) return new Pose(counter.position + new Vector3(0f, 1.55f, 3f), Quaternion.LookRotation(Vector3.back));
            var targets = new List<Bounds>();
            for (int i = 1; i <= (spots != null ? count : 3); i++)
            {
                Transform a = spots != null ? spots[i - 1] : FindChild(counter, "OfferAnchor" + i);
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
            { ShopCatalog.Ranch, "Ranch|ShopkeeperRancher" }, { ShopCatalog.Fishery, "Fishery|ShopkeeperFisher" }, { ShopCatalog.Bakery, "Bakery|ShopkeeperBaker" },
        };

        private List<int> ShopRegions()
        {
            var regions = new List<int>();
            foreach (string id in travel.DiscoveredRegions) { RegionDefinition r = scene.Catalog.Region(id); if (r != null) regions.Add(r.Number); }
            return regions;
        }
        private ShopOffer[] ShopOffers() => ShopService.Offers(shops, shopId, ShopRegions(), crafting, MysteryBoxes.Reserved(balloon));

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
            int slots = ShopService.Offers(shops, id, ShopRegions(), crafting, MysteryBoxes.Reserved(balloon)).Length;   // shopId is set below: count the opening shop's stock
            if (shopHud == null) CreateShopHud();
            SetOfferSlotCount(slots); offerMaxWidth = OfferMaxWidth(slots);
            System.Array.Clear(shopAnchors, 0, shopAnchors.Length);
            for (int i = 0; i < 3; i++)
            {
                shopAnchors[i] = FindChild(counter, "OfferAnchor" + (i + 1));
                if (shopAnchors[i] != null) foreach (Transform baked in shopAnchors[i]) baked.gameObject.SetActive(false);   // baked display props make way for the offers
            }
            if (slots > 3 && shopAnchors[0] != null && shopAnchors[2] != null)
            {
                Vector3[] spots = OfferSpots(counter.InverseTransformPoint(shopAnchors[0].position), counter.InverseTransformPoint(shopAnchors[2].position), slots);
                Quaternion facing = Quaternion.Inverse(counter.rotation) * shopAnchors[0].rotation;
                for (int i = 0; i < slots; i++)
                {
                    var spot = new GameObject("OfferSpot" + (i + 1)).transform; spot.SetParent(counter, false); spot.localPosition = spots[i]; spot.localRotation = facing;
                    shopAnchors[i] = spot;
                }
            }
            shopHolder = holder; shopId = id;

            Pose pose = ShopCounterPose(cameraView, counter, shopAnchors, slots);
            StreetView.HoldAt(pose.position, pose.rotation);

            shopHud.gameObject.SetActive(true); CloseShopSheet(); SetCounterChrome(true);
            ShowScreenReturn(shopHud.gameObject, CloseShop);
            for (int i = 0; i < slots; i++) RefreshShopSlot(i);
            RefreshWalletLabels();
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
            ReleaseUnusedArt();
        }

        // Every exit (Return, and ResetHomeView).
        private void TearDownShop()
        {
            if (!ShopOpen) return;
            SetCounterChrome(false);
            CloseShopSheet();
            if (Application.isPlaying) Destroy(shopHolder.gameObject); else DestroyImmediate(shopHolder.gameObject);
            shopHolder = null; shopId = null; lumberPointer = null; offerMaxWidth = 0f;
            System.Array.Clear(shopAnchors, 0, shopAnchors.Length); System.Array.Clear(shopModels, 0, shopModels.Length);
            if (shopHud != null) shopHud.gameObject.SetActive(false);
            if (shopHud != null && screenReturnOwner == shopHud.gameObject) HideScreenReturn();
        }

        // The slot's item model on its anchor, or the Sold out label when the slot is empty or bought out.
        private void RefreshShopSlot(int slot)
        {
            ShopOffer o = ShopOffers()[slot];
            ItemDefinition def = o.Empty ? null : TheTrailWalker.Domain.Crafting.Items.Find(o.ItemId);
            SetOfferSlot(slot, OfferTagText(o, def != null ? def.Name : o.ItemId), o.Empty || o.UnitsLeft <= 0, def == null ? null : (System.Func<Transform, GameObject>)(a => PlaceOfferModel(a, def, offerMaxWidth)));
        }

        // Shared by the shop and balloon counters: the slot's tag (bold name line) and its model (placed on the anchor unless sold out).
        private void SetOfferSlot(int slot, string text, bool soldOut, System.Func<Transform, GameObject> place)
        {
            if (shopModels[slot] != null) { if (Application.isPlaying) Destroy(shopModels[slot]); else DestroyImmediate(shopModels[slot]); shopModels[slot] = null; }
            Text tag = shopSoldOutLabels[slot];
            int nl = text.IndexOf('\n');
            tag.text = nl < 0 ? text : "<b>" + text.Substring(0, nl) + "</b>" + text.Substring(nl);
            shopTagChars[slot] = LongestLine(text); shopModelTopY[slot] = float.NaN; shopModelBounds[slot] = default;
            tag.color = soldOut ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
            tag.transform.parent.gameObject.SetActive(shopAnchors[slot] != null);
            if (soldOut || shopAnchors[slot] == null || place == null) return;
            shopModels[slot] = place(shopAnchors[slot]);
            if (shopModels[slot] == null) return;
            var bounds = new Bounds(shopAnchors[slot].position, Vector3.zero);
            foreach (Renderer r in shopModels[slot].GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            shopModelTopY[slot] = bounds.max.y; shopModelBounds[slot] = bounds;   // the model never moves while the counter is open
        }

        // The trail's model at the trail's size (largest front side = the footprint), standing on the anchor; maxWidth > 0 shrinks
        // it to fit a spread counter (more than three offers).
        public static GameObject PlaceOfferModel(Transform anchor, ItemDefinition def, float maxWidth = 0f)
        {
            var probe = new GameObject("Offer " + def.Id, typeof(MeshRenderer), typeof(MeshFilter));   // AttachPath hides this renderer
            probe.transform.SetParent(anchor, false);
            GameObject visual = ResourceVisualTuning.Attach(probe, def);
            if (visual == null) return probe;   // no art yet: nothing to show (the sheet still sells it)
            Bounds b = LocalMeshBounds(probe.transform, visual);
            float scale = ResourceVisualTuning.World(def) * Mathf.Max(def.Width, def.Height) / 1000f / Mathf.Max(0.01f, Mathf.Max(b.size.x, b.size.y));
            if (maxWidth > 0f) scale = Mathf.Min(scale, maxWidth / Mathf.Max(0.01f, Mathf.Max(b.size.x, b.size.z)));
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
            CreateWalletLabels(shopHud);
            for (int i = 0; i < ShopService.MaxOffers; i++)
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
            if (shopSheet == null) CreateShopSheet();   // the reward sheet may have built it first (MENU, Boxes)
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
            bool recipe = o.RecipeId != null, gold = o.Currency == Currency.Gold;
            // A recipe offer has no amounts: one PAY while it lasts, and PAY's refusals (gold, already known) go on the note.
            int[] choices = recipe ? (o.UnitsLeft > 0 ? new[] { 1 } : new int[0]) : gold ? GoldAmountChoices(o, travel.Gold) : AmountChoices(o.UnitsLeft, CashMinor, o.UnitPriceMinor);
            if (System.Array.IndexOf(choices, shopAmount) < 0) shopAmount = choices.Length > 0 ? choices[0] : 0;   // default: the smallest
            ClearContent(shopSheetPage);
            ItemDefinition def = TheTrailWalker.Domain.Crafting.Items.Find(o.ItemId);
            Image icon = Rect("Picture", shopSheetPage, 0.2f, 0.56f, 0.8f, 0.98f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
            Sprite sprite = def != null ? ItemIconRenderer.For(def) : null;
            if (sprite != null) icon.sprite = sprite; else icon.color = Color.clear;
            Text name = Label(shopSheetPage, def != null ? def.Name : o.ItemId, 0.04f, 0.46f, 0.96f, 0.55f, 34); name.color = Ink; name.fontStyle = FontStyle.Bold;
            name.resizeTextForBestFit = true; name.resizeTextMinSize = 14; name.resizeTextMaxSize = 34;
            Label(shopSheetPage, OfferLine(o, shopAmount), 0.04f, 0.39f, 0.96f, 0.46f, 26).color = FadedInk;
            Label(shopSheetPage, LeftLine(o), 0.04f, 0.32f, 0.96f, 0.39f, 26).color = FadedInk;
            if (choices.Length == 0)
            {
                Text none = Label(shopSheetPage, o.UnitsLeft <= 0 ? ShopSoldOutText : gold ? "Not enough gold" : "Not enough cash", 0.04f, 0.10f, 0.96f, 0.26f, 28);
                none.color = FadedInk; none.fontStyle = FontStyle.Italic;
                return;
            }
            const float x0 = 0.04f, x1 = 0.96f, gap = 0.03f;
            float w = (x1 - x0 - gap * (choices.Length - 1)) / choices.Length;
            for (int i = 0; i < choices.Length && !recipe; i++)
            {
                int units = choices[i];
                Text b = Button(shopSheetPage, units.ToString("N0", System.Globalization.CultureInfo.InvariantCulture), x0 + i * (w + gap), 0.17f, x0 + i * (w + gap) + w, 0.27f, () => { shopAmount = units; RefreshShopSheet(); });
                if (units == shopAmount) { b.transform.parent.GetComponent<Image>().color = new Color(0.72f, 0.46f, 0.12f); b.fontStyle = FontStyle.Bold; }   // the chosen amount
            }
            Text pay = Button(shopSheetPage, "PAY", 0.06f, 0f, 0.94f, 0.12f, ShopPayTapped);
            pay.text = PayText(o, shopAmount);
        }

        private void ShopPayTapped()
        {
            if (shopSheetSlot < 0 || !ShopOpen || saveBlocked) return;
            int slot = shopSheetSlot;
            long cashBefore = travel.Cash, goldBefore = travel.Gold;
            ShopOffer o = ShopOffers()[slot];
            if (!BuyAndRecord(shops, shopId, slot, shopSheetRevision, shopAmount, travel.Wallet, ledger, crafting, ShopService.EffectiveRegions(shops, ShopRegions()), () => "shop-" + System.Guid.NewGuid().ToString("N"), warehouseLog, System.DateTime.UtcNow, out string reason, MysteryBoxes.Reserved(balloon))) { ShowSettlementNote(reason); return; }
            statistics.SpendCash(cashBefore - travel.Cash); statistics.SpendGold(goldBefore - travel.Gold);
            shopSheetRevision = o.Revision;   // the sheet now shows the new stock
            Redraw(); SaveNow();   // one save: the payment, the stock and (for a recipe) the unlock land together
            if (o.RecipeId != null) { ItemDefinition food = TheTrailWalker.Domain.Crafting.Items.Find(o.ItemId); ShowSettlementNote("Recipe learned: " + (food != null ? food.Name : o.ItemId)); }
            RefreshShopSlot(slot); RefreshShopSheet();
            RefreshWalletLabels();
        }

        // Per frame from UpdateSettlement: the wallet labels catch any change, and the price tags follow their anchors on screen.
        private void UpdateShops()
        {
            if (ShopOpen && cameraView != null) UpdateOfferTags();
        }
        // Shared by the shop and balloon counters (their offers fill shopAnchors, shopModelTopY and shopModelBounds).
        private void UpdateOfferTags()
        {
            UpdateWalletLabels();
            safe.GetWorldCorners(shopSafeCorners);   // overlay canvas: screen pixels
            float scale = CanvasScale;
            for (int i = 0; i < offerSlots; i++)
            {
                shopShown[i] = false;
                if (shopAnchors[i] == null) continue;
                Vector3 a = shopAnchors[i].position;
                Vector3 p = cameraView.WorldToScreenPoint(float.IsNaN(shopModelTopY[i]) ? new Vector3(a.x, a.y + 0.05f, a.z) : new Vector3(a.x, shopModelTopY[i], a.z));
                shopShown[i] = p.z > 0f; shopTops[i] = p;
            }
            for (int i = 0; i < offerSlots; i++) shopModelRects[i] = ScreenBoundsRect(cameraView, shopModelBounds[i]);
            ShopTagLayout(shopTops, shopTagChars, UnityEngine.Rect.MinMaxRect(shopSafeCorners[0].x, shopSafeCorners[0].y, shopSafeCorners[2].x, shopSafeCorners[2].y), 76f * scale, Screen.height * 0.01f, shopTagRects, shopModelRects);
            for (int i = 0; i < offerSlots; i++)
            {
                RectTransform plate = shopTagPlates[i];
                plate.localScale = shopShown[i] ? Vector3.one : Vector3.zero;
                if (!shopShown[i]) { shopTagRects[i] = default; continue; }
                plate.sizeDelta = new Vector2(shopTagRects[i].width / scale, 76f);
                plate.position = new Vector3(shopTagRects[i].x, shopTagRects[i].y, 0f);
            }
        }

        // A bounds' screen rect (px, y up) from its 8 corners; width 0 for empty bounds or any corner behind the camera. Allocation-free.
        public static Rect ScreenBoundsRect(Camera cam, Bounds b)
        {
            if (b.size == Vector3.zero) return default;
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            for (int k = 0; k < 8; k++)
            {
                Vector3 v = cam.WorldToScreenPoint(b.center + Vector3.Scale(b.extents, new Vector3((k & 1) == 0 ? -1 : 1, (k & 2) == 0 ? -1 : 1, (k & 4) == 0 ? -1 : 1)));
                if (v.z <= 0f) return default;
                x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
            }
            return UnityEngine.Rect.MinMaxRect(x0, y0, x1, y1);
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
            int pick = OfferTapPick(screen);
            if (pick >= 0) OpenShopSheet(pick);
        }
        // Shared by the shop and balloon counters: the offer slot under a tap (its anchor or tag), or -1.
        private int OfferTapPick(Vector2 screen)
        {
            var anchors = new Vector2?[offerSlots];
            for (int i = 0; i < offerSlots; i++) if (LumberAnchorDistance(shopAnchors[i], screen) < float.MaxValue) anchors[i] = cameraView.WorldToScreenPoint(shopAnchors[i].position);
            return ShopTapPick(screen, anchors, shopTagRects, LumberReach);
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
