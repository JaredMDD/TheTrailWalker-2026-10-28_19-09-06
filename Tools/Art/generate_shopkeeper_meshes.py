"""Settlement shopkeepers (user request, November 2, 2026): one static NPC per storefront, shown from the waist up
behind the shop counter, each dressed for its shop. Faceted style on the shared skeleton (faceted_people).
Art only: no shop logic, stock, dialogue or animation. Python standard library only.

Per keeper two parts: <Name>Body in the torso frame (waist-up torso, arms resting on the counter, outfit) and
<Name>Head in the head frame (head, face, hair, facial hair, headwear). Standing pivots: torso 0.94 m, head +0.49;
the posed hands rest at about 1.03 m, so a counter top near 1.02 m hides everything below the waist.
"""
from pathlib import Path
import json
import math
import faceted_character_kit as k
import faceted_people as fp

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Art/Characters/Shopkeepers'
HEADER = 'The Trail Walker settlement shopkeeper (faceted); metres; +Y up; +Z forward; rig-frame local.'

pal = k.Palette()
for name, color, *detail in [
    # Skin tones (one per keeper) and hair colours.
    ('SkinA', '#E8BE98'), ('SkinB', '#C68A5E'), ('SkinC', '#9A6440'), ('SkinD', '#6E432E'), ('SkinE', '#D6A477'), ('SkinF', '#4A2E22'), ('SkinG', '#B57A50'),
    ('HairBrown', '#5A3A24'), ('HairBlack', '#221A15'), ('HairBlonde', '#B99A60'), ('HairGray', '#8A867C'), ('HairAuburn', '#8D4E30'), ('HairDarkBrown', '#3A2518'),
    ('Eye', '#1C1612'), ('Lips', '#8A5440'), ('Soot', '#5A4C44'),
    # Shared trims.
    ('White', '#ECE6D6', 'cloth'), ('Cream', '#D8CBAE', 'cloth'), ('Leather', '#7A5234', 'leather'), ('LeatherDark', '#4E3420', 'leather'), ('Brass', '#B8913A'), ('Steel', '#7C8084'),
    ('Red', '#A23B2C', 'cloth'), ('Dark', '#2E2B2A'),
    # Theme colours.
    ('Flour', '#F6F2EA', 'cloth'), ('Denim', '#5E7590', 'cloth'), ('HatTan', '#B08A5A', 'leather'), ('Straw', '#D8B868', 'thatch'), ('StrawDark', '#B4964F', 'thatch'), ('Olive', '#8A9A5B', 'cloth'),
    ('Dungaree', '#55657A', 'cloth'), ('Jacket', '#4A4A48', 'cloth'), ('Lamp', '#F3E6A0'), ('Navy', '#3E5A78', 'cloth'), ('Sweater', '#C9B99A', 'cloth'), ('Oilskin', '#D9A23A', 'leather'),
    ('Plum', '#5A3E5D', 'cloth'), ('Tape', '#E3C24E'), ('Visor', '#3F7A52'), ('Waistcoat', '#6B4C34', 'cloth'), ('Garter', '#8A2C2C', 'cloth'), ('Ledger', '#5B2E26'),
    # Balloon merchant signature: crimson lacquer, brass/gold trim, cream silk.
    ('Crimson', '#9C271E', 'cloth'), ('CrimsonDark', '#701D19', 'cloth'), ('Gold', '#D9A441'), ('Silk', '#F0E2B6', 'cloth'), ('Glass', '#7FA6B0'), ('SkinH', '#C99A70')]:
    pal.add(name, color, *detail)

# ------------------------------------------------------------------ posed arms (hands resting on the counter)

UPPER_PITCH, FORE_PITCH, HAND_PITCH, SPREAD = -25, -60, 5, 8

def arm_chain(female, side):
    sx, sy = fp.SHOULDER[female]
    shoulder = (side*sx, sy, 0)
    def from_shoulder(p): return k.add(fp.rot_z(fp.rot_x(p, UPPER_PITCH), side*SPREAD), shoulder)
    def from_elbow(p): return from_shoulder(k.add(fp.rot_x(p, FORE_PITCH), (0, -fp.UPPER_ARM, 0)))
    def from_wrist(p): return from_elbow(k.add(fp.rot_x(p, HAND_PITCH), (0, -fp.FOREARM, 0)))
    return from_shoulder, from_elbow, from_wrist

def mirror_x(m, v0, f0):
    for i in range(v0, len(m.vertices)): x, y, z = m.vertices[i]; m.vertices[i] = (-x, y, z)
    m.faces[f0:] = [(a, c, b, kk) for a, b, c, kk in m.faces[f0:]]

def arms(m, female, skin, sleeve, cuff, long_sleeves=False, rolled=True):
    for side in (-1, 1):
        sh, el, wr = arm_chain(female, side)
        if long_sleeves:
            fp.place(m, lambda mm: fp.sleeve(mm, fp.UPPER_ARM+.01, sleeve, sleeve), sh)
            fp.place(m, lambda mm: fp.forearm(mm, sleeve_color=sleeve, cuff=cuff), el)
        else:
            fp.place(m, lambda mm: fp.sleeve(mm, fp.UPPER_ARM-.02, sleeve, cuff, rolled=rolled), sh)
            fp.place(m, lambda mm: fp.forearm(mm, skin=skin), el)
        # Hand: built for the left side (thumb toward the body), mirrored for the right.
        v0, f0 = len(m.vertices), len(m.faces)
        _, _, left = arm_chain(female, -1)
        fp.place(m, lambda mm: fp.hand(mm, skin=skin, shade=skin), left)
        if side > 0: mirror_x(m, v0, f0)

def neck(m, female, skin):
    r = .046 if female else .052
    k.limb(m, [((0, .43, -.012), r, r-.002), ((0, .50, -.010), r-.002, r-.004), ((0, .54, -.012), r-.004, r-.006)], skin, sides=6)

def front(female, x, y, lift=0.0): return fp.torso_front(female, x, y) + lift

def apron(m, female, color, strap, y_top=.30):
    """Bib apron: a front panel from the waist to the chest with neck and waist straps."""
    rows = []
    for y in (-.080, .020, .120, .220, y_top):
        w = .150 if y < .2 else .110
        rows.append((y, w))
    for (y0, w0), (y1, w1) in zip(rows, rows[1:]):
        ids = [m.v((-w0, y0, front(female, -w0, y0, .010))), m.v((w0, y0, front(female, w0, y0, .010))),
               m.v((w1, y1, front(female, w1, y1, .010))), m.v((-w1, y1, front(female, -w1, y1, .010)))]
        m.quad(*ids, color, (0, (y0+y1)/2, 0))
    for s in (-1, 1):
        a = (s*.100, y_top, front(female, s*.1, y_top, .010)); c = (s*.060, .470, .030)
        along = k.unit(k.sub(c, a)); k.box(m, k.lerp(a, c, .5), (.026, k.length(k.sub(c, a)), .008), strap, axes=((1, 0, 0), along, k.cross((1, 0, 0), along)))
    sec = fp.torso_sections(female)
    w, f, bk = fp.section_at(sec, .02)
    k.vertical_loft(m, [(.005, w+.008, f+.012, bk+.008), (.030, w+.008, f+.012, bk+.008)], strap, a=.70, b=.62, cap_start=False, cap_end=False)

# ------------------------------------------------------------------ headwear (head frame)

def head_ring(female, y, gap):
    w, f, bk = fp.section_at(fp.head_sections(female), y)
    return (y, w+gap, f+gap, bk+gap)

def toque(m, female):
    k.vertical_loft(m, [head_ring(female, .200, .014), head_ring(female, .232, .014)], 'White', a=.62, b=.55, cap_start=False, cap_end=False)
    k.vertical_loft(m, [(.228, .090, .108, .112), (.290, .106, .124, .128), (.350, .118, .134, .138), (.372, .100, .114, .118), (.382, .050, .058, .060)],
                    'Flour', a=.55, b=.50, cap_start=False)

def wide_hat(m, female, crown, band, brim_color, under, radius=.21, droop=.018):
    k.vertical_loft(m, [head_ring(female, .205, .016), (.270, .092, .108, .114), (.320, .080, .094, .100), (.335, .050, .060, .064)],
                    crown, a=.62, b=.55, cap_start=False)
    k.box(m, (0, .336, -.004), (.040, .012, .110), under, taper=.8)          # crown pinch
    k.vertical_loft(m, [head_ring(female, .208, .019), head_ring(female, .232, .019)], band, a=.62, b=.55, cap_start=False, cap_end=False)
    w = fp.section_at(fp.head_sections(female), .205)[0]
    fp.brim(m, .206, radius, radius*.92, w+.012, brim_color, under, droop=droop)

def miner_helmet(m, female):
    k.vertical_loft(m, [head_ring(female, .190, .018), head_ring(female, .240, .020), (.290, .080, .094, .100), (.318, .040, .048, .052)],
                    'Leather', a=.60, b=.55, cap_start=False)
    w = fp.section_at(fp.head_sections(female), .19)[0]
    fp.brim(m, .192, w+.040, w+.050, w+.012, 'LeatherDark', 'LeatherDark', droop=0.0, thickness=.008)
    z = fp.section_at(fp.head_sections(female), .24)[1] + .020
    k.box(m, (0, .245, z+.012), (.050, .040, .030), 'Brass')
    k.box(m, (0, .245, z+.029), (.032, .026, .006), 'Lamp', shade=4)
    k.box(m, (0, .275, z+.002), (.016, .030, .012), 'Brass')

def beanie(m, female, color, cuff):
    k.vertical_loft(m, [head_ring(female, .185, .014), head_ring(female, .230, .016), (.272, .074, .086, .094), (.300, .044, .050, .056), (.312, .012, .014, .016)],
                    color, a=.60, b=.55, cap_start=False)
    k.vertical_loft(m, [head_ring(female, .176, .022), head_ring(female, .214, .022)], cuff, a=.60, b=.55, cap_start=False, cap_end=False)

def eyeshade(m, female):
    k.vertical_loft(m, [head_ring(female, .205, .010), head_ring(female, .226, .010)], 'Dark', a=.62, b=.55, cap_start=False, cap_end=False)
    f = fp.section_at(fp.head_sections(female), .21)[1]
    k.box(m, (0, .214, f+.048), (.150, .006, .085), 'Visor', axes=((1, 0, 0), (0, 1, .45), (0, -.45, 1)), taper=.8)

def spectacles(m, female):
    sec = fp.head_sections(female); y = .148
    for s in (-1, 1):
        x = s*.034; z = fp.front_z(sec, x, y)+.010
        for dx, dy, w, h in ((0, .012, .034, .004), (0, -.012, .034, .004), (-.016, 0, .004, .026), (.016, 0, .004, .026)):
            k.box(m, (x+dx, y+dy, z), (w, h, .004), 'Dark')
        k.box(m, (s*.068, y+.004, z-.040), (.004, .004, .080), 'Dark')
    k.box(m, (0, y+.004, fp.front_z(sec, 0, y)+.014), (.018, .004, .004), 'Dark')

def pencil(m, female):
    w = fp.section_at(fp.head_sections(female), .15)[0]
    k.box(m, (w+.010, .150, .010), (.008, .008, .090), 'Tape', axes=((1, 0, 0), (0, 1, 0), (0, -.25, 1)))

# ------------------------------------------------------------------ keepers

def body_base(m, female, skin, shirt, shirt_dark, long_sleeves=False, rolled=True, sleeve=None, cuff=None):
    fp.torso(m, female, cloth=shirt, hem=shirt, collar=shirt_dark, hem_band=False)
    neck(m, female, skin)
    arms(m, female, skin, sleeve or shirt, cuff or shirt_dark, long_sleeves, rolled)

def head_base(m, female, skin, hair, style, beard=False, brows=True):
    pal_skin = skin
    fp.head(m, female, skin=pal_skin, shade=pal_skin, lips='Lips')
    fp.eyes(m, female)
    if brows: fp.brows(m, female, hair)
    if style: fp.hair(m, female, style, hair, hair)
    if beard: fp.beard(m, hair, hair)

KEEPERS = []
def keeper(name, shop, female, notes):
    def wrap(fn): KEEPERS.append(dict(Name=name, Shop=shop, Female=female, Notes=notes, Build=fn)); return fn
    return wrap

@keeper('Baker', 'Bakery', True, 'Flour-white toque and bib apron over a cream shirt with rolled sleeves; warm, round-shouldered stance.')
def baker():
    body = k.Mesh('BakerBody', pal); body_base(body, True, 'SkinA', 'Cream', 'White')
    apron(body, True, 'Flour', 'White')
    head = k.Mesh('BakerHead', pal); head_base(head, True, 'SkinA', 'HairBrown', 'Bob'); toque(head, True)
    # A smudge of flour on the cheek.
    k.box(head, (.048, .118, fp.front_z(fp.FEMALE_HEAD, .048, .118)+.002), (.020, .014, .004), 'Flour')
    return body, head

@keeper('Rancher', 'Ranch', False, 'Wide-brim tan hat, denim shirt, open leather vest and a red neckerchief; full beard.')
def rancher():
    body = k.Mesh('RancherBody', pal); body_base(body, False, 'SkinB', 'Denim', 'Navy', long_sleeves=True)
    rows = [(y, w+.010, f+.012, bk+.010) for y, w, f, bk in fp.torso_sections(False) if -.08 <= y <= .44]
    fp.open_shell(body, rows, 'Leather', 'LeatherDark', 46, edge='LeatherDark', edge_size=.010)
    top = fp.MALE_TORSO[-1]
    k.box(body, (0, top[0]-.035, fp.torso_front(False, 0, top[0]-.035)+.010), (.090, .070, .012), 'Red', taper=.25, axes=((1, 0, 0), (0, -1, 0), (0, 0, 1)))
    k.vertical_loft(body, [(top[0]-.012, top[1]+.012, top[2]+.012, top[3]+.012), (top[0]+.012, top[1]+.004, top[2]+.006, top[3]+.006)], 'Red', a=.70, b=.62, cap_start=False, cap_end=False)
    head = k.Mesh('RancherHead', pal); head_base(head, False, 'SkinB', 'HairGray', 'Crop', beard=True)
    wide_hat(head, False, 'HatTan', 'LeatherDark', 'HatTan', 'LeatherDark')
    return body, head

@keeper('Farmer', 'Farm', True, 'Straw hat with a green band, olive blouse with rolled sleeves and dungarees with a bib and straps.')
def farmer():
    body = k.Mesh('FarmerBody', pal); body_base(body, True, 'SkinC', 'Olive', 'StrawDark')
    apron(body, True, 'Dungaree', 'Dungaree', y_top=.27)
    for s in (-1, 1): k.box(body, (s*.085, .270, fp.torso_front(True, s*.085, .27)+.018), (.018, .018, .008), 'Brass')
    k.box(body, (0, .150, fp.torso_front(True, 0, .15)+.016), (.090, .070, .008), 'Dungaree', shade=1)   # bib pocket
    head = k.Mesh('FarmerHead', pal); head_base(head, True, 'SkinC', 'HairBlonde', 'Tied')
    wide_hat(head, True, 'Straw', 'Olive', 'Straw', 'StrawDark', radius=.23, droop=.026)
    return body, head

@keeper('Miner', 'Mine', False, 'Leather helmet with a brass lamp, dark work jacket with leather shoulder pads, red kerchief and soot on the face.')
def miner():
    body = k.Mesh('MinerBody', pal); body_base(body, False, 'SkinD', 'Jacket', 'Dark', long_sleeves=True)
    sx, sy = fp.SHOULDER[False]
    for s in (-1, 1):
        k.box(body, (s*(sx-.040), sy+.020, 0), (.090, .022, .160), 'Leather', axes=((1, -s*.35, 0), (s*.35, 1, 0), (0, 0, 1)))
    for y in (.06, .16, .26, .36): k.box(body, (.020, y, fp.torso_front(False, .02, y)+.005), (.012, .012, .006), 'Steel')
    k.box(body, (.020, .220, fp.torso_front(False, .02, .22)+.002), (.006, .400, .004), 'Dark')
    top = fp.MALE_TORSO[-1]
    k.vertical_loft(body, [(top[0]-.014, top[1]+.014, top[2]+.014, top[3]+.014), (top[0]+.012, top[1]+.004, top[2]+.006, top[3]+.006)], 'Red', a=.70, b=.62, cap_start=False, cap_end=False)
    head = k.Mesh('MinerHead', pal); head_base(head, False, 'SkinD', 'HairBlack', 'Crop', beard=True)
    miner_helmet(head, False)
    for x, y in ((-.050, .118), (.040, .175)):
        k.box(head, (x, y, fp.front_z(fp.MALE_HEAD, x, y)+.0015), (.024, .016, .003), 'Soot')
    return body, head

@keeper('Fisher', 'Fishery', False, 'Navy knit beanie, cream roll-neck sweater with cable bands and yellow oilskin bib overalls; gray beard.')
def fisher():
    body = k.Mesh('FisherBody', pal); body_base(body, False, 'SkinE', 'Sweater', 'Cream', long_sleeves=True)
    top = fp.MALE_TORSO[-1]
    k.vertical_loft(body, [(top[0]-.020, top[1]+.012, top[2]+.012, top[3]+.012), (top[0]+.040, top[1]-.004, top[2]-.002, top[3]-.002)], 'Sweater', a=.70, b=.62, cap_start=False, cap_end=False)
    for x in (-.050, .050): k.box(body, (x, .200, fp.torso_front(False, x, .2)+.004), (.016, .300, .006), 'Cream')
    apron(body, False, 'Oilskin', 'Oilskin', y_top=.25)
    head = k.Mesh('FisherHead', pal); head_base(head, False, 'SkinE', 'HairGray', None, beard=True)
    beanie(head, False, 'Navy', 'Navy')
    return body, head

@keeper('Tailor', 'Tailor-material shop', True, 'Plum waistcoat over a white shirt, measuring tape around the neck, wrist pincushion and spectacles.')
def tailor():
    body = k.Mesh('TailorBody', pal); body_base(body, True, 'SkinF', 'White', 'White', long_sleeves=True)
    rows = [(y, w+.008, f+.010, bk+.008) for y, w, f, bk in fp.torso_sections(True) if -.08 <= y <= .41]
    fp.open_shell(body, rows, 'Plum', 'Plum', 30, edge='Brass', edge_size=.006)
    # Measuring tape draped around the neck and down the chest, with marks.
    for s in (-1, 1):
        a = (s*.045, .470, .030); c = (s*.060, .200, fp.torso_front(True, s*.06, .20)+.016)
        along = k.unit(k.sub(c, a)); k.box(body, k.lerp(a, c, .5), (.022, k.length(k.sub(c, a)), .004), 'Tape', axes=((1, 0, 0), along, k.cross((1, 0, 0), along)))
        for t in (.2, .4, .6, .8):
            k.box(body, k.add(k.lerp(a, c, t), (0, 0, .003)), (.022, .003, .003), 'Dark')
    _, _, wr = arm_chain(True, -1)
    k.box(body, wr((0, .02, .030)), (.050, .030, .050), 'Red')
    head = k.Mesh('TailorHead', pal); head_base(head, True, 'SkinF', 'HairBlack', 'Bob'); spectacles(head, True)
    return body, head

@keeper('Trader', 'Warehouse / trading house', False, 'Green eyeshade visor, white shirt with red sleeve garters, brown waistcoat with a brass watch chain, bow tie and a pencil behind the ear.')
def trader():
    body = k.Mesh('TraderBody', pal); body_base(body, False, 'SkinG', 'White', 'White', long_sleeves=True)
    rows = [(y, w+.010, f+.012, bk+.010) for y, w, f, bk in fp.torso_sections(False) if -.08 <= y <= .43]
    fp.open_shell(body, rows, 'Waistcoat', 'Waistcoat', 34, edge='LeatherDark', edge_size=.008)
    for y in (.05, .13, .21): k.box(body, (.040, y, fp.torso_front(False, .04, y)+.018), (.012, .012, .008), 'Brass')
    k.box(body, (-.060, .110, fp.torso_front(False, -.06, .11)+.018), (.080, .006, .006), 'Brass')
    top = fp.MALE_TORSO[-1]
    for s in (-1, 1): k.box(body, (s*.022, top[0]-.006, fp.torso_front(False, 0, top[0])+.014), (.036, .028, .012), 'Dark', taper=.5, axes=((0, 1, 0), (-s, 0, 0), (0, 0, 1)))
    for side in (-1, 1):
        sh, _, _ = arm_chain(False, side)
        fp.place(body, lambda mm: k.limb(mm, [((0, -.150, 0), .066, .068), ((0, -.175, 0), .066, .068)], 'Garter', sides=8, turn=math.pi/8), sh)
    head = k.Mesh('TraderHead', pal); head_base(head, False, 'SkinG', 'HairDarkBrown', 'Sweep'); eyeshade(head, False); pencil(head, False)
    return body, head

@keeper('BalloonMerchant', 'Balloon merchant', False, 'Crimson lacquer coat, double-breasted with brass buttons and gold edging and epaulettes, cream silk scarf, brass balloon emblem pin, leather aviator cap with brass goggles pushed up, handlebar moustache.')
def balloon_merchant():
    body = k.Mesh('BalloonMerchantBody', pal)
    body_base(body, False, 'SkinH', 'Crimson', 'Gold', long_sleeves=True, cuff='Gold')
    # Double-breasted front: an offset gold edge and two rows of brass buttons.
    k.box(body, (.045, .200, fp.torso_front(False, .045, .2)+.003), (.008, .520, .006), 'Gold')
    for y in (.06, .15, .24, .33):
        for x in (-.050, .085): k.box(body, (x, y, fp.torso_front(False, x, y)+.006), (.016, .016, .008), 'Brass')
    sx, sy = fp.SHOULDER[False]
    for s_ in (-1, 1):
        k.box(body, (s_*(sx-.035), sy+.022, 0), (.090, .018, .150), 'Gold', axes=((1, -s_*.35, 0), (s_*.35, 1, 0), (0, 0, 1)))
        for i in range(4): k.box(body, (s_*(sx-.005), sy-.004-.004*i, -.045+.03*i), (.012, .030, .010), 'Gold')
    top = fp.MALE_TORSO[-1]
    k.vertical_loft(body, [(top[0]-.016, top[1]+.014, top[2]+.014, top[3]+.014), (top[0]+.034, top[1]+.002, top[2]+.004, top[3]+.004)], 'Silk', a=.70, b=.62, cap_start=False, cap_end=False)
    k.box(body, (-.020, top[0]-.060, fp.torso_front(False, -.02, top[0]-.06)+.016), (.050, .090, .012), 'Silk', taper=.5, axes=((1, 0, 0), (-.25, -1, 0), (0, 0, 1)))
    # Brass balloon emblem on the chest: envelope over a small basket.
    ex, ey = -.105, .300; ez = fp.torso_front(False, ex, ey)+.010
    k.box(body, (ex, ey+.012, ez), (.030, .034, .008), 'Brass', taper=.7)
    k.box(body, (ex, ey-.016, ez), (.012, .010, .008), 'Gold')
    head = k.Mesh('BalloonMerchantHead', pal)
    head_base(head, False, 'SkinH', 'HairAuburn', 'Crop')
    # Leather aviator cap with ear flaps and a chin strap.
    # Front edge sits on the forehead above the brows; the back and sides come lower.
    rows = []
    for y in (.130, .170, .200, .240):
        w_, f_, bk_ = fp.section_at(fp.MALE_HEAD, y)
        rows.append((y, w_+.014, f_+.016 if y >= .200 else f_-.030, bk_+.016))
    k.vertical_loft(head, rows + [(.280, .068, .080, .088), (.298, .030, .036, .040)], 'Leather', a=.60, b=.55, cap_start=False)
    w = fp.section_at(fp.MALE_HEAD, .12)[0]
    for s_ in (-1, 1):
        k.box(head, (s_*(w+.014), .118, -.010), (.014, .070, .056), 'LeatherDark', taper=.85)
        k.box(head, (s_*(w+.004), .050, .030), (.008, .070, .012), 'LeatherDark', axes=((1, 0, 0), (0, 1, -.6), (0, .6, 1)))
    # Brass goggles pushed up onto the cap, with their strap.
    f = fp.section_at(fp.MALE_HEAD, .23)[1]
    k.vertical_loft(head, [head_ring(False, .222, .020), head_ring(False, .238, .020)], 'LeatherDark', a=.60, b=.55, cap_start=False, cap_end=False)
    for s_ in (-1, 1):
        c = (s_*.034, .232, f+.032)
        k.limb(head, [(k.add(c, (0, 0, -.012)), .022, .022), (k.add(c, (0, 0, .010)), .022, .022)], 'Brass', sides=8, forward_hint=(0, 1, 0))
        k.box(head, k.add(c, (0, 0, .011)), (.030, .030, .004), 'Glass', shade=4)
    k.box(head, (0, .232, f+.028), (.020, .008, .012), 'Brass')
    # Handlebar moustache with upturned ends.
    my = .093; mz = fp.front_z(fp.MALE_HEAD, 0, my)+.005
    for s_ in (-1, 1):
        k.box(head, (s_*.020, my, mz), (.038, .010, .009), 'HairAuburn', axes=((1, -s_*.08, 0), (s_*.08, 1, 0), (0, 0, 1)), taper=.85)
        k.box(head, (s_*.044, my+.005, mz-.003), (.012, .010, .007), 'HairAuburn', axes=((1, -s_*.6, 0), (s_*.6, 1, 0), (0, 0, 1)), taper=.5)
    return body, head

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    pal.write_png(OUT/'ShopkeeperPalette.png')
    out = []
    for kp in KEEPERS:
        body, head = kp['Build']()
        sb, sh = body.save(OUT, HEADER), head.save(OUT, HEADER)
        out.append(dict(Name=kp['Name'], Shop=kp['Shop'], Body=sb['name'], Head=sh['name'], Female=kp['Female'], Triangles=sb['triangles']+sh['triangles'], Notes=kp['Notes']))
    manifest = dict(Revision=1, Style='faceted', TorsoPivot=fp.HIPS_Y+fp.TORSO_UP, HeadUp=fp.HEAD_UP, CounterTop=1.02, Keepers=out,
                    Scope='Art only: static waist-up shopkeepers per storefront. No shop logic, dialogue, stock or animation.')
    (OUT/'ShopkeeperManifest.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
    print('Shopkeepers:', ', '.join(f"{o['Name']} {o['Triangles']}" for o in out))

if __name__ == '__main__':
    main()
