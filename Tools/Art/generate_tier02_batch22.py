"""Five distinct tier-two fish meals; documentary recipes, passive art only."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import generate_tier02_batch19 as drinks
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch20 import append,bowl
from generate_tier02_batch21 import grains,carrot
from tier02_batch_workflow import generate


def top_support(m):
    surface=base.Model('ReadOnlyFoodTop')
    surface.vertices=[(x,z,-y) for x,y,z in m.vertices];surface.faces=list(m.faces)
    def at(x,z,offset=.002):
        p=kit.front_surface(surface,x,z,offset);return (x,-p[2],z)
    return at


def fish_relief(m,pike=False,tomato=False):
    at=drinks.support(m);y=-.075
    w=.065 if pike else .052;h=.019 if pike else .033
    ellipsoid(m,at(-.02,y,.011),(w,h,.010),7,10)
    # A closed triangular tail and short head-side gill identify the fish motif.
    kit.closed_patch(m,[at(-w-.01,y,.01),at(-w-.045,y+.030,.01),at(-w-.045,y-.030,.01)],(0,0,-1),.008,7)
    stroke(m,[at(.009,y+h*.6,.022),at(.004,y-h*.6,.022)],4,.0021)
    if tomato:ellipsoid(m,at(.083,y,.013),(.029,.026,.012),5,10)
    else:
        for x,yy in [(.083,y+.025),(.092,y-.012)]:ellipsoid(m,at(x,yy,.010),(.023,.007,.008),5,6)


def fillet(m,x,y,z,length=.27,width=.10,turn=0,skin=7):
    part=base.Model('CookedFishFillet')
    rows=[(-length*.52,0,0),(-length*.46,.022,width*.55),(-length*.22,.034,width),
          (length*.15,.036,width*.96),(length*.41,.025,width*.58),(length*.52,0,0)]
    kit.loft(part,rows,lambda k,i:skin if math.cos((i+.5)*math.tau/12)<.30 else 3 if k%2 else 4,12,True)
    part.vertices=[(a,max(b,-.018),c) for a,b,c in part.vertices]
    at=top_support(part)
    # Three pale chevrons show cooked muscle flakes on the actual upper surface.
    for xx in [-length*.24,0,length*.24]:
        stroke(part,[at(xx-.014,-width*.44,.002),at(xx+.008,0,.002),at(xx-.014,width*.44,.002)],4,.0027)
    append(m,part,lambda p:(x+p[0]*math.cos(turn)-p[2]*math.sin(turn),y+p[1],z+p[0]*math.sin(turn)+p[2]*math.cos(turn)))


def rice(m):
    ellipsoid(m,(0,.056,0),(.303,.103,.303),2,24)
    at=top_support(m)
    for i in range(48):
        a=i*2.399963;r=.276*math.sqrt((i+.5)/48);x,z=r*math.cos(a),r*math.sin(a)
        pos=at(x,z,-.001);grain=base.Model('CookedRice')
        ellipsoid(grain,(0,0,0),(.025,.007,.009),5 if i%4 else 6,6)
        t=i*1.73
        append(m,grain,lambda p:(x+p[0]*math.cos(t)-p[2]*math.sin(t),pos[1]+p[1],z+p[0]*math.sin(t)+p[2]*math.cos(t)))


def pike_cakes():
    m=base.Model('PikeBarleyCakes');bowl(m,fill=None,shallow=True)
    patty=base.Model('OneCookedCake')
    kit.loft(patty,[(-.061,0,0),(-.061,.18,.18),(-.045,.235,.235),(.022,.242,.242),
          (.052,.22,.22),(.070,.17,.17),(.076,0,0)],lambda k,i:3 if k in [1,2] else 2 if k%2 else 4,20)
    patty.vertices=[(x*(1+.035*math.sin(math.atan2(z,x)*5)),y,z*(1+.025*math.cos(math.atan2(z,x)*7))) for x,y,z in patty.vertices]
    at=top_support(patty)
    # Sparse cream fish flakes and browned patches remain embedded in its crust.
    for i in range(25):
        a=i*2.399963;r=.202*math.sqrt((i+.5)/25);x,z=r*math.cos(a),r*math.sin(a)
        p=at(x,z,.001)
        ellipsoid(patty,p,(.015 if i%3 else .025,.0025,.009 if i%3 else .015),4 if i%3 else 3,6)
    append(m,patty);fish_relief(m,True);return m


def walleye_soup():
    m=base.Model('WalleyeBarleySoup');bowl(m)
    grains(m,32,.094,5)
    for x,z,t in [(-.12,-.08,.25),(.12,-.06,-.4),(0,.14,.7)]:fillet(m,x,.086,z,.145,.039,t)
    for x,z in [(-.20,.06),(.18,.12),(-.04,-.20),(.03,0)]:carrot(m,x,.098,z,.028,8)
    fish_relief(m);return m


def carp_rice():
    m=base.Model('CarpRiceBowl');bowl(m);rice(m)
    # Broad curved portions with warm skin differ from the long pike fillet.
    fillet(m,-.07,.139,-.07,.30,.083,.55)
    fillet(m,.095,.138,.10,.26,.071,.55)
    fish_relief(m);return m


def pike_rice():
    m=base.Model('PikeRiceBowl');bowl(m);rice(m)
    # A long tapered split fillet, olive skin at the edge, over the same exact staple.
    fillet(m,0,.145,-.055,.47,.060,-.25)
    fillet(m,.015,.14,.092,.36,.044,-.25)
    fish_relief(m,True);return m


def catfish_stew():
    m=base.Model('CatfishTomatoStew');bowl(m)
    # Rice is required even though tomato broth is the dominant visual identity.
    for i in range(18):
        a=i*2.399963;r=.255*math.sqrt((i+.5)/18)
        ellipsoid(m,(r*math.cos(a),.09,r*math.sin(a)),(.022,.006,.009),6,6)
    for x,z,t in [(-.13,-.08,.35),(.11,-.07,-.4),(.02,.14,.7)]:fillet(m,x,.091,z,.17,.047,t)
    for x,z in [(-.18,.13),(.20,.07),(.01,-.20),(-.02,-.005)]:
        ellipsoid(m,(x,.103,z),(.042,.022,.033),5,10)
    fish_relief(m,tomato=True);return m


SPECS=[
 ('PikeBarleyCakes','Pike barley cakes',pike_cakes,[(162,159,123),(217,209,167),(187,127,52),(127,75,30),(233,200,128),(221,203,147),(240,222,176),(86,98,53)],['ceramic','ceramic','grain','grain','grain','grain','grain','flat']),
 ('WalleyeBarleySoup','Walleye-barley soup',walleye_soup,[(139,153,156),(190,205,201),(173,149,85),(221,209,163),(247,234,189),(215,190,130),(240,217,166),(106,118,98),(217,112,35),(244,156,67)],['ceramic','ceramic','flat','flat','flat','grain','grain','flat','flat','flat']),
 ('CarpRiceBowl','Carp rice bowl',carp_rice,[(151,91,57),(200,139,84),(211,194,144),(184,172,153),(227,211,185),(224,207,159),(240,222,181),(132,97,51)],['ceramic','ceramic','grain','flat','flat','grain','grain','flat']),
 ('PikeRiceBowl','Pike rice bowl',pike_rice,[(104,123,106),(170,187,150),(214,199,149),(181,194,180),(227,232,209),(225,208,161),(242,225,187),(76,91,53)],['ceramic','ceramic','grain','flat','flat','grain','grain','flat']),
 ('CatfishTomatoStew','Catfish-tomato stew',catfish_stew,[(135,80,56),(191,129,84),(175,64,32),(226,211,169),(250,234,192),(216,94,43),(235,220,172),(95,108,103)],['ceramic','ceramic','flat','flat','flat','fruit_skin','grain','flat'])]
RECIPES={
 'PikeBarleyCakes':('T2-E7',9,'1 Northern pike + 1 Barley flour + 1 Egg -> 2'),
 'WalleyeBarleySoup':('T2-E9',10,'1 Walleye + 2 Barley + 1 Carrot -> 1'),
 'CarpRiceBowl':('T2-E11',11,'2 Rice + 2 Common carp + 1 Rock salt -> 2'),
 'PikeRiceBowl':('T2-E12',11,'2 Rice + 2 Northern pike + 1 Rock salt -> 2'),
 'CatfishTomatoStew':('T2-E13',11,'1 Channel catfish + 2 Tomatoes + 1 Rice -> 1')}
META={name:{'Representation':'display','DepictedPortions':1,
 'VesselPolicy':'Illustrative ceramic serving dish, not a recipe input',
 'BindingStatus':'Passive Food art; canonical output IDs/live bindings unresolved'} for name,*_ in SPECS}
for name,slug in [('PikeBarleyCakes','pike_barley_cakes'),('WalleyeBarleySoup','walleye_barley_soup'),('CatfishTomatoStew','catfish_tomato_stew')]:META[name]['RecipeId']='recipe.food.'+slug
for name in ['CarpRiceBowl','PikeRiceBowl']:META[name]['SaltPolicy']='Plan Salt is exact Rock salt per section 6 source identity clarification; no substitution'
META['PikeRiceBowl']['IdentityPolicy']='Independent recipe/output from Carp rice bowl; no shared output binding inferred'
META['PikeBarleyCakes']['SourcePolicy']='Mission T2-E7; Barley flour conversion and exact Egg source gates; one cake per inventory portion'

if __name__=='__main__':generate(22,SPECS,RECIPES,category='Food',role='Food',preview='--preview' in sys.argv,item_metadata=META)
