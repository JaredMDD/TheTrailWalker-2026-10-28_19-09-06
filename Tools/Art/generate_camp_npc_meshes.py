"""Common camp NPC avatars, revision 4: faceted style (approved, user, November 2, 2026). Python standard library only.

Rigid-joint parts for CampNpcAvatarAssets, built from faceted_people in their pivot frames (see faceted_people for the
skeleton). Palette slots are unchanged from revision 3: 0-2 skin/hair (tinted by material), 3 dark, 6 eyes, 7 lips,
8-10 shirt cloth, 9 trousers, 11-13 leather, 14-15 boot, 16-17 pack, 18 thread/buckle, 19-20 rag, 21 frame,
22-23 feather. New in revision 4: four hair styles per body, eyebrows in the hair colour and a beard (male), so each
NPC can roll its own hairstyle and facial hair.
"""
from pathlib import Path
import json
import math
import generate_region01_meshes as b
import faceted_character_kit as k
import faceted_people as fp

ROOT = Path(__file__).resolve().parents[2]; OUT = ROOT/'Assets/_Game/Art/Characters/CampCommon'
COLORS = [(242, 237, 222), (205, 199, 184), (255, 249, 230), (31, 27, 22),
          (72, 54, 35), (245, 237, 215), (21, 22, 19), (117, 78, 57),
          (173, 164, 119), (128, 119, 83), (203, 190, 139), (126, 92, 58),
          (84, 61, 40), (169, 135, 88), (91, 67, 43), (48, 38, 27),
          (144, 125, 80), (109, 77, 48), (226, 204, 151), (158, 157, 133),
          (128, 112, 84), (153, 108, 62), (218, 210, 171), (101, 121, 73)]
PAL = fp.SlotPalette(len(COLORS), {
    'Skin': 0, 'SkinShade': 1, 'SkinLight': 2, 'Hair': 0, 'HairDark': 1, 'Brow': 0, 'Dark': 3, 'Eye': 6, 'Lips': 7,
    'Cloth': 8, 'ClothDark': 9, 'ClothLight': 10, 'Pants': 9, 'PantsDark': 12, 'Leather': 11, 'LeatherDark': 12, 'LeatherLight': 13,
    'Boot': 14, 'Sole': 15, 'Pack': 16, 'PackDark': 17, 'Metal': 18, 'RagLight': 19, 'Rag': 20, 'Frame': 21, 'Feather': 22, 'FeatherDark': 23})
stats = []
HEADER = 'The Trail Walker camp NPC part (faceted revision 4); metres; +Y up; +Z forward; rig-frame local.'

def part(name, build):
    m = k.Mesh(name, PAL); build(m)
    s = m.save(OUT, HEADER); stats.append(dict(Name=s['name'], Triangles=s['triangles']))

# ------------------------------------------------------------------ accessories that follow the torso (torso frame)

def pockets(m, female):
    for s in (-1, 1):
        x, y = s*.085, .27 if not female else .23
        z = fp.torso_front(female, x, y)
        k.box(m, (x, y, z+.006), (.090, .085, .012), 'Leather')
        k.box(m, (x, y+.046, z+.014), (.096, .022, .012), 'LeatherDark')
        k.box(m, (x, y+.040, z+.022), (.012, .012, .006), 'Metal')

def rag_patches(m, female):
    for x, y, w, h in [(-.090, .150, .100, .090), (.080, .300, .075, .065)]:
        z = fp.torso_front(female, x, y)
        k.box(m, (x, y, z+.004), (w, h, .010), 'Rag')
        for i in range(4): k.box(m, (x-w/2+.012+i*w/4, y+h/2-.008, z+.011), (.010, .005, .004), 'Metal')
    for i in range(7):
        x = -.17+i*.057; y = -.075+.010*(i % 2)
        k.box(m, (x, y, fp.torso_front(female, x, -.07)-.006), (.034, .024, .014), 'RagLight' if i % 2 else 'Rag', taper=.6)

def straps(m, female):
    for s in (-1, 1):
        x = s*.110
        pts = []
        for y in (.02, .12, .22, .32, .40):
            pts.append((x+s*.004*(y/.4), y, fp.torso_front(female, x, y)+.008))
        top = fp.torso_sections(female)[-3][0]
        pts += [(x*1.05, top+.010, .02), (x*1.05, top-.020, -fp.torso_back(female, top)-.006), (x, .20, -fp.torso_back(female, .2)-.008)]
        for a, c in zip(pts, pts[1:]):
            mid = k.lerp(a, c, .5); along = k.unit(k.sub(c, a))
            normal = k.unit((0, 0, 1)) if mid[2] > 0 else (0, 0, -1)
            side = k.unit(k.cross(along, normal))
            k.box(m, mid, (.038, k.length(k.sub(c, a))+.006, .010), 'Leather', axes=(side, along, k.cross(side, along)))
        y = .20; k.box(m, (x, y, fp.torso_front(female, x, y)+.016), (.034, .030, .008), 'Metal')

def trim(m, female):
    sec = fp.torso_sections(female); top = sec[-1]
    k.vertical_loft(m, [(top[0]-.032, top[1]+.030, top[2]+.022, top[3]+.022), (top[0]-.004, top[1]+.012, top[2]+.010, top[3]+.010)],
                    'LeatherLight', a=.70, b=.62, cap_start=False, cap_end=False)
    sx, sy = fp.SHOULDER[female]
    for s in (-1, 1):
        k.box(m, (s*(sx-.035), sy+.020, 0), (.085, .020, .150), 'Leather', axes=((1, -s*.35, 0), (s*.35, 1, 0), (0, 0, 1)))
        for y in (.08, .20):
            k.box(m, (s*.045, y, fp.torso_front(female, s*.045, y)+.004), (.012, .080, .008), 'LeatherLight')

def belt(m):
    # Hips frame: the belt sits over the shirt hem at the waist (torso frame y 0.00-0.05).
    y0 = fp.TORSO_UP
    rows = []
    for dy in (-.002, .046):
        w, f, bk = fp.section_at(fp.MALE_TORSO, dy)
        rows.append((y0+dy, w+.010, f+.010, bk+.010))
    k.vertical_loft(m, rows, 'Leather', a=.70, b=.62, cap_start=False, cap_end=False, colors=['Leather'])
    f = rows[0][2]
    k.box(m, (0, y0+.022, f+.004), (.052, .044, .012), 'Metal')
    k.box(m, (0, y0+.022, f+.010), (.028, .022, .006), 'LeatherDark')

# ------------------------------------------------------------------ headwear (head frame)

def fur_cap(m):
    sec = fp.MALE_HEAD
    rows = []
    for y, g in [(.200, .018), (.235, .020), (.270, .016), (.300, .010)]:
        w, f, bk = fp.section_at(sec, min(y, sec[-1][0]))
        rows.append((y, w+g, f+g, bk+g))
    rows.append((.318, .040, .046, .060))
    k.vertical_loft(m, rows, 'Leather', a=.62, b=.55, cap_start=False)
    # Chunky fur brim: angled blocks around the band.
    w, f, bk = fp.section_at(sec, .205)
    for i in range(14):
        a = i*math.tau/14
        x, z = (w+.024)*math.cos(a), ((f if math.sin(a) > 0 else bk)+.024)*math.sin(a)
        k.box(m, (x, .204+.004*math.sin(i*2.3), z), (.050, .036, .030), 'LeatherLight' if i % 2 else 'Leather',
              axes=((-math.sin(a), 0, math.cos(a)), (0, 1, 0), (math.cos(a), 0, math.sin(a))))

def feather_trim(m):
    for i in range(3):
        base = (.090+i*.012, .230+i*.018, -.020-i*.010)
        k.box(m, k.add(base, (.010, .070, 0)), (.014, .140, .030), 'Feather' if i % 2 else 'FeatherDark',
              axes=((0, 0, 1), (.30, 1, -.10), (1, 0, 0)), taper=.35)
    k.limb(m, [((.088, .210, -.020), .003, .003), ((.140, .380, -.050), .002, .002)], 'Frame', sides=4)

def cloth_hood(m):
    # Open-face hood: an arc of sections around the head, double sided, with a hemmed face edge.
    sec = fp.MALE_HEAD
    n = 10; a0, a1 = math.radians(55), math.radians(125)   # opening centred on +Z
    rings = []
    for y, g in [(.000, .055), (.060, .050), (.130, .045), (.200, .040), (.260, .035), (.300, .025)]:
        w, f, bk = fp.section_at(sec, min(max(y, sec[0][0]), sec[-1][0]))
        ring = []
        for i in range(n+1):
            a = a1 + i*(math.tau-(a1-a0))/n
            x = (w+g)*math.cos(a); z = ((f if math.sin(a) > 0 else bk)+g)*math.sin(a)
            ring.append(m.v((x, y, z)))
        rings.append(ring)
    top = m.v((0, .318, -.010))
    for r in range(len(rings)-1):
        for i in range(n):
            q = (rings[r][i], rings[r][i+1], rings[r+1][i+1], rings[r+1][i])
            m.quad(*q, 'Cloth', (0, (r+.5)*.06, 0)); m.faces.append((q[0], q[2], q[1], PAL.index('ClothDark'))); m.faces.append((q[0], q[3], q[2], PAL.index('ClothDark')))
    for i in range(n): m.tri(rings[-1][i], rings[-1][i+1], top, 'Cloth', (0, .25, 0))
    for edge in (0, n):
        for r in range(len(rings)-1):
            a, c = m.vertices[rings[r][edge]], m.vertices[rings[r+1][edge]]
            k.box(m, k.lerp(a, c, .5), (.016, k.length(k.sub(c, a))+.004, .016), 'ClothLight', axes=((1, 0, 0), k.unit(k.sub(c, a)), (0, 0, 1)))

# ------------------------------------------------------------------ backpacks (BackpackAnchor frame: front face at z = 0)

def chamfer_box(m, center, w, h, d, color, a=.75):
    c0 = k.add(center, (0, -h/2, 0)); c1 = k.add(center, (0, h/2, 0))
    rings = [(c, k.ring_points(c, (1, 0, 0), (0, 0, 1), k.chamfer_rect(w/2, d/2, d/2, a, .7))) for c in (c0, c1)]
    k.loft(m, rings, color)

def backpack_body(m):
    chamfer_box(m, (0, .0, -.090), .330, .400, .170, 'Pack')
    chamfer_box(m, (0, .175, -.095), .350, .100, .185, 'PackDark')           # top flap
    chamfer_box(m, (0, -.080, -.190), .220, .160, .050, 'Pack')             # front pocket
    for x in (-.090, .090):
        k.box(m, (x, .020, -.187), (.028, .300, .014), 'Leather')
        k.box(m, (x, .095, -.197), (.034, .034, .010), 'Metal')

def rag_backpack(m):
    chamfer_box(m, (0, -.010, -.100), .360, .380, .190, 'Rag', a=.6)
    for i, (x, y) in enumerate([(-.08, .06), (.09, -.08), (-.02, -.12)]):
        k.box(m, (x, y, -.197), (.090, .080, .010), 'RagLight' if i % 2 else 'Rag')
    for x in (-.100, .100): k.box(m, (x, 0, -.198), (.022, .340, .014), 'Leather')
    k.limb(m, [((0, .175, -.10), .055, .045), ((0, .230, -.10), .030, .025)], 'Rag', sides=6)
    k.box(m, (0, .190, -.10), (.070, .016, .060), 'Metal')

def pack_frame(m):
    for x in (-.190, .190): k.box(m, (x, 0, -.050), (.026, .500, .026), 'Frame')
    for y in (-.200, .200): k.box(m, (0, y, -.050), (.400, .022, .022), 'Frame')

def pack_roll(m):
    k.limb(m, [((-.190, .245, -.090), .066, .066), ((.190, .245, -.090), .066, .066)], 'Cloth', sides=8, turn=math.pi/8, forward_hint=(0, 0, 1))
    for x in (-.120, .120): k.box(m, (x, .245, -.090), (.020, .150, .150), 'Leather')

# ------------------------------------------------------------------ export

def main():
    OUT.mkdir(parents=True, exist_ok=True); b.OUT = OUT; b.COLORS = COLORS
    for female, label in [(False, 'Male'), (True, 'Female')]:
        part(label+'Head', lambda m: fp.head(m, female))
        part(label+'FaceDetails', lambda m: fp.eyes(m, female))
        part(label+'Brows', lambda m: fp.brows(m, female, 'Brow'))
        for style in fp.HAIR_STYLES: part(label+'Hair'+style, lambda m: fp.hair(m, female, style))
        part(label+'Torso', lambda m: fp.torso(m, female, cloth='Cloth', hem='ClothDark', collar='ClothDark'))
        part(label+'ShirtPockets', lambda m: pockets(m, female))
        part(label+'RagPatches', lambda m: rag_patches(m, female))
        part(label+'PackStraps', lambda m: straps(m, female))
        part(label+'AdvancedTrim', lambda m: trim(m, female))
        part(label+'Hips', lambda m: fp.hips(m, female, cloth='Pants', band='Pants'))
        part(label+'Thigh', lambda m: fp.thigh(m, 'Pants', female))
    part('Beard', lambda m: fp.beard(m, 'Hair', 'HairDark'))
    part('Neck', lambda m: k.limb(m, [((0, .43, -.012), .052, .050), ((0, .50, -.010), .050, .048), ((0, .54, -.012), .048, .046)], 'Skin', sides=6))
    part('UpperArm', lambda m: fp.sleeve(m, fp.UPPER_ARM-.02, 'Cloth', 'ClothDark', rolled=True))
    part('Forearm', lambda m: fp.forearm(m))
    part('Hand', lambda m: fp.hand(m))
    part('Shin', lambda m: fp.shin(m, 'Pants', 'PantsDark'))
    part('Boot', lambda m: fp.boot(m, 'Boot', 'Sole', 'LeatherDark', 'Metal'))
    part('BareFoot', lambda m: fp.bare_foot(m))
    part('Belt', belt); part('FurCap', fur_cap); part('FeatherTrim', feather_trim); part('ClothHood', cloth_hood)
    part('BackpackBody', backpack_body); part('RagBackpack', rag_backpack); part('PackFrame', pack_frame); part('PackRoll', pack_roll)
    b.palette('CampNpcPalette.png')
    schemes = [('Tier00Rags', (137, 119, 86), (92, 75, 53), (114, 93, 62)),
               ('Tier01HempRabbit', (178, 165, 111), (133, 102, 66), (163, 139, 91)),
               ('Tier02FlaxBeaver', (145, 155, 143), (105, 74, 48), (141, 120, 85)),
               ('Tier03CottonFeather', (200, 192, 164), (135, 91, 58), (169, 146, 99)),
               ('Tier04CottonBoar', (110, 119, 104), (67, 56, 45), (121, 101, 70))]
    for name, cloth, leather, packcolor in schemes:
        colors = list(COLORS)
        for idx, c in [(8, cloth), (9, tuple(round(v*.78) for v in cloth)), (10, tuple(min(255, round(v*1.13)) for v in cloth)),
                       (11, leather), (12, tuple(round(v*.72) for v in leather)), (13, tuple(min(255, round(v*1.25)) for v in leather)),
                       (14, leather), (16, packcolor), (17, leather)]:
            colors[idx] = c
        b.COLORS = colors; b.palette(name+'Palette.png')
    b.COLORS = COLORS
    (OUT/'MeshManifest.json').write_text(json.dumps(dict(Revision=4, Style='faceted', Parts=stats,
        Rig='Shared faceted skeleton (faceted_people): hips pivot 0.50 seated, torso +0.06, head +0.49, shoulders (0.205|0.188, 0.415|0.400), '
            'upper arm 0.29, forearm 0.25, thigh 0.38, shin 0.40, ankle 0.10. Flat normals. Right hand is mirrored.'), indent=2)+'\n', encoding='utf-8')
    print('Camp NPC parts:', len(stats), 'total triangles', sum(s['Triangles'] for s in stats))

if __name__ == '__main__':
    main()
