using System;
using System.IO;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Harvest;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Tier01HarvestAssets
    {
        private const string Source="Assets/_Game/Art/Harvest/Tier01/";
        private const string Drops="Assets/_Game/Resources/Tier01/Drops/";
        private const string Actors="Assets/_Game/Resources/Tier01/Interactables/";
        private static readonly string[] DropNames={"PineLog","PineNuts","RawRabbitMeat","Bone","AnimalFat","RainbowTrout","LargemouthBass","Bluegill","YellowPerch"};
        static Tier01HarvestAssets(){EditorApplication.update+=EnsureImported;}
        private static void EnsureImported()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=EnsureImported;
            if(!File.Exists("Assets/_Game/Resources/Tier01/HarvestArtCatalog.asset")){Build();return;}
            foreach(string name in DropNames)if(!File.Exists(Drops+name+".prefab")){Build();return;}
            foreach(string name in new[]{"PineTrunk","PineTrunkMassive","Rabbit","FishingPoint"})if(!File.Exists(Actors+name+".prefab")){Build();return;}
        }
        [MenuItem("The Trail Walker/Art/Rebuild Tier 1 Harvest Assets")]
        public static void Build()
        {
            Region01ResourceAssets.BuildRegion(Source,Drops,"Tier01Palette","Tier01Drops",DropNames);
            Directory.CreateDirectory(Actors);
            BuildPrefab("PineTrunk",BuildPine);
            BuildPrefab("PineTrunkMassive",root=>{BuildPine(root);root.transform.localScale=Vector3.one*2.5f;});
            BuildPrefab("Rabbit",BuildRabbit);
            BuildPrefab("FishingPoint",BuildFishingPoint);
            BuildCatalog();AssetDatabase.SaveAssets();
            File.WriteAllText("Temp/Tier01Harvest-art.result","SUCCEEDED: 9 drop prefabs, 4 interactable prefabs and source catalog. "+DateTime.UtcNow.ToString("O"));
        }
        private static void BuildPrefab(string name,Action<GameObject> populate)
        {
            var root=new GameObject(name);
            try{populate(root);PrefabUtility.SaveAsPrefabAsset(root,Actors+name+".prefab");}
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        // Faceted revision 4 (user, November 7, 2026). Regenerate the meshes first: python Tools/Art/generate_tier01_rabbit.py
        public static void RebuildRabbitOnly()
        {
            AssetDatabase.ImportAsset(Source+"RabbitPalette.png",ImportAssetOptions.ForceUpdate);
            foreach(string name in new[]{"RabbitBody","RabbitHead","RabbitEar","RabbitFrontFoot","RabbitHindFoot","RabbitTail"})
                AssetDatabase.ImportAsset(Source+name+".obj",ImportAssetOptions.ForceUpdate);
            BuildPrefab("Rabbit",BuildRabbit);AssetDatabase.SaveAssets();CaptureRabbitPoses();CaptureRabbitAnatomy();
            Capture(true,"Docs/Art/Tier01/interactables-unity-v1.png");
        }
        private static Transform Pivot(Transform parent,string name,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }
        private static void Part(Transform parent,string sourceName)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Source+sourceName+".obj");
            if(source==null)throw new InvalidOperationException("Missing model "+sourceName);
            var visual=UnityEngine.Object.Instantiate(source,parent,false);visual.name=sourceName;
            // The faceted rabbit (revision 4) has its own palette strip; everything else shares the Tier 1 drops palette.
            Material material=sourceName.StartsWith("Rabbit",StringComparison.Ordinal)?RabbitMaterial():AssetDatabase.LoadAssetAtPath<Material>(Drops+"Tier01Palette.mat");
            foreach(Renderer renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                var slots=new Material[renderer.sharedMaterials.Length];for(int i=0;i<slots.Length;i++)slots[i]=material;renderer.sharedMaterials=slots;
            }
        }
        private static Material RabbitMaterial()
        {
            const string path=Source+"RabbitPalette.mat";
            var palette=AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"RabbitPalette.png");
            if(palette==null)throw new InvalidOperationException("Missing RabbitPalette.png; run Tools/Art/generate_tier01_rabbit.py");
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetFloat("_Smoothness",.05f);AssetDatabase.CreateAsset(material,path);}
            FacetedDetailMaterials.Configure(material,palette); // fur detail tile from the palette alpha
            return material;
        }
        private static Tier1HarvestVisual Marker(GameObject root,InteractableKind kind,Vector3 center,Vector3 size)
        {
            var marker=root.AddComponent<Tier1HarvestVisual>();marker.Kind=kind;
            var collider=root.AddComponent<BoxCollider>();collider.center=center;collider.size=size;
            // Milestone 5 scenery is nonblocking. Milestone 6 can explicitly enable its selection layer.
            collider.enabled=false;marker.SelectionBounds=collider;return marker;
        }
        private static void BuildPine(GameObject root)
        {
            var rig=root.AddComponent<Tier1PineRig>();
            rig.LowerTrunk=Pivot(root.transform,"LowerTrunk",Vector3.zero);Part(rig.LowerTrunk,"PineTrunkLower");
            rig.FallPivot=Pivot(root.transform,"FallPivot",new Vector3(0,.65f,0));Part(rig.FallPivot,"PineTrunkUpper");
            rig.CutArea=Pivot(root.transform,"CutArea",new Vector3(0,.75f,.32f));
            rig.HintAnchor=Pivot(root.transform,"AxeHintAnchor",new Vector3(0,3.1f,0));
            var marker=Marker(root,InteractableKind.Tree,new Vector3(0,1.4f,0),new Vector3(.92f,2.85f,.92f));
            marker.FocusAnchor=rig.CutArea;marker.HintAnchor=rig.HintAnchor;
        }
        private static void BuildRabbit(GameObject root)
        {
            var rig=root.AddComponent<Tier1RabbitRig>();
            rig.GroundAnchor=Pivot(root.transform,"GroundAnchor",Vector3.zero);
            rig.MotionRoot=Pivot(root.transform,"MotionRoot",Vector3.zero);
            rig.BodyPivot=Pivot(rig.MotionRoot,"BodyPivot",new Vector3(0,.20f,0));Part(rig.BodyPivot,"RabbitBody");
            rig.HeadPivot=Pivot(rig.BodyPivot,"HeadPivot",new Vector3(0,.14f,.23f));Part(rig.HeadPivot,"RabbitHead");
            rig.LeftEarPivot=Pivot(rig.HeadPivot,"LeftEarPivot",new Vector3(-.065f,.16f,.035f));Part(rig.LeftEarPivot,"RabbitEar");rig.LeftEarPivot.localRotation=Quaternion.Euler(-8,0,-9);
            rig.RightEarPivot=Pivot(rig.HeadPivot,"RightEarPivot",new Vector3(.065f,.16f,.035f));Part(rig.RightEarPivot,"RabbitEar");rig.RightEarPivot.localRotation=Quaternion.Euler(5,0,9);
            rig.FrontLeftPivot=Pivot(rig.MotionRoot,"FrontLeftPivot",new Vector3(-.083f,.07f,.17f));Part(rig.FrontLeftPivot,"RabbitFrontFoot");
            rig.FrontRightPivot=Pivot(rig.MotionRoot,"FrontRightPivot",new Vector3(.083f,.07f,.17f));Part(rig.FrontRightPivot,"RabbitFrontFoot");
            rig.HindLeftPivot=Pivot(rig.MotionRoot,"HindLeftPivot",new Vector3(-.125f,.11f,-.22f));Part(rig.HindLeftPivot,"RabbitHindFoot");
            rig.HindRightPivot=Pivot(rig.MotionRoot,"HindRightPivot",new Vector3(.125f,.11f,-.22f));Part(rig.HindRightPivot,"RabbitHindFoot");
            rig.TailPivot=Pivot(rig.BodyPivot,"TailPivot",new Vector3(0,.10f,-.32f));Part(rig.TailPivot,"RabbitTail");
            rig.HintAnchor=Pivot(root.transform,"BowHintAnchor",new Vector3(0,1.05f,0));
            var marker=Marker(root,InteractableKind.AnimalSpawn,new Vector3(0,.41f,0),new Vector3(.50f,.84f,.88f));
            marker.FocusAnchor=rig.HeadPivot;marker.HintAnchor=rig.HintAnchor;
        }
        private static void BuildFishingPoint(GameObject root)
        {
            Part(root.transform,"FishingPool");
            var marker=Marker(root,InteractableKind.FishingPoint,new Vector3(0,.04f,0),new Vector3(3.2f,.15f,2.4f));
            marker.RippleRoot=Pivot(root.transform,"RippleRoot",Vector3.zero);Part(marker.RippleRoot,"FishingRipples");
            marker.FocusAnchor=Pivot(root.transform,"FishingFocus",new Vector3(0,.15f,0));
            marker.HintAnchor=Pivot(root.transform,"RodHintAnchor",new Vector3(0,.70f,0));
        }
        private static HarvestDropArt Drop(ItemDefinition item,string path,int n0=0,int n1=0,int e0=0,int e1=0)
        {
            GameObject visual=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(visual==null)throw new InvalidOperationException("Missing drop art "+path);
            return new HarvestDropArt{ItemId=item.Id,Visual=visual,NormalMinBundles=n0,NormalMaxBundles=n1,EliteMinBundles=e0,EliteMaxBundles=e1};
        }
        private static void BuildCatalog()
        {
            const string path="Assets/_Game/Resources/Tier01/HarvestArtCatalog.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<Tier1HarvestArtCatalog>(path);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<Tier1HarvestArtCatalog>();AssetDatabase.CreateAsset(catalog,path);}
            catalog.PineTrunk=AssetDatabase.LoadAssetAtPath<GameObject>(Actors+"PineTrunk.prefab");
            catalog.MassivePineTrunk=AssetDatabase.LoadAssetAtPath<GameObject>(Actors+"PineTrunkMassive.prefab");
            catalog.Rabbit=AssetDatabase.LoadAssetAtPath<GameObject>(Actors+"Rabbit.prefab");
            catalog.FishingPoint=AssetDatabase.LoadAssetAtPath<GameObject>(Actors+"FishingPoint.prefab");
            catalog.TreeDrops=new[]{Drop(PrototypeItems.Branch,"Assets/_Game/Resources/Region01/PineBranch.prefab",2,5,4,10),Drop(PrototypeItems.Log,Drops+"PineLog.prefab",2,5,4,10),Drop(PrototypeItems.PineNuts,Drops+"PineNuts.prefab",2,4,4,6)};
            catalog.HuntingDrops=new[]{Drop(PrototypeItems.RabbitPelt,"Assets/_Game/Resources/Region01/RabbitPelt.prefab",2,5,4,10),Drop(PrototypeItems.RawRabbitMeat,Drops+"RawRabbitMeat.prefab",2,5,4,10),Drop(PrototypeItems.Bone,Drops+"Bone.prefab",2,5,4,10),Drop(PrototypeItems.AnimalFat,Drops+"AnimalFat.prefab",1,3,1,6)};
            catalog.FishVisuals=new[]{Drop(PrototypeItems.RainbowTrout,Drops+"RainbowTrout.prefab"),Drop(PrototypeItems.LargemouthBass,Drops+"LargemouthBass.prefab"),Drop(PrototypeItems.Bluegill,Drops+"Bluegill.prefab"),Drop(PrototypeItems.YellowPerch,Drops+"YellowPerch.prefab")};
            string[] fish={PrototypeItems.RainbowTrout.Id,PrototypeItems.LargemouthBass.Id,PrototypeItems.Bluegill.Id,PrototypeItems.YellowPerch.Id};
            int[,] pairs={{0,2},{1,3},{0,1},{2,3},{0,3},{1,2}};
            catalog.RegionFishPairs=new Tier1FishPair[6];for(int i=0;i<6;i++)catalog.RegionFishPairs[i]=new Tier1FishPair{RegionNumber=i+1,FishA=fish[pairs[i,0]],FishB=fish[pairs[i,1]]};
            EditorUtility.SetDirty(catalog);
        }

        [MenuItem("The Trail Walker/Art/Capture Tier 1 Harvest Preview")]
        public static void CapturePreview()
        {
            Capture(true,"Docs/Art/Tier01/interactables-unity-v1.png");Capture(false,"Docs/Art/Tier01/harvest-drops-unity-v1.png");
            CaptureRabbitPoses();
        }
        private static void CaptureRabbitPoses()
        {
            var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<2;i++)
                {
                    var source=AssetDatabase.LoadAssetAtPath<GameObject>(Actors+"Rabbit.prefab");
                    var view=UnityEngine.Object.Instantiate(source);view.transform.position=new Vector3(i==0?-.65f:.65f,-.5f,0);
                    view.transform.rotation=Quaternion.Euler(0,145,0);view.transform.localScale=Vector3.one*1.2f;
                    if(i==1)
                    {
                        var rig=view.GetComponent<Tier1RabbitRig>();rig.HeadPivot.localPosition+=Vector3.down*.13f;
                        rig.HeadPivot.localRotation=Quaternion.Euler(55,0,0);
                    }
                    preview.AddSingleGO(view);
                }
                preview.camera.orthographic=true;preview.camera.orthographicSize=.9f;
                preview.camera.transform.position=new Vector3(0,.9f,-4);preview.camera.transform.LookAt(new Vector3(0,-.03f,0));
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.19f,.22f,.25f);
                preview.lights[0].intensity=1.2f;preview.lights[0].transform.rotation=Quaternion.Euler(40,30,0);preview.lights[1].intensity=.6f;
                preview.ambientColor=new Color(.4f,.4f,.4f);preview.BeginStaticPreview(new Rect(0,0,1400,800));preview.Render(true);
                Texture2D image=preview.EndStaticPreview();
                try{File.WriteAllBytes("Docs/Art/Tier01/rabbit-pose-study-faceted.png",image.EncodeToPNG());}
                finally{UnityEngine.Object.DestroyImmediate(image);}
            }
            finally{preview.Cleanup();}
        }
        private static void CaptureRabbitAnatomy()
        {
            var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<3;i++)
                {
                    var view=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Actors+"Rabbit.prefab"));
                    view.transform.position=new Vector3((i-1)*.9f,0,0);view.transform.rotation=Quaternion.Euler(0,new[]{180,90,145}[i],0);preview.AddSingleGO(view);
                }
                preview.camera.orthographic=true;preview.camera.orthographicSize=.56f;preview.camera.transform.position=new Vector3(0,.72f,-4);preview.camera.transform.LookAt(new Vector3(0,.3f,0));
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.19f,.22f,.25f);preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=20;
                preview.lights[0].intensity=1.3f;preview.lights[0].transform.rotation=Quaternion.Euler(40,30,0);preview.lights[1].intensity=.6f;preview.ambientColor=new Color(.4f,.4f,.4f);
                preview.BeginStaticPreview(new Rect(0,0,1800,650));preview.Render(true);var image=preview.EndStaticPreview();
                try{File.WriteAllBytes("Docs/Art/Tier01/rabbit-anatomy-faceted.png",image.EncodeToPNG());}
                finally{UnityEngine.Object.DestroyImmediate(image);}
            }
            finally{preview.Cleanup();}
        }
        private static void Capture(bool actors,string destination)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                string[] paths=actors?new[]{Actors+"PineTrunk.prefab",Actors+"Rabbit.prefab",Actors+"FishingPoint.prefab"}:new[]{"Assets/_Game/Resources/Region01/PineBranch.prefab",Drops+"PineLog.prefab",Drops+"PineNuts.prefab","Assets/_Game/Resources/Region01/RabbitPelt.prefab",Drops+"RawRabbitMeat.prefab",Drops+"Bone.prefab",Drops+"AnimalFat.prefab",Drops+"RainbowTrout.prefab",Drops+"LargemouthBass.prefab",Drops+"Bluegill.prefab",Drops+"YellowPerch.prefab"};
                for(int i=0;i<paths.Length;i++)
                {
                    var source=AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);if(source==null)throw new InvalidOperationException(paths[i]);
                    GameObject view=UnityEngine.Object.Instantiate(source);
                    if(actors)
                    {
                        view.transform.position=new Vector3((i-1)*2.5f,-1.25f,0);
                        view.transform.localScale=Vector3.one*(i==1 ? 2.2f : i==2 ? 0.75f : 1);
                        if(i==1)view.transform.rotation=Quaternion.Euler(0,145,0);
                    }
                    else
                    {
                        Renderer[] renderers=view.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(Renderer r in renderers)bounds.Encapsulate(r.bounds);
                        view.transform.localScale=Vector3.one*(1.15f/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));
                        view.transform.position=new Vector3((i%4-1.5f)*1.85f,(1-i/4)*1.7f,0);
                    }
                    preview.AddSingleGO(view);
                }
                preview.camera.orthographic=true;preview.camera.orthographicSize=actors?2.5f:3.1f;
                preview.camera.transform.position=new Vector3(0,actors?3.2f:3.0f,-10);preview.camera.transform.LookAt(Vector3.zero);
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.19f,.22f,.25f);
                preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=40;
                preview.lights[0].intensity=1.2f;preview.lights[0].transform.rotation=Quaternion.Euler(40,30,0);preview.lights[1].intensity=.6f;preview.ambientColor=new Color(.4f,.4f,.4f);
                preview.BeginStaticPreview(new Rect(0,0,1600,1000));preview.Render(true);Texture2D image=preview.EndStaticPreview();
                try{Directory.CreateDirectory(Path.GetDirectoryName(destination));File.WriteAllBytes(destination,image.EncodeToPNG());}
                finally{UnityEngine.Object.DestroyImmediate(image);}
            }
            finally{preview.Cleanup();}
        }
    }

    internal sealed class Tier01HarvestImport : AssetPostprocessor
    {
        private const string Source="Assets/_Game/Art/Harvest/Tier01/";
        private void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;
            m.addCollider=false;m.importNormals=ModelImporterNormals.Calculate;
            // Flat normals; the faceted rabbit (revision 4) dropped its 65-degree smoothing but keeps the explicit FromAngle source.
            if(Path.GetFileName(assetPath).StartsWith("Rabbit",StringComparison.Ordinal))m.normalSmoothingSource=ModelImporterNormalSmoothingSource.FromAngle;
            m.normalSmoothingAngle=0;
            m.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        private void OnPreprocessTexture()
        {
            if(assetPath!=Source+"Tier01Palette.png"&&assetPath!=Source+"RabbitPalette.png")return;
            var t=(TextureImporter)assetImporter;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;
            t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
