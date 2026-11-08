"""Balloon special gear refitted to the faceted player bodies (user request, November 7, 2026).

The October 31 worn meshes were built on the earlier smooth (revision-3) body and its ShirtAnchor/PantsAnchor. These
builders use the shared faceted body helpers (faceted_people) that the tier-one fitted garments use, so each garment
follows the current male and female bodies:
  hats    HeadAnchor/HatAnchor local (the head origin), hollow crown that fits the hat-compatible hair
  shirts  body-root space (replace the base tunic), pants body-root space (replace the base trousers)
  boots   body-root space, a pair; packs BackpackAnchor local (authored in root space, then offset)
Each tier keeps its October 31 identity: its 16-colour palette slots, the crimson-and-brass merchant signature with
the brass balloon badge, plumes and veils, and the visible pocket / tool-loop counts 2 / 3 / 5 / 8, which mirror the
tier's pocket and hook stats. Returns faceted_character_kit meshes whose face colours are palette slot numbers.
"""
import math
import faceted_character_kit as k
import faceted_people as fp
import generate_player_avatar_meshes as avatar

# The balloon palettes' slot roles (generate_balloon_goods.PALETTES).
SLOT = {'Main': 0, 'Second': 1, 'Leather': 2, 'LeatherDark': 3, 'Crimson': 4, 'CrimsonDark': 5, 'Brass': 6, 'Gold': 7,
        'Lining': 8, 'Sole': 9, 'Shell': 10, 'Wood': 11, 'WoodDark': 12, 'Feather': 13, 'White': 14, 'Dark': 15}
PAL = fp.SlotPalette(16, SLOT)
PACK = (0, 1.14, -.20)
COUNTS = [2, 3, 5, 8]                       # pockets on the shirt, tool loops on the pants (the tier's stats)


def mesh(name): return k.Mesh(name, PAL)


def badge(m, c, s=.020, facing=(0, 0, 1)):
    """Brass balloon silhouette (envelope, basket, two cords) facing +Z, or a given facing (unit, XZ plane)."""
    f = k.unit(facing); side = k.unit(k.cross((0, 1, 0), f))
    def at(dx, dy, dz): return k.add(c, k.add(k.add(k.mul(side, dx), (0, dy, 0)), k.mul(f, dz)))
    k.limb(m, [(at(0, -s*.9, 0), s*.25, s*.12), (at(0, -s*.3, 0), s*.95, s*.18), (at(0, s*.45, 0), s*.85, s*.18), (at(0, s*1.05, 0), s*.25, s*.10)],
           'Brass', sides=6, forward_hint=f)
    k.box(m, at(0, -s*1.55, 0), (s*.8, s*.45, s*.25), 'Brass', axes=(side, (0, 1, 0), f))
    for dx in (-1, 1):
        k.box(m, at(dx*s*.45, -s*1.15, 0), (s*.08, s*.55, s*.08), 'Gold', axes=(side, (0, 1, 0), f))


# ------------------------------------------------------------------ hats (head-local)

def crown(m, female, rows, color, top_color=None):
    """Hollow crown from the hairline up (the hat-compatible hair is cut at about 0.178 head height)."""
    sy = fp.head_sections(female)[-1][0]/.270
    k.vertical_loft(m, [(y*sy, w, f, b) for y, w, f, b in rows], color, a=.62, b=.55, cap_start=False,
                    colors=[color]*(len(rows)-2) + [top_color or color])
    return sy


def hat(tier, female):
    m = mesh(('Female' if female else 'Male')+'Hat')
    base = [(.194, .100, .123, .130), (.214, .100, .121, .129)]
    if tier == 3:
        # Dune wrap: a cloth wrap built up in bands, side veils behind the cheeks, crimson front band and badge.
        sy = crown(m, female, base + [(.246, .092, .108, .122), (.276, .066, .078, .094), (.292, .030, .036, .048)], 'Main')
        for i, y in enumerate((.200, .224, .248)):
            w = .104-.004*i
            k.vertical_loft(m, [((y-.008)*sy, w, w*1.20, w*1.27), ((y+.008)*sy, w-.002, w*1.18, w*1.25)], 'Second', a=.62, b=.55, cap_start=False, cap_end=False)
        for s in (-1, 1):
            k.box(m, (s*.104, .110*sy, -.040), (.018, .180, .120), 'Main', taper=.92)
        k.box(m, (0, .214*sy, .124), (.120, .024, .012), 'Crimson')
        badge(m, (.050, .214*sy, .133), .014)
        return m
    tall = .312 if tier == 1 else .300
    crown_color = 'Shell' if tier == 4 else 'Main'
    sy = crown(m, female, base + [(.258, .094, .110, .120), (.292, .068, .078, .092), (tall, .036, .042, .052)], crown_color)
    k.vertical_loft(m, [(.200*sy, .104, .127, .134), (.226*sy, .103, .125, .132)], 'Crimson', a=.62, b=.55, cap_start=False, cap_end=False)
    brim_color = 'Main' if tier == 1 else 'Leather' if tier == 2 else 'Shell'
    rx = .225 if tier in (1, 4) else .205
    # The rain hat's brim drops at the back (a rain shield); the sun hat's brim is widest.
    fp.brim(m, .196*sy, rx, rx*.86, .100, brim_color, 'Lining', z_offset=-.012 if tier != 2 else -.030, droop=.010 if tier != 2 else .028)
    badge(m, (.060, .214*sy, .128), .015)
    if tier in (1, 2):
        stem = [(-.090, .220*sy, -.010), (-.120, .300*sy, -.025), (-.140, .385*sy, -.040)]
        for a, b in zip(stem, stem[1:]): k.box(m, k.lerp(a, b, .5), (.006, k.length(k.sub(b, a)), .006), 'Gold', axes=((1, 0, 0), k.unit(k.sub(b, a)), (0, 0, 1)))
        k.limb(m, [((-.122, .300*sy, -.028), .006, .004), ((-.132, .345*sy, -.034), .024, .007), ((-.142, .405*sy, -.042), .006, .004)],
               'Feather', sides=6, forward_hint=(1, 0, 0))
    if tier == 4:
        for i in range(8):
            a = i*math.tau/8
            k.box(m, (.150*math.cos(a), .203*sy, .130*math.sin(a)-.012), (.026, .014, .020), 'White' if i % 2 else 'Gold')
    return m


# ------------------------------------------------------------------ shirts (body-root)

def pocket(m, female, x, y, w, h, tier, torso_y):
    z = fp.torso_front(female, x, y)
    if tier == 3:   # robe: narrow outlined slit with a brass pull, no flap
        k.box(m, (x, torso_y+y, z+.006), (w, .016, .010), 'Second')
        k.box(m, (x+w*.35, torso_y+y-.020, z+.012), (.010, .030, .008), 'Brass')
        return
    k.box(m, (x, torso_y+y-h*.30, z+.006), (w, h, .012), 'Main' if tier != 1 else 'Second')
    k.box(m, (x, torso_y+y+h*.25, z+.014), (w*1.06, h*.32, .012), 'Second' if tier != 1 else 'Main')
    k.box(m, (x, torso_y+y+h*.20, z+.022), (.012, .012, .006), 'Brass')


def shirt(tier, female):
    m = mesh(('Female' if female else 'Male')+'Shirt')
    torso_y = fp.HIPS_Y+fp.TORSO_UP
    body = 'Second' if tier == 1 else 'Main'
    fp.place(m, lambda mm: fp.torso(mm, female, cloth=body, hem='Crimson', collar='Crimson', bulk=1.0 if tier != 3 else 1.04), lambda p: k.add(p, (0, torso_y, 0)))
    count = COUNTS[tier-1]
    cols = 2 if count < 5 else 4
    for i in range(count):
        col, row = i % cols, i//cols
        x = (col-(cols-1)/2)*(.150 if cols == 2 else .072)
        y = (.27 if not female else .23)-row*.12
        pocket(m, female, x, y, .090 if cols == 2 else .058, .080 if cols == 2 else .062, tier, torso_y)
    for side in (-1, 1):
        from_shoulder, from_elbow, _ = avatar.arm_to_root(side, female)
        if tier in (3, 4):
            fp.place(m, lambda mm: fp.sleeve(mm, fp.UPPER_ARM+.02, body, body), from_shoulder)
            fp.place(m, lambda mm: fp.forearm(mm, sleeve_color=body, cuff='Crimson'), from_elbow)
        else:
            fp.place(m, lambda mm: fp.sleeve(mm, .24, body, 'Crimson', rolled=tier == 1), from_shoulder)
    if tier == 3:   # sash around the waist
        rows = []
        for dy in (.040, .085):
            w, f, bk = fp.section_at(fp.torso_sections(female, 1.04), dy)
            rows.append((torso_y+dy, w+.008, f+.008, bk+.008))
        k.vertical_loft(m, rows, 'Crimson', a=.70, b=.62, cap_start=False, cap_end=False)
    if tier == 4:   # crimson lapels in a V
        for s in (-1, 1):
            top = (s*.055, torso_y+.40, fp.torso_front(female, s*.055, .40)+.010)
            low = (s*.012, torso_y+.26, fp.torso_front(female, s*.012, .26)+.012)
            k.box(m, k.lerp(top, low, .5), (.034, k.length(k.sub(low, top)), .008), 'Crimson', axes=((1, 0, 0), k.unit(k.sub(top, low)), (0, 0, 1)))
    y = .31 if not female else .28
    badge(m, (.145 if not female else .130, torso_y+y, fp.torso_front(female, .14, y)+.016), .015)
    return m


# ------------------------------------------------------------------ pants (body-root)

def pants(tier, female):
    m = mesh(('Female' if female else 'Male')+'Pants')
    cloth = 'Main' if tier == 1 else 'Second'
    fp.place(m, lambda mm: fp.hips(mm, female, cloth=cloth, band=cloth), lambda p: k.add(p, (0, fp.HIPS_Y, 0)))
    belt = 'Leather' if tier != 4 else 'Lining'
    y0 = fp.TORSO_UP
    rows = []
    for dy in (-.002, .046):
        w, f, bk = fp.section_at(fp.torso_sections(female), dy)
        rows.append((fp.HIPS_Y+y0+dy, w+.010, f+.010, bk+.010))
    k.vertical_loft(m, rows, belt, a=.70, b=.62, cap_start=False, cap_end=False)
    w0, f0 = rows[0][1], rows[0][2]
    k.box(m, (0, fp.HIPS_Y+y0+.022, f0+.004), (.048, .040, .012), 'Crimson')
    count = COUNTS[tier-1]
    for i in range(count):   # tool loops spread around the front and sides of the belt
        a = math.pi/2 + (i-(count-1)/2)*(math.pi*.9/max(1, count-1) if count > 1 else 0)
        x, z = w0*1.02*math.cos(a), f0*1.02*math.sin(a) if math.sin(a) > 0 else rows[0][3]*math.sin(a)
        if abs(x) < .03 and z > 0: x += .045   # keep the buckle clear
        k.box(m, (x, fp.HIPS_Y+y0-.010, z), (.014, .060, .014), 'Brass' if i % 2 == 0 else 'LeatherDark')
    badge(m, (-(w0-.010), fp.HIPS_Y+y0+.020, f0*.55), .012, facing=(-.55, 0, .83))
    for side in (-1, 1):
        hip = (side*fp.HIP_X, fp.HIPS_Y, 0)
        fp.place(m, lambda mm: fp.thigh(mm, cloth, female), lambda p: k.add(p, hip))
        fp.place(m, lambda mm: fp.shin(mm, cloth, 'Crimson' if tier != 3 else cloth), lambda p: k.add(p, k.add(hip, (0, -fp.THIGH, 0))))
        knee = k.add(hip, (0, -fp.THIGH, 0))
        if tier == 1:
            k.box(m, (knee[0], knee[1]-.010, .082), (.090, .110, .016), 'Leather')
        if tier == 2:
            for dy in (-.170, -.060): k.box(m, (knee[0], knee[1]+dy, .078), (.080, .070, .014), 'LeatherDark')
        if tier in (3, 4):
            for dy in (-.30, -.33, -.36):
                k.vertical_loft(m, [(knee[1]+dy-.008, .059, .063, .063, .0), (knee[1]+dy+.008, .060, .064, .064, .0)],
                                'Main' if tier == 3 else 'Lining', a=.70, b=.62, x=knee[0], cap_start=False, cap_end=False)
    return m


# ------------------------------------------------------------------ boots (body-root pair)

def boots(tier, female):
    m = mesh(('Female' if female else 'Male')+'Boots')
    tall = .30 if tier == 2 else .17 if tier == 3 else .12
    leather = 'Second' if tier == 4 else 'Leather'
    cuff = 'Lining' if tier == 2 else 'Crimson'
    def one(mm):
        fp.boot(mm, leather, 'Sole', cuff, 'Brass', tall=tall)
        if tier == 2:   # high marsh-strider shaft flares toward the knee
            k.vertical_loft(mm, [(tall-.10, .062, .068, .066), (tall-.02, .070, .076, .074)], leather, a=.70, b=.62, cap_start=False, cap_end=False)
            k.box(mm, (.072, tall-.11, .015), (.020, .040, .060), 'Brass')
        if tier == 1:
            k.box(mm, (0, .040-fp.ANKLE_HEIGHT, .170), (.094, .040, .046), 'Brass', taper=.85)
        if tier == 3:
            for y in (.02, .06, .10, .14):
                k.vertical_loft(mm, [(y-.006, .058, .064, .062), (y+.006, .058, .064, .062)], 'Second', a=.70, b=.62, cap_start=False, cap_end=False)
        badge(mm, (0, tall-.040, .072), .010)
    for side in (-1, 1):
        fp.place(m, one, lambda p: k.add(p, (side*fp.HIP_X, fp.ANKLE_HEIGHT, 0)))
    return m


# ------------------------------------------------------------------ packs (BackpackAnchor-local)

def straps(m, female):
    """Shoulder straps routed over the torso like the fitted starter pack (rear ends at the pack's front face)."""
    sx = .158 if not female else .153
    crown_y = .94 + fp.torso_sections(female)[-1][0] + .012
    side_x = .197 if not female else .183
    for s in (-1, 1):
        pts = [(s*.115, 1.295, -.125), (s*.105, crown_y-.040, -.115), (s*.105, crown_y, -.065), (s*.105, crown_y, .045), (s*.110, crown_y-.025, .105)]
        for y in (.390, .350, .300, .240, .180, .120):
            pts.append((s*sx, .94+y, fp.torso_front(female, sx, y)+.016))
        pts += [(s*.120, 1.025, .140), (s*side_x, 1.035, .035), (s*side_x, 1.045, -.060), (s*(side_x-.010), 1.050, -.120), (s*.115, 1.055, -.128)]
        rings = []
        for i, p in enumerate(pts):
            tangent = k.unit(k.sub(pts[min(i+1, len(pts)-1)], pts[max(0, i-1)]))
            normal = k.unit(k.cross((1, 0, 0), tangent))
            rings.append((p, [k.add(p, k.add(k.mul((1, 0, 0), x*.0135), k.mul(normal, z*.003))) for x, z in ((-1, -1), (1, -1), (1, 1), (-1, 1))]))
        k.loft(m, rings, 'LeatherDark')


def pack(tier, female):
    m = mesh(('Female' if female else 'Male')+'Pack')
    front = -.118 if not female else -.110          # bag front just behind the shirt's back (-0.111 / -0.104)
    zc = front-.090
    if tier == 4:   # estuary sea chest: a boxy chest with crimson bands, brass corners and lid
        k.box(m, (0, 1.160, zc), (.300, .380, .180), 'Wood')
        for x in (-.105, -.035, .035, .105): k.box(m, (x, 1.160, zc-.091), (.012, .350, .010), 'Crimson')
        k.box(m, (0, 1.370, zc), (.316, .050, .192), 'WoodDark')
        for x in (-.140, .140):
            for y in (1.000, 1.320): k.box(m, (x, y, zc-.090), (.040, .040, .016), 'Brass')
    else:
        k.vertical_loft(m, [(.960, .120, .080, .085, zc), (1.000, .150, .088, .095, zc), (1.300, .150, .086, .094, zc), (1.360, .118, .070, .080, zc)],
                        'Main' if tier != 3 else 'Leather', a=.73, b=.65)
        k.vertical_loft(m, [(1.290, .158, .092, .100, zc), (1.340, .160, .094, .102, zc), (1.362, .130, .078, .088, zc)], 'Crimson', a=.73, b=.65)
        for x in (-.090, .090):
            k.box(m, (x, 1.150, zc-.096), (.026, .220, .010), 'LeatherDark')
            k.box(m, (x, 1.180, zc-.104), (.040, .034, .008), 'Brass')
        if tier == 1:   # ridge pack: a bedroll strapped on top
            k.limb(m, [((-.150, 1.420, zc), .052, .052), ((.150, 1.420, zc), .052, .052)], 'Second', sides=8, forward_hint=(0, 1, 0))
            for x in (-.080, .080): k.vertical_loft(m, [(1.360, .010, .058, .058, zc), (1.480, .010, .058, .058, zc)], 'LeatherDark', x=x, cap_start=False, cap_end=False)
        if tier == 2:   # taiga pack: fur trim and wooden side rails
            k.vertical_loft(m, [(1.300, .166, .100, .108, zc), (1.330, .168, .102, .110, zc)], 'Feather', a=.73, b=.65, cap_start=False, cap_end=False)
            for x in (-.165, .165): k.box(m, (x, 1.140, zc), (.022, .440, .022), 'Wood')
        if tier == 3:   # caravan saddle pack: two side bags
            for x in (-.190, .190):
                k.box(m, (x, 1.090, zc+.010), (.090, .200, .150), 'Main')
                k.box(m, (x, 1.200, zc+.010), (.100, .040, .156), 'Crimson')
    badge(m, (0, 1.220, zc-(.100 if tier != 4 else .094)), .020, facing=(0, 0, -1))
    straps(m, female)
    m.transform(lambda p: k.sub(p, PACK))
    return m
