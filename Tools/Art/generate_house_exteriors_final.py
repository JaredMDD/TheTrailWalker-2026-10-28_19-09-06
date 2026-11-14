"""Final house exteriors, stage 3 of the exterior redesign (Claude, November 13, 2026).

Batch 1: levels 1-4 and batch 2: levels 5-8, replacing HouseL01Front-HouseL08Front in place (same names, same palette slots and kit as
generate_house_meshes.py). The massing follows the approved blockouts (Docs/Art/Settlement/HouseExteriorRedesign/
Blockouts, revision 3); the detail follows the house kit (house_geometry.py). Interiors, grid guides, furnishing
studies and pet homes are NOT touched (redesign rule: the interiors keep their grids).
  L1 Small shed          4.6 x 2.9 m  as today
  L2 Extended shed       5.6 x 3.4 m  side lean-to (plank, mono-pitch roof) on the right
  L3 Proper wooden home  7.0 x 4.5 m  gabled porch roof over the door, chimney, gable window
  L4 Back room           7.6 x 5.2 m  raised rear gable (back room) rising behind the main roof
Run: python Tools/Art/generate_house_exteriors_final.py   (standard library only)
"""
from pathlib import Path
import json
import faceted_character_kit as k
import house_geometry as g
from house_specs import LEVELS, PALETTE
from house_palette_extension import FUTURE_WOODS, palette_uv

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Art/Settlement/Houses'
HEADER = 'The Trail Walker house art; metres; +Y up; +Z toward street camera; no gameplay.'
FINAL_LEVELS = (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12)
PLOT_WIDTH, PLOT_DEPTH, WING_FROM = 15.2, 19.0, 3.0

pal = k.Palette()
for spec in PALETTE: pal.add(*spec)
# Levels 9-16 also use the future woods (Maple, Cedar, Redwood, Ebony) in the palette's upper band (house_palette_extension):
# the base slots keep their U at V = .5, so levels 1-8 and every older mesh are unaffected.
BASE_SLOTS = pal.slots
pal_hi = k.Palette()
for spec in PALETTE + FUTURE_WOODS: pal_hi.add(*spec)

# Final dimensions per level (front width, ridge height, overall depth incl. eaves, eaves height), from the blockouts.
SPEC = {
    1: dict(Width=4.6, Height=2.9, Depth=3.8, Eaves=2.15),
    2: dict(Width=5.6, Height=3.4, Depth=4.3, Eaves=2.42),
    3: dict(Width=7.0, Height=4.5, Depth=4.8, Eaves=3.10),
    4: dict(Width=7.6, Height=5.2, Depth=5.8, Eaves=3.12, MainPeak=4.55),
    5: dict(Eaves=3.8), 6: dict(Eaves=3.8), 7: dict(Eaves=5.5), 8: dict(Eaves=5.8),
}


def shifted(m, dx, build):
    start = len(m.vertices); build(); m.transform(lambda p: (p[0] + dx, p[1], p[2]), start)


def block(m, level, w, depth, e, peak, wall_color, roof_color, wood, door_x, windows, chimneys, gable_window, rear_windows=True,
          upper=(), side_rows=(1.38,), gable_windows=(), fan=True, skirt=None, band=None, lower=None):
    """One gabled house body centred on x = 0 (front face z = 0, ridge toward the back), in the kit of
    generate_house_meshes.exterior(). depth: wall depth. windows: front (x, y, w, h).
    Batch 2 options: upper (front upper-floor windows, also on the back), side_rows (side window heights),
    gable_windows (half-storey windows in the front gable; the gable fan is dropped so no timber crosses them),
    skirt (a stone plinth band up to that height), band (a floor band height across the front).
    Batch 3 option: lower = (height, colour) builds the walls below that height in another material (stone ground floor)."""
    def W(mm, x0, x1, y0, y1, z, thick, holes, color):
        if lower and y0 < lower[0] < y1:
            g.wall(mm, x0, x1, y0, lower[0], z, thick, holes, lower[1]); g.wall(mm, x0, x1, lower[0], y1, z, thick, holes, color)
        else: g.wall(mm, x0, x1, y0, y1, z, thick, holes, color)
    half = w / 2 - .25; rear = .18 - depth
    stone = 'Brick' if level < 3 else 'Stone' if level < 5 else 'Granite' if level < 7 else 'Basalt'
    metal = 'Copper' if level == 4 else 'Iron' if level < 8 else 'Steel'
    all_front = list(windows) + list(upper)
    if level == 1:
        for x in (-half + .2, half - .2):
            for z in (-.1, rear + .2): g.box(m, (x, .14, z), (.5, .28, .5), stone)
    else:
        g.box(m, (0, .17, rear / 2), (w - .3, .34, depth - .02), stone)
        g.box(m, (0, .35, rear / 2), (w - .24, .10, depth + .06), wood)
    door_h = 1.65 if level == 1 else 1.92
    win_w, win_h = (.76, .85) if level < 3 else (.88, 1.02)
    side_z = g.side_window_positions(depth, level)
    openings = [(door_x - .57, door_x + .57, .33, .33 + door_h + .12)] if door_x is not None else []
    openings.extend((x - ww / 2, x + ww / 2, y - hh / 2, y + hh / 2) for x, y, ww, hh in all_front)
    W(m, -half, half, .32, e, -.08, .16, openings, wall_color)
    back = [(x - ww / 2, x + ww / 2, y - hh / 2, y + hh / 2) for x, y, ww, hh in all_front] if rear_windows else []
    W(m, -half, half, .32, e, rear + .14, .16, back, wall_color)
    for sign in (-1, 1):
        lo, hi = sorted((g.along(sign, rear + .06), g.along(sign, 0.0)))
        holes = [(min(g.along(sign, z - win_w / 2), g.along(sign, z + win_w / 2)), max(g.along(sign, z - win_w / 2), g.along(sign, z + win_w / 2)), sy - win_h / 2, sy + win_h / 2) for z in side_z for sy in side_rows]
        g.side(m, sign, half, lambda mm, lo=lo, hi=hi, holes=holes: W(mm, lo, hi, .32, e, 0, .16, holes, wall_color))
        for z in side_z:
            for sy in side_rows:
                g.side(m, sign, half, lambda mm, z=z, sign=sign, sy=sy: g.window(mm, g.along(sign, z), sy, win_w, win_h, .135, 'WoodDark' if level < 3 else wood, 3 <= level < 5, level >= 8))   # side shutters only to level 4 (budget)
    if level < 3:   # narrow battens on sheds
        for sgn in (-1, 1):
            for j in range(10):
                zz = rear + .3 + (depth - .5) * (j + .5) / 10
                if all(abs(zz - z) > win_w / 2 + .12 for z in side_z): g.box(m, (sgn * (half + .085), 1.17, zz), (.025, 1.7, .05), wood, shade=1 + j % 3)
        for x in [(-half + .12) + i * (2 * half - .24) / 12 for i in range(13)]:
            if (door_x is None or abs(x - door_x) > .65) and all(abs(x - wx) > ww / 2 + .1 for wx, _, ww, _ in windows):
                g.box(m, (x, (e + .33) / 2, .018), (.035, e - .33, .028), wood, shade=1)
    for sign in (-1, 1):
        for z in (.035, rear + .12): g.box(m, (sign * (half - .05), (e + .32) / 2, z), (.19, e - .32, .18), wood)
        bays = len(side_z); posts = [-depth * i / bays for i in range(1, bays)] if bays >= 2 else [rear * .30, rear * .66]
        for z in posts: g.box(m, (sign * (half + .08), (e + .32) / 2, z), (.16, e - .32, .18), wood)
    for y in (.43, e - .10): g.box(m, (0, y, .035), (2 * half, .16, .18), wood)
    if level >= 3:
        for sgn in (-1, 1):
            x = sgn * (half - .55)
            g.beam(m, (x - sgn * .35, .56, .06), (x + sgn * .28, 1.03, .06), .13, wood)
    g.gable(m, half, e, peak, -.08, wall_color)
    g.gable(m, half, e, peak, rear + .14, wall_color)
    g.roof(m, w, depth + .18, e, peak, .30, roof_color, wood, True, 8, .55 if level < 5 else .75 if level < 9 else 1.0)   # larger tiles on big houses (budget)
    if fan and not gable_windows:
        g.beam(m, (0, e, .06), (0, peak - .12, .06), .13, wood)
        for sign in (-1, 1): g.beam(m, (sign * (half - .1), e + .02, .06), (0, peak - .12, .06), .11, wood)
    else:
        g.box(m, (0, e + .02, .06), (2 * half - .2, .14, .12), wood)   # collar along the gable foot
    for x, y, ww, hh in gable_windows: g.window(m, x, y, ww, hh, .02, wood, False, level >= 8)
    if band: g.box(m, (0, band, .06), (2 * half, .18, .16), wood)
    if skirt:
        g.box(m, (0, skirt / 2, .02), (2 * half + .12, skirt, .14), stone)
        for sign in (-1, 1): g.box(m, (sign * (half + .03), skirt / 2, rear / 2), (.14, skirt, depth), stone)
    for x, y, ww, hh in all_front:
        g.window(m, x, y, ww, hh, .055, 'WoodDark' if level < 3 else wood, level >= 3, level >= 8)
        if rear_windows:
            start = len(m.vertices); g.window(m, x, y, ww, hh, 0, wood, False, False)
            m.transform(lambda p: (p[0], p[1], rear + .04 - p[2]), start)
    if gable_window: g.window(m, 0, e + .45, .58, .63, .06, wood, False)
    for zc in [rear * .70, rear * .22][:chimneys]:
        cx = -(half - .65); cy = e + .35   # architectural flue, as in the current kit
        g.box(m, (cx, cy, zc), (.45, 1.4, .45), 'Brick' if level < 7 else stone)
        g.box(m, (cx, cy + .73, zc), (.58, .12, .58), stone)
        g.box(m, (cx, cy + .80, zc), (.34, .025, .34), 'WoodDark')
    if level >= 4:
        for sign in (-1, 1):
            for y in (.65, e - .35): g.box(m, (sign * (half - .07), y, .145), (.27, .095, .035), metal)
    return door_h


def door_and_step(m, level, wood, door_h, x=0.0):
    stone = 'Brick' if level < 3 else 'Stone' if level < 5 else 'Granite' if level < 7 else 'Basalt'
    g.door(m, x, .34, .07, 'WoodDark' if level < 3 else wood, level, door_h)
    g.box(m, (x, .10, .39), (1.45, .20, .78), stone)


def level1():
    # "As today": the approved level 1 is the current shed, rebuilt with the same kit and numbers.
    s = LEVELS[0]; m = k.Mesh('HouseL01Front', pal)
    half = s['Width'] / 2 - .25
    windows = [(half * .60, 1.20, .63, .64)]
    door_h = block(m, 1, s['Width'], s['Depth'] - .8, s['Eaves'], s['Height'], s['Wall'], s['Roof'], s['Wood'], 0.0, windows, 0, False)
    door_and_step(m, 1, s['Wood'], door_h)
    return m


def level2():
    """Extended shed: the gabled shed (4.05 m) to the left, a plank lean-to (1.55 m) on the right."""
    m = k.Mesh('HouseL02Front', pal); wood, wall, roof = 'Pine', 'PineLight', 'RoofClay'
    mw, mx, e, peak, depth = 4.05, -.775, 2.42, 3.4, 4.3 - .8
    rear = .18 - depth
    shifted(m, mx, lambda: block(m, 2, mw, depth, e, peak, wall, roof, wood, -mx, [(-.85, 1.38, .76, .85)], 0, False))
    # The door stays at the street centre line (x = 0), inside the main shed.
    door_and_step(m, 2, wood, 1.92)
    # Porch canopy over the door (as the current level 2).
    g.box(m, (0, 1.92 + .57, .45), (1.65, .12, 1.0), roof)
    for sign in (-1, 1):
        g.box(m, (sign * (1.65 / 2 - .10), (1.92 + .65) / 2, .78), (.11, 1.92 + .27, .11), wood)
        g.beam(m, (sign * (1.65 / 2 - .10), 1.92 + .25, .78), (sign * (1.65 / 2 - .42), 1.92 + .51, .78), .085, wood)
    # Lean-to: walls from the shed's side wall (x = 1.0) out to x = 2.62, roof falling outward under the main eaves.
    x0, x1, top_in, top_out = mx + mw / 2 - .25, 2.62, 2.30, 1.95
    g.box(m, ((x0 + x1) / 2, .12, rear / 2), (x1 - x0, .24, depth - .02), 'Brick')
    win = ((x0 + x1) / 2, 1.20, .62, .62)
    g.wall(m, x0, x1, .24, top_out, -.08, .14, [(win[0] - win[2] / 2, win[0] + win[2] / 2, win[1] - win[3] / 2, win[1] + win[3] / 2)], 'Pine')
    g.wall(m, x0, x1, .24, top_out, rear + .14, .14, [], 'Pine')
    g.side(m, 1, x1, lambda mm: g.wall(mm, g.along(1, 0.0), g.along(1, rear + .06), .24, top_out, 0, .14, [], 'Pine'))
    # Sloped infill above the low walls (front and back), so the walls meet the roof.
    for z in (-.08, rear + .14):
        ids = [m.v(p) for p in [(x0, top_out, z + .07), (x1, top_out, z + .07), (x0, top_in, z + .07), (x0, top_out, z - .07), (x1, top_out, z - .07), (x0, top_in, z - .07)]]
        inside = ((x0 * 2 + x1) / 3, (top_out * 2 + top_in) / 3, z)
        m.tri(ids[0], ids[1], ids[2], 'Pine', inside, shade=2); m.tri(ids[3], ids[4], ids[5], 'Pine', inside, shade=2)
    # Mono-pitch roof: from under the main eaves (x0 - .1) down and out past the outer wall.
    rx0, rx1, ry0, ry1 = x0 - .12, x1 + .22, top_in + .08, top_out - .05
    front, back_z = .22, rear - .12
    k.box(m, ((rx0 + rx1) / 2, (ry0 + ry1) / 2, (front + back_z) / 2), (k.length((rx1 - rx0, ry1 - ry0, 0)), .09, front - back_z), roof,
          axes=(k.unit((rx1 - rx0, ry1 - ry0, 0)), k.unit((-(ry1 - ry0), rx1 - rx0, 0)), (0, 0, 1)), shade=1)
    for z in (front + .02, back_z - .02): g.beam(m, (rx0, ry0 + .05, z), (rx1, ry1 + .05, z), .09, wood)
    for x in (x0 + .05, x1 - .05):
        for z in (.03, rear + .12): g.box(m, (x, (top_out + .24) / 2, z), (.12, top_out - .24, .14), wood)
    g.window(m, win[0], win[1], win[2], win[3], .05, 'WoodDark', False)
    for j in range(5):   # battens on the lean-to front
        x = x0 + .1 + (x1 - x0 - .2) * (j + .5) / 5
        if abs(x - win[0]) > win[2] / 2 + .1: g.box(m, (x, (top_out + .24) / 2, .005), (.035, top_out - .3, .028), wood, shade=1)
    return m


def gabled_porch(m, wood, roof, width=2.25, front=1.10, eave=2.70, peak=3.12, posts=2, floor='Stone', min_cols=8):
    g.box(m, (0, .10, front / 2), (width, .20, front), floor)
    start = len(m.vertices); g.roof(m, width, front + .10, eave, peak, front + .08, roof, wood, True, min_cols)
    g.gable(m, width / 2 - .12, eave, peak - .02, front - .05, 'Plaster', .08)
    g.beam(m, (0, eave, front - .01), (0, peak - .10, front - .01), .09, wood)
    for i in range(posts):
        x = (-width / 2 + .16) + (width - .32) * i / (posts - 1); sign = 1 if x > 0 else -1
        g.box(m, (x, (eave + .2) / 2, front - .08), (.12, eave - .2, .12), wood)
        if abs(x) > .2: g.beam(m, (x, eave - .45, front - .08), (x - sign * .38, eave - .02, front - .08), .07, wood)


def level3(m=None, name='HouseL03Front', w=7.0, peak=4.5, e=3.10, depth=4.8 - .8):
    m = m or k.Mesh(name, pal); wood, wall, roof = 'Birch', 'Plaster', 'RoofEarth'
    windows = [(x, 1.38, .88, 1.02) for x in (-w * .28, w * .28)]
    door_h = block(m, 3, w, depth, e, peak, wall, roof, wood, 0.0, windows, 1, True)
    door_and_step(m, 3, wood, door_h)
    gabled_porch(m, wood, roof)
    return m


def level4():
    """Back room: the level-3 home widened to 7.6 m, a raised rear gable (the back room) rising behind the main
    roof (its gable and window rise above the main ridge line)."""
    m = k.Mesh('HouseL04Front', pal); wood, wall, roof = 'Birch', 'Plaster', 'RoofEarth'
    w, e, mainpeak, depth = 7.6, 3.12, 4.55, 5.8 - .8
    rear = .18 - depth
    windows = [(x, 1.38, .88, 1.02) for x in (-w * .28, w * .28)]
    door_h = block(m, 4, w, depth, e, mainpeak, wall, roof, wood, 0.0, windows, 1, True)
    door_and_step(m, 4, wood, door_h)
    gabled_porch(m, wood, roof)
    # Raised back room: a gabled block 4.2 m wide at x = 1.2, from 2.1 m behind the front to the back wall.
    bx, bw, be, bpeak, bfront = 1.2, 4.2, 4.30, 5.2, -2.1
    bhalf = bw / 2 - .2; bdepth = bfront - (rear + .14)

    def back_room():
        g.wall(m, -bhalf, bhalf, 2.7, be, bfront, .14, [], wall)
        g.wall(m, -bhalf, bhalf, 2.7, be, rear + .10, .14, [], wall)
        for sign in (-1, 1):
            lo, hi = sorted((g.along(sign, bfront), g.along(sign, rear + .14)))
            g.side(m, sign, bhalf, lambda mm, lo=lo, hi=hi: g.wall(mm, lo, hi, 2.7, be, 0, .14, [], wall))
            for z in (bfront - .04, rear + .16): g.box(m, (sign * (bhalf - .05), (be + 2.7) / 2, z), (.16, be - 2.7, .16), wood)
        g.gable(m, bhalf, be, bpeak, bfront, wall)
        g.gable(m, bhalf, be, bpeak, rear + .10, wall)
        g.roof(m, bw, bdepth + .30, be, bpeak, bfront + .18, roof, wood)
        g.beam(m, (0, be, bfront + .08), (0, bpeak - .12, bfront + .08), .11, wood)
        g.box(m, (0, be - .06, bfront + .06), (2 * bhalf, .14, .14), wood)
        g.window(m, 0, be + .32, .55, .48, bfront + .09, wood, False)
        start = len(m.vertices); g.window(m, 0, be + .32, .55, .48, 0, wood, False)
        m.transform(lambda p: (p[0], p[1], rear + .06 - p[2]), start)
    shifted(m, bx, back_room)
    # No street-facing dormer: on a front-gable roof it clashed with the gable timbers (review, November 13); dormers
    # arrive with the level-5 half-storey. The raised back room is level 4's new silhouette.
    return m


def gabled_bay(m, x, w, bottom, e, peak, front, back, wall, roof, wood, window=None, brackets=False):
    """A small gabled block whose front face stands at z = front (a projecting bay or an upper cross gable)."""
    half = w / 2 - .15
    def build():
        hole = [] if not window else [(window[0] - window[2] / 2, window[0] + window[2] / 2, window[1] - window[3] / 2, window[1] + window[3] / 2)]
        g.wall(m, -half, half, bottom, e, front - .07, .14, hole, wall)
        for sign in (-1, 1):
            lo, hi = sorted((g.along(sign, front - .14), g.along(sign, back)))
            g.side(m, sign, half, lambda mm, lo=lo, hi=hi: g.wall(mm, lo, hi, bottom, e, 0, .14, [], wall))
            g.box(m, (sign * (half - .04), (bottom + e) / 2, front - .02), (.15, e - bottom, .15), wood)
        g.box(m, (0, bottom + .07, front - .02), (2 * half, .14, .15), wood)
        g.box(m, (0, e - .06, front - .02), (2 * half, .13, .15), wood)
        g.gable(m, half, e, peak, front - .07, wall)
        g.roof(m, w, front - back + .25, e, peak, front + .25, roof, wood, True, 3, .75)
        g.beam(m, (0, e, front + .02), (0, peak - .12, front + .02), .10, wood)
        if window: g.window(m, window[0], window[1], window[2], window[3], front + .01, wood, True, False)
        if brackets:
            for xx in (-half + .2, 0, half - .2): g.beam(m, (xx, bottom - .4, .05), (xx, bottom - .02, front - .05), .09, wood)
    shifted(m, x, build)


def level5():
    """Long hall: 8.2 m, a half-storey (knee walls to 3.8 m with windows under the eaves line and in the gable)
    and a projecting bay with its own gable at the screen-right front corner."""
    m = k.Mesh('HouseL05Front', pal); wood, wall, roof = 'Mesquite', 'SandPlaster', 'RoofEarth'
    w, e, peak, depth = 8.2, 3.8, 5.8, 6.8 - .8
    windows = [(w * .28, 1.38, .88, 1.02)]                        # the screen-right window sits in the bay
    upper = [(x, 3.05, .70, .62) for x in (-1.25, 1.25)]
    door_h = block(m, 5, w, depth, e, peak, wall, roof, wood, 0.0, windows, 1, False, False, upper=upper, gable_windows=[(0, 4.5, .62, .70)])
    door_and_step(m, 5, wood, door_h)
    gabled_porch(m, wood, roof, min_cols=3)
    gabled_bay(m, -2.75, 2.3, .30, 2.85, 3.95, 1.05, -.05, wall, roof, wood, window=(0, 1.62, .95, 1.05))
    g.box(m, (-2.75, .17, .50), (2.25, .34, 1.15), 'Granite')
    return m


def level6():
    """Extended hall: 8.8 m and slate, a partial second storey (an upper cross gable over the screen-left half,
    jettied forward on brackets) and a second chimney."""
    m = k.Mesh('HouseL06Front', pal); wood, wall, roof = 'Mesquite', 'SandPlaster', 'Slate'
    w, e, peak, depth = 8.8, 3.8, 6.4, 7.8 - .8
    windows = [(x, 1.38, .88, 1.02) for x in (-w * .28, w * .28)]
    door_h = block(m, 6, w, depth, e, peak, wall, roof, wood, 0.0, windows, 2, False, False, upper=[(-2.45, 3.05, .70, .62)],
                   gable_windows=[(-1.2, 4.6, .55, .60)])
    door_and_step(m, 6, wood, door_h)
    gabled_porch(m, wood, roof, eave=2.62, peak=3.0, min_cols=3)
    gabled_bay(m, 2.0, 4.4, 3.15, 5.15, 6.6, .45, -3.2, wall, roof, wood, window=(0, 4.15, 1.25, 1.1), brackets=True)
    return m


def level7():
    """Frontier manor: 9.6 m, full two storeys (eaves 5.5 m) and a porch across the front."""
    m = k.Mesh('HouseL07Front', pal); wood, wall, roof = 'Oak', 'Plaster', 'Slate'
    w, e, peak, depth = 9.6, 5.5, 7.6, 8.8 - .8
    windows = [(x, 1.38, .88, 1.02) for x in (-w * .28, w * .28)]
    upper = [(x, 4.3, 1.0, 1.1) for x in (-w * .28, 0, w * .28)]
    door_h = block(m, 7, w, depth, e, peak, wall, roof, wood, 0.0, windows, 2, True, False, upper=upper, side_rows=(1.38,), band=3.08)
    door_and_step(m, 7, wood, door_h)
    gabled_porch(m, wood, roof, width=w - .9, eave=2.72, peak=3.0, posts=4, min_cols=3)
    return m


def level8():
    """Finished homestead: 10.2 m, a stone base, and a gabled side wing (its own gable, projecting slightly) at the
    screen-left; the porch covers the main front beside it."""
    m = k.Mesh('HouseL08Front', pal); wood, wall, roof = 'Oak', 'FinePlaster', 'Slate'
    w, e, peak, depth = 10.2, 5.8, 8.2, 9.8 - .8
    windows = [(-w * .28, 1.38, .88, 1.02)]
    upper = [(x, 4.3, 1.0, 1.1) for x in (-w * .28, 0)]
    door_h = block(m, 8, w, depth, e, peak, wall, roof, wood, 0.0, windows, 2, True, False, upper=upper, side_rows=(1.38,), band=3.08, skirt=.70)
    door_and_step(m, 8, wood, door_h)
    shifted(m, -.95, lambda: gabled_porch(m, wood, roof, width=4.5, eave=2.72, peak=3.0, posts=3, floor='Marble', min_cols=3))
    gabled_bay(m, 3.35, 3.5, .30, 4.55, 6.5, .30, -4.0, wall, roof, wood, window=(0, 1.52, 1.06, 1.2))
    shifted(m, 3.35, lambda: g.window(m, 0, 3.55, .92, .9, .31, wood, True, True))
    shifted(m, 3.35, lambda: g.box(m, (0, .35, .30), (3.25, .70, .14), 'Basalt'))
    return m


# ---------------------------------------------------------------- batch 3: levels 9-12 (tier 5 maple, tier 6 cedar)

def sloped(m, x0, x1, z0, y0, z1, y1, color, thick=.09):
    """A flat slab across x0..x1 running from (z0, y0) to (z1, y1): a mono-pitch roof falling toward the street."""
    s = k.unit((0, y1 - y0, z1 - z0)); n = (0, s[2], -s[1])
    k.box(m, ((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2), (x1 - x0, thick, k.length((0, y1 - y0, z1 - z0))), color, axes=((1, 0, 0), n, s), shade=1)


def shed_porch(m, x0, x1, wood, roof, floor='Stone', front=1.32, h_in=3.0, h_out=2.62, posts=5):
    """A porch across the front under a mono-pitch roof that starts at the floor line of the upper storey."""
    g.box(m, ((x0 + x1) / 2, .10, front / 2), (x1 - x0, .20, front), floor)
    sloped(m, x0 - .08, x1 + .08, -.02, h_in, front + .12, h_out - .02, roof)
    g.box(m, ((x0 + x1) / 2, h_out - .12, front - .08), (x1 - x0, .16, .14), wood)
    for i in range(posts):
        x = x0 + .1 + (x1 - x0 - .2) * i / (posts - 1)
        if abs(x) < .8: continue                    # keep the door bay open
        g.box(m, (x, (h_out + .2) / 2, front - .08), (.13, h_out - .2, .13), wood)
        for sgn in (-1, 1):
            if x0 + .3 < x + sgn * .4 < x1 - .3: g.beam(m, (x, h_out - .55, front - .08), (x + sgn * .4, h_out - .2, front - .08), .07, wood)


def rail(m, x0, x1, z, y, wood, spacing=.28):
    for yy in (y + .95, y + .12): g.box(m, ((x0 + x1) / 2, yy, z), (x1 - x0, .07, .07), wood)
    n = max(2, round((x1 - x0) / spacing))
    for i in range(n + 1): g.box(m, (x0 + (x1 - x0) * i / n, y + .53, z), (.05, .84, .05), wood)


def balcony(m, x, w, y, depth, wood):
    """A small upper-floor balcony on brackets: deck, front rail, short side rails."""
    g.box(m, (x, y, depth / 2), (w, .14, depth), wood)
    rail(m, x - w / 2 + .05, x + w / 2 - .05, depth - .04, y + .07, wood)
    for sgn in (-1, 1):
        xx = x + sgn * (w / 2 - .05)
        g.box(m, (xx, y + 1.02, depth / 2), (.07, .07, depth), wood)
        g.box(m, (xx, y + .55, depth * .5), (.05, .84, .05), wood)
        g.beam(m, (xx - sgn * .15, y - .65, .06), (xx - sgn * .15, y - .07, depth - .15), .085, wood)


def gable_hood(m, x, y, w, wood, roof):
    """A small pent hood over a gable window, so the upper gable reads as an attic with dormer-like openings."""
    sloped(m, x - w / 2, x + w / 2, .02, y + .08, .42, y - .12, roof, .07)
    for sgn in (-1, 1): g.beam(m, (x + sgn * (w / 2 - .06), y - .32, .06), (x + sgn * (w / 2 - .06), y - .06, .36), .05, wood)


def tower(m, x, w, split, top, peak, lower, upper, wood, roof):
    """A square lookout tower at the front corner: stone shaft, timber top storey with windows on three faces,
    a pyramid roof and a finial."""
    zc = -w / 2 + .12
    g.box(m, (x, split / 2, zc), (w, split, w), lower)
    g.box(m, (x, (split + top) / 2, zc), (w - .06, top - split, w - .06), upper)
    for y in (split, top - .1): g.box(m, (x, y, zc), (w + .12, .16, w + .12), wood)
    for sx in (-1, 1):
        for sz in (-1, 1): g.box(m, (x + sx * (w / 2 - .02), (split + top) / 2, zc + sz * (w / 2 - .02)), (.16, top - split, .16), wood)
    zf = .12 + .005
    for y, ww, hh in ((1.5, .55, .9), (4.6, .55, .9), (split + (top - split) * .55, .78, 1.15)):
        g.window(m, x, y, ww, hh, zf, wood, False, True)
    start = len(m.vertices)   # the top window also on the outer side face (facing +x)
    g.window(m, 0, split + (top - split) * .55, .78, 1.15, w / 2 + .005, wood, False, True)
    m.transform(lambda p: (x + p[2], p[1], zc - p[0]), start)
    o = w / 2 + .22
    base = [m.v((x + sx * o, top, zc + sz * o)) for sx, sz in ((-1, 1), (1, 1), (1, -1), (-1, -1))]
    apex = m.v((x, peak, zc)); inside = (x, top + .05, zc)
    for i in range(4): m.tri(base[i], base[(i + 1) % 4], apex, roof, inside, shade=1 + i % 3)
    m.quad(base[0], base[1], base[2], base[3], roof, (x, top + .5, zc), shade=2)
    g.box(m, (x, peak + .18, zc), (.10, .45, .10), 'Iron')
    g.box(m, (x, peak - .02, zc), (.22, .12, .22), wood)


def lodge(level, name, w, e, peak, depth, upper_wall, ground, roof, wood, bx=0.0, gable_windows=(), hoods=(), uppers=None):
    """The tier 5-6 body: stone ground floor to 3 m, timber upper storey, gable windows (no gable fan), two chimneys.
    bx shifts the body sideways (the door stays on the street centre line)."""
    m = k.Mesh(name, pal_hi)
    windows = [(x, 1.38, .88, 1.02) for x in (-w * .28, w * .28)]
    upper = [(x, 4.5, 1.0, 1.15) for x in (uppers if uppers is not None else (-w * .28, -bx))]   # default: screen-right and above the door
    def build():
        block(m, level, w, depth, e, peak, upper_wall, roof, wood, -bx, windows, 2, False, False, upper=upper,
              side_rows=(1.38, 4.5), gable_windows=gable_windows, band=3.0, lower=(3.0, ground))
        for x, y, ww in hoods: gable_hood(m, x, y + .45, ww + .3, wood, roof)
    shifted(m, bx, build)
    door_and_step(m, level, wood, 1.92)
    return m


def level9():
    """Maple lodge: 10.6 m, a stone ground floor and a maple upper storey, a long porch roof across the front and an
    upper cross gable at the screen-left."""
    w = 10.6
    m = lodge(9, 'HouseL09Front', w, 6.0, 8.6, 10.0, 'Maple', 'Stone', 'Slate', 'WoodDark', gable_windows=[(0, 7.0, .72, .78)])
    shed_porch(m, -w / 2 + .2, w / 2 - .2, 'WoodDark', 'Slate')
    gabled_bay(m, 2.85, 3.4, 3.0, 6.0, 7.75, .14, -2.6, 'Maple', 'Slate', 'WoodDark', window=(0, 4.5, 1.0, 1.15))
    return m


def level10():
    """Maple lodge with veranda: ridge 9.2 m; the veranda wraps the screen-left corner and returns along the side."""
    w, bx = 9.9, -.45
    m = lodge(10, 'HouseL10Front', w, 6.0, 9.2, 11.0, 'Maple', 'Stone', 'Slate', 'WoodDark', bx=bx, gable_windows=[(0, 7.25, .78, .84)])
    x0, x1 = bx - w / 2 + .1, 5.30
    shed_porch(m, x0, x1, 'WoodDark', 'Slate', posts=6)
    # Side return: floor, roof falling outward (+x) and posts along the side, from the front corner to 2.5 m back.
    side_x0, back = bx + w / 2 - .12, -2.55
    g.box(m, ((side_x0 + x1) / 2, .10, back / 2), (x1 - side_x0, .20, -back), 'Stone')
    s = k.unit((x1 + .12 - side_x0, 2.60 - 3.0, 0)); n = (-s[1], s[0], 0)
    k.box(m, ((side_x0 + x1 + .12) / 2, 2.80, (back - .1) / 2), (k.length((x1 + .12 - side_x0, -.4, 0)), .09, -back + .1), 'Slate', axes=(s, n, (0, 0, 1)), shade=1)
    for z in (-1.15, back + .08):
        g.box(m, (x1 - .08, 1.31, z), (.13, 2.42, .13), 'WoodDark')
    g.box(m, (x1 - .08, 2.50, back / 2), (.14, .16, -back), 'WoodDark')
    rail(m, x0 + .2, -.9, 1.24, .2, 'WoodDark', .32); rail(m, .9, x1 - .2, 1.24, .2, 'WoodDark', .32)
    shifted(m, 0, lambda: g.side(m, 1, x1 - .08, lambda mm: rail(mm, g.along(1, -.15), g.along(1, back + .15), 0, .2, 'WoodDark', .32)))
    gabled_bay(m, bx + 2.65, 3.4, 3.0, 6.0, 7.75, .14, -2.6, 'Maple', 'Slate', 'WoodDark', window=(0, 4.5, 1.0, 1.15))
    return m


def level11():
    """Cedar chalet: 10.8 m, steep roof (ridge 10 m), granite ground floor and cedar upper storey, two balconies on the
    upper floor and hooded gable windows."""
    w = 10.8
    gw = [(-2.1, 7.1, .62, .70), (0, 8.55, .62, .70)]   # the screen-left side of the gable is the cross gable's
    m = lodge(11, 'HouseL11Front', w, 6.2, 10.0, 12.0, 'Cedar', 'Granite', 'Slate', 'WoodDark', gable_windows=gw, hoods=[(x, y + .35, ww) for x, y, ww, _ in gw])
    shed_porch(m, -w / 2 + .2, w / 2 - .2, 'WoodDark', 'Slate')
    gabled_bay(m, 2.85, 3.4, 3.0, 6.2, 8.05, .14, -2.6, 'Cedar', 'Slate', 'WoodDark', window=(0, 4.5, 1.0, 1.15))
    balcony(m, 2.85, 2.3, 3.32, 1.05, 'WoodDark'); balcony(m, -w * .28, 2.1, 3.32, 1.05, 'WoodDark')
    return m


def level12():
    """Cedar chalet with lookout tower: the chalet body (9 m) moves right and a 12 m tower stands at the screen-left
    front corner."""
    w, bx = 9.0, -.95
    gw = [(-1.75, 7.1, .62, .70), (0, 8.55, .62, .70)]
    m = lodge(12, 'HouseL12Front', w, 6.2, 10.0, 13.0, 'Cedar', 'Granite', 'Slate', 'WoodDark', bx=bx, gable_windows=gw, hoods=[(x, y + .35, ww) for x, y, ww, _ in gw],
              uppers=(-w * .28,))
    shed_porch(m, bx - w / 2 + .2, bx + w / 2 - .2, 'WoodDark', 'Slate', posts=4)
    gabled_bay(m, bx + 2.55, 3.0, 3.0, 6.2, 7.85, .14, -2.6, 'Cedar', 'Slate', 'WoodDark', window=(0, 4.5, 1.0, 1.15))
    balcony(m, bx + 2.55, 2.1, 3.32, 1.05, 'WoodDark'); balcony(m, bx - w * .28, 2.0, 3.32, 1.05, 'WoodDark')
    tower(m, 4.38, 1.7, 6.0, 10.0, 11.6, 'Granite', 'Cedar', 'WoodDark', 'Slate')
    return m


def measure(m):
    lo = [min(v[i] for v in m.vertices) for i in range(3)]; hi = [max(v[i] for v in m.vertices) for i in range(3)]
    front = [v[0] for v in m.vertices if v[2] >= -WING_FROM]
    return dict(Width=round(max(front) - min(front), 4), WingWidth=round(hi[0] - lo[0], 4), Height=round(hi[1], 4), Depth=round(-lo[2], 4),
                FrontReach=round(hi[2], 4), Triangles=m.triangles, BoundsMin=lo, BoundsMax=hi)


BUILDERS = {1: level1, 2: level2, 3: level3, 4: level4, 5: level5, 6: level6, 7: level7, 8: level8, 9: level9, 10: level10, 11: level11, 12: level12}
NEW_ELEMENT = {1: 'Original shed scale', 2: 'Side lean-to', 3: 'Gabled porch roof, chimney', 4: 'Raised rear gable (back room) rising behind the main roof',
               5: 'Half-storey (gable and knee-wall windows), projecting side bay', 6: 'Partial second storey (jettied cross gable), second chimney',
               7: 'Full two storeys, porch across the front', 8: 'Gabled side wing, stone base',
               9: 'Stone ground floor, maple upper storey, long porch roof, upper cross gable',
               10: 'Veranda wrapping the screen-left corner with a side return; 9.2 m ridge',
               11: 'Steep cedar roof (10 m), two upper balconies, hooded gable windows',
               12: 'Lookout tower (12 m) at the screen-left front corner'}
# Batch 3 adds manifest rows: (name, tier, interior grid depth, wood, wall, roof, chimneys). Interiors for 9-16 come later.
NEW_ROWS = {9: ('Maple lodge', 5, 19, 'Maple', 'Stone', 'Slate', 2), 10: ('Maple lodge with veranda', 5, 21, 'Maple', 'Stone', 'Slate', 2),
            11: ('Cedar chalet', 6, 23, 'Cedar', 'Granite', 'Slate', 2), 12: ('Cedar chalet with lookout tower', 6, 25, 'Cedar', 'Granite', 'Slate', 2)}


def final_front(level):
    m = BUILDERS[level]()
    r = measure(m)
    w = r['Width']
    assert w <= 11.0001 and r['WingWidth'] <= 14.8001 and r['Height'] <= 13 and r['Depth'] <= 19 and r['FrontReach'] <= 1.5001, (level, r)
    assert m.triangles <= (8000 if level <= 8 else 12000), (level, m.triangles)
    row = dict(Width=w, Depth=r['Depth'], Height=r['Height'], WingWidth=r['WingWidth'], Triangles=m.triangles, FrontReach=r['FrontReach'],
               PetHomeAnchor=[round(w / 2 + .95, 4), 0, .25], CollectionAnchor=[round(-(w / 2 + .95), 4), 0, .25],
               PlotWidth=PLOT_WIDTH, PlotDepth=PLOT_DEPTH, Stage='Final exterior (batch 1)' if level <= 8 else 'Final exterior (batch 3)', NewElement=NEW_ELEMENT[level],
               HomeCameraAnchor=[0, max(2.4, r['Height'] * .45), 13])
    if level in SPEC and 'Eaves' in SPEC[level]: row['Eaves'] = SPEC[level]['Eaves']
    return m, row, r


def save_hi(m):
    """OBJ for a level 9-16 front: same layout as Mesh.save, but UVs through palette_uv (future woods in the upper band)."""
    lines = ['# ' + HEADER, '# Faceted style study: flat normals (s off), palette-strip UVs (two bands).', 'o ' + m.name]
    lines += ['v %.6f %.6f %.6f' % p for p in m.vertices]
    lines += ['vt %.6f %.6f' % palette_uv(i, BASE_SLOTS) for i in range(pal_hi.slots)]
    lines += ['s off']
    lines += ['f %d/%d %d/%d %d/%d' % (a + 1, s + 1, b + 1, s + 1, c + 1, s + 1) for a, b, c, s in m.faces]
    (OUT / (m.name + '.obj')).write_text('\n'.join(lines) + '\n', encoding='utf-8')
    return {'name': m.name, 'vertices': len(m.vertices), 'triangles': len(m.faces)}


def main():
    manifest_path = OUT / 'HouseManifest.json'
    manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
    houses = {h['Level']: h for h in manifest['Houses']}
    for level in FINAL_LEVELS:
        m, row, r = final_front(level)
        info = m.save(OUT, HEADER) if level <= 8 else save_hi(m); info['BoundsMin'] = r['BoundsMin']; info['BoundsMax'] = r['BoundsMax']
        if level not in houses:
            name, tier, rows, wood, wall, roof, chimneys = NEW_ROWS[level]
            base = json.loads(json.dumps(houses[8]))
            base.update(Level=level, Name=name, Tier=tier, GridWidth=12, GridDepth=rows, Wood=wood, Wall=wall, Roof=roof, Chimneys=chimneys,
                        Front=m.name, Interior='', GridGuides='', FurnitureStudy='', FurnitureSamples=[], PetsUnlocked=True, WallHeight=3.0)
            houses[level] = base; manifest['Houses'].append(base)
            manifest.setdefault('Models', []).append(dict(name=m.name))
        houses[level].update(row)
        for i, mm in enumerate(manifest.get('Models', [])):
            if mm.get('name') == m.name: manifest['Models'][i] = info
        print(m.name, m.triangles, 'triangles', {k_: row[k_] for k_ in ('Width', 'Height', 'Depth', 'WingWidth')})
    manifest_path.write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')


if __name__ == '__main__': main()
