using System;

namespace TheTrailWalker.Domain.Settlement
{
    // Home revision section 7: a neighbour's look-only room. Furnished deterministically from (settlement seed, plot, level):
    // the same neighbour at the same level always gets the same room; a level-up re-rolls it. The result is a separate
    // HomeState that is never saved, never buffs and never touches the player's home.
    public static class NeighbourHome
    {
        public static HomeState Furnish(int settlementSeed, int plot, int level)
        {
            if (level < 1 || level > HomeState.MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
            HomeState home = HomeState.AtLevel(level);
            var rng = new SplitMix(settlementSeed, plot, level);
            int n = 0;
            foreach (FurnitureDefinition d in FurnitureCatalog.UnlockedAt(level))   // catalog order
            {
                if (rng.Next(3) == 0) continue;                                       // included with chance 2/3
                int rotation = rng.Next(4);
                string id = "neighbour." + n;
                bool placed = false;
                (int width, int depth) = HomeState.Footprint(d, rotation);
                int xs = home.GridWidth - width + 1, zs = home.GridDepth - depth + 1;
                for (int attempt = 0; attempt < 12 && !placed; attempt++)
                {
                    int x = rng.Next(Math.Max(1, xs)), z = rng.Next(Math.Max(1, zs));
                    placed = xs > 0 && zs > 0 && home.TryAddPlaced(d, id, x, z, rotation);
                }
                for (int turn = 0; turn < 2 && !placed; turn++)
                {
                    int r = (rotation + turn) % 4;
                    if (home.TryNearestFree(d, out int x, out int z, null, r)) placed = home.TryAddPlaced(d, id, x, z, r);
                }
                if (placed) n++;                                                      // a piece that fits nowhere is left out
            }
            return home;
        }

        // SplitMix64 (Steele, Lea, Flood 2014): a fixed integer sequence on every runtime (System.Random's is not guaranteed).
        private struct SplitMix
        {
            private ulong state;

            public SplitMix(int seed, int plot, int level)
            {
                state = 0x4E65696768626F75UL;   // "Neighbou"
                state = Mix(state ^ (uint)seed);
                state = Mix(state ^ ((ulong)(uint)plot << 32));
                state = Mix(state ^ (uint)level);
            }

            private static ulong Mix(ulong z)
            {
                z += 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }

            private ulong NextU64() { state += 0x9E3779B97F4A7C15UL; ulong z = state; z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL; z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL; return z ^ (z >> 31); }

            // 0..bound-1 (bound >= 1).
            public int Next(int bound) { return (int)(NextU64() % (ulong)bound); }
        }
    }
}
