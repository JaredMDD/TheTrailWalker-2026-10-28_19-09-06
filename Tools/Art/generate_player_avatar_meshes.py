"""Default player avatars, faceted revision 2 (approved style, user, November 2, 2026). Python standard library only.

Same contract as revision 1 (PlayerDefaultAvatarAssets builds the prefabs from these): part names, root-space body
parts, HeadAnchor-local head/eyes/hair, HeadAnchor at (0, 1.43, 0), material groups Skin, Hair, Brows, EyeWhite,
Iris, Pupil, Lips, BaseShirt, BasePants. Geometry comes from faceted_people (shared with camp NPCs and specials).
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k
import faceted_people as fp

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Characters/PlayerDefault'
MATERIALS = ['Skin', 'Hair', 'Brows', 'EyeWhite', 'Iris', 'Pupil', 'Lips', 'BaseShirt', 'BasePants',
             # Fitted tier-one wearables (user, November 5, 2026), in the crafted item models' colours.
             'HempShirt', 'HempShirtDark', 'HempPants', 'HempPantsDark', 'PeltLeather', 'PeltLeatherDark', 'PeltFur', 'PeltSole', 'Brass']
WEAR_COLORS = {'HempShirt': '#BDA978', 'HempShirtDark': '#806444', 'HempPants': '#8A784D', 'HempPantsDark': '#584A36',
               'PeltLeather': '#A7845A', 'PeltLeatherDark': '#774D2B', 'PeltFur': '#E2D0A2', 'PeltSole': '#4F311E', 'Brass': '#B8913A'}
# Item model name -> fitted wearable part (one per body). Read by PlayerDefaultAvatarAssets through the manifest.
WEARABLES = {'item.hemp_shirt': 'HempShirt', 'item.hemp_pants': 'HempPants', 'item.rabbit_boots': 'RabbitBoots'}
SKINS = ['#F2CCAA', '#D6A477', '#BA8055', '#966140', '#6E432E', '#452B20']
HAIRS = ['#241C17', '#513522', '#8D4E30', '#B99A60', '#858176']
STYLES = list(fp.HAIR_STYLES)
HEAD_ANCHOR = (0, 1.43, 0)
ARM_SPREAD, ELBOW_BEND = 9, 8
PAL = fp.MaterialPalette({'Skin': 'Skin', 'SkinShade': 'Skin', 'Lips': 'Lips', 'Eye': 'Pupil', 'Hair': 'Hair', 'HairDark': 'Hair',
                          'Brow': 'Brows', 'Cloth': 'BaseShirt', 'ClothDark': 'BaseShirt', 'Pants': 'BasePants', 'PantsDark': 'BasePants',
                          **{name: name for name in WEAR_COLORS}})
stats = []

def save(m):
    order = fp.save_groups(m, OUT, 'The Trail Walker player art; metres; +Y up; +Z forward.', 'PlayerDefault.mtl')
    assert all(all(math.isfinite(c) for c in v) for v in m.vertices), m.name
    stats.append({'name': m.name, 'vertices': len(m.vertices), 'triangles': len(m.faces), 'materials': order,
                  'boundsMin': [round(min(p[i] for p in m.vertices), 6) for i in range(3)],
                  'boundsMax': [round(max(p[i] for p in m.vertices), 6) for i in range(3)]})

def mirror_x(m, v_start, f_start):
    for i in range(v_start, len(m.vertices)): x, y, z = m.vertices[i]; m.vertices[i] = (-x, y, z)
    m.faces[f_start:] = [(a, c, b, kk) for a, b, c, kk in m.faces[f_start:]]

def arm_to_root(side, female):
    sx, sy = fp.SHOULDER[female]
    shoulder = (side*sx, fp.HIPS_Y+fp.TORSO_UP+sy, 0)
    def from_shoulder(p): p = fp.rot_z(p, side*ARM_SPREAD); return k.add(p, shoulder)
    def from_elbow(p): p = fp.rot_x(p, -ELBOW_BEND); return from_shoulder(k.add(p, (0, -fp.UPPER_ARM, 0)))
    def from_wrist(p): return from_elbow(k.add(p, (0, -fp.FOREARM, 0)))
    return from_shoulder, from_elbow, from_wrist

def body(female):
    label = 'Female' if female else 'Male'
    skin, shirt, pants = (k.Mesh(label+s, PAL) for s in ('Skin', 'BaseShirt', 'BasePants'))
    head = k.Mesh(label+'Head', PAL); fp.head(head, female); fp.brows(head, female, 'Brow'); save(head)
    eyes = k.Mesh(label+'Eyes', PAL); fp.eyes(eyes, female); save(eyes)
    torso_y = fp.HIPS_Y+fp.TORSO_UP
    k.limb(skin, [((0, torso_y+.43, -.012), .052 if not female else .046, .050 if not female else .045),
                  ((0, 1.44, -.010), .050 if not female else .044, .048 if not female else .043),
                  ((0, HEAD_ANCHOR[1]+.05, -.012), .048 if not female else .042, .046 if not female else .041)], 'Skin', sides=6)
    fp.place(shirt, lambda m: fp.torso(m, female), lambda p: k.add(p, (0, torso_y, 0)))
    fp.place(pants, lambda m: fp.hips(m, female), lambda p: k.add(p, (0, fp.HIPS_Y, 0)))
    for side in (-1, 1):
        from_shoulder, from_elbow, from_wrist = arm_to_root(side, female)
        fp.place(shirt, lambda m: fp.sleeve(m, .17), from_shoulder)
        fp.place(skin, lambda m: fp.upper_arm_skin(m, .12), from_shoulder)
        fp.place(skin, lambda m: fp.forearm(m), from_elbow)
        # Palm turned toward the thigh (hand frame -Z to the body side), thumb forward.
        # The right hand is the left hand mirrored about the body centre.
        v0, f0 = len(skin.vertices), len(skin.faces)
        _, _, left_wrist = arm_to_root(-1, female)
        fp.place(skin, lambda m: fp.hand(m), lambda p: left_wrist(k.rotate_y(p, -90)))
        if side > 0: mirror_x(skin, v0, f0)
        hip = (side*fp.HIP_X, fp.HIPS_Y, 0)
        fp.place(pants, lambda m: fp.thigh(m, female=female), lambda p: k.add(p, hip))
        fp.place(pants, lambda m: fp.shin(m), lambda p: k.add(p, k.add(hip, (0, -fp.THIGH, 0))))
        fp.place(skin, lambda m: fp.bare_foot(m), lambda p: k.add(p, (side*fp.HIP_X, fp.ANKLE_HEIGHT, 0)))
    for part in (skin, shirt, pants): save(part)
    for style in STYLES:
        h = k.Mesh(label+'Hair'+style, PAL); fp.hair(h, female, style); save(h)

# ------------------------------------------------------------------ fitted tier-one wearables (root space, standing)
# Equipment redesign, first set (user, November 5, 2026): shells on the same body landmarks as the basewear, so the
# equipment avatar shows what is worn. The shirt and pants replace the base tunic/trousers (the avatar hides those);
# the boots enclose the bare feet. Hat and backpack keep their item models at the anchors for now.

def shirt_pockets(m, female):
    """Torso frame: two chest pockets with flaps and a brass button (the shirt is what adds pockets)."""
    for s in (-1, 1):
        x, y = s*.085, .27 if not female else .23
        z = fp.torso_front(female, x, y)
        k.box(m, (x, y, z+.006), (.090, .085, .012), 'PeltLeather')
        k.box(m, (x, y+.046, z+.014), (.096, .022, .012), 'PeltLeatherDark')
        k.box(m, (x, y+.040, z+.022), (.012, .012, .006), 'Brass')

def hemp_shirt(female):
    m = k.Mesh(('Female' if female else 'Male')+'WearHempShirt', PAL)
    torso_y = fp.HIPS_Y+fp.TORSO_UP
    fp.place(m, lambda mm: fp.torso(mm, female, cloth='HempShirt', hem='HempShirtDark', collar='HempShirtDark'), lambda p: k.add(p, (0, torso_y, 0)))
    fp.place(m, lambda mm: shirt_pockets(mm, female), lambda p: k.add(p, (0, torso_y, 0)))
    for side in (-1, 1):
        from_shoulder, _, _ = arm_to_root(side, female)
        # Longer rolled sleeves than the base tunic, so the shirt reads as a different garment.
        fp.place(m, lambda mm: fp.sleeve(mm, .24, 'HempShirt', 'HempShirtDark', rolled=True), from_shoulder)
    return m

def belt_with_loops(m, female):
    """Hips frame: belt over the waist and two tool loops at the sides (the pants are what add hooks)."""
    y0 = fp.TORSO_UP
    rows = []
    for dy in (-.002, .046):
        w, f, bk = fp.section_at(fp.torso_sections(female), dy)
        rows.append((y0+dy, w+.010, f+.010, bk+.010))
    k.vertical_loft(m, rows, 'PeltLeather', a=.70, b=.62, cap_start=False, cap_end=False, colors=['PeltLeather'])
    f, w = rows[0][2], rows[0][1]
    k.box(m, (0, y0+.022, f+.004), (.052, .044, .012), 'Brass')
    k.box(m, (0, y0+.022, f+.010), (.028, .022, .006), 'PeltLeatherDark')
    for s in (-1, 1):
        k.box(m, (s*(w+.006), y0-.012, 0), (.014, .070, .034), 'PeltLeatherDark')
        k.box(m, (s*(w+.012), y0-.050, 0), (.010, .014, .040), 'Brass')

def hemp_pants(female):
    m = k.Mesh(('Female' if female else 'Male')+'WearHempPants', PAL)
    fp.place(m, lambda mm: fp.hips(mm, female, cloth='HempPants', band='HempPantsDark'), lambda p: k.add(p, (0, fp.HIPS_Y, 0)))
    fp.place(m, lambda mm: belt_with_loops(mm, female), lambda p: k.add(p, (0, fp.HIPS_Y, 0)))
    for side in (-1, 1):
        hip = (side*fp.HIP_X, fp.HIPS_Y, 0)
        fp.place(m, lambda mm: fp.thigh(mm, 'HempPants', female), lambda p: k.add(p, hip))
        fp.place(m, lambda mm: fp.shin(mm, 'HempPants', 'HempPantsDark'), lambda p: k.add(p, k.add(hip, (0, -fp.THIGH, 0))))
    return m

def rabbit_boots(female):
    m = k.Mesh(('Female' if female else 'Male')+'WearRabbitBoots', PAL)
    def one(mm):
        fp.boot(mm, 'PeltLeather', 'PeltSole', 'PeltFur', 'PeltLeatherDark', tall=.10)
        # Rabbit-fur cuff over the boot top, wider than the trouser hem so the two never share a surface.
        k.vertical_loft(mm, [(.055, .066, .074, .072), (.085, .072, .080, .078), (.112, .066, .074, .072)], 'PeltFur', a=.70, b=.62)
    for side in (-1, 1):
        fp.place(m, one, lambda p: k.add(p, (side*fp.HIP_X, fp.ANKLE_HEIGHT, 0)))
    return m

def wearables():
    for female in (False, True):
        for build in (hemp_shirt, hemp_pants, rabbit_boots): save(build(female))

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    colors = [SKINS[2], HAIRS[1], HAIRS[1], '#F5EDDA', '#483623', '#1C1612', '#754E39', '#99876A', '#655740'] + [WEAR_COLORS[n] for n in MATERIALS[9:]]
    mtl = []
    for name, color in zip(MATERIALS, colors):
        rgb = [int(color[i:i+2], 16)/255 for i in (1, 3, 5)]
        mtl += ['newmtl '+name, 'Kd '+' '.join(f'{c:.5f}' for c in rgb), 'Ks 0 0 0', 'Ns 1', '']
    (OUT/'PlayerDefault.mtl').write_text('\n'.join(mtl), encoding='utf-8')
    body(False); body(True); wearables()
    manifest = {'wearables': [{'itemId': item, 'part': part} for item, part in WEARABLES.items()],'revision': 2, 'style': 'faceted', 'units': 'metres', 'forward': '+Z', 'up': '+Y', 'headOrigin': list(HEAD_ANCHOR),
                'bodyVariants': ['Male', 'Female'], 'skinColors': SKINS, 'hairColors': HAIRS, 'hairStyles': STYLES,
                'defaultHair': {'Male': 'Sweep', 'Female': 'Tied'}, 'parts': stats,
                'scope': 'Static modular art only. No selection, persistence, gameplay, skin weights or animation.',
                'anchors': {'Hat': list(HEAD_ANCHOR), 'Backpack': [0, 1.14, -.20], 'Shirt': [0, .95, 0],
                            'Pants': [0, .89, 0], 'LeftBoot': [-fp.HIP_X, .10, 0], 'RightBoot': [fp.HIP_X, .10, 0]}}
    (OUT/'PlayerAvatarManifest.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
    print(f'Exported {len(stats)} faceted player parts:', ', '.join(f"{s['name']} {s['triangles']}" for s in stats))

if __name__ == '__main__':
    main()
