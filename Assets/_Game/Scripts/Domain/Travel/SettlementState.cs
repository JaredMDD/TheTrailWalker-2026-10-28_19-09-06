using System;
using System.Collections.Generic;

namespace TheTrailWalker.Domain.Travel
{
    // Settlement data (spec 2026-11-08, section 1): the chosen home plot, five neighbours on the other plots and their
    // seeded level-up countdowns. Plots 0-2 sit left of the shops, 3-5 right. Pure C#, no engine types.
    public sealed class SettlementState
    {
        public const int PlotCount = 6;
        public const int MaxLevel = 16;   // Part 2 section 16.5: neighbours can reach level 16 (the 16 exteriors)

        public sealed class Neighbour
        {
            public int Plot;
            public int Level;
            public int Countdown;
        }

        private readonly List<Neighbour> neighbours = new List<Neighbour>();
        private readonly List<string> countedRegions = new List<string>();

        public bool Discovered { get; private set; }
        public int PlotIndex { get; private set; } = -1;
        public int Seed { get; private set; }
        public IReadOnlyList<Neighbour> Neighbours => neighbours;
        public IReadOnlyList<string> CountedRegions => countedRegions;

        private SettlementState() { }

        public static SettlementState New(int seed) => new SettlementState { Seed = seed };

        public void Discover() { Discovered = true; }

        public bool CanChoosePlot(int plot, out string reason)
        {
            if (PlotIndex >= 0) { reason = "The plot has already been chosen."; return false; }
            if (plot < 0 || plot >= PlotCount) { reason = "Plot " + plot + " does not exist."; return false; }
            reason = null;
            return true;
        }

        // Regions discovered before the choice are recorded as counted and never advance a neighbour.
        public void ChoosePlot(int plot, IEnumerable<string> discoveredRegions)
        {
            if (!CanChoosePlot(plot, out string reason)) throw new InvalidOperationException(reason);
            PlotIndex = plot;
            Discovered = true;
            neighbours.Clear();
            for (int p = 0; p < PlotCount; p++)
            {
                if (p == plot) continue;
                neighbours.Add(new Neighbour { Plot = p, Level = 1, Countdown = Roll(p, "start") });
            }
            countedRegions.Clear();
            if (discoveredRegions != null)
                foreach (string id in discoveredRegions)
                    if (!string.IsNullOrEmpty(id) && !countedRegions.Contains(id)) countedRegions.Add(id);
        }

        // Applies regions not yet counted, one at a time in the order given. Each one ticks every neighbour's countdown;
        // at zero the neighbour gains a level (up to the maximum) and rolls a fresh countdown for the next region.
        public int ApplyNewRegions(IEnumerable<string> discoveredRegions)
        {
            if (PlotIndex < 0 || discoveredRegions == null) return 0;
            int applied = 0;
            foreach (string id in discoveredRegions)
            {
                if (string.IsNullOrEmpty(id) || countedRegions.Contains(id)) continue;
                countedRegions.Add(id);
                applied++;
                foreach (Neighbour n in neighbours)
                {
                    n.Countdown--;
                    if (n.Countdown > 0) continue;
                    if (n.Level < MaxLevel) n.Level++;
                    n.Countdown = Roll(n.Plot, id);
                }
            }
            return applied;
        }

        public static SettlementState Restore(bool discovered, int plot, int seed, IEnumerable<(int plot, int level, int countdown)> neighbours, IEnumerable<string> countedRegions)
        {
            if (plot < -1 || plot >= PlotCount) throw new ArgumentException("Settlement plot out of range: " + plot);
            var state = new SettlementState { Discovered = discovered, PlotIndex = plot, Seed = seed };
            var seen = new HashSet<int>();
            if (neighbours != null)
                foreach ((int p, int level, int countdown) in neighbours)
                {
                    if (p < 0 || p >= PlotCount) throw new ArgumentException("Neighbour plot out of range: " + p);
                    if (p == plot) throw new ArgumentException("A neighbour cannot share the player's plot: " + p);
                    if (!seen.Add(p)) throw new ArgumentException("Duplicate neighbour plot: " + p);
                    if (level < 1 || level > MaxLevel) throw new ArgumentException("Neighbour level out of range: " + level);
                    if (countdown < 1 || countdown > 3) throw new ArgumentException("Neighbour countdown out of range: " + countdown);
                    state.neighbours.Add(new Neighbour { Plot = p, Level = level, Countdown = countdown });
                }
            if (plot < 0 && state.neighbours.Count > 0) throw new ArgumentException("Neighbours exist before a plot was chosen.");
            if (plot >= 0 && state.neighbours.Count != PlotCount - 1) throw new ArgumentException("A chosen plot needs " + (PlotCount - 1) + " neighbours.");
            if (countedRegions != null)
                foreach (string id in countedRegions)
                {
                    if (string.IsNullOrEmpty(id)) throw new ArgumentException("Empty counted region ID.");
                    if (!state.countedRegions.Contains(id)) state.countedRegions.Add(id);
                }
            return state;
        }

        private int Roll(int plot, string key) => (int)(Hash(Seed, plot, key) % 3) + 1;

        private static uint Hash(int seed, int plot, string key)
        {
            uint hash = 2166136261;
            foreach (char c in seed + "|" + plot + "|" + key) { hash ^= c; hash *= 16777619; }
            return hash;
        }
    }
}
