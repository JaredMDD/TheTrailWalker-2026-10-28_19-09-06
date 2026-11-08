"""Batch 08: five passive glass/quartz/stone Material displays, Y-up OBJ."""
import sys, math, json
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
import bpy
from mathutils import Vector
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch07 import outline

ROOT = Path(__file__).resolve().parents[2]
DOC = ROOT/'Docs/Art/Tier02/Batch08'


def slab(m, points, center, angle, depth, stone=False, variant=0):
    """Closed beveled slab; front cap has inset rings for seated facet detail."""
    cx,cy,cz=center
    def point(x,y,z):
        return (cx+x*math.cos(angle)-y*math.sin(angle), cy+x*math.sin(angle)+y*math.cos(angle), cz+z)
    rings=[]
    levels=[(0.94,depth/2),(1,depth/2-.009),(1,-depth/2+.009),(.94,-depth/2)]
    if not stone:levels.append((.78,-depth/2-.002))
    for scale,z in levels:
        rings.append([m.v(point(x*scale,y*scale,z)) for x,y in points])
    n=len(points)
    for k in range(len(rings)-1):
        for i in range(n):
            j=(i+1)%n
            color=([2,1,3,0][k] if not stone else [2,3,1,0][k])
            if k==3: color=(2 if i in (1,2,6) else 0) if not stone else (2 if (i+variant)%3 else 4)
            m.tri(rings[k][i],rings[k][j],rings[k+1][j],color)
            m.tri(rings[k][i],rings[k+1][j],rings[k+1][i],color)
    for ring,z,front in [(rings[0],depth/2,False),(rings[-1],levels[-1][1],True)]:
        c=m.v(point(0,0,z))
        for i in range(n):
            color=(0 if i%3 else 1) if not stone else 0
            m.tri(c,ring[i],ring[(i+1)%n],color)


def glass():
    m=base.Model('Glass')
    for pts,center,a,d in [
        ([(-.27,-.20),(.18,-.23),(.29,.02),(.10,.22),(-.24,.17)],(-.12,-.05,.025),-.20,.12),
        ([(-.15,-.14),(.17,-.10),(.10,.21),(-.11,.12)],(.21,.14,-.11),.32,.10),
        ([(-.20,-.09),(.20,-.06),(.14,.09),(-.12,.13)],(-.16,.28,.10),-.25,.075)]:
        slab(m,pts,center,a,d)
    return m


def bottle():
    m=base.Model('GlassBottle')
    profile=[(-.48,0,0),(-.48,.19,.16),(-.46,.235,.19),(-.42,.25,.205),
             (.13,.25,.205),(.23,.23,.19),(.31,.16,.14),(.35,.105,.10),
             (.40,.105,.10),(.41,.13,.12),(.45,.13,.12),(.465,.115,.105),
             (.465,.080,.072),(.44,.078,.070),(.36,.078,.070),(.315,.125,.11),
             (.22,.20,.16),(.12,.22,.175),(-.40,.22,.175),(-.43,.18,.14),(-.43,0,0)]
    def color(k,i):
        if k in (9,10,11):return 2
        if k>=12:return 3 if k<16 else 1
        if k in (1,2):return 1
        return 2 if i in (11,12,17) else 1 if i in (10,13,18) else 0
    kit.loft(m,profile,color,24)
    return m


def panels():
    m=base.Model('GlassPanels')
    slab(m,outline(.69,.84,.025),(-.09,.025,.042),-.10,.038)
    slab(m,outline(.69,.84,.025),(.095,-.035,-.015),.10,.038)
    return m


def quartz():
    m=base.Model('CutQuartz');rings=[]
    for scale,z in [(.60,-.17),(.80,-.115),(.96,-.045),(1,0),(.99,.025),(.72,.14),(.35,.24)]:
        rings.append([m.v((x,y,z)) for x,y in outline(.78*scale,.62*scale,.10*scale)])
    for k in range(len(rings)-1):
        for i in range(8):
            j=(i+1)%8;slot=[0,2,1,3,1,2,0,3][(i+k)%8]
            m.tri(rings[k][i],rings[k][j],rings[k+1][j],slot)
            m.tri(rings[k][i],rings[k+1][j],rings[k+1][i],slot)
    for ring,z,slot in [(rings[0],-.17,0),(rings[-1],.24,1)]:
        c=m.v((0,0,z))
        for i in range(8):m.tri(c,ring[i],ring[(i+1)%8],slot)
    return m


def sandstone():
    m=base.Model('SandstoneTiles')
    # Dressed corner irregularity and bevels, not giant boulders or brickwork.
    pts=[(-.36,-.27),(.31,-.28),(.36,-.21),(.35,.23),(.29,.28),(-.31,.27),(-.37,.20),(-.37,-.20)]
    slab(m,pts,(-.055,.03,.055),-.08,.09,True)
    slab(m,pts,(.065,-.04,-.04),.09,.09,True,1)
    # Shallow bedding bands seated on the broad face, never a glass-like inset frame.
    for y in (-.15,-.02,.13):
        angle=.09
        points=[]
        for x,dy in [(-.27,0),(.25,.025),(.24,.045),(-.25,.014)]:
            points.append((.065+x*math.cos(angle)-(y+dy)*math.sin(angle),-.04+x*math.sin(angle)+(y+dy)*math.cos(angle),-.0855))
        kit.closed_patch(m,points,(0,0,-1),.002,4 if y<0 else 2)
    return m


GLASS=[(150,196,191),(107,157,160),(220,240,229),(68,114,122),(182,216,209)]
QUARTZ=[(191,207,218),(225,235,236),(166,185,205),(242,245,237),(126,149,174)]
STONE=[(190,152,100),(161,119,73),(216,181,127),(140,108,71),(201,165,110)]
SPECS=[('Glass','Glass',glass,GLASS,'glass'),('GlassBottle','Glass bottle',bottle,GLASS,'glass'),
       ('GlassPanels','Glass panels',panels,GLASS,'glass'),('CutQuartz','Cut quartz',quartz,QUARTZ,'glass'),
       ('SandstoneTiles','Sandstone tiles',sandstone,STONE,'mineral')]
RECIPES={'Glass':('T2-G6',9,'3 Quartz + 1 Pine charcoal -> 2'),
         'GlassBottle':('T2-G17',12,'2 Glass -> 1'), 'GlassPanels':('T2-L17',12,'3 Glass -> 1'),
         'CutQuartz':('T2-G7',9,'2 Quartz -> 1'), 'SandstoneTiles':('T2-L9',10,'3 Sandstone -> 2')}


def main():
    kit.DOC=DOC;kit.ART=ROOT/'Assets/_Game/Art/Crafting/Tier02'
    for p in [kit.ART,kit.PAL,DOC]:p.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    # Disable OS file-browser thumbnails in headless sandbox runs.
    bpy.context.preferences.filepaths.file_preview_type='NONE'
    bpy.context.preferences.filepaths.save_version=0
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16
    scene.render.resolution_x=640;scene.render.resolution_y=640;scene.render.resolution_percentage=100
    scene.world=bpy.data.worlds.new('SlateWorld');scene.world.use_nodes=True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.19,.22,.24,1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value=.45
    scene.view_settings.view_transform='Standard';scene.view_settings.look='None'
    camera=bpy.data.objects.new('ReviewCamera',bpy.data.cameras.new('ReviewCamera'));scene.collection.objects.link(camera);scene.camera=camera;camera.data.type='ORTHO'
    for name,pos,power in [('Key',(-3,-4,6),450),('Fill',(4,-2,3),260),('Rim',(1,4,4),330)]:
        light=bpy.data.objects.new(name,bpy.data.lights.new(name,'AREA'));scene.collection.objects.link(light)
        light.data.energy=power;light.data.size=4;light.location=pos;kit.aim(light,(0,0,0))
    objects=[];items=[]
    for index,(name,label,build,colors,kind) in enumerate(SPECS):
        obj,row=kit.create_asset(name,colors,[kind]*len(colors),build());obj.location.x=(index-2)*1.3
        obj.data.materials[0].node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.32 if kind=='glass' else .8
        mission,region,brief=RECIPES[name]
        row.update(DisplayName=label,ItemId='',RecipeId='',RuntimeBinding='Passive Material art; canonical IDs and live binding unresolved',MissionCode=mission,MissionRegion=region,IngredientBrief=brief,PrefabPath='Tier02/Materials/'+name)
        obj['AssetRole']='crafted Material output art';objects.append(obj);items.append(row)
    manifest={'Revision':1,'Batch':8,'Scope':'Five passive Material assets; no gameplay values or shared shader changes.','BlenderVersion':bpy.app.version_string,'Items':items}
    (kit.ART/'Tier02Batch08Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    views={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    # --preview keeps the same geometry but produces cheap proportion/finish views.
    if '--preview' in sys.argv:views={'front':views['front'],'study':views['study']};scene.render.resolution_percentage=50
    for obj in objects:
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(corners,Vector())/8
        camera.data.ortho_scale=max(max(v[i] for v in corners)-min(v[i] for v in corners) for i in range(3))*1.4
        for view,direction in views.items():
            camera.location=center+Vector(direction);kit.aim(camera,center);scene.render.filepath=str(DOC/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,9,4);kit.aim(camera,(0,0,0));camera.data.ortho_scale=7
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC/'tier02-batch08.blend'))
    print('BATCH08_GENERATED: five Material meshes/palettes and native scene.')


if __name__=='__main__':main()
