"""Five exact-plan food displays, including November 7 beaver glaze / G22 changes."""
import sys, math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import generate_tier02_batch19 as drinks
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch06 import tube
from generate_tier02_batch20 import append,bowl
from tier02_batch_workflow import generate


def pot(m):
    kit.loft(m,[(-.22,0,0),(-.22,.23,.23),(-.19,.29,.29),(-.08,.35,.35),
        (.12,.35,.35),(.145,.365,.365),(.16,.35,.35),(.16,.322,.322),
        (.12,.315,.315),(-.155,.24,.24),(-.155,0,0)],lambda k,i:1 if k in [4,5,6,7] else 0,24)
    kit.loft(m,[(-.14,0,0),(-.14,.235,.235),(.095,.312,.312),(.095,0,0)],2,24)
    for sign in [-1,1]:
        # Ceramic lug handles penetrate their own wall at each attachment.
        tube(m,[(sign*(.337+.13*math.sin(math.pi*i/12)),.04,.095*math.cos(math.pi*i/12)) for i in range(13)],.027,1,8)


def carrot(m,x,y,z,r=.038,slot=5):
    ellipsoid(m,(x,y,z),(r,.016,r*.85),slot,10)
    ellipsoid(m,(x,y+.014,z),(r*.55,.002,r*.49),slot+1,10)


def meat(m,x,y,z,r=.065,pale=False):
    part=base.Model('CookedMeat')
    kit.loft(part,[(-.038,0,0),(-.038,r*.80,r*.70),(-.018,r,r*.83),
        (.035,r*.91,r*.78),(.052,r*.60,r*.55),(.052,0,0)],lambda k,i:4 if k>=3 else 3,10)
    phase=(x*37+z*53)
    part.vertices=[(px*(1+.10*math.sin(math.atan2(pz,px)*3+phase)),
                    py+.007*math.sin(px*31+pz*19+phase),
                    pz*(1+.10*math.cos(math.atan2(pz,px)*3+phase))) for px,py,pz in part.vertices]
    append(m,part,lambda p:(p[0]+x,p[1]+y,p[2]+z))
    # Short pale cooked fibres sit within the top surface, not dangling garnish.
    for j in [-1,1]:
        stroke(m,[(x+j*.016-.01,y+.052,z-.017),(x+j*.016+.01,y+.052,z+.012)],4,.002)


def mushroom(m,x,y,z,r=.055):
    # Stems enter stew; low domed caps have pale undersides and darker crowns.
    m.cylinder((x,y-.065,z),(x,y+.006,z),r*.23,r*.28,8,6,6)
    cap=base.Model('CookedMushroomCap')
    kit.loft(cap,[(-.009,0,0),(-.009,r,r*.80),(.009,r*1.06,r*.85),
        (.029,r*.80,r*.65),(.043,r*.37,r*.30),(.046,0,0)],lambda k,i:6 if k==0 else 5,12)
    append(m,cap,lambda p:(p[0]+x,p[1]+y,p[2]+z))


def grains(m,count,y,slot=5,mound=False):
    for i in range(count):
        a=i*2.399963;r=.27*math.sqrt((i+.5)/count)
        x,z=r*math.cos(a),r*math.sin(a)
        h=y+(.073*math.sqrt(max(0,1-(r/.30)**2)) if mound else 0)
        grain=base.Model('CookedBarley')
        ellipsoid(grain,(0,0,0),(.018,.009,.012),slot if i%4 else slot+1,6)
        turn=i*1.71
        append(m,grain,lambda p:(x+p[0]*math.cos(turn)-p[2]*math.sin(turn),h+p[1],z+p[0]*math.sin(turn)+p[2]*math.cos(turn)))


def emblem(m,kind):
    seat=drinks.support(m)
    if kind=='carrot':
        kit.closed_patch(m,[seat(-.05,-.025,.006),seat(.05,-.025,.006),seat(0,-.14,.006)],(0,0,-1),.008,5)
        for y in [-.05,-.077]:stroke(m,[seat(-.018,y,.014),seat(.021,y,.014)],6,.0023)
    elif kind=='barley':
        for x,y in [(-.025,-.035),(.025,-.065),(-.025,-.095)]:ellipsoid(m,seat(x,y,.009),(.025,.009,.009),5,6)
        stroke(m,[seat(0,-.13,.01),seat(0,-.015,.01)],6,.002)
    elif kind=='mushroom':
        # Small surface-projected pictogram remains legible in a level front view.
        for x,y in [(-.045,-.055),(0,-.025),(.045,-.055)]:
            ellipsoid(m,seat(x,y,.01),(.029,.020,.010),5,8)
        stroke(m,[seat(0,-.06,.013),seat(0,-.13,.013)],6,.012)
    elif kind=='truffle':
        ellipsoid(m,seat(0,-.065,.014),(.052,.043,.016),7,10)
        for x,y in [(-.024,-.05),(.012,-.045),(.023,-.075),(-.014,-.086)]:
            stroke(m,[seat(x-.009,y-.005,.031),seat(x+.009,y+.005,.031)],8,.0024)


def glazed_beaver():
    m=base.Model('BlackberryGlazedBeaver');bowl(m,fill=None,shallow=True)
    # One substantial cooked cut, lower face seated on the dish floor.
    kit.loft(m,[(-.055,0,0),(-.055,.19,.14),(-.025,.235,.17),(.045,.23,.165),
        (.105,.21,.15),(.135,.15,.105),(.14,0,0)],lambda k,i:3 if k<4 else 4,18)
    # Copy actual crown triangles so the glaze has exactly the same facets.
    # Each shallow closed prism seats into the cut; no chord can sink through it.
    crown=[]
    for a,b,c,_ in m.faces:
        pts=[m.vertices[i-1] for i in (a,b,c)]
        if min(p[1] for p in pts)>=.105-1e-8:crown.append(pts)
    for pts in crown:
        kit.closed_patch(m,[(x,y+.003,z) for x,y,z in pts],(0,1,0),.006,7)
    # Two grouped blackberries identify the new glaze, rather than another plain roast.
    for cx,cz in [(-.16,-.17),(.17,-.13)]:
        ellipsoid(m,(cx,.035,cz),(.051,.047,.044),7,10)
        for row in range(3):
            y=.012+row*.024;rr=.035 if row<2 else .023
            for j in range(6):
                a=j*math.tau/6+row*.4
                ellipsoid(m,(cx+rr*math.cos(a),y,cz+rr*math.sin(a)),(.019,.018,.019),7 if j%3 else 8,6)
    seat=drinks.support(m)
    for row in range(3):
        for j in range(3 if row==1 else 2):
            x=(j-(2 if row==1 else 1)/2)*.026
            ellipsoid(m,seat(x,-.018-row*.022,.009),(.018,.017,.009),7 if j%2 else 8,6)
    return m


def carrot_stew():
    m=base.Model('BeaverCarrotStew');bowl(m)
    for x,z in [(-.12,-.09),(.10,.09),(.13,-.09),(-.10,.11)]:meat(m,x,.068,z)
    for x,z in [(0,-.19),(-.21,0),(.20,.02),(-.02,.17),(0,0)]:carrot(m,x,.096,z)
    emblem(m,'carrot');return m


def rabbit_soup():
    m=base.Model('RabbitBarleySoup');bowl(m);grains(m,36,.093)
    for x,z in [(-.13,-.09),(.13,-.08),(.03,.14)]:meat(m,x,.065,z,.060,True)
    # Tea herb is the exact farm ingredient; no extra parsley/carrot/mushroom.
    for x,z in [(-.13,.09),(.12,.10)]:
        kit.closed_patch(m,[(x-.035,.101,z),(x,.109,z+.022),(x+.04,.101,z),(x,.098,z-.016)],(0,1,0),.006,7)
    emblem(m,'barley');return m


def mushroom_pot():
    m=base.Model('BeaverMushroomPot');pot(m)
    for x,z in [(-.13,-.10),(.12,.04),(-.025,.16)]:meat(m,x,.08,z)
    for x,z in [(-.13,.09),(.115,-.12),(.02,-.04)]:mushroom(m,x,.123,z,.067)
    # Carrot is present but secondary to the mushroom identity.
    carrot(m,-.20,.108,-.04,.027,7);carrot(m,.18,.108,.13,.027,7)
    emblem(m,'mushroom');return m


def truffle_pot():
    m=base.Model('CreamyTruffleBarleyPot');pot(m)
    ellipsoid(m,(0,.075,0),(.303,.075,.303),2,24);grains(m,48,.075,5,True)
    # Thin irregular truffle sections sit within the cream/barley crown.
    for x,z,turn in [(-.13,-.11,.3),(.12,-.08,-.4),(.03,.14,.6)]:
        r=math.hypot(x,z);y=.075+.073*math.sqrt(1-(r/.30)**2)
        part=base.Model('TruffleSlice')
        outline=[(math.cos(i*math.tau/11)*(.057+.004*math.sin(i*2)),.005,math.sin(i*math.tau/11)*.042) for i in range(11)]
        kit.closed_patch(part,outline,(0,1,0),.012,7)
        stroke(part,[(-.04,.007,-.01),(-.013,.007,.004),(.011,.007,-.009),(.043,.007,.011)],8,.0026)
        stroke(part,[(.011,.007,-.009),(.02,.007,-.028)],8,.002)
        append(m,part,lambda p:(x+p[0]*math.cos(turn)-p[2]*math.sin(turn),y+p[1],z+p[0]*math.sin(turn)+p[2]*math.cos(turn)))
    emblem(m,'truffle');return m


# Stable shared slot meanings: vessel / lip / broth / meat / cooked fibre /
# orange or barley or mushroom / light accent / berry, herb or truffle / vein.
SPECS=[
 ('BlackberryGlazedBeaver','Blackberry-glazed beaver',glazed_beaver,[(181,173,146),(222,213,180),(102,58,38),(106,58,34),(146,81,48),(112,51,48),(182,117,68),(65,30,53),(113,53,78)],['ceramic','ceramic','flat','flat','flat','flat','flat','fruit_skin','fruit_skin']),
 ('BeaverCarrotStew','Beaver-carrot stew',carrot_stew,[(142,80,51),(194,128,78),(133,79,39),(101,55,32),(164,100,58),(220,110,36),(243,154,64)],['ceramic','ceramic','flat','flat','flat','flat','flat']),
 ('RabbitBarleySoup','Rabbit barley soup',rabbit_soup,[(162,145,106),(208,190,143),(174,140,75),(182,149,102),(218,188,139),(219,196,142),(243,222,173),(68,91,42)],['ceramic','ceramic','flat','flat','flat','grain','grain','flat']),
 ('BeaverMushroomPot','Beaver mushroom pot',mushroom_pot,[(102,62,44),(161,103,62),(121,77,42),(91,49,30),(153,93,54),(164,114,66),(221,184,128),(218,111,35),(241,154,69)],['ceramic','ceramic','flat','flat','flat','flat','flat','flat','flat']),
 ('CreamyTruffleBarleyPot','Creamy truffle-barley pot',truffle_pot,[(112,130,110),(174,185,150),(212,194,143),(96,61,39),(165,125,73),(225,204,155),(242,224,182),(54,43,35),(159,137,99)],['ceramic','ceramic','flat','flat','flat','grain','grain','flat','flat'])]
RECIPES={
 'BlackberryGlazedBeaver':('T2-H2',7,'1 Raw beaver meat + 1 Blackberries + 2 Birch branches -> 1'),
 'BeaverCarrotStew':('T2-H8',8,'2 Raw beaver meat + 2 Carrots + 1 Rock salt -> 2'),
 'RabbitBarleySoup':('T2-H10',9,'2 Rabbit meat + 2 Barley + 1 Tea herb -> 2'),
 'BeaverMushroomPot':('T2-H13',8,'2 Beaver meat + 2 Mushrooms + 1 Carrot -> 2'),
 'CreamyTruffleBarleyPot':('T2-G22',10,'2 Truffle + 2 Barley + 1 Cow cream -> 1')}
SLUGS=['blackberry_glazed_beaver','beaver_carrot_stew',None,None,'truffle_barley_pot']
META={name:{'Representation':'display','DepictedPortions':1,
 'VesselPolicy':'Illustrative serving vessel; no additional recipe inputs',
 'BindingStatus':'Passive Food art; canonical output IDs/live bindings unresolved'} for name,*_ in SPECS}
for row,slug in zip(SPECS,SLUGS):
    if slug:META[row[0]]['RecipeId']='recipe.food.'+slug
META['BlackberryGlazedBeaver']['FuelPolicy']='Two Birch branches are consumed cooking fuel, not an edible garnish'
META['CreamyTruffleBarleyPot']['UnlockPolicy']='T2-G22 optional and nonblocking; collect 10 Truffle; Barley region 9 and exact Cow cream source G13 required'
META['RabbitBarleySoup']['SourcePolicy']='Tea herbs from farm; rabbit meat from tier one'
META['BeaverMushroomPot']['SourcePolicy']='Mushrooms from region 4; appended H13 opens with region 8'

if __name__=='__main__':generate(21,SPECS,RECIPES,category='Food',role='Food',preview='--preview' in sys.argv,item_metadata=META)
