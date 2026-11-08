"""Settlement storefronts (user request, November 2, 2026): shared building kit + the bakery prototype for review.
Faceted style, flat normals, palette strip. Art only: no shop logic, stock or selection.

Coordinates: metres, +Y up, +Z toward the street camera (front face at z = 0 for street fronts). Each shop has
  <Shop>Front    street front (side-scrolling settlement view), root at ground centre of the front face.
  <Shop>Counter  counter view: room, counter (top 1.02 m) and props; the shopkeeper stands at the origin facing +Z.
Offer anchors (three per shop) are written to the manifest for the later shop UI.
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Settlement/Storefronts'
HEADER = 'The Trail Walker settlement storefront (faceted); metres; +Y up; +Z toward the street camera.'
COUNTER_TOP = 1.02

pal = k.Palette()
for name, color, *detail in [
    ('Stone', '#8C877C', 'stone'), ('StoneDark', '#6B665D', 'stone'), ('Plaster', '#E6DCC4', 'plaster'), ('Timber', '#5A3D27', 'wood'), ('TimberLight', '#7D5A3C', 'wood'),
    ('Plank', '#9A7552', 'planks'), ('PlankDark', '#7A5A3E', 'planks'), ('RoofRed', '#9C4A34', 'shingle'), ('RoofDark', '#743426', 'shingle'), ('Brick', '#A2583E', 'brick'),
    ('Glass', '#5E7A86'), ('GlassLight', '#9DB8C0'), ('Iron', '#3A3735'), ('Brass', '#B8913A'), ('Smoke', '#C9C6C0'),
    ('AwningRed', '#B23A2E', 'cloth'), ('AwningCream', '#EFE6D2', 'cloth'), ('Wheat', '#D9B45A', 'thatch'), ('WheatDark', '#B48E3E', 'thatch'),
    ('Bread', '#C9883E', 'leather'), ('BreadDark', '#9C6128', 'leather'), ('Crust', '#E0A955', 'leather'), ('Flour', '#F2EEE6', 'plaster'), ('Sack', '#CDBB93', 'cloth'), ('SackDark', '#A8966E', 'cloth'),
    ('Glow', '#F2A43A'), ('GlowHot', '#FFD27A'), ('Soot', '#3E3A36'), ('Green', '#6E8A4A'), ('Cloth', '#C6463A', 'cloth'),
    ('Dome', '#CFE3E6'),
    # Ranch, farm, mine, fishery, tailor and warehouse (November 2, 2026).
    ('BarnRed', '#8E3B2C', 'planks'), ('BarnRedDark', '#6E2C22', 'planks'), ('Trim', '#E4DCC8', 'wood'), ('Hay', '#D8B860', 'thatch'), ('HayDark', '#B0913E', 'thatch'),
    ('Thatch', '#B89A5A', 'thatch'), ('ThatchDark', '#8F7640', 'thatch'), ('Rock', '#77726A', 'plaster'), ('RockDark', '#5A564F', 'plaster'), ('Ore', '#7C5A44', 'stone'),
    ('Copper', '#B06A3C', 'stone'), ('Coal', '#2E2C2A', 'soil'), ('Water', '#3C6E86'), ('Navy', '#3E5A78', 'planks'), ('NavyDark', '#2C4058', 'planks'),
    ('Oilskin', '#D9A23A', 'leather'), ('Net', '#A8946A', 'cloth'), ('BuoyRed', '#B8402E'), ('BuoyWhite', '#E8E4DA'), ('Ice', '#D6EAEE'), ('Fish', '#8E9EA4', 'leather'),
    ('FishBelly', '#C8CEC8'), ('Plum', '#5A3E5D', 'wood'), ('PlumDark', '#432E46', 'wood'), ('FabricRed', '#B65A4A', 'cloth'), ('FabricBlue', '#5A7A9A', 'cloth'),
    ('FabricGold', '#C9B26A', 'cloth'), ('FabricGreen', '#6E8A5A', 'cloth'), ('Cheese', '#E2B84E', 'leather'), ('CheeseRind', '#B88A2E', 'leather'), ('Meat', '#9C4A3A', 'leather'),
    ('Steel', '#8A8E90'), ('Tomato', '#B8402E', 'leather'), ('Carrot', '#D9822E', 'leather'), ('Cabbage', '#7EA05A', 'needles'), ('Leaf', '#5E8A3A', 'needles'),
    ('Pumpkin', '#D98A2E', 'leather'), ('Gold', '#D9A441'), ('Ledger', '#5B2E26', 'leather'), ('Paper', '#E6DCC0', 'plaster'), ('Flower', '#C8506A'), ('Soil', '#5E4632', 'soil'),
    ('GlassDark', '#2E3A40'), ('PostDark', '#45321F', 'wood')]:
    pal.add(name, color, *detail)

# ------------------------------------------------------------------ shared kit

def box(m, c, size, color, **kw): k.box(m, c, size, color, **kw)

def wall_with_openings(m, x0, x1, y0, y1, z, thick, openings, color):
    """Front wall slab with rectangular openings (x0, x1, y0, y1); built from strips so openings are real holes."""
    xs = sorted({x0, x1} | {o[0] for o in openings} | {o[1] for o in openings})
    ys = sorted({y0, y1} | {o[2] for o in openings} | {o[3] for o in openings})
    for xa, xb in zip(xs, xs[1:]):
        for ya, yb in zip(ys, ys[1:]):
            cx, cy = (xa+xb)/2, (ya+yb)/2
            if any(o[0] <= cx <= o[1] and o[2] <= cy <= o[3] for o in openings): continue
            box(m, (cx, cy, z), (xb-xa, yb-ya, thick), color)

def window(m, x0, x1, y0, y1, z, frame='Timber', glass='Glass', panes=(2, 2), sill=True):
    w, h = x1-x0, y1-y0; cx, cy = (x0+x1)/2, (y0+y1)/2
    if glass: box(m, (cx, cy, z-.05), (w, h, .02), glass)
    for x in (x0, x1): box(m, (x, cy, z), (.08, h+.08, .10), frame)
    for y in (y0, y1): box(m, (cx, y, z), (w+.08, .08, .10), frame)
    for i in range(1, panes[0]): box(m, (x0+w*i/panes[0], cy, z-.02), (.04, h, .05), frame)
    for i in range(1, panes[1]): box(m, (cx, y0+h*i/panes[1], z-.02), (w, .04, .05), frame)
    if sill: box(m, (cx, y0-.06, z+.06), (w+.20, .06, .18), 'TimberLight')

def shutters(m, x0, x1, y0, y1, z, color='Green'):
    w = (x1-x0)/2
    for s, x in ((-1, x0-w/2-.04), (1, x1+w/2+.04)):
        box(m, (x, (y0+y1)/2, z+.02), (w, y1-y0, .05), color)
        for i in range(3): box(m, (x, y0+(y1-y0)*(i+.5)/3, z+.05), (w*.8, .03, .02), 'Timber')

def door(m, x0, x1, y0, y1, z, color='Plank'):
    w = x1-x0; cx = (x0+x1)/2
    for i in range(4): box(m, (x0+w*(i+.5)/4, (y0+y1)/2, z-.04), (w/4-.01, y1-y0, .06), color if i % 2 else 'PlankDark')
    for y in (y0+.25, y1-.3): box(m, (cx, y, z), (w, .07, .04), 'Iron')
    box(m, (x1-.12, (y0+y1)/2, z+.03), (.05, .05, .05), 'Brass')
    for x in (x0, x1): box(m, (x, (y0+y1)/2+.04, z+.02), (.10, y1-y0+.08, .12), 'Timber')
    box(m, ((x0+x1)/2, y1+.05, z+.02), (w+.2, .10, .12), 'Timber')

def gable_roof(m, x0, x1, eave_y, ridge_y, z_front, z_back, color='RoofRed', dark='RoofDark', overhang=.35):
    """Ridge runs front to back (gable faces the street). Two slabs plus a ridge cap and the gable wall."""
    cx = (x0+x1)/2; half = (x1-x0)/2+overhang
    zf, zb = z_front+overhang, z_back-overhang
    for s in (-1, 1):
        a = (cx, ridge_y+.06, zf); b = (cx, ridge_y+.06, zb); c = (cx+s*half, eave_y-.15, zb); d = (cx+s*half, eave_y-.15, zf)
        up = (0, 1, 0)
        ids = [m.v(p) for p in (a, b, c, d)]
        m.quad(*ids, color, (cx, eave_y-1, (zf+zb)/2))
        low = [m.v(k.add(p, (0, -.12, 0))) for p in (a, b, c, d)]
        m.quad(*low, dark, (cx, ridge_y+1, (zf+zb)/2))
        m.quad(ids[3], ids[2], low[2], low[3], dark, (cx, eave_y, (zf+zb)/2))
        m.quad(ids[0], ids[3], low[3], low[0], dark, (cx, eave_y, zb-1))
        # Tile courses: thin steps across the slab.
        for t in (.2, .4, .6, .8):
            p0 = k.lerp(a, d, t); p1 = k.lerp(b, c, t)
            mid = k.lerp(p0, p1, .5)
            box(m, k.add(mid, (0, .03, 0)), (.05, .04, abs(zf-zb)), dark, shade=1)
    box(m, (cx, ridge_y+.08, (zf+zb)/2), (.16, .10, abs(zf-zb)+.06), dark)
    # Gable triangle (plaster with timber).
    g = [m.v((x0, eave_y, z_front)), m.v((x1, eave_y, z_front)), m.v((cx, ridge_y, z_front))]
    m.tri(*g, 'Plaster', (cx, eave_y+.5, z_front-1))

def timber_frame(m, x0, x1, y0, y1, z, posts, braces=()):
    for x in posts: box(m, (x, (y0+y1)/2, z+.03), (.14, y1-y0, .12), 'Timber')
    for y in (y0, y1): box(m, ((x0+x1)/2, y, z+.03), (x1-x0+.1, .14, .14), 'Timber')
    for bay, (a, b) in enumerate(zip(posts, posts[1:])):
            if bay not in braces: continue
            mid = ((a+b)/2, (y0+y1)/2, z+.035); L = math.hypot(b-a, y1-y0)*.55
            for s in (-1, 1):
                d = k.unit((s*(b-a), y1-y0, 0))
                box(m, k.add(mid, (s*-(b-a)*.18, 0, 0)), (.10, L, .08), 'Timber', axes=(k.unit(k.cross(d, (0, 0, 1))), d, (0, 0, 1)))

def awning(m, x0, x1, y, depth, colors=('AwningRed', 'AwningCream'), z=0.0, stripes=8):
    w = (x1-x0)/stripes
    for i in range(stripes):
        cx = x0+w*(i+.5)
        box(m, (cx, y-.18, z+depth/2), (w+.002, .03, depth), colors[i % 2], axes=((1, 0, 0), (0, 1, .42), (0, -.42, 1)))
        box(m, (cx, y-.36-.06, z+depth-.02), (w, .14, .02), colors[i % 2], taper=.6)
    for x in (x0, x1): box(m, (x, y-.2, z+depth/2), (.04, .04, depth*1.05), 'Iron', axes=((1, 0, 0), (0, 1, .42), (0, -.42, 1)))

def hanging_sign(m, x, y, z_wall, reach=.9):
    """Bracket sign turned to face the street camera (the settlement scrolls sideways past the fronts)."""
    box(m, (x+reach/2, y, z_wall+.30), (reach, .06, .06), 'Iron')
    box(m, (x+.15, y-.20, z_wall+.30), (.04, .45, .04), 'Iron', axes=((1, .9, 0), (-.9, 1, 0), (0, 0, 1)))
    box(m, (x, y, z_wall+.15), (.10, .10, .30), 'Iron')
    for dx in (.18, reach-.12): box(m, (x+dx, y-.10, z_wall+.30), (.02, .20, .02), 'Iron')
    c = (x+reach/2+.02, y-.55, z_wall+.30)
    box(m, c, (.70, .62, .08), 'Timber')
    box(m, c, (.62, .54, .10), 'Plank')
    return c

def wheat_icon(m, c, scale=1.0, axis_x=True):
    """Wheat sheaf relief for the sign (both faces): stalks, ears and a tie."""
    for face in (-1, 1):
        ox = 0
        for i, ang in enumerate((-.35, -.15, 0, .15, .35)):
            base = k.add(c, (0, -.18*scale, face*.055))
            d = (0, math.cos(ang), math.sin(ang))
            tip = k.add(base, k.mul(d, .34*scale))
            d = (math.sin(ang), math.cos(ang), 0)
            tip = k.add(base, k.mul(d, .34*scale))
            box(m, k.lerp(base, tip, .5), (.018, .34*scale, .012), 'WheatDark', axes=(k.cross(d, (0, 0, 1)), d, (0, 0, 1)))
            box(m, k.add(tip, k.mul(d, -.04*scale)), (.04, .11*scale, .02), 'Wheat', axes=(k.cross(d, (0, 0, 1)), d, (0, 0, 1)), taper=.4)
        box(m, k.add(c, (0, -.06*scale, face*.058)), (.14*scale, .03, .02), 'AwningRed')

def loaf(m, c, w, h, d, color='Bread', crust='Crust', turn=0.0):
    ax = (math.cos(turn), 0, -math.sin(turn)); az = (math.sin(turn), 0, math.cos(turn))
    rows = [(k.add(c, (0, -h/2, 0)), k.ring_points(k.add(c, (0, -h/2, 0)), ax, az, k.chamfer_rect(w/2, d/2, d/2, .7, .6))),
            (k.add(c, (0, h*.15, 0)), k.ring_points(k.add(c, (0, h*.15, 0)), ax, az, k.chamfer_rect(w/2*1.02, d/2*1.02, d/2*1.02, .7, .6))),
            (k.add(c, (0, h*.38, 0)), k.ring_points(k.add(c, (0, h*.38, 0)), ax, az, k.chamfer_rect(w/2*.92, d/2*.86, d/2*.86, .65, .55))),
            (k.add(c, (0, h/2, 0)), k.ring_points(k.add(c, (0, h/2, 0)), ax, az, k.chamfer_rect(w/2*.66, d/2*.48, d/2*.48, .6, .5)))]
    k.loft(m, rows, color, colors=[color, crust, crust])
    for i in (-1, 0, 1):
        # Scored crust lines across the top.
        box(m, k.add(c, k.add(k.mul(ax, i*w*.22), (0, h/2-.004, 0))), (w*.06, .008, d*.55), 'BreadDark', axes=(ax, (0, 1, 0), az))
    box(m, k.add(c, (0, h/2-.002, 0)), (w*.6, .006, d*.18), 'BreadDark', axes=(ax, (0, 1, 0), az))

def round_loaf(m, c, r, color='Bread'):
    k.vertical_loft(m, [(c[1], r*.9, r*.9, r*.9), (c[1]+r*.35, r, r, r), (c[1]+r*.75, r*.7, r*.7, r*.7), (c[1]+r*.9, r*.25, r*.25, r*.25)],
                    color, a=.6, b=.5, x=c[0], zc=c[2], colors=[color, 'Crust', color])

def sack(m, c, w=.42, h=.55, color='Sack'):
    k.vertical_loft(m, [(c[1], w*.42, w*.32, w*.32), (c[1]+h*.45, w*.50, w*.40, w*.40), (c[1]+h*.85, w*.34, w*.26, w*.26), (c[1]+h, w*.14, w*.10, w*.10)],
                    color, a=.65, b=.55, x=c[0], zc=c[2], colors=[color, color, 'SackDark'])
    box(m, (c[0], c[1]+h*.88, c[2]), (w*.32, .04, w*.24), 'Timber')

def barrel_crate(m, c, size=(.6, .5, .5), color='Plank'):
    sx, sy, sz = size
    box(m, (c[0], c[1]+sy/2, c[2]), size, color)
    for dy in (.06, sy-.06): box(m, (c[0], c[1]+dy, c[2]+sz/2+.005), (sx+.01, .05, .01), 'PlankDark')

def lantern(m, c):
    box(m, (c[0], c[1], c[2]), (.16, .22, .16), 'Glow', shade=4)
    for dx in (-.08, .08):
        for dz in (-.08, .08): box(m, (c[0]+dx, c[1], c[2]+dz), (.02, .24, .02), 'Iron')
    box(m, (c[0], c[1]+.14, c[2]), (.20, .04, .20), 'Iron', taper=.5)
    box(m, (c[0], c[1]+.22, c[2]), (.02, .12, .02), 'Iron')

def smoke(m, c):
    for i, (dx, dy, r) in enumerate([(0, 0, .22), (.12, .38, .28), (.30, .78, .34), (.55, 1.2, .38)]):
        k.vertical_loft(m, [(c[1]+dy-r*.6, r*.6, r*.6, r*.6), (c[1]+dy, r, r, r), (c[1]+dy+r*.6, r*.55, r*.55, r*.55)], 'Smoke', a=.6, b=.5, x=c[0]+dx, zc=c[2])

# ------------------------------------------------------------------ bakery

W, D = 6.4, 4.6         # footprint: width across the street, depth back from the front face
GROUND, FLOOR1, FLOOR2, EAVE, RIDGE = .5, 3.3, 3.45, 5.6, 7.7

def bakery_front():
    m = k.Mesh('BakeryFront', pal)
    x0, x1 = -W/2, W/2
    # Stone footing and side walls (depth keeps a three-quarter view solid).
    box(m, (0, GROUND/2, -D/2), (W+.2, GROUND, D+.1), 'Stone')
    for i in range(9): box(m, (x0+.35+i*.72, GROUND*.55, .06), (.62, .30, .04), 'StoneDark' if i % 2 else 'Stone')
    box(m, (x0+.1, (GROUND+EAVE)/2, -D/2), (.2, EAVE-GROUND, D), 'Plaster')
    box(m, (x1-.1, (GROUND+EAVE)/2, -D/2), (.2, EAVE-GROUND, D), 'Plaster')
    box(m, (0, (GROUND+EAVE)/2, -D+.1), (W, EAVE-GROUND, .2), 'Plaster')
    # Ground floor: large display window, door, plaster with timber frame.
    win = (-2.75, -.45, 1.05, 2.65); dr = (.55, 1.65, GROUND, 2.75)
    wall_with_openings(m, x0, x1, GROUND, FLOOR1, -.1, .2, [win, dr], 'Plaster')
    timber_frame(m, x0, x1, GROUND, FLOOR1, 0, [x0+.07, -2.85, -.35, .45, 1.75, x1-.07])
    window(m, *win, z=.04, glass=None, panes=(3, 2))
    box(m, (-1.6, 1.85, -.62), (2.3, 1.6, .05), 'Timber')
    lantern(m, (-1.6, 2.45, -.45))
    door(m, *dr, z=.02)
    # Bread racks and loaves inside the display window.
    for sy in (1.18, 1.78):
        box(m, (-1.6, sy, -.30), (2.2, .04, .45), 'Plank')
        for i in range(6):
            loaf(m, (-2.45+i*.34, sy+.08, -.28), .26, .12, .14, turn=.2*(i % 3-1))
    for i in range(4): round_loaf(m, (-2.3+i*.55, 2.36, -.3), .11)
    # Jettied upper floor with shuttered windows and braced timber frame.
    box(m, (0, FLOOR2-.04, .12), (W+.3, .16, .40), 'Timber')
    box(m, (0, (FLOOR2+EAVE)/2, .10), (W+.2, EAVE-FLOOR2, .2), 'Plaster')
    timber_frame(m, x0-.1, x1+.1, FLOOR2, EAVE, .2, [x0-.03, -2.3, -.9, .9, 2.3, x1+.03], braces=(0, 2, 4))
    for wx in (-1.6, 1.6):
        window(m, wx-.45, wx+.45, 4.0, 5.0, .26, panes=(2, 2))
        shutters(m, wx-.45, wx+.45, 4.0, 5.0, .24)
    # Gable roof facing the street, brick chimney with smoke.
    gable_roof(m, x0-.1, x1+.1, EAVE, RIDGE, .2, -D)
    box(m, (0, (EAVE+RIDGE)/2-.3, .22), (.2, RIDGE-EAVE-.8, .1), 'Timber')
    for s in (-1, 1): box(m, (s*1.3, EAVE+.55, .22), (.14, 1.3, .1), 'Timber', axes=((1, 0, 0), (-s*.65, 1, 0), (0, 0, 1)))
    box(m, (2.0, RIDGE-.55, -2.9), (.7, 2.0, .7), 'Brick')
    box(m, (2.0, RIDGE+.47, -2.9), (.84, .14, .84), 'StoneDark')
    smoke(m, (2.0, RIDGE+.85, -2.9))
    # Striped awning over the display window, hanging wheat sign by the door, props.
    awning(m, -2.95, -.25, 3.05, 1.05)
    sign = hanging_sign(m, 2.45, 2.95, .1)
    wheat_icon(m, sign)
    lantern(m, (.30, 2.55, .22)); box(m, (.30, 2.85, .12), (.04, .04, .22), 'Iron')
    box(m, (-1.6, .78, .55), (2.0, .08, .45), 'Plank'); box(m, (-2.45, .64, .55), (.10, .28, .38), 'PlankDark'); box(m, (-.75, .64, .55), (.10, .28, .38), 'PlankDark')
    for x in (-2.15, -1.05):
        k.vertical_loft(m, [(.82, .20, .15, .15), (.96, .25, .19, .19)], 'WheatDark', a=.6, b=.5, x=x, zc=.55, cap_end=False)
        for i in range(3): loaf(m, (x-.07+i*.07, .99, .52+i*.03), .22, .10, .12, turn=.4*(i-1))
    round_loaf(m, (-1.6, .82, .55), .13)
    for i, x in enumerate((2.3, 2.75)): sack(m, (x, 0, .45), w=.46, h=.62)
    box(m, (-.15, .3, .6), (.50, .30, .40), 'Plank'); [loaf(m, (-.3+i*.15, .52, .6), .14, .08, .10) for i in range(3)]
    return m

def bakery_counter():
    m = k.Mesh('BakeryCounter', pal)
    # Room: plank floor, plaster back wall with timber, side walls.
    for i in range(14): box(m, (-3.25+i*.5, -.03, -.4), (.49, .06, 3.6), 'Plank' if i % 2 else 'PlankDark')
    box(m, (0, 1.6, -1.95), (6.8, 3.2, .1), 'Plaster')
    for x in (-3.3, -1.1, 1.1, 3.3): box(m, (x, 1.6, -1.88), (.16, 3.2, .1), 'Timber')
    box(m, (0, 3.1, -1.88), (6.8, .16, .12), 'Timber')
    for s in (-1, 1): box(m, (s*3.4, 1.6, -.4), (.1, 3.2, 3.2), 'Plaster')
    box(m, (0, 3.25, -.4), (6.9, .1, 3.2), 'Plaster')
    for z in (-1.6, -.6, .4): box(m, (0, 3.13, z), (6.8, .16, .18), 'Timber')
    # Bread shelves on the back wall.
    for y in (1.15, 1.65, 2.15):
        box(m, (-1.9, y, -1.70), (2.4, .05, .40), 'Plank')
        for i in range(7): loaf(m, (-2.95+i*.35, y+.09, -1.68), .28, .13, .15, turn=.25*((i*7) % 3-1))
    for y in (1.35, 1.95):
        box(m, (2.05, y, -1.70), (1.0, .05, .40), 'Plank')
    for i in range(3): round_loaf(m, (1.75+i*.30, 1.40, -1.68), .11)
    for i in range(2): round_loaf(m, (1.85+i*.40, 2.00, -1.68), .13, 'BreadDark')
    # Brick oven in the corner with a warm glow.
    box(m, (2.6, .65, -1.45), (1.3, 1.3, .9), 'Brick')
    k.vertical_loft(m, [(1.30, .65, .45, .45), (1.80, .55, .40, .40), (2.10, .30, .25, .25)], 'Brick', a=.6, b=.5, x=2.6, zc=-1.45)
    box(m, (2.6, .70, -.99), (.56, .46, .04), 'Soot')
    box(m, (2.6, .62, -.96), (.46, .30, .03), 'Glow', shade=4)
    box(m, (2.6, .54, -.95), (.30, .10, .03), 'GlowHot', shade=4)
    # Flour sacks and a peel leaning on the wall.
    for i, (x, z) in enumerate(((-2.9, -1.2), (-2.5, -1.4), (-2.95, -.75))): sack(m, (x, 0, z), color='Sack' if i % 2 else 'SackDark')
    box(m, (1.55, .95, -1.75), (.06, 1.9, .06), 'TimberLight', axes=((1, 0, 0), (.18, 1, 0), (0, 0, 1)))
    box(m, (1.42, .25, -1.73), (.34, .42, .03), 'TimberLight')
    # Counter: plank front, thick top at 1.02 m, with a cloth runner.
    box(m, (0, COUNTER_TOP-.04, .45), (3.4, .08, .62), 'TimberLight')
    box(m, (0, (COUNTER_TOP-.08)/2, .72), (3.3, COUNTER_TOP-.08, .06), 'Plank')
    for i in range(7): box(m, (-1.5+i*.5, (COUNTER_TOP-.08)/2, .755), (.06, COUNTER_TOP-.10, .03), 'PlankDark')
    box(m, (0, (COUNTER_TOP-.08)/2, .17), (3.3, COUNTER_TOP-.08, .06), 'PlankDark')
    box(m, (0, COUNTER_TOP+.003, .45), (1.0, .006, .66), 'AwningCream')
    # Offer displays: one common basket, two premium glass domes.
    k.vertical_loft(m, [(COUNTER_TOP, .17, .13, .13), (COUNTER_TOP+.11, .22, .17, .17)], 'WheatDark', a=.6, b=.5, x=-1.15, zc=.48, cap_end=False)
    for i in range(3): loaf(m, (-1.21+i*.06, COUNTER_TOP+.14, .44+i*.04), .22, .10, .12, turn=.4*(i-1))
    for x in (.85, 1.40):
        box(m, (x, COUNTER_TOP+.02, .48), (.34, .04, .34), 'Brass')
        k.vertical_loft(m, [(COUNTER_TOP+.04, .15, .15, .15), (COUNTER_TOP+.22, .15, .15, .15), (COUNTER_TOP+.34, .10, .10, .10), (COUNTER_TOP+.38, .03, .03, .03)],
                        'Dome', a=.6, b=.5, x=x, zc=.48, cap_start=False)
        box(m, (x, COUNTER_TOP+.42, .48), (.05, .05, .05), 'Brass')
    lantern(m, (-.6, 2.55, -1.6))
    return m, [(-1.15, COUNTER_TOP, .48), (.85, COUNTER_TOP, .48), (1.40, COUNTER_TOP, .48)]

def main():
    # Every shop (the bakery here plus the six in storefront_shops) is written by storefront_shops.main().
    import storefront_shops
    storefront_shops.main()

if __name__ == '__main__':
    main()
