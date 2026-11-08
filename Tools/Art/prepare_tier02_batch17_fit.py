"""Derive both-body Taiga garment fit studies from the current garment reviewer."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2];p=ROOT/'Assets/_Game/Editor'
code=(p/'Tier02Batch14FitAssets.cs').read_text().replace('Tier02Batch14FitAssets','Tier02Batch17FitAssets').replace('Batch14/','Batch17/').replace('Build batch 14 first','Build batch 17 first')
for old,new in [('WearFlaxWorkShirt','WearTaigaWaxedShirt'),('WearFlaxTrousers','WearTaigaTrousers'),('WearMarshBoots','WearTaigaBoots'),('WearBeaverFurCap','WearTaigaFurHat')]:code=code.replace(old,new)
# The full sleeve must follow the same shoulder rotation as the existing skin study.
# Blend only in the shoulder transition; every lower sleeve vertex is outside x=.20.
code=code.replace('else amount=p.y>1.08f ? Mathf.Clamp01((Mathf.Abs(p.x)-.135f)/.08f) : 0;',
                  'else amount=Mathf.Clamp01((Mathf.Abs(p.x)-.135f)/.08f);')
(p/'Tier02Batch17FitAssets.cs').write_text(code)
code=(ROOT/'Tools/Art/compose_tier02_batch14_fit_reviews.ps1').read_text().replace('Batch14','Batch17')
(ROOT/'Tools/Art/compose_tier02_batch17_fit_reviews.ps1').write_text(code)
print('Prepared batch17 both-body static garment reviewer/compositor.')
