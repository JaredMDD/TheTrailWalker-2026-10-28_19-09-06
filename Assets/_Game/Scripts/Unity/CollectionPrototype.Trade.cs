using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Economy;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 9: the camp conveyor trade (plan section 12.1). A 2D belt panel over the camp view (user, November 8, 2026):
    // entry on the left, shredder on the right, the three traders above the belt at 25/50/75%, the player's sellable items
    // below with a quantity picker. Sitting at a camp opens a 60-second offer, shown on a worn paper note near the top edge
    // with START TRADING; the session then runs 30 s (only trade actions and Return work) and closes straight back to the
    // camp, without a summary screen. The wallet shows only inside the trade (and shops), never on the camp HUD.
    // Rules and settlement live in Domain.Economy.TradeSession; this file draws it and applies each settlement once.
    public sealed partial class CollectionPrototype
    {
        private TradeSession tradeSession;
        private List<TradeRecord> tradeLog = new List<TradeRecord>();
        private GameObject tradePanel, tradeNote;
        private Text tradeTimer, tradeWallet, tradeQuote, tradeFeedback, tradeNoteTitle, tradeNoteTimer;
        private RectTransform tradeBelt, tradeStockContent;
        private Button tradeListButton;
        private readonly Dictionary<string, RectTransform> beltViews = new Dictionary<string, RectTransform>();
        private readonly List<RectTransform> stockCells = new List<RectTransform>();
        private ItemEntry tradeSelected;
        private int tradeQuantity = 1, tradeStockRevision = -1;
        private const float BeltX0 = 0.03f, BeltX1 = 0.83f;

        private bool TradeActive => tradeSession != null && tradeSession.Active;
        private bool TradePanelOpen => tradePanel != null && tradePanel.activeSelf;
        private static string ItemName(string id) => TheTrailWalker.Domain.Crafting.Items.Find(id)?.Name ?? id;

        private void RestoreTrade(SessionSnapshot snapshot)
        {
            tradeLog = snapshot.tradeLog ?? new List<TradeRecord>();
            tradeSession = snapshot.trade != null && !string.IsNullOrEmpty(snapshot.trade.campId) ? new TradeSession(snapshot.trade) : null;
        }

        // ------------------------------------------------------------------ HUD

        private void CreateTradeHud()
        {
            // Offer note (user, November 8, 2026): worn paper near the top edge of the camp view. While the debug HUD exists it
            // sits over the debug title/status lines; it only shows during the 60-second offer.
            tradeNote = Rect("Trade offer note", safe, 0.17f, 0.865f, 0.83f, 0.995f).gameObject;
            Image paper = tradeNote.AddComponent<Image>();
            Sprite sheet = Resources.Load<Sprite>("UI/Missions/paper_1");
            if (sheet != null) paper.sprite = sheet; else paper.color = new Color(0.86f, 0.78f, 0.6f, 0.98f);
            var ink = new Color(0.27f, 0.17f, 0.09f);
            tradeNoteTitle = Label(tradeNote.transform, "", 0.06f, 0.62f, 0.94f, 0.92f, 22); tradeNoteTitle.color = ink;
            tradeNoteTimer = Label(tradeNote.transform, "", 0.06f, 0.40f, 0.94f, 0.64f, 20); tradeNoteTimer.color = ink;
            Button(tradeNote.transform, "START TRADING", 0.24f, 0.07f, 0.76f, 0.38f, StartTradeFromNote);
            tradeNote.SetActive(false);

            tradePanel = Rect("Camp trade", safe, 0, 0, 1, 1).gameObject;
            tradePanel.AddComponent<Image>().color = new Color(0.03f, 0.06f, 0.07f, 0.94f);   // blocks every HUD touch behind it
            Transform p = tradePanel.transform;
            Label(p, "CAMP TRADE", 0.03f, 0.93f, 0.50f, 0.98f, 28).alignment = TextAnchor.MiddleLeft;
            tradeTimer = Label(p, "", 0.50f, 0.93f, 0.97f, 0.98f, 30); tradeTimer.alignment = TextAnchor.MiddleRight;
            tradeWallet = Label(p, "", 0.03f, 0.885f, 0.97f, 0.93f, 22); tradeWallet.alignment = TextAnchor.MiddleLeft;
            for (int t = 0; t < 3; t++)
            {
                float x = Mathf.Lerp(BeltX0, BeltX1, TradeSession.TraderPositions[t]);
                RectTransform trader = Rect("Trader " + (t + 1), p, x - 0.09f, 0.76f, x + 0.09f, 0.87f);
                trader.gameObject.AddComponent<Image>().color = new Color(0.32f, 0.22f, 0.13f, 0.95f);
                Label(trader, "TRADER " + (t + 1), 0, 0.45f, 1, 1, 18);
                Label(trader, "buys some\nlistings", 0, 0, 1, 0.5f, 14).color = new Color(0.9f, 0.85f, 0.7f);
            }
            tradeBelt = Rect("Belt", p, BeltX0, 0.56f, BeltX1, 0.74f);
            tradeBelt.gameObject.AddComponent<Image>().color = new Color(0.22f, 0.22f, 0.21f, 1f);
            for (int i = 0; i <= 10; i++) { RectTransform tick = Rect("Slat", tradeBelt, i / 10f - 0.002f, 0, i / 10f + 0.002f, 1); tick.gameObject.AddComponent<Image>().color = new Color(0.32f, 0.32f, 0.3f); }
            RectTransform lane = Rect("Lanes", tradeBelt, 0, 0.49f, 1, 0.51f); lane.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f);
            Label(tradeBelt, "traders' goods (tap to buy)", 0.01f, 0.86f, 0.6f, 1f, 13).alignment = TextAnchor.UpperLeft;
            Label(tradeBelt, "your listings", 0.01f, 0.0f, 0.6f, 0.14f, 13).alignment = TextAnchor.LowerLeft;
            RectTransform shredder = Rect("Shredder", p, BeltX1, 0.55f, 0.97f, 0.75f);
            shredder.gameObject.AddComponent<Image>().color = new Color(0.45f, 0.14f, 0.1f, 1f);
            Label(shredder, "SHRED\n80%", 0, 0, 1, 1, 18);
            Label(p, "110%", BeltX0 - 0.02f, 0.515f, BeltX0 + 0.10f, 0.555f, 16);
            Label(p, "buy price falls along the belt", 0.2f, 0.515f, 0.66f, 0.555f, 15).color = new Color(0.75f, 0.8f, 0.8f);
            Label(p, "80%", BeltX1 - 0.10f, 0.515f, BeltX1 + 0.02f, 0.555f, 16);

            Label(p, "YOUR ITEMS: tap one, choose a quantity, LIST", 0.03f, 0.465f, 0.97f, 0.505f, 18).alignment = TextAnchor.MiddleLeft;
            RectTransform viewport = Rect("Stock", p, 0.03f, 0.34f, 0.97f, 0.465f);
            viewport.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.105f, 0.085f, 0.97f);
            viewport.gameObject.AddComponent<RectMask2D>();
            tradeStockContent = Rect("Stock content", viewport, 0, 0, 0, 1); tradeStockContent.pivot = new Vector2(0, 0.5f);
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.content = tradeStockContent; scroll.horizontal = true; scroll.vertical = false; scroll.movementType = ScrollRect.MovementType.Clamped;

            string[] steps = { "-100", "-10", "-1", "+1", "+10", "+100" }; int[] deltas = { -100, -10, -1, 1, 10, 100 };
            for (int i = 0; i < steps.Length; i++)
            {
                int delta = deltas[i]; float x0 = 0.03f + i * 0.118f;
                Button(p, steps[i], x0, 0.265f, x0 + 0.11f, 0.325f, () => ChangeTradeQuantity(delta));
            }
            Button(p, "ALL", 0.74f, 0.265f, 0.97f, 0.325f, () => ChangeTradeQuantity(int.MaxValue));
            tradeQuote = Label(p, "", 0.03f, 0.18f, 0.97f, 0.26f, 19); tradeQuote.alignment = TextAnchor.MiddleLeft;
            tradeQuote.resizeTextForBestFit = true; tradeQuote.resizeTextMinSize = 12; tradeQuote.resizeTextMaxSize = 19;
            tradeListButton = Button(p, "LIST ON BELT", 0.40f, 0.095f, 0.97f, 0.17f, ListSelected).transform.parent.GetComponent<Button>();
            // Return: the dedicated cancel (70% on unsold listings) while the belt runs; closes the summary afterwards.
            Text back = Button(p, "RETURN", 0.03f, 0.095f, 0.37f, 0.17f, ReturnFromTrade);
            back.text = "RETURN (sell rest at 70%)";
            tradeFeedback = Label(p, "", 0.03f, 0.02f, 0.97f, 0.09f, 18);
            tradeFeedback.resizeTextForBestFit = true; tradeFeedback.resizeTextMinSize = 11; tradeFeedback.resizeTextMaxSize = 18;

            tradePanel.SetActive(false);
        }

        // ------------------------------------------------------------------ opening

        private void StartTradeFromNote()
        {
            if (!milestone5 || travel == null || tradeSession == null || !tradeSession.Offering || !tradeSession.Matches(travel.CurrentCampId, travel.VisitNumber)) return;
            if (phase != TravelPhase.Seated || TravelBusy) { Say("You can't trade right now."); return; }
            var resources = new List<string>();
            foreach (string regionId in travel.DiscoveredRegions)
            {
                RegionDefinition region = scene.Catalog.Region(regionId);
                RegionPlanEntry plan = region == null ? null : RegionPlan.Find(region.Number);
                if (plan != null) resources.AddRange(plan.Featured);
            }
            tradeSession.Begin(TradeOffers.Generate(TradeSession.SeedFor(travel.CurrentCampId, travel.VisitNumber), resources));
            SaveNow(true);
            if (saveBlocked) return;
            ShowTradePanel();
        }

        private void ShowTradePanel()
        {
            SetMenu(false);
            if (inventoryOpen) ToggleInventory();
            if (detailsPanel != null) detailsPanel.SetActive(false);
            if (gearPanel != null) gearPanel.SetActive(false);
            if (mapPanel != null && mapPanel.activeSelf) CloseMap();
            CloseCraftPanelForTrade();
            router.CancelAll(); router.InteractionLocked = true;
            tradeSelected = null; tradeQuantity = 1; tradeStockRevision = -1;
            if (tradeNote != null) tradeNote.SetActive(false);
            tradePanel.transform.SetAsLastSibling(); tradePanel.SetActive(true);
            tradeFeedback.text = "Traders buy some of your listings as they pass (100%). The rest reach the shredder (80%).";
            RefreshTradePanel();
        }

        private void CloseTradePanel()
        {
            if (TradeActive) return;   // only Return ends a running session
            tradePanel.SetActive(false);
            foreach (RectTransform view in beltViews.Values) Destroy(view.gameObject);
            beltViews.Clear();
            router.InteractionLocked = stamina != null && stamina.Fainted;
            RefreshTravelHud();
        }

        private void CloseCraftPanelForTrade() { if (craftPanel != null && craftPanel.activeSelf) craftPanel.SetActive(false); }

        // ------------------------------------------------------------------ per frame

        private void UpdateTrade()
        {
            if (travel == null || saveBlocked) return;
            bool seated = phase == TravelPhase.Seated;
            // Each camp visit opens one offer; a session left running by an interrupted older save finishes naturally first.
            if (tradeSession != null && tradeSession.Active && (!tradeSession.Matches(travel.CurrentCampId, travel.VisitNumber) || (!seated && phase != TravelPhase.Transition)))
            {
                tradeSession.Data.elapsed = TradeSession.Duration;
                ApplySettlements(tradeSession.Advance(0.001f));
                if (tradeSession.Active) ApplySettlements(tradeSession.Cancel());
                if (TradePanelOpen) CloseTradePanel();
                return;
            }
            if (seated && (tradeSession == null || !tradeSession.Matches(travel.CurrentCampId, travel.VisitNumber))) { tradeSession = TradeSession.Offer(travel.CurrentCampId, travel.VisitNumber); lastSavedState = null; }
            if (tradeSession == null) return;
            // The offer counts down while seated in the foreground (pauses freeze it, like the session clock).
            if (tradeSession.Offering && seated && !TradePanelOpen && tradeSession.AdvanceOffer(Mathf.Min(Time.unscaledDeltaTime, 0.25f)))
            { SaveNow(); Say("The traders packed up. Trade again on your next camp visit."); }
            bool note = seated && tradeSession.Offering && !TradePanelOpen;
            if (tradeNote != null)
            {
                if (tradeNote.activeSelf != note) { tradeNote.SetActive(note); if (note) tradeNote.transform.SetAsLastSibling(); }
                if (note)
                {
                    int left = Mathf.CeilToInt(tradeSession.OfferRemaining);
                    tradeNoteTitle.text = "Traders at " + travel.CurrentCamp.Name;
                    tradeNoteTimer.text = "offer ends in " + (left / 60) + ":" + (left % 60).ToString("00");
                }
            }
            if (tradeSession.Active)
            {
                if (!TradePanelOpen) { ShowTradePanel(); return; }   // resumed after a restart or a pause
                // Pauses freeze the clock (Update does not run); a long first frame after resuming is capped.
                ApplySettlements(tradeSession.Advance(Mathf.Min(Time.unscaledDeltaTime, 0.25f)));
                if (!tradeSession.Active) { FinishTrade(); return; }
            }
            if (TradePanelOpen) RefreshTradePanel();
        }

        // No summary screen (user, November 8, 2026): the belt closes back to the camp with a one-line result.
        private void FinishTrade()
        {
            long earned = 0, spent = 0;
            foreach (TradeListing l in tradeSession.Data.listings) { if (l.owner == ListingOwner.Player) earned += l.cash; else if (l.result == ListingResult.BoughtByPlayer) spent += l.cash; }
            CloseTradePanel();
            Say("Trading finished: +" + Money.Cash(earned) + " cash from sales, " + Money.Cash(spent) + " spent.");
        }

        private void ApplySettlements(List<TradeSettlement> settled)
        {
            if (settled.Count == 0) return;
            long earned = 0;
            foreach (TradeSettlement s in settled)
            {
                TradeListing listing = s.Listing;
                if (listing.owner != ListingOwner.Player) continue;
                ItemEntry escrow = null; foreach (ItemEntry e in ledger.Entries) if (e.Id == listing.escrowId && e.Owner == ItemOwner.Escrow) { escrow = e; break; }
                if (escrow != null) ledger.SettleEscrow(escrow);
                travel.Wallet.Credit(Currency.Cash, listing.cash); statistics.AddCash(listing.cash); earned += listing.cash;
                TradeLog.Add(tradeLog, TradeRecord.From(tradeSession.Data, listing, s.Percent, DateTime.UtcNow));
                string how = listing.result == ListingResult.SoldToTrader ? $"Trader {listing.trader + 1} bought" : listing.result == ListingResult.Shredded ? "Shredded" : listing.result == ListingResult.Canceled ? "Returned" : "Time up for";
                tradeFeedback.text = $"{how} {ItemName(listing.itemId)} x{listing.quantity:N0}: +{Money.Cash(listing.cash)} cash ({s.Percent}%).";
            }
            if (!tradeSession.Active) WriteTradeReport();
            SaveNow(true);
        }

        // ------------------------------------------------------------------ actions

        private void ListSelected()
        {
            if (!TradeActive || tradeSelected == null) { tradeFeedback.text = "Tap one of your items first."; return; }
            if (!tradeSession.CanList(out string reason)) { tradeFeedback.text = reason; return; }
            int quantity = Mathf.Clamp(tradeQuantity, 1, tradeSelected.Quantity);
            if (!ledger.TryEscrow(tradeSelected, quantity, "escrow-" + Guid.NewGuid().ToString("N"), out ItemEntry escrow, out reason)) { tradeFeedback.text = reason; return; }
            long quote = SaleQuote.TotalMinor(escrow, home.Buffs.SalePrice);
            tradeSession.AddPlayerListing(escrow.Id, escrow.Definition, escrow.Quantity, escrow.Quality, quote);
            tradeFeedback.text = $"Listed {escrow.Definition.Name} x{escrow.Quantity:N0}: quote {Money.Format(quote)} cash (100% if bought, 80% at the shredder).";
            if (tradeSelected.Owner != ItemOwner.Backpack && tradeSelected.Owner != ItemOwner.Pocket && tradeSelected.Owner != ItemOwner.Hook) tradeSelected = null;
            tradeQuantity = 1; tradeStockRevision = -1;
            SaveNow(true);
        }

        private void BuyListing(TradeListing listing)
        {
            if (!TradeActive) return;
            if (!tradeSession.CanBuy(listing, travel.Cash, out long price, out string reason)) { tradeFeedback.text = reason; return; }
            ItemDefinition definition = TheTrailWalker.Domain.Crafting.Items.Find(listing.itemId);
            if (definition == null) { tradeFeedback.text = "Unknown item"; return; }
            TradeSettlement purchase = tradeSession.Buy(listing, travel.Cash);
            travel.Wallet.Debit(Currency.Cash, listing.cash); statistics.SpendCash(listing.cash);
            ItemEntry item = ledger.AddWorld("trade-" + Guid.NewGuid().ToString("N"), definition, listing.quantity, listing.quality, listing.Profile);
            ledger.StoreBought(item, out bool overflow);
            TradeLog.Add(tradeLog, TradeRecord.From(tradeSession.Data, listing, 0, DateTime.UtcNow));
            tradeFeedback.text = $"Bought {definition.Name} Q{listing.quality} for {Money.Cash(listing.cash)} cash" + (overflow ? " (bag full: sent to overflow)." : ".");
            tradeStockRevision = -1;
            SaveNow(true);
        }

        private void ReturnFromTrade()
        {
            if (tradeSession == null || !TradeActive) { CloseTradePanel(); return; }
            ApplySettlements(tradeSession.Cancel());
            FinishTrade();
        }

        private void ChangeTradeQuantity(int delta)
        {
            if (tradeSelected == null) { tradeFeedback.text = "Tap one of your items first."; return; }
            int max = tradeSelected.Quantity;
            tradeQuantity = delta == int.MaxValue ? max : Mathf.Clamp(tradeQuantity + delta, 1, max);
        }

        // ------------------------------------------------------------------ drawing

        private void RefreshTradePanel()
        {
            if (tradeSession == null) return;
            tradeTimer.text = TradeActive ? $"{tradeSession.Remaining:0.0} s" : "ended";
            tradeWallet.text = $"Cash {Money.Cash(travel.Cash)}   Gold {Money.Cash(travel.Gold)}";
            if (tradeSelected != null && (tradeSelected.Owner != ItemOwner.Backpack && tradeSelected.Owner != ItemOwner.Pocket && tradeSelected.Owner != ItemOwner.Hook || tradeSelected.Quantity < 1)) tradeSelected = null;
            if (tradeSelected != null)
            {
                tradeQuantity = Mathf.Clamp(tradeQuantity, 1, tradeSelected.Quantity);
                long quote = SaleQuote.LowestMinor(tradeSelected, tradeQuantity, home.Buffs.SalePrice);
                long unit = tradeQuantity > 0 ? quote / tradeQuantity : 0;
                tradeQuote.text = $"{tradeSelected.Definition.Name} x{tradeQuantity:N0} of {tradeSelected.Quantity:N0} ({tradeSelected.QualityLabel}): {Money.Format(unit)} each, total {Money.Format(quote)} cash.\n" +
                                  $"Bought by a trader: {Money.Cash(Money.Payout(quote, 100))}   shredded/time up: {Money.Cash(Money.Payout(quote, 80))}   returned: {Money.Cash(Money.Payout(quote, 70))}";
            }
            else tradeQuote.text = "No item selected.";
            tradeListButton.interactable = TradeActive && tradeSelected != null;
            if (tradeStockRevision != ledger.Revision) RebuildTradeStock();
            DrawBelt();
        }

        private void RebuildTradeStock()
        {
            tradeStockRevision = ledger.Revision;
            foreach (RectTransform cell in stockCells) Destroy(cell.gameObject);
            stockCells.Clear();
            float size = 0.95f, x = 6;
            float height = ((RectTransform)tradeStockContent.parent).rect.height;
            float side = Mathf.Max(40, height * size);
            // Misc and other crafts first, materials last: players mostly sell crafts and keep materials (user, Nov 17, 2026).
            foreach (ItemEntry item in ledger.Entries.OrderByDescending(e => ItemPrices.CategoryOf(e.Definition)))
            {
                if ((item.Owner != ItemOwner.Backpack && item.Owner != ItemOwner.Pocket && item.Owner != ItemOwner.Hook) || !item.Ready || item.Quantity < 1) continue;
                ItemEntry captured = item;
                RectTransform cell = Rect(item.Definition.Name, tradeStockContent, 0, 0, 0, 0);
                cell.anchorMin = new Vector2(0, 0.5f); cell.anchorMax = new Vector2(0, 0.5f); cell.pivot = new Vector2(0, 0.5f);
                cell.sizeDelta = new Vector2(side, side); cell.anchoredPosition = new Vector2(x, 0); x += side + 6;
                Image back = cell.gameObject.AddComponent<Image>(); back.color = item == tradeSelected ? new Color(0.85f, 0.62f, 0.2f) : new Color(0.25f, 0.21f, 0.16f);
                var button = cell.gameObject.AddComponent<Button>(); button.targetGraphic = back;
                button.onClick.AddListener(() => { tradeSelected = captured; tradeQuantity = captured.Quantity; tradeStockRevision = -1; });
                Icon(cell, item.Definition);
                Text count = Label(cell, ItemIconRenderer.Quantity(item), 0.02f, 0.0f, 0.98f, 0.3f, 16); count.alignment = TextAnchor.LowerRight;
                Text q = Label(cell, item.QualityLabel, 0.02f, 0.7f, 0.98f, 1f, 14); q.alignment = TextAnchor.UpperLeft;
                stockCells.Add(cell);
            }
            tradeStockContent.sizeDelta = new Vector2(x, 0);
        }

        private static void Icon(RectTransform cell, ItemDefinition definition)
        {
            Sprite sprite = ItemIconRenderer.For(definition);
            RectTransform pic = Rect("Icon", cell, 0.1f, 0.1f, 0.9f, 0.9f);
            Image image = pic.gameObject.AddComponent<Image>(); image.raycastTarget = false;
            if (sprite != null) { image.sprite = sprite; image.preserveAspect = true; }
            else { image.color = ColorFor(definition); }
        }

        private void DrawBelt()
        {
            var seen = new HashSet<string>();
            float width = tradeBelt.rect.width, height = tradeBelt.rect.height;
            foreach (TradeListing listing in tradeSession.Data.listings)
            {
                bool show = TradeActive && tradeSession.OnBeltNow(listing);
                if (!show) continue;
                seen.Add(listing.id);
                if (!beltViews.TryGetValue(listing.id, out RectTransform view)) beltViews[listing.id] = view = BeltView(listing, height * 0.44f);
                float pos = tradeSession.PositionNow(listing);
                bool npc = listing.owner == ListingOwner.Npc;
                view.anchoredPosition = new Vector2(pos * width, npc ? height * 0.75f : height * 0.25f);
                if (npc) view.Find("Price").GetComponent<Text>().text = Money.Cash(tradeSession.PriceNow(listing));
            }
            var gone = new List<string>();
            foreach (var pair in beltViews) if (!seen.Contains(pair.Key)) gone.Add(pair.Key);
            foreach (string id in gone) { Destroy(beltViews[id].gameObject); beltViews.Remove(id); }
        }

        private RectTransform BeltView(TradeListing listing, float side)
        {
            ItemDefinition definition = TheTrailWalker.Domain.Crafting.Items.Find(listing.itemId);
            RectTransform view = Rect(listing.id, tradeBelt, 0, 0, 0, 0);
            view.anchorMin = view.anchorMax = new Vector2(0, 0); view.pivot = new Vector2(0.5f, 0.5f); view.sizeDelta = new Vector2(side, side);
            bool npc = listing.owner == ListingOwner.Npc;
            Image back = view.gameObject.AddComponent<Image>(); back.color = npc ? new Color(0.36f, 0.26f, 0.12f) : new Color(0.14f, 0.3f, 0.3f);
            if (definition != null) Icon(view, definition);
            if (npc)
            {
                var button = view.gameObject.AddComponent<Button>(); button.targetGraphic = back;
                button.onClick.AddListener(() => BuyListing(listing));
                Text price = Label(view, "", -0.3f, -0.42f, 1.3f, 0.02f, 15); price.name = "Price"; price.color = new Color(1f, 0.86f, 0.4f);
                Text quality = Label(view, "Q" + listing.quality, 0.02f, 0.7f, 0.98f, 1f, 13); quality.alignment = TextAnchor.UpperLeft;
            }
            else
            {
                Text count = Label(view, listing.quantity >= 1000 ? "1k" : listing.quantity > 1 ? listing.quantity.ToString() : "", 0.02f, 0f, 0.98f, 0.3f, 14); count.alignment = TextAnchor.LowerRight;
            }
            return view;
        }

        // Development report next to the save (read over adb): one line per transaction of the finished session.
        private void WriteTradeReport()
        {
            try
            {
                var lines = new List<string>();
                foreach (TradeRecord record in tradeLog) if (record.campId == tradeSession.Data.campId && record.visit == tradeSession.Data.visit) lines.Add(record.Line(ItemName));
                lines.Add(TradeLog.Summary(tradeSession.Data, ItemName).Replace('\n', ' ')); lines.Add("");
                File.AppendAllLines(Path.Combine(SaveDirectory, "trade-report.txt"), lines);
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException) { Debug.LogWarning("[Milestone9] Trade report: " + error.Message); }
        }
    }
}
