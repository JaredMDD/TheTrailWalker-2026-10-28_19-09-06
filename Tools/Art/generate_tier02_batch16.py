"""Batch 16: continuous lined ear-covering hat and fine-leather/tin boots."""
import sys,math,json
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import generate_tier02_batch13 as previous
import faceted_people as fp
from generate_tier02_batch06 import tube
from tier02_batch_workflow import generate,ROOT

COLORS=[(83,61,43),(111,84,57),(83,61,43),(181,180,148),(91,63,43),
        (62,43,32),(127,93,61),(172,160,116),(155,169,166),(106,121,120)]
TYPES=['fur','fur','leather','cloth','leather','leather','leather','plant_fibre','metal','metal']


def append(m,source,fn=lambda p:p,slot=lambda k:k):
    off=len(m.vertices);m.vertices +=[fn(p) for p in source.vertices]
    m.faces +=[(a+off,b+off,c+off,slot(col)) for a,b,c,col in source.faces]


def bottom(angle):
    s=math.sin(angle)
    if s>=0:
        t=min(1,s/.68);t=t*t*(3-2*t)
        return .082+.120*t
    return .082+.085*abs(s)**1.8


def hat(female=False):
    m=base.Model('TaigaFurHat');n=32;sy=fp.head_sections(female)[-1][0]/.270
    skins=[]
    # The scalloped lower boundary is part of the crown shell: no added ear slabs.
    for inner in (False,True):
        rings=[]
        for row in range(6):
            ring=[]
            for i in range(n):
                a=i*math.tau/n;s=math.sin(a);low=bottom(a)
                if row==0:y=low;rx=.114;rz=.125 if s>0 else .142
                elif row==1:y=low+.009;rx=.116;rz=.128 if s>0 else .144
                elif row==2:y=.229;rx=.113;rz=.128 if s>0 else .141
                elif row==3:y=.292;rx=.099;rz=.108 if s>0 else .121
                elif row==4:y=.319;rx=.067;rz=.073 if s>0 else .086
                else:y=.331;rx=.027;rz=.030 if s>0 else .040
                if inner:rx-=.009;rz-=.009;y-=.003
                ring.append(m.v((rx*math.cos(a),y*sy,rz*s)))
            rings.append(ring)
        for row in range(5):
            for i in range(n):
                j=(i+1)%n;slot=3 if inner else (1 if (i+row*3)%7==0 else 0)
                m.tri(rings[row][i],rings[row][j],rings[row+1][j],slot)
                m.tri(rings[row][i],rings[row+1][j],rings[row+1][i],slot)
        pole=m.v((0,(.335-(.008 if inner else 0))*sy,-.004))
        for i in range(n):m.tri(pole,rings[-1][i],rings[-1][(i+1)%n],3 if inner else 0)
        skins.append(rings)
    for i in range(n):
        j=(i+1)%n;m.tri(skins[0][0][i],skins[0][0][j],skins[1][0][j],3)
        m.tri(skins[0][0][i],skins[1][0][j],skins[1][0][i],3)
    # Cattail ties emerge from the actual lower side panels, hang clear of the jaw.
    for sign in (-1,1):
        tube(m,[(sign*.113,.088*sy,.0),(sign*.112,.065*sy,.010),
                (sign*.103,.045*sy,.021),(sign*.105,.020*sy,.024)],.002,7,6)
    # Subtle cloth seam joins the pelt panels, seated on the crown meridian.
    tube(m,[(0,.230*sy,.129),(0,.290*sy,.110),(0,.318*sy,.075),(0,.332*sy,.032)],.0010,3,5)
    return m


def boot():
    m=base.Model('TaigaBoot')
    # Accepted continuous ankle/foot construction is reused read-only, with a taller shaft.
    old=previous.boot()
    remap={0:4,1:5,2:4,3:6,4:4,5:5,6:6,7:5}
    append(m,old,lambda p:(p[0],p[1]*(1.55 if p[1]>0 else 1),p[2]),lambda s:remap[s])
    support=base.Model('TinToeSupport');support.vertices=[(x,z,-y) for x,y,z in m.vertices];support.faces=list(m.faces)
    # Grid follows the actual tapered vamp in both axes, avoiding a floating flat chord.
    rows=[];under=[];nx=6
    for z,w in [(.130,.048),(.150,.047),(.175,.042),(.195,.030)]:
        top=[];low=[]
        for j in range(nx+1):
            x=w*(2*j/nx-1);p=kit.front_surface(support,x,z,.0025);y=-p[2]
            top.append(m.v((x,y,z)));low.append(m.v((x,y-.0035,z)))
        rows.append(top);under.append(low)
    for r in range(len(rows)-1):
        for j in range(nx):
            for grid,col in [(rows,8),(under,9)]:
                a,b=grid[r],grid[r+1];m.tri(a[j],a[j+1],b[j+1],col);m.tri(a[j],b[j+1],b[j],col)
    boundary=[(0,j) for j in range(nx+1)]+[(r,nx) for r in range(1,len(rows))]+[(len(rows)-1,j) for j in range(nx-1,-1,-1)]+[(r,0) for r in range(len(rows)-2,0,-1)]
    for (r,j),(s,k) in zip(boundary,boundary[1:]+boundary[:1]):
        a,b,c,d=rows[r][j],rows[s][k],under[s][k],under[r][j]
        m.tri(a,b,c,9);m.tri(a,c,d,9)
    return m


def pair(female=False):
    m=base.Model('TaigaBootPair');one=boot()
    for sign in (-1,1):append(m,one,lambda p:(p[0]+sign*fp.HIP_X,p[1]+fp.ANKLE_HEIGHT,p[2]))
    return m


def display(part):
    m=hat() if part=='hat' else boot()
    m.vertices=[(x*3,y*3,-z*3) if part=='hat' else (z*2.8,y*2.8,-x*2.8) for x,y,z in m.vertices]
    mid=[(min(v[i] for v in m.vertices)+max(v[i] for v in m.vertices))/2 for i in range(3)]
    m.vertices=[tuple(v[i]-mid[i] for i in range(3)) for v in m.vertices]
    return m


SPECS=[('TaigaFurHat','Taiga fur hat',lambda:display('hat'),COLORS,TYPES),('TaigaBoots','Taiga boots',lambda:display('boot'),COLORS,TYPES)]
META={}
for female in (False,True):
    body='Female' if female else 'Male'
    for name,label,fn,anchor in [('TaigaFurHat','Taiga fur hat',lambda female=female:hat(female),'HeadAnchor local; (0,1.43,0)'),
                                  ('TaigaBoots','Taiga boots',lambda female=female:pair(female),'BodyRoot; split ankle/foot ownership during integration')]:
        key=body+'Wear'+name;SPECS.append((key,label+' ('+body+' fitted)',fn,COLORS,TYPES))
        META[key]={'Representation':'worn','Body':body,'Anchor':anchor}
for name,*_ in SPECS:
    item='taiga_fur_hat' if 'Hat' in name else 'taiga_boots'
    META.setdefault(name,{'Representation':'display'})
    META[name].update(ItemId='item.'+item,RecipeId='recipe.wearable.'+item,BindingStatus='Passive art; live binding, weights and dynamic validation pending')
RECIPES={name:('T2-G20',12,'1 Fine beaver pelt + 1 Fine flax cloth + 1 Cattail cord -> 1') if 'Hat' in name else
         ('T2-L15',12,'2 Fine beaver leather + 1 Tin ingot -> 1') for name,*_ in SPECS}

if __name__=='__main__':
    generate(16,SPECS,RECIPES,category='Wearables',role='Wearable',preview='--preview' in sys.argv,item_metadata=META)
    (ROOT/'Assets/_Game/Art/Crafting/Tier02/Tier02Batch16ArtMap.json').write_text(json.dumps({'Scope':'Passive Taiga hat/boots; no gameplay changes',
        'HeadAnchor':[0,1.43,0],'BootAnkles':[[x,.10,0] for x in [-.10,.10]],'Items':META,
        'HairReuse':'Existing Tier01Accessories {Body}HatHair{Crop,Sweep,Bob,Tied}; preserve saved style/palette',
        'Pending':'Live binding, per-foot ownership/weights, animation, current save/device tests'},indent=2)+'\n')
