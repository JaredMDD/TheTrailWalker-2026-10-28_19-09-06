"""Original tier-one crafted models + exact recipe art catalog. Python stdlib only."""
from pathlib import Path
import json, math
import generate_region01_meshes as b
from generate_region05_meshes import sphere
from tier01_recipe_specs import RECIPES, EXTERNAL

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Art/Crafting/Tier01'
COLORS=[(79,49,30),(119,77,43),(158,109,63),(204,166,107),
 (138,120,77),(189,169,120),(226,208,162),(245,228,189),
 (88,74,54),(128,100,68),(167,132,90),(202,170,119),
 (62,78,65),(102,124,87),(151,167,116),(203,205,165),
 (35,37,39),(67,70,71),(113,119,116),(166,170,154),
 (139,66,41),(183,95,55),(222,153,73),(244,195,108),
 (127,33,37),(185,48,42),(224,83,60),(249,215,170),
 (241,232,205),(230,208,143),(249,211,90),(74,117,141)]

def box(m,c,size,k):
    x,y,z=c;a,d,e=[v/2 for v in size]
    v=[m.v((x+sx*a,y+sy*d,z+sz*e)) for sx,sy,sz in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
    top={19:19,23:23,30:30}.get(k,min(31,k+1))
    for q,col in [((0,3,2,1),k),((4,5,6,7),k),((0,4,7,3),max(0,k-1)),((1,2,6,5),k),((3,7,6,2),top),((0,1,5,4),k)]:
        a0,c0,d0,e0=[v[i] for i in q];m.tri(a0,c0,d0,col);m.tri(a0,d0,e0,col)

def tube(m,points,r,k):
    for a,c in zip(points,points[1:]):m.cylinder(a,c,r,r,6,k,k)

def loop(m,c,rx,ry,r,k,turns=1,steps=24):
    tube(m,[(c[0]+rx*math.cos(i*2*math.pi*turns/steps),c[1]+ry*math.sin(i*2*math.pi*turns/steps),c[2]+(.004*i if turns>1 else 0)) for i in range(steps+1)],r,k)

def ring(m,c,radius,thickness,k):
    tube(m,[(c[0]+radius*math.cos(i*math.tau/16),c[1],c[2]+radius*math.sin(i*math.tau/16)) for i in range(17)],thickness,k)

def boot_profile(m,outline,depth,k,bevel=.015):
    """Closed bevelled side-profile extrusion; concave outline is triangulated by ear clipping."""
    def cross(a,c,d):return (c[0]-a[0])*(d[1]-a[1])-(c[1]-a[1])*(d[0]-a[0])
    if sum(outline[i][0]*outline[(i+1)%len(outline)][1]-outline[(i+1)%len(outline)][0]*outline[i][1] for i in range(len(outline)))<0:outline=list(reversed(outline))
    cx=sum(x for x,y in outline)/len(outline);cy=sum(y for x,y in outline)/len(outline)
    inner=[(x+(cx-x)*bevel,y+(cy-y)*bevel) for x,y in outline]
    rings=[]
    for z,points in [(-depth/2,inner),(-depth*.34,outline),(depth*.34,outline),(depth/2,inner)]:
        rings.append([m.v((x,y,z)) for x,y in points])
    for a,c in zip(rings,rings[1:]):
        for i in range(len(outline)):
            j=(i+1)%len(outline);m.tri(a[i],c[j],a[j],k);m.tri(a[i],c[i],c[j],k)
    remaining=list(range(len(inner)));triangles=[]
    while len(remaining)>3:
        for index,j in enumerate(remaining):
            i=remaining[index-1];k0=remaining[(index+1)%len(remaining)]
            if cross(inner[i],inner[j],inner[k0])<=1e-10:continue
            if any(all(cross(a,c,inner[p])>=-1e-10 for a,c in [(inner[i],inner[j]),(inner[j],inner[k0]),(inner[k0],inner[i])]) for p in remaining if p not in (i,j,k0)):continue
            triangles.append((i,j,k0));remaining.pop(index);break
        else:raise ValueError('Boot outline could not be triangulated')
    triangles.append(tuple(remaining))
    for i,j,k0 in triangles:
        m.tri(rings[0][k0],rings[0][j],rings[0][i],k)
        m.tri(rings[-1][i],rings[-1][j],rings[-1][k0],k)

def rabbit_boot(m):
    # Item convention: XY is the icon plane. One boot's lateral silhouette faces -Z,
    # toe to +X. Character footwear remains separate in Characters/CampCommon etc.
    boot_profile(m,[(-.34,-.20),(.32,-.20),(.385,-.155),(.368,-.10),(.31,-.065),
                    (.095,-.035),(.005,.025),(-.025,.225),(-.29,.225),(-.31,-.045),(-.34,-.12)],.235,10,.065)
    boot_profile(m,[(-.35,-.25),(.33,-.25),(.385,-.205),(.378,-.177),(-.34,-.177)],.255,8,.025)
    # Heel is visually separated from the forefoot by the sole's raised arch.
    boot_profile(m,[(-.35,-.28),(-.16,-.28),(-.15,-.235),(-.35,-.235)],.25,8,.03)
    boot_profile(m,[(.07,-.265),(.33,-.265),(.375,-.225),(.08,-.225)],.25,8,.03)
    # Fur cuff with a dark recessed opening, not a solid pillar top.
    count=16;outer=[];inner=[];low=[]
    for i in range(count):
        a=i*math.tau/count
        outer.append(m.v((-.157+.161*math.cos(a),.285,.142*math.sin(a))))
        inner.append(m.v((-.157+.118*math.cos(a),.285,.102*math.sin(a))))
        low.append(m.v((-.157+.161*math.cos(a),.205,.142*math.sin(a))))
    center=m.v((-.157,.263,0))
    for i in range(count):
        j=(i+1)%count
        m.tri(outer[i],outer[j],low[j],6);m.tri(outer[i],low[j],low[i],6)
        m.tri(outer[i],inner[j],outer[j],7);m.tri(outer[i],inner[i],inner[j],7)
        # Slightly recessed dark interior, under the cuff rim.
        a=m.v((m.vertices[inner[i]-1][0],.263,m.vertices[inner[i]-1][2]))
        c=m.v((m.vertices[inner[j]-1][0],.263,m.vertices[inner[j]-1][2]))
        m.tri(center,c,a,0)
        m.tri(inner[i],a,c,9);m.tri(inner[i],c,inner[j],9)
    # Side-facing lace panel and stitches survive the actual front icon capture.
    boot_profile(m,[(-.075,-.04),(.035,-.045),(-.005,.19),(-.105,.19)],.248,9,.02)
    for y in (.015,.065,.115,.165):
        tube(m,[(-.10,y,-.133),(.002,y+.023,-.133)],.009,5)
        tube(m,[(-.10,y+.023,-.135),(.002,y,-.135)],.009,5)
    for x in (-.26,-.18,-.10,.0,.10,.20,.29):box(m,(x,-.157,-.123),(.021,.009,.01),5)
    # Match the old item footprint's aspect exactly; this is a presentation-only replacement.
    lo=[min(v[i] for v in m.vertices) for i in range(2)];hi=[max(v[i] for v in m.vertices) for i in range(2)]
    m.vertices=[((x-lo[0])*.77/(hi[0]-lo[0]),(y-lo[1])*.565/(hi[1]-lo[1]),z) for x,y,z in m.vertices]

def vessel(m,r=.31,height=.38,k=20,fill=None):
    # Closed cross-section: base, outer wall, rim, inner wall, interior floor.
    profile=[(.02,-height/2),(r*.72,-height/2),(r,height/2),(r-.035,height/2+.015),(r-.055,-height/2+.07),(.01,-height/2+.07)]
    rings=[[m.v((rad*math.cos(i*math.tau/12),y,rad*math.sin(i*math.tau/12))) for i in range(12)] for rad,y in profile]
    for a,c in zip(rings,rings[1:]):
        for i in range(12):
            j=(i+1)%12;m.tri(a[i],c[j],a[j],k);m.tri(a[i],c[i],c[j],k)
    if fill is not None:m.cylinder((0,height*.42,0),(0,height*.43,0),r-.045,r-.045,12,fill,fill)

def stitch(m,x,y,width,k=6):
    for i in range(7):box(m,(x+(i-3)*width/7,y,-.147),(.025,.012,.014),k)

def cloth(m,k,fine=False):
    for j in range(3):box(m,(j*.035,-.08+j*.055,j*.015),(.72-j*.04,.055,.38),k+j%2)
    box(m,(.27,.08,-.03),(.13,.22,.39),k)
    stitch(m,0,-.08,.62,7 if fine else k+1)
    if fine:loop(m,(.02,0,-.23),.14,.12,.012,6,steps=16)

def grain(m,k=6,count=14):
    for i in range(count):
        a=i*2.4;r=.05+.18*(i%4)/3
        sphere(m,(r*math.cos(a),.08+.025*(i%3),r*math.sin(a)),(.045,.065,.035),k+i%2,6,3)

def pie(m,kind):
    vessel(m,.39,.22,21,25 if kind=='premium_apple_pie' else 27)
    # Horizontal torus made from short segments.
    tube(m,[(.365*math.cos(i*math.tau/24),.14,.365*math.sin(i*math.tau/24)) for i in range(25)],.034,22)
    if kind=='premium_apple_pie':
        for x in (-.22,-.07,.08,.23):
            length=2*math.sqrt(.33**2-x*x)
            box(m,(x,.16,0),(.045,.028,length),23);box(m,(0,.185,x),(length,.028,.045),22)
    elif kind=='shrimp_pie':
        for x,z in [(-.14,0),(.14,-.08),(0,.18)]:shrimp(m,(x,.16,z),.45,26)
    else:
        for i in range(5):box(m,((i%3-1)*.14,.15,(i//3-.5)*.2),(.1,.06,.1),30)

def shrimp(m,c=(0,0,0),scale=1,k=26):
    pts=[]
    for i in range(7):
        a=(i*31+15)*math.pi/180;p=(c[0]+.27*scale*math.cos(a),c[1]+.22*scale*math.sin(a),c[2]);pts.append(p)
        sphere(m,p,(.09*scale,.075*scale,.065*scale),k,7,3)
    tail=pts[-1]
    for j in (-1,0,1):
        sphere(m,(tail[0]-.075*scale,tail[1]-.045*scale+j*.05*scale,tail[2]),(.11*scale,.036*scale,.025*scale),k-1,6,3)
    sphere(m,(pts[0][0]+.02*scale,pts[0][1]+.025*scale,c[2]-.06*scale),(.013*scale,)*3,16,6,3)
    for j in (-1,1):
        tube(m,[pts[0],(pts[0][0]+.12*scale,pts[0][1]+j*.04*scale,c[2]),(pts[0][0]+.2*scale,pts[0][1]+j*.07*scale,c[2])],.007*scale,k)
    for p in pts[1:5]:
        tube(m,[p,(p[0]-.025*scale,p[1]-.095*scale,c[2]-.025*scale)],.008*scale,k-1)

def milk_jug(m,goat=False):
    # Ceramic presentation only, no new container cost or shop code.
    color=19 if goat else 28
    profile=[(.01,-.33),(.19,-.33),(.26,-.22),(.27,.08),(.16,.23),(.13,.27),(.13,.39),(.10,.39),(.10,.31),(.01,.31)]
    rings=[[m.v((r*math.cos(i*math.tau/12),y,r*math.sin(i*math.tau/12))) for i in range(12)] for r,y in profile]
    for a,c in zip(rings,rings[1:]):
        for i in range(12):
            j=(i+1)%12;m.tri(a[i],c[j],a[j],color);m.tri(a[i],c[i],c[j],color)
    m.cylinder((0,.33,0),(0,.335,0),.099,.099,12,28,28)
    loop(m,(.27,.045,0),.14,.19,.034,color,steps=18)
    if goat:
        tube(m,[(-.12,.025,-.266),(-.09,.10,-.264),(0,.045,-.28),(.09,.10,-.264),(.12,.025,-.266)],.017,28)
    else:
        for x,y,rx,ry in [(-.13,.035,.067,.09),(.10,-.10,.08,.065),(.03,.12,.04,.05)]:
            sphere(m,(x,y,-.245), (rx,ry,.018),16,7,3)

def fish(m,key):
    # Species retain raw silhouettes while the palette changes to cooked browns.
    species=key.replace('roasted_','').replace('herb_','rainbow_')
    source=ROOT/('Assets/_Game/Art/Harvest/Tier01/'+''.join(s.title() for s in species.split('_'))+'.obj')
    vs=[]
    for line in source.read_text().splitlines():
        p=line.split()
        if p and p[0]=='v':vs.append(m.v(tuple(map(float,p[1:]))))
        elif p and p[0]=='f':
            idx=[int(s.split('/')[0])-1 for s in p[1:]]
            m.tri(*(vs[i] for i in idx),21 if len(m.faces)%5 else 22)
    tube(m,[(-.65,-.17,-.12),(.65,-.17,-.12)],.025,2)
    if key=='herb_trout':
        for x in (-.16,0,.16):sphere(m,(x,.12,-.13),(.09,.035,.025),13,6,3)

def model(key,name):
    m=b.Model(name)
    if key=='pine_plank':
        box(m,(0,0,0),(1,.13,.29),2)
        for y in (-.03,.025):box(m,(.05,y,-.15),(.8,.009,.005),1)
    elif key=='pine_charcoal':
        for c,r in [((-.22,0,0),(.22,.15,.18)),((.13,.04,.02),(.24,.19,.2)),((0,-.03,-.19),(.18,.13,.15))]:sphere(m,c,r,16,7,3)
    elif key in ('hemp_cord','hemp_thread','nettle_thread'):
        k=5 if key!='nettle_thread' else 13
        if key=='hemp_cord':
            loop(m,(0,0,-.08),.28,.22,.032,k,3,72);tube(m,[(.28,0,.7),(.4,-.15,.55),(.27,-.26,.4)],.032,k)
        else:
            m.cylinder((0,-.3,0),(0,.3,0),.12,.12,8,2,3)
            for y in (-.3,.3):m.cylinder((0,y-.025,0),(0,y+.025,0),.22,.22,10,2,3)
            for i in range(9):ring(m,(0,-.22+i*.055,0),.16,.016,k)
    elif 'cloth' in key:cloth(m,13 if key=='nettle_cloth' else 5,'fine' in key)
    elif 'pelt' in key or 'leather' in key:
        k=10 if 'leather' in key else 6
        sphere(m,(0,0,0),(.42,.055,.3),k,12,3)
        for x in (-.31,.31):
            for z in (-.22,.22):sphere(m,(x,0,z),(.15,.04,.12),k,7,3)
        if 'fine' in key:stitch(m,0,.04,.55,7)
        if 'leather' in key:box(m,(.22,.03,0),(.1,.08,.48),9)
    elif key=='plant_stuffing':
        for i in range(9):sphere(m,((i%3-1)*.17,(i//3)*.04,((i*7)%5-2)*.08),(.17,.1,.13),6+i%2,7,3)
    elif key=='pine_beam':
        box(m,(0,0,0),(1.1,.24,.24),2)
        for x in (-.3,.3):tube(m,[(x,-.13,-.14),(x,.14,-.14),(x,.14,.14),(x,-.13,.14),(x,-.13,-.14)],.02,5)
    elif key=='pine_panel':
        for x in (-.25,0,.25):box(m,(x,0,0),(.24,.65,.08),2)
        for y in (-.21,.21):box(m,(0,y,-.06),(.78,.08,.06),1)
    elif key in ('limestone_tiles','fired_bricks'):
        for i in range(2 if key=='limestone_tiles' else 3):box(m,((i%2)*.16-.08,i*.1,(i%2)*.07),(.52,.08 if key=='limestone_tiles' else .14,.29),19 if key=='limestone_tiles' else 21)
    elif key=='polished_pebble':sphere(m,(0,0,0),(.28,.22,.16),19,12,5)
    elif key=='clay_cup':
        vessel(m,.24,.47,20);loop(m,(.25,0,0),.13,.15,.03,21,steps=20)
    elif key in ('wheat_flour','mortar'):
        sphere(m,(0,-.03,0),(.4,.13,.3),28 if key=='wheat_flour' else 18,12,4)
        sphere(m,(-.04,.055,.02),(.24,.18,.2),28 if key=='wheat_flour' else 19,9,3)
    elif key=='reed_braid':
        for j in range(3):tube(m,[(.045*math.sin(i*.9+j*math.tau/3),-.4+i*.04,.045*math.cos(i*.9+j*math.tau/3)) for i in range(21)],.027,13+j%2)
    elif key=='pine_axe_normal':
        tube(m,[(-.12,-.55,0),(0,.36,0),(.06,.49,0)],.045,2)
        sphere(m,(.14,.30,0),(.3,.15,.055),18,5,3)
        for y in (.24,.29,.34,.39):tube(m,[(-.065,y,-.07),(.065,y+.02,-.07)],.016,5)
    elif key=='pine_slingshot_normal':
        tube(m,[(0,-.4,0),(0,.03,0),(-.24,.38,0)],.05,2);tube(m,[(0,.03,0),(.24,.38,0)],.045,2)
        tube(m,[(-.24,.38,0),(0,.16,-.16),(.24,.38,0)],.025,9);box(m,(0,.16,-.16),(.13,.09,.03),10)
    elif key=='pine_rod_normal':
        tube(m,[(-.18,-.7,0),(-.1,-.15,0),(.08,.5,0),(.17,.85,0)],.022,2)
        tube(m,[(.17,.85,0),(.38,.48,0),(.38,-.25,0),(.34,-.34,0),(.29,-.31,0)],.009,6)
        tube(m,[(.38,-.25,0),(.38,-.34,0),(.32,-.37,0),(.29,-.31,0)],.015,28)
    elif key=='rabbit_hat':
        sphere(m,(0,.07,0),(.36,.24,.27),10,12,5)
        box(m,(0,-.09,-.02),(.79,.11,.53),6)
        for x in (-.32,.32):box(m,(x,-.2,0),(.12,.25,.28),6)
    elif key=='rabbit_boots':
        rabbit_boot(m)
    elif key=='hemp_shirt':
        box(m,(0,-.01,0),(.56,.66,.18),5)
        for x in (-.4,.4):box(m,(x,.17,0),(.24,.3,.18),5)
        for x in (-.15,.15):box(m,(x,-.15,-.105),(.2,.19,.035),10);stitch(m,x,-.24,.18)
        box(m,(0,.31,-.03),(.19,.06,.2),9)
    elif key=='hemp_pants':
        box(m,(0,.26,0),(.59,.24,.18),5)
        for x in (-.16,.16):box(m,(x,-.13,0),(.26,.58,.18),5)
        box(m,(0,.34,-.11),(.62,.06,.035),9)
        for x in (-.23,.23):tube(m,[(x,.3,-.15),(x,.18,-.16),(x+.05,.17,-.16)],.02,2)
    elif key=='starter_backpack':
        box(m,(0,0,0),(.56,.66,.27),5);box(m,(0,.25,-.17),(.6,.22,.08),9)
        for x in (-.23,.23):tube(m,[(x,-.36,.16),(x,.37,.16)],.033,2)
        for x in (-.16,.16):box(m,(x,-.06,-.155),(.05,.42,.025),9)
        box(m,(0,-.19,-.19),(.28,.22,.11),10);loop(m,(0,.35,.1),.12,.13,.025,9,steps=18)
    elif key in ('cow_milk','goat_milk'):milk_jug(m,key=='goat_milk')
    elif key in ('apple_juice','strawberry_cow_yogurt_drink','cow_cream','cow_yogurt'):
        fill=29 if key=='apple_juice' else 27 if key.startswith('strawberry') else 28
        vessel(m,.24,.5,2 if key=='apple_juice' else 20,fill)
        if key.startswith('strawberry'):sphere(m,(.17,.2,-.1),(.075,.09,.07),25,8,3)
    elif key in ('cow_cheese','goat_cheese','mixed_milk_cheese'):
        m.cylinder((0,-.13,0),(0,.13,0),.32,.32,12,29 if key!='goat_cheese' else 28,30 if key=='cow_cheese' else 28)
        for x,y in [(-.13,-.01),(.11,.05),(.02,-.07)]:sphere(m,(x,y,-.3),(.035,.028,.012),22 if key!='goat_cheese' else 19,7,3)
        if key=='mixed_milk_cheese':box(m,(0,.145,0),(.06,.02,.6),30)
    elif key=='cow_butter':box(m,(0,0,0),(.46,.22,.29),29);box(m,(0,.12,0),(.35,.02,.2),30)
    elif key in ('premium_apple_pie','shrimp_pie','cow_cheese_tart'):pie(m,key)
    elif key in ('apple_oat_compote','red_berry_oat_porridge'):
        vessel(m,.36,.27,20,29);grain(m,7,16)
        for x,z in [(-.15,-.12),(.13,.09),(.03,-.1)]:sphere(m,(x,.17,z),(.07,.06,.07),25 if key.startswith('red') else 23,7,3)
    elif key=='berry_oat_cakes':
        for x,y,z in [(-.12,0,0),(.12,.09,.03)]:
            m.cylinder((x,y-.035,z),(x,y+.035,z),.24,.23,10,22,23)
            for j in range(4):sphere(m,(x+.12*math.cos(j*2),y+.05,z+.12*math.sin(j*2)),(.03,)*3,24,6,3)
    elif key=='toasted_pine_nuts':grain(m,22,18)
    elif key.startswith('roasted_') and key not in ('roasted_mushrooms','roasted_rabbit','roasted_shrimp') or key=='herb_trout':fish(m,key)
    elif key in ('mushrooms','roasted_mushrooms'):
        for x,y,z in [(-.17,-.05,0),(.16,.05,.03),(.02,-.02,-.15)]:
            # Stem slot 28: the cylinder helper shades every third side one slot darker, and 27 -> 26 is red (it
            # drew a red stripe on each stem; November 5, 2026). 28 -> 27 stays cream.
            m.cylinder((x,y-.2,z),(x,y+.02,z),.035,.04,6,28,28)
            sphere(m,(x,y+.06,z),(.17,.09,.14),21 if key.startswith('roasted') else 10,
                   10 if key.startswith('roasted') else 16, 4 if key.startswith('roasted') else 6)
        if key.startswith('roasted'):tube(m,[(-.4,-.22,-.08),(.4,-.22,-.08)],.02,2)
    elif key in ('shrimp','roasted_shrimp'):
        shrimp(m,k=26 if key.startswith('roasted') else 19)
        if key.startswith('roasted'):tube(m,[(-.4,-.16,.07),(.4,-.16,.07)],.025,2)
    elif key=='roasted_rabbit':
        sphere(m,(0,0,0),(.28,.17,.2),21,10,4);sphere(m,(.24,-.05,0),(.18,.12,.13),22,9,4)
        tube(m,[(-.48,-.1,.03),(.48,-.1,.03)],.025,2)
    elif key=='egg':sphere(m,(0,0,0),(.17,.23,.17),28,10,6)
    elif key=='pine_bird':
        sphere(m,(0,0,0),(.27,.19,.16),2,9,4);sphere(m,(.2,.19,0),(.13,.13,.12),3,8,3)
        m.cylinder((.27,.2,0),(.43,.18,0),.06,0,5,2,2);sphere(m,(.24,.22,-.108),(.018,)*3,0,6,3)
        box(m,(0,-.22,0),(.38,.07,.26),1);tube(m,[(0,-.2,0),(0,-.08,0)],.04,2)
    elif key=='cord_charm':
        loop(m,(0,.15,0),.22,.28,.025,5,steps=30);sphere(m,(0,-.22,-.04),(.16,.2,.09),19,10,4)
        tube(m,[(-.1,-.1,-.14),(0,-.27,-.14),(.1,-.1,-.14)],.017,5)
    elif key=='rabbit_toy':
        sphere(m,(0,-.07,0),(.23,.27,.16),6,9,4);sphere(m,(0,.2,-.01),(.19,.17,.15),6,9,4)
        for x in (-.09,.09):sphere(m,(x,.43,0),(.055,.2,.035),10,7,4);sphere(m,(x,.23,-.15),(.02,)*3,8,6,3)
        for x in (-.18,.18):sphere(m,(x,-.26,-.04),(.11,.09,.14),6,7,3)
        stitch(m,0,0,.25,9)
    elif key in ('pinecone_ornament','pinecone_rabbit'):
        sphere(m,(0,0,0),(.2,.3,.18),1,9,4)
        for j in range(4):
            for i in range(8):
                a=i*math.tau/8+j*.3;sphere(m,(.17*math.cos(a),-.2+j*.12,.15*math.sin(a)),(.065,.075,.035),2+j%2,5,3)
        if key=='pinecone_ornament':loop(m,(0,.38,0),.1,.14,.018,5,steps=16);tube(m,[(-.27,-.33,0),(.27,-.33,0)],.03,2)
        else:
            for x in (-.09,.09):sphere(m,(x,.39,0),(.05,.2,.04),3,7,3)
            sphere(m,(0,.15,-.17),(.1,.08,.05),6,8,3);box(m,(0,-.33,0),(.5,.08,.34),2)
    else:raise ValueError(key)
    # Center authored bounds, keeping mesh and inventory axes identical.
    lo=[min(v[i] for v in m.vertices) for i in range(3)];hi=[max(v[i] for v in m.vertices) for i in range(3)]
    m.vertices=[tuple(p[i]-(hi[i]+lo[i])/2 for i in range(3)) for p in m.vertices]
    if key == 'mushrooms':
        m.fit_bounds((-.335,-.225,-.223148),(.335,.225,.223148))
    m.save();d=[hi[i]-lo[i] for i in range(3)]
    # User footprint overrides (October 31, 2026): planks read like branches in the bag; log-like, slightly larger.
    # Roasted dishes include a full-width skewer: enlarged so the food reads as large as its raw item (user).
    overrides={'AppleJuice':(485,520),'CowCream':(485,520),'CowYogurt':(485,520),'StrawberryCowYogurtDrink':(462,520),'Egg':(355,480),'PinePlank':(1100,550),'PineBeam':(1100,550),'HempShirt':(900,780),'StarterBackpack':(939,1350),'RoastedRabbit':(1300,462),'HerbTrout':(1400,467),'RoastedRainbowTrout':(1400,421),
               'RoastedLargemouthBass':(1400,541),'RoastedBluegill':(1150,615),'RoastedYellowPerch':(1400,467)}
    if name in overrides:return dict(Model=name,Triangles=len(m.faces),Width=overrides[name][0],Height=overrides[name][1])
    return dict(Model=name,Triangles=len(m.faces),Width=max(300,round(900*d[0]/max(d[:2]))),Height=max(300,round(900*d[1]/max(d[:2]))))

def main():
    OUT.mkdir(parents=True,exist_ok=True);b.OUT=OUT;b.COLORS=COLORS
    art=[]
    for r in RECIPES:
        if r['OutputId']=='material.pine_log':r['PrefabPath']='Tier01/Drops/PineLog';continue
        exclusive=r['UnlockSource'].startswith('Optional balloon recipe')
        b.OUT=ROOT/'Assets/_Game/Art/Crafting/BalloonExclusive' if exclusive else OUT
        b.OUT.mkdir(parents=True,exist_ok=True)
        art.append(model(r['Style'],r['Model']));r['PrefabPath']=('BalloonExclusive/Items/' if exclusive else 'Tier01/Crafted/')+r['Model']
    b.OUT=OUT
    for e in EXTERNAL:
        key={'CowMilk':'cow_milk','GoatMilk':'goat_milk'}.get(e['Model'],e['Model'].lower())
        art.append(model(key,e['Model']));e['PrefabPath']='Tier01/Crafted/'+e['Model']
    b.palette('Tier01CraftedPalette.png')
    b.OUT=ROOT/'Assets/_Game/Art/Crafting/BalloonExclusive';b.palette('Tier01CraftedPalette.png');b.OUT=OUT
    # Explicit provenance is available to the future offer filter; never infer tier from location or Q5.
    sources=[]
    loose=[['material.pine_branch','animal.rabbit.pelt','food.apples'],['material.hemp','material.flint','material.pine_cone'],['material.nettle_fiber','material.clay','ingredient.rock_salt'],['material.limestone','food.strawberries','ingredient.mushrooms'],['ingredient.oats','material.pebbles','food.berries'],['material.reeds','ingredient.wheat','material.sand']]
    for region,items in enumerate(loose,1):
        for item in items:sources.append(dict(ItemId=item,Regions=[region],Source='Exact loose source discovery',Blocked=False))
    for item in ('material.pine_branch','material.pine_log','ingredient.pine_nuts','animal.rabbit.pelt','animal.rabbit.meat','material.bone','ingredient.animal_fat'):
        sources.append(dict(ItemId=item,Regions=list(range(1,7)),Source='Pine chopping' if item in ('material.pine_branch','material.pine_log','ingredient.pine_nuts') else 'Rabbit hunting',Blocked=False))
    for species,regions in [('rainbow_trout',[1,3,5]),('largemouth_bass',[2,3,6]),('bluegill',[1,4,6]),('yellow_perch',[2,4,5])]:
        sources.append(dict(ItemId='fish.'+species,Regions=regions,Source='Exact local fishing pair',Blocked=False))
    loose_ids={s['ItemId'] for s in sources}
    for e in EXTERNAL:
        if e['Id'] not in loose_ids:sources.append(dict(ItemId=e['Id'],Regions=[],Source=e['Source'],Blocked=False))
    # Tea herbs moved from region 4 loose gathering to the farm catalog (October 30, 2026).
    sources.append(dict(ItemId='ingredient.tea_herbs',Regions=[4],Source='Farm; require region 4 discovered, settlement reached and exact listing',Blocked=False))
    raw={s['ItemId'] for s in sources};producer={r['OutputId']:r for r in RECIPES}
    def dependencies(item,visiting=()):
        if item in raw:return {item},set()
        if item in visiting:raise ValueError('Recipe cycle: '+item)
        recipe=producer[item];leaves=set();conversions={recipe['Id']}
        for i in recipe['Inputs']:
            a,c=dependencies(i['ItemId'],visiting+(item,));leaves.update(a);conversions.update(c)
        return leaves,conversions
    for r in RECIPES:
        leaves=set();conversions=set()
        for i in r['Inputs']:
            a,c=dependencies(i['ItemId']);leaves.update(a);conversions.update(c)
        r['CraftingPathSourceIds']=sorted(leaves);r['ConversionRecipeIds']=sorted(conversions)
        r['BlockedSource']=any(s['Blocked'] and s['ItemId'] in leaves for s in sources)
    # All prerequisites are authoring requirements; owning/buying an item never teaches its recipe.
    data=dict(Revision=1,Recipes=RECIPES,ExternalInputs=EXTERNAL,Models=art,Sources=sources,
        UnlockPolicy='Require recipe acquisition AND discovered exact ingredient sources AND known intermediate conversions or accessible exact-item shops. Recursively check upstream sources; no future-tier pantry bypass. Never offer unassigned sources. Optional box/bakery recipes cannot block mandatory missions.',
        ArtPolicy='Catalog is authoring data only. No auto-learning, crafting transaction, shop stock or region spawn changes. Serving vessels are illustrative unless explicitly listed as an ingredient. Recipes are knowledge, not inventory scroll stacks. CraftingPathSourceIds/ConversionRecipeIds describe the self-crafted supply path; an explicitly reachable exact-output shop may provide an alternate path, never a recipe grant. Source records for the same ID are alternatives; Regions lists are any eligible source region, not all regions required.')
    (OUT/'Tier01Recipes.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    byname={a['Model']:a for a in art};definitions=[];paths=[]
    for r in RECIPES:
        paths.append((r['OutputId'],r['PrefabPath']))
        if r['OutputId'] in ('material.pine_log','tool.pine_axe.normal','item.toasted_pine_nuts'):continue
        a=byname[r['Model']];args=''
        if r['Category']=='Tool':args=', stackable: false'
        elif r['Category']=='Wearable':args=', stackable: false, gearSlot: TheTrailWalker.Domain.Equipment.GearSlot.'+r['Slot']+', gearRank: 1'
        elif r['Hearts']:args=f", foodHearts: {r['Hearts']}, preparedFood: true"
        elif r['Category']=='Food' or r['OutputId']=='ingredient.wheat_flour':args=', category: ItemCategory.RefinedIngredient'
        definitions.append(f'            new ItemDefinition("{r["OutputId"]}", "{r["Name"]}", {a["Width"]}, {a["Height"]}{args}),')
    for e in EXTERNAL:
        a=byname[e['Model']];paths.append((e['Id'],e['PrefabPath']))
        definitions.append(f'            new ItemDefinition("{e["Id"]}", "{e["Name"]}", {a["Width"]}, {a["Height"]}, category: ItemCategory.RawIngredient),')
    code='''// Generated by Tools/Art/generate_tier01_crafted_items.py. Catalog entries do not grant knowledge.
namespace TheTrailWalker.Domain.Items
{
    public static class Tier1CraftedItems
    {
        public static readonly ItemDefinition[] NewDefinitions = {
'''+ '\n'.join(definitions)+'''
        };
        public static ItemDefinition[] AppendTo(ItemDefinition[] originals)
        {
            var result = new ItemDefinition[originals.Length + NewDefinitions.Length];
            System.Array.Copy(originals, result, originals.Length);
            System.Array.Copy(NewDefinitions, 0, result, originals.Length, NewDefinitions.Length);
            return result;
        }
        public static string VisualPath(string id)
        {
            switch (id)
            {
'''+ '\n'.join(f'                case "{i}": return "{p}";' for i,p in paths)+'''
                default: return null;
            }
        }
    }
}
'''
    (ROOT/'Assets/_Game/Scripts/Domain/Items/Tier1CraftedItems.cs').write_text(code,encoding='utf-8')
    docs=ROOT/'Docs/Art/Tier01';docs.mkdir(parents=True,exist_ok=True)
    notes=['TIER 1 RECIPE AND CRAFTED ITEM DESIGN / revision 1',f'{len(RECIPES)} exact recipe designs; {len(art)} new meshes; existing Pine log reused.',data['ArtPolicy'],data['UnlockPolicy'],
        'Source map: loose region 1 branch/pelt/apples; 2 hemp/flint/cone; 3 nettle/clay/salt; 4 limestone/strawberries/mushrooms; 5 oats/pebbles/red berries; 6 reeds/wheat/sand.',
        'Interaction sources: tier-one pine branch/log/nuts; rabbit pelt/meat/bone/fat. Fish regional pairs remain in HarvestArtCatalog. Shrimp stays fishery-only.',
        'All nonmaterial recipes begin at mastery 1 only when learned. Material recipes are unlevelled. Tool/gear batch x1 only; other outputs stack to 1000.',
        'Wearables: rank 1; hat four-heart cap, boots speed, shirt two pockets, pants two hooks, backpack 8x4. Do not infer infinite durability from an art prefab.',
        'Timers use category defaults where unspecified; explicit new fixed Material durations are proposals. Dairy has Food mastery but no newly invented direct recovery.',
        'Mushrooms are region 4 loose resources; Tea herbs are a farm-catalog herb after region 4. Mixed-milk cheese remains optional.',
        'Rebuild: python Tools/Art/generate_tier01_crafted_items.py, then The Trail Walker > Art > Rebuild Tier 1 Crafted Assets in Unity.',
        'Pending focused retests: M2 inventory front-view icons, footprints/rotation, stackable materials/meals and x1 tools/gear (existing axe gains a mesh); M3 rank-one equip/unequip and finite durability profiles; M4 old ID/order compatibility plus new-ID quality/profile/overflow save round trips. Not passed device validation. M1 movement/stamina logic unchanged.',
        'M7 must consume exact recipe data and enforce acquisition/source gates before enabling crafting.']
    store_notes=['TIER 1 STORE-DEPENDENT INGREDIENT ART',
        'Assets and inventory visual mappings only; settlement/balloon shop logic, stock, purchase UI and unlocks remain unimplemented.',
        'Ceramic milk jugs distinguish cow milk (cream with dark patches) and goat milk (gray with horn mark). Egg is cream and oval; shrimp has segmented body, tail fan, antennae and legs; mushrooms have tan caps and pale stems.',
        'Containers are part of the illustration, not additional recipe ingredients, equipment or reusable returns.',
        'Mushrooms are gathered loose in region 4 (not a shop item); the art is shared with the trail pickup.',
        'Dairy intermediates (cream/butter/cheeses/yogurt) retain their existing crafted meshes. Flour/salt/produce retain their existing resource/conversion art; optional shop availability does not create new item varieties.',
        'All five item IDs already participate in the shared inventory/world visual lookup; no new spawning or recipe knowledge is granted.']
    for e in EXTERNAL:
        notes.append(e['Name']+': '+e['Source'])
        direct=[r['Name'] for r in RECIPES if any(i['ItemId']==e['Id'] for i in r['Inputs'])]
        indirect=[r['Name'] for r in RECIPES if e['Id'] in r['CraftingPathSourceIds'] and r['Name'] not in direct]
        store_notes.append('\n'+e['Name']+' | '+e['Id']+' | '+e['PrefabPath']+'\nSource: '+e['Source']+'\nDirect recipe uses: '+', '.join(direct)+'\nFurther crafted dependencies: '+(', '.join(indirect) or 'None'))
    (docs/'store-ingredient-designs.txt').write_text('\n'.join(store_notes)+'\n',encoding='utf-8')
    for r in RECIPES:notes.append(f"\n{r['Id']} | {r['Name']} | {r['Category']} | "+' + '.join(str(i['Quantity'])+' '+i['ItemId'] for i in r['Inputs'])+f" -> {r['Yield']} {r['OutputId']} | {r['BaseSeconds']} s base | {r['Hearts']} hearts/serving\nUnlock: {r['UnlockSource']}. {r['Note']}")
    (docs/'crafted-item-designs.txt').write_text('\n'.join(notes)+'\n',encoding='utf-8')
    print(f'{len(RECIPES)} recipes; {len(art)} meshes; {len(definitions)} new item definitions.')

if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--mesh-only',choices=[r['Style'] for r in RECIPES],help='Rebuild one existing mesh and its triangle metadata, without rewriting item definitions or recipes.')
    args=parser.parse_args()
    if args.mesh_only:
        b.OUT=OUT;b.COLORS=COLORS
        recipe=next(r for r in RECIPES if r['Style']==args.mesh_only)
        if recipe['UnlockSource'].startswith('Optional balloon recipe'):b.OUT=ROOT/'Assets/_Game/Art/Crafting/BalloonExclusive'
        result=model(recipe['Style'],recipe['Model'])
        path=OUT/'Tier01Recipes.json';data=json.loads(path.read_text(encoding='utf-8'))
        entry=next(x for x in data['Models'] if x['Model']==recipe['Model'])
        assert (entry['Width'],entry['Height'])==(result['Width'],result['Height']), 'Mesh-only export must preserve the existing footprint'
        entry['Triangles']=result['Triangles']
        path.write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    else:main()
