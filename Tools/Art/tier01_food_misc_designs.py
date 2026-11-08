"""Unsaved tier-one Food/Misc geometry; orchestration belongs to the caller.

Indices refer to generate_tier01_crafted_items.COLORS, never the region palette.
No generator globals are changed. Vessels are illustration, not recipe inputs.
PALETTE_NOTES documents context-dependent semantics for the parent's remapper.
"""
import math

import generate_region01_meshes as b
import generate_tier01_crafted_items as c
from generate_region05_meshes import sphere


FOOD_KEYS = (
    'apple_juice', 'roasted_mushrooms', 'berry_oat_cakes', 'roasted_rabbit',
    'herb_trout', 'roasted_rainbow_trout', 'roasted_largemouth_bass',
    'roasted_bluegill', 'roasted_yellow_perch', 'strawberry_cow_yogurt_drink',
    'cow_cheese_tart', 'cow_cream', 'cow_butter', 'cow_cheese', 'goat_cheese',
    'mixed_milk_cheese', 'cow_yogurt', 'toasted_pine_nuts', 'roasted_shrimp',
    'shrimp_pie', 'apple_oat_compote', 'red_berry_oat_porridge',
    'premium_apple_pie',
)
MISC_KEYS = ('pine_bird', 'cord_charm', 'rabbit_toy', 'pinecone_ornament',
             'pinecone_rabbit')
EXTERNAL_KEYS = ('cow_milk', 'goat_milk', 'egg', 'shrimp')
SUPPORTED_KEYS = FOOD_KEYS + MISC_KEYS + EXTERNAL_KEYS

PALETTE_NOTES = {
    '0-3': 'wood/skewers/carving; 0 also carved eye recesses',
    '5-7': 'hemp cord, toy fur/stitching, oat flakes',
    '8-11': 'toy seam/ear inset; mushroom gills use 8',
    '12-15': 'tea herbs ONLY on herb_trout',
    '16-19': 'char, eyes, stone; goat milk jug uses 19/18',
    '20-23': 'terracotta vessels, baked crust, roast flesh, toasted nuts',
    '24-26': 'berry/strawberry/apple peel or cooked shrimp; 24 char on shrimp',
    '27': 'cream/custard/cheese interior; apple flesh in apple recipes',
    '28': 'milk/yogurt/egg white and goat cheese; 27 is its helper shadow',
    '29-30': 'butter/cow cheese; 29 also apple juice and milk-oat base',
    '31': 'unused',
}


def _lathe(m, profile, k, sides=16, center=(0, 0, 0)):
    """Closed revolution, including real inner walls when in the profile.

    Axis endpoints become single cap vertices, avoiding degenerate pole faces.
    Profiles run bottom axis -> exterior -> lip/interior -> upper axis.
    """
    rings = []
    for radius, y in profile:
        rings.append([m.v((center[0] + radius * math.cos(i * math.tau / sides),
                           center[1] + y,
                           center[2] + radius * math.sin(i * math.tau / sides)))
                      for i in range(sides)] if radius else
                     [m.v((center[0], center[1] + y, center[2]))])
    for a, d in zip(rings, rings[1:]):
        for i in range(sides):
            j = (i + 1) % sides
            if len(a) == 1:
                m.tri(a[0], d[i], d[j], k)
            elif len(d) == 1:
                m.tri(a[i], d[0], a[j], k)
            else:
                m.tri(a[i], d[j], a[j], k)
                m.tri(a[i], d[i], d[j], k)


def _rim(m, radius, y, thickness, k, center=(0, 0), steps=24):
    """Closed torus: 6 cross-section sides, shared ring vertices."""
    rings = []
    for i in range(steps):
        a = i * math.tau / steps
        rings.append([m.v((center[0] + (radius + thickness * math.cos(t)) * math.cos(a),
                           y + thickness * math.sin(t),
                           center[1] + (radius + thickness * math.cos(t)) * math.sin(a)))
                      for t in (j * math.tau / 6 for j in range(6))])
    for i in range(steps):
        for j in range(6):
            a, d = rings[i], rings[(i + 1) % steps]
            m.tri(a[j], d[(j + 1) % 6], d[j], k)
            m.tri(a[j], a[(j + 1) % 6], d[(j + 1) % 6], k)


def _bowl(m, filling, radius=.34):
    _lathe(m, [(0, -.19), (.19, -.19), (.24, -.15), (radius, .09),
               (radius, .13), (radius-.035, .13), (.21, -.10), (0, -.10)], 20)
    _rim(m, radius-.015, .13, .018, 21)
    _lathe(m, [(0, -.08), (.20, -.08), (radius-.052, .085), (0, .085)], filling)


def _flakes(m, y, radius=.24, count=14):
    for i in range(count):
        a = i * 2.39996
        r = radius * math.sqrt((i+.5)/count)
        sphere(m, (r*math.cos(a), y + .008*(i % 3), r*math.sin(a)),
               (.032, .012, .019), 6+i % 2, 6, 3)


def _apple_piece(m, pos, size=.09):
    # Cooked flesh with a small red peel strip, no added garnish.
    sphere(m, pos, (size, size*.55, size*.65), 27, 7, 3)
    x, y, z = pos
    sphere(m, (x, y+.006, z-size*.48), (size*.83, size*.33, size*.12), 25, 7, 3)


def _pie(m, key):
    # One individual pie/tart, never multiple outputs despite recipe Yield.
    _lathe(m, [(0, -.16), (.25, -.16), (.33, -.10), (.37, .08),
               (.35, .12), (.31, .07), (.27, -.09), (0, -.09)], 21, 20)
    fill = 29 if key == 'premium_apple_pie' else 27
    _lathe(m, [(0, -.08), (.27, -.08), (.315, .075), (0, .075)], fill, 20)
    _rim(m, .34, .105, .025, 23, steps=28)
    for i in range(20):
        a = i*math.tau/20
        sphere(m, (.342*math.cos(a), .12, .342*math.sin(a)),
               (.031, .019, .031), 22, 5, 3)
    if key == 'premium_apple_pie':
        for i in range(7):
            a = i*2.4
            _apple_piece(m, (.19*math.cos(a), .095, .19*math.sin(a)), .075)
        for x in (-.20, -.07, .07, .20):
            length = 2*math.sqrt(.30**2-x*x)
            c.box(m, (x, .145, 0), (.035, .025, length), 22)
            c.box(m, (0, .17, x), (length, .025, .035), 23)
    elif key == 'shrimp_pie':
        # A single visible curled shrimp embedded in buttery filling.
        c.shrimp(m, (0, .105, -.04), .67, 26)
        for x, z in ((-.17, .12), (.14, .15), (.04, -.20)):
            sphere(m, (x, .10, z), (.045, .015, .03), 29, 6, 3)
    else:
        # Egg is baked into custard, cheese remains visible; no herbs or berries.
        for x, z in ((-.13, -.11), (.13, -.09), (0, .15)):
            c.box(m, (x, .105, z), (.09, .035, .07), 29)
        for x, z in ((-.18, .08), (.08, .05), (.18, .12), (0, -.18)):
            sphere(m, (x, .089, z), (.025, .006, .021), 22, 6, 3)


def _drink(m, key):
    filling = 29 if key == 'apple_juice' else 27 if key == 'cow_cream' else 28
    # Strawberry milk is pink using the original salmon slot, not extra fruit.
    if key == 'strawberry_cow_yogurt_drink':
        filling = 26
    _lathe(m, [(0, -.32), (.16, -.32), (.20, -.27), (.21, .23),
               (.215, .29), (.18, .29), (.17, -.25), (0, -.25)], 20)
    _rim(m, .198, .29, .019, 21, steps=20)
    _lathe(m, [(0, -.24), (.165, -.24), (.177, .235), (0, .235)], filling)
    if key == 'cow_cream':
        # Thick cream forms a raised soft spiral instead of flat liquid.
        points = []
        for i in range(25):
            a = i*math.tau/12
            r = .12*(1-i/28)
            points.append((r*math.cos(a), .24+i*.0025, r*math.sin(a)))
        # Keep cream in 27/28: c.tube(27) would shade into red slot 26.
        start = len(m.faces)
        c.tube(m, points, .019, 28)
        m.faces[start:] = [(a, d, e, 27) for a, d, e, _ in m.faces[start:]]
    elif key == 'cow_yogurt':
        sphere(m, (0, .24, 0), (.13, .038, .12), 28, 10, 3)
        _rim(m, .08, .263, .009, 27, steps=16)
    elif key == 'strawberry_cow_yogurt_drink':
        for i in range(7):
            a = i*2.4
            sphere(m, (.12*math.cos(a), .244, .12*math.sin(a)),
                   (.014, .006, .012), 25, 5, 3)


def _cheese(m, key):
    if key == 'goat_cheese':
        # Small tapered chevre log; a closed creamy body with subtle rind rings.
        _lathe(m, [(0, -.24), (.17, -.24), (.19, -.20), (.17, .20),
                   (.15, .24), (0, .24)], 28, 14)
        for y in (-.15, -.04, .07, .17):
            _rim(m, .18-(y+.2)*.05, y, .007, 27, steps=16)
    elif key == 'cow_cheese':
        # Recognisable cut wedge with closed triangular end faces.
        outline = [(0, -.28), (.30, .16), (.16, .28), (-.16, .28), (-.30, .16)]
        rings = [[m.v((x, y, z)) for x, z in outline] for y in (-.12, .12)]
        for i in range(5):
            j = (i+1) % 5
            k = 30 if i in (0, 4) else 29
            m.tri(rings[0][i], rings[1][j], rings[0][j], k)
            m.tri(rings[0][i], rings[1][i], rings[1][j], k)
        for y, ring in ((-.12, rings[0]), (.12, rings[1])):
            mid = m.v((0, y, .05))
            for i in range(5):
                a, d = ring[i], ring[(i+1) % 5]
                m.tri(mid, a if y < 0 else d, d if y < 0 else a, 30)
        for x, z, r in ((0, -.10, .035), (.10, .10, .045), (-.12, .13, .027)):
            sphere(m, (x, .121, z), (r, .003, r*.7), 29, 8, 3)
    else:
        _lathe(m, [(0, -.14), (.25, -.14), (.29, -.10), (.29, .10),
                   (.25, .14), (0, .14)], 29, 18)
        _lathe(m, [(0, .141), (.23, .141), (.23, .146), (0, .146)], 28, 18)
        _rim(m, .26, .115, .012, 27, steps=20)
        for x in (-.12, 0, .12):
            c.tube(m, [(x, .15, -.15), (x+.025, .15, .15)], .009, 30)


def _roast(m, key):
    if key == 'roasted_rabbit':
        # A single boneless roast portion: capped body and folded haunch lobes.
        sphere(m, (0, .02, 0), (.30, .19, .18), 21, 12, 5)
        for x in (-.22, .22):
            sphere(m, (x, -.06, -.025), (.15, .12, .14), 22, 9, 4)
        for x in (-.14, 0, .14):
            c.tube(m, [(x-.035, .12, -.12), (x, .18, -.055),
                       (x+.035, .19, .035)], .009, 20)
        c.tube(m, [(-.48, -.10, .02), (.48, -.10, .02)], .022, 2)
    elif key == 'roasted_shrimp':
        c.shrimp(m, k=26)
        for i in range(4):
            a = (i*31+46)*math.pi/180
            sphere(m, (.27*math.cos(a), .22*math.sin(a), -.062),
                   (.032, .014, .006), 24, 6, 3)
        c.tube(m, [(-.4, -.16, .07), (.4, -.16, .07)], .02, 2)
    elif key == 'roasted_mushrooms':
        # One serving on a branch skewer; cooked caps have closed undersides.
        for x, y in ((-.20, -.01), (.04, .03), (.25, -.025)):
            m.cylinder((x, y-.18, 0), (x, y, 0), .035, .043, 8, 28, 28)
            sphere(m, (x, y+.035, 0), (.135, .09, .12), 21, 10, 4)
            _lathe(m, [(0, 0), (.115, 0), (.11, .01), (0, .01)],
                   8, 12, (x, y-.005, 0))
            for dx in (-.05, .025):
                c.tube(m, [(x+dx, y+.09, -.04), (x+dx+.03, y+.11, .025)], .007, 20)
        c.tube(m, [(-.44, -.14, 0), (.44, -.14, 0)], .018, 2)
    else:
        c.fish(m, key)
        # Keep exact species mesh; add baked skin/scoring, no non-recipe garnish.
        lo = [min(v[i] for v in m.vertices) for i in range(3)]
        hi = [max(v[i] for v in m.vertices) for i in range(3)]
        for t in (.39, .50, .61):
            x = lo[0]+(hi[0]-lo[0])*t
            y = lo[1]+(hi[1]-lo[1])*.56
            z = lo[2]-.004
            c.tube(m, [(x-.025, y-.04, z), (x+.025, y+.045, z)], .009, 20)


def _cone(m, center, scale=1):
    x, y, z = center
    sphere(m, center, (.16*scale, .26*scale, .14*scale), 1, 8, 4)
    for row in range(5):
        width = (.10, .17, .18, .14, .08)[row]*scale
        for i in range(7):
            a = (i + .5*(row % 2))*math.tau/7
            # Individual pointed, closed scales rather than smooth ball clusters.
            p = (x+width*math.cos(a), y+(-.19+row*.095)*scale,
                 z+width*.86*math.sin(a))
            m.cylinder(p, (p[0]+.035*scale*math.cos(a), p[1]+.075*scale,
                           p[2]+.03*scale*math.sin(a)), .052*scale, 0, 5, 2, 3)


def _misc(m, key):
    if key == 'pine_bird':
        sphere(m, (0, 0, 0), (.25, .17, .14), 2, 10, 4)
        sphere(m, (.20, .19, 0), (.12, .13, .105), 3, 8, 4)
        m.cylinder((.28, .19, 0), (.43, .17, 0), .05, 0, 5, 2, 3)
        m.cylinder((-.17, .015, 0), (-.42, .14, 0), .10, .025, 5, 2, 3)
        sphere(m, (-.03, .01, -.13), (.18, .105, .025), 1, 8, 3)
        for i in range(3):
            c.tube(m, [(-.17+i*.055, -.025, -.151), (-.02+i*.055, .065, -.151)], .007, 3)
        sphere(m, (.235, .22, -.097), (.016, .016, .009), 0, 6, 3)
        c.box(m, (0, -.23, 0), (.40, .065, .27), 1)
        for x in (-.07, .08):
            m.cylinder((x, -.2, 0), (x, -.12, 0), .025, .025, 6, 2, 3)
        for z in (-.07, 0, .07):
            c.tube(m, [(-.14, -.264, z), (.14, -.264, z)], .005, 3)
    elif key == 'cord_charm':
        c.loop(m, (0, .17, 0), .20, .27, .022, 5, steps=24)
        sphere(m, (0, -.22, -.015), (.145, .18, .08), 19, 12, 5)
        # Actual cord cage around the polished pebble, including back strap.
        for z in (-.085, .065):
            c.tube(m, [(-.11, -.10, z), (0, -.37, z), (.11, -.10, z)], .012, 5)
        c.tube(m, [(-.12, -.19, -.07), (0, -.23, -.10), (.12, -.19, -.07)], .013, 5)
        for y in (-.03, 0, .03):
            _rim(m, .033, y, .009, 6, steps=12)
    elif key == 'rabbit_toy':
        sphere(m, (0, -.08, 0), (.23, .27, .16), 6, 12, 5)
        sphere(m, (0, .21, -.02), (.18, .17, .15), 7, 10, 4)
        for x in (-.085, .085):
            sphere(m, (x, .44, 0), (.06, .20, .04), 6, 8, 4)
            sphere(m, (x, .46, -.035), (.028, .135, .009), 10, 6, 3)
            # Embroidered eyes instead of introduced glass/button ingredients.
            c.tube(m, [(x-.018, .245, -.165), (x+.018, .215, -.169)], .009, 8)
            c.tube(m, [(x-.018, .215, -.169), (x+.018, .245, -.165)], .009, 8)
        for x in (-.18, .18):
            sphere(m, (x, -.27, -.05), (.105, .085, .14), 6, 8, 4)
            sphere(m, (x, -.035, -.015), (.09, .15, .09), 7, 8, 4)
        sphere(m, (0, -.15, .17), (.075, .07, .06), 7, 8, 3)
        c.tube(m, [(-.025, .16, -.17), (0, .14, -.183), (.025, .16, -.17)], .009, 8)
        c.tube(m, [(0, -.24, -.10), (0, -.14, -.16), (0, 0, -.16), (0, .09, -.12)], .006, 9)
        for y in (-.18, -.12, -.06, 0):
            c.tube(m, [(-.023, y-.008, -.164), (.023, y+.008, -.164)], .006, 5)
    elif key == 'pinecone_ornament':
        # Both cones are ingredients; hanger is bent branch, not invented cord.
        _cone(m, (-.16, -.08, 0), .78)
        _cone(m, (.17, -.02, .015), .85)
        c.tube(m, [(-.28, .22, 0), (.26, .26, 0)], .026, 2)
        c.tube(m, [(-.16, .11, 0), (-.16, .23, 0)], .019, 2)
        c.tube(m, [(.17, .18, .015), (.17, .25, 0)], .019, 2)
        c.tube(m, [(0, .24, 0), (0, .37, 0), (.07, .42, 0), (.12, .36, 0)], .019, 2)
    else:
        _cone(m, (0, -.04, 0))
        sphere(m, (0, .23, -.015), (.14, .12, .105), 3, 9, 4)
        for x in (-.075, .075):
            sphere(m, (x, .42, 0), (.042, .18, .035), 3, 7, 4)
            c.tube(m, [(x, .32, -.036), (x, .53, -.025)], .007, 1)
            sphere(m, (x, .25, -.105), (.013, .014, .006), 0, 6, 3)
            sphere(m, (x*1.6, -.28, -.05), (.10, .05, .13), 3, 7, 3)
        sphere(m, (0, .195, -.12), (.027, .021, .018), 1, 6, 3)
        c.loop(m, (0, .12, -.02), .15, .035, .012, 5, steps=16)
        c.tube(m, [(0, .10, -.13), (-.075, .04, -.16)], .012, 5)
        c.tube(m, [(0, .10, -.13), (.07, .025, -.16)], .012, 5)


def build(key: str, name: str) -> b.Model | None:
    """Return a fresh unsaved model, or None for unrelated styles (incl. mushrooms).

    Bounds and semantic palette remapping are deliberately left to the caller.
    Only c.fish reads an asset: the current raw species OBJ, never recovery data.
    """
    if key not in SUPPORTED_KEYS:
        return None
    m = b.Model(name)
    if key in MISC_KEYS:
        _misc(m, key)
    elif key in ('cow_milk', 'goat_milk'):
        c.milk_jug(m, goat=key == 'goat_milk')
        _rim(m, .115, .39, .014, 19 if key == 'goat_milk' else 28, steps=16)
    elif key == 'egg':
        _lathe(m, [(0, -.25), (.105, -.21), (.16, -.11), (.17, .01),
                   (.14, .14), (.08, .23), (0, .27)], 28, 16)
    elif key == 'shrimp':
        c.shrimp(m, k=19)
    elif key in ('apple_juice', 'cow_cream', 'cow_yogurt', 'strawberry_cow_yogurt_drink'):
        _drink(m, key)
    elif key in ('cow_cheese', 'goat_cheese', 'mixed_milk_cheese'):
        _cheese(m, key)
    elif key == 'cow_butter':
        _lathe(m, [(0, -.10), (.22, -.10), (.24, -.07), (.24, .07),
                   (.20, .11), (0, .11)], 29, 12)
        # Churned scoop and shallow spread grooves distinguish it from cheese.
        sphere(m, (.035, .12, -.015), (.16, .055, .11), 30, 10, 4)
        for x in (-.06, 0, .06):
            c.tube(m, [(x, .15, -.07), (x+.025, .16, .06)], .006, 29)
    elif key in ('premium_apple_pie', 'cow_cheese_tart', 'shrimp_pie'):
        _pie(m, key)
    elif key in ('apple_oat_compote', 'red_berry_oat_porridge'):
        _bowl(m, 29 if key == 'apple_oat_compote' else 28)
        _flakes(m, .10)
        if key == 'apple_oat_compote':
            for x, z in ((-.13, -.09), (.10, -.11), (.02, .13)):
                _apple_piece(m, (x, .14, z), .09)
        else:
            for i in range(7):
                a = i*2.4
                sphere(m, (.19*math.cos(a), .13, .19*math.sin(a)),
                       (.045, .037, .041), 24+i % 2, 7, 3)
    elif key == 'berry_oat_cakes':
        _lathe(m, [(0, -.07), (.24, -.07), (.28, -.03), (.27, .04),
                   (.23, .07), (0, .07)], 22, 16)
        _flakes(m, .075, .22, 18)
        for i in range(6):
            a = i*2.4
            sphere(m, (.18*math.cos(a), .075, .18*math.sin(a)),
                   (.035, .018, .03), 24, 7, 3)
    elif key == 'toasted_pine_nuts':
        _bowl(m, 22, .30)
        for i in range(18):
            a = i*2.4
            r = .21*math.sqrt((i+.5)/18)
            x, z = r*math.cos(a), r*math.sin(a)
            sphere(m, (x, .12+(i % 3)*.012, z), (.025, .022, .060), 22+i % 2, 6, 3)
    else:
        _roast(m, key)
    return m
