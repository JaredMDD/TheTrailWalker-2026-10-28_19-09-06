"""Four passive Misc goods; current November 7 Plant-stuffing recipe retained."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import bpy,bmesh
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import generate_tier02_batch19 as design
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch06 import tube
from generate_tier02_batch07 import annulus
from generate_tier02_batch20 import append
from tier02_batch_workflow import generate


def fuse(m):
    """Continuous carved primary form, before local incisions/material features."""
    mesh=bpy.data.meshes.new('CarvingUnionConstruction')
    mesh.from_pydata(m.vertices,[],[(a-1,b-1,c-1) for a,b,c,_ in m.faces]);mesh.update()
    obj=bpy.data.objects.new('CarvingUnionConstruction',mesh);bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active=obj;obj.select_set(True)
    mod=obj.modifiers.new('ContinuousCarvedWood','REMESH');mod.mode='VOXEL';mod.voxel_size=.018;mod.use_smooth_shade=False
    bpy.ops.object.modifier_apply(modifier=mod.name)
    mod=obj.modifiers.new('IntentionalCarvingFacets','DECIMATE');mod.ratio=.14
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.triangulate(bm,faces=list(bm.faces));bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
    out=base.Model(m.name);out.vertices=[tuple(v.co) for v in obj.data.vertices]
    out.faces=[(*(i+1 for i in f.vertices),0) for f in obj.data.polygons]
    final_mesh=obj.data;bpy.data.objects.remove(obj,do_unlink=True);bpy.data.meshes.remove(final_mesh)
    return out


def paddle(m,toy=False):
    tail=base.Model('BroadFlatPaddle')
    ellipsoid(tail,(0,0,0),(.265,.031,.115),2,16)
    # Crosshatching is seated on the true upper tail, not floating above a plane.
    support=base.Model('TailSurface');support.vertices=[(x,z,-y) for x,y,z in tail.vertices];support.faces=list(tail.faces)
    for sign in [-1,1]:
        for x in [-.15,-.075,0,.075,.15]:
            pts=[]
            for z in [-.064,0,.064]:
                xx=x+sign*z*.5
                p=kit.front_surface(support,xx,z,.002);pts.append((xx,-p[2],z))
            stroke(tail,pts,3,.0025 if toy else .0017)
    turn=-.20 if toy else .30;cx=-.28 if toy else .28
    append(m,tail,lambda p:(cx+p[0]*math.cos(turn)-p[2]*math.sin(turn),-.266+p[1],.11+p[0]*math.sin(turn)+p[2]*math.cos(turn)))


def beaver(toy=False):
    m=base.Model('BeaverToy' if toy else 'BirchBeaverSculpture')
    ellipsoid(m,(0,-.035,.025),(.225,.27,.205 if toy else .235),0,16)
    ellipsoid(m,(0,.255,-.095),(.178,.172,.163),0,16)
    for sign in [-1,1]:
        ellipsoid(m,(sign*.123,.396,-.035),(.051,.059,.033),0,10)
        ellipsoid(m,(sign*.15,-.242,-.13),(.100,.064,.132),0,12)
        # Broad short paws join the torso; never thin dangling appendages.
        ellipsoid(m,(sign*.178,-.015,-.132),(.068,.117,.075),0,12)
    for sign in [-1,1]:ellipsoid(m,(sign*.045,.208,-.233),(.066,.045,.049),0,12)
    if not toy:m=fuse(m)
    paddle(m,toy)
    # Face features are wood recess-color cues or stitched fibre, not added beads.
    seat=design.support(m)
    for sign in [-1,1]:
        x=sign*.09;y=.292
        if toy:
            for d in [-1,1]:stroke(m,[seat(x-.013,y+d*.014,.003),seat(x+.013,y-d*.014,.003)],3,.004)
        else:ellipsoid(m,seat(x,y,.003),(.012,.013,.005),2,8)
        if toy:ellipsoid(m,(sign*.123,.399,-.061),(.027,.033,.008),1,8)
    ellipsoid(m,seat(0,.224,.003),(.026,.019,.012),3 if toy else 2,10)
    for sign in [-1,1]:
        # Paired cream textile incisors on the toy, pale carved birch on sculpture.
        pts=[seat(sign*.022+dx,y,.005) for dx,y in [(-.016,.174),(.016,.174),(.015,.126),(-.015,.126)]]
        kit.closed_patch(m,pts,(0,0,-1),.008,4 if toy else 1)
    if toy:
        # Hemp-thread seam and spaced cross-stitches follow the actual pelt shell.
        stroke(m,[seat(0,y,.003) for y in [-.23,-.15,-.07,.01,.08]],3,.0027)
        for y in [-.20,-.14,-.08,-.02,.04]:stroke(m,[seat(-.017,y-.004,.005),seat(.017,y+.004,.005)],4,.0024)
        for sign in [-1,1]:
            for y in [-.01,.04,.09]:stroke(m,[seat(sign*.16-.008,y-.007,.004),seat(sign*.16+.008,y+.007,.004)],4,.002)
    else:
        # Sparse carved grain on the chest with no fur fibers or sewn seams.
        for x in [-.12,-.06,.06,.12]:
            stroke(m,[seat(x+.007*math.sin(y*15),y,.001) for y in [-.17,-.10,-.03,.04,.10]],1,.0019)
    return m


def pendant():
    m=base.Model('CopperPendant')
    # Original raw-quartz prism: mission requests Cut quartz, but the recipe
    # explicitly consumes Quartz. Do not silently substitute the refined input.
    rings=[];n=6
    for y,r in [(-.39,.0),(-.28,.115),(.015,.119),(.095,.075),(.14,0)]:
        rings.append([m.v((0,y,0))] if r==0 else [m.v((r*math.cos(i*math.tau/n),y,.072*math.sin(i*math.tau/n))) for i in range(n)])
    for k,(a,b) in enumerate(zip(rings,rings[1:])):
        for i in range(n):
            j=(i+1)%n;slot=[3,4,5,4,3,5][i]
            if len(a)==1:m.tri(a[0],b[j],b[i],slot)
            elif len(b)==1:m.tri(a[i],a[j],b[0],slot)
            else:m.tri(a[i],a[j],b[j],slot);m.tri(a[i],b[j],b[i],slot)
    # Two copper cradle rails support the tip and reach a true pierced bail.
    for z in [-.064,.064]:
        tube(m,[(-.102,.055,z),(-.123,-.17,z),(-.073,-.31,z),(0,-.385,0),(.073,-.31,z),(.123,-.17,z),(.102,.055,z)],.012,0,8)
    for y in [-.22,.025]:
        tube(m,[(.123*math.cos(i*math.tau/24),y,.078*math.sin(i*math.tau/24)) for i in range(25)],.010,1,8)
    tube(m,[(-.10,.05,0),(-.06,.13,0),(0,.17,0),(.06,.13,0),(.10,.05,0)],.014,0,8)
    # Bail stands in the YZ plane: cord passes through its actual hole.
    bail=base.Model('PiercedCopperBail');annulus(bail,(0,0,0),.052,.029,.030,0,20)
    append(m,bail,lambda p:(p[2],p[1]+.18,p[0]))
    # One folded hemp-cord loop; its bottom crosses the bail opening.
    tube(m,[(-.0,.18,-.011),(-.18,.31,-.010),(-.20,.51,0),(-.11,.62,0),(.11,.62,0),(.20,.51,0),(.18,.31,.010),(0,.18,.011)],.014,6,8)
    for y in [.29,.33,.37]:
        # Small fiber winding marks on the two suspended sides.
        for sign in [-1,1]:
            x=sign*(.18+(y-.31)*.10)
            stroke(m,[(x-.007,y-.006,-.016),(x+.007,y+.006,-.016)],7,.002)
    return m


def pouch():
    m=base.Model('LavenderPouch');rings=[];n=28
    # Filled cloth silhouette, gathered throat and flared folded mouth.
    rows=[(-.33,0,0),(-.33,.19,.10),(-.27,.255,.145),(-.08,.25,.15),
          (.08,.19,.115),(.18,.078,.060),(.22,.075,.058),(.32,.14,.075),(.35,.145,.077)]
    for j,(y,rx,rz) in enumerate(rows):
        if not rx:rings.append([m.v((0,y,0))]);continue
        rings.append([m.v((rx*math.cos(i*math.tau/n)*(1+(.10 if j>3 else .025)*math.cos(i*math.tau*7/n)),
                          y+(.013 if j>=7 else .004)*math.sin(i*math.tau*7/n),
                          rz*math.sin(i*math.tau/n)*(1+.09*math.cos(i*math.tau*7/n)))) for i in range(n)])
    for a,b in zip(rings,rings[1:]):
        for i in range(n):
            j=(i+1)%n
            if len(a)==1:m.tri(a[0],b[j],b[i],0)
            else:m.tri(a[i],a[j],b[j],0);m.tri(a[i],b[j],b[i],0)
    # Narrow closed top with a real folded hem; no open mesh edge.
    inside=[m.v((x*.83,y-.012,z*.83)) for x,y,z in [m.vertices[i-1] for i in rings[-1]]]
    for i in range(n):
        j=(i+1)%n;m.tri(rings[-1][i],rings[-1][j],inside[j],1);m.tri(rings[-1][i],inside[j],inside[i],1)
    center=m.v((0,.305,0))
    for i in range(n):m.tri(inside[i],inside[(i+1)%n],center,0)
    # Cloth-cut self tie consumes the same Hemp cloth, no extra Cord recipe input.
    tube(m,[(.083*math.cos(i*math.tau/28),.195,.066*math.sin(i*math.tau/28)) for i in range(29)],.012,1,8,True)
    for sign in [-1,1]:
        tube(m,[(0,.195,-.072),(sign*.09,.24,-.083),(sign*.16,.21,-.079),(sign*.12,.17,-.078),(0,.195,-.072)],.012,1,8,True)
        tube(m,[(0,.19,-.071),(sign*.07,.13,-.10),(sign*.11,.07,-.12)],.012,1,8,True)
    # Lavender-colored surface motif is an illustrative content identifier.
    seat=design.support(m)
    for sign in [-1,1]:
        stem=[seat(sign*(.018+(y+.22)*(.032/.18)),y,.004) for y in [-.22,-.18,-.14,-.10,-.06,-.04]]
        stem += [seat(sign*(.05+(y+.04)*.3125),y,.004) for y in [-.02,0,.02,.04]]
        stroke(m,stem,3,.003)
        for y in [-.04,.0,.04]:
            x=sign*(.05+(y+.04)*.31)
            for side in [-1,1]:ellipsoid(m,seat(x+side*.014,y,.005),(.018,.012,.007),2 if side<0 else 4,6)
    # Visible side-stitch marks are illustrative cloth finishing, no extra input.
    for sign in [-1,1]:
        for y in [-.24,-.17,-.10,-.03]:
            x=sign*(.205 if y<-.06 else .185)
            stroke(m,[seat(x-.005,y-.008,.003),seat(x+.005,y+.008,.003)],1,.0023)
    return m


SPECS=[
 ('CopperPendant','Copper pendant',pendant,[(176,91,48),(215,131,73),(111,61,34),(174,199,191),(218,229,209),(139,172,172),(161,133,86),(211,184,127)],['metal','metal','metal','glass','glass','glass','plant_fibre','plant_fibre']),
 ('BirchBeaverSculpture','Birch beaver sculpture',lambda:beaver(False),[(194,155,96),(226,190,131),(116,79,41),(153,111,60)],['wood','wood','wood','wood']),
 ('BeaverToy','Beaver toy',lambda:beaver(True),[(112,75,45),(164,122,75),(80,54,36),(55,39,26),(210,186,137)],['fur','fur','leather','plant_fibre','plant_fibre']),
 ('LavenderPouch','Lavender pouch',pouch,[(184,167,127),(219,201,157),(105,68,135),(82,104,57),(157,106,172)],['cloth','cloth','flat','flat','flat'])]
RECIPES={
 'CopperPendant':('T2-L8',9,'1 Copper ingot + 1 Quartz + 1 Hemp cord -> 1'),
 'BirchBeaverSculpture':('T2-L12',11,'2 Birch planks + 1 Beeswax -> 1'),
 'BeaverToy':('T2-H9',9,'2 Fine beaver pelts + 1 Plant stuffing + 1 Hemp thread -> 1'),
 'LavenderPouch':('T2-G21',10,'2 Lavender + 1 Hemp cloth -> 1')}
META={name:{'Representation':'display','BindingStatus':'Passive sale Misc art; canonical IDs/live bindings unresolved',
 'GameplayRole':'Sale Misc, never functional furniture or equipment; no companion behavior'} for name,*_ in SPECS}
META['CopperPendant']['IngredientPolicy']='Quartz remains recipe input; Cut quartz is mission request, not a substitution; no equipment stat/buff'
META['BirchBeaverSculpture']['SourcePolicy']='Beeswax from T2-G12 / region 11 Honey; sale sculpture, no furniture buff'
META['BeaverToy']['RecipeRevision']='November 7 reversal: exact Plant stuffing (H8), not Flax stuffing; Hemp thread E9; no buttons/glass eyes'
META['LavenderPouch']['SourcePolicy']='Farm Lavender unlocked by region 10 discovery; appended G21 opens in region 10; integral cloth self-tie, no added cord ingredient'

if __name__=='__main__':generate(23,SPECS,RECIPES,category='Misc',role='Misc',preview='--preview' in sys.argv,item_metadata=META)
