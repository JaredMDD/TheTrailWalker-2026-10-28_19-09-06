"""Counter lamp (Milestone 14 phone round R2): a tier 1 settlement lantern that hangs under each counter's roof beam.
Dark wrought-iron frame (ceiling plate, two chain links, peaked cap, corner posts, base and drop finial) around four warm
amber panes. Two parts on the storefront palette strip: CounterLamp (iron, lit) and CounterLampGlass (the panes, drawn
unlit so they read as lit). Origin at the hang point (top of the plate); the lamp hangs down -Y, 0.50 m in all.
CounterLampAssets.Build() makes the prefab; CounterLamps.Hang places it and adds the light at LightY.
Run: python Tools/Art/generate_counter_lamp.py   (writes only the two lamp OBJs)
"""
from generate_storefront_meshes import pal, box, OUT, HEADER
import faceted_character_kit as k

LIGHT_Y = -.35   # glass centre: where the point light sits (CounterLampAssets.LightY)
DOWN = ((1, 0, 0), (0, -1, 0), (0, 0, 1))   # taper the -Y end

def frame():
    m = k.Mesh('CounterLamp', pal)
    box(m, (0, -.01, 0), (.07, .02, .07), 'Iron')                                       # ceiling plate
    box(m, (0, -.045, 0), (.035, .055, .012), 'Iron'); box(m, (0, -.09, 0), (.012, .055, .035), 'Iron')   # chain links
    box(m, (0, -.135, 0), (.05, .03, .012), 'Iron')                                      # hanging loop
    box(m, (0, -.19, 0), (.21, .07, .21), 'Iron', taper=.18)                             # peaked cap
    box(m, (0, -.235, 0), (.17, .02, .17), 'Soot')                                       # cap lip
    for sx in (-1, 1):
        for sz in (-1, 1): box(m, (sx*.066, LIGHT_Y, sz*.066), (.02, .21, .02), 'Iron')   # corner posts
    box(m, (0, -.455, 0), (.17, .03, .17), 'Soot')                                       # base
    box(m, (0, -.48, 0), (.08, .04, .08), 'Iron', axes=DOWN, taper=.25)                 # drop finial
    return m

def glass():
    m = k.Mesh('CounterLampGlass', pal)
    box(m, (0, LIGHT_Y, 0), (.124, .20, .124), 'GlowHot')
    return m

def main():
    stats = [part.save(OUT, HEADER) for part in (frame(), glass())]
    assert sum(s['triangles'] for s in stats) < 400, stats
    print('Counter lamp:', ', '.join(f"{s['name']} {s['triangles']}" for s in stats))

if __name__ == '__main__':
    main()
