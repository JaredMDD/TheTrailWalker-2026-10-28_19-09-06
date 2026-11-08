"""Tier-two camp: a BURNING birch-wood campfire (user request, November 6, 2026). Python stdlib only.

Tier two's wood is birch (PROJECT_RECOMENDATIONS.md section 6.2). Its camps seat traders on the existing BirchLogSeat
(CampProps catalog, regions 7-12) around this fire. Unlike the tier-one BurnedWoodCampfire (charred wood, no flames),
this fire is lit: a teepee of birch logs whose white bark (with dark birch marks) chars toward the middle and glows at
the tips, a bed of ash and embers, a ring of river stones, two split birch logs waiting as fuel, and layered faceted
flames. Two meshes:
  BirchCampfireWood    logs, embers, ash bed, stones and spare logs (lit palette material, surface detail)
  BirchCampfireFlames  flame tongues only (unlit, no shadows, animated by CampfireFlicker)
Coordinates: metres, +Y up, ground at y 0, fire centre at the origin; about 1.4 m across, flames about 0.9 m tall.
Run: python Tools/Art/generate_tier02_camp_meshes.py, then The Trail Walker > Art > Build Tier 2 Camp.
"""
from pathlib import Path
import json, math, random
import generate_region01_meshes as b
from generate_region05_meshes import sphere

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Environment/CampProps/Tier02'
# The cylinder helper shades every third side one slot lower, so each wood band sits one slot above its darker partner:
# 1 char (0 darker), 6 birch white (5 = its pale shade), 11 ember glow (10 ember orange). Slot 7 is the dark birch mark,
# drawn as short patches around the log (birch lenticels run around the trunk, not along it).
COLORS = [(28, 27, 25), (49, 47, 42), (94, 91, 81), (140, 136, 126), (170, 138, 92),
          (192, 188, 175), (222, 218, 205), (38, 36, 34), (206, 178, 128),
          (150, 40, 20), (214, 90, 30), (240, 150, 50),
          (235, 110, 30), (245, 160, 45), (250, 210, 90), (255, 240, 170),
          (98, 100, 98), (128, 130, 126), (160, 160, 152), (88, 104, 64)]
STATS = []

def save(m):
    m.save()
    STATS.append(dict(Name=m.name, Triangles=len(m.faces), BoundsMin=[round(min(v[i] for v in m.vertices), 4) for i in range(3)],
                      BoundsMax=[round(max(v[i] for v in m.vertices), 4) for i in range(3)]))

def marks(m, rng, a, c, radius, t0, t1, count):
    """Dark birch lenticels: short patches running around the log, set just above the bark."""
    axis = b.unit(b.sub(c, a)); u = b.unit(b.cross(axis, (0, 1, 0) if abs(axis[1]) < .9 else (1, 0, 0))); v = b.cross(axis, u)
    for _ in range(count):
        t = rng.uniform(t0, t1); ang = rng.uniform(0, math.tau); r = radius * 1.03
        centre = b.add(b.add(a, b.mul(b.sub(c, a), t)), b.add(b.mul(u, r * math.cos(ang)), b.mul(v, r * math.sin(ang))))
        normal = b.unit(b.add(b.mul(u, math.cos(ang)), b.mul(v, math.sin(ang)))); tangent = b.cross(axis, normal)
        w, h = radius * rng.uniform(.5, .9), .008
        corners = [b.add(centre, b.add(b.mul(tangent, sx * w), b.mul(axis, sy * h))) for sx, sy in ((-1, -1), (1, -1), (1, 1), (-1, 1))]
        ids = [m.v(p) for p in corners]
        m.tri(ids[0], ids[1], ids[2], 7); m.tri(ids[0], ids[2], ids[3], 7); m.tri(ids[0], ids[2], ids[1], 7); m.tri(ids[0], ids[3], ids[2], 7)

def log(m, a, c, radius, burn_from=.45, rng=None):
    """Birch log from a (outer, unburnt end) to c (inner, burning end): white bark, then char, then a glowing tip."""
    bands = [(0, burn_from, 6, 8), (burn_from, .88, 1, 0), (.88, 1, 11, 11)]
    for t0, t1, bark, end in bands:
        p0 = b.add(a, b.mul(b.sub(c, a), t0)); p1 = b.add(a, b.mul(b.sub(c, a), t1))
        m.cylinder(p0, p1, radius * (1 - .12 * t0), radius * (1 - .12 * t1), 8, bark, end)
    if rng is not None: marks(m, rng, a, c, radius, .05, burn_from - .05, 6)

def stone(m, rng, centre, size):
    sphere(m, (centre[0], size * .45, centre[1]), (size * rng.uniform(.9, 1.2), size * .5, size * rng.uniform(.8, 1.05)), 16 + rng.randrange(3), 7, 3)

def wood():
    m = b.Model('BirchCampfireWood'); rng = random.Random(207)
    # Ash and ember bed (a low faceted mound, not a flat overlay).
    sphere(m, (0, .01, 0), (.5, .045, .48), 2, 12, 3)
    for i in range(9):
        ang = i * math.tau / 9 + rng.uniform(-.2, .2); r = rng.uniform(.08, .32)
        sphere(m, (r * math.cos(ang), .035, r * math.sin(ang)), (.07, .03, .06), 9 + i % 3, 6, 3)
    # Teepee of five birch logs leaning to the centre.
    for i in range(5):
        ang = i * math.tau / 5 + .3
        outer = (.5 * math.cos(ang), .04, .5 * math.sin(ang)); inner = (.07 * math.cos(ang + .5), .5, .07 * math.sin(ang + .5))
        log(m, outer, inner, .055 + .01 * (i % 2), rng=rng)
    # Two cross logs low in the fire, burning in the middle.
    log(m, (-.42, .08, -.1), (0, .1, 0), .06, burn_from=.35, rng=rng); log(m, (.4, .08, .14), (0, .1, 0), .06, burn_from=.35, rng=rng)
    # Ring of river stones.
    for i in range(10):
        ang = i * math.tau / 10 + rng.uniform(-.08, .08); r = .66
        stone(m, rng, (r * math.cos(ang), r * math.sin(ang)), rng.uniform(.08, .11))
    # Two split birch logs set aside as fuel (white bark, pale split face up).
    for z, yaw in ((.95, .25), (1.05, -.1)):
        a = (-.35, .07, z); c = (.35, .07, z + math.sin(yaw) * .3)
        m.cylinder(a, c, .07, .065, 8, 6, 8); marks(m, rng, a, c, .07, .08, .92, 9)
    save(m)

def tongue(m, base, height, radius, colour, rng, sway):
    rings = []; steps = 5
    for j in range(steps + 1):
        t = j / steps; r = radius * (1 - t) ** .55 * (1 + .45 * math.sin(t * math.pi)); y = base[1] + height * t
        cx = base[0] + sway[0] * t * t; cz = base[2] + sway[1] * t * t; n = 6; twist = t * 1.4
        rings.append([m.v((cx + r * math.cos(i * math.tau / n + twist), y, cz + r * math.sin(i * math.tau / n + twist))) for i in range(n)])
    tip = m.v((base[0] + sway[0] * 1.1, base[1] + height * 1.05, base[2] + sway[1] * 1.1))
    for j in range(steps):
        for i in range(6):
            k = (i + 1) % 6
            m.tri(rings[j][i], rings[j + 1][i], rings[j + 1][k], colour); m.tri(rings[j][i], rings[j + 1][k], rings[j][k], colour)
    for i in range(6): m.tri(rings[-1][i], tip, rings[-1][(i + 1) % 6], colour)

def flames():
    m = b.Model('BirchCampfireFlames'); rng = random.Random(208)
    # Outer orange tongues, amber middle, yellow inner and a pale core: opaque layered shells (no transparency).
    # Rounded, swaying tongues rather than spikes: a wide orange base, amber and yellow above, a pale hot core.
    for i in range(5):
        ang = i * math.tau / 5; r = .12
        tongue(m, (r * math.cos(ang), .08, r * math.sin(ang)), rng.uniform(.38, .55), .15, 12, rng, (math.cos(ang) * .08 + rng.uniform(-.05, .05), math.sin(ang) * .08 + rng.uniform(-.05, .05)))
    for i in range(3):
        ang = i * math.tau / 3 + .6; r = .06
        tongue(m, (r * math.cos(ang), .1, r * math.sin(ang)), rng.uniform(.55, .7), .12, 13, rng, (rng.uniform(-.08, .08), rng.uniform(-.08, .08)))
    tongue(m, (0, .12, 0), .78, .11, 14, rng, (.05, -.03))
    tongue(m, (0, .13, 0), .42, .08, 15, rng, (-.02, .02))
    save(m)

if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True); b.OUT = OUT; b.COLORS = COLORS
    wood(); flames()
    b.palette('Tier02CampPalette.png')
    (OUT/'MeshManifest.json').write_text(json.dumps(dict(Models=STATS, Wood='Birch', WoodTier=2, FirstRegion=7, LastRegion=12,
        Policy='Decorative art only. Flames are opaque faceted shells drawn unlit without shadows and animated by a tiny flicker component; no particles, lights or colliders. Seats are the existing BirchLogSeat.'), indent=2) + '\n', encoding='utf-8')
    print('Tier-two camp meshes:', ', '.join(f"{s['Name']} {s['Triangles']}" for s in STATS))
