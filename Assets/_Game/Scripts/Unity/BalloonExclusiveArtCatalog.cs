using System;
using UnityEngine;

namespace TheTrailWalker
{
    [Serializable] public sealed class BalloonExclusiveOutputArt
    {
        public string RecipeId,OutputId,Name,Category,IngredientBrief,Kind,DrinkKey,RequiredRecipeId,PrerequisiteBrief;
        public int Tier,Yield,HeartsPerServing;
        public bool FutureTierArtOnly,Consumable;
        public GameObject Visual;
    }
    // Appearance references only, separate from recipe acquisition, stock and physical box rewards.
    public sealed class BalloonExclusiveArtCatalog : ScriptableObject
    {
        public BalloonExclusiveOutputArt[] Outputs;
    }
}
