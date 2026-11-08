"""Prepare a read-only current-body Marsh-pack static fit reviewer."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
folder=ROOT/'Assets/_Game/Editor'
code=(folder/'Tier02Batch14FitAssets.cs').read_text()
code=code.replace('Tier02Batch14FitAssets','Tier02Batch15FitAssets').replace('Batch14/','Batch15/').replace('Build batch 14 first','Build batch 15 first')
marker='            var head = Pivot(upper, "HeadAnchor", new Vector3(0, 1.43f, 0));'
assert marker in code
code=code.replace(marker,'''            var packAnchor = Pivot(upper,"BackpackAnchor",new Vector3(0,1.14f,-.20f));
            NewArt(packAnchor,body+"WearMarshPack");
'''+marker,1)
start=code.index('                foreach (int yaw in new[] { 0,90,180,35 })')
end=code.index('            }\n            File.WriteAllText',start)
code=code[:start]+'''                foreach (string pose in new[] { "standing","seated" })
                    foreach (int yaw in new[] {0,90,180,145})
                        Render(body+"-pack-close-"+pose+"-"+yaw+".png",Review(body,"Tied",pose),yaw,
                            new Vector3(0,pose=="seated" ? .80f : 1.18f,-.10f),.44f);
                foreach(string style in new[] {"Crop","Sweep","Bob","Tied"})
                    foreach(int yaw in new[] {90,180})
                        Render(body+"-pack-hair-"+style+"-"+yaw+".png",Review(body,style,"standing"),yaw,new Vector3(0,1.40f,-.10f),.46f);
'''+code[end:]
code=code.replace('48 current-body static fit/garment views','64 current-body static pack/strap/clearance views')
(folder/'Tier02Batch15FitAssets.cs').write_text(code)
print('Prepared batch 15 body/strap/hair-clearance reviewer.')
