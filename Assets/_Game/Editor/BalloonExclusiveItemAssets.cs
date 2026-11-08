using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    public static class BalloonExclusiveItemAssets
    {
        const string Source="Assets/_Game/Art/Crafting/BalloonExclusive/",Target="Assets/_Game/Resources/BalloonExclusive/",Docs="Docs/Art/BalloonExclusive/";
        const string LegacySource="Assets/_Game/Art/Crafting/Tier01/",LegacyTarget="Assets/_Game/Resources/Tier01/Crafted/";
        [Serializable] class Data { public Item[] Items;public int FoodCount,MiscCount,BarrelCount; }
        [Serializable] class Item {public string RecipeId,OutputId,Name,Category,IngredientBrief,Model,PrefabPath,Kind,DrinkKey,RequiredRecipeId,PrerequisiteBrief;public int Tier,Triangles,Yield,HeartsPerServing;public bool Reused,FutureTierArtOnly,Consumable;}
        static Data Load()=>JsonUtility.FromJson<Data>(File.ReadAllText(Source+"BalloonExclusiveManifest.json"));
        [MenuItem("The Trail Walker/Art/Build Mystery Box Exclusive Outputs")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Migrate and build exclusive art outside Play mode.");
            Directory.CreateDirectory(Target+"Items");Directory.CreateDirectory(Docs);AssetDatabase.Refresh();
            var data=Load();
            string report="GUID-preserving exclusive-art migration\n";
            foreach(var item in data.Items.Where(x=>x.Reused))
            {
                foreach(var pair in new[]{new[]{LegacySource+item.Model+".obj",Source+item.Model+".obj"},new[]{LegacyTarget+item.Model+".prefab",Target+"Items/"+item.Model+".prefab"}})
                {
                    if(!File.Exists(pair[0]))
                    {
                        if(!File.Exists(pair[1]))throw new InvalidOperationException("Missing existing exclusive asset "+pair[0]);
                        continue;
                    }
                    string guid=AssetDatabase.AssetPathToGUID(pair[0]);string error=AssetDatabase.MoveAsset(pair[0],pair[1]);
                    if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);
                    if(guid!=AssetDatabase.AssetPathToGUID(pair[1]))throw new InvalidOperationException("Asset GUID changed during migration.");
                    report+=pair[0]+" -> "+pair[1]+" / GUID "+guid+"\n";
                }
            }
            if(report.Contains(" -> "))File.WriteAllText(Docs+"asset-migration.txt",report);
            Region01ResourceAssets.BuildRegion(Source,Target+"Items/","Tier01CraftedPalette","BalloonExclusiveOutputs",data.Items.Select(x=>x.Model).ToArray());
            var bottleMaterial=AssetDatabase.LoadAssetAtPath<Material>(Target+"Items/BottledDrinks.mat");
            if(bottleMaterial==null){bottleMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(bottleMaterial,Target+"Items/BottledDrinks.mat");}
            bottleMaterial.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"Tier01CraftedPalette.png"));bottleMaterial.SetColor("_BaseColor",Color.white);bottleMaterial.SetFloat("_Smoothness",.42f);EditorUtility.SetDirty(bottleMaterial);
            foreach(var item in data.Items.Where(x=>x.Kind=="BottledDrink"))
            {
                string prefabPath=Target+"Items/"+item.Model+".prefab";var root=PrefabUtility.LoadPrefabContents(prefabPath);
                try{foreach(var r in root.GetComponentsInChildren<Renderer>())r.sharedMaterials=Enumerable.Repeat(bottleMaterial,r.sharedMaterials.Length).ToArray();PrefabUtility.SaveAsPrefabAsset(root,prefabPath);}
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            const string path=Target+"BalloonExclusiveArtCatalog.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<BalloonExclusiveArtCatalog>(path);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<BalloonExclusiveArtCatalog>();AssetDatabase.CreateAsset(catalog,path);}
            catalog.Outputs=data.Items.Select(x=>new BalloonExclusiveOutputArt{RecipeId=x.RecipeId,OutputId=x.OutputId,Name=x.Name,Category=x.Category,IngredientBrief=x.IngredientBrief,Tier=x.Tier,FutureTierArtOnly=x.FutureTierArtOnly,Kind=x.Kind,DrinkKey=x.DrinkKey,RequiredRecipeId=x.RequiredRecipeId,PrerequisiteBrief=x.PrerequisiteBrief,Yield=x.Yield,HeartsPerServing=x.HeartsPerServing,Consumable=x.Consumable,Visual=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Items/"+x.Model+".prefab")}).ToArray();
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            // Refresh the existing tier-one authoring catalog's references after moving those visuals.
            var tierOne=AssetDatabase.LoadAssetAtPath<Tier1RecipeArtCatalog>("Assets/_Game/Resources/Tier01/RecipeArtCatalog.asset");
            foreach(var recipe in tierOne.Recipes)
            {
                var moved=data.Items.FirstOrDefault(x=>x.Reused&&x.RecipeId==recipe.Id);if(moved==null)continue;
                recipe.PrefabPath=moved.PrefabPath;recipe.Visual=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Items/"+moved.Model+".prefab");
            }
            EditorUtility.SetDirty(tierOne);AssetDatabase.SaveAssets();Verify();
            Debug.Log(data.Items.Length+" mystery-box-exclusive output visuals built, including non-consumable barrels. No expansion activation or recipe grants.");
        }
        [MenuItem("The Trail Walker/Art/Capture Mystery Box Exclusive Outputs")]
        public static void CapturePreview()
        {
            Directory.CreateDirectory(Docs);var data=Load();
            foreach(var item in data.Items)Capture(new[]{item},item.Model+".png",1,true);
            for(int tier=1;tier<=8;tier++)Capture(data.Items.Where(x=>x.Tier==tier).ToArray(),"tier"+tier.ToString("00")+"-exclusive-items.png",4,false);
            Capture(data.Items.Where(x=>x.Category=="Misc").ToArray(),"exclusive-misc-gallery.png",3,false);
            Capture(data.Items.Where(x=>x.Kind=="FilledBarrel").ToArray(),"filled-oak-barrels.png",3,false);
            Capture(data.Items.Where(x=>x.Kind=="BottledDrink").ToArray(),"bottled-drinks.png",3,false);
            Capture(data.Items.Where(x=>x.Kind=="FilledBarrel"||x.Kind=="BottledDrink").ToArray(),"barrel-to-bottle-pairs.png",6,false);
            Verify();
        }
        static void Capture(Item[] items,string file,int columns,bool single)
        {
            int rows=(items.Length+columns-1)/columns;var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<items.Length;i++)
                {
                    var source=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Items/"+items[i].Model+".prefab");
                    var view=UnityEngine.Object.Instantiate(source);var renderers=view.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;
                    foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                    float scale=.82f/Mathf.Max(bounds.size.x,bounds.size.y);view.transform.localScale=Vector3.one*scale;
                    view.transform.position=new Vector3((i%columns-(columns-1)*.5f)*1.05f,((rows-1)*.5f-i/columns)*1.05f,0)-bounds.center*scale;preview.AddSingleGO(view);
                }
                preview.camera.orthographic=true;preview.camera.orthographicSize=rows*.55f;
                preview.camera.transform.position=new Vector3(0,0,-8);preview.camera.transform.LookAt(Vector3.zero);
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.17f,.21f,.23f);preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=40;
                preview.lights[0].intensity=1.4f;preview.lights[0].transform.rotation=Quaternion.Euler(30,30,0);preview.lights[1].intensity=.7f;preview.ambientColor=new Color(.5f,.48f,.44f);
                preview.BeginStaticPreview(new Rect(0,0,columns*(single?700:420),rows*(single?700:420)));preview.Render(true);var image=preview.EndStaticPreview();
                try{File.WriteAllBytes(Docs+file,image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
            }
            finally{preview.Cleanup();}
            File.WriteAllLines(Docs+Path.GetFileNameWithoutExtension(file)+"-labels.txt",items.Select(x=>x.RecipeId+" | "+x.Name+" | tier "+x.Tier));
        }
        public static void Verify()
        {
            var data=Load();var catalog=AssetDatabase.LoadAssetAtPath<BalloonExclusiveArtCatalog>(Target+"BalloonExclusiveArtCatalog.asset");
            if(catalog.Outputs.Length!=data.Items.Length||catalog.Outputs.Select(x=>x.RecipeId).Distinct().Count()!=data.Items.Length||catalog.Outputs.Count(x=>x.Category=="Food")!=data.FoodCount||catalog.Outputs.Count(x=>x.Category=="Misc")!=data.MiscCount||catalog.Outputs.Count(x=>x.Category=="Barrel")!=data.BarrelCount)throw new InvalidOperationException("Exclusive recipe coverage mismatch.");
            foreach(var item in data.Items)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Items/"+item.Model+".prefab");if(prefab==null)throw new InvalidOperationException(item.Model);
                foreach(var filter in prefab.GetComponentsInChildren<MeshFilter>())if(filter.sharedMesh==null)throw new InvalidOperationException("Missing mesh "+item.Model);
                if(item.Reused&&(File.Exists(LegacySource+item.Model+".obj")||File.Exists(LegacyTarget+item.Model+".prefab")))throw new InvalidOperationException("Duplicate legacy exclusive "+item.Model);
                if(item.Tier>4&&!item.FutureTierArtOnly)throw new InvalidOperationException("Expansion art must remain marked inactive.");
                if(item.Reused&&TheTrailWalker.Domain.Items.Tier1CraftedItems.VisualPath(item.OutputId)!=item.PrefabPath)throw new InvalidOperationException("Moved item lookup mismatch.");
                if(item.Kind=="FilledBarrel"&&(item.Category!="Barrel"||item.Consumable||item.Yield!=1||item.HeartsPerServing!=0))throw new InvalidOperationException("Filled barrel must remain non-consumable.");
                if(item.Kind=="BottledDrink"&&(item.Category!="Food"||!item.Consumable||item.Yield!=8||!data.Items.Any(x=>x.RecipeId==item.RequiredRecipeId&&x.Kind=="FilledBarrel"&&x.DrinkKey==item.DrinkKey)))throw new InvalidOperationException("Incorrect paired bottling recipe art.");
            }
            File.WriteAllText(Docs+"validation.txt","PASS: "+data.Items.Length+" unique mystery-box recipe outputs ("+data.FoodCount+" Food, "+data.MiscCount+" Misc, "+data.BarrelCount+" non-consumable barrels); all prefab meshes resolve; paired filling/bottling identities and x1/x8 yields match; previous moved assets remain outside mission directories; "+data.Items.Count(x=>x.FutureTierArtOnly)+" expansion-tier visuals remain art-only. No recipe, reward or expansion activation.");
        }
    }
    internal sealed class BalloonExclusiveImport : AssetPostprocessor
    {
        const string Source="Assets/_Game/Art/Crafting/BalloonExclusive/";
        void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;m.addCollider=false;
            m.importNormals=ModelImporterNormals.Calculate;m.normalSmoothingAngle=0;m.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        void OnPreprocessTexture()
        {
            if(assetPath!=Source+"Tier01CraftedPalette.png")return;
            var t=(TextureImporter)assetImporter;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
