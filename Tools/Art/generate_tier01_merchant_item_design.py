"""T1 merchant surface palettes only; never generates or modifies source geometry.

Run this file, then invoke Tier01MerchantItemDesignAssets.Build()/Capture() in
Unity via the parent orchestrator. No Unity process is started here.
"""
from pathlib import Path
import hashlib
import json

import detail_textures as detail
from generate_balloon_goods import PALETTES

ROOT = Path(__file__).resolve().parents[2]
SOURCE = 'Assets/_Game/Art/Items/BalloonGoods/'
OUT = 'Assets/_Game/Art/Items/Tier01Design/'
TARGET = 'Assets/_Game/Resources/BalloonGoods/'
MATERIALS = 'Assets/_Game/Resources/Tier01/MerchantDesignMaterials/'
MANIFEST = OUT + 'MerchantItemDesignManifest.json'
DOCS = 'Docs/Art/Tier01/MerchantItemDesign/'
SLOT_NAMES = (
    'maincloth', 'secondarycloth', 'leather', 'darkleather', 'crimsoncloth',
    'darkcrimsoncloth', 'brassmetal', 'brasshighlight', 'liningcloth',
    'leathersoles', 'shell', 'framewood', 'darkframewood', 'feather', 'white', 'dark',
)
SLOT_TYPES = (
    'cloth', 'cloth', 'leather', 'leather', 'cloth', 'cloth', 'metal', 'metal',
    'cloth', 'leather', 'flat', 'wood', 'wood', 'plant_fibre', 'flat', 'flat',
)


def palette(profile, box=False):
    colors = list(PALETTES[0])
    semantics = list(SLOT_NAMES)
    kinds = list(SLOT_TYPES)
    if box:
        # Exact original generate_balloon_goods.mystery() palette override.
        colors[0] = (112, 134, 64)
        semantics[0] = 'oliveboxlacquer'
        semantics[4] = 'crimsonboxlacquer'
        semantics[5] = 'darkcrimsonlining'
        kinds[0] = kinds[4] = 'flat'  # Lacquer stays smooth, never woven cloth.
        kinds[5] = 'cloth'
    path = OUT + 'Palettes/' + profile + 'Palette.png'
    row = [(*rgb, detail.alpha(kind))
           for rgb, kind in zip(colors, kinds) for _ in range(4)]
    detail.write_png(ROOT/path, 64, 4, [row]*4)
    return dict(Name=profile, Path=path, MaterialPath=MATERIALS+profile+'.mat',
                DetailScale=7 if not box else 5, DetailStrength=.45 if not box else .38,
                Slots=[dict(Index=i, Semantic=semantic, DetailType=kind, RGB=list(rgb))
                       for i, (semantic, kind, rgb) in enumerate(zip(semantics, kinds, colors))])


def main():
    data = json.loads((ROOT/(SOURCE+'BalloonGoodsManifest.json')).read_text(encoding='utf-8'))
    items = [row for row in data['Items'] if row['Tier'] == 1]
    if len(items) != 5 or {r['Slot'] for r in items} != {'hat', 'boots', 'shirt', 'pants', 'pack'}:
        raise ValueError('Expected the five existing T1 merchant items.')
    profiles, rows = [], []
    for item in items:
        profile = 'Merchant'+item['Slot'].title()
        profiles.append(palette(profile))
        for field, role, folder in (
                ('ItemMesh', 'Display', 'Items/'),
                ('MaleMesh', 'MaleWorn', 'Wearables/Male/'),
                ('FemaleMesh', 'FemaleWorn', 'Wearables/Female/')):
            mesh = item[field]
            if not mesh.startswith('SpecialT01'):
                raise ValueError('Refusing non-T1 merchant mesh: '+mesh)
            source = SOURCE+mesh+'.obj'
            prefab = TARGET+folder+mesh+'.prefab'
            if not (ROOT/prefab).is_file():
                raise FileNotFoundError(prefab)
            rows.append(dict(Id=item['Id'], Name=item['Name'], Model=mesh, Role=role,
                             PrefabPath=prefab, PaletteFamily=profile,
                             SourcePaths=[source],
                             SourceSHA256=[hashlib.sha256((ROOT/source).read_bytes()).hexdigest()]))
    profiles.append(palette('MerchantMysteryBox', box=True))
    sources = [SOURCE+name+'.obj' for name in ('MysteryBoxBody', 'MysteryBoxLid', 'MysteryTier1Seal')]
    box_prefab = TARGET+'MysteryBoxes/MysteryBoxTier01.prefab'
    if not (ROOT/box_prefab).is_file():
        raise FileNotFoundError(box_prefab)
    rows.append(dict(Id='MysteryBoxTier01', Name='Tier-one mystery box — olive and brass',
                     Model='MysteryBoxTier01', Role='Box', PrefabPath=box_prefab,
                     PaletteFamily='MerchantMysteryBox', SourcePaths=sources,
                     SourceSHA256=[hashlib.sha256((ROOT/p).read_bytes()).hexdigest() for p in sources]))
    result = dict(Revision=1, Scope='T1 merchant surface assignment only', Items=rows,
                  PaletteFamilies=profiles,
                  FitStatus='Ordinary T1 hats/pack fits are handled elsewhere. Special worn geometry was not refitted; old special-fit geometry remains for a later refit.',
                  GeometryPolicy='Existing source geometry, UVs, prefab hierarchy, transforms, hinges and animation are preserved.')
    (ROOT/MANIFEST).write_text(json.dumps(result, indent=2, ensure_ascii=False)+'\n', encoding='utf-8')
    print(f'Wrote {len(profiles)} isolated 16-slot palettes and {MANIFEST}; {len(rows)} existing prefab targets. No Unity calls.')


if __name__ == '__main__':
    main()
