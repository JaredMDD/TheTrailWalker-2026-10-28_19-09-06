"""Read existing art and design tables; write planning records only, never Unity assets."""
from collections import Counter
from datetime import datetime, timezone
import hashlib
import json
import math
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Docs/Art/Tier02'
PLAN = ROOT / 'PROJECT_RECOMENDATIONS.md'


def relative(path):
    return path.relative_to(ROOT).as_posix()


def read_json(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def prefab_source_reference(mesh_path, prefab_path):
    meta = (ROOT / mesh_path).with_suffix(Path(mesh_path).suffix + '.meta')
    match = re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig'), re.M) if meta.exists() else None
    if not match:
        return 'missing source GUID; review needed'
    return ('source GUID present in prefab YAML' if match.group(1) in (ROOT / prefab_path).read_text(encoding='utf-8-sig')
            else 'source GUID not directly present; review nested references')


def norm(value):
    return re.sub(r'[^a-z0-9]', '', value.lower())


def inspect_obj(path):
    vertices, faces, uv_count = [], [], 0
    objects, materials, smoothing = set(), set(), set()
    for line in path.read_text(encoding='utf-8-sig').splitlines():
        fields = line.split()
        if not fields:
            continue
        if fields[0] == 'v':
            vertices.append(tuple(float(x) for x in fields[1:4]))
        elif fields[0] == 'f':
            # Resolve negative OBJ indices at their authored position.
            faces.append([int(x.split('/')[0]) - 1 if int(x.split('/')[0]) > 0
                          else len(vertices) + int(x.split('/')[0]) for x in fields[1:]])
        elif fields[0] == 'vt':
            uv_count += 1
        elif fields[0] == 'o':
            objects.add(' '.join(fields[1:]))
        elif fields[0] == 'usemtl':
            materials.add(' '.join(fields[1:]))
        elif fields[0] == 's':
            smoothing.add(' '.join(fields[1:]))
    meta = path.with_suffix(path.suffix + '.meta')
    match = re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig'), re.M) if meta.exists() else None
    return {
        'Path': relative(path), 'SHA256': hashlib.sha256(path.read_bytes()).hexdigest(),
        'Vertices': len(vertices), 'Polygons': len(faces),
        'AuthoredTriangulationCount': sum(max(0, len(f) - 2) for f in faces),
        'BoundsMin': [min(v[i] for v in vertices) for i in range(3)] if vertices else None,
        'BoundsMax': [max(v[i] for v in vertices) for i in range(3)] if vertices else None,
        'NonFiniteCoordinates': sum(not all(math.isfinite(x) for x in v) for v in vertices),
        'InvalidVertexIndices': sum(i < 0 or i >= len(vertices) for f in faces for i in f),
        'UVCoordinates': uv_count, 'Objects': sorted(objects),
        'UseMaterialNames': sorted(materials), 'SmoothingDirectives': sorted(smoothing),
        'MetaGuid': match.group(1) if match else None,
        'ValidationLimit': 'Source OBJ inventory only; no evaluated Blender/Unity topology, fit, animation or device QA.',
    }


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    lines = PLAN.read_text(encoding='utf-8-sig').splitlines()
    meshes = [inspect_obj(p) for p in sorted((ROOT / 'Assets/_Game/Art').rglob('*.obj'))]
    by_stem = {}
    for mesh in meshes:
        by_stem.setdefault(norm(Path(mesh['Path']).stem), []).append(mesh['Path'])
    prefabs = {}
    for path in (ROOT / 'Assets/_Game/Resources').rglob('*.prefab'):
        prefabs.setdefault(path.stem, []).append(relative(path))

    materials = {'Birch log', 'Glass', 'Cut quartz', 'Barley flour', 'Flax oil', 'Beeswax',
                 'Glass bottle', 'Birch plank', 'Copper ingot', 'Copper fittings', 'Birch beam',
                 'Sandstone tiles', 'Birch charcoal', 'Birch panel', 'Tin ingot', 'Glass panels',
                 'Beaver leather', 'Cured beaver pelt', 'Fine beaver pelt', 'Fine beaver leather',
                 'Flax cloth', 'Flax cord', 'Flax stuffing', 'Fine flax cloth', 'Cattail cord'}
    misc = {'Beeswax candle', 'Lavender pouch', 'Copper pendant', 'Sandstone owl',
            'Birch beaver sculpture', 'Painted birch bird', 'Beaver toy', 'Fishing charm'}
    wearable_ids = {}
    for i, line in enumerate(lines, 1):
        if line.startswith('| 2 A |') or line.startswith('| 2 B |'):
            cells = [x.strip() for x in line.strip('|').split('|')]
            match = re.match(r'(.+?) \(`([^`]+)`\)', cells[2])
            if match:
                wearable_ids[norm(match[1])] = {'RecipeId': 'recipe.wearable.' + match[2],
                    'OutputId': 'item.' + match[2], 'Set': cells[0][-1], 'Slot': cells[1],
                    'IngredientBrief': cells[3], 'SourceLine': i}
    # Only use explicit IDs where they already exist in the current design.
    explicit_ids = {}
    for i, line in enumerate(lines, 1):
        if not line.startswith('|'):
            continue
        match = re.search(r'`(recipe\.[a-z0-9_.]+)`\s*[^A-Za-z`]*([^|]+)\|', line)
        if match:
            explicit_ids[norm(match[2].strip())] = (match[1], i)
        # Revised food rows put the current bold name before its explicit ID,
        # after a struck-through former name (e.g. Blackberry-glazed beaver).
        revised = re.search(r'\*\*([^*]+)\*\* \(`(recipe\.[a-z0-9_.]+)`\)', line)
        if revised:
            explicit_ids[norm(revised[1])] = (revised[2], i)
    explicit_ids[norm('Birch sap milk drink')] = ('recipe.food.birch_sap_milk_drink',
        next(i for i, s in enumerate(lines, 1) if s.startswith('| 2 / Birch |')))

    targets = []
    for i, line in enumerate(lines, 1):
        if not re.match(r'^\| T2-[GLHE]\d+ \|', line):
            continue
        cells = [x.strip() for x in line.strip('|').split('|')]
        name = re.sub(r' \(2[AB] (?:hat|boots|shirt|pants|backpack)\)', '', cells[3].replace('**', ''))
        category = ('Material' if name in materials else 'Misc' if name in misc else
                    'Tool' if name.startswith(('Normal ', 'Elite ')) else
                    'Wearable' if norm(name) in wearable_ids else 'Food')
        row = {'Name': name, 'Category': category, 'Tier': 2, 'UnlockSource': 'Mission',
               'MissionCode': cells[0], 'MissionRegion': int(cells[1]), 'RequestLabel': cells[2],
               'DependencyNote': cells[4], 'Source': relative(PLAN), 'SourceLine': i, 'MissionSourceLine': i,
               'RecipeId': None, 'OutputId': None, 'ExistingMeshPaths': by_stem.get(norm(name), []),
               'Action': 'Design missing inventory output; resolve canonical IDs before import'}
        if norm(name) in wearable_ids:
            row.update(wearable_ids[norm(name)])
        elif norm(name) in explicit_ids:
            row['RecipeId'] = explicit_ids[norm(name)][0]
        if row['ExistingMeshPaths']:
            row['Action'] = 'Existing source art; inspect its batch handoff and runtime binding separately'
        if name == 'Birch sap milk drink':
            row['OutputId'] = 'item.birch_sap_milk_drink'
        targets.append(row)
    # The existing optional truffle output gained a mission-table row (T2-G22).
    # Count that row once; this changes mission coverage, not the recipe target set.
    truffle_in_table=any(r['Name']=='Creamy truffle-barley pot' for r in targets)
    assert len(targets) == 68+int(truffle_in_table), len(targets)
    assert Counter(r['Category'] for r in targets) == {'Material': 25, 'Tool': 6, 'Wearable': 10, 'Food': 19+int(truffle_in_table), 'Misc': 8}
    mission_target_count=len(targets)
    mission_categories=dict(Counter(r['Category'] for r in targets))

    for slug, name, category, source in [
        ('truffle_barley_pot', 'Creamy truffle-barley pot', 'Food', 'Optional authored side mission; code/order unresolved'),
        ('honey_roast_duck', 'Honey-roast duck', 'Food', 'Balloon mystery box'),
        ('duck_blueberry_rice', 'Duck blueberry rice', 'Food', 'Balloon mystery box'),
        ('quartz_prism_lantern', 'Quartz prism lantern', 'Misc', 'Balloon mystery box'),
        ('tin_quartz_music_box', 'Tin-and-quartz music box', 'Misc', 'Balloon mystery box'),
        ('honey_brioche', 'Honey brioche', 'Food', 'Bakery recipe substitution / prepared stock'),
        ('blackberry_cream_pastry', 'Blackberry cream pastry', 'Food', 'Bakery recipe substitution / prepared stock'),
    ]:
        recipe_id = 'recipe.' + category.lower() + '.' + slug
        assert any(recipe_id in s for s in lines), recipe_id
        already=next((r for r in targets if r['Name']==name),None)
        if already:
            assert slug=='truffle_barley_pot','Unexpected duplicate optional output'
            already['RecipeId']=recipe_id
            continue
        targets.append({'Name': name, 'Category': category, 'Tier': 2, 'UnlockSource': source,
                        'RecipeId': recipe_id, 'OutputId': None, 'ExistingMeshPaths': by_stem.get(norm(name), []),
                        'Action': 'Design missing output; preserve authored recipe; resolve output ID before import',
                        'Source': relative(PLAN), 'SourceLine': next(i for i,s in enumerate(lines,1) if recipe_id in s)})
    exclusive_path = ROOT / 'Assets/_Game/Art/Crafting/BalloonExclusive/BalloonExclusiveManifest.json'
    existing_exclusives = [r for r in read_json(exclusive_path)['Items'] if r['Tier'] == 2]
    for item in existing_exclusives:
        mesh = ROOT / f"Assets/_Game/Art/Crafting/BalloonExclusive/{item['Model']}.obj"
        prefab = ROOT / f"Assets/_Game/Resources/{item['PrefabPath']}.prefab"
        assert mesh.is_file() and prefab.is_file(), item['Name']
        targets.append({'Name': item['Name'], 'Category': item['Category'], 'Tier': 2,
                        'RecipeId': item['RecipeId'], 'OutputId': item['OutputId'],
                        'UnlockSource': 'Balloon mystery box', 'ExistingMeshPaths': [relative(mesh)],
                        'ExistingPrefabPath': relative(prefab), 'Action': 'Reuse existing art; review surfaces/recipe metadata',
                        'PrefabSourceReference': prefab_source_reference(relative(mesh), relative(prefab)),
                        'Source': relative(exclusive_path), 'LegacyArtMetadata': item})
    assert len(targets) == 82
    known = [r['RecipeId'] for r in targets if r.get('RecipeId')]
    assert len(set(known)) == len(known), 'Duplicate explicit recipe ID'

    goods_path = ROOT / 'Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsManifest.json'
    goods = read_json(goods_path)
    special = [r for r in goods['Items'] if r['Tier'] == 2]
    for row in special:
        row['ObservedMeshes'] = {k: by_stem.get(norm(row[k]), []) for k in ['ItemMesh', 'MaleMesh', 'FemaleMesh']}
        row['ObservedPrefabs'] = {k: prefabs.get(row[k], []) for k in ['ItemMesh', 'MaleMesh', 'FemaleMesh']}
        assert all(row['ObservedMeshes'].values()) and all(row['ObservedPrefabs'].values())
        row['PrefabSourceReferences'] = {k: prefab_source_reference(row['ObservedMeshes'][k][0], row['ObservedPrefabs'][k][0])
                                        for k in ['ItemMesh', 'MaleMesh', 'FemaleMesh']}
        row['Action'] = 'Preserve IDs/paths; review semantic materials and current-body fit, not craftable'
    loose = []
    environment_count = 0
    for region in range(7,13):
        path = ROOT / f'Assets/_Game/Art/Items/Region{region:02}/ResourceManifest.json'
        for row in read_json(path)['Models']:
            row = dict(row, Region=region, MeshPath=f'Assets/_Game/Art/Items/Region{region:02}/{row["Name"]}.obj',
                       PrefabPaths=prefabs.get(row['Name'], []), Action='Preserve/reuse; binding/footprint acceptance separate')
            assert (ROOT / row['MeshPath']).is_file() and row['PrefabPaths']
            row['PrefabSourceReference'] = prefab_source_reference(row['MeshPath'], row['PrefabPaths'][0])
            loose.append(row)
        environment_count += len(list((ROOT / f'Assets/_Game/Art/Environment/Tier02/Region{region:02}').glob('*.obj')))

    house = read_json(ROOT / 'Assets/_Game/Art/Settlement/Houses/HouseManifest.json')
    houses = [r for r in house['Houses'] if r['Level'] in (3,4)]
    furniture = []
    for h in houses:
        authored = next(s for s in lines if s.startswith(f'| {h["Level"]} | ') and ' (C)' in s) if h['Level'] == 3 else next(
            s for s in lines if s.startswith('| 4 | ') and ' (P)' in s)
        definitions = re.findall(r'([^,|]+?) \((C|F|S|B|Sh|H|P|A|Rg|Rd|Bg|St)\)', authored)
        assert len(definitions) == 6, definitions
        for (name, effect), proxy in zip(definitions, h['FurnitureSamples']):
            furniture.append({'Name': name.strip(), 'EffectCode': effect, 'HouseLevel': h['Level'],
                              'ProxyName': proxy['Name'], 'Action': 'Design standalone purchasable furniture; study proxy only',
                              'StudyMeshPath': f'Assets/_Game/Art/Settlement/Houses/{h["FurnitureStudy"]}.obj'})
    assert len(furniture) == 12
    harvested = ['Birch log', 'Birch sap', 'Raw beaver meat', 'Common carp', 'Northern pike', 'Channel catfish', 'Walleye']
    store = ['Lavender', 'Crab', 'Duck meat']
    raw_targets = [{'Name': n, 'ExistingMeshPaths': by_stem.get(norm(n), []),
                    'Action': ('Existing source art; check batch import/binding status' if by_stem.get(norm(n))
                               else 'Design raw inventory item; Birch log shares its recipe output')} for n in harvested+store]
    now = datetime.now(timezone.utc).isoformat()
    inventory = {'GeneratedAtUtc': now, 'Scope': 'Read-only source geometry and prefab existence inventory for tier-two art planning.',
                 'Limits': 'No fresh Blender/Unity evaluated import, multiview rendering, gameplay or device validation. Existing preview sheets inspected separately.',
                 'Summary': {'AllProjectObjMeshesScanned': len(meshes), 'TierTwoLooseMeshesWithPrefabs': len(loose),
                             'TierTwoEnvironmentObjMeshes': environment_count, 'TierTwoExclusiveRecipeMeshesWithPrefabs': len(existing_exclusives),
                             'TierTwoSpecialGearItems': len(special), 'TierTwoSpecialGearRepresentations': len(special)*3,
                             'MainMissionRecipeTargets': mission_target_count, 'MainMissionCategories': mission_categories,
                             'AllRecipeTargetsIncludingSideAndOptional': len(targets),
                             'RecipeTargetsWithExistingMesh': sum(bool(r['ExistingMeshPaths']) for r in targets)},
                 'LooseResources': loose, 'SpecialMerchantItems': special, 'HouseLevelsThreeAndFour': houses,
                 'ExistingExclusiveRows': existing_exclusives, 'MeshRecords': meshes,
                 'SourceSnapshots': [{'Path': relative(p), 'SHA256': hashlib.sha256(p.read_bytes()).hexdigest()}
                                     for p in [PLAN, exclusive_path, goods_path,
                                         ROOT/'Assets/_Game/Art/Settlement/Houses/HouseManifest.json',
                                         *[ROOT/f'Assets/_Game/Art/Items/Region{r:02}/ResourceManifest.json' for r in range(7,13)]]],
                 'OtherMeshFormatPaths': [relative(p) for p in (ROOT/'Assets/_Game').rglob('*') if p.suffix.lower() in {'.fbx','.glb','.gltf','.blend'}]}
    catalog = {'GeneratedAtUtc': now, 'Scope': 'Planning only. Null IDs need reconciliation before runtime authoring; names are not substitute IDs.',
               'Recipes': targets, 'HarvestAndNewStoreInputs': raw_targets, 'FurnitureTargets': furniture,
               'CommonPetTargets': [{'Id': 'pet.'+n, 'Action': 'Plan common companion model; no dedicated mesh confirmed; rabbit harvest parts are only reference'}
                                    for n in ['dog','cat','rabbit','pig','squirrel','tortoise']]}
    (OUT/'tier2-mesh-inventory-2026-11-06.json').write_text(json.dumps(inventory, indent=2, ensure_ascii=False)+'\n',encoding='utf-8')
    (OUT/'tier2-item-design-catalog-2026-11-06.json').write_text(json.dumps(catalog, indent=2, ensure_ascii=False)+'\n',encoding='utf-8')
    rows = ['# Tier-two item design coverage', '', 'Generated from current design tables and on-disk meshes. Planning inventory, not runtime acceptance.', '',
            '| Output | Category | Source | Existing dedicated mesh | Action |', '| --- | --- | --- | --- | --- |']
    for r in targets:
        source = r.get('MissionCode',r['UnlockSource'])
        if 'MissionRegion' in r:
            source += f" (region {r['MissionRegion']})"
        paths = '<br>'.join(r['ExistingMeshPaths']) or 'None found'
        rows.append(f"| {r['Name']} | {r['Category']} | {source} | {paths} | {r['Action']} |")
    (OUT/'tier2-item-design-coverage-2026-11-06.md').write_text('\n'.join(rows)+'\n',encoding='utf-8')
    print(json.dumps(inventory['Summary'],indent=2))
    print('Planning records written under Docs/Art/Tier02; no art or gameplay files changed.')


if __name__ == '__main__':
    main()
