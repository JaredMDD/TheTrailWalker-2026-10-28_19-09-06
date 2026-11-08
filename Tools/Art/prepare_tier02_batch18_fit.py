"""Prepare both-body Taiga pack/strap/hair clearance studies."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];p=ROOT/'Assets/_Game/Editor'
code=(p/'Tier02Batch15FitAssets.cs').read_text().replace('Tier02Batch15FitAssets','Tier02Batch18FitAssets').replace('Batch15/','Batch18/').replace('Build batch 15 first','Build batch 18 first')
for old,new in [('WearFlaxWorkShirt','WearTaigaWaxedShirt'),('WearFlaxTrousers','WearTaigaTrousers'),('WearMarshBoots','WearTaigaBoots'),('WearBeaverFurCap','WearTaigaFurHat'),('WearMarshPack','WearTaigaFramePack')]:code=code.replace(old,new)
code=code.replace('else amount=p.y>1.08f ? Mathf.Clamp01((Mathf.Abs(p.x)-.135f)/.08f) : 0;','else amount=Mathf.Clamp01((Mathf.Abs(p.x)-.135f)/.08f);')
(p/'Tier02Batch18FitAssets.cs').write_text(code)
code=(ROOT/'Tools/Art/compose_tier02_batch15_fit_reviews.ps1').read_text().replace('Batch15','Batch18')
(ROOT/'Tools/Art/compose_tier02_batch18_fit_reviews.ps1').write_text(code)
print('Prepared batch18 pack reviewer/compositor.')
