"""Two premium optional ranch-meat foods; passive art, no live recipe changes."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import generate_tier02_batch19 as design
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch20 import append,bowl
from generate_tier02_batch22 import top_support
from tier02_batch_workflow import generate


def breast():
    m=base.Model('ScoredHoneyDuck');rings=[];n=24;steps=32
    for j in range(steps+1):
        t=j/steps;x=-.27+.54*t;s=math.sin(math.pi*t)**.45
        if j in [0,steps]:rings.append([m.v((x,-.005,0))]);continue
        ring=[]
        for i in range(n):
            a=i*math.tau/n;c=math.cos(a);z=.165*s*math.sin(a)
            y=max(-.060,.012+.118*s*c)
            if c>.25:
                distance=min(abs(((x+sign*z)*10+.5)%1-.5)/10 for sign in [-1,1])
                y-=.004*max(0,1-distance/.010)
            ring.append(m.v((x,y,z)))
        rings.append(ring)
    for j,(a,b) in enumerate(zip(rings,rings[1:])):
        for i in range(n):
            q=(i+1)%n;c=math.cos((i+.5)*math.tau/n)
            slot=6 if c>.45 else 5 if c>.0 else 3
            if len(a)==1:m.tri(a[0],b[q],b[i],slot)
            elif len(b)==1:m.tri(a[i],a[q],b[0],slot)
            else:m.tri(a[i],a[q],b[q],slot);m.tri(a[i],b[q],b[i],slot)
    at=top_support(m)
    # Short surface-projected dark cuts reinforce the authored scored skin.
    for sign in [-1,1]:
        for intercept in [-.14,-.07,0,.07,.14]:
            pts=[]
            for j in range(13):
                z=-.098+j*.196/12;x=intercept+sign*z*.70
                if abs(x)<.235:pts.append(at(x,z,.001))
            if len(pts)>1:stroke(m,pts,5,.0019)
    # Embedded amber glaze glints, not detached syrup strings or extra butter.
    for x,z in [(-.15,-.03),(-.045,.065),(.06,-.06),(.15,.025)]:
        ellipsoid(m,at(x,z,.001),(.027,.0018,.011),7,8)
    return m


def roast():
    m=base.Model('HoneyRoastDuck');bowl(m,fill=None,shallow=True)
    # Existing recipe Honey forms the shallow glaze pool; no unlisted sauce.
    ellipsoid(m,(0,-.056,0),(.265,.010,.185),2,24)
    append(m,breast())
    seat=design.support(m)
    # Small honey droplet follows the existing serving-dish relief convention.
    pts=[(0,-.015),(.025,-.050),(.023,-.077),(0,-.092),(-.023,-.077),(-.025,-.050)]
    center=seat(0,-.055,.006)
    for a,b in zip(pts,pts[1:]+pts[:1]):kit.closed_patch(m,[center,seat(*a,.006),seat(*b,.006)],(0,0,-1),.007,7)
    return m


def duck_slice(m,x,y,z,turn):
    p=base.Model('CookedDuckSlice')
    # Closed meat slice with darker curved skin edge and pale cooked cut faces.
    # Local XY silhouette extrudes along Z, with the skin across the high crown.
    n=16;rings=[]
    for zz,scale in [(-.032,.92),(-.024,1),(.024,1),(.032,.92)]:
        ring=[]
        for i in range(n):
            a=i*math.tau/n
            ring.append(p.v((.102*math.cos(a)*scale,.048*math.sin(a)*scale,zz)))
        rings.append(ring)
    for k,(a,b) in enumerate(zip(rings,rings[1:])):
        for i in range(n):
            j=(i+1)%n;slot=6 if math.sin((i+.5)*math.tau/n)>.55 else 5
            p.tri(a[i],a[j],b[j],slot);p.tri(a[i],b[j],b[i],slot)
    for ring,zz,rev in [(rings[0],-.032,True),(rings[-1],.032,False)]:
        center=p.v((0,0,zz))
        for i in range(n):
            j=(i+1)%n
            p.tri(center,ring[j],ring[i],4) if rev else p.tri(center,ring[i],ring[j],4)
    append(m,p,lambda q:(x+q[0]*math.cos(turn)-q[2]*math.sin(turn),y+q[1],z+q[0]*math.sin(turn)+q[2]*math.cos(turn)))


def rice_dish():
    m=base.Model('DuckBlueberryRice');bowl(m,fill=None)
    ellipsoid(m,(0,.049,0),(.302,.105,.302),2,24)
    top=top_support(m)
    for i in range(42):
        a=i*2.399963;r=.271*math.sqrt((i+.5)/42);x,z=r*math.cos(a),r*math.sin(a)
        p=top(x,z,-.001);grain=base.Model('CookedRiceGrain');ellipsoid(grain,(0,0,0),(.025,.007,.010),9 if i%4 else 10,6)
        t=i*1.73
        append(m,grain,lambda q:(x+q[0]*math.cos(t)-q[2]*math.sin(t),p[1]+q[1],z+q[0]*math.sin(t)+q[2]*math.cos(t)))
    # Three cut portions are one inventory serving, seated in the rice mound.
    for x,z,t in [(-.02,-.105,.25),(.015,0,.16),(.035,.105,.07)]:
        duck_slice(m,x,.145,z,t)
    for x,z,r in [(-.19,-.10,.027),(-.20,-.035,.024),(-.17,.13,.026),(.18,.10,.027),(.19,-.10,.024),(.16,-.16,.023)]:
        pos=top(x,z,.004)
        ellipsoid(m,(x,pos[1]+r*.45,z),(r,r*.88,r),7,10)
        # Small recessed-looking crown marks distinguish round blueberries.
        ellipsoid(m,(x,pos[1]+r*1.21,z),(r*.30,.0018,r*.30),8,6)
    seat=design.support(m)
    for x,y in [(-.031,-.06),(.022,-.06),(-.004,-.102)]:ellipsoid(m,seat(x,y,.010),(.023,.022,.009),7,10)
    for x,y in [(.067,-.075),(.073,-.110)]:ellipsoid(m,seat(x,y,.008),(.022,.007,.008),9,6)
    return m


SPECS=[
 ('HoneyRoastDuck','Honey-roast duck',roast,[(165,151,117),(221,207,162),(163,99,30),(119,68,48),(180,123,93),(123,62,28),(188,112,35),(231,173,61)],['ceramic','ceramic','flat','flat','flat','flat','flat','flat']),
 ('DuckBlueberryRice','Duck blueberry rice',rice_dish,[(101,124,141),(169,192,198),(212,194,142),(117,70,57),(180,123,105),(102,55,31),(160,88,34),(52,61,111),(103,113,161),(230,211,163),(247,229,192)],['ceramic','ceramic','grain','flat','flat','flat','flat','fruit_skin','fruit_skin','grain','grain'])]
RECIPES={
 'HoneyRoastDuck':('',0,'1 Duck meat + 1 Honey + 2 Birch branches -> 1'),
 'DuckBlueberryRice':('',0,'1 Duck meat + 1 Blueberries + 1 Rice -> 1')}
META={name:{'Representation':'display','DepictedPortions':1,
 'UnlockSource':'Balloon mystery box (tier 2); no mission',
 'UnlockTier':2,'VesselPolicy':'Illustrative ceramic serving dish; not an additional ingredient',
 'SourcePolicy':'Exact ranch.duck_meat, Store only; tier 2 discovered and ranch reachable/eligible; no hunted duck, loose/pet/NPC/physical-box meat source',
 'BindingStatus':'Passive premium Food art; canonical output IDs/live bindings unresolved'} for name,*_ in SPECS}
META['HoneyRoastDuck'].update(RecipeId='recipe.food.honey_roast_duck',AuthoredHeartsPerServing=10,
    EligibilityPolicy='Exact Duck meat ranch source, Honey region 11, exact Birch fuel source; box recipe never appears early',
    FuelPolicy='Two Birch branches consumed cooking fuel; not edible garnish or a separate serving prop')
META['DuckBlueberryRice'].update(RecipeId='recipe.food.duck_blueberry_rice',AuthoredHeartsPerServing=9,
    EligibilityPolicy='Exact Duck meat ranch source, Blueberries region 9, Rice region 11; no Honey input')

if __name__=='__main__':generate(25,SPECS,RECIPES,category='Food',role='Food',preview='--preview' in sys.argv,item_metadata=META)
