"""Original Fern Hollow resource meshes; Python 3 standard library only."""
from pathlib import Path
import math
import random
import generate_region01_meshes as base

OUT=Path(__file__).resolve().parents[2]/'Assets/_Game/Art/Items/Region04'
COLORS=[
    (218,211,184),(239,232,207),(191,185,161),(167,165,145),
    (173,37,43),(207,48,48),(230,68,54),(246,94,67),
    (242,204,115),(212,169,84),(58,85,39),(78,111,47),
    (106,139,61),(137,162,82),(116,100,54),(166,178,108),
]

def save(m,largest):
    m.save()
    dx=max(p[0] for p in m.vertices)-min(p[0] for p in m.vertices)
    dy=max(p[1] for p in m.vertices)-min(p[1] for p in m.vertices)
    print('  front footprint:',round(largest*dx/max(dx,dy)),round(largest*dy/max(dx,dy)))

def limestone():
    m=base.Model('Limestone');rng=random.Random(401);n=9;rings=[]
    for radius,y in [(.27,-.25),(.44,-.12),(.46,-.035),(.45,.005),(.37,.22),(.23,.28)]:
        rings.append([m.v((radius*(1+.05*math.cos(i*3))*math.cos(i*2*math.pi/n),y+rng.uniform(-.008,.008),radius*.78*math.sin(i*2*math.pi/n))) for i in range(n)])
    top=m.v((-.05,.29,.01));bottom=m.v((0,-.25,0))
    for i in range(n):
        j=(i+1)%n;m.tri(bottom,rings[0][i],rings[0][j],2);m.tri(top,rings[-1][j],rings[-1][i],1)
        for k in range(len(rings)-1):
            c=3 if k==2 else 0 if k in (1,3) else 1 if k==4 else 2
            m.tri(rings[k][i],rings[k+1][i],rings[k+1][j],c)
            m.tri(rings[k][i],rings[k+1][j],rings[k][j],c if i%3 else min(c+1,3))
    save(m,1000)

def leaf(m,a,b,width,color):
    direction=base.sub(b,a);length=math.hypot(direction[0],direction[1])
    side=(-direction[1]/length*width,direction[0]/length*width,0)
    middle=base.add(a,base.mul(direction,.53))
    points=[a,base.add(middle,side),b,base.sub(middle,side),base.add(middle,(0,0,-.035))]
    v=[m.v(p) for p in points]
    for i in range(4):
        j=(i+1)%4;m.tri(v[i],v[4],v[j],color+i%2);m.tri(v[j],v[4],v[i],10)

def strawberry():
    m=base.Model('Strawberry');n=20;rings=[]
    profile=[(.045,-.38),(.20,-.23),(.32,-.07),(.37,.10),(.32,.25),(.14,.29)]
    for radius,y in profile:
        rings.append([m.v((radius*math.cos(i*2*math.pi/n),y,radius*math.sin(i*2*math.pi/n))) for i in range(n)])
    bottom=m.v((0,-.40,0));top=m.v((0,.27,0))
    for i in range(n):
        j=(i+1)%n;m.tri(bottom,rings[0][i],rings[0][j],4);m.tri(top,rings[-1][j],rings[-1][i],5)
        for k in range(len(rings)-1):
            c=4+(i+k)%3
            m.tri(rings[k][i],rings[k+1][i],rings[k+1][j],c)
            m.tri(rings[k][i],rings[k+1][j],rings[k][j],c)
    # Small pale seed diamonds sit on the surface; no bitmap fruit texture needed.
    for row,(y,radius) in enumerate([(-.22,.209),(-.04,.331),(.16,.357)]):
        for i in range(8):
            t=(i+.5*(row%2))*2*math.pi/8
            radial=(math.cos(t),0,math.sin(t));tangent=(-math.sin(t),0,math.cos(t))
            center=base.mul(radial,radius);center=(center[0],y,center[2]);points=[]
            for along,up in [(0,-.022),(.011,0),(0,.022),(-.011,0)]:
                p=base.add(center,base.mul(tangent,along));points.append(m.v((p[0],p[1]+up,p[2])))
            m.tri(points[0],points[1],points[2],8);m.tri(points[0],points[2],points[3],9)
            # Both sides keep the paper-thin seed visible from any camera yaw.
            m.tri(points[0],points[2],points[1],8);m.tri(points[0],points[3],points[2],9)
    for i in range(5):
        t=i*2*math.pi/5
        leaf(m,(0,.29,0),(.22*math.cos(t),.27,.22*math.sin(t)),.048,11)
    m.cylinder((0,.28,0),(.018,.40,.01),.018,.012,sides=4,bark=11,end=12)
    m.fit_bounds((-.37,-.4,-.37),(.37,.40178,.37))
    save(m,800)

def herbs():
    m=base.Model('TeaHerbs')
    m.cylinder((-.43,-.075,.015),(.40,.09,.015),.018,.009,sides=5,bark=11,end=12)
    leaf(m,(-.25,-.04,.015),(-.44,.27,.005),.10,11)
    leaf(m,(-.13,-.015,.015),(-.09,-.31,.015),.12,12)
    leaf(m,(.06,.025,.015),(.20,.36,-.015),.12,11)
    leaf(m,(.23,.06,.015),(.44,-.17,.025),.10,12)
    leaf(m,(.34,.08,.015),(.52,.22,.015),.075,11)
    save(m,900)

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True);base.OUT=OUT;base.COLORS=COLORS
    limestone();strawberry();herbs();base.palette('Region04Palette.png')
