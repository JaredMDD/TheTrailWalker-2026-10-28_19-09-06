"""Original Mistwood resource meshes; Python 3 standard library only."""
from pathlib import Path
import math
import random
import generate_region01_meshes as base

OUT=Path(__file__).resolve().parents[2]/'Assets/_Game/Art/Items/Region03'
COLORS=[
    (78,88,48),(107,119,67),(137,145,84),(172,168,109),
    (180,111,77),(157,87,59),(198,131,94),(215,151,111),
    (241,239,225),(218,224,223),(191,204,207),(255,252,237),
    (128,137,119),(147,160,141),(114,80,56),(173,143,108),
]

def save(m,largest):
    m.save()
    dx=max(v[0] for v in m.vertices)-min(v[0] for v in m.vertices)
    dy=max(v[1] for v in m.vertices)-min(v[1] for v in m.vertices)
    print('  front footprint:',round(largest*dx/max(dx,dy)),round(largest*dy/max(dx,dy)))

def nettle():
    m=base.Model('NettleFiber');rng=random.Random(301)
    for i in range(9):
        z=(i%3-1)*.07;y=(i//3-1)*.035
        start=(-.44+rng.uniform(-.07,.03),y,z)
        middle=(-.04+rng.uniform(-.03,.03),y+.04,z*.7)
        shoulder=(.27,y+.065,z-.015)
        end=(.44+rng.uniform(-.05,.06),y+.10+rng.uniform(-.03,.06),z+.045)
        c=1+i%3
        m.cylinder(start,middle,.019,.022,sides=4,bark=c,end=c)
        m.cylinder(middle,shoulder,.022,.016,sides=4,bark=c,end=c)
        m.cylinder(shoulder,end,.016,.004,sides=3,bark=c,end=c)
        if i%3==0:
            m.cylinder(start,(-.53,y+.085,z-.025),.01,.002,sides=3,bark=c,end=c)
    save(m,1000)

def clay():
    m=base.Model('Clay');rng=random.Random(302);n=10;rings=[]
    # Soft rounded lump with broad facets and a shallow top depression, not a fired pot or brick.
    for radius,y in [(.30,-.24),(.45,-.08),(.39,.16),(.23,.27)]:
        ring=[]
        for i in range(n):
            t=(i+.04)*2*math.pi/n;r=radius*rng.uniform(.92,1.06)
            ring.append(m.v((r*math.cos(t),y+rng.uniform(-.018,.018),r*.8*math.sin(t))))
        rings.append(ring)
    top=m.v((-.04,.235,.02));bottom=m.v((0,-.24,0))
    for i in range(n):
        j=(i+1)%n
        m.tri(bottom,rings[0][i],rings[0][j],5)
        m.tri(top,rings[-1][j],rings[-1][i],6 if i%3 else 7)
        for k in range(len(rings)-1):
            c=4 if k==1 else 5 if k==0 else 6
            m.tri(rings[k][i],rings[k+1][i],rings[k+1][j],c)
            m.tri(rings[k][i],rings[k+1][j],rings[k][j],c if i%3 else min(c+1,7))
    save(m,900)

def salt():
    m=base.Model('RockSalt')
    def block(center,size,angle):
        co,si=math.cos(angle),math.sin(angle);v=[]
        for x,y,z in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]:
            xx=x*size[0]/2;zz=z*size[2]/2
            v.append(m.v((center[0]+xx*co-zz*si,center[1]+y*size[1]/2,center[2]+xx*si+zz*co)))
        for a,b,c,d,color in [(0,3,2,1,9),(4,5,6,7,10),(0,4,7,3,9),(1,2,6,5,8),(3,7,6,2,11),(0,1,5,4,10)]:
            m.tri(v[a],v[b],v[c],color);m.tri(v[a],v[c],v[d],color)
    block((-.08,-.07,.01),(.43,.38,.40),-.15)
    block((.15,.15,.06),(.31,.40,.30),.20)
    block((.32,-.13,-.05),(.23,.24,.25),.35)
    block((-.30,-.17,.02),(.22,.22,.24),-.28)
    save(m,800)

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True);base.OUT=OUT;base.COLORS=COLORS
    nettle();clay();salt();base.palette('Region03Palette.png')
