using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    public static class BalloonGoodsAssets
    {
        const string Source="Assets/_Game/Art/Items/BalloonGoods/",Target="Assets/_Game/Resources/BalloonGoods/",Docs="Docs/Art/BalloonGoods/";
        const string Player="Assets/_Game/Art/Characters/PlayerDefault/Unity/";
        static readonly string[] Styles={"Crop","Sweep","Bob","Tied"};
        [Serializable] class Data { public Part[] Parts;public Item[] Items; }
        [Serializable] class Part { public string Name;public int Tier,Triangles; }
        [Serializable] class Item { public string Id,Name,Slot,Anchor,ItemMesh,MaleMesh,FemaleMesh;public int Tier; }
        static Data data;static Material[] materials;
        static void Load(){data=JsonUtility.FromJson<Data>(File.ReadAllText(Source+"BalloonGoodsManifest.json"));materials=Enumerable.Range(1,4).Select(i=>AssetDatabase.LoadAssetAtPath<Material>(Target+"Materials/Tier"+i.ToString("00")+".mat")).ToArray();}
        [MenuItem("The Trail Walker/Art/Build Balloon Special Gear And Boxes")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build balloon goods outside Play mode.");
            foreach(string folder in new[]{"Materials","Items","Wearables/Male","Wearables/Female","HatHair","MysteryBoxes","Animation"})Directory.CreateDirectory(Target+folder);
            AssetDatabase.Refresh();Load();
            for(int i=0;i<4;i++)
            {
                string path=Target+"Materials/Tier"+(i+1).ToString("00")+".mat";
                var mat=materials[i];if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
                mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"BalloonGoodsTier"+(i+1).ToString("00")+"Palette.png"));
                mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_Smoothness",.10f);FacetedDetailMaterials.Configure(mat,mat.GetTexture("_BaseMap"));EditorUtility.SetDirty(mat);materials[i]=mat; // surface detail (November 7, 2026)
            }
            var catalog=AssetDatabase.LoadAssetAtPath<BalloonGoodsArtCatalog>(Target+"BalloonGoodsArtCatalog.asset");
            if(catalog==null){catalog=ScriptableObject.CreateInstance<BalloonGoodsArtCatalog>();AssetDatabase.CreateAsset(catalog,Target+"BalloonGoodsArtCatalog.asset");}
            catalog.SpecialItems=data.Items.Select(item=>new BalloonSpecialItemArt{
                Id=item.Id,Name=item.Name,Tier=item.Tier,Slot=item.Slot,Anchor=item.Anchor,
                Display=SaveMesh(item.ItemMesh,Target+"Items/"+item.ItemMesh+".prefab",materials[item.Tier-1]),
                MaleWearable=SaveMesh(item.MaleMesh,Target+"Wearables/Male/"+item.MaleMesh+".prefab",materials[item.Tier-1]),
                FemaleWearable=SaveMesh(item.FemaleMesh,Target+"Wearables/Female/"+item.FemaleMesh+".prefab",materials[item.Tier-1])}).ToArray();
            catalog.MaleHatHair=Styles.Select(style=>SaveMesh("MaleHatHair"+style,Target+"HatHair/MaleHatHair"+style+".prefab",AssetDatabase.LoadAssetAtPath<Material>(Player+"Materials/Hair02.mat"))).ToArray();
            catalog.FemaleHatHair=Styles.Select(style=>SaveMesh("FemaleHatHair"+style,Target+"HatHair/FemaleHatHair"+style+".prefab",AssetDatabase.LoadAssetAtPath<Material>(Player+"Materials/Hair02.mat"))).ToArray();
            var controller=BuildBoxController();catalog.MysteryBoxes=new BalloonMysteryBoxArt[4];
            for(int tier=1;tier<=4;tier++)
            {
                string materialPath=Target+"Materials/MysteryBoxTier"+tier.ToString("00")+".mat";
                var boxMaterial=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(boxMaterial==null){boxMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(boxMaterial,materialPath);}
                boxMaterial.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"MysteryBoxTier"+tier.ToString("00")+"Palette.png"));boxMaterial.SetColor("_BaseColor",Color.white);boxMaterial.SetFloat("_Smoothness",.12f);FacetedDetailMaterials.Configure(boxMaterial,boxMaterial.GetTexture("_BaseMap"));EditorUtility.SetDirty(boxMaterial);
                string path=Target+"MysteryBoxes/MysteryBoxTier"+tier.ToString("00")+".prefab";
                var box=new GameObject("MysteryBoxTier"+tier.ToString("00"));
                try
                {
                    Add(box.transform,"MysteryBoxBody",boxMaterial);Add(box.transform,"MysteryTier"+tier+"Seal",boxMaterial);
                    var hinge=new GameObject("LidPivot").transform;hinge.SetParent(box.transform,false);hinge.localPosition=new Vector3(0,.39f,.30f);
                    Add(hinge,"MysteryBoxLid",boxMaterial);
                    var animator=box.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
                    catalog.MysteryBoxes[tier-1]=new BalloonMysteryBoxArt{Tier=tier,FirstRegion=(tier-1)*6+1,LastRegion=tier*6,Material=boxMaterial,Prefab=PrefabUtility.SaveAsPrefabAsset(box,path)};
                }
                finally{UnityEngine.Object.DestroyImmediate(box);}
            }
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();Verify();
            // Rebuilding the prefabs resets them to the plain tier material; re-apply the isolated tier-one merchant surfaces
            // (Tier01MerchantItemDesignAssets) so a rebuild never drops them (regression fixed November 7, 2026).
            if(File.Exists("Assets/_Game/Art/Items/Tier01Design/MerchantItemDesignManifest.json"))Tier01MerchantItemDesignAssets.Build();
            Debug.Log("Balloon goods art: 20 display models, 40 wearable fits, 8 hat hair fits and 4 animated box tiers. No stock, rewards or equip wiring changed.");
        }
        static GameObject SaveMesh(string mesh,string path,Material material)
        {
            var root=new GameObject(mesh);
            try{Add(root.transform,mesh,material);return PrefabUtility.SaveAsPrefabAsset(root,path);}finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        static GameObject Add(Transform parent,string name,Material material)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Source+name+".obj");if(source==null)throw new InvalidOperationException("Missing mesh "+name);
            var part=UnityEngine.Object.Instantiate(source,parent,false);part.name=name;
            foreach(var renderer in part.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=Enumerable.Repeat(material,renderer.sharedMaterials.Length).ToArray();
            return part;
        }
        static AnimationClip Clip(string name,float from,float to,float duration)
        {
            string path=Target+"Animation/"+name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,path);}clip.ClearCurves();clip.frameRate=30;
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("LidPivot",typeof(Transform),"localEulerAnglesRaw.x"),AnimationCurve.EaseInOut(0,from,duration,to));
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(clip,settings);EditorUtility.SetDirty(clip);return clip;
        }
        static AnimatorController BuildBoxController()
        {
            string path=Target+"Animation/MysteryBox.controller";var c=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if(c==null)c=AnimatorController.CreateAnimatorControllerAtPath(path);
            foreach(var parameter in c.parameters.ToArray())c.RemoveParameter(parameter);
            c.AddParameter("Open",AnimatorControllerParameterType.Trigger);c.AddParameter("Close",AnimatorControllerParameterType.Trigger);
            var sm=c.layers[0].stateMachine;foreach(var child in sm.states)sm.RemoveState(child.state);
            var closed=sm.AddState("Closed");closed.motion=Clip("MysteryBoxClosed",0,0,.1f);
            var opening=sm.AddState("Opening");opening.motion=Clip("MysteryBoxOpen",0,105,1.0f);
            var opened=sm.AddState("Opened");opened.motion=Clip("MysteryBoxOpened",105,105,.1f);
            var closing=sm.AddState("Closing");closing.motion=Clip("MysteryBoxClose",105,0,.8f);sm.defaultState=closed;
            var a=closed.AddTransition(opening);a.hasExitTime=false;a.duration=0;a.AddCondition(AnimatorConditionMode.If,0,"Open");
            var b=opening.AddTransition(opened);b.hasExitTime=true;b.exitTime=1;b.duration=0;
            var d=opened.AddTransition(closing);d.hasExitTime=false;d.duration=0;d.AddCondition(AnimatorConditionMode.If,0,"Close");
            var e=closing.AddTransition(closed);e.hasExitTime=true;e.exitTime=1;e.duration=0;EditorUtility.SetDirty(c);return c;
        }
        static BalloonGoodsArtCatalog Catalog()=>AssetDatabase.LoadAssetAtPath<BalloonGoodsArtCatalog>(Target+"BalloonGoodsArtCatalog.asset");
        static GameObject Dressed(int tier,bool female,string style="Sweep")
        {
            string sex=female?"Female":"Male";
            var avatar=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Player+"Avatars/"+sex+"Skin03.prefab"));
            foreach(Transform child in avatar.transform)if(child.name.EndsWith("BaseShirt")||child.name.EndsWith("BasePants"))child.gameObject.SetActive(false);
            var head=avatar.transform.Find("HeadAnchor");head.Find("HairAnchor").gameObject.SetActive(false);
            int hair=Array.IndexOf(Styles,style);var hairPrefab=(female?Catalog().FemaleHatHair:Catalog().MaleHatHair)[hair];
            UnityEngine.Object.Instantiate(hairPrefab,head,false);
            foreach(var item in Catalog().SpecialItems.Where(x=>x.Tier==tier))
            {
                Transform anchor=item.Anchor=="Root"?avatar.transform:item.Anchor=="HeadAnchor"?head:avatar.transform.Find(item.Anchor);
                UnityEngine.Object.Instantiate(female?item.FemaleWearable:item.MaleWearable,anchor,false);
            }
            return avatar;
        }
        [MenuItem("The Trail Walker/Art/Capture Balloon Special Gear And Boxes")]
        public static void CapturePreview()
        {
            Load();Directory.CreateDirectory(Docs);
            for(int tier=1;tier<=4;tier++)
            {
                var objects=Catalog().SpecialItems.Where(x=>x.Tier==tier).Select(x=>UnityEngine.Object.Instantiate(x.Display)).ToArray();
                Sheet("tier"+tier.ToString("00")+"-items.png",objects,5,1,1.05f,1,.68f,false);
            }
            var outfits=Enumerable.Range(0,8).Select(i=>Dressed(i%4+1,i>=4,i>=4?"Tied":"Sweep")).ToArray();
            Sheet("special-gear-avatar-fits.png",outfits,4,2,1.12f,2.10f,2.15f,true);
            var backs=Enumerable.Range(0,8).Select(i=>{var a=Dressed(i%4+1,i>=4);a.transform.rotation=Quaternion.Euler(0,165,0);return a;}).ToArray();
            Sheet("special-gear-backpack-fits.png",backs,4,2,1.12f,2.10f,2.15f,true);
            foreach(bool female in new[]{false,true})
            {
                var heads=Enumerable.Range(0,16).Select(i=>
                {
                    var a=Dressed(i/4+1,female,Styles[i%4]);
                    foreach(Transform child in a.transform)if(child.name!="HeadAnchor")child.gameObject.SetActive(false);
                    a.transform.Find("HeadAnchor").localPosition=Vector3.zero;return a;
                }).ToArray();
                Sheet(female?"female-hat-hair-fits.png":"male-hat-hair-fits.png",heads,4,4,.56f,.55f,1.23f,true,true);
            }
            var boxes=Enumerable.Range(0,8).Select(i=>
            {
                var a=UnityEngine.Object.Instantiate(Catalog().MysteryBoxes[i%4].Prefab);UnityEngine.Object.DestroyImmediate(a.GetComponent<Animator>());
                a.transform.Find("LidPivot").localRotation=Quaternion.Euler(i>=4?105:0,0,0);a.transform.rotation=Quaternion.Euler(0,-15,0);return a;
            }).ToArray();
            Sheet("mystery-box-tier-colors.png",boxes,4,2,1.18f,1.05f,1.25f,false);
            var phases=Enumerable.Range(0,5).Select(i=>
            {
                var a=UnityEngine.Object.Instantiate(Catalog().MysteryBoxes[0].Prefab);UnityEngine.Object.DestroyImmediate(a.GetComponent<Animator>());
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Target+"Animation/MysteryBoxOpen.anim").SampleAnimation(a,i/4f);return a;
            }).ToArray();
            Sheet("mystery-box-opening-study.png",phases,5,1,1.16f,1,.65f,false);Verify();
        }
        static void Sheet(string file,GameObject[] objects,int columns,int rows,float pitch,float rowPitch,float size,bool avatar,bool head=false)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<objects.Length;i++)
                {
                    int col=i%columns,row=i/columns;
                    objects[i].transform.position=new Vector3((avatar?(columns-1)*.5f-col:col-(columns-1)*.5f)*pitch,(rows-1-row)*rowPitch,0);preview.AddSingleGO(objects[i]);
                }
                float centerY=head?(rows-1)*rowPitch*.5f+.13f:avatar?(rows-1)*rowPitch*.5f+.88f:(rows-1)*rowPitch*.5f+.25f;
                if(!avatar&&rows==1&&file.Contains("items"))centerY=0;
                if(file.Contains("opening-study")){centerY=.53f;size=.85f;}
                if(file.Contains("tier-colors"))size=1.34f;
                preview.camera.orthographic=true;preview.camera.orthographicSize=size;
                preview.camera.transform.position=new Vector3(0,centerY+(file.Contains("mystery")?2.5f:0),avatar?8:-8);preview.camera.transform.LookAt(new Vector3(0,centerY,0));
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.17f,.21f,.23f);preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=40;
                preview.lights[0].intensity=1.35f;preview.lights[0].transform.rotation=Quaternion.Euler(30,avatar?210:30,0);preview.lights[1].intensity=.7f;preview.ambientColor=new Color(.5f,.49f,.45f);
                int width=columns*420,height=rows==1?480:rows==4?1700:file.Contains("mystery")?900:1500;
                if(file.Contains("opening-study"))height=600;
                preview.BeginStaticPreview(new Rect(0,0,width,height));preview.Render(true);var image=preview.EndStaticPreview();
                try{File.WriteAllBytes(Docs+file,image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
            }
            finally{preview.Cleanup();}
        }
        public static void Verify()
        {
            Directory.CreateDirectory(Docs);var catalog=Catalog();
            if(catalog.SpecialItems.Length!=20||catalog.SpecialItems.Select(x=>x.Id).Distinct().Count()!=20)throw new InvalidOperationException("Expected 20 exact special IDs.");
            foreach(var item in catalog.SpecialItems)if(item.Display==null||item.MaleWearable==null||item.FemaleWearable==null)throw new InvalidOperationException("Missing art "+item.Id);
            for(int region=1;region<=24;region++)if(catalog.MysteryBoxForCampRegion(region).Tier!=(region-1)/6+1)throw new InvalidOperationException("Camp region art tier mismatch.");
            var view=UnityEngine.Object.Instantiate(catalog.MysteryBoxes[0].Prefab);UnityEngine.Object.DestroyImmediate(view.GetComponent<Animator>());
            try
            {
                var hinge=view.transform.Find("LidPivot");var body=view.transform.Find("MysteryBoxBody");Vector3 bodyPosition=body.position;
                foreach(string name in new[]{"MysteryBoxOpen","MysteryBoxClose"})
                {
                    var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(Target+"Animation/"+name+".anim");
                    for(int i=0;i<=40;i++)
                    {
                        clip.SampleAnimation(view,clip.length*i/40);float angle=Quaternion.Angle(Quaternion.identity,hinge.localRotation);
                        if(angle>105.1f||Vector3.Distance(body.position,bodyPosition)>.00001f||Vector3.Distance(hinge.localPosition,new Vector3(0,.39f,.30f))>.00001f)throw new InvalidOperationException("Box hinge moved or exceeded its range.");
                    }
                    float expected=name.EndsWith("Open")?105:0;if(Mathf.Abs(Quaternion.Angle(Quaternion.identity,hinge.localRotation)-expected)>.1f)throw new InvalidOperationException("Incorrect lid endpoint.");
                }
                if(catalog.MysteryBoxes.Select(x=>x.Material).Distinct().Count()!=4)throw new InvalidOperationException("Box tier colors must be distinct.");
                File.WriteAllText(Docs+"validation.txt","PASS: 20 distinct listed item IDs, 20 display prefabs, 40 wearable fits, 8 hair fits, 4 distinct box materials; every camp-region mapping 1-24; actual open/close clips sampled 41 times each, fixed body and hinge, correct 0/105 degree endpoints. Art only, no shop binding or Android test.");
            }
            finally{UnityEngine.Object.DestroyImmediate(view);}
        }
    }
    internal sealed class BalloonGoodsImport : AssetPostprocessor
    {
        const string Source="Assets/_Game/Art/Items/BalloonGoods/";
        void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;m.addCollider=false;
            m.importNormals=ModelImporterNormals.Calculate;m.normalSmoothingAngle=45;m.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal)||!assetPath.EndsWith("Palette.png",StringComparison.Ordinal))return;
            var t=(TextureImporter)assetImporter;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
