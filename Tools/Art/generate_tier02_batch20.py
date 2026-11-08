"""Batch 20 Food art: one display per individual portion; no gameplay edits."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import generate_tier02_batch08 as glass
import generate_tier02_batch19 as drinks
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch06 import tube
from tier02_batch_workflow import generate


def append(m,part,fn=lambda p:p,slot=lambda c:c):
    off=len(m.vertices);m.vertices +=[fn(p) for p in part.vertices]
    m.faces +=[(a+off,b+off,c+off,slot(col)) for a,b,c,col in part.faces]


def bowl(m,fill=2,shallow=False):
    bottom=-.13 if shallow else -.20;rim=.065 if shallow else .13
    kit.loft(m,[(bottom,0,0),(bottom,.21,.21),(bottom+.025,.25,.25),
                (rim-.025,.36,.36),(rim,.365,.365),(rim+.008,.35,.35),
                (rim+.008,.325,.325),(rim-.03,.317,.317),
                (bottom+.065,.205,.205),(bottom+.065,0,0)],lambda k,i:1 if k in [3,4,5,6] else 0,24)
    if fill is not None:
        kit.loft(m,[(bottom+.075,0,0),(bottom+.075,.205,.205),
                    (rim-.05,.307,.307),(rim-.045,0,0)],fill,24)
    return rim


def bottled_tea():
    m=base.Model('BottledHoneyTea');append(m,glass.bottle(),slot=lambda c:[0,1,2,1][c])
    # Stopper seats inside the existing bottle throat; no separately granted closure item.
    kit.loft(m,[(.425,0,0),(.425,.076,.068),(.49,.080,.073),(.51,.070,.063),(.51,0,0)],6,16)
    seat=drinks.support(m)
    drinks.leaf_relief(m,seat,-.055,-.075,.036,.085,3)
    drinks.honey_relief(m,seat,.067,-.078)
    return m


def soup():
    m=base.Model('CarrotSoup');rim=bowl(m)
    # A few immersed cooked carrot pieces, with no cream, herb or crouton garnish.
    for x,z,r in [(-.105,.045,.042),(.065,-.07,.036),(.08,.09,.032)]:
        ellipsoid(m,(x,rim-.044,z),(r,.009,r*.8),4,10)
    seat=drinks.support(m)
    kit.closed_patch(m,[seat(-.065,-.015,.005),seat(.065,-.015,.005),seat(-.010,-.135,.005)],(0,0,-1),.007,3)
    for y in [-.05,-.077]:stroke(m,[seat(-.023,y,.014),seat(.024,y,.014)],4,.0024)
    return m


def bread():
    m=base.Model('BarleyBread');n=24;rings=[]
    # Shared cross-sections form the flat-based loaf and real shallow crown grooves.
    rows=[(-.43,.0,False),(-.415,.45,False),(-.36,.80,False),(-.28,.96,False)]
    for x in [-.16,0,.16]:rows +=[(x-.025,1,False),(x-.007,1,True),(x+.007,1,True),(x+.025,1,False)]
    rows +=[(.28,.96,False),(.36,.80,False),(.415,.45,False),(.43,0,False)]
    rows.sort(key=lambda r:r[0])
    for x,scale,groove in rows:
        if not scale:rings.append([m.v((x,-.005,0))]);continue
        ring=[]
        for i in range(n):
            a=i*math.tau/n;c,s=math.cos(a),math.sin(a)
            y=max(-.125,-.005+.235*scale*c)
            if groove and c>.10:y-=.027*((c-.10)/.90)**.7
            ring.append(m.v((x,y,.255*scale*s)))
        rings.append(ring)
    for index,(a,b) in enumerate(zip(rings,rings[1:])):
        for i in range(n):
            j=(i+1)%n
            top=math.cos((i+.5)*math.tau/n)
            col=2 if top<-.4 else 3 if top>.1 and (rows[index][2] or rows[index+1][2]) else 0 if top>.4 else 1
            if len(a)==1:m.tri(a[0],b[i],b[j],col)
            elif len(b)==1:m.tri(a[i],b[0],a[j],col)
            else:m.tri(a[i],a[j],b[j],col);m.tri(a[i],b[j],b[i],col)
    # Sparse flour-colored crust freckles, not added raw oats/seeds or other ingredients.
    support=base.Model('CrustTop');support.vertices=[(x,z,-y) for x,y,z in m.vertices];support.faces=list(m.faces)
    for i in range(11):
        x=-.30+.06*i;z=.095*math.sin(i*2.4)
        q=kit.front_surface(support,x,z,.001)
        ellipsoid(m,(x,-q[2],z),(.014,.0028,.009),4,6)
    return m


def rice_bowl():
    m=base.Model('TomatoRiceBowl');bowl(m)
    ellipsoid(m,(0,.052,0),(.303,.104,.303),2,24)
    # Individual grains sit partly within the mound, with deterministic varied headings.
    for i in range(43):
        a=i*2.399963;r=.271*math.sqrt((i+.5)/43);x,z=r*math.cos(a),r*math.sin(a)
        y=.052+.104*math.sqrt(max(0,1-(r/.303)**2))
        grain=base.Model('CookedRiceGrain');ellipsoid(grain,(0,0,0),(.028,.009,.011),2 if i%4 else 3,6)
        turn=i*1.73
        append(m,grain,lambda p,x=x,y=y,z=z,turn=turn:(x+p[0]*math.cos(turn)-p[2]*math.sin(turn),y+p[1],z+p[0]*math.sin(turn)+p[2]*math.cos(turn)))
    # Cooked red tomato sections partly enter the rice; no extra green herb garnish.
    for x,z,turn in [(-.13,-.10,.4),(.11,-.12,-.6),(.10,.10,.9),(-.12,.12,-.2)]:
        y=.052+.104*math.sqrt(1-(math.hypot(x,z)/.303)**2)
        part=base.Model('TomatoPiece');ellipsoid(part,(0,0,0),(.070,.030,.052),4,10)
        for sx,sz in [(-.024,-.01),(.017,.009)]:ellipsoid(part,(sx,.028,sz),(.010,.003,.006),5,6)
        append(m,part,lambda p,x=x,y=y,z=z,turn=turn:(x+p[0]*math.cos(turn)-p[2]*math.sin(turn),y+p[1],z+p[0]*math.sin(turn)+p[2]*math.cos(turn)))
    seat=drinks.support(m)
    ellipsoid(m,seat(.055,-.065,.009),(.034,.033,.012),4,10)
    for x,y in [(-.055,-.04),(-.065,-.075),(-.04,-.105)]:
        ellipsoid(m,seat(x,y,.007),(.027,.008,.008),2,6)
    return m


def glazed_carrots():
    m=base.Model('HoneyGlazedCarrots');bowl(m,fill=None,shallow=True)
    # Stable low pile of chunky tapered carrot lengths, bounded by the serving dish.
    for index,(x,z,turn,length) in enumerate([(-.13,-.10,.2,.24),(.095,-.11,-.4,.25),
                 (-.115,.055,.7,.22),(.11,.075,-.6,.24),(0,.02,.10,.25),(-.04,.16,-.4,.18)]):
        part=base.Model('CookedCarrot');r=.043 if index<4 else .036
        kit.loft(part,[(-length/2,0,0),(-length/2,r,r*.85),(-length*.30,r*1.05,r*.92),
                       (length*.33,r*.70,r*.62),(length/2,r*.38,r*.34),(length/2,0,0)],
                 lambda k,i:4 if k in [0,4] else 3 if math.cos((i+.5)*math.tau/12)>.3 else 2,12,along_x=True)
        y=-.005 if index<4 else .052
        # Amber glints are embedded in the actual upper rind, rather than floating drips.
        ellipsoid(part,(-.025,r*.98,0),(.030,.003,.011),5,8)
        append(m,part,lambda p,x=x,y=y,z=z,turn=turn:(x+p[0]*math.cos(turn)-p[2]*math.sin(turn),y+p[1],z+p[0]*math.sin(turn)+p[2]*math.cos(turn)))
    seat=drinks.support(m)
    kit.closed_patch(m,[seat(-.12,-.02,.005),seat(-.02,-.02,.005),seat(-.07,-.095,.005)],(0,0,-1),.007,3)
    drinks.honey_relief(m,seat,.075,-.04)
    return m


SPECS=[
 ('BottledHoneyTea','Bottled honey tea',bottled_tea,[(161,108,36),(194,147,65),(225,229,190),(72,104,52),(50,77,39),(229,179,59),(150,110,63)],['glass','glass','glass','flat','flat','flat','wood']),
 ('CarrotSoup','Carrot soup',soup,[(150,87,57),(194,131,82),(218,118,40),(178,77,26),(240,150,65)],['ceramic','ceramic','flat','flat','flat']),
 ('BarleyBread','Barley bread',bread,[(171,105,48),(137,77,34),(108,59,30),(219,177,110),(226,196,141)],['grain','grain','grain','grain','grain']),
 ('TomatoRiceBowl','Tomato rice bowl',rice_bowl,[(153,94,62),(200,144,93),(225,213,164),(194,179,126),(184,62,35),(235,183,101)],['ceramic','ceramic','grain','grain','fruit_skin','flat']),
 ('HoneyGlazedCarrots','Honey-glazed carrots',glazed_carrots,[(188,181,151),(220,211,179),(175,80,28),(218,119,40),(237,148,64),(237,181,72)],['ceramic','ceramic','flat','flat','flat','flat'])]
RECIPES={'BottledHoneyTea':('T2-G18',12,'1 Tea herb + 1 Honey + 1 Glass bottle -> 1'),
 'CarrotSoup':('T2-G4',8,'3 Carrots + 1 Rock salt -> 1'),
 'BarleyBread':('T2-G10',10,'2 Barley flour + 1 Water -> 2'),
 'TomatoRiceBowl':('T2-G13',11,'2 Rice + 2 Tomatoes -> 2'),
 'HoneyGlazedCarrots':('T2-G14',11,'3 Carrots + 1 Honey -> 2')}
META={name:{'Representation':'display','VesselPolicy':'Illustrative serving vessel/closure; only the tea recipe explicitly consumes Glass bottle',
            'DepictedPortions':1,'BindingStatus':'Passive Food art; canonical output IDs/live bindings and gameplay pending'} for name,*_ in SPECS}
for name,slug in [('BarleyBread','barley_bread'),('TomatoRiceBowl','tomato_rice_bowl'),('HoneyGlazedCarrots','honey_glazed_carrots')]:META[name]['RecipeId']='recipe.food.'+slug

if __name__=='__main__':generate(20,SPECS,RECIPES,category='Food',role='Food',preview='--preview' in sys.argv,item_metadata=META)
