using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace TheTrailWalker.Editor {
// Selective passive art builder: never touches the massive tree assets or live scenes.
public static class LumberFieldArtAssets {
 const string Source="Assets/_Game/Art/Settlement/LumberField/",Doc="Docs/Art/Settlement/LumberFieldStorefront/";
 [Serializable] class Node {public string Name,Parent,Model,Reference;public float[] LocalPosition,LocalRotation,LookAt;}
 [Serializable] class Piece {public string Model,SourcePath,PalettePath,MaterialGroup;public int Triangles;public float[] BoundsMin,BoundsMax;}
 [Serializable] class Assembly {public string Name,PrefabPath,Species;public Node[] Nodes;public int Triangles,TriangleCeiling,Tier;public float Width,ColumnSpacing,RowSpacing,FrontClearance,BackFenceZ,FrontZ,SweepMaxZ,StandingMinZ,Margin;public float[][] TreeAnchors;public string[] VariantSequence;}
 [Serializable] class Manifest {public Assembly[] Assemblies;public Piece[] Pieces;}
 static Manifest Load()=>JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Source+"LumberFieldArtManifest.json"));
 static Vector3 V(float[] a)=>new Vector3(a[0],a[1],a[2]);
 static string PathFor(string name)=>"Assets/_Game/Resources/"+Load().Assemblies.Single(x=>x.Name==name).PrefabPath+".prefab";
 // LumberT2: the tier 2 (birch) front has its own, longer palette strip, so its own material.
 static string MatPath(string group)=>group=="Forester"?"Assets/_Game/Resources/Characters/Shopkeepers/ForesterArt.mat":group=="LumberT2"?"Assets/_Game/Resources/Settlement/Storefronts/LumberArtT2.mat":"Assets/_Game/Resources/Settlement/Storefronts/LumberArt.mat";
 static void Prepare() {
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Build outside Play mode");AssetDatabase.Refresh();var manifest=Load();
  foreach(var p in manifest.Pieces.Where(x=>x.MaterialGroup!="LumberT2"))PreparePiece(p);
  foreach(var group in new[]{"Lumber","Forester"})PrepareMaterial(manifest,group);
 }
 static void PreparePiece(Piece p) {
  var tex=(TextureImporter)AssetImporter.GetAtPath(p.PalettePath);tex.filterMode=FilterMode.Point;tex.mipmapEnabled=false;tex.wrapMode=TextureWrapMode.Clamp;tex.alphaSource=TextureImporterAlphaSource.FromInput;tex.alphaIsTransparency=false;tex.textureCompression=TextureImporterCompression.Uncompressed;tex.npotScale=TextureImporterNPOTScale.None;tex.maxTextureSize=4096;tex.SaveAndReimport();
  var imp=(ModelImporter)AssetImporter.GetAtPath(p.SourcePath);imp.globalScale=1;imp.isReadable=true;imp.addCollider=false;imp.importAnimation=false;imp.importCameras=false;imp.importLights=false;imp.materialImportMode=ModelImporterMaterialImportMode.None;imp.importNormals=ModelImporterNormals.Calculate;imp.normalSmoothingAngle=0;imp.SaveAndReimport();
 }
 static void PrepareMaterial(Manifest manifest,string group) {
  string path=MatPath(group);Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));var material=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(material==null){material=new Material(Shader.Find("TheTrailWalker/FacetedDetail"));AssetDatabase.CreateAsset(material,path);}
  var row=manifest.Pieces.First(p=>p.MaterialGroup==group);FacetedDetailMaterials.Configure(material,AssetDatabase.LoadAssetAtPath<Texture2D>(row.PalettePath));material.SetColor("_BaseColor",Color.white);material.SetFloat("_DetailScale",2);material.SetFloat("_DetailStrength",.35f);EditorUtility.SetDirty(material);
 }
 // Narrow path (Milestone 14): only the tier 2 LumberFrontT2 (its piece, material and prefab, plus its StorefrontManifest part row);
 // never re-imports or rebuilds the tier 1 lumber pieces and prefabs.
 public static void BuildFrontT2() {
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Build outside Play mode");AssetDatabase.Refresh();var manifest=Load();
  var piece=manifest.Pieces.Single(p=>p.Model=="LumberFrontT2");PreparePiece(piece);PrepareMaterial(manifest,"LumberT2");BuildOne("LumberFrontT2");
  const string store="Assets/_Game/Art/Settlement/Storefronts/StorefrontManifest.json";
  var s=JObject.Parse(File.ReadAllText(store));Upsert((JArray)s["Parts"],new JObject{{"name",piece.Model},{"triangles",piece.Triangles}},"name");File.WriteAllText(store,s.ToString(Formatting.Indented)+"\n");
 }
 static void BuildOne(string name) {
  var m=Load();var a=m.Assemblies.Single(x=>x.Name==name);var root=new GameObject(name);var nodes=new Dictionary<string,Transform>();
  try {
   foreach(var n in a.Nodes) {
    var t=new GameObject(n.Name).transform;t.SetParent(n.Parent==null?root.transform:nodes[n.Parent],false);t.localPosition=V(n.LocalPosition);t.localRotation=Quaternion.Euler(V(n.LocalRotation));nodes.Add(n.Name,t);
    if(n.LookAt!=null)t.rotation=Quaternion.LookRotation(V(n.LookAt)-t.position);
    if(n.Model!=null) {
     var piece=m.Pieces.Single(x=>x.Model==n.Model);var source=AssetDatabase.LoadAssetAtPath<GameObject>(piece.SourcePath);if(source==null)throw new Exception(piece.SourcePath);
     var visual=Object.Instantiate(source,t,false);visual.name=n.Model+"Visual";
     // Unity's OBJ importer reflects X. Restore the authored coordinates so mesh
     // openings, asymmetric props and hinge geometry agree with runtime anchors.
     visual.transform.localScale=new Vector3(-1,1,1);
     var material=AssetDatabase.LoadAssetAtPath<Material>(MatPath(piece.MaterialGroup));
     foreach(var renderer in visual.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=Enumerable.Repeat(material,renderer.sharedMaterials.Length).ToArray();
    }
    if(n.Reference!=null) {
     var source=AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(n.Reference));if(source==null)throw new Exception("Build reference first "+n.Reference);
     var visual=(GameObject)PrefabUtility.InstantiatePrefab(source,t);visual.name=n.Reference;
    }
   }
   string path=PathFor(name);Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));PrefabUtility.SaveAsPrefabAsset(root,path);
  }finally{Object.DestroyImmediate(root);}
 }
 public static string BuildAll() {Prepare();BuildOne("ShopkeeperForester");BuildOne("LumberFront");BuildOne("LumberCounter");for(int i=1;i<=4;i++)BuildOne("LumberZoneT"+i);UpdateManifests();AssetDatabase.SaveAssets();return "BUILT seven passive lumber prefabs";}
 public static void BuildForester() {Prepare();BuildOne("ShopkeeperForester");}
 public static void BuildStorefront() {Prepare();BuildOne("ShopkeeperForester");BuildOne("LumberFront");BuildOne("LumberCounter");}
 static void Upsert(JArray array,JObject row,string key) {var old=array.OfType<JObject>().FirstOrDefault(x=>(string)x[key]==(string)row[key]);if(old==null)array.Add(row);else foreach(var prop in row.Properties())old[prop.Name]=prop.Value.DeepClone();}
 static void UpdateManifests() {
  const string store="Assets/_Game/Art/Settlement/Storefronts/StorefrontManifest.json",keepers="Assets/_Game/Art/Characters/Shopkeepers/ShopkeeperManifest.json";
  var s=JObject.Parse(File.ReadAllText(store));Upsert((JArray)s["Shops"],new JObject{{"Name","Lumber"},{"Id","shop.lumber"},{"Front","LumberFront"},{"Counter","LumberCounter"},{"Keeper","ShopkeeperForester"},{"Width",7.8f},{"OfferAnchors",new JArray()},{"OfferAnchorsFlat",new JArray()},{"OfferKinds",new JArray()},{"ArtBuilder","LumberFieldArtAssets"}},"Name");
  foreach(var p in Load().Pieces.Where(x=>x.MaterialGroup=="Lumber"&&!x.Model.StartsWith("LumberZone")))Upsert((JArray)s["Parts"],new JObject{{"name",p.Model},{"triangles",p.Triangles}},"name");File.WriteAllText(store,s.ToString(Formatting.Indented)+"\n");
  var k=JObject.Parse(File.ReadAllText(keepers));Upsert((JArray)k["Keepers"],new JObject{{"Name","Forester"},{"Shop","Lumber"},{"Body","ForesterBody"},{"Head","ForesterHead"},{"Female",false},{"Triangles",Load().Assemblies.Single(x=>x.Name=="ShopkeeperForester").Triangles},{"Notes","Plaid-looking olive cloth, rolled sleeves, leather apron and felt cap. Dedicated passive lumber art builder."}},"Name");File.WriteAllText(keepers,k.ToString(Formatting.Indented)+"\n");
 }
 public static string Validate() {
  var m=Load();var reports=new List<object>();
  foreach(var p in m.Pieces) {
   var imp=(ModelImporter)AssetImporter.GetAtPath(p.SourcePath);if(!imp.isReadable)throw new Exception("Read/Write "+p.Model);
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(p.SourcePath);int count=source.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);if(count!=p.Triangles)throw new Exception("Count "+p.Model);
  }
  foreach(var a in m.Assemblies) {
   var root=AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(a.Name));if(root.GetComponentsInChildren<Collider>(true).Length>0||root.GetComponentsInChildren<MonoBehaviour>(true).Length>0)throw new Exception("Passive contract "+a.Name);
   var transforms=root.GetComponentsInChildren<Transform>(true);
   var actualNodes=new Dictionary<string,Transform>();
   foreach(var n in a.Nodes) {
    var parent=n.Parent==null?root.transform:actualNodes[n.Parent];var t=parent.Find(n.Name);if(t==null)throw new Exception("Missing node "+a.Name+" "+n.Name);actualNodes.Add(n.Name,t);
    if((t.localPosition-V(n.LocalPosition)).magnitude>.00001f)throw new Exception("Node position "+n.Name);
    if(n.Model!=null) {
     var piece=m.Pieces.Single(x=>x.Model==n.Model);var visual=t.Find(n.Model+"Visual");
     var points=visual.GetComponentsInChildren<MeshFilter>().SelectMany(f=>f.sharedMesh.vertices.Select(v=>t.InverseTransformPoint(f.transform.TransformPoint(v)))).ToArray();
     var min=new Vector3(points.Min(v=>v.x),points.Min(v=>v.y),points.Min(v=>v.z));var max=new Vector3(points.Max(v=>v.x),points.Max(v=>v.y),points.Max(v=>v.z));
     if((min-V(piece.BoundsMin)).magnitude>.001f||(max-V(piece.BoundsMax)).magnitude>.001f)throw new Exception("Imported geometry/anchor alignment "+n.Model);
    }
   }
   int count=root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);if(count!=a.Triangles||count>a.TriangleCeiling)throw new Exception("Budget "+a.Name);
   foreach(var r in root.GetComponentsInChildren<Renderer>())foreach(var mat in r.sharedMaterials)if(mat.shader.name!="TheTrailWalker/FacetedDetail")throw new Exception("Shader "+a.Name);
   if(a.Name=="LumberFront") {
    var renderers=root.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);if(bounds.size.x>a.Width+.01f)throw new Exception("Front width exceeds manifest "+bounds.size.x);
   }
   if(a.Name=="LumberCounter") {
    if(transforms.Any(t=>t.name.StartsWith("OfferAnchor")))throw new Exception("Unexpected offers");var pivot=root.transform.Find("DoorPivot");if(pivot.Find("DoorLeaf")==null)throw new Exception("Door hierarchy");
    if(root.transform.Find("DoorSelectAnchor").localPosition.x>=0)throw new Exception("Door must be on customer right (-X)");
   }
   if(a.Name.StartsWith("LumberZone")) {
    if(transforms.Any(t=>t.name.StartsWith("Massive")))throw new Exception("Trees stored in zone");if(transforms.Count(t=>t.name.StartsWith("TreeAnchor"))!=6)throw new Exception("Tree anchor count");
    if(transforms.Count(t=>t.name.StartsWith("BackdropAnchor"))!=(a.Tier==4?0:3))throw new Exception("Backdrop count");
    if(a.RowSpacing+a.StandingMinZ-a.SweepMaxZ<a.Margin)throw new Exception("Fall-row clearance");if(a.FrontClearance-a.SweepMaxZ<3)throw new Exception("Fall-front clearance");
   }
   reports.Add(new{a.Name,Triangles=count,a.TriangleCeiling,Passive=true,Anchors=a.Nodes.Length});
  }
  File.WriteAllText(Doc+"unity-validation.json",JsonConvert.SerializeObject(new{Status="PASS",Assemblies=reports,Scope="Art only; runtime UI/travel/unlock/chopping/device tests remain with Claude"},Formatting.Indented));return "PASS: seven passive prefabs, readable meshes, names, budgets and zone/door contracts";
 }
 public static string Capture() {
  foreach(var a in Load().Assemblies) {var src=AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(a.Name));Render(src,Doc+a.Name+"-unity-front.png",new Vector3(0,.3f,4));Render(src,Doc+a.Name+"-unity-study.png",new Vector3(2.7f,2.2f,4));}
  var counter=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PathFor("LumberCounter")));
  try{CounterView(counter,Doc+"counter-door-closed.png");counter.transform.Find("DoorPivot").localRotation=Quaternion.Euler(0,100,0);CounterView(counter,Doc+"counter-door-open.png");
   var position=new Vector3(.55f,1.85f,1.45f);FixedCamera(counter,Doc+"ledger-close.png",position,Quaternion.LookRotation(new Vector3(.55f,1.07f,.48f)-position),32);
  }finally{Object.DestroyImmediate(counter);}
  var front=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PathFor("LumberFront")));
  try{var position=new Vector3(-2.17f,2.84f,1.9f);FixedCamera(front,Doc+"sign-close.png",position,Quaternion.LookRotation(new Vector3(-2.17f,2.77f,.35f)-position),32);}finally{Object.DestroyImmediate(front);}
  var street=new GameObject("ReviewOnlyStreet");
  try {
   var tailor=Object.Instantiate(Resources.Load<GameObject>("Settlement/Storefronts/TailorFront"),street.transform);tailor.transform.localPosition=new Vector3(8.0f,0,0);
   Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PathFor("LumberFront")),street.transform);
   var plot=Object.Instantiate(Resources.Load<GameObject>("Settlement/Houses/HouseL01Front"),street.transform);
   if(plot!=null){plot.name="ReviewOnlyPlot3";plot.transform.localPosition=new Vector3(-9,0,0);}Render(street,Doc+"street-tailor-lumber-plot3.png",new Vector3(0,1.4f,6));
  }finally{Object.DestroyImmediate(street);}
  foreach(var a in Load().Assemblies.Where(x=>x.Tier>0)) {
   var zone=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(a.Name)));
   try {
    for(int i=1;i<=6;i++)ReviewTree(zone.transform.Find("TreeAnchor"+i),a.Species,a.VariantSequence[i-1]);
    if(a.Tier<4)for(int i=1;i<=3;i++)ReviewTree(zone.transform.Find("BackdropAnchor"+i),new[]{"Pine","Birch","Mesquite","Oak"}[a.Tier],new[]{"A","B","C"}[i-1]);
    var cam=zone.transform.Find("ZoneCameraAnchor");FixedCamera(zone,Doc+a.Name+"-with-trees.png",cam.position,cam.rotation,55);
   }finally{Object.DestroyImmediate(zone);}
  }
  return "CAPTURED passive views, door states, street and four temporary tree-filled zones";
 }
 static void ReviewTree(Transform anchor,string species,string variant) {var src=Resources.Load<GameObject>("Settlement/LumberField/Massive"+species+variant);if(src==null)throw new Exception("Missing existing massive tree");var obj=Object.Instantiate(src,anchor,false);foreach(var c in obj.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);foreach(var c in obj.GetComponentsInChildren<MonoBehaviour>(true))Object.DestroyImmediate(c);}
 static void CounterView(GameObject source,string path){var t=source.transform.Find("CustomerCameraAnchor");FixedCamera(source,path,t.position,t.rotation,70);}
 static void FixedCamera(GameObject source,string path,Vector3 position,Quaternion rotation,float fov) {var preview=new PreviewRenderUtility();try{preview.AddSingleGO(Object.Instantiate(source));var cam=preview.camera;cam.orthographic=false;cam.transform.SetPositionAndRotation(position,rotation);cam.fieldOfView=fov;SavePreview(preview,path,1280,800);}finally{preview.Cleanup();}}
 static void Render(GameObject source,string path,Vector3 direction) {
  var preview=new PreviewRenderUtility();try {
   var view=Object.Instantiate(source);preview.AddSingleGO(view);var rs=view.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);var camera=preview.camera;camera.orthographic=true;camera.transform.position=b.center+direction.normalized*Mathf.Max(12,b.size.magnitude);camera.transform.LookAt(b.center);
   float x=0,y=0;for(int i=0;i<8;i++){var q=camera.transform.InverseTransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));x=Mathf.Max(x,Mathf.Abs(q.x));y=Mathf.Max(y,Mathf.Abs(q.y));}camera.orthographicSize=Mathf.Max(.1f,Mathf.Max(x,y)*1.15f);SavePreview(preview,path,900,900);
  }finally{preview.Cleanup();}
 }
 static void SavePreview(PreviewRenderUtility p,string path,int w,int h) {
  p.camera.nearClipPlane=.01f;p.camera.farClipPlane=1000;p.camera.clearFlags=CameraClearFlags.SolidColor;p.camera.backgroundColor=new Color(.23f,.28f,.31f);p.lights[0].intensity=1.15f;p.lights[0].transform.rotation=Quaternion.Euler(35,20,0);p.lights[1].intensity=.75f;p.ambientColor=new Color(.55f,.55f,.55f);p.BeginStaticPreview(new Rect(0,0,w,h));p.Render(true);var image=p.EndStaticPreview();try{File.WriteAllBytes(path,image.EncodeToPNG());}finally{Object.DestroyImmediate(image);}
 }
}
}
