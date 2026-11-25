using System.Collections.Generic;
using TheTrailWalker.Domain.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Phone round R2 (user, November 21, 2026): at every counter (the six shops, the warehouse, the lumber counter) the cash
    // carried shows on a long paper label centred just above the bag (in the HUD-square row, between the Return square and the
    // hidden user menu), and the gold on one at the top centre. The icon says which; the amount follows it. Each counter HUD
    // builds its own pair, so the labels hide with that HUD on every teardown path.
    public sealed partial class CollectionPrototype
    {
        public const float WalletLabelWidth = 300f, WalletLabelHeight = 56f, WalletTopGap = 8f;   // canvas units: room for 999,999,999
        private readonly List<Text> walletCashTexts = new List<Text>(), walletGoldTexts = new List<Text>();
        private long walletShownCash = -1, walletShownGold = -1;
        private float walletBarLine = -1f;

        // Safe-area canvas rects (origin bottom left): the cash label sits on the HUD squares' bar line, the gold label under the top edge.
        public static Rect WalletCashRect(float safeWidth, float safeHeight, float barLine) =>
            new Rect((safeWidth - WalletLabelWidth) * 0.5f, barLine * safeHeight, WalletLabelWidth, WalletLabelHeight);
        public static Rect WalletGoldRect(float safeWidth, float safeHeight) =>
            new Rect((safeWidth - WalletLabelWidth) * 0.5f, safeHeight - WalletTopGap - WalletLabelHeight, WalletLabelWidth, WalletLabelHeight);

        // A counter view owns the screen like the home views: the prototype title, the status lines and the mid-screen debug buttons
        // (UNLOCK CAMPS, AUTO SORT, LEARN T1 RECIPES, CLAIM OVERFLOW) step aside while it is open, RESET SAVE stays (controller
        // rulings, R2 fix 1), and all come back on every teardown path (each TearDown* calls this with false).
        private void SetCounterChrome(bool counterOpen)
        {
            if (titleLabel != null) titleLabel.gameObject.SetActive(!counterOpen);
            if (status != null) status.gameObject.SetActive(!counterOpen);
            foreach (GameObject button in counterHiddenButtons) button.SetActive(!counterOpen);
        }

        // Called once per counter HUD, as it is built.
        private void CreateWalletLabels(RectTransform parent)
        {
            walletCashTexts.Add(WalletLabel(parent, "Cash label", "cash", out _));
            walletGoldTexts.Add(WalletLabel(parent, "Gold label", "gold", out RectTransform gold));
            gold.anchorMin = gold.anchorMax = gold.pivot = new Vector2(0.5f, 1f); gold.anchoredPosition = new Vector2(0f, -WalletTopGap);
            walletShownCash = -1;   // the next refresh fills the new pair
        }
        private static Text WalletLabel(RectTransform parent, string name, string icon, out RectTransform rect)
        {
            rect = Rect(name, parent, 0, 0, 0, 0);
            rect.pivot = new Vector2(0.5f, 0f); rect.sizeDelta = new Vector2(WalletLabelWidth, WalletLabelHeight);
            Sheet("Paper", rect, "paper_1", 0f, 0f, 1f, 1f, 0f).raycastTarget = false;
            HudIconStyle.ApplyTo(Rect("Icon", rect, 0.03f, 0.04f, 0.21f, 0.96f), icon, false);
            Text amount = Label(rect, "", 0.22f, 0.08f, 0.96f, 0.92f, 32);
            amount.color = Ink; amount.fontStyle = FontStyle.Bold;
            amount.resizeTextForBestFit = true; amount.resizeTextMinSize = 14; amount.resizeTextMaxSize = 32;
            return amount;
        }

        // After every purchase, deposit, payout or unlock, and on opening: the amounts, and the cash strip on the current bar line.
        private void RefreshWalletLabels()
        {
            if (travel == null) return;
            walletShownCash = travel.Cash; walletShownGold = travel.Gold; walletBarLine = menuBarLine;
            string cash = Money.Cash(walletShownCash), gold = Money.Cash(walletShownGold);
            for (int i = 0; i < walletCashTexts.Count; i++)
            {
                walletCashTexts[i].text = cash; walletGoldTexts[i].text = gold;
                var rect = (RectTransform)walletCashTexts[i].transform.parent;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, walletBarLine); rect.anchoredPosition = Vector2.zero;
            }
        }
        // Per frame from the counters: allocation-free unless the wallet or the bar line changed.
        private void UpdateWalletLabels()
        {
            if (travel != null && (travel.Cash != walletShownCash || travel.Gold != walletShownGold || menuBarLine != walletBarLine)) RefreshWalletLabels();
        }
    }
}
