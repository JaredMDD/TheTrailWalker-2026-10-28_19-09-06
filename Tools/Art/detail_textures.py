"""Surface detail for the faceted style (user request, November 2, 2026): plain solid colours read as modern
architecture, trees as coloured pillars and rabbits as painted blobs. Python standard library only.

How it works: every palette colour carries a detail TYPE in its alpha channel (alpha = type*8 + 4; 255 = none).
The TheTrailWalker/FacetedDetail shader projects a tileable grayscale detail texture (one tile per type in an 8x4
atlas) onto each face in object space (triplanar, one tile per metre) and multiplies it over the palette colour.
No UVs or mesh changes are needed, so every existing palette-strip model can take detail.

Run: python Tools/Art/detail_textures.py   (writes the atlas and re-encodes the mapped palettes in place)
"""
from pathlib import Path
import math, random, struct, zlib

ROOT = Path(__file__).resolve().parents[2]
ATLAS = ROOT/'Assets/_Game/Art/Shared/Detail/DetailAtlas.png'
TILE, COLS, ROWS = 256, 8, 4

TYPES = {'flat': 0, 'plaster': 1, 'stone': 2, 'wood': 3, 'bark': 4, 'shingle': 5, 'brick': 6, 'fur': 7,
         'cloth': 8, 'thatch': 9, 'needles': 10, 'soil': 11, 'leather': 12, 'grass': 13, 'planks': 14,
         'fish_scales': 15, 'plant_fibre': 16, 'grain': 17, 'metal': 18, 'ceramic': 19,
         'paper': 20, 'stitched_leather': 21, 'fruit_skin': 22, 'ash': 23, 'glass': 24, 'mineral': 25, 'none': 31}

def alpha(type_name): return 255 if type_name in (None, 'none') else TYPES[type_name]*8 + 4

# Palette-strip files (4 px per colour) and the detail type of each colour slot. Slot meanings come from each
# generator's palette contract; unmapped slots stay flat.
def slots(spec):
    out = {}
    for key, value in spec.items():
        for i in (key if isinstance(key, tuple) else (key,)): out[i] = value
    return out
R = lambda a, b: tuple(range(a, b+1))
PALETTES = {
    'Assets/_Game/Art/Items/Region01/Region01Palette.png': slots({R(0, 2): 'bark', 3: 'wood', R(4, 6): 'fur', 7: 'leather', R(8, 11): 'fruit_skin'}),
    'Assets/_Game/Art/Items/Region02/Region02Palette.png': slots({R(0, 5): 'stone', R(6, 11): 'plant_fibre', R(12, 15): 'wood'}),
    'Assets/_Game/Art/Items/Region03/Region03Palette.png': slots({R(0, 3): 'plant_fibre', R(4, 7): 'soil', R(8, 13): 'stone', R(14, 15): 'plant_fibre'}),
    'Assets/_Game/Art/Items/Region04/Region04Palette.png': slots({R(0, 3): 'stone', R(4, 7): 'fruit_skin', R(10, 15): 'plant_fibre'}),
    'Assets/_Game/Art/Items/Region05/Region05Palette.png': slots({R(0, 1): 'plant_fibre', R(2, 3): 'grain', R(4, 7): 'stone', R(8, 9): 'fruit_skin', 10: 'plant_fibre', 11: 'plant_fibre', R(12, 13): 'fruit_skin', R(14, 15): 'plant_fibre'}),
    'Assets/_Game/Art/Items/Region06/Region06Palette.png': slots({R(0, 4): 'plant_fibre', R(5, 7): 'grain', R(8, 11): 'soil', R(12, 15): 'plant_fibre'}),
    # Tier-two loose resources (regions 7-12, November 6, 2026; generate_tier02_loose_resources.py palette contract).
    'Assets/_Game/Art/Items/Region07/Region07Palette.png': slots({R(0, 2): 'bark', 3: 'wood', R(4, 6): 'fur', 7: 'leather', R(8, 10): 'fruit_skin', R(11, 13): 'plant_fibre', R(14, 15): 'leather'}),
    'Assets/_Game/Art/Items/Region08/Region08Palette.png': slots({R(0, 5): 'plant_fibre', R(6, 7): 'mineral', R(8, 9): 'metal', R(10, 11): 'stone', R(12, 14): 'fruit_skin', 15: 'plant_fibre'}),
    'Assets/_Game/Art/Items/Region09/Region09Palette.png': slots({R(0, 3): 'grain', R(4, 7): 'mineral', R(8, 10): 'fruit_skin', R(11, 13): 'plant_fibre', 14: 'stone', 15: 'plant_fibre'}),
    'Assets/_Game/Art/Items/Region10/Region10Palette.png': slots({R(0, 5): 'stone', R(6, 9): 'soil', R(10, 13): 'grain', R(14, 15): 'plant_fibre'}),
    'Assets/_Game/Art/Items/Region11/Region11Palette.png': slots({R(0, 3): 'grain', R(4, 7): 'fruit_skin', R(8, 9): 'plant_fibre'}),
    'Assets/_Game/Art/Items/Region12/Region12Palette.png': slots({R(0, 3): 'stone', 4: 'mineral', R(5, 6): 'metal', R(7, 10): 'soil', R(11, 14): 'plant_fibre', 15: 'fur'}),
    # Isolated material for the existing Mushrooms mesh. Do not texture the
    # shared crafted palette: its same colour slots also represent other goods.
    'Assets/_Game/Art/Items/Region04/MushroomsPalette.png': slots({10: 'leather', R(27, 28): 'ceramic'}),
    # Harvest interactables: trunk bark 0-2, cut face 3-4, rabbit fur 5-6.
    # Only the trunk and fishing pond still use this palette (drops moved to the item-design palettes, November 5); the
    # pond's stones 29 and reeds 30 gained detail on November 7, 2026. Water 27-28 stays flat.
    'Assets/_Game/Art/Harvest/Tier01/Tier01Palette.png': slots({R(0, 2): 'bark', R(3, 4): 'wood', R(5, 6): 'fur', 29: 'stone', 30: 'plant_fibre'}),
    # Camp props: log seats per wood tier (bark), campfire ash and stones, burned wood.
    # Balloon special gear and mystery boxes, tiers 2-4 (item surface detail, November 7, 2026; tier one uses the merchant palettes).
    'Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsTier01Palette.png': slots({R(0, 1): 'cloth', R(2, 3): 'leather', R(4, 5): 'cloth', R(6, 7): 'metal', 8: 'cloth', 9: 'leather', 10: 'ceramic', R(11, 12): 'wood', 13: 'fur'}),
    'Assets/_Game/Art/Items/BalloonGoods/MysteryBoxTier01Palette.png': slots({0: 'wood', R(2, 3): 'leather', R(6, 7): 'metal', 8: 'cloth', R(11, 12): 'wood'}),
    'Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsTier02Palette.png': slots({R(0, 1): 'cloth', R(2, 3): 'leather', R(4, 5): 'cloth', R(6, 7): 'metal', 8: 'cloth', 9: 'leather', 10: 'ceramic', R(11, 12): 'wood', 13: 'fur'}),
    'Assets/_Game/Art/Items/BalloonGoods/MysteryBoxTier02Palette.png': slots({0: 'wood', R(2, 3): 'leather', R(6, 7): 'metal', 8: 'cloth', R(11, 12): 'wood'}),
    'Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsTier03Palette.png': slots({R(0, 1): 'cloth', R(2, 3): 'leather', R(4, 5): 'cloth', R(6, 7): 'metal', 8: 'cloth', 9: 'leather', 10: 'ceramic', R(11, 12): 'wood', 13: 'fur'}),
    'Assets/_Game/Art/Items/BalloonGoods/MysteryBoxTier03Palette.png': slots({0: 'wood', R(2, 3): 'leather', R(6, 7): 'metal', 8: 'cloth', R(11, 12): 'wood'}),
    'Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsTier04Palette.png': slots({R(0, 1): 'cloth', R(2, 3): 'leather', R(4, 5): 'cloth', R(6, 7): 'metal', 8: 'cloth', 9: 'leather', 10: 'ceramic', R(11, 12): 'wood', 13: 'fur'}),
    'Assets/_Game/Art/Items/BalloonGoods/MysteryBoxTier04Palette.png': slots({0: 'wood', R(2, 3): 'leather', R(6, 7): 'metal', 8: 'cloth', R(11, 12): 'wood'}),
    'Assets/_Game/Art/Environment/CampProps/CampPropsPalette.png': slots({R(0, 5): 'bark', R(6, 7): 'bark', R(9, 11): 'wood', R(12, 23): 'bark', R(24, 26): 'stone', R(27, 30): 'wood'}),
    # Environment kits (32-colour contract): bark 0-3, grasses 4-7, bushes 8-10, pine needles 12-14, stone 16-18,
    # ground 19-23, leaves 27-28.
    **{f'Assets/_Game/Art/Environment/Tier01/Region0{r}/EnvironmentPalette.png':
       slots({R(0, 3): 'bark', R(4, 7): 'grass', R(8, 10): 'needles', R(12, 14): 'needles', R(16, 18): 'stone', R(19, 23): 'soil', R(27, 28): 'needles'})
       for r in range(1, 7)},
    # Tier-two camp fire (November 6, 2026): char and ash 0-3, cut wood 4 and 8, birch bark 5-7, stones 16-18;
    # embers and flames (9-15) stay flat.
    'Assets/_Game/Art/Environment/CampProps/Tier02/Tier02CampPalette.png': slots({R(0, 1): 'wood', R(2, 3): 'soil', 4: 'wood', R(5, 7): 'bark', 8: 'wood', R(16, 18): 'stone'}),
    # Tier-two kits (regions 7-12, November 6, 2026): same slot contract; birch leaves in 12-14; slots 29-31 are sand
    # or snow (region 12) and stay flat.
    **{f'Assets/_Game/Art/Environment/Tier02/Region{r:02d}/EnvironmentPalette.png':
       slots({R(0, 3): 'bark', R(4, 7): 'grass', R(8, 10): 'needles', R(12, 14): 'needles', R(16, 18): 'stone', R(19, 23): 'soil', R(27, 28): 'needles'})
       for r in range(7, 13)},
    'Assets/_Game/Art/Environment/Region01/DawnPlainsPalette.png': slots({R(0, 3): 'bark', R(4, 7): 'grass', R(8, 10): 'needles', R(12, 14): 'needles', R(16, 18): 'stone', R(19, 23): 'soil'}),
    # Camp NPC clothing palettes: cloth 8-10, leather 11-13, boots 14-15, pack 16-17, rags 19-20.
    **{f'Assets/_Game/Art/Characters/CampCommon/{name}Palette.png': slots({R(8, 10): 'cloth', R(11, 15): 'leather', R(16, 17): 'cloth', R(19, 20): 'cloth'})
       for name in ('Tier00Rags', 'Tier01HempRabbit', 'Tier02FlaxBeaver', 'Tier03CottonFeather', 'Tier04CottonBoar')},
    # Mission lady and ferryman: dress, coat, hats, shirt, waistcoat, trousers (cloth), leather, rope.
    'Assets/_Game/Art/Characters/CampSpecial/CampSpecialPalette.png': slots({R(8, 10): 'cloth', 11: 'leather', R(12, 13): 'cloth', R(15, 17): 'cloth', 20: 'cloth', 21: 'wood', 22: 'thatch', 29: 'cloth', 31: 'leather'}),
}
# Loose rocks need a natural mineral surface, not the masonry/mortar tile used by buildings.
for _path, _mapping in PALETTES.items():
    if '/Art/Items/' in _path:
        for _slot, _kind in list(_mapping.items()):
            if _kind == 'stone': _mapping[_slot] = 'mineral'

# ------------------------------------------------------------------ PNG io (8-bit RGB/RGBA, non-interlaced)

def read_png(path):
    data = Path(path).read_bytes(); assert data[:8] == b'\x89PNG\r\n\x1a\n', path
    pos, idat, w = 8, b'', 0
    while pos < len(data):
        n = struct.unpack('>I', data[pos:pos+4])[0]; tag = data[pos+4:pos+8]; body = data[pos+8:pos+8+n]; pos += 12+n
        if tag == b'IHDR': w, h, depth, ctype = struct.unpack('>IIBB', body[:10]); assert depth == 8 and ctype in (2, 6), (path, depth, ctype)
        elif tag == b'IDAT': idat += body
    bpp = 3 if ctype == 2 else 4; raw = zlib.decompress(idat); stride = w*bpp; rows = []; prev = bytearray(stride); p = 0
    for _ in range(h):
        f = raw[p]; line = bytearray(raw[p+1:p+1+stride]); p += 1+stride
        for i in range(stride):
            a = line[i-bpp] if i >= bpp else 0; b = prev[i]; c = prev[i-bpp] if i >= bpp else 0
            if f == 1: line[i] = (line[i]+a) & 255
            elif f == 2: line[i] = (line[i]+b) & 255
            elif f == 3: line[i] = (line[i]+(a+b)//2) & 255
            elif f == 4:
                pa, pb, pc = abs(b-c), abs(a-c), abs(a+b-2*c)
                line[i] = (line[i]+(a if pa <= pb and pa <= pc else b if pb <= pc else c)) & 255
        rows.append([tuple(line[x*bpp:x*bpp+bpp]) for x in range(w)]); prev = line
    return w, h, rows

def write_png(path, w, h, rows, rgba=True):
    raw = b''.join(b'\x00'+bytes(v for px in row for v in (px if rgba else px[:3])) for row in rows)
    def chunk(tag, body): return struct.pack('>I', len(body))+tag+body+struct.pack('>I', zlib.crc32(tag+body) & 0xffffffff)
    png = b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, 6 if rgba else 2, 0, 0, 0))+chunk(b'IDAT', zlib.compress(raw, 9))+chunk(b'IEND', b'')
    Path(path).parent.mkdir(parents=True, exist_ok=True); Path(path).write_bytes(png)

def encode_palette(path, mapping, cell=4):
    w, h, rows = read_png(path)
    out = [[(px[0], px[1], px[2], alpha(mapping.get(x//cell))) for x, px in enumerate(row)] for row in rows]
    write_png(path, w, h, out)

def palette_alpha_for(filename):
    """Alpha list for generate_region01_meshes.palette(): keeps the detail mapping when a generator is rerun."""
    for key, mapping in PALETTES.items():
        if key.endswith('/'+filename): return mapping
    return {}

def migrate_legacy_palettes():
    """Preserve RGB and explicit Palette.add mappings without regenerating scenes.

    The old atlas dimensions identify the old encoding. Running again after
    build_atlas is safe; new palettes must be generated after this migration.
    """
    if not ATLAS.exists() or read_png(ATLAS)[:2] != (1024, 1024): return
    for path in sorted((ROOT/'Assets/_Game/Art').rglob('*Palette.png')):
        w, h, rows = read_png(path)
        if any(len(px) == 4 and px[3] != 255 for row in rows for px in row):
            converted = [[(*px[:3], 255 if len(px) == 3 else
                           (px[3]//16)*8+4 if px[3] % 16 == 8 else px[3])
                          for px in row] for row in rows]
            write_png(path, w, h, converted)
            print('Migrated detail encoding:', path.relative_to(ROOT))

def mushroom_palette():
    """Copy RGB only from the shared crafted palette; give mushrooms their own detail semantics."""
    source = ROOT/'Assets/_Game/Art/Crafting/Tier01/Tier01CraftedPalette.png'
    target = ROOT/'Assets/_Game/Art/Items/Region04/MushroomsPalette.png'
    w, h, rows = read_png(source)
    mapping = PALETTES[str(target.relative_to(ROOT)).replace('\\', '/')]
    write_png(target, w, h, [[(*px[:3], alpha(mapping.get(x//4))) for x, px in enumerate(row)] for row in rows])

# ------------------------------------------------------------------ tileable noise

class Noise:
    def __init__(self, seed, period):
        rng = random.Random(seed); self.p = period
        self.g = [[rng.random() for _ in range(period)] for _ in range(period)]
    def __call__(self, u, v):
        x, y = u*self.p, v*self.p; x0, y0 = int(math.floor(x)), int(math.floor(y)); fx, fy = x-x0, y-y0
        sx, sy = fx*fx*(3-2*fx), fy*fy*(3-2*fy); p = self.p; g = self.g
        a = g[y0 % p][x0 % p]; b = g[y0 % p][(x0+1) % p]; c = g[(y0+1) % p][x0 % p]; d = g[(y0+1) % p][(x0+1) % p]
        return (a+(b-a)*sx)+((c+(d-c)*sx)-(a+(b-a)*sx))*sy

def fbm(layers, u, v):
    total = norm = 0.0
    for amp, n in layers: total += amp*n(u, v); norm += amp
    return total/norm

def clamp(x): return 0.0 if x < 0 else 1.0 if x > 1 else x

def pattern(kind, seed=7):
    rng = random.Random(seed)
    N = lambda period, s=0: Noise(seed*31+period+s, period)
    soft = [(1, N(4)), (.5, N(8)), (.25, N(16))]; fine = [(1, N(32)), (.5, N(64))]
    if kind == 'flat': return lambda u, v: .5
    if kind == 'plant_fibre':
        strands = Noise(seed+20, 96)
        return lambda u, v: .5 + .18*math.sin(math.tau*(u*32+.35*math.sin(v*math.tau))) + .12*(strands(u, v)-.5)
    if kind == 'grain':
        def f(u, v):
            row = int(v*8); x = (u*12+.5*(row%2))%1-.5; y = (v*8)%1-.5
            return .5 + .13*math.cos(math.tau*x)*math.cos(math.tau*y) - (.09 if abs(x)<.035 and abs(y)<.3 else 0) + .05*(fbm(fine,u,v)-.5)
        return f
    if kind == 'fruit_skin':
        pores = Noise(seed+21, 128)
        return lambda u, v: .5 + .09*(fbm(soft,u,v)-.5) + .14*(pores(u,v)-.5)
    if kind == 'mineral':
        return lambda u,v: .5 + .25*(fbm(soft,u,v)-.5) + .12*(fbm(fine,u,v)-.5)
    if kind == 'fish_scales':
        def f(u,v):
            row=int(v*10); x=(u*10+.5*(row%2))%1-.5; y=(v*10)%1
            edge=abs(math.hypot(x,y*.65)-.52)
            return .53 - .18*clamp(1-edge/.07) + .07*(fbm(fine,u,v)-.5)
        return f
    if kind == 'metal':
        brushed = Noise(seed+22,96)
        return lambda u,v: .5 + .12*(brushed(u,v*.125)-.5) + .04*math.sin(u*math.tau*64)
    if kind == 'ceramic':
        return lambda u,v: .5 + .08*(fbm(soft,u,v)-.5) + .06*(fbm(fine,u,v)-.5)
    if kind == 'paper':
        return lambda u,v: .5 + .12*(fbm(fine,u,v)-.5) + .06*(fbm(soft,u,v)-.5)
    if kind == 'stitched_leather':
        leather=pattern('leather',seed)
        return lambda u,v: leather(u,v) + (.12 if abs(u-.12)<.012 and (v*16)%1<.6 else 0)
    if kind == 'ash':
        return lambda u,v: .5 + .4*(fbm(soft,u,v)-.5) + .25*(fbm(fine,u,v)-.5)
    if kind == 'glass':
        return lambda u,v: .5 + .04*math.sin((u+v)*math.tau*3) + .025*(fbm(fine,u,v)-.5)
    if kind == 'plaster':
        cracks = set()
        for _ in range(5):
            x, y = rng.random(), rng.random(); ang = rng.uniform(0, math.tau)
            for _ in range(70):
                ang += rng.uniform(-.5, .5); x = (x+math.cos(ang)/TILE*1.5) % 1; y = (y+math.sin(ang)/TILE*1.5) % 1
                cracks.add((int(x*TILE), int(y*TILE)))
        def f(u, v):
            value = .5 + .28*(fbm(soft, u, v)-.5) + .16*(fbm(fine, u, v)-.5)
            return value - (.22 if (int(u*TILE), int(v*TILE)) in cracks else 0)
        return f
    if kind in ('stone', 'brick'):
        rows = 4 if kind == 'stone' else 8; widths = []
        for r in range(rows):
            xs, x = [0.0], 0.0
            while x < 1:
                step = rng.uniform(.18, .34) if kind == 'stone' else .25
                x = min(1.0, x+step); xs.append(x)
            if xs[-1] - xs[-2] < .08: xs.pop(-2)
            off = rng.random() if kind == 'stone' else (.125 if r % 2 else 0)
            widths.append((xs, off, [rng.uniform(-.13, .13) for _ in xs]))
        mortar = .013 if kind == 'stone' else .012
        def f(u, v):
            r = int(v*rows) % rows; xs, off, tones = widths[r]; uu = (u+off) % 1
            for i in range(len(xs)-1):
                if xs[i] <= uu < xs[i+1]: break
            ex = min(uu-xs[i], xs[i+1]-uu); ey = min(v*rows-math.floor(v*rows), math.ceil(v*rows)-v*rows)/rows
            edge = min(ex, ey)
            if edge < mortar: return .28 + .05*fbm(fine, u, v)
            bevel = .12*clamp(1-(edge-mortar)/.02)
            return .55 + tones[i] + .22*(fbm(soft, u, v)-.5) + .10*(fbm(fine, u, v)-.5) - bevel
        return f
    if kind == 'wood':
        warp = N(6)
        return lambda u, v: .5 + .14*math.sin((u*14 + 1.6*warp(u, v))*math.tau) + .12*(fbm(fine, u*2 % 1, v*.5 % 1)-.5) + .08*(fbm(soft, u, v)-.5)
    if kind == 'planks':
        rows, warp = 5, N(6)
        seams = [rng.random() for _ in range(rows)]; tones = [rng.uniform(-.08, .08) for _ in range(rows)]
        def f(u, v):
            r = int(v*rows) % rows; fy = v*rows - math.floor(v*rows)
            if fy < .045 or abs(((u+seams[r]) % 1)-.5) < .006: return .25
            nail = (abs(((u+seams[r]) % 1)-.47) < .012 or abs(((u+seams[r]) % 1)-.53) < .012) and abs(fy-.5) < .05
            if nail: return .2
            return .52 + tones[r] + .12*math.sin((v*40 + 2.5*warp(u, v))*math.tau)*.6 + .10*(fbm(fine, u, v)-.5)
        return f
    if kind == 'bark':
        cols = 9; edges = sorted(rng.random() for _ in range(cols)); breaks = N(10, 3); warp = N(5, 5)
        def f(u, v):
            uu = (u + .05*(warp(u, v)-.5)) % 1
            dist = min(min(abs(uu-e), 1-abs(uu-e)) for e in edges)
            furrow = clamp(dist/.045)
            plate = .58 + .12*(breaks(u, v)-.5)
            crack = .10 if abs(((v*3.0 + 4*breaks(uu, v*.2 % 1)) % 1)-.5) < .012 and furrow > .6 else 0
            return .22 + (plate-.22)*furrow - crack + .08*(fbm(fine, u, v)-.5)
        return f
    if kind == 'shingle':
        rows, cols = 6, 5
        def f(u, v):
            r = int(v*rows) % rows; fy = v*rows-math.floor(v*rows); uu = (u + (.5/cols if r % 2 else 0)) % 1
            fx = uu*cols - math.floor(uu*cols)
            gap = abs(fx-.5) > .46 or fy < .06
            round_edge = fy < .22 and (fx-.5)**2*2.2 + (.22-fy)**2*9 > .1
            if gap or round_edge: return .24
            return .38 + .35*fy + .10*(fbm(fine, u, v)-.5)
        return f
    if kind == 'fur':
        strands = [(1, Noise(seed+1, 48)), (.6, Noise(seed+2, 96))]
        return lambda u, v: .5 + .55*(fbm(strands, u, (v*.25) % 1)-.5) + .15*(fbm(soft, u, v)-.5)
    if kind == 'cloth':
        def f(u, v):
            cx, cy = int(u*64), int(v*64); fx, fy = u*64-cx, v*64-cy
            warp_on = (cx+cy) % 2 == 0
            t = fx if warp_on else fy
            return .5 + .14*math.cos(t*math.tau) + .08*(fbm(fine, u, v)-.5) + .06*(fbm(soft, u, v)-.5)
        return f
    if kind == 'thatch':
        strands = [(1, Noise(seed+3, 64)), (.5, Noise(seed+4, 128))]
        return lambda u, v: .5 + .6*(fbm(strands, u, (v*.12) % 1)-.5) - (.18 if (v*5) % 1 < .05 else 0)
    if kind == 'needles':
        a = Noise(seed+5, 40); b = Noise(seed+6, 40)
        return lambda u, v: .5 + .45*(a((u+v) % 1, ((u-v)*.3) % 1)-.5) + .35*(b((u-v) % 1, ((u+v)*.3) % 1)-.5)
    if kind == 'soil':
        speck = Noise(seed+7, 128)
        return lambda u, v: .5 + .3*(fbm(soft, u, v)-.5) + .3*(speck(u, v)-.5) - (.15 if speck(u, v) > .82 else 0)
    if kind == 'leather':
        pores = Noise(seed+8, 96)
        return lambda u, v: .5 + .22*(fbm(soft, u, v)-.5) + .10*(pores(u, v)-.5)
    if kind == 'grass':
        blades = [(1, Noise(seed+9, 64)), (.5, Noise(seed+10, 128))]
        return lambda u, v: .5 + .5*(fbm(blades, u, (v*.2) % 1)-.5) + .15*(fbm(soft, u, v)-.5)
    raise ValueError(kind)

def build_atlas():
    width, height = TILE*COLS, TILE*ROWS
    rows = [[(128, 128, 128, 255)]*width for _ in range(height)]
    names = sorted(TYPES, key=TYPES.get)
    for name in names:
        t = TYPES[name]; f = pattern(name if name != 'none' else 'flat', seed=11+t)
        ox, oy = (t % COLS)*TILE, (t//COLS)*TILE
        for y in range(TILE):
            # PNG rows run top to bottom; tile v runs bottom to top so "up" in the pattern is +V in the shader.
            # Eight wrapped gutter pixels isolate bilinear/mip samples.
            v = ((TILE-1-y-8+.5) % (TILE-16))/(TILE-16)
            row = rows[oy+y]
            for x in range(TILE):
                u = ((x-8+.5) % (TILE-16))/(TILE-16)
                g = int(round(clamp(f(u, v))*255)); row[ox+x] = (g, g, g, 255)
    write_png(ATLAS, width, height, rows, rgba=False)

if __name__ == '__main__':
    migrate_legacy_palettes()
    mushroom_palette()
    build_atlas(); print('Detail atlas:', ATLAS.relative_to(ROOT))
    for path, mapping in PALETTES.items():
        full = ROOT/path
        if full.exists(): encode_palette(full, mapping); print('Detail alpha:', path)
        else: print('Missing palette (skipped):', path)
