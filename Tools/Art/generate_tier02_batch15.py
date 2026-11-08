"""Batch 15: Marsh pack, separate display and body-projected fitted straps."""
import sys,math,json
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from mathutils import Vector
from mathutils.bvhtree import BVHTree
import bpy,bmesh
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch06 import tube
from generate_tier02_batch07 import annulus
from generate_tier02_batch02 import stroke
from tier02_geometry import beveled_profile
from tier02_batch_workflow import generate,ROOT

ANCHOR=(0,1.14,-.20)
COLORS=[(189,169,120),(140,121,82),(111,73,46),(153,104,62),(201,177,126),
        (153,125,79),(182,102,54),(230,156,96),(127,69,39)]
TYPES=['cloth','cloth','leather','leather','wood','wood','metal','metal','metal']


def append(m,source,fn=lambda p:p,map_slot=lambda k:k):
    offset=len(m.vertices);m.vertices +=[fn(p) for p in source.vertices]
    m.faces +=[(a+offset,b+offset,c+offset,map_slot(col)) for a,b,c,col in source.faces]


def read_obj(path):
    model=base.Model('ReadOnlyBodySupport')
    for line in path.read_text().splitlines():
        words=line.split()
        if not words:continue
        if words[0]=='v':model.v(tuple(float(v) for v in words[1:4]))
        elif words[0]=='f':model.tri(*[int(v.split('/')[0]) for v in words[1:4]],0)
    return model


def surface_support(female):
    body='Female' if female else 'Male'
    source=read_obj(ROOT/f'Assets/_Game/Art/Crafting/Tier02/{body}WearFlaxWorkShirt.obj')
    front=base.Model('Front');front.vertices=[(x,y,-z) for x,y,z in source.vertices];front.faces=source.faces
    top=base.Model('Top');top.vertices=[(x,z,-y) for x,y,z in source.vertices];top.faces=source.faces
    tree=BVHTree.FromPolygons([Vector(p) for p in source.vertices],[(a-1,b-1,c-1) for a,b,c,col in source.faces],all_triangles=True)
    def seat_front(x,y):
        p=kit.front_surface(front,x,y,.0025);return (p[0],p[1],-p[2])
    def seat_back(x,y):return kit.front_surface(source,x,y,.0025)
    def seat_top(x,z):
        p=kit.front_surface(top,x,z,.0025);return (p[0],-p[2],p[1])
    def seat_radial(sign,angle,y):
        direction=Vector((sign*math.cos(angle),0,math.sin(angle)))
        hit,normal,index,distance=tree.ray_cast(Vector((0,y,0))+direction*2,-direction,3)
        if hit is None:raise ValueError('Body wrap missed shirt at '+str((sign,angle,y)))
        if normal.dot(direction)<0:normal=-normal
        return hit+normal*.0025,normal
    return seat_front,seat_back,seat_top,seat_radial


def ribbon(m,points,width,thickness,slot,closed=False):
    rings=[]
    for i,p in enumerate(points):
        previous=points[(i-1)%len(points)] if closed else points[max(0,i-1)]
        following=points[(i+1)%len(points)] if closed else points[min(len(points)-1,i+1)]
        tangent=(Vector(following)-Vector(previous)).normalized()
        across=Vector((1,0,0));normal=across.cross(tangent).normalized()
        rings.append([m.v(tuple(Vector(p)+across*x*width/2+normal*z*thickness/2)) for x,z in [(-1,-1),(1,-1),(1,1),(-1,1)]])
    count=len(rings) if closed else len(rings)-1
    for i in range(count):
        a,b=rings[i],rings[(i+1)%len(rings)]
        for j in range(4):
            k=(j+1)%4;m.tri(a[j],a[k],b[k],slot);m.tri(a[j],b[k],b[j],slot)
    if not closed:
        for ring,p in [(rings[0],points[0]),(rings[-1],points[-1])]:
            c=m.v(p)
            for j in range(4):m.tri(c,ring[j],ring[(j+1)%4],slot)


def box(m,center,size,slot,bevel=.004):
    w,h,d=size;x,y,z=center;r=min(bevel,w*.25,h*.25)
    shape=[(-w/2+r,-h/2),(w/2-r,-h/2),(w/2,-h/2+r),(w/2,h/2-r),
           (w/2-r,h/2),(-w/2+r,h/2),(-w/2,h/2-r),(-w/2,-h/2+r)]
    part=base.Model('WorkedPanel');beveled_profile(part,shape,d,slot,slot,min(r,d*.2))
    # Clamp-overlap bevel corners can collapse at OBJ precision: clean only this new panel.
    mesh=bpy.data.meshes.new('PanelCleanup');mesh.from_pydata(part.vertices,[],[(a-1,b-1,c-1) for a,b,c,col in part.faces]);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh)
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000001)
    bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.0000005)
    bmesh.ops.triangulate(bm,faces=list(bm.faces));bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    part=base.Model('WorkedPanel');part.vertices=[tuple(v.co) for v in mesh.vertices]
    part.faces=[(*[i+1 for i in p.vertices],slot) for p in mesh.polygons];bpy.data.meshes.remove(mesh)
    append(m,part,lambda p:(x+p[0],y+p[1],z+p[2]))


def projected_shoulder(m,path,width,front,back,top):
    rings=[]
    for p,kind in path:
        ring=[]
        for across,depth in [(-1,-1),(1,-1),(1,1),(-1,1)]:
            if kind=='front':q=Vector(front(p[0]+across*width/2,p[1]));normal=Vector((0,0,1))
            elif kind=='back':q=Vector(back(p[0]+across*width/2,p[1]));normal=Vector((0,0,-1))
            elif kind=='top':q=Vector(top(p[0]+across*width/2,p[2]));normal=Vector((0,1,0))
            else:q=Vector(p)+Vector((across*width/2,0,0));normal=Vector((0,0,-1))
            ring.append(m.v(tuple(q+normal*depth*.003)))
        rings.append(ring)
    finish_rings(m,rings,2)


def finish_rings(m,rings,slot):
    for a,b in zip(rings,rings[1:]):
        for j in range(4):
            k=(j+1)%4;m.tri(a[j],a[k],b[k],slot);m.tri(a[j],b[k],b[j],slot)
    for ring in (rings[0],rings[-1]):
        p=sum((Vector(m.vertices[i-1]) for i in ring),Vector())/4;c=m.v(tuple(p))
        for j in range(4):m.tri(c,ring[j],ring[(j+1)%4],slot)


def projected_return(m,sign,start_angle,radial,frame):
    rings=[]
    for angle in [start_angle,.30,0,-.30,-.60,-.85]:
        ring=[]
        for across,depth in [(-1,-1),(1,-1),(1,1),(-1,1)]:
            q,normal=radial(sign,angle,1.06+across*.009)
            ring.append(m.v(tuple(q+normal*depth*.003)))
        rings.append(ring)
    x,y,z=frame
    rings.append([m.v((x,y+across*.009,z+depth*.003)) for across,depth in [(-1,-1),(1,-1),(1,1),(-1,1)]])
    finish_rings(m,rings,2)


def bag(m,female):
    n=24;w=.153 if female else .160;rings=[]
    rows=[(.95,w*.76,.090,.105),(.975,w*.96,.110,.118),(1.06,w,.113,.120),
          (1.24,w,.108,.117),(1.305,w*.90,.100,.108)]
    for y,width,front,back in rows:
        rings.append([m.v((width*math.copysign(abs(math.cos(i*math.tau/n))**.62,math.cos(i*math.tau/n)),y,
                           -.29+(front if math.sin(i*math.tau/n)>0 else back)*math.copysign(abs(math.sin(i*math.tau/n))**.62,math.sin(i*math.tau/n)))) for i in range(n)])
    for a,b in zip(rings,rings[1:]):
        for i in range(n):
            j=(i+1)%n;col=1 if i in (1,10,13,22) else 0
            m.tri(a[i],a[j],b[j],col);m.tri(a[i],b[j],b[i],col)
    for ring,y in [(rings[0],rows[0][0]),(rings[-1],rows[-1][0])]:
        c=m.v((0,y,-.29))
        for i in range(n):m.tri(c,ring[i],ring[(i+1)%n],0)
    # Rolled mouth stays seated on the closed upper canvas form.
    roll=base.Model('RolledCanvas')
    kit.loft(roll,[(-w*.94,0,0),(-w*.94,.019,.045),(-w*.86,.026,.053),
                   (w*.86,.026,.053),(w*.94,.019,.045),(w*.94,0,0)],0,20,along_x=True)
    append(m,roll,lambda p:(p[0],p[1]+1.323,p[2]-.29))
    # A rear pocket grows from the bag's outer surface rather than hovering behind it.
    pocket=base.Model('RearPocket')
    kit.loft(pocket,[(1.005,0,0),(1.005,.065,.020),(1.025,.085,.035),
                     (1.14,.085,.035),(1.16,.074,.027),(1.16,0,0)],0,20)
    append(m,pocket,lambda p:(p[0],p[1],p[2]-.423))
    box(m,(0,1.158,-.455),(.174,.023,.013),2)


def hardware(m,x,y,z,front=True):
    # Buckle is an actual annular opening, with strap beneath its central bar.
    part=base.Model('CopperBuckle');annulus(part,(0,0,0),1,.62,.32,0,20)
    mapping={0:6,1:7,2:7,3:8}
    append(m,part,lambda p:(x+p[0]*.016,y+p[1]*.022,z+p[2]*.016),lambda col:mapping[col])
    m.cylinder((x-.013,y,z),(x+.013,y,z),.0025,.0025,8,7,7)


def pack(female=False,display=False):
    name='MarshPack' if display else ('Female' if female else 'Male')+'WearMarshPack'
    m=base.Model(name);w=.153 if female else .160;bag(m,female)
    # Birch rails/crossbars are cut from the authored planks, not an extra wood input.
    rail=w-.025
    for sign in (-1,1):box(m,(sign*rail,1.17,-.169),(.026,.43,.026),4,.005)
    for y in (1.065,1.285):box(m,(0,y,-.170),(rail*2+.035,.024,.026),4,.005)
    for sign in (-1,1):
        x=sign*rail
        for y in (1.06,1.285):
            ribbon(m,[(x,y-.015,-.148),(x,y+.015,-.148),(x,y+.015,-.230),(x,y-.015,-.230)],.026,.005,2,True)
    # Rear closure straps project onto the actual bag at every sample.
    support=base.Model('BagSupport');support.vertices=list(m.vertices);support.faces=list(m.faces)
    for sign in (-1,1):
        x=sign*.105
        pts=[kit.front_surface(support,x,y,.003) for y in (1.30,1.27,1.23,1.19,1.15,1.11)]
        ribbon(m,pts,.022,.005,2);p=kit.front_surface(support,x,1.175,.006)
        hardware(m,p[0],p[1],p[2])
    if display:
        # Folded shoulder straps lie against the body-side frame in the loose display.
        for sign in (-1,1):
            x=sign*.085
            ribbon(m,[(x,1.34,-.15),(x,1.24,-.132),(x,1.05,-.135),
                      (x,1.005,-.155),(x,1.16,-.155),(x,1.29,-.157)],.026,.006,2)
    else:
        front,back,top,radial=surface_support(female)
        for sign in (-1,1):
            x=sign*(.154 if female else .162)
            # Pads bridge the exact current-shirt back to the birch crossbars.
            for y in (1.08,1.275):
                touch=back(sign*.085,y)[2];frame=-.170
                box(m,(sign*.085,y,(touch+frame)/2),(.057,.050,abs(touch-frame)+.008),2,.005)
            # Path: frame -> shoulder back -> actual shoulder roof -> chest -> lower frame.
            points=[((sign*rail,1.355,-.170),'frame'),(back(x,1.35),'back')]
            for z in (-.082,-.050,0,.050,.082):points.append((top(x,z),'top'))
            for y in (1.33,1.29,1.24,1.18,1.12,1.06):points.append((front(x,y),'front'))
            projected_shoulder(m,points,.018,front,back,top)
            end=front(x,1.06)
            projected_return(m,sign,math.atan2(end[2],abs(end[0])),radial,(sign*rail,1.06,-.170))
            p=front(x,1.115);hardware(m,p[0],p[1],p[2]+.003,False)
    # Readable hem seams sit directly on the bag-facing canvas.
    for y in (1.02,1.24):
        pts=[kit.front_surface(support,x,y,.0015) for x in (-.12,-.08,-.04,0,.04,.08,.12)]
        stroke(m,pts,1,.0012)
    if display:
        center=[(min(p[i] for p in m.vertices)+max(p[i] for p in m.vertices))/2 for i in range(3)]
        m.vertices=[tuple((p[i]-center[i])*1.8 for i in range(3)) for p in m.vertices]
    else:m.vertices=[tuple(p[i]-ANCHOR[i] for i in range(3)) for p in m.vertices]
    return m


SPECS=[('MarshPack','Marsh pack',lambda:pack(False,True),COLORS,TYPES),
       ('MaleWearMarshPack','Marsh pack (Male fitted)',lambda:pack(False),COLORS,TYPES),
       ('FemaleWearMarshPack','Marsh pack (Female fitted)',lambda:pack(True),COLORS,TYPES)]
RECIPES={name:('T2-E6',9,'4 Hemp cloth + 3 Beaver leather + 2 Birch planks + 2 Copper fittings -> 1') for name,*_ in SPECS}
META={name:{'Representation':'display' if name=='MarshPack' else 'worn','Body':'' if name=='MarshPack' else 'Female' if name.startswith('Female') else 'Male',
            'ItemId':'item.marsh_pack','RecipeId':'recipe.wearable.marsh_pack','Anchor':'Centered inventory display' if name=='MarshPack' else 'BackpackAnchor local; parent at (0,1.14,-0.20); identity TRS',
            'BindingStatus':'Passive art only; runtime equipment/strap weighting and phone tests pending'} for name,*_ in SPECS}


if __name__=='__main__':
    generate(15,SPECS,RECIPES,category='Wearables',role='Wearable',preview='--preview' in sys.argv,item_metadata=META)
    (ROOT/'Assets/_Game/Art/Crafting/Tier02/Tier02Batch15ArtMap.json').write_text(json.dumps({'Scope':'Passive Marsh pack art; no gameplay changes','BackpackAnchor':ANCHOR,'Items':META,
        'SupportSources':['Assets/_Game/Art/Crafting/Tier02/MaleWearFlaxWorkShirt.obj','Assets/_Game/Art/Crafting/Tier02/FemaleWearFlaxWorkShirt.obj'],
        'DocumentarySetAOnly':{'logical_bag_units':[9,4]},'Pending':'Live binding, dynamic strap attachment/weights, saved inventory migration and current device tests'},indent=2)+'\n')
