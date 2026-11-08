"""Tier 1 harvest art: rigid rabbit parts (generate_tier01_rabbit.py), tall cut pine, pond, and nine drop meshes.

Python standard library only. Animation clips and reward generation are deliberately
owned by later gameplay work; coordinates here define reusable art and pivots.
"""
from pathlib import Path
import math
import json
import generate_region01_meshes as base

OUT=Path(__file__).resolve().parents[2]/'Assets/_Game/Art/Harvest/Tier01'
COLORS=[
 (79,49,29),(117,75,42),(151,103,60),(197,153,96),(231,197,142),
 (164,144,119),(226,210,181),(35,32,28),(192,130,124),(215,162,151),
 (185,92,85),(223,142,129),(235,225,195),(234,216,174),
 (100,143,149),(193,124,139),(203,212,177),(60,78,70),
 (91,122,68),(49,72,42),(185,187,130),(84,127,144),
 (114,137,79),(204,174,91),(216,179,70),(82,92,50),
 (198,128,54),(58,116,132),(118,180,181),(123,132,121),(93,128,64),(159,126,78),
]
stats=[]

def ellipsoid(m,center,radii,color,segments=10,rings=5):
    layers=[]
    for j in range(1,rings):
        phi=j*math.pi/rings
        layers.append([m.v((center[0]+radii[0]*math.sin(phi)*math.cos(i*2*math.pi/segments),center[1]+radii[1]*math.cos(phi),center[2]+radii[2]*math.sin(phi)*math.sin(i*2*math.pi/segments))) for i in range(segments)])
    top=m.v((center[0],center[1]+radii[1],center[2]));bottom=m.v((center[0],center[1]-radii[1],center[2]))
    for i in range(segments):
        k=(i+1)%segments
        m.tri(top,layers[0][k],layers[0][i],color);m.tri(bottom,layers[-1][i],layers[-1][k],color)
        for j in range(len(layers)-1):
            m.tri(layers[j][i],layers[j][k],layers[j+1][k],color)
            m.tri(layers[j][i],layers[j+1][k],layers[j+1][i],color)

def face(m,points,color):
    v=[m.v(p) for p in points]
    for i in range(1,len(v)-1):
        m.tri(v[0],v[i],v[i+1],color);m.tri(v[0],v[i+1],v[i],color)

def save(m,largest=0):
    m.save()
    lo=[min(v[a] for v in m.vertices) for a in range(3)];hi=[max(v[a] for v in m.vertices) for a in range(3)]
    size=[hi[a]-lo[a] for a in range(3)]
    footprint=[max(300,round(largest*s/max(size[:2]))) for s in size[:2]] if largest else None
    stats.append(dict(name=m.name,triangles=len(m.faces),bounds_min=lo,bounds_max=hi,front_footprint=footprint))
    if footprint:print('  front footprint:',*footprint)

def trunk(name,height,lower):
    """Pine trunk, revision 2 (user, November 2, 2026: trees read as coloured pillars). Irregular bark outline shared
    across the FallPivot joint, root buttresses on the lower part, broken branch stubs and knots on the upper part,
    one bark colour with slot variation only (the FacetedDetail bark texture carries the ridges). Same frames:
    lower 0-0.65 m, upper 0-height above the FallPivot, oblique sawn top."""
    m=base.Model(name);n=14;rings=[]
    wob=lambda i:1+.06*math.cos(i*2.3)+.04*math.sin(i*5.1)   # same at the joint for both parts
    profile=[(0,.47),(.10,.40),(.26,.345),(.45,.325),(height,.315)] if lower else [(0,.315),(.55,.302),(1.2,.278),(1.75,.257),(height,.235)]
    for y,radius in profile:
        ring=[]
        for i in range(n):
            t=i*2*math.pi/n;r=radius*wob(i)
            yy=y+(.035*math.cos(t) if not lower and y==height else 0)
            ring.append(m.v((r*math.cos(t),yy,r*math.sin(t))))
        rings.append(ring)
    for j in range(len(rings)-1):
        for i in range(n):
            k=(i+1)%n;c=0 if i%4==0 else 2 if i%7==3 else 1   # furrow columns, not a patchwork
            m.tri(rings[j][i],rings[j+1][k],rings[j][k],c);m.tri(rings[j][i],rings[j+1][i],rings[j+1][k],c)
    a=m.v((0,0,0));b=m.v((0,height,0))
    for i in range(n):
        k=(i+1)%n;m.tri(a,rings[0][i],rings[0][k],3);m.tri(b,rings[-1][k],rings[-1][i],4)
    def out_tri(p,q,r,color,inside):
        ids=[m.v(x) for x in (p,q,r)];nrm=base.cross(base.sub(q,p),base.sub(r,p))
        cen=tuple((p[i]+q[i]+r[i])/3 for i in range(3))
        if sum(nrm[i]*(cen[i]-inside[i]) for i in range(3))<0:ids[1],ids[2]=ids[2],ids[1]
        m.tri(ids[0],ids[1],ids[2],color)
    if lower:
        # Root buttresses: angular wedges spreading from the trunk into the ground.
        for i,ang in enumerate((.3,1.55,2.75,3.9,5.2)):
            ca,sa=math.cos(ang),math.sin(ang);side=(-sa,0,ca);w=.075+.02*(i%2)
            top=(.30*ca,.30,.30*sa);foot=(.66*ca,-.01,.66*sa);mid=(.48*ca,.07,.48*sa)
            l=(.34*ca-side[0]*w*1.6,0,.34*sa-side[2]*w*1.6);r=(.34*ca+side[0]*w*1.6,0,.34*sa+side[2]*w*1.6)
            ml=base.add(mid,base.mul(side,-w));mr=base.add(mid,base.mul(side,w))
            inside=(.30*ca,.05,.30*sa)
            for tri_ in ((top,ml,mr),(ml,foot,mr),(top,l,ml),(top,mr,r),(ml,l,foot),(mr,foot,r)):out_tri(*tri_,1 if i%2 else 0,inside)
    else:
        # Broken branch stubs (with a sawn/broken end) and two knots.
        for y,ang,length,r in ((.95,.8,.20,.055),(1.45,3.4,.16,.045),(1.85,5.3,.13,.038)):
            ca,sa=math.cos(ang),math.sin(ang);rad=.29-.025*y
            a0=(rad*.8*ca,y,rad*.8*sa);a1=((rad+length)*ca,y+length*.55,(rad+length)*sa)
            m.cylinder(a0,a1,r,r*.7,6,1,3)
        for y,ang in ((.40,2.1),(1.25,4.6)):
            ca,sa=math.cos(ang),math.sin(ang);rad=.30-.03*y
            m.cylinder((rad*.92*ca,y,rad*.92*sa),((rad+.03)*ca,y,(rad+.03)*sa),.05,.035,6,0,0)
    if not lower:
        # An annual ring on the oblique sawn face; top is a cut trunk, never foliage.
        for i in range(n):
            t=i*2*math.pi/n;u=(i+1)*2*math.pi/n
            points=[]
            for r,angle in [(.16,t),(.16,u),(.145,u),(.145,t)]:
                x=r*math.cos(angle);z=r*math.sin(angle);points.append((x,height+.035*x/.235+.002,z))
            face(m,points,3)
    save(m)

def rabbit_parts_legacy():
    m=base.Model('RabbitBody');ellipsoid(m,(0,.11,-.015),(.19,.22,.32),5);ellipsoid(m,(0,.045,.13),(.145,.15,.19),6);save(m)
    m=base.Model('RabbitHead');ellipsoid(m,(0,.055,.075),(.125,.125,.15),5);ellipsoid(m,(0,.015,.19),(.085,.065,.055),6,8,4)
    ellipsoid(m,(0,.04,.24),(.024,.018,.012),8,6,3)
    for side in [-1,1]:ellipsoid(m,(side*.105,.09,.12),(.018,.023,.016),7,6,3)
    save(m)
    m=base.Model('RabbitEar');ellipsoid(m,(0,.145,0),(.042,.16,.027),5,8,5)
    face(m,[(-.022,.045,-.027),(.022,.045,-.027),(.021,.235,-.027),(0,.285,-.012),(-.021,.235,-.027)],9)
    face(m,[(-.022,.045,.027),(.022,.045,.027),(.021,.235,.027),(0,.285,.012),(-.021,.235,.027)],9);save(m)
    m=base.Model('RabbitFrontFoot');ellipsoid(m,(0,.045,-.005),(.044,.15,.055),5,8,4);ellipsoid(m,(0,-.02,.025),(.055,.075,.10),6,8,4);save(m)
    m=base.Model('RabbitHindFoot');ellipsoid(m,(0,.14,-.015),(.09,.15,.10),5,8,4);ellipsoid(m,(0,.025,0),(.085,.13,.12),5,8,4);ellipsoid(m,(0,-.05,.055),(.065,.06,.135),6,8,3);save(m)
    m=base.Model('RabbitTail');ellipsoid(m,(0,0,0),(.075,.075,.07),6,8,4);save(m)

def rabbit_parts():
    # Faceted revision 4 (user, November 7, 2026): the rabbit has its own generator and palette.
    from generate_tier01_rabbit import write_all
    stats.extend(write_all())

def pond():
    m=base.Model('FishingPool');n=24;centre=m.v((0,.015,0));ring=[]
    for i in range(n):
        t=i*2*math.pi/n;ring.append(m.v((1.50*math.cos(t),.015,1.12*math.sin(t))))
    for i in range(n):m.tri(centre,ring[(i+1)%n],ring[i],27)
    for i in range(7):
        t=i*2*math.pi/7;ellipsoid(m,(1.46*math.cos(t),.06,1.09*math.sin(t)),(.19,.12,.15),29,7,3)
    for x,z in [(1.15,.68),(1.24,.74),(1.05,.8)]:m.cylinder((x,0,z),(x+.04,.38,z),.018,.009,sides=4,bark=30,end=30)
    save(m)
    m=base.Model('FishingRipples')
    for radius in [.18,.34,.52]:
        for i in range(24):
            a=i*2*math.pi/24;b=(i+1)*2*math.pi/24
            face(m,[(radius*math.cos(a),.026,radius*.65*math.sin(a)),(radius*math.cos(b),.026,radius*.65*math.sin(b)),((radius+.015)*math.cos(b),.026,(radius+.015)*.65*math.sin(b)),((radius+.015)*math.cos(a),.026,(radius+.015)*.65*math.sin(a))],28)
    save(m)

def drops():
    m=base.Model('PineLog');m.cylinder((-.60,0,-.12),(.60,0,.12),.33,.32,sides=10,bark=1,end=4);save(m)
    m=base.Model('PineNuts')
    for x,y,z,rx in [(-.15,-.12,0,.15),(.15,-.12,.015,.15),(0,.12,-.02,.14)]:ellipsoid(m,(x,y,z),(rx,.18,.09),13,8,4)
    save(m)
    m=base.Model('RawRabbitMeat');ellipsoid(m,(0,0,0),(.39,.22,.20),10,10,5);ellipsoid(m,(.16,.12,-.025),(.17,.12,.16),11,8,4);save(m,900)
    m=base.Model('Bone');m.cylinder((-.25,0,0),(.25,0,0),.045,.045,sides=7,bark=12,end=12)
    for x in [-.27,.27]:
        for y in [-.045,.045]:ellipsoid(m,(x,y,0),(.085,.067,.062),12,7,3)
    save(m,700)
    m=base.Model('AnimalFat')
    for pos,r in [((0,0,0),(.31,.18,.20)),((.17,.08,.015),(.13,.08,.14))]:ellipsoid(m,pos,r,12,8,4)
    save(m,650)

def fish(name,kind):
    m=base.Model(name);n=10;rings=[]
    tall=.235 if kind=='bluegill' else .17 if kind=='bass' else .125
    depth=.095 if kind=='bluegill' else .085
    profile=[(-.32,.18),(-.23,.74),(-.08,1),(.10,1),(.25,.76),(.36,.35)]
    colors={'trout':14,'bass':18,'bluegill':21,'perch':24};basecolor=colors[kind]
    for x,r in profile:
        rings.append([m.v((x,tall*r*math.cos(i*2*math.pi/n),depth*r*math.sin(i*2*math.pi/n))) for i in range(n)])
    for k in range(len(rings)-1):
        for i in range(n):
            j=(i+1)%n;co=math.cos((i+.5)*2*math.pi/n)
            c=basecolor
            if kind=='trout':c=15 if abs(co)<.35 else 16 if co<-.35 else 14
            elif kind=='bass':c=19 if abs(co)<.35 else 20 if co<-.35 else 18
            elif kind=='bluegill':c=23 if co<-.35 else 22 if co>.6 else 21
            elif kind=='perch':c=25 if k%2==1 and co>-.6 else 24
            m.tri(rings[k][i],rings[k][j],rings[k+1][j],c);m.tri(rings[k][i],rings[k+1][j],rings[k+1][i],c)
    end=m.v((.39,0,0));tail=m.v((-.35,0,0))
    for i in range(n):
        j=(i+1)%n;m.tri(end,rings[-1][i],rings[-1][j],basecolor);m.tri(tail,rings[0][j],rings[0][i],basecolor)
    fin=26 if kind in ('perch','bluegill') else 17 if kind=='trout' else 19
    face(m,[(-.32,0,0),(-.56,tall*.85,0),(-.50,0,0),(-.56,-tall*.85,0)],fin)
    face(m,[(-.16,tall*.85,0),(-.10,tall*1.55,0),(.02,tall*1.35,0),(.16,tall*.90,0)],fin)
    face(m,[(-.02,-tall*.85,0),(.05,-tall*1.4,0),(.19,-tall*.70,0)],fin)
    for side in [-1,1]:
        ellipsoid(m,(.265,tall*.30,side*depth*.69),(.018,.021,.009),7,6,3)
        face(m,[(.09,0,side*depth),(-.06,-tall*.55,side*(depth+.035)),(-.01,.01,side*depth)],fin)
        if kind=='bluegill':ellipsoid(m,(.13,.01,side*(depth+.004)),(.035,.045,.009),17,6,3)
        if kind=='trout':
            for x in [-.16,-.03,.10]:ellipsoid(m,(x,tall*.58,side*depth*.84),(.009,.009,.005),17,5,3)
    save(m,900 if kind=='bluegill' else 1100)

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True);base.OUT=OUT;base.COLORS=COLORS
    import sys
    if '--trunk-only' in sys.argv:
        prior=json.loads((OUT/'mesh-manifest.json').read_text(encoding='utf-8'))
        trunk('PineTrunkLower',.65,True);trunk('PineTrunkUpper',2.15,False);updates={row['name']:row for row in stats}
        merged=[updates.pop(row['name'],row) for row in prior];merged.extend(updates.values())
        (OUT/'mesh-manifest.json').write_text(json.dumps(merged,indent=2)+'\n',encoding='utf-8')
    elif '--rabbit-only' in sys.argv:
        prior=json.loads((OUT/'mesh-manifest.json').read_text(encoding='utf-8'))
        rabbit_parts();updates={row['name']:row for row in stats}
        merged=[updates.pop(row['name'],row) for row in prior];merged.extend(updates.values())
        (OUT/'mesh-manifest.json').write_text(json.dumps(merged,indent=2)+'\n',encoding='utf-8')
    else:
        trunk('PineTrunkLower',.65,True);trunk('PineTrunkUpper',2.15,False)
        rabbit_parts();pond();drops()
        for name,kind in [('RainbowTrout','trout'),('LargemouthBass','bass'),('Bluegill','bluegill'),('YellowPerch','perch')]:fish(name,kind)
        base.palette('Tier01Palette.png')
        (OUT/'mesh-manifest.json').write_text(json.dumps(stats,indent=2)+'\n',encoding='utf-8')
