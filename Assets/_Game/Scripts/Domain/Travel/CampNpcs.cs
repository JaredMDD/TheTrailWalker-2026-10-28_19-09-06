using System;
using System.Collections.Generic;

namespace TheTrailWalker.Domain.Travel
{
    public enum NpcBody { Male, Female }

    // A full clothing set (hat, boots, shirt, pants) worn together. Tier 0 is the starter rags and is always known.
    public sealed class NpcClothingSet
    {
        public readonly string Id; public readonly int Tier;
        public readonly IReadOnlyList<string> RecipeIds;
        public NpcClothingSet(string id, int tier, params string[] recipeIds) { Id = id; Tier = tier; RecipeIds = recipeIds; }
    }

    // A backpack look; chosen independently of the clothing set. Tier 0 (rag pack) has no recipe and is always known.
    public sealed class NpcBackpack
    {
        public readonly string Id; public readonly int Tier; public readonly string RecipeId;
        public NpcBackpack(string id, int tier, string recipeId) { Id = id; Tier = tier; RecipeId = recipeId; }
    }

    public readonly struct NpcAppearance
    {
        public readonly NpcBody Body; public readonly int SkinTone, HairColor;
        public readonly string ClothingSetId, BackpackId;
        // Idle animation offset (0-1) and speed multiplier, so seated NPCs never sway in sync.
        public readonly float IdlePhase, IdleSpeed;
        // Hair style index (0 Crop, 1 Sweep, 2 Bob, 3 Tied) and, for men only, a full beard or none (no stubble).
        public readonly int HairStyle; public readonly bool Beard;
        public NpcAppearance(NpcBody body, int skinTone, int hairColor, string clothingSetId, string backpackId, float idlePhase, float idleSpeed, int hairStyle = 1, bool beard = false)
        { Body = body; SkinTone = skinTone; HairColor = hairColor; ClothingSetId = clothingSetId; BackpackId = backpackId; IdlePhase = idlePhase; IdleSpeed = idleSpeed; HairStyle = hairStyle; Beard = beard && body == NpcBody.Male; }
    }

    // Cosmetic look of the three traders seated around a camp's campfire. A clothing set is discovered once at least one
    // of its recipes is known; a backpack once its own recipe is known. Looks are seeded by camp and visit, so they stay
    // the same for the whole visit (also across reloads) and change on the next visit. Nothing here is sold or saved.
    public static class CampNpcs
    {
        public const int TraderSeats = 3, SkinTones = 6, HairColors = 4, HairStyles = 4;

        public static readonly IReadOnlyList<NpcClothingSet> ClothingSets = new[] {
            new NpcClothingSet("clothing.tier0.rags", 0),
            new NpcClothingSet("clothing.tier1.hemp_rabbit", 1,
                "recipe.wearable.rabbit_hat", "recipe.wearable.rabbit_boots", "recipe.wearable.hemp_shirt", "recipe.wearable.hemp_pants") };
        public static readonly IReadOnlyList<NpcBackpack> Backpacks = new[] {
            new NpcBackpack("backpack.tier0.rags", 0, null),
            new NpcBackpack("backpack.tier1.starter", 1, "recipe.wearable.starter_backpack") };

        public static List<NpcClothingSet> DiscoveredSets(ICollection<string> knownRecipes)
        {
            var result = new List<NpcClothingSet>();
            foreach (NpcClothingSet set in ClothingSets)
            {
                bool known = set.RecipeIds.Count == 0;
                foreach (string recipe in set.RecipeIds) known |= knownRecipes.Contains(recipe);
                if (known) result.Add(set);
            }
            return result;
        }

        public static List<NpcBackpack> DiscoveredBackpacks(ICollection<string> knownRecipes)
        {
            var result = new List<NpcBackpack>();
            foreach (NpcBackpack pack in Backpacks) if (pack.RecipeId == null || knownRecipes.Contains(pack.RecipeId)) result.Add(pack);
            return result;
        }

        // Rolled when the camp is shown. Discoveries made during the visit (a mission claim) appear from the next visit.
        public static NpcAppearance[] Roll(string campId, int visitNumber, ICollection<string> knownRecipes)
        {
            if (string.IsNullOrWhiteSpace(campId) || knownRecipes == null) throw new ArgumentException("Invalid camp NPC roll");
            List<NpcClothingSet> sets = DiscoveredSets(knownRecipes);
            List<NpcBackpack> packs = DiscoveredBackpacks(knownRecipes);
            var result = new NpcAppearance[TraderSeats];
            var used = new HashSet<int>();
            for (int seat = 0; seat < TraderSeats; seat++)
            {
                uint hash = LooseDrops.Hash(campId, visitNumber, seat);
                // Body and skin tone differ between the three traders of one camp.
                int look = (int)(hash % (2 * SkinTones));
                while (!used.Add(look)) look = (look + 1) % (2 * SkinTones);
                uint next = LooseDrops.Hash(campId, visitNumber, seat + 100);
                uint idle = LooseDrops.Hash(campId, visitNumber, seat + 200);
                // Separate stream for the faceted details (November 2, 2026), so the earlier fields keep their values.
                uint detail = LooseDrops.Hash(campId, visitNumber, seat + 300);
                NpcBody body = look % 2 == 0 ? NpcBody.Male : NpcBody.Female;
                result[seat] = new NpcAppearance(body, look / 2, (int)((idle >> 20) % HairColors),
                    sets[(int)(next % (uint)sets.Count)].Id, packs[(int)((next >> 16) % (uint)packs.Count)].Id,
                    (idle % 1000) / 1000f, 0.85f + ((idle >> 10) % 300) / 1000f,
                    (int)(detail % HairStyles), body == NpcBody.Male && ((detail >> 8) & 1) == 1);
            }
            return result;
        }
    }
}
