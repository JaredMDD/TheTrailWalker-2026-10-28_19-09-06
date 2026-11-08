"""Targeted drop geometry. Never regenerates interactables or reward data."""
import math
import generate_region01_meshes as b
import generate_tier01_crafted_items as c
import generate_tier01_harvest_meshes as h


def capped_log(m):
    # A closed bark sleeve with cut-face rings at both ends, along the original slanted axis.
    a=(-.60,0,-.12); end=(.60,0,.12)
    m.cylinder(a,end,.33,.32,14,1,4)
    axis=b.unit(b.sub(end,a)); u=b.unit(b.cross(axis,(0,1,0))); v=b.cross(axis,u)
    for centre,sign in ((a,-1),(end,1)):
        centre=b.add(centre,b.mul(axis,sign*.003))
        for radius in (.075,.145,.225,.285):
            rings=[]
            for r in (radius-.0025,radius+.0025):
                rings.append([m.v(b.add(centre,b.add(b.mul(u,r*math.cos(i*math.tau/24)),b.mul(v,r*math.sin(i*math.tau/24))))) for i in range(24)])
            for i in range(24):
                j=(i+1)%24
                for face in ((rings[1][i],rings[1][j],rings[0][j]),(rings[1][i],rings[0][j],rings[0][i])):
                    m.tri(*(face if sign>0 else tuple(reversed(face))),3)
        for ang in (.4,2.9,4.8):
            direction=b.add(b.mul(u,math.cos(ang)),b.mul(v,math.sin(ang)))
            m.cylinder(b.add(centre,b.mul(direction,.23)),b.add(centre,b.mul(direction,.31)),.004,.001,3,2,2)


def fish(m,kind):
    n=16; rings=[]
    tall=.235 if kind=='bluegill' else .17 if kind=='bass' else .125
    depth=.095 if kind=='bluegill' else .085
    basecolor={'trout':14,'bass':18,'bluegill':21,'perch':24}[kind]
    profile=[(-.32,.18),(-.23,.74),(-.08,1),(.10,1),(.25,.76),(.36,.35)]
    for x,r in profile:
        rings.append([m.v((x,tall*r*math.cos(i*math.tau/n),depth*r*math.sin(i*math.tau/n))) for i in range(n)])
    for row in range(len(rings)-1):
        for i in range(n):
            j=(i+1)%n; co=math.cos((i+.5)*math.tau/n); color=basecolor
            if kind=='trout':color=15 if abs(co)<.35 else 16 if co<-.35 else 14
            elif kind=='bass':color=19 if abs(co)<.35 else 20 if co<-.35 else 18
            elif kind=='bluegill':color=23 if co<-.35 else 22 if co>.6 else 21
            elif kind=='perch':color=25 if row%2 and co>-.6 else 24
            m.tri(rings[row][i],rings[row][j],rings[row+1][j],color)
            m.tri(rings[row][i],rings[row+1][j],rings[row+1][i],color)
    nose=m.v((.39,0,0));tail=m.v((-.35,0,0))
    for i in range(n):
        j=(i+1)%n;m.tri(nose,rings[-1][i],rings[-1][j],basecolor);m.tri(tail,rings[0][j],rings[0][i],basecolor)
    fin=32 # Extra semantic slot, independent from scaled body colours.
    h.face(m,[(-.32,0,0),(-.56,tall*.85,0),(-.50,0,0),(-.56,-tall*.85,0)],fin)
    h.face(m,[(-.16,tall*.85,0),(-.10,tall*1.55,0),(.02,tall*1.35,0),(.16,tall*.90,0)],fin)
    h.face(m,[(-.02,-tall*.85,0),(.05,-tall*1.4,0),(.19,-tall*.70,0)],fin)
    for sign in (-1,1):
        h.ellipsoid(m,(.265,tall*.30,sign*depth*.69),(.019,.021,.010),7,8,4)
        h.ellipsoid(m,(.270,tall*.34,sign*(depth*.69+.009)),(.005,.006,.002),12,6,3)
        h.face(m,[(.09,0,sign*depth),(-.06,-tall*.55,sign*(depth+.035)),(-.01,.01,sign*depth)],fin)
        # Gill crescent, lateral line and fin rays are geometry, not tiny text.
        for p,q in zip([( .20,tall*.64,sign*depth*.65),(.16,0,sign*(depth+.004)),(.20,-tall*.62,sign*depth*.66)],
                       [(.16,0,sign*(depth+.004)),(.20,-tall*.62,sign*depth*.66)]):
            m.cylinder(p,q,.003,.003,3,33,33)
        if kind=='trout':
            for x in (-.18,-.06,.06):h.ellipsoid(m,(x,tall*.60,sign*depth*.82),(.008,.008,.003),33,6,3)
        if kind=='bluegill':h.ellipsoid(m,(.13,.01,sign*(depth+.004)),(.035,.045,.009),33,8,4)
    for sign in (-1,1):
        m.cylinder((-.34,0,sign*.004),(-.53,tall*.62,sign*.004),.002,.001,3,33,33)
        m.cylinder((-.34,0,sign*.004),(-.53,-tall*.62,sign*.004),.002,.001,3,33,33)


def build(name):
    m=b.Model(name)
    if name=='PineLog':capped_log(m)
    elif name=='PineNuts':
        for x,y,z,rx in [(-.15,-.12,0,.15),(.15,-.12,.015,.15),(0,.12,-.02,.14)]:
            h.ellipsoid(m,(x,y,z),(rx,.18,.09),13,12,6)
            m.cylinder((x,y-.14,z-.083),(x,y+.13,z-.083),.003,.003,3,31,31)
    elif name=='RawRabbitMeat':
        h.ellipsoid(m,(0,0,0),(.39,.22,.20),10,14,7)
        h.ellipsoid(m,(.16,.12,-.025),(.17,.12,.16),11,12,5)
        for x in (-.18,-.07,.04,.15):
            c.tube(m,[(x,-.08,-.177),(x+.03,.025,-.20),(x+.045,.10,-.15)],.006,9)
        h.ellipsoid(m,(-.08,-.12,-.10),(.21,.045,.10),12,12,4)
    elif name=='Bone':
        m.cylinder((-.25,0,0),(.25,0,0),.045,.045,10,12,12)
        for x in (-.27,.27):
            for y in (-.045,.045):h.ellipsoid(m,(x,y,0),(.085,.067,.062),12,10,5)
    elif name=='AnimalFat':
        # Distinct pale folded portion, rather than a recoloured meat cut.
        c.box(m,(0,-.03,0),(.49,.12,.32),12)
        for x in (-.16,0,.16):h.ellipsoid(m,(x,.07,0),(.11,.09,.17),13,10,5)
        c.tube(m,[(-.22,.03,-.17),(0,.06,-.18),(.21,.03,-.17)],.007,9)
    else:
        kind={'RainbowTrout':'trout','LargemouthBass':'bass','Bluegill':'bluegill','YellowPerch':'perch'}[name]
        fish(m,kind)
    return m
