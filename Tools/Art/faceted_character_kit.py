"""Faceted low-poly character kit (style study, November 2, 2026). Python standard library only.

Style goals (user references in Docs/Art/Characters/References, used for style only, not copied): flat planar
facets instead of spheres, boxy heads with a squared jaw, slab brows, small dark eyes and a wedge nose, chunky
angular hair clumps, natural slim proportions, and clothing built as separate shells (collars, cuffs, belts,
straps). Colour comes from a palette strip with several shades per colour; faces pick a shade so flat facets read
like painted planes. Coordinates: metres, +Y up, +Z forward; triangles face outward (counter-clockwise outside).
"""
import math
import struct
import zlib

def add(a, b): return (a[0]+b[0], a[1]+b[1], a[2]+b[2])
def sub(a, b): return (a[0]-b[0], a[1]-b[1], a[2]-b[2])
def mul(a, s): return (a[0]*s, a[1]*s, a[2]*s)
def dot(a, b): return a[0]*b[0]+a[1]*b[1]+a[2]*b[2]
def cross(a, b): return (a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0])
def length(a): return math.sqrt(dot(a, a))
def unit(a):
    n = length(a)
    return mul(a, 1/n) if n > 1e-12 else (0, 1, 0)
def lerp(a, b, t): return add(a, mul(sub(b, a), t))

SHADES = (0.90, 0.95, 1.0, 1.04, 1.08)

class Palette:
    """Named colours, each expanded into five shades on one horizontal strip texture."""
    def __init__(self):
        self.names, self.colors, self.details = [], [], []
    def add(self, name, hex_color, detail=None):
        """detail: surface type for the FacetedDetail shader (detail_textures.TYPES); None keeps a flat colour."""
        self.names.append(name); self.details.append(detail)
        self.colors.append(tuple(int(hex_color[i:i+2], 16) for i in (1, 3, 5)))
    def index(self, name, shade=2): return self.names.index(name)*len(SHADES)+shade
    @property
    def slots(self): return len(self.names)*len(SHADES)
    def write_png(self, path, cell=4):
        from detail_textures import alpha
        width, height = self.slots*cell, cell
        rows = []
        for _ in range(height):
            row = bytearray([0])
            for color, detail in zip(self.colors, self.details):
                a = alpha(detail)
                for s in SHADES:
                    px = bytes([max(0, min(255, round(c*s))) for c in color] + [a])
                    row += px*cell
            rows.append(bytes(row))
        raw = b''.join(rows)
        def chunk(tag, data): return struct.pack('>I', len(data))+tag+data+struct.pack('>I', zlib.crc32(tag+data) & 0xffffffff)
        png = b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0))
        png += chunk(b'IDAT', zlib.compress(raw, 9))+chunk(b'IEND', b'')
        path.write_bytes(png)

class Mesh:
    def __init__(self, name, palette):
        self.name, self.palette, self.vertices, self.faces = name, palette, [], []
    def v(self, p):
        self.vertices.append(tuple(p)); return len(self.vertices)-1
    def tri(self, a, b, c, color, inside=None, shade=None):
        """color: palette name. inside: a point the face must point away from. shade: None picks one per face."""
        pa, pb, pc = self.vertices[a], self.vertices[b], self.vertices[c]
        n = cross(sub(pb, pa), sub(pc, pa))
        if length(n) < 1e-14: return
        if inside is not None:
            centroid = mul(add(add(pa, pb), pc), 1/3)
            if dot(n, sub(centroid, inside)) < 0: b, c = c, b
        if shade is None: shade = self.pick_shade(a, c)
        self.faces.append((a, b, c, self.palette.index(color, shade)))
    def pick_shade(self, a, c):
        # Deterministic, subtle facet variation; most planes stay at the base shade.
        h = (len(self.faces)*2654435761 + a*97 + c*31) % 100
        return 2 if h < 58 else 1 if h < 78 else 3
    def quad(self, a, b, c, d, color, inside=None, shade=None):
        # Both halves of a quad share one shade so planes read as single facets, not diagonal stripes.
        if shade is None: shade = self.pick_shade(a, c)
        self.tri(a, b, c, color, inside, shade); self.tri(a, c, d, color, inside, shade)
    def merge(self, other):
        base = len(self.vertices); self.vertices += other.vertices
        self.faces += [(a+base, b+base, c+base, k) for a, b, c, k in other.faces]
    def transform(self, fn, start=0):
        for i in range(start, len(self.vertices)): self.vertices[i] = fn(self.vertices[i])
    @property
    def triangles(self): return len(self.faces)
    def save(self, folder, header):
        lines = ['# '+header, '# Faceted style study: flat normals (s off), palette-strip UVs.', 'o '+self.name]
        lines += ['v %.6f %.6f %.6f' % p for p in self.vertices]
        n = self.palette.slots
        lines += ['vt %.6f 0.5' % ((i+.5)/n) for i in range(n)]
        lines += ['s off']
        lines += ['f %d/%d %d/%d %d/%d' % (a+1, k+1, b+1, k+1, c+1, k+1) for a, b, c, k in self.faces]
        (folder/(self.name+'.obj')).write_text('\n'.join(lines)+'\n', encoding='utf-8')
        assert all(all(math.isfinite(c) for c in p) for p in self.vertices), self.name
        return {'name': self.name, 'vertices': len(self.vertices), 'triangles': len(self.faces)}

# ------------------------------------------------------------------ section shapes

def chamfer_rect(w, front, back, a=.62, b=.55):
    """Eight-point angular section in a local (side, forward) plane: flat front/back planes with cut corners."""
    return [(w*a, front), (w, front*b), (w, -back*b), (w*a, -back),
            (-w*a, -back), (-w, -back*b), (-w, front*b), (-w*a, front)]

def hexagon(rx, rz, turn=0.0):
    return [(rx*math.cos(turn+i*math.pi/3), rz*math.sin(turn+i*math.pi/3)) for i in range(6)]

def ring_points(center, side, forward, shape):
    return [add(center, add(mul(side, s), mul(forward, f))) for s, f in shape]

def loft(mesh, rings, color, cap_start=True, cap_end=True, colors=None):
    """rings: list of (center, [points]); every ring has the same point count. colors: optional per-band names."""
    ids = [[mesh.v(p) for p in pts] for _, pts in rings]
    n = len(ids[0])
    for r in range(len(ids)-1):
        inside = lerp(rings[r][0], rings[r+1][0], .5)
        band = colors[r] if colors else color
        for i in range(n):
            j = (i+1) % n
            c = band(i) if callable(band) else band
            mesh.quad(ids[r][i], ids[r][j], ids[r+1][j], ids[r+1][i], c, inside)
    for cap, r, other in ((cap_start, 0, 1), (cap_end, len(ids)-1, len(ids)-2)):
        if not cap: continue
        center = rings[r][0]
        inside = lerp(center, rings[other][0], .5)
        c = mesh.v(center)
        for i in range(n):
            mesh.tri(c, ids[r][i], ids[r][(i+1) % n], color, inside)
    return ids

def vertical_loft(mesh, sections, color, a=.62, b=.55, zc=0.0, x=0.0, cap_start=True, cap_end=True, colors=None):
    """sections: (y, half width, front depth, back depth[, z offset]) stacked bottom to top."""
    rings = []
    for s in sections:
        y, w, f, bk = s[:4]; z = s[4] if len(s) > 4 else zc
        rings.append(((x, y, z), ring_points((x, y, z), (1, 0, 0), (0, 0, 1), chamfer_rect(w, f, bk, a, b))))
    return loft(mesh, rings, color, cap_start, cap_end, colors)

def limb(mesh, joints, color, sides=6, turn=math.pi/6, forward_hint=(0, 0, 1), colors=None, cap_start=True, cap_end=True):
    """joints: (point, rx, rz) along a path; rx is across (side), rz is front-back thickness."""
    rings = []
    for i, (p, rx, rz) in enumerate(joints):
        a = joints[max(0, i-1)][0]; b = joints[min(len(joints)-1, i+1)][0]
        axis = unit(sub(b, a))
        fwd = unit(sub(forward_hint, mul(axis, dot(forward_hint, axis))))
        side = cross(axis, fwd)
        shape = [(rx*math.cos(turn+k*2*math.pi/sides), rz*math.sin(turn+k*2*math.pi/sides)) for k in range(sides)]
        rings.append((p, ring_points(p, side, fwd, shape)))
    return loft(mesh, rings, color, cap_start, cap_end, colors)

def box(mesh, center, size, color, axes=((1, 0, 0), (0, 1, 0), (0, 0, 1)), taper=1.0, shade=None):
    """Oriented box; taper scales the +Y end (axes[1]) for wedges and slabs."""
    sx, sy, sz = (s/2 for s in size)
    ax, ay, az = axes
    pts = []
    for yy in (-1, 1):
        t = taper if yy > 0 else 1.0
        for xx, zz in ((-1, -1), (1, -1), (1, 1), (-1, 1)):
            pts.append(add(center, add(add(mul(ax, xx*sx*t), mul(ay, yy*sy)), mul(az, zz*sz*t))))
    ids = [mesh.v(p) for p in pts]
    for f in ((0, 1, 2, 3), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)):
        mesh.quad(*(ids[i] for i in f), color, center, shade)
    return ids

def wedge_clump(mesh, base_center, base_side, base_fwd, half_w, half_d, tip, color, shade=None):
    """Angular hair clump: a four-sided pyramid from a small base patch to a tip point."""
    corners = [add(base_center, add(mul(base_side, sx*half_w), mul(base_fwd, sz*half_d))) for sx, sz in ((-1, -1), (1, -1), (1, 1), (-1, 1))]
    ids = [mesh.v(p) for p in corners]; t = mesh.v(tip)
    inside = lerp(base_center, tip, .3)
    for i in range(4): mesh.tri(ids[i], ids[(i+1) % 4], t, color, inside, shade)
    mesh.quad(*ids, color, tip, shade)

def rotate_y(p, deg):
    a = math.radians(deg); c, s = math.cos(a), math.sin(a)
    return (p[0]*c+p[2]*s, p[1], -p[0]*s+p[2]*c)
