"""Recipe-level stars 1-10 (user, October 31, 2026): the quality star design continued past five.

Levels 1-5 are the quality stars; 6-10 keep adding star points and use the same segment digits ("10" as two narrow
digits). Editable SVG + transparent 256px PNG, standard library only.
Run from the project root: python Tools/Art/generate_level_stars.py
"""
from pathlib import Path
import generate_hud_icons as art
import generate_quality_badges as quality

ROOT = Path(__file__).resolve().parents[2]
INK = '#38250f'
PATTERNS = {0: 'abcdef', 1: 'bc', 2: 'abged', 3: 'abgcd', 4: 'fgbc', 5: 'afgcd', 6: 'afgedc', 7: 'abc', 8: 'abcdefg', 9: 'abcdfg'}


def digit(value, x0, width, thickness):
    """Seven-segment digit inside the star's number box (y 47-86)."""
    t, w = thickness, width
    seg = {'a': (x0, 47, w, t), 'b': (x0 + w - t, 49, t, 18), 'c': (x0 + w - t, 65, t, 18),
           'd': (x0, 80, w, t), 'e': (x0, 65, t, 18), 'f': (x0, 49, t, 18), 'g': (x0, 64, w, t)}
    return [art.rect(*seg[k], c=INK, radius=2) for k in PATTERNS[value]]


def points_for(level):
    return {1: 5, 2: 5, 3: 6, 4: 6, 5: 10, 6: 11, 7: 11, 8: 12, 9: 12, 10: 14}[level]


def shapes(level):
    if level <= 5:
        return quality.shapes(level)
    p = points_for(level)
    s = [quality.star(59, '#70491b', p), quality.star(54, '#d6a22a', p), art.poly([(64, 10), (76, 47), (64, 61), (52, 47)], '#ffe08a')]
    if level < 10:
        s += digit(level, 53, 24, 6)
    else:
        s += digit(1, 40, 14, 6) + digit(0, 60, 22, 6)
    return s


if __name__ == '__main__':
    art.SVG = ROOT / 'Assets/_Game/Art/UI/Crafting'
    art.PNG = ROOT / 'Assets/_Game/Resources/UI/Crafting'
    art.SVG.mkdir(parents=True, exist_ok=True)
    art.PNG.mkdir(parents=True, exist_ok=True)
    for n in range(1, 11):
        art.write_svg('level_' + str(n), shapes(n))
        art.raster('level_' + str(n), shapes(n))
        print('level_' + str(n), 'SVG + 256px RGBA PNG')
