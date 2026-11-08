"""Twenty listed balloon special items, avatar fits and four-tier hinged mystery box art."""
from pathlib import Path
import math,json
import generate_region01_meshes as b
import natural_character_geometry as n
from generate_tier01_crafted_items import boot_profile

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

def hat(tier,female=False):
    m=b.Model('Hat');sy=.96 if female else 1
    if tier==3:
        n.surface(m,[(.168,0,-.006,.102,.102),(.19,0,-.008,.107,.107),(.24,0,-.005,.098,.098),(.275,0,-.006,.067,.070),(.293,0,-.006,.008,.009)],24,0)
        for y in (.18,.205,.232,.255):loop(m,(0,y,-.004),.104-(y-.18)*.3,.105-(y-.18)*.3,.007,1,'xz',24)
        # Side veil stays behind the cheeks, leaving the face and vision clear.
        for side in (-1,1):box(m,(side*.098,.093,-.037),(.024,.19,.115),0)
        box(m,(0,.191,.099),(.18,.023,.014),4);badge(m,(.067,.189,.113),.016)
    else:
        brim=.22 if tier in (1,4) else .20
        n.surface(m,[(.179,0,-.009,brim,.172),(.191,0,-.009,brim,.172),(.198,0,-.01,.106,.102)],32,0 if tier==1 else 2 if tier==2 else 10)
        n.surface(m,[(.193,0,-.009,.111,.105),(.228,0,-.013,.112,.102),(.277,0,-.019,.090,.081),(.296,0,-.021,.055,.051),(.301,0,-.021,.008,.008)],24,0 if tier!=4 else 10)
        n.surface(m,[(.198,0,-.009,.114,.108),(.224,0,-.012,.114,.107)],24,4)
        badge(m,(.072,.216,.087),.017)
        if tier in (1,2):
            tube(m,[(-.086,.218,0),(-.13,.33,-.015),(-.15,.42,-.034)],.004,6)
            feather=b.Model('Plume');n.ellipse(feather,(-.132,.34,-.018),(.021,.087,.009),13,12,7);add(m,feather,angle=-12)
        if tier==4:
            for i in range(8):
                a=i*math.tau/8;n.ellipse(m,(.117*math.cos(a),.212,.11*math.sin(a)-.008),(.014,.012,.009),10,8,4)
            for r in (.14,.17,.20):loop(m,(0,.193,-.009),r,r*.78,.0025,8,'xz',32)
    if tier==2:
        m.vertices=[(x,y-.025 if z<-.08 and .175<y<.20 else y,z*1.35 if z<-.08 and .175<y<.20 else z) for x,y,z in m.vertices]
    if female:m.vertices=[(x*.96,y*sy,z*.98) for x,y,z in m.vertices]
    return m

def shirt(tier,female):
    m=b.Model('Shirt');radius=.213 if female else .232;bottom=-.19 if tier==3 else -.15
    n.surface(m,[(bottom,0,0,.246 if tier==3 else .223,.175 if tier==3 else .152),(-.02,0,0,.23 if tier==3 else .206,.16 if tier==3 else .143),(.09,0,0,.166 if female else .182,.129),(.24,0,0,radius,.144),(.34,0,0,radius+.007,.135),(.385,0,0,.18,.108),(.418,0,0,.077,.070),(.433,0,0,.070,.066)],28,0)
    for side in (-1,1):
        length=.50 if tier in (3,4) else .29
        sleeve=n.limb('Sleeve',[(-length,.044,.041,0),(-.24,.063,.059,0),(-.10,.080,.075,0),(.026,.080,.076,0)],0)
        add(m,sleeve,(side*(.225 if female else .25),.345,0),side*12)
        cap=b.Model('Cap');n.ellipse(cap,(0,0,0),(.088,.081,.077),0,18,8);add(m,cap,(side*(.20 if female else .225),.34,0))
    count=[2,3,5,8][tier-1]
    # Hidden-pocket robe uses narrow outlined openings; other styles show flap pockets.
    for i in range(count):
        cols=2 if count<5 else 4;x=(i%cols-(cols-1)/2)*(.115 if cols==4 else .19);y=.22-(i//cols)*.17
        if tier==3:
            box(m,(x,y,.149),(.075,.018,.012),1);box(m,(x+.025,y-.025,.16),(.012,.04,.012),6)
        else:
            box(m,(x,y-.035,.145),(.083 if cols==4 else .14,.11,.024),0)
            box(m,(x,y+.018,.163),(.089 if cols==4 else .15,.035,.015),1)
            n.ellipse(m,(x,y+.009,.176),(.009,.009,.004),6,8,4)
            if tier==2:
                tube(m,[(x-.025,y+.02,.18),(x+.025,y+.02,.18)],.007,4)
                n.ellipse(m,(x+.025,y+.02,.18),(.007,.007,.008),6,8,4)
    # Collar piping and merchant signature on every garment.
    loop(m,(0,.423,0),.079,.072,.008,4,'xz',24)
    if tier==3:box(m,(0,-.015,.153),(.37,.045,.02),4)
    if tier==4:
        tube(m,[(-.06,.40,.077),(0,.31,.158),(.06,.40,.077)],.017,4)
        for x in (-.11,.11):box(m,(x,.38,.11),(.055,.04,.015),1)
    badge(m,(.15,.31,.147),.016)
    if tier==1:m.faces=[(i,j,k,1 if c==0 else c) for i,j,k,c in m.faces]
    return m

def pants(tier,female):
    m=b.Model('Pants');col=0 if tier==1 else 1
    n.surface(m,[(-.10,0,0,.149,.121),(-.04,0,0,.182 if female else .174,.136),(.06,0,0,.178,.126)],24,col)
    for side in (-1,1):
        thigh=n.limb('Thigh',[(-.375,.075,.076,0),(-.28,.083,.085,0),(-.10,.102 if tier!=3 else .117,.106,0),(.025,.09,.09,0)],col)
        shin=n.limb('Shin',[(-.435,.052,.051,0),(-.35,.055,.058,0),(-.19,.070,.075,0),(.02,.074,.071,0)],col)
        add(m,thigh,(side*.115,0,0));add(m,shin,(side*.115,-.36,0))
        if tier==1:box(m,(side*.115,-.34,.079),(.12,.15,.025),2)
        if tier==2:
            for y in (-.65,-.53):box(m,(side*.115,y,.066),(.10,.095,.022),3)
        if tier in (3,4):
            for y in (-.62,-.66,-.70):loop(m,(side*.115,y,0),.062,.063,.008,0 if tier==3 else 8,'xz',18)
    n.surface(m,[(.021,0,0,.186,.14),(.06,0,0,.186,.14)],24,2 if tier!=4 else 8)
    count=[2,3,5,8][tier-1]
    for i in range(count):
        x=(i-(count-1)/2)*(.045 if count>=5 else .11)
        loop(m,(x,-.001,.154),.014,.020,.004,6)
    box(m,(0,.04,.145),(.045,.035,.012),4);badge(m,(-.146,.025,.13),.013)
    return m

def boots(tier):
    m=b.Model('Boots')
    for side in (-1,1):
        foot=n.foot('Boot',False);foot.vertices=[(x*1.06,y,z*1.03) for x,y,z in foot.vertices]
        foot.faces=[(i,j,k,9 if c==15 else 1 if tier==4 else 2) for i,j,k,c in foot.faces]
        add(m,foot,(side*.115,.10,0))
        height=.32 if tier==2 else .17 if tier==3 else .13
        shaft=[(.11,side*.115,0,.060,.058),(.19,side*.115,-.004,.086,.086),(.10+height,side*.115,-.008,.095,.095)] if tier==2 else [(.11,side*.115,0,.060,.058),(.10+height,side*.115,-.008,.063,.060)]
        n.surface(m,shaft,20,1 if tier==4 else 2)
        cuff=.10+height
        n.surface(m,[(cuff-.025,side*.115,-.008,.102 if tier==2 else .068,.102 if tier==2 else .065),(cuff+.013,side*.115,-.008,.102 if tier==2 else .068,.102 if tier==2 else .065)],20,8 if tier==2 else 4)
        for y in (.13,.16,.19):tube(m,[(side*.115-.035,y,.064),(side*.115+.035,y+.014,.064)],.004,4)
        if tier==1:box(m,(side*.115,.037,.196),(.104,.05,.040),6)
        if tier==2:box(m,(side*.115+.096,.28,.018),(.024,.045,.065),6)
        if tier==2:box(m,(side*.115,cuff+.028,-.096),(.025,.07,.015),4)
        if tier==3:
            for y in (.13,.17,.21,.25):loop(m,(side*.115,y,0),.064,.060,.007,1,'xz',18)
        badge(m,(side*.115,cuff-.009,.068),.011)
    return m

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

def pack(tier):
    m=b.Model('Pack')
    if tier==4:
        box(m,(0,.04,-.16),(.49,.43,.28),11)
        for x in (-.21,-.07,.07,.21):box(m,(x,.04,-.308),(.012,.39,.016),12)
        box(m,(0,.28,-.16),(.51,.075,.30),4)
        for x in (-.23,.23):
            for y in (-.15,.23):box(m,(x,y,-.31),(.075,.07,.025),6)
        box(m,(0,.16,-.314),(.065,.10,.025),6)
    else:
        n.surface(m,[(-.24,0,-.16,.16,.10),(-.20,0,-.16,.225,.14),(.17,0,-.16,.23,.145),(.29,0,-.16,.18,.10)],24,0 if tier!=3 else 2)
        box(m,(0,.19,-.302),(.41,.15,.045),4)
        for x in (-.145,.145):
            box(m,(x,-.01,-.322),(.035,.30,.027),2);box(m,(x,.045,-.343),(.061,.055,.018),6)
        if tier==1:
            tube(m,[(-.24,.31,-.15),(.24,.31,-.15)],.079,1,14)
            for x in (-.15,.15):loop(m,(x,.31,-.15),.085,.085,.011,4,plane='xy',steps=16)
        if tier==2:
            for x in (-.24,.24):tube(m,[(x,-.25,-.07),(x,.31,-.07)],.014,11)
            box(m,(0,.28,-.16),(.47,.065,.29),8)
            for i in range(8):n.ellipse(m,(-.205+i*.058,.248,-.313),(.036,.029,.017),8,8,4)
        if tier==3:
            for x in (-.25,.25):
                box(m,(x,-.02,-.17),(.16,.35,.25),2);box(m,(x,.14,-.19),(.18,.055,.25),4)
                for y in (-.15,.08):box(m,(x,y,-.31),(.10,.05,.02),6)
                tube(m,[(x,-.18,-.30),(x,-.30,-.30)],.012,4)
            for x in (-.10,0,.10):tube(m,[(x-.035,-.05,-.308),(x,.0,-.312),(x+.035,-.05,-.308),(x,-.10,-.312),(x-.035,-.05,-.308)],.003,3)
    # Straps arc around the shoulder area, while the pack shell stays behind the torso.
    for x in (-.16,.16):tube(m,[(x,.23,-.03),(x,.28,.13),(x,.12,.36),(x,-.15,.34),(x,-.21,-.02)],.018,2)
    badge(m,(0,.10,-.345),.026)
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
            anchor={'hat':'HeadAnchor','shirt':'ShirtAnchor','pants':'PantsAnchor','pack':'BackpackAnchor','boots':'Root'}[slot]
            fits={}
            for female,sex in [(False,'Male'),(True,'Female')]:
                mesh={'hat':lambda:hat(tier,female),'shirt':lambda:shirt(tier,female),'pants':lambda:pants(tier,female),'boots':lambda:boots(tier),'pack':lambda:pack(tier)}[slot]()
                mesh.name=prefix+sex+'Worn';save(mesh,tier);fits[sex]=mesh.name
                if not female:
                    display=make_item(item_boot(tier) if slot=='boots' else mesh,prefix+'Item',slot)
                    save(display,tier)
            ITEMS.append({'Id':f'special.t{tier}.{slot}','Name':NAMES[tier-1][index],'Tier':tier,'Slot':slot,'ItemMesh':prefix+'Item','MaleMesh':fits['Male'],'FemaleMesh':fits['Female'],'Anchor':anchor,'InfiniteDurability':True})
    mystery();clipped_hair()
    data={'Revision':1,'Parts':STATS,'Items':ITEMS,'BoxTiers':[{'Tier':i,'FirstRegion':(i-1)*6+1,'LastRegion':i*6,'Palette':f'MysteryBoxTier{i:02}Palette','Color':['Olive and brass','Deep teal and brass','Amber and crimson','Navy and brass'][i-1]} for i in range(1,5)],'BoxLidPivot':[0,.39,.30],
          'ThemeSource':'PROJECT_RECOMENDATIONS.md section 14.1','Scope':'Art and animation only; no purchases, item stats, loot rolling or shop population.'}
    (OUT/'BalloonGoodsManifest.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    print(f'{len(ITEMS)} special item designs; {len(STATS)} source parts; four mystery-box palette tiers; eight hat-compatible hair meshes.')

if __name__=='__main__':main()
