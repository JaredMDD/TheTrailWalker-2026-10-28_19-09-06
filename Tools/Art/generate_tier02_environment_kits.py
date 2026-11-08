"""Six reusable tier-two environment kits (regions 7-12), user request November 6, 2026. Stdlib only.

Same contract as the tier-one kits (generate_tier01_environment_kits.py): 20 decorative models per region, one shared
32-colour palette per region with the same slot semantics (bark 0-3, grass 4-7, bushes 8-10, leaves 12-14, stone
16-18, ground 19-23, water 24-26, leaves 27-28, sand/snow 29-31), flat faceted normals, opaque geometry, manifest with
category/footprint/clearance/shadow/distant/terrain flags. No route, scatter, camp, reward or collider changes.

Tier two's wood is birch and its animal the beaver (PROJECT_RECOMENDATIONS.md section 6.2), so every decorative tree is
a birch variant, as every tier-one tree is a pine. Region themes and landmark names follow section 6.2.6:
  7 Whispering Marsh    wetlands and boardwalks          (landmark Marsh)
  8 Amber Riverbanks    warm river terraces              (landmark Riverbank)
  9 Bluewater Basin     lowland basin and ponds          (landmark Basin)
 10 Sunlit Uplands      sunny uplands, wooded hollows    (landmark Knoll / Upland)
 11 Mirror Lakes        linked lakes and islands         (landmark Lakeshore)
 12 Frostveil Taiga     snowy taiga, thawed river edges  (landmark Taiga)
Decorative pieces never imitate the region's loose pickups (no berry bushes, quartz crystals, honeycombs or flax).

Run: python Tools/Art/generate_tier02_environment_kits.py
Then The Trail Walker > Art > Rebuild All Tier 2 Region Environment Kits, and Capture All Tier 2 Region Environment Kits.
"""
from pathlib import Path
import json, math, random
import generate_region01_meshes as b
from generate_region05_meshes import sphere
import generate_tier01_environment_kits as t1
from generate_tier01_environment_kits import quad, tone, leaf, bush, grasses, fern, reeds, rock, cluster, landform, patch, deadwood

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Environment/Tier02'
REGIONS = {7: 'Whispering Marsh', 8: 'Amber Riverbanks', 9: 'Bluewater Basin', 10: 'Sunlit Uplands', 11: 'Mirror Lakes', 12: 'Frostveil Taiga'}

# ------------------------------------------------------------------ palettes (tier-one slot contract)
BIRCH = {0: (38, 36, 34), 1: (222, 218, 205), 2: (192, 188, 175), 3: (206, 178, 128),       # dark marks, white bark, shade, cut wood
         12: (86, 126, 52), 13: (114, 150, 62), 14: (146, 174, 82)}                          # birch leaves
PALETTE_CHANGES = {
    7: {4: (94, 110, 60), 5: (110, 126, 68), 6: (126, 138, 78), 7: (148, 144, 88), 8: (58, 82, 50), 9: (72, 98, 56), 10: (92, 116, 64),
        16: (96, 102, 92), 17: (120, 126, 112), 18: (146, 150, 132), 19: (70, 58, 42), 20: (88, 72, 50), 21: (108, 90, 62),
        24: (60, 82, 70), 25: (72, 98, 84), 26: (92, 118, 100)},
    8: {4: (156, 132, 70), 5: (176, 150, 82), 6: (196, 168, 96), 7: (214, 186, 112), 8: (106, 118, 56), 9: (128, 136, 64), 10: (150, 150, 74),
        12: (150, 140, 60), 13: (182, 160, 70), 14: (206, 172, 74), 16: (168, 132, 96), 17: (190, 152, 112), 18: (210, 176, 134),
        19: (128, 92, 58), 20: (150, 110, 70), 21: (170, 128, 82), 24: (70, 110, 118), 25: (86, 130, 136), 26: (112, 152, 150)},
    9: {4: (78, 112, 58), 5: (92, 128, 64), 6: (110, 146, 72), 7: (150, 150, 90), 16: (100, 110, 120), 17: (128, 138, 146), 18: (156, 166, 172),
        24: (52, 96, 130), 25: (66, 116, 150), 26: (96, 142, 170)},
    10: {4: (128, 146, 70), 5: (150, 164, 80), 6: (172, 180, 92), 7: (196, 186, 110), 12: (110, 150, 58), 13: (140, 172, 66), 14: (170, 192, 84),
         16: (176, 140, 98), 17: (198, 162, 116), 18: (220, 188, 140), 19: (122, 98, 64), 20: (144, 116, 76), 21: (166, 136, 90)},
    11: {4: (84, 120, 70), 5: (98, 136, 78), 6: (118, 152, 90), 7: (150, 158, 104), 16: (112, 120, 124), 17: (140, 148, 150), 18: (170, 176, 176),
         24: (96, 140, 160), 25: (120, 162, 180), 26: (150, 186, 198), 29: (196, 184, 150), 30: (212, 200, 166), 31: (226, 216, 184)},
    12: {4: (120, 128, 108), 5: (140, 146, 124), 6: (162, 166, 144), 7: (176, 176, 156), 8: (62, 78, 60), 9: (78, 94, 72), 10: (98, 112, 88),
         12: (176, 156, 70), 13: (198, 176, 86), 14: (214, 196, 110), 16: (120, 124, 128), 17: (146, 150, 154), 18: (172, 176, 180),
         19: (92, 80, 66), 20: (110, 96, 78), 24: (88, 120, 136), 25: (110, 142, 156), 26: (150, 180, 192),
         29: (210, 218, 226), 30: (226, 232, 236), 31: (240, 244, 248)},
}
PALETTES = {}
for region in REGIONS:
    colors = list(t1.BASE)
    for i, value in {**BIRCH, **PALETTE_CHANGES[region]}.items(): colors[i] = value
    PALETTES[region] = colors

# ------------------------------------------------------------------ new shapes

def recolor(m, start, mapping):
    """Remaps face colours from face index start (for example grass slots -> snow slots)."""
    m.faces[start:] = [(a, b2, c, mapping.get(k, k)) for a, b2, c, k in m.faces[start:]]

def box(m, centre, size, k, yaw=0.0):
    cx, cy, cz = centre; sx, sy, sz = (s / 2 for s in size); ca, sa = math.cos(yaw), math.sin(yaw)
    def p(x, y, z): return (cx + x * ca - z * sa, cy + y, cz + x * sa + z * ca)
    c = [p(x, y, z) for x in (-sx, sx) for y in (-sy, sy) for z in (-sz, sz)]
    for f in [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]:
        quad(m, [c[i] for i in f], k)

def birch(m, rng, height=6, width=1, lean=0, snow=False):
    """White trunk; the cylinder helper darkens every third side (slot 0), which reads as birch lenticel marks."""
    segments = 4; base = (0, 0, 0)
    for s in range(segments):
        t0, t1_ = s / segments, (s + 1) / segments
        a = (lean * t0 * .7, height * .72 * t0, 0); c = (lean * t1_ * .7, height * .72 * t1_, 0)
        m.cylinder(a, c, height * .028 * (1 - t0 * .55), height * .028 * (1 - t1_ * .55), 7, 1, 3)
    for i in range(3):
        y = height * (.45 + i * .1); ang = i * 2.1 + rng.uniform(-.3, .3)
        m.cylinder((lean * .7 * y / height, y, 0), (lean * .7 * y / height + .55 * math.cos(ang), y + .45, .55 * math.sin(ang)), .045, .015, 5, 2, 2)
    # Dark horizontal lenticel marks, the birch signature, set into the white bark.
    for i in range(9):
        y = height * rng.uniform(.08, .66); ang = rng.uniform(0, math.tau); r = height * .028 * (1 - y / (height * .72) * .55)
        cx = lean * .7 * y / (height * .72) * .72
        box(m, (cx + math.cos(ang) * r * .82, y, math.sin(ang) * r * .82), (r * 1.15, height * .014, r * .55), 0, -ang)
    top = height * .72
    for i in range(6):
        ang = i * math.tau / 6 + rng.uniform(-.25, .25); r = width * height * rng.uniform(.06, .12)
        cx = lean * .7 + r * math.cos(ang); cz = r * math.sin(ang); cy = top + height * rng.uniform(-.06, .12)
        sphere(m, (cx, cy, cz), (width * height * .11, height * .11, width * height * .1), 12 + i % 3, 8, 4)
        if snow: sphere(m, (cx, cy + height * .08, cz), (width * height * .08, height * .025, width * height * .07), 30, 7, 3)
    sphere(m, (lean * .7, top + height * .17, 0), (width * height * .09, height * .1, width * height * .085), 13, 8, 4)
    if snow: sphere(m, (lean * .7, top + height * .26, 0), (width * height * .06, height * .02, width * height * .055), 31, 7, 3)

def grove(m, rng, height=5, count=3):
    for i in range(count):
        start = len(m.vertices); birch(m, rng, height * rng.uniform(.75, 1.05), .85, rng.uniform(-.3, .3))
        dx, dz = math.cos(i * 2.3) * 1.1, math.sin(i * 2.3) * 1.1
        m.vertices[start:] = [(x + dx, y, z + dz) for x, y, z in m.vertices[start:]]

def cattails(m, rng, height=1.6, width=1, bent=False):
    """Reed stems with the brown cattail heads (region 12's Cattail fiber stays a pickup; these are scenery)."""
    for i in range(8):
        x = rng.uniform(-width * .42, width * .42); z = rng.uniform(-width * .38, width * .38); h = height * rng.uniform(.65, 1)
        drift = .3 if bent else .05; tip = (x + drift, h, z)
        m.cylinder((x, 0, z), tip, .013, .008, 4, 6, 6)
        leaf(m, (x, h * .25, z), (x + .28, h * .55, z + .06), .035, 5)
        head = b.add((x, 0, z), b.mul(b.sub(tip, (x, 0, z)), .82))
        m.cylinder(head, b.add(head, (drift * .05, .16, 0)), .03, .03, 6, 20, 20)

def boardwalk(m, rng, length=4, width=1.1):
    """Plank walkway on posts, for marsh edges. A Structure: placed by route authoring, never scattered or on the road."""
    for i in range(int(length / .22)):
        z = -length / 2 + .11 + i * .22
        box(m, (rng.uniform(-.02, .02), .42, z), (width, .05, .2), 3 if i % 3 else 2, rng.uniform(-.03, .03))
    for z in (-length / 2 + .2, 0, length / 2 - .2):
        for x in (-width / 2 + .06, width / 2 - .06):
            m.cylinder((x, -.25, z), (x, .44, z), .055, .05, 6, 2, 3)
    for x in (-width / 2 + .03, width / 2 - .03): box(m, (x, .36, 0), (.06, .06, length), 2)
    m.vertices = [(z, y, -x) for x, y, z in m.vertices]   # runs along X, like a roadside walkway seen from the trail

def beaver_lodge(m, rng, width=2.4, height=1.1):
    sphere(m, (0, height * .35, 0), (width * .5, height * .55, width * .45), 20, 10, 5)
    for i in range(26):
        ang = rng.uniform(0, math.tau); r = width * rng.uniform(.25, .5); y = rng.uniform(.05, height * .8)
        a = (r * math.cos(ang), y, r * .9 * math.sin(ang)); c = (a[0] * .3, min(height * 1.02, y + rng.uniform(.25, .6)), a[2] * .3)
        m.cylinder(a, c, .03, .02, 4, 3 if i % 3 else 2, 3)

def beaver_dam(m, rng, length=4, height=.9):
    """A mounded wall of birch logs and sticks packed with mud, across a stream (the beaver is tier two's animal)."""
    sphere(m, (0, height * .3, 0), (length * .52, height * .55, .55), 20, 12, 4)
    for layer in range(3):
        y = .15 + layer * height * .28; half = length * (.5 - layer * .1)
        for i in range(7 - layer):
            x = -half + (i + .5) * 2 * half / (7 - layer) + rng.uniform(-.1, .1)
            m.cylinder((x - .45, y, rng.uniform(-.3, -.15)), (x + .45, y + rng.uniform(-.05, .05), rng.uniform(.15, .3)), .08, .065, 6, 1, 3)
    for i in range(22):
        x = rng.uniform(-length * .48, length * .48); z = rng.uniform(-.35, .35)
        m.cylinder((x, .1, z), (x + rng.uniform(-.4, .4), height * rng.uniform(.75, 1.15), z * .4), .025, .015, 4, 3, 3)

def terrace(m, rng, height=1.6, width=5, length=5, steps=3):
    """Stepped river terrace: flat treads and short risers on a fixed boundary (a terrain replacement, like banks)."""
    nx, nz = 12, 8; grid = []
    for zi in range(nz + 1):
        row = []
        for xi in range(nx + 1):
            u = xi / nx * 2 - 1; v = zi / nz * 2 - 1; level = min(steps - 1, int((u + 1) / 2 * steps))
            edge = max(0, 1 - v * v); y = height * (level / (steps - 1)) * edge
            row.append((u * width, y, v * length))
        grid.append(row)
    for zi in range(nz):
        for xi in range(nx):
            riser = abs(grid[zi][xi][1] - grid[zi][xi + 1][1]) > .05
            quad(m, [grid[zi][xi], grid[zi + 1][xi], grid[zi + 1][xi + 1], grid[zi][xi + 1]], 19 + tone(xi, zi, 2) if riser else 4 + tone(xi, zi, 2))

def lilies(m, rng, width=2, length=2):
    patch(m, rng, width=width, length=length, kind='water')
    for i in range(14):
        x = rng.uniform(-width * .85, width * .85); z = rng.uniform(-length * .85, length * .85); r = rng.uniform(.16, .3)
        ring = [m.v((x + r * math.cos(a), .012, z + r * math.sin(a))) for a in [j * math.tau / 7 for j in range(7)]]
        centre = m.v((x, .012, z))
        for j in range(6): m.tri(centre, ring[j + 1], ring[j], 13 + i % 2)   # a notch is left open, like a lily pad
        if i % 4 == 0: sphere(m, (x, .05, z), (.05, .035, .05), 22, 6, 3)

def snowcap(m, rng, height=1.2, width=1.2):
    start = len(m.faces); rock(m, rng, height=height, width=width)
    sphere(m, (0, height * .92, 0), (width * .72, height * .16, width * .68), 30, 8, 3)

def snowlog(m, rng, length=2.4, width=.18):
    deadwood(m, rng, length=length, width=width)
    sphere(m, (0, width * 2.05, 0), (length * .46, .06, width * .9), 31, 9, 3)

# ------------------------------------------------------------------ kit lists (20 per region)
E = t1.entry
KITS = {
 7: [E('MarshBirch', 'birch', height=6), E('MarshBirchSapling', 'birch', height=2.4, width=.8), E('LeaningMarshBirch', 'birch', height=5.2, lean=.9),
     E('MarshWillowShrub', 'bush', height=1.3, width=1.6), E('LowBogShrub', 'bush', height=.55, width=1.1), E('CattailClump', 'cattails', height=1.7),
     E('BentCattailClump', 'bentcattails', height=1.4), E('MarshSedge', 'grass', height=1, width=.7), E('BogMossCarpet', 'moss'),
     E('MarshStoneCluster', 'cluster', height=.35, width=1.2), E('BeaverLodge', 'lodge'), E('SunkenBirchLog', 'deadwood', length=2.6, width=.16),
     E('BoardwalkSection', 'boardwalk'), E('MarshMudBank', 'bank', height=.6, width=4, length=4), E('LowMarshIsland', 'mound', height=.4, width=5, length=4),
     E('MarshGroundPatch', 'mud'), E('BogWaterTile', 'water', width=4, length=4), E('LilyPadWaterTile', 'lilies', width=4, length=4),
     E('BoardwalkTrailShoulder', 'shoulder'), E('MarshClearingPatch', 'ground', width=4, length=4)],
 8: [E('RiverbankBirch', 'birch', height=6.4, width=1.1), E('AmberBirch', 'birch', height=5, width=1.25, lean=.35), E('RiverbankBirchSapling', 'birch', height=2.2),
     E('AmberWillowShrub', 'bush', height=1.2, width=2), E('RiverScrub', 'bush', height=.7, width=1.3), E('AmberGrass', 'drygrass', height=.8),
     E('RiverbankFlowers', 'flowers', height=.4), E('TerraceReeds', 'reeds', height=1.4), E('RiverPebbleCluster', 'cluster', height=.3, width=1.4),
     E('SandstoneRiverRock', 'rock', height=1, width=1.4), E('BeaverDam', 'dam'), E('RiverDriftwood', 'deadwood', length=2.2, width=.13),
     E('RiverTerraceStep', 'terrace', height=1.6, width=5, length=5), E('RiverTerraceBank', 'bank', height=1.2, width=4, length=4),
     E('AmberBankCut', 'cliff', height=1.3, width=3, length=5), E('RiverbankGroundPatch', 'ground'), E('RiverSandbar', 'sand'),
     E('RiverWaterTile', 'water', width=4, length=8), E('RiverbankTrailShoulder', 'shoulder'), E('RiverbankClearingPatch', 'ground', width=4, length=4)],
 9: [E('BasinBirch', 'birch', height=6), E('TwinBasinBirch', 'grove', height=5, count=2), E('BasinBirchSapling', 'birch', height=2.5, width=.85),
     E('PondsideShrub', 'bush', height=1.1, width=1.5), E('LowBasinShrub', 'bush', height=.5, width=1.2), E('PondReeds', 'reeds', height=1.5),
     E('PondSedge', 'grass', height=.9, width=.6), E('BasinGrass', 'grass', height=.6), E('BasinWildflowers', 'flowers', height=.35),
     E('BlueStoneCluster', 'cluster', height=.5, width=1.5), E('PondEdgeBoulder', 'rock', height=1, width=1.3), E('BasinRootLog', 'roots', length=2, width=.15),
     E('PondBasin', 'basin', height=.9, width=5, length=5), E('BasinBank', 'bank', height=1, width=4, length=3), E('BasinRise', 'mound', height=1.6, width=6, length=5),
     E('PondWaterTile', 'water', width=4, length=4), E('LilyPondTile', 'lilies', width=4, length=4), E('BasinGroundPatch', 'ground'),
     E('BasinTrailShoulder', 'shoulder'), E('BasinClearingPatch', 'ground', width=4, length=4)],
 10: [E('UplandBirch', 'birch', height=6.5, width=1.05), E('BirchGroveClump', 'grove', height=5.5, count=3), E('SunlitBirchSapling', 'birch', height=2.3),
      E('UplandHeathShrub', 'drybush', height=.6, width=1.3), E('HollowShrub', 'bush', height=1.2, width=1.4), E('UplandGrass', 'drygrass', height=.6),
      E('SunlitWildflowers', 'flowers', height=.35), E('HollowFern', 'fern', height=.7, width=1), E('SandstoneOutcrop', 'rock', height=1.6, width=1.8),
      E('SandstoneLedge', 'rock', height=.7, width=2.6, length=1.3), E('UplandStoneCluster', 'cluster', height=.6, width=1.6),
      E('FallenBirchTrunk', 'deadwood', length=2.8, width=.18), E('WoodedHollow', 'basin', height=1.2, width=5, length=5),
      E('UplandSlope', 'slope', height=2.6, width=5, length=6), E('UplandKnoll', 'mound', height=2.4, width=6, length=5),
      E('DistantUplandRidge', 'ridge', height=6, width=14, length=14), E('UplandGroundPatch', 'ground'), E('HollowLeafLitter', 'leaves'),
      E('UplandTrailShoulder', 'shoulder'), E('UplandClearingPatch', 'ground', width=4, length=4)],
 11: [E('LakeshoreBirch', 'birch', height=6.2), E('IslandBirch', 'birch', height=4.4, width=.9, lean=.6), E('LakeBirchSapling', 'birch', height=2.4),
      E('ShoreShrub', 'bush', height=1, width=1.5), E('LowShoreBush', 'bush', height=.5, width=1.1), E('LakeReeds', 'reeds', height=1.5),
      E('ShoreSedge', 'grass', height=.8, width=.6), E('LakeshoreGrass', 'grass', height=.55), E('ShoreFlowers', 'flowers', height=.3),
      E('ShorePebbleCluster', 'cluster', height=.25, width=1.4), E('LakeBoulder', 'rock', height=1.2, width=1.5), E('LakeDriftwood', 'deadwood', length=2.4, width=.13),
      E('IslandMound', 'mound', height=1.3, width=5, length=4), E('LakeshoreBank', 'bank', height=.8, width=4, length=4),
      E('DistantLakeIsland', 'mound', height=4, width=14, length=10), E('MirrorLakeTile', 'water', width=4, length=8), E('CalmWaterTile', 'water', width=4, length=4),
      E('LakeshoreSandPatch', 'sand'), E('LakeshoreTrailShoulder', 'shoulder'), E('LakeshoreClearingPatch', 'ground', width=4, length=4)],
 12: [E('FrostBirch', 'birch', height=6), E('SnowCappedBirch', 'snowbirch', height=5.4), E('TaigaBirchSapling', 'birch', height=2.2, width=.8),
      E('FrostShrub', 'bush', height=.9, width=1.4), E('LowTaigaShrub', 'bush', height=.45, width=1.1), E('FrostGrass', 'drygrass', height=.55),
      E('ThawSedge', 'grass', height=.8, width=.6), E('ThawCattails', 'cattails', height=1.3), E('SnowCappedBoulder', 'snowrock', height=1.2, width=1.3),
      E('FrostRockCluster', 'cluster', height=.5, width=1.6), E('SnowCoveredLog', 'snowlog'), E('TaigaSnowBank', 'snowbank', height=1, width=4, length=4),
      E('TaigaSnowMound', 'snowmound', height=1.5, width=6, length=5), E('DistantTaigaRidge', 'snowridge', height=7, width=16, length=14),
      E('TaigaSnowPatch', 'snow'), E('ThawedMudPatch', 'mud'), E('ThawWaterTile', 'water', width=4, length=4), E('IceEdgeWaterTile', 'ice', width=4, length=4),
      E('TaigaTrailShoulder', 'shoulder'), E('TaigaClearingPatch', 'ground', width=4, length=4)],
}

SNOW = {4: 29, 5: 30, 6: 31, 7: 30}

def make(spec, seed):
    name, kind, args = spec['Name'], spec['Kind'], spec['Args']
    m = b.Model(name); rng = random.Random(seed)
    if kind in ('birch', 'snowbirch'): birch(m, rng, snow=kind == 'snowbirch', **args)
    elif kind == 'grove': grove(m, rng, **args)
    elif kind in ('cattails', 'bentcattails'): cattails(m, rng, bent=kind == 'bentcattails', **args)
    elif kind == 'boardwalk': boardwalk(m, rng, **args)
    elif kind == 'lodge': beaver_lodge(m, rng, **args)
    elif kind == 'dam': beaver_dam(m, rng, **args)
    elif kind == 'terrace': terrace(m, rng, **args)
    elif kind == 'lilies': lilies(m, rng, **args)
    elif kind == 'snowrock': snowcap(m, rng, **args)
    elif kind == 'snowlog': snowlog(m, rng, **args)
    elif kind in ('snowbank', 'snowmound', 'snowridge'):
        landform(m, rng, form={'snowbank': 'bank', 'snowmound': 'mound', 'snowridge': 'ridge'}[kind], **args); recolor(m, 0, SNOW)
    elif kind == 'snow': patch(m, rng, kind='grass', **args); recolor(m, 0, SNOW)
    elif kind == 'ice': patch(m, rng, kind='water', **args); recolor(m, 0, {24: 26, 25: 26})
    elif kind in ('bush', 'drybush'): bush(m, rng, dry=kind == 'drybush', **args)
    elif kind in ('grass', 'drygrass', 'flowers'): grasses(m, rng, dry=kind == 'drygrass', flowers=kind == 'flowers', **args)
    elif kind == 'fern': fern(m, rng, **args)
    elif kind in ('reeds', 'bentreeds'): reeds(m, rng, bent=kind == 'bentreeds', **args)
    elif kind == 'rock': rock(m, rng, **args)
    elif kind in ('cluster', 'mossrock'): cluster(m, rng, moss=kind == 'mossrock', **args)
    elif kind in ('mound', 'bank', 'slope', 'ridge', 'cliff', 'basin'): landform(m, rng, form=kind, **args)
    elif kind in ('deadwood', 'roots'): deadwood(m, rng, roots=kind == 'roots', **args)
    elif kind == 'moss':
        for i in range(8): sphere(m, ((i % 3 - 1) * .24, .035, (i // 3 - 1) * .24), (.22, .07, .20), 8 + i % 3, 7, 3)
    else: patch(m, rng, kind={'ground': 'grass'}.get(kind, kind), **args)
    m.save(); return m

TERRAIN = ('ground', 'shoulder', 'leaves', 'sand', 'mud', 'snow', 'mound', 'bank', 'slope', 'ridge', 'cliff', 'basin', 'terrace',
           'snowbank', 'snowmound', 'snowridge')
def metadata(name, kind, vertices, triangles, region):
    lo = [min(v[i] for v in vertices) for i in range(3)]; hi = [max(v[i] for v in vertices) for i in range(3)]; height = hi[1] - lo[1]
    category = ('Tree' if kind in ('birch', 'snowbirch', 'grove') else 'Shrub' if kind in ('bush', 'drybush') else
                'Rock' if kind in ('rock', 'cluster', 'mossrock', 'snowrock') else 'Deadwood' if kind in ('deadwood', 'roots', 'snowlog') else
                'Water' if kind in ('water', 'lilies', 'ice') else 'Structure' if kind in ('boardwalk', 'lodge', 'dam') else
                'Terrain' if kind in TERRAIN else 'GroundCover')
    distant = 'Distant' in name
    caster = category in ('Tree', 'Structure') or category == 'Shrub' and height > 1
    radius = math.hypot(hi[0] - lo[0], hi[2] - lo[2]) * .5
    return dict(Id=f'env.tier02.region{region:02d}.' + name, Name=name, Category=category, Triangles=triangles, BoundsMin=lo, BoundsMax=hi,
                FootprintRadius=round(radius, 3), CastsShadows=caster and not distant, DistantOnly=distant,
                RecommendedRoadClearance=round(radius + (8 if category == 'Tree' else 4 if category == 'Structure' else 3), 2),
                TerrainReplacement=category in ('Terrain', 'Water'), ResourcePath=f'Environment/Tier02/Region{region:02d}/' + name)

if __name__ == '__main__':
    rows = []
    for region, specs in KITS.items():
        assert len(specs) == 20, (region, len(specs))
        assert len({s['Name'] for s in specs}) == 20, region
        target = OUT/f'Region{region:02d}'; target.mkdir(parents=True, exist_ok=True); b.OUT = target; b.COLORS = PALETTES[region]
        models = []
        for i, spec in enumerate(specs):
            m = make(spec, region * 100 + i); models.append(metadata(m.name, spec['Kind'], m.vertices, len(m.faces), region))
        b.palette('EnvironmentPalette.png')
        row = dict(Number=region, Name=REGIONS[region], PaletteSize=32, TrailColorRGB=list(PALETTES[region][20]), Models=models)
        (target/'EnvironmentManifest.json').write_text(json.dumps(row, indent=2) + '\n', encoding='utf-8'); rows.append(row)
    names = [m['Name'] for r in rows for m in r['Models']]
    assert len(names) == len(set(names)), 'model names must be unique across tier two'
    (OUT/'Tier02EnvironmentManifest.json').write_text(json.dumps(dict(Revision=1, Regions=rows,
        Policy='Reusable decorative art library for tier two (regions 7-12). Same contract as tier one: readable meshes, one opaque 32-colour palette material per region, separate shadow groups, no camp/route/reward/collider/save/scatter changes. Structure pieces (boardwalk, beaver lodge, beaver dam) are placed by route authoring only, never scattered and never on the road.'), indent=2) + '\n', encoding='utf-8')
    print('TOTAL', len(names), 'models across six tier-two region kits')
