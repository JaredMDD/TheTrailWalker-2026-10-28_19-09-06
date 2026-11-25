using System.Collections.Generic;

namespace TheTrailWalker.Domain.Travel
{
    // Camps that host the balloon merchant (regions 1-4). One in every 4-6 camps along the route, never adjacent:
    // route camp indexes 4, 9, 14, 19, 25, 30, 35, 40 (0-based, regions in order).
    public static class BalloonHosts
    {
        public static readonly IReadOnlyList<string> All = new[]
        {
            "camp.01.05", "camp.01.10", "camp.02.05", "camp.02.10", "camp.03.04", "camp.03.09", "camp.04.03", "camp.04.08",
        };

        public static bool IsHost(string campId) { foreach (string id in All) if (id == campId) return true; return false; }

        // The host camp's region tier, the same formula the shops use; 0 for a non-host or unknown camp.
        public static int RegionTier(string campId)
        {
            int region = Region(campId);
            return region == 0 ? 0 : (region - 1) / 6 + 1;
        }

        // The host camp's region number; 0 for a non-host or unknown camp.
        public static int Region(string campId) => IsHost(campId) ? int.Parse(campId.Substring(5, 2)) : 0;
    }
}
