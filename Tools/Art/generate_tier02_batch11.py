"""Batch 11: Normal/Elite birch slingshots, passive inventory art."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch02 import fin,stroke,ellipsoid
from generate_tier02_batch06 import tube
from tier02_geometry import beveled_profile
from tier02_batch_workflow import generate

COLORS=[(222,214,188),(176,161,128),(71,63,51),(119,76,47),(156,105,63),
        (173,149,96),(117,96,62),(205,185,146),(164,143,106),(228,211,175)]
TYPES=['bark','wood','bark','leather','leather','plant_fibre','plant_fibre','fur','fur','flat']


def slingshot(elite=False):
    m=base.Model('EliteBirchSlingshot' if elite else 'NormalBirchSlingshot')
    # A single closed fork, avoiding two coincident cylinders through the grip.
    points=[(-.07,-.43),(.06,-.43),(.065,-.15),(.095,-.06),(.19,.10),(.29,.34),
            (.275,.39),(.21,.40),(.18,.36),(.115,.17),(.035,.06),(0,.03),
            (-.035,.07),(-.12,.20),(-.18,.37),(-.21,.40),(-.275,.38),(-.29,.33),
            (-.20,.12),(-.10,-.04),(-.065,-.15)]
    beveled_profile(m,points,.10,0,1,.014)
    # Brown beaver-hide grip sleeve with continuous closed cross-section.
    grip=base.Model('Grip')
    kit.loft(grip,[(-.37,0,0),(-.37,.074,.064),(-.34,.078,.066),(-.13,.074,.063),
                   (-.10,.065,.058),(-.10,0,0)],3,16)
    m.vertices+=grip.vertices;off=len(m.vertices)-len(grip.vertices)
    m.faces +=[(a+off,b+off,c+off,k) for a,b,c,k in grip.faces]
    # Thongs meet the tip and pouch at each end; retained game art leather styling.
    for sign in (-1,1):
        tube(m,[(sign*.24,.36,-.028),(sign*.205,.30,-.10),(sign*.13,.205,-.16),
                (sign*.075,.145,-.185)],.022,4,8,flat=True)
        # Three hemp lashings clamp each thong to the fork tip.
        for dy in (-.035,0,.035):
            tube(m,[(sign*.24+.053*math.cos(i*math.tau/16),.345+dy,.054*math.sin(i*math.tau/16)) for i in range(17)],.007,5,6)
    # Curved leather pouch with a shallow concave face and bevelled outline.
    pouch=base.Model('Pouch')
    beveled_profile(pouch,[(-.095,-.032),(-.075,-.052),(.075,-.052),(.095,-.032),
                          (.095,.032),(.075,.052),(-.075,.052),(-.095,.032)],.025,3,4,.008)
    off=len(m.vertices)
    m.vertices +=[(x,y+.145,z-.185+.6*x*x) for x,y,z in pouch.vertices]
    m.faces +=[(a+off,b+off,c+off,k) for a,b,c,k in pouch.faces]
    # Birch marks on exposed fork arms, front and reverse.
    support=base.Model('Support');support.vertices=list(m.vertices);support.faces=list(m.faces)
    for sign in (-1,1):
        for x,y in [(sign*.14,.19),(sign*.20,.30)]:
            for z in (-.052,.052):
                fin(m,[(x-.021,y-.004,z),(x+.018,y-.006,z),(x+.017,y+.004,z),(x-.018,y+.005,z)],2,.003 if z>0 else -.003)
    # Grip seam and visible stitches are seated in the beaver-hide sleeve.
    stroke(m,[kit.front_surface(support,.026,y,.002) for y in [-.33,-.24,-.14]],4,.003)
    for y in [-.32,-.275,-.23,-.185,-.14]:
        stroke(m,[kit.front_surface(support,x,yy,.003) for x,yy in [(.015,y-.006),(.038,y+.006)]],5,.0025)
    if elite:
        # Fine rabbit-pelt wrap occupies the lower grip; no extra current-tier cost.
        collar=base.Model('RabbitWrap')
        kit.loft(collar,[(-.36,0,0),(-.36,.086,.076),(-.33,.092,.080),(-.18,.088,.077),
                         (-.15,.078,.070),(-.15,0,0)],7,20)
        off=len(m.vertices);m.vertices+=collar.vertices;m.faces +=[(a+off,b+off,c+off,k) for a,b,c,k in collar.faces]
        for i in range(9):
            y=-.32+i*.018
            # Restrained pale pelt nap on the outward face, not dangling fur balls.
            stroke(m,[(-.044,y,-.067),(-.018,y+.012,-.080),(.015,y+.009,-.080)],9,.002)
        for y in (-.30,-.25,-.20):stroke(m,[(.034,y,-.074),(.055,y+.009,-.065)],8,.003)
    return m


SPECS=[('NormalBirchSlingshot','Normal birch slingshot',lambda:slingshot(False),COLORS,TYPES),
       ('EliteBirchSlingshot','Elite birch slingshot',lambda:slingshot(True),COLORS,TYPES)]
RECIPES={'NormalBirchSlingshot':('T2-H1',7,'2 Birch branches + 1 Raw beaver pelt + 1 Hemp cord -> 1'),
         'EliteBirchSlingshot':('T2-H6',8,'1 Normal birch slingshot + 2 Fine rabbit pelts -> 1')}

if __name__=='__main__':generate(11,SPECS,RECIPES,category='Tools',role='Tool',preview='--preview' in sys.argv)
