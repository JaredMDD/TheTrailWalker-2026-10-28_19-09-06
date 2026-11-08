"""Make bounded art work packets from the existing tier-two planning catalog."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Docs/Art/Tier02'


def main():
    catalog = json.loads((OUT / 'tier2-item-design-catalog-2026-11-06.json').read_text(encoding='utf-8'))
    recipes = {r['Name']: r for r in catalog['Recipes']}
    batches, assigned = [], set()

    def add(title, names, kind='new inventory art', note=''):
        keys = []
        for name in names:
            key = ('recipe:' if name in recipes else kind + ':') + name
            assert key not in assigned, key
            assigned.add(key)
            keys.append(key)
        number = len(batches) + 1
        batches.append({'Number': number, 'Title': title, 'Kind': kind, 'Subjects': names,
                        'PlanningKeys': keys, 'Note': note, 'Status': 'planned'})

    add('First harvest-input slice', ['Birch log', 'Birch sap', 'Raw beaver meat'],
        note='One log asset is shared by tree drops and the branch-to-log recipe. Only Birch sap has a confirmed runtime ID; log/meat art can be delivered without inventing IDs.')
    add('Four raw fish', ['Common carp', 'Northern pike', 'Channel catfish', 'Walleye'])
    add('New shop ingredients', ['Lavender', 'Crab', 'Duck meat'], note='Preserve farm/fishery/ranch source exclusivity.')
    add('Birch processed wood', ['Birch plank', 'Birch beam', 'Birch charcoal', 'Birch panel'])
    add('Beaver refinements', ['Beaver leather', 'Cured beaver pelt', 'Fine beaver pelt', 'Fine beaver leather'])
    add('Flax and cattail materials', ['Flax cloth', 'Flax cord', 'Flax stuffing', 'Fine flax cloth', 'Cattail cord'])
    add('Metal inputs and hardware', ['Copper ingot', 'Copper fittings', 'Tin ingot'])
    add('Glass, quartz and stone', ['Glass', 'Glass bottle', 'Glass panels', 'Cut quartz', 'Sandstone tiles'])
    add('Cooking refinements', ['Barley flour', 'Flax oil', 'Beeswax'])
    for family in ['axe', 'slingshot', 'rod']:
        add('Normal and Elite birch ' + family, ['Normal birch ' + family, 'Elite birch ' + family], note='Two unique tools, with previous-tier upgrade materials visible; no new recipe costs.')
    add('Marsh hat and boots', ['Beaver-fur cap', 'Marsh boots'], note='Two item displays plus four fitted representations; six core meshes, with hair/pose review.')
    add('Marsh shirt and trousers', ['Flax work shirt', 'Flax trousers'], note='Two item displays plus four fitted representations; six core meshes.')
    add('Marsh backpack', ['Marsh pack'], note='Display plus male/female fit, straps, standing/seated clearance; Hemp cloth recipe is retained.')
    add('Taiga hat and boots', ['Taiga fur hat', 'Taiga boots'], note='Two item displays plus four fitted representations; six core meshes, with hair/pose review.')
    add('Taiga shirt and trousers', ['Taiga waxed shirt', 'Taiga trousers'], note='Two item displays plus four fitted representations; six core meshes.')
    add('Taiga backpack', ['Taiga frame pack'], note='Display plus male/female fit; resolve structure without inventing a wood input.')
    add('Drinks and preserves', ['Blackberry juice', 'Birch sap milk drink', 'Blueberry jam', 'Honey herbal tea', 'Honey oat drink'])
    add('Bottled tea and plant foods', ['Bottled honey tea', 'Carrot soup', 'Barley bread', 'Tomato rice bowl', 'Honey-glazed carrots'])
    add('Wild-meat meals and truffle side dish', ['Blackberry-glazed beaver', 'Beaver-carrot stew', 'Rabbit barley soup', 'Beaver mushroom pot', 'Creamy truffle-barley pot'], note='November 7 design replaces Roasted beaver with Blackberry-glazed beaver at T2-H2. Truffle side recipe has mission identity T2-G22; preserve optional nonblocking order and exact source gates. No runtime recipe authoring in the art batch.')
    add('Five fish dishes', ['Pike barley cakes', 'Walleye-barley soup', 'Carp rice bowl', 'Pike rice bowl', 'Catfish-tomato stew'])
    add('Misc craft goods A', ['Copper pendant', 'Birch beaver sculpture', 'Beaver toy', 'Lavender pouch'])
    add('Misc craft goods B', ['Painted birch bird', 'Fishing charm', 'Beeswax candle', 'Sandstone owl'])
    add('New premium duck dishes', ['Honey-roast duck', 'Duck blueberry rice'])
    add('New premium bakery foods', ['Honey brioche', 'Blackberry cream pastry'])
    add('New premium keepsakes', ['Quartz prism lantern', 'Tin-and-quartz music box'])
    add('Existing exclusive review A', ['Roasted crab', 'Creamy crab soup', 'Honey-blackberry biscuits', 'Beaver-barley hotpot'], 'existing-art review', 'Preserve paths/GUIDs; stale food values and Roasted crab premium eligibility require logic review before ingredient-driven changes.')
    add('Existing exclusive review B', ['Blackberry-honey oats', 'Birch-wax bee toy', 'Truffle cow-cheese flatbread'], 'existing-art review', 'Review current surfaces and silhouettes without changing food values.')
    add('Merchant hat and boots', ['special.t2.hat', 'special.t2.boots'], 'merchant review/refit', 'Existing item and worn assets; body/hair fit and material review, not craftable gear.')
    add('Merchant shirt and pants', ['special.t2.shirt', 'special.t2.pants'], 'merchant review/refit', 'Existing item and worn assets; no infinite-durability or stat changes.')
    add('Merchant backpack', ['special.t2.pack'], 'merchant review/refit', 'Existing three representations; standing/seated strap and back clearance.')
    add('Tier-two mystery box', ['MysteryBoxTier02'], 'existing-art review', 'Keep shared body/lid hierarchy, pivots and clips; isolated teal/brass surface review.')
    furniture = catalog['FurnitureTargets']
    for i in range(0, len(furniture), 2):
        pair = furniture[i:i+2]
        add('House-level-' + str(pair[0]['HouseLevel']) + ' furniture ' + str(i//2+1),
            [r['Name'] for r in pair], 'purchasable furniture', 'Standalone finished pieces, not study proxies; keep first-purchase effects and store-only acquisition.')
    for row in catalog['CommonPetTargets']:
        add('Common companion ' + row['Id'].split('.')[-1], [row['Id']], 'companion design',
            'Separate creature workload: reference/model/pose review; stop before animation if not included in its own brief. Pet-home model work stays deferred until Part 2 is close.')

    recipe_keys = {'recipe:' + name for name in recipes}
    assert recipe_keys <= assigned, sorted(recipe_keys-assigned)
    assert len([k for k in assigned if k.startswith('recipe:')]) == 82
    status_path = OUT / 'tier2-design-batch-status.json'
    old = {}
    if status_path.exists():
        old = json.loads(status_path.read_text(encoding='utf-8'))
        statuses = {b['Number']: b for b in old['Batches']}
        for b in batches:
            if b['Number'] in statuses:
                prior = statuses[b['Number']]
                if b['Number']==21 and prior['Status']=='planned':
                    # Explicit November 7 design rename; no accepted art is migrated.
                    migrated=['Blackberry-glazed beaver' if n=='Roasted beaver' else n for n in prior['Subjects']]
                    assert migrated==b['Subjects'],'Unexpected batch 21 membership change'
                else:
                    assert prior['Subjects'] == b['Subjects'], 'Batch membership drift; migrate deliberately'
                for field in ['Status', 'Evidence', 'Pending']:
                    if field in prior:
                        b[field] = prior[field]
    session_note = old.get('CurrentRunAuthorization','Default: one batch per request, finishing evidence and handoff before stopping.')
    record = {'Scope': 'Small restartable production batches, not estimates of account tokens or daily quota.',
              'CurrentRunAuthorization': session_note,
              'Rules': ['Default: one batch per request; finish evidence and handoff, then stop.',
                        'Several batches may fit one day, but no account allowance is assumed.',
                        'If interrupted, record current stage and next command; resume the same batch.',
                        'Do not automatically start the next batch or reopen already accepted art.',
                        'Logic-dependent rows remain pending until the owning logic decisions are settled.'],
              'Batches': batches}
    status_path.write_text(json.dumps(record,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
    rows = ['# Tier-two production batches', '',
            'November 6, 2026. These are bounded work packets to manage daily usage, not promises about token percentages or how many fit per day.', '',
            'Default to one batch per request unless the user authorizes more. Finish source, assets, selective import, useful review views and handoff before advancing. Resume an interrupted batch from its checkpoint.', '',
            '**Current run:** ' + session_note, '',
            'Ordinary packets contain 2-5 inventory subjects. Clothing packets are smaller because each item needs display plus two fitted representations. Backpacks and companions get separate packets. Existing-art reviews are distinct from new meshes. Keep pet-home models deferred.', '',
            '| Batch | Work | Subjects | Status |', '| --- | --- | --- | --- |']
    for b in batches:
        rows.append(f"| {b['Number']:02} | {b['Title']} | {', '.join(b['Subjects'])} | {b['Status']} |")
    rows += ['', 'Every one of the 82 recipe outputs is assigned exactly once; Birch log is shared between the first harvest batch and its recipe output.', '',
             'Machine-readable membership/checkpoints: `tier2-design-batch-status.json`. Broad brief: `remaining-tier2-design-plan-2026-11-06.md`. These packets do not activate recipes, sources, companion gameplay or expansion tiers.']
    (OUT/'tier2-production-batches-2026-11-06.md').write_text('\n'.join(rows)+'\n',encoding='utf-8')
    print(f'{len(batches)} bounded packets; all 82 recipe outputs assigned once.')


if __name__ == '__main__':
    main()
