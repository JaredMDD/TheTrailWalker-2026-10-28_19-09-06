using System.Collections.Generic;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Movement;
using UnityEngine;

namespace TheTrailWalker.Travel
{
    // Environment art-study assembly helper. Not called by the live route: the ten-camp layout is still unauthored.
    // No collision, harvest identity, rewards or save-state randomness.
    public sealed class Region01Environment : MonoBehaviour
    {
        private Mesh generated;
        private void OnDestroy(){if(generated!=null){if(Application.isPlaying)Destroy(generated);else DestroyImmediate(generated);}}
        public static Material GroundMaterial=>Resources.Load<Material>("Environment/Region01/DawnPlainsPalette");
        public static Material TrailMaterial=>Resources.Load<Material>("Environment/Region01/DawnTrail");

        public static void StyleTerrain(Transform parent)
        {
            Transform terrain=parent.Find("Terrain");Material material=GroundMaterial;
            if(terrain==null||material==null)return;
            var filter=terrain.GetComponent<MeshFilter>();Mesh original=filter.sharedMesh;
            Vector3[] source=original.vertices;int[] indices=original.triangles;
            var vertices=new Vector3[indices.Length];var uv=new Vector2[indices.Length];var triangles=new int[indices.Length];
            for(int i=0;i<indices.Length;i+=3)
            {
                Vector3 centre=(source[indices[i]]+source[indices[i+1]]+source[indices[i+2]])/3;
                // Coherent meadow patches, with restrained faceted color variation rather than a noisy checkerboard.
                float noise=Mathf.PerlinNoise(centre.x*.045f+19,centre.z*.045f+41);
                int color=noise<.37f?4:noise>.66f?6:5;
                for(int j=0;j<3;j++){vertices[i+j]=source[indices[i+j]];triangles[i+j]=i+j;uv[i+j]=new Vector2((color+.5f)/24,.5f);}
            }
            var mesh=new Mesh{name="Dawn Plains faceted meadow",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,vertices=vertices,triangles=triangles,uv=uv};
            mesh.RecalculateNormals();mesh.RecalculateBounds();filter.sharedMesh=mesh;
            terrain.GetComponent<MeshRenderer>().sharedMaterial=material;
            terrain.gameObject.AddComponent<Region01Environment>().generated=mesh;
            if(Application.isPlaying)Destroy(original);else DestroyImmediate(original); // Transient study grid only.
        }

        // groundHeight (optional) places each piece on the actual terrain, for sloped segments; without it the flat baseline is used.
        public static void BuildStudy(Transform parent,ITrailPath path,SegmentDefinition segment,IReadOnlyList<InteractableAnchor> anchors,System.Func<Vector3,float> groundHeight=null,float spacing=18)
        {
            Material material=GroundMaterial;if(material==null)return;
            var root=new GameObject("Dawn Plains decorative scenery");root.transform.SetParent(parent,false);
            var instances=new List<CombineInstance>();var samples=new List<Vector3>();
            for(float d=0;d<path.Length;d+=2){path.Evaluate(d,out Vector3 p,out _);samples.Add(p);}
            var reservations=new List<Vector3>();
            foreach(var anchor in anchors)
            {
                path.Evaluate(anchor.Distance,out Vector3 p,out Vector3 t);
                reservations.Add(p+Vector3.Cross(Vector3.up,t).normalized*anchor.Lateral);
            }
            uint seed=2166136261;foreach(char c in segment.Id){seed^=c;seed*=16777619;}
            var random=new System.Random((int)(seed&0x7fffffff));
            var prefabs=new Dictionary<string,GameObject>();
            for(float d=25;d<path.Length-25;d+=spacing)
            {
                path.Evaluate(d,out Vector3 p,out Vector3 t);Vector3 side=Vector3.Cross(Vector3.up,t).normalized;
                for(int sign=-1;sign<=1;sign+=2)
                {
                    Place(random.Next(3)==0?"MeadowBushTall":"MeadowBushLow",p+side*sign*(7+(float)random.NextDouble()*6),.75f+(float)random.NextDouble()*.45f,5);
                    Place(random.Next(7)==0?"WildflowerClump":"GrassTuft",p+side*sign*(3.8f+(float)random.NextDouble()*1.3f),1,3);
                    if(random.Next(3)==0)Place(new[]{"MeadowPine","YoungPine","WindPine"}[random.Next(3)],p+side*sign*(19+(float)random.NextDouble()*12),.8f+(float)random.NextDouble()*.35f,11);
                    if(random.Next(8)==0)Place("GroundMound",p+side*sign*24,1,13);
                    if(random.Next(9)==0)Place("MeadowBoulder",p+side*sign*15,.8f,8);
                    if(random.Next(8)==0)Place("DryShrub",p+side*sign*13,.8f,6);
                }
            }
            if(instances.Count>0)
            {
                var mesh=new Mesh{name="Dawn Plains combined scenery",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
                mesh.CombineMeshes(instances.ToArray(),true,true);mesh.RecalculateBounds();
                root.AddComponent<MeshFilter>().sharedMesh=mesh;root.AddComponent<MeshRenderer>().sharedMaterial=material;
                root.AddComponent<Region01Environment>().generated=mesh;
            }
            void Place(string name,Vector3 position,float scale,float roadClearance)
            {
                // Test against the whole curved road, not just the local tangent; keep adjacent bends clear too.
                foreach(Vector3 point in samples)if(FlatDistance(position,point)<roadClearance)return;
                foreach(Vector3 point in reservations)if(FlatDistance(position,point)<5)return;
                // Endpoint clearings, traders, and ferry approaches remain undecorated.
                path.Evaluate(0,out Vector3 start,out _);path.Evaluate(path.Length,out Vector3 end,out _);
                if(FlatDistance(position,start)<22||FlatDistance(position,end)<22)return;
                if(!prefabs.TryGetValue(name,out GameObject prefab)){prefab=Resources.Load<GameObject>("Environment/Region01/"+name);prefabs[name]=prefab;}
                if(prefab==null)return;
                // Region 1 is level: all scenery shares the ground grid's authored -0.12 metre baseline.
                position.y=groundHeight!=null?groundHeight(position):position.y-.12f;
                Matrix4x4 placement=Matrix4x4.TRS(position,Quaternion.Euler(0,(float)random.NextDouble()*360,0),Vector3.one*scale);
                foreach(MeshFilter filter in prefab.GetComponentsInChildren<MeshFilter>())
                    instances.Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.transform.worldToLocalMatrix*placement*filter.transform.localToWorldMatrix});
            }
        }
        private static float FlatDistance(Vector3 a,Vector3 b)=>Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z));
    }
}
