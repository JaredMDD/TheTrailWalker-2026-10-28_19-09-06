"""Batch 04: Birch plank, beam, charcoal and panel. Passive art only."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import math,json
import bpy,bmesh
from mathutils import Vector
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import stroke

ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'Docs/Art/Tier02/Batch04'


def bevel_box(m,center,size,face_slot=1,bevel=.009,end_axis=0):
    bm=bmesh.new();result=bmesh.ops.create_cube(bm,size=1)
    for v in result['verts']:
        for i in range(3):v.co[i]*=size[i]
    bmesh.ops.bevel(bm,geom=list(bm.edges),offset=bevel,segments=1,affect='EDGES')
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.normal_update()
    bm.verts.ensure_lookup_table();bm.verts.index_update();offset=len(m.vertices)
    m.vertices += [tuple(v.co[i]+center[i] for i in range(3)) for v in bm.verts]
    for f in bm.faces:
        ids=[v.index+1+offset for v in f.verts]
        slot=3 if abs(f.normal[end_axis])>.95 else face_slot if max(abs(v) for v in f.normal)>.95 else 2
        m.tri(*ids,slot)
    bm.free()


def ellipse_line(m,center,rx,ry,slot=4):
    points=[(center[0]+rx*math.cos(i*math.tau/20),center[1]+ry*math.sin(i*math.tau/20),center[2]) for i in range(20)]
    stroke(m,points+[points[0]],slot,.0017)


def birch_plank():
    m=base.Model('BirchPlank');bevel_box(m,(0,0,0),(1.10,.35,.09),1,.011)
    for sign in (-1,1):
        z=sign*.046
        for y in (-.115,-.04,.10):
            stroke(m,[(-.47,y,z),(-.20,y+.012,z),(.09,y-.004,z),(.45,y+.008,z)],4,.0014)
        ellipse_line(m,(.13,.025,z),.065,.022)
    # Sawn end fibers, distinct from the long face grain.
    for sign in (-1,1):
        for y in (-.07,.06):stroke(m,[(sign*.551,y-.025,-.021),(sign*.551,y+.015,0),(sign*.551,y+.032,.021)],4,.0013)
    return m


def birch_beam():
    m=base.Model('BirchBeam')
    for y in (-.065,.065):
        for z in (-.065,.065):bevel_box(m,(0,y,z),(1.06,.124,.124),1 if y>0 else 0,.006)
    # One recipe cord can be cut into the two visible bindings; no new input.
    for x in (-.30,.30):
        ring=[(x,y,z) for y,z in [(.139,.095),(.095,.139),(-.095,.139),(-.139,.095),(-.139,-.095),(-.095,-.139),(.095,-.139),(.139,-.095)]]
        stroke(m,ring+[ring[0]],5,.014)
    for sign in (-1,1):
        for y in (-.085,.045):stroke(m,[(-.46,y,sign*.128),(-.1,y+.008,sign*.128),(.45,y-.005,sign*.128)],4,.0015)
    return m


def birch_charcoal():
    m=base.Model('BirchCharcoal')
    for cx,cy,cz,length,ry,rz,angle in [(-.19,.035,-.015,.58,.15,.115,-.18),(.20,.04,.025,.48,.16,.13,.34),(.01,-.16,-.015,.43,.11,.13,-.30)]:
        chunk=base.Model('charcoal')
        kit.loft(chunk,[(-length*.5,0,0),(-length*.44,ry*.7,rz*.8),(-length*.20,ry,rz),(length*.22,ry*.82,rz*.93),(length*.48,ry*.68,rz*.65),(length*.5,0,0)],lambda k,i: (k+i)%3,7,True)
        offset=len(m.vertices);co,si=math.cos(angle),math.sin(angle)
        m.vertices += [(cx+x*co-y*si,cy+x*si+y*co,cz+z) for x,y,z in chunk.vertices]
        m.faces += [(a+offset,b+offset,c+offset,k) for a,b,c,k in chunk.faces]
    support=base.Model('charcoal support');support.vertices=list(m.vertices);support.faces=list(m.faces)
    for x,y in [(-.25,.025),(.18,.055),(.005,-.15)]:
        points=[kit.front_surface(support,xx,yy,.001) for xx,yy in [(x-.06,y+.015),(x-.012,y-.01),(x+.045,y+.02)]]
        stroke(m,points,3,.0025)
    return m


def birch_panel():
    m=base.Model('BirchPanel')
    # Three joined boards: no hinges, nails, free furniture or extra plank input.
    for n in range(3):
        x=(n-1)*.245
        bevel_box(m,(x,0,0),(.241,.83,.075),0 if n==1 else 1,.008,1)
        for sign in (-1,1):
            z=sign*.0385
            for dx in (-.062,.045):stroke(m,[(x+dx,-.34,z),(x+dx+.008,-.10,z),(x+dx-.003,.14,z),(x+dx+.006,.34,z)],4,.0013)
    # Thin tongue/joining strips are cut from the same boards and sit in seams.
    for x in (-.1225,.1225):bevel_box(m,(x,0,0),(.016,.78,.045),0,.003,1)
    ellipse_line(m,(-.21,.10,-.0385),.025,.056)
    return m


WOOD=[(211,190,151),(228,208,171),(190,165,125),(205,181,139),(158,131,92),(175,152,104)]
SPECS=[
 ('BirchPlank','Birch plank',birch_plank,WOOD,[7]),
 ('BirchBeam','Birch beam',birch_beam,WOOD,[9]),
 ('BirchCharcoal','Birch charcoal',birch_charcoal,[(43,46,47),(61,63,62),(79,80,77),(104,103,96)], [11]),
 ('BirchPanel','Birch panel',birch_panel,WOOD,[11]),
]
TYPES={'BirchPlank':['wood']*6,'BirchBeam':['wood']*5+['plant_fibre'],'BirchCharcoal':['ash']*4,'BirchPanel':['wood']*6}
RECIPES={'BirchPlank':('T2-L1','3 Birch logs -> 1'),'BirchBeam':('T2-L5','4 Birch planks + 1 Hemp cord -> 1'),'BirchCharcoal':('T2-L11','3 Birch branches -> 2'),'BirchPanel':('T2-L13','3 Birch planks + 1 Beeswax -> 1')}


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
        # Per-output wood, ash and fiber slots reuse the unchanged detail atlas.
        types=TYPES[name]
        obj,row=kit.create_asset(name,colors,types,build());obj.location.x=(index-1.5)*1.85
        mission,brief=RECIPES[name]
        row.update(DisplayName=label,ItemId='',RecipeId='',RuntimeBinding='Passive Material art; canonical item/recipe IDs and live binding unresolved',MissionCode=mission,MissionRegion=regions[0],IngredientBrief=brief,PrefabPath='Tier02/Materials/'+name)
        obj['AssetRole']='crafted Material output art';objects.append(obj);items.append(row)
    manifest={'Revision':1,'Batch':4,'Scope':'Four passive Material output assets; no recipes/stats/sources/footprints changed.','BlenderVersion':bpy.app.version_string,'Items':items}
    (kit.ART/'Tier02Batch04Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    views={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    for obj in objects:
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(corners,Vector())/8
        camera.data.ortho_scale=max(max(v[i] for v in corners)-min(v[i] for v in corners) for i in range(3))*1.25
        for view,direction in views.items():
            camera.location=center+Vector(direction);kit.aim(camera,center);scene.render.filepath=str(DOC/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,7,3);kit.aim(camera,(0,0,0));camera.data.ortho_scale=7.6
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC/'tier02-batch04.blend'))
    print('BATCH04_GENERATED: four Material meshes/palettes, native scene and 24 review views.')


if __name__=='__main__':main()
