"""Prepare static Taiga hat/boot studies on both current bodies."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
p=ROOT/'Assets/_Game/Editor'
code=(p/'Tier02Batch14FitAssets.cs').read_text()
code=code.replace('Tier02Batch14FitAssets','Tier02Batch16FitAssets').replace('Batch14/','Batch16/').replace('Build batch 14 first','Build batch 16 first')
code=code.replace('WearMarshBoots','WearTaigaBoots').replace('WearBeaverFurCap','WearTaigaFurHat')
# Reuse the known cap/boot capture grid, with the newer continuous clothing context.
old=(p/'Tier02Batch13FitAssets.cs').read_text()
start=code.index('                foreach (int yaw in new[] { 0,90,180,35 })')
end=code.index('            }\n            File.WriteAllText',start)
source=old[old.index('                foreach (string style in new[]'):old.index('            }\n            File.WriteAllText')]
source=source.replace('new Vector3(0,1.60f,0),.245f','new Vector3(0,1.61f,0),.29f').replace('new Vector3(0,.14f,.04f),.27f','new Vector3(0,.17f,.04f),.30f')
code=code[:start]+source+code[end:]
code=code.replace('pose == "reach"','pose == "focus"').replace('"stride", "reach"','"stride", "focus"')
code=code.replace('            if (pose == "focus") { PoseReach(skin, body, true); PoseReach(shirt, body, false); }','')
code=code.replace('48 current-body static fit/garment views. Standing/seated/rigid stride/arm reach','64 current-body static hat/hair/footwear views. Standing/seated/rigid stride/head focus')
(p/'Tier02Batch16FitAssets.cs').write_text(code)
c=(ROOT/'Tools/Art/compose_tier02_batch13_fit_reviews.ps1').read_text().replace('Batch13','Batch16')
(ROOT/'Tools/Art/compose_tier02_batch16_fit_reviews.ps1').write_text(c)
print('Prepared batch16 static fit reviewer/compositor.')
