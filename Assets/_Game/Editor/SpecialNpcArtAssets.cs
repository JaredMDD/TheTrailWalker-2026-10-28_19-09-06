using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class SpecialNpcArtAssets
    {
        private const string Source="Assets/_Game/Art/Characters/CampSpecial/";
        private const string Target="Assets/_Game/Resources/Characters/CampSpecial/";
        [Serializable] private sealed class PartData{public string Name;public int Triangles;}
        [Serializable] private sealed class Data{public PartData[] Parts;}
        static SpecialNpcArtAssets(){EditorApplication.update+=Ensure;}
        private static void Ensure()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=Ensure;if(!File.Exists(Target+"SpecialNpcArtCatalog.asset"))Build();
        }
        [MenuItem("The Trail Walker/Art/Rebuild Mission Lady and Ferryman Meshes")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build outside Play mode.");
            var data=JsonUtility.FromJson<Data>(File.ReadAllText(Source+"MeshManifest.json"));var names=new string[data.Parts.Length];for(int i=0;i<names.Length;i++)names[i]=data.Parts[i].Name;
            Region01ResourceAssets.BuildRegion(Source,Target+"Parts/","CampSpecialPalette","CampSpecial",names);
            Directory.CreateDirectory(Target+"Materials");Directory.CreateDirectory(Target+"Actors");Directory.CreateDirectory(Target+"Props");AssetDatabase.Refresh();
            Material("Clothing",Color.white);Material("Eyes",Color.white);
            Material("LadySkin",new Color(.84f,.64f,.46f));Material("FerrymanSkin",new Color(.73f,.50f,.33f));
            Material("LadyHair",new Color(.18f,.13f,.11f));Material("FerrymanHair",new Color(.48f,.47f,.43f));
            for(int pose=0;pose<4;pose++)
            {
                GameObject root=Lady(pose);try{PrefabUtility.SaveAsPrefabAsset(root,Target+"Actors/"+LadyName(pose)+".prefab");}finally{UnityEngine.Object.DestroyImmediate(root);}
            }
            GameObject ferryman=Ferryman();try{PrefabUtility.SaveAsPrefabAsset(ferryman,Target+"Actors/FerrymanStatic.prefab");}finally{UnityEngine.Object.DestroyImmediate(ferryman);}
            SaveProp("RecipeParchmentVisual");SaveProp("FerrymanWoodenOar");
            const string catalogPath=Target+"SpecialNpcArtCatalog.asset";var catalog=AssetDatabase.LoadAssetAtPath<SpecialNpcArtCatalog>(catalogPath);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<SpecialNpcArtCatalog>();AssetDatabase.CreateAsset(catalog,catalogPath);}
            catalog.MissionLadySideways=LoadActor(LadyName(0));catalog.MissionLadyFacing=LoadActor(LadyName(1));catalog.MissionLadyRecipeHeld=LoadActor(LadyName(2));catalog.MissionLadyOffering=LoadActor(LadyName(3));catalog.FerrymanStatic=LoadActor("FerrymanStatic");
            catalog.RecipeDisplayProp=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Props/RecipeParchmentVisual.prefab");catalog.WoodenOar=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Props/FerrymanWoodenOar.prefab");
            catalog.PoseIntent="ART ONLY. Mission lady begins sideways. Future interaction can rotate FacingPivot toward the player for completion/new-mission selection, then transition recipe-held to extended-arm offering pose. RecipeGripAnchor follows the wrist. No Animator, animation clip, trigger, reward/learning transaction or live scene replacement is implemented. Ferryman is static. Parchment is a display prop, never a recipe inventory stack.";
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();Debug.Log("Special NPC art: four mission-lady static pose studies, one static ferryman and two props. No gameplay/animation logic.");
        }
        private static string LadyName(int pose)=>new[]{"MissionLadySideways","MissionLadyFacing","MissionLadyRecipeHeld","MissionLadyOffering"}[pose];
        private static GameObject LoadActor(string name)=>AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Actors/"+name+".prefab");
        private static Material Material(string name,Color tint)
        {
            string path=Target+"Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"CampSpecialPalette.png"));material.SetColor("_BaseColor",tint);material.SetFloat("_Smoothness",.10f);material.enableInstancing=true;EditorUtility.SetDirty(material);return material;
        }
        private static Material Mat(string name)=>AssetDatabase.LoadAssetAtPath<Material>(Target+"Materials/"+name+".mat");
        private static Transform Pivot(Transform parent,string name,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }
        private static GameObject Part(Transform parent,string name,string material="Clothing")
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Source+name+".obj");if(source==null)throw new InvalidOperationException(name);
            var view=UnityEngine.Object.Instantiate(source,parent,false);view.name=name;
            foreach(var renderer in view.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=Mat(material);
            return view;
        }
        private static void SaveProp(string name)
        {
            var root=new GameObject(name);try{Part(root.transform,name);PrefabUtility.SaveAsPrefabAsset(root,Target+"Props/"+name+".prefab");}finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        private static SpecialNpcArtRig Base(string name,string role,float hipHeight)
        {
            var root=new GameObject(name);var rig=root.AddComponent<SpecialNpcArtRig>();rig.Role=role;rig.GroundAnchor=Pivot(root.transform,"GroundAnchor",Vector3.zero);
            rig.FacingPivot=Pivot(root.transform,"FacingPivot",Vector3.zero);rig.HipsPivot=Pivot(rig.FacingPivot,"HipsPivot",new Vector3(0,hipHeight,0));
            rig.TorsoPivot=Pivot(rig.HipsPivot,"TorsoPivot",new Vector3(0,TorsoUp,0));return rig;
        }
        // Faceted revision 2 (November 2, 2026): the shared skeleton from Tools/Art/faceted_people.py.
        private const float HipsY=.88f,TorsoUp=.06f,HeadUp=.49f,UpperArm=.29f,Forearm=.25f,Thigh=.38f,Shin=.40f;
        private static GameObject Lady(int pose)
        {
            var rig=Base(LadyName(pose),"Mission lady",HipsY);rig.PoseStudy=LadyName(pose);
            Part(rig.HipsPivot,"LadyLongDress");Part(rig.TorsoPivot,"LadyFittedBodice");Part(rig.TorsoPivot,"LadyOpenOvercoat");Part(rig.TorsoPivot,"SpecialNeck","LadySkin");
            rig.HeadPivot=Pivot(rig.TorsoPivot,"HeadPivot",new Vector3(0,HeadUp,0));Part(rig.HeadPivot,"LadyHead","LadySkin");Part(rig.HeadPivot,"LadyFaceDetails","Eyes");Part(rig.HeadPivot,"LadyHair","LadyHair");
            rig.HatAnchor=Pivot(rig.HeadPivot,"HatAnchor",Vector3.zero);Part(rig.HatAnchor,"LadyBroadHat");
            Legs(rig,true);
            for(int side=-1;side<=1;side+=2)
            {
                var shoulder=Pivot(rig.TorsoPivot,side<0?"LeftShoulder":"RightShoulder",new Vector3(side*.188f,.40f,0));Part(shoulder,"LadySleeve");
                var elbow=Pivot(shoulder,side<0?"LeftElbow":"RightElbow",new Vector3(0,-UpperArm,0));Part(elbow,"LadyForeSleeve");Part(elbow,"LadyIvoryCuff");
                var wrist=Pivot(elbow,side<0?"LeftWrist":"RightWrist",new Vector3(0,-Forearm,0));var hand=Part(wrist,"SpecialHand","LadySkin");if(side>0)hand.transform.localScale=new Vector3(-1,1,1);
                if(side<0){rig.LeftShoulder=shoulder;rig.LeftElbow=elbow;rig.LeftWrist=wrist;}else{rig.RightShoulder=shoulder;rig.RightElbow=elbow;rig.RightWrist=wrist;}
            }
            rig.FacingPivot.localRotation=Quaternion.Euler(0,pose==0?65:0,0);rig.HeadPivot.localRotation=Quaternion.Euler(0,pose==0?-12:0,0);
            // Static authoring poses. No clip, timeline or runtime interpolation is created.
            Aim(rig.LeftShoulder,rig.LeftElbow,new Vector3(-.13f,-.06f,.14f),new Vector3(-.7f,-1,0),UpperArm,Forearm);
            Vector3 right=pose==3?new Vector3(.16f,.26f,.50f):pose==2?new Vector3(.08f,.20f,.23f):new Vector3(.24f,-.08f,.10f);
            Aim(rig.RightShoulder,rig.RightElbow,right,new Vector3(.7f,-1,.2f),UpperArm,Forearm);
            if(pose>=2)
            {
                // Palm and document face upward/toward the player; their anchors follow the posed forearm.
                Quaternion desired=Quaternion.Euler(-78,0,0);rig.RightWrist.localRotation=Quaternion.Inverse(rig.RightElbow.rotation)*rig.TorsoPivot.rotation*desired;
            }
            rig.RecipeGripAnchor=Pivot(rig.RightWrist,"RecipeGripAnchor",new Vector3(0,-.085f,.023f));rig.RecipeGripAnchor.localRotation=Quaternion.Euler(18,0,0);
            rig.RecipeVisual=Part(rig.RecipeGripAnchor,"RecipeParchmentVisual");rig.RecipeVisual.SetActive(pose>=2);
            return rig.gameObject;
        }
        private static GameObject Ferryman()
        {
            var rig=Base("FerrymanStatic","Static ferryman",HipsY);rig.PoseStudy="Standing with wooden oar";
            Part(rig.HipsPivot,"FerrymanHips");Part(rig.HipsPivot,"FerrymanRopeBelt");Part(rig.TorsoPivot,"FerrymanShirt");Part(rig.TorsoPivot,"FerrymanWaistcoat");Part(rig.TorsoPivot,"SpecialNeck","FerrymanSkin");
            rig.HeadPivot=Pivot(rig.TorsoPivot,"HeadPivot",new Vector3(0,HeadUp,0));Part(rig.HeadPivot,"FerrymanHead","FerrymanSkin");Part(rig.HeadPivot,"FerrymanFaceDetails","Eyes");Part(rig.HeadPivot,"FerrymanHair","FerrymanHair");Part(rig.HeadPivot,"FerrymanShortBeard","FerrymanHair");
            rig.HatAnchor=Pivot(rig.HeadPivot,"HatAnchor",Vector3.zero);Part(rig.HatAnchor,"FerrymanCap");Legs(rig,false);
            for(int side=-1;side<=1;side+=2)
            {
                var shoulder=Pivot(rig.TorsoPivot,side<0?"LeftShoulder":"RightShoulder",new Vector3(side*.217f,.42f,0));Part(shoulder,"FerrymanUpperArm");Part(shoulder,"FerrymanRolledCuff");
                var elbow=Pivot(shoulder,side<0?"LeftElbow":"RightElbow",new Vector3(0,-UpperArm,0));Part(elbow,"FerrymanForearm","FerrymanSkin");
                var wrist=Pivot(elbow,side<0?"LeftWrist":"RightWrist",new Vector3(0,-Forearm,0));if(side<0)Part(wrist,"SpecialHand","FerrymanSkin");
                if(side<0){rig.LeftShoulder=shoulder;rig.LeftElbow=elbow;rig.LeftWrist=wrist;}else{rig.RightShoulder=shoulder;rig.RightElbow=elbow;rig.RightWrist=wrist;}
            }
            Aim(rig.RightShoulder,rig.RightElbow,new Vector3(.40f,.12f,.14f),new Vector3(.8f,-1,-.2f),UpperArm,Forearm);
            Aim(rig.LeftShoulder,rig.LeftElbow,new Vector3(-.25f,-.08f,.10f),new Vector3(-.8f,-1,0),UpperArm,Forearm);
            rig.OarAnchor=Pivot(rig.FacingPivot,"OarAnchor",new Vector3(.43f,0,.17f));Part(rig.OarAnchor,"FerrymanWoodenOar");
            var grip=Pivot(rig.OarAnchor,"StaticOarGripAnchor",new Vector3(0,1.055f,0));Part(grip,"FerrymanOarGrip","FerrymanSkin");
            return rig.gameObject;
        }
        private static void Legs(SpecialNpcArtRig rig,bool lady)
        {
            float thigh=Thigh,shin=Shin;
            for(int side=-1;side<=1;side+=2)
            {
                var hip=Pivot(rig.HipsPivot,side<0?"LeftHip":"RightHip",new Vector3(side*(lady ? .10f : .105f),0,lady&&side<0 ? .025f : 0));Part(hip,lady?"LadyHiddenThigh":"FerrymanThigh");
                var knee=Pivot(hip,"Knee",new Vector3(0,-thigh,0));Part(knee,lady?"LadyHiddenShin":"FerrymanShin");
                var ankle=Pivot(knee,"Ankle",new Vector3(0,-shin,0));Part(ankle,"SpecialBoot");
            }
        }
        private static void Aim(Transform shoulder,Transform elbow,Vector3 target,Vector3 pole,float upperLength,float lowerLength)
        {
            Vector3 origin=shoulder.localPosition,delta=target-origin;float distance=Mathf.Clamp(delta.magnitude,.04f,upperLength+lowerLength-.001f);
            Vector3 direction=delta.normalized,across=Vector3.ProjectOnPlane(pole,direction).normalized;
            float angle=Mathf.Acos(Mathf.Clamp((upperLength*upperLength+distance*distance-lowerLength*lowerLength)/(2*upperLength*distance),-1,1));
            Vector3 joint=origin+upperLength*(Mathf.Cos(angle)*direction+Mathf.Sin(angle)*across);
            Quaternion upper=Quaternion.FromToRotation(Vector3.down,(joint-origin).normalized),lower=Quaternion.FromToRotation(Vector3.down,(origin+direction*distance-joint).normalized);
            shoulder.localRotation=upper;elbow.localRotation=Quaternion.Inverse(upper)*lower;
        }
        [MenuItem("The Trail Walker/Art/Capture Mission Lady and Ferryman Meshes")]
        public static void CapturePreview()
        {
            Directory.CreateDirectory("Docs/Art/Characters/CampSpecial");
            Capture(new[]{LadyName(0),LadyName(1),LadyName(2),LadyName(3)},"mission-lady-pose-studies.png",false);
            Capture(new[]{"FerrymanStatic","FerrymanStatic","FerrymanStatic"},"ferryman-static-study.png",true);
            CaptureHandoffDetail();
        }
        private static void CaptureHandoffDetail()
        {
            var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<2;i++)
                {
                    var view=UnityEngine.Object.Instantiate(LoadActor(LadyName(i+2)));view.transform.position=new Vector3(i==0?.9f:-.9f,0,0);view.transform.rotation=Quaternion.Euler(0,i==0?45:75,0);preview.AddSingleGO(view);
                }
                preview.camera.orthographic=true;preview.camera.orthographicSize=.85f;preview.camera.transform.position=new Vector3(0,1.75f,6);preview.camera.transform.LookAt(new Vector3(0,1.38f,0));
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.20f,.23f,.25f);preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=30;
                preview.lights[0].intensity=1.5f;preview.lights[0].transform.rotation=Quaternion.Euler(30,200,0);preview.lights[1].intensity=.8f;preview.ambientColor=new Color(.5f,.5f,.5f);
                preview.BeginStaticPreview(new Rect(0,0,1600,900));preview.Render(true);var image=preview.EndStaticPreview();
                try{File.WriteAllBytes("Docs/Art/Characters/CampSpecial/mission-lady-handoff-detail.png",image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
            }
            finally{preview.Cleanup();}
        }
        private static void Capture(string[] names,string file,bool ferry)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<names.Length;i++)
                {
                    var view=UnityEngine.Object.Instantiate(LoadActor(names[i]));if(view==null)throw new InvalidOperationException(names[i]);
                    view.transform.position=new Vector3(((names.Length-1)*.5f-i)*1.45f,0,0);view.transform.rotation=Quaternion.Euler(0,ferry?new[]{0,80,160}[i]:-12,0);preview.AddSingleGO(view);
                }
                preview.camera.orthographic=true;preview.camera.orthographicSize=1.3f;preview.camera.transform.position=new Vector3(0,1.65f,7);preview.camera.transform.LookAt(new Vector3(0,1.03f,0));
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.20f,.23f,.25f);preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=40;
                preview.lights[0].intensity=1.5f;preview.lights[0].transform.rotation=Quaternion.Euler(30,200,0);preview.lights[1].intensity=.8f;preview.ambientColor=new Color(.5f,.5f,.5f);
                preview.BeginStaticPreview(new Rect(0,0,ferry?1650:2100,950));preview.Render(true);Texture2D image=preview.EndStaticPreview();
                try{File.WriteAllBytes("Docs/Art/Characters/CampSpecial/"+file,image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
            }
            finally{preview.Cleanup();}
        }
    }
    internal sealed class SpecialNpcMeshImport : AssetPostprocessor
    {
        private const string Source="Assets/_Game/Art/Characters/CampSpecial/";
        private void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;m.addCollider=false;
            m.importNormals=ModelImporterNormals.Calculate;m.normalSmoothingSource=ModelImporterNormalSmoothingSource.FromAngle;m.normalSmoothingAngle=0; // faceted: flat normalsm.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        private void OnPreprocessTexture()
        {
            if(assetPath!=Source+"CampSpecialPalette.png")return;
            var t=(TextureImporter)assetImporter;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
