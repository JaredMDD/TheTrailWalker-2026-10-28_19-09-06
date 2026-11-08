"""Six reusable tier-one environment kits. No route/segment/scatter changes. Stdlib only."""
from pathlib import Path
import json,math,random
import generate_region01_meshes as b
from generate_region05_meshes import sphere
from generate_region01_environment import COLORS as LEGACY_COLORS

ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'Assets/_Game/Art/Environment/Tier01'
REGIONS=['Dawn Plains','Windward Hills','Mistwood','Fern Hollow','Cloudstep Mountains','Reedwater Delta']
# Common UV semantics, 32 colors. Legacy Region01 keeps its existing 24-color texture unchanged.
BASE=list(LEGACY_COLORS)+[(65,107,117),(85,133,140),(118,159,153),(67,103,52),(105,132,68),(149,135,92),(175,159,109),(199,180,130)]
PALETTES=[]
for region in range(1,7):
    c=list(BASE)
    palettes={
      2:{4:(101,115,60),5:(123,132,70),6:(143,149,86),7:(173,161,100),12:(47,70,46),13:(65,89,51),14:(90,112,62),16:(108,112,100),17:(139,141,122),18:(169,167,143)},
      3:{4:(48,71,48),5:(62,86,55),6:(81,103,65),8:(40,66,43),9:(55,85,49),10:(81,109,60),12:(29,54,44),13:(39,70,52),14:(59,90,62),16:(74,89,79),17:(98,111,94),18:(127,132,111),19:(84,72,51),20:(106,89,60),21:(133,111,76)},
      4:{4:(65,92,47),5:(82,111,54),6:(106,133,65),8:(49,84,42),9:(68,108,48),10:(99,138,62),12:(37,66,44),13:(52,88,51),14:(76,114,64),16:(122,129,106),17:(153,157,127),18:(188,188,154),27:(51,96,43),28:(88,128,52)},
      5:{4:(94,108,66),5:(115,125,78),6:(134,143,94),8:(72,91,53),9:(94,111,62),10:(121,133,79),12:(44,66,54),13:(62,85,61),14:(86,109,73),16:(97,108,110),17:(128,139,138),18:(159,168,159),19:(124,119,95),20:(146,136,103),21:(172,157,118)},
      6:{4:(76,99,58),5:(96,117,65),6:(117,134,78),7:(168,157,96),8:(62,92,48),9:(81,115,52),10:(113,139,69),12:(40,70,48),13:(56,88,56),14:(80,111,68),16:(102,113,99),17:(130,139,117),18:(161,165,135),19:(103,87,57),20:(128,107,69),21:(153,129,84),24:(56,102,113),25:(73,125,136),26:(106,151,151)}
    }
    for i,v in palettes.get(region,{}).items():c[i]=v
    PALETTES.append(c)

def quad(m,points,k,double=False):
    ids=[m.v(p) for p in points]
    for i in range(1,len(ids)-1):
        m.tri(ids[0],ids[i],ids[i+1],k)
        if double:m.tri(ids[0],ids[i+1],ids[i],k)

def tone(x,z,levels=3):
    value=math.sin(x*.63+z*.21)+math.cos(z*.57-x*.17)
    return (0 if value<-.35 else 2 if value>.85 else 1) if levels==3 else int(value>.25)

def leaf(m,a,c,width,k=27):
    direction=b.sub(c,a);side=b.unit(b.cross(direction,(0,0,1) if abs(direction[2])<.7 else (1,0,0)))
    middle=b.add(a,b.mul(direction,.52));left=b.add(middle,b.mul(side,width));right=b.sub(middle,b.mul(side,width));ridge=b.add(middle,(0,.028,0))
    for x,y in [(a,left),(left,c),(c,right),(right,a)]:quad(m,[x,ridge,y],k,True)

def bush(m,rng,height=1,width=1,dry=False):
    for i in range(5):
        a=i*2.4;r=width*(.16+i%3*.12);x=r*math.cos(a);z=r*math.sin(a);y=height*(.26+i%2*.12)
        m.cylinder((x*.5,0,z*.5),(x,y,z),.035,.014,5,1,2)
        sphere(m,(x,y+height*.12,z),(width*.34,height*.32,width*.3),7 if dry else 8+i%3,8,4)

def pine(m,rng,height=5,width=1,lean=0):
    m.cylinder((0,0,0),(lean*.65,height*.68,0),height*.035,height*.015,8,1,3)
    for i in range(4):
        a=i*math.tau/4;m.cylinder((0,.15,0),(.4*math.cos(a),0,.4*math.sin(a)),.065,.018,5,1,2)
    for j in range(4):
        y=height*(.25+j*.15);cx=lean*y/height;radius=height*(.225-j*.036)*width;n=9
        tip=m.v((lean*min(1,y/height+.32),min(height,y+height*.35),0));bottom=m.v((cx,y+.02,0));ring=[]
        for i in range(n):
            a=i*math.tau/n+j*.29;r=radius*rng.uniform(.88,1.08);ring.append(m.v((cx+r*math.cos(a),y+.04*(i%2),r*math.sin(a))))
        for i in range(n):m.tri(tip,ring[(i+1)%n],ring[i],12+(i+j)%3);m.tri(bottom,ring[i],ring[(i+1)%n],12)

def grasses(m,rng,height=.5,width=.8,dry=False,flowers=False):
    for i in range(12):
        x=rng.uniform(-width/2,width/2);z=rng.uniform(-width/2,width/2);h=height*rng.uniform(.5,1);lean=rng.uniform(-.16,.16)
        quad(m,[(x-.022,0,z),(x+.022,0,z),(x+lean,h,z+.05)],7 if dry else 5+i%2,True)
        if flowers and i%3==0:
            for j in range(5):
                a=j*math.tau/5;sphere(m,(x+lean+.035*math.cos(a),h,z+.05+.035*math.sin(a)),(.035,.014,.025),22,5,3)
            sphere(m,(x+lean,h+.016,z+.05),(.023,.013,.023),23,5,3)

def fern(m,rng,height=.7,width=1):
    for j in range(7):
        angle=j*math.tau/7;tip=(math.cos(angle)*width*.65,height,math.sin(angle)*width*.65)
        m.cylinder((0,.03,0),tip,.008,.003,4,9,9)
        for i in range(1,7):
            t=i/8;center=b.mul(tip,t);wing=width*.17*math.sin(t*math.pi)
            for side in (-1,1):
                end=b.add(center,(math.cos(angle+side*1.1)*wing,-.025,math.sin(angle+side*1.1)*wing))
                leaf(m,center,end,wing*.29,27+j%2)

def reeds(m,rng,height=1.7,width=1,bent=False):
    for i in range(9):
        x=rng.uniform(-width*.45,width*.45);z=rng.uniform(-width*.4,width*.4);h=height*rng.uniform(.6,1);drift=.35 if bent else .07
        tip=(x+drift,h,z);m.cylinder((x,0,z),tip,.014,.009,4,6,6)
        for t,sign in [(.3,-1),(.55,1)]:leaf(m,(x+drift*t,h*t,z),(x+drift*t+sign*.25,h*(t+.2),z+.08),.035,6)
        m.cylinder(tip,b.add(tip,(drift*.05,.17,0)),.035,.017,5,7,7)

def rock(m,rng,height=1,width=1,length=1,k=16):
    n=8;rings=[]
    for s,y in [(1,0),(.94,height*.28),(.64,height*.75),(.24,height)]:
        rings.append([m.v((math.cos(i*math.tau/n)*width*s*rng.uniform(.85,1.1),y,math.sin(i*math.tau/n)*length*s*rng.uniform(.85,1.1))) for i in range(n)])
    for j in range(3):
        for i in range(n):
            a,c=rings[j],rings[j+1];nxt=(i+1)%n;col=k+(i+j)%3
            m.tri(a[i],c[nxt],a[nxt],col);m.tri(a[i],c[i],c[nxt],col)
    top=m.v((0,height*1.03,0))
    for i in range(n):m.tri(top,rings[-1][(i+1)%n],rings[-1][i],k+2)

def cluster(m,rng,height=.6,width=1,moss=False):
    for i in range(5):
        start=len(m.vertices);rock(m,rng,height*rng.uniform(.45,1),width*.33,width*.3)
        dx=(i%3-1)*width*.55;dz=(i//3-.5)*width*.65
        m.vertices[start:]=[(x+dx,y,z+dz) for x,y,z in m.vertices[start:]]
        if moss:sphere(m,(dx,height*.37,dz),(width*.24,height*.1,width*.21),9,7,3)

def landform(m,rng,height=1,width=4,length=4,form='mound'):
    # Structured height field, fixed boundary. No path-following ribbons, folded strips or coplanar overlays.
    nx=10;nz=8;grid=[]
    for z in range(nz+1):
        row=[]
        for x in range(nx+1):
            u=x/nx*2-1;v=z/nz*2-1;edge=max(0,(1-u*u)*(1-v*v))
            if form=='slope':value=(u+1)*.5*math.sin((v+1)*math.pi*.5)
            elif form=='cliff':value=(.5+.5*math.tanh((u+.2)*8))*max(0,1-v*v)
            elif form=='basin':value=-edge
            elif form=='bank':value=.6*(1-u*u)*(v+1)*.5
            elif form=='ridge':value=max(0,1-abs(u))*(.55+.45*math.cos(v*3))
            else:value=edge*(.85+.15*math.cos(u*4+v*3))
            y=height*value
            row.append((u*width,y,v*length))
        grid.append(row)
    for z in range(nz):
        for x in range(nx):
            y=sum(grid[z+i][x+j][1] for i,j in [(0,0),(0,1),(1,0),(1,1)])/4
            terrainRock=form=='cliff' or 'Mountain' in m.name or 'Scree' in m.name
            k=19+tone(x,z,2) if 'Mud' in m.name or 'RiverBank' in m.name else 16+tone(x,z) if terrainRock else 4+tone(x,z,2)
            quad(m,[grid[z][x],grid[z+1][x],grid[z+1][x+1],grid[z][x+1]],k)

def patch(m,rng,width=3,length=3,kind='grass'):
    for z in range(6):
        for x in range(6):
            x0=(x/6-.5)*width*2;x1=((x+1)/6-.5)*width*2;z0=(z/6-.5)*length*2;z1=((z+1)/6-.5)*length*2
            if kind=='water':k=24+tone(x,z)
            elif kind=='sand':k=29+tone(x,z)
            elif kind=='mud':k=19+tone(x,z,2)
            elif kind=='leaves':k=19+tone(x,z,2)
            elif kind=='shoulder':k=20+x%2 if x<2 else 7 if x==2 else 4+(z%2)
            else:k=4+(1 if math.sin(x*.7)+math.cos(z*.8)>0 else 0)
            # Each tile has an exact planar boundary. Water is a replacement surface, never an overlay on ground.
            quad(m,[(x0,0,z0),(x0,0,z1),(x1,0,z1),(x1,0,z0)],k)
    if kind=='leaves':
        for i in range(18):
            x=rng.uniform(-width*.9,width*.9);z=rng.uniform(-length*.9,length*.9)
            leaf(m,(x,.009,z),(x+rng.uniform(.12,.3),.009,z+rng.uniform(-.2,.2)),.045,20+i%2)

def deadwood(m,rng,length=2,width=.16,roots=False):
    m.cylinder((-length/2,width,0),(length/2,width,0),width,width*.75,9,1,3)
    if roots:
        for i in range(5):
            a=i*math.tau/5;m.cylinder((-length/2,width,0),(-length/2-.5,abs(math.sin(a))*.4,math.cos(a)*.65),.06,.012,5,1,3)
    else:m.cylinder((-.2,width,0),(.1,width+.23,.34),.065,.025,5,1,3)

def entry(name,kind,**args):return dict(Name=name,Kind=kind,Args=args)
E=entry
KITS=[
 # Region 1 retains all twelve original designs in an isolated, remapped-UV library.
 [E('MeadowBushWide','bush',height=.7,width=1.8),E('MeadowPineBroad','pine',height=6,width=1.25),E('PineSapling','pine',height=1.8,width=.8),E('MeadowFlowerPatch','flowers',width=1.6,height=.4),E('MeadowStoneCluster','cluster',width=1.2,height=.6),E('MeadowBank','bank',height=.9,width=4,length=3),E('DistantMeadowRise','mound',height=3,width=12,length=9),E('MeadowClearingPatch','ground',width=4,length=4)],
 [E('WindBentPine','pine',height=5.8,lean=1.2),E('HillPineYoung','pine',height=3.3,width=.8,lean=.3),E('RidgePine','pine',height=4.6,width=.8,lean=.7),E('WindScrubLow','bush',height=.6,width=1.2),E('WindScrubWide','bush',height=.8,width=2),E('HillDryShrub','drybush',height=.7,width=1.1),E('HillGrass','grass',height=.6),E('DryHillGrass','drygrass',height=.7),E('HillFlowerClump','flowers',height=.35),E('HillBoulder','rock',height=1.1,width=1.1),E('SplitRockCluster','cluster',height=1.3,width=1.7),E('HillRockLedge','rock',height=.65,width=2.2,length=1.2),E('RoundedHill','mound',height=2.8,width=7,length=6),E('WindwardSlope','slope',height=3,width=5,length=6),E('SaddleBank','bank',height=1.7,width=5,length=4),E('DistantHillRidge','ridge',height=6,width=12,length=14),E('HillGroundPatch','ground'),E('GravelShoulder','shoulder'),E('HillClearingPatch','ground',width=4,length=4),E('PebbledEarthPatch','stonepatch')],
 [E('ForestPineTall','pine',height=8,width=.9),E('ForestPineYoung','pine',height=3.8,width=.85),E('CrookedForestPine','pine',height=6.7,lean=.65),E('ShadedBush','bush',height=1.25,width=1.4),E('ForestLowShrub','bush',height=.65,width=1.4),E('ForestFern','fern',height=.65),E('BroadleafClump','fern',height=.4,width=.7),E('ForestGrass','grass',height=.65),E('MossCarpet','moss'),E('MossyBoulder','mossrock',height=1,width=1.4),E('DarkRockCluster','cluster',height=.7,width=1.7),E('RootFan','roots',length=1.3),E('FallenForestTrunk','deadwood',length=2.8,width=.2),E('RaisedRootBank','bank',height=1.3,width=4,length=3),E('ForestSlope','slope',height=2.8,width=5,length=6),E('MistwoodRidge','ridge',height=5,width=11,length=14),E('ForestFloorPatch','ground'),E('LeafLitterPatch','leaves'),E('ForestTrailShoulder','shoulder'),E('ForestClearingPatch','ground',width=4,length=4)],
 [E('OpenWoodlandPine','pine',height=6,width=1.1),E('LeaningHollowPine','pine',height=5.2,lean=.8),E('HollowPineSapling','pine',height=2.4),E('FernFan','fern',height=.8,width=1.1),E('FernClump','fern',height=.6,width=1.5),E('YoungFern','fern',height=.3,width=.5),E('HollowBush','bush',height=1.2,width=1.4),E('LowLeafBush','bush',height=.6,width=1.2),E('GroundIvy','fern',height=.14,width=.8),E('HollowTallGrass','grass',height=.8),E('MossRockCluster','mossrock',height=.7,width=1.3),E('PaleRockOutcrop','rock',height=1.4,width=1.8),E('RootedFallenLog','roots',length=2.3,width=.18),E('HollowRim','bank',height=1.7,width=5,length=4),E('HollowBasin','basin',height=1,width=5,length=5),E('WoodlandBank','bank',height=1.1,width=4,length=3),E('LeafyGroundPatch','ground'),E('HollowLeafLitter','leaves'),E('FernTrailShoulder','shoulder'),E('HollowClearingPatch','ground',width=4,length=4)],
 [E('AlpinePine','pine',height=5.5,width=.8,lean=.25),E('StuntedPine','pine',height=2.5,width=1.1,lean=.5),E('MountainWindPine','pine',height=4.2,width=.75,lean=1),E('AlpineShrub','bush',height=.55,width=1.2),E('CushionPlant','bush',height=.25,width=1),E('MountainGrass','grass',height=.35),E('AlpineDryGrass','drygrass',height=.5),E('AlpineWildflowers','flowers',height=.25),E('MountainBoulder','rock',height=1.5,width=1.3),E('ScreeCluster','cluster',height=.5,width=2),E('RockSpire','rock',height=4,width=1,length=.8),E('LayeredRockLedge','rock',height=1.2,width=3,length=1.8),E('MountainCliffFace','cliff',height=7,width=5,length=7),E('ScreeSlope','slope',height=4,width=5,length=6),E('MountainRidgePeak','ridge',height=12,width=9,length=12),E('DistantMountainRidge','ridge',height=18,width=20,length=24),E('MountainGroundPatch','ground'),E('RockyTrailShoulder','shoulder'),E('SwitchbackBank','bank',height=2.5,width=4,length=4),E('MountainClearingPatch','ground',width=4,length=4)],
 [E('DeltaPine','pine',height=5.3,width=1.05),E('DeltaPineSapling','pine',height=2.3),E('DeltaShrub','bush',height=.9,width=1.5),E('LowMarshBush','bush',height=.55,width=1.2),E('TallReedClump','reeds',height=2),E('ShortReedClump','reeds',height=1.1),E('BentReedClump','bentreeds',height=1.6),E('MarshGrass','grass',height=.8),E('SedgeTuft','grass',height=1.1,width=.6),E('DeltaDriftwood','deadwood',length=2,width=.13),E('DeltaStoneCluster','cluster',height=.35,width=1.3),E('MudBank','bank',height=.65,width=4,length=4),E('LowMudIsland','mound',height=.45,width=5,length=4),E('RiverBankCut','cliff',height=1.1,width=3,length=5),E('DeltaGroundPatch','mud'),E('SandbankPatch','sand'),E('SandyTrailShoulder','shoulder'),E('ShallowWaterTile','water',width=4,length=4),E('RiverWaterTile','water',width=4,length=8),E('DeltaClearingPatch','ground',width=4,length=4)]
]

def make(spec,seed):
    name=spec['Name'];kind=spec['Kind'];args=spec['Args'];m=b.Model(name);rng=random.Random(seed)
    if kind in ('bush','drybush'):bush(m,rng,dry=kind=='drybush',**args)
    elif kind=='pine':pine(m,rng,**args)
    elif kind in ('grass','drygrass','flowers'):grasses(m,rng,dry=kind=='drygrass',flowers=kind=='flowers',**args)
    elif kind=='fern':fern(m,rng,**args)
    elif kind in ('reeds','bentreeds'):reeds(m,rng,bent=kind=='bentreeds',**args)
    elif kind=='rock':rock(m,rng,**args)
    elif kind in ('cluster','mossrock'):cluster(m,rng,moss=kind=='mossrock',**args)
    elif kind in ('mound','bank','slope','ridge','cliff','basin'):landform(m,rng,form=kind,**args)
    elif kind in ('deadwood','roots'):deadwood(m,rng,roots=kind=='roots',**args)
    elif kind=='moss':
        for i in range(8):sphere(m,((i%3-1)*.24,.035,(i//3-1)*.24),(.22,.07,.20),8+i%3,7,3)
    else:
        patch(m,rng,kind={'ground':'grass','stonepatch':'mud'}.get(kind,kind),**args)
        if kind=='stonepatch':cluster(m,rng,height=.06,width=.65)
    m.save();return m

def metadata(name,kind,vertices,triangles,region):
    lo=[min(v[i] for v in vertices) for i in range(3)];hi=[max(v[i] for v in vertices) for i in range(3)]
    height=hi[1]-lo[1]
    category='Tree' if kind=='pine' else 'Shrub' if kind in ('bush','drybush') else 'Rock' if kind in ('rock','cluster','mossrock') else 'Deadwood' if kind in ('deadwood','roots') else 'Water' if kind=='water' else 'Terrain' if kind in ('ground','stonepatch','shoulder','leaves','sand','mud','mound','bank','slope','ridge','cliff','basin') else 'GroundCover'
    distant='Distant' in name or 'RidgePeak' in name
    caster=category=='Tree' or category=='Shrub' and height>1
    radius=math.hypot(hi[0]-lo[0],hi[2]-lo[2])*.5
    return dict(Id=f'env.tier01.region{region:02d}.'+name,Name=name,Category=category,Triangles=triangles,BoundsMin=lo,BoundsMax=hi,FootprintRadius=round(radius,3),CastsShadows=caster and not distant,DistantOnly=distant,RecommendedRoadClearance=round(radius+(8 if category=='Tree' else 3),2),TerrainReplacement=category in ('Terrain','Water'),ResourcePath=f'Environment/Tier01/Region{region:02d}/'+name)

def legacy_region_one(target):
    manifest=json.loads((ROOT/'Assets/_Game/Art/Environment/Region01/EnvironmentManifest.json').read_text());result=[]
    for record in manifest['Models']:
        name=record['Name'];source=ROOT/f'Assets/_Game/Art/Environment/Region01/{name}.obj';lines=source.read_text().splitlines();vertices=[];count=0;vt=0
        for index,line in enumerate(lines):
            if line.startswith('v '):
                point=tuple(map(float,line.split()[1:]))
                if name in ('MeadowGroundPatch','TrailShoulder'):
                    point=(point[0],0,point[2]);lines[index]='v %.6f %.6f %.6f'%point
                vertices.append(point)
            elif line.startswith('vt '):lines[index]=f'vt {(vt+.5)/32:.6f} 0.5';vt+=1
            elif line.startswith('f '):count+=1
        (target/(name+'.obj')).write_text('\n'.join(lines)+'\n',encoding='utf-8')
        kind='pine' if 'Pine' in name else 'bush' if 'Bush' in name or name=='DryShrub' else 'rock' if name=='MeadowBoulder' else 'mound' if name=='GroundMound' else 'ground' if 'Patch' in name else 'shoulder' if name=='TrailShoulder' else 'grass'
        result.append(metadata(name,kind,vertices,count,1))
    return result

if __name__=='__main__':
    allregions=[]
    for index,specs in enumerate(KITS):
        region=index+1;target=OUT/f'Region{region:02d}';target.mkdir(parents=True,exist_ok=True);b.OUT=target;b.COLORS=PALETTES[index]
        models=legacy_region_one(target) if region==1 else []
        for i,spec in enumerate(specs):
            m=make(spec,region*100+i);models.append(metadata(m.name,spec['Kind'],m.vertices,len(m.faces),region))
        b.palette('EnvironmentPalette.png')
        row=dict(Number=region,Name=REGIONS[index],PaletteSize=32,TrailColorRGB=list(PALETTES[index][20]),Models=models)
        (target/'EnvironmentManifest.json').write_text(json.dumps(row,indent=2)+'\n',encoding='utf-8');allregions.append(row)
    (OUT/'Tier01EnvironmentManifest.json').write_text(json.dumps(dict(Revision=1,Regions=allregions,Policy='Reusable decorative art library only. No camp, route, reward, collider, save, graphics-profile or live scatter changes. Readable meshes, opaque shared regional palettes, separate shadow groups. Legacy Region01 remains 24 colors; this kit uses 32 and must not be assigned to its hardcoded-24 terrain UVs.'),indent=2)+'\n',encoding='utf-8')
    print('TOTAL',sum(len(r['Models']) for r in allregions),'models across six region kits')
