using System.IO;
using UnityEditor;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Region05ResourceAssets
    {
        private const string Source="Assets/_Game/Art/Items/Region05/";
        private const string Target="Assets/_Game/Resources/Region05/";
        private static readonly string[] Names={"Oats","Pebbles","RedBerries"};
        static Region05ResourceAssets(){EditorApplication.update+=EnsureImported;}
        private static void EnsureImported()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=EnsureImported;
            foreach(string name in Names)if(!File.Exists(Target+name+".prefab")){Build();return;}
        }
        [MenuItem("The Trail Walker/Art/Rebuild Region 5 Resource Prefabs")]
        public static void Build()=>Region01ResourceAssets.BuildRegion(Source,Target,"Region05Palette","Region05",Names);
        [MenuItem("The Trail Walker/Art/Capture Region 5 Resource Preview")]
        public static void CapturePreview()=>Region01ResourceAssets.CaptureRegionPreview(Target,Names,"Docs/Art/Region05/resource-meshes-unity-v1.png");
    }
}
