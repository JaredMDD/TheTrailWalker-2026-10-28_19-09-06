using System;

namespace TheTrailWalker.Domain.Equipment
{
    // Models stamina and pending food delivery (no reserve beyond the cap); it does not own inventory transactions.
    public sealed class StaminaState
    {
        public double Hearts { get; private set; } = 3;
        public double Maximum { get; private set; } = 3;
        public double PendingFood { get; private set; }
        public bool Fainted { get; private set; }
        public double FaintElapsed { get; private set; }

        public void SetMaximum(double maximum)
        {
            if (double.IsNaN(maximum) || double.IsInfinity(maximum) || maximum < 3 || maximum > 12) throw new ArgumentOutOfRangeException(nameof(maximum));
            Maximum = maximum; Hearts = Math.Min(Hearts, Maximum);
        }
        public void QueueFood(double recovery)
        {
            if (double.IsNaN(recovery) || double.IsInfinity(recovery) || recovery <= 0 || Fainted) throw new ArgumentOutOfRangeException(nameof(recovery));
            PendingFood += recovery;
        }
        public void Restore(double hearts, double maximum, double pending, bool fainted, double elapsed)
        {
            if (double.IsNaN(hearts) || double.IsInfinity(hearts) || double.IsNaN(maximum) || double.IsInfinity(maximum) || double.IsNaN(pending) || double.IsInfinity(pending) || double.IsNaN(elapsed) || double.IsInfinity(elapsed) || maximum < 3 || maximum > 12 || hearts < 0 || hearts > maximum || pending < 0 || elapsed < 0 || elapsed > 48)
                throw new ArgumentException("Invalid saved stamina");
            Hearts = hearts; Maximum = maximum; PendingFood = pending; Fainted = fainted; FaintElapsed = elapsed;
        }

        // The caller decides and commits the faint-loss manifest before calling BeginFaint.
        // Walking is clipped at the zero boundary using float path distances, which can leave a tiny residue
        // (for example 1e-8 hearts). Anything at or below this tolerance counts as an empty meter.
        public const double EmptyEpsilon = 1e-4;
        // Confirmed shared rate for camp/settlement regeneration, faint recovery and food delivery (raised from 0.25).
        public const double RecoveryPerSecond = 1.0 / 3.0;
        public bool IsEmpty => Hearts <= EmptyEpsilon;
        // Auto-eat (user, November 7, 2026): carried prepared food is eaten at 0.1 hearts, while walking continues, so the
        // meter normally never reaches zero. Zero with no prepared food still stops the walker and leads to fainting.
        public const double AutoEatThreshold = 0.1;
        public bool ShouldAutoEat(bool safeArea) => !safeArea && !Fainted && PendingFood <= 0 && Hearts <= AutoEatThreshold;
        public bool ShouldFaint(bool safeArea, bool accessibleFood) => !safeArea && !Fainted && IsEmpty && PendingFood <= 0 && !accessibleFood;
        public void BeginFaint() { if (Fainted || !IsEmpty || PendingFood > 0) throw new InvalidOperationException("Invalid faint transition"); Hearts = 0; Fainted = true; FaintElapsed = 0; }

        public double LimitWalkingSeconds(double seconds, double metresPerSecond, double drainPerMetre = 0.002)
        {
            if (!FiniteNonnegative(seconds) || !FiniteNonnegative(metresPerSecond) || !FiniteNonnegative(drainPerMetre)) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (Fainted || IsEmpty) return 0;
            if (metresPerSecond == 0 || drainPerMetre == 0 || Hearts > seconds * metresPerSecond * drainPerMetre) return seconds;
            // Only use the slower boundary search when the available meter may reach zero.
            var probe = new StaminaState(); probe.Restore(Hearts, Maximum, PendingFood, false, 0);
            probe.Advance(seconds, seconds * metresPerSecond, false, true, drainPerMetre);
            if (probe.Hearts > 0) return seconds;
            double lo = 0, hi = seconds;
            for (int i = 0; i < 30; i++)
            {
                double mid = (lo + hi) * 0.5;
                probe.Restore(Hearts, Maximum, PendingFood, false, 0);
                probe.Advance(mid, mid * metresPerSecond, false, true, drainPerMetre);
                if (probe.Hearts > 1e-10) lo = mid; else hi = mid;
            }
            return hi;
        }

        public void Advance(double seconds, double walkedMetres, bool safeArea, bool foreground, double drainPerMetre = 0.002, double foodPerSecond = RecoveryPerSecond, double safePerSecond = RecoveryPerSecond)
        {
            if (!FiniteNonnegative(seconds) || !FiniteNonnegative(walkedMetres) || !FiniteNonnegative(drainPerMetre) || !FiniteNonnegative(foodPerSecond) || !FiniteNonnegative(safePerSecond)) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!foreground) return;
            if (Fainted)
            {
                double step = Math.Min(seconds, (Maximum - Hearts) / RecoveryPerSecond);
                FaintElapsed += step; Hearts = Math.Min(Maximum, Hearts + RecoveryPerSecond * step);
                if (Hearts >= Maximum) { Hearts = Maximum; Fainted = false; }
                return;
            }
            if (seconds == 0) { Hearts = Math.Max(0, Hearts - walkedMetres * drainPerMetre); return; }
            // Integrate as piecewise-constant rates, splitting exactly at food-exhaustion/cap/zero boundaries.
            double remaining = seconds, drain = walkedMetres * drainPerMetre / seconds;
            while (remaining > 1e-9)
            {
                double passive = safeArea ? safePerSecond : 0;
                double food = PendingFood > 1e-9 ? foodPerSecond : 0;
                double net = passive + food - drain;
                // Food is delivered at a fixed rate and never builds a reserve: at the cap it can at most
                // balance walking drain, and whatever would exceed the maximum is lost.
                if (Hearts >= Maximum - 1e-9 && net >= 0) { Hearts = Maximum; net = 0; }
                if (Hearts <= 1e-9 && net < 0) { Hearts = 0; break; }
                double step = remaining;
                if (food > 0) step = Math.Min(step, PendingFood / food);
                if (net > 0 && Hearts < Maximum) step = Math.Min(step, (Maximum - Hearts) / net);
                if (net < 0 && Hearts > 0) step = Math.Min(step, Hearts / -net);
                if (step <= 1e-12) break;
                Hearts = Math.Max(0, Math.Min(Maximum, Hearts + net * step));
                PendingFood = Math.Max(0, PendingFood - food * step);
                remaining -= step;
            }
        }
        private static bool FiniteNonnegative(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }
}
