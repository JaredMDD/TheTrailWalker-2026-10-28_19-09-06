"""Two tier-one ranch dishes from the mystery-box recipes (Milestone 14 slice 3, Ruling 3): chicken and mushroom pie, and
apple-glazed chicken. They replace the Premium apple pie and Roasted rabbit stand-ins. Python 3 standard library only.

Same contract as generate_bakery_foods.py: local Y-up OBJ (front faces -Z), palette-strip UVs, detail TYPE in palette alpha
(none: prepared foods stay flat), one shared palette for the two, bounds centred. Sized against Berry oat cakes (largest side
0.72, kept within 0.8-1.25x) and kept under 1.25x Premium apple pie's triangles.
Run: python Tools/Art/generate_ranch_dishes.py   (writes only the two OBJs and RanchDishesPalette.png)
"""
import sys, math
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
import generate_region01_meshes as base
import detail_textures as detail
import generate_chicken_meat as chicken
from generate_region05_meshes import sphere
from generate_bakery_foods import closed, turn, MAX_SIDE, MAX_TRIS

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Art/Items/Ranch'
PAL = OUT / 'RanchDishesPalette.png'
COLORS = [(128, 72, 36), (196, 124, 58), (226, 170, 96), (242, 198, 124),    # 0 dark crust, 1 golden crust, 2 light crust, 3 crust highlight
          (238, 216, 168), (226, 190, 146), (118, 82, 56), (200, 170, 128),   # 4 cream sauce, 5 chicken, 6 mushroom cap, 7 mushroom flesh
          (88, 128, 56), (198, 104, 34), (142, 60, 22), (246, 176, 82),       # 8 herb, 9 glaze amber, 10 glaze dark, 11 glaze gloss
          (184, 38, 36), (242, 228, 174), (238, 230, 208), (232, 226, 212),   # 12 apple skin, 13 apple flesh, 14 bone, 15 plate
          (74, 104, 148), (206, 150, 84)]                                      # 16 plate rim, 17 baked filling between the lattice


def tri_out(m, a, b, c, col, inside):
    """Triangle wound away from `inside` (the outward winding closed() checks): cut-plane fans and boxes."""
    pa, pb, pc = (m.vertices[v - 1] for v in (a, b, c))
    u, w = [pb[n] - pa[n] for n in range(3)], [pc[n] - pa[n] for n in range(3)]
    normal = (u[1] * w[2] - u[2] * w[1], u[2] * w[0] - u[0] * w[2], u[0] * w[1] - u[1] * w[0])
    out = sum(normal[n] * ((pa[n] + pb[n] + pc[n]) / 3 - inside[n]) for n in range(3)) >= 0
    m.tri(a, b, c, col) if out else m.tri(a, c, b, col)


def lathe(m, profile, colors, segments, a0=0.0, a1=math.tau, centre=(0, 0, 0), radius=lambda r, a: r, squash=1.0, cut=None):
    """Revolution of an (r, y) profile running bottom axis -> outside -> top axis (counter-clockwise), from angle a0 to a1 in
    the x-z plane. colors[i] colours band i. A partial sweep closes both cut planes, each a fan coloured by cut(r, y).
    radius(r, a) reshapes rings (pie crimp); squash scales z (oval plate)."""
    full = abs(a1 - a0 - math.tau) < 1e-9
    steps = segments if full else segments + 1
    at = lambda r, y, a: (centre[0] + radius(r, a) * math.cos(a), centre[1] + y, centre[2] + radius(r, a) * math.sin(a) * squash)
    angles = [a0 + (a1 - a0) * i / segments for i in range(steps)]
    rings = [[m.v(at(r, y, a)) for a in angles] if r else [m.v(at(0, y, 0))] for r, y in profile]
    # Winding by construction: (profile step) x (angle step) points outward for a counter-clockwise profile, whatever the
    # crimp does to the rings (an inside-point test is not robust there).
    for n in range(len(profile) - 1):
        ra, rb = rings[n], rings[n + 1]
        for i in range(segments):
            j = (i + 1) % steps
            if len(ra) == 1: m.tri(ra[0], rb[i], rb[j], colors[n])
            elif len(rb) == 1: m.tri(ra[i], rb[0], ra[j], colors[n])
            else: m.tri(ra[i], rb[i], rb[j], colors[n]); m.tri(ra[i], rb[j], ra[j], colors[n])
    if full: return
    back = at(.5 * max(r for r, _ in profile), sum(y for _, y in profile) / len(profile), (a0 + a1) / 2)   # inside the sweep
    for a, ring_index in ((a0, 0), (a1, steps - 1)):
        pts = [(r, y) for r, y in profile]
        cr, cy = sum(r for r, _ in pts) / len(pts), sum(y for _, y in pts) / len(pts)
        c = m.v(at(cr, cy, a))
        ids = [ring[0] if len(ring) == 1 else ring[ring_index] for ring in rings]
        for (p, q), (va, vb) in zip(zip(pts, pts[1:] + pts[:1]), zip(ids, ids[1:] + ids[:1])):
            if p[0] == 0 and q[0] == 0: continue   # the axis edge
            tri_out(m, c, va, vb, cut((p[0] + q[0] + cr) / 3, (p[1] + q[1] + cy) / 3), back)


def blob(m, centre, radii, color, yaw=0.0, n=6, rings=3, part='blob'):
    """sphere() turned about Y (yaw) at its centre; its winding is checked."""
    s, v = len(m.faces), len(m.vertices)
    sphere(m, centre, radii, color, n, rings)
    if yaw: turn(m, v, centre, 'y', yaw)
    closed(m, s, part)


def chicken_mushroom_pie():
    """A deep pie with a crimped golden rim and a woven lattice, one wedge cut away toward the front: the cut faces show the
    crust wall, cream sauce, chicken pieces and mushroom slices."""
    m = base.Model('ChickenMushroomPie')
    half = math.radians(27)                       # half the missing wedge, centred on -Z (the front)
    a0, a1 = -math.pi / 2 + half, 3 * math.pi / 2 - half
    crimp = lambda r, a: r * (1 + .035 * math.cos(16 * a)) if r > .28 else r
    profile = [(0, -.06), (.255, -.06), (.288, -.04), (.302, .005), (.314, .036), (.29, .052), (.262, .042), (.25, .032), (0, .037)]
    lathe(m, profile, [0, 1, 1, 2, 3, 2, 1, 17], 44, a0, a1, radius=crimp, cut=lambda r, y: 2)   # cut planes in crust colour
    # Lattice: strips along X over strips along Z, clipped to the inner rim and kept out of the missing wedge.
    R, w, tn = .258, .046, math.tan(half)
    def strip(c, along_x, lo, hi):
        if hi - lo < .03: return
        mid, ln = (lo + hi) / 2, hi - lo
        x, z, sx, sz = (mid, c, ln, w) if along_x else (c, mid, w, ln)
        blob_box(m, (x, .043 if along_x else .049, z), (sx, .014, sz), 1 if (round(c * 10) % 2) else 3)
    for c in (-.2, -.1, 0, .1, .2):
        L = math.sqrt(R * R - c * c)
        if c <= 0:   # an X strip at the front crosses the wedge (|x| <= -z tan(half) for z < 0): keep both ends
            gap = (-c + w / 2) * tn + .008
            strip(c, True, -L, -gap); strip(c, True, gap, L)
        else: strip(c, True, -L, L)
        near = max(0.0, abs(c) - w / 2)       # a Z strip's edge nearest the axis decides where the wedge starts
        strip(c, False, max(-L, -near / tn + .008), L)
    # Filling on both cut faces: a cream-sauce layer just proud of the crust-coloured cut plane, then chicken pieces and
    # mushroom slices (cap over pale flesh) half sunk into it.
    for a, side in ((a0, -1), (a1, 1)):
        normal_yaw = -a
        n = (math.sin(a), 0, -math.cos(a)) if side < 0 else (-math.sin(a), 0, math.cos(a))   # toward the gap
        layer = [(0.0, -.044), (.236, -.044), (.236, .022), (.25, .031), (0.0, .036)]
        pts = [m.v((r * math.cos(a) + n[0] * .0015, y, r * math.sin(a) + n[2] * .0015)) for r, y in layer]
        cr = sum(r for r, _ in layer) / len(layer); cy = sum(y for _, y in layer) / len(layer)
        c = m.v((cr * math.cos(a) + n[0] * .0015, cy, cr * math.sin(a) + n[2] * .0015))
        behind = (cr * math.cos(a) - n[0] * .05, cy, cr * math.sin(a) - n[2] * .05)
        for i in range(len(pts)): tri_out(m, c, pts[i], pts[(i + 1) % len(pts)], 4, behind)
        for r, y in ((.07, -.018), (.16, .004), (.215, -.022)):
            blob(m, (r * math.cos(a), y, r * math.sin(a)), (.032, .02, .026), 5, normal_yaw, part='chicken')
        for r, y in ((.115, -.024), (.19, .012)):
            blob(m, (r * math.cos(a), y, r * math.sin(a)), (.03, .016, .012), 7, normal_yaw, part='mushroom')
            blob(m, (r * math.cos(a), y + .014, r * math.sin(a)), (.032, .009, .013), 6, normal_yaw, part='cap')
    for x, z in ((-.12, .1), (.05, .17), (.15, -.02), (-.17, -.05), (.0, .02)):   # herb flecks in the lattice gaps
        blob(m, (x, .042, z), (.016, .004, .012), 8, x * 9, 4, 3, 'herb')
    return m


def blob_box(m, c, size, col):
    """Axis-aligned box, wound outward."""
    hx, hy, hz = (s / 2 for s in size)
    v = [m.v((c[0] + sx * hx, c[1] + sy * hy, c[2] + sz * hz)) for sx in (-1, 1) for sy in (-1, 1) for sz in (-1, 1)]
    for q in ((0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)):
        tri_out(m, v[q[0]], v[q[1]], v[q[2]], col, c); tri_out(m, v[q[0]], v[q[2]], v[q[3]], col, c)


DRUM = [(-.2, 0, 0), (-.19, .06, .055), (-.15, .105, .095), (-.08, .125, .11), (.0, .115, .1), (.07, .085, .075), (.12, .055, .05),
        (.155, .04, .036), (.17, 0, 0)]
BONE = [(.14, 0, 0), (.15, .024, .024), (.24, .022, .022), (.26, .04, .036), (.29, .036, .032), (.30, 0, 0)]


def glazed_drumstick(m, pivot, yaw):
    """A roast drumstick on generate_chicken_meat's loft (bumpy faceted skin): amber glaze, darker underside, glossy top facets,
    and the bone knuckle."""
    def glaze(k, i):
        up = math.cos((i + .5) * math.tau / chicken.SIDES)   # the loft's +Y side
        if up > .4 and (k + i) % 2 == 0: return 11
        return 10 if up < -.4 or (k * 3 + i) % 9 == 0 else 9
    v = len(m.vertices)
    s = len(m.faces); chicken.loft(m, DRUM, glaze, .06); closed(m, s, 'drumstick')
    s = len(m.faces); chicken.loft(m, BONE, 14); closed(m, s, 'bone')
    turn(m, v, (0, 0, 0), 'y', yaw)
    for n in range(v, len(m.vertices)):
        x, y, z = m.vertices[n]; m.vertices[n] = (x + pivot[0], y + pivot[1], z + pivot[2])


def apple_wedge(m, centre, yaw):
    """An apple wedge lying on its cut face: red skin outside, cream flesh on the cut planes."""
    v = len(m.vertices)
    profile = [(0, -.07), (.045, -.066), (.075, -.035), (.085, 0), (.075, .035), (.045, .062), (0, .066)]
    lathe(m, profile, [12] * 6, 4, -math.radians(26), math.radians(26), cut=lambda r, y: 13)
    turn(m, v, (0, 0, 0), 'z', math.pi / 2)     # lay it down on its core edge: the apple's axis along X, the skin arc on top
    turn(m, v, (0, 0, 0), 'y', yaw)
    lo = min(m.vertices[n][1] for n in range(v, len(m.vertices)))
    for n in range(v, len(m.vertices)):
        x, y, z = m.vertices[n]; m.vertices[n] = (x + centre[0], y - lo + centre[1], z + centre[2])


def apple_glazed_chicken():
    """Two glazed drumsticks side by side on an oval plate with a blue rim, bones toward the back right; three apple wedges in
    front, skin up; a herb sprig and glaze beads."""
    m = base.Model('AppleGlazedChicken')
    plate = [(0, -.035), (.27, -.035), (.35, -.008), (.37, .012), (.345, .014), (.29, -.004), (0, -.004)]
    lathe(m, plate, [15, 15, 16, 16, 15, 15], 24, squash=.7)
    lift = -.004 + .125                           # the drumsticks rest on the plate well
    glazed_drumstick(m, (-.03, lift, .1), -.18)
    glazed_drumstick(m, (.0, lift, -.04), -.32)
    for x, yaw in ((-.21, .25), (-.08, .05), (.05, -.15)):
        apple_wedge(m, (x, -.004, -.19), yaw)
    for x, z in ((-.12, .09), (-.05, .1), (-.08, -.05), (.0, -.03)):
        blob(m, (x, lift + .122, z), (.018, .009, .015), 11, part='glaze bead')
    for x, z, yaw in ((.2, -.15, .4), (.24, -.12, 1.3), (.22, -.19, 2.2)):
        blob(m, (x, .0, z), (.034, .006, .014), 8, yaw, part='herb leaf')
    return m


def save(m):
    lo = [min(v[i] for v in m.vertices) for i in range(3)]; hi = [max(v[i] for v in m.vertices) for i in range(3)]
    m.vertices = [tuple(p[i] - (hi[i] + lo[i]) / 2 for i in range(3)) for p in m.vertices]   # centre bounds like the tier 1 foods
    side = max(h - l for l, h in zip(lo, hi))
    assert MAX_SIDE[0] <= side <= MAX_SIDE[1] and len(m.faces) <= MAX_TRIS, (m.name, side, len(m.faces))
    text = ['# TheTrailWalker; local Y up; flat palette-mapped item mesh.', 'o ' + m.name]
    text += ['v %.7f %.7f %.7f' % v for v in m.vertices]
    text += ['vt %.7f 0.5' % ((i + .5) / len(COLORS)) for i in range(len(COLORS))] + ['s off']
    text += ['f %d/%d %d/%d %d/%d' % (a, k + 1, b, k + 1, c, k + 1) for a, b, c, k in m.faces]
    (OUT / (m.name + '.obj')).write_text('\n'.join(text) + '\n', encoding='utf-8')
    print(m.name, 'triangles', len(m.faces), 'bounds', [round(h - l, 3) for l, h in zip(lo, hi)])


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for build in (chicken_mushroom_pie, apple_glazed_chicken):
        save(build())
    px = [(*rgb, detail.alpha('none')) for rgb in COLORS]   # no surface detail, like the tier 1 prepared foods
    detail.write_png(PAL, len(COLORS) * 4, 4, [[p for p in px for _ in range(4)] for _ in range(4)])


if __name__ == '__main__':
    main()
