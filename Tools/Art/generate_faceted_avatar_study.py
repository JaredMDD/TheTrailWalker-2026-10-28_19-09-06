"""Faceted avatar style study (November 2, 2026): one male default player in base clothes, for user review.
Writes Assets/_Game/Art/Characters/FacetedStudy/. Does not touch the live player or camp NPC meshes.
Run: python Tools/Art/generate_faceted_avatar_study.py, then The Trail Walker > Art > Faceted Avatar Study.
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Characters/FacetedStudy'
HEAD_ANCHOR = (0, 1.43, 0)

def blend(a, b, t):
    ca = [int(a[i:i+2], 16) for i in (1, 3, 5)]; cb = [int(b[i:i+2], 16) for i in (1, 3, 5)]
    return '#'+''.join('%02X' % round(x+(y-x)*t) for x, y in zip(ca, cb))

SKIN = '#BA8055'
pal = k.Palette()
for name, color in [('Skin', SKIN), ('SkinShade', blend(SKIN, '#3B2418', .22)), ('Stubble', blend(SKIN, '#3A302A', .17)),
                    ('Lips', '#8A5440'), ('Eye', '#1C1612'), ('Hair', '#4E3321'), ('HairDark', '#3A2518'),
                    ('Shirt', '#99876A'), ('ShirtDark', '#7C6C52'), ('Pants', '#655740'), ('PantsDark', '#4F4432'), ('Nail', '#D8A887')]:
    pal.add(name, color)

# ------------------------------------------------------------------ head (HeadAnchor-local)

# Shortened chin (user, November 2, 2026): the lower face is about 2 cm shorter.
HEAD = [(0.020, .034, .093, -.050), (0.038, .060, .099, -.005), (0.062, .074, .102, .055), (0.092, .077, .106, .085),
        (0.120, .079, .108, .100), (0.150, .080, .106, .106), (0.178, .081, .104, .108), (0.215, .078, .094, .106),
        (0.248, .066, .074, .094), (0.270, .040, .044, .062)]

def head(name):
    m = k.Mesh(name, pal)
    # No stubble (user, November 2, 2026): facial hair is either a full beard (separate part) or none.
    k.vertical_loft(m, HEAD, 'Skin', a=.62, b=.55)
    # Ears: tapered flat slabs set back on the sides.
    for s in (-1, 1):
        k.box(m, (s*.080, .136, -.004), (.016, .056, .040), 'SkinShade', taper=.85)
        k.box(m, (s*.086, .138, -.002), (.006, .036, .024), 'Skin')
    # Wedge nose from the brow bridge to a forward tip.
    T, BL, BR = m.v((0, .170, .104)), m.v((-.011, .150, .107)), m.v((.011, .150, .107))
    P, L, R, B = m.v((0, .110, .138)), m.v((-.018, .102, .110)), m.v((.018, .102, .110)), m.v((0, .099, .126))
    inside = (0, .130, .095)
    for f in ((T, BL, P), (T, P, BR), (BL, L, P), (BR, P, R), (L, B, P), (R, P, B), (L, R, B)): m.tri(*f, 'Skin', inside)
    # Small dark eyes, slab brows (hair colour) angled down to the nose, and a thin mouth line.
    for s in (-1, 1):
        k.box(m, (s*.034, .146, .1045), (.019, .009, .006), 'Eye', shade=2)
        k.box(m, (s*.035, .170, .1075), (.040, .012, .012), 'Hair', axes=((1, -s*.18, 0), (s*.18, 1, 0), (0, 0, 1)), shade=1)
    k.box(m, (0, .079, .1055), (.040, .006, .006), 'Lips', shade=1)
    k.box(m, (0, .073, .1040), (.030, .006, .006), 'Lips', shade=3)
    return m

def beard(name):
    """Full angular beard in the hair colour: a shell over jaw and chin with an open mouth, plus a moustache."""
    m = k.Mesh(name, pal)
    sections = [(0.006, .030, .100, -.060), (0.028, .064, .108, -.004), (0.060, .080, .111, .052), (0.092, .083, .113, .080),
                (0.118, .084, .112, .092), (0.158, .084, .106, .096)]
    rings = []
    for y, w, f, bk in sections:
        c = (0, y, 0); rings.append((c, k.ring_points(c, (1, 0, 0), (0, 0, 1), k.chamfer_rect(w, f, bk, .62, .55))))
    ids = [[m.v(p) for p in pts] for _, pts in rings]
    # Faces per band: 7 front, 0/6 front chamfers, 1/5 sides. Band 2 leaves the mouth open; bands 3-4 keep only the sides, joining the sideburns.
    keep = {0: (7, 0, 6, 1, 5), 1: (7, 0, 6, 1, 5), 2: (0, 6, 1, 5), 3: (1, 5), 4: (1, 5)}
    for band, faces in keep.items():
        inside = k.lerp(rings[band][0], rings[band+1][0], .5)
        for i in faces:
            j = (i+1) % 8
            m.quad(ids[band][i], ids[band][j], ids[band+1][j], ids[band+1][i], 'Hair', inside)
    cap = m.v(rings[0][0])
    for i in (7, 0, 6):
        m.tri(cap, ids[0][i], ids[0][(i+1) % 8], 'HairDark', rings[1][0])
    # Moustache over the mouth, with the ends dropping into the beard; a darker chin tuft adds a plane break.
    k.box(m, (0, .090, .110), (.050, .012, .010), 'Hair', taper=.8)
    for s_ in (-1, 1):
        k.box(m, (s_*.029, .078, .109), (.012, .024, .010), 'Hair', shade=1)
    k.box(m, (0, .020, .100), (.034, .020, .016), 'HairDark', taper=.7)
    return m

def lock(m, start, end, width, thick, color, lift=0.0, shade=None):
    """Flat tapered hair lock lying from start to end; lift raises its middle off the scalp."""
    along = k.unit(k.sub(end, start)); up = k.unit(k.sub((0, 1, 0), k.mul(along, k.dot((0, 1, 0), along))))
    if abs(along[1]) > .9: up = (0, 0, 1)
    side = k.cross(along, up)
    center = k.add(k.lerp(start, end, .5), k.mul(up, lift))
    k.box(m, center, (width, k.length(k.sub(end, start)), thick), color, axes=(side, along, up), taper=.55, shade=shade)

def hair_sweep(name):
    m = k.Mesh(name, pal)
    # Close angular cap: hairline at the forehead, trimmed sides above the ears and a short nape.
    shell = [(0.095, .066, .010, .104), (0.140, .083, .050, .110), (0.180, .085, .084, .112), (0.207, .084, .101, .111),
             (0.240, .076, .092, .104), (0.268, .060, .068, .086), (0.286, .034, .040, .054)]
    k.vertical_loft(m, shell, 'Hair', a=.60, b=.55, cap_start=False)
    # Flat locks swept back from a raised front, slightly to the right.
    for x, lift in [(-.046, .004), (-.016, .010), (.016, .012), (.046, .006)]:
        lock(m, (x, .222, .098), (x + .018, .268, -.010), .040, .018, 'Hair', lift)
    for x in (-.035, .000, .035):
        lock(m, (x, .272, .010), (x + .012, .246, -.085), .042, .016, 'HairDark' if x == 0 else 'Hair', .006)
    # Raised quiff over the brow.
    lock(m, (-.030, .218, .104), (.040, .256, .088), .036, .022, 'Hair', .012, shade=3)
    # Trimmed side panels and sideburns.
    for s in (-1, 1):
        k.box(m, (s*.0815, .152, .038), (.006, .030, .016), 'Hair', taper=.7)
    return m

# ------------------------------------------------------------------ body (root space)

SHOULDER_X, SHOULDER_Y, ARM_SPREAD = .205, 1.355, 9

def arm_frame(side):
    a = math.radians(ARM_SPREAD)
    down = (side*math.sin(a), -math.cos(a), 0)
    return (side*SHOULDER_X, SHOULDER_Y, 0), down

def arm_points(side):
    shoulder, down = arm_frame(side)
    elbow = k.add(shoulder, k.mul(down, .29))
    fore = k.unit(k.add(down, (0, 0, .14)))   # natural slight forward bend at the elbow
    wrist = k.add(elbow, k.mul(fore, .25))
    return shoulder, down, elbow, fore, wrist

def skin_parts(name):
    m = k.Mesh(name, pal)
    k.limb(m, [((0, 1.36, -.012), .052, .050), ((0, 1.42, -.010), .050, .048), ((0, 1.47, -.012), .048, .046)], 'Skin', sides=6)
    for side in (-1, 1):
        shoulder, down, elbow, fore, wrist = arm_points(side)
        p = lambda s: k.add(shoulder, k.mul(down, s))
        f = lambda s: k.add(elbow, k.mul(fore, s))
        k.limb(m, [(p(.10), .046, .048), (p(.20), .043, .045), (elbow, .039, .041), (f(.06), .044, .046), (f(.16), .037, .035), (wrist, .028, .024)],
               'Skin', sides=6)
        hand(m, side, wrist, fore)
        foot(m, side)
    return m

def hand(m, side, wrist, down):
    out = k.unit(k.cross((0, 0, 1), down)); out = k.mul(out, -1 if k.dot(out, (side, 0, 0)) < 0 else 1)
    fwd = k.unit(k.cross(down, out)); fwd = k.mul(fwd, -1 if fwd[2] < 0 else 1)
    at = lambda d, f=0, o=0: k.add(wrist, k.add(k.mul(down, d), k.add(k.mul(fwd, f), k.mul(out, o))))
    # Palm: thickness across the body (rx along out), width front-back.
    k.limb(m, [(at(-.005), .020, .030), (at(.035), .023, .044), (at(.080), .020, .045)], 'Skin', sides=6, turn=0, forward_hint=fwd)
    # Fingers as one angular block curling toward the thigh, with the knuckle line catching light.
    k.limb(m, [(at(.078, 0, -.002), .018, .043), (at(.115, 0, -.010), .016, .040), (at(.150, -.004, -.024), .013, .034)],
           'Skin', sides=6, turn=0, forward_hint=fwd, colors=[lambda i: 'Skin', lambda i: 'SkinShade' if i in (2, 3) else 'Skin'])
    # Thumb on the forward edge.
    k.limb(m, [(at(.020, .030, -.006), .012, .013), (at(.055, .048, -.016), .011, .012), (at(.080, .052, -.024), .009, .010)],
           'Skin', sides=5, forward_hint=fwd)

def foot(m, side):
    x = side*.102
    rings = []
    for z, w, top, bottom, cy in [(-.055, .034, .030, .030, .032), (-.012, .042, .058, .036, .036), (.060, .046, .026, .030, .030),
                                  (.140, .048, .014, .018, .018), (.178, .040, .008, .012, .012)]:
        c = (x, cy, z)
        rings.append((c, k.ring_points(c, (1, 0, 0), (0, 1, 0), k.chamfer_rect(w, top, bottom, .7, .6))))
    k.loft(m, rings, 'Skin')
    for i, tx in enumerate((-.030, -.012, .008, .026)):
        k.box(m, (x + side*tx, .010, .172 - .006*abs(i-1.5)), (.014, .016, .016), 'SkinShade' if i == 0 else 'Skin')

def shirt(name):
    m = k.Mesh(name, pal)
    sections = [(0.860, .192, .124, .122), (0.878, .191, .124, .121), (0.960, .188, .121, .117), (1.050, .180, .120, .112), (1.140, .178, .125, .109),
                (1.230, .187, .131, .111), (1.300, .198, .125, .111), (1.340, .204, .112, .104), (1.370, .196, .096, .094), (1.395, .150, .080, .079),
                (1.418, .074, .064, .060)]
    k.vertical_loft(m, sections, 'Shirt', a=.70, b=.62, cap_end=False,
                    colors=['ShirtDark'] + ['Shirt']*(len(sections)-2))
    # Ribbed crew collar and a chest seam line.
    k.vertical_loft(m, [(1.410, .078, .068, .064), (1.432, .068, .060, .056)], 'ShirtDark', a=.70, b=.62, cap_start=False, cap_end=False)
    for side in (-1, 1):
        shoulder, down, *_ = arm_points(side)
        p = lambda s: k.add(shoulder, k.mul(down, s))
        k.limb(m, [(k.add(p(-.020), (-side*.060, -.010, 0)), .040, .046), (p(.020), .062, .064), (p(.08), .062, .064), (p(.145), .058, .060),
                   (p(.150), .061, .063), (p(.165), .060, .062), (p(.170), .050, .052)], 'Shirt', sides=8, turn=math.pi/8,
               colors=['Shirt', 'Shirt', 'Shirt', 'ShirtDark', 'ShirtDark', 'ShirtDark'])
    return m

def pants(name):
    m = k.Mesh(name, pal)
    k.vertical_loft(m, [(0.800, .165, .100, .106), (0.880, .182, .110, .119), (0.960, .176, .108, .112), (1.020, .166, .105, .105),
                        (1.050, .162, .104, .102)], 'Pants', a=.72, b=.62, colors=['Pants', 'Pants', 'Pants', 'PantsDark'])
    for side in (-1, 1):
        # Fuller hips and legs (user, November 2, 2026): the lower body read too thin.
        x = side*.100
        joints = [((x, .860, 0), .094, .099), ((x, .720, .004), .086, .090), ((side*.102, .580, .008), .070, .075),
                  ((side*.102, .500, .012), .064, .068), ((side*.102, .400, .002), .067, .073), ((side*.102, .250, -.004), .055, .059),
                  ((side*.102, .140, -.006), .050, .054), ((side*.102, .118, -.006), .058, .062), ((side*.102, .092, -.006), .058, .062),
                  ((side*.102, .084, -.006), .050, .054)]
        k.limb(m, joints, 'Pants', sides=8, turn=math.pi/8, colors=['Pants']*6 + ['PantsDark']*3)
    return m

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    pal.write_png(OUT/'StudyPalette.png')
    header = 'The Trail Walker faceted avatar study; metres; +Y up; +Z forward.'
    parts = [head('StudyMaleHead'), hair_sweep('StudyMaleHairSweep'), beard('StudyMaleBeard'), skin_parts('StudyMaleSkin'), shirt('StudyMaleShirt'), pants('StudyMalePants')]
    stats = [p.save(OUT, header) for p in parts]
    manifest = {'revision': 1, 'headAnchor': list(HEAD_ANCHOR), 'palette': 'StudyPalette.png', 'colors': pal.names,
                'shades': len(k.SHADES), 'headLocal': ['StudyMaleHead', 'StudyMaleHairSweep'], 'parts': stats,
                'avatars': [{'name': 'StudyMale', 'body': ['StudyMaleSkin', 'StudyMaleShirt', 'StudyMalePants'],
                             'head': ['StudyMaleHead', 'StudyMaleHairSweep']},
                            {'name': 'StudyMaleBeard', 'body': ['StudyMaleSkin', 'StudyMaleShirt', 'StudyMalePants'],
                             'head': ['StudyMaleHead', 'StudyMaleHairSweep', 'StudyMaleBeard']}]}
    (OUT/'FacetedStudyManifest.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
    total = sum(s['triangles'] for s in stats)
    print('Faceted study:', ', '.join(f"{s['name']} {s['triangles']}" for s in stats), '| total', total, 'triangles')

if __name__ == '__main__':
    main()
