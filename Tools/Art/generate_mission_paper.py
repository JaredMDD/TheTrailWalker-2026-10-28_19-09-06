"""Worn paper sheets and tab separators for the mission selection menu (user, October 31, 2026).

The menu reads as a stack of worn papers with recipe-book-like tab separators, one per mission branch.
Run from the repository root or Tools/Art: python Tools/Art/generate_mission_paper.py  (needs Pillow).
Writes Assets/_Game/Resources/UI/Missions/paper_0..2.png (sheet variants) and tab.png.
"""
import random
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageChops

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Resources/UI/Missions'
BASE = (226, 210, 172)


def noise(size, cells, rng, low, high):
    small = Image.new('L', (cells, max(1, cells * size[1] // size[0])))
    small.putdata([rng.randint(low, high) for _ in range(small.width * small.height)])
    return small.resize(size, Image.BICUBIC)


def ragged_mask(size, rng, margin=10, jitter=5):
    """Alpha mask with slightly uneven, nicked edges and softened corners."""
    w, h = size
    points = []
    step = 6
    offset = [0.0]
    def wobble():
        offset[0] = max(-jitter, min(jitter, offset[0] * 0.9 + rng.uniform(-0.9, 0.9)))
        return offset[0]
    for x in range(margin, w - margin + 1, step): points.append((x, margin + wobble()))
    for y in range(margin, h - margin + 1, step): points.append((w - margin + wobble(), y))
    for x in range(w - margin, margin - 1, -step): points.append((x, h - margin + wobble()))
    for y in range(h - margin, margin - 1, -step): points.append((margin + wobble(), y))
    mask = Image.new('L', size, 0)
    ImageDraw.Draw(mask).polygon(points, fill=255)
    # A few small tears along the edges.
    draw = ImageDraw.Draw(mask)
    for _ in range(4):
        side = rng.randrange(4)
        t = rng.uniform(0.15, 0.85)
        r = rng.uniform(4, 9)
        cx, cy = [(t * w, margin), (w - margin, t * h), (t * w, h - margin), (margin, t * h)][side]
        draw.polygon([(cx - r, cy - r * 0.2), (cx + r, cy - r * 0.2), (cx, cy + r)] if side in (0, 2) else
                     [(cx - r * 0.2, cy - r), (cx - r * 0.2, cy + r), (cx + r, cy)], fill=0)
    return mask.filter(ImageFilter.GaussianBlur(0.8))


def sheet(seed, size=(512, 640)):
    rng = random.Random(seed)
    w, h = size
    img = Image.new('RGB', size, BASE)
    # Mottled fibres: two noise octaves multiplied into the base colour.
    grain = ImageChops.multiply(noise(size, 10, rng, 232, 255).convert('RGB'), noise(size, 60, rng, 240, 255).convert('RGB'))
    img = ImageChops.multiply(img, grain)
    draw = ImageDraw.Draw(img, 'RGBA')
    # Folds: one horizontal and one vertical crease, a light line beside a darker one.
    fy = int(h * rng.uniform(0.42, 0.58)); fx = int(w * rng.uniform(0.40, 0.60))
    draw.line([(0, fy), (w, fy + rng.randint(-6, 6))], fill=(120, 96, 60, 70), width=2)
    draw.line([(0, fy + 2), (w, fy + 2)], fill=(255, 248, 228, 60), width=2)
    draw.line([(fx, 0), (fx + rng.randint(-5, 5), h)], fill=(120, 96, 60, 45), width=2)
    # Stains: soft rings and blotches.
    stains = Image.new('RGBA', size, (0, 0, 0, 0)); sd = ImageDraw.Draw(stains)
    for _ in range(rng.randint(2, 4)):
        cx, cy, r = rng.uniform(60, w - 60), rng.uniform(60, h - 60), rng.uniform(22, 60)
        sd.ellipse([cx - r, cy - r * 0.9, cx + r, cy + r * 0.9], outline=(130, 92, 40, 80), width=rng.randint(3, 6))
        sd.ellipse([cx - r * 0.8, cy - r * 0.7, cx + r * 0.8, cy + r * 0.7], fill=(150, 112, 60, 22))
    stains = stains.filter(ImageFilter.GaussianBlur(3))
    img = Image.alpha_composite(img.convert('RGBA'), stains)
    # Darkened, aged edges (vignette).
    edge = Image.new('L', size, 0); ImageDraw.Draw(edge).rectangle([26, 26, w - 26, h - 26], fill=255)
    edge = edge.filter(ImageFilter.GaussianBlur(28))
    dark = Image.new('RGBA', size, (112, 80, 40, 0))
    dark.putalpha(ImageChops.invert(edge).point(lambda v: int(v * 0.6)))
    img = Image.alpha_composite(img, dark)
    img.putalpha(ragged_mask(size, rng))
    return img


def tab(seed, size=(200, 96)):
    """A separator tab: rounded top, worn card colour, open at the bottom where it joins the page."""
    rng = random.Random(seed)
    w, h = size
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    mask = Image.new('L', size, 0)
    ImageDraw.Draw(mask).rounded_rectangle([6, 6, w - 6, h + 30], radius=26, fill=255)
    card = Image.new('RGB', size, (236, 224, 192))
    card = ImageChops.multiply(card, noise(size, 12, rng, 215, 255).convert('RGB'))
    shade = Image.new('L', size, 0); ImageDraw.Draw(shade).rounded_rectangle([18, 16, w - 18, h + 30], radius=18, fill=255)
    shade = shade.filter(ImageFilter.GaussianBlur(10))
    card = Image.composite(card, ImageChops.multiply(card, Image.new('RGB', size, (200, 170, 120))), shade)
    img.paste(card, (0, 0), mask.filter(ImageFilter.GaussianBlur(0.8)))
    return img


if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    for i in range(3):
        sheet(1307 + i * 31).save(OUT / f'paper_{i}.png')
        print(f'paper_{i}.png')
    tab(77).save(OUT / 'tab.png'); print('tab.png')
