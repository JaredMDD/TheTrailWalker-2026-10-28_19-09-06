"""Four exact-plan passive Misc goods; no furniture, equipment or source edits."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import generate_tier02_batch19 as detail
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch06 import tube
from generate_tier02_batch07 import annulus,outline
from generate_tier02_batch20 import append
from generate_tier02_batch23 import fuse
from tier02_geometry import beveled_profile
from tier02_batch_workflow import generate


def plinth(m,w,d,y,slot):
    p=base.Model('IntegralBirchPlinth')
    beveled_profile(p,outline(w,d,.035),.064,slot,slot,.009)
    append(m,p,lambda q:(q[0],y+q[2],q[1]))


def bird():
    m=base.Model('PaintedBirchBird')
    ellipsoid(m,(0,.005,0),(.257,.179,.140),0,16)
    ellipsoid(m,(.173,.158,0),(.124,.134,.112),0,14)
    m.cylinder((.263,.168,0),(.388,.153,0),.037,.002,8,0,0)
    # Tail is carved timber, not metal wire or a raw feather ingredient.
    tail=base.Model('CarvedTail')
    beveled_profile(tail,[(-.20,.035),(-.44,.09),(-.47,.035),(-.43,-.025),(-.20,-.055)],.13,0,0,.012)
    append(m,tail)
    m=fuse(m)
    plinth(m,.48,.28,-.245,0)
    for x in [-.08,.09]:
        m.cylinder((x,-.23,0),(x,-.12,0),.026,.030,10,0,0)
        ellipsoid(m,(x,-.199,-.025),(.048,.022,.061),0,10)
    # One yellow-ochre paint family; bare/recessed wood supplies dark detail.
    # Wings follow actual opposite side surfaces rather than a floating flat decal.
    body=base.Model('ReadOnlyBirdSurface');body.vertices=list(m.vertices);body.faces=list(m.faces)
    for sign in [-1,1]:
        def at(x,y,offset=.003):
            p=kit.front_surface(body,x,y,offset);return(p[0],p[1],p[2] if sign<0 else -p[2])
        coords=[(-.22,.03),(-.14,.11),(.03,.12),(.12,.04),(.06,-.06),(-.09,-.085)]
        center=(-.055,.025)
        for a,b in zip(coords,coords[1:]+coords[:1]):
            # Subdivide fan over curved wood, retaining a seated yellow paint shell.
            steps=4
            def sample(i,j):
                u=i/steps;v=j/steps
                return at((1-u-v)*center[0]+u*a[0]+v*b[0],(1-u-v)*center[1]+u*a[1]+v*b[1],.004)
            for i in range(steps):
                for j in range(steps-i):
                    kit.closed_patch(m,[sample(i,j),sample(i+1,j),sample(i,j+1)],(0,0,sign),.007,1)
                    if i+j<steps-1:kit.closed_patch(m,[sample(i+1,j),sample(i+1,j+1),sample(i,j+1)],(0,0,sign),.007,1)
        for x in [-.13,-.06,.01]:
            stroke(m,[at(x-.035,y,.010) for y in [-.035,0,.035]],2,.0022)
        eye=at(.220,.198,.003);ellipsoid(m,eye,(.010,.010,.005),2,8)
    return m


def charm():
    m=base.Model('FishingCharm')
    # Fish-shaped copper fitting, a single sale ornament with no hook/rod function.
    points=[(-.28,-.13),(-.23,-.025),(-.28,.12),(-.10,.065),(.02,.13),(.16,.10),(.27,.035),
            (.29,0),(.26,-.035),(.14,-.10),(.02,-.12),(-.10,-.06)]
    fish=base.Model('WorkedCopperFish')
    beveled_profile(fish,points,.044,0,1,.008)
    # Original eyelet integrated at upper back; no glass bead or extra gem.
    annulus(fish,(-.105,.115,0),.046,.024,.040,0,20)
    append(m,fish,lambda p:(p[0],p[1]-.24,p[2]))
    seat=detail.support(m)
    ellipsoid(m,seat(.18,-.22,.004),(.012,.012,.005),2,8)
    stroke(m,[seat(.12,y,.004) for y in [-.175,-.21,-.25,-.29]],2,.0027)
    for x in [-.04,.015,.07]:
        stroke(m,[seat(x-.017,-.21,.003),seat(x-.027,-.24,.003),seat(x-.017,-.27,.003)],1,.0023)
    # Flat plaited Cattail cord loop, visibly passing through the copper eyelet.
    path=[(-.105,-.115,0),(-.205,.04,0),(-.19,.25,0),(-.10,.36,0),(.035,.37,0),(.125,.26,0),(.13,.07,0),(.03,-.05,0),(-.105,-.115,0)]
    for strand in range(3):
        pts=[]
        for i in range(len(path)-1):
            a,b=path[i],path[i+1]
            for j in range(8):
                t=j/8;k=i*8+j;phase=k*.85+strand*math.tau/3
                pts.append((a[0]*(1-t)+b[0]*t+.006*math.cos(phase),a[1]*(1-t)+b[1]*t,.009*math.sin(phase)))
        pts.append((path[-1][0]+.006*math.cos(64*.85+strand*math.tau/3),path[-1][1],.009*math.sin(64*.85+strand*math.tau/3)))
        tube(m,pts,.006,3+strand%2,6,True)
    return m


def candle():
    m=base.Model('BeeswaxCandle');n=32;rings=[]
    rows=[(-.40,0),(-.40,.152),(-.37,.174),(-.12,.176),(.15,.172),(.36,.174),(.39,.164),(.375,.125),(.355,.045),(.355,0)]
    for k,(y,r) in enumerate(rows):
        if r==0:rings.append([m.v((0,y,0))]);continue
        rings.append([m.v((r*(1+.015*math.cos(i*math.tau*5/n))*math.cos(i*math.tau/n),
                          y+(.004*math.sin(i*math.tau*3/n) if k>=5 else 0),
                          r*(1+.015*math.cos(i*math.tau*5/n))*math.sin(i*math.tau/n))) for i in range(n)])
    for k,(a,b) in enumerate(zip(rings,rings[1:])):
        for i in range(n):
            j=(i+1)%n;c=1 if k>=5 else 0
            if len(a)==1:m.tri(a[0],b[j],b[i],c)
            elif len(b)==1:m.tri(a[i],a[j],b[0],c)
            else:m.tri(a[i],a[j],b[j],c);m.tri(a[i],b[j],b[i],c)
    # Unlit Flax-cord wick enters the recessed wax top; no flame/light emitter.
    for strand in range(2):
        tube(m,[(.003*math.cos(i*.9+strand*math.pi),.345+i*.006,.003*math.sin(i*.9+strand*math.pi)) for i in range(12)],.0036,3,6)
    # Original shallow molded comb marks, not a paper label or extra ingredient.
    seat=detail.support(m)
    for x,y in [(-.036,-.065),(.036,-.065),(0,-.002)]:
        pts=[seat(x+.039*math.cos(i*math.tau/6),y+.039*math.sin(i*math.tau/6),.002) for i in range(7)]
        tube(m,pts,.0027,2,6)
    return m


def owl():
    m=base.Model('SandstoneOwl')
    ellipsoid(m,(0,-.055,0),(.223,.273,.15),0,18)
    ellipsoid(m,(0,.180,-.006),(.218,.180,.145),0,18)
    for sign in [-1,1]:
        ellipsoid(m,(sign*.176,-.063,.0),(.075,.195,.12),0,12)
        ellipsoid(m,(sign*.095,-.289,-.055),(.074,.047,.083),0,10)
    m=fuse(m)
    # Birch plank is an actual visible recipe material, not a hidden extra stand.
    plinth(m,.52,.36,-.365,4)
    seat=detail.support(m)
    for sign in [-1,1]:
        cx=sign*.103;cy=.18
        ring=base.Model('CarvedEyeRing');annulus(ring,(0,0,0),.077,.043,.020,1,24)
        append(m,ring,lambda p:(cx+p[0],cy+p[1],seat(cx+p[0],cy+p[1],.008)[2]+p[2]))
        ellipsoid(m,seat(cx,cy,.006),(.031,.034,.009),3,12)
        # Short radiating cuts imply carved stone, not painted feather stripes.
        for i in range(12):
            a=i*math.tau/12
            stroke(m,[seat(cx+r*math.cos(a),cy+r*math.sin(a),.020) for r in [.049,.067]],2,.0017)
        for y in [-.22,-.14,-.06,.02]:
            x=sign*(.16 if y<-.1 else .19)
            stroke(m,[seat(x-sign*.014,y+.024,.002),seat(x,y,.002),seat(x+sign*.012,y+.018,.002)],2,.002)
    # Small stone beak and chest chevrons, all seated on the actual front.
    kit.closed_patch(m,[seat(-.027,.110,.007),seat(.027,.110,.007),seat(0,.035,.021)],(0,0,-1),.022,1)
    for y in [-.04,-.10,-.16]:stroke(m,[seat(-.043,y+.018,.003),seat(0,y,.003),seat(.043,y+.018,.003)],1,.0025)
    return m


SPECS=[
 ('PaintedBirchBird','Painted birch bird',bird,[(197,158,101),(205,157,50),(115,82,44)],['wood','wood','wood']),
 ('FishingCharm','Fishing charm',charm,[(177,95,51),(216,134,78),(102,57,34),(148,131,79),(192,173,108)],['metal','metal','metal','plant_fibre','plant_fibre']),
 ('BeeswaxCandle','Beeswax candle',candle,[(222,174,64),(241,198,94),(184,131,38),(160,144,99)],['flat','flat','flat','plant_fibre']),
 ('SandstoneOwl','Sandstone owl',owl,[(180,146,107),(219,187,140),(147,111,76),(110,82,57),(194,156,99)],['mineral','mineral','mineral','mineral','wood'])]
RECIPES={
 'PaintedBirchBird':('T2-L16',12,'2 Birch planks + 1 Yellow ochre -> 1'),
 'FishingCharm':('T2-E17',12,'1 Cattail cord + 1 Copper fitting -> 1'),
 'BeeswaxCandle':('T2-G19',12,'2 Beeswax + 1 Flax cord -> 1'),
 'SandstoneOwl':('T2-L10',10,'2 Sandstone tiles + 1 Birch plank -> 1')}
META={name:{'Representation':'display','GameplayRole':'Sale Misc, never functional furniture or equipment',
 'BindingStatus':'Passive art; canonical output IDs/live bindings unresolved'} for name,*_ in SPECS}
META['BeeswaxCandle']['RecipeId']='recipe.misc.beeswax_candle'
META['SandstoneOwl']['RecipeId']='recipe.misc.sandstone_owl'
META['BeeswaxCandle']['SourcePolicy']='Beeswax T2-G12 / Honey region 11; exact Flax cord T2-E2; unlit static candle, no light or home buff'
META['FishingCharm']['IngredientPolicy']='Cattail cord T2-E14 and Copper fittings T2-L4; not Hemp/Flax cord; no fishing luck, tool uses or hook function'
META['PaintedBirchBird']['IngredientPolicy']='Yellow ochre only pigment; remaining dark/light cues are bare/recessed Birch; no feathers or metal legs'
META['SandstoneOwl']['IngredientPolicy']='Sandstone sculpture with integral Birch plank plinth; no marble/gem eyes, paint, furniture placement or buff'

if __name__=='__main__':generate(24,SPECS,RECIPES,category='Misc',role='Misc',preview='--preview' in sys.argv,item_metadata=META)
