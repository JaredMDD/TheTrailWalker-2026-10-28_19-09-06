"""Settlement route art (regions 3-4 spec, November 8, 2026): the entrance arch where the approach road meets the
settlement, a palisade segment for the wall either side of it and a split-rail fence section for the farmland edge
along the approach. Faceted style on a house-like wood palette. Python 3 standard library only; art only.

Frames (metres, +Y up, +Z = walking direction, origin on the ground at the centre line):
  EntranceArch     two log posts at x +-2.6 (clear opening 4.64 m wide, >= 3.2 m high under the board), crossbeam with a small shingle
                   roof, a hanging board with a carved house icon (no text). Budget <= 900 triangles.
  PalisadeSegment  4 m of pointed stakes along X (x -2..2), two back rails. Budget <= 900 triangles.
  FenceSection     4 m split-rail fence along X (x -2..2): three posts, two rails. Budget <= 900 triangles.
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Settlement/Route'
HEADER = 'The Trail Walker settlement route (faceted); metres; +Y up; +Z walking direction.'
BUDGET = 900

pal = k.Palette()
for name, color, *detail in [
    ('Log', '#6B4A30', 'bark'), ('LogEnd', '#B08A5C', 'wood'), ('Beam', '#7A5638', 'wood'), ('Plank', '#8C6A48', 'planks'),
    ('PlankDark', '#6E523A', 'planks'), ('Shingle', '#5A4A3E', 'planks'), ('ShingleDark', '#463A30', 'planks'),
    ('Carve', '#3E2E20', 'wood'), ('Rope', '#B49A6A', 'thatch'), ('Stone', '#7E7A70', 'stone'), ('Moss', '#5E7340', 'needles'),
    ('Stake', '#6E5038', 'bark'), ('StakeTip', '#9A7A54', 'wood')]:
    pal.add(name, color, *detail)


def box(m, c, size, color, **kw): k.box(m, c, size, color, **kw)


def log(m, a, b, r, color='Log', sides=6):
    k.limb(m, [(a, r, r), (b, r*.94, r*.94)], color, sides=sides, turn=math.pi/sides, forward_hint=(0, 0, 1))


def arch():
    m = k.Mesh('EntranceArch', pal)
    # Thick posts on stone footings; their inner faces stand 2.32 m from the centre line.
    for sx in (-1, 1):
        x = sx*2.6
        log(m, (x, -.3, 0), (x, 4.15, 0), .28, sides=8)
        for i, (dx, dz, s) in enumerate(((-.35, .3, .34), (.38, -.25, .3), (.1, .42, .24), (-.2, -.4, .26))):
            box(m, (x+dx, .06, dz), (s, s*.55, s*.9), 'Moss' if i == 2 else 'Stone', taper=.72)
        # Knee braces kept above 3.35 m so the opening stays clear.
        k.limb(m, [((sx*2.36, 3.38, 0), .07, .07), ((sx*1.75, 3.86, 0), .07, .07)], 'Beam', sides=5, forward_hint=(0, 0, 1))
    # Crossbeam and a short ridge board under the shingles.
    box(m, (0, 4.02, 0), (6.6, .32, .42), 'Beam')
    for sx in (-1, 1): box(m, (sx*3.25, 4.02, 0), (.12, .26, .36), 'LogEnd')
    # Shingle roof: two pitched planes in three courses each, overhanging front and back.
    for sz in (-1, 1):
        for row in range(3):
            z = sz*(.16 + row*.2); y = 4.5 - row*.12
            box(m, (0, y, z), (7.1, .06, .26), 'Shingle' if row % 2 == 0 else 'ShingleDark', axes=((1, 0, 0), (0, 1, sz*.55), (0, -sz*.55, 1)))
    box(m, (0, 4.6, 0), (7.15, .08, .14), 'ShingleDark')
    # Hanging board on two ropes with a carved house icon (walls, door and a pitched roof) on both faces.
    for x in (-.45, .45): box(m, (x, 3.76, 0), (.03, .2, .03), 'Rope')
    box(m, (0, 3.46, 0), (1.25, .46, .07), 'Plank')
    for sz in (-1, 1):
        z = sz*.045
        box(m, (0, 3.40, z), (.42, .24, .02), 'Carve')
        box(m, (0, 3.39, z*1.25), (.12, .16, .02), 'PlankDark')
        for sx in (-1, 1): box(m, (sx*.13, 3.60, z), (.36, .05, .02), 'Carve', axes=((1, -sx*.62, 0), (sx*.62, 1, 0), (0, 0, 1)))
    return m


def palisade():
    m = k.Mesh('PalisadeSegment', pal)
    n = 13
    for i in range(n):
        x = -2 + (i+.5)*4/n
        h = 2.55 + .35*math.sin(i*2.3) + .12*math.cos(i*5.1); r = .15 + .015*math.sin(i*1.7)
        log(m, (x, -.3, 0), (x, h, 0), r, color='Stake')
        k.limb(m, [((x, h, 0), r*.94, r*.94), ((x, h+.38, 0), .02, .02)], 'StakeTip', sides=6, turn=math.pi/6, forward_hint=(0, 0, 1), cap_end=False)
    for y in (.75, 1.85): box(m, (0, y, -.2), (4.05, .14, .1), 'Beam')
    return m


def fence():
    m = k.Mesh('FenceSection', pal)
    for x in (-2, 0, 2): log(m, (x, -.25, 0), (x, 1.15, 0), .08, color='Log')
    for i, y in enumerate((.48, .92)):
        for a, b in ((-2, 0), (0, 2)):
            k.limb(m, [((a, y + .02*i, 0), .055, .045), ((b, y - .015, 0), .05, .045)], 'Beam', sides=5, forward_hint=(0, 1, 0))
    return m


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    pal.write_png(OUT/'SettlementRoutePalette.png')
    parts = [arch(), palisade(), fence()]
    for p in parts: assert p.triangles <= BUDGET, (p.name, p.triangles)
    stats = [p.save(OUT, HEADER) for p in parts]
    (OUT/'SettlementRouteManifest.json').write_text(json.dumps(dict(Revision=1, Style='faceted', Parts=stats, ArchClearWidth=4.64, ArchClearHeight=3.23,
        Scope='Art only: entrance arch, palisade segment and fence section for the settlement approach.'), indent=2)+'\n', encoding='utf-8')
    print('Settlement route:', ', '.join(f"{s['name']} {s['triangles']}" for s in stats))


if __name__ == '__main__':
    main()
