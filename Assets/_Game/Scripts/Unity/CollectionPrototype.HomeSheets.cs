using System.Collections.Generic;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheTrailWalker
{
    public sealed partial class CollectionPrototype
    {
        // The store menu (2a task 8; one menu with Furniture and Storage since the home revision's Task 5), checked by
        // HomeValidation.StoreChecks. Buffs stay in the background: a row
        // shows its purpose in words and its price, never a buff number, total or counter.
        public const string StoreNoSpaceNote = "No space; it's in house storage.";
        public const string StorageNoSpaceNote = "No space for this piece";
        public const string StorageEmptyNote = "House storage is empty.";
        public static string PurposeLine(BuffCategory c)
        {
            switch (c)
            {
                case BuffCategory.CraftQuality: return "Helps craft finer goods";
                case BuffCategory.FoodRecovery: return "Meals restore more";
                case BuffCategory.SalePrice: return "Crafted goods sell for more";
                case BuffCategory.BootsLife: return "New boots last longer";
                case BuffCategory.ShirtLife: return "New shirts last longer";
                case BuffCategory.HatLife: return "New hats last longer";
                case BuffCategory.PantsLife: return "New trousers last longer";
                case BuffCategory.AxeLife: return "New axes last longer";
                case BuffCategory.HuntingToolLife: return "New hunting tools last longer";
                case BuffCategory.RodLife: return "New rods last longer";
                case BuffCategory.BackpackLife: return "New backpacks last longer";
                default: return "Holds your items";   // Storage
            }
        }
        // A piece's own purpose line: the tool rack has no buff category of its own to describe, the rest follow their category.
        public static string PurposeLine(FurnitureDefinition d) => d.IsRack ? "Holds tools and spare gear" : PurposeLine(d.Category);
        // The store lists every type unlocked at the home's level, in catalog order.
        public static IEnumerable<FurnitureDefinition> StoreRows(int houseLevel) => FurnitureCatalog.UnlockedAt(houseLevel);
        public static string StoreIconPath(FurnitureDefinition d) => "UI/Furniture/" + d.Asset;
        public static string StorePriceLabel(FurnitureDefinition d) => Money.Cash(d.PriceCash) + " cash";
        public static string StoreBuyPrompt(FurnitureDefinition d) => "Buy " + d.Name + " for " + Money.Cash(d.PriceCash) + " cash?";
        // A row's check: at least one piece of the type stands in the room.
        public static bool StoreRowPlaced(HomeState home, FurnitureDefinition d)
        {
            if (home == null || d == null) return false;
            foreach (FurniturePiece p in home.Pieces) if (p.Placed && p.DefinitionId == d.Id) return true;
            return false;
        }
        public static List<FurniturePiece> StoredPieces(HomeState home)
        {
            var stored = new List<FurniturePiece>();
            if (home != null) foreach (FurniturePiece p in home.Pieces) if (!p.Placed) stored.Add(p);
            return stored;
        }
        // Instance ids follow the HUD's other generated ids (a prefix and a GUID, as "trade-", "gear-").
        public static string NewFurnitureInstanceId() => "furniture-" + System.Guid.NewGuid().ToString("N");

        // What a sheet action did: Bought (store only), the piece to select in the bird's-eye view, or a paper note.
        public struct HomeSheetResult { public bool Bought; public string Select; public string Note; }
        // A confirmed buy: refused with CanBuy's reason; bought and placed (select it); or bought into storage (the note).
        public static HomeSheetResult StoreBuy(HomeState home, FurnitureDefinition d, Wallet wallet, string newInstanceId, TheTrailWalker.Domain.PlayerStatistics stats = null)
        {
            if (home == null) return new HomeSheetResult { Note = "Unknown furniture" };
            if (!home.CanBuy(d, wallet, out string reason)) return new HomeSheetResult { Note = reason };
            FurniturePiece piece = home.Buy(d, wallet, newInstanceId);
            stats?.SpendCash(d.PriceCash);
            return piece.Placed ? new HomeSheetResult { Bought = true, Select = piece.InstanceId } : new HomeSheetResult { Bought = true, Note = StoreNoSpaceNote };
        }
        // A tap on a stored piece: placed at the nearest free spot (select it), or the no-space note.
        public static HomeSheetResult StorageTap(HomeState home, string instanceId)
        {
            if (home != null && instanceId != null && home.PlaceFromStorage(instanceId)) return new HomeSheetResult { Select = instanceId };
            return new HomeSheetResult { Note = StorageNoSpaceNote };
        }

        // ---------------------------------------------------------------- the store menu (home revision, Task 5)

        public enum StoreSection { Furniture, Storage }
        public static readonly string[] StoreSectionCaptions = { "Furniture", "Storage" };
        public static readonly string[] StoreSectionIcons = { "store", "storage" };
        // The Furniture catalog: every house level 1-8 in order with its six types; levels above the house's are locked (shown,
        // never buyable: CanBuy refuses them with the same words as LockedLine).
        public static IEnumerable<(int level, IReadOnlyList<FurnitureDefinition> types, bool locked)> CatalogGroups(int houseLevel)
        {
            for (int level = 1; level <= HomeState.MaxLevel; level++)
            {
                var types = new List<FurnitureDefinition>();
                foreach (FurnitureDefinition d in FurnitureCatalog.All) if (d.HouseLevel == level) types.Add(d);
                yield return (level, types, level > houseLevel);
            }
        }
        public static string CatalogHeader(int level) => "House level " + level;
        public static string LockedLine(int level) => "Unlocks at house level " + level;

        private GameObject homeStoreMenu;
        private RectTransform homeStoreContent, homeStoreTabs;
        private ScrollRect homeStoreScroll;
        private StoreSection homeStoreSection;
        private System.Action homeBuyConfirm;   // the YES action of a buy confirmation on screen, or null
        private bool HomeSheetsOpen => (homeStoreMenu != null && homeStoreMenu.activeSelf) || UpgradeSheetOpen;
        private bool HomeInside => homeView == HomeView.Interior || homeView == HomeView.BirdsEye;
        private static readonly Color HomeRowBand = new Color(1f, 0.97f, 0.88f, 0.38f), HomeLockedBand = new Color(0.86f, 0.82f, 0.74f, 0.30f),
            HomeTickInk = new Color(0.2f, 0.45f, 0.18f), HomeLockInk = new Color(0.42f, 0.32f, 0.2f, 0.8f);
        private const float HomeLockedAlpha = 0.45f;

        // One paper sheet over a dim backdrop (a tap on the backdrop closes it; the paper itself never does): the two section
        // tabs at its top, a Close square (the return icon) at full HUD size and a vertically scrolling page.
        private void CreateStoreMenu()
        {
            RectTransform root = Rect("Home store menu", safe, 0, 0, 1, 1);
            RectTransform backdrop = Rect("Backdrop", root, 0, 0, 1, 1);
            Image dim = backdrop.gameObject.AddComponent<Image>(); dim.color = new Color(0f, 0f, 0f, 0.35f);   // blocks every touch behind the sheet
            var away = backdrop.gameObject.AddComponent<UnityEngine.UI.Button>(); away.transition = Selectable.Transition.None; away.onClick.AddListener(CloseHomeSheets);
            Image paper = Sheet("Sheet", root, "paper_1", 0.05f, 0.13f, 0.95f, 0.87f, 0f);
            Transform p = paper.transform;
            homeStoreTabs = Rect("Section tabs", p, 0.04f, 0.885f, 0.78f, 0.975f);
            Text close = Button(p, "Close", 0, 0, 0, 0, CloseHomeSheets);
            var closeRect = (RectTransform)close.transform.parent; closeRect.name = "Home: Close store";
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.97f, 0.975f); closeRect.pivot = new Vector2(1f, 1f);
            closeRect.sizeDelta = new Vector2(HudSquare, HudSquare); closeRect.anchoredPosition = Vector2.zero;
            HudIconStyle.Apply(close, "return");
            RectTransform page = Rect("Page", p, 0.05f, 0.04f, 0.95f, 0.87f);
            homeStoreScroll = page.gameObject.AddComponent<ScrollRect>();
            RectTransform viewport = Rect("Viewport", page, 0, 0, 1, 1); viewport.gameObject.AddComponent<RectMask2D>();
            Image catcher = viewport.gameObject.AddComponent<Image>(); catcher.color = new Color(0, 0, 0, 0);   // drag anywhere to scroll
            homeStoreContent = Rect("Content", viewport, 0, 1, 1, 1); homeStoreContent.pivot = new Vector2(0.5f, 1f);
            homeStoreScroll.viewport = viewport; homeStoreScroll.content = homeStoreContent; homeStoreScroll.horizontal = false;
            homeStoreScroll.movementType = ScrollRect.MovementType.Clamped;
            homeStoreMenu = root.gameObject;
            homeStoreMenu.SetActive(false);
            CreateUpgradeSheet();
        }

        // Only inside the house. Nothing is remembered between openings: the interior's Store opens on Furniture, the
        // bird's-eye Storage on Storage, both at the top.
        private void OpenStoreMenu(StoreSection section)
        {
            if (!HomeInside || saveBlocked || homeStoreMenu == null) return;
            CancelInteriorPointer();
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            ShowStoreSection(section);
            homeStoreMenu.transform.SetAsLastSibling(); homeStoreMenu.SetActive(true);
        }

        // A section (from opening or a tab tap): its tabs and page rebuilt, scrolled to the top.
        // A buy confirmation still on screen is answered NO first: it belongs to the page being left.
        private void ShowStoreSection(StoreSection section)
        {
            DismissBuyConfirm();
            homeStoreSection = section;
            BuildStoreTabs();
            if (section == StoreSection.Storage) BuildStorageSection(); else BuildFurnitureSection();
            homeStoreScroll.StopMovement();
            homeStoreContent.anchoredPosition = Vector2.zero;
        }

        // The menu closes, and a buy confirmation still on screen is answered NO (TearDownInterior calls this on every exit).
        private void CloseHomeSheets()
        {
            DismissBuyConfirm();
            CloseUpgradeSheet();
            if (homeStoreMenu != null) homeStoreMenu.SetActive(false);
        }
        private void DismissBuyConfirm()
        {
            if (homeBuyConfirm != null && settlementConfirm != null && settlementConfirm.activeSelf && settlementConfirmed == homeBuyConfirm) CloseSettlementConfirm(false);
            homeBuyConfirm = null;
        }

        // The two tabs, as the mission book's: the selected one stands taller and brighter.
        private void BuildStoreTabs()
        {
            ClearContent(homeStoreTabs);
            for (int i = 0; i < StoreSectionCaptions.Length; i++)
            {
                var section = (StoreSection)i;
                bool selected = section == homeStoreSection;
                float x0 = i * 0.5f + 0.01f;
                RectTransform tab = Rect("Home: " + StoreSectionCaptions[i] + " tab", homeStoreTabs, x0, selected ? 0f : 0.12f, x0 + 0.47f, 1f);
                Image plate = tab.gameObject.AddComponent<Image>();
                Sprite tabSprite = Paper("tab");
                if (tabSprite != null) plate.sprite = tabSprite;
                plate.color = selected ? Color.white : new Color(0.86f, 0.8f, 0.68f);
                tab.gameObject.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { if (HomeSheetsOpen && homeStoreSection != section) ShowStoreSection(section); });
                RectTransform iconRect = Rect("Icon", tab, 0.05f, 0.14f, 0.30f, 0.90f);
                iconRect.gameObject.AddComponent<Image>().color = Color.clear;
                HudIconStyle.ApplyTo(iconRect, StoreSectionIcons[i], false);
                Text name = Label(tab, StoreSectionCaptions[i].ToUpperInvariant(), 0.32f, 0.10f, 0.97f, 0.90f, 22);
                name.color = selected ? Ink : FadedInk; name.fontStyle = FontStyle.Bold; name.alignment = TextAnchor.MiddleLeft;
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 10; name.resizeTextMaxSize = 22;
            }
        }

        private static void ClearContent(RectTransform content)
        {
            for (int i = content.childCount - 1; i >= 0; i--) { GameObject child = content.GetChild(i).gameObject; if (Application.isPlaying) Destroy(child); else DestroyImmediate(child); }
        }
        private static Image IconImage(RectTransform parent, FurnitureDefinition d, float x0, float y0, float x1, float y1)
        {
            Image icon = Rect("Icon", parent, x0, y0, x1, y1).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
            Sprite sprite = Resources.Load<Sprite>(StoreIconPath(d));
            if (sprite != null) icon.sprite = sprite; else icon.color = new Color(0, 0, 0, 0);
            return icon;
        }

        // The catalog: per house level a header, then its six rows (icon, name, purpose line, price, a drawn check when a piece
        // of the type stands in the room). Locked rows keep their price but are dimmed, carry a lock and the locked line, and
        // take no taps.
        private void BuildFurnitureSection()
        {
            ClearContent(homeStoreContent);
            float y = 0f; const float rowHeight = 116f, headerHeight = 62f;
            foreach ((int level, IReadOnlyList<FurnitureDefinition> types, bool locked) in CatalogGroups(home.Level))
            {
                RectTransform header = Rect("Level " + level, homeStoreContent, 0, 1, 1, 1); header.pivot = new Vector2(0.5f, 1f);
                header.anchoredPosition = new Vector2(0f, -y); header.sizeDelta = new Vector2(0f, headerHeight); y += headerHeight + 6f;
                Text title = Label(header, CatalogHeader(level), 0.01f, 0.08f, 0.99f, 0.92f, 26);
                title.color = locked ? FadedInk : Ink; title.fontStyle = FontStyle.Bold; title.alignment = TextAnchor.LowerLeft;
                Image rule = Rect("Rule", header, 0f, 0f, 1f, 0f).gameObject.AddComponent<Image>();
                rule.rectTransform.sizeDelta = new Vector2(0f, 2f); rule.color = new Color(FadedInk.r, FadedInk.g, FadedInk.b, 0.5f); rule.raycastTarget = false;
                foreach (FurnitureDefinition d in types)
                {
                    RectTransform row = Rect(d.Id, homeStoreContent, 0, 1, 1, 1); row.pivot = new Vector2(0.5f, 1f);
                    row.anchoredPosition = new Vector2(0f, -y); row.sizeDelta = new Vector2(0f, rowHeight - 8f); y += rowHeight;
                    Image band = row.gameObject.AddComponent<Image>(); band.color = locked ? HomeLockedBand : HomeRowBand;
                    if (locked) band.raycastTarget = false;   // no tap; a drag falls through to the page and scrolls
                    else
                    {
                        FurnitureDefinition chosen = d;
                        var button = row.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = band; button.onClick.AddListener(() => StoreRowTapped(chosen));
                    }
                    Image icon = IconImage(row, d, 0.01f, 0.05f, 0.19f, 0.95f);
                    if (locked && icon.sprite != null) icon.color = new Color(1f, 1f, 1f, HomeLockedAlpha);
                    Text name = Label(row, d.Name, 0.22f, 0.50f, 0.70f, 0.95f, 23); name.color = Dim(Ink, locked); name.fontStyle = FontStyle.Bold; name.alignment = TextAnchor.MiddleLeft;
                    name.resizeTextForBestFit = true; name.resizeTextMinSize = 12; name.resizeTextMaxSize = 23;
                    Text purpose = Label(row, locked ? LockedLine(level) : PurposeLine(d), 0.22f, 0.05f, 0.84f, 0.50f, 18); purpose.color = FadedInk; purpose.alignment = TextAnchor.MiddleLeft;
                    if (locked) purpose.fontStyle = FontStyle.Italic;
                    purpose.resizeTextForBestFit = true; purpose.resizeTextMinSize = 11; purpose.resizeTextMaxSize = 18;
                    Text price = Label(row, StorePriceLabel(d), 0.68f, 0.50f, 0.98f, 0.95f, 20); price.color = Dim(Ink, locked); price.alignment = TextAnchor.MiddleRight;
                    price.resizeTextForBestFit = true; price.resizeTextMinSize = 11; price.resizeTextMaxSize = 20;
                    if (locked) LockMark(row);
                    else if (StoreRowPlaced(home, d)) PlacedTick(row);
                }
                y += 10f;
            }
            homeStoreContent.sizeDelta = new Vector2(0f, y);
        }
        private static Color Dim(Color c, bool locked) => locked ? new Color(c.r, c.g, c.b, HomeLockedAlpha) : c;

        // A check mark drawn from two strokes (the HUD font has no check glyph), bottom right of the row.
        private static void PlacedTick(RectTransform row)
        {
            RectTransform box = Rect("Placed", row, 1f, 0f, 1f, 0f); box.pivot = new Vector2(1f, 0f);
            box.sizeDelta = new Vector2(44f, 44f); box.anchoredPosition = new Vector2(-12f, 6f);
            void Stroke(float cx, float cy, float length, float angle)
            {
                RectTransform r = Rect("Stroke", box, 0.5f, 0.5f, 0.5f, 0.5f);
                r.sizeDelta = new Vector2(length, 6f); r.anchoredPosition = new Vector2(cx, cy); r.localRotation = Quaternion.Euler(0f, 0f, angle);
                Image i = r.gameObject.AddComponent<Image>(); i.color = HomeTickInk; i.raycastTarget = false;
            }
            Stroke(-9f, -3f, 17f, -45f);
            Stroke(5f, 3f, 30f, 55f);
        }

        // A small padlock drawn from blocks (a shackle over a body with a keyhole), where a placed row has its check.
        private static void LockMark(RectTransform row)
        {
            RectTransform box = Rect("Locked", row, 1f, 0f, 1f, 0f); box.pivot = new Vector2(1f, 0f);
            box.sizeDelta = new Vector2(40f, 44f); box.anchoredPosition = new Vector2(-14f, 6f);
            Image Block(string name, float x0, float y0, float x1, float y1, Color c)
            {
                Image i = Rect(name, box, x0, y0, x1, y1).gameObject.AddComponent<Image>(); i.color = c; i.raycastTarget = false; return i;
            }
            Block("Shackle left", 0.22f, 0.50f, 0.34f, 0.92f, HomeLockInk);
            Block("Shackle right", 0.66f, 0.50f, 0.78f, 0.92f, HomeLockInk);
            Block("Shackle top", 0.22f, 0.84f, 0.78f, 0.96f, HomeLockInk);
            Block("Body", 0.08f, 0.0f, 0.92f, 0.56f, HomeLockInk);
            Block("Keyhole", 0.45f, 0.14f, 0.55f, 0.38f, new Color(1f, 0.97f, 0.88f, 0.9f));
        }

        // A row tap: short of cash says so; otherwise the paper YES/NO confirmation (the settlement's).
        private void StoreRowTapped(FurnitureDefinition d)
        {
            if (!HomeInside || saveBlocked || !HomeSheetsOpen) return;
            if (!home.CanBuy(d, travel.Wallet, out string reason)) { ShowSettlementNote(reason); return; }
            homeBuyConfirm = () => { homeBuyConfirm = null; ConfirmStoreBuy(d); };
            AskSettlement(StoreBuyPrompt(d), homeBuyConfirm);
        }

        // YES: buy, save, close the sheet, then the bird's-eye view with the new piece selected, or the storage note.
        private void ConfirmStoreBuy(FurnitureDefinition d)
        {
            if (!HomeInside || saveBlocked) return;
            HomeSheetResult result = StoreBuy(home, d, travel.Wallet, NewFurnitureInstanceId(), statistics);
            if (!result.Bought) { ShowSettlementNote(result.Note); return; }
            SaveNow(true);
            CloseHomeSheets();
            if (result.Select != null) ShowPieceInBirdsEye(result.Select);
            if (result.Note != null) ShowSettlementNote(result.Note);
        }

        // The Storage section: the stored pieces' icons in a grid of four (2a's storage sheet).
        private void BuildStorageSection()
        {
            ClearContent(homeStoreContent);
            List<FurniturePiece> stored = StoredPieces(home);
            const int columns = 4; const float tileHeight = 150f;
            if (stored.Count == 0)
            {
                RectTransform none = Rect("Empty", homeStoreContent, 0, 1, 1, 1); none.pivot = new Vector2(0.5f, 1f); none.sizeDelta = new Vector2(0f, 120f);
                Text empty = Label(none, StorageEmptyNote, 0, 0, 1, 1, 22); empty.color = FadedInk;
                homeStoreContent.sizeDelta = new Vector2(0f, 120f);
                return;
            }
            for (int i = 0; i < stored.Count; i++)
            {
                FurniturePiece piece = stored[i];
                FurnitureDefinition d = piece.Definition;
                int col = i % columns, line = i / columns;
                RectTransform tile = Rect(piece.InstanceId, homeStoreContent, (float)col / columns, 1f, (float)(col + 1) / columns, 1f);
                tile.pivot = new Vector2(0.5f, 1f); tile.sizeDelta = new Vector2(-10f, tileHeight - 10f); tile.anchoredPosition = new Vector2(0f, -line * tileHeight);
                Image band = tile.gameObject.AddComponent<Image>(); band.color = HomeRowBand;
                string id = piece.InstanceId;
                var button = tile.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = band; button.onClick.AddListener(() => StorageTileTapped(id));
                IconImage(tile, d, 0.08f, 0.26f, 0.92f, 0.96f);
                Text name = Label(tile, d.Name, 0.04f, 0.02f, 0.96f, 0.26f, 16); name.color = Ink;
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 10; name.resizeTextMaxSize = 16;
            }
            homeStoreContent.sizeDelta = new Vector2(0f, ((stored.Count + columns - 1) / columns) * tileHeight);
        }

        // A stored piece tapped: placed at the nearest free spot, saved, and selected in the bird's-eye view; or the note.
        private void StorageTileTapped(string instanceId)
        {
            if (!HomeInside || saveBlocked || !HomeSheetsOpen) return;
            HomeSheetResult result = StorageTap(home, instanceId);
            if (result.Select == null) { ShowSettlementNote(result.Note); return; }
            SaveNow();
            CloseHomeSheets();
            ShowPieceInBirdsEye(result.Select);
        }

        // A piece just placed by the store or storage: its view joins the room, then the bird's-eye view opens with it selected
        // (the interior hold's entry), or, already in that view, the selection moves to it.
        private void ShowPieceInBirdsEye(string instanceId)
        {
            FurniturePiece piece = HomePiece(instanceId);
            if (piece == null || !piece.Placed || interiorRoot == null) return;
            CancelInteriorPointer();
            if (!pieceViews.ContainsKey(instanceId))
            {
                GridAnchors(interiorRoot, out Vector3 o, out Vector3 ax, out Vector3 az);
                pieceViews[instanceId] = InstancePiece(interiorRoot, piece, o, ax, az);
            }
            if (homeView == HomeView.Interior) EnterBirdsEye(instanceId);
            else if (homeView == HomeView.BirdsEye) SelectPiece(instanceId);
        }
    }
}
