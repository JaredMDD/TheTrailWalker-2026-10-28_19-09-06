"""Naturalistic, smoothly shaded character surfaces. Preserves the existing rigid-part joint frames."""
import math
import generate_region01_meshes as b

def surface(m,rows,n,color=0,axis='y'):
    """Rows: (axis coordinate, centre1, centre2, radius1, radius2). Closed loft, outward winding."""
    rings=[]
    for position,c1,c2,r1,r2 in rows:
        ring=[]
        for i in range(n):
            a=i*math.tau/n;p=(c1+r1*math.cos(a),position,c2+r2*math.sin(a))
            if axis=='z':p=(p[0],-p[2],p[1]) # proper rotation, preserving winding
            ring.append(m.v(p))
        rings.append(ring)
    for j in range(len(rows)-1):
        for i in range(n):
            k=(i+1)%n;c=color(j,i) if callable(color) else color
            m.tri(rings[j][i],rings[j+1][k],rings[j][k],c);m.tri(rings[j][i],rings[j+1][i],rings[j+1][k],c)
    for end in (0,len(rows)-1):
        position,c1,c2,_,_=rows[end];p=(c1,position,c2);p=(p[0],-p[2],p[1]) if axis=='z' else p;center=m.v(p)
        for i in range(n):
            a,c=rings[end][i],rings[end][(i+1)%n]
            k=color(max(0,end-1),i) if callable(color) else color
            m.tri(center,a,c,k) if end==0 else m.tri(center,c,a,k)

def ellipse(m,c,r,color,n=20,rings=10):
    rows=[]
    for j in range(1,rings):
        a=math.pi-j*math.pi/rings;rows.append((c[1]+r[1]*math.cos(a),c[0],c[2],r[0]*math.sin(a),r[2]*math.sin(a)))
    surface(m,rows,n,color)

def stroke(m,points,r,k,sides=6):
    # Constant color, unlike the resource cylinder's intentional bark shade alternation.
    for a,c in zip(points,points[1:]):
        before=len(m.faces);m.cylinder(a,c,r,r,sides,k,k)
        m.faces[before:]=[(x,y,z,k) for x,y,z,_ in m.faces[before:]]

def triangle(m,points,k):
    v=[m.v(p) for p in points];m.tri(*v,k)

HEAD_PROFILE=[(0,.034,.036,.040),(.015,.052,.048,.052),(.032,.066,.056,.064),(.05,.076,.065,.071),(.07,.082,.072,.078),(.085,.086,.076,.082),(.098,.089,.079,.085),(.11,.091,.079,.087),(.12,.091,.078,.088),(.132,.090,.077,.089),(.142,.089,.077,.089),(.153,.089,.079,.088),(.164,.089,.081,.086),(.18,.087,.080,.084),(.199,.081,.077,.08),(.216,.069,.067,.071),(.231,.052,.052,.056),(.243,.029,.03,.032),(.249,.005,.006,.006)]

def profile(y,female):
    sy=.96 if female else 1;yy=y/sy
    for a,c in zip(HEAD_PROFILE,HEAD_PROFILE[1:]):
        if a[0]<=yy<=c[0]:
            t=(yy-a[0])/(c[0]-a[0]);return tuple((a[i]+(c[i]-a[i])*t)*(.95 if female and i==1 else .97 if female else 1) for i in range(1,4))
    return HEAD_PROFILE[0][1:] if yy<0 else HEAD_PROFILE[-1][1:]

def face_z(x,y,female):
    rx,front,_=profile(y,female);z=front*math.sqrt(max(0,1-(x/rx)**2));sy=.96 if female else 1;yy=y/sy
    def g(v,s):return math.exp(-(v/s)**2)
    z+=.004*g(abs(x)-.055,.025)*g(yy-.113,.026)
    z-=.0045*(g(x-.033,.019)+g(x+.033,.019))*g(yy-.137,.01)
    z+=.003*g(yy-.154,.009)*(g(x-.03,.025)+g(x+.03,.025))
    # Nose bridge/tip is part of the facial surface, not an attached triangular spike.
    z+=(.008*g(yy-.134,.023)+.022*g(yy-.107,.011))*g(x,.0115)
    z+=.004*g(yy-.102,.007)*(g(x-.012,.006)+g(x+.012,.006))
    z+=.002*g(x,.022)*(g(yy-.068,.004)+g(yy-.059,.004))
    return z

def head(name,female):
    m=b.Model(name);n=48;rows=[];sy=.96 if female else 1
    for y,_,_,_ in HEAD_PROFILE:
        yy=y*sy;rx,front,back=profile(yy,female);row=[]
        for i in range(n):
            a=i*math.tau/n;x=rx*math.cos(a);z=face_z(x,yy,female) if math.sin(a)>=0 else back*math.sin(a)
            row.append(m.v((x,yy,z)))
        rows.append(row)
    for j in range(len(rows)-1):
        for i in range(n):
            k=(i+1)%n;m.tri(rows[j][i],rows[j+1][k],rows[j][k],0);m.tri(rows[j][i],rows[j+1][i],rows[j+1][k],0)
    for end in (0,len(rows)-1):
        c=m.v((0,HEAD_PROFILE[end][0]*sy,0))
        for i in range(n):m.tri(c,rows[end][i],rows[end][(i+1)%n],0) if end==0 else m.tri(c,rows[end][(i+1)%n],rows[end][i],0)
    # Ears follow the side of the skull rather than protruding like round knobs.
    for side in (-1,1):
        ellipse(m,(side*.097*(.95 if female else 1),.124*sy,-.004),(.011,.025,.015),0,12,8)
        stroke(m,[(side*.104*(.95 if female else 1),.108*sy,.001),(side*.106*(.95 if female else 1),.128*sy,.006),(side*.103*(.95 if female else 1),.142*sy,.001)],.0016,1)
        ex=side*.033;ey=.137*sy
        for upper in (False,True):
            points=[]
            for i in range(13):
                x=ex-.014+i*.028/12;t=i/12;y=ey+(.0048 if upper else -.0032)*math.sin(t*math.pi)
                points.append((x,y,face_z(x,y,female)+.0012))
            stroke(m,points,.00125 if upper else .0008,0)
        points=[]
        for i in range(8):
            x=ex-.018+i*.036/7;y=(.157+.0025*math.sin(i*math.pi/7))*sy
            points.append((x,y,face_z(x,y,female)+.0009))
        stroke(m,points,.00115,3)
        x=side*.011;y=.099*sy;ellipse(m,(x,y,face_z(x,y,female)+.0006),(.003,.0013,.0008),7,8,4)
    stroke(m,[(x,.064*sy,face_z(x,.064*sy,female)+.0007) for x in [-.022,-.014,-.006,0,.006,.014,.022]],.00065,7)
    return m

def eyes(name,female):
    m=b.Model(name);sy=.96 if female else 1
    for side in (-1,1):
        cx=side*.033;cy=.137*sy;center=m.v((cx,cy,face_z(cx,cy,female)+.0015));outline=[]
        for i in range(20):
            a=i*math.tau/20;x=cx+.0125*math.cos(a);y=cy+(.0032 if math.sin(a)>0 else .0025)*math.sin(a)
            outline.append(m.v((x,y,face_z(x,y,female)+.00065)))
        for i in range(20):m.tri(center,outline[i],outline[(i+1)%20],5)
        z=face_z(cx,cy,female)+.0022
        ellipse(m,(cx,cy,z),(.0038,.0038,.0009),4,16,8);ellipse(m,(cx,cy,z+.0008),(.0018,.0018,.0004),6,12,6)
    return m

def hair(name,female):
    m=b.Model(name);n=40;rows=[];sy=.96 if female else 1
    for j in range(12):
        t=j/11;row=[]
        for i in range(n):
            a=i*math.tau/n;front=max(0,math.sin(a));line=.105+.076*front # above the brow, lower around the back
            y=(line+(.252-line)*t)*sy;rx,rf,rb=profile(min(y,.249*sy),female)
            volume=.007;z=(rf if math.sin(a)>0 else rb)*math.sin(a)
            row.append(m.v(((rx+volume)*math.cos(a),y+.004,z+volume*math.sin(a))))
        rows.append(row)
    for j in range(11):
        for i in range(n):
            k=(i+1)%n;m.tri(rows[j][i],rows[j+1][k],rows[j][k],0);m.tri(rows[j][i],rows[j+1][i],rows[j+1][k],0)
    crown=m.v((0,.26*sy,0))
    for i in range(n):m.tri(crown,rows[-1][(i+1)%n],rows[-1][i],0)
    if female:
        surface(m,[(.018,0,-.112,.004,.006),(.045,0,-.124,.012,.013),(.085,0,-.135,.018,.018),(.125,0,-.126,.023,.023),(.155,0,-.109,.022,.022),(.173,0,-.094,.012,.012)],20,0)
    return m

def torso(name,female):
    m=b.Model(name);rows=[]
    for y,rx,rz,cz in [(-.045,.187,.13,0),(0,.18,.126,0),(.08,.164 if female else .174,.12,0),(.17,.172 if female else .19,.127,0),(.25,.193 if female else .214,.137,.003),(.32,.213 if female else .232,.131,0),(.36,.214 if female else .23,.12,0),(.393,.17,.10,0),(.418,.095,.074,0),(.429,.067,.063,0)]:rows.append((y,0,cz,rx,rz))
    surface(m,rows,28,8)
    # Soft cloth hem/collar without alternating armor-like face patches.
    surface(m,[(.405,0,0,.089,.078),(.43,0,0,.074,.068)],28,8)
    return m

def limb(name,rows,color):
    m=b.Model(name);surface(m,[(y,0,z,rx,rz) for y,rx,rz,z in rows],20,color);return m

def hand():
    m=limb('Hand',[(-.09,.039,.018,0),(-.06,.041,.020,0),(-.028,.035,.022,0),(.006,.029,.025,0)],0)
    for i,(x,length) in enumerate([(-.027,.048),(-.009,.058),(.009,.056),(.026,.044)]):
        points=[(x,-.078,0),(x,-.108,-.006),(x,-.09-length,-.018)]
        stroke(m,points,.0078 if i<3 else .007,0,10)
    stroke(m,[(.032,-.03,0),(.048,-.058,-.009),(.044,-.085,-.018)],.0105,0,10)
    return m

def foot(name,bare=False):
    m=b.Model(name)
    if bare:
        rows=[(-.07,0,.04,.045,.045),(-.025,0,.045,.055,.05),(.035,0,.06,.06,.037),(.11,0,.07,.06,.026),(.165,0,.077,.048,.019),(.18,0,.078,.027,.015)]
    else:
        rows=[(-.075,0,.01,.05,.06),(-.03,0,.02,.068,.06),(.025,0,.04,.07,.042),(.10,0,.05,.067,.032),(.17,0,.054,.051,.024),(.19,0,.055,.024,.018)]
    surface(m,rows,24,0 if bare else 14,axis='z')
    surface(m,[(-.025,0,0,.052,.05),(.015,0,0,.051,.049),(.045,0,0,.048,.046)],20,0 if bare else 14)
    if not bare:
        outline=[(-.05,-.08),(.05,-.08),(.071,-.04),(.071,.12),(.052,.2),(-.052,.2),(-.071,.12),(-.071,-.04)]
        low=[m.v((x,-.099,z)) for x,z in outline];high=[m.v((x,-.08,z)) for x,z in outline]
        for i in range(len(outline)):
            j=(i+1)%len(outline);m.tri(low[i],high[i],high[j],15);m.tri(low[i],high[j],low[j],15)
        for row,y,reverse in [(low,-.099,False),(high,-.08,True)]:
            c=m.v((0,y,.06))
            for i in range(len(row)):m.tri(c,row[(i+1)%len(row)],row[i],15) if reverse else m.tri(c,row[i],row[(i+1)%len(row)],15)
    return m

def hat():
    m=b.Model('FurCap');surface(m,[(.187,0,-.004,.102,.097),(.205,0,-.004,.106,.10),(.228,0,-.006,.098,.094),(.25,0,-.009,.076,.075),(.268,0,-.009,.035,.034),(.273,0,-.009,.006,.006)],28,11)
    surface(m,[(.184,0,-.004,.103,.099),(.203,0,-.004,.107,.101),(.212,0,-.004,.104,.099)],28,13)
    return m

def hood():
    m=b.Model('ClothHood');n=28;rings=[]
    for y,r in [(0,.13),(.09,.13),(.17,.123),(.225,.105),(.265,.063),(.278,.009)]:
        rings.append([m.v((r*math.cos(math.pi*5/6+i*math.pi*4/3/n),y,r*math.sin(math.pi*5/6+i*math.pi*4/3/n)-.013)) for i in range(n+1)])
    for a,c in zip(rings,rings[1:]):
        for i in range(n):m.tri(a[i],c[i+1],a[i+1],8);m.tri(a[i],c[i],c[i+1],8)
    m.faces += [(a,c,d,k) for a,d,c,k in list(m.faces)]
    return m

def write_npc_parts(save):
    for female,label in [(False,'Male'),(True,'Female')]:
        save(head(label+'Head',female));save(eyes(label+'FaceDetails',female));save(torso(label+'Torso',female));save(hair('TiedHair' if female else 'ShortHair',female))
    save(hat());save(hood());save(hand());save(foot('Boot'));save(foot('BareFoot',True))
    for name,rows,color in [
      ('Neck',[(.39,.054,.052,0),(.44,.05,.05,0),(.5,.047,.047,.005)],0),
      ('Hips',[(-.105,.14,.12,-.006),(-.075,.182,.14,0),(0,.194,.145,0),(.07,.178,.126,0)],9),
      ('UpperArm',[(-.262,.061,.058,0),(-.23,.063,.061,0),(-.15,.069,.067,0),(-.06,.077,.075,0),(.015,.073,.073,0),(.04,.04,.04,0)],8),
      ('Forearm',[(-.245,.029,.027,0),(-.20,.036,.032,0),(-.11,.048,.043,-.003),(-.03,.05,.047,0),(.022,.04,.04,0)],0),
      ('Thigh',[(-.365,.07,.071,0),(-.3,.078,.08,0),(-.18,.092,.095,.003),(-.06,.101,.10,0),(.03,.082,.082,0)],9),
      ('Shin',[(-.435,.047,.045,0),(-.37,.05,.05,-.005),(-.26,.059,.067,-.012),(-.14,.067,.07,-.007),(-.025,.069,.066,0),(.04,.067,.063,-.005),(.07,.045,.04,-.01)],9)]:save(limb(name,rows,color))

def rabbit_parts(save):
    m=b.Model('RabbitBody')
    surface(m,[(-.30,0,-.065,.01,.02),(-.27,0,-.07,.065,.095),(-.23,0,-.075,.125,.145),(-.15,0,-.072,.16,.16),(-.04,0,-.06,.15,.145),(.08,0,-.055,.135,.13),(.18,0,-.07,.105,.11),(.21,0,-.09,.065,.08),(.24,0,-.105,.02,.03)],28,5,axis='z');save(m)
    m=b.Model('RabbitHead')
    surface(m,[(-.11,0,-.03,.008,.012),(-.095,0,-.035,.025,.035),(-.07,0,-.035,.038,.055),(-.02,0,-.035,.067,.075),(.04,0,-.04,.073,.08),(.09,0,-.025,.06,.06),(.135,0,-.012,.041,.038),(.16,0,-.006,.026,.025)],28,5,axis='z')
    ellipse(m,(0,.006,.169),(.012,.008,.007),8,12,8)
    for side in (-1,1):
        ellipse(m,(side*.071,.066,.059),(.005,.010,.012),7,14,8)
        ellipse(m,(side*.075,.069,.064),(.0014,.0015,.0017),12,8,4)
    save(m)
    m=b.Model('RabbitEar');surface(m,[(-.11,0,0,.012,.011),(-.07,0,0,.018,.013),(0,0,0,.024,.014),(.07,0,0,.031,.012),(.14,0,0,.020,.008),(.18,0,0,.002,.003)],20,5)
    # A narrow recessed inner ear, not a broad pink triangular card.
    for j in range(7):
        y0=-.008+j*.022;y1=y0+.022;w0=.015*math.sin((j+1)*math.pi/9);w1=.015*math.sin((j+2)*math.pi/9)
        a=m.v((-w0,y0,.0145));c=m.v((w0,y0,.0145));d=m.v((w1,y1,.0145-j*.0007));e=m.v((-w1,y1,.0145-j*.0007))
        m.tri(a,c,d,8);m.tri(a,d,e,8)
    save(m)
    m=b.Model('RabbitFrontFoot');surface(m,[(-.047,0,.005,.016,.017),(.01,0,0,.017,.020),(.06,0,-.008,.020,.024),(.12,0,-.025,.025,.03),(.20,0,-.045,.026,.034)],20,5)
    ellipse(m,(0,-.052,.033),(.023,.016,.056),5,18,8);save(m)
    m=b.Model('RabbitHindFoot');surface(m,[(-.08,0,-.04,.025,.035),(-.025,0,-.035,.031,.045),(.04,0,.005,.048,.075),(.12,0,.035,.059,.10),(.20,0,.035,.048,.081),(.24,0,.025,.025,.04)],24,5)
    ellipse(m,(0,-.09,.055),(.04,.018,.13),5,22,8);save(m)
    m=b.Model('RabbitTail');ellipse(m,(0,0,0),(.034,.039,.043),6,18,10);save(m)
