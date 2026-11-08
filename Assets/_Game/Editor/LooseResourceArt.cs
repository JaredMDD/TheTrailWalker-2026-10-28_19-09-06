using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Explicit integration only: no automatic scene, terrain, Play-mode or test operations.
    public static class LooseResourceArt
    {
        [MenuItem("The Trail Walker/Art/Rebuild Loose Resource Improvements")]
        public static void Rebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new System.InvalidOperationException("Rebuild outside Play mode.");
            AssetDatabase.Refresh();
            Region01ResourceAssets.Build(); Region02ResourceAssets.Build();
            Region03ResourceAssets.Build(); Region04ResourceAssets.Build();
            Region05ResourceAssets.Build(); Region06ResourceAssets.Build();
            FacetedDetailMaterials.Apply();
            Debug.Log("Loose resource improvements rebuilt. Visual, gameplay and device testing are pending.");
        }
    }
}
