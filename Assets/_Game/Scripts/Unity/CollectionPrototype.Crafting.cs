using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TheTrailWalker.Domain.Crafting;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Milestone 7 crafting (plan sections 6.1, 9, 10): a separate menu opened by the CRAFT button (camps only), placed
    // above the open bag like the equipment menu. 3 x 3 ingredient grid with CRAFT / AUTOFILL to its right, the recipe
    // book at top right and RETURN at bottom right; x1/x10/x100/x1000 for stackables, x1 for tools and gear. Timed
    // outputs drop into the bag at once as pending items with a timer badge; they become usable (and credit mastery once)
    // when ready, even while the app was closed (timers use the device clock).
    public sealed partial class CollectionPrototype
    {
        private CraftingState crafting = new CraftingState();
        private GameObject craftPanel;
        private RectTransform craftBook, craftShow;
        private Text craftHint, craftPreview, craftSlotsInfo, craftLevelInfo, craftShowText;
        private Image craftOutputIcon, craftShowIcon, craftLevelStar, craftProgressFill;
        private Text craftRecipeName;
        private static readonly Sprite[] LevelStars = new Sprite[11];
        private static Sprite LevelStar(int level) => LevelStars[level] != null ? LevelStars[level] : LevelStars[level] = Resources.Load<Sprite>("UI/Crafting/level_" + level);
        private readonly List<(Image frame, Image icon, Text count)> craftCells = new List<(Image, Image, Text)>();
        private readonly List<(Button button, Text label, int size)> craftSizes = new List<(Button, Text, int)>();
        private RecipeDefinition craftRecipe;
        private int craftSize = 1;
        private bool craftFilled, craftBusy;
        private float nextCraftTick;
        private readonly Dictionary<string, Text> pendingLabels = new Dictionary<string, Text>();

        // ---------------------------------------------------------------- persistence

        // Known recipes in the saved profile, for scene building before this component loads (trader outfits).
        public static HashSet<string> KnownRecipesFromSave()
        {
            try
            {
                var store = new SessionSaveStore(SaveDirectory, new SystemClock(), 3);
                LoadStatus status = store.Load(float.MaxValue, out SessionSnapshot snapshot, out _);
                if ((status == LoadStatus.Loaded || status == LoadStatus.Recovered) && snapshot?.crafting?.known != null) return new HashSet<string>(snapshot.crafting.known);
            }
            catch (Exception) { }
            return new HashSet<string>();
        }

        private SavedCrafting SaveCrafting()
        {
            var saved = new SavedCrafting();
            saved.known.AddRange(crafting.KnownInOrder); saved.lastRecipe = crafting.LastCrafted;
            foreach (var pair in crafting.Units) saved.mastery.Add(new SavedMastery { recipeId = pair.Key, units = pair.Value });
            foreach (CraftBatch b in crafting.Batches)
                saved.batches.Add(new SavedBatch { id = b.Id, recipeId = b.RecipeId, executions = b.Executions, outputUnits = b.OutputUnits, level = b.Level, quality = b.Quality,
                    unitValue = b.UnitValue, maxDurability = b.MaxDurability, credited = b.Credited, masteryCredited = b.MasteryCredited, startedUtc = b.StartedUtc.ToString("O"), readyAtUtc = b.ReadyAtUtc.ToString("O"), itemIds = new List<string>(b.ItemIds) });
            return saved;
        }

        private void RestoreCrafting(SavedCrafting saved)
        {
            crafting = new CraftingState();
            if (saved == null) return;
            var mastery = new List<KeyValuePair<string, long>>(); foreach (SavedMastery m in saved.mastery) mastery.Add(new KeyValuePair<string, long>(m.recipeId, m.units));
            var batches = new List<CraftBatch>();
            foreach (SavedBatch b in saved.batches)
            {
                DateTime.TryParse(b.startedUtc, null, DateTimeStyles.RoundtripKind, out DateTime started);
                DateTime.TryParse(b.readyAtUtc, null, DateTimeStyles.RoundtripKind, out DateTime ready);
                batches.Add(new CraftBatch { Id = b.id, RecipeId = b.recipeId, Executions = b.executions, OutputUnits = b.outputUnits, Level = b.level, Quality = b.quality, UnitValue = b.unitValue,
                    MaxDurability = b.maxDurability, Credited = b.credited, MasteryCredited = b.masteryCredited, StartedUtc = started.ToUniversalTime(), ReadyAtUtc = ready.ToUniversalTime(), ItemIds = new List<string>(b.itemIds) });
            }
            crafting.Restore(saved.known, mastery, batches);
            crafting.LastCrafted = RecipeBook.Find(saved.lastRecipe) != null ? saved.lastRecipe : null;
        }

        // Fixture resets replace the bag: keep recipe knowledge and mastery, drop batches whose outputs no longer exist.
        private void ResetCraftingBatches()
        {
            var units = new List<KeyValuePair<string, long>>(crafting.Units); string last = crafting.LastCrafted;
            crafting.Restore(new List<string>(crafting.KnownInOrder), units, new CraftBatch[0]); crafting.LastCrafted = last;
        }

        // ---------------------------------------------------------------- timers

        private void UpdateCrafting()
        {
            SyncBagButtonIcon();
            if (Time.unscaledTime < nextCraftTick) return;
            nextCraftTick = Time.unscaledTime + 0.25f;
            if (crafting.Batches.Count > 0)
            {
                List<CraftBatch> done = CraftingService.Finish(ledger, crafting, DateTime.UtcNow);
                if (done.Count > 0)
                {
                    var names = new List<string>();
                    foreach (CraftBatch b in done) { RecipeDefinition r = RecipeBook.Find(b.RecipeId); names.Add($"{r?.Output?.Name ?? b.RecipeId} x{b.OutputUnits}"); }
                    Say("Ready: " + string.Join(", ", names));
                    Redraw(); SaveNow();
                }
            }
            foreach (var pair in pendingLabels)
            {
                CraftBatch batch = null; foreach (CraftBatch b in crafting.Batches) if (b.ItemIds.Contains(pair.Key)) { batch = b; break; }
                if (pair.Value != null) pair.Value.text = batch == null ? "..." : FormatTime((batch.ReadyAtUtc - DateTime.UtcNow).TotalSeconds);
            }
            if (craftPanel != null && craftPanel.activeSelf) { if (!AtCamp) craftPanel.SetActive(false); else { LayoutCraftPanel(); RefreshCraftPanel(); } }
        }

        private static string FormatTime(double seconds)
        {
            if (seconds <= 0.05) return "Instant";
            var span = TimeSpan.FromSeconds(Math.Ceiling(seconds));
            return span.TotalHours >= 1 ? $"{(int)span.TotalHours}h {span.Minutes:00}m" : span.TotalMinutes >= 1 ? $"{span.Minutes}:{span.Seconds:00}" : $"{span.Seconds}s";
        }

        // Timer badge on a pending (not yet ready) bag item; Redraw clears and rebuilds these.
        private void AddPendingBadge(RectTransform rect, ItemEntry item)
        {
            // Centred on the model (user): the remaining time reads over the item itself.
            RectTransform badge = Rect("Timer", rect, 0f, 0f, 0f, 0f);
            badge.anchorMin = badge.anchorMax = new Vector2(0.5f, 0.5f); badge.pivot = new Vector2(0.5f, 0.5f); badge.sizeDelta = new Vector2(84f, 34f);
            Image back = badge.gameObject.AddComponent<Image>(); back.sprite = DurabilityStyle.RoundedSprite; back.type = Image.Type.Sliced; back.color = new Color(0.05f, 0.09f, 0.1f, 1f); back.raycastTarget = false;
            Text text = Label(badge, "...", 0, 0, 1, 1, 16); text.color = DurabilityStyle.Selection; text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true; text.resizeTextMinSize = 8; text.resizeTextMaxSize = 16;
            pendingLabels[item.Id] = text;
        }

        // ---------------------------------------------------------------- debug (removed with the debug cleanup)

        private void LearnAllRecipes()
        {
            int learned = 0; foreach (RecipeDefinition r in RecipeBook.All) if (!r.Retired && crafting.Learn(r.Id)) learned++;
            SaveNow();
            Say(learned > 0 ? $"Learned {learned} tier-one recipes (debug; missions unlock recipes in Milestone 8)." : "All tier-one recipes are already known.");
        }

        // Debug: adds exactly what the selected recipe needs for the chosen batch (stacks of up to 1,000; tool inputs as
        // fresh instances), into the bag or the overflow tray. Lets every recipe be tested without visiting its regions.
        private void GrantRecipeIngredients()
        {
            if (craftRecipe == null) { Say("Choose a recipe first."); return; }
            int overflow = 0;
            foreach (RecipeInput input in craftRecipe.Inputs)
            {
                ItemDefinition item = Items.Find(input.ItemId);
                if (item == null) continue;
                long left = (long)input.Quantity * craftSize;
                while (left > 0)
                {
                    int quantity = item.Stackable ? (int)Math.Min(1000, left) : 1; left -= quantity;
                    ItemProfile profile = item.Stackable ? new ItemProfile(1)
                        : TheTrailWalker.Domain.Harvest.HarvestTools.TryGet(item, out var spec) ? new ItemProfile(100, spec.BaseUses, spec.BaseUses) : new ItemProfile(100, 10, 10);
                    ItemEntry entry = ledger.AddWorld("debug-input-" + Guid.NewGuid().ToString("N"), item, quantity, 1, profile);
                    const int token = -834;
                    if (!ledger.Reserve(entry, token)) continue;
                    bool stored = ledger.TryQuickStore(entry, token, out _); ledger.Release(entry, token);
                    if (!stored || entry.Owner == ItemOwner.World) { if (entry.Owner == ItemOwner.World && ledger.RestoreStored(entry, ItemOwner.Overflow, 0, 0)) overflow++; }
                }
            }
            Redraw(); SaveNow(); RefreshCraftPanel();
            Say($"Added the ingredients for {craftRecipe.Name} x{craftSize}" + (overflow > 0 ? $" ({overflow} stacks in overflow: CLAIM OVERFLOW)." : "."));
        }

        // ---------------------------------------------------------------- menu

        private void ToggleCraftPanel()
        {
            if (saveBlocked || (milestone4 && stamina.Fainted)) return;
            if (!AtCamp) { Say("Crafting is available at camps (and later at home)."); return; }
            if (craftPanel == null) CreateCraftPanel();
            if (craftPanel.activeSelf) { craftPanel.SetActive(false); if (inventoryOpen) ToggleInventory(); return; }
            if (gearPanel != null && gearPanel.activeSelf) gearPanel.SetActive(false);
            if (!inventoryOpen) ToggleInventory();
            craftPanel.transform.SetAsLastSibling(); LayoutCraftPanel(); craftPanel.SetActive(true);
            if (inventoryButton != null) inventoryButton.SetAsLastSibling(); // the return button stays on top
            SetMenu(false); // the user-menu column closes behind the crafting menu
            // Opens on the last crafted recipe, or the first unlocked one on the first opening; the book stays closed.
            string start = crafting.LastCrafted != null && crafting.Knows(crafting.LastCrafted) ? crafting.LastCrafted : crafting.FirstLearned;
            if (start != null && (craftRecipe == null || craftRecipe.Id != start)) { craftRecipe = RecipeBook.Find(start); craftSize = 1; craftFilled = false; }
            craftBook.gameObject.SetActive(false);
            RefreshCraftPanel();
            Say(crafting.Known.Count == 0 ? "No recipes known yet. (Debug: LEARN T1 RECIPES.)" : "Choose a recipe, AUTOFILL, then CRAFT.");
        }

        private void LayoutCraftPanel()
        {
            if (craftPanel == null || bagFrame == null) return;
            var rect = (RectTransform)craftPanel.transform;
            // Ends above the HUD button row on top of the bag, so the bag button (return icon while crafting) stays visible.
            // The HUD squares stand on the bar line (top of the open bag); the panel starts above their row.
            float buttonRow = safe.rect.height > 0 ? (HudSquare + HudGap * 2f) / safe.rect.height : 0.06f;
            float baseLine = Mathf.Max(menuBarLine, bagFrame.anchorMax.y);
            rect.anchorMin = new Vector2(0.02f, Mathf.Min(0.72f, baseLine + buttonRow)); rect.anchorMax = new Vector2(0.98f, 0.995f);
        }

        private void CreateCraftPanel()
        {
            craftPanel = Rect("Crafting", safe, 0.02f, 0.56f, 0.98f, 0.995f).gameObject;
            Image panel = craftPanel.AddComponent<Image>(); panel.sprite = DurabilityStyle.RoundedSprite; panel.type = Image.Type.Sliced; panel.color = DurabilityStyle.Panel;
            Text title = Label(craftPanel.transform, "CRAFTING", 0.05f, 0.925f, 0.6f, 0.99f, 30); title.alignment = TextAnchor.MiddleLeft; title.fontStyle = FontStyle.Bold; title.color = DurabilityStyle.Text;
            craftHint = Label(craftPanel.transform, "", 0.04f, 0.885f, 0.96f, 0.925f, 16); craftHint.color = new Color(DurabilityStyle.Text.r, DurabilityStyle.Text.g, DurabilityStyle.Text.b, 0.8f);
            craftHint.resizeTextForBestFit = true; craftHint.resizeTextMinSize = 10; craftHint.resizeTextMaxSize = 16;

            // 3 x 3 ingredient grid.
            const float gx0 = 0.04f, gy0 = 0.30f, gx1 = 0.60f, gy1 = 0.685f, gap = 0.012f;
            float cw = (gx1 - gx0 - 2 * gap) / 3f, ch = (gy1 - gy0 - 2 * gap) / 3f;
            for (int i = 0; i < 9; i++)
            {
                int col = i % 3, row = i / 3;
                float x0 = gx0 + col * (cw + gap), y1 = gy1 - row * (ch + gap);
                RectTransform cell = Rect("Ingredient " + i, craftPanel.transform, x0, y1 - ch, x0 + cw, y1);
                Image fill = cell.gameObject.AddComponent<Image>(); fill.sprite = DurabilityStyle.RoundedSprite; fill.type = Image.Type.Sliced; fill.color = DurabilityStyle.Slot; fill.raycastTarget = false;
                Image frame = Rect("Frame", cell, 0, 0, 1, 1).gameObject.AddComponent<Image>(); frame.sprite = DurabilityStyle.FrameSprite; frame.type = Image.Type.Sliced; frame.raycastTarget = false;
                Image icon = Rect("Icon", cell, 0.12f, 0.26f, 0.88f, 0.94f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
                Text count = Label(cell, "", 0.04f, 0.02f, 0.96f, 0.26f, 15); count.fontStyle = FontStyle.Bold; count.resizeTextForBestFit = true; count.resizeTextMinSize = 8; count.resizeTextMaxSize = 15;
                craftCells.Add((frame, icon, count));
            }
            // Recipe info (user): top left, opposite the recipe book: output model, name, level star (1-10, the quality
            // star design continued) and progress to the next level.
            RectTransform info = Rect("Recipe info", craftPanel.transform, 0.04f, 0.705f, 0.60f, 0.875f);
            Image infoBack = info.gameObject.AddComponent<Image>(); infoBack.sprite = DurabilityStyle.RoundedSprite; infoBack.type = Image.Type.Sliced; infoBack.color = DurabilityStyle.Slot; infoBack.raycastTarget = false;
            craftOutputIcon = Rect("Model", info, 0.02f, 0.06f, 0.30f, 0.94f).gameObject.AddComponent<Image>(); craftOutputIcon.preserveAspect = true; craftOutputIcon.raycastTarget = false;
            craftRecipeName = Label(info, "", 0.33f, 0.56f, 0.98f, 0.96f, 18); craftRecipeName.alignment = TextAnchor.MiddleLeft; craftRecipeName.color = DurabilityStyle.Text; craftRecipeName.fontStyle = FontStyle.Bold;
            craftRecipeName.resizeTextForBestFit = true; craftRecipeName.resizeTextMinSize = 10; craftRecipeName.resizeTextMaxSize = 18;
            craftLevelStar = Rect("Level star", info, 0.33f, 0.08f, 0.50f, 0.56f).gameObject.AddComponent<Image>(); craftLevelStar.preserveAspect = true; craftLevelStar.raycastTarget = false;
            RectTransform track = Rect("Level progress", info, 0.53f, 0.36f, 0.97f, 0.46f);
            Image trackImage = track.gameObject.AddComponent<Image>(); trackImage.sprite = DurabilityStyle.RoundedSprite; trackImage.type = Image.Type.Sliced; trackImage.color = DurabilityStyle.Track; trackImage.raycastTarget = false;
            craftProgressFill = Rect("Fill", track, 0, 0, 0, 1).gameObject.AddComponent<Image>(); craftProgressFill.sprite = DurabilityStyle.RoundedSprite; craftProgressFill.type = Image.Type.Sliced; craftProgressFill.color = DurabilityStyle.Selection; craftProgressFill.raycastTarget = false;
            craftLevelInfo = Label(info, "", 0.53f, 0.06f, 0.98f, 0.34f, 14); craftLevelInfo.alignment = TextAnchor.MiddleLeft; craftLevelInfo.color = DurabilityStyle.Text;
            craftLevelInfo.resizeTextForBestFit = true; craftLevelInfo.resizeTextMinSize = 8; craftLevelInfo.resizeTextMaxSize = 14;

            // Right: recipe book (top), then AUTOFILL and CRAFT beside the grid.
            // Recipe book: an icon button (book), toggling the list.
            Text book = GearButton(craftPanel.transform, "RECIPES", 0.80f, 0.765f, 0.96f, 0.875f, ToggleRecipeBook);
            if (HudIconStyle.ApplyTo((RectTransform)book.transform.parent, "book", true)) book.enabled = false;
            GearButton(craftPanel.transform, "AUTOFILL", 0.64f, 0.555f, 0.96f, 0.635f, CraftAutofill);
            GearButton(craftPanel.transform, "CRAFT", 0.64f, 0.455f, 0.96f, 0.535f, CraftSelected).resizeTextMaxSize = 24;
            craftSlotsInfo = Label(craftPanel.transform, "", 0.64f, 0.30f, 0.96f, 0.44f, 14); craftSlotsInfo.color = DurabilityStyle.Text;
            craftSlotsInfo.resizeTextForBestFit = true; craftSlotsInfo.resizeTextMinSize = 9; craftSlotsInfo.resizeTextMaxSize = 14;

            // Output preview under the grid.
            RectTransform preview = Rect("Output", craftPanel.transform, 0.04f, 0.205f, 0.96f, 0.285f);
            Image previewBack = preview.gameObject.AddComponent<Image>(); previewBack.sprite = DurabilityStyle.RoundedSprite; previewBack.type = Image.Type.Sliced; previewBack.color = DurabilityStyle.Slot; previewBack.raycastTarget = false;
            craftPreview = Label(preview, "", 0.02f, 0.04f, 0.98f, 0.96f, 17); craftPreview.alignment = TextAnchor.MiddleLeft; craftPreview.color = DurabilityStyle.Text;
            craftPreview.resizeTextForBestFit = true; craftPreview.resizeTextMinSize = 9; craftPreview.resizeTextMaxSize = 17;

            // Batch sizes, mastery line and RETURN (bottom right).
            // Debug (removed with the debug cleanup): the selected recipe's ingredients for the chosen batch size.
            GearButton(craftPanel.transform, "+ INGREDIENTS", 0.64f, 0.115f, 0.96f, 0.19f, GrantRecipeIngredients);
            int[] sizes = { 1, 10, 100, 1000 };
            for (int i = 0; i < sizes.Length; i++)
            {
                int size = sizes[i];
                Text label = GearButton(craftPanel.transform, "x" + size, 0.04f + i * 0.145f, 0.115f, 0.175f + i * 0.145f, 0.19f, () => { craftSize = size; craftFilled = false; RefreshCraftPanel(); });
                craftSizes.Add((label.transform.parent.GetComponent<Button>(), label, size));
            }

            // Centred presentation of a new product (one second, menu stays open).
            craftShow = Rect("Presentation", craftPanel.transform, gx0, gy0, gx1, gy1);
            Image showBack = craftShow.gameObject.AddComponent<Image>(); showBack.sprite = DurabilityStyle.RoundedSprite; showBack.type = Image.Type.Sliced; showBack.color = new Color(0.07f, 0.12f, 0.13f, 1f);
            craftShowIcon = Rect("Icon", craftShow, 0.15f, 0.3f, 0.85f, 0.95f).gameObject.AddComponent<Image>(); craftShowIcon.preserveAspect = true; craftShowIcon.raycastTarget = false;
            craftShowText = Label(craftShow, "", 0.04f, 0.02f, 0.96f, 0.3f, 18); craftShowText.color = DurabilityStyle.Selection; craftShowText.fontStyle = FontStyle.Bold;
            craftShowText.resizeTextForBestFit = true; craftShowText.resizeTextMinSize = 9; craftShowText.resizeTextMaxSize = 18;
            craftShow.gameObject.SetActive(false);

            // Recipe book overlay.
            craftBook = Rect("Recipe book", craftPanel.transform, 0.03f, 0.11f, 0.97f, 0.88f);
            Image bookBack = craftBook.gameObject.AddComponent<Image>(); bookBack.sprite = DurabilityStyle.RoundedSprite; bookBack.type = Image.Type.Sliced; bookBack.color = new Color(0.06f, 0.11f, 0.12f, 1f);
            craftBook.gameObject.SetActive(false);
            craftPanel.SetActive(false);
        }

        // Recipe-book pages (user, October 31, 2026): one tab per category with an icon, like page guides.
        private static readonly RecipeRole[] BookOrder = { RecipeRole.Material, RecipeRole.Tool, RecipeRole.Wearable, RecipeRole.Food, RecipeRole.Misc };
        private static readonly string[] BookIcons = { "cat_material", "cat_tool", "cat_wearable", "cat_food", "cat_misc" };
        private static readonly string[] BookNames = { "MATERIALS", "TOOLS", "CLOTHING", "FOOD", "MISC" };
        private RecipeRole bookPage = RecipeRole.Material;

        private bool CanCraftNow(RecipeDefinition recipe)
        {
            foreach (RecipeInput input in recipe.Inputs) if (ledger.CountIngredient(input.ItemId) < input.Quantity) return false;
            return true;
        }

        // The current page's known recipes: those craftable with the carried ingredients first, then by tier and name.
        private void BuildRecipeBook()
        {
            foreach (Transform child in craftBook) Destroy(child.gameObject);
            // Tabs.
            for (int i = 0; i < BookOrder.Length; i++)
            {
                RecipeRole role = BookOrder[i];
                float x0 = 0.02f + i * 0.196f;
                RectTransform tab = Rect(BookNames[i] + " tab", craftBook, x0, 0.86f, x0 + 0.17f, 0.98f);
                Image plate = tab.gameObject.AddComponent<Image>(); plate.sprite = DurabilityStyle.RoundedSprite; plate.type = Image.Type.Sliced;
                tab.gameObject.AddComponent<Button>().onClick.AddListener(() => { bookPage = role; BuildRecipeBook(); });
                bool selectedTab = role == bookPage;
                if (!HudIconStyle.ApplyTo(tab, BookIcons[i], true)) Label(tab, BookNames[i], 0, 0, 1, 1, 14).color = DurabilityStyle.Text;
                plate.color = selectedTab ? new Color(0.98f, 0.92f, 0.78f, 1f) : new Color(0.62f, 0.56f, 0.46f, 1f);
                Image ring = Rect("Frame", tab, -0.04f, -0.06f, 1.04f, 1.06f).gameObject.AddComponent<Image>(); ring.sprite = DurabilityStyle.FrameSprite; ring.type = Image.Type.Sliced; ring.raycastTarget = false;
                ring.color = selectedTab ? DurabilityStyle.Selection : new Color(0, 0, 0, 0);
            }
            Text pageTitle = Label(craftBook, BookNames[System.Array.IndexOf(BookOrder, bookPage)], 0.03f, 0.80f, 0.97f, 0.855f, 18);
            pageTitle.alignment = TextAnchor.MiddleLeft; pageTitle.color = DurabilityStyle.Selection; pageTitle.fontStyle = FontStyle.Bold;

            // Page list.
            RectTransform page = Rect("Page", craftBook, 0.02f, 0.02f, 0.98f, 0.79f);
            var scroll = page.gameObject.AddComponent<ScrollRect>();
            RectTransform viewport = Rect("Viewport", page, 0, 0, 1, 1); viewport.gameObject.AddComponent<RectMask2D>();
            Image catcher = viewport.gameObject.AddComponent<Image>(); catcher.color = new Color(0, 0, 0, 0); // drag anywhere to scroll
            RectTransform content = Rect("Content", viewport, 0, 1, 1, 1); content.pivot = new Vector2(0.5f, 1f);
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            float y = 0f; const float rowHeight = 64f;
            void Row(RectTransform row, float height) { row.anchorMin = new Vector2(0, 1); row.anchorMax = new Vector2(1, 1); row.pivot = new Vector2(0.5f, 1f); row.anchoredPosition = new Vector2(0, -y); row.sizeDelta = new Vector2(0, height - 4f); y += height; }
            var known = new List<RecipeDefinition>();
            foreach (RecipeDefinition r in RecipeBook.All) if (r.Role == bookPage && crafting.Knows(r.Id)) known.Add(r);
            var craftable = new Dictionary<RecipeDefinition, bool>(); foreach (RecipeDefinition r in known) craftable[r] = CanCraftNow(r);
            known.Sort((a, b) => craftable[a] != craftable[b] ? (craftable[a] ? -1 : 1) : a.Tier != b.Tier ? a.Tier.CompareTo(b.Tier) : string.CompareOrdinal(a.Name, b.Name));
            if (known.Count == 0)
            {
                RectTransform none = Rect("None", content, 0, 0, 1, 0); Row(none, 120f);
                Label(none, crafting.Known.Count == 0 ? "No recipes known yet.\nMissions unlock recipes (Milestone 8).\nDebug: LEARN T1 RECIPES." : "No known recipes on this page yet.", 0, 0, 1, 1, 18).color = DurabilityStyle.Text;
            }
            foreach (RecipeDefinition recipe in known)
            {
                RectTransform row = Rect(recipe.Id, content, 0, 0, 1, 0); Row(row, rowHeight);
                Image back = row.gameObject.AddComponent<Image>(); back.sprite = DurabilityStyle.RoundedSprite; back.type = Image.Type.Sliced; back.color = recipe == craftRecipe ? new Color(0.2f, 0.3f, 0.3f) : DurabilityStyle.Slot;
                RecipeDefinition chosen = recipe;
                row.gameObject.AddComponent<Button>().onClick.AddListener(() => SelectRecipe(chosen));
                Image icon = Rect("Icon", row, 0.01f, 0.06f, 0.12f, 0.94f).gameObject.AddComponent<Image>(); icon.preserveAspect = true; icon.raycastTarget = false;
                Sprite sprite = ItemIconRenderer.For(ItemArt.ModelFor(recipe.Output)); if (sprite != null) icon.sprite = sprite; else icon.color = new Color(0, 0, 0, 0);
                Text name = Label(row, recipe.Name, 0.14f, 0.38f, 0.76f, 1f, 18); name.alignment = TextAnchor.MiddleLeft; name.color = DurabilityStyle.Text;
                Text ready = Label(row, craftable[recipe] ? "Can craft" : "Missing ingredients", 0.14f, 0f, 0.76f, 0.42f, 13); ready.alignment = TextAnchor.MiddleLeft;
                ready.color = craftable[recipe] ? DurabilityStyle.Healthy : new Color(DurabilityStyle.Text.r, DurabilityStyle.Text.g, DurabilityStyle.Text.b, 0.45f);
                string mastery = recipe.Levelled ? $"L{crafting.LevelOf(recipe.Id)}" : "No mastery";
                Text level = Label(row, mastery, 0.76f, 0, 0.98f, 1, 16); level.alignment = TextAnchor.MiddleRight; level.color = DurabilityStyle.Selection;
            }
            content.sizeDelta = new Vector2(0, y);
        }

        private void ToggleRecipeBook()
        {
            bool open = !craftBook.gameObject.activeSelf;
            craftBook.gameObject.SetActive(open);
            if (open) { if (craftRecipe != null) bookPage = craftRecipe.Role; craftBook.SetAsLastSibling(); BuildRecipeBook(); }
        }

        // While the bag is open (alone or with the equipment or crafting menu) the bag button shows the return icon and
        // closes them all (user, November 5, 2026: every menu closes with the return button).
        private string bagButtonIcon = "inventory";
        private void SyncBagButtonIcon()
        {
            string wanted = inventoryOpen ? "return" : "inventory";
            if (wanted == bagButtonIcon || inventoryButton == null) return;
            if (HudIconStyle.ApplyTo(inventoryButton, wanted, true)) bagButtonIcon = wanted;
        }

        private void SelectRecipe(RecipeDefinition recipe)
        {
            craftRecipe = recipe; craftSize = 1; craftFilled = false;
            craftBook.gameObject.SetActive(false);
            RefreshCraftPanel();
        }

        private void RefreshCraftPanel()
        {
            if (craftPanel == null) return;
            craftHint.text = feedback.text;
            craftSlotsInfo.text = (crafting.Batches.Count == 0 ? "Nothing crafting" : $"Crafting: {crafting.Batches.Count}") + NextReadyText();
            for (int i = 0; i < 9; i++)
            {
                var (frame, icon, count) = craftCells[i];
                bool used = craftRecipe != null && i < craftRecipe.Inputs.Length;
                icon.enabled = used; frame.color = DurabilityStyle.Border; count.text = "";
                if (!used) { frame.color = new Color(DurabilityStyle.Border.r, DurabilityStyle.Border.g, DurabilityStyle.Border.b, 0.35f); continue; }
                RecipeInput input = craftRecipe.Inputs[i];
                ItemDefinition item = Items.Find(input.ItemId);
                Sprite sprite = item != null ? ItemIconRenderer.For(ItemArt.ModelFor(item)) : null;
                icon.sprite = sprite; icon.color = sprite != null ? (craftFilled ? Color.white : new Color(1, 1, 1, 0.55f)) : new Color(0, 0, 0, 0);
                long need = (long)input.Quantity * craftSize, have = ledger.CountIngredient(input.ItemId);
                count.text = $"{Short(have)}/{Short(need)}";
                count.color = have >= need ? DurabilityStyle.Healthy : DurabilityStyle.Critical;
                if (craftFilled) frame.color = DurabilityStyle.Selection;
                if (sprite == null) count.text = (item?.Name ?? input.ItemId) + "\n" + count.text;
            }
            foreach (var (button, label, size) in craftSizes)
            {
                bool allowed = craftRecipe != null && CraftStats.BatchAllowed(craftRecipe, size);
                button.interactable = allowed;
                label.color = !allowed ? new Color(DurabilityStyle.Text.r, DurabilityStyle.Text.g, DurabilityStyle.Text.b, 0.3f) : size == craftSize ? DurabilityStyle.Selection : DurabilityStyle.Text;
            }
            if (craftRecipe == null)
            {
                craftPreview.text = "Open RECIPES and choose a recipe."; craftOutputIcon.enabled = false; craftLevelInfo.text = ""; craftRecipeName.text = "No recipe selected";
                craftLevelStar.enabled = false; craftProgressFill.rectTransform.anchorMax = new Vector2(0, 1); return;
            }
            CraftResult result = CraftStats.Compute(craftRecipe, crafting.UnitsOf(craftRecipe.Id), craftSize);
            ItemDefinition output = craftRecipe.Output;
            Sprite outputSprite = ItemIconRenderer.For(ItemArt.ModelFor(output));
            craftOutputIcon.enabled = outputSprite != null; craftOutputIcon.sprite = outputSprite;
            string durability = result.MaxDurability > 0 ? (output.GearSlot.HasValue ? " | " + Distance(result.MaxDurability) : $" | {result.MaxDurability} uses") : "";
            craftPreview.text = $"x{result.OutputUnits}  |  Q{result.Quality}  |  {result.UnitValue:N0} cash each{durability}  |  {FormatTime(result.Seconds)}";
            craftRecipeName.text = craftRecipe.Name;
            long units = crafting.UnitsOf(craftRecipe.Id);
            craftLevelStar.enabled = craftRecipe.Levelled && LevelStar(result.Level) != null;
            if (craftLevelStar.enabled) craftLevelStar.sprite = LevelStar(result.Level);
            float progress = 0f;
            if (craftRecipe.Levelled)
            {
                if (result.Level >= 10) progress = 1f;
                else { long from = CraftingMastery.Thresholds[result.Level - 1], to = CraftingMastery.Thresholds[result.Level]; progress = Mathf.Clamp01((float)(units - from) / Mathf.Max(1, to - from)); }
            }
            craftProgressFill.rectTransform.anchorMax = new Vector2(progress, 1);
            craftLevelInfo.text = !craftRecipe.Levelled ? "Material: no mastery" : result.Level >= 10 ? $"Level 10 (max), {units:N0} crafted" : $"Level {result.Level}: {CraftingMastery.UnitsToNext(units)} more to level {result.Level + 1}";
        }

        private static string Short(long value) => value >= 10000 ? (value / 1000) + "k" : value.ToString("N0", CultureInfo.InvariantCulture);

        private string NextReadyText()
        {
            CraftBatch next = null;
            foreach (CraftBatch b in crafting.Batches) if (next == null || b.ReadyAtUtc < next.ReadyAtUtc) next = b;
            if (next == null) return "";
            return $"\nNext: {RecipeBook.Find(next.RecipeId)?.Output?.Name} {FormatTime((next.ReadyAtUtc - DateTime.UtcNow).TotalSeconds)}";
        }

        // Autofill checks the exact ingredients for the chosen batch (lowest quality is used first when crafting).
        private void CraftAutofill()
        {
            if (craftRecipe == null) { Say("Choose a recipe first."); return; }
            var preview = CraftingService.Preview(ledger, crafting, craftRecipe, craftSize);
            craftFilled = preview.Success;
            Say(preview.Success ? "Ingredients ready. Tap CRAFT." : preview.Reason + ".");
            RefreshCraftPanel();
        }

        private void CraftSelected()
        {
            if (craftBusy || craftRecipe == null) return;
            if (!craftFilled) { Say("Tap AUTOFILL first."); return; }
            var outcome = CraftingService.Craft(ledger, crafting, craftRecipe, craftSize, DateTime.UtcNow, () => Guid.NewGuid().ToString("N"));
            craftFilled = false;
            if (!outcome.Success) { Say(outcome.Reason + "."); RefreshCraftPanel(); return; }
            crafting.LastCrafted = craftRecipe.Id;
            statistics.AddCraft(craftRecipe.Id, outcome.Result.OutputUnits);
            Redraw(); SaveNow();
            Say($"Crafted {craftRecipe.Output.Name} x{outcome.Result.OutputUnits}" + (outcome.ToOverflow > 0 ? " (bag full: sent to overflow)." : "."));
            StartCoroutine(PresentCraft(outcome.Result));
        }

        // The product shows centred on the grid for one second (the timer is already running), then drops into the bag.
        private IEnumerator PresentCraft(CraftResult result)
        {
            craftBusy = true;
            craftShow.gameObject.SetActive(true); craftShow.SetAsLastSibling();
            craftShowIcon.sprite = ItemIconRenderer.For(ItemArt.ModelFor(craftRecipe.Output)); craftShowIcon.enabled = craftShowIcon.sprite != null;
            string durability = result.MaxDurability > 0 ? (craftRecipe.Output.GearSlot.HasValue ? " | " + Distance(result.MaxDurability) : $" | {result.MaxDurability} uses") : "";
            craftShowText.text = $"x{result.OutputUnits}  Q{result.Quality}{durability}  {FormatTime(result.Seconds)}";
            float t = 0;
            while (t < 1f) { t += Time.unscaledDeltaTime; craftShow.localScale = Vector3.one * (0.85f + 0.15f * Mathf.SmoothStep(0, 1, Mathf.Min(1, t * 4f))); yield return null; }
            for (float s = 0; s < 1f; s += Time.unscaledDeltaTime / 0.25f) { craftShow.localScale = Vector3.one * (1f - s * 0.8f); craftShow.anchoredPosition = new Vector2(0, -s * 300f); yield return null; }
            craftShow.localScale = Vector3.one; craftShow.anchoredPosition = Vector2.zero; craftShow.gameObject.SetActive(false);
            craftBusy = false;
            RefreshCraftPanel();
        }
    }
}
