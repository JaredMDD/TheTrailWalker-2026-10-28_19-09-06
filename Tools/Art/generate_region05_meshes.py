"""Original Cloudstep Mountains resource meshes; Python standard library only."""
from pathlib import Path
import math
import generate_region01_meshes as base

OUT=Path(__file__).resolve().parents[2]/'Assets/_Game/Art/Items/Region05'
COLORS=[
 (123,94,47),(166,131,65),(205,173,97),(239,213,145),
 (103,111,113),(130,140,143),(160,165,157),(117,115,105),
 (141,32,47),(190,43,58),(65,91,44),(93,126,54),
 (220,63,65),(239,91,77),(121,147,68),(70,62,39),
]

def sphere(m,center,radii,color,n=8,rings=4):
    layers=[]
    for j in range(1,rings):
        phi=j*math.pi/rings
        layers.append([m.v((center[0]+radii[0]*math.sin(phi)*math.cos(i*2*math.pi/n),center[1]+radii[1]*math.cos(phi),center[2]+radii[2]*math.sin(phi)*math.sin(i*2*math.pi/n))) for i in range(n)])
    top=m.v((center[0],center[1]+radii[1],center[2]));bottom=m.v((center[0],center[1]-radii[1],center[2]))
    for i in range(n):
        k=(i+1)%n;m.tri(top,layers[0][k],layers[0][i],color);m.tri(bottom,layers[-1][i],layers[-1][k],color)
        for j in range(len(layers)-1):
            m.tri(layers[j][i],layers[j][k],layers[j+1][k],color)
            m.tri(layers[j][i],layers[j+1][k],layers[j+1][i],color)

def leaf(m,a,b,width):
    d=base.sub(b,a);length=math.hypot(d[0],d[1]);side=(-d[1]/length*width,d[0]/length*width,0)
    mid=base.add(a,base.mul(d,.53));points=[a,base.add(mid,side),b,base.sub(mid,side),base.add(mid,(0,0,-.025))]
    v=[m.v(p) for p in points]
    for i in range(4):
        j=(i+1)%4;m.tri(v[i],v[4],v[j],11+i%2*3);m.tri(v[j],v[4],v[i],10)

def save(m,largest):
    m.save();dx=max(v[0] for v in m.vertices)-min(v[0] for v in m.vertices);dy=max(v[1] for v in m.vertices)-min(v[1] for v in m.vertices)
    print('  front footprint:',*[max(300,round(largest*v/max(dx,dy))) for v in (dx,dy)])

def oats():
    m=base.Model('Oats')
    m.cylinder((-.045,-.45,.01),(0,.43,.01),.017,.009,sides=4,bark=1,end=2)
    for i,(x,y,z) in enumerate([(-.21,.28,0),(.23,.23,.02),(-.28,.05,-.015),(.29,.005,0),(-.18,-.17,.01),(.18,-.20,-.015)]):
        start=(0,y+.08,.01);shoulder=(x*.8,y+.08,z);tip=(x,y-.005,z)
        m.cylinder(start,shoulder,.009,.007,sides=3,bark=1,end=2)
        m.cylinder(shoulder,tip,.007,.004,sides=3,bark=1,end=2)
        sphere(m,(x,y-.08,z),(.053,.105,.035),2+i%2,6,4)
        # An oat spikelet hangs down; keep its short awn with the same unprocessed grain.
        m.cylinder((x,y-.17,z),(x+.025,y-.22,z),.004,.001,sides=3,bark=2,end=2)
    leaf(m,(-.02,-.20,.01),(-.23,-.12,.005),.026)
    save(m,900)

def pebbles():
    m=base.Model('Pebbles')
    sphere(m,(-.13,-.015,0),(.27,.18,.22),5,9,4)
    sphere(m,(.22,-.065,.015),(.18,.13,.16),6,9,4)
    sphere(m,(-.02,-.09,-.22),(.15,.105,.13),7,8,4)
    save(m,700)

def berries():
    m=base.Model('RedBerries')
    m.cylinder((-.24,.28,.025),(.21,.37,.025),.014,.009,sides=4,bark=11,end=11)
    for i,(x,y,z) in enumerate([(-.20,.11,0),(.04,.14,.025),(.23,.06,0),(-.10,-.12,-.01),(.12,-.15,-.02)]):
        m.cylinder((x,.29,.025),(x,y+.13,z),.006,.004,sides=3,bark=11,end=11)
        sphere(m,(x,y,z),(.125,.125,.11),[9,12,8,12,9][i],14,8)
    leaf(m,(-.02,.32,.025),(.17,.51,.025),.075)
    m.fit_bounds((-.317462,-.275,-.128329),(.355,.51,.133329))
    save(m,800)

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True);base.OUT=OUT;base.COLORS=COLORS
    oats();pebbles();berries();base.palette('Region05Palette.png')
