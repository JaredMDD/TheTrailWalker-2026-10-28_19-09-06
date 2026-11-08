"""River crossing art (user request, November 2, 2026: the ferryman needs a ferry). Cable ferry: log raft with a plank
deck, rails and pulley guide posts riding a rope cable; plank dock on pilings with mooring posts, a lantern and a
ferry-call bell; cable posts on both banks. Faceted style with surface detail. Art only: no crossing animation.

Frames (metres, +Y up, +Z = crossing direction along the road, root at the near bank where the road meets the river):
  FerryDock   deck from z 0 to 3.2, top at y 0.10 (the road's end), pilings into the water.
  FerryRaft   centred on its own origin, deck top at y 0.30; placed at z 5 (it rides the cable on its +X side).
  FerryCable  posts at z 3.4 (near dock) and z 16.4 (far bank), rope at x +1.45, height about 1.25 m.
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Environment/Ferry'
HEADER = 'The Trail Walker river ferry (faceted); metres; +Y up; +Z crossing direction.'

pal = k.Palette()
for name, color, *detail in [
    ('Plank', '#8C6A48', 'planks'), ('PlankDark', '#6E523A', 'planks'), ('Post', '#5C4330', 'wood'), ('PostDark', '#45321F', 'wood'),
    ('Log', '#6B4A30', 'bark'), ('LogEnd', '#B08A5C', 'wood'), ('Rope', '#B49A6A', 'thatch'), ('Iron', '#3C3A38'), ('Brass', '#B8913A'),
    ('Glow', '#F2A43A'), ('Moss', '#5E7340', 'needles'), ('Stone', '#7E7A70', 'stone')]:
    pal.add(name, color, *detail)

def box(m, c, size, color, **kw): k.box(m, c, size, color, **kw)

def log(m, a, b, r, color='Log', end='LogEnd'):
    k.limb(m, [(a, r, r), (b, r*.96, r*.96)], color, sides=8, turn=math.pi/8, forward_hint=(0, 1, 0))

def rope(m, points, r=.016):
    for a, b in zip(points, points[1:]):
        along = k.unit(k.sub(b, a)); side = k.unit(k.cross(along, (0, 1, 0) if abs(along[1]) < .9 else (1, 0, 0)))
        box(m, k.lerp(a, b, .5), (r*2, k.length(k.sub(b, a))+.01, r*2), 'Rope', axes=(side, along, k.cross(side, along)))

def coil(m, c, r=.16):
    for i in range(3):
        k.vertical_loft(m, [(c[1]+i*.035, r-.01*i, r-.01*i, r-.01*i), (c[1]+i*.035+.03, r-.01*i, r-.01*i, r-.01*i)], 'Rope', a=.6, b=.5, x=c[0], zc=c[2], cap_start=False, cap_end=False)

def post(m, x, z, top, bottom=-1.1, r=.09, color='Post'):
    k.limb(m, [((x, bottom, z), r, r), ((x, top, z), r*.92, r*.92)], color, sides=6, forward_hint=(0, 0, 1))
    k.vertical_loft(m, [(top, r*.95, r*.95, r*.95), (top+.03, r*.6, r*.6, r*.6)], 'PostDark', a=.6, b=.5, x=x, zc=z, cap_start=False)

def dock():
    m = k.Mesh('FerryDock', pal)
    # Plank deck running toward the water, on two stringers and pilings.
    n = 12
    for i in range(n):
        z = .14 + i*(3.2-.14)/(n-1)
        box(m, (0, .055, z), (2.5 + .04*math.sin(i*2.1), .07, .25), 'Plank' if i % 3 else 'PlankDark', axes=((1, 0, .015*math.sin(i*1.7)), (0, 1, 0), (0, 0, 1)))
    for x in (-.95, .95): box(m, (x, -.06, 1.65), (.16, .16, 3.4), 'PostDark')
    for x in (-1.15, 1.15):
        for z in (.2, 1.7, 3.2): post(m, x, z, .02 if z < 3 else .02, r=.10)
    # Mooring posts at the water end, with rope wraps and a coil on the deck.
    for x in (-1.15, 1.15):
        post(m, x, 3.25, .85, bottom=.05, r=.095)
        for y in (.55, .62): k.vertical_loft(m, [(y, .105, .105, .105), (y+.04, .105, .105, .105)], 'Rope', a=.6, b=.5, x=x, zc=3.25, cap_start=False, cap_end=False)
    coil(m, (-.75, .09, 2.75))
    # Lantern post and a ferry-call bell on a small gallows.
    post(m, -1.15, .35, 1.9, bottom=.05, r=.07)
    box(m, (-.95, 1.82, .35), (.45, .06, .06), 'Post')
    box(m, (-.78, 1.62, .35), (.15, .22, .15), 'Glow', shade=4); box(m, (-.78, 1.76, .35), (.19, .05, .19), 'Iron', taper=.5)
    post(m, 1.15, .45, 1.6, bottom=.05, r=.07)
    box(m, (1.15, 1.55, .80), (.06, .06, .70), 'Post')
    k.vertical_loft(m, [(1.18, .085, .085, .085), (1.32, .07, .07, .07), (1.40, .035, .035, .035)], 'Brass', a=.6, b=.5, x=1.15, zc=1.0)
    box(m, (1.15, 1.47, 1.0), (.02, .10, .02), 'Iron')
    # Mossy stones where the dock meets the bank.
    for i, (x, z, s) in enumerate(((-1.5, .1, .35), (1.55, .25, .3), (-1.35, .7, .22))):
        box(m, (x, .02, z), (s, s*.6, s*.9), 'Stone' if i != 1 else 'Moss', taper=.75)
    return m

def raft():
    m = k.Mesh('FerryRaft', pal)
    # Six pontoon logs along the crossing direction under a cross-planked deck.
    for i in range(6):
        x = -1.05 + i*.42
        log(m, (x, 0, -2.25), (x, 0, 2.25), .19)
    n = 14
    for i in range(n):
        z = -2.05 + i*4.1/(n-1)
        box(m, (0, .24, z), (2.55, .07, .27), 'Plank' if i % 4 else 'PlankDark')
    for x in (-1.05, 1.05): box(m, (x, .30, 0), (.12, .06, 4.2), 'PostDark')
    # Low rails on both sides with open ends for boarding; posts at the corners.
    for x in (-1.22, 1.22):
        for z in (-1.9, -.65, .65, 1.9): post(m, x, z, .95, bottom=.22, r=.055)
        box(m, (x, .92, 0), (.07, .07, 3.9), 'Post')
        box(m, (x, .58, 0), (.06, .05, 3.9), 'PostDark')
    # Guide posts with pulleys on the cable side (+X), so the raft rides the rope.
    for z in (-1.4, 1.4):
        post(m, 1.38, z, 1.45, bottom=.22, r=.07)
        box(m, (1.45, 1.27, z), (.05, .14, .14), 'Iron')
        k.limb(m, [((1.42, 1.27, z), .05, .05), ((1.48, 1.27, z), .05, .05)], 'Iron', sides=8, forward_hint=(0, 1, 0))
    # Cargo: a crate and a rope coil, and the ferryman's pole laid on the deck.
    box(m, (-.55, .48, -1.2), (.5, .42, .5), 'Plank'); box(m, (-.55, .48, -1.2), (.52, .06, .52), 'PostDark')
    coil(m, (.35, .28, 1.5), .14)
    box(m, (-.9, .32, .3), (.05, .05, 2.6), 'Post')
    return m

def cable():
    m = k.Mesh('FerryCable', pal)
    for z, x in ((3.4, 1.45), (16.4, 1.45)):
        post(m, x+.10, z, 1.55, bottom=-.9, r=.10)
        box(m, (x+.02, 1.32, z), (.16, .08, .10), 'Iron')
        for y in (.25, .32): k.vertical_loft(m, [(y, .11, .11, .11), (y+.04, .11, .11, .11)], 'Rope', a=.6, b=.5, x=x+.10, zc=z, cap_start=False, cap_end=False)
    # The rope sags slightly across the river.
    pts = []
    for i in range(9):
        t = i/8; z = 3.4 + t*13.0; y = 1.32 - .22*math.sin(t*math.pi)
        pts.append((1.45, y, z))
    rope(m, pts, .018)
    return m

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    pal.write_png(OUT/'FerryPalette.png')
    parts = [dock(), raft(), cable()]
    stats = [p.save(OUT, HEADER) for p in parts]
    (OUT/'FerryManifest.json').write_text(json.dumps(dict(Revision=1, Style='faceted', Parts=stats, RaftOffset=5.0,
        Scope='Art only: dock, raft and cable for the river crossing. No crossing animation or toll logic.'), indent=2)+'\n', encoding='utf-8')
    print('Ferry:', ', '.join(f"{s['name']} {s['triangles']}" for s in stats))

if __name__ == '__main__':
    main()
