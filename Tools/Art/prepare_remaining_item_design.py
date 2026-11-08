"""Read-only source inventory for the remaining item design pass.
Writes planning JSON/text under Docs only; never generates art or runtime catalogs.
"""
from pathlib import Path
from collections import Counter
import hashlib
import json
import re

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Docs/Art/ItemDesignPreparation'
PATHS={
    'tier1':'Assets/_Game/Art/Crafting/Tier01/Tier01Recipes.json',
    'exclusive':'Assets/_Game/Art/Crafting/BalloonExclusive/BalloonExclusiveManifest.json',
    'merchant':'Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsManifest.json',
    'harvest':'Assets/_Game/Art/Harvest/Tier01/mesh-manifest.json',
    'avatar':'Assets/_Game/Art/Characters/PlayerDefault/PlayerAvatarManifest.json',
    'recommendations':'PROJECT_RECOMENDATIONS.md',
    'expansion':'PROJECT_RECOMENDATIONS_PART_2.md',
}


def read(path): return (ROOT/path).read_text(encoding='utf-8-sig')
def load(key): return json.loads(read(PATHS[key]))


def main():
    OUT.mkdir(parents=True,exist_ok=True)
    tier=load('tier1'); exclusive=load('exclusive'); merchant=load('merchant'); avatar=load('avatar')
    assets={}; recipes={}; issues=[]
    def asset(key,name,category,batch):
        return assets.setdefault(key,dict(ItemId=key,Name=name,Category=category,DesignBatch=batch,
            Scope='Existing authored asset; runtime eligibility must be checked independently',References=[]))
    def recipe(row,source):
        identity=row.get('Id',row.get('RecipeId'))
        record=recipes.setdefault(identity,dict(RecipeId=identity,OutputId=row['OutputId'],SourceRecords=[]))
        record['SourceRecords'].append(dict(Source=source,Data=row))
        category=row['Category']; batch={'Material':'B2','Tool':'B3','Wearable':'B3','Food':'B4','Misc':'B5','Barrel':'B5'}[category]
        entry=asset(row['OutputId'],row['Name'],category,batch)
        entry['References'].append(dict(Source=source,Model=row.get('Model'),PrefabPath=row.get('PrefabPath')))
        if row.get('FutureTierArtOnly') or row.get('Tier',1)>4:entry['Scope']='Deferred expansion art only; do not activate or redesign in the active pass'
    for row in tier['Recipes']:recipe(row,PATHS['tier1'])
    for row in exclusive['Items']:recipe(row,PATHS['exclusive'])
    for row in tier['ExternalInputs']:
        entry=asset(row['Id'],row['Name'],'Shop/raw ingredient','B1')
        entry['References'].append(dict(Source=PATHS['tier1'],Model=row['Model'],PrefabPath=row['PrefabPath']))
        if row['Id']=='ingredient.mushrooms':entry['Scope']='Completed loose-resource reference; preserve isolated material and reuse'
    for row in merchant['Items']:
        entry=asset(row['Id'],row['Name'],'Merchant '+row['Slot'],'B6')
        entry['References'].append(dict(Source=PATHS['merchant'],Data=row))
    # Resolve harvest and runtime-only identities from the owning literal definitions.
    runtime={}; prototype_names={}
    for path in ['Assets/_Game/Scripts/Domain/Items/ItemModel.cs','Assets/_Game/Scripts/Domain/Items/Tier1CraftedItems.cs']:
        text=read(path)
        for match in re.finditer(r'new ItemDefinition\("([^"\n]+)",\s*"([^"\n]+)",\s*(\d+),\s*(\d+)([^\n]*)',text):
            identity,name,width,height,tail=match.groups()
            if identity.startswith(('debug.','test.')):continue
            hearts=re.search(r'foodHearts:\s*(\d+)',tail)
            runtime[identity]=dict(Name=name,Width=int(width),Height=int(height),Hearts=int(hearts[1]) if hearts else 0,Source=path)
        for match in re.finditer(r'ItemDefinition\s+(\w+)\s*=\s*new ItemDefinition\("([^"\n]+)"',text):prototype_names[match[1]]=match[2]
    names=['PineLog','PineNuts','RawRabbitMeat','Bone','AnimalFat','RainbowTrout','LargemouthBass','Bluegill','YellowPerch']
    keys=['Log','PineNuts','RawRabbitMeat','Bone','AnimalFat','RainbowTrout','LargemouthBass','Bluegill','YellowPerch']
    for model,key in zip(names,keys):
        identity=prototype_names[key]; entry=asset(identity,runtime[identity]['Name'],'Harvest drop','B1')
        entry['DesignBatch']='B1'
        entry['References'].append(dict(Source='Assets/_Game/Editor/Tier01HarvestAssets.cs',Model=model,PrefabPath='Tier01/Drops/'+model))
    region_text=read('Assets/_Game/Scripts/Domain/Travel/RegionPlan.cs')
    completed=set()
    for line in region_text.splitlines():
        match=re.search(r'new RegionPlanEntry\((\d+),',line)
        if match and int(match[1])<=6:completed.update(re.findall(r'"((?:material|food|ingredient|animal)\.[^"]+)"',line))
    for identity,row in runtime.items():
        if identity not in assets and identity not in completed:
            entry=asset(identity,row['Name'],'Runtime definition without inventoried art reference','B0')
            entry['Scope']='Resolve existing visual lookup before scheduling new geometry'
        if identity in assets:assets[identity]['RuntimeDefinition']=row
    for identity in completed:
        if identity in assets:assets[identity]['Scope']='Completed loose-resource reference; reuse existing model/material'
    for record in recipes.values():
        values={row['Data'].get('Hearts',row['Data'].get('HeartsPerServing',0)) for row in record['SourceRecords']}
        output=record['OutputId']
        if output in runtime:values.add(runtime[output]['Hearts'])
        if len(values)>1:
            issues.append(dict(Kind='Recovery metadata disagreement',RecipeId=record['RecipeId'],OutputId=output,
                ObservedValues=sorted(values),Action='Reconcile source ownership before regeneration; do not change balance during the art pass'))
    for model in tier['Models']:
        for row in tier['Recipes']+tier['ExternalInputs']:
            identity=row.get('OutputId',row.get('Id'))
            if row['Model']==model['Model'] and identity in runtime:
                old=[model['Width'],model['Height']]; current=[runtime[identity]['Width'],runtime[identity]['Height']]
                if old!=current:issues.append(dict(Kind='Art footprint metadata differs from runtime',ItemId=identity,Model=model['Model'],Art=old,Runtime=current,
                    Action='Classify intended nominal-size/presentation overrides versus stale metadata; preserve live footprint and approved envelope without rebalance'))
    # Furniture names/effect codes come from the design table, not simplified house proxy labels.
    text=read(PATHS['recommendations']); furniture=[]
    section=text.split('### 13.2 Furniture and buff distribution',1)[1].split('### 13.3',1)[0]
    for line in section.splitlines():
        match=re.match(r'\|\s*([1-8])\s*\|\s*(.+?)\s*\|$',line)
        if not match:continue
        for name,effect in re.findall(r'([^,]+?)\s*\(([A-Za-z]+)\)',match[2]):
            furniture.append(dict(Name=name.strip(),HouseLevel=int(match[1]),EffectCode=effect,DesignBatch='B7',
                CanonicalItemId=None,Scope='Finished furniture design pending; existing house studies are proxies, not runtime definitions'))
    candidates={}
    for key in ('recommendations','expansion'):
        for number,line in enumerate(read(PATHS[key]).splitlines(),1):
            if not line.startswith('|'):continue
            for identity in re.findall(r'`(recipe\.[a-z_]+\.[a-z0-9_]+)`',line):
                if identity in recipes:continue
                candidate=candidates.setdefault(identity,dict(RecipeId=identity,Scope='Deferred expansion proposal' if key=='expansion' else 'Documented proposal; canonical catalog/source authoring required',References=[]))
                candidate['References'].append(dict(Source=PATHS[key],Line=number,TableRow=line))
    clothing=[]
    # Section 9.6.1 explicitly prescribes recipe.wearable.<id> -> item.<id>.
    # These are documented future designs, not evidence of runtime definitions.
    for number,line in enumerate(text.splitlines(),1):
        match=re.match(r'\|\s*([2-4])\s+([AB])\s*\|\s*(Hat|Boots|Shirt|Pants|Backpack)\s*\|\s*(.*?)\s*\(`([a-z0-9_]+)`\)\s*\|\s*(.*?)\s*\|\s*(\d+)\s*\|',line)
        if not match:continue
        tier_number,set_name,slot,name,suffix,inputs,region=match.groups()
        entry=dict(Tier=int(tier_number),Set=set_name,Slot=slot,Name=name,ItemId='item.'+suffix,
            RecipeId='recipe.wearable.'+suffix,IngredientBrief=inputs,LatestSourceRegion=int(region),
            DesignBatch='B3',Scope='Documented active-campaign design; runtime catalog integration separate',Source=PATHS['recommendations'],Line=number)
        clothing.append(entry)
        candidates.setdefault(entry['RecipeId'],dict(RecipeId=entry['RecipeId'],Scope=entry['Scope'],References=[dict(Source=entry['Source'],Line=number,TableRow=line)]))
    sources=[dict(Path=path,SHA256=hashlib.sha256((ROOT/path).read_bytes()).hexdigest()) for path in PATHS.values()]
    summary=dict(TierOneRecipeRows=len(tier['Recipes']),TierOneCategories=dict(Counter(r['Category'] for r in tier['Recipes'])),
        ExclusiveRecipeRows=len(exclusive['Items']),ExclusiveActiveTierArtRows=sum(r['Tier']<=4 for r in exclusive['Items']),
        ExclusiveDeferredExpansionRows=sum(r['Tier']>4 for r in exclusive['Items']),
        UniqueExistingRecipeIdentities=len(recipes),PhysicalItemIdentities=len(assets),MerchantGearRows=len(merchant['Items']),
        MysteryBoxVariants=len(merchant['BoxTiers']),FurnitureBriefs=len(furniture),SourceDiscrepancies=len(issues),
        DocumentedRecipeCandidates=len(candidates),DocumentedLaterTierCraftedWearables=len(clothing))
    output=dict(Status='Design preparation only. No art, gameplay or test execution.',
        Coverage='Complete rows from the three existing item/recipe manifests plus literal runtime definitions, nine harvest drops and furniture table. Document-only recipe candidates are partial extraction, not an exhaustive authored tier-2-to-4 catalog.',
        Summary=summary,Sources=sources,Items=sorted(assets.values(),key=lambda r:(r['DesignBatch'],r['ItemId'])),
        Recipes=sorted(recipes.values(),key=lambda r:r['RecipeId']),MysteryBoxes=merchant['BoxTiers'],
        FittedTierOneWearables=avatar.get('wearables',[]),Furniture=furniture,
        DocumentedLaterTierCraftedWearables=clothing,
        DocumentedRecipeCandidates=sorted(candidates.values(),key=lambda r:r['RecipeId']),SourceDiscrepancies=issues)
    (OUT/'item-design-inventory.json').write_text(json.dumps(output,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
    lines=['SOURCE RECONCILIATION / PLANNING ONLY','Do not overwrite live gameplay values from an older art manifest.','']
    for issue in issues:lines.append(json.dumps(issue,ensure_ascii=False))
    (OUT/'source-reconciliation.txt').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    print(json.dumps(summary,indent=2))


if __name__=='__main__':main()
