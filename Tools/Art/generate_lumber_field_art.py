"""Selective lumber-store/forester/zone authoring. Never regenerates existing trees/shops."""
from pathlib import Path
import sys,math,json,copy
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(Path(__file__).parent))
import bpy
from mathutils import Vector
import faceted_character_kit as k
import generate_storefront_meshes as g
import storefront_shops as s
import generate_shopkeeper_meshes as people
import faceted_people as fp
import generate_region01_meshes as b
import generate_tier02_batch01 as kit
DOC=ROOT/'Docs/Art/Settlement/LumberFieldStorefront';ART=ROOT/'Assets/_Game/Art/Settlement/LumberField'
pal=copy.deepcopy(g.pal)
for name,color,kind in [('LumberTeal','#447F82','cloth'),('LumberTealDark','#315C60','cloth'),('GroundT1','#8D9B60','grass'),('GroundT2','#768365','soil'),('GroundT3','#B7A36D','grass'),('GroundT4','#B8A164','grass')]:pal.add(name,color,kind)
# Tier 2 (birch) front, Milestone 14: the storefronts' recolours and copper/bark/oilskin slots on this palette (own strip).
pal2=s.tier2_palette(pal)
ASSEMBLIES=[];MODELS=[]
def box(m,c,size,color,**kw):k.box(m,c,size,color,**kw)
def limb(m,a,z,r,color='Timber',n=7):k.limb(m,[(a,r,r),(z,r*.9,r*.9)],color,sides=n,forward_hint=(0,1,0))
def model(name):return k.Mesh(name,pal)
def node(name,parent=None,pos=(0,0,0),mesh=None,rotation=None,reference=None,look=None):
    return dict(Name=name,Parent=parent,LocalPosition=list(pos),Model=mesh,LocalRotation=list(rotation or (0,0,0)),Reference=reference,LookAt=look)
def add(name,nodes,budget,path,**data):ASSEMBLIES.append(dict(Name=name,Nodes=nodes,TriangleCeiling=budget,PrefabPath=path,**data))

def log(m,c,length=.75,r=.12,bark='Timber'):
    k.limb(m,[((c[0]-length/2,c[1],c[2]),r,r),((c[0]+length/2,c[1],c[2]),r*.94,r*.94)],bark,sides=8,forward_hint=(0,1,0))
    # Birch logs: black lenticel dashes across the street-facing bark.
    if bark=='BirchBark':
        for dx in [-.22,.18]:box(m,(c[0]+dx*length,c[1]+r*.35,c[2]+r*.93),(length*.18,.022,.012),'Lenticel')
    # Pale end disks sit outside the bark sleeve, read as cut ends.
    for sign in [-1,1]:k.limb(m,[((c[0]+sign*(length/2+.001),c[1],c[2]),r*.9,r*.9),((c[0]+sign*(length/2+.005),c[1],c[2]),r*.9,r*.9)],'TimberLight',sides=8,forward_hint=(0,1,0))

def axe_log_icon(m,c,scale=1):
    for sign in [-1,1]:
        z=c[2]+sign*.065
        axis=k.unit((.9,.5,0));across=(-axis[1],axis[0],0)
        a=k.add((c[0],c[1]-.015,z),k.mul(axis,-.23*scale));b0=k.add((c[0],c[1]-.015,z),k.mul(axis,.23*scale))
        k.limb(m,[(a,.060*scale,.060*scale),(b0,.060*scale,.060*scale)],'Trim',sides=8,forward_hint=(0,0,1))
        axis=k.unit((-.55,1,0));across=(-axis[1],axis[0],0)
        box(m,(c[0],c[1]-.02,z+sign*.045),(.035*scale,.48*scale,.018),'Timber',axes=(across,axis,(0,0,1)))
        p=k.add((c[0],c[1]-.02,z+sign*.055),k.mul(axis,.16*scale))
        box(m,p,(.20*scale,.095*scale,.018),'Steel',axes=(across,axis,(0,0,1)),taper=.65)

def front(tier=1):
    # Tier 2: birch timber, slate roof, birch logs, ochre oilskin awning, copper lean-to and fittings (same massing and sign).
    t2=tier>1;P=pal2 if t2 else pal;bark='BirchBark' if t2 else 'Timber'
    m=k.Mesh('LumberFront'+('T2' if t2 else ''),P);box(m,(0,.16,-1.8),(5.7,.32,4.0),'StoneDark')
    g.wall_with_openings(m,-2.6,2.6,.32,3.55,-.08,.15,[(-1.95,-.80,.32,2.50),(-.35,2.12,1.0,2.78)],'Plaster')
    for x in [-2.6,2.6]:box(m,(x,1.94,-1.85),(.16,3.24,3.65),'Plaster')
    box(m,(0,1.94,-3.68),(5.2,3.24,.16),'Plaster')
    g.timber_frame(m,-2.67,2.67,.35,3.60,.04,[-2.6,-.66,2.6],braces=())
    g.gable_roof(m,-2.65,2.65,3.65,5.5,.08,-3.8,'RoofDark','Timber',overhang=.22)
    s.prism(m,[(-2.6,3.65),(2.6,3.65),(0,5.50)],-3.78,-3.68,'Plaster')
    g.window(m,-.35,2.12,1.0,2.78,.05,panes=(3,2),glass=None)
    box(m,(.88,1.89,-.33),(2.4,1.76,.08),'PlankDark')
    for y in [1.08,1.67,2.24]:
        box(m,(.88,y,-.15),(2.36,.06,.35),'Plank')
        for i in range(3):log(m,(.10+i*.71,y+.15,-.12),.60,.10,bark)
    g.door(m,-1.95,-.80,.32,2.50,.04)
    g.awning(m,-.52,2.32,3.05,.80,colors=('OilOchre','Flax') if t2 else ('LumberTeal','AwningCream'),z=.13,stripes=8)
    # Open lean-to with four supported posts and a sheltered wood pile.
    for x in [2.77,3.61]:
        for z in [-.05,-2.40]:box(m,(x,1.1,z),(.12,2.2,.12),'Timber')
    s.roof(m,[(2.68,2.52),(3.76,2.21)],.08,-2.55,'PlankDark','Timber',thick=.09,overhang=.03)
    for row in range(3):
        for j in range(3-row):log(m,(3.16,.17+row*.25,-.30-j*.43),.90,.14,bark)
    # Sawhorse on left yard; legs terminate at ground, top supports the working log.
    for x in [-3.45,-2.83]:
        for sign in [-1,1]:limb(m,(x+sign*.14,0,.55),(x,.68,.55),.042,'TimberLight',5)
    box(m,(-3.14,.72,.55),(.88,.11,.20),'Timber');log(m,(-3.14,.90,.55),1.05,.13,bark)
    sign=g.hanging_sign(m,-2.55,3.32,.05,.72);axe_log_icon(m,sign,.90)
    if t2:
        s.slab(m,(2.68,2.55),(3.76,2.24),.11,-2.58,.03,'CopperFit','Verdigris')
        for x in [2.77,3.61]:s.bark_post(m,(x,1.1,-.05),(.12,2.2,.12),4)
        box(m,(0,5.58,-1.86),(.2,.08,4.5),'CopperFit');box(m,(-1.37,2.62,.1),(1.35,.05,.2),'CopperFit')
        MODELS.append((m,P));add(m.name,[node('FrontVisual',mesh=m.name),node('SelectAnchor',pos=(0,1.4,.2))],3500,'Settlement/Storefronts/'+m.name,Width=7.8,ShopId='shop.lumber',Tier=2,Awning='ochre oilskin/flax (tier 2 birch look)');return
    MODELS.append((m,pal));add('LumberFront',[node('FrontVisual',mesh=m.name),node('SelectAnchor',pos=(0,1.4,.2))],4000,'Settlement/Storefronts/LumberFront',Width=7.8,ShopId='shop.lumber',Awning='petrol teal/cream; new dedicated color')

def ledger():
    m=model('LumberLedger');cx=.55;top=1.02
    box(m,(cx,top+.022,.48),(.79,.038,.48),'Ledger')
    # Two wedge page blocks, a V spine and broad no-text wood swatches.
    for sign in [-1,1]:
        x0=cx+sign*.025;x1=cx+sign*.36
        pts=[(x0,top+.045,.27),(x1,top+.10,.27),(x1,top+.10,.69),(x0,top+.045,.69)]
        s.hexa(m,pts+[(x,y-.025,z) for x,y,z in pts],'Paper','AwningCream')
        for j,color in enumerate(['TimberLight','Trim','Copper','PlankDark']):
            x=cx+sign*.20;z=.32+j*.075;y=top+.045+(.20-.025)/(.36-.025)*.055+.004
            box(m,(x,y,z),(.14,.004,.048),color)
    box(m,(cx,top+.044,.48),(.025,.007,.40),'PlankDark');MODELS.append((m,pal))

def counter():
    m=model('LumberCounter')
    for i in range(14):box(m,(-3.25+i*.5,-.03,-.4),(.49,.06,3.6),'Plank' if i%2 else 'PlankDark')
    g.wall_with_openings(m,-3.4,3.4,0,3.2,-1.95,.12,[(-3.15,-2.0,0,2.35)],'Plaster')
    for x in [-3.4,3.4]:box(m,(x,1.60,-.4),(.10,3.2,3.2),'Plaster')
    box(m,(0,3.25,-.4),(6.9,.10,3.2),'PlankDark')
    for x in [-3.34,-1.85,1.1,3.34]:box(m,(x,1.6,-1.85),(.12,3.2,.10),'Timber')
    box(m,(0,3.12,-1.85),(6.7,.13,.12),'Timber')
    s.counter(m)
    for y in [1.1,1.65,2.2]:
        box(m,(2.32,y,-1.64),(1.6,.07,.40),'TimberLight')
        for j in range(2):log(m,(1.94+j*.68,y+.14,-1.59),.60,.10)
    # Peg board with supported axes and a toothed crosscut saw.
    box(m,(-.25,2.18,-1.78),(2.2,.96,.055),'Plank')
    for x in [-1.03,-.45]:
        limb(m,(x,1.83,-1.70),(x+.13,2.55,-1.70),.025,'TimberLight',6)
        box(m,(x+.11,2.45,-1.68),(.30,.16,.045),'Steel',taper=.55)
        box(m,(x,2.59,-1.70),(.10,.035,.12),'Iron')
    box(m,(.29,2.18,-1.68),(.55,.10,.025),'Steel')
    for i in range(8):s.prism(m,[(.025+i*.064,2.13),(.060+i*.064,2.09),(.087+i*.064,2.13)],-1.66,-1.69,'Steel')
    for x in [-.04,.61]:box(m,(x,2.18,-1.68),(.08,.20,.04),'TimberLight')
    # Saw-blade emblem, no text.
    poly=[(1.55+(.28 if i%2==0 else .235)*math.cos(i*math.tau/32),2.72+(.28 if i%2==0 else .235)*math.sin(i*math.tau/32)) for i in range(32)]
    s.prism(m,poly,-1.75,-1.80,'Steel');box(m,(1.55,2.72,-1.72),(.06,.06,.02),'Iron')
    # Door frame is static; opening really exists in the wall.
    frame=model('LumberDoorFrame')
    for x in [-3.20,-1.95]:box(frame,(x,1.18,-1.83),(.12,2.36,.17),'Timber')
    box(frame,(-2.575,2.40,-1.83),(1.36,.12,.17),'Timber');box(frame,(-2.575,.025,-1.78),(1.13,.05,.24),'TimberLight');MODELS.append((frame,pal))
    # A daylight glimpse behind the actual hole, decorative only.
    box(m,(-2.575,1.24,-4.2),(1.50,2.48,.03),'GlassLight')
    for x in [-3.02,-2.22]:limb(m,(x,0,-3.9),(x+.05,2.4,-3.9),.075,'Timber',6)
    MODELS.append((m,pal));ledger()
    leaf=model('LumberDoorLeaf')
    for i in range(5):box(leaf,((i+.5)*1.10/5,1.14,.025),(.212,2.28,.06),'Plank' if i%2 else 'PlankDark')
    for y in [.30,1.95]:box(leaf,(.55,y,.065),(1.08,.065,.025),'Iron')
    box(leaf,(.94,1.12,.095),(.05,.05,.04),'Brass');MODELS.append((leaf,pal))
    nodes=[node('RoomVisual',mesh=m.name),node('DoorFrame',mesh=frame.name),node('DoorPivot',pos=(-3.15,0,-1.86)),node('DoorLeaf','DoorPivot',mesh=leaf.name),node('Ledger',mesh='LumberLedger'),node('LedgerAnchor',pos=(.55,1.10,.48),rotation=(-80,0,0)),node('LedgerSelectAnchor',pos=(.55,1.13,.48)),node('DoorSelectAnchor',pos=(-2.575,1.1,-1.82)),node('DoorwayCameraAnchor',pos=(-2.575,1.55,-1.1),look=[-2.575,1.40,-4]),node('CustomerCameraAnchor',pos=(0,1.55,3),look=[0,1.35,0]),node('Shopkeeper',reference='ShopkeeperForester')]
    add('LumberCounter',nodes,6000,'Settlement/Storefronts/LumberCounter',ShopId='shop.lumber',CounterTop=1.02,DoorOpenAngle=100,DoorRotationAxis='local Y; inward negative Z',DoorOpening=[-3.15,-2.0,0,2.35],LedgerSurfaceSize=[.65,.37],OfferAnchors=[])

def forester():
    body=k.Mesh('ForesterBody',people.pal);people.body_base(body,False,'SkinG','Olive','Dark',rolled=True)
    # Surface-seated woven plaid bands, using existing keeper palette colors.
    for y in [.04,.12,.20,.28,.36]:
        for side in [-1,1]:
            pts=[]
            for x,yy in [(-.14,y-.009),(.14,y-.009),(.14,y+.009),(-.14,y+.009)]:
                z=fp.torso_front(False,x,yy)+.002 if side>0 else -fp.section_at(fp.torso_sections(False),yy)[2]-.002
                pts.append((x,yy,z))
            ids=[body.v(p) for p in pts];body.quad(*ids,'LeatherDark')
    for x in [-.12,-.06,0,.06,.12]:
        for ya,yb in [(-.035,.08),(.08,.20),(.20,.32),(.32,.39)]:
            pts=[(xx,yy,fp.torso_front(False,xx,yy)+.003) for xx,yy in [(x-.006,ya),(x+.006,ya),(x+.006,yb),(x-.006,yb)]]
            ids=[body.v(p) for p in pts];body.quad(*ids,'Cream')
    # Subdivide across the curved torso so the apron cannot sink through its centre.
    rows=[(-.08,.15),(.02,.15),(.12,.15),(.22,.11),(.25,.11)]
    for (ya,wa),(yb,wb) in zip(rows,rows[1:]):
        for col in range(6):
            a=-1+col/3;b0=-1+(col+1)/3
            pts=[(x,y,fp.torso_front(False,x,y)+.014) for x,y in [(wa*a,ya),(wa*b0,ya),(wb*b0,yb),(wb*a,yb)]]
            ids=[body.v(p) for p in pts];body.quad(*ids,'Leather',(0,(ya+yb)/2,0))
    for sign in [-1,1]:
        a=(sign*.10,.25,fp.torso_front(False,sign*.10,.25)+.015);z=(sign*.06,.47,.03);axis=k.unit(k.sub(z,a))
        box(body,k.lerp(a,z,.5),(.026,k.length(k.sub(z,a)),.008),'LeatherDark',axes=((1,0,0),axis,k.cross((1,0,0),axis)))
    w,f,bk=fp.section_at(fp.torso_sections(False),.02)
    k.vertical_loft(body,[(.005,w+.008,f+.012,bk+.008),(.030,w+.008,f+.012,bk+.008)],'LeatherDark',a=.70,b=.62,cap_start=False,cap_end=False)
    head=k.Mesh('ForesterHead',people.pal);people.head_base(head,False,'SkinG','HairDarkBrown','Crop',beard=True);people.beanie(head,False,'Olive','LeatherDark')
    MODELS.extend([(body,people.pal),(head,people.pal)])
    add('ShopkeeperForester',[node('TorsoPivot',pos=(0,.94,0)),node('Body','TorsoPivot',mesh=body.name),node('HeadPivot','TorsoPivot',pos=(0,.49,0)),node('Head','HeadPivot',mesh=head.name)],2300,'Characters/Shopkeepers/ShopkeeperForester',Keeper='Forester',ShopId='shop.lumber',TorsoPivot=.94,HeadUp=.49)

def vertices(path):return [tuple(map(float,l.split()[1:])) for l in path.read_text().splitlines() if l.startswith('v ')]
def zone_contracts():
    trees=json.loads((ROOT/'Assets/_Game/Art/Harvest/Massive/MassiveTreesManifest.json').read_text())['Trees'];result=[]
    for tier,species in enumerate(['Pine','Birch','Mesquite','Oak'],1):
        standing=[];swept=[]
        for t in [t for t in trees if t['Species']==species]:
            low=vertices(ROOT/('Assets/_Game/Art/Harvest/Massive/'+t['LowerMesh']+'.obj'));upper=vertices(ROOT/('Assets/_Game/Art/Harvest/Massive/'+t['UpperMesh']+'.obj'))
            standing+=low+[(x,y+t['CutHeight'],z) for x,y,z in upper]
            for angle in range(0,86,5):
                co=math.cos(math.radians(angle));si=math.sin(math.radians(angle))
                swept +=[(x,t['CutHeight']+y*co-z*si,y*si+z*co) for x,y,z in upper]
        minx=min(x for x,y,z in standing+swept);maxx=max(x for x,y,z in standing+swept);minz=min(z for x,y,z in standing);maxz=max(z for x,y,z in swept+standing)
        col=math.ceil(maxx-minx+3);row=math.ceil(maxz-minz+3);front=math.ceil(maxz+4);back=-row+math.floor(minz)-4;width=2*col+(maxx-minx)+8
        anchors=[[-col,0,0],[0,0,0],[col,0,0],[-col,0,-row],[0,0,-row],[col,0,-row]]
        result.append(dict(Tier=tier,Species=species,ColumnSpacing=col,RowSpacing=row,FrontClearance=front,BackFenceZ=back,Width=width,FrontZ=front+2,TreeAnchors=anchors,StandingBoundsX=[minx,maxx],StandingMinZ=minz,SweepMaxZ=maxz,Margin=3,FallAngle=85,FallAxis='local X',TreeYawDegrees=0,VariantSequence=['A','B','C','C','A','B']))
    return result

def zones():
    for z in zone_contracts():
        tier=z['Tier'];m=model('LumberZoneT'+str(tier));w=z['Width'];front=z['FrontZ'];back=z['BackFenceZ'];groundback=back-(25 if tier<4 else 8)
        box(m,(0,-.06,(front+groundback)/2),(w,.12,front-groundback),'GroundT'+str(tier))
        for yrow in [0,-z['RowSpacing']]:box(m,(0,.003,yrow+z['FrontClearance']*.45),(w-4,.006,z['FrontClearance']*.9),'Soil')
        # Palisade has pointed posts, rails and supports, not a stretched texture wall.
        count=math.ceil(w/2)
        for i in range(count+1):
            x=-w/2+w*i/count;box(m,(x,1.65,back),(.95,3.3,.28),'Timber')
            s.prism(m,[(x-.475,3.3),(x,3.75),(x+.475,3.3)],back+.15,back-.15,'TimberLight')
        for y in [.65,2.5]:box(m,(0,y,back+.18),(w,.16,.16),'TimberLight')
        # Low side fences; posts sit on the ground.
        steps=math.ceil((front-back)/6)
        for sign in [-1,1]:
            x=sign*w/2
            for i in range(steps+1):box(m,(x,.60,back+(front-back)*i/steps),(.17,1.20,.17),'Timber')
            for y in [.42,.90]:box(m,(x,y,(front+back)/2),(.11,.12,front-back),'TimberLight')
        # Front boundary leaves a real centered doorway.
        for sign in [-1,1]:box(m,(sign*(w/4+.3425),.48,front),(w/2-.685,.96,.13),'Timber')
        for x in [-.625,.625]:box(m,(x,1.18,front),(.12,2.36,.17),'Timber')
        box(m,(0,2.4,front),(1.36,.12,.17),'Timber');box(m,(0,.025,front),(1.13,.05,.24),'TimberLight')
        if tier==4:
            s.prism(m,[(-w/2,0),(-w/3,4),(-w/6,3),(0,5),(w/5,3.6),(w/2,2),(w/2,0)],back-6,back-7,'GroundT4')
        MODELS.append((m,pal));nodes=[node('ZoneVisual',mesh=m.name),node('ExitSelectAnchor',pos=(0,1.1,front+.2)),node('ZoneCameraAnchor',pos=(0,max(35,w*.70),front+32),look=[0,1,-z['RowSpacing']*.45]),node('PickupCameraAnchor',pos=(0,1.6,7),look=[0,1,0])]
        for i,p in enumerate(z['TreeAnchors'],1):nodes.append(node('TreeAnchor'+str(i),pos=p))
        if tier<4:
            for i,x in enumerate([-z['ColumnSpacing'],0,z['ColumnSpacing']],1):nodes.append(node('BackdropAnchor'+str(i),pos=(x,0,back-19)))
        add(m.name,nodes,3000,'Settlement/LumberField/'+m.name,**z)

def expanded(p):
    colors=[];types=[]
    for c,d in zip(p.colors,p.details):
        for shade in k.SHADES:colors.append(tuple(max(0,min(255,round(v*shade))) for v in c));types.append(d or 'none')
    return colors,types

def front_t2_only():
    """Narrow run (--front-t2): writes LumberFrontT2.obj/Palette.png and upserts its rows in the manifest; no other piece, render or docs."""
    bpy.ops.wm.read_factory_settings(use_empty=True);front(2);kit.ART=ART;kit.PAL=ART;(m,palette),=MODELS;a,=ASSEMBLIES
    raw=b.Model(m.name);raw.vertices=list(m.vertices);raw.faces=[(x+1,y+1,z+1,slot) for x,y,z,slot in m.faces]
    colors,types=expanded(palette);obj,row=kit.create_asset(m.name,colors,types,raw);row.pop('PrefabPath',None);row['MaterialGroup']='LumberT2'
    a['Pieces']=[m.name];a['Triangles']=row['Triangles'];assert a['Triangles']<=a['TriangleCeiling'],(a['Name'],a['Triangles'])
    path=ART/'LumberFieldArtManifest.json';manifest=json.loads(path.read_text())
    for key,item,name in [('Assemblies',a,'Name'),('Pieces',row,'Model')]:
        rows=manifest[key];old=[i for i,r in enumerate(rows) if r[name]==item[name]]
        if old:rows[old[0]]=item
        else:rows.append(item)
    path.write_text(json.dumps(manifest,indent=2)+'\n');print('LUMBER_ART_T2',a['Name'],a['Triangles'])

def main():
    if '--front-t2' in sys.argv:return front_t2_only()
    gray='--graybox' in sys.argv
    for p in [DOC,ART]:p.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True);bpy.context.preferences.filepaths.save_version=0;bpy.context.preferences.filepaths.file_preview_type='NONE'
    front();forester();counter();zones();kit.ART=ART;kit.PAL=ART
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=12;scene.render.resolution_x=720;scene.render.resolution_y=720;scene.render.resolution_percentage=50 if gray else 100
    scene.world=bpy.data.worlds.new('SlateStudio');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.25,.29,.31,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.65;scene.view_settings.view_transform='Standard';scene.view_settings.look='None'
    camera=bpy.data.objects.new('ReviewCamera',bpy.data.cameras.new('ReviewCamera'));scene.collection.objects.link(camera);scene.camera=camera;camera.data.type='ORTHO'
    for name,pos,power in [('Key',(-5,-7,10),1000),('Fill',(7,-2,7),700),('Rim',(1,9,10),900)]:
        obj=bpy.data.objects.new(name,bpy.data.lights.new(name,'AREA'));scene.collection.objects.link(obj);obj.location=pos;obj.data.energy=power;obj.data.size=8;kit.aim(obj,(0,0,1))
    sources={};pieces=[]
    for m,palette in MODELS:
        target=ROOT/'Assets/_Game/Art/Characters/Shopkeepers' if m.name in ['ForesterBody','ForesterHead'] else ART;kit.ART=target
        raw=b.Model(m.name);raw.vertices=list(m.vertices);raw.faces=[(a+1,bb+1,c+1,slot) for a,bb,c,slot in m.faces]
        colors,types=expanded(palette);obj,row=kit.create_asset(m.name,colors,types,raw);obj.rotation_euler=(0,0,0);obj.hide_render=True;obj.hide_set(True);row.pop('PrefabPath',None)
        row['MaterialGroup']='Forester' if m.name.startswith('Forester') else 'Lumber';sources[m.name]=obj;pieces.append(row)
    groups={};lookup={a['Name']:a for a in ASSEMBLIES}
    def native_assembly(a,parent=None,prefix=''):
        root=bpy.data.objects.new(prefix+a['Name'],None);scene.collection.objects.link(root)
        if parent:root.parent=parent
        else:root.rotation_euler.x=math.pi/2
        nodes={}
        for n in a['Nodes']:
            t=bpy.data.objects.new(prefix+a['Name']+'::'+n['Name'],None);scene.collection.objects.link(t);t.parent=nodes[n['Parent']] if n['Parent'] else root;t.location=n['LocalPosition'];t.rotation_euler=[math.radians(v) for v in n['LocalRotation']];t['RuntimeName']=n['Name'];nodes[n['Name']]=t
            if n['Model']:
                obj=sources[n['Model']].copy();obj.data=sources[n['Model']].data;scene.collection.objects.link(obj);obj.parent=t;obj.hide_render=False;obj.hide_set(False);obj.location=(0,0,0)
            if n['Reference']:native_assembly(lookup[n['Reference']],t,prefix+a['Name']+'::')
        return root
    for a in ASSEMBLIES:
        root=native_assembly(a);groups[a['Name']]=root
        a['Pieces']=[r['Model'] for r in pieces if any(n['Model']==r['Model'] for n in a['Nodes'])]
    def total(a):return sum(next(r['Triangles'] for r in pieces if r['Model']==n['Model']) if n['Model'] else total(lookup[n['Reference']]) if n['Reference'] else 0 for n in a['Nodes'])
    for a in ASSEMBLIES:a['Triangles']=total(a);assert a['Triangles']<=a['TriangleCeiling'],(a['Name'],a['Triangles'])
    if gray:
        mat=bpy.data.materials.new('Neutral');mat.use_nodes=True;mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.52,.52,.52,1);scene.view_layers[0].material_override=mat
    output=DOC/'graybox' if gray else DOC;output.mkdir(exist_ok=True)
    views={'front':(0,-4,.1),'back':(0,4,.1),'left':(-4,0,.1),'right':(4,0,.1),'top':(0,-.001,4),'study':(2.7,-4,2.5)}
    if gray:views={k:views[k] for k in ['front','study']}
    for name,root in groups.items():
        if '--render-only' in sys.argv and name not in sys.argv[sys.argv.index('--render-only')+1].split(','):continue
        for other in groups.values():
            for obj in other.children_recursive:obj.hide_render=other!=root
        bpy.context.view_layer.update();corners=[obj.matrix_world@Vector(c) for obj in root.children_recursive if obj.type=='MESH' for c in obj.bound_box];center=sum(corners,Vector())/len(corners);extent=max(max(c[i] for c in corners)-min(c[i] for c in corners) for i in range(3));camera.data.ortho_scale=extent*1.28
        for light_name,offset,power in [('Key',(-.5,-.7,1),1000),('Fill',(.7,-.2,.7),700),('Rim',(.1,.9,1),900)]:
            light=bpy.data.objects[light_name];light.location=center+Vector(offset)*extent;light.data.energy=power*(extent/7)**2;light.data.size=extent*.7;kit.aim(light,center)
        for view,direction in views.items():
            camera.location=center+Vector(direction)*max(4,extent);kit.aim(camera,center);scene.render.filepath=str(output/(name+'-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for i,root in enumerate(groups.values()):
        root.location.x=i*100
        for obj in root.children_recursive:obj.hide_render=False
    camera.data.ortho_scale=15;camera.location=(3,-12,8);kit.aim(camera,(0,0,2))
    bpy.ops.wm.save_as_mainfile(filepath=str(output/'lumber-field-art.blend'))
    # Keep the tier 2 front rows that --front-t2 upserts (this full run authors tier 1 only); appended last, as the upsert does.
    old=ART/'LumberFieldArtManifest.json';prior=json.loads(old.read_text()) if old.exists() else {}
    ASSEMBLIES.extend(a for a in prior.get('Assemblies',[]) if a['Name']=='LumberFrontT2');pieces.extend(p for p in prior.get('Pieces',[]) if p['Model']=='LumberFrontT2')
    manifest=dict(Revision=1,Scope='Passive settlement art only; no runtime UI/unlocks/travel/respawn changes',Assemblies=ASSEMBLIES,Pieces=pieces,Sign='Original axe-cross-log relief; no text; SVG master in docs',Forester='Existing keeper body/head construction and palette; plaid cloth appearance, no wool ingredient')
    (ART/'LumberFieldArtManifest.json').write_text(json.dumps(manifest,indent=2)+'\n');print('LUMBER_ART',[(a['Name'],a['Triangles']) for a in ASSEMBLIES])

if __name__=='__main__':main()
