"""Tier-one art authoring data. Exact inputs; not a gameplay unlock service."""
RECIPES=[]
def add(key,name,category,inputs,yield_=1,seconds=None,hearts=0,style='',source='Normal mission; branch placement pending',existing='',note='',retired=False):
    RECIPES.append(dict(Id='recipe.'+category.lower()+'.'+key,Name=name,Category=category,
        OutputId=existing or ('ingredient.' if key in ('wheat_flour','cow_cream','cow_butter','cow_cheese','goat_cheese','mixed_milk_cheese','cow_yogurt') else 'item.')+key,
        Yield=yield_,BaseSeconds=seconds if seconds is not None else dict(Material=10,Tool=5,Wearable=15,Food=3,Misc=10)[category],
        Hearts=hearts,Style=style or key,Model=''.join(p.title() for p in key.split('_')),UnlockSource=source,Note=note,
        Inputs=[dict(ItemId=k,Quantity=v) for k,v in inputs],Tier=1))
    if retired:RECIPES[-1]['Retired']=True
B='material.pine_branch';L='material.pine_log';H='material.hemp';N='material.nettle_fiber';P='animal.rabbit.pelt';S='ingredient.rock_salt'
def mat(k,n,i,y=1,s=10,**kw):add(k,n,'Material',i,y,s,**kw)
mat('pine_log','Pine log',[(B,3)],s=0,existing=L,source='Gatherer G1; also gathered directly')
mat('pine_plank','Pine plank',[(L,3)],source='Gatherer G4')
mat('pine_charcoal','Pine charcoal',[(B,3)],2,source='Lumberjack L6')
mat('hemp_cord','Hemp cord',[(H,3)],2,5,source='Explorer E4')
mat('hemp_thread','Hemp thread',[(H,3)],3,5,source='Explorer E9')
mat('nettle_thread','Nettle thread',[(N,3)],3,5,source='Explorer E16')
mat('hemp_cloth','Hemp cloth',[(H,4)],2,source='Explorer E1')
mat('nettle_cloth','Nettle cloth',[(N,4)],2,source='Explorer E15')
mat('fine_hemp_cloth','Fine hemp cloth',[('item.hemp_cloth',3),('item.hemp_thread',1)],s=20,source='Explorer E10')
mat('cured_rabbit_pelt','Cured rabbit pelt',[(P,2),('ingredient.animal_fat',1)],s=15,source='Hunter H5',note='Revised November 7, 2026: Animal fat replaces Rock salt (fat-softened fur).')
mat('fine_rabbit_pelt','Fine rabbit pelt',[('item.cured_rabbit_pelt',2),('item.hemp_thread',1)],s=20,source='Hunter H6 after Explorer E9')
mat('rabbit_leather','Rabbit leather',[(P,2),(S,1)],s=15,source='Hunter H4')
mat('fine_rabbit_leather','Fine rabbit leather',[('item.rabbit_leather',2),('item.nettle_thread',1)],s=20,source='Hunter H7 after Explorer E16',note='Proposed fixed 20-second timer. Revised November 7, 2026: Nettle thread replaces Hemp thread.')
mat('plant_stuffing','Plant stuffing',[(N,3)],2,source='Hunter H8',note='Proposed fixed 10-second timer where unspecified.')
mat('pine_beam','Pine beam',[('item.pine_plank',4),('item.hemp_cord',1)],s=20,source='Lumberjack L4')
mat('pine_panel','Pine panel',[('item.pine_plank',3)],source='Lumberjack L11',note='Proposed fixed 10-second timer where unspecified.')
mat('limestone_tiles','Limestone tiles',[('material.limestone',3)],2,15,source='Lumberjack L8')
mat('polished_pebble','Polished pebble',[('material.pebbles',2),('material.flint',1)],source='Lumberjack L9',note='Proposed fixed 10-second timer where unspecified.')
mat('fired_bricks','Fired bricks',[('material.clay',3),('item.pine_charcoal',1)],3,source='Lumberjack L7',note='Proposed fixed 10-second timer where unspecified.')
mat('clay_cup','Clay cup',[('material.clay',2)],source='Gatherer T4-G9 (tier-four mission; not in the tier-one catalog yet)',note='Proposed fixed 10-second timer where unspecified.')
mat('wheat_flour','Wheat flour',[('ingredient.wheat',3)],2,5,source='Gatherer G6')
mat('reed_braid','Reed braid',[('material.reeds',3)],source='Gatherer G5',note='Proposed fixed 10-second timer where unspecified.')
mat('mortar','Mortar',[('material.sand',2),('material.limestone',1)],source='Lumberjack L5',note='Proposed fixed 10-second timer where unspecified.')
add('pine_axe_normal','Pine axe','Tool',[(B,2),('material.flint',2)],existing='tool.pine_axe.normal',source='Lumberjack L1',note='Normal only; base 10 uses.')
add('pine_slingshot_normal','Pine slingshot','Tool',[(B,2),(P,1)],source='Hunter H1',note='Normal only; base 6 uses.')
add('pine_rod_normal','Pine rod','Tool',[(B,2),('item.hemp_cord',2),('material.bone',1)],source='Explorer E5 (Bone) after E4',note='Normal only; base 10 uses; bone hook, no metal reel.')
for key,name,inputs,source,slot in [
 ('rabbit_hat','Rabbit-pelt hat',[(P,2)],'Gatherer G3 after Lumberjack L2','Hat'),
 ('rabbit_boots','Rabbit-pelt boots',[(P,2),(H,1)],'Lumberjack L3','Boots'),
 ('hemp_shirt','Hemp shirt',[('item.hemp_cloth',3),(P,1)],'Hunter H3','Shirt'),
 ('hemp_pants','Hemp pants',[('item.hemp_cloth',3),(B,1)],'Explorer E3','Pants'),
 ('starter_backpack','Starter backpack',[('item.hemp_cloth',3),('item.rabbit_leather',2),(B,2)],'Explorer E7 after Hunter H4','Backpack')]:
    add(key,name,'Wearable',inputs,source=source,note='Rank 1. Proposed base durability 4000 metres.');RECIPES[-1]['Slot']=slot
def food(k,n,i,y=1,h=2,s=3,**kw):add(k,n,'Food',i,y,s,h,**kw)
food('apple_juice','Apple juice',[('food.apples',3)],h=1,source='Gatherer G2')
food('roasted_mushrooms','Roasted mushrooms',[('ingredient.mushrooms',2),(B,1)],h=1,source='Explorer E6 after region 4 mushrooms')
food('berry_oat_cakes','Berry oat cakes',[('ingredient.oats',2),('food.berries',2)],2,1,source='Gatherer G12')
food('roasted_rabbit','Roasted rabbit',[('animal.rabbit.meat',1),(B,1)],source='Explorer E2')
food('herb_trout','Herb trout',[('fish.rainbow_trout',1),('ingredient.tea_herbs',1),(B,1)],source='Explorer E8 once farm Tea herbs are buyable')
# November 7, 2026: the plain roasts are retired (kept for saves that know them) and each species gets a different dish.
# Until their own models exist, each dish uses its species' roast mesh (style) as a stand-in.
for fish,key,name,extra,qty,src in [
 ('rainbow_trout','trout_mushroom_skillet','Trout mushroom skillet','ingredient.mushrooms',1,'Explorer E11'),
 ('largemouth_bass','salt_baked_bass','Salt-baked bass',S,2,'Explorer E12'),
 ('bluegill','pine_nut_bluegill','Pine-nut bluegill','ingredient.pine_nuts',1,'Explorer E13'),
 ('yellow_perch','apple_stuffed_perch','Apple-stuffed perch','food.apples',1,'Explorer E14')]:
    old='Roasted '+fish.replace('_',' ')
    food('roasted_'+fish,old,[('fish.'+fish,1),(B,1)],source='Retired November 7, 2026: replaced by '+name+'; kept for saves that know it',retired=True)
    food(key,name,[('fish.'+fish,1),(extra,qty),(B,1)],style='roasted_'+fish,source=src,note='November 7, 2026 fish-variety dish. Stand-in art: the '+old+' model until its own model exists.')
food('strawberry_cow_yogurt_drink','Strawberry cow-yogurt drink',[('ingredient.cow_yogurt',1),('food.strawberries',2)],h=3,source='Gatherer G11 after G10 (ranch milk, region 5)')
food('cow_cheese_tart','Cow-cheese tart',[('ingredient.wheat_flour',2),('ingredient.cow_cheese',1),('ingredient.egg',1)],2,5,source='Gatherer G15 (ranch milk and eggs)')
DAIRY=dict(cow_cream='Gatherer G13 (ranch milk, region 5)',cow_butter='Gatherer G14 (ranch milk, region 5)',cow_cheese='Gatherer G9 (ranch milk, region 5)',
 goat_cheese='No mission yet (open decision)',mixed_milk_cheese='No mission yet (open decision)',cow_yogurt='Gatherer G10 (ranch milk, region 5)')
for k,n,i,y,s in [
 ('cow_cream','Cow cream',[('food.milk.cow',3)],2,10),
 ('cow_butter','Cow butter',[('ingredient.cow_cream',2)],1,10),
 ('cow_cheese','Cow cheese',[('food.milk.cow',3),(S,1)],1,300),
 ('goat_cheese','Goat cheese',[('food.milk.goat',3),(S,1)],1,300),
 ('mixed_milk_cheese','Mixed-milk cheese',[('food.milk.cow',1),('food.milk.goat',1),(S,1)],1,300),
 ('cow_yogurt','Cow yogurt',[('food.milk.cow',2)],2,60)]:
    food(k,n,i,y,0,s,source=DAIRY[k],note='Food recipe/mastery, refined ingredient output; direct recovery not authored.'+(' Optional recipe proposal.' if k=='mixed_milk_cheese' else ''))
food('toasted_pine_nuts','Toasted pine nuts',[('ingredient.pine_nuts',2),(B,1)],s=20,existing='item.toasted_pine_nuts',source='Gatherer G8')
BOX='Optional balloon recipe; exact ingredient sources must be discovered and accessible'
food('roasted_shrimp','Roasted shrimp',[('ingredient.shrimp',1),(B,1)],h=8,source=BOX)
food('shrimp_pie','Shrimp pie',[('ingredient.shrimp',1),('ingredient.wheat_flour',1),('ingredient.cow_butter',1)],h=10,source=BOX)
food('apple_oat_compote','Apple-oat compote',[('food.apples',3),('ingredient.oats',2),('food.milk.cow',1)],h=10,source=BOX)
food('red_berry_oat_porridge','Red-berry oat porridge',[('food.berries',3),('ingredient.oats',2),('food.milk.cow',1)],h=10,source=BOX)
food('premium_apple_pie','Premium apple pie',[('ingredient.wheat_flour',2),('food.apples',3),('ingredient.cow_butter',1)],h=12,source='Bakery replacement recipe offer: 1% proposal; exactly one knowledge unlock, never a stack',note='8 gold per serving; 7200 gold per 1000; recipe 80 gold. Source gates apply before random selection.')
for k,n,i,src in [
 ('pine_bird','Pine bird sculpture',[(L,2)],'Lumberjack L2'),
 ('cord_charm','Cord charm',[('item.polished_pebble',1),('item.hemp_cord',1)],'Lumberjack L10'),
 ('rabbit_toy','Rabbit toy',[(P,2),(H,1)],'Hunter H2'),
 ('pinecone_ornament','Pine-cone ornament',[('material.pine_cone',2),(B,1)],'Gatherer G7'),
 ('pinecone_rabbit','Pine-cone rabbit ornament',[('material.pine_cone',1),(L,1),(H,1)],BOX)]:add(k,n,'Misc',i,source=src)

# No extra loose-resource slots: these are source declarations, not enabled inventories.
EXTERNAL=[
 dict(Id='food.milk.cow',Name='Cow milk',Model='CowMilk',Source='Ranch; require discovered settlement and reachable exact listing'),
 dict(Id='food.milk.goat',Name='Goat milk',Model='GoatMilk',Source='Ranch; require discovered settlement and reachable exact listing'),
 dict(Id='ingredient.egg',Name='Egg',Model='Egg',Source='Ranch; require discovered settlement and reachable exact listing'),
 dict(Id='ingredient.mushrooms',Name='Mushrooms',Model='Mushrooms',Source='Exact loose source discovery (region 4)'),
 dict(Id='ingredient.shrimp',Name='Shrimp',Model='Shrimp',Source='Tier-one fishery cash listing only; never a region fishing drop')]
