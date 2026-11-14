using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Square icon buttons: BAG bottom-left, the user menu bottom-right opposite it. The menu opens a column above
    // itself (crafting, map, equipment, settings from the bottom up); the trash sits left of it while the bag is open.
    public sealed partial class CollectionPrototype
    {
        private const float HudSquare = 84f, HudGap = 10f, HudMargin = 12f;
        // Bottom bar line of the HUD squares above the bag opening; with the bag closed the opening sits at the bottom.
        private const float BarLineGap = 0.04f, ClosedBarLine = BarLineGap;
        private RectTransform userButton;
        private readonly List<RectTransform> menuButtons = new List<RectTransform>();
        private bool menuOpen;
        // Buttons that only work at a camp are hidden while walking a segment; the column closes the gap.
        private readonly HashSet<RectTransform> campOnlyButtons = new HashSet<RectTransform>();
        private float menuBarLine;
        private RectTransform mapMenuButton; // camp-only, but also kept in the settlement (settlement shell, section 3)
        private RectTransform craftMenuButton; // camp-only, but also shown inside the house (CraftAvailable, home revision 4)

        private void CreateUserMenu()
        {
            Text user = Button(safe, "MENU", 0, 0, 0, 0, ToggleUserMenu);
            userButton = (RectTransform)user.transform.parent; userButton.name = "User menu";
            HudIconStyle.Apply(user, "user");
            campOnlyButtons.Add(craftMenuButton = AddMenuButton("CRAFT", "crafting", () => { if (milestone5) ToggleCraftPanel(); else Say("Crafting needs the camp world."); }));
            campOnlyButtons.Add(mapMenuButton = AddMenuButton("MAP", "map", OpenMapFromMenu));
            AddMenuButton("GEAR", "equipment", () => { if (milestone4) ToggleGearPanel(); });
            AddMenuButton("SETTINGS", "settings", () => Say("Settings arrive in a later milestone."));
            SetMenu(false);
        }
        private RectTransform AddMenuButton(string text, string icon, UnityEngine.Events.UnityAction action)
        {
            Text label = Button(safe, text, 0, 0, 0, 0, action);
            HudIconStyle.Apply(label, icon);
            var button = (RectTransform)label.transform.parent;
            menuButtons.Add(button); return button;
        }
        private void ToggleUserMenu() { if (!(milestone4 && stamina != null && stamina.Fainted)) SetMenu(!menuOpen); }
        private void SetMenu(bool open)
        {
            menuOpen = open;
            int row = 1;
            foreach (RectTransform button in menuButtons)
            {
                bool show = open && (!campOnlyButtons.Contains(button) || (button == craftMenuButton ? CraftHere : AtCamp) || (button == mapMenuButton && phase == TravelPhase.Settlement));
                button.gameObject.SetActive(show);
                if (show) PlaceSquare(button, true, menuBarLine, 0, row++);
            }
        }
        private void OpenMapFromMenu()
        {
            if (!milestone5) { Say("The camp map arrives with camp travel."); return; }
            // In the settlement the map opens on the region list, read-only.
            if (phase != TravelPhase.Seated && phase != TravelPhase.Settlement) { Say("The map opens while seated at a camp."); return; }
            OpenMap();
        }

        // Every menu and interaction closes with the return icon in the bag button's place (user, November 5, 2026),
        // never a CLOSE/X/PUT AWAY text button. Menus shown with the bag (bag, equipment, crafting) turn the bag button
        // itself into the return button; full-screen menus (map, mission papers) use this shared one, which hides the
        // bag and user-menu buttons while it shows and disappears with its menu.
        private RectTransform screenReturn;
        private GameObject screenReturnOwner;
        private UnityEngine.Events.UnityAction screenReturnAction;
        private static RectTransform ReturnSquare(Transform parent, UnityEngine.Events.UnityAction action)
        {
            Text label = Button(parent, "RETURN", 0, 0, 0, 0, action);
            var rect = (RectTransform)label.transform.parent; rect.name = "Return";
            HudIconStyle.Apply(label, "return");
            return rect;
        }
        private void ShowScreenReturn(GameObject owner, UnityEngine.Events.UnityAction close)
        {
            if (screenReturn == null) screenReturn = ReturnSquare(safe, () => screenReturnAction?.Invoke());
            screenReturnOwner = owner; screenReturnAction = close;
            if (inventoryButton != null) inventoryButton.gameObject.SetActive(false);
            if (userButton != null) userButton.gameObject.SetActive(false);
            SetMenu(false);
            PlaceSquare(screenReturn, false, menuBarLine, 0, 0);
            screenReturn.gameObject.SetActive(true); screenReturn.SetAsLastSibling();
        }
        private void HideScreenReturn()
        {
            if (screenReturnOwner == null) return;
            screenReturnOwner = null; screenReturnAction = null;
            if (screenReturn != null) screenReturn.gameObject.SetActive(false);
            if (inventoryButton != null) inventoryButton.gameObject.SetActive(true);
            if (userButton != null) userButton.gameObject.SetActive(true);
        }
        // The shared return leaves with its menu, however the menu closed (a choice, a hand-off, travel). A harvest
        // hides the whole HUD and restores it itself, so nothing changes while one is showing.
        private void SyncScreenReturn()
        {
            if (screenReturnOwner != null && !screenReturnOwner.activeSelf && !(harvestRoot != null && harvestRoot.gameObject.activeSelf)) HideScreenReturn();
        }
        // The bag button: closes item details first, otherwise opens or closes the bag (and the menus shown with it).
        private void BagButton()
        {
            if (detailsPanel != null && detailsPanel.activeSelf) { detailsPanel.SetActive(false); return; }
            ToggleInventory();
        }

        // Rows are counted upward from the bottom bar line; columns inward from the screen edge.
        private static void PlaceSquare(RectTransform rect, bool right, float anchorY, int column, int row)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(right ? 1 : 0, anchorY);
            rect.pivot = new Vector2(right ? 1 : 0, 0);
            rect.sizeDelta = new Vector2(HudSquare, HudSquare);
            float x = HudMargin + column * (HudSquare + HudGap);
            rect.anchoredPosition = new Vector2(right ? -x : x, row * (HudSquare + HudGap));
        }
        private void LayoutSquares(float barLine)
        {
            PlaceSquare(inventoryButton, false, barLine, 0, 0);
            PlaceSquare(userButton, true, barLine, 0, 0);
            PlaceSquare(dropPocket, true, barLine, 1, 0);
            menuBarLine = barLine; SetMenu(menuOpen);
            LayoutMissionTracker(barLine);
        }
    }
}
