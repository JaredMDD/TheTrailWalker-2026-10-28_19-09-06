"""Red pagoda balloon merchant: editable, palette-mapped mesh parts. Python stdlib only."""
from pathlib import Path
import json, math
import generate_region01_meshes as b

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Art/Environment/BalloonMerchant'
COLORS=[(112,29,25),(156,39,30),(191,53,35),(214,76,42),
        (91,31,26),(132,48,31),(175,85,39),(215,128,58),
        (126,81,34),(173,119,45),(217,164,65),(247,204,110),
        (230,205,150),(249,226,174),(199,167,116),(166,133,87),
        (63,35,27),(95,53,33),(133,79,42),(171,111,59),
        (90,60,32),(133,92,49),(186,144,82),(223,181,113),
        (239,155,54),(255,200,89),(255,227,145),(252,237,181),
        (54,37,29),(77,63,43),(109,89,56),(147,120,75)]
STATS=[]

def box(m,c,size,k):
    x,y,z=c;a,d,e=[v/2 for v in size]
    ids=[m.v((x+sx*a,y+sy*d,z+sz*e)) for sx,sy,sz in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
    for q in [(0,3,2,1),(4,5,6,7),(0,4,7,3),(1,2,6,5),(3,7,6,2),(0,1,5,4)]:
        a,c,d,e=[ids[i] for i in q];m.tri(a,c,d,k);m.tri(a,d,e,k)

def tube(m,points,r,k,sides=8):
    for a,c in zip(points,points[1:]):
        start=len(m.faces);m.cylinder(a,c,r,r,sides,k,k)
        m.faces[start:]=[(i,j,l,k) for i,j,l,_ in m.faces[start:]]

def loop(m,c,rx,ry,r,k,plane='xy'):
    points=[]
    for i in range(25):
        a=i*math.tau/24
        points.append((c[0]+rx*math.cos(a),c[1]+(ry*math.sin(a) if plane=='xy' else 0),c[2]+(ry*math.sin(a) if plane=='xz' else 0)))
    tube(m,points,r,k)

def save(m):
    assert all(all(math.isfinite(c) for c in v) for v in m.vertices)
    m.save()
    STATS.append({'Name':m.name,'Triangles':len(m.faces),'Min':[min(v[i] for v in m.vertices) for i in range(3)],'Max':[max(v[i] for v in m.vertices) for i in range(3)]})

def cabin():
    m=b.Model('Cabin')
    box(m,(0,.72,0),(1.94,.18,1.7),17)
    box(m,(0,.81,0),(1.88,.045,1.64),10)
    # Solid box wall panels, with one actual door recess at the front (-Z).
    for x in (-.78,.78):box(m,(x,1.56,0),(.105,1.50,1.42),1)
    box(m,(0,1.56,.69),(1.56,1.50,.09),1)
    for x in (-.565,.565):box(m,(x,1.56,-.69),(.43,1.50,.09),2)
    box(m,(0,2.22,-.69),(.70,.18,.09),1)
    for x in (-.80,.80):
        for z in (-.73,.73):
            box(m,(x,1.57,z),(.125,1.58,.125),0)
            for y in (.91,2.24):box(m,(x,y,z),(.165,.065,.165),10)
    for y in (.96,2.18,2.33):
        box(m,(0,y,.755),(1.68,.04,.025),9)
        for x in (-.845,.845):box(m,(x,y,0),(.025,.04,1.46),9)
    # Warm wood underside, plank seams, porch and a small suspended step.
    for x in [-.75+i*.25 for i in range(7)]:box(m,(x,.63,0),(.022,.045,1.58),16)
    box(m,(0,.77,-.95),(1.12,.10,.35),18)
    box(m,(0,.54,-1.05),(.72,.075,.25),17)
    for x in (-.28,.28):tube(m,[(x,.59,-1.05),(x,.75,-1.00)],.017,21)
    # Simple raised circular merchant medallion on each side, no unreadable text.
    for side in (-1,1):
        x=side*.845
        for y in (1.15,1.99):box(m,(x,y,0),(.035,.035,1.10),10)
        # Coin motif oriented into the side wall plane (YZ).
        points=[(x*1.015,1.57+.24*math.cos(i*math.tau/24),.24*math.sin(i*math.tau/24)) for i in range(25)]
        tube(m,points,.018,10)
        for z in (-.07,.07):box(m,(x*1.025,1.57,z),(.022,.14,.018),11)
        for y in (1.50,1.64):box(m,(x*1.025,y,0),(.022,.018,.14),11)
    save(m)
    m=b.Model('FrontDoor')
    box(m,(0,1.49,-.705),(.64,1.31,.065),16)
    for x in (-.30,.30):box(m,(x,1.50,-.76),(.055,1.36,.07),9)
    for y in (.84,2.16):box(m,(0,y,-.76),(.66,.055,.07),10)
    for x in [-.23+i*.092 for i in range(6)]:box(m,(x,1.31,-.752),(.080,.85,.028),1+(round(x*20)%2))
    box(m,(0,1.96,-.75),(.53,.27,.028),28)
    for x in (-.20,-.10,0,.10,.20):box(m,(x,1.96,-.78),(.018,.25,.025),10)
    for y in (1.89,2.01):box(m,(0,y,-.78),(.50,.018,.025),10)
    for y in (.95,1.73):box(m,(0,y,-.78),(.52,.035,.035),9)
    loop(m,(.19,1.40,-.81),.04,.055,.009,11)
    for y in (1.12,1.66):box(m,(-.24,y,-.80),(.095,.04,.015),8)
    save(m)

def roof():
    m=b.Model('PagodaRoof')
    def tier(y,rx,rz,height):
        ring_points=[];segments=8
        perimeter=[]
        for side in range(4):
            for i in range(segments):
                t=-1+2*i/segments
                perimeter.append([(t,-1),(1,t),(-t,1),(-1,-t)][side])
        for scale,lift in [(1,0),(.82,.015),(.52,height*.55),(.12,height)]:
            points=[]
            for u,v in perimeter:
                corner=(abs(u*v)**3)*.17*scale
                points.append((u*rx*scale,y+lift+corner,v*rz*scale))
            ring_points.append(points)
        rings=[[m.v(p) for p in row] for row in ring_points]
        for j in range(3):
            for i in range(len(perimeter)):
                k=(i+1)%len(perimeter);col=4+(i//2%3)
                m.tri(rings[j][i],rings[j+1][k],rings[j][k],col);m.tri(rings[j][i],rings[j+1][i],rings[j+1][k],col)
        crown=m.v((0,y+height+.015,0))
        for i in range(len(perimeter)):m.tri(crown,rings[-1][(i+1)%len(perimeter)],rings[-1][i],5)
        # Roof underside and gold curled eaves conceal the open underside of the tiled shell.
        box(m,(0,y-.025,0),(rx*1.72,.055,rz*1.72),0)
        tube(m,ring_points[0]+[ring_points[0][0]],.025,10)
        for i in range(0,len(perimeter),2):tube(m,[row[i] for row in ring_points],.009,3,6)
        for i in (0,8,16,24):tube(m,[row[i] for row in ring_points],.025,9)
    tier(2.40,1.23,1.06,.44)
    tier(2.90,.56,.48,.26)
    tube(m,[(0,3.15,0),(0,3.31,0)],.035,10)
    m.cylinder((0,3.28,0),(0,3.37,0),.085,.008,10,10,11)
    # Decorative bracket arms under the main eaves.
    for x in (-.76,.76):
        for z in (-.70,.70):
            tube(m,[(x,2.18,z),(x*1.2,2.32,z*1.2),(x*1.40,2.38,z*1.40)],.035,9)
    save(m)

def lanterns():
    m=b.Model('WarmLanterns')
    for x in (-.69,.69):
        tube(m,[(x,2.31,-.80),(x,2.31,-.98),(x,2.12,-.98)],.014,8)
        # Faceted paper body, dark end caps and eight fine ribs.
        rings=[]
        for y,r in [(1.79,.07),(1.84,.12),(1.96,.145),(2.07,.11),(2.11,.06)]:
            rings.append([m.v((x+r*math.cos(i*math.tau/12),y,-.98+r*math.sin(i*math.tau/12))) for i in range(12)])
        for j in range(4):
            for i in range(12):
                k=(i+1)%12;m.tri(rings[j][i],rings[j+1][k],rings[j][k],24+(i%3));m.tri(rings[j][i],rings[j+1][i],rings[j+1][k],24+(i%3))
        for y in (1.79,2.11):m.cylinder((x,y-.012,-.98),(x,y+.012,-.98),.073,.073,12,9,10)
        for i in range(0,12,2):tube(m,[m.vertices[row[i]-1] for row in rings],.005,9,6)
        tube(m,[(x,1.78,-.98),(x,1.68,-.98)],.011,2)
    save(m)

def balloon():
    m=b.Model('BalloonEnvelope');rows=[];count=24
    # No pole degeneracy: small closed end rings rather than zero-radius latitude rings.
    for j in range(17):
        a=math.pi*(.035+.93*j/16);y=5.0-1.50*math.cos(a);r=math.sin(a)
        rows.append([m.v((1.38*r*math.cos(i*math.tau/count),y,1.18*r*math.sin(i*math.tau/count))) for i in range(count)])
    for j in range(len(rows)-1):
        for i in range(count):
            k=(i+1)%count;col=(12 if (i//2)%4==0 else 1+(i//2)%3)
            m.tri(rows[j][i],rows[j+1][k],rows[j][k],col);m.tri(rows[j][i],rows[j+1][i],rows[j+1][k],col)
    for end in (0,len(rows)-1):
        c=m.v((0,m.vertices[rows[end][0]-1][1],0))
        for i in range(count):
            k=(i+1)%count;m.tri(c,rows[end][i],rows[end][k],0) if end==0 else m.tri(c,rows[end][k],rows[end][i],0)
    save(m)
    m=b.Model('BalloonHarness')
    for i in range(0,count,2):
        a=i*math.tau/count;points=[]
        for j in range(17):
            t=math.pi*(.035+.93*j/16);r=math.sin(t)
            points.append((1.39*r*math.cos(a),5-1.50*math.cos(t),1.19*r*math.sin(a)))
        tube(m,points,.009,14,6)
    loop(m,(0,3.58,0),.18,.15,.028,9,'xz')
    loop(m,(0,4.14,0),1.14,.976,.023,20,'xz')
    for side in (-1,1):
        for zside in (-1,1):
            anchor=(side*.92,.74,zside*.78);guide=(side*1.23,2.57,zside*1.06);upper=(side*.81,4.14,zside*.69)
            tube(m,[anchor,guide,upper],.022,21)
            loop(m,anchor,.05,.06,.012,9)
    save(m)

def ground():
    m=b.Model('GroundStake')
    # One pointed wooden stake, slightly driven into the ground.
    box(m,(2.0,.18,-1.37),(.105,.44,.105),18)
    box(m,(2.0,.40,-1.37),(.14,.035,.13),19)
    for y in (.19,.215,.24):loop(m,(2.0,y,-1.37),.068,.065,.012,21,'xz')
    save(m)
    # Unit-Y rope; Unity authoring positions/scales it between the fixed stake and cabin.
    m=b.Model('GroundTether');tube(m,[(0,0,0),(0,1,0)],.018,21,10);save(m)
    m=b.Model('CabinTieRing');loop(m,(.96,.78,-.76),.065,.06,.018,9);save(m)

def main():
    OUT.mkdir(parents=True,exist_ok=True);b.OUT=OUT;b.COLORS=COLORS
    cabin();roof();lanterns();balloon();ground();b.palette('BalloonMerchantPalette.png')
    data={'Revision':1,'Units':'metres','Front':'-Z','Parts':STATS,
          'StakeTie':[2,.22,-1.37],'CabinTie':[.96,.78,-.76],
          'Motion':{'PeriodSeconds':4.8,'RiseMetres':.12,'GroundStakeFixed':True},
          'Scope':'Art prefab and looping idle animation only; no region placement or merchant logic.'}
    (OUT/'BalloonMerchantManifest.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    print(f'Balloon merchant: {len(STATS)} mesh parts, {sum(x["Triangles"] for x in STATS)} triangles')

if __name__=='__main__':main()
