using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    public static class BalloonMerchantAssets
    {
        const string Source="Assets/_Game/Art/Environment/BalloonMerchant/";
        const string Target="Assets/_Game/Resources/Environment/BalloonMerchant/";
        const string Docs="Docs/Art/Environment/BalloonMerchant/";
        const float Period=4.8f,Rise=.12f;
        // Unity's OBJ importer reflects source X. Keep animation endpoints in imported Unity space.
        static readonly Vector3 StakeTie=new Vector3(-2,.22f,-1.37f),CabinTie=new Vector3(-.96f,.78f,-.76f);
        static readonly string[] Names={"Cabin","FrontDoor","PagodaRoof","WarmLanterns","BalloonEnvelope","BalloonHarness","GroundStake","GroundTether","CabinTieRing"};
        static readonly string[] Floating={"Cabin","FrontDoor","PagodaRoof","WarmLanterns","BalloonEnvelope","BalloonHarness","CabinTieRing"};

        [MenuItem("The Trail Walker/Art/Build Balloon Merchant")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build balloon merchant art outside Play mode.");
            Region01ResourceAssets.BuildRegion(Source,Target+"Parts/","BalloonMerchantPalette","BalloonMerchant",Names);
            Directory.CreateDirectory(Target+"Animation");AssetDatabase.Refresh();
            var root=new GameObject("BalloonMerchant");
            try
            {
                var floating=new GameObject("FloatingAssembly").transform;floating.SetParent(root.transform,false);
                foreach(string name in Floating)Add(floating,name);
                var ground=new GameObject("GroundAnchor").transform;ground.SetParent(root.transform,false);Add(ground,"GroundStake");
                var tether=new GameObject("TetherPivot").transform;tether.SetParent(ground,false);tether.localPosition=StakeTie;
                Add(tether,"GroundTether");SetPose(root,0);
                var animator=root.AddComponent<Animator>();animator.runtimeAnimatorController=MakeController();animator.applyRootMotion=false;
                animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                var doorAnchor=new GameObject("DoorInteractionAnchor").transform;doorAnchor.SetParent(floating,false);doorAnchor.localPosition=new Vector3(0,1.45f,-.82f);
                PrefabUtility.SaveAsPrefabAsset(root,Target+"BalloonMerchant.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();Debug.Log("Balloon merchant art and 4.8s bob clip built. Stake fixed; tether follows cabin. No region placement or merchant logic.");
        }
        static void Add(Transform parent,string name)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"Parts/"+name+".prefab");
            if(source==null)throw new InvalidOperationException("Missing balloon merchant part "+name);
            var part=UnityEngine.Object.Instantiate(source,parent,false);part.name=name;
        }
        static float Bob(float time)=>Rise*.5f*(1-Mathf.Cos(time*Mathf.PI*2/Period));
        static Quaternion RopeRotation(float time)=>Quaternion.FromToRotation(Vector3.up,(CabinTie+Vector3.up*Bob(time)-StakeTie).normalized);
        static void SetPose(GameObject root,float time)
        {
            root.transform.Find("FloatingAssembly").localPosition=Vector3.up*Bob(time);
            var tether=root.transform.Find("GroundAnchor/TetherPivot");
            tether.localRotation=RopeRotation(time);tether.localScale=new Vector3(1,(CabinTie+Vector3.up*Bob(time)-StakeTie).magnitude,1);
        }
        static AnimatorController MakeController()
        {
            string clipPath=Target+"Animation/BalloonMerchantBob.anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,clipPath);}
            clip.ClearCurves();clip.frameRate=30;
            const int samples=32;
            var position=new AnimationCurve();var length=new AnimationCurve();
            var rotation=new[]{new AnimationCurve(),new AnimationCurve(),new AnimationCurve(),new AnimationCurve()};
            for(int i=0;i<=samples;i++)
            {
                float t=Period*i/samples;
                float velocity=Rise*.5f*Mathf.Sin(t*Mathf.PI*2/Period)*Mathf.PI*2/Period;
                position.AddKey(new Keyframe(t,Bob(t),velocity,velocity));
                Vector3 direction=CabinTie+Vector3.up*Bob(t)-StakeTie;
                float stretchVelocity=direction.y/direction.magnitude*velocity;
                length.AddKey(new Keyframe(t,direction.magnitude,stretchVelocity,stretchVelocity));
                Quaternion q=RopeRotation(t),before=RopeRotation(t-.001f),after=RopeRotation(t+.001f);
                for(int c=0;c<4;c++)
                {
                    float tangent=(after[c]-before[c])/.002f;
                    rotation[c].AddKey(new Keyframe(t,q[c],tangent,tangent));
                }
            }
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("FloatingAssembly",typeof(Transform),"m_LocalPosition.y"),position);
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("GroundAnchor/TetherPivot",typeof(Transform),"m_LocalScale.y"),length);
            for(int i=0;i<4;i++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("GroundAnchor/TetherPivot",typeof(Transform),"m_LocalRotation."+"xyzw"[i]),rotation[i]);
            clip.EnsureQuaternionContinuity();
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=false;AnimationUtility.SetAnimationClipSettings(clip,settings);EditorUtility.SetDirty(clip);
            string controllerPath=Target+"Animation/BalloonMerchant.controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine=controller.layers[0].stateMachine;
            AnimatorState state=null;
            foreach(var child in machine.states)if(child.state.name=="Gentle Bob")state=child.state;
            if(state==null)state=machine.AddState("Gentle Bob");state.motion=clip;machine.defaultState=state;EditorUtility.SetDirty(controller);
            return controller;
        }
        [MenuItem("The Trail Walker/Art/Capture Balloon Merchant")]
        public static void CapturePreview()
        {
            Directory.CreateDirectory(Docs);
            Capture("balloon-merchant-hero.png",new Vector3(8,5.8f,-12),new Vector3(-.3f,3.15f,0),3.70f,1250,1500,0,false);
            Capture("balloon-merchant-front.png",new Vector3(0,3.2f,-14),new Vector3(.3f,3.2f,0),3.55f,1100,1500,0,false);
            Capture("balloon-merchant-door-detail.png",new Vector3(3,2.8f,-6),new Vector3(0,1.65f,-.1f),1.65f,1400,1000,0,false);
            Capture("balloon-merchant-bob-study.png",new Vector3(0,3.4f,-16),new Vector3(-.25f,3.2f,0),3.65f,2200,1000,0,true);
            VerifyAnimation();
        }
        static void Capture(string file,Vector3 camera,Vector3 target,float size,int width,int height,float time,bool phases)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"BalloonMerchant.prefab");
                int count=phases?3:1;
                for(int i=0;i<count;i++)
                {
                    var view=UnityEngine.Object.Instantiate(prefab);UnityEngine.Object.DestroyImmediate(view.GetComponent<Animator>());
                    SetPose(view,phases?i*Period/4:time);view.transform.position=new Vector3(phases?(1-i)*4.4f:0,0,0);preview.AddSingleGO(view);
                }
                // Neutral ground reference is preview-only; it is not part of the floating storefront.
                var ground=GameObject.CreatePrimitive(PrimitiveType.Cube);UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>());
                ground.transform.position=new Vector3(0,-.075f,0);ground.transform.localScale=new Vector3(phases?17:8,.10f,6);
                var groundMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));groundMat.SetColor("_BaseColor",new Color(.20f,.23f,.19f));ground.GetComponent<Renderer>().sharedMaterial=groundMat;preview.AddSingleGO(ground);
                preview.camera.orthographic=true;preview.camera.orthographicSize=size;preview.camera.transform.position=camera;preview.camera.transform.LookAt(target);
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.15f,.19f,.21f);
                preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=60;
                preview.lights[0].intensity=1.5f;preview.lights[0].transform.rotation=Quaternion.Euler(35,145,0);preview.lights[1].intensity=.7f;
                preview.ambientColor=new Color(.5f,.47f,.41f);
                preview.BeginStaticPreview(new Rect(0,0,width,height));preview.Render(true);var image=preview.EndStaticPreview();
                try{File.WriteAllBytes(Docs+file,image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(groundMat);}
            }
            finally{preview.Cleanup();}
        }
        public static void VerifyAnimation()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Target+"BalloonMerchant.prefab");
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(Target+"Animation/BalloonMerchantBob.anim");
            var view=UnityEngine.Object.Instantiate(prefab);UnityEngine.Object.DestroyImmediate(view.GetComponent<Animator>());
            float maxError=0;
            try
            {
                var stakeBounds=view.transform.Find("GroundAnchor/GroundStake").GetComponentInChildren<Renderer>().bounds;
                var ringBounds=view.transform.Find("FloatingAssembly/CabinTieRing").GetComponentInChildren<Renderer>().bounds;
                if(Mathf.Abs(stakeBounds.center.x-StakeTie.x)>.005f||Mathf.Abs(stakeBounds.center.z-StakeTie.z)>.005f)
                    throw new InvalidOperationException("Animation ground tie does not align to imported stake geometry.");
                if(Vector3.Distance(ringBounds.center,CabinTie)>.005f)throw new InvalidOperationException("Animation cabin tie does not align to imported ring geometry.");
                for(int i=0;i<=96;i++)
                {
                    clip.SampleAnimation(view,Period*i/96);
                    var rope=view.transform.Find("GroundAnchor/TetherPivot");
                    var end=rope.TransformPoint(Vector3.up);
                    var tie=view.transform.Find("FloatingAssembly").TransformPoint(CabinTie);
                    maxError=Mathf.Max(maxError,Vector3.Distance(end,tie));
                    if(Vector3.Distance(rope.position,StakeTie)>.0001f)throw new InvalidOperationException("Ground tie moved.");
                }
                if(maxError>.002f)throw new InvalidOperationException("Tether endpoint error "+maxError);
                var start=view.transform.Find("FloatingAssembly");clip.SampleAnimation(view,0);Vector3 initial=start.localPosition;
                clip.SampleAnimation(view,Period);if(Vector3.Distance(initial,start.localPosition)>.0001f)throw new InvalidOperationException("Bob loop does not close.");
                File.WriteAllText(Docs+"animation-validation.txt","PASS: actual AnimationClip sampled at 97 times over 4.8 s; fixed ground tie; closed position loop; max tether endpoint error "+maxError.ToString("F6")+" m. Rise 0.12 m. No scene placement or device test.");
            }
            finally{UnityEngine.Object.DestroyImmediate(view);}
        }
    }
    internal sealed class BalloonMerchantImport : AssetPostprocessor
    {
        const string Source="Assets/_Game/Art/Environment/BalloonMerchant/";
        void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var importer=(ModelImporter)assetImporter;importer.globalScale=1;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;
            importer.importNormals=ModelImporterNormals.Calculate;importer.normalSmoothingAngle=0;importer.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        void OnPreprocessTexture()
        {
            if(assetPath!=Source+"BalloonMerchantPalette.png")return;
            var importer=(TextureImporter)assetImporter;importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Clamp;
            importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;importer.sRGBTexture=true;
        }
    }
}
