using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheTrailWalker.Editor
{
    [InitializeOnLoad]
    public static class Tier01EnvironmentAssets
    {
        private const string Source="Assets/_Game/Art/Environment/Tier01/";
        private const string Target="Assets/_Game/Resources/Environment/Tier01/";
        [Serializable] private sealed class ModelData
        {
            public string Id,Name,Category,ResourcePath;
            public int Triangles;
            public float[] BoundsMin,BoundsMax;
            public float FootprintRadius,RecommendedRoadClearance;
            public bool CastsShadows,DistantOnly,TerrainReplacement;
        }
        [Serializable] private sealed class RegionData
        {
            public int Number,PaletteSize;
            public string Name;
            public int[] TrailColorRGB;
            public ModelData[] Models;
        }
        [Serializable] private sealed class Data
        {
            public int Revision;
            public RegionData[] Regions;
            public string Policy;
        }
        static Tier01EnvironmentAssets(){EditorApplication.update+=Ensure;}
        private static void Ensure()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            EditorApplication.update-=Ensure;if(!File.Exists(Target+"Tier01EnvironmentLibrary.asset"))Build();
        }
        [MenuItem("The Trail Walker/Art/Rebuild All Tier 1 Region Environment Kits")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Build outside Play mode.");
            var data=JsonUtility.FromJson<Data>(File.ReadAllText(Source+"Tier01EnvironmentManifest.json"));
            var regions=new RegionalEnvironmentKit[data.Regions.Length];
            foreach(var region in data.Regions)
            {
                string suffix="Region"+region.Number.ToString("00")+"/",source=Source+suffix,target=Target+suffix;
                Region01ResourceAssets.BuildRegion(source,target,"EnvironmentPalette","Tier01Environment"+region.Number,region.Models.Select(m=>m.Name).ToArray());
                var material=AssetDatabase.LoadAssetAtPath<Material>(target+"EnvironmentPalette.mat");material.enableInstancing=true;EditorUtility.SetDirty(material);
                string trailPath=target+"Trail.mat";var trail=AssetDatabase.LoadAssetAtPath<Material>(trailPath);
                if(trail==null){trail=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(trail,trailPath);}
                trail.SetColor("_BaseColor",new Color(region.TrailColorRGB[0]/255f,region.TrailColorRGB[1]/255f,region.TrailColorRGB[2]/255f));trail.SetFloat("_Smoothness",.04f);EditorUtility.SetDirty(trail);
                var records=new RegionalSceneryArt[region.Models.Length];
                for(int i=0;i<records.Length;i++)
                {
                    var model=region.Models[i];string path=target+model.Name+".prefab";var contents=PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        foreach(var filter in contents.GetComponentsInChildren<MeshFilter>())
                            if(filter.sharedMesh==null||!filter.sharedMesh.isReadable)throw new InvalidOperationException("Runtime combining requires readable mesh: "+model.Id);
                        foreach(var renderer in contents.GetComponentsInChildren<MeshRenderer>())
                        {
                            renderer.shadowCastingMode=model.CastsShadows?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off;
                            renderer.receiveShadows=true;
                        }
                        PrefabUtility.SaveAsPrefabAsset(contents,path);
                    }
                    finally{PrefabUtility.UnloadPrefabContents(contents);}
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var renderers=prefab.GetComponentsInChildren<Renderer>();Bounds importedBounds=renderers[0].bounds;
                    foreach(var renderer in renderers)importedBounds.Encapsulate(renderer.bounds);
                    records[i]=new RegionalSceneryArt{Id=model.Id,Name=model.Name,Category=model.Category,ResourcePath=model.ResourcePath,Prefab=prefab,Bounds=importedBounds,Triangles=model.Triangles,FootprintRadius=model.FootprintRadius,RecommendedRoadClearance=model.RecommendedRoadClearance,CastsShadows=model.CastsShadows,DistantOnly=model.DistantOnly,TerrainReplacement=model.TerrainReplacement};
                }
                regions[region.Number-1]=new RegionalEnvironmentKit{Number=region.Number,Name=region.Name,PaletteSize=region.PaletteSize,SharedMaterial=material,TrailMaterial=trail,Models=records};
            }
            const string catalogPath=Target+"Tier01EnvironmentLibrary.asset";var library=AssetDatabase.LoadAssetAtPath<Tier01EnvironmentLibrary>(catalogPath);
            if(library==null){library=ScriptableObject.CreateInstance<Tier01EnvironmentLibrary>();AssetDatabase.CreateAsset(library,catalogPath);}
            library.Revision=data.Revision;library.Regions=regions;
            library.DrawingContract=data.Policy+" Each regional material is a separate combine group; split tall shadow casters from noncasting ground cover. Opaque geometry only. All terrain/water modules require authored stitching/replacement and must not be layered coplanar with the live terrain or ribbon. Use the current ground-height projection and road/camp/resource clearance when placing. No live renderer/quality settings are changed.";
            EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
            File.WriteAllText("Temp/Tier01EnvironmentLibrary-art.result","SUCCEEDED: "+regions.Sum(r=>r.Models.Length)+" readable environment prefabs, six regional materials/trail colors and authoring catalog.");
        }
        [MenuItem("The Trail Walker/Art/Capture All Tier 1 Region Environment Kits")]
        public static void CapturePreview()
        {
            var library=AssetDatabase.LoadAssetAtPath<Tier01EnvironmentLibrary>(Target+"Tier01EnvironmentLibrary.asset");if(library==null)throw new InvalidOperationException("Build the library first.");
            foreach(var region in library.Regions)
            {
                string directory="Docs/Art/Environment/Tier01/Region"+region.Number.ToString("00");Directory.CreateDirectory(directory);
                foreach(var model in region.Models)CaptureModel(model,directory);
                File.WriteAllLines(directory+"/labels.txt",region.Models.Select(m=>m.Name));CaptureDiorama(region,directory);
            }
        }
        private static void Lighting(PreviewRenderUtility preview)
        {
            preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.19f,.22f,.23f);
            preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=250;
            preview.lights[0].intensity=1.3f;preview.lights[0].transform.rotation=Quaternion.Euler(45,30,0);preview.lights[1].intensity=.65f;preview.ambientColor=new Color(.4f,.4f,.4f);
        }
        private static void Save(PreviewRenderUtility preview,int width,int height,string path)
        {
            preview.BeginStaticPreview(new Rect(0,0,width,height));preview.Render(true);Texture2D image=preview.EndStaticPreview();
            try{File.WriteAllBytes(path,image.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(image);}
        }
        private static void CaptureModel(RegionalSceneryArt model,string directory)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                var view=UnityEngine.Object.Instantiate(model.Prefab);preview.AddSingleGO(view);
                var renderers=view.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                view.transform.position=-bounds.center;Lighting(preview);preview.camera.orthographic=true;
                preview.camera.orthographicSize=Mathf.Max(bounds.size.y,Mathf.Max(bounds.size.x,bounds.size.z))*.7f;
                preview.camera.transform.position=new Vector3(0,3,-6);preview.camera.transform.LookAt(Vector3.zero);
                Save(preview,320,280,directory+"/"+model.Name+".png");
            }
            finally{preview.Cleanup();}
        }
        private static void CaptureDiorama(RegionalEnvironmentKit region,string directory)
        {
            // An asset arrangement, without camps, gameplay paths, segment IDs, scatter or placement changes.
            var preview=new PreviewRenderUtility();
            try
            {
                var views=new List<GameObject>();
                var ground=region.Models.FirstOrDefault(m=>m.Category=="Terrain"&&m.Name.Contains("GroundPatch"))??region.Models.First(m=>m.Category=="Terrain"&&m.Name.Contains("Clearing"));
                var water=region.Models.FirstOrDefault(m=>m.Category=="Water"&&m.Name.Contains("Shallow"));
                for(int x=0;x<2;x++)for(int z=0;z<2;z++)
                {
                    var tile=x==1&&water!=null?water:ground;Vector3 size=tile.Bounds.size;
                    Place(tile,new Vector3((x-.5f)*6,0,(z-.5f)*6),0,new Vector3(6/size.x,1,6/size.z));
                }
                var trees=region.Models.Where(m=>m.Category=="Tree").ToArray();int treeCount=region.Number==1?2:region.Number==3?5:3;
                for(int i=0;i<treeCount;i++)Place(trees[i%trees.Length],new Vector3(-4.7f+(i%3)*3.1f,0,2.2f+(i/3)*2),i*91,Vector3.one*.8f);
                var shrubs=region.Models.Where(m=>m.Category=="Shrub").ToArray();
                for(int i=0;i<4&&shrubs.Length>0;i++)Place(shrubs[i%shrubs.Length],new Vector3(-4+i*2.1f,0,-.4f),i*43,Vector3.one*.7f);
                var covers=region.Models.Where(m=>m.Category=="GroundCover").ToArray();
                for(int i=0;i<12&&covers.Length>0;i++)Place(covers[i%covers.Length],new Vector3(-5+(i%6)*1.65f,0,-4.3f+(i/6)*1.8f),i*37,Vector3.one*.85f);
                var rocks=region.Models.Where(m=>m.Category=="Rock").ToArray();
                if(rocks.Length>0)Place(rocks[0],new Vector3(4.5f,0,.5f),25,Vector3.one*.7f);
                var feature=region.Models.FirstOrDefault(m=>m.Name.Contains(region.Number==5?"RidgePeak":region.Number==2?"RoundedHill":region.Number==4?"HollowRim":region.Number==6?"MudBank":region.Number==3?"FallenForest":"GroundMound"));
                if(feature!=null)Place(feature,new Vector3(4,0,3.5f),20,Vector3.one*(region.Number==5 ? .3f : .55f));
                Lighting(preview);preview.camera.orthographic=true;
                var renderers=views.SelectMany(v=>v.GetComponentsInChildren<Renderer>()).ToArray();Bounds framing=renderers[0].bounds;foreach(var renderer in renderers)framing.Encapsulate(renderer.bounds);
                preview.camera.transform.position=framing.center+new Vector3(11,9,-15);preview.camera.transform.LookAt(framing.center);
                float halfHeight=0,halfWidth=0;
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                {
                    Vector3 corner=Vector3.Scale(framing.extents,new Vector3(x,y,z));
                    halfHeight=Mathf.Max(halfHeight,Mathf.Abs(Vector3.Dot(corner,preview.camera.transform.up)));
                    halfWidth=Mathf.Max(halfWidth,Mathf.Abs(Vector3.Dot(corner,preview.camera.transform.right)));
                }
                preview.camera.orthographicSize=Mathf.Max(halfHeight,halfWidth/1.4f)*1.07f;
                Save(preview,1400,1000,directory+"/biome-art-study.png");
                void Place(RegionalSceneryArt asset,Vector3 position,float yaw,Vector3 scale)
                {
                    var view=UnityEngine.Object.Instantiate(asset.Prefab);view.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));view.transform.localScale=scale;preview.AddSingleGO(view);views.Add(view);
                }
            }
            finally{preview.Cleanup();}
        }
    }
    internal sealed class Tier01EnvironmentImport : AssetPostprocessor
    {
        private const string Source="Assets/_Game/Art/Environment/Tier01/";
        private void OnPreprocessModel()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal))return;
            var m=(ModelImporter)assetImporter;m.isReadable=true;m.globalScale=1;m.importAnimation=false;m.importCameras=false;m.importLights=false;m.addCollider=false;
            m.importNormals=ModelImporterNormals.Calculate;m.normalSmoothingAngle=0;m.materialImportMode=ModelImporterMaterialImportMode.None;
        }
        private void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith(Source,StringComparison.Ordinal)||!assetPath.EndsWith("EnvironmentPalette.png",StringComparison.Ordinal))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.mipmapEnabled=false;
            t.textureCompression=TextureImporterCompression.Uncompressed;t.npotScale=TextureImporterNPOTScale.None;t.sRGBTexture=true;
        }
    }
}
