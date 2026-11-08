using UnityEngine;

namespace TheTrailWalker
{
    public sealed class SpecialNpcArtCatalog : ScriptableObject
    {
        public GameObject MissionLadySideways, MissionLadyFacing, MissionLadyRecipeHeld, MissionLadyOffering;
        public GameObject FerrymanStatic, RecipeDisplayProp, WoodenOar;
        [TextArea(3,8)] public string PoseIntent;
    }
}
