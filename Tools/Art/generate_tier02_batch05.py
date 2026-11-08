"""Batch 05: beaver leather/pelt refinements, passive Material art only."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import math,json
import bpy
from mathutils import Vector
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import stroke

ROOT=Path(__file__).resolve().parents[2]
DOC=ROOT/'Docs/Art/Tier02/Batch05'


def hide_sheet(m,rx,ry,fine=False,leather=False,center=(0,0,0)):
    """Closed curved hide; broad side authored in the inventory's XY plane."""
    n=40;cx,cy,cz=center
    def edge(i):
        a=i*math.tau/n
        variation=1+(.013 if fine else .04)*math.cos(5*a)+(.006 if fine else .018)*math.sin(7*a)
        return rx*math.cos(a)*variation,ry*math.sin(a)*variation
    front=[];back=[]
    for r in (.42,.86,1):
        front.append([m.v((cx+edge(i)[0]*r,cy+edge(i)[1]*r,cz-.025-(.035 if not leather else .012)*(1-r*r)+(.006 if fine else .014)*r**3*math.sin(i*math.tau/10))) for i in range(n)])
        back.append([m.v((cx+edge(i)[0]*r,cy+edge(i)[1]*r,cz+.018+.007*(1-r))) for i in range(n)])
    fp=m.v((cx,cy,cz-(.06 if not leather else .037)));bp=m.v((cx,cy,cz+.026))
    for i in range(n):
        j=(i+1)%n;m.tri(fp,front[0][i],front[0][j],1 if not leather else 0);m.tri(bp,back[0][j],back[0][i],3)
        for k in range(2):
            slot=(2 if k==1 else 1) if not leather else (1 if k==1 else 0)
            m.tri(front[k][i],front[k+1][i],front[k+1][j],slot);m.tri(front[k][i],front[k+1][j],front[k][j],slot)
            m.tri(back[k][j],back[k+1][j],back[k+1][i],3);m.tri(back[k][j],back[k+1][i],back[k][i],3)
        m.tri(front[-1][i],back[-1][i],back[-1][j],2);m.tri(front[-1][i],back[-1][j],front[-1][j],2)
    return edge


def copy_support(m,back=False):
    support=base.Model('surface support');support.vertices=[(x,y,-z if back else z) for x,y,z in m.vertices];support.faces=list(m.faces)
    return support


def beaver_pelt(fine=False):
    m=base.Model('FineBeaverPelt' if fine else 'CuredBeaverPelt')
    edge=hide_sheet(m,.47,.34,fine)
    support=copy_support(m)
    # Small seated guard-hair wedges; the shader carries the finer fur texture.
    for row in range(5 if fine else 4):
        y=-.20+row*(.095 if fine else .125)
        for col in range(7 if fine else 6):
            x=-.30+col*(.10 if fine else .12)+(row%2)*.016
            if (x/.37)**2+(y/.26)**2>1:continue
            points=[kit.front_surface(support,px,py,.002) for px,py in [(x-.009,y+.01),(x+.009,y+.01),(x,y-.026)]]
            kit.closed_patch(m,points,(0,0,-1),.006,1 if (row+col)%3 else 0)
    if fine:
        reverse=copy_support(m,True)
        for i in range(0,40,2):
            x,y=edge(i);front=kit.front_surface(support,x*.87,y*.87,.002)
            q=kit.front_surface(reverse,x*.90,y*.90,.002);rear=(q[0],q[1],-q[2])
            stroke(m,[front,(x*1.004,y*1.004,.005),rear],4,.0035)
    return m


def leather_roll(m,cx,cy,cz,length,radius):
    """Closed hollow rolled edge: visible inner leather and a real wall thickness."""
    n=20;rings=[]
    for x,r in [(cx-length/2,radius),(cx+length/2,radius),(cx-length/2,radius-.014),(cx+length/2,radius-.014)]:
        rings.append([m.v((x,cy+r*math.cos(i*math.tau/n),cz+r*math.sin(i*math.tau/n))) for i in range(n)])
    for i in range(n):
        j=(i+1)%n
        for a,b,slot in [(0,1,0),(2,3,3),(0,2,2),(1,3,2)]:
            m.tri(rings[a][i],rings[a][j],rings[b][j],slot);m.tri(rings[a][i],rings[b][j],rings[b][i],slot)


def beaver_leather(fine=False):
    m=base.Model('FineBeaverLeather' if fine else 'BeaverLeather')
    hide_sheet(m,.44,.29,fine,True)
    if fine:
        # A tighter second fold sits on the same hide; no thread is added.
        hide_sheet(m,.37,.19,True,True,(.015,-.06,-.032))
        leather_roll(m,0,.115,-.035,.61,.048)
    else:
        leather_roll(m,-.025,.14,-.027,.64,.067)
    support=copy_support(m)
    # Pressed/burnished edge lines are material finish, not a stitched seam.
    for sign in (-1,1):
        points=[kit.front_surface(support,x,y,.001) for x,y in [(-.30,sign*.17),(-.12,sign*.21),(.10,sign*.205),(.29,sign*.16)]]
        stroke(m,points,2,.002 if fine else .003)
    return m


SPECS=[
 ('BeaverLeather','Beaver leather',lambda:beaver_leather(False),[(129,84,51),(153,105,67),(101,64,42),(177,132,85),(155,132,90)],[7]),
 ('CuredBeaverPelt','Cured beaver pelt',lambda:beaver_pelt(False),[(78,54,37),(107,75,48),(163,130,86),(184,151,106),(164,142,99)],[7]),
 ('FineBeaverPelt','Fine beaver pelt',lambda:beaver_pelt(True),[(72,49,34),(98,68,43),(160,127,82),(183,149,103),(213,194,146)],[8]),
 ('FineBeaverLeather','Fine beaver leather',lambda:beaver_leather(True),[(117,73,45),(143,94,57),(86,54,36),(171,125,80),(155,132,90)],[11]),
]
TYPES={name:(['fur','fur','leather','leather','plant_fibre'] if 'Pelt' in name else ['leather']*5) for name,_,_,_,_ in SPECS}
RECIPES={'BeaverLeather':('T2-H3','2 Raw beaver pelts + 1 Rock salt -> 1'),'CuredBeaverPelt':('T2-H4','2 Raw beaver pelts + 1 Rock salt -> 1'),'FineBeaverPelt':('T2-H5','2 Cured beaver pelts + 1 Hemp thread -> 1'),'FineBeaverLeather':('T2-H11','2 Beaver leather + 1 Beeswax -> 1')}


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
        obj,row=kit.create_asset(name,colors,types,build());obj.location.x=(index-1.5)*1.85
        mission,brief=RECIPES[name]
        row.update(DisplayName=label,ItemId='',RecipeId='',RuntimeBinding='Passive Material art; canonical item/recipe IDs and live binding unresolved',MissionCode=mission,MissionRegion=regions[0],IngredientBrief=brief,PrefabPath='Tier02/Materials/'+name)
        obj['AssetRole']='crafted Material output art'
        if name=='FineBeaverLeather':obj.data.materials[0].node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.43
        objects.append(obj);items.append(row)
    manifest={'Revision':1,'Batch':5,'Scope':'Four passive Material output assets; no recipes/stats/sources/footprints changed.','BlenderVersion':bpy.app.version_string,'Items':items}
    (kit.ART/'Tier02Batch05Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    views={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    for obj in objects:
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(v) for v in obj.bound_box];center=sum(corners,Vector())/8
        camera.data.ortho_scale=max(max(v[i] for v in corners)-min(v[i] for v in corners) for i in range(3))*1.25
        for view,direction in views.items():
            camera.location=center+Vector(direction);kit.aim(camera,center);scene.render.filepath=str(DOC/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,7,3);kit.aim(camera,(0,0,0));camera.data.ortho_scale=7.6
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC/'tier02-batch05.blend'))
    print('BATCH05_GENERATED: four Material meshes/palettes, native scene and 24 review views.')


if __name__=='__main__':main()
