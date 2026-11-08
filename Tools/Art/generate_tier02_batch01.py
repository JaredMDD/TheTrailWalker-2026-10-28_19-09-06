"""Blender batch 01: three passive tier-two item assets; no gameplay writes.

Run Blender --background --factory-startup --python this_file.py.
OBJ interchange uses the project's Y-up coordinates; the native scene rotates
the three objects to Blender Z-up. Only new batch paths are generated.
"""
import sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(Path(__file__).parent))
import hashlib
import json
import math
import random
import bpy
import bmesh
from mathutils import Vector
import generate_region01_meshes as base
import detail_textures as detail

ART = ROOT / 'Assets/_Game/Art/Harvest/Tier02'
PAL = ROOT / 'Assets/_Game/Art/Items/Tier02Design/Palettes'
DOC = ROOT / 'Docs/Art/Tier02/Batch01'


def loft(m, rows, color, sides=16, along_x=False):
    """Closed loft with shared poles, avoiding coincident zero-radius rings."""
    rings = []
    for position, ry, rz in rows:
        if ry == 0 or rz == 0:
            rings.append([m.v((position, 0, 0) if along_x else (0, position, 0))])
        else:
            rings.append([m.v((position, ry*math.cos(i*math.tau/sides), rz*math.sin(i*math.tau/sides)) if along_x
                              else (ry*math.cos(i*math.tau/sides), position, rz*math.sin(i*math.tau/sides))) for i in range(sides)])
    for k, (a, b) in enumerate(zip(rings, rings[1:])):
        for i in range(sides):
            j = (i+1) % sides
            c = color(k, i) if callable(color) else color
            if len(a) == 1:
                m.tri(a[0], b[j], b[i], c)
            elif len(b) == 1:
                m.tri(a[i], a[j], b[0], c)
            else:
                m.tri(a[i], a[j], b[j], c); m.tri(a[i], b[j], b[i], c)


def closed_patch(m, points, normal, thickness, color):
    lower = [m.v(base.sub(p, base.mul(normal, thickness))) for p in points]
    upper = [m.v(p) for p in points]
    for i in range(1, len(points)-1):
        m.tri(upper[0], upper[i], upper[i+1], color)
        m.tri(lower[0], lower[i+1], lower[i], color)
    for i in range(len(points)):
        j = (i+1) % len(points)
        m.tri(upper[i], lower[i], lower[j], color); m.tri(upper[i], lower[j], upper[j], color)


def front_surface(m, x, y, offset=.001):
    """Project relief onto the actual triangulated front, not a guessed radius."""
    candidates=[]
    for a,b,c,_ in m.faces:
        p,q,r=(m.vertices[i-1] for i in (a,b,c))
        determinant=(q[1]-r[1])*(p[0]-r[0])+(r[0]-q[0])*(p[1]-r[1])
        if abs(determinant)<1e-10:continue
        u=((q[1]-r[1])*(x-r[0])+(r[0]-q[0])*(y-r[1]))/determinant
        v=((r[1]-p[1])*(x-r[0])+(p[0]-r[0])*(y-r[1]))/determinant
        w=1-u-v
        if min(u,v,w)>=-1e-6:candidates.append(u*p[2]+v*q[2]+w*r[2])
    if not candidates:raise ValueError('Relief has no supporting surface')
    return (x,y,min(candidates)-offset)


def birch_log():
    m = base.Model('BirchLog')
    start, end = (-.57, 0, -.13), (.57, 0, .13)
    axis = base.unit(base.sub(end, start)); u = base.unit(base.cross(axis, (0,1,0))); v = base.cross(axis,u)
    m.cylinder(start, end, .305, .285, 18, 2, 4)
    # Cut-face growth rings follow each end plane and stay seated on the face.
    for center, sign, radius in [(start,-1,.305),(end,1,.285)]:
        center = base.add(center,base.mul(axis,sign*.0015))
        for r in (.065,.128,.197,.258):
            ring=[]
            for width in (-.003,.003):
                ring.append([m.v(base.add(center,base.add(base.mul(u,(r+width)*math.cos(i*math.tau/32)),base.mul(v,(r+width)*math.sin(i*math.tau/32))))) for i in range(32)])
            for i in range(32):
                j=(i+1)%32
                m.tri(ring[0][i],ring[0][j],ring[1][j],5);m.tri(ring[0][i],ring[1][j],ring[1][i],5)
        for angle in (.6,2.8,4.7):
            d=base.add(base.mul(u,math.cos(angle)),base.mul(v,math.sin(angle)))
            m.cylinder(base.add(center,base.mul(d,.21)),base.add(center,base.mul(d,radius*.96)),.003,.0018,4,5,5)
    # Small lenticels wrap AROUND the log, never a lengthwise dark cylinder stripe.
    rng=random.Random(207)
    for row in range(7):
        t=.09+row*.13
        center=base.add(start,base.mul(base.sub(end,start),t)); radius=.305+(.285-.305)*t
        for mark in range(7):
            angle=mark*math.tau/7+row*.41+rng.uniform(-.1,.1)
            normal=base.add(base.mul(u,math.cos(angle)),base.mul(v,math.sin(angle)))
            tangent=base.add(base.mul(u,-math.sin(angle)),base.mul(v,math.cos(angle)))
            point=base.add(center,base.mul(normal,radius+.001))
            width=rng.uniform(.012,.026); length=rng.uniform(.028,.067)
            pts=[base.add(point,base.add(base.mul(axis,x),base.mul(tangent,y))) for x,y in [(-width,-length),(width,-length*.85),(width,length),(-width,length*.75)]]
            closed_patch(m,pts,normal,.007,3 if mark%3 else 0)
    # One trimmed branch scar, joined to the bark sleeve.
    m.cylinder((.16,.255,.02),(.18,.35,.02),.07,.055,10,2,4)
    return m


def birch_sap():
    m=base.Model('BirchSap')
    # Opaque stylized glass shows the pale fill through its lower color bands.
    profile=[(-.40,0,0),(-.40,.19,.15),(-.36,.28,.20),(-.27,.30,.22),(.10,.30,.22),(.22,.26,.20),(.30,.13,.12),(.37,.12,.11),(.39,.15,.13),(.43,.15,.13),(.44,0,0)]
    loft(m,profile,lambda k,i: (0 if k<4 else 1) if k<7 else 2,18)
    # Birch-wood stopper, with a beveled top and no floating closure.
    loft(m,[(.405,0,0),(.405,.105,.095),(.465,.11,.10),(.485,.09,.083),(.485,0,0)],3,12)
    for y in (.29,.32):
        # Closed collar of coarse fiber around the neck.
        loft(m,[(y-.007,0,0),(y-.007,.146,.124),(y+.007,.146,.124),(y+.007,0,0)],4,18)
    # Birch leaf motif is an illustrative identity cue, not a new ingredient.
    outline=[(-.11,-.08),(-.04,.07),(.10,.13),(.09,-.02),(0,-.12)]
    support=base.Model('SapSupport');support.vertices=list(m.vertices);support.faces=list(m.faces)
    # Tessellate the label over the actual bottle triangles. A large flat fan
    # sinks into the curved body even when its boundary vertices are seated.
    center=(.015,-.005);steps=4
    for index in range(len(outline)):
        a,b,c=outline[index],outline[(index+1)%len(outline)],center
        def sample(i,j):
            u=i/steps;v=j/steps
            return front_surface(support,(1-u-v)*a[0]+u*b[0]+v*c[0],(1-u-v)*a[1]+u*b[1]+v*c[1],.004)
        for i in range(steps):
            for j in range(steps-i):
                closed_patch(m,[sample(i,j),sample(i+1,j),sample(i,j+1)],(0,0,-1),.012,5)
                if i+j<steps-1:
                    closed_patch(m,[sample(i+1,j),sample(i+1,j+1),sample(i,j+1)],(0,0,-1),.012,5)
    vein=[front_surface(m,x,y,.002) for x,y in [(-.085,-.09),(-.03,-.025),(.02,.025),(.065,.08)]]
    for p,q in zip(vein,vein[1:]):m.cylinder(p,q,.003,.0025,5,6,6)
    # Restrained opaque-glass highlights, no transparent material dependency.
    for x in (-.20,.20):
        points=[front_surface(m,x,y,.002) for y in (-.25,-.10,.05,.11)]
        for p,q in zip(points,points[1:]):m.cylinder(p,q,.006,.005,5,7,7)
    # Match the existing 700 x 800 runtime footprint in the front silhouette.
    m.fit_bounds((-.35,-.4,-.23),(.35,.4,.23))
    return m


def raw_beaver_meat():
    m=base.Model('RawBeaverMeat')
    rows=[(-.46,0,0),(-.41,.11,.10),(-.29,.21,.16),(-.12,.255,.20),(.10,.245,.19),(.29,.20,.15),(.40,.11,.08),(.44,0,0)]
    def color(k,i):
        theta=(i+.5)*math.tau/20
        if math.cos(theta)>.64 and math.sin(theta)<.25:return 4 if k%3 else 5
        return [0,1,2,1,3,1,2][k]
    loft(m,rows,color,20,True)
    # Fine connective lines lie partially embedded in the actual front surface.
    for x,ry,rz in [(-.25,.222,.17),(-.09,.253,.199),(.08,.247,.192),(.25,.211,.159)]:
        points=[]
        for j,y in enumerate((-.10,-.025,.055,.12)):
            px=x+(.008 if j%2 else -.008)
            points.append(front_surface(m,px,y,.001))
        for p,q in zip(points,points[1:]):m.cylinder(p,q,.0045,.0035,5,6,6)
    return m


DEFINITIONS=[
    ('BirchLog','Birch log','',birch_log,[(53,47,40),(210,204,180),(236,231,210),(75,68,59),(200,160,101),(139,105,65)],['bark','bark','bark','bark','wood','wood']),
    ('BirchSap','Birch sap','ingredient.birch_sap',birch_sap,[(182,208,201),(142,182,184),(203,222,212),(182,143,88),(152,127,82),(92,123,62),(198,190,136),(222,237,224)],['glass','glass','glass','wood','plant_fibre','flat','flat','glass']),
    ('RawBeaverMeat','Raw beaver meat','',raw_beaver_meat,[(126,43,45),(149,52,54),(113,36,43),(165,67,65),(221,199,164),(201,171,142),(188,125,111)],['flat','flat','flat','flat','flat','flat','flat']),
]


def create_asset(name, colors, types, model):
    mesh=bpy.data.meshes.new(name+'Mesh')
    mesh.from_pydata(model.vertices,[],[(a-1,b-1,c-1) for a,b,c,k in model.faces]);mesh.update()
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    uv=mesh.uv_layers.new(name='PaletteUV')
    for poly,face in zip(mesh.polygons,model.faces):
        for loop in poly.loop_indices:uv.data[loop].uv=((face[3]+.5)/len(colors),.5)
        poly.use_smooth=False
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj);obj.rotation_euler[0]=math.pi/2
    palette=PAL/(name+'Palette.png');pixels=[(*rgb,detail.alpha(t)) for rgb,t in zip(colors,types)]
    detail.write_png(palette,len(colors)*4,4,[[p for p in pixels for _ in range(4)] for _ in range(4)])
    mat=bpy.data.materials.new(name+'Palette');mat.use_nodes=True
    nodes=mat.node_tree.nodes;bsdf=nodes.get('Principled BSDF');bsdf.inputs['Roughness'].default_value=.32 if name=='BirchSap' else .72
    tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(palette));tex.interpolation='Closest'
    # Alpha encodes a detail TYPE, never opacity or a premultiplied RGB mask.
    tex.image.alpha_mode='CHANNEL_PACKED';tex.image.pack()
    mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);mesh.materials.append(mat)
    # Export actual corrected topology, local Y-up vertices, with palette UVs.
    obj_path=ART/(name+'.obj')
    text=['# TheTrailWalker batch 01; local Y up; flat palette-mapped item mesh.','o '+name]
    text+=['v %.7f %.7f %.7f'%tuple(v.co) for v in mesh.vertices]
    text+=['vt %.7f 0.5'%((i+.5)/len(colors)) for i in range(len(colors))];text+=['s off']
    for poly,face in zip(mesh.polygons,model.faces):
        text.append('f '+' '.join(f'{i+1}/{face[3]+1}' for i in poly.vertices))
    obj_path.write_text('\n'.join(text)+'\n',encoding='utf-8')
    return obj,dict(Name=name,Model=name,SourcePath=obj_path.relative_to(ROOT).as_posix(),PalettePath=palette.relative_to(ROOT).as_posix(),
                    PrefabPath='Tier02/Drops/'+name,Triangles=len(mesh.polygons),
                    BoundsMin=[round(min(v.co[i] for v in mesh.vertices),6) for i in range(3)],
                    BoundsMax=[round(max(v.co[i] for v in mesh.vertices),6) for i in range(3)])


def aim(obj, point):obj.rotation_euler=(Vector(point)-obj.location).to_track_quat('-Z','Y').to_euler()


def main():
    for p in [ART,PAL,DOC]:p.mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16
    scene.render.resolution_x=640;scene.render.resolution_y=640;scene.render.resolution_percentage=100
    scene.world=bpy.data.worlds.new('SlateWorld');scene.world.use_nodes=True
    scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.19,.22,.24,1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value=.45
    scene.view_settings.view_transform='Standard';scene.view_settings.look='None';scene.view_settings.exposure=0
    cam_data=bpy.data.cameras.new('ReviewCamera');camera=bpy.data.objects.new('ReviewCamera',cam_data);scene.collection.objects.link(camera);scene.camera=camera;cam_data.type='ORTHO'
    for name,pos,power,size in [('Key',(-3,-4,6),450,4),('Fill',(4,-2,3),260,4),('Rim',(1,4,4),330,3)]:
        data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size
        light=bpy.data.objects.new(name,data);scene.collection.objects.link(light);light.location=pos;aim(light,(0,0,0))
    objects=[];items=[]
    for index,(name,label,item_id,build,colors,types) in enumerate(DEFINITIONS):
        obj,row=create_asset(name,colors,types,build());obj.location.x=(index-1)*1.65
        row.update(DisplayName=label,ItemId=item_id,RuntimeBinding='Not installed; log/meat IDs unresolved' if not item_id else 'Existing definition retained; art binding pending')
        obj['AssetRole']='passive inventory art';obj['CanonicalItemId']=item_id
        objects.append(obj);items.append(row)
    manifest=dict(Revision=1,Batch=1,Scope='Three passive art assets; no recipe/stat/source/footprint edits.',BlenderVersion=bpy.app.version_string,Items=items)
    (ART/'Tier02Batch01Manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    directions={'front':(0,4,0),'back':(0,-4,0),'left':(-4,0,0),'right':(4,0,0),'top':(0,.001,4),'study':(2.7,4,2)}
    for obj in objects:
        for other in objects:other.hide_render=other!=obj
        bpy.context.view_layer.update();bounds=[obj.matrix_world@Vector(v) for v in obj.bound_box]
        center=sum(bounds,Vector())/8
        extent=max((max(v[i] for v in bounds)-min(v[i] for v in bounds)) for i in range(3))
        cam_data.ortho_scale=extent*1.35
        for view,direction in directions.items():
            camera.location=center+Vector(direction);aim(camera,center)
            scene.render.filepath=str(DOC/(obj.name+'-blender-'+view+'.png'));bpy.ops.render.render(write_still=True)
    for obj in objects:obj.hide_render=False
    camera.location=(2,6,3);aim(camera,(0,0,0));cam_data.ortho_scale=5.3
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC/'tier02-batch01.blend'))
    print('BATCH01_GENERATED: three meshes/palettes, native scene, 18 rendered review views.')


if __name__=='__main__':main()
