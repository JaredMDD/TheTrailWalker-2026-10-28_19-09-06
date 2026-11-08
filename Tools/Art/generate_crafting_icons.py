"""Recipe-book and return icons in the HUD silhouette style (user, October 31, 2026); stdlib only.

Run from Tools/Art: python generate_crafting_icons.py  (writes next to the other HUD icons).
"""
import generate_hud_icons as art

BLACK, GRAY, LIGHT, CLEAR = art.BLACK, art.GRAY, art.LIGHT, art.CLEAR

icons = {
    # Open book: two pages with ruled lines and a dark spine.
    'book': [
        art.poly([(10, 30), (62, 40), (62, 106), (10, 96)], BLACK),
        art.poly([(66, 40), (118, 30), (118, 96), (66, 106)], BLACK),
        art.poly([(17, 39), (56, 47), (56, 97), (17, 89)], LIGHT),
        art.poly([(72, 47), (111, 39), (111, 89), (72, 97)], LIGHT),
        art.rect(60, 38, 8, 70, GRAY, 3),
        art.poly([(23, 52), (50, 57), (50, 61), (23, 56)], GRAY), art.poly([(23, 64), (50, 69), (50, 73), (23, 68)], GRAY),
        art.poly([(23, 76), (50, 81), (50, 85), (23, 80)], GRAY),
        art.poly([(78, 57), (105, 52), (105, 56), (78, 61)], GRAY), art.poly([(78, 69), (105, 64), (105, 68), (78, 73)], GRAY),
        art.poly([(78, 81), (105, 76), (105, 80), (78, 85)], GRAY)],
    # Return: a U-turn arrow (arc over the top, arrowhead down on the left).
    'return': [
        art.circle(66, 58, 40, BLACK), art.circle(66, 58, 23, CLEAR),
        art.rect(14, 58, 100, 64, CLEAR),
        art.rect(89, 54, 17, 52, BLACK, 5),
        art.poly([(12, 54), (58, 54), (35, 88)], BLACK)],
    # Recipe-book category tabs (user, October 31, 2026).
    'cat_material': [  # log pile seen from the ends
        art.circle(40, 86, 24, BLACK), art.circle(88, 86, 24, BLACK), art.circle(64, 44, 24, BLACK),
        art.circle(40, 86, 12, LIGHT), art.circle(88, 86, 12, LIGHT), art.circle(64, 44, 12, LIGHT),
        art.circle(40, 86, 5, GRAY), art.circle(88, 86, 5, GRAY), art.circle(64, 44, 5, GRAY)],
    'cat_tool': [  # axe
        art.poly([(58, 30), (70, 30), (78, 116), (64, 116)], BLACK),
        art.poly([(68, 22), (104, 14), (112, 38), (104, 62), (68, 54)], BLACK),
        art.poly([(46, 28), (68, 22), (68, 54), (46, 48)], GRAY)],
    'cat_wearable': [  # shirt
        art.poly([(44, 20), (24, 28), (8, 52), (26, 64), (38, 50), (38, 112), (90, 112), (90, 50), (102, 64), (120, 52), (104, 28), (84, 20), (76, 32), (52, 32)], BLACK),
        art.poly([(52, 32), (64, 46), (76, 32)], LIGHT), art.rect(60, 54, 8, 52, GRAY, 3)],
    'cat_food': [  # apple
        art.circle(50, 74, 34, BLACK), art.circle(78, 74, 34, BLACK), art.rect(60, 18, 7, 26, GRAY, 3),
        art.poly([(68, 34), (94, 20), (86, 42)], GRAY), art.circle(44, 62, 8, LIGHT)],
    'cat_misc': [  # gem (sculptures, toys and jewelry)
        art.poly([(32, 36), (96, 36), (118, 60), (64, 114), (10, 60)], BLACK),
        art.poly([(32, 36), (50, 60), (10, 60)], GRAY), art.poly([(96, 36), (118, 60), (78, 60)], GRAY),
        art.poly([(50, 60), (78, 60), (64, 100)], LIGHT)],
    # Milestone 8 missions (October 31, 2026): the mission lady button is a rolled scroll with a seal; branch badges
    # for the selection list (Lumberjack reuses the axe tab icon).
    'mission': [
        art.rect(28, 24, 72, 80, BLACK, 4), art.rect(34, 30, 60, 68, LIGHT, 2),
        art.rect(16, 14, 96, 20, BLACK, 10), art.rect(16, 94, 96, 20, BLACK, 10), art.rect(22, 19, 84, 6, GRAY, 3), art.rect(22, 99, 84, 6, GRAY, 3),
        art.rect(42, 44, 44, 6, GRAY, 3), art.rect(42, 58, 44, 6, GRAY, 3), art.rect(42, 72, 28, 6, GRAY, 3),
        art.circle(82, 80, 10, GRAY), art.circle(82, 80, 4, BLACK)],
    # Camp map (October 31, 2026): camp buttons carry a campfire; the river crossing a ferry boat.
    'campfire': [
        art.poly([(14, 104), (24, 92), (112, 112), (104, 124)], BLACK), art.poly([(114, 104), (104, 92), (16, 112), (24, 124)], BLACK),
        art.poly([(64, 12), (88, 46), (94, 72), (84, 96), (64, 104), (44, 96), (34, 72), (40, 46)], BLACK),
        art.poly([(64, 46), (76, 66), (78, 84), (64, 96), (50, 84), (52, 66)], LIGHT), art.poly([(64, 66), (70, 80), (64, 90), (58, 80)], GRAY)],
    'ferry': [
        art.poly([(10, 72), (118, 72), (100, 100), (28, 100)], BLACK), art.rect(60, 22, 6, 52, BLACK, 2),
        art.poly([(68, 26), (104, 64), (68, 64)], GRAY), art.rect(6, 106, 116, 6, GRAY, 3), art.rect(18, 116, 92, 5, GRAY, 3)],
    'branch_gatherer': [  # woven basket with a handle
        art.circle(64, 50, 34, BLACK), art.circle(64, 50, 24, CLEAR), art.rect(20, 50, 88, 8, CLEAR),
        art.poly([(16, 58), (112, 58), (100, 112), (28, 112)], BLACK),
        art.rect(28, 70, 72, 6, LIGHT, 2), art.rect(32, 86, 64, 6, LIGHT, 2), art.rect(36, 100, 56, 5, GRAY, 2)],
    'branch_hunter': [  # paw print
        art.circle(64, 84, 26, BLACK), art.circle(32, 50, 12, BLACK), art.circle(52, 32, 12, BLACK),
        art.circle(76, 32, 12, BLACK), art.circle(96, 50, 12, BLACK), art.circle(64, 86, 10, GRAY)],
    'branch_explorer': [  # compass
        art.circle(64, 64, 50, BLACK), art.circle(64, 64, 40, LIGHT),
        art.poly([(64, 26), (74, 64), (54, 64)], BLACK), art.poly([(64, 102), (54, 64), (74, 64)], GRAY), art.circle(64, 64, 6, BLACK)],
}

if __name__ == '__main__':
    art.SVG.mkdir(parents=True, exist_ok=True)
    art.PNG.mkdir(parents=True, exist_ok=True)
    for name, shapes in icons.items():
        art.write_svg(name, shapes)
        art.raster(name, shapes)
        print(name, 'SVG + 256px RGBA PNG')
