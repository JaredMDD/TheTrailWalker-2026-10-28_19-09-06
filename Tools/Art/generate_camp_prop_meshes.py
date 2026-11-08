"""Static burned-wood campfire and four regional log seats. Python stdlib only."""
from pathlib import Path
import math,json,random
import generate_region01_meshes as b

ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'Assets/_Game/Art/Environment/CampProps'
COLORS=[(80,51,30),(121,77,41),(155,105,56),(199,156,91),(223,188,123),(145,100,53),
 (190,186,162),(224,220,194),(66,64,55),(199,170,115),(231,210,159),(157,132,90),
 (68,37,26),(102,55,34),(141,78,43),(169,105,55),(207,145,81),(120,68,34),
 (67,56,42),(101,84,59),(136,115,78),(167,125,69),(204,163,96),(121,86,44),
 (28,27,25),(49,47,42),(94,91,81),(75,49,31),(121,80,43),(174,129,74),(187,154,108),(18,18,17)]
STATS=[]
def save(m):
    m.save();STATS.append(dict(Name=m.name,Triangles=len(m.faces),BoundsMin=[min(v[i] for v in m.vertices) for i in range(3)],BoundsMax=[max(v[i] for v in m.vertices) for i in range(3)]))

def quad(m,p,color):
    v=[m.v(x) for x in p];m.tri(v[0],v[1],v[2],color);m.tri(v[0],v[2],v[3],color)

def seat(name,tier):
    m=b.Model(name);rng=random.Random(80+tier)
    cross=[(.43,-.10),(.43,.10),(.38,.19),(.27,.235),(.13,.21),(.035,.13),(0,0),(.035,-.13),(.13,-.21),(.27,-.235),(.38,-.19)]
    length=[1.4,1.34,1.38,1.5][tier-1];offset=[0,6,12,18][tier-1];n=len(cross);rings=[];points=[]
    for j,x in enumerate([-length/2,0,length/2]):
        section=[]
        for y,z in cross:
            width=([1,1.03,.98][j] if tier==1 else [1,.96,1][j] if tier==2 else [1.06,.97,1.02][j] if tier==3 else [1.1,1.07,1.12][j])
            # Log bends remain below a common flat sitting plane; no obstruction at the seat anchor.
            zz=z*width+(.024*math.sin(j*2) if tier==3 else 0)
            section.append((x,y,zz))
        points.append(section);rings.append([m.v(p) for p in section])
    for j in range(2):
        for i in range(n):
            k=(i+1)%n;color=offset+3 if i==0 else (6+(i%3!=0) if tier==2 else offset+1+(i%3==0))
            m.tri(rings[j][i],rings[j][k],rings[j+1][k],color);m.tri(rings[j][i],rings[j+1][k],rings[j+1][i],color)
            if i==0:continue
            a,c,d,e=points[j][i],points[j][k],points[j+1][k],points[j+1][i]
            normal=b.unit(b.cross(b.sub(c,a),b.sub(e,a)))
            for mark in range(2 if tier!=4 else 3):
                u=rng.uniform(.15,.75);v=rng.uniform(.2,.75);du=rng.uniform(.12,.25);dv=.027 if tier==2 else .045
                def surface(s,t):return b.add(b.add(b.mul(b.add(b.mul(a,1-t),b.mul(c,t)),1-s),b.mul(b.add(b.mul(e,1-t),b.mul(d,t)),s)),b.mul(normal,.0015))
                quad(m,[surface(u,v),surface(u,v+dv),surface(u+du,v+dv),surface(u+du,v)],offset+2 if tier==2 else offset)
    for side in (0,2):
        # Shared annular topology gives growth rings without coplanar overlay flicker.
        allrings=[];scales=[1,.94,.72,.695,.45,.425,.19,.165]
        for scale in scales:
            allrings.append([m.v((p[0],.215+(p[1]-.215)*scale,p[2]*scale)) for p in points[side]])
        for j in range(len(allrings)-1):
            color=offset+1 if j==0 else offset+5 if j%2==0 else offset+3+(tier%2)
            for i in range(n):
                k=(i+1)%n;f=(allrings[j][i],allrings[j][k],allrings[j+1][k],allrings[j+1][i])
                if side==0:f=tuple(reversed(f))
                m.tri(f[0],f[1],f[2],color);m.tri(f[0],f[2],f[3],color)
        center=m.v((points[side][0][0],.215,0))
        for i in range(n):
            a,c=allrings[-1][i],allrings[-1][(i+1)%n]
            m.tri(center,c,a,offset+3) if side==0 else m.tri(center,a,c,offset+3)
    # A small sawn branch knot, away from the sitting surface, distinguishes species silhouettes.
    if tier!=2:
        x=-.23 if tier!=3 else .26
        m.cylinder((x,.25,.20),(x+.025,.27,.29),.055,.045,7,offset+1,offset+3)
    save(m)

def burned_log(m,a,c,radius,seed,mostly_charred):
    rng=random.Random(seed);axis=b.unit(b.sub(c,a));u=b.unit(b.cross(axis,(0,1,0)));v=b.cross(axis,u);rings=[];n=7
    for j,t in enumerate([0,.2,.53,.8,1]):
        center=b.add(a,b.mul(b.sub(c,a),t));ring=[]
        for i in range(n):
            angle=i*math.tau/n;rad=radius*(.82+rng.random()*.25)
            p=b.add(center,b.add(b.mul(u,rad*math.cos(angle)),b.mul(v,rad*math.sin(angle))))
            if j in (0,4):p=b.add(p,b.mul(axis,rng.uniform(-.035,.035)))
            ring.append(m.v(p))
        rings.append(ring)
    for j in range(4):
        for i in range(n):
            k=(i+1)%n
            charred=mostly_charred or j in (1,2)
            col=(24+(i+j)%3 if charred else 27+(i%3))
            m.tri(rings[j][i],rings[j][k],rings[j+1][k],col);m.tri(rings[j][i],rings[j+1][k],rings[j+1][i],col)
    for end in (0,4):
        center=m.v(a if end==0 else c)
        for i in range(n):
            k=(i+1)%n;color=25 if mostly_charred or end==4 else 30
            m.tri(center,rings[end][k],rings[end][i],color) if end==0 else m.tri(center,rings[end][i],rings[end][k],color)

def campfire():
    m=b.Model('BurnedWoodCampfire')
    for i,(a,c,r) in enumerate([
        ((-.54,.095,-.15),(.48,.10,.20),.085),
        ((-.40,.09,.36),(.39,.11,-.35),.09),
        ((-.52,.12,.12),(.40,.25,-.12),.075),
        ((-.25,.10,-.47),(.10,.24,.36),.08),
        ((-.32,.28,-.15),(.33,.22,.21),.075),
        ((.21,.08,-.41),(.42,.12,.32),.055),
        ((-.31,.08,.30),(.17,.075,.40),.045)]):burned_log(m,a,c,r,30+i,i in (1,4,6))
    # Short splintered remnants are still wood, never flames, glowing embers, stones or particles.
    burned_log(m,(-.48,.045,-.30),(-.23,.06,-.39),.025,80,True)
    burned_log(m,(.31,.045,.35),(.53,.04,.22),.023,81,True)
    lowest=min(p[1] for p in m.vertices);m.vertices=[(x,y-lowest,z) for x,y,z in m.vertices]
    save(m)

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True);b.OUT=OUT;b.COLORS=COLORS
    campfire()
    for tier,wood in enumerate(['Pine','Birch','Mesquite','Oak'],1):seat(wood+'LogSeat',tier)
    b.palette('CampPropsPalette.png')
    (OUT/'MeshManifest.json').write_text(json.dumps(dict(Models=STATS,SeatSurfaceHeight=.43,Policy='Static decorative art only. No flames, glow, smoke, particles, animation, colliders, resource drops, selection logic or live camp replacement.'),indent=2)+'\n',encoding='utf-8')
