"""Closed, deterministic geometry helpers for faceted tier-two tools."""
import bpy,bmesh
from mathutils import Vector,geometry


def beveled_profile(m,points,depth,face_slot,edge_slot,bevel=.012):
    """Concave XY profile with true closed side walls and evaluated bevels."""
    n=len(points)
    vertices=[(x,y,z) for z in (-depth/2,depth/2) for x,y in points]
    lookup={(x,y):i for i,(x,y) in enumerate(points)}
    faces=[];slots=[]
    for triangle in geometry.tessellate_polygon([[Vector((x,y,0)) for x,y in points]]):
        indices=list(triangle) if isinstance(triangle[0],int) else [lookup[(v.x,v.y)] for v in triangle]
        faces+=[tuple(indices[::-1]),tuple(i+n for i in indices)];slots +=[face_slot,face_slot]
    for i in range(n):
        j=(i+1)%n;faces.append((i,j,j+n,i+n));slots.append(edge_slot)
    mesh=bpy.data.meshes.new('ProfileConstruction');mesh.from_pydata(vertices,[],faces);mesh.update()
    obj=bpy.data.objects.new('ProfileConstruction',mesh);bpy.context.collection.objects.link(obj)
    # Real slots let bevel faces inherit the intended edge palette index.
    for k in range(max(face_slot,edge_slot)+1):mesh.materials.append(bpy.data.materials.new('ConstructionSlot'+str(k)))
    for p,slot in zip(mesh.polygons,slots):p.material_index=slot
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    bpy.context.view_layer.objects.active=obj
    mod=obj.modifiers.new('RoundedWorkedEdges','BEVEL');mod.width=bevel;mod.segments=2;mod.material=edge_slot
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.triangulate(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free()
    offset=len(m.vertices)
    for v in mesh.vertices:m.v(tuple(v.co))
    for p in mesh.polygons:m.tri(*[i+1+offset for i in p.vertices],p.material_index)
    bpy.data.objects.remove(obj,do_unlink=True);bpy.data.meshes.remove(mesh)
