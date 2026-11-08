"""Collects every fitted garment, any tier, into one normalised list (equipment redesign, November 7, 2026).

Sources (each pipeline keeps its own format; this file is the only place that knows them):
  tier-one shells       PlayerDefault/PlayerAvatarManifest.json "wearables"  -> PlayerDefault/Unity/Wearables prefabs
  tier-one accessories  Resources/Characters/Tier01Accessories/Tier01AccessoriesArtMap.json (hat, backpack, hat hair)
  tier-two garments     Art/Crafting/Tier02/Tier02Batch*ArtMap.json (worn entries; both of the other tool's schemas)
  balloon special gear  Art/Items/BalloonGoods/BalloonGoodsManifest.json
Writes Assets/_Game/Art/Characters/WearableArtMap.json (asset paths). Unity: The Trail Walker > Art > Build Wearable
Art Catalog turns it into Resources/Characters/WearableArtCatalog.asset, which the equipment avatar reads.
Run: python Tools/Art/build_wearable_art_map.py
"""
from pathlib import Path
import glob
import json

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Characters/WearableArtMap.json'
SLOTS = ['Hat', 'Boots', 'Shirt', 'Pants', 'Backpack']


def slot_of(item_id):
    low = item_id.lower()
    for words, slot in ((('hat', 'cap', 'hood'), 'Hat'), (('boots', 'boot'), 'Boots'), (('shirt', 'robe', 'coat'), 'Shirt'),
                        (('trousers', 'pants'), 'Pants'), (('pack', 'backpack'), 'Backpack')):
        if any(w in low.replace('.', '_').split('_') or low.endswith(w) for w in words): return slot
    raise ValueError('Cannot tell the slot of ' + item_id)


def anchor_of(text):
    text = text or ''
    if 'HatAnchor' in text or 'HeadAnchor' in text: return 'HeadAnchor/HatAnchor'
    if 'BackpackAnchor' in text: return 'BackpackAnchor'
    return 'Root'


def resource_prefab(path): return 'Assets/_Game/Resources/' + path + '.prefab'


def main():
    entries = {}
    def add(item_id, slot, anchor, body, prefab, source):
        if not (ROOT/prefab).exists(): raise FileNotFoundError(prefab)
        e = entries.setdefault(item_id, dict(itemId=item_id, slot=slot, anchor=anchor, male='', female='', source=source))
        if (e['slot'], e['anchor']) != (slot, anchor): raise ValueError('Conflicting entries for ' + item_id)
        e['female' if body == 'Female' else 'male'] = prefab

    # Tier-one shells (shirt, pants, boots): body-root.
    avatar = json.loads((ROOT/'Assets/_Game/Art/Characters/PlayerDefault/PlayerAvatarManifest.json').read_text(encoding='utf-8'))
    for w in avatar['wearables']:
        for body in ('Male', 'Female'):
            add(w['itemId'], slot_of(w['itemId']), 'Root', body,
                f"Assets/_Game/Art/Characters/PlayerDefault/Unity/Wearables/{body}Wear{w['part']}.prefab", 'tier-one shells')
    # Tier-one accessories (hat, backpack) and the shared hat-compatible hair.
    acc = json.loads((ROOT/'Assets/_Game/Resources/Characters/Tier01Accessories/Tier01AccessoriesArtMap.json').read_text(encoding='utf-8'))
    for item in acc['items']:
        for body in ('Male', 'Female'):
            add(item['itemId'], slot_of(item['itemId']), anchor_of(item['anchor']), body, resource_prefab(item['worn'][body]), 'tier-one accessories')
    hair = {body: [resource_prefab(acc['compatibleHair'][body][s]) for s in ('Crop', 'Sweep', 'Bob', 'Tied')] for body in ('Male', 'Female')}
    # Tier-two (and later) garments from the batch art maps.
    for f in sorted(glob.glob(str(ROOT/'Assets/_Game/Art/Crafting/Tier0*/Tier0*Batch*ArtMap.json'))):
        data = json.loads(Path(f).read_text(encoding='utf-8'))
        items = data.get('Items', {})
        rows = items.items() if isinstance(items, dict) else ((i.get('Model'), i) for i in items)
        tier_folder = Path(f).parent.name
        for name, v in rows:
            if v.get('Representation') != 'worn': continue
            prefab = resource_prefab(v['PrefabPath']) if v.get('PrefabPath') else f'Assets/_Game/Resources/{tier_folder}/Wearables/{name}.prefab'
            add(v['ItemId'], slot_of(v['ItemId']), anchor_of(v.get('Anchor')), v['Body'], prefab, Path(f).name)
    # Balloon special gear.
    balloon = json.loads((ROOT/'Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsManifest.json').read_text(encoding='utf-8'))
    for item in balloon['Items']:
        slot = {'hat': 'Hat', 'boots': 'Boots', 'shirt': 'Shirt', 'pants': 'Pants', 'pack': 'Backpack'}[item['Slot']]
        for body, mesh in (('Male', item['MaleMesh']), ('Female', item['FemaleMesh'])):
            add(item['Id'], slot, anchor_of(item['Anchor']), body, f'Assets/_Game/Resources/BalloonGoods/Wearables/{body}/{mesh}.prefab', 'balloon special gear')

    missing = [e['itemId'] for e in entries.values() if not e['male'] or not e['female']]
    if missing: raise ValueError('Missing a body fit: ' + ', '.join(missing))
    out = dict(revision=1, scope='Fitted garment art map (asset paths); built into Resources/Characters/WearableArtCatalog.asset.',
               slots=SLOTS, maleHatHair=hair['Male'], femaleHatHair=hair['Female'], items=list(entries.values()))
    OUT.write_text(json.dumps(out, indent=2)+'\n', encoding='utf-8')
    counts = {}
    for e in entries.values(): counts[e['source']] = counts.get(e['source'], 0)+1
    print(len(entries), 'fitted garments:', ', '.join(f'{v} {k}' for k, v in counts.items()))


if __name__ == '__main__':
    main()
