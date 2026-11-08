"""Batch 03: three store-input meshes; no live catalogs or recipe writes.

Run through Blender 5.2 with --background --factory-startup --python.
"""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import math,json
import bpy
from mathutils import Vector
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import ellipsoid,stroke

ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'Docs/Art/Tier02/Batch03'


def bud(m,center,radii,color):
    tmp=base.Model('bud')
    kit.loft(tmp,[(-1,0,0),(-.45,.85,.85),(.38,1,1),(1,0,0)],color,5)
    offset=len(m.vertices)
    m.vertices += [tuple(center[i]+p[i]*radii[i] for i in range(3)) for p in tmp.vertices]
    m.faces += [(a+offset,b+offset,c+offset,k) for a,b,c,k in tmp.faces]


def lavender():
    m=base.Model('Lavender')
    ends=[(-.25,.31,-.015),(-.13,.46,.04),(0,.51,-.035),(.14,.43,.025),(.27,.30,-.015)]
    for n,(x,y,z) in enumerate(ends):
        a=((n-2)*.023,-.43,(n%2-.5)*.035);mid=((n-2)*.013,-.12,z*.4)
        m.cylinder(a,mid,.009,.008,6,0,0);m.cylinder(mid,(x,y,z),.008,.004,6,1,1)
        for row in range(4):
            cy=y-.12+row*.04;cx=x*(cy+.12)/(y+.12)
            for j in range(3):
                angle=(j/3+row*.13)*math.tau
                bud(m,(cx+.025*math.cos(angle),cy,z+.020*math.sin(angle)),(.021,.035,.018),2+(j+row+n)%3)
        bud(m,(x,y+.042,z),(.014,.027,.014),4)
        # Narrow leaves grow from the stems; their base is embedded, not floating.
        for sign in (-1,1):
            start=(mid[0],-.01,z*.7)
            tip=(mid[0]+sign*.105,.14,z*.7)
            points=[start,(mid[0]+sign*.055,.065,z*.7-.014),tip,(mid[0]+sign*.04,.075,z*.7+.008)]
            kit.closed_patch(m,points,(0,0,-1),.005,1)
    # Two close-fitting fiber bindings around the gathered stems.
    for y in (-.19,-.155):
        kit.loft(m,[(y-.01,0,0),(y-.01,.087,.053),(y+.01,.087,.053),(y+.01,0,0)],5,14)
    ellipsoid(m,(.025,-.17,-.058),(.025,.025,.015),5,6)
    return m


def crab():
    m=base.Model('Crab')
    # The carapace faces the inventory camera (-Z); walking legs fan in XY.
    ellipsoid(m,(0,0,0),(.35,.23,.105),0,16)
    ellipsoid(m,(0,.005,-.047),(.30,.185,.077),1,16)
    for sign in (-1,1):
        for y,elbow,tip,extent in [(.12,.25,.18,.75),(.035,.05,-.04,.82),(-.065,-.17,-.33,.78),(-.15,-.29,-.47,.64)]:
            a=(sign*.27,y,.015);b=(sign*.43,elbow,.006);c=(sign*(extent-.08),tip+.06,.02);d=(sign*extent,tip,.025)
            m.cylinder(a,b,.028,.023,6,2,2);m.cylinder(b,c,.023,.012,6,2,2);m.cylinder(c,d,.012,.002,5,3,3)
        # Two chelipeds in addition to the four walking-leg pairs.
        a=(sign*.23,.15,.01);b=(sign*.36,.29,-.01);c=(sign*.43,.42,-.015)
        m.cylinder(a,b,.043,.036,7,2,2);m.cylinder(b,c,.036,.035,7,2,2)
        ellipsoid(m,(sign*.43,.45,-.018),(.078,.105,.060),2,10)
        stroke(m,[(sign*.385,.49,-.023),(sign*.36,.56,-.023),(sign*.395,.62,-.023)],3,.020)
        stroke(m,[(sign*.475,.49,-.023),(sign*.49,.57,-.023),(sign*.435,.625,-.023)],3,.022)
        # Seated eyestalks, kept small rather than cartoon googly eyes.
        m.cylinder((sign*.09,.16,-.05),(sign*.115,.255,-.063),.011,.009,6,3,3)
        ellipsoid(m,(sign*.115,.255,-.065),(.016,.016,.014),4,6)
    for i in range(12):
        angle=i*math.tau/12
        bud(m,(.327*math.cos(angle),.212*math.sin(angle),-.015),(.023,.018,.025),2)
    support=base.Model('CrabSupport');support.vertices=list(m.vertices);support.faces=list(m.faces)
    for x,y in [(-.17,.055),(-.085,-.06),(.035,.10),(.14,.01),(.07,-.095)]:
        p=kit.front_surface(support,x,y,.001)
        bud(m,p,(.026,.018,.003),5)
    return m


def duck_meat():
    m=base.Model('DuckMeat')
    rows=[(-.44,0,0),(-.40,.155,.075),(-.29,.24,.12),(-.08,.255,.14),(.17,.19,.115),(.34,.095,.065),(.43,0,0)]
    # Skin covers the front face, unlike the narrow fat band on the beaver cut.
    def color(k,i):
        angle=(i+.5)*math.tau/20;front=math.sin(angle)
        if front<0 and math.cos(angle)<-.55:return 3 if k%2 else 4
        if front<-.35:return 0 if k%3 else 1
        if front<-.05:return 2
        return 3 if k%2 else 4
    kit.loft(m,rows,color,20,True)
    support=base.Model('DuckSupport');support.vertices=list(m.vertices);support.faces=list(m.faces)
    for x,y in [(-.26,-.055),(-.24,.055),(-.15,-.095),(-.12,.015),(-.10,.12),(-.015,-.11),(.02,-.01),(.035,.10),(.13,-.07),(.17,.035),(.25,.02)]:
        p=kit.front_surface(support,x,y,.001)
        bud(m,p,(.0045,.004,.0015),5)
    return m


SPECS=[
 ('Lavender','Lavender',lavender,[(92,111,62),(132,152,92),(83,57,121),(120,80,163),(157,116,187),(185,151,99)],[10]),
 ('Crab','Crab',crab,[(105,88,64),(129,106,79),(154,128,93),(66,55,48),(20,24,22),(106,85,66)],[]),
 ('DuckMeat','Duck meat',duck_meat,[(232,202,183),(220,183,169),(236,208,177),(143,58,64),(116,43,53),(199,157,144)],[]),
]
TYPES={'Lavender':['plant_fibre','plant_fibre','flat','flat','flat','plant_fibre'],'Crab':['flat']*6,'DuckMeat':['flat']*6}
SOURCES={'Lavender':('Farm','Discover region 10',''),'Crab':('Fishery','Discover a tier-two region',''),'DuckMeat':('Ranch','Discover a tier-two region','ranch.duck_meat')}


def main():
    kit.DOC=DOC;kit.ART=ROOT/'Assets/_Game/Art/Items/Tier02Design/ShopInputs'
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
        # Catfish skin is smooth; fins/eyes stay flat on every species.
        types=TYPES[name]
        obj,row=kit.create_asset(name,colors,types,build());obj.location.x=(index-1)*1.85
        service,gate,item_id=SOURCES[name]
        row.update(DisplayName=label,ItemId=item_id,RuntimeBinding='Passive art only; runtime IDs/stock binding need content review',SourceService=service,SourceGate=gate,SourceRegions=regions,PrefabPath='Tier02/ShopInputs/'+name)
        obj['AssetRole']='raw shop-input inventory art';objects.append(obj);items.append(row)
    manifest={'Revision':1,'Batch':3,'Scope':'Three passive shop-input assets; no recipes/stats/sources/footprints changed.','BlenderVersion':bpy.app.version_string,'Items':items}
    (kit.ART/'Tier02Batch03Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    views={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    for obj in objects:
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(corners,Vector())/8
        camera.data.ortho_scale=max(max(v[i] for v in corners)-min(v[i] for v in corners) for i in range(3))*1.25
        for view,direction in views.items():
            camera.location=center+Vector(direction);kit.aim(camera,center);scene.render.filepath=str(DOC/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,7,3);kit.aim(camera,(0,0,0));camera.data.ortho_scale=7.6
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC/'tier02-batch03.blend'))
    print('BATCH03_GENERATED: three shop-input meshes/palettes, native scene and 18 review views.')


if __name__=='__main__':main()
