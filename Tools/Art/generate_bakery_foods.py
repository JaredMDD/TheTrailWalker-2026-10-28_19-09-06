"""Five bakery foods for the Milestone 14 bakery (wheat bread, pine-nut oat cookies, strawberry cream roll, blueberry muffins,
flax-oil biscuits). Python 3 standard library only.

Same contract as generate_chicken_meat.py: local Y-up OBJ (front faces -Z), palette-strip UVs, detail TYPE in palette alpha,
one shared palette for the five. Sized against Berry oat cakes (largest side 0.72) and kept under Premium apple pie's triangles.
Run: python Tools/Art/generate_bakery_foods.py   (writes only the five OBJs and BakeryFoodsPalette.png)
"""
import sys, math
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
import generate_region01_meshes as base
import detail_textures as detail
from generate_region05_meshes import sphere
from tier01_food_misc_designs import _lathe

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Art/Items/Bakery'
PAL = OUT / 'BakeryFoodsPalette.png'
COLORS = [(128, 72, 36), (184, 112, 54), (218, 160, 88), (238, 212, 160),   # 0 dark crust, 1 golden crust, 2 light crust, 3 crumb
          (252, 238, 208), (198, 44, 54), (232, 206, 150), (200, 150, 86),   # 4 cream, 5 strawberry, 6 pine nut, 7 oat cookie
          (48, 56, 120), (250, 200, 80), (170, 84, 62), (140, 66, 50),       # 8 blueberry, 9 golden gloss, 10-11 muffin liner
          (236, 192, 112), (92, 58, 36), (222, 196, 140), (82, 92, 160)]     # 12 sponge, 13 flax seed, 14 oat flake, 15 berry bloom
MAX_SIDE, MAX_TRIS = (.72 * .8, .72 * 1.25), 1544 * 1.25   # Berry oat cakes' largest side, Premium apple pie's triangles


def closed(m, start, part):
    """Self-check: every face of a part winds outward like sphere() and _lathe() (normal away from the part's centre)."""
    faces = m.faces[start:]
    used = {v for f in faces for v in f[:3]}
    c = [sum(m.vertices[v - 1][n] for v in used) / len(used) for n in range(3)]
    for f in faces:
        a, b, d = (m.vertices[v - 1] for v in f[:3])
        u, w = [b[n] - a[n] for n in range(3)], [d[n] - a[n] for n in range(3)]
        normal = (u[1] * w[2] - u[2] * w[1], u[2] * w[0] - u[0] * w[2], u[0] * w[1] - u[1] * w[0])
        assert sum(normal[n] * (a[n] + b[n] + d[n] - 3 * c[n]) for n in range(3)) >= 0, part + ' has an inward face'


def turn(m, vstart, pivot, axis, angle):
    """Rotate vertices from vstart about a principal axis ('x', 'y' or 'z') through pivot."""
    c, s = math.cos(angle), math.sin(angle)
    i, j = {'x': (1, 2), 'y': (2, 0), 'z': (0, 1)}[axis]
    for n in range(vstart, len(m.vertices)):
        p = [m.vertices[n][k] - pivot[k] for k in range(3)]
        p[i], p[j] = p[i] * c - p[j] * s, p[i] * s + p[j] * c
        m.vertices[n] = tuple(p[k] + pivot[k] for k in range(3))


def loft_x(m, rows, sides, color, shape=lambda t: (math.cos(t), math.sin(t))):
    """Closed loft along X; rows are (x, half width z, height above y0, y0), zero width = pole. color(k, i, centre) per quad."""
    rings = []
    for x, w, h, y0 in rows:
        rings.append([m.v((x, y0, 0))] if w == 0 else
                     [m.v((x, y0 + h * shape(i * math.tau / sides)[1], w * shape(i * math.tau / sides)[0])) for i in range(sides)])
    for k, (a, b) in enumerate(zip(rings, rings[1:])):
        for i in range(sides):
            j = (i + 1) % sides
            corners = [a[0 if len(a) == 1 else i], b[0 if len(b) == 1 else j]]
            centre = tuple(sum(m.vertices[v - 1][n] for v in corners) / 2 for n in range(3))
            col = color(k, i, centre)
            if len(a) == 1: m.tri(a[0], b[i], b[j], col)
            elif len(b) == 1: m.tri(a[i], b[0], a[j], col)
            else: m.tri(a[i], b[i], b[j], col); m.tri(a[i], b[j], a[j], col)


def wheat_bread():
    """A scored batard: golden domed loaf, three diagonal slashes opened to pale crumb, a light flour dusting."""
    m = base.Model('WheatBread')
    L, W, H, sides = .36, .175, .245, 24
    bread = lambda t: (math.cos(t), math.sin(t) if math.sin(t) > 0 else .18 * math.sin(t))   # domed top, nearly flat base
    envelope = lambda u: max(0.0, 1 - abs(u) ** 2.6) ** .5
    rows = [(u * L, W * envelope(u), H * envelope(u) ** .7, -.07) for u in (-1 + 2 * n / 28 for n in range(29))]
    def top(x, z):
        e = envelope(x / L)
        return -.07 + H * e ** .7 * math.sqrt(max(0.0, 1 - (z / (W * e)) ** 2))
    def slash(centre):
        x, y, z = centre
        d, t = (x + .55 * z) / .17, (z - .55 * x + .55 * .17 * round((x + .55 * z) / .17)) / 1.14   # across, along the slash
        return y > .075 and abs(t) < .14 and abs(d - round(d)) < .22 * math.sqrt(1 - (t / .14) ** 2) and abs(round(d)) <= 1
    def crust(k, i, centre):
        if slash(centre): return 3
        up = math.sin((i + .5) * math.tau / sides)
        return 1 if up > .55 else 2 if up > -.2 else 0
    s = len(m.faces); loft_x(m, rows, sides, crust, bread); closed(m, s, 'loaf')
    # Open the slashes: sink their vertices a little so each score reads as a groove, not paint.
    for n, (x, y, z) in enumerate(m.vertices):
        if slash((x, y, z)): m.vertices[n] = (x, y - .018, z)
    for x, z in ((-.22, .02), (-.05, -.06), (.12, .05), (.25, -.03), (.0, .07)):
        s = len(m.faces); sphere(m, (x, top(x, z) + .002, z), (.03, .006, .022), 4, 6, 3); closed(m, s, 'flour')
    return m


def cookie(m, centre, r, top=7):
    s = len(m.faces)
    _lathe(m, [(0, -.024), (r * .9, -.024), (r, -.008), (r * .97, .014), (r * .82, .027), (0, .031)], top, 14, centre)
    closed(m, s, 'cookie')


def pine_nut_oat_cookies():
    """Three oat cookies stacked a little off true, a fourth leaning against the stack; toasted pine nuts on the top one."""
    m = base.Model('PineNutOatCookies')
    r = .19
    for n, (dx, dz) in enumerate(((0, 0), (.018, -.012), (-.012, .014))):
        cookie(m, (-.13 + dx, -.06 + n * .056, dz), r)
    lean = lambda v: turn(m, v, (.38, -.084, .02), 'z', -.38)   # left edge raised against the stack, right edge on the table
    v = len(m.vertices); cookie(m, (.20, -.06, .02), .18); lean(v)
    for i in range(5):   # oat flakes between the nuts on the top cookie
        a = i * 2.4 + .9; rr = .15 * math.sqrt((i + .5) / 5)
        s = len(m.faces); sphere(m, (-.14 + rr * math.cos(a), .087, rr * math.sin(a)), (.026, .006, .016), 14, 6, 3); closed(m, s, 'flake')
    for i in range(6):
        a = i * math.tau / 6 + .3
        s = len(m.faces); v = len(m.vertices)
        sphere(m, (-.13 + .085 * math.cos(a), .093, .085 * math.sin(a)), (.018, .015, .042), 6, 6, 3)
        turn(m, v, (-.13 + .085 * math.cos(a), .093, .085 * math.sin(a)), 'y', -a); closed(m, s, 'nut')
    s = len(m.faces); sphere(m, (-.13, .098, 0), (.018, .015, .042), 6, 6, 3); closed(m, s, 'nut')
    for x, z in ((.0, -.06), (.05, .05), (-.05, .0)):   # nuts on the leaning cookie, rotated with it
        v = len(m.vertices); s = len(m.faces)
        sphere(m, (.20 + x, -.017, .02 + z), (.018, .014, .04), 6, 6, 3); lean(v); closed(m, s, 'nut')
    return m


def spiral(r, a, R, turns=2.3):
    """Palette index on a Swiss-roll cut face: sponge, a thin strawberry line, then cream, winding out to a crust rim."""
    if r > .9 * R: return 1
    f = (r / R * turns - a / math.tau) % 1
    return 12 if f < .44 else 5 if f < .64 else 4


def roll_piece(m, x0, x1, R, spiral0, spiral1, sectors=24, rings=8):
    """Closed cylinder along X; a swirled end is a polar grid coloured by spiral(), a plain end a crust-coloured fan."""
    ends = []
    for x, sign, swirl in ((x0, -1, spiral0), (x1, 1, spiral1)):
        n = rings if swirl else 1
        centre = m.v((x, 0, 0))
        grid = [[m.v((x, R * j / n * math.sin(i * math.tau / sectors), R * j / n * math.cos(i * math.tau / sectors)))
                 for i in range(sectors)] for j in range(1, n + 1)]
        ends.append((centre, grid, sign, swirl))
    for centre, grid, sign, swirl in ends:
        n = len(grid)
        col = lambda j, i: spiral(R * (j + .5) / n, sign * (i + .5) * math.tau / sectors, R) if swirl else 1
        for i in range(sectors):
            k = (i + 1) % sectors
            tri = (centre, grid[0][k], grid[0][i]) if sign > 0 else (centre, grid[0][i], grid[0][k])
            m.tri(*tri, col(0, i))
            for j in range(n - 1):
                a, b, c, d = grid[j][i], grid[j][k], grid[j + 1][k], grid[j + 1][i]
                if sign > 0: m.tri(a, b, c, col(j + 1, i)); m.tri(a, c, d, col(j + 1, i))
                else: m.tri(a, c, b, col(j + 1, i)); m.tri(a, d, c, col(j + 1, i))
    o0, o1 = ends[0][1][-1], ends[1][1][-1]
    for i in range(sectors):
        k = (i + 1) % sectors
        col = 2 if i % 3 else 12
        m.tri(o0[i], o1[k], o0[k], col); m.tri(o0[i], o1[i], o1[k], col)


def strawberry_cream_roll():
    """The roll lying along X with its cut end swirled, one thick slice stood up beside it showing the red and cream spiral."""
    m = base.Model('StrawberryCreamRoll')
    R = .15
    s = len(m.faces); roll_piece(m, -.37, .03, R, False, True, 20, 7); closed(m, s, 'roll')
    v = len(m.vertices); s = len(m.faces)
    roll_piece(m, -.04, .04, R, True, True, 28, 11)
    turn(m, v, (0, 0, 0), 'y', -1.35)   # turn the slice so a cut face looks toward the front (-Z)
    for n in range(v, len(m.vertices)):
        x, y, z = m.vertices[n]; m.vertices[n] = (x + .22, y + .005, z - .04)
    closed(m, s, 'slice')
    for x in (-.27, -.13):   # cream dollops topped with a strawberry piece
        s = len(m.faces); sphere(m, (x, R + .012, 0), (.05, .034, .05), 4, 8, 3); closed(m, s, 'cream')
        s = len(m.faces); sphere(m, (x, R + .052, 0), (.026, .032, .024), 5, 8, 4); closed(m, s, 'berry')
    for x, z in ((-.33, .06), (-.2, -.08), (-.06, .07)):   # icing-sugar flecks
        s = len(m.faces); sphere(m, (x, math.sqrt(R * R - z * z) + .001, z), (.018, .006, .014), 4, 5, 3); closed(m, s, 'sugar')
    return m


def muffin(m, cx, cz, berries):
    s = len(m.faces)
    pleats, rows = 16, [(0, -.11), (.115, -.11), (.15, .035)]
    rings = [[m.v((cx + r * (1 if i % 2 else .94) * math.cos(i * math.tau / pleats), y, cz + r * (1 if i % 2 else .94) * math.sin(i * math.tau / pleats)))
              for i in range(pleats)] if r else [m.v((cx, y, cz))] for r, y in rows]
    for i in range(pleats):   # pleated paper case, base cap then wall
        j = (i + 1) % pleats
        m.tri(rings[0][0], rings[1][i], rings[1][j], 11)
        col = 10 if i % 2 else 11
        m.tri(rings[1][i], rings[2][j], rings[1][j], col); m.tri(rings[1][i], rings[2][i], rings[2][j], col)
    top = m.v((cx, .035, cz))
    for i in range(pleats):
        j = (i + 1) % pleats; m.tri(top, rings[2][j], rings[2][i], 11)
    closed(m, s, 'liner')
    s = len(m.faces); dome = [(0, .02), (.135, .02), (.175, .045), (.18, .075), (.16, .118), (.10, .152), (0, .165)]
    _lathe(m, dome, 2, 16, (cx, 0, cz))
    closed(m, s, 'dome')
    crown = dome[:2:-1]   # upper profile, centre outwards
    for n, (a, rr) in enumerate(berries):   # blueberries pressed into the dome, on its surface
        r = .175 * rr
        (r0, y0), (r1, y1) = next(p for p in zip(crown, crown[1:]) if p[1][0] >= r)
        y = y0 + (y1 - y0) * (r - r0) / (r1 - r0) - .006
        s = len(m.faces); sphere(m, (cx + r * math.cos(a), y, cz + r * math.sin(a)), (.024, .021, .024), 8 if n % 3 else 15, 6, 3); closed(m, s, 'berry')


def blueberry_muffins():
    """Two muffins side by side: domed golden tops dotted with blueberries in pleated red-brown paper cases."""
    m = base.Model('BlueberryMuffins')
    muffin(m, -.185, .01, [(i * 2.4 + .4, .2 + .7 * ((i * 5) % 7) / 6) for i in range(8)])
    muffin(m, .185, -.01, [(i * 2.4 + 1.3, .2 + .7 * ((i * 3) % 7) / 6) for i in range(8)])
    return m


def biscuit(m, centre, r):
    s = len(m.faces)
    _lathe(m, [(0, -.022), (r * .92, -.022), (r, -.006), (r * .98, .012), (r * .9, .02), (0, .022)], 2, 16, centre)
    closed(m, s, 'biscuit')
    s = len(m.faces)   # the oil glaze: a thin glossy cap just inside the rim
    _lathe(m, [(0, .016), (r * .86, .016), (r * .8, .027), (0, .03)], 9, 16, centre)
    closed(m, s, 'glaze')
    cx, cy, cz = centre
    for i in range(7):   # flax seeds on the glaze
        a = i * 2.4 + .7; rr = r * .62 * math.sqrt((i + .5) / 7)
        s = len(m.faces); v = len(m.vertices)
        sphere(m, (cx + rr * math.cos(a), cy + .03, cz + rr * math.sin(a)), (.013, .005, .007), 13, 5, 3)
        turn(m, v, (cx + rr * math.cos(a), cy + .03, cz + rr * math.sin(a)), 'y', a); closed(m, s, 'seed')


def flax_oil_biscuits():
    """Three flat round biscuits: two side by side, the third resting across them; golden oil gloss and flax seeds on top."""
    m = base.Model('FlaxOilBiscuits')
    for x, z in ((-.18, .03), (.18, .02)):
        biscuit(m, (x, 0, z), .165)
    biscuit(m, (0, .052, -.025), .165)
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
    for build in (wheat_bread, pine_nut_oat_cookies, strawberry_cream_roll, blueberry_muffins, flax_oil_biscuits):
        save(build())
    px = [(*rgb, detail.alpha('none')) for rgb in COLORS]   # no surface detail, like the tier 1 prepared foods
    detail.write_png(PAL, len(COLORS) * 4, 4, [[p for p in px for _ in range(4)] for _ in range(4)])


if __name__ == '__main__':
    main()
