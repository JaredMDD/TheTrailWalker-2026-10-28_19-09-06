using System;
using System.IO;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Movement;
using TheTrailWalker.Travel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Region01EnvironmentAssets
    {
        private const string Source="Assets/_Game/Art/Environment/Region01/";
        private const string Target="Assets/_Game/Resources/Environment/Region01/";
        private static readonly string[] Names={"MeadowBushLow","MeadowBushTall","DryShrub","MeadowPine","YoungPine","WindPine","GrassTuft","WildflowerClump","GroundMound","MeadowBoulder","MeadowGroundPatch","TrailShoulder"};
        static Region01EnvironmentAssets(){EditorApplication.update+=Ensure;}
        private static void Ensure()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=Ensure;if(!File.Exists(Target+Names[0]+".prefab"))Build();
        }
        [MenuItem("The Trail Walker/Art/Rebuild Region 1 Environment Kit")]
        public static void Build()
        {
            Region01ResourceAssets.BuildRegion(Source,Target,"DawnPlainsPalette","Region01Environment",Names);
            var ground=AssetDatabase.LoadAssetAtPath<Material>(Target+"DawnPlainsPalette.mat");ground.enableInstancing=true;EditorUtility.SetDirty(ground);
            var trail=AssetDatabase.LoadAssetAtPath<Material>(Target+"DawnTrail.mat");
            if(trail==null){trail=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(trail,Target+"DawnTrail.mat");}
            trail.SetColor("_BaseColor",new Color(.52f,.39f,.24f));trail.SetFloat("_Smoothness",.04f);EditorUtility.SetDirty(trail);AssetDatabase.SaveAssets();
        }
        [MenuItem("The Trail Walker/Art/Capture Region 1 Environment Kit")]
        public static void CapturePreview()
        {
            string folder="Docs/Art/Environment/Region01";Directory.CreateDirectory(folder);
            for(int i=0;i<Names.Length;i++)
            {
                var preview=new PreviewRenderUtility();
                try
                {
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Target+Names[i]+".prefab");if(prefab==null)throw new InvalidOperationException(Names[i]);
                    var view=UnityEngine.Object.Instantiate(prefab);preview.AddSingleGO(view);
                    var renderers=view.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                    view.transform.position=-bounds.center;
                    preview.camera.orthographic=true;preview.camera.orthographicSize=Mathf.Max(bounds.size.y,Mathf.Max(bounds.size.x,bounds.size.z))*.69f;
                    preview.camera.transform.position=new Vector3(0,3,-6);preview.camera.transform.LookAt(Vector3.zero);
                    preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.19f,.22f,.23f);
                    preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=50;
                    preview.lights[0].intensity=1.3f;preview.lights[0].transform.rotation=Quaternion.Euler(40,30,0);preview.lights[1].intensity=.6f;
                    preview.ambientColor=new Color(.4f,.4f,.4f);preview.BeginStaticPreview(new Rect(0,0,360,320));preview.Render(true);
                    var image=preview.EndStaticPreview();try{File.WriteAllBytes(folder+"/"+Names[i]+".png",image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
                }
                finally{preview.Cleanup();}
            }
            File.WriteAllLines(folder+"/labels.txt",Names);CaptureStudy();
        }
        private static void CaptureStudy()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Capture outside Play mode.");
            Scene previous=SceneManager.GetActiveScene();Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            RenderTexture target=null;Texture2D image=null;Mesh roadMesh=null;RenderTexture old=RenderTexture.active;
            try
            {
                SceneManager.SetActiveScene(scene);var root=new GameObject("Dawn Plains art study - NOT a campaign segment");
                var path=new SampledTrailPath(new[]{new Vector3(-3,0,-18),new Vector3(0,0,20),new Vector3(7,0,55),new Vector3(2,0,100),new Vector3(-2,0,145)});
                TravelScenery.BuildTerrain(root.transform,path,Region01Environment.GroundMaterial,45,3);Region01Environment.StyleTerrain(root.transform);
                var study=new SegmentDefinition("art.study.dawn-plains","art.camp.a","art.camp.b",path.Length);
                Region01Environment.BuildStudy(root.transform,path,study,Array.Empty<InteractableAnchor>());
                int count=Mathf.CeilToInt(path.Length)+1;var vertices=new Vector3[count*2];var triangles=new int[(count-1)*6];
                for(int i=0;i<count;i++)
                {
                    path.Evaluate(path.Length*i/(count-1),out Vector3 p,out Vector3 t);Vector3 side=Vector3.Cross(Vector3.up,t)*1.7f;
                    vertices[i*2]=p-side;vertices[i*2+1]=p+side;
                    if(i==count-1)continue;int v=i*2,k=i*6;triangles[k]=v;triangles[k+1]=v+2;triangles[k+2]=v+1;triangles[k+3]=v+1;triangles[k+4]=v+2;triangles[k+5]=v+3;
                }
                roadMesh=new Mesh{name="Illustrative path only",vertices=vertices,triangles=triangles};roadMesh.RecalculateNormals();
                var road=new GameObject("Path fit study",typeof(MeshFilter),typeof(MeshRenderer));road.transform.SetParent(root.transform,false);
                road.GetComponent<MeshFilter>().sharedMesh=roadMesh;road.GetComponent<MeshRenderer>().sharedMaterial=Region01Environment.TrailMaterial;
                var sunlight=new GameObject("Art study sun").AddComponent<Light>();sunlight.type=LightType.Directional;sunlight.intensity=1.4f;sunlight.transform.rotation=Quaternion.Euler(48,-35,0);
                var cam=new GameObject("Art study camera").AddComponent<Camera>();cam.transform.position=new Vector3(28,19,18);cam.transform.LookAt(new Vector3(0,1,71));cam.fieldOfView=57;
                cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.65f,.74f,.72f);cam.nearClipPlane=.1f;cam.farClipPlane=210;cam.cullingMask=1<<29;
                foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=29;sunlight.cullingMask=1<<29;
                target=new RenderTexture(1600,1000,24);target.Create();
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
                RenderTexture.active=target;image=new Texture2D(1600,1000,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();
                File.WriteAllBytes("Docs/Art/Environment/Region01/dawn-plains-art-study.png",image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=old;if(image!=null)UnityEngine.Object.DestroyImmediate(image);if(roadMesh!=null)UnityEngine.Object.DestroyImmediate(roadMesh);
                if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                EditorSceneManager.CloseScene(scene,true);if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);
            }
        }
    }
    internal sealed class Region01EnvironmentImport : AssetPostprocessor
    {
        private const string Source="Assets/_Game/Art/Environment/Region01/";
        private void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.isReadable=true; /* runtime CombineMeshes needs CPU-readable meshes on device */m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;m.addCollider=false;
            m.importNormals=ModelImporterNormals.Calculate;m.normalSmoothingAngle=0;m.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        private void OnPreprocessTexture()
        {
            if(assetPath!=Source+"DawnPlainsPalette.png")return;
            var t=(TextureImporter)assetImporter;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;
            t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
