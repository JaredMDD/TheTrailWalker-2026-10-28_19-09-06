"""Blockout revision 3 (Claude, November 13, 2026; user: "apply your redesign suggestions").

Builds on revision 2 (tower junctions) and changes only L13-L16:
- L13-L16: the timber front frame is simplified. The concept's centre post no longer runs down through the front door (it
  starts above the door head), and the four diagonal braces across the upper wall plus the two lowest corner braces are
  dropped, so the frame reads as a timber gable instead of scaffolding.
- L15-L16: the stone terrace reads from the street: the ebony rails are replaced by a stone parapet with capped pillars
  that enclose the porch posts, either side of the centre steps.
- L16: the boxed front dormer is replaced by an open belvedere (lantern) on a lower main ridge (11.2 m): drum, open posts
  with rails around a dark core, a pyramid roof and a finial at 12.9 m.
Run: blender --background --python Tools/Art/revise_house_blockouts_r3.py
"""
from pathlib import Path
import sys, json, copy, hashlib
ROOT = Path(__file__).resolve().parents[2]; sys.path.insert(0, str(Path(__file__).parent))
import bpy
import concept_house_exterior_redesign as c
import revise_house_blockouts_r1 as r1
import revise_house_blockouts_r2 as r2

ART = r1.ART; DOC = r1.DOC; REV = DOC / 'Revision03'
LEVELS = {13, 14, 15, 16}
L16_RIDGE = 11.2
BELVEDERE_Z = -1.6
NEW_ELEMENT = {
    13: 'Full-height timber gable frame (posts, king post, rafters); symmetric low lean-to wings',
    14: 'Full-width upper gallery and future paint-band marker',
    15: 'Stone terrace: raised plinth, stone parapet and capped pillars either side of the centre steps; low lean-to wings',
    16: 'Open belvedere (lantern) on a lower ridge, side tower and full stone terraces',
}
current = {'level': 0}


def near(a, b, eps=1e-4):
    return all(abs(x - y) < eps for x, y in zip(a, b))


def patches(original):
    """Wrappers installed on the concept module underneath R1/R2 (they capture whatever is installed as their original)."""
    def box(m, pos, size, col, **kw):
        level = current['level']
        if level >= 13 and near(pos, (0, 5.1, .13)) and near(size, (.17, 7.3, .16)):
            # Centre post: from above the door head (2.47 + trim) to the old top, never through the door.
            return original['box'](m, (0, (2.6 + 8.75) / 2, .13), (.17, 8.75 - 2.6, .16), col, **kw)
        if level == 16 and near(pos, (0, 9.8, .75)) and near(size, (3.0, .18, 1.15)):
            return  # the old dormer's ledge
        return original['box'](m, pos, size, col, **kw)

    def beam(m, a, b, width, col):
        level = current['level']
        if level >= 13:
            for x in (-4, -2, 2, 4):
                if near(a, (x - .65, 5.0, .17)) and near(b, (x + .65, 6.5, .17)):
                    return  # concept diagonals across the upper wall
            for sign in (-1, 1):
                if near(a, (sign * 4.60, .65, .34)) and near(b, (sign * 3.90, 2.55, .34)):
                    return  # R1's lowest corner braces
        if level == 16 and near(b, (0, r1.HEIGHTS[16] - .14, .34)):
            b = (0, L16_RIDGE - .14, .34)  # king post and rafters meet the lowered ridge
        return original['beam'](m, a, b, width, col)

    def mass(m, x, width, bottom, eave, peak, front, back, wood, wall, roofcolor='Slate'):
        level = current['level']
        if level == 16 and x == 0 and width == 11 and abs(peak - 12.5) < 1e-6:
            peak = L16_RIDGE
        if level == 16 and x == 0 and abs(width - 2.80) < 1e-6 and abs(bottom - 9.8) < 1e-6:
            return  # the old boxed dormer
        return original['mass'](m, x, width, bottom, eave, peak, front, back, wood, wall, roofcolor)

    def window(m, x, y, z, w=1.0, height=1.15, wood='WoodDark'):
        if current['level'] == 16 and x == 0 and abs(y - 10.8) < 1e-6:
            return  # the old dormer's window
        return original['window'](m, x, y, z, w, height, wood)

    def rail(m, x, width, y, z, wood):
        if current['level'] >= 15 and abs(y - 1.04) < 1e-6 and abs(width - 3.1) < 1e-6:
            return  # ebony terrace rails, replaced by the stone parapet
        return original['rail'](m, x, width, y, z, wood)

    return dict(box=box, beam=beam, mass=mass, window=window, rail=rail)


def terrace(m, box):
    """Stone parapet and capped pillars on the L15/L16 plinth (top 0.32 m, front face z = 1.2 m)."""
    for sign in (-1, 1):
        for x in (1.63, 4.89):  # the porch posts stand inside these pillars
            box(m, (sign * x, .86, 1.06), (.40, 1.08, .40), 'Stone')
            box(m, (sign * x, 1.46, 1.06), (.50, .12, .50), 'Stone')
        box(m, (sign * 3.26, .66, 1.06), (2.86, .68, .22), 'Stone')


def belvedere(m, box, wood):
    z = BELVEDERE_Z
    box(m, (0, 10.5, z), (2.4, 1.0, 2.4), 'FinePlaster')          # drum rising out of the roof
    box(m, (0, 11.07, z), (2.8, .14, 2.8), wood)                   # floor slab and cornice
    box(m, (0, 11.57, z), (1.9, .86, 1.9), 'GlassDark')            # dark core: the openings read as a lantern
    for sx in (-1, 1):
        for sz in (-1, 1):
            box(m, (sx * 1.15, 11.57, z + sz * 1.15), (.18, .86, .18), wood)
    for sz in (-1, 1):
        box(m, (0, 11.45, z + sz * 1.15), (2.3, .07, .07), wood)
        box(m, (sz * 1.15, 11.45, z), (.07, .07, 2.3), wood)
    # Pyramid roof: eaves at 12.0 m, apex 12.75 m, slate faces and a dark soffit.
    half, eave, apex = 1.5, 12.0, 12.75
    corners = [(-half, eave, z - half), (half, eave, z - half), (half, eave, z + half), (-half, eave, z + half)]
    ids = [m.v(p) for p in corners]; top = m.v((0, apex, z))
    for i in range(4):
        m.tri(ids[i], ids[(i + 1) % 4], top, 'Slate', (0, eave - .3, z))
    m.quad(*ids, 'WoodDark', (0, apex + 1, z))
    box(m, (0, 12.82, z), (.12, .16, .12), wood)                   # finial, top at 12.9 m


def revised(level, original):
    current['level'] = level
    mesh, row, ledger = r2.revised(level)
    if level >= 15: terrace(mesh, original['box'])
    if level == 16: belvedere(mesh, original['box'], row['Wood'])
    c.measure(mesh, row)
    return mesh, row


def main():
    REV.mkdir(parents=True, exist_ok=True)
    base = json.loads((r2.REV / 'before/HouseBlockoutManifest.json').read_text())
    rows = copy.deepcopy(base['ExteriorBlockouts']); models = []
    original = {name: getattr(c, name) for name in ['box', 'beam', 'mass', 'window', 'rail']}
    try:
        for name, fn in patches(original).items(): setattr(c, name, fn)
        for i, old in enumerate(rows):
            level = old['Level']
            if level in r2.LEVELS or level in LEVELS:
                if level in LEVELS:
                    mesh, new = revised(level, original)
                    stage, revision = 'Blockout revision 3', 3
                else:
                    current['level'] = level
                    mesh, new, _ = r2.revised(level)
                    stage, revision = 'Blockout revision 2', 2
                mesh.name = old['Name']
                mesh.save(ART, 'House exterior blockout R%d; review only; metres; +Y up; +Z front' % revision)
                for key in ['GroundAnchor', 'DoorAnchor', 'SelectAnchor', 'PetHomeAnchor', 'CollectionAnchor', 'HomeCameraAnchor']: new.pop(key, None)
                row = copy.deepcopy(old); row.update(new); row.update(Name=old['Name'], Stage=stage, Revision=revision)
                if level in NEW_ELEMENT: row['NewElement'] = NEW_ELEMENT[level]
                rows[i] = row
            else:
                mesh = r1.read_mesh(old)
            models.append(mesh)
    finally:
        for name, fn in original.items(): setattr(c, name, fn)
        current['level'] = 0
    assert all(rows[i]['Height'] >= rows[i - 1]['Height'] for i in range(1, 16))
    manifest = copy.deepcopy(base)
    manifest.update(Revision=4, Scope='Stage 2 revision 3 (Claude): L13-L16 frame, L15-L16 stone terrace, L16 belvedere; R2 junctions kept',
                    RevisedLevels=sorted(LEVELS | r2.LEVELS), RevisionSourceSHA256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(), ExteriorBlockouts=rows)
    (ART / 'HouseBlockoutManifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    hp = ART.parent / 'HouseManifest.json'; hm = json.loads(hp.read_text()); hm['ExteriorBlockouts'] = rows; hp.write_text(json.dumps(hm, indent=2) + '\n')

    bpy.ops.wm.read_factory_settings(use_empty=True); bpy.context.preferences.filepaths.save_version = 0; bpy.context.preferences.filepaths.file_preview_type = 'NONE'
    mat = bpy.data.materials.new('SharedBlockoutGrey'); mat.use_nodes = True; bs = mat.node_tree.nodes['Principled BSDF']; bs.inputs['Roughness'].default_value = .85
    tex = mat.node_tree.nodes.new('ShaderNodeTexImage'); tex.image = bpy.data.images.load(str(ART / 'BlockoutPalette.png')); tex.image.alpha_mode = 'CHANNEL_PACKED'; tex.image.pack(); tex.interpolation = 'Closest'
    mat.node_tree.links.new(tex.outputs['Color'], bs.inputs['Base Color'])
    for i, (mesh, row) in enumerate(zip(models, rows)):
        obj = c.native(mesh, mat)
        for poly, f in zip(obj.data.polygons, mesh.faces):
            for li in poly.loop_indices: obj.data.uv_layers.active.data[li].uv = ((f[3] + .5) / c.pal.slots, .5)
        obj.location.x = i * 18
        for key in ['GroundAnchor', 'DoorAnchor', 'SelectAnchor', 'PetHomeAnchor', 'CollectionAnchor', 'HomeCameraAnchor']:
            x, y, z = row[key]; o = bpy.data.objects.new(row['Name'] + '::' + key, None); bpy.context.scene.collection.objects.link(o); o.parent = obj; o.location = (x, -z, y)
    bpy.ops.wm.save_as_mainfile(filepath=str(DOC / 'house-exterior-blockouts.blend'))
    print('R3_SOURCES', [(r['Level'], round(r['Height'], 2), r['Triangles']) for r in rows if r['Level'] >= 12])


if __name__ == '__main__': main()
