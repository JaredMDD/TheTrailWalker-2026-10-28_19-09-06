"""Five tier-two Food illustrations. Vessels are presentation, not recipe inputs."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch06 import tube
from tier02_batch_workflow import generate


def cup(m,wide=False,pale=False,handle=False):
    w=.31 if wide else .25;h=.20 if wide else .34
    kit.loft(m,[(-h,0,0),(-h,w*.72,w*.72),(-h+.04,w*.84,w*.84),
                (h-.06,w,w),(h,w*1.03,w*1.03),(h+.014,w,w),
                (h+.014,w*.91,w*.91),(h-.018,w*.90,w*.90),
                (-h+.06,w*.71,w*.71),(-h+.06,0,0)],lambda k,i:1 if k in [3,4,5,6] else 0,24)
    kit.loft(m,[(-h+.075,0,0),(-h+.075,w*.70,w*.70),(h-.055,w*.89,w*.89),
                (h-.05,w*.87,w*.87),(h-.05,0,0)],2,24)
    if handle:
        pts=[]
        for i in range(17):
            a=-math.pi/2+math.pi*i/16
            # Both capped ends penetrate the tapered outer wall at their own heights.
            pts.append((w*(.87+.055*math.sin(a))+.145*math.cos(a),h*.72*math.sin(a),0))
        tube(m,pts,.026,1,8)
    return h,w


def support(m):
    q=base.Model('ReadOnlyVesselSurface');q.vertices=list(m.vertices);q.faces=list(m.faces)
    return lambda x,y,offset=.003:kit.front_surface(q,x,y,offset)


def leaf_relief(m,seat,cx,cy,w=.045,h=.085,slot=3):
    coords=[(cx,cy-h),(cx+w*.7,cy-h*.35),(cx+w,cy+h*.10),(cx,cy+h),
            (cx-w,cy+h*.10),(cx-w*.7,cy-h*.35)]
    # Surface patches follow the vessel instead of a flat floating medallion.
    center=seat(cx,cy,.006)
    for a,b in zip(coords,coords[1:]+coords[:1]):
        kit.closed_patch(m,[center,seat(*a,.006),seat(*b,.006)],(0,0,-1),.007,slot)
    stroke(m,[seat(cx,cy-h*.8,.012),seat(cx,cy+h*.8,.012)],4,.0018)


def honey_relief(m,seat,x,y):
    points=[(x,y+.045),(x+.025,y+.004),(x+.022,y-.025),(x,y-.035),(x-.022,y-.025),(x-.025,y+.004)]
    c=seat(x,y,.006)
    for a,b in zip(points,points[1:]+points[:1]):kit.closed_patch(m,[c,seat(*a,.006),seat(*b,.006)],(0,0,-1),.006,5)


def blackberry_juice():
    m=base.Model('BlackberryJuice');cup(m);seat=support(m)
    # One embossed berry icon with a compound drupelet surface, no extra garnish.
    for row,(y,n) in enumerate([(-.085,2),(-.04,3),(.006,3),(.052,2)]):
        for j in range(n):
            x=(j-(n-1)/2)*.042
            ellipsoid(m,seat(x,y,.009),(.028,.027,.015),3 if (j+row)%2 else 4,8)
    stroke(m,[seat(0,.07,.007),seat(.012,.115,.007)],6,.003)
    return m


def sap_milk():
    m=base.Model('BirchSapMilkDrink');cup(m,handle=True);seat=support(m)
    leaf_relief(m,seat,0,.015,.063,.105,3)
    # Sap droplet beneath the birch-leaf pictogram identifies the drink's exact base.
    honey_relief(m,seat,0,-.16)
    return m


def jam():
    m=base.Model('BlueberryJam')
    # Bulbous jar, narrower throat, true open inner wall and broad rolled lip.
    kit.loft(m,[(-.25,0,0),(-.25,.19,.19),(-.22,.26,.26),(-.10,.30,.30),
                (.10,.28,.28),(.17,.245,.245),(.205,.245,.245),(.22,.27,.27),
                (.24,.27,.27),(.24,.23,.23),(.19,.225,.225),(-.17,.24,.24),(-.19,0,0)],
             lambda k,i:1 if k in [6,7,8,9] else 0,24)
    kit.loft(m,[(-.18,0,0),(-.18,.23,.23),(.16,.224,.224),(.175,.215,.215),(.175,0,0)],2,24)
    # Cooked fruit surface is visibly thicker than a flat drink meniscus.
    for x,z,rx,rz in [(-.08,.02,.105,.08),(.07,-.035,.11,.09),(.0,.095,.09,.075)]:
        ellipsoid(m,(x,.173,z),(rx,.018,rz),3,12)
    seat=support(m)
    for x,y in [(-.060,-.025),(.055,-.035),(.0,.062)]:
        ellipsoid(m,seat(x,y,.008),(.052,.050,.016),4,10)
        c=seat(x,y,.028)
        # Five-lobed calyx mark on the blueberry relief, no added flower garnish.
        for i in range(5):
            a=math.tau*i/5
            stroke(m,[c,(c[0]+.018*math.cos(a),c[1]+.018*math.sin(a),c[2])],6,.0024)
    return m


def tea():
    m=base.Model('HoneyHerbalTea');h,w=cup(m,wide=True,handle=True);seat=support(m)
    leaf_relief(m,seat,-.055,-.01,.033,.060,3);honey_relief(m,seat,.065,-.01)
    # Two tea-herb leaves partly immersed in the visible amber surface.
    for x,z,sign in [(-.07,.025,1),(.055,-.055,-1)]:
        kit.closed_patch(m,[(x-.05,h-.045,z),(x,h-.035,z+.025*sign),
                            (x+.055,h-.045,z),(x,h-.040,z-.025*sign)],(0,1,0),.009,3)
    return m


def oat_drink():
    m=base.Model('HoneyOatDrink');h,w=cup(m);seat=support(m)
    stroke(m,[seat(-.03,y,.006) for y in [-.15,-.07,.02,.12]],3,.004)
    for y in [-.08,-.012,.056]:
        for sign in [-1,1]:
            ellipsoid(m,seat(-.03+sign*.025,y,.008),(.021,.042,.012),3,8)
    honey_relief(m,seat,.09,-.08)
    for i in range(11):
        a=i*2.399;r=.15*math.sqrt((i+.5)/11)
        ellipsoid(m,(r*math.cos(a),h-.048,r*math.sin(a)),(.022,.004,.012),4,8)
    return m


SPECS=[
 ('BlackberryJuice','Blackberry juice',blackberry_juice,[(151,88,63),(188,127,88),(84,35,67),(58,28,60),(111,48,91),(213,171,88),(78,102,58)],['ceramic','ceramic','flat','fruit_skin','fruit_skin','flat','flat']),
 ('BirchSapMilkDrink','Birch sap milk drink',sap_milk,[(189,180,150),(222,210,174),(238,231,204),(100,125,72),(63,94,52),(227,223,196)],['ceramic','ceramic','flat','flat','flat','flat']),
 ('BlueberryJam','Blueberry jam',jam,[(94,104,134),(162,175,186),(63,37,87),(84,48,104),(74,88,143),(199,182,145),(40,49,82)],['glass','glass','flat','flat','flat','flat','flat']),
 ('HoneyHerbalTea','Honey herbal tea',tea,[(153,100,60),(204,151,95),(169,115,40),(71,104,56),(91,81,42),(227,174,65)],['ceramic','ceramic','flat','flat','flat','flat']),
 ('HoneyOatDrink','Honey oat drink',oat_drink,[(171,128,79),(212,174,119),(220,206,163),(136,102,54),(175,144,84),(225,166,53)],['ceramic','ceramic','flat','grain','grain','flat'])]
RECIPES={'BlackberryJuice':('T2-G2',7,'3 Blackberries -> 1'),
 'BirchSapMilkDrink':('T2-G3',7,'2 Birch sap + 1 Cow milk -> 1'),
 'BlueberryJam':('T2-G5',9,'3 Blueberries -> 1'),
 'HoneyHerbalTea':('T2-G15',11,'1 Tea herb + 1 Honey + 1 Water -> 1'),
 'HoneyOatDrink':('T2-G16',11,'1 Oats + 1 Honey + 1 Cow milk -> 1')}
META={name:{'Representation':'display','VesselPolicy':'Illustrative serving vessel; no extra recipe input or container output','BindingStatus':'Passive Food art; live IDs/bindings and gameplay pending'} for name,*_ in SPECS}
META['BirchSapMilkDrink'].update(ItemId='item.birch_sap_milk_drink',RecipeId='recipe.food.birch_sap_milk_drink')

if __name__=='__main__':generate(19,SPECS,RECIPES,category='Food',role='Food',preview='--preview' in sys.argv,item_metadata=META)
