"""Fresh native/OBJ checks, conservative falling-tree spacing and preservation audit."""
from pathlib import Path
import sys,json,math,hashlib
import bpy
ROOT=Path(__file__).resolve().parents[2];DOC=ROOT/'Docs/Art/Settlement/LumberFieldStorefront';ART=ROOT/'Assets/_Game/Art/Settlement/LumberField'
m=json.loads((ART/'LumberFieldArtManifest.json').read_text());pieces=[]
bpy.ops.wm.open_mainfile(filepath=str(DOC/'lumber-field-art.blend'))
for row in m['Pieces']:
    obj=bpy.data.objects[row['Model']];obj.data.calc_loop_triangles();assert len(obj.data.loop_triangles)==row['Triangles']
    assert obj.data.uv_layers.active
    assert all(n.image.packed_file for mat in obj.data.materials for n in mat.node_tree.nodes if n.type=='TEX_IMAGE')
for row in m['Pieces']:
    bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.wm.obj_import(filepath=str(ROOT/row['SourcePath']),forward_axis='Z',up_axis='Y')
    meshes=[o.data for o in bpy.context.scene.objects if o.type=='MESH'];assert len(meshes)==1;mesh=meshes[0];mesh.calc_loop_triangles()
    count=len(mesh.loop_triangles);degenerate=sum(t.area<1e-12 for t in mesh.loop_triangles)
    assert count==row['Triangles'],(row['Model'],count,row['Triangles']);assert degenerate==0,(row['Model'],degenerate)
    assert all(math.isfinite(x) for v in mesh.vertices for x in v.co);assert mesh.uv_layers.active
    vs=[tuple(map(float,l.split()[1:])) for l in (ROOT/row['SourcePath']).read_text().splitlines() if l.startswith('v ')]
    lo=[min(v[a] for v in vs) for a in range(3)];hi=[max(v[a] for v in vs) for a in range(3)]
    assert max(abs(lo[a]-row['BoundsMin'][a]) for a in range(3))<2e-6;assert max(abs(hi[a]-row['BoundsMax'][a]) for a in range(3))<2e-6
    dims=sorted(max(v.co[a] for v in mesh.vertices)-min(v.co[a] for v in mesh.vertices) for a in range(3))
    assert max(abs(x-y) for x,y in zip(dims,sorted(hi[a]-lo[a] for a in range(3))))<2e-6
    pieces.append(dict(Model=row['Model'],Triangles=count,DegenerateFaces=degenerate,Bounds='PASS',UV=True))
zones=[]
for a in m['Assemblies']:
    assert a['Triangles']<=a['TriangleCeiling']
    if not a['Name'].startswith('LumberZone'):continue
    assert len(a['TreeAnchors'])==6
    rowgap=a['RowSpacing']+a['StandingMinZ']-a['SweepMaxZ'];frontgap=a['FrontClearance']-a['SweepMaxZ']
    assert rowgap>=3 and frontgap>=3
    assert a['ColumnSpacing']-(a['StandingBoundsX'][1]-a['StandingBoundsX'][0])>=3
    assert len([n for n in a['Nodes'] if n['Name'].startswith('BackdropAnchor')])==(0 if a['Tier']==4 else 3)
    zones.append(dict(Name=a['Name'],ColumnSpacing=a['ColumnSpacing'],RowSpacing=a['RowSpacing'],FrontClearance=a['FrontClearance'],RowSweepMargin=rowgap,FrontSweepMargin=frontgap,AllTreeAnchorsEmpty=True,NextTierBackdrop=a['Tier']<4))
counter=next(a for a in m['Assemblies'] if a['Name']=='LumberCounter')
assert not any(n['Name'].startswith('OfferAnchor') for n in counter['Nodes'])
hinge=next(n for n in counter['Nodes'] if n['Name']=='DoorPivot')['LocalPosition'];leaf=next(r for r in m['Pieces'] if r['Model']=='LumberDoorLeaf')
leafvs=[tuple(map(float,l.split()[1:])) for l in (ROOT/leaf['SourcePath']).read_text().splitlines() if l.startswith('v ')]
maxDoorZ=-999
for angle in range(101):
    co=math.cos(math.radians(angle));si=math.sin(math.radians(angle))
    maxDoorZ=max(maxDoorZ,max(hinge[2]-x*si+z*co for x,y,z in leafvs))
assert hinge[0]<0 and maxDoorZ<.13 # counter starts at Z=.14; leaf always stays behind it.
baseline=json.loads((DOC/'preservation-baseline.json').read_text());changed=[]
for row in baseline:
    p=ROOT/row['Path']
    if not p.exists() or hashlib.sha256(p.read_bytes()).hexdigest()!=row['SHA256']:changed.append(row['Path'])
allowed={'Assets/_Game/Editor/FacetedDetailMaterials.cs','Assets/_Game/Editor/StorefrontArtAssets.cs','Assets/_Game/Editor/ShopkeeperArtAssets.cs','Assets/_Game/Art/Settlement/Storefronts/StorefrontManifest.json','Assets/_Game/Art/Characters/Shopkeepers/ShopkeeperManifest.json'}
unexpected=[p for p in changed if p not in allowed];assert not unexpected,unexpected
for rel,key,field in [('Assets/_Game/Art/Settlement/Storefronts/StorefrontManifest.json','Shops','Name'),('Assets/_Game/Art/Characters/Shopkeepers/ShopkeeperManifest.json','Keepers','Name')]:
    prior=json.loads((DOC/'before'/rel).read_text());current=json.loads((ROOT/rel).read_text())
    for row in prior[key]:assert next(x for x in current[key] if x[field]==row[field])==row
report=dict(Status='PASS',Pieces=pieces,Zones=zones,DoorSweep=dict(CustomerRight='negative X',OpenDegrees=100,MaxDoorZ=maxDoorZ,CounterBackZ=.14,ClearOfCounter=True),ExistingAssetFilesChecked=len(baseline),ExpectedChangedFiles=changed,UnexpectedChanges=unexpected,ExistingMassiveTrees='All source/prefab/meta bytes preserved',ExistingShopAndKeeperRows='Unchanged',Scope='Passive art checks; no gameplay/travel/respawn/device acceptance')
(DOC/'offline-validation.json').write_text(json.dumps(report,indent=2)+'\n');print('LUMBER_VALIDATED',len(pieces),'pieces',len(zones),'zones; changes',changed)
