"""Record manually reviewed static fit evidence, with PNG and source checks."""
import argparse,hashlib,json,struct
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--batch',type=int,required=True);p.add_argument('--views',type=int,required=True)
    a=p.parse_args();code=f'{a.batch:02}';doc=ROOT/f'Docs/Art/Tier02/Batch{code}'
    files=list(doc.glob('*-fit-*.png'))+list(doc.glob('*-hat-*.png'))+list(doc.glob('*-boot-close-*.png'))+list(doc.glob('*-pack-close-*.png'))+list(doc.glob('*-pack-hair-*.png'))
    # Close garment studies use shirt/pants naming in batch 14.
    files+=list(doc.glob('*-shirt-close-*.png'))+list(doc.glob('*-pants-close-*.png'))
    files=[f for f in files if not f.name.endswith('-review.png')]
    assert len(set(files))==a.views,(len(set(files)),a.views)
    for f in files:
        b=f.read_bytes();assert b[:8]==b'\x89PNG\r\n\x1a\n' and struct.unpack('>II',b[16:24])==(800,900),f
    sources=[f'Tools/Art/generate_tier02_batch{code}.py',f'Assets/_Game/Editor/Tier02Batch{code}FitAssets.cs']
    data={'Scope':'Static body/garment fit studies only; dynamic binding and animation unverified','Bodies':['Male','Female'],
          'FitViews':a.views,'VisualReview':'All final fit boards manually reviewed; see review-notes.md',
          'AnimatedAttachmentMetrics':'Unverified; no skin weights or runtime animation authored',
          'SourceSHA256':{s:hashlib.sha256((ROOT/s).read_bytes()).hexdigest() for s in sources}}
    (doc/'fit-review-status.json').write_text(json.dumps(data,indent=2)+'\n')
    print(f'Batch {code}: {a.views} static fit PNGs checked and source hashes recorded.')
