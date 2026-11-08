"""Batch 06: flax textiles and cattail cord; passive Material art only."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import math,json
import bpy
from mathutils import Vector
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import stroke,ellipsoid
from generate_tier02_batch05 import copy_support

ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'Docs/Art/Tier02/Batch06'

def slab(m,cx,cy,z,w,h,slot):
    points=[(cx+x*w/2,cy+y*h/2,z+.008*math.sin(i*1.8)) for i,(x,y) in enumerate([(-.94,-1),(.90,-1),(1,-.86),(1,.86),(.92,1),(-.92,1),(-1,.86),(-1,-.86)])]
    kit.closed_patch(m,points,(0,0,1),.018,slot)

def cloth(fine=False):
    m=base.Model('FineFlaxCloth' if fine else 'FlaxCloth')
    for i in range(3):slab(m,-.012*i,.028*i,-.035*i,.76-.025*i,.45-.016*i,i%2)
    # A turned-back fabric corner and soft rolled hem break the rigid slab silhouette.
    kit.closed_patch(m,[(-.36,.13,-.093),(-.18,.24,-.097),(-.34,.07,-.14)],(0,0,-1),.012,1)
    tube(m,[(-.34+i*.065,.247,-.083+.003*math.sin(i)) for i in range(11)],.015,1,8)
    support=copy_support(m)
    # Raised warp/weft follows the exposed broad face; selvedge belongs to cloth.
    for i in range(19 if fine else 12):
        x=-.35+i*(.66/(18 if fine else 11))
        stroke(m,[kit.front_surface(support,x,y,.002) for y in [-.14,-.04,.06,.15,.21]],2,.0015 if fine else .0025)
    for i in range(11 if fine else 7):
        y=-.135+i*(.35/(10 if fine else 6))
        stroke(m,[kit.front_surface(support,x,y,.003) for x in [-.34,-.2,-.05,.1,.30]],3,.0015 if fine else .002)
    for y in (-.14,.21):stroke(m,[kit.front_surface(support,x,y,.004) for x in [-.34,-.2,-.05,.1,.3]],1,.004)
    if fine:
        # Hemp-thread finishing stitches only on the advanced recipe output.
        for i in range(20):
            x=-.34+i*.033
            stroke(m,[kit.front_surface(support,x,y,.004) for x,y in [(x,-.131),(x+.012,-.116)]],4,.002)
    else:
        for i in range(13):
            x=-.34+i*.053
            stroke(m,[kit.front_surface(support,x,y,.003) for x,y in [(x,-.151),(x+.008,-.18-(i%3)*.007)]],2,.0025)
    return m

def tube(m,points,radius,slot,sides=6,flat=False):
    rings=[]
    for j,p in enumerate(points):
        tangent=Vector(points[min(j+1,len(points)-1)])-Vector(points[max(0,j-1)])
        tangent.normalize();u=tangent.cross(Vector((0,0,1)))
        if u.length<.01:u=tangent.cross(Vector((0,1,0)))
        u.normalize();v=tangent.cross(u).normalized()
        rings.append([m.v(Vector(p)+radius*(u*math.cos(k*math.tau/sides)+v*math.sin(k*math.tau/sides)*(.38 if flat else 1))) for k in range(sides)])
    for j in range(len(rings)-1):
        for k in range(sides):
            q=(k+1)%sides
            m.tri(rings[j][k],rings[j][q],rings[j+1][q],slot);m.tri(rings[j][k],rings[j+1][q],rings[j+1][k],slot)
    for j,reverse in [(0,True),(-1,False)]:
        c=m.v(points[j])
        for k in range(sides):
            a,b=rings[j][k],rings[j][(k+1)%sides]
            m.tri(c,b,a,slot) if reverse else m.tri(c,a,b,slot)

def cord(cattail=False):
    m=base.Model('CattailCord' if cattail else 'FlaxCord')
    # One continuous coil, three strands: twisted fine fibers versus flat leaf plait.
    for strand in range(3):
        points=[]
        for i in range(241):
            t=i/240;a=t*math.tau*2.15;r=.31-.15*t
            twist=a*(7 if cattail else 10)+strand*math.tau/3
            offset=.016*math.sin(twist)
            points.append(((r+offset)*math.cos(a),(r*.78+offset)*math.sin(a),.018*math.cos(twist)-.018*t))
        tube(m,points,.017 if cattail else .014,strand,6,flat=cattail)
    # Exposed ends read as loose cord rather than a closed wreath.
    for strand in range(3):
        tube(m,[(.31+i*.018,-i*.013,.018+strand*.008) for i in range(7)],.01,strand,6,flat=cattail)
    return m

def stuffing():
    m=base.Model('FlaxStuffing')
    ellipsoid(m,(0,0,-.01),(.24,.14,.045),1,10)
    for i in range(21):
        a=i*2.399;r=.03+.15*(i%4)/3
        ellipsoid(m,(r*math.cos(a),r*.7*math.sin(a),-.035+.014*math.sin(i)),(.13,.035,.022),i%3,10)
    for i in range(27):
        a=i*2.399;x=.19*math.cos(a);y=.12*math.sin(a)
        points=[(x*.5,y*.5,-.053),(x,y,-.065),(x*1.35,y*1.4,-.04)]
        tube(m,points,.003,2 if i%2 else 3,5)
    return m

SPECS=[
 ('FlaxCloth','Flax cloth',lambda:cloth(False),[(180,164,119),(200,186,143),(155,140,101),(213,201,164),(223,211,176)],[8]),
 ('FlaxCord','Flax cord',lambda:cord(False),[(169,150,102),(195,177,128),(221,203,158),(151,135,94),(210,195,150)],[8]),
 ('FlaxStuffing','Flax stuffing',stuffing,[(194,178,135),(213,199,160),(232,220,184),(176,160,117),(225,213,176)],[8]),
 ('FineFlaxCloth','Fine flax cloth',lambda:cloth(True),[(204,193,161),(223,214,187),(183,173,142),(233,226,204),(174,153,103)],[10]),
 ('CattailCord','Cattail cord',lambda:cord(True),[(133,126,73),(166,154,96),(194,179,117),(121,116,70),(181,167,106)],[12]),
]
TYPES={name:['plant_fibre']*5 for name,_,_,_,_ in SPECS}
RECIPES={'FlaxCloth':('T2-E1','4 Flax fiber -> 2'),'FlaxCord':('T2-E2','3 Flax fiber -> 2'),'FlaxStuffing':('T2-E4','3 Flax fiber -> 2'),'FineFlaxCloth':('T2-E10','3 Flax cloth + 1 Hemp thread -> 1'),'CattailCord':('T2-E14','3 Cattail fiber -> 1')}


def main():
    kit.DOC=DOC;kit.ART=ROOT/'Assets/_Game/Art/Crafting/Tier02'
    for p in [kit.ART,kit.PAL,DOC]:p.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
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
    for index,(name,label,build,colors,regions) in enumerate(SPECS):
        # Pelt faces, leather reverses and actual thread use separate semantic slots.
        types=TYPES[name]
        obj,row=kit.create_asset(name,colors,types,build());obj.location.x=(index-2)*1.85
        mission,brief=RECIPES[name]
        row.update(DisplayName=label,ItemId='',RecipeId='',RuntimeBinding='Passive Material art; canonical item/recipe IDs and live binding unresolved',MissionCode=mission,MissionRegion=regions[0],IngredientBrief=brief,PrefabPath='Tier02/Materials/'+name)
        obj['AssetRole']='crafted Material output art'
        objects.append(obj);items.append(row)
    manifest={'Revision':1,'Batch':6,'Scope':'Five passive Material output assets; no recipes/stats/sources/footprints changed.','BlenderVersion':bpy.app.version_string,'Items':items}
    (kit.ART/'Tier02Batch06Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    views={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    for obj in objects:
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(corners,Vector())/8
        camera.data.ortho_scale=max(max(v[i] for v in corners)-min(v[i] for v in corners) for i in range(3))*1.25
        for view,direction in views.items():
            camera.location=center+Vector(direction);kit.aim(camera,center);scene.render.filepath=str(DOC/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,7,3);kit.aim(camera,(0,0,0));camera.data.ortho_scale=9.5
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC/'tier02-batch06.blend'))
    print('BATCH06_GENERATED: five Material meshes/palettes, native scene and 30 review views.')


if __name__=='__main__':main()
