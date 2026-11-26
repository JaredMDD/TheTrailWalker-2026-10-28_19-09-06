using System.Collections;
using System.Collections.Generic;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Harvest;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Missions;
using TheTrailWalker.Domain.Travel;
using TheTrailWalker.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 8 missions (plan section 11, flow confirmed by the user October 31, 2026):
    // - The tall mission lady stands at the first camp; tapping her opens the mission papers for the first selection.
    // - Afterwards she appears only at the camp where the active mission is complete: arriving there hands the recipe
    //   over automatically and the papers open by themselves; choosing a mission closes them.
    // - The papers are a stack of worn sheets with one tab per branch (Gatherer, Lumberjack, Hunter, Explorer).
    // - Carried items are delivered automatically to the active mission (ten units; tools and spare gear by instance).
    public sealed partial class CollectionPrototype
    {
        private MissionState missions = new MissionState();
        private GameObject missionPanel, missionShow;
        private RectTransform missionPage, missionTabs, missionTrackerRect;
        private Text missionTracker, missionShowText;
        private Image missionShowIcon;
        private MissionBranch missionTab = MissionBranch.Gatherer;
        private int missionRevision = -1;
        private float nextMissionTick;
        private bool missionHandoff;

        private static readonly MissionBranch[] BranchOrder = { MissionBranch.Gatherer, MissionBranch.Lumberjack, MissionBranch.Hunter, MissionBranch.Explorer };
        private static readonly string[] BranchIcons = { "branch_gatherer", "cat_tool", "branch_hunter", "branch_explorer" };
        private static readonly string[] BranchRoles = { "Hats", "Boots", "Shirts", "Pants" };
        private static readonly Color Ink = new Color(0.24f, 0.16f, 0.08f), FadedInk = new Color(0.42f, 0.32f, 0.2f), RedInk = new Color(0.55f, 0.18f, 0.1f);

        // ---------------------------------------------------------------- persistence

        private SavedMissions SaveMissions() => new SavedMissions { claimed = new List<string>(missions.Claimed), active = missions.ActiveId, delivered = missions.Delivered, region = missions.UnlockedRegion,
            catalog = MissionBook.CatalogVersion, legacyClaims = missions.LegacyClaims, activeLegacy = missions.ActiveLegacy,
            claimCatalogs = new List<int>(missions.ClaimCatalogs), activeCatalog = missions.ActiveId == null ? 0 : missions.ActiveCatalog };
        private void RestoreMissions(SavedMissions saved)
        {
            // Already validated by SessionSaveStore.Validate.
            missions = saved == null ? new MissionState() : MissionState.Restore(saved.claimed, string.IsNullOrEmpty(saved.active) ? null : saved.active, saved.delivered, saved.region,
                saved.catalog, saved.legacyClaims, saved.activeLegacy, saved.claimCatalogs, saved.activeCatalog);
            missionRevision = -1;
        }

        // ---------------------------------------------------------------- rules context

        // Shop-only mission items (milk, eggs, chicken, shrimp, tea herbs) count once a settlement can be reached: a discovered
        // region whose final camp starts a settlement approach (region 4 today). It was never set, so those missions stayed
        // locked as "sold at a settlement (later milestone)" (user report, November 25, 2026).
        public static bool SettlementReachableFrom(TravelCatalog catalog, IEnumerable<string> discoveredRegionIds)
        {
            if (catalog == null || discoveredRegionIds == null) return false;
            foreach (string id in discoveredRegionIds) { RegionDefinition region = catalog.Region(id); if (region != null && catalog.SettlementApproachFrom(region.FinalCamp.Id) != null) return true; }
            return false;
        }

        private MissionContext MissionContextNow() => new MissionContext
        {
            SettlementReachable = travel != null && scene != null && SettlementReachableFrom(scene.Catalog, travel.DiscoveredRegions),
            RegionDiscovered = number =>
            {
                if (travel == null || scene == null) return number == 1;
                foreach (string id in travel.DiscoveredRegions) { RegionDefinition region = scene.Catalog.Region(id); if (region != null && region.Number == number) return true; }
                return false;
            },
            KnowsRecipe = id => crafting.Knows(id),
            CarriesTool = family =>
            {
                foreach (ItemEntry e in ledger.Entries)
                    if (e.Ready && (e.Owner == ItemOwner.Backpack || e.Owner == ItemOwner.Pocket || e.Owner == ItemOwner.Hook) && HarvestTools.TryGet(e.Definition, out ToolSpec spec) && spec.Family == family) return true;
                return false;
            },
        };

        // ---------------------------------------------------------------- update

        private void UpdateMissions()
        {
            if (saveBlocked || ledger == null) return;
            // Automatic delivery whenever the carried inventory changes (acquisition, transfer, crafts becoming ready).
            if (missions.Active != null && !missions.ActiveComplete && ledger.Revision != missionRevision)
            {
                int taken = missions.Deliver(ledger);
                missionRevision = ledger.Revision;
                if (taken > 0)
                {
                    MissionDefinition m = missions.Active;
                    Say($"Mission {m.Code}: delivered {taken} {missions.RequestedItemFor(m).Name} ({missions.Delivered}/{m.Quantity})." + (missions.ActiveComplete ? " Complete: the mission lady waits at the next camp." : ""));
                    SaveNow();
                }
            }
            if (Time.unscaledTime < nextMissionTick) return;
            nextMissionTick = Time.unscaledTime + 0.4f;
            bool seated = phase == TravelPhase.Seated;
            MissionContext context = MissionContextNow();
            bool present = missions.LadyPresent(context);
            if (scene != null && scene.StartCamp != null && scene.StartCamp.MissionNpc != null) scene.StartCamp.MissionNpc.SetActive(present);
            // The camp ahead shows her only when she will be there on arrival.
            if (scene != null && scene.EndCamp != null && scene.EndCamp.MissionNpc != null) scene.EndCamp.MissionNpc.SetActive(present);
            if (!seated && missionPanel != null && missionPanel.activeSelf) missionPanel.SetActive(false);
            // Recipe hand-off: seated at a camp with a completed mission, she hands the recipe over and the papers open.
            if (seated && missions.ActiveComplete && !missionHandoff && !TravelBusy && !(milestone4 && stamina.Fainted)) StartCoroutine(HandOff());
            RefreshMissionTracker();
        }

        private void RefreshMissionTracker()
        {
            if (missionTracker == null) return;
            MissionDefinition m = missions.Active;
            bool covered = (craftPanel != null && craftPanel.activeSelf) || (gearPanel != null && gearPanel.activeSelf) || (missionPanel != null && missionPanel.activeSelf) || (mapPanel != null && mapPanel.activeSelf);
            missionTrackerRect.gameObject.SetActive(m != null && !covered);
            if (m == null) return;
            // Only the requested item's model and the counter (user): no name or description.
            if (missionTrackerItem != missions.RequestedFor(m))
            {
                missionTrackerItem = missions.RequestedFor(m);
                Sprite sprite = ItemIconRenderer.For(ItemArt.ModelFor(missions.RequestedItemFor(m)));
                missionTrackerIcon.sprite = sprite; missionTrackerIcon.color = sprite != null ? Color.white : new Color(0, 0, 0, 0);
            }
            missionTracker.text = $"{missions.Delivered}/{m.Quantity}";
            missionTracker.color = missions.ActiveComplete ? DurabilityStyle.Healthy : Color.white;
        }

        // Tapping the mission lady (seated at her camp) opens the papers.
        private readonly RaycastHit[] ladyHits = new RaycastHit[16];
        private bool TryTapMissionLady(Vector2 screen)
        {
            GameObject lady = scene != null && scene.StartCamp != null ? scene.StartCamp.MissionNpc : null;
            if (lady == null || !lady.activeInHierarchy) return false;
            int count = Physics.RaycastNonAlloc(cameraView.ScreenPointToRay(screen), ladyHits, cameraView.farClipPlane, ~0, QueryTriggerInteraction.Collide);
            bool hit = false;
            for (int i = 0; i < count; i++) if (ladyHits[i].transform.IsChildOf(lady.transform)) { hit = true; break; }
            if (!hit) return false;
            if (phase != TravelPhase.Seated) { Say("Sit at the camp to talk to the mission lady."); return true; }
            if (missions.ActiveComplete) { if (!missionHandoff) StartCoroutine(HandOff()); return true; }
            if (missions.Active != null) { Say($"Current mission {missions.Active.Code}: {missions.Delivered}/{missions.Active.Quantity} {missions.RequestedItemFor(missions.Active).Name}."); return true; }
            OpenMissionPapers();
            return true;
        }

        // ---------------------------------------------------------------- hand-off

        private IEnumerator HandOff()
        {
            missionHandoff = true;
            yield return new WaitForSeconds(0.6f); // let the seat transition settle
            if (!missions.ActiveComplete || phase != TravelPhase.Seated) { missionHandoff = false; yield break; }
            MissionDefinition done = missions.Claim();
            bool learned = crafting.Learn(done.RewardRecipeId);
            SaveNow();
            if (missionShow == null) CreateMissionPanel();
            if (inventoryOpen) ToggleInventory();
            if (craftPanel != null) craftPanel.SetActive(false);
            if (gearPanel != null) gearPanel.SetActive(false);
            // The view turns to the lady for the hand-off, like other interactions focus their target.
            GameObject lady = scene != null && scene.StartCamp != null ? scene.StartCamp.MissionNpc : null;
            float yaw = orbit.Yaw; bool focused = false;
            if (lady != null && CameraRig != null)
            {
                Vector3 at = lady.transform.position, toSeat = Vector3.ProjectOnPlane(scene.StartCamp.Seat - at, Vector3.up);
                if (toSeat.sqrMagnitude < 0.01f) toSeat = lady.transform.forward;
                toSeat.Normalize();
                Vector3 side = Vector3.Cross(Vector3.up, toSeat);
                CameraRig.Focus(at + toSeat * 2.6f + side * 0.5f + Vector3.up * 1.7f, at + Vector3.up * 1.45f);
                focused = true;
            }
            RecipeDefinition reward = done.Reward;
            Sprite icon = reward?.Output != null ? ItemIconRenderer.For(ItemArt.ModelFor(reward.Output)) : null;
            missionShowIcon.sprite = icon; missionShowIcon.color = icon != null ? Color.white : new Color(0, 0, 0, 0);
            missionShowText.text = $"MISSION {done.Code} COMPLETE\n\n{reward?.Name ?? done.RewardRecipeId} recipe" + (learned ? " received" : " (already known)");
            missionShow.transform.SetAsLastSibling(); missionShow.SetActive(true);
            Say($"The mission lady hands you the {reward?.Name} recipe.");
            yield return new WaitForSeconds(2.4f);
            missionShow.SetActive(false);
            if (focused) { CameraRig?.ClearFocus(); orbit.Restore(yaw); }
            missionHandoff = false;
            if (phase == TravelPhase.Seated) OpenMissionPapers();
        }

        // ---------------------------------------------------------------- papers (selection menu)

        private void OpenMissionPapers()
        {
            if (missionPanel == null) CreateMissionPanel();
            if (inventoryOpen) ToggleInventory();
            if (craftPanel != null) craftPanel.SetActive(false);
            if (gearPanel != null) gearPanel.SetActive(false);
            SetMenu(false);
            // Opens on the first branch with an offerable mission.
            MissionContext context = MissionContextNow();
            foreach (MissionOffer offer in missions.Offers(context)) if (offer.Eligible) { missionTab = offer.Branch; break; }
            missionPanel.transform.SetAsLastSibling(); missionPanel.SetActive(true);
            BuildMissionPage();
        }

        private void CreateMissionPanel()
        {
            missionPanel = Rect("Mission papers", safe, 0.05f, 0.17f, 0.95f, 0.83f).gameObject;
            // The stack: two worn sheets peeking out behind the front page, slightly turned.
            Sheet("Back sheet", missionPanel.transform, "paper_2", 0.015f, 0.0f, 0.985f, 0.88f, -3.2f);
            Sheet("Middle sheet", missionPanel.transform, "paper_1", 0.0f, 0.01f, 0.98f, 0.885f, 2.1f);
            missionTabs = Rect("Branch tabs", missionPanel.transform, 0.03f, 0.86f, 0.97f, 1f);
            Sheet("Front sheet", missionPanel.transform, "paper_0", 0.005f, 0.0f, 0.995f, 0.89f, 0f);
            missionPage = Rect("Page", missionPanel.transform, 0.07f, 0.05f, 0.93f, 0.84f);
            missionPanel.SetActive(false);

            missionShow = Rect("Recipe hand-off", safe, 0.14f, 0.07f, 0.86f, 0.33f).gameObject; // below the lady while the view focuses on her
            Sheet("Hand-off sheet", missionShow.transform, "paper_1", 0, 0, 1, 1, -1.5f);
            missionShowIcon = Rect("Recipe", missionShow.transform, 0.32f, 0.42f, 0.68f, 0.92f).gameObject.AddComponent<Image>();
            missionShowIcon.preserveAspect = true; missionShowIcon.raycastTarget = false;
            missionShowText = Label(missionShow.transform, "", 0.06f, 0.06f, 0.94f, 0.42f, 24);
            missionShowText.color = Ink; missionShowText.fontStyle = FontStyle.Bold;
            missionShowText.resizeTextForBestFit = true; missionShowText.resizeTextMinSize = 12; missionShowText.resizeTextMaxSize = 24;
            missionShow.SetActive(false);
        }

        private static readonly Dictionary<string, Sprite> PaperSprites = new Dictionary<string, Sprite>();
        private static Sprite Paper(string key)
        {
            if (!PaperSprites.TryGetValue(key, out Sprite sprite) || sprite == null) { sprite = Resources.Load<Sprite>("UI/Missions/" + key); PaperSprites[key] = sprite; }
            return sprite;
        }
        private static Image Sheet(string name, Transform parent, string key, float x0, float y0, float x1, float y1, float angle)
        {
            RectTransform rect = Rect(name, parent, x0, y0, x1, y1);
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            Image image = rect.gameObject.AddComponent<Image>();
            Sprite sprite = Paper(key);
            if (sprite != null) image.sprite = sprite; else image.color = new Color(0.86f, 0.79f, 0.63f, 1f);
            return image;
        }

        private void BuildMissionPage()
        {
            foreach (Transform child in missionTabs) Destroy(child.gameObject);
            foreach (Transform child in missionPage) Destroy(child.gameObject);
            MissionContext context = MissionContextNow();
            IReadOnlyList<MissionOffer> offers = missions.Offers(context);
            // Tabs: the selected one stands taller and brighter, like a separator pulled forward.
            for (int i = 0; i < BranchOrder.Length; i++)
            {
                MissionBranch branch = BranchOrder[i];
                MissionOffer offer = offers[(int)branch];
                bool selected = branch == missionTab;
                float x0 = i * 0.25f + 0.01f;
                RectTransform tab = Rect(branch + " tab", missionTabs, x0, selected ? 0f : 0.12f, x0 + 0.23f, 1f);
                Image plate = tab.gameObject.AddComponent<Image>();
                Sprite tabSprite = Paper("tab");
                if (tabSprite != null) plate.sprite = tabSprite;
                plate.color = selected ? Color.white : offer.Eligible ? new Color(0.86f, 0.8f, 0.68f) : new Color(0.66f, 0.6f, 0.5f);
                tab.gameObject.AddComponent<Button>().onClick.AddListener(() => { missionTab = branch; BuildMissionPage(); });
                RectTransform iconRect = Rect("Icon", tab, 0.06f, 0.18f, 0.40f, 0.92f);
                iconRect.gameObject.AddComponent<Image>().color = Color.clear;
                HudIconStyle.ApplyTo(iconRect, BranchIcons[i], false);
                Text name = Label(tab, branch.ToString().ToUpperInvariant(), 0.38f, 0.12f, 0.98f, 0.92f, 15);
                name.color = selected ? Ink : FadedInk; name.fontStyle = FontStyle.Bold; name.alignment = TextAnchor.MiddleLeft;
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 8; name.resizeTextMaxSize = 15;
                if (offer.Eligible && !selected)
                {
                    // A small ink dot marks branches with an offerable mission.
                    Image dot = Rect("Ready", tab, 0.84f, 0.66f, 0.95f, 0.9f).gameObject.AddComponent<Image>();
                    dot.sprite = DurabilityStyle.RoundedSprite; dot.color = new Color(0.36f, 0.48f, 0.22f); dot.raycastTarget = false;
                }
            }
            MissionOffer page = offers[(int)missionTab];
            int index = System.Array.IndexOf(BranchOrder, missionTab);
            int total = 0; foreach (MissionDefinition m in MissionBook.Branch(missionTab)) total++;
            Text title = InkText(Label(missionPage, missionTab.ToString().ToUpperInvariant(), 0f, 0.88f, 0.62f, 1f, 30)); title.alignment = TextAnchor.MiddleLeft; title.fontStyle = FontStyle.Bold;
            Text role = InkText(Label(missionPage, page.Mission != null ? $"Mission {page.Mission.Index} of {total}  |  {BranchRoles[index]}" : BranchRoles[index], 0.5f, 0.88f, 1f, 1f, 17));
            role.alignment = TextAnchor.MiddleRight; role.color = FadedInk;
            Rule(0.865f);
            if (page.Mission == null)
            {
                Text done = InkText(Label(missionPage, $"All tier-one {missionTab} missions are complete.\nNew ones arrive with the next tier.", 0f, 0.4f, 1f, 0.8f, 22));
                done.color = FadedInk;
            }
            else
            {
                MissionDefinition m = page.Mission;
                Row(0.60f, 0.84f, missions.RequestedItemFor(m), $"Bring {m.Quantity} {missions.RequestedItemFor(m).Name}" + (m.CountsInstances ? "\n(each a finished, unworn item)" : ""));
                Text arrow = InkText(Label(missionPage, "v", 0.06f, 0.53f, 0.22f, 0.61f, 26)); arrow.color = FadedInk;
                RecipeDefinition reward = m.Reward;
                Row(0.30f, 0.54f, reward?.Output, $"Reward: {reward?.Name} recipe" + (crafting.Knows(m.RewardRecipeId) ? "\n(already known)" : ""));
                Rule(0.285f);
                if (page.Eligible)
                {
                    long carried = ledger.CountIngredient(missions.RequestedFor(m));
                    string note = carried > 0 ? $"You carry {carried}: {System.Math.Min(carried, m.Quantity)} will be delivered at once." : "Carried items are delivered automatically.";
                    if (!string.IsNullOrEmpty(m.Note)) note += "\n" + m.Note + ".";
                    Text info = InkText(Label(missionPage, note, 0f, 0.15f, 1f, 0.28f, 17)); info.color = FadedInk;
                    Text accept = GearButton(missionPage, "ACCEPT", 0.30f, 0.0f, 0.70f, 0.12f, () => AcceptMission(m.Id));
                    accept.resizeTextMaxSize = 24;
                }
                else
                {
                    Text locked = InkText(Label(missionPage, "Not yet possible:\n" + page.Reason + ".", 0f, 0.08f, 1f, 0.28f, 19)); locked.color = RedInk;
                }
            }
            // While some branch is offerable the papers close only by choosing; otherwise they can be put away.
            bool anyEligible = false; foreach (MissionOffer o in offers) anyEligible |= o.Eligible;
            // Putting them away uses the shared return button (user, November 5, 2026), not a PUT AWAY text button.
            if (!anyEligible) ShowScreenReturn(missionPanel, () => missionPanel.SetActive(false));
            else if (screenReturnOwner == missionPanel) HideScreenReturn();
        }

        private static Text InkText(Text text)
        {
            text.color = Ink; text.resizeTextForBestFit = true; text.resizeTextMinSize = 10; text.resizeTextMaxSize = Mathf.Max(10, text.fontSize);
            if (text.GetComponent<Shadow>() is Shadow shadow) Destroy(shadow);
            return text;
        }
        private void Rule(float y)
        {
            Image line = Rect("Rule", missionPage, 0f, y - 0.004f, 1f, y + 0.004f).gameObject.AddComponent<Image>();
            line.color = new Color(0.45f, 0.33f, 0.18f, 0.55f); line.raycastTarget = false;
        }
        private void Row(float y0, float y1, ItemDefinition item, string text)
        {
            Image icon = Rect("Icon", missionPage, 0f, y0, 0.26f, y1).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
            Sprite sprite = item != null ? ItemIconRenderer.For(ItemArt.ModelFor(item)) : null;
            if (sprite != null) icon.sprite = sprite; else icon.color = new Color(0, 0, 0, 0);
            Text label = InkText(Label(missionPage, text, 0.3f, y0, 1f, y1, 22)); label.alignment = TextAnchor.MiddleLeft;
        }

        private void AcceptMission(string missionId)
        {
            MissionContext context = MissionContextNow();
            if (!missions.CanAccept(missionId, context, out string reason)) { Say(reason + "."); BuildMissionPage(); return; }
            missions.Accept(missionId, context);
            int taken = missions.Deliver(ledger); missionRevision = ledger.Revision;
            missionPanel.SetActive(false); // the papers close once a mission is chosen
            MissionDefinition m = missions.Active;
            SaveNow(); Redraw(); RefreshMissionTracker();
            Say($"Mission {m.Code} accepted: bring {m.Quantity} {missions.RequestedItemFor(m).Name}." + (taken > 0 ? $" {taken} delivered from your bag ({missions.Delivered}/{m.Quantity})." : ""));
        }

        // HUD tracker pill beside the bag button row; laid out with the HUD squares.
        // Mission progress (user): the requested item's model with a counter, in the square above the bag button.
        private Image missionTrackerIcon;
        private string missionTrackerItem;
        private void CreateMissionTracker()
        {
            missionTrackerRect = Rect("Mission tracker", safe, 0, 0, 0, 0);
            missionTrackerIcon = Rect("Model", missionTrackerRect, -0.05f, 0.12f, 1.05f, 1.1f).gameObject.AddComponent<Image>();
            missionTrackerIcon.preserveAspect = true; missionTrackerIcon.raycastTarget = false;
            missionTracker = Label(missionTrackerRect, "", 0f, -0.06f, 1f, 0.34f, 22);
            missionTracker.fontStyle = FontStyle.Bold; missionTracker.color = Color.white;
            missionTracker.resizeTextForBestFit = true; missionTracker.resizeTextMinSize = 10; missionTracker.resizeTextMaxSize = 22;
            var outline = missionTracker.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0, 0, 0, 0.85f); outline.effectDistance = new Vector2(1.5f, -1.5f);
            missionTrackerRect.gameObject.SetActive(false);
        }
        private void LayoutMissionTracker(float barLine)
        {
            if (missionTrackerRect == null) return;
            PlaceSquare(missionTrackerRect, false, barLine, 0, 1);
        }
    }
}
