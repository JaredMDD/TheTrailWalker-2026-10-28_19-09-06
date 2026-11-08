using System;
using System.IO;
using System.Linq;
using TheTrailWalker.Domain.Items;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Tier01CraftedAssets
    {
        private const string Source="Assets/_Game/Art/Crafting/Tier01/";
        private const string Target="Assets/_Game/Resources/Tier01/Crafted/";
        private const string CatalogPath="Assets/_Game/Resources/Tier01/RecipeArtCatalog.asset";
        [Serializable] private sealed class Data
        {
            public int Revision;
            public string UnlockPolicy,ArtPolicy;
            public RecipeArtDesign[] Recipes;
            public RecipeExternalInputArt[] ExternalInputs;
            public CraftedModelArt[] Models;
            public RecipeSourceDesign[] Sources;
        }
        static Tier01CraftedAssets(){EditorApplication.update+=Ensure;}
        private static void Ensure()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=Ensure;
            if(!File.Exists(CatalogPath))Build();
        }
        [MenuItem("The Trail Walker/Art/Rebuild Tier 1 Crafted Assets")]
        public static void Build()
        {
            Data data=JsonUtility.FromJson<Data>(File.ReadAllText(Source+"Tier01Recipes.json"));
            // Reject broken authoring references before publishing the catalog.
            var ids=PrototypeItems.All.ToDictionary(x=>x.Id);
            foreach(var recipe in data.Recipes)
            {
                if(!ids.ContainsKey(recipe.OutputId)||recipe.Tier!=1||recipe.Yield<1)throw new InvalidOperationException(recipe.Id);
                foreach(var input in recipe.Inputs)
                    if(!ids.ContainsKey(input.ItemId)||input.Quantity<1)throw new InvalidOperationException(recipe.Id+": "+input.ItemId);
            }
            var exclusive=data.Recipes.Where(x=>x.UnlockSource.StartsWith("Optional balloon recipe",StringComparison.Ordinal)).Select(x=>x.Model).ToHashSet();
            Region01ResourceAssets.BuildRegion(Source,Target,"Tier01CraftedPalette","Tier01Crafted",data.Models.Where(x=>!exclusive.Contains(x.Model)).Select(x=>x.Model).ToArray());
            Region01ResourceAssets.BuildRegion("Assets/_Game/Art/Crafting/BalloonExclusive/","Assets/_Game/Resources/BalloonExclusive/Items/","Tier01CraftedPalette","Tier01BoxExclusives",data.Models.Where(x=>exclusive.Contains(x.Model)).Select(x=>x.Model).ToArray());
            var catalog=AssetDatabase.LoadAssetAtPath<Tier1RecipeArtCatalog>(CatalogPath);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<Tier1RecipeArtCatalog>();AssetDatabase.CreateAsset(catalog,CatalogPath);}
            catalog.Revision=data.Revision;catalog.UnlockPolicy=data.UnlockPolicy;catalog.ArtPolicy=data.ArtPolicy;
            foreach(var r in data.Recipes)r.Visual=Visual(r.PrefabPath);
            foreach(var e in data.ExternalInputs)e.Visual=Visual(e.PrefabPath);
            catalog.Recipes=data.Recipes;catalog.ExternalInputs=data.ExternalInputs;catalog.Models=data.Models;catalog.Sources=data.Sources;
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            Debug.Log("Tier 1 recipe art: "+data.Recipes.Length+" designs, "+data.Models.Length+" new models. No recipe knowledge granted.");
        }
        private static GameObject Visual(string path)
        {
            var result=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Resources/"+path+".prefab");
            if(result==null)throw new InvalidOperationException("Missing recipe visual "+path);
            return result;
        }
        [MenuItem("The Trail Walker/Art/Capture Tier 1 Crafted Previews")]
        public static void CapturePreview()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<Tier1RecipeArtCatalog>(CatalogPath);
            if(catalog==null)throw new InvalidOperationException("Build first.");
            foreach(string category in new[]{"Material","Tool","Wearable","Food","Misc"})
            {
                var ordinary=catalog.Recipes.Where(x=>x.Category==category&&!x.UnlockSource.StartsWith("Optional balloon recipe",StringComparison.Ordinal)).ToArray();
                Capture(category,ordinary.Select(x=>x.Visual).ToArray(),ordinary.Select(x=>x.Name).ToArray());
            }
            Capture("ShopInputs",catalog.ExternalInputs.Select(x=>x.Visual).ToArray(),catalog.ExternalInputs.Select(x=>x.Name).ToArray());
        }
        [MenuItem("The Trail Walker/Art/Rebuild Boots Item Only")]
        public static void RebuildBootItem()
        {
            Region01ResourceAssets.BuildRegion(Source,Target,"Tier01CraftedPalette","Tier01BootItem",new[]{"RabbitBoots"});
            var data=JsonUtility.FromJson<Data>(File.ReadAllText(Source+"Tier01Recipes.json"));
            var catalog=AssetDatabase.LoadAssetAtPath<Tier1RecipeArtCatalog>(CatalogPath);
            var revised=data.Models.Single(x=>x.Model=="RabbitBoots");
            catalog.Models=Array.ConvertAll(catalog.Models,x=>x.Model=="RabbitBoots"?revised:x);
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            var boot=Visual("Tier01/Crafted/RabbitBoots");
            Capture("BootItemProfile",new[]{boot},new[]{"Rabbit-pelt boots / single side-profile item mesh"},true);
            Capture("BootItemStudy",new[]{boot},new[]{"Rabbit-pelt boots / single side-profile item mesh"});
        }
        private static void Capture(string category,GameObject[] prefabs,string[] names,bool frontView=false)
        {
            string directory="Docs/Art/Tier01/Crafted";Directory.CreateDirectory(directory);
            // Render separate cells so camera elevation cannot displace later rows; labels composed by the source tool.
            for(int i=0;i<prefabs.Length;i++)
            {
                var preview=new PreviewRenderUtility();
                try
                {
                    GameObject view=UnityEngine.Object.Instantiate(prefabs[i]);preview.AddSingleGO(view);
                    var renderers=view.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;
                    foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
                    view.transform.position=-bounds.center;
                    preview.camera.orthographic=true;
                    preview.camera.orthographicSize=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*.72f;
                    preview.camera.transform.position=frontView?new Vector3(0,0,-5):new Vector3(0,1.8f,-5);preview.camera.transform.LookAt(Vector3.zero);
                    preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.19f,.22f,.25f);
                    preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=30;
                    preview.lights[0].intensity=1.2f;preview.lights[0].transform.rotation=Quaternion.Euler(40,30,0);
                    preview.lights[1].intensity=.6f;preview.ambientColor=new Color(.4f,.4f,.4f);
                    preview.BeginStaticPreview(new Rect(0,0,320,280));preview.Render(true);
                    Texture2D image=preview.EndStaticPreview();
                    try{File.WriteAllBytes(directory+"/"+category+"-"+i.ToString("00")+".png",image.EncodeToPNG());}
                    finally{UnityEngine.Object.DestroyImmediate(image);}
                }
                finally{preview.Cleanup();}
            }
            File.WriteAllLines(directory+"/"+category+"-labels.txt",names);
        }
    }
    internal sealed class Tier01CraftedImport : AssetPostprocessor
    {
        private const string Source="Assets/_Game/Art/Crafting/Tier01/";
        private void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;
            m.addCollider=false;m.importNormals=ModelImporterNormals.Calculate;m.normalSmoothingAngle=0;m.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        private void OnPreprocessTexture()
        {
            if(assetPath!=Source+"Tier01CraftedPalette.png")return;
            var t=(TextureImporter)assetImporter;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;
            t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
