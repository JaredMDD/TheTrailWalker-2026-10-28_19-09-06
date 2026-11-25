"""Store furniture production models (PROJECT_RECOMENDATIONS.md section 13.2). Standard library only.

Art batch A (November 7, 2026): the six house-level-1 types. Batch B (same day): levels 2 (pine) and 3 (birch).
Batch C (same day): levels 4 (birch, copper fittings) and 5 (mesquite, iron fittings).
Batch D (same day): levels 6 (mesquite, iron), 7 (oak, iron) and 8 (oak, steel). This completes the 48 types.
Chests are built with a separate lid part hinged at the back top edge, so the chest can open when touched.
Contract: metres, +Y up, +Z toward the camera/street. Each model fills its footprint (0.5 m per grid cell) with its
pivot at the FRONT-LEFT cell corner as seen from the camera, so the body spans x in [-width, 0] and z in [-depth, 0]
(grid columns run toward world -X, rows toward the back, matching the house interiors). Height at most 2.0 m.
Top surfaces carry each piece's identity, since the angled interior view and the bird's-eye placement view look down.
Colours use the house palette (HousePalette.png) so furniture shares the house material.
Run: python Tools/Art/generate_furniture.py, then The Trail Walker > Art > Build Furniture.
WARNING (Tier 3 integration slice C, November 24, 2026): three OBJs in Art/Settlement/Furniture were replaced by reviewed
repairs that this script does not reproduce: FurnitureTrophyCabinet (plaque seated on the crown), FurnitureClothPress (caps
sunk 5 mm into the crossbeam) and FurnitureMesquiteChest (hollow body). A full run rewrites them; restore those three from
git afterwards, or port the repairs into trophy_cabinet / cloth_press / the mesquite chest body first.
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k
import house_geometry as g
from house_specs import PALETTE, GRID_METRES

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Settlement/Furniture'
HEADER = 'The Trail Walker store furniture; metres; +Y up; +Z toward the camera; pivot at the front-left footprint corner.'
pal = k.Palette()
for spec in PALETTE: pal.add(*spec)

# (id, display name, house level, buff category, footprint width x depth in cells, cash price, chest)
CATALOG = [
    ('furniture.l1.worktable', 'Worktable', 1, 'CraftQuality', (2, 1), 3000, False),
    ('furniture.l1.bedroll', 'Bedroll', 1, 'FoodRecovery', (2, 1), 3000, False),
    ('furniture.l1.market_sign', 'Market sign', 1, 'SalePrice', (1, 1), 3000, False),
    ('furniture.l1.boot_bench', 'Boot bench', 1, 'BootsLife', (2, 1), 3000, False),
    ('furniture.l1.sewing_basket', 'Sewing basket', 1, 'ShirtLife', (1, 1), 3000, False),
    ('furniture.l1.hat_peg', 'Hat peg', 1, 'HatLife', (1, 1), 3000, False),
    ('furniture.l1.tool_rack', 'Tool rack', 1, 'Storage', (2, 1), 3000, False),   # extra: no buff, holds tools and spare clothing
    ('furniture.l2.trouser_rack', 'Trouser rack', 2, 'PantsLife', (2, 1), 6000, False),
    ('furniture.l2.chopping_block', 'Chopping block', 2, 'AxeLife', (1, 1), 6000, False),
    ('furniture.l2.sling_stand', 'Sling stand', 2, 'HuntingToolLife', (1, 1), 6000, False),
    ('furniture.l2.rod_rack', 'Rod rack', 2, 'RodLife', (2, 1), 6000, False),
    ('furniture.l2.bag_frame', 'Bag frame', 2, 'BackpackLife', (1, 1), 6000, False),
    ('furniture.l2.pine_chest', 'Pine chest', 2, 'Storage', (2, 1), 6000, True),
    ('furniture.l3.loom', 'Loom', 3, 'CraftQuality', (2, 2), 20000, False),
    ('furniture.l3.clay_cookstove', 'Clay cookstove', 3, 'FoodRecovery', (2, 1), 20000, False),
    ('furniture.l3.display_counter', 'Display counter', 3, 'SalePrice', (3, 1), 20000, False),
    ('furniture.l3.sofa', 'Sofa', 3, 'BootsLife', (3, 1), 20000, False),
    ('furniture.l3.sewing_desk', 'Sewing desk', 3, 'ShirtLife', (2, 1), 20000, False),
    ('furniture.l3.hat_cabinet', 'Hat cabinet', 3, 'HatLife', (2, 1), 20000, False),
    ('furniture.l4.tailors_stool', "Tailor's stool", 4, 'PantsLife', (1, 1), 50000, False),
    ('furniture.l4.whetstone_stand', 'Whetstone stand', 4, 'AxeLife', (1, 1), 50000, False),
    ('furniture.l4.bow_workbench', 'Bow workbench', 4, 'HuntingToolLife', (3, 1), 50000, False),
    ('furniture.l4.tackle_desk', 'Tackle desk', 4, 'RodLife', (2, 1), 50000, False),
    ('furniture.l4.leather_table', 'Leather table', 4, 'BackpackLife', (2, 2), 50000, False),
    ('furniture.l4.birch_chest', 'Birch chest', 4, 'Storage', (2, 1), 50000, True),
    ('furniture.l5.artisan_bench', 'Artisan bench', 5, 'CraftQuality', (3, 1), 150000, False),
    ('furniture.l5.pantry_dresser', 'Pantry dresser', 5, 'FoodRecovery', (2, 1), 150000, False),
    ('furniture.l5.trophy_cabinet', 'Trophy cabinet', 5, 'SalePrice', (2, 1), 150000, False),
    ('furniture.l5.boot_cabinet', 'Boot cabinet', 5, 'BootsLife', (2, 1), 150000, False),
    ('furniture.l5.cloth_press', 'Cloth press', 5, 'ShirtLife', (2, 2), 150000, False),
    ('furniture.l5.hat_display', 'Hat display', 5, 'HatLife', (1, 1), 150000, False),
    ('furniture.l6.clothing_armoire', 'Clothing armoire', 6, 'PantsLife', (2, 1), 400000, False),
    ('furniture.l6.tool_forge', 'Tool forge', 6, 'AxeLife', (2, 2), 400000, False),
    ('furniture.l6.bow_cabinet', 'Bow cabinet', 6, 'HuntingToolLife', (2, 1), 400000, False),
    ('furniture.l6.reel_workshop', 'Reel workshop', 6, 'RodLife', (2, 2), 400000, False),
    ('furniture.l6.pack_maker_bench', 'Pack-maker bench', 6, 'BackpackLife', (3, 1), 400000, False),
    ('furniture.l6.mesquite_chest', 'Mesquite chest', 6, 'Storage', (3, 1), 400000, True),
    ('furniture.l7.master_loom', 'Master loom', 7, 'CraftQuality', (3, 2), 1000000, False),
    ('furniture.l7.dining_table', 'Dining table', 7, 'FoodRecovery', (3, 2), 1000000, False),
    ('furniture.l7.gallery_pedestal', 'Gallery pedestal', 7, 'SalePrice', (1, 1), 1000000, False),
    ('furniture.l7.travel_sofa', 'Travel sofa', 7, 'BootsLife', (3, 1), 1000000, False),
    ('furniture.l7.fine_cloth_workstation', 'Fine-cloth workstation', 7, 'ShirtLife', (3, 1), 1000000, False),
    ('furniture.l7.milliner_bench', 'Milliner bench', 7, 'HatLife', (2, 1), 1000000, False),
    ('furniture.l8.frontier_wardrobe', 'Frontier wardrobe', 8, 'PantsLife', (2, 1), 2000000, False),
    ('furniture.l8.master_tool_station', 'Master tool station', 8, 'AxeLife', (3, 2), 2000000, False),
    ('furniture.l8.hunters_cabinet', "Hunter's cabinet", 8, 'HuntingToolLife', (2, 1), 2000000, False),
    ('furniture.l8.anglers_cabinet', "Angler's cabinet", 8, 'RodLife', (2, 1), 2000000, False),
    ('furniture.l8.expedition_workbench', 'Expedition workbench', 8, 'BackpackLife', (3, 2), 2000000, False),
    ('furniture.l8.reinforced_oak_chest', 'Reinforced oak chest', 8, 'Storage', (3, 1), 2000000, True),
]
# Extra rows are not among the 48 buff types: no buff, never furnished into neighbour homes (Milestone 11).
EXTRA = {'furniture.l1.tool_rack'}
RACKS = {'furniture.l1.tool_rack'}
WOOD = {1: 'Pine', 2: 'Pine', 3: 'Birch', 4: 'Birch', 5: 'Mesquite', 6: 'Mesquite', 7: 'Oak', 8: 'Oak'}
METAL = {1: 'Iron', 2: 'Iron', 3: 'Iron', 4: 'Copper', 5: 'Iron', 6: 'Iron', 7: 'Iron', 8: 'Steel'}   # as the house trim


def cyl(m, a, b, r, color, sides=8, r2=None):
    """Faceted cylinder (or cone frustum) from point a to point b."""
    axis = k.unit(k.sub(b, a))
    hint = (0, 0, 1) if abs(axis[2]) < .9 else (1, 0, 0)
    k.limb(m, [(a, r, r), (b, r2 if r2 is not None else r, r2 if r2 is not None else r)], color, sides=sides, forward_hint=hint)


def worktable(m, W, D):
    # Pine workbench: thick top, splayed legs, lower shelf with planks; tools laid out on top (craft quality).
    cx, cz = -W/2, -D/2; top = .78; tw, td = W-.06, D-.08
    g.box(m, (cx, top, cz), (tw, .07, td), 'Pine')
    g.box(m, (cx, top+.005, cz+td/2-.02), (tw, .06, .04), 'PineLight')            # front apron edge
    for sx in (-1, 1):
        for sz in (-1, 1):
            g.box(m, (cx+sx*(tw/2-.06), top/2, cz+sz*(td/2-.05)), (.07, top, .07), 'WoodDark')
    g.box(m, (cx, .16, cz), (tw-.12, .04, td-.10), 'Pine')                         # lower shelf
    for i in range(3): g.box(m, (cx-.05+i*.02, .21+i*.045, cz), (.62, .04, .14), 'PineLight' if i % 2 else 'Pine')
    # Bench vice at the right end, hand plane, mallet, chisel and a cut board on top.
    g.box(m, (-W+.10, top+.07, cz+.10), (.12, .08, .16), 'WoodDark')
    cyl(m, (-W+.10, top+.07, cz+.17), (-W+.10, top+.07, cz+.235), .015, 'Iron', 6)
    g.box(m, (cx+.22, top+.07, cz-.02), (.24, .06, .08), 'Pine'); g.box(m, (cx+.22, top+.12, cz-.02), (.05, .05, .05), 'WoodDark')
    g.box(m, (cx+.26, top+.105, cz-.02), (.03, .015, .05), 'Iron')
    cyl(m, (cx-.10, top+.06, cz+.08), (cx-.10, top+.06, cz-.12), .018, 'PineLight', 6)
    g.box(m, (cx-.10, top+.08, cz-.15), (.12, .08, .09), 'WoodDark')               # mallet head
    g.box(m, (cx-.32, top+.045, cz+.04), (.22, .02, .03), 'Iron'); g.box(m, (cx-.24, top+.05, cz+.04), (.08, .03, .035), 'WoodDark')
    g.box(m, (cx+.02, top+.05, cz-.08), (.30, .03, .12), 'PineLight')


def bedroll(m, W, D):
    # Low pine pallet with a fur bedroll, rolled pillow and folded blanket (rest and meals: food recovery).
    cx, cz = -W/2, -D/2
    for i in range(5): g.box(m, (cx, .05, cz-D/2+.06+i*(D-.12)/4), (W-.04, .05, .07), 'Pine')
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.08), .025, cz), (.08, .05, D-.06), 'WoodDark')
    g.box(m, (cx-.04, .115, cz), (W-.16, .08, D-.12), 'Fur')
    g.box(m, (cx-.20, .165, cz), (.46, .03, D-.14), 'Cloth')                       # folded blanket
    g.box(m, (cx-.20, .185, cz-.02), (.46, .02, D-.20), 'CreamCloth')
    cyl(m, (-.10, .20, cz-D/2+.08), (-.10, .20, cz+D/2-.08), .065, 'CreamCloth', 6)  # rolled pillow at the left
    for zz in (cz-.10, cz+.10): g.box(m, (-.10, .20, zz), (.15, .14, .025), 'Leather')
    # A small cup and bowl on the pallet edge hint at the meal the rest goes with.
    cyl(m, (-W+.10, .08, cz+.13), (-W+.10, .16, cz+.13), .04, 'Ceramic', 8)
    cyl(m, (-W+.11, .08, cz-.08), (-W+.11, .12, cz-.08), .07, 'Ceramic', 8, .055)


def market_sign(m, W, D):
    # Standing A-frame sign with a green painted board, a price notice and a copper coin (sale price).
    cx, cz = -W/2, -D/2; h = 1.02; lean = 9
    for sz, face in ((1, True), (-1, False)):
        a = math.radians(lean*sz)
        up = (0, math.cos(a), -math.sin(a)); fwd = (0, math.sin(a), math.cos(a))
        base = (cx, .012, cz+sz*.13)
        centre = k.add(base, k.mul(up, h*.55))
        g.box(m, centre, (.40, .52, .035), 'Green' if face else 'GreenLight', axes=((1, 0, 0), up, fwd))
        for sx in (-1, 1): g.box(m, k.add(base, k.add(k.mul(up, h/2), (sx*.21, 0, 0))), (.04, h, .045), 'Pine', axes=((1, 0, 0), up, fwd))
        g.box(m, k.add(base, k.mul(up, h-.02)), (.46, .05, .05), 'Pine', axes=((1, 0, 0), up, fwd))
        if face:
            n = k.mul(fwd, .025)
            g.box(m, k.add(k.add(centre, n), k.mul(up, .08)), (.26, .16, .01), 'Paper', axes=((1, 0, 0), up, fwd))
            for row in range(3): g.box(m, k.add(k.add(centre, k.mul(fwd, .032)), k.mul(up, .13-row*.045)), (.18-row*.04, .012, .006), 'WoodDark', axes=((1, 0, 0), up, fwd))
            coin = k.add(k.add(centre, k.mul(fwd, .03)), k.mul(up, -.14))
            k.limb(m, [(coin, .055, .055), (k.add(coin, k.mul(fwd, .015)), .055, .055)], 'Copper', sides=8, forward_hint=(0, 1, 0))
    g.box(m, (cx, .30, cz), (.36, .025, .25), 'WoodDark')                          # spreader bar seen from above


def boot_bench(m, W, D):
    # Cobbler's boot bench: seat with a boot on a last and a brush on top, a pair of boots on the shelf below.
    cx, cz = -W/2, -D/2; seat = .44
    g.box(m, (cx, seat, cz), (W-.06, .06, D-.10), 'Pine')
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.08), seat/2, cz), (.06, seat, D-.14), 'WoodDark')
    g.box(m, (cx, .12, cz), (W-.20, .035, D-.16), 'Pine')
    def boot(x, z, y, scale=1.0, color='Leather'):
        g.box(m, (x, y+.11*scale, z-.02), (.09*scale, .22*scale, .10*scale), color)
        g.box(m, (x, y+.04*scale, z+.04*scale), (.09*scale, .08*scale, .20*scale), color, taper=.9)
        g.box(m, (x, y+.005, z+.03*scale), (.10*scale, .015, .24*scale), 'WoodDark')
    for x in (cx-.12, cx+.02): boot(x, cz-.02, .14, .95)
    # The last (wooden foot form) on a stand with a boot being worked, plus a brush and a tin of polish.
    cyl(m, (cx+.30, seat+.03, cz), (cx+.30, seat+.16, cz), .02, 'WoodDark', 6)
    boot(cx+.30, cz, seat+.16, .85)
    g.box(m, (cx-.15, seat+.05, cz+.05), (.16, .04, .06), 'Pine'); g.box(m, (cx-.15, seat+.035, cz+.05), (.14, .02, .05), 'CreamCloth')
    cyl(m, (cx-.32, seat+.03, cz-.06), (cx-.32, seat+.07, cz-.06), .045, 'Iron', 8)


def sewing_basket(m, W, D):
    # Woven pine-banded basket with an arched handle, fabric rolls, thread spools and a pincushion (shirt life).
    cx, cz = -W/2, -D/2
    k.limb(m, [((cx, .0, cz), .17, .17), ((cx, .22, cz), .20, .20)], 'CreamCloth', sides=8, turn=math.pi/8, cap_end=False)
    k.limb(m, [((cx, .03, cz), .175, .175), ((cx, .23, cz), .195, .195)], 'Cloth', sides=8, turn=math.pi/8, cap_start=False, cap_end=False)
    g.box(m, (cx, .02, cz), (.32, .03, .32), 'Pine')
    for y in (.06, .15, .225): k.limb(m, [((cx, y-.012, cz), .185+y*.14, .185+y*.14), ((cx, y+.012, cz), .186+y*.14, .186+y*.14)], 'Pine', sides=8, turn=math.pi/8)
    g.box(m, (cx, .19, cz), (.34, .02, .34), 'CreamCloth')                         # contents surface
    for i, (dx, dz, col) in enumerate(((-.06, .05, 'Cloth'), (.07, .03, 'Green'), (0, -.08, 'Paper'))):
        cyl(m, (cx+dx-.08, .24, cz+dz), (cx+dx+.08, .24, cz+dz), .04, col, 6)
    for dx, dz, col in ((.10, -.07, 'Copper'), (-.10, -.06, 'Green')):
        cyl(m, (cx+dx, .20, cz+dz), (cx+dx, .27, cz+dz), .022, col, 6)
        cyl(m, (cx+dx, .27, cz+dz), (cx+dx, .28, cz+dz), .03, 'Pine', 6)
    cyl(m, (cx-.11, .21, cz-.02), (cx-.11, .26, cz-.02), .045, 'Cloth', 8)        # pincushion
    g.box(m, (cx-.11, .285, cz-.02), (.005, .04, .005), 'Steel')
    for i in range(7):                                                              # arched handle
        a0, a1 = math.pi*i/7, math.pi*(i+1)/7
        p0 = (cx+.17*math.cos(a0), .23+.20*math.sin(a0), cz); p1 = (cx+.17*math.cos(a1), .23+.20*math.sin(a1), cz)
        g.beam(m, p0, p1, .03, 'Pine', .03)


def hat_peg(m, W, D):
    # Free-standing hat peg stand (the wall peg made free-standing): cross feet, post, four pegs, two hats (hat life).
    cx, cz = -W/2, -D/2; h = 1.62
    for ax in ((1, 0, 0), (0, 0, 1)):
        g.box(m, (cx, .04, cz), (.42 if ax[0] else .06, .06, .06 if ax[0] else .42), 'WoodDark')
    cyl(m, (cx, .07, cz), (cx, h, cz), .03, 'Pine', 6)
    cyl(m, (cx, h, cz), (cx, h+.05, cz), .045, 'WoodDark', 6)
    pegs = []
    for i, ang in enumerate((45, 135, 225, 315)):
        a = math.radians(ang); d = (math.cos(a), 0, math.sin(a)); y = h-.12-(i % 2)*.05
        tip = (cx+d[0]*.14, y+.06, cz+d[2]*.14)
        cyl(m, (cx+d[0]*.02, y, cz+d[2]*.02), tip, .013, 'WoodDark', 6); pegs.append(tip)
    def hat(at, brim_r, crown_r, crown_h, color, band):
        x, y, z = at
        k.limb(m, [((x, y-.005, z), brim_r, brim_r), ((x, y+.012, z), brim_r, brim_r)], color, sides=10)
        k.limb(m, [((x, y+.01, z), crown_r, crown_r*.9), ((x, y+crown_h, z), crown_r*.82, crown_r*.72)], color, sides=8)
        k.limb(m, [((x, y+.012, z), crown_r+.003, crown_r*.9+.003), ((x, y+.04, z), crown_r+.002, crown_r*.9+.002)], band, sides=8, cap_start=False, cap_end=False)
    hat((pegs[0][0], pegs[0][1]+.01, pegs[0][2]), .13, .08, .10, 'Fur', 'Leather')        # rabbit-fur hat (front-left)
    hat((pegs[2][0], pegs[2][1]+.01, pegs[2][2]), .125, .072, .09, 'Leather', 'CreamCloth')  # leather hat (back-right)
    g.box(m, (pegs[1][0], pegs[1][1]-.12, pegs[1][2]), (.05, .24, .03), 'Cloth')            # scarf hanging from a peg


RACK_PEG_BOX = (.22, .40, .12)   # metres: the volume one hung tool or garment occupies below its peg


def rack_layout(W, D):
    """Peg tips and hang volumes for the tool rack: 4 columns x 3 rows, row-major from the top-left (Spot00-Spot03 the top row, left to right)."""
    cx, cz = -W/2, -D/2; front = cz-.08+.025
    spots = []
    for row in range(3):
        for col in range(4):
            peg_y = 1.35-row*.40
            spots.append((cx-(col-1.5)*.20, peg_y-.15, front+.06))   # +X is screen-left, so column 0 sits at +X
    return spots


def tool_rack(m, W, D):
    # Free-standing pine pegboard (extra piece, no buff): two posts on foot boards, a pegged board, a cap rail, 4 x 3 pegs (4 across, 3 rows).
    cx, cz = -W/2, -D/2; bz = cz-.08; front = bz+.025
    for sx in (-1, 1):
        x = cx+sx*(W/2-.05)
        g.box(m, (x, .03, cz), (.09, .06, D-.08), 'WoodDark')                      # foot board
        g.box(m, (x, .90, bz), (.07, 1.76, .07), 'Pine')                            # post
    g.box(m, (cx, .90, bz), (W-.16, 1.25, .05), 'PineLight')                         # the pegboard
    for i in range(3):                                                                  # plank seams and grain on the front face
        g.box(m, (cx-.20+i*.20, .90, front+.002), (.010, 1.23, .006), 'WoodDark')
    for y in (1.52, .28):
        g.box(m, (cx, y, bz), (W-.16, .06, .075), 'Pine')                           # top and bottom rails
    g.box(m, (cx, 1.80, bz), (W-.10, .05, .09), 'WoodDark')                          # cap rail across the posts
    g.box(m, (cx, .12, cz+.02), (W-.20, .035, .05), 'Pine')                          # low stretcher between the feet
    for (x, y, z) in rack_layout(W, D):
        py = y+.15
        cyl(m, (x, py, front), (x, py+.025, front+.13), .020, 'WoodDark', 6)
        g.box(m, (x, py+.027, front+.135), (.045, .045, .02), 'Pine')               # peg head


# ------------------------------------------------------------------ batch B: level 2 (pine)

def trouser_rack(m, W, D, wood='Pine'):
    # Two-rail drying/hanging rack with folded trousers over the rails (pants life).
    cx, cz = -W/2, -D/2; h = 1.15
    for sx in (-1, 1):
        x = cx+sx*(W/2-.06)
        g.box(m, (x, .03, cz), (.08, .06, D-.06), 'WoodDark')
        g.box(m, (x, h/2, cz), (.06, h, .06), wood)
    for y, dz in ((h-.04, .08), (h-.30, -.08)):
        cyl(m, (-.06, y, cz+dz), (-W+.06, y, cz+dz), .02, 'PineLight', 6)
    def trousers(x0, x1, y, z, color):
        # draped over a rail: a flat panel hanging down on each side of the rail
        for side in (-1, 1): g.box(m, ((x0+x1)/2, y-.24, z+side*.035), (x1-x0, .46, .025), color, shade=2+side)
        g.box(m, ((x0+x1)/2, y+.02, z), (x1-x0, .03, .09), color)
        g.box(m, ((x0+x1)/2, y-.465, z+.035), (x1-x0, .03, .03), 'Leather')     # hem
    trousers(cx-.38, cx-.04, h-.04, cz+.08, 'CreamCloth')
    trousers(cx+.02, cx+.34, h-.04, cz+.08, 'Cloth')
    trousers(cx-.20, cx+.14, h-.30, cz-.08, 'Leather')


def chopping_block(m, W, D, wood='Pine'):
    # Stump block with growth rings on top, an axe sunk in it and split logs beside (axe life).
    cx, cz = -W/2, -D/2
    k.limb(m, [((cx, 0, cz), .19, .19), ((cx, .40, cz), .175, .175)], 'WoodDark', sides=9)
    for r, col in ((.165, 'PineLight'), (.11, 'Pine'), (.05, 'PineLight')):
        k.limb(m, [((cx, .40, cz), r, r), ((cx, .405+(.17-r)*.02, cz), r, r)], col, sides=9)
    # Axe: head buried at an angle near the centre, handle leaning toward the front-left.
    head = (cx-.02, .44, cz+.01)
    g.box(m, head, (.03, .10, .12), 'Iron', axes=((1, 0, 0), (0, 1, 0), (0, 0, 1)))
    cyl(m, (head[0], head[1]+.03, head[2]), (cx+.17, .78, cz+.10), .017, 'PineLight', 6)
    for dz, rot in ((-.17, 0), (-.13, 1)):
        cyl(m, (-.04-rot*.03, .045, cz+dz+.24), (-.04-rot*.03, .045+rot*.0, cz+dz+.06), .04, 'Pine', 5)


def sling_stand(m, W, D, wood='Pine'):
    # Small post stand with a hanging slingshot, a pouch of stones and a spare band (hunting-tool life).
    cx, cz = -W/2, -D/2
    g.box(m, (cx, .03, cz), (.34, .06, .34), 'WoodDark')
    cyl(m, (cx, .06, cz), (cx, 1.05, cz), .028, wood, 6)
    g.box(m, (cx, 1.0, cz), (.30, .04, .05), 'WoodDark')                           # cross arm
    # Slingshot hanging from the arm: handle, fork and the leather band with its pouch.
    sx = cx+.09
    cyl(m, (sx, .62, cz+.04), (sx, .80, cz+.04), .016, 'PineLight', 6)
    for side in (-1, 1): cyl(m, (sx, .80, cz+.04), (sx+side*.05, .90, cz+.04), .013, 'PineLight', 6)
    for side in (-1, 1): g.beam(m, (sx+side*.05, .90, cz+.045), (sx, .78, cz+.07), .01, 'Leather', .006)
    g.box(m, (sx, .775, cz+.07), (.035, .025, .012), 'Leather')
    g.box(m, (sx, .95, cz+.04), (.012, .05, .012), 'Leather')                      # hanging loop
    # Stone pouch on the base and a coiled spare band.
    cyl(m, (cx-.08, .06, cz-.06), (cx-.08, .17, cz-.06), .06, 'Leather', 7, .045)
    for dx, dz in ((-.08, -.06), (-.06, -.04), (-.10, -.05)): g.box(m, (cx+dx, .175, cz+dz), (.025, .022, .025), 'Stone')
    k.limb(m, [((cx+.07, .06, cz-.08), .05, .05), ((cx+.07, .075, cz-.08), .05, .05)], 'Leather', sides=8)


def rod_rack(m, W, D, wood='Pine'):
    # Rack with three fishing rods standing upright, reels, line and a wicker creel at the foot (rod life).
    cx, cz = -W/2, -D/2
    g.box(m, (cx, .04, cz-.06), (W-.08, .08, .26), 'WoodDark')                     # base trough
    g.box(m, (cx, 1.10, cz-.16), (W-.10, .06, .07), wood)                          # top rail with notches
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.07), .58, cz-.16), (.06, 1.10, .06), wood)
    for i, (dx, col) in enumerate(((-.30, 'PineLight'), (-.10, 'Pine'), (.10, 'PineLight'))):
        x = cx+dx
        cyl(m, (x, .08, cz-.06), (x, 1.75, cz-.16), .012, col, 6, .006)
        reel = (x, .40, cz-.035)
        k.limb(m, [(reel, .045, .045), ((reel[0]+.03, reel[1], reel[2]), .045, .045)], 'Iron', sides=8, forward_hint=(0, 1, 0))
        g.box(m, (x+.015, .40, cz-.035), (.035, .02, .02), 'CreamCloth')
    # Creel (fish basket) on the floor at the right end, inside the front half.
    g.box(m, (cx+.33, .12, cz+.12), (.24, .24, .16), 'CreamCloth')
    g.box(m, (cx+.33, .25, cz+.12), (.25, .03, .17), 'Pine')
    for y in (.06, .15): g.box(m, (cx+.33, y, cz+.12), (.25, .02, .17), 'Pine')
    g.box(m, (cx+.33, .262, cz+.205), (.06, .025, .01), 'Leather')


def bag_frame(m, W, D, wood='Pine'):
    # Pack-display frame (a backpack form on a post) with a strapped pack on it (backpack life).
    cx, cz = -W/2, -D/2
    g.box(m, (cx, .03, cz), (.38, .06, .30), 'WoodDark')
    cyl(m, (cx, .06, cz-.08), (cx, 1.20, cz-.08), .025, wood, 6)
    g.box(m, (cx, 1.05, cz-.08), (.32, .05, .04), wood)                            # shoulder bar
    # Backpack: body, flap, front pocket, side roll, straps over the bar.
    g.box(m, (cx, .78, cz+.0), (.32, .44, .17), 'Leather')
    g.box(m, (cx, .96, cz+.02), (.33, .10, .19), 'Cloth', taper=.95)
    g.box(m, (cx, .70, cz+.10), (.20, .18, .05), 'Leather', shade=1)
    for sx in (-1, 1):
        g.box(m, (cx+sx*.10, .84, cz+.092), (.03, .30, .012), 'WoodDark')
        g.box(m, (cx+sx*.10, .96, cz+.113), (.035, .03, .01), 'Iron')
    cyl(m, (cx-.15, .60, cz+.0), (cx+.15, .60, cz+.0), .055, 'CreamCloth', 6)      # bedroll strapped at the bottom


def chest_body(m, W, D, wood='Pine', metal='Iron', hollow=False):
    # Chest body only (the lid is a separate part, see chest_lid). Metal corners and bands; front lock plate.
    cx, cz = -W/2, -D/2; h = .40
    g.box(m, (cx, .03, cz), (W-.04, .06, D-.04), 'WoodDark')                       # plinth
    if hollow:
        # Tier-two birch chest: visible cavity beneath its existing hinged lid.
        ow,od,t=W-.08,D-.08,.025
        for sx in (-1,1):g.box(m,(cx+sx*(ow/2-t/2),.06+h/2,cz),(t,h,od),wood)
        for sz in (-1,1):g.box(m,(cx,.06+h/2,cz+sz*(od/2-t/2)),(ow-2*t,h,t),wood)
        g.box(m,(cx,.08,cz),(ow-2*t,.04,od-2*t),'WoodDark')
    else:
        g.box(m, (cx, .06+h/2, cz), (W-.08, h, D-.08), wood)
    for i in range(3): g.box(m, (cx, .10+i*.13, cz+(D-.08)/2+.004), (W-.10, .012, .01), 'WoodDark')   # plank seams
    for sx in (-1, 1):
        g.box(m, (cx+sx*(W/2-.08), .06+h/2, cz+(D-.08)/2+.008), (.05, h, .012), metal)
        g.box(m, (cx+sx*(W/2-.04), .06+h/2, cz), (.012, h, D-.06), metal)
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.05), .06+h/2, cz+sz*(D/2-.05)), (.035, h+.01, .035), metal)
    g.box(m, (cx, .06+h-.06, cz+(D-.08)/2+.012), (.09, .10, .015), 'Copper')       # lock plate
    for sx in (-1, 1): k.limb(m, [((cx+sx*(W/2-.04)+sx*.006, .30, cz-.06), .025, .025), ((cx+sx*(W/2-.04)+sx*.02, .30, cz-.06), .025, .025)], metal, sides=6, forward_hint=(0, 1, 0))


def pine_chest(m, W, D, wood='Pine'): chest_body(m, W, D, wood, 'Iron')


def chest_lid(display, W, D, wood='Pine', metal='Iron'):
    """Lid in hinge-local coordinates: hinge axis along X at the back top edge (pivot at x=-W/2, y=0, z=0),
    lid extends toward +Z (front) when closed. Returns (mesh, hinge point in footprint coordinates)."""
    m = k.Mesh(asset_name(display) + 'Lid', pal); w = W-.06; d = D-.06
    g.box(m, (0, .035, d/2), (w, .07, d), wood)
    g.box(m, (0, .085, d/2), (w-.06, .03, d-.06), wood, shade=1)                     # raised panel
    for sx in (-1, 1):
        g.box(m, (sx*(w/2-.06), .06, d/2), (.05, .10, d+.01), metal)
    g.box(m, (0, .03, d+.006), (.08, .07, .012), 'Copper')                          # hasp
    hinge = (-W/2, .46, -D+.03)
    return m, hinge


# ------------------------------------------------------------------ batch B: level 3 (birch)

def loom(m, W, D, wood='Birch'):
    # Floor loom: four posts, breast and back beams, warp threads, heddle frame and cloth on the front roll (craft quality).
    cx, cz = -W/2, -D/2; h = 1.45; fw, fd = W-.08, D-.10
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(fw/2-.04), h/2 if sz < 0 else .45, cz+sz*(fd/2-.04)), (.07, h if sz < 0 else .90, .07), wood)
        g.box(m, (cx+sx*(fw/2-.04), .12, cz), (.06, .06, fd), 'WoodDark')             # floor runners
        g.box(m, (cx+sx*(fw/2-.04), h-.05, cz-.10), (.06, .06, fd*.6), wood)
    front, back = cz+fd/2-.06, cz-fd/2+.06
    cyl(m, (cx-fw/2+.04, .86, front), (cx+fw/2-.04, .86, front), .045, wood, 8)      # breast beam with cloth
    cyl(m, (cx-fw/2+.06, .80, front-.06), (cx+fw/2-.06, .80, front-.06), .07, 'Cloth', 8)
    cyl(m, (cx-fw/2+.04, 1.05, back), (cx+fw/2-.04, 1.05, back), .05, wood, 8)        # warp beam
    for i in range(16):                                                                # warp threads to the heddles
        x = cx-fw/2+.14+i*(fw-.28)/15
        g.box(m, (x, .97, (back+front)/2), (.006, .006, front-back-.12), 'CreamCloth', axes=((1, 0, 0), k.unit((0, 1, (front-back)*1.4)), k.unit((0, -(front-back)*1.4, 1))))
    g.box(m, (cx, 1.22, cz+.02), (fw-.12, .05, .05), wood)                            # heddle frame
    for sx in (-1, 1): g.box(m, (cx+sx*(fw/2-.12), 1.05, cz+.02), (.035, .35, .035), wood)
    g.box(m, (cx, .58, front-.10), (fw-.20, .04, .14), 'WoodDark')                    # treadles and bench edge
    g.box(m, (cx+.20, .98, front-.02), (.30, .03, .05), 'PineLight')                  # shuttle on the beam


def clay_cookstove(m, W, D, wood='Birch'):
    # Clay cookstove on a stone base: firebox door, iron hob with a pot and kettle, flue pipe (food recovery).
    cx, cz = -W/2, -D/2
    g.box(m, (cx, .06, cz), (W-.04, .12, D-.04), 'Stone')
    g.box(m, (cx, .12+.30, cz), (W-.10, .60, D-.10), 'Ceramic')
    g.box(m, (cx-.15, .36, cz+(D-.10)/2+.01), (.24, .20, .02), 'Iron')                # firebox door
    g.box(m, (cx-.15, .36, cz+(D-.10)/2+.022), (.18, .02, .01), 'Amber')              # ember glow slit
    g.box(m, (cx+.22, .30, cz+(D-.10)/2+.01), (.16, .10, .02), 'Iron')                # ash drawer
    g.box(m, (cx, .735, cz), (W-.08, .03, D-.08), 'Iron')                             # hob plate
    cyl(m, (cx-.20, .75, cz+.02), (cx-.20, .88, cz+.02), .09, 'Iron', 8, .085)        # pot
    cyl(m, (cx-.20, .88, cz+.02), (cx-.20, .90, cz+.02), .092, 'WoodDark', 8)
    cyl(m, (cx+.05, .75, cz+.05), (cx+.05, .85, cz+.05), .06, 'Copper', 8, .045)      # kettle
    cyl(m, (cx+.09, .80, cz+.05), (cx+.16, .86, cz+.05), .012, 'Copper', 5)
    cyl(m, (cx+.33, .75, cz-.10), (cx+.33, 1.85, cz-.10), .055, 'Iron', 8)            # flue pipe
    cyl(m, (cx+.33, 1.85, cz-.10), (cx+.33, 1.90, cz-.10), .075, 'Iron', 8)


def display_counter(m, W, D, wood='Birch'):
    # Shop counter with a panelled front, a glazed display case and goods on top: scale, jug, cloth bolt (sale price).
    cx, cz = -W/2, -D/2; h = .90
    g.box(m, (cx, .04, cz), (W-.04, .08, D-.04), 'WoodDark')
    g.box(m, (cx, h/2, cz), (W-.08, h-.08, D-.10), wood)
    for i in range(3): g.box(m, (cx-W/3+i*W/3, h/2-.02, cz+(D-.10)/2+.01), (W/3-.10, h-.30, .015), wood, shade=1)
    g.box(m, (cx, h, cz), (W-.04, .05, D-.06), 'WoodDark')                            # top
    # Glazed case on the left half of the top.
    case = (cx+W/4, h+.13, cz-.02)
    g.box(m, (case[0], h+.03, case[2]), (W/2-.10, .02, D-.16), wood)
    g.box(m, case, (W/2-.12, .20, D-.18), 'GlassLight', shade=2)
    for sx in (-1, 1): g.box(m, (case[0]+sx*(W/4-.06), case[1], case[2]), (.03, .22, D-.16), wood)
    g.box(m, (case[0], case[1]+.11, case[2]), (W/2-.10, .02, D-.16), wood)
    for i, col in enumerate(('Copper', 'Ceramic', 'Green')): g.box(m, (case[0]-.18+i*.18, h+.07, case[2]), (.08, .06, .08), col)
    # Balance scale, jug and a cloth bolt on the right.
    g.box(m, (cx-.15, h+.03, cz), (.10, .02, .08), 'Copper')
    cyl(m, (cx-.15, h+.04, cz), (cx-.15, h+.24, cz), .008, 'Copper', 5)
    g.box(m, (cx-.15, h+.24, cz), (.26, .012, .012), 'Copper')
    for sx in (-1, 1): cyl(m, (cx-.15+sx*.12, h+.12, cz), (cx-.15+sx*.12, h+.13, cz), .05, 'Copper', 8)
    cyl(m, (cx-.40, h+.03, cz+.04), (cx-.40, h+.18, cz+.04), .05, 'Ceramic', 8, .035)
    cyl(m, (cx-.62, h+.07, cz-.10), (cx-.62, h+.07, cz+.10), .045, 'Cloth', 6)


def sofa(m, W, D, wood='Birch', cushion='Cloth'):
    # Birch-framed sofa with cushions and a boot pair resting at its foot end (boots life: rest for tired feet).
    cx, cz = -W/2, -D/2
    seat_w = W-.50; base_w = seat_w+.20; bx = cx+.10                                     # sofa sits toward +X
    g.box(m, (bx, .10, cz), (base_w, .12, D-.06), wood)                                 # frame base
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (bx+sx*(base_w/2-.05), .03, cz+sz*(D/2-.07)), (.06, .06, .06), 'WoodDark')
    g.box(m, (cx+.10, .26, cz+.03), (seat_w, .16, D-.14), cushion)                   # seat cushions
    for i in range(2): g.box(m, (cx+.10-seat_w/4+i*seat_w/2, .345, cz+.03), (seat_w/2-.02, .02, D-.16), cushion, shade=1+i)
    g.box(m, (cx+.10, .55, cz-D/2+.08), (seat_w, .44, .10), cushion, taper=.95)        # back
    g.box(m, (cx+.10, .78, cz-D/2+.08), (seat_w+.04, .04, .12), wood)
    for x in (cx+.10-seat_w/2-.04, cx+.10+seat_w/2+.04):
        g.box(m, (x, .40, cz), (.08, .44, D-.08), wood)                               # arms
    g.box(m, (cx+.10+seat_w/2-.18, .36, cz+.02), (.24, .06, .20), 'CreamCloth')       # folded throw
    # Boot pair standing on the floor at the sofa's free (-X, screen-right) end, inside the footprint.
    for dx in (-.07, .07):
        x = -W+.16+dx
        g.box(m, (x, .11, cz-.05), (.09, .22, .10), 'Leather')
        g.box(m, (x, .04, cz+.0), (.09, .08, .18), 'Leather', taper=.9)
        g.box(m, (x, .006, cz-.01), (.10, .012, .21), 'WoodDark')


def sewing_desk(m, W, D, wood='Birch'):
    # Desk with drawers, a spool rack, cloth being cut, scissors and a folded shirt (shirt life).
    cx, cz = -W/2, -D/2; h = .76
    g.box(m, (cx, h, cz), (W-.06, .05, D-.08), wood)
    g.box(m, (cx+.28, h/2, cz), (.36, h-.04, D-.12), wood)                            # drawer pedestal (left)
    for i in range(3):
        g.box(m, (cx+.28, .14+i*.21, cz+(D-.12)/2+.01), (.32, .17, .015), wood, shade=1)
        g.box(m, (cx+.28, .14+i*.21, cz+(D-.12)/2+.025), (.06, .02, .015), 'Copper')
    for sz in (-1, 1): g.box(m, (cx-W/2+.07, h/2, cz+sz*(D/2-.08)), (.05, h, .05), wood)
    g.box(m, (cx-.15, h+.03, cz+.02), (.40, .012, .28), 'CreamCloth')                 # cloth being cut
    g.box(m, (cx-.15, h+.04, cz+.02), (.30, .004, .004), 'WoodDark')                  # chalk line
    for sx in (-1, 1): g.box(m, (cx-.02+sx*.015, h+.045, cz+.10), (.012, .006, .09), 'Steel', axes=((1, 0, 0), (0, 1, 0), k.unit((sx*.25, 0, 1))))
    g.box(m, (cx+.28, h+.06, cz+.04), (.24, .08, .20), 'Cloth')                        # folded shirt
    g.box(m, (cx+.28, h+.105, cz+.08), (.10, .012, .06), 'CreamCloth')
    g.box(m, (cx, h+.14, cz-D/2+.07), (W-.20, .02, .06), wood)                       # spool rack at the back
    for i in range(7):
        x = cx-W/2+.16+i*(W-.32)/6
        cyl(m, (x, h+.155, cz-D/2+.07), (x, h+.20, cz-D/2+.07), .018, ('Cloth', 'Green', 'CreamCloth', 'Copper')[i % 4], 6)
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.10), h+.07, cz-D/2+.07), (.03, .14, .05), wood)


def hat_cabinet(m, W, D, wood='Birch'):
    # Cabinet with glazed doors and two hats displayed on stands on top (hat life).
    cx, cz = -W/2, -D/2; h = 1.10
    g.box(m, (cx, .04, cz), (W-.04, .08, D-.06), 'WoodDark')
    g.box(m, (cx, h/2+.04, cz), (W-.08, h, D-.10), wood)
    for sx in (-1, 1):
        door = (cx+sx*(W/4-.02), .62, cz+(D-.10)/2+.008)
        g.box(m, door, (W/2-.10, .80, .015), wood, shade=1)
        g.box(m, (door[0], door[1]+.06, door[2]+.006), (W/2-.22, .52, .01), 'GlassLight', shade=2)
        g.box(m, (cx+sx*.04, .58, door[2]+.02), (.02, .08, .02), 'Copper')
    g.box(m, (cx, .16, cz+(D-.10)/2+.008), (W-.14, .12, .015), wood, shade=3)          # bottom drawer
    g.box(m, (cx, h+.06, cz), (W-.04, .04, D-.06), 'WoodDark')                         # top
    def hat(x, z, brim, crown, ch, color, band):
        cyl(m, (x, h+.08, z), (x, h+.20, z), .012, wood, 5)                             # stand
        y = h+.20
        k.limb(m, [((x, y, z), brim, brim), ((x, y+.015, z), brim, brim)], color, sides=10)
        k.limb(m, [((x, y+.012, z), crown, crown*.9), ((x, y+ch, z), crown*.82, crown*.72)], color, sides=8)
        k.limb(m, [((x, y+.015, z), crown+.003, crown*.9+.003), ((x, y+.045, z), crown+.002, crown*.9+.002)], band, sides=8, cap_start=False, cap_end=False)
    hat(cx+.24, cz+.02, .15, .085, .11, 'Fur', 'Leather')
    hat(cx-.24, cz+.02, .14, .08, .12, 'Cloth', 'CreamCloth')


# ------------------------------------------------------------------ batch C: level 4 (birch, copper fittings)

def tailors_stool(m, W, D, wood='Birch'):
    # Round tailor's stool with a leather seat, folded trousers, a tape measure and a footrest ring (pants life).
    cx, cz = -W/2, -D/2; seat = .52
    for i in range(4):
        a = math.radians(45+90*i); top = (cx+.11*math.cos(a), seat-.03, cz+.11*math.sin(a)); foot = (cx+.17*math.cos(a), .012, cz+.17*math.sin(a))
        cyl(m, foot, top, .018, wood, 6)
    k.limb(m, [((cx, .20, cz), .155, .155), ((cx, .22, cz), .155, .155)], 'Copper', sides=10)    # footrest ring
    k.limb(m, [((cx, seat-.03, cz), .17, .17), ((cx, seat, cz), .17, .17)], wood, sides=10)
    k.limb(m, [((cx, seat, cz), .165, .165), ((cx, seat+.03, cz), .15, .15)], 'Leather', sides=10)
    g.box(m, (cx, seat+.06, cz-.01), (.22, .06, .16), 'Cloth')                               # folded trousers
    g.box(m, (cx, seat+.095, cz-.01), (.22, .012, .16), 'Cloth', shade=1)
    k.limb(m, [((cx+.06, seat+.10, cz+.06), .04, .04), ((cx+.06, seat+.12, cz+.06), .04, .04)], 'CreamCloth', sides=8)  # tape roll
    g.box(m, (cx-.03, seat+.103, cz+.08), (.14, .004, .02), 'CreamCloth')


def whetstone_stand(m, W, D, wood='Birch'):
    # Grinding wheel on a frame with a crank, a water trough under it and an axe resting against the leg (axe life).
    cx, cz = -W/2, -D/2; axle = .58
    for sx in (-1, 1):
        g.box(m, (cx+sx*.12, axle/2, cz-.05), (.05, axle, .05), wood)
        g.box(m, (cx+sx*.12, .03, cz), (.06, .06, .38), 'WoodDark')
    k.limb(m, [((cx-.05, axle, cz-.05), .19, .19), ((cx+.05, axle, cz-.05), .19, .19)], 'Stone', sides=12, forward_hint=(0, 1, 0))
    cyl(m, (cx-.16, axle, cz-.05), (cx+.17, axle, cz-.05), .015, 'Copper', 6)
    cyl(m, (cx+.17, axle, cz-.05), (cx+.17, axle+.10, cz+.02), .012, 'Copper', 5)              # crank arm
    cyl(m, (cx+.17, axle+.10, cz+.02), (cx+.22, axle+.10, cz+.02), .02, wood, 6)               # handle
    g.box(m, (cx, .22, cz-.05), (.20, .12, .30), wood)                                         # trough
    g.box(m, (cx, .275, cz-.05), (.16, .02, .26), 'Glass', shade=2)                            # water
    g.box(m, (cx+.03, .16, cz+.17), (.03, .10, .12), 'Steel')                                  # axe head on the floor
    cyl(m, (cx+.03, .18, cz+.15), (cx-.17, .03, cz+.04), .016, 'PineLight', 6)


def bow_workbench(m, W, D, wood='Birch'):
    # Long bench with a bow clamped in a vise, bow staves, a string coil and fletched arrows (hunting-tool life).
    cx, cz = -W/2, -D/2; top = .80
    g.box(m, (cx, top, cz), (W-.06, .06, D-.08), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.08), top/2, cz+sz*(D/2-.08)), (.06, top, .06), 'WoodDark')
        g.box(m, (cx+sx*(W/2-.08), .12, cz), (.05, .05, D-.16), 'WoodDark')
    g.box(m, (cx, .12, cz), (W-.16, .03, .05), 'WoodDark')
    # Vise on the right end holding a bow; the bow arcs along the bench as a chain of tapered segments.
    g.box(m, (-W+.14, top+.08, cz), (.14, .10, .18), 'WoodDark')
    pts = [(-W+.16+i*(W-.40)/8, top+.10+.10*math.sin(math.pi*i/8), cz-.02) for i in range(9)]
    for i in range(8): g.beam(m, pts[i], pts[i+1], .03-.012*abs(i-3.5)/3.5, 'Mesquite' if i in (3, 4) else wood, .03)
    g.beam(m, pts[0], pts[8], .006, 'CreamCloth', .006)                                       # string
    for i in range(3): g.box(m, (cx+.20+i*.07, top+.05, cz+.12), (.03, .03, .02), wood)         # stave offcuts
    for i in range(4):                                                                         # arrows
        z = cz+.08-i*.035
        g.box(m, (cx-.15, top+.04, z), (.62, .012, .012), 'PineLight')
        g.box(m, (cx-.47, top+.04, z), (.04, .02, .012), 'Iron')
        g.box(m, (cx+.13, top+.045, z), (.08, .025, .004), 'CreamCloth')
    k.limb(m, [((cx+.45, top+.03, cz-.10), .05, .05), ((cx+.45, top+.05, cz-.10), .05, .05)], 'CreamCloth', sides=8)  # string coil


def tackle_desk(m, W, D, wood='Birch'):
    # Desk of small tackle drawers with an open tackle tray of floats and lures, a reel and a line spool (rod life).
    cx, cz = -W/2, -D/2; top = .78
    g.box(m, (cx, top/2, cz), (W-.06, top, D-.10), wood)
    for r in range(3):
        for c in range(4):
            x = cx-W/2+.13+c*(W-.20)/4+.04; y = .14+r*.22
            g.box(m, (x, y, cz+(D-.10)/2+.008), ((W-.20)/4-.03, .17, .012), wood, shade=1+(r+c) % 3)
            g.box(m, (x, y, cz+(D-.10)/2+.02), (.04, .015, .015), 'Copper')
    g.box(m, (cx, top+.02, cz), (W-.04, .04, D-.06), 'WoodDark')
    g.box(m, (cx+.18, top+.07, cz), (.40, .06, .30), wood)                                     # open tackle tray
    for i in range(3):
        for j in range(2): g.box(m, (cx+.05+i*.13, top+.10, cz-.07+j*.14), (.11, .012, .12), 'WoodDark')
    for i, col in enumerate(('Green', 'Copper', 'Cloth', 'Amber', 'GlassLight', 'Green')):
        g.box(m, (cx+.05+(i % 3)*.13, top+.115, cz-.07+(i // 3)*.14), (.04, .025, .025), col)
    k.limb(m, [((cx-.22, top+.10, cz-.02), .06, .06), ((cx-.18, top+.10, cz-.02), .06, .06)], 'Iron', sides=10, forward_hint=(0, 1, 0))
    cyl(m, (cx-.38, top+.04, cz+.08), (cx-.38, top+.10, cz+.08), .04, 'CreamCloth', 8)


def leather_table(m, W, D, wood='Birch'):
    # Large cutting table with a hide spread out, pattern pieces, knife and awl, and a half-sewn backpack (backpack life).
    cx, cz = -W/2, -D/2; top = .80
    g.box(m, (cx, top, cz), (W-.06, .06, D-.06), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.08), top/2, cz+sz*(D/2-.08)), (.07, top, .07), 'WoodDark')
    g.box(m, (cx, .20, cz), (W-.20, .03, D-.20), wood)                                         # low shelf
    for i in range(3): g.box(m, (cx-.15+i*.12, .24, cz), (.10, .05, .50), 'Leather', shade=1+i)  # stacked hides
    # Hide on top: an irregular outline from overlapping slabs.
    for dx, dz, w, d in ((0, 0, .58, .50), (.10, .18, .30, .20), (-.16, -.16, .26, .24), (.22, -.10, .16, .30)):
        g.box(m, (cx+dx, top+.035, cz+dz), (w, .01, d), 'Leather', shade=2)
    for dx, dz in ((-.08, .05), (.12, -.06)): g.box(m, (cx+dx, top+.045, cz+dz), (.18, .006, .12), 'Paper')   # pattern pieces
    g.box(m, (cx+.30, top+.045, cz+.32), (.16, .008, .025), 'Steel'); g.box(m, (cx+.38, top+.05, cz+.32), (.07, .02, .03), 'WoodDark')
    cyl(m, (cx-.32, top+.04, cz+.30), (cx-.24, top+.04, cz+.30), .012, 'WoodDark', 6)
    g.box(m, (cx-.32, top+.15, cz-.30), (.26, .22, .14), 'Leather')                           # half-made pack
    g.box(m, (cx-.32, top+.27, cz-.28), (.27, .03, .16), 'CreamCloth')


def birch_chest(m, W, D, wood='Birch'): chest_body(m, W, D, wood, 'Copper', hollow=True)


# ------------------------------------------------------------------ batch C: level 5 (mesquite, iron fittings)

def artisan_bench(m, W, D, wood='Mesquite'):
    # Workbench with a tool board at the back (hanging saw, square, chisels), a vise and finished small work (craft quality).
    cx, cz = -W/2, -D/2; top = .82
    g.box(m, (cx, top, cz+.03), (W-.06, .07, D-.14), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.08), top/2, cz+.03+sz*(D/2-.12)), (.07, top, .07), 'WoodDark')
    g.box(m, (cx, .16, cz+.03), (W-.16, .04, D-.20), wood)
    g.box(m, (cx, 1.18, cz-D/2+.04), (W-.06, .70, .04), wood, shade=1)                          # tool board
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.05), 1.0, cz-D/2+.04), (.06, 1.10, .06), 'WoodDark')
    g.box(m, (cx+.40, 1.25, cz-D/2+.075), (.36, .12, .015), 'Steel')                           # saw blade
    g.box(m, (cx+.62, 1.25, cz-D/2+.08), (.08, .12, .03), wood)
    g.box(m, (cx-.05, 1.30, cz-D/2+.07), (.20, .025, .012), 'Iron'); g.box(m, (cx-.14, 1.22, cz-D/2+.07), (.025, .18, .012), 'Iron')  # square
    for i in range(5):
        x = cx-.30-i*.07
        g.box(m, (x, 1.12, cz-D/2+.07), (.02, .14, .015), 'Steel'); g.box(m, (x, 1.22, cz-D/2+.075), (.025, .07, .02), wood)
    g.box(m, (-W+.12, top+.08, cz+.10), (.14, .10, .16), 'WoodDark')                           # vise
    for i, (dx, col) in enumerate(((.10, 'Ceramic'), (.28, 'Copper'), (.45, 'PineLight'))):
        cyl(m, (cx+dx, top+.035, cz+.08), (cx+dx, top+.12+.03*i, cz+.08), .04, col, 8)
    g.box(m, (cx-.25, top+.05, cz+.06), (.30, .03, .14), 'PineLight')


def pantry_dresser(m, W, D, wood='Mesquite'):
    # Dresser: cupboard below, open shelves above with jars, bread and plates, herbs hanging from the top (food recovery).
    cx, cz = -W/2, -D/2; base = .80; tall = 1.80
    g.box(m, (cx, base/2, cz), (W-.06, base, D-.08), wood)
    for sx in (-1, 1):
        g.box(m, (cx+sx*(W/4-.02), .42, cz+(D-.08)/2+.008), (W/2-.10, .60, .015), wood, shade=1)
        g.box(m, (cx+sx*.05, .48, cz+(D-.08)/2+.022), (.02, .07, .02), 'Iron')
    g.box(m, (cx, base+.02, cz), (W-.02, .04, D-.06), 'WoodDark')
    g.box(m, (cx, (base+tall)/2, cz-D/2+.06), (W-.10, tall-base, .03), wood, shade=1)          # back board
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.06), (base+tall)/2, cz-.06), (.04, tall-base, D-.24), wood)
    for y in (1.15, 1.48, tall-.02): g.box(m, (cx, y, cz-.06), (W-.10, .03, D-.24), wood)
    for i, col in enumerate(('Ceramic', 'Copper', 'Ceramic', 'Green')):
        cyl(m, (cx-.30+i*.20, 1.165, cz-.05), (cx-.30+i*.20, 1.165+.14+.03*(i % 2), cz-.05), .05, col, 8, .045)
    g.box(m, (cx+.20, 1.53, cz-.05), (.22, .07, .12), 'Floor')                                  # bread loaf
    for i in range(3):                                                                         # plates on edge
        k.limb(m, [((cx-.32+i*.10, 1.60, cz-.10), .08, .08), ((cx-.32+i*.10, 1.60, cz-.085), .08, .08)], 'Ceramic', sides=10, forward_hint=(0, 1, 0))
    for i in range(3):                                                                         # herb bunches
        x = cx-.25+i*.25
        g.box(m, (x, tall-.12, cz+.02), (.06, .18, .06), 'Green', taper=.4)


def trophy_cabinet(m, W, D, wood='Mesquite'):
    # Glazed cabinet of medals and cups with a carved plaque and a trophy cup on top (sale price: a merchant's honours).
    cx, cz = -W/2, -D/2; h = 1.35
    front, floor = glazed_cabinet(m, W, D, h, wood, 'Iron')
    g.box(m, (cx, .78, cz-.03), (W-.14, .03, D-.20), wood)                                       # middle shelf
    for i, col in enumerate(('Amber', 'Copper', 'Steel')):                                     # cups and medals inside
        cyl(m, (cx-.30+i*.30, .13, cz-.04), (cx-.30+i*.30, .30, cz-.04), .05, col, 6, .07)
        g.box(m, (cx-.30+i*.30, .90, cz-.10), (.08, .08, .012), col)                                 # medal on the back board
        g.box(m, (cx-.30+i*.30, .97, cz-.10), (.03, .06, .008), 'Cloth')                            # its ribbon
    g.box(m, (cx-.18, h+.30, cz-.10), (.40, .30, .04), wood, shade=1)                          # plaque
    g.box(m, (cx-.18, h+.30, cz-.075), (.24, .06, .01), 'Amber')
    cyl(m, (cx+.28, h+.11, cz), (cx+.28, h+.15, cz), .05, 'WoodDark', 8)                       # trophy cup on top
    cyl(m, (cx+.28, h+.15, cz), (cx+.28, h+.22, cz), .015, 'Amber', 6)
    cyl(m, (cx+.28, h+.22, cz), (cx+.28, h+.35, cz), .04, 'Amber', 8, .08)
    for sx in (-1, 1): g.box(m, (cx+.28+sx*.09, h+.30, cz), (.02, .07, .02), 'Amber')


def boot_cabinet(m, W, D, wood='Mesquite'):
    # Open-fronted boot cabinet: two slatted shelves holding three pairs of boots, a boot and brush on top (boots life).
    cx, cz = -W/2, -D/2; h = .95
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.05), h/2, cz), (.05, h, D-.08), wood)
    g.box(m, (cx, h/2, cz-D/2+.05), (W-.10, h, .03), wood, shade=1)
    for y in (.05, .45, h): g.box(m, (cx, y, cz), (W-.06, .04, D-.08), wood if y < h else 'WoodDark')
    def boot(x, y, z, color):
        g.box(m, (x, y+.11, z-.03), (.08, .22, .09), color)
        g.box(m, (x, y+.04, z+.03), (.08, .08, .17), color, taper=.9)
        g.box(m, (x, y+.006, z+.02), (.09, .012, .2), 'WoodDark')
    for j, (y, col) in enumerate(((.07, 'Leather'), (.47, 'Fur'))):
        for i in range(3 if j == 0 else 2):
            for dx in (-.045, .045): boot(cx-.30+i*.30+dx, y, cz+.03, col if i != 1 else 'WoodDark')
    boot(cx+.25, h+.02, cz, 'Leather')
    g.box(m, (cx-.20, h+.05, cz+.05), (.16, .04, .06), wood); g.box(m, (cx-.20, h+.03, cz+.05), (.14, .02, .05), 'CreamCloth')


def cloth_press(m, W, D, wood='Mesquite'):
    # Screw press: two heavy posts, crossbeam with an iron screw and capstan bar, pressing plate on a stack of cloth (shirt life).
    cx, cz = -W/2, -D/2; h = 1.70
    for sx in (-1, 1):
        g.box(m, (cx+sx*.36, h/2, cz), (.12, h, .16), wood)
        g.box(m, (cx+sx*.36, .05, cz), (.18, .10, .70), 'WoodDark')                           # feet
        g.beam(m, (cx+sx*.36, .10, cz+.28), (cx+sx*.36, .55, cz+.04), .06, wood)              # braces
        g.beam(m, (cx+sx*.36, .10, cz-.28), (cx+sx*.36, .55, cz-.04), .06, wood)
    g.box(m, (cx, h-.08, cz), (.88, .16, .18), wood)                                           # crossbeam
    g.box(m, (cx, .36, cz), (.66, .08, .44), wood)                                             # bed
    for i, col in enumerate(('CreamCloth', 'Cloth', 'CreamCloth', 'Leather', 'CreamCloth')):    # cloth stack
        g.box(m, (cx, .42+i*.035, cz), (.54-i*.01, .03, .36-i*.01), col, shade=1+i % 3)
    g.box(m, (cx, .66, cz), (.58, .05, .40), wood)                                             # pressing plate
    cyl(m, (cx, .69, cz), (cx, h, cz), .045, 'Iron', 8)                                        # screw
    for i in range(9): k.limb(m, [((cx, .75+i*.10, cz), .055, .055), ((cx, .77+i*.10, cz), .055, .055)], 'Iron', sides=8)  # thread
    cyl(m, (cx-.30, 1.10, cz), (cx+.30, 1.10, cz), .025, 'WoodDark', 6)                        # capstan bar
    k.limb(m, [((cx, 1.07, cz), .07, .07), ((cx, 1.13, cz), .07, .07)], 'Iron', sides=8)


def hat_display(m, W, D, wood='Mesquite'):
    # Tall three-tier hat stand, each tier holding a hat (hat life).
    cx, cz = -W/2, -D/2
    k.limb(m, [((cx, 0, cz), .20, .20), ((cx, .05, cz), .19, .19)], 'WoodDark', sides=8)
    cyl(m, (cx, .05, cz), (cx, 1.60, cz), .025, wood, 6)
    def hat(at, brim, crown, ch, color, band):
        x, y, z = at
        k.limb(m, [((x, y, z), brim, brim), ((x, y+.015, z), brim, brim)], color, sides=10)
        k.limb(m, [((x, y+.012, z), crown, crown*.9), ((x, y+ch, z), crown*.82, crown*.72)], color, sides=8)
        k.limb(m, [((x, y+.015, z), crown+.003, crown*.9+.003), ((x, y+.045, z), crown+.002, crown*.9+.002)], band, sides=8, cap_start=False, cap_end=False)
    for i, (ang, y, col, band) in enumerate(((40, .62, 'Leather', 'CreamCloth'), (160, 1.02, 'Fur', 'Leather'), (280, 1.42, 'Cloth', 'Amber'))):
        a = math.radians(ang); d = (math.cos(a)*.10, 0, math.sin(a)*.10)
        cyl(m, (cx, y-.04, cz), (cx+d[0], y, cz+d[2]), .012, wood, 5)
        k.limb(m, [((cx+d[0], y-.02, cz+d[2]), .05, .05), ((cx+d[0], y, cz+d[2]), .05, .05)], wood, sides=8)
        hat((cx+d[0], y, cz+d[2]), .12, .07, .10, col, band)
    cyl(m, (cx, 1.60, cz), (cx, 1.66, cz), .04, 'Iron', 8)


# ------------------------------------------------------------------ shared pieces for batch D

def boot_pair(m, x, z, y=0.0, color='Leather', scale=1.0):
    for dx in (-.045*scale, .045*scale):
        g.box(m, (x+dx, y+.11*scale, z-.03*scale), (.08*scale, .22*scale, .09*scale), color)
        g.box(m, (x+dx, y+.04*scale, z+.03*scale), (.08*scale, .08*scale, .17*scale), color, taper=.9)
        g.box(m, (x+dx, y+.006, z+.02*scale), (.09*scale, .012, .20*scale), 'WoodDark')


def hat_on(m, at, brim, crown, ch, color, band):
    x, y, z = at
    k.limb(m, [((x, y, z), brim, brim), ((x, y+.015, z), brim, brim)], color, sides=10)
    k.limb(m, [((x, y+.012, z), crown, crown*.9), ((x, y+ch, z), crown*.82, crown*.72)], color, sides=8)
    k.limb(m, [((x, y+.015, z), crown+.003, crown*.9+.003), ((x, y+.045, z), crown+.002, crown*.9+.002)], band, sides=8, cap_start=False, cap_end=False)


def glazed_cabinet(m, W, D, h, wood, metal, glass_doors=True):
    """Tall two-door cabinet; returns (front z of the doors, inner floor y). Glazed cabinets are HOLLOW (back, sides,
    floor and top boards) with door FRAMES and thin glint strips instead of opaque panes, because the palette has no
    transparency: what stands inside stays visible. Solid (wardrobe) cabinets keep closed panelled doors."""
    cx, cz = -W/2, -D/2; bw, bd = W-.08, D-.16; bz = cz-.03
    g.box(m, (cx, .05, cz), (W-.04, .10, D-.06), 'WoodDark')
    front = bz+bd/2
    if glass_doors:
        g.box(m, (cx, h/2+.05, bz-bd/2+.015), (bw, h, .03), wood, shade=1)                        # back board
        for sx in (-1, 1): g.box(m, (cx+sx*(bw/2-.015), h/2+.05, bz), (.03, h, bd), wood)           # sides
        for y in (.115, h+.035): g.box(m, (cx, y, bz), (bw, .03, bd), wood)                          # floor, top
    else:
        g.box(m, (cx, h/2+.05, bz), (bw, h, bd), wood)
    for sx in (-1, 1):
        door = (cx+sx*(W/4-.02), h/2+.10, front+.01); dw, dh = W/2-.10, h-.16
        if glass_doors:
            for dx in (-1, 1): g.box(m, (door[0]+dx*(dw/2-.025), door[1], door[2]), (.05, dh, .02), wood)   # stiles
            for dy in (-1, 1): g.box(m, (door[0], door[1]+dy*(dh/2-.025), door[2]), (dw, .05, .02), wood)   # rails
            g.box(m, (door[0], door[1], door[2]), (dw-.08, .025, .015), wood)                         # glazing bar
            g.box(m, (door[0]+dw*.18, door[1]+dh*.18, door[2]+.004), (.012, dh*.30, .004), 'GlassLight', axes=((1, 0, 0), k.unit((.35, 1, 0)), (0, 0, 1)))   # glint
        else:
            g.box(m, door, (dw, dh, .015), wood, shade=1)
        g.box(m, (cx+sx*.05, h/2+.05, front+.03), (.02, .10, .02), metal)
        for y in (.30, h-.15): g.box(m, (cx+sx*(W/2-.07), y, front+.02), (.04, .06, .02), metal)   # hinges
    g.box(m, (cx, h+.08, cz-.02), (W, .06, D-.08), 'WoodDark')                                # crown
    return front, .10


def bow_shape(m, x0, x1, y, z, depth, wood, vertical=False, grip='Leather'):
    pts = []
    for i in range(9):
        t = i/8
        if vertical: pts.append((x0+depth*math.sin(math.pi*t), y+(x1-y)*t, z))
        else: pts.append((x0+(x1-x0)*t, y+depth*math.sin(math.pi*t), z))
    for i in range(8): g.beam(m, pts[i], pts[i+1], .026-.010*abs(i-3.5)/3.5, grip if i in (3, 4) else wood, .026)
    g.beam(m, pts[0], pts[8], .006, 'CreamCloth', .006)


# ------------------------------------------------------------------ batch D: level 6 (mesquite, iron)

def clothing_armoire(m, W, D, wood='Mesquite'):
    # Tall armoire with panelled doors, a drawer and folded trousers stacked on top (pants life).
    cx, cz = -W/2, -D/2; h = 1.62
    front, _ = glazed_cabinet(m, W, D, h, wood, 'Iron', glass_doors=False)
    for sx in (-1, 1): g.box(m, (cx+sx*(W/4-.02), h/2+.10, front+.02), (W/2-.24, h-.50, .01), wood, shade=3)
    for i, col in enumerate(('Leather', 'Cloth', 'CreamCloth', 'Cloth')):
        g.box(m, (cx-.10, h+.14+i*.045, cz-.02), (.36-i*.02, .04, .24), col, shade=1+i % 3)
    g.box(m, (cx+.28, h+.21, cz-.02), (.20, .20, .20), 'Leather')                              # hat box
    g.box(m, (cx+.28, h+.315, cz-.02), (.22, .03, .22), 'WoodDark')


def tool_forge(m, W, D, wood='Mesquite'):
    # Stone forge hearth with glowing coals, leather bellows and a hood, an anvil on a stump with an axe head (axe life).
    cx, cz = -W/2, -D/2
    hx, hz = cx-.22, cz-.18                                                                     # hearth at the back-right
    g.box(m, (hx, .38, hz), (.52, .76, .52), 'Stone')
    g.box(m, (hx, .77, hz), (.42, .03, .42), 'Basalt')
    for i in range(6): g.box(m, (hx-.12+(i % 3)*.12, .80, hz-.07+(i // 3)*.14), (.09, .04, .09), 'Amber' if i % 2 else 'Brick')
    g.box(m, (hx, 1.25, hz-.06), (.48, .26, .36), 'Iron', taper=.55)                            # hood
    for sx in (-1, 1): g.box(m, (hx+sx*.21, .99, hz-.20), (.03, .44, .03), 'Iron')              # hood posts on the hearth
    cyl(m, (hx, 1.36, hz-.06), (hx, 1.95, hz-.06), .07, 'Iron', 8)
    g.box(m, (hx+.34, .55, hz), (.10, .26, .34), 'Leather', taper=.6)                           # bellows
    cyl(m, (hx+.29, .55, hz), (hx+.24, .58, hz), .02, 'Iron', 5)
    k.limb(m, [((cx+.20, 0, cz+.16), .14, .14), ((cx+.20, .42, cz+.16), .13, .13)], 'WoodDark', sides=8)   # stump
    g.box(m, (cx+.20, .50, cz+.16), (.30, .14, .12), 'Iron', taper=.8)                           # anvil
    g.box(m, (cx+.35, .54, cz+.16), (.10, .05, .06), 'Iron', taper=.4)                           # horn
    g.box(m, (cx+.16, .60, cz+.16), (.10, .06, .03), 'Steel')                                    # axe head on the anvil
    for sx in (-1, 1): cyl(m, (cx+.10, .58, cz+.24), (cx+.28+sx*.01, .58, cz+.30), .008, 'Iron', 4)   # tongs
    g.box(m, (cx-.32, .25, cz+.30), (.16, .25, .16), wood)                                     # quench bucket
    g.box(m, (cx-.32, .37, cz+.30), (.13, .01, .13), 'GlassDark')


def bow_cabinet(m, W, D, wood='Mesquite'):
    # Glazed cabinet with two bows standing inside and a quiver of arrows on top (hunting-tool life).
    cx, cz = -W/2, -D/2; h = 1.40
    front, floor = glazed_cabinet(m, W, D, h, wood, 'Iron')
    for i, dx in enumerate((-.22, .18)):
        bow_shape(m, cx+dx, floor+h-.10, floor+.10, front-.04, .07 if i else -.07, wood, vertical=True)
    g.box(m, (cx+.20, h+.25, cz-.02), (.12, .34, .12), 'Leather', taper=.85)                   # quiver
    for i in range(4): g.box(m, (cx+.17+(i % 2)*.05, h+.45, cz-.04+(i // 2)*.04), (.01, .06, .03), 'CreamCloth')
    g.box(m, (cx-.20, h+.14, cz), (.30, .06, .05), 'CreamCloth')                                # spare string coil


def reel_workshop(m, W, D, wood='Mesquite'):
    # Workshop table with a reel jig in a vise, opened reel parts, a spool rack and rods laid across (rod life).
    cx, cz = -W/2, -D/2; top = .80
    g.box(m, (cx, top, cz), (W-.06, .06, D-.06), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.08), top/2, cz+sz*(D/2-.08)), (.07, top, .07), 'WoodDark')
    g.box(m, (cx, .18, cz), (W-.20, .03, D-.20), wood)
    g.box(m, (cx-.25, top+.08, cz+.25), (.14, .10, .16), 'WoodDark')                            # vise with a reel
    k.limb(m, [((cx-.25, top+.20, cz+.25), .08, .08), ((cx-.20, top+.20, cz+.25), .08, .08)], 'Iron', sides=10, forward_hint=(0, 1, 0))
    for i in range(5): g.box(m, (cx+.05+i*.06, top+.04, cz+.25), (.04, .02, .04), ('Copper', 'Steel', 'Iron', 'Copper', 'Steel')[i])  # parts
    for i in range(2): cyl(m, (cx-.40, top+.05, cz-.05+i*.06), (cx+.42, top+.05, cz-.15+i*.06), .012, 'PineLight', 6, .006)  # rods
    g.box(m, (cx+.20, top+.25, cz-.38), (.50, .40, .05), wood, shade=1)                         # spool rack board
    for r in range(2):
        for c in range(4): cyl(m, (cx+.02+c*.12, top+.15+r*.18, cz-.35), (cx+.02+c*.12, top+.15+r*.18, cz-.29), .035, ('CreamCloth', 'Green', 'Cloth', 'GlassLight')[(r+c) % 4], 8)


def pack_maker_bench(m, W, D, wood='Mesquite'):
    # Long bench with a finished pack, a pack frame being fitted, canvas rolls and straps on a back rail (backpack life).
    cx, cz = -W/2, -D/2; top = .80
    g.box(m, (cx, top, cz+.03), (W-.06, .06, D-.14), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.08), top/2, cz+.03+sz*(D/2-.12)), (.07, top, .07), 'WoodDark')
    g.box(m, (cx, 1.25, cz-D/2+.04), (W-.06, .06, .05), wood)                                   # back rail
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.05), 1.02, cz-D/2+.04), (.06, .46, .06), 'WoodDark')
    for i in range(5): g.box(m, (cx-.50+i*.25, 1.08, cz-D/2+.07), (.035, .30, .01), ('Leather', 'WoodDark', 'Leather', 'CreamCloth', 'Leather')[i])   # straps
    g.box(m, (cx+.42, top+.25, cz+.02), (.34, .44, .20), 'Leather')                             # finished pack
    g.box(m, (cx+.42, top+.43, cz+.04), (.35, .10, .22), 'Cloth', taper=.95)
    for sx in (-1, 1): g.box(m, (cx+.42+sx*.10, top+.28, cz+.125), (.03, .30, .012), 'WoodDark')
    for sx in (-1, 1): g.box(m, (cx-.05+sx*.14, top+.20, cz+.0), (.03, .34, .03), wood)       # pack frame being fitted
    for y in (.08, .30): g.box(m, (cx-.05, top+y, cz), (.32, .03, .03), wood)
    for i, col in enumerate(('CreamCloth', 'Cloth')): cyl(m, (cx-.62, top+.06+i*.09, cz-.10), (cx-.32, top+.06+i*.09, cz-.10), .045, col, 6)


def mesquite_chest(m, W, D, wood='Mesquite'): chest_body(m, W, D, wood, 'Iron')


# ------------------------------------------------------------------ batch D: level 7 (oak, iron)

def master_loom(m, W, D, wood='Oak'):
    # The loom grown to a master loom: wider frame, a two-colour warp, finished bolts stacked beside it (craft quality).
    loom(m, W-.50, D, wood)
    cx, cz = -W/2, -D/2
    for i, col in enumerate(('Cloth', 'CreamCloth', 'Green', 'Cloth')):                         # cloth bolts stack at the -X end
        cyl(m, (-W+.06, .07+i*.12, cz-.30+i*.02), (-W+.42, .07+i*.12, cz-.30+i*.02), .055, col, 6)
    g.box(m, (-W+.24, .02, cz-.25), (.40, .04, .30), 'WoodDark')
    g.box(m, (-W+.24, .13, cz+.22), (.30, .26, .26), 'CreamCloth')                             # basket of yarn skeins
    g.box(m, (-W+.24, .27, cz+.22), (.26, .04, .22), 'Cloth')


def dining_table(m, W, D, wood='Oak'):
    # Oak table with benches on both long sides, laid with plates, bowls, bread, a jug and a candle (food recovery).
    cx, cz = -W/2, -D/2; top = .76
    g.box(m, (cx, top, cz), (W-.14, .06, .52), wood)
    for sx in (-1, 1):
        g.box(m, (cx+sx*(W/2-.20), top/2, cz), (.08, top, .40), 'WoodDark')
        g.box(m, (cx+sx*(W/2-.20), .06, cz), (.12, .06, .48), 'WoodDark')
    g.box(m, (cx, .20, cz), (W-.46, .05, .05), 'WoodDark')
    for sz in (-1, 1):                                                                         # benches
        g.box(m, (cx, .44, cz+sz*.39), (W-.30, .05, .18), wood)
        for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.25), .22, cz+sz*.39), (.05, .44, .14), 'WoodDark')
    for i in range(3):
        for sz in (-1, 1):
            x = cx-.48+i*.48
            k.limb(m, [((x, top+.03, cz+sz*.15), .09, .09), ((x, top+.04, cz+sz*.15), .09, .09)], 'Ceramic', sides=10)   # plates
    for x, col in ((cx-.24, 'Ceramic'), (cx+.24, 'Ceramic')): cyl(m, (x, top+.03, cz), (x, top+.09, cz), .07, col, 8, .05)
    g.box(m, (cx, top+.07, cz), (.24, .07, .12), 'Floor')                                      # bread
    cyl(m, (cx+.48, top+.03, cz), (cx+.48, top+.20, cz), .05, 'Copper', 8, .035)                # jug
    cyl(m, (cx-.48, top+.03, cz), (cx-.48, top+.07, cz), .04, 'Iron', 8)                        # candle holder
    cyl(m, (cx-.48, top+.07, cz), (cx-.48, top+.18, cz), .015, 'CreamCloth', 6)
    g.box(m, (cx-.48, top+.20, cz), (.012, .025, .012), 'Amber')


def gallery_pedestal(m, W, D, wood='Oak'):
    # Marble pedestal with an amber-gilded vase and a small framed price card (sale price: goods shown at their best).
    cx, cz = -W/2, -D/2
    g.box(m, (cx, .05, cz), (.40, .10, .40), 'Marble')
    g.box(m, (cx, .55, cz), (.28, .90, .28), 'Marble', shade=1)
    g.box(m, (cx, 1.03, cz), (.38, .06, .38), 'Marble')
    k.limb(m, [((cx, 1.06, cz), .07, .07), ((cx, 1.14, cz), .11, .11), ((cx, 1.28, cz), .09, .09), ((cx, 1.40, cz), .05, .05), ((cx, 1.46, cz), .07, .07)], 'Amber', sides=10)
    k.limb(m, [((cx, 1.18, cz), .112, .112), ((cx, 1.21, cz), .112, .112)], 'Copper', sides=10, cap_start=False, cap_end=False)
    g.box(m, (cx, .70, cz+.145), (.18, .12, .012), wood)                                        # framed price card
    g.box(m, (cx, .70, cz+.152), (.14, .08, .006), 'Paper')


def travel_sofa(m, W, D, wood='Oak'):
    # Leather-upholstered sofa with a rolled travel blanket, a walking stick and boots by its end (boots life).
    sofa(m, W, D, wood, 'Leather')
    cx, cz = -W/2, -D/2
    cyl(m, (cx-.20, .43, cz+.02), (cx+.25, .43, cz+.02), .06, 'Fur', 6)                         # rolled blanket on the seat
    cyl(m, (-W+.04, .02, cz-.18), (-W+.10, .95, cz-.20), .015, 'WoodDark', 6)                    # walking stick


def fine_cloth_workstation(m, W, D, wood='Oak'):
    # Long cutting table with fine cloth, shears and chalk, a dress form wearing a shirt at one end (shirt life).
    cx, cz = -W/2, -D/2; top = .80
    g.box(m, (cx-.15, top, cz), (W-.36, .05, D-.08), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx-.15+sx*((W-.36)/2-.06), top/2, cz+sz*(D/2-.08)), (.06, top, .06), 'WoodDark')
    g.box(m, (cx-.15, .16, cz), (W-.50, .03, D-.16), wood)
    for i, col in enumerate(('CreamCloth', 'Cloth', 'Green')): cyl(m, (cx-.70+i*.02, .21+i*.08, cz), (cx-.30+i*.02, .21+i*.08, cz), .04, col, 6)   # bolts below
    g.box(m, (cx-.25, top+.035, cz+.02), (.60, .01, .34), 'CreamCloth')                       # fine cloth laid out
    g.box(m, (cx-.25, top+.042, cz+.02), (.46, .003, .003), 'Amber')
    for sx in (-1, 1): g.box(m, (cx+.10+sx*.016, top+.045, cz-.12), (.012, .006, .12), 'Steel', axes=((1, 0, 0), (0, 1, 0), k.unit((sx*.25, 0, 1))))
    # Dress form at the +X end (screen-left) wearing a shirt.
    fx = -.16
    k.limb(m, [((fx, 0, cz), .14, .14), ((fx, .04, cz), .13, .13)], 'WoodDark', sides=8)
    cyl(m, (fx, .04, cz), (fx, .95, cz), .02, wood, 6)
    k.limb(m, [((fx, .95, cz), .10, .08), ((fx, 1.15, cz), .12, .09), ((fx, 1.38, cz), .14, .10), ((fx, 1.46, cz), .06, .05)], 'Cloth', sides=8)
    k.limb(m, [((fx, 1.46, cz), .035, .035), ((fx, 1.56, cz), .03, .03)], wood, sides=6)


def milliner_bench(m, W, D, wood='Oak'):
    # Hat-maker's bench: two wooden hat blocks with hats being shaped, ribbon rolls and a feather (hat life).
    cx, cz = -W/2, -D/2; top = .78
    g.box(m, (cx, top, cz), (W-.06, .05, D-.10), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.07), top/2, cz+sz*(D/2-.08)), (.06, top, .06), 'WoodDark')
    g.box(m, (cx, .18, cz), (W-.16, .03, D-.18), wood)
    for i, (dx, col, band) in enumerate(((.24, 'Fur', 'Leather'), (-.08, 'Cloth', 'Amber'))):
        k.limb(m, [((cx+dx, top+.025, cz), .07, .07), ((cx+dx, top+.18, cz), .075, .07), ((cx+dx, top+.24, cz), .05, .05)], 'PineLight', sides=8)   # hat block
        hat_on(m, (cx+dx, top+.10, cz), .13, .08, .12, col, band)
    for i, col in enumerate(('Amber', 'Green', 'Cloth')): cyl(m, (cx-.32, top+.03+i*.0, cz-.10+i*.09), (cx-.32, top+.08, cz-.10+i*.09), .035, col, 8)
    g.box(m, (cx-.40, top+.03, cz+.13), (.20, .006, .03), 'Green', axes=((1, 0, 0), (0, 1, 0), k.unit((.4, 0, 1))))   # feather


# ------------------------------------------------------------------ batch D: level 8 (oak, steel)

def frontier_wardrobe(m, W, D, wood='Oak'):
    # Grand wardrobe: mirrored left door, steel fittings, a carved crown and folded trousers with a hat box on top (pants life).
    cx, cz = -W/2, -D/2; h = 1.55
    front, _ = glazed_cabinet(m, W, D, h, wood, 'Steel', glass_doors=False)
    g.box(m, (cx+W/4-.02, h/2+.12, front+.025), (W/2-.24, h-.50, .01), 'GlassLight', shade=2)     # mirror
    g.box(m, (cx-W/4+.02, h/2+.10, front+.02), (W/2-.24, h-.50, .01), wood, shade=3)
    g.box(m, (cx, h+.14, cz-.02), (W-.20, .06, D-.14), 'WoodDark', taper=.85)                   # carved crown
    for i, col in enumerate(('Leather', 'CreamCloth', 'Cloth')):
        g.box(m, (cx-.12, h+.20+i*.045, cz-.02), (.34-i*.02, .04, .24), col, shade=1+i % 3)
    g.box(m, (cx+.28, h+.28, cz-.02), (.22, .22, .22), 'Leather')
    g.box(m, (cx+.28, h+.40, cz-.02), (.24, .03, .24), 'Steel')


def master_tool_station(m, W, D, wood='Oak'):
    # Heavy station: axes hung on a back board, a bench grinder, an anvil corner and a sharpening bench (axe life).
    cx, cz = -W/2, -D/2; top = .82
    g.box(m, (cx, top, cz+.10), (W-.06, .07, D-.30), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.08), top/2, cz+.10+sz*((D-.30)/2-.06)), (.08, top, .08), 'WoodDark')
    g.box(m, (cx, .18, cz+.10), (W-.20, .04, D-.40), wood)
    g.box(m, (cx, 1.30, cz-D/2+.05), (W-.06, .90, .05), wood, shade=1)                          # back board
    for sx in (-1, 1): g.box(m, (cx+sx*(W/2-.05), 1.00, cz-D/2+.05), (.07, 1.20, .07), 'WoodDark')
    for i in range(4):                                                                         # hanging axes
        x = cx-.55+i*.36
        cyl(m, (x, .98, cz-D/2+.10), (x, 1.55, cz-D/2+.10), .016, 'PineLight', 6)
        g.box(m, (x+.05, 1.48, cz-D/2+.10), (.10, .12, .025), 'Steel' if i % 2 else 'Iron')
    k.limb(m, [((cx+.45, top+.20, cz+.15), .11, .11), ((cx+.49, top+.20, cz+.15), .11, .11)], 'Stone', sides=10, forward_hint=(0, 1, 0))   # grinder
    g.box(m, (cx+.53, top+.10, cz+.15), (.10, .14, .12), 'Steel')
    g.box(m, (cx-.40, top+.12, cz+.20), (.28, .14, .12), 'Iron', taper=.8)                     # bench anvil
    g.box(m, (cx-.05, top+.05, cz+.25), (.30, .03, .07), 'Stone')                              # whetstone
    g.box(m, (cx-.05, top+.08, cz+.25), (.10, .03, .03), 'Steel')
    g.box(m, (cx+.10, .30, cz+.30), (.40, .22, .20), 'Leather')                                 # tool roll on the shelf


def hunters_cabinet(m, W, D, wood='Oak'):
    # Glazed cabinet holding a bow and a slingshot, antlers mounted on top (hunting-tool life).
    cx, cz = -W/2, -D/2; h = 1.40
    front, floor = glazed_cabinet(m, W, D, h, wood, 'Steel')
    bow_shape(m, cx-.20, floor+h-.12, floor+.10, front-.04, -.07, wood, vertical=True)
    for side in (-1, 1): cyl(m, (cx+.22, floor+.80, front-.04), (cx+.22+side*.06, floor+.92, front-.04), .012, wood, 5)   # slingshot fork
    cyl(m, (cx+.22, floor+.60, front-.04), (cx+.22, floor+.80, front-.04), .015, wood, 6)
    g.box(m, (cx, h+.20, cz-.10), (.26, .24, .04), wood, shade=1)                               # antler plaque
    for side in (-1, 1):
        base = (cx+side*.05, h+.24, cz-.06)
        tip = (cx+side*.28, h+.46, cz-.06)
        cyl(m, base, tip, .018, 'CreamCloth', 5, .008)
        for t in (.55,):
            mid = k.lerp(base, tip, t)
            cyl(m, mid, (mid[0]+side*.02, mid[1]+.08, mid[2]+.03), .012, 'CreamCloth', 5, .005)


def anglers_cabinet(m, W, D, wood='Oak'):
    # Glazed cabinet of standing rods and reels with a mounted fish on top (rod life).
    cx, cz = -W/2, -D/2; h = 1.50
    front, floor = glazed_cabinet(m, W, D, h, wood, 'Steel')
    for i in range(4):
        x = cx-.30+i*.20
        cyl(m, (x, floor+.04, front-.05), (x, floor+h-.08, front-.06), .010, 'PineLight', 6, .005)
        k.limb(m, [((x, floor+.40, front-.03), .035, .035), ((x+.025, floor+.40, front-.03), .035, .035)], 'Steel', sides=6, forward_hint=(0, 1, 0))
    g.box(m, (cx, h+.26, cz-.10), (.60, .26, .04), wood, shade=1)                               # fish plaque
    fish = [(cx-.20, .05), (cx-.08, .08), (cx+.06, .07), (cx+.16, .04)]
    for (x, r) in fish: g.box(m, (x, h+.26, cz-.07), (.13, r*2, .03), 'Glass', taper=1.0)
    g.box(m, (cx+.24, h+.26, cz-.07), (.06, .12, .02), 'Glass', taper=.3)                        # tail
    g.box(m, (cx-.25, h+.27, cz-.05), (.02, .02, .01), 'GlassDark')                             # eye


def expedition_workbench(m, W, D, wood='Oak'):
    # Big map table: an expedition map with a compass, two packed backpacks, a rope coil and a lantern (backpack life).
    cx, cz = -W/2, -D/2; top = .80
    g.box(m, (cx, top, cz), (W-.06, .07, D-.08), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (cx+sx*(W/2-.08), top/2, cz+sz*(D/2-.08)), (.08, top, .08), 'WoodDark')
    g.box(m, (cx, .18, cz), (W-.20, .04, D-.20), wood)
    g.box(m, (cx-.10, top+.04, cz+.10), (.70, .006, .46), 'Paper')                              # map
    for x0, z0, x1, z1 in ((-.40, .25, -.10, .05), (-.10, .05, .15, .18)): g.beam(m, (cx+x0, top+.046, cz+z0), (cx+x1, top+.046, cz+z1), .008, 'Brick', .002)
    k.limb(m, [((cx+.05, top+.04, cz+.0), .05, .05), ((cx+.05, top+.06, cz+.0), .05, .05)], 'Copper', sides=10)   # compass
    for i, (dx, col) in enumerate(((.45, 'Leather'), (.10, 'Cloth'))):                          # packs (one on the table, one on the shelf)
        y = top+.04 if i == 0 else .20
        g.box(m, (cx+dx, y+.22, cz-.25), (.32, .44, .20), col)
        g.box(m, (cx+dx, y+.40, cz-.23), (.33, .10, .22), 'Leather', taper=.95)
        cyl(m, (cx+dx-.16, y+.06, cz-.25), (cx+dx+.16, y+.06, cz-.25), .05, 'CreamCloth', 6)
    k.limb(m, [((cx-.55, top+.04, cz-.20), .10, .10), ((cx-.55, top+.08, cz-.20), .10, .10)], 'CreamCloth', sides=10)   # rope coil
    g.box(m, (cx-.55, top+.12, cz+.25), (.10, .14, .10), 'Iron'); g.box(m, (cx-.55, top+.12, cz+.25), (.07, .10, .107), 'Amber')   # lantern


def reinforced_oak_chest(m, W, D, wood='Oak'):
    chest_body(m, W, D, wood, 'Steel')
    cx, cz = -W/2, -D/2
    for x in (cx-W/4, cx+W/4): g.box(m, (x, .26, cz+(D-.08)/2+.008), (.05, .40, .012), 'Steel')   # extra bands


BUILDERS = {'Tool rack': tool_rack, 'Worktable': worktable, 'Bedroll': bedroll, 'Market sign': market_sign, 'Boot bench': boot_bench,
            'Sewing basket': sewing_basket, 'Hat peg': hat_peg,
            'Trouser rack': trouser_rack, 'Chopping block': chopping_block, 'Sling stand': sling_stand, 'Rod rack': rod_rack,
            'Bag frame': bag_frame, 'Pine chest': pine_chest, 'Loom': loom, 'Clay cookstove': clay_cookstove,
            'Display counter': display_counter, 'Sofa': sofa, 'Sewing desk': sewing_desk, 'Hat cabinet': hat_cabinet,
            "Tailor's stool": tailors_stool, 'Whetstone stand': whetstone_stand, 'Bow workbench': bow_workbench,
            'Tackle desk': tackle_desk, 'Leather table': leather_table, 'Birch chest': birch_chest,
            'Artisan bench': artisan_bench, 'Pantry dresser': pantry_dresser, 'Trophy cabinet': trophy_cabinet,
            'Boot cabinet': boot_cabinet, 'Cloth press': cloth_press, 'Hat display': hat_display,
            'Clothing armoire': clothing_armoire, 'Tool forge': tool_forge, 'Bow cabinet': bow_cabinet,
            'Reel workshop': reel_workshop, 'Pack-maker bench': pack_maker_bench, 'Mesquite chest': mesquite_chest,
            'Master loom': master_loom, 'Dining table': dining_table, 'Gallery pedestal': gallery_pedestal,
            'Travel sofa': travel_sofa, 'Fine-cloth workstation': fine_cloth_workstation, 'Milliner bench': milliner_bench,
            'Frontier wardrobe': frontier_wardrobe, 'Master tool station': master_tool_station, "Hunter's cabinet": hunters_cabinet,
            "Angler's cabinet": anglers_cabinet, 'Expedition workbench': expedition_workbench, 'Reinforced oak chest': reinforced_oak_chest}
LIDS = {'Pine chest', 'Birch chest', 'Mesquite chest', 'Reinforced oak chest'}   # chests: a separate lid part hinged at the back top edge (chest_lid)


def asset_name(display): return 'Furniture' + ''.join(w.capitalize() for w in display.replace("'", '').replace('-', ' ').split())


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    rows = []
    for fid, display, level, category, (fw, fd), price, chest in CATALOG:
        W, D = fw*GRID_METRES, fd*GRID_METRES
        m = k.Mesh(asset_name(display), pal); build = BUILDERS[display]
        build(m, W, D) if level == 1 else build(m, W, D, WOOD[level])
        info = m.save(OUT, HEADER)
        verts = list(m.vertices); extra = {}
        if display in LIDS:
            lid, hinge = chest_lid(display, W, D, WOOD[level], METAL[level])
            lid_info = lid.save(OUT, HEADER)
            verts += [(p[0]+hinge[0], p[1]+hinge[1], p[2]+hinge[2]) for p in lid.vertices]   # closed lid in footprint space
            extra = dict(LidAsset=lid_info['name'], LidHinge=list(hinge), LidTriangles=lid_info['triangles'])
            info['triangles'] += lid_info['triangles']
        if display == 'Tool rack': extra = dict(PegBox=list(RACK_PEG_BOX), Spots=[c for s in rack_layout(W, D) for c in s])
        lo = [min(v[i] for v in verts) for i in range(3)]; hi = [max(v[i] for v in verts) for i in range(3)]
        # Footprint contract: inside [-W, 0] x [-D, 0] (1 cm tolerance), on the floor, at most 2.0 m tall.
        assert lo[0] >= -W-.01 and hi[0] <= .01 and lo[2] >= -D-.01 and hi[2] <= .01, (display, lo, hi)
        assert lo[1] >= -.001 and hi[1] <= 2.0, (display, lo, hi)
        assert info['triangles'] <= 600, (display, info['triangles'], 'over the 600-triangle budget')
        rows.append(dict(Id=fid, Name=display, Asset=info['name'], Level=level, Category=category, FootprintWidth=fw,
                         FootprintDepth=fd, PriceCash=price, IsChest=chest, IsExtra=fid in EXTRA, IsRack=fid in RACKS, Triangles=info['triangles'],
                         BoundsMin=lo, BoundsMax=hi, **extra))
        print('%-24s %4d tris  %dx%d  height %.2f m' % (info['name'], info['triangles'], fw, fd, hi[1]))
    (OUT/'FurnitureManifest.json').write_text(json.dumps(dict(
        Revision=4, Batch='A-D complete (levels 1-8, 48 types)', Palette='Assets/_Game/Art/Settlement/Houses/HousePalette.png',
        GridMetres=GRID_METRES, Pivot='front-left footprint corner; body in x [-width, 0], z [-depth, 0]',
        Furniture=rows), indent=2)+'\n', encoding='utf-8')


if __name__ == '__main__':
    main()
