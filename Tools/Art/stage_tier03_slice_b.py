"""Tier 3 integration slice B: stage the refined tier 3 balloon special gear (MERCHANT01-03).

For special.t3.{hat,boots,shirt,pants,pack}: the display model from the FINAL02 ledger and the male/female worn fits from its
WornSources, copied unchanged (hash-checked) from Docs/Art/Tier03 into Assets/_Game/Art/Tier03/Special/. Writes
Tier03SliceBManifest.json (Role Display/MaleWorn/FemaleWorn) and regenerates Tier3Visuals.cs. The original SpecialT03*
balloon models stay in place, unused (user, November 24, 2026). Standard library only; run from the repository root.
"""
from pathlib import Path
import hashlib, json, shutil, sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
from tier03_visuals import write_visuals  # noqa: E402

ROOT = Path(__file__).resolve().parents[2]
LEDGER = ROOT / 'Docs/Art/Tier03/BatchFINAL02/coverage-audit.json'
PRESERVED = ROOT / 'Docs/Art/Tier03/BatchFINAL02/source-preservation.json'
FOLDER = ROOT / 'Assets/_Game/Art/Tier03/Special'
MANIFEST = ROOT / 'Assets/_Game/Art/Tier03/Tier03SliceBManifest.json'
IDS = [f'special.t3.{p}' for p in ('hat', 'boots', 'shirt', 'pants', 'pack')]
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()


def stage(source, palette, model, role, item_id, packet, triangles=None):
    preserved = json.loads(PRESERVED.read_text(encoding='utf-8'))
    for p in (source, palette):
        rel = p.relative_to(ROOT).as_posix()
        if rel in preserved and preserved[rel] != sha(p):
            raise SystemExit('Source changed since FINAL02: ' + rel)
    meta = next((x for x in json.loads((ROOT / 'Docs/Art/Tier03' / packet / 'manifest.json').read_text(encoding='utf-8'))['Items']
                 if x.get('Model') == model or x.get('Name') == model), None)
    if meta is None:
        raise SystemExit('No packet manifest row for ' + model)
    FOLDER.mkdir(parents=True, exist_ok=True)
    targets = []
    for p in (source, palette):
        t = FOLDER / p.name
        if not t.exists() or sha(t) != sha(p):
            shutil.copyfile(p, t)
        targets.append(t)
    tris = triangles if triangles is not None else int(meta['Triangles'])
    return dict(Name=model, Model=model, Role=role, SourcePath=targets[0].relative_to(ROOT).as_posix(),
                PalettePath=targets[1].relative_to(ROOT).as_posix(), PrefabPath='Tier03/Special/' + model, ItemIds=[item_id],
                Triangles=tris, Readable=bool(meta.get('RequiresReadableMeshOnImport', True)),
                OriginSource=source.relative_to(ROOT).as_posix(), OriginPalette=palette.relative_to(ROOT).as_posix(),
                Packet=packet, SourceSHA256=sha(source), PaletteSHA256=sha(palette))


def main():
    audit = json.loads(LEDGER.read_text(encoding='utf-8'))
    displays = {r['ItemId']: r for r in audit['Displays']}
    rows = []
    for item_id in IDS:
        d = displays[item_id]
        rows.append(stage(ROOT / d['Source'], ROOT / d['Palette'], d['Model'], 'Display', item_id, d['Packet'], int(d['Triangles'])))
        worn = [w for w in audit['WornSources'] if w['ItemId'] == item_id]
        if sorted(w['Body'] for w in worn) != ['Female', 'Male']:
            raise SystemExit('Expected one male and one female fit for ' + item_id)
        for w in worn:
            source = ROOT / w['Source']
            rows.append(stage(source, source.with_name(source.stem + 'Palette.png'), w['Model'], w['Body'] + 'Worn', item_id, w['Packet']))
    MANIFEST.write_text(json.dumps(dict(Slice='Tier 3 integration slice B', Items=rows), indent=2) + '\n', encoding='utf-8')
    count = write_visuals()
    print(f'Staged {len(rows)} special gear models; Tier3Visuals has {count} item IDs')


if __name__ == '__main__':
    sys.exit(main())
