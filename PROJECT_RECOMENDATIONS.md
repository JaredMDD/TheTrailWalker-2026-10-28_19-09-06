# TheTrailWalker: Revised Design and Step-by-Step Development Plan

Updated: October 29, 2026.

**Latest confirmed additions:** balloon boxes gain more optional, stronger Food recipes using ingredients from discovered regions; the camp map shows current-region camps/segments and previously discovered regions below its first camp; same-region travel arrives at the selected camp, while cross-region travel arrives at the destination's first camp. Consumed food/drinks restore stamina progressively while walking and preserve pending recovery above the current meter capacity. See sections 4.4.3, 9.4.2, and 12.3. Recipe amounts, recovery rates, and bakery selections below are proposed starting values.

**Confirmed section-20 answers:** crafting stays open and presents the product at the center of its 3 x 3 grid before dropping it into the backpack with its timer; inventory uses free placement without rotation; tool durability follows family/tier and doubled Elite baselines; NPC discounts are enabled with level-one random-quality gear and no furniture bonuses; missions match only exact item IDs/quantities; chests use one horizontal scrolling container; warehouse accepts every ready sellable item and pays 100% cash plus 3% gold per ten completed boxes, without extra bonuses. Full-stack testing remains active; after testing each gathered object moved to inventory gives one item. Sections below incorporate these answers and the delegated defaults for questions 5 and 9.

**Latest pet/truffle revision:** preserve the house-level-three pet shop, house-front pet home for selecting one owned pet, and separate home-front idle-reward collection container. Section 13.5 proposes six pets with distinct loot weights, acquisition and source filters. Add exact Truffle as a tier-two gathering ingredient; the proposed region-10 assignment replaces loose Lavender, retaining lavender in discovered farm stock and keeping three loose resources per region. Section 9.4.3 adds one mission recipe and two optional box recipes; the box collection becomes 26 recipes (21 Food, 5 Misc). Pet species, weights, prices and recipe values are proposed starting data.

## 1. Purpose, authority, and current status

This document analyzes `Trail.md`, reconciles conflicting rules, proposes additional resources and recipes, and defines an implementation sequence for a solo developer learning Unity. `Trail.md` remains the original design record and has not been edited. No gameplay code, packages, or Unity settings were changed while preparing this plan.

Labels used below:

- **Requirement:** a rule from `Trail.md` to preserve.
- **Confirmed:** an explicit subsequent instruction from the developer.
- **Recommendation:** a proposed refinement, not an already approved replacement of a requirement.
- **Starting value:** editable test data, not a promise of final game balance.
- **Deferred:** implement after the mechanics and limits have been tested.

**Confirmed testing priority:** every stackable item picked up from the trail gives a full stack of **1,000 units**. Keep this behavior while testing all mechanics and limits. Do not reduce pickup quantities for natural progression or player enjoyment yet. Tools and equipment remain individual, nonstackable instances. Future balancing must be a separate phase.

**Confirmed crafting revision:** material recipes have no mastery level. Conversion examples are 3 branches -> 1 log and 3 logs -> 1 plank. A material that can also be gathered has no crafting timer. Other recipes have levels 1-10 from crafted quantity, with quality capped at Q5; levels above five continue improving price, applicable durability, and category-specific timers. Durability has three layers: recipe-level base durability, a bonus from the finished product's quality, and the matching furniture-durability bonus. Craft-quality furniture contributes to the finished quality rather than being the only source of the quality durability bonus. Furniture never changes the timer.

**Current mission rule:** the latest `Trail.md` section, **revisions missions and region resources**, supersedes the earlier three-track/strict recipe-category order. There are four branches: Gatherer (hat), Lumberjack (boots), Hunter (shirt), and Explorer (pants). Each branch has its own ordered mixed-recipe missions with explicit prerequisites; only one mission is active globally. Section 11 preserves the new first-tier examples and plans later advanced-material/backpack/food/Misc unlocks.

**Current tool rules:** three families (axes, hunting tools, fishing rods), Normal only at tier one, Normal/Elite from tier two. Elite has greater base durability, fewer required actions against targets at/below its tier (subject to a one-action floor), and boosted drops for **every eligible target**, including higher-tier targets. The earlier equal-action rule is superseded. Both variants can reach at most two tiers above their own; without an eligible carried tool, no interaction starts.

**Latest Elite recipe/mission revision:** craft an Elite from **one exact same-tier Normal tool plus previous-tier advanced materials**, not a fresh same-tier advanced-material bundle. The normal instance is consumed, the Elite is a new x1-crafted instance, and exact ingredient identities remain enforced. Collection missions start at ten items for tier one and reach at most **150 items for tier four**; intermediate tier defaults are proposed in section 11.4. Full-stack pickups remain 1,000 during testing.

**Confirmed stamina/mission/trading revisions:** hearts are stamina only; walking consumes it and food/drinks restore it. Stamina regenerates progressively at camps and in settlement, including during trading. Animals are passive moving targets, with aiming but no combat or player damage. Missions consume only required quantities and may consume carried tools or spare equipment directly; currently equipped clothing/backpack are protected. Tools in bags or hooks are not equipped and are eligible. Bulk crafting is removed for all unstackable outputs. Furniture buffs unlock on first purchase only. Camp trading exclusively occupies a 15-second session; Return cancels it, sells remaining player listings at 70%, and spends that visit's trading opportunity.

**Confirmed camera, breakage, and stop/resume revision:** horizontal hold/swipe gestures orbit the camera through 360 degrees without stopping or steering the player. Avatar vertical speed gestures remain the way to slow/stop/restart walking. Worn equipment loses durability by walked distance, disappears at zero, and immediately reverts its slot to no hat/no boots/rag shirt/rag pants/rag pack with all that item's bonuses removed. Defaults remain three stamina hearts and slow walking; boots increase the speed cap only up to brisk walking and hats increase maximum hearts only up to twelve. Harvesting stops the player at animation start and leaves them stopped afterward until a new upward speed gesture. Inventory opening temporarily stops movement; closing restores its saved pre-open speed, subject to current caps and other stop conditions.

**Confirmed capacity and scope revision:** rag shirt/pants start at **zero pockets/zero hooks**; the independent maximum is **ten shirt pockets and ten pants hooks**. Stack quantity remains capped at 1,000 and stack model growth at **1.5 times its base linear dimensions**. Inventory restrictions use item size and backpack capacity only, never weight. The current target is **24 regions sharing four wood/animal tiers**. The other four tiers are a deferred expansion backlog with no mandatory dependencies in current progression.

**Confirmed crafting/store revision:** materials support player recipes, advanced containers, and house upgrades, but do not craft furniture. Furniture, including production-looking pieces and storage furniture, is bought from the furniture store after its house-level unlock. Abstract real-life processes into a few meaningful ingredients: cheese uses milk and salt, with waiting time representing omitted steps. House-upgrade bricks/glass panels are valid; direct window/oven furniture crafting is not.

**Confirmed recipe identity revision:** every recipe has a unique ID and a fixed list of exact ingredient item IDs/quantities. No ingredient variety is interchangeable: cow milk never substitutes for goat milk, birch never substitutes for pine, and a higher-tier material never substitutes automatically. A deliberately mixed recipe lists both varieties and requires each stated quantity. Separate variants are separate recipe definitions/unlocks/mastery counters, not one recipe with an ingredient selector. Grouped chains below describe families only; runtime recipes must be concrete.

**Confirmed price revision:** the simplest tier-one Q1 material starts at **1 cash per unit**. A level-ten Misc reference item has a **6,000-cash Q1-equivalent base unit price**, before quality/durability premiums. Base-price priority is **Misc -> Backpacks -> Clothing -> Tools -> Advanced materials -> Food -> Other materials/ingredients -> Basics**. Quality and applicable crafted-item durability raise the final price above that base. The 6,000 reference is not a final Q5 sale-price cap; the per-tier distributions and bonus percentages below are proposed test values.

**Confirmed store-quality revision:** every store-bought item is **Q5**, whether bought with cash or gold. The only purchase exception is the camp's 15-second conveyor trading session, where goods retain their offered quality. The balloon shop is Q5 even when physically located at a camp. Q5 is product quality, not automatic tier-four equipment, recipe mastery ten, or infinite durability.

**Latest regional/recovery revision:** each of 24 regions highlights three different loose-gathering resources: 18 unique loose resources per six-region tier, 72 across the active campaign. Each region also has its tier's tree/hunting/fishing interactables. Branches and the tier's pelt/hide are the two loose/interactable overlaps; tier three uses **turkey feathers**, as confirmed in follow-up. Logs, meat, other hunting outputs, and fish are not extra loose-table entries. Early regions have 10-14 camps, later regions grow toward 24. The first region unlocks food before longer food-requiring routes. Zero stamina with neither pending food recovery nor accessible ready food/drink triggers fainting, recovery, and random loss of 2-4 eligible carried entries/stacks, excluding worn gear.

**Latest distance cap:** the highest active-tier camp-to-camp segment costs **at most 26 stamina hearts from walking**. Hats still cap at twelve; the extra route cost must be met through food/drinks, not a larger stamina meter. Numeric lower-tier budgets/distances/rates are proposals in section 6.2.4.

**Latest tool/fishing/cooking revision:** the tools section explicitly includes Normal pine axe, slingshot, and rod. The proposed full fishing catalog grows from eight to sixteen species: four per resource tier, allowing six different two-species region pairings within each six-region tier block. Roasted meat/fish recipes consume exact named wood as fuel/skewer material; tier-one roast uses one meat + one Pine branch, and larger game can use more branches or a log plus branches.

**Confirmed recipe-tier rule:** a recipe first unlocked in tier one uses tier-one ingredients; tier two uses tiers one/two; tier three uses tiers two/three; tier four uses tiers three/four. This is its authored unlock tier, not mastery or current player location. **Cooking exception:** explicit global pantry ingredients such as flour and cheese can remain usable in later Food recipes. Exact ingredient IDs still apply, and the exception never grants an inaccessible future ingredient or applies to tools/material/Misc recipes.

**Latest fishery/box revision:** every fishery reset offers **two fish stacks and one random exclusive seafood stack**, all for cash and all Q5, with at most 1,000 units in each offer per reset. Shrimp/crab/lobster are store-exclusive examples; Squid is a proposed fourth exclusive type. Their recipes can appear by chance in balloon mystery boxes, along with more optional Food/Misc recipes. Recipe rewards are knowledge, not finished Q5 products or 1,000 scrolls; a newly learned nonmaterial recipe starts at mastery one. Physical box rewards remain Q5.

The previous recommendation of free movement inside a corridor is superseded by forward-only path movement. The previous weight-capacity recommendation is superseded by size-based backpack capacity throughout the game. No joystick is needed for normal trail movement.

### 1.1 Rechecked project baseline

| Area | Observed state | Implication |
| --- | --- | --- |
| Unity | `6000.4.7f1`, Unity 6.4 | Keep the editor version stable during the prototype |
| Rendering | URP `17.4.0` | Suitable starting point for mobile |
| Quality | Android selects Mobile; saved current quality/default graphics pipeline use PC | Preview and profile Mobile explicitly |
| Mobile renderer | Forward, render scale 0.8, one cascade, no soft shadows, no required depth/opaque textures | Retain the inexpensive defaults |
| Mobile extras | HDR on; shadow distance 50; sample camera post-processing on | Disable unnecessary effects for initial profiling |
| Android | IL2CPP, ARM64, Vulkan then OpenGL ES 3 | Suitable baseline; still needs a device build |
| Android SDK settings | Minimum API 25; target API automatic/highest installed | Choose supported devices deliberately; verify release requirements later |
| Input | Input System `1.19.0`, new system only | Build gesture routing on this |
| UI/testing | uGUI `2.0.0`, Test Framework `1.6.0` | Sufficient for initial UI and domain tests |
| Build scenes | Only `SampleScene.unity` | Replace with bootstrap and prototype trail scenes during implementation |
| Gameplay | No gameplay scripts, prefabs, or assembly definitions | Build foundations before producing lots of content |

The project is still a URP template. Installed Android tools, the active build target, and runtime performance have not been verified. Unity and an APK were not run for this assessment.

## 2. Intended game and scope

The core loop is:

1. Leave a camp and walk forward along a predefined trail.
2. Adjust walking speed through vertical gestures over the avatar and orbit the camera horizontally to collect nearby resources behind the player.
3. Collect full stacks of loose resources and interact with trees, animals, and fishing points.
4. Arrange items in a spatial backpack, shirt pockets, and pants hooks.
5. Manage stamina, food, equipment wear, and tool uses.
6. Reach a camp, trade, claim mission rewards, craft, and prepare.
7. Pay a one-time river toll to discover another region.
8. After region four, establish a settlement home and unlock its long-term systems.

Keep the experience single-player and offline-first initially. NPCs simulate camp trading; neighboring settlement houses are decorative. Networking, real player markets, real-money purchases, combat AI, and procedural worlds are outside the initial scope.

### 2.1 Deliverable boundaries

| Deliverable | Included | Excluded until later |
| --- | --- | --- |
| Movement proof | One curved path, avatar speed gestures, independent 360-degree orbit camera, temporary inventory stop, camp stop | Resources and economy |
| Collection proof | Full-stack pickups, backpack geometry, pause/resume, save/load | Large content catalog |
| Mechanics test slice | Two segments, one camp, basic harvesting, four mission branches, crafting, trading, equipment, faint recovery | Twenty-four finished regions |
| Settlement proof | Reachable test settlement, home, storage, warehouse, one massive-tree tier | All furniture art and shops |
| Full mechanics build | All system types, placeholder content, debug access to limits | Natural progression tuning |
| Progression pass | Regional content, prices, quantities, pacing, polished presentation | Additional systems without a demonstrated need |

Use debug travel to test settlement and late-tier content before all 24 regions are authored. Author one representative segment per active resource tier first; the 24-region layout is a content plan, not a prerequisite for validating mechanics. Normal progression still introduces settlement between regions four and five, both within the first resource tier.

## 3. Decisions and contradictions to resolve in the design

| Topic in `Trail.md` | Analysis | Planned interpretation or recommendation |
| --- | --- | --- |
| Pickup stacks versus random drop counts | A 2-5 resource roll is ambiguous if every pickup is 1,000 units | **Confirmed:** pickups contain 1,000 units. **Recommendation:** interpret random counts as numbers of bundles; see section 7 |
| Backpack area versus old weight proposal | Weight is explicitly excluded | Use item footprint/fit and backpack area only throughout the game; no weight or encumbrance restriction |
| Resources/hooks wording | One sentence appears to swap where resources and tools belong | Resources go in eligible shirt pockets; tools go on pants hooks |
| Tool families, tiers, and variants | Equal-action and same-tier advanced Elite recipes are superseded | Elite consumes exact Normal tool + previous-tier advanced material; fewer actions up to own tier, longer lifetime, drop boost for every eligible target |
| Tool effectiveness versus doubling uses | Same-rank 3 uses and 5-6 axe uses conflict with universal doubling | Use the explicit interaction matrix in section 8; keep the alternative doubling curve as a test preset |
| Health/stamina hearts | A separate health system would conflict with passive hunting | Stamina only; walking drain, food/drink recovery, progressive camp/settlement recovery; no combat |
| Missions consume acquired items automatically | Stack quantities and unique equipment need different handling | Consume exact required units/instances; spare gear and carried tools are eligible; only worn clothing/backpack are protected |
| Mission rewards at camps | Completing a mission at home should not silently start another | Mark completed, then claim/unlock the next step from the standing NPC at a camp |
| Bulk crafting nonstackable gear | Gear bulk crafting is explicitly removed | Tools/wearables/backpacks are x1 only in UI and domain validation; no temporary stacks or bulk claims |
| Craft timers on products | Different completion timestamps prevent normal stacking | Keep pending batches separate; merge compatible stackables only after readiness |
| Material recipes versus mastery | Conversions and finished goods have different progression rules | Materials never level; gatherable material outputs are instant; all other recipe roles level 1-10 |
| Q5 goods at mastery levels 5-10 | Equal displayed quality does not imply equal value or durability | Preserve per-output stat profiles and price by those snapshots |
| Mission order | Latest `Trail.md` examples mix recipe categories and add Explorer | Four independent mixed-recipe branches with source/recipe prerequisites; the old category-phase gate is removed |
| Restock on leaving and entering | Visit identity controls refreshes and repeat trading | Preserve shop visit-based behavior; completed/canceled camp trades refresh only after leaving and returning or reaching a different camp |
| Warehouse gold | Previously ambiguous exchange denomination | Confirmed direct 3% of cash paid for ten boxes; 10,000 cash -> 300 gold, fractional carry, no extra bonuses |
| Furniture grants a buff on first purchase | The first-purchase behavior is now confirmed | Permanent unlock per furniture definition; moving/storing/selling/rebuying never grants another copy of its buff |
| Camp crafting and production furniture | Mandatory settlement stations could prevent camp crafting | Furniture improves recipes; basic unlocked recipes remain craftable at camps/home unless explicitly station-tagged later |
| Mystery box full stacks and a full bag | Six full stacks can exceed available area | Use a persistent reward tray; never delete or force-place overflow |
| Pocket eligibility | A tiny stone can be small while a 1,000-stack is visibly large | Preserve one eligible stack per pocket regardless of quantity; make this intentional compression explicit |
| Resource point and camp timing | Harvesting/trading/menu time could drain stamina or move the player away | Apply explicit pause reasons and context states; do not use global time scale for everything |
| Camera versus movement gestures | Looking back must not stop, turn, or reverse walking | Horizontal orbit changes camera only; vertical avatar gestures control requested speed |
| Harvest stop versus inventory stop | Resuming every pause would move the player away from fresh drops | Harvest start clears walking intent; a fresh upward gesture restarts after harvesting. Inventory restores its own prior speed only |
| Worn equipment at zero durability | Earlier plan retained broken storage equipment | Remove exhausted instance and its bonuses immediately, restore default slot, and preserve displaced contents through a safe transfer/overflow process |

The recommendations in this table are implementation defaults to make systems testable. They should remain data-driven where practical. The full-stack pickup rule is confirmed and must not be weakened by these defaults.

## 4. Movement, gestures, camera, and stamina

### 4.1 Forward-only path travel

Represent the trail as an authored spline or sampled path. Store segment ID and distance along the path in meters. Advance by `speedMetersPerSecond * deltaTime`; derive position and facing from the path. Build an arc-length lookup so curves do not change apparent speed. Unity Splines is a reasonable authoring option; a waypoint path behind an `ITrailPath` interface is enough for the first proof.

Do not use a NavMeshAgent for the player. A CharacterController is optional for collision/ground adjustment, not the source of route selection. The player never freely steers left, right, or backward. Sampled ground height or an authored elevated path handles hills. Keep the camera independent from the motor and smooth its position/rotation without excessive banking on bends.

### 4.2 Gesture ownership

Use one `GestureRouter` that assigns a touch to a target on pointer-down and retains ownership until release/cancel:

| Initial touch target | Allowed gesture | Effect |
| --- | --- | --- |
| Avatar screen-space hit area | Vertical drag; mouse wheel over avatar in Editor | Change requested walking speed; upward gesture can restart after harvesting |
| Avatar or unoccupied world area | Horizontal hold-drag or horizontal swipe | Orbit camera left/right through 360 degrees without changing movement |
| Resource | Hold then drag | Pick up and position in backpack |
| Resource | Quick downward swipe | Send toward backpack quick-storage aperture |
| Inventory item | Hold without dragging | Show name, quality, quantity, readiness, durability; food gets Eat |
| Inventory item | Hold then move past drag threshold | Move/split/merge item |
| Tree/animal/pond | Tap or short hold | Start the matching harvest interaction |
| UI control | Tap/drag handled by UI | Never also control movement or world pickup |

Starting thresholds: hold 250 ms; drag threshold scaled to screen density; resource quick swipe roughly 10% of screen height in under 250 ms. Tune on a device. Keep avatar/resource hit regions separate. UI wins, then a selected resource/interaction target, then the avatar/world gesture surface. Resource holds/drags are never reassigned to camera orbit mid-touch. On the avatar, lock the gesture to horizontal camera orbit or vertical speed after an initial directional threshold (recommended dominant axis at least 1.5 times the other). A diagonal gesture must not change speed and camera together. A vertical drag starting on empty world does not change speed. Two touches can own separate camera/speed/resource gestures, but two fingers cannot own the same item.

**Temporary inventory stop:** on the first inventory opening, save the current requested walking speed and set actual movement to zero through an InventoryOpen pause reason. Nested item details/equipment panels do not overwrite that snapshot. Closing the final inventory panel restores the saved requested speed, clamped to the current equipment cap, and removes only that pause reason. If the player was already stopped, the saved speed is zero. Empty stamina, active harvesting, camp arrival, post-harvest collection stop, scene loading, and app suspension still prevent movement; closing inventory never clears them.

**Persistent harvest stop:** starting an eligible chopping/hunting/fishing interaction stops actual movement at the start of its animation and sets requested speed to zero. Once started, completing, missing, canceling, breaking a tool, or closing its interaction UI never restores the old walking speed. Keep the player still to collect drops. Only a fresh upward avatar-speed gesture after the active harvest/aim interaction ends clears the post-harvest stop and requests positive speed. Camera gestures, pickups, eating, and inventory close do not clear it. Ignore upward resume gestures during a committed harvest animation rather than queueing an automatic restart. An ineligible resource tap that never starts an interaction does not alter movement.

Model temporary pause reasons separately from persistent walking intent/post-harvest stop. Example: walk at 1.0 m/s -> harvest -> stop -> open/close inventory -> remain stopped -> upward speed gesture -> walk. By contrast: walk at 1.0 m/s -> open/close inventory -> resume at 1.0 m/s if still permitted. Save this distinction rather than a single easily overwritten moving/not-moving boolean.

### 4.2.1 Independent 360-degree camera orbit

**Confirmed:** permit a full horizontal orbit around the avatar so the player can look behind and pick resources they almost passed. Horizontal hold-drag continuously changes yaw while the finger moves; a horizontal swipe also rotates it. Recommendation: stop rotation on release without inertia, keep the chosen yaw until the player changes it, and avoid automatic recentering during collection. Pitch can stay fixed initially or use a bounded optional adjustment later; 360 degrees refers to horizontal yaw, not flipping the camera vertically.

Keep the follow target anchored to the avatar while camera yaw is independent of the path tangent. Walking continues forward along the trail at its current requested speed throughout camera movement. Camera orbit cannot rotate the avatar's travel direction, reduce speed, add a movement pause, or stop stamina/distance wear while walking. The player must use the separate vertical avatar gesture to slow or stop. Keep the avatar's on-screen speed-hit area usable from every viewing angle.

Recommended implementation: `CameraOrbitController` owns yaw, `FollowCameraRig` handles target tracking/smoothing and wall/ground avoidance, and `TrailMotor` reads neither camera yaw nor orbit input. Avoid orbital collision pushing the player or moving world resources. Store yaw in session/save state. Do not force a camera snap after harvest completion or inventory close; recenter only through an explicit control or scene-entry rule outside an active gesture. Orbit is unavailable during exclusive trading and blocking inventory/transition UI, preserving their existing input rules.

### 4.2.2 Resources behind the player

Looking backward must reveal collectible nearby items rather than only scenery. Do not expire/despawn logical drops simply because their path position or camera-facing dot product is behind the avatar. Keep a configurable rear collection window within the current loaded segment; starting recommendation: 15 meters of trail behind, subject to a modest physical pickup radius and line of sight. Show out-of-reach feedback rather than allowing pickups through walls or from distant adjacent sections of a looping path.

Render/raycast the nearby resource targets from the current camera angle, including behind the avatar, while preserving full-stack quantities and the pooled-view budget. Frustum culling may hide their views but must not consume/delete the underlying loot. Orbit does not extend collection range, rewind the player, keep the previous segment permanently loaded, or collect automatically. If still walking, the player can eventually pass beyond the rear window, so deliberate slowing/stopping matters. Author segment exits so immediate checkpoint unloading does not cut off reachable last-second pickups without warning.

### 4.3 Starting equipment capabilities

Use four active equipment/resource capability tiers, independent of five quality grades and ten recipe mastery levels. Regional advancement within a six-region tier unlocks recipes/variants without another wood/animal species tier. Make the global caps achievable within the current four-tier plan. Intermediate values below are proposed; zero starting pockets/hooks and ten maximum of each are confirmed.

| Rank | Boots max speed, m/s | Hat max hearts | Shirt pockets | Pants hooks | Backpack width x height, logical units |
| --- | --- | --- | --- | --- | --- |
| Starter, no boots/hat | 0.8 | 3 | 0 | 0 | 6 x 4 |
| 1 | 1.0 | 4 | 2 | 2 | 8 x 4 |
| 2 | 1.2 | 6 | 4 | 4 | 10 x 4 |
| 3 | 1.4 | 9 | 7 | 7 | 12 x 4 |
| 4 | 1.6 | 12 | 10 | 10 | 14 x 4 |

Requested speed ranges from zero to the current boots cap. **Confirmed defaults:** no boots gives slow walking (starting value 0.8 m/s); no hat gives three stamina hearts; rag shirt/pants give zero pockets/hooks and the rag pack provides starter area. Initially, tools/resources must fit in the backpack. Highest active-tier boots cap at brisk walking (starting value 1.6 m/s), hats at twelve hearts, shirts at ten pockets, and pants at ten hooks. Merchant gear cannot exceed those caps. Starter rags are nondepleting fallback representations with no sellable instance or extra bonuses. Backpack dimensions are virtual layout units, not kilograms or visible slots.

### 4.4 Stamina and wear

**Confirmed:** hearts represent stamina, not health. Only actual forward trail distance drains it; food/drinks replenish it. Keep distance-based costs so slow walking or harvesting stops do not cost extra. The earlier **2 hearts/km is an opening-region test rate**, not a mandatory global rate. At that rate a 250 m tutorial leg costs 0.5 heart. Later segments author a fixed distance/drain budget, capped at **26 hearts per leg** in the highest tier; section 6.2.4 provides proposals. Standing/menu time, aiming, loading, and teleportation never drain stamina.

At zero stamina, stop. If accessible ready food/drink can restore stamina, show Eat/Drink and let the player use it; do not faint immediately. **Latest requirement:** with neither pending food recovery nor such a carried consumable, faint and apply the recovery/loss procedure in section 4.4.2. The earlier free camp-rescue/no-loss proposal is removed. Camp/settlement regeneration remains continuous, including trading/menus, with no Rest action. Starting rate: 0.05 heart/second, so 15 active trade seconds restore 0.75 heart unless capped. Food/drinks start progressive recovery on use, continuing after inventory closes and walking resumes; unused recovery is retained (section 4.4.3). The global meter maximum stays twelve.

Regeneration is a simulation service, independent of UI input locks and trade animation. Default recommendation: passive safe-area recovery counts only active foreground camp/settlement time; food recovery also counts active foreground trail time. App suspension freezes both recovery services and the remaining trade countdown together. Offline stamina recovery is not required by this revision. Persist current stamina and remaining trade time so reopening cannot restart either interval.

Animals never attack or damage the player. They can walk within a local resource area; hunting requires the user to aim at the moving target, then commit a shot. Use resource harvest progress, not a player combat/health system. Recommendation: a committed miss consumes one tool use but makes no harvest progress; canceling before shot commitment consumes none. Validate tool eligibility before entering aim mode and again before committing a shot.

Tools lose uses only for committed actions. **Confirmed:** worn hats, boots, shirts, pants, and backpacks lose durability by actual trail distance walked, never by camera rotation, menu time, stationary harvesting, teleportation, or settlement/camp time. Example rank durability remains `2 + 2 * rank` km before mastery/quality/furniture bonuses. Carried spare wearables do not wear down. Infinite-durability merchant gear remains exempt.

### 4.4.1 Equipment expiration and immediate defaults

**Confirmed:** when finite worn-item durability reaches zero, remove that item instance entirely, remove every capability/buff supplied by it, and replace its equipped slot with the corresponding default immediately. Do not keep a broken item equipped, preserve its capacity, auto-equip a spare, create a repairable shell, or give a salvage refund.

| Exhausted slot | Immediate fallback | Capability change |
| --- | --- | --- |
| Hat | No hat | Maximum stamina becomes three hearts; clamp current hearts to three without granting a refill |
| Boots | No boots | Maximum walking speed becomes the default slow speed; clamp requested/actual speed immediately |
| Shirt | Rag shirt | Pocket count becomes zero; transfer everything from removed pockets |
| Pants | Rag pants | Hook count becomes zero; transfer every hooked tool |
| Backpack | Rag pack | Return to starter backpack area immediately |

Permanent first-purchase furniture buffs remain unlocked; the bonuses lost here are those of the vanished equipment. Spare gear is not a substitute until the player deliberately equips it. Default rag representations cannot be sold, consumed by a mission, or worn down into an equipment-loss loop.

**Recommended content-preservation rule:** remove exhausted equipment while transferring its contents transactionally. Default shirt/pants have no surviving pockets/hooks: move their entire contents to valid backpack space and any remainder to the persistent overflow tray. If the backpack also expires, use the rag pack's area. The exhausted item never enters the tray. Overflow gives no extra active capacity; its contents cannot be used, sold, or consumed by missions until claimed into valid carried storage. Notify the player without retaining expired-item bonuses to avoid the downgrade.

For a bag expiration, remove the old container/instance and transfer its contents into the rag pack/tray in the same operation. For several items expiring on the same movement update, compute all defaults and plan one transfer before committing, rather than dropping items during sequential resizes. Persist equipment removal, recalculated caps, clamped speed/stamina, and new ownership together so reload cannot restore a destroyed item or duplicate its contents. The earlier recommendation to leave broken storage gear equipped is superseded.

### 4.4.2 Fainting, inventory loss, and wake-up

**Confirmed trigger:** on the trail, stamina reaches zero, no positive consumed-food recovery remains pending, and there is no accessible ready food/drink with positive recovery. Pending crafts, chest/shelf/tray contents, inedible raw ingredients, and food already consumed by a mission do not count. This is recovery, not combat/death. Stop at the current path position, clear walking intent, and play faint/recovery/wake animations. Lock normal pickup/harvest/craft/travel/equipment actions through this state so they cannot race the loss manifest; allow application pause and nonblocking loss/recovery information.

Roll loss count 2-4 and select that many distinct eligible **inventory entries**, without replacement. A selected stack loses its whole remaining quantity, not 2-4 units. Eligible entries include backpack resources/food, pocket stacks, tools in backpack/hooks, and spare clothing/backpacks. Exclude currently worn hat/boots/shirt/pants/backpack, wallet balances, home storage, unclaimed rewards, and craft/trade escrow. If fewer than the rolled count are eligible, lose only those available; an empty inventory loses nothing. Uniform selection per eligible entry is a proposed initial rule, not weight/value-based selection.

Persist a unique faint-event ID, selected item/stack IDs and quantities, committed removals, recovery state, and position **before** showing loss animations. Animate up to the actual 2-4 selected entries slipping out and disappearing; these are presentation objects, not collectible world drops. A 990-unit stack is one entry and all 990 disappear if selected. Do not reroll or charge another penalty on reload/resume, create replacement tools, or reverse already-completed mission contributions.

Stamina progressively refills while fainted, which is a specific recovery exception to normal no-regeneration trail standing. Recommendation: recover to the current hat-derived maximum over 12 active foreground seconds; freeze/save on app suspension. Wake only after recovery and penalty commit are complete, preserve current equipped bonuses, and remain stopped until a fresh upward speed gesture. No teleport, automatic walk, or recipe/currency reward. Tool loss can leave harvesting unavailable, but loose gathering remains possible for rebuilding through known recipes.

### 4.4.3 Progressive food recovery without wasted hearts

**Confirmed:** eating/drinking consumes one serving and adds its full authored recovery, including the applicable food-recovery furniture bonus, to a persistent pending-recovery queue. It never adds all hearts immediately. Food values in every table mean **total hearts over time per serving**, not a raised stamina cap. The player can open inventory, eat, close it, and resume the valid pre-open walking speed while food recovery and distance drain run together. Existing harvest/manual/exhaustion stop conditions still take priority.

**Starting implementation proposal:** one FIFO queue at **0.25 heart/active second**, shared by foods and drinks; six hearts take 24 seconds of actual delivery. Multiple servings add their remaining amounts without multiplying the delivery rate or replacing the active serving. Display current/max stamina, pending food hearts, and recovery rate on the HUD and item popup. Snapshot each serving's effective recovery when consumed; later furniture changes do not revalue it.

Integrate walking drain and recovery continuously in one simulation step (split at queue exhaustion, meter-cap, or zero-stamina boundaries). Spend pending food only for hearts actually supplied to the meter or concurrent walking cost. At maximum stamina while stationary, pause food delivery without expiring or discarding the queue; walking creates room and resumes delivery. Safe-area passive recovery contributes independently at its existing rate and takes priority for free capacity; food supplies the remainder. Passive recovery never spends the food queue. A lower cap after hat expiration clamps only current stamina and preserves pending food. Pending hearts are a consumable reserve, never extra visible meter capacity.

Examples: at 3/3 hearts, consuming a six-heart meal leaves 3/3 plus six pending. Standing keeps the six; walking drains and draws from them progressively. At 2/3 with 0.10 heart/second walking drain, the proposed 0.25 food rate yields +0.15 heart/second until capped, then supplies only ongoing drain. If walking drain exceeds recovery, stamina still falls; food does not promise unlimited walking. At zero with pending positive recovery, stop without fainting, continue recovery, and allow a fresh upward speed gesture once stamina is positive. Faint only when zero, no pending recovery, and no accessible ready consumable remain; this qualifies the trigger in section 4.4.2. Resolve the final recovery tick before evaluating fainting.

Food recovery continues during foreground inventory, harvesting, camp/settlement menus, and trading; trail standing has no free passive regeneration. Freeze delivery during loading/app suspension, save queue balances/order, serving snapshots, fractional timing, and consumption transaction IDs, and resume exactly once without offline food recovery, duplicate consumption, or queue reset on travel. Saving consumption and queue insertion is atomic. Departure previews simulate retained pending recovery plus usable carried food, walking rate, and timing; do not subtract nominal overflow as waste or count the same serving twice. A 26-heart leg still requires sufficient total provision **and** a viable recovery schedule.

## 5. Inventory, stacks, and item identity

### 5.1 Deterministic spatial inventory

**Confirmed:** item size, nonoverlap, bounds, and backpack capacity determine fit. Weight is not a limit, speed penalty, stamina penalty, or secondary encumbrance system. Do not add a weight budget to transfer validation. Fainting is the explicit inventory-loss mechanic; capacity failure still never silently destroys an item.

Keep the slotless appearance, but avoid simulated rigidbody piles in the UI. Represent each entry by a 2D position and rectangular footprint inside a logical container. Hidden placement precision is acceptable; visible slot cells are not required. Validate bounds and overlap deterministically. **Confirmed:** free placement only, with fixed item orientation and no rotation controls. Bounds/overlap checks use that fixed footprint.

Quick storage first attempts a compatible merge, then searches for free space. Manual drag validates the intended placement and merges when compatible stack footprints touch. If a merge enlarges the result, validate the new footprint before consuming the source. Partial merging is allowed up to 1,000; leave the remainder at its prior valid location. A failed pickup stays on the trail and cannot be collected twice by overlapping inputs.

Stack key: definition ID + quality + ready state + output-stat profile and any gameplay-relevant variation. Include per-unit intrinsic price and any other differing functional stats in that profile: two Q5 goods made at recipe levels five and ten can have different prices and must not silently merge. Equivalent profiles may merge even if provenance differs. Pending craft batches never merge with ready items. Tools, clothing, and backpacks have unique instance IDs and never merge.

### 5.2 Bounded stack-size growth

**Confirmed:** quantities increase footprint/model size up to **1.5x base linear model scale** for every stackable item. Quantity cap stays **1,000 units**; quantity and visual scale are separate limits.

Recommended curve: `linearScale(q) = clamp(1 + 0.5 * log(q) / log(1000), 1, 1.5)` for quantities 1-1,000. It produces 1x at one unit and 1.5x at 1,000, with at most 2.25x base rectangular footprint area; the 1.5 cap is per dimension, not an area multiplier. Remove zero-quantity entries before evaluating the logarithm. Use the same capped growth for models and fit; never apply it twice. Full-stack pickups use maximum scale during testing. Shelf fitting and whole-UI scaling are presentation transforms, not extra capacity.

Starting nominal shapes: small gem/egg 0.6 x 0.6; food 0.8 x 0.8; cloth/pelt 1 x 1; branches 1.6 x 0.6; logs 1.8 x 0.9; tool 1.8 x 0.7. Author per-item overrides for unusually large crafts. Do not instantiate 1,000 models; render one icon/model and a quantity badge.

Pockets accept one eligible resource/food stack or individual unit each. Existing stacks can be inserted, but pockets do not combine separate stacks internally. Moving a pocket stack back into the backpack allows normal merging. Hooks hold one compatible tool instance each. Equipables cannot be hidden in a small-resource pocket.

Clamp pocket/hook capability separately to 0-10. Display only the current count, not ten usable positions on starter rags. At ten, use a readable two-column/five-row or scrollable side panel rather than shrinking touch targets. Destroying a shirt/pants upgrade returns its count to zero; ten-pocket/ten-hook contents must transfer safely without retaining invisible slots.

### 5.3 Ownership and transactions

Every item or quantity has exactly one owner: world bundle, backpack, pocket, hook, equipment slot, chest, shelf, craft batch, mission escrow, trade escrow, shop, or reward tray. Transfers either commit entirely or leave ownership unchanged. Never use a pickup animation or UI element as the authoritative item record.

Full-stack collection may divert mission-required units before backpack placement. Example: a 1,000-unit pine bundle, with 12 pine still required, contributes 12 and leaves 988 to place. Show both numbers. If the remainder does not fit, commit neither the collection nor the mission contribution; the bundle remains intact. This preserves an atomic pickup without dropping the leftover units.

Reward trays are persistent delivery queues for crafting, boxes, overflow, and load recovery. They are not consumable/tradable storage and do not count toward missions until claimed. Expose queued quantities and never silently truncate them. Put limits on new batch creation when claim queues are full, while preserving rewards already earned.

## 6. Quality, resource progression, and item taxonomy

### 6.1 Five quality grades and three independent progression axes

| Grade | Display name | Recipe-level baseline for nonmaterial crafts |
| --- | --- | --- |
| Q1 | Rough | Level 1 |
| Q2 | Serviceable | Level 2 |
| Q3 | Fine | Level 3 |
| Q4 | Superior | Level 4 |
| Q5 | Masterwork | Levels 5-10 |

Keep **region number/resource-equipment tier**, **recipe mastery level**, and **output quality** separate. A tier-one axe can reach mastery ten/Q5 without becoming a tier-four axe. There are 24 region numbers but four active resource tiers. Speed, pockets, hooks, bag area, and heart caps come from equipment tier, not mastery/quality. Food recovery stays recipe-defined apart from its separate furniture buff. Food/materials/misc have no durability.

**Confirmed:** material recipes do not level, and their crafted quality is influenced only by craft-quality furniture. For nonmaterials, recipe level establishes base durability and quality up to Q5; levels six to ten retain Q5 but continue changing base price/durability/timer. Finished quality adds durability even without furniture, and craft-quality furniture can raise that quality. Matching durability furniture adds a final durability bonus. Price now uses the Q1-equivalent base, a finished-quality premium, and an applicable durability premium, as section 12.2 defines. None changes craft time.

Use deterministic output-stat calculation, not the earlier random quality-upgrade chance and not lowest-ingredient-quality inheritance. Ingredient quality remains valuable for direct sale and explicit minimum-quality requirements; it does not silently determine crafted output. Autofill still allows a visible ingredient-selection policy so high-value materials are not spent accidentally. Show grade, recipe level, intrinsic value, and durability separately in details.

The exact percentages and grade-combination formula below are recommended starting values that make those rules executable. They are not additional confirmed balance requirements.

**Confirmed furniture scope:** item bonuses from furniture apply only to player-crafted products. Persist `CreationSource` and the creation-time modifier snapshot; purchases, NPC stock, loot and box goods never acquire the player's quality/durability/food/sale furniture buffs merely by ownership. Recipe/quality bonuses themselves still apply. NPC gear uses L1 and random Q with `C = U = 0`; player conveyor listings preserve already-created stats. Food recovery snapshots the eligible bonus once at consumption (zero for non-player-crafted food). Sale-price furniture is eligible only for player-crafted goods in ordinary sales, and **never** in warehouse payouts. Ready ingredient quality/mission identity rules remain distinct.

### 6.1.1 Recommended ordered stat calculation

Inputs:

- `L`: recipe mastery level 1-10 for a nonmaterial recipe; absent for a material recipe.
- `C`: total craft-quality furniture strength expressed as an integer level 0-5, capped at five.
- `U`: total applicable equipment-durability furniture bonus, as a fraction such as 0.20. Zero for materials, food, and misc.
- `Q`: finished product quality, capped at Q5, after recipe-level quality and craft-quality furniture are combined.
- `PriceReferenceAtLevel10`: the item's Q1-equivalent base-price anchor at mastery ten, for nonmaterials. Materials instead use a fixed `MaterialBasePrice` with no mastery.
- `D0`: base maximum durability in uses or distance, only where applicable.

Starting curves: `priceLevelFactor(L) = (L + 35) / 45`, ranging from 0.80 at level one to 1.00 at level ten, and `durabilityLevelMultiplier(L) = 1 + 0.08 * (L - 1)`. The proposed price curve authors level-ten references directly. Apply the finished-quality price multiplier from section 12.2; craft-quality furniture raises price through finished quality, not a separate direct `+5% price per C` layer.

Proposed durability bonus by finished quality:

| Finished quality | Bonus fraction `qualityDurabilityBonus(Q)` |
| --- | --- |
| Q1 | 0.00 |
| Q2 | 0.10 |
| Q3 | 0.20 |
| Q4 | 0.30 |
| Q5 | 0.40 |

**Confirmed structure:** level-adjusted base + product-quality bonus + furniture-durability bonus. **Recommended percentage interpretation:** calculate both bonuses from the level-adjusted base and add them; do not compound the furniture percentage on top of the quality bonus. These percentages remain test data.

Calculate in this order:

1. Set the baseline quality score to `min(L, 5)` for nonmaterials or `1` for materials.
2. Set finished/displayed quality `Q = clamp(baselineQualityScore + C, 1, 5)`. Q5 is the maximum even when mastery and furniture together exceed five. Material output quality has no mastery/input-quality contribution.
3. Compute Q1-equivalent base unit price `Pbase = PriceReferenceAtLevel10 * priceLevelFactor(L)` for nonmaterials. Materials use `Pbase = MaterialBasePrice`, without a mastery factor.
4. Compute level-adjusted base durability `Dbase = D0 * durabilityLevelMultiplier(L)` for tools/wearables only.
5. Add the finished-product-quality contribution: `DqualityBonus = Dbase * qualityDurabilityBonus(Q)`. This applies to recipe-derived quality as well as furniture-assisted quality; it is not conditional on `C > 0`.
6. Add the matching furniture contribution: `DfurnitureBonus = Dbase * U`; final durability is `Dfinal = Dbase + DqualityBonus + DfurnitureBonus`. Do not also multiply durability directly by `C` or reapply the Q bonus.
7. Apply the finished-quality price multiplier once: `Pquality = Pbase * qualityPriceMultiplier(Q)`. For finite crafted tools/wearables/backpacks, set `durabilityPriceMultiplier = Dfinal / Dbase = 1 + qualityDurabilityBonus(Q) + U` and `Pintrinsic = Pquality * durabilityPriceMultiplier`. For items without durability, `Pintrinsic = Pquality`. Use the full calculation/rules in section 12.2; no direct C price bonus and no second multiplication by U.
8. Calculate the timer independently from tier/category/mastery/batch size as section 10 specifies. Neither `C` nor `U` changes it.
9. Snapshot the resulting grade, intrinsic unit value, base durability, both durability bonuses, final maximum durability, level, and modifier profile at craft start. Use high-precision/fixed-point intermediates and round money/uses once under a documented rule.

The compact durability formula is `Dfinal = D0 * durabilityLevelMultiplier(L) * (1 + qualityDurabilityBonus(Q) + U)`. There is exactly one product-quality bonus and one matching furniture-durability bonus. The quality label can remain Q5 while additional recipe levels improve the base and therefore the final durability. Craft-quality furniture has no additional durability effect once Q5 is reached; matching durability furniture can still add its own bonus. These improvements do not create Q6-Q10. Sale-price furniture and location/trade multipliers remain later price modifiers on `Pintrinsic`, not craft quality.

Durability example: a level-six tool with `D0 = 100 uses`, `C = 2`, and `U = 0.20` has Q5; level base 140 + quality 56 + furniture 28 = **224 uses**. Without either furniture bonus, that recipe still makes Q5 and **196 uses**. Calculate its price separately using section 12.2's base/quality/durability layers. A material with fixed Q1 base 10 and `C = 2` has Q3 and, with the proposed 1.35 quality multiplier, intrinsic unit value 13.50 before transaction rounding; no mastery/durability.

For UI, show a readable breakdown such as `140 base + 56 quality + 28 furniture = 224 uses`; wearable durability uses distance in place of uses. Treat finite starting/current durability as the rounded final maximum at creation. Never reset existing gear to a newly calculated maximum when its recipe levels up or furniture changes.

Gathered goods use their source quality tables. **Store purchases are always Q5**; camp conveyor purchases retain the listing's quality instead. Author vendor intrinsic/durability profiles independently of the player's mastery/furniture; buying never levels or unlocks a recipe. Apply the Q5 premium once to vendor baseline stats, or store an already-computed Q5 profile, never both. Finite vendor gear gets the Q5 durability bonus on its authored base, without the player's craft-only furniture bonus. Special infinite-durability merchant gear uses its authored price/flag. Being Q5 does not itself make equipment indestructible or raise its capability tier.

### 6.2 Twenty-four regions, four active resource tiers

**Confirmed scope:** 24 regions share four wood/animal tiers. **Recommended distribution:** six consecutive regions per tier. Resource tier is distinct from region number, equipment variant, recipe mastery, and house level. Species rankings are game progression, not claims about real-world strength/value.

| Resource tier | Regions | Wood / wild animal | Fish band / effective tool tier | Materials introduced within the block |
| --- | --- | --- | --- | --- |
| 1 | 1-6 | Pine / rabbit | A / 1 | Apples first, then hemp/flint, nettle/clay/salt, limestone/herbs/fruit, oats, wheat/reeds/sand; dairy from settlement |
| 2 | 7-12 | Birch / beaver | B / 2 | Flax/copper, barley/quartz, sandstone, rice/tomato/honey, late tin/ochre/cattail; refined beaver materials |
| 3 | 13-18 | Mesquite / turkey | C / 3 | Feathers/corn, cotton/iron/beans, stone/crops, turquoise/soy, sugar/chili/oilseed, silver/mint/agave; bronze uses earlier tin |
| 4 | 19-24 | Oak / boar | D / 4 | Steel, gold ore, marble/basalt, select gems, fine active-tier cloth/leather, advanced food/containers |

Introduce each block's resources gradually across six regions rather than delivering its whole catalog at the first camp. Access older loose materials by revisiting their regions or discovered shops; later harvesting may supply only explicitly allowed interaction outputs/overlaps, not a fourth loose gathering slot. Recipes/local abundance provide progression without another wood/animal tier every region.

**Confirmed naming direction:** region/camp names describe terrain, climate and local landmarks, independently of resource tier. The following 24 names are the proposed localized catalog:

| Tier | Region/name pairs |
| --- | --- |
| 1 | 1 Dawn Plains; 2 Windward Hills; 3 Mistwood; 4 Fern Hollow; 5 Cloudstep Mountains; 6 Reedwater Delta |
| 2 | 7 Whispering Marsh; 8 Amber Riverbanks; 9 Bluewater Basin; 10 Sunlit Uplands; 11 Mirror Lakes; 12 Frostveil Taiga |
| 3 | 13 Golden Grasslands; 14 Ember Canyons; 15 Red Dune Desert; 16 Hidden Oasis; 17 Windswept Plateau; 18 Snowmelt Highlands |
| 4 | 19 Sunset Savanna; 20 Stormwatch Cliffs; 21 Crescent Beaches; 22 Rainforest Coast; 23 Silverwater Estuary; 24 Coral Shores |

Settlement remains between regions **4 and 5**, not between resource tiers four and five. Thus the first home must use tier-one pine/materials, not oak or deferred maple. A river/toll still separates individual regions, including adjacent regions in the same resource tier.

Four fish bands still map to tool/resource tiers 1-4. Expand the **proposed full catalog** to four species per tier and use section 6.2.5's six unique pairings per block; do not add two new species for every region. A regional pair is the union of its fishing-node outputs, not a claim that both species share one pond. Preserve separate freshwater/coastal/open-water nodes and the full-stack/Elite drop rules. Fish remain interaction outputs, never a fourth featured loose resource.

Start with one representative segment per tier. The latest design replaces the old three-segment/four-camp region template with 10-14 camps in early regions, growing toward 24 later. The proposed full layout below has 437 camps/413 internal trail segments; treat these as data/content targets, never simultaneously loaded scenes or a prerequisite for mechanics testing. Reuse environment/camp kits, and stream a small number of segments. Mission ordering is the new four-branch prerequisite graph, not a fixed Tool phase.

### 6.2.1 Deferred tiers and content gate

| Pending tier | Reserved wood / wild animal | Status |
| --- | --- | --- |
| 5 | Maple / deer | Backlog only; no region assignment or required recipe |
| 6 | Cedar / mountain goat | Backlog only |
| 7 | Redwood / buffalo | Backlog only |
| 8 | Ebony / bear | Backlog only |

Do not populate these tiers' runtime loot, tools, missions, shops, mystery boxes, pet rewards, or house costs in the current build. Keep their ideas in a design backlog; no expansion art/recipes/classes are needed now. Do not reserve current speed/hearts/pocket/hook caps for those future tiers. Existing IDs in a test save should be migrated or quarantined safely if content is disabled, never silently reinterpreted as another species.

### 6.2.2 Three featured loose resources per region

**Confirmed interpretation:** six regions x three featured resources = **18 distinct loose-gathering item IDs per tier**, including its branch and pelt/hide entry. It is not 18 plus those two. Each region's three-item set is different, with no repeated featured loose item within its tier. The draft below also uses 72 distinct IDs across all four tiers. Existing recipes must use these exact named IDs, not generic interchangeable materials.

Every region separately contains its current-tier tree, passive animal, and suitable fishing interactables. Logs, raw meat, fish, fat, bones, eggs/sap and other interaction-only rewards do not occupy a featured loose slot. Only branch + pelt/hide overlap the two source lists; **tier three's second overlap is turkey feather**, explicitly confirmed instead of a nonexistent turkey pelt. Boar hide is tier four's pelt-family equivalent. Other featured fruit/honey/etc. must not also be placed in tree/animal loot tables. Keep tree bonus outputs distinct from its tier's loose list.

| Region | Tier | Three featured loose item IDs, shown by display name | Intended recipe uses | Proposed camps |
| --- | --- | --- | --- | --- |
| 1 Dawn Plains | 1 | Pine branches; Rabbit pelt; Apples | Pine logs/tools; slingshot/toy; apple juice | 10 |
| 2 Windward Hills | 1 | Hemp fiber; Flint; Pine cones | Hemp cloth; pine axe; pine-cone ornament | 10 |
| 3 Mistwood | 1 | Nettle fiber; Clay; Rock salt | Thread/stuffing; bricks/cups; curing/food | 11 |
| 4 Fern Hollow | 1 | Limestone; Strawberries; Tea herbs | Tiles; yogurt drink; tea | 12 |
| 5 Cloudstep Mountains | 1 | Oats; Pebbles; Red berries | Oat cakes; polished-pebble charm; berry cakes | 13 |
| 6 Reedwater Delta | 1 | Reeds; Wheat; Sand | Reed braid; wheat flour; upgrade mortar | 14 |
| 7 Whispering Marsh | 2 | Birch branches; Beaver pelt; Blackberries | Birch logs; beaver leather/toy; blackberry juice | 12 |
| 8 Amber Riverbanks | 2 | Flax fiber; Copper ore; Carrots | Flax cloth; copper ingot/fittings; carrot soup | 13 |
| 9 Bluewater Basin | 2 | Barley; Quartz; Blueberries | Barley flour/soup; glass/cut quartz; blueberry jam | 14 |
| 10 Sunlit Uplands | 2 | Sandstone; Truffle; Flax seeds | Tiles; truffle meals; flax oil | 14 |
| 11 Mirror Lakes | 2 | Rice; Tomatoes; Honey | Fish bowls; stew; honey drinks | 18 |
| 12 Frostveil Taiga | 2 | Tin ore; Yellow ochre; Cattail fiber | Bronze; painted birch carving; cattail cord/fishing charm | 20 |
| 13 Golden Grasslands | 3 | Mesquite branches; Turkey feathers; Corn | Mesquite logs; feather garment/toy; corn flour | 20 |
| 14 Ember Canyons | 3 | Cotton fiber; Iron ore; Beans | Cotton cloth; iron tools; stew/chili | 20 |
| 15 Red Dune Desert | 3 | Granite; Chickpeas; Pumpkins | Tiles; chickpea stew; porridge | 22 |
| 16 Hidden Oasis | 3 | Slate; Turquoise; Soybeans | Tiles; turquoise jewelry; soy milk | 22 |
| 17 Windswept Plateau | 3 | Sugar cane; Green chili; Sunflower seeds | Sugar; turkey chili; sunflower oil | 24 |
| 18 Snowmelt Highlands | 3 | Silver ore; Wild mint; Agave fiber | Silver jewelry; mint tea; agave cord/desert charm | 24 |
| 19 Sunset Savanna | 4 | Oak branches; Boar hide; Potatoes | Oak logs; boar leather; stew | 24 |
| 20 Stormwatch Cliffs | 4 | Marble; Gold ore; Cacao beans | Tiles; gold jewelry; cacao powder/drinks | 24 |
| 21 Crescent Beaches | 4 | Basalt; Emerald; Coffee beans | Polished stone; emerald jewelry; coffee | 24 |
| 22 Rainforest Coast | 4 | Obsidian; Ruby; Vanilla pods | Obsidian pendant; ruby jewelry; vanilla drink | 24 |
| 23 Silverwater Estuary | 4 | Sapphire; Lapis lazuli; Seaweed | Sapphire pendant; lapis bracelet; seaweed rice parcel | 24 |
| 24 Coral Shores | 4 | Diamond; Coconuts; Bamboo fiber | Diamond necklace; coconut drink; bamboo cloth/coastal shirt | 24 |

The featured resources are the normal loose spawn table for that region; their weights can differ between segments. Older materials can remain available through revisiting regions, camps' offered items, discovered settlement stock, and the two permitted interaction overlaps. Do not casually add a fourth loose type or repeat old featured IDs in later maps. Map cards show exactly the three resource icons in this table, separately from tree/animal/fishing indicators, plus locked/discovered state. A fishing point can use its local tier's valid species without making fish a fourth featured resource.

The names are an authored proposal, not all new production assets to build immediately. Each item has a useful seed use; preserve that requirement before finalizing a slot. If a material is removed, update the region icons, missions, loot tables, shop filters, and exact recipes together rather than leaving a decorative/useless collectible.

**Truffle source proposal:** `ingredient.truffle` (display Truffle), material tier two, is a stackable raw cooking ingredient, not a ready consumable. It is one of region 10's three normal loose resources, with a proposed 15% loose spawn weight (Sandstone 45%, Flax seeds 40%). No tool, pig, new digging mechanic, or paid ingredient is required. Collect one per object in Release and 1,000 in MechanicsTest. Eligible pigs and other pets can supply it only after region 10 is discovered. The same exact ID is used everywhere; never substitute an ordinary mushroom.

To preserve the existing three-per-region/18-per-tier/72-total constraint, this is a **proposed source reassignment**: Lavender leaves region 10's loose table and becomes a Q5 cash farm-catalog herb unlocked by discovering region 10. Keep its exact existing ID/material tier and the Lavender pouch recipe; its source label and all future optional requests must point to that farm catalog. Lavender does not enter pet loose-material tables. Source validators and region-10 map icons replace Lavender with Truffle together. Truffle remains a main regional ingredient, not a global pantry exception; its recipes use unlock tier two or three.

### 6.2.3 Source tags and small recipe additions

Use separate `LooseGathering`, `TreeHarvest`, `AnimalHarvest`, `Fishing`, `CraftOnly`, and `Store` sources. The same item ID can have more than one source only under explicit rules; branch + pelt/hide/confirmed feather are the loose/interaction overlaps. A crafted item can be bought without becoming loose gatherable. Pine logs are TreeHarvest and craftable, so conversion is instant; apples are LooseGathering, not a tree-chop reward. Rock salt is the exact Salt ID already used by recipes. Flint stones means the Flint ID, and Rabbit pelt/raw Rabbit pelt refer to the same unprocessed `animal.rabbit.pelt`, not alternative ingredient IDs. Normalize these display descriptions consistently when authoring. Generic Berry becomes the exact Red berries ID; generic Herb becomes Tea herbs where specified. These naming clarifications never permit substitution between different varieties.

Useful **fixed-input recipe seeds**, to unlock through the four branches (not all available from the start):

| Output | Exact inputs -> yield | Rule/use |
| --- | --- | --- |
| Apple juice | 3 Apples -> 1 | Early Food recipe, proposed 3-second level-one timer, restores 1 heart |
| Pine-cone ornament | 2 Pine cones + 1 Pine branch -> 1 | Stackable Misc; gives pine cones a sale use |
| Reed braid | 3 Reeds -> 1 | Material; house-upgrade input |
| Mortar | 2 Sand + 1 Limestone -> 1 | Material; house-upgrade input, no furniture crafting |
| Blackberry juice | 3 Blackberries -> 1 | Separate Food recipe/output/mastery from apple juice |
| Flax cloth | 4 Flax fiber -> 2 | Separate fixed Material recipe |
| Carrot soup | 3 Carrots + 1 Rock salt -> 1 | Food |
| Blueberry jam | 3 Blueberries -> 1 | Food |
| Lavender pouch | 2 Lavender + 1 Hemp cloth -> 1 | Misc |
| Flax oil | 3 Flax seeds -> 1 | Ingredient Material, fixed timer |
| Painted birch bird | 2 Birch planks + 1 Yellow ochre -> 1 | Misc |
| Cattail cord | 3 Cattail fiber -> 1 | Material |
| Fishing charm | 1 Cattail cord + 1 Copper fitting -> 1 | Stackable Misc; cattail has a sink outside Elite upgrades |
| Chickpea stew | 3 Chickpeas + 1 Rock salt -> 1 | Food |
| Turkey chili | 2 Raw turkey meat + 2 Beans + 1 Green chili -> 1 | Food |
| Sunflower oil | 3 Sunflower seeds -> 1 | Different Material/output from flax oil; no substitution |
| Mint tea | 2 Wild mint + 1 Water -> 1 | Food |
| Agave cord | 3 Agave fiber -> 1 | Material |
| Desert charm | 1 Agave cord + 1 Turquoise -> 1 | Stackable Misc; agave has a sink outside Elite upgrades |
| Silver pendant | 1 Silver ingot + 1 Flax cord -> 1 | Tier-three Misc, inputs from tiers three/two |
| Cacao powder | 2 Cacao beans -> 2 | Material |
| Roasted coffee | 2 Coffee beans + 1 Oak branch -> 2 | Tier-four Material ingredient; explicit roasting fuel |
| Cut emerald/ruby/sapphire/diamond | Each gets its own fixed recipe: 2 named raw gems -> 1 same named cut gem | Four separate Material definitions; no wildcard runtime recipe |
| Emerald ring | 1 Cut emerald + 1 Gold ingot -> 1 | Misc |
| Ruby ring | 1 Cut ruby + 1 Gold ingot -> 1 | Misc |
| Sapphire pendant | 1 Cut sapphire + 1 Silver ingot -> 1 | Misc |
| Lapis bracelet | 2 Lapis lazuli + 1 Cotton cord -> 1 | Tier-four Misc using tier-three cord |
| Obsidian pendant | 2 Obsidian + 1 Cotton cord -> 1 | Tier-four Misc using tier-three cord |
| Seaweed rice parcel | 2 Seaweed + 2 Rice + 1 Tuna -> 1 | Food |
| Diamond necklace | 1 Cut diamond + 1 Gold ingot + 1 Cotton cord -> 1 | Tier-four Misc using only tiers four/three |
| Coconut drink | 2 Coconuts + 1 Glass bottle -> 1 | Food |
| Bamboo cloth | 4 Bamboo fiber -> 2 | Material |
| Coastal shirt | 4 Bamboo cloth + 2 Fine boar leather -> 1 | Tier-four optional second-set Wearable, max ten pockets |

Flax oil and sunflower oil have distinct sinks: seed oil biscuits use 2 Wheat flour + 1 Flax oil, while chili oat cakes use 2 Oats + 1 Sunflower oil + 1 Green chili. Never use an interchangeable Oil input. The draft house table uses four Reed braids for level two and six Mortar for level three; their region-six material unlocks are proposed later missions below. Counts remain test data; materials are unlevelled and furniture store-only.

### 6.2.4 Camp spacing and the 26-heart ceiling

Count camps including a region's entry/exit camps; internal walking legs are `campCount - 1`. Ferry transitions do not add walking cost. The proposed counts above are 10-14 in early regions, 18-20 near the end of tier two, and 20-24 in later tiers. Build only short representative test chains initially.

For each leg, author world distance `D` and a fixed `drainHeartsPerMeter`, then compute `requiredHearts = D * drainHeartsPerMeter`. This budget is independent of camera input, time spent stationary, carried quantities, current equipment, or current stamina. Hats raise the meter cap, boots reduce elapsed walking time, and food adds usable stamina during the leg; none silently changes its authored walking cost.

**Confirmed maximum: highest-tier requiredHearts <= 26 per leg.** Keep the cap at twelve hearts. At a full twelve-heart start, a 26-heart leg requires at least fourteen additional hearts from food/drinks. Three ordinary six-heart servings supply eighteen, permitting a four-heart finish if consumed without waste; premium gold food is not required. If starting below full stamina, the food budget must increase accordingly.

Proposed pacing data (ranges are examples, not implemented level measurements):

| Progression | Typical leg distance | Authored walking cost | Intent |
| --- | --- | --- | --- |
| Region 1 opening camps | 100-250 m | 0.2-0.5 heart at 2 hearts/km | Safe with three starting hearts and no food |
| Rest of region 1 | 150-400 m | At most 1.5 hearts | Obtain apples, finish Gatherer G1/G2, craft juice before region 2 |
| Regions 2-6 | 350-600 m | 3.5-6 hearts | Increasing distance, first mandatory food planning |
| Regions 7-12 | 500-900 m | 6.5-12 hearts | More food and stronger hat access |
| Regions 13-18 | 750-1,200 m | 10-20 hearts | Multiple meals on longer legs |
| Regions 19-24 | 1,000-1,600 m | 14-26 hearts, never above 26 | Food during travel remains necessary even with twelve hearts |

Derive each leg's drain rate from its chosen distance and budget; never combine the maximum distance/rate blindly. Example highest-tier leg: 1,300 m at 20 hearts/km = **26 hearts**. At 1.6 m/s it is about 13 minutes 33 seconds of walking before harvesting stops. Keeping a universal 2 hearts/km would require 13 km/about 2 hours 15 minutes for the same cost; variable authored regional drain rates are therefore a **recommendation** for mobile pacing, while the 26-heart cap is confirmed.

Provide a departure preview with required hearts, current/max hearts, and **usable** ready food recovery, including retained pending food hearts and simulating recovery timing against walking drain; excess at the meter cap remains pending rather than being wasted. Warn if under-provisioned; do not invisibly lock departure or auto-buy food. Guarantee first-region apples before the recipe handoff, and G1/G2 camp claims before exit. Camp regeneration must permit a safe next opening leg even if trading/menus are skipped. Simulate starter legs against three hearts and advanced legs against 26, including partial starts and six-heart food scheduling without waste. Maximum hat capacity remains twelve even while cumulative per-leg consumption reaches 26.

### 6.2.5 Regional fish pairings without repeating a pair inside a tier

**Recommended distribution:** four fish species per tier give exactly six unordered pairs (`4 choose 2`), one for each region. Retain the original eight and add eight, rather than authoring 48 unique fish. Species additions/pairings below are design proposals; four tool tiers, fishing everywhere, exact recipes, and three loose-resource icons per region remain fixed.

| Fish tier | Catalog, with new species marked | Fishing tool tier |
| --- | --- | --- |
| 1 | Rainbow trout; Largemouth bass; **Bluegill (new)**; **Yellow perch (new)** | 1 |
| 2 | Common carp; Northern pike; **Channel catfish (new)**; **Walleye (new)** | 2 |
| 3 | Salmon; Sturgeon; **Brown trout (new)**; **Lake trout (new)** | 3 |
| 4 | Longnose gar; Tuna; **Red drum (new)**; **Red snapper (new)** | 4 |

| Region | Exact fish pair | Authored node presentation |
| --- | --- | --- |
| 1 Dawn Plains | Rainbow trout + Bluegill | Stream/pool nodes |
| 2 Windward Hills | Largemouth bass + Yellow perch | Lake/pond nodes |
| 3 Mistwood | Rainbow trout + Largemouth bass | Separate stream/lake nodes |
| 4 Fern Hollow | Bluegill + Yellow perch | Pond/lake nodes |
| 5 Cloudstep Mountains | Rainbow trout + Yellow perch | Stream/lake nodes |
| 6 Reedwater Delta | Largemouth bass + Bluegill | Backwater/pool nodes |
| 7 Whispering Marsh | Common carp + Channel catfish | Slow-water nodes |
| 8 Amber Riverbanks | Northern pike + Walleye | River/lake nodes |
| 9 Bluewater Basin | Common carp + Northern pike | Wetland/lake nodes |
| 10 Sunlit Uplands | Channel catfish + Walleye | Reservoir/river nodes |
| 11 Mirror Lakes | Common carp + Walleye | Lake nodes |
| 12 Frostveil Taiga | Northern pike + Channel catfish | Separate lake/river nodes |
| 13 Golden Grasslands | Salmon + Brown trout | Authored cool-water river reach |
| 14 Ember Canyons | Sturgeon + Lake trout | River/deep-reservoir nodes |
| 15 Red Dune Desert | Salmon + Sturgeon | River reach, not a desert puddle |
| 16 Hidden Oasis | Brown trout + Lake trout | Cool/deep lake nodes |
| 17 Windswept Plateau | Salmon + Lake trout | Stream/reservoir nodes |
| 18 Snowmelt Highlands | Sturgeon + Brown trout | Separate river/tributary nodes |
| 19 Sunset Savanna | Longnose gar + Red drum | Separate freshwater/estuary access |
| 20 Stormwatch Cliffs | Longnose gar + Red snapper | Inland water plus an authored lower-coast fishing stop |
| 21 Crescent Beaches | Red drum + Red snapper | Estuary/coastal fishing access |
| 22 Rainforest Coast | Longnose gar + Tuna | Separate freshwater and open-coast pier |
| 23 Silverwater Estuary | Red drum + Tuna | Estuary plus open-water presentation |
| 24 Coral Shores | Red snapper + Tuna | Coastal/open-water fishing stops |

These are game habitat/route authoring targets, not biological cohabitation guarantees. In particular, Red drum is associated with coastal/estuarine waters and Red snapper with marine habitat; do not put the pair into an inland pond. Tier-four route art must include the listed coast access, even when the wider region is forest/highland. Keep deeper/offshore fish as pier/cast-point presentation rather than a new player-boat system. Habitat references: [NC DEQ Red drum](https://www.deq.nc.gov/about/divisions/marine-fisheries/public-information-and-education/species-profiles/red-drum), [NOAA Red snapper](https://www.fisheries.noaa.gov/species/red-snapper). Cool-water/river presentations for tier three are authored tributaries/reservoirs, not a claim that the whole mesquite biome has cold water.

Start with a few fish/placeholder views; every species referenced by an enabled regional pair still needs a valid item definition before that pair activates. Add art/remaining regions incrementally. Every fish has an exact ID/named roast in section 9.4.1, never interchangeable Fish input. Regional weights/nodes may differ, but both species need attainable sources before a local required mission asks for them. The fishery offers discovered species only, always Q5. Keep three loose icons and show the pair in a separate fishing-detail panel.

### 6.2.6 Landscape themes and camp names

**Confirmed:** names no longer repeat Pine/Birch/Mesquite/Oak to signal tiers. Keep resource tier, source tables, fish pairs, camp counts and progression separate from the display name. Snow/desert/coast are visual map themes, not new temperature/thirst hazards or resource tiers. Preserve appropriate water nodes and sheltered growing areas for existing fish/crops; region 18 may combine snowy peaks with warm foothill gardens, and region 20 retains inland-water/coastal access.

| Region | Map theme | Camps 1, 2 and final camp (examples from the full naming rule) |
| --- | --- | --- |
| 1 Dawn Plains | Open temperate plains | Dawn Meadow; Quiet Meadow; Low Meadow |
| 2 Windward Hills | Rolling windy hills | Dawn Hilltop; Quiet Hilltop; Low Hilltop |
| 3 Mistwood | Misty temperate forest | Dawn Woodland; Quiet Woodland; Distant Woodland |
| 4 Fern Hollow | Green valley and fern slopes | Dawn Hollow; Quiet Hollow; Bright Hollow |
| 5 Cloudstep Mountains | Rocky mountain passes | Dawn Ridge; Quiet Ridge; Silent Ridge |
| 6 Reedwater Delta | River delta and reed islands | Dawn Channel; Quiet Channel; Shaded Channel |
| 7 Whispering Marsh | Wetlands and boardwalks | Dawn Marsh; Quiet Marsh; Bright Marsh |
| 8 Amber Riverbanks | Warm river terraces | Dawn Riverbank; Quiet Riverbank; Silent Riverbank |
| 9 Bluewater Basin | Lowland basin and ponds | Dawn Basin; Quiet Basin; Shaded Basin |
| 10 Sunlit Uplands | Sunny uplands and wooded hollows | Dawn Knoll; Quiet Knoll; Shaded Knoll |
| 11 Mirror Lakes | Linked lakes and islands | Dawn Lakeshore; Quiet Lakeshore; Southern Lakeshore |
| 12 Frostveil Taiga | Snowy taiga and thawed river edges | Dawn Taiga; Quiet Taiga; Rainwashed Taiga |
| 13 Golden Grasslands | Dry golden grasslands | Dawn Grassland; Quiet Grassland; Rainwashed Grassland |
| 14 Ember Canyons | Red canyons and deep river reaches | Dawn Canyon; Quiet Canyon; Rainwashed Canyon |
| 15 Red Dune Desert | Dunes and desert river corridor | Dawn Dune; Quiet Dune; Winding Dune |
| 16 Hidden Oasis | Desert springs and cool deep lakes | Dawn Spring; Quiet Spring; Winding Spring |
| 17 Windswept Plateau | Dry plateau and reservoir | Dawn Mesa; Quiet Mesa; Twilight Mesa |
| 18 Snowmelt Highlands | Snowy peaks with thawed foothills | Dawn Snowfield; Quiet Snowfield; Twilight Snowfield |
| 19 Sunset Savanna | Savanna and estuary fringe | Dawn Waterhole; Quiet Waterhole; Twilight Waterhole |
| 20 Stormwatch Cliffs | Coastal cliffs and inland streams | Dawn Cliffside; Quiet Cliffside; Twilight Cliffside |
| 21 Crescent Beaches | Sandy coves and tidal beaches | Dawn Cove; Quiet Cove; Twilight Cove |
| 22 Rainforest Coast | Rainforest with open-coast pier | Dawn Rainforest; Quiet Rainforest; Twilight Rainforest |
| 23 Silverwater Estuary | Estuary channels and offshore outlook | Dawn Estuary; Quiet Estuary; Twilight Estuary |
| 24 Coral Shores | Tropical beaches and coral viewpoints | Dawn Shore; Quiet Shore; Twilight Shore |

**Complete camp naming rule:** each camp uses its region's landmark from the table and the following ordered descriptor list, taking only the first `campCount` descriptors: Dawn, Quiet, Windy, Sunlit, Hidden, Old, Moonlit, Sheltered, High, Low, Distant, Bright, Silent, Shaded, Eastern, Western, Northern, Southern, Starlit, Rainwashed, Lonely, Winding, Last, Twilight. Compose `[descriptor] [regional landmark]`; examples above show camps 1, 2 and the final ordinal. The regional landmarks in order are Meadow, Hilltop, Woodland, Hollow, Ridge, Channel, Marsh, Riverbank, Basin, Knoll, Lakeshore, Taiga, Grassland, Canyon, Dune, Spring, Mesa, Snowfield, Waterhole, Cliffside, Cove, Rainforest, Estuary, Shore. Thus every existing camp (10-24 per region) receives a fixed name with local variations, not a tier prefix or a random name on each visit. Show camp ordinal as a secondary map label for navigation. Localization authors can polish grammar while keeping each stable CampId.

Names are localized display fields only. Preserve existing region/camp IDs, discovery, tolls, lastTrailCampId, save positions and source references. Even a legacy ID such as `region.pine_meadows` remains stable while its display becomes Dawn Plains. Never parse names for tier/biome/resource logic. Save content may migrate display keys/aliases, never reset unlocks. Map headers, both regional tables, camp markers and travel previews use the same catalog.

### 6.3 Resource catalog and acquisition

| Family | Materials to retain/add | Source and progression purpose |
| --- | --- | --- |
| Wood | Active: pine, birch, mesquite, oak, each with branches/logs/planks | Trail trees, four lumber tiers; tools, carvings, house upgrades. Maple/cedar/redwood/ebony are deferred |
| Fiber | Nettle, hemp, cotton, reeds; add flax and thread | Early gathering, selected farm stock; cordage, cloth, toys |
| Animal textiles | Active: rabbit/beaver pelts, boar leather, optional ranch wool; hemp/cotton/fine cloth | Defer deer/goat/buffalo hides, bear fur, silk/cashmere chains unless separately approved as current imports |
| Meat | Wild: rabbit, beaver, turkey, boar. Domestic ranch meat can use a small generic selection initially | Venison, hunted mountain goat, buffalo, and bear recipes are deferred; domestic goat milk need not imply a mountain-goat hunt |
| Byproducts | Active: turkey feathers, eggs, boar tusk, bones, animal fat | Defer deer antler, buffalo/goat hunting horns, bear claw until their wild tiers exist |
| Dairy/apiary | Cow/goat milk, cream, butter, yogurt, cheese, beeswax, honey | Milk from ranch; dairy processing; bee-associated gathering nodes/honey trees where appropriate |
| Crops | Candidate pool: wheat, corn, potatoes, rice, barley, oats, strawberries, tomatoes, beans, soybeans, pumpkins, sugar cane, coffee, cacao, vanilla | Start with a few crops supporting implemented recipes. Add selected late imports within tier four; this is not a requirement to author all crops now |
| Food additions | Berries, mushrooms, apples, nuts, salt, tea herbs, water, exact tier-two Truffle | Truffle gathers in region 10 and feeds three dishes (section 9.4.3); no mandatory yeast/culture/enzyme catalog; water is refillable at camp |
| Metals | Copper, iron, silver, gold ore; add tin and carbon/charcoal | Trail loose ore initially, mine shop later; bronze/steel and jewelry |
| Stone/earth | Limestone, sandstone, granite, marble, basalt, slate, clay, obsidian, quartz | Carving, tiles, ceramics, glass; deduplicate the repeated basalt entry |
| Gems | Diamond, emerald, ruby, sapphire, turquoise, lapis lazuli | Late jewelry; no ordinary tool power gated behind diamond |
| Processing aids | Charcoal, cord/thread, beeswax, glass, nails/fittings | Keep a small set with several uses; omit tannin/culture/glue subchains initially. Glass/clay containers can support advanced food/drink recipes |

Normalize names/IDs and keep the deferred bear-claw concept as one backlog entry. Treat rabbit pelt as raw input and rabbit fur/felt as processed outputs. Keep species-specific leather IDs only when useful to recipes. Gold ore/ingot remain materials, never wallet currency. Avoid duplicate pelt/fur rewards from one body unless deliberately configured.

Keep the original resource ideas as a backlog, not a requirement to implement every crop/gem/import immediately. Author a small region-one subset first, then only resources needed by the four active tiers. Add each material with a source and useful recipe/house sink. Materials may become ingredients, containers, or house upgrades; they never become player-crafted furniture. Higher region numbers within one tier can add recipes and scarcity patterns without adding another species or processing subchain.

## 7. Full-stack harvesting and dropped-item limits

**Confirmed post-testing collection mode:** keep `MechanicsTest` at 1,000 units per stackable trail pickup now. After mechanics/limits testing, `Release` transfers **one item per gathered world object pulled into inventory**, including the quick-storage gesture. Never multiply that one by 1,000 or convert a whole visible pile with one gesture. Node drop count and Elite drop boosts determine how many collectible objects are produced, not units per collected object; for large manifests use a persistent counter spawning bounded visible objects. Pending node counters decrement by one per committed release pickup. Stack capacity stays 1,000. This changes gathering, not recipe yield, shop/box stack rewards, mission quantities or existing save stacks. Persist the mode; switching modes neither rescales inventory nor rerolls committed node manifests. After testing, re-evaluate proposed recipe quantities/mastery thresholds, tool replacement costs, mission ten/50/100/150 requirements and route food availability using one-item pickups. Preserve confirmed caps and exact recipes; tune only delegated/draft costs, density and pacing. Add a fresh-save release playthrough, not merely the full-stack limits test.

### 7.1 Bundle count is separate from unit count

**Confirmed default:** `WorldPickupStackQuantity = 1000` for every stackable trail pickup, including resources dropped by harvesting. Keep this in the main mechanics-test configuration, not behind a debug toggle that is off by default.

| Event | Number of bundles | Units in each bundle | Result |
| --- | --- | --- | --- |
| Loose resource pickup | 1 | 1,000 | One full stack, subject to mission diversion and merge capacity |
| Normal tree branches | Random 2-5 | 1,000 | 2,000-5,000 branch units |
| Normal tree logs | Random 2-5 | 1,000 | 2,000-5,000 log units |
| Tree bonus ingredients | At most two configured types; 2-4 bundles/type | 1,000 | Species/biome-specific food or sap |
| Elite-tool tree wood | 4-10 bundles for each wood output | 1,000 | Full-stack rewards remain unchanged |
| Elite-tool tree ingredients | 4-6 bundles/type | 1,000 | At most two bonus ingredient types |
| Normal hunt | 2-5 bundles for each selected produce output; fat 1-3 | 1,000 | Use species-specific loot tables, not every possible byproduct |
| Elite-tool hunt | 4-10 produce bundles/output; fat 1-6 | 1,000 | High-load case for spatial inventory and loot queues |
| Fishing completion | 2-5 bundles drawn from local fish table | 1,000 | Each bundle contains one fish species and one quality |
| Elite-tool fishing | Proposed 4-10 bundles from the same local fish table | 1,000 | Boost on all eligible targets; fewer actions at/below tool tier, species restrictions unchanged |
| Massive tree | Same bundle-generation rules, multiplied by 50 | 1,000 | Queue rewards; never render them all at once |
| Camp NPC purchase | 1 unit for a stackable offer | 1 | Not a trail pickup; tools/gear are one instance |
| Settlement stack purchase | 1 full stack | 1,000 | Persist overflow in a delivery tray if appropriate |
| Mystery box | 6 typed reward entries | 1,000 for physical stackables | Gear is individual; recipe unlock is one knowledge grant, not a stack |

Example: a normal tree yielding three branch bundles and four log bundles produces 3,000 branches and 4,000 logs. A massive tree with the same underlying result produces 150 branch bundles and 200 log bundles, totaling 350,000 units. This is intentional stress-test behavior, not the final progression economy.

Roll rewards once when harvesting completes. Persist the resulting aggregate manifest and a unique claim identifier before exposing drops. Reloading must not reroll quality, multiply rewards, or reset a partially harvested node.

### 7.2 Visible drops and persistence

- Start with at most 24 active world-bundle views per loaded area; this is a profiling target, not a guaranteed device limit.
- Represent each view by one pooled object, regardless of quantity.
- Keep the rest in a logical source queue. Spawn the next bundle only when space opens.
- At massive trees, group the visible piles by output type; a pile may advertise hundreds of pending full bundles.
- Keep each of the six trees per lumber tier independent: node state, pending quantities, and respawn timestamp.
- Record persistent massive-tree pending rewards until that tree respawns; then remove its unclaimed rewards as requested, with a visible expiry time.
- **Confirmed:** ordinary trail interactables and gathering-point anchors have fixed authored positions. Returning to the segment's previous/start camp creates a new drop-roll cycle for that segment, not a new random point layout. Preserve the existing cycle through app suspension/reload; merely reopening the app does not refresh drops. See section 7.3.
- When leaving a trail segment, warn if unclaimed drops will be abandoned. Persist the current segment's loot until the transition is committed.

Full-stack testing should exercise both many units and many differently shaped stacks. A million units in one aggregate number is cheap; hundreds of distinct entries, gestures, and UI elements are the real performance challenge.

### 7.3 Fixed trail points and camp-return drop cycles

**Confirmed layout rule:** author stable IDs and positions for trail interactables (trees, animal encounter areas, fishing points) and loose gathering points. Neither their placement nor their authored type is randomized on replay. Animals may still move inside their fixed encounter area under the existing passive-hunting rule; this does not move the encounter anchor. Dropped-item views use the point's authored local offsets/pile area, not newly randomized points along the route.

**Confirmed refresh trigger:** for segment `Camp A -> Camp B`, **Camp A is its previous/start camp**. First entry initializes one drop cycle. Every actual later return to Camp A, including unlocked map travel back from Camp B, rolls a new cycle for that outgoing segment. Persist `SegmentId`, `StartCampId`, `DropCycleId`, the return/arrival transaction ID, and a cycle seed. One committed return increments the cycle once; loading/rebinding the same arrival cannot roll it twice. Re-entering the trail from that camp uses the prepared cycle, without a second roll. Returning to another camp refreshes that camp's outgoing segment, not every segment/region. Selecting the current camp, opening maps/inventory/shops, balloon rerolls, rotating the camera, fainting in place, or reopening/resuming the app are not camp returns and do not reroll trail drops.

For each fixed loose gathering point, use the cycle seed and stable point ID to roll its resource identity and quality from the segment's eligible regional loose table, plus any explicitly authored drop-count rules. Keep exactly the region's three possible loose resource IDs; random results may repeat between cycles. Save the generated per-point manifest, collected quantities and claim IDs before exposing pickups. A fresh cycle replaces the prior cycle's uncollected ordinary trail drops; show the existing abandonment warning before committing travel. Already collected inventory, mission contributions, crafted products and delivery-tray rewards remain owned and never reroll. Test mode still gives 1,000 units per picked object and Release one; this revision changes refresh timing, not those quantities.

Tree/hunt/fishing outputs still follow their own exact source tables and tool/drop rules; do not turn a fixed tree into a random different interactable. The harvest-completion rule in section 7.1 remains: commit the harvest reward manifest before exposing its drops, with no completion/reload reroll within that cycle. Use the stable point/cycle context for random state. Preserve partial harvest progress across tool replacement/reload within the same cycle. Settlement massive trees retain their independent 72-hour respawns and pending-reward expiry; pet idle rewards and store refreshes keep their separate triggers.

**Implementation/acceptance:** separate `GatheringPointDefinition` (fixed ID/position/type/table) from `SegmentDropCycleState` (seed, manifests, progress, collected claims). Add camp-return cycle commits to travel persistence and the harvesting milestone. Verify A -> segment -> B -> A changes the cycle once while all anchors/types stay identical; the new random result need not differ. Reopening/reloading at A preserves the prepared cycle; restarting mid-segment preserves already rolled/collected state. Verify unrelated-camp returns, current-camp no-op, interrupted arrival, three-resource source filters, partial collection, abandonment, and both pickup presets without duplicate rewards or lost owned items.

## 8. Tools, harvesting difficulty, and durability

### 8.1 Separate the dimensions

- **Family:** axe, ranged hunting tool, fishing rod.
- **Active resource rank:** 1-4 for wood/animals; fish bands A-D map to effective ranks 1/2/3/4.
- **Tool tier/rank:** its intended matching resource tier; it may also work up to two tiers above it.
- **Variant:** Normal only at tier one; Normal and Elite at each tier from two onward.
- **Quality:** Q1-Q5 price/durability, independent of rank.

For four active tool tiers this gives **7 definitions per family**, 21 across three families: one at tier one and Normal/Elite pairs at tiers two through four. All use the same harvesting service. Future tiers five through eight are not authored now. Start with tier-one Normal tools and one tier-two pair; no tier-one Elite recipe.

### 8.2 Recommended effectiveness matrix

Let `difference = resourceRank - toolRank`.

| Target tier - tool tier | Normal axe | Elite axe | Normal hunting hits | Elite hunting hits | Normal fishing | Elite fishing | Eligibility |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0, matching tier | 6 | **3** | 3 | **1** | 3 | **1** | Allowed |
| -1 or less, lower-tier target | 3 | **2** | 1 | **1** | 1 | **1** | Allowed; one-action floor |
| +1, target one tier higher | 12 | 12 | 6 | 6 | 6 | 6 | Allowed |
| +2, target two tiers higher | 24 | 24 | 12 | 12 | 12 | 12 | Allowed |
| +3 or more | None | None | None | None | None | None | No interaction without a different eligible tool |

The numeric counts are **proposed test values**. **Confirmed:** Elite reduces required uses/actions against targets at or below its own tier, not only lower tiers. Where Normal already takes one action, Elite remains at the one-action minimum. The reduction is not a hidden tool-tier increase. Against targets above tool tier, keep the Normal counts for now, while Elite still boosts drops. The +2 ceiling remains identical for both variants.

Elite selects the boosted loot table for **every successful eligible harvest**, whether target tier is lower, equal, +1, or +2. An over-tier Normal tool never gains the boost. Apply it to valid wood/bonus-ingredient/hunting/fishing outputs once; no out-of-range reward or boost on a failed/canceled harvest. Each stackable world bundle still contains 1,000 units; boosted tables change bundle count, not stack quantity.

**Confirmed tool baselines (tier T, recipe level one, before quality/furniture):** Normal axe/rod `D0 = 10 + 10 * (T - 1)`; Normal slingshot `D0 = 6 + 10 * (T - 1)`. Elite `D0 = 2 * sameFamilyNormalD0`; no tier-one Elite exists. Apply the existing mastery multiplier, then quality and eligible player-craft furniture bonuses once. At identical mastery/quality/furniture the Elite remains exactly double the Normal durability; its separate action savings/drop boost remain unchanged.

| Tier | Normal axe | Normal slingshot | Normal rod | Elite axe | Elite slingshot | Elite rod |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 10 | 6 | 10 | — | — | — |
| 2 | 20 | 16 | 20 | 40 | 32 | 40 |
| 3 | 30 | 26 | 30 | 60 | 52 | 60 |
| 4 | 40 | 36 | 40 | 80 | 72 | 80 |

Each committed action/miss spends one use. Preserve fractional maximum durability from modifiers internally; allow an action whenever remaining uses are positive, subtract one with a zero floor, then break at zero after that action resolves. Test starter axe one six-action tree plus four remaining uses, slingshot two three-hit rabbits before misses, and rod three three-action catches plus one remaining use. Higher-target attempts may outlast a tool: preserve harvest progress across replacement tools. These baselines supersede the old 120/180 proposal.

Massive trees follow the same eligibility/reduction scope. Proposed counts:

| Target tier - tool tier | Normal axe actions | Elite axe actions |
| --- | --- | --- |
| +2 | 80 | 80 |
| +1 | 40 | 40 |
| 0 | 20 | **10** |
| -1 | 10 | **5** |
| -2 | 5 | **3** |
| -3 or less | 3 | **2** |

Elite boosts massive-tree drops even at +1/+2. Against targets at/below its tier, these proposed Elite counts are `ceil(NormalActions / 2)`, minimum one, while the retained Normal curve floors at three. Apply massive-tree x50 once and the Elite boosted table once. These counts replace the earlier equal-action rule.

Search carried backpack/hook tools for the matching family, positive remaining uses, and eligible tier before starting. Hooks are storage locations, not equipment slots. Recommend showing the selected tool and allowing a Normal/Elite preference when multiple tools qualify. A resource tap with no valid tool does not enter harvesting/aiming, consume uses, spend stamina, or start an animation; an unobtrusive requirement hint is allowed. Revalidate after mission consumption or inventory changes, including between committed actions.

### 8.3 Tool construction progression

#### 8.3.1 Complete pine starter set

All three tier-one recipes are explicit, distinct, **Normal-only x1 crafts**:

| Exact tool output | Fixed recipe inputs -> one output | Unlock/access |
| --- | --- | --- |
| Normal pine axe | 2 Pine branches + 2 Flint -> 1 | Lumberjack L1; Flint arrives in region two |
| Normal pine slingshot | 2 Pine branches + 1 Raw rabbit pelt -> 1 | Hunter H1; both loose inputs available in region one |
| Normal pine rod | 2 Pine branches + 2 Hemp cord + 1 Bone -> 1 | Explorer E5, after E4's cord recipe and hunting bone access |

All ingredients are tier one; Bone is an interaction-only tier-one rabbit output, not another featured loose resource. The rod is a fishing tool, not another axe recipe. The first food unlock remains apple juice, so the rod is not needed to survive region one. These recipes use the same IDs as section 9.3 and the mission table; no pine Elite variant exists. Their confirmed level-one Normal baselines are axe 10, slingshot 6, and rod 10 uses before mastery/quality/eligible furniture (section 8.2).

#### 8.3.2 Later material themes

| Rank | Axe material theme | Hunting material theme | Rod material theme |
| --- | --- | --- | --- |
| 1 | Pine, flint, hemp cord | Pine fork, cord, leather strip | Pine, cord, bone hook |
| 2 | Birch, copper fitting, stone edge | Birch, beaver leather grip | Birch, copper hook |
| 3 | Mesquite, bronze edge | Mesquite bow, cotton string | Mesquite, bronze hook, cotton grip |
| 4 | Oak, iron edge | Oak bow, boar leather | Oak, iron hook, leather grip |

Branches/logs/planks provide handles and frames; metal supplies fittings/edges rather than being implied to improve automatically because it is more expensive. Do not add ammunition inventory in the first hunting implementation; tool uses already account for action cost.

### 8.4 Elite upgrades consume a Normal tool and earlier advanced materials

**Confirmed direction:** an Elite is an upgrade recipe, not a second expensive rebuild from its own tier's advanced wood/hide/cloth. It consumes **one same-family, same-tier Normal tool instance** plus a small fixed quantity of **previous-tier** advanced material. No tier-one Elite. Keep exact input IDs; a different Normal tier, an Elite, or another material species cannot substitute.

Proposed complete active upgrade table, one output per craft:

| Elite output | Exact consumed Normal instance | Previous-tier additional material |
| --- | --- | --- |
| Tier 2 Elite birch axe | 1 Normal birch axe | 2 Pine planks (tier 1) |
| Tier 2 Elite birch slingshot | 1 Normal birch slingshot | 2 Fine rabbit pelts (tier 1) |
| Tier 2 Elite birch rod | 1 Normal birch rod | 2 Fine hemp cloth (tier 1) |
| Tier 3 Elite mesquite axe | 1 Normal mesquite axe | 2 Birch planks (tier 2) |
| Tier 3 Elite mesquite bow | 1 Normal mesquite bow | 2 Fine beaver pelts (tier 2) |
| Tier 3 Elite mesquite rod | 1 Normal mesquite rod | 2 Fine flax cloth (tier 2) |
| Tier 4 Elite oak axe | 1 Normal oak axe | 2 Mesquite planks (tier 3) |
| Tier 4 Elite oak bow | 1 Normal oak bow | 2 Turkey feather weave (tier 3) |
| Tier 4 Elite oak rod | 1 Normal oak rod | 2 Fine cotton cloth (tier 3) |

Tier-three turkeys have no pelts: the tier-four hunting upgrade therefore uses an advanced turkey-feather material. Its fixed Material recipe is 3 Turkey feathers + 1 Cotton thread -> 1 Turkey feather weave. Fine flax cloth is 3 Flax cloth + 1 Hemp thread -> 1. Fine hemp cloth uses 3 Hemp cloth + 1 Hemp thread -> 1, never tier-two beeswax. Materials remain unlevelled/furniture-quality-only. Unlock tier-one Pine planks/Fine rabbit pelts/Fine hemp cloth before tier-two Elite requests; tier-two Birch planks/Fine beaver pelts/Fine flax cloth before tier three; and tier-three Mesquite planks/Feather weave/Fine cotton cloth before tier four. Add their exact refining missions to the previous block, not an inaccessible current-tier requirement. House/other sinks remain competing uses.

**Recommended input-condition policy:** allow a partially used Normal tool with positive uses remaining; do not require a fresh/full-durability copy. Show its identity/uses/quality/value and predicted Elite output before committing; let the player choose among eligible exact-ID instances. Autofill should prefer the least valuable qualifying instance, visibly selected, never silently sacrifice a valuable Q5 copy. Backpack/hooks are eligible; tools are not equipped. The condition/selection policy is proposed. Zero-use, mission-consumed, trade-listed, or otherwise unavailable tools are invalid inputs.

At craft start, reserve and consume the one Normal instance plus exact materials atomically, then record one pending Elite output. The Normal does not survive as a spare or remain harvestable during the timer. At completion, create a new unique Elite instance with stats from **its own recipe mastery, finished quality, and current-at-start furniture snapshot**, not the input tool's grade/mastery/remaining uses. Credit only the finished Elite recipe once; reusing the input never grants more Normal mastery. The input condition has no hidden ingredient-substitution effect.

The Elite has more base durability than Normal at equal tier/mastery/quality/modifiers. An unmastered Q1 Elite does not automatically inherit the total durability of an old mastery-ten Q5 Normal; preview that difference rather than silently changing the established stat rules. Its reduced action counts/drop boost still apply independently of quality. Upgrade stats/prices are authored for the new output; consuming an expensive Normal is a real cost, not a cash refund.

Missions keep automatic tool consumption. A Normal-tool request may consume a newly claimed Normal before it can be upgraded; complete that requirement/claim and plan the next accepted mission before making a spare. An upgrade cannot take a tool already contributed. Do not hide tools from missions or add bulk upgrading. All upgrades stay **x1**. Failed reservation, missing materials, full queue, and reload cannot duplicate either instance or charge twice.

## 9. Processing chains and recipes

All counts below are **units**, not stacks. One 1,000-unit pickup can fund many recipes during mechanics testing; do not inflate costs to counteract it. Aim for **one to three distinct ingredients**, with four only for a clear advanced gameplay reason. The nine-slot UI is an upper bound, not a target to fill. Omit real-life reagents/intermediates when they add no useful choice; waiting time can represent fermentation, firing, or curing. Material recipes still have no mastery, with instant conversion when their output is gatherable.

### 9.0 Exact ingredients and unique recipes

**Confirmed:** each ingredient requirement is one exact `ItemDefinitionId` plus a quantity, never an OR-list, category tag, minimum resource tier, material-family selector, or player-selected alternative. For example, `recipe.cheese.cow` requires `food.milk.cow`, not a generic Milk tag. A request to craft it with goat milk fails without spending anything. Manual placement and Autofill obey the same rule; shortages remain shortages even if another milk/wood/metal/fish is available.

Examples of separate recipes, not choices inside one recipe:

- Cow cheese: 3 cow milk + 1 salt -> 1 cow cheese.
- Goat cheese: 3 goat milk + 1 salt -> 1 goat cheese.
- Mixed-milk cheese, if authored: 1 cow milk + 1 goat milk + 1 salt -> 1 mixed-milk cheese. Two cow milk or two goat milk cannot satisfy it.

These are candidate recipe definitions with independent IDs; author only those needed, without a combinatorial expansion of every ingredient combination. Distinct cheese outputs are recommended so downstream recipes can explicitly require cow cheese, goat cheese, or the mixed product. A cow-cheese tart cannot accept the other cheeses unless a separate tart recipe is authored for them.

Mastery/unlocks/timers are per nonmaterial recipe ID: crafting cow cheese does not level or unlock goat cheese. Material conversions have unique recipe IDs but no mastery, as before. Each batch executes one recipe only; it cannot mix cow-cheese and goat-cheese executions under one generic Cheese batch.

Quality grades of the **same exact ingredient ID** remain governed by the recipe's explicit quality requirement and visible inventory-selection policy. Combining quantities from several qualifying cow-milk stacks is allowed; counting goat milk as cow milk is not. Input grade does not determine output grade under the existing crafting formula. Tags remain useful for inventory eligibility/search/UI grouping, never ingredient substitution.

An editor template may generate separate pine/birch/etc. definitions with exact IDs; it must not leave a runtime wildcard. Family diagrams and a mission phase summary are not executable recipes. Missing concrete inputs in a draft must be resolved before enabling that recipe. Validate a recipe's fixed requirements again at reservation/commit; never silently substitute on a race or shortage.

### 9.0.1 Recipe unlock tiers and cooking pantry exceptions

**Confirmed standard rule:** a recipe's authored first-unlock resource tier `R` determines its **direct** ingredient tier window: tier one -> `{1}`; later tiers -> `{R - 1, R}`. Tier two can use 1/2; tier three 2/3; tier four 3/4. It may use either or both permitted tiers, but cannot use arbitrary older materials or future-tier inputs. A tier-one recipe learned late or crafted while visiting tier four remains a tier-one recipe. Recipe mastery 1-10 and quality Q1-Q5 never change ingredient tiers.

Assign explicit `RecipeUnlockTier` and `ItemDefinition.MaterialTier`; do not infer them from current location, quality, store price, or a localized name. Check the direct ingredient IDs, not their entire historical supply chain: an allowed tier-two material may itself have been made using tier-one inputs in its own valid tier-two recipe.

**Confirmed cooking exception:** `Food` recipes can use explicitly designated older global cooking ingredients beyond that window. **Proposed pantry list:** exact Wheat flour, Barley flour, Corn flour, Oats, Rice, Cow cheese, Goat cheese, Mixed-milk cheese, Cow milk, Goat milk, Cow cream, Cow butter, Cow yogurt, Egg, Rock salt, Water, Sugar, Honey, Tea herbs, Flax oil, and Sunflower oil. Proposed recipe-container exceptions: exact Clay cup and Glass bottle. These remain distinct IDs: a recipe requiring Cow cheese cannot take Goat cheese even though both are pantry-marked. More globals require a deliberate data change, not a broad 'any cooking ingredient' wildcard.

Main meats/fish, regional fruits/vegetables, wood/fuel, and arbitrary textiles/metals/gems are **not** global cooking ingredients. For example a tier-four roast cannot use a Pine branch just because a branch is fuel, and a tier-four fish recipe cannot substitute a tier-one fish. Flour/cheese can remain shared ingredients without making all regional foods interchangeable.

Validation per required item: exact ID/quantity/quality checks **and** `(materialTier in allowedTierWindow OR (recipeRole == Food AND item.IsGlobalCookingIngredient AND materialTier <= RecipeUnlockTier))`. Availability is still required: a pantry flag never introduces an undiscovered future ingredient into an early mandatory recipe. Cooking with older staples does not remove that ingredient's quality/stat profile or master its originating recipe again. `IsGlobalCookingIngredient` is a permission on one exact ID, not an input selector.

The exception does not apply to Material/Tool/Wearable/Misc recipes. House-upgrade deposits, furniture purchases, and mission collection requests are not crafting recipes; retain their separately authored source/progression costs rather than accidentally applying recipe rules to every transaction. Thus older glass panels in a later house deposit remain a valid long-term sink, while a tier-four crafted garment must obey 3/4 inputs.

### 9.0.2 Explicit tier bridges for nonfood recipes

To remove the draft's older-tier leaks without interchangeable ingredients, use these fixed Material recipes:

| Output / material tier | Exact ingredients -> yield |
| --- | --- |
| Pine charcoal / 1 | 3 Pine branches -> 2 |
| Birch charcoal / 2 | 3 Birch branches -> 2 |
| Mesquite charcoal / 3 | 3 Mesquite branches -> 2 |
| Oak charcoal / 4 | 3 Oak branches -> 2 |
| Flax cord / 2 | 3 Flax fiber -> 2 |
| Cotton cord / 3 | 3 Cotton fiber -> 2 |
| Flax stuffing / 2 | 3 Flax fiber -> 2 |
| Cotton stuffing / 3 | 3 Cotton fiber -> 2 |

These are new exact IDs, not choices inside generic Charcoal/Cord/Stuffing. Existing Plant stuffing remains tier one from Nettle fiber. Source profiles set Beeswax to tier two, optional Raw wool to tier three, and refined outputs to their named resource tier. The small added bridge set replaces deep ingredient-chain exceptions; only implement bridges used by active recipes. Use previous-tier charcoal for smelting where listed, later cords for later tools/panels, and later stuffing for later toys. None adds a fifth resource tier or changes Elite's same-tier Normal + previous-tier advanced-material rule.

### 9.1 Shared processing chains

| Family | Processing chain | Later uses |
| --- | --- | --- |
| Wood | 3 matching branches -> 1 log; 3 matching logs -> 1 plank; planks -> joinery beams -> carved panels; branches can also make charcoal | Tools, sculptures, house structure |
| Pelts | Raw pelt -> cured pelt -> fine pelt; hide -> leather -> fine leather | Clothing, backpacks, stuffed toys |
| Fibers | Raw fiber -> thread/cord -> cloth -> fine cloth | Shirts, pants, bags, toys |
| Optional active wool | Raw wool -> yarn -> wool cloth -> fine cloth | Higher active-tier clothing; cashmere/silk chains deferred |
| Metals | Ore + charcoal -> ingot -> fittings/nails; copper + tin -> bronze; iron + charcoal -> steel | Tools, house hardware, jewelry |
| Stone | Rough stone -> cut blocks/tiles -> polished stone | House upgrade materials and sale sculptures; no furniture crafting |
| Gems | Rough gem -> cut gem -> set jewelry | High-value trading goods |
| Clay/glass | Clay -> bricks or small cups; quartz + charcoal -> glass -> bottles or panels | Bricks/panels for house upgrades; cups/bottles for advanced recipes; no direct windows/oven furniture |
| Food | Crop/meat/fish -> cooked/basic dish -> composite meal or beverage | Stamina recovery and sale |
| Dairy | Milk -> cream/yogurt/cheese; cream -> butter | Food and bakery inputs |

Abstract leather through fixed tier-valid recipes: Rabbit pelt + Rock salt makes tier-one Rabbit leather; Beaver pelt + Rock salt makes tier-two Beaver leather; Boar hide + Sunflower oil makes tier-four Boar leather with a tier-three aid. Rock salt's cooking exception cannot be used to bypass the nonfood tier-four rule. No species substitution/tannin chain/rabbit-hide duplicate is needed. Toys use named tier-valid stuffing. Furniture remains store-only.

### 9.2 Processing and construction recipe seeds

**Confirmed material rules:** no recipe levels or mastery counters. Species-specific log conversion is instant because logs are also gatherable; 3 branches produce 1 log. Three logs produce 1 plank; because planks are crafted-only in this catalog, their material recipe may have a fixed, tier-authored timer. Material output quality/value uses only its definition and craft-quality furniture, not ingredient quality or craft count. All conversion costs are unit counts, including in x1000 crafting.

| Output | Inputs per craft | Yield | Context/timer starting value |
| --- | --- | --- | --- |
| Pine log | 3 pine branches | 1 | Camp/home, instant, no mastery |
| Birch log | 3 birch branches | 1 | Camp/home, instant, no mastery |
| Mesquite log | 3 mesquite branches | 1 | Camp/home, instant, no mastery |
| Oak log | 3 oak branches | 1 | Camp/home, instant, no mastery |
| Pine plank | 3 pine logs | 1 | Camp/home, fixed material timer, early example 10 s |
| Birch plank | 3 birch logs | 1 | Camp/home, fixed tier-two material timer |
| Mesquite plank | 3 mesquite logs | 1 | Camp/home, fixed tier-three material timer |
| Oak plank | 3 oak logs | 1 | Camp/home, fixed tier-four material timer |
| Pine charcoal | 3 pine branches | 2 | Tier-one Material, 10 s; no other wood accepted |
| Birch charcoal | 3 birch branches | 2 | Tier-two fixed Material recipe |
| Mesquite charcoal | 3 mesquite branches | 2 | Tier-three fixed Material recipe |
| Oak charcoal | 3 oak branches | 2 | Tier-four fixed Material recipe |
| Hemp cord | 3 hemp fiber | 2 | Camp/home, 5 s |
| Flax cord | 3 flax fiber | 2 | Tier-two Material |
| Cotton cord | 3 cotton fiber | 2 | Tier-three Material |
| Flax stuffing | 3 flax fiber | 2 | Tier-two Material |
| Cotton stuffing | 3 cotton fiber | 2 | Tier-three Material |
| Nettle thread | 3 nettle fiber | 3 | Camp/home, 5 s; separate recipe |
| Hemp thread | 3 hemp fiber | 3 | Camp/home, 5 s; separate recipe |
| Cotton thread | 3 cotton fiber | 3 | Camp/home, 5 s |
| Nettle cloth | 4 nettle fiber | 2 | Camp/home, 10 s |
| Hemp cloth | 4 hemp fiber | 2 | Camp/home, 10 s |
| Cotton cloth | 4 cotton fiber | 2 | Camp/home, 10 s |
| Fine hemp cloth | 3 hemp cloth + 1 hemp thread | 1 | Camp/home, 20 s; fully tier-one sources |
| Fine flax cloth | 3 flax cloth + 1 hemp thread | 1 | Camp/home, fixed material timer; tier-two advanced cloth |
| Fine cotton cloth | 3 cotton cloth + 1 beeswax | 1 | Camp/home, 20 s |
| Turkey feather weave | 3 turkey feathers + 1 cotton thread | 1 | Camp/home, fixed material timer; tier-three advanced animal material |
| Cured rabbit pelt | 2 raw rabbit pelts + 1 salt | 1 | Camp/home, 15 s |
| Cured beaver pelt | 2 raw beaver pelts + 1 salt | 1 | Camp/home, 15 s |
| Rabbit leather | 2 raw rabbit pelts + 1 salt | 1 | Camp/home, 15 s |
| Beaver leather | 2 raw beaver pelts + 1 salt | 1 | Camp/home, 15 s |
| Boar leather | 2 boar hides + 1 sunflower oil | 1 | Tier-four Material with tier-three oil |
| Fine rabbit leather | 2 rabbit leather + 1 hemp thread | 1 | Tier-one Material; no future beeswax |
| Fine beaver leather | 2 beaver leather + 1 beeswax | 1 | Camp/home, 20 s |
| Fine boar leather | 2 boar leather + 1 cotton thread | 1 | Tier-four Material with tier-three thread |
| Fine rabbit pelt | 2 cured rabbit pelts + 1 hemp thread | 1 | Camp/home, 20 s |
| Fine beaver pelt | 2 cured beaver pelts + 1 hemp thread | 1 | Camp/home, 20 s |
| Wool yarn | 3 raw wool | 2 | Camp/home, 5 s |
| Wool cloth | 3 wool yarn + 1 cotton thread | 2 | Tier-three Material |
| Copper ingot | 3 copper ore + 1 pine charcoal | 1 | Tier-two Material |
| Tin ingot | 3 tin ore + 1 pine charcoal | 1 | Tier-two Material |
| Iron ingot | 3 iron ore + 1 birch charcoal | 1 | Tier-three Material |
| Silver ingot | 3 silver ore + 1 birch charcoal | 1 | Tier-three Material |
| Gold ingot | 3 gold ore + 1 oak charcoal | 1 | Tier-four Material; no wallet conversion |
| Bronze ingots | 2 copper ingots + 1 tin ingot | 3 | Camp/home, 20 s |
| Steel ingot | 2 iron ingots + 1 mesquite charcoal | 1 | Tier-four Material using tier-three inputs |
| Nails | 1 iron ingot | 6 | Camp/home, 5 s |
| Copper fittings | 1 copper ingot | 3 | Camp/home, 10 s |
| Iron fittings | 1 iron ingot | 3 | Camp/home, 10 s |
| Steel fittings | 1 steel ingot | 3 | Camp/home, 10 s |
| Pine beam | 4 pine planks + 1 hemp cord | 1 | Camp/home, 20 s |
| Birch beam | 4 birch planks + 1 hemp cord | 1 | Camp/home, fixed material timer |
| Mesquite beam | 4 mesquite planks + 1 flax cord | 1 | Tier-three Material |
| Oak beam | 4 oak planks + 1 cotton cord | 1 | Tier-four Material |
| Pine panel | 3 pine planks | 1 | Tier-one Material; no future beeswax |
| Birch panel | 3 birch planks + 1 beeswax | 1 | Camp/home, fixed material timer |
| Mesquite panel | 3 mesquite planks + 1 beeswax | 1 | Camp/home, fixed material timer |
| Oak panel | 3 oak planks + 1 sunflower oil | 1 | Tier-four Material with tier-three oil |
| Limestone tiles | 3 limestone | 2 | Camp/home, 15 s |
| Sandstone tiles | 3 sandstone | 2 | Camp/home, 15 s |
| Granite tiles | 3 granite | 2 | Camp/home, fixed material timer |
| Slate tiles | 3 slate | 2 | Camp/home, fixed material timer |
| Marble tiles | 3 marble | 2 | Camp/home, fixed material timer |
| Polished basalt | 2 basalt | 1 | Tier-four Material; omit an obsolete tier-two abrasive |
| Polished pebble | 2 pebbles + 1 flint | 1 | Tier-one Material; no future quartz |
| Fired bricks | 3 clay + 1 pine charcoal | 3 | Tier-one Material |
| Glass | 3 quartz + 1 pine charcoal | 2 | Tier-two Material |
| Clay cup | 2 clay | 1 | Camp/home, fixed material timer; advanced food/drink container |
| Glass bottle | 2 glass | 1 | Camp/home, fixed material timer; advanced drink container |
| Glass panels | 3 glass | 1 | Camp/home, fixed material timer; house upgrade material, not a window furniture item |
| Cut quartz | 2 quartz | 1 | Camp/home, 20 s |
| Cut turquoise | 2 turquoise | 1 | Camp/home, 20 s |
| Wheat flour | 3 wheat | 2 | Camp/home, 5 s |
| Barley flour | 3 barley | 2 | Camp/home, 5 s; separate recipe/output |
| Sugar | 3 sugar cane | 2 | Camp/home, 10 s |
| Cow cream | 3 cow milk | 2 | Camp/home, Food level-one 10 s |
| Cow butter | 2 cow cream | 1 | Camp/home, Food level-one 10 s |
| Cow cheese | 3 cow milk + 1 salt | 1 | Camp/home, Food level-one 5 min; cow milk only |
| Goat cheese | 3 goat milk + 1 salt | 1 | Separate Food recipe/mastery, level-one 5 min |
| Mixed-milk cheese, optional | 1 cow milk + 1 goat milk + 1 salt | 1 | Separate Food recipe/mastery; both milks required |
| Cow yogurt | 2 cow milk | 2 | Camp/home, Food level-one 1 min; no culture item |

No starter culture, yeast, enzyme, or tannin is required. Each cheese recipe uses its exact named milk(s) and salt; waiting abstracts the process and follows the Food mastery/time limit. Nettle cloth and hemp cloth are separate tier-one recipes, not interchangeable inputs. Cotton belongs to tier three. Extra cut-gem/wood/metal variants require their own explicit recipe/output definitions before use. Provide guaranteed early fiber/salt sources so material-first missions are achievable.

Classify each recipe by its **primary output role**, not by whether it appears as an ingredient. Logs, planks, cord/thread/cloth, leather, ingots, fittings, tiles, beams, panels, cups/bottles, flour, and sugar are Material recipes. Cream, butter, cheese, and yogurt are Food recipes: ingredient use does not remove their mastery/timer rules. Food times are authored level-one examples. Packaging cups/bottles are stackable crafted materials, not storage furniture or equipment, and are consumed into the final recipe. Do not add reusable-container return/recycling mechanics initially. Using prepared food as an ingredient never grants its recipe mastery twice.

The material rows' nonzero times are fixed per recipe/tier, not improved by use. Add `CanGatherOutput` to recipe validation: when true, a Material recipe must have zero duration. A shop selling a material does not alone make it gatherable. If a later loot table adds gatherable planks, that species' plank recipe becomes instant too. Purely crafted advanced materials may take longer at higher tiers, but still never acquire mastery levels.

### 9.3 Tools, clothes, and backpack recipe templates

| Output | Example inputs | Output rule |
| --- | --- | --- |
| Normal pine axe | 2 pine branches + 2 flint | One unique tool; early Lumberjack L1 recipe |
| Normal birch axe | 2 birch branches + 1 copper ingot + 1 hemp cord | Tier-two Normal; proposed low-input rebuild, no drop boost |
| Normal birch slingshot | 2 birch branches + 1 raw beaver pelt + 1 hemp cord | Tier-two Normal hunting tool |
| Normal birch rod | 2 birch branches + 1 copper fitting + 1 hemp cord | Tier-two Normal rod |
| Elite birch axe | 1 Normal birch axe + 2 pine planks | Tier-two upgrade using tier-one wood |
| Elite birch slingshot | 1 Normal birch slingshot + 2 fine rabbit pelts | Tier-two upgrade using tier-one animal material |
| Elite birch rod | 1 Normal birch rod + 2 fine hemp cloth | Tier-two upgrade using tier-one cloth |
| Normal pine slingshot | 2 pine branches + 1 raw rabbit pelt | One unique hunting tool; early Hunter H1 recipe |
| Normal pine rod | 2 pine branches + 2 hemp cord + 1 bone | Tier-one Normal rod, Explorer E5 |
| Normal mesquite axe | 2 mesquite branches + 1 iron ingot + 1 flax cord | Tier-three Normal with tier-two cord |
| Normal mesquite bow | 2 mesquite branches + 2 turkey feathers + 1 flax cord | Tier-three Normal with tier-two cord |
| Normal mesquite rod | 2 mesquite branches + 1 iron fitting + 1 cotton thread | Tier-three Normal rod |
| Elite mesquite axe | 1 Normal mesquite axe + 2 birch planks | Tier-three upgrade using tier-two wood |
| Elite mesquite bow | 1 Normal mesquite bow + 2 fine beaver pelts | Tier-three upgrade using tier-two animal material |
| Elite mesquite rod | 1 Normal mesquite rod + 2 fine flax cloth | Tier-three upgrade using tier-two cloth |
| Normal oak axe | 2 oak branches + 1 steel ingot + 1 cotton cord | Tier-four Normal with tier-three cord |
| Normal oak bow | 2 oak branches + 1 boar hide + 1 cotton thread | Tier-four Normal hunting tool |
| Normal oak rod | 2 oak branches + 1 steel fitting + 1 cotton thread | Tier-four Normal rod |
| Elite oak axe | 1 Normal oak axe + 2 mesquite planks | Tier-four upgrade using tier-three wood |
| Elite oak bow | 1 Normal oak bow + 2 turkey feather weave | Tier-four upgrade using tier-three animal material |
| Elite oak rod | 1 Normal oak rod + 2 fine cotton cloth | Tier-four upgrade using tier-three cloth |
| Rank-one boots | 2 raw rabbit pelts + 1 hemp fiber | Lumberjack reward; speed capability rank one |
| Rank-one hat | 2 raw rabbit pelts | Gatherer reward; four-heart cap |
| Rank-one shirt | 3 hemp cloth + 1 raw rabbit pelt | Hunter reward; two pockets |
| Rank-one pants | 3 hemp cloth + 1 pine branch | Explorer reward; two hooks, rag default zero |
| Rank-one backpack | 3 hemp cloth + 2 rabbit leather + 2 pine branches | 8 x 4 logical area |
| Tier-three feather garment | 3 cotton cloth + 2 turkey feathers + 1 beaver leather | Use existing leather plus the new bird/cloth materials; no invented turkey hide |
| Tier-four shirt | 4 fine cotton cloth + 2 fine boar leather | Ten pockets maximum |
| Tier-four pants | 4 fine cotton cloth + 2 iron fittings + 1 boar leather | Ten hooks maximum |
| Tier-two hat | 2 fine beaver pelts + 1 hemp thread | Six-heart cap |
| Tier-two backpack | 4 hemp cloth + 3 beaver leather + 2 birch planks + 2 copper fittings | 10 x 4 area; fixed four-ingredient advanced recipe |
| Tier-three backpack | 4 cotton cloth + 3 beaver leather + 2 mesquite planks + 2 iron fittings | 12 x 4 area; no generic leather/metal selector |
| Tier-four backpack | 4 fine cotton cloth + 3 fine boar leather + 2 oak planks + 2 steel fittings | 14 x 4 area; no substitutions |

These are individual fixed-ingredient recipes. An authoring template may help create additional definitions, but never exposes a runtime material choice. Remaining hats/boots/shirts/pants need their own exact recipes before activation; no generic higher-tier garment recipe fills those gaps automatically. Indestructible merchant gear remains noncraftable with the same global caps.

Every tool is Normal unless marked Elite. The table now covers all 21 active tool definitions: three tier-one Normals plus Normal/Elite pairs at tiers two-four. Proposed Normal recipes use current-tier branches/core metal or animal inputs rather than a second large current-tier plank bundle. Elite then consumes that Normal plus exactly two previous-tier advanced units as section 8.4 specifies. The Elite keeps family/tier but has fewer actions up to that tier, greater base durability, and `HasDropBoost = true` for every eligible target. Normals never gain a drop buff from over-tier advantage. All Tool/Wearable recipes stay x1 only.

### 9.4 Food and beverages

| Recipe | Inputs | Yield | Hearts per serving |
| --- | --- | --- | --- |
| Apple juice | 3 apples | 1 | 1; region-one unlock, proposed level-one 3-second timer |
| Roasted mushrooms | 2 mushrooms + 1 pine branch | 1 | 1; tier-one wood is consumed |
| Berry oat cakes | 2 oats + 2 red berries | 2 | 1 |
| Roasted rabbit | 1 raw rabbit meat + 1 pine branch | 1 | 2; Explorer reward, no salt prerequisite |
| Roasted beaver | 1 raw beaver meat + 2 birch branches | 1 | 3; tier-two roast |
| Roasted turkey | 1 raw turkey meat + 3 mesquite branches | 1 | 4; tier-three roast |
| Roasted boar | 1 raw boar meat + 1 oak log + 2 oak branches | 1 | 6; tier-four roast |
| Herb trout | 1 rainbow trout + 1 tea herb + 1 pine branch | 1 | 2; separate tier-one recipe, no salt needed |
| Corn flatbread | 2 corn flour + 1 water | 2 | 2 |
| Turkey bean stew | 2 turkey meat + 2 beans + 1 tomato | 2 | 3 |
| Mesquite nut cakes | 2 mesquite pods + 1 wheat flour + 1 honey | 2 | 3 |
| Boar potato stew | 2 boar meat + 2 potatoes + 1 salt | 2 | 4 |
| Carp rice bowl | 2 rice + 2 common carp + 1 salt | 2 | 4 |
| Pike rice bowl | 2 rice + 2 northern pike + 1 salt | 2 | 4; independent recipe/output |
| Rabbit barley soup | 2 rabbit meat + 2 barley + 1 tea herb | 2 | 5 |
| Cow-cheese tart | 2 wheat flour + 1 cow cheese + 1 egg | 2 | 5; goat/mixed cheese not accepted |
| Boar bean chili | 2 boar meat + 2 beans + 1 green chili | 2 | 6; tier-four recipe, no tier-two tomato |
| Salmon rice parcel | 2 salmon + 2 rice + 1 tea herb | 2 | 6 |
| Beaver mushroom pot | 2 beaver meat + 2 mushrooms + 1 carrot | 2 | 6; tier-two recipe using tier-one mushrooms |
| Honey herbal tea | 1 tea herb + 1 honey + 1 water | 1 | 1 |
| Honey oat drink | 1 oats + 1 honey + 1 cow milk | 1 | 2 |
| Strawberry cow-yogurt drink | 1 cow yogurt + 2 strawberries | 1 | 3 |
| Sweet coffee | 1 roasted coffee + 1 sugar + 1 water | 1 | 3 |
| Hot cacao | 1 cacao powder + 1 cow milk + 1 sugar | 1 | 4 |
| Vanilla cow-cream drink | 1 cow cream + 1 vanilla + 1 sugar | 1 | 4 |
| Soy milk | 2 soybeans + 1 water | 2 | 1 |
| Tofu vegetable bowl | 2 tofu + 1 rice + 1 tomato | 2 | 4 |
| Pumpkin oat porridge | 2 pumpkin + 1 oats + 1 cow milk | 2 | 3 |
| Bottled honey tea | 1 tea herb + 1 honey + 1 glass bottle | 1 | 3 |
| Hot cacao cup | 1 cacao powder + 1 cow milk + 1 clay cup | 1 | 4 |

Add corn flour (3 Corn -> 2) and tofu (2 Soy milk + 1 Rock salt -> 1) with explicit Food/Material roles; tofu's older salt is a cooking exception. Roasted coffee is the exact tier-four Material recipe 2 Coffee beans + 1 Oak branch -> 2; no interchangeable fuel. Cacao powder is 2 Cacao beans -> 2. Maple/venison/buffalo/bear foods stay deferred. Water may be implicit except where an exact Water ingredient is listed. Beverage recovery adds no thirst/temperature/caffeine system. Pantry-marked Oats/Rice/flour/dairy/etc. permit the older staple inputs above; main produce/proteins and wood still obey the tier window. Recipe unlock/source metadata, not current player tier, controls these fixed recipes.

Bakery assortment: one cash stack of bread/oat biscuits/pumpkin rolls recovering 1-6 hearts, and two gold stacks selected from honey brioche (8), berry cream pastries (10), premium apple pie (12), and vanilla cacao celebration cake (12). Premium apple pie is a proposed Q5 bakery-only gold good, separate from apple juice and the box compote recipe; buying it grants neither recipe. All these values are total progressive recovery; a twelve-heart pie can be used with a three-heart hatless meter because excess remains pending (section 4.4.3). Shop-exclusive goods can be authored directly without a dependency graph for every real-life ingredient. They are optional; ordinary crafted meals support all trail progression without gold food.

### 9.4.1 Wood-fueled roasting and exact fish recipes

**Confirmed:** wood in roasted recipes represents fuel/skewer material and is consumed as an ingredient. It is not tool durability, a reusable equipped utensil, a timer-only decoration, or a generic substitute fuel. Keep the ingredient window: tier-one Pine; tier-two Birch; tier-three Mesquite; tier-four Oak in the examples. Larger targets can require two/three branches or a log plus branches. No new stove prerequisite; unlocked recipes remain camp/home crafts.

Each row below is a separate named Food recipe/output with its own mastery and exact fish ID. Proposed fuel quantities use the animal-roast progression without expanding to realistic seasoning/processing chains:

| Unlock tier | Exact roasted-fish output | Fixed inputs -> one serving |
| --- | --- | --- |
| 1 | Roasted rainbow trout | 1 Rainbow trout + 1 Pine branch -> 1 |
| 1 | Roasted largemouth bass | 1 Largemouth bass + 1 Pine branch -> 1 |
| 1 | Roasted bluegill | 1 Bluegill + 1 Pine branch -> 1 |
| 1 | Roasted yellow perch | 1 Yellow perch + 1 Pine branch -> 1 |
| 2 | Roasted common carp | 1 Common carp + 2 Birch branches -> 1 |
| 2 | Roasted northern pike | 1 Northern pike + 2 Birch branches -> 1 |
| 2 | Roasted channel catfish | 1 Channel catfish + 2 Birch branches -> 1 |
| 2 | Roasted walleye | 1 Walleye + 2 Birch branches -> 1 |
| 3 | Roasted salmon | 1 Salmon + 3 Mesquite branches -> 1 |
| 3 | Roasted sturgeon | 1 Sturgeon + 3 Mesquite branches -> 1 |
| 3 | Roasted brown trout | 1 Brown trout + 3 Mesquite branches -> 1 |
| 3 | Roasted lake trout | 1 Lake trout + 3 Mesquite branches -> 1 |
| 4 | Roasted longnose gar | 1 Longnose gar + 1 Oak log + 2 Oak branches -> 1 |
| 4 | Roasted tuna | 1 Tuna + 1 Oak log + 2 Oak branches -> 1 |
| 4 | Roasted red drum | 1 Red drum + 1 Oak log + 2 Oak branches -> 1 |
| 4 | Roasted red snapper | 1 Red snapper + 1 Oak log + 2 Oak branches -> 1 |

Starting stamina values can be two/three/four/six hearts per tier-one/two/three/four serving, matching ordinary food planning for the 26-heart leg; species-specific tuning can follow without substituting ingredients. All are stackable, support existing bulk quantities, level 1-10, and stay under the one-hour Food timer limit. Bulk multiplies wood and fish costs exactly: x100 roasted boar needs 100 Raw boar meat + 100 Oak logs + 200 Oak branches; an x100 request never treats one wood piece as fuel for the whole batch.

Existing stews, drinks, and non-roasted dishes do not automatically receive another hidden fuel charge. The recipe itself must list each cost. Roasted coffee's material recipe also lists its Oak branch. Apple juice remains the fuel-free early survival recipe, and the revised early roasted-rabbit mission can use leftover Pine branches without salt or an advanced processing unlock.

### 9.4.2 Optional balloon-box recipe collection

**Confirmed direction:** add recipe unlocks to mystery-box rewards, mainly Food and Misc. The following **26 recipe seeds** (21 Food, 5 Misc, including eight additional high-recovery regional meals and two truffle dishes in section 9.4.3) are a proposal, separate from normal mission recipes. They never replace the first apple juice, roasted rabbit, Pine bird, Rabbit toy, or clothing unlocks. Make them optional discoveries, not random prerequisites for mandatory progression. Each ID is unique; names do not create interchangeable ingredient slots.

| Recipe ID / output | Unlock tier | Exact ingredients -> yield | Starting hearts per serving |
| --- | --- | --- | --- |
| `recipe.food.roasted_shrimp` — Roasted shrimp | 1 | 1 Shrimp + 1 Pine branch -> 1 | 2 |
| `recipe.food.shrimp_pie` — Shrimp pie | 1 | 1 Shrimp + 1 Wheat flour + 1 Cow butter -> 1 | 3 |
| `recipe.food.roasted_crab` — Roasted crab | 2 | 1 Crab + 2 Birch branches -> 1 | 3 |
| `recipe.food.creamy_crab_soup` — Creamy crab soup | 2 | 1 Crab + 1 Cow cream + 1 Carrot -> 1 | 4 |
| `recipe.food.honey_blackberry_biscuits` — Honey-blackberry biscuits | 2 | 2 Wheat flour + 1 Honey + 1 Blackberry -> 2 | 3 |
| `recipe.food.roasted_squid` — Roasted squid | 3 | 1 Squid + 3 Mesquite branches -> 1 | 4 |
| `recipe.food.squid_rice_bowl` — Squid rice bowl | 3 | 1 Squid + 1 Rice + 1 Green chili -> 1 | 5 |
| `recipe.food.mint_cow_cheese_flatbread` — Mint cow-cheese flatbread | 3 | 1 Wheat flour + 1 Cow cheese + 1 Wild mint -> 1 | 4 |
| `recipe.food.roasted_lobster` — Roasted lobster | 4 | 1 Lobster + 1 Oak log + 2 Oak branches -> 1 | 6 |
| `recipe.food.lobster_rice_bowl` — Lobster rice bowl | 4 | 1 Lobster + 1 Rice + 1 Seaweed -> 1 | 6 |
| `recipe.food.cacao_coconut_tart` — Cacao-coconut tart | 4 | 1 Cacao powder + 1 Coconut + 1 Wheat flour -> 1 | 6 |

**Additional regional meals — proposed values, confirmed stronger-food direction:** these eight non-pie dishes extend the same optional box pool. Apple juice stays the guaranteed one-heart starter; a richer apple dish becomes an optional six-heart discovery. Pies can remain premium bakery goods. Every serving below has six or more total progressive hearts, no durability, and the usual Food mastery/timer rules.

| Recipe ID / output | Unlock tier | Exact ingredients -> yield | Total hearts per serving | Required discovered ingredient regions |
| --- | --- | --- | --- | --- |
| `recipe.food.apple_oat_compote` — Apple-oat compote | 1 | 3 Apples + 2 Oats + 1 Cow milk -> 1 | 6 | 1 and 5; reachable cow-milk shop |
| `recipe.food.red_berry_oat_porridge` — Red-berry oat porridge | 1 | 3 Red berries + 2 Oats + 1 Cow milk -> 1 | 6 | 5; reachable cow-milk shop |
| `recipe.food.beaver_barley_hotpot` — Beaver-barley hotpot | 2 | 2 Raw beaver meat + 2 Barley + 1 Carrot -> 1 | 7 | Beaver hunting in 7-12, Barley in 9, Carrot in 8 |
| `recipe.food.blackberry_honey_oats` — Blackberry-honey oats | 2 | 3 Blackberries + 2 Oats + 1 Honey -> 1 | 7 | 7, 5, and 11 |
| `recipe.food.turkey_bean_bowl` — Turkey-bean bowl | 3 | 2 Raw turkey meat + 2 Beans + 1 Corn flour -> 1 | 8 | Turkey hunting in 13-18, Beans in 14, Corn in 13 plus known flour conversion |
| `recipe.food.pumpkin_chickpea_pot` — Pumpkin-chickpea pot | 3 | 2 Pumpkins + 2 Chickpeas + 1 Cow cheese -> 1 | 8 | 15; reachable exact cow-cheese source |
| `recipe.food.boar_potato_chili` — Boar-potato chili | 4 | 2 Raw boar meat + 2 Potatoes + 1 Green chili -> 1 | 9 | Boar hunting in 19-24, Potatoes in 19, Green chili in 17 |
| `recipe.food.coconut_cacao_pudding` — Coconut-cacao pudding | 4 | 2 Coconuts + 1 Cacao powder + 1 Cow milk -> 1 | 10 | 24 and 20 plus known cacao conversion; reachable cow-milk shop |

The region column is an eligibility gate, not an extra ingredient or a requirement to hold stock. At box generation, require discovery of **every named regional ingredient source**, an accessible exact dairy/shop source, and a known conversion or reachable shop for flour/cheese/cacao. A discovered tier alone is insufficient: reaching region 1 does not make the region-5 oat recipes eligible. Older exact Oats/Honey/dairy/flour use existing Food pantry exceptions; main produce/meat remain within own/previous tier. None requires gold ingredients, a new station, a hidden fuel charge, or another random box recipe. Revisiting a discovered source region remains valid after progressing. Existing seafood/other recipe values are unchanged; these are stronger optional alternatives.

| Recipe ID / sale good | Unlock tier | Exact ingredients -> yield | Role |
| --- | --- | --- | --- |
| `recipe.misc.pinecone_rabbit` — Pine-cone rabbit ornament | 1 | 1 Pine cone + 1 Pine log + 1 Hemp fiber -> 1 | Misc sculpture |
| `recipe.misc.birch_wax_bee` — Birch-wax bee toy | 2 | 1 Birch plank + 1 Beeswax + 1 Raw beaver pelt -> 1 | Misc toy |
| `recipe.misc.painted_turkey` — Painted turkey carving | 3 | 1 Mesquite plank + 1 Turkey feather + 1 Yellow ochre -> 1 | Misc sculpture |
| `recipe.misc.silver_sun_brooch` — Silver sun brooch | 3 | 1 Silver ingot + 1 Cut turquoise + 1 Flax cord -> 1 | Misc jewelry |
| `recipe.misc.obsidian_boar` — Obsidian boar carving | 4 | 1 Oak plank + 1 Obsidian + 1 Sunflower oil -> 1 | Misc sculpture |

Food keeps the shorter-with-mastery timer under one hour, no durability, and the authored stamina recovery; Misc keeps longer-with-mastery timers and higher category prices, with no durability. Both stack to 1,000 per compatible profile and support normal bulk quantities. Apply exact wood costs to every roasted serving. Cooking pantry exceptions permit older named flour/dairy/rice in Food; they never allow another cheese/flour variety, future inaccessible inputs, old wood fuel, or pantry exemptions in Misc. Each Misc seed follows only its unlock tier/previous tier.

Use the existing category/tier timer curves and price references for initial definitions, then author per-recipe overrides if needed. No new stations, seafood processing trees, or mandatory rare reagents are required. These are sale/consumable products, not furniture recipes or special merchant clothing. Mark their recipe-book entries with **Source: Balloon mystery box** and show exact ingredient/source requirements while locked.

The seafood ingredients are bought Q5, but their quality does not override crafted-output rules: learning a recipe gives mastery level one, and its first craft uses that level plus craft-quality furniture. A recipe unlock itself has no item grade, durability, inventory footprint, stack count, or free output. Purchased physical rewards still follow Q5 policy. Store ingredient stock can appear before the recipe is found; it is not a reason to force the recipe through a main mission.

### 9.4.3 Truffle recipes and exact unlock sources

**Confirmed direction:** add gathering truffles and useful recipes; the following exact dishes/unlock placement are proposals. All use the existing Food timers/mastery and progressive stamina recovery (section 4.4.3); no hidden fuel or new cooking station. Truffle is a tier-two main ingredient, not interchangeable with Mushrooms and not a tier-four pantry exemption.

| Recipe ID / output | Unlock tier | Exact ingredients -> yield | Total hearts per serving | Unlock source and prerequisites |
| --- | --- | --- | --- | --- |
| `recipe.food.truffle_barley_pot` — Creamy truffle-barley pot | 2 | 2 Truffle + 2 Barley + 1 Cow cream -> 1 | 6 | Optional authored Gatherer side mission after region 10, Barley region 9 and reachable exact Cow cream source; collect 10 Truffle to learn |
| `recipe.food.truffle_cow_cheese_flatbread` — Truffle cow-cheese flatbread | 2 | 2 Truffle + 1 Wheat flour + 1 Cow cheese -> 1 | 8 | Balloon box; region 10 and region 6 Wheat/flour conversion or reachable exact flour shop; known/reachable exact Cow cheese source |
| `recipe.food.turkey_truffle_bean_pot` — Turkey-truffle bean pot | 3 | 1 Truffle + 2 Raw turkey meat + 2 Beans -> 1 | 9 | Balloon box; discovered truffle region 10, turkey hunting in 13-18 and Beans region 14 |

The ten-Truffle request is an explicit lower-cost optional tier-two mission override, not a new mandatory branch gate. It shares the existing one-active-mission limit, exact-ID consumption and camp hand-in; offer only after every ingredient source is accessible. It grants only the named recipe once at mastery one. Finish or defer it without blocking main missions/house/region progression. Guarantee sufficient loose truffle opportunities without any pet. The first recipe is mission-only and excluded from box rolls; the other two extend section 9.4.2's pool to **26 IDs = 21 Food + 5 Misc**. No free finished output is granted. Duplicate protection, six typed box slots, proposed roll weights, exact source discovery and mastery-one grants remain unchanged. Mark Truffle in the food-ingredient catalog and show its region-10 source in recipe previews.

### 9.5 Sculptures, jewelry, and stuffed toys

| Rank | Sculpture recipe | Jewelry recipe | Stuffed-toy recipe |
| --- | --- | --- | --- |
| 1 | Pine bird: 2 pine logs | Cord charm: 1 polished pebble + 1 hemp cord | Rabbit toy: 2 raw rabbit pelts + 1 hemp fiber |
| 2 | Birch beaver: 2 birch planks + 1 beeswax | Copper pendant: 1 copper ingot + 1 quartz + 1 hemp cord | Beaver toy: 2 fine beaver pelts + 1 plant stuffing + 1 hemp thread |
| 3 | Mesquite sun: 2 mesquite planks + 1 turquoise | Turquoise bracelet: 1 cut turquoise + 1 bronze ingot | Turkey toy: 2 cotton cloth + 2 turkey feathers + 1 flax stuffing |
| 4 | Oak boar: 2 oak planks + 1 sunflower oil | Ruby brooch: 1 iron fitting + 1 cut ruby | Boar toy: 2 fine boar leather + 1 cotton cloth + 1 cotton stuffing |

All products stack to 1,000 by compatible item ID/quality/stat profile. Plant stuffing has one fixed material recipe: 3 nettle fiber -> 2 plant stuffing. Reeds cannot replace nettle in that recipe. Extra silver/gold/gem jewelry variants need separate fixed definitions if introduced within tier four. Deferred species remain backlog only. Sale Misc is never functional furniture. Preserve full-stack testing while deferring price balance.

## 10. Crafting UI, timers, and bulk crafting

**Confirmed presentation:** keep the crafting menu and adjacent inventory open. Preserve the 3 x 3 ingredient grid, Craft/Autofill on its right, recipe book at top right, and Return at bottom right. Select a known recipe, exact ingredients and x1/x10/x100/x1000 for stackables; tools/wearables/backpacks are x1 only. Slots represent distinct ingredient types and reserved quantities, not one unit each. Preview resulting quality, applicable durability, total quantity, timer and bag footprint.

On Craft, validate location/ingredients/ownership/capacity/queue and consume ingredients atomically with creation of the output and batch record. Present the item or bulk **centered on the 3 x 3 grid**, showing quality, durability when applicable (otherwise no durability), and remaining timer for a proposed **1 second**, then animate it directly into the backpack. The menu stays open for the next recipe. This visual hold is not extra crafting time; the timer starts at committed consumption. Instant outputs show Instant. Prevent a second press from spending the same ingredients during the brief presentation.

The timed product **occupies real backpack space immediately after this drop**. It can be repositioned freely but cannot be eaten, equipped, sold, used as an ingredient, submitted to a mission, put on a shelf, or deposited in the warehouse until ready. The same batch's stackable outputs share one timer; split yield above 1,000 into legal stacks with a shared batch ID. Different pending batches never merge; ready outputs may merge by ordinary metadata rules. Tools/gear always remain unique x1 instances. Preserve the timer badge on the bag product; show a HUD ready indicator without closing/reopening crafting.

**Delegated question-5 default:** deterministically find free bag placement after removing inputs and reserve it before committing. Never rearrange existing player items. If there is no room, visibly route the output into the persistent overflow tray and animate toward its badge beside the backpack; the timer keeps running there. A pending output has exactly one owner (bag or tray), with a shared batch reference, never a duplicate item in both a bag and an invisible queue. Eight outstanding batch records is the proposed initial limit, including ready tray outputs; reject further requests without costs. Ready bag outputs need no second claim; tray transfer requires room. Completion credits mastery once whether the item is in bag or tray; mission eligibility begins only when ready and carried.

Use `1 + 0.2 * log2(batchCount)` as the proposed stackable duration factor, including x1000; x1 gear uses factor one. Category/mastery curves and the less-than-one-hour Food/Tool limit remain. Materials have no mastery and gathered-material conversions stay instant. Snapshot output stats/furniture at start. Persist output owner/placement, shared timer, completion/mastery transaction and presentation state atomically; reload resumes remaining time without duplicate output, repeated consumption or mastery. Presentation animation can be skipped after reload. Crafting is allowed at camps and inside the player's home only.

### 10.1 Recipe mastery levels 1-10

**Confirmed:** every nonmaterial recipe has its own mastery progression from crafted quantity. Material conversions never level. Recipe level is independent of region/tool/equipment rank, five-grade item quality, and house level.

Recommended cumulative **finished-output-unit** thresholds:

| Recipe level | Total finished units required | Baseline quality before furniture | Base-price factor versus level-ten reference | Durability multiplier, where applicable |
| --- | --- | --- | --- | --- |
| 1 | 0 | Q1 | 0.8000 | 1.00 |
| 2 | 5 | Q2 | 0.8222 | 1.08 |
| 3 | 15 | Q3 | 0.8444 | 1.16 |
| 4 | 35 | Q4 | 0.8667 | 1.24 |
| 5 | 65 | Q5 | 0.8889 | 1.32 |
| 6 | 110 | Q5 | 0.9111 | 1.40 |
| 7 | 180 | Q5 | 0.9333 | 1.48 |
| 8 | 280 | Q5 | 0.9556 | 1.56 |
| 9 | 430 | Q5 | 0.9778 | 1.64 |
| 10 | 650 | Q5 | 1.0000 | 1.72 |

These thresholds are proposed test data, not confirmed final pacing. A batch producing 2,000 finished food servings counts 2,000 units, not one button press. A stackable x1000 craft can therefore reach level ten rapidly, which is useful for the requested limit tests. Tools/wearables advance through repeated x1 crafts; bulk requests remain unavailable. Do not slow mastery just to offset full-stack pickups.

Snapshot mastery and furniture at the batch's start; every output in that batch uses that snapshot even if it finishes enough units to gain several levels. Credit finished units once when a successful batch becomes ready, not on start, inventory transfer, repeated claim, sale, or use. Partially claiming a ready batch does not grant more mastery. Instant materials contribute none. If multiple batches become ready together, each retains its own creation snapshot. A save transaction records both completion and credited units so reopening cannot farm mastery.

Only future batches benefit from a level gain or new furniture. Existing items retain their intrinsic values and durability. The recipe book shows level, count to next level, current/next stats, and the category-specific time change; material recipes display **No mastery** and **Instant** or their fixed timer instead of a fake level one.

### 10.2 Category-, tier-, and mastery-dependent timers

**Confirmed direction:** Tool and Food levels shorten timers. Wearable and Misc levels lengthen timers. Low-tier crafts take seconds; advanced wearables/misc may take hours. Tool and Food craft timers must always stay below one hour. Materials have no level-based time adjustment, and gatherable materials are instant.

Recommended formula for single-craft duration:

- Gatherable Material: `0`, regardless of tier, quantity, furniture, or number of previous crafts.
- Crafted-only Material: authored fixed `materialBaseSeconds` for that recipe/tier.
- Tool/Food: `tierBaseSeconds / (1 + 0.08 * (L - 1))`.
- Wearable/Misc: `tierBaseSeconds * (1 + 0.10 * (L - 1))`.

Then apply the recommended batch factor once. Keep high-resolution time internally and round the display only, so UI rounding does not reverse the timer direction. Craft-quality furniture and all durability buffs have **zero** effect on timers. Food-recovery and sale-price furniture do not change them either.

Starting level-one, x1 base-duration examples:

| Progression tier | Tool | Food | Wearable, including backpack | Misc, including toys/jewelry/sculptures |
| --- | --- | --- | --- | --- |
| 1 | 5 s | 3 s | 15 s | 10 s |
| 2 | 20 s | 20 s | 3 min | 2 min |
| 3 | 1 min | 90 s | 30 min | 15 min |
| 4 | 5 min | 10 min | 2 hours | 1 hour |

Definition-specific overrides are allowed within four active tiers; region number is not a fifth-plus timer tier. Seed times are recipe overrides. At active tier four/mastery ten, a single wearable takes 3 hours 48 minutes, a tool about 174 seconds, and food about 349 seconds. The slowest listed x1000 Food batch stays below 30 minutes at mastery one. Recipes within a six-region block can use different authored times without another species or real-life processing subchain.

Validate `maximumSingleDuration * maximumSupportedBatchFactor < 3600 seconds` for Tool/Food recipes, using factor 1 for Tool (x1 only) and approximately 2.993 for Food (x1000 supported). Reject invalid definitions rather than allowing an hour-plus Food/Tool timer or clamping many mastery levels to the same time. Wearables can take hours for one item, but cannot be bulk crafted. Stackable Misc batches may take longer than single crafts, including multiple hours; preview the actual completion time before consuming materials. Queue waiting time is separate from the item's own timer; the eight outstanding craft slots run independently until ready.

Examples to verify:

- 3 pine branches -> 1 pine log: instant, no mastery, furniture-only output quality.
- 3 pine logs -> 1 pine plank: fixed authored material time, no mastery, furniture-only output quality.
- 1,000 executions of the log conversion: 3,000 branches consumed, 1,000 logs produced instantly; no XP/level counter.
- 1,000 branches: at most 333 log conversions, producing 333 logs with one branch left; no rounding up to a full stack of crafted output.
- Level-five and level-ten versions of the same wearable: both baseline Q5, level ten has higher price/durability and a longer timer.
- Level-five and level-ten versions of the same Food recipe: both baseline Q5, level ten has higher price and a shorter timer, with no durability.
- Adding craft-quality or equipment-life furniture changes the appropriate output stats and price but leaves the predicted timer identical.

## 11. Missions and unlock dependencies

**Confirmed:** four independent ordered branches: **Gatherer, Lumberjack, Hunter, Explorer**. Only one mission is active globally. Each branch keeps its own next mission/tier; after a camp claim, choose any eligible branch's next mission. Explorer covers exploration, fibers, fishing-related unlocks, and its clothing role, not another free-roaming movement system. The new branch examples replace the earlier strict Materials -> Tools -> Food -> Clothing -> Misc phase gate.

The standing NPC appears at the starting camp and at camp visits with a claimable completion. It does not occupy a log seat. The first mission can be accepted before departure. Claim grants the specified recipe once, persists the reward, then offers the next eligible missions.

### 11.1 First-tier branches and clothing assignments

Preserve the new examples as twelve specific missions. Generic words in `Trail.md` are resolved to exact item IDs below; no interchangeable input rule is introduced. Each requirement is **10 units**, or ten individual instances for tools. All rewards are recipe unlocks, not free crafted outputs.

| Branch / clothing slot | Mission 1: consume -> recipe reward | Mission 2: consume -> recipe reward | Mission 3: consume -> recipe reward |
| --- | --- | --- | --- |
| Gatherer / hat | 10 Pine branches -> Pine log | 10 Apples -> Apple juice | 10 Pine bird sculptures -> Tier-one hat |
| Lumberjack / boots | 10 Flint stones -> Normal pine axe | 10 Pine logs -> Pine bird sculpture | 10 Normal pine axes -> Tier-one boots |
| Hunter / shirt | 10 Rabbit pelts -> Normal pine slingshot | 10 Normal pine slingshots -> Rabbit toy | 10 Rabbit toys -> Tier-one shirt |
| Explorer / pants | 10 Hemp fiber -> Hemp cloth | 10 Raw rabbit meat -> Roasted rabbit | 10 Hemp cloth -> Tier-one pants |

These four clothing rewards complete one set's hat/boots/shirt/pants; backpack comes in a later mission. Branch-to-slot assignments remain consistent in later tiers. Switching branches resumes its saved mission index; it does not create a new category-phase gate or reset another branch. Optional second clothing sets can add variants within a tier, with independent recipes/stats and the existing caps; they do not add a fifth resource tier. Do not make two full sets a prerequisite for the first prototype.

### 11.2 Make the starter graph achievable

Several earlier seed recipes depended on advanced materials unlocked later. **Recommended recipe revisions for the first set**, preserving exact IDs and the new mission order:

| Early output | Fixed ingredients -> output | Reason |
| --- | --- | --- |
| Normal pine axe | 2 Pine branches + 2 Flint -> 1 | Lumberjack L1 can supply its own recipe without a locked cord recipe |
| Normal pine slingshot | 2 Pine branches + 1 Rabbit pelt -> 1 | Hunter H1 bootstraps from the two shared loose materials |
| Pine bird sculpture | 2 Pine logs -> 1 | L2 unlocks it before the later plank recipe; G1 already unlocks logs |
| Rabbit toy | 2 Rabbit pelts + 1 Hemp fiber -> 1 | H2 does not depend on a later fine-pelt/stuffing/thread chain |
| Tier-one hat | 2 Rabbit pelts -> 1 | First clothing reward is usable without advanced curing recipes |
| Tier-one boots | 2 Rabbit pelts + 1 Hemp fiber -> 1 | No locked leather/cord processing prerequisite |
| Tier-one shirt | 3 Hemp cloth + 1 Rabbit pelt -> 1 | Explorer E1 supplies the exact cloth recipe |
| Tier-one pants | 3 Hemp cloth + 1 Pine branch -> 1 | No locked cord or interaction-only bone requirement |
| Roasted rabbit | 1 Raw rabbit meat + 1 Pine branch -> 1 | Early Explorer food uses tier-one fuel, no salt prerequisite |

These replace the corresponding older first-tier seed recipes in section 9, not alternate ingredient choices within the same recipe. Later refined variants get new recipe IDs.

Important cross-branch/source edges:

- G1/G2 can finish in **region one**, whose loose resources are branches/pelts/apples. Accept G1 at entry, hand it in at the next camp, accept G2, and turn in apples at another early camp to unlock juice. Guarantee apples before that turn-in; retain enough after the ten-unit mission contribution to craft. No tool or shop purchase is needed.
- Region two introduces Flint and Hemp fiber, enabling Lumberjack L1 and Explorer E1. Do not pretend those sources are in region one or require a random camp shop for them.
- Gatherer G3 requires the Pine bird recipe from Lumberjack L2, which itself needs Pine logs from G1 or a tree interaction. Show G3 as locked until the recipe/source dependency is satisfied, and offer other branches instead of activating an impossible mission.
- Hunter H2 consumes **ten x1-crafted slingshots**, including carried tools if present; there is no gear bulk crafting or last-tool protection. The player can craft an additional spare after contribution when needed. H3 uses the toy recipe from H2 and region-two Hemp fiber.
- Explorer E2 requires raw rabbit meat from hunting, not a loose gathering slot. Hunter H1 supplies the slingshot recipe first. Region-one rabbits exist as interactables even before the player can use them. E3 requires craft-ready Hemp cloth from E1, then awards pants.
- Four branches can be interleaved, but only one mission is active. Completion requests a camp claim before the next acceptance. Taking tools for a mission never reverses its known recipe or confiscates the worn outfit.

### 11.3 Later missions, regional availability, and source validation

After the first clothing rewards, extend each tier with plank/refined pelt/leather/cloth, backpacks, food, sale Misc, and fishing. Proposed tier-one extensions:

- Gatherer G4: 10 Pine logs -> Pine plank recipe; G5: 10 Reeds -> Reed braid (region six).
- Lumberjack L4: 10 Pine planks -> Pine beam, with the cord recipe available through Explorer; L5: 10 Sand -> Mortar (region six).
- Hunter H4: 10 Rabbit pelts -> Rabbit leather; then pelt/refinement and toy variants as needed.
- Explorer E4: 10 Hemp fiber -> Hemp cord; E5: 10 Pine branches -> Normal pine rod, after cord/bone access; E6: 10 Rainbow trout -> Herb trout after region-four Tea herbs are reachable; E7: 10 Hemp cloth -> starter backpack after Hunter's Rabbit leather unlock.

These are proposed additions after the twelve specified starters. Never require fish before a rod recipe/source exists. Validate the reward recipe's craftability as well as the mission's requested item. The graph permits interleaving branches; it does not reinstate the old universal category sequence.

From tier two, spread Normal/Elite axe/hunting/rod unlocks and up to two clothing sets across the six-region block. Keep Gatherer hats, Lumberjack boots, Hunter shirts, and Explorer pants; assign backpack and advanced materials explicitly to one branch. A second set may improve durability or presentation while respecting ten pockets/hooks, twelve hearts, and brisk-walk caps. Use the 18-item featured source table and interaction-only drops to schedule mission availability; never request a region-14 material while region 14 is undiscovered.

Each mission definition stores branch, resource tier, sequence index, exact requested item/quantity, prerequisite mission/recipe IDs, earliest accessible source region, and recipe reward ID. Tier three's hunting-shared ingredient is Turkey feathers, not a nonexistent pelt. Shops are optional supplements; Q5 store stock is not a mandatory shortcut around an inaccessible source/recipe. No mission references a deferred animal/wood tier.

Keep the specified first twelve mission counts at ten. Extra mechanics tests may use 999/1,000/1,001/10,000-unit requirements, separately from authored campaign missions. Tool/clothing requests count unique instances; stackable requests consume only necessary units. A mission asking for a crafted product is offered only when its recipe can be known and the required inputs have achievable sources. If dependencies block one branch, keep another eligible branch visible rather than forcing acceptance and requiring cancellation.

### 11.4 Collection quantities from ten to 150

**Confirmed endpoints:** lowest-tier missions require ten items, and highest-tier requirements rise up to **150**. Counts are units/unique instances, never full stacks. Proposed default schedule:

| Active resource tier | Region block | Default mission quantity | Status |
| --- | --- | --- | --- |
| 1 | 1-6 | **10** | Confirmed lowest-tier count; retain the twelve starter missions |
| 2 | 7-12 | **50** | Proposed intermediate value |
| 3 | 13-18 | **100** | Proposed intermediate value |
| 4 | 19-24 | **150 maximum** | Confirmed ceiling; exact mission may be lower when its authored cost warrants it |

Apply the default by resource tier, not region number or recipe mastery. Each mission still stores its explicit quantity; any lower override for an expensive high-tier craft should be an authored balancing choice, not a silent runtime discount. Do not exceed 150 in the campaign. Boundary-only mechanics-test missions can exceed it but are not normal progression. Warehouse boxes remain ten units and are unaffected.

Keep tool-instance missions honest: a request for 150 tools means 150 finished unique tools through x1 crafts, not 150 tool uses, 150 partial durability units, or a newly introduced bulk recipe. Requests must name Normal or Elite IDs explicitly. A Normal-tool request cannot be filled by an Elite, and the Normal consumed in an upgrade cannot also count toward the active mission.

Budget **recursive recipe costs and competing sinks**, not just the last recipe screen. For example, 150 Elite axe upgrades at the proposed two-plank cost consume **150 same-tier Normal axes + 300 previous-tier planks**. Through the 3-log/3-branch conversions, 300 planks equal 900 logs or 2,700 branches if all wood is converted from branches, before counting the Normal tools' own ingredients. Direct tree logs provide another acquisition path. Home upgrades, materials submitted to missions, other equipment, and expected tool wear must remain in the same ledger.

During mechanics tests, 1,000-unit pickups remain the default and make these quotas inexpensive; **do not mistake that for a validated final economy**. Preserve a separate balance-analysis preset/report that models future lower pickup quantities and actual harvesting rates without changing the live full-stack test profile. Measure gathering actions/travel, normal-tool cost, previous-tier refinement cost, mission units consumed, available upgrade stock, house reserves, craft queue time, and failed/used-tool replacement. At 150 gear instances, x1-only interaction/timer workload is also a pacing cost; flag it for the later balance review rather than adding bulk gear crafting behind the user's rule.

Unlock previous-tier refinement recipes before a new tier's Elite missions, and unlock its Normal tool before its Elite recipe. Avoid demanding an Elite as the reward-unlock mission's own required item. Prefer early current-tier raw/Normal requests to unlock Elite recipes, then later Elite-output requests when the chain is already usable. The four branch/clothing assignments and exact ingredient rule stay unchanged.

### 11.5 Collection and contribution rules

Collection rules:

- Automatically consume eligible carried items for the active mission, including on mission acceptance and after acquisition/transfer. No separate per-item Contribute confirmation is needed. Show the requested quantities and eligible carried tools/spare gear in the mission preview so the player can plan. Existing chest contents are never scanned and removed automatically.
- Moving an item from a chest into personal inventory counts as the requested acquisition opportunity. Repeated moves cannot count the same units because contributed units are consumed.
- Consume only the remaining requirement, not an entire full stack: a mission needing 10 units reduces 1,000 to **990**. Already-completed requirement types consume nothing further. Emit a quantity/instance receipt.
- Tools are not equipped: eligible tools in the backpack or on pants hooks can be consumed directly, including the last usable tool of that family. Do not silently spare them, manufacture replacements, or exempt a tool because it was the preferred harvesting choice. Missing-tool interactions then remain unavailable until another eligible tool is obtained.
- Spare hats, boots, shirts, pants, and backpacks in carried inventory are eligible instances. **Currently worn clothing and the currently equipped backpack are protected**: never automatically unequip or consume them. Once replaced and stored as spares, they can become eligible.
- Consume only the requested number of matching unique instances and retain all others. **Confirmed:** match only exact item ID and remaining quantity; no quality, mastery, or durability threshold. Tier/variant matter only because they have distinct item IDs. Pending crafts and unavailable/committed items remain ineligible; worn protection remains. Recommendation: choose the lowest quoted value among matching exact-ID items first and show its identity in the receipt; add a selection override later only if testing demonstrates a need.
- Pending craft outputs, unclaimed reward trays, chest/shelf contents, trade escrow, and already committed transaction quantities are not carried contributions. Claimed ready tools/gear become eligible immediately. Serialize competing actions so one instance cannot also be shot, sold, eaten, or reserved in the same transaction; a short per-action lease must not become permanent mission protection for a carried tool.
- Crafted-item missions count ready outputs when claimed, not merely when the timer starts.
- Completed progress is monotonic; the NPC reward is idempotent across reloads.
- Hide the HUD counter in settlement while still processing eligible contributions and show a completion notification.
- Never choose an inaccessible future-region mission by ordinary progression. The mystery-box exception for a current mission does not justify making it mandatory to buy gold boxes.

Every unlock needs a path from available resources/recipes. Build an editor dependency report for cycles, missing sources, disabled-tier references, furniture outputs, and excessive ingredients (warn above three distinct ingredients, reject above nine). Check that all eight house levels are achievable with the four active tiers, and that the first home after region four does not require tier-two-plus materials.

## 12. Camps, trading, currency, and travel

### 12.1 Camps and conveyor trading

Each camp has a campfire and four log seats: one player and three NPC traders. The mission NPC stands separately. The 15-second trading session is one opportunity per camp visit. Leaving and returning creates a new opportunity as requested; restarting the app during the same visit does not.

NPCs each offer 3-5 persisted seeded entries from discovered-region resources, tools, and clothing. A stackable offer transfers one unit, gear one instance. **Confirmed:** apply position-based NPC discounts; starting curve `buyMultiplier = 1.1 - 0.3 * normalizedBeltPosition` gives 110% at entry and 80% before the shredder. Quote from the item's actual profile, then round only at payment. Persist item quality/profile and price curve through reopen/reload.

NPC-generated tools/wearables/backpacks are always **creation recipe level one**, with seeded random Q1-Q5 (proposed uniform distribution), full condition, and zero furniture bonuses. Compute level-one base price/durability and the actual quality premium once; do not derive their quality from the player's mastery or furniture. Bought NPC gear grants no recipe/mastery. Player-owned conveyor listings retain their real creation level/quality/durability/profile. Ordinary settlement/balloon shop purchases remain Q5: random quality is the NPC conveyor exception.

Player listings should let the player select quantity from a stack. Selling all 1,000 is allowed, with an explicit quantity and total shown; never accidentally interpret a unit price as a full-stack price. Persist the selected quantity in trade escrow so it cannot also be crafted, contributed to a mission, or put in a box. Listing selection is part of the trading UI, not access to the general inventory/equipment menus.

**Confirmed trading lock:** the conveyor session lasts 15 seconds. During it, only trading actions and the dedicated Return/cancel button are available. Do not allow crafting, eating/drinking, equipment changes, mission interaction, harvesting, maps, settlement travel, or general inventory/settings panels. There is no detail-overlay pause or user-controlled timer extension. Trade-specific item details must stay nonblocking and keep the timer running.

Settlement rules:

- NPC purchases of player listings: retain the recommended 100% quoted sale price for the listed quantity.
- Player listing reaching the shredder naturally: retain the 80% fallback from the original design. NPC-owned items give the player nothing when shredded.
- Natural 15-second expiry: recommend settling remaining unsold player listings at 80%, then mark the visit session finished. This end-of-session fallback is a recommendation; it is separate from the confirmed cancellation penalty.
- **Return cancellation at any point before expiry:** stop the animation, immediately sell every still-unsold player-owned listing on the conveyor for **70% of its quoted undiscounted sale value**, remove those listings, and return to normal camp UI. Quantities never listed remain untouched. Already sold/shredded listings keep their settled result; do not pay again or reprice them. NPC-owned listings produce no player cash.
- Cancellation is a terminal result, just like natural completion. That camp visit cannot trade again until the player leaves and returns, or reaches another camp. Closing the trade panel, reopening menus, returning from a home shortcut without ending the camp visit, or restarting the app cannot reset it.

An actual completed departure to settlement or another trail location ends the camp visit; returning creates a new visit as required. Merely opening a travel menu or canceling a transition does not count as departure. Return in the trading screen ends the trade and returns to camp UI; it does not itself leave the camp or refresh the opportunity.

Persist `TradeSessionState` (NotStarted/Active/Completed/Canceled), remaining duration, ownership, and each listing's settlement ID/reason. Commit cancel payouts and the Canceled state together before leaving the screen. Serialize expiry, NPC purchases, shredder events, and Return so an item settles once even when they occur in the same frame. If the 15 seconds have already elapsed, finish the natural-expiry path; a late Return cannot settle it again.

Cancellation example: one player listing worth 100 cash is still unsold, another worth 200 has already sold, and an NPC listing is still on the belt. Return adds 70 cash for the unsold player listing only. It neither pays the NPC listing nor pays the already-sold listing again. The trading opportunity is spent even if the player had no unsold listings to settle.

**Confirmed:** camp stamina regeneration continues throughout the animation, including while placing/buying listings. With the proposed rate, 15 active seconds gives 0.75 heart; canceling after five gives 0.25 heart. Input exclusivity does not pause regeneration or background craft readiness. Defer unrelated notifications/popups until trading ends. Recommendation for OS suspension: freeze/persist the remaining trading time and foreground regeneration, then resume the same session on return; never grant a new 15-second opportunity.

### 12.2 Currency definitions and arithmetic

Use two integer wallets with distinct icons and IDs:

- `Cash`: camp trades, river tolls, basic shops, home upgrades/furniture where specified.
- `Gold`: settlement lumber-tier unlocks, balloon stock, premium bakery goods, selected long-term unlocks.
- `GoldOre` and `GoldIngot`: item definitions; never mutate the Gold wallet when smelting.

Store currency as integer minor units and 64-bit balances/totals. **Every price below is per item unit, not per stack.** Preserve fractional intermediate unit values in fixed-point form and round once at the final transaction, not once per modifier/unit. Recommendation: show whole-cash totals rounded down; the cheapest ordinary Q1 material unit is one cash. Snapshot intrinsic value at craft creation, never recalculate an old item's price from the player's latest recipe level/furniture.

### 12.2.1 Price anchors, category order, and scope

**Confirmed base-price order:** Misc (sculptures/toys/jewelry) > Backpacks > Clothing > Tools > Advanced materials > Food > Other materials/ingredients > Basics. Backpacks are Wearable recipes for timer/durability rules but have their own higher price category. Raw milk/eggs/fiber/ore can be Other ingredients; prepared cheese/meals/drinks are Food; refined cloth/leather/beams/glass containers can be Advanced materials. A recipe's timing role and its price category are separate explicit fields.

**Confirmed anchors:** one cash for the simplest tier-one Q1 material, and 6,000 cash as the level-ten Misc reference's Q1-equivalent base, before finished quality/durability. A level-ten nonmaterial craft normally has at least Q5, so Q1-equivalent means a calculation baseline, not a naturally obtainable level-ten Q1 product. The final price can exceed 6,000. Misc has no durability premium.

Recommended distribution: place the 6,000 reference in active tier four and scale earlier-tier reference goods below it. This tier placement/spacing is a proposal, not a requirement that every Misc recipe have the same price. Six regions per resource tier do not introduce extra price tiers; per-recipe values can vary within that block.

| Price category | Tier 1 reference | Tier 2 reference | Tier 3 reference | Tier 4 reference | Mastery interpretation |
| --- | --- | --- | --- | --- | --- |
| Misc | 600 | 1,800 | 3,600 | **6,000** | Q1-equivalent reference at recipe level 10 |
| Backpack | 350 | 1,050 | 2,100 | 3,500 | Q1-equivalent reference at recipe level 10 |
| Clothing | 220 | 660 | 1,320 | 2,200 | Q1-equivalent reference at recipe level 10 |
| Tool | 140 | 420 | 840 | 1,400 | Q1-equivalent reference at recipe level 10 |
| Advanced material | 100 | 300 | 600 | 1,000 | Fixed material reference, **no recipe levels** |
| Prepared food/drink | 60 | 180 | 360 | 600 | Q1-equivalent reference at recipe level 10 |
| Other material/ingredient | 20 | 60 | 120 | 200 | Fixed material/ingredient reference |
| Simplest basic material | **1** | 3 | 6 | 10 | Fixed raw-material reference |

These are category reference values, not a command to make every material cost 100 or every meal cost 60. Each exact item definition gets its own authored base, retaining the intended order for comparable-tier reference goods. Example basics: tier-one pine branch/nettle fiber/ordinary pebble can each start at one cash; pine log can be three and pine plank nine, reflecting the 3:1 conversions. Fine cloth/beams/containers occupy the advanced band rather than forcing every processed material into it. Author recipe economics together with their exact inputs and flag unintentional craft-value losses in reports; do not change ingredient identities or full-stack pickups to hide them.

Compare the hierarchy at equivalent tier and base conditions. Higher-tier goods and quality/durability bonuses can overlap lower-tier categories; do not globally clamp all tools below the cheapest clothing or all final prices below 6,000. The proposed references also keep equally maxed tier-four Misc above a backpack with the current Q5/+20% durability test bonuses.

### 12.2.2 Base price by recipe level

For a nonmaterial recipe, author its `PriceReferenceAtLevel10`, then calculate:

`Pbase(L) = PriceReferenceAtLevel10 * (L + 35) / 45`, for levels 1-10.

Thus level one has 80% of its level-ten reference, and every mastery step increases the Q1-equivalent base through level ten. Keep exact rational/fixed-point arithmetic internally. Materials/ingredients have a fixed `MaterialBasePrice`; they do not gain levels merely to fit this price table. The percentages remain editable test data, separate from the confirmed 6,000/one-cash anchors and category order.

### 12.2.3 Finished-quality and durability premiums

Proposed finished-quality price multipliers:

| Finished quality | Intrinsic-price multiplier |
| --- | --- |
| Q1 | 1.00 |
| Q2 | 1.15 |
| Q3 | 1.35 |
| Q4 | 1.60 |
| Q5 | 2.00 |

1. Compute finished Q from recipe mastery plus craft-quality furniture, capped at five, as section 6.1 defines. Material quality remains furniture-only for crafted material output.
2. Apply the price premium once: `Pquality = Pbase * qualityPriceMultiplier(Q)`.
3. For finite crafted tools, clothing, and backpacks, apply a durability-value multiplier: `1 + qualityDurabilityBonus(Q) + U`. This equals the extra crafted durability relative to the **level-adjusted base**, using unrounded stat ratios. `Pintrinsic = Pquality * (1 + qualityDurabilityBonus(Q) + U)`.
4. Misc/Food/materials/ingredients have no durability, so `Pintrinsic = Pquality`; do not give them fictional durability or the tool/wearable premium.
5. Persist intrinsic unit value, grade, recipe level, base price, and the exact modifier breakdown in the output stat profile.

Quality has two intentional effects on durable crafts: higher quality raises intrinsic value, and the resulting extra lifespan also adds value. Do not accidentally add the **same** effect twice: no direct `+5% per C` price layer, no second U multiplier, no second quality-price multiplier, and no multiplication by `Dfinal / D0` that repeats level-based durability growth already reflected by the base level curve. Craft-quality furniture acts through finished Q; once Q5 is reached it grants no hidden extra price tier. Matching durability furniture still increases both lifespan and its durability price premium.

Normal/Elite tools keep authored base durability/price references; an Elite reference can reflect longer life, action savings, and drop ability. Do not invent another automatic Elite multiplier. Infinite-durability merchant gear uses its authored price. This defines new-item value; any used-gear condition discount is separate and never mutates the original craft profile. Recipe economics also include explicitly listed wood fuel per serving/batch, not just meat/fish; global cooking status does not make an ingredient free.

Worked examples before selling-location modifiers:

| Item | Q1-equivalent base | Finished Q | Durability price factor | Intrinsic unit value |
| --- | --- | --- | --- | --- |
| Basic tier-one material | 1 | Q1 | None | **1 cash** |
| Level-ten tier-four Misc reference | 6,000 | Q5 (2.00) | None | **12,000 cash** |
| Level-ten tier-four backpack reference | 3,500 | Q5 (2.00) | 1 + 0.40 + 0.20 = 1.60 | **11,200 cash** |
| Level-ten tier-four clothing reference | 2,200 | Q5 (2.00) | 1.60 | **7,040 cash** |
| Level-ten tier-four tool reference | 1,400 | Q5 (2.00) | 1.60 | **4,480 cash** |
| Advanced material, fixed base | 100 | Q3 (1.35) | None | **135 cash** |
| Level-ten tier-four food reference | 600 | Q5 (2.00) | None | **1,200 cash** |

### 12.2.4 Selling, stacks, and test-phase inflation

For ordinary sales, apply sale-price furniture only to eligible player-crafted goods: `quotedSaleTotal = sum(conditionAdjustedIntrinsicValue) * (1 + eligibleSaleBonus)`. Non-player-crafted goods use zero bonus. Then apply the camp purchase/80% fallback/70% cancellation factor once. Warehouse uses `sum(conditionAdjustedIntrinsicValue)` at 100%, with **zero** sale bonus and its separate direct 3% gold cycle. Preserve fixed-point precision and round cash only at payout; record actual paid cash as the gold-cycle basis. Retain creation/quality/durability premiums already in the profile without recalculating them. Selling never grants mastery.

A full 1,000-unit Q5 Misc stack at the 6,000 base reference is worth **12,000,000 cash** before sale buffs, not 6,000 cash for the entire stack. A full stack of one-cash basics is worth 1,000. These large totals are deliberate consequences of confirmed mechanics testing. Keep 1,000-unit pickups unchanged, support 64-bit totals and large ledger/UI displays, and report extreme money/gold throughput for later balance review. Gold shop prices remain separately authored; gold ore/ingots never directly become currency.

Store offers must quote their **Q5 product** from the outset. Any authored markup or fixed gold price is separate from intrinsic resale value. Do not display a Q1 price and secretly apply Q5 after purchase, or multiply an already-Q5 store profile by the Q5 factor a second time. With the proposed factor, a basic material with one-cash Q1 base has Q5 intrinsic value two cash before store markup; the one-cash base anchor remains unchanged for Q1 goods from other sources. Store quality does not alter full-stack quantities or grant mastery. Camp-trade listings are priced from their own actual quality/profile and do not become Q5 on transfer.

**Confirmed warehouse settlement:** every completed ten-unit box pays **100% of the deposited items' cash sale value**, with no furniture sale bonus, category bonus, extra payout, or cash-to-gold exchange denominator. Track the exact cash paid for each ten-box cycle. On the tenth completed box grant `gold = floor((3 * cycleCash + goldRemainderNumerator) / 100)`; retain the numerator remainder modulo 100 and reset the cycle cash/count atomically. The 3% is the required gold payout, not another bonus layered on it. Example: ten boxes paying 10,000 cash produce **300 gold**, not three. Fractional carry prevents repeated rounding loss; no additional reward or mastery is granted.

Do not rebalance away the large rewards caused by full-stack testing. Add transaction logs and reports for purchases, crafts, wear cost, box sales, gold conversion, and repeatable route income. Later compare net outcomes to detect shop-buy/warehouse-sell or shredding loops that generate risk-free value. Repeated free rerolls and ferry/map shortcuts should be measured before changing the required behavior.

### 12.3 Rivers, maps, and fast travel

- Region borders end at a camp, with a river, ferry boat, and toll NPC just outside it.
- Pay each border toll once. Persist the unlock before changing scenes; insufficient cash leaves the player at the camp with options to trade or revisit discovered locations.
- On the settlement map, show all discovered regions and the next undiscovered silhouette. Only discovered regions are travel destinations; arrive at that region's first camp.
- **Confirmed camp-map layout:** show the current region's name/number, current camp marker, and its segments/camps, with previously unlocked destinations selectable and locked ones visibly disabled. **Below the first camp of that region's section**, list all other previously discovered regions as region destinations. This list comes from discovery history, including a later-numbered region discovered before returning to an earlier one; do not filter only by lower region number. Keep region entries distinct from camp buttons. Recommend free map travel during mechanics testing.
- **Same-region selection:** arrive at the exact selected unlocked camp, including a later camp already visited before backtracking. Segment selection uses its authored unlocked entry camp; it does not spawn the player partway along the path or unlock the next camp. Selecting the current camp is a no-op, with no new visit/trading opportunity or stock reroll.
- **Cross-region selection:** arrive at the selected discovered region's **first camp**, never its last visited camp or arbitrary segment. Refresh the map to that region and its own unlocked camps, with the other discovered regions below its first camp. Example: region 12 camp 8 -> region 12 camp 3 arrives at camp 3; region 12 -> discovered region 7 arrives at region 7 camp 1, even if camp 6 was previously visited.
- Validate the region and exact camp unlock before committing a travel transaction; region discovery alone does not unlock all camps. Persist destination/arrival anchor and transition ID; interrupted loads resume that destination once. Update current region/camp and `lastTrailCampId` atomically on arrival. Travel is unavailable during exclusive trading or faint recovery; it spends no walking stamina/durability, preserves pending food recovery, and never bypasses an unpaid border.
- Traveling backward by map does not permit backward walking along a segment and does not bypass a locked river border.
- Record `lastTrailCampId` separately from the player's home/settlement view. Return to Trail uses that camp.
- Home/warehouse/lumber shortcuts are unavailable until settlement is discovered. In a camp, they may travel to settlement; on the trail between camps they are hidden.
- Preserve current inventory, active craft batches, mission state, paid tolls, and shop/camp visit state through all travel paths.

### 12.4 Context-sensitive UI

| Context | Available actions |
| --- | --- |
| Between camps | Avatar vertical speed gestures, independent horizontal 360-degree camera orbit, nearby resources including behind the player, harvest, inventory, contextual Eat, pause/settings |
| Camp, not trading | Inventory, crafting, equipment, regional map, settings; settlement shortcuts after discovery; stamina regenerates |
| Settlement street | Inventory, equipment, world map, settings, Home/Warehouse/Lumber/Return to Trail; no crafting |
| Inside home | Settlement actions plus crafting, furniture store, house upgrades, storage, pets after level three |
| Settlement shop | Context UI owns input; prevent click-through to other buildings; stamina regenerates |
| Active camp trade | Trading/listing actions and Return/cancel only for 15 seconds; all other actions locked; stamina keeps regenerating |

Retain the two expandable action menus beside the inventory button. The lower menu expands upward for Craft/Equipment/Map/Settings; the upper expands downward for Home/Warehouse/Lumber/Return. Disable unavailable actions with a short reason where discoverability helps; hide Return outside settlement. Prototype portrait first as a recommendation because vertical resource swipes and the lower backpack aperture fit that composition; validate the full inventory/crafting layout on a physical phone before fixing orientation. This replaces the earlier landscape-first recommendation.

## 13. Settlement, house, furniture, storage, and pets

The settlement is a side-scrolling selection scene; there is no direct character movement. Buildings are selectable destinations. Neighbor houses use seeded visual variants and house levels but have no simulated economy or real owners initially.

### 13.1 House progression and upgrade deposits

Keep eight **house levels**, distinct from four resource tiers: two house levels use each active tier's materials. The first two grow horizontally/slightly vertically; after level three, keep width fixed and grow vertically. Use a scrollable interior. Settlement after region four remains within tier one, so the first home/upgrade uses pine/early materials only.

| Level | Visual stage | Interior grid | Example crafted upgrade cost to reach it |
| --- | --- | --- | --- |
| 1 | Small shed | 8 x 5 | Tier 1: welcome quest; 8 pine planks + 4 fired bricks |
| 2 | Extended shed | 10 x 6 | Tier 1: 10 pine beams + 12 bricks + 8 hemp cloth + 4 reed braids |
| 3 | Proper wooden home; pets unlock | 12 x 7 | Tier 2: 14 birch beams + 16 limestone tiles + 10 fine beaver leather + 4 glass panels + 6 mortar |
| 4 | Loft | 12 x 9 | Tier 2: 18 birch panels + 20 sandstone tiles + 12 fine hemp cloth + 12 copper fittings |
| 5 | Two stories | 12 x 11 | Tier 3: 22 mesquite beams + 24 granite tiles + 14 cotton cloth + 16 iron fittings |
| 6 | Expanded upper rooms | 12 x 13 | Tier 3: 24 mesquite panels + 24 slate tiles + 16 fine cotton cloth + 18 glass panels |
| 7 | Frontier manor | 12 x 15 | Tier 4: 28 oak beams + 28 polished basalt + 18 fine boar leather + 20 glass panels |
| 8 | Finished homestead | 12 x 17 | Tier 4: 32 oak panels + 24 marble tiles + 12 cut quartz + 24 steel fittings |

Counts are deliberately small test data. House levels 3-4 need tier two (region seven onward), levels 5-6 tier three (region thirteen onward), levels 7-8 tier four (region nineteen onward), plus relevant material-unlock missions on the new branch graph. No deferred species/global Materials phase is required. Reed braid/Mortar make level-two/three upgrades useful sinks for their region-six sources; level-one home construction remains possible at region-four settlement arrival. House upgrades unlock store listings, not free crafted oven/window/chest outputs.

The upgrade menu sits beside the furniture-store button and shows current level, next level, deposited/required units, and missing sources. Deposits transfer materials into house-upgrade escrow immediately; cancellation returns them through a tray. Upgrade completion, cost consumption, grid growth, and unlocks commit together. Use a migration rule if costs change after a save already contains deposits.

### 13.2 Furniture and buff distribution

There are ten required buff categories: crafted quality, food recovery, sale price, boots life, shirts life, hats life, pants life, axe life, hunting-tool life, and rod life. Add backpack life as a useful eleventh category and storage capacity as the twelfth allocation. Four cycles of twelve entries give **48 furniture definitions, six at each of eight levels**. Thus every category gets four opportunities with an even distribution.

Each row below unlocks exactly six types. C = crafted quality, F = food recovery, S = sale price, B = boots, Sh = shirt, H = hat, P = pants, A = axe, Rg = ranged hunting tool, Rd = rod, Bg = backpack, St = storage. Equipment buffs affect newly crafted items, not existing gear. Food buffs apply at consumption, sale buffs at sale, and craft-quality/equipment-life buffs are snapshotted at craft start. Craft-quality effects are deterministic and precede equipment-specific durability effects; neither changes the timer.

| House level | Six unlocked furniture types and effects |
| --- | --- |
| 1 | Worktable (C), bedroll (F), market sign (S), boot bench (B), sewing basket (Sh), hat peg (H) |
| 2 | Trouser rack (P), chopping block (A), sling stand (Rg), rod rack (Rd), bag frame (Bg), pine chest (St) |
| 3 | Loom (C), clay cookstove (F), display counter (S), sofa (B), sewing desk (Sh), hat cabinet (H) |
| 4 | Tailor's stool (P), whetstone stand (A), bow workbench (Rg), tackle desk (Rd), leather table (Bg), birch chest (St) |
| 5 | Artisan bench (C), pantry dresser (F), trophy cabinet (S), boot cabinet (B), cloth press (Sh), hat display (H) |
| 6 | Clothing armoire (P), tool forge (A), bow cabinet (Rg), reel workshop (Rd), pack-maker bench (Bg), mesquite chest (St) |
| 7 | Master loom (C), dining table (F), gallery pedestal (S), travel sofa (B), fine-cloth workstation (Sh), milliner bench (H) |
| 8 | Frontier wardrobe (P), master tool station (A), hunter's cabinet (Rg), angler's cabinet (Rd), expedition workbench (Bg), reinforced oak chest (St) |

Recommended increments per first purchase: equipment life +5%, food recovery +5%, sale price +2%; four purchases give +20% life/recovery and +8% sale value. Craft-quality strength: worktable +1, loom +1, artisan bench +1, master loom +2, capped at `C = 5`. It raises finished Q (max Q5), which determines the quality-price premium and product-quality durability bonus; it no longer grants direct +5%-per-C price. Matching equipment-life furniture adds its percentage of level-adjusted base durability and contributes once to the durability-price factor. Selling-price furniture applies at sale, not craft. No bonus changes craft timers. Storage entries unlock containers, and food still cannot exceed maximum stamina. Values are proposed tuning data.

**Confirmed:** buff unlocks are granted once per furniture definition on its first purchase and remain when furniture is stored or moved. Duplicates are decorative/storage only. Selling/rebuying, moving, placing, or loading the item never unlocks the buff again. Decorative rugs, lamps, pictures, and the twelve-position shelf can be variants within unlocked types or optional extras; do not accidentally count them as a seventh required type per level.

Footprints: most furniture 1 x 1, 2 x 1, or 2 x 2; largest 3 x 2. No individual piece occupies more than 15% of the level-one interior. Allow furniture to be stored in a catalog when not placed. A production-looking piece grants its listed buff initially; adding workstation minigames would be additional scope.

**Confirmed store-only acquisition:** furniture has a store price/unlock, not an ingredient recipe; purchases never request materials. For example, bricks/glass panels contribute to a house upgrade; that level can unlock a clay cookstove or decorative window, which the player buys. Clay cups/glass bottles are recipe containers, not furniture. Chests/display shelves are bought too. Material drops cannot bypass the store through furniture crafting. Eight furniture-unlock levels do not imply eight resource tiers.

Furniture/store companions are Q5 products under the store rule, but this does not add crafting levels, wear, or a second multiplier to their defined first-purchase buffs. Keep their authored buff strength/capacity and store price; their quality label does not duplicate a permanent unlock. Offline pet rewards are earned output, not purchases, and still use their own reward definitions.

### 13.3 Chests and shelf layout

**Confirmed:** each chest is one continuous spatial container with **horizontal scrolling**, like the backpack; no independent pages, vertical scrolling, or item rotation. Use fixed-height storage with proposed tier sizes 6 x 6, 12 x 6, 24 x 6, and 48 x 6 logical units. The largest remains 288 logical square units, not 288 slots. Free placement can cross any viewport boundary because there are no page boundaries. Save container ID, item coordinates and horizontal scroll offset, not page ownership. Migrate old page layouts into consecutive horizontal strips without losing contents.

Render/virtualize only visible entries plus a small margin; clipping affects views, not item ownership. Use readable touch targets (about 48 dp), screen safe areas, and a transfer drawer or tabs when chest/backpack cannot fit side by side. Edge-drag autoscroll is a proposed convenience; scrolling on empty space and dragging an item have separate gesture ownership. Start with four placed chests and retain searchable offscreen entries.

The display shelf remains three rows of four positions, one ready item per position. Split one unit from a stack, preserve unique metadata, and scale the view without changing inventory footprint.

### 13.4 Settlement businesses

| Building | Mechanics to implement |
| --- | --- |
| Warehouse/trading house | Two ten-unit exact-ID boxes; every ready sellable item, including unique gear; sum actual deposited values; 100% cash, 3% gold every ten boxes; no extra bonuses |
| Lumber field | Two rows, six massive trees per wood tier; gold unlock per tier; independent 72-hour respawn timestamps and persistent pending loot |
| Bakery | Three full-stack offers: one common cash item and two premium gold foods |
| Ranch | Three full-stack cash offers from domestic meat/dairy/animal products; no wild hides/pelts |
| Farm | Three full-stack cash crop offers; region discovery controls availability |
| Mine | Three full-stack cash ore/stone offers; avoid future-region crafting bypass |
| Fishery | Exactly two distinct discovered fish offers + one random eligible exclusive seafood offer; Q5, cash-only, each capped at one 1,000-unit stack per reset |
| Tailor-material shop | Three full-stack cash offers from active-tier hemp/cotton/leather and optional wool; no pending silk/cashmere/hunted-species dependency |

**Confirmed:** warehouse accepts **any ready, owned sellable item**, including materials, food, Misc, tools, clothing and backpacks; recipe knowledge, currencies, defaults, pending products and committed items are not sellable. Worn gear must be unequipped first. Keep two partial boxes, ten units/instances of one exact item ID per box. **Delegated default:** allow mixed quality/mastery/condition within that ID and sum each deposited unit's saved value; one tool counts as one, never as its remaining uses. Item IDs distinguish tier/variant. Proposed used-gear valuation: `cashUnitValue = savedIntrinsicValue * clamp(remainingDurability / maximumDurability, 0, 1)` for finite tools/wearables, full saved value for nondurables/indestructibles. Apply no furniture-sale or extra transaction bonus. Show each deposit's exact cash quote; lock its valuation once deposited. No partial-box payout. Remove ownership into escrow, replace completed boxes, pay cash and advance the ten-box gold cycle exactly once. Begin with one box/action, then bounded bulk deposit; a full test stack can fill 100 boxes without 100 simultaneous animations.

**Confirmed settlement refresh (replaces the 30-minute proposal):** generate stock on settlement entry and refresh all ordinary resource/ingredient shop offers whenever the player actually **leaves the settlement and returns**. No timer, cooldown or daily cap. A new persisted `SettlementVisitId` commits fresh offer IDs/seed and full per-offer stock budgets once on re-entry; the player can immediately buy materials instead of gathering. Moving between home, shops, warehouse or other settlement subviews does not leave settlement. Reopening a resource shop within the same visit keeps its stock/purchases; app suspension/reload or canceled travel also keeps that visit. Show stock and cash/gold prices, no countdown. Pet ownership catalog remains permanent. Balloon shop sessions use their separate close/reopen rule in section 14.

**All six settlement shops deliver Q5:** bakery, ranch, farm, mine, fishery, and tailor materials. Ordinary cash goods and premium gold goods follow the same rule. Item identity/quantity/price may vary by offer, but quality is fixed at five and is persisted through purchase, overflow delivery, and reload. Store-bought Q5 ingredients still cannot replace another material variety in a recipe.

Lumber field scope remains four tiers x six trees = 24 records, pine through oak. Render visible rows only. Section 8.2 defines separate Normal/Elite action curves: Elite is faster at/below its tier and boosts drops on every eligible tree including +1/+2. Apply x50 rewards once and retain 72-hour independent respawn/expiry. Pending species stay deferred; UTC/local-clock limitations remain.

### 13.4.1 Fishery-exclusive seafood and reset stock

**Confirmed:** keep three slots, but reserve **two for ordinary fish and one for an exclusive ingredient**. Randomly select two distinct fish whose source regions have been discovered, then one exclusive from its eligible catalog. Never fill the exclusive slot with a third fish or charge gold for it. The fishery remains a cash shop, unlike the bakery's premium gold slots.

Proposed compact exclusive catalog:

| Exact ingredient | Material tier | Earliest stock eligibility | Initial acquisition source |
| --- | --- | --- | --- |
| Shrimp | 1 | Settlement/fishery unlocked after region four | Fishery-exclusive |
| Crab | 2 | Tier-two region discovery | Fishery-exclusive |
| Squid, proposed addition | 3 | Tier-three region discovery | Fishery-exclusive |
| Lobster | 4 | Tier-four region discovery | Fishery-exclusive |

Select one uniformly from the eligible exclusive pool for initial testing; current-tier weighting can be tuned later. Previously unlocked types remain eligible, so reaching tier four can still show Shrimp/Crab/Squid/Lobster. Current player location never changes the ingredient's MaterialTier. Every offer is Q5, with an authored cash quote using its actual quality profile exactly once.

Each slot starts with **one full stack/1,000 units of stock per reset**, not unlimited repeat purchases. Buying that stack exhausts its slot. If partial purchases are later exposed, all purchases together still cannot exceed its remaining 1,000-unit budget; partial-buy UI is optional, not necessary for the first implementation. Persist selected IDs, quantity remaining, quoted cash price, visit/reset ID, and purchase transactions. Reopening/reloading cannot restock or reroll; only actual settlement departure followed by re-entry restocks it.

Exclusivity is an initial-source restriction: raw seafood does not spawn as loose loot, fish-node catches, tree/hunt rewards, pet rewards, or ordinary mystery-box item rewards. It does not join the sixteen trail fish or the 72 featured loose resources. Players may later trade/sell their purchased goods through normal ownership rules; a procedural NPC should not become another raw seafood supply. Recipe rewards may appear in boxes, **not the exclusive raw ingredient itself**, unless that source rule is explicitly changed later. A mission-item preference in a box must not bypass exclusivity.

These are ingredients for optional collectible cooking recipes, not mandatory inputs to region/clothing/house progression. The fishery can show an ingredient before its recipe is learned; display its recipe-book entries as locked/box-obtainable. Main progression remains achievable through branch food/normal fish recipes. Store Q5 ingredients do not pass their grade into the resulting craft under the established output-quality rule.

### 13.5 Pet roster, pet home, and idle collection

**Confirmed layout:** house level three unlocks the **Pets section of the home store** for buying companions. The **pet home to the left of the home front** opens selection among owned pets, with one active pet; a **separate home-front collection container** holds its idle materials. Buying does not auto-switch the active pet. The store shows species, price, owned state and loot-weight preview, while selection shows the active companion and collection status. These are settlement game companions, with no combat, tool uses, breeding, hunger or wear requirements.

**Proposed first roster:** six distinct `PetDefinitionId`s. Buy each once for a proposed **1,000 cash**, no gold requirement, duplicate purchase or upkeep. All unlock with house level three; show undiscovered specialties as locked loot, not as access to future regions. Equal purchase price/cadence keeps the choice about supplies, not a stronger paid pet. The first implementation can ship Dog and Pig placeholders before adding the other four. Pets are Q5 purchased companions under the shop label policy; that label does not turn idle drops into Q5 or grant furniture buffs. Palette variants are cosmetic and share the same species table.

| Pet ID / species | Wood | Stone/ore | Fibers | Produce/herbs | Seeds | Truffle | Intended specialty |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `pet.dog` — Dog | 45% | 40% | 5% | 5% | 5% | 0% | Building supplies; 85% wood/minerals |
| `pet.cat` — Cat | 10% | 10% | 40% | 25% | 15% | 0% | Textile plants and useful ingredients |
| `pet.rabbit` — Rabbit | 5% | 5% | 20% | 45% | 25% | 0% | Fruit, herbs and seeds |
| `pet.pig` — Pig | 10% | 15% | 5% | 20% | 10% | 40% | Truffle specialist after region 10 |
| `pet.squirrel` — Squirrel | 25% | 10% | 15% | 20% | 30% | 0% | Seeds plus branches |
| `pet.tortoise` — Tortoise | 10% | 60% | 15% | 10% | 5% | 0% | Stone, earth and raw ore |

**Authored supply groups, not literal ecology:** Wood = exact discovered loose branches, never logs/planks. Stone/ore = discovered loose stone/earth/raw ore/gem IDs (e.g. Pebbles, Clay, Copper ore). Fibers = loose plant fibers only (Hemp, Flax, Cotton, etc.), not finished cloth or hunted outputs. Produce/herbs = exact loose Apples, Tea herbs, Barley, Carrots, Pumpkins, etc., excluding Seeds/Truffle. Seeds = exact Flax seeds and Sunflower seeds; no invented generic seed item. Truffle = only `ingredient.truffle`. Build explicit non-overlapping ID lists, with each item's existing source region/tier. Pelt/feather, meat, fish, farm-only dairy/Lavender, processed materials, money, recipes, equipment and fishery-exclusive seafood are excluded from this initial pet pool. Pets provide bonus gathered materials, never new ingredient identities or a source bypass.

The pet store uses a permanent owned/unowned catalog, not random three-slot stock or the settlement resource-shop visit refresh. Purchase and owned-ID registration are atomic. Pets occupy no backpack space, are selected by ID rather than equipped as an item, and are not warehouse goods. An inactive/unowned pet produces nothing.

**Roll order:** snapshot pet ID/weights and discovered-source IDs at the start of each offline interval. For each completed opportunity remove empty/undiscovered groups and renormalize the remaining group weights, then choose within the group: proposed 70% highest discovered eligible resource tier/30% earlier eligible tiers, renormalizing if either tier pool is empty; equal weights across exact IDs inside the selected tier is the initial default. Never multiply group weight by the number of species/items. If no group is eligible, produce no reward and bank no future reroll. Before region 10 Pig cannot find Truffle and its other weights renormalize; discovery never relabels old rewards. Show effective as well as authored percentages so a region-7 pig's 0% effective Truffle chance is clear. Choosing a pet never unlocks a region/material/recipe, and no mission or recipe requires owning it.

**Idle model:** only closed/suspended-app time earns opportunities; online play and menus do not. Starting values: one roll per **30 minutes offline**, at most **eight hours per absence** = 16 rolls, **one material per roll in Release**, with an explicit 1,000-per-roll MechanicsTest preset for container limits. The collection container holds **16 reward entries**, each retaining generated quantity/quality/seed. Do not merge generated entries to evade this cap; claim transfers may merge compatible inventory stacks. Source quality tables determine drops, not pet/shop Q5 or player furniture. These quantities are independent of trail pickups and store/box full stacks.

Persist each opportunity's result once before display. An unclaimed entry remains until collected, with no expiry/replacement. Stop generating when all 16 entries are occupied; missed time/rolls are not banked for immediate claims afterward. Display capacity, contents and capped earnings before closing. Claim individually or Claim All up to available bag space; preserve exact untransferred remainders and allow mission consumption only after a ready material reaches carried inventory. The container is a reward queue, not a general deposit chest.

**Time/switching:** settle the previous offline interval before a purchase, selection, region-source change or claim. The pet home switches only the next opportunity's pet: any saved fractional opportunity retains its original pet/source snapshot until completed; later opportunities use the new choice. Preserve sub-30-minute carry across short absences, but discard time beyond the eight-hour/space cap. With no active pet there is no accrual or catch-up. On resume use `max(0, nowUtc - offlineStartedUtc)`, cap once, and atomically commit generated entry IDs/seeds, partial interval, consumed interval ID and a monotonic last-seen timestamp. Backward clock movement gives zero progress and cannot move the watermark backward; repeated resume cannot replay an interval. Keep unclaimed results through pet changes and reload. Local-clock manipulation remains an offline limitation; use an injected clock for verification.

**Save/acceptance requirements:** save owned pet IDs/purchases, active ID, pet definitions/weight revision, offline interval and source snapshots, fractional progress, per-opportunity transaction/result, container entries/claim remainders and cap discard state. Validate six rows each totaling 100%, distinct specialties, Dog 85% building materials, Pig Truffle gated at region 10, exact source filters, no pet-only mandatory recipes, no quality/furniture leak, bag-full partial claims, full-container no reroll, switch at 29/31 minutes, eight-hour cap, clock reversal and reload after generation/claim.

## 14. Balloon merchant and mystery boxes

At selected camps, show exactly three gold offers: one indestructible clothing item, one mystery box, and one backpack. Define explicitly whether the merchant backpack is indestructible; recommendation: yes, as special equipment at that rank. Do not infer that every gold-purchased object is indestructible.

**Confirmed balloon refresh:** keep three offers (special indestructible clothing, mystery box, backpack). Generate a new offer set each time the shop opens after being closed, even within the same camp visit. Closing ends that offer session; reopening rerolls the **special clothing item shown** and other unpurchased offers from current eligible catalog/tier. A legitimate random roll may repeat the same identity. No timer, currency charge, camp departure or purchase is needed to refresh. Use a persisted `BalloonShopSessionId`/open revision, not only a camp-visit seed. Repainting UI, switching item details or duplicate Open events while already open do not reroll; reload restores the currently open session until an actual close/reopen. Charge/commit against the displayed session/offer revision so a stale click cannot buy a different rerolled item.

Physical store/box products remain Q5, special clothing indestructible within global caps, ordinary box gear finite. Recipe knowledge has no Q/physical item field and starts at mastery one. **Purchased box manifests and purchases remain immutable** through all refreshes: reroll shop offers, never the contents/quality/recipe reservations of a box already paid for, nor an overflow reward. Each new box purchase gets its own atomic six-entry manifest; the unopened offer exposes no pre-rolled contents to reroll for free. This shop refresh never refreshes the camp conveyor trading opportunity.

Proposed six-entry generation, retaining the existing total rather than adding a seventh reward:

1. Reserve one eligible physical stackable mission-item entry. Its source must permit mystery-box delivery: **fishery-exclusive raw seafood is excluded even when requested by a mission**. If none is eligible, use a discovered current-region material.
2. Roll the other five slots independently using the proposed weights below.
3. For eligible identities, favor the current unlocked tier at 70%, older tiers at 30%, with a valid fallback when one subpool is empty. A recipe must have `MysteryBoxEligible` and pass the unlearned/unreserved checks below; do not bypass normal mission recipe unlocks.
4. Physical stackables deliver 1,000 units, equipment one instance, and a RecipeUnlock slot one exact recipe ID. Recipe slots do not become physical scroll stacks and do not consume backpack area. No special clothing enters the item pool.
5. Keep source/region filters. The existing active-mission item exception never overrides fishery-only source restrictions. Recipe unlocks must be within discovered tiers and have achievable exact ingredient sources; their pantry exceptions do not grant unavailable future ingredients.
6. Pre-roll all six typed entries, physical Q5 stat profiles, recipe IDs, and any duplicate fallback rewards; charge gold and persist the manifest atomically. Reopening/reload/overflow never rerolls results. Item claims use normal inventory/tray rules; recipe claims update known-recipe state once.

| Category for each of the five random slots | Proposed weight |
| --- | --- |
| Eligible materials/food items | 80% |
| Tool instance | 10% |
| Ordinary clothing/backpack instance | 5% |
| Optional Food/Misc recipe unlock | **5%** |

With a nonempty eligible recipe pool, five independent 5% rolls give about **22.6% chance of at least one recipe** per box (`1 - 0.95^5`). This is a starting proposal, not a confirmed drop probability or recipe guarantee. The mission-preference slot is not a sixth recipe roll. All physical rewards remain Q5; recipe grants do not receive quality, durability, cash resale, or automatic mastery.

Recipe selection/duplicate policy:

- Initial collectible pool: the twenty-six optional Food/Misc recipe IDs in sections 9.4.2-9.4.3, including seafood recipes. Do not silently move the mandatory apple juice/tool/clothing missions into this random pool.
- Require discovered `RecipeUnlockTier` and reachable ingredient sources, including an unlocked fishery/catalog tier for seafood. A refined ingredient can be supplied through an already-known recipe or an eligible discovered shop catalog; it need not be in the bag at purchase. No mandatory future-region ingredient or inaccessible crafting chain.
- Exclude already-known recipes and recipes reserved in other unclaimed purchased boxes. Reserve newly selected IDs within the current box too, so two slots cannot award the same recipe.
- If no eligible unlearned recipe remains, convert that slot to an eligible physical material/food reward. Do not invent a useless duplicate, refund loop, or 1,000 scrolls.
- If a selected recipe becomes known before claim through another authorized source, use its **persisted pre-rolled fallback**. Do not grant mastery, choose a fresh random recipe, or silently discard a paid reward.
- Claiming a new recipe grants knowledge once at mastery level one with zero crafted count. Subsequent crafts use the normal grade/furniture/timer/value rules. Claim and known-ID update share a transaction ID so reload cannot repeat grants/fallbacks.

Physical duplicates may merge after claim if their item/stat profiles match; recipe duplicates are prevented as above. Mission contribution occurs when an eligible physical reward is claimed, never from recipe knowledge. A full bag does not block learning a recipe, while physical overflow stays in the persistent tray. A completed mission is not eligible for the mission preference. Raw seafood remains a fishery cash purchase, even when its recipe is obtained from a gold-bought box.

Adding a recipe does not automatically add its finished product to physical box loot. Keep separate item-source/recipe-unlock flags; initially these twenty-six additions are recipe rewards, not a backdoor source of free seafood dishes or sale goods before their recipes are learned. Any later finished-item reward must be deliberately authored under the normal source filters.

These probabilities are proposed starting values, not facts from the reference game. Gold is earned in-game in this plan. Real-money monetization is outside the current implementation scope.

## 15. Maintainable C# architecture

### 15.1 Boundaries and dependency direction

Use a small domain/application/Unity-adapter split. Plain C# owns rules and saved state; MonoBehaviours translate Unity input, transforms, and views. ScriptableObjects author immutable definitions and convert them to runtime records. Do not put the current player's mutable state into shared ScriptableObject assets.

| Assembly | Responsibilities | Allowed project references |
| --- | --- | --- |
| `TheTrailWalker.Domain` | Item identity, containers, wallet, missions, crafting state, tool rules | None; ideally no UnityEngine dependency |
| `TheTrailWalker.Application` | Transaction commands, use cases, session coordination, save snapshots, clock/random interfaces | Domain |
| `TheTrailWalker.Unity` | Input, path motor, camera, scene adapters, definition loading, audio, pooled world views, persistence adapter | Application, Domain |
| `TheTrailWalker.UI` | uGUI presenters, drag views, panels, HUD, accessibility | Application, Domain; narrow Unity adapter references only when necessary |
| `TheTrailWalker.Editor` | Content authoring, recipe/source validation, balance reports | Relevant runtime assemblies; Editor only |
| Test assemblies | Domain rules, application flows, Unity integration separately | Only layers under test |

Start with Domain and Unity assemblies plus tests in the first proof if creating all six immediately slows learning. Split Application/UI when the collection slice exists. No dependency-injection framework is required: a `GameBootstrapper` constructs services and passes references explicitly. Avoid a global service locator and an event bus that hides ownership.

### 15.2 Suggested classes and records

| System | Main types and important responsibilities |
| --- | --- |
| Definitions | `ItemDefinition` with MaterialTier/explicit cooking-global permission; exact-ID `RecipeDefinition` with RecipeUnlockTier; mastery/time curves, three-ID `RegionGatheringProfile`, two-species `RegionFishingProfile`, source-specific loot, camp/leg-budget/mission/equipment/furniture/shop definitions |
| Scope/store validation | `ContentScopeProfile`: 24 region slots, four active resource tiers, disabled expansion catalog. `FurnitureDefinition` has price/house unlock but no material recipe; validate all active references and sources |
| Purchase quality policy | Physical store offers/reward delivery use `FixedQ5`; recipe knowledge has no grade. `CampConveyorTrade` preserves the actual listing profile. Choose by transaction/reward type, not currency or location |
| Fishery/box reward definitions | Two `DiscoveredFish` offer slots + one `FisheryExclusiveSeafood` slot with 1,000-unit reset budgets; typed `RewardEntry` as physical stack/equipment/`RecipeUnlock(RecipeId)` with persisted fallback. Quality applies to physical entries, never knowledge |
| Price definitions | Explicit `PriceCategory`, active resource tier, nonmaterial level-ten Q1-equivalent reference or fixed material base; finished-quality price curve and applicable durability-value curve; no category inferred from display name |
| Identity/state | `ItemStack`, `ItemStatProfile`, `EquipmentInstance`, `ContainerState`, `PlayerState`, `WorldBundle`, stable definition IDs and unique instance IDs |
| Inventory | `InventoryService`, `SpatialPlacementRules`, `ContainerTransferCommand`, `StackCompatibility`; no UI coordinates in domain rules |
| Movement/camera | `ITrailPath`, `TrailMotor`, `SpeedController`, `MovementPauseReasons`, `PostHarvestStopState`, `CameraOrbitController`, `FollowCameraRig`; independent yaw, explicit harvest restart, inventory-speed snapshot |
| Input | `GestureRouter`, `AvatarSpeedGesture`, `CameraOrbitGesture`, `ResourceGesture`, `InventoryDragController`; lock horizontal/vertical ownership and never change movement from orbit |
| Collection | `PickupService`, `ResourcePointState`, `HarvestService`, `LootManifest`, `WorldDropPresenter`, `WorldDropPool` |
| Equipment | `EquipmentService`, `CapabilityCalculator`, `DurabilityService`, `EquipmentExpirationCommand`; remove zero-durability worn instances, restore defaults/caps, atomically transfer displaced contents |
| Stamina/recovery | `StaminaService`, `SegmentStaminaBudget`, `FaintRecoveryService`, `FaintEvent`: distance-only authored drain (<=26 hearts/leg), progressive food/camp recovery with retained pending hearts, zero-plus-no-pending-or-ready-food faint trigger, exact-once 2-4 eligible-entry loss and wake state; no combat health |
| Hunting aim | `PassiveAnimalPresenter`, `HuntingAimController`: moving passive targets, eligible carried tool gate, committed hit/miss events; no attacks on the player |
| Crafting | `CraftingService`, `ExactIngredientMatcher`, instance-aware `IngredientReservation`, `EliteUpgradeValidator`, per-recipe mastery/stat/time services and claim views; upgrades atomically consume one exact Normal instance plus prior-tier material, never bulk gear |
| Missions | `MissionService`, `MissionProgress`, `MissionPrerequisiteGraph`, `MissionContributionPolicy`, `MissionNpcPresenter`; four independent branch sequence indices/tier/source prerequisites and one active mission; no global recipe-category phase gate |
| Economy | `WalletService`, `CraftPriceCalculator`, `SaleQuoteCalculator`, `TradeSession`, `TradeListing`, `TradeInputGate`, `ShopVisitState`, `WarehouseService`; fixed-point per-unit value, 64-bit totals, separate craft quality/durability and transaction factors |
| Travel | `TravelService`, `TrailSceneLoader`, `RegionUnlockState`, `CampVisitState` |
| Settlement | `HouseService`, `FurniturePlacement`, `BuffRegistry`, `LumberFieldState`, `PetRewardService` |
| Persistence | `SaveService`, versioned DTOs, `SaveMigrator`, `IClock`, `IRandomSource`, transaction IDs |

Use typed IDs or clear conventions such as `wood.pine.log`, `recipe.pine.plank`, `recipe.cheese.cow`, `recipe.cheese.goat`, `recipe.cheese.mixed`, and `region.pine_meadows`. Never use localized names as save keys. Composition/tags help classify pocket eligibility, tools, food, or UI groups; tags never satisfy recipe ingredient requirements. Distinct material varieties are distinct item IDs, not a generic item with a freely interchangeable label.

Store region ID and resource tier as separate fields. For the proposed distribution, regions 1-6/7-12/13-18/19-24 map to tiers 1/2/3/4; never feed region 24 into a tier/durability/timer formula. Validate that active loot/recipes/missions/shops/house costs reference no disabled future species. Warn when a recipe adds more than three distinct ingredients, reject over nine, and reject furniture outputs from player recipes. No weight budget belongs in inventory state/transfer rules.

Validate exactly 18 distinct loose IDs per tier, three per region, with source overlaps limited to its branch + pelt/hide/confirmed feather. Logs/meat/fish cannot appear as featured loose IDs. Map icons come from the same definitions as spawning. Validate source availability by region, four-branch recipe reachability, optional second sets against caps, early camp/food accessibility, and a hard maximum 26-heart walking budget for each leg. Camp count scales from the data template, not from loaded-scene count.

Also validate all nine Elite recipes: one Normal of the same exact family/tier, extra advanced ingredient from tier `toolTier - 1`, known previous-tier refinement, no same-tier advanced add-on, x1 quantity, and positive eligible input uses under the proposed condition policy. Normal/Elite action tables differ at/below tool tier but share the +2 ceiling; only Elite has the all-eligible-target loot buff. Campaign mission quantities are explicit tier data with ten at the start and a 150 ceiling; developer boundary tests are separate. Cost reports recursively include the sacrificed Normal, refinement and house/mission reserves.

Recipe validation also rejects duplicate recipe IDs, unresolved family placeholders, ingredient OR-lists/tag selectors, and unknown item IDs. Every required variety in an intentional mixed recipe must have its own quantity. Autofill may aggregate qualifying stacks of the same required item ID; it may not use a higher tier or another species. Unlocks/mastery/save records key by recipe ID, so equal-looking output families never merge distinct recipe progress.

Enforce the unlock-tier window on every direct ingredient: `{1}` for tier one, `{R-1,R}` otherwise. Only exact pantry/container IDs explicitly approved as cooking-global can bypass the older-tier bound, only for Food and only when accessible at or before unlock tier. Never allow that flag to change input identity or help a nonfood recipe. Validate the three pine family recipes, tier-two/three/four cord/fuel bridges, and all nine Elite inputs against this rule. House deposits/store purchases remain separate transaction types.

Fishing validation: four fish IDs/tier, six distinct unordered pairs across each tier's six regions, and exactly two regional fish IDs per profile. Validate habitat-specific node sources and mission availability; the three loose-resource icons do not change. Each proposed fish has its own exact roast recipe using its tier's named wood, and every repeated/bulk serving spends that fuel. Source-discovery filters apply to fishery/mystery-box offers, not just a broad fish-band unlock.

Fishery validation separately requires two distinct eligible fish offers and one eligible exclusive, all Q5/cash, each <=1,000 units per reset with purchase/reload conservation. Exclusive seafood never enters regional fish profiles, loose tables, pet/NPC supply, or raw mystery rewards. Validate the twenty-six collectible recipe IDs (Food/Misc, tier rules, exact inputs, source availability), six total typed box slots, weights totaling 100%, duplicate exclusion, and exact-once knowledge/fallback claims. Main mission/house/tier unlocks must not require a random recipe reward.

UI subscribes to state-change events such as InventoryChanged, CraftBatchReady, and MissionCompleted, and unsubscribes on disposal. Domain services do not call UI methods. A command result contains success/failure and a player-facing reason; the presenter decides how to show it.

### 15.3 Folder structure

**Applied:** at the developer's request, the 52-directory scaffold below now exists, with unique Unity folder `.meta` files and `.gitkeep` files retaining empty leaf directories in version control. Existing assets/settings remain at their current paths. `Bootstrap.unity` is a future scene, not an empty placeholder file; gameplay classes and assembly definitions will be added with their implementation milestones. Further layout revisions can follow project development.

```text
Assets/
  _Game/
    Art/
      Characters/
      Environment/
      Items/
      UI/
    Audio/
    Data/
      Items/
      Recipes/
      LootTables/
      Missions/
      Regions/
      Equipment/
      Settlement/
      BalanceProfiles/
    Input/
    Materials/
    Prefabs/
      Player/
      ResourcePoints/
      WorldDrops/
      Camps/
      Settlement/
      UI/
    Scenes/
      Bootstrap.unity
      Prototypes/
      Regions/
      Settlement/
    Scripts/
      Domain/
        Inventory/
        Items/
        Crafting/
        Missions/
        Economy/
        Settlement/
      Application/
      Unity/
        Input/
        Movement/
        Camera/
        Interaction/
        SceneLoading/
        Persistence/
      UI/
    Settings/
    Editor/
    Tests/
      EditMode/
      PlayMode/
  ThirdParty/
```

Put each assembly definition at the root of its intended assembly subtree. Keep generated saves/build artifacts out of Assets. Keep `.meta` files in version control, and move existing URP/Input assets with Unity-aware tooling during implementation so GUID references survive. The directory scaffold is explicitly approved; add gameplay classes only when their milestone needs them.

### 15.4 Scene and presentation lifecycle

Bootstrap owns a single persistent session root: services, player state, player/camera presentation as appropriate, and HUD shell. Trail segment scenes contain paths, resource points, environment, camp anchors, and stable entity IDs. Settlement replaces the trail presentation and uses selectable building views; it does not recreate inventory/services.

Load scenes asynchronously. Bind the saved player to the correct path/camp anchor only after loading completes; then enable gestures. Persist the transition destination before unloading the old scene. Prevent duplicate transition commands and duplicate persistent roots. Keep one current segment and at most one prefetched segment; postpone Addressables until built-in scene loading is demonstrably insufficient.

**Additional implementation/validation requirements:** `StaminaService` owns a `FoodRecoveryQueue` and atomic consume/enqueue records, integrates net walking/recovery without frame-order waste, and exposes pending hearts to faint/departure/HUD logic. `TravelService` uses separate current-region camp and discovered-region destination models, with explicit selected-camp versus first-camp resolution. Validate all 26 optional recipe IDs, including each new dish's exact regional ingredient access rather than tier access alone. Bakery apple pie remains a separate Q5 premium product, outside the 26-recipe box pool.

**Section-20 implementation/acceptance additions:** persist creation provenance (player craft/NPC/shop/loot), creation level, immutable stat snapshots, pending-product owner/timer/footprint, horizontal chest coordinates/offset, warehouse per-instance escrow/value and direct-3%-gold remainder, collection mode, and settlement visit/departure state plus balloon open-session revision. Migrate page containers and earlier warehouse conversion state explicitly without re-paying past rewards. Reject rotation requests and mission quality/durability filters. Verify the full tool table and Elite doubling with matching modifiers; NPC L1/random Q/zero buffs against player-crafted profiles; craft center-display/drop with menu open and timer still running; ready transition in bag/tray credits once; used gear accepted by missions and warehouse with different identity/valuation responsibilities; ten unique tools fill a box; mixed-profile exact-ID deposits conserve values; 10,000 cash pays 300 gold once; no extra warehouse bonuses; one-object release pickups and 1,000-unit test pickups conserve manifests. Replace obsolete page/quality-lock/exchange-denomination milestone checks with these requirements. Milestone 11 builds horizontal storage, milestone 12 sells every category under direct 3%, milestone 9 persists NPC random profiles/discounts, and milestones 16-17 exercise both collection presets.

**Pet/truffle implementation checks:** implement `PetDefinition`, `PetLootWeightGroup`, `OwnedPets`, `PetOfflineInterval`, `PetRewardContainer` and exact-once purchase/claim services. Section 13.5 defines interval carry and source-snapshot ownership; never reroll unclaimed rewards. Add Truffle to region/source maps, source-filtered NPC/material boxes and optional ingredient requests only after region 10; move Lavender to the farm catalog. Validate 72 distinct loose IDs/18 per tier, three region-10 icons, and the 26 box recipes plus one separate mission truffle recipe. Implement the optional Gatherer side mission without advancing/reordering its mandatory branch index; keep the global one-active-mission rule. Verify all truffle recipes without owning a pet, recipe tier/pantry windows, and progressive six/eight/nine-heart totals. Save truffle unlock/mission IDs and migrate old Lavender loose manifests by preserving already-owned/committed Lavender rather than silently converting it to Truffle. Future spawn tables use the new definition revision.

**Naming/refresh implementation and checks:** localize the 24 region names and all camp names via the ordinal rule in section 6.2.6; assert no tier-resource prefixes in names and no changes to CampId, source rows, fish pairs or counts. Test locale switching, existing-save map/Return to Trail and stable camp names after reload. Replace all shop reset countdown state with settlement visit/departure state and balloon open-session revision; no stock cooldown or refresh scheduler. Verify settlement leave/return replenishes fishery limits once; internal shop/home navigation and restart do not. Verify balloon close/reopen rerolls special offers in the same camp, duplicate open does not, stale purchase fails without charge, and paid boxes/recipe reservations remain exact-once across rerolls. Milestone 14 and the shop/box acceptance matrix use these distinct refresh triggers.

## 16. Save data, timers, and recovery

Build persistence early because quantities, pending batches, node loot, one-time rewards, and three-day timers are central mechanics.

Save at least:

- Schema version, content version, profile ID, and save revision.
- Current context, region/segment/camp IDs, distance along path, authored leg-budget revision, requested speed, post-harvest/manual-restart state, last trail camp, camera yaw, inventory-resume snapshot. Never rescale the remaining budget from current gear or reroll a faint event while restoring position.
- Stamina, foreground regeneration timing state, tool family/tier/Normal-or-Elite variant, equipment instances, uses/distance remaining, infinite-durability flags, and equipment-expiration transaction state. Zero-durability worn instances stay removed on reload, with default slots and displaced-content ownership preserved. Hooks identify carried tools, not equipped tools.
- Every container, item position/fixed orientation/quantity/quality, immutable output-stat profiles (Q1-equivalent base unit price, finished-quality price factor, applicable durability-value factor, intrinsic unit value, level-adjusted base durability, quality/furniture durability bonuses, final maximum durability, creation mastery/furniture snapshot), pockets, hooks, shelves, and delivery queues. Values stay fixed-point, balances/transaction totals 64-bit.
- Active mission, each of the four branches' tier/next mission index, prerequisite completion IDs, contributed quantities, completed/rewarded recipe IDs, clothing-set variant unlocks; no old category-phase state.
- Known recipes with authored unlock-tier/content revision, per-nonmaterial mastery counts, pending snapshot tier/category/level/stats and exact ingredient/fuel reservations, completion/claim IDs, and modifier snapshots. Cooking-global permission is definition data, not a player-granted ability or stored substitution. Materials never gain mastery.
- Pending Elite upgrade input instance ID/consumption transaction and prior-material quantities, output recipe/stat snapshot and claim ID; the consumed Normal cannot reappear or grant Normal mastery on recovery. Preserve per-mission authored quantity, not a hard-coded ten applied after reload.
- Both wallets, paid tolls, discovered regions/camps, warehouse partial boxes, last-ten-box totals/remainders.
- Camp visit/trade availability and cancellation settlement records; shop visit/reset IDs, selected offer slot roles/IDs/Q5 quotes, stock remaining and purchases. Fishery two-fish/one-exclusive selection and 1,000-unit stock limits persist until the actual reset.
- Box purchase IDs and all six typed reward entries, physical profiles, recipe IDs reserved/claimed, pre-rolled duplicate fallbacks and exact-once claim transactions. Known recipe IDs/new mastery-one states persist atomically with grants; no knowledge-stack quantity or Q5 crafted output is invented.
- House level/deposits, furniture placement, permanent first-purchase buff unlocks, horizontal chest containers/scroll offsets.
- Lumber-tier unlocks, all tree progress/timers, remaining drop manifests.
- Active pet, offline settlement timestamp, and pending pet rewards.
- Faint event ID, selected eligible entry IDs/quantities, loss-commit flag, recovery progress/target, and animation phase. Reload resumes that event once, never refunds/removes again, and never selects worn equipment.

Additional required save fields: pending food queue entries (consumption ID, serving recovery snapshot, remaining hearts/order, fractional timing) and atomic consumption state; current-region map selection, unlocked segments/camps, discovered-region IDs, and in-flight travel destination/transition commit. Loading an older save initializes an empty food queue without granting recovery or changing discovery.

Use a versioned snapshot DTO format such as JSON initially, with explicit enum/ID serialization and migrations. Write to a temporary file, flush and replace the main save using platform-supported operations, and retain a last-known-good backup. Validate counts, IDs, bounds, and duplicates on load; route recoverable placement problems into a tray. Never overwrite an unknown newer-schema save without surfacing the incompatibility.

Save critical transactions immediately or through a short coalescing queue that completes before confirming irreversible local rewards. Save on application pause/focus loss as well; mobile OS termination may not call quit. On resume, reconcile timestamps once and refresh views. Use monotonic elapsed time during an active session and UTC for persisted cross-session timers. Inject the clock and random source for repeatable tests.

Crash-recovery test examples: kill after ingredients are spent but before craft UI updates; after a listing reaches the shredder but before payout animation; after gold is charged but before the mystery box opens; during a ferry transition; after tree rewards are rolled but before bundles appear. Each reload must yield one valid committed outcome without duplicated items, double charges, or vanished rewards.

## 17. Android configuration and performance plan

### 17.1 Initial configuration work

1. Verify Unity Hub Android Build Support, SDK/NDK Tools, and OpenJDK for this editor version.
2. Select/create an Android Build Profile; set a project-specific application identifier.
3. Retain IL2CPP, ARM64, and Vulkan/OpenGL ES 3 initially; verify on at least one Vulkan-capable phone and an appropriate fallback device if available.
4. Enable optimized frame pacing and choose an explicit 30 FPS starting target.
5. Preview with the Mobile quality asset. Disable HDR and post-processing for the first pass; reduce shadow distance to approximately 20-30 meters and use one directional light.
6. Prototype portrait with safe-area-aware layout; test crafting/inventory readability before locking orientation.
7. Build a development APK immediately, before any large content production. Use AAB/signing setup when preparing store delivery, not as a blocker for the local prototype.

Installed packages can remain pinned. Add a Unity-compatible Splines release when authored curved routes need it; keep a waypoint adapter for tests. Cinemachine 3 is optional for the camera. AI Navigation is unnecessary for the player. Visual Scripting, Multiplayer Center, Timeline, and unused IDE integrations can be removed later after checking dependencies; package cleanup is not a prerequisite for gameplay.

### 17.2 Profiling targets

These are initial engineering targets to measure, not already achieved results:

| Area | Starting target/test |
| --- | --- |
| Frame pacing | Stable 30 FPS, 33.3 ms frame budget; check sustained play for 15-20 minutes |
| Managed allocation | No routine per-frame allocations in movement, input, and static HUD updates |
| World loot | 24 simultaneous pooled bundle views; much larger logical pending manifests |
| Inventory | One representation per stack/instance; virtualize offscreen container entries and recipe rows |
| Bulk crafting | Stackable x1000 completes/claims without 1,000 GameObjects/UI cards; unstackable requests other than x1 are rejected |
| Scene lifecycle | No repeated growth in loaded scene objects, listeners, or memory after 20 transitions |
| Save/load | Profile with all 24 active lumber trees, 24 region-discovery records, full chests, and pending crafts; measure main-thread stalls |
| Touch/camera | UI/resource gestures stay responsive during saves/drop spawning; 360-degree orbit does not change speed or allocate every frame; nearby rear pickups remain targetable |

Use shared materials, texture atlases, simple colliders, baked/static lighting where suitable, mesh LODs, modest foliage, and GPU instancing for repeated compatible environment meshes. Prefer stylized low-cost visuals over adding full-screen effects. Do not simulate every dropped unit with physics; the confirmed full stacks must stay cheap in representation.

Record device model, OS, build revision, graphics API, CPU/GPU frame time, memory, temperature/thermal behavior, and test scenario. Set the actual memory budget after measuring the intended minimum device rather than inventing a universal limit. Use Android texture compression appropriate to target coverage, test ASTC where supported, and supply a supported fallback if that coverage requires it.

## 18. Step-by-step action plan

Implement one milestone at a time. Each must produce a small usable build or a demonstrable rules test before the next major subsystem starts. Estimates should be made after the first two Unity learning milestones; dates here would imply unjustified certainty.

### Milestone 0: Project safety, learning scene, and device baseline

Dependencies: none.

1. Commit/review the template state and keep this document plus `Trail.md` in version control.
2. Verify Unity version/toolchain and create the Android Build Profile.
3. Create a throwaway prototype scene to learn GameObjects, components, prefabs, serialization, scene loading, Canvas, and the Input System.
4. Configure Mobile quality, provisional portrait safe area, application ID, frame cap, and frame pacing.
5. Produce and install a development APK with one lit primitive and a UI button.

**Exit check:** the scene launches on a phone, the button responds, and one short profiler capture establishes a baseline. No store publishing is needed.

### Milestone 1: Forward movement and gesture ownership

Dependencies: 0.

1. Create Bootstrap and one curved 100-250 m path with a camp anchor at its end.
2. Implement distance-based path evaluation, requested speed, caps, and a follow camera with independent full 360-degree horizontal orbit.
3. Implement avatar-only vertical speed gestures, horizontal hold/swipe orbit, directional gesture ownership, and Editor controls; no joystick. Orbit never stops/steers the player.
4. Add inventory placeholder with pre-open-speed snapshot/resume, temporary pause reasons, persistent post-harvest manual-restart state, and app-suspension handling.
5. Add the automatic camp stop and a debug display of distance/speed/pause reasons.

**Exit check:** no backward/sideways movement; full camera orbit at rest/while walking leaves speed/path direction unchanged; horizontal and vertical gestures do not interfere; camera release keeps chosen yaw; inventory restores only its saved speed subject to caps/other stops; closing it during a post-harvest stop does not restart; camp arrival never overshoots.

### Milestone 2: Item model, area capacity, and full-stack collection

Dependencies: 1.

1. Add Domain assembly and stable item IDs; author pine branch/log, stone, hemp, berries, and a starter tool.
2. Implement 1,000-cap stacks, five quality grades, unique equipment instances, and deterministic 2D container fit.
3. Add world bundles, hold-drag, downward quick swipe, backpack aperture, and pickup presentation. Retain nearby drops behind the avatar within a configurable rear collection window; target them through the rotated camera without pausing walking or bypassing range/occlusion.
4. Add stack growth, compatible touch merging, rejection feedback, and quantity badges.
5. Add debug commands for mixed shapes, quantities 1/999/1000, and a completely full bag.

**Exit check:** every trail stackable pickup starts at 1,000; no repeated-touch duplication; failed placement leaves it intact; quality/profile differences stay separate; tools cannot stack; resizing cannot overlap/escape the bag; camera orbit reveals nearby missed resources for collection without stopping the player or reaching beyond the permitted window.

### Milestone 3: Persistence foundation and recovery

Dependencies: 2.

1. Save inventory, nearby world bundles including behind-player drops, path position, requested speed, camera yaw, inventory-resume snapshot as applicable, post-harvest restart state, and player state.
2. Implement versioned DTOs, backup/recovery, stable instance IDs, and transaction records where needed.
3. Save on critical collection and app pause; restore without refreshing the current segment.
4. Add injected clock/random source and a developer Reset Profile action with confirmation.

**Exit check:** force-close after collection and reopen; quantities and placement survive; corrupted main snapshot can recover the backup; loading never duplicates world and inventory ownership.

### Milestone 4: Equipment, pockets/hooks, and stamina

Dependencies: 3.

1. Add five gear slots, starter rags, boots/hat caps, and quality-adjusted durability.
2. Implement zero starter pockets/hooks, independent caps of ten each, eligibility, inventory sideways navigation/readable ten-position panels, and contextual detail/Eat popup. No weight limit.
3. Add fractional hearts, food use, authored per-leg distance drain, progressive safe-area regen, and zero-stamina/no-food faint recovery with 2-4 eligible carried-entry loss. Keep regen independent of UI/trading locks and persist the loss manifest before animation.
4. Implement distance-zero equipment removal, immediate no-hat/no-boots/rag-shirt/rag-pants/rag-pack fallback, lost equipment bonuses, cap clamping, and atomic content transfer to valid default storage/overflow. Debug-expire each slot and all slots together.

**Exit check:** starter/global caps, no-weight fit, and 1.5x stack scale hold. Expired equipment transfers contents without accidental loss. Faint loss is deliberate: 2-4 actual eligible entries/stacks, worn outfit protected, no duplicate penalty on reload, no-food trigger distinct from zero-with-food stop; recover/wake in place and wait for upward speed input. Regeneration/trading remain independent.

### Milestone 5: Two segments, camps, map, and toll

Dependencies: 4.

1. Add an asynchronous transition between two trail segments and a camp view with four seats.
2. Persist visited camps, region discovery, camp visit IDs, and last trail camp.
3. Add a minimal regional map and ferry NPC with a one-time test toll.
4. Block input during transitions and bind the persistent player at the arrival anchor.

**Exit check:** repeated travel creates one player/session only, a paid toll remains paid after restart, locked regions cannot be fast-traveled into, and return location is correct.

**Milestones 4-5 additions:** implement section 4.4.3 before stamina/faint acceptance, including consume while inventory is open, resume walking, full-meter retention, and queue save/reload. Build the camp map with current-region camps plus the other discovered-region list below its first camp, then verify selected-camp and cross-region first-camp arrivals with interrupted transitions. Milestone 14 authors/gates the eight stronger box dishes and separate premium apple pie; retain six box entries and the existing proposed recipe-roll weights.

### Milestone 6: All three harvesting families and queued drops

Dependencies: 5.

1. Implement separate Normal/Elite action tables by family/tier difference, Normal-only tier one, Elite action savings at/below its tier, all-eligible-target Elite drop boost, and one-use-per-committed-action wear. Stop at animation start/remain stopped until upward speed input.
2. Author one stump, moving passive rabbit, and fishing point; add aiming/no animal attacks. Begin with the explicit pine axe/slingshot/rod recipes; test the region-one Rainbow trout/Bluegill pair using placeholder views without waiting for all sixteen fish assets.
3. Roll and persist bundle manifests; full-stack counts follow section 7.
4. Add pooled visible drops, type-grouped pending piles, and partial harvest recovery.
5. Add debug resource/tool tiers and Normal/Elite comparisons; block target tiers above tool tier + 2 before an interaction starts.

**Exit check:** matching-tier Elite axe/hunting/rod need fewer actions; lower targets reduce where above the one-action floor; +1/+2 targets remain eligible and receive Elite boosted drops even without an action discount; +3 cannot start. Normal never gains drop boost, each committed action wears one use, and harvest stop/aim/queue/reload rules remain correct.

### Milestone 7: Material conversions, recipe mastery, timers, and x1000 crafting

Dependencies: 6.

1. Add nine-slot recipe panel, recipe selector, Autofill, quantity selection, and output/level/timer previews.
2. Implement exact-ID unlevelled material conversions, instant gatherable outputs, and per-definition MaterialTier/RecipeUnlockTier. Add the used cord/charcoal/stuffing bridges. Wrong variety or out-of-window direct input fails without costs; future-tier ingredients never leak into starter recipes.
3. Implement nonmaterial levels 1-10 keyed by unique recipe ID, stat order/Q5 cap, and immutable output profiles. Add Q1-equivalent base pricing, finished-quality premium, and finite-gear durability premium; preserve the 1-cash basic/6,000-cash level-ten Misc anchors. Cow/goat/mixed cheese never shares mastery or substitutes ingredients.
4. Implement category/tier/mastery timers: Food/Tool down with level, Wearable/Misc up, hours for advanced Wearable/Misc, Food/Tool always below one hour even at x1000; buffs never change time.
5. Add reservations, atomic costs, the centered one-second stat/timer presentation with menu kept open, immediate timed-product drop into the bag, exact-once mastery at readiness, and persistent overflow delivery.
6. Restrict tools/wearables/backpacks to x1 in both UI and domain commands; reject x10/x100/x1000 without spending materials. Keep unique single-item claims, and merge ready stackables only when output-stat profiles match.
7. Add all nine instance-consuming Elite upgrades and earlier-tier material recipes. Reject wrong Normal family/tier/Elite-as-input, missing previous materials, exhausted/mission-owned input, or full queue without costs. Save/resume cannot return the sacrificed Normal or duplicate the Elite; output uses its own recipe snapshot.
8. Author fixed-input examples, explicit pine tool trio, one meat/fish roast per tier, and a later bakery recipe using exact older global Wheat flour/Cow cheese. Check permitted cooking exceptions, rejected nonfood pantry use, exact wood consumption on x1/x100/x1000, no spice/fuel substitution, and the existing Food timer cap.

**Exit check:** exact IDs/quantities and separate recipe mastery hold; all nine Elite upgrades use matching Normal + previous-tier advanced material once, with no ingredient-stat inheritance or extra Normal XP. Partial-wear policy is explicit, previewed; no costs on failure or duplicate items on reload. Material conversion, Q5/timer rules, stackable-only bulk, and overflow handling remain correct.

### Milestone 8: Four mission branches, featured resources, and route safety

Dependencies: 7.

1. Add four branch definitions, the twelve first-tier missions and cross-branch prerequisites. Keep one active mission; no old Materials/Tools/Food/etc. phase gate.
2. Build source data for three featured items/region and 18/tier, with map icons and separate interactables. Validate all 24 slots as data; use only short representative scenes initially.
3. Verify automatic mission consumption/claims, ten starter units and proposed 50/100/up-to-150 later quantities, four clothing assignments, worn-gear protection, and x1-only tools. Test competition between Normal-tool mission contributions and Elite upgrade reservations.
4. Play the new region-one introduction: branches -> log unlock, apples -> juice unlock, accessible camp crafts and enough stamina before food; region two introduces Flint/Hemp fiber and longer legs requiring food.
5. Simulate lower-tier route budgets and the maximum 26-heart leg with twelve hearts plus ordinary food. Add faint/recovery/loss/wake animations and restart/reload checks; show HUD/map/departure information from the same data.
6. Validate 10-14 early and up-to-24 later camp layouts with streaming; author one active-tier representative before scaling to full region content. Confirm tier-three feathers as the approved shared source.

**Exit check:** four independent branch sequences persist; first twelve rewards match the table and are achievable; G1/G2 unlock usable food in region one; inaccessible dependencies show locked branches rather than active impossible missions. Three map icons match loose spawns, 18 distinct IDs/tier and only permitted overlaps hold. Ten-unit missions leave 990; tool-instance missions/gear protection remain correct. No leg exceeds 26 hearts; opening legs work without food, advanced ones work with ordinary meals, and faint loss cannot reroll/double on reload.

### Milestone 9: Camp trading and wallets

Dependencies: 8.

1. Implement Cash/Gold wallets, category/tier/mastery base-price data and the section 12.2 quality/durability formula, 64-bit stack quotes, three NPC offer sets, and one exclusive 15-second visit session. Author the price references now; final economic balance remains a later phase.
2. Add conveyor views, quantity escrow, buy price position, NPC purchase, natural 80% shredder/expiry fallback, and Return cancellation at 70% of remaining player listings.
3. Lock all nontrade player actions; keep camp stamina regeneration running. Persist payout IDs, remaining time, Completed/Canceled states, and spent visit status; handle OS pause without refreshing the session.
4. Add transaction reporting for both single units and full-stack listings.

**Exit check:** only trade/Return actions work during the 15 seconds; stamina still regenerates; natural shredder/expiry pays 80% once; cancel pays 70% only on unsold player listings, then ends that visit's opportunity; NPC goods never pay player cash; no expiry/cancel double settlement; restart/reopen cannot refresh; leaving/returning or reaching a different camp creates a new opportunity.

**First mechanics slice review:** walk safe opening/food-requiring test legs, collect full stacks, view three featured icons versus interactables, use three tool families, arrange/equip, eat, craft supported batches, progress four branches, trade, intentionally faint/recover, and reload. Fix correctness before settlement expansion.

### Milestone 10: Settlement shell and first home

Dependencies: successful mechanics slice review.

1. Add side-scrolling settlement building selection and debug access for testing.
2. Add home-front view, level-one interior grid, store/upgrade buttons, and six store-purchased furniture types. Require no furniture recipes/material purchase costs; first house upgrades use tier-one pine/bricks/cloth.
3. Implement first-purchase buff records and house deposits. Verify level base + quality + furniture durability, including quality without furniture. Price uses Q1-equivalent base -> finished-quality premium -> applicable durability premium -> later sale modifiers; no duplicate C/U layers and no craft timer change.
4. Add world map and Home/Warehouse/Lumber/Return context navigation.

**Exit check:** normal unlock remains after region four, debug access is clearly separate, return goes to the recorded camp, buffs do not duplicate through repurchase, and upgrades persist deposited materials.

### Milestone 11: Home storage and placement limits

Dependencies: 10.

1. Implement single horizontal-scrolling chest and validate its usable layout on the target phone.
2. Add shelves with three rows/four entries and one-unit splitting.
3. Add furniture collision/placement and storage-to-inventory transfer transactions.
4. Debug-unlock level eight and maximum chest capacity without changing the normal progression rules.

**Exit check:** all content stays in safe bounds at the smallest supported screen size; shelves reject stacks without losing units; huge hidden storage does not create huge active UI trees; transfers have exactly one owner across reloads.

### Milestone 12: Warehouse and gold conversion

Dependencies: 11 and wallets from 9.

1. Add two ten-unit boxes, exact item-ID lock for every sellable category, partial deposits, replacement boxes, and cash payout.
2. Add ten-box accounting, direct 3% gold settlement, and fractional remainder persistence.
3. Add optional bounded multi-box deposit after single-box correctness is proven.
4. Record inputs, outputs, and totals for later economy analysis.

**Exit check:** ten units from a full stack leave 990; no partial-box payout; each tenth completed box pays the expected gold once; reload during payout cannot repeat rewards; quantity/value multiplication cannot overflow.

### Milestone 13: Lumber field and long timers

Dependencies: 12.

1. Build one tier with six independent trees in two rows and gold-tier unlock.
2. Implement section 8.2's massive-tree Normal/Elite action curves, +2 ceiling, Elite faster at/below own tier, Elite drop buff even at eligible +1/+2, x50 manifests, type piles, and 72-hour respawns.
3. Add simulated time/expiry warnings, then expand only to four active wood tiers (24 tree records). Leave the other four tiers in the design backlog.

**Exit check:** all six timers are independent, x50 rewards remain full stacks, leaving/reentering preserves pending quantities, advancing time respawns only eligible trees and expires only their old rewards, and rendering stays capped.

### Milestone 14: Settlement shops and balloon merchant

Dependencies: 13.

1. Implement the three-offer shop service/Q5 policy. Configure fishery as exactly two distinct discovered fish + one random eligible exclusive seafood, all cash; one stack/1,000 units per slot/reset. Add proposed Shrimp/Crab/Squid/Lobster catalog without adding trail fish/loose entries.
2. Persist stock/slot roles, region filters, Q5 quotes, purchases/overflow. Reopen/reload never refreshes stock; valid settlement reset does. Buying ingredients never grants their recipe/mastery or player craft buffs.
3. Add balloon gear/backpack and six **typed** box entries; integrate the proposed 21 Food/5 Misc collectible recipes. Refresh unpurchased balloon offers on close/reopen, including special clothing; preserve paid manifests and main mission unlocks.
4. Add a configurable recipe category (proposed 5% of each random slot), discovered/ingredient-source eligibility, unlearned/pending-reservation exclusion, persisted duplicate fallback, and exact-once claim. Physical entries stay Q5; new recipe knowledge starts mastery one/zero crafts and takes no bag space. Exclusive raw ingredients remain fishery-only.

**Exit check:** fishery always has two fish/one exclusive at Q5/cash and no reset budget can exceed 1,000; seafood source restrictions hold. Boxes keep six entries, eligible unlearned recipes grant once even with full bags, duplicates/exhausted pools fall back deterministically, and reload never rerolls. All physical rewards stay Q5; knowledge does not create a product/stack/quality/mastery-ten grant. Ordinary gear stays finite, conveyor quality unchanged, and no mandatory progression relies on box luck.

### Milestone 15: Pets, offline reconciliation, and remaining furniture

Dependencies: 14.

1. Add the level-three home-store pet purchase section, left-front pet-home selection, separate reward container, and Dog/Pig definitions first; then author the six-pet roster and previews.
2. Implement capped offline rewards, pet switching, and exact-once reconciliation.
3. Populate the remaining furniture levels using the evenly distributed buff table.
4. Add decorative neighbor-house variants with no simulation.

**Exit check:** no pet access before level three in normal play, one offline interval pays once, switching pets cannot duplicate time, backward clock movement yields zero negative progress, first-purchase buffs remain capped.

### Milestone 16: Full mechanics and limits audit

Dependencies: 15; keep full-stack pickups enabled.

1. Use a Mechanics Test balance profile with default trail pickup quantity 1,000, all debug travel/rank selectors, simulated clock, and reproducible random seeds.
2. Run the limit matrix: instant material x1000, mastery 1/5/6/10, timer directions, hour-scale Wearable/Misc, rejected gear bulk, Normal/Elite action/drop differences, all nine upgrades, ten/50/100/150 mission quantities, mission-versus-upgrade tool ownership, +2/+3 boundaries, trading/regen, and massive manifests. Keep 1,000-unit pickups.
3. Capture sustained Android frame time/memory and repeat scene transitions, save/load, and suspension.
4. Fix ownership, UI readability, overflow, input conflicts, and data validation before tuning resource amounts.
5. Mark each proposed rule as accepted, revised, or still experimental based on observed tests.

**Exit check:** all major systems work at their intended limits with no item/currency duplication or loss; document results with device/build identifiers. This is the required gate before natural progression balancing.

### Milestone 17: Regional content and progression balancing

Dependencies: mechanics-and-limits audit accepted.

1. Validate all 24 regions' three-item tables (72 IDs) and four resource tiers using representative segments; incrementally author 10-24-camp region templates with reused kits/streaming. Do not add future wood/animal tiers or require hundreds of finished segments before mechanics validation.
2. Populate only needed active-tier fixed-ID recipes. Use one-to-three-ingredient abstractions, cow-milk-and-salt cheese with a longer timer, optional separate goat/mixed cheese definitions, and exact cup/bottle inputs. Furniture remains store-only; house upgrades consume explicitly named bricks/panels.
3. Simulate exact tier-valid recipes/branches, loose sources, all six fishing pairs per tier/habitat, <=26-heart legs, food/fainting, and price/wear/stock. Include roasting wood, pantry ingredients, ten-to-150 mission quotas, sacrificed Normal/prior-advanced material/house reserves, x1 workload, and hypothetical lower pickup rates in a report—never alter current full-stack testing to make it pass.
4. Fine-tune the already-implemented pricing references/curves and other pacing data only after mechanics checks. Any later pickup/drop revision is a separate deliberate balance change; preserve the 1,000-unit Mechanics Test profile permanently. Keep the confirmed price-category order and anchor definitions unless explicitly revised.
5. Play through from a fresh save without debug grants; ensure every region, recipe, and house gate has an achievable route.

**Exit check:** no circular dependencies, no mandatory premium-food/box purchase, no unexplained material with no source/use, and each new region adds a meaningful choice beyond a larger number.

### Milestone 18: Presentation, accessibility, and release preparation

Dependencies: a complete fresh-save progression pass.

1. Replace placeholders with consistent original art, clear quality icons, readable timers, resource hit targets, audio, and effects.
2. Add reduced motion, adjustable text/UI scale, alternative tap-to-collect, and readable nonblocking trade details. Preserve the confirmed exclusive 15-second duration and Return cancellation rule.
3. Implement audio/graphics/controls/language settings; localize through stable string keys rather than embedding text in rules.
4. Test device aspect ratios, interruptions, low storage, save migration, and graphics API compatibility.
5. Verify current store SDK/signing/build requirements, generate a signed release bundle when appropriate, and run release-build performance checks.

**Exit check:** tested release build, recoverable saves, completed device matrix, and a documented list of remaining limitations. Publishing is a separate explicitly requested action.

### Immediate next implementation task

Begin with milestones 0 and 1 only: build to Android, then forward-only path movement, avatar speed gestures, independent 360-degree camera orbit, and the distinct stop/resume rules. Next implement milestone 2's full-stack spatial pickups, including reachable resources behind the avatar. Do not start by authoring the full recipe catalog, all regions, or a complete settlement UI.

## 19. Verification and mechanics-limit matrix

Use focused EditMode tests for deterministic rules and PlayMode/device tests for interaction and lifecycle. Avoid tests that merely repeat a trivial field assignment. High-value test scenarios are:

| Area | Required cases |
| --- | --- |
| Quantity | 0 rejected; 1/999/1000 valid; 1001 splits or rejects by operation; checked large batch arithmetic |
| Stack growth | Boundary fits, merge enlarges beyond neighbors, mixed quality, same Q5 with different intrinsic values, ready/pending state, full stack touching full stack |
| Movement/camera | Full 360-degree orbit while walking/stopped, horizontal/vertical axis ownership, no camera-induced stop/steering, yaw persists, orbit disabled in exclusive/blocking UI, avatar hit area works from all yaw angles |
| Stop/resume | Harvest start resets speed; success/cancel/miss/tool break stay stopped; only fresh upward gesture resumes; inventory round trip restores old speed if permitted; inventory after harvest stays zero; current boots cap overrides stale snapshot |
| Collection | Nearby behind-player drops remain raycastable/in range after orbit; no behind-camera logical deletion, no distant/occluded reach; two touches on one bundle, failed placement, app pause during animation, full-stack mission remainder |
| Containers | Size/fit only and no weight rejection; zero starter hooks/pockets; ten maximum each; ten-to-zero transfer; 1.5x stack linear scale/2.25x rectangular area cap; backpack bounds, small screens, shelf split-one, overflow |
| Equipment | Distance-zero worn instance disappears; exact default per slot, gear bonuses removed, no spare auto-equip, three-heart/no-boots caps, current stamina clamp without refill, rag-pack/pocket/hook content transfer, simultaneous expiration, no lost contents/retained capacity, save recovery, infinite gear exempt |
| Stamina/passive animals | Walking-only drain; stationary aiming/harvesting no drain; progressive food/drink recovery with retained excess; camp/settlement progressive regen during menus/trade; max cap; passive movement/aim hits and misses; no player damage |
| Spacing/food | First opening camps reachable on three hearts/no food; region-one apple juice unlock; longer region-two legs require food; fixed authored rate independent of gear; every leg <=26 hearts; twelve-heart start + three ordinary six-heart servings safely covers the 26-heart example without waste |
| Faint recovery | Zero with ready food stops without faint; zero/no available food faints in place; distinct random 2-4 carried entries, full-stack loss, hooks/spares eligible/worn gear excluded; fewer-than-two/empty cases; penalty manifest commits once before animation; recover/wake without auto-walk or reload reroll |
| Harvesting | Normal-only tier one; Elite action savings at matching/lower tier with one-action floor; one wear/use per committed action; all eligible target tiers get Elite drop boost including +1/+2; Normal never boosts; +3 no start; revised massive curves/x50 |
| Material crafting | 3 branches -> 1 log, 3 logs -> 1 plank, 1,000 inputs leave correct remainder, no mastery, gathered-output recipes instant at x1000, crafted-only duration fixed, input quality does not control output |
| Craft mastery/stats | Levels 1/5/6/10, bulk finished-unit counts, no credit at start/repeated claim, Q5 cap, continued level-based base durability growth, quality bonus without furniture, additive base + quality + matching furniture breakdown, no direct C durability multiplier, no Q5 overflow, old output stats unchanged |
| Pricing | One-cash tier-one Q1 basic; level-ten reference Misc base exactly 6,000 before Q/durability; category order across four comparable-tier reference tables; mastery factor continues past level five; Q5 Misc 12,000; backpack 11,200 with stated bonuses; materials never level; nondurable items never get durability premium; C/U/Q not double-applied; full-stack/box totals use fixed point and 64-bit arithmetic |
| Craft timers | Food/Tool decrease by level, Wearable/Misc increase, progression seconds to hours, Food/Tool under 3,600 seconds for all batch sizes, instant materials stay zero, furniture never changes time |
| Crafting transactions | Stackable x1/x10/x100/x1000; Tool/Wearable/Backpack x1 only, forged bulk request rejected without costs; ingredient shortage/overflow, same-batch snapshot, unique instance, ready-time reload, queue ceiling |
| Elite upgrades | Nine exact Normal-instance + previous-tier advanced recipes; wrong tier/family/Elite input rejected; proposed partly-used Normal policy; no current-tier advanced add-on, no direct input-stat inheritance, no extra Normal XP, no mission/upgrade double-spend, input disappears at start/output once at readiness, full-queue/reload recovery |
| Exact recipe ingredients | Cow recipe rejects goat milk; mixed recipe rejects two cow or two goat units; wood/fiber/metal/fish variants never substitute; Autofill/manual/bulk use identical ID matching; same-ID stacks can combine under quality rules; recipe unlock/mastery remains separate |
| Recipe tiers/cooking | Unlock tier one accepts only 1; two accepts 1/2; three accepts 2/3; four accepts 3/4. Approved exact older flour/cheese/etc. work only in Food with source access; no substitution/future leak or nonfood exception. Roasts charge exact wood x quantity, later wood cannot be Pine by convenience; mastery/location/Q5 store quality never changes MaterialTier |
| Pine tools/fishing | Three explicit tier-one Normal recipes, x1/no Elite; four fish IDs per tier, six unique regional pairs, two profile species, habitat-specific nodes, sixteen distinct fuelled fish-roast definitions, discovered-species shop filters; no fourth loose icon |
| Missions | Four branches/one active, starter rewards/slots/prerequisites; quantities ten/50/100/up to150 by resource tier; 150 leaves 850 from 1,000; unique-tool counts not uses/stacks; upgrade input cannot also contribute; worn protection; previous-refinement and Normal unlock before Elite request; no duplicate rewards |
| Content scope | 24 regions/four six-region tiers; exactly three loose IDs/region and 18 unique/tier; only branch+pelt/hide/confirmed turkey-feather overlap; no loose log/meat/fish; identical map/spawn tables; early 10-14/later up-to-24 camps; seven tools/family, 24 lumber trees, no pending species dependency |
| Simplified recipes/store | Each cheese uses its fixed milk ID(s) + salt with Food timer/mastery; no mandatory culture/tannin; cups/bottles are exact Material inputs; bricks/panels upgrade house; furniture remains store-only |
| Trading | Exclusive 15 seconds; all other actions blocked; regen continues; Return settles unsold player listings at 70% once; natural shredder/expiry 80%; NPC/previously settled goods excluded; cancel/expiry race; spent session survives restart; actual departure/return refreshes |
| Warehouse | All sellable categories, two partial ten-unit exact-ID boxes, mixed profile/condition values, 100% cash/no extras, 10,000 cash -> 300 gold on tenth box, fractional carry, exact-once reload, unique-instance escrow |
| Shops/boxes | Fishery two distinct fish + one exclusive, Q5/cash, <=1,000 stock/slot/reset; source filtering and no reroll/refill on reopen/reload. Six typed box slots, physical Q5 vs mastery-one recipe grant, learned/reserved/within-box duplicates excluded, no eligible recipe fallback, recipe became-known-before-claim fallback, full-bag learning, exact-once claims, no raw exclusive/unknown finished-product backdoor, main progression never random-gated |
| Travel | One-time toll, forbidden region destination, repeated scene load, interrupted transition, correct return camp |
| Settlement | Eight grid sizes, six furniture types/level, duplicate purchase buff, one horizontal chest container up to 48 x 6, 12 shelf entries |
| Timers | Craft completion, 72-hour independent trees, pet cap, device clock forward/back, repeated resume |
| Save recovery | Missing definition, older schema, corrupt primary, newer unsupported schema, interruption at each critical transaction |
| Performance | 24 live bundles plus huge pending queue, full bag/large horizontal storage, x1000 crafts, all tree records, 20 scene round trips |

Additional acceptance checks:

- **Food:** eat six hearts at 3/3; verify six pending with no instant gain, no loss while standing capped, and progressive consumption while walking after inventory close. Test equal/higher/lower drain rates, queue exhaustion within a frame, different frame sizes, multiple servings without rate stacking, safe-area priority, twelve-heart premium pie with a three-heart cap, hat break, zero with pending food, suspension/travel/reload, and exact-once consume/enqueue. Simulate the 26-heart route with the proposed rate and retained surplus, including a partial start.
- **Recipes:** exactly 26 optional IDs = 21 Food + 5 Misc; eight added dishes recover 6/6/7/7/8/8/9/10 total hearts per serving. Region 1 without region 5 rejects oat dishes; discovered source regions plus accessible exact milk enable them. Check all later-region gates, ingredient quantities/yields, tier/pantry rules, known refinement chains, duplicate prevention, and Q5 bakery pie exclusion from the recipe pool.
- **Map:** check current-region header/marker, locked segments/camps, and discovered-region list below the first camp. Same-region travel selects the exact unlocked camp; cross-region travel always selects first camp, including a later-numbered already-discovered region. Verify selected-current-camp no-op, unpaid-border rejection, new map after arrival, correct Return to Trail, preserved food queue, and interrupted travel without duplicate arrival/visit refresh.

Helpful developer tools: searchable item/recipe browser; fill/clear selected container; grant currencies; equip rank; mission selector; seeded loot preview; simulated UTC clock; teleport to camp; show ownership/transaction ledger; export economy report. Keep debug commands separated from normal player UI and removable/disabled in release builds.

## 20. Confirmed answers and delegated implementation defaults

The developer answered the earlier questions; these decisions supersede conflicting proposals above. Questions 5 and 9 delegate implementation defaults, not new approval gates.

1. **Craft presentation:** keep crafting/inventory open. On Craft, center item/bulk on the 3 x 3 grid with quality, applicable durability and timer briefly, then drop into the backpack. Proposed hold: one second. Timed products occupy bag space and remain unusable until ready; the timer is not a wait before delivery. Portrait remains the working layout proposal; verify readability on device.
2. **Placement:** free placement only, fixed orientation, no item rotation.
3. **Durability:** tier-one level-one Normal axe 10, slingshot 6, rod 10 uses. Add ten per tier within each family. Elite has exactly twice the same-tier Normal baseline; only tiers 2-4 have Elite. Existing mastery/quality/eligible furniture layers apply afterward. Harvest action counts remain the section-8.2 starting proposal.
4. **NPC prices/profiles:** discounts toward the shredder are enabled; use the proposed 110%-to-80% curve. NPC-generated gear is creation level one, random Q1-Q5, with no furniture buffs. Player listings retain their actual profile; normal shops remain Q5. Item furniture buffs belong only to player-crafted goods.
5. **Pending products/overflow — delegated default:** bag-owned timed products with a timer badge; retain eight outstanding records. Reserve valid bag space without rearranging items; route genuine overflow visibly to the persistent tray. One owner and one completion/mastery credit across reloads.
6. **Missions:** exact item ID and quantity only, with no quality/durability/mastery threshold. Preserve ready/available ownership checks and worn-gear protection; partially used tools of the required ID count.
7. **Chests:** one horizontal scrolling free-placement container like the backpack, no pages. Proposed tier dimensions: 6/12/24/48 wide by six high; virtualize offscreen views.
8. **Warehouse:** any ready sellable item, including unique gear. Two boxes, ten units/instances per exact ID. Pay full quoted cash with no extra/furniture bonus, then 3% of the last ten boxes' paid cash in gold. No 100-cash-per-gold divisor: 10,000 cash -> 300 gold. Preserve fractional carry. Mixed profiles per ID and condition-adjusted used-gear quotes are delegated valuation defaults, not mission filters.
9. **Travel/reset policy — confirmed update:** unlocked map travel remains free, with existing destination/toll rules. Camp trade refreshes only after a genuine departure/return or a different camp; the current-camp no-op never refreshes it. Settlement material/ingredient shops refresh each actual settlement departure and return, with no timer. Balloon offers reroll on shop close/reopen, including the displayed special clothing, within the same camp visit. Ordinary shop reopen/reload preserves the settlement visit; balloon reload preserves the open session. Paid purchases/box manifests never reroll. This update supersedes the earlier 30-minute default; do not add a wait to ingredient/material shopping.

**Additional confirmed release rule:** each gathered world object moved into inventory gives one item after testing; mechanics tests stay at 1,000. See section 7 for source boundaries and migration. Section 18 must include a fresh-save one-item playthrough before finalizing draft costs/density/mastery pacing. Existing limits (24 regions/four tiers, twelve-heart meter, <=26-heart legs, exact ingredients, six box rewards and x1-only gear crafts) stay in force. Other explicitly proposed recipe/recovery rates remain tuning data.

## 21. Sources and reference limitations

- Primary project inputs: [`Trail.md`](Trail.md), `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/QualitySettings.asset`, `Packages/manifest.json`, and the Mobile renderer assets.
- [The Trail community item reference](https://the-trail.fandom.com/wiki/Items): direct access failed during this review; indexed excerpts were available. They support broad comparisons such as raw resources feeding equipment, sale goods, and house materials. This document does not claim a complete audit of the wiki or reproduce its recipe catalog. The proposed regions, quantities, recipes, and balance tables above are original design suggestions derived from this project's requirements.
- [Unity Splines documentation](https://docs.unity3d.com/Packages/com.unity.splines@2.8/manual/index.html): path-authoring option for the forward-only motor. Select an editor-compatible package release through Package Manager rather than assuming a particular patch.
- [Unity 6.4 URP performance guidance](https://docs.unity3d.com/6000.4/Documentation/Manual/urp/configure-for-better-performance.html): basis for disabling unnecessary HDR/depth/opaque textures and profiling rendering cost. Actual performance remains unmeasured.
- [Unity Android environment setup](https://docs.unity3d.com/6000.4/Documentation/Manual/android-sdksetup.html): toolchain reference for milestone 0.
- [Input System on-screen controls](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/OnScreen.html): optional controls for accessibility; standard joystick movement is not part of this design.
- [Cinemachine Third Person Follow](https://docs.unity.cn/Packages/com.unity.cinemachine%403.1/manual/CinemachineThirdPersonFollow.html): optional follow-camera implementation.
- [NC DEQ Red drum](https://www.deq.nc.gov/about/divisions/marine-fisheries/public-information-and-education/species-profiles/red-drum) and [NOAA Red snapper](https://www.fisheries.noaa.gov/species/red-snapper): habitat guidance for separate coastal fishing-node presentation. The sixteen-fish progression tiers/pairings are game design proposals, not real-world biological rankings.

This document is a design and implementation plan. Completion checks describe future work, not tests already passed. Only documentation was changed in this task.
