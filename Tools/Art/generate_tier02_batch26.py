"""Two premium bakery foods; rare bakery unlocks, not balloon-box recipes."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch06 import tube
from generate_tier02_batch20 import append
from generate_tier02_batch22 import top_support
from tier02_batch_workflow import generate


def brioche():
    m=base.Model('HoneyBrioche');n=32;rings=[]
    # One intact round bun, with a stable baked base and shallow proofing crease.
    rows=[(-.17,0),(-.17,.20),(-.145,.275),(-.085,.315),(-.015,.326),
          (.004,.317),(.025,.325),(.090,.299),(.155,.244),(.197,.157),(.215,.065),(.219,0)]
    for k,(y,r) in enumerate(rows):
        if not r:rings.append([m.v((0,y,0))]);continue
        rings.append([m.v((r*math.cos(i*math.tau/n)*(1+.018*math.sin(i*math.tau*5/n)),
                          y+.003*math.sin(i*math.tau*3/n),r*math.sin(i*math.tau/n)*.93)) for i in range(n)])
    for k,(a,b) in enumerate(zip(rings,rings[1:])):
        for i in range(n):
            j=(i+1)%n;c=2 if k<2 else 3 if k in [3,4] else 1 if k<6 else 0
            if len(a)==1:m.tri(a[0],b[j],b[i],c)
            elif len(b)==1:m.tri(a[i],a[j],b[0],c)
            else:m.tri(a[i],a[j],b[j],c);m.tri(a[i],b[j],b[i],c)
    # Honey is in the recipe: sparse seated amber glaze, no seed/egg garnish.
    at=top_support(m)
    for x,z in [(-.14,-.09),(.055,.12),(.16,-.03)]:ellipsoid(m,at(x,z,.001),(.035,.002,.015),4,10)
    tube(m,[at(-.10+i*.015,-.105+.025*math.sin(i*.5),.002) for i in range(15)],.0038,4,6,True)
    return m


def rounded_square(a):
    c,s=math.cos(a),math.sin(a)
    return math.copysign(abs(c)**.5,c),math.copysign(abs(s)**.5,s)


def pastry():
    m=base.Model('BlackberryCreamPastry');n=40;rings=[]
    # A closed folded dough shell: shallow layered sides and raised square rim,
    # with an actual recessed center for the cream. No butter in this recipe.
    rows=[(-.125,0),(-.125,.85),(-.106,1),(-.088,.965),(-.073,1.015),
          (-.052,.975),(-.035,1.015),(-.012,.98),(.016,1.01),(.050,.96),
          (.075,.87),(.071,.72),(.024,.63),(-.012,.57),(-.014,0)]
    for k,(y,r) in enumerate(rows):
        if r==0:rings.append([m.v((0,y,0))]);continue
        ring=[]
        for i in range(n):
            a=i*math.tau/n;x,z=rounded_square(a)
            # Slight corners/folds keep the perimeter pastry-like, not a ceramic tile.
            ripple=.006*math.sin(i*1.7+k*.31) if k<10 else .002*math.sin(i*1.7)
            ring.append(m.v((.31*r*x,y+ripple,.26*r*z)))
        rings.append(ring)
    for k,(a,b) in enumerate(zip(rings,rings[1:])):
        for i in range(n):
            j=(i+1)%n;c=2 if k<2 else 1 if k in [2,4,6] else 3 if k in [3,5,7] else 0
            if len(a)==1:m.tri(a[0],b[j],b[i],c)
            elif len(b)==1:m.tri(a[i],a[j],b[0],c)
            else:m.tri(a[i],a[j],b[j],c);m.tri(a[i],b[j],b[i],c)
    # Exact Cow cream fills the recess, not cream cheese/custard/vanilla/icing.
    ellipsoid(m,(0,.030,0),(.221,.044,.176),4,24)
    for i,(x,z) in enumerate([(-.087,-.025),(.082,-.027),(-.008,.075)]):
        berry=base.Model('CompoundBlackberry')
        ellipsoid(berry,(0,0,0),(.058,.063,.053),5,10)
        for row,(yy,r) in enumerate([(-.031,.041),(.005,.049),(.039,.035)]):
            for j in range(7 if row==1 else 6):
                a=j*math.tau/(7 if row==1 else 6)+row*.4
                ellipsoid(berry,(r*math.cos(a),yy,r*math.sin(a)),(.022,.022,.022),5 if (j+row)%3 else 6,6)
        # Each berry base enters the cream; three berries depict one serving.
        append(m,berry,lambda p:(x+p[0],.104+p[1],z+p[2]))
    return m


SPECS=[
 ('HoneyBrioche','Honey brioche',brioche,[(168,98,36),(205,144,68),(126,70,31),(228,178,104),(237,183,70)],['grain','grain','grain','grain','flat']),
 ('BlackberryCreamPastry','Blackberry cream pastry',pastry,[(191,119,47),(225,170,91),(132,74,29),(164,95,35),(238,225,182),(58,31,57),(111,58,99)],['grain','grain','grain','grain','flat','fruit_skin','fruit_skin'])]
RECIPES={
 'HoneyBrioche':('',0,'2 Wheat flour + 1 Cow butter + 1 Honey -> 1'),
 'BlackberryCreamPastry':('',0,'2 Wheat flour + 2 Blackberries + 1 Cow cream -> 1')}
META={name:{'Representation':'display','DepictedPortions':1,'UnlockTier':2,
 'UnlockSource':'Rare bakery recipe substitution (tier 2); no mission; excluded from Balloon box',
 'ArtReusePolicy':'Same source mesh for prepared bakery stock and crafted output; no duplicate shop-only asset',
 'QualityPolicy':'Prepared stock Q5; crafted output ordinary Food mastery/quality; food purchase never grants recipe',
 'BindingStatus':'Passive premium bakery Food art; canonical output IDs/live bindings unresolved'} for name,*_ in SPECS}
META['HoneyBrioche'].update(RecipeId='recipe.food.honey_brioche',AuthoredHeartsPerServing=8,
    EligibilityPolicy='Wheat region 6, Honey region 11; accessible exact Cow butter, known flour/butter conversion or exact shops; source discovery never waived',
    DocumentaryGoldPerServing=5,DocumentaryGoldPerFullStack=4500,DocumentaryGoldPerRecipe=50)
META['BlackberryCreamPastry'].update(RecipeId='recipe.food.blackberry_cream_pastry',AuthoredHeartsPerServing=10,
    EligibilityPolicy='Wheat region 6, Blackberries region 7; accessible exact Cow cream, known flour/cream conversion or exact shops; no cream cheese substitution',
    DocumentaryGoldPerServing=7,DocumentaryGoldPerFullStack=6300,DocumentaryGoldPerRecipe=70)
for row in META.values():row['OfferPolicy']='Existing 1% per eligible premium bakery slot; same-food recipe replaces whole food offer, once; source/status rechecks at payment; prices/recovery documentary only'

if __name__=='__main__':generate(26,SPECS,RECIPES,category='Food',role='Food',preview='--preview' in sys.argv,item_metadata=META)
