"""Batch 07: copper/tin ingots and copper hardware; passive Material art."""
import sys,math,json
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import bpy,bmesh
from mathutils import Vector
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import stroke

ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'Docs/Art/Tier02/Batch07'

def outline(w,h,c):
    return [(-w/2+c,-h/2),(w/2-c,-h/2),(w/2,-h/2+c),(w/2,h/2-c),(w/2-c,h/2),(-w/2+c,h/2),(-w/2,h/2-c),(-w/2,-h/2+c)]

def ingot(tin=False):
    m=base.Model('TinIngot' if tin else 'CopperIngot')
    w,h=(.80,.43) if tin else (.94,.36)
    rings=[]
    for z,scale in [(.085,.96),(.065,1),(-.07,.86),(-.085,.82),(-.077,.70)]:
        rings.append([m.v((x,y,z)) for x,y in outline(w*scale,h*scale,.026)])
    for j in range(4):
        for i in range(8):
            k=(i+1)%8;slot=[2,0,1,2][j]
            m.tri(rings[j][i],rings[j][k],rings[j+1][k],slot);m.tri(rings[j][i],rings[j+1][k],rings[j+1][i],slot)
    for ring,z,slot in [(rings[0],.085,3),(rings[-1],-.077,0)]:
        c=m.v((0,0,z))
        for i in range(8):m.tri(c,ring[i],ring[(i+1)%8],slot)
    # Shallow casting flow cues are seated relief, with no stamped brand/quality claims.
    for i in range(4 if tin else 3):
        x=-.20+i*.12
        stroke(m,[(x-.025,-.05,-.078),(x,.025,-.079),(x+.02,.05,-.078)],3,.0014)
    return m

def annulus(m,center,outer,inner,depth,slot=0,n=24):
    cx,cy,cz=center;rings=[]
    for z,r in [(cz-depth/2,outer),(cz+depth/2,outer),(cz-depth/2,inner),(cz+depth/2,inner)]:
        rings.append([m.v((cx+r*math.cos(i*math.tau/n),cy+r*math.sin(i*math.tau/n),z)) for i in range(n)])
    for a,b,color in [(0,1,slot),(2,3,3),(0,2,1),(1,3,2)]:
        for i in range(n):
            j=(i+1)%n;m.tri(rings[a][i],rings[a][j],rings[b][j],color);m.tri(rings[a][i],rings[b][j],rings[b][i],color)

def plate(m):
    bpy.ops.mesh.primitive_cube_add(size=1)
    obj=bpy.context.object;obj.name='PiercedPlateConstruction';obj.scale=(.64,.16,.035)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for x in (-.23,.23):
        bpy.ops.mesh.primitive_cylinder_add(vertices=24,radius=.036,depth=.20,location=(x,0,0))
        cutter=bpy.context.object;bpy.context.view_layer.objects.active=obj
        mod=obj.modifiers.new('TrueThroughHole','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter
        bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(cutter,do_unlink=True)
    bpy.context.view_layer.objects.active=obj
    bevel=obj.modifiers.new('WorkedEdges','BEVEL');bevel.width=.004;bevel.segments=1
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    offset=len(m.vertices)
    for v in obj.data.vertices:
        x,y,z=v.co;angle=.12
        m.v((x*math.cos(angle)-y*math.sin(angle),.11+x*math.sin(angle)+y*math.cos(angle),z))
    for f in obj.data.polygons:
        slot=1 if f.normal.z<-.9 else 0 if abs(f.normal.z)>.9 else 2
        m.tri(*[i+1+offset for i in f.vertices],slot)
    bpy.data.objects.remove(obj,do_unlink=True)

def fittings():
    m=base.Model('CopperFittings');plate(m)
    annulus(m,(-.17,-.13,0),.105,.052,.028,0)
    annulus(m,(.13,-.15,.008),.077,.055,.12,0)
    return m

COPPER=[(173,91,48),(210,131,78),(236,162,105),(125,64,38),(192,110,62)]
TIN=[(159,171,176),(195,206,209),(223,231,232),(117,130,140),(179,192,197)]
SPECS=[('CopperIngot','Copper ingot',lambda:ingot(False),COPPER,[8]),('CopperFittings','Copper fittings',fittings,COPPER,[8]),('TinIngot','Tin ingot',lambda:ingot(True),TIN,[12])]
TYPES={name:['metal']*5 for name,_,_,_,_ in SPECS}
RECIPES={'CopperIngot':('T2-L2','3 Copper ore + 1 Pine charcoal -> 1'),'CopperFittings':('T2-L4','1 Copper ingot -> 3'),'TinIngot':('T2-L14','3 Tin ore + 1 Pine charcoal -> 1')}


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
        # All palette slots use the existing metal detail tile.
        types=TYPES[name]
        obj,row=kit.create_asset(name,colors,types,build());obj.location.x=(index-1)*1.85
        mission,brief=RECIPES[name]
        row.update(DisplayName=label,ItemId='',RecipeId='',RuntimeBinding='Passive Material art; canonical item/recipe IDs and live binding unresolved',MissionCode=mission,MissionRegion=regions[0],IngredientBrief=brief,PrefabPath='Tier02/Materials/'+name)
        obj['AssetRole']='crafted Material output art'
        objects.append(obj);items.append(row)
    manifest={'Revision':1,'Batch':7,'Scope':'Three passive Material output assets; no recipes/stats/sources/footprints changed.','BlenderVersion':bpy.app.version_string,'Items':items}
    (kit.ART/'Tier02Batch07Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    views={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    for obj in objects:
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(corners,Vector())/8
        camera.data.ortho_scale=max(max(v[i] for v in corners)-min(v[i] for v in corners) for i in range(3))*1.25
        for view,direction in views.items():
            camera.location=center+Vector(direction);kit.aim(camera,center);scene.render.filepath=str(DOC/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,7,3);kit.aim(camera,(0,0,0));camera.data.ortho_scale=6
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC/'tier02-batch07.blend'))
    print('BATCH07_GENERATED: three Material meshes/palettes, native scene and 18 review views.')


if __name__=='__main__':main()

