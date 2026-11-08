"""Tier-two loose resources (regions 7-12), design pass started November 6, 2026. Python stdlib only.

The 18 featured loose resources of tier two (PROJECT_RECOMENDATIONS.md section 6.2.2; IDs from Domain/Travel/RegionPlan.cs),
three per region, built with the tier-one loose-resource conventions (generate_region01..06_meshes.py): faceted
standard-library OBJ meshes about one unit across, front view in the XY plane (the bag picture and footprint come from
it), one small palette per region whose alpha carries the surface-detail type, flat normals on import.
  7 Whispering Marsh   BirchBranch (material.birch_branch), BeaverPelt (animal.beaver.pelt), Blackberries (food.blackberries)
  8 Amber Riverbanks   FlaxFiber (material.flax_fiber), CopperOre (material.copper_ore), Carrots (ingredient.carrots)
  9 Bluewater Basin    Barley (ingredient.barley), Quartz (material.quartz), Blueberries (food.blueberries)
 10 Sunlit Uplands     Sandstone (material.sandstone), Truffle (ingredient.truffle), FlaxSeeds (ingredient.flax_seeds)
 11 Mirror Lakes       Rice (ingredient.rice), Tomatoes (ingredient.tomatoes), Honey (ingredient.honey)
 12 Frostveil Taiga    TinOre (material.tin_ore), YellowOchre (material.yellow_ochre), CattailFiber (material.cattail_fiber)
Each design is distinct from its tier-one neighbour (birch is white-barked, not a recoloured pine branch; barley has long
awns unlike wheat; blackberries are drupelet clusters unlike the smooth red berries) and from the tier-two scenery.

Run: python Tools/Art/generate_tier02_loose_resources.py
Then The Trail Walker > Art > Rebuild Tier 2 Resource Prefabs, Apply Surface Detail, and Capture Tier 2 Resource Previews.
"""
from pathlib import Path
import json, math, random
import generate_region01_meshes as base
from generate_region05_meshes import sphere

ROOT = Path(__file__).resolve().parents[2]
STATS = []

def save(m, largest):
    m.save()
    lo = [min(p[i] for p in m.vertices) for i in range(3)]; hi = [max(p[i] for p in m.vertices) for i in range(3)]
    dx, dy = hi[0] - lo[0], hi[1] - lo[1]
    STATS.append(dict(Name=m.name, Triangles=len(m.faces), BoundsMin=[round(v, 4) for v in lo], BoundsMax=[round(v, 4) for v in hi],
                      ProposedFootprint=[round(largest * dx / max(dx, dy)), round(largest * dy / max(dx, dy))]))

def leaf(m, a, b, width, color, back):
    d = base.sub(b, a); length = math.hypot(d[0], d[1]) or 1e-6
    side = (-d[1] / length * width, d[0] / length * width, 0); mid = base.add(a, base.mul(d, .53))
    v = [m.v(p) for p in (a, base.add(mid, side), b, base.sub(mid, side), base.add(mid, (0, 0, -.03)))]
    for i in range(4):
        j = (i + 1) % 4; m.tri(v[i], v[4], v[j], color + i % 2); m.tri(v[j], v[4], v[i], back)

def lathe(m, profile, colors, n=12, squash=1.0, wobble=0.0, seed=0):
    """Revolved shape from (radius, y) rows; colors picks a slot per row band."""
    rng = random.Random(seed); rings = []
    for radius, y in profile:
        rings.append([m.v((radius * (1 + wobble * rng.uniform(-1, 1)) * math.cos(i * math.tau / n), y,
                           radius * squash * (1 + wobble * rng.uniform(-1, 1)) * math.sin(i * math.tau / n))) for i in range(n)])
    bottom = m.v((0, profile[0][1] - .01, 0)); top = m.v((0, profile[-1][1] + .005, 0))
    for i in range(n):
        j = (i + 1) % n; m.tri(bottom, rings[0][i], rings[0][j], colors(0, i)); m.tri(top, rings[-1][j], rings[-1][i], colors(len(rings) - 1, i))
        for k in range(len(rings) - 1):
            c = colors(k, i); m.tri(rings[k][i], rings[k + 1][i], rings[k + 1][j], c); m.tri(rings[k][i], rings[k + 1][j], rings[k][j], c)

def rock(m, seed, rows, colors, n=8, depth=.8):
    rng = random.Random(seed); rings = []
    for k, (radius, y) in enumerate(rows):
        rings.append([m.v((radius * rng.uniform(.82, 1.1) * math.cos((i + .2 * k) * math.tau / n), y + rng.uniform(-.03, .03),
                           radius * depth * rng.uniform(.82, 1.1) * math.sin((i + .2 * k) * math.tau / n))) for i in range(n)])
    top = m.v((rng.uniform(-.05, .05), rows[-1][1] + .05, 0)); bottom = m.v((0, rows[0][1] - .04, 0))
    for i in range(n):
        j = (i + 1) % n; m.tri(bottom, rings[0][i], rings[0][j], colors(0, i)); m.tri(top, rings[-1][j], rings[-1][i], colors(9, i))
        for k in range(len(rings) - 1):
            c = colors(k, i); m.tri(rings[k][i], rings[k + 1][i], rings[k + 1][j], c); m.tri(rings[k][i], rings[k + 1][j], rings[k][j], c)

def facet_box(m, centre, size, axes, color):
    cx, cy, cz = centre; ex, ey, ez = (s / 2 for s in size); ax, ay, az = axes
    def p(x, y, z): return (cx + ax[0] * x + ay[0] * y + az[0] * z, cy + ax[1] * x + ay[1] * y + az[1] * z, cz + ax[2] * x + ay[2] * y + az[2] * z)
    c = [p(x, y, z) for x in (-ex, ex) for y in (-ey, ey) for z in (-ez, ez)]
    for f in [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]:
        v = [m.v(c[i]) for i in f]; m.tri(v[0], v[1], v[2], color); m.tri(v[0], v[2], v[3], color)

# ================================================================== region 7: Whispering Marsh
R7 = [(38, 36, 34), (224, 220, 206), (196, 192, 178), (210, 184, 134), (88, 58, 38), (112, 76, 48), (138, 96, 62), (62, 42, 28),
      (40, 24, 46), (62, 34, 66), (88, 48, 88), (74, 112, 48), (98, 138, 60), (58, 86, 40), (150, 110, 70), (176, 140, 96)]

def marks(m, rng, a, c, radius, count):
    """Dark birch lenticels: short patches running AROUND the branch (as on the tier-two camp logs)."""
    axis = base.unit(base.sub(c, a)); u = base.unit(base.cross(axis, (0, 1, 0) if abs(axis[1]) < .9 else (1, 0, 0))); w = base.cross(axis, u)
    for _ in range(count):
        t = rng.uniform(.08, .92); ang = rng.uniform(0, math.tau); r = radius * 1.04
        centre = base.add(base.add(a, base.mul(base.sub(c, a), t)), base.add(base.mul(u, r * math.cos(ang)), base.mul(w, r * math.sin(ang))))
        normal = base.unit(base.add(base.mul(u, math.cos(ang)), base.mul(w, math.sin(ang)))); tangent = base.cross(axis, normal)
        half, h = radius * rng.uniform(.6, 1.0), .007
        ids = [m.v(base.add(centre, base.add(base.mul(tangent, sx * half), base.mul(axis, sy * h)))) for sx, sy in ((-1, -1), (1, -1), (1, 1), (-1, 1))]
        m.tri(ids[0], ids[1], ids[2], 0); m.tri(ids[0], ids[2], ids[3], 0); m.tri(ids[0], ids[2], ids[1], 0); m.tri(ids[0], ids[3], ids[2], 0)

def birch_branch():
    """White birch bark (slot 2 with its white partner 1 on every third side) with dark lenticels around it; small leaves."""
    m = base.Model('BirchBranch'); rng = random.Random(700)
    for a, c, ra, rb, sides in [((-.49, -.05, -.06), (.02, .0, -.03), .062, .05, 7), ((.02, .0, -.03), (.49, .05, -.08), .05, .036, 7),
                                ((.0, .0, -.03), (.25, .2, .2), .036, .022, 6), ((-.22, -.03, -.05), (-.36, .14, .05), .028, .016, 6)]:
        m.cylinder(a, c, ra, rb, sides, 2, 3); marks(m, rng, a, c, (ra + rb) / 2, int(18 * math.dist(a, c)) + 2)
    for a, b2 in [((.25, .2, .2), (.33, .33, .22)), ((-.36, .14, .05), (-.44, .27, .07)), ((.49, .05, -.08), (.57, .16, -.06))]:
        leaf(m, a, b2, .05, 11, 13)
    save(m, 1200)

def beaver_pelt():
    """A stretched oval beaver plew (the traditional round-stretched pelt): dense dark-brown fur, paler leather rim."""
    m = base.Model('BeaverPelt'); rng = random.Random(701); n = 18
    centre = m.v((0, .09, 0)); inner, outer, lower = [], [], []
    for i in range(n):
        t = i * math.tau / n; rx, rz = .47, .40
        x, z = rx * math.cos(t) * (1 + .04 * math.sin(t * 5)), rz * math.sin(t) * (1 + .04 * math.cos(t * 4))
        inner.append(m.v((x * .55, .085 + .015 * math.cos(t * 3), z * .55)))
        outer.append(m.v((x, .03 + .02 * math.sin(t * 2), z))); lower.append(m.v((x * .99, .0, z * .99)))
    bottom = m.v((0, .005, 0))
    for i in range(n):
        j = (i + 1) % n
        m.tri(centre, inner[j], inner[i], 5 if rng.random() < .7 else 4)
        m.tri(inner[i], inner[j], outer[j], 6 if i % 3 else 5); m.tri(inner[i], outer[j], outer[i], 5)
        m.tri(outer[i], outer[j], lower[j], 15); m.tri(outer[i], lower[j], lower[i], 14); m.tri(bottom, lower[i], lower[j], 14)
    # Stretching-hoop lacing marks on the leather rim.
    for i in range(0, n, 3):
        t = i * math.tau / n; x, z = .47 * math.cos(t), .40 * math.sin(t)
        sphere(m, (x * .97, .03, z * .97), (.022, .012, .022), 7, 5, 3)
    save(m, 900)

def blackberries():
    """Three blackberries of glossy drupelets on a thorny cane with a toothed leaf (not smooth red berries)."""
    m = base.Model('Blackberries'); rng = random.Random(702)
    m.cylinder((-.3, .36, .02), (.26, .44, .02), .016, .01, 5, 14, 14)
    for bx, by in [(-.18, .08), (.06, .14), (.24, .02)]:
        m.cylinder((bx, .39, .02), (bx, by + .14, .02), .007, .005, 3, 13, 13)
        for j in range(14):
            phi = math.acos(1 - 2 * (j + .5) / 14); th = j * 2.39996
            sphere(m, (bx + .1 * math.sin(phi) * math.cos(th), by + .12 * math.cos(phi), .02 + .09 * math.sin(phi) * math.sin(th)),
                   (.045, .045, .042), 8 + j % 3, 6, 3)
    leaf(m, (-.04, .4, .02), (-.3, .6, .02), .09, 11, 13); leaf(m, (.1, .42, .02), (.3, .62, .02), .08, 12, 13)
    save(m, 800)

# ================================================================== region 8: Amber Riverbanks
R8 = [(208, 196, 158), (186, 172, 132), (226, 216, 182), (164, 150, 112), (122, 98, 66), (96, 74, 48),
      (68, 120, 98), (92, 150, 118), (184, 110, 64), (210, 136, 82), (120, 112, 104), (96, 90, 84),
      (226, 120, 38), (240, 146, 58), (200, 96, 30), (86, 140, 54)]

def flax_fiber():
    """Long silver-gold retted flax strands, tied in the middle with a twisted band (hemp is a loose rough hank)."""
    m = base.Model('FlaxFiber'); rng = random.Random(801)
    for i in range(14):
        z = (i % 7 - 3) * .03; y = (i // 7) * .045 - .02
        a = (-.48 + rng.uniform(-.02, .03), y + rng.uniform(-.02, .02), z); mid = (0, y + .01, z * .55)
        b2 = (.48 + rng.uniform(-.03, .02), y + rng.uniform(-.03, .03), z); col = [0, 1, 2, 3][i % 4]
        m.cylinder(a, mid, .011, .014, 4, col, col); m.cylinder(mid, b2, .014, .009, 4, col, col)
    m.cylinder((-.035, .01, 0), (.035, .01, 0), .1, .1, 8, 5, 4)    # the twisted tie around the strands
    save(m, 1000)

def copper_ore():
    """Grey-brown host rock with green copper bloom and bright native-copper nuggets."""
    m = base.Model('CopperOre')
    rock(m, 802, [(.26, -.24), (.46, -.04), (.34, .18)], lambda k, i: [10, 11, 6, 10, 11][(i + k) % 5] if k < 9 else (7 if i % 2 else 10))
    for x, y, z in [(.16, .04, .36), (-.2, -.04, .37), (.04, .22, .1), (-.16, .2, -.04), (.3, -.06, .24)]:
        for zz in (z, -z): sphere(m, (x, y, zz), (.075, .06, .06), 8 + int(x > 0), 6, 3)
    save(m, 480)

def carrots():
    """Three orange carrots with ridged roots and feathery green tops."""
    m = base.Model('Carrots')
    for k, (x, tilt) in enumerate([(-.16, -.12), (.02, 0), (.18, .14)]):
        rows = [(.004, -.46), (.03, -.36), (.06, -.18), (.08, .0), (.085, .12), (.06, .17)]
        start = len(m.vertices)
        lathe(m, rows, lambda r, i: [14, 12, 13][(i + r) % 3], n=8)
        m.vertices[start:] = [(px + x + py * tilt * .4, py, pz + k * .03) for px, py, pz in m.vertices[start:]]
        top = (x + .17 * tilt * .4, .17, k * .03)
        for j in range(3):
            ang = (j - 1) * .45 + tilt
            m.cylinder(top, (top[0] + math.sin(ang) * .2, .17 + math.cos(ang) * .3, top[2]), .012, .006, 4, 15, 15)
            leaf(m, (top[0] + math.sin(ang) * .12, .17 + math.cos(ang) * .18, top[2]), (top[0] + math.sin(ang) * .26, .17 + math.cos(ang) * .36, top[2]), .05, 15, 15)
    save(m, 800)

# ================================================================== region 9: Bluewater Basin
R9 = [(198, 172, 108), (176, 150, 92), (220, 196, 130), (150, 126, 76), (232, 236, 240), (206, 214, 224), (178, 190, 204), (150, 160, 176),
      (44, 56, 112), (62, 78, 138), (86, 104, 160), (120, 132, 176), (74, 112, 52), (98, 138, 62), (120, 86, 60), (60, 84, 40)]

def barley():
    """A nodding barley ear: plump grains in rows with long awns sweeping up (wheat's ear is compact and upright)."""
    m = base.Model('Barley')
    m.cylinder((-.05, -.46, .01), (.02, .18, .01), .013, .009, 4, 1, 3)
    m.cylinder((.02, .18, .01), (.16, .34, .01), .009, .007, 4, 1, 3)
    for row in range(7):
        t = row / 6; cx, cy = .02 + .14 * t, .18 + .16 * t
        for side in (-1, 1):
            gx, gy = cx + side * .04, cy - .02
            sphere(m, (gx, gy, .01), (.035, .055, .03), 0 if (row + side) % 2 else 2, 6, 3)
            m.cylinder((gx, gy + .04, .01), (gx + side * .02 + .02, gy + .45 - row * .025, .01), .003, .001, 3, 3, 3)
    leaf(m, (-.03, -.2, .01), (-.27, .02, .01), .03, 12, 15); leaf(m, (-.02, -.05, .01), (.22, .1, .01), .026, 12, 15)
    save(m, 900)

def quartz():
    """A cluster of six-sided clear-white quartz points on a little matrix (not a round pebble)."""
    m = base.Model('Quartz')
    sphere(m, (0, -.26, 0), (.36, .1, .24), 14, 8, 3)
    for k, (x, z, h, r, lean) in enumerate([(0, 0, .58, .11, 0), (-.18, .05, .42, .09, -.3), (.18, -.02, .46, .09, .28), (.06, .12, .32, .07, .12), (-.08, -.1, .3, .06, -.15)]):
        axis = base.unit((math.sin(lean), math.cos(lean), 0)); u = base.unit(base.cross(axis, (0, 0, 1))); w = base.cross(axis, u)
        start = (x, -.24, z); end = base.add(start, base.mul(axis, h)); tip = base.add(end, base.mul(axis, r * 1.4))
        ring0 = [m.v(base.add(start, base.add(base.mul(u, r * math.cos(i * math.tau / 6)), base.mul(w, r * math.sin(i * math.tau / 6))))) for i in range(6)]
        ring1 = [m.v(base.add(end, base.add(base.mul(u, r * math.cos(i * math.tau / 6)), base.mul(w, r * math.sin(i * math.tau / 6))))) for i in range(6)]
        t = m.v(tip)
        for i in range(6):
            j = (i + 1) % 6; c = 4 + (i + k) % 3
            m.tri(ring0[i], ring1[i], ring1[j], c); m.tri(ring0[i], ring1[j], ring0[j], c); m.tri(ring1[i], t, ring1[j], 4 if i % 2 else 5)
    save(m, 480)

def blueberries():
    """Small dusty-blue berries, each with a five-pointed crown, on a woody twig with oval leaves."""
    m = base.Model('Blueberries')
    m.cylinder((-.34, .3, .02), (.3, .36, .02), .013, .009, 5, 14, 14)
    for i, (x, y, z) in enumerate([(-.24, .1, 0), (-.06, .14, .03), (.12, .1, 0), (.28, .14, .02), (-.14, -.08, .02), (.05, -.08, 0), (.22, -.06, .03)]):
        m.cylinder((x, .32, .02), (x, y + .08, z), .005, .004, 3, 14, 14)
        sphere(m, (x, y, z), (.085, .08, .08), 8 + i % 3, 10, 6)
        for j in range(5):
            a = j * math.tau / 5
            m.cylinder((x, y - .07, z), (x + .025 * math.cos(a), y - .095, z + .025 * math.sin(a)), .006, .002, 3, 11, 11)
    leaf(m, (-.1, .34, .02), (-.34, .52, .02), .07, 12, 15); leaf(m, (.16, .35, .02), (.36, .5, .02), .06, 12, 15)
    save(m, 700)

# ================================================================== region 10: Sunlit Uplands
R10 = [(196, 152, 104), (214, 172, 122), (176, 132, 88), (156, 110, 72), (228, 196, 148), (138, 96, 62),
       (58, 46, 38), (82, 66, 52), (104, 86, 66), (40, 32, 28), (126, 84, 46), (150, 104, 58), (174, 126, 72), (98, 66, 38), (110, 140, 60), (88, 112, 48)]

def sandstone():
    """A warm layered sandstone block: horizontal strata in alternating tones, worn rounded edges."""
    m = base.Model('Sandstone')
    lathe(m, [(.32, -.26), (.44, -.18), (.46, -.06), (.45, .06), (.42, .16), (.32, .24)],
          lambda r, i: [2, 0, 1, 4, 0, 3][r % 6] if i % 4 else [3, 2, 0, 1, 2, 5][r % 6], n=9, squash=.7, wobble=.05, seed=1001)
    save(m, 480)

def truffle():
    """A knobbly black truffle: a lumpy round body covered in small pyramidal warts, with a cut showing marbled flesh."""
    m = base.Model('Truffle'); rng = random.Random(1002)
    sphere(m, (0, 0, 0), (.36, .32, .34), 6, 12, 7)
    for j in range(40):
        phi = math.acos(1 - 2 * (j + .5) / 40); th = j * 2.39996
        p = (.36 * math.sin(phi) * math.cos(th), .32 * math.cos(phi), .34 * math.sin(phi) * math.sin(th))
        if p[2] < -.2 and abs(p[0]) < .14 and abs(p[1]) < .12: continue
        sphere(m, base.mul(p, .96), (.05, .045, .05), (7, 9, 6)[j % 3], 5, 3)
    # Cut face with pale marbling, toward the viewer.
    centre = m.v((0, 0, -.345)); ring = [m.v((.13 * math.cos(a), .11 * math.sin(a), -.335)) for a in [i * math.tau / 10 for i in range(10)]]
    for i in range(10): m.tri(centre, ring[(i + 1) % 10], ring[i], 4 if i % 3 else 8); m.tri(centre, ring[i], ring[(i + 1) % 10], 4 if i % 3 else 8)
    save(m, 480)

def flax_seeds():
    """A small heap of glossy brown flax seeds: a low mound studded with flat teardrop seeds (sand is a fine dune)."""
    m = base.Model('FlaxSeeds'); rng = random.Random(1003)
    rows = [(.46, -.12), (.36, -.02), (.2, .08), (.06, .13), (0, .14)]
    lathe(m, rows[:4], lambda r, i: 13 if i % 2 else 10, n=14, squash=.75, seed=1004)
    def height(r):
        for (r0, y0), (r1, y1) in zip(rows, rows[1:]):
            if r1 <= r <= r0: return y0 + (y1 - y0) * (r0 - r) / (r0 - r1)
        return rows[0][1]
    for j in range(70):
        a = rng.uniform(0, math.tau); r = .44 * math.sqrt(rng.random()); h = height(r) - .005
        x, z = r * math.cos(a), r * .75 * math.sin(a); yaw = rng.uniform(0, math.pi)
        facet_box(m, (x, h + .01, z), (.07, .022, .04), ((math.cos(yaw), 0, math.sin(yaw)), (0, 1, 0), (-math.sin(yaw), 0, math.cos(yaw))), 11 + j % 3)
    save(m, 700)

# ================================================================== region 11: Mirror Lakes
R11 = [(214, 196, 140), (192, 172, 116), (232, 218, 170), (168, 148, 96), (190, 40, 36), (214, 58, 46), (236, 86, 62), (160, 30, 30),
       (70, 120, 46), (92, 146, 56), (226, 158, 40), (244, 186, 64), (196, 128, 30), (250, 214, 110), (120, 92, 52), (150, 116, 66)]

def rice():
    """A drooping rice panicle: a curved stem with many small husked grains on fine branches (barley stands with awns)."""
    m = base.Model('Rice')
    pts = [(-.08, -.46, .01), (-.04, -.05, .01), (.06, .22, .01), (.22, .32, .01), (.36, .22, .01)]
    for a, b2 in zip(pts, pts[1:]): m.cylinder(a, b2, .011, .009, 4, 3, 3)
    for k in range(14):
        t = k / 13; seg = min(3, int(t * 3.99)); u = t * 4 - seg
        a = pts[1 + min(seg, 2)]; b2 = pts[2 + min(seg, 2)] if seg < 3 else pts[4]
        p = base.add(a, base.mul(base.sub(b2, a), u if seg < 3 else 1)); side = 1 if k % 2 else -1
        tip = (p[0] + side * .06, p[1] - .12 - .06 * t, .01 + side * .02)
        m.cylinder(p, tip, .004, .003, 3, 3, 3)
        for g in range(3):
            q = base.add(p, base.mul(base.sub(tip, p), .45 + .27 * g)); sphere(m, base.add(q, (side * .018, 0, 0)), (.018, .03, .015), g % 3, 5, 3)
    leaf(m, (-.06, -.25, .01), (-.3, .05, .01), .03, 8, 9); leaf(m, (-.05, -.1, .01), (.2, .1, .01), .025, 8, 9)
    save(m, 900)

def tomatoes():
    """Two ripe ribbed tomatoes with green five-pointed calyxes and stubs of vine."""
    m = base.Model('Tomatoes')
    for k, (x, s) in enumerate([(-.2, 1.0), (.22, .85)]):
        start = len(m.vertices)
        lathe(m, [(.08, -.3), (.24, -.25), (.33, -.12), (.35, .0), (.32, .12), (.22, .2), (.08, .22)],
              lambda r, i: [4, 5, 6][(i // 2 + r) % 3] if i % 4 else 7, n=16, wobble=.03, seed=1101 + k)
        m.vertices[start:] = [(px * s + x, py * s, pz * s) for px, py, pz in m.vertices[start:]]
        top = (x, .22 * s, 0)
        for j in range(5):
            a = j * math.tau / 5; leaf(m, top, (x + .16 * s * math.cos(a), .2 * s, .16 * s * math.sin(a)), .035, 8, 9)
        m.cylinder(top, (x + .02, .32 * s, .0), .018, .014, 5, 8, 9)
    save(m, 800)

def honey():
    """A slab of wild honeycomb with cells on BOTH faces (as real comb): hexagonal wax cells, some capped pale, some
    open with golden honey, and a drip."""
    m = base.Model('Honey')
    facet_box(m, (0, 0, 0), (.8, .5, .08), ((1, 0, 0), (0, 1, 0), (0, 0, 1)), 12)
    r = .06; dx = r * math.sqrt(3)
    for face in (1, -1):
      for row in range(5):
        for col in range(8):
            cx = -.36 + col * dx + (dx / 2 if row % 2 else 0); cy = -.2 + row * r * 1.5
            if abs(cx) > .38: continue
            capped = (row * 3 + col + (face < 0)) % 4 == 0
            ring = [m.v((cx + r * .9 * math.cos(a + math.pi / 6), cy + r * .9 * math.sin(a + math.pi / 6), face * .045)) for a in [i * math.tau / 6 for i in range(6)]]
            centre = m.v((cx, cy, face * (.015 if not capped else .053)))
            for i in range(6):   # each cell is a shallow hexagonal cone (wax rim to a recessed or domed centre), two-sided
                j = (i + 1) % 6; c = 13 if capped else (10 if i % 2 else 11)
                m.tri(centre, ring[i], ring[j], c); m.tri(centre, ring[j], ring[i], c)
    sphere(m, (.12, -.27, .0), (.04, .07, .035), 11, 6, 4)
    save(m, 800)

# ================================================================== region 12: Frostveil Taiga
R12 = [(120, 122, 124), (146, 148, 150), (96, 98, 100), (172, 174, 176), (52, 50, 52), (196, 200, 206), (224, 228, 232),
       (190, 150, 56), (214, 176, 72), (168, 128, 44), (232, 200, 104), (120, 96, 62), (150, 140, 96), (176, 166, 120), (102, 72, 46), (126, 92, 58)]

def tin_ore():
    """Pale grey granite host with dark lustrous cassiterite crystals and silver-white tin glints."""
    m = base.Model('TinOre')
    rock(m, 1201, [(.24, -.24), (.46, -.05), (.33, .18)], lambda k, i: [0, 1, 3, 0, 2][(i + k) % 5] if k < 9 else (3 if i % 2 else 1))
    for x, y, z, s in [(.16, .02, .36, .08), (-.18, -.06, .37, .07), (.02, .22, .08, .07), (-.2, .2, -.06, .06), (.3, -.04, .22, .06)]:
        for zz in (z, -z):
            facet_box(m, (x, y, zz), (s, s * 1.4, s), ((.8, .2, 0), (-.2, .8, .3), (0, -.3, .9)), 4)
            sphere(m, (x + .03, y + .05, zz + .02 * (1 if zz > 0 else -1)), (.022, .022, .022), 6, 5, 3)
    save(m, 480)

def yellow_ochre():
    """Crumbly lumps of yellow ochre earth, soft rounded clods with powdery facets (pigment, not stone)."""
    m = base.Model('YellowOchre'); rng = random.Random(1202)
    for x, y, z, s in [(-.16, -.08, 0, .26), (.18, -.12, .04, .2), (.02, .1, -.04, .18), (-.3, -.16, .12, .12), (.3, -.18, -.1, .1)]:
        start = len(m.vertices)
        lathe(m, [(s * .6, -s * .7), (s, -s * .2), (s * .95, s * .3), (s * .5, s * .7)], lambda r, i: [7, 8, 9, 10][(i + r) % 4], n=8, wobble=.12, seed=rng.randrange(999))
        m.vertices[start:] = [(px + x, py + y, pz + z) for px, py, pz in m.vertices[start:]]
    save(m, 480)

def cattail_fiber():
    """A tied bundle of split cattail leaf strips with a few fluffy seed heads (raw fibre, not the standing plant)."""
    m = base.Model('CattailFiber'); rng = random.Random(1203)
    for i in range(12):
        z = (i % 6 - 2.5) * .03; y = (i // 6) * .05 - .02
        a = (-.46 + rng.uniform(-.03, .03), y, z); b2 = (.44 + rng.uniform(-.04, .04), y + rng.uniform(-.04, .04), z)
        m.cylinder(a, (0, y, z * .6), .016, .018, 4, 12, 12); m.cylinder((0, y, z * .6), b2, .018, .006, 4, 13 if i % 2 else 12, 12)
    for x, y in [(-.3, .085), (.2, .09)]:
        m.cylinder((x - .09, y, .03), (x + .09, y + .01, .03), .042, .042, 7, 15, 15)   # brown seed head
        sphere(m, (x + .1, y + .015, .03), (.05, .055, .05), 13, 6, 3)                # pale fluff splitting from one end
    m.cylinder((-.035, .01, 0), (.035, .01, 0), .09, .09, 8, 12, 11)
    save(m, 1000)

REGIONS = {7: (R7, [birch_branch, beaver_pelt, blackberries]), 8: (R8, [flax_fiber, copper_ore, carrots]),
           9: (R9, [barley, quartz, blueberries]), 10: (R10, [sandstone, truffle, flax_seeds]),
           11: (R11, [rice, tomatoes, honey]), 12: (R12, [tin_ore, yellow_ochre, cattail_fiber])}

if __name__ == '__main__':
    for region, (colors, makers) in REGIONS.items():
        out = ROOT/f'Assets/_Game/Art/Items/Region{region:02d}'; out.mkdir(parents=True, exist_ok=True)
        base.OUT = out; base.COLORS = colors; start = len(STATS)
        for make in makers: make()
        base.palette(f'Region{region:02d}Palette.png')
        (out/'ResourceManifest.json').write_text(json.dumps(dict(Region=region, Models=STATS[start:]), indent=2) + '\n', encoding='utf-8')
    for s in STATS: print(f"{s['Name']:14} {s['Triangles']:5} tris  footprint {s['ProposedFootprint']}")
