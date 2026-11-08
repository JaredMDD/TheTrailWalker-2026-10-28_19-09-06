"""Mission lady and ferryman, faceted revision 2 (approved style, user, November 2, 2026). Art only, no mission,
animation, spawn or inventory logic. Built on the shared faceted skeleton (faceted_people); part names, palette and
the SpecialNpcArtAssets pose builder are kept. Props (recipe parchment, oar, oar grip) keep their revision-1 shapes.
"""
from pathlib import Path
import math, json
import generate_region01_meshes as b
import natural_character_geometry as n
import faceted_character_kit as k
import faceted_people as fp

ROOT = Path(__file__).resolve().parents[2]; OUT = ROOT/'Assets/_Game/Art/Characters/CampSpecial'
COLORS = [(242, 237, 222), (205, 199, 184), (255, 249, 230), (31, 27, 22), (72, 54, 35), (225, 219, 200), (21, 22, 19), (117, 78, 57),
          (54, 42, 65), (38, 31, 47), (31, 36, 48), (58, 40, 33), (35, 30, 44), (96, 64, 86), (212, 204, 177), (105, 120, 123),
          (49, 65, 69), (54, 59, 57), (176, 144, 82), (130, 138, 132), (88, 58, 77), (130, 90, 49), (163, 140, 99), (76, 65, 49),
          (215, 199, 153), (175, 155, 114), (128, 45, 38), (237, 221, 177), (23, 23, 31), (78, 84, 79), (125, 106, 71), (33, 30, 28)]
PAL = fp.SlotPalette(len(COLORS), {
    'Skin': 0, 'SkinShade': 1, 'Hair': 0, 'HairDark': 1, 'Brow': 0, 'Eye': 6, 'Lips': 7,
    'Dress': 8, 'DressDark': 9, 'Coat': 10, 'Leather': 11, 'Hat': 12, 'Ribbon': 13, 'Ivory': 14, 'Shirt': 15, 'Waistcoat': 16,
    'Trousers': 17, 'Gold': 18, 'Lining': 20, 'Wood': 21, 'Rope': 22, 'HatUnder': 28, 'Cap': 29, 'Sole': 31,
    # Shared-part names mapped onto these outfits.
    'Cloth': 15, 'ClothDark': 16, 'Pants': 17, 'PantsDark': 31, 'Boot': 11, 'LeatherDark': 31, 'Metal': 18})
STATS = []
HEADER = 'The Trail Walker special NPC part (faceted revision 2); metres; +Y up; +Z forward; rig-frame local.'

def part(name, build):
    m = k.Mesh(name, PAL); build(m); s = m.save(OUT, HEADER)
    STATS.append(dict(Name=name, Triangles=s['triangles'], BoundsMin=[min(v[i] for v in m.vertices) for i in range(3)], BoundsMax=[max(v[i] for v in m.vertices) for i in range(3)]))

def legacy(name, build):
    """Revision-1 prop geometry (smooth curves suit paper and wood); written in the palette-strip format."""
    m = b.Model(name); build(m); m.save()
    STATS.append(dict(Name=name, Triangles=len(m.faces), BoundsMin=[min(v[i] for v in m.vertices) for i in range(3)], BoundsMax=[max(v[i] for v in m.vertices) for i in range(3)]))

def pleated(w, f, bk, count=16, depth=.05):
    """Angular skirt section: alternating radii make crisp pleats."""
    pts = []
    for i in range(count):
        a = i*math.tau/count; r = 1.0 if i % 2 == 0 else 1-depth
        pts.append((w*math.cos(a)*r, (f if math.sin(a) > 0 else bk)*math.sin(a)*r))
    return pts

def open_shell(m, rows, color, inner, opening_deg, edge=None, edge_size=.012):
    """Garment shell open at the front (+Z): rows (y, w, front, back); double sided; optional trimmed edges."""
    n_ = 12; half = math.radians(opening_deg/2)
    a0 = math.pi/2 + half; span = math.tau - 2*half
    rings = []
    for y, w, f, bk in rows:
        ring = []
        for i in range(n_+1):
            a = a0 + i*span/n_
            # Squared (superelliptic) section so the shell clears the angular torso at its corners.
            c, sn = math.cos(a), math.sin(a)
            x = w*math.copysign(abs(c)**.55, c); z = (f if sn > 0 else bk)*math.copysign(abs(sn)**.55, sn)
            ring.append(m.v((x, y, z)))
        rings.append(ring)
    for r in range(len(rings)-1):
        for i in range(n_):
            q = (rings[r][i], rings[r][i+1], rings[r+1][i+1], rings[r+1][i])
            m.quad(*q, color, (0, (rows[r][0]+rows[r+1][0])/2, 0))
            m.faces.append((q[0], q[2], q[1], PAL.index(inner))); m.faces.append((q[0], q[3], q[2], PAL.index(inner)))
    if edge:
        for e in (0, n_):
            for r in range(len(rings)-1):
                a, c = m.vertices[rings[r][e]], m.vertices[rings[r+1][e]]
                along = k.unit(k.sub(c, a))
                k.box(m, k.lerp(a, c, .5), (edge_size, k.length(k.sub(c, a))+.004, edge_size), edge, axes=((1, 0, 0), along, k.unit(k.cross((1, 0, 0), along))))
        for i in range(n_):
            a, c = m.vertices[rings[0][i]], m.vertices[rings[0][i+1]]
            along = k.unit(k.sub(c, a))
            k.box(m, k.lerp(a, c, .5), (k.length(k.sub(c, a))+.004, edge_size, edge_size), edge, axes=(along, (0, 1, 0), k.cross(along, (0, 1, 0))))

# ------------------------------------------------------------------ mission lady

def lady_dress(m):
    rows = [(.165, .160, .102, .100), (.060, .190, .124, .128), (-.150, .220, .158, .162), (-.450, .258, .196, .200), (-.790, .298, .236, .240)]
    rings = []
    for y, w, f, bk in rows:
        c = (0, y, 0); rings.append((c, [(x, y, z) for x, z in pleated(w, f, bk)]))
    k.loft(m, rings, 'Dress', cap_end=False, colors=['Dress', 'Dress', 'Dress', lambda i: 'DressDark' if i % 2 else 'Dress'])
    # Gold hem band.
    y, w, f, bk = rows[-1]
    hem = [((0, y, 0), [(x, y, z) for x, z in pleated(w+.004, f+.004, bk+.004)]), ((0, y+.030, 0), [(x, y+.030, z) for x, z in pleated(w-.004, f-.004, bk-.004)])]
    k.loft(m, hem, 'Gold', cap_start=False, cap_end=False)

def lady_bodice(m):
    fp.torso(m, True, cloth='Dress', hem='Dress', collar='Ivory', hem_band=False)
    top = fp.FEMALE_TORSO[-1]
    # Ivory collar tabs and an old-gold brooch; small buttons down the front.
    for s in (-1, 1):
        k.box(m, (s*.030, top[0]-.020, fp.torso_front(True, s*.03, top[0]-.02)+.008), (.040, .045, .008), 'Ivory',
              axes=((1, s*.5, 0), (-s*.5, 1, 0), (0, 0, 1)), taper=.6)
    k.box(m, (0, top[0]-.055, fp.torso_front(True, 0, top[0]-.055)+.012), (.026, .030, .010), 'Gold')
    for y in (.06, .14, .22, .30):
        k.box(m, (0, y, fp.torso_front(True, 0, y)+.004), (.010, .010, .006), 'Gold')

def lady_coat(m):
    rows = [(.455, .150, .086, .086), (.405, .200, .106, .110), (.330, .200, .148, .122), (.200, .180, .138, .122),
            (.020, .200, .136, .140), (-.200, .232, .170, .176), (-.480, .268, .206, .212)]
    open_shell(m, rows, 'Coat', 'Lining', 70, edge='Gold', edge_size=.010)
    # Turned-back lapels over the shoulders.
    for s in (-1, 1):
        k.box(m, (s*.080, .385, .108), (.060, .100, .012), 'Coat', axes=((1, -s*.4, 0), (s*.4, 1, .2), (0, -.2, 1)), taper=.5)

def lady_hat(m):
    # Broad flat brim (two-sided) with a crown, ribbon band and a folded bow.
    n_ = 16; y = .222
    top = [m.v((.300*math.cos(i*math.tau/n_), y+.010*math.cos(2*i*math.tau/n_), .260*math.sin(i*math.tau/n_)-.010)) for i in range(n_)]
    low = [m.v((x, yy-.010, z)) for x, yy, z in (m.vertices[t] for t in top)]
    inner_top = [m.v((.100*math.cos(i*math.tau/n_), y+.004, .100*math.sin(i*math.tau/n_)-.010)) for i in range(n_)]
    inner_low = [m.v((x, yy-.010, z)) for x, yy, z in (m.vertices[t] for t in inner_top)]
    for i in range(n_):
        j = (i+1) % n_
        m.quad(inner_top[i], inner_top[j], top[j], top[i], 'Hat', (0, y-.5, 0))
        m.quad(inner_low[i], low[i], low[j], inner_low[j], 'HatUnder', (0, y+.5, 0))
        m.quad(top[i], top[j], low[j], low[i], 'Hat', (0, y, 0))
    k.vertical_loft(m, [(.205, .104, .112, .118), (.250, .108, .114, .120), (.330, .090, .094, .100), (.352, .050, .054, .058)], 'Hat', a=.62, b=.55, zc=-.012, cap_start=False)
    k.vertical_loft(m, [(.230, .110, .117, .123), (.262, .110, .117, .123)], 'Ribbon', a=.62, b=.55, zc=-.012, cap_start=False, cap_end=False)
    for dz in (-.025, .015): k.box(m, (-.120, .250, -.030+dz), (.020, .040, .040), 'Ribbon', taper=.6)

def lady_parts():
    part('LadyHead', lambda m: fp.head(m, True))
    part('LadyFaceDetails', lambda m: fp.eyes(m, True))
    def hair(m):
        fp.hair(m, True, 'Tied'); fp.brows(m, True, 'Hair')
        k.box(m, (0, .150, -.140), (.070, .060, .050), 'HairDark', taper=.8)   # low bun under the hat
    part('LadyHair', hair)
    part('LadyLongDress', lady_dress); part('LadyFittedBodice', lady_bodice); part('LadyOpenOvercoat', lady_coat); part('LadyBroadHat', lady_hat)
    part('LadySleeve', lambda m: fp.sleeve(m, fp.UPPER_ARM, 'Coat', 'Coat', radius=1.05))
    part('LadyForeSleeve', lambda m: fp.forearm(m, sleeve_color='Coat', cuff='Coat'))
    part('LadyIvoryCuff', lambda m: k.limb(m, [((0, -fp.FOREARM+.030, 0), .050, .049), ((0, -fp.FOREARM-.002, 0), .046, .045)], 'Ivory', sides=8, turn=math.pi/8))
    part('LadyHiddenThigh', lambda m: fp.thigh(m, 'Dress', True))
    part('LadyHiddenShin', lambda m: fp.shin(m, 'Dress', 'Dress'))

# ------------------------------------------------------------------ ferryman

BULK = 1.06

def ferryman_parts():
    part('FerrymanHead', lambda m: fp.head(m, False))
    part('FerrymanFaceDetails', lambda m: fp.eyes(m, False))
    part('FerrymanHair', lambda m: (fp.hair(m, False, 'Crop'), fp.brows(m, False, 'Hair')))
    part('FerrymanShortBeard', lambda m: fp.beard(m, 'Hair', 'HairDark'))
    part('FerrymanShirt', lambda m: fp.torso(m, False, cloth='Shirt', hem='Shirt', collar='Shirt', bulk=BULK, hem_band=False))
    def waistcoat(m):
        rows = [(y, w+.010, f+.012, bk+.010) for y, w, f, bk in fp.torso_sections(False, BULK) if -.06 <= y <= .44]
        open_shell(m, rows, 'Waistcoat', 'Waistcoat', 40, edge='Leather', edge_size=.010)
        for y in (.05, .13, .21): k.box(m, (.048, y, fp.torso_front(False, .048, y, BULK)+.018), (.012, .012, .008), 'Gold')
    part('FerrymanWaistcoat', waistcoat)
    def cap(m):
        k.vertical_loft(m, [(.200, .092, .112, .118), (.236, .100, .124, .124), (.270, .090, .112, .116), (.292, .050, .060, .066)], 'Cap', a=.62, b=.55, cap_start=False)
        k.vertical_loft(m, [(.196, .093, .113, .119), (.214, .094, .114, .120)], 'Leather', a=.62, b=.55, cap_start=False, cap_end=False)
        k.box(m, (0, .204, .140), (.150, .010, .070), 'Cap', axes=((1, 0, 0), (0, 1, -.25), (0, .25, 1)), taper=.75)
    part('FerrymanCap', cap)
    part('FerrymanUpperArm', lambda m: fp.sleeve(m, fp.UPPER_ARM-.03, 'Shirt', 'Shirt', rolled=True, radius=1.06))
    part('FerrymanRolledCuff', lambda m: k.limb(m, [((0, -fp.UPPER_ARM+.050, 0), .068, .070), ((0, -fp.UPPER_ARM+.026, 0), .068, .070)], 'Shirt', sides=8, turn=math.pi/8))
    part('FerrymanForearm', lambda m: fp.forearm(m))
    part('FerrymanHips', lambda m: fp.hips(m, False, cloth='Trousers', band='Trousers'))
    def rope(m):
        for dy in (.060, .078):
            w, f, bk = fp.section_at(fp.torso_sections(False, BULK), dy-fp.TORSO_UP)
            k.vertical_loft(m, [(dy-.007, w+.012, f+.012, bk+.012), (dy+.007, w+.012, f+.012, bk+.012)], 'Rope', a=.70, b=.62, cap_start=False, cap_end=False)
        k.limb(m, [((.090, .070, .135), .008, .008), ((.110, -.040, .140), .007, .007), ((.095, -.120, .132), .006, .006)], 'Rope', sides=5)
    part('FerrymanRopeBelt', rope)
    part('FerrymanThigh', lambda m: fp.thigh(m, 'Trousers'))
    part('FerrymanShin', lambda m: fp.shin(m, 'Trousers', 'Trousers'))

# ------------------------------------------------------------------ shared + legacy props (revision 1 shapes)

def recipe_sheet(m):
    nx = 10; ny = 12; front = []; back = []
    def curl(x, y): return .006*(x/.095)**2+.014*max(0, (y-.07)/.06)**2
    for j in range(ny+1):
        y = (j/ny-.5)*.26; f = []; r = []
        for i in range(nx+1):
            x = (i/nx-.5)*.19; z = curl(x, y); f.append(m.v((x, y, z))); r.append(m.v((x, y, z-.0018)))
        front.append(f); back.append(r)
    for j in range(ny):
        for i in range(nx):
            a, c, d, e = front[j][i], front[j][i+1], front[j+1][i+1], front[j+1][i]; m.tri(a, c, d, 24); m.tri(a, d, e, 24)
            a, c, d, e = back[j][i], back[j+1][i], back[j+1][i+1], back[j][i+1]; m.tri(a, c, d, 25); m.tri(a, d, e, 25)
    def ink(points, width=.0008): n.stroke(m, [(x, y, curl(x, y)+.0018) for x, y in points], width, 23, 4)
    ink([(-.06, .09), (.06, .09)], .0012); ink([(0, -.025), (0, .057)])
    for y, side in [(-.01, -1), (.012, 1), (.034, -1), (.048, 1)]: ink([(0, y), (side*.027, y+.009), (side*.038, y+.023)])
    for j in range(3): ink([(-.055, -.054-j*.014), (.045-j*.007, -.054-j*.014)], .00065)
    n.ellipse(m, (.058, -.093, curl(.058, -.093)+.003), (.012, .014, .002), 26, 16, 8)

def oar(m):
    n.surface(m, [(.015, 0, 0, .045, .009), (.06, 0, 0, .105, .012), (.28, 0, 0, .10, .014), (.41, 0, 0, .05, .017), (.47, 0, 0, .019, .019)], 20, 21)
    n.stroke(m, [(0, .40, 0), (0, 1.95, 0)], .018, 21, 12)
    n.stroke(m, [(0, 1.90, 0), (0, 1.98, 0)], .021, 22, 12)

def oar_grip(m):
    # A faceted fist wrapped around the shaft (shaft along Y through the origin).
    k.box(m, (-.012, 0, -.004), (.050, .085, .056), 'Skin')
    for i, y in enumerate((-.030, -.010, .010, .030)):
        k.box(m, (.012, y, .022), (.034, .018, .022), 'Skin' if i % 2 else 'SkinShade')
    k.box(m, (-.018, .050, .014), (.020, .036, .022), 'Skin', axes=((1, 0, 0), (0, 1, .3), (0, -.3, 1)))

def main():
    OUT.mkdir(parents=True, exist_ok=True); b.OUT = OUT; b.COLORS = COLORS
    lady_parts(); ferryman_parts()
    part('SpecialNeck', lambda m: k.limb(m, [((0, .43, -.012), .046, .045), ((0, .50, -.010), .044, .043), ((0, .54, -.012), .042, .041)], 'Skin', sides=6))
    part('SpecialHand', lambda m: fp.hand(m))
    part('SpecialBoot', lambda m: fp.boot(m, 'Leather', 'Sole', 'Leather', 'Gold'))
    part('FerrymanOarGrip', oar_grip)
    legacy('RecipeParchmentVisual', recipe_sheet); legacy('FerrymanWoodenOar', oar)
    b.palette('CampSpecialPalette.png')
    (OUT/'MeshManifest.json').write_text(json.dumps(dict(Revision=2, Style='faceted', Parts=STATS,
        Policy='Character art and static pose studies only. No mission, recipe learning, handoff transaction, Animator or idle behavior. Recipe parchment is a display prop, never an inventory recipe stack.'), indent=2)+'\n', encoding='utf-8')
    print('Special NPC parts:', len(STATS))

if __name__ == '__main__':
    main()
