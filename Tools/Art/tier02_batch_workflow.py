"""Shared deterministic presentation for later tier-two design batches.

Each batch owns its geometry, exact subjects, palettes and documentary metadata.
No runtime catalog, scene, shader or gameplay edits occur here.
"""
import json,sys
from pathlib import Path
import bpy
from mathutils import Vector
import generate_tier02_batch01 as kit

ROOT=Path(__file__).resolve().parents[2]


def generate(batch,specs,recipes,category='Materials',role='Material',preview=False,item_metadata=None):
    doc=ROOT/f'Docs/Art/Tier02/Batch{batch:02}'
    kit.ART=ROOT/'Assets/_Game/Art/Crafting/Tier02'
    for p in (doc,kit.ART,kit.PAL):p.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.filepaths.file_preview_type='NONE'
    bpy.context.preferences.filepaths.save_version=0
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16
    scene.render.resolution_x=640;scene.render.resolution_y=640;scene.render.resolution_percentage=50 if preview else 100
    scene.world=bpy.data.worlds.new('SlateWorld');scene.world.use_nodes=True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.19,.22,.24,1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value=.45
    scene.view_settings.view_transform='Standard';scene.view_settings.look='None'
    camera=bpy.data.objects.new('ReviewCamera',bpy.data.cameras.new('ReviewCamera'));scene.collection.objects.link(camera);scene.camera=camera;camera.data.type='ORTHO'
    for name,pos,power in [('Key',(-3,-4,6),450),('Fill',(4,-2,3),260),('Rim',(1,4,4),330)]:
        light=bpy.data.objects.new(name,bpy.data.lights.new(name,'AREA'));scene.collection.objects.link(light)
        light.data.energy=power;light.data.size=4;light.location=pos;kit.aim(light,(0,0,0))
    objects=[];items=[]
    for index,(name,label,build,colors,types) in enumerate(specs):
        assert len(colors)==len(types),(name,'palette type mismatch')
        obj,row=kit.create_asset(name,colors,types,build());obj.location.x=(index-(len(specs)-1)/2)*1.8
        mission,region,brief=recipes[name]
        row.update(DisplayName=label,ItemId='',RecipeId='',RuntimeBinding='Passive art; canonical IDs and live binding unresolved',
                   MissionCode=mission,MissionRegion=region,IngredientBrief=brief,PrefabPath=f'Tier02/{category}/'+name,Role=role)
        if name=='Beeswax':row['RecipeId']='recipe.material.beeswax'
        row.update((item_metadata or {}).get(name,{}))
        obj['AssetRole']=role+' output display art';objects.append(obj);items.append(row)
    manifest={'Revision':1,'Batch':batch,'Scope':f'{len(items)} passive {role} art outputs; no gameplay value or shared shader changes.',
              'BlenderVersion':bpy.app.version_string,'Items':items}
    (kit.ART/f'Tier02Batch{batch:02}Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    views={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    if preview:views={k:views[k] for k in ['front','study']}
    for obj in objects:
        row=next(r for r in items if r['Model']==obj.name)
        object_views=views
        if row.get('Representation')=='worn':
            object_views={k:(v[0],-v[1],v[2]) for k,v in views.items()}
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(corners,Vector())/8
        camera.data.ortho_scale=max(max(v[i] for v in corners)-min(v[i] for v in corners) for i in range(3))*1.4
        for view,direction in object_views.items():
            camera.location=center+Vector(direction);kit.aim(camera,center)
            scene.render.filepath=str(doc/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,10,4);kit.aim(camera,(0,0,0));camera.data.ortho_scale=len(objects)*1.9
    bpy.ops.wm.save_as_mainfile(filepath=str(doc/f'tier02-batch{batch:02}.blend'))
    print(f'BATCH{batch:02}_GENERATED: '+str([(r['Model'],r['Triangles']) for r in items]))
