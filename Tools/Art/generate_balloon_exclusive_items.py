"""Art-only outputs of the optional mystery-box recipes. Standard library, no loot activation."""
from pathlib import Path
import math,json,re
import generate_region01_meshes as b
from generate_region05_meshes import sphere
from generate_tier01_crafted_items import COLORS,vessel,box

ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'Assets/_Game/Art/Crafting/BalloonExclusive'
LEGACY=ROOT/'Assets/_Game/Art/Crafting/Tier01/Tier01Recipes.json'
REUSE={'roasted_shrimp','shrimp_pie','apple_oat_compote','red_berry_oat_porridge','pinecone_rabbit'}

def recipes():
    p1=(ROOT/'PROJECT_RECOMENDATIONS.md').read_text(encoding='utf-8')
    p1=p1.split('### 9.4.2 Optional balloon-box recipe collection',1)[1].split('### 9.4.4 Premium bakery',1)[0]
    p2=(ROOT/'PROJECT_RECOMENDATIONS_PART_2.md').read_text(encoding='utf-8')
    p2=p2.split('## 10. Optional premium mystery-box recipes',1)[1].split('## 11.',1)[0]
    rows=[];old={r['Id']:r for r in json.loads(LEGACY.read_text(encoding='utf-8'))['Recipes']}
    for text,expansion in [(p1,False),(p2,True)]:
        for line in text.splitlines():
            if not line.startswith('|'):continue
            cols=[x.strip() for x in line.strip('|').split('|')]
            match=re.search(r'`(recipe\.(food|misc)\.([a-z_]+))`',cols[0])
            if not match or match[3]=='truffle_barley_pot':continue
            recipe_id,category,key=match.groups();tier=int(cols[1])
            name=cols[2] if expansion else cols[0].split('`')[-1].strip(' —–-')
            ingredient=cols[3] if expansion else cols[2]
            existing=old.get(recipe_id)
            rows.append({'RecipeId':recipe_id,'Name':name,'Key':key,'Category':category.title(),'Tier':tier,
                         'IngredientBrief':ingredient,'Model':existing['Model'] if existing else ''.join(p.title() for p in key.split('_')),
                         'OutputId':existing['OutputId'] if existing else 'item.'+key,'Reused':key in REUSE,'HeartsPerServing':int(cols[4 if expansion else 3]) if category=='food' and cols[4 if expansion else 3].isdigit() else 0,
                         'FutureTierArtOnly':tier>4,'PrefabPath':'BalloonExclusive/Items/'+(existing['Model'] if existing else ''.join(p.title() for p in key.split('_')))})
    assert len(rows)==42 and len({x['RecipeId'] for x in rows})==42
    assert sum(x['Category']=='Food' for x in rows)==33
    for row in rows:
        row['Kind']='PreparedFood' if row['Category']=='Food' else 'Misc'
        row['Consumable']=row['Category']=='Food'
        match=re.search(r'->\s*(\d+)',row['IngredientBrief']);row['Yield']=int(match[1]) if match else 1
    # Filling and bottling are separate unlocks, not an automatic recipe pair grant.
    drinks=(ROOT/'PROJECT_RECOMENDATIONS.md').read_text(encoding='utf-8').split('### 9.4.5 Oak-barrel drinks',1)[1].split('### 9.5',1)[0]
    drink_count=0
    for line in drinks.splitlines():
        if not line.startswith('|'):continue
        cols=[x.strip() for x in line.strip('|').split('|')]
        filling=re.search(r'`(recipe\.barrel\.([a-z_]+))`',cols[1]) if len(cols)>2 else None
        if not filling:continue
        bottling=re.search(r'`(recipe\.food\.([a-z_]+))`',cols[2]);assert bottling and bottling[2]==filling[2]
        key=filling[2];drink_count+=1
        for barrel in (True,False):
            model=''.join(p.title() for p in key.split('_'))+('Barrel' if barrel else 'Bottle')
            rows.append({'RecipeId':filling[1] if barrel else bottling[1],'Name':cols[0]+(' barrel' if barrel else ' bottle'),
                         'Key':key+('_barrel' if barrel else ''),'DrinkKey':key,'Kind':'FilledBarrel' if barrel else 'BottledDrink',
                         'Category':'Barrel' if barrel else 'Food','Tier':4,
                         'IngredientBrief':cols[1].split('`')[-1].strip(': ')+' -> 1 filled barrel' if barrel else '1 '+cols[0]+' barrel + 8 Glass bottles -> 8 '+cols[0],
                         'Model':model,'OutputId':'item.'+key+('_barrel' if barrel else ''),'Reused':False,'FutureTierArtOnly':False,
                         'PrefabPath':'BalloonExclusive/Items/'+model,'Yield':1 if barrel else 8,'Consumable':not barrel,
                         'HeartsPerServing':0 if barrel else int(cols[3]),'RequiredRecipeId':'' if barrel else filling[1],
                         'PrerequisiteBrief':'Known Oak plank recipe and discovered exact ingredient sources'+('' if barrel else '; also know this drink\'s filling recipe')})
    assert drink_count==9 and len(rows)==60 and len({x['RecipeId'] for x in rows})==60
    return rows

def ingredient_mark(m,key,c,s=.05):
    x,y,z=c
    if key=='apple_cider':
        ell(m,(x-s*.3,y,z),(s*.72,s*.8,s*.20),12,9,5);ell(m,(x+s*.3,y,z),(s*.72,s*.8,s*.20),13,9,5)
        tube(m,[(x,y+s*.6,z),(x+s*.15,y+s*1.1,z)],s*.10,2)
    elif key=='strawberry_wine':
        ell(m,(x,y,z),(s*.85,s,s*.2),25,10,5)
        for side in (-1,0,1):tube(m,[(x,y+s*.75,z-.004),(x+side*s*.7,y+s*1.0,z-.004)],s*.13,12)
        for dx,dy in [(-.3,.15),(.3,.2),(0,-.4)]:ell(m,(x+dx*s,y+dy*s,z-s*.2),(s*.08,s*.1,s*.06),6,6,3)
    elif key in ('blackberry_wine','blueberry_wine'):
        for i in range(7):
            a=i*2.4;r=s*(.25+.35*(i%3)/2);ell(m,(x+r*math.cos(a),y+r*math.sin(a),z),(s*.38,s*.38,s*.25),24 if key.startswith('black') else 31,8,4)
    elif key=='honey_mead':
        for dx,dy in [(-.4,0),(.4,0),(0,.65)]:
            ell(m,(x+dx*s,y+dy*s,z),(s*.42,s*.42,s*.18),29,6,3)
        tube(m,[(x+s*.4,y-s*.4,z),(x+s*.9,y-s*.1,z)],s*.12,13)
    elif key in ('barley_ale','wheat_beer'):
        tube(m,[(x,y-s,z),(x,y+s,z)],s*.08,3)
        for i in range(4):
            for side in (-1,1):ell(m,(x+side*s*.28,y+s*(-.5+i*.38),z),(s*.27,s*.17,s*.14),6 if key=='wheat_beer' else 22,7,4)
    elif key=='cane_rum':
        for side in (-1,0,1):
            tube(m,[(x+side*s*.45,y-s,z),(x+side*s*.45+s*.25,y+s,z)],s*.15,13)
            for yy in (-.5,.2,.7):box(m,(x+side*s*.45+s*.15,y+yy*s,z-s*.05),(s*.32,s*.10,s*.22),6)
    elif key=='corn_whiskey':
        ell(m,(x,y,z),(s*.52,s,s*.23),30,10,5)
        for i in range(5):tube(m,[(x-s*.35,y+s*(-.7+i*.32),z-s*.23),(x+s*.35,y+s*(-.7+i*.32),z-s*.23)],s*.045,22)
        tube(m,[(x-s*.8,y-s*.7,z),(x-s*.65,y+s*.45,z)],s*.10,16)

def filled_barrel(m,key):
    n=18;profiles=[(-.39,.225),(-.35,.25),(-.22,.28),(0,.30),(.22,.28),(.35,.25),(.39,.225)];rings=[]
    for y,r in profiles:rings.append([m.v((r*math.cos(i*math.tau/n),y,r*math.sin(i*math.tau/n))) for i in range(n)])
    for j in range(len(rings)-1):
        for i in range(n):
            k=(i+1)%n;color=1+i%3
            m.tri(rings[j][i],rings[j+1][k],rings[j][k],color);m.tri(rings[j][i],rings[j+1][i],rings[j+1][k],color)
    for end in (0,len(rings)-1):
        center=m.v((0,profiles[end][0],0))
        for i in range(n):
            k=(i+1)%n
            if end==0:m.tri(center,rings[end][i],rings[end][k],2)
            else:m.tri(center,rings[end][k],rings[end][i],3)
    # Exactly two visibly heavy iron hoops, matching the common filling brief.
    for y in (-.255,.255):
        rr=.274
        for i in range(n):
            a=i*math.tau/n;aa=(i+1)*math.tau/n
            ids=[m.v(p) for p in [(rr*math.cos(a),y-.035,rr*math.sin(a)),(rr*math.cos(aa),y-.035,rr*math.sin(aa)),(rr*math.cos(aa),y+.035,rr*math.sin(aa)),(rr*math.cos(a),y+.035,rr*math.sin(a))]]
            m.tri(ids[0],ids[2],ids[1],17);m.tri(ids[0],ids[3],ids[2],17)
            ell(m,(rr*math.cos(a),y,rr*math.sin(a)),(.009,.009,.009),18,6,3)
    for i in range(n):
        a=i*math.tau/n;tube(m,[(r*math.cos(a),y,r*math.sin(a)) for y,r in profiles],.0025,0)
    for x in (-.13,-.065,0,.065,.13):
        length=2*math.sqrt(.22*.22-x*x);tube(m,[(x,.392,-length/2),(x,.392,length/2)],.002,1)
    m.cylinder((.10,.393,.055),(.10,.418,.055),.032,.032,10,1,3)
    # Paper stamp + ingredient relief, with merchant-red wax mark beside it.
    box(m,(0,0,-.302),(.25,.21,.015),28);ingredient_mark(m,key,(0,.005,-.32),.059)
    ell(m,(.17,.12,-.252),(.035,.037,.014),25,10,5)
    if key=='corn_whiskey':
        for x in (-.15,-.08,.10,.17):tube(m,[(x,-.16,-.275),(x+.015,-.06,-.29)],.012,16)

def bottled_drink(m,key):
    wine=key.endswith('wine');beer=key in ('barley_ale','wheat_beer')
    radius=.15 if wine else .18 if beer else .19
    color=12 if wine else 22 if beer else 29 if key=='honey_mead' else 2 if key in ('cane_rum','corn_whiskey') else 13
    height=.43 if wine else .36 if key=='wheat_beer' else .30
    profiles=[(-.35,radius*.84),(-.32,radius),(.04,radius),(.11,radius*.85),(.18,.061),(height,.055),(height+.025,.068)]
    if key=='honey_mead':profiles=[(-.31,.11),(-.27,.18),(-.10,.22),(.06,.19),(.17,.065),(.30,.058),(.32,.07)]
    sides=8 if key=='corn_whiskey' else 16;rings=[]
    for y,r in profiles:rings.append([m.v((r*math.cos(i*math.tau/sides),y,r*math.sin(i*math.tau/sides))) for i in range(sides)])
    for j in range(len(rings)-1):
        for i in range(sides):
            k=(i+1)%sides;m.tri(rings[j][i],rings[j+1][k],rings[j][k],color);m.tri(rings[j][i],rings[j+1][i],rings[j+1][k],color)
    for end in (0,len(rings)-1):
        center=m.v((0,profiles[end][0],0))
        for i in range(sides):
            k=(i+1)%sides
            if end==0:m.tri(center,rings[end][i],rings[end][k],color)
            else:m.tri(center,rings[end][k],rings[end][i],color)
    top=profiles[-1][0]
    m.cylinder((0,top,0),(0,top+.035,0),.070 if beer else .052,.070 if beer else .052,12,17 if beer else 1,18 if beer else 3)
    if not beer:m.cylinder((0,top-.045,0),(0,top+.002,0),.071,.071,14,24,25)
    # Curved paper label follows the glass front instead of floating off the bottle.
    def body_radius(y):
        for (a,r0),(c,r1) in zip(profiles,profiles[1:]):
            if a<=y<=c:return r0+(r1-r0)*(y-a)/(c-a)
        return profiles[0][1] if y<profiles[0][0] else profiles[-1][1]
    label=[]
    for y in sorted(set([-.21,-.17,-.13,-.10,-.05,-.025]+[y for y,r in profiles if -.21<y<-.025])):
        rr=body_radius(y)+.007
        label.append([m.v((rr*math.sin(math.radians(-48+i*96/12)),y,-rr*math.cos(math.radians(-48+i*96/12)))) for i in range(13)])
    for a,c in zip(label,label[1:]):
        for i in range(12):m.tri(a[i],c[i+1],a[i+1],28);m.tri(a[i],c[i],c[i+1],28)
    ingredient_mark(m,key,(0,-.115,-body_radius(-.115)-.022),.048)
    # Faceted opaque glass: a restrained baked glint, no transparency sorting dependency.
    glint_y=sorted(set([-.28,.015]+[y for y,r in profiles if -.28<y<.015]))
    tube(m,[(-body_radius(y)*.65,y,-body_radius(y)*.82) for y in glint_y],.006,28)
    if key=='honey_mead':loop(m,(.215,.075,0),.075,.10,.018,color)

def tube(m,pts,r,k,sides=6):
    for a,c in zip(pts,pts[1:]):
        start=len(m.faces);m.cylinder(a,c,r,r,sides,k,k);m.faces[start:]=[(i,j,l,k) for i,j,l,_ in m.faces[start:]]

def ell(m,c,r,k,n=10,rings=5):sphere(m,c,r,k,n,rings)
def loop(m,c,rx,ry,r,k,plane='xy',steps=18):
    tube(m,[(c[0]+rx*math.cos(i*math.tau/steps),c[1]+(ry*math.sin(i*math.tau/steps) if plane=='xy' else 0),c[2]+(ry*math.sin(i*math.tau/steps) if plane=='xz' else 0)) for i in range(steps+1)],r,k)

def leaf(m,c,scale=1,k=13):
    x,y,z=c;ids=[m.v((x+dx*scale,y+dy*scale,z+dz*scale)) for dx,dy,dz in [(-.07,0,0),(0,.012,-.035),(.075,0,0),(0,.012,.035),(0,.027,0)]]
    for i in range(4):m.tri(ids[i],ids[(i+1)%4],ids[4],k);m.tri(ids[(i+1)%4],ids[i],ids[4],k)

def bowl(m,fill=6,pot=False):
    vessel(m,.40,.30,20 if pot else 19,fill)
    if pot:
        for x in (-.46,.46):loop(m,(x,.015,0),.085,.10,.021,20)
    loop(m,(0,.158,0),.40,.40,.014,21,'xz',24)

def grain(m,k=6,count=28,y=.17):
    for i in range(count):
        a=i*2.399;r=.05+.25*math.sqrt((i+.5)/count)
        ell(m,(r*math.cos(a),y+.006*(i%3),r*math.sin(a)),(.032,.016,.018),k+(i%3==0),7,4)

def chunks(m,k,count=7,y=.20,size=.075):
    for i in range(count):
        a=i*2.4;r=.08+.18*(i%3)/2;ell(m,(r*math.cos(a),y,r*math.sin(a)),(size,.045,size*.8),k,7,4)

def plate(m):
    m.cylinder((0,-.09,0),(0,-.065,0),.47,.47,20,19,19)
    loop(m,(0,-.06,0),.45,.45,.012,21,'xz',24)

def shell_fan(m,c,scale=1,k=28,rough=False):
    x,y,z=c;center=m.v((x,y,z+.12*scale));edge=[]
    for i in range(17):
        a=math.pi*i/16;r=scale*(.28+(.02*math.sin(i*6) if rough else 0))
        edge.append(m.v((x+r*math.cos(a),y+.025*scale*math.sin(a),z+.12*scale-r*math.sin(a))))
    for i in range(16):m.tri(center,edge[i],edge[i+1],k+i%2)
    # Back surface prevents a paper-thin shell in icon/three-quarter views.
    back=m.v((x,y-.035*scale,z))
    for i in range(16):m.tri(back,edge[i+1],edge[i],k)
    m.tri(center,back,edge[0],k);m.tri(center,edge[-1],back,k)
    for i in range(1,16,2):
        target=m.vertices[edge[i]-1];tube(m,[(x,y+.003,z+.12*scale),((target[0]+x)/2,y+.035*scale,(target[2]+z+.12*scale)/2),target],.004*scale,k+1)

def seafood(m,kind,c=(0,.14,0),scale=1):
    temp=b.Model('Seafood');x,y,z=c
    if kind=='crab':
        ell(temp,(0,0,0),(.22,.075,.16),21,12,5)
        for side in (-1,1):
            for i in range(4):tube(temp,[(side*.15,0,(i-1.5)*.065),(side*(.27+i*.012),-.025,(i-1.5)*.10),(side*(.36+i*.009),-.045,(i-1.5)*.13)],.018,22)
            tube(temp,[(side*.17,.015,-.10),(side*.30,.015,-.22)],.035,21)
            ell(temp,(side*.33,.015,-.27),(.075,.042,.10),22)
            for spread in (-1,1):tube(temp,[(side*.33,.015,-.28),(side*.33+spread*.042,.01,-.36)],.019,22)
    elif kind in ('squid','octopus'):
        if kind=='squid':ell(temp,(0,.035,.08),(.12,.065,.24),27,12,5)
        else:ell(temp,(0,.08,.02),(.14,.15,.14),21,12,6)
        for i in range(8):
            a=(i/7-.5)*math.pi if kind=='squid' else i*math.tau/8
            pts=[]
            for j in range(10):
                t=j/9;r=.07+.30*t;aa=a+.7*t
                pts.append((r*math.sin(aa),-.015+.013*math.sin(t*math.pi),(-.10 if kind=='squid' else 0)-r*math.cos(aa)))
            tube(temp,pts,.016 if kind=='squid' else .025,22 if kind=='octopus' else 27)
            if kind=='octopus':
                for j in (3,5,7):ell(temp,(pts[j][0],pts[j][1]+.018,pts[j][2]),(.012,.006,.012),27,6,3)
    elif kind=='lobster':
        ell(temp,(0,0,-.04),(.13,.07,.19),21,12,5)
        for i in range(5):ell(temp,(0,-.005,.13+i*.052),(.12-i*.014,.055,.045),22-i%2)
        for side in (-1,1):
            tube(temp,[(side*.09,0,-.08),(side*.23,.01,-.16),(side*.28,.025,-.28)],.028,21)
            ell(temp,(side*.29,.03,-.32),(.075,.048,.11),21)
            tube(temp,[(side*.30,.035,-.36),(side*.33,.035,-.45)],.018,22)
            for i in range(3):tube(temp,[(side*.09,0,i*.06),(side*.23,-.025,i*.07+.025)],.012,22)
        for side in (-1,1):tube(temp,[(side*.05,.03,-.16),(side*.13,.05,-.34),(side*.17,.045,-.45)],.006,22)
    elif kind in ('scallop','oyster'):
        shell_fan(temp,(0,-.035,0),1,28 if kind=='scallop' else 18,kind=='oyster')
        ell(temp,(0,.0,-.015),(.095,.045,.085),28 if kind=='scallop' else 27,12,5)
        if kind=='scallop':ell(temp,(0,.027,-.018),(.064,.018,.058),22,12,4)
        else:ell(temp,(.025,.028,-.015),(.055,.012,.036),14,8,4)
    elif kind=='mussel':
        for side in (-1,1):
            xx=side*.12
            ell(temp,(xx,-.015,0),(.105,.037,.22),16,12,5)
            ell(temp,(xx,.009,-.015),(.08,.015,.17),18,12,4)
            ell(temp,(xx,.026,-.015),(.054,.025,.12),22,10,4)
            ell(temp,(xx,.13,.14),(.10,.12,.04),16,10,5)
    else:raise ValueError(kind)
    start=len(m.vertices)
    for a,b0,c0 in temp.vertices:m.v((x+a*scale,y+b0*scale,z+c0*scale))
    m.faces += [(a+start,b0+start,c0+start,k) for a,b0,c0,k in temp.faces]

def food(m,key):
    if key.startswith('roasted_'):
        kind=key.removeprefix('roasted_');plate(m);seafood(m,kind,(0,.06,0),.80)
        # Charred cooking support is illustrative; exact fuel costs remain in the recipe record.
        for x in (-.19,.19):tube(m,[(x,-.03,-.36),(x,-.03,.36)],.020,16)
    elif key=='honey_blackberry_biscuits':
        plate(m)
        for x in (-.19,.19):
            ell(m,(x,.01,0),(.16,.065,.15),22,12,5)
            for i in range(4):ell(m,(x+.05*math.cos(i*2.4),.062,.05*math.sin(i*2.4)),(.038,.025,.03),24,7,4)
            tube(m,[(x-.10,.074,-.07),(x+.06,.076,0),(x-.06,.074,.07)],.008,29)
    elif 'flatbread' in key:
        plate(m);ell(m,(0,.0,0),(.40,.047,.31),22,16,5)
        for i in range(9):
            a=i*2.4;r=.06+.20*(i%3)/2;box(m,(r*math.cos(a),.05,r*math.sin(a)),(.065,.021,.06),28)
        if key.startswith('mint'):
            for i in range(5):leaf(m,(.20*math.cos(i*1.4),.07,.16*math.sin(i*1.4)),.65)
        else:chunks(m,0,7,.075,.038)
    elif key=='cacao_coconut_tart':
        vessel(m,.39,.18,22,0)
        for i in range(22):
            a=i*2.4;r=.25*math.sqrt((i+.5)/22);box(m,(r*math.cos(a),.093,r*math.sin(a)),(.040,.012,.016),28)
    elif key=='coconut_cacao_pudding':
        vessel(m,.36,.32,0,0)
        for i in range(15):
            a=i*2.4;r=.18*math.sqrt((i+.5)/15);box(m,(r*math.cos(a),.143,r*math.sin(a)),(.055,.012,.02),28)
    else:
        pot=key.endswith(('_pot','_hotpot','_chili'));bowl(m,27 if any(x in key for x in ('cream','chowder')) else 6,pot)
        if any(x in key for x in ('rice','barley','rye','oats','millet','sorghum','buckwheat','amaranth')):
            grain(m,28 if 'rice' in key else 6,22)
        shell=next((x for x in ('crab','squid','lobster','scallop','mussel','octopus','oyster') if x in key),None)
        if shell:seafood(m,shell,(0,.22,0),.57)
        elif 'blackberry' in key:
            for i in range(6):ell(m,(.19*math.cos(i*1.3),.205,.17*math.sin(i*1.3)),(.052,.035,.048),24,8,4)
            tube(m,[(-.18,.23,-.06),(.17,.24,.08),(-.10,.23,.16)],.009,29)
        elif 'pumpkin' in key:
            chunks(m,26,6,.21,.07)
            for i in range(12):ell(m,(.25*math.cos(i*2.4),.205,.20*math.sin(i*2.4)),(.034,.025,.033),6,7,4)
            for x in (-.12,.08,.20):box(m,(x,.26,0),(.055,.025,.045),28)
        else:
            chunks(m,20 if any(x in key for x in ('turkey','boar','beaver','venison','goat','buffalo','bear')) else 27,6,.205,.067)
        if any(x in key for x in ('bean','chickpea','chili')):
            for i in range(10):ell(m,(.23*math.cos(i*2.4),.216,.22*math.sin(i*2.4)),(.036,.024,.026),20 if 'bean' in key else 6,7,4)
        if 'potato' in key:chunks(m,28,4,.232,.055)
        if 'pear' in key:chunks(m,15,4,.24,.058)
        if 'apricot' in key:chunks(m,26,4,.24,.06)
        if 'date' in key:chunks(m,0,5,.23,.045)
        if 'truffle' in key:chunks(m,0,5,.24,.038)
        if 'carrot' in ('carrot' if key in ('creamy_crab_soup','beaver_barley_hotpot') else ''):chunks(m,26,4,.25,.035)
        if any(x in key for x in ('mint','thyme','tarragon','seaweed','chili')) or key in ('lobster_rice_bowl','squid_rice_bowl','mussel_buckwheat_bowl','octopus_millet_bowl'):
            for i in range(3):leaf(m,(.19*math.cos(i*2.4),.265,.17*math.sin(i*2.4)),.65,12 if key=='lobster_rice_bowl' else 13)
        if key=='goat_apricot_pot':
            for i in range(8):ell(m,(.24*math.cos(i*2.4),.25,.20*math.sin(i*2.4)),(.019,.008,.008),14,6,3)

def plaque(m,k=2,oval=False):
    if oval:ell(m,(0,-.06,0),(.28,.32,.045),k,16,6)
    else:box(m,(0,-.06,.02),(.52,.55,.075),k)

def horn(m,kind):
    if kind=='goat':points=[(-.10,-.09,-.08),(-.19,.02,-.09),(-.21,.20,-.08),(-.12,.31,-.07),(.03,.28,-.07),(.10,.17,-.07),(.055,.11,-.07)]
    elif kind=='buffalo':points=[(-.25,.23,-.09),(-.22,.07,-.09),(-.10,-.02,-.11),(.04,.0,-.11),(.16,.12,-.09),(.21,.28,-.09)]
    else:points=[(-.12,.24,-.09),(-.06,.15,-.10),(.025,.05,-.13),(.09,-.11,-.13),(.055,-.18,-.13)]
    for i,(a,c) in enumerate(zip(points,points[1:])):
        before=len(m.faces);m.cylinder(a,c,.055*(1-i/len(points)*.7),.055*(1-(i+1)/len(points)*.7),10,3,3);m.faces[before:]=[(a0,b0,c0,3 if kind!='claw' else 19) for a0,b0,c0,_ in m.faces[before:]]

def misc(m,key):
    if key=='birch_wax_bee':
        box(m,(0,-.23,0),(.60,.05,.36),19)
        ell(m,(0,0,0),(.24,.11,.12),29,12,6);ell(m,(.21,.025,-.015),(.10,.09,.09),29)
        box(m,(0,-.17,0),(.10,.12,.12),19)
        for x in (-.12,0,.10):tube(m,[(x,.112*math.cos(i*math.tau/18),.122*math.sin(i*math.tau/18)) for i in range(19)],.015,0)
        for side in (-1,1):ell(m,(-.04+side*.085,.13,.055),(.10,.13,.02),8,12,4)
        for z in (-.055,.055):ell(m,(.28,.05,z),(.012,.016,.012),16,6,4)
        for z in (-.05,.05):tube(m,[(.23,.09,z),(.26,.18,z*1.5)],.006,0)
    elif key=='painted_turkey':
        box(m,(0,-.24,0),(.53,.05,.32),1)
        ell(m,(0,-.03,0),(.20,.19,.12),22,12,6);ell(m,(.16,.17,-.03),(.085,.10,.075),23)
        for i in range(9):
            a=(i/8-.5)*math.pi*.9
            ell(m,(-.12+.27*math.sin(a),.02+.28*math.cos(a),.10),(.06,.16,.035),2+i%2,8,4)
        tube(m,[(.21,.19,-.09),(.29,.18,-.09)],.023,23)
        for x in (-.08,.08):tube(m,[(x,-.15,0),(x,-.22,0)],.015,2)
    elif key=='silver_sun_brooch':
        loop(m,(0,0,.035),.32,.32,.016,5)
        for i in range(12):
            a=i*math.tau/12;p=[m.v((r*math.cos(t),r*math.sin(t),-.035)) for r,t in [(.20,a-.10),(.34,a),(.20,a+.10)]]
            m.tri(*p,19);m.tri(*reversed(p),19)
        ell(m,(0,0,0),(.22,.22,.045),18,16,6);ell(m,(0,0,-.055),(.11,.11,.035),31,10,5)
    elif key=='obsidian_boar':
        box(m,(0,-.25,0),(.70,.07,.35),1)
        ell(m,(-.04,-.01,0),(.28,.17,.14),16,9,4);ell(m,(.24,-.02,-.01),(.15,.12,.11),17,8,4)
        box(m,(.35,-.04,-.01),(.12,.09,.14),16)
        for x in (-.19,.13):
            for z in (-.09,.09):box(m,(x,-.18,z),(.07,.13,.07),16)
        for z in (-.08,.08):
            ell(m,(.20,.10,z),(.035,.08,.025),16,7,4)
            tube(m,[(.28,-.04,z),(.35,-.02,z*1.5),(.32,.04,z*1.5)],.013,18)
        tube(m,[(-.28,.01,0),(-.36,.08,.02),(-.34,.11,.045)],.015,16)
    elif key=='deer_antler_maple_token':
        plaque(m,2,True);loop(m,(0,.31,.02),.07,.10,.014,5)
        tube(m,[(-.09,-.20,-.07),(-.10,.01,-.08),(-.04,.20,-.08),(.07,.32,-.08)],.025,28)
        for a,c in [((-.10,-.02,-.08),(-.22,.07,-.08)),((-.08,.08,-.08),(-.18,.20,-.08)),((-.035,.20,-.08),(-.07,.33,-.08)),((.025,.27,-.08),(.16,.30,-.08))]:tube(m,[a,c],.017,28)
    elif key in ('goat_horn_silk_charm','buffalo_horn_indigo_token','bear_claw_lotus_charm'):
        plaque(m,0 if key.startswith('bear') else 1 if key.startswith('buffalo') else 3,True)
        horn(m,'claw' if key.startswith('bear') else 'buffalo' if key.startswith('buffalo') else 'goat')
        cloth=31 if key.startswith('buffalo') else 28 if key.startswith('goat') else 27
        for side in (-1,1):
            ell(m,(side*.10,-.19,-.12),(.13,.055,.018),cloth,8,4)
            box(m,(side*.07,-.29,-.085),(.08,.17,.023),cloth)
        loop(m,(0,.30,.01),.07,.11,.013,5)
    else:raise ValueError(key)

def export(row):
    m=b.Model(row['Model'])
    if row.get('Kind')=='FilledBarrel':filled_barrel(m,row['DrinkKey'])
    elif row.get('Kind')=='BottledDrink':bottled_drink(m,row['DrinkKey'])
    else:(food if row['Category']=='Food' else misc)(m,row['Key'])
    if row['Category']=='Food' and row.get('Kind')!='BottledDrink':
        a=math.radians(-32);m.vertices=[(x,y*math.cos(a)-z*math.sin(a),y*math.sin(a)+z*math.cos(a)) for x,y,z in m.vertices]
    lo=[min(p[i] for p in m.vertices) for i in range(3)];hi=[max(p[i] for p in m.vertices) for i in range(3)]
    m.vertices=[tuple(p[i]-(hi[i]+lo[i])/2 for i in range(3)) for p in m.vertices]
    assert all(all(math.isfinite(v) for v in p) for p in m.vertices)
    assert all(len({i,j,k})==3 and all(1<=v<=len(m.vertices) for v in (i,j,k)) for i,j,k,_ in m.faces)
    m.save();row['Triangles']=len(m.faces)

def main(new_only=False,only_kind=None):
    OUT.mkdir(parents=True,exist_ok=True);b.OUT=OUT;b.COLORS=COLORS;rows=recipes()
    b.palette('Tier01CraftedPalette.png')
    created=[]
    for row in rows:
        if row['Reused']:
            old=json.loads(LEGACY.read_text(encoding='utf-8'));row['Triangles']=next(x['Triangles'] for x in old['Models'] if x['Model']==row['Model'])
        elif (new_only or (only_kind and row.get('Kind')!=only_kind)) and (OUT/(row['Model']+'.obj')).exists():
            row['Triangles']=sum(line.startswith('f ') for line in (OUT/(row['Model']+'.obj')).read_text(encoding='utf-8').splitlines())
        else:export(row);created.append(row['RecipeId'])
    manifest={'Revision':2,'Items':rows,'FoodCount':sum(x['Category']=='Food' for x in rows),'MiscCount':sum(x['Category']=='Misc' for x in rows),
              'BarrelCount':sum(x['Category']=='Barrel' for x in rows),'ReusedCount':5,'NewCount':len(rows)-5,
              'Scope':'Finished-output art only; recipe knowledge has no physical scroll model; expansion tiers are not activated. Filled barrels are non-consumable.'}
    (OUT/'BalloonExclusiveManifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
    # Update only the five moved visual paths; recipe data and balance remain intact.
    old=json.loads(LEGACY.read_text(encoding='utf-8'));moved={x['RecipeId']:x for x in rows if x['Reused']}
    for recipe in old['Recipes']:
        if recipe['Id'] in moved:recipe['PrefabPath']=moved[recipe['Id']]['PrefabPath']
    LEGACY.write_text(json.dumps(old,indent=2)+'\n',encoding='utf-8')
    domain=ROOT/'Assets/_Game/Scripts/Domain/Items/Tier1CraftedItems.cs';code=domain.read_text(encoding='utf-8')
    for row in moved.values():code=code.replace('"Tier01/Crafted/'+row['Model']+'"','"'+row['PrefabPath']+'"')
    domain.write_text(code,encoding='utf-8')
    print(f'{len(rows)} exclusive outputs; {len(created)} source meshes written this run; {manifest["FoodCount"]} Food, {manifest["MiscCount"]} Misc, {manifest["BarrelCount"]} non-consumable barrels.')

if __name__=='__main__':
    import argparse
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--new-only',action='store_true',help='Preserve existing source meshes; create only missing outputs.')
    parser.add_argument('--only-kind',choices=['FilledBarrel','BottledDrink'],help='Rebuild this kind while retaining all other existing meshes.')
    args=parser.parse_args();main(args.new_only,args.only_kind)
