using System.IO;
using UnityEditor;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Region06ResourceAssets
    {
        private const string Source="Assets/_Game/Art/Items/Region06/";
        private const string Target="Assets/_Game/Resources/Region06/";
        private static readonly string[] Names={"Reeds","Wheat","Sand"};
        static Region06ResourceAssets(){EditorApplication.update+=EnsureImported;}
        private static void EnsureImported()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=EnsureImported;
            foreach(string name in Names)if(!File.Exists(Target+name+".prefab")){Build();return;}
        }
        [MenuItem("The Trail Walker/Art/Rebuild Region 6 Resource Prefabs")]
        public static void Build()=>Region01ResourceAssets.BuildRegion(Source,Target,"Region06Palette","Region06",Names);
        [MenuItem("The Trail Walker/Art/Capture Region 6 Resource Preview")]
        public static void CapturePreview()=>Region01ResourceAssets.CaptureRegionPreview(Target,Names,"Docs/Art/Region06/resource-meshes-unity-v1.png");
    }
}
