"""Region emblems for the map's region list (phone round R1, Nov 21, 2026); stdlib only.

In colour (user, Nov 21: "the icons should use different color to properly represent what they refer to"): a ring badge
in the region's tint (the colour of the row swatch, darkened) around a sky-coloured face with a flat faceted motif from
the region, drawn with the same shape helpers as the HUD silhouettes (lit side and shaded side per shape).
Output: Assets/_Game/Resources/UI/Map/Emblems/region_NN.png (256 px RGBA), imported as Sprites by CraftingUiImport.
"""
from pathlib import Path
import math
import sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_hud_icons as hud
from generate_hud_icons import rect, circle, poly

OUT = hud.ROOT / 'Assets/_Game/Resources/UI/Map/Emblems'
FACE = 53  # motif shapes are clipped to the badge face (radius in the 128 grid)
# CollectionPrototype.RegionColor tints, darkened for the ring so the badge reads against its swatch.
TINT = {1: (0.62, 0.70, 0.36), 2: (0.56, 0.60, 0.42), 3: (0.33, 0.45, 0.38), 4: (0.40, 0.58, 0.32)}
def ring(n): return '#' + ''.join(f'{round(v * 0.62 * 255):02x}' for v in TINT[n])


def pine(x, base, w, h, lit, shade, trunk):
    """Faceted pine: three stacked tiers, lit left half and shaded right half, on a trunk."""
    shapes = [rect(x - 3, base - 8, 6, 10, trunk)]
    for k in range(3):
        bottom = base - 6 - k * h * 0.26
        half = w / 2 * (1 - k * 0.22)
        apex = bottom - h * 0.42
        shapes += [poly([(x, apex), (x, bottom), (x - half, bottom)], lit),
                   poly([(x, apex), (x + half, bottom), (x, bottom)], shade)]
    return shapes


def rays(cx, cy, r0, r1, angles, width, color):
    out = []
    for a in angles:
        t = math.radians(a)
        dx, dy = math.cos(t), -math.sin(t)
        px, py = -dy * width / 2, dx * width / 2
        out.append(poly([(cx + dx * r0 + px, cy + dy * r0 + py), (cx + dx * r1, cy + dy * r1),
                         (cx + dx * r0 - px, cy + dy * r0 - py)], color))
    return out


def fern(lit, shade, stem):
    """A fern frond: a stem with broad alternating leaflets shrinking upward and a fiddlehead curl at the top."""
    shapes = [poly([(61, 114), (66, 114), (66, 34), (63, 34)], stem)]
    for y in range(106, 42, -11):
        length = 9 + (y - 34) * 0.36
        shapes.append(poly([(64, y + 2), (64 - length * 0.5, y - 9), (64 - length, y - 12), (64 - length * 0.6, y - 2)], lit))
        shapes.append(poly([(65, y - 4), (65 + length * 0.5, y - 15), (65 + length, y - 18), (65 + length * 0.6, y - 8)], shade))
    shapes += [circle(68, 30, 9, stem), circle(69, 30, 4.5, '#e9f1d2'), circle(70, 31, 2, stem)]
    return shapes


emblems = {
    # Dawn Plains: the sun rising over a sunlit meadow, grass tufts on the near hill.
    1: ('#fbe6b4', rays(64, 78, 26, 44, [20, 45, 70, 90, 110, 135, 160], 8, '#f3b83d')
        + [circle(64, 78, 21, '#f39c2a'), poly([(43, 78), (64, 57), (64, 78)], '#f7c04a')]
        + [poly([(0, 84), (34, 74), (64, 80), (96, 72), (128, 82), (128, 128), (0, 128)], '#79a83c'),
           poly([(64, 80), (96, 72), (128, 82), (128, 128), (64, 128)], '#5e8a2e')]
        + [poly([(30 + d, 98), (34 + d, 86), (38 + d, 98)], '#b5d867') for d in (0, 10, 50, 60)]),
    # Windward Hills: an olive hill behind a heather hill, gusts of wind above them.
    2: ('#d9e9f1', [poly([(0, 92), (36, 62), (70, 88), (128, 70), (128, 128), (0, 128)], '#a3ab6c'),
        poly([(0, 104), (50, 80), (92, 96), (128, 88), (128, 128), (0, 128)], '#8e7aa3'),
        poly([(50, 80), (92, 96), (128, 88), (128, 128), (50, 128)], '#6f5f86'),
        rect(26, 30, 56, 7, '#6d8ea3', 3.5), circle(80, 24, 8, '#6d8ea3'), circle(79, 24, 3.5, '#d9e9f1'), rect(70, 24, 9, 4, '#d9e9f1'),
        rect(44, 46, 54, 7, '#6d8ea3', 3.5), rect(30, 58, 30, 6, '#9fb6c4', 3)]),
    # Mistwood: three dark pines in the fog, mist drifting across their feet.
    3: ('#d3ddd8', pine(42, 98, 34, 66, '#3f6a50', '#2d4d3b', '#4a3b2c') + pine(88, 100, 32, 60, '#3f6a50', '#2d4d3b', '#4a3b2c')
        + pine(65, 110, 42, 86, '#4d7d5f', '#335a44', '#4a3b2c')
        + [rect(4, 88, 58, 12, '#f1f4f2', 6), rect(70, 98, 58, 6, '#f1f4f2', 3), rect(0, 108, 128, 20, '#3a4c40')]),
    # Fern Hollow: a fern frond curling open beside a red mushroom.
    4: ('#e6efd1', fern('#5f9c34', '#447a25', '#3c6a20')
        + [rect(27, 86, 8, 12, '#d6c296', 2), poly([(17, 88), (31, 72), (45, 88)], '#c8402f'), poly([(31, 72), (45, 88), (31, 88)], '#a3311f'),
           circle(26, 83, 2.2, '#ffffff'), circle(35, 80, 2, '#ffffff'), circle(39, 85, 1.6, '#ffffff')]),
}


def raster(name, face, motif, ring_color):
    """Like generate_hud_icons.raster (4x grid, 2x downsample to 256 px) with the motif clipped to the badge face."""
    n, scale = 512, 4
    buf = bytearray(n * n * 4)
    badge = [circle(64, 64, 61, ring_color), circle(64, 64, FACE + 0.5, face)]
    for k, shape in enumerate(badge + motif):
        x0, y0, x1, y1 = hud.bounds(shape)
        color = hud.rgba(shape[2])
        for y in range(max(0, int(y0 * scale)), min(n, math.ceil(y1 * scale))):
            for x in range(max(0, int(x0 * scale)), min(n, math.ceil(x1 * scale))):
                px, py = (x + .5) / scale, (y + .5) / scale
                if k >= len(badge) and (px - 64) ** 2 + (py - 64) ** 2 > FACE ** 2:
                    continue
                if hud.inside(shape, px, py):
                    i = (y * n + x) * 4
                    buf[i:i + 4] = bytes(color)
    w = n // 2
    out = bytearray(w * w * 4)
    for y in range(w):
        for x in range(w):
            samples = [(2 * y + dy) * n * 4 + (2 * x + dx) * 4 for dy in range(2) for dx in range(2)]
            a = sum(buf[k + 3] for k in samples)
            i = (y * w + x) * 4
            if a:
                for c in range(3):
                    out[i + c] = round(sum(buf[k + c] * buf[k + 3] for k in samples) / a)
            out[i + 3] = round(a / 4)
    hud.write_png(OUT / (name + '.png'), w, w, out)


if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    for number, (face, motif) in emblems.items():
        raster(f'region_{number:02}', face, motif, ring(number))
        print(f'region_{number:02}', '256px RGBA PNG')
