"""Isolated tier-one accessory art. Standard-library OBJ pipeline, no Unity calls.

Run: python Tools/Art/generate_tier01_fitted_accessories.py
Imports geometry helpers only; never calls another generator's main/save/build.
"""
from pathlib import Path
import hashlib
import json
import sys
sys.dont_write_bytecode = True  # Helpers are read-only, including their cache directories.
import faceted_character_kit as k
import faceted_people as fp
import generate_player_avatar_meshes as avatar

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Art/Characters/Tier01Accessories'
RESOURCES = ROOT / 'Assets/_Game/Resources/Characters/Tier01Accessories'
COLORS = {**avatar.WEAR_COLORS, 'FrameWood': '#9E6D3F', 'FrameEnd': '#CCA66B', 'Hair': avatar.HAIRS[1], 'Skin': avatar.SKINS[2]}
PAL = fp.MaterialPalette({n: n for n in COLORS})
HEAD = (0, 1.43, 0)
PACK = (0, 1.14, -.20)
PARTS = []


def mesh(name):
    return k.Mesh(name, PAL)


def export(m, role, body='', style='', frame=''):
    order = fp.save_groups(m, OUT, 'Tier01 isolated accessory art; metres; +Y up; +Z forward.', 'Tier01Accessories.mtl')
    PARTS.append(dict(name=m.name, role=role, body=body, style=style, frame=frame,
                      prefab='Assets/_Game/Resources/Characters/Tier01Accessories/Parts/'+m.name+'.prefab',
                      vertices=len(m.vertices), triangles=len(m.faces), materials=order,
                      boundsMin=[round(min(v[i] for v in m.vertices), 6) for i in range(3)],
                      boundsMax=[round(max(v[i] for v in m.vertices), 6) for i in range(3)]))


def ribbon(m, points, width, thickness, color):
    """Continuous leather ribbon, closed edges; width along X, depth normal to path.
    Path lies in a shoulder's YZ plane, avoiding twisting or disconnected boxes.
    """
    rings = []
    for i, p in enumerate(points):
        tangent = k.unit(k.sub(points[min(i+1, len(points)-1)], points[max(0, i-1)]))
        across = (1, 0, 0)
        normal = k.unit(k.cross(across, tangent))
        rings.append((p, [k.add(p, k.add(k.mul(across, x*width/2), k.mul(normal, z*thickness/2)))
                          for x, z in ((-1, -1), (1, -1), (1, 1), (-1, 1))]))
    k.loft(m, rings, color)


def rabbit_hat(female=False):
    body = 'Female' if female else 'Male'
    m = mesh(body+'WearRabbitHat')
    sy = fp.head_sections(female)[-1][0]/.270
    # Hollow shell: inner surface and real rim, never a solid cap across the skull.
    rows = [(.194, .097, .120, .127), (.215, .096, .117, .126),
            (.249, .084, .097, .116), (.278, .055, .067, .086), (.289, .018, .026, .042)]
    outer = k.vertical_loft(m, [(y*sy, w, f, b) for y, w, f, b in rows], 'PeltLeather', a=.62, b=.55, cap_start=False)
    inner_start = len(m.faces)
    inner = k.vertical_loft(m, [(y*sy, w-.004, f-.004, b-.004) for y, w, f, b in rows], 'PeltLeatherDark', a=.62, b=.55, cap_start=False)
    m.faces[inner_start:] = [(a, c, b, col) for a, b, c, col in m.faces[inner_start:]]
    for i in range(8):
        j = (i+1) % 8
        m.quad(outer[0][i], outer[0][j], inner[0][j], inner[0][i], 'PeltFur', (0, .25*sy, 0))
    # Rolled pelt band, front above eyebrows, rear high enough for the tied ponytail.
    k.vertical_loft(m, [(.192*sy, .101, .124, .131), (.213*sy, .102, .125, .132)],
                    'PeltFur', a=.62, b=.55, cap_start=False, cap_end=False)
    # Ear flaps stay outside the current ears and Bob panels; do not cover eyes/face.
    for s in (-1, 1):
        ribbon(m, [(s*.103, .207*sy, -.025), (s*.106, .163*sy, -.028), (s*.103, .120*sy, -.025)],
               .016, .065, 'PeltFur')
        ribbon(m, [(s*.112, .198*sy, -.024), (s*.114, .150*sy, -.027), (s*.110, .128*sy, -.025)],
               .005, .044, 'PeltLeather')
    # Front seam and small edge stitches on actual leather, not painted silhouette.
    ribbon(m, [(0, .218*sy, .119), (0, .248*sy, .100), (0, .276*sy, .071)], .004, .003, 'PeltLeatherDark')
    for x in (-.050, -.025, .025, .050):
        k.box(m, (x, .219*sy, .119), (.008, .002, .003), 'PeltFur')
    return m


def compatible_hair(female, style):
    """Clip source triangles to a cap-safe hairline; keep lower style geometry and palette.
    New fringe distinguishes Sweep from Crop after its quiff is tucked under the cap.
    Bob side/nape panels and the entire low Tied ponytail retain their source shapes.
    """
    m = mesh(('Female' if female else 'Male')+'HatHair'+style)
    source = k.Mesh('source', avatar.PAL)
    fp.hair(source, female, style)
    sy = fp.head_sections(female)[-1][0]/.270
    ceiling = .191*sy
    for a, b, c, _ in source.faces:
        poly = [source.vertices[i] for i in (a, b, c)]
        clipped = []
        for p, q in zip(poly, poly[1:]+poly[:1]):
            pin, qin = p[1] <= ceiling, q[1] <= ceiling
            if pin:
                clipped.append(p)
            if pin != qin:
                clipped.append(k.lerp(p, q, (ceiling-p[1])/(q[1]-p[1])))
        ids = [m.v(p) for p in clipped]
        for i in range(1, len(ids)-1):
            m.tri(ids[0], ids[i], ids[i+1], 'Hair')
    if style == 'Sweep':
        fp.lock(m, (-.045, .190*sy, .111), (.035, .181*sy, .113), .013, .008, 'Hair')
    return m


def starter_pack(female=False):
    body = 'Female' if female else 'Male'
    m = mesh(body+'WearStarterBackpack')
    # Author in root space first. Bag sits behind rails, rails behind shirt.
    top = 1.355 if not female else 1.340
    width = .155 if not female else .146
    for s in (-1, 1):
        x = s*(width-.025)
        k.limb(m, [((x, .995, -.168), .013, .013), ((x, top+.014, -.168), .013, .013)], 'FrameWood', sides=6)
        k.box(m, (x, top+.017, -.168), (.024, .006, .024), 'FrameEnd')
    for y in (1.035, 1.295):
        k.box(m, (0, y, -.169), (width*2, .020, .021), 'FrameWood')
    k.vertical_loft(m, [(1.015, width*.75, .063, .070), (1.043, width, .075, .090),
                       (1.215, width, .071, .090), (top-.035, width*.93, .062, .077)],
                    'HempShirt', a=.73, b=.65, zc=-.270)
    k.vertical_loft(m, [(top-.095, width+.008, .065, .089), (top-.040, width+.010, .069, .092),
                       (top-.020, width*.90, .056, .080)], 'PeltLeatherDark', zc=-.271, a=.73, b=.65)
    # Rear pocket follows bag volume and has a distinct flap and lower corner facets.
    k.vertical_loft(m, [(1.045, .070, .021, .026), (1.065, .083, .025, .033), (1.155, .080, .025, .032)],
                    'PeltLeather', zc=-.365, a=.75, b=.65)
    k.box(m, (0, 1.154, -.399), (.164, .023, .009), 'PeltLeatherDark')
    for s in (-1, 1):
        x = s*.092
        ribbon(m, [(x, 1.290, -.365), (x, 1.180, -.371), (x, 1.155, -.373)], .022, .006, 'PeltLeatherDark')
        k.box(m, (x, 1.184, -.379), (.030, .026, .007), 'Brass')
        k.box(m, (x, 1.184, -.384), (.017, .013, .004), 'PeltLeatherDark')
        # Shoulder loops derived from torso sections, inset from arm/sleeve joints.
        # Front paths stay lateral to chest pockets; sides route around underarm.
        sx = .158 if not female else .153
        crown = .94 + fp.torso_sections(female)[-1][0] + .012
        pts = [(s*.115, 1.295, -.170), (s*.105, crown-.040, -.115),
               (s*.105, crown, -.065), (s*.105, crown, .045), (s*.110, crown-.025, .105)]
        for y in (.390, .350, .300, .240, .180, .120):
            pts.append((s*sx, .94+y, fp.torso_front(female, sx, y)+.016))
        side_x = .197 if not female else .183
        pts += [(s*.120, 1.025, .140), (s*side_x, 1.035, .035),
                (s*side_x, 1.045, -.060), (s*(side_x-.010), 1.050, -.135), (s*.115, 1.055, -.172)]
        ribbon(m, pts, .027, .006, 'PeltLeatherDark')
        k.box(m, (s*sx, 1.115, fp.torso_front(female, sx, .175)+.024), (.034, .032, .008), 'Brass')
        k.box(m, (s*sx, 1.115, fp.torso_front(female, sx, .175)+.029), (.021, .019, .004), 'PeltLeatherDark')
        # Lashings bind frame to pack at both ends.
        for y in (1.045, 1.285):
            k.box(m, (s*(width-.025), y, -.184), (.034, .016, .035), 'PeltLeather')
    k.limb(m, [(p, .008, .008) for p in [(-.048, top-.020, -.240), (-.045, top+.020, -.240),
                                       (.045, top+.020, -.240), (.048, top-.020, -.240)]],
           'PeltLeatherDark', sides=6)
    m.transform(lambda p: k.sub(p, PACK))
    return m


def seated_studies(female):
    """Review-only rigid limb placement. No weights, animations or runtime pose promise."""
    body = 'Female' if female else 'Male'
    # Keep the existing neck, arms and hands verbatim. The standing skin mesh also
    # contains bare feet; omit only its foot triangles in this independent study.
    skin = k.Mesh(body+'SeatedUpperSkinStudy', avatar.PAL)
    group = None
    for line in (ROOT/'Assets/_Game/Art/Characters/PlayerDefault'/f'{body}Skin.obj').read_text().splitlines():
        words = line.split()
        if not words:
            continue
        if words[0] == 'v':
            skin.v(tuple(float(v) for v in words[1:4]))
        elif words[0] == 'usemtl':
            group = avatar.PAL.names.index(words[1])
        elif words[0] == 'f':
            ids = [int(v.split('/')[0])-1 for v in words[1:4]]
            if min(skin.vertices[i][1] for i in ids) > .25:
                skin.faces.append((*ids, group))
    used = sorted({i for face in skin.faces for i in face[:3]})
    remap = {old: new for new, old in enumerate(used)}
    skin.vertices = [skin.vertices[i] for i in used]
    skin.faces = [(remap[a], remap[b], remap[c], col) for a, b, c, col in skin.faces]
    export(skin, 'poseStudy', body, frame='upper root; translate down .38 for seated review')
    pants = k.Mesh(body+'SeatedHempPantsStudy', avatar.PAL)
    fp.place(pants, lambda m: fp.hips(m, female, cloth='HempPants', band='HempPantsDark'), lambda p: k.add(p, (0, .50, 0)))
    fp.place(pants, lambda m: avatar.belt_with_loops(m, female), lambda p: k.add(p, (0, .50, 0)))
    for s in (-1, 1):
        fp.place(pants, lambda m: fp.thigh(m, 'HempPants', female), lambda p: k.add(fp.rot_x(p, -90), (s*fp.HIP_X, .50, 0)))
        fp.place(pants, lambda m: fp.shin(m, 'HempPants', 'HempPantsDark'), lambda p: k.add(p, (s*fp.HIP_X, .50, .38)))
    export(pants, 'poseStudy', body, frame='root; seated hips .50')
    boots = avatar.rabbit_boots(female)
    boots.name = body+'SeatedRabbitBootsStudy'
    boots.transform(lambda p: k.add(p, (0, 0, .38)))
    export(boots, 'poseStudy', body, frame='root; seated feet forward .38')


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    PARTS.clear()
    text = []
    for name, color in COLORS.items():
        rgb = [int(color[i:i+2], 16)/255 for i in (1, 3, 5)]
        text += ['newmtl '+name, 'Kd '+' '.join(f'{v:.6f}' for v in rgb), 'Ks 0 0 0', 'Ns 1', '']
    (OUT/'Tier01Accessories.mtl').write_text('\n'.join(text), encoding='utf-8')
    # Six representations: two loose item/display studies, four fitted shells.
    for name, build in [('DisplayRabbitHat', rabbit_hat), ('DisplayStarterBackpack', starter_pack)]:
        m = build(False)
        m.name = name
        lo = [min(p[i] for p in m.vertices) for i in range(3)]
        hi = [max(p[i] for p in m.vertices) for i in range(3)]
        center = tuple((a+b)/2 for a, b in zip(lo, hi))
        m.transform(lambda p: k.sub(p, center))
        export(m, 'display', frame='centered loose study; no live item replacement')
    for female in (False, True):
        body = 'Female' if female else 'Male'
        export(rabbit_hat(female), 'wornHat', body, frame='HatAnchor local; identity TRS')
        export(starter_pack(female), 'wornBackpack', body, frame='BackpackAnchor local; identity TRS')
        for style in fp.HAIR_STYLES:
            export(compatible_hair(female, style), 'hatHair', body, style, 'HeadAnchor/HairAnchor local')
        seated_studies(female)
    sources = ['Tools/Art/faceted_people.py', 'Tools/Art/generate_player_avatar_meshes.py', 'Tools/Art/faceted_character_kit.py',
               'Assets/_Game/Art/Characters/PlayerDefault/MaleSkin.obj', 'Assets/_Game/Art/Characters/PlayerDefault/FemaleSkin.obj']
    manifest = dict(revision=1, units='metres', up='+Y', forward='+Z', headAnchor=HEAD, backpackAnchor=PACK,
                    parts=PARTS, materials=[dict(name=n, hex=c) for n, c in COLORS.items()],
                    references=[dict(path=p, sha256=hashlib.sha256((ROOT/p).read_bytes()).hexdigest()) for p in sources],
                    itemIds=['item.rabbit_hat', 'item.starter_backpack'],
                    liveFootprints=[[900, 723], [939, 1350]],
                    status='Source generated only; parent must import/build/capture; binding and deformation unimplemented.')
    (OUT/'Tier01AccessoriesManifest.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
    RESOURCES.mkdir(parents=True, exist_ok=True)
    art_map = dict(scope='Passive art handoff data only. No loader, catalog registration, equip binding or live item replacement.',
                   prefabStatus='Parent must execute Tier01FittedAccessoriesAssets.Build before these paths exist.',
                   sourceManifest='Assets/_Game/Art/Characters/Tier01Accessories/Tier01AccessoriesManifest.json',
                   items=[dict(itemId=item, liveFootprint=footprint, existingDisplayResource=legacy,
                               displayStudy='Characters/Tier01Accessories/Parts/Display'+part,
                               worn={body: 'Characters/Tier01Accessories/Parts/'+body+'Wear'+part for body in ('Male', 'Female')},
                               anchor=anchor, localPosition=[0, 0, 0], localEuler=[0, 0, 0], localScale=[1, 1, 1])
                          for item, part, footprint, legacy, anchor in [
                              ('item.rabbit_hat', 'RabbitHat', [900, 723], 'Tier01/Crafted/RabbitHat', 'HeadAnchor/HatAnchor'),
                              ('item.starter_backpack', 'StarterBackpack', [939, 1350], 'Tier01/Crafted/StarterBackpack', 'BackpackAnchor')]],
                   compatibleHair={body: {style: 'Characters/Tier01Accessories/Parts/'+body+'HatHair'+style for style in fp.HAIR_STYLES}
                                   for body in ('Male', 'Female')},
                   hairMaterialPaths=['Assets/_Game/Art/Characters/PlayerDefault/Unity/Materials/Hair'+str(i).zfill(2)+'.mat' for i in range(1, 6)],
                   integration='Keep saved hairstyle; substitute compatible geometry while hat is shown, restore original on removal. Preserve face, brows and independent beard attachments.')
    (RESOURCES/'Tier01AccessoriesArtMap.json').write_text(json.dumps(art_map, indent=2)+'\n', encoding='utf-8')
    print(f'Generated {len(PARTS)} OBJ sources in {OUT}; no Unity calls.')


if __name__ == '__main__':
    main()
