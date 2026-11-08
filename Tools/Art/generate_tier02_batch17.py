"""Batch 17: long-sleeve fine-flax Taiga garments, four pockets/hooks."""
import sys,math,json
from pathlib import Path
from functools import lru_cache
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch14 as h
import faceted_people as fp
import faceted_character_kit as k
import generate_player_avatar_meshes as avatar
from mathutils import Vector
from generate_tier02_batch06 import tube
from generate_tier02_batch02 import stroke
from tier02_batch_workflow import generate,ROOT

COLORS=[(93,111,83),(66,84,60),(91,63,42),(127,91,56),
        (182,104,57),(224,154,95),(123,69,39),(147,158,124),(173,160,110)]
PANT_COLORS=[(107,119,100),(76,91,72),*COLORS[2:]]
TYPES=['cloth','cloth','leather','leather','metal','metal','metal','cloth','plant_fibre']


def sleeve(female,side,inner=False):
    mesh=k.Mesh('ContinuousLongSleeve',h.PAL);shr=-.005 if inner else .007
    elbow=-fp.UPPER_ARM
    points=[((0,-.035 if inner else .025,0),.058+shr,.060+shr),
            ((0,-.075,0),.061+shr,.063+shr),((0,-.20,0),.049+shr,.051+shr),
            ((0,elbow,0),.044+shr,.046+shr)]
    for length,rx,rz in [(.055,.049,.050),(.155,.042,.040),(.225,.036,.034),(.255 if inner else .242,.035,.033)]:
        q=fp.rot_x((0,-length,0),-avatar.ELBOW_BEND)
        points.append(((q[0],elbow+q[1],q[2]),rx+shr,rz+shr))
    k.limb(mesh,points,'Flax',sides=12,turn=math.pi/12)
    transform,_,_=avatar.arm_to_root(side,female);mesh.transform(transform)
    return h.convert(mesh)


@lru_cache(None)
def shirt(female=False):
    rows=fp.torso_sections(female)
    rows=[(-.045,*rows[0][1:]),(-.025,*rows[1][1:]),*rows[2:]]
    outer=h.vertical('TaigaTorsoOuter',[(y+.94,w+.009,f+.009,b+.009) for y,w,f,b in rows])
    inner_rows=[(y+.94,w-.002,f-.002,b-.002) for y,w,f,b in rows]
    inner_rows[0]=(inner_rows[0][0]-.06,*inner_rows[0][1:]);inner_rows[-1]=(inner_rows[-1][0]+.06,*inner_rows[-1][1:])
    inner=h.vertical('TaigaTorsoInner',inner_rows)
    m=h.shell([outer,sleeve(female,-1),sleeve(female,1)],[inner,sleeve(female,-1,True),sleeve(female,1,True)],'TaigaWaxedShirt')
    seat=h.front_support(m)
    for cx,cy,w,height in [(-.084,1.225,.101,.100),(.084,1.225,.101,.100),
                             (-.092,1.035,.115,.096),(.092,1.035,.115,.096)]:
        h.patch(m,seat,cx-w/2,cx+w/2,cy-height/2,cy+height/2,1)
        h.patch(m,seat,cx-w/2,cx+w/2,cy+height/2-.018,cy+height/2+.006,2,.0035)
        for x in [cx-w/2+.005,cx+w/2-.005]:
            stroke(m,[seat(x,y,.004) for y in [cy-height/2+.008,cy,cy+height/2-.02]],7,.0010)
    h.patch(m,seat,-.012,.012,.93,1.36,1)
    for y in [1.04,1.15,1.30]:stroke(m,[seat(-.014,y,.004),seat(.014,y,.004)],3,.003)
    # Cloth collar seam follows the actual narrowed neck opening.
    top=rows[-1];y=top[0]+.94;w,f,b=top[1:]
    pts=[]
    for i in range(25):
        a=math.tau*i/24
        # Reuse the actual chamfered cross-section, not an ellipse outside the collar.
        shape=k.chamfer_rect(w+.009,f+.009,b+.009,.70,.62)
        j=(i*8)//24;fraction=(i*8/24)%1;p=shape[j%8];q=shape[(j+1)%8]
        pts.append((p[0]+(q[0]-p[0])*fraction,y-.003,p[1]+(q[1]-p[1])*fraction))
    # Closed seam loop shares its wrap boundary instead of overlapping end caps.
    pts=pts[:-1];rings=[]
    for i,p in enumerate(pts):
        tangent=(Vector(pts[(i+1)%len(pts)])-Vector(pts[i-1])).normalized()
        u=Vector((0,1,0));v=tangent.cross(u).normalized()
        rings.append([m.v(Vector(p)+.0012*(u*math.cos(j*math.tau/5)+v*math.sin(j*math.tau/5))) for j in range(5)])
    for i,a in enumerate(rings):
        b=rings[(i+1)%len(rings)]
        for j in range(5):
            q=(j+1)%5;m.tri(a[j],a[q],b[q],7);m.tri(a[j],b[q],b[j],7)
    return m


@lru_cache(None)
def pants(female=False):
    rows=fp.HIPS[female]
    outer=h.vertical('TaigaSeatOuter',[(y+.88,w+.005,f+.005,b+.005) for y,w,f,b in rows])
    inner_rows=[(y+.88,w-.005,f-.005,b-.005) for y,w,f,b in rows]
    inner_rows[-1]=(inner_rows[-1][0]+.05,*inner_rows[-1][1:]);inner=h.vertical('TaigaSeatInner',inner_rows)
    m=h.shell([outer,h.leg(female,-1),h.leg(female,1)],[inner,h.leg(female,-1,True),h.leg(female,1,True)],'TaigaTrousers')
    seat=h.front_support(m)
    h.patch(m,seat,-.070,.070,.985,1.025,1)
    for sign in (-1,1):
        h.patch(m,seat,sign*.10-.037,sign*.10+.037,.43,.575,1,.0017)
        for a,b in [(.20,.38),(.60,.76)]:stroke(m,[seat(sign*.10,y,.001) for y in [a,(a+b)/2,b]],7,.0011)
    # Four existing Set B hooks on a rail, with cattail loops seated into the side waist.
    width=max(w for y,w,f,b in rows)+.008;x=width+.012
    tube(m,[(x,.94,-.089),(x,.94,.089)],.0045,4,8)
    for z in [-.075,-.025,.025,.075]:
        tube(m,[(x-.015,.94,z),(x+.009,.94,z),(x+.014,.90,z),(x+.014,.875,z),
                (x+.028,.864,z),(x+.039,.875,z),(x+.038,.891,z)],.0033,4,8)
    for z in [-.074,.074]:
        tube(m,[(width-.011,.965,z),(x,.950,z),(x,.929,z),(width-.011,.923,z)],.0035,8,6)
    # Hanging ends identify the Cattail cord without inventing a leather belt.
    tube(m,[(x,.953,.076),(x+.008,.925,.084),(x+.006,.882,.091),(x+.014,.852,.087)],.0021,8,6)
    tube(m,[(x,.953,.076),(x+.003,.920,.069),(x-.002,.872,.074)],.0021,8,6)
    return m


def display(part):
    m=h.copy_model(shirt() if part=='shirt' else pants(),'TaigaDisplay')
    mid=[(min(v[i] for v in m.vertices)+max(v[i] for v in m.vertices))/2 for i in range(3)]
    scale=1.35 if part=='shirt' else .92
    m.vertices=[((x-mid[0])*scale,(y-mid[1])*scale,-(z-mid[2])*scale) for x,y,z in m.vertices]
    return m


SPECS=[('TaigaWaxedShirt','Taiga waxed shirt',lambda:display('shirt'),COLORS,TYPES),('TaigaTrousers','Taiga trousers',lambda:display('pants'),PANT_COLORS,TYPES)]
META={}
for female in (False,True):
    body='Female' if female else 'Male'
    for name,label,fn,colors in [('TaigaWaxedShirt','Taiga waxed shirt',lambda female=female:h.copy_model(shirt(female),'TaigaShirt'),COLORS),
                                 ('TaigaTrousers','Taiga trousers',lambda female=female:h.copy_model(pants(female),'TaigaPants'),PANT_COLORS)]:
        key=body+'Wear'+name;SPECS.append((key,label+' ('+body+' fitted)',fn,colors,TYPES));META[key]={'Representation':'worn','Body':body,'Anchor':'BodyRoot metre coordinates; runtime weights/binding pending'}
for name,*_ in SPECS:
    item='taiga_waxed_shirt' if 'Shirt' in name else 'taiga_trousers'
    META.setdefault(name,{'Representation':'display'});META[name].update(ItemId='item.'+item,RecipeId='recipe.wearable.'+item)
RECIPES={name:('T2-H12',11,'3 Fine flax cloth + 1 Fine beaver leather -> 1') if 'Shirt' in name else
         ('T2-E15',12,'3 Fine flax cloth + 1 Cattail cord + 1 Copper fittings -> 1') for name,*_ in SPECS}

if __name__=='__main__':
    generate(17,SPECS,RECIPES,category='Wearables',role='Wearable',preview='--preview' in sys.argv,item_metadata=META)
    (ROOT/'Assets/_Game/Art/Crafting/Tier02/Tier02Batch17ArtMap.json').write_text(json.dumps({'Scope':'Passive Taiga clothing art; no gameplay edits',
        'Frame':'Body-root metres; +Y up/+Z forward','Items':META,'DocumentarySetBOnly':{'shirt_pockets':4,'pants_hooks':4},
        'Pending':'Runtime weights/bindings, capacity contents/footprints/save reconciliation, current animation/device tests'},indent=2)+'\n')
