"""Targeted tier-one art generation; never rewrites recipes, stats or gameplay.

Existing mesh paths and exact authored envelopes are retained. Semantic palette
families replace shared-colour assumptions without touching the atlas/terrain.
"""
from pathlib import Path
from collections import defaultdict, deque
import hashlib
import json
import math
import re
import generate_tier01_crafted_items as crafted
import generate_tier01_harvest_meshes as harvest
import generate_region01_meshes as base
import detail_textures as detail
from tier01_harvest_designs import build as harvest_model

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Art/Items/Tier01Design'
PALETTES=OUT/'Palettes'
MANIFEST=OUT/'Tier01ItemDesignManifest.json'


def read_json(path):return json.loads((ROOT/path).read_text(encoding='utf-8'))


def envelope(path):
    vertices=[tuple(map(float,line.split()[1:])) for line in path.read_text(encoding='utf-8').splitlines() if line.startswith('v ')]
    return [min(p[i] for p in vertices) for i in range(3)],[max(p[i] for p in vertices) for i in range(3)]


def surface_family(key,category):
    types={};family=category
    def use(indices,kind):types.update({i:kind for i in indices})
    if category=='Harvest':
        family='HarvestOrganic'
        if key=='PineLog':family='HarvestTimber';use(range(3),'bark');use((3,4),'wood')
        elif key=='PineNuts':family='HarvestNuts';use((13,),'grain');use((31,),'wood')
        elif key in ('RainbowTrout','LargemouthBass','Bluegill','YellowPerch'):
            family='RawFish'+key;use(range(14,26),'fish_scales');use((16,20,23),'flat')
    elif category=='Material':
        if key.startswith('pine_') and key!='pine_charcoal':family='Timber';use(range(4),'wood');use(range(4,8),'plant_fibre')
        elif key=='pine_charcoal':family='Charcoal';use(range(16,20),'ash')
        elif key in ('hemp_cord','hemp_thread','nettle_thread','reed_braid','plant_stuffing'):
            family='Fibre';use(range(4),'wood');use(range(4,16),'plant_fibre')
        elif 'cloth' in key:family='Textiles';use(range(4,16),'cloth')
        elif 'leather' in key:family='Leather';use(range(4,8),'plant_fibre');use(range(8,12),'leather')
        elif 'pelt' in key:family='ProcessedHide';use(range(4,8),'fur');use(range(8,12),'leather')
        elif key in ('clay_cup','fired_bricks'):family='Pottery';use(range(20,24),'ceramic')
        elif key in ('limestone_tiles','polished_pebble','mortar'):family='Mineral';use(range(16,20),'mineral')
        else:family='Flour';use((28,29),'flat')
    elif category=='Tool':
        family='PineTools';use(range(4),'wood');use(range(4,8),'plant_fibre');use(range(8,12),'leather');use(range(16,20),'mineral')
    elif category=='Wearable':
        family='PeltGear' if key in ('rabbit_hat','rabbit_boots') else 'TextileGear'
        use(range(4),'wood');use(range(4,8),'fur' if family=='PeltGear' else 'cloth');use(range(8,12),'leather');use(range(12,16),'cloth')
    elif category=='Food' or category=='External':
        family='PreparedFood'
        use(range(4),'wood');use((6,7),'grain');use((19,20),'ceramic')
        if key in ('cow_milk','goat_milk'):
            family='GoatMilk' if key=='goat_milk' else 'CowMilk';types={};use((19,) if key=='goat_milk' else (28,),'ceramic')
        elif key in ('egg','shrimp'):family='RawEgg' if key=='egg' else 'RawShrimp';types={}
        elif key in ('apple_juice','strawberry_cow_yogurt_drink'):
            family='JuiceVessels';use((19,20),'ceramic');use((24,25),'fruit_skin')
        elif key in ('cow_cream','cow_yogurt','cow_butter','cow_cheese','goat_cheese','mixed_milk_cheese'):
            family='Dairy';use((19,20),'ceramic')
    elif category=='Misc':
        family='WoodenMisc';use(range(4),'wood');use(range(4,8),'plant_fibre')
        if key in ('rabbit_toy','pinecone_rabbit'):family='ToyMisc';use(range(4,8),'fur');use(range(8,12),'stitched_leather')
        if key=='pinecone_ornament':use(range(12,16),'wood')
    return family,types


def write_mesh(model,path,colors,types):
    # Legacy helpers connected outward caps to inward extrusion side bands.
    # Orient only closed manifold components; retain double-sided fins/relief.
    edges=defaultdict(list)
    for index,(a,b,c,_) in enumerate(model.faces):
        for p,q in ((a,b),(b,c),(c,a)):edges[tuple(sorted((p,q)))].append((index,p<q))
    adjacency=defaultdict(list)
    for incident in edges.values():
        if len(incident)==2:
            (a,d0),(b,d1)=incident
            adjacency[a].append((b,d0==d1));adjacency[b].append((a,d0==d1))
    visited=set()
    for first in range(len(model.faces)):
        if first in visited:continue
        queue=deque([first]);flips={first:False};component=[];closed=True
        while queue:
            face=queue.popleft()
            if face in visited:continue
            visited.add(face);component.append(face)
            a,b,c,_=model.faces[face]
            if any(len(edges[tuple(sorted(pair))])!=2 for pair in ((a,b),(b,c),(c,a))):closed=False
            for neighbour,toggle in adjacency[face]:
                if neighbour not in flips:flips[neighbour]=flips[face]^toggle;queue.append(neighbour)
        if not closed:continue
        volume=0
        for face in component:
            a,b,c,_=model.faces[face]
            if flips[face]:b,c=c,b
            p,q,r=(model.vertices[i-1] for i in (a,b,c))
            volume+=sum(x*y for x,y in zip(p,base.cross(q,r)))
        for face in component:
            if flips[face]^(volume<0):
                a,b,c,k=model.faces[face];model.faces[face]=(a,c,b,k)
    model.name=path.stem
    lines=['# Tier-one item design; stable source path and authored bounds; Y up, metres.',
           '# Faceted normals; semantic palette assigned by Tier01ItemDesignAssets.','o '+model.name]
    lines+=['v %.6f %.6f %.6f'%p for p in model.vertices]
    lines+=['vt %.7f 0.5'%((i+.5)/len(colors)) for i in range(len(colors))]
    lines+=['s off']
    lines+=['f %d/%d %d/%d %d/%d'%(a,k+1,b,k+1,c,k+1) for a,b,c,k in model.faces]
    path.write_text('\n'.join(lines)+'\n',encoding='utf-8')


def main():
    from tier01_material_tool_designs import build as material_model
    from tier01_food_misc_designs import build as food_model
    OUT.mkdir(parents=True,exist_ok=True);PALETTES.mkdir(parents=True,exist_ok=True)
    data=read_json('Assets/_Game/Art/Crafting/Tier01/Tier01Recipes.json')
    planned=[];entries=[];profiles={};snapshots={}
    for row in data['Recipes']:
        if row['OutputId']=='material.pine_log':continue
        folder='BalloonExclusive' if row['PrefabPath'].startswith('BalloonExclusive/') else 'Tier01'
        planned.append(dict(ItemId=row['OutputId'],Name=row['Name'],Model=row['Model'],Key=row['Style'],Category=row['Category'],
            SourcePath=f'Assets/_Game/Art/Crafting/{folder}/{row["Model"]}.obj',PrefabPath=row['PrefabPath'],RecipeId=row['Id']))
    for row in data['ExternalInputs']:
        if row['Id']=='ingredient.mushrooms':continue # completed shared loose item, keep isolated palette.
        key={'CowMilk':'cow_milk','GoatMilk':'goat_milk'}.get(row['Model'],row['Model'].lower())
        planned.append(dict(ItemId=row['Id'],Name=row['Name'],Model=row['Model'],Key=key,Category='External',
            SourcePath=f'Assets/_Game/Art/Crafting/Tier01/{row["Model"]}.obj',PrefabPath=row['PrefabPath'],RecipeId=''))
    for name,identity in [('PineLog','material.pine_log'),('PineNuts','ingredient.pine_nuts'),('RawRabbitMeat','animal.rabbit.meat'),
                          ('Bone','material.bone'),('AnimalFat','ingredient.animal_fat'),('RainbowTrout','fish.rainbow_trout'),
                          ('LargemouthBass','fish.largemouth_bass'),('Bluegill','fish.bluegill'),('YellowPerch','fish.yellow_perch')]:
        planned.insert(0,dict(ItemId=identity,Name=re.sub(r'(?<!^)(?=[A-Z])',' ',name),Model=name,Key=name,Category='Harvest',
            SourcePath=f'Assets/_Game/Art/Harvest/Tier01/{name}.obj',PrefabPath='Tier01/Drops/'+name,RecipeId=''))
    # Save the contract before generating any geometry. Re-runs use the same exact envelope.
    old=read_json(str(MANIFEST.relative_to(ROOT))) if MANIFEST.exists() else None
    bounds={r['ItemId']:(r['BoundsMin'],r['BoundsMax']) for r in old['Items']} if old else {}
    owner_paths=['Assets/_Game/Art/Crafting/Tier01/Tier01Recipes.json','Assets/_Game/Art/Crafting/BalloonExclusive/BalloonExclusiveManifest.json',
                 'Assets/_Game/Scripts/Domain/Items/ItemModel.cs','Assets/_Game/Scripts/Domain/Items/Tier1CraftedItems.cs',
                 'Assets/_Game/Scripts/Domain/Crafting/Tier1Recipes.cs']
    snapshots={p:hashlib.sha256((ROOT/p).read_bytes()).hexdigest() for p in owner_paths}
    for row in planned:
        path=ROOT/row['SourcePath'];lo,hi=bounds.get(row['ItemId'],envelope(path))
        if row['Category']=='Harvest':
            model=harvest_model(row['Model']);colors=list(harvest.COLORS)
            if row['Model'] in ('RainbowTrout','LargemouthBass','Bluegill','YellowPerch'):
                fin={'RainbowTrout':17,'LargemouthBass':19,'Bluegill':26,'YellowPerch':26}[row['Model']]
                colors.extend([harvest.COLORS[fin],harvest.COLORS[7]])
        else:
            model=material_model(row['Key'],row['Model']) or food_model(row['Key'],row['Model'])
            if model is None:raise ValueError('No targeted design for '+row['Key'])
            colors=list(crafted.COLORS)
        model.fit_bounds(lo,hi)
        family,types=surface_family(row['Key'],row['Category'])
        signature=json.dumps([colors,types],sort_keys=True)
        if signature not in profiles:
            profile_name=family
            if any(p['Name']==profile_name for p in profiles.values()):profile_name+='_'+hashlib.sha1(signature.encode()).hexdigest()[:6]
            palette=PALETTES/(profile_name+'Palette.png')
            pixels=[(*color,detail.alpha(types.get(i))) for i,color in enumerate(colors)]
            detail.write_png(palette,len(colors)*4,4,[[px for px in pixels for _ in range(4)] for _ in range(4)])
            profiles[signature]=dict(Name=profile_name,Path=str(palette.relative_to(ROOT)).replace('\\','/'),DetailScale=5,DetailStrength=.5)
        profile=profiles[signature]
        write_mesh(model,path,colors,types)
        row.update(PalettePath=profile['Path'],PaletteFamily=profile['Name'],BoundsMin=lo,BoundsMax=hi,Triangles=len(model.faces))
        entries.append(row)
        print(row['Model'],len(model.faces),'triangles',profile['Name'])
    result=dict(Revision=1,Scope='Tier-one art only. No recipe/stat/terrain changes. Mushrooms retained from loose-item pass.',
        Items=entries,PaletteFamilies=list(profiles.values()),OwningSourceHashes=snapshots,
        RecipeRows=len(data['Recipes']),PreservedMushrooms='Tier01/Crafted/Mushrooms')
    MANIFEST.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print('Generated',len(entries),'stable-path item meshes and',len(profiles),'shared semantic palettes.')


if __name__=='__main__':main()
