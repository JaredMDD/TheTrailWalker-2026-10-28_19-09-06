"""Tier 1 rabbit (faceted revision 4, approved by the user November 7, 2026): the live rabbit in the approved faceted
character style (flat planar facets, chamfered sections, slab ears, small box eyes, wedge nose, palette-strip shades).
Replaced the smooth revision-3 rabbit; the comparison renders are kept in Docs/Art/Studies/FacetedRabbit.

Writes the six rigid parts Rabbit{Body,Head,Ear,FrontFoot,HindFoot,Tail}.obj and RabbitPalette.png into
Assets/_Game/Art/Harvest/Tier01 and merges their rows into mesh-manifest.json. Every part is authored in its
Tier1RabbitRig pivot frame (BodyPivot, HeadPivot, ear/foot/tail pivots), so rig, selection bounds and catalog are
unchanged. Run: python Tools/Art/generate_tier01_rabbit.py (generate_tier01_harvest_meshes.py calls it too), then
Unity: Tier01HarvestAssets.RebuildRabbitOnly().
"""
from pathlib import Path
import json
import faceted_character_kit as k

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Harvest/Tier01'

pal = k.Palette()
# Fur colours carry the shared 'fur' detail tile (TheTrailWalker/FacetedDetail, like the NPC cloth).
for name, color, detail in [('Fur', '#8C7A66', 'fur'), ('FurBack', '#776755', 'fur'), ('FurShade', '#6B5B4A', 'fur'),
                            ('Belly', '#B8A890', 'fur'), ('InnerEar', '#B07C74', None), ('Eye', '#1C1612', None),
                            ('Nose', '#A86A66', None), ('Tail', '#E0D6C4', 'fur')]:
    pal.add(name, color, detail)

def along_z(mesh, sections, colors, cap_start=True, cap_end=True, a=.62, b=.55):
    """Loft along +Z. sections: (z, half width, top, bottom, y centre); colors(face index) picks a palette name.
    Face indices of chamfer_rect with forward=+Y: 7 top plane, 0/6 upper chamfers, 1/5 sides, 2/4 lower chamfers, 3 bottom."""
    rings = []
    for z, w, top, bottom, y in sections:
        c = (0, y, z)
        rings.append((c, k.ring_points(c, (1, 0, 0), (0, 1, 0), k.chamfer_rect(w, top, bottom, a, b))))
    k.loft(mesh, rings, colors(0), cap_start, cap_end, colors=[colors]*(len(rings)-1))

def fur_band(i):
    return 'Belly' if i == 3 else 'FurBack' if i == 7 else 'Fur'

def body():
    # BodyPivot frame (0, .20, 0 above ground). Pear-shaped: high wide haunch, narrower chest rising into the neck.
    m = k.Mesh('RabbitBody', pal)
    along_z(m, [(-.300, .060, .050, .045, .075), (-.265, .120, .120, .105, .085), (-.175, .155, .160, .150, .090),
                (-.050, .140, .140, .130, .085), (.070, .118, .120, .110, .085), (.170, .092, .100, .090, .105),
                (.235, .058, .060, .050, .135)], fur_band)
    return m

def head():
    # HeadPivot frame. Squared cheeks, a short blunt muzzle with a flat front plane, side eyes, wedge nose.
    m = k.Mesh('RabbitHead', pal)
    along_z(m, [(-.100, .040, .040, .040, .030), (-.060, .066, .070, .056, .036), (.000, .077, .080, .062, .040),
                (.060, .070, .074, .056, .035), (.112, .050, .054, .042, .020), (.158, .030, .030, .026, .008)],
            fur_band)
    # Wedge nose on the muzzle plane (the pale chin is the head's bottom plane).
    T, L, R = m.v((0, .030, .157)), m.v((-.013, .012, .159)), m.v((.013, .012, .159))
    P = m.v((0, .014, .171))
    B = m.v((0, .002, .160))
    for f in ((T, L, P), (T, P, R), (L, B, P), (R, P, B)): m.tri(*f, 'Nose', (0, .015, .150), shade=2)
    # Small dark eyes set on the side planes, slightly forward, with a lighter lid slab above.
    for s in (-1, 1):
        k.box(m, (s*.0695, .060, .058), (.008, .017, .022), 'Eye', shade=2)
        k.box(m, (s*.0690, .074, .056), (.008, .006, .028), 'FurShade', shade=1)
    return m

def ear():
    # Ear pivot frame: a flat tapered slab along +Y with an inset inner-ear plate on the front (+Z) face.
    m = k.Mesh('RabbitEar', pal)
    k.vertical_loft(m, [(-.100, .013, .010, .010), (-.020, .020, .012, .012), (.060, .028, .012, .011),
                        (.135, .025, .010, .009), (.180, .007, .004, .004)], 'Fur',
                    colors=[lambda i: 'FurBack' if i == 3 else 'Fur']*4)
    k.box(m, (0, .062, .0118), (.028, .150, .002), 'InnerEar', taper=.45, shade=2)
    return m

def front_foot():
    # Front foot pivot frame: slim angular foreleg from inside the chest down to a flat forepaw.
    m = k.Mesh('RabbitFrontFoot', pal)
    k.limb(m, [((0, .200, -.045), .026, .034), ((0, .110, -.020), .023, .028), ((0, .040, -.006), .018, .021),
               ((0, -.030, .008), .016, .018)], 'Fur', sides=6)
    k.box(m, (0, -.050, .030), (.040, .024, .088), 'Fur', taper=.78)
    return m

def hind_foot():
    # Hind pivot frame: folded haunch block (bulges past the body side) over a long flat hind foot.
    m = k.Mesh('RabbitHindFoot', pal)
    k.vertical_loft(m, [(-.070, .024, .030, .030, -.030), (-.020, .033, .045, .040, -.025), (.050, .048, .075, .070, .000),
                        (.120, .058, .100, .090, .030), (.190, .050, .085, .070, .035), (.240, .024, .040, .035, .025)],
                    'Fur', colors=[lambda i: 'FurShade' if i == 3 else 'Fur']*5)
    k.box(m, (0, -.088, .050), (.068, .028, .250), 'Fur', taper=.82)
    return m

def tail():
    # Tail pivot frame: a faceted cotton puff.
    m = k.Mesh('RabbitTail', pal)
    k.vertical_loft(m, [(-.036, .014, .014, .014), (-.020, .034, .036, .034), (.010, .040, .042, .040),
                        (.030, .026, .028, .026), (.040, .008, .008, .008)], 'Tail', a=.6, b=.6)
    return m

def write_all():
    """Writes the six parts and the palette; returns their stats rows. Safe to call from other generators."""
    OUT.mkdir(parents=True, exist_ok=True)
    header = 'The Trail Walker Tier 1 rabbit, faceted revision 4 (generate_tier01_rabbit.py)'
    stats = [part().save(OUT, header) for part in (body, head, ear, front_foot, hind_foot, tail)]
    pal.write_png(OUT/'RabbitPalette.png')
    for s in stats: s['palette'] = 'RabbitPalette.png'
    manifest = OUT/'mesh-manifest.json'
    if manifest.exists():
        rows = {s['name']: s for s in stats}
        merged = [rows.pop(r['name'], r) for r in json.loads(manifest.read_text(encoding='utf-8'))] + list(rows.values())
        manifest.write_text(json.dumps(merged, indent=2)+'\n', encoding='utf-8')
    return stats

if __name__ == '__main__':
    stats = write_all()
    print('\n'.join('%-18s %4d tris' % (s['name'], s['triangles']) for s in stats), '\ntotal', sum(s['triangles'] for s in stats))
