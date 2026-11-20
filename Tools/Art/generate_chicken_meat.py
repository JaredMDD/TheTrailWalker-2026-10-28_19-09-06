"""Raw chicken leg (thigh + drumstick + bone knuckle) for the Ranch shop. Python 3 standard library only.

Same contract as generate_tier02_batch03.duck_meat: local Y-up OBJ lofted along X, palette-strip UVs, detail TYPE in palette alpha.
Run: python Tools/Art/generate_chicken_meat.py   (writes only the chicken OBJ, palette and manifest)
"""
import sys, json, math
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
import generate_region01_meshes as base
import detail_textures as detail

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Art/Items/Tier01Design/ShopInputs'
PAL = ROOT / 'Assets/_Game/Art/Items/Tier01Design/Palettes/ChickenMeatPalette.png'
# 0-2 pale pink-beige skin, 3 bone, 4 exposed cut, 5 bump spots
COLORS = [(238, 206, 190), (226, 188, 172), (246, 224, 206), (241, 233, 214), (205, 120, 118), (214, 170, 154)]
SIDES = 16


def loft(m, rows, color, jitter=0.0):
    """Closed loft along X with pole ends; radial jitter gives the bumpy faceted skin."""
    rings = []
    for k, (x, ry, rz) in enumerate(rows):
        if ry == 0 or rz == 0:
            rings.append([m.v((x, 0, 0))])
        else:
            rings.append([m.v((x, ry * s * math.cos(i * math.tau / SIDES), rz * s * math.sin(i * math.tau / SIDES)))
                          for i in range(SIDES) for s in [1 + jitter * (((i * 7 + k * 5) % 5) - 2) / 2]])
    for k, (a, b) in enumerate(zip(rings, rings[1:])):
        for i in range(SIDES):
            j = (i + 1) % SIDES
            c = color(k, i) if callable(color) else color
            if len(a) == 1: m.tri(a[0], b[j], b[i], c)
            elif len(b) == 1: m.tri(a[i], a[j], b[0], c)
            else:
                m.tri(a[i], a[j], b[j], c); m.tri(a[i], b[j], b[i], c)


def chicken_meat():
    m = base.Model('ChickenMeat')
    def skin(k, i):
        angle = (i + .5) * math.tau / SIDES
        if math.sin(angle) < -.3: return (0, 2)[(k + i) % 2] if (k + i) % 5 else 5  # camera-facing side: light skin with bumps
        return 1 if (k + i) % 3 else 0
    thigh_and_drum = [(-.43, 0, 0), (-.40, .12, .095), (-.30, .21, .165), (-.14, .245, .185), (.0, .20, .15),
                      (.10, .13, .10), (.19, .09, .07), (.27, .075, .06), (.30, 0, 0)]
    loft(m, thigh_and_drum, skin, .09)
    loft(m, [(.26, 0, 0), (.275, .035, .035), (.36, .032, .032), (.385, .056, .05), (.425, .05, .046), (.44, 0, 0)], 3)
    return m


def main():
    m = chicken_meat()
    OUT.mkdir(parents=True, exist_ok=True)
    text = ['# TheTrailWalker; local Y up; flat palette-mapped item mesh.', 'o ChickenMeat']
    text += ['v %.7f %.7f %.7f' % v for v in m.vertices]
    text += ['vt %.7f 0.5' % ((i + .5) / len(COLORS)) for i in range(len(COLORS))] + ['s off']
    text += ['f %d/%d %d/%d %d/%d' % (a, k + 1, b, k + 1, c, k + 1) for a, b, c, k in m.faces]
    (OUT / 'ChickenMeat.obj').write_text('\n'.join(text) + '\n', encoding='utf-8')
    px = [(*rgb, detail.alpha('flat')) for rgb in COLORS]
    detail.write_png(PAL, len(COLORS) * 4, 4, [[p for p in px for _ in range(4)] for _ in range(4)])
    lo = [min(v[i] for v in m.vertices) for i in range(3)]; hi = [max(v[i] for v in m.vertices) for i in range(3)]
    manifest = {'Name': 'ChickenMeat', 'Model': 'ChickenMeat', 'SourcePath': 'Assets/_Game/Art/Items/Tier01Design/ShopInputs/ChickenMeat.obj',
                'PalettePath': 'Assets/_Game/Art/Items/Tier01Design/Palettes/ChickenMeatPalette.png', 'PrefabPath': 'Tier01/ShopInputs/ChickenMeat',
                'ItemId': 'animal.chicken.meat', 'Triangles': len(m.faces), 'BoundsMin': lo, 'BoundsMax': hi}
    (OUT / 'ChickenMeatManifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
    print('triangles', len(m.faces), 'bounds', [round(h - l, 3) for l, h in zip(lo, hi)])


if __name__ == '__main__':
    main()
