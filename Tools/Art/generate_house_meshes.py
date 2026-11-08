"""Eight matched house exteriors/interiors and separate furnishing studies.
Run with Python 3; standard library only. Does not modify scenes or gameplay.
Depth growth (user, November 7, 2026): single-storey houses that get LONGER toward the back with each level; the
interior is a floor seen from the front at an angle, its furnishing grid running width x depth (row 0 at the front).
"""
from pathlib import Path
import json
import faceted_character_kit as k
import house_geometry as g
from house_specs import LEVELS, FURNITURE, PALETTE, GRID_METRES, PLOT_WIDTH, PLOT_DEPTH

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Art/Settlement/Houses'
HEADER='The Trail Walker house art; metres; +Y up; +Z toward street camera; no gameplay.'
pal=k.Palette()
for spec in PALETTE: pal.add(*spec)


def exterior(s):
    level=s['Level']; w=s['Width']; depth=s['Depth']-.8; e=s['Eaves']; peak=s['Height']
    wood=s['Wood']; m=k.Mesh(f'HouseL{level:02}Front',pal); half=w/2-.25; rear=.18-depth
    stone='Brick' if level<3 else 'Stone' if level<5 else 'Granite' if level<7 else 'Basalt'
    if level==1:
        for x in (-half+.2,half-.2):
            for z in (-.1,rear+.2): g.box(m,(x,.14,z),(.5,.28,.5),stone)
    else:
        g.box(m,(0,.17,rear/2),(w-.3,.34,depth-.02),stone)
        g.box(m,(0,.35,rear/2),(w-.24,.10,depth+.06),wood)
    door_h=1.65 if level==1 else 1.92
    win_w,win_h=(.76,.85) if level<3 else (.88,1.02)
    windows=[(half*.60,1.20,.63,.64)] if level==1 else [(x,1.38,win_w,win_h) for x in (-half*.58,half*.58)]
    # Side windows along both long walls: one per ~1.7 m of depth, so longer houses read as longer.
    side_z=g.side_window_positions(depth,level)
    openings=[(-.57,.57,.33,.33+door_h+.12)]
    openings.extend((x-ww/2,x+ww/2,y-hh/2,y+hh/2) for x,y,ww,hh in windows)
    g.wall(m,-half,half,.32,e,-.08,.16,openings,s['Wall'])
    backopen=[(x-ww/2,x+ww/2,y-hh/2,y+hh/2) for x,y,ww,hh in windows]
    g.wall(m,-half,half,.32,e,rear+.14,.16,backopen,s['Wall'])
    z0,z1=rear+.06,0.0
    for sign in (-1,1):
        lo,hi=sorted((g.along(sign,z0),g.along(sign,z1)))
        holes=[(min(g.along(sign,z-win_w/2),g.along(sign,z+win_w/2)),max(g.along(sign,z-win_w/2),g.along(sign,z+win_w/2)),1.38-win_h/2,1.38+win_h/2) for z in side_z]
        g.side(m,sign,half,lambda mm,lo=lo,hi=hi,holes=holes:g.wall(mm,lo,hi,.32,e,0,.16,holes,s['Wall']))
        for z in side_z:
            g.side(m,sign,half,lambda mm,z=z,sign=sign:g.window(mm,g.along(sign,z),1.38,win_w,win_h,.135,'WoodDark' if level<3 else wood,level>=3,level==8))
    # Narrow battens on sheds, articulated timber bays on finished homes.
    if level<3:
        for side in (-1,1):
            for j in range(10):
                zz=rear+.3+(depth-.5)*(j+.5)/10
                if all(abs(zz-z)>win_w/2+.12 for z in side_z): g.box(m,(side*(half+.085),1.17,zz),(.025,1.7,.05),wood,shade=1+j%3)
        for x in [(-half+.12)+i*(2*half-.24)/12 for i in range(13)]:
            if abs(x)>.65: g.box(m,(x,(e+.33)/2,.018),(.035,e-.33,.028),wood,shade=1)
    for sign in (-1,1):
        for z in (.035,rear+.12): g.box(m,(sign*(half-.05),(e+.32)/2,z),(.19,e-.32,.18),wood)
        bays=len(side_z); posts=[-depth*i/bays for i in range(1,bays)] if bays>=2 else [rear*.30,rear*.66]
        for z in posts: g.box(m,(sign*(half+.08),(e+.32)/2,z),(.16,e-.32,.18),wood)
    for y in (.43,e-.10): g.box(m,(0,y,.035),(2*half,.16,.18),wood)
    if level>=3:
        for side in (-1,1):
            x=side*(half-.55)
            g.beam(m,(x-side*.35,.56,.06),(x+side*.28,1.03,.06),.13,wood)
            if level>=5: g.beam(m,(x-side*.4,e-.67,.06),(x+side*.28,e-.17,.06),.12,wood)
    g.gable(m,half,e,peak,-.08,s['Wall'])
    g.gable(m,half,e,peak,rear+.14,s['Wall'])
    g.roof(m,w,depth+.18,e,peak,.30,s['Roof'],wood)
    # Visible gable timber, with a restrained fan rather than flat blank infill.
    g.beam(m,(0,e,.06),(0,peak-.12,.06),.13,wood)
    for sign in (-1,1):
        g.beam(m,(sign*(half-.1),e+.02,.06),(0,peak-.12,.06),.11,wood)
    for x,y,ww,hh in windows:
        g.window(m,x,y,ww,hh,.055,'WoodDark' if level<3 else wood,level>=3,level==8)
        # The rear openings match the cutaway's glazing family.
        start=len(m.vertices);g.window(m,x,y,ww,hh,0,wood,False,level==8)
        m.transform(lambda p:(p[0],p[1],rear+.04-p[2]),start)
    g.door(m,0,.34,.07,'WoodDark' if level<3 else wood,level,door_h)
    g.box(m,(0,.10,.39),(1.45,.20,.78), 'Marble' if level==8 else stone)
    if level>=2:
        porch_w=1.65 if level<5 else 2.0
        g.box(m,(0,door_h+.57,.45),(porch_w,.12,1.0),s['Roof'])
        for sign in (-1,1):
            g.box(m,(sign*(porch_w/2-.10),(door_h+.65)/2,.78),(.11,door_h+.27,.11),wood)
            g.beam(m,(sign*(porch_w/2-.10),door_h+.25,.78),(sign*(porch_w/2-.42),door_h+.51,.78),.085,wood)
    if level>=3:
        g.window(m,0,e+.45,.58,.63,.06,wood,False)
    # Architectural flues only; they grant no stove or furnishing buff. Long halls get a second one further forward.
    for zc in [rear*.70,rear*.22][:s['Chimneys']]:
        chimney_x=-(half-.65); chimney_y=e+.35
        g.box(m,(chimney_x,chimney_y,zc),(.45,1.4,.45),'Brick' if level<7 else stone)
        g.box(m,(chimney_x,chimney_y+.73,zc),(.58,.12,.58),stone)
        g.box(m,(chimney_x,chimney_y+.80,zc),(.34,.025,.34),'WoodDark')
    if level>=4:
        metal='Copper' if level==4 else 'Iron' if level<8 else 'Steel'
        for sign in (-1,1):
            for y in (.65,e-.35):
                g.box(m,(sign*(half-.07),y,.145),(.27,.095,.035),metal)
    if level==8:
        for y in (.37,e-.22):g.box(m,(0,y,.12),(w-.4,.09,.12),'Marble')
        for sign in (-1,1):g.box(m,(sign*(half-.05),.62,.14),(.30,.48,.16),'Marble')
    return m,(windows,side_z)


def interior(s, openings_spec):
    """Cutaway room seen from the front at an angle: floor grid width x depth, back and side walls, no front wall.
    Grid origin (0,0,0) is the front-left floor corner as seen from the camera; columns run toward world -X and rows
    run toward the back (world -Z). Upgrades add rows at the back, so saved furniture coordinates never move."""
    windows,side_z=openings_spec
    level=s['Level']; w=s['GridWidth']*GRID_METRES; d=s['GridDepth']*GRID_METRES; h=s['WallHeight']
    m=k.Mesh(f'HouseL{level:02}Interior',pal); wood=s['Wood']; wall=s['Wall']
    ext_depth=s['Depth']-.8
    win_w,win_h=(.63,.64) if level==1 else (.76,.85) if level<3 else (.88,1.02)
    sill=min(1.38,h-.75)
    # Floor: a dark base slab and one board per grid column running front to back (trim stays outside the grid).
    g.box(m,(-w/2,-.11,-d/2),(w+.36,.12,d+.30),'FloorDark')
    for i in range(s['GridWidth']):g.box(m,(-GRID_METRES*(i+.5),-.03,-d/2),(GRID_METRES-.012,.06,d),'Floor')
    # Back wall with the exterior's rear windows (mirrors of the front pair).
    back_x=[-w/2+x*w/(s["Width"]-.5) for x,y,ww,hh in windows]
    back=[(xx-ww/2,xx+ww/2,sill-hh/2,sill+hh/2) for xx,(x,y,ww,hh) in zip(back_x,windows)]
    g.wall(m,-w-.16,.16,0,h,-d-.08,.16,back,wall)
    for xx,(x,y,ww,hh) in zip(back_x,windows):
        start=len(m.vertices);g.window(m,0,sill,ww,hh,0,wood,False,level==8)
        m.transform(lambda p,xx=xx:(p[0]+xx,p[1],p[2]-d+.01),start)
    # Side walls carry the same number of windows as the exterior, at the same relative depth. Their windows face
    # into the room: the wall at +X (screen-left) is built as a -X-facing side and the other way round.
    zs=[z*d/ext_depth for z in side_z]
    for face,xw in ((-1,.08),(1,-w-.08)):
        holes=[(min(g.along(face,z-win_w/2),g.along(face,z+win_w/2)),max(g.along(face,z-win_w/2),g.along(face,z+win_w/2)),sill-win_h/2,sill+win_h/2) for z in zs]
        lo,hi=sorted((g.along(face,-d-.16),g.along(face,0.0)))
        start=len(m.vertices); g.side(m,face,0,lambda mm,lo=lo,hi=hi,holes=holes:g.wall(mm,lo,hi,0,h,0,.16,holes,wall))
        m.transform(lambda p,xw=xw:(p[0]+xw,p[1],p[2]),start)
        for z in zs:
            start=len(m.vertices); g.side(m,face,0,lambda mm,z=z,face=face:g.window(mm,g.along(face,z),sill,win_w,win_h,.09,wood,False,level==8))
            m.transform(lambda p,xw=xw:(p[0]+xw,p[1],p[2]),start)
    # Timber: corner posts, wall plates along the tops, wall posts between window bays from level 3.
    for x in (.08,-w-.08):
        g.box(m,(x,h/2,-d-.08),(.20,h,.20),wood)
        g.box(m,(x,h+.06,-d/2),(.22,.12,d+.3),wood)
    g.box(m,(-w/2,h+.06,-d-.08),(w+.4,.12,.22),wood)
    if level>=3:
        for z in [-d*i/max(1,len(zs)) for i in range(1,len(zs))]:
            for x in (-.02,-w+.02):g.box(m,(x,h/2,z),(.08,h,.12),wood)
        for x in (-w*.33,-w*.67):g.box(m,(x,h/2,-d+.01),(.075,h,.04),wood)
        g.box(m,(-w/2,.9,-d+.005),(w,.06,.03),wood)
    # Rafter stubs rest on the wall plates only, so nothing hangs between the camera and the floor.
    n=max(3,round(d/1.1))
    for i in range(n):
        z=-d*(i+.5)/n
        for x,dx in ((.08,-.18),(-w-.08,.18)):g.box(m,(x+dx,h+.17,z),(.42,.10,.10),wood)
    # Cut front edge: a low sill shows where the front wall was removed, with a threshold gap at the door.
    for x0,x1 in ((.16,-w/2+.55),(-w/2-.55,-w-.16)):
        g.box(m,((x0+x1)/2,.06,.09),(abs(x1-x0),.12,.16),wood)
    g.box(m,(-w/2,.015,.10),(1.1,.03,.18),'Stone')
    guides=k.Mesh(f'HouseL{level:02}GridGuides',pal)
    for i in range(s['GridWidth']+1):g.box(guides,(-i*GRID_METRES,.006,-d/2),(.012,.008,d),'Grid',shade=2)
    for j in range(s['GridDepth']+1):g.box(guides,(-w/2,.006,-j*GRID_METRES),(w,.008,.012),'Grid',shade=2)
    return m,guides


def furniture_proxy(m,name,c,wood):
    """Readable furnishing study only, not a finished furniture asset or free item."""
    x,y,z=c; low=name.lower()
    if 'chest' in low:g.chest(m,c,wood=wood)
    elif 'sofa' in low or 'bedroll' in low:
        g.box(m,(x,y+.19,z),(.95,.20,.42),'Cloth' if 'sofa' in low else 'Fur')
        if 'sofa' in low:
            g.box(m,(x,y+.46,z-.18),(.95,.42,.10),'Cloth')
            for sx in (-1,1):g.box(m,(x+sx*.46,y+.36,z),(.09,.30,.48),wood)
        else:g.box(m,(x-.29,y+.32,z),(.26,.12,.40),'CreamCloth')
    elif 'loom' in low or 'press' in low or 'frame' in low:
        for sx in (-1,1):g.box(m,(x+sx*.42,y+.46,z),(.085,.92,.14),wood)
        for yy in (.12,.86):g.box(m,(x,y+yy,z),(.95,.085,.15),wood)
        for i in range(10):g.box(m,(x-.33+i*.073,y+.51,z+.07),(.018,.57,.025),'CreamCloth')
    elif any(word in low for word in ('cabinet','dresser','wardrobe','armoire')):
        g.box(m,(x,y+.43,z),(.82,.86,.35),wood)
        for sx in (-1,1):
            g.box(m,(x+sx*.20,y+.44,z+.19),(.36,.74,.04),'WoodDark')
            g.box(m,(x+sx*.05,y+.44,z+.23),(.025,.08,.03),'Copper')
    elif 'cookstove' in low or 'forge' in low:
        g.box(m,(x,y+.27,z),(.78,.54,.42),'Ceramic' if 'cookstove' in low else 'Basalt')
        g.box(m,(x,y+.28,z+.23),(.34,.28,.035),'Iron')
        g.box(m,(x,y+.56,z),(.84,.055,.48),'Iron')
        g.box(m,(x+.24,y+.79,z-.12),(.15,.43,.15),'Iron')
    elif 'rack' in low or 'stand' in low or 'peg' in low or 'display' in low:
        for sx in (-1,1):g.box(m,(x+sx*.36,y+.44,z),(.07,.88,.10),wood)
        for yy in (.10,.69):g.box(m,(x,y+yy,z),(.90,.07,.12),wood)
        for i in (-1,0,1):g.box(m,(x+i*.24,y+.65,z+.10),(.05,.16,.17),'Iron')
    elif 'basket' in low:
        g.box(m,(x,y+.19,z),(.58,.38,.40),'CreamCloth')
        for yy in (.08,.30):g.box(m,(x,y+yy,z+.21),(.59,.04,.025),wood)
    elif 'sign' in low:
        for sx in (-1,1):g.box(m,(x+sx*.25,y+.34,z),(.07,.68,.12),wood)
        g.box(m,(x,y+.56,z+.03),(.76,.40,.10),'Green')
        g.box(m,(x,y+.56,z+.10),(.37,.045,.025),'Paper')
    else:
        g.table(m,c,wood)
        if 'stool' in low:g.box(m,(x,y+.64,z),(.40,.08,.38),'Leather')
        elif 'bench' not in low:g.box(m,(x+.20,y+.71,z),(.24,.18,.18),'CreamCloth')


def study(s):
    m=k.Mesh(f'HouseL{s["Level"]:02}FurnitureStudy',pal); entries=[]
    # All six proxies stand on the floor of the smallest 8x5 room, facing the camera; no physics/placement authority.
    for i,name in enumerate(FURNITURE[s['Level']-1]):
        col=i%3; row=i//3; gx=.25+col*2.55; gz=.25+row*2.5
        furniture_proxy(m,name,(-(gx+1.0)*GRID_METRES,0,-(gz+1.0)*GRID_METRES),'WoodDark')
        entries.append(dict(Name=name,GridX=gx,GridZ=gz,Width=2,Depth=2,PreviewOnly=True))
    return m,entries


def attachments():
    pet=k.Mesh('PlayerPetHome',pal)
    # Dark doorway is recessed, not painted across a solid front wall.
    g.box(pet,(0,.08,-.36),(1.38,.16,1.10),'Pine')
    g.wall(pet,-.62,.62,.16,.91,0,.12,[(-.30,.30,.16,.69)],'PineLight')
    for x in (-.62,.62):g.box(pet,(x,.52,-.40),(.10,.75,.82),'Pine')
    g.box(pet,(0,.52,-.83),(1.24,.75,.10),'Pine')
    g.box(pet,(0,.45,-.55),(.64,.56,.035),'WoodDark')
    g.gable(pet,.62,.91,1.34,0,'PineLight')
    g.gable(pet,.62,.91,1.34,-.83,'Pine')
    g.roof(pet,1.48,1.15,.92,1.34,.16,'RoofClay','Pine',False)
    for x in (-.30,.30):g.box(pet,(x,.43,.085),(.075,.56,.10),'WoodDark')
    collection=k.Mesh('PetCollectionContainer',pal)
    g.chest(collection,(0,0,-.28),.88,.58,.56,'Pine','Iron')
    # Plain leaf relief distinguishes gathered supplies from a normal chest.
    g.beam(collection,(-.16,.25,.035),(.12,.48,.035),.025,'Green',.02)
    for sign in (-1,1):
        g.box(collection,(sign*.09,.38,.055),(.12,.055,.025),'GreenLight',taper=.5)
    return pet,collection


def save(m):
    info=m.save(OUT,HEADER)
    info['BoundsMin']=[min(v[i] for v in m.vertices) for i in range(3)]
    info['BoundsMax']=[max(v[i] for v in m.vertices) for i in range(3)]
    print(m.name, m.triangles, 'triangles')
    return info


def main():
    OUT.mkdir(parents=True,exist_ok=True); pal.write_png(OUT/'HousePalette.png')
    models=[]; houses=[]
    for original in LEVELS:
        s=dict(original); front,windows=exterior(s); inside,guides=interior(s,windows); furnishing,items=study(s)
        for mesh in (front,inside,guides,furnishing):models.append(save(mesh))
        s.update(Front=front.name,Interior=inside.name,GridGuides=guides.name,FurnitureStudy=furnishing.name,
                 GridMetres=GRID_METRES,PlotWidth=PLOT_WIDTH,PlotDepth=PLOT_DEPTH,GridOrigin=[0,0,0],GridAxisX=[-GRID_METRES,0,0],
                 GridAxisDepth=[0,0,-GRID_METRES],DoorAnchor=[0,.34,.15],SelectAnchor=[0,1.4,.2],GroundAnchor=[0,0,0],
                 PetHomeAnchor=[4.45,0,.25],CollectionAnchor=[-4.4,0,.25],
                 HomeCameraAnchor=[0,max(2.4,s['Height']*.45),13],PetsUnlocked=s['Level']>=3,FurnitureSamples=items)
        houses.append(s)
    for mesh in attachments():models.append(save(mesh))
    manifest=dict(Revision=2,Growth='depth: one storey; rows are added toward the back of the house',Style='faceted',Scope='Art only. Furnishing studies are optional previews, never owned items.',
                  Coordinates='+Y up; +Z street-facing; +X is screen-left from the street camera; interior rows run toward -Z.',
                  Palette='HousePalette.png',Houses=houses,Models=models)
    (OUT/'HouseManifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    print('Generated 8 exterior/interior pairs, guides, optional furnishing studies and 2 pet-front attachments.')


if __name__=='__main__':main()
