using System;
using System.Globalization;

namespace TheTrailWalker.Domain.Economy
{
    public enum Currency { Cash, Gold }

    // Fixed-point money (plan section 12.2, Milestone 9). Item unit values are stored in minor units (1/100 cash) so
    // quality and level factors keep their fractions; every payout is rounded down to whole cash once, at the final
    // transaction. Wallets hold whole cash and gold only (integer-only acquisition, October 31, 2026).
    public static class Money
    {
        public const long MinorPerCash = 100;
        public static long Minor(long cash) => checked(cash * MinorPerCash);
        public static long Minor(double cash) => (long)Math.Floor(cash * MinorPerCash + 1e-9);
        // Whole cash paid for a minor-unit total at a percentage (100, 80 or 70), rounded down once.
        public static long Payout(long totalMinor, int percent)
        {
            if (totalMinor < 0 || percent < 0) throw new ArgumentOutOfRangeException(nameof(totalMinor));
            return (long)(new decimal(totalMinor) * percent / 100m / MinorPerCash);
        }
        // Whole cash charged for a minor-unit price: rounded down as well, so a quote never charges more than shown.
        public static long Charge(long priceMinor) => priceMinor < 0 ? throw new ArgumentOutOfRangeException(nameof(priceMinor)) : priceMinor / MinorPerCash;
        // "1.15" for small fractional values, "12,000" for whole amounts.
        public static string Format(long minor)
        {
            long whole = minor / MinorPerCash, cents = Math.Abs(minor % MinorPerCash);
            string head = whole.ToString("N0", CultureInfo.InvariantCulture);
            return cents == 0 ? head : head + "." + cents.ToString("00", CultureInfo.InvariantCulture);
        }
        public static string Cash(long cash) => cash.ToString("N0", CultureInfo.InvariantCulture);
    }

    // The two player wallets (section 12.2): integer, 64-bit, never negative. Gold ore and ingots are items, never gold.
    public sealed class Wallet
    {
        public long Cash { get; private set; }
        public long Gold { get; private set; }
        public Wallet(long cash = 0, long gold = 0)
        {
            if (cash < 0 || gold < 0) throw new ArgumentOutOfRangeException(nameof(cash));
            Cash = cash; Gold = gold;
        }
        public long Balance(Currency currency) => currency == Currency.Gold ? Gold : Cash;
        public void Credit(Currency currency, long amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return;
            if (currency == Currency.Gold) Gold = checked(Gold + amount); else Cash = checked(Cash + amount);
        }
        public bool CanDebit(Currency currency, long amount) => amount >= 0 && Balance(currency) >= amount;
        public void Debit(Currency currency, long amount)
        {
            if (!CanDebit(currency, amount)) throw new InvalidOperationException($"Not enough {currency.ToString().ToLowerInvariant()}");
            if (currency == Currency.Gold) Gold -= amount; else Cash -= amount;
        }
    }
}
