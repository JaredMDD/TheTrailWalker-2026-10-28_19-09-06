"""Entry strips for the eight house interiors (home revision, spec 2026-11-14 section 2).
Run with Python 3; standard library only. Writes ONLY HouseL01Entry.obj ... HouseL08Entry.obj next to the interiors.
Does not touch the interiors, fronts, guides, studies, manifest or palette (generate_house_meshes.py owns those).

The strip lies in front of the cut front edge (the door threshold stays in the interior at z ~0.1): z from 0.17 to
2.2 (2 m of entry plus the gap under the threshold), full grid width plus the side-wall thickness. Same coordinate
contract as interior(): grid origin at the front-left floor corner, columns toward -X, +Z toward the camera.
Nothing is taller than 0.3 m so the angled camera sees over it. The pet bed and collection box come later (anchors
PetBedAnchor and CollectionBoxAnchor in HouseArtAssets.Interior); until then the strip is plain floor.
"""
import faceted_character_kit as k
import house_geometry as g
from generate_house_meshes import OUT, HEADER, pal
from house_specs import LEVELS, GRID_METRES

FRONT = 2.2     # the strip's front edge (ScrollMin)
BACK = 0.17     # meets the interior's cut-front sill (z 0.01-0.17) without overlapping it


def entry(s):
    level = s['Level']; w = s['GridWidth'] * GRID_METRES; wood = s['Wood']
    m = k.Mesh(f'HouseL{level:02}Entry', pal)
    depth = FRONT - BACK; mid = (FRONT + BACK) / 2
    # Floor: the room's dark base slab and one board per grid column, continuing the room's boards toward the camera.
    g.box(m, (-w / 2, -.11, mid), (w + .36, .12, depth), 'FloorDark')
    for i in range(s['GridWidth']):
        g.box(m, (-GRID_METRES * (i + .5), -.03, mid - .02), (GRID_METRES - .012, .06, depth - .08), 'Floor')
    # Front nosing in the level's wood: a finished edge where the strip ends.
    g.box(m, (-w / 2, -.04, FRONT - .04), (w + .36, .12, .08), wood)
    # Low side edging continuing the side walls (wall centres x 0.08 and -w-0.08, 0.16 thick), capped at the front.
    for x in (.08, -w - .08):
        g.box(m, (x, .11, mid), (.16, .22, depth), wood)
        g.box(m, (x, .135, FRONT - .10), (.20, .27, .20), wood, shade=1)
    # Plain floor otherwise (user, November 14: no mat; the pet bed and collection box come with the pets milestone).
    return m


def main():
    for s in LEVELS:
        m = entry(dict(s))
        lo = [min(v[i] for v in m.vertices) for i in range(3)]; hi = [max(v[i] for v in m.vertices) for i in range(3)]
        assert lo[2] >= .15 and hi[2] <= 2.25 and hi[1] <= .3, (m.name, lo, hi)
        m.save(OUT, HEADER)
        print(m.name, m.triangles, 'triangles', 'x %.2f..%.2f y %.2f..%.2f z %.2f..%.2f' % (lo[0], hi[0], lo[1], hi[1], lo[2], hi[2]))
    print('Generated 8 entry strips.')


if __name__ == '__main__': main()
