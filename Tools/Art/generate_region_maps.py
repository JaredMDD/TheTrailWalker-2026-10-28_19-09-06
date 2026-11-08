"""Painted, worn region maps for the camp map (user, October 31, 2026).

A real worn-out map with representative colours and shapes per region: watercolour-like terrain areas (meadows,
forests, hills, water, mountains, marsh) with darker ink outlines and map symbols, on aged, folded, stained paper.
The game draws the road and camp buttons on top (their count and sides come from the travel data), so the central
corridor stays as open ground, and a parchment cartouche at the top holds the region title.
Run: python Tools/Art/generate_region_maps.py  (needs Pillow). Writes Assets/_Game/Resources/UI/Map/region_01..06.png.
"""
import math
import random
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageChops
import importlib.util

HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('paper', HERE / 'generate_mission_paper.py')
paper = importlib.util.module_from_spec(spec); spec.loader.exec_module(paper)

ROOT = HERE.parents[1]
OUT = ROOT / 'Assets/_Game/Resources/UI/Map'
W, H = 768, 1000
INK = (70, 46, 22)

GREEN = (122, 160, 78); MEADOW = (168, 190, 98); WHEAT = (214, 190, 104); FOREST = (58, 104, 56); DEEP = (40, 78, 48)
HILL = (166, 132, 82); ROCK = (140, 132, 120); WATER = (84, 140, 176); MIST = (210, 222, 226); SNOW = (240, 240, 236)
MARSH = (110, 150, 120); MOUNT = (128, 112, 96); FERN = (82, 136, 70); SAND = (214, 196, 150)


def blob(rng, cx, cy, rx, ry, lumps=9):
    """Organic area mask: overlapping ellipses, blurred and thresholded into a soft-edged shape."""
    mask = Image.new('L', (W, H), 0); d = ImageDraw.Draw(mask)
    for _ in range(lumps):
        ox, oy = rng.gauss(0, rx * 0.45), rng.gauss(0, ry * 0.45)
        sx, sy = rx * rng.uniform(0.35, 0.7), ry * rng.uniform(0.35, 0.7)
        d.ellipse([cx + ox - sx, cy + oy - sy, cx + ox + sx, cy + oy + sy], fill=255)
    mask = mask.filter(ImageFilter.GaussianBlur(14)).point(lambda v: 255 if v > 110 else 0)
    return mask.filter(ImageFilter.GaussianBlur(2))


def paint(img, mask, colour, rng, alpha=0.78, outline=True):
    """Watercolour fill: mottled colour, darker pooled edge and an ink outline."""
    tex = paper.noise((W, H), 18, rng, 200, 255)
    layer = Image.new('RGBA', (W, H), colour + (0,))
    layer.putalpha(ImageChops.multiply(mask, tex.point(lambda v: int(v * alpha))))
    img = Image.alpha_composite(img, layer)
    if outline:
        edge = ImageChops.subtract(mask.filter(ImageFilter.MaxFilter(5)), mask.filter(ImageFilter.MinFilter(3)))
        pool = Image.new('RGBA', (W, H), tuple(int(c * 0.6) for c in colour) + (0,)); pool.putalpha(edge.point(lambda v: int(v * 0.55)))
        img = Image.alpha_composite(img, pool)
    return img


def wash(img, colour, rng, alpha):
    layer = Image.new('RGBA', (W, H), colour + (0,))
    layer.putalpha(paper.noise((W, H), 7, rng, int(alpha * 150), int(alpha * 255)))
    return Image.alpha_composite(img, layer)


def corridor_clear(x):
    return abs(x - W / 2) < W * 0.15


def side_point(rng, y0=200, y1=H - 80):
    while True:
        x, y = rng.uniform(30, W - 30), rng.uniform(y0, y1)
        if not corridor_clear(x): return x, y


def river(img, rng, colour=WATER, width=14, points=None):
    d = ImageDraw.Draw(img, 'RGBA')
    pts = points or []
    if not pts:
        left = rng.random() < 0.5
        lo, hi = (30, W * 0.33) if left else (W * 0.67, W - 30)
        x = rng.uniform(lo, hi); y = H + 10
        while y > 180:
            pts.append((x, y)); y -= rng.uniform(40, 70); x = min(max(x + rng.uniform(-30, 30), lo), hi)
    d.line(pts, fill=colour + (210,), width=width, joint='curve')
    d.line(pts, fill=(48, 92, 128, 150), width=2, joint='curve')
    return img


# ---- ink symbols
def tree(d, x, y, s=1.0, colour=DEEP):
    d.polygon([(x - 7 * s, y), (x + 7 * s, y), (x, y - 16 * s)], fill=colour + (230,), outline=INK + (200,))
    d.line([(x, y), (x, y + 4 * s)], fill=INK + (200,), width=2)


def round_tree(d, x, y, s=1.0, colour=FOREST):
    d.ellipse([x - 7 * s, y - 14 * s, x + 7 * s, y], fill=colour + (230,), outline=INK + (200,))
    d.line([(x, y), (x, y + 4 * s)], fill=INK + (200,), width=2)


def hill_symbol(d, x, y, s=1.0):
    d.chord([x - 26 * s, y - 16 * s, x + 26 * s, y + 16 * s], 180, 360, fill=HILL + (220,), outline=INK + (210,))
    for i in range(4): d.line([(x + 4 * s + i * 5 * s, y - 12 * s + i * 3 * s), (x + i * 5 * s, y - 2 * s)], fill=INK + (130,), width=1)


def mountain_symbol(d, x, y, s=1.0):
    d.polygon([(x - 30 * s, y), (x, y - 40 * s), (x + 30 * s, y)], fill=MOUNT + (235,), outline=INK + (220,))
    d.polygon([(x - 10 * s, y - 27 * s), (x, y - 40 * s), (x + 10 * s, y - 27 * s), (x + 3 * s, y - 30 * s), (x - 3 * s, y - 26 * s)], fill=SNOW + (240,))
    d.polygon([(x, y - 40 * s), (x + 30 * s, y), (x + 6 * s, y)], fill=(90, 76, 64, 120))


def grass(d, x, y, s=1.0):
    for k in (-1, 0, 1): d.line([(x + k * 4 * s, y), (x + k * 6 * s, y - (8 + 3 * (k == 0)) * s)], fill=(70, 100, 40, 200), width=2)


def flower(d, x, y, rng):
    c = rng.choice([(206, 92, 80), (232, 196, 80), (180, 120, 190), (240, 240, 230)])
    d.ellipse([x - 3, y - 3, x + 3, y + 3], fill=c + (230,))


def reed(d, x, y, s=1.0):
    for k in range(3):
        rx = x + (k - 1) * 4 * s
        d.line([(rx, y), (rx, y - 16 * s)], fill=(70, 96, 50, 220), width=2); d.ellipse([rx - 2 * s, y - 22 * s, rx + 2 * s, y - 14 * s], fill=(120, 80, 40, 230))


def fern(d, x, y, s=1.0):
    d.line([(x, y), (x + 3 * s, y - 20 * s)], fill=(50, 96, 44, 230), width=2)
    for i in range(5):
        py = y - 4 * s - i * 3.5 * s; l = (7 - i) * s
        d.line([(x + i * 0.6 * s, py), (x - l, py - 3 * s)], fill=(60, 116, 52, 220), width=2); d.line([(x + i * 0.6 * s, py), (x + l, py - 3 * s)], fill=(60, 116, 52, 220), width=2)


def rocks(d, x, y, s=1.0):
    d.polygon([(x - 9 * s, y), (x - 5 * s, y - 8 * s), (x + 4 * s, y - 10 * s), (x + 10 * s, y - 2 * s), (x + 7 * s, y)], fill=ROCK + (230,), outline=INK + (200,))


def wind(d, x, y, s=1.0):
    pts = [(x + 22 * s * math.cos(t) * (1 - t / 9), y + 10 * s * math.sin(t) * (1 - t / 9)) for t in [i * 0.3 for i in range(22)]]
    d.line([(x - 36 * s, y + 10 * s)] + pts, fill=(240, 236, 220, 200), width=3)
    d.line([(x - 36 * s, y + 10 * s)] + pts, fill=INK + (120,), width=1)


def compass(d, x, y, r):
    d.ellipse([x - r, y - r, x + r, y + r], fill=(236, 222, 186, 200), outline=INK + (200,), width=2)
    for a, l in ((0, 1.0), (90, 1.0), (180, 1.0), (270, 1.0), (45, 0.6), (135, 0.6), (225, 0.6), (315, 0.6)):
        t = math.radians(a - 90); s = math.radians(a)
        tip = (x + r * l * math.cos(t), y + r * l * math.sin(t))
        side = (x + r * 0.14 * math.cos(s), y + r * 0.14 * math.sin(s)); side2 = (x - r * 0.14 * math.cos(s), y - r * 0.14 * math.sin(s))
        d.polygon([tip, side, (x, y)], fill=(150, 40, 30, 230) if a == 0 else INK + (220,)); d.polygon([tip, side2, (x, y)], fill=(236, 222, 186, 230), outline=INK + (220,))
    d.text((x - 4, y - r - 15), 'N', fill=INK + (230,))


def cartouche(img):
    """Parchment ribbon behind the title so it always reads."""
    d = ImageDraw.Draw(img, 'RGBA')
    d.rounded_rectangle([120, 26, W - 120, 118], radius=26, fill=(238, 224, 186, 235), outline=INK + (200,), width=3)
    d.rounded_rectangle([130, 36, W - 130, 108], radius=20, outline=INK + (110,), width=1)
    return img


def scatter(d, fn, count, rng, y0=200, y1=H - 70, *args):
    for _ in range(count):
        x, y = side_point(rng, y0, y1); fn(d, x, y, *args) if args else fn(d, x, y)


def region(number):
    rng = random.Random(5200 + number)
    img = paper.sheet(9100 + number, (W, H))
    alpha = img.getchannel('A')

    if number == 1:  # Dawn Plains: open meadows, wheat patches, small groves, a stream, wildflowers, morning sun
        img = wash(img, MEADOW, rng, 0.42)
        for _ in range(5): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 80, 60), WHEAT, rng, 0.55)
        for _ in range(6): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 60, 46), FOREST, rng, 0.7)
        img = river(img, rng)
        d = ImageDraw.Draw(img, 'RGBA')
        for _ in range(26): x, y = side_point(rng); tree(d, x, y, rng.uniform(0.8, 1.2))
        for _ in range(40): x, y = side_point(rng); grass(d, x, y)
        for _ in range(60): x, y = side_point(rng); flower(d, x, y, rng)
        d.ellipse([W - 108, 140, W - 52, 196], fill=(244, 200, 90, 220), outline=INK + (180,), width=2)
        for i in range(12):
            a = i * math.pi / 6; d.line([(W - 80 + 34 * math.cos(a), 168 + 34 * math.sin(a)), (W - 80 + 46 * math.cos(a), 168 + 46 * math.sin(a))], fill=(214, 160, 60, 220), width=3)
    elif number == 2:  # Windward Hills: ochre grassland, rolling brown hills, rocky outcrops, wind
        img = wash(img, (190, 178, 110), rng, 0.4)
        for _ in range(7): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 90, 50), HILL, rng, 0.6)
        for _ in range(3): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 50, 40), FOREST, rng, 0.7)
        d = ImageDraw.Draw(img, 'RGBA')
        for _ in range(16): x, y = side_point(rng); hill_symbol(d, x, y, rng.uniform(0.8, 1.3))
        for _ in range(14): x, y = side_point(rng); rocks(d, x, y)
        for _ in range(14): x, y = side_point(rng); tree(d, x, y, rng.uniform(0.8, 1.1))
        for _ in range(7): x, y = side_point(rng); wind(d, x, y)
    elif number == 3:  # Mistwood: dense dark forest, small clearings, drifting mist
        img = wash(img, (96, 130, 92), rng, 0.45)
        for _ in range(8): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 110, 90), DEEP, rng, 0.75)
        d = ImageDraw.Draw(img, 'RGBA')
        for _ in range(90): x, y = side_point(rng); tree(d, x, y, rng.uniform(0.7, 1.1), FOREST)
        for _ in range(6): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 90, 30), MIST, rng, 0.55, outline=False)
    elif number == 4:  # Fern Hollow: green hollows, ferns, round trees, a pond
        img = wash(img, (128, 168, 96), rng, 0.45)
        for _ in range(5): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 80, 60), FERN, rng, 0.65)
        x, y = side_point(rng, 300, 700); img = paint(img, blob(rng, x, y, 60, 40), WATER, rng, 0.8)
        d = ImageDraw.Draw(img, 'RGBA')
        for _ in range(50): x, y = side_point(rng); fern(d, x, y, rng.uniform(0.8, 1.2))
        for _ in range(26): x, y = side_point(rng); round_tree(d, x, y, rng.uniform(0.8, 1.2))
    elif number == 5:  # Cloudstep Mountains: grey peaks with snow, green valleys, clouds
        img = wash(img, (150, 150, 138), rng, 0.4)
        for _ in range(4): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 70, 50), GREEN, rng, 0.6)
        d = ImageDraw.Draw(img, 'RGBA')
        for _ in range(22): x, y = side_point(rng); mountain_symbol(d, x, y, rng.uniform(0.8, 1.4))
        for _ in range(12): x, y = side_point(rng); tree(d, x, y)
        for _ in range(6): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 70, 24), (246, 246, 244), rng, 0.7, outline=False)
    else:  # Reedwater Delta: braided blue channels, teal marsh, sand bars, reeds
        img = wash(img, (150, 186, 168), rng, 0.4)
        for _ in range(6): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 90, 60), MARSH, rng, 0.65)
        for _ in range(4): img = river(img, rng, WATER, rng.randint(8, 16))
        for _ in range(4): x, y = side_point(rng); img = paint(img, blob(rng, x, y, 50, 24), SAND, rng, 0.7)
        d = ImageDraw.Draw(img, 'RGBA')
        for _ in range(50): x, y = side_point(rng); reed(d, x, y, rng.uniform(0.8, 1.2))

    # Open ground along the road corridor, so the game's road reads clearly.
    road = Image.new('L', (W, H), 0); ImageDraw.Draw(road).rectangle([W * 0.36, 150, W * 0.64, H], fill=255)
    road = road.filter(ImageFilter.GaussianBlur(30))
    ground = Image.new('RGBA', (W, H), (226, 208, 166, 0)); ground.putalpha(road.point(lambda v: int(v * 0.55)))
    img = Image.alpha_composite(img, ground)
    img = cartouche(img)
    compass(ImageDraw.Draw(img, 'RGBA'), 76, H - 86, 42)
    # Re-age the painted sheet: darker edges and the torn outline from the paper.
    edge = Image.new('L', (W, H), 0); ImageDraw.Draw(edge).rectangle([30, 30, W - 30, H - 30], fill=255)
    edge = edge.filter(ImageFilter.GaussianBlur(30))
    dark = Image.new('RGBA', (W, H), (96, 66, 30, 0)); dark.putalpha(ImageChops.invert(edge).point(lambda v: int(v * 0.55)))
    img = Image.alpha_composite(img, dark)
    img.putalpha(alpha)
    return img


if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    for n in range(1, 7):
        region(n).save(OUT / f'region_{n:02d}.png')
        print(f'region_{n:02d}.png')
