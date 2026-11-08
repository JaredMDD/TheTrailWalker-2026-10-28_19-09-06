"""Batch 14: connected flax work shirt/trousers, display + fitted body variants."""
import sys,math,json
from pathlib import Path
from functools import lru_cache
sys.path.insert(0,str(Path(__file__).parent))
import bpy,bmesh
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import faceted_people as fp
import faceted_character_kit as k
import generate_player_avatar_meshes as avatar
from generate_tier02_batch13 import mesh_object,append
from generate_tier02_batch06 import tube
from generate_tier02_batch02 import stroke
from tier02_batch_workflow import generate,ROOT

COLORS=[(159,173,158),(116,133,119),(111,72,45),(143,97,57),
        (183,103,55),(228,151,91),(128,70,40),(185,195,172)]
PANT_COLORS=[(117,136,123),(86,104,94),*COLORS[2:]]
TYPES=['cloth','cloth','leather','leather','metal','metal','metal','cloth']
PAL=fp.SlotPalette(8,{'Flax':0,'Dark':1})


def convert(source):
    m=base.Model(source.name);m.vertices=list(source.vertices)
    m.faces=[(a+1,b+1,c+1,col) for a,b,c,col in source.faces];return m


def vertical(name,rows):
    m=k.Mesh(name,PAL);k.vertical_loft(m,rows,'Flax',a=.70,b=.62);return convert(m)


def arm(female,side,inner=False):
    mesh=k.Mesh('SleeveCage',PAL)
    shrink=-.007 if inner else .005
    length=.255 if inner else .225
    # The sleeve cavity begins inside the torso, so it cannot punch through the shoulder cap.
    joints=[((0,-.030 if inner else .025,0),.058+shrink,.060+shrink),
            ((0,-.065 if inner else -.025,0),.062+shrink,.065+shrink),((0,-.10,0),.060+shrink,.063+shrink),
            ((0,-length,0),.057+shrink,.060+shrink)]
    k.limb(mesh,joints,'Flax',sides=12,turn=math.pi/12)
    transform,_,_=avatar.arm_to_root(side,female);mesh.transform(transform)
    return convert(mesh)


def shell(outers,inners,name):
    """Union complete volumes, then subtract a connected cavity for real openings."""
    def union(models,label):
        obj=mesh_object(models[0],label)
        for part in models[1:]:
            other=mesh_object(part,label+'Part');bpy.context.view_layer.objects.active=obj
            mod=obj.modifiers.new('ContinuousGarmentUnion','BOOLEAN');mod.operation='UNION';mod.solver='EXACT';mod.object=other
            bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(other,do_unlink=True)
        return obj
    obj=union(outers,name+'Outer');cutter=union(inners,name+'Cavity');bpy.context.view_layer.objects.active=obj
    mod=obj.modifiers.new('HollowGarment','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter
    bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(cutter,do_unlink=True)
    bm=bmesh.new();bm.from_mesh(obj.data)
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001)
    bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.000001)
    bmesh.ops.triangulate(bm,faces=list(bm.faces));bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(obj.data);bm.free();m=base.Model(name)
    for v in obj.data.vertices:m.v(tuple(v.co))
    for p in obj.data.polygons:m.tri(*[i+1 for i in p.vertices],0)
    bpy.data.objects.remove(obj,do_unlink=True);return m


def copy_model(source,name):
    m=base.Model(name);m.vertices=list(source.vertices);m.faces=list(source.faces);return m


def front_support(m):
    s=base.Model('FrontSupport');s.vertices=[(x,y,-z) for x,y,z in m.vertices];s.faces=list(m.faces)
    def seat(x,y,offset=.001):
        p=kit.front_surface(s,x,y,offset);return p[0],p[1],-p[2]
    return seat


def patch(m,seat,x0,x1,y0,y1,slot,offset=.0025):
    # Tessellate in BOTH axes: long flat chords otherwise sink through the curved chest.
    nx=max(1,math.ceil((x1-x0)/.012));ny=max(1,math.ceil((y1-y0)/.025))
    for i in range(ny):
        a=y0+(y1-y0)*i/ny;b=y0+(y1-y0)*(i+1)/ny
        for j in range(nx):
            l=x0+(x1-x0)*j/nx;r=x0+(x1-x0)*(j+1)/nx
            kit.closed_patch(m,[seat(l,a,offset),seat(r,a,offset),seat(r,b,offset),seat(l,b,offset)],(0,0,1),.003,slot)


@lru_cache(None)
def shirt(female=False):
    name=('Female' if female else 'Male')+'WearFlaxWorkShirt'
    rows=fp.torso_sections(female)
    rows=[(-.04,*rows[0][1:]),(-.024,*rows[1][1:]),*rows[2:]]
    outer=vertical('TorsoOuter',[(y+.94,w+.006,f+.006,b+.006) for y,w,f,b in rows])
    inner_rows=[(y+.94,w-.002,f-.002,b-.002) for y,w,f,b in rows]
    inner_rows[0]=(inner_rows[0][0]-.06,*inner_rows[0][1:]);inner_rows[-1]=(inner_rows[-1][0]+.06,*inner_rows[-1][1:])
    inner=vertical('TorsoInner',inner_rows)
    m=shell([outer,arm(female,-1),arm(female,1)],[inner,arm(female,-1,True),arm(female,1,True)],name)
    seat=front_support(m)
    # Three Set A pockets. Leather reinforcement stays inside the existing input budget.
    for cx,cy,w,h in [(-.084,1.215,.105,.105),(.084,1.215,.105,.105),(.092,1.025,.080,.075)]:
        patch(m,seat,cx-w/2,cx+w/2,cy-h/2,cy+h/2,1)
        patch(m,seat,cx-w/2,cx+w/2,cy+h/2-.012,cy+h/2+.010,2,.0035)
        for x in [cx-w/2+.006,cx+w/2-.006]:
            stroke(m,[seat(x,y,.004) for y in [cy-h/2+.008,cy,cy+h/2-.015]],7,.0014)
    # Narrow flax placket with leather toggles, no invented metal buttons.
    patch(m,seat,-.010,.010,.95,1.345,1)
    for y in [1.08,1.19,1.31]:
        stroke(m,[seat(-.014,y,.003),seat(.014,y,.003)],3,.003)
    return m


def leg(female,side,inner=False):
    mesh=k.Mesh('LegCage',PAL);shrink=-.006 if inner else .004
    g=1.03 if female else 1
    # A continuous hip-to-hem limb cage; no capped thigh/shin seam.
    joints=[((side*.10,.93,0),.092*g+shrink,.097*g+shrink),
            ((side*.10,.85,0),.094*g+shrink,.099*g+shrink),
            ((side*.10,.72,.004),.086*g+shrink,.090*g+shrink),
            ((side*.10,.58,.008),.070+shrink,.075+shrink),
            ((side*.10,.50,.012),.064+shrink,.068+shrink),
            ((side*.10,.40,.002),.067+shrink,.073+shrink),
            ((side*.10,.25,-.004),.055+shrink,.059+shrink),
            ((side*.10,.12,-.006),.054+shrink,.058+shrink),
            ((side*.10,.05 if inner else .084,-.006),.054+shrink,.058+shrink)]
    k.limb(mesh,joints,'Flax',sides=12,turn=math.pi/12);return convert(mesh)


@lru_cache(None)
def pants(female=False):
    name=('Female' if female else 'Male')+'WearFlaxTrousers'
    rows=fp.HIPS[female]
    outer=vertical('SeatOuter',[(y+.88,w+.005,f+.005,b+.005) for y,w,f,b in rows])
    inner_rows=[(y+.88,w-.005,f-.005,b-.005) for y,w,f,b in rows]
    inner_rows[-1]=(inner_rows[-1][0]+.05,*inner_rows[-1][1:])
    inner=vertical('SeatInner',inner_rows)
    m=shell([outer,leg(female,-1),leg(female,1)],[inner,leg(female,-1,True),leg(female,1,True)],name)
    seat=front_support(m)
    patch(m,seat,-.070,.070,.985,1.025,1)
    # Small copper rail at the right hip, with exactly three existing Set A hooks.
    width=max(w for y,w,f,b in rows)+.008;x=width+.009
    tube(m,[(x,.94,-.078),(x,.94,.078)],.0045,4,8)
    for z in [-.060,0,.060]:
        tube(m,[(x-.017,.94,z),(x+.008,.94,z),(x+.014,.90,z),(x+.013,.875,z),
                (x+.027,.863,z),(x+.039,.874,z),(x+.038,.889,z)],.0035,4,8)
    # Flax waist loops attach rail to the cloth, without adding a leather-belt input.
    for z in [-.065,.065]:tube(m,[(width-.006,.965,z),(x,.948,z),(x,.927,z),(width-.006,.92,z)],.005,1,6,flat=True)
    # Seated front seams follow each leg's evaluated surface.
    for sign in (-1,1):
        xleg=sign*.10
        for a,b in [(.16,.36),(.38,.56),(.58,.75)]:
            stroke(m,[seat(xleg,y,.0008) for y in [a,(a+b)/2,b]],1,.0013)
    return m


def display(part):
    source=shirt(False) if part=='shirt' else pants(False)
    m=copy_model(source,'FlaxWorkShirt' if part=='shirt' else 'FlaxTrousers')
    center=[(min(v[i] for v in m.vertices)+max(v[i] for v in m.vertices))/2 for i in range(3)]
    scale=1.45 if part=='shirt' else .92
    m.vertices=[((x-center[0])*scale,(y-center[1])*scale,-(z-center[2])*scale) for x,y,z in m.vertices]
    return m


SPECS=[('FlaxWorkShirt','Flax work shirt',lambda:display('shirt'),COLORS,TYPES),
       ('FlaxTrousers','Flax trousers',lambda:display('pants'),PANT_COLORS,TYPES)]
META={}
for female in (False,True):
    body='Female' if female else 'Male'
    for suffix,label,build,colors,slug in [('WearFlaxWorkShirt','Flax work shirt',lambda female=female:copy_model(shirt(female),('Female' if female else 'Male')+'WearFlaxWorkShirt'),COLORS,'flax_work_shirt'),
                                         ('WearFlaxTrousers','Flax trousers',lambda female=female:copy_model(pants(female),('Female' if female else 'Male')+'WearFlaxTrousers'),PANT_COLORS,'flax_trousers')]:
        name=body+suffix;SPECS.append((name,label+' ('+body+' fitted)',build,colors,TYPES))
        META[name]={'Representation':'worn','Body':body,'Anchor':'Body-root metre coordinates; runtime skinning/binding pending','ItemId':'item.'+slug,'RecipeId':'recipe.wearable.'+slug}
for name,slug in [('FlaxWorkShirt','flax_work_shirt'),('FlaxTrousers','flax_trousers')]:
    META[name]={'Representation':'display','ItemId':'item.'+slug,'RecipeId':'recipe.wearable.'+slug}
RECIPES={name:(('T2-H7',8,'3 Flax cloth + 1 Beaver leather -> 1') if 'Shirt' in name else
               ('T2-E3',8,'3 Flax cloth + 1 Copper fittings -> 1')) for name,*_ in SPECS}


if __name__=='__main__':generate(14,SPECS,RECIPES,category='Wearables',role='Wearable',preview='--preview' in sys.argv,item_metadata=META)
