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
def bar(x0,y0,x1,y1,w,c=BLACK):
 # A straight stroke of width w from (x0,y0) to (x1,y1), as a quad.
 dx,dy=x1-x0,y1-y0;k=w/2/math.hypot(dx,dy);nx,ny=-dy*k,dx*k
 return poly([(x0+nx,y0+ny),(x1+nx,y1+ny),(x1-nx,y1-ny),(x0-nx,y0-ny)],c)
def oval(cx,cy,rx,ry,c=BLACK,n=40): return poly([(cx+rx*math.cos(2*math.pi*i/n),cy+ry*math.sin(2*math.pi*i/n)) for i in range(n)],c)
def grain(cx,cy,angle,c=BLACK):
 # One wheat kernel: a pointed oval 9 long, tilted by angle (radians from straight up).
 s,co=math.sin(angle),math.cos(angle)
 return poly([(cx+x*co-y*s,cy+x*s+y*co) for x,y in ((0,-6),(3.6,-1.5),(3.2,2.5),(0,5),(-3.2,2.5),(-3.6,-1.5))],c)
def ear(x0,y0,x1,y1):
 # A wheat ear along its stalk's top: kernel pairs from (x0,y0) up to the tip (x1,y1).
 a=math.atan2(x1-x0,y0-y1);out=[]
 for i in range(5):
  t=i/4.6;x=x0+(x1-x0)*t;y=y0+(y1-y0)*t
  out+= [grain(x-3.4*math.cos(a),y-3.4*math.sin(a),a-.45),grain(x+3.4*math.cos(a),y+3.4*math.sin(a),a+.45)]
 return out+[grain(x1,y1-1,a)]
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
 # Counter wallet labels (phone round R2, November 21, 2026): in colour, unlike every other HUD icon, so the two read apart at a
 # glance (user: "the icons should use different color to properly represent what they refer to"). cash: a banded stack of
 # green notes; gold: a faceted ingot, lit top face, shaded side, a highlight streak.
 'cash':[
  poly([(24,24),(110,16),(116,64),(30,72)],'#24502E'),
  poly([(16,38),(106,32),(110,80),(20,86)],'#336F40'),
  rect(8,52,112,62,'#4C9A58',7),
  rect(15,59,98,48,'#69B76C',4),
  poly([(15,59),(113,59),(113,67),(15,81)],'#84CB80'),
  circle(72,83,15,'#3A7F47'),circle(72,83,10,'#A8DC9F'),circle(72,83,4,'#3A7F47'),
  rect(96,64,12,5,'#3A7F47',2),rect(96,97,12,5,'#3A7F47',2),
  rect(34,50,18,66,'#E8D7A0',2),rect(34,50,4,66,'#C2A964'),rect(38,76,14,14,'#C2A964',2)],
 'gold':[
  poly([(36,38),(92,38),(104,58),(24,58)],'#FFE07A'),
  poly([(92,38),(108,48),(124,92),(116,100),(104,58)],'#A86F10'),
  poly([(24,58),(104,58),(116,100),(10,100)],'#E3A626'),
  poly([(30,63),(50,63),(38,95),(17,95)],'#F7CD52'),
  poly([(42,42),(60,42),(54,53),(35,53)],'#FFF5C8'),
  rect(56,72,30,14,'#C88C18',3),rect(60,76,22,6,'#EDBB45',2),
  poly([(10,100),(116,100),(118,106),(8,106)],'#8A5A0C')],
 # Settlement list shops (phone round R2, November 21, 2026): black and grey like the home, warehouse and lumber icons.
 'bakery':[
  rect(10,44,108,62,BLACK,31),rect(10,80,108,28,BLACK,6),
  poly([(36,52),(46,50),(36,80),(26,82)],LIGHT),poly([(60,48),(70,48),(60,80),(50,80)],LIGHT),poly([(84,50),(94,52),(84,82),(74,80)],LIGHT),
  rect(16,96,96,7,GRAY,3)],
 'farm':[
  bar(62,118,50,40,7),bar(66,118,64,34,7),bar(70,118,80,40,7),
  bar(58,118,30,50,6),bar(72,118,100,50,6)]
  +ear(50,44,44,8)+ear(64,38,64,4)+ear(80,44,86,8)+ear(31,52,16,20)+ear(99,52,114,20)
  +[rect(46,82,38,11,GRAY,4),rect(46,86,38,3,LIGHT,1)],
 'mine':[
  bar(30,116,86,38,13),bar(30,116,46,94,15,GRAY),
  poly([(12,48),(32,28),(64,16),(96,22),(122,46),(100,40),(66,32),(36,40)]),
  poly([(36,36),(64,24),(92,28),(66,28)],GRAY),
  rect(74,28,22,22,BLACK,4)],
 'tailor':[
  rect(18,20,56,12,BLACK,4),rect(18,98,56,12,BLACK,4),
  rect(26,32,40,66,GRAY,2),
  rect(26,42,40,4,BLACK),rect(26,56,40,4,BLACK),rect(26,70,40,4,BLACK),rect(26,84,40,4,BLACK),
  bar(104,12,92,116,7),oval(103,24,2.5,6,LIGHT),
  bar(66,48,101,25,3,GRAY)],
 'ranch':[
  poly([(36,34),(14,12),(22,30),(34,42)],GRAY),poly([(92,34),(114,12),(106,30),(94,42)],GRAY),
  poly([(10,46),(36,38),(36,58),(16,58)]),poly([(118,46),(92,38),(92,58),(112,58)]),
  rect(32,28,64,78,BLACK,24),
  poly([(46,30),(70,30),(62,50),(50,46)],GRAY),
  rect(36,76,56,40,GRAY,18),circle(52,96,5),circle(76,96,5),
  circle(50,60,5,LIGHT),circle(78,60,5,LIGHT)],
 'fishery':[
  poly([(46,42),(62,20),(80,26),(78,44)]),
  poly([(88,64),(122,34),(114,64),(122,94)]),
  oval(56,64,46,28),
  poly([(50,86),(62,104),(70,88)],BLACK),
  poly([(36,42),(44,52),(46,64),(44,76),(36,86),(40,64)],GRAY),
  circle(26,58,6,LIGHT),circle(26,58,2.5,BLACK),
  poly([(56,58),(64,52),(72,58),(64,64)],GRAY),poly([(72,66),(80,60),(88,66),(80,72)],GRAY),poly([(56,72),(64,66),(72,72),(64,78)],GRAY)],
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
