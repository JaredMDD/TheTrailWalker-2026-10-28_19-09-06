# TheTrailWalker: Revised Design and Step-by-Step Development Plan

**Progression logic revision - November 6, 2026:** construction bridges now precede settlement arrival, mission execution order is separate from stable codes, barrel filling has explicit pantry permissions, and exotic-pet capacity counts opportunities separately from test stacks. See Part 1 sections 9.0.1 and 11.6.1; Part 2 sections 10.1, 15 and 16.5.1. **Design updated; gameplay implementation and current test/build/device status are not certified by this edit.** The other tool should review [the logic and testing handoff](Docs/PROJECT_HANDOFF.md#logic-review).

Updated: November 6, 2026 (progression logic: early construction bridges, explicit mission execution order, barrel permissions, and exotic-pet capacity; implementation review pending).

**Progression beyond tier 4:** [Recommendations Part 2](PROJECT_RECOMENDATIONS_PART_2.md) collects the already reserved tier-5–8 advancements, open design data and proposed expansion milestones. Current implementation scope remains the four-tier base.

**Confirmed first-person gathering revision:** Milestone 2 showed that short pickup reach leaves too little reaction time at higher walking speeds. Change the trail viewpoint to first person and allow visible, unobstructed pickups nearly as far as view distance and reliable touch detection permit. The prototype's 8 m radius and 15 m rear window are superseded; measure a practical reach on phones at maximum test speed rather than inventing a final distance. Preserve independent 360-degree look, forward-only travel, stop/resume behavior, full-stack test pickups, and ownership and line-of-sight protections. Resource gestures must start below the top system notification/gesture area, respecting device safe areas, screen height and insets without intercepting system gestures. Define an accessible first-person speed-gesture surface that does not conflict with UI, resources or camera look. This supersedes earlier third-person/avatar-hit-area and short-range instructions. The Milestone 1 and 2 prototypes now implement this revision; broader device and maximum-speed usability testing remains pending.

**Confirmed development scope — debug and testing only:** the 24 regions and four currently defined resource tiers are the initial base, not the complete or final product. Higher-tier materials and content still require definition and implementation. Keep full-stack 1,000-unit pickups, debug grants, travel/rank selectors, and other test tools available throughout the current milestones. Completing a test milestone does not authorize rebalancing, removal/disablement of debug grants, switching to release collection quantities, release preparation, or deployment/publishing. Each of those phases requires an explicit future user instruction. Local development APK builds and device installation remain part of testing. References below to `Release` describe deferred design intent, not an active implementation or acceptance requirement; this scope supersedes earlier automatic post-testing transitions.

**Latest confirmed additions:** balloon boxes gain more optional, stronger Food recipes using ingredients from discovered regions; the camp map shows current-region camps/segments and previously discovered regions below its first camp; same-region travel arrives at the selected camp, while cross-region travel arrives at the destination's first camp. Consumed food/drinks restore stamina progressively; recovery beyond the meter capacity is lost (revised October 30, 2026), so food never builds a reserve. See sections 4.4.3, 9.4.2, and 12.3. Recipe amounts, recovery rates, and bakery selections below are proposed starting values.

**Confirmed section-20 answers:** crafting stays open and presents the product at the center of its 3 x 3 grid before dropping it into the backpack with its timer; inventory uses free placement without rotation; tool durability follows family/tier and doubled Elite baselines; NPC discounts are enabled with level-one random-quality gear and no furniture bonuses; missions match only exact item IDs/quantities; chests use one horizontal scrolling container; warehouse accepts every ready sellable item and pays 100% cash plus 1 gold per 1,000 cash per ten completed boxes (revised October 31, 2026 from 3%), without extra bonuses. Full-stack testing remains active; the one-item collection rule is deferred until explicitly requested, with no automatic switch after testing. Sections below incorporate these answers and the delegated defaults for questions 5 and 9.

**Latest pet/truffle revision:** preserve the house-level-three pet shop, house-front pet home for selecting one owned pet, and separate home-front idle-reward collection container. Section 13.5 proposes six pets with distinct loot weights, acquisition and source filters. Add exact Truffle as a tier-two gathering ingredient; the proposed region-10 assignment replaces loose Lavender, retaining lavender in discovered farm stock and keeping three loose resources per region. Section 9.4.3 adds one mission recipe and two optional box recipes; the box collection becomes 26 recipes (21 Food, 5 Misc). Pet species, weights, prices and recipe values are proposed starting data.

## 1. Purpose, authority, and current status

This document analyzes `Trail.md`, reconciles conflicting rules, proposes additional resources and recipes, and defines an implementation sequence for a solo developer learning Unity. `Trail.md` remains the original design record and has not been edited. No gameplay code, packages, or Unity settings were changed while preparing this plan.

Labels used below:

- **Requirement:** a rule from `Trail.md` to preserve.
- **Confirmed:** an explicit subsequent instruction from the developer.
- **Recommendation:** a proposed refinement, not an already approved replacement of a requirement.
- **Starting value:** editable test data, not a promise of final game balance.
- **Deferred:** outside the current debug/testing scope; testing completion alone does not activate it. Rebalancing, debug removal, release preparation, and deployment require an explicit future user instruction.

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

**Latest regional/recovery revision:** each of 24 regions highlights three different loose-gathering resources: 18 unique loose resources per six-region tier, 72 across the active campaign. Each region also has its tier's tree/hunting/fishing interactables. Branches and the tier's pelt/hide are the baseline loose/interactable overlaps; explicit tree-food overlaps are also allowed (section 7.1.1); tier three uses **turkey feathers**, as confirmed in follow-up. Logs, meat, other hunting outputs, and fish are not extra loose-table entries. Early regions have 10-14 camps, later regions grow toward 24. The first region unlocks food before longer food-requiring routes. Zero stamina with no pending food recovery triggers fainting, recovery, and random loss of 2-4 eligible carried entries/stacks, excluding worn gear; carried raw food never prevents it, while carried prepared food is auto-eaten first (section 4.4.2).

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
| Warehouse gold | Previously ambiguous exchange denomination; then 3% | **Revised October 31, 2026:** 1 gold per 1,000 cash paid for ten boxes; 10,000 cash -> 10 gold; integer cash remainder carried; no extra bonuses |
| Furniture grants a buff on first purchase | The first-purchase behavior is now confirmed; **revised November 7, 2026:** the buff applies only while a piece of that type is placed | One buff per furniture definition, active while placed, none while in house storage; duplicates, moving or rebuying never grant another copy (section 13.2) |
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
| Starter, no boots/hat | 1.5 | 3 | 0 | 0 | 6 x 4 |
| 1 | 2.25 | 4 | 2 | 2 | 8 x 4 |
| 2 | 3.0 | 6 | 4 | 4 | 10 x 4 |
| 3 | 3.75 | 9 | 7 | 7 | 12 x 4 |
| 4 | 4.5 | 12 | 10 | 10 | 14 x 4 |

**Two sets per tier from tier two (user, November 5, 2026):** the rank values above are each tier's limit, which **Set B** reaches. **Set A**, built from the block's first three regions, sits halfway from the previous limit, rounded up. Wearables therefore store explicit capability values; see section 9.6.1.

Requested speed ranges from zero to the current boots cap. **Confirmed defaults:** no boots gives slow walking (**1.5 m/s**; raised from 0.8 to 1.0 on October 30 and 1.5x to 1.5 on October 31, 2026); no hat gives three stamina hearts; rag shirt/pants give zero pockets/hooks and the rag pack provides starter area. Initially, tools/resources must fit in the backpack. Highest active-tier boots cap (**confirmed 4.5 m/s**; raised from 1.6 to 2.0 and then to 3.0 on October 30, 2026 because walking felt slow, then 1.5x to 4.5 on October 31, 2026; boots add 0.75 m/s per rank), hats at twelve hearts, shirts at ten pockets, and pants at ten hooks. Merchant gear cannot exceed those caps. Starter rags are nondepleting fallback representations with no sellable instance or extra bonuses. Backpack dimensions are virtual layout units, not kilograms or visible slots.

### 4.4 Stamina and wear

**Confirmed:** hearts represent stamina, not health. Only actual forward trail distance drains it; food/drinks replenish it. Keep distance-based costs so slow walking or harvesting stops do not cost extra. The earlier **2 hearts/km is an opening-region test rate**, not a mandatory global rate. At that rate a 250 m tutorial leg costs 0.5 heart. Later segments author a fixed distance/drain budget, capped at **26 hearts per leg** in the highest tier; section 6.2.4 provides proposals. Standing/menu time, aiming, loading, and teleportation never drain stamina.

At zero stamina, stop. Pending recovery from food already eaten is delivered first. **Revised October 30, 2026:** carried raw food never prevents fainting; carried ready prepared (crafted/bought) food is eaten automatically, exactly one serving, only when no pending recovery remains. With neither, faint and apply the recovery/loss procedure in section 4.4.2. The earlier free camp-rescue/no-loss proposal is removed. Camp/settlement regeneration remains continuous, including trading/menus, with no Rest action. The confirmed rate is 1/3 heart/second (raised from 0.25), so 15 active trade seconds restore 5 hearts unless capped. Food/drinks start progressive recovery on use, continuing after inventory closes and walking resumes; unused recovery is retained (section 4.4.3). The global meter maximum stays twelve.

Regeneration is a simulation service, independent of UI input locks and trade animation. Default recommendation: passive safe-area recovery counts only active foreground camp/settlement time; food recovery also counts active foreground trail time. App suspension freezes both recovery services and the remaining trade countdown together. Offline stamina recovery is not required by this revision. Persist current stamina and remaining trade time so reopening cannot restart either interval.

Animals never attack or damage the player. They can walk within a local resource area; hunting requires the user to aim at the moving target, then commit a shot. Use resource harvest progress, not a player combat/health system. Recommendation: a committed miss consumes one tool use but makes no harvest progress; canceling before shot commitment consumes none. Validate tool eligibility before entering aim mode and again before committing a shot.

Tools lose uses only for committed actions. **Confirmed:** worn hats, boots, shirts, pants, and backpacks lose durability by actual trail distance walked, never by camera rotation, menu time, stationary harvesting, teleportation, or settlement/camp time. Example rank durability remains `2 + 2 * rank` km before mastery/quality/furniture bonuses. **Confirmed reference (October 30, 2026):** in the original game, tier-three gear at mastery level 10 and Q5 reached about **10 km**; tier four must be clearly better. Calibrate base durability and the mastery/quality/furniture bonuses so tier-three L10 Q5 lands near 10 km. *Proposed:* tier-four L10 Q5 around 14 km. Prototype test gear (180 m) is deliberately short for testing only. Carried spare wearables do not wear down. Infinite-durability merchant gear remains exempt.

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

**Confirmed content-preservation rule (October 30, 2026):** remove exhausted equipment while transferring its contents transactionally. Default shirt/pants have no surviving pockets/hooks: their pocket and hook items are sent directly above the backpack opening and stored in the bag (falling in under the deferred gravity model). Any item that does not fit **falls to the trail as if dropped by the player**, keeping its exact quantity, quality and durability and following the dropped-item rules: it can be picked up again and is deleted if still uncollected when the segment's next drop cycle rolls (section 7.3). This supersedes the earlier persistent overflow tray for equipment expiry; the prototype's overflow list and CLAIM OVERFLOW button remain a debug stand-in until this is implemented. *Recommendation:* apply the same rule to bag items displaced when an expiring backpack falls back to the rag pack's area. The exhausted item itself is removed, never stored or dropped. Notify the player without retaining expired-item bonuses to avoid the downgrade.

For a bag expiration, remove the old container/instance and transfer its contents into the rag pack/tray in the same operation. For several items expiring on the same movement update, compute all defaults and plan one transfer before committing, rather than dropping items during sequential resizes. Persist equipment removal, recalculated caps, clamped speed/stamina, and new ownership together so reload cannot restore a destroyed item or duplicate its contents. The earlier recommendation to leave broken storage gear equipped is superseded.

### 4.4.2 Fainting, inventory loss, and wake-up

**Confirmed trigger (revised October 30, 2026):** on the trail, stamina reaches zero and no positive consumed-food recovery remains pending. Raw gathered food (berries and similar ingredients) is never eaten automatically and does not prevent fainting; the player must track stamina and eat manually. Carried ready **prepared** food (crafted, or bought such as bakery goods) is eaten automatically at zero stamina to prevent fainting; only when none is carried does the player faint. This supersedes the earlier rule that any accessible ready food/drink prevented fainting. Pending crafts, chest/shelf/tray contents, inedible raw ingredients, and food already consumed by a mission do not count. This is recovery, not combat/death. Stop at the current path position, clear walking intent, and play faint/recovery/wake animations. Lock normal pickup/harvest/craft/travel/equipment actions through this state so they cannot race the loss manifest; allow application pause and nonblocking loss/recovery information.

Roll loss count 2-4 and select that many distinct eligible **inventory entries**, without replacement. A selected stack loses its whole remaining quantity, not 2-4 units. Eligible entries include backpack resources/food, pocket stacks, tools in backpack/hooks, and spare clothing/backpacks. Exclude currently worn hat/boots/shirt/pants/backpack, wallet balances, home storage, unclaimed rewards, and craft/trade escrow. If fewer than the rolled count are eligible, lose only those available; an empty inventory loses nothing. Uniform selection per eligible entry is a proposed initial rule, not weight/value-based selection.

Persist a unique faint-event ID, selected item/stack IDs and quantities, committed removals, recovery state, and position **before** showing loss animations. Animate up to the actual 2-4 selected entries slipping out and disappearing; these are presentation objects, not collectible world drops. A 990-unit stack is one entry and all 990 disappear if selected. Do not reroll or charge another penalty on reload/resume, create replacement tools, or reverse already-completed mission contributions.

Stamina progressively refills while fainted, which is a specific recovery exception to normal no-regeneration trail standing. Recover at the confirmed **1/3 heart per active foreground second** (raised from 0.25 on October 30, 2026) up to the current hat-derived maximum: three hearts take 9 seconds and twelve hearts take 36 seconds from zero. Freeze/save on app suspension. Wake only after recovery and penalty commit are complete, preserve current equipped bonuses, and remain stopped until a fresh upward speed gesture. No teleport, automatic walk, or recipe/currency reward. Tool loss can leave harvesting unavailable, but loose gathering remains possible for rebuilding through known recipes.

### 4.4.3 Progressive food recovery (no reserve beyond the cap)

**Confirmed:** eating/drinking consumes one serving and adds its full authored recovery, including the applicable food-recovery furniture bonus, to a persistent pending-recovery queue. It never adds all hearts immediately. Food values in every table mean **total hearts over time per serving**, not a raised stamina cap. The player can open inventory, eat, close it, and resume the valid pre-open walking speed while food recovery and distance drain run together. Existing harvest/manual/exhaustion stop conditions still take priority.

**Starting implementation proposal:** one FIFO queue at **1/3 heart/active second** (confirmed shared recovery rate), shared by foods and drinks; six hearts take 18 seconds of actual delivery. Multiple servings add their remaining amounts without multiplying the delivery rate or replacing the active serving. Display current/max stamina, pending food hearts, and recovery rate on the HUD and item popup. Snapshot each serving's effective recovery when consumed; later furniture changes do not revalue it.

Integrate walking drain and recovery continuously in one simulation step (split at queue exhaustion, meter-cap, or zero-stamina boundaries). Spend pending food only for hearts actually supplied to the meter or concurrent walking cost. **Confirmed revision (October 30, 2026):** food never builds a reserve. A serving is delivered at the fixed shared rate from the moment it is eaten; below the cap it raises hearts, at the cap it can at most balance concurrent walking drain, and any delivery that would exceed the maximum is **lost**. Eating at a full meter while standing therefore wastes the serving as it delivers. Safe-area passive recovery contributes independently at its existing rate; their combined excess at the cap is likewise lost. This supersedes the earlier rule that delivery paused and the queue was retained at the cap. Passive recovery never spends the food queue. A lower cap after hat expiration clamps only current stamina and preserves pending food. Pending hearts are a consumable reserve, never extra visible meter capacity.

Examples: at 3/3 hearts, a six-heart meal delivers for 18 seconds; standing, all six are lost; walking with 0.10 heart/second drain keeps the meter at 3/3 and the remaining delivery is lost. At 2/3 with 0.10 heart/second walking drain, the 1/3 food rate yields about +0.23 heart/second until capped, then only balances drain while the serving lasts. If walking drain exceeds recovery, stamina still falls; food does not promise unlimited walking. At zero with pending positive recovery, stop without fainting, continue recovery, and allow a fresh upward speed gesture once stamina is positive. Faint only when zero, no pending recovery, and no accessible ready consumable remain; this qualifies the trigger in section 4.4.2. Resolve the final recovery tick before evaluating fainting.

Food recovery continues during foreground inventory, harvesting, camp/settlement menus, and trading; trail standing has no free passive regeneration. Freeze delivery during loading/app suspension, save queue balances/order, serving snapshots, fractional timing, and consumption transaction IDs, and resume exactly once without offline food recovery, duplicate consumption, or queue reset on travel. Saving consumption and queue insertion is atomic. Departure previews simulate pending recovery plus usable carried food, walking rate, and timing; delivery beyond the cap is wasted, and the same serving is never counted twice. A 26-heart leg still requires sufficient total provision **and** a viable recovery schedule.

## 5. Inventory, stacks, and item identity

### 5.1 Deterministic spatial inventory

**Confirmed:** item size, nonoverlap, bounds, and backpack capacity determine fit. Weight is not a limit, speed penalty, stamina penalty, or secondary encumbrance system. Do not add a weight budget to transfer validation. Fainting is the explicit inventory-loss mechanic; capacity failure still never silently destroys an item.

Keep the slotless appearance, but avoid simulated rigidbody piles in the UI. Represent each entry by a 2D position and rectangular footprint inside a logical container. Hidden placement precision is acceptable; visible slot cells are not required. Validate bounds and overlap deterministically. **Confirmed:** free placement only, with fixed item orientation and no rotation controls. Bounds/overlap checks use that fixed footprint.

Quick storage first attempts a compatible merge, then searches for free space. Manual drag validates the intended placement and merges when compatible stack footprints touch. If a merge enlarges the result, validate the new footprint before consuming the source. Partial merging is allowed up to 1,000; leave the remainder at its prior valid location. A failed pickup stays on the trail and cannot be collected twice by overlapping inputs.

Stack key: definition ID + quality + ready state + output-stat profile and any gameplay-relevant variation. Include per-unit intrinsic price and any other differing functional stats in that profile: two Q5 goods made at recipe levels five and ten can have different prices and must not silently merge. Equivalent profiles may merge even if provenance differs. Pending craft batches never merge with ready items. Tools, clothing, and backpacks have unique instance IDs and never merge.

### 5.2 Bounded stack-size growth

**Confirmed:** quantities increase footprint/model size up to **1.5x base linear model scale** for every stackable item. Quantity cap stays **1,000 units**; quantity and visual scale are separate limits.

**Confirmed stack identity revision (October 30, 2026):** stacks are keyed by **item ID** only (plus readiness, so a pending craft never joins ready items), not by quality. Each stack tracks its contents as a **pile** of quality/profile portions with quantities, ordered from lowest quality, so price and quality information is kept without splitting stacks. A pickup gives **all** of its units: it tops up existing stacks of that ID up to 1,000 and the remainder becomes a new stack in free bag space; only a full bag leaves the rest on the trail. Edibles are consumed **lowest quality first**. When a stack cannot grow in place (its larger footprint would overlap a neighbour or leave the bag), only that stack moves to free bag space; if none exists it is topped up as far as it fits, and the rest becomes a new stack. Other items never move in the current fixed-position bag. **Confirmed for the gravity/collision inventory:** once that model is implemented and tested, a growing stack pushes surrounding items aside by collision instead of relocating or being limited by them; the relocation/partial top-up rule is the interim behaviour. This supersedes the earlier rule that different qualities or price profiles form separate stacks. *Recommendation:* other consumption (recipes, missions, sales) also takes the lowest quality first unless the player chooses specific portions; crafted-output quality still never inherits ingredient quality (section 7.1.1).

Recommended curve: `linearScale(q) = clamp(1 + 0.5 * log(q) / log(1000), 1, 1.5)` for quantities 1-1,000. It produces 1x at one unit and 1.5x at 1,000, with at most 2.25x base rectangular footprint area; the 1.5 cap is per dimension, not an area multiplier. Remove zero-quantity entries before evaluating the logarithm. Use the same capped growth for models and fit; never apply it twice. Full-stack pickups use maximum scale during testing. Shelf fitting and whole-UI scaling are presentation transforms, not extra capacity.

**Confirmed resource size standard (October 30, 2026, from the meshed region 1-2 resources):** these sizes are the standard for further resources with a similar form factor; only tools, equipment, certain foods and Misc items may be larger. Bag footprint in logical units (width x height) and trail size relative to the footprint:

| Form factor | Reference item | Bag footprint | Trail size |
| --- | --- | --- | --- |
| Apple-sized: stones, ores, berry-type fruits, similar | Apples (also Flint, Pine cone, Red berries) | about 420 x 480 (largest side 480) | 80% |
| Flat/soft bundles: pelts, fibres, cloth | Rabbit pelt (also Hemp) | 850 x 300 | 60% |
| Branches and sticks | Pine branch | 1200 x 200 (mesh slimmed) | 100% |
| Logs | Pine log | 1300 x 650, slightly larger than a branch | 100% |

Footprints keep each mesh's proportions; the footprint is the collision box and the drawn model is about 10% larger. Earlier nominal shapes below are superseded where they conflict. **Applied to regions 3-6 loose resources:** apple-sized Clay 480x279, Rock salt 480x332, Limestone 480x280, Strawberries 443x480, Mushrooms 480x322, Pebbles 480x240, Sand 480x240; soft or stalk bundles (largest side 850, 60% on the trail) Nettle fiber 850x300, Tea herbs 850x593, Oats 652x850, Reeds 672x850, Wheat 422x850. Harvest drops (meat, fish, bone, fat) keep their own sizes as allowed larger foods. Pine branches stand upright on the trail so they are easy to touch; their bag picture stays horizontal.

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
| Q2 | 0.20 |
| Q3 | 0.40 |
| Q4 | 0.60 |
| Q5 | 0.80 |

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

See [Part 2: Progression Beyond Tier 4](PROJECT_RECOMENDATIONS_PART_2.md) for the consolidated expansion backlog.

| Pending tier | Reserved wood / wild animal | Status |
| --- | --- | --- |
| 5 | Maple / deer | Backlog only; no region assignment or required recipe |
| 6 | Cedar / mountain goat | Backlog only |
| 7 | Redwood / buffalo | Backlog only |
| 8 | Ebony / bear | Backlog only |

Do not populate these tiers' runtime loot, tools, missions, shops, mystery boxes, pet rewards, or house costs in the current build. Keep their ideas in a design backlog; no expansion art/recipes/classes are needed now. Do not reserve current speed/hearts/pocket/hook caps for those future tiers. Existing IDs in a test save should be migrated or quarantined safely if content is disabled, never silently reinterpreted as another species.

### 6.2.2 Three featured loose resources per region

**Confirmed interpretation:** six regions x three featured resources = **18 distinct loose-gathering item IDs per tier**, including its branch and pelt/hide entry. It is not 18 plus those two. Each region's three-item set is different, with no repeated featured loose item within its tier. The draft below also uses 72 distinct IDs across all four tiers. Existing recipes must use these exact named IDs, not generic interchangeable materials.

Every region separately contains its current-tier tree, passive animal, and suitable fishing interactables. Logs, raw meat, fish, fat, bones, eggs/sap and other interaction-only rewards do not occupy a featured loose slot. Branch + pelt/hide are the baseline overlaps, with explicitly authored tree-food overlaps permitted by section 7.1.1; **tier three's second overlap is turkey feather**, explicitly confirmed instead of a nonexistent turkey pelt. Boar hide is tier four's pelt-family equivalent. Tree food ingredients are mandatory under section 7.1.1. An explicitly authored food ingredient may share TreeHarvest and LooseGathering sources using the same exact ID; this exception does not add loose slots or authorize unrelated animal overlaps.

**Camps per region (confirmed by the user, October 31, 2026):** tier one 10-14 camps per region, tier two 12-16, tier three 14-18, tier four 16-20. Each region's count is random within its tier's range, not incremental (for example 13, 10, 11, 14). **Past tier four the count does not increase (user, November 5, 2026):** tiers 5-8 use 16-20 camps per region too ([Part 2](PROJECT_RECOMENDATIONS_PART_2.md) section 7). The table's camp counts were rolled once (seed 20261031) and are kept for later region map planning; Dawn Plains is built with 10.

| Region | Tier | Three featured loose item IDs, shown by display name | Intended recipe uses | Proposed camps |
| --- | --- | --- | --- | --- |
| 1 Dawn Plains | 1 | Pine branches; Rabbit pelt; Apples | Pine logs/tools; slingshot/toy; apple juice | 10 |
| 2 Windward Hills | 1 | Hemp fiber; Flint; Pine cones | Hemp cloth; pine axe; pine-cone ornament | 12 |
| 3 Mistwood | 1 | Nettle fiber; Clay; Rock salt | Thread/stuffing; bricks/cups; curing/food | 11 |
| 4 Fern Hollow | 1 | Limestone; Strawberries; Mushrooms | Tiles; yogurt drink; roasted mushrooms | 13 |
| 5 Cloudstep Mountains | 1 | Oats; Pebbles; Red berries | Oat cakes; polished-pebble charm; berry cakes | 10 |
| 6 Reedwater Delta | 1 | Reeds; Wheat; Sand | Reed braid; wheat flour; upgrade mortar | 14 |
| 7 Whispering Marsh | 2 | Birch branches; Beaver pelt; Blackberries | Birch logs; beaver leather/toy; blackberry juice | 12 |
| 8 Amber Riverbanks | 2 | Flax fiber; Copper ore; Carrots | Flax cloth; copper ingot/fittings; carrot soup | 13 |
| 9 Bluewater Basin | 2 | Barley; Quartz; Blueberries | Barley flour/soup; glass/cut quartz; blueberry jam | 13 |
| 10 Sunlit Uplands | 2 | Sandstone; Truffle; Flax seeds | Tiles; truffle meals; flax oil | 15 |
| 11 Mirror Lakes | 2 | Rice; Tomatoes; Honey | Fish bowls; stew; honey drinks | 13 |
| 12 Frostveil Taiga | 2 | Tin ore; Yellow ochre; Cattail fiber | Bronze; painted birch carving; cattail cord/fishing charm | 13 |
| 13 Golden Grasslands | 3 | Mesquite branches; Turkey feathers; Corn | Mesquite logs; feather garment/toy; corn flour | 14 |
| 14 Ember Canyons | 3 | Cotton fiber; Iron ore; Beans | Cotton cloth; iron tools; stew/chili | 18 |
| 15 Red Dune Desert | 3 | Granite; Chickpeas; Pumpkins | Tiles; chickpea stew; porridge | 18 |
| 16 Hidden Oasis | 3 | Slate; Turquoise; Soybeans | Tiles; turquoise jewelry; soy milk | 14 |
| 17 Windswept Plateau | 3 | Sugar cane; Green chili; Sunflower seeds | Sugar; turkey chili; sunflower oil | 16 |
| 18 Snowmelt Highlands | 3 | Silver ore; Wild mint; Agave fiber | Silver jewelry; mint tea; agave cord/desert charm | 15 |
| 19 Sunset Savanna | 4 | Oak branches; Boar hide; Potatoes | Oak logs; boar leather; stew | 16 |
| 20 Stormwatch Cliffs | 4 | Marble; Gold ore; Cacao beans | Tiles; gold jewelry; cacao powder/drinks | 17 |
| 21 Crescent Beaches | 4 | Basalt; Emerald; Coffee beans | Polished stone; emerald jewelry; coffee | 20 |
| 22 Rainforest Coast | 4 | Obsidian; Ruby; Vanilla pods | Obsidian pendant; ruby jewelry; vanilla drink | 19 |
| 23 Silverwater Estuary | 4 | Sapphire; Lapis lazuli; Seaweed | Sapphire pendant; lapis bracelet; seaweed rice parcel | 17 |
| 24 Coral Shores | 4 | Diamond; Coconuts; Bamboo fiber | Diamond necklace; coconut drink; bamboo cloth/coastal shirt | 19 |

The featured resources are the normal loose spawn table for that region; their weights can differ between segments. Older materials can remain available through revisiting regions, camps' offered items, discovered settlement stock, and the baseline interaction overlaps and explicitly authored tree-food overlaps (section 7.1.1). Do not casually add a fourth loose type or repeat old featured IDs in later maps. Map cards show exactly the three resource icons in this table, separately from tree/animal/fishing indicators, plus locked/discovered state. A fishing point can use its local tier's valid species without making fish a fourth featured resource.

The names are an authored proposal, not all new production assets to build immediately. Each item has a useful seed use; preserve that requirement before finalizing a slot. If a material is removed, update the region icons, missions, loot tables, shop filters, and exact recipes together rather than leaving a decorative/useless collectible.

**Truffle source proposal:** `ingredient.truffle` (display Truffle), material tier two, is a stackable raw cooking ingredient, not a ready consumable. It is one of region 10's three normal loose resources, with a proposed 15% loose spawn weight (Sandstone 45%, Flax seeds 40%). No tool, pig, new digging mechanic, or paid ingredient is required. Collect one per object in Release and 1,000 in MechanicsTest. Eligible pigs and other pets can supply it only after region 10 is discovered. The same exact ID is used everywhere; never substitute an ordinary mushroom.

To preserve the existing three-per-region/18-per-tier/72-total constraint, this is a **proposed source reassignment**: Lavender leaves region 10's loose table and becomes a Q5 cash farm-catalog herb unlocked by discovering region 10. Keep its exact existing ID/material tier and the Lavender pouch recipe; its source label and all future optional requests must point to that farm catalog. Lavender does not enter pet loose-material tables. Source validators and region-10 map icons replace Lavender with Truffle together. Truffle remains a main regional ingredient, not a global pantry exception; its recipes use unlock tier two or three.

**Confirmed source reassignment (October 30, 2026):** Mushrooms are a tier-one recipe ingredient (Roasted mushrooms; the tier-two Beaver mushroom pot) that had no source. `ingredient.mushrooms` (display Mushrooms, stackable raw cooking ingredient already defined with its mesh among the tier-one crafting inputs, now apple-sized at 480x322 under the section 5.2 standard) replaces Tea herbs as one of **region 4 Fern Hollow's three loose resources**. **Tea herbs** keep their exact ID and material tier and become a Q5 cash **farm-catalog** herb unlocked by discovering region 4, like Lavender for region 10. Every tea-herb recipe (Herb trout, Rabbit barley soup, Salmon rice parcel, Honey herbal tea, Bottled honey tea) now draws on the farm source. **Mission replacement (confirmed direction):** Explorer E6 no longer leads to a tea-herb reward; it becomes 10 Mushrooms -> Roasted mushrooms, which also gives the formerly unplaced Roasted mushrooms recipe its unlock. Herb trout moves to Explorer E8, gated on farm Tea herbs (section 11.3). The tier-one recipe data (`Tools/Art/tier01_recipe_specs.py`, `Tier01Recipes.json`, `RecipeArtCatalog`) records Mushrooms as a region 4 loose source and Tea herbs as a farm source, and no longer blocks Roasted mushrooms. Source validators and region-4 map icons replace Tea herbs with Mushrooms together; Tea herbs do not enter pet loose-material tables, and the three-per-region/18-per-tier/72-total counts are unchanged.

### 6.2.3 Source tags and small recipe additions

Use separate `LooseGathering`, `TreeHarvest`, `AnimalHarvest`, `Fishing`, `CraftOnly`, and `Store` sources. The same item ID can have more than one source only under explicit rules; branch + pelt/hide/confirmed feather are baseline overlaps; section 7.1.1 additionally permits explicitly authored tree-food overlaps. A crafted item can be bought without becoming loose gatherable. Pine logs are TreeHarvest and craftable, so conversion is instant; apples are LooseGathering, not a tree-chop reward. Rock salt is the exact Salt ID already used by recipes. Flint stones means the Flint ID, and Rabbit pelt/raw Rabbit pelt refer to the same unprocessed `animal.rabbit.pelt`, not alternative ingredient IDs. Normalize these display descriptions consistently when authoring. Generic Berry becomes the exact Red berries ID; generic Herb becomes Tea herbs where specified. These naming clarifications never permit substitution between different varieties.

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
| Polished lapis | 2 Lapis lazuli -> 1 | Tier-four Material (November 6, 2026). Lapis is opaque, so it is polished rather than cut. Feeds the premium Lapis jewel box; the ordinary Lapis bracelet keeps raw lapis beads |
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

Flax oil and sunflower oil have distinct sinks: seed oil biscuits use 2 Wheat flour + 1 Flax oil, while chili oat cakes use 2 Oats + 1 Sunflower oil + 1 Green chili. Never use an interchangeable Oil input. **Note (November 7, 2026):** neither biscuit nor oat-cake recipe was ever authored, so Flax oil's use is now the house level 6 deposit (section 13.1). Sunflower oil already has recipe uses (Boar leather, Oak panel, Oak boar, Highland shirt). The draft house table uses four Reed braids for level two and six Mortar for level three; their region-six material unlocks are proposed later missions below. Counts remain test data; materials are unlevelled and furniture store-only.

### 6.2.4 Camp spacing and the 26-heart ceiling

Count camps including a region's entry/exit camps; internal walking legs are `campCount - 1`. Ferry transitions do not add walking cost. The proposed counts above are 10-14 in early regions, 18-20 near the end of tier two, and 20-24 in later tiers. Build only short representative test chains initially.

For each leg, author world distance `D` and a fixed `drainHeartsPerMeter`, then compute `requiredHearts = D * drainHeartsPerMeter`. This budget is independent of camera input, time spent stationary, carried quantities, current equipment, or current stamina. Hats raise the meter cap, boots reduce elapsed walking time, and food adds usable stamina during the leg; none silently changes its authored walking cost.

**Confirmed maximum: highest-tier requiredHearts <= 26 per leg.** Keep the cap at twelve hearts. At a full twelve-heart start, a 26-heart leg requires at least fourteen additional hearts from food/drinks. Three ordinary six-heart servings supply eighteen, permitting a four-heart finish if consumed without waste; premium gold food is not required. If starting below full stamina, the food budget must increase accordingly.

Proposed pacing data (ranges are examples, not implemented level measurements):

| Progression | Typical leg distance | Authored walking cost | Intent |
| --- | --- | --- | --- |
| Region 1 opening camps | 100-250 m | 1.2-3 hearts at 12 hearts/km (confirmed starting rate, October 30, 2026; replaces 2 and then 8 hearts/km) | Safe with three starting hearts and no food |
| Rest of region 1 | 150-400 m | At most 1.5 hearts | Obtain apples, finish Gatherer G1/G2, craft juice before region 2 |
| Regions 2-6 | 350-600 m | 3.5-6 hearts | Increasing distance, first mandatory food planning |
| Regions 7-12 | 500-900 m | 6.5-12 hearts | More food and stronger hat access |
| Regions 13-18 | 750-1,200 m | 10-20 hearts | Multiple meals on longer legs |
| Regions 19-24 | 1,000-1,600 m | 14-26 hearts, never above 26 | Food during travel remains necessary even with twelve hearts |

Derive each leg's drain rate from its chosen distance and budget; never combine the maximum distance/rate blindly. Example highest-tier leg: 1,300 m at 20 hearts/km = **26 hearts**. At the 2.0 m/s maximum it is about 10 minutes 50 seconds of walking before harvesting stops. Keeping a universal 2 hearts/km would require 13 km/about 1 hour 48 minutes for the same cost; variable authored regional drain rates are therefore a **recommendation** for mobile pacing, while the 26-heart cap is confirmed.

Provide a departure preview with required hearts, current/max hearts, and **usable** ready food recovery, including pending food hearts and simulating recovery timing against walking drain; delivery beyond the meter cap is lost, so the preview counts only hearts that can actually be absorbed. Warn if under-provisioned; do not invisibly lock departure or auto-buy food. Guarantee first-region apples before the recipe handoff, and G1/G2 camp claims before exit. Camp regeneration must permit a safe next opening leg even if trading/menus are skipped. Simulate starter legs against three hearts and advanced legs against 26, including partial starts and six-heart food scheduling without waste. Maximum hat capacity remains twelve even while cumulative per-leg consumption reaches 26.

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

**Deferred collection design (not an active milestone):** retain `MechanicsTest` at 1,000 units per stackable trail pickup throughout debug/testing. The previously specified `Release` design transfers one item per gathered world object, including quick storage; node drop counts and Elite boosts determine object counts, and stack capacity remains 1,000. That design does not change recipe yields, store/box stacks, mission quantities, or existing save stacks. Preserve it as future reference only. Do not implement/activate the release switch, retune recipe amounts, mastery thresholds, tool costs, mission quotas, food provision, density or pacing, or require a fresh-save release playthrough unless a future user prompt explicitly requests that phase. Current testing records defects and measurements using the unchanged full-stack profile.

### 7.1 Bundle count is separate from unit count

**Confirmed default:** `WorldPickupStackQuantity = 1000` for every stackable trail pickup, including resources dropped by harvesting. Keep this in the main mechanics-test configuration, not behind a debug toggle that is off by default.

| Event | Number of bundles | Units in each bundle | Result |
| --- | --- | --- | --- |
| Loose resource pickup | 1 | 1,000 | One full stack, subject to mission diversion and merge capacity |
| Normal tree branches | Random 2-5 | 1,000 | 2,000-5,000 branch units |
| Normal tree logs | Random 2-5 | 1,000 | 2,000-5,000 log units |
| Tree bonus ingredients | One required type, optional second; 2-4 bundles/type | 1,000 | Species/biome-specific food or sap |
| Elite-tool tree wood | 4-10 bundles for each wood output | 1,000 | Full-stack rewards remain unchanged |
| Elite-tool tree ingredients | 4-6 bundles/type | 1,000 | One required type, maximum two |
| Normal hunt | 2-5 bundles for each selected produce output; fat 1-3 | 1,000 | Use species-specific loot tables, not every possible byproduct |
| Elite-tool hunt | 4-10 produce bundles/output; fat 1-6 | 1,000 | High-load case for spatial inventory and loot queues |
| Fishing completion | 2-5 bundles drawn from local fish table | 1,000 | Each bundle contains one fish species and one quality |
| Elite-tool fishing | Proposed 4-10 bundles from the same local fish table | 1,000 | Boost on all eligible targets; fewer actions at/below tool tier, species restrictions unchanged |
| Massive tree | Same bundle-generation rules, multiplied by 50 | 1,000 | Queue rewards; never render them all at once. **After the post-debug rebalance (user, October 31, 2026): about 450 items per tree**, not 450,000 |
| Camp NPC purchase | 1 unit for a stackable offer | 1 | Not a trail pickup; tools/gear are one instance |
| Settlement stack purchase | 1 full stack | 1,000 | Persist overflow in a delivery tray if appropriate |
| Mystery box | 6 typed reward entries | 1,000 for physical stackables | Gear is individual; recipe unlock is one knowledge grant, not a stack |

Example: a normal tree yielding three branch bundles, four log bundles and two food-ingredient bundles produces 9,000 units. The same massive-tree result gives 150 branch, 200 log and 100 ingredient bundles, totaling 450,000 units. Ingredient rewards are mandatory; this remains intentional stress-test behavior.

Roll rewards once when harvesting completes. Persist the resulting aggregate manifest and a unique claim identifier before exposing drops. Reloading must not reroll quality, multiply rewards, or reset a partially harvested node.

### 7.1.1 Required tree food-ingredient drops

**Recipe coverage review:** all four active-tier ingredients have exact recipe uses below. Pine nuts and Birch sap are consumed directly by Food recipes; Mesquite pods and Acorns first enter their named Material conversions, whose outputs enter Food recipes. No additional recipe ID is needed to cover these ingredients. The four expansion ingredients and their matching recipes are audited in Part 2, section 12.

**Ingredient acceptance:** recipe matching uses the canonical ingredient ID and requested quantity, not whether the item came from a tree, loose gathering or a valid transferred stack. A raw harvested cooking ingredient is ready for recipe input; “raw” does not mean a pending craft, nor does it make the ingredient prepared food for automatic eating. Apply the usual readiness/reservation checks. Distinct quality/profile stacks of the same ingredient ID may contribute their exact required quantities under ordinary crafting rules; do not change crafted-output quality to inherit the ingredient's Q. Source discovery still gates learning/offers independently of consuming owned stock. Never substitute Birch sap for Maple sap, Mesquite pods for Mesquite flour, Acorns for Roasted acorn meal, or raw Maple sap for Maple syrup. Preserve exact wood fuel and scale all conversion/food costs and yields for bulk crafting.

**Confirmed correction:** every completed chopping interaction produces its exact wood's branches, logs **and at least one edible or cooking-ingredient type**. This applies to trail trees and settlement massive trees. A node may author a second food ingredient (two maximum); an empty ingredient list is invalid. Ingredients are part of the completed harvest, not a rare replacement for wood, not one reward per axe swing, and not a consumable reward granted by merely discovering a tree. Persist their exact IDs/quantities with the same harvest manifest as the wood.

**Proposed active-tier assignments:**

| Tier / tree | Required output catalog | Ingredient ID / readiness | Exact useful Food recipe (nonrandom unlock) |
| --- | --- | --- | --- |
| 1 / Pine | Pine branches + Pine logs + Pine nuts | `ingredient.pine_nuts`, raw cooking ingredient | `recipe.food.toasted_pine_nuts`: 2 Pine nuts + 1 Pine branch -> 1 Toasted pine nuts, 2 total hearts |
| 2 / Birch | Birch branches + Birch logs + Birch sap | `ingredient.birch_sap`, raw cooking ingredient | `recipe.food.birch_sap_milk_drink`: 2 Birch sap + 1 Cow milk -> 1 Birch sap milk drink, 3 total hearts |
| 3 / Mesquite | Mesquite branches + Mesquite logs + Mesquite pods | `ingredient.mesquite_pods`, raw cooking ingredient | `recipe.food.mesquite_pod_cakes`: 2 Mesquite flour + 1 Cow butter -> 2 Mesquite pod cakes, 4 hearts per serving |
| 4 / Oak | Oak branches + Oak logs + Acorns | `ingredient.acorns`, raw cooking ingredient | `recipe.food.acorn_biscuits`: 1 Roasted acorn meal + 1 Wheat flour + 1 Cow milk -> 1 Acorn biscuits, 5 total hearts |

**Exact prerequisite conversions:** tier-3 `recipe.material.mesquite_flour`: 3 Mesquite pods -> 2 Mesquite flour (`ingredient.mesquite_flour`); tier-4 `recipe.material.roasted_acorn_meal`: 2 Acorns + 1 Oak branch -> 2 Roasted acorn meal (`ingredient.roasted_acorn_meal`). No interchangeable flour/fuel. Both are unlevelled Material recipes with proposed fixed 10-second batch timers; the two outputs are not directly gatherable. Food output IDs match their recipe suffix under `item.`. Food retains ordinary mastery, crafted quality, no durability and progressive recovery; propose 20-second level-one single-batch base timers, retaining the Food bulk/timer cap. Unlock these recipes/conversions nonrandomly after discovery of the matching tree and all other exact ingredient sources (including known conversions or accessible exact shops); detailed branch task placement remains authored later. They are additional ordinary recipes, excluded from bakery/box collection counts. Ingredients are not ready prepared food and are never auto-eaten at zero stamina.

**Quantity rule:** keep section 7.1's existing Normal/Elite ranges. Normal completion gives 2–5 branch bundles, 2–5 log bundles and 2–4 bundles of each configured ingredient. Elite gives 4–10 bundles of each wood output and 4–6 per configured ingredient. Each bundle remains 1,000 units in MechanicsTest. Apply massive-tree x50 to **all three output categories** once, after the normal/Elite roll; do not omit ingredients or double-apply x50/Elite. Example: 3 branch + 4 log + 2 ingredient bundles = 9,000 units; the corresponding massive harvest yields 150 + 200 + 100 bundles = 450,000 units. Keep pooled views and persistent queued remainders.

**Sources and discovery:** these four ingredients are proposed TreeHarvest-only sources across their respective tier's discovered tree nodes; they do not change the 72 loose IDs or add a fourth map icon. Additional explicit dual-source ingredients are permitted (Part 2's Maple sap and fruits); use the **same item ID** for both sources. For each required ingredient, accept at least one discovered and reachable authored source, not every source in its catalog. Across different required ingredients, all ingredient requirements still apply. Possession, gifts or shop stock never grant discovery. A tree-only ingredient requires tree/source discovery and eligible harvesting access; cooking/refining dependencies remain explicit. Keep locked previews honest about alternative sources.

**Acceptance and milestone work:** Milestone 6 authors these four tree tables plus prerequisite recipe/source definitions and checks mandatory ingredient output. Milestone 7 integrates the ordinary cooking/refinement recipes; Milestone 13 applies the same output tables to all six massive trees per active wood tier. Validate each tree's Normal/Elite completion, eligible higher-tier target, zero-ingredient configuration rejection, second-ingredient cap, exact bulk counts, massive x50, interrupted harvest/claim, capacity overflow and reload without reroll/duplication. Milestone 14's recipe eligibility uses the alternative-source rule. Part 2 carries the same checks into P2.1–P2.4. These are documentation requirements, not evidence of implementation or tests passing.

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

### 8.2.1 Tree chopping interaction

**Confirmed (October 30, 2026):**

- **Interactable look:** a wood interactable is a standing tree cut off partway up: no canopy, but clearly taller than a stump. An axe silhouette moving up and down above it signals that it can be interacted with.
- **Starting:** selecting the tree starts the interaction. The view focuses on the trunk and camera control is disabled for the whole interaction. The inventory button becomes a **Return** button that cancels it. The usual harvest stop applies (section 4.2): walking stops at the start and stays stopped until a fresh upward speed gesture afterwards.
- **Cutting:** the player completes a **horizontal cut line on the lower part of the trunk**. Each swipe across the trunk is one committed action and spends one tool use (section 8.2), adding one segment to the cut line. The tree falls when a line reaches the required number of segments: the action count from section 8.2 for this tool and target (for example, 6 for a matching-tier Normal axe, 3 for Elite).
- **Misaligned swipes:** a swipe noticeably higher or lower than the previous cut starts a **new cut line** instead of extending it. That use is still spent, and segments on separate lines do not add up, so sloppy cuts waste uses.
- **Screen safety:** the cutting area sits low on the trunk but always above the device's bottom system-gesture/navigation area (Android home button or gesture), so a cut never triggers a system action or closes the app. Use the device safe area and gesture insets, not a fixed pixel margin.

**Confirmed details (October 30, 2026):**

- **What counts as a swipe:** it must cross most of the trunk's on-screen width (starting value 70%) in either direction. Shorter swipes or ones off the trunk are ignored and spend no use. One swipe adds at most one segment.
- **Alignment tolerance:** a swipe continues the previous line when its height is within a band around that line. Starting value: 6% of the trunk's on-screen height, never below about 48 dp, so it stays fair on small screens. The first swipe sets the first line.
- **Progress lasts only for the current interaction:** cancelling with Return discards the cut lines; selecting the tree again starts from zero. Uses already spent are never refunded. This supersedes the general partial-harvest-recovery rule (sections 7.3 and 8.2) for trail trees. **Tool breaking (confirmed):** if the tool breaks mid-interaction, the interaction continues with its cut lines intact as long as another valid tool (matching family, eligible tier, uses remaining) is carried in the bag or on a hook, and a tool-broken warning is shown. If no valid tool remains, the interaction ends (cut lines discarded as with Return). During the interaction, the top warning strip (section 12.4) shows a small model of the tool in use with its remaining-uses counter, updating after each use and on replacement.
- **Finishing a trail tree:** completing a line commits the harvest manifest (wood plus the required food ingredient, section 7.1.1) once, closes the interaction, plays the tree-falling animation, and then spawns the rewards on the trail for pickup. Eligibility, Elite drop boosts and the +2 tier ceiling follow section 8.2 unchanged; without an eligible carried axe, selecting the tree shows a hint and starts nothing.
- **Camera:** when the interaction ends, restore the yaw the player had before it started.
- **Massive trees (settlement lumber field):** same cutting mechanic with their larger action counts, but finishing does **not** close the interaction: the rewards spawn in the same focused screen for pickup, and only the Return button closes it. Their pending-reward persistence and expiry remain as in sections 7.2 and 13; other presentation is settled in Milestone 13.
- **Massive trees can be revisited after the cut (user, November 5, 2026):** the interaction is not one-time. While standing, a massive tree works like the other trees (tap, focused screen, chopping). Once it is cut down, tapping the felled tree (its stump and reward piles) **opens the focused pickup screen again**, so a player whose bag filled up can come back and keep picking up the remaining drops. Re-entering never re-rolls or adds rewards: it shows exactly the pending piles that are left, and Return closes it as before. The felled tree stays interactive this way until its piles are empty or they expire with its 72-hour reward cycle. Then it shows only the regrowth stump until it respawns.

### 8.2.2 Hunting interaction

**Confirmed (October 30, 2026):**

- **Interactable look:** animals have simple movement and grazing animations near their fixed spawn points (encounter areas, section 7.3). A bow silhouette being drawn and released above an animal signals that it can be hunted. Animals stay passive: no attacks and no player damage.
  - **Implemented (user, November 7, 2026):** the rabbit is rebuilt in the approved faceted character style (revision 4, same rig pivots, 600 triangles). Animals no longer bob in place: each one cycles between grazing (head down to the grass, nibbling, ears laid back), looking around (sits up, ears upright, head turns, ear flicks) and moving to another spot inside a 1 m encounter area, following the terrain. Rabbits move in real hops: crouch, push off with the hind legs while the forelegs reach, arc nose-up then nose-down, land on the forepaws, gather the hind feet, pause. While hunted the rabbit hops alertly between the hunt's points (about 0.5 m/s, about 1.5 m/s with longer, flatter hops after a miss). A generic four-beat walk gait (`AnimalGait.Walk`) is ready for the later walking species (beaver, boar) once their models exist; the turkey will need its own two-legged gait. Code: `AnimalMotion.cs` (AnimalAnimator, AnimalWanderer); review filmstrips: The Trail Walker > Art > Animal Motion Filmstrips (`Docs/Art/Tier01/Motion/`); Milestone 6 adds five movement checks. **Device check pending:** hop readability at the trail camera distance, wander staying clear of the road, and frame time with six animals per anchor.
- **Starting:** selecting the animal starts the interaction with the same rules as chopping (section 8.2.1). The view focuses on the animal, camera control is off, the inventory button becomes **Return**, and the harvest stop applies. Unlike a tree, the animal **keeps moving within the screen bounds** during the interaction.
- **Shooting:** the hunting tool's model is shown at the bottom of the screen: the slingshot (tiers 1-2) or the bow (tiers 3-4, preferably shown horizontally). The player drags down to draw the string, aims at the animal and releases to shoot. The draw area stays above the device's bottom system-gesture area, as in chopping.
- **Animal heart:** while the interaction lasts, the animal shows a full heart above it. Each hit removes a fraction of the heart equal to one hit of the action count in section 8.2: thirds for a matching-tier Normal tool (3 hits), the whole heart for a matching-tier Elite (1 hit), smaller fractions for higher-tier targets. This heart is a harvest-progress display, not combat health or player stamina.
- **Finishing:** when the heart empties, the harvest manifest is committed once, the interaction closes, the animal falls sideways to the ground, and the rewards then spawn for pickup.
- **Shared rules:** tool breaking (continue with another valid carried tool plus a warning), progress lasting only for the current interaction (Return or reselecting starts over with a full heart; spent uses are never refunded), restoring the camera direction afterwards, and the top-strip tool model with remaining-uses counter all work as in section 8.2.1.

**Confirmed details (approved October 30, 2026):**

- **Aim:** the shot goes opposite to the drag, like a real slingshot or bow: pull down-left to shoot up-right. A short aim guide shows the direction only while drawn, and longer draws travel farther.
- **Committed shot:** a release past a minimum draw (starting value 25% of the maximum) fires and spends one use, hit or miss (section 8.2). Releasing below it cancels the shot without spending a use.
- **Misses:** a miss takes nothing off the heart. The animal briefly moves faster, but stays on screen and keeps its encounter area.

### 8.2.3 Fishing interaction

**Confirmed (October 30, 2026):**

- **Interactable look:** a fishing point is a water source with circular ripples and a fishing-rod silhouette above it signalling the interaction.
- **Starting and casting:** selecting it starts the interaction with the same rules as chopping and hunting (focused view, no camera control, **Return** instead of the inventory button, harvest stop). The rod is drawn the same way as the hunting tool (drag down, then release) and cast into the water.
- **Reeling:** fishing is deliberately simple. Each tap on the rod plays a short line-tensing animation and spends **one use**. The number of taps is the section 8.2 action count for this rod and fishing band (for example, 3 for a matching-tier Normal rod, 1 for a matching-tier Elite rod). The final tap pulls the rod, commits the harvest manifest once, closes the interaction and spawns the rewards. The caught species comes from the region's fish pairing (section 6.2.5).
- **Shared rules:** tool breaking (continue with another valid carried rod plus a warning), progress lasting only for the current interaction (Return or reselecting starts over; spent uses are never refunded), restoring the camera direction afterwards, the top-strip rod model with remaining-uses counter, and keeping all touch areas above the device's bottom system-gesture area all work as in sections 8.2.1 and 8.2.2.

**Confirmed detail (approved October 30, 2026):** the cast itself spends no use and always lands in the water point, with no aiming skill involved; only the taps that follow count as actions. Taps are accepted on the rod model, and very fast repeated taps are debounced to one per line-tensing animation, so a use is never spent by accident.

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

**Barrel-filling permission (revised November 6, 2026):** use one explicit `BarrelFilling` role for both parts, independently of its Misc aging timer. In addition to direct inputs inside {R-1, R}, filling may use (a) the recipe's exact authored aging-produce IDs (discovered fruit, grain, cane or Honey, regardless of tier), and (b) its exact authored barrel-pantry IDs: **Sugar, Honey and Maple syrup**. Food pantry flags alone do not authorize a Misc/BarrelFilling input. Define per-recipe allowlists, never runtime choices of any tagged ingredient. Honey may have both permissions but is consumed once. Wood, fittings, charcoal/fuel and all nonlisted inputs keep the normal tier window. Upstream conversions are checked under their own recipe role/tier; this exception never spreads recursively into smelting, syrup production, tools, garments or ordinary Misc.

Validate each exact input with `inTierWindow OR (role == Food AND approvedFoodPantryOrContainer AND materialTier <= RecipeUnlockTier) OR (role == BarrelFilling AND exactInputInAuthoredAgingOrBarrelPantryList)`, as well as the existing exact-ID/quantity/quality/source checks. Filling and bottling remain separate unlocks; bottling remains Food with its exact filled barrel plus Glass bottles. Check source access and conversion knowledge recursively at offer generation and commit. **Pending checks:** accept tier-six Apricot brandy with known Sugar conversion/source and tier-five Plum wine with known Maple syrup conversion/source; reject missing sources/conversions, unlisted fruit substitutions and out-of-window wood/fittings/fuel. This is a plan correction, not an implementation/test result.

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
| Cured rabbit pelt | 2 raw rabbit pelts + 1 animal fat | 1 | Camp/home, 15 s. **Revised November 7, 2026:** animal fat replaces salt (fat-softened fur), so it no longer duplicates Rabbit leather's inputs and the rabbit's fat drop has a use |
| Cured beaver pelt | 2 raw beaver pelts + 1 salt | 1 | Camp/home, 15 s |
| Rabbit leather | 2 raw rabbit pelts + 1 salt | 1 | Camp/home, 15 s |
| Beaver leather | 2 raw beaver pelts + 1 salt | 1 | Camp/home, 15 s |
| Boar leather | 2 boar hides + 1 sunflower oil | 1 | Tier-four Material with tier-three oil |
| Fine rabbit leather | 2 rabbit leather + 1 nettle thread | 1 | Tier-one Material; no future beeswax. **Revised November 7, 2026:** Nettle thread replaces Hemp thread so Nettle thread has a use (section 11.6.1.1) |
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

**Dairy unlock gate (November 7, 2026):** milk is a ranch purchase, so Cow cream, Cow butter, the cheeses, Cow yogurt and every recipe that uses them unlock no earlier than mission region 5 (section 11.6.1.1). No starter culture, yeast, enzyme, or tannin is required. Each cheese recipe uses its exact named milk(s) and salt; waiting abstracts the process and follows the Food mastery/time limit. Nettle cloth and hemp cloth are separate tier-one recipes, not interchangeable inputs. Cotton belongs to tier three. Extra cut-gem/wood/metal variants require their own explicit recipe/output definitions before use. Provide guaranteed early fiber/salt sources so material-first missions are achievable.

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

**Superseded for garments (November 5, 2026):** the tier-two hat and backpack, the tier-three feather garment and backpack, and the tier-four shirt, pants and backpack above are now the **Set A** garments of section 9.6.1, with Set A capabilities instead of the tier limit. Section 9.6.1 lists every garment for both sets of tiers two to four. These are individual fixed-ingredient recipes. An authoring template may help create additional definitions, but never exposes a runtime material choice. Remaining hats/boots/shirts/pants need their own exact recipes before activation; no generic higher-tier garment recipe fills those gaps automatically. Indestructible merchant gear remains noncraftable with the same global caps.

Every tool is Normal unless marked Elite. The table now covers all 21 active tool definitions: three tier-one Normals plus Normal/Elite pairs at tiers two-four. Proposed Normal recipes use current-tier branches/core metal or animal inputs rather than a second large current-tier plank bundle. Elite then consumes that Normal plus exactly two previous-tier advanced units as section 8.4 specifies. The Elite keeps family/tier but has fewer actions up to that tier, greater base durability, and `HasDropBoost = true` for every eligible target. Normals never gain a drop buff from over-tier advantage. All Tool/Wearable recipes stay x1 only.

### 9.4 Food and beverages

| Recipe | Inputs | Yield | Hearts per serving |
| --- | --- | --- | --- |
| Apple juice | 3 apples | 1 | 1; region-one unlock, proposed level-one 3-second timer |
| Roasted mushrooms | 2 mushrooms + 1 pine branch | 1 | 1; tier-one wood is consumed |
| Berry oat cakes | 2 oats + 2 red berries | 2 | 1 |
| Roasted rabbit | 1 raw rabbit meat + 1 pine branch | 1 | 2; Explorer reward, no salt prerequisite |
| ~~Roasted beaver~~ -> **Blackberry-glazed beaver** (`recipe.food.blackberry_glazed_beaver`) | 1 raw beaver meat + 1 blackberries + 2 birch branches | 1 | 3; tier-two glaze, region 7 (revised November 7, 2026) |
| ~~Roasted turkey~~ -> **Turkey carrot skewers** (`recipe.food.turkey_carrot_skewers`) | 1 raw turkey meat + 1 carrots + 2 mesquite branches | 1 | 4; tier-three skewers, tier-two Carrots (revised November 7, 2026) |
| ~~Roasted boar~~ -> **Acorn-crusted boar** (`recipe.food.acorn_crusted_boar`) | 1 raw boar meat + 1 roasted acorn meal + 2 oak branches | 1 | 6; tier-four crust, region 19 (revised November 7, 2026) |
| Herb trout | 1 rainbow trout + 1 tea herb + 1 pine branch | 1 | 2; separate tier-one recipe, no salt needed |
| Corn flatbread | 2 corn flour + 1 water | 2 | 2 |
| Turkey bean stew | 2 turkey meat + 2 beans + 1 tomato | 2 | 3 |
| Mesquite nut cakes | 2 mesquite pods + 1 wheat flour + 1 honey | 2 | 3 |
| ~~Boar potato stew~~ -> **Boar potato hash** (`recipe.food.boar_potato_hash`) | 1 boar meat + 2 potatoes + 1 cow butter | 2 | 5; revised November 7, 2026 (was a copy of the Beaver-carrot stew pattern) |
| Carp rice bowl | 2 rice + 2 common carp + 1 salt | 2 | 4 |
| Pike rice bowl | 2 rice + 2 northern pike + 1 salt | 2 | 4; independent recipe/output |
| Rabbit barley soup | 2 rabbit meat + 2 barley + 1 tea herb | 2 | 5 |
| Cow-cheese tart | 2 wheat flour + 1 cow cheese + 1 egg | 2 | 5; goat/mixed cheese not accepted |
| ~~Boar bean chili~~ -> **Boar cacao mole** (`recipe.food.boar_cacao_mole`) | 1 boar meat + 1 cacao powder + 1 green chili | 1 | 6; revised November 7, 2026: the old recipe copied Turkey chili (2 meat + 2 Beans + 1 Green chili) with more hearts |
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

Bakery assortment: one cash stack of bread/oat biscuits/pumpkin rolls recovering 1-6 hearts, and two gold stacks selected from honey brioche (8), berry cream pastries (10), premium apple pie (12), vanilla cacao celebration cake (12), pumpkin cow-cheese galette (10), mint sugar rolls (11), and coconut cream tart (12). Premium apple pie is a proposed Q5 bakery gold good with its own rare recipe unlock (section 9.4.4), separate from apple juice and the box compote recipe; buying it grants neither recipe. All these values are total progressive recovery; a twelve-heart pie can be used with a three-heart hatless meter because excess remains pending (section 4.4.3). Shop-exclusive goods can be authored directly without a dependency graph for every real-life ingredient. They are optional; ordinary crafted meals support all trail progression without gold food.

**Proposed premium recipe offers:** Alongside each eligible crafted premium food offer, the bakery may rarely offer that food's single recipe as an alternative. A recipe offer is exactly one recipe item, never a stack; it replaces the crafted premium food offer for that slot and is purchased for gold at **10x the authored gold price of one serving** of that food. Premium food itself remains purchasable in full stacks; price a 1,000-serving stack at a **10% discount** from its per-serving price (for example, 5 gold/serving -> 5,000 gold list total -> **4,500 gold** stack price). Roll the recipe substitution at stock generation and persist the result with the visit stock so reopening or reloading does not reroll it. Buying a recipe teaches/unlocks it; buying prepared food does not.

### 9.4.1 Wood-fueled roasting and exact fish recipes

**Confirmed:** wood in roasted recipes represents fuel/skewer material and is consumed as an ingredient. It is not tool durability, a reusable equipped utensil, a timer-only decoration, or a generic substitute fuel. Keep the ingredient window: tier-one Pine; tier-two Birch; tier-three Mesquite; tier-four Oak in the examples. Larger targets can require two/three branches or a log plus branches. No new stove prerequisite; unlocked recipes remain camp/home crafts.

Each row below is a separate named Food recipe/output with its own mastery and exact fish ID. Proposed fuel quantities use the animal-roast progression without expanding to realistic seasoning/processing chains:

| Unlock tier | Exact roasted-fish output | Fixed inputs -> one serving |
| --- | --- | --- |
| 1 | ~~Roasted rainbow trout~~ -> **Trout mushroom skillet** (`recipe.food.trout_mushroom_skillet`) | 1 Rainbow trout + 1 Mushrooms + 1 Pine branch -> 1; 2 hearts |
| 1 | ~~Roasted largemouth bass~~ -> **Salt-baked bass** (`recipe.food.salt_baked_bass`) | 1 Largemouth bass + 2 Rock salt + 1 Pine branch -> 1; 2 hearts |
| 1 | ~~Roasted bluegill~~ -> **Pine-nut bluegill** (`recipe.food.pine_nut_bluegill`) | 1 Bluegill + 1 Pine nuts + 1 Pine branch -> 1; 2 hearts |
| 1 | ~~Roasted yellow perch~~ -> **Apple-stuffed perch** (`recipe.food.apple_stuffed_perch`) | 1 Yellow perch + 1 Apples + 1 Pine branch -> 1; 2 hearts |
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

**Tier one revised (user, November 7, 2026):** the four plain tier-one roasts are replaced by four different dishes (rows above), each pairing its fish with a tier-one ingredient discovered by region 4: Mushrooms (region 4), Rock salt (region 3), Pine nuts (pine chopping) and Apples (region 1). None uses ranch dairy, so the dishes need no settlement access. Each keeps the Pine branch as listed fire fuel, the 2-heart tier-one value of Herb trout, and its own ID and mastery. With Herb trout (E8), tier one has five fish dishes and no plain roast. The four roast recipes already exist in code (`Tier1Recipes.cs`, `Tier1CraftedItems.cs`, `Tier01Recipes.json`) with art. On implementation, remove them from the recipe catalog, but keep their item definitions as legacy food so roasts already in test saves stay valid and edible: never delete them silently or turn them into another item. The four new dishes need their own models. **Tiers two to four (user, November 5, 2026):** from tier two each fish species is a collection mission that teaches a varied dish instead (section 9.6.5), so the tier 2-4 rows are not authored as recipes. Starting stamina values can be two/three/four/six hearts per tier-one/two/three/four serving, matching ordinary food planning for the 26-heart leg; species-specific tuning can follow without substituting ingredients. All are stackable, support existing bulk quantities, level 1-10, and stay under the one-hour Food timer limit. Bulk multiplies wood and fish costs exactly: x100 Acorn-crusted boar needs 100 Raw boar meat + 100 Roasted acorn meal + 200 Oak branches; an x100 request never treats one wood piece as fuel for the whole batch.

Existing stews, drinks, and non-roasted dishes do not automatically receive another hidden fuel charge. The recipe itself must list each cost. Roasted coffee's material recipe also lists its Oak branch. Apple juice remains the fuel-free early survival recipe, and the revised early roasted-rabbit mission can use leftover Pine branches without salt or an advanced processing unlock.

### 9.4.2 Optional balloon-box recipe collection

**Premium goods (confirmed October 31, 2026):** every Food recipe obtained from a mystery box or bought in a store (balloon box, bakery) makes a **premium good**, like the Premium apple pie: proposed **8-12 total hearts per serving**, rising with tier for box recipes (tier one 8-10, tier two 9-10, tier three 10-11, tier four 11-12); bakery goods keep their section 9.4.4 values (8-12). Mission recipes keep the ordinary 1-6 hearts. The tables in sections 9.4.2-9.4.3 use these values; tier one is implemented.

**Confirmed direction:** add recipe unlocks to mystery-box rewards, mainly Food and Misc. The following **26 recipe seeds** (21 Food, 5 Misc, including eight additional high-recovery regional meals and two truffle dishes in section 9.4.3) are a proposal, separate from normal mission recipes. They never replace the first apple juice, roasted rabbit, Pine bird, Rabbit toy, or clothing unlocks. Make them optional discoveries, not random prerequisites for mandatory progression. Each ID is unique; names do not create interchangeable ingredient slots.

| Recipe ID / output | Unlock tier | Exact ingredients -> yield | Starting hearts per serving |
| --- | --- | --- | --- |
| `recipe.food.roasted_shrimp` — Roasted shrimp | 1 | 1 Shrimp + 1 Pine branch -> 1 | 8 |
| `recipe.food.shrimp_pie` — Shrimp pie | 1 | 1 Shrimp + 1 Wheat flour + 1 Cow butter -> 1 | 10 |
| `recipe.food.roasted_crab` — Roasted crab | 2 | 1 Crab + 2 Birch branches -> 1 | 9 |
| `recipe.food.creamy_crab_soup` — Creamy crab soup | 2 | 1 Crab + 1 Cow cream + 1 Carrot -> 1 | 10 |
| `recipe.food.honey_blackberry_biscuits` — Honey-blackberry biscuits | 2 | 2 Wheat flour + 1 Honey + 1 Blackberry -> 2 | 9 |
| `recipe.food.roasted_squid` — Roasted squid | 3 | 1 Squid + 3 Mesquite branches -> 1 | 10 |
| `recipe.food.squid_rice_bowl` — Squid rice bowl | 3 | 1 Squid + 1 Rice + 1 Green chili -> 1 | 11 |
| `recipe.food.mint_cow_cheese_flatbread` — Mint cow-cheese flatbread | 3 | 1 Wheat flour + 1 Cow cheese + 1 Wild mint -> 1 | 10 |
| `recipe.food.roasted_lobster` — Roasted lobster | 4 | 1 Lobster + 1 Oak log + 2 Oak branches -> 1 | 11 |
| `recipe.food.lobster_rice_bowl` — Lobster rice bowl | 4 | 1 Lobster + 1 Rice + 1 Seaweed -> 1 | 12 |
| `recipe.food.cacao_coconut_tart` — Cacao-coconut tart | 4 | 1 Cacao powder + 1 Coconut + 1 Wheat flour -> 1 | 12 |

**Additional regional meals — proposed values, confirmed stronger-food direction:** these eight non-pie dishes extend the same optional box pool. Apple juice stays the guaranteed one-heart starter; a richer apple dish becomes an optional ten-heart discovery. Every serving below has ten or more total progressive hearts, no durability, and the usual Food mastery/timer rules.

| Recipe ID / output | Unlock tier | Exact ingredients -> yield | Total hearts per serving | Required discovered ingredient regions |
| --- | --- | --- | --- | --- |
| `recipe.food.apple_oat_compote` — Apple-oat compote | 1 | 3 Apples + 2 Oats + 1 Cow milk -> 1 | 10 | 1 and 5; reachable cow-milk shop |
| `recipe.food.red_berry_oat_porridge` — Red-berry oat porridge | 1 | 3 Red berries + 2 Oats + 1 Cow milk -> 1 | 10 | 5; reachable cow-milk shop |
| `recipe.food.beaver_barley_hotpot` — Beaver-barley hotpot | 2 | 2 Raw beaver meat + 2 Barley + 1 Carrot -> 1 | 10 | Beaver hunting in 7-12, Barley in 9, Carrot in 8 |
| `recipe.food.blackberry_honey_oats` — Blackberry-honey oats | 2 | 3 Blackberries + 2 Oats + 1 Honey -> 1 | 10 | 7, 5, and 11 |
| `recipe.food.turkey_bean_bowl` — Turkey-bean bowl | 3 | 2 Raw turkey meat + 2 Beans + 1 Corn flour -> 1 | 11 | Turkey hunting in 13-18, Beans in 14, Corn in 13 plus known flour conversion |
| `recipe.food.pumpkin_chickpea_pot` — Pumpkin-chickpea pot | 3 | 2 Pumpkins + 2 Chickpeas + 1 Cow cheese -> 1 | 11 | 15; reachable exact cow-cheese source |
| `recipe.food.boar_potato_chili` — Boar-potato chili | 4 | 2 Raw boar meat + 2 Potatoes + 1 Green chili -> 1 | 12 | Boar hunting in 19-24, Potatoes in 19, Green chili in 17 |
| `recipe.food.coconut_cacao_pudding` — Coconut-cacao pudding | 4 | 2 Coconuts + 1 Cacao powder + 1 Cow milk -> 1 | 12 | 24 and 20 plus known cacao conversion; reachable cow-milk shop |

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
| `recipe.food.truffle_cow_cheese_flatbread` — Truffle cow-cheese flatbread | 2 | 2 Truffle + 1 Wheat flour + 1 Cow cheese -> 1 | 10 | Balloon box; region 10 and region 6 Wheat/flour conversion or reachable exact flour shop; known/reachable exact Cow cheese source |
| `recipe.food.turkey_truffle_bean_pot` — Turkey-truffle bean pot | 3 | 1 Truffle + 2 Raw turkey meat + 2 Beans -> 1 | 11 | Balloon box; discovered truffle region 10, turkey hunting in 13-18 and Beans region 14 |

The ten-Truffle request is an explicit lower-cost optional tier-two mission override, not a new mandatory branch gate. It shares the existing one-active-mission limit, exact-ID consumption and camp hand-in; offer only after every ingredient source is accessible. It grants only the named recipe once at mastery one. Finish or defer it without blocking main missions/house/region progression. Guarantee sufficient loose truffle opportunities without any pet. The first recipe is mission-only and excluded from box rolls; the other two extend section 9.4.2's pool to **26 IDs = 21 Food + 5 Misc**. No free finished output is granted. Duplicate protection, six typed box slots, proposed roll weights, exact source discovery and mastery-one grants remain unchanged. Mark Truffle in the food-ingredient catalog and show its region-10 source in recipe previews.

### 9.4.4 Premium bakery recipes and gold offers

**Expanded scope:** add seven optional bakery Food unlocks to the 26 balloon-box recipes: **33 optional collectible recipes = 28 Food + 5 Misc** across these two sources. The separate truffle side-mission recipe is additional (34 across these three catalogs). The bakery's seven recipes are acquired only through rare bakery offers; the balloon pool stays at 26. All values below are proposed starting data for later review.

| Recipe ID / premium food | Unlock tier | Exact ingredients -> yield | Total hearts per serving | Gold per serving | Gold per 1,000-serving stack (-10%) | Gold per single recipe |
| --- | --- | --- | --- | --- | --- | --- |
| `recipe.food.honey_brioche` - Honey brioche | 2 | 2 Wheat flour + 1 Cow butter + 1 Honey -> 1 | 8 | 5 | 4,500 | 50 |
| `recipe.food.blackberry_cream_pastry` - Blackberry cream pastry (the berry cream bakery good) | 2 | 2 Wheat flour + 2 Blackberries + 1 Cow cream -> 1 | 10 | 7 | 6,300 | 70 |
| `recipe.food.premium_apple_pie` — Premium apple pie | 1 | 2 Wheat flour + 3 Apples + 1 Cow butter -> 1 | 12 | 8 | 7,200 | 80 |
| `recipe.food.pumpkin_cow_cheese_galette` — Pumpkin cow-cheese galette | 3 | 2 Wheat flour + 2 Pumpkins + 1 Cow cheese + 1 Cow butter -> 1 | 10 | 8 | 7,200 | 80 |
| `recipe.food.mint_sugar_rolls` — Mint sugar rolls | 3 | 2 Wheat flour + 1 Wild mint + 1 Sugar + 1 Cow butter -> 1 | 11 | 9 | 8,100 | 90 |
| `recipe.food.vanilla_cacao_celebration_cake` - Vanilla cacao celebration cake | 4 | 2 Wheat flour + 1 Cow cream + 1 Cacao powder + 1 Vanilla pod + 1 Sugar -> 1 | 12 | 10 | 9,000 | 100 |
| `recipe.food.coconut_cream_tart` - Coconut cream tart | 4 | 2 Wheat flour + 2 Coconuts + 1 Cow cream + 1 Sugar -> 1 | 12 | 10 | 9,000 | 100 |

Use the existing exact ingredient definitions: Vanilla pod is the region-22 Vanilla pods resource, not a new refined vanilla product. Offer a recipe only after discovery of all its regional sources: brioche requires Wheat region 6 and Honey region 11; pastry requires Wheat 6 and Blackberries 7; pie requires Apples 1 and Wheat 6; cake requires Wheat 6, Sugar cane 17, Cacao beans 20 and Vanilla pods 22. Also require accessible exact dairy sources and known conversions or accessible exact shops for flour, butter, cream, cacao powder and sugar. Pantry exceptions allow the named older Wheat flour/dairy/Sugar; they never waive discovery or permit substitutes. Recipe unlock tier is authored, not the player's current tier. Crafted output uses ordinary Food mastery/quality, timers below one hour, x1/x100/x1000 quantities, no durability and progressive recovery; ingredients and yield scale exactly. Shop food remains Q5. These simplified bakes require no new ingredient, station, hidden fuel or gold crafting cost. Learning starts at mastery one/zero crafts and grants no food. They remain optional for progression.

**Rare substitution proposal:** generate one common cash-food slot and two premium gold slots per settlement visit. Select each premium food normally, then roll **1% per eligible premium slot** to replace its entire food-stack offer with **one unlock of that same food's recipe**. A slot holds either food or its recipe. Skip learned recipes and duplicate recipe IDs already offered/reserved in that stock; keep the original food if ineligible, without another roll. Persist the selected food ID, offer kind, recipe ID, quantity (1 recipe or 1,000 servings), quoted price and purchase state. Each recipe slot can be purchased once and grants knowledge atomically with payment, even with a full bag; it is never a stack of 1,000 scrolls. Recheck learned status before payment to prevent charging for a duplicate. Reopen/reload and internal settlement navigation preserve the roll; actual departure/return generates fresh stock. Ordinary food purchases never teach recipes.

**Pricing:** `recipeGold = servingGold * 10`; `fullStackGold = servingGold * 1000 * 90 / 100`. The recipe uses the undiscounted single-serving price. At 5 gold/serving, the recipe costs 50 gold and the full stack costs **4,500 gold**. Apply the discount once to a full premium food stack, never to recipes or common cash food. **Partial purchases (confirmed October 31, 2026):** shop stacks are not all-or-nothing. The player chooses any quantity from 1 up to the offered stock and pays `servingGold * quantity` (whole gold); the 10% discount applies **only** when buying the complete 1,000-serving stack. Full stacks are a late-game convenience for players with better loot and crafted-item income; newer players buy what they need and can afford. Store quotes with integer/fixed-point 64-bit totals and persist them through purchase/reload. Player-crafted resale follows the ordinary cash valuation rules; it does not mint the bakery's gold retail price.

**Additional tier-four recipe:** `recipe.food.coconut_cream_tart` — Coconut cream tart: **2 Wheat flour + 2 Coconuts + 1 Cow cream + 1 Sugar -> 1 serving**, 12 total hearts; tier 4; 10 gold/serving, 9,000 gold/full stack, 100 gold/single recipe. This is distinct from the balloon's cacao-coconut tart and requires no cacao.

**Tier distribution and discovery checks:** the seven bakery recipes comprise **one tier-1** (apple pie), **two tier-2** (honey brioche, blackberry cream pastry), **two tier-3** (pumpkin galette, mint sugar rolls) and **two tier-4** (vanilla cacao cake, coconut cream tart). Add all seven corresponding foods to the bakery's premium selection pool. Ingredient access gates apply to recipe substitution independently of whether prepared food can be sold. Tier, mastery, gold balance, prepared-food purchases, debug inventory and possession of gifted ingredients never substitute for source discovery.

At offer generation **and again before payment/unlock**, require the authored recipe tier to be reached through normal regional discovery and every exact ingredient's source to be discovered/accessible. For refined ingredients, follow their fixed dependency chain to the raw source and require a known conversion or an accessible exact shop; finding refined stock alone never bypasses an undiscovered regional raw source. Check the existing source list above plus: galette requires Wheat 6, Pumpkins 15 and reachable exact Cow cheese/Cow butter sources; mint rolls require Wheat 6, Sugar cane 17, Wild mint 18 and exact Cow butter; coconut tart requires Wheat 6, Sugar cane 17, Coconuts 24 and exact Cow cream. Persist required source IDs and validate against discovery state after reload/migration, without rerolling offers. Reject invalid/stale recipe purchases without charging; retain the stored offer until a legitimate stock refresh. Recipe-book previews show unmet sources and do not grant knowledge.

**Acceptance cases:** assert the 1/2/2/2 tier distribution and unique IDs. Test each recipe with all prerequisites met, then with each required source missing individually. Specifically reject galette before region 15, mint rolls before either 17 or 18, cake before either 20 or 22, and coconut tart before 24; also reject each when Wheat 6 or a required exact dairy/conversion source is unavailable. Test high mastery/gold, gifted inputs and purchased food with undiscovered sources, save/reload and stale purchase rejection. Discovering only a tier's first region must not enable its later-region recipes. With all prerequisites met, an unlearned recipe is eligible for the existing 1% roll; discovery never guarantees an offer.

### 9.4.5 Oak-barrel drinks (mystery-box exclusive, proposal October 31, 2026; revised the same day)

Two-step drinks: a **filling recipe** puts fruit, grain, cane or honey into an oak barrel to age, and a separate **bottling recipe** turns that filled barrel plus glass bottles into the finished drink. Both kinds are **mystery-box exclusive** recipe unlocks (added to the box pool in section 9.4.2), and they are learned **separately**: unlocking a barrel recipe never unlocks its bottling recipe.

**Filled barrels are not consumables.** A filled barrel cannot be eaten, is never auto-eaten at zero stamina and gives no hearts. It is the bottling input, or a **valuable trade good**: it sells for good cash that reflects the materials and the aging (see pricing). The bottling recipe consumes the filled barrel; no empty barrel is returned (avoids an extra item and a reuse loop; a returnable barrel can be added later if wanted).

**Wood (recommendation):** oak only. Oak is the real-world standard for barrels; pine is unsuitable (its resin taints drinks), birch is rarely used, and mesquite is mainly a smoking wood (here it chars the whiskey barrel). A mesquite "smoked" variant could be a later addition.

**Appearance condition:** barrel and bottling recipes can enter a mystery box only after the player knows the **Oak plank** recipe (tier four), and after discovery of every named ingredient source (and any needed conversion such as Sugar or Mesquite charcoal). A bottling recipe additionally requires its own filling recipe to be known, so a box never hands out a bottling recipe with no way to make its barrel. Recipe unlock tier: 4.

**Recipe levels (confirmed):** filling recipes are **levelled 1-10 like other nonmaterial crafts**: mastery and craft-quality furniture raise the barrel's finished quality and its value (section 6.1 / 12.2), and a new recipe starts at mastery one. Aging uses the **Misc timer direction**: higher mastery ages longer (better barrels take more time), reaching hours at high levels; buffs never change time. Bottling recipes are ordinary levelled Food recipes (Food timer curve, crafted quality, progressive recovery; each bottle is one serving).

**Shared shape:** filling = **4 Oak planks + 2 Iron fittings + the drink's ingredients -> 1 filled barrel**. Bottling = **1 filled barrel + 8 Glass bottles -> 8 bottles of the drink**. Exact-ID rules apply: no substituting fruits, grains, cane, honey, sugar, charcoal, fittings or bottles.

| Drink | Filling recipe (ID: inputs -> 1 filled barrel) | Bottling recipe (ID) | Hearts per bottle | Ingredient sources |
| --- | --- | --- | --- | --- |
| Apple cider | `recipe.barrel.apple_cider`: 4 Oak planks + 2 Iron fittings + 12 Apples | `recipe.food.apple_cider` | 11 | Apples region 1 |
| Strawberry wine | `recipe.barrel.strawberry_wine`: 4 Oak planks + 2 Iron fittings + 12 Strawberries + 2 Sugar | `recipe.food.strawberry_wine` | 12 | Strawberries 4, Sugar (Sugar cane 17) |
| Blackberry wine | `recipe.barrel.blackberry_wine`: 4 Oak planks + 2 Iron fittings + 12 Blackberries + 2 Sugar | `recipe.food.blackberry_wine` | 12 | Blackberries 7, Sugar (17) |
| Blueberry wine | `recipe.barrel.blueberry_wine`: 4 Oak planks + 2 Iron fittings + 12 Blueberries + 2 Honey | `recipe.food.blueberry_wine` | 12 | Blueberries 9, Honey 11 |
| Spiced honey mead | `recipe.barrel.honey_mead`: 4 Oak planks + 2 Iron fittings + 10 Honey + 1 Wild mint | `recipe.food.honey_mead` | 12 | Honey 11, Wild mint 18 |
| Barley ale | `recipe.barrel.barley_ale`: 4 Oak planks + 2 Iron fittings + 12 Barley + 2 Honey | `recipe.food.barley_ale` | 11 | Barley 9, Honey 11 |
| Wheat beer | `recipe.barrel.wheat_beer`: 4 Oak planks + 2 Iron fittings + 8 Wheat + 4 Barley | `recipe.food.wheat_beer` | 11 | Wheat 6, Barley 9 |
| Cane rum | `recipe.barrel.cane_rum`: 4 Oak planks + 2 Iron fittings + 12 Sugar cane + 2 Sugar | `recipe.food.cane_rum` | 12 | Sugar cane 17 (Sugar from it) |
| Charred-oak whiskey | `recipe.barrel.corn_whiskey`: 4 Oak planks + 2 Iron fittings + 8 Corn + 4 Barley + 1 Mesquite charcoal | `recipe.food.corn_whiskey` | 12 | Corn 13, Barley 9, Mesquite charcoal (tier 3) |

Every bottling recipe is `1 [drink] barrel + 8 Glass bottles -> 8 [drink]` (e.g. `recipe.food.apple_cider`: 1 Apple cider barrel + 8 Glass bottles -> 8 Apple cider).

**Pricing (proposal; barrels use the Misc price range, user decision):** filled barrels are priced as **Misc** (the top base-price category, like sculptures, toys and jewelry), with per-drink level-10 Q1-equivalent references around the tier-four Misc reference of 6,000: ciders and beers lowest, wines and mead in the middle, aged spirits highest. Values then follow the ordinary rules: `Pbase(L) = reference * (L + 35) / 45`, times the finished-quality multiplier (Q5 = 2.00); no durability premium. Bottled drinks are Food with their own premium references, set so that bottling adds value over selling the barrel (eight bottles are worth more than their barrel plus the bottles).

| Drink | Barrel reference (L10, Q1-equivalent) | L1 Q1 barrel | L10 Q5 barrel | Bottle reference (L10) | L10 Q5 bottles x 8 |
| --- | --- | --- | --- | --- | --- |
| Apple cider | 5,000 | 4,000 | 10,000 | 700 | 11,200 |
| Barley ale | 5,200 | 4,160 | 10,400 | 720 | 11,520 |
| Wheat beer | 5,200 | 4,160 | 10,400 | 720 | 11,520 |
| Strawberry wine | 5,600 | 4,480 | 11,200 | 780 | 12,480 |
| Blackberry wine | 5,800 | 4,640 | 11,600 | 800 | 12,800 |
| Blueberry wine | 5,800 | 4,640 | 11,600 | 800 | 12,800 |
| Spiced honey mead | 6,000 | 4,800 | 12,000 | 820 | 13,120 |
| Cane rum | 6,600 | 5,280 | 13,200 | 900 | 14,400 |
| Charred-oak whiskey | 7,200 | 5,760 | 14,400 | 980 | 15,680 |

The inputs are cheap tier-one to tier-four materials (planks, fittings and produce worth a few hundred cash in total), so a barrel is a strong late-game trade good, as intended for an aged product. All values are proposals for the post-debug rebalance.

**Further candidates (not in the pool yet):** rice wine (Rice 11), red-berry wine (Red berries 5 + Sugar 17).

**Resolved (November 6, 2026; the user left both decisions to Claude):**
1. **Hearts:** the bottles now give 11-12 hearts like the other tier-four premium box Food (section 9.4.2): ciders and beers 11; wines, mead, rum and whiskey 12. Prices are unchanged. Part 2's barrel drinks already use 11-12.
2. **Ingredient window:** an explicit **barrel exception** (section 9.0.1). Filling recipes may use any discovered fruit, grain, cane or honey from any tier, because aging is how old produce keeps its value late in the game. The barrel itself (Oak planks, Iron fittings) and any conversion input (Mesquite charcoal) stay inside the tier window. Bottling recipes need no exception: their inputs are the barrel and Glass bottles.

**Box pool effect:** the collectible box pool grows from 26 to **44 recipes** (26 + 9 filling + 9 bottling), the eight premium ranch recipes (section 13.4.2) bring it to 52, and the seven precious-metal and gem keepsakes (section 9.5.1) to 59; the per-slot recipe chance (5%) and the six-entry box structure are unchanged, so each individual recipe becomes a little rarer.

### 9.5 Sculptures, jewelry, and stuffed toys

| Rank | Sculpture recipe | Jewelry recipe | Stuffed-toy recipe |
| --- | --- | --- | --- |
| 1 | Pine bird: 2 pine logs | Cord charm: 1 polished pebble + 1 hemp cord | Rabbit toy: 2 raw rabbit pelts + 1 hemp fiber |
| 2 | Birch beaver: 2 birch planks + 1 beeswax | Copper pendant: 1 copper ingot + 1 quartz + 1 hemp cord | Beaver toy: 2 fine beaver pelts + 1 plant stuffing + 1 hemp thread |
| 3 | Mesquite sun: 2 mesquite planks + 1 turquoise | Turquoise bracelet: 1 cut turquoise + 1 bronze ingot | Turkey toy: 2 cotton cloth + 2 turkey feathers + 1 flax stuffing |
| 4 | Oak boar: 2 oak planks + 1 sunflower oil | Ruby brooch: 1 iron fitting + 1 cut ruby | Boar toy: 2 fine boar leather + 1 cotton cloth + 1 cotton stuffing |

All products stack to 1,000 by compatible item ID/quality/stat profile. Plant stuffing has one fixed material recipe: 3 nettle fiber -> 2 plant stuffing. Reeds cannot replace nettle in that recipe. Extra silver/gold/gem jewelry variants need separate fixed definitions if introduced within tier four. Deferred species remain backlog only. Sale Misc is never functional furniture. Preserve full-stack testing while deferring price balance.

### 9.5.1 Precious-metal and gem keepsakes (mystery-box exclusive, proposal, user request November 5, 2026)

Seven premium **Misc** recipes tied to each tier's precious metals and gems, unlocked only from the mystery box. **Tier one has none** (it has no precious metal or gem; user decision). Each follows the premium rule of section 9.6.6: at least three distinct ingredients, including a refined material (ingot, cut gem, fittings or fine leather), priced at the **top of its tier's Misc range** (section 12.2). They stay within the nonfood tier window {R-1, R}. Misc rules apply: levelled 1-10, longer timers with mastery, stack to 1,000, no durability, never furniture.

| Recipe ID / output | Tier | Exact ingredients -> 1 | Sources needed |
| --- | --- | --- | --- |
| `recipe.misc.quartz_prism_lantern` — Quartz prism lantern | 2 | 1 Cut quartz + 2 Copper fittings + 1 Glass | Quartz 9, Copper ore 8 (Cut quartz, Copper fittings and Glass recipes known) |
| `recipe.misc.tin_quartz_music_box` — Tin-and-quartz music box | 2 | 1 Tin ingot + 1 Cut quartz + 1 Birch plank | Tin ore 12, Quartz 9 |
| `recipe.misc.silver_turquoise_buckle` — Silver turquoise belt buckle | 3 | 1 Silver ingot + 1 Cut turquoise + 1 Fine beaver leather | Silver ore 18, Turquoise 16, Honey 11 (Beeswax) |
| `recipe.misc.bronze_turquoise_compass` — Bronze turquoise compass | 3 | 1 Bronze ingots + 1 Cut turquoise + 1 Glass | Copper 8 and Tin 12 (Bronze), Turquoise 16 |
| `recipe.misc.diamond_pocket_watch` — Diamond pocket watch | 4 | 1 Gold ingot + 1 Cut diamond + 1 Steel fittings | Gold ore 20, Diamond 24 |
| `recipe.misc.royal_gem_chalice` — Royal gem chalice | 4 | 1 Gold ingot + 1 Cut ruby + 1 Cut sapphire | Gold ore 20, Ruby 22, Sapphire 23 |
| `recipe.misc.lapis_jewel_box` — Lapis jewel box | 4 | 2 Oak planks + 1 Polished lapis + 1 Gold ingot | Lapis lazuli 23 (Polished lapis recipe known), Gold ore 20 |

**Eligibility** (section 14): the recipe tier is discovered, every named source is discovered, and each refined input has a known recipe (all are mission rewards in section 11.6) or an accessible exact shop. **Box pool effect:** the collectible pool grows from 52 to **59** (26 + 18 barrel + 8 ranch + 7 keepsakes). Part 2 section 10.3 continues with tiers 5-8.

### 9.6 Tiers 2-4: two equipment sets, more food and Misc (proposal, user request November 5, 2026)

**Why (user, November 5, 2026):** from tier two the regions stretch further (12-20 camps, legs up to 26 hearts) and each block's resources arrive over six regions. So, besides the Normal/Elite tools, **every tier from two up has two equipment sets**, and missions get **more Food and Misc recipes** to keep progression steady across the long stretches. Section 11.6 places every recipe below in the four branches. **Ingredient rule for these recipes (user):** a recipe uses only the most significant real-life ingredients that exist in the current catalog, one to three distinct inputs (four only for advanced backpacks). Real-life seasonings, binders and processing steps that have no catalog item are left out. All the usual rules still apply: exact IDs only, the {R-1, R} tier window for nonfood (section 9.0.1), the Food pantry exceptions, Wearable/Tool recipes x1 only, and timers by category and tier (section 10.2).

#### 9.6.1 The two sets: Set A (early regions) and Set B (late regions)

- **Set A** is built from the first three regions of the block (7-9, 13-15, 19-21) plus previous-tier materials. It reaches **halfway from the previous tier's limit to this tier's limit, rounded up** (boots in 0.05 m/s steps).
- **Set B** needs at least one material from the last three regions (10-12, 16-18, 22-24). It reaches the **tier limit** in the section 4.3 table and has **+25% base durability** over Set A at equal mastery, quality and furniture. The calibration target "tier-three gear at L10 Q5 about 10 km" applies to tier-three Set B; Set A lands near 8 km. Tier-four Set B at L10 Q5 is about 14 km.
- **Not Normal/Elite:** the two sets are separate garments with their own recipes, mastery, art and names. There is no upgrade recipe and nothing is consumed from Set A. Tier one keeps its single set (it already reaches the tier-one limit).
- **Balloon special gear (section 14.1)** rounds the same halfway value **down** and never wears out, so Set A is always equal or slightly stronger while special gear keeps its infinite durability. The global caps are unchanged.

| Tier | Set | Hat hearts | Boots m/s | Shirt pockets | Pants hooks | Backpack | Source regions |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 2 | A Marsh | 5 | 2.65 | 3 | 3 | 9 x 4 | 7-9 |
| 2 | B Taiga | 6 | 3.0 | 4 | 4 | 10 x 4 | 10-12 |
| 3 | A Prairie | 8 | 3.4 | 6 | 6 | 11 x 4 | 13-15 |
| 3 | B Highland | 9 | 3.75 | 7 | 7 | 12 x 4 | 16-18 |
| 4 | A Savanna | 11 | 4.15 | 9 | 9 | 13 x 4 | 19-21 |
| 4 | B Coastal | 12 | 4.5 | 10 | 10 | 14 x 4 | 22-24 |

**Implementation impact:** capabilities can no longer be looked up from the slot's rank alone (`EquipmentState.Hearts[Rank]` and similar), because Set A sits between ranks. Each wearable definition stores its explicit capability value (hearts, speed, pockets, hooks or backpack size). The rank stays as the tier for durability, prices and the section 4.3 caps. Saved gear keeps working because the tier-one items keep their current values.

**Set recipes** (IDs `recipe.wearable.<id>` -> `item.<id>`; branches as in section 11.1: Gatherer hats, Lumberjack boots, Hunter shirts, Explorer pants and backpacks). Rows marked *existing* are the section 9.3 garments, now assigned to Set A with the Set A capability above; their ingredients are unchanged.

| Tier / set | Slot | Item (ID) | Exact ingredients -> 1 | Latest source region |
| --- | --- | --- | --- | --- |
| 2 A | Hat | Beaver-fur cap (`beaver_fur_cap`) | 2 Fine beaver pelts + 1 Hemp thread (*existing* tier-two hat) | 7 |
| 2 A | Boots | Marsh boots (`marsh_boots`) | 2 Beaver leather + 1 Birch plank | 7 |
| 2 A | Shirt | Flax work shirt (`flax_work_shirt`) | 3 Flax cloth + 1 Beaver leather | 8 |
| 2 A | Pants | Flax trousers (`flax_trousers`) | 3 Flax cloth + 1 Copper fittings | 8 |
| 2 A | Backpack | Marsh pack (`marsh_pack`) | 4 Hemp cloth + 3 Beaver leather + 2 Birch planks + 2 Copper fittings (*existing* tier-two backpack) | 8 |
| 2 B | Hat | Taiga fur hat (`taiga_fur_hat`) | 1 Fine beaver pelt + 1 Fine flax cloth + 1 Cattail cord | 12 |
| 2 B | Boots | Taiga boots (`taiga_boots`) | 2 Fine beaver leather + 1 Tin ingot | 12 |
| 2 B | Shirt | Taiga waxed shirt (`taiga_waxed_shirt`) | 3 Fine flax cloth + 1 Fine beaver leather | 11 |
| 2 B | Pants | Taiga trousers (`taiga_trousers`) | 3 Fine flax cloth + 1 Cattail cord + 1 Copper fittings | 12 |
| 2 B | Backpack | Taiga frame pack (`taiga_frame_pack`) | 3 Fine flax cloth + 2 Fine beaver leather + 2 Cattail cord | 12 |
| 3 A | Hat | Prairie feather hat (`prairie_feather_hat`) | 2 Cotton cloth + 3 Turkey feathers | 14 |
| 3 A | Boots | Canyon boots (`canyon_boots`) | 2 Beaver leather + 1 Iron fittings + 1 Cotton cord | 14 |
| 3 A | Shirt | Prairie feather shirt (`prairie_feather_shirt`) | 3 Cotton cloth + 2 Turkey feathers + 1 Beaver leather (*existing* feather garment) | 14 |
| 3 A | Pants | Canyon trousers (`canyon_trousers`) | 3 Cotton cloth + 1 Iron fittings + 1 Flax cord | 14 |
| 3 A | Backpack | Canyon pack (`canyon_pack`) | 4 Cotton cloth + 3 Beaver leather + 2 Mesquite planks + 2 Iron fittings (*existing* tier-three backpack) | 14 |
| 3 B | Hat | Highland hat (`highland_hat`) | 2 Fine cotton cloth + 1 Turkey feather weave + 1 Cut turquoise | 16 |
| 3 B | Boots | Highland boots (`highland_boots`) | 2 Fine beaver leather + 2 Agave cord + 1 Iron fittings | 18 |
| 3 B | Shirt | Highland shirt (`highland_shirt`) | 3 Fine cotton cloth + 1 Fine beaver leather + 1 Sunflower oil | 17 |
| 3 B | Pants | Highland trousers (`highland_trousers`) | 3 Fine cotton cloth + 2 Iron fittings + 1 Agave cord | 18 |
| 3 B | Backpack | Highland frame pack (`highland_frame_pack`) | 4 Fine cotton cloth + 2 Fine beaver leather + 2 Mesquite beams + 2 Agave cord | 18 |
| 4 A | Hat | Savanna hide hat (`savanna_hide_hat`) | 2 Boar leather + 1 Fine cotton cloth | 19 |
| 4 A | Boots | Savanna boots (`savanna_boots`) | 2 Fine boar leather + 1 Steel fittings | 19 |
| 4 A | Shirt | Savanna shirt (`savanna_shirt`) | 4 Fine cotton cloth + 2 Fine boar leather (*existing* tier-four shirt) | 19 |
| 4 A | Pants | Savanna trousers (`savanna_trousers`) | 4 Fine cotton cloth + 2 Iron fittings + 1 Boar leather (*existing* tier-four pants) | 19 |
| 4 A | Backpack | Savanna pack (`savanna_pack`) | 4 Fine cotton cloth + 3 Fine boar leather + 2 Oak planks + 2 Steel fittings (*existing* tier-four backpack) | 19 |
| 4 B | Hat | Coral-coast sun hat (`coral_coast_sun_hat`) | 3 Bamboo cloth + 1 Fine boar leather | 24 |
| 4 B | Boots | Tide boots (`tide_boots`) | 2 Fine boar leather + 1 Bamboo cloth + 1 Steel fittings | 24 |
| 4 B | Shirt | Coastal shirt (`coastal_shirt`) | 4 Bamboo cloth + 2 Fine boar leather (*existing* section 6.2.3 second-set shirt) | 24 |
| 4 B | Pants | Coastal trousers (`coastal_trousers`) | 4 Bamboo cloth + 2 Steel fittings + 1 Fine boar leather | 24 |
| 4 B | Backpack | Coastal pack (`coastal_pack`) | 4 Bamboo cloth + 3 Fine boar leather + 2 Oak beams + 2 Steel fittings | 24 |

Tier-four Set B depends on Coral Shores' Bamboo fiber, the last region, so it is the capstone set of phase one. Every Set B garment also needs that block's late-region materials, so Set A is the set worn through the first half of each block. Art follows the faceted fitted-shell pipeline (`Docs/Art/Characters/PlayerDefault/player-avatar-designs.txt`), one item model and one fitted wearable per garment, themed by each set's landscape.

#### 9.6.2 New Material recipe: Beeswax

Beeswax is used by Birch panel, Birch beaver, Fine beaver leather, Fine cotton cloth, Mesquite panel and the Birch-wax bee toy, but had no source. **Proposed:** `recipe.material.beeswax`, tier two: **2 Honey -> 1 Beeswax**, fixed material timer, no mastery. It is crafted only, from region 11's Honey. All beeswax recipes therefore become craftable from region 11, which matches the Set B timing. **Revision reversed (November 7, 2026):** the Beaver toy keeps **1 Plant stuffing** as section 9.5 lists. The earlier switch to Flax stuffing was made because Plant stuffing had no unlock path; tier-one mission H8 (section 11.6.1.1) now teaches it, and the toy is the only use of Plant stuffing. Flax stuffing keeps its use in the tier-three Turkey toy.

#### 9.6.3 New Food recipes (mission rewards, ordinary 3-6 hearts)

Mission foods keep the ordinary range (premium 8-12 stays with boxes and the bakery). Values per serving rise by tier, so tier-four legs (14-26 hearts) can be crossed with 6-heart meals as section 6.2.4 assumes.

| Recipe ID / output | Tier | Exact ingredients -> yield | Hearts per serving | Source regions |
| --- | --- | --- | --- | --- |
| `recipe.food.beaver_carrot_stew` — Beaver-carrot stew | 2 | 2 Raw beaver meat + 2 Carrots + 1 Rock salt -> 2 | 5 | beaver 7-12, carrots 8 |
| `recipe.food.barley_bread` — Barley bread | 2 | 2 Barley flour + 1 Water -> 2 | 3 | 9 |
| `recipe.food.walleye_barley_soup` — Walleye-barley soup | 2 | 1 Walleye + 2 Barley + 1 Carrot -> 1 | 5 | walleye 8/10/11, barley 9 |
| `recipe.food.pike_barley_cakes` — Pike barley cakes | 2 | 1 Northern pike + 1 Barley flour + 1 Egg -> 2 | 4 | pike 8/9/12, barley 9 |
| `recipe.food.tomato_rice_bowl` — Tomato rice bowl | 2 | 2 Rice + 2 Tomatoes -> 2 | 4 | 11 |
| `recipe.food.honey_glazed_carrots` — Honey-glazed carrots | 2 | 3 Carrots + 1 Honey -> 2 | 4 | 8, 11 |
| `recipe.food.catfish_tomato_stew` — Catfish-tomato stew | 2 | 1 Channel catfish + 2 Tomatoes + 1 Rice -> 1 | 5 | catfish 7/10/12, 11 |
| `recipe.food.corn_turkey_pie` — Corn-turkey pie | 3 | 2 Corn flour + 1 Raw turkey meat + 1 Egg -> 2 | 5 | corn 13, turkey 13-18 |
| `recipe.food.pumpkin_cream_soup` — Pumpkin cream soup | 3 | 2 Pumpkins + 1 Cow cream -> 2 | 4 | 15 |
| `recipe.food.sturgeon_chickpea_pot` — Sturgeon-chickpea pot | 3 | 1 Sturgeon + 2 Chickpeas + 1 Rock salt -> 1 | 6 | sturgeon 14/15/18, 15 |
| `recipe.food.sunflower_corn_bread` — Sunflower corn bread | 3 | 2 Corn flour + 1 Sunflower seeds -> 2 | 4 | 13, 17 |
| `recipe.food.chili_bean_wrap` — Chili bean wrap | 3 | 1 Corn flatbread + 2 Beans + 1 Green chili -> 1 | 6 | 13, 14, 17 |
| `recipe.food.brown_trout_mint_bake` — Brown-trout mint bake | 3 | 1 Brown trout + 1 Wild mint + 1 Cow butter -> 1 | 6 | trout 13/16/18, 18 |
| `recipe.food.lake_trout_corn_chowder` — Lake-trout corn chowder | 3 | 1 Lake trout + 2 Corn + 1 Cow milk -> 1 | 6 | lake trout 14/16/17, corn 13 |
| `recipe.food.potato_red_drum_bake` — Potato red-drum bake | 4 | 1 Red drum + 2 Potatoes + 1 Cow butter -> 1 | 6 | drum 19/21/23, 19 |
| `recipe.food.gar_fish_cakes` — Gar fish cakes | 4 | 1 Longnose gar + 2 Potatoes + 1 Egg -> 2 | 5 | gar 19/20/22, potatoes 19 |
| `recipe.food.cacao_oat_bar` — Cacao oat bar | 4 | 2 Cacao powder + 1 Oats + 1 Honey -> 2 | 5 | 20 |
| `recipe.food.mocha` — Mocha | 4 | 1 Roasted coffee + 1 Cacao powder + 1 Cow milk -> 1 | 5 | 20, 21 |
| `recipe.food.coffee_roast_boar` — Coffee-roast boar | 4 | 1 Raw boar meat + 1 Roasted coffee + 1 Oak log -> 1 | 6 | boar 19-24, 21 |
| `recipe.food.vanilla_coconut_milk` — Vanilla coconut milk | 4 | 2 Coconuts + 1 Vanilla pods -> 2 | 5 | 22, 24 |
| `recipe.food.coconut_snapper_curry` — Coconut snapper curry | 4 | 1 Red snapper + 1 Coconut + 1 Green chili -> 1 | 6 | snapper 20/21/24, 24 |

Older Oats, Honey, Rock salt, Egg, Water and dairy enter through the existing pantry list. Green chili (tier three) is a regional vegetable within the tier-four window. No hidden fuel: only the boar roast lists wood, as section 9.4.1 requires.

#### 9.6.4 New Misc recipes (sale goods, mission rewards)

| Recipe ID / output | Tier | Exact ingredients -> 1 | Kind |
| --- | --- | --- | --- |
| `recipe.misc.beeswax_candle` — Beeswax candle | 2 | 2 Beeswax + 1 Flax cord | Craft good |
| `recipe.misc.sandstone_owl` — Sandstone owl | 2 | 2 Sandstone tiles + 1 Birch plank | Sculpture |
| `recipe.misc.granite_turkey` — Granite turkey statue | 3 | 3 Granite tiles + 1 Mesquite plank | Sculpture |
| `recipe.misc.slate_oasis_plaque` — Slate oasis plaque | 3 | 2 Slate tiles + 1 Cut turquoise | Sculpture |
| `recipe.misc.feather_dreamcatcher` — Feather dreamcatcher | 3 | 1 Agave cord + 3 Turkey feathers + 1 Mesquite branch | Craft good |
| `recipe.misc.silver_bell` — Silver bell | 3 | 1 Silver ingot + 1 Cotton cord | Jewelry/charm |
| `recipe.misc.marble_bust` — Marble boar bust | 4 | 3 Marble tiles + 1 Gold ingot | Sculpture |
| `recipe.misc.basalt_lantern` — Basalt lantern | 4 | 1 Polished basalt + 1 Steel fittings | Sculpture |
| `recipe.misc.carved_tusk_pendant` — Carved tusk pendant | 4 | 1 Boar tusk + 1 Cotton cord | Jewelry |
| `recipe.misc.bamboo_wind_chime` — Bamboo wind chime | 4 | 3 Bamboo fiber + 1 Gold ingot | Craft good |

Boar tusk is the boar's interaction-only byproduct (section 6.3); it needs a boar harvest output definition before this recipe activates.

#### 9.6.5 Fish collection missions and varied fish dishes (user, November 5, 2026)

**Every fish species from tier two up has its own collection mission** (section 11.6): the player brings that species and learns a fish dish. Discovery alone teaches nothing.

**Variety instead of plain roasts (user):** tiers two to four do not use the plain wood-roast fish recipes of section 9.4.1. Each species' mission teaches a different kind of dish (cakes, rice bowl, soup, stew, pot, chowder, bake, parcel or curry), using ingredients from the same block. **Tier one (revised November 7, 2026):** its four plain roasts are replaced by the Trout mushroom skillet, Salt-baked bass, Pine-nut bluegill and Apple-stuffed perch (section 9.4.1), next to Herb trout, so no tier uses plain fish roasts. **Meat variety (user, November 7, 2026):** the same rule now covers hunted meat. Roasted rabbit stays as the only plain roast: the implemented tier-one starter that defines the basic form. Each later tier's first hunting food is a different dish made with that block's early ingredients, instead of the same roast with more hearts: Blackberry-glazed beaver, Turkey carrot skewers and Acorn-crusted boar (section 9.4). The two near-copies Boar potato stew and Boar bean chili became Boar potato hash and Boar cacao mole. Each tier's meat dishes now differ in form (glaze, skewers, crust, hash, mole, stew, pot, pie, chili). Remaining close pair, left as is: Turkey bean stew (tomato) and Turkey chili (green chili) share 2 meat + 2 Beans within tier three.

| Tier | Species -> dish (mission) |
| --- | --- |
| 2 | Common carp -> Carp rice bowl; Northern pike -> Pike barley cakes; Channel catfish -> Catfish-tomato stew; Walleye -> Walleye-barley soup (a Rice mission also teaches the Pike rice bowl) |
| 3 | Salmon -> Salmon rice parcel; Sturgeon -> Sturgeon-chickpea pot; Brown trout -> Brown-trout mint bake; Lake trout -> Lake-trout corn chowder |
| 4 | Longnose gar -> Gar fish cakes; Tuna -> Seaweed rice parcel; Red drum -> Potato red-drum bake; Red snapper -> Coconut snapper curry |

The tree-food recipes and conversions (section 7.1.1) for birch, mesquite and oak are mission rewards too (Birch sap, Mesquite pods and Acorns requests in section 11.6), not discovery unlocks. Tier one's Toasted pine nuts stays as designed.

#### 9.6.6 Every recipe has a mission; the shop and mystery-box rule

**All recipes of tiers two to four are mission rewards (user, November 5, 2026),** including materials and the recipes that earlier had no source:
- **Tier 2:** Honey herbal tea, Honey oat drink, Bottled honey tea, Rabbit barley soup, Pike rice bowl, Copper pendant, Birch panel, Glass panels, Cut quartz.
- **Tier 3:** Pumpkin oat porridge, Mesquite nut cakes, Bronze ingots, Turquoise bracelet, Nails, Mesquite panel.
- **Tier 4:** Clay cup and Hot cacao cup, Oak panel, Ruby brooch.

The optional wool chain (Raw wool, Wool yarn, Wool cloth) has no source and no recipe that needs it, so it is **deferred** to the backlog rather than placed. Section 11.6.5 checks that no tier 2-4 recipe is left without a mission.

**Shop and mystery-box rule (user):** a recipe may move from a mission to a shop or the mystery box only if it is reworked into an **advanced (premium) recipe**:
- **Food:** 8-12 total hearts (section 9.4.2) and at least three distinct ingredients, one of them refined or from another region.
- **Misc:** at least three distinct ingredients including a refined material (ingot, cut gem, fine cloth or leather), priced as top Misc.

Simple one- or two-ingredient recipes and every Material, Tool and Wearable recipe stay mission rewards. This keeps shops and boxes as optional sources of strong recipes, never the only path to a basic one.

## 10. Crafting UI, timers, and bulk crafting

**Confirmed presentation:** keep the crafting menu and adjacent inventory open. Preserve the 3 x 3 ingredient grid, Craft/Autofill on its right, recipe book at top right, and Return at bottom right. Select a known recipe, exact ingredients and x1/x10/x100/x1000 for stackables; tools/wearables/backpacks are x1 only. Slots represent distinct ingredient types and reserved quantities, not one unit each. Preview resulting quality, applicable durability, total quantity, timer and bag footprint.

On Craft, validate location/ingredients/ownership/capacity/queue and consume ingredients atomically with creation of the output and batch record. Present the item or bulk **centered on the 3 x 3 grid**, showing quality, durability when applicable (otherwise no durability), and remaining timer for a proposed **1 second**, then animate it directly into the backpack. The menu stays open for the next recipe. This visual hold is not extra crafting time; the timer starts at committed consumption. Instant outputs show Instant. Prevent a second press from spending the same ingredients during the brief presentation.

The timed product **occupies real backpack space immediately after this drop**. It can be repositioned freely but cannot be eaten, equipped, sold, used as an ingredient, submitted to a mission, put on a shelf, or deposited in the warehouse until ready. The same batch's stackable outputs share one timer; split yield above 1,000 into legal stacks with a shared batch ID. Different pending batches never merge; ready outputs may merge by ordinary metadata rules. Tools/gear always remain unique x1 instances. Preserve the timer badge on the bag product; show a HUD ready indicator without closing/reopening crafting.

**Delegated question-5 default:** deterministically find free bag placement after removing inputs and reserve it before committing. Never rearrange existing player items. If there is no room, visibly route the output into the persistent overflow tray and animate toward its badge beside the backpack; the timer keeps running there. A pending output has exactly one owner (bag or tray), with a shared batch reference, never a duplicate item in both a bag and an invisible queue. **No craft-slot limit (confirmed October 31, 2026, replacing the earlier eight-record proposal):** any number of timed batches can run at once, so late-game x1 tools and gear never wait for a free slot. Ready bag outputs need no second claim; tray transfer requires room. Mastery is counted once when the craft starts (revised November 2, 2026, see section 10), whether the output sits in the bag or the tray; mission eligibility begins only when ready and carried.

Use `1 + 0.2 * log2(batchCount)` as the proposed stackable duration factor, including x1000; x1 gear uses factor one. Category/mastery curves and the less-than-one-hour Food/Tool limit remain. Materials have no mastery and gathered-material conversions stay instant. Snapshot output stats/furniture at start. Persist output owner/placement, shared timer, completion/mastery transaction and presentation state atomically; reload resumes remaining time without duplicate output, repeated consumption or mastery. Presentation animation can be skipped after reload. Crafting is allowed at camps and inside the player's home only.

### 10.1 Recipe mastery levels 1-10

**Confirmed:** every nonmaterial recipe has its own mastery progression from crafted quantity. Material conversions never level. Recipe level is independent of region/tool/equipment rank, five-grade item quality, and house level.

Recommended cumulative **finished-output-unit** thresholds:

| Recipe level | Total finished units required | Baseline quality before furniture | Base-price factor versus level-ten reference | Durability multiplier, where applicable |
| --- | --- | --- | --- | --- |
| 1 | 0 | Q1 | 0.8000 | 1.00 |
| 2 | 3 | Q2 | 0.8222 | 1.08 |
| 3 | 12 | Q3 | 0.8444 | 1.16 |
| 4 | 18 | Q4 | 0.8667 | 1.24 |
| 5 | 20 | Q5 | 0.8889 | 1.32 |
| 6 | 32 | Q5 | 0.9111 | 1.40 |
| 7 | 48 | Q5 | 0.9333 | 1.48 |
| 8 | 66 | Q5 | 0.9556 | 1.56 |
| 9 | 88 | Q5 | 0.9778 | 1.64 |
| 10 | 115 | Q5 | 1.0000 | 1.72 |

These thresholds are proposed test data, not confirmed final pacing. A batch producing 2,000 finished food servings counts 2,000 units, not one button press. A stackable x1000 craft can therefore reach level ten rapidly, which is useful for the requested limit tests. Tools/wearables advance through repeated x1 crafts; bulk requests remain unavailable. Do not slow mastery just to offset full-stack pickups.

Snapshot mastery and furniture at the batch's start; every output in that batch uses that snapshot even if it finishes enough units to gain several levels. **Revised (user, November 2, 2026): count the crafted units once at the moment of crafting, not when the timer completes.** Otherwise several same-level items could be crafted back to back without levelling up, and the player would have to wait for the timer to get the next level's bonus stats. A craft started while an earlier batch of the same recipe is still pending already uses the level that batch earned. Units are never counted on inventory transfer, repeated claim, sale or use. Batches saved before this change are counted once when they finish (`CraftBatch.MasteryCredited`). Partially claiming a ready batch does not grant more mastery. Instant materials contribute none. If multiple batches become ready together, each retains its own creation snapshot. A save transaction records both completion and credited units so reopening cannot farm mastery.

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

These four clothing rewards complete one set's hat/boots/shirt/pants; backpack comes in a later mission. Branch-to-slot assignments remain consistent in later tiers. Switching branches resumes its saved mission index; it does not create a new category-phase gate or reset another branch. **From tier two every tier has two clothing sets (user, November 5, 2026):** Set A from the block's early regions and Set B from its late regions, with independent recipes and stats within the existing caps (section 9.6.1). They do not add a fifth resource tier. Tier one keeps one set.

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
- Explorer E4: 10 Hemp fiber -> Hemp cord; E5: 10 Bone -> Normal pine rod, after cord/bone access (**request revised November 7, 2026** from Pine branches, which G1 already collects, so the rabbit's Bone drop has a collection mission; section 11.6.1.1); E6: 10 Mushrooms -> Roasted mushrooms (region 4 loose mushrooms; the reward needs only mushrooms and pine branches); E7: 10 Hemp cloth -> starter backpack after Hunter's Rabbit leather unlock; E8: 10 Rainbow trout -> Herb trout, offered only once farm Tea herbs are buyable (region 4 discovered and the farm settlement reachable), because the reward recipe needs one Tea herb. No mission consumes or collects Tea herbs.

These are proposed additions after the twelve specified starters. Never require fish before a rod recipe/source exists. Validate the reward recipe's craftability as well as the mission's requested item. The graph permits interleaving branches; it does not reinstate the old universal category sequence.

**Tiers two to four are planned mission by mission in section 11.6 (November 5, 2026)**, including the tier-one bridge missions that unlock the refinements tier two needs. From tier two, spread Normal/Elite axe/hunting/rod unlocks and the two clothing sets across the six-region block. Keep Gatherer hats, Lumberjack boots, Hunter shirts, and Explorer pants; assign backpack and advanced materials explicitly to one branch. A second set may improve durability or presentation while respecting ten pockets/hooks, twelve hearts, and brisk-walk caps. Use the 18-item featured source table and interaction-only drops to schedule mission availability; never request a region-14 material while region 14 is undiscovered.

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

**Instance requests (proposal, November 5, 2026):** missions that ask for tools or garments use authored lower counts of **20 / 30 / 40** in tiers two / three / four (section 11.6), because each instance is a separate x1 craft. Stackable requests keep 50 / 100 / 150. Keep tool-instance missions honest: a request for 150 tools means 150 finished unique tools through x1 crafts, not 150 tool uses, 150 partial durability units, or a newly introduced bulk recipe. Requests must name Normal or Elite IDs explicitly. A Normal-tool request cannot be filled by an Elite, and the Normal consumed in an upgrade cannot also count toward the active mission.

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

### 11.6 Mission plan for tiers 2-4 (proposal, user request November 5, 2026)

Every row is one mission: **consume the request -> learn one recipe**, as in tier one. The rules of sections 11.1-11.5 and the implemented mission region gate are unchanged:
- one active mission
- missions open by **mission region** (the region column): region N's missions open once region N-1 has none left to offer
- a mission stays locked, showing its first missing link, until its reward recipe is craftable

Recipes come from sections 8.4, 9.2-9.6 and 9.0.2. **Bold** rewards are the two equipment sets (section 9.6.1).

**Quantities:** stackable requests use the section 11.4 defaults: **50** in tier two, **100** in tier three, **150** in tier four. Tool and garment requests are unique x1 instances, so they use an authored lower override: **20 / 30 / 40** (section 11.4 allows lower authored values; x1 crafting workload is the reason). Prerequisites from another branch are noted the way tier one notes them (for example G3 needs the Pine bird recipe from L2).

**Ordering rules kept:**
- the Normal tool before its Elite
- previous-tier refinements before the Elite upgrades that consume them (section 8.4)
- every Set A before the Set B of the same slot
- each block finishes the refinements the next block's Elites need: Birch planks, Fine beaver pelts and Fine flax cloth in tier two; Mesquite planks, Turkey feather weave and Fine cotton cloth in tier three

#### 11.6.1 Tier-one bridge missions (regions 3-4 for construction; region 6 for the remaining bridges)

The tier-two Elite upgrades consume Pine planks, Fine rabbit pelts and Fine hemp cloth, and the first house levels need Fired bricks and Limestone tiles. Copper and tin smelting need Pine charcoal, and several tier-two recipes need Hemp thread and Wheat flour. None of these is unlocked by the 22 implemented tier-one missions. Ten units each:

| Code | Request | Reward recipe | Note |
| --- | --- | --- | --- |
| G6 | 10 Wheat | Wheat flour | Pantry flour for later Food |
| L6 | 10 Pine logs | Pine charcoal | Mission region 3; before L7; fuel for bricks and later smelting |
| H5 | 10 Animal fat | Cured rabbit pelt | **Revised November 7, 2026:** requests the rabbit's fat drop, which the revised pelt recipe (2 Rabbit pelts + 1 Animal fat) also uses |
| H6 | 10 Cured rabbit pelts | Fine rabbit pelt | Needs Hemp thread (E9) |
| E9 | 10 Hemp cord | Hemp thread | |
| E10 | 10 Hemp thread | Fine hemp cloth | |
| L7 | 10 Clay | Fired bricks | Mission region 3; needs Pine charcoal (L6); house levels 1-2 |
| L8 | 10 Limestone | Limestone tiles | Mission region 4 (house level 3) |

**Progression correction, November 6, 2026 (design; implementation review pending):** L6 and L7 both belong to mission region 3, in that order; L8 belongs to region 4. G6, H5, H6, E9 and E10 remain region 6. Stable codes are identifiers, not sort keys. The tier-one Lumberjack execution order is **L1 -> L2 -> L3 -> L4 -> L6 -> L7 -> L8 -> L5**, followed by tier-two missions. L5 (Mortar) remains region 6 and must not precede the construction bridges. Preserve the existing starter order and cross-branch prerequisites; do not globally sort by code or region.

**Implementation (Claude, November 6, 2026; 12:08 build installed, save kept):** implemented. `MissionBook` is mission catalog 2: Lumberjack runs L1-L4, **L6** (10 Pine logs -> Pine charcoal, region 3), **L7** (10 Clay -> Fired bricks, region 3), **L8** (10 Limestone -> Limestone tiles, region 4), then **L5** Mortar (region 6). The papers show each mission's position (L5 is "Mission 8 of 8"). Saves record the mission catalog; older saves are checked against the original L1-L5 order and then migrated once. Claims are kept and nothing is granted. An L5 already accepted may finish first, and the branch then continues with L6. Thirteen new Milestone 8 checks cover the order, the region gates, the first-home chain (G4, L6, L7 and L8 learned by the region-four missions without grants) and migration (L5 claimed, L5 active, round trips, forged flags rejected). All Milestone 1-8 suites pass. The phone save (L4 claimed, E6 active, mission region 4) loads, and its Lumberjack paper offers L6 next. Not yet played on the phone.

The first-home chain is source-reachable by settlement arrival after region 4: G4 Pine planks; L6 Pine charcoal from region-one branches; L7 Fired bricks from region-three Clay. L8 Limestone tiles uses region-four Limestone for the later level-three house. House level 2 still waits for region-six Reed braid, and level 3 for its tier-two refinements. No gift, debug grant, shop bypass or recipe-tier exception is needed for this design path.

**Ordering and migration contract:** author a separate execution ordinal/ordered-ID list per branch. Preserve mission/recipe IDs and quantities. Validate legacy saves against their original catalog before migration; retain claimed rewards and any active mission/delivered count without charging again. A migrated active mission may finish before newly inserted predecessors; then offer the first unclaimed mission in the revised order. Existing claims stay satisfied even when new predecessors are unclaimed. Never infer a new claim from a later claimed code or regrant an old reward. Version this migration and apply it once. New saves use the revised order immediately.

**Acceptance pending:** without debug bypasses, prove charcoal -> bricks -> level-one home is source-reachable by region-four settlement arrival, while Mortar/Reed braid and tier-two house gates remain later. Check branch switching, locked reward prerequisites and legacy saves before/after L5 with active/partially delivered missions. Keep full-stack testing and debug tools; these targeted checks do not certify campaign balance or complete natural progression. See [logic/testing review handoff](Docs/PROJECT_HANDOFF.md#logic-review).

#### 11.6.1.1 Tier-one coverage missions (design, November 7, 2026; implementation pending)

**Why (user, November 7, 2026):** every item that can be gathered, loose or from an interactable, needs a collection mission, and every tier-one material needs at least one use (a recipe or a house deposit). No new tier-one recipe is added: each mission below teaches a recipe that already existed without a mission. Ten units each; same region gate and one-active-mission rule.

| Code | Mission region | Request | Reward recipe | Needs / note |
| --- | --- | --- | --- | --- |
| G7 | 2 | Pine cones | Pine-cone ornament | Region-2 loose Pine cones |
| G8 | 2 | Pine nuts | Toasted pine nuts | Pine nuts from chopping (Normal pine axe, L1) |
| G9 | 5 | Rock salt | Cow cheese | Ranch Cow milk (settlement after region 4) |
| G10 | 5 | Oats | Cow yogurt | Ranch Cow milk |
| G11 | 5 | Strawberries | Strawberry cow-yogurt drink | Cow yogurt (G10) |
| G12 | 5 | Red berries | Berry oat cakes | Region-5 Oats |
| G13 | 5 | Cow yogurt | Cow cream | Crafted request; Cow cream feeds tier-two to tier-four missions (Truffle pot, Pumpkin cream soup, Vanilla cow-cream drink) |
| G14 | 5 | Cow cream | Cow butter | Crafted request; Cow butter feeds Mesquite pod cakes, Brown-trout mint bake and Potato red-drum bake |
| G15 | 6 | Cow cheese | Cow-cheese tart | Wheat flour (G6), ranch Egg |
| L9 | 5 | Pebbles | Polished pebble | Region-5 Pebbles, region-2 Flint |
| L10 | 5 | Polished pebbles | Cord charm | Hemp cord (E4) |
| L11 | 6 | Pine planks | Pine panel | House level 2 deposit (section 13.1) |
| H7 | 6 | Fine rabbit pelts | Fine rabbit leather | Nettle thread (E16), which replaces Hemp thread in Fine rabbit leather: 2 Rabbit leather + 1 Nettle thread -> 1. House level 4 deposit |
| H8 | 6 | Nettle fiber | Plant stuffing | Used by the tier-two Beaver toy (section 9.6.2 revision reversed) |
| E11 | 4 | Rainbow trout | Trout mushroom skillet | Normal pine rod (E5); Mushrooms (region 4) |
| E12 | 4 | Largemouth bass | Salt-baked bass | Bass regions 2/3/6; Rock salt (region 3) |
| E13 | 4 | Bluegill | Pine-nut bluegill | Bluegill regions 1/4/6; Pine nuts from chopping |
| E14 | 4 | Yellow perch | Apple-stuffed perch | Perch regions 2/4/5; Apples (region 1) |
| E15 | 3 | Nettle fiber | Nettle cloth | House level 2 deposit (replaces Hemp cloth there) |
| E16 | 6 | Nettle cloth | Nettle thread | Used by Fine rabbit leather |

**Dairy gate (user, November 7, 2026):** cream, yogurt and cheese are made from Cow or Goat milk, which is only sold at the ranch, so every milk-based recipe is unlocked **after region 4**: its mission region is 5 or later (G9, G10, G13 and G14 in region 5; G11 strawberry drink in region 5; G15 cheese tart in region 6). This gives the player a region to reach the settlement and its ranch first. The same applies to optional box and bakery offers that use milk, cream, butter, yogurt or cheese (Apple-oat compote, Red-berry oat porridge, Shrimp pie, Premium apple pie and later dairy recipes): offer them only once region 5 is discovered, not merely the settlement. No tier-one mission before region 5, and no tier-one fish dish, uses dairy.

**Changed existing missions:** E5 requests **10 Bone** instead of Pine branches (its Pine rod reward already needs Bone, and G1 already collects branches). H5 requests **10 Animal fat** instead of Rabbit leather, and Cured rabbit pelt becomes 2 Rabbit pelts + 1 Animal fat (section 9.2). E5 is in the implemented catalog, so changing its request needs the section 11.6.1 versioned migration: an E5 already accepted or partly delivered finishes on its old request.

**Branch execution order (explicit, not by code; revised November 7, 2026 for the prototype):** missions that need a settlement listing (ranch dairy, farm Tea herbs) go **last in their branch**. Settlements arrive in Milestone 10, and branches run in order, so a settlement-locked mission in the middle would block every later mission of its branch. Moving E8 keeps its order relative to every older mission (it still follows E7), so saves made under catalogs 1-2 validate unchanged.
- Gatherer: G1, G2, G3, G4, G7, G8, G12, G5, G6, then the dairy missions G9, G10, G11, G13, G14, G15
- Lumberjack: L1, L2, L3, L4, L6, L7, L8, L9, L10, L5, L11
- Hunter: H1, H2, H3, H4, H5, H8, H6, H7 (H8 runs before H6, which waits for Hemp thread from Explorer E9)
- Explorer: E1, E2, E3, E4, E5, E15, E6, E7, E11, E12, E13, E14, E9, E10, E16, then E8 (farm Tea herbs)

**Result:** tier one has 50 missions (22 implemented + 8 bridge + 20 coverage), about 0.71 per camp over 70 camps, close to tier two's density. Every tier-one loose resource and interaction drop (Pine logs and nuts; Rabbit meat, Bone and fat; all four fish) has a collection mission. Every tier-one material has a use. The only tier-one recipes still without a mission are Goat cheese and Mixed-milk cheese (no consumer either; keep them as ranch-milk side foods or move them to the backlog) and the optional box and bakery recipes.

#### 11.6.2 Tier two (regions 7-12; 50 units, 20 instances)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T2-G1 | 7 | Birch branches | Birch log | |
| T2-G2 | 7 | Blackberries | Blackberry juice | |
| T2-G3 | 7 | Birch sap | Birch sap milk drink | Sap from birch trees (the pine axe works one tier above) |
| T2-G4 | 8 | Carrots | Carrot soup | |
| T2-G5 | 9 | Blueberries | Blueberry jam | |
| T2-G6 | 9 | Quartz | Glass | Pine charcoal (L6) |
| T2-G7 | 9 | Quartz | Cut quartz | |
| T2-G8 | 9 | Blueberry jam | **Beaver-fur cap (2A hat)** | Fine beaver pelt (T2-H5), Hemp thread (E9) |
| T2-G9 | 9 | Barley | Barley flour | |
| T2-G10 | 10 | Barley flour | Barley bread | |
| T2-G11 | 10 | Flax seeds | Flax oil | |
| T2-G12 | 11 | Honey | Beeswax | |
| T2-G13 | 11 | Tomatoes | Tomato rice bowl | |
| T2-G14 | 11 | Carrots | Honey-glazed carrots | |
| T2-G15 | 11 | Honey | Honey herbal tea | Farm Tea herbs |
| T2-G16 | 11 | Honey herbal tea | Honey oat drink | |
| T2-G17 | 12 | Glass | Glass bottle | Container for later drinks |
| T2-G18 | 12 | Glass bottles | Bottled honey tea | |
| T2-G19 | 12 | Beeswax | Beeswax candle | Flax cord (T2-E2) |
| T2-G20 | 12 | Beeswax candles | **Taiga fur hat (2B hat)** | Fine flax cloth (T2-E10), Cattail cord (T2-E14) |
| T2-G21 | 10 | Lavender | Lavender pouch | Farm Lavender (region 10 discovered); listed last, opens with region 10 |
| T2-G22 | 10 | **10** Truffle | Creamy truffle-barley pot | Added November 7, 2026: the optional side mission of section 9.4.3, now in the table. Barley (region 9), Cow cream (G13). Lower authored quantity (10); listed last, never blocks other missions |
| T2-L1 | 7 | Birch logs | Birch plank | |
| T2-L2 | 8 | Copper ore | Copper ingot | Pine charcoal (L6) |
| T2-L3 | 8 | Copper ingots | Normal birch axe | |
| T2-L4 | 8 | Copper ore | Copper fittings | |
| T2-L5 | 9 | Birch planks | Birch beam | |
| T2-L6 | 9 | Birch beams | **Marsh boots (2A boots)** | Beaver leather (T2-H3) |
| T2-L7 | 9 | 20 Normal birch axes | Elite birch axe | Pine plank (G4) |
| T2-L8 | 9 | Cut quartz | Copper pendant | |
| T2-L9 | 10 | Sandstone | Sandstone tiles | |
| T2-L10 | 10 | Sandstone tiles | Sandstone owl | |
| T2-L11 | 11 | Birch logs | Birch charcoal | Fuel for tier-three iron and silver |
| T2-L12 | 11 | Beeswax | Birch beaver sculpture | Beeswax (T2-G12) |
| T2-L13 | 11 | Birch planks | Birch panel | Beeswax (T2-G12) |
| T2-L14 | 12 | Tin ore | Tin ingot | |
| T2-L15 | 12 | Tin ingots | **Taiga boots (2B boots)** | Fine beaver leather (T2-H11) |
| T2-L16 | 12 | Yellow ochre | Painted birch bird | |
| T2-L17 | 12 | Glass | Glass panels | |
| T2-H1 | 7 | Beaver pelts | Normal birch slingshot | Hemp cord (E4) |
| T2-H2 | 7 | Raw beaver meat | Blackberry-glazed beaver | Hunted with the pine slingshot (one tier above); Blackberries (region 7) |
| T2-H3 | 7 | Beaver pelts | Beaver leather | |
| T2-H4 | 7 | Beaver pelts | Cured beaver pelt | |
| T2-H5 | 8 | Cured beaver pelts | Fine beaver pelt | |
| T2-H6 | 8 | 20 Normal birch slingshots | Elite birch slingshot | Fine rabbit pelt (H6) |
| T2-H7 | 8 | Beaver leather | **Flax work shirt (2A shirt)** | Flax cloth (T2-E1) |
| T2-H8 | 8 | Carrots | Beaver-carrot stew | |
| T2-H9 | 9 | Fine beaver pelts | Beaver toy | Plant stuffing (H8), Hemp thread (E9); revised November 7, 2026 from Flax stuffing |
| T2-H10 | 9 | Barley | Rabbit barley soup | Farm Tea herbs; tier-one rabbit meat |
| T2-H11 | 11 | Beeswax | Fine beaver leather | Beeswax (T2-G12) |
| T2-H12 | 11 | Beaver toys | **Taiga waxed shirt (2B shirt)** | Fine flax cloth (T2-E10) |
| T2-H13 | 8 | Mushrooms | Beaver mushroom pot | Tier-one Mushrooms (region 4); listed last, opens with region 8 |
| T2-E1 | 8 | Flax fiber | Flax cloth | |
| T2-E2 | 8 | Flax fiber | Flax cord | |
| T2-E3 | 8 | Flax cloth | **Flax trousers (2A pants)** | Copper fittings (T2-L4) |
| T2-E4 | 8 | Flax cord | Flax stuffing | |
| T2-E5 | 8 | Birch logs | Normal birch rod | Copper fittings (T2-L4) |
| T2-E6 | 9 | 20 Flax trousers | **Marsh pack (2A backpack)** | Beaver leather (T2-H3) |
| T2-E7 | 9 | Northern pike | Pike barley cakes | Caught with the pine rod (one tier above) |
| T2-E8 | 10 | 20 Normal birch rods | Elite birch rod | Fine hemp cloth (E10) |
| T2-E9 | 10 | Walleye | Walleye-barley soup | |
| T2-E10 | 10 | Flax cloth | Fine flax cloth | Hemp thread (E9) |
| T2-E11 | 11 | Common carp | Carp rice bowl | |
| T2-E12 | 11 | Rice | Pike rice bowl | |
| T2-E13 | 11 | Channel catfish | Catfish-tomato stew | |
| T2-E14 | 12 | Cattail fiber | Cattail cord | |
| T2-E15 | 12 | Cattail cord | **Taiga trousers (2B pants)** | Fine flax cloth (T2-E10) |
| T2-E16 | 12 | 20 Taiga trousers | **Taiga frame pack (2B backpack)** | Fine beaver leather (T2-H11) |
| T2-E17 | 12 | Copper fittings | Fishing charm | |

#### 11.6.3 Tier three (regions 13-18; 100 units, 30 instances)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T3-G1 | 13 | Mesquite branches | Mesquite log | |
| T3-G2 | 13 | Corn | Corn flour | |
| T3-G3 | 13 | Corn flour | Corn flatbread | |
| T3-G4 | 13 | Mesquite pods | Mesquite flour | Pods from mesquite trees |
| T3-G5 | 13 | Mesquite flour | Mesquite pod cakes | |
| T3-G6 | 13 | Mesquite pods | Mesquite nut cakes | |
| T3-G7 | 14 | Beans | Turkey bean stew | |
| T3-G8 | 15 | Pumpkins | Pumpkin cream soup | |
| T3-G9 | 15 | Pumpkin cream soup | Pumpkin oat porridge | |
| T3-G10 | 15 | Corn flatbread | **Prairie feather hat (3A hat)** | Cotton cloth (T3-E1) |
| T3-G11 | 15 | Chickpeas | Chickpea stew | |
| T3-G12 | 16 | Soybeans | Soy milk | |
| T3-G13 | 16 | Soy milk | Tofu | |
| T3-G14 | 16 | Tofu | Tofu vegetable bowl | |
| T3-G15 | 16 | Turquoise | Cut turquoise | |
| T3-G16 | 17 | Sugar cane | Sugar | |
| T3-G17 | 17 | Sunflower seeds | Sunflower oil | |
| T3-G18 | 17 | Corn flour | Sunflower corn bread | |
| T3-G19 | 17 | Beans | Chili bean wrap | |
| T3-G20 | 18 | Wild mint | Mint tea | |
| T3-G21 | 18 | Mint tea | **Highland hat (3B hat)** | Fine cotton cloth (T3-E9), Turkey feather weave (T3-H3), Cut turquoise (T3-G15) |
| T3-L1 | 13 | Mesquite logs | Mesquite plank | |
| T3-L2 | 14 | Iron ore | Iron ingot | Birch charcoal (T2-L11) |
| T3-L3 | 14 | Iron ingots | Normal mesquite axe | |
| T3-L4 | 14 | Iron ore | Iron fittings | |
| T3-L5 | 14 | Iron ingots | Nails | |
| T3-L6 | 14 | Mesquite planks | Mesquite beam | |
| T3-L7 | 15 | Mesquite beams | **Canyon boots (3A boots)** | Cotton cord (T3-E3) |
| T3-L8 | 15 | 30 Normal mesquite axes | Elite mesquite axe | |
| T3-L9 | 15 | Mesquite planks | Mesquite panel | |
| T3-L10 | 15 | Granite | Granite tiles | |
| T3-L11 | 15 | Granite tiles | Granite turkey statue | |
| T3-L12 | 16 | Slate | Slate tiles | |
| T3-L13 | 16 | Slate tiles | Slate oasis plaque | Cut turquoise (T3-G15) |
| T3-L14 | 16 | Cut turquoise | Mesquite sun | |
| T3-L15 | 16 | Tin ingots | Bronze ingots | |
| T3-L16 | 16 | Bronze ingots | Turquoise bracelet | Cut turquoise (T3-G15) |
| T3-L17 | 17 | Mesquite logs | Mesquite charcoal | Fuel for tier-four steel |
| T3-L18 | 18 | Silver ore | Silver ingot | |
| T3-L19 | 18 | Silver ingots | **Highland boots (3B boots)** | Agave cord (T3-E12) |
| T3-L20 | 18 | Silver ore | Silver bell | |
| T3-H1 | 13 | Turkey feathers | Normal mesquite bow | |
| T3-H2 | 13 | Raw turkey meat | Turkey carrot skewers | Carrots (region 8) |
| T3-H3 | 14 | Turkey feathers | Turkey feather weave | Cotton thread (T3-E2) |
| T3-H4 | 14 | 30 Normal mesquite bows | Elite mesquite bow | |
| T3-H5 | 14 | Cotton fiber | Cotton stuffing | For the tier-four Boar toy |
| T3-H6 | 15 | Turkey feather weave | **Prairie feather shirt (3A shirt)** | Cotton cloth (T3-E1) |
| T3-H7 | 15 | Cotton stuffing | Turkey toy | The toy itself uses Flax stuffing (T2-E4) |
| T3-H8 | 15 | Raw turkey meat | Corn-turkey pie | |
| T3-H9 | 17 | Green chili | Turkey chili | |
| T3-H10 | 17 | Sunflower oil | **Highland shirt (3B shirt)** | Fine cotton cloth (T3-E9) |
| T3-H11 | 18 | Agave fiber | Feather dreamcatcher | Agave cord (T3-E12) |
| T3-E1 | 14 | Cotton fiber | Cotton cloth | |
| T3-E2 | 14 | Cotton fiber | Cotton thread | |
| T3-E3 | 14 | Cotton fiber | Cotton cord | |
| T3-E4 | 14 | Cotton cloth | **Canyon trousers (3A pants)** | Iron fittings (T3-L4) |
| T3-E5 | 14 | Mesquite branches | Normal mesquite rod | Iron fittings (T3-L4), Cotton thread (T3-E2) |
| T3-E6 | 15 | 30 Canyon trousers | **Canyon pack (3A backpack)** | |
| T3-E7 | 15 | 30 Normal mesquite rods | Elite mesquite rod | |
| T3-E8 | 15 | Sturgeon | Sturgeon-chickpea pot | Caught with the birch rod (one tier above) |
| T3-E9 | 16 | Cotton cloth | Fine cotton cloth | |
| T3-E10 | 16 | Lake trout | Lake-trout corn chowder | |
| T3-E11 | 17 | Salmon | Salmon rice parcel | Farm Tea herbs |
| T3-E12 | 18 | Agave fiber | Agave cord | |
| T3-E13 | 18 | Agave cord | **Highland trousers (3B pants)** | |
| T3-E14 | 18 | 30 Highland trousers | **Highland frame pack (3B backpack)** | |
| T3-E15 | 18 | Brown trout | Brown-trout mint bake | |
| T3-E16 | 18 | Silver ingots | Silver pendant | |
| T3-E17 | 18 | Agave cord | Desert charm | |

#### 11.6.4 Tier four (regions 19-24; 150 units, 40 instances)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T4-G1 | 19 | Oak branches | Oak log | |
| T4-G2 | 19 | Acorns | Roasted acorn meal | Acorns from oak trees |
| T4-G3 | 19 | Roasted acorn meal | Acorn biscuits | |
| T4-G4 | 20 | Cacao beans | Cacao powder | |
| T4-G5 | 20 | Cacao powder | Hot cacao | |
| T4-G6 | 20 | Hot cacao | **Savanna hide hat (4A hat)** | Boar leather (T4-H3) |
| T4-G7 | 20 | Cacao powder | Cacao oat bar | |
| T4-G8 | 20 | Gold ore | Gold ingot | Oak charcoal (T4-L2) |
| T4-G9 | 20 | Clay | Clay cup | Tier-one Clay from region 3; the cup is a tier-one container recipe |
| T4-G10 | 20 | Clay cups | Hot cacao cup | |
| T4-G11 | 21 | Coffee beans | Roasted coffee | |
| T4-G12 | 21 | Roasted coffee | Sweet coffee | |
| T4-G13 | 21 | Sweet coffee | Mocha | |
| T4-G14 | 21 | Emerald | Cut emerald | |
| T4-G15 | 22 | Vanilla pods | Vanilla cow-cream drink | |
| T4-G16 | 22 | Ruby | Cut ruby | |
| T4-G17 | 23 | Sapphire | Cut sapphire | |
| T4-G22 | 23 | Lapis lazuli | Polished lapis | Added November 6, 2026 (code appended) |
| T4-G18 | 24 | Coconuts | Coconut drink | Glass bottle (T2-G17) |
| T4-G19 | 24 | Coconuts | Vanilla coconut milk | |
| T4-G20 | 24 | Diamond | Cut diamond | |
| T4-G21 | 24 | Coconut drink | **Coral-coast sun hat (4B hat)** | Bamboo cloth (T4-E11) |
| T4-L1 | 19 | Oak logs | Oak plank | |
| T4-L2 | 19 | Oak branches | Oak charcoal | |
| T4-L3 | 19 | Iron ingots | Steel ingot | Mesquite charcoal (T3-L17) |
| T4-L4 | 19 | Steel ingots | Normal oak axe | |
| T4-L5 | 19 | Iron ore | Steel fittings | |
| T4-L6 | 20 | Oak planks | Oak beam | |
| T4-L7 | 20 | Oak beams | **Savanna boots (4A boots)** | Fine boar leather (T4-H5) |
| T4-L8 | 20 | 40 Normal oak axes | Elite oak axe | |
| T4-L9 | 20 | Oak planks | Oak panel | |
| T4-L10 | 20 | Marble | Marble tiles | |
| T4-L11 | 20 | Marble tiles | Marble boar bust | Gold ingot (T4-G8) |
| T4-L12 | 21 | Basalt | Polished basalt | |
| T4-L13 | 21 | Polished basalt | Basalt lantern | |
| T4-L14 | 21 | Oak planks | Oak boar sculpture | |
| T4-L15 | 22 | Obsidian | Obsidian pendant | |
| T4-L16 | 24 | Bamboo fiber | Bamboo wind chime | |
| T4-L17 | 24 | Bamboo wind chimes | **Tide boots (4B boots)** | Bamboo cloth (T4-E11) |
| T4-H1 | 19 | Boar hides | Normal oak bow | |
| T4-H2 | 19 | Raw boar meat | Acorn-crusted boar | Roasted acorn meal (T4-G2) |
| T4-H3 | 19 | Boar hides | Boar leather | |
| T4-H4 | 19 | Potatoes | Boar potato hash | Cow butter (G14) |
| T4-H5 | 20 | Boar leather | Fine boar leather | |
| T4-H6 | 20 | 40 Normal oak bows | Elite oak bow | |
| T4-H7 | 20 | Fine boar leather | **Savanna shirt (4A shirt)** | |
| T4-H8 | 20 | Boar tusks | Carved tusk pendant | Boar tusk harvest output (section 9.6.4) |
| T4-H9 | 21 | Raw boar meat | Boar cacao mole | Cacao powder (T4-G4), Green chili (region 17) |
| T4-H10 | 21 | Roasted coffee | Coffee-roast boar | |
| T4-H11 | 21 | Fine boar leather | Boar toy | |
| T4-H12 | 22 | Cut ruby | Ruby ring | Gold ingot (T4-G8) |
| T4-H13 | 22 | Ruby rings | Ruby brooch | |
| T4-H14 | 24 | Boar toys | **Coastal shirt (4B shirt)** | Bamboo cloth (T4-E11) |
| T4-E1 | 19 | Oak branches | Normal oak rod | Steel fittings (T4-L5) |
| T4-E2 | 19 | Fine cotton cloth | **Savanna trousers (4A pants)** | Boar leather (T4-H3) |
| T4-E3 | 19 | Red drum | Potato red-drum bake | Caught with the mesquite rod (one tier above) |
| T4-E4 | 20 | 40 Savanna trousers | **Savanna pack (4A backpack)** | |
| T4-E5 | 20 | 40 Normal oak rods | Elite oak rod | |
| T4-E6 | 20 | Longnose gar | Gar fish cakes | |
| T4-E7 | 21 | Cut emerald | Emerald ring | |
| T4-E8 | 23 | Tuna | Seaweed rice parcel | |
| T4-E9 | 23 | Seaweed | Lapis bracelet | Request revised November 7, 2026 from Lapis lazuli (still collected by T4-G22), so Seaweed has a collection mission |
| T4-E10 | 23 | Cut sapphire | Sapphire pendant | |
| T4-E11 | 24 | Bamboo fiber | Bamboo cloth | |
| T4-E12 | 24 | Bamboo cloth | **Coastal trousers (4B pants)** | |
| T4-E13 | 24 | 40 Coastal trousers | **Coastal pack (4B backpack)** | |
| T4-E14 | 24 | Red snapper | Coconut snapper curry | |
| T4-E15 | 24 | Cut diamond | Diamond necklace | |

#### 11.6.5 Summary and checks

- **Tiers 5-8** follow the same rules, with no new stats beyond tier four: [Part 2](PROJECT_RECOMENDATIONS_PART_2.md) sections 13-15 (180 missions, including nine gem-processing missions added November 6, 2026).
- **Count:** 8 tier-one bridge missions, 20 tier-one coverage missions (section 11.6.1.1) and 69 / 69 / 68 missions in tiers two / three / four (234 new; 256 with tier one's 22). Tier four includes Polished lapis, added November 6, 2026; tier two includes the Truffle mission T2-G22, added November 7, 2026.
- **Rewards per tier:**
  - both equipment sets (10 garments) and the six tools
  - the block's materials and refinements
  - every Food recipe, including one dish per fish species and the tree foods
  - every Misc recipe
- **Mission density:** about 0.6-0.85 missions per camp. Tier two has 79 camps, tier three 95 and tier four 108 (section 6.2.2); tier one has 50 missions over 70 camps (section 11.6.1.1). Each region still opens a new batch through the region gate.
- **Load:** the last region of each block carries the Set B garments, so the block finishes with its best gear. Tier four's Coral Shores carries 12 missions.
- **Automated checks (when implemented):**
  - every request has a source no later than its mission region
  - every reward is craftable when offered
  - the Normal tool before its Elite, and the previous-tier refinement before its Elite
  - Set A before Set B per slot
  - no duplicate reward
  - every tier 2-4 recipe is a mission reward unless it is a premium shop or box recipe (section 9.6.6)
  - each tier's four fish species have a collection mission (tier one included, November 7, 2026)
  - every gatherable item, loose or from an interactable (logs, tree food, meat, bones, fat, trophies, fish), has a collection mission in its tier (November 7, 2026)
  - every Material has at least one use: a recipe input or a house-upgrade deposit (November 7, 2026)
  - every Set A and Set B value as in section 9.6.1
  - instance requests at 20/30/40
  - mission foods at or below 6 hearts

## 12. Camps, trading, currency, and travel

### 12.1 Camps and conveyor trading

Each camp has a campfire and four log seats: one player and three NPC traders. The mission NPC stands separately. **Mission lady appearance (confirmed October 31, 2026):** she appears at the starting camp for the **first mission selection**, and afterwards only at the camp where the active mission is **completed**, to hand the recipe to the player and offer the next mission selection; at other camps she is absent. The exact hand-off and selection flow is defined with the mission implementation (Milestone 8). During the current testing phase she stands at every camp.

**Confirmed camp trader looks (October 30, 2026; meshes in progress):** the three seated traders use general male and female body meshes with a range of skin tones (six proposed). Each wears a **full clothing set** (hat, boots, shirt, pants) picked at random from tier 0 (starter rags: bare head and feet, rag shirt/pants) and every **discovered** set; a set is discovered once **at least one of its recipes is unlocked** (tier 1: Rabbit-pelt hat, Rabbit-pelt boots, Hemp shirt, Hemp pants). The **backpack** is picked independently among the rag pack and the backpacks whose own recipe is unlocked, so a tier 1 outfit can carry the rag pack and vice versa. Optional second sets within a tier count as separate choices once discovered. Looks are cosmetic only: they are not trade stock, show no quality or durability, and are not saved; they are seeded by camp and visit number, so they stay the same for a visit (also across a reload) and change on the next visit; a recipe unlocked during a visit (a mission claim) shows from the next visit. The three traders of one camp differ in body or skin tone. **Seated idle:** subtle breathing, head turns and posture shifts with a random phase and speed (0.85-1.15x) per trader so they never move in sync; idles must keep feet and hips on the log. Engine-free selection lives in `CampNpcs` (Domain/Travel) with Milestone 5 checks; it needs the Milestone 7 recipe-knowledge state as input, so until then every trader wears tier 0 unless a debug recipe set is supplied. The standing mission NPC and the toll NPC are separate characters and outside this rule.

**Confirmed camp approach, seating and departure (October 30, 2026):** a camp is a visible clearing **beside the road, on its left or right side**, never on the path itself. Arriving at a segment's end camp stops forward travel; the player then leaves the road toward that camp's side and **sits on the player's seat around the campfire**. Sitting sets walking speed to zero. While seated the player can trade with the three NPC traders (the 15-second conveyor session below) and, if that camp hosts the balloon merchant, buy from the balloon shop (section 14). The player stays seated until making the **upward speed gesture**; only then do they stand, walk back to the road, and resume walking along the next segment at the requested speed. Downward/zero speed gestures, menus, trading, the map, inventory and app restarts do not stand the player up. Persist seated state so a reload returns to the seat, never mid-road.

**Confirmed camp details (approved October 30, 2026):** author each camp's side (`Left`/`Right`), a road exit point and a seat anchor, and play short scripted approach/return moves along a fixed local path, with gesture input blocked during them. They are camp transitions, not trail distance, so they cost no stamina and add no gear wear. Seated time is the camp safe area: regeneration runs at the shared 1/3 heart/second, including while trading. Camera orbit stays available while seated. Arrival at the end camp is the trigger for that camp's arrival records and for the camp-return drop-cycle rule in section 7.3; standing up and leaving does not roll again. The 15-second trading session is one opportunity per camp visit. Leaving and returning creates a new opportunity as requested; restarting the app during the same visit does not.

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

**Confirmed:** camp stamina regeneration continues throughout the animation, including while placing/buying listings. At 1/3 heart/second, 15 active seconds gives up to 5 hearts; canceling after five gives up to 1.67 hearts, subject to the current cap. Input exclusivity does not pause regeneration or background craft readiness. Defer unrelated notifications/popups until trading ends. Recommendation for OS suspension: freeze/persist the remaining trading time and foreground regeneration, then resume the same session on return; never grant a new 15-second opportunity.

### 12.2 Currency definitions and arithmetic

Use two integer wallets with distinct icons and IDs:

- `Cash`: camp trades, river tolls, basic shops, home upgrades/furniture where specified.
- `Gold`: settlement lumber-tier unlocks, balloon stock, premium bakery goods, selected long-term unlocks.
- `GoldOre` and `GoldIngot`: item definitions; never mutate the Gold wallet when smelting.

Store currency as integer minor units and 64-bit balances/totals. **Every price below is per item unit, not per stack.** Preserve fractional intermediate unit values in fixed-point form and round once at the final transaction, not once per modifier/unit. Recommendation: show whole-cash totals rounded down; the cheapest ordinary Q1 material unit is one cash. Snapshot intrinsic value at craft creation, never recalculate an old item's price from the player's latest recipe level/furniture.

### 12.2.1 Price anchors, category order, and scope

**Confirmed base-price order:** Misc (sculptures/toys/jewelry; filled barrels are priced as Misc, section 9.4.5) > Backpacks > Clothing > Tools > Advanced materials > Food > Other materials/ingredients > Basics. Backpacks are Wearable recipes for timer/durability rules but have their own higher price category. Raw milk/eggs/fiber/ore can be Other ingredients; prepared cheese/meals/drinks are Food; refined cloth/leather/beams/glass containers can be Advanced materials. A recipe's timing role and its price category are separate explicit fields.

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

For ordinary sales, apply sale-price furniture only to eligible player-crafted goods: `quotedSaleTotal = sum(conditionAdjustedIntrinsicValue) * (1 + eligibleSaleBonus)`. Non-player-crafted goods use zero bonus. Then apply the camp purchase/80% fallback/70% cancellation factor once. Warehouse uses `sum(conditionAdjustedIntrinsicValue)` at 100%, with **zero** sale bonus and its separate gold cycle (1 gold per 1,000 cash, section 12.2.4). Preserve fixed-point precision and round cash only at payout; record actual paid cash as the gold-cycle basis. Retain creation/quality/durability premiums already in the profile without recalculating them. Selling never grants mastery.

A full 1,000-unit Q5 Misc stack at the 6,000 base reference is worth **12,000,000 cash** before sale buffs, not 6,000 cash for the entire stack. A full stack of one-cash basics is worth 1,000. These large totals are deliberate consequences of confirmed mechanics testing. Keep 1,000-unit pickups unchanged, support 64-bit totals and large ledger/UI displays, and report extreme money/gold throughput for later balance review. Gold shop prices remain separately authored; gold ore/ingots never directly become currency.

Store offers must quote their **Q5 product** from the outset. Any authored markup or fixed gold price is separate from intrinsic resale value. Do not display a Q1 price and secretly apply Q5 after purchase, or multiply an already-Q5 store profile by the Q5 factor a second time. With the proposed factor, a basic material with one-cash Q1 base has Q5 intrinsic value two cash before store markup; the one-cash base anchor remains unchanged for Q1 goods from other sources. Store quality does not alter full-stack quantities or grant mastery. Camp-trade listings are priced from their own actual quality/profile and do not become Q5 on transfer.

**Confirmed warehouse settlement:** every completed ten-unit box pays **100% of the deposited items' cash sale value**, with no furniture sale bonus, category bonus, extra payout, or cash-to-gold exchange denominator. Track the exact cash paid for each ten-box cycle. On the tenth completed box grant `gold = floor((cycleCash + cashRemainder) / 1000)`; retain `cashRemainder = (cycleCash + cashRemainder) mod 1000` as an integer and reset the cycle cash/count atomically. **Revised October 31, 2026 (was 3%):** the exchange is **1 gold per 1,000 cash**; the mechanic is unchanged (gold only after every ten completed boxes, no extra bonuses). Example: ten boxes paying 10,000 cash produce **10 gold**; 12,750 cash produce 12 gold and carry 750. The remainder carry prevents repeated rounding loss; no additional reward or mastery is granted.

**Integer-only acquisition (confirmed October 31, 2026):** every cash and gold amount the player receives is a whole number. Internal per-unit values may keep fixed-point precision, but each payout (camp sale, 80% fallback, 70% cancellation, warehouse box, gold cycle) is rounded down to an integer once, at the final transaction; the warehouse carries its cash remainder as an integer. Wallets never hold fractions.

Do not rebalance away the large rewards caused by full-stack testing. Add transaction logs and reports for purchases, crafts, wear cost, box sales, gold conversion, and repeatable route income. Later compare net outcomes to detect shop-buy/warehouse-sell or shredding loops that generate risk-free value. Repeated free rerolls and ferry/map shortcuts should be measured before changing the required behavior.

### 12.3 Rivers, maps, and fast travel

**Camp map look (confirmed by the user, October 31, 2026):** the map looks like a real worn-out paper map, each region on its own themed sheet (Dawn Plains meadows and sun, Windward Hills hills and wind, Mistwood forest and fog, Fern Hollow ferns, Cloudstep Mountains peaks and clouds, Reedwater Delta channels and reeds). The background shows the **road** winding from the region's first camp up to the river and ferry; every camp is a **button with a campfire icon and its name beside it**, placed on the side of the road where the camp stands. **Undiscovered camps show as inactive buttons**; the current camp is highlighted. The sheet is headed by the region's three featured resource icons with small tree/animal/fishing indicators; other discovered regions are further pages (travel to them arrives at their first camp). The complete region-one layout (ten camps) was planned first.

- Region borders end at a camp, with a river, ferry boat, and toll NPC just outside it.
- Pay each border toll once. Persist the unlock before changing scenes; insufficient cash leaves the player at the camp with options to trade or revisit discovered locations.
- **Ferry toll table (proposal, October 31, 2026):** tolls rise steeply so that each new tier is a real savings goal; the crossing out of tier three reaches about 2.2 million cash. The gold alternative follows the warehouse exchange: `goldToll = max(1, floor(cashToll / 1000))`. Region 24 has no outgoing ferry.

| Crossing from region | Cash toll | Gold toll |
| --- | --- | --- |
| 1 -> 2 | 500 | 1 |
| 2 -> 3 | 1,000 | 1 |
| 3 -> 4 | 2,000 | 2 |
| 4 -> 5 (settlement lies between) | 4,000 | 4 |
| 5 -> 6 | 8,000 | 8 |
| 6 -> 7 (into tier 2) | 15,000 | 15 |
| 7 -> 8 | 25,000 | 25 |
| 8 -> 9 | 40,000 | 40 |
| 9 -> 10 | 60,000 | 60 |
| 10 -> 11 | 90,000 | 90 |
| 11 -> 12 | 130,000 | 130 |
| 12 -> 13 (into tier 3) | 180,000 | 180 |
| 13 -> 14 | 250,000 | 250 |
| 14 -> 15 | 400,000 | 400 |
| 15 -> 16 | 650,000 | 650 |
| 16 -> 17 | 1,000,000 | 1,000 |
| 17 -> 18 | 1,500,000 | 1,500 |
| 18 -> 19 (into tier 4) | 2,200,000 | 2,200 |
| 19 -> 20 | 3,000,000 | 3,000 |
| 20 -> 21 | 4,000,000 | 4,000 |
| 21 -> 22 | 5,000,000 | 5,000 |
| 22 -> 23 | 6,500,000 | 6,500 |
| 23 -> 24 | 8,000,000 | 8,000 |

Reasoning: early crossings cost a few full stacks of basics or a handful of crafted goods, so newcomers are never stuck; by tier three a crossing equals hundreds of top crafted goods, matching the scale of late-game full-stack income (a full stack of level-ten Q5 tier-three Misc is worth over 7 million cash). Values are proposals for the post-debug rebalance.
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

**Confirmed trail HUD layout (October 30, 2026; presentation work, not yet implemented):** the pickup touch area extends up to just below the top buttons. The strip above it is reserved for the stamina display, the upper settlement-travel button, and a **durability warning silhouette** of the player. During tool interactions the strip also shows a small model of the tool in use with its remaining-uses counter, plus a warning when a tool breaks (section 8.2.1). Each worn piece below **10% durability** shows its body area in red: head for the hat, torso for the shirt, legs for the pants, feet for the boots; a low-durability backpack shows a red backpack in place of the player silhouette.

| Context | Available actions |
| --- | --- |
| Between camps | Avatar vertical speed gestures, independent horizontal 360-degree camera orbit, nearby resources including behind the player, harvest, inventory, contextual Eat, pause/settings |
| Camp, not trading | Inventory, crafting, equipment, regional map, settings; settlement shortcuts after discovery; stamina regenerates |
| Settlement street | Inventory, equipment, world map, settings, Home/Warehouse/Lumber/Return to Trail; no crafting |
| Inside home | Settlement actions plus crafting, furniture store, house upgrades, storage, pets after level three |
| Settlement shop | Context UI owns input; prevent click-through to other buildings; stamina regenerates |
| Active camp trade | Trading/listing actions and Return/cancel only for 15 seconds; all other actions locked; stamina keeps regenerating |

Retain the two expandable action menus beside the inventory button. The lower menu expands upward for Craft/Equipment/Map/Settings; the upper expands downward for Home/Warehouse/Lumber/Return. Disable unavailable actions with a short reason where discoverability helps; hide Return outside settlement. Prototype portrait first as a recommendation because vertical resource swipes and the lower backpack aperture fit that composition; validate the full inventory/crafting layout on a physical phone before fixing orientation. This replaces the earlier landscape-first recommendation.

### 12.5 Item collector book (planned, user request October 31, 2026)

A collection book, opened from the user menu (book icon) wherever the menu is available (not during a harvest interaction or a trade session), showing every item in the game.

- **Sections by type:** Materials (basic and advanced), Ingredients (raw and refined, including fish), Food and drinks (crafted and store-bought), Tools, Clothing, Backpacks, Misc (sculptures, toys, jewelry, filled barrels) and Special gear. Within each section entries are **ordered by tier** (1-4), then by name.
- **Entry page:** a **3D model viewer** of the item (drag to rotate it freely; pinch to zoom is optional), with its name, tier, category and quality-free description; its **region(s) of origin** and how it is obtained (loose gathering in region N, tree/hunt/fishing in a region, settlement shop, mystery box, crafted); and the **unlocked recipes that use it** (as an ingredient) or make it. Locked recipes are not listed.
- **Discovery (clarified by the user):** before an entry is unlocked, the page shows **only how the item is obtained** (for example: Gathering, Tree / Hunting / Fishing harvest, Mission recipe, Mystery box, Settlement shop, Crafting); everything else, **including the 3D model**, name details, regions and recipes, stays hidden behind a dark silhouette. An entry unlocks on the **first gather** for gathered items, and on **unlocking its recipe** for crafted items (or the first time the item is obtained another way, such as a box or shop purchase). Harvest outputs count as gathered: they unlock when the player first **collects** them from a felled tree, a hunted animal or a fishing catch; merely finding the tree, animal or regional pond unlocks nothing. Unlocks are saved per item ID and never lost (faint loss, selling or using items does not remove entries). They are separate from region/source discovery and from recipe knowledge.
- **Model view:** reuse the equipment avatar approach (a dedicated camera rendering the item model to a texture, only while the page is open), with neutral lighting and a soft key light; tools and gear use their bag display models, so the book always matches what the player carries.
- **Counts:** each section header shows collected / total for that section, and the book shows an overall total, for completion goals.
- **When:** after the mechanics slice (Milestone 9 or later), once regional sources and recipe unlocks exist; it reads the same item, source and recipe catalogs, adds a save field for collected item IDs, and needs no new game rules.

## 13. Settlement, house, furniture, storage, and pets

The settlement is a side-scrolling selection scene; there is no direct character movement. Buildings are selectable destinations. Neighbor houses use seeded visual variants and house levels but have no simulated economy or real owners initially.

**Confirmed settlement layout and neighbour houses (user, November 2, 2026; supersedes the line above where they differ):**
- **Six house plots: three to the left of the settlement centre, three to the right.** The storefronts and the lumber field stand between the two groups of houses.
- **The player picks a plot.** On first discovery of the settlement the player selects one of the six plots for their house; the other five become neighbours. The choice is saved and does not change afterwards (no plot moving is planned).
- **All six start at house level 1.**
- **The player's house** levels up only through the material upgrades in section 13.1.
- **Each neighbour house** rolls a random number **1 to 3** at the start: the number of new regions the player must discover before it upgrades. A region counts as discovered when the player reaches **its first camp**. Each time a new region is discovered, every neighbour's count goes down by one. A neighbour whose count reaches zero upgrades one level and rolls a new 1-3 count. Example: a neighbour that rolled 1 upgrades at the next region; one that rolled 2 upgrades at the one after.
- **Shared exteriors.** Every house, the player's and the neighbours', uses the **same exterior design for its level**, so the street shows a mix of levels. The 8 level exteriors (section 13.1) are made later, together with the player-home interiors for each level, so that both are organised as one set.
- *Delegated defaults (proposals, adjust if needed):*
  - Neighbours stop at level 8. Their level can be above or below the player's; they are decoration only, with no economy or owners.
  - The counts and rolls are seeded and saved like other settlement state, so a reload never rerolls them.
  - Regions discovered before the settlement do not count; counting starts when the plot is chosen.
  - Several regions discovered while the settlement is not visited are applied one by one on the next visit.
- **Build order:** the storefronts first; the house exteriors and interiors later.

### 13.1 House progression and upgrade deposits

Keep eight **house levels**, distinct from four resource tiers: two house levels use each active tier's materials. **Depth growth (user, November 7, 2026; replaces the earlier vertical growth):** every house is **one storey**. Levels 1-3 widen (8, 10, 12 columns); every upgrade makes the house and its interior **longer toward the back**, never taller. The interior grid is **width x depth** on the floor: the room is seen from the front at an angle with the front wall cut away, row 0 is at the front and upgrades add rows at the back, so placed furniture keeps its coordinates. Deep rooms scroll front to back. Settlement after region four remains within tier one, so the first home/upgrade uses pine/early materials only.

| Level | Visual stage | Interior grid (width x depth) | Upgrade deposit to reach it (25 x tier per material type) |
| --- | --- | --- | --- |
| 1 | Small shed | 8 x 5 | Tier 1: welcome quest; 25 pine planks + 25 fired bricks |
| 2 | Extended shed | 10 x 6 | Tier 1: 25 pine beams + 25 fired bricks + 25 nettle cloth + 25 reed braids + 25 pine panels |
| 3 | Proper wooden home; pets unlock | 12 x 7 | Tier 2: 50 birch beams + 50 limestone tiles + 50 fine beaver leather + 50 glass panels + 50 mortar |
| 4 | Back room | 12 x 9 | Tier 2: 50 birch panels + 50 sandstone tiles + 50 fine hemp cloth + 50 fine rabbit leather + 50 copper fittings |
| 5 | Long hall | 12 x 11 | Tier 3: 75 mesquite beams + 75 granite tiles + 75 cotton cloth + 75 iron fittings + 75 nails |
| 6 | Extended hall | 12 x 13 | Tier 3: 75 mesquite panels + 75 slate tiles + 75 fine cotton cloth + 75 glass panels + 75 flax oil |
| 7 | Frontier manor | 12 x 15 | Tier 4: 100 oak beams + 100 polished basalt + 100 fine boar leather + 100 glass panels |
| 8 | Finished homestead | 12 x 17 | Tier 4: 100 oak panels + 100 marble tiles + 100 cut quartz + 100 steel fittings + 100 polished lapis |

**Material sinks (design, November 7, 2026):** five deposits gained a material that otherwise had no use or only a mystery-box use. Level 2 swaps Hemp cloth (many recipe uses) for Nettle cloth and adds Pine panels. Level 4 adds Fine rabbit leather, level 5 Nails, level 6 Flax oil (a wood finish; older-tier deposits are allowed, section 9.0.1) and level 8 Polished lapis. No level has more than five material types. Level 1 is unchanged, so the region-four first-home chain (G4, L6, L7) stays as tested. Each new material is a mission reward that unlocks no later than its level's regions: Nettle cloth E15, Pine panel L11, Fine rabbit leather H7, Nails T3-L5, Flax oil T2-G11, Polished lapis T4-G22.

**Tiers 5-8** add house levels 9-16 (two per tier), decorative furniture, four more shop looks and four lumber sections, with no stronger buffs: [Part 2](PROJECT_RECOMENDATIONS_PART_2.md) section 16. **Deposit growth (user, November 5, 2026):** each upgrade asks the same amount of every listed material: **25 x the level's resource tier**, so tier one asks 25, tier two 50, tier three 75 and **tier four 100** per material type. Part 2 continues the rule (125 / 150 / 175 / 200 for tiers 5-8). Fired bricks and Limestone tiles, needed by levels 1-3, are unlocked by tier-one bridge missions L7 and L8 (section 11.6.1). House levels 3-4 need tier two (region seven onward), levels 5-6 tier three (region thirteen onward), levels 7-8 tier four (region nineteen onward), plus relevant material-unlock missions on the new branch graph. No deferred species/global Materials phase is required. Reed braid/Mortar make level-two/three upgrades useful sinks for their region-six sources; level-one home construction remains possible at region-four settlement arrival. House upgrades unlock store listings, not free crafted oven/window/chest outputs.

The upgrade menu sits beside the furniture-store button and shows current level, next level, deposited/required units, and missing sources. Deposits transfer materials into house-upgrade escrow immediately; cancellation returns them through a tray. Upgrade completion, cost consumption, grid growth, and unlocks commit together. Use a migration rule if costs change after a save already contains deposits.

**House art revision 2, depth growth (Claude, November 7, 2026):** the eight exteriors are rebuilt as one-storey homes whose wall depth grows 3.0 / 3.5 / 4.0 / 5.0 / 6.0 / 7.0 / 8.0 / 9.0 m (rows x 0.5 m + walls), widths unchanged (4.6 / 5.6 / 6.8 m), roof peak 2.9 -> 4.9 m. Side windows (one per ~1.7 m of wall), timber bays between them and a second chimney from level 6 make the length readable; the front dormers and floor bands are gone. Interiors are floors with back and side walls whose windows match the exterior, a cut front sill with the door threshold, and rafter stubs only on the wall plates so nothing blocks the angled view. Prefab anchors: `GridDepthUnit` (0, 0, -0.5), `ScrollMin/Max` front to back, and `InteriorCameraAnchor` (about 50 degrees down from the front). `HouseArtMetadata.FurnishingGrid` is columns x rows; `GridAxisY` became `GridAxisDepth`. Proposed plot depth 10.6 m (manifest `PlotDepth`). Sheets: `house-design-overview.png` (three-quarter views), `house-interior-overview.png`, `eight-level-lineup.png`. **Pending:** the in-game interior camera, scrolling and placement UI (none is implemented yet), and a device look.

**House art, levels 1-8 (another tool, November 5, 2026; review by Claude the same day):** `Docs/PROJECT_HANDOFF.md#houses-2026-11-05`.
- **Delivered:** eight exterior prefabs shared by the player and the neighbours, eight matching cutaway interiors with the exact grids above (8x5 to 12x17), eight furnished studies (six furniture proxies per level, preview only), and separate `PlayerPetHome` and `PetCollectionContainer` prefabs. Prefabs are in `Resources/Settlement/Houses`, the source is `Tools/Art/generate_house_meshes.py`, and the menus are **The Trail Walker > Art > Build / Capture Eight House Levels**. The sheets `house-design-overview.png`, `house-interior-overview.png` and `six-plot-street-study.png` are in `Docs/Art/Settlement/Houses`.
- **Matches this section:**
  - pine, birch, mesquite and oak for the two levels of each tier
  - levels 1-2 widen (4.6 / 5.6 / 6.8 m) and the width stays fixed from level 3 while the house grows upward
  - the pet home left of the home front and the collection container to its right, shown from level 3
  - the street order (warehouse, bakery, ranch, farm, fishery, mine, tailor-material shop, lumber-field gap) between three plots on each side
  - no furniture, buffs, costs or save data changed
- **Fixed in review:** the six-plot street study cut off the outer house on each side (its frame covered about 102 m of a 130 m street). The frame now fits the whole street, so all six plots show. Recaptured; all Milestone 1-8 suites pass.
- **Open for art:**
  - the pet home has **one** model, but section 13.5 now gives it four levels (kennel, fenced run, pen with shelter, small barn), so it needs three more looks; Part 2 adds four more
  - levels 9-16 (Part 2 section 16) are to be designed as one set with these eight
  - the furniture proxies are placeholders for the 48 store furniture types
- **Pending integration and testing:** plot choice, house placement, upgrade escrow and costs, neighbour countdowns, the upgrade and furniture UI, vertical home scrolling, furniture placement, chests and the shelf, pets at house level 3, and device checks (draw calls, memory, frame time).

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

~~Buff unlocks remain when furniture is stored or moved.~~ **Superseded (user, November 7, 2026): only placed furniture gives its buff, and only once per type.** A type's buff counts while **any** piece of that type is placed in the house (confirmed November 7, 2026); a piece in house storage gives nothing. Duplicates never add a second buff (they are decorative, or extra containers for chests). Moving a piece inside the room keeps the buff. Craft-quality and equipment-life buffs are still snapshotted at craft start, so storing a piece only affects crafts started afterwards; food and sale buffs read the placed set at the moment of eating or selling. Decorative rugs, lamps, pictures and the twelve-position shelf can be variants within unlocked types or optional extras; do not count them as a seventh required type per level.

Footprints: most furniture 1 x 1, 2 x 1, or 2 x 2; largest 3 x 2. No individual piece occupies more than 15% of the level-one interior. Allow furniture to be stored in a catalog when not placed. A production-looking piece grants its listed buff initially; adding workstation minigames would be additional scope.

**Buff tracking (user, November 7, 2026):** buffs are tracked in the background only. There is no buff panel, total, progress counter or other visual measure. The buff registry is derived from the placed pieces (and their first-purchase records), never stored as a separate total.

**Furniture interaction, placement and storage (user, November 7, 2026):**
- **Chests are the only furniture with a touch action.** Touching a placed chest opens that chest's inventory **above** the player inventory, and items move between the two (section 13.3). Every chest is its own container.
- **Every other piece is used by holding it.** Holding a piece switches to a **bird's-eye (top-down) view of the house interior** with that piece selected, and the player moves it around the floor grid. A piece cannot be dropped onto cells another piece occupies: the other piece has to be moved first (no swapping or pushing). **Return** leaves the bird's-eye view and goes back to the normal interior view.
- **Buying a piece** opens the same bird's-eye view with the new piece already placed at the **nearest free space**. The player can move it, or just press Return. **If no space fits it, it goes to the house storage.**
- **House storage** holds bought pieces that are not placed. Stored pieces give no buff (rule above).

**Interaction details (proposed, confirmed by the user November 7, 2026):**
- Holding a chest also enters the bird's-eye view to move it (touch opens it, hold moves it), so chests follow the same placement rules.
- In the bird's-eye view, a **Store** button sends the selected piece to house storage, and the storage list lets the player pick a stored piece to place, which appears at the nearest free space. A chest must be **empty** to be stored; a full chest stays placed.
- "Nearest free space" means the free position closest to the room's front-centre (the door), searched row by row from the front. Bird's-eye placement snaps to whole grid cells. Furniture keeps its authored orientation (no rotation, as for inventory items).
- Long rooms (levels 4-8) scroll front to back in the bird's-eye view, as in the normal view.

**Proposed per-type footprints (width x depth in 0.5 m cells; every type a player owns always fits, using under 60% of the room):**

| Level | Footprints | All types so far / room cells |
| --- | --- | --- |
| 1 | Worktable 2x1, Bedroll 2x1, Market sign 1x1, Boot bench 2x1, Sewing basket 1x1, Hat peg 1x1 | 9 / 40 |
| 2 | Trouser rack 2x1, Chopping block 1x1, Sling stand 1x1, Rod rack 2x1, Bag frame 1x1, Pine chest 2x1 | 18 / 60 |
| 3 | Loom 2x2, Clay cookstove 2x1, Display counter 3x1, Sofa 3x1, Sewing desk 2x1, Hat cabinet 2x1 | 34 / 84 |
| 4 | Tailor's stool 1x1, Whetstone stand 1x1, Bow workbench 3x1, Tackle desk 2x1, Leather table 2x2, Birch chest 2x1 | 47 / 108 |
| 5 | Artisan bench 3x1, Pantry dresser 2x1, Trophy cabinet 2x1, Boot cabinet 2x1, Cloth press 2x2, Hat display 1x1 | 61 / 132 |
| 6 | Clothing armoire 2x1, Tool forge 2x2, Bow cabinet 2x1, Reel workshop 2x2, Pack-maker bench 3x1, Mesquite chest 3x1 | 79 / 156 |
| 7 | Master loom 3x2, Dining table 3x2, Gallery pedestal 1x1, Travel sofa 3x1, Fine-cloth workstation 3x1, Milliner bench 2x1 | 100 / 180 |
| 8 | Frontier wardrobe 2x1, Master tool station 3x2, Hunter's cabinet 2x1, Angler's cabinet 2x1, Expedition workbench 3x2, Reinforced oak chest 3x1 | 121 / 204 |

Because buffs need placed pieces, the remaining space is what duplicates, extra chests and decorations can use; a validation should keep "all owned types fit" true for every level whenever footprints change.

**Proposed prices (premium, cash; user, November 7, 2026: below the later ferry tolls of the same tier, but expensive):** every type at a level costs the same.

| House level (tier) | 1 (T1) | 2 (T1) | 3 (T2) | 4 (T2) | 5 (T3) | 6 (T3) | 7 (T4) | 8 (T4) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Cash per piece | 3,000 | 6,000 | 20,000 | 50,000 | 150,000 | 400,000 | 1,000,000 | 2,000,000 |
| Full level (six types) | 18,000 | 36,000 | 120,000 | 300,000 | 900,000 | 2,400,000 | 6,000,000 | 12,000,000 |

One piece stays under the same tier's late tolls (for example 2,000,000 against 3,000,000-8,000,000 in tier four) but above its early ones, and a full level costs several crossings, so the buffs are a premium goal for players who sell crafted goods. Duplicates cost the same. Cash only is proposed (no gold price); the toll's gold exchange (cash / 1,000) could be added later if wanted. Values are tuning data.

**Proposed IDs and save data:** IDs `furniture.l<level>.<name>` (for example `furniture.l3.loom`). The save records each owned piece (instance ID, definition ID, placed cell or "stored", chest container ID and contents) and the first-purchase record per definition. Buffs are recomputed from placed pieces on load.

**Art batch A delivered (Claude, November 7, 2026; art only):** the six level-1 types (Worktable, Bedroll, Market sign, Boot bench, Sewing basket, Hat peg) as production models in the faceted house palette, 188-456 triangles each, 0.27-1.67 m tall. Source `Tools/Art/generate_furniture.py` (catalog with IDs, categories, footprints and prices; it refuses a mesh outside its footprint), prefabs `Resources/Settlement/Furniture/Furniture<Name>.prefab` with `FurnitureArtMetadata` and a footprint selection box, transparent 256 px store icons in `Resources/UI/Furniture` (sprite import rule added), menus **The Trail Walker > Art > Build / Capture Furniture**. `FurnitureArtAssets.Validate()` runs 32 checks (IDs, six per level, footprint limits, price, mesh inside its footprint and under 2.0 m, all owned types fit with the nearest-free-space rule); report `Builds/Validation/Furniture-art-checks.txt`. Review renders in `Docs/Art/Settlement/Furniture` (lineup, level-1 room angled and bird's-eye, icon sheet). **Not implemented:** store, buffs, placement, house storage, chest inventories. **Art batch B delivered (Claude, November 7, 2026; art only):** the twelve level-2 (pine) and level-3 (birch) types: Trouser rack, Chopping block, Sling stand, Rod rack, Bag frame, Pine chest; Loom, Clay cookstove, Display counter, Sofa, Sewing desk, Hat cabinet. 156-444 triangles each (the Pine chest 276 with its lid), 0.57-1.90 m tall. The **Pine chest has a separate lid on a `LidPivot` hinge** at the back top edge, so the touch action can open it; a check requires a hinged lid on every chest. Validation is now 100 checks (adds six different buff categories per level and the chest lid); the room renders show every type owned so far placed by the nearest-free-space rule in the level 2 and level 3 rooms (`Docs/Art/Settlement/Furniture/level<N>-lineup / -room-angled / -room-birds-eye / -store-icons.png`). Batches C (levels 4-5) and D (6-8) remain.

**Art batch C delivered (Claude, November 7, 2026; art only):** the twelve level-4 (birch with copper fittings, as the level-4 house trim) and level-5 (mesquite, iron) types: Tailor's stool, Whetstone stand, Bow workbench, Tackle desk, Leather table, Birch chest; Artisan bench, Pantry dresser, Trophy cabinet, Boot cabinet, Cloth press, Hat display. 224-568 triangles each, 0.57-1.80 m tall. The Birch chest reuses the generic chest body and hinged lid (`chest_lid`) in birch and copper. The generator and the validation now also enforce the 600-triangle budget (the Tackle desk was trimmed from 636 to 540). Validation: 197 checks, including all 24 and all 30 owned types fitting the level 4 (12x9) and level 5 (12x11) rooms. Batch D (levels 6-8) remains.

**Art batch D delivered, furniture art complete (Claude, November 7, 2026; art only):** the eighteen level-6 (mesquite, iron), level-7 (oak, iron) and level-8 (oak, steel, as the level-8 house trim) types: Clothing armoire, Tool forge, Bow cabinet, Reel workshop, Pack-maker bench, Mesquite chest; Master loom, Dining table, Gallery pedestal, Travel sofa, Fine-cloth workstation, Milliner bench; Frontier wardrobe, Master tool station, Hunter's cabinet, Angler's cabinet, Expedition workbench, Reinforced oak chest. 180-592 triangles each, 0.57-1.97 m tall. All four chests share the hinged-lid design. **Glazed display cabinets are hollow** with framed doors and glint strips (the palette has no transparency), so the bows, rods and cups inside stay visible; the level-5 Trophy cabinet was rebuilt the same way. All **48 types** now exist as prefabs with metadata and transparent store icons; validation passes **316 checks**, including all 48 owned types fitting the level-8 (12x17) room together. Still to build in code: the furniture store, buff registry (placed pieces, once per type), bird's-eye placement, house storage and chest inventories.

**Reviewed (user, November 7, 2026):** the nearest-free-space rule stays even though it fills the front row by the door first, so the first purchases can stand in front of the doorway. No cells are reserved. Buying opens the bird's-eye view, so the player can move the new piece right away (or later by holding it).

**Art brief for the 48 production models (replacing the proxy studies):** the faceted house palette, with each level's wood matching its house tier (pine L1-2, birch L3-4, mesquite L5-6, oak L7-8). Each model fills its footprint (0.5 m per cell) with its pivot at the front-left cell corner and faces the street/camera side. Height up to 2.0 m. The top surfaces carry each piece's identity (tools on bench tops, cloth on the press), because both the angled interior view and the bird's-eye view look down on them. Wall-style pieces become free-standing (hat peg as a peg stand, market sign as a standing board). About 150-600 triangles each, plus a store icon from the existing item-icon renderer. Batches: A = level 1 (the six types Milestone 14 needs), B = levels 2-3, C = levels 4-5, D = levels 6-8. Validation: 48 unique IDs, six per level, four per category; buff totals reach their caps; footprints within limits and "all owned types fit"; each mesh inside its footprint; a price and no recipe for every type.


**Confirmed store-only acquisition:** furniture has a store price/unlock, not an ingredient recipe; purchases never request materials. For example, bricks/glass panels contribute to a house upgrade; that level can unlock a clay cookstove or decorative window, which the player buys. Clay cups/glass bottles are recipe containers, not furniture. Chests/display shelves are bought too. Material drops cannot bypass the store through furniture crafting. Eight furniture-unlock levels do not imply eight resource tiers.

Furniture/store companions are Q5 products under the store rule, but this does not add crafting levels, wear, or a second multiplier to their defined first-purchase buffs. Keep their authored buff strength/capacity and store price; their quality label does not duplicate a permanent unlock. Offline pet rewards are earned output, not purchases, and still use their own reward definitions.

### 13.3 Chests and shelf layout

**Chest access (user, November 7, 2026):** chests are placed furniture inside the house. Touching one opens its own inventory **above** the player inventory so items move between them; every chest is a separate container, including duplicates bought as extra storage. Proposed sizes by chest type: Pine 6 x 6, Birch 12 x 6, Mesquite 24 x 6, Reinforced oak 48 x 6.

**Confirmed:** each chest is one continuous spatial container with **horizontal scrolling**, like the backpack; no independent pages, vertical scrolling, or item rotation. Use fixed-height storage with proposed tier sizes 6 x 6, 12 x 6, 24 x 6, and 48 x 6 logical units. The largest remains 288 logical square units, not 288 slots. Free placement can cross any viewport boundary because there are no page boundaries. Save container ID, item coordinates and horizontal scroll offset, not page ownership. Migrate old page layouts into consecutive horizontal strips without losing contents.

Render/virtualize only visible entries plus a small margin; clipping affects views, not item ownership. Use readable touch targets (about 48 dp), screen safe areas, and a transfer drawer or tabs when chest/backpack cannot fit side by side. Edge-drag autoscroll is a proposed convenience; scrolling on empty space and dragging an item have separate gesture ownership. Start with four placed chests and retain searchable offscreen entries.

The display shelf remains three rows of four positions, one ready item per position. Split one unit from a stack, preserve unique metadata, and scale the view without changing inventory footprint.

### 13.4 Settlement businesses

| Building | Mechanics to implement |
| --- | --- |
| Warehouse/trading house | Two ten-unit exact-ID boxes; every ready sellable item, including unique gear; sum actual deposited values; 100% cash, 1 gold per 1,000 cash every ten boxes; no extra bonuses |
| Lumber field | Two rows, six massive trees per wood tier; gold unlock per tier (proposal: pine 50, birch 300, mesquite 750, oak 1,500 gold; each requires its wood tier to be discovered); independent 72-hour respawn timestamps and persistent pending loot |
| Bakery | Three offers: one common cash food stack and two premium gold slots; each premium stack may become one recipe (section 9.4.4) |
| Ranch | Three full-stack cash offers: two dairy/egg offers and one ranch-exclusive meat (section 13.4.2); no wild hides/pelts |
| Farm | Three full-stack cash crop offers; region discovery controls availability (catalog herbs include Tea herbs after region 4 and Lavender after region 10) |
| Mine | Three full-stack cash ore/stone offers; avoid future-region crafting bypass |
| Fishery | Exactly two distinct discovered fish offers + one random eligible exclusive seafood offer; Q5, cash-only, each capped at one 1,000-unit stack per reset |
| Tailor-material shop | Three full-stack cash offers from active-tier hemp/cotton/leather and optional wool; no pending silk/cashmere/hunted-species dependency |

**Confirmed:** warehouse accepts **any ready, owned sellable item**, including materials, food, Misc, tools, clothing and backpacks; recipe knowledge, currencies, defaults, pending products and committed items are not sellable. Worn gear must be unequipped first. Keep two partial boxes, ten units/instances of one exact item ID per box. **Delegated default:** allow mixed quality/mastery/condition within that ID and sum each deposited unit's saved value; one tool counts as one, never as its remaining uses. Item IDs distinguish tier/variant. Proposed used-gear valuation: `cashUnitValue = savedIntrinsicValue * clamp(remainingDurability / maximumDurability, 0, 1)` for finite tools/wearables, full saved value for nondurables/indestructibles. Apply no furniture-sale or extra transaction bonus. Show each deposit's exact cash quote; lock its valuation once deposited. No partial-box payout. Remove ownership into escrow, replace completed boxes, pay cash and advance the ten-box gold cycle exactly once. Begin with one box/action, then bounded bulk deposit; a full test stack can fill 100 boxes without 100 simultaneous animations.

**Confirmed settlement refresh (replaces the 30-minute proposal):** generate stock on settlement entry and refresh all ordinary resource/ingredient shop offers whenever the player actually **leaves the settlement and returns**. No timer, cooldown or daily cap. A new persisted `SettlementVisitId` commits fresh offer IDs/seed and full per-offer stock budgets once on re-entry; the player can immediately buy materials instead of gathering. Moving between home, shops, warehouse or other settlement subviews does not leave settlement. Reopening a resource shop within the same visit keeps its stock/purchases; app suspension/reload or canceled travel also keeps that visit. Show stock and cash/gold prices, no countdown. Pet ownership catalog remains permanent. Balloon shop sessions use their separate close/reopen rule in section 14.

**Surface detail (user, November 2, 2026; 22:09 build installed for device testing):** plain solid colours read as modern architecture, trees as coloured pillars and rabbits as painted blobs, so surfaces now carry texture.
- **How it works:**
  - New shader `TheTrailWalker/FacetedDetail` (URP main light with shadows, ambient light, fog).
  - Each palette colour stores a detail type in its alpha channel.
  - The shader projects the matching grayscale tile from `Assets/_Game/Art/Shared/Detail/DetailAtlas.png` onto each face in object space (one tile per metre) and multiplies it over the colour. Meshes and UVs are unchanged, and colours without a type stay flat.
- **The 15 detail tiles:** plaster with cracks, stone blocks, wood grain, bark furrows, shingles, brick, fur, cloth weave, thatch/straw, needles/leaves, soil, leather, grass, planks.
- **Source:** `Tools/Art/detail_textures.py` builds the atlas and holds the palette slot mapping. The palette writers (`generate_region01_meshes.palette` and the kit `Palette`) keep the types when generators are rerun.
- **Applying it:** **The Trail Walker > Art > Apply Surface Detail** switches every material that uses a mapped palette. The region builder no longer resets the shader on a rebuild.
- **Applied to:**
  - trail trees (bark) and rabbits (fur)
  - camp log seats, campfire and camp NPC clothing (cloth weave and leather)
  - the mission lady and the ferryman
  - the tier-one environment kits (bark, grass, needles, stone, soil)
  - the storefronts (plaster, stone, timber, planks, shingles, brick)
  - the shopkeepers
- **Trail pine trunk, revision 2:** flared root buttresses, an irregular outline shared across the chop joint, broken branch stubs and knots, and vertical bark furrows instead of a striped patchwork. The chopping rig is unchanged.
- **Device checks:**
  - bark and roots on trail trees, fur on rabbits
  - camp seats and NPC clothes
  - environment trees, grass and rocks
  - the frame rate while walking (the shader does three texture lookups per pixel)

**River ferry (user, November 2, 2026):** the ferryman now has a ferry. The river crossing's placeholder boxes are replaced by a cable ferry:
- `FerryDock`: a plank pier on pilings with mooring posts, rope, a lantern and a ferry-call bell.
- `FerryRaft`: a log raft with a plank deck, rails, a crate and rope coil, and two pulley guide posts riding the cable.
- `FerryCable`: bank posts with a sagging rope across the river.

All three use surface detail and appear on both the departure bank and the arrival landing (`TravelScenery.BuildRiver`; the primitives remain as fallback). Source `Tools/Art/generate_ferry_meshes.py`, menu **Build Ferry**. Art only: the crossing itself is not animated yet.

**Camp ground fix (user, November 2, 2026: Quiet Hilltop looked slightly above the ground):**
- **Cause:** the terrain follows the height of the nearest road point, and the scene builds one terrain over three joined legs, so the existing end-of-path flattening never reached the camps. On hill legs the ground under a clearing sat up to 0.5 m below it.
- **Fix:** a disc around each clearing centre (full within 6.5 m, fading out by 9.5 m, kept clear of the road's first 1.6-2.8 m) is now levelled 0.1 m below the camp's height. The clearing has a 0.3 m base, so any small edge gap is hidden.
- **New Milestone 5 check:** samples the ground under every camp clearing at both ends of every segment, on either side. Before the fix it flagged ten Windward Hills camps; now every camp passes, and the road clearance check still passes.

**Hill legs fix (user, November 2, 2026; 23:02 build):** the road and some interactables looked raised above the ground beyond Quiet Hilltop and further on, and the road dipped into a camp and climbed out again in a V. **The Trail Walker > Validation > Terrain Report** (`Builds/Validation/terrain-report.txt`) measured every leg before and after.
- **Causes:**
  - Each leg ended wherever its hill curve happened to be, so camps sat in dips or on slopes. For example, Low Hilltop had a -33% grade arriving and +17% leaving.
  - The ground next to the road was sunk in proportion to the slope, so on steep legs the road stood up to 0.8 m above the ground beside it.
- **Fixes:**
  - The hill curve eases to zero over the first and last two knots, so every camp has a level approach and departure and every leg returns to its camp height.
  - The road clearance follows how sharply the road's height profile bends (the sag of a grid triangle over a 3 m span) plus a 6 cm gap, instead of the slope. On a straight climb the grid follows the road exactly.
  - Hill height is 85% of the relief, so the steepest stretches are about 40% instead of almost 50%. Route costs still use the catalog relief.
  - Interactables sit on the lowest ground under their footprint.
- **Result:** the road now sits about 0.1 m above the ground on average on hill legs (0.24 m at worst; it was about 0.2 on average and 0.8 at worst). Every camp grade is within 3%.
- **New Milestone 5 checks, on every leg of every region:** the ground never covers the road, the road is at most 0.25 m above the ground at its edges, and the ends are level.

**Map region list (user, November 2, 2026):** the sideways page arrows are gone.
- Tapping the region name opens a list of all regions. Each row is in its region's colour (from its environment kit: meadow, olive heather, misty forest, fern, grey-blue stone, reedy teal), with the region's map sheet as a thumbnail and its "camps reached" count. Undiscovered regions show as "Unknown lands".
- Tapping a discovered row opens its camps. A right-arrow button (new `travel` HUD icon) on each row travels to that region's first camp once it has been reached; in the current region it returns to the first camp.
- Tapping the title again returns to the camp page.

**Plan: surface detail for gathering and craftable objects (user request, November 2, 2026; not started):** the same shader, extended to the items players pick up, carry and craft.
1. **Map item palettes to detail types:**
   - loose region items (`Region01`-`Region06` palettes): branches bark, pelts fur, hemp, nettle and reeds fibre, flint and limestone stone, clay and sand soil, fruit and berries skin
   - harvest drops in `Tier01Palette`: logs bark and wood, pelts fur, meat, fish
   - crafted items in `Tier01CraftedPalette`: tool handles wood, blades metal, clothes cloth, boots and packs leather, food
   - balloon goods and exclusives
   - mystery boxes: lacquer and brass
2. **Grow the atlas to 32 tiles.** The alpha encoding changes from type x 16 to type x 8, so the shader and all palettes are re-encoded. New tiles: fish scales, plant fibre and strands, seeds and grain, brushed metal, ceramic and glazed clay, paper, stitched leather, fruit skin with pores, ash and coal, glass. Earlier types keep their meaning.
3. **Detail scale per material:** small objects need finer texture, so item materials use about 4-6 tiles per metre instead of 1. The same mesh in the bag, the slots, the equipment panel and the world then shows the same texture.
4. **Fix weak silhouettes while texturing**, so the texture sits on good shapes: smooth apples and berries, the rabbit pelt and meat cuts, plain log ends. Characters and rabbits keep their smoothing where it was chosen.
5. **Review sheets per palette group**, and a device check of the open bag and crafting book with many textured items on screen.
6. **Order:** trail pickups and harvest drops first (seen most), then crafted tools and gear (bag, equipment, crafting book), then foods, then balloon goods and mystery boxes.

**Storefront shopkeepers (user, November 2, 2026; art built, storefronts not designed yet):** every storefront gets a **static NPC shown from the waist up behind its counter** (no legs are modelled), dressed for its shop, in the faceted character style. Seven keepers are built, as art only (no shop logic, dialogue or animation): **Bakery:** Baker (female), flour-white toque, bib apron over a cream shirt, a flour smudge. **Ranch:** Rancher (male), wide-brim tan hat, denim shirt, open leather vest, red neckerchief, gray beard. **Farm:** Farmer (female), straw hat with a green band, olive blouse, dungarees with a bib and brass buttons. **Mine:** Miner (male), leather helmet with a brass lamp, dark work jacket with leather shoulder pads, red kerchief, soot marks, black beard. **Fishery:** Fisher (male), navy knit beanie, cream roll-neck cable sweater, yellow oilskin bib, gray beard. **Tailor-material shop:** Tailor (female), plum waistcoat over a white shirt, measuring tape around the neck, wrist pincushion, spectacles. **Warehouse/trading house:** Trader (male), green eyeshade, white shirt with red sleeve garters, brown waistcoat with a brass watch chain, bow tie, pencil behind the ear. The lumber field is a field, not a counter, so it has no keeper for now (a forester could be added if it gets a hut). **Balloon merchant (user, November 2, 2026):** built the same way, as a waist-up keeper behind a counter. **Look:** a crimson-lacquer double-breasted coat with brass buttons, gold edging and epaulettes, a cream silk scarf and a brass balloon emblem pin (the merchant signature from section 14.1); a leather aviator cap with ear flaps and brass goggles pushed up; a handlebar moustache. Prefab: `ShopkeeperBalloonMerchant`. Source: `Tools/Art/generate_shopkeeper_meshes.py`. Prefabs: `Resources/Characters/Shopkeepers/Shopkeeper<Name>.prefab`, each with `TorsoPivot` (0.94 m) and `HeadPivot` (+0.49) for a later idle. Build with **The Trail Walker > Art > Build Shopkeepers / Capture Shopkeepers**; sheets are in `Docs/Art/Characters/Shopkeepers`. **Design notes for when the storefronts are designed:** (1) **counter height about 1.02 m**, with the keeper about 0.3-0.45 m behind its front edge; the posed hands rest on the top at about 1.03 m, so the counter must hide everything below the waist (the torso mesh stops at about 0.86 m). (2) Frame the shop view at the keeper's chest-to-hat height; the hats are the strongest theme signal. (3) Each storefront should repeat its keeper's palette and props: bakery bread racks, flour sacks and warm whites; ranch hay, fences, a cow bell and leather; farm crates, straw and green produce; mine timber props, lanterns, ore carts and dark stone; fishery nets, buoys, an ice tray and navy/yellow; tailor fabric bolts, a mannequin and spools in plum; warehouse ledgers, scales, numbered boxes and a brass till. (4) The keeper stays one fixed person per shop (not rolled like camp traders), so players recognise each shop. (5) A subtle idle (breathing, head turns) and a greeting nod when a purchase completes would be enough animation; hands stay on the counter. (6) Offer slots, stock and prices sit on the counter or a board beside the keeper, never covering the face. 

**Storefront design (proposal, November 2, 2026; bakery prototype built for review):** each shop has a **street front** (the facade in the side-scrolling settlement, between the two groups of three house plots) and a **counter view** (after the player taps the shop: the keeper behind a 1.02 m counter, the three offers on the counter, themed shelves behind).
- **Shared kit** so the street reads as one village: timber frames, stone footings, one window/door/shutter set, gable roofs facing the street, and a hanging **picture sign with no text** turned to face the camera. Each shop has its own awning colour. Fronts are about 6-7 m wide.
- **Bakery (built):** stone-and-timber two-storey house with a jettied upper floor, green shutters, red tile roof, brick chimney with smoke, red-and-cream striped awning over an open display window of bread racks, a wheat-sheaf sign, a bench with bread baskets and flour sacks. **Counter view:** loaf shelves on the back wall, a brick oven with a glow, flour sacks and a peel. The common cash offer sits in a bread basket; the two premium gold offers sit under brass-based glass domes.
- **All seven built (user, November 2, 2026: "focus first on the store fronts"):** street fronts and counter views for the warehouse, bakery, ranch, farm, fishery, mine and tailor-material shop, each with its keeper, three offer anchors and surface detail. The six shops after the bakery are in `Tools/Art/storefront_shops.py` (run `generate_storefront_meshes.py`). `settlement-street.png` shows the whole street in the proposed order. Offer anchors: the warehouse uses its two deposit boxes and the gold scale; the fishery uses two ice trays for the ordinary fish slots and a lobster trap for the exclusive seafood slot. The rock face uses the mottled natural-rock texture, not cut stone.
- **As built (the original proposal, realised):**
  - **Ranch:** red barn front with a two-slope roof, fence, hay bales, milk churns and a cowbell sign. Counter view: hanging cured meat, cheese wheels, churns.
  - **Farm:** open market stall with a thatched roof, tilted produce crates, green awning, a scarecrow and a sprout sign. Counter view: crates of produce and herb bundles.
  - **Mine:** timber-framed entrance cut into a rock face, an ore cart on rails, lanterns and a pickaxe sign. Counter view: ore bins, a balance scale, lantern-lit stone.
  - **Fishery:** dock shack on stilts with nets and buoys in navy and yellow, and a fish sign. Counter view: an ice-tray counter, with a lobster-trap display for the exclusive seafood offer.
  - **Tailor-material shop:** narrow, tall plum-trimmed house with fabric bolts and a mannequin in the window, and a spool sign. Counter view: fabric bolts on the wall and a measuring rule on the counter.
  - **Warehouse/trading house:** the largest front, with double barn doors, a loading crane, stacked crates and a brass-scale sign. Counter view: a ledger counter with the two deposit boxes shown as crates, and a gold scale.
  - **Lumber field (user, November 2, 2026):** like the storefronts it has an **entrance on the street** and its **own area inside**, with these differences:
    - **Entrance:** a gate in a timber fence with a forester's sign (axe and log), seen from the street at the end of the shop row.
    - **Area:** a separate scene or view entered through the gate, not a counter. It holds the **two rows of six massive trees per wood tier**, one section per tier (pine, birch, mesquite, oak).
    - **Locked tiers:** their sections stay fenced off, with a gold-unlock sign.
    - **Respawning trees:** a tree on its 72-hour respawn shows its large cut stump and a regrowth marker.
    - **Chopping:** tapping a tree opens the existing focused chopping screen; massive trees keep the screen open after the last swing for pickup, and only Return closes it. A felled tree with drops left can be tapped again to reopen the pickup screen (for a bag that filled up; section 8.2.1, user, November 5, 2026).
    - **No keeper or offers.** *Proposal:* a forester standing by the gate gives the one-time introduction dialog (tutorial, settlement step) and could later host the unlock purchases.
    - **Upgrades:** the area grows by tier through its unlocked sections, not through the shop tier looks.
    - **Art:** the new massive trees (recommendation in section 13.4) and the faceted ground kit; the gate uses the storefront kit.
- **Assets:**
  - source `Tools/Art/generate_storefront_meshes.py`; prefabs `Resources/Settlement/Storefronts/<Shop>Front` and `<Shop>Counter`
  - the counter prefab includes the keeper, `OfferAnchor1-3` and a `CustomerCameraAnchor`; the front has a `SelectAnchor`
  - menus **The Trail Walker > Art > Build Storefronts / Capture Storefronts**; review sheets in `Docs/Art/Settlement/Storefronts`
- **Street order (delegated to Claude, user, November 2, 2026; confirmed):** between the three left and three right house plots, the street runs **warehouse, bakery, ranch, farm, fishery, mine, tailor-material shop, lumber field**. Reasoning: the most-used building (the warehouse, for selling) comes first; then the food shops, ordered from everyday bakery goods to fresh produce and fish; then the material shops (stone and ore, then cloth); and the wide lumber field at the end of the street, where its rows of massive trees have room. `settlement-street.png` shows this order.
- **Shop upgrades by tier (user, November 2, 2026):** shops upgrade when the player reaches the **next tier's regions**: tier 2 from region 7, tier 3 from region 13 and tier 4 from region 19. Mostly visual for now (other mechanics only if the user thinks of some).
  - *Proposal:* four looks per shop, each in its tier's materials, matching the wood tiers: pine, then birch, mesquite and oak timbers and trims, with sturdier roofs, more stock on display and richer signs at each tier.
  - Each counter view upgrades with its front, and the keeper stays the same person.
  - The upgrade happens once, the first time the player reaches a region of that tier, and is saved.
  - Houses keep their own 8-level rule, separate from this.

**Lumber field trees (recommendation, November 2, 2026):** make **new massive-tree models** rather than only scaling the trail trunk. The current `PineTrunkMassive` placeholder is the trail chop trunk at 2.5x (12 sides, 192 triangles), and at that size its facets look coarse and it does not read as ancient. A massive tree needs:
- buttress roots, deep bark ridges, moss and knots, and a canopy or broken crown;
- all four woods (pine, birch, mesquite, oak); only pine exists now;
- a large cut stump for the 72-hour regrowth;
- two or three trunk shapes per species, plus small lean and rotation, so the two rows of six do not look cloned.

Keep from the existing art:
- the chopping rig points (lower trunk, fall pivot, cut area, axe hint), so the current chopping interaction drives them unchanged;
- each species' bark colours, the faceted style, and the environment library's grass, shrubs and boulders around the bases.

**All six settlement shops deliver Q5:** bakery, ranch, farm, mine, fishery, and tailor materials. Ordinary cash goods and premium gold goods follow the same rule. Item identity/quantity/price may vary by offer, but quality is fixed at five and is persisted through purchase, overflow delivery, and reload. Store-bought Q5 ingredients still cannot replace another material variety in a recipe.

Lumber field scope remains four tiers x six trees = 24 records, pine through oak. Render visible rows only. Section 8.2 defines separate Normal/Elite action curves: Elite is faster at/below its tier and boosts drops on every eligible tree including +1/+2. Apply x50 rewards once and retain 72-hour independent respawn/expiry. Pending species stay deferred; UTC/local-clock limitations remain.

**Lumber unlock reasoning (proposal, October 31, 2026; revised the same day):** after the post-debug rebalance a massive tree yields **about 450 items** (the current 450,000 comes from the 1,000-unit test bundles). That is roughly 450-4,500 cash of materials per tree, and the six trees of a tier respawn every 72 hours, so the lumber field is a steady wood supply for crafting and building rather than a money source. The unlocks are priced as early settlement conveniences: pine 50 gold (less than a tier-one special hat), birch 300, mesquite 750 and oak 1,500 gold.

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

### 13.4.2 Ranch products and premium ranch recipes (proposal, user request November 5, 2026)

The ranch sells **domestic** products that no trail source gives. Like the fishery's exclusive seafood (section 13.4.1), each tier has one **ranch-exclusive meat**: a raw ingredient with source `Store` only.
- **Where it never comes from:** loose gathering, hunting or other interactables, pet rewards, mystery-box physical rewards, and procedural NPC supply. Players may still trade what they bought.
- **What uses it:** only optional **premium mystery-box recipes** (below), never mandatory progression.
- **No mission:** the ranch can show a meat before its recipe is known, and that is not a reason to add a mission for it (section 9.4.2).
- **Not hunted animals:** wild rabbit, beaver, turkey and boar stay hunted only, and the ranch never sells their meat or hides.

| Exact ingredient (ID) | Material tier | Ranch stock eligibility | Proposed Q5 cash / unit |
| --- | --- | --- | --- |
| Chicken meat (`ranch.chicken_meat`) | 1 | Settlement and ranch unlocked (after region four) | 50 |
| Duck meat (`ranch.duck_meat`) | 2 | A tier-two region discovered | 100 |
| Lamb meat (`ranch.lamb_meat`) | 3 | A tier-three region discovered | 180 |
| Beef (`ranch.beef`) | 4 | A tier-four region discovered | 300 |

The existing domestic products stay: Cow milk, Goat milk and Egg (pantry ingredients). The optional Raw wool chain stays deferred. **Ranch stock:** three Q5 cash offers per settlement visit, each one full 1,000-unit stack:
- **two dairy/egg offers** (two distinct items from Cow milk, Goat milk and Egg)
- **one meat offer** (one eligible ranch meat; older tiers stay eligible)

The usual rules apply: the refresh on leaving and returning, partial purchases at the unit price, and persisted offers and purchases. Prices are Q5 quotes, not multiplied by quality again, and get no bakery stack discount. Store Q5 ingredients do not pass their grade into the crafted dish.

**Premium ranch recipes (mystery-box exclusive, eight recipes, two per tier).** They follow section 9.4.2's premium hearts by tier (tier one 8-10, two 9-10, three 10-11, four 11-12) and the premium rule of section 9.6.6: at least three distinct ingredients, one refined or from another source. Wood fuel is listed where the dish is roasted.

| Recipe ID / output | Tier | Exact ingredients -> yield | Hearts per serving | Other sources needed |
| --- | --- | --- | --- | --- |
| `recipe.food.chicken_mushroom_pie` — Chicken mushroom pie | 1 | 1 Chicken meat + 2 Mushrooms + 1 Wheat flour -> 1 | 10 | Mushrooms 4, Wheat flour (Wheat 6 + conversion) |
| `recipe.food.apple_glazed_chicken` — Apple-glazed chicken | 1 | 1 Chicken meat + 2 Apples + 1 Pine branch -> 1 | 9 | Apples 1 |
| `recipe.food.honey_roast_duck` — Honey-roast duck | 2 | 1 Duck meat + 1 Honey + 2 Birch branches -> 1 | 10 | Honey 11 |
| `recipe.food.duck_blueberry_rice` — Duck blueberry rice | 2 | 1 Duck meat + 1 Blueberries + 1 Rice -> 1 | 9 | Blueberries 9, Rice 11 |
| `recipe.food.mint_lamb_stew` — Mint lamb stew | 3 | 2 Lamb meat + 1 Wild mint + 2 Chickpeas -> 1 | 11 | Wild mint 18, Chickpeas 15 |
| `recipe.food.lamb_chili_flatbread` — Lamb chili flatbread | 3 | 1 Lamb meat + 1 Corn flatbread + 1 Green chili -> 1 | 10 | Corn flatbread recipe (Corn 13), Green chili 17 |
| `recipe.food.coffee_braised_beef` — Coffee-braised beef | 4 | 1 Beef + 1 Roasted coffee + 2 Potatoes -> 1 | 12 | Roasted coffee (Coffee beans 21), Potatoes 19 |
| `recipe.food.beef_cacao_chili` — Beef cacao chili | 4 | 1 Beef + 1 Cacao powder + 1 Green chili -> 1 | 11 | Cacao powder (Cacao beans 20), Green chili 17 |

**Eligibility** (same as the other box recipes, section 14):
- the recipe's tier is discovered
- the ranch is reachable and stocks that meat tier
- every other named source is discovered, with any needed conversion known or an exact shop reachable
- no box offers the recipe before then

Recipe-book entries show **Source: Balloon mystery box** and list the ranch as the meat's source. **Box pool effect:** these eight recipes join the collectible pool: 26 + 18 barrel + 8 ranch = **52** in Part 1 (Part 2 section 10.2 adds the tier 5-8 ranch recipes). The 5% per-slot recipe chance is unchanged.

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

**Pet home upgrades (proposal, user request November 5, 2026):** the pet home has its own levels, and **each level admits one more active pet**. Every active pet keeps its own roll timer, its own weights and its own 16-entry collection container. Output per pet, the 30-minute roll, the eight-hour cap and the 70/30 tier roll are unchanged, so more pets means more pets working, not stronger pets. An admission slot holds one owned pet. Switching follows the existing per-pet settle-then-switch rule, and an empty slot earns nothing. Upgrades are material deposits with the house rules (escrow, cancellation through the tray, atomic completion), at **half the house amount of the same tier** (rounded to 5), using three types: planks for the pen, cloth for bedding, fittings for gates.

| Pet home level | Active pets | Requires | Deposit |
| --- | --- | --- | --- |
| 1 | 1 | House level 3 (pets unlock) | none |
| 2 | 2 | House level 4 | 25 Birch planks + 25 Flax cloth + 25 Copper fittings |
| 3 | 3 | House level 6 | 40 Mesquite planks + 40 Cotton cloth + 40 Iron fittings |
| 4 | 4 | House level 8 | 50 Oak planks + 50 Fine cotton cloth + 50 Steel fittings |

With six pets in the roster, four can work at once by the end of tier four; [Part 2](PROJECT_RECOMENDATIONS_PART_2.md) section 16.5 continues to eight. The pet home's look grows with its level (kennel, fenced run, pen with shelter, small barn). Its art is part of the house set.

The pet store uses a permanent owned/unowned catalog, not random three-slot stock or the settlement resource-shop visit refresh. Purchase and owned-ID registration are atomic. Pets occupy no backpack space, are selected by ID rather than equipped as an item, and are not warehouse goods. An inactive/unowned pet produces nothing.

**Roll order:** snapshot pet ID/weights and discovered-source IDs at the start of each offline interval. For each completed opportunity remove empty/undiscovered groups and renormalize the remaining group weights, then choose within the group: proposed 70% highest discovered eligible resource tier/30% earlier eligible tiers, renormalizing if either tier pool is empty; equal weights across exact IDs inside the selected tier is the initial default. Never multiply group weight by the number of species/items. If no group is eligible, produce no reward and bank no future reroll. Before region 10 Pig cannot find Truffle and its other weights renormalize; discovery never relabels old rewards. Show effective as well as authored percentages so a region-7 pig's 0% effective Truffle chance is clear. Choosing a pet never unlocks a region/material/recipe, and no mission or recipe requires owning it.

**Idle model:** only closed/suspended-app time earns opportunities; online play and menus do not. Starting values: one roll per **30 minutes offline**, at most **eight hours per absence** = 16 rolls, **one material per roll in Release**, with an explicit 1,000-per-roll MechanicsTest preset for container limits. The collection container holds **16 reward entries**, each retaining generated quantity/quality/seed. Do not merge generated entries to evade this cap; claim transfers may merge compatible inventory stacks. Source quality tables determine drops, not pet/shop Q5 or player furniture. These quantities are independent of trail pickups and store/box full stacks.

Persist each opportunity's result once before display. An unclaimed entry remains until collected, with no expiry/replacement. Stop generating when all 16 entries are occupied; missed time/rolls are not banked for immediate claims afterward. Display capacity, contents and capped earnings before closing. Claim individually or Claim All up to available bag space; preserve exact untransferred remainders and allow mission consumption only after a ready material reaches carried inventory. The container is a reward queue, not a general deposit chest.

**Time/switching:** settle the previous offline interval before a purchase, selection, region-source change or claim. The pet home switches only the next opportunity's pet: any saved fractional opportunity retains its original pet/source snapshot until completed; later opportunities use the new choice. Preserve sub-30-minute carry across short absences, but discard time beyond the eight-hour/space cap. With no active pet there is no accrual or catch-up. On resume use `max(0, nowUtc - offlineStartedUtc)`, cap once, and atomically commit generated entry IDs/seeds, partial interval, consumed interval ID and a monotonic last-seen timestamp. Backward clock movement gives zero progress and cannot move the watermark backward; repeated resume cannot replay an interval. Keep unclaimed results through pet changes and reload. Local-clock manipulation remains an offline limitation; use an injected clock for verification.

**Save/acceptance requirements:** save owned pet IDs/purchases, active ID, pet definitions/weight revision, offline interval and source snapshots, fractional progress, per-opportunity transaction/result, container entries/claim remainders and cap discard state. Validate six rows each totaling 100%, distinct specialties, Dog 85% building materials, Pig Truffle gated at region 10, exact source filters, no pet-only mandatory recipes, no quality/furniture leak, bag-full partial claims, full-container no reroll, switch at 29/31 minutes, eight-hour cap, clock reversal and reload after generation/claim.

## 14. Balloon merchant and mystery boxes

**Balloon frequency (confirmed October 31, 2026):** the merchant appears at **one camp in every 4-6 camps** along the route (authored per region, deterministic, never two hosts in a row); during the current testing phase it stands at every camp (placement test). At its host camps, show exactly three gold offers: one indestructible clothing item, one mystery box, and one backpack. Define explicitly whether the merchant backpack is indestructible; recommendation: yes, as special equipment at that rank. Do not infer that every gold-purchased object is indestructible.

**Balloon shop interior (user, November 2, 2026):** the balloon allows interaction: the player **enters the shop and sees the counter**, with the balloon merchant shopkeeper standing behind it, waist-up like the settlement storefronts (art: `ShopkeeperBalloonMerchant`; design notes in section 13.4). The interior is a separate view, not the exterior cabin. The exterior door opening is only about 1.3 m tall, so the merchant is never shown standing in it; entering is a transition into the interior. The interior and its counter follow the same notes as the storefronts (counter top about 1.02 m; offers on the counter or a board, never over the merchant's face), dressed in crimson lacquer, brass trim and warm paper lanterns to match the cabin.

**Mystery box presentation (confirmed October 31, 2026; art in progress):** the box has an **open and a close animation**, and its **colour set follows the tier of the region where the balloon merchant is found** (one colour set for camps in tier-one regions, another for tier two, and so on). The colour is presentation only; contents follow the generation rules below. The special gear meshes (section 14.1) are being made alongside it. Both wait for the shop interaction and logic (Milestone 14).

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

- Initial collectible pool: the twenty-six optional Food/Misc recipe IDs in sections 9.4.2-9.4.3, including seafood recipes, plus the eighteen oak-barrel filling/bottling recipes in section 9.4.5 the eight premium ranch recipes in section 13.4.2 and the seven precious-metal and gem keepsakes in section 9.5.1 (59 total; proposals October 31 and November 5, 2026). Do not silently move the mandatory apple juice/tool/clothing missions into this random pool.
- Require discovered `RecipeUnlockTier` and reachable ingredient sources, including an unlocked fishery/catalog tier for seafood. A refined ingredient can be supplied through an already-known recipe or an eligible discovered shop catalog; it need not be in the bag at purchase. No mandatory future-region ingredient or inaccessible crafting chain.
- Exclude already-known recipes and recipes reserved in other unclaimed purchased boxes. Reserve newly selected IDs within the current box too, so two slots cannot award the same recipe.
- If no eligible unlearned recipe remains, convert that slot to an eligible physical material/food reward. Do not invent a useless duplicate, refund loop, or 1,000 scrolls.
- If a selected recipe becomes known before claim through another authorized source, use its **persisted pre-rolled fallback**. Do not grant mastery, choose a fresh random recipe, or silently discard a paid reward.
- Claiming a new recipe grants knowledge once at mastery level one with zero crafted count. Subsequent crafts use the normal grade/furniture/timer/value rules. Claim and known-ID update share a transaction ID so reload cannot repeat grants/fallbacks.

Physical duplicates may merge after claim if their item/stat profiles match; recipe duplicates are prevented as above. Mission contribution occurs when an eligible physical reward is claimed, never from recipe knowledge. A full bag does not block learning a recipe, while physical overflow stays in the persistent tray. A completed mission is not eligible for the mission preference. Raw seafood remains a fishery cash purchase, even when its recipe is obtained from a gold-bought box.

Adding a recipe does not automatically add its finished product to physical box loot. Keep separate item-source/recipe-unlock flags; initially these twenty-six additions are recipe rewards, not a backdoor source of free seafood dishes or sale goods before their recipes are learned. Any later finished-item reward must be deliberately authored under the normal source filters.

These probabilities are proposed starting values, not facts from the reference game. Gold is earned in-game in this plan. Real-money monetization is outside the current implementation scope.

### 14.1 Special balloon gear (proposal, October 31, 2026)

The balloon's two equipment offers are the **special gear**: one garment (hat, boots, shirt or pants) in the clothing slot and one backpack in the backpack slot. This answers the open question above: **the merchant backpack is special gear too (infinite durability)**. Special gear is noncraftable, has no recipe, never appears in mystery boxes, is always Q5, never wears out (the equipment badge shows the infinity marker) and stays within the global caps. Proposed values for review; prices assume the revised warehouse rate (1 gold per 1,000 cash) and will be rebalanced with everything else after the debug cleanup (Milestone 7 note).

**Offer eligibility:** each refresh rolls the garment's slot uniformly among the four, then its tier: the highest discovered tier 70%, earlier discovered tiers 30% (the same split as box rewards), with a valid fallback when one side is empty. The backpack offer rolls its tier the same way. Already-owned special items may be offered again (a second copy is a spare).

**Stat rule (confirmed by the user):** special gear trades a little power for never wearing out.

- Tier 1 special gear reaches the **tier-1 limit**.
- From tier 2, special gear reaches **at most halfway** from the previous tier's limit to its own tier's limit. Whole-number stats (hearts, pockets, hooks, backpack width) round **down**; boots speed keeps 0.05 m/s steps.
- The crafted **Set A** garments (section 9.6.1) use the same halfway point rounded **up**, so they match or slightly beat special gear but wear out. Set B reaches the tier limit.

| Special tier | Hat hearts | Boots speed, m/s | Shirt pockets | Pants hooks | Backpack (logical units) | Regular tier limit, for comparison |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 4 | 2.25 | 2 | 2 | 8 x 4 | 4 / 2.25 / 2 / 2 / 8 x 4 |
| 2 | 5 | 2.6 | 3 | 3 | 9 x 4 | 6 / 3.0 / 4 / 4 / 10 x 4 |
| 3 | 7 | 3.35 | 5 | 5 | 11 x 4 | 9 / 3.75 / 7 / 7 / 12 x 4 |
| 4 | 10 | 4.1 | 8 | 8 | 13 x 4 | 12 / 4.5 / 10 / 10 / 14 x 4 |

**Price reasoning (gold, whole numbers):** gold now comes from the warehouse at 1 gold per 1,000 cash after every ten boxes (100 units). One cycle of a tier's best product at recipe level 10/Q5 (section 12.2 references) yields about **120 gold at tier 1** (Misc, 1,200 cash each), **360 at tier 2**, **720 at tier 3** and **1,200 at tier 4**; clothing or food cycles yield far less, and raw-material cycles almost nothing. Special prices sit at about 1.5-3 top-product cycles of their own tier, so they are a deliberate saving goal rather than an impulse buy; boots (speed) and backpacks (area) cost more because they change travel and carrying the most.

**Resale (proposal):** special gear sells at camps and the warehouse for a fixed cash value equal to its tier's level-10 Q5 reference with no durability premium (clothing 440 / 1,320 / 2,640 / 4,400 cash; backpacks 700 / 2,100 / 4,200 / 7,000 cash for tiers 1-4), so buying and reselling can never turn gold back into meaningful gold.

**Visual identity:** every special item carries the balloon merchant's signature, **crimson lacquer accents with brass/gold trim and a small brass balloon emblem**, over its tier's landscape theme and materials, so it reads as merchant gear at a glance. Each item needs two meshes: the item display model (bag, slots, drag ghost) and a fitted wearable for the player avatar (equipment menu now; trail avatar later), built to the default avatar anchors and the clothing fitting pass. Hats must handle the four hair styles; backpacks clear the shoulder blades.

**Tier 1 — Wayfarer** (temperate plains, windy hills, misty woods, fern valleys, mountain passes, reed delta; felt, waxed canvas, oiled leather):

| ID | Item | Slot | Stat | Gold | Design brief |
| --- | --- | --- | --- | --- | --- |
| `special.t1.hat` | Wayfarer's felt hat | Hat | 4 hearts | 150 | Wide-brim olive felt hat, crimson band with brass balloon pin, one pheasant feather |
| `special.t1.boots` | Wayfarer's trail boots | Boots | 2.25 m/s | 200 | Ankle boots in oiled brown leather, crimson laces, brass eyelets and toe caps |
| `special.t1.shirt` | Wayfarer's waxed shirt | Shirt | 2 pockets | 150 | Oatmeal waxed-canvas shirt, two flap pockets with brass buttons, crimson collar piping |
| `special.t1.pants` | Wayfarer's field trousers | Pants | 2 hooks | 150 | Moss-green twill, reinforced knees, leather belt with two brass tool rings |
| `special.t1.pack` | Wayfarer's ridge pack | Backpack | 8 x 4 | 250 | Canvas rucksack with rolled blanket on top, crimson flap, brass buckles, small balloon charm |

**Tier 2 — Lakeshore** (marshes, warm riverbanks, basins, sunny uplands, linked lakes, snowy taiga; oilskin, waterproof leather, fur trim):

| ID | Item | Slot | Stat | Gold | Design brief |
| --- | --- | --- | --- | --- | --- |
| `special.t2.hat` | Heron-plume rain hat | Hat | 5 hearts | 500 | Sou'wester-style oilskin hat, crimson lining, brass rivets, grey heron plume |
| `special.t2.boots` | Marsh-strider boots | Boots | 2.6 m/s | 650 | Tall waterproof boots, fur cuff, crimson pull tabs, brass buckles |
| `special.t2.shirt` | Lakeshore oilskin shirt | Shirt | 3 pockets | 500 | Short oilskin overshirt in deep teal, three pockets, crimson toggles with brass tips |
| `special.t2.pants` | Boardwalk trousers | Pants | 3 hooks | 500 | Waxed slate trousers with waterproof shins, belt with three brass rings |
| `special.t2.pack` | Taiga fur-trimmed pack | Backpack | 9 x 4 | 800 | Frame pack with fur-trimmed top, crimson oilskin cover, brass fittings |

**Tier 3 — Sunward Nomad** (golden grasslands, red canyons, dunes, hidden oases, plateau, snowmelt highlands; linen wraps, sun protection, caravan leather):

| ID | Item | Slot | Stat | Gold | Design brief |
| --- | --- | --- | --- | --- | --- |
| `special.t3.hat` | Dune wrap | Hat | 7 hearts | 1,100 | Sand-linen head wrap with a light veil, crimson inner fold, brass clasp |
| `special.t3.boots` | Canyon sand boots | Boots | 3.35 m/s | 1,400 | Soft wrapped desert boots, ochre suede, crimson binding, brass ankle studs |
| `special.t3.shirt` | Nomad linen robe | Shirt | 5 pockets | 1,100 | Long-sleeved cream robe-shirt, crimson sash, five hidden pockets with brass toggles |
| `special.t3.pants` | Caravan wrap trousers | Pants | 5 hooks | 1,100 | Loose terracotta trousers, wrapped calves, belt with five brass rings |
| `special.t3.pack` | Caravan saddle pack | Backpack | 11 x 4 | 1,800 | Twin saddle-bag pack in tooled leather, crimson tassels, brass corner plates |

**Tier 4 — Tidewatch Mariner** (savanna, coastal cliffs, beaches, rainforest coast, estuary, coral shores; sailcloth, rope, shell and brass):

| ID | Item | Slot | Stat | Gold | Design brief |
| --- | --- | --- | --- | --- | --- |
| `special.t4.hat` | Coral-shell sun hat | Hat | 10 hearts | 2,000 | Wide woven straw hat, crimson band set with small shells, brass balloon pin |
| `special.t4.boots` | Tidewalker boots | Boots | 4.1 m/s | 2,600 | Reef boots in navy canvas and rubbered sole, crimson rope laces, brass eyelets |
| `special.t4.shirt` | Mariner's sailcloth shirt | Shirt | 8 pockets | 2,000 | White sailcloth shirt with navy trim, crimson neckerchief, eight brass-buttoned pockets |
| `special.t4.pants` | Reef-runner trousers | Pants | 8 hooks | 2,000 | Rolled navy sailcloth trousers, rope belt with eight brass rings |
| `special.t4.pack` | Estuary sea-chest pack | Backpack | 13 x 4 | 3,200 | Small wooden sea chest on shoulder straps, crimson lacquer lid, brass corners and lock |

**Bakery prices after the rate change (resolved October 31, 2026):** the premium gold prices stay as authored (5-10 gold per serving, 50-100 gold per recipe, 4,500-9,000 gold per discounted full stack). Players buy partial quantities at the serving price (section 9.4.4); the full-stack discount is for late-game players. **Ferry and lumber (resolved October 31, 2026):** the ferry toll table and its gold alternative (cash / 1,000) are in section 12.3; lumber-tier gold unlocks (50 / 300 / 750 / 1,500 gold, priced for about 450 items per massive tree after the rebalance) are in section 13.4.

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

**Additional implementation/validation requirements:** `StaminaService` owns a `FoodRecoveryQueue` and atomic consume/enqueue records, integrates net walking/recovery without frame-order waste, and exposes pending hearts to faint/departure/HUD logic. `TravelService` uses separate current-region camp and discovered-region destination models, with explicit selected-camp versus first-camp resolution. Validate all 33 optional collectible recipe IDs (26 balloon + 7 bakery), including each new dish's exact regional ingredient access rather than tier access alone. Bakery apple pie has a separate rare bakery recipe; all seven bakery unlocks remain outside the 26-recipe box pool (section 9.4.4).

**Section-20 implementation/acceptance additions:** persist creation provenance (player craft/NPC/shop/loot), creation level, immutable stat snapshots, pending-product owner/timer/footprint, horizontal chest coordinates/offset, warehouse per-instance escrow/value and integer cash remainder of the gold cycle, collection mode, and settlement visit/departure state plus balloon open-session revision. Migrate page containers and earlier warehouse conversion state explicitly without re-paying past rewards. Reject rotation requests and mission quality/durability filters. Verify the full tool table and Elite doubling with matching modifiers; NPC L1/random Q/zero buffs against player-crafted profiles; craft center-display/drop with menu open and timer still running; ready transition in bag/tray credits once; used gear accepted by missions and warehouse with different identity/valuation responsibilities; ten unique tools fill a box; mixed-profile exact-ID deposits conserve values; 10,000 cash pays 10 gold once; no extra warehouse bonuses; one-object release pickups and 1,000-unit test pickups conserve manifests. Replace obsolete page/quality-lock/exchange-denomination milestone checks with these requirements. Milestone 11 builds horizontal storage, milestone 12 sells every category under the 1-gold-per-1,000-cash cycle, milestone 9 persists NPC random profiles/discounts, and milestones 16-17 exercise the 1,000-unit MechanicsTest preset only; release-preset work remains deferred until explicitly requested.

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
7. Build a development APK immediately, before any large content production. AAB/store-signing setup and store delivery are outside the current scope and require an explicit future instruction; continue using local development APKs for testing.

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

**Recovery checkpoint — October 29, 2026: Milestone 0 device exit check passed.**

- Reviewed the existing template commits (`8a2c4da`, `5f0fd2f`); this plan and `Trail.md` are already tracked. The new milestone implementation and settings changes remain uncommitted.
- Unity `6000.4.7f1`; Android Development Build Profile; IL2CPP/ARM64; application ID `com.thetrailwalker.prototype`; Mobile quality, portrait safe area, 30 FPS cap and optimized frame pacing.
- Prototype: `Assets/_Game/Scenes/Prototypes/AndroidBaseline.unity`. Editor commands: **The Trail Walker > Milestone 0 > 1 - Prepare Android Baseline** and **2 - Build Development APK**.
- Development APK built successfully and installed on the connected nubia NX769J (Android 16, Adreno 750, Vulkan, 1116 x 2480). The user confirmed that tapping the UI button changes the cube's color.
- Device capture at `2026-10-30T03:10:32Z`: 299 frames over 10.005 seconds, **29.88 FPS**, **33.46 ms average frame interval**, 136,416,138 bytes Unity allocated memory, one tap recorded before capture completion. These are development-build measurements with profiler overhead; frame interval is not CPU/GPU execution time and this is not a thermal test.
- Local artifacts: `Builds/Android/TheTrailWalker-Milestone0.apk` and `Builds/Android/Baseline-2026-10-29/milestone0.json` / `milestone0.raw` (13,624,745 bytes). The raw capture was retrieved but has not yet been inspected in Unity Profiler. Build artifacts are ignored by Git; retain a separate copy if clearing `Builds`.
- APK SHA-256: `AE4A519D63D0C36D847846C4964FDB2F82F286CA11C12FA5CF9C49EDB8536BF9`.
- Next implementation milestone: Milestone 1. Preserve this working baseline for comparison.

### Milestone 1: Forward movement and gesture ownership

**RETEST FLAG — conditional integration smoke check; completed baseline retained.** The tree/recipe catalog changes do not alter path evaluation or speed rules. They add pickup/food interactions that share gesture ownership and stop/resume state, so rerun this focused check when the new item fixtures or related controls are integrated: collect a new tree ingredient while orbiting/walking; open its details; inspect a raw ingredient versus use a prepared dish; close inventory and verify only valid pre-open intent resumes. No gesture may also collect/use an item or change speed unintentionally. A harvest/faint/manual stop must survive popup/inventory closure. Real chopping completion is checked with Milestone 6, not invented as an existing Milestone 1 feature. Status: pending integration retest; no new test result is claimed.

**Retest status — October 30, 2026, 16:39 build:** tree-food fixtures are integrated (TREE FOOD debug button: raw tree ingredients and dishes on the trail; dishes, refined inputs, a pending dish and a partial raw stack in the bag). Automated coverage is unchanged for movement; the device pass (collect a tree ingredient while walking/orbiting, open raw versus prepared details, eat a dish, close the bag and confirm only valid intent resumes) is pending.

**Design review additions — October 30, 2026 (requirements, not new verification):** use the current first-person camera and the centre SPEED gesture area from the 13:17 revision in Milestone 4; the original avatar-only/left-strip instructions below are historical. Keep the system notification/safe-area exclusion and dominant-axis gesture ownership. Recheck speed/orbit/pickup ownership with the open bag, sideways panels and food popup; pressing Eat, dragging an item or dismissing a popup must not also change speed or collect a resource. Inventory pause/resume must retain legitimate intent while fainting, camp arrival, harvesting and exhaustion stops keep priority. Recheck caps after boots expire: starter 0.8 m/s, current maximum 2.0 m/s; changing camera yaw never changes forward path distance. These are regression checks for the latest controls, not permission to replace them.

Dependencies: 0.

1. Create Bootstrap and one curved 100-250 m path with a camp anchor at its end.
2. Implement distance-based path evaluation, requested speed, caps, and a follow camera with independent full 360-degree horizontal orbit.
3. Implement avatar-only vertical speed gestures, horizontal hold/swipe orbit, directional gesture ownership, and Editor controls; no joystick. Orbit never stops/steers the player.
4. Add inventory placeholder with pre-open-speed snapshot/resume, temporary pause reasons, persistent post-harvest manual-restart state, and app-suspension handling.
5. Add the automatic camp stop and a debug display of distance/speed/pause reasons.

**Exit check:** no backward/sideways movement; full camera orbit at rest/while walking leaves speed/path direction unchanged; horizontal and vertical gestures do not interfere; camera release keeps chosen yaw; inventory restores only its saved speed subject to caps/other stops; closing it during a post-harvest stop does not restart; camp arrival never overshoots.

**Implementation checkpoint — October 29, 2026: Milestone 1 exit check passed.**

- `Bootstrap.unity` creates one persistent session root and asynchronously loads `MovementTrail.unity`. The 158.53 m curved trail uses a sampled Catmull-Rom arc-length lookup, a path-facing avatar, markers, and an end-camp anchor. Open `Assets/_Game/Scenes/Bootstrap.unity` to test in the Editor.
- `MovementState` in the engine-independent Domain assembly owns distance, requested speed, the 0.8 m/s starter cap (maximum 1.6), nested inventory snapshot/resume, independent pause flags, and post-harvest intent. `TrailMotor` uses distance, never camera yaw. The prototype starts stopped; an upward avatar gesture starts walking.
- Enhanced Touch input assigns contacts to UI first, then locks avatar/world contacts to the dominant gesture axis. Vertical world gestures are blocked; separate fingers can own speed and orbit. Release/cancel, inventory, focus loss, and suspension clear ownership. Full camera yaw remains at release. Mouse drag and wheel-over-avatar provide Editor controls.
- The HUD shows distance, requested/actual speed, cap, yaw, pause reasons, and harvest stop. **INVENTORY** tests temporary stop/resume. **TEST HARVEST** runs a two-second placeholder interaction (tap again to cancel), then requires a fresh upward gesture. **CAP: 0.8 / 1.6** and **RESTART TRAIL** are prototype test controls. The backpack has no item logic yet.
- **The Trail Walker > Milestone 1** provides prepare, validate, and build commands. Preparation preserves existing scenes. Building runs all **34 movement/path/gesture checks** first. Checks live in `Assets/_Game/Editor/Milestone1Validation.cs`; report: `Builds/Validation/Milestone1-checks.txt`.
- Installed and tested on nubia NX769J: vertical speed control; orbit while moving/stopped; blocked world-vertical input; inventory restoration and orbit blocking; post-harvest inventory closure and upward restart; background/foreground recovery. The user confirmed both gestures respond well. A roughly 20-second background interval preserved distance exactly. Device camp arrival stopped at **158.529296875 m**, exactly equal to path length, with requested and actual speed zero.
- The first device build exposed runtime primitive collider stripping; `Assets/_Game/link.xml` retains those types, and the rebuilt APK passed the phone checks. APK: `Builds/Android/TheTrailWalker-Milestone1.apk`. Screenshots/JSON evidence: `Builds/Android/Milestone1-Verification/`. Build artifacts are ignored by Git. Development builds write a current `milestone1.json` diagnostic to the app's persistent data directory.
- Session intent survives temporary pauses, not process termination; durable save/load is Milestone 3. Real inventory, stamina consumption, harvesting, obstacle avoidance for future tall scenery, and final art remain for later work. All milestone changes remain uncommitted.

### Milestone 2: Item model, area capacity, and full-stack collection

**RETEST FLAG — required for new item definitions; completed baseline retained.** Why: four new active-tree raw IDs, two new refinements and their Food products extend the item catalog, and expansion fruits/sap can share tree and loose sources. Check new active IDs are recognized with correct category/readiness, stable identity, quantity, quality/profile and footprint. With fixtures, verify 1/999/1,000 quantities, exact partial re-pickup, full-bag failure without loss and separate stacks for incompatible profiles. Once dual-source expansion definitions are enabled, tree and loose stacks of the same canonical ID/profile must merge normally; provenance alone must not split them or create another item variety. Wrong sap/flour IDs must stay distinct. Raw ingredients are usable recipe inputs but not prepared food; recipe knowledge never becomes a bag stack. Status: pending catalog integration retest. Actual harvest-manifest quantities belong to Milestone 6 and gravity settling remains deferred.

**Retest status — October 30, 2026, 16:39 build:** catalog integrated as definitions only (four raw tree ingredients, Mesquite flour, Roasted acorn meal, four Food products at 2/3/4/5 hearts) with an explicit `ItemCategory`. Twelve new Milestone 2 checks pass (53 total): unique IDs, raw/refined/prepared classification, distinct raw/refined/cooked IDs, contradictory category rejection, 1-1,000 stack bounds, merge only for the same ID/quality/readiness, exact partial top-up remainder, raw and pending items not edible, one-serving consumption. Dual-source expansion IDs remain for Part 2. Device pass pending.

**Design review additions — October 30, 2026 (requirements, not new verification):** the fixed-position container below is the implemented prototype baseline; the gravity/collision replacement, central quick-drop, 20%-of-interior-height overpacking band and expiry spill belong to the dedicated deferred inventory work described in Milestone 4. Preserve a regression boundary between the two implementations. Test player-dropped entries separately from generated resources: a re-pickup preserves exact remaining quantity, quality, intrinsic profile and durability rather than granting another 1,000 units. Dropping/recollecting must transfer one owner atomically and never duplicate on interruption. Keep prepared-food versus raw-ingredient identity explicit; food recovery comes from the food definition/profile, not quality alone. Bakery recipe offers are one knowledge grant, not physical stacks or Q5 food: do not model them as 1,000 inventory scrolls. The catalog/payment integration stays in Milestone 14. Retain checks for 1/999/1,000 stacks and compatible/incompatible merges so new bakery food definitions inherit the existing rules.

**First-person revision checkpoint — October 30, 2026:** Milestone 0 remains the separate device baseline. Milestones 1 and 2 now use an eye-height first-person camera with independent 360-degree yaw. The dedicated left SPEED area owns vertical input even with a pickup behind it. Contacts starting in the top 8% or above the safe-area edge are not claimed by the gesture router. Collection uses the camera's visible range and line of sight instead of the old 8 m/15 m limits. Fixture spacing increased for testing without changing regional content or economy.

Both development APKs were rebuilt. The 34 movement and 35 inventory/gesture checks passed. On nubia NX769J, a Q3 stone stack beyond the old radius was collected at 1,000 units; SPEED changed movement without collecting the object behind it; and the user confirmed the notification shade and pickup below the edge both work. Evidence: `Builds/Android/FirstPerson-Verification/` (ignored by Git). Further tests across screen sizes/insets, overlapping or occluded targets, far-distance readability, and human reaction at maximum speed remain necessary. The older prototype checkpoint below is historical; its camera and distance limits are superseded.

Dependencies: 1.

1. Add Domain assembly and stable item IDs; author pine branch/log, stone, hemp, berries, and a starter tool.
2. Implement 1,000-cap stacks, five quality grades, unique equipment instances, and deterministic 2D container fit.
3. Add world bundles, hold-drag, downward quick swipe, backpack aperture, and pickup presentation. Retain nearby drops behind the avatar within a configurable rear collection window; target them through the rotated camera without pausing walking or bypassing range/occlusion.
4. Add stack growth, compatible touch merging, rejection feedback, and quantity badges.
5. Add debug commands for mixed shapes, quantities 1/999/1000, and a completely full bag.

**Exit check:** every trail stackable pickup starts at 1,000; no repeated-touch duplication; failed placement leaves it intact; stacks combine by item ID with a quality pile (section 5.2 revision; earlier builds kept qualities separate); tools cannot stack; resizing cannot overlap/escape the bag; camera orbit reveals nearby missed resources for collection without stopping the player or reaching beyond the permitted window.

**Implementation checkpoint — October 30, 2026: Milestone 2 prototype exit check passed.**

- Open `Assets/_Game/Scenes/CollectionBootstrap.unity`; it loads `CollectionTrail.unity`. Milestone 1's scenes/APK remain available. **The Trail Walker > Milestone 2** provides preparation, validation, and Android development-build commands.
- Domain now defines stable pine branch/log, stone, hemp, berries and Normal pine axe IDs; Q1-Q5; readiness/immutable functional profiles; unique equipment identities; and World/Backpack/Consumed ownership. World stackable fixtures start at 1,000; tools stay individual. Logs/tools are explicit test fixtures, not changes to regional loose-resource tables.
- The 6 x 4 backpack uses deterministic positions in thousandths of a logical unit, fixed orientation, no visible slots and no weight. Footprint/model growth caps at 1.5x. Quick storage tries compatible merging then deterministic free-space placement. Manual drag merges touching compatible stacks. Growth is validated before mutation; partial remainders stay at the source; rejected operations preserve ownership, quantity and position.
- Swipe a resource downward about 10% of screen height within 250 ms to quick-store, or hold at least 250 ms and drag into the bag/auto-store aperture. Open the backpack to pause and move contents; closing restores valid walking intent. Resource contacts retain ownership and cannot become camera/speed gestures. World collection itself does not pause walking.
- Nearby logical drops survive camera turns and visibility changes. Defaults: 15 m rear path window, 8 m physical pickup radius, camera/player line-of-sight checks, revalidated on transfer. Eighteen fixture bundle models use one model plus quantity/quality badge each, never one model per unit.
- **EMPTY**, **MIXED**, and **FULL BAG** deliberately replace test inventory and respawn fixtures near the current location. MIXED covers 1/999/1000, shapes, qualities, different functional values and a unique tool; FULL BAG fills the container. **RESTART** returns to the start with an empty bag. Debug tools/full-stack quantities remain enabled; no rebalancing or release/deployment work.
- Passed **30 inventory checks**, including **1,000 seeded randomized operations**, and all **34 Milestone 1 regression checks**. Reports: `Builds/Validation/Milestone2-checks.txt` and `Milestone1-checks.txt`. **12 recorded phone checks** on nubia NX769J cover quick/manual pickup, bag movement/rejection, 1+999 merge, profile separation, full-bag preservation, rear pickup while walking at 0.2 m/s, range rejection, occluded selection, quantity conservation and nonoverlapping bounds. The user confirmed quick swipes and hold/drag both work.
- APK: `Builds/Android/TheTrailWalker-Milestone2.apk`. Screenshots, JSON snapshots and `device-checks.json`: `Builds/Android/Milestone2-Verification/`. Build artifacts are ignored by Git. The device's `milestone2.json` is diagnostic output, not a restorable save.
- At the Milestone 2 checkpoint, inventory/drop state reset on process restart; Milestone 3 below now adds persistence. Splitting, missions, real harvesting, equipment slots and production content remain later work. Changes remain uncommitted.

### Milestone 3: Persistence foundation and recovery

**RETEST FLAG — required for catalog/save integration; completed baseline retained.** Why: save validation checks known definitions, so new ingredient/refinement/Food IDs and additional source provenance can expose missing-definition rejection, identity remapping or duplicated ownership. Save/reload each new active raw/refined/prepared item with partial quantities and distinct compatible/incompatible profiles; force-close after pickup/drop/re-pickup and recover a backup. Preserve exact IDs/quantities/owners, original profiles, and consumed-food delivery progress; never restore already spent or wasted delivery. A loaded ingredient or changed provenance must not grant recipe knowledge or regional discovery. When harvest manifests arrive in Milestone 6, extend this check across interrupted ingredient/wood claims; when expansion catalogs arrive, validate dual-source IDs and disabled-definition migration/quarantine. Discovery/recipe/shop transaction persistence remains owned by Milestones 5/7/14. Status: pending integration retest, not a failed historical checkpoint.

**Retest status — October 30, 2026, 16:39 build:** a new Milestone 4 persistence check saves and reloads every new raw/refined/prepared item with partial quantities, distinct qualities/profiles, a pending dish and dropped markers, restoring each exactly (23 persistence checks pass). No recipe or discovery systems exist yet, so loading items cannot grant them. Device force-close/backup pass with these items pending.

**Design review additions — October 30, 2026 (requirements, not new verification):** Milestone 4 extends this save foundation with atomic consumption/food-queue insertion, per-serving recovery snapshots, pending balances/order, fractional timing, equipped-item wear/cap changes, and the faint transaction/state. Preserve dropped-item provenance and exact quantities through reload; a reload never creates a new trail drop cycle. Add interruption/migration cases for these fields when their owning feature is implemented, retaining the original profile and avoiding repeated consumption/loss. Design extension points for stable recipe IDs, known recipe/mastery records and discovered source IDs: Milestone 5 owns discovery persistence and Milestone 14 owns bakery offer kind, recipe/food ID, remaining stock, quote, visit ID and payment/unlock transaction. Do not grant discovery or recipe knowledge by loading an item or a locked recipe preview. On later migration, missing discovery stays undiscovered and source requirements come from authored definitions; purchased food and gifted inputs cannot infer regional discovery. Future shop saves must preserve the stored offer and revalidate all source gates before payment, without rolling again on reload. These future features are integration obligations for their named milestones, not claims that they already exist in Milestone 3.

**First-person follow-up:** persist and restore look yaw and applicable configured reach/touch-safe-area settings alongside movement and world items; loading must not change reach or duplicate pickups.

Dependencies: 2.

1. Save inventory, nearby world bundles including behind-player drops, path position, requested speed, camera yaw, inventory-resume snapshot as applicable, post-harvest restart state, and player state.
2. Implement versioned DTOs, backup/recovery, stable instance IDs, and transaction records where needed.
3. Save on critical collection and app pause; restore without refreshing the current segment.
4. Add injected clock/random source and a developer Reset Profile action with confirmation.

**Exit check:** force-close after collection and reopen; quantities and placement survive; corrupted main snapshot can recover the backup; loading never duplicates world and inventory ownership.

**Implementation checkpoint — October 30, 2026: Milestone 3 prototype exit check passed.**

- The collection prototype now saves a versioned, checksummed session snapshot in `Application.persistentDataPath/PrototypeSession/session-v1.json`. It records trail distance, requested/capped speed, first-person yaw, inventory pause/resume snapshot, post-harvest stop, camp state, item identities/qualities/profiles/quantities/positions/owners, and the current world-drop positions. Consumed sources are omitted. Reopening restores the same cycle and does not generate fresh drops.
- Successful inventory transfers save immediately; foreground state changes are saved periodically when changed and on suspension/focus loss/quit. A flushed temporary file is atomically replaced into place with a previous-snapshot `.bak`. Loading validates checksum, version, known definitions, unique IDs, quantity, movement state and backpack fit before using data. A corrupted primary recovers the previous backup; the most recent changes after that backup may be lost. Newer or unrecoverable saves are preserved and block further writes until the user chooses the debug reset.
- **RESET SAVE** requires two taps within five seconds and archives existing save files before making a fresh test profile. Existing EMPTY/MIXED/FULL BAG buttons remain explicit debug fixtures and replace the current session. `IClock`/`IRandomSource` and a seeded random implementation provide injectable foundations; current fixture layout is deterministic and does not require random rolls.
- **The Trail Walker > Milestone 3** runs validation and builds `Builds/Android/TheTrailWalker-Milestone3.apk`. The 13 persistence checks and all 34 movement / 35 inventory regression checks passed. On nubia NX769J, seven recorded checks passed: a 1,000-unit pickup survived force-close, uncollected drops remained world-owned, open-inventory pause/position and yaw survived reload, a deliberately corrupted primary recovered its backup, a post-harvest stop survived reload and inventory close, and item identities stayed unique. Evidence: `Builds/Android/Milestone3-Verification/` (ignored by Git).
- This is the prototype's first schema, not a migration of `milestone2.json`, which remains diagnostics only. Equipment, missions, later scene transitions and other future systems must add their own save data and migrations when implemented. Changes remain uncommitted.

### Milestone 4: Equipment, pockets/hooks, and stamina

**RETEST FLAG — required for food classification and recovery; completed baseline retained.** Why: tree drops are raw cooking inputs and must not accidentally inherit prepared-food Eat/auto-eat behavior, while their crafted dishes use the existing stamina system. At zero stamina, test raw-only inventory (including nuts, sap, pods and acorns): no automatic consumption or faint prevention. With a ready prepared dish, consume exactly one serving automatically; pending flour/meal/dish crafts remain unavailable. Check manual Eat appears only where the definition permits it, one serving is removed, recipe knowledge is unaffected, and recovery continues through inventory close subject to stop priorities. Exercise 2/3/4/5-heart new active dishes plus a twelve-heart fixture, bulk-stack quantities, the 1/3-heart/active-second rate, suspension and reload, hat expiry and faint transactions.

**Current expected recovery result for this retest:** section 4.4.3's later no-reserve revision controls: delivery continues at the fixed rate, and delivery beyond the meter cap is lost rather than retained for later walking. At 3/3 while standing, a twelve-heart serving is progressively wasted over 36 active seconds; suspension freezes the remaining delivery. Below cap, hearts recover progressively; walking can absorb concurrent delivery. Multiple servings do not multiply the rate. Older assertions that capped delivery pauses or preserves a reserve are superseded and must not be used as pass criteria. Do not infer a new hat-expiry rule from this note; test cap changes against the current section 4.4.3 definition. Status: pending focused retest; actual bakery/recipe purchase checks remain Milestone 14 and real cooking acceptance remains Milestone 7.

**Retest status — October 30, 2026, 16:39 build:** Editor play test with the TREE FOOD fixture: at zero stamina exactly one ready dish serving was auto-eaten (1,000 to 999, two hearts delivering); with only raw/refined ingredients and a pending dish the walker fainted and nothing was eaten (the faint penalty removed entries as designed). Manual EAT appears only for edible ready definitions; the details popup now names the category. Device pass pending.

**Design review additions — October 30, 2026 (requirements, not new verification):** the current shared camp/faint/food recovery rate is **1/3 heart per active second**; from zero, three hearts take 9 seconds and twelve take 36 seconds. The old 0.25 rate and 12/48-second figures below describe earlier builds. Current test-leg drain is 12 hearts/km and maximum boots speed is 2.0 m/s; authored regional drain remains configurable. Before closing this milestone, exercise section 4.4.3 explicitly: consume a twelve-heart premium-food fixture at 3/3 and confirm the excess is lost under the October 30 no-reserve revision (section 4.4.3), deliver progressively below the cap while walking, consume multiple servings without rate stacking, expire a hat without deleting pending hearts, and force-close after consumption without eating twice. Use a temporary authored food fixture if bakery/crafting is unavailable; it must not unlock a recipe or claim that the bakery is implemented. Resolve pending recovery before auto-eating prepared food or fainting; raw ingredients never prevent fainting and pending crafts are inedible. Test exact-one-serving auto-eat, queue-exhaustion/zero boundaries and the 0.0001-heart empty tolerance, foreground menu recovery, suspension freeze and wake-stopped behavior. Verify original save preservation, simultaneous gear expiry and side-panel ownership. Keep gravity-based spill/overpacking acceptance in its dedicated deferred milestone; current debug overflow is the temporary behavior. Later Milestone 14 reuses these food checks with all seven real bakery definitions and checks their discovery/payment rules separately. Document fresh device evidence before claiming these additions passed.

**Implementation checkpoint — October 30, 2026: Milestone 4 exit check passed (15:41 build).** On nubia NX769J the user confirmed every remaining device case: current controls and caps (centre speed line, thermometer, open-bag/side-panel/popup ownership, 0.8 m/s starter and 2.0 m/s maximum after boots expire, yaw never moving the walker), drop/re-pickup with exact quantity/quality, force-close after pickup/drop without duplication, corrupted-primary recovery from backup with the original v1 profile unchanged, wear across force-close, worn-gear preservation after fixtures, simultaneous expiry into overflow and CLAIM OVERFLOW, menu-open camp recovery and suspension freeze, fainting at 3 and 12 maximum hearts with the 2–4-entry penalty and stopped wake-up, prepared-food auto-eat instead of fainting, and all section 4.4.3 no-reserve food cases. All automated checks pass (34 movement, 41 inventory, 13 Milestone 3 persistence, 21 Milestone 4 persistence/migration, plus the equipment/stamina model). Deferred items remain deferred: gravity/collision inventory with central quick-drop, overpacking band and expiry spill to the trail; camp-return drop cycles (TRAIL START is the debug stand-in); trading; real crafted/bakery food; final hook/tool-belt art; and the durability calibration toward about 10 km for tier-three L10 Q5 gear. Next implementation milestone: Milestone 5. Changes remain uncommitted.

**Recovery checkpoint — October 30, 2026 (historical): Milestone 4 was in progress.** A separate `SurvivalBootstrap.unity` / `SurvivalTrail.unity` prototype now integrates stamina, gear test slots, side storage, overflow, food consumption, expiry and a persisted faint-loss manifest. Schema 2 uses `session-v2.json` alongside the unchanged Milestone 3 `session-v1.json`; migration copies the old profile, clamps its debug speed to starter gear, and keeps the original file. Full-stack test pickups and debug tools remain enabled.

- *(Superseded October 30, 2026: the shared rate is now 1/3 heart/second; see section 4.4.2.)* The user confirmed stamina regeneration at **0.25 heart/second**. Apply this to safe-area regeneration and faint recovery; food delivery also uses 0.25 heart/second. From zero, faint recovery takes 12 seconds at a three-heart maximum or 48 seconds at twelve. Trail standing still has no free regeneration. The user instruction supersedes the earlier 0.05/second and fixed-12-second faint proposals.
- A previous development APK passed phone checks for migration, five rank-four test slots, pocket/hook storage, simultaneous expiry preserving contents, distance-only gear wear, zero stamina with ready food, gradual food use, and a 2–4-entry faint manifest that survived force-close without reapplying loss. A real spare hat was equipped, restored after force-close and expired. Camp regeneration was observed at approximately 0.25 heart/second. Evidence: `Builds/Android/Milestone4-SurvivalTest/` (ignored by Git); original v1 save was copied to `01-m3-original.json` before testing.
- Work added after those phone checks includes preserving worn gear when debug fixtures change, a hold-for-details/Eat popup, precise expiry-boundary handling, per-slot selection/unequip, and fractional remaining-wear serialization. An earlier build request returned `FAILED: Milestone 4 build failed: Failed`; Unity's detailed cause has not yet been diagnosed. The APK last written at 11:00:43 is an earlier build and must not be treated as verification of the current source. The local standalone model suite passed 54 checks; the latest completed Unity save/migration report contains 16 checks, while newer validation changes are pending a successful refresh/run.
- **Build recovered — October 30, 2026, 12:08:** the earlier failure's log was lost when the Editor restarted, and a fresh request-file build of the current source succeeded (`Builds/Android/TheTrailWalker-Milestone4-Development.apk`, SHA-256 `7337771106361AFBC4B554A6E9E721E27B7E4B0B1DF0C09F8DF708BA9989843E`). Its preparation reran and passed 34 movement, 35 inventory, 13 Milestone 3 persistence and 18 Milestone 4 persistence/migration checks; the separate equipment-model validation also passed. The APK was installed over the existing prototype on nubia NX769J, and the on-device `session-v1.json` still matches `01-m3-original.json` (SHA-256 `2519B15E…87E4`). The device checks listed below have not yet been performed on this build.
- **Phone feedback fixes — October 30, 2026, 12:48 build:** the user's "upward swipe does not walk" report was the GEAR / SLOTS panel auto-opening the backpack (an Inventory pause) and its CLOSE leaving the backpack open; an adb swipe on the closed backpack walked normally. Closing the gear panel now also closes a backpack it opened, ignored SPEED swipes explain the active pause, the SPEED strip is drawn, and buttons no longer overlap it. Gear disappearing on unequip was the debug double-tap expiry plus virtual R4 test gear; slot taps now only select, expiry has an explicit EXPIRE SLOT button, and R4 TEST GEAR grants real items. Wearables equip by dragging a bag item onto the gear panel (swapping a worn item back to the bag), by the item details EQUIP button, or by selection; a worn slot dragged into the bag (or TAKE OFF) returns it there, using overflow only when the bag lacks space. Equipment is deleted only at zero durability or by explicit debug expiry. Editor play checks covered panel close, R4 take-off/re-equip, swap, backpack-slot drag-off and EQUIP; all Milestone 1–4 validations passed again before the build was installed on nubia NX769J.
- **Inventory UX revision — October 30, 2026, 13:10 build (implemented):** a closed backpack shows only its opening (which is the quick-storage drop target, without the old "drop here" text) and a small BAG button at bottom-left just above it; the whole bag appears only when opened. GEAR / SLOTS opens the whole bag and closing it closes the bag. The hold-for-details popup closes immediately after EAT/EQUIP and automatically after four seconds otherwise. A debug **TRAIL START** button returns to 0 m keeping bag, gear and stamina, so wear and faint tests can continue past the end camp (camps are safe areas, so fainting never triggers there).
- **Speed-control revision — October 30, 2026, 13:17 build (implemented):** vertical speed drags now start on a narrow centre line (40–60% of safe width, 17–54% of safe height, between the closed bag and the debug buttons) instead of the left strip; horizontal drags anywhere still orbit. A narrow thermometer above the BAG button appears only while speed is being adjusted (and briefly after): its height follows the current boots cap (half height at the 0.8 m/s base, full at 1.6 m/s) and its fill shows requested speed. The speed gesture still takes priority over a pickup under the finger on that line. The Milestone 2 speed-area check was updated; all Milestone 1–4 checks passed, and on nubia NX769J an upward centre drag walked at 0.51 m/s with the gauge showing 0.5/1.6.
- **Stamina, food and side-panel revision — October 30, 2026, 13:51 build (implemented):** the test leg drains at the confirmed **8 hearts/km** (the 158.5 m trail costs about 1.27 hearts). Raw berries no longer prevent fainting; carried prepared food is auto-eaten at zero stamina. A debug **Test bread** (prepared, 2 hearts per serving) is added to MIXED until crafted food exists. The open bag spans the full screen width from the bottom edge, with six bag columns visible; touching the container (not an item) scrolls it sideways to reveal the **hooks panel left of the bag and the pockets panel right of it** (only unlocked positions, two columns of five). This implements Milestone 4 task 2's sideways navigation, replacing the gear-panel pocket/hook buttons. Items are dragged between bag and side cells, side items return by dropping in the bag or on the opening, and dragging near a screen edge auto-scrolls. All Milestone 1–4 checks passed; on nubia NX769J the closed opening, full-width open bag and sideways scroll were confirmed by screenshot. Editor checks confirmed bag-to-pocket drag, bread auto-eat, and fainting while carrying 1,002 berries.
- **Side-panel layout revision — October 30, 2026, 14:07 build (implemented):** hooks form one straight, spaced row running left from the bag (hook 1 nearest), each slot two bag cells wide for tool models, with a placeholder belt and hook shape. Pockets form two spaced rows running right from the bag. Cell size now also respects a 45% screen-height limit on short screens. **Deferred art requirement:** in the finished game each hook must look like a real hook holding its tool, reading as a tool belt.
- **Drop pocket — October 30, 2026, 14:17 build (implemented):** while the bag is open, a small DROP pocket appears opposite CLOSE (right side, above the bag). Dragging any bag, pocket, hook or overflow item onto it returns that whole entry to the trail about 2.5 m in front of the current view, as an ordinary visible pickup that can be collected again. Worn equipment cannot be dropped this way. Four Milestone 2 checks cover wrong-pointer rejection, whole-entry drop, no double drop, and re-pickup without duplication (39 inventory checks pass); an Editor test dropped and re-collected a tool.
- **Confirmed dropped-item rules:** a dropped entry stays on the trail until picked up; uncollected dropped items are deleted when the segment's next drop cycle rolls, i.e. when the player reaches or travels to that segment's previous/start camp (section 7.3). That deletion arrives with camp-return drop cycles in Milestones 5–6. Player-dropped stacks keep their exact quantity, quality and durability, and picking one up again returns that same stack. The deferred one-item collection rule applies only to generated trail resources, never to player-dropped entries.
- **Confirmed expiry spill rule (deferred to the gravity-inventory/drop-cycle work, not implemented):** when a shirt or pants reaches zero durability, its pocket/hook items go directly above the backpack for storage; whatever has no bag space falls to the trail as a player-dropped item under the rules above. Until then, displaced items still go to the debug overflow list (CLAIM OVERFLOW). See section 4.4.1.
- **Confirmed overpacking allowance (part of the deferred gravity inventory, not implemented):** a visually full backpack keeps a small slack band above its top line, nominally 1 cm, like an overpacked bag. After items settle, any item or stack whose **centre point** lies above the top of that band is dropped to the trail as a player-dropped item (same rules as above); an item whose edge merely touches or crosses the line, with its centre still inside, stays. Applies equally to single items and full stacks. **Confirmed sizing:** the band is a percentage of the backpack's interior height, so it scales with the bag on every screen and gives the same capacity on every device: **20% of bag height** (800 logical units for the current 4,000-unit/four-row bag). On the test nubia NX769J this is about 0.94 cm, close to the requested 1 cm. Treat 20% as an editable starting value for device testing.
- **Current pickup placement (unchanged):** quick-stored pickups still go to the first free position from the bag's left side.
- **Phone feedback fixes — October 30, 2026, 14:52 build (implemented):** (1) Walking to zero stamina stopped the player but never fainted: float path distances left a residue just above zero (about 1e-8 hearts). An empty-meter tolerance of 0.0001 heart now governs stopping, the walking limit and fainting; fainting also clears walking intent, so saves made while stuck reload into a valid faint. Verified in the Editor by walking to zero, reloading a stuck save and waking stopped at 12/12. (2) Test-leg drain raised to **12 hearts/km** (3 hearts last 250 m, about 1.6 test sections). (3) Shared recovery rate raised to **1/3 heart/second** for camp regeneration, faint recovery and food delivery. (4) Maximum boots speed raised to **2.0 m/s**. (5) **TRAIL START** now stands in for returning to the segment's previous camp: it deletes uncollected player-dropped items (a persisted `dropped` flag), keeping fixture resources; real camp-return drop cycles remain Milestones 5–6. Items dropped before this build are not marked. New checks cover the residue faint, walking-to-zero, and drop-cycle removal; all Milestone 1–4 checks pass.
- **Confirmed inventory UX revision — deferred to a dedicated milestone (not implemented):** replace the fixed-position spatial bag with a gravity/collision container. Quick-pulled pickups appear above the opening and fall into the bag; dragged items can be released anywhere along the opening's horizontal span, kept on screen. Inside the open bag, items can be pushed around; others are displaced by collision and settle under gravity once released. With gravity, the closed opening shows the **centermost section** of the backpack: every picked-up item drops into that central area, and the player rearranges items after opening the inventory. Growing stacks push surrounding items aside by collision (replacing the interim relocate/partial top-up rule in section 5.2). This supersedes Milestone 2's fixed-position placement and needs its own design for deterministic settling, save/restore of settled positions, stack merging on contact, capacity/overflow rules and new validation before implementation.
- **Design-review coverage — October 30, 2026, 15:21 build:** the debug Test premium food is now a **twelve-heart** prepared-food fixture (same item ID; not a recipe or bakery item). New automated checks cover: twelve pending hearts retained at a full meter, progressive delivery while walking at 1/3 heart/second, several servings without rate stacking, hat expiry clamping without deleting pending food, pending recovery preventing faint before auto-eat, re-picked drops keeping quality/profile/remaining wear, and pending food plus dropped-item provenance surviving a save (Milestone 2: 41 checks; Milestone 4 persistence: 21; all Milestone 1–4 checks pass).
- **Confirmed on device (user, October 30, 2026):** drag-to-equip/unequip, equipment expiry returning slots to defaults with storage handling, camp regeneration, the EAT button, and walking to zero stamina fainting at both 3 and 12 maximum hearts with the 2–4-entry penalty applied. On the 15:21 build the user also confirmed carried prepared food (twelve-heart test fixture) is auto-eaten at zero stamina instead of fainting.
- **Save recovery on the v2 save — October 30, 2026 (passed on device):** with the app force-stopped, all save files were copied to `Builds/Android/Milestone4-SurvivalTest/recovery-153033/` (ignored by Git), the primary `session-v2.json` was deliberately corrupted, and on launch the app reported "Recovered the previous backup; recent changes may be missing", loading the backup (73.5 m) instead of the primary (76.0 m), then saved valid snapshots again. The original `session-v1.json` hash remained unchanged. Because the backup restored walking intent at 2.0 m/s, the walker moved about 35 m before being stopped, and all five worn R4 test items (33.5 m remaining) expired together: worn slots returned to defaults and every carried entry kept its owner, with nothing pushed to overflow.
- **No-reserve food revision — October 30, 2026, 15:41 build (implemented):** at the user's correction, food delivery runs at the fixed 1/3 heart/second rate whenever a serving is active; at the cap it only balances walking drain and any excess is lost instead of being retained. The HUD label now reads "Food delivering". Model and editor checks were rewritten for wasted excess at a full stationary meter, balancing at the cap while walking, progressive gain below the cap, catching up with the maximum, safe-area plus food at the cap, and unchanged rate stacking; all Milestone 1–4 checks pass. Device tests of user checks 1–6 and 13–16 passed (user, October 30, 2026); the food checks remain to be run on this build.
- **Device verification complete — October 30, 2026:** the user confirmed all section 4.4.3 food cases on the 15:41 build; see the exit-check checkpoint above.
- **Retest feedback fixes — October 30, 2026, 17:14 build (implemented):** (1) Hold-drag release over the closed opening returned items to the trail because the opening had become a thin strip at the bottom edge; releasing over the opening or within a band 10% of screen height above it now stores the item (Editor-verified for both). (2) Stacks now follow the confirmed stack identity revision in section 5.2: one stack per item ID with a quality pile, lowest-quality-first eating, and pickups that store all units (top-up plus a new stack). The save gains an optional per-item `pile`; older saves load as one portion, and the phone save converted on first load. Milestone 2 checks were updated for the new rule and extended (56 pass); Milestone 4 persistence covers pile reload and mismatch rejection (26 pass). User retests of tree-food controls, saves and food behaviour passed on the 16:39 build; on this build the user confirmed drag-to-opening and quality piles with overflow. (3) 17:22 build: a pickup into a stack wedged against a neighbour was stored as a separate stack because the full stack could not grow in place; the grown stack now relocates to free space (or tops up as far as it fits), with a matching Milestone 2 check (57 pass). The user confirmed on device that the wedged stack now fills (Q1 x997 + Q3 x3) and that eating takes the lowest quality first. **All Milestone 1–4 retest flags and feedback fixes are now verified on device; Milestones 1–4 remain passed on the 17:22 build. Next: Milestone 5.**

**Model-only Android test:** `Builds/Android/TheTrailWalker-Milestone4-ModelTest.apk` is a separate package (`com.thetrailwalker.milestone4.modeltest`) and does not open the Milestone 3 profile. On nubia NX769J, all 15 standalone model checks passed; evidence is in `Builds/Android/Milestone4-ModelTest/` (ignored by Git). This is not a playable Milestone 4 build and does not test inventory transfer, save migration, or faint-loss transactions. The original prototype package and Android project identifier remain `com.thetrailwalker.prototype`.

Dependencies: 3.

1. Add five gear slots, starter rags, boots/hat caps, and quality-adjusted durability.
2. Implement zero starter pockets/hooks, independent caps of ten each, eligibility, inventory sideways navigation/readable ten-position panels, and contextual detail/Eat popup. No weight limit.
3. Add fractional hearts, food use, authored per-leg distance drain, progressive safe-area regen, and zero-stamina/no-food faint recovery with 2-4 eligible carried-entry loss. Keep regen independent of UI/trading locks and persist the loss manifest before animation.
4. Implement distance-zero equipment removal, immediate no-hat/no-boots/rag-shirt/rag-pants/rag-pack fallback, lost equipment bonuses, cap clamping, and atomic content transfer to valid default storage/overflow. Debug-expire each slot and all slots together.

**Exit check:** starter/global caps, no-weight fit, and 1.5x stack scale hold. Expired equipment transfers contents without accidental loss. Faint loss is deliberate: 2-4 actual eligible entries/stacks, worn outfit protected, no duplicate penalty on reload, raw food never prevents fainting while carried prepared food is auto-eaten instead; recover/wake in place and wait for upward speed input. Regeneration/trading remain independent.

### Milestone 5: Two segments, camps, map, and toll

Dependencies: 4.

1. Add an asynchronous transition between two trail segments and a camp view with four seats. Place the camp clearing beside the road on an authored left/right side; on arrival, move the player off the road to the player seat by the campfire (speed 0); stand up and return to the road only on the upward speed gesture (section 12.1). Persist seated state across reloads.
2. Persist visited camps, region discovery, camp visit IDs, and last trail camp.
3. Add a minimal regional map and ferry NPC with a one-time test toll.
4. Block input during transitions and bind the persistent player at the arrival anchor.

**Exit check:** repeated travel creates one player/session only, a paid toll remains paid after restart, locked regions cannot be fast-traveled into, and return location is correct. The camp sits beside the road on its authored side; arrival seats the player at speed 0; only an upward speed gesture stands them up and returns them to the road; reloading while seated restores the seat.

**Implementation checkpoint — October 30, 2026, 18:17 build: Milestone 5 implemented; automated checks pass; device tests pending.**

- **Scenes:** open `Assets/_Game/Scenes/TravelBootstrap.unity`; it loads `TravelTrail.unity`, which builds the segment leaving the saved camp. **The Trail Walker > Milestone 5** prepares, validates and builds `Builds/Android/TheTrailWalker-Milestone5-Development.apk` (request file `Temp/Milestone5.request`). The APK was built but, at the user's request while away, not installed.
- **Test world (starting data):** Dawn Plains (Dawn Meadow, Quiet Meadow, Low Meadow) and Windward Hills (Dawn Hilltop, Low Hilltop); each final camp has a river with dock, ferry boat and toll NPC (tolls 50 and 80 test cash). Region 3 is the locked next silhouette. Loose drops use each region's three featured IDs (Rabbit pelt, Apples, Flint and Pine cone were added as item definitions).
- **Camp flow (section 12.1):** camp clearings sit beside the road on their authored side with campfire, four log seats (three traders), and a standing mission NPC. Reaching a segment's end walks the player to the seat (input blocked), commits the arrival to the save, then reloads into the camp's outgoing segment seated at speed 0 (camp-safe regeneration). Only an upward centre-line gesture stands the player up and walks them back to the road; the stand-up drag sets the starting speed. The region's final camp refuses to stand until the toll is paid, and the ferry then crosses to the next region's first camp.
- **Map (section 12.3):** while seated, MAP lists the region's camps from last to first (reached ones selectable, current and unreached disabled) with other discovered regions below the first camp; region entries arrive at their first camp. Travel is blocked while fainted. Debug: +100 CASH (wallet stand-in until Milestone 9) and BACK TO CAMP (a real return to the segment's start camp).
- **Drop cycles (section 7.3):** every camp arrival rolls a new cycle for the camp's outgoing segment, replacing its uncollected generated drops and player-dropped items; reloads and standing up never roll. Trail items carry their segment, and only that segment's scene shows them.
- **Save schema 3:** `session-v3.json` adds the travel block and trail-item segments; a copy of the v2 profile (or v1 through v2) migrates seated at Dawn Meadow with carried items, gear and stamina kept; old prototype-trail items are not carried over; earlier files stay untouched.
- **Automated evidence:** 40 Milestone 5 checks (travel rules, first-camp versus selected-camp resolution, locked camps and regions, toll once and persisted, river blocking, interrupted-arrival resume, deterministic drops with fixed positions, schema 3 validation and migration) plus all Milestone 1–4 checks (34/57/13/26 and the equipment model). An Editor play run migrated the v2 profile, stood up, arrived at Quiet Meadow, travelled back to Dawn Meadow (cycle 2), reached Low Meadow, was refused the ferry until paying the toll, crossed to Dawn Hilltop, survived a restart unchanged, and travelled back to Dawn Plains' first camp, with no runtime errors.
- **Device feedback round — October 30, 2026, 19:45 build:** the user's Milestone 5 device tests passed. Changes from that feedback: (1) **Restart rule (confirmed, revised the same day):** if the game closes while walking a segment, reopening returns the player seated at that segment's starting camp; collected items stay and segment progress resets. This **counts as a camp visit**: a new visit starts and the segment's resources reroll. (2) **Resource density (confirmed starting counts):** every walkable segment has at least 20 gathering points (about one per 7 m on longer legs, alternating sides, clear of both camps), at least 4 chopping trees, 2-3 animal spawns of 1-4 animals (at most 2 for large animals from tier 3), and fishing points (1 per segment in region 1, 2-3 elsewhere); longer legs between camps, typical from tier 3, get more. Trees, animal spawns and fishing points are fixed authored anchors shown as placeholders (cut-off trunks, small animals, ponds); their interaction is Milestone 6. (3) **Region meshes:** the six region 1-2 loose-resource meshes are scaled evenly instead of being squashed into the old cube; their inventory footprints follow mesh proportions (Pine branch 1600x488, Rabbit pelt 1000x300, Apples 700x800, Hemp 1000x300, Flint 600x382, Pine cone 602x700; 300-unit minimum) and world size matches the footprint. (4) **HUD (confirmed):** square beige icon buttons. BAG sits bottom-left and a **user menu** button bottom-right opposite it; pressing the user button shows crafting, map, equipment and settings stacked upward above it (crafting nearest) and hides them when pressed again. While the bag is open the trash sits left of the user button on the right side. The menu replaced the separate MAP and GEAR / SLOTS text buttons and an earlier player-summary button; crafting and settings show "arrives later" messages. Crafting and settings icons were drawn to match the generated set. All checks pass (Milestone 5: 45).
- **Item presentation round — October 30, 2026, 20:26 build:** (1) Sizes from device feedback: Apples footprint 420x480 (smaller in the bag and on the trail); Pine branch keeps its 1600 length but is slimmer (footprint height 240, mesh thinned to 45% in height/depth on the trail so it no longer reads as a log section; the previous size suits future logs); Rabbit pelt keeps its bag footprint but shows at 60% on the trail. (2) **Bag items show their models (confirmed):** each meshed item is rendered once to a front-view picture used in the bag, pockets and hooks, drawn about 10% larger than its footprint, which acts as the slightly smaller collision box. Bag entries show only the quantity as a plain number ("10", full stack "1k"); names and quality appear only in the details popup. Items without a mesh yet keep a coloured placeholder with their name. (3) **Details popup (designs already in the project, confirmed):** the parchment background and numbered gold quality stars (`Resources/UI/ItemDetails`) are used; quality no longer appears as Q1-Q5 text. (4) Restart now rerolls, per the revised rule above. All checks pass.
- **Item presentation fixes — October 30, 2026, 20:46 build:** bag pictures of items rendered in the same frame showed each other's meshes (an apple pierced by a branch, a branch tinted by apple), because earlier capture objects were removed only at the end of the frame; each capture now uses its own position and is destroyed immediately. Rabbit pelt bag footprint shortened to 850x300 (also slightly smaller on the trail). Apples show at 80% on the trail (bag size unchanged), a mild reduction that keeps the model readable. The tier 1 interactable meshes (pine trunks, rabbits, fishing points) and tier 1 drop meshes integrated separately are used by the trail scene and the bag picture lookup; other tiers keep placeholders. All checks pass.
- **Item and hills round — October 30, 2026, 21:16 build:** Pine branch footprint 1200x200 (shorter and slimmer); Pine log 1300x650 (was 1800x900); Flint, Pine cone and Red berries at apple size; Hemp at pelt size (see the resource size standard in section 5.2). Pick boxes now move with models lowered onto the ground, so branches and pelts are picked where they are drawn. Trail items show only their model, with no name, quantity or quality label. **Windward Hills** gains its plan-named second camp **Quiet Hilltop** (stable ID `camp.02.03`, so existing saves load): Dawn Hilltop to Quiet Hilltop is a gentle transition (about +/-2.5 m) and Quiet Hilltop to Low Hilltop climbs and descends about +/-8 m. Segments carry a relief value; trail scenes build a road-following terrain strip so camps, drops and interactables sit on the slopes. Region 1 stays flat. Milestone 2 checks that assumed the old berry size were made size-relative; all checks pass (Milestone 5: 47).
- **Feedback round — October 30, 2026, 21:29 build:** size standard applied to the region 3-6 loose resources (section 5.2); branches stand upright on the trail; the terrain is now a grid whose height follows the nearest road point, because the earlier road-following strip folded over itself on tight bends and flickered (reported near 118 m on the road from Dawn Meadow); the open bag sits 6% of the safe height above the screen edge so dragging near its lower edge never triggers the system home gesture. All checks pass.
- **Speed, slope and container-art round — October 30, 2026:** speed caps raised to 1.0 m/s without boots and 3.0 m/s with rank-four boots (+0.5 per rank, section 4 table). On climbs and descents the road disappeared under the side terrain: grid heights followed the nearest 2 m road sample (a stepped profile), and flat 4 m triangles cut across the curved road. The terrain now takes the height of the closest point on the road line and, within about two cells of the road, sinks by what a cell can rise on the local slope (up to about 0.7 m beside the steepest Windward Hills leg; flat legs unchanged). A new check samples the road centre and edges every 0.5 m to confirm the terrain stays below. **Camera tilt on slopes:** the first-person pitch (15 degrees down) adds the road's slope in the viewing direction, smoothed over about 0.35 s, so climbs and descents keep the flat-ground view distance and pickup reach; it levels while seated or walking to a seat, and a sideways view along a slope stays unchanged. **Inventory container art** (integrated separately, reviewed): each shirt, pants and backpack selects its own skin by exact item ID (colours plus a vertex-coloured Canvas mesh per kind: bag, opening, pockets, belt, tool loops); base rags without gear. **Confirmed:** only backpacks (bag and opening) and shirt pockets change with the worn item; the pants belt and tool loops are one universal design. Bag and pocket interiors are drawn darker (no light inside), including the rag patches; item pictures keep normal brightness for convenience. A new check requires every craftable shirt and backpack to have its own skins, so the neutral fallback serves only debug gear. **Region 1 environment kit** (integrated separately, reviewed): twelve low-poly decorative meshes (bushes, dry shrub, three full-foliage pines, grass, wildflowers, mound, boulder, ground and shoulder tiles) without colliders or harvest signals, plus a deterministic art-study placement; not yet placed on the live route.
- **Camp art, segment dressing and HUD round — October 30, 2026:** vertical branch pickup confirmed on the device. Hemp and Nettle fiber now also stand upright on the trail (stalk bundles such as Reeds, Wheat and Oats are already modelled upright); bag pictures stay horizontal. Camps use the authored art: burned-wood campfire, log seats from the region's wood tier (pine for regions 1-6) facing the fire, and the three traders seated on their logs (male/female bodies, six skin tones, four hair colours, outfit and backpack rolled by `CampNpcs` per camp and visit, independent backpack, desynchronised seated idle on torso/head/shoulders with hips and feet planted). The player sits on the fourth log. Missing art falls back to the old primitives. The end camp is rolled for the next visit number, and a mid-segment restart shows the start camp for its new visit. Segments are dressed with the region 1 environment kit placed on the actual terrain (works on Windward Hills slopes too) and slightly denser than the art study (every 11 m), clear of the road, camp clearings and interactables; the old 10 m marker posts and spheres are removed from camp segments. Dawn Plains gets the meadow ground, dirt trail and dirt camp clearings. Camp-only buttons (MAP in the user menu, +100 CASH, and the existing PAY TOLL/FERRY) are hidden while walking between camps; the menu column closes the gap. The standing mission NPC and toll NPC are still placeholders.
- **Device confirmation and test-gear round — October 30, 2026:** the user confirmed on the device: camp look and trader idle, segment scenery density, MAP and +100 CASH hiding when standing up, upright hemp/nettle fiber, walking speed, road visibility and camera tilt on slopes, and the darker bag interior. CRAFT is now also camp-only (hidden while walking; crafting happens seated at camps). Rank-four R4 TEST GEAR and ADD SPARE GEAR items now last 50 km instead of 180 m so later regions can be explored without constant breakage; expiry stays testable with EXPIRE SLOT / EXPIRE ALL and the MIXED fixture's 180 m hat. Open: the camp trader models read as odd (about 2,000 triangles of separate rigid boxes); a higher-detail pass is requested.
- **Trader detail and high-detail graphics round — October 30, 2026:** the camp trader generator was redesigned (revision 2, `Tools/Art/generate_camp_npc_meshes.py`): about 5,500 triangles per dressed trader instead of about 2,000, with jointed limbs whose caps hide the seams, shaped male and female torsos and faces, eyes, brows, lips, fuller hair, fingered hands, toes, boot soles and clothing details; the rig contract is unchanged. **High-detail test profile** (the user wants to find device limits before adding graphics tiers): 60 FPS target instead of 30, full render resolution (was 80%), 4x MSAA, 2048 shadow map with two cascades, soft shadows, 70 m shadow distance (was 25), LOD bias 2 and a 320 m camera far plane (was 200). The status line shows a smoothed FPS readout. Later work: low/medium/high graphics tiers for other devices. **Known mismatch:** R4 TEST GEAR grants rank-four capabilities but its bag, pocket and opening art falls back to the tier 1 container skins, because tier 2-4 container art does not exist yet; author it with those gear tiers.
- **Device benchmark — October 30, 2026 (nubia NX769J, Adreno 750, Vulkan):** a hidden benchmark mode (push `benchmark.txt` into the app's files folder with adb; it shows a fixed Dawn Plains leg, never touches the save, and writes `benchmark-results.txt`) ran nine graphics stages while turning the camera a full circle. Every stage held 60 FPS with 1% lows at 60; the display stayed at 60 Hz although 120 Hz was requested. Average GPU frame times: previous low profile 3.1 ms; current profile 4.3-9.2 ms; MSAA 8x 4.3 ms; 4096 shadows at 120 m with 4 cascades 5.6 ms; doubled scenery 4.3 ms; render scale 1.25x 8.3 ms and 1.5x 11.4 ms; combined high (1.25x, 4096 shadows, 120 m, 4 cascades, doubled scenery) 9.5 ms, against a 16.7 ms budget. GPU times vary with the phone's dynamic clocks (the same profile read 9.2 and 4.3 ms), so treat them as upper bounds. No thermal drop was seen over the 2.5-minute run. Conclusion: the device has clear headroom at 60 FPS; the combined-high profile fits.
- **Combined-high profile applied — October 30, 2026 (user approved):** render scale 1.25x, 4x MSAA, 4096 shadow map, 120 m shadow distance, 4 cascades, soft shadows, 60 FPS target, and segment scenery at double density (a placement row every 5.5 m instead of 11 m). The benchmark's 'current' stages now use this profile. Note: editing the URP asset file while the Editor is open was overwritten by the Editor (shadow distance had silently returned to 25 m in the earlier high-detail builds); graphics assets are now changed through the Editor API.
- **Trader arm poses — October 30, 2026:** arms no longer hover above the knees. Female traders rest their hands on their knees; male traders cross their arms at the chest (left forearm under the right, wrists bent so the hands wrap the opposite upper arm). `CampNpcAvatarAssets.PoseArms` aims shoulder and elbow at wrist targets with a two-joint solve; the seated idle still moves torso, head and shoulders by a few degrees.
- **Device confirmation — October 30, 2026, 23:42 build:** the user confirmed every remaining item: terrain flicker fix near 118 m, the lifted open bag, region 2 sizes, CRAFT hidden while walking, 50 km test gear, revision 2 trader models with arm poses, play at the combined-high graphics profile, and no development-console errors in normal play. **Milestone 5 has no pending device tests.** Follow-up from that round: wearable durability (walking distance) now shows in km from 100 m upward (0.12 km, 0.2 km, 50 km) in the gear slots, item details and quick info; below 100 m it stays in metres; tool durability remains a count of uses.
- **Ferry and terrain round — October 31, 2026:** **Ferry (confirmed):** the toll is no longer a button at the camp. From the region's last camp the player stands up and walks the river leg; at the river the ferry NPC opens a dialog asking for the one-time crossing fee, payable in **cash (higher price: 50 / 80)** or **gold (lower number; revised October 31, 2026 to cash / 1,000, at least 1, with tolls 500 / 1,000 cash; table in section 12.3)**; NOT NOW walks back to the camp (a real camp return). After the first payment (persisted), walking up to the ferry loads the next region's first camp directly, like a teleport, with no travel button. Walking toward the river is now a valid saved state; a debug +1 GOLD button joins +100 CASH at camps (wallet stand-in until Milestone 9), and the status line shows gold. **Terrain detail:** every region's ground is now palette-coloured and faceted at build time (packed-earth shoulder within 2.6 m of the road, a dry-grass border to 4.2 m, meadow patches beyond, darker hollows and lighter rises), on a finer 3 m grid, with rolling ground beyond 16 m from the road (kept flat near camps and interactables): about 0.7 m of undulation on Dawn Plains and up to about 3 m on the hilliest Windward Hills leg. The environment kit's flat ground and shoulder tiles were not placed as separate meshes, because flat tiles clip into sloped or rolling ground; their colour design is built into the terrain instead. The road-clearance check now runs on the rolling, styled terrain. Windward Hills also gets the dirt trail.
- **Ferry landing and next-leg preview — October 31, 2026 (confirmed):** crossing the ferry no longer arrives at the next camp. It lands the player walking on the far bank at the start of a whole **landing leg** to the next region's first camp (Dawn Plains -> Windward Hills: `segment.02.landing`, 120 m, gently rolling, with its own resources and interactables, rolled on each crossing); reaching its end seats the player at that camp, and the region is discovered then. Closing the game on the landing leg returns the player to the previous region's last camp (a camp visit), while the toll unlock and the money spent persist. A scene reload that continues a walk (the crossing) is told apart from a closed game by a flag that a fresh app start never has. **Next-leg preview:** each scene also builds the following leg beyond its end camp (road, terrain, scenery, trees and animals; for a river leg, the river), placed in line with the end of the current road, so the road visibly continues on arrival instead of ending in an empty stretch. Loose drops of that leg still roll only on arrival at the camp. The preview's rolling ground can differ slightly from the leg's own scene.
- **Previous-leg view — October 31, 2026:** the user confirmed the ferry landing and next-leg preview on the device. Each scene now also shows the leg already walked behind the start camp (road, terrain, scenery, trees and animals; for a region's first camp, its ferry landing back to the river), turned to end exactly where the current road starts, so looking back is no longer an empty expanse. Its resources are not repopulated: they roll only when the player returns to that leg's camp (camp arrival or map travel). A ferry landing leg shows no previous leg, because the river is behind it.
- **Field scenery and a device bug — October 31, 2026:** a new field-scatter layer (`SceneryScatter`) fills the ground beside all shown legs beyond the 6 m pickup zone, out to 65 m: grass carpet, flowers and low bushes near the road, bushes, shrubs, boulders and young pines in the middle distance, decorative pine groves (noise-driven) with mounds and boulders further out; camps (10 m) and interactables (4.5 m) stay clear, nothing is placed past the open ends of the shown roads (rivers), and placement is deterministic per segment. **Bug found:** the environment kit meshes were imported without CPU read access, so runtime mesh combining produced nothing on the device. The decorative scenery had never appeared on the phone (it only showed in the Editor). The kit importer now enables Read/Write and the meshes were reimported. **Cost on the nubia:** with scenery now actually drawn, the combined-high profile measured 13.0 ms GPU per frame; only pines and tall bushes now cast shadows (grass, flowers, low shrubs and boulders do not), which brought it to 11.8 ms against the 16.7 ms budget, still a steady 60 FPS.
- **App icon — October 31, 2026:** the default Unity icon is replaced by an original low-poly icon (a walker with a backpack, seen from behind on a dirt trail winding through the meadow toward pine groves). `Tools/Art/generate_app_icon.py` writes `Assets/_Game/Art/AppIcon` (adaptive background, adaptive foreground, full-bleed icon); **The Trail Walker > Art > Apply App Icon** assigns the Android adaptive (Android 8+), round and legacy icons and the default icon. The APK contains the adaptive icon resources.

**Implementation checkpoint — October 31, 2026, 00:56 build: Milestone 5 exit check passed.** The user confirmed every device test on the nubia NX769J with the newest build. Milestone 5 closes with:

- **Travel:** Dawn Plains (3 camps) and Windward Hills (3 camps, hills relief), camps beside the road with seated arrival, upward-gesture stand-up, map travel, drop cycles per camp visit, restart returns to the previous camp (a camp visit), save schema 3 with travel, gold and far-bank state.
- **Ferry:** walk the river leg; the ferry NPC asks a one-time toll (cash, or cheaper gold); paid crossings land on the far bank at the start of a 120 m landing leg into the next region, discovered on reaching its first camp; closing on the landing leg returns to the previous camp with the unlock and payment kept.
- **World presentation:** previous leg behind and next leg beyond each scene; palette-styled faceted terrain with rolling ground beyond 16 m; near-road dressing plus field scenery out to 65 m (kit meshes now readable on device); camp art with burned-wood campfire, wood-tier log seats and three seated traders (revision 2 models, arm poses, idle, rolled looks; tier 0 until recipe unlocks); dirt trail and clearings; camera tilt on slopes.
- **HUD and tuning:** camp-only MAP, CRAFT, +100 CASH and +1 GOLD; speed caps 1.0-3.0 m/s; 50 km test gear; durability in km from 100 m; app icon; combined-high graphics profile at a steady 60 FPS (about 11.8 ms GPU on the nubia) with a hidden on-device benchmark.
- **Automated evidence:** Milestone 1-5 suites (M1 34, M2 57, M3 13, M4 persistence 26, M5 60 checks) and the equipment model checks pass.
- **Open notes carried forward:** tier 2-4 container skins for test gear; tier 2-4 trader outfit trims slightly protrude at the shoulders; standing mission NPC and toll NPC are placeholders; Windward Hills reuses the region 1 environment kit; a 120 Hz device test is pending a display setting on the phone; friends user-test APK checkpoint after regions 1-4 and the settlement; reminder at Milestone 7 to check tier 1 trader outfits once recipe unlocks exist.

**Post-completion art changelog — October 31, 2026:** Milestone 5 remains completed. The following entries cover new mesh libraries and revisions to existing character prefabs; new library scene assembly and device validation remain pending as specified below.

- **Common camp NPC mesh rework (revision 3):** rebuilt the male/female campfire avatars with adult head proportions, integrated facial contours, smaller ears, neutral eye details independent of skin tint, more continuous hair, tapered hands/limbs, improved joint and shoe shapes, and more consistent clothing surfaces. Updated the ten existing avatar presets while preserving six skin tones, four hair colors, clothing/backpack support, seated poses, rig pivots and existing idle bindings. Character normals use 65-degree smoothing; appearance selection and discovery logic are unchanged.
- **Animal mesh rework (revision 3, rabbit):** rebuilt the existing rabbit with a leaner continuous torso/neck, shorter muzzle, smaller lateral eyes, narrower ears, connected forelegs, tapered folded hind legs, longer hind feet and a smaller tail. Updated the six mesh parts and existing prefab while retaining rig references and selection bounds. Standing/sniffing and lowered-head grazing renders are static pose studies; locomotion/grazing animations and additional species were not implemented. Both character reworks passed Unity import/prefab generation and visual inspection; 242 unrelated art sources were verified unchanged. Retest NPC appearance/idle at camp camera distance and rabbit silhouette, selection and pose clearance on device after the next build; no new APK or performance result is claimed. See [character revision notes](Docs/Art/Characters/Revision03/character-revision-notes.txt).

- **General environment meshes for all six tier 1 regions:** added 120 reusable meshes/prefabs, 20 per region: Dawn Plains (meadows, shrubs, pines and gentle rises), Windward Hills (wind-bent pines, scrub, ledges and ridges), Mistwood (dark pines, moss, ferns, roots and fallen trunks), Fern Hollow (woodland, ferns, ivy, pale outcrops and hollows), Cloudstep Mountains (alpine pines, scree, cliffs and mountain silhouettes), and Reedwater Delta (reeds, sedge, driftwood, mud/sand banks and static water). `Tier01EnvironmentLibrary` records regional materials, bounds, footprints, clearance and shadow metadata for later trail design. Readable mesh imports, compatible palettes and separate tall-scenery shadow groups preserve the current drawing requirements. These kits do not populate the ten camp segments or replace the live scenery yet; terrain/water pieces require deliberate stitching and regional assembly. See [environment mesh notes](Docs/Art/Environment/Tier01/environment-library-notes.txt) and [six-region preview](Docs/Art/Environment/Tier01/six-biome-overview.png).
- **Mission lady and ferryman meshes:** added a tall, mysterious lady with a broad hat, fine plum dress, dark coat, ivory details and gold trim; four static pose prefabs show sideways rest, player-facing conversation, a held recipe and an extended-arm offering. Separate body pivots and a parchment grip prepare later animation. Added a static ferryman with cap, waistcoat, rope belt, boots, beard and wooden oar with a dedicated hand grip. `SpecialNpcArtCatalog` contains the actor and prop references. Mission logic, turning/idle/handoff animation, recipe rewards and live placeholder replacement remain deferred. Unity asset generation and rendered pose inspection were performed; these additions have no new APK/device-performance result. See [special NPC mesh notes](Docs/Art/Characters/CampSpecial/mesh-design-notes.txt) and [lady pose studies](Docs/Art/Characters/CampSpecial/mission-lady-pose-studies.png).

**Art integration checkpoint — October 31, 2026, 07:59 build (implemented; device tests pending, phone not connected, APK not installed):** the new art from the changelog above is now in the live trail scenes.

- **Regional scenery:** `SceneryScatter` now dresses every shown leg (previous, current, next) from **its own region's** `Tier01EnvironmentLibrary` kit, so Windward Hills gets its wind-bent pines, hill grass, scrub and rock ledges instead of the Dawn Plains pieces, and a ferry landing or the leg behind a river uses each side's kit. It replaces both the old roadside study (`Region01Environment.BuildStudy`, kept only for the Editor study) and the old field scatter: a roadside pass every 5.5 m per side plus the jittered field grid out to 65 m. Rules from the drawing contract: terrain-replacement and distant-only pieces are never scattered; each piece keeps its catalog road clearance (scaled); pieces are combined per kit material and shadow flag (casters separate); kits are never mixed into the 24-colour ground UVs. Ground cover is picked by triangle cost (weight about 200/triangles, 1-9), so grass stays the carpet and the 500-800-triangle flower/fern clumps are accents; this brought Dawn Meadow's non-casting scenery from 224k to 126k triangles (casters about 129k). Deadwood pieces (regions 3, 4, 6) count as rocks. The terrain stays on the legacy Dawn Plains palette in every region (open note).
- **Special NPCs:** the camp mission capsule is replaced by the **mission lady** (sideways rest pose, turned towards the player seat) and the river toll capsule by the **ferryman** with his oar, facing the arriving walker. Both are static; colliders are stripped; the capsules come back only if the art is missing. Mission flow, idle/turn/handoff animation and the other lady poses remain deferred.
- **Already live from the revision 3 prefabs:** camp traders (revision 3 bodies) and the rabbit.
- **Benchmark:** the stress stage now duplicates the whole `Regional scenery` (roadside + field), so it is heavier than the previous run's stress stage and not directly comparable.
- **Automated evidence:** Milestone 1-5 suites pass (M1 34, M2 57, M3 13, M4 persistence 26, M5 67 with 7 new art-integration checks: own 20-piece 32-colour kit per authored region, readable kit meshes, segments 01.01/02.01 dressed only from their region's material with separate shadow groups and at least 2.5 m road clearance, special NPC art present). Editor renders checked the lady at Dawn Meadow, a Windward Hills leg and the ferryman at the dock.
- **Device benchmark (installed, October 31, 2026):** current profile 11.9 ms GPU (was 11.8), combined high with all regional scenery doubled 12.8 ms (was 12.0 with only the roadside study doubled), thermal re-check 11.8 ms; steady 60 FPS at 60 Hz. The regional scenery does not measurably change frame time.
- **System trace (October 31, 2026, 60 s walk Dawn Meadow to Quiet Meadow at top speed with pickups and camera orbit):** median frame 16.6 ms, p95 19.3 ms; 34 frames above 33 ms. Two causes: (1) **the 2-second walking autosave** (`CollectionPrototype` `SaveNow`, pre-existing since Milestone 3) costs about 40-50 ms of main-thread CPU every 2.0 s, with about 5,100 managed allocations per save (a normal frame has about 28) and a garbage collection; disk waits are only about 3 ms. The trace's per-allocation markers inflate this somewhat, but it is still several dropped frames every 2 s. (2) Arrival at the next camp reloads the scene in one 187 ms frame (asset integration; about 20 ms of it is scenery mesh combining). Rendering itself is not the bottleneck.
- **Autosave fix (October 31, 2026, 14:32 build):** stage timings on the phone (without tracing) put a walking save at 18-31 ms of main-thread time: validate and serialize twice (about 6-9 ms), SHA-256 (2.5-6.5 ms), flushed write (7-9 ms), replace with backup (about 1 ms). The main thread now only builds the snapshot and its change-check JSON; `SessionSaveStore.SaveInBackground` hands it to one shared background writer that validates (with the real path length), hashes and writes atomically, in order, keeping only the newest waiting snapshot. `Load`, `ArchiveForReset` and synchronous `Save` wait for pending writes; pause, focus loss and quit save synchronously; a background failure triggers the same "Save failed, progress paused" handling on the next frame. Measured on the nubia: **2-4 ms main-thread cost per save** (was 18-31 ms); the write itself takes about 15-20 ms in the background. Device checks: save and backup hash-valid after a scripted walk; force-close 1.87 m into a leg reopened seated at Low Meadow as a new visit (restart rule unchanged). Development builds append per-save stage timings to `save-timing.txt` in the app files folder. Automated: M5 69 checks (+2: queued saves load the newest; a rejected background save is reported and keeps the last good save), all other suites unchanged and passing.
- **User confirmation:** the mission lady and ferryman look fine on device.
- **Equipment menu, design v1 (October 31, 2026, 14:51 build; design assets in `Docs/Art/UI/Equipment` and `Assets/_Game/Art/UI/Equipment`):** the panel now fills the space from just above the open bag to the top of the safe area, covering the top debug buttons and stats; the written stats summary is gone (the latest message shows as a hint line under the title). Hat/Boots/Shirt on the left and Pants/Backpack on the right show the worn item's model with a durability badge on its lower edge (value plus a proportional bar; `DurabilityStyle`: green above 50%, orange from 10% through 50%, red below 10%, from remaining / maximum; no maximum or infinite gear is neutral). Empty slots show a faint tier-1 silhouette with a neutral DEFAULT badge. The five debug actions stay, in two rows; tap-to-select, drag-to-equip and drag-down-to-unequip are unchanged. **The player avatar is deferred** (user decision: all default designs at once); the centre shows a placeholder stage. R4 test and spare gear keep their debug stats and durability but borrow the slot's tier-1 model (Rabbit-pelt hat and boots, Hemp shirt and pants, Starter backpack, all confirmed present); the bag shows the same models. Tools and wearables in the bag and on the hooks carry the same compact badge (uses for tools, distance for wearables). Panels and badges are fully opaque: the project blends in linear space, so even 1.5% transparency let the bright HUD show through visibly. Automated: M5 72 checks (+3: band boundaries, divide-by-zero/clamp, slot art). Device: layout, models, 48.56 km and 50 km green badges, DEFAULT slot after EXPIRE SLOT and re-equip via R4 TEST GEAR verified by screenshots. User confirmed the menu looks fine.
- **Equipment avatar (October 31, 2026, 15:18 build; art in `Assets/_Game/Art/Characters/PlayerDefault`, notes in `Docs/Art/Characters/PlayerDefault/player-avatar-designs.txt`):** the centre stage now shows the default player avatar, rendered live by its own camera into a texture only while the menu is open (layer 30, at a distant stage beyond terrain and shadow distance, with its own soft key light so the camp sun cannot leave it dark). **Appearance is random** (body, 6 skin tones, 4 hair styles, 5 hair colours), picked once and kept in PlayerPrefs, not the save; the start-of-game selection is deferred (user decision). It wears the neutral basewear plus the worn **hat** and **backpack**, shown with their item models at the art's HatAnchor and BackpackAnchor (debug gear borrows the tier-1 models); fitted shirts, pants and boots need the clothing fitting pass named in the art notes. `PlayerAvatarArtCatalog` (Resources/Characters/PlayerDefault, built by The Trail Walker > Art > Build Player Avatar Catalog) references the art without modifying it. Automated: M5 73 checks (+1: catalog bodies, hair styles, skin/hair colours and anchors). Device screenshot: avatar visible, lit, hat on and pack behind the shoulder.
- **Drag ghost fix (user report, October 31, 2026):** dragging an item showed a yellow text label with its name; the dragged ghost is now the item itself (its bag picture at its bag size, with its durability badge, slightly transparent and lifted above the finger). Dragging a worn slot down shows the worn item the same way. Drop targets still use the touch position.
- **Economy review before Milestone 6 (October 31, 2026, 16:07 build):** warehouse gold revised to 1 gold per 1,000 cash with integer-only payouts (section 12.2.4); bakery premium goods buyable in partial quantities with the discount only on full stacks (9.4.4); balloon merchant every 4-6 camps and mission lady appearance rule recorded (sections 14 and 12.1; both stay at every camp for testing); special balloon gear proposal (14.1); ferry toll table (12.3) and lumber-tier gold unlocks (13.4; revised to 50 / 300 / 750 / 1,500 gold because massive trees will yield about 450 items each after the rebalance). In the game: test tolls are now **500 / 1,000 cash**, the gold toll is cash / 1,000 (at least 1, so 1 / 1 gold), and the debug cash button grants **+1,000**. M5 checks updated (73 pass), all suites pass. The Spanish plan received the same mechanics and progression changes (no development notes).
- **Balloon merchant placement test (art: `Assets/_Game/Resources/Environment/BalloonMerchant/BalloonMerchant.prefab`, notes in `Docs/Art/Environment/BalloonMerchant`):** red pagoda-style cabin with balloon, tethered to a ground stake, with its 4.8 s bobbing loop. For placement testing it stands at **every camp**, just outside the clearing on the far side from the road, door facing the campfire. Exterior art only: no interior, interaction or shop logic (Milestone 14 decides which camps host it and adds the shop).
- **Device retest when the phone is connected:** NPC appearance and idle at camp camera distance (revision 3); rabbit silhouette, selection and pose clearance; mission lady and ferryman look and placement (no floating, no clipping with the dock/clearing); Windward Hills scenery reads distinct from Dawn Plains; no scenery on the road or blocking pickups; frame time at the combined-high profile (previously about 11.8 ms GPU) using the hidden benchmark.

**Next session:** after the device retest above, start Milestone 6 (tree chopping, hunting and fishing per the confirmed designs in sections 8.2.1-8.2.3, with queued drops).

**Milestones 4-5 additions:** implement section 4.4.3 before stamina/faint acceptance, including consume while inventory is open, resume walking, the no-reserve cap rule (excess delivery at the cap is lost), and queue save/reload. Build the camp map with current-region camps plus the other discovered-region list below its first camp, then verify selected-camp and cross-region first-camp arrivals with interrupted transitions. Milestone 14 authors/gates the eight stronger box dishes and seven premium bakery foods/recipes (section 9.4.4); retain six box entries and the existing proposed recipe-roll weights.

### Milestone 6: All three harvesting families and queued drops

**Tree-drop addition:** author and validate the four mandatory food-ingredient tables in section 7.1.1 alongside wood; persist all output types in the same manifest. Reject ingredient-free nodes and verify Normal/Elite bundle ranges.

Dependencies: 5.

1. Implement separate Normal/Elite action tables by family/tier difference, Normal-only tier one, Elite action savings at/below its tier, all-eligible-target Elite drop boost, and one-use-per-committed-action wear. Stop at animation start/remain stopped until upward speed input.
2. Author one cut-off standing tree with the animated axe hint and the section 8.2.1 chopping interaction (focused view, no camera control, Return cancels, horizontal cut lines above the system-gesture area, misaligned swipes start new lines), a moving passive rabbit with grazing animation, bow-hint silhouette and the section 8.2.2 hunting interaction (focused view, animal keeps moving on screen, bottom slingshot/bow drawn by dragging down then released, heart reduced per hit, falls sideways when finished), and a fishing point with ripples, rod-hint silhouette and the section 8.2.3 fishing interaction (draw and cast like hunting, each rod tap spends one use with a line-tensing animation, the final tap pulls the rod and spawns rewards); no animal attacks. Begin with the explicit pine axe/slingshot/rod recipes; test the region-one Rainbow trout/Bluegill pair using placeholder views without waiting for all sixteen fish assets.
3. Roll and persist bundle manifests; full-stack counts follow section 7.
4. Add pooled visible drops, type-grouped pending piles, and partial harvest recovery.
5. Add debug resource/tool tiers and Normal/Elite comparisons; block target tiers above tool tier + 2 before an interaction starts.

**Implementation checkpoint — October 31, 2026, 16:37 build: Milestone 6 first pass implemented; Editor play-tested; device tests pending.**

- **Starting (user note):** an interaction starts by touching the interactable's model or the animated icon above it. Icons are not designed yet; placeholders are the tool's own picture: a rocking axe over trees, a pulsing slingshot over rabbits, a swaying rod over ponds. Icons show within 30 m while walking (not seated, bag closed).
- **Decisions made (open for review):** each interactable can be harvested **once per drop cycle** (a felled tree stays down, a hunted rabbit is gone, a fished pond is spent until the segment's next camp-return cycle), persisted in the save as `harvested` keys (schema 3, optional field). Rewards appear beside the road at the interactable, **up to six bundles at a time per interactable**; the rest wait in the saved queue and appear as earlier ones are picked up. Tool choice is automatic: **the eligible tool with the fewest remaining uses is used first** (user, October 31, 2026), so worn tools are used up before fresh ones; ties go to fewest actions, then Elite, then the lower tier. The same rule picks the replacement when a tool breaks mid-interaction. A requirement hint shows when no eligible tool is carried.
- **Shared flow:** focused camera with control off, harvest stop, RETURN in place of the bag button (cancels; spent uses are not refunded), top strip with the tool model, remaining uses and progress; each committed action spends one use; a broken tool continues with another eligible carried tool (warning) or ends the interaction; yaw restored afterwards. The manifest is rolled once (seeded by segment, cycle and anchor) and saved before anything appears.
- **Chopping:** swipes must cross at least 70% of the lower trunk's width, above the system-gesture area; a swipe off the line (tolerance 6% of trunk height, at least 48 dp) starts a new cut line and wastes that use; the tree falls away from the road when a line reaches the action count.
- **Hunting:** the rabbit keeps moving inside its encounter area and on screen; drag down to draw, aim dots show direction; releases under 25% draw cancel without a use; the stone flies opposite to the drag, farther for longer draws; hits empty the heart by one action's share, misses make the rabbit dart. When the heart empties the rabbit falls sideways and the rewards appear. Idle rabbits graze and hop.
- **Fishing:** drag down and release to cast (no use; always lands); each tap on the rod spends one use with a line-tensing animation (debounced); the final tap completes.
- **Outputs (tier one):** pine tree branches/logs/pine nuts (Normal 2-5/2-5/2-4, Elite 4-10/4-10/4-6), rabbit pelt/meat/bone (2-5, Elite 4-10) and fat (1-3, Elite 1-6), fish 2-5 (Elite 4-10) from the region's pair; 1,000 units per bundle.
- **Debug (removed with the debug cleanup):** **TARGET T1-T4** sets the tier the interactables count as (to test +1, +2 and the blocked +3); **+ TOOL KIT** hands out the next kit in the order T1 Normal (pine trio), T2 Normal, T2 Elite, T3 Normal, T3 Elite, T4 Normal, T4 Elite, at their level-one base uses (debug tools borrow the pine models).
- **Automated:** new Milestone 6 suite, 19 checks (action matrix, Elite boost scope, +3 block, tool choice, base uses, tool identities, manifests and ranges, determinism, tool wear and breaking, harvest keys and save validation); Milestones 1-5 unchanged and passing (M5 73). Editor play test: stood up, chopped a tree with three simulated swipes (manifest of 8 saved, 6 shown, 2 queued), hunted a rabbit with one aimed shot (12 bundles), cast and reeled a pond (4 fish).
- **Not yet done:** massive trees (Milestone 13 presentation), final hint icons, a bow model for tiers 3-4, and the device test round.
- **Next build — model touch fixed, slingshot trail size (user, October 31, 2026):** fish designs, rod size (bag and trail) and axe size confirmed fine. Touching the model still failed for two reasons: the interactable art prefabs ship their selection collider **disabled** ("no input" art), and the pickup touch gave up before the harvest check when its ray hit nothing. Live interactables now enable their selection colliders, and the harvest check runs first (verified in the Editor: a tap on a tree model reached the harvest start). The slingshot (and debug slingshots/bows) now use a smaller trail-only size (0.28); its bag size is unchanged. **Hooks (user):** hooks accepted only the Pine axe; they now hold **any tool** (every unique item that is not a wearable: axes, slingshots/bows, rods, debug tiers), with **one tool per hook** as the only constraint (also on load). M6 suite now 22 checks (+3: every tool family hangs on a hook, one per hook, stacks refused and hooked tools still usable). 17:39 build installed; all suites pass.
- **17:26 build — device feedback fixes (user, October 31, 2026):** (1) **Sizes:** tools are now among the largest bag items, in portrait like their upright models (Pine axe 1000x2000, Pine slingshot 1100x1600, Pine rod 600x2400; debug tools match their family); Pine log 1000x500 (was 1300x650). Trail-only size multipliers: logs 0.65, raw rabbit meat 0.55, animal fat 0.7, tools 0.45 (bones unchanged). (2) **Load migration:** a backpack item that no longer fits its saved spot after a footprint change moves to overflow instead of invalidating the save (the phone save: three pine tools and one Pine nuts stack move to overflow; CLAIM OVERFLOW returns them). (3) **Trail spawns:** Stone and Red berries were not region-one gathering items; they, and the oversized logs and axes, came from the old Milestone 2 debug fixtures (EMPTY / MIXED / FULL BAG / TREE FOOD), which dropped 18 test items on the trail. In the camp world the fixtures now only reset the bag, and saved fixture leftovers are hidden; **tools are no longer in any trail spawn list** (a tool dropped by the player still shows on the trail, to check its model size). Stone remains only a Milestone 2 test item; Red berries are region 5's featured item. (4) **Touching the model** now starts the interaction (the selection colliders are triggers, which the pickup raycast ignored); a pickup in front of the interactable takes the touch instead. (5) **Rewards appear on the harvested interactable**: along the felled trunk, in a ring where the animal fell, or on the pond's near bank; harvested interactables' colliders switch off so they never block pickups. All suites pass (M2 footprint check made size-relative).
- **16:49 build — drawn band and line, grounded interactables:** the slingshot has a drawn elastic band (two strands from the fork tips to a leather pouch holding the stone; the pouch follows the finger up to the full draw, the strands thin and darken as they stretch, and snap back with a small overshoot on release; the stone leaves from the pouch). The fishing line runs from the rod tip to the bobber, bowing while slack; each reel tap pulls it taut briefly, and the final tap lifts the rod and pulls the bobber out of the water before the catch completes. Both are procedural UI lines (`UiPolyline`), no new art. **Fix (user report):** rabbits floated above the ground because interactables sat at road height; every interactable (trees, rabbits by their ground anchor, ponds) now rests on the terrain under it on all shown legs.

**Device confirmation — October 31, 2026, 17:53 build: Milestone 6 device tests passed.** The user tested all three interactions on the nubia NX769J: chopping, hunting and fishing start by touching the model or the icon; cancelling with RETURN works as defined (progress discarded, spent uses kept); when the tool runs out with no eligible replacement the interaction closes as defined; the tool with the fewest remaining uses is consumed first; hooks hold every tool, one per hook; drawn slingshot band and fishing line, grounded rabbits, rewards on the harvested interactable and the revised item sizes (fish, rod and axe confirmed) all passed. All automated suites pass (M1 34, M2 57, M3 13, M4 26, M5 73, M6 23). **Carried forward (not blocking):** final hint icons (placeholders are the tool pictures), a bow model for tiers 3-4, massive trees (Milestone 13), and the debug TARGET / + TOOL KIT controls, which go with the debug cleanup.

**Exit check:** matching-tier Elite axe/hunting/rod need fewer actions; lower targets reduce where above the one-action floor; +1/+2 targets remain eligible and receive Elite boosted drops even without an action discount; +3 cannot start. Normal never gains drop boost, each committed action wears one use, and harvest stop/aim/queue/reload rules remain correct. Chopping: each counted swipe adds one segment and spends one use; a misaligned swipe starts a new line and wastes that use; the tree falls exactly once when a line reaches its action count; no cut gesture can trigger the device's bottom system gesture; Return cancels and discards that interaction's cut lines; completing a trail tree closes the interaction, plays the fall, then spawns rewards; camera control is off during the interaction. Hunting: the animal stays within screen bounds while moving; each hit removes one action's share of its heart (a whole heart for a one-hit Elite); a committed miss spends a use without heart loss; the empty heart closes the interaction, the animal falls sideways, then rewards spawn; tool breaking and cancel behave as for chopping. Fishing: each tap spends exactly one use and the action count of taps catches once; casting spends none; cancel/reselect starts over.

### Milestone 7: Material conversions, recipe mastery, timers, and x1000 crafting

**Reminder for the user (requested October 30, 2026):** once recipe unlocks from missions exist, check the camp traders' outfits on the device: unlocking any one tier 1 clothing recipe (Rabbit-pelt hat, Rabbit-pelt boots, Hemp shirt or Hemp pants) should start showing full tier 1 outfits at later camp visits, and unlocking the Starter backpack recipe should show tier 1 packs independently of the outfit. Wire `CampNpcs.Roll` to the real learned-recipe set (currently an empty set in `MovementPrototype`). No debug shortcut is planned for this.

**Debug cleanup and rebalance (user note, October 31, 2026):** once the mission recipe unlock has been tested and the crafting mechanic tests begin (recipe level-up, stat growth and related checks), **remove all debug buttons and debug item stats** (fixtures, test/spare gear, test cash/gold, test harvest/empty, overflow and expiry shortcuts, R4 test gear with 50 km durability, and similar). Keep only **full-stack pickups**, and rebalance items to their designed values (durability, stats, prices) at that point.

**Tier-one crafted art pass:** `Assets/_Game/Art/Crafting/Tier01/Tier01Recipes.json` and `Resources/Tier01/RecipeArtCatalog` author 59 exact recipe designs, including the optional mixed-milk cheese proposal, with 63 new low-poly output/shop-input meshes and the existing Pine log reused. See `Docs/Art/Tier01/crafted-item-designs.txt` and its labeled galleries. This is art/catalog integration; crafting and recipe acquisition remain pending. Preserve starter-mission overrides, Normal-only pine tools, box/bakery separation, and recursive ingredient discovery/conversion checks. Roasted mushrooms remains blocked until its tier-one source is assigned. Unspecified fixed Material timers are identified proposals; no new loose-resource slots are introduced.

**Focused retests pending after catalog/art integration:** M2 inventory icons, footprints/rotation and stackable versus x1 behavior; M3 rank-one gear presentation/equip/unequip with finite durability profiles; M4 old save IDs/order plus new-ID quality/profile/overflow round trips. These are not passed device checks. M1 movement/stamina logic is unchanged. M7 still owns transactions, mastery, timers and learning; M14 owns source-filtered bakery/box offers, single recipe offers and full-stack discounts.

**Tree-food recipe addition:** integrate section 7.1.1's four ordinary Food recipes and two exact Material conversions, with nonrandom source-gated unlocks. These are outside bakery/box collectible counts. Check exact fuel, bulk yield, crafted quality and raw-versus-prepared identity.

Dependencies: 6.

1. Add nine-slot recipe panel, recipe selector, Autofill, quantity selection, and output/level/timer previews.
2. Implement exact-ID unlevelled material conversions, instant gatherable outputs, and per-definition MaterialTier/RecipeUnlockTier. Add the used cord/charcoal/stuffing bridges. Wrong variety or out-of-window direct input fails without costs; future-tier ingredients never leak into starter recipes.
3. Implement nonmaterial levels 1-10 keyed by unique recipe ID, stat order/Q5 cap, and immutable output profiles. Add Q1-equivalent base pricing, finished-quality premium, and finite-gear durability premium; preserve the 1-cash basic/6,000-cash level-ten Misc anchors. Cow/goat/mixed cheese never shares mastery or substitutes ingredients.
4. Implement category/tier/mastery timers: Food/Tool down with level, Wearable/Misc up, hours for advanced Wearable/Misc, Food/Tool always below one hour even at x1000; buffs never change time.
5. Add reservations, atomic costs, the centered one-second stat/timer presentation with menu kept open, immediate timed-product drop into the bag, exact-once mastery at craft start (revised November 2, 2026), and persistent overflow delivery.
6. Restrict tools/wearables/backpacks to x1 in both UI and domain commands; reject x10/x100/x1000 without spending materials. Keep unique single-item claims, and merge ready stackables only when output-stat profiles match.
7. Add all nine instance-consuming Elite upgrades and earlier-tier material recipes. Reject wrong Normal family/tier/Elite-as-input, missing previous materials, exhausted/mission-owned input, or full queue without costs. Save/resume cannot return the sacrificed Normal or duplicate the Elite; output uses its own recipe snapshot.
8. Author fixed-input examples, explicit pine tool trio, one meat/fish roast per tier, and a later bakery recipe using exact older global Wheat flour/Cow cheese. Check permitted cooking exceptions, rejected nonfood pantry use, exact wood consumption on x1/x100/x1000, no spice/fuel substitution, and the existing Food timer cap.

**Exit check:** exact IDs/quantities and separate recipe mastery hold; all nine Elite upgrades use matching Normal + previous-tier advanced material once, with no ingredient-stat inheritance or extra Normal XP. Partial-wear policy is explicit, previewed; no costs on failure or duplicate items on reload. Material conversion, Q5/timer rules, stackable-only bulk, and overflow handling remain correct.

**Implementation checkpoint — October 31, 2026, 18:22 build: Milestone 7 logic implemented; device tests pending.** Domain `Domain/Crafting`: 59 tier-one recipes generated from `Tier01Recipes.json` (`Tools/Crafting/generate_tier1_recipes.py`) plus three tier-two Elite test upgrades (debug Normal T2 tool + 2 pine planks / fine rabbit pelts / fine hemp cloth); exact IDs and tier window with Food pantry; mastery 1-10 from finished units; deterministic quality/durability/price (section 6.1.1; proposed wearable base 1.6/2.4/3.2/4.5 km); category/mastery timers with batch factor; x1 for tools/gear; atomic consumption lowest quality first; pending outputs placed in the bag without merging (overflow when full) with a timer badge centred on the model; outputs become ready and credit mastery once, also across app closes (device clock); 8 outstanding batches; save section `crafting`. Pending items cannot be eaten, equipped, used as tools or ingredients, or merged (selling/missions/warehouse inherit this later). Camp traders' outfits now use the saved known recipes. UI: CRAFT opens a separate menu above the bag (3 x 3 grid, RECIPES top right, AUTOFILL/CRAFT right of the grid, x1-x1000, RETURN bottom right, one-second centred presentation); debug LEARN T1 RECIPES. Automated: M7 suite 28 checks; all suites pass. Editor play test: instant log conversion and a timed x10 juice batch (pending, ready, mastery L2).

**Crafting and item feedback round (user, October 31, 2026; 20:06 build installed):** Pine plank footprint 1100x550 (log-like, slightly larger than the 1000x500 log; generator override kept) and its bag picture shows the broad face tilted instead of the thin edge that read like a branch. **Trail sizes:** the reviewed sizes (regions 1-2 loose resources, tier-one harvest drops, tools) stay as they are and are the default maximum; every other stackable item's largest trail dimension at a full stack is capped at 1.05 m (about an apple/log) unless tuned. **Crafting menu:** opens on the last crafted recipe (saved), or on the first unlocked recipe on the first opening; the recipe book stays closed until its button is tapped; the book button is an icon (open book); the in-menu X and RETURN are gone: while crafting is open the bag button shows a **return icon** (U-turn arrow) above the inventory at the left and closes crafting and the bag; the panel ends above the HUD button row and the user-menu column closes when crafting opens. New icons from `Tools/Art/generate_crafting_icons.py` (book, return). **Recipe book pages (20:13 build):** category tabs like page guides with icons (Materials: log pile, Tools: axe, Clothing: shirt, Food: apple, Misc: gem); the book opens on the current recipe's page; on each page recipes craftable with the carried ingredients come first (marked "Can craft"), then the rest ("Missing ingredients"), each group by tier then name. Verified on the phone.

**Recipe names (20:21 build):** the recipe book and info block show only the item name (no tier suffix).

**Device results and fixes (user, October 31, 2026; 20:3x build):** passed on the phone: x10/x100/x1000 yields, x1 only for tools, missing ingredients block crafting, pending-item restrictions with the centred timer, timers across app closes, mastery increases. Batch timer clarified: one shared timer per batch by design, grown by 1 + 0.2 log2(batch) (x10 about x1.7, x1000 about x3), easy to miss with short tier-one timers. **Roasted dishes** include a full-width skewer, so the food read much smaller than the raw item: footprints enlarged keeping the model proportions (Roasted rabbit 1300x462, Roasted rainbow trout 1400x421, Roasted largemouth bass 1400x541, Roasted bluegill 1150x615, Roasted yellow perch and Herb trout 1400x467; generator overrides kept). **Debug + INGREDIENTS** in the crafting menu adds the selected recipe's exact ingredients for the chosen batch size (bag, then overflow), so every recipe can be tested without visiting regions 3-6.

**Stack growth flattened and popup order (user, October 31, 2026; 20:59 build installed):** a single unit read far too small next to its full stack (for example one rabbit pelt), while the full-stack size is the intended base look. Stack growth is now `Scale(q) = 1.5 / 1.15 * (1 + 0.15 * log(q) / log(1000))`: a full 1,000 stack keeps exactly 1.5x (bag and trail unchanged), a single unit is about 1.30x instead of 1x, so stacks differ by at most about 15% per side. This supersedes the 1x-1.5x curve recommended in section 4 for the prototype. Non-stackables are unaffected. Validation fixtures and the debug bag presets (full bag, tree food) were re-spaced for the larger partial stacks. The item details popup now draws in front of the crafting and equipment menus.

**Materials size review (user, October 31, 2026):** unit and full-stack sizes in the bag and as crafting requirements confirmed OK for clay cup, cured rabbit pelt, fine hemp cloth, fine rabbit leather, fine rabbit pelt, fired bricks, hemp cloth, hemp cord, hemp thread, limestone tiles, mortar, nettle cloth, nettle thread, pine charcoal, pine log, pine panel, pine plank, plant stuffing, polished pebble, rabbit leather, reed braid and wheat flour. Changed: **Pine beam** 900x300 -> 1100x550 (like the plank; generator override kept); raw **Pebbles** 480x240 -> 600x300 (slightly bigger). **Bag on app entry:** the bag and its menus (equipment, crafting, user menu) close when the app goes to the background, so a resumed app always opens with the bag closed; a cold start already opened closed. The open state is never saved. **Tools:** all tier-one tool sizes confirmed OK. **Clothing:** hemp pants, rabbit-pelt boots and rabbit-pelt hat OK; **Hemp shirt** 900x588 -> 900x780 with its model stretched vertically to match (the low model read as a folded shirt; bag picture and trail only, not the worn avatar); **Starter backpack** 1.5x, 626x900 -> 939x1350 (generator overrides kept). Remaining categories (food, misc) are under review.

**Device result (user, October 31, 2026):** crafted tier-one equipment shows the correct parameters (quality, crafted durability and stats) — passed (part of remaining test 10; crafted tools harvesting with their uses still to confirm).

**Food size review and premium hearts (user, October 31, 2026):** OK: apple-oat compote, berry oat cakes, cow butter, cow cheese, cow-cheese tart (product), goat cheese, herb trout, mixed-milk cheese, premium apple pie, red-berry oat porridge, all roasted fish/rabbit/mushrooms/shrimp, shrimp pie, toasted pine nuts. Reduced to about apple size (apple 420x480): **Apple juice** and **Cow cream** and **Cow yogurt** 839x900 -> 485x520, **Strawberry cow-yogurt drink** 800x900 -> 462x520, **Egg** 665x900 -> 355x480 (generator overrides kept). **Premium hearts:** Shrimp pie recovered only 3 hearts; box and store-bought recipes are premium goods (rule in section 9.4.2). Tier-one box foods now: Roasted shrimp 2 -> 8, Shrimp pie 3 -> 10, Apple-oat compote 6 -> 10, Red-berry oat porridge 6 -> 10 (Premium apple pie stays 12); higher-tier box values updated in the tables as proposals. Prices still follow the category formula and are rebalanced after the debug cleanup. **Misc:** cord charm, pine bird sculpture, pine-cone ornament, pine-cone rabbit ornament and rabbit toy OK. The tier-one size review is complete.

**Milestone 7 device results (user, October 31, 2026; 15:56 build):** passed: crafted tools harvest with their uses, eight outstanding batches with the ninth refused at no cost, tier-two Elite upgrade, traders in tier-one outfits from the next visit, menu behaviour (last crafted recipe, book tabs/ordering, return button, closing with the bag). **Fix:** a pending (still crafting) tool moved to a hook, or a pending stack to a pocket, lost its visible timer; readiness itself was unaffected. Pockets and hooks now draw the centred timer badge too (over the hook loop). **Overflow output on a full bag:** covered by the Milestone 7 automated check; the device test is deferred to the real overflow-tray UI that replaces the debug CLAIM OVERFLOW button. **Milestone 7 exit check passed (16:03 build):** the user confirmed the pending timer in pockets and hooks; every Milestone 7 device test has passed (overflow device check deferred as above). Carried forward: the debug cleanup (all debug buttons and stats removed, only full-stack pickups kept, items rebalanced) happens once Milestone 8 mission recipe unlocks are tested and the crafting mastery tests begin; until then LEARN T1 RECIPES, + INGREDIENTS and + TOOL KIT stay for testing.

**Milestone 7 device tests remaining (October 31, 2026):** (1) craft each available tier-one recipe once (instant materials, timed materials, tools, clothing, food, misc) and check outputs and models in the bag and on the trail; (2) x10 / x100 / x1000 batches for stackables (yield, stacks of at most 1,000, timer grows with the batch) and x1 only for tools, clothing and backpacks; (3) a shortage crafts nothing and spends nothing; ingredients from pockets and hooks count; the lowest quality is used first; (4) pending items: centred timer, cannot be eaten, equipped, used as a tool or ingredient, or merged until ready; (5) timers keep running while the app is closed and finish on reopening with mastery credited once; (6) mastery: level star and progress bar rise, quality reaches Q5 at level 5, food gets faster and clothing/misc slower with levels; (7) eight outstanding batches, a ninth refused without cost; (8) bag full: output goes to overflow (CLAIM OVERFLOW returns it); (9) tier-two Elite upgrade (+ TOOL KIT twice for the T2 Normal trio; craft planks, fine rabbit pelts and fine hemp cloth): one Normal tool + two materials -> an Elite with its own stats; (10) crafted gear equips with its crafted durability; crafted tools harvest with their uses; (11) camp traders start wearing tier-one outfits from the next camp visit after a tier-one clothing or backpack recipe is known; (12) menu behaviour: opens on the last crafted recipe, book tabs and ordering, return button, closing with the bag.

**Recipe info block (user request, done in the next build):** at the top left, opposite RECIPES: the recipe's output model, name and tier, the current level as a star in the quality-star design continued to 10 (`Tools/Art/generate_level_stars.py` -> `Resources/UI/Crafting/level_1-10`; levels 1-5 equal the quality stars, 6-10 add star points), a progress bar and the units to the next level (materials show "no mastery"). The grid moved below it; output details are one line under the grid.

### Milestone 8: Four mission branches, featured resources, and route safety

**Mission flow (confirmed by the user, October 31, 2026):** at the first camp, tapping the tall mission lady opens the **mission selection menu**: a stack of worn papers with recipe-book-like tab separators, one tab per branch (Gatherer, Lumberjack, Hunter, Explorer) showing that branch's next mission, its request, the reward recipe and, when not yet possible, the first missing link. Afterwards, arriving at the camp where the active mission is complete, the lady **hands the recipe over automatically** and the menu **opens by itself**; it **closes by itself** once the next mission is chosen (a PUT AWAY button appears only when no branch has an offerable mission).

**Implementation checkpoint — October 31, 2026, 16:21 build: mission core.** Domain `Domain/Missions/MissionModel.cs`: 22 tier-one missions (the twelve section 11.1 starters plus the section 11.3 extensions G4-G5, L4-L5, H4, E4-E8), ten units or ten instances each; per-branch progress, one active mission, monotonic claims; eligibility = branch's next mission + requested item obtainable now + reward recipe craftable once learned, from an exact tier-one source table (featured loose items per region, axe/slingshot/rod harvest families, settlement listings unavailable until Milestone 10, recursive known recipes). Locked missions show their first missing link (for example G3 "Needs the Pine bird sculpture recipe", H4 "Rock salt is found in region 3", E8 settlement Tea herbs). Automatic delivery (`SpatialInventory.ConsumeForMission`) takes only the remaining requirement from carried ready items (bag, pockets, hooks), cheapest quoted value first; worn gear, overflow, trail items and pending crafts never count. Mission lady presence: a claimable completion, or no active mission while something is offerable (first selection and after a claim); the camp ahead shows her only if she will be there. HUD tracker pill beside the bag button (code, item, delivered/required). Save section `missions` (claimed in order, active, delivered) validated on load. New art: worn paper sheets and tab (`Tools/Art/generate_mission_paper.py`), mission scroll and branch icons (`generate_crafting_icons.py`). Automated: Milestone 8 suite 36 checks (data, featured sources vs catalog and recipe data, region-one offers, 990 left after a ten-unit mission, one active mission, claims once, cross-branch locks, prototype-region reachability of all twelve starters, tool instances, worn/pending protection, craft competition, persistence). Build tooling: the request poller refreshes and waits for compilation first and refuses to run on compile errors. Items 2 (map icons), 5 (route budgets, faint animations) and 6 (camp layouts) of this milestone are not started.

**Mission device feedback (user, October 31, 2026):** the first mission build's checks passed on the phone. Changes: mission progress is shown only as the requested item's **model with a counter** (delivered/required, green when complete) in the square **above the bag button**, with no name or description; hidden while the crafting, equipment or mission menus cover it. The **recipe hand-off focuses the view on the mission lady** (like other interactions focus their target), with the hand-off card below her; the view returns afterwards and the papers open.

**Speed gauge redesign (user, October 31, 2026):** the speed thermometer is now a **worn wooden ruler** with tick marks and **no numbers** (the numeric readout is removed); requested speed shows as a translucent stain up the ruler topped by a dark marker line. The ruler stands **above the mission square** (which stays above the bag button). Its **length grows with the boots tier buff** (speed cap): the full height is the maximum cap and the ruler tiles instead of stretching, so a higher cap shows more ticks. Art: `Tools/Art/generate_speed_ruler.py`.

**Craft slots removed (user, October 31, 2026):** the eight-outstanding-batch limit is gone; any number of timed crafts run at once (the crafting menu shows how many are running and the next to finish). Reason: late-game players crafting many high-quality tools and gear, which have no bulk crafting, would otherwise wait on slots. Milestone 7 check updated (twelve batches at once).

**Device results (user, October 31, 2026; 17:04 build):** passed: mission tracker (model and counter above the bag button), recipe hand-off focusing the view on the mission lady, wooden speed ruler above the mission square growing with the boots cap, speed caps 1.5x, mission region gate (region-two missions open after the region-one missions), and more than eight crafts running at once.

**Implementation checkpoint — October 31, 2026: camp map and complete Dawn Plains (Milestone 8 item 1).** Dawn Plains now has its ten proposed camps (Dawn Meadow, Quiet Meadow, Low Meadow, Clover Bend, Lark Rise, Dew Hollow, Amber Field, Hare's Run, Willow Rest, Last Light; legs 150-240 m, alternating sides), with the river and ferry after Last Light. Camp IDs 01-03 are unchanged; saves whose segment no longer matches their camp (for example seated at Low Meadow when it was the last camp) are reseated at that camp with a drop cycle for its new leg (`SessionSaveStore.MigrateCatalogLayout`). The camp map is rebuilt as the worn themed map described in section 12.3 (`CollectionPrototype.Map.cs`; sheets from `Tools/Art/generate_region_maps.py`; campfire and ferry icons). Milestone 5 travel checks now follow the catalog's final camp and cover the layout migration. Windward Hills keeps its three test camps for now.

**Map feedback (user, October 31, 2026):** the first map showed a blank white sheet (the region textures had been imported before their sprite importer existed). The sheets are now **painted maps with representative colours and shapes per region** (Dawn Plains meadow, wheat patches, groves, a stream, wildflowers and sun; Windward Hills ochre grass, brown hills, rocks and wind; Mistwood dark forest and mist; Fern Hollow ferns, round trees and a pond; Cloudstep Mountains snow peaks and clouds; Reedwater Delta channels, marsh, sand bars and reeds), with a parchment cartouche for the title, open ground along the road and a compass rose. **CLOSE is replaced by a return button with the return icon at the bottom left**, in the bag button's place; the bag, user-menu and trash buttons (and the mission square) hide while the map is open. Camp counts per tier recorded in section 6.2.2.

**Dawn Plains stamina pacing (user, October 31, 2026):** at the 12 hearts/km opening drain, the first two legs keep their lengths and cost less than the starter's 3 hearts (Dawn Meadow -> Quiet Meadow 150 m = 1.8 hearts, Quiet Meadow -> Low Meadow 180 m = 2.16 hearts): G1 is claimed at Quiet Meadow and the G2 apple delivery unlocks apple juice at Low Meadow. From Low Meadow on, every leg costs at least half a heart more than 3, so food is needed: 300, 310, 320, 330, 340, 350, 360 m (3.6-4.3 hearts). **Resources:** every leg's drop roll now contains each of the region's three featured items at least once (deterministic; a missing item replaces the first point of the most common one), so the opening legs always offer apples; one 1,000-apple pickup covers the ten for G2 and leaves 990 for juice (3 apples each). Shared `RouteBudget` (12 hearts/km, starter 3 hearts, 26-heart leg ceiling) in the domain; Milestone 8 checks cover the opening/food-leg split, the 26-heart ceiling and featured items on every leg and cycle. Note: a player who takes Hunter H1 before G1/G2 reaches the first food leg before juice is unlocked; the opening flow assumes the Gatherer start. (Resolved November 2, 2026: G1 is now forced as the first mission.)

**Apples are edible (user, October 31, 2026):** Apples become raw food worth **1 heart**, eaten manually from the bag (EAT) to help with stamina on the longer legs; like other raw food they are **never eaten automatically** to prevent fainting (only prepared food is). They remain the G2 mission item and the apple juice ingredient.

**Session close — October 31, 2026 (18:47 build installed).** Device-confirmed: legs from Low Meadow on need more than the starter's 3 hearts (the player fainted before the camp and the faint mechanic applied); apples are edible by hand and help recovery; the painted camp map, return button and hidden HUD; the complete ten-camp Dawn Plains. Milestone 8 state:
- **Done:** mission core (22 tier-one missions, four branches, papers UI, automatic delivery, hand-off with view focus, tracker, mission region gate, persistence); item 1 (worn painted region maps with featured icons, campfire camp buttons by road side, inactive undiscovered camps); complete Dawn Plains with stamina pacing and guaranteed featured items per leg; speed caps 1.5x and wooden speed ruler; craft-slot limit removed; apples edible.
- **Decided, to implement next session (user, October 31, 2026):** **G1 is the first mission**: the very first selection offers only Gatherer G1 (pine branches -> Pine log), so the opening always runs G1 -> G2 -> apple juice before the first food leg. Other branches open after G1 by the usual rules.
- **Remaining Milestone 8 items:** item 2 extras (map indicators are in; full 24-slot featured data validation exists for tier one only); item 5 route safety (faint/recovery/wake animations, reload checks that faint loss cannot reroll or double, HUD/map/departure route information from the same data); item 6 layouts (~~Windward Hills to its 12 planned camps~~ done November 2, 2026; streaming for larger regions, map scrolling past about 12 camps).
- **Implemented (November 2, 2026):** G1 is the first mission. `MissionState.Offerable` locks every other branch with "Opens after the first mission (G1 Pine branches)" until G1 is claimed; saves already holding another active or claimed mission keep it. Milestone 8 checks updated (first selection offers only G1, H1 refused before G1, legacy active mission kept, H1 opens after G1); all Milestone 1-8 checks pass. Device check pending.
- **Debug cleanup, first pass (user, November 2, 2026):** crafting now supplies real gear and tools, so the camp world removes R4 TEST GEAR, ADD SPARE GEAR, + TOOL KIT and the MIXED, FULL BAG and TREE FOOD fixtures (the older collection/survival scenes, which have no crafting, keep theirs; item definitions stay so older saves load). Kept for upcoming tests: EMPTY, RESET SAVE, TARGET T1-T4, BACK TO CAMP, EAT SELECTED, TEST EMPTY (faint tests, item 5), CLAIM OVERFLOW, EXPIRE SLOT / EXPIRE ALL, LEARN T1 RECIPES, + INGREDIENTS, +1,000 CASH and +1 GOLD (ferry tolls until Milestone 9). **New debug UNLOCK CAMPS** (the old + TOOL KIT spot) marks every camp of the current region as reached, so the map can travel to any of them; other regions, discovery, drop cycles and the player position are unchanged, and it is saved. Milestone 5 checks cover it; all Milestone 1-8 checks pass. Device check pending.
- **Next steps:** ~~make G1 the first mission~~ (done), ~~plan and build Windward Hills (12 camps)~~ (done), ~~item 5~~ (done November 2, device round pending); commit after Milestone 8 (user's request), then the debug cleanup and mastery tests.

**Windward Hills layout (approved by the user and implemented, November 2, 2026):** grows the region from three test camps to its planned **12 camps** (section 6.2.2), following the Dawn Plains approach.
- **Camps and IDs:** the three existing camps keep their IDs and become camps 1-3: Dawn Hilltop (`camp.02.01`, left), Quiet Hilltop (`camp.02.03`, now right), Low Hilltop (`camp.02.02`, now left; no longer final). Sides strictly alternate so neighbouring camp tabs never overlap on the 12-camp map (two same-side neighbours would overlap at this spacing). Nine new hill names (no Knoll/Ridge/Hollow, which are other regions' landmarks), continuing the alternation: 4 Windy Crest, 5 Heather Brow, 6 Gorse Saddle, 7 Cairn Top, 8 Shepherd's Fold, 9 Weathervane, 10 Stone Gate, 11 Kestrel Point, 12 Far Lookout (final camp; river and ferry to region 3 follow it). `segment.02.01` and `segment.02.03` keep their IDs, a new `segment.02.02` joins Low Hilltop to Windy Crest, and `segment.02.river` moves to Far Lookout. `SessionSaveStore.MigrateCatalogLayout` already reseats saves whose segment no longer matches (for example seated at Low Hilltop facing the old river); the paid region-two toll is kept.
- **Pacing (12 hearts/km, unchanged rate):** the ferry landing stays 120 m (1.44 hearts). Region 2 is the first region of the "regions 2-6: 350-600 m, 3.5-6 hearts" band, so every leg needs food from the start and lengths climb steadily: 360, 370, 380, 390, 400, 410, 420, 440, 460, 480, 500 m = 4.32 / 4.44 / 4.56 / 4.68 / 4.80 / 4.92 / 5.04 / 5.28 / 5.52 / 5.76 / 6.00 hearts. Total 4,610 m and about 55 hearts; about 51 minutes of walking at the starter 1.5 m/s or 34 minutes with rank-one boots. All legs stay far below the 26-heart ceiling.
- **Relief (implemented):** 4, 8, 6, 7, 10, 12, 8, 9, 12, 8, 5 m per leg, steepest out of Gorse Saddle and Weathervane. The automated road-clearance check passes on every Windward leg.
- **Food stocking is intended (user, November 2, 2026):** Dawn Plains offers plenty of food to farm, and the player is expected to plan and prioritise stacking food there to cross the following regions. This is why stacks hold 1,000 (the reference game used 10 per stack and the bag filled very fast). Do not add region-two food sources to make the crossing easier.
- **Food check:** with the starter 3-heart cap, a 6-heart leg needs at least 3 hearts eaten on the way (1/3 heart per second delivery). Region 2 has no apples, so the player relies on carried apples and apple juice (1 heart each; a 1,000-apple pickup minus G2 gives 330 juices) and, after Explorer E2, roasted rabbit (2 hearts; rabbits spawn in all tier-one regions). The region-two missions fit this order: L1 Flint -> pine axe, E1 Hemp -> hemp cloth, L2 logs -> pine bird, G3 pine bird -> rabbit hat (4-heart cap), E2 rabbit meat -> roasted rabbit.
- **Featured items and interactables:** each leg keeps Hemp fiber, Flint and Pine cones at least once per drop roll (shared rule); legs of 300 m or more get 3+ fishing points (Largemouth bass, Yellow perch) and 3+ animal spawns.
- **Map:** 12 camps sit at the limit noted for item 6; check the painted Windward Hills sheet for spacing and add scrolling only if the camp buttons crowd.
- **Checks (implemented):** Milestone 8: 12 camps on alternating sides with the river after Far Lookout; every leg 4.32-6.0 hearts, rising, above the starter 3 hearts, with relief. Milestone 5: the walk through all twelve camps to Far Lookout, the old Low Hilltop river save reseated facing Windy Crest with its toll kept, road clearance on every leg, and the interactable count check now follows the layout rule (legs from 450 m get a fourth animal spawn, from 500 m a fourth fishing point). All Milestone 1-8 checks pass. **Device check pending:** the 12-camp painted map, the hill legs, and pacing with carried food.

**Implementation checkpoint — November 2, 2026, 17:44 build: Milestone 8 code complete (device round pending).** Installed on the nubia NX769J.
- **Item 2, region plan as data:** `Domain/Travel/RegionPlan.cs` holds all 24 regions (name, tier, three featured IDs, rolled camp count) from section 6.2.2. Tier 2-4 IDs are names only (no runtime loot, recipes or art); they follow the existing ID patterns (`material.`, `ingredient.`, `food.`, `animal.<species>.pelt/hide/feathers`). Checks: 24 regions x 3, 18 distinct per tier, 72 overall, camp counts inside each tier's range, the branch and pelt-family entry in every tier (tier three: turkey feathers), authored regions matching the plan, tier-one mission sources matching exactly, and only branch + pelt overlapping interaction rewards in tier one.
- **Item 5, route safety:** `RouteBudget.Simulate` walks a leg on the real stamina model (fixed-rate delivery, no reserve beyond the cap, standing still while food arrives on an empty meter). Checks: every authored leg and ferry landing is walkable from a full 3-heart meter with 1-heart juices without stopping; opening legs need no food and every leg above 3 hearts fails without food; three starter hearts last exactly 250 m; the 26-heart leg (1,300 m at 20 hearts/km) works with 12 hearts plus three 6-heart meals and finishes near 4 hearts, fails with two meals, and from a 6-heart start needs a fourth meal; speed does not change food need; a restart while fainted returns to camp still fainted with the loss neither rolled nor charged again. **Faint presentation** (`CollectionPrototype.Faint.cs`, `FollowCameraRig.Collapse`): after the loss is committed and saved, the view sinks to the ground with a tilt (1.2 s), the lost entries slip out as their item icons with quantities and fade, the view breathes under a translucent shade while hearts refill, then rises again (1.5 s, "Waking up..."); input stays locked until the wake ends and walking still needs a fresh upward swipe. A reload while fainted shows the collapsed view directly without replaying the loss; seated at a camp the view only slumps halfway. **Route information from the same data:** a parchment route card above the bag button. Seated: next camp, distance, heart cost, your hearts, carried food hearts and a verdict from the simulator ("No food needed", "OK: about N servings on the way", or a red "Not enough food"); it warns only and never locks departure or eats food. Walking: metres and hearts to the next camp. The camp map shows each camp's next leg (metres and hearts) or "River and ferry ahead". Hidden while the bag, a menu, the toll dialog, a harvest or a faint is showing.
- **Item 6, layouts:** Windward Hills complete (12 camps, see above). Streaming is structural: each scene builds only the previous, current and next legs, so region size does not grow the scene. Map space: with strictly alternating sides, same-side tabs stay apart up to the 20-camp maximum, so the sheet needs no scrolling (checked for all 24 planned counts). Tier-three turkey feathers are recorded as the confirmed shared source.
- **Mission lady reminder:** done; she appears only by the `LadyPresent` rule (first selection, claimable completion, next selection).
- **Automated:** Milestone 1-8 suites pass (M1 34, M2 57, M3 13, M4 persistence 26, M5 79, M6 23, M7 28, M8 67).

**Session checkpoint (November 2, 2026, end of day; resume testing from here).** Installed on the phone: the **23:28 development build**, with the save kept (adds statistics tracking to the 23:07 build). All automated suites pass: M1 34, M2 57, M3 13, M4 persistence 26, M5 83, M6 23, M7 31, M8 74.

Done today, beyond the Milestone 8 code:
- debug AUTO SORT
- the faceted character redesign: player, camp NPCs with a hair and beard roll, the mission lady, the ferryman, seven storefront shopkeepers and the balloon merchant
- surface detail textures
- a reshaped trail pine trunk
- the river ferry art
- all seven storefronts (art only)
- the camp-ground fix
- the hill-leg fix (level camp approaches, road on the ground)
- the map region list with travel arrows
- mastery counted at craft start

**Device checks still pending, in addition to the seven Milestone 8 items below:**
1. **AUTO SORT:**
   - fills stacks to 1,000 and repacks the bag
   - refuses while an item is held
   - leaves pending items alone
2. **New characters in camps:**
   - traders show varied hair styles, and men a beard or none
   - skin tones and outfits are right
   - the mission lady and the ferryman look right
   - the equipment-panel avatar is the new player
3. **Surface detail (first impressions approved):** watch the frame rate while walking and look for flicker or textures that look wrong.
4. **Ferry:** dock, raft and cable at the end of a region and at the far-bank landing.
5. **Camp ground (Windward Hills):** clearings sit on the ground at every camp, with no floating edges.
6. **Hill legs (23:02 build):**
   - the road dips no more than a little at camps (no V)
   - the road meets the ground on climbs and descents
   - no floating trees or rabbits on slopes
   - a saved position mid-leg may shift a few metres, because leg shapes changed
7. **Map:**
   - tapping the region name opens the region list; rows have their region colours and thumbnails
   - tapping a row opens its camps
   - the right arrow travels to that region's first camp only once that camp has been reached
   - tapping the title returns to the camp view
   - the list's layout on the phone is unchecked
8. **Crafting mastery:** two back-to-back crafts of the same timed recipe; the second shows the higher level at once.

**Floating pickups fix (user, November 5, 2026: on the 23:28 build the resources looked like they floated, seen from a camp):**
- **Cause:** loose pickups lie 1.9 m from the road centre, just past the road's edge (1.7 m), but their model was rested 2 cm above the *road* height. The redrawn terrain sits below the road beside it (6 cm base gap plus the bend clearance, more on crests and at the camp clearings, which are levelled 0.1 m below the camp), so pickups hung up to 0.25 m above the ground. Pickups saved under the old leg shapes also kept their old height.
- **Fix:** a collider-free height lookup over the drawn terrain mesh (`Travel/TerrainHeightField.cs`, kept by `MovementPrototype.Ground`; a kept collider would block the touch raycasts). Each pickup's model rests 2 cm above the lowest ground under its footprint, as trees and rabbits do. This covers trail drops, harvest rewards, player-dropped items and saved positions, whatever height was saved. Scenes without a styled terrain keep the old road-height rule.
- **New Milestone 5 check:** on every leg of every region, with the camp clearings, the lookup matches a real collider at both pickup spots every 2 m and never misses the ground. All Milestone 1-8 suites pass. **Device (user, November 5, 2026; 21:24 build):** confirmed at the problem camp, the previous camp and the leg between them; pickups sit on the ground and touching them works. The rest of the region is still to walk.

**Loose-resource surface detail, review (November 5, 2026):** another tool implemented step A of the item surface-detail plan (`Docs/PROJECT_HANDOFF.md#loose-resources-2026-11-05`): detail on all 18 tier-one loose resources, the 32-tile atlas (alpha now type x 8 + 4), rounder fruit, a hide-shaped pelt, rounder mushroom caps, an isolated Mushrooms palette and the real mushrooms in the region 4 preview. Testing was left to Claude:
- **Passed:** all Milestone 1-8 suites. All 25 detail palettes use the new encoding; none are left in the old format. Palettes import uncompressed, point-filtered and without mipmaps, so the type survives on Android. The storefront street renders the same as before the migration (under 0.002% of pixels differ, by at most 12 of 255 levels). All six resource sheets were recaptured into `Docs/Art/Region0N`, and all 18 bag pictures render their models.
- **Fixed:** every mushroom stem had a red side. The shared cylinder helper shades every third side one palette slot darker, and the stem slot 27 (cream) became 26 (red). The stems now use slot 28, which stays cream; detail covers slots 27-28. Only Mushrooms was regenerated. `RoastedMushrooms` uses the same stem code but is only rewritten by the full crafted-items generator, so it keeps the red side until that generator runs.
- **Notes for the user:** the pelt's bag picture is a thin edge-on sliver, as before, because it is a front view of a flat hide; a tilt like the plank's (`ResourceVisualTuning.IconRotation`) would show its new shape. The detail atlas is now 2048x1024 and uncompressed, about 11 MB of texture memory with mipmaps (double before); a single-channel format would cut it to a quarter if the phone memory check asks for it.
- **Device results (user, November 5, 2026; 21:24 build installed, save kept):** partial round, covering the camp where pickups floated, the previous camp and the leg between them. Passed so far: resources rest on the ground; resource looks are right on the trail and in the bag; pickups work; no noticeable frame-rate drop yet. **Still to check:** the rest of the region (other legs and camps, hill legs), region 4 mushrooms and the pelt up close, frame rate with a full open bag and many textured items, and whether the other scenery looks unchanged.

**Device report and fixes (user, November 7, 2026; 20:15 build installed, save kept):**
- **Stuck at Stone Gate, "Invalid session header" (fixed):** the phone save held 1,012 item entries, 979 of them uncollected loot from the 19 walked legs (45-70 piles each). Save validation caps a save at 1,024 entries, so the next leg's pickups pushed it over: every later save failed and the arrival was undone, sending the player back to Stone Gate. Those leftovers can never be seen again (only the current leg is shown, and returning to a leg's start camp re-rolls it), so arriving on a leg (every drop roll) and loading a save now drop the world items of every other leg (`SpatialInventory.RemoveWorldItemsExcept`). The existing save is cleaned on its next load. New Milestone 5 checks: leftovers of other legs go, carried items stay; a simulated 25-leg walk peaks at 70 world entries (1,750 before the fix).
- **Equipment swaps kept emptying pockets and hooks (fixed):** equipping over an occupied slot took the old item off first, which re-applied the default gear's pocket/hook counts (usually zero) and pushed their contents into the bag or overflow. A swap now changes capacity once, with the new item's counts: contents stay when the new count is equal or greater, and only the contents of removed slots move. New equipment checks for equal, greater and fewer slots.
- **Auto-eat (changed):** carried prepared food is eaten at **0.1 hearts**, without stopping the walker (`StaminaState.AutoEatThreshold`, retried at most once a second while no food is carried). Reaching zero with no prepared food still stops the player and leads to fainting. New checks for the threshold, camp exclusion and food already being delivered.
- All Milestone 1-8 checks pass (equipment and stamina model: 61). **Device (user, November 7, 2026; 20:15 build): all three checks passed** (past Stone Gate with no more "Invalid session header", gear swaps keep pocket/hook contents, auto-eat at 0.1 hearts without stopping) and the player reached the end of region 2 (Windward Hills). Mission catalog 3 needs **no save reset**: the save's catalog-2 claims and active mission migrate, and each branch continues with its first unclaimed mission in the new order.
- **Milestone 8 device round closed (user, November 7, 2026):** all outstanding device checks passed (AUTO SORT, characters and equipment avatar, surface detail and frame rate, ferry, Windward Hills camp ground and hill legs, map region list, back-to-back crafting mastery, item models and tilted bag pictures). **Deferred:** the faceted rabbit's hop, graze and look-around animations are reviewed as play continues into the regions after 2, where rabbits still appear. Milestone 8 is complete; committed on main as requested.

**Return button everywhere (user, November 5, 2026):** every menu and interaction closes with the **return icon button in the bag button's place**, never a CLOSE, X or PUT AWAY text button. Implemented:
- **Bag, equipment and crafting** (shown with the bag): the bag button shows the return icon whenever the bag is open and closes the bag together with its menu. It stays on top of the equipment panel, over its empty lower-left corner. The equipment X and CLOSE buttons are gone, and TAKE OFF is centred on the bottom row.
- **Item details:** no CLOSE button. The return button closes the details first; they still close by themselves after a while. EAT and EQUIP are centred.
- **Map and mission papers:** one shared return button (`ShowScreenReturn`) in the bag button's place hides the bag and user-menu buttons and disappears with its menu, however the menu closes. The papers show it only when no mission can be chosen (it replaces PUT AWAY); otherwise they still close only by choosing.
- **Interactions (harvest screens):** the RETURN text button is replaced by the return square in the closed bag button's place.
- **Rule for new menus:** use the bag button (menus shown with the bag) or `ShowScreenReturn` (full-screen menus); no text close buttons.
- **Unchanged, a choice rather than a close:** the ferry toll dialog keeps PAY / NOT NOW.

**Equipment menu cleanup and fitted tier-one wearables (user, November 5, 2026):**
- **Buttons removed:** the camp world's equipment menu has no EXPIRE SLOT, EXPIRE ALL or TAKE OFF. Gear is equipped and taken off by dragging between the bag and the slots, expiry follows real wear, and the return button closes the menu. The avatar stage extends into the freed space. The older test scenes without crafting keep these buttons for their checks.
- **Avatar showed no worn clothing:** the equipment avatar only placed the hat and backpack item models at their anchors; shirts, pants and boots had been left for the equipment redesign, so equipping the tier-one pants changed nothing.
- **Fixed with the first fitted set (user chose the tier-one set now):** hemp shirt (rolled sleeves, two chest pockets), hemp pants (belt with buckle and two tool loops) and rabbit-pelt boots (fur cuff) on both player bodies, built from the shared faceted parts in the standing pose and coloured like the crafted item models. Equipping swaps the shell in and hides the base tunic or trousers it replaces. Debug test gear borrows its slot's tier-one shell. Hat and backpack keep their item models for now. Details and the rebuild steps are in `Docs/Art/Characters/PlayerDefault/player-avatar-designs.txt`; the review sheet is `tier1-wearables.png`.
- **New Milestone 5 check:** every tier-one shirt, pants and boots item (and its debug stand-in) has a fitted shell with meshes on both bodies.
- **Still to do (equipment redesign):** a fitted hat that hides the hair under it, a backpack fitted to the back, and later tiers' sets.

**Tier-one item art pass (another tool, November 5, 2026; review and integration by Claude the same day; 23:42 build installed, save kept):** `Docs/PROJECT_HANDOFF.md#tier-one-items`.
- **Delivered:**
  - 71 redesigned tier-one item meshes on their existing paths and GUIDs (harvest drops, materials, tools, wearable displays, foods, Misc and store inputs), each fitted to its previous size, so footprints, stacks and trail sizes are unchanged
  - 25 semantic surface profiles, so unrelated items that share a colour no longer share a texture
  - a material pass on the tier-one merchant items and the tier-one mystery box
  - the **fitted rabbit-pelt hat and starter backpack** for both bodies, with eight hat-compatible hair variants (`Docs/Art/Characters/Tier01Accessories`)
- **Review:** the foods have real rims and toppings, the fish keep their species colours, and tools, gear and toys read clearly.
- **Integrated by Claude:**
  - **Avatar binding:** `EquipmentAvatarView` attaches the fitted hat at `HatAnchor` and the backpack at `BackpackAnchor` (identity transform, from `Tier01AccessoriesArtMap.json`). While the hat is worn, the saved hairstyle shows as its hat-compatible variant in the saved colour, and the original hair returns when the hat comes off; the saved appearance never changes. Items without fitted art keep the earlier item-model attachment.
  - **Bag pictures:** the raw rabbit pelt, cured and fine rabbit pelt, rabbit leather, fine rabbit leather and limestone tiles showed only an edge, so they now use the plank's tilt (presentation only; footprints unchanged).
  - **Pelt and leather footprints (user, November 6, 2026):** with the tilt, the raw rabbit pelt draws almost square (about 1.1:1) and the four processed hides about 1.4:1, so each sat small inside its old strip footprint. The raw rabbit pelt is now **480 x 480** (apple-like, was 850 x 300) and cured/fine rabbit pelt and rabbit/fine rabbit leather are **600 x 420** (were 900 x 300). Trail sizes are unchanged (`ResourceVisualTuning.TrailScale` compensates); saved bag items that no longer fit move to overflow through the existing footprint migration. All Milestone 1-8 suites pass; not yet checked on the phone.
  - **Fishing ponds on hills (user, November 6, 2026):** on the leg after Gorse Saddle the fishing pond looked half buried during fishing: it was seated on the lowest ground within 0.45 m of its centre, but the ground fell 1-1.6 m across its 3.1 x 2.4 m footprint. The terrain now levels a disc under every shown leg's fishing pond (6.2 m flat, fading out by 9.5 m, road band untouched), the same way camp clearings are levelled, so each pond rests flat with its whole rim visible. Probe: every pond on segments 02.05/02.06 now has 0.00 m ground spread under it; before/after render checked. All Milestone 1-8 suites pass; not yet checked on the phone.
  - **New Milestone 5 check:** the hat and backpack have fitted meshes on both bodies, and hat hair exists for all four styles on both bodies. All Milestone 1-8 suites pass.
- **Open:**
  - in the art sheet's side views the fitted **backpack hangs a few centimetres behind the back**; it should rest against the shirt (an art fix in its generator)
  - the older balloon special worn geometry is not refitted to the faceted bodies (a separate art batch)
  - nothing is skinned or animated yet
- **Device check pending:**
  - the equipment avatar with the hat and backpack, on both bodies and all four hairstyles
  - the hair returning when the hat comes off
  - the new item models on the trail and in the bag (with the icon cache refreshed)
  - the six tilted bag pictures
  - frame time with a full bag

**Decisions recorded at the end of the session (user, November 2, 2026):**
- **Shop street order:** delegated to Claude and set as warehouse, bakery, ranch, farm, fishery, mine, tailor-material shop, lumber field (section 13.4).
- **Shops upgrade** when the player reaches the next tier's regions. Mostly visual (section 13.4).
- **Tutorial outline confirmed:** gathering on a stretch before the first camp; the first mission received automatically with an explanation of mission tasks; crafting shown after the first mission is completed, with enough materials granted for one item if needed; and short keeper dialogs introducing each shop on the first settlement visit (testing-only HUD notes above).
- **Statistics page:** pending until phase 1 (24 regions) is complete, and likely to be data-rich. The counters are tracked in the save from the 23:28 build on (see the testing-only HUD notes).
- **Lumber field:** a street entrance (gated fence) leading to its own area with the tier sections of massive trees, unlike the shops' counter views (section 13.4).

**Still open:** nothing blocking. The tutorial and the shop upgrades are designs for later milestones, not current work.

**Next work, in order:**
1. Finish the device round above.
2. **Commit Milestone 8** (the user asked for a commit after the Milestone 8 round). It is all uncommitted, including the untracked art folders, `Tools/Art` and `Docs/Art`. The revision-3 character sources are backed up only in the session scratchpad, so committing protects this work.
3. The rest of the debug cleanup and the crafting mastery tests.
4. The item surface-detail plan (pickups and drops first).
5. The new massive lumber-field trees.
6. The equipment redesign (fitted hats, shirts, pants, boots and backpacks on the faceted bodies).
7. The 8 house-level exteriors and the player-home interiors, organised as one set.

**Milestone 8 device test list (one round for everything since the 18:47 build of October 31):**
1. **G1 first:** RESET SAVE; at Dawn Meadow the papers offer only Gatherer G1; the other three tabs say "Opens after the first mission (G1 Pine branches)". After claiming G1, H1 opens; the region-two branches open after the region-one missions.
2. **Debug panel:** MIXED, FULL BAG, TREE FOOD, ADD SPARE GEAR and + TOOL KIT are gone; the equipment panel has no R4 TEST GEAR (EXPIRE SLOT / EXPIRE ALL on the top row). The remaining debug buttons still work.
3. **UNLOCK CAMPS:** at a camp, tap it, open the map: every camp of the current region is selectable and travel works; other regions stay as they were; it survives closing and reopening the app.
4. **Windward Hills:** cross the ferry; 12 camps (Dawn Hilltop ... Far Lookout) on alternating sides; hill legs look right (road above the terrain on the steeper legs out of Gorse Saddle and Weathervane); the 12-camp map is readable, with each tab's "Next leg ... m · ... hearts" line; the river and ferry come after Far Lookout. An old save at Low Hilltop facing the river is reseated there facing Windy Crest.
5. **Route card:** seated, the card shows the next leg and a verdict that matches reality (with food carried it says OK; with no food on a food leg it turns red); walking, metres and hearts to the next camp count down; it hides with the bag, menus, map, toll dialog and harvests.
6. **Faint animations:** with TEST EMPTY and no prepared food on a leg, the view sinks, the 2-4 lost entries slip out with their icons and amounts, the shade and "Recovering..." text show, then "Waking up..." and the view rises; touches do nothing until it stands, and walking needs a new upward swipe. Close the app while fainted and reopen: back at the camp, still recovering, the same loss count, nothing else lost, the view slumped halfway.
7. **Pacing with carried food:** walk a few Windward legs eating juices or roasted rabbit; legs take about 4-6 hearts as the card says.

**Debug AUTO SORT (user, November 2, 2026):** a camp-world debug button (the old TREE FOOD spot) that tidies the backpack until the gravity- and collision-controlled bag exists. It tops up stacks of the same ready item to 1,000, keeps every unit and quality, leaves pending (still crafting) items as they are, and repacks the bag largest-first, grouped by category and item, with the deterministic bottom-left placement. It is all or nothing: it refuses while an item is held, and if the repacked layout would not fit nothing moves. Pockets, hooks and overflow are untouched. Four Milestone 8 checks cover it. Device check pending (with the next build).

**Testing-only HUD text, to remove once testing is over (user decision, November 2, 2026):** the route card above the bag (next camp, remaining distance, heart cost, verdict), the action/feedback messages, and all the text along the upper edge (title, status lines) are testing aids. The camp map will show **only the camp name** (no leg length or heart cost). In the finished game the **hearts are the only stamina measure, shown visually, with no text or numbers**. The other values (distance travelled and similar) move to the player info as statistics. **The statistics page stays a pending decision until the whole of phase 1 (24 regions) is complete** (user, November 2, 2026). If it is added, it will most likely hold a lot of data: distance travelled, items gathered per item, interaction counts per interactable type (trees chopped, animals hunted, fish caught), items crafted per recipe, current tier, house level, and historic totals plus current balances of cash and gold. **Tracking implemented (user, November 2, 2026; 23:28 build):** the save now keeps `PlayerStatistics` (section `statistics` in the session snapshot; older saves start at zero): walked trail distance, units gathered per item ID (only what a pickup actually took), units and craft actions per recipe, completed harvests per interactable kind (tree, animal, fishing), cash and gold earned and spent (debug grants and tolls for now; trading, the warehouse and shops add to them when they exist), and faints. Current tier, house level, camps reached, missions claimed and current balances are derived from their own state and not duplicated. No page shows the data yet; three Milestone 8 checks cover the counters, the save round trip and older saves. Sources to hook later: the warehouse and trading (cash/gold earned), shops (spent), the house level, and pet rewards. Until testing ends, all of this stays as is. The route simulator and its checks stay, because they validate the leg budgets.

**Tutorial (user, November 2, 2026; outline confirmed, not built yet):** a tutorial stretch comes **before the first camp**. The 250 m tutorial leg in section 4 was only an earlier example; the stretch's length is set when it is built.
1. **Gathering:** the tutorial stretch teaches the main gathering mechanic: swiping pickups into the bag, holding and dragging, and the speed gesture with stop and resume.
2. **First mission, automatic:** the first mission (G1, Pine branches) is received automatically during the stretch, with a short explanation of how mission tasks work. Carried items count toward the task and are claimed with the mission lady. The first mission paper does not need to be picked by hand.
3. **Crafting, after the first mission is completed:** the crafting mechanic is shown with the recipe the mission unlocks. If the player does not carry enough materials, the tutorial **grants exactly enough for one item**, so the first craft can always be done. One craft only, not a stockpile.
4. **Settlement:** on the first visit, each shop gets a **small tutorial dialog** from its keeper that introduces what it does. The warehouse, bakery, ranch, farm, fishery, mine, tailor-material shop and lumber field each show theirs once, at first entry, and can be skipped.
- *Delegated defaults (proposals):*
  - Each step runs once and is saved, so a reload never repeats it.
  - Dialogs are short (one or two lines) and can be skipped.
  - The tutorial grant is recorded so it can never be claimed twice.
  - Debug tools keep working during the tutorial.
  - The tutorial counts as region one: its pickups are Dawn Plains items.

**Character art direction, faceted study (user, November 2, 2026):** the user approved a new style for the player and NPC avatars. It is based on user-supplied style references (`Docs/Art/Characters/References`), which are used for style only; no characters or art are copied. The style: flat facets instead of smoothed spheres and tubes; a boxy head with a squared jaw, slab brows, small dark eyes and a wedge nose; flat swept hair locks; natural slim proportions; and clothing as separate shells (collars, hems, cuffs). The source is `Tools/Art/faceted_character_kit.py` and `generate_faceted_avatar_study.py`, which output to `Assets/_Game/Art/Characters/FacetedStudy`. Review sheets are rendered by **The Trail Walker > Art > Faceted Avatar Study** into `Docs/Art/Characters/FacetedStudy`. The male study figure is about 2,000 triangles; the current avatar is about 11,000. User feedback applied: the chin is shorter (the head read too tall); **facial hair is either a full beard in the hair colour or none (no stubble)**; the hips and legs are fuller (the lower body read too thin on male figures). **Camp NPCs: each NPC rolls its own details (beard or none, hairstyle, skin tone, hair colour) from the base male and female designs.** Equipment items (hats, shirts, pants, boots, backpacks) will be redesigned to match, as shells fitted to the same body landmarks. Next steps: the female base and the remaining hairstyles; one full equipment set as a sample; the seated NPC version with the random appearance roll; then replacing the live player, NPC and equipment-panel avatars. The live avatars stay unchanged until then.

**Faceted redesign applied (user, November 2, 2026):** the user approved the study ("the general design is good") and noted that the beard floated slightly in front of the face; the beard now sits flush on the jaw. The new style is now live for the **default player avatars** (male and female; four hair styles Crop, Sweep, Bob and Tied on both; six skin tones; five hair colours; the equipment-panel avatar uses them), the **random camp NPCs**, the **mission lady** (four pose studies) and the **ferryman**. Every character shares one skeleton and parts library, `Tools/Art/faceted_people.py`: hips pivot 0.88 m standing or 0.50 m seated, torso +0.06, head +0.49, upper arm 0.29, forearm 0.25, thigh 0.38, shin 0.40, ankle 0.10. All character importers now use flat normals. **Camp NPC roll:** besides body, skin, hair colour, clothing set, backpack and idle phase, each trader now rolls a **hair style (any of the four)** and, **for men only, a full beard or none**. This comes from its own hash stream, so the earlier fields keep their values. It is checked in Milestone 5. Every NPC prefab carries all four hair styles and the beard, and the camp shows the rolled ones. Rebuild: run the generators (`generate_player_avatar_meshes.py`, `generate_camp_npc_meshes.py`, `generate_special_npc_meshes.py`), then **The Trail Walker > Art > Build Default Player Avatars / Rebuild Common Camp NPC Meshes / Rebuild Mission Lady and Ferryman Meshes**, each with its Capture menu. Review sheets are in `Docs/Art/Characters/{PlayerDefault,CampCommon,CampSpecial}`. Revision-3 sources are backed up outside the project. Hats and backpacks in the equipment panel are still the item display models; fitted wearables come with the equipment redesign.

**Speed caps raised 1.5x (user, October 31, 2026):** starter 1.5 m/s (was 1.0), +0.75 m/s per boots rank (was 0.5), rank-four maximum 4.5 m/s (was 3.0); section 4 table updated. Special boots keep the halfway rule (0.05 m/s steps, rounded down): 2.25 / 2.6 / 3.35 / 4.1 m/s (section 14.1). Stamina drain is per metre, so heart costs per leg are unchanged; legs simply take less time. Checks updated (equipment capabilities, migration clamp to the starter cap).

**Mission region gate (user, October 31, 2026):** missions no longer show as locked because their items come from a region the player has not reached. They open by **mission region**: region-one missions first (in the prototype G1, H1, G2); once no region-one mission is left to offer, the **region-two missions open even before the player reaches region two** (L1 Flint, E1 Hemp, then the branches that follow), and so on. The gate only moves forward and is saved (`missions.region`), so **there is always a mission to choose** (and, with the papers closing only on a selection, always an active mission) while any remain. Recipe, tool and settlement dependencies still lock individual missions with their reason (for example G3 needs the Pine bird sculpture recipe; E8 needs settlement Tea herbs). Locked text for later regions: "Opens after the region N missions". This replaces the earlier source-discovery lock in section 11 for ordinary progression.

**Reminder (October 31, 2026):** implement the mission lady appearance rule in section 12.1 (starting camp for the first selection, then only at the camp where the active mission completes, for the recipe hand-off and next selection); remove her every-camp test placement.

Dependencies: 7.

1. Add four branch definitions, the twelve first-tier missions and cross-branch prerequisites. Keep one active mission; no old Materials/Tools/Food/etc. phase gate.
2. Build source data for three featured items/region and 18/tier, with map icons and separate interactables. Validate all 24 slots as data; use only short representative scenes initially.
3. Verify automatic mission consumption/claims, ten starter units and proposed 50/100/up-to-150 later quantities, four clothing assignments, worn-gear protection, and x1-only tools. Test competition between Normal-tool mission contributions and Elite upgrade reservations.
4. Play the new region-one introduction: branches -> log unlock, apples -> juice unlock, accessible camp crafts and enough stamina before food; region two introduces Flint/Hemp fiber and longer legs requiring food.
5. Simulate lower-tier route budgets and the maximum 26-heart leg with twelve hearts plus ordinary food. Add faint/recovery/loss/wake animations and restart/reload checks; show HUD/map/departure information from the same data.
6. Validate camp layouts per the confirmed counts (tier one 10-14, tier two 12-16, tier three 14-18, tier four 16-20 camps per region, random within the range; section 6.2.2) with streaming; author one active-tier representative before scaling to full region content. Confirm tier-three feathers as the approved shared source.

**Exit check:** four independent branch sequences persist; first twelve rewards match the table and are achievable; G1/G2 unlock usable food in region one; inaccessible dependencies show locked branches rather than active impossible missions. Three map icons match loose spawns, 18 distinct IDs/tier and only permitted overlaps hold. Ten-unit missions leave 990; tool-instance missions/gear protection remain correct. No leg exceeds 26 hearts; opening legs work without food, advanced ones work with ordinary meals, and faint loss cannot reroll/double on reload.

### Milestone 9: Camp trading and wallets

**Reminder (October 31, 2026):** after the mechanics slice, schedule the item collector book (section 12.5): sections by type ordered by tier, 3D model viewer with rotation, regions of origin and unlocked recipes per item, saved per-item discovery.

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
2. Add ten-box accounting, gold settlement at 1 gold per 1,000 cash, and integer cash-remainder persistence.
3. Add optional bounded multi-box deposit after single-box correctness is proven.
4. Record inputs, outputs, and totals for later economy analysis.

**Exit check:** ten units from a full stack leave 990; no partial-box payout; each tenth completed box pays the expected gold once; reload during payout cannot repeat rewards; quantity/value multiplication cannot overflow.

### Milestone 13: Lumber field and long timers

**Massive-tree food addition:** every active wood tier uses section 7.1.1's required ingredient. Multiply its rolled bundles by 50 exactly once along with branches/logs; persist partial ingredient claims and expire them with that tree's existing reward cycle.

Dependencies: 12.

1. Build one tier with six independent trees in two rows and gold-tier unlock.
2. Implement section 8.2's massive-tree Normal/Elite action curves, +2 ceiling, Elite faster at/below own tier, Elite drop buff even at eligible +1/+2, x50 manifests, type piles, and 72-hour respawns. Use the section 8.2.1 cutting mechanic; finishing keeps the focused screen open, rewards spawn there for pickup, and only Return closes it. Tapping a felled tree that still has pending drops reopens that pickup screen with exactly the remaining piles (no re-roll), until they are collected or expire.
3. Add simulated time/expiry warnings, then expand only to four active wood tiers (24 tree records). Leave the other four tiers in the design backlog.

**Exit check:** all six timers are independent, x50 rewards remain full stacks, leaving/reentering preserves pending quantities, advancing time respawns only eligible trees and expires only their old rewards, and rendering stays capped.

### Milestone 14: Settlement shops and balloon merchant

**Reminders (November 2, 2026):** build the seven storefronts' tier looks (shops upgrade, mostly visually, when the player first reaches a tier-2, tier-3 and tier-4 region), and lay the street out in the confirmed order between the house plots (section 13.4). **Reminders (October 31, 2026):** place the balloon merchant at one camp in every 4-6 camps (section 14) and remove its every-camp placement test; implement the special gear list and stat/price rules in section 14.1; wire the mystery box art (open/close animation, colour set by the tier of the host camp's region).

Dependencies: 13.

1. Implement the three-offer shop service/Q5 policy. Configure fishery as exactly two distinct discovered fish + one random eligible exclusive seafood, all cash; one stack/1,000 units per slot/reset. Add proposed Shrimp/Crab/Squid/Lobster catalog without adding trail fish/loose entries.
2. Persist stock/slot roles, region filters, Q5 quotes, purchases/overflow. Reopen/reload never refreshes stock; valid settlement reset does. Buying ingredients never grants their recipe/mastery or player craft buffs.
3. Add balloon gear/backpack and six **typed** box entries; integrate the proposed 21 Food/5 Misc collectible recipes. Refresh unpurchased balloon offers on close/reopen, including special clothing; preserve paid manifests and main mission unlocks.
4. Add a configurable recipe category (proposed 5% of each random slot), discovered/ingredient-source eligibility, unlearned/pending-reservation exclusion, persisted duplicate fallback, and exact-once claim. Physical entries stay Q5; new recipe knowledge starts mastery one/zero crafts and takes no bag space. Exclusive raw ingredients remain fishery-only.

5. Author the seven premium bakery recipes/food definitions in section 9.4.4, exact source gates and proposed gold quotes. Assert the 1/2/2/2 distribution and execute section 9.4.4 missing-source cases at offer generation and before payment/unlock, including after reload/migration. Implement the 1% substitution per eligible premium slot, single-recipe stock, duplicate protection and atomic payment/unlock. Persist offer kind/quantity/price until settlement departure/return. Apply the 10% discount once to full premium food stacks.

**Bakery exit check:** seven unique recipe IDs; 33 optional collectibles total with 26 remaining in balloons. Check eligible/ineligible/learned recipes, same-stock duplicates, sold-out stock, full bags, interrupted purchases, reload and departure/return. Verify 5 gold/serving -> 50 gold/recipe and 4,500 gold/full stack; all seven price rows, exact ingredients/yields and ordinary crafted quality/mastery must agree.

**Exit check:** fishery always has two fish/one exclusive at Q5/cash and no reset budget can exceed 1,000; seafood source restrictions hold. Boxes keep six entries, eligible unlearned recipes grant once even with full bags, duplicates/exhausted pools fall back deterministically, and reload never rerolls. All physical rewards stay Q5; knowledge does not create a product/stack/quality/mastery-ten grant. Ordinary gear stays finite, conveyor quality unchanged, and no mandatory progression relies on box luck.

### Milestone 15: Pets, offline reconciliation, and remaining furniture

Dependencies: 14.

1. Add the level-three home-store pet purchase section, left-front pet-home selection, separate reward container, and Dog/Pig definitions first; then author the six-pet roster and previews.
2. Implement capped offline rewards, pet switching, and exact-once reconciliation.
3. Populate the remaining furniture levels using the evenly distributed buff table.
4. Add the neighbour houses per the November 2, 2026 rule in section 13: six plots (three left and three right of the shops and lumber field), the player picking one on first discovery, and five neighbours advancing one level each time their saved 1-3 region count runs out (rerolled after each upgrade). They share the player's 8 level exteriors; no simulation beyond the level.

**Exit check:** no pet access before level three in normal play, one offline interval pays once, switching pets cannot duplicate time, backward clock movement yields zero negative progress, first-purchase buffs remain capped.

### Milestone 16: Full mechanics and limits audit

Dependencies: 15; keep full-stack pickups and debug tools enabled.

1. Use the MechanicsTest profile with 1,000-unit trail pickups, debug grants, travel/rank selectors, simulated clock, and reproducible random seeds.
2. Run the limit matrix: instant material x1000, mastery 1/5/6/10, timer directions, hour-scale Wearable/Misc, rejected gear bulk, Normal/Elite action/drop differences, all nine upgrades, ten/50/100/150 mission quantities, mission-versus-upgrade tool ownership, +2/+3 boundaries, trading/regen, and massive manifests. Keep existing test values unchanged.
3. Capture sustained Android frame time/memory and repeat scene transitions, save/load, and suspension in development builds.
4. Fix ownership, UI readability, overflow, input conflicts, and data-validation defects. Record economy/pacing observations without changing quantities, costs, rates, or curves to improve balance.
5. Record each tested rule as verified, defective, or pending definition/implementation. Separate defects against defined rules from intentional debug shortcuts and undefined future content.

**Exit check:** defined systems pass mechanics/limits checks without unintended item/currency duplication or loss; results include device/build identifiers and a reproducible defect/backlog list. Passing continues the testing phase; it does not start natural-progression balancing or debug removal.

### Checkpoint: friends user-test APK (requested October 31, 2026)

**When:** once the first **four regions** and the **settlement** (between regions 4 and 5) exist with most mechanics completed, and **before authoring regions beyond 4**. Remind the user at that point; the user will send the APK to friends for testing and feedback.

**This build differs from the development builds (explicitly requested for this step, overriding the general "keep debug tools" rule for this one build only):**
- Final-product gathering: trail pickups give their natural quantities (1 unit per loose resource, per the confirmed final-product rule), not the 1,000-unit debug stacks; player-dropped stacks still keep their quantity and quality.
- No debug or testing modules: no fixture buttons (EMPTY, MIXED, FULL BAG, RESET SAVE, TEST HARVEST, TREE FOOD, ADD SPARE GEAR, CLAIM OVERFLOW, TEST EMPTY, R4 TEST GEAR, EXPIRE buttons, BACK TO CAMP, +100 CASH, +1 GOLD), no status/FPS debug text, no benchmark trigger, no Development Console.
- A fresh profile on first launch; earlier test saves are not carried over.
- Keep it a locally signed APK for sideloading (no store submission, bundle or publishing work).
- Prepare a short tester guide (how to install, what to try, how to report feedback) and keep the matching development build for reproducing reported issues.

Development builds keep all debug tools and 1,000-unit pickups before and after this checkpoint; regions 5+ continue in debug mode after the feedback round.

### Milestone 17: Base regional content and debug-assisted progression testing

Dependencies: mechanics-and-limits audit accepted for the implemented test scope; debug tools and 1,000-unit pickups remain enabled.

**Tier 2-4 content plan (proposal, user request November 5, 2026):** the recipes in section 9.6 (two equipment sets per tier, Beeswax, 21 new Food and 10 new Misc, varied fish dishes) and the missions in section 11.6 (8 tier-one bridge missions plus 68 / 69 / 67; every tier 2-4 recipe is a mission reward) are the content this milestone authors as each tier's regions are built. Implementation steps:
- explicit capability values on wearables (Set A sits between ranks)
- mission data for tiers 2-4 plus the bridge missions in tier one
- the Boar tusk harvest output
- the section 11.6.5 checks
- fitted art for every garment through the player wearables pipeline

Resolved by the user (November 5, 2026): fish are collection missions with varied dishes from tier two (section 9.6.5), and every recipe is placed as a mission; any later shop or box recipe must be premium (section 9.6.6). The tier-one bridge missions can be added earlier, with the rest of tier one, because they only use tier-one items.

**Tier-two environment kits built (November 6, 2026; art library, regions not yet authored):** 120 decorative meshes, 20 per region 7-12, with the tier-one kit contract (readable meshes, one 32-colour palette material per region, surface detail, separate shadow groups, catalog records).
- **Theme and naming:** follow sections 6.2 and 6.2.6. Every tree is a **birch** (tier two's wood), the beaver appears as scenery (lodge, dam), names use each region's landmark word, and no piece imitates a loose pickup.
- **Signature pieces by region:**
  - **Whispering Marsh:** boardwalk, beaver lodge, cattails, lily-pad water.
  - **Amber Riverbanks:** stepped river terrace, beaver dam, golden birches.
  - **Bluewater Basin:** pond basin, lily pond.
  - **Sunlit Uplands:** birch grove, sandstone outcrops, wooded hollow.
  - **Mirror Lakes:** islands and mirror water.
  - **Frostveil Taiga:** snow-capped birches and rocks, snow banks, ice-edge water.
- **New category "Structure":** the boardwalk, lodge and dam are placed by route authoring only and are never scattered.
- **Scenery lookup:** `SceneryScatter.Kit` now also searches the Tier02 library, so regions 7-12 are dressed from their own kit when they are built.
- **Source and output:** `Tools/Art/generate_tier02_environment_kits.py`; notes in `Docs/Art/Environment/Tier02/environment-library-notes.txt`; renders in `Docs/Art/Environment/Tier02/Region07..12`.
- **Checks:** new Milestone 5 check; all Milestone 1-8 suites pass.
- **Not done:** region 7-12 routes, camps and device checks.

**Tier-two camp props (November 6, 2026):** tier-two camps (regions 7-12) use the existing **BirchLogSeat** and a new **burning birch campfire**. Tier-one camps keep the static burned-wood fire.
- **The fire:** a teepee of birch logs (white bark with dark birch marks, charring to glowing tips), an ember bed, a river-stone ring, and split logs waiting as fuel.
- **Flames:** layered faceted flames, opaque, drawn unlit without shadows, with a light flicker (`CampfireFlicker`). No particles or lights.
- **Selection:** `CampAssembly.Campfire` now picks the fire by region from the camp prop catalog's new `Campfires` list, the same way seats are picked.
- **Source and notes:** `Tools/Art/generate_tier02_camp_meshes.py`; menus **Build / Capture Tier 2 Camp**; notes and renders in `Docs/Art/Environment/CampProps/Tier02`.
- **Checks:** new Milestone 5 check; all suites pass.
- **Device check pending:** when the first tier-two camp exists.

**Tier-two loose resources designed (November 6, 2026; art only, no item definitions yet):** 18 meshes, three per region 7-12, for the region IDs in `RegionPlan.cs`. They use the tier-one loose-resource contract: per-region 16-colour palette with surface detail, flat normals, prefabs in `Resources/Region07..12`.
- **Designs (each distinct from its tier-one neighbour):**
  - **Whispering Marsh:** birch branch with lenticels around the bark, round-stretched beaver pelt, blackberry drupelet clusters.
  - **Amber Riverbanks:** tied flax strands, copper ore with green bloom and nuggets, carrots.
  - **Bluewater Basin:** long-awned barley, quartz points, crowned blueberries.
  - **Sunlit Uplands:** strata sandstone, warty truffle with a cut face, flax-seed heap.
  - **Mirror Lakes:** drooping rice panicle, ribbed tomatoes, two-faced honeycomb.
  - **Frostveil Taiga:** tin ore with dark crystals, yellow-ochre clods, tied cattail fibre with seed heads.
- **Size:** 108-1,240 triangles each.
- **Source and notes:** `Tools/Art/generate_tier02_loose_resources.py`; `Assets/_Game/Editor/Tier02ResourceAssets.cs` (importer plus the menus **Rebuild Tier 2 Resource Prefabs / Capture Tier 2 Resource Previews**); notes in `Docs/Art/Tier02/loose-resource-designs.txt`; renders in `Docs/Art/Region07..12/resource-meshes-unity-v1.png`.
- **Checks:** new Milestone 5 check ("three designed tier-two loose-resource prefabs per region"); Milestone 5 passes.
- **Pending, with tier-two content:**
  - item definitions and the Region07+ pickup visual binding
  - bag footprints, taken from each region's `ResourceManifest.json`
  - the beaver pelt joining the tilted-icon set
  - spawn tables
  - device checks

**Tier-two batch 25 delivered (November 7, 2026; passive art):** Honey-roast duck, Duck blueberry rice now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 866 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-25). Progress 25/45 art batches; source coverage 78/82, 4 designs remaining. Batch 25 completed for this request under the established one-batch default. Evidence and handoff are updated; batch 26 is the next planned packet.

**Tier-two batch 24 delivered (November 7, 2026; passive art):** Painted birch bird, Fishing charm, Beeswax candle, Sandstone owl now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 862 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-24). Progress 24/45 art batches; source coverage 76/82, 6 designs remaining. Batch 24 completed for this request under the established one-batch default. Evidence and handoff are updated; batch 25 is the next planned packet.

**Tier-two batch 23 delivered (November 7, 2026; passive art):** Copper pendant, Birch beaver sculpture, Beaver toy, Lavender pouch now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 858 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-23). Progress 23/45 art batches; source coverage 72/82, 10 designs remaining. Batch 23 completed for this request under the established one-batch default. Evidence and handoff are updated; batch 24 is the next planned packet.

**Tier-two batch 22 delivered (November 7, 2026; passive art):** Pike barley cakes, Walleye-barley soup, Carp rice bowl, Pike rice bowl, Catfish-tomato stew now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 853 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-22). Progress 22/45 art batches; source coverage 68/82, 14 designs remaining. Batch 22 completed for this request under the established one-batch default. Evidence and handoff are updated; batch 23 is the next planned packet.

**Tier-two batch 21 delivered (November 7, 2026; passive art):** Blackberry-glazed beaver, Beaver-carrot stew, Rabbit barley soup, Beaver mushroom pot, Creamy truffle-barley pot now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 848 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-21). Progress 21/45 art batches; source coverage 63/82, 19 designs remaining. This continuation request was handled as the next bounded packet, batch 21, following the established one-batch default. Batch 21 is complete; batch 22 is the next planned packet.

**Tier-two batch 20 delivered (November 7, 2026; passive art):** Bottled honey tea, Carrot soup, Barley bread, Tomato rice bowl, Honey-glazed carrots now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 791 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-20). Progress 20/45 art batches; source coverage 58/82, 24 designs remaining. User authorized batch 20 only. Work stopped after completing this batch and updating the handoff; do not start batch 21 until requested.

**Tier-two batch 19 delivered (November 7, 2026; passive art):** Blackberry juice, Birch sap milk drink, Blueberry jam, Honey herbal tea, Honey oat drink now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 786 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-19). Progress 19/45 art batches; source coverage 53/82, 29 designs remaining. Stopped after batch 19 at the user's request; batch 20 has not started. Resume only when requested.

**Tier-two batch 18 delivered (November 7, 2026; passive art):** Taiga frame pack now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 783 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-18). Progress 18/45 art batches; source coverage 48/82, 34 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 17 delivered (November 7, 2026; passive art):** Taiga waxed shirt, Taiga trousers now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 777 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-17). Progress 17/45 art batches; source coverage 47/82, 35 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 16 delivered (November 7, 2026; passive art):** Taiga fur hat, Taiga boots now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 771 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-16). Progress 16/45 art batches; source coverage 45/82, 37 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 15 delivered (November 7, 2026; passive art):** Marsh pack now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 768 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-15). Progress 15/45 art batches; source coverage 43/82, 39 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 14 delivered (November 7, 2026; passive art):** Flax work shirt, Flax trousers now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 762 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-14). Progress 14/45 art batches; source coverage 42/82, 40 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 13 delivered (November 7, 2026; passive art):** Beaver-fur cap, Marsh boots now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 756 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-13). Progress 13/45 art batches; source coverage 40/82, 42 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 12 delivered (November 7, 2026; passive art):** Normal birch rod, Elite birch rod now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 754 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-12). Progress 12/45 art batches; source coverage 38/82, 44 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 11 delivered (November 7, 2026; passive art):** Normal birch slingshot, Elite birch slingshot now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 752 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-11). Progress 11/45 art batches; source coverage 36/82, 46 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 10 delivered (November 7, 2026; passive art):** Normal birch axe, Elite birch axe now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 750 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-10). Progress 10/45 art batches; source coverage 34/82, 48 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 09 delivered (November 7, 2026; passive art):** Barley flour, Flax oil, Beeswax now have packed native source, OBJ/palettes, passive Unity prefabs/materials, reviewed multiviews and fresh-import validation. 747 earlier meshes and shared atlas preserved. Canonical bindings and current gameplay/device tests remain pending; no recipes/stats/footprints/saves changed. [Single handoff](Docs/PROJECT_HANDOFF.md#batch-09). Progress 9/45 art batches; source coverage 32/82, 50 designs remaining. Continue sequentially until told to stop.

**Tier-two batch 08 delivered (November 7, 2026; passive Material art):** Glass, Glass bottle, Glass panels, Cut quartz and Sandstone tiles have packed Blender source, isolated OBJ/palettes, Unity art prefabs/materials and reviewed views. Native/fresh-import geometry and bounds checks and selective Build/Capture passed; all 742 earlier source meshes and the shared atlas were preserved. Glass uses the existing opaque faceted treatment; sandstone uses mineral grain. Canonical IDs, live binding and current gameplay/device tests remain with the other tool. Preserve earlier-tier Pine charcoal, exact bottle identity and panels as Material. No recipe/timer/value/footprint/save changes. [Single project handoff](Docs/PROJECT_HANDOFF.md#batch-08). Source-mesh coverage: 29/82, with 53 remaining; batches 01-08 art complete, 37 packets planned. Batch 09 (Barley flour, Flax oil, Beeswax) has not started.

**Tier-two batch 07 delivered (November 6, 2026; passive Material art):** Copper ingot, Copper fittings and Tin ingot have packed Blender source, isolated OBJ/palettes, Unity art prefabs/materials and reviewed views. Native/fresh-import geometry and bounds checks and selective Build/Capture passed; all 739 earlier source meshes and the shared atlas were preserved. Canonical IDs, live binding and current gameplay/device tests remain with the other tool. Both ingots retain earlier-tier Pine charcoal; fittings consume one Copper ingot for three outputs, despite the T2-L4 Copper ore unlock request. No recipe/timer/value/footprint/save changes. [Single project handoff](Docs/PROJECT_HANDOFF.md#batch-07). Source-mesh coverage: 24/82, with 58 remaining; batches 01-07 art complete, 38 packets planned. Batch 08 has not started.

**Tier-two batch 06 delivered (November 6, 2026; passive Material art):** Flax cloth, Flax cord, Flax stuffing, Fine flax cloth and Cattail cord have packed Blender source, isolated OBJ/palettes, Unity art prefabs/materials and reviewed views. Native/fresh-import geometry checks and selective Build/Capture passed; 734 earlier meshes and the shared atlas were preserved. Canonical IDs, live recipe binding and current gameplay/device tests remain with the other tool. T2-E4 unlocks stuffing by requesting cord, while the recipe consumes raw Flax fiber; Fine flax cloth retains its Hemp thread prerequisite. No recipe/timer/value/footprint/save changes. [Single project handoff](Docs/PROJECT_HANDOFF.md#batch-06). Source-mesh coverage: 21/82, with 61 remaining; batches 01-06 art complete, 39 packets planned. Batch 07 has not started.

**Tier-two batch 05 delivered (November 6, 2026; passive Material art):** Beaver leather, Cured beaver pelt, Fine beaver pelt and Fine beaver leather now have native Blender source, isolated OBJ/palettes, Unity art prefabs/materials and reviewed views. Native/fresh-import checks and selective Build/Capture passed. IDs, live refinements and output bindings remain pending with the other tool; existing recipes, timers, values, footprints, saves, earlier source meshes and the shared atlas were preserved. [Single project handoff](Docs/PROJECT_HANDOFF.md#batch-05). Recipe-output source-mesh coverage is 16/82, with 66 remaining; batches 01-05 are art complete and 40 packets remain planned. Batch 06 has not started.

**Tier-two batch 04 delivered (November 6, 2026; passive Material art):** Birch plank, Birch beam, Birch charcoal and Birch panel now have native Blender source, isolated OBJ/palettes, Unity art prefabs/materials and reviewed views. Native/fresh-import checks and selective Build/Capture passed. Canonical item/recipe IDs and live bindings remain pending with the other tool; existing recipes, timers, values, footprints, saves, earlier source meshes and the shared atlas were preserved. [Single project handoff](Docs/PROJECT_HANDOFF.md#batch-04). Current recipe-output mesh coverage is 12/82, with 70 remaining; batches 01-04 are art complete and 41 packets remain planned. Batch 05 has not started.

**Tier-two batch 03 delivered (November 6, 2026; passive art):** Lavender, raw Crab and Duck meat now have native Blender source, isolated OBJ/palette assets, Unity art prefabs/materials and reviewed previews. Native/fresh-import checks and selective Build/Capture passed. Farm/fishery/ranch source gates and live bindings remain pending with the other tool; only Duck meat has the authored design ID `ranch.duck_meat`, not a newly registered runtime item. Existing meshes, atlas, recipes, stats, source logic, footprints and save data were preserved. Details: [single project handoff](Docs/PROJECT_HANDOFF.md#batch-03). Batches 01-03 are art complete; 42 packets remain planned. Batch 04 has not started.

**Tier-two batch 02 and consolidated handoff (November 6, 2026):** Common carp, Northern pike, Channel catfish and Walleye now have distinct passive raw-fish meshes, isolated palettes, native Blender source and Unity art prefabs/materials. Native/fresh-import checks and selective Unity Build/Capture passed; source IDs, live bindings and gameplay/device tests remain pending. All seven former text handoffs plus this delivery are maintained in [one project handoff](Docs/PROJECT_HANDOFF.md#batch-02), with historical dates and limitations preserved; original handoff files are routing pointers. The batch schedule marks 01/02 art complete and the other 43 packets planned. Batch 03 has not started. No recipes, stats, sources, footprints, save data, earlier source meshes or shared atlas changed.

**Tier-two batch 01 delivered (November 6, 2026; passive art only):** [45 restartable production batches](Docs/Art/Tier02/tier2-production-batches-2026-11-06.md) now bound the work for daily usage. Batch 01 supplies Birch log, Birch sap and Raw beaver meat with native Blender source, three OBJ/palette assets, three Unity prefabs/materials and reviewed previews. See [batch 01 handoff](Docs/PROJECT_HANDOFF.md#batch-01). Native/fresh-import geometry checks and selective Unity Build/Capture passed; runtime binding and phone/gameplay testing remain pending with the other tool. Birch sap retains its existing ID; log/meat IDs remain unresolved. Current recipe-output coverage is eight of 82 with source meshes, leaving 74 to design. Batch 02 (four raw fish) has not started.

**Remaining tier-two design preparation (November 6, 2026; inventory/planning only):** [remaining item and shop design plan](Docs/Art/Tier02/remaining-tier2-design-plan-2026-11-06.md), with a reproducible mesh inventory and output-by-output coverage. Source inspection found 18 loose-resource meshes with prefabs, 120 regional scenery meshes, seven existing optional output meshes and five special merchant items with fifteen item/worn representations. The 68 main-mission output designs comprise 25 Materials, six Tools, ten Wearables, nineteen Food and eight Misc; no dedicated output meshes were found for those names. Including the separate truffle side recipe, eleven box recipes and two bakery recipes gives 82 output targets, seven with existing meshes and 75 still needing designs. Raw harvest/store inputs, twelve house-level-3/4 furniture types and common companion art are separately planned; furniture-study proxies and merchant gear do not complete crafted items. Preserve existing assets/GUIDs, exact identities and all current gameplay values. Stale exclusive-art hearts, Roasted crab premium eligibility, appended T2-G21/T2-H13 execution positions and the truffle side-mission identity require the other tool's logic review. No art/runtime changes, Unity tests or device validation are claimed by this preparation; pet-home model work stays deferred per the session-close decision.

**Session close (November 6, 2026):** the tier-two art library for regions 7-12 now has three parts: environment kits, the camp (birch fire and seats), and the loose resources. Regions 7-12 themselves (routes, camps, item data) are not authored yet.

Decisions taken at session close (user, November 6, 2026):
- **Part 1 barrel drinks:** left to Claude. Both proposals are adopted: bottles give 11-12 hearts, and filling recipes get the barrel exception (sections 9.0.1 and 9.4.5).
- **Gems:** every gem has a cut or polished version, used by the better recipes. Part 1 adds Polished lapis (mission T4-G22). Part 2 adds nine processed gems with missions, used by its keepsakes and Set B garments (Part 2 sections 10.3, 13.1, 14.1 and 15).
- **Pet home models:** deferred until Part 2 work is close; not needed now.
- **Spanish plan files:** updated with the plan decisions, without the testing and art-build checkpoints.

Still open: the fitted backpack hangs a few centimetres behind the back.

1. Validate the initial 24 regions' three-item tables (72 IDs) and four defined resource tiers using representative segments; incrementally author 10-24-camp test templates with reused kits/streaming. These regions are the base, not a final content ceiling. Track higher-tier materials/regions/recipes as pending definition and implementation; do not invent their rules or declare the product complete.
2. Populate the defined active-tier fixed-ID recipes needed for testing. Preserve exact ingredients, existing test quantities/timers, separate milk variants, and exact cup/bottle inputs. Furniture remains store-only; house upgrades use their defined named materials. Record missing definitions rather than inventing balance changes to close gaps.
3. Exercise recipe/mission/source dependencies, fishing pairs/habitats, <=26-heart legs, food/fainting, price calculations, wear, and stock using the current MechanicsTest profile. Record Normal-tool consumption, prior-tier refinement, house reserves, and x1 workload. Treat results as correctness/coverage evidence, not a final economy or pacing assessment.
4. Keep current price references, curves, costs, drops, timers, quotas, and mastery/pacing values unchanged. Log suspected balance issues and unfinished upper-tier dependencies for separately requested future work. Debug access to a gate must be recorded as a test bypass, not proof of an achievable natural progression route.
5. Start from a fresh test save with debug grants/selectors available. Use reproducible grants or targeted unlocks as needed to exercise every currently defined region, recipe, and house gate; record prerequisites, expected allowed/rejected transitions, and any bypass used. Test gate enforcement separately from bypass access. Mark undefined/unimplemented routes as pending. Do not require a grant-free playthrough or remove/disable debug tooling.

**Exit check:** every implemented base-scope gate has reproducible prerequisite/transition checks and recorded results; defined-content circular dependencies and missing sources/uses are identified as defects, and undefined upper-tier content is explicitly pending. Debug-assisted coverage does not certify natural progression, balance, final content completeness, or release readiness.

### Milestone 18: Prototype presentation, accessibility, and device testing

Dependencies: documented base-content test coverage and known limitations from Milestone 17, not a complete grant-free progression pass.

1. Improve prototype readability with consistent placeholders, quality icons, timers, resource hit targets, audio, and effects where they support testing. Final production art is not an exit requirement.
2. Test reduced motion, adjustable text/UI scale, alternative tap-to-collect, and readable trade details. Preserve the confirmed exclusive 15-second duration and Return cancellation rule.
3. Implement/test audio, graphics, controls, and language settings with stable localization keys.
4. Remove the testing-only HUD text (route card, action messages, upper-edge text, leg details on the map tabs) per the November 2, 2026 decision in Milestone 8: hearts stay as the only, purely visual, stamina measure; travel numbers go to player statistics (statistics page in the options still undecided). Build the tutorial per its confirmed outline (Milestone 8 notes, November 2, 2026): gathering on a stretch before the first camp, the automatic first mission with mission-task help, crafting after the first mission (one item's materials granted if short), and the one-time shop introduction dialogs in the settlement.
5. Test device aspect ratios, interruptions, low storage, save migration, graphics API compatibility, and sustained performance using local development builds with debug tools retained.
6. Package local development APKs, test scenarios, captures, and known-issue reports for repeatable device testing. Do not begin store SDK/signing preparation, release bundles, deployment, publishing, debug-grant removal, or rebalancing without an explicit future prompt requesting the relevant phase.

**Exit check:** a usable development test build, recoverable test saves, documented device coverage, and an updated defect/content-definition backlog. Continue debug/testing iterations; no automatic transition to balancing, final-product completion, or deployment.

### Immediate next implementation task

**Implemented (Claude, November 7, 2026, 16:21 build; not installed): steps 1-6 and the APK.**
- **Recipes:** `Tier01Recipes.json` (patched), `Tools/Art/tier01_recipe_specs.py` and the regenerated `Tier1Recipes.cs` now have 63 tier-one recipes: four new fish dishes and four retired roasts. Cured rabbit pelt takes Animal fat and Fine rabbit leather takes Nettle thread, and every unlock label names its mission. Regenerating also restored the L6-L8 labels that had been hand-edited in the C# file.
- **Retired roasts:** `RecipeDefinition.Retired` keeps the four roasts registered. The debug LEARN T1 RECIPES skips them.
- **Items:** the four dish items are added by hand to `Tier1CraftedItems.cs`, with their species' roast footprint and prefab as stand-ins. Its art generator was not rerun, because that would rebuild every mesh and overwrite the hand-tuned sizes.
- **Missions:** mission catalog 3 has 50 missions in the section 11.6.1.1 order (Hunter H8 before H6). E5 asks for Bone, with Pine branches kept for an E5 accepted under catalogs 1-2.
- **Save migration:** the save gains `claimCatalogs` and `activeCatalog` (schema still 3). Catalog 0-2 saves convert from `legacyClaims`/`activeLegacy`, and the mission UI and delivery use the per-state request.
- **Checks:** all Milestone 1-8 suites pass (M7 36 checks, M8 108). New checks cover the dishes and retired roasts, both new inputs, a pending roast batch surviving reload, the 27 gatherable IDs each having a mission, material uses, settlement-last order, G7/G8 in region 2, and migrations of the region-2 round shapes. Those shapes are E5 active with Pine branches delivered, nothing active, H4 active, the November 6 phone save with E6 active, and E8 claimed before its move, plus eight forged-data rejections and a JSON round trip.
- **APK:** `Builds/Android/TheTrailWalker-Milestone5-Development.apk`, SHA-256 `d8eced2c0828e060...`, 95.4 MB against 76.5 MB for the 12:08 build on the phone. The size difference was not investigated; it likely comes from art imported since November 6.
- **Waiting for the phone:** step 8, the backup, the pulled-save migration check and the install. Step 9, the dish art, waits for the other tool.

**Original plan (November 7, 2026): tier-one coverage and variety update.** It applies the November 7 design changes that touch tier one (sections 9.2, 9.4.1 and 11.6.1.1). Tiers 2-8 are not in code yet, so their revised dishes, requests and house deposits are picked up when those tiers are built. House-deposit changes wait for Milestone 11.

*Constraints:*
- The phone is disconnected.
- A region-2 test round is in progress on the current build and its save.
- So all work and checks happen in the Editor first. The APK is built but **not installed** until the phone is back and the tester is at a safe stopping point.
- The update must keep the tester's save and progress. The only change the tester will see in region 2 is listed in step 7.

1. **Recipe data** (`Tools/Art/tier01_recipe_specs.py` / `Tools/Crafting/generate_tier1_recipes.py` -> `Tier01Recipes.json`, `Tier1Recipes.cs`):
   - add the four fish dishes (Trout mushroom skillet, Salt-baked bass, Pine-nut bluegill, Apple-stuffed perch; Food, 2 hearts, tier-one timers)
   - Cured rabbit pelt becomes 2 Rabbit pelts + 1 Animal fat
   - Fine rabbit leather becomes 2 Rabbit leather + 1 Nettle thread
   - set the unlock-source text of every recipe to its mission code (no "branch placement pending" left except Goat cheese, Mixed-milk cheese and the box/bakery recipes)
2. **Retired roasts:** add a `Retired` flag to `RecipeDefinition` and keep the four plain fish roasts in `RecipeBook`. `CraftingState.Restore` drops batches whose recipe ID is unknown, so deleting them could strand pending roast crafts and mastery in test saves. Retired recipes:
   - are never a mission reward and are skipped by the debug LEARN T1 RECIPES
   - stay craftable only for a save that already knows them
   - keep their items as legacy food
3. **Items** (`Tier1CraftedItems.cs`): four new food items. Until the dish art exists, each uses its species' roast model and footprint as a stand-in (step 9).
4. **Mission catalog 3** (`MissionModel.cs`):
   - add the five bridge missions G6, H5, H6, E9 and E10 (design-only so far; H5 requests Animal fat) and the 20 coverage missions, in the section 11.6.1.1 branch order
   - E5 requests Bone
   - E8 moves to the end of the Explorer branch, keeping its order relative to all older missions
   - Index and the paper position follow the new order
5. **Save migration** (save schema stays 3; new fields are optional):
   - **Per-claim catalog:** the single `legacyClaims` prefix becomes a per-claim catalog list. A catalog-1 save converts as its first `legacyClaims` claims -> 1 and the rest -> its saved catalog.
   - **Active mission:** the active mission records the catalog it was accepted under. It validates against that catalog's order and may finish before newly inserted predecessors, as in the catalog-2 migration.
   - **E5's old request:** an E5 accepted under catalog 1-2 keeps its old request (Pine branches) and its delivered count. Mission definitions carry the request per catalog where it changed.
   - Nothing is granted and claims are never inferred. Migration runs once on load and is saved with catalog 3.
6. **Editor checks** (Milestone 7 and 8 suites, run through `Temp/Milestone5.request`):
   - recipe counts and inputs, and the retired roasts excluded from LEARN T1 RECIPES and from mission rewards
   - every tier-one loose and harvest source has a mission (section 11.6.5 check)
   - every tier-one Material has a recipe or deposit use
   - branch order, region gate and settlement-last placement
   - migrations: catalog 1 -> 3 and 2 -> 3, using saves shaped like the tester's region-2 state (claims through G4/L3/H3/E4, E5 active with some Pine branches delivered, H4 active, nothing active); the Nov 6 phone save shape (L4 claimed, E6 active); E8 claimed before the move
   - forged or out-of-order claims rejected
   - a pending roast batch and its known recipe surviving reload
   - All Milestone 1-8 suites must pass; then build the development APK.
7. **What the region-2 tester will see after installing:**
   - The Gatherer branch offers **G7 (10 Pine cones -> Pine-cone ornament)** after G4, then **G8 (10 Pine nuts -> Toasted pine nuts)** once a pine axe is known or carried.
   - **E5 asks for 10 Bone** (from rabbits), unless E5 was already accepted: then it finishes on Pine branches.
   - Nothing else changes before region 3. The new regions 3-6 missions only appear as the mission region advances.
   - Missions that need ranch milk or farm Tea herbs stay locked ("sold at a settlement") at the end of their branches until Milestone 10.
8. **Install procedure (when the phone is back):**
   - At a camp, force-stop the app.
   - Back up `PrototypeSession/` with `adb pull` to an ignored folder under `Builds/Android/`.
   - Run the pulled `session-v3.json` through the Editor save validation and migration before installing.
   - Install over the existing app, keeping its data, and confirm the claims and active mission are kept.
   - Device list: G7/G8 offers and pickup delivery in region 2; E5 Bone (or legacy Pine branches); paper positions; reload during an active migrated mission; the four fish dishes crafting from debug ingredients; Cured rabbit pelt and Fine rabbit leather with their new inputs; no lost pending crafts.
9. **Art (other tool, when the user resumes art work):** models for the four fish dishes, replacing the stand-ins. Request: `Docs/Art/tier1-fish-dishes-art-request-2026-11-07.txt`. Not blocking.

*Not in this update:* tier 2-8 recipes and missions (not implemented yet); house deposits (Milestone 11); settlement listings (Milestone 10); Goat cheese and Mixed-milk cheese placement (open decision). The Spanish plans are not synced.

**Current (November 2, 2026, end of day):** Milestone 8 is complete in code, and its device round, together with the checks from the same day's art and terrain work, is pending on the 23:28 build. See "Session checkpoint (November 2, 2026, end of day)" in the Milestone 8 notes for the full test list, open decisions and the next work in order. After the device round, commit Milestone 8.

**Earlier (October 31, 2026):** Milestones 0-5 have passed their exit checks. Next is Milestone 6: the three harvesting families (tree chopping, hunting, fishing) using the confirmed interaction designs in sections 8.2.1-8.2.3, with queued drops. Original guidance kept for reference: Begin with milestones 0 and 1 only: build to Android, then forward-only path movement, avatar speed gestures, independent 360-degree camera orbit, and the distinct stop/resume rules. Next implement milestone 2's full-stack spatial pickups, including reachable resources behind the avatar. Do not start by authoring the full recipe catalog, all regions, or a complete settlement UI.

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
| Content scope | 24 regions/four six-region tiers; exactly three loose IDs/region and 18 unique/tier; baseline branch+pelt/hide/confirmed turkey-feather overlap plus explicit tree-food overlaps (7.1.1); no loose log/meat/fish; identical map/spawn tables; early 10-14/later up-to-24 camps; seven tools/family, 24 lumber trees, no pending species dependency |
| Simplified recipes/store | Each cheese uses its fixed milk ID(s) + salt with Food timer/mastery; no mandatory culture/tannin; cups/bottles are exact Material inputs; bricks/panels upgrade house; furniture remains store-only |
| Trading | Exclusive 15 seconds; all other actions blocked; regen continues; Return settles unsold player listings at 70% once; natural shredder/expiry 80%; NPC/previously settled goods excluded; cancel/expiry race; spent session survives restart; actual departure/return refreshes |
| Warehouse | All sellable categories, two partial ten-unit exact-ID boxes, mixed profile/condition values, 100% cash/no extras, 10,000 cash -> 10 gold on tenth box, integer remainder carry, exact-once reload, unique-instance escrow |
| Shops/boxes | Fishery two distinct fish + one exclusive, Q5/cash, <=1,000 stock/slot/reset; source filtering and no reroll/refill on reopen/reload. Six typed box slots, physical Q5 vs mastery-one recipe grant, learned/reserved/within-box duplicates excluded, no eligible recipe fallback, recipe became-known-before-claim fallback, full-bag learning, exact-once claims, no raw exclusive/unknown finished-product backdoor, main progression never random-gated |
| Travel | One-time toll, forbidden region destination, repeated scene load, interrupted transition, correct return camp |
| Settlement | Eight grid sizes, six furniture types/level, buff only while placed and once per type (duplicates add none), stored pieces give none, every chest a separate horizontal container (up to 48 x 6) opened above the player inventory, hold-to-move bird's-eye placement without overlap, purchase placed at the nearest free space or sent to storage, all owned types fit at every level, 12 shelf entries |
| Timers | Craft completion, 72-hour independent trees, pet cap, device clock forward/back, repeated resume |
| Save recovery | Missing definition, older schema, corrupt primary, newer unsupported schema, interruption at each critical transaction |
| Performance | 24 live bundles plus huge pending queue, full bag/large horizontal storage, x1000 crafts, all tree records, 20 scene round trips |

Additional acceptance checks:

- **Food:** eat six hearts at 3/3; verify six pending with no instant gain, no loss while standing capped, and progressive consumption while walking after inventory close. Test equal/higher/lower drain rates, queue exhaustion within a frame, different frame sizes, multiple servings without rate stacking, safe-area priority, twelve-heart premium pie with a three-heart cap, hat break, zero with pending food, suspension/travel/reload, and exact-once consume/enqueue. Simulate the 26-heart route with the proposed rate and retained surplus, including a partial start.
- **Recipes:** 33 optional collectible IDs = 28 Food + 5 Misc: 26 balloon recipes plus seven bakery recipes; eight added dishes recover 6/6/7/7/8/8/9/10 total hearts per serving. Region 1 without region 5 rejects oat dishes; discovered source regions plus accessible exact milk enable them. Check all later-region gates, ingredient quantities/yields, tier/pantry rules, known refinement chains, duplicate prevention, and exclusion of all seven bakery recipes from the balloon pool.
- **Map:** check current-region header/marker, locked segments/camps, and discovered-region list below the first camp. Same-region travel selects the exact unlocked camp; cross-region travel always selects first camp, including a later-numbered already-discovered region. Verify selected-current-camp no-op, unpaid-border rejection, new map after arrival, correct Return to Trail, preserved food queue, and interrupted travel without duplicate arrival/visit refresh.

Helpful developer tools: searchable item/recipe browser; fill/clear selected container; grant currencies; equip rank; mission selector; seeded loot preview; simulated UTC clock; teleport to camp; show ownership/transaction ledger; export economy report. Keep debug commands separated from normal player UI and available in current development/test builds. Removal or disablement is deferred until explicitly requested; no milestone completion triggers it automatically.

## 20. Confirmed answers and delegated implementation defaults

The developer answered the earlier questions; these decisions supersede conflicting proposals above. Questions 5 and 9 delegate implementation defaults, not new approval gates.

1. **Craft presentation:** keep crafting/inventory open. On Craft, center item/bulk on the 3 x 3 grid with quality, applicable durability and timer briefly, then drop into the backpack. Proposed hold: one second. Timed products occupy bag space and remain unusable until ready; the timer is not a wait before delivery. Portrait remains the working layout proposal; verify readability on device.
2. **Placement:** free placement only, fixed orientation, no item rotation.
3. **Durability:** tier-one level-one Normal axe 10, slingshot 6, rod 10 uses. Add ten per tier within each family. Elite has exactly twice the same-tier Normal baseline; only tiers 2-4 have Elite. Existing mastery/quality/eligible furniture layers apply afterward. Harvest action counts remain the section-8.2 starting proposal.
4. **NPC prices/profiles:** discounts toward the shredder are enabled; use the proposed 110%-to-80% curve. NPC-generated gear is creation level one, random Q1-Q5, with no furniture buffs. Player listings retain their actual profile; normal shops remain Q5. Item furniture buffs belong only to player-crafted goods.
5. **Pending products/overflow — delegated default:** bag-owned timed products with a timer badge; no limit on outstanding records (October 31, 2026). Reserve valid bag space without rearranging items; route genuine overflow visibly to the persistent tray. One owner and one completion/mastery credit across reloads.
6. **Missions:** exact item ID and quantity only, with no quality/durability/mastery threshold. Preserve ready/available ownership checks and worn-gear protection; partially used tools of the required ID count.
7. **Chests:** one horizontal scrolling free-placement container like the backpack, no pages. Proposed tier dimensions: 6/12/24/48 wide by six high; virtualize offscreen views.
8. **Warehouse:** any ready sellable item, including unique gear. Two boxes, ten units/instances per exact ID. Pay full quoted cash with no extra/furniture bonus, then 1 gold per 1,000 cash of the last ten boxes' paid cash (revised October 31, 2026 from 3%): 10,000 cash -> 10 gold. Carry the integer cash remainder. Mixed profiles per ID and condition-adjusted used-gear quotes are delegated valuation defaults, not mission filters.
9. **Travel/reset policy — confirmed update:** unlocked map travel remains free, with existing destination/toll rules. Camp trade refreshes only after a genuine departure/return or a different camp; the current-camp no-op never refreshes it. Settlement material/ingredient shops refresh each actual settlement departure and return, with no timer. Balloon offers reroll on shop close/reopen, including the displayed special clothing, within the same camp visit. Ordinary shop reopen/reload preserves the settlement visit; balloon reload preserves the open session. Paid purchases/box manifests never reroll. This update supersedes the earlier 30-minute default; do not add a wait to ingredient/material shopping.

**Deferred release design and scope override:** the previously specified one-item collection rule remains future design intent only. Current milestones retain 1,000-unit pickups and debug tools; no fresh-save grant-free/release playthrough or rebalancing is required. The initial 24 regions/four tiers are the base testing scope, not the final product; higher-tier materials/content remain pending definition and implementation. Existing defined mechanics (twelve-heart meter, <=26-heart legs, exact ingredients, six box rewards and x1-only gear crafts) remain unchanged. Rebalancing, debug-grant removal/disablement, release collection, release preparation, and deployment/publishing each require an explicit future user instruction.

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
