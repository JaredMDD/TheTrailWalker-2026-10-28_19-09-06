"""Batch 12: Normal/Elite birch rods with hemp line and copper hook."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from mathutils import Vector
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch06 import tube
from generate_tier02_batch02 import fin,stroke
from generate_tier02_batch07 import annulus
from tier02_batch_workflow import generate

COLORS=[(220,211,182),(174,158,122),(75,66,52),(172,148,93),(120,99,63),
        (184,104,57),(230,154,94),(137,73,41),(202,186,145),(156,139,105),(223,207,167)]
TYPES=['bark','wood','bark','plant_fibre','plant_fibre','metal','metal','metal','cloth','cloth','cloth']


def shaft_x(y):return -.17+.16*(y+.72)+.065*((y+.72)/1.5)**2


def tapered_shaft(m):
    n=12;rings=[]
    for y in [-.73,-.68,-.50,-.26,0,.24,.46,.64,.79]:
        t=(y+.73)/1.52;r=.036*(1-t)+.011*t
        rings.append([m.v((shaft_x(y)+r*math.cos(i*math.tau/n),y,r*math.sin(i*math.tau/n))) for i in range(n)])
    for a,b in zip(rings,rings[1:]):
        for i in range(n):
            j=(i+1)%n;m.tri(a[i],a[j],b[j],0);m.tri(a[i],b[j],b[i],0)
    for ring,y in [(rings[0],-.73),(rings[-1],.79)]:
        c=m.v((shaft_x(y),y,0))
        for i in range(n):m.tri(c,ring[i],ring[(i+1)%n],1)


def rod(elite=False):
    m=base.Model('EliteBirchRod' if elite else 'NormalBirchRod');tapered_shaft(m)
    # Side-to-side bark marks lie on the narrow shaft; no bamboo nodes.
    for sign in (-1,1):
        for i,y in enumerate([-.66,-.36,-.16,.10,.33,.53,.67]):
            x=shaft_x(y);r=.036*(1-(y+.73)/1.52)+.011*((y+.73)/1.52)
            w=r*.62
            fin(m,[(x-w,y-.003,sign*r),(x+w,y-.005,sign*r),(x+w*.7,y+.003,sign*r),(x-w*.7,y+.004,sign*r)],2,sign*.002)
    # Normal hemp handgrip and short tip binding use the existing cord input.
    for ya,yb,turns,radius in [(-.64,-.40,8,.040),(.70,.77,5,.017)]:
        pts=[]
        for i in range(turns*12+1):
            t=i/(turns*12);y=ya+(yb-ya)*t;a=t*turns*math.tau
            pts.append((shaft_x(y)+radius*math.cos(a),y,radius*math.sin(a)))
        tube(m,pts,.0055 if ya<0 else .0035,3,5)
    tip=(shaft_x(.765),.765,-.014)
    line=[tip,(.21,.66,-.018),(.34,.43,-.023),(.385,.18,-.022),(.39,-.08,-.018),(.39,-.28,-.01)]
    tube(m,line,.005,3,6)
    # Copper hook: true closed eye, long shank, rounded bend, point and barb.
    eye=base.Model('HookEye');annulus(eye,(.39,-.295,-.01),.020,.011,.010,0,20)
    off=len(m.vertices);m.vertices+=eye.vertices
    mapping={0:5,1:6,2:6,3:7};m.faces +=[(a+off,b+off,c+off,mapping[k]) for a,b,c,k in eye.faces]
    hook=[(.39,-.314,-.01),(.39,-.40,-.01),(.384,-.445,-.01),(.36,-.47,-.01),
          (.325,-.47,-.01),(.300,-.45,-.01),(.295,-.415,-.01),(.305,-.392,-.01)]
    tube(m,hook,.011,5,10)
    tube(m,[(.305,-.392,-.01),(.321,-.419,-.01)],.0055,6,6)
    # The line passes inside the eye and ends in a short seated knot.
    tube(m,[(.39,-.28,-.01),(.39,-.295,-.025),(.405,-.303,-.012),(.397,-.311,-.005)],.0038,4,6)
    if elite:
        # Previous-tier fine hemp cloth covers the original grip without changing rod.
        sleeve=base.Model('FineHempGrip')
        profile=[(-.655,0,0),(-.655,.050,.046),(-.63,.055,.049),(-.40,.052,.047),(-.38,.046,.043),(-.38,0,0)]
        kit.loft(sleeve,profile,8,20)
        off=len(m.vertices);m.vertices +=[(x+shaft_x(y),y,z) for x,y,z in sleeve.vertices]
        m.faces +=[(a+off,b+off,c+off,k) for a,b,c,k in sleeve.faces]
        # Fine woven diagonal wrap, hem and short matching cloth tie.
        for i in range(7):
            y=-.62+i*.032
            pts=[(shaft_x(y)+.052*math.cos(a),y+.012*j/12,.048*math.sin(a)) for j,a in enumerate([math.pi+j*math.pi/12 for j in range(13)])]
            tube(m,pts,.0027,10,5)
        tube(m,[(shaft_x(-.39),-.39,-.045),(shaft_x(-.37)+.030,-.37,-.055),(shaft_x(-.34)+.046,-.34,-.06)],.008,9,6,flat=True)
    return m


SPECS=[('NormalBirchRod','Normal birch rod',lambda:rod(False),COLORS,TYPES),
       ('EliteBirchRod','Elite birch rod',lambda:rod(True),COLORS,TYPES)]
RECIPES={'NormalBirchRod':('T2-E5',8,'2 Birch branches + 1 Copper fitting + 1 Hemp cord -> 1'),
         'EliteBirchRod':('T2-E8',10,'1 Normal birch rod + 2 Fine hemp cloth -> 1')}

if __name__=='__main__':generate(12,SPECS,RECIPES,category='Tools',role='Tool',preview='--preview' in sys.argv)
