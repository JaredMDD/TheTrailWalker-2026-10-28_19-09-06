"""The other six storefronts (user, November 2, 2026): ranch, farm, mine, fishery, tailor-material shop and the
warehouse/trading house, built on the shared kit in generate_storefront_meshes (which keeps the bakery). Each shop
has <Shop>Front (street) and <Shop>Counter (counter view, keeper at the origin, three offer anchors).
Run: python Tools/Art/generate_storefront_meshes.py   (writes every shop)
"""
from generate_storefront_meshes import *          # shared kit, palette and the bakery
import generate_storefront_meshes as gs
import copy

# ------------------------------------------------------------------ general shapes

def hexa(m, pts, color, under=None, sides=None):
    """Eight corners: top face 0-3 then bottom face 4-7 in the same order."""
    inside = tuple(sum(p[a] for p in pts)/8 for a in range(3)); ids = [m.v(p) for p in pts]
    m.quad(ids[0], ids[1], ids[2], ids[3], color, inside); m.quad(ids[4], ids[5], ids[6], ids[7], under or color, inside)
    for a in range(4):
        b = (a+1) % 4; m.quad(ids[a], ids[b], ids[4+b], ids[4+a], sides or under or color, inside)

def slab(m, p, q, zf, zb, thick, color, under):
    """Roof slab between two profile points (x, y), running from zf (front) back to zb."""
    d = (q[0]-p[0], q[1]-p[1]); L = math.hypot(*d); n = (-d[1]/L, d[0]/L)
    if n[1] < 0: n = (-n[0], -n[1])
    top = [(p[0], p[1], zf), (q[0], q[1], zf), (q[0], q[1], zb), (p[0], p[1], zb)]
    hexa(m, top + [(x-n[0]*thick, y-n[1]*thick, z) for x, y, z in top], color, under)

def roof(m, profile, zf, zb, color, under, thick=.14, overhang=.35):
    pts = list(profile)
    for end, nxt in ((0, 1), (-1, -2)):
        a, b = pts[end], pts[nxt]; d = (a[0]-b[0], a[1]-b[1]); L = math.hypot(*d)
        pts[end] = (a[0]+d[0]/L*overhang, a[1]+d[1]/L*overhang)
    for a, b in zip(pts, pts[1:]): slab(m, a, b, zf+overhang, zb-overhang, thick, color, under)

def prism(m, poly, z0, z1, color, side=None):
    """Extrudes a star-shaped XY polygon between z0 (front) and z1 (back)."""
    cx = sum(p[0] for p in poly)/len(poly); cy = sum(p[1] for p in poly)/len(poly); inside = (cx, cy, (z0+z1)/2)
    f = [m.v((x, y, z0)) for x, y in poly]; b = [m.v((x, y, z1)) for x, y in poly]; cf = m.v((cx, cy, z0)); cb = m.v((cx, cy, z1))
    for a in range(len(poly)):
        c = (a+1) % len(poly)
        m.tri(cf, f[a], f[c], color, inside); m.tri(cb, b[c], b[a], color, inside); m.quad(f[a], f[c], b[c], b[a], side or color, inside)

def rock(m, c, w, h, d, color='Rock', seed=0):
    j = lambda t: 1 + .12*math.sin(seed*3.1 + t*7.3)
    k.vertical_loft(m, [(c[1], w*j(1), d*j(2), d*j(3)), (c[1]+h*.45, w*.92*j(4), d*.9*j(5), d*.92*j(6)), (c[1]+h*.85, w*.55*j(7), d*.5*j(8), d*.55*j(9)), (c[1]+h, w*.2, d*.2, d*.2)],
                    color, a=.55+.1*math.sin(seed), b=.5, x=c[0], zc=c[2], colors=[color, 'RockDark' if seed % 2 and color == 'Rock' else color, color])

def ring_band(m, c, y, r, color, h=.04):
    k.vertical_loft(m, [(y-h/2, r, r, r), (y+h/2, r, r, r)], color, a=.62, b=.55, x=c[0], zc=c[2], cap_start=False, cap_end=False)

def barrel(m, c, r=.28, h=.8, color='Plank', band='Iron'):
    k.vertical_loft(m, [(c[1], r*.88, r*.88, r*.88), (c[1]+h*.5, r, r, r), (c[1]+h, r*.88, r*.88, r*.88)], color, a=.62, b=.55, x=c[0], zc=c[2])
    for t, rr in ((.12, .9), (.5, 1.01), (.88, .9)): ring_band(m, c, c[1]+h*t, r*rr+.006, band)

def crate(m, c, s=(.55, .45, .45), color='Plank', turn=0.0):
    ax = (math.cos(turn), 0, -math.sin(turn)); az = (math.sin(turn), 0, math.cos(turn))
    box(m, k.add(c, (0, s[1]/2, 0)), s, color, axes=(ax, (0, 1, 0), az))
    for dy in (.05, s[1]-.05): box(m, k.add(c, (0, dy, 0)), (s[0]+.012, .05, s[2]+.012), 'PlankDark', axes=(ax, (0, 1, 0), az))

def churn(m, c, r=.16, h=.55):
    k.vertical_loft(m, [(c[1], r, r, r), (c[1]+h*.7, r, r, r), (c[1]+h*.85, r*.6, r*.6, r*.6), (c[1]+h, r*.55, r*.55, r*.55)], 'Steel', a=.62, b=.55, x=c[0], zc=c[2])
    box(m, (c[0], c[1]+h+.03, c[2]), (r*1.2, .05, r*1.2), 'Steel')

def produce(m, c, color, n=7, r=.055, spread=.16, seed=1):
    for a in range(n):
        ang = a*2.4+seed; rad = spread*math.sqrt((a+.5)/n)
        x, z = c[0]+rad*math.cos(ang), c[2]+rad*math.sin(ang)
        k.vertical_loft(m, [(c[1]+.01*(a % 2), r*.8, r*.8, r*.8), (c[1]+r*.9, r, r, r), (c[1]+r*1.7, r*.55, r*.55, r*.55)], color, a=.6, b=.5, x=x, zc=z)

def basket(m, c, r=.22, h=.14, color='WheatDark'):
    k.vertical_loft(m, [(c[1], r*.75, r*.6, r*.6), (c[1]+h, r, r*.8, r*.8)], color, a=.6, b=.5, x=c[0], zc=c[2], cap_end=False)

def fish(m, c, length=.36, turn=0.0, color='Fish'):
    ax = (math.cos(turn), 0, -math.sin(turn)); az = (math.sin(turn), 0, math.cos(turn)); up = (0, 1, 0)
    rings = []
    for t, h, w in ((-.5, .02, .015), (-.3, .06, .035), (0, .08, .045), (.3, .055, .035), (.5, .012, .01)):
        cc = k.add(c, k.mul(ax, t*length)); rings.append((cc, k.ring_points(cc, az, up, k.hexagon(w, h))))
    k.loft(m, rings, color, colors=[color, 'FishBelly', color, color])
    box(m, k.add(c, k.mul(ax, -.56*length)), (.06, .08, .01), color, axes=(ax, up, az), taper=1.6)

def bolt(m, c, length=.9, r=.07, color='FabricRed'):
    k.limb(m, [(k.add(c, (-length/2, 0, 0)), r, r), (k.add(c, (length/2, 0, 0)), r, r)], color, sides=8, turn=math.pi/8, forward_hint=(0, 1, 0))
    for x in (-length/2-.01, length/2+.01):
        k.limb(m, [(k.add(c, (x-.01, 0, 0)), .025, .025), (k.add(c, (x+.01, 0, 0)), .025, .025)], 'PlankDark', sides=6, forward_hint=(0, 1, 0))

def wheel(m, c, r, axis_x=True, color='Iron', w=.08):
    a = k.add(c, (-w/2, 0, 0)) if axis_x else k.add(c, (0, 0, -w/2)); b = k.add(c, (w/2, 0, 0)) if axis_x else k.add(c, (0, 0, w/2))
    k.limb(m, [(a, r, r), (b, r, r)], color, sides=8, turn=math.pi/8, forward_hint=(0, 1, 0))

def fence(m, x0, x1, z, posts=4, h=1.0):
    for a in range(posts): box(m, (x0+(x1-x0)*a/(posts-1), h/2, z), (.12, h, .12), 'Timber')
    for y in (.4, .8): box(m, ((x0+x1)/2, y, z), (x1-x0+.1, .09, .06), 'TimberLight')

def diagonal(m, c, length, ang, size, color):
    d = (math.sin(ang), math.cos(ang), 0); side = k.cross(d, (0, 0, 1))
    box(m, c, (size, length, size), color, axes=(side, d, (0, 0, 1)))

# Sign icons, drawn on both faces of a street-facing hanging sign.
def faces(sign):
    for s_ in (-1, 1): yield k.add(sign, (0, 0, s_*.056))

def bell_icon(m, sign):
    for c in faces(sign):
        box(m, k.add(c, (0, -.02, 0)), (.20, .22, .02), 'Brass', taper=.45)
        box(m, k.add(c, (0, .12, 0)), (.05, .05, .02), 'Iron'); box(m, k.add(c, (0, -.15, 0)), (.05, .04, .02), 'Iron')

def sprout_icon(m, sign):
    for c in faces(sign):
        box(m, k.add(c, (0, -.06, 0)), (.025, .22, .02), 'Leaf')
        for s_ in (-1, 1): box(m, k.add(c, (s_*.07, .06, 0)), (.13, .06, .02), 'Leaf', axes=((1, s_*.5, 0), (-s_*.5, 1, 0), (0, 0, 1)), taper=.4)
        box(m, k.add(c, (0, -.19, 0)), (.20, .04, .02), 'Soil')

def pick_icon(m, sign):
    for c in faces(sign):
        for s_ in (-1, 1):
            d = k.unit((s_, 1, 0)); side = k.cross(d, (0, 0, 1))
            box(m, c, (.022, .34, .02), 'TimberLight', axes=(side, d, (0, 0, 1)))
            box(m, k.add(c, k.mul(d, .15)), (.20, .03, .02), 'Steel', axes=(side, d, (0, 0, 1)), taper=.4)

def fish_icon(m, sign):
    for c in faces(sign):
        box(m, c, (.10, .26, .02), 'Fish', taper=.6, axes=((0, 1, 0), (1, 0, 0), (0, 0, 1)))
        box(m, k.add(c, (-.17, 0, 0)), (.12, .08, .02), 'Fish', taper=.3, axes=((0, -1, 0), (1, 0, 0), (0, 0, 1)))

def spool_icon(m, sign):
    for c in faces(sign):
        for dy in (-.12, .12): box(m, k.add(c, (0, dy, 0)), (.22, .04, .02), 'TimberLight')
        box(m, c, (.15, .20, .02), 'FabricRed')
        box(m, k.add(c, (.13, -.05, 0)), (.012, .16, .02), 'FabricRed', axes=((1, .4, 0), (-.4, 1, 0), (0, 0, 1)))

def scale_icon(m, sign):
    for c in faces(sign):
        box(m, k.add(c, (0, -.02, 0)), (.025, .26, .02), 'Brass'); box(m, k.add(c, (0, .10, 0)), (.30, .022, .02), 'Brass')
        for s_ in (-1, 1): box(m, k.add(c, (s_*.14, -.04, 0)), (.10, .03, .02), 'Brass', taper=1.6)
        box(m, k.add(c, (0, -.16, 0)), (.14, .03, .02), 'Brass')

# ------------------------------------------------------------------ counter-view room

T = COUNTER_TOP
OFFERS = [(-1.15, T, .48), (.85, T, .48), (1.40, T, .48)]

def room(m, wall='Plaster', timber='Timber', floor=('Plank', 'PlankDark'), ceiling='Plaster'):
    for a in range(14): box(m, (-3.25+a*.5, -.03, -.4), (.49, .06, 3.6), floor[a % 2])
    box(m, (0, 1.6, -1.95), (6.8, 3.2, .1), wall)
    for x in (-3.3, -1.1, 1.1, 3.3): box(m, (x, 1.6, -1.88), (.16, 3.2, .1), timber)
    box(m, (0, 3.1, -1.88), (6.8, .16, .12), timber)
    for s_ in (-1, 1): box(m, (s_*3.4, 1.6, -.4), (.1, 3.2, 3.2), wall)
    box(m, (0, 3.25, -.4), (6.9, .1, 3.2), ceiling)
    for z in (-1.6, -.6, .4): box(m, (0, 3.13, z), (6.8, .16, .18), timber)

def counter(m, top='TimberLight', front='Plank', seams='PlankDark'):
    box(m, (0, T-.04, .45), (3.4, .08, .62), top)
    box(m, (0, (T-.08)/2, .72), (3.3, T-.08, .06), front)
    for a in range(7): box(m, (-1.5+a*.5, (T-.08)/2, .755), (.06, T-.10, .03), seams)
    box(m, (0, (T-.08)/2, .17), (3.3, T-.08, .06), seams)

def shelf(m, x0, x1, y, depth=.40, color='Plank'):
    box(m, ((x0+x1)/2, y, -1.70), (x1-x0, .05, depth), color)

def hanger(m, x, y_top=2.45, color='Iron'):
    box(m, (x, y_top, -1.75), (.02, .2, .02), color)

# ------------------------------------------------------------------ ranch

def ranch_front():
    m = k.Mesh('RanchFront', pal)
    box(m, (0, .15, -2.5), (6.8, .3, 5.2), 'StoneDark')
    prism(m, [(-3.3, .3), (3.3, .3), (3.3, 3.4), (2.3, 5.3), (0, 6.4), (-2.3, 5.3), (-3.3, 3.4)], 0, -5.0, 'BarnRed')
    roof(m, [(-3.3, 3.4), (-2.3, 5.3), (0, 6.4), (2.3, 5.3), (3.3, 3.4)], 0, -5.0, 'RoofDark', 'BarnRedDark', thick=.16, overhang=.4)
    # Double barn doors with white trim and X braces, a hayloft door with hay and a hoist beam.
    for x0, x1 in ((-1.55, 0), (0, 1.55)):
        cx = (x0+x1)/2; box(m, (cx, 1.65, .04), (x1-x0-.04, 2.7, .06), 'BarnRedDark')
        for x in (x0+.06, x1-.06): box(m, (x, 1.65, .09), (.10, 2.7, .05), 'Trim')
        for y in (.36, 2.94): box(m, (cx, y, .09), (x1-x0, .10, .05), 'Trim')
        ang = math.atan2(x1-x0-.2, 2.5)
        for s_ in (-1, 1): diagonal(m, (cx, 1.65, .10), math.hypot(x1-x0-.2, 2.5), s_*ang, .09, 'Trim')
    box(m, (0, 3.08, .1), (3.4, .14, .1), 'Trim')
    box(m, (0, 4.4, .04), (1.3, 1.2, .06), 'BarnRedDark')
    for x in (-.65, .65): box(m, (x, 4.4, .09), (.09, 1.3, .05), 'Trim')
    for y in (3.8, 5.0): box(m, (0, y, .09), (1.4, .09, .05), 'Trim')
    for a in range(5): box(m, (-.4+a*.2, 3.95, .14), (.04, .18, .12), 'Hay', axes=((1, 0, 0), (0, 1, .5), (0, -.5, 1)))
    box(m, (0, 5.75, .55), (.14, .14, 1.2), 'Timber'); box(m, (0, 5.62, 1.05), (.06, .14, .14), 'Iron')
    box(m, (0, 4.7, 1.05), (.03, 1.8, .03), 'Hay')
    for x in (-2.55, 2.55):
        window(m, x-.4, x+.4, 1.6, 2.5, .04, panes=(2, 2)); shutters(m, x-.4, x+.4, 1.6, 2.5, .03, 'Trim')
    # Fence, hay bales, milk churns, a water trough and the cowbell sign.
    fence(m, -3.4, -1.9, 1.2, 3)
    for x, z, y in ((2.2, .8, 0), (2.95, .9, 0), (2.55, .85, .5)):
        box(m, (x, y+.25, z), (.7, .5, .45), 'Hay')
        for dx in (-.18, .18): box(m, (x+dx, y+.25, z), (.05, .52, .47), 'HayDark')
    churn(m, (1.85, 0, .55)); churn(m, (1.55, 0, .75), r=.14, h=.5)
    box(m, (-2.6, .3, .6), (1.0, .4, .45), 'PlankDark'); box(m, (-2.6, .46, .6), (.9, .06, .35), 'Water')
    bell_icon(m, hanging_sign(m, -2.0, 3.25, .1))
    lantern(m, (1.85, 2.6, .25)); box(m, (1.85, 2.9, .14), (.04, .04, .22), 'Iron')
    return m

def ranch_counter():
    m = k.Mesh('RanchCounter', pal); room(m, wall='Plank', timber='Timber'); counter(m)
    box(m, (-1.9, 2.55, -1.75), (2.8, .06, .06), 'Iron')
    for a in range(6):
        x = -3.05+a*.45; hanger(m, x); box(m, (x, 2.12, -1.75), (.16, .48, .13), 'Meat', taper=.6)
    for y in (1.2, 1.75):
        shelf(m, 1.1, 3.1, y)
        for a in range(3): k.vertical_loft(m, [(y+.03, .2, .2, .2), (y+.17, .2, .2, .2)], 'Cheese', a=.62, b=.55, x=1.45+a*.6, zc=-1.68, colors=['CheeseRind'])
    churn(m, (-2.9, 0, -1.2)); churn(m, (-2.55, 0, -1.45), r=.14); churn(m, (2.9, 0, -1.0))
    box(m, (-2.85, .25, -.4), (.7, .5, .5), 'Hay')
    # Offers: a cheese wheel on a board, a cut of meat on a board, a small milk churn.
    box(m, (-1.15, T+.02, .48), (.46, .04, .34), 'TimberLight')
    k.vertical_loft(m, [(T+.04, .17, .17, .17), (T+.16, .17, .17, .17)], 'Cheese', a=.62, b=.55, x=-1.15, zc=.48, colors=['CheeseRind'])
    box(m, (.85, T+.02, .48), (.40, .04, .30), 'TimberLight'); box(m, (.85, T+.08, .48), (.30, .08, .18), 'Meat', taper=.8)
    churn(m, (1.40, T, .48), r=.11, h=.36)
    lantern(m, (.4, 2.55, -1.6))
    return m, OFFERS

# ------------------------------------------------------------------ farm

def farm_front():
    m = k.Mesh('FarmFront', pal)
    box(m, (0, .1, -2.6), (7.0, .2, 5.4), 'Soil')
    prism(m, [(-3.2, .2), (3.2, .2), (3.2, 3.0), (0, 5.0), (-3.2, 3.0)], -1.6, -5.0, 'Plaster')
    window(m, 1.3, 2.3, 1.2, 2.2, -1.56, panes=(2, 2)); shutters(m, 1.3, 2.3, 1.2, 2.2, -1.58, 'FabricGreen')
    door(m, -2.4, -1.5, .2, 2.3, -1.58)
    roof(m, [(-3.6, 2.9), (0, 5.4), (3.6, 2.9)], 1.0, -5.0, 'Thatch', 'ThatchDark', thick=.38, overhang=.3)
    # Market stall under the long thatch: posts and a table of tilted produce crates.
    for x in (-3.0, -1.0, 1.0, 3.0):
        for z in (.85, -1.3): box(m, (x, 1.55, z), (.14, 2.9, .14), 'Timber')
    box(m, (0, .85, .25), (5.8, .08, 1.0), 'PlankDark')
    for x in (-2.75, 2.75): box(m, (x, .42, .25), (.1, .84, .9), 'Timber')
    colors = ['Tomato', 'Carrot', 'Cabbage', 'Pumpkin', 'Tomato', 'Cabbage']
    for a, x in enumerate((-2.25, -1.35, -.45, .45, 1.35, 2.25)):
        crate(m, (x, .9, .3), (.8, .22, .7), 'Plank')
        produce(m, (x, 1.08, .3), colors[a], n=9, r=.065 if colors[a] != 'Pumpkin' else .10, spread=.28, seed=a)
    awning(m, -3.1, 3.1, 2.75, .95, colors=('FabricGreen', 'Trim'), z=.85, stripes=10)
    # Scarecrow with a straw hat, pumpkins, sacks and the sprout sign.
    sx = 3.75; box(m, (sx, 1.1, .4), (.07, 2.2, .07), 'Timber'); box(m, (sx, 1.7, .4), (1.1, .07, .07), 'Timber')
    box(m, (sx, 1.45, .4), (.5, .55, .22), 'FabricBlue', taper=1.25); sack(m, (sx, 1.85, .4), w=.30, h=.32)
    k.vertical_loft(m, [(2.18, .30, .30, .30), (2.20, .30, .30, .30), (2.21, .13, .13, .13), (2.34, .10, .10, .10)], 'Hay', a=.6, b=.5, x=sx, zc=.4)
    for x, r in ((-3.6, .22), (-3.15, .17)):
        k.vertical_loft(m, [(0, r, r, r), (r*.9, r*1.1, r*1.1, r*1.1), (r*1.6, r*.5, r*.5, r*.5)], 'Pumpkin', a=.6, b=.5, x=x, zc=.9)
    sack(m, (-3.4, 0, -.2)); sack(m, (3.3, 0, -.6), color='SackDark')
    sprout_icon(m, hanging_sign(m, -3.0, 3.1, .85, reach=.7))
    return m

def farm_counter():
    m = k.Mesh('FarmCounter', pal); room(m); counter(m)
    colors = ['Tomato', 'Carrot', 'Cabbage', 'Pumpkin']
    for r_, y in enumerate((1.1, 1.65, 2.2)):
        shelf(m, -3.2, -.7, y)
        for a in range(4):
            crate(m, (-2.95+a*.6, y+.03, -1.70), (.5, .18, .34))
            produce(m, (-2.95+a*.6, y+.2, -1.70), colors[(a+r_) % 4], n=6, r=.05, spread=.15, seed=a+r_)
    box(m, (2.0, 2.9, -1.75), (2.6, .05, .05), 'Timber')
    for a in range(5):
        x = 1.0+a*.45; box(m, (x, 2.75, -1.75), (.02, .3, .02), 'Thatch')
        box(m, (x, 2.45, -1.75), (.14, .34, .1), 'Leaf', taper=1.8, axes=((1, 0, 0), (0, -1, 0), (0, 0, 1)))
    for x in (1.4, 2.0, 2.6): sack(m, (x, 0, -1.4), color='Sack' if x != 2.0 else 'SackDark')
    for (x, y, z), col in zip(OFFERS, ('Tomato', 'Carrot', 'Cabbage')):
        basket(m, (x, y, z), r=.20); produce(m, (x, y+.08, z), col, n=7, r=.05, spread=.12)
    return m, OFFERS

# ------------------------------------------------------------------ mine

def mine_front():
    m = k.Mesh('MineFront', pal)
    for a, (x, y, z, w, h, d) in enumerate([(-2.6, 0, -1.8, 2.2, 4.6, 1.9), (2.6, 0, -1.9, 2.3, 4.2, 2.0), (0, 2.6, -2.6, 2.6, 3.6, 2.2),
                                             (-1.2, 3.4, -2.2, 1.8, 2.6, 1.6), (1.4, 3.1, -2.4, 1.9, 2.8, 1.7), (-3.8, 0, -1.2, 1.2, 2.2, 1.2), (3.9, 0, -1.0, 1.0, 1.8, 1.1)]):
        rock(m, (x, y, z), w, h, d, 'Rock', seed=a)
    box(m, (0, 1.45, -.55), (2.3, 2.9, .3), 'Coal')
    for x in (-1.28, 1.28): box(m, (x, 1.5, -.25), (.24, 3.0, .24), 'Timber')
    box(m, (0, 3.08, -.25), (3.0, .26, .3), 'Timber')
    for s_ in (-1, 1): diagonal(m, (s_*1.0, 2.75, -.12), .7, -s_*.75, .12, 'Timber')
    for a in range(5): box(m, (-.9+a*.45, 3.42, -.2), (.42, .4, .06), 'PlankDark' if a % 2 else 'Plank')
    # Rails out of the mouth, with an ore cart.
    for x in (-.45, .45): box(m, (x, .06, .9), (.06, .06, 3.2), 'Iron')
    for a in range(8): box(m, (0, .02, -.5+a*.42), (1.2, .05, .14), 'Timber')
    box(m, (0, .58, 1.4), (1.0, .55, 1.3), 'PlankDark')
    for dz in (-.6, .6): box(m, (0, .58, 1.4+dz), (1.04, .58, .06), 'Iron')
    for x in (-.5, .5):
        for z in (1.0, 1.8): wheel(m, (x, .2, z), .17)
    for a, col in enumerate(('Ore', 'Copper', 'Coal', 'Ore', 'Copper')):
        rock(m, (-.3+a*.15, .84, 1.25+.1*(a % 3)), .16, .2, .14, col, seed=a+9)
    # Counter booth on the right with a lean-to roof, lanterns, crates and the pick sign.
    box(m, (3.0, 1.0, .2), (2.0, 2.0, 1.4), 'Plank'); box(m, (3.0, 1.45, .92), (1.4, .7, .05), 'GlassDark')
    for x in (2.05, 3.95): box(m, (x, 1.3, .92), (.12, 2.6, .12), 'Timber')
    slab(m, (1.9, 2.7), (4.1, 2.3), 1.3, -.6, .1, 'PlankDark', 'Timber')
    for x in (-1.75, 1.75):
        lantern(m, (x, 2.4, .05)); box(m, (x, 2.75, -.08), (.04, .3, .04), 'Iron')
    crate(m, (-2.6, 0, .9)); crate(m, (-2.25, .45, .85), (.5, .4, .4), turn=.3); barrel(m, (-3.2, 0, .5), color='PlankDark')
    pick_icon(m, hanging_sign(m, 2.4, 3.6, .3, reach=.7))
    return m

def mine_counter():
    m = k.Mesh('MineCounter', pal); room(m, wall='Rock', timber='Timber', ceiling='RockDark'); counter(m, top='PlankDark')
    for a in range(4):
        x = -3.0+a*.62; crate(m, (x, 0, -1.55), (.56, .55, .5), 'PlankDark')
        for b in range(4): rock(m, (x-.15+b*.1, .52, -1.55+.06*(b % 2)), .1, .12, .09, ('Ore', 'Copper', 'Coal', 'Ore')[(a+b) % 4], seed=a*4+b)
    for x in (1.2, 1.8, 2.4):
        diagonal(m, (x, 1.7, -1.86), .9, .3, .04, 'TimberLight')
        box(m, (x+.13, 2.12, -1.84), (.34, .05, .04), 'Steel', taper=.4)
    lantern(m, (-1.2, 2.4, -1.65)); lantern(m, (2.9, 1.6, -1.5))
    # Offers: three ore trays; a balance scale stands near the keeper.
    for (x, y, z), col in zip(OFFERS, ('Ore', 'Copper', 'Coal')):
        box(m, (x, y+.03, z), (.42, .06, .32), 'Iron')
        for b in range(4): rock(m, (x-.1+b*.07, y+.06, z+.04*(b % 2)), .07, .08, .06, col, seed=b+3)
    box(m, (-.55, T+.12, .45), (.03, .24, .03), 'Brass'); box(m, (-.55, T+.24, .45), (.36, .02, .02), 'Brass')
    for dx in (-.17, .17): box(m, (-.55+dx, T+.16, .45), (.10, .02, .10), 'Brass')
    return m, OFFERS

# ------------------------------------------------------------------ fishery

def fishery_front():
    m = k.Mesh('FisheryFront', pal)
    box(m, (0, -.28, -2.2), (8.0, .06, 6.4), 'Water')
    for x in (-2.7, -.9, .9, 2.7):
        for z in (.7, -1.5, -4.0): box(m, (x, .2, z), (.18, 1.2, .18), 'PostDark')
    for a in range(16): box(m, (-3.0+a*.4, .82, -1.6), (.38, .07, 5.2), 'Plank' if a % 3 else 'PlankDark')
    for a in range(3): box(m, (2.0, .2+a*.22, .95+a*.12), (1.0, .06, .3), 'Plank')
    # Shack: navy plank walls and a single-slope roof falling to the back.
    prism(m, [(-2.6, .85), (2.6, .85), (2.6, 3.6), (-2.6, 3.6)], -.6, -3.8, 'Navy')
    slab(m, (-2.9, 3.9), (2.9, 3.9), .1, -4.2, .12, 'NavyDark', 'PostDark')
    box(m, (0, 3.8, -.55), (5.4, .12, .12), 'BuoyWhite')
    door(m, -1.9, -1.0, .86, 2.9, -.57, color='Navy')
    window(m, .2, 1.4, 1.6, 2.6, -.56, panes=(2, 2)); shutters(m, .2, 1.4, 1.6, 2.6, -.58, 'BuoyWhite')
    # Nets on the wall, buoys, crossed oars, a barrel and ice crates of fish on the deck.
    for a in range(7): box(m, (1.0+a*.12, 2.2-.03*a, -.52), (.02, 1.3-.06*a, .02), 'Net')
    for a in range(6): box(m, (1.36, 1.6+a*.2, -.52), (.9, .02, .02), 'Net')
    for x, col in ((-2.3, 'BuoyRed'), (-2.0, 'BuoyWhite'), (2.35, 'BuoyRed')):
        k.vertical_loft(m, [(2.5, .08, .08, .08), (2.65, .12, .12, .12), (2.8, .08, .08, .08)], col, a=.6, b=.5, x=x, zc=-.45)
        box(m, (x, 2.95, -.48), (.02, .3, .02), 'Net')
    for s_ in (-1, 1): diagonal(m, (-.55, 2.9, -.5), 1.5, s_*.6, .06, 'TimberLight')
    barrel(m, (-2.3, .86, .3), color='PlankDark')
    for x in (-.8, .3):
        box(m, (x, 1.05, .45), (.8, .4, .55), 'Plank'); box(m, (x, 1.26, .45), (.72, .03, .47), 'Ice')
        for b in range(3): fish(m, (x-.2+b*.2, 1.31, .45+.08*(b % 2)), .3, turn=.4*(b-1))
    for x in (-2.8, 2.8): box(m, (x, 1.35, .9), (.08, 1.0, .08), 'PostDark')
    box(m, (0, 1.75, .9), (5.6, .06, .04), 'Net')
    fish_icon(m, hanging_sign(m, 1.7, 3.5, 0.0, reach=.7))
    return m

def fishery_counter():
    m = k.Mesh('FisheryCounter', pal); room(m, wall='Navy', timber='PostDark', ceiling='NavyDark'); counter(m, top='Plank')
    for a in range(8): box(m, (-2.9+a*.13, 2.0, -1.88), (.02, 1.6, .02), 'Net')
    for a in range(7): box(m, (-2.45, 1.3+a*.22, -1.88), (1.0, .02, .02), 'Net')
    for x, col in ((1.0, 'BuoyRed'), (1.5, 'BuoyWhite'), (2.0, 'BuoyRed')):
        k.vertical_loft(m, [(2.2, .1, .1, .1), (2.38, .14, .14, .14), (2.56, .1, .1, .1)], col, a=.6, b=.5, x=x, zc=-1.75)
    for s_ in (-1, 1): diagonal(m, (2.7, 1.8, -1.85), 1.8, s_*.5, .06, 'TimberLight')
    barrel(m, (-2.9, 0, -1.2), color='PlankDark'); barrel(m, (2.9, 0, -1.0), color='PlankDark')
    # Offers: two ice trays of fish (the ordinary fish slots) and a lobster trap for the exclusive seafood slot.
    for x in (OFFERS[0][0], OFFERS[1][0]):
        box(m, (x, T+.03, .48), (.50, .06, .36), 'Steel'); box(m, (x, T+.065, .48), (.46, .02, .32), 'Ice')
        for b in range(2): fish(m, (x-.08+b*.16, T+.1, .48), .30, turn=.3*(1-2*b))
    x = OFFERS[2][0]
    for dy in (.02, .26): box(m, (x, T+dy, .48), (.42, .03, .32), 'TimberLight')
    for dx in (-.19, .19):
        for dz in (-.14, .14): box(m, (x+dx, T+.14, .48+dz), (.03, .26, .03), 'TimberLight')
    for a in range(5): box(m, (x-.16+a*.08, T+.14, .64), (.01, .24, .01), 'Net')
    return m, OFFERS

# ------------------------------------------------------------------ tailor-material shop

def tailor_front():
    m = k.Mesh('TailorFront', pal)
    w = 2.5
    box(m, (0, .2, -2.0), (5.2, .4, 4.2), 'StoneDark')
    box(m, (0, 1.8, -2.0), (5.0, 2.8, 4.0), 'Plaster')
    box(m, (0, 3.25, .02), (5.3, .14, .32), 'PlumDark')
    box(m, (0, 4.75, -1.9), (5.3, 2.9, 4.0), 'Plaster')
    timber_frame(m, -w-.15, w+.15, 3.35, 6.2, .12, [-w-.12, -.9, .9, w+.12], braces=(1,))
    box(m, (0, 6.25, .1), (5.5, .16, .3), 'PlumDark')
    prism(m, [(-2.7, 6.3), (2.7, 6.3), (0, 9.0)], .12, -3.9, 'Plaster')
    roof(m, [(-2.7, 6.3), (0, 9.0), (2.7, 6.3)], .12, -3.9, 'RoofDark', 'PlumDark', thick=.15, overhang=.3)
    window(m, -.45, .45, 7.0, 7.9, .16, panes=(2, 2))
    # Bay display window with a mannequin and fabric bolts.
    box(m, (-.9, .65, .25), (2.6, .5, .55), 'PlumDark')
    for x in (-2.2, .4): box(m, (x, 1.95, .5), (.12, 2.1, .12), 'Plum')
    box(m, (-.9, 3.05, .5), (2.7, .14, .2), 'Plum'); box(m, (-.9, .92, .5), (2.7, .08, .25), 'Plum')
    box(m, (-.9, 2.0, -.05), (2.4, 2.0, .05), 'PlumDark')
    k.vertical_loft(m, [(1.35, .16, .1, .1), (1.75, .2, .12, .12), (2.15, .17, .11, .1), (2.35, .07, .06, .06)], 'FabricGold', a=.7, b=.6, x=-1.5, zc=.15)
    box(m, (-1.5, 1.15, .15), (.04, .45, .04), 'Iron')
    for a, col in enumerate(('FabricRed', 'FabricBlue', 'FabricGreen')): bolt(m, (-.45, 1.05+a*.15, .2), .8, .065, col)
    door(m, 1.0, 1.9, .4, 2.75, .03, color='Plum')
    for wx in (-1.6, 1.6):
        window(m, wx-.4, wx+.4, 3.9, 5.1, .18, panes=(2, 3))
        box(m, (wx, 3.85, .36), (1.0, .2, .25), 'PlumDark')
        for a in range(4): k.vertical_loft(m, [(3.95, .05, .05, .05), (4.05, .07, .07, .07)], 'Flower' if a % 2 else 'Leaf', a=.6, b=.5, x=wx-.33+a*.22, zc=.36)
    awning(m, -2.3, .5, 3.15, .8, colors=('Plum', 'Trim'), z=.5, stripes=7)
    spool_icon(m, hanging_sign(m, 2.1, 3.0, .1, reach=.6))
    return m

def tailor_counter():
    m = k.Mesh('TailorCounter', pal); room(m, wall='Plaster', timber='Plum'); counter(m, top='Plum', front='PlumDark', seams='Plum')
    cols = ('FabricRed', 'FabricBlue', 'FabricGold', 'FabricGreen', 'Trim', 'Plum')
    for r_, y in enumerate((.9, 1.4, 1.9, 2.4)):
        shelf(m, -3.2, -.4, y, color='PlankDark')
        for a in range(3): bolt(m, (-2.75+a*.85, y+.08, -1.70), .75, .07, cols[(a+r_*2) % 6])
    k.vertical_loft(m, [(.95, .17, .1, .1), (1.35, .21, .13, .13), (1.75, .18, .11, .1), (1.95, .07, .06, .06)], 'Trim', a=.7, b=.6, x=2.4, zc=-1.2)
    box(m, (2.4, .48, -1.2), (.04, .95, .04), 'Iron'); box(m, (2.4, .02, -1.2), (.4, .04, .4), 'Iron')
    shelf(m, .9, 2.0, 1.68, color='PlankDark')
    for a in range(6): k.vertical_loft(m, [(1.71, .04, .04, .04), (1.85, .04, .04, .04)], cols[a], a=.6, b=.5, x=1.1+a*.16, zc=-1.72)
    box(m, (-.45, T+.01, .5), (.9, .01, .04), 'Wheat')
    for (x, y, z), col in zip(OFFERS, ('FabricRed', 'FabricBlue', 'FabricGold')):
        for b in range(4): box(m, (x, y+.03+b*.05, z), (.36-.01*b, .045, .28), col if b % 2 == 0 else 'Trim')
    return m, OFFERS

# ------------------------------------------------------------------ warehouse / trading house

def warehouse_front():
    m = k.Mesh('WarehouseFront', pal)
    W2 = 8.6
    box(m, (0, 1.95, -3.0), (W2, 3.3, 6.0), 'Stone')
    box(m, (0, .15, -3.0), (W2+.2, .3, 6.1), 'StoneDark')
    box(m, (0, 5.0, -3.0), (W2, 2.8, 6.0), 'PlankDark')
    for a in range(18): box(m, (-W2/2+.24+a*.48, 5.0, .02), (.06, 2.8, .04), 'Timber')
    for y in (3.62, 6.42): box(m, (0, y, .06), (W2+.2, .2, .2), 'Timber')
    prism(m, [(-W2/2, 6.4), (W2/2, 6.4), (0, 8.6)], 0, -6.0, 'PlankDark')
    roof(m, [(-W2/2, 6.4), (0, 8.6), (W2/2, 6.4)], 0, -6.0, 'RoofRed', 'RoofDark', thick=.16, overhang=.4)
    # Double barn doors with iron straps, a side door and the loading door with a hoist and hanging crate.
    for x0, x1 in ((-1.7, 0), (0, 1.7)):
        cx = (x0+x1)/2
        for a in range(4): box(m, (x0+(x1-x0)*(a+.5)/4, 1.75, .03), ((x1-x0)/4-.01, 2.9, .06), 'Plank' if a % 2 else 'PlankDark')
        for y in (.8, 2.7): box(m, (cx, y, .08), (x1-x0-.1, .1, .03), 'Iron')
    box(m, (0, 3.3, .08), (3.6, .22, .14), 'Timber')
    for x in (-1.8, 1.8): box(m, (x, 1.75, .08), (.2, 3.1, .14), 'Timber')
    door(m, 2.6, 3.4, .3, 2.5, .03)
    box(m, (0, 4.95, .05), (1.6, 1.8, .06), 'Plank')
    for x in (-.8, .8): box(m, (x, 4.95, .09), (.12, 1.9, .06), 'Timber')
    box(m, (0, 7.15, .6), (.2, .2, 1.4), 'Timber'); box(m, (0, 7.0, 1.2), (.08, .2, .2), 'Iron')
    box(m, (0, 5.95, 1.2), (.03, 1.9, .03), 'Hay'); crate(m, (0, 4.55, 1.2), (.6, .5, .5))
    for x in (-3.2, 3.2): window(m, x-.5, x+.5, 4.3, 5.5, .06, panes=(2, 2))
    # Stacked crates and barrels, sacks, a hand cart and the brass-scale sign.
    for x, y, z, t in ((-3.6, 0, .7, 0), (-3.0, 0, .9, .2), (-3.35, .45, .8, -.1), (3.3, 0, .9, .1)): crate(m, (x, y, z), (.6, .45, .5), turn=t)
    barrel(m, (-2.25, 0, .8)); barrel(m, (3.95, 0, .6)); sack(m, (2.55, 0, .9)); sack(m, (2.95, 0, 1.05), color='SackDark')
    box(m, (-1.0, .55, 1.4), (1.0, .08, .7), 'Plank')
    for x in (-1.5, -.5): wheel(m, (x, .25, 1.4), .25, color='TimberLight')
    scale_icon(m, hanging_sign(m, -3.3, 3.2, .1, reach=.8))
    return m

def warehouse_counter():
    m = k.Mesh('WarehouseCounter', pal); room(m, wall='Plank', timber='Timber'); counter(m, top='PlankDark')
    for y in (.95, 1.75, 2.55):
        shelf(m, -3.2, 3.2, y, depth=.45, color='PlankDark')
        for a in range(8): crate(m, (-2.9+a*.82, y+.03, -1.70), (.5, .4, .36), 'Plank' if a % 2 else 'TimberLight', turn=.05*(a % 3-1))
    barrel(m, (-2.95, 0, -.9)); barrel(m, (2.95, 0, -.7)); sack(m, (2.4, 0, -1.2))
    # Ledger and inkwell by the keeper; the offer anchors are the two deposit boxes and the gold scale.
    box(m, (-.45, T+.02, .45), (.42, .04, .30), 'Ledger'); box(m, (-.45, T+.045, .45), (.38, .01, .27), 'Paper')
    box(m, (.15, T+.04, .40), (.06, .08, .06), 'Coal')
    x = OFFERS[2][0]
    box(m, (x, T+.02, .48), (.22, .04, .18), 'Brass'); box(m, (x, T+.17, .48), (.03, .3, .03), 'Gold'); box(m, (x, T+.32, .48), (.40, .02, .02), 'Gold')
    for dx in (-.19, .19):
        box(m, (x+dx, T+.22, .48), (.12, .02, .12), 'Gold'); box(m, (x+dx, T+.27, .48), (.01, .1, .01), 'Gold')
    return m, OFFERS

# ------------------------------------------------------------------ tier 2 looks (Milestone 14): the birch tier

# One wood per tier (pine to ebony): tier 2 is birch. Each <Shop>FrontT2 is the tier 1 build (same massing, sign icon
# and goods) retargeted to pal2, which keeps every tier 1 slot in order and recolours the wood, roof, plaster and
# awning slots (pale birch timber, cool slate and reed roofs, limewash, oilskin awnings), plus the shop's additions:
# birch-bark posts with black lenticels, copper fittings and oilskin canopies. pal2 has extra slots, so the tier 2 fronts
# use their own strip (StorefrontPaletteT2.png) and the tier 1 OBJs and palette stay byte-identical.
T2_RECOLOURS = [('Timber', '#CBB48C'), ('TimberLight', '#E0CFAA'), ('Plank', '#D9C6A0'), ('PlankDark', '#BCA37B'), ('PostDark', '#A68E68'),
                ('Plaster', '#DADCD3'), ('RoofRed', '#5E6F74'), ('RoofDark', '#47565C'), ('Thatch', '#A49C76'), ('ThatchDark', '#7D7759'),
                ('AwningRed', '#2E6A6D'), ('AwningCream', '#DCD2B4'), ('FabricGreen', '#C08F34')]
T2_COLOURS = [('BirchBark', '#ECE8DE', 'bark'), ('Lenticel', '#2A2724'), ('CopperFit', '#C27842', 'metal'), ('Verdigris', '#6FA092', 'metal'),
              ('OilTeal', '#2E6A6D', 'cloth'), ('OilSlate', '#55646C', 'cloth'), ('OilOchre', '#C29032', 'cloth'), ('Flax', '#D8CEB0', 'cloth')]

def tier2_palette(base):
    p = copy.deepcopy(base)
    for name, color in T2_RECOLOURS: p.colors[p.names.index(name)] = tuple(int(color[i:i+2], 16) for i in (1, 3, 5))
    for name, color, *detail in T2_COLOURS: p.add(name, color, *detail)
    return p

pal2 = tier2_palette(pal)

def bark_post(m, c, size, dashes=None):
    """Birch-bark sleeve over a post (centre c, post size): white bark with black lenticel dashes on the street face."""
    w, h, d = size; n = dashes or max(1, int(h/.42)); box(m, c, (w+.03, h, d+.03), 'BirchBark')
    for a in range(n):
        box(m, (c[0]+(.025 if a % 2 else -.02), c[1]-h/2+h*(a+.5)/n, c[2]+d/2+.018), (w*(.62 if a % 3 else .45), .028, .012), 'Lenticel')

def vane(m, x, y, z):
    """Copper weathervane: rod, arrow and tail fin turned along the street."""
    box(m, (x, y+.35, z), (.035, .7, .035), 'CopperFit'); box(m, (x, y+.62, z), (.62, .035, .035), 'CopperFit')
    box(m, (x+.34, y+.62, z), (.12, .1, .02), 'CopperFit', taper=.2); box(m, (x-.28, y+.62, z), (.14, .16, .02), 'Verdigris')

def warehouse_t2(m):
    W2 = 8.6
    box(m, (0, 8.68, -3.0), (.3, .12, 6.8), 'CopperFit'); vane(m, 0, 8.72, -3.0)
    for x in (-1.8, 1.8): bark_post(m, (x, 1.75, .08), (.2, 3.1, .14), 5)
    for x0, x1 in ((-1.7, 0), (0, 1.7)):
        for y in (.8, 2.7): box(m, (x0+.18 if x0 < 0 else x1-.18, y, .11), (.26, .16, .03), 'CopperFit')
    for x in (-.14, .14): box(m, (x, 1.75, .12), (.05, .32, .05), 'CopperFit')
    awning(m, 2.35, 3.65, 2.95, .8, colors=('OilSlate', 'Flax'), z=.05, stripes=4)
    box(m, (0, 6.42, .19), (W2+.2, .05, .05), 'CopperFit')
    return m

def bakery_t2(m):
    R = gs.RIDGE
    box(m, (0, R+.15, -2.2), (.22, .08, 5.6), 'CopperFit')
    box(m, (2.0, R+.56, -2.9), (.92, .06, .92), 'CopperFit'); box(m, (2.0, R+.66, -2.9), (.5, .14, .5), 'Verdigris', taper=.4)
    box(m, (1.1, 3.0, .38), (1.45, .05, .55), 'CopperFit', axes=((1, 0, 0), (0, 1, .35), (0, -.35, 1)))
    for x in (.45, 1.75): box(m, (x, 2.86, .5), (.03, .3, .03), 'CopperFit', axes=((1, 0, 0), (0, 1, -.9), (0, .9, 1)))
    for x in (-gs.W/2-.02, gs.W/2+.02): bark_post(m, (x, (gs.GROUND+gs.FLOOR1)/2, .05), (.16, gs.FLOOR1-gs.GROUND, .14))
    return m

def ranch_t2(m):
    box(m, (0, 6.5, -2.5), (.26, .12, 5.8), 'CopperFit'); vane(m, 0, 6.55, -2.5)
    for x in (-3.28, 3.28): bark_post(m, (x, 1.85, .02), (.18, 3.1, .16), 3)
    for x0, x1 in ((-1.55, 0), (0, 1.55)):
        for y in (.9, 2.45): box(m, (x0+.22 if x0 < 0 else x1-.22, y, .16), (.32, .1, .03), 'CopperFit')
    awning(m, -.8, .8, 5.38, .5, colors=('OilOchre', 'Flax'), z=.1, stripes=3)
    return m

def farm_t2(m):
    for x in (-3.0, -1.0, 1.0, 3.0): bark_post(m, (x, 1.55, .85), (.14, 2.9, .14), 5)
    vane(m, 0, 5.75, -2.0)
    for x in (-2.75, 2.75): box(m, (x, .9, .72), (.16, .06, .06), 'CopperFit')
    box(m, (0, .9, .76), (5.8, .04, .03), 'CopperFit')
    return m

def fishery_t2(m):
    awning(m, .15, 2.45, 3.4, .95, colors=('OilOchre', 'Flax'), z=-.55, stripes=6)
    for x in (-2.7, -.9, .9, 2.7): bark_post(m, (x, .2, .7), (.18, 1.2, .18), 2)
    for x in (-2.8, 2.8): bark_post(m, (x, 1.35, .9), (.08, 1.0, .08), 2)
    for t, rr in ((.12, .9), (.88, .9)): ring_band(m, (-2.3, .86, .3), .86+.8*t, .28*rr+.014, 'CopperFit')
    box(m, (0, 3.8, -.47), (5.4, .05, .05), 'CopperFit')
    return m

def mine_t2(m):
    for x in (-1.28, 1.28): bark_post(m, (x, 1.5, -.25), (.24, 3.0, .24), 6)
    slab(m, (1.9, 2.73), (4.1, 2.33), 1.32, -.62, .03, 'CopperFit', 'Verdigris')
    for x in (-1.75, 1.75): box(m, (x, 2.6, .05), (.28, .05, .28), 'CopperFit', taper=.5)
    box(m, (-2.45, .93, .88), (1.1, .04, .62), 'OilTeal'); box(m, (-2.45, .72, 1.18), (1.1, .4, .03), 'OilTeal')
    return m

def tailor_t2(m):
    box(m, (0, 9.05, -1.89), (.2, .1, 4.6), 'CopperFit'); box(m, (0, 9.3, .14), (.05, .5, .05), 'CopperFit')
    box(m, (0, 9.6, .14), (.14, .14, .14), 'Verdigris', taper=.3)
    box(m, (-.9, 3.14, .52), (2.75, .05, .24), 'CopperFit')
    for wx in (-1.6, 1.6): box(m, (wx, 3.96, .5), (1.02, .03, .03), 'CopperFit')
    box(m, (1.45, 2.88, .12), (1.05, .05, .25), 'CopperFit')
    return m

def tier2_front(front_fn, extras):
    m = front_fn(); m.name += 'T2'; m.palette = pal2   # same slot indices: pal2 extends pal
    return extras(m)

TIER2 = {'Warehouse': warehouse_t2, 'Bakery': bakery_t2, 'Ranch': ranch_t2, 'Farm': farm_t2, 'Fishery': fishery_t2, 'Mine': mine_t2, 'Tailor': tailor_t2}

CRATE = (.46, .34, .36)   # deposit crate size (x, y, z); the lid is a separate part that hinges on its back top edge

def deposit_crate_parts():
    """Body (origin at the floor centre) and Lid (origin at the back hinge edge, lid extends toward +Z)."""
    body = k.Mesh('DepositCrateBody', pal); w, h, d = CRATE
    # Pale wood with brass bands and corner edges, so the two interactable crates stand out from the dark stock behind (user, Nov 18).
    box(body, (0, h/2, 0), CRATE, 'Trim'); box(body, (0, .05, 0), (w+.012, .05, d+.012), 'Brass')
    box(body, (0, h-.03, 0), (w+.012, .04, d+.012), 'Brass')
    for sx in (-1, 1):
        for sz in (-1, 1): box(body, (sx*(w/2-.0115), h/2, sz*(d/2-.0115)), (.035, h, .035), 'Brass')   # outer faces flush with the bands
    lid = k.Mesh('DepositCrateLid', pal)
    box(lid, (0, .02, d/2), (w+.02, .04, d+.02), 'Trim'); box(lid, (0, .02, d/2), (w+.032, .05, .05), 'Brass')
    box(lid, (0, .02, d+.005), (w+.032, .05, .03), 'Brass')
    return body, lid

# Proposed street order between the two groups of houses (open decision; see the plan, section 13.4).
SHOPS = [
    ('Warehouse', warehouse_front, warehouse_counter, 'ShopkeeperTrader', 8.6, ['Deposit box', 'Deposit box', 'Gold scale']),
    ('Bakery', gs.bakery_front, gs.bakery_counter, 'ShopkeeperBaker', gs.W, ['Common cash', 'Premium gold', 'Premium gold']),
    ('Ranch', ranch_front, ranch_counter, 'ShopkeeperRancher', 6.6, ['Cash offer', 'Cash offer', 'Cash offer']),
    ('Farm', farm_front, farm_counter, 'ShopkeeperFarmer', 7.2, ['Cash offer', 'Cash offer', 'Cash offer']),
    ('Fishery', fishery_front, fishery_counter, 'ShopkeeperFisher', 6.0, ['Fish', 'Fish', 'Exclusive seafood']),
    ('Mine', mine_front, mine_counter, 'ShopkeeperMiner', 8.2, ['Cash offer', 'Cash offer', 'Cash offer']),
    ('Tailor', tailor_front, tailor_counter, 'ShopkeeperTailor', 5.4, ['Cash offer', 'Cash offer', 'Cash offer'])]

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    pal.write_png(OUT/'StorefrontPalette.png')
    shops, stats = [], []
    for name, front_fn, counter_fn, keeper, width, kinds in SHOPS:
        front = front_fn(); counter_mesh, offers = counter_fn()
        stats += [front.save(OUT, HEADER), counter_mesh.save(OUT, HEADER)]
        shops.append(dict(Name=name, Front=name+'Front', Counter=name+'Counter', Keeper=keeper, Width=width,
                          OfferAnchors=[list(o) for o in offers], OfferAnchorsFlat=[c for o in offers for c in o], OfferKinds=kinds))
    stats += [part.save(OUT, HEADER) for part in deposit_crate_parts()]
    pal2.write_png(OUT/'StorefrontPaletteT2.png')
    stats += [tier2_front(front_fn, TIER2[name]).save(OUT, HEADER) for name, front_fn, *_ in SHOPS]
    # Dedicated lumber authoring owns its doorway/ledger hierarchy and palette.
    # Preserve its registration when regenerating the seven older storefronts.
    existing=OUT/'StorefrontManifest.json'
    if existing.exists():
        prior=json.loads(existing.read_text(encoding='utf-8-sig'))
        shops += [row for row in prior.get('Shops',[]) if row.get('Name')=='Lumber']
        stats += [row for row in prior.get('Parts',[]) if row.get('name','').startswith('Lumber')]
    manifest = dict(Revision=2, Style='faceted', CounterTop=COUNTER_TOP, Shops=shops, Parts=stats,
                    Scope='Art only: street front and counter view per shop. No shop logic, stock, prices or selection.')
    (OUT/'StorefrontManifest.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
    print('Storefronts:', ', '.join(f"{s['name']} {s['triangles']}" for s in stats))

if __name__ == '__main__':
    main()
