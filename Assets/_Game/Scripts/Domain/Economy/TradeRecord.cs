using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TheTrailWalker.Domain.Economy
{
    // One trade transaction for reporting (Milestone 9 item 4): per listing, its unit and total quote, the camp factor and
    // the whole cash actually paid or charged, so single units and full 1,000-unit stacks read the same way.
    [Serializable] public sealed class TradeRecord
    {
        public string utc, campId, settlementId, itemId, result;
        public int visit, quantity, quality, percent;
        public long unitMinor, totalMinor, cash;   // cash > 0 paid to the player; cash < 0 charged to the player

        public static TradeRecord From(TradeSessionData session, TradeListing listing, int percent, DateTime utcNow)
        {
            bool bought = listing.result == ListingResult.BoughtByPlayer;
            long total = bought ? listing.valueMinor * listing.quantity : listing.valueMinor;
            return new TradeRecord { utc = utcNow.ToString("O", CultureInfo.InvariantCulture), campId = session.campId, visit = session.visit, settlementId = listing.settlementId,
                itemId = listing.itemId, result = listing.result.ToString(), quantity = listing.quantity, quality = listing.quality, percent = bought ? 0 : percent,
                unitMinor = listing.quantity > 0 ? total / listing.quantity : total, totalMinor = total, cash = bought ? -listing.cash : listing.cash };
        }

        public string Line(Func<string, string> name) =>
            $"{utc} {campId}#{visit} {result,-14} {name(itemId)} x{quantity} Q{quality} unit {Money.Format(unitMinor)} total {Money.Format(totalMinor)}" +
            (percent > 0 ? $" at {percent}%" : "") + $" -> {(cash >= 0 ? "+" : "")}{Money.Cash(cash)} cash";
    }

    public static class TradeLog
    {
        public const int Keep = 200;
        public static void Add(List<TradeRecord> log, TradeRecord record)
        {
            log.Add(record);
            if (log.Count > Keep) log.RemoveRange(0, log.Count - Keep);
        }
        // Totals of one session for the end-of-trade summary.
        public static string Summary(TradeSessionData session, Func<string, string> name)
        {
            long earned = 0, spent = 0; int sold = 0, shredded = 0, expired = 0, canceled = 0, bought = 0;
            var lines = new StringBuilder();
            foreach (TradeListing l in session.listings)
            {
                if (l.owner == ListingOwner.Player)
                {
                    earned += l.cash;
                    switch (l.result) { case ListingResult.SoldToTrader: sold++; break; case ListingResult.Shredded: shredded++; break; case ListingResult.Expired: expired++; break; case ListingResult.Canceled: canceled++; break; }
                    string how = l.result == ListingResult.SoldToTrader ? $"sold to trader {l.trader + 1} (100%)" : l.result == ListingResult.Shredded ? "shredded (80%)" : l.result == ListingResult.Expired ? "time up (80%)" : l.result == ListingResult.Canceled ? "returned (70%)" : "on the belt";
                    lines.AppendLine($"{name(l.itemId)} x{l.quantity:N0}: quote {Money.Format(l.valueMinor)} ({Money.Format(l.quantity > 0 ? l.valueMinor / l.quantity : 0)} each), {how} = +{Money.Cash(l.cash)}");
                }
                else if (l.result == ListingResult.BoughtByPlayer)
                {
                    bought++; spent += l.cash;
                    lines.AppendLine($"Bought {name(l.itemId)} Q{l.quality} from trader {l.trader + 1}: -{Money.Cash(l.cash)}");
                }
            }
            string head = session.state == TradeState.Canceled ? "Trade returned early." : "Trade finished.";
            return $"{head} Earned {Money.Cash(earned)} cash, spent {Money.Cash(spent)}. Sold {sold}, shredded {shredded}, time-up {expired}, returned {canceled}, bought {bought}.\n" + lines.ToString().TrimEnd();
        }
    }
}
