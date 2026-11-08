"""Shared faceted character parts (approved style, user, November 2, 2026). Python standard library only.

One skeleton for every character (player, camp NPCs, mission lady, ferryman). Each part is built in its rig frame:
  head frame   origin at the HeadPivot / HeadAnchor (neck top); chin about 2 cm above it, crown about 0.27 m.
  torso frame  origin at the TorsoPivot (waist), shoulders at (+-SHOULDER_X, SHOULDER_Y).
  hips frame   origin at the hip-joint height; hip joints at (+-HIP_X, 0).
  limb frames  origin at the upper joint, the limb hanging along -Y (shoulder, elbow, wrist, hip, knee).
  ankle frame  origin at the ankle; sole at -ANKLE_HEIGHT, toes toward +Z.
Standing: hips pivot 0.88 m, torso pivot 0.94 m, head pivot 1.43 m. Colours are logical names that each pipeline
maps to its own palette slots or materials (see SlotPalette / MaterialPalette).
"""
import math
import faceted_character_kit as k

HIPS_Y, TORSO_UP, HEAD_UP = .88, .06, .49
UPPER_ARM, FOREARM, THIGH, SHIN, ANKLE_HEIGHT = .29, .25, .38, .40, .10
HIP_X = .100
SHOULDER = {False: (.205, .415), True: (.188, .400)}   # (x, y) in the torso frame

class SlotPalette:
    """Maps logical colour names to fixed palette slots (palette-strip pipelines)."""
    def __init__(self, slots, mapping):
        self.slots, self.mapping = slots, mapping
    def index(self, name, shade=2):
        v = self.mapping[name]
        return v[min(shade, len(v)-1)] if isinstance(v, (list, tuple)) else v

class MaterialPalette:
    """Maps logical colour names to material names (material-group pipelines)."""
    def __init__(self, mapping):
        self.mapping = mapping; self.names = sorted(set(mapping.values()))
        self.slots = len(self.names)
    def index(self, name, shade=2): return self.names.index(self.mapping[name])

def save_groups(mesh, folder, header, mtllib):
    """Writes faces grouped by material (contiguous, first-appearance order); returns that material order."""
    pal = mesh.palette
    order = list(dict.fromkeys(pal.names[f[3]] for f in mesh.faces))
    lines = ['# '+header, '# Faceted characters: flat normals (s off), material groups.', 'mtllib '+mtllib, 'o '+mesh.name]
    lines += ['v %.6f %.6f %.6f' % p for p in mesh.vertices]
    lines += ['s off']
    for material in order:
        lines.append('usemtl '+material)
        lines += ['f %d %d %d' % (a+1, b+1, c+1) for a, b, c, kk in mesh.faces if pal.names[kk] == material]
    (folder/(mesh.name+'.obj')).write_text('\n'.join(lines)+'\n', encoding='utf-8')
    return order

def transformed(mesh, start, fn):
    for i in range(start, len(mesh.vertices)): mesh.vertices[i] = fn(mesh.vertices[i])

def place(mesh, build, fn):
    """Runs build(mesh) and moves only the geometry it added with fn(point)."""
    start = len(mesh.vertices); build(mesh); transformed(mesh, start, fn)

def rot_x(p, deg):
    a = math.radians(deg); c, s = math.cos(a), math.sin(a)
    return (p[0], p[1]*c - p[2]*s, p[1]*s + p[2]*c)
def rot_z(p, deg):
    a = math.radians(deg); c, s = math.cos(a), math.sin(a)
    return (p[0]*c - p[1]*s, p[0]*s + p[1]*c, p[2])

# ------------------------------------------------------------------ head

MALE_HEAD = [(0.020, .034, .093, -.050), (0.038, .060, .099, -.005), (0.062, .074, .102, .055), (0.092, .077, .106, .085),
             (0.120, .079, .108, .100), (0.150, .080, .106, .106), (0.178, .081, .104, .108), (0.215, .078, .094, .106),
             (0.248, .066, .074, .094), (0.270, .040, .044, .062)]
# Softer, narrower jaw and a slightly smaller skull for the female head.
FEMALE_HEAD = [(0.026, .030, .089, -.048), (0.044, .052, .095, -.004), (0.068, .065, .098, .052), (0.095, .072, .101, .082),
               (0.122, .075, .103, .096), (0.150, .076, .102, .101), (0.176, .077, .100, .103), (0.210, .074, .090, .101),
               (0.241, .063, .071, .090), (0.262, .038, .042, .059)]

def head_sections(female): return FEMALE_HEAD if female else MALE_HEAD

def section_at(sections, y):
    """Interpolated (w, front, back) of a vertical section list at height y."""
    if y <= sections[0][0]: return sections[0][1:4]
    for lo, hi in zip(sections, sections[1:]):
        if lo[0] <= y <= hi[0]:
            t = (y-lo[0])/(hi[0]-lo[0]); return tuple(lo[i]+(hi[i]-lo[i])*t for i in (1, 2, 3))
    return sections[-1][1:4]

def front_z(sections, x, y, a=.62, b=.55):
    """Front surface depth of a chamfered section stack at (x, y)."""
    w, f, _ = section_at(sections, y)
    ax = abs(x)
    if ax <= w*a: return f
    t = min(1, (ax - w*a)/(w*(1-a)))
    return f + (f*b - f)*t

def head(m, female, skin='Skin', shade='SkinShade', lips='Lips'):
    """Head shell, ears, nose and mouth. Eyes and brows are separate parts (eyes(), brows())."""
    sections = head_sections(female)
    k.vertical_loft(m, sections, skin, a=.62, b=.55)
    ear_y = .138 if female else .136
    for s in (-1, 1):
        w = section_at(sections, ear_y)[0]
        k.box(m, (s*(w+.001), ear_y, -.004), (.015, .050 if female else .056, .038), shade, taper=.85)
        k.box(m, (s*(w+.007), ear_y+.002, -.002), (.006, .032, .022), skin)
    ns = .85 if female else 1.0
    fz = lambda y: front_z(sections, 0, y)
    top, bridge = .170 if not female else .166, .150 if not female else .147
    tip_y, base_y = .112 if not female else .116, .102 if not female else .107
    T = m.v((0, top, fz(top)-.002)); BL = m.v((-.011*ns, bridge, fz(bridge)+.001)); BR = m.v((.011*ns, bridge, fz(bridge)+.001))
    P = m.v((0, tip_y, fz(tip_y)+.032*ns)); L = m.v((-.018*ns, base_y, fz(base_y)+.004)); R = m.v((.018*ns, base_y, fz(base_y)+.004))
    B = m.v((0, base_y-.003, fz(base_y)+.020*ns))
    inside = (0, (top+base_y)/2, fz(.13)-.01)
    for f in ((T, BL, P), (T, P, BR), (BL, L, P), (BR, P, R), (L, B, P), (R, P, B), (L, R, B)): m.tri(*f, skin, inside)
    my = .081 if not female else .085
    if female:
        k.box(m, (0, my+.002, fz(my)+.0015), (.036, .008, .006), lips, shade=2)
        k.box(m, (0, my-.007, fz(my)+.001), (.030, .009, .006), lips, shade=3)
    else:
        k.box(m, (0, my, fz(my)+.0015), (.040, .006, .006), lips, shade=1)
        k.box(m, (0, my-.008, fz(my)+.0005), (.030, .006, .006), lips, shade=3)

def eyes(m, female, color='Eye', lash='Eye'):
    sections = head_sections(female)
    y = .148 if female else .146
    for s in (-1, 1):
        x = s*(.033 if female else .034)
        z = front_z(sections, x, y)
        k.box(m, (x, y, z+.001), (.019, .011 if female else .009, .006), color, shade=2)
        if female:
            # Upper lash line extends past the outer corner.
            k.box(m, (x+s*.003, y+.0065, z+.0015), (.024, .003, .006), lash, axes=((1, -s*.12, 0), (s*.12, 1, 0), (0, 0, 1)), shade=1)

def brows(m, female, color='Hair'):
    sections = head_sections(female)
    for s in (-1, 1):
        x, y = s*.035, (.174 if female else .170)
        z = front_z(sections, x, y)
        tilt = .10 if female else .18
        k.box(m, (x, y, z+.003), (.040, .008 if female else .012, .010), color,
              axes=((1, -s*tilt, 0), (s*tilt, 1, 0), (0, 0, 1)), shade=1)

def beard(m, color='Hair', dark='HairDark'):
    """Full angular beard over jaw and chin, hugging the face (no floating gap), with a moustache and open mouth."""
    base = MALE_HEAD
    sections = [(.012, .030, .095, -.056), (.028, .062, .102, -.002), (.060, .077, .105, .057), (.092, .080, .108, .087),
                (.118, .082, .110, .102), (.158, .083, .108, .108)]
    rings = []
    for y, w, f, bk in sections:
        c = (0, y, 0); rings.append((c, k.ring_points(c, (1, 0, 0), (0, 0, 1), k.chamfer_rect(w, f, bk, .62, .55))))
    ids = [[m.v(p) for p in pts] for _, pts in rings]
    keep = {0: (7, 0, 6, 1, 5), 1: (7, 0, 6, 1, 5), 2: (0, 6, 1, 5), 3: (1, 5), 4: (1, 5)}
    for band, faces in keep.items():
        inside = k.lerp(rings[band][0], rings[band+1][0], .5)
        for i in faces:
            j = (i+1) % 8
            m.quad(ids[band][i], ids[band][j], ids[band+1][j], ids[band+1][i], color, inside)
    cap = m.v(rings[0][0])
    for i in (7, 0, 6): m.tri(cap, ids[0][i], ids[0][(i+1) % 8], dark, rings[1][0])
    fz = lambda y: front_z(base, 0, y)
    k.box(m, (0, .091, fz(.091)+.004), (.050, .011, .008), color, taper=.8)
    for s in (-1, 1): k.box(m, (s*.029, .079, fz(.079)+.002), (.012, .024, .008), color, shade=1)
    k.box(m, (0, .022, fz(.03)+.004), (.032, .018, .012), dark, taper=.7)

# ------------------------------------------------------------------ hair

def lock(m, start, end, width, thick, color, lift=0.0, shade=None):
    along = k.unit(k.sub(end, start)); up = k.unit(k.sub((0, 1, 0), k.mul(along, k.dot((0, 1, 0), along))))
    if abs(along[1]) > .9: up = (0, 0, 1)
    side = k.cross(along, up)
    center = k.add(k.lerp(start, end, .5), k.mul(up, lift))
    k.box(m, center, (width, k.length(k.sub(end, start)), thick), color, axes=(side, along, up), taper=.55, shade=shade)

def hair_shell(m, female, gap, hairline, nape, color, sides_down=None):
    """Angular cap following the head: visible above the hairline in front, down to the nape at the back."""
    sec = head_sections(female); top = sec[-1][0]
    rows = []
    start = nape
    ys = [start] + [s[0] for s in sec if s[0] > start+.01]
    for y in ys:
        w, f, bk = section_at(sec, y)
        front = f+gap if y >= hairline else f-.03
        side = w+gap if (sides_down is None or y >= sides_down) else w-.01
        rows.append((y+(.004 if y == ys[-1] else 0), side, front, bk+gap))
    rows.append((top+gap+.012, sec[-1][1]*.55, sec[-1][2]*.55, sec[-1][3]*.6))
    k.vertical_loft(m, rows, color, a=.60, b=.55, cap_start=False)

def hair(m, female, style, color='Hair', dark='HairDark'):
    sy = (FEMALE_HEAD if female else MALE_HEAD)[-1][0]/MALE_HEAD[-1][0]
    Y = lambda y: y*sy
    if style == 'Crop':
        hair_shell(m, female, .006, Y(.205), Y(.105), color, sides_down=Y(.150))
        for x in (-.04, -.012, .016, .044):
            lock(m, (x, Y(.226), .090), (x+.006, Y(.262), .010), .034, .010, color, .002)
        for s in (-1, 1): k.box(m, (s*.0805, Y(.153), .036), (.006, .028, .014), color, taper=.7)
    elif style == 'Sweep':
        hair_shell(m, female, .008, Y(.205), Y(.095), color, sides_down=Y(.140))
        for x, lift in [(-.046, .004), (-.016, .010), (.016, .012), (.046, .006)]:
            lock(m, (x, Y(.222), .098), (x+.018, Y(.268), -.010), .040, .018, color, lift)
        for x in (-.035, .000, .035):
            lock(m, (x, Y(.272), .010), (x+.012, Y(.246), -.085), .042, .016, dark if x == 0 else color, .006)
        lock(m, (-.030, Y(.218), .104), (.040, Y(.256), .088), .036, .022, color, .012, shade=3)
        for s in (-1, 1): k.box(m, (s*.0815, Y(.152), .038), (.006, .030, .016), color, taper=.7)
    elif style == 'Bob':
        hair_shell(m, female, .009, Y(.200), Y(.060), color)
        sec = head_sections(female)
        for s in (-1, 1):
            # Side panels to the jaw, covering the ears, cut square at the bottom.
            # Side panels hug the head down to the jaw line, covering the ears.
            w = section_at(sec, Y(.13))[0]
            k.box(m, (s*(w+.011), Y(.135), -.016), (.012, Y(.140), .120), color, taper=1.04)
            k.box(m, (s*(w+.009), Y(.165), .050), (.010, Y(.075), .026), dark, taper=.8)
        k.box(m, (0, Y(.105), -.110), (.140, Y(.110), .018), color)
        # Straight fringe over the brow, in two angled planes.
        for s in (-1, 1):
            lock(m, (s*.062, Y(.232), .084), (s*.006, Y(.200), .106), .060, .016, color, .004)
    elif style == 'Tied':
        hair_shell(m, female, .006, Y(.208), Y(.100), color, sides_down=Y(.150))
        for x in (-.03, .0, .03):
            lock(m, (x, Y(.230), .090), (x*.6, Y(.262), -.040), .040, .010, color, .002)
        # Band and an angular ponytail hanging down the back.
        k.box(m, (0, Y(.176), -.118), (.040, .024, .022), dark)
        k.limb(m, [((0, Y(.182), -.122), .022, .018), ((0, Y(.150), -.146), .026, .022), ((0, Y(.100), -.156), .023, .019),
                   ((0, Y(.060), -.150), .015, .013), ((0, Y(.035), -.142), .006, .006)], color, sides=6, turn=0,
               forward_hint=(0, 0, 1))
    else:
        raise ValueError(style)

HAIR_STYLES = ('Crop', 'Sweep', 'Bob', 'Tied')

# ------------------------------------------------------------------ torso and hips

MALE_TORSO = [(-.080, .192, .124, .122), (-.062, .191, .124, .121), (.020, .188, .121, .117), (.110, .180, .120, .112),
              (.200, .178, .125, .109), (.290, .187, .131, .111), (.360, .198, .125, .111), (.400, .204, .112, .104),
              (.430, .196, .096, .094), (.455, .150, .080, .079), (.478, .074, .064, .060)]
FEMALE_TORSO = [(-.080, .202, .126, .132), (-.062, .201, .126, .131), (.020, .186, .118, .120), (.110, .160, .110, .104),
                (.200, .162, .120, .104), (.260, .172, .136, .106), (.320, .180, .133, .106), (.370, .187, .112, .100),
                (.405, .181, .094, .090), (.440, .138, .074, .074), (.465, .068, .060, .056)]
def torso_sections(female, bulk=1.0):
    return [(y, w*bulk, f*bulk, b*bulk) for y, w, f, b in (FEMALE_TORSO if female else MALE_TORSO)]

def torso(m, female, cloth='Cloth', hem='ClothDark', collar='ClothDark', bulk=1.0, hem_band=True):
    sec = torso_sections(female, bulk)
    colors = ([hem] if hem_band else [cloth]) + [cloth]*(len(sec)-2)
    k.vertical_loft(m, sec, cloth, a=.70, b=.62, cap_end=False, colors=colors)
    top = sec[-1]
    k.vertical_loft(m, [(top[0]-.008, top[1]+.004, top[2]+.004, top[3]+.004), (top[0]+.014, top[1]-.006, top[2]-.004, top[3]-.004)],
                    collar, a=.70, b=.62, cap_start=False, cap_end=False)

def torso_front(female, x, y, bulk=1.0): return front_z(torso_sections(female, bulk), x, y, .70, .62)
def torso_back(female, y, bulk=1.0): return section_at(torso_sections(female, bulk), y)[2]

HIPS = {False: [(-.080, .165, .100, .106), (0.000, .182, .110, .119), (.080, .176, .108, .112), (.140, .166, .105, .105), (.170, .162, .104, .102)],
        True: [(-.080, .172, .104, .112), (0.000, .190, .114, .124), (.080, .178, .108, .112), (.140, .160, .102, .102), (.165, .156, .100, .100)]}

def hips(m, female, cloth='Pants', band='PantsDark'):
    sec = HIPS[female]
    k.vertical_loft(m, sec, cloth, a=.72, b=.62, colors=[cloth, cloth, cloth, band])

# ------------------------------------------------------------------ limbs (rig frames, hanging along -Y)

def sleeve(m, length, cloth='Cloth', cuff='ClothDark', rolled=False, radius=1.0):
    r = radius
    rings = [((0, .012, 0), .048*r, .052*r), ((0, -.020, 0), .062*r, .064*r), ((0, -.080, 0), .061*r, .063*r),
             ((0, -(length-.030), 0), .057*r, .059*r), ((0, -(length-.022), 0), .060*r, .062*r)]
    colors = [cloth, cloth, cloth, cuff]
    if rolled:
        rings += [((0, -(length-.002), 0), .063*r, .065*r), ((0, -length, 0), .052*r, .054*r)]
        colors += [cuff, cuff]
    else:
        rings += [((0, -length, 0), .052*r, .054*r)]
        colors += [cuff]
    k.limb(m, rings, cloth, sides=8, turn=math.pi/8, colors=colors)

def upper_arm_skin(m, start=.10, skin='Skin'):
    L = UPPER_ARM
    k.limb(m, [((0, -start, 0), .046, .048), ((0, -.20, 0), .043, .045), ((0, -L, 0), .039, .041), ((0, -L-.02, 0), .036, .038)], skin, sides=6)

def forearm(m, skin='Skin', sleeve_color=None, cuff=None):
    """Elbow frame. With sleeve_color the forearm is covered (long sleeves) and ends in a cuff."""
    L = FOREARM
    joints = [((0, .025, 0), .040, .042), ((0, 0, 0), .040, .042), ((0, -.06, 0), .044, .046), ((0, -.16, 0), .037, .035), ((0, -L, 0), .028, .024)]
    if sleeve_color:
        joints = [((0, .03, 0), .050, .052), ((0, 0, 0), .052, .054), ((0, -.07, 0), .054, .056), ((0, -.18, 0), .047, .046),
                  ((0, -L+.02, 0), .044, .043), ((0, -L+.015, 0), .048, .047), ((0, -L+.002, 0), .048, .047), ((0, -L+.002, 0), .030, .028)]
        k.limb(m, joints, sleeve_color, sides=8, turn=math.pi/8,
               colors=[sleeve_color]*4 + [cuff or sleeve_color]*3)
    else:
        k.limb(m, joints, skin, sides=6)

def hand(m, skin='Skin', shade='SkinShade'):
    """Wrist frame: fingers along -Y curling toward -Z (palm side), thumb on +X (left hand; mirror for the right)."""
    k.limb(m, [((0, .012, 0), .030, .020), ((0, -.035, 0), .044, .023), ((0, -.080, 0), .045, .020)], skin, sides=6, turn=0)
    k.limb(m, [((0, -.078, -.002), .043, .018), ((0, -.115, -.010), .040, .016), ((0, -.150, -.024), .034, .013)], skin, sides=6, turn=0,
           colors=[skin, lambda i: shade if i in (4, 5) else skin])
    k.limb(m, [((.030, -.020, -.006), .012, .013), ((.048, -.055, -.016), .011, .012), ((.052, -.080, -.024), .009, .010)], skin, sides=5)

def thigh(m, cloth='Pants', female=False):
    g = 1.03 if female else 1.0
    k.limb(m, [((0, .040, 0), .092*g, .097*g), ((0, -.020, 0), .094*g, .099*g), ((0, -.160, .004), .086*g, .090*g),
               ((0, -.300, .008), .070, .075), ((0, -THIGH, .012), .064, .068), ((0, -THIGH-.03, .010), .060, .064)], cloth, sides=8, turn=math.pi/8)

def shin(m, cloth='Pants', hem='PantsDark', bare_ankle=False):
    L = SHIN
    joints = [((0, .035, .012), .064, .068), ((0, 0, .012), .064, .068), ((0, -.100, .002), .067, .073), ((0, -.250, -.004), .055, .059),
              ((0, -.360, -.006), .050, .054), ((0, -.382, -.006), .058, .062), ((0, -.408, -.006), .058, .062), ((0, -.416, -.006), .050, .054)]
    k.limb(m, joints, cloth, sides=8, turn=math.pi/8, colors=[cloth]*5 + [hem]*2)

def bare_foot(m, skin='Skin', shade='SkinShade'):
    rings = []
    h = ANKLE_HEIGHT
    for z, w, top, bottom, cy in [(-.055, .034, .030, .030, .032), (-.012, .042, .062, .036, .036), (.060, .046, .026, .030, .030),
                                  (.140, .048, .014, .018, .018), (.178, .040, .008, .012, .012)]:
        c = (0, cy-h, z)
        rings.append((c, k.ring_points(c, (1, 0, 0), (0, 1, 0), k.chamfer_rect(w, top, bottom, .7, .6))))
    k.loft(m, rings, skin)
    for i, tx in enumerate((-.030, -.012, .008, .026)):
        k.box(m, (tx, .010-h, .172-.006*abs(i-1.5)), (.014, .016, .016), shade if i == 0 else skin)

def boot(m, leather='Boot', sole='Sole', cuff='LeatherDark', lace='Metal', tall=.10):
    h = ANKLE_HEIGHT
    # Shaft around the ankle up to the trouser hem, with a turned cuff.
    k.vertical_loft(m, [(-.035, .052, .060, .058), (tall-.02, .056, .062, .060)], leather, a=.70, b=.62, cap_end=False)
    k.vertical_loft(m, [(tall-.024, .060, .066, .064), (tall, .060, .066, .064)], cuff, a=.70, b=.62, cap_start=False, cap_end=False)
    rings = []
    for z, w, top, bottom, cy in [(-.070, .046, .040, .034, .045), (-.020, .054, .075, .040, .045), (.060, .058, .035, .036, .040),
                                  (.150, .056, .022, .026, .028), (.196, .046, .014, .018, .020)]:
        c = (0, cy-h, z)
        rings.append((c, k.ring_points(c, (1, 0, 0), (0, 1, 0), k.chamfer_rect(w, top, bottom, .7, .6))))
    k.loft(m, rings, leather)
    k.box(m, (0, .009-h, .062), (.122, .018, .282), sole)
    k.box(m, (0, .022-h, -.062), (.100, .010, .070), sole)
    for i, z in enumerate((.020, .050, .080)):
        k.box(m, (0, .068-h-.012*i, z+.010), (.050, .007, .008), lace)

# ------------------------------------------------------------------ garments shared by special NPCs and shopkeepers

def open_shell(m, rows, color, inner, opening_deg, edge=None, edge_size=.012, segments=12):
    """Garment shell open at the front (+Z): rows (y, w, front, back), double sided, squared to clear the torso."""
    half = math.radians(opening_deg/2); a0 = math.pi/2 + half; span = math.tau - 2*half
    rings = []
    for y, w, f, bk in rows:
        ring = []
        for i in range(segments+1):
            a = a0 + i*span/segments; c, sn = math.cos(a), math.sin(a)
            ring.append(m.v((w*math.copysign(abs(c)**.55, c), y, (f if sn > 0 else bk)*math.copysign(abs(sn)**.55, sn))))
        rings.append(ring)
    inner_index = m.palette.index(inner)
    for r in range(len(rings)-1):
        for i in range(segments):
            q = (rings[r][i], rings[r][i+1], rings[r+1][i+1], rings[r+1][i])
            m.quad(*q, color, (0, (rows[r][0]+rows[r+1][0])/2, 0))
            m.faces.append((q[0], q[2], q[1], inner_index)); m.faces.append((q[0], q[3], q[2], inner_index))
    if edge:
        for e in (0, segments):
            for r in range(len(rings)-1):
                a, c = m.vertices[rings[r][e]], m.vertices[rings[r+1][e]]
                along = k.unit(k.sub(c, a))
                k.box(m, k.lerp(a, c, .5), (edge_size, k.length(k.sub(c, a))+.004, edge_size), edge,
                      axes=((1, 0, 0), along, k.unit(k.cross((1, 0, 0), along))))

def brim(m, y, rx, rz, inner, color, under, z_offset=-.010, droop=0.0, sides=16, thickness=.010):
    """Flat hat brim ring between an inner radius and (rx, rz); droop lowers the outer edge front and back."""
    top, low, itop, ilow = [], [], [], []
    for i in range(sides):
        a = i*math.tau/sides; c, s = math.cos(a), math.sin(a)
        oy = y - droop*abs(s)
        top.append(m.v((rx*c, oy, rz*s+z_offset))); low.append(m.v((rx*c, oy-thickness, rz*s+z_offset)))
        itop.append(m.v((inner*c, y+.004, inner*s+z_offset))); ilow.append(m.v((inner*c, y-thickness, inner*s+z_offset)))
    for i in range(sides):
        j = (i+1) % sides
        m.quad(itop[i], itop[j], top[j], top[i], color, (0, y-.5, z_offset))
        m.quad(ilow[i], low[i], low[j], ilow[j], under, (0, y+.5, z_offset))
        m.quad(top[i], top[j], low[j], low[i], color, (0, y, z_offset))
