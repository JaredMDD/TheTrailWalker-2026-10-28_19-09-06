"""Massive lumber-field trees (Claude, November 8, 2026; PROJECT_RECOMENDATIONS.md section 13.4, "Lumber field trees").

Four woods (pine, birch, mesquite, oak) x three trunk shapes, so the two rows of six in each tier section do not
look cloned. Every tree is two parts on the existing chopping rig (Tier1PineRig):
  <Name>Lower.obj  the stump, 0..cut height: buttress roots, moss, knots, a sawn top with bark rim and annual rings.
                   It stays standing after the fall and is the 72-hour regrowth stump.
  <Name>Upper.obj  everything above the cut, in the FallPivot frame (y = 0 at the cut): bole, limbs, crown and a
                   matching sawn underside.
Faceted palette-strip style, one 32-colour palette (MassiveTreesPalette.png) whose detail types are in
detail_textures.PALETTES (bark, wood, needles, moss). Python standard library only; deterministic.

Run: python Tools/Art/generate_massive_trees.py   then Unity: The Trail Walker > Art > Build Massive Trees.
"""
from pathlib import Path
import json, math, random
import generate_region01_meshes as base

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Harvest/Massive'
PALETTE = 'MassiveTreesPalette.png'
# Slot contract (bark colours follow the camp log seats of each wood, CampPropsPalette).
COLORS = [
    (66, 39, 25), (112, 66, 39), (146, 90, 54), (206, 164, 104), (166, 113, 63),       # 0-4 pine: furrow, bark, plate, cut, heartwood
    (40, 38, 35), (224, 220, 206), (190, 186, 171), (226, 202, 152), (84, 80, 73),      # 5-9 birch: lenticel, white, shade, cut, old dark base
    (58, 37, 28), (96, 59, 40), (130, 86, 57), (214, 180, 122), (148, 70, 40),          # 10-14 mesquite: furrow, bark, ridge, sapwood, red heart
    (58, 51, 41), (98, 86, 67), (132, 118, 92), (202, 162, 102), (150, 104, 58),        # 15-19 oak: furrow, bark, ridge, cut, heartwood
    (36, 62, 42), (52, 84, 50), (76, 108, 60),                                          # 20-22 pine needles: shade, mid, light
    (104, 144, 60), (146, 176, 78),                                                     # 23-24 birch leaves
    (98, 124, 66), (138, 158, 94),                                                      # 25-26 mesquite leaflets
    (48, 82, 38), (72, 110, 48),                                                        # 27-28 oak leaves
    (78, 96, 52), (104, 92, 72), (138, 130, 116),                                       # 29 moss, 30 root soil, 31 dead silver wood
]
SPECIES = {
    #            furrow mid light cut heart  leaves (shade, mid, light)  tier
    'Pine':     dict(bark=(0, 1, 2), cut=3, heart=4, leaves=(20, 21, 22), tier=1),
    'Birch':    dict(bark=(7, 6, 6), cut=8, heart=8, leaves=(23, 23, 24), tier=2),
    'Mesquite': dict(bark=(10, 11, 12), cut=13, heart=14, leaves=(25, 25, 26), tier=3),
    'Oak':      dict(bark=(15, 16, 17), cut=18, heart=19, leaves=(27, 27, 28), tier=4),
}
MOSS, SOIL, DEAD, LENTICEL, BIRCH_BASE = 29, 30, 31, 5, 9
MAX_TRIANGLES = 3200
STATS = []


def tri_out(m, a, b, c, color, inside):
    """Triangle whose right-hand normal points away from `inside` (this pipeline's outward convention)."""
    p, q, r = (m.vertices[i - 1] for i in (a, b, c))
    n = base.cross(base.sub(q, p), base.sub(r, p))
    cen = tuple((p[i] + q[i] + r[i]) / 3 for i in range(3))
    if sum(n[i] * (cen[i] - inside[i]) for i in range(3)) < 0: b, c = c, b
    m.tri(a, b, c, color)


def lerp(a, b, t): return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))


def frame(axis):
    axis = base.unit(axis)
    u = base.unit(base.cross(axis, (0, 0, 1) if abs(axis[2]) < .9 else (1, 0, 0)))
    return u, base.cross(axis, u)


def tube(m, path, radii, sides, color, cap_end=None, rng=None, wobble=0.0, squash=1.0):
    """Tapered tube along a polyline (limbs, roots). color(i, j) -> slot; optional end cap slot."""
    rings = []
    for j, (p, r) in enumerate(zip(path, radii)):
        d = base.sub(path[min(j + 1, len(path) - 1)], path[max(j - 1, 0)])
        u, v = frame(d)
        ring = []
        for i in range(sides):
            a = i * math.tau / sides + j * .35
            rr = r * (1 + (rng.uniform(-wobble, wobble) if rng else 0))
            off = base.add(base.mul(u, rr * math.cos(a)), base.mul(v, rr * math.sin(a)))
            if squash != 1.0: off = (off[0], off[1] * squash, off[2])
            ring.append(m.v(base.add(p, off)))
        rings.append(ring)
    for j in range(len(rings) - 1):
        mid = lerp(path[j], path[j + 1], .5)
        for i in range(sides):
            k = (i + 1) % sides; c = color(i, j)
            tri_out(m, rings[j][i], rings[j + 1][k], rings[j][k], c, mid)
            tri_out(m, rings[j][i], rings[j + 1][i], rings[j + 1][k], c, mid)
    if cap_end is not None:
        e = path[-1]; d = base.unit(base.sub(path[-1], path[-2])); tip = m.v(base.add(e, base.mul(d, radii[-1] * .6)))
        for i in range(sides): tri_out(m, rings[-1][i], rings[-1][(i + 1) % sides], tip, cap_end, path[-2])


def clump(m, rng, centre, radii, slots, n=7, rings=4, flat_bottom=.65):
    """Faceted foliage mass: jittered ellipsoid, under-side faces in the shade slot."""
    layers = []
    for j in range(1, rings):
        phi = j * math.pi / rings; layer = []
        for i in range(n):
            a = i * math.tau / n + rng.uniform(-.2, .2) + j * .4
            s = rng.uniform(.82, 1.12); ry = radii[1] * (flat_bottom if math.cos(phi) < 0 else 1)
            layer.append(m.v((centre[0] + radii[0] * s * math.sin(phi) * math.cos(a), centre[1] + ry * math.cos(phi),
                              centre[2] + radii[2] * s * math.sin(phi) * math.sin(a))))
        layers.append(layer)
    top = m.v(base.add(centre, (rng.uniform(-.1, .1) * radii[0], radii[1] * rng.uniform(.9, 1.1), 0)))
    bottom = m.v(base.add(centre, (0, -radii[1] * flat_bottom, 0)))

    def slot(ids):
        y = sum(m.vertices[i - 1][1] for i in ids) / 3 - centre[1]
        if y < -radii[1] * .15: return slots[0]
        return slots[2] if y > radii[1] * .45 and rng.random() < .7 else slots[1]
    for i in range(n):
        k = (i + 1) % n
        tri_out(m, top, layers[0][k], layers[0][i], slot((top, layers[0][k], layers[0][i])), centre)
        tri_out(m, bottom, layers[-1][i], layers[-1][k], slots[0], centre)
        for j in range(len(layers) - 1):
            tri_out(m, layers[j][i], layers[j][k], layers[j + 1][k], slot((layers[j][i], layers[j][k], layers[j + 1][k])), centre)
            tri_out(m, layers[j][i], layers[j + 1][k], layers[j + 1][i], slot((layers[j][i], layers[j + 1][k], layers[j + 1][i])), centre)


class Tree:
    """One massive tree: a bole profile (height, radius) with ridge depth, plus species-specific dressing."""

    def __init__(self, species, variant, seed, cut, profile, sides=18, ridge=.09, lean=(0, 0), twist=0.0):
        self.species, self.variant, self.spec = species, variant, SPECIES[species]
        self.name = f'Massive{species}{variant}'
        self.rng = random.Random(seed); self.cut = cut; self.profile = profile; self.sides = sides
        self.ridge = ridge; self.lean = lean; self.twist = twist
        self.wob = [1 + .07 * math.cos(i * 2.3 + seed) + .05 * math.sin(i * 5.1 + seed * .3) for i in range(sides)]
        self.lower = base.Model(self.name + 'Lower'); self.upper = base.Model(self.name + 'Upper')
        self.top_y = profile[-1][0]; self.extent = 0.0; self.crown_top = 0.0

    # The bole leans only above the cut, so the stump top is level and the FallPivot sits on the trunk axis.
    def centre(self, y):
        t = max(0.0, y - self.cut) / max(1e-6, self.top_y - self.cut)
        return (self.lean[0] * t * t, y, self.lean[1] * t * t)

    def radius(self, y):
        p = self.profile
        for (y0, r0), (y1, r1) in zip(p, p[1:]):
            if y0 <= y <= y1: return r0 + (r1 - r0) * (y - y0) / (y1 - y0)
        return p[-1][1]

    def ring_points(self, y, scale=1.0, flat=False):
        c = self.centre(y); r = self.radius(y) * scale; pts = []
        for i in range(self.sides):
            a = i * math.tau / self.sides + self.twist * y
            rr = r * self.wob[i] * (1 if flat else (1 - self.ridge * (i % 2)))
            pts.append((c[0] + rr * math.cos(a), y, c[2] + rr * math.sin(a)))
        return pts

    def bark_color(self, i, y, j):
        dark, mid, light = self.spec['bark']
        if self.species == 'Birch':
            if y < 1.1 or (y < 2.6 and self.rng.random() < (2.6 - y) / 1.5 * .8):   # old birches turn dark and fissured at the base
                return BIRCH_BASE if (i + j) % 3 else LENTICEL
            return mid if i % 5 else light
        if self.species == 'Oak' and i % 2 == 0: return light if self.rng.random() < .3 else mid   # blocky oak plates, irregular
        return dark if i % 2 else (light if (i // 2) % 4 == 1 else mid)

    def bole(self, m, heights, offset_y):
        rings = [[m.v((p[0], p[1] - offset_y, p[2])) for p in self.ring_points(y)] for y in heights]
        for j in range(len(rings) - 1):
            y0, y1 = heights[j], heights[j + 1]; mid = self.centre((y0 + y1) / 2); mid = (mid[0], mid[1] - offset_y, mid[2])
            band = self.species == 'Birch' and y0 >= 2.0 and j % 2 == 1      # thin bands carry the lenticel dashes
            for i in range(self.sides):
                k = (i + 1) % self.sides; c = self.bark_color(i, y0, j)
                if band and self.rng.random() < .45: c = LENTICEL
                if m is self.lower and y0 < self.cut * .5 and self.rng.random() < .3 and math.sin(i * math.tau / self.sides) < -.5: c = MOSS
                tri_out(m, rings[j][i], rings[j + 1][k], rings[j][k], c, mid)
                tri_out(m, rings[j][i], rings[j + 1][i], rings[j + 1][k], c, mid)
        return rings

    def sawn_face(self, m, y, offset_y, up):
        """Bark rim, sapwood, an annual ring line and heartwood as non-overlapping annuli (no z-fighting)."""
        c = self.centre(y); c = (c[0], y - offset_y, c[2])
        inside = (c[0], c[1] - (1 if up else -1), c[2])
        scales = [(1.0, None), (.9, self.spec['bark'][0]), (.72, self.spec['cut']), (.66, self.spec['heart']), (.38, self.spec['cut']), (.0, self.spec['heart'])]
        prev = [m.v((p[0], p[1] - offset_y, p[2])) for p in self.ring_points(y)]
        for s, color in scales[1:]:
            if s == 0:
                centre = m.v(c)
                for i in range(self.sides): tri_out(m, prev[i], prev[(i + 1) % self.sides], centre, color, inside)
                break
            ring = [m.v((p[0], p[1] - offset_y, p[2])) for p in self.ring_points(y, s, flat=True)]
            for i in range(self.sides):
                k = (i + 1) % self.sides
                tri_out(m, prev[i], prev[k], ring[k], color, inside); tri_out(m, prev[i], ring[k], ring[i], color, inside)
            prev = ring

    def roots(self, count, reach, height, slots=None):
        m = self.lower; r0 = self.radius(0)
        for n in range(count):
            a = n * math.tau / count + self.rng.uniform(-.25, .25); ca, sa = math.cos(a), math.sin(a)
            length = r0 * reach * self.rng.uniform(.8, 1.15); rad = r0 * .4
            h = min(height * self.rng.uniform(.75, 1.1), self.cut - rad * .6 - .08)
            bend = self.rng.uniform(-.25, .25); path = []; radii = []
            for t in (0, .25, .5, .75, 1.0):
                ang = a + bend * t; d = r0 * .55 + length * t
                path.append((d * math.cos(ang), h * (1 - t) ** 2.2 - .1 * t, d * math.sin(ang)))
                radii.append(rad * (1 - t * .7))
            bark = self.spec['bark']; back = sa < -.3
            tube(m, path, radii, 7, lambda i, j: MOSS if (back and j < 2 and i in (1, 2) and self.rng.random() < .5) else (BIRCH_BASE if self.species == 'Birch' else bark[i % 3]),
                 rng=self.rng, wobble=.06, squash=.6)
            self.extent = max(self.extent, r0 * .7 + length)

    def knots(self, m, ys, offset_y):
        for y in ys:
            a = self.rng.uniform(0, math.tau); c = self.centre(y); r = self.radius(y) * .92
            p0 = (c[0] + r * math.cos(a), y - offset_y, c[2] + r * math.sin(a))
            p1 = (c[0] + (r + .09) * math.cos(a), y - offset_y + .02, c[2] + (r + .09) * math.sin(a))
            tube(m, [p0, p1], [.14, .08], 6, lambda i, j: self.spec['bark'][0], cap_end=self.spec['heart'])

    def limb(self, start_y, angle, rise, length, r0, segments=3, droop=0.0, color=None, broken=False):
        """A limb in the upper frame from the bole surface outward, curving up (rise) or drooping."""
        m = self.upper; c = self.centre(start_y); rb = self.radius(start_y) * .55
        ca, sa = math.cos(angle), math.sin(angle); path = []; radii = []
        for s in range(segments + 1):
            t = s / segments; d = rb + length * t
            y = start_y + rise * t - droop * t * t + self.rng.uniform(-.1, .1) * t
            path.append((c[0] + d * ca + self.rng.uniform(-.15, .15) * t, y - self.cut, c[2] + d * sa + self.rng.uniform(-.15, .15) * t))
            radii.append(r0 * (1 - .72 * t))
        bark = self.spec['bark']
        tube(m, path, radii, 6, color or (lambda i, j: bark[i % 3] if self.species != 'Birch' else (LENTICEL if (i + j) % 5 == 0 else 6)),
             cap_end=DEAD if broken else None, rng=self.rng, wobble=.05)
        tip = path[-1]; self.extent = max(self.extent, math.hypot(tip[0], tip[2]))
        return (tip[0], tip[1] + self.cut, tip[2])

    def foliage(self, centre, radii, n=7, rings=4, flat=.65):
        c = (centre[0], centre[1] - self.cut, centre[2])
        clump(self.upper, self.rng, c, radii, self.spec['leaves'], n, rings, flat)
        self.extent = max(self.extent, math.hypot(centre[0], centre[2]) + radii[0])
        self.crown_top = max(self.crown_top, centre[1] + radii[1])

    def build_bole(self, lower_rings, upper_heights, knots_lower=(), knots_upper=()):
        self.bole(self.lower, [0.0] + list(lower_rings) + [self.cut], 0.0)
        self.sawn_face(self.lower, self.cut, 0.0, True)
        bottom_cap_needed = self.radius(0) > 0
        self.bole(self.upper, [self.cut] + list(upper_heights), self.cut)
        self.sawn_face(self.upper, self.cut, self.cut, False)
        # Close the bole top (hidden in the crown) with a dark cap.
        ring = self.ring_points(upper_heights[-1]); c = self.centre(upper_heights[-1])
        ids = [self.upper.v((p[0], p[1] - self.cut, p[2])) for p in ring]; top = self.upper.v((c[0], c[1] - self.cut + .05, c[2]))
        for i in range(self.sides): tri_out(self.upper, ids[i], ids[(i + 1) % self.sides], top, self.spec['bark'][0], (c[0], c[1] - self.cut - 1, c[2]))
        self.knots(self.lower, knots_lower, 0.0); self.knots(self.upper, knots_upper, self.cut)
        self.crown_top = max(self.crown_top, upper_heights[-1])

    def save(self):
        rows = []
        for m in (self.lower, self.upper):
            assert all(all(math.isfinite(c) for c in p) for p in m.vertices), m.name
            assert all(len({a, b, c}) == 3 for a, b, c, _ in m.faces), m.name
            m.save(); rows.append(len(m.faces))
        total = sum(rows); assert total <= MAX_TRIANGLES, f'{self.name}: {total} triangles'
        lo = min(min(p[1] for p in self.lower.vertices), 0.0)
        assert lo > -.3, f'{self.name}: roots sink {lo:.2f} m'
        r = self.radius(self.cut)
        STATS.append(dict(Name=self.name, Species=self.species, Variant=self.variant, WoodTier=self.spec['tier'],
                          LowerMesh=self.lower.name, UpperMesh=self.upper.name, LowerTriangles=rows[0], UpperTriangles=rows[1],
                          CutHeight=round(self.cut, 3), CutRadius=round(r, 3), BaseRadius=round(self.radius(0), 3),
                          Height=round(self.crown_top, 2), CrownRadius=round(self.extent, 2),
                          CutArea=[0, round(self.cut + .12, 3), round(r * .98, 3)], AxeHint=[0, round(self.cut + 1.9, 3), round(r + .25, 3)],
                          Selection=dict(Centre=[0, 1.7, 0], Size=[round(2 * r + .5, 2), 3.4, round(2 * r + .5, 2)])))
        print(f'  {self.name}: {total} triangles, {self.crown_top:.1f} m tall, crown radius {self.extent:.1f} m')


def heights(a, b, n): return [a + (b - a) * i / n for i in range(1, n + 1)]


# ------------------------------------------------------------------ species

def pine(variant, seed):
    """Old pine: a tall clean bole of red-brown plates, a high crown of flat needle pads on near-level limbs."""
    tall = {'A': 13.0, 'B': 10.8, 'C': 14.2}[variant]
    t = Tree('Pine', variant, seed, cut=1.05, profile=[(0, 1.05), (.35, .86), (1.0, .74), (3, .66), (7, .52), (tall, .30)],
             sides=18, ridge=.11, lean={'A': (.25, .1), 'B': (-.15, .3), 'C': (.6, -.2)}[variant])
    top = tall if variant != 'B' else 8.6
    t.roots(6, 1.25, .75)
    t.build_bole(heights(0, 1.05, 2)[:-1], heights(1.05, top, 8), knots_lower=(.55,), knots_upper=(2.6, 4.1))
    rng = t.rng; start = top * .5
    whorls = 6 if variant != 'B' else 4
    for w in range(whorls):
        y = start + (top - start - .6) * w / whorls; n = 3 if w < whorls - 1 else 2
        for k in range(n):
            ang = k * math.tau / n + w * 1.1 + rng.uniform(-.3, .3); length = (2.6 - 1.6 * w / whorls) * rng.uniform(.85, 1.15)
            tip = t.limb(y, ang, rng.uniform(.2, .9) + .3 * w / whorls, length, .2 - .1 * w / whorls, segments=3)
            size = .8 + .35 * (1 - w / whorls); c0 = t.centre(y)
            for p in range(3 if w < whorls - 2 else 2):
                f = 1 - .32 * p; a2 = ang + rng.uniform(-.5, .5) * p
                pos = (c0[0] + (tip[0] - c0[0]) * f + .4 * math.cos(a2) * p, tip[1] + .2 + rng.uniform(-.25, .3), c0[2] + (tip[2] - c0[2]) * f + .4 * math.sin(a2) * p)
                t.foliage(pos, (size * rng.uniform(.8, 1.15), .42 * rng.uniform(.85, 1.3), size * rng.uniform(.7, 1.0)), n=7, rings=3, flat=.6)
    if variant == 'B':
        # Broken crown: the leader snapped; a silver dead spike and a dead limb, crown pads lower and fewer.
        c = t.centre(top)
        tube(t.upper, [(c[0], top - t.cut, c[2]), (c[0] + .1, top - t.cut + 1.6, c[2] + .05), (c[0] + .25, top - t.cut + 2.3, c[2] - .1)],
             [.34, .2, .04], 7, lambda i, j: DEAD if j else t.spec['bark'][i % 2], rng=rng, wobble=.12)
        t.limb(top - 1.0, 2.4, -.3, 2.0, .14, segments=2, color=lambda i, j: DEAD, broken=True)
        t.crown_top = max(t.crown_top, top + 2.3)
    else:
        c = t.centre(top)
        t.foliage((c[0], top + .55, c[2]), (1.0, .8, .95), n=8, rings=4, flat=.6)
    t.save()


def birch(variant, seed):
    """Old birch: dark fissured base, white bark with lenticel dashes, forked leaders, airy drooping crown."""
    t = Tree('Birch', variant, seed, cut=1.0, profile=[(0, .92), (.35, .76), (1.0, .64), (3.5, .54), (5.2, .48)],
             sides=16, ridge=.07, lean={'A': (.3, 0), 'B': (-.5, .35), 'C': (.15, -.4)}[variant])
    t.roots(5, 1.1, .6)
    hs = []
    y = 1.0
    while y < 5.2 - 1e-6:
        hs += [y + .5, y + .58]; y += .58
    hs = [h for h in hs if h <= 5.2] + [5.2]
    t.build_bole([.5], hs, knots_lower=(.45,), knots_upper=(2.4,))
    rng = t.rng; leaders = {'A': 3, 'B': 2, 'C': 3}[variant]
    for k in range(leaders):
        ang = k * math.tau / leaders + rng.uniform(-.3, .3)
        tip = t.limb(5.0, ang, 4.6 + rng.uniform(-.6, .6), 1.7, .32, segments=3)
        for s in range(3):
            a2 = ang + rng.uniform(-1.2, 1.2); y2 = 6.2 + s * 1.2 + rng.uniform(-.3, .3)
            base_pt = lerp((t.centre(5)[0], 5.0, t.centre(5)[2]), tip, (y2 - 5) / (tip[1] - 5))
            ln = rng.uniform(1.2, 2.0); ca, sa = math.cos(a2), math.sin(a2)
            path = [(base_pt[0], base_pt[1] - t.cut, base_pt[2]), (base_pt[0] + ca * ln * .5, base_pt[1] + .35 - t.cut, base_pt[2] + sa * ln * .5),
                    (base_pt[0] + ca * ln, base_pt[1] - .25 - t.cut, base_pt[2] + sa * ln)]
            tube(t.upper, path, [.1, .07, .03], 5, lambda i, j: 6 if (i + j) % 4 else LENTICEL)
            t.foliage((path[-1][0], path[-1][1] + t.cut - .1, path[-1][2]), (1.0, .85, .95), n=7, rings=4, flat=.8)
        t.foliage((tip[0], tip[1] + .4, tip[2]), (1.2, 1.1, 1.1), n=7, rings=4)
    c = t.centre(5.2)
    for k in range(4):
        a = k * math.tau / 4 + .5
        t.foliage((c[0] + 1.1 * math.cos(a), 8.0 + rng.uniform(-.4, .6), c[2] + 1.1 * math.sin(a)), (1.2, 1.0, 1.1), n=7, rings=4)
    t.save()


def mesquite(variant, seed):
    """Old mesquite: a short, twisted, dark trunk that splits low into sweeping limbs under a wide, flat, light crown."""
    t = Tree('Mesquite', variant, seed, cut=.85, profile=[(0, 1.0), (.3, .84), (.85, .72), (1.9, .68), (2.3, .74)],
             sides=16, ridge=.13, lean={'A': (.4, .2), 'B': (-.3, -.5), 'C': (.7, .0)}[variant], twist=.55)
    t.roots(6, 1.3, .55)
    t.build_bole([.45], heights(.85, 2.3, 4), knots_lower=(.4,), knots_upper=(1.5,))
    rng = t.rng; limbs = {'A': 4, 'B': 3, 'C': 5}[variant]; c = t.centre(2.3)
    for k in range(limbs):
        ang = k * math.tau / limbs + rng.uniform(-.35, .35); length = rng.uniform(3.2, 4.4)
        tip = t.limb(2.1, ang, rng.uniform(2.6, 3.6), length, .42, segments=4)
        for s in range(2):
            a2 = ang + (s - .5) * 1.1 + rng.uniform(-.2, .2); ln = rng.uniform(1.4, 2.2)
            start = (tip[0] * .7 + c[0] * .3, tip[1] - .5, tip[2] * .7 + c[2] * .3)
            end = (start[0] + ln * math.cos(a2), start[1] + .7, start[2] + ln * math.sin(a2))
            tube(t.upper, [(start[0], start[1] - t.cut, start[2]), lerp((start[0], start[1] - t.cut, start[2]), (end[0], end[1] - t.cut, end[2]), .5), (end[0], end[1] - t.cut, end[2])],
                 [.16, .1, .04], 5, lambda i, j: t.spec['bark'][i % 3], rng=rng, wobble=.1)
            t.foliage((end[0], end[1] + .2, end[2]), (1.5, .5, 1.3), n=8, rings=3, flat=.45)
        t.foliage((tip[0], tip[1] + .3, tip[2]), (1.8, .6, 1.6), n=8, rings=3, flat=.45)
    t.foliage((c[0], 6.0, c[2]), (2.0, .6, 1.8), n=8, rings=3, flat=.45)
    t.save()


def oak(variant, seed):
    """Ancient oak: a huge grey-brown bole of blocky ridges, massive limbs spreading low and wide, a deep dome crown."""
    t = Tree('Oak', variant, seed, cut=1.15, profile=[(0, 1.4), (.4, 1.18), (1.15, 1.06), (2.6, 1.0), (3.4, 1.06)],
             sides=20, ridge=.1, lean={'A': (.2, .1), 'B': (-.35, .25), 'C': (.1, -.45)}[variant], twist=.08)
    t.roots(7, 1.15, .9)
    t.build_bole([.55], heights(1.15, 3.4, 5), knots_lower=(.7,), knots_upper=(2.1, 2.8))
    rng = t.rng; limbs = {'A': 5, 'B': 4, 'C': 6}[variant]; c = t.centre(3.4)
    for k in range(limbs):
        ang = k * math.tau / limbs + rng.uniform(-.3, .3); length = rng.uniform(3.6, 5.0)
        broken = variant == 'B' and k == 1
        tip = t.limb(3.0, ang, rng.uniform(2.0, 3.4), length * (.55 if broken else 1), .5, segments=4, broken=broken)
        if broken: continue
        for s in range(2):
            a2 = ang + (s - .5) * .9; d = rng.uniform(1.2, 2.0)
            t.foliage((tip[0] + d * math.cos(a2) * .6, tip[1] + .9 + s * .5, tip[2] + d * math.sin(a2) * .6), (2.0, 1.4, 1.9), n=8, rings=4, flat=.6)
        t.foliage((tip[0] * 1.08, tip[1] - .2, tip[2] * 1.08), (1.7, 1.1, 1.6), n=8, rings=4, flat=.55)
    for k in range(4):
        a = k * math.tau / 4 + .8
        t.foliage((c[0] + 2.0 * math.cos(a), 8.3 + rng.uniform(-.4, .4), c[2] + 2.0 * math.sin(a)), (2.2, 1.5, 2.1), n=8, rings=4)
    t.foliage((c[0], 9.6, c[2]), (2.2, 1.4, 2.1), n=8, rings=4)
    t.save()


if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True); base.OUT = OUT; base.COLORS = COLORS
    for n, v in enumerate('ABC'):
        pine(v, 101 + n); birch(v, 201 + n); mesquite(v, 301 + n); oak(v, 401 + n)
    base.palette(PALETTE)
    (OUT/'MassiveTreesManifest.json').write_text(json.dumps(dict(Palette=PALETTE, Trees=STATS), indent=2) + '\n', encoding='utf-8')
    print(len(STATS), 'massive trees')
