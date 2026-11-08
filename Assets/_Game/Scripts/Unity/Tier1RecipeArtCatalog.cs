using System;
using UnityEngine;

namespace TheTrailWalker
{
    [Serializable] public sealed class RecipeArtInput
    {
        public string ItemId;
        public int Quantity;
    }
    [Serializable] public sealed class RecipeArtDesign
    {
        public string Id, Name, Category, OutputId, Model, PrefabPath, UnlockSource, Note, Slot;
        public int Tier, Yield, Hearts;
        public float BaseSeconds;
        public bool BlockedSource;
        public string[] CraftingPathSourceIds, ConversionRecipeIds;
        public RecipeArtInput[] Inputs;
        public GameObject Visual;
    }
    [Serializable] public sealed class RecipeExternalInputArt
    {
        public string Id, Name, Model, Source, PrefabPath;
        public GameObject Visual;
    }
    [Serializable] public sealed class CraftedModelArt
    {
        public string Model;
        public int Triangles, Width, Height;
    }
    [Serializable] public sealed class RecipeSourceDesign
    {
        public string ItemId, Source;
        public int[] Regions;
        public bool Blocked;
    }
    // Authoring reference for Milestone 7, deliberately not a learned-recipe inventory or crafting service.
    public sealed class Tier1RecipeArtCatalog : ScriptableObject
    {
        public int Revision;
        [TextArea(3,8)] public string UnlockPolicy, ArtPolicy;
        public RecipeArtDesign[] Recipes;
        public RecipeExternalInputArt[] ExternalInputs;
        public CraftedModelArt[] Models;
        public RecipeSourceDesign[] Sources;
    }
}
