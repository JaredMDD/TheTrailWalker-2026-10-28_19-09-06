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
            foreach(var gear in InventoryStorageGear.All)BuildGear(gear);
            AssetDatabase.SaveAssets();Debug.Log("Inventory container skins, native Canvas meshes and prefabs built ("+Names.Length+" base, "+InventoryStorageGear.All.Length+" storage gear items).");
        }
        // Table-driven skins: a shirt gets a Pocket, a backpack a Backpack and an Opening. Merchant gear samples its display prefab.
        private static void BuildGear(InventoryStorageGear.Entry gear)
        {
            Color cloth=gear.Cloth,patch=gear.Patch,binding=gear.Binding;
            if(gear.Sampled)SampleMerchantPalette(gear.ItemId,out cloth,out binding,out patch);
            string baseName=InventoryStorageGear.PascalName(gear.ItemId);
            if(gear.Backpack){BuildGearSkin(baseName+"Backpack",InventoryContainerKind.Backpack,gear.ItemId,new Vector2(800,400),cloth,patch,binding,gear.Thread);BuildGearSkin(baseName+"Opening",InventoryContainerKind.Opening,gear.ItemId,new Vector2(600,32),cloth,patch,binding,gear.Thread);}
            else BuildGearSkin(baseName+"Pocket",InventoryContainerKind.Pocket,gear.ItemId,new Vector2(140,240),cloth,patch,binding,gear.Thread);
        }
        private static void BuildGearSkin(string name,InventoryContainerKind kind,string itemId,Vector2 size,Color cloth,Color patch,Color binding,Color thread)
        {
            string path=Target+name+".asset";
            var skin=AssetDatabase.LoadAssetAtPath<InventoryContainerSkin>(path);
            if(skin==null){skin=ScriptableObject.CreateInstance<InventoryContainerSkin>();AssetDatabase.CreateAsset(skin,path);}
            skin.Kind=kind;skin.EquipmentDefinitionId=itemId;skin.Cloth=cloth;skin.Shade=new Color(cloth.r*.45f,cloth.g*.45f,cloth.b*.45f,1f);
            skin.Binding=binding;skin.Thread=thread;skin.Patch=patch;EditorUtility.SetDirty(skin);
            var root=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(InventoryContainerGraphic));
            try
            {
                root.GetComponent<RectTransform>().sizeDelta=size;var graphic=root.GetComponent<InventoryContainerGraphic>();graphic.Bind(skin);
                SaveMesh(graphic,name);PrefabUtility.SaveAsPrefabAsset(root,Target+name+".prefab");
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        // Area-weighted palette sampling of "special.tN.shirt|pack" -> Resources/BalloonGoods/Items/SpecialT0N{Shirt|Pack}Item.
        // Cloth = most-used colour, Binding (outer rim) = darkest of the top three, Patch (top band) = most saturated of the top three;
        // if both pick one colour, Patch is the next most saturated distinct colour among the top five.
        public static string LastSampleReport="";
        private static void SampleMerchantPalette(string itemId,out Color cloth,out Color binding,out Color patch)
        {
            string[] parts=itemId.Split('.');string path="BalloonGoods/Items/SpecialT0"+parts[1].Substring(1)+(parts[2]=="pack"?"Pack":"Shirt")+"Item";
            var prefab=Resources.Load<GameObject>(path);if(prefab==null)throw new InvalidOperationException("Merchant display prefab missing: "+path);
            var weights=new Dictionary<Color32,double>();
            foreach(var renderer in prefab.GetComponentsInChildren<MeshRenderer>())
            {
                var filter=renderer.GetComponent<MeshFilter>();var mesh=filter!=null?filter.sharedMesh:null;if(mesh==null)continue;
                var texture=renderer.sharedMaterial.GetTexture("_BaseMap") as Texture2D;if(texture==null)texture=renderer.sharedMaterial.mainTexture as Texture2D;
                if(texture==null)throw new InvalidOperationException("No palette texture on "+path);
                Color32[] pixels=ReadPixels(texture);Matrix4x4 matrix=renderer.transform.localToWorldMatrix;
                Vector3[] vertices=mesh.vertices;Vector2[] uv=mesh.uv;
                for(int sub=0;sub<mesh.subMeshCount;sub++)
                {
                    int[] tri=mesh.GetTriangles(sub);
                    for(int t=0;t+2<tri.Length;t+=3)
                    {
                        Vector3 a=matrix.MultiplyPoint3x4(vertices[tri[t]]),b=matrix.MultiplyPoint3x4(vertices[tri[t+1]]),c=matrix.MultiplyPoint3x4(vertices[tri[t+2]]);
                        double area=Vector3.Cross(b-a,c-a).magnitude*.5;if(area<=0)continue;
                        Vector2 centre=(uv[tri[t]]+uv[tri[t+1]]+uv[tri[t+2]])/3f;
                        int x=Mathf.Clamp(Mathf.FloorToInt((centre.x-Mathf.Floor(centre.x))*texture.width),0,texture.width-1),y=Mathf.Clamp(Mathf.FloorToInt((centre.y-Mathf.Floor(centre.y))*texture.height),0,texture.height-1);
                        Color32 colour=pixels[y*texture.width+x];colour.a=255;weights.TryGetValue(colour,out double sum);weights[colour]=sum+area;
                    }
                }
            }
            var top=new List<KeyValuePair<Color32,double>>(weights);top.Sort((l,r)=>r.Value.CompareTo(l.Value));if(top.Count==0)throw new InvalidOperationException("No triangles sampled on "+path);
            if(top.Count>5)top.RemoveRange(5,top.Count-5);
            Color Of(Color32 c)=>new Color(c.r/255f,c.g/255f,c.b/255f,1f);
            cloth=Of(top[0].Key);binding=cloth;patch=cloth;float darkest=float.MaxValue;
            float Sat(Color c){Color.RGBToHSV(c,out _,out float sat,out _);return sat;}
            for(int i=0;i<Mathf.Min(3,top.Count);i++){Color c=Of(top[i].Key);float luma=c.r*.299f+c.g*.587f+c.b*.114f;if(luma<darkest){darkest=luma;binding=c;}}
            float best=-1;
            for(int i=0;i<Mathf.Min(3,top.Count);i++){Color c=Of(top[i].Key);if(Sat(c)>best){best=Sat(c);patch=c;}}
            if(patch==binding)
            {
                best=-1;Color pick=binding;
                for(int i=0;i<top.Count;i++){Color c=Of(top[i].Key);if(c!=binding&&Sat(c)>best){best=Sat(c);pick=c;}}
                patch=pick;
            }
            string line=itemId+": cloth "+(Color32)cloth+" binding "+(Color32)binding+" patch "+(Color32)patch;
            LastSampleReport+=line+"\n";Debug.Log("Sampled "+line);
        }
        private static Color32[] ReadPixels(Texture2D texture)
        {
            var target=RenderTexture.GetTemporary(texture.width,texture.height,0,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;
            var oldFilter=texture.filterMode;texture.filterMode=FilterMode.Point;
            try
            {
                Graphics.Blit(texture,target);RenderTexture.active=target;var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
                copy.ReadPixels(new UnityEngine.Rect(0,0,texture.width,texture.height),0,0);var pixels=copy.GetPixels32();UnityEngine.Object.DestroyImmediate(copy);return pixels;
            }
            finally{texture.filterMode=oldFilter;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);}
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
        [MenuItem("The Trail Walker/Art/Capture Inventory Storage Gear Preview")]
        public static void CaptureGearPreview()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Capture outside Play mode.");
            Scene previous=SceneManager.GetActiveScene();Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            RenderTexture target=null;Texture2D output=null;RenderTexture old=RenderTexture.active;const int W=1920,H=3270;
            try
            {
                SceneManager.SetActiveScene(scene);
                var cam=new GameObject("Container gear camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=H/200f;
                cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.12f,.13f,.13f);
                cam.nearClipPlane=.01f;cam.farClipPlane=30;cam.cullingMask=1<<31;
                var canvas=new GameObject("Container gear study",typeof(RectTransform),typeof(Canvas)).GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cam;
                canvas.GetComponent<RectTransform>().sizeDelta=new Vector2(W,H);canvas.transform.localScale=Vector3.one*.01f;
                Label(canvas.transform,"INVENTORY BAGS / STARTER PACK AND ONE OR TWO PACKS PER TIER",60,3190,1800,60,34);
                string[] bags={"Tier01Backpack","MarshPackBackpack","TaigaFramePackBackpack","CanyonPackBackpack","HighlandFramePackBackpack","SavannaPackBackpack","CoastalPackBackpack","SpecialT2PackBackpack","SpecialT4PackBackpack"};
                string[] bagLabels={"T1 STARTER PACK","T2 MARSH PACK","T2 TAIGA FRAME PACK","T3 CANYON PACK","T3 HIGHLAND FRAME PACK","T4 SAVANNA PACK","T4 COASTAL PACK","T2 MERCHANT PACK (SAMPLED)","T4 MERCHANT PACK (SAMPLED)"};
                for(int i=0;i<bags.Length;i++)
                {
                    float x=60+(i%2)*930,y=2730-(i/2)*470;Skin(canvas.transform,bags[i],x,y,800,400);Label(canvas.transform,bagLabels[i],x,y+400,800,40,24);
                }
                Label(canvas.transform,"SHIRT POCKETS",60,430,1800,50,28);
                string[] pockets={"Tier01ShirtPocket","FlaxWorkShirtPocket","TaigaWaxedShirtPocket","PrairieFeatherShirtPocket","HighlandShirtPocket","SavannaShirtPocket","CoastalShirtPocket","SpecialT1ShirtPocket","SpecialT2ShirtPocket","SpecialT3ShirtPocket","SpecialT4ShirtPocket"};
                string[] pocketLabels={"T1 HEMP","FLAX","WAXED","FEATHER","HIGHLAND","SAVANNA","COASTAL","MERCH T1","MERCH T2","MERCH T3","MERCH T4"};
                for(int i=0;i<pockets.Length;i++){float x=60+i*165;Skin(canvas.transform,pockets[i],x,150,140,240);Label(canvas.transform,pocketLabels[i],x,100,170,40,18);}
                foreach(Transform t in canvas.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                Canvas.ForceUpdateCanvases();target=new RenderTexture(W,H,24);target.Create();
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;output=new Texture2D(W,H,TextureFormat.RGBA32,false);output.ReadPixels(new UnityEngine.Rect(0,0,W,H),0,0);output.Apply();
                Directory.CreateDirectory("Docs/Art/UI");File.WriteAllBytes("Docs/Art/UI/inventory-storage-gear-unity-v1.png",output.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=old;if(output!=null)UnityEngine.Object.DestroyImmediate(output);
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                EditorSceneManager.CloseScene(scene,true);
                if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);
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
