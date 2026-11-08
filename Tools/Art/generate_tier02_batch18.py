"""Taiga frame pack: stitched leather structure, fine flax and cattail closures."""
import sys,math,json
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from mathutils import Vector
from mathutils.bvhtree import BVHTree
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import generate_tier02_batch15 as p
from generate_tier02_batch06 import tube
from generate_tier02_batch07 import annulus
from generate_tier02_batch02 import stroke
from tier02_batch_workflow import generate,ROOT

ANCHOR=(0,1.14,-.20)
COLORS=[(139,150,114),(104,120,87),(90,62,42),(127,90,56),(169,155,105),(200,185,135)]
TYPES=['cloth','cloth','leather','leather','plant_fibre','plant_fibre']


def surface_support(female):
    body='Female' if female else 'Male'
    source=p.read_obj(ROOT/f'Assets/_Game/Art/Crafting/Tier02/{body}WearTaigaWaxedShirt.obj')
    front=base.Model('Front');front.vertices=[(x,y,-z) for x,y,z in source.vertices];front.faces=source.faces
    roof=base.Model('Roof');roof.vertices=[(x,z,-y) for x,y,z in source.vertices];roof.faces=source.faces
    tree=BVHTree.FromPolygons([Vector(v) for v in source.vertices],[(a-1,b-1,c-1) for a,b,c,col in source.faces],all_triangles=True)
    def seat_front(x,y):
        q=kit.front_surface(front,x,y,.0025);return(q[0],q[1],-q[2])
    def seat_back(x,y):return kit.front_surface(source,x,y,.0025)
    def seat_top(x,z):
        q=kit.front_surface(roof,x,z,.0025);return(q[0],-q[2],q[1])
    def radial(sign,angle,y):
        direction=Vector((sign*math.cos(angle),0,math.sin(angle)))
        hit,normal,index,distance=tree.ray_cast(Vector((0,y,0))+direction*2,-direction,3)
        if hit is None:raise ValueError('Taiga strap missed shirt')
        if normal.dot(direction)<0:normal=-normal
        return hit+normal*.0025,normal
    return seat_front,seat_back,seat_top,radial


def body(m,female):
    n=24;w=.162 if female else .170;rings=[]
    for y,width,f,b in [(.95,w*.78,.095,.107),(.978,w*.98,.116,.135),(1.08,w,.12,.14),
                         (1.27,w*.96,.116,.13),(1.325,w*.83,.101,.11)]:
        ring=[]
        for i in range(n):
            a=i*math.tau/n;c,s=math.cos(a),math.sin(a)
            ring.append(m.v((width*math.copysign(abs(c)**.62,c),y,-.30+(f if s>0 else b)*math.copysign(abs(s)**.62,s))))
        rings.append(ring)
    for a,b in zip(rings,rings[1:]):
        for i in range(n):
            j=(i+1)%n;slot=1 if i in [1,10,13,22] else 0
            m.tri(a[i],a[j],b[j],slot);m.tri(a[i],b[j],b[i],slot)
    for ring,y in [(rings[0],.95),(rings[-1],1.325)]:
        c=m.v((0,y,-.30))
        for i in range(n):m.tri(c,ring[i],ring[(i+1)%n],0)
    # Recipe-compatible soft frame: two laminated leather layers and stitched perimeter.
    p.box(m,(0,1.15,-.171),(w*1.69,.368,.018),2,.007)
    p.box(m,(0,1.15,-.160),(w*1.57,.347,.008),3,.006)
    for sign in (-1,1):
        x=sign*w*.75
        stroke(m,[(x,y,-.154) for y in [1.00,1.07,1.16,1.25,1.31]],5,.0012)
    # Leather load bands cradle the cloth body and attach into the back structure.
    for sign in (-1,1):
        x=sign*.111
        p.ribbon(m,[(x,1.315,-.158),(x,1.337,-.30),(x,1.285,-.432),
                    (x,1.08,-.443),(x,.980,-.429),(x,.949,-.30),(x,.982,-.169)],.022,.006,2)
    return w


def eyelet(m,center):
    part=base.Model('LeatherEyelet');annulus(part,(0,0,0),.008,.0048,.004,0,16)
    p.append(m,part,lambda v:tuple(v[i]+center[i] for i in range(3)),lambda slot:3)


def pack(female=False,display=False):
    m=base.Model('TaigaFramePack');w=body(m,female)
    # Cloth flap rides over the actual cloth support; no rolled top or metal closure.
    support=base.Model('ClothSupport');support.vertices=list(m.vertices);support.faces=list(m.faces)
    p.ribbon(m,[(0,1.325,-.182),(0,1.331,-.24),(0,1.331,-.32),
                (0,1.312,-.40),(0,1.292,-.435),(0,1.245,-.444),(0,1.185,-.448)],w*1.60,.008,1)
    # Four leather eyelet pairs in the flap/body carry an actual zigzag cattail cord.
    lacing=[]
    for i,y in enumerate([1.26,1.225,1.19,1.155]):
        # Reproject every eyelet after flap construction; lower eyes sit on the sack.
        for sign in (-1,1):
            q=kit.front_surface(m,sign*.037,y,.0015);eyelet(m,q)
        x=(-1 if i%2==0 else 1)*.037
        # Exclude the eyelet geometry itself when finding its cloth support.
        q=kit.front_surface(m,x,y,.0);lacing.append((x,y,q[2]))
    for mirror,slot in [(1,4),(-1,5)]:
        path=[]
        for i,(x,y,z) in enumerate(lacing):
            # Alternate front/back runs; each normal segment actually threads the ring.
            pair=[(x*mirror,y,z+.003),(x*mirror,y,z-.0035)]
            path+=pair if i%2==0 else pair[::-1]
        x,y,z=lacing[-1];path +=[(x*mirror*.97,y-.001,z-.0035),(x*mirror*.55,y-.025,z-.007),(x*mirror*.7,y-.045,z-.004)]
        tube(m,path,.0018,slot,6)
    # Cloth hem seam follows the actual lower sack surface.
    stroke(m,[kit.front_surface(support,x,.995,.0015) for x in [-.11,-.07,0,.07,.11]],1,.0012)
    if display:
        for sign in (-1,1):
            x=sign*.080
            p.ribbon(m,[(x,1.32,-.150),(x,1.24,-.132),(x,1.02,-.134),
                        (x,.987,-.152),(x,1.15,-.155),(x,1.28,-.154)],.027,.007,2)
    else:
        front,back,top,radial=surface_support(female)
        for sign in (-1,1):
            x=sign*(.157 if female else .165)
            for y in [1.08,1.27]:
                touch=back(sign*.085,y)[2];frame=-.152
                p.box(m,(sign*.085,y,(touch+frame)/2),(.057,.051,abs(touch-frame)+.008),2,.005)
            path=[((sign*.112,1.325,-.152),'frame'),(back(x,1.35),'back')]
            path +=[(top(x,z),'top') for z in [-.082,-.05,0,.05,.082]]
            path +=[(front(x,y),'front') for y in [1.33,1.29,1.24,1.18,1.12,1.06]]
            p.projected_shoulder(m,path,.014,front,back,top)
            end=front(x,1.06);p.projected_return(m,sign,math.atan2(end[2],abs(end[0])),radial,(sign*.112,1.06,-.152))
            # Cord stitches through the leather strap; no hidden metal adjuster.
            for y in [1.105,1.115,1.125]:
                stroke(m,[front(x-.007,y),front(x+.007,y)],5,.0011)
    if display:
        center=[(min(v[i] for v in m.vertices)+max(v[i] for v in m.vertices))/2 for i in range(3)]
        m.vertices=[tuple((v[i]-center[i])*1.8 for i in range(3)) for v in m.vertices]
    else:m.vertices=[tuple(v[i]-ANCHOR[i] for i in range(3)) for v in m.vertices]
    return m


SPECS=[('TaigaFramePack','Taiga frame pack',lambda:pack(False,True),COLORS,TYPES),
       ('MaleWearTaigaFramePack','Taiga frame pack (Male fitted)',lambda:pack(False),COLORS,TYPES),
       ('FemaleWearTaigaFramePack','Taiga frame pack (Female fitted)',lambda:pack(True),COLORS,TYPES)]
META={name:{'Representation':'display' if name=='TaigaFramePack' else 'worn',
            'Body':'' if name=='TaigaFramePack' else 'Female' if name.startswith('Female') else 'Male',
            'ItemId':'item.taiga_frame_pack','RecipeId':'recipe.wearable.taiga_frame_pack',
            'Anchor':'Centered display' if name=='TaigaFramePack' else 'BackpackAnchor local; parent (0,1.14,-.20); identity TRS',
            'BindingStatus':'Passive art only; no live equipment/weights/capacity changes'} for name,*_ in SPECS}
RECIPES={name:('T2-E16',12,'3 Fine flax cloth + 2 Fine beaver leather + 2 Cattail cord -> 1') for name,*_ in SPECS}

if __name__=='__main__':
    generate(18,SPECS,RECIPES,category='Wearables',role='Wearable',preview='--preview' in sys.argv,item_metadata=META)
    (ROOT/'Assets/_Game/Art/Crafting/Tier02/Tier02Batch18ArtMap.json').write_text(json.dumps({'Scope':'Passive Taiga pack; no gameplay changes',
        'BackpackAnchor':ANCHOR,'Items':META,'FrameInterpretation':'Stitched laminated-leather soft back structure; no wood or metal',
        'SupportSources':[f'Assets/_Game/Art/Crafting/Tier02/{b}WearTaigaWaxedShirt.obj' for b in ['Male','Female']],
        'DocumentarySetBOnly':{'logical_bag_units':[10,4]},'Pending':'Live binding, dynamic straps/weights, capacity/content/save/device validation'},indent=2)+'\n')
