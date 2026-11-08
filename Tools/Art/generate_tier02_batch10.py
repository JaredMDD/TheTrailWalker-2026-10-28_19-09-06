"""Batch 10: Normal/Elite birch axes, passive display art."""
import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import generate_region01_meshes as base
import generate_tier02_batch01 as kit
from generate_tier02_batch06 import tube
from generate_tier02_batch02 import fin,stroke
from tier02_batch_workflow import generate

COLORS=[(221,213,185),(173,160,128),(77,69,56),(185,104,57),(229,151,89),
        (137,72,41),(172,147,94),(119,94,61),(151,102,55),(195,151,89)]
TYPES=['bark','wood','bark','metal','metal','metal','plant_fibre','plant_fibre','wood','wood']


def handle_x(y):return -.075+.045*(.54-y)+.018*math.sin((y+.50)*4)


def socket(m):
    rings=[];n=24
    for y,rx,rz in [(.32,.091,.105),(.52,.091,.105),(.32,.048,.052),(.52,.048,.052)]:
        rings.append([m.v((-.075+rx*math.cos(i*math.tau/n),y,rz*math.sin(i*math.tau/n))) for i in range(n)])
    for a,b,color in [(0,1,3),(2,3,5),(0,2,4),(1,3,4)]:
        for i in range(n):
            j=(i+1)%n;m.tri(rings[a][i],rings[a][j],rings[b][j],color);m.tri(rings[a][i],rings[b][j],rings[b][i],color)


def blade(m):
    # Solid tapered cutting wedge; thin bright cutting bevel, no stone ingredient.
    pts=[(-.006,.33),(.17,.29),(.38,.24),(.42,.29),(.425,.49),(.38,.565),(.15,.52),(-.006,.51)]
    rings=[]
    for side in (-1,1):
        rings.append([m.v((x,y,side*(.012 if x>.37 else .068 if x<.04 else .047))) for x,y in pts])
    for side,ring in enumerate(rings):
        c=m.v((.16,.425,(-1 if side==0 else 1)*.050))
        for i in range(8):m.tri(c,ring[i],ring[(i+1)%8],4 if i in (1,2,3,4) else 3)
    for i in range(8):
        j=(i+1)%8;color=4 if i in (2,3,4) else 5
        m.tri(rings[0][i],rings[0][j],rings[1][j],color);m.tri(rings[0][i],rings[1][j],rings[1][i],color)
    # Blunt rear poll shares the socket body; not an unrelated floating block.
    fin(m,[(-.22,.34,-.068),(-.14,.33,-.068),(-.14,.51,-.068),(-.22,.50,-.068)],3,.136)


def axe(elite=False):
    m=base.Model('EliteBirchAxe' if elite else 'NormalBirchAxe')
    tube(m,[(handle_x(y),y,0) for y in [-.60,-.52,-.35,-.10,.12,.32,.54]],.043,0,12)
    # Small dark transverse birch lenticels on front/back, not pine grain.
    for sign in (-1,1):
        for i,y in enumerate([-.55,-.44,-.11,.03,.16,.25]):
            x=handle_x(y);width=.018 if i%2 else .026
            fin(m,[(x-width,y-.004,sign*.042),(x+width,y-.008,sign*.042),(x+width*.8,y+.003,sign*.042),(x-width*.7,y+.006,sign*.042)],2,sign*.004)
    socket(m);blade(m)
    # Exposed handle end and a seated birch wedge in the eye.
    fin(m,[(-.10,.514,-.037),(-.056,.514,-.037),(-.057,.555,-.037),(-.10,.55,-.037)],1,.074)
    # Continuous helical hemp wrap hugs the lower grip.
    points=[]
    for i in range(129):
        t=i/128;y=-.48+.23*t;a=t*math.tau*7
        points.append((handle_x(y)+.047*math.cos(a),y,.047*math.sin(a)))
    tube(m,points,.007,6,6)
    if elite:
        # Previous-tier pine-plank cheek reinforcement, retained copper head.
        for sign in (-1,1):
            pts=[(handle_x(-.17)-.043,-.17,sign*.050),(handle_x(-.17)+.043,-.17,sign*.050),
                 (handle_x(.28)+.038,.28,sign*.050),(handle_x(.28)-.038,.28,sign*.050)]
            fin(m,pts,8,sign*.018)
            stroke(m,[(handle_x(y)-.012,y,sign*.071) for y in [-.12,.03,.22]],9,.0025)
        for y in (-.10,.22):
            # Wooden pins are cut from the same reinforcement material.
            m.cylinder((handle_x(y),y,-.075),(handle_x(y),y,.075),.013,.013,10,9,9)
    return m


SPECS=[('NormalBirchAxe','Normal birch axe',lambda:axe(False),COLORS,TYPES),
       ('EliteBirchAxe','Elite birch axe',lambda:axe(True),COLORS,TYPES)]
RECIPES={'NormalBirchAxe':('T2-L3',8,'2 Birch branches + 1 Copper ingot + 1 Hemp cord -> 1'),
         'EliteBirchAxe':('T2-L7',9,'1 Normal birch axe + 2 Pine planks -> 1')}

if __name__=='__main__':generate(10,SPECS,RECIPES,category='Tools',role='Tool',preview='--preview' in sys.argv)
