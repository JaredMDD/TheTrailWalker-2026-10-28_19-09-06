using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class InventoryContainerAssets
    {
        private const string Target="Assets/_Game/Resources/UI/Inventory/";
        private const string Meshes="Assets/_Game/Art/UI/Inventory/";
        private static readonly string[] Names={"BaseRags","Tier01Backpack","Tier01ShirtPocket","Tier01PantsBelt","Tier01ToolLoop","BaseRagsOpening","Tier01BackpackOpening"};
        static InventoryContainerAssets(){EditorApplication.update+=Ensure;}
        private static void Ensure()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=Ensure;if(!File.Exists(Target+"BaseRags.asset"))Build();
        }
        [MenuItem("The Trail Walker/Art/Rebuild Inventory Container Designs")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build outside Play mode.");
            Directory.CreateDirectory(Target);Directory.CreateDirectory(Meshes);AssetDatabase.Refresh();
            for(int i=0;i<Names.Length;i++)
            {
                string path=Target+Names[i]+".asset";
                var skin=AssetDatabase.LoadAssetAtPath<InventoryContainerSkin>(path);
                if(skin==null){skin=ScriptableObject.CreateInstance<InventoryContainerSkin>();AssetDatabase.CreateAsset(skin,path);}
                skin.Kind=new[]{InventoryContainerKind.Rags,InventoryContainerKind.Backpack,InventoryContainerKind.Pocket,InventoryContainerKind.Belt,InventoryContainerKind.ToolLoop,InventoryContainerKind.Opening,InventoryContainerKind.Opening}[i];
                bool rags=i==0||i==5;
                skin.EquipmentDefinitionId=rags||i==3||i==4?"":i==2?"item.hemp_shirt":"item.starter_backpack"; // belt and loops are universal
                skin.Cloth=rags?new Color(.37f,.31f,.23f):new Color(.43f,.40f,.28f);
                skin.Shade=new Color(.20f,.16f,.11f);skin.Binding=new Color(.25f,.17f,.105f);
                skin.Thread=new Color(.72f,.63f,.44f);skin.Patch=rags?new Color(.47f,.39f,.27f):new Color(.39f,.28f,.17f);
                EditorUtility.SetDirty(skin);
                var root=new GameObject(Names[i],typeof(RectTransform),typeof(CanvasRenderer),typeof(InventoryContainerGraphic));
                try
                {
                    var rect=root.GetComponent<RectTransform>();
                    rect.sizeDelta=i==0?new Vector2(600,400):i==1?new Vector2(800,400):i==2?new Vector2(140,240):i==3?new Vector2(600,32):i==4?new Vector2(200,340):new Vector2(600,32);
                    var graphic=root.GetComponent<InventoryContainerGraphic>();graphic.Bind(skin);
                    SaveMesh(graphic,Names[i]);PrefabUtility.SaveAsPrefabAsset(root,Target+Names[i]+".prefab");
                    if(i==4){graphic.Bind(skin,true);SaveMesh(graphic,"Tier01ToolLoopFront");PrefabUtility.SaveAsPrefabAsset(root,Target+"Tier01ToolLoopFront.prefab");}
                }
                finally{UnityEngine.Object.DestroyImmediate(root);}
            }
            string materialPath=Meshes+"InventoryClothUI.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null){material=new Material(Shader.Find("UI/Default"));AssetDatabase.CreateAsset(material,materialPath);}
            AssetDatabase.SaveAssets();Debug.Log("Inventory container skins, eight native Canvas meshes and prefabs built.");
        }
        private static void SaveMesh(InventoryContainerGraphic graphic,string name)
        {
            string path=Meshes+name+".asset";Mesh fresh=graphic.BakeMesh();Mesh existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing==null)AssetDatabase.CreateAsset(fresh,path);
            else{EditorUtility.CopySerialized(fresh,existing);EditorUtility.SetDirty(existing);UnityEngine.Object.DestroyImmediate(fresh);}
        }
        private static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=Vector2.zero;rect.pivot=Vector2.zero;rect.anchoredPosition=new Vector2(x,y);rect.sizeDelta=new Vector2(w,h);return rect;
        }
        private static void Skin(Transform parent,string name,float x,float y,float w,float h,bool front=false)
        {
            var graphic=Rect(parent,name,x,y,w,h).gameObject.AddComponent<InventoryContainerGraphic>();
            graphic.Bind(AssetDatabase.LoadAssetAtPath<InventoryContainerSkin>(Target+name+".asset"),front);
        }
        private static void Label(Transform parent,string value,float x,float y,float w,float h,int size=24)
        {
            var text=Rect(parent,"Label",x,y,w,h).gameObject.AddComponent<UnityEngine.UI.Text>();
            text.text=value;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;
            text.color=new Color(.9f,.83f,.67f);text.raycastTarget=false;text.alignment=TextAnchor.MiddleLeft;
        }
        private static void BowStudy(Transform parent,float x,float y)
        {
            // Fit reference only: no tier-one bow recipe/item is introduced.
            Vector2 last=new Vector2(0,-115);
            for(int i=1;i<=12;i++)
            {
                float a=-Mathf.PI/2+i*Mathf.PI/12;Vector2 point=new Vector2(Mathf.Cos(a)*65,Mathf.Sin(a)*115);
                Stroke(parent,new Vector2(x,y)+last,new Vector2(x,y)+point,8,new Color(.52f,.34f,.17f));last=point;
            }
            Stroke(parent,new Vector2(x,y-115),new Vector2(x,y+115),2,new Color(.78f,.69f,.49f));
        }
        private static void Stroke(Transform parent,Vector2 a,Vector2 b,float width,Color color)
        {
            var rect=Rect(parent,"Bow fit reference",a.x,a.y,width,Vector2.Distance(a,b));
            rect.pivot=new Vector2(.5f,0);rect.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(b.x-a.x,b.y-a.y)*Mathf.Rad2Deg);
            var image=rect.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;
        }
        private static void Icon(Transform parent,string path,float x,float y,float w,float h,List<UnityEngine.Object> temporary)
        {
            var prefab=Resources.Load<GameObject>(path);if(prefab==null)throw new InvalidOperationException(path);
            GameObject view=null,cameraObject=null;RenderTexture target=null;RenderTexture previous=RenderTexture.active;
            try
            {
                view=UnityEngine.Object.Instantiate(prefab);view.transform.position=new Vector3(100,-1000,0);
                var renderers=view.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
                foreach(var t in view.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
                float aspect=bounds.size.x/Mathf.Max(.001f,bounds.size.y);int width=aspect>=1?300:Mathf.Max(16,Mathf.RoundToInt(300*aspect));int height=aspect>=1?Mathf.Max(16,Mathf.RoundToInt(300/aspect)):300;
                cameraObject=new GameObject("Transparent item study camera");var camera=cameraObject.AddComponent<Camera>();
                camera.orthographic=true;camera.orthographicSize=bounds.extents.y*1.04f;camera.aspect=(float)width/height;
                camera.transform.position=bounds.center-Vector3.forward*(bounds.extents.z+5);camera.cullingMask=1<<30;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.enabled=false;
                target=new RenderTexture(width,height,16,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;camera.Render();
                RenderTexture.active=target;var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.ReadPixels(new UnityEngine.Rect(0,0,width,height),0,0);texture.Apply();
                temporary.Add(texture);Sprite sprite=Sprite.Create(texture,new UnityEngine.Rect(0,0,width,height),new Vector2(.5f,.5f));temporary.Add(sprite);
                var image=Rect(parent,"Item fit study",x,y,w,h).gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            }
            finally
            {
                RenderTexture.active=previous;if(view!=null)UnityEngine.Object.DestroyImmediate(view);if(cameraObject!=null)UnityEngine.Object.DestroyImmediate(cameraObject);
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
            }
        }
        [MenuItem("The Trail Walker/Art/Capture Inventory Container Preview")]
        public static void CapturePreview()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Capture outside Play mode.");
            Scene previous=SceneManager.GetActiveScene();Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            RenderTexture target=null;Texture2D output=null;RenderTexture old=RenderTexture.active;var temporary=new List<UnityEngine.Object>();
            try
            {
                SceneManager.SetActiveScene(scene);
                var cam=new GameObject("Container art camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6.3f;
                cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.12f,.13f,.13f);
                cam.nearClipPlane=.01f;cam.farClipPlane=30;cam.cullingMask=1<<31;
                var canvas=new GameObject("Container art study",typeof(RectTransform),typeof(Canvas)).GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cam;
                canvas.GetComponent<RectTransform>().sizeDelta=new Vector2(1920,1260);canvas.transform.localScale=Vector3.one*.01f;
                Label(canvas.transform,"INVENTORY / CLOTH & TOOL BELT",80,1160,1750,60,36);
                Label(canvas.transform,"BASE RAGS / 6 x 4",80,1060,650,60,28);Label(canvas.transform,"TIER 1 HEMP BACKPACK / 8 x 4",840,1060,1000,60,28);
                Skin(canvas.transform,"BaseRags",80,650,600,400);Skin(canvas.transform,"Tier01Backpack",840,650,800,400);
                Icon(canvas.transform,"Region01/Apple",260,790,110,110,temporary);Icon(canvas.transform,"Tier01/Crafted/HempCloth",1150,800,180,130,temporary);
                Label(canvas.transform,"Mended scraps, frayed edges",80,595,680,45,23);Label(canvas.transform,"Hemp lining, leather binding and stitched seams",840,595,1000,45,23);
                Label(canvas.transform,"TIER 1 SHIRT / TWO POCKETS",80,500,650,60,26);
                Skin(canvas.transform,"Tier01ShirtPocket",80,200,140,240);Skin(canvas.transform,"Tier01ShirtPocket",270,200,140,240);
                Icon(canvas.transform,"Region05/RedBerries",287,245,106,135,temporary);
                Label(canvas.transform,"No shirt: no pockets",80,135,470,45,23);
                Label(canvas.transform,"PANTS BELT / UNIVERSAL TOOL RETAINERS",600,500,1250,60,26);
                Skin(canvas.transform,"Tier01PantsBelt",600,438,1180,32);
                for(int i=0;i<4;i++)Skin(canvas.transform,"Tier01ToolLoop",620+i*285,130,240,310);
                Icon(canvas.transform,"Tier01/Crafted/PineAxeNormal",645,150,190,275,temporary);
                Icon(canvas.transform,"Tier01/Crafted/PineSlingshotNormal",930,150,190,275,temporary);
                BowStudy(canvas.transform,1230,275);
                Icon(canvas.transform,"Tier01/Crafted/PineRodNormal",1500,150,190,275,temporary);
                for(int i=0;i<4;i++)Skin(canvas.transform,"Tier01ToolLoop",620+i*285,130,240,310,true);
                string[] captions={"AXE","SLINGSHOT","BOW FIT STUDY","FISHING ROD"};
                for(int i=0;i<4;i++)Label(canvas.transform,captions[i],620+i*285,80,275,40,20);
                Label(canvas.transform,"Four tool shapes shown for fit; tier 1 pants still provide exactly TWO hooks. No pants: no belt/hooks.",600,25,1230,45,21);
                foreach(Transform t in canvas.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                Canvas.ForceUpdateCanvases();target=new RenderTexture(1920,1260,24);target.Create();
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;output=new Texture2D(1920,1260,TextureFormat.RGBA32,false);output.ReadPixels(new UnityEngine.Rect(0,0,1920,1260),0,0);output.Apply();
                Directory.CreateDirectory("Docs/Art/UI");File.WriteAllBytes("Docs/Art/UI/inventory-containers-unity-v1.png",output.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=old;if(output!=null)UnityEngine.Object.DestroyImmediate(output);
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                EditorSceneManager.CloseScene(scene,true);foreach(var item in temporary)UnityEngine.Object.DestroyImmediate(item);
                if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);
            }
        }
    }
}
