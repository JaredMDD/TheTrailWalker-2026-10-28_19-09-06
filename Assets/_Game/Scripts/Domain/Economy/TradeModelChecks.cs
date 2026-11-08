using System;
using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Inventory;
using TheTrailWalker.Domain.Items;

namespace TheTrailWalker.Domain.Economy
{
    // Engine-free Milestone 9 checks (wallets, prices, escrow, the 15-second session and its settlement rules).
    public static class TradeModelChecks
    {
        public static List<string> Run()
        {
            var passed = new List<string>();
            void Check(bool ok, string name) { if (!ok) throw new InvalidOperationException("Milestone 9: " + name); passed.Add(name); }

            // ---- money and wallets
            Check(Money.Payout(10000, 80) == 80 && Money.Payout(199, 100) == 1 && Money.Payout(115, 70) == 0 && Money.Payout(100000, 70) == 700, "payouts round down to whole cash once");
            Check(Money.Format(115) == "1.15" && Money.Format(1200000) == "12,000", "minor units format as cash");
            var wallet = new Wallet(5, 1);
            wallet.Credit(Currency.Cash, 10); wallet.Debit(Currency.Cash, 15); bool threw = false;
            try { wallet.Debit(Currency.Cash, 1); } catch (InvalidOperationException) { threw = true; }
            Check(wallet.Cash == 0 && wallet.Gold == 1 && threw, "wallets never go negative");
            var rich = new Wallet(long.MaxValue - 5); threw = false;
            try { rich.Credit(Currency.Cash, 10); } catch (OverflowException) { threw = true; }
            Check(threw && rich.Cash == long.MaxValue - 5, "64-bit wallet overflow is refused, not wrapped");

            // ---- price data (section 12.2)
            ItemDefinition branch = PrototypeItems.Branch, log = PrototypeItems.Log, plank = Crafting.Items.Find("item.pine_plank");
            Check(ItemPrices.IntrinsicMinor(branch, 1) == 100 && ItemPrices.IntrinsicMinor(branch, 5) == 200 && ItemPrices.IntrinsicMinor(branch, 2) == 115, "one-cash basic: Q1 1.00, Q2 1.15, Q5 2.00");
            Check(ItemPrices.IntrinsicMinor(log, 1) == 300 && ItemPrices.IntrinsicMinor(plank, 1) == 900, "pine log 3 and pine plank 9 (3:1 conversions)");
            Check(ItemPrices.CategoryOf(branch) == PriceCategory.Basic && ItemPrices.CategoryOf(Crafting.Items.Find("item.starter_backpack")) == PriceCategory.Backpack
                && ItemPrices.CategoryOf(Crafting.Items.Find("item.pine_bird")) == PriceCategory.Misc && ItemPrices.CategoryOf(PrototypeItems.Tool) == PriceCategory.Tool, "price categories");
            Check(ItemPrices.ReferenceAtLevel10(RecipeRole.Misc, 4, false) == 6000 && ItemPrices.ReferenceAtLevel10(RecipeRole.Wearable, 4, true) == 3500
                && ItemPrices.ReferenceAtLevel10(RecipeRole.Wearable, 4, false) == 2200 && ItemPrices.ReferenceAtLevel10(RecipeRole.Tool, 4, false) == 1400 && ItemPrices.ReferenceAtLevel10(RecipeRole.Food, 4, false) == 600, "tier-four references 6,000 / 3,500 / 2,200 / 1,400 / 600");
            RecipeDefinition bird = RecipeBook.Find("recipe.misc.pine_bird");
            Check(bird != null && CraftStats.Compute(bird, 0, 1).UnitValue == Money.Minor(480L) && CraftStats.Compute(bird, 115, 1).UnitValue == Money.Minor(1200L), "craft values snapshot in minor units (L1 Q1 480, L10 Q5 1,200)");
            ItemDefinition hat = Crafting.Items.Find("item.rabbit_hat");
            ItemProfile npcHat1 = ItemPrices.Profile(hat, 1), npcHat5 = ItemPrices.Profile(hat, 5);
            Check(npcHat1.UnitValue == Money.Minor(220 * 36 / 45.0) && npcHat5.UnitValue > npcHat1.UnitValue * 2 && npcHat5.MaxDurability > npcHat1.MaxDurability && npcHat1.Durability == npcHat1.MaxDurability,
                "NPC gear: level one, quality premium plus durability premium, full condition");
            Check(ItemPrices.MigrateLegacyValue(branch, 3, 100) == 135 && ItemPrices.MigrateLegacyValue(Crafting.Items.Find("item.pine_bird"), 5, 1200) == 120000, "legacy values migrate (drops re-derived, crafts x100)");

            // ---- escrow
            var bag = new SpatialInventory();
            ItemEntry logs = bag.AddWorld("logs", log, 1000, 2, ItemPrices.Profile(log, 2));
            bag.Reserve(logs, 1); bag.TryPlace(logs, 1, 0, 0, false, out _); bag.Release(logs, 1);
            Check(SaleQuote.LowestMinor(logs, 250) == 250 * 345 && SaleQuote.TotalMinor(logs) == 1000L * 345, "stack quotes sum every unit (never a unit price for the stack)");
            Check(bag.TryEscrow(logs, 250, "esc-1", out ItemEntry escrow, out _) && logs.Quantity == 750 && escrow.Quantity == 250 && escrow.Owner == ItemOwner.Escrow && bag.CountIngredient(log.Id) == 750,
                "listing moves the chosen quantity into escrow; it cannot also be crafted");
            Check(!bag.TryEscrow(escrow, 10, "esc-2", out _, out _) && bag.ConsumeForMission(log.Id, 1000) == 750 && escrow.Quantity == 250, "escrowed units cannot be relisted or delivered");
            Check(bag.SettleEscrow(escrow) && escrow.Owner == ItemOwner.Consumed && !bag.SettleEscrow(escrow), "escrow settles exactly once");
            ItemEntry bought = bag.AddWorld("bought", hat, 1, 3, ItemPrices.Profile(hat, 3));
            Check(bag.StoreBought(bought, out bool overflow) && (bought.Owner == ItemOwner.Backpack || (overflow && bought.Owner == ItemOwner.Overflow)), "bought goods go to the bag or overflow, never lost");

            // ---- session rules
            var offers = TradeOffers.Generate(1234, new[] { "material.pine_branch", "food.apples" });
            var perTrader = offers.GroupBy(o => o.Trader).ToList();
            Check(perTrader.Count == 3 && perTrader.All(g => g.Count() >= 3 && g.Count() <= 5), "three traders with 3-5 offers each");
            Check(offers.All(o => o.Quality >= 1 && o.Quality <= 5) && offers.Where(o => !Crafting.Items.Find(o.ItemId).Stackable).All(o => o.Profile.MaxDurability == 0 || o.Profile.Durability == o.Profile.MaxDurability), "offers: Q1-Q5, gear at full condition");
            Check(TradeOffers.Generate(1234, new[] { "material.pine_branch" }).Select(o => o.ItemId + o.Quality).SequenceEqual(TradeOffers.Generate(1234, new[] { "material.pine_branch" }).Select(o => o.ItemId + o.Quality)), "offers are seeded per visit");

            TradeSession Fresh(int seed) { var s = TradeSession.Start("camp.test", seed, new List<TradeOffer>()); s.Data.seed = seed; return s; }
            // Natural shredder: find a seed where no trader buys listing 0.
            int noBuy = Enumerable.Range(1, 500).First(seed => TradeSession.TraderPositions.Select((_, t) => TradeSession.Hash01(seed, 0, t)).All(r => r >= TradeSession.BuyChance));
            var shred = Fresh(noBuy); TradeListing a = shred.AddPlayerListing("e", log, 1000, 1, 300000);
            var events = shred.Advance(TradeSession.BeltSeconds + 1f);
            Check(events.Count == 1 && a.result == ListingResult.Shredded && a.cash == 2400 && events[0].Percent == 80, "a listing reaching the shredder pays 80% once");
            Check(shred.Advance(1f).Count == 0, "no second payout for a shredded listing");
            int buy = Enumerable.Range(1, 500).First(seed => TradeSession.Hash01(seed, 0, 0) < TradeSession.BuyChance);
            var sale = Fresh(buy); TradeListing b = sale.AddPlayerListing("e", log, 1000, 1, 300000);
            var sold = sale.Advance(TradeSession.TraderPositions[0] * TradeSession.BeltSeconds + 0.5f);
            Check(sold.Count == 1 && b.result == ListingResult.SoldToTrader && b.trader == 0 && b.cash == 3000, "a trader buying a listing pays 100% of its quote");
            sale.Advance(TradeSession.Duration + 5f);
            Check(sale.State == TradeState.Completed && b.cash == 3000 && sale.Cancel().Count == 0, "expiry and a late Return never settle a sold listing again");
            var expiry = Fresh(noBuy); expiry.Advance(TradeSession.Duration - 3f); TradeListing late = expiry.AddPlayerListing("e", log, 10, 1, 3000);
            var ended = expiry.Advance(5f);
            Check(expiry.State == TradeState.Completed && late.result == ListingResult.Expired && late.cash == 24 && ended.Count == 1, "listings still on the belt at the end pay 80% (natural expiry)");
            Check(!expiry.CanList(out _) && expiry.Advance(1f).Count == 0, "the session ends at its duration");

            // Cancellation example (section 12.1): unsold 100, sold 200, an NPC listing on the belt -> +70 only.
            int mixed = Enumerable.Range(1, 2000).First(seed => TradeSession.TraderPositions.Select((_, t) => TradeSession.Hash01(seed, 1, t)).All(r => r >= TradeSession.BuyChance) && TradeSession.Hash01(seed, 2, 0) < TradeSession.BuyChance);
            var example = TradeSession.Start("camp.test", 1, new List<TradeOffer> { new TradeOffer(1, branch.Id, 1, ItemPrices.Profile(branch, 1)) }); example.Data.seed = mixed;
            example.Data.listings[0].enterTime = 0;
            TradeListing unsold = example.AddPlayerListing("u", log, 1, 1, 10000), soldOne = example.AddPlayerListing("s", log, 1, 1, 20000);
            example.Advance(TradeSession.TraderPositions[0] * TradeSession.BeltSeconds + 1f);
            Check(soldOne.result == ListingResult.SoldToTrader && soldOne.cash == 200 && !unsold.Settled, "example setup: one listing sold, one unsold");
            var cancel = example.Cancel();
            Check(example.State == TradeState.Canceled && cancel.Count == 1 && unsold.cash == 70 && soldOne.cash == 200 && example.Data.listings[0].result == ListingResult.NpcExpired && example.Data.listings[0].cash == 0,
                "Return pays 70% on unsold player listings only; sold and NPC listings pay nothing more");
            Check(example.Cancel().Count == 0 && example.Advance(5f).Count == 0 && !example.CanList(out _), "a cancelled session is terminal");
            var lateReturn = Fresh(noBuy); TradeListing lr = lateReturn.AddPlayerListing("e", log, 1, 1, 10000); lateReturn.Data.elapsed = TradeSession.Duration;
            lateReturn.Cancel();
            Check(lateReturn.State == TradeState.Completed && lr.result == ListingResult.Expired && lr.cash == 80, "Return after the session time finishes the natural expiry instead");

            // NPC goods: position price, never paying the player, the buyer pays once.
            var npc = TradeSession.Start("camp.test", 2, new List<TradeOffer> { new TradeOffer(0, hat.Id, 3, ItemPrices.Profile(hat, 3)) });
            TradeListing good = npc.Data.listings[0]; good.enterTime = 0;
            long atEntry = npc.PriceNow(good); npc.Advance(TradeSession.BeltSeconds - 0.01f); long atEnd = npc.PriceNow(good);
            Check(Math.Abs(atEntry - good.valueMinor * 1.1 / 100) <= 1 && Math.Abs(atEnd - good.valueMinor * 0.8 / 100) <= 1, "NPC price slides from 110% at entry to 80% before the shredder");
            Check(!npc.CanBuy(good, atEnd - 1, out _, out _), "buying needs enough cash");
            npc.Buy(good, atEnd);
            Check(good.result == ListingResult.BoughtByPlayer && good.cash == atEnd && !npc.CanBuy(good, long.MaxValue, out _, out _), "an NPC listing sells once");
            var lost = TradeSession.Start("camp.test", 3, new List<TradeOffer> { new TradeOffer(0, hat.Id, 3, ItemPrices.Profile(hat, 3)) });
            lost.Data.listings[0].enterTime = 0;
            Check(lost.Advance(TradeSession.BeltSeconds + 1f).Count == 0 && lost.Data.listings[0].result == ListingResult.NpcShredded && lost.Data.listings[0].cash == 0, "shredded NPC goods give the player nothing");

            // Determinism and keying.
            var first = Fresh(77); var second = Fresh(77);
            for (int i = 0; i < 6; i++) { first.AddPlayerListing("x" + i, log, 1, 1, 1000); second.AddPlayerListing("x" + i, log, 1, 1, 1000); }
            first.Advance(30f); second.Advance(8.4f); second.Advance(6.6f); second.Advance(15f);
            Check(first.Data.listings.Select(l => l.result + ":" + l.cash).SequenceEqual(second.Data.listings.Select(l => l.result + ":" + l.cash)), "outcomes do not depend on frame timing");
            Check(first.Matches("camp.test", 77) && !first.Matches("camp.test", 78) && !first.Matches("camp.other", 77), "one session per camp visit");
            // Offer window (user, November 8, 2026): 60 s to start; missed offers are spent; a 30 s session with a 12 s belt.
            Check(TradeSession.Duration == 30f && TradeSession.BeltSeconds == 12f && TradeSession.OfferSeconds == 60f, "session 30 s, belt 12 s, offer window 60 s");
            var offer = TradeSession.Offer("camp.test", 5);
            Check(offer.Offering && !offer.Active && !offer.Ended && !offer.AdvanceOffer(59f) && Math.Abs(offer.OfferRemaining - 1f) < 1e-4, "the offer stays open for 60 seconds");
            offer.Begin(offers);
            Check(offer.Active && offer.Data.listings.Count == offers.Count && offer.Data.listings.All(l => l.enterTime < TradeSession.Duration - 3f), "START TRADING begins the session with the traders' goods");
            var missed = TradeSession.Offer("camp.test", 6);
            Check(missed.AdvanceOffer(61f) && missed.State == TradeState.Missed && missed.Ended && !missed.AdvanceOffer(1f), "an untouched offer is missed for the visit");
            threw = false; try { missed.Begin(offers); } catch (InvalidOperationException) { threw = true; }
            Check(threw, "a missed offer cannot start a session");
            return passed;
        }
    }
}
