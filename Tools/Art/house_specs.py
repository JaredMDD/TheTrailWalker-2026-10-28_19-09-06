"""House art specification. Grid sizes and progression follow recommendations 13.1.
Dimensions, colours and massing are the approved design plan's authoring defaults.
No upgrade prices, neighbour simulation or gameplay data is generated here.
"""
# Depth growth (user, November 7, 2026): every level is one storey. Levels 1-3 widen (8, 10, 12 columns); every level
# adds furnishing rows toward the BACK of the house, so the exterior grows deeper, not taller. GridDepth rows run from
# the front (row 0, nearest the camera) to the back wall. Depth = rows x 0.5 m + 0.5 m walls + 0.8 m eaves/porch.
LEVELS = [
    dict(Level=1, Name='Small shed', Tier=1, GridWidth=8, GridDepth=5, Width=4.6, Depth=3.8, Height=2.9, Eaves=2.15, WallHeight=2.1, Wood='Pine', Wall='Pine', Roof='RoofClay', Chimneys=0),
    dict(Level=2, Name='Extended shed', Tier=1, GridWidth=10, GridDepth=6, Width=5.6, Depth=4.3, Height=3.4, Eaves=2.45, WallHeight=2.3, Wood='Pine', Wall='PineLight', Roof='RoofClay', Chimneys=0),
    dict(Level=3, Name='Proper wooden home', Tier=2, GridWidth=12, GridDepth=7, Width=6.8, Depth=4.8, Height=4.4, Eaves=2.95, WallHeight=2.6, Wood='Birch', Wall='Plaster', Roof='RoofEarth', Chimneys=1),
    dict(Level=4, Name='Back room', Tier=2, GridWidth=12, GridDepth=9, Width=6.8, Depth=5.8, Height=4.5, Eaves=3.0, WallHeight=2.6, Wood='Birch', Wall='Plaster', Roof='RoofEarth', Chimneys=1),
    dict(Level=5, Name='Long hall', Tier=3, GridWidth=12, GridDepth=11, Width=6.8, Depth=6.8, Height=4.6, Eaves=3.05, WallHeight=2.7, Wood='Mesquite', Wall='SandPlaster', Roof='RoofEarth', Chimneys=1),
    dict(Level=6, Name='Extended hall', Tier=3, GridWidth=12, GridDepth=13, Width=6.8, Depth=7.8, Height=4.7, Eaves=3.1, WallHeight=2.7, Wood='Mesquite', Wall='SandPlaster', Roof='Slate', Chimneys=2),
    dict(Level=7, Name='Frontier manor', Tier=4, GridWidth=12, GridDepth=15, Width=6.8, Depth=8.8, Height=4.8, Eaves=3.15, WallHeight=2.8, Wood='Oak', Wall='Plaster', Roof='Slate', Chimneys=2),
    dict(Level=8, Name='Finished homestead', Tier=4, GridWidth=12, GridDepth=17, Width=6.8, Depth=9.8, Height=4.9, Eaves=3.2, WallHeight=2.8, Wood='Oak', Wall='FinePlaster', Roof='Slate', Chimneys=2),
]

FURNITURE = [
    ['Worktable', 'Bedroll', 'Market sign', 'Boot bench', 'Sewing basket', 'Hat peg'],
    ['Trouser rack', 'Chopping block', 'Sling stand', 'Rod rack', 'Bag frame', 'Pine chest'],
    ['Loom', 'Clay cookstove', 'Display counter', 'Sofa', 'Sewing desk', 'Hat cabinet'],
    ["Tailor stool", 'Whetstone stand', 'Bow workbench', 'Tackle desk', 'Leather table', 'Birch chest'],
    ['Artisan bench', 'Pantry dresser', 'Trophy cabinet', 'Boot cabinet', 'Cloth press', 'Hat display'],
    ['Clothing armoire', 'Tool forge', 'Bow cabinet', 'Reel workshop', 'Pack-maker bench', 'Mesquite chest'],
    ['Master loom', 'Dining table', 'Gallery pedestal', 'Travel sofa', 'Fine-cloth workstation', 'Milliner bench'],
    ['Frontier wardrobe', 'Master tool station', 'Hunter cabinet', 'Angler cabinet', 'Expedition workbench', 'Reinforced oak chest'],
]

PALETTE = [
    ('Pine', '#9C744B', 'planks'), ('PineLight', '#B18B5C', 'planks'),
    ('Birch', '#A98B63', 'wood'), ('Mesquite', '#80503A', 'wood'), ('Oak', '#67472F', 'wood'),
    ('WoodDark', '#493323', 'wood'), ('Floor', '#987248', 'planks'), ('FloorDark', '#725236', 'planks'),
    ('Plaster', '#E0D3B5', 'plaster'), ('SandPlaster', '#CCBB98', 'plaster'), ('FinePlaster', '#E8DFCB', 'plaster'),
    ('Brick', '#9D6047', 'brick'), ('Stone', '#969080', 'stone'), ('Granite', '#807C75', 'mineral'),
    ('Basalt', '#4C5050', 'mineral'), ('Marble', '#CFCCC0', 'mineral'),
    ('RoofClay', '#8C4A37', 'shingle'), ('RoofEarth', '#6E513B', 'shingle'), ('Slate', '#53636A', 'shingle'),
    ('Green', '#637752', 'wood'), ('GreenLight', '#859267', 'wood'),
    ('Copper', '#AD7044', 'metal'), ('Iron', '#454441', 'metal'), ('Steel', '#89928F', 'metal'),
    ('Glass', '#668C96', 'glass'), ('GlassLight', '#A9C3C2', 'glass'), ('GlassDark', '#3B5961', 'glass'),
    ('Cloth', '#A66B47', 'cloth'), ('CreamCloth', '#C9BC95', 'cloth'), ('Fur', '#B49A76', 'fur'),
    ('Leather', '#7E563C', 'leather'), ('Ceramic', '#B87851', 'ceramic'),
    ('Amber', '#E4AE57', None), ('Grid', '#6D7869', None), ('Paper', '#DED2AE', 'paper'),
]

GRID_METRES = .5
PLOT_WIDTH = 11.2
PLOT_DEPTH = 10.6   # fits the 9.8 m level-8 house plus a rear margin
