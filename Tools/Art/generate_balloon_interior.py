"""Balloon shop interior (Milestone 14 slice 3, task 7): the merchant's cabin seen from the customer side of its counter.
Faceted style on the storefront kit (generate_storefront_meshes / storefront_shops helpers), its own palette strip with
surface-detail types in alpha. Art only: no shop logic.

Look: the exterior balloon's crimson lacquer, brass trim and warm paper lanterns (generate_balloon_merchant COLORS), as
a cabin room: lacquer wall bays between dark rosewood posts, brass rails and a brass coin medallion over the merchant,
round sky portholes, curio shelves, ballast sacks and rigging rope. The counter is crimson lacquer with brass pilasters
and edge banding under a rosewood top at 1.02 m; three gold-edged silk mats mark the offer spots, clear of the merchant.

Same room envelope and roof beams as the storefront counters (room() in storefront_shops), so CounterLamps.Hang's
default hang points and ShopCounterPose's framing carry over. The keeper stands at the origin facing +Z.

Unity's OBJ import mirrors X: the offer anchors are written in Unity space and the mats are drawn at -x here.
Run: python Tools/Art/generate_balloon_interior.py   (writes only Assets/_Game/Art/Settlement/Balloon)
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k
from storefront_shops import prism, ring_band, crate, bolt
from generate_storefront_meshes import sack

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Settlement/Balloon'
HEADER = 'The Trail Walker balloon interior (faceted); metres; +Y up; +Z toward the customer camera.'
T = 1.02                    # counter top
MAT = .008                  # silk mat thickness: the offer anchors stand on it
# Unity space. The storefront spots spread wider: the balloon's garment, box and pack are bulkier than shop goods.
OFFERS = [(-1.25, T+MAT, .48), (.80, T+MAT, .48), (1.55, T+MAT, .48)]
CW = 3.8                    # counter width
MAX_TRIS = 12000            # Milestone14BalloonValidation.InteriorPrefab's cap

pal = k.Palette()
for name, color, *detail in [
    # From the exterior balloon (generate_balloon_merchant COLORS): lacquer, rosewood, brass, lantern paper.
    ('LacquerDark', '#701D19'), ('Lacquer', '#9C271E'), ('LacquerLight', '#BF3523'), ('LacquerHi', '#D64C2A'),
    ('Wood', '#3F231B', 'wood'), ('WoodMid', '#5F3521', 'planks'), ('WoodLight', '#854F2A', 'planks'),
    ('Brass', '#D9A441', 'metal'), ('BrassDark', '#AD772D', 'metal'), ('BrassHi', '#F7CC6E', 'metal'),
    ('Paper', '#EF9B36', 'paper'), ('PaperGlow', '#FFC859', 'paper'), ('PaperHot', '#FFE391'),
    ('Silk', '#E6CD96', 'cloth'), ('Rope', '#BA9052', 'plant_fibre'),
    ('Sky', '#7EACCE'), ('SkyLight', '#B0D0E4'), ('Cloud', '#F0F4F6'),
    ('Jade', '#5A9678', 'ceramic'), ('Teal', '#2E6A6D', 'cloth'),
    # Names the shared storefront helpers draw with (sack, crate, bolt).
    ('Sack', '#CDBB93', 'cloth'), ('SackDark', '#A8966E', 'cloth'), ('Timber', '#5A3D27', 'wood'),
    ('Plank', '#854F2A', 'planks'), ('PlankDark', '#5F3521', 'planks'), ('Iron', '#3A3735')]:
    pal.add(name, color, *detail)

def box(m, c, size, color, **kw): k.box(m, c, size, color, **kw)

def disc(m, c, r, z0, z1, color, n=12):
    """Polygon in the XY plane extruded from z0 to z1 (porthole glass, medallion faces)."""
    prism(m, [(c[0]+r*math.cos(i*math.tau/n), c[1]+r*math.sin(i*math.tau/n)) for i in range(n)], z0, z1, color)

def ring_xy(m, c, r, z, thick, depth, color, n=12):
    """Ring of tangential bars in the XY plane (brass porthole and medallion frames)."""
    for i in range(n):
        a = (i+.5)*math.tau/n; L = 2*r*math.sin(math.pi/n)+thick*.6
        box(m, (c[0]+r*math.cos(a), c[1]+r*math.sin(a), z), (L, thick, depth), color,
            axes=((-math.sin(a), math.cos(a), 0), (math.cos(a), math.sin(a), 0), (0, 0, 1)))

def rope(m, a, b, r=.018):
    k.limb(m, [(a, r, r), (b, r, r)], 'Rope', sides=4, cap_start=False, cap_end=False)

def paper_lantern(m, c, r, h, string_top):
    """Faceted paper lantern: glowing belly, lacquer bands, rosewood caps, a red tassel and its cord."""
    x, y, z = c
    k.vertical_loft(m, [(y-h/2, r*.5, r*.5, r*.5), (y-h*.28, r*.92, r*.92, r*.92), (y, r, r, r), (y+h*.28, r*.92, r*.92, r*.92), (y+h/2, r*.5, r*.5, r*.5)],
                    'Paper', a=.7, b=.7, x=x, zc=z, colors=['Paper', 'PaperGlow', 'PaperGlow', 'Paper'])
    for t in (-.28, .28): ring_band(m, (x, 0, z), y+h*t, r*.93, 'LacquerDark', h=.025)
    for s in (-1, 1): box(m, (x, y+s*(h/2+.02), z), (r*1.05, .04, r*1.05), 'Wood')
    box(m, (x, y-h/2-.08, z), (.03, .1, .03), 'LacquerLight', taper=1.6)
    box(m, (x, (y+h/2+string_top)/2, z), (.012, string_top-y-h/2, .012), 'Rope')

def medallion(m, c, r, z):
    """Brass coin motif (the exterior cabin's side medallion): rim, square hole frame and cross bars on a lacquer disc."""
    disc(m, c, r*.98, z-.02, z+.01, 'LacquerLight', 16)
    ring_xy(m, c, r, z+.02, .045, .04, 'Brass', 16)
    ring_xy(m, c, r*.62, z+.02, .025, .03, 'BrassDark', 12)
    for dx, dy, w, h in ((0, r*.18, r*.36, .03), (0, -r*.18, r*.36, .03), (r*.18, 0, .03, r*.36), (-r*.18, 0, .03, r*.36)):
        box(m, (c[0]+dx, c[1]+dy, z+.03), (w, h, .03), 'BrassHi')

def porthole(m, c, r, z):
    disc(m, c, r, z-.01, z+.01, 'Sky', 12)
    disc(m, (c[0]-.06, c[1]+.13, 0), r*.42, z+.012, z+.018, 'SkyLight', 8)
    for dx, dy, s in ((-.08, -.08, .12), (.1, -.12, .08), (.0, -.02, .1)):   # a low cloud bank
        box(m, (c[0]+dx, c[1]+dy, z+.024), (s*1.8, s*.7, .01), 'Cloud', taper=.7)
    ring_xy(m, c, r+.03, z+.03, .07, .08, 'Brass', 12)
    for i in range(4):   # rivets
        a = i*math.tau/4+math.pi/4; box(m, (c[0]+(r+.03)*math.cos(a), c[1]+(r+.03)*math.sin(a), z+.075), (.035, .035, .02), 'BrassHi')

def chest(m, c, s, color='Lacquer'):
    box(m, (c[0], c[1]+s[1]/2, c[2]), s, color)
    box(m, (c[0], c[1]+s[1]*.72, c[2]+.002), (s[0]+.012, .025, s[2]+.012), 'Brass')
    for dx in (-s[0]*.32, s[0]*.32): box(m, (c[0]+dx, c[1]+s[1]/2, c[2]+.002), (.025, s[1]+.01, s[2]+.012), 'BrassDark')
    box(m, (c[0], c[1]+s[1]*.6, c[2]+s[2]/2+.01), (.05, .06, .02), 'BrassHi')

def jar(m, c, r, h, color='Jade'):
    k.vertical_loft(m, [(c[1], r*.6, r*.6, r*.6), (c[1]+h*.45, r, r, r), (c[1]+h*.85, r*.55, r*.55, r*.55), (c[1]+h, r*.6, r*.6, r*.6)],
                    color, a=.65, b=.6, x=c[0], zc=c[2])
    box(m, (c[0], c[1]+h+.02, c[2]), (r*.9, .04, r*.9), 'Brass')

# ------------------------------------------------------------------ room

POSTS = (-3.3, -2.3, -.9, .9, 2.3, 3.3)
WALL_Z = -1.90               # back wall face

def room(m):
    for a in range(14): box(m, (-3.25+a*.5, -.03, -.4), (.49, .06, 3.6), ('WoodMid', 'WoodLight')[a % 2])
    box(m, (0, 1.6, -1.95), (6.8, 3.2, .1), 'Lacquer')
    for s in (-1, 1): box(m, (s*3.4, 1.6, -.4), (.1, 3.2, 3.2), 'Lacquer')
    # Rosewood ceiling and the storefront beam layout (CounterLamps hangs under the z -0.6 beam, 0.25 m below the top).
    box(m, (0, 3.25, -.4), (6.9, .1, 3.2), 'Wood')
    for z in (-1.6, -.6, .4): box(m, (0, 3.13, z), (6.8, .16, .18), 'WoodMid')
    # Back wall: rosewood wainscot, lacquer bays between posts, brass rails (the exterior's rail heights, scaled to the room).
    box(m, (0, .45, WALL_Z+.02), (6.8, .9, .04), 'Wood')
    for x in (-2.8, -1.6, 0, 1.6, 2.8): box(m, (x, .45, WALL_Z+.05), ({-2.8: .8, 2.8: .8, 0: 1.5}.get(x, 1.1), .62, .02), 'WoodMid')
    for x in POSTS: box(m, (x, 1.6, WALL_Z+.06), (.16, 3.1, .12), 'Wood')
    for y, h in ((.92, .05), (2.62, .04), (2.96, .07)): box(m, (0, y, WALL_Z+.13), (6.8, h, .03), 'Brass')
    for x0, x1 in zip(POSTS, POSTS[1:]):
        box(m, ((x0+x1)/2, 1.76, WALL_Z+.015), (x1-x0-.3, 1.48, .03), 'LacquerLight')   # raised lacquer panel per bay
        box(m, ((x0+x1)/2, 2.79, WALL_Z+.015), (x1-x0-.3, .24, .03), 'LacquerDark')     # frieze
    for x in POSTS:
        for y in (.95, 2.6): box(m, (x, y, WALL_Z+.13), (.22, .07, .05), 'BrassDark')     # post collars
    # Side walls: posts, rails and lacquer panels, with rigging rope running up the corners.
    for s in (-1, 1):
        for z in (-1.6, -.4, .8): box(m, (s*3.33, 1.6, z), (.08, 3.1, .14), 'Wood')
        for y, h in ((.92, .05), (2.96, .07)): box(m, (s*3.33, y, -.4), (.05, h, 3.2), 'Brass')
        box(m, (s*3.34, .45, -.4), (.04, .9, 3.2), 'Wood')
        for z0, z1 in ((-1.6, -.4), (-.4, .8)): box(m, (s*3.34, 1.76, (z0+z1)/2), (.03, 1.48, z1-z0-.3), 'LacquerLight')
        rope(m, (s*3.25, 3.05, -1.75), (s*2.95, .95, -1.8)); rope(m, (s*3.25, 3.05, .9), (s*3.27, .95, .3))

def back_wall_dressing(m):
    medallion(m, (0, 2.3, 0), .3, WALL_Z+.05)                    # over the merchant's head (head top ~1.73 m)
    for s in (-1, 1):
        # A phone shows the back wall from about x -1.9 to 1.9: a sky porthole above a curio shelf in each side bay,
        # and a second porthole in the outer bays for wider screens.
        porthole(m, (s*1.6, 2.06, 0), .27, WALL_Z+.04); porthole(m, (s*2.8, 1.85, 0), .3, WALL_Z+.04)
        box(m, (s*1.6, 1.3, WALL_Z+.2), (1.2, .04, .34), 'Wood'); box(m, (s*1.6, 1.25, WALL_Z+.36), (1.2, .05, .02), 'Brass')
        chest(m, (s*1.95, 1.32, WALL_Z+.2), (.3, .2, .22)); chest(m, (s*1.95, 1.52, WALL_Z+.2), (.22, .14, .16), 'LacquerDark')
        jar(m, (s*1.22, 1.32, WALL_Z+.2), .09, .24)
        bolt(m, (s*1.6, 1.39, WALL_Z+.2), .4, .06, 'Teal' if s < 0 else 'LacquerHi')

def floor_props(m):
    for s in (-1, 1):
        crate(m, (s*2.65, 0, -1.35), (.6, .45, .5), 'Plank', s*.1)
        chest(m, (s*2.62, .45, -1.35), (.42, .26, .3))
        for i, (dx, dz) in enumerate(((.0, .0), (.32, .12))):   # ballast sacks against the side wall
            sack(m, (s*(3.0-dx), 0, -.35+dz), w=.36, h=.46, color='Sack' if i else 'SackDark')
    k.vertical_loft(m, [(0, .26, .26, .26), (.12, .28, .28, .28)], 'Rope', a=.7, b=.7, x=-2.2, zc=-1.5)   # coiled rope
    ring_band(m, (-2.2, 0, -1.5), .14, .2, 'Rope', h=.04)

def lanterns(m):
    for s in (-1, 1):
        paper_lantern(m, (s*2.75, 2.25, -1.15), .2, .42, 3.08)
        for x, r in ((.5, .1), (1.6, .1), (2.6, .09)): paper_lantern(m, (s*x, 2.72, .4), r, r*2.2, 3.05)   # festoon under the front beam

# ------------------------------------------------------------------ counter

def counter(m):
    # Rosewood top with brass banding on the customer edge and ends.
    box(m, (0, T-.04, .45), (CW, .08, .62), 'Wood')
    box(m, (0, T-.035, .765), (CW+.02, .05, .03), 'Brass')
    for s in (-1, 1): box(m, (s*(CW/2+.015), T-.035, .45), (.03, .05, .64), 'Brass')
    # Lacquer front between brass pilasters, three raised panels, a brass rail under the top and a rosewood plinth.
    H = T-.08
    W = CW-.1; bay = (W-.08)/3
    box(m, (0, H/2, .72), (W, H, .06), 'Lacquer')
    box(m, (0, .05, .76), (W+.06, .1, .08), 'Wood')
    box(m, (0, H-.06, .757), (W, .035, .02), 'Brass')
    for x in (-1.5*bay, -.5*bay, .5*bay, 1.5*bay): box(m, (x, H/2+.05, .765), (.08, H-.1, .04), 'BrassDark')
    for x in (-bay, 0, bay):
        box(m, (x, .5, .757), (bay-.16, .52, .025), 'LacquerLight')
        box(m, (x, .5, .772), (bay-.28, .4, .01), 'LacquerHi')
    medallion(m, (0, .5, 0), .14, .77)
    box(m, (0, H/2, .17), (W, H, .06), 'LacquerDark')
    for s in (-1, 1): box(m, (s*(W/2+.02), H/2, .445), (.06, H, .55), 'Lacquer')
    # Gold-edged silk mats on the offer spots (drawn at -x: the import mirrors X).
    for x, y, z in OFFERS:
        box(m, (-x, T+.002, z), (.5, .004, .4), 'Brass')
        box(m, (-x, T+.006, z), (.46, .004, .36), 'Silk')

def balloon_interior():
    m = k.Mesh('BalloonInterior', pal)
    room(m); back_wall_dressing(m); floor_props(m); lanterns(m); counter(m)
    return m

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    pal.write_png(OUT/'BalloonInteriorPalette.png')
    m = balloon_interior()
    assert m.triangles <= MAX_TRIS, m.triangles
    stats = m.save(OUT, HEADER)
    manifest = dict(Revision=1, Style='faceted', CounterTop=T, Mesh='BalloonInterior', Keeper='ShopkeeperBalloonMerchant',
                    OfferAnchorsFlat=[c for o in OFFERS for c in o], OfferKinds=['Special garment', 'Mystery box', 'Special backpack'], Parts=[stats],
                    Scope='Art only: the balloon shop counter view. Offer anchors are in Unity space (the OBJ import mirrors X).')
    (OUT/'BalloonInteriorManifest.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
    print('BalloonInterior', stats['triangles'], 'triangles')

if __name__ == '__main__':
    main()
