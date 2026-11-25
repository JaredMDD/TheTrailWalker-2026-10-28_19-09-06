"""Tier 3 integration slice A: stage item display art for Resources import.

Reads the slice A audit and the FINAL02 ledger, copies each selected export (OBJ + palette PNG) unchanged from
Docs/Art/Tier03 into Assets/_Game/Art/Tier03/<Family>/, writes the import manifest and generates
Assets/_Game/Scripts/Unity/Tier3Visuals.cs. Standard library only; run from the repository root.
"""
from pathlib import Path
import hashlib, json, shutil, sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
from tier03_visuals import write_visuals  # noqa: E402

ROOT = Path(__file__).resolve().parents[2]
AUDIT = ROOT / 'Docs/Art/Tier03/Integration/slice-a-audit.json'
LEDGER = ROOT / 'Docs/Art/Tier03/BatchFINAL02/coverage-audit.json'
PRESERVED = ROOT / 'Docs/Art/Tier03/BatchFINAL02/source-preservation.json'
ART = ROOT / 'Assets/_Game/Art/Tier03'
MANIFEST = ART / 'Tier03SliceAManifest.json'
FAMILIES = {'RAW': 'Raw', 'MAT': 'Materials', 'FOOD': 'Food', 'PREMIUM': 'Premium', 'TOOL': 'Tools'}
# Runtime IDs that use a ledger row under another planning ID (audit decisions, user November 24, 2026).
ALIASES = {'animal.lamb.meat': 'ranch.lamb_meat'}
for family, word in [('axe', 'axe'), ('hunting', 'bow'), ('rod', 'rod')]:
    for grade in ['normal', 'elite']:
        ALIASES[f'debug.tool.{family}.t3.{grade}'] = f'item.mesquite_{word}_{grade}'

sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()


def family_of(packet):
    for prefix, name in FAMILIES.items():
        if packet.startswith('Batch' + prefix):
            return name
    raise SystemExit('Unexpected packet ' + packet)


def main():
    audit = json.loads(AUDIT.read_text(encoding='utf-8'))['Rows']
    ledger = {r['ItemId']: r for r in json.loads(LEDGER.read_text(encoding='utf-8'))['Displays']}
    preserved = json.loads(PRESERVED.read_text(encoding='utf-8'))
    runtime = {r['ItemId'] for r in audit if r['Runtime']}

    bindings = {}  # runtime item id -> ledger row
    for r in audit:
        if r['Runtime'] and r['InLedger'] and not r['Model']:
            bindings[r['ItemId']] = ledger[r['ItemId']]
    for item_id, ledger_id in ALIASES.items():
        if item_id not in runtime:
            raise SystemExit('Alias target is not a runtime item: ' + item_id)
        bindings[item_id] = ledger[ledger_id]

    rows = {}
    for item_id, l in sorted(bindings.items()):
        model = l['Model']
        if model in rows:
            rows[model]['ItemIds'].append(item_id)
            continue
        family = family_of(l['Packet'])
        source, palette = ROOT / l['Source'], ROOT / l['Palette']
        for p in (source, palette):
            rel = p.relative_to(ROOT).as_posix()
            if rel in preserved and preserved[rel] != sha(p):
                raise SystemExit('Source changed since FINAL02: ' + rel)
        packet_rows = json.loads((ROOT / 'Docs/Art/Tier03' / l['Packet'] / 'manifest.json').read_text(encoding='utf-8'))['Items']
        meta = next((x for x in packet_rows if x.get('Model') == model), None)
        if meta is None:
            raise SystemExit('No packet manifest row for ' + model)
        folder = ART / family
        folder.mkdir(parents=True, exist_ok=True)
        targets = []
        for p in (source, palette):
            target = folder / p.name
            if not target.exists() or sha(target) != sha(p):
                shutil.copyfile(p, target)
            targets.append(target)
        rows[model] = dict(Name=model, Model=model,
                           SourcePath=targets[0].relative_to(ROOT).as_posix(),
                           PalettePath=targets[1].relative_to(ROOT).as_posix(),
                           PrefabPath=f'Tier03/{family}/{model}', ItemIds=[item_id],
                           Triangles=int(l['Triangles']), BoundsMin=meta.get('BoundsMin'), BoundsMax=meta.get('BoundsMax'),
                           Readable=bool(meta.get('RequiresReadableMeshOnImport', True)),
                           OriginSource=l['Source'], OriginPalette=l['Palette'], Packet=l['Packet'],
                           SourceSHA256=sha(source), PaletteSHA256=sha(palette))

    items = sorted(rows.values(), key=lambda r: r['PrefabPath'])
    MANIFEST.write_text(json.dumps(dict(Slice='Tier 3 integration slice A', Items=items), indent=2) + '\n', encoding='utf-8')

    count = write_visuals()
    print(f'Staged {len(items)} models; Tier3Visuals has {count} item IDs; folder {ART.relative_to(ROOT).as_posix()}')


if __name__ == '__main__':
    sys.exit(main())
