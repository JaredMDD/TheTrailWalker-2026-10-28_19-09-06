using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class CampPropAssets
    {
        private const string Source="Assets/_Game/Art/Environment/CampProps/";
        private const string Target="Assets/_Game/Resources/Environment/CampProps/";
        private static readonly string[] Names={"BurnedWoodCampfire","PineLogSeat","BirchLogSeat","MesquiteLogSeat","OakLogSeat"};
        static CampPropAssets(){EditorApplication.update+=Ensure;}
        private static void Ensure()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=Ensure;if(!File.Exists(Target+"CampPropArtCatalog.asset"))Build();
        }
        [MenuItem("The Trail Walker/Art/Rebuild Campfire and Log Seat Meshes")]
        public static void Build()
        {
            Region01ResourceAssets.BuildRegion(Source,Target,"CampPropsPalette","CampProps",Names);
            var material=AssetDatabase.LoadAssetAtPath<Material>(Target+"CampPropsPalette.mat");material.enableInstancing=true;
            material.DisableKeyword("_EMISSION");material.SetColor("_EmissionColor",Color.black);EditorUtility.SetDirty(material);
            foreach(string name in Names)
            {
                string path=Target+name+".prefab";GameObject root=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Anchor(root.transform,"GroundAnchor",Vector3.zero);
                    if(name==Names[0])Anchor(root.transform,"FireCentre",new Vector3(0,.12f,0));
                    else{Anchor(root.transform,"SeatAnchor",new Vector3(0,.43f,0));Anchor(root.transform,"FacingAnchor",new Vector3(0,.43f,.5f));}
                    PrefabUtility.SaveAsPrefabAsset(root,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            const string catalogPath=Target+"CampPropArtCatalog.asset";var catalog=AssetDatabase.LoadAssetAtPath<CampPropArtCatalog>(catalogPath);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<CampPropArtCatalog>();AssetDatabase.CreateAsset(catalog,catalogPath);}
            catalog.StaticBurnedWoodCampfire=AssetDatabase.LoadAssetAtPath<GameObject>(Target+Names[0]+".prefab");catalog.LogSeats=new CampLogSeatArt[4];
            string[] woods={"Pine","Birch","Mesquite","Oak"};
            for(int i=0;i<4;i++)catalog.LogSeats[i]=new CampLogSeatArt{Wood=woods[i],WoodTier=i+1,FirstRegion=i*6+1,LastRegion=(i+1)*6,Prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Target+Names[i+1]+".prefab")};
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();Debug.Log("Camp prop art: static partially burned wood and Pine/Birch/Mesquite/Oak log seats. No live scene or gameplay changes.");
        }
        private static void Anchor(Transform root,string name,Vector3 position)
        {
            var anchor=new GameObject(name).transform;anchor.SetParent(root,false);anchor.localPosition=position;
        }
        [MenuItem("The Trail Walker/Art/Capture Campfire and Log Seat Meshes")]
        public static void CapturePreview()
        {
            Directory.CreateDirectory("Docs/Art/Environment/CampProps");CaptureFire();CaptureSeats();CaptureSeatedFit();
        }
        private static GameObject View(PreviewRenderUtility preview,string name,Vector3 position,Quaternion rotation)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Target+name+".prefab");if(prefab==null)throw new InvalidOperationException(name);
            var view=UnityEngine.Object.Instantiate(prefab);view.transform.SetPositionAndRotation(position,rotation);preview.AddSingleGO(view);return view;
        }
        private static void Camera(PreviewRenderUtility preview,float size,Vector3 position,Vector3 target)
        {
            preview.camera.orthographic=true;preview.camera.orthographicSize=size;preview.camera.transform.position=position;preview.camera.transform.LookAt(target);
            preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.20f,.23f,.25f);preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=40;
            preview.lights[0].intensity=1.3f;preview.lights[0].transform.rotation=Quaternion.Euler(45,30,0);preview.lights[1].intensity=.65f;preview.ambientColor=new Color(.45f,.45f,.45f);
        }
        private static void Save(PreviewRenderUtility preview,int width,int height,string name)
        {
            preview.BeginStaticPreview(new Rect(0,0,width,height));preview.Render(true);var image=preview.EndStaticPreview();
            try{File.WriteAllBytes("Docs/Art/Environment/CampProps/"+name+".png",image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
        }
        private static void CaptureFire()
        {
            var preview=new PreviewRenderUtility();
            try{View(preview,Names[0],Vector3.zero,Quaternion.identity);Camera(preview,.77f,new Vector3(1.3f,1.5f,-2),new Vector3(0,.12f,0));Save(preview,1100,800,"burned-wood-campfire");}
            finally{preview.Cleanup();}
        }
        private static void CaptureSeats()
        {
            var preview=new PreviewRenderUtility();
            try
            {
                for(int i=0;i<4;i++)View(preview,Names[i+1],new Vector3((i-1.5f)*2.05f,0,0),Quaternion.Euler(0,28,0));
                Camera(preview,1.2f,new Vector3(0,2,-5),new Vector3(0,.22f,0));Save(preview,2000,550,"wood-tier-log-seats");
            }
            finally{preview.Cleanup();}
        }
        private static void CaptureSeatedFit()
        {
            var preview=new PreviewRenderUtility();
            try
            {
                View(preview,"PineLogSeat",new Vector3(-1,0,0),Quaternion.identity);
                View(preview,"BirchLogSeat",new Vector3(1,0,0),Quaternion.Euler(0,80,0));
                string avatarRoot="Assets/_Game/Resources/Characters/CampCommon/Avatars/";
                string[] avatars={"MaleTier01HempRabbit","FemaleTier01HempRabbit"};
                for(int i=0;i<2;i++)
                {
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(avatarRoot+avatars[i]+".prefab");if(prefab==null)throw new InvalidOperationException("Build common NPC art first.");
                    var avatar=UnityEngine.Object.Instantiate(prefab);avatar.transform.SetPositionAndRotation(new Vector3(i==0?-1:1,0,0),Quaternion.Euler(0,i==0?180:260,0));preview.AddSingleGO(avatar);
                }
                Camera(preview,1.05f,new Vector3(0,1.6f,-6),new Vector3(0,.72f,0));Save(preview,1450,800,"npc-seat-fit");
            }
            finally{preview.Cleanup();}
        }
    }
    internal sealed class CampPropImport : AssetPostprocessor
    {
        private const string Source="Assets/_Game/Art/Environment/CampProps/";
        private void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;m.addCollider=false;m.importNormals=ModelImporterNormals.Calculate;m.normalSmoothingAngle=0;m.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        private void OnPreprocessTexture()
        {
            if(assetPath!=Source+"CampPropsPalette.png")return;
            var t=(TextureImporter)assetImporter;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
