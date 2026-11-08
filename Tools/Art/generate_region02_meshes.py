"""Original Region 2 low-poly props; Python 3, no third-party dependencies.

Shares Region 1's OBJ/palette writer; never regenerates or replaces its assets.
"""
from pathlib import Path
import math
import random
import generate_region01_meshes as base

OUT = Path(__file__).resolve().parents[2] / 'Assets/_Game/Art/Items/Region02'
COLORS = [
    (69, 77, 83), (88, 99, 107), (111, 122, 130), (148, 157, 160),
    (179, 182, 176), (129, 120, 106), (78, 85, 69), (114, 126, 74),
    (154, 158, 95), (186, 176, 112), (214, 197, 141), (230, 214, 165),
    (85, 51, 30), (121, 76, 42), (157, 104, 58), (191, 139, 83),
]

def hemp():
    rng=random.Random(202);m=base.Model('HempFiber')
    for i in range(10):
        z=(i%5-2)*.052;y=(i//5)*.06-.03
        a=(-.43+rng.uniform(-.06,.04),y,z)
        mid=(rng.uniform(-.04,.05),y+.035,z*.65)
        b=(.41+rng.uniform(-.06,.06),y+.035+rng.uniform(-.025,.025),z+.025)
        color=[8,9,10,11][i%4]
        m.cylinder(a,mid,.015,.018,sides=4,bark=color,end=color)
        m.cylinder(mid,b,.018,.009,sides=4,bark=color,end=color)
        if i%2==0:
            tip=(b[0]+.06,b[1]+.05,b[2]+.025)
            m.cylinder(b,tip,.009,.002,sides=3,bark=color,end=color)
    m.save()

def flint():
    rng=random.Random(203);m=base.Model('Flint');n=7;rings=[]
    for k,(radius,y) in enumerate([(.24,-.22),(.46,-.02),(.32,.20)]):
        ring=[]
        for i in range(n):
            angle=(i+.18*k)*2*math.pi/n;r=radius*rng.uniform(.85,1.08)
            ring.append(m.v((r*math.cos(angle),y+rng.uniform(-.035,.035),r*.8*math.sin(angle))))
        rings.append(ring)
    top=m.v((-.06,.30,.02));bottom=m.v((.02,-.27,0))
    for i in range(n):
        j=(i+1)%n
        m.tri(bottom,rings[0][i],rings[0][j],0)
        m.tri(top,rings[-1][j],rings[-1][i],3 if i%3 else 4)
        for k in range(2):
            c=[0,1,2,3][(i+k)%4]
            m.tri(rings[k][i],rings[k+1][i],rings[k+1][j],c)
            m.tri(rings[k][i],rings[k+1][j],rings[k][j],min(c+1,4))
    m.save()

def pinecone():
    m=base.Model('PineCone')
    # Core and small stem are visible through gaps between modeled overlapping scales.
    m.cylinder((0,-.40,0),(0,.40,0),.12,.025,sides=7,bark=13,end=14)
    m.cylinder((0,-.48,0),(.015,-.34,0),.035,.04,sides=5,bark=13,end=14)
    for row,(y,radius,count) in enumerate([(-.31,.20,8),(-.16,.28,9),(0,.29,9),(.16,.24,8),(.30,.14,6)]):
        for i in range(count):
            t=(i/count+.055*(row%2))*2*math.pi
            radial=(math.cos(t),0,math.sin(t));tangent=(-math.sin(t),0,math.cos(t))
            def point(r,h,side):
                return m.v((radial[0]*r+tangent[0]*side,y+h,radial[2]*r+tangent[2]*side))
            width=radius*.43
            a=point(radius*.48,-.075,-width*.6)
            b=point(radius*.48,-.075,width*.6)
            c=point(radius+.065,.015,width)
            d=point(radius+.095,.055,0)
            e=point(radius+.065,.015,-width)
            ridge=point(radius*.88,.065,0)
            color=13+(i+row)%2
            m.tri(a,ridge,b,12)
            m.tri(b,ridge,c,color)
            m.tri(c,ridge,d,15)
            m.tri(d,ridge,e,14)
            m.tri(e,ridge,a,color)
            # Closed undersides prevent invisible scales when the player looks upward.
            m.tri(a,b,d,12);m.tri(b,c,d,13);m.tri(a,d,e,13)
    m.save()

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True)
    base.OUT=OUT;base.COLORS=COLORS
    hemp();flint();pinecone();base.palette('Region02Palette.png')
