"""Worn wooden ruler tile for the speed gauge (user, October 31, 2026): no numbers, only tick marks.

The gauge's length follows the boots speed cap, so the ruler is a vertically tiling segment (64 x 128 px):
grain along the length, worn darker edges, and ink ticks on the right edge (long, short, medium, short).
Run: python Tools/Art/generate_speed_ruler.py  (needs Pillow). Writes Assets/_Game/Resources/UI/Hud/ruler_tile.png.
"""
import random
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageChops

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Resources/UI/Hud'
W, H = 64, 128


def tile(seed=11):
    rng = random.Random(seed)
    img = Image.new('RGB', (W, H), (178, 132, 82))
    draw = ImageDraw.Draw(img, 'RGBA')
    # Grain: wavy vertical streaks that wrap seamlessly top to bottom (same x at y=0 and y=H).
    for _ in range(26):
        x = rng.uniform(0, W); width = rng.choice((1, 1, 2)); shade = rng.randint(-38, 22)
        colour = (120 + shade, 82 + shade, 44 + shade, rng.randint(50, 110))
        amp, phase = rng.uniform(0.5, 2.2), rng.uniform(0, 6.28)
        import math
        points = [(x + amp * math.sin(phase + 2 * math.pi * y / H), y) for y in range(0, H + 1, 4)]
        draw.line(points, fill=colour, width=width)
    img = img.filter(ImageFilter.GaussianBlur(0.6))
    draw = ImageDraw.Draw(img, 'RGBA')
    # Worn edges: darker, rubbed sides.
    for i in range(7):
        a = int(90 * (1 - i / 7))
        draw.line([(i, 0), (i, H)], fill=(70, 44, 20, a)); draw.line([(W - 1 - i, 0), (W - 1 - i, H)], fill=(70, 44, 20, a))
    # Ticks on the right edge: long at the tile start, medium halfway, short at the quarters; slightly faded ink.
    for y, length, width in ((0, 30, 3), (64, 22, 3), (32, 13, 2), (96, 13, 2)):
        ink = (48, 28, 12, rng.randint(170, 220))
        draw.line([(W - 4 - length, y), (W - 4, y)], fill=ink, width=width)
        if y == 0: draw.line([(W - 4 - length, H - 1), (W - 4, H - 1)], fill=ink, width=1)  # wrap the long tick across the seam
    # Scuffs: a few light scratches.
    for _ in range(5):
        x, y = rng.uniform(8, W - 20), rng.uniform(4, H - 4)
        draw.line([(x, y), (x + rng.uniform(4, 12), y + rng.uniform(-3, 3))], fill=(236, 206, 160, 70), width=1)
    return img


if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    tile().save(OUT / 'ruler_tile.png')
    print('ruler_tile.png')
