using System;
using TheTrailWalker.Domain.Settlement;
using TheTrailWalker.Travel;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    public sealed partial class CollectionPrototype
    {
        // The house upgrade sheet (slice 2b task 3; spec section 3), checked by HomeValidation.UpgradeSheetChecks. One paper sheet in
        // the store menu's style: a title, one row per material (icon, name, deposited / required, where to earn it, DEPOSIT or
        // DONE) and Close. Deposits are final, so each confirmed deposit is saved before the sheet refreshes.
        public static bool HomeUpgradeOpensSheet(int level) => level < HomeState.PlayerMaxLevel;
        public const string HomeUpgradeCapNote = "Your house is as big as it gets for now.";
        public static string UpgradeTitle(int level) => $"House level {level}  →  {level + 1}";
        public static string UpgradeCount(int deposited, int required) => $"{deposited} / {required}";
        public static string DepositPrompt(int units, string name) => $"Deposit {units} {name}? Deposits can't be taken back.";

        // The trigger's tap shows the cap note only in the player's own interior and only when the sheet does not open.
        public static bool HomeUpgradeTapShowsCapNote(bool interior, bool neighbour, int level) => interior && !neighbour && !HomeUpgradeOpensSheet(level);

        // Opening the sheet on a fully deposited house finishes the upgrade instead (see OpenUpgradeSheet).
        public static bool UpgradeOpenCompletes(bool saveBlocked, bool ready, bool canUpgrade) => !saveBlocked && ready && canUpgrade;

        private GameObject homeUpgradeSheet;
        private RectTransform homeUpgradeContent;
        private Text homeUpgradeTitle;
        private Action homeDepositConfirm;   // the YES action of a deposit confirmation on screen, or null
        private bool UpgradeSheetOpen => homeUpgradeSheet != null && homeUpgradeSheet.activeSelf;

        private void CreateUpgradeSheet()
        {
            RectTransform root = Rect("Home upgrade sheet", safe, 0, 0, 1, 1);
            RectTransform backdrop = Rect("Backdrop", root, 0, 0, 1, 1);
            Image dim = backdrop.gameObject.AddComponent<Image>(); dim.color = new Color(0f, 0f, 0f, 0.35f);
            var away = backdrop.gameObject.AddComponent<UnityEngine.UI.Button>(); away.transition = Selectable.Transition.None; away.onClick.AddListener(CloseHomeSheets);
            Image paper = Sheet("Sheet", root, "paper_1", 0.05f, 0.13f, 0.95f, 0.87f, 0f);
            Transform p = paper.transform;
            homeUpgradeTitle = Label(p, "", 0.05f, 0.885f, 0.78f, 0.975f, 28);
            homeUpgradeTitle.color = Ink; homeUpgradeTitle.fontStyle = FontStyle.Bold; homeUpgradeTitle.alignment = TextAnchor.MiddleLeft;
            homeUpgradeTitle.resizeTextForBestFit = true; homeUpgradeTitle.resizeTextMinSize = 14; homeUpgradeTitle.resizeTextMaxSize = 28;
            Text close = Button(p, "Close", 0, 0, 0, 0, CloseHomeSheets);
            var closeRect = (RectTransform)close.transform.parent; closeRect.name = "Home: Close upgrade";
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.97f, 0.975f); closeRect.pivot = new Vector2(1f, 1f);
            closeRect.sizeDelta = new Vector2(HudSquare, HudSquare); closeRect.anchoredPosition = Vector2.zero;
            HudIconStyle.Apply(close, "return");
            homeUpgradeContent = Rect("Page", p, 0.05f, 0.04f, 0.95f, 0.87f);
            homeUpgradeSheet = root.gameObject;
            homeUpgradeSheet.SetActive(false);
        }

        private void OpenUpgradeSheet()
        {
            if (!HomeInside || NeighbourView || saveBlocked || homeUpgradeSheet == null || !HomeUpgradeOpensSheet(home.Level)) return;
            // A house whose last deposit was saved but whose completion was not (a blocked save, a kill between the two writes)
            // would otherwise show five DONE rows forever: finish it here.
            if (UpgradeOpenCompletes(saveBlocked, HouseUpgradeService.Ready(home, ledger), HomeUpgradeOpensSheet(home.Level))) { CompleteHouseUpgrade(); return; }
            CancelInteriorPointer();
            if (settlementNote != null && settlementNote.activeSelf) settlementNote.SetActive(false);
            RefreshUpgradeSheet();
            homeUpgradeSheet.transform.SetAsLastSibling(); homeUpgradeSheet.SetActive(true);
        }

        // A deposit confirmation still on screen is answered NO, as the buy confirmation is.
        private void CloseUpgradeSheet()
        {
            if (homeDepositConfirm != null && settlementConfirm != null && settlementConfirm.activeSelf && settlementConfirmed == homeDepositConfirm) CloseSettlementConfirm(false);
            homeDepositConfirm = null;
            if (homeUpgradeSheet != null) homeUpgradeSheet.SetActive(false);
        }

        // Rebuilt on opening and after each deposit, never per frame: the service rebuilds a ledger map per call, so each row's
        // numbers are read once here.
        private void RefreshUpgradeSheet()
        {
            if (homeUpgradeContent == null) return;
            ClearContent(homeUpgradeContent);
            if (!HomeUpgradeOpensSheet(home.Level)) { homeUpgradeTitle.text = ""; return; }
            homeUpgradeTitle.text = UpgradeTitle(home.Level);
            const float rowHeight = 132f;
            float y = 0f;
            foreach (UpgradeRequirement r in HouseUpgrades.For(home.Level + 1))
            {
                TheTrailWalker.Domain.Items.ItemDefinition definition = TheTrailWalker.Domain.Crafting.Items.Find(r.ItemId);
                string name = definition != null ? definition.Name : r.ItemId;
                int deposited = HouseUpgradeService.Deposited(home, ledger, r.ItemId);
                int remaining = Mathf.Max(0, r.Quantity - deposited);
                int available = HouseUpgradeService.Available(ledger, r.ItemId);
                RectTransform row = Rect(r.ItemId, homeUpgradeContent, 0, 1, 1, 1); row.pivot = new Vector2(0.5f, 1f);
                row.anchoredPosition = new Vector2(0f, -y); row.sizeDelta = new Vector2(0f, rowHeight - 8f); y += rowHeight;
                Image band = row.gameObject.AddComponent<Image>(); band.color = HomeRowBand; band.raycastTarget = false;
                Image icon = Rect("Icon", row, 0.01f, 0.05f, 0.17f, 0.95f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
                Sprite sprite = definition != null ? ItemIconRenderer.For(ItemArt.ModelFor(definition)) : null;
                if (sprite != null) icon.sprite = sprite; else icon.color = new Color(0, 0, 0, 0);
                Text nameText = Label(row, name, 0.19f, 0.55f, 0.62f, 0.95f, 24); nameText.color = Ink; nameText.fontStyle = FontStyle.Bold; nameText.alignment = TextAnchor.MiddleLeft;
                nameText.resizeTextForBestFit = true; nameText.resizeTextMinSize = 12; nameText.resizeTextMaxSize = 24;
                Text count = Label(row, UpgradeCount(deposited, r.Quantity), 0.62f, 0.72f, 0.98f, 0.98f, 24); count.color = Ink; count.alignment = TextAnchor.MiddleRight;
                Text source = Label(row, r.Source, 0.19f, 0.05f, 0.62f, 0.55f, 18); source.color = FadedInk; source.alignment = TextAnchor.MiddleLeft;
                source.resizeTextForBestFit = true; source.resizeTextMinSize = 11; source.resizeTextMaxSize = 18;
                if (remaining == 0)
                {
                    Text done = Label(row, "DONE", 0.64f, 0.02f, 0.98f, 0.70f, 22); done.color = HomeTickInk; done.fontStyle = FontStyle.Bold; done.alignment = TextAnchor.MiddleRight;
                    continue;
                }
                string itemId = r.ItemId; int units = Mathf.Min(remaining, available);
                Text deposit = Button(row, "DEPOSIT", 0, 0, 0, 0, () => DepositTapped(itemId, name));
                var depositRect = (RectTransform)deposit.transform.parent; depositRect.name = "Deposit " + r.ItemId;
                // 0.70 of the 124-unit row is 87 units, over the 84-unit (48 dp) touch minimum
                depositRect.anchorMin = new Vector2(0.64f, 0.0f); depositRect.anchorMax = new Vector2(0.98f, 0.70f); depositRect.offsetMin = depositRect.offsetMax = Vector2.zero;
                deposit.fontStyle = FontStyle.Bold;
                if (units == 0)
                {
                    depositRect.GetComponent<UnityEngine.UI.Button>().interactable = false;
                    deposit.color = new Color(1f, 1f, 1f, 0.45f);
                }
            }
        }

        // DEPOSIT: the paper YES/NO confirmation for everything the bag can give, up to what is still needed.
        private void DepositTapped(string itemId, string name)
        {
            if (!HomeInside || saveBlocked || !UpgradeSheetOpen) return;
            int deposited = HouseUpgradeService.Deposited(home, ledger, itemId);
            int required = 0;
            foreach (UpgradeRequirement r in HouseUpgrades.For(home.Level + 1)) if (r.ItemId == itemId) required = r.Quantity;
            int units = Mathf.Min(Mathf.Max(0, required - deposited), HouseUpgradeService.Available(ledger, itemId));
            if (units <= 0) return;
            homeDepositConfirm = () => { homeDepositConfirm = null; ConfirmDeposit(itemId); };
            AskSettlement(DepositPrompt(units, name), homeDepositConfirm);
        }

        // YES: move the units, save at once (a deposit cannot be undone, so the save comes before anything else), refresh, and
        // finish the upgrade when every row is full.
        private void ConfirmDeposit(string itemId)
        {
            if (!HomeInside) return;
            if (saveBlocked) { ShowSettlementNote("Can't save right now."); return; }
            int moved = HouseUpgradeService.Deposit(home, ledger, itemId, () => "house-escrow-" + Guid.NewGuid().ToString("N"), out string reason);
            if (moved > 0) SaveNow(true);
            if (UpgradeSheetOpen) RefreshUpgradeSheet();
            if (moved == 0) { if (!string.IsNullOrEmpty(reason)) ShowSettlementNote(reason); return; }
            if (!saveBlocked && HouseUpgradeService.Ready(home, ledger)) CompleteHouseUpgrade();
        }

        private HouseFireworks houseFireworks;

        // The level-up, in this order: the all-or-nothing Complete, the save (the upgrade is final, so nothing on screen changes
        // before it is written), then the views: the sheet and the room go, the street is rebuilt with the new house, the camera
        // centres on it and the fireworks go off over its roof. If the save fails the state is still correct in memory (saveBlocked
        // stops further play), so the views change regardless.
        private void CompleteHouseUpgrade()
        {
            if (saveBlocked) return;
            if (!HouseUpgradeService.Complete(home, ledger)) return;
            SaveNow(true);
            CloseUpgradeSheet();
            homeView = HomeView.None;
            TearDownInterior();
            settlementListLayout = -1;
            RebuildStreetWith(SettlementScenery.HouseLevels(settlement, home.Level));
            StreetBuilding house = Street?.Find("plot." + settlement.PlotIndex);
            if (house != null && StreetView != null) StreetView.CentreOn(house.CentreX);
            if (StreetView != null) StreetView.SnapToStreet();   // after CentreOn; even without the building, the camera must leave the interior
            if (house != null && StreetView != null)
            {
                Vector3 top = house.Root.position + Vector3.up * 3f;
                var box = house.Root.GetComponent<BoxCollider>();
                if (box != null) top = house.Root.TransformPoint(box.center + new Vector3(0f, box.size.y / 2f, 0f));
                if (houseFireworks != null) houseFireworks.Stop();
                houseFireworks = HouseFireworks.Play(top + Vector3.up);
            }
            houseLevelPreview = 0;   // the debug preview no longer applies once the real level changed
            if (houseLevelButton != null) houseLevelButton.text = HouseLevelCaption;
            SetMenu(menuOpen);
        }
    }
}
