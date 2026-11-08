"""Batch 13: Beaver-fur cap and Marsh boots, display + male/female fitted art."""
import sys,math,json
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import bpy,bmesh
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
import faceted_people as fp
import faceted_character_kit as k
from generate_tier02_batch06 import tube
from generate_tier02_batch02 import stroke
from tier02_batch_workflow import generate,ROOT

COLORS=[(109,76,48),(82,56,37),(105,65,40),(201,180,133),(200,172,120),
        (153,122,77),(138,92,53),(71,46,31)]
TYPES=['fur','fur','leather','plant_fibre','wood','wood','leather','leather']


def append(m,other,transform=lambda p:p):
    off=len(m.vertices);m.vertices +=[transform(p) for p in other.vertices]
    m.faces +=[(a+off,b+off,c+off,col) for a,b,c,col in other.faces]


def cap(female=False):
    m=base.Model(('Female' if female else 'Male')+'WearBeaverFurCap');n=24
    sy=fp.head_sections(female)[-1][0]/.270
    rows=[(.194,.100,.122,.130),(.211,.101,.122,.130),(.218,.098,.117,.125),
          (.248,.084,.095,.108),(.274,.058,.067,.082),(.292,.024,.034,.045)]
    skins=[]
    for inner in (False,True):
        rings=[]
        for y,w,f,b in rows:
            shrink=.004 if inner else 0
            rings.append([m.v(((w-shrink)*math.cos(i*math.tau/n),(y-(.003 if inner else 0))*sy,
                               ((f-shrink) if math.sin(i*math.tau/n)>0 else (b-shrink))*math.sin(i*math.tau/n))) for i in range(n)])
        for r in range(len(rings)-1):
            for i in range(n):
                j=(i+1)%n;col=1 if inner else 0 if (i+r)%5 else 1
                m.tri(rings[r][i],rings[r][j],rings[r+1][j],col);m.tri(rings[r][i],rings[r+1][j],rings[r+1][i],col)
        top=m.v((0,(.299-(.004 if inner else 0))*sy,-.006))
        for i in range(n):m.tri(top,rings[-1][i],rings[-1][(i+1)%n],1 if inner else 0)
        skins.append(rings)
    for i in range(n):
        j=(i+1)%n;m.tri(skins[0][0][i],skins[0][0][j],skins[1][0][j],2);m.tri(skins[0][0][i],skins[1][0][j],skins[1][0][i],2)
    # Fine hemp-thread seam follows the actual front meridian, not a floating stripe.
    points=[(0,y*sy,f+.001) for y,w,f,b in rows[2:]]
    tube(m,points,.0011,3,5)
    for y,z in [(.226,.112),(.239,.102),(.254,.090),(.267,.076)]:
        stroke(m,[(-.003,y*sy,z+.002),(.003,(y+.002)*sy,z+.001)],3,.0008)
    return m


def mesh_object(model,name):
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(model.vertices,[],[(a-1,b-1,c-1) for a,b,c,col in model.faces]);mesh.update()
    for i in range(len(COLORS)):mesh.materials.append(bpy.data.materials.new(name+'Slot'+str(i)))
    for p,f in zip(mesh.polygons,model.faces):p.material_index=f[3]
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);return obj


def ankle_solid(name,profile,slot):
    m=base.Model(name);kit.loft(m,profile,slot,24);return m


def boot_upper():
    # Longitudinal foot sections resolve heel, instep, ball and tapered rounded toe.
    foot=base.Model('FootUpper');rings=[]
    for z,w,cy,top,low in [(-.077,.042,-.037,.040,.028),(-.035,.053,-.028,.076,.037),
                           (.020,.058,-.040,.064,.029),(.090,.061,-.050,.044,.023),
                           (.165,.053,-.060,.027,.018),(.207,.036,-.062,.017,.014)]:
        # Lower perimeter seats into the shared sole plane, avoiding a floating heel.
        rings.append([foot.v((x,min(cy+y,-.078) if y<0 else cy+y,z)) for x,y in k.chamfer_rect(w,top,low,.70,.62)])
    for a,b in zip(rings,rings[1:]):
        for i in range(8):
            j=(i+1)%8;foot.tri(a[i],a[j],b[j],2);foot.tri(a[i],b[j],b[i],2)
    for ring,z in [(rings[0],-.077),(rings[-1],.207)]:
        c=foot.v((0,sum(foot.vertices[i-1][1] for i in ring)/8,z))
        for i in range(8):foot.tri(c,ring[i],ring[(i+1)%8],2)
    outer=ankle_solid('Shaft',[(-.052,0,0),(-.052,.059,.066),(.085,.070,.077),(.14,.072,.079),(.14,0,0)],2)
    hole=ankle_solid('Interior',[(-.012,0,0),(-.012,.052,.058),(.085,.063,.070),(.19,.065,.072),(.19,0,0)],7)
    obj=mesh_object(foot,'BootContinuousUpper');shaft=mesh_object(outer,'AnkleConstruction');cutter=mesh_object(hole,'AnkleInterior')
    bpy.context.view_layer.objects.active=obj
    for other,operation in [(shaft,'UNION'),(cutter,'DIFFERENCE')]:
        mod=obj.modifiers.new('ConnectedAnkle'+operation,'BOOLEAN');mod.operation=operation;mod.solver='EXACT';mod.object=other
        bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(other,do_unlink=True)
    bevel=obj.modifiers.new('WorkedLeatherEdges','BEVEL');bevel.width=.002;bevel.segments=1
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    bm=bmesh.new();bm.from_mesh(obj.data)
    # Boolean intersections can leave coincident micron-scale corners; clean locally.
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.00001)
    bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.000001)
    bmesh.ops.triangulate(bm,faces=list(bm.faces));bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(obj.data);bm.free()
    m=base.Model('BootUpper')
    for v in obj.data.vertices:m.v(tuple(v.co))
    for p in obj.data.polygons:m.tri(*[i+1 for i in p.vertices],2)
    bpy.data.objects.remove(obj,do_unlink=True)
    return m


def boot():
    m=base.Model('MarshBoot');append(m,boot_upper())
    # Sole follows the foot footprint, including a narrower heel and rounded toe.
    outline=[(-.044,-.083),(.044,-.083),(.063,-.040),(.068,.080),(.062,.155),
             (.040,.218),(-.040,.218),(-.062,.155),(-.068,.080),(-.063,-.040)]
    rings=[]
    for y,scale in [(-.10,.96),(-.096,1),(-.077,1),(-.073,.96)]:
        rings.append([m.v((x*scale,y+(max(0,z-.15)*.07),z)) for x,z in outline])
    for r in range(3):
        for i in range(len(outline)):
            j=(i+1)%len(outline);col=5 if r==0 else 4
            m.tri(rings[r][i],rings[r][j],rings[r+1][j],col);m.tri(rings[r][i],rings[r+1][j],rings[r+1][i],col)
    for ring,y in [(rings[0],-.10),(rings[-1],-.073)]:
        c=m.v((0,y,.06))
        for i in range(len(outline)):m.tri(c,ring[i],ring[(i+1)%len(outline)],4)
    # Rolled leather opening, with real inner/outer wall and no fur cuff.
    cuff=base.Model('LeatherCuff');n=24;rings=[]
    for y,rx,rz in [(.123,.074,.081),(.143,.074,.081),(.123,.064,.071),(.143,.064,.071)]:
        rings.append([cuff.v((rx*math.cos(i*math.tau/n),y,rz*math.sin(i*math.tau/n))) for i in range(n)])
    for a,b in [(0,1),(2,3),(0,2),(1,3)]:
        for i in range(n):
            j=(i+1)%n;cuff.tri(rings[a][i],rings[a][j],rings[b][j],6);cuff.tri(rings[a][i],rings[b][j],rings[b][i],6)
    append(m,cuff)
    # Leather seams project onto actual vamp through a Y-up ray transformation.
    support=base.Model('VampSupport');support.vertices=[(x,z,-y) for x,y,z in m.vertices];support.faces=list(m.faces)
    def seat(x,z):
        q=kit.front_surface(support,x,z,.001);return (q[0],-q[2],q[1])
    tube(m,[seat(x,.11+.012*(1-abs(x)/.045)) for x in [-.045,-.03,0,.03,.045]],.0015,7,5)
    for sign in (-1,1):
        tube(m,[(sign*.066,y,-.012) for y in [.015,.06,.11]],.0015,6,5)
    return m


def worn_boots(female=False):
    m=base.Model(('Female' if female else 'Male')+'WearMarshBoots')
    one=boot()
    # Current male/female avatars share the same foot/ankle landmarks, verified separately.
    for sign in (-1,1):append(m,one,lambda p:(p[0]+sign*fp.HIP_X,p[1]+fp.ANKLE_HEIGHT,p[2]))
    return m


def display(which):
    m=cap(False) if which=='cap' else boot();m.name='BeaverFurCap' if which=='cap' else 'MarshBoots'
    if which=='boot':m.vertices=[(z*2.8,y*2.8,-x*2.8) for x,y,z in m.vertices]
    else:m.vertices=[(x*3,y*3,-z*3) for x,y,z in m.vertices]
    center=[(min(v[i] for v in m.vertices)+max(v[i] for v in m.vertices))/2 for i in range(3)]
    m.vertices=[tuple(v[i]-center[i] for i in range(3)) for v in m.vertices]
    return m


SPECS=[('BeaverFurCap','Beaver-fur cap',lambda:display('cap'),COLORS,TYPES),
       ('MarshBoots','Marsh boots',lambda:display('boot'),COLORS,TYPES)]
METADATA={}
for female in (False,True):
    body='Female' if female else 'Male'
    for part,label,build,anchor,item_id in [
        ('WearBeaverFurCap','Beaver-fur cap',lambda female=female:cap(female),'HeadAnchor/HatAnchor','item.beaver_fur_cap'),
        ('WearMarshBoots','Marsh boots',lambda female=female:worn_boots(female),'BodyRoot; split LeftBoot/RightBoot for runtime','item.marsh_boots')]:
        name=body+part;SPECS.append((name,label+' ('+body+' fitted)',build,COLORS,TYPES))
        METADATA[name]={'Representation':'worn','Body':body,'Anchor':anchor,'ItemId':item_id,'BindingStatus':'Authored item identity only; no live equipment binding/weights/animation'}
for name,item in [('BeaverFurCap','beaver_fur_cap'),('MarshBoots','marsh_boots')]:
    METADATA[name]={'Representation':'display','ItemId':'item.'+item,'RecipeId':'recipe.wearable.'+item}
RECIPES={name:(('T2-G8',9,'2 Fine beaver pelts + 1 Hemp thread -> 1') if 'Cap' in name else
               ('T2-L6',9,'2 Beaver leather + 1 Birch plank -> 1')) for name,*_ in SPECS}


if __name__=='__main__':
    generate(13,SPECS,RECIPES,category='Wearables',role='Wearable',preview='--preview' in sys.argv,item_metadata=METADATA)
    artmap={'Scope':'Passive wearable art; no live binding or item footprint changes','HeadAnchor':[0,1.43,0],
            'HatCompatibleHairReuse':'Characters/Tier01Accessories/Parts/{Body}HatHair{Style}',
            'HairStyles':list(fp.HAIR_STYLES),'HairPolicy':'Keep saved style/palette; substitute existing compatible geometry while cap shown, restore original on removal.',
            'Bodies':['Male','Female'],'BootFrame':'Body-root metre coordinates; pair; +Z forward; ankle anchors at (+/-0.10,0.10,0)',
            'Items':METADATA,'ValidationLimit':'Static fitted art and rigid pose studies; not skinned animation or runtime equip validation.'}
    (ROOT/'Assets/_Game/Art/Crafting/Tier02/Tier02Batch13ArtMap.json').write_text(json.dumps(artmap,indent=2)+'\n')
