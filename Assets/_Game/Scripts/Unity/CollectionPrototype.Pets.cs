using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Pets;
using TheTrailWalker.Domain.Settlement;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 15 slice 1 (spec 2026-11-24): the backyard behind the house (the Pet button, house level 3+), the paper pet
    // shop and pet home sheets, and the active pet on its bed in the house entry area. Checked by Milestone15Validation.
    public sealed partial class CollectionPrototype
    {
        // ---------------------------------------------------------------- words (checked)

        public static string PetGroupName(SupplyGroup g)
        {
            switch (g)
            {
                case SupplyGroup.Wood: return "Wood";
                case SupplyGroup.StoneOre: return "Stone";
                case SupplyGroup.Fibers: return "Fibre";
                case SupplyGroup.Produce: return "Produce";
                case SupplyGroup.Seeds: return "Seeds";
                default: return "Truffle";
            }
        }
        // What the pet finds in the discovered regions, largest share first ("Wood 47% · Stone 42% · Produce 5%").
        public static string PetEffectiveLine(PetDefinition pet, ICollection<int> regions)
        {
            Dictionary<SupplyGroup, double> w = PetSupplies.Effective(pet, regions);
            var parts = w.Where(kv => kv.Value > 0).OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key)
                .Select(kv => PetGroupName(kv.Key) + " " + Mathf.RoundToInt((float)(kv.Value * 100)) + "%").ToList();
            return parts.Count > 0 ? string.Join(" · ", parts) : "Nothing to find yet";
        }
        public static string PetSlotsLine(PetState s, int houseLevel) => s.Owned.Count + " of " + PetState.Slots(houseLevel);
        // A shop row's right-hand words: Owned, the price, or why it cannot be bought now.
        public static string PetRowStatus(PetState s, PetDefinition pet, int houseLevel, Wallet wallet)
        {
            if (s.Owns(pet.Id)) return "Owned";
            if (s.CanBuy(pet, houseLevel, wallet, out string reason) || reason == "Not enough cash") return Money.Cash(pet.PriceCash) + " cash";
            return reason;
        }
        public static string PetBuyPrompt(PetState s, PetDefinition pet, int houseLevel) =>
            pet.Name + " stays with you. " + (s.Owned.Count + 1) + " of " + PetState.Slots(houseLevel) + " slots used.";
        public static string PetHomeLine(bool active, int waiting) => (active ? "With you" : "At home") + (waiting > 0 ? " · " + waiting + " waiting" : "");
        public static string CollectionTitle(int waiting) => "Collection · " + waiting + " / " + PetIdle.QueueSize;
        public static string FindLine(PetFind f) => Money.Cash(f.Quantity) + " · Q" + f.Quality;
        public const string CollectionEmpty = "Nothing waiting", CollectionNoRoom = "No room in the bag", ClaimAllCaption = "Claim all", ClaimCaption = "Claim";
        public static string PetModelFor(string petId, bool standing)
        {
            PetDefinition p = PetCatalog.Find(petId);
            return p == null ? null : standing ? p.StandingModel : p.SecondModel;
        }
        public static string PetIconPath(PetDefinition pet) => "UI/Pets/" + pet.Name;

        // ---------------------------------------------------------------- the backyard view

        public static readonly string[] BackyardButtonNames = { "Backyard: Shop", "Backyard: Pets", "Backyard: Return" };
        public static readonly string[] BackyardCaptions = { "Shop", "Pets", "Return" };
        public static readonly string[] BackyardIcons = { "store", "pet", "return" };
        public static readonly Vector3 BackyardEye = new Vector3(0f, 1.1f, 4.6f), BackyardLook = new Vector3(0f, 0.8f, -1.0f);
        private Transform backyardRoot;
        private RectTransform backyardHud;

        private void CreateBackyardHud()
        {
            backyardHud = Rect("Backyard HUD", safe, 0, 0, 1, 1);
            var actions = new UnityEngine.Events.UnityAction[] { () => OpenPetSheet(PetSheetMode.Shop), () => OpenPetSheet(PetSheetMode.Home), CloseBackyard };
            for (int i = 0; i < BackyardButtonNames.Length; i++)
            {
                Text label = Button(backyardHud, BackyardCaptions[i], 0, 0, 0, 0, actions[i]);
                var rect = (RectTransform)label.transform.parent; rect.name = BackyardButtonNames[i];
                rect.anchorMin = rect.anchorMax = new Vector2(1f, SettlementListTop); rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(HudSquare, HudSquare);
                rect.anchoredPosition = new Vector2(-HudMargin - (BackyardButtonNames.Length - 1 - i) * (HudSquare + HudGap), 0f);
                HudIconStyle.Apply(label, BackyardIcons[i]);
            }
            backyardHud.gameObject.SetActive(false);
            CreatePetSheet();
        }

        // The Pet button: leave the room (its views go, the backyard takes the same holder) and frame the yard.
        private void OpenBackyard()
        {
            if (homeView != HomeView.Interior || NeighbourView || saveBlocked || !HomePetButtonVisible(home.Level)) return;
            TearDownInterior();
            homeView = HomeView.Backyard;
            BuildBackyardScene();
            SetMenu(menuOpen);
        }

        // (Re)builds the yard and holds the camera on it; also after the active pet changes.
        private void BuildBackyardScene()
        {
            if (world == null) world = GetComponent<MovementPrototype>();
            backyardRoot = world != null ? world.BuildBackyard(home.Level, PetModelFor(pets.Active, true)) : null;
            if (backyardRoot == null || StreetView == null) return;
            StreetView.HoldAt(backyardRoot.TransformPoint(BackyardEye), backyardRoot.rotation * Quaternion.LookRotation(BackyardLook - BackyardEye));
        }

        // Return: back into the room, as a fresh interior (OpenHomeInterior needs no open view).
        private void CloseBackyard()
        {
            if (homeView != HomeView.Backyard) return;
            TearDownBackyard();
            if (world == null) world = GetComponent<MovementPrototype>();
            world?.ClearInterior();
            homeView = HomeView.None;
            OpenHomeInterior();
            ReleaseUnusedArt();
        }

        // Every exit (CloseBackyard, and TearDownInterior on leaving, a faint or travel): no sheet, no confirmation, no HUD.
        private void TearDownBackyard()
        {
            ClosePetSheet();
            backyardRoot = null;
            if (backyardHud != null) backyardHud.gameObject.SetActive(false);
        }

        private void UpdateBackyard(bool panelsCovered)
        {
            if (backyardHud == null) return;
            bool show = homeView == HomeView.Backyard && !panelsCovered && !PetSheetOpen;
            if (backyardHud.gameObject.activeSelf != show) backyardHud.gameObject.SetActive(show);
        }

        // ---------------------------------------------------------------- the pet shop and pet home sheets

        private GameObject petSheet;
        private RectTransform petContent;
        private ScrollRect petScroll;
        private Text petTitle;
        private enum PetSheetMode { Shop, Home, Collection }
        private PetSheetMode petSheetMode;
        private bool petShopShown => petSheetMode == PetSheetMode.Shop;
        private System.Action petBuyConfirm;
        private bool PetSheetOpen => petSheet != null && petSheet.activeSelf;

        // One paper sheet over a dim backdrop, like the store menu: a title, a Close square and a scrolling page.
        private void CreatePetSheet()
        {
            RectTransform root = Rect("Pet sheet", safe, 0, 0, 1, 1);
            RectTransform backdrop = Rect("Backdrop", root, 0, 0, 1, 1);
            backdrop.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);
            var away = backdrop.gameObject.AddComponent<UnityEngine.UI.Button>(); away.transition = Selectable.Transition.None; away.onClick.AddListener(ClosePetSheet);
            Image paper = Sheet("Sheet", root, "paper_1", 0.05f, 0.13f, 0.95f, 0.87f, 0f);
            Transform p = paper.transform;
            petTitle = Label(p, "", 0.05f, 0.885f, 0.78f, 0.975f, 30); petTitle.color = Ink; petTitle.fontStyle = FontStyle.Bold; petTitle.alignment = TextAnchor.MiddleLeft;
            Text close = Button(p, "Close", 0, 0, 0, 0, ClosePetSheet);
            var closeRect = (RectTransform)close.transform.parent; closeRect.name = "Backyard: Close sheet";
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.97f, 0.975f); closeRect.pivot = new Vector2(1f, 1f);
            closeRect.sizeDelta = new Vector2(HudSquare, HudSquare); closeRect.anchoredPosition = Vector2.zero;
            HudIconStyle.Apply(close, "return");
            RectTransform page = Rect("Page", p, 0.05f, 0.04f, 0.95f, 0.87f);
            petScroll = page.gameObject.AddComponent<ScrollRect>();
            RectTransform viewport = Rect("Viewport", page, 0, 0, 1, 1); viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            petContent = Rect("Content", viewport, 0, 1, 1, 1); petContent.pivot = new Vector2(0.5f, 1f);
            petScroll.viewport = viewport; petScroll.content = petContent; petScroll.horizontal = false;
            petScroll.movementType = ScrollRect.MovementType.Clamped;
            root.gameObject.SetActive(false);
            petSheet = root.gameObject;
        }

        private void OpenPetSheet(PetSheetMode mode)
        {
            bool where = mode == PetSheetMode.Collection ? homeView == HomeView.Interior && !NeighbourView : homeView == HomeView.Backyard;
            if (!where || saveBlocked || petSheet == null) return;
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            CancelInteriorPointer();
            petSheetMode = mode;
            RefreshPetSheet();
            petScroll.StopMovement(); petContent.anchoredPosition = Vector2.zero;
            petSheet.transform.SetAsLastSibling(); petSheet.SetActive(true);
        }

        private void ClosePetSheet()
        {
            if (petBuyConfirm != null && settlementConfirm != null && settlementConfirm.activeSelf && settlementConfirmed == petBuyConfirm) CloseSettlementConfirm(false);
            petBuyConfirm = null;
            if (petSheet != null) petSheet.SetActive(false);
        }

        private ICollection<int> PetRegions() => ShopService.EffectiveRegions(shops, ShopRegions());   // debug TIER +1 counts as discovery

        private void RefreshPetSheet()
        {
            ClearContent(petContent);
            float y = 0f;
            if (petSheetMode == PetSheetMode.Collection) { BuildCollectionPage(ref y); petContent.sizeDelta = new Vector2(0f, y); return; }
            if (petShopShown)
            {
                petTitle.text = "Pet shop · " + PetSlotsLine(pets, home.Level);
                ICollection<int> regions = PetRegions();
                foreach (PetDefinition pet in PetCatalog.All)
                {
                    PetDefinition chosen = pet;
                    bool owned = pets.Owns(pet.Id);
                    PetRow(pet, pet.Specialty, PetEffectiveLine(pet, regions), PetRowStatus(pets, pet, home.Level, travel.Wallet), owned, ref y, owned ? (System.Action)null : () => PetShopRowTapped(chosen));
                }
            }
            else
            {
                petTitle.text = "Your pets";
                if (pets.Owned.Count == 0)
                {
                    Text none = Label(petContent, "No pets yet", 0, 0, 1, 1, 26); none.color = FadedInk; none.fontStyle = FontStyle.Italic;
                    var r = (RectTransform)none.transform; r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(0.5f, 1f); r.sizeDelta = new Vector2(0, 120); y = 120;
                }
                foreach (string id in pets.Owned)
                {
                    PetDefinition pet = PetCatalog.Find(id);
                    if (pet == null) continue;
                    bool active = pets.Active == id;
                    PetRow(pet, pet.Specialty, PetHomeLine(active, pets.Idle.Waiting(id)), active ? "Active" : "", active, ref y, active ? (System.Action)null : () => PetHomeRowTapped(pet.Id));
                }
            }
            petContent.sizeDelta = new Vector2(0f, y);
        }

        private void PetRow(PetDefinition pet, string line1, string line2, string status, bool marked, ref float y, System.Action tap)
        {
            const float rowHeight = 170f;
            RectTransform row = Rect(pet.Id, petContent, 0, 1, 1, 1); row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -y); row.sizeDelta = new Vector2(0f, rowHeight - 8f); y += rowHeight;
            Image band = row.gameObject.AddComponent<Image>(); band.color = HomeRowBand;
            if (tap != null) { var b = row.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = band; b.onClick.AddListener(() => tap()); }
            else band.raycastTarget = false;
            Image icon = Rect("Icon", row, 0.01f, 0.05f, 0.21f, 0.95f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
            Sprite sprite = Resources.Load<Sprite>(PetIconPath(pet));
            if (sprite != null) icon.sprite = sprite; else icon.color = new Color(0, 0, 0, 0);
            Text name = Label(row, pet.Name, 0.24f, 0.62f, 0.66f, 0.96f, 25); name.color = Ink; name.fontStyle = FontStyle.Bold; name.alignment = TextAnchor.MiddleLeft;
            Text first = Label(row, line1, 0.24f, 0.34f, 0.98f, 0.62f, 19); first.color = FadedInk; first.alignment = TextAnchor.MiddleLeft;
            Text second = Label(row, line2, 0.24f, 0.04f, 0.98f, 0.34f, 18); second.color = FadedInk; second.alignment = TextAnchor.MiddleLeft;
            second.resizeTextForBestFit = true; second.resizeTextMinSize = 11; second.resizeTextMaxSize = 18;
            Text right = Label(row, status, 0.62f, 0.62f, 0.98f, 0.96f, 20); right.color = marked ? HomeTickInk : Ink; right.alignment = TextAnchor.MiddleRight;
            right.resizeTextForBestFit = true; right.resizeTextMinSize = 11; right.resizeTextMaxSize = 20;
        }

        private void PetShopRowTapped(PetDefinition pet)
        {
            if (!PetSheetOpen || !petShopShown || saveBlocked) return;
            if (!pets.CanBuy(pet, home.Level, travel.Wallet, out string reason)) { ShowSettlementNote(reason); return; }
            string prompt = PetBuyPrompt(pets, pet, home.Level);
            petBuyConfirm = () => { petBuyConfirm = null; ConfirmPetBuy(pet); };
            AskSettlement(prompt, petBuyConfirm);
        }

        // YES: TryBuy re-checks everything (a stale confirmation after the state changed is refused), then one save.
        private void ConfirmPetBuy(PetDefinition pet)
        {
            if (homeView != HomeView.Backyard || saveBlocked) return;
            if (!pets.TryBuy(pet, home.Level, travel.Wallet, System.DateTime.UtcNow, out string reason)) { ShowSettlementNote(reason); return; }
            statistics?.SpendCash(pet.PriceCash);
            SaveNow(true);
            if (PetSheetOpen) RefreshPetSheet();
        }

        // The pet home sheet: a tap makes that pet the active one (one save) and the yard shows it.
        private void PetHomeRowTapped(string petId)
        {
            if (!PetSheetOpen || petSheetMode != PetSheetMode.Home || saveBlocked || !pets.SetActive(petId)) return;
            SaveNow(true);
            BuildBackyardScene();
            RefreshPetSheet();
        }

        // ---------------------------------------------------------------- offline (slice 2)

        private bool petSettlePending;
        // The app goes away: the absence starts with the active pet and the discovered regions (nothing without a pet).
        private void BeginPetAbsence()
        {
            if (pets == null || !initialized) return;
            pets.Idle.BeginOffline(pets.Active, PetRegions(), System.DateTime.UtcNow);
        }
        // The app is back (or a save from a closed app was loaded): settle once, save before anything shows the finds.
        private void SettlePetAbsence()
        {
            if (pets == null) return;
            if (!initialized || saveBlocked) { petSettlePending = true; return; }   // not ready: settle on a later frame, never unsaved
            if (pets.Idle.OfflineStartedTicks == 0) { pets.Idle.Settle(System.DateTime.UtcNow, 0); return; }   // not away: the watermark only
            int made = pets.Idle.Settle(System.DateTime.UtcNow, (ulong)(uint)settlement.Seed);
            SaveNow(true);
            if (made > 0) RefreshCollectionBox();
        }

        // ---------------------------------------------------------------- the entry area (inside the house)

        public const string EntryBedName = "PetBed", EntryPetName = "EntryPet";
        // The shared bed at PetBedAnchor with the active pet's second pose on its cushion; nothing without an active pet, and
        // never in a neighbour's room.
        private void PlaceEntryPet(Transform interior)
        {
            PlaceCollectionBox(interior);
            if (interior == null || neighbourHome != null || pets.Active == null) return;
            Transform anchor = FindChild(interior, "PetBedAnchor");
            GameObject bedPrefab = Resources.Load<GameObject>("Settlement/Pets/PetBed"), petPrefab = Resources.Load<GameObject>(PetModelFor(pets.Active, false) ?? "");
            if (anchor == null || bedPrefab == null || petPrefab == null) return;
            GameObject bed = Instantiate(bedPrefab, anchor, false); bed.name = EntryBedName;
            Transform top = bed.transform.Find("PetAnchor");
            GameObject pet = Instantiate(petPrefab, top != null ? top : bed.transform, false); pet.name = EntryPetName;
        }

        // ---------------------------------------------------------------- the collection box (slice 2)

        public const string CollectionBoxName = "CollectionBox";
        private GameObject collectionBoxView;
        private Text collectionBadge;

        // The box stands at CollectionBoxAnchor once a pet is owned (a trigger box makes it tappable); never in a neighbour's room.
        private void PlaceCollectionBox(Transform interior)
        {
            collectionBoxView = null;
            if (interior == null || neighbourHome != null || pets.Owned.Count == 0) return;
            Transform anchor = FindChild(interior, "CollectionBoxAnchor");
            GameObject prefab = Resources.Load<GameObject>("Settlement/Houses/PetCollectionContainer");
            if (anchor == null || prefab == null) return;
            collectionBoxView = Instantiate(prefab, anchor, false); collectionBoxView.name = CollectionBoxName;
            var renderers = collectionBoxView.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds b = renderers[0].bounds; foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
            var hit = collectionBoxView.AddComponent<BoxCollider>(); hit.isTrigger = true;
            hit.center = collectionBoxView.transform.InverseTransformPoint(b.center); hit.size = b.size + Vector3.one * 0.1f;
        }

        private bool TapCollectionBox(Vector2 screen)
        {
            if (collectionBoxView == null || cameraView == null) return false;
            int count = Physics.RaycastNonAlloc(cameraView.ScreenPointToRay(screen), interiorHits, cameraView.farClipPlane, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++) if (interiorHits[i].collider != null && interiorHits[i].collider.gameObject == collectionBoxView) return true;
            return false;
        }

        // The count of waiting finds, a small paper tag over the box while the interior view shows it.
        private void UpdateCollectionBadge(bool show)
        {
            if (collectionBadge == null && homeInsideHud != null)
            {
                RectTransform tag = Rect("Collection badge", homeInsideHud, 0, 0, 0, 0); tag.sizeDelta = new Vector2(64f, 40f); tag.pivot = new Vector2(0.5f, 0f);
                Image back = tag.gameObject.AddComponent<Image>(); back.color = new Color(0.94f, 0.9f, 0.8f, 0.95f); back.raycastTarget = false;
                collectionBadge = Label(tag, "", 0, 0, 1, 1, 24); collectionBadge.color = Ink; collectionBadge.fontStyle = FontStyle.Bold;
            }
            if (collectionBadge == null) return;
            int waiting = pets.Active != null ? pets.Idle.Waiting(pets.Active) : 0;
            bool visible = show && collectionBoxView != null && waiting > 0 && cameraView != null;
            var rect = (RectTransform)collectionBadge.transform.parent;
            if (visible)
            {
                Vector3 sp = cameraView.WorldToScreenPoint(collectionBoxView.transform.position + Vector3.up * 0.7f);
                visible = sp.z > 0;
                if (visible)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(homeInsideHud, sp, null, out Vector2 local);
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.anchoredPosition = local;
                    collectionBadge.text = waiting.ToString();
                }
            }
            if (rect.gameObject.activeSelf != visible) rect.gameObject.SetActive(visible);
        }

        private void RefreshCollectionBox()
        {
            if (PetSheetOpen && petSheetMode != PetSheetMode.Shop) RefreshPetSheet();
        }

        // The collection page: the active pet's finds in queue order, Claim per find and Claim all at the top.
        private void BuildCollectionPage(ref float y)
        {
            string pet = pets.Active;
            IReadOnlyList<PetFind> finds = pet != null ? pets.Idle.Queue(pet) : new List<PetFind>();
            petTitle.text = CollectionTitle(finds.Count);
            if (finds.Count == 0)
            {
                Text none = Label(petContent, CollectionEmpty, 0, 0, 1, 1, 26); none.color = FadedInk; none.fontStyle = FontStyle.Italic;
                var r = (RectTransform)none.transform; r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(0.5f, 1f); r.sizeDelta = new Vector2(0, 120); y = 120;
                return;
            }
            RectTransform all = Rect("Claim all", petContent, 0, 1, 1, 1); all.pivot = new Vector2(0.5f, 1f); all.sizeDelta = new Vector2(0f, 84f); all.anchoredPosition = Vector2.zero; y = 96f;
            Text allButton = Button(all, ClaimAllCaption.ToUpperInvariant(), 0.55f, 0.05f, 0.99f, 0.95f, ClaimAllTapped);
            allButton.fontStyle = FontStyle.Bold;
            const float rowHeight = 130f;
            foreach (PetFind f in finds)
            {
                PetFind find = f;
                ItemShown(find.ItemId, out string name, out Sprite sprite);
                RectTransform row = Rect(find.EntryId, petContent, 0, 1, 1, 1); row.pivot = new Vector2(0.5f, 1f);
                row.anchoredPosition = new Vector2(0f, -y); row.sizeDelta = new Vector2(0f, rowHeight - 8f); y += rowHeight;
                row.gameObject.AddComponent<Image>().color = HomeRowBand;
                Image icon = Rect("Icon", row, 0.01f, 0.06f, 0.18f, 0.94f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
                if (sprite != null) icon.sprite = sprite; else icon.color = new Color(0, 0, 0, 0);
                Text label = Label(row, name, 0.21f, 0.50f, 0.70f, 0.95f, 23); label.color = Ink; label.fontStyle = FontStyle.Bold; label.alignment = TextAnchor.MiddleLeft;
                label.resizeTextForBestFit = true; label.resizeTextMinSize = 12; label.resizeTextMaxSize = 23;
                Text detail = Label(row, FindLine(find), 0.21f, 0.06f, 0.70f, 0.50f, 19); detail.color = FadedInk; detail.alignment = TextAnchor.MiddleLeft;
                Text claim = Button(row, ClaimCaption.ToUpperInvariant(), 0.72f, 0.12f, 0.99f, 0.88f, () => ClaimTapped(find.EntryId));
                claim.fontStyle = FontStyle.Bold;
            }
        }

        private static void ItemShown(string itemId, out string name, out Sprite sprite)
        {
            var def = TheTrailWalker.Domain.Crafting.Items.Find(itemId);
            name = def != null ? def.Name : itemId;
            sprite = def != null ? ItemIconRenderer.For(ItemArt.ModelFor(def)) : null;
        }

        private void ClaimTapped(string entryId)
        {
            if (!PetSheetOpen || petSheetMode != PetSheetMode.Collection || saveBlocked || pets.Active == null) return;
            if (!pets.Idle.Claim(pets.Active, entryId, ledger)) { ShowSettlementNote(CollectionNoRoom); return; }
            SaveNow(true);
            RefreshPetSheet();
        }

        private void ClaimAllTapped()
        {
            if (!PetSheetOpen || petSheetMode != PetSheetMode.Collection || saveBlocked || pets.Active == null) return;
            int before = pets.Idle.Waiting(pets.Active);
            int claimed = pets.Idle.ClaimAll(pets.Active, ledger);
            if (claimed > 0) SaveNow(true);
            if (claimed < before) ShowSettlementNote(CollectionNoRoom);
            RefreshPetSheet();
        }
    }
}
