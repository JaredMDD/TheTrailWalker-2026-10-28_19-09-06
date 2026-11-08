"""The Trail Walker app icon (original artwork, October 31, 2026). Requires Pillow.

Flat low-poly scene in the game's palette: banded sky, distant hills, pine groves, a meadow and a dirt trail winding
to the horizon, with a small walker carrying a backpack seen from behind. Outputs (1024 px) into
Assets/_Game/Art/AppIcon:
  AppIconBackground.png  adaptive-icon background layer (full-bleed scene, no walker)
  AppIconForeground.png  adaptive-icon foreground layer (walker only, transparent, inside the central safe zone)
  AppIcon.png            legacy/round icon (scene and walker together, full bleed; launchers apply their own mask)
"""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Art/AppIcon'
SIZE, SS = 1024, 3            # final size and supersampling factor for smooth polygon edges
W = SIZE * SS

SKY = [(124, 178, 196), (110, 165, 186), (98, 152, 176)]
HILL_FAR, HILL_NEAR = (112, 146, 104), (96, 132, 82)
MEADOW = [(122, 140, 72), (108, 129, 63), (93, 116, 58)]
DRY = (172, 165, 99)
TRAIL, TRAIL_EDGE = (133, 99, 61), (155, 123, 79)
PINE_DARK, PINE, PINE_LIGHT, TRUNK = (39, 69, 49), (54, 90, 58), (79, 115, 65), (113, 75, 44)
SKIN, SHIRT, SHIRT_SHADE = (196, 140, 98), (178, 165, 111), (150, 138, 90)
TROUSER, BOOT, PACK, PACK_LEATHER, HAIR = (128, 119, 83), (72, 54, 35), (163, 139, 91), (133, 102, 66), (60, 42, 28)

def s(v):  # design units are 0..1024
    return v * SS

def poly(d, pts, color):
    d.polygon([(s(x), s(y)) for x, y in pts], fill=color)

def pine(d, x, base, h, w):
    d.rectangle([s(x - w * .07), s(base - h * .14), s(x + w * .07), s(base)], fill=TRUNK)
    for i, (frac, col) in enumerate([(1.0, PINE_DARK), (.78, PINE), (.56, PINE_LIGHT)]):
        top = base - h * (0.28 + i * 0.24) - h * .3
        bottom = base - h * (0.1 + i * 0.22)
        half = w * frac / 2
        poly(d, [(x, top), (x + half, bottom), (x, bottom - h * .04)], col)
        poly(d, [(x, top), (x - half, bottom), (x, bottom - h * .04)], tuple(max(0, c - 14) for c in col))

def scene():
    img = Image.new('RGB', (W, W), SKY[0]); d = ImageDraw.Draw(img)
    for i, col in enumerate(SKY):  # flat sky bands
        d.rectangle([0, s(i * 150), W, s((i + 1) * 150 + 10)], fill=col)
    d.rectangle([0, s(450), W, W], fill=SKY[-1])
    poly(d, [(0, 470), (180, 395), (330, 440), (520, 380), (700, 430), (860, 390), (1024, 445), (1024, 560), (0, 560)], HILL_FAR)
    poly(d, [(0, 520), (240, 470), (430, 505), (640, 460), (880, 500), (1024, 480), (1024, 600), (0, 600)], HILL_NEAR)
    # Meadow facets.
    poly(d, [(0, 540), (1024, 520), (1024, 1024), (0, 1024)], MEADOW[1])
    poly(d, [(0, 600), (380, 560), (300, 760), (0, 820)], MEADOW[0])
    poly(d, [(1024, 580), (700, 600), (820, 780), (1024, 760)], MEADOW[2])
    poly(d, [(0, 860), (260, 820), (180, 1024), (0, 1024)], MEADOW[2])
    poly(d, [(1024, 840), (800, 880), (900, 1024), (1024, 1024)], MEADOW[0])
    poly(d, [(560, 640), (700, 620), (640, 700)], DRY)
    poly(d, [(150, 700), (260, 680), (210, 740)], DRY)
    # Pine groves on both sides of the horizon.
    for x, base, h, w in [(150, 545, 170, 95), (230, 560, 210, 115), (88, 575, 150, 85), (300, 548, 130, 75),
                          (780, 540, 190, 105), (860, 555, 230, 125), (950, 570, 170, 95), (705, 545, 120, 70)]:
        pine(d, x, base, h, w)
    # Dirt trail winding from the bottom edge to the horizon, with a lighter shoulder.
    left = [(170, 1024), (330, 900), (420, 790), (390, 705), (455, 630), (500, 572), (510, 545)]
    right = [(860, 1024), (690, 900), (600, 790), (610, 705), (565, 630), (530, 572), (518, 545)]
    shoulder_l = [(x - 34 * (y - 540) / 480, y) for x, y in left]; shoulder_r = [(x + 34 * (y - 540) / 480, y) for x, y in right]
    poly(d, shoulder_l + list(reversed(shoulder_r)), TRAIL_EDGE)
    poly(d, left + list(reversed(right)), TRAIL)
    return img

def walker(d, cx, base, k=1.0):
    """Walker seen from behind, mid-stride, centred on cx with feet at base. k scales the figure."""
    u = lambda v: v * k
    # Legs and boots (stride: left leg forward/up, right leg back).
    poly(d, [(cx - u(46), base - u(200)), (cx - u(8), base - u(200)), (cx - u(16), base - u(40)), (cx - u(52), base - u(52))], TROUSER)
    poly(d, [(cx + u(8), base - u(200)), (cx + u(46), base - u(200)), (cx + u(50), base - u(14)), (cx + u(14), base - u(10))], TROUSER)
    poly(d, [(cx - u(58), base - u(58)), (cx - u(12), base - u(44)), (cx - u(14), base - u(18)), (cx - u(62), base - u(30))], BOOT)
    poly(d, [(cx + u(10), base - u(16)), (cx + u(54), base - u(20)), (cx + u(58), base + u(6)), (cx + u(8), base + u(8))], BOOT)
    # Torso and arms.
    poly(d, [(cx - u(70), base - u(400)), (cx + u(70), base - u(400)), (cx + u(60), base - u(190)), (cx - u(60), base - u(190))], SHIRT)
    poly(d, [(cx - u(70), base - u(395)), (cx - u(98), base - u(250)), (cx - u(78), base - u(240)), (cx - u(58), base - u(330))], SHIRT_SHADE)
    poly(d, [(cx + u(70), base - u(395)), (cx + u(100), base - u(262)), (cx + u(80), base - u(250)), (cx + u(58), base - u(330))], SHIRT_SHADE)
    poly(d, [(cx - u(98), base - u(250)), (cx - u(78), base - u(240)), (cx - u(84), base - u(208)), (cx - u(104), base - u(214))], SKIN)
    poly(d, [(cx + u(100), base - u(262)), (cx + u(80), base - u(250)), (cx + u(86), base - u(220)), (cx + u(106), base - u(228))], SKIN)
    # Backpack with leather flap and straps.
    poly(d, [(cx - u(62), base - u(390)), (cx + u(62), base - u(390)), (cx + u(56), base - u(222)), (cx - u(56), base - u(222))], PACK)
    poly(d, [(cx - u(62), base - u(390)), (cx + u(62), base - u(390)), (cx + u(58), base - u(320)), (cx - u(58), base - u(320))], PACK_LEATHER)
    d.rectangle([s(cx - u(44)), s(base - u(400)), s(cx - u(30)), s(base - u(320))], fill=PACK_LEATHER)
    d.rectangle([s(cx + u(30)), s(base - u(400)), s(cx + u(44)), s(base - u(320))], fill=PACK_LEATHER)
    poly(d, [(cx - u(70), base - u(236)), (cx + u(70), base - u(236)), (cx + u(70), base - u(206)), (cx - u(70), base - u(206))], (154, 150, 102))  # bedroll
    # Head: neck, hair from behind.
    d.rectangle([s(cx - u(18)), s(base - u(430)), s(cx + u(18)), s(base - u(395))], fill=SKIN)
    d.ellipse([s(cx - u(46)), s(base - u(510)), s(cx + u(46)), s(base - u(418))], fill=HAIR)
    d.ellipse([s(cx - u(52)), s(base - u(470)), s(cx - u(40)), s(base - u(444))], fill=SKIN)
    d.ellipse([s(cx + u(40)), s(base - u(470)), s(cx + u(52)), s(base - u(444))], fill=SKIN)

def finish(img):
    return img.resize((SIZE, SIZE), Image.LANCZOS)

if __name__ == '__main__':
    OUT.mkdir(parents=True, exist_ok=True)
    background = scene()
    finish(background).save(OUT / 'AppIconBackground.png')
    # Adaptive foreground: walker inside the central safe zone (launchers show roughly the middle 66%).
    fg = Image.new('RGBA', (W, W), (0, 0, 0, 0)); walker(ImageDraw.Draw(fg), 512, 770, 0.8)
    finish(fg).save(OUT / 'AppIconForeground.png')
    full = background.copy(); walker(ImageDraw.Draw(full), 500, 930, 0.82)
    finish(full).save(OUT / 'AppIcon.png')
    print('App icon written to', OUT)
