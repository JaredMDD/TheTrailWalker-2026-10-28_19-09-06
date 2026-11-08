"""Unsaved tier-one inventory geometry; XY presentation plane, front is -Z.

Contract: 22 processed materials, three pine tools, five display wearables.
The caller owns semantic palettes, old-envelope fitting, export and validation.
No fitted character shells are supplied here. Construction uses the prepared
first-design-batch brief and the existing crafted-item shapes as references.
Ordinary models are designed for fewer than 2500 triangles, without runtime
subdivision. Components are closed solids, including seams and raised details;
intersecting components intentionally explain folds, lashings and joinery.

Raw slots refer to c.COLORS (the original 32-slot crafted palette), NOT b.COLORS.
Helpers may shade one slot down (c.box also shades one slot up). In particular:
  0..3 wood; 4..7 hemp/fur; 8..11 hide/leather; 12..15 nettle/reed;
  16..19 charcoal/flint/stone; 20..23 fired clay; 27..29 flour/bone.
Charcoal uses explicit 16/17/18 faces, never cylinder slot-15 green shading.
Rod bone hook uses 28 with shade 27; no slot 26 red, and no metal/reel.
Hat uses 10 for hide, 6/7 for fur. Shirt pockets use hide 10; pants toggles
use wood 2. Backpack frame is wood 2, bag hemp 5, straps leather 9/10.
Fine pelt stitching is hemp 5/6, not a second hide/fur material assignment.
"""

import math

import generate_region01_meshes as b
import generate_tier01_crafted_items as c


MATERIAL_KEYS = (
    'pine_plank', 'pine_charcoal', 'hemp_cord', 'hemp_thread',
    'nettle_thread', 'hemp_cloth', 'nettle_cloth', 'fine_hemp_cloth',
    'cured_rabbit_pelt', 'fine_rabbit_pelt', 'rabbit_leather',
    'fine_rabbit_leather', 'plant_stuffing', 'pine_beam', 'pine_panel',
    'limestone_tiles', 'polished_pebble', 'fired_bricks', 'clay_cup',
    'wheat_flour', 'reed_braid', 'mortar',
)
TOOL_KEYS = ('pine_axe_normal', 'pine_slingshot_normal', 'pine_rod_normal')
WEARABLE_KEYS = ('rabbit_hat', 'rabbit_boots', 'hemp_shirt', 'hemp_pants',
                 'starter_backpack')
SUPPORTED_KEYS = MATERIAL_KEYS + TOOL_KEYS + WEARABLE_KEYS


def _profile(m, outline, depth, slot, center=(0, 0, 0), bevel=.055):
    """Closed bevelled XY polygon, with the existing concave ear clipper."""
    start = len(m.vertices)
    c.boot_profile(m, outline, depth, slot, bevel)
    for i in range(start, len(m.vertices)):
        m.vertices[i] = b.add(m.vertices[i], center)


def _slab(m, center, size, slot, bevel=.045):
    x, y, z = size
    r = min(x, y) * bevel
    _profile(m, [(-x/2+r, -y/2), (x/2-r, -y/2),
                 (x/2, -y/2+r), (x/2, y/2-r), (x/2-r, y/2),
                 (-x/2+r, y/2), (-x/2, y/2-r), (-x/2, -y/2+r)],
             z, slot, center)


def _sweep(m, points, radius, slot, sides=6):
    """Closed continuous tube; shared rings avoid capped cylinders per segment."""
    rings = []
    for i, point in enumerate(points):
        tangent = b.unit(b.sub(points[min(i+1, len(points)-1)],
                               points[max(0, i-1)]))
        u = b.unit(b.cross(tangent, (0, 0, 1) if abs(tangent[2]) < .9
                           else (0, 1, 0)))
        v = b.cross(tangent, u)
        rings.append([m.v(b.add(point, b.add(
            b.mul(u, radius*math.cos(j*math.tau/sides)),
            b.mul(v, radius*math.sin(j*math.tau/sides)))))
                      for j in range(sides)])
    for a, d in zip(rings, rings[1:]):
        for j in range(sides):
            k = (j+1) % sides
            m.tri(a[j], a[k], d[k], slot)
            m.tri(a[j], d[k], d[j], slot)
    first, last = m.v(points[0]), m.v(points[-1])
    for j in range(sides):
        k = (j+1) % sides
        m.tri(first, rings[0][k], rings[0][j], slot)
        m.tri(last, rings[-1][j], rings[-1][k], slot)


def _oval(m, center, rx, ry, radius, slot, steps=20):
    _sweep(m, [(center[0]+rx*math.cos(i*math.tau/steps),
                center[1]+ry*math.sin(i*math.tau/steps), center[2])
               for i in range(steps+1)], radius, slot)


def _lathe(m, profile, slot, sides=16, center=(0, 0, 0)):
    """Closed revolution about Y; zero-radius endpoints are single poles.

    Profile walks bottom centre -> exterior -> lip/interior -> floor centre.
    It can also describe a solid spindle with two pole endpoints.
    """
    rings = []
    for radius, y in profile:
        rings.append([m.v(b.add(center, (radius*math.cos(i*math.tau/sides),
                                        y, radius*math.sin(i*math.tau/sides))))
                      for i in range(sides)] if radius else
                     [m.v(b.add(center, (0, y, 0)))])
    for a, d in zip(rings, rings[1:]):
        for i in range(sides):
            j = (i+1) % sides
            if len(a) == 1:
                m.tri(a[0], d[i], d[j], slot)
            elif len(d) == 1:
                m.tri(a[i], d[0], a[j], slot)
            else:
                m.tri(a[i], d[i], d[j], slot)
                m.tri(a[i], d[j], a[j], slot)


def _seam(m, a, d, slot=6, count=8, radius=.005):
    for i in range(count):
        t = (i+.5)/count
        p = tuple(a[j]+(d[j]-a[j])*t for j in range(3))
        _sweep(m, [b.add(p, (-.009, -.006, 0)),
                   b.add(p, (.009, .006, 0))], radius, slot, 4)


def _wood(m, key):
    if key == 'pine_panel':
        for i in range(4):
            x = (i-1.5)*.19
            _slab(m, (x, 0, 0), (.182, .68, .075), 2)
            for z in (-.042, .042):
                _sweep(m, [(x-.045, -.27, z), (x-.03, -.06, z),
                           (x-.048, .27, z)], .004, 1, 4)
        # Wooden cross battens and pegs, visible from both broad faces.
        for z in (-.063, .063):
            for y in (-.22, .22):
                _slab(m, (0, y, z), (.79, .073, .045), 1)
                for x in (-.285, .285):
                    m.cylinder((x, y, z-.027), (x, y, z+.027),
                               .014, .014, 6, 3, 3)
        return
    beam = key == 'pine_beam'
    h, depth = (.24, .24) if beam else (.43, .105)
    _slab(m, (0, 0, 0), (.94, h, depth), 2)
    # Exposed stepped tenons communicate milled joinery, rather than bark.
    for x in (-.52, .52):
        _slab(m, (x, 0, 0), (.13, h*.52, depth*.68), 3)
    for z in (-depth/2-.003, depth/2+.003):
        for y in (-h*.29, 0, h*.29):
            _sweep(m, [(-.43, y, z), (-.19, y+.013, z),
                       (.12, y-.01, z), (.43, y+.006, z)], .004, 1, 4)
        _oval(m, (.18, .015, z), .055, h*.12, .005, 1, 12)
    # End-grain arcs lie in the end's YZ plane, on each exposed shoulder.
    for x in (-.471, .471):
        for scale in (.60, .86):
            _sweep(m, [(x, h*.43*scale*math.cos(i*math.tau/12),
                        depth*.43*scale*math.sin(i*math.tau/12))
                       for i in range(13)], .004, 1, 4)
    if beam:
        for x in (-.29, .29):
            _sweep(m, [(x, .132*math.cos(i*math.tau/16),
                        .132*math.sin(i*math.tau/16)) for i in range(17)],
                   .014, 5)


def _cord(m):
    # Two intertwined strands follow a two-turn elliptical coil in XY.
    for strand in range(2):
        points = []
        for i in range(73):
            t = i*math.tau*2/72
            twist = t*7+strand*math.pi
            rx, ry = .25+.035*t/math.tau, .20+.028*t/math.tau
            points.append(((rx+.012*math.cos(twist))*math.cos(t),
                           (ry+.012*math.cos(twist))*math.sin(t),
                           .012*math.sin(twist)+.018*t/math.tau))
        _sweep(m, points, .014, 5+strand, 5)
    _sweep(m, [(.32, 0, .04), (.40, -.13, .025),
               (.33, -.29, -.015), (.19, -.30, -.015)], .024, 5)
    for y in (-.09, -.065, -.04):
        _oval(m, (.34, y, .025), .036, .016, .009, 4, 10)


def _spool(m, nettle):
    k = 13 if nettle else 5
    _lathe(m, [(0, -.33), (.20, -.33), (.215, -.30), (.20, -.265),
               (.105, -.265), (.105, .265), (.20, .265), (.215, .30),
               (.20, .33), (0, .33)], 2, 12)
    _lathe(m, [(0, -.255), (.12, -.255), (.17, -.215), (.17, .215),
               (.12, .255), (0, .255)], k, 16)
    # Fine winding, a restrained cross-wound outer strand, and loose tail.
    _sweep(m, [(.175*math.cos(i*math.tau/12), -.22+i*.44/144,
                .175*math.sin(i*math.tau/12)) for i in range(145)],
           .0045, k+1, 4)
    _sweep(m, [(-.12, -.15, -.123), (.10, .16, -.146),
               (.16, .05, -.09), (.29, -.24, -.03)], .007, k)


def _cloth(m, key):
    fine = key == 'fine_hemp_cloth'
    k = 13 if key == 'nettle_cloth' else 5
    for i in range(3):
        _slab(m, (-.025+i*.025, -.19+i*.12, i*.023),
              (.76-i*.035, .37, .065), k+i%2)
    # A closed draped corner breaks the rectangular stack silhouette.
    _profile(m, [(.14, .21), (.34, .21), (.37, -.11),
                 (.29, -.19), (.23, -.05)], .085, k, (0, 0, -.045))
    for i in range(3):
        cx,cy,cz=-.025+i*.025,-.19+i*.12,i*.023
        for z in (cz-.034,cz+.034):
            _sweep(m, [(cx-.30,cy-.09,z),(cx-.14,cy-.08,z),(cx+.12,cy-.085,z)],
                   .004 if fine else .006,k+1,4)
            if fine:
                _seam(m,(cx-.29,cy+.13,z),(cx+.27,cy+.13,z),7,15,.0025)
                _seam(m,(cx-.30,cy-.13,z),(cx-.30,cy+.12,z),7,10,.0025)


def _hide(m, key):
    leather = 'leather' in key
    fine = key.startswith('fine_')
    k = 10 if leather else 6
    outline = [(-.13, .36), (.13, .36), (.20, .28), (.36, .32),
               (.31, .10), (.36, -.10), (.42, -.31), (.24, -.28),
               (.13, -.36), (-.13, -.36), (-.24, -.28), (-.42, -.31),
               (-.36, -.10), (-.31, .10), (-.36, .32), (-.20, .28)]
    _profile(m, outline, .055 if leather else .09, k)
    for z in (-.05 if leather else -.065, .05 if leather else .065):
        if leather:
            # Folded-over edge and a smooth leather roll, not fluffy hide.
            _slab(m, (.25, -.015, z), (.12, .50, .035), 9)
            _sweep(m, [(.18, -.26, z), (.16, -.05, z), (.18, .23, z)],
                   .011, 11)
        else:
            for x, y in [(-.27, .17), (.27, .17), (-.30, -.19),
                         (.30, -.19), (0, -.26), (0, .25)]:
                c.sphere(m, (x, y, z*.67), (.09, .07, .038), 7, 6, 3)
        if fine:
            _seam(m, (-.21, -.27, z), (.21, -.27, z), 5, 10)
            _seam(m, (-.23, .25, z), (.23, .25, z), 5, 10)
            _seam(m, (-.28, -.17, z), (-.25, .19, z), 5, 8)


def _mineral(m, key):
    if key == 'polished_pebble':
        c.sphere(m, (0, 0, 0), (.34, .26, .20), 19, 16, 8)
        return
    if key in ('limestone_tiles', 'fired_bricks'):
        brick = key == 'fired_bricks'
        for i in range(3):
            _slab(m, ((i%2)*.11-.055, -.18+i*.15, (i%2)*.045),
                  (.64, .13 if brick else .085, .30), 21 if brick else 19)
            if brick:
                for z in (-.157+(i%2)*.045, .157+(i%2)*.045):
                    _slab(m, ((i%2)*.11-.055, -.18+i*.15, z),
                          (.36, .035, .012), 20)
        return
    if key == 'pine_charcoal':
        for x, y, z, scale in [(-.23, -.08, 0, .9), (.14, .015, .035, 1),
                                (.015, -.14, -.13, .72)]:
            start = len(m.faces)
            c.sphere(m, (x, y, z), (.25*scale, .20*scale, .18*scale), 16, 7, 3)
            for i in range(start, len(m.faces)):
                a, d, e, _ = m.faces[i]
                m.faces[i] = (a, d, e, 17 if i%7 == 0 else 16)
            _sweep(m, [(x-.09, y+.08, z-.15*scale),
                       (x-.025, y+.025, z-.18*scale),
                       (x+.07, y+.015, z-.16*scale)], .007, 18, 4)
        return
    flour = key == 'wheat_flour'
    k = 28 if flour else 18
    c.sphere(m, (0, -.12, 0), (.40, .17, .20), k, 12, 4)
    c.sphere(m, (-.055, .015, .015), (.25, .23, .16), k, 10, 4)
    for i in range(8):
        x = -.31+i*.09
        c.sphere(m, (x, -.17+(i%3)*.017, -.13-(i%2)*.025),
                 (.026, .022, .026), 29 if flour else 19, 5, 3)
    if not flour:
        _sweep(m, [(-.17, .015, -.155), (-.04, -.02, -.19),
                   (.13, -.075, -.185)], .012, 17, 5)


def _cup(m):
    _lathe(m, [(0, -.27), (.15, -.27), (.20, -.245), (.24, .19),
               (.255, .245), (.245, .27), (.21, .27), (.203, .235),
               (.17, -.19), (0, -.19)], 21, 18)
    _oval(m, (.255, .015, 0), .135, .17, .032, 20, 18)
    # Foot ring is a solid annulus section, not an open wall.
    _lathe(m, [(.14, -.27), (.16, -.285), (.19, -.27),
               (.18, -.24), (.14, -.27)], 20, 16)


def _braid(m):
    for strand in range(3):
        _sweep(m, [(.048*math.sin(i*.40+strand*math.tau/3),
                    -.39+i*.02, .025*math.cos(i*.40+strand*math.tau/3))
                   for i in range(41)], .023, 13+strand%2, 5)
    for y in (-.35, .35):
        _sweep(m, [(.073*math.cos(i*math.tau/12), y,
                    .05*math.sin(i*math.tau/12)) for i in range(13)], .009, 5, 5)


def _stuffing(m):
    for i in range(9):
        c.sphere(m, ((i%3-1)*.19, (i//3-1)*.12, (i%2)*.025),
                 (.16, .12, .11), 6+i%2, 7, 3)
    for i in range(7):
        x = (i-3)*.08
        _sweep(m, [(x-.04, -.12, -.10), (x+.015, .03, -.12),
                   (x-.025, .17+(i%2)*.035, -.055)], .006, 5, 4)


def _tool(m, key):
    if key == 'pine_axe_normal':
        _sweep(m, [(-.14, -.53, 0), (-.105, -.20, 0),
                   (-.04, .24, 0), (-.025, .45, 0)], .043, 2, 8)
        # Flint head: tapered, asymmetric cutting blade with flaked facets.
        _profile(m, [(-.19, .20), (-.22, .35), (-.15, .43),
                     (.14, .45), (.37, .37), (.40, .17), (.29, .10),
                     (.12, .19)], .11, 18, bevel=.20)
        for z in (-.062, .062):
            for y in (.23, .29, .35, .41):
                _sweep(m, [(-.14, y, z), (.045, y-.02, z)], .014, 5)
            _profile(m, [(.16, .23), (.35, .18), (.33, .32), (.23, .37)],
                     .012, 17, (0, 0, z), .18)
        return
    if key == 'pine_slingshot_normal':
        for sign in (-1, 1):
            _sweep(m, [(0, -.40, 0), (.01, -.04, 0),
                       (sign*.16, .19, 0), (sign*.24, .37, 0)], .042, 2, 8)
            # Leather thong, not modern elastic or a metal fitting.
            _sweep(m, [(sign*.24, .36, 0), (sign*.16, .24, -.09),
                       (sign*.055, .15, -.16)], .018, 9)
            for y in (.29, .32, .35):
                _sweep(m, [(sign*.24-.045, y, -.026),
                           (sign*.24+.045, y+.018, -.026)], .009, 10, 5)
        _slab(m, (0, .15, -.16), (.145, .09, .055), 10)
        for y in (-.32, -.27, -.22, -.17):
            _oval(m, (0, y, 0), .05, .022, .009, 9, 10)
        return
    _sweep(m, [(-.19, -.70, 0), (-.13, -.25, 0), (-.035, .16, 0),
               (.075, .54, 0), (.17, .84, 0)], .018, 2, 8)
    _sweep(m, [(-.19, -.69, 0), (-.15, -.38, 0)], .031, 2, 8)
    for y in (-.62, -.56, -.50, -.44):
        _sweep(m, [(-.205, y, -.027), (-.14, y+.015, -.027)], .009, 5, 5)
    _sweep(m, [(.17, .84, 0), (.36, .52, 0), (.36, -.20, 0)], .007, 6, 5)
    # Carved bone J-hook and inward barb; no reel or metal line guides.
    _sweep(m, [(.36, -.18, 0), (.36, -.30, 0), (.33, -.35, 0),
               (.28, -.35, 0), (.25, -.31, 0), (.26, -.27, 0)], .014, 28)
    _sweep(m, [(.26, -.27, 0), (.285, -.30, 0)], .007, 28, 5)


def _hat(m):
    _lathe(m, [(0, -.10), (.33, -.10), (.36, -.04), (.35, .10),
               (.28, .24), (.13, .31), (0, .32)], 10, 16)
    # Thick continuous fur band and shaped hanging ear flaps.
    _lathe(m, [(0, -.11), (.35, -.11), (.385, -.07), (.385, .015),
               (.35, .045), (0, .045)], 6, 16)
    for sign in (-1, 1):
        _profile(m, [(sign*x, y) for x, y in
                     [(.25, -.04), (.36, -.06), (.365, -.22),
                      (.32, -.30), (.25, -.26), (.23, -.13)]], .22, 6)
        _sweep(m, [(sign*.30, -.26, -.03), (sign*.29, -.35, -.04),
                   (sign*.22, -.40, -.04)], .014, 9)
    for z in (-.285, .285):
        _sweep(m, [(-.22, .10, z*.81), (0, .26, z*.50),
                   (.22, .10, z*.81)], .008, 9)
        _seam(m, (-.23, -.02, z), (.23, -.02, z), 7, 12)


def _shirt(m):
    # Neck notch, sloped shoulders, angled sleeves and a subtly flared hem.
    outline = [(-.28, -.36), (.28, -.36), (.25, .03), (.40, -.015),
               (.51, .18), (.31, .32), (.13, .35), (.08, .255),
               (-.08, .255), (-.13, .35), (-.31, .32), (-.51, .18),
               (-.40, -.015), (-.25, .03)]
    _profile(m, outline, .18, 5)
    for z in (-.103, .103):
        _sweep(m, [(-.13, .34, z), (-.08, .25, z),
                   (0, .23, z), (.08, .25, z), (.13, .34, z)], .014, 9)
        _sweep(m, [(0, .23, z), (0, .02, z)], .009, 4)
        _seam(m, (-.23, -.32, z), (.23, -.32, z), 6, 12)
        for sign in (-1, 1):
            _sweep(m, [(sign*.42, .005, z), (sign*.48, .15, z)], .012, 6)
    for x in (-.145, .145):
        _profile(m, [(-.085, .08), (.085, .08), (.08, -.09),
                     (0, -.13), (-.08, -.09)], .035, 10, (x, -.10, -.115))
        _seam(m, (x-.065, -.17, -.138), (x+.065, -.17, -.138), 6, 6)


def _pants(m):
    _profile(m, [(-.29, .40), (.29, .40), (.30, .17), (.28, -.40),
                 (.07, -.40), (.055, .07), (0, .13), (-.055, .07),
                 (-.07, -.40), (-.28, -.40), (-.30, .17)], .19, 5)
    for z in (-.108, .108):
        _slab(m, (0, .34, z), (.59, .065, .025), 4)
        for sign in (-1, 1):
            x = sign*.175
            _slab(m, (x, -.365, z), (.21, .048, .025), 6)
            _sweep(m, [(sign*.26, .23, z), (sign*.24, -.08, z),
                       (sign*.24, -.33, z)], .006, 6, 4)
            _profile(m, [(-.075, .07), (.075, .07), (.065, -.065),
                         (0, -.10), (-.065, -.065)], .025, 5, (x, .19, z))
            _seam(m, (x-.055, .245, z*1.16),
                  (x+.055, .245, z*1.16), 6, 5)
    _sweep(m, [(0, .32, -.13), (0, .16, -.13)], .008, 4)
    for x in (-.055, .055):
        _sweep(m, [(x, .35, -.14), (x*.8, .26, -.14)], .009, 5)
        _slab(m, (x, .255, -.14), (.055, .017, .025), 2)


def _backpack(m):
    _profile(m, [(-.26, -.34), (.26, -.34), (.30, -.25),
                 (.285, .22), (.22, .32), (-.22, .32),
                 (-.285, .22), (-.30, -.25)], .29, 5, bevel=.12)
    _profile(m, [(-.30, .30), (.30, .30), (.28, .12),
                 (.20, .055), (-.20, .055), (-.28, .12)], .07, 9,
             (0, 0, -.16))
    _slab(m, (0, -.17, -.185), (.32, .23, .10), 10, .14)
    _slab(m, (0, -.085, -.24), (.34, .065, .035), 9)
    for x in (-.18, .18):
        _slab(m, (x, -.005, -.205), (.045, .40, .025), 9)
        _slab(m, (x, -.02, -.225), (.067, .045, .02), 10)
        _slab(m, (x, -.02, -.24), (.035, .012, .012), 3)
    # Visible branch frame and arched leather shoulder straps on reverse.
    for x in (-.24, .24):
        _sweep(m, [(x, -.37, .17), (x, .36, .17)], .022, 2, 7)
        _sweep(m, [(x, .29, .17), (x*.8, .23, .30),
                   (x*.75, -.10, .32), (x, -.28, .17)], .026, 9)
    for y in (-.27, .25):
        _sweep(m, [(-.25, y, .17), (.25, y, .17)], .021, 2, 7)
    _sweep(m, [(-.10, .30, .10), (-.10, .43, .10),
               (.10, .43, .10), (.10, .30, .10)], .023, 9)
    for z, y in [(-.245, -.255), (.152, -.29)]:
        _seam(m, (-.13, y, z), (.13, y, z), 6, 10)


def build(key: str, name: str) -> b.Model | None:
    """Return an UNSAVED display model, or None for shared/unrelated keys."""
    if key not in SUPPORTED_KEYS:
        return None  # Includes pine_log: harvest geometry is shared.
    m = b.Model(name)
    if key in ('pine_plank', 'pine_beam', 'pine_panel'):
        _wood(m, key)
    elif key == 'hemp_cord':
        _cord(m)
    elif key in ('hemp_thread', 'nettle_thread'):
        _spool(m, key == 'nettle_thread')
    elif key in ('hemp_cloth', 'nettle_cloth', 'fine_hemp_cloth'):
        _cloth(m, key)
    elif key in ('cured_rabbit_pelt', 'fine_rabbit_pelt',
                 'rabbit_leather', 'fine_rabbit_leather'):
        _hide(m, key)
    elif key == 'plant_stuffing':
        _stuffing(m)
    elif key == 'reed_braid':
        _braid(m)
    elif key == 'clay_cup':
        _cup(m)
    elif key in TOOL_KEYS:
        _tool(m, key)
    elif key == 'rabbit_boots':
        c.rabbit_boot(m)
    elif key == 'rabbit_hat':
        _hat(m)
    elif key == 'hemp_shirt':
        _shirt(m)
    elif key == 'hemp_pants':
        _pants(m)
    elif key == 'starter_backpack':
        _backpack(m)
    else:
        _mineral(m, key)
    return m
