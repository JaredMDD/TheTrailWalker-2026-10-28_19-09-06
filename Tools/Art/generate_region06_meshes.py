"""Original Reedwater Delta loose resource meshes; standard library only."""
from pathlib import Path
import math
import generate_region01_meshes as base
from generate_region05_meshes import sphere

OUT=Path(__file__).resolve().parents[2]/'Assets/_Game/Art/Items/Region06'
COLORS=[
 (49,77,38),(83,112,56),(128,151,76),(180,193,125),
 (140,105,47),(189,147,67),(223,187,103),(247,220,147),
 (207,181,126),(230,209,159),(177,150,104),(245,228,185),
 (69,99,46),(155,164,95),(157,119,54),(145,125,88),
]

def leaf(m,a,b,width,color):
    d=base.sub(b,a);length=math.hypot(d[0],d[1]);side=(-d[1]/length*width,d[0]/length*width,0)
    mid=base.add(a,base.mul(d,.53));points=[a,base.add(mid,side),b,base.sub(mid,side),base.add(mid,(0,0,-.018))]
    v=[m.v(p) for p in points]
    for i in range(4):
        j=(i+1)%4;m.tri(v[i],v[4],v[j],color+i%2);m.tri(v[j],v[4],v[i],color-1)

def save(m,largest):
    m.save();dx=max(v[0] for v in m.vertices)-min(v[0] for v in m.vertices);dy=max(v[1] for v in m.vertices)-min(v[1] for v in m.vertices)
    print('  front footprint:',*[max(300,round(largest*v/max(dx,dy))) for v in (dx,dy)])

def reeds():
    m=base.Model('Reeds')
    for i,(x,height,z) in enumerate([(-.13,.86,.015),(0,1.01,0),(.15,.77,.04)]):
        a=(x,-.46,z);joint=(x+.025,.02,z);tip=(x+.09,height-.46,z)
        m.cylinder(a,joint,.022,.02,sides=6,bark=1,end=3)
        m.cylinder(joint,tip,.02,.014,sides=6,bark=1,end=3)
        direction=-1 if i%2==0 else 1
        leaf(m,(x+.02,-.02,z),(x+direction*.28,.27,z),.035,1)
        leaf(m,(x+.02,-.23,z),(x-direction*.24,.04,z+.015),.03,1)
    save(m,1000)

def wheat():
    m=base.Model('Wheat')
    m.cylinder((-.035,-.46,.01),(0,.43,.01),.013,.008,sides=4,bark=5,end=6)
    # Compact upward-pointing ear, unlike oat heads on wide drooping branches.
    for row in range(6):
        y=-.005+row*.064
        for side in [-1,1]:
            x=side*(.049 if row<4 else .036)
            sphere(m,(x,y,.005),(.048 if row<4 else .038,.072,.032),6 if (row+side)%2 else 7,6,3)
            m.cylinder((x,y+.035,.005),(x+side*.055,y+.13,.005),.0035,.0008,sides=3,bark=6,end=7)
    leaf(m,(-.015,-.27,.01),(-.25,-.055,.01),.035,5)
    leaf(m,(-.012,-.18,.01),(.20,-.005,.01),.03,5)
    save(m,900)

def sand():
    m=base.Model('Sand');n=14;rings=[]
    for radius,y in [(.48,-.10),(.37,.025),(.17,.14)]:
        ring=[]
        for i in range(n):
            t=i*2*math.pi/n;r=radius*(1+.06*math.cos(i*2))
            ring.append(m.v((r*math.cos(t),y,r*.77*math.sin(t))))
        rings.append(ring)
    top=m.v((-.04,.155,.015));bottom=m.v((0,-.10,0))
    for i in range(n):
        j=(i+1)%n;m.tri(bottom,rings[0][i],rings[0][j],10);m.tri(top,rings[-1][j],rings[-1][i],9 if i%3 else 11)
        for k in range(2):
            c=8 if k==0 else 9
            m.tri(rings[k][i],rings[k+1][i],rings[k+1][j],c)
            m.tri(rings[k][i],rings[k+1][j],rings[k][j],c if i%3 else 10)
    # A few low raised grains suggest granular sand without particle simulation.
    for x,y,z in [(-.16,.11,-.02),(.12,.11,.04),(.02,.15,.03),(.25,.065,-.04),(-.29,.035,.025)]:
        a=m.v((x-.012,y,z-.01));b=m.v((x+.012,y,z-.01));c=m.v((x,y,z+.013));d=m.v((x,y+.017,z))
        m.tri(a,d,b,11);m.tri(b,d,c,9);m.tri(c,d,a,8)
    save(m,900)

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True);base.OUT=OUT;base.COLORS=COLORS
    reeds();wheat();sand();base.palette('Region06Palette.png')
