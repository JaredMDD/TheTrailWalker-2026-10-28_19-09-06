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
 SVG.mkdir(parents=True,exist_ok=True);PNG.mkdir(parents=True,exist_ok=True)
 for name,shapes in icons.items():write_svg(name,shapes);raster(name,shapes);print(name,'SVG + 256px RGBA PNG')
