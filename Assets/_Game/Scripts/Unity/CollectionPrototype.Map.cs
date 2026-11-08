using System.Collections.Generic;
using System.Linq;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Camp map (section 12.3; look confirmed by the user, October 31, 2026): a real worn-out map, one themed sheet per
    // region. The road winds up the sheet from the first camp to the river; each camp is a button with a campfire icon
    // and its name, on the side of the road where the camp stands. Undiscovered camps show as inactive buttons.
    // The region's three featured resources head the sheet; other discovered regions are further pages.
    public sealed partial class CollectionPrototype
    {
        private RectTransform mapPage;
        private Image mapSheet;
        private string mapRegionId;
        private bool mapRegionList;
        private static readonly Color MapInk = new Color(0.27f, 0.17f, 0.08f), MapFaded = new Color(0.27f, 0.17f, 0.08f, 0.45f);

        private void CreateMapPanel()
        {
            mapPanel = Rect("Camp map", safe, 0.03f, 0.135f, 0.97f, 0.95f).gameObject;
            mapSheet = mapPanel.AddComponent<Image>();
            mapPage = Rect("Map page", mapPanel.transform, 0, 0, 1, 1);
            mapPanel.SetActive(false);
        }

        // Closing uses the shared return button in the bag button's place (user): the bag, user-menu and trash buttons hide.
        private void SetMapHud(bool open)
        {
            if (dropPocket != null && open) dropPocket.gameObject.SetActive(false);
            if (missionTrackerRect != null && open) missionTrackerRect.gameObject.SetActive(false);
            if (open) ShowScreenReturn(mapPanel, CloseMap); else if (screenReturnOwner == mapPanel) HideScreenReturn();
        }
        private void CloseMap()
        {
            if (mapPanel != null) mapPanel.SetActive(false);
            SetMapHud(false);
            RefreshMissionTracker();
        }

        private static Sprite MapSprite(int region)
        {
            Sprite sprite = Resources.Load<Sprite>($"UI/Map/region_{region:00}");
            return sprite != null ? sprite : Resources.Load<Sprite>("UI/Missions/paper_0");
        }

        private void OpenMap()
        {
            if (phase != TravelPhase.Seated) return;
            if (inventoryOpen) ToggleInventory();
            if (craftPanel != null) craftPanel.SetActive(false);
            if (gearPanel != null) gearPanel.SetActive(false);
            mapRegionId = travel.CurrentRegion.Id; mapRegionList = false;
            BuildMapPage();
            mapPanel.transform.SetAsLastSibling(); // in front of the HUD squares
            mapPanel.SetActive(true);
            SetMapHud(true);
        }

        private Text MapText(Transform parent, string value, float x0, float y0, float x1, float y1, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            Text text = Label(parent, value, x0, y0, x1, y1, size);
            text.color = color; text.alignment = anchor;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 9; text.resizeTextMaxSize = size;
            if (text.GetComponent<Shadow>() is Shadow shadow) Destroy(shadow);
            return text;
        }

        private void BuildMapPage()
        {
            foreach (Transform child in mapPage) Destroy(child.gameObject);
            TravelCatalog catalog = scene.Catalog;
            RegionDefinition region = catalog.Region(mapRegionId) ?? travel.CurrentRegion;
            bool here = region.Id == travel.CurrentRegion.Id;
            mapSheet.sprite = MapSprite(region.Number); mapSheet.color = Color.white;

            // Title inside the sheet's parchment cartouche. Tapping it switches to the region list (user, November 2, 2026:
            // replaces the sideways page arrows); tapping it again in the list returns to this region's camps.
            MapTitle(mapRegionList ? "REGIONS" : region.Name.ToUpperInvariant(), mapRegionList ? "Tap a region to see its camps" : $"Region {region.Number} - tap for regions");
            if (mapRegionList) { BuildRegionList(catalog); return; }

            // The region's three featured loose resources, then small tree / animal / fishing indicators (section 6.2.2).
            MapText(mapPage, "Found here", 0.06f, 0.835f, 0.3f, 0.87f, 15, MapInk, TextAnchor.MiddleLeft);
            for (int i = 0; i < region.LooseItemIds.Count; i++) MapIcon(MapItem(region.LooseItemIds[i]), 0.28f + i * 0.1f, 0.825f, 0.37f + i * 0.1f, 0.885f);
            string fish = FishFor(region.Number);
            string[] indicators = { "material.pine_log", "animal.rabbit.meat", fish };
            for (int i = 0; i < indicators.Length; i++) MapIcon(MapItem(indicators[i]), 0.66f + i * 0.09f, 0.835f, 0.73f + i * 0.09f, 0.875f, 0.75f);

            // River and ferry at the top of the road; beyond it the next region (or unknown lands).
            Image river = Rect("River", mapPage, 0.04f, 0.755f, 0.96f, 0.805f).gameObject.AddComponent<Image>();
            river.color = new Color(0.33f, 0.5f, 0.6f, 0.55f); river.raycastTarget = false;
            RectTransform ferry = Rect("Ferry", mapPage, 0.42f, 0.75f, 0.58f, 0.81f);
            ferry.gameObject.AddComponent<Image>().color = Color.clear; HudIconStyle.ApplyTo(ferry, "ferry", false);
            RegionDefinition beyond = catalog.Region(region.NextRegionId);
            string beyondName = beyond != null && travel.DiscoveredRegions.Contains(beyond.Id) ? "To " + beyond.Name : "Unknown lands";
            MapText(mapPage, beyondName, 0.6f, 0.755f, 0.96f, 0.805f, 16, MapInk);

            // The road: winds from below the first camp up to the ferry, one node per camp.
            int n = region.Camps.Count;
            float bottom = MapRoadBottom, top = MapRoadTop;
            var nodes = new List<Vector2>();
            for (int i = 0; i < n; i++)
            {
                float y = n == 1 ? (bottom + top) / 2 : bottom + (top - bottom) * i / (n - 1);
                nodes.Add(new Vector2(0.5f + 0.07f * Mathf.Sin(i * 1.35f + region.Number), y));
            }
            var road = new List<Vector2> { new Vector2(0.5f, 0.06f) }; road.AddRange(nodes); road.Add(new Vector2(0.5f, 0.775f));
            var smooth = Smooth(road, 6);
            MapRoad(smooth, 13f, new Color(0.36f, 0.24f, 0.11f, 0.85f));
            MapRoad(smooth, 6f, new Color(0.82f, 0.71f, 0.5f, 0.95f));

            for (int i = 0; i < n; i++)
            {
                CampDefinition camp = region.Camps[i];
                Vector2 node = nodes[i];
                Image dot = Rect("Node " + camp.Name, mapPage, node.x - 0.018f, node.y - 0.012f, node.x + 0.018f, node.y + 0.012f).gameObject.AddComponent<Image>();
                dot.sprite = DurabilityStyle.RoundedSprite; dot.color = MapInk; dot.raycastTarget = false;
                bool current = here && camp.Id == travel.CurrentCampId;
                bool reached = travel.VisitedCamps.Contains(camp.Id);
                // Same region: any reached camp; other regions: travel arrives at their first camp only.
                bool selectable = !current && reached && (here || i == 0);
                CampButton(camp, node, current, reached, selectable, here);
            }

        }

        // Map layout (Milestone 8 item 6, November 2, 2026): camps alternate road sides, so same-side tabs are two road
        // steps apart; up to the 20-camp tier-four maximum they never overlap, and the sheet needs no scrolling.
        public const float MapRoadBottom = 0.1f, MapRoadTop = 0.71f, CampTabHalfHeight = 0.03f;
        public static bool CampTabsFit(int camps) => camps <= 2 || 2f * (MapRoadTop - MapRoadBottom) / (camps - 1) >= 2f * CampTabHalfHeight;

        private static ItemDefinition MapItem(string id) => id == null ? null : TheTrailWalker.Domain.Crafting.Items.Find(id);
        // The region's local fishing pair (harvest table): the first species stands for fishing on the map.
        private static string FishFor(int region)
        {
            string[] pairs = { "fish.rainbow_trout", "fish.largemouth_bass", "fish.rainbow_trout", "fish.bluegill", "fish.rainbow_trout", "fish.largemouth_bass" };
            return pairs[(region - 1) % pairs.Length];
        }

        private void MapIcon(ItemDefinition item, float x0, float y0, float x1, float y1, float alpha = 1f)
        {
            Image icon = Rect("Icon", mapPage, x0, y0, x1, y1).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
            Sprite sprite = item != null ? ItemIconRenderer.For(ItemArt.ModelFor(item)) : null;
            if (sprite != null) { icon.sprite = sprite; icon.color = new Color(1, 1, 1, alpha); } else icon.color = Color.clear;
        }

        private void MapTitle(string title, string subtitle)
        {
            RectTransform rect = Rect("Region title", mapPage, 0.16f, 0.882f, 0.84f, 0.968f);
            Image hit = rect.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = hit;
            button.onClick.AddListener(() => { mapRegionList = !mapRegionList; BuildMapPage(); });
            MapText(rect, title, 0.03f, 0.35f, 0.97f, 0.97f, 34, MapInk).fontStyle = FontStyle.Bold;
            MapText(rect, subtitle, 0.1f, 0.02f, 0.9f, 0.34f, 17, MapFaded);
        }

        // Region colours from each region's environment kit (the ground, foliage and rock of its biome).
        private static Color RegionColor(int number)
        {
            switch (number)
            {
                case 1: return new Color(0.62f, 0.70f, 0.36f);   // Dawn Plains: sunlit meadow
                case 2: return new Color(0.56f, 0.60f, 0.42f);   // Windward Hills: windswept olive and heather
                case 3: return new Color(0.33f, 0.45f, 0.38f);   // Mistwood: misty dark forest
                case 4: return new Color(0.40f, 0.58f, 0.32f);   // Fern Hollow: fern green
                case 5: return new Color(0.55f, 0.60f, 0.64f);   // Cloudstep Mountains: grey-blue stone
                case 6: return new Color(0.44f, 0.61f, 0.56f);   // Reedwater Delta: reedy teal
                default: return Color.HSVToRGB((number * 0.137f) % 1f, 0.35f, 0.62f);
            }
        }

        // Region list (user, November 2, 2026): one row per region in its own colour, with its map sheet as a thumbnail.
        // Tapping a discovered row opens its camps; the arrow on the right travels to its first camp once reached.
        private void BuildRegionList(TravelCatalog catalog)
        {
            int count = catalog.Regions.Count; float top = 0.86f, bottom = 0.06f;
            float row = Mathf.Min(0.11f, (top - bottom) / Mathf.Max(1, count));
            for (int i = 0; i < count; i++)
            {
                RegionDefinition region = catalog.Regions[i];
                bool discovered = travel.DiscoveredRegions.Contains(region.Id), here = region.Id == travel.CurrentRegion.Id;
                float y1 = top - i * row, y0 = y1 - row * 0.86f;
                RectTransform rect = Rect(region.Name, mapPage, 0.05f, y0, 0.95f, y1);
                Image plate = rect.gameObject.AddComponent<Image>();
                Sprite tab = Resources.Load<Sprite>("UI/Missions/tab"); if (tab != null) plate.sprite = tab;
                Color tint = RegionColor(region.Number);
                plate.color = discovered ? Color.Lerp(tint, Color.white, 0.35f) : new Color(0.72f, 0.68f, 0.6f, 0.5f);
                if (discovered)
                {
                    var open = rect.gameObject.AddComponent<Button>(); open.targetGraphic = plate;
                    string id = region.Id; open.onClick.AddListener(() => { mapRegionId = id; mapRegionList = false; BuildMapPage(); });
                }
                // Thumbnail: the region's own map sheet, edged in the region colour.
                RectTransform swatch = Rect("Swatch", rect, 0.015f, 0.1f, 0.175f, 0.9f);
                Image edge = swatch.gameObject.AddComponent<Image>(); edge.color = discovered ? tint : new Color(0.5f, 0.47f, 0.42f, 0.5f); edge.raycastTarget = false;
                Image thumb = Rect("Sheet", swatch, 0.06f, 0.08f, 0.94f, 0.92f).gameObject.AddComponent<Image>();
                thumb.sprite = MapSprite(region.Number); thumb.raycastTarget = false;
                thumb.color = discovered ? Color.white : new Color(0.45f, 0.42f, 0.38f, 0.6f);
                int reached = 0; foreach (CampDefinition c in region.Camps) if (travel.VisitedCamps.Contains(c.Id)) reached++;
                string name = discovered ? region.Name + (here ? "  (here)" : "") : "Unknown lands";
                Text title = MapText(rect, name, 0.2f, 0.42f, 0.8f, 0.95f, 24, discovered ? MapInk : MapFaded, TextAnchor.MiddleLeft);
                title.fontStyle = here ? FontStyle.Bold : FontStyle.Normal;
                MapText(rect, discovered ? $"Region {region.Number} - {reached}/{region.Camps.Count} camps reached" : $"Region {region.Number}",
                    0.2f, 0.06f, 0.8f, 0.44f, 15, discovered ? MapInk : MapFaded, TextAnchor.MiddleLeft);
                // Travel arrow: to the region's first camp (in the current region, back to its first camp).
                bool firstReached = travel.VisitedCamps.Contains(region.FirstCamp.Id);
                bool atFirst = here && travel.CurrentCampId == region.FirstCamp.Id;
                bool canGo = discovered && firstReached && !atFirst;
                RectTransform go = Rect("Travel", rect, 0.83f, 0.1f, 0.985f, 0.9f);
                Image goPlate = go.gameObject.AddComponent<Image>(); goPlate.color = canGo ? new Color(1f, 0.93f, 0.75f, 0.95f) : new Color(0.8f, 0.76f, 0.68f, 0.4f);
                var goButton = go.gameObject.AddComponent<Button>(); goButton.targetGraphic = goPlate; goButton.interactable = canGo;
                RectTransform arrow = Rect("Arrow", go, 0.12f, 0.12f, 0.88f, 0.88f); arrow.gameObject.AddComponent<Image>().color = Color.clear;
                if (HudIconStyle.ApplyTo(arrow, "travel", false) && !canGo)
                {
                    Transform silhouette = arrow.Find("Silhouette");
                    if (silhouette != null) silhouette.GetComponent<Image>().color = new Color(1, 1, 1, 0.35f);
                }
                string regionId = region.Id, firstCamp = region.FirstCamp.Id;
                goButton.onClick.AddListener(() => { if (here) SelectCamp(firstCamp); else SelectRegion(regionId); });
            }
        }

        private void MapRoad(List<Vector2> points, float thickness, Color color)
        {
            RectTransform rect = Rect("Road", mapPage, 0, 0, 1, 1);
            rect.gameObject.AddComponent<CanvasRenderer>();
            UiPolyline line = rect.gameObject.AddComponent<UiPolyline>(); line.color = color; line.raycastTarget = false;
            line.SetNormalizedPoints(points, thickness);
        }

        // Catmull-Rom smoothing so the road winds instead of zig-zagging between camps.
        private static List<Vector2> Smooth(List<Vector2> points, int steps)
        {
            var result = new List<Vector2>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 p0 = points[Mathf.Max(0, i - 1)], p1 = points[i], p2 = points[i + 1], p3 = points[Mathf.Min(points.Count - 1, i + 2)];
                for (int s = 0; s < steps; s++)
                {
                    float t = s / (float)steps, t2 = t * t, t3 = t2 * t;
                    result.Add(0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3));
                }
            }
            result.Add(points[points.Count - 1]);
            return result;
        }

        private void CampButton(CampDefinition camp, Vector2 node, bool current, bool reached, bool selectable, bool sameRegion)
        {
            bool right = camp.Side == CampSide.Right;
            float h = CampTabHalfHeight, gap = 0.035f;
            float x0 = right ? node.x + gap : 0.035f, x1 = right ? 0.965f : node.x - gap;
            RectTransform rect = Rect(camp.Name, mapPage, x0, node.y - h, x1, node.y + h);
            Image plate = rect.gameObject.AddComponent<Image>();
            Sprite tab = Resources.Load<Sprite>("UI/Missions/tab");
            if (tab != null) { plate.sprite = tab; plate.type = Image.Type.Simple; }
            plate.color = current ? new Color(1f, 0.9f, 0.6f, 1f) : reached ? new Color(0.95f, 0.9f, 0.8f, 1f) : new Color(0.75f, 0.7f, 0.6f, 0.55f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = plate;
            button.interactable = selectable;
            string id = camp.Id, regionId = camp.RegionId;
            button.onClick.AddListener(() => { if (sameRegion) SelectCamp(id); else SelectRegion(regionId); });
            // Campfire icon on the road side, the name on the outer side.
            RectTransform icon = Rect("Campfire", rect, right ? 0.02f : 0.80f, 0.06f, right ? 0.2f : 0.98f, 0.94f);
            Image iconImage = icon.gameObject.AddComponent<Image>(); iconImage.color = Color.clear;
            if (HudIconStyle.ApplyTo(icon, "campfire", false) && !reached) icon.Find("Silhouette").GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
            string label = current ? camp.Name + "  (here)" : camp.Name;
            Text name = MapText(rect, label, right ? 0.22f : 0.03f, 0.40f, right ? 0.97f : 0.78f, 0.98f, 20, reached ? MapInk : MapFaded, right ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight);
            name.fontStyle = current ? FontStyle.Bold : FontStyle.Normal;
            // The camp's outgoing leg, from the same RouteBudget data as the departure preview (Milestone 8 item 5).
            SegmentDefinition leg = scene.Catalog.Outgoing(camp.Id);
            string legText = leg == null ? "" : leg.RiverApproach ? "River and ferry ahead" : $"Next leg {leg.Length:N0} m · {RouteBudget.Hearts(leg):0.#} hearts";
            MapText(rect, legText, right ? 0.22f : 0.03f, 0.02f, right ? 0.97f : 0.78f, 0.44f, 14, reached ? MapInk : MapFaded, right ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight);
            if (current)
            {
                var ring = rect.gameObject.AddComponent<Outline>(); ring.effectColor = new Color(0.55f, 0.32f, 0.08f, 0.9f); ring.effectDistance = new Vector2(2f, -2f);
            }
        }
    }
}
