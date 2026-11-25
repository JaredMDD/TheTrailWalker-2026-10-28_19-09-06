"""Milestone 15 slice 1: the shared pet bed (user, November 24, 2026: one design for every house level).

An oval woven basket with a cloth cushion, in the house palette (HousePalette.png) so it shares the house material.
Contract: metres, +Y up, +Z toward the camera; pivot at the floor centre (it stands on PetBedAnchor). It holds the
largest pet (the Dog, 0.28 x 0.80 m) with a margin, and stays within 300 triangles. Standard library only.
Run: python Tools/Art/generate_pet_bed.py, then The Trail Walker > Pets > Build Pet Art (or the pets-art request).
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k
from house_specs import PALETTE

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Settlement/Pets'
HEADER = 'The Trail Walker pet bed; metres; +Y up; +Z toward the camera; pivot at the floor centre.'
STRETCH = 1.55          # the round basket is stretched along Z into an oval for the pets' long bodies
RADIUS, RIM = .32, .17  # x half-width 0.32 m (0.64 m across), z half-length 0.50 m; rim height 0.17 m
SIDES = 12

pal = k.Palette()
for spec in PALETTE: pal.add(*spec)


def band(m, y, r, h, color):
    k.limb(m, [((0, y-h/2, 0), r, r), ((0, y+h/2, 0), r, r)], color, sides=SIDES, turn=math.pi/SIDES, cap_start=False, cap_end=False)


def bed():
    m = k.Mesh('PetBed', pal)
    k.limb(m, [((0, 0, 0), RADIUS, RADIUS), ((0, RIM, 0), RADIUS+.02, RADIUS+.02)], 'PineLight', sides=SIDES, turn=math.pi/SIDES, cap_end=False)   # woven wall
    for y in (.055, .115): band(m, y, RADIUS+.012+y*.12, .025, 'WoodDark')                                                                         # weave bands
    band(m, RIM, RADIUS+.035, .045, 'Pine')                                                                                                       # rolled rim
    k.limb(m, [((0, .02, 0), RADIUS-.03, RADIUS-.03), ((0, .095, 0), RADIUS-.05, RADIUS-.05)], 'Cloth', sides=SIDES, turn=math.pi/SIDES)          # cushion
    k.limb(m, [((0, .095, 0), RADIUS-.05, RADIUS-.05), ((0, .11, 0), RADIUS-.12, RADIUS-.12)], 'CreamCloth', sides=SIDES, turn=math.pi/SIDES, cap_start=False)  # cushion dip
    m.vertices = [(x, y, z*STRETCH) for x, y, z in m.vertices]
    return m


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    m = bed()
    info = m.save(OUT, HEADER)
    lo = [min(v[i] for v in m.vertices) for i in range(3)]; hi = [max(v[i] for v in m.vertices) for i in range(3)]
    assert info['triangles'] <= 300, info
    assert lo[1] >= -.001 and hi[0] - lo[0] >= .28 + .2 and hi[2] - lo[2] >= .80 + .1, (lo, hi)   # holds the Dog with a margin
    (OUT/'PetArtManifest.json').write_text(json.dumps(dict(
        Revision=1, Palette='Assets/_Game/Art/Settlement/Houses/HousePalette.png', Pivot='floor centre',
        Beds=[dict(Asset=info['name'], Triangles=info['triangles'], BoundsMin=lo, BoundsMax=hi)]), indent=2)+'\n', encoding='utf-8')
    print('PetBed %d tris, %.2f x %.2f x %.2f m' % (info['triangles'], hi[0]-lo[0], hi[1]-lo[1], hi[2]-lo[2]))


if __name__ == '__main__':
    main()
