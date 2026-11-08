using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    // Art authoring only: no scene injection, gameplay components or startup selector.
    public static class PlayerDefaultAvatarAssets
    {
        const string Source="Assets/_Game/Art/Characters/PlayerDefault/";
        const string Target="Assets/_Game/Art/Characters/PlayerDefault/Unity/";
        const string Docs="Docs/Art/Characters/PlayerDefault/";
        static readonly string[] Skins={"F2CCAA","D6A477","BA8055","966140","6E432E","452B20"};
        static readonly string[] Hairs={"241C17","513522","8D4E30","B99A60","858176"};
        static readonly string[] Styles={"Crop","Sweep","Bob","Tied"};
        [Serializable] class Manifest { public PartInfo[] parts; public WearableInfo[] wearables; }
        [Serializable] class PartInfo { public string name; public string[] materials; }
        // Fitted wearables (November 5, 2026): item model ID -> part name (<Body>Wear<part>), from the generator.
        [Serializable] public class WearableInfo { public string itemId; public string part; }
        static Manifest manifest;
        // Wearable colours: the crafted item models' palette (generate_player_avatar_meshes.WEAR_COLORS).
        static readonly (string name, string hex)[] WearMaterials = {("HempShirt","BDA978"),("HempShirtDark","806444"),("HempPants","8A784D"),
            ("HempPantsDark","584A36"),("PeltLeather","A7845A"),("PeltLeatherDark","774D2B"),("PeltFur","E2D0A2"),("PeltSole","4F311E"),("Brass","B8913A")};
        public static WearableInfo[] Wearables()
        {
            var m=JsonUtility.FromJson<Manifest>(File.ReadAllText(Source+"PlayerAvatarManifest.json"));
            return m.wearables??new WearableInfo[0];
        }

        [MenuItem("The Trail Walker/Art/Build Default Player Avatars")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build player art outside Play mode.");
            Directory.CreateDirectory(Target+"Materials");Directory.CreateDirectory(Target+"Avatars");Directory.CreateDirectory(Target+"Hair");
            AssetDatabase.Refresh();
            manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Source+"PlayerAvatarManifest.json"));
            foreach(var part in manifest.parts)
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(Source+part.name+".obj");
                importer.globalScale=1;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importNormals=ModelImporterNormals.Calculate;
                importer.normalSmoothingAngle=0;importer.SaveAndReimport(); // faceted revision 2: flat normals
            }
            for(int i=0;i<Skins.Length;i++)Mat("Skin"+(i+1).ToString("00"),Skins[i]);
            for(int i=0;i<Hairs.Length;i++)Mat("Hair"+(i+1).ToString("00"),Hairs[i]);
            Mat("EyeWhite","F5EDDA");Mat("Iris","483623");Mat("Pupil","151613");Mat("Lips","754E39");
            Mat("BaseShirt","99876A");Mat("BasePants","655740");
            foreach(var (name,hex) in WearMaterials)Mat(name,hex);
            Directory.CreateDirectory(Target+"Wearables");
            foreach(string body in new[]{"Male","Female"})
            {
                foreach(var wear in manifest.wearables??new WearableInfo[0])
                {
                    var root=new GameObject(body+"Wear"+wear.part);
                    try { Add(root.transform,root.name,2,1);PrefabUtility.SaveAsPrefabAsset(root,Target+"Wearables/"+root.name+".prefab"); }
                    finally { UnityEngine.Object.DestroyImmediate(root); }
                }
                foreach(string style in Styles)
                {
                    var root=new GameObject(body+"Hair"+style);
                    try { Add(root.transform,root.name,2,1);PrefabUtility.SaveAsPrefabAsset(root,Target+"Hair/"+root.name+".prefab"); }
                    finally { UnityEngine.Object.DestroyImmediate(root); }
                }
                for(int tone=0;tone<Skins.Length;tone++)
                {
                    var root=Avatar(body,body=="Male"?"Sweep":"Tied",tone,1);
                    try { PrefabUtility.SaveAsPrefabAsset(root,Target+"Avatars/"+body+"Skin"+(tone+1).ToString("00")+".prefab"); }
                    finally { UnityEngine.Object.DestroyImmediate(root); }
                }
            }
            AssetDatabase.SaveAssets();Debug.Log("Default player art: 12 standing art prefabs, 8 hair fit prefabs, 6 skin materials and 5 independent hair colors. No runtime selector or skinning.");
        }
        static Material Mat(string name,string hex=null)
        {
            string path=Target+"Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null) { mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path); }
            if(hex!=null) { ColorUtility.TryParseHtmlString("#"+hex,out Color c);mat.SetColor("_BaseColor",c);mat.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(mat); }
            return mat;
        }
        static void Add(Transform parent,string name,int skin,int hair)
        {
            var info=Array.Find(manifest.parts,p=>p.name==name);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Source+name+".obj");
            if(source==null)throw new InvalidOperationException("Missing mesh "+name);
            var part=UnityEngine.Object.Instantiate(source,parent,false);part.name=name;
            var materials=new Material[info.materials.Length];
            for(int i=0;i<materials.Length;i++)
            {
                string key=info.materials[i];
                materials[i]=Mat(key=="Skin"?"Skin"+(skin+1).ToString("00"):key=="Hair"||key=="Brows"?"Hair"+(hair+1).ToString("00"):key);
            }
            foreach(var r in part.GetComponentsInChildren<MeshRenderer>())
            {
                if(r.GetComponent<MeshFilter>().sharedMesh.subMeshCount!=materials.Length)throw new InvalidOperationException("Material group mismatch: "+name);
                r.sharedMaterials=materials;
            }
        }
        static Transform Pivot(Transform parent,string name,Vector3 position)
        {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;
        }
        static GameObject Avatar(string body,string style,int skin,int hair)
        {
            var root=new GameObject(body+"DefaultPlayer");
            Add(root.transform,body+"Skin",skin,hair);Add(root.transform,body+"BaseShirt",skin,hair);Add(root.transform,body+"BasePants",skin,hair);
            var head=Pivot(root.transform,"HeadAnchor",new Vector3(0,1.43f,0));
            Add(head,body+"Head",skin,hair);Add(head,body+"Eyes",skin,hair);
            var hairRoot=Pivot(head,"HairAnchor",Vector3.zero);Add(hairRoot,body+"Hair"+style,skin,hair);
            Pivot(head,"HatAnchor",Vector3.zero);Pivot(root.transform,"BackpackAnchor",new Vector3(0,1.14f,-.20f));
            Pivot(root.transform,"ShirtAnchor",new Vector3(0,.95f,0));Pivot(root.transform,"PantsAnchor",new Vector3(0,.89f,0));
            Pivot(root.transform,"LeftBootAnchor",new Vector3(-.115f,.10f,0));Pivot(root.transform,"RightBootAnchor",new Vector3(.115f,.10f,0));
            return root;
        }
        [MenuItem("The Trail Walker/Art/Capture Default Player Avatars")]
        public static void CapturePreview()
        {
            manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Source+"PlayerAvatarManifest.json"));Directory.CreateDirectory(Docs);
            Capture("body-and-skin-studies.png",6,2,1.0f,(row,col)=>Avatar(row==0?"Male":"Female",row==0?"Sweep":"Tied",col,1),false);
            Capture("hair-style-studies.png",4,2,.40f,(row,col)=>Avatar(row==0?"Male":"Female",Styles[col],2,1),true);
            Capture("hair-profile-studies.png",4,2,.40f,(row,col)=>{var a=Avatar(row==0?"Male":"Female",Styles[col],2,1);a.transform.rotation=Quaternion.Euler(0,80,0);return a;},true);
            Capture("hair-color-studies.png",5,2,.40f,(row,col)=>Avatar(row==0?"Male":"Female",row==0?"Sweep":"Tied",2,col),true);
            Capture("standing-turnaround.png",3,2,1.0f,(row,col)=>{var a=Avatar(row==0?"Male":"Female",row==0?"Sweep":"Tied",2,1);a.transform.rotation=Quaternion.Euler(0,col*90,0);return a;},false);
            CaptureWearables();
            Debug.Log("Default player actual-mesh previews saved to "+Docs);
        }
        // Tier-one fitted set review (male top, female bottom): basewear, pants only, the full set front, side and back.
        [MenuItem("The Trail Walker/Art/Capture Player Tier 1 Wearables")]
        public static void CaptureWearables()
        {
            manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Source+"PlayerAvatarManifest.json"));Directory.CreateDirectory(Docs);
            Capture("tier1-wearables.png",5,2,1.0f,(row,col)=>
            {
                string body=row==0?"Male":"Female";var a=Avatar(body,row==0?"Sweep":"Tied",2,1);
                bool pantsOnly=col==1,full=col>=2;
                if(pantsOnly||full)Wear(a,body,"HempPants");
                if(full){Wear(a,body,"HempShirt");Wear(a,body,"RabbitBoots");}
                a.transform.rotation=Quaternion.Euler(0,col==3?90:col==4?180:0,0);
                return a;
            },false);
        }
        static void Wear(GameObject avatar,string body,string part)
        {
            Add(avatar.transform,body+"Wear"+part,2,1);
            string hidden=part=="HempShirt"?"BaseShirt":part=="HempPants"?"BasePants":null;
            if(hidden!=null)foreach(Transform child in avatar.transform)if(child.name.EndsWith(hidden))child.gameObject.SetActive(false);
        }
        static void Capture(string file,int columns,int rows,float size,Func<int,int,GameObject> make,bool portrait)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                float pitch=portrait?.33f:.95f;float vertical=portrait?.60f:1.95f;
                for(int row=0;row<rows;row++)for(int col=0;col<columns;col++)
                {
                    var a=make(row,col);a.transform.position=new Vector3(((columns-1)*.5f-col)*pitch,(1-row)*vertical,0);
                    if(portrait)
                    {
                        // Keep the busts independent so upper-row torsos cannot occlude the lower-row faces.
                        foreach(Transform child in a.transform)if(child.name.EndsWith("Skin")||child.name.EndsWith("BaseShirt")||child.name.EndsWith("BasePants"))child.gameObject.SetActive(false);
                        if(a.transform.rotation==Quaternion.identity)a.transform.rotation=Quaternion.Euler(0,-12,0);
                    }
                    preview.AddSingleGO(a);
                }
                float centerY=portrait?1.84f:1.80f;
                preview.camera.orthographic=true;preview.camera.orthographicSize=portrait?.62f:2.0f;
                preview.camera.transform.position=new Vector3(0,centerY,8);preview.camera.transform.LookAt(new Vector3(0,centerY,0));
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.18f,.22f,.24f);
                preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=30;
                preview.lights[0].intensity=1.3f;preview.lights[0].transform.rotation=Quaternion.Euler(25,210,0);preview.lights[1].intensity=.7f;
                preview.ambientColor=new Color(.5f,.5f,.5f);
                int width=portrait?columns*450:columns*400;int height=portrait?1000:1500;
                preview.BeginStaticPreview(new Rect(0,0,width,height));preview.Render(true);var image=preview.EndStaticPreview();
                try { File.WriteAllBytes(Docs+file,image.EncodeToPNG()); }finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            finally { preview.Cleanup(); }
        }
    }
}
