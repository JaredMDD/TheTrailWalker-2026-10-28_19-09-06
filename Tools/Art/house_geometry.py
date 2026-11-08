"""Shared faceted house kit. +Y up, +Z street-facing; real meshes, no painted concepts."""
import math
import faceted_character_kit as k


def box(m, c, size, color, **kwargs):
    return k.box(m, c, size, color, **kwargs)


def beam(m, a, b, width, color, depth=None):
    axis = k.unit(k.sub(b, a))
    across = k.unit(k.cross(axis, (0, 0, 1)))
    box(m, k.lerp(a, b, .5), (width, k.length(k.sub(b, a)), depth or width), color,
        axes=(across, axis, (0, 0, 1)))


def wall(m, x0, x1, y0, y1, z, thick, openings, color):
    xs = sorted({x0, x1} | {v for o in openings for v in o[:2]})
    ys = sorted({y0, y1} | {v for o in openings for v in o[2:]})
    for a, b in zip(xs, xs[1:]):
        for c, d in zip(ys, ys[1:]):
            if a < x0 or b > x1 or c < y0 or d > y1: continue
            if any(o[0] <= (a+b)/2 <= o[1] and o[2] <= (c+d)/2 <= o[3] for o in openings): continue
            box(m, ((a+b)/2, (c+d)/2, z), (b-a, d-c, thick), color, shade=2)


def window(m, x, y, w, h, z, wood, shutters=True, fine=False):
    box(m, (x, y, z-.03), (w, h, .05), 'GlassDark', shade=2)
    # Separate pane colours read as glazing without transparent sorting.
    for sx in (-1, 1):
        for sy in (-1, 1):
            box(m, (x+sx*w*.24, y+sy*h*.24, z), (w*.44, h*.44, .025),
                'GlassLight' if sx == -sy else 'Glass', shade=2)
    for xx in (x-w/2, x+w/2): box(m, (xx, y, z+.035), (.085, h+.14, .10), wood)
    for yy in (y-h/2, y+h/2): box(m, (x, yy, z+.035), (w+.15, .085, .10), wood)
    box(m, (x, y, z+.055), (.04, h, .06), wood)
    box(m, (x, y, z+.055), (w, .04, .06), wood)
    box(m, (x, y-h/2-.09, z+.09), (w+.25, .09, .22), 'Marble' if fine else wood)
    if shutters:
        for sign in (-1, 1):
            xx=x+sign*(w*.75+.14)
            box(m, (xx,y,z+.01), (w*.42,h,.055), 'Green')
            for yy in (-h*.32,h*.32): box(m, (xx,y+yy,z+.05), (w*.38,.055,.045), wood)


def door(m, x, bottom, z, wood, level, height=1.92, width=.98):
    for i in range(6):
        box(m,(x-width/2+width*(i+.5)/6,bottom+height/2,z), (width/6-.012,height,.085), wood)
    metal='Steel' if level == 8 else 'Copper' if level == 4 else 'Iron'
    for y in (.34, height-.34): box(m,(x,bottom+y,z+.065), (width,.065,.035),metal)
    for xx in (x-width/2-.08,x+width/2+.08): box(m,(xx,bottom+height/2,z+.04),(.13,height+.18,.15),wood)
    box(m,(x,bottom+height+.08,z+.04),(width+.29,.14,.15),wood)
    box(m,(x-width*.30,bottom+height*.48,z+.11),(.055,.075,.055),metal)


def roof(m, width, depth, eaves, peak, front, color, wood, tiled=True):
    # Gable ridge runs away from the street; both slopes have full back faces.
    for sign in (-1,1):
        slope=k.unit((sign*width/2,eaves-peak,0))
        normal=(-slope[1]*sign,slope[0]*sign,0)
        length=math.hypot(width/2,peak-eaves)
        centre=(sign*width/4,(eaves+peak)/2,front-depth/2)
        axes=(slope,normal,(0,0,1))
        box(m,centre,(length,.10,depth),color,axes=axes,shade=1)
        if tiled:
            cols=max(8,round(depth/.55))   # long (deep) roofs keep the same tile size
            for row in range(5):
                for col in range(cols):
                    p=k.add((0,peak,front-depth*(col+.5)/cols),k.mul(slope,length*(row+.5)/5))
                    box(m,k.add(p,k.mul(normal,.065)),(length/5+.035,.045,depth/cols-.018),color,
                        axes=axes,shade=1+(row+col)%3)
        for zz in (front+.025,front-depth-.025):
            beam(m,(0,peak+.035,zz),(sign*width/2,eaves+.035,zz),.115,wood)
        box(m,(sign*width/2,eaves,front-depth/2),(.12,.14,depth+.08),wood)
    box(m,(0,peak+.075,front-depth/2),(.18,.13,depth+.12),color)


def gable(m, half, bottom, peak, z, color, depth=.15):
    vertices=[m.v(p) for p in [(-half,bottom,z+depth/2),(half,bottom,z+depth/2),(0,peak,z+depth/2),
                              (-half,bottom,z-depth/2),(half,bottom,z-depth/2),(0,peak,z-depth/2)]]
    inside=(0,bottom+(peak-bottom)*.35,z)
    m.tri(*vertices[:3],color,inside,shade=2)
    m.tri(*vertices[3:],color,inside,shade=2)
    for ids in ((0,1,4,3),(1,2,5,4),(2,0,3,5)):
        m.quad(*(vertices[i] for i in ids),color,inside,shade=2)


def dormer(m,x,y,w,h,z,wood,roof_color):
    # A shallow projecting gabled bay with its own roof and glazed face.
    wall(m,x-w/2,x+w/2,y,y+h*.68,z,.16,[], 'Plaster')
    gable(m,w/2,y+h*.68,y+h,z,'Plaster') if x == 0 else None
    if x != 0:
        start=len(m.vertices); gable(m,w/2,y+h*.68,y+h,z,'Plaster');m.transform(lambda p:(p[0]+x,p[1],p[2]),start)
    for sign in (-1,1): box(m,(x+sign*w/2,y+h*.34,z+.06),(.10,h*.68,.12),wood)
    start=len(m.vertices)
    roof(m,w+.15,.72,y+h*.68,y+h,z+.13,roof_color,wood,False)
    m.transform(lambda p:(p[0]+x,p[1],p[2]),start)
    window(m,x,y+h*.36,w*.63,h*.44,z+.11,wood,False)


def chest(m,c,w=.8,h=.55,d=.45,wood='Pine',metal='Iron'):
    x,y,z=c
    box(m,(x,y+h/2,z),(w,h,d),wood)
    box(m,(x,y+h+.025,z),(w+.045,.08,d+.045),wood)
    for sx in (-.32,.32):
        box(m,(x+sx*w,y+h*.52,z+d/2+.025),(.06,h,.035),metal)
        box(m,(x+sx*w,y+h+.075,z),(.06,.025,d+.045),metal)
    box(m,(x,y+h*.67,z+d/2+.045),(.10,.13,.04),metal)


def table(m,c,wood,w=1.0,h=.58,d=.42):
    x,y,z=c
    box(m,(x,y+h,z),(w,.09,d),wood)
    for sx in (-1,1):
        for sz in (-1,1): box(m,(x+sx*w*.40,y+h*.48,z+sz*d*.34),(.065,h,.065),wood)
    box(m,(x,y+h*.22,z),(w*.86,.06,.05),wood)


def side(m, sign, half, build):
    """Builds a +Z-facing element with build(m) and turns it onto the side wall at x=sign*half, facing outward.
    In build, x is minus the depth position for sign=+1 and the depth position for sign=-1 (see along()), z is
    the offset from the wall plane. Proper rotations only, so face winding stays outward."""
    start=len(m.vertices); build(m)
    if sign>0: m.transform(lambda p:(p[2]+half,p[1],-p[0]),start)
    else: m.transform(lambda p:(-p[2]-half,p[1],p[0]),start)


def along(sign, z):
    """Build-space x for depth position z on the given side (inverse of side()'s rotation)."""
    return -z if sign>0 else z


def side_window_positions(depth, level):
    """Depth positions (negative z) of the side windows: one per ~1.7 m of wall from level 2."""
    if level<2: return []
    n=max(1,int((depth-1.2)/1.7))
    return [-depth*(i+.5)/n for i in range(n)]
