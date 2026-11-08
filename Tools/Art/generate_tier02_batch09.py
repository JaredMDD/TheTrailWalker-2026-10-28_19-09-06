"""Batch 09: Barley flour, Flax oil, Beeswax. Passive Material art only."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import ellipsoid,stroke
from generate_tier02_batch06 import tube
from generate_tier02_batch08 import slab
from generate_tier02_batch07 import outline
from tier02_batch_workflow import generate


def flour():
    m=base.Model('BarleyFlour')
    # Full sack with folded mouth around a seated pale flour mound.
    kit.loft(m,[(-.37,0,0),(-.37,.20,.145),(-.33,.30,.21),(-.16,.33,.225),
                (.10,.31,.22),(.20,.28,.20),(.24,.29,.21),(.265,.27,.19),
                (.245,.245,.165),(.17,.24,.16),(.12,0,0)],lambda k,i:1 if i%5==0 else 0,20)
    ellipsoid(m,(0,.23,0),(.244,.13,.165),2,20)
    # Closed raised barley-ear pictogram seated to the actual front sleeve.
    support=base.Model('Support');support.vertices=list(m.vertices);support.faces=list(m.faces)
    stem=[kit.front_surface(support,.012,y,.004) for y in [-.24,-.14,-.04,.06]]
    stroke(m,stem,3,.005)
    for y in (-.15,-.085,-.02):
        for sign in (-1,1):
            p=kit.front_surface(support,sign*.035,y,.004)
            ellipsoid(m,p,(.028,.043,.010),3,8)
            end=kit.front_surface(support,sign*.063,y+.066,.004)
            stroke(m,[p,end],3,.0023)
    # Two sleeve seams with cross stitches follow the curved front surface.
    for sign in (-1,1):
        for y in [-.26,-.19,-.12,-.05,.02,.09]:
            stroke(m,[kit.front_surface(support,sign*.19-.008,y-.007,.003),kit.front_surface(support,sign*.19+.008,y+.007,.003)],1,.0028)
    return m


def oil():
    m=base.Model('FlaxOil')
    kit.loft(m,[(-.46,0,0),(-.46,.14,.13),(-.43,.20,.17),(-.34,.21,.18),
                (.12,.21,.18),(.20,.19,.16),(.29,.11,.10),(.35,.09,.085),
                (.40,.10,.09),(.43,.10,.09),(.43,0,0)],lambda k,i:(1 if i in (11,12) else 0) if k<5 else 2,24)
    kit.loft(m,[(.41,0,0),(.41,.084,.077),(.465,.087,.080),(.48,.070,.065),(.48,0,0)],3,16)
    support=base.Model('Support');support.vertices=list(m.vertices);support.faces=list(m.faces)
    # A blue flax blossom distinguishes oil from honey and sap. Five petal reliefs.
    for i in range(5):
        a=i*math.tau/5
        x=.052*math.sin(a);y=-.06+.052*math.cos(a)
        ellipsoid(m,kit.front_surface(support,x,y,.005),(.038,.038,.012),4,10)
    ellipsoid(m,kit.front_surface(support,0,-.06,.019),(.019,.019,.010),5,8)
    # Sparse glints reinforce stylized filled glass, no transparency pipeline.
    for x in (-.15,.15):
        stroke(m,[kit.front_surface(support,x,y,.004) for y in [-.30,-.20,-.10,.02]],1,.004)
    return m


def wax():
    m=base.Model('Beeswax')
    # Cast slab with chamfered corners and a smaller snapped-off slice.
    slab(m,outline(.67,.45,.045),(-.04,.04,.012),-.08,.17,True)
    slab(m,[(-.18,-.07),(.16,-.08),(.18,.04),(.02,.08),(-.16,.06)],(.18,-.24,-.015),.14,.10,True)
    support=base.Model('Support');support.vertices=list(m.vertices);support.faces=list(m.faces)
    # Three shallow honeycomb motifs, geometric rather than lettering.
    for cx,cy in [(-.15,.04),(.025,.05),(-.065,.18)]:
        points=[]
        for i in range(7):
            a=i*math.tau/6
            points.append(kit.front_surface(support,cx+.068*math.cos(a),cy+.058*math.sin(a),.002))
        tube(m,points,.0045,4,6)
    return m


FLOUR=[(155,127,89),(112,88,61),(223,207,168),(119,91,47)]
OIL=[(181,123,33),(232,184,78),(139,161,127),(155,110,66),(101,131,185),(219,191,94)]
WAX=[(220,173,65),(195,143,40),(241,198,92),(173,126,37),(185,132,38)]
SPECS=[('BarleyFlour','Barley flour',flour,FLOUR,['cloth','cloth','grain','flat']),
       ('FlaxOil','Flax oil',oil,OIL,['glass','glass','glass','wood','flat','flat']),
       ('Beeswax','Beeswax',wax,WAX,['flat']*5)]
RECIPES={'BarleyFlour':('T2-G9',9,'3 Barley -> 2; camp/home 5 s'),
         'FlaxOil':('T2-G11',10,'3 Flax seeds -> 1; fixed Material timer'),
         'Beeswax':('T2-G12',11,'2 Honey -> 1; fixed Material timer; no mastery')}

if __name__=='__main__':generate(9,SPECS,RECIPES,preview='--preview' in sys.argv)
