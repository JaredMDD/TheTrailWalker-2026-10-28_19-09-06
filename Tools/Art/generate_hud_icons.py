"""Five editable SVG silhouettes and antialiased transparent PNGs; stdlib only."""
from pathlib import Path
import math
import struct
import zlib

ROOT=Path(__file__).resolve().parents[2]
SVG=ROOT/'Assets/_Game/Art/UI/Icons'
PNG=ROOT/'Assets/_Game/Resources/UI/Icons'

def rect(x,y,w,h,c='#111111',radius=0): return ('rect',(x,y,w,h,radius),c)
def circle(x,y,r,c='#111111'): return ('circle',(x,y,r),c)
def poly(points,c='#111111'): return ('polygon',points,c)
BLACK='#111111';GRAY='#777777';LIGHT='#dedede';CLEAR='#00000000'
icons={
 'inventory':[
  rect(43,12,42,33,BLACK,12),rect(51,19,26,23,CLEAR,7),
  rect(22,41,14,61,GRAY,6),rect(92,41,14,61,GRAY,6),
  rect(30,27,68,89,BLACK,19),rect(39,66,50,39,LIGHT,8),
  rect(42,73,44,5,BLACK,2),rect(78,76,4,12,GRAY,1),
  rect(45,40,38,6,GRAY,3)],
 'trash':[
  rect(49,15,30,18,BLACK,6),rect(55,21,18,12,CLEAR,2),
  rect(28,32,72,9,BLACK,3),
  poly([(34,47),(94,47),(87,110),(41,110)]),
  rect(51,57,6,40,CLEAR,3),rect(71,57,6,40,CLEAR,3)],
 'map':[
  circle(64,66,49),circle(64,66,40,LIGHT),
  rect(59,6,10,17,BLACK,4),circle(64,8,5),
  poly([(64,25),(77,66),(64,60),(51,66)],BLACK),
  poly([(64,107),(51,66),(64,72),(77,66)],GRAY),
  poly([(105,66),(64,79),(70,66),(64,53)],'#999999'),
  poly([(23,66),(64,53),(58,66),(64,79)],'#444444'),
  circle(64,66,7,BLACK),circle(64,66,3,LIGHT)],
 'equipment':[
  circle(64,20,12),
  poly([(48,39),(34,43),(20,63),(33,74),(44,61),(44,80),(84,80),(84,61),(95,74),(108,63),(94,43),(80,39),(72,48),(56,48)]),
  poly([(45,85),(83,85),(89,116),(68,116),(64,96),(60,116),(39,116)]),
  rect(47,80,34,6,GRAY,1)],
 'travel':[
  poly([(18,52),(70,52),(70,26),(112,64),(70,102),(70,76),(18,76)],BLACK),
  poly([(26,59),(78,59),(78,44),(98,64),(78,84),(78,69),(26,69)],GRAY)],
 # Settlement list (settlement shell, November 8, 2026): home, warehouse and lumber field.
 'home':[
  rect(80,20,13,30,BLACK,2),
  poly([(10,64),(64,16),(118,64),(106,64),(64,28),(22,64)]),
  poly([(64,34),(100,66),(100,112),(28,112),(28,66)]),
  rect(56,78,16,34,GRAY,3),
  rect(37,74,13,13,LIGHT,2),rect(78,74,13,13,LIGHT,2)],
 'warehouse':[
  poly([(8,52),(64,20),(120,52),(110,52),(64,30),(18,52)]),
  poly([(64,36),(108,56),(108,112),(20,112),(20,56)]),
  rect(36,62,56,50,GRAY,2),
  poly([(40,66),(46,66),(88,108),(82,108)],LIGHT),poly([(82,66),(88,66),(46,108),(40,108)],LIGHT),
  rect(57,40,14,12,LIGHT,2)],
 'lumber':[
  poly([(64,4),(86,34),(42,34)]),poly([(64,16),(96,54),(32,54)]),
  rect(59,52,10,14,BLACK,1),
  circle(38,106,13),circle(64,106,13),circle(90,106,13),circle(51,82,13),circle(77,82,13),
  circle(38,106,7,LIGHT),circle(64,106,7,LIGHT),circle(90,106,7,LIGHT),circle(51,82,7,LIGHT),circle(77,82,7,LIGHT),
  circle(38,106,2.5,GRAY),circle(64,106,2.5,GRAY),circle(90,106,2.5,GRAY),circle(51,82,2.5,GRAY),circle(77,82,2.5,GRAY)],
 # Settlement menu button (user, November 9, 2026): a row of houses; tapping it opens the list downward.
 'settlement':[
  poly([(2,74),(20,54),(38,74)]),rect(6,72,28,40,BLACK,1),
  poly([(90,74),(108,54),(126,74)]),rect(94,72,28,40,BLACK,1),
  poly([(40,56),(64,26),(88,56)]),rect(44,54,40,58,BLACK,1),
  rect(57,84,14,28,GRAY,2),rect(58,64,12,12,LIGHT,2),
  rect(14,84,12,12,LIGHT,2),rect(102,84,12,12,LIGHT,2)],
 # Inside-house HUD (home revision, November 14, 2026): store (a market stall), upgrade (an up arrow) and pet (a paw).
 'store':[
  rect(10,16,108,12,BLACK,4),
  rect(14,28,20,18,BLACK),rect(34,28,20,18,GRAY),rect(54,28,20,18,BLACK),rect(74,28,20,18,GRAY),rect(94,28,20,18,BLACK),
  circle(24,46,10),circle(44,46,10,GRAY),circle(64,46,10),circle(84,46,10,GRAY),circle(104,46,10),
  rect(20,56,9,30,BLACK,2),rect(99,56,9,30,BLACK,2),
  rect(12,80,104,34,BLACK,5),rect(22,90,84,6,LIGHT,3),rect(22,101,84,4,GRAY,2)],
 'upgrade':[
  poly([(64,8),(112,58),(84,58),(84,94),(44,94),(44,58),(16,58)]),
  poly([(64,26),(94,52),(74,52),(74,86),(54,86),(54,52),(34,52)],GRAY),
  rect(28,102,72,14,BLACK,6)],
 'pet':[
  circle(30,56,13),circle(50,32,13),circle(78,32,13),circle(98,56,13),
  circle(64,82,22),circle(48,96,16),circle(80,96,16),rect(48,90,32,22,BLACK,8)],
 # Store menu and bird's-eye HUD (home revision Task 5, November 14, 2026): storage (a chest) and store_piece (an arrow
 # dropping a piece into an open crate).
 'storage':[
  rect(14,26,100,34,BLACK,12),rect(14,46,100,8,GRAY),
  rect(14,58,100,56,BLACK,6),rect(40,82,48,5,GRAY,2),rect(40,96,48,5,GRAY,2),
  rect(14,52,100,8,LIGHT),
  rect(54,42,20,30,BLACK,4),rect(59,50,10,14,LIGHT,3),
  rect(26,30,8,84,GRAY,2),rect(94,30,8,84,GRAY,2)],
 'store_piece':[
  poly([(52,6),(76,6),(76,38),(96,38),(64,70),(32,38),(52,38)]),
  poly([(57,12),(71,12),(71,44),(84,44),(64,63),(44,44),(57,44)],GRAY),
  poly([(8,64),(30,64),(40,78),(88,78),(98,64),(120,64),(108,116),(20,116)]),
  rect(30,90,68,6,LIGHT,3),rect(36,102,56,4,GRAY,2)],
 'user':[
  poly([(64+49*(1 if i%2==0 else .49)*math.cos(-math.pi/2+i*math.pi/8),64+49*(1 if i%2==0 else .49)*math.sin(-math.pi/2+i*math.pi/8)) for i in range(16)])]
}

def rgba(c):
 h=c.lstrip('#');return tuple(int(h[i:i+2],16) for i in (0,2,4))+((int(h[6:8],16) if len(h)==8 else 255),)
def inside(shape,x,y):
 kind,p,_=shape
 if kind=='circle': return (x-p[0])**2+(y-p[1])**2<=p[2]**2
 if kind=='rect':
  a,b,w,h,r=p
  if not(a<=x<=a+w and b<=y<=b+h): return False
  if r==0:return True
  dx=max(a+r-x,0,x-(a+w-r));dy=max(b+r-y,0,y-(b+h-r))
  return dx*dx+dy*dy<=r*r
 hit=False;j=len(p)-1
 for i,(a,b) in enumerate(p):
  c,d=p[j]
  if (b>y)!=(d>y) and x<(c-a)*(y-b)/(d-b)+a:hit=not hit
  j=i
 return hit
def bounds(shape):
 kind,p,_=shape
 if kind=='circle': x,y,r=p;return (x-r,y-r,x+r,y+r)
 if kind=='rect':x,y,w,h,_=p;return (x,y,x+w,y+h)
 return min(x for x,y in p),min(y for x,y in p),max(x for x,y in p),max(y for x,y in p)
def svg_shape(shape,erase=False):
 kind,p,c=shape;color='#000000' if erase else c[:7]
 if kind=='rect':x,y,w,h,r=p;tag=f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}"'
 elif kind=='circle':x,y,r=p;tag=f'<circle cx="{x}" cy="{y}" r="{r}"'
 else:tag='<polygon points="'+' '.join(f'{x:.3f},{y:.3f}' for x,y in p)+'"'
 return tag+f' fill="{color}"/>'
def write_svg(name,shapes):
 # Apply cutouts in drawing order so later filled shapes can cover earlier holes.
 defs=[];body=''
 for s in shapes:
  if rgba(s[2])[3]:body+=svg_shape(s)
  else:
   key='cut'+str(len(defs))
   defs.append('<mask id="'+key+'"><rect width="128" height="128" fill="white"/>'+svg_shape(s,True)+'</mask>')
   body='<g mask="url(#'+key+')">'+body+'</g>'
 opacity=' opacity="0.65"' if name=='trash' else ''
 text='<svg xmlns="http://www.w3.org/2000/svg" width="128" height="128" viewBox="0 0 128 128"><defs>'+''.join(defs)+'</defs><g'+opacity+'>'+body+'</g></svg>\n'
 (SVG/(name+'.svg')).write_text(text,encoding='utf-8')
def write_png(path,w,h,data):
 def ch(t,b):return struct.pack('!I',len(b))+t+b+struct.pack('!I',zlib.crc32(t+b)&0xffffffff)
 raw=b''.join(b'\0'+bytes(data[y*w*4:(y+1)*w*4]) for y in range(h))
 path.write_bytes(b'\x89PNG\r\n\x1a\n'+ch(b'IHDR',struct.pack('!2I5B',w,h,8,6,0,0,0))+ch(b'IDAT',zlib.compress(raw))+ch(b'IEND',b''))
def raster(name,shapes):
 n=512;scale=4;buf=bytearray(n*n*4)
 for shape in shapes:
  x0,y0,x1,y1=bounds(shape);color=rgba(shape[2])
  for y in range(max(0,int(y0*scale)),min(n,math.ceil(y1*scale))):
   for x in range(max(0,int(x0*scale)),min(n,math.ceil(x1*scale))):
    if inside(shape,(x+.5)/scale,(y+.5)/scale):
     k=(y*n+x)*4;buf[k:k+4]=bytes(color)
 # 2x supersampling yields a 256px sprite with premultiplied-correct edge colors.
 w=n//2;out=bytearray(w*w*4)
 for y in range(w):
  for x in range(w):
   samples=[(2*y+dy)*n*4+(2*x+dx)*4 for dy in range(2) for dx in range(2)]
   a=sum(buf[k+3] for k in samples);i=(y*w+x)*4
   if a:
    for c in range(3):out[i+c]=round(sum(buf[k+c]*buf[k+3] for k in samples)/a)
   out[i+3]=round(a/4*(.65 if name=='trash' else 1))
 write_png(PNG/(name+'.png'),w,w,out)

if __name__=='__main__':
 # Names on the command line regenerate only those icons (e.g. store upgrade pet); none regenerates all of them.
 import sys
 wanted=sys.argv[1:] or list(icons)
 unknown=[n for n in wanted if n not in icons]
 if unknown:raise SystemExit('Unknown icons: '+', '.join(unknown))
 SVG.mkdir(parents=True,exist_ok=True);PNG.mkdir(parents=True,exist_ok=True)
 for name in wanted:shapes=icons[name];write_svg(name,shapes);raster(name,shapes);print(name,'SVG + 256px RGBA PNG')
