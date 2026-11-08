using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class CampNpcAvatarAssets
    {
        private const string Source="Assets/_Game/Art/Characters/CampCommon/";
        private const string Target="Assets/_Game/Resources/Characters/CampCommon/";
        // Faceted revision 4 (user-approved style, November 2, 2026): shared skeleton from Tools/Art/faceted_people.py.
        public static readonly string[] HairStyles={"Crop","Sweep","Bob","Tied"};
        private static readonly string[] Parts=BuildPartList();
        private static string[] BuildPartList()
        {
            var list=new List<string>{"Beard","Neck","UpperArm","Forearm","Hand","Shin","Boot","BareFoot","Belt","FurCap","FeatherTrim","ClothHood","BackpackBody","RagBackpack","PackFrame","PackRoll"};
            foreach(string body in new[]{"Male","Female"})
            {
                foreach(string piece in new[]{"Head","FaceDetails","Brows","Torso","ShirtPockets","RagPatches","PackStraps","AdvancedTrim","Hips","Thigh"})list.Add(body+piece);
                foreach(string style in HairStyles)list.Add(body+"Hair"+style);
            }
            return list.ToArray();
        }
        // Skeleton (metres): seated hips pivot, torso and head offsets, limb lengths and hip spacing.
        private const float HipsY=.50f,TorsoUp=.06f,HeadUp=.49f,UpperArmLength=.29f,ForearmLength=.25f,ThighLength=.38f,ShinLength=.40f,HipX=.10f;
        private static Vector3 ShoulderPosition(bool female,int side)=>new Vector3(side*(female?.188f:.205f),female?.400f:.415f,0);
        private static readonly string[] Styles={"Tier00Rags","Tier01HempRabbit","Tier02FlaxBeaver","Tier03CottonFeather","Tier04CottonBoar"};
        private static readonly Color[] Tones={new Color(.95f,.80f,.66f),new Color(.84f,.64f,.46f),new Color(.73f,.50f,.33f),new Color(.59f,.38f,.25f),new Color(.43f,.26f,.18f),new Color(.27f,.17f,.125f)};
        private static readonly Color[] Hairs={new Color(.14f,.11f,.09f),new Color(.31f,.20f,.12f),new Color(.58f,.43f,.22f),new Color(.49f,.48f,.43f)};
        static CampNpcAvatarAssets(){EditorApplication.update+=Ensure;}
        private static void Ensure()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=Ensure;if(!File.Exists(Target+"CampNpcArtCatalog.asset"))Build();
        }
        [MenuItem("The Trail Walker/Art/Rebuild Common Camp NPC Meshes")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build outside Play mode.");
            Region01ResourceAssets.BuildRegion(Source,Target+"Parts/","CampNpcPalette","CampNpcParts",Parts);
            Directory.CreateDirectory(Target+"Materials");Directory.CreateDirectory(Target+"Avatars");Directory.CreateDirectory(Target+"Backpacks");AssetDatabase.Refresh();
            var skins=new Material[Tones.Length];var hairs=new Material[Hairs.Length];var clothing=new Material[Styles.Length];
            for(int i=0;i<skins.Length;i++)skins[i]=Material("Skin"+(i+1).ToString("00"),"CampNpcPalette",Tones[i]);
            for(int i=0;i<hairs.Length;i++)hairs[i]=Material("Hair"+(i+1).ToString("00"),"CampNpcPalette",Hairs[i]);
            for(int i=0;i<clothing.Length;i++)clothing[i]=Material(Styles[i],Styles[i]+"Palette",Color.white);
            Material("FaceFeatures","CampNpcPalette",Color.white);
            var studies=new CampNpcOutfitArt[5];
            for(int tier=0;tier<5;tier++)
            {
                var pack=CreatePack(tier,clothing[tier]);
                try{PrefabUtility.SaveAsPrefabAsset(pack,Target+"Backpacks/"+Styles[tier]+"Backpack.prefab");}finally{UnityEngine.Object.DestroyImmediate(pack);}
                studies[tier]=new CampNpcOutfitArt{Name=Styles[tier],VisualTier=tier,ClothingMaterial=clothing[tier],Backpack=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Backpacks/"+Styles[tier]+"Backpack.prefab")};
                for(int sex=0;sex<2;sex++)
                {
                    bool female=sex==1;string name=(female?"Female":"Male")+Styles[tier];
                    GameObject root=CreateAvatar(name,female,tier,tier,skins[(tier+sex)%skins.Length],hairs[(tier+sex)%hairs.Length],clothing[tier],clothing[tier]);
                    try
                    {
                        string path=Target+"Avatars/"+name+".prefab";PrefabUtility.SaveAsPrefabAsset(root,path);
                        if(female)studies[tier].Female=AssetDatabase.LoadAssetAtPath<GameObject>(path);else studies[tier].Male=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    }
                    finally{UnityEngine.Object.DestroyImmediate(root);}
                }
            }
            const string catalogPath=Target+"CampNpcArtCatalog.asset";var catalog=AssetDatabase.LoadAssetAtPath<CampNpcArtCatalog>(catalogPath);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<CampNpcArtCatalog>();AssetDatabase.CreateAsset(catalog,catalogPath);}
            catalog.MaleBase=studies[0].Male;catalog.FemaleBase=studies[0].Female;catalog.SkinTones=skins;catalog.HairColors=hairs;catalog.OutfitStudies=studies;
            catalog.FutureSelectionRule="ART ONLY. Future implementation: tier-zero rags always available; any learned hat/boots/shirt/pants recipe discovers that matching full clothing set. Select a whole eligible set, never mixed clothing pieces. Select backpack separately from its own learned recipe IDs, independent of outfit tier. Owning gear, visiting regions, quality, mastery, or knowing a backpack recipe does not discover a clothing set. Tiers 2-4 here are visual studies, not authored gameplay recipe bindings. No selection, animation, spawning or persistence logic is implemented.";
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();Debug.Log("Common NPC revision 4 (faceted): "+Parts.Length+" mesh parts, ten seated avatar presets with four built-in hair styles and a beard, six skin tones.");
        }
        public static void ReimportNaturalCharacterMeshes()
        {
            foreach(string name in Parts)AssetDatabase.ImportAsset(Source+name+".obj",ImportAssetOptions.ForceUpdate);
            Build();CapturePreview();
        }
        private static Material Material(string name,string palette,Color tint)
        {
            string path=Target+"Materials/"+name+".mat";var result=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(result==null){result=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(result,path);}
            result.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+palette+".png"));result.SetColor("_BaseColor",tint);result.SetFloat("_Smoothness",.06f);result.enableInstancing=true;EditorUtility.SetDirty(result);return result;
        }
        private static Transform Pivot(Transform parent,string name,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }
        private static void Part(Transform parent,string name,Material material,List<Renderer> surfaces=null)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Source+name+".obj");if(source==null)throw new InvalidOperationException(name);
            GameObject part=UnityEngine.Object.Instantiate(source,parent,false);part.name=name;
            foreach(var renderer in part.GetComponentsInChildren<Renderer>()){renderer.sharedMaterial=material;surfaces?.Add(renderer);}
        }
        private static GameObject CreatePack(int tier,Material material,List<Renderer> surfaces=null)
        {
            var root=new GameObject(Styles[tier]+"Backpack");Part(root.transform,tier==0?"RagBackpack":"BackpackBody",material,surfaces);
            if(tier==2||tier==3)Part(root.transform,"PackFrame",material,surfaces);
            if(tier>=3)Part(root.transform,"PackRoll",material,surfaces);
            if(tier==4)root.transform.localScale=new Vector3(1,1.1f,1);
            return root;
        }
        private static GameObject CreateAvatar(string name,bool female,int tier,int packTier,Material skin,Material hair,Material cloth,Material packMaterial)
        {
            var root=new GameObject(name);var rig=root.AddComponent<CampNpcAvatarRig>();rig.BodyVariant=female?"Female":"Male";rig.OutfitStudy=Styles[tier];
            var skins=new List<Renderer>();var hairs=new List<Renderer>();var clothes=new List<Renderer>();var packs=new List<Renderer>();
            rig.GroundAnchor=Pivot(root.transform,"GroundAnchor",Vector3.zero);rig.SeatAnchor=Pivot(root.transform,"SeatAnchor",new Vector3(0,.43f,0));
            string body=female?"Female":"Male";
            rig.HipsPivot=Pivot(root.transform,"HipsPivot",new Vector3(0,HipsY,0));Part(rig.HipsPivot,body+"Hips",cloth,clothes);
            rig.TorsoPivot=Pivot(rig.HipsPivot,"TorsoPivot",new Vector3(0,TorsoUp,0));Part(rig.TorsoPivot,body+"Torso",cloth,clothes);Part(rig.TorsoPivot,"Neck",skin,skins);
            rig.HeadPivot=Pivot(rig.TorsoPivot,"HeadPivot",new Vector3(0,HeadUp,0));Part(rig.HeadPivot,body+"Head",skin,skins);
            // Eyes keep their neutral colour across all six skin tints; brows follow the hair colour.
            Part(rig.HeadPivot,body+"FaceDetails",AssetDatabase.LoadAssetAtPath<Material>(Target+"Materials/FaceFeatures.mat"));
            Part(rig.HeadPivot,body+"Brows",hair,hairs);
            // Every hair style and the beard are built in; the camp roll shows one style and, for men, a beard or none.
            var hairAnchor=Pivot(rig.HeadPivot,"HairAnchor",Vector3.zero);var styles=new GameObject[HairStyles.Length];
            for(int i=0;i<HairStyles.Length;i++){Part(hairAnchor,body+"Hair"+HairStyles[i],hair,hairs);styles[i]=hairAnchor.GetChild(i).gameObject;styles[i].SetActive(i==(female?3:1));}
            rig.HairStyles=styles;
            if(!female){Part(rig.HeadPivot,"Beard",hair,hairs);rig.Beard=rig.HeadPivot.Find("Beard").gameObject;rig.Beard.SetActive(false);}
            rig.HatAnchor=Pivot(rig.HeadPivot,"HatAnchor",Vector3.zero);
            if(tier>0)Part(rig.HatAnchor,tier==4?"ClothHood":"FurCap",cloth,clothes);
            if(tier==3)Part(rig.HatAnchor,"FeatherTrim",cloth,clothes);
            Part(rig.TorsoPivot,body+(tier==0?"RagPatches":"ShirtPockets"),cloth,clothes);
            if(tier>0)Part(rig.HipsPivot,"Belt",cloth,clothes);
            if(tier>=2)Part(rig.TorsoPivot,body+"AdvancedTrim",cloth,clothes);
            // The pack's front face is its anchor plane, set just behind the back of the shirt.
            rig.BackpackAnchor=Pivot(rig.TorsoPivot,"BackpackAnchor",new Vector3(0,.24f,female?-.112f:-.118f));
            var pack=CreatePack(packTier,packMaterial,packs);pack.transform.SetParent(rig.BackpackAnchor,false);Part(rig.TorsoPivot,body+"PackStraps",packMaterial,packs);
            for(int side=-1;side<=1;side+=2)
            {
                string label=side<0?"Left":"Right";
                Transform shoulder=Pivot(rig.TorsoPivot,label+"Shoulder",ShoulderPosition(female,side));shoulder.localRotation=Quaternion.Euler(-35,0,side*5);
                Part(shoulder,"UpperArm",cloth,clothes);Transform elbow=Pivot(shoulder,label+"Elbow",new Vector3(0,-UpperArmLength,0));elbow.localRotation=Quaternion.Euler(-35,0,0);Part(elbow,"Forearm",skin,skins);
                Transform wrist=Pivot(elbow,label+"Wrist",new Vector3(0,-ForearmLength,0));Part(wrist,"Hand",skin,skins);
                if(side>0)wrist.Find("Hand").localScale=new Vector3(-1,1,1); // the hand has its thumb on +X: mirror for the right side
                Transform hip=Pivot(rig.HipsPivot,label+"Hip",new Vector3(side*HipX,0,0));hip.localRotation=Quaternion.Euler(-90,0,0);Part(hip,body+"Thigh",cloth,clothes);
                Transform knee=Pivot(hip,label+"Knee",new Vector3(0,-ThighLength,0));knee.localRotation=Quaternion.Euler(90,0,0);Part(knee,"Shin",cloth,clothes);
                Transform ankle=Pivot(knee,label+"Ankle",new Vector3(0,-ShinLength,0));Part(ankle,tier==0?"BareFoot":"Boot",tier==0?skin:cloth,tier==0?skins:clothes);
                if(side<0){rig.LeftShoulder=shoulder;rig.LeftElbow=elbow;rig.LeftWrist=wrist;rig.LeftHip=hip;rig.LeftKnee=knee;rig.LeftAnkle=ankle;}
                else{rig.RightShoulder=shoulder;rig.RightElbow=elbow;rig.RightWrist=wrist;rig.RightHip=hip;rig.RightKnee=knee;rig.RightAnkle=ankle;}
            }
            PoseArms(rig,female);
            rig.SkinSurfaces=skins.ToArray();rig.HairSurfaces=hairs.ToArray();rig.ClothingSurfaces=clothes.ToArray();rig.BackpackSurfaces=packs.ToArray();
            return root;
        }
        // Seated arm poses (user request, October 30, 2026): women rest their hands on their knees, men cross their arms
        // at the chest. A two-joint solve aims shoulder and elbow at a wrist target in TorsoPivot space (hips pivot is
        // 0.06 m below it; knees sit 0.36 m forward of the hips), with a pole that keeps the elbows back and out.
        private static void PoseArms(CampNpcAvatarRig rig,bool female)
        {
            for(int side=-1;side<=1;side+=2)
            {
                Transform shoulder=side<0?rig.LeftShoulder:rig.RightShoulder,elbow=side<0?rig.LeftElbow:rig.RightElbow;
                Vector3 wrist,pole;
                if(female){wrist=new Vector3(side*.12f,.04f,.27f);pole=new Vector3(side*.4f,-.3f,-1f);}
                else
                {
                    // Left forearm lies under the right one; each hand tucks against the opposite upper arm.
                    bool under=side<0;
                    wrist=new Vector3(-side*.060f,under?.25f:.295f,under?.205f:.245f);pole=new Vector3(side*.6f,-1f,.35f);
                }
                Aim(shoulder,elbow,wrist,pole);
                // Crossed arms: bend the wrists so the fingers wrap back around the opposite upper arm.
                Transform hand=side<0?rig.LeftWrist:rig.RightWrist;if(!female)hand.localRotation=Quaternion.Euler(60,0,0);
            }
        }
        private static void Aim(Transform shoulder,Transform elbow,Vector3 target,Vector3 pole)
        {
            Vector3 origin=shoulder.localPosition,toward=target-origin;
            float d=Mathf.Clamp(toward.magnitude,.05f,UpperArmLength+ForearmLength-.001f);
            Vector3 u=toward.normalized,v=Vector3.ProjectOnPlane(pole,u).normalized;
            float a=Mathf.Acos(Mathf.Clamp((UpperArmLength*UpperArmLength+d*d-ForearmLength*ForearmLength)/(2*UpperArmLength*d),-1,1));
            Vector3 elbowPoint=origin+UpperArmLength*(Mathf.Cos(a)*u+Mathf.Sin(a)*v);
            Quaternion upper=Quaternion.FromToRotation(Vector3.down,(elbowPoint-origin).normalized);
            Quaternion lower=Quaternion.FromToRotation(Vector3.down,(origin+u*d-elbowPoint).normalized);
            shoulder.localRotation=upper;elbow.localRotation=Quaternion.Inverse(upper)*lower;
        }
        [MenuItem("The Trail Walker/Art/Capture Common Camp NPC Meshes")]
        public static void CapturePreview()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<CampNpcArtCatalog>(Target+"CampNpcArtCatalog.asset");if(catalog==null)throw new InvalidOperationException("Build art first.");
            Directory.CreateDirectory("Docs/Art/Characters/CampCommon");
            Capture(catalog,false);Capture(catalog,true);CaptureBacks(catalog);CaptureSeatStudy(catalog);CaptureFaceStudy(catalog);
        }
        // Review sheets show the runtime variety: one hair style and (men) a beard or none.
        private static void Look(CampNpcAvatarRig rig,int style,bool beard)
        {
            for(int i=0;i<rig.HairStyles.Length;i++)rig.HairStyles[i].SetActive(i==style);
            if(rig.Beard!=null)rig.Beard.SetActive(beard);
        }
        private static void Camera(PreviewRenderUtility preview,float size,Vector3 position,Vector3 target)
        {
            preview.camera.orthographic=true;preview.camera.orthographicSize=size;preview.camera.transform.position=position;preview.camera.transform.LookAt(target);
            preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.19f,.22f,.24f);preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=60;
            preview.lights[0].intensity=1.4f;preview.lights[0].transform.rotation=Quaternion.Euler(35,210,0);preview.lights[1].intensity=.6f;preview.ambientColor=new Color(.45f,.45f,.45f);
        }
        private static void CaptureFaceStudy(CampNpcArtCatalog catalog)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<4;i++)
                {
                    var avatar=CreateAvatar("Face anatomy study",i%2==1,1,1,catalog.SkinTones[i+1],catalog.HairColors[i],catalog.OutfitStudies[1].ClothingMaterial,catalog.OutfitStudies[1].ClothingMaterial);
                    avatar.GetComponent<CampNpcAvatarRig>().HatAnchor.gameObject.SetActive(false);Look(avatar.GetComponent<CampNpcAvatarRig>(),i,i==0);
                    avatar.transform.position=new Vector3((1.5f-i)*.3f,0,0);avatar.transform.rotation=Quaternion.Euler(0,i<2?0:65,0);preview.AddSingleGO(avatar);
                }
                Camera(preview,.20f,new Vector3(0,1.21f,5),new Vector3(0,1.21f,0));Save(preview,1800,600,"natural-face-study-v3.png");
            }
            finally{preview.Cleanup();}
        }
        private static void Save(PreviewRenderUtility preview,int width,int height,string file)
        {
            preview.BeginStaticPreview(new Rect(0,0,width,height));preview.Render(true);Texture2D image=preview.EndStaticPreview();
            try{File.WriteAllBytes("Docs/Art/Characters/CampCommon/"+file,image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
        }
        private static void Capture(CampNpcArtCatalog catalog,bool outfits)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                int columns=outfits?5:6;
                for(int row=0;row<2;row++)for(int col=0;col<columns;col++)
                {
                    int tier=outfits?col:1;int packTier=outfits?(col+2)%5:col%5;
                    var avatar=CreateAvatar("Art study",row==1,tier,packTier,catalog.SkinTones[outfits?(col+row)%6:col],catalog.HairColors[(col+row)%4],catalog.OutfitStudies[tier].ClothingMaterial,catalog.OutfitStudies[packTier].ClothingMaterial);
                    if(!outfits)avatar.GetComponent<CampNpcAvatarRig>().HatAnchor.gameObject.SetActive(false);
                    Look(avatar.GetComponent<CampNpcAvatarRig>(),(col+row*2)%HairStyles.Length,col%2==0);
                    avatar.transform.position=new Vector3(((columns-1)*.5f-col)*1.4f,row==0?1.6f:0,0);avatar.transform.rotation=Quaternion.Euler(0,-18,0);preview.AddSingleGO(avatar);
                }
                Camera(preview,1.85f,new Vector3(0,2.4f,9),new Vector3(0,1.48f,0));
                Save(preview,outfits?1800:2400,1000,outfits?"outfit-studies.png":"body-and-skin-studies.png");
            }
            finally{preview.Cleanup();}
        }
        private static void CaptureBacks(CampNpcArtCatalog catalog)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<5;i++)
                {
                    var avatar=CreateAvatar("Independent backpack study",i%2==1,1,i,catalog.SkinTones[i],catalog.HairColors[i%4],catalog.OutfitStudies[1].ClothingMaterial,catalog.OutfitStudies[i].ClothingMaterial);
                    avatar.transform.position=new Vector3((2-i)*1.3f,0,0);avatar.transform.rotation=Quaternion.Euler(0,165,0);preview.AddSingleGO(avatar);
                }
                Camera(preview,1.1f,new Vector3(0,1.8f,8),new Vector3(0,.72f,0));Save(preview,2000,650,"backpack-studies.png");
            }
            finally{preview.Cleanup();}
        }
        private static void CaptureSeatStudy(CampNpcArtCatalog catalog)
        {
            var preview=new PreviewRenderUtility();var logMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));logMaterial.SetColor("_BaseColor",new Color(.35f,.23f,.13f));
            try
            {
                for(int i=0;i<3;i++)
                {
                    var avatar=CreateAvatar("Seated joint study",i==1,i==0?0:1,1,catalog.SkinTones[i+1],catalog.HairColors[i],catalog.OutfitStudies[i==0?0:1].ClothingMaterial,catalog.OutfitStudies[1].ClothingMaterial);
                    Vector3 position=new Vector3((1-i)*1.7f,0,0);avatar.transform.position=position;avatar.transform.rotation=Quaternion.Euler(0,i==0?-20:i==1?80:160,0);
                    // Static art pose to show joint clearance, not an animation clip or idle behavior.
                    if(i==2){var rig=avatar.GetComponent<CampNpcAvatarRig>();rig.TorsoPivot.localRotation=Quaternion.Euler(3,2,0);rig.HeadPivot.localRotation=Quaternion.Euler(-2,-7,0);}
                    preview.AddSingleGO(avatar);
                    var seat=GameObject.CreatePrimitive(PrimitiveType.Cylinder);UnityEngine.Object.DestroyImmediate(seat.GetComponent<Collider>());
                    seat.transform.position=position+Vector3.up*.215f;seat.transform.rotation=avatar.transform.rotation*Quaternion.Euler(0,0,90);seat.transform.localScale=new Vector3(.42f,.52f,.42f);seat.GetComponent<Renderer>().sharedMaterial=logMaterial;preview.AddSingleGO(seat);
                }
                Camera(preview,1.15f,new Vector3(0,1.8f,8),new Vector3(0,.73f,0));Save(preview,1600,750,"seated-joint-study.png");
            }
            finally{preview.Cleanup();UnityEngine.Object.DestroyImmediate(logMaterial);}
        }
    }
    internal sealed class CampNpcMeshImport : AssetPostprocessor
    {
        private const string Source="Assets/_Game/Art/Characters/CampCommon/";
        private void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;m.addCollider=false;
            m.importNormals=ModelImporterNormals.Calculate;m.normalSmoothingSource=ModelImporterNormalSmoothingSource.FromAngle;m.normalSmoothingAngle=0; // faceted: flat normalsm.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        private void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal)||!assetPath.EndsWith("Palette.png",StringComparison.Ordinal))return;
            var t=(TextureImporter)assetImporter;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
