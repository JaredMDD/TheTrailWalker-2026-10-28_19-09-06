"""Original Dawn Plains scenery meshes; deterministic, Python standard library only."""
from pathlib import Path
import math,json,random
import generate_region01_meshes as b
from generate_region05_meshes import sphere

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Art/Environment/Region01'
COLORS=[(76,50,31),(113,75,44),(155,109,65),(194,151,96),
 (93,116,58),(108,129,63),(122,140,72),(172,165,99),
 (62,87,43),(87,116,49),(124,143,66),(66,83,48),
 (39,69,49),(54,90,58),(79,115,65),(215,184,123),
 (104,109,93),(134,139,116),(166,167,139),(126,103,66),
 (151,123,79),(178,146,96),(221,213,170),(226,175,69)]
STATS=[]

def save(m):
    m.save();STATS.append(dict(Name=m.name,Triangles=len(m.faces),BoundsMin=[min(p[i] for p in m.vertices) for i in range(3)],BoundsMax=[max(p[i] for p in m.vertices) for i in range(3)]))

def bush(name,tall=False,dry=False):
    m=b.Model(name)
    for i,(x,y,z,r) in enumerate([(-.4,.29,0,.38),(.1,.35,.13,.48),(.43,.23,-.12,.32),(-.1,.26,-.27,.36)]):
        h=1.4 if tall else 1
        m.cylinder((x*.6,0,z*.6),(x,y*h,z),.045,.018,5,1,2)
        sphere(m,(x,y*h+.1,z),(r,(r*.8)*(1.45 if tall else 1),r*.8),7 if dry else 8+i%3,8,4)
    save(m)

def pine(name,height,lean=0,young=False):
    m=b.Model(name)
    m.cylinder((0,0,0),(lean*.5,height*.52,0),.14 if young else .23,.07,8,1,3)
    for i in range(4):
        a=i*math.tau/4;m.cylinder((0,.12,0),(.48*math.cos(a),.025,.48*math.sin(a)),.09,.025,6,1,2)
    rng=random.Random(19+int(height*10));n=9
    for j in range(4):
        y=height*(.25+j*.15);radius=height*(.22-j*.035);cx=lean*(y/height)
        tip=m.v((lean*(y/height+.3),min(height,y+height*.35),0));bottom=m.v((cx,y+.025,0));ring=[]
        for i in range(n):
            a=i*math.tau/n+j*.31;r=radius*(.9+rng.random()*.16)
            ring.append(m.v((cx+r*math.cos(a),y+(.07 if i%2 else 0),r*math.sin(a))))
        for i in range(n):
            k=(i+1)%n;m.tri(tip,ring[k],ring[i],12+(i+j)%3);m.tri(bottom,ring[i],ring[k],12)
        if j<2:m.cylinder((cx,y-.1,0),(cx+radius*.7,y+.1,.1),.06,.02,5,1,2)
    save(m)

def grass(name,flowers=False):
    m=b.Model(name);rng=random.Random(44)
    for i in range(11 if not flowers else 7):
        x=rng.uniform(-.32,.32);z=rng.uniform(-.24,.24);h=rng.uniform(.23,.52);lean=rng.uniform(-.15,.15)
        a=m.v((x-.025,0,z));c=m.v((x+.025,0,z));d=m.v((x+lean,h,z+.04))
        m.tri(a,c,d,5+i%3);m.tri(d,c,a,5+i%3)
        if flowers and i%2==0:
            for j in range(5):
                angle=j*math.tau/5;sphere(m,(x+lean+.045*math.cos(angle),h+.01,z+.04+.045*math.sin(angle)),(.042,.015,.025),22,5,3)
            sphere(m,(x+lean,h+.025,z+.04),(.025,.015,.025),23,6,3)
    save(m)

def mound(name,rx,rz,height,rock=False):
    m=b.Model(name);rng=random.Random(77);n=10;rings=[]
    for radius,y in [(1,0),(.65,height*.55),(.28,height*.95)]:
        rings.append([m.v((rx*radius*math.cos(i*math.tau/n),y+(rng.random()-.5)*height*.12,rz*radius*math.sin(i*math.tau/n))) for i in range(n)])
    for j in range(2):
        for i in range(n):
            k=(i+1)%n;color=(16+i%3) if rock else (4+(i+j)%3)
            m.tri(rings[j][i],rings[j+1][k],rings[j][k],color);m.tri(rings[j][i],rings[j+1][i],rings[j+1][k],color)
    top=m.v((0,height,0))
    for i in range(n):m.tri(top,rings[-1][(i+1)%n],rings[-1][i],18 if rock else 6)
    save(m)

def ground(name,shoulder=False):
    m=b.Model(name);n=6
    v=[[m.v(((x-n/2),.025*math.sin(x*2+z),z-n/2)) for x in range(n+1)] for z in range(n+1)]
    for z in range(n):
        for x in range(n):
            c=(20+(z+x)%2 if x<2 else 7 if x==2 else 5+(z+x)%2) if shoulder else (4 if math.sin(x*.9)+math.cos(z*.8)<-.3 else 6 if math.sin(x*.9)+math.cos(z*.8)>.8 else 5)
            m.tri(v[z][x],v[z+1][x],v[z][x+1],c);m.tri(v[z][x+1],v[z+1][x],v[z+1][x+1],c)
    save(m)

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True);b.OUT=OUT;b.COLORS=COLORS
    bush('MeadowBushLow');bush('MeadowBushTall',True);bush('DryShrub',dry=True)
    pine('MeadowPine',5.8);pine('YoungPine',3.2,young=True);pine('WindPine',4.8,.65)
    grass('GrassTuft');grass('WildflowerClump',True)
    mound('GroundMound',4,2.8,.55);mound('MeadowBoulder',.95,.65,.9,True)
    ground('MeadowGroundPatch');ground('TrailShoulder',True);b.palette('DawnPlainsPalette.png')
    (OUT/'EnvironmentManifest.json').write_text(json.dumps(dict(Region=1,Models=STATS,Policy='Decorative scenery only. No colliders, item definitions, harvest markers, rewards or resource spawn changes.'),indent=2)+'\n',encoding='utf-8')
