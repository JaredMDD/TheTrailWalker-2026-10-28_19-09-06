"""Generate only tier-one loose-resource art; no scenes, recipe data or tests.

After generation, use The Trail Walker > Art > Rebuild Loose Resource Improvements.
The atlas migration updates encoded alpha in place while retaining palette RGB.
"""
from pathlib import Path
import runpy
import detail_textures as detail


def main():
    # Migrate before palette writers start emitting the new encoding.
    detail.migrate_legacy_palettes()
    directory = Path(__file__).resolve().parent
    for region in range(1, 7):
        runpy.run_path(str(directory/f'generate_region{region:02}_meshes.py'), run_name='__main__')
    import generate_tier01_crafted_items as crafted
    crafted.b.OUT = crafted.OUT
    crafted.b.COLORS = crafted.COLORS
    crafted.model('mushrooms', 'Mushrooms')
    detail.mushroom_palette()
    detail.build_atlas()
    for path, mapping in detail.PALETTES.items():
        # Other scene/character mappings retain their migrated values. Do not
        # rewrite palettes that another tool may be adjusting for terrain.
        if '/Art/Items/' in path:
            full = detail.ROOT/path
            detail.encode_palette(full, mapping)
    print('Generated loose-resource meshes, item palettes and 32-tile atlas. Testing deferred.')


if __name__ == '__main__':
    main()
