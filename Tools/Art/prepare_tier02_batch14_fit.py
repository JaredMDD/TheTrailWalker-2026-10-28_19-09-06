"""Derive batch 14's static pose reviewer from the accepted current-body reviewer."""
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
path=ROOT/'Assets/_Game/Editor'
code=(path/'Tier02Batch13FitAssets.cs').read_text()
code=code.replace('Tier02Batch13FitAssets','Tier02Batch14FitAssets').replace('Batch13/','Batch14/').replace('Build batch 13 first','Build batch 14 first')
start=code.index('            // Existing upper study');end=code.index('            var boots =',start)
code=code[:start]+'''            var skin = Source(upper, body, "SeatedUpperSkinStudy", true);
            var shirt = NewArt(upper, body + "WearFlaxWorkShirt");
            var pants = NewArt(root.transform, body + "WearFlaxTrousers");
            if (seated) PoseSeated(pants);
            if (pose == "reach") { PoseReach(skin, body, true); PoseReach(shirt, body, false); }
'''+code[end:]
code=code.replace('pose == "focus"','pose == "reach"')
marker='        static GameObject Review('
methods='''        static void PoseSeated(GameObject root)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var clone = Object.Instantiate(filter.sharedMesh); OwnedMeshes.Add(clone);
                var points = clone.vertices;
                for (int i=0;i<points.Length;i++)
                {
                    var p=points[i];
                    var hip=new Vector3(p.x < 0 ? -.10f : .10f,.88f,0);
                    float blend=Mathf.Clamp01((.84f-p.y)/.08f);
                    var thigh=hip+Quaternion.AngleAxis(-90*blend,Vector3.right)*(p-hip)+new Vector3(0,-.38f,0);
                    var shin=new Vector3(p.x,p.y,p.z+.38f);
                    points[i]=Vector3.Lerp(shin,thigh,Mathf.Clamp01((p.y-.48f)/.04f));
                }
                clone.vertices=points;clone.RecalculateNormals();clone.RecalculateBounds();filter.sharedMesh=clone;
            }
        }

        static void PoseReach(GameObject root, string body, bool skin)
        {
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var clone=Object.Instantiate(filter.sharedMesh);OwnedMeshes.Add(clone);var points=clone.vertices;
                for(int i=0;i<points.Length;i++)
                {
                    var p=points[i];float amount;
                    if(skin) amount=Mathf.Abs(p.x)>.13f ? 1 : 0;
                    else amount=p.y>1.08f ? Mathf.Clamp01((Mathf.Abs(p.x)-.135f)/.08f) : 0;
                    float sx=body=="Female" ? .188f : .205f, sy=body=="Female" ? 1.340f : 1.355f;
                    var shoulder=new Vector3(p.x<0 ? -sx : sx,sy,0);
                    points[i]=shoulder+Quaternion.AngleAxis(-40*amount,Vector3.right)*(p-shoulder);
                }
                clone.vertices=points;clone.RecalculateNormals();clone.RecalculateBounds();filter.sharedMesh=clone;
            }
        }

'''
code=code.replace(marker,methods+marker,1)
start=code.index('                foreach (string style in new[]');end=code.index('            }\n            File.WriteAllText',start)
code=code[:start]+'''                foreach (int yaw in new[] { 0,90,180,35 })
                {
                    Render(body+"-shirt-close-"+yaw+".png",Review(body,"Tied","standing"),yaw,new Vector3(0,1.18f,0),.40f);
                    Render(body+"-pants-close-"+yaw+".png",Review(body,"Tied","standing"),yaw,new Vector3(0,.62f,0),.57f);
                }
'''+code[end:]
code=code.replace('"standing", "seated", "stride", "focus"','"standing", "seated", "stride", "reach"')
code=code.replace('64 current-body static fit/hair/footwear views. Standing/seated/rigid stride/head focus','48 current-body static fit/garment views. Standing/seated/rigid stride/arm reach')
(path/'Tier02Batch14FitAssets.cs').write_text(code)
print('Prepared batch 14 static fitted-garment reviewer.')
