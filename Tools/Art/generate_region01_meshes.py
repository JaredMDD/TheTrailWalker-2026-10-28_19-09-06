"""Deterministic source meshes. Run from any directory with Python 3 (no dependencies)."""
from pathlib import Path
import math
import random
import struct
import zlib

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Assets/_Game/Art/Items/Region01'
COLORS = [
    (91, 56, 33), (121, 76, 42), (150, 100, 57), (207, 166, 104),
    (232, 210, 165), (199, 163, 115), (165, 125, 83), (133, 94, 61),
    (176, 41, 36), (203, 52, 39), (227, 70, 42), (238, 100, 48),
    (104, 132, 51), (131, 154, 63), (89, 112, 43), (244, 185, 80),
]

def add(a, b): return tuple(x + y for x, y in zip(a, b))
def sub(a, b): return tuple(x - y for x, y in zip(a, b))
def mul(a, n): return tuple(x * n for x in a)
def cross(a, b): return (a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0])
def unit(a): return mul(a, 1 / math.sqrt(sum(x*x for x in a)))

class Model:
    def __init__(self, name): self.name, self.vertices, self.faces = name, [], []
    def v(self, p): self.vertices.append(p); return len(self.vertices)
    def tri(self, a, b, c, color): self.faces.append((a, b, c, color))
    def fit_bounds(self, lower, upper):
        """Refine a silhouette inside its approved authored envelope."""
        lo=[min(p[i] for p in self.vertices) for i in range(3)]
        hi=[max(p[i] for p in self.vertices) for i in range(3)]
        self.vertices=[tuple(lower[i]+(p[i]-lo[i])*(upper[i]-lower[i])/(hi[i]-lo[i]) for i in range(3)) for p in self.vertices]
    def cylinder(self, a, b, ra, rb, sides=7, bark=1, end=3):
        axis = unit(sub(b, a))
        u = unit(cross(axis, (0, 0, 1) if abs(axis[2]) < .9 else (0, 1, 0)))
        v = cross(axis, u)
        rings=[]
        for center,r in [(a,ra),(b,rb)]:
            rings.append([self.v(add(center, add(mul(u, r*math.cos(i*2*math.pi/sides)), mul(v,r*math.sin(i*2*math.pi/sides))))) for i in range(sides)])
        for i in range(sides):
            j=(i+1)%sides; color=max(0,bark-(i%3==0))
            self.tri(rings[0][i],rings[0][j],rings[1][j],color)
            self.tri(rings[0][i],rings[1][j],rings[1][i],color)
        ca,cb=self.v(a),self.v(b)
        for i in range(sides):
            j=(i+1)%sides
            self.tri(ca,rings[0][j],rings[0][i],end)
            self.tri(cb,rings[1][i],rings[1][j],end)
    def save(self):
        lines=['# TheTrailWalker original resource mesh; Y up, metre units.', '# Palette UVs; s off preserves faceted normals.', 'o '+self.name]
        lines += ['v %.6f %.6f %.6f'%p for p in self.vertices]
        lines += ['vt %.6f 0.5'%((i+.5)/len(COLORS)) for i in range(len(COLORS))]
        lines += ['s off']
        lines += ['f %d/%d %d/%d %d/%d'%(a,k+1,b,k+1,c,k+1) for a,b,c,k in self.faces]
        (OUT/(self.name+'.obj')).write_text('\n'.join(lines)+'\n',encoding='utf-8')
        print(self.name, len(self.faces), 'triangles')

def branch():
    m=Model('PineBranch')
    m.cylinder((-.49,-.05,-.10),(.05,.005,-.04),.075,.06)
    m.cylinder((.05,.005,-.04),(.49,.04,-.11),.06,.048)
    m.cylinder((.04,0,-.04),(.31,.15,.29),.048,.032)
    m.save()

def pelt():
    rng=random.Random(61);m=Model('RabbitPelt')
    # Folded hide with shoulder/haunch lobes, not a round rug or a whole rabbit.
    outline=[(0,.44),(.12,.35),(.32,.43),(.29,.20),(.38,.08),(.34,-.14),
             (.46,-.40),(.22,-.35),(0,-.42),(-.22,-.35),(-.46,-.40),
             (-.34,-.14),(-.38,.08),(-.29,.20),(-.32,.43),(-.12,.35)]
    outline.reverse()  # Match the upward-facing winding of the original top surface.
    n=len(outline)
    center=m.v((0,.08,0));inner=[];outer=[];lower=[]
    for i,(x,z) in enumerate(outline):
        t=i*2*math.pi/n
        y=.015+.055*max(0,math.cos(t+1))+.025*math.sin(t*3)
        inner.append(m.v((x*.55,.075+.02*math.cos(t*2),z*.55)))
        outer.append(m.v((x,y,z)));lower.append(m.v((x,y-.018,z)))
    bottom=m.v((0,.025,0))
    for i in range(n):
        j=(i+1)%n
        m.tri(center,inner[j],inner[i],4 if rng.random()<.8 else 5)
        m.tri(inner[i],inner[j],outer[j],5 if i%3 else 4)
        m.tri(inner[i],outer[j],outer[i],5 if i%3 else 6)
        m.tri(outer[i],outer[j],lower[j],7)
        m.tri(outer[i],lower[j],lower[i],7)
        m.tri(bottom,lower[i],lower[j],7)
    m.fit_bounds((-.48,-.028,-.48),(.48,.095,.48))
    m.save()

def apple():
    rng=random.Random(18);m=Model('Apple');n=20;rings=[]
    # Radius/height profile gives a squat silhouette and shallow stem hollow.
    for radius,y in [(.09,-.38),(.22,-.36),(.34,-.29),(.43,-.12),(.46,.05),(.45,.18),(.36,.31),(.23,.34),(.11,.28)]:
        rings.append([m.v((radius*(1+.045*math.cos(i*10*math.pi/n))*math.cos(i*2*math.pi/n),y+.012*math.cos(i*10*math.pi/n),radius*(1+.045*math.cos(i*10*math.pi/n))*math.sin(i*2*math.pi/n))) for i in range(n)])
    for k in range(len(rings)-1):
        for i in range(n):
            j=(i+1)%n;c=(10 if k>=5 else 9) if i%5 else 8
            m.tri(rings[k][i],rings[k+1][j],rings[k][j],c)
            m.tri(rings[k][i],rings[k+1][i],rings[k+1][j],c)
    bottom=m.v((0,-.38,0));top=m.v((0,.25,0))
    for i in range(n):
        j=(i+1)%n
        m.tri(bottom,rings[0][i],rings[0][j],8)
        m.tri(top,rings[-1][j],rings[-1][i],10)
    m.cylinder((0,.25,0),(-.025,.49,.01),.027,.023,sides=5,bark=1,end=2)
    leaf=[m.v(p) for p in [(0,.41,.01),(.08,.64,.01),(.34,.65,.01),(.26,.43,.01),(.17,.55,-.03)]]
    for i in range(4):
        j=(i+1)%4;m.tri(leaf[i],leaf[4],leaf[j],12+i%2);m.tri(leaf[j],leaf[4],leaf[i],14)
    m.fit_bounds((-.4393,-.402,-.46),(.4807,.65,.46))
    m.save()

def palette(filename='Region01Palette.png'):
    # Standard PNG, constructed directly; no image libraries or generated-image edits. RGBA: the alpha channel
    # carries each colour's surface-detail type for the FacetedDetail shader (detail_textures.PALETTES).
    from detail_textures import palette_alpha_for, alpha
    details=palette_alpha_for(filename)
    w,h=len(COLORS)*4,4
    raw=b''.join(b'\x00'+bytes(v for i in range(w) for v in (*COLORS[i//4],alpha(details.get(i//4)))) for _ in range(h))
    def chunk(tag,data): return struct.pack('!I',len(data))+tag+data+struct.pack('!I',zlib.crc32(tag+data)&0xffffffff)
    png=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('!2I5B',w,h,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(raw))+chunk(b'IEND',b'')
    (OUT/filename).write_bytes(png)

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True)
    branch();pelt();apple();palette()
