"""Batch 02: four distinct raw fish, passive art only. Run with Blender 5.2."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import math,json
import bpy
from mathutils import Vector,geometry
import generate_tier02_batch01 as kit
import generate_region01_meshes as base

ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'Docs/Art/Tier02/Batch02'


def fin(m, outline, color, depth=.012):
    """Thin closed fin, including concave tail notches and dorsal spines."""
    pts=[Vector(p) for p in outline]
    front=[m.v(tuple(p)) for p in pts];back=[m.v((p.x,p.y,p.z+depth)) for p in pts]
    lookup={tuple(p):i for i,p in enumerate(pts)}
    for triangle in geometry.tessellate_polygon([pts]):
        a,b,c=tuple(triangle) if isinstance(triangle[0],int) else [lookup[tuple(v)] for v in triangle]
        m.tri(front[a],front[b],front[c],color);m.tri(back[c],back[b],back[a],color)
    for i in range(len(pts)):
        j=(i+1)%len(pts)
        m.tri(front[i],back[i],back[j],color);m.tri(front[i],back[j],front[j],color)


def ellipsoid(m,center,radii,color,sides=8):
    tmp=base.Model('detail')
    rows=[(-1,0,0)]+[(math.cos(math.pi*(1-j/5)),math.sin(math.pi*j/5),math.sin(math.pi*j/5)) for j in range(1,5)]+[(1,0,0)]
    kit.loft(tmp,rows,color,sides)
    offset=len(m.vertices)
    m.vertices += [tuple(center[i]+p[i]*radii[i] for i in range(3)) for p in tmp.vertices]
    m.faces += [(a+offset,b+offset,c+offset,k) for a,b,c,k in tmp.faces]


def stroke(m,points,color=7,radius=.003):
    for a,b in zip(points,points[1:]):m.cylinder(a,b,radius,radius*.85,5,color,color)


def eyes(m,x,y,z,size,iris=5):
    for sign in (-1,1):
        ellipsoid(m,(x,y,sign*z),(size,size*1.12,.012),iris)
        ellipsoid(m,(x+.003,y,sign*(z+.009)),(size*.48,size*.64,.006),6)
        ellipsoid(m,(x+.006,y+size*.27,sign*(z+.015)),(size*.13,size*.16,.003),8,6)


def common_carp():
    m=base.Model('CommonCarp')
    rows=[(-.40,0,0),(-.34,.045,.035),(-.23,.17,.095),(-.06,.235,.125),(.12,.22,.115),(.29,.14,.085),(.39,.06,.05),(.43,0,0)]
    kit.loft(m,rows,lambda k,i: 2 if math.cos((i+.5)*math.tau/18)<-.4 else 1 if k%2 else 0,18,True)
    fin(m,[(-.36,.035,-.004),(-.62,.17,-.004),(-.55,0,-.004),(-.62,-.17,-.004),(-.36,-.035,-.004)],3)
    fin(m,[(-.26,.14,0),(-.21,.31,0),(-.12,.34,0),(-.02,.30,0),(.19,.27,0),(.23,.17,0)],3)
    fin(m,[(-.16,-.18,0),(-.23,-.30,0),(.02,-.24,0),(.11,-.19,0)],3)
    for sign in (-1,1):
        fin(m,[(.18,-.04,sign*.095),(-.04,-.19,sign*.16),(.03,-.04,sign*.12)],3)
        stroke(m,[(.28,.11,sign*.066),(.235,.015,sign*.101),(.28,-.09,sign*.073)],7)
        stroke(m,[(.385,-.01,sign*.036),(.44,-.055,sign*.065),(.43,-.095,sign*.07)],7,.004)
        # Several readable scale arcs, seated on the real side surface.
        for x in (-.18,-.06,.065,.17):
            for y in (-.06,.035,.115):
                pts=[kit.front_surface(m,xx,yy,.0015) for xx,yy in [(x-.016,y+.016),(x-.027,y),(x-.016,y-.016)]]
                if sign>0:pts=[(a,b,-c) for a,b,c in pts]
                stroke(m,pts,4,.002)
    for x,y in [(-.19,.29),(-.08,.31),(.05,.27),(.16,.25)]:stroke(m,[(x,.19,-.008),(x,y,-.008)],4,.002)
    eyes(m,.32,.055,.073,.026)
    return m


def northern_pike():
    m=base.Model('NorthernPike')
    rows=[(-.52,0,0),(-.47,.045,.033),(-.34,.115,.065),(-.08,.135,.074),(.20,.12,.07),(.38,.09,.062),(.49,.065,.06),(.64,.048,.057),(.69,0,0)]
    kit.loft(m,rows,lambda k,i: 2 if math.cos((i+.5)*math.tau/18)<-.45 else 1 if math.cos((i+.5)*math.tau/18)<.4 else 0,18,True)
    fin(m,[(-.48,.035,0),(-.73,.16,0),(-.65,0,0),(-.73,-.16,0),(-.48,-.035,0)],3)
    fin(m,[(-.38,.10,0),(-.33,.265,0),(-.18,.25,0),(-.12,.13,0)],3)
    fin(m,[(-.35,-.09,0),(-.39,-.25,0),(-.14,-.18,0),(-.12,-.11,0)],3)
    for sign in (-1,1):
        fin(m,[(.29,-.035,sign*.05),(.13,-.18,sign*.10),(.19,-.06,sign*.068)],3)
        stroke(m,[(.50,-.015,sign*.056),(.60,-.017,sign*.05),(.665,-.004,sign*.027)],7,.0035)
        stroke(m,[(.35,.07,sign*.04),(.32,0,sign*.065),(.35,-.065,sign*.045)],7,.0025)
        for x,y in [(-.30,.04),(-.19,-.035),(-.08,.05),(.025,-.035),(.14,.035),(.25,-.025)]:
            a,b,z=kit.front_surface(m,x,y,0)
            ellipsoid(m,(a,b,z if sign<0 else -z),(.026,.011,.0035),4,6)
    eyes(m,.445,.032,.060,.020)
    return m


def channel_catfish():
    m=base.Model('ChannelCatfish')
    rows=[(-.46,0,0),(-.39,.04,.025),(-.25,.12,.075),(-.02,.155,.105),(.19,.15,.14),(.34,.105,.145),(.43,.055,.10),(.46,0,0)]
    kit.loft(m,rows,lambda k,i:2 if math.cos((i+.5)*math.tau/18)<-.40 else 0 if k%2 else 1,18,True)
    fin(m,[(-.405,.025,0),(-.68,.21,0),(-.56,0,0),(-.68,-.21,0),(-.405,-.025,0)],3)
    fin(m,[(.015,.15,0),(.10,.32,0),(.18,.24,0),(.24,.14,0)],3)
    fin(m,[(-.34,.075,0),(-.27,.15,0),(-.17,.16,0),(-.10,.13,0)],3)
    fin(m,[(-.31,-.08,0),(-.31,-.21,0),(.05,-.22,0),(.12,-.13,0)],3)
    for sign in (-1,1):
        fin(m,[(.27,-.035,sign*.12),(.05,-.20,sign*.22),(.07,-.045,sign*.13)],3)
        # Mouth barbels, nose barbels and chin barbels; all rooted in the head.
        for y,z,dy,dz in [(-.018,.112,-.06,.12),(.03,.095,.055,.085),(-.06,.065,-.13,.04),(-.045,.025,-.13,.035)]:
            stroke(m,[(.36,y,sign*z),(.48,y+dy*.45,sign*(z+dz*.65)),(.58,y+dy,sign*(z+dz))],4,.005)
        for x,y in [(-.21,.025),(-.08,.055),(.04,.02),(.13,.075)]:
            p=kit.front_surface(m,x,y,0)
            ellipsoid(m,(p[0],p[1],p[2] if sign<0 else -p[2]),(.012,.012,.003),7,6)
        stroke(m,[(.405,-.01,sign*.10),(.44,-.02,sign*.07)],7,.004)
    eyes(m,.31,.06,.12,.024)
    return m


def walleye():
    m=base.Model('Walleye')
    rows=[(-.43,0,0),(-.37,.04,.028),(-.25,.12,.065),(-.06,.175,.085),(.13,.17,.083),(.29,.12,.075),(.41,.055,.047),(.46,0,0)]
    kit.loft(m,rows,lambda k,i:2 if math.cos((i+.5)*math.tau/18)<-.45 else 0 if k%2==0 else 1,18,True)
    fin(m,[(-.39,.025,0),(-.65,.17,0),(-.58,0,0),(-.65,-.18,0),(-.39,-.025,0)],3)
    fin(m,[(-.11,.17,0),(-.075,.34,0),(-.025,.25,0),(.015,.36,0),(.055,.25,0),(.10,.34,0),(.14,.24,0),(.185,.30,0),(.21,.15,0)],3)
    fin(m,[(-.32,.09,0),(-.27,.23,0),(-.14,.25,0),(-.13,.17,0)],3)
    fin(m,[(-.17,-.15,0),(-.24,-.28,0),(.01,-.21,0),(.08,-.16,0)],3)
    # Pale lower tail tip is a species cue, not a quality indicator.
    fin(m,[(-.64,-.17,-.002),(-.575,-.108,-.002),(-.62,-.105,-.002)],8,.005)
    for sign in (-1,1):
        fin(m,[(.18,-.025,sign*.064),(.01,-.19,sign*.14),(.055,-.055,sign*.08)],3)
        stroke(m,[(.28,.11,sign*.043),(.225,.015,sign*.077),(.28,-.09,sign*.048)],7)
        stroke(m,[(.36,-.02,sign*.055),(.438,-.012,sign*.025)],7,.0035)
        for x,y in [(-.22,.075),(-.08,.12),(.07,.12)]:
            p=kit.front_surface(m,x,y,0)
            ellipsoid(m,(p[0],p[1],p[2] if sign<0 else -p[2]),(.020,.015,.003),7,6)
    for x,y in [(-.07,.33),(.015,.35),(.10,.33),(.18,.28)]:stroke(m,[(x,.17,-.008),(x,y,-.008)],4,.002)
    eyes(m,.31,.050,.071,.033)
    return m


SPECS=[
 ('CommonCarp','Common carp',common_carp,[(138,112,53),(171,140,73),(211,186,126),(143,105,53),(107,80,37),(224,181,66),(24,29,24),(80,65,36),(239,226,177)],[7,9,11]),
 ('NorthernPike','Northern pike',northern_pike,[(83,103,56),(117,139,73),(197,204,145),(117,124,63),(220,218,146),(213,187,74),(23,30,22),(61,73,43),(236,231,184)],[8,9,12]),
 ('ChannelCatfish','Channel catfish',channel_catfish,[(85,104,116),(104,121,129),(196,201,187),(93,105,109),(159,170,158),(181,165,105),(21,29,32),(50,69,82),(225,224,197)],[7,10,12]),
 ('Walleye','Walleye',walleye,[(118,115,58),(157,148,77),(214,207,154),(131,120,58),(192,168,84),(224,189,64),(25,32,24),(74,78,42),(234,225,178)],[8,10,11]),
]


def main():
    kit.DOC=DOC
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
        types=['flat']*len(colors)
        if name!='ChannelCatfish':types[:3]=['fish_scales']*3
        obj,row=kit.create_asset(name,colors,types,build());obj.location.x=(index-1.5)*1.85
        row.update(DisplayName=label,ItemId='',RuntimeBinding='Passive raw fish art; canonical runtime IDs/bindings unresolved',SourceRegions=regions)
        obj['AssetRole']='raw fish inventory art';objects.append(obj);items.append(row)
    manifest={'Revision':1,'Batch':2,'Scope':'Four passive raw fish assets; no recipes/stats/sources/footprints changed.','BlenderVersion':bpy.app.version_string,'Items':items}
    (kit.ART/'Tier02Batch02Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    views={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    for obj in objects:
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(corners,Vector())/8
        camera.data.ortho_scale=max(max(v[i] for v in corners)-min(v[i] for v in corners) for i in range(3))*1.25
        for view,direction in views.items():
            camera.location=center+Vector(direction);kit.aim(camera,center);scene.render.filepath=str(DOC/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,7,3);kit.aim(camera,(0,0,0));camera.data.ortho_scale=7.6
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC/'tier02-batch02.blend'))
    print('BATCH02_GENERATED: four fish meshes/palettes, native scene and 24 review views.')


if __name__=='__main__':main()
