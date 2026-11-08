"""Twenty listed balloon special items, avatar fits and four-tier hinged mystery box art.

Worn fits: special_gear_fitted.py (faceted bodies, November 7, 2026); this file keeps the item displays, the boot
item profile, the mystery box and the hat-compatible hair.
"""
from pathlib import Path
import math,json
import generate_region01_meshes as b
import natural_character_geometry as n
from generate_tier01_crafted_items import boot_profile
import special_gear_fitted as fitted

ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'Assets/_Game/Art/Items/BalloonGoods'
# Material slots: main textile, secondary textile, leather, dark leather, crimson,
# dark crimson, brass, gold highlight, lining, sole, shell, wood, dark wood, feather, white, dark.
PALETTES=[
 [(153,158,91),(223,205,157),(107,70,40),(67,43,28),(170,36,42),(112,24,34),(195,146,62),(240,200,106),(195,180,133),(48,42,32),(235,216,175),(137,88,46),(77,47,29),(133,94,58),(245,235,205),(31,31,27)],
 [(41,102,104),(84,110,121),(84,67,49),(49,46,38),(170,36,42),(112,24,34),(195,146,62),(240,200,106),(211,205,181),(34,44,45),(220,214,193),(115,91,64),(66,52,38),(170,184,186),(242,234,204),(27,34,35)],
 [(230,210,166),(176,103,66),(154,104,56),(99,64,39),(170,36,42),(112,24,34),(195,146,62),(240,200,106),(212,190,144),(78,56,34),(240,220,171),(161,106,53),(94,60,31),(226,209,174),(250,239,209),(41,33,25)],
 [(234,228,199),(39,62,83),(98,70,42),(57,46,34),(170,36,42),(112,24,34),(195,146,62),(240,200,106),(204,184,131),(29,37,40),(243,223,184),(151,100,53),(83,53,30),(204,218,214),(251,243,217),(25,34,41)]]
NAMES=[
 ["Wayfarer's felt hat","Wayfarer's trail boots","Wayfarer's waxed shirt","Wayfarer's field trousers","Wayfarer's ridge pack"],
 ["Heron-plume rain hat","Marsh-strider boots","Lakeshore oilskin shirt","Boardwalk trousers","Taiga fur-trimmed pack"],
 ["Dune wrap","Canyon sand boots","Nomad linen robe","Caravan wrap trousers","Caravan saddle pack"],
 ["Coral-shell sun hat","Tidewalker boots","Mariner's sailcloth shirt","Reef-runner trousers","Estuary sea-chest pack"]]
SLOTS=['hat','boots','shirt','pants','pack'];STATS=[];ITEMS=[]

def box(m,c,size,k):
    x,y,z=c;a,d,e=[v/2 for v in size]
    v=[m.v((x+sx*a,y+sy*d,z+sz*e)) for sx,sy,sz in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
    for q in [(0,3,2,1),(4,5,6,7),(0,4,7,3),(1,2,6,5),(3,7,6,2),(0,1,5,4)]:
        a,c,d,e=[v[i] for i in q];m.tri(a,c,d,k);m.tri(a,d,e,k)

def tube(m,pts,r,k,sides=6):
    for a,c in zip(pts,pts[1:]):
        before=len(m.faces);m.cylinder(a,c,r,r,sides,k,k);m.faces[before:]=[(i,j,l,k) for i,j,l,_ in m.faces[before:]]

def loop(m,c,rx,ry,r,k,plane='xy',steps=12):
    tube(m,[(c[0]+rx*math.cos(i*math.tau/steps),c[1]+(ry*math.sin(i*math.tau/steps) if plane=='xy' else 0),c[2]+(ry*math.sin(i*math.tau/steps) if plane=='xz' else 0)) for i in range(steps+1)],r,k)

def badge(m,c,s=.025):
    # Tiny brass balloon, deliberately a silhouette rather than unreadable lettering.
    n.ellipse(m,c,(s,s*1.22,s*.18),6,10,6)
    x,y,z=c;box(m,(x,y-s*1.63,z),(s*.9,s*.55,s*.3),6)
    for side in (-1,1):tube(m,[(x+side*s*.65,y-s*.85,z),(x+side*s*.35,y-s*1.5,z)],s*.09,6)

def add(m,part,offset=(0,0,0),angle=0,force=None):
    base=len(m.vertices);a=math.radians(angle)
    for x,y,z in part.vertices:m.v((x*math.cos(a)-y*math.sin(a)+offset[0],x*math.sin(a)+y*math.cos(a)+offset[1],z+offset[2]))
    for i,j,k,c in part.faces:m.tri(base+i,base+j,base+k,c if force is None else force)

def save(m,tier=0):
    assert all(all(math.isfinite(c) for c in p) for p in m.vertices),m.name
    assert all(len({i,j,k})==3 and all(1<=v<=len(m.vertices) for v in (i,j,k)) for i,j,k,_ in m.faces),m.name
    m.save();STATS.append({'Name':m.name,'Tier':tier,'Triangles':len(m.faces)})

def as_model(km,name):
    """A faceted_character_kit mesh (face colours are palette slots) as this pipeline's Model (1-based faces)."""
    m=b.Model(name);m.vertices=list(km.vertices);m.faces=[(i+1,j+1,l+1,c) for i,j,l,c in km.faces];return m

def item_boot(tier):
    m=b.Model('BootItem');height=[.22,.56,.30,.22][tier-1]
    boot_profile(m,[(-.32,-.18),(.33,-.18),(.39,-.12),(.35,-.045),(.08,.015),(-.01,.11),(-.04,height),(-.30,height)],.23,1 if tier==4 else 2,.05)
    boot_profile(m,[(-.34,-.25),(.34,-.25),(.39,-.19),(.38,-.165),(-.33,-.165)],.25,9,.02)
    boot_profile(m,[(-.34,-.28),(-.16,-.28),(-.15,-.23),(-.34,-.23)],.25,9,.02)
    box(m,(-.17,height-.015,0),(.30,.07,.25),8 if tier==2 else 4)
    if tier==1:boot_profile(m,[(.20,-.15),(.37,-.14),(.36,-.065),(.20,-.04)],.245,6,.03)
    if tier==2:box(m,(-.26,.27,-.132),(.065,.09,.025),6)
    if tier==2:
        box(m,(-.30,height+.035,-.02),(.028,.10,.035),4)
        for i in range(7):
            x=-.31+i*.045
            ids=[m.v((x,height-.048,-.129)),m.v((x+.042,height-.048,-.129)),m.v((x+.021,height-.071,-.129))]
            m.tri(*reversed(ids),8)
    for j in range(4 if tier!=2 else 6):
        y=.025+j*(height-.08)/(4 if tier!=2 else 6)
        tube(m,[(-.10,y,-.132),(-.015,y+.021,-.132)],.008,4)
        n.ellipse(m,(-.105,y,-.135),(.012,.012,.005),6,8,4)
    if tier==3:
        for y in (.09,.16,.23,.30):box(m,(-.17,y,-.125),(.27,.018,.02),1)
    badge(m,(-.225,height-.11,-.14),.027)
    return m

def make_item(source,name,slot):
    m=b.Model(name)
    # Hats/shirts/pants show the front (+Z on the avatar) to the item camera at -Z.
    # Packs show their decorated rear; boots have their own authored side-profile item.
    m.vertices=[((-x if slot in ('hat','shirt','pants') else x),y,(-z if slot in ('hat','shirt','pants') else z)) for x,y,z in source.vertices]
    m.faces=list(source.faces)
    lo=[min(p[i] for p in m.vertices) for i in range(3)];hi=[max(p[i] for p in m.vertices) for i in range(3)]
    scale=.85/max(hi[i]-lo[i] for i in range(2))
    m.vertices=[tuple((p[i]-(hi[i]+lo[i])/2)*scale for i in range(3)) for p in m.vertices]
    return m

def mystery():
    body=b.Model('MysteryBoxBody')
    box(body,(0,.035,0),(.88,.07,.62),12)
    # Hollow box: independent walls and a recessed lining, no solid block hidden under lid.
    for x in (-.405,.405):box(body,(x,.22,0),(.07,.33,.60),0)
    for z in (-.275,.275):box(body,(0,.22,z),(.75,.33,.07),0)
    box(body,(0,.079,0),(.73,.025,.46),5)
    for x in (-.425,.425):
        for z in (-.29,.29):box(body,(x,.22,z),(.045,.35,.045),6)
    for y in (.075,.365):
        box(body,(0,y,-.315),(.87,.027,.03),6);box(body,(0,y,.315),(.87,.027,.03),6)
        for x in (-.445,.445):box(body,(x,y,0),(.025,.027,.62),6)
    for x in (-.26,.26):box(body,(x,.215,-.313),(.045,.28,.025),4)
    box(body,(0,.255,-.325),(.11,.14,.035),6)
    n.ellipse(body,(0,.27,-.348),(.018,.024,.004),15,10,5)
    badge(body,(.23,.22,-.33),.045)
    # Back hinge knuckles are aligned to the lid pivot Y=.39, Z=.30.
    for x in (-.26,.26):tube(body,[(x-.065,.39,.30),(x+.065,.39,.30)],.025,6,10)
    save(body)
    lid=b.Model('MysteryBoxLid')
    box(lid,(0,.035,-.30),(.88,.07,.62),0)
    box(lid,(0,-.006,-.30),(.73,.018,.46),5)
    # Pagoda-inspired raised lid panels, shallow enough to read as a hinged coffer.
    for j,(width,depth,y) in enumerate([(.92,.65,.075),(.78,.54,.12),(.56,.38,.155)]):
        box(lid,(0,y,-.30),(width,.045,depth),0 if j!=1 else 4)
    for x in (-.27,.27):box(lid,(x,.159,-.30),(.042,.025,.55),6)
    for z in (-.61,.01):box(lid,(0,.075,z),(.94,.025,.025),6)
    for x in (-.46,.46):box(lid,(x,.075,-.30),(.025,.025,.62),6)
    box(lid,(0,.01,-.635),(.12,.11,.025),6)
    save(lid)
    for tier in range(1,5):
        seal=b.Model('MysteryTier'+str(tier)+'Seal')
        for i in range(tier):n.ellipse(seal,((i-(tier-1)/2)*.045,.125,-.336),(.014,.014,.004),7,8,4)
        save(seal,tier)

def clipped_hair():
    for sex in ('Male','Female'):
        for style in ('Crop','Sweep','Bob','Tied'):
            source=ROOT/f'Assets/_Game/Art/Characters/PlayerDefault/{sex}Hair{style}.obj'
            vs=[];fs=[]
            for line in source.read_text().splitlines():
                if line.startswith('v '):vs.append(tuple(map(float,line.split()[1:])))
                elif line.startswith('f '):fs.append([int(x)-1 for x in line.split()[1:]])
            mesh=b.Model(sex+'HatHair'+style)
            for face in fs:
                poly=[vs[i] for i in face];out=[]
                for a,c in zip(poly,poly[1:]+poly[:1]):
                    inside=a[1]<=.178;other=c[1]<=.178
                    if inside:out.append(a)
                    if inside!=other:
                        t=(.178-a[1])/(c[1]-a[1]);out.append(tuple(a[k]+(c[k]-a[k])*t for k in range(3)))
                if len(out)>=3:
                    ids=[mesh.v(p) for p in out]
                    for i in range(1,len(ids)-1):mesh.tri(ids[0],ids[i],ids[i+1],15)
            save(mesh)

def main():
    OUT.mkdir(parents=True,exist_ok=True);b.OUT=OUT;b.COLORS=PALETTES[0]
    for tier in range(1,5):
        b.COLORS=PALETTES[tier-1];b.palette(f'BalloonGoodsTier{tier:02}Palette.png')
        boxcolors=list(b.COLORS);boxcolors[0]=[(112,134,64),(38,102,104),(190,126,57),(39,62,83)][tier-1]
        b.COLORS=boxcolors;b.palette(f'MysteryBoxTier{tier:02}Palette.png');b.COLORS=PALETTES[tier-1]
        for index,slot in enumerate(SLOTS):
            prefix=f'SpecialT{tier:02}{slot.title()}'
            # Faceted body frames (November 7, 2026): hats at the head origin, shirts/pants/boots in body-root space.
            anchor={'hat':'HeadAnchor/HatAnchor','shirt':'Root','pants':'Root','pack':'BackpackAnchor','boots':'Root'}[slot]
            fits={}
            for female,sex in [(False,'Male'),(True,'Female')]:
                build={'hat':fitted.hat,'shirt':fitted.shirt,'pants':fitted.pants,'boots':fitted.boots,'pack':fitted.pack}[slot]
                mesh=as_model(build(tier,female),prefix+sex+'Worn');save(mesh,tier);fits[sex]=mesh.name
                if not female:
                    display=make_item(item_boot(tier) if slot=='boots' else mesh,prefix+'Item',slot)
                    save(display,tier)
            ITEMS.append({'Id':f'special.t{tier}.{slot}','Name':NAMES[tier-1][index],'Tier':tier,'Slot':slot,'ItemMesh':prefix+'Item','MaleMesh':fits['Male'],'FemaleMesh':fits['Female'],'Anchor':anchor,'InfiniteDurability':True})
    mystery();clipped_hair()
    data={'Revision':2,'WornFit':'faceted bodies (special_gear_fitted.py)','Parts':STATS,'Items':ITEMS,'BoxTiers':[{'Tier':i,'FirstRegion':(i-1)*6+1,'LastRegion':i*6,'Palette':f'MysteryBoxTier{i:02}Palette','Color':['Olive and brass','Deep teal and brass','Amber and crimson','Navy and brass'][i-1]} for i in range(1,5)],'BoxLidPivot':[0,.39,.30],
          'ThemeSource':'PROJECT_RECOMENDATIONS.md section 14.1','Scope':'Art and animation only; no purchases, item stats, loot rolling or shop population.'}
    (OUT/'BalloonGoodsManifest.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    print(f'{len(ITEMS)} special item designs; {len(STATS)} source parts; four mystery-box palette tiers; eight hat-compatible hair meshes.')

if __name__=='__main__':main()
