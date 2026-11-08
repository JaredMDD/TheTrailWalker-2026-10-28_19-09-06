"""Editable SVG + transparent 256px numbered quality stars; standard library only."""
from pathlib import Path
import math
import generate_hud_icons as art

ROOT=Path(__file__).resolve().parents[2]
def star(radius,color,points):
    return art.poly([(64+radius*(1 if i%2==0 else .46)*math.cos(-math.pi/2+i*math.pi/points),64+radius*(1 if i%2==0 else .46)*math.sin(-math.pi/2+i*math.pi/points)) for i in range(points*2)],color)
def shapes(n):
    points=5 if n<=2 else 6 if n<=4 else 10
    s=[star(59,'#70491b',points),star(54,'#d6a22a',points),art.poly([(64,10),(76,47),(64,61),(52,47)],'#ffe08a')]
    ink='#38250f'
    if n==1:
        s += [art.poly([(56,54),(63,47),(69,47),(69,80),(76,80),(76,86),(53,86),(53,80),(61,80),(61,56),(56,60)],ink)]
        return s
    segments={
      'a':(53,47,24,6),'b':(71,49,6,18),'c':(71,65,6,18),
      'd':(53,80,24,6),'e':(53,65,6,18),'f':(53,49,6,18),'g':(53,64,24,6)}
    patterns={2:'abged',3:'abgcd',4:'fgbc',5:'afgcd'}
    for key in patterns[n]:s.append(art.rect(*segments[key],c=ink,radius=2))
    return s
if __name__=='__main__':
    art.SVG=ROOT/'Assets/_Game/Art/UI/ItemDetails';art.PNG=ROOT/'Assets/_Game/Resources/UI/ItemDetails'
    art.SVG.mkdir(parents=True,exist_ok=True);art.PNG.mkdir(parents=True,exist_ok=True)
    for n in range(1,6):
        art.write_svg('quality_'+str(n),shapes(n));art.raster('quality_'+str(n),shapes(n))
        print('quality_'+str(n),'SVG + 256px RGBA PNG')
