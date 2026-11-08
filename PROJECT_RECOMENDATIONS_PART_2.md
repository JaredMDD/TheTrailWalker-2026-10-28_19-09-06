# TheTrailWalker: Recommendations Part 2 — Progression Beyond Tier 4

**Progression logic revision - November 6, 2026:** construction bridges now precede settlement arrival, mission execution order is separate from stable codes, barrel filling has explicit pantry permissions, and exotic-pet capacity counts opportunities separately from test stacks. See Part 1 sections 9.0.1 and 11.6.1; Part 2 sections 10.1, 15 and 16.5.1. **Design updated; gameplay implementation and current test/build/device status are not certified by this edit.** The other tool should review [the logic and testing handoff](Docs/PROJECT_HANDOFF.md#logic-review).

Updated: November 6, 2026 (gems are cut or polished before the keepsakes and Set B garments use them: sections 10.3, 13.1, 14.1 and 15). Before: November 5, 2026. Sections 13-16 add the house and settlement upgrades, the equipment sets, tools, added recipes and the full tier 5-8 mission plan, following Part 1 sections 9.6 and 11.6.

## 1. Scope and status

This document collects the expansion advancements already reserved in [Part 1](PROJECT_RECOMENDATIONS.md), sections 6.2.1, 6.3, 8, 9 and 13, and proposes a sequence for defining them. It covers resource tiers **5–8 and their future regions**, after the current 24-region/four-tier base. Region number, resource tier, recipe mastery, quality and house level are distinct. Region 5 is tier 1, not the start of this expansion.

**Status: design backlog; not implemented.** Part 1 remains the authority for current mechanics/testing. Creating Part 2 does not activate expansion content or mark existing milestones complete. “Carried forward” means already mentioned in Part 1; “proposed” means a new planning recommendation; “open” means data/decisions still needed.

## 2. Carried-forward advancements

| Future tier | Reserved wood / wild animal | Material and food ideas already deferred in Part 1 |
| --- | --- | --- |
| 5 | Maple / deer | Maple wood progression, deer hides, venison recipes, deer antler. Section 8 now proposes Maple sap and an exact syrup conversion; these were not defined in Part 1. |
| 6 | Cedar / mountain goat | Cedar wood progression, hunted goat hides/meat, hunting horns. Wild mountain goat is separate from existing domestic goat dairy; milk access does not imply hunting access. |
| 7 | Redwood / buffalo | Redwood wood progression, buffalo hides/meat, hunting horns. |
| 8 | Ebony / bear | Ebony wood progression, bear fur/meat, bear claw. Keep bear claw as one catalog concept rather than duplicate definitions. |

These species pairings were reserved in Part 1. Sections 7–12 now propose region assignments, resource sources, recipes, yields and selected shop prices. Silk and cashmere were unassigned backlog: this draft explicitly proposes tier-6 sources and conversions in sections 7–8, with shed Goat down gathered separately from hunting. Existing approved optional wool remains in the base catalog. All new assignments remain proposed design data.

**Proposed:** extend the existing branches/logs/planks pattern to each reserved wood once exact IDs, sources and conversion costs are authored. Define useful raw/processed hide/fur outputs without duplicate rewards from one animal unless explicitly configured. Every material needs an acquisition source and a useful recipe or house-upgrade sink. No expansion art, recipes or runtime classes are required by the current milestones.

## 3. Rules to carry into expansion design

- Preserve exact ingredient identities. Proposed extension of the current recipe-tier window: tier 5 accepts 4/5, tier 6 accepts 5/6, tier 7 accepts 6/7, tier 8 accepts 7/8. Only explicitly approved Food pantry ingredients may be older. Author every recipe before treating this extension as executable data.
- Reuse existing Normal/Elite tool behavior as the basis: Normal tools at every tier; Elite recipes consume an exact same-tier Normal instance plus explicit previous-tier advanced materials. Section 13 authors the tier-5–8 tools (Normal/Elite) and **two equipment sets per tier** (Set A from the block's early regions, Set B from its late regions).
- Keep four mission branches and one active mission globally. New missions require exact IDs/counts and accessible prerequisites. Random recipe discoveries must not block mandatory progression. Section 15 plans every tier-5–8 mission: **150 units** for stackable requests and **40 instances** for tools and garments, never above the campaign ceiling.
- Preserve three featured loose resources per region. This draft proposes six regions per new tier, adding 24 regions and 72 loose-resource entries (section 7). Hunting outputs, logs and fish do not occupy extra loose-resource slots. Section 9 proposes the fish species and regional pairs.
- Existing caps are already available in the base game: twelve hearts, ten pockets, ten hooks, 1,000-unit stacks and 1.5x stack model growth. Do not reserve them for future tiers. Expansion does not automatically raise speed/capacity or the current 26-heart segment budget. **No new stats (user, November 5, 2026): tier four is the capability limit**; tiers 5–8 add durability, tool reach and variety, not hearts, speed, pockets, hooks or area (section 13).
- **Every recipe is a mission reward (user, November 5, 2026)**: materials, Food and Misc included, with no discovery unlocks. Each fish species is a collection mission that teaches a varied dish, not a plain roast (section 14.2). A recipe may be a shop or box unlock only if it is a premium recipe (section 14.4).
- Keep progressive food recovery and pending hearts. Ordinary mission food stays at most 6 hearts per serving (the tier-four level); premium box food uses Part 1's tier-four band of 11–12 and bakery food 12 (section 14.2). Sections 8–11 propose gold prices while retaining existing shop/box probabilities. The seven tier-1–4 bakery recipes remain eligible; eight expansion additions bring the proposed catalog to fifteen.
- Furniture stays purchased. Materials may support ingredients, containers and house upgrades; future woods/trophies do not imply player-crafted furniture. Section 16 adds house levels 9–16, decorative furniture, four more shop looks and lumber sections; no new buff strength.

## 4. Discovery and persistence requirements

Require normal discovery of a recipe's authored tier and **each exact ingredient, with at least one discovered and reachable authored source per ingredient** before offering/unlocking it. Follow refined inputs to raw sources and require known conversions or accessible exact shops as applicable. High mastery, gold, gifted inputs, bought food or domestic goat products never prove discovery of future regions/wild species.

Validate at offer generation and again before payment/unlock, including after reload/migration. Locked previews show unmet sources without granting knowledge. Persist discovery, recipe IDs/mastery, stock, quotes, visit IDs and transactions; never infer discovery from owned items, reroll paid rewards or charge twice. Existing tier-1–4 rules continue to apply.

Until implemented explicitly, exclude expansion definitions from runtime loot, tools, missions, shops, boxes, pets and house costs. If disabled IDs already exist in test saves, migrate or quarantine them safely; never silently reinterpret a species or delete paid entitlements. Preserve existing IDs, quantities and intrinsic profiles.

## 5. Proposed expansion milestones

These planning steps follow the relevant working base systems. They do not renumber Part 1 or claim its milestones passed.

| Step | Work | Acceptance |
| --- | --- | --- |
| P2.1 — Define the tier-5 slice | Use sections 7–8 and 13–15 (regions, sources, Maple/deer IDs, recipes, Normal/Elite tools, both equipment sets and missions T5-G1…T5-E14); define access from tier 4. | Every output has an exact source/recipe; no inaccessible mandatory ingredient or missing previous-tier dependency. |
| P2.2 — Validate one tier-5 route | Integrate one representative route/camp, discovery, harvesting, food, gear, missions and save migration. | Tier-4 saves survive; undiscovered sources block unlocks; transfers/loss/drop cycles settle once. Keep full-stack pickups/debug tools. |
| P2.3 — Complete approved tier-5 content | Fill approved regional tables, routes, economy, shop/box/pet filters and house sinks. | Counts match approved tables; sources/recipes are reachable; earlier catalogs retain their meaning. |
| P2.4 — Repeat for tiers 6–8 | Review/implement Cedar/goat, Redwood/buffalo, then Ebony/bear using sections 7–12; review the proposed tier-6 silk/cashmere sources. | Missing-region/source tests per tier; no future content leaks into earlier rewards or requirements. |

## 6. Open data and checks

Resolved by sections 13–16 (November 5, 2026): mission tasks/rewards, tool definitions/durability, the equipment sets, camps per region (16–20) and house levels 9–16 with their costs. Still open: route geometry/drain tuning, detailed harvesting quantities and timing/valuation overrides, general cash-price tables and pet weight/source-group assignment for new materials. Sections 7–12 supply initial proposed region/resource/fish and recipe catalogs, plus selected shop quotes. Review these proposals before runtime implementation.

For each new recipe, check all prerequisites satisfied, then remove each required source/conversion individually. Include gifted inputs, refined shop stock, duplicate offers, locked previews, interrupted payment and reload/migration. Check exact ingredient consumption, Normal-to-Elite instance replacement, output ownership, 64-bit totals and full-stack performance on mobile.

The debug/testing policy continues: 1,000-unit generated pickups and debug tools remain available. Planning expansion or completing testing does not activate rebalancing, debug removal, one-item collection, release preparation or publication.

**Catalog update:** sections 7–12 now propose concrete region-25–48 data. Earlier open-item notes describe the initial backlog; the new tables resolve the listed region/recipe proposals while detailed tools, missions and house costs remain open.

## 7. Proposed region and resource catalog

**First populated proposal; design data, not runtime activation.** Propose six regions per tier: regions 25–48, three distinct loose resources per region, 18 per tier and 72 additions. Combined design target: 48 regions and 144 loose resources. English catalog names and IDs are stable across both language documents; localize display names during implementation. **Camps per region (confirmed by the user, November 5, 2026): the count does not increase past tier four.** Each region rolls 16–20 camps like tier four, random within the range (seed 20261105, column below; 104–110 camps per tier), with a maximum 26-heart walking budget per leg; no new capacity caps or climate hazards.


| Region / ID | Tier | Three exact loose resources / IDs | Fish pair | Water-node presentation | Camps |
| --- | --- | --- | --- | --- | --- |
| `region.25` Copperleaf Vale | 5 | Maple branches (`resource.maple_branches`); Deer hide (`resource.deer_hide`); Pears (`resource.pears`) | Brook trout + Smallmouth bass | stream and lake | 17 |
| `region.26` Lantern Meadows | 5 | Rye (`resource.rye`); Amber (`resource.amber`); Gneiss (`resource.gneiss`) | White crappie + Sauger | river and reservoir | 17 |
| `region.27` Veiled Ravine | 5 | Ramie fiber (`resource.ramie_fiber`); Zinc ore (`resource.zinc_ore`); Plums (`resource.plums`) | Brook trout + White crappie | stream and river | 20 |
| `region.28` Golden Hollow | 5 | Chestnuts (`resource.chestnuts`); Calendula (`resource.calendula`); Safflower seeds (`resource.safflower_seeds`) | Smallmouth bass + Sauger | lake and reservoir | 16 |
| `region.29` Glasswater Reach | 5 | Magnetite (`resource.magnetite`); Gypsum (`resource.gypsum`); Agate (`resource.agate`) | Brook trout + Sauger | stream and reservoir | 19 |
| `region.30` Sunset Orchard | 5 | Maple sap (`resource.maple_sap`); Chicory root (`resource.chicory_root`); Willow bast (`resource.willow_bast`) | Smallmouth bass + White crappie | river and lake | 20 |
| `region.31` Cloudgate Pass | 6 | Cedar branches (`resource.cedar_branches`); Mountain goat hide (`resource.mountain_goat_hide`); Apricots (`resource.apricots`) | Arctic char + Grayling | cold stream and alpine lake | 17 |
| `region.32` Highland Terrace | 6 | Jute fiber (`resource.jute_fiber`); Nickel ore (`resource.nickel_ore`); Buckwheat (`resource.buckwheat`) | Whitefish + Dolly Varden | tributary and deep lake | 19 |
| `region.33` Silvermist Basin | 6 | Blackcurrants (`resource.blackcurrants`); Hazelnuts (`resource.hazelnuts`); Kaolin (`resource.kaolin`) | Arctic char + Whitefish | cold stream and tributary | 18 |
| `region.34` Silken Canopy | 6 | Mulberry leaves (`resource.mulberry_leaves`); Silk cocoons (`resource.silk_cocoons`); Celestite (`resource.celestite`) | Grayling + Dolly Varden | alpine lake and deep lake | 17 |
| `region.35` Whistling Escarpment | 6 | Goat down (`resource.goat_down`); Mountain thyme (`resource.mountain_thyme`); Fennel seeds (`resource.fennel_seeds`) | Arctic char + Dolly Varden | cold stream and deep lake | 19 |
| `region.36` Moonlit Foothills | 6 | Almandine (`resource.almandine`); Pumice (`resource.pumice`); Quinces (`resource.quinces`) | Grayling + Whitefish | tributary and alpine lake | 20 |
| `region.37` Dawnwood Expanse | 7 | Redwood branches (`resource.redwood_branches`); Buffalo hide (`resource.buffalo_hide`); Peaches (`resource.peaches`) | Striped bass + Mullet | estuary and sheltered coast | 18 |
| `region.38` Ochre Prairie | 7 | Sisal fiber (`resource.sisal_fiber`); Chromium ore (`resource.chromium_ore`); Millet (`resource.millet`) | Flounder + Cobia | tidal inlet and open-coast pier | 17 |
| `region.39` Echoing Bluffs | 7 | Raspberries (`resource.raspberries`); Pecans (`resource.pecans`); Travertine (`resource.travertine`) | Striped bass + Flounder | estuary and tidal inlet | 17 |
| `region.40` Spicewind Flats | 7 | Tarragon (`resource.tarragon`); Sesame seeds (`resource.sesame_seeds`); Malachite (`resource.malachite`) | Mullet + Cobia | sheltered coast and open-coast pier | 16 |
| `region.41` Painted Gorge | 7 | Pomegranates (`resource.pomegranates`); Tourmaline (`resource.tourmaline`); Indigo leaves (`resource.indigo_leaves`) | Striped bass + Cobia | estuary and open-coast pier | 18 |
| `region.42` Saffron Horizon | 7 | Saffron (`resource.saffron`); Amaranth seeds (`resource.amaranth_seeds`); Red clay (`resource.red_clay`) | Mullet + Flounder | tidal inlet and sheltered coast | 18 |
| `region.43` Nightbloom Coast | 8 | Ebony branches (`resource.ebony_branches`); Bear fur (`resource.bear_fur`); Mangoes (`resource.mangoes`) | Grouper + Mahi-mahi | rocky shore and coastal pier | 18 |
| `region.44` Mirrorpetal Marsh | 8 | Lotus fiber (`resource.lotus_fiber`); Titanium ore (`resource.titanium_ore`); Sorghum (`resource.sorghum`) | Amberjack + Wahoo | sheltered bay and offshore pier | 16 |
| `region.45` Emerald Lagoon | 8 | Passion fruit (`resource.passion_fruit`); Pistachios (`resource.pistachios`); Serpentine (`resource.serpentine`) | Grouper + Amberjack | rocky shore and sheltered bay | 19 |
| `region.46` Starlit Gardens | 8 | Cardamom (`resource.cardamom`); Star anise (`resource.star_anise`); Moonstone (`resource.moonstone`) | Mahi-mahi + Wahoo | coastal pier and offshore pier | 16 |
| `region.47` Prismatic Inlet | 8 | Dragon fruit (`resource.dragon_fruit`); Black sesame seeds (`resource.black_sesame_seeds`); Aquamarine (`resource.aquamarine`) | Grouper + Wahoo | rocky shore and offshore pier | 19 |
| `region.48` Lastlight Shores | 8 | Dates (`resource.dates`); Teff (`resource.teff`); Opal (`resource.opal`) | Mahi-mahi + Amberjack | sheltered bay and coastal pier | 20 |



Branches and hide/fur are baseline overlaps; the tree-food IDs in section 8 are explicit additional LooseGathering/TreeHarvest overlaps. Logs, meat, antlers/horns/claws and fish are interaction outputs, never extra loose icons. Singular/plural ingredient labels below resolve to the exact resource ID above, not separate variants. Goat down is a shed-fiber gathering resource in region 35, not a second hunted pelt drop. Silk cocoons gather in region 34; Mulberry leaves have an authored craft sink without adding silkworm farming or a pet. These sources and tier-6 silk/cashmere placement are explicit new proposals.


## 8. Processing, hunting and regional recipes

All inputs below are exact. Regional raw IDs are written explicitly in section 7; processed outputs use `item.<recipe_suffix>`, except branch-to-log conversions output `item.maple_log`, `item.cedar_log`, `item.redwood_log` and `item.ebony_log`. Tree logs use these same IDs. `Maple branch`/`Maple branches` refer to the one `resource.maple_branches` ID; apply the same singular/plural alias rule to other resources. Singular/plural labels never permit variety substitution. Wild meat IDs are `item.raw_deer_meat`, `item.raw_mountain_goat_meat`, `item.raw_buffalo_meat` and `item.raw_bear_meat`; trophies are `item.deer_antler`, `item.mountain_goat_horn`, `item.buffalo_horn` and `item.bear_claw`. Hunted hide/fur outputs use the same exact resource IDs as their loose counterparts. Fishery ingredient and fish IDs are explicit in section 9.

Material recipes are unlevelled with fixed timers; Food/Misc retain Part 1 mastery/quality rules. Nothing crafts furniture. Costs represent simplified processes with no hidden fuel. **Every recipe in this section is a mission reward (section 15, November 5, 2026).** Do not require a random box or bakery recipe to unlock these conversions or ordinary meals. Tier-5–8 timer/valuation overrides must be authored before activation; do not silently extrapolate the four-tier numeric curves past their defined range.


| Recipe ID | Tier | Output | Exact inputs -> yield | Category |
| --- | --- | --- | --- | --- |
| `recipe.material.maple_log_from_branches` | 5 | Maple log | 3 Maple branches -> 1 | Material |
| `recipe.material.cedar_log_from_branches` | 6 | Cedar log | 3 Cedar branches -> 1 | Material |
| `recipe.material.redwood_log_from_branches` | 7 | Redwood log | 3 Redwood branches -> 1 | Material |
| `recipe.material.ebony_log_from_branches` | 8 | Ebony log | 3 Ebony branches -> 1 | Material |
| `recipe.material.maple_plank` | 5 | Maple plank | 3 Maple logs -> 1 | Material |
| `recipe.material.deer_leather` | 5 | Deer leather | 2 Deer hide -> 1 | Material |
| `recipe.misc.amber_gneiss_token` | 5 | Amber gneiss token | 1 Amber + 2 Gneiss -> 1 | Misc |
| `recipe.material.ramie_cloth` | 5 | Ramie cloth | 4 Ramie fiber -> 2 | Material |
| `recipe.material.zinc_ingot` | 5 | Zinc ingot | 2 Zinc ore + 1 Oak branch -> 1 | Material |
| `recipe.misc.calendula_chestnut_keepsake` | 5 | Calendula chestnut keepsake | 2 Calendula + 1 Chestnut + 1 Ramie cloth -> 1 | Misc |
| `recipe.material.safflower_oil` | 5 | Safflower oil | 3 Safflower seeds -> 1 | Material |
| `recipe.material.magnetite_ingot` | 5 | Magnetite ingot | 2 Magnetite + 1 Oak branch -> 1 | Material |
| `recipe.misc.agate_gypsum_medallion` | 5 | Agate gypsum medallion | 1 Agate + 2 Gypsum + 1 Ramie cloth -> 1 | Misc |
| `recipe.material.willow_cord` | 5 | Willow cord | 3 Willow bast -> 1 | Material |
| `recipe.material.maple_syrup` | 5 | Maple syrup | 3 Maple sap + 1 Maple branch -> 1 | Material |
| `recipe.material.cedar_plank` | 6 | Cedar plank | 3 Cedar logs -> 1 | Material |
| `recipe.material.mountain_goat_leather` | 6 | Mountain goat leather | 2 Mountain goat hide -> 1 | Material |
| `recipe.material.jute_cloth` | 6 | Jute cloth | 4 Jute fiber -> 2 | Material |
| `recipe.material.nickel_ingot` | 6 | Nickel ingot | 2 Nickel ore + 1 Maple branch -> 1 | Material |
| `recipe.misc.kaolin_hazelnut_charm` | 6 | Kaolin hazelnut charm | 2 Kaolin + 1 Hazelnut + 1 Jute cloth -> 1 | Misc |
| `recipe.material.silk_thread` | 6 | Silk thread | 3 Silk cocoons -> 2 | Material |
| `recipe.material.silk_cloth` | 6 | Silk cloth | 3 Silk thread -> 1 | Material |
| `recipe.misc.mulberry_celestite_pouch` | 6 | Mulberry celestite pouch | 2 Mulberry leaves + 1 Celestite + 1 Silk cloth -> 1 | Misc |
| `recipe.material.cashmere_yarn` | 6 | Cashmere yarn | 3 Goat down -> 2 | Material |
| `recipe.material.cashmere_cloth` | 6 | Cashmere cloth | 3 Cashmere yarn -> 1 | Material |
| `recipe.misc.almandine_pumice_carving` | 6 | Almandine pumice carving | 1 Almandine + 2 Pumice + 1 Jute cloth -> 1 | Misc |
| `recipe.material.redwood_plank` | 7 | Redwood plank | 3 Redwood logs -> 1 | Material |
| `recipe.material.buffalo_leather` | 7 | Buffalo leather | 2 Buffalo hide -> 1 | Material |
| `recipe.material.sisal_cloth` | 7 | Sisal cloth | 4 Sisal fiber -> 2 | Material |
| `recipe.material.chromium_ingot` | 7 | Chromium ingot | 2 Chromium ore + 1 Cedar branch -> 1 | Material |
| `recipe.misc.pecan_travertine_keepsake` | 7 | Pecan travertine keepsake | 1 Pecan + 2 Travertine + 1 Sisal cloth -> 1 | Misc |
| `recipe.material.sesame_oil` | 7 | Sesame oil | 3 Sesame seeds -> 1 | Material |
| `recipe.misc.malachite_tarragon_pouch` | 7 | Malachite tarragon pouch | 1 Malachite + 2 Tarragon + 1 Sisal cloth -> 1 | Misc |
| `recipe.material.indigo_dye` | 7 | Indigo dye | 3 Indigo leaves -> 1 | Material |
| `recipe.misc.tourmaline_indigo_token` | 7 | Tourmaline indigo token | 1 Tourmaline + 1 Indigo dye + 1 Sisal cloth -> 1 | Misc |
| `recipe.misc.saffron_clay_keepsake` | 7 | Saffron clay keepsake | 1 Saffron + 2 Red clay + 1 Sisal cloth -> 1 | Misc |
| `recipe.material.ebony_plank` | 8 | Ebony plank | 3 Ebony logs -> 1 | Material |
| `recipe.material.bear_felt` | 8 | Bear felt | 2 Bear fur -> 1 | Material |
| `recipe.material.lotus_cloth` | 8 | Lotus cloth | 4 Lotus fiber -> 2 | Material |
| `recipe.material.titanium_ingot` | 8 | Titanium ingot | 2 Titanium ore + 1 Redwood branch -> 1 | Material |
| `recipe.misc.pistachio_serpentine_token` | 8 | Pistachio serpentine token | 1 Pistachio + 2 Serpentine + 1 Lotus cloth -> 1 | Misc |
| `recipe.misc.moonstone_spice_pouch` | 8 | Moonstone spice pouch | 1 Moonstone + 1 Cardamom + 1 Star anise + 1 Lotus cloth -> 1 | Misc |
| `recipe.material.black_sesame_oil` | 8 | Black sesame oil | 3 Black sesame seeds -> 1 | Material |
| `recipe.misc.aquamarine_lotus_charm` | 8 | Aquamarine lotus charm | 1 Aquamarine + 1 Lotus cloth -> 1 | Misc |
| `recipe.misc.opal_keepsake` | 8 | Opal keepsake | 1 Opal + 1 Ebony plank -> 1 | Misc |



**Proposed exact interaction outputs and conversions:**


| Tier | Definition ID | Source | Output catalog (drop quantities use existing harvest service) |
| --- | --- | --- | --- |
| 5 | `tree.maple` | Maple | Maple branches + Maple logs + Maple sap (`resource.maple_sap`); food ingredient required |
| 5 | `hunt.deer` | deer | Deer hide + Raw deer meat + Deer antler; one hide/fur type per animal |
| 6 | `tree.cedar` | Cedar | Cedar branches + Cedar logs + Apricots (`resource.apricots`); food ingredient required |
| 6 | `hunt.mountain_goat` | mountain goat | Mountain goat hide + Raw mountain goat meat + Mountain goat horn; one hide/fur type per animal |
| 7 | `tree.redwood` | Redwood | Redwood branches + Redwood logs + Peaches (`resource.peaches`); food ingredient required |
| 7 | `hunt.buffalo` | buffalo | Buffalo hide + Raw buffalo meat + Buffalo horn; one hide/fur type per animal |
| 8 | `tree.ebony` | Ebony | Ebony branches + Ebony logs + Mangoes (`resource.mangoes`); food ingredient required |
| 8 | `hunt.bear` | bear | Bear fur + Raw bear meat + Bear claw; one hide/fur type per animal |



Each wood also has fixed Material recipe `recipe.material.<wood>_log_from_branches`: 3 exact branches -> 1 exact log. Planks consume tree or converted logs. Trophy Misc recipes consume the exact raw trophy directly, with no hidden cutting/polishing step. Harvest quantities/Elite boosts use the existing service's authored behavior, not an invented per-tier multiplier. Exact expansion tools, durability and mission requirements still require authoring before region activation.


| Recipe ID | Tier | Regional Food output | Exact inputs -> yield | Total hearts (mission range, section 14.2) |
| --- | --- | --- | --- | --- |
| `recipe.food.pear_rye_porridge` | 5 | Pear rye porridge | 2 Pears + 2 Rye + 1 Cow milk -> 1 | 6 |
| `recipe.food.plum_chestnut_pot` | 5 | Plum chestnut pot | 2 Plums + 2 Chestnuts + 1 Honey -> 1 | 6 |
| `recipe.food.chicory_maple_drink` | 5 | Chicory maple drink | 2 Chicory root + 1 Maple syrup + 1 Cow milk -> 1 | 5 |
| `recipe.food.apricot_buckwheat_pot` | 6 | Apricot buckwheat pot | 2 Apricots + 2 Buckwheat + 1 Cow milk -> 1 | 6 |
| `recipe.food.blackcurrant_hazelnut_pot` | 6 | Blackcurrant hazelnut pot | 2 Blackcurrants + 1 Hazelnut + 1 Cow yogurt -> 1 | 6 |
| `recipe.food.quince_thyme_compote` | 6 | Quince thyme compote | 2 Quinces + 1 Mountain thyme + 1 Fennel seed -> 1 | 6 |
| `recipe.food.peach_millet_pot` | 7 | Peach millet pot | 2 Peaches + 2 Millet + 1 Cow milk -> 1 | 6 |
| `recipe.food.raspberry_pecan_pot` | 7 | Raspberry pecan pot | 2 Raspberries + 1 Pecan + 1 Cow yogurt -> 1 | 6 |
| `recipe.food.pomegranate_amaranth_pot` | 7 | Pomegranate amaranth pot | 2 Pomegranates + 2 Amaranth seeds + 1 Honey -> 1 | 6 |
| `recipe.food.mango_sorghum_pot` | 8 | Mango sorghum pot | 2 Mangoes + 2 Sorghum + 1 Cow milk -> 1 | 6 |
| `recipe.food.passionfruit_pistachio_pot` | 8 | Passion fruit pistachio pot | 2 Passion fruit + 1 Pistachio + 1 Cow yogurt -> 1 | 6 |
| `recipe.food.date_teff_pot` | 8 | Date teff pot | 2 Dates + 2 Teff + 1 Cow milk -> 1 | 6 |
| `recipe.food.maple_sap_venison_steak` | 5 | Maple-sap venison steak (replaces Roasted deer) | 1 Raw deer meat + 1 Maple sap + 1 Maple branch -> 1 | 6 |
| `recipe.food.cedar_plank_goat` | 6 | Cedar-plank goat with plums (replaces Roasted mountain goat) | 1 Raw mountain goat meat + 1 Plums + 1 Cedar plank -> 1 | 6 |
| `recipe.food.smoked_buffalo_jerky` | 7 | Smoked buffalo jerky (replaces Roasted buffalo) | 1 Raw buffalo meat + 1 Rock salt + 1 Redwood branch -> 2 | 5 |
| `recipe.food.honey_braised_bear` | 8 | Honey-braised bear (replaces Roasted bear) | 1 Raw bear meat + 1 Honey + 1 Mangoes -> 1 | 6 |

**Meat variety (user, November 7, 2026):** the four plain roasts, the same wood-fired product with more hearts, are replaced by four different first hunting dishes, each using an ingredient the block's first region already supplies: Maple sap (maple trees, region 25), Plums (region 27, tier-five window) with a Cedar plank (T6-L1, region 31) as the grilling plank, Rock salt (pantry) and Honey (pantry) with Mangoes (ebony trees, region 43). The forms differ (steak, plank-grill, jerky, braise) and also avoid the premium box pots (Venison pear pot, Goat apricot pot). The braise lists no wood because it is not a roast (Part 1 section 9.4.1).



**Explicit Food pantry extension:** exact Wheat flour/dairy/Honey/Sugar remain available as older inputs; add Maple syrup (tier 5), Sesame oil (7) and Black sesame oil (8) for these and later Food recipes after their sources are discovered. No pantry exception applies to Material/Misc. Main produce, meat, fish, regional spices and fuel remain own/previous tier. No oil or dairy substitution. Food timers remain below one hour; x1/x100/x1000 scale exact costs/yields without 1,000 visual objects.


**Required food drops and alternative-source gates (correction):** Part 1 section 7.1.1 applies to every expansion tree, including massive trees: branches + logs + one required food ingredient, with at most a second authored ingredient. Maple gives Maple sap, Cedar gives Apricots, Redwood gives Peaches, and Ebony gives Mangoes. These are the existing exact loose-resource IDs, not new fruit variants; loose sources remain in regions 30/31/37/43 respectively. Do not add a fourth featured loose resource or count the same ID twice. Sap and fruit are raw cooking ingredients, not prepared food for automatic eating. Their existing syrup, regional-meal, box and bakery recipes provide useful sinks.

Discovering and having eligible harvesting access to any tree node in 25–30 / 31–36 / 37–42 / 43–48 respectively makes its ingredient source reachable; merely owning a gifted ingredient does not. For each ingredient, use OR across valid discovered sources, AND across distinct ingredients. Therefore a Maple tree in region 25 can supply sap for the pear tart without region 30, and a Cedar tree in discovered region 33 can supply apricots without separately requiring loose region 31. Refinement knowledge, other ingredient discovery and exact dairy/flour access still apply. For old saved locked offers, revalidate source eligibility against this revised table without rerolling or charging twice.

Use Part 1's Normal 2–4 ingredient bundles, Elite 4–6, 1,000 units per bundle and massive x50 on food as well as wood. Persist aggregate manifests before exposing drops. P2.1–P2.4 check all four tree IDs for mandatory food, at-most-two ingredients, Normal/Elite/massive quantities and exact-once claim/reload. Include both positive alternative-source tests (tree discovered, loose source undiscovered; loose discovered with valid access, tree unavailable) and negative tests (neither source accessible). This corrects the former branches/logs-only output table and the old requirement to discover every possible source of an ingredient.

## 9. Fish and fishery

| Fish ID | Tier | Species | Collection-mission dish (section 14.2) |
| --- | --- | --- | --- |
| `fish.brook_trout` | 5 | Brook trout | Calendula brook-trout bake |
| `fish.smallmouth_bass` | 5 | Smallmouth bass | Bass pear bake |
| `fish.white_crappie` | 5 | White crappie | Crappie chestnut cakes |
| `fish.sauger` | 5 | Sauger | Sauger rye chowder |
| `fish.arctic_char` | 6 | Arctic char | Arctic char apricot bake |
| `fish.grayling` | 6 | Grayling | Grayling fennel stew |
| `fish.whitefish` | 6 | Whitefish | Whitefish hazelnut cakes |
| `fish.dolly_varden` | 6 | Dolly Varden | Dolly Varden buckwheat parcel |
| `fish.striped_bass` | 7 | Striped bass | Striped-bass pecan crust |
| `fish.mullet` | 7 | Mullet | Sesame mullet rice |
| `fish.flounder` | 7 | Flounder | Flounder millet cakes |
| `fish.cobia` | 7 | Cobia | Cobia saffron stew |
| `fish.grouper` | 8 | Grouper | Grouper mango bake |
| `fish.mahi_mahi` | 8 | Mahi-mahi | Mahi-mahi passion fruit bowl |
| `fish.amberjack` | 8 | Amberjack | Amberjack teff broth |
| `fish.wahoo` | 8 | Wahoo | Black-sesame wahoo |

Sixteen new fish, four per tier, with all six distinct unordered pairs per tier in the regional table. Combined trail catalog: 32 species. Discovering one region unlocks only its pair, not all four species. Nodes use suitable freshwater/coastal access as listed. **Revised (user, November 5, 2026):** there are no plain fish roasts in tiers 5–8. Each species is a collection mission that teaches its dish above (section 15); none of them is a box recipe.


| Exclusive ID | Tier | Ingredient | Required discovered region | Proposed Q5 cash/unit | Cash/full 1,000 stack |
| --- | --- | --- | --- | --- | --- |
| `fishery.scallop` | 5 | Scallop | 25 | 500 | 500000 |
| `fishery.mussel` | 6 | Mussel | 31 | 800 | 800000 |
| `fishery.octopus` | 7 | Octopus | 37 | 1200 | 1200000 |
| `fishery.oyster` | 8 | Oyster | 43 | 1800 | 1800000 |



These four exclusive ingredients support optional premium box recipes and remain **cash purchases, not gold**. Together with Part 1 there are eight exclusive types. Keep three Q5 offers: two distinct discovered fish and one random eligible exclusive, 1,000 units per slot/visit. Older exclusives remain eligible. Only actual departure/return refreshes stock. Exclusive seafood never comes from trail/fish nodes, pets, procedural NPC initial supply or physical box rewards. Boxes grant recipes, not these raw ingredients. Listed cash prices are already Q5 quotes; do not multiply quality again or apply the bakery stack discount.


## 10. Optional premium mystery-box recipes

| Recipe ID | Tier | Output | Exact inputs -> 1 | Hearts / role |
| --- | --- | --- | --- | --- |
| `recipe.misc.deer_antler_maple_token` | 5 | Deer antler maple token | 1 Deer antler + 1 Maple plank + 1 Willow cord | Misc |
| `recipe.food.maple_glazed_scallops` | 5 | Maple-glazed scallops | 1 Scallop + 1 Maple syrup + 1 Cow butter | 11 |
| `recipe.food.scallop_rye_chowder` | 5 | Scallop rye chowder | 2 Scallop + 2 Rye + 1 Cow cream | 12 |
| `recipe.food.venison_pear_pot` | 5 | Venison pear pot | 2 Raw deer meat + 2 Pears + 1 Maple syrup | 11 |
| `recipe.food.goat_apricot_pot` | 6 | Mountain goat apricot pot | 2 Raw mountain goat meat + 2 Apricots + 1 Fennel seed | 11 |
| `recipe.misc.goat_horn_silk_charm` | 6 | Mountain goat horn silk charm | 1 Mountain goat horn + 1 Cedar plank + 1 Silk cloth | Misc |
| `recipe.food.mussel_buckwheat_bowl` | 6 | Mussel buckwheat bowl | 2 Mussel + 2 Buckwheat + 1 Mountain thyme | 12 |
| `recipe.food.mussels_thyme_cream` | 6 | Mussels in thyme cream | 2 Mussel + 1 Mountain thyme + 1 Cow cream | 11 |
| `recipe.food.buffalo_amaranth_pot` | 7 | Buffalo amaranth pot | 2 Raw buffalo meat + 2 Amaranth seeds + 1 Sesame oil | 12 |
| `recipe.misc.buffalo_horn_indigo_token` | 7 | Buffalo horn indigo token | 1 Buffalo horn + 1 Redwood plank + 1 Indigo dye | Misc |
| `recipe.food.octopus_millet_bowl` | 7 | Octopus millet bowl | 2 Octopus + 2 Millet + 1 Tarragon | 12 |
| `recipe.food.saffron_sesame_octopus` | 7 | Saffron sesame octopus | 1 Octopus + 1 Saffron + 1 Sesame oil | 11 |
| `recipe.misc.bear_claw_lotus_charm` | 8 | Bear claw lotus charm | 1 Bear claw + 1 Ebony plank + 1 Lotus cloth | Misc |
| `recipe.food.bear_date_pot` | 8 | Bear date pot | 2 Raw bear meat + 2 Dates + 1 Black sesame oil | 12 |
| `recipe.food.oyster_sorghum_chowder` | 8 | Oyster sorghum chowder | 2 Oyster + 2 Sorghum + 1 Cow cream | 12 |
| `recipe.food.star_anise_butter_oysters` | 8 | Oysters in star-anise butter | 1 Oyster + 1 Star anise + 1 Cow butter | 11 |



**Premium rework (November 5, 2026):** the four plain exclusive-seafood roasts are replaced by composed dishes (scallops, mussels, octopus, oysters above), so every box recipe meets the premium rule in section 14.4. Sixteen additions = 12 Food + 4 Misc, all optional box unlocks with no free finished output. Combined with Part 1's original 26, these make 42; with the barrel drinks of both parts (Part 1 section 9.4.5 and section 10.1 below) the combined box pool is **116 recipes** (sections 10.1-10.3). Retain six typed rewards, the proposed 5% recipe-category roll per random slot, learned/reserved exclusion and deterministic duplicate fallback. Physical rewards remain Q5; knowledge has no grade/bag footprint and starts mastery one/zero crafts. No mandatory progression depends on these discoveries. Misc outputs are sale goods, not furniture.


### 10.1 Barrel drinks for tiers 5-8 (mystery-box exclusive, proposal November 5, 2026)

Part 1 section 9.4.5 has nine oak-barrel drinks (tier four). Part 2 had none. These **twelve drinks (three per tier)** follow the same two-step design:
- a **filling recipe** puts the drink's ingredients into a barrel to age
- a separate **bottling recipe** turns the filled barrel and Glass bottles into eight bottles

**Ingredient rule (revised November 6, 2026):** filling uses Part 1 section 9.0.1's explicit `BarrelFilling` permission, not the Food role. Exact Sugar, Honey and Maple syrup inputs use each recipe's barrel-pantry allowlist; authored aging produce has its separate permission. Listed recipes/quantities stay unchanged, with regional fruit/grain/spice as listed and all sources/conversions required. Wood, fittings and unlisted fuel obey the nonfood tier window. Both steps stay **mystery-box exclusive**, learned separately; bottling requires its known filling recipe and remains Food. Filled barrels are trade goods, never auto-eaten, with level 1-10 Misc aging timers. Bottling consumes the exact barrel and Glass bottles, returns no empty barrel, and produces eight Food bottles. Pantry permission does not extend to upstream conversions or ordinary Misc.

**Barrel wood and fittings by tier:** Part 1 uses oak only, which is the real-world barrel wood. From tier six oak falls outside the recipe window, so each tier uses a wood with a real cask tradition and its own fittings:

| Tier | Barrel shape (filling) | Real-world basis |
| --- | --- | --- |
| 5 | 4 Oak planks + 2 Zinc fittings + ingredients | Oak casks (tier four oak is inside the {4,5} window) |
| 6 | 4 Cedar planks + 2 Nickel fittings + ingredients | Cedar casks, as used for sake |
| 7 | 4 Redwood planks + 2 Chromium fittings + ingredients | Redwood fermenting tanks |
| 8 | 4 Redwood planks + 2 Titanium fittings + ingredients | Redwood again (ebony is too dense for cooperage) |

Bottling is always **1 filled barrel + 8 Glass bottles -> 8 bottles** (Glass bottle is a container exception, Part 1 section 9.0.1). Sugar and Honey enter as pantry staples, and the main fruit, grain or spice comes from the tier's own block.

| Drink | Tier | Filling recipe (ID: drink inputs added to the barrel shape) | Bottling recipe | Hearts per bottle | Sources |
| --- | --- | --- | --- | --- | --- |
| Pear cider (perry) | 5 | `recipe.barrel.pear_cider`: 12 Pears | `recipe.food.pear_cider` | 11 | Pears 25 |
| Plum wine | 5 | `recipe.barrel.plum_wine`: 12 Plums + 2 Maple syrup | `recipe.food.plum_wine` | 11 | Plums 27, Maple sap 25-30 |
| Maple rye whiskey | 5 | `recipe.barrel.maple_rye_whiskey`: 12 Rye + 1 Maple syrup | `recipe.food.maple_rye_whiskey` | 11 | Rye 26, Maple sap 25-30 |
| Apricot brandy | 6 | `recipe.barrel.apricot_brandy`: 12 Apricots + 2 Sugar | `recipe.food.apricot_brandy` | 11 | Apricots 31 / cedar trees, Sugar cane 17 |
| Blackcurrant wine | 6 | `recipe.barrel.blackcurrant_wine`: 12 Blackcurrants + 2 Honey | `recipe.food.blackcurrant_wine` | 11 | Blackcurrants 33, Honey 11 |
| Quince thyme mead | 6 | `recipe.barrel.quince_thyme_mead`: 10 Honey + 4 Quinces + 1 Mountain thyme | `recipe.food.quince_thyme_mead` | 11 | Quinces 36, Mountain thyme 35, Honey 11 |
| Peach brandy | 7 | `recipe.barrel.peach_brandy`: 12 Peaches + 2 Sugar | `recipe.food.peach_brandy` | 11 | Peaches 37 / redwood trees, Sugar cane 17 |
| Pomegranate wine | 7 | `recipe.barrel.pomegranate_wine`: 12 Pomegranates + 2 Honey | `recipe.food.pomegranate_wine` | 11 | Pomegranates 41, Honey 11 |
| Raspberry millet beer | 7 | `recipe.barrel.raspberry_millet_beer`: 10 Millet + 4 Raspberries | `recipe.food.raspberry_millet_beer` | 12 | Millet 38, Raspberries 39 |
| Spiced mango rum | 8 | `recipe.barrel.spiced_mango_rum`: 8 Mangoes + 4 Sugar + 1 Star anise | `recipe.food.spiced_mango_rum` | 12 | Mangoes 43 / ebony trees, Star anise 46, Sugar cane 17 |
| Date palm wine | 8 | `recipe.barrel.date_wine`: 12 Dates + 2 Honey | `recipe.food.date_wine` | 12 | Dates 48, Honey 11 |
| Cardamom sorghum spirit | 8 | `recipe.barrel.cardamom_sorghum_spirit`: 12 Sorghum + 1 Cardamom | `recipe.food.cardamom_sorghum_spirit` | 12 | Sorghum 44, Cardamom 46 |

**Appearance condition** (as in Part 1): a barrel recipe can enter a box only after the player knows the barrel wood's **plank recipe** and the fittings recipe of that tier (missions T5-L4, T6-L4, T7-L4 and T8-L4 for the fittings). It also needs every named ingredient source discovered, plus the known conversions (Maple syrup, Sugar). A bottling recipe additionally needs its filling recipe, so a box never hands out a bottle recipe without its barrel. The recipe unlock tier is the drink's tier.

**Premium rule:** each drink counts as one premium pair. The filling recipe carries the premium requirement (barrel wood, fittings and the regional ingredients, at least three distinct inputs), and the bottled serving gives 11-12 hearts, the tier-four premium band (section 14.4). The two-input bottling step is accepted because its barrel is the refined input.

**Pricing (proposal; barrels are priced as Misc, as in Part 1):** level-10 Q1-equivalent references continue Part 1's tier-four range (cider 5,000 up to whiskey 7,200), rising about 1,000 per tier. Within a tier, ciders and beers are lowest, wines and meads in the middle and spirits highest. Bottles keep Part 1's ratio, about 14% of the barrel reference each, so eight bottles are worth more than the barrel plus the bottles. The finished-quality multiplier (Q5 = 2.00) applies; there is no durability premium.

| Tier | Barrel references (L10, Q1-equivalent) | Bottle reference (L10) |
| --- | --- | --- |
| 5 | Pear cider 7,500; Plum wine 8,000; Maple rye whiskey 8,600 | 1,050; 1,120; 1,200 |
| 6 | Blackcurrant wine 9,000; Quince thyme mead 9,300; Apricot brandy 9,800 | 1,260; 1,300; 1,370 |
| 7 | Raspberry millet beer 10,200; Pomegranate wine 10,600; Peach brandy 11,200 | 1,430; 1,480; 1,570 |
| 8 | Date palm wine 11,800; Cardamom sorghum spirit 12,400; Spiced mango rum 13,000 | 1,650; 1,740; 1,820 |

These values are proposals for the Part 2 price tables (still open in section 6).

**Mystery-box look for tiers 5-8:** Part 1 section 14 colours the box by the tier of the region where the balloon merchant stands. Four more colour sets follow for tiers 5-8 (maple, cedar, redwood and ebony tones with the merchant's crimson and brass), presentation only. Box contents keep Part 1's rules: six pre-rolled entries, 70% current tier / 30% older tiers, the 5% recipe slot, Q5 physical rewards, and fishery seafood never as a physical reward.

**Box pool effect:** Part 2 adds **24 recipes** (12 filling + 12 bottling) to its 16 box recipes, so 40 in Part 2. With Part 1's 44 (26 + 18 barrel recipes) the combined box pool has 84 recipes; the ranch recipes (Part 1 section 13.4.2 and section 10.2) bring it to **100**. The per-slot 5% recipe chance and the six-entry box are unchanged, so each recipe becomes rarer.

### 10.2 Ranch products and premium ranch recipes for tiers 5-8 (proposal, November 5, 2026)

Part 1 section 13.4.2 defines the ranch-exclusive meats: one per tier, store-only like the fishery's seafood, used only by optional premium box recipes. Tiers 5-8 continue with one more meat per tier, chosen to fit each block's landscape and kept apart from the wild hunted animals (deer, mountain goat, buffalo, bear). Pig and peacock are pets, so they are not ranch animals.

| Exact ingredient (ID) | Material tier | Ranch stock eligibility | Proposed Q5 cash / unit |
| --- | --- | --- | --- |
| Goose meat (`ranch.goose_meat`) | 5 | A tier-five region discovered | 400 |
| Quail meat (`ranch.quail_meat`) | 6 | A tier-six region discovered | 600 |
| Ostrich meat (`ranch.ostrich_meat`) | 7 | A tier-seven region discovered | 900 |
| Guinea fowl meat (`ranch.guinea_fowl_meat`) | 8 | A tier-eight region discovered | 1,200 |

The ranch keeps Part 1's three offers per visit (two dairy/egg offers and one meat offer from any eligible tier). The meat prices stay below the tier's exclusive seafood (section 9). Like seafood, the meats never come from trail, hunting, pets or box physical rewards.

**Premium ranch recipes (mystery-box exclusive, eight recipes, two per tier).** They use the tier-four premium band (**11-12 hearts**, no stats beyond tier four) and the premium rule of section 14.4:

| Recipe ID / output | Tier | Exact ingredients -> yield | Hearts per serving | Other sources needed |
| --- | --- | --- | --- | --- |
| `recipe.food.maple_roast_goose` — Maple-roast goose | 5 | 1 Goose meat + 1 Maple syrup + 1 Maple log -> 1 | 11 | Maple sap (trees 25-30 or loose 30) + syrup conversion |
| `recipe.food.goose_pear_rye_pot` — Goose pear-rye pot | 5 | 1 Goose meat + 1 Pears + 2 Rye -> 1 | 12 | Pears 25, Rye 26 |
| `recipe.food.quail_hazelnut_parcel` — Quail hazelnut parcel | 6 | 2 Quail meat + 1 Hazelnuts + 2 Buckwheat -> 1 | 11 | Hazelnuts 33, Buckwheat 32 |
| `recipe.food.quail_blackcurrant_glaze` — Blackcurrant-glazed quail | 6 | 2 Quail meat + 1 Blackcurrants + 1 Honey -> 1 | 12 | Blackcurrants 33, Honey 11 |
| `recipe.food.sesame_ostrich_skewers` — Sesame ostrich skewers | 7 | 1 Ostrich meat + 1 Sesame oil + 1 Tarragon -> 1 | 11 | Sesame seeds 40 + oil conversion, Tarragon 40 |
| `recipe.food.saffron_ostrich_stew` — Saffron ostrich stew | 7 | 1 Ostrich meat + 1 Saffron + 2 Millet -> 1 | 12 | Saffron 42, Millet 38 |
| `recipe.food.cardamom_guinea_fowl_curry` — Cardamom guinea-fowl curry | 8 | 1 Guinea fowl meat + 1 Cardamom + 1 Mangoes -> 1 | 12 | Cardamom 46, Mangoes (ebony trees or loose 43) |
| `recipe.food.guinea_fowl_date_tagine` — Guinea-fowl date tagine | 8 | 1 Guinea fowl meat + 2 Dates + 1 Star anise -> 1 | 12 | Dates 48, Star anise 46 |

Eligibility, the recipe-book source label and the claim rules follow Part 1 section 13.4.2 and section 4 here. **Box pool effect:** Part 2's box pool grows to 16 + 24 barrel + 8 ranch = **48**. With Part 1's 52, the combined pool is 100 recipes; the keepsakes in section 10.3 bring it to 116.

### 10.3 Precious-metal and gem keepsakes for tiers 5-8 (mystery-box exclusive, proposal, user request November 5, 2026)

Nine premium **Misc** recipes continuing Part 1 section 9.5.1, with the same premium rule (section 14.4) and Misc rules. Part 2 adds gems but no precious metal, and gold and silver (tiers four and three) fall outside the windows from tier six on. So the keepsakes use each tier's own metal:
- **Tier 5:** gold, still inside the {4,5} window, and magnetite, the hematite of real bead jewelry.
- **Tier 6:** a **Nickel silver** alloy (below).
- **Tier 7:** chromium fittings.
- **Tier 8:** titanium, common in modern jewelry.

**Gems are processed first (user decision, November 6, 2026):** every Part 2 gem has a cut or polished version (section 14.1), and the keepsakes and the gem-trimmed Set B garments use it. This matches Part 1, where Cut quartz, turquoise, ruby, sapphire and diamond feed the better recipes. Raw gems keep their simpler uses: the regional Misc tokens of section 8 and the house deposits of section 16.1.

| Recipe ID / output | Tier | Exact ingredients -> 1 | Sources needed |
| --- | --- | --- | --- |
| `recipe.misc.amber_gold_locket` — Amber gold locket | 5 | 1 Gold ingot + 1 Polished amber + 1 Fine ramie cloth | Gold ore 20 (tier four, inside the {4,5} window), Amber 26, Willow bast 30 |
| `recipe.misc.magnetite_bead_bracelet` — Magnetite bead bracelet | 5 | 3 Magnetite + 1 Polished agate + 1 Willow cord | Magnetite 29, Agate 29, Willow bast 30 |
| `recipe.misc.celestite_silk_tiara` — Celestite silk tiara | 6 | 1 Cut celestite + 1 Silk cloth + 1 Nickel silver ingot | Celestite 34, Silk cocoons 34, Nickel ore 32, Zinc ore 27 |
| `recipe.misc.almandine_signet_ring` — Almandine signet ring | 6 | 1 Cut almandine + 1 Nickel silver ingot + 1 Cashmere cloth | Almandine 36, Nickel ore 32, Goat down 35 |
| `recipe.misc.malachite_inlay_casket` — Malachite inlay casket | 7 | 2 Redwood planks + 1 Polished malachite + 1 Chromium fittings | Malachite 40, Chromium ore 38 |
| `recipe.misc.tourmaline_star_brooch` — Tourmaline star brooch | 7 | 1 Cut tourmaline + 1 Chromium fittings + 1 Fine sisal cloth | Tourmaline 41, Indigo leaves 41 |
| `recipe.misc.titanium_moonstone_circlet` — Titanium moonstone circlet | 8 | 1 Titanium ingot + 1 Polished moonstone + 1 Fine lotus cloth | Titanium ore 44, Moonstone 46 |
| `recipe.misc.opal_aquamarine_pendant` — Opal-aquamarine pendant | 8 | 1 Polished opal + 1 Cut aquamarine + 1 Titanium fittings | Opal 48, Aquamarine 47 |
| `recipe.misc.jewelled_ebony_music_box` — Jewelled ebony music box | 8 | 2 Ebony planks + 1 Polished opal + 1 Titanium fittings | Opal 48, Titanium ore 44 |

**New Material:** `recipe.material.nickel_silver_ingot`, tier six: **2 Nickel ingots + 1 Zinc ingot -> 2 Nickel silver ingots**. It is a real silver-coloured alloy, with no precious metal needed. Fixed material timer, no mastery. Like every recipe it is a mission reward (T6-L10, section 15.2).

**Eligibility** follows Part 1 section 9.5.1. **Box pool effect:** Part 2's pool grows to **57** (16 + 24 barrel + 8 ranch + 9 keepsakes); with Part 1's 59 the combined pool is **116 recipes**, or 131 optional recipes with the 15 bakery ones.

## 11. New premium bakery recipes

| Recipe ID / food | Tier | Exact inputs -> 1 | Hearts | Gold/serving | Gold/full stack -10% | Gold/single recipe |
| --- | --- | --- | --- | --- | --- | --- |
| `recipe.food.maple_pear_tart` — Maple pear tart | 5 | 2 Wheat flour + 2 Pears + 1 Maple syrup + 1 Cow butter | 12 | 11 | 9900 | 110 |
| `recipe.food.plum_chestnut_brioche` — Plum chestnut brioche | 5 | 2 Wheat flour + 2 Plums + 1 Chestnut + 1 Cow butter | 12 | 11 | 9900 | 110 |
| `recipe.food.apricot_hazelnut_cake` — Apricot hazelnut cake | 6 | 2 Wheat flour + 2 Apricots + 1 Hazelnut + 1 Cow cream | 12 | 12 | 10800 | 120 |
| `recipe.food.blackcurrant_quince_pastry` — Blackcurrant quince pastry | 6 | 2 Wheat flour + 2 Blackcurrants + 1 Quince + 1 Cow butter | 12 | 12 | 10800 | 120 |
| `recipe.food.peach_pecan_galette` — Peach pecan galette | 7 | 2 Wheat flour + 2 Peaches + 1 Pecan + 1 Cow butter | 12 | 13 | 11700 | 130 |
| `recipe.food.raspberry_saffron_cake` — Raspberry saffron cake | 7 | 2 Wheat flour + 2 Raspberries + 1 Saffron + 1 Cow cream | 12 | 13 | 11700 | 130 |
| `recipe.food.mango_passionfruit_tart` — Mango passion fruit tart | 8 | 2 Wheat flour + 2 Mangoes + 1 Passion fruit + 1 Cow cream | 12 | 14 | 12600 | 140 |
| `recipe.food.dragonfruit_cardamom_cake` — Dragon fruit cardamom cake | 8 | 2 Wheat flour + 2 Dragon fruit + 1 Cardamom + 1 Cow butter | 12 | 14 | 12600 | 140 |



Eight additions, two per tier 5–8. Combined bakery catalog: **15**, distributed 1/2/2/2/2/2/2/2 across tiers 1–8. Keep the 1% substitution per eligible premium slot: one unlock of the same food's recipe replaces its stack, never 1,000 scrolls. Recipe price is 10x the undiscounted serving; full food stack costs 90% of 1,000 servings. Keep two premium slots/one common, persisted until departure/return. Bought food teaches no recipe. Twelve hearts is progressive total recovery, not a new cap; these variants add ingredient/economy variety without forcing stronger recovery or capacity.


## 12. Exact gates, dependencies and acceptance

**Tree-ingredient recipe coverage review:** each required expansion tree ingredient already has at least one exact input recipe, and every refinement has a finished-food use. Retain these definitions rather than creating duplicate unlocks.

| Canonical harvested ingredient ID | Exact input recipe / useful chain | Other existing food uses |
| --- | --- | --- |
| `resource.maple_sap` | `recipe.material.maple_syrup`: 3 Maple sap + 1 Maple branch -> 1 Maple syrup; then `recipe.food.chicory_maple_drink`: 2 Chicory root + 1 Maple syrup + 1 Cow milk -> 1 | `recipe.food.maple_pear_tart`; `recipe.food.venison_pear_pot` use the syrup |
| `resource.apricots` | `recipe.food.apricot_buckwheat_pot`: 2 Apricots + 2 Buckwheat + 1 Cow milk -> 1 | `recipe.food.apricot_hazelnut_cake`; `recipe.food.goat_apricot_pot` |
| `resource.peaches` | `recipe.food.peach_millet_pot`: 2 Peaches + 2 Millet + 1 Cow milk -> 1 | `recipe.food.peach_pecan_galette` |
| `resource.mangoes` | `recipe.food.mango_sorghum_pot`: 2 Mangoes + 2 Sorghum + 1 Cow milk -> 1 | `recipe.food.mango_passionfruit_tart` |

Tree and loose outputs use these same canonical IDs. Every listed recipe accepts either source and valid mixtures of same-ID stacks; do not author a separate “tree apricot” or provenance-specific recipe. Raw harvested ingredients are ready for crafting, but not prepared food for automatic consumption. Pending craft products remain unavailable until ready. Apply ordinary exact quantities, reservation, quality/profile and output-quality rules. Maple sap must be converted to `item.maple_syrup` where syrup is required; Birch sap is a different ID and cannot substitute. Source eligibility remains OR across authorized sources for each ingredient and AND across different required ingredients.

**Recipe acceptance checks:** for each row, craft with tree-sourced ingredients, loose-sourced ingredients and a valid mixture; output/yield must agree for the same craft profile. For the four Part 1 tree-only ingredients, use their TreeHarvest source and exact conversion chain. Reject wrong sap/fruit/flour IDs, missing quantities, reserved inputs and pending refinements without spending anything. Check raw-to-refined-to-Food chains, x1/x100/x1000 costs, wood fuel and exact-once consumption/save recovery. Harvested Q5 ingredients must not force crafted Q5. Confirm every configured tree ingredient has an authored input recipe and a reachable finished Food use; apply that content check whenever a second bonus ingredient is added. This review adds no recipe IDs and leaves bakery/box counts unchanged.

Each recipe must author `RequiredRegionIds`, exact shop/interaction source IDs and conversion prerequisites. Resolve these from the region table and recursively through each refined input: Maple syrup needs its conversion plus access to Maple sap and Maple branches. Sap is available from discovered Maple trees in 25–30 or loose in 30; branches come from those trees or loose in 25. One valid source per ingredient suffices. Merely entering tier 5 is insufficient. Part 1 ingredients retain their exact sources: Wheat 6, Honey 11, Sugar cane 17 and accessible exact dairy. Source access, not currently holding ingredients, gates the roll. Fish eligibility is the union of discovered regional pairs, not the entire tier catalog.

Required negative cases: block maple pear tart without Pears 25, an accessible Maple sap source (tree 25–30 or loose 30), its conversion and Wheat 6/dairy; plum brioche without 27 or 28; apricot cake without Hazelnuts 33 and Apricots (Cedar tree in 31–36 or loose 31); blackcurrant pastry without 33 or 36; peach galette without Pecans 39 and Peaches (Redwood tree in 37–42 or loose 37); raspberry cake without 39 or 42; mango tart without Passion fruit 45 and Mangoes (Ebony tree in 43–48 or loose 43); dragon fruit cake without 46 or 47. Scallop/Mussel/Octopus/Oyster require region 25/31/37/43 and reachable fishery; each dish additionally requires its cereal/herb/dairy sources. Trophy crafts require discovered exact hunting sources, eligible tools and accessible refinements.

Check gates at generation and before payment/unlock/claim. Gifts, mastery, quality, gold, purchased refined goods, previews and prepared food never grant discovery. Persist IDs/catalog version, offers/quotes, visits, reservations, paid manifests and recipe knowledge. Reject invalid unlocks without charging/consuming the reward; preserve paid manifests until a valid claim without rerolling. Activate tiers/catalog versions explicitly; disabled expansion data stays outside current catalogs.

P2.1–P2.4 incorporate these tables incrementally: resources/refinements/source gates first; then route/tools/hunting/fishing; then missions and shop/box/bakery integration; finally migration/regressions. Exact tools/tasks still need authoring; none of these tables claims implementation completion. Before activation, check 24 regions/72 distinct resources/18 per tier, six unique pairs per tier and 16 fish; 16 new box and eight new bakery IDs with no collisions; each ingredient with all its alternative sources unavailable, and each alternative source independently available. Check ×10/−10% prices, mastery-one knowledge, crafted quality independent of Q5 inputs, exact bulk/fuel costs and no reload duplication. Combined two-source collection: **131 optional recipes** (116 box recipes, including the 21 barrel-filling and 21 bottling recipes, the 16 ranch recipes and the 16 precious-metal and gem keepsakes of both parts, plus 15 bakery recipes); the truffle mission and ordinary regional recipes are additional and excluded from that count.


## 13. Equipment and tools for tiers 5-8 (proposal, November 5, 2026)

**No new stats (user, November 5, 2026):** tier four is the stats limit. Tiers five to eight add **no** new capability values: hats stay at most 12 hearts, boots 4.5 m/s, shirts 10 pockets, pants 10 hooks, backpacks 14 x 4. The 26-heart leg budget and the 12-heart meter are also unchanged. What grows with each tier:
- **durability**
- **tool reach**: a tool works up to two tiers above its own (Part 1 section 8.2), so later resources need later tools
- **recipe value**
- **variety**

**Two equipment sets per tier, as in Part 1 section 9.6.1:**
- **Set A** is built from the block's first three regions (25-27, 31-33, 37-39, 43-45).
- **Set B** needs at least one material from the last three regions.
- Both sets have the tier-four capabilities (Set B values: 12 hearts, 4.5 m/s, 10 pockets, 10 hooks, 14 x 4). They differ in sources, art and **durability: Set B has +25% base durability over Set A**.
- Proposed durability targets at L10 Q5, continuing Part 1's tier-four Set B at about 14 km:

| Tier | Set A (km) | Set B (km) |
| --- | --- | --- |
| 5 | 14.4 | 18 |
| 6 | 16.8 | 21 |
| 7 | 19.2 | 24 |
| 8 | 21.6 | 27 |

**Balloon special gear** keeps its tier 1-4 catalog. In tiers five to eight its tier roll treats tier four as the highest tier, so no new special items or stats are needed.

### 13.1 Set recipes

IDs are `recipe.wearable.<id>` -> `item.<id>`. Branches follow Part 1: Gatherer hats, Lumberjack boots, Hunter shirts, Explorer pants and backpacks. All inputs stay inside the tier window ({4,5}, {5,6}, {6,7}, {7,8}).

| Tier / set | Hat | Boots | Shirt | Pants | Backpack |
| --- | --- | --- | --- | --- | --- |
| 5 A Vale | Deer-hide hat (`deer_hide_hat`): 2 Deer leather + 1 Ramie cloth | Copperleaf boots (`copperleaf_boots`): 2 Deer leather + 1 Zinc fittings | Ramie shirt (`ramie_shirt`): 3 Ramie cloth + 1 Deer leather | Ramie trousers (`ramie_trousers`): 3 Ramie cloth + 1 Zinc fittings | Vale pack (`vale_pack`): 4 Ramie cloth + 3 Deer leather + 2 Maple planks + 2 Zinc fittings |
| 5 B Sunset | Sunset hat (`sunset_hat`): 2 Fine ramie cloth + 1 Fine deer leather + 1 Polished agate | Sunset boots (`sunset_boots`): 2 Fine deer leather + 1 Magnetite fittings | Sunset shirt (`sunset_shirt`): 3 Fine ramie cloth + 1 Fine deer leather | Sunset trousers (`sunset_trousers`): 3 Fine ramie cloth + 1 Magnetite fittings + 1 Willow cord | Orchard frame pack (`orchard_frame_pack`): 3 Fine ramie cloth + 2 Fine deer leather + 2 Maple beams + 2 Willow cord |
| 6 A Terrace | Goat-hide cap (`goat_hide_cap`): 2 Mountain goat leather + 1 Jute cloth | Terrace boots (`terrace_boots`): 2 Mountain goat leather + 1 Nickel fittings | Jute shirt (`jute_shirt`): 3 Jute cloth + 1 Mountain goat leather | Jute trousers (`jute_trousers`): 3 Jute cloth + 1 Nickel fittings | Terrace pack (`terrace_pack`): 4 Jute cloth + 3 Mountain goat leather + 2 Cedar planks + 2 Nickel fittings |
| 6 B Silken | Cashmere hood (`cashmere_hood`): 2 Cashmere cloth + 1 Silk cloth | Escarpment boots (`escarpment_boots`): 2 Fine goat leather + 1 Cashmere cloth + 1 Nickel fittings | Silk shirt (`silk_shirt`): 3 Silk cloth + 1 Fine goat leather | Cashmere trousers (`cashmere_trousers`): 3 Cashmere cloth + 1 Nickel fittings + 1 Cut almandine | Foothill frame pack (`foothill_frame_pack`): 3 Silk cloth + 2 Fine goat leather + 2 Cedar beams + 2 Cashmere cloth |
| 7 A Bluff | Buffalo-hide hat (`buffalo_hide_hat`): 2 Buffalo leather + 1 Sisal cloth | Bluff boots (`bluff_boots`): 2 Buffalo leather + 1 Chromium fittings | Sisal shirt (`sisal_shirt`): 3 Sisal cloth + 1 Buffalo leather | Sisal trousers (`sisal_trousers`): 3 Sisal cloth + 1 Chromium fittings | Ochre pack (`ochre_pack`): 4 Sisal cloth + 3 Buffalo leather + 2 Redwood planks + 2 Chromium fittings |
| 7 B Painted | Painted hat (`painted_hat`): 2 Fine sisal cloth + 1 Fine buffalo leather + 1 Cut tourmaline | Gorge boots (`gorge_boots`): 2 Fine buffalo leather + 1 Chromium fittings + 1 Sisal cord | Indigo shirt (`indigo_shirt`): 3 Fine sisal cloth + 1 Fine buffalo leather | Painted trousers (`painted_trousers`): 3 Fine sisal cloth + 1 Chromium fittings + 1 Polished malachite | Horizon frame pack (`horizon_frame_pack`): 3 Fine sisal cloth + 2 Fine buffalo leather + 2 Redwood beams + 2 Sisal cord |
| 8 A Lagoon | Bear-fur hat (`bear_fur_hat`): 2 Bear felt + 1 Lotus cloth | Nightbloom boots (`nightbloom_boots`): 2 Bear felt + 1 Titanium fittings | Lotus shirt (`lotus_shirt`): 3 Lotus cloth + 1 Bear felt | Lotus trousers (`lotus_trousers`): 3 Lotus cloth + 1 Titanium fittings | Lagoon pack (`lagoon_pack`): 4 Lotus cloth + 3 Bear felt + 2 Ebony planks + 2 Titanium fittings |
| 8 B Starlit | Moonstone hat (`moonstone_hat`): 2 Fine lotus cloth + 1 Fine bear felt + 1 Polished moonstone | Starlit boots (`starlit_boots`): 2 Fine bear felt + 1 Titanium fittings + 1 Lotus cord | Starlit shirt (`starlit_shirt`): 3 Fine lotus cloth + 1 Fine bear felt | Prismatic trousers (`prismatic_trousers`): 3 Fine lotus cloth + 1 Titanium fittings + 1 Cut aquamarine | Lastlight frame pack (`lastlight_frame_pack`): 3 Fine lotus cloth + 2 Fine bear felt + 2 Ebony beams + 1 Polished opal |

The Set B materials that gate each tier:
- **Tier 5:** Fine ramie cloth (Willow cord, region 30), Fine deer leather (Safflower oil, 28), Polished agate and Magnetite (29).
- **Tier 6:** Silk (34), Cashmere (35), Cut almandine (36).
- **Tier 7:** Fine buffalo leather (Sesame oil, 40), Fine sisal cloth (Indigo dye, 41), Cut tourmaline (41) and Polished malachite (40).
- **Tier 8:** Fine bear felt (Black sesame oil, 47), Polished moonstone (46), Cut aquamarine (47), Polished opal (48).

### 13.2 Tools

Part 1 rules apply: Normal tools at every tier; an Elite consumes one same-tier Normal plus 2 previous-tier advanced units; x1 only; the effectiveness matrix (Part 1 section 8.2) is unchanged. The hunting tool is a bow from tier three on. Durability baselines continue Part 1's formulas: Normal axe and rod `10 + 10 * (T - 1)`, Normal bow `6 + 10 * (T - 1)`, Elite twice the Normal.

| Tier | Normal axe | Normal bow | Normal rod | Elite inputs (axe / bow / rod) | Normal / Elite uses (axe, bow, rod) |
| --- | --- | --- | --- | --- | --- |
| 5 Maple | 2 Maple branches + 1 Zinc ingot + 1 Ramie cord | 2 Maple branches + 1 Deer hide + 1 Ramie cord | 2 Maple branches + 1 Zinc fittings + 1 Ramie cord | 2 Oak planks / 2 Fine boar leather / 2 Bamboo cloth | 50, 46, 50 / 100, 92, 100 |
| 6 Cedar | 2 Cedar branches + 1 Nickel ingot + 1 Jute cord | 2 Cedar branches + 1 Mountain goat hide + 1 Jute cord | 2 Cedar branches + 1 Nickel fittings + 1 Jute cord | 2 Maple planks / 2 Fine deer leather / 2 Fine ramie cloth | 60, 56, 60 / 120, 112, 120 |
| 7 Redwood | 2 Redwood branches + 1 Chromium ingot + 1 Sisal cord | 2 Redwood branches + 1 Buffalo hide + 1 Sisal cord | 2 Redwood branches + 1 Chromium fittings + 1 Sisal cord | 2 Cedar planks / 2 Fine goat leather / 2 Fine jute cloth | 70, 66, 70 / 140, 132, 140 |
| 8 Ebony | 2 Ebony branches + 1 Titanium ingot + 1 Lotus cord | 2 Ebony branches + 1 Bear fur + 1 Lotus cord | 2 Ebony branches + 1 Titanium fittings + 1 Lotus cord | 2 Redwood planks / 2 Fine buffalo leather / 2 Fine sisal cloth | 80, 76, 80 / 160, 152, 160 |

Tier four already unlocks the tier-five Elite inputs (Oak plank T4-L1, Fine boar leather T4-H5, Bamboo cloth T4-E11), so no extra tier-four bridge missions are needed. Each later block unlocks the inputs for the next block's Elites (section 15).

### 13.3 Timers (authored, not extrapolated)

Proposed level-one x1 base durations, keeping Part 1's rule that a Tool or Food timer stays under one hour (Food: x1000 at L1 stays under 54 minutes):

| Tier | Tool | Food | Wearable | Misc |
| --- | --- | --- | --- | --- |
| 5 | 8 min | 12 min | 3 h | 1.5 h |
| 6 | 12 min | 14 min | 4 h | 2 h |
| 7 | 16 min | 16 min | 5 h | 2.5 h |
| 8 | 20 min | 18 min | 6 h | 3 h |

Material recipes keep fixed authored timers; gatherable outputs stay instant.

## 14. Added recipes for tiers 5-8 (proposal, November 5, 2026)

The same ingredient rule as Part 1 section 9.6 applies: one to three significant real ingredients that exist in the catalog (four only for backpacks and the existing spice pouch), exact IDs, and the tier window. **Every recipe below and in section 8 is a mission reward** (section 15); none is a discovery unlock.

### 14.1 Materials

| Recipe / output | Tier | Exact inputs -> yield |
| --- | --- | --- |
| Ramie cord / Jute cord / Sisal cord / Lotus cord | 5 / 6 / 7 / 8 | 3 Ramie / Jute / Sisal / Lotus fiber -> 2 |
| Maple beam / Cedar beam / Redwood beam / Ebony beam | 5 / 6 / 7 / 8 | 4 planks of that wood + 1 Ramie / Jute / Sisal / Lotus cord -> 1 |
| Zinc fittings / Magnetite fittings / Nickel fittings / Chromium fittings / Titanium fittings | 5 / 5 / 6 / 7 / 8 | 1 matching ingot -> 3 |
| Nickel silver ingot | 6 | 2 Nickel ingots + 1 Zinc ingot -> 2 |
| Fine deer leather | 5 | 2 Deer leather + 1 Safflower oil -> 1 |
| Fine goat leather | 6 | 2 Mountain goat leather + 1 Safflower oil -> 1 |
| Fine buffalo leather | 7 | 2 Buffalo leather + 1 Sesame oil -> 1 |
| Fine bear felt | 8 | 2 Bear felt + 1 Black sesame oil -> 1 |
| Fine ramie cloth | 5 | 3 Ramie cloth + 1 Willow cord -> 1 |
| Fine jute cloth | 6 | 3 Jute cloth + 1 Willow cord -> 1 |
| Fine sisal cloth | 7 | 3 Sisal cloth + 1 Indigo dye -> 1 |
| Fine lotus cloth | 8 | 3 Lotus cloth + 1 Sisal cord -> 1 |
| Polished amber / Polished agate | 5 | 2 Amber / 2 Agate -> 1 |
| Cut celestite / Cut almandine | 6 | 2 Celestite / 2 Almandine -> 1 |
| Polished malachite / Cut tourmaline | 7 | 2 Malachite / 2 Tourmaline -> 1 |
| Polished moonstone / Cut aquamarine / Polished opal | 8 | 2 Moonstone / 2 Aquamarine / 2 Opal -> 1 |

**Gem processing (user decision, November 6, 2026):** works like Part 1's Cut quartz and Cut turquoise (2 raw -> 1, camp/home, fixed material timer). Faceted gems (celestite, almandine, tourmaline, aquamarine) are **cut**. Opaque or soft stones shaped as cabochons or beads (amber, agate, malachite, moonstone, opal) are **polished**. Each is its own Material ID inside its tier, and each is a mission reward (section 15).

**Correction:** planks use **3 logs -> 1**, as in Part 1 section 9.2 (section 8 previously said 2).

### 14.2 Food: fish dishes, meat dishes and hearts

**Fish (user, November 5, 2026):** each species is a **collection mission** that teaches a **varied dish**. Tiers five to eight have no plain fish roasts; section 9's roast column is replaced by these dishes.

| Species | Tier | Dish (`recipe.food.<id>`) | Exact inputs -> yield |
| --- | --- | --- | --- |
| Smallmouth bass | 5 | Bass pear bake (`bass_pear_bake`) | 1 Smallmouth bass + 1 Pears + 1 Cow butter -> 1 |
| Sauger | 5 | Sauger rye chowder (`sauger_rye_chowder`) | 1 Sauger + 1 Rye + 1 Cow milk -> 1 |
| White crappie | 5 | Crappie chestnut cakes (`crappie_chestnut_cakes`) | 1 White crappie + 1 Chestnuts + 1 Egg -> 2 |
| Brook trout | 5 | Calendula brook-trout bake (`calendula_brook_trout_bake`) | 1 Brook trout + 1 Calendula + 1 Cow butter -> 1 |
| Arctic char | 6 | Arctic char apricot bake (`arctic_char_apricot_bake`) | 1 Arctic char + 1 Apricots + 1 Cow butter -> 1 |
| Dolly Varden | 6 | Dolly Varden buckwheat parcel (`dolly_varden_buckwheat_parcel`) | 1 Dolly Varden + 2 Buckwheat + 1 Cow butter -> 1 |
| Whitefish | 6 | Whitefish hazelnut cakes (`whitefish_hazelnut_cakes`) | 1 Whitefish + 1 Hazelnuts + 1 Egg -> 2 |
| Grayling | 6 | Grayling fennel stew (`grayling_fennel_stew`) | 1 Grayling + 1 Fennel seeds + 1 Rock salt -> 1 |
| Flounder | 7 | Flounder millet cakes (`flounder_millet_cakes`) | 1 Flounder + 1 Millet + 1 Egg -> 2 |
| Striped bass | 7 | Striped-bass pecan crust (`striped_bass_pecan_crust`) | 1 Striped bass + 1 Pecans + 1 Wheat flour -> 1 |
| Mullet | 7 | Sesame mullet rice (`sesame_mullet_rice`) | 1 Mullet + 1 Sesame seeds + 1 Rice -> 1 |
| Cobia | 7 | Cobia saffron stew (`cobia_saffron_stew`) | 1 Cobia + 1 Saffron + 1 Amaranth seeds -> 1 |
| Grouper | 8 | Grouper mango bake (`grouper_mango_bake`) | 1 Grouper + 1 Mangoes + 1 Cow butter -> 1 |
| Mahi-mahi | 8 | Mahi-mahi passion fruit bowl (`mahi_mahi_passion_fruit_bowl`) | 1 Mahi-mahi + 1 Passion fruit + 1 Sorghum -> 1 |
| Wahoo | 8 | Black-sesame wahoo (`black_sesame_wahoo`) | 1 Wahoo + 1 Black sesame seeds + 1 Rice -> 1 |
| Amberjack | 8 | Amberjack teff broth (`amberjack_teff_broth`) | 1 Amberjack + 2 Teff + 1 Star anise -> 1 |

**Meat:** each tier has its first hunting dish (section 8, revised November 7, 2026: no plain roasts) and one second dish. **Revised November 7, 2026:** the four second dishes were all the same "2 meat + 2 grain + 1 Rock salt" stew. Only the venison stew keeps that form; the others become different dishes (codes and requests unchanged):
- Venison rye stew: 2 Raw deer meat + 2 Rye + 1 Rock salt -> 2 (kept)
- Mountain goat thyme pie (replaces Mountain goat thyme stew, `recipe.food.goat_thyme_pie`): 2 Raw mountain goat meat + 2 Wheat flour + 1 Mountain thyme -> 2
- Buffalo millet patties (replaces Buffalo millet stew, `recipe.food.buffalo_millet_patties`): 2 Raw buffalo meat + 2 Millet + 1 Egg -> 2
- Bear sorghum dumplings (replaces Bear sorghum stew, `recipe.food.bear_sorghum_dumplings`): 2 Raw bear meat + 2 Sorghum + 1 Wheat flour -> 2

Wheat flour and Egg come in through the Food pantry.

**Hearts (no augmented stats):** mission foods in tiers five to eight follow the ordinary mission range and top out at **6 hearts per serving**, the tier-four level:
- section 8's regional pots, the Chicory maple drink and the four first hunting dishes become **6** (the drink and the two-serving Smoked buffalo jerky **5**)
- the fish dishes and stews above give 6, or 5 when the yield is 2
- premium box recipes use Part 1's tier-four band, **11-12** (Part 1 section 9.4.2: premium hearts rise with tier up to 11-12 at tier four, and stats stop rising there); bakery recipes keep 12

Tier-four meals already cover the 26-heart leg budget, and that budget does not grow.

### 14.3 Misc

Section 8's fourteen regional Misc recipes stay, now as mission rewards. Each tier adds one trophy carving (Misc, 2 planks + 1 trophy). The box keeps the richer three-ingredient trophy tokens.
- Maple deer sculpture: 2 Maple planks + 1 Deer antler
- Cedar goat carving: 2 Cedar planks + 1 Mountain goat horn
- Redwood buffalo totem: 2 Redwood planks + 1 Buffalo horn
- Ebony bear carving: 2 Ebony planks + 1 Bear claw

### 14.4 Shop and mystery-box rule

As in Part 1 section 9.6.6, a recipe can be a shop or box unlock only if it is **premium**:
- **Food:** 11-12 hearts (the tier-four premium band), with at least three distinct ingredients, one refined or from another source.
- **Misc:** at least three ingredients including a refined material.

The eight bakery recipes, the trophy tokens and the barrel drinks (section 10.1) already qualify. The four plain exclusive-seafood roasts in section 10 do not, so they are **reworked** there into composed premium dishes.

## 15. Mission plan for tiers 5-8 (proposal, November 5, 2026)

The Part 1 section 11.6 conventions apply:
- one recipe per mission
- missions open by **mission region** (the region column)
- a mission stays locked until its reward is craftable
- the Normal tool before its Elite, and the previous-tier refinement before an Elite
- Set A before Set B

**Quantities never exceed the campaign ceiling:** **150** units for stackable requests and **40** instances for tools and garments in every tier from five to eight (the tier-four values). **Bold** rewards are the equipment sets.

**Camps per region (confirmed, November 5, 2026):** 16-20 per region as in tier four (section 7), 104-110 per tier, so about 0.4 missions per camp.

**Execution order correction, November 6, 2026:** stable mission codes are identifiers; use explicit branch order following table row order within each branch, not numeric code sorting. Tier-six Lumberjack executes **T6-L1 -> T6-L2 -> T6-L3 -> T6-L4 -> T6-L10 -> T6-L5 -> T6-L6 -> T6-L7 -> T6-L8 -> T6-L9**. Nickel silver therefore unlocks in mission region 32 after Nickel ingot and the previous tier's Zinc ingot, without waiting for region 36. Gem-processing codes inserted into the tables also execute at their displayed positions. Apply Part 1 section 11.6.1's versioned migration contract if affected IDs already exist in test saves. Pending review must check every request and reward dependency, not only regions or code numbers.

### 15.1 Tier five (regions 25-30)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T5-G1 | 25 | Maple branches | Maple log | |
| T5-G2 | 26 | Pears | Pear rye porridge | Request revised November 7, 2026 (Rye stays with T5-H3) |
| T5-G3 | 26 | Gneiss | Amber gneiss token | Request revised November 7, 2026 (Amber stays with T5-G12) |
| T5-G12 | 26 | Amber | Polished amber | |
| T5-G4 | 27 | Plums | **Deer-hide hat (5A hat)** | Deer leather (T5-H2), Ramie cloth (T5-E3) |
| T5-G5 | 28 | Chestnuts | Plum chestnut pot | |
| T5-G6 | 28 | Calendula | Calendula chestnut keepsake | Ramie cloth (T5-E3) |
| T5-G7 | 28 | Safflower seeds | Safflower oil | |
| T5-G8 | 29 | Gypsum | Agate gypsum medallion | Request revised November 7, 2026 (Agate stays with T5-G13) |
| T5-G13 | 29 | Agate | Polished agate | |
| T5-G9 | 30 | Maple sap | Maple syrup | Sap from maple trees or loose in 30 |
| T5-G10 | 30 | Chicory root | Chicory maple drink | |
| T5-G11 | 30 | Chicory maple drink | **Sunset hat (5B hat)** | Fine ramie cloth (T5-E12), Fine deer leather (T5-H7), Polished agate (T5-G13) |
| T5-L1 | 25 | Maple logs | Maple plank | |
| T5-L2 | 27 | Zinc ore | Zinc ingot | |
| T5-L3 | 27 | Zinc ingots | Normal maple axe | Ramie cord (T5-E4) |
| T5-L4 | 27 | Zinc ore | Zinc fittings | |
| T5-L5 | 27 | Maple planks | Maple beam | Ramie cord (T5-E4) |
| T5-L6 | 27 | Maple beams | **Copperleaf boots (5A boots)** | Deer leather (T5-H2) |
| T5-L7 | 28 | 40 Normal maple axes | Elite maple axe | Oak plank (T4-L1) |
| T5-L8 | 29 | Magnetite | Magnetite ingot | |
| T5-L9 | 29 | Magnetite ingots | Magnetite fittings | |
| T5-L10 | 30 | Magnetite fittings | **Sunset boots (5B boots)** | Fine deer leather (T5-H7) |
| T5-H1 | 25 | Raw deer meat | Maple-sap venison steak | Hunted with the oak bow (one tier above); Maple sap from maple trees |
| T5-H2 | 25 | Deer hide | Deer leather | |
| T5-H3 | 26 | Rye | Venison rye stew | |
| T5-H4 | 27 | Deer hide | Normal maple bow | Ramie cord (T5-E4) |
| T5-H5 | 27 | Deer leather | **Ramie shirt (5A shirt)** | Ramie cloth (T5-E3) |
| T5-H6 | 28 | 40 Normal maple bows | Elite maple bow | Fine boar leather (T4-H5) |
| T5-H7 | 28 | Safflower oil | Fine deer leather | Safflower oil (T5-G7) |
| T5-H8 | 28 | Deer antlers | Maple deer sculpture | |
| T5-H9 | 30 | Fine deer leather | **Sunset shirt (5B shirt)** | Fine ramie cloth (T5-E12) |
| T5-E1 | 25 | Smallmouth bass | Bass pear bake | Caught with the oak rod (one tier above) |
| T5-E2 | 26 | Sauger | Sauger rye chowder | |
| T5-E3 | 27 | Ramie fiber | Ramie cloth | |
| T5-E4 | 27 | Ramie fiber | Ramie cord | |
| T5-E5 | 27 | Ramie cloth | **Ramie trousers (5A pants)** | Zinc fittings (T5-L4) |
| T5-E6 | 27 | Ramie cord | Normal maple rod | Zinc fittings (T5-L4) |
| T5-E7 | 28 | 40 Ramie trousers | **Vale pack (5A backpack)** | Maple planks (T5-L1) |
| T5-E8 | 28 | 40 Normal maple rods | Elite maple rod | Bamboo cloth (T4-E11) |
| T5-E9 | 28 | White crappie | Crappie chestnut cakes | |
| T5-E10 | 28 | Brook trout | Calendula brook-trout bake | |
| T5-E11 | 30 | Willow bast | Willow cord | |
| T5-E12 | 30 | Willow cord | Fine ramie cloth | |
| T5-E13 | 30 | Fine ramie cloth | **Sunset trousers (5B pants)** | Magnetite fittings (T5-L9) |
| T5-E14 | 30 | 40 Sunset trousers | **Orchard frame pack (5B backpack)** | Maple beams (T5-L5), Fine deer leather (T5-H7) |

### 15.2 Tier six (regions 31-36)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T6-G1 | 31 | Cedar branches | Cedar log | |
| T6-G2 | 32 | Buckwheat | Apricot buckwheat pot | Apricots from cedar trees or loose in 31 |
| T6-G3 | 32 | Apricots | **Goat-hide cap (6A hat)** | Mountain goat leather (T6-H2), Jute cloth (T6-E2) |
| T6-G4 | 33 | Blackcurrants | Blackcurrant hazelnut pot | |
| T6-G5 | 33 | Kaolin | Kaolin hazelnut charm | Jute cloth (T6-E2) |
| T6-G6 | 34 | Mulberry leaves | Mulberry celestite pouch | Silk cloth (T6-E12) |
| T6-G9 | 34 | Celestite | Cut celestite | |
| T6-G7 | 36 | Quinces | Quince thyme compote | |
| T6-G8 | 36 | Hazelnuts | **Cashmere hood (6B hat)** | Cashmere cloth (T6-E14), Silk cloth (T6-E12) |
| T6-G10 | 36 | Almandine | Cut almandine | Listed last; opens with region 36, before Cashmere trousers (T6-E16) |
| T6-L1 | 31 | Cedar logs | Cedar plank | |
| T6-L2 | 32 | Nickel ore | Nickel ingot | |
| T6-L3 | 32 | Nickel ingots | Normal cedar axe | Jute cord (T6-E3) |
| T6-L4 | 32 | Nickel ore | Nickel fittings | |
| T6-L10 | 32 | Nickel ingots | Nickel silver ingot | Zinc ingot (T5-L2), Nickel ingot (T6-L2); executes after T6-L4 and before T6-L5 |
| T6-L5 | 32 | Cedar planks | Cedar beam | Jute cord (T6-E3) |
| T6-L6 | 32 | Cedar beams | **Terrace boots (6A boots)** | Mountain goat leather (T6-H2) |
| T6-L7 | 33 | 40 Normal cedar axes | Elite cedar axe | Maple plank (T5-L1) |
| T6-L8 | 36 | Pumice | Almandine pumice carving | |
| T6-L9 | 36 | Fennel seeds | **Escarpment boots (6B boots)** | Fine goat leather (T6-H6), Cashmere cloth (T6-E14) |
| T6-H1 | 31 | Raw mountain goat meat | Cedar-plank goat with plums | Cedar plank (T6-L1), Plums (region 27) |
| T6-H2 | 31 | Mountain goat hide | Mountain goat leather | |
| T6-H3 | 32 | Mountain goat hide | Normal cedar bow | Jute cord (T6-E3) |
| T6-H4 | 32 | Mountain goat leather | **Jute shirt (6A shirt)** | Jute cloth (T6-E2) |
| T6-H5 | 33 | 40 Normal cedar bows | Elite cedar bow | Fine deer leather (T5-H7) |
| T6-H6 | 33 | Mountain goat leather | Fine goat leather | Safflower oil (T5-G7) |
| T6-H7 | 33 | Mountain goat horns | Cedar goat carving | |
| T6-H8 | 35 | Mountain thyme | Mountain goat thyme pie | |
| T6-H9 | 35 | Fine goat leather | **Silk shirt (6B shirt)** | Silk cloth (T6-E12) |
| T6-E1 | 31 | Arctic char | Arctic char apricot bake | Caught with the maple rod (one tier above) |
| T6-E2 | 32 | Jute fiber | Jute cloth | |
| T6-E3 | 32 | Jute fiber | Jute cord | |
| T6-E4 | 32 | Jute cloth | **Jute trousers (6A pants)** | Nickel fittings (T6-L4) |
| T6-E5 | 32 | Jute cord | Normal cedar rod | Nickel fittings (T6-L4) |
| T6-E6 | 32 | Dolly Varden | Dolly Varden buckwheat parcel | |
| T6-E7 | 33 | 40 Jute trousers | **Terrace pack (6A backpack)** | |
| T6-E8 | 33 | 40 Normal cedar rods | Elite cedar rod | Fine ramie cloth (T5-E12) |
| T6-E9 | 33 | Whitefish | Whitefish hazelnut cakes | |
| T6-E10 | 33 | Jute cloth | Fine jute cloth | Willow cord (T5-E11); input for the tier-seven Elite rod |
| T6-E11 | 34 | Silk cocoons | Silk thread | |
| T6-E12 | 34 | Silk thread | Silk cloth | |
| T6-E13 | 35 | Goat down | Cashmere yarn | |
| T6-E14 | 35 | Cashmere yarn | Cashmere cloth | |
| T6-E15 | 35 | Grayling | Grayling fennel stew | |
| T6-E16 | 36 | Cashmere cloth | **Cashmere trousers (6B pants)** | Cut almandine (T6-G10) |
| T6-E17 | 36 | 40 Cashmere trousers | **Foothill frame pack (6B backpack)** | Cedar beams (T6-L5), Fine goat leather (T6-H6) |

### 15.3 Tier seven (regions 37-42)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T7-G1 | 37 | Redwood branches | Redwood log | |
| T7-G2 | 38 | Millet | Peach millet pot | Peaches from redwood trees or loose in 37 |
| T7-G3 | 38 | Peaches | **Buffalo-hide hat (7A hat)** | Buffalo leather (T7-H2), Sisal cloth (T7-E1) |
| T7-G4 | 39 | Raspberries | Raspberry pecan pot | |
| T7-G5 | 39 | Travertine | Pecan travertine keepsake | Sisal cloth (T7-E1) |
| T7-G6 | 40 | Tarragon | Malachite tarragon pouch | |
| T7-G12 | 40 | Malachite | Polished malachite | |
| T7-G7 | 40 | Sesame seeds | Sesame oil | |
| T7-G8 | 41 | Pomegranates | Tourmaline indigo token | Indigo dye (T7-E10) |
| T7-G13 | 41 | Tourmaline | Cut tourmaline | |
| T7-G9 | 42 | Amaranth seeds | Pomegranate amaranth pot | |
| T7-G10 | 42 | Red clay | Saffron clay keepsake | |
| T7-G11 | 42 | Saffron | **Painted hat (7B hat)** | Fine sisal cloth (T7-E11), Fine buffalo leather (T7-H8), Cut tourmaline (T7-G13) |
| T7-L1 | 37 | Redwood logs | Redwood plank | |
| T7-L2 | 38 | Chromium ore | Chromium ingot | |
| T7-L3 | 38 | Chromium ingots | Normal redwood axe | Sisal cord (T7-E2) |
| T7-L4 | 38 | Chromium ore | Chromium fittings | |
| T7-L5 | 38 | Redwood planks | Redwood beam | Sisal cord (T7-E2) |
| T7-L6 | 38 | Redwood beams | **Bluff boots (7A boots)** | Buffalo leather (T7-H2) |
| T7-L7 | 39 | 40 Normal redwood axes | Elite redwood axe | Cedar plank (T6-L1) |
| T7-L8 | 41 | Pecans | **Gorge boots (7B boots)** | Fine buffalo leather (T7-H8) |
| T7-H1 | 37 | Raw buffalo meat | Smoked buffalo jerky | |
| T7-H2 | 37 | Buffalo hide | Buffalo leather | |
| T7-H3 | 38 | Buffalo hide | Normal redwood bow | Sisal cord (T7-E2) |
| T7-H4 | 38 | Buffalo leather | **Sisal shirt (7A shirt)** | Sisal cloth (T7-E1) |
| T7-H5 | 38 | Millet | Buffalo millet patties | |
| T7-H6 | 39 | 40 Normal redwood bows | Elite redwood bow | Fine goat leather (T6-H6) |
| T7-H7 | 39 | Buffalo horns | Redwood buffalo totem | |
| T7-H8 | 40 | Sesame oil | Fine buffalo leather | Sesame oil (T7-G7) |
| T7-H9 | 41 | Fine buffalo leather | **Indigo shirt (7B shirt)** | Fine sisal cloth (T7-E11) |
| T7-E1 | 38 | Sisal fiber | Sisal cloth | |
| T7-E2 | 38 | Sisal fiber | Sisal cord | |
| T7-E3 | 38 | Sisal cloth | **Sisal trousers (7A pants)** | Chromium fittings (T7-L4) |
| T7-E4 | 38 | Sisal cord | Normal redwood rod | Chromium fittings (T7-L4) |
| T7-E5 | 38 | Flounder | Flounder millet cakes | Caught with the cedar rod (one tier above) |
| T7-E6 | 39 | 40 Sisal trousers | **Ochre pack (7A backpack)** | |
| T7-E7 | 39 | 40 Normal redwood rods | Elite redwood rod | Fine jute cloth (T6-E10) |
| T7-E8 | 39 | Striped bass | Striped-bass pecan crust | |
| T7-E9 | 40 | Mullet | Sesame mullet rice | |
| T7-E10 | 41 | Indigo leaves | Indigo dye | |
| T7-E11 | 41 | Indigo dye | Fine sisal cloth | |
| T7-E12 | 41 | Fine sisal cloth | **Painted trousers (7B pants)** | Polished malachite (T7-G12) |
| T7-E13 | 42 | 40 Painted trousers | **Horizon frame pack (7B backpack)** | Redwood beams (T7-L5) |
| T7-E14 | 42 | Cobia | Cobia saffron stew | |

### 15.4 Tier eight (regions 43-48)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T8-G1 | 43 | Ebony branches | Ebony log | |
| T8-G2 | 44 | Sorghum | Mango sorghum pot | Mangoes from ebony trees or loose in 43 |
| T8-G3 | 44 | Mangoes | **Bear-fur hat (8A hat)** | Bear felt (T8-H2), Lotus cloth (T8-E2) |
| T8-G4 | 45 | Pistachios | Passion fruit pistachio pot | |
| T8-G5 | 45 | Serpentine | Pistachio serpentine token | Lotus cloth (T8-E2) |
| T8-G6 | 46 | Cardamom | Moonstone spice pouch | |
| T8-G11 | 46 | Moonstone | Polished moonstone | |
| T8-G7 | 47 | Black sesame seeds | Black sesame oil | |
| T8-G8 | 47 | Passion fruit | Aquamarine lotus charm | Request revised November 7, 2026 (Aquamarine stays with T8-G12) |
| T8-G12 | 47 | Aquamarine | Cut aquamarine | |
| T8-G9 | 48 | Teff | Date teff pot | |
| T8-G13 | 48 | Opal | Polished opal | |
| T8-G10 | 48 | Dates | **Moonstone hat (8B hat)** | Fine lotus cloth (T8-E9), Fine bear felt (T8-H8), Polished moonstone (T8-G11) |
| T8-L1 | 43 | Ebony logs | Ebony plank | |
| T8-L2 | 44 | Titanium ore | Titanium ingot | |
| T8-L3 | 44 | Titanium ingots | Normal ebony axe | Lotus cord (T8-E3) |
| T8-L4 | 44 | Titanium ore | Titanium fittings | |
| T8-L5 | 44 | Ebony planks | Ebony beam | Lotus cord (T8-E3) |
| T8-L6 | 44 | Ebony beams | **Nightbloom boots (8A boots)** | Bear felt (T8-H2) |
| T8-L7 | 45 | 40 Normal ebony axes | Elite ebony axe | Redwood plank (T7-L1) |
| T8-L8 | 48 | Star anise | Opal keepsake | Request revised November 7, 2026 (Opal stays with T8-G13) |
| T8-L9 | 48 | Dragon fruit | **Starlit boots (8B boots)** | Fine bear felt (T8-H8) |
| T8-H1 | 43 | Raw bear meat | Honey-braised bear | Mangoes from ebony trees or loose in 43 |
| T8-H2 | 43 | Bear fur | Bear felt | |
| T8-H3 | 44 | Bear fur | Normal ebony bow | Lotus cord (T8-E3) |
| T8-H4 | 44 | Bear felt | **Lotus shirt (8A shirt)** | Lotus cloth (T8-E2) |
| T8-H5 | 44 | Sorghum | Bear sorghum dumplings | |
| T8-H6 | 45 | 40 Normal ebony bows | Elite ebony bow | Fine buffalo leather (T7-H8) |
| T8-H7 | 45 | Bear claws | Ebony bear carving | |
| T8-H8 | 47 | Black sesame oil | Fine bear felt | Black sesame oil (T8-G7) |
| T8-H9 | 47 | Fine bear felt | **Starlit shirt (8B shirt)** | Fine lotus cloth (T8-E9) |
| T8-E1 | 43 | Grouper | Grouper mango bake | Caught with the redwood rod (one tier above) |
| T8-E2 | 44 | Lotus fiber | Lotus cloth | |
| T8-E3 | 44 | Lotus fiber | Lotus cord | |
| T8-E4 | 44 | Lotus cloth | **Lotus trousers (8A pants)** | Titanium fittings (T8-L4) |
| T8-E5 | 44 | Lotus cord | Normal ebony rod | Titanium fittings (T8-L4) |
| T8-E6 | 45 | 40 Lotus trousers | **Lagoon pack (8A backpack)** | |
| T8-E7 | 45 | 40 Normal ebony rods | Elite ebony rod | Fine sisal cloth (T7-E11) |
| T8-E8 | 45 | Mahi-mahi | Mahi-mahi passion fruit bowl | |
| T8-E9 | 45 | Lotus cloth | Fine lotus cloth | Sisal cord (T7-E2) |
| T8-E10 | 47 | Wahoo | Black-sesame wahoo | |
| T8-E11 | 47 | Fine lotus cloth | **Prismatic trousers (8B pants)** | Cut aquamarine (T8-G12) |
| T8-E12 | 48 | 40 Prismatic trousers | **Lastlight frame pack (8B backpack)** | Ebony beams (T8-L5), Polished opal (T8-G13) |
| T8-E13 | 48 | Amberjack | Amberjack teff broth | |

### 15.5 Summary and checks

- **Count:** 46 / 46 / 44 / 44 missions in tiers five / six / seven / eight, **180** in total, including the nine gem-processing missions added on November 6, 2026 (new codes are appended; existing codes are unchanged). Across both parts there are 436 missions: tier one 50 (22 + 8 bridge + 20 coverage, Part 1 section 11.6.1.1), tiers 2-4 206, tiers 5-8 180.
- **Gathering coverage (design, November 7, 2026):** every loose resource and interaction drop now has a collection mission. Sixteen requests were changed, with no new missions or recipes: each one moved off a crafted item or a raw item that another mission already collects. Tier 5: T5-G2 Pears, T5-G3 Gneiss, T5-G8 Gypsum. Tier 6: T6-G3 Apricots, T6-G6 Mulberry leaves, T6-G8 Hazelnuts, T6-L9 Fennel seeds. Tier 7: T7-G3 Peaches, T7-G8 Pomegranates, T7-L8 Pecans, T7-G11 Saffron. Tier 8: T8-G3 Mangoes, T8-G8 Passion fruit, T8-L8 Star anise, T8-G10 Dates, T8-L9 Dragon fruit. Each new request's source region is no later than its mission region, and every replaced raw request is still collected by another mission. Dragon fruit's only recipe is still the bakery cake, an optional use.
- **Every tier gets:**
  - both equipment sets (10 garments) and the six tools
  - its refinements, including the next block's Elite inputs
  - one fish-collection mission per species (16 in total)
  - a first hunting dish and a second meat dish, each a different form (no plain roasts, November 7, 2026)
  - every regional pot, drink and Misc recipe from section 8
- **Checks:**
  - every source is reached no later than its mission region
  - every reward is craftable when offered
  - the Normal tool before its Elite, and the previous-tier refinement before an Elite
  - Set A before Set B
  - no duplicate reward
  - no recipe left without a mission unless it is a premium box or bakery recipe
  - every gatherable item, loose or from an interactable, has a collection mission in its tier
  - every Material has a recipe input or a house-deposit use outside the mystery box
  - no capability above the tier-four limits
  - quantities at or below 150 / 40
  - mission foods at or below 6 hearts
- **Open:** prices for the new materials and garments; art for the 40 garments through Part 1's fitted-shell pipeline.

## 16. House and settlement upgrades for tiers 5-8 (proposal, user request November 5, 2026)

The settlement stays where it is, between regions 4 and 5, reached by fast travel (Part 1 section 12.3). Part 1 section 13 still applies: houses level up only through material deposits, furniture is bought, never crafted, and shops refresh on leaving and returning. Tiers 5-8 continue the same pattern of **two house levels per resource tier**. The lower level of each pair uses the block's early-region materials, the higher one its late-region materials, like the two equipment sets.

**No new stats:** as with equipment (section 13), the new levels add **space, looks and storage arrangement**, not stronger buffs. Furniture buffs and chest sizes keep their Part 1 caps. The exception, by request, is pets: the pet home admits up to eight active pets and the exotic pets gather more (section 16.5).

### 16.1 House levels 9-16

The house keeps the fixed 12-unit width from level 3 on and keeps growing upward (two rows per level), with the scrollable interior. Upgrade costs are deposits, not recipes, so raw loose stones and gems are valid sinks. That gives the block's stones (Gneiss, Gypsum, Kaolin, Pumice, Travertine, Red clay, Serpentine) a second use besides their Misc token. **Deposit growth (user, November 5, 2026):** every listed material type asks **25 x the resource tier**, as in Part 1 section 13.1 (tier four 100), so tiers 5-8 ask **125 / 150 / 175 / 200** per type.

| Level | Visual stage | Interior grid | Upgrade deposit to reach it (25 x tier per type) | Needs regions |
| --- | --- | --- | --- | --- |
| 9 | Maple lodge (stone base, maple timber upper floor) | 12 x 19 | 125 Maple beams + 125 Gneiss + 125 Ramie cloth + 125 Zinc fittings + 125 Polished amber | 25-27 |
| 10 | Maple lodge with veranda and orchard porch | 12 x 21 | 125 Maple planks + 125 Gypsum + 125 Fine ramie cloth + 125 Magnetite fittings | 28-30 |
| 11 | Cedar chalet (steep roof, balconies) | 12 x 23 | 150 Cedar beams + 150 Kaolin + 150 Jute cloth + 150 Nickel fittings + 150 Nickel silver ingots | 31-33 |
| 12 | Cedar chalet with lookout tower | 12 x 25 | 150 Cedar planks + 150 Pumice + 150 Silk cloth + 150 Almandine + 150 Cut celestite | 34-36 |
| 13 | Redwood hall (tall timber frame) | 12 x 27 | 175 Redwood beams + 175 Travertine + 175 Sisal cloth + 175 Chromium fittings | 37-39 |
| 14 | Redwood hall with painted gallery | 12 x 29 | 175 Redwood planks + 175 Red clay + 175 Fine sisal cloth + 175 Tourmaline | 40-42 |
| 15 | Ebony estate (dark timber, stone terraces) | 12 x 31 | 200 Ebony beams + 200 Serpentine + 200 Lotus cloth + 200 Titanium fittings | 43-45 |
| 16 | Grand homestead (final) | 12 x 33 | 200 Ebony planks + 200 Fine lotus cloth + 200 Titanium fittings + 200 Opal | 46-48 |

Every deposited material is a section 15 mission reward or a loose resource of its block, so each level is reachable when its regions are. **Added November 7, 2026:** Polished amber (T5-G12, region 26), Nickel silver ingots (T6-L10, region 32) and Cut celestite (T6-G9, region 34) were mission-taught materials whose only use was a mystery-box keepsake; levels 9, 11 and 12 now take them, at most five types per level. The upgrade menu, escrow, cancellation through the tray and atomic completion stay as in Part 1 section 13.1. **Art:** the 16 exteriors and their interiors are one set, as Part 1 already plans for levels 1-8, so the eight new exteriors are designed together with the first eight.

### 16.2 Furniture for levels 9-16

Part 1's 48 buff furniture types (six per level, each of the twelve categories bought four times for its full bonus) stay the **complete buff set**. Levels 9-16 add no new buff types and no stronger copies. Instead, each level unlocks **six decorative types in its wood and landscape theme**:
- **Bed, table, lamp and rug** (one set per wood: maple, cedar, redwood, ebony).
- **A trophy display** for that tier's trophy (antler mount, horn rack, buffalo skull plaque, claw case). It is purely decorative; the trophy items stay sale Misc.
- **A themed chest look** using an existing Part 1 chest size (no chest larger than 48 x 6).

The larger interior holds more placed chests and displays rather than bigger ones. Decorative types are bought once per item like other furniture and grant nothing on purchase.

### 16.3 Settlement shops

- **Looks by tier:** Part 1's shops change look at regions 7, 13 and 19 (pine, birch, mesquite, oak). Four more looks follow at the first camp of regions **25, 31, 37 and 43** (maple, cedar, redwood, ebony timbers and trims, with richer signs and more stock on display). Each counter view changes with its front and the keeper stays the same person. Each change happens once and is saved, as in Part 1.
- **Stock:** each shop's existing stock filters extend to discovered tier-5-8 items. The three-offer rule, Q5, full stacks and the refresh on leaving and returning are unchanged.
  - Mine: the block's ores and stones (Zinc, Magnetite, Nickel, Chromium and Titanium ore; Gneiss, Gypsum, Kaolin, Pumice, Travertine, Red clay, Serpentine).
  - Farm: the new produce, herbs and seeds.
  - Tailor-material shop: Ramie, Jute, Sisal and Lotus fiber, Willow bast, and the block's leathers. Silk cocoons and Goat down stay gathered only.
  - Fishery: the tier-5-8 fish of discovered pairs plus the section 9 exclusives.
  - Ranch: two dairy/egg offers and one ranch-exclusive meat (section 10.2); hunted meats stay hunted.
  - Bakery: section 11.
  - Gems stay loose or box-only (no gem shop).
- **Warehouse:** unchanged (two ten-unit boxes, 1 gold per 1,000 cash). Tier-5-8 goods are deposited at their own values.

### 16.4 Lumber field: four more sections

The field grows from four to **eight wood sections**, two rows of six massive trees each (48 trees), with the same rules as Part 1 section 13.4:
- separate 72-hour respawns per tree
- x50 rewards, including the tree food (Maple sap, Apricots, Peaches, Mangoes)
- the focused cutting screen, which can be reopened on a felled tree to pick up remaining drops (Part 1 section 8.2.1)

Each new section has a gold unlock and requires its wood tier to be discovered. Proposed prices, continuing pine 50 / birch 300 / mesquite 750 / oak 1,500:

| Section | Gold unlock | Requires |
| --- | --- | --- |
| Maple | 2,200 | Region 25 |
| Cedar | 3,000 | Region 31 |
| Redwood | 3,900 | Region 37 |
| Ebony | 5,000 | Region 43 |

Tool reach still applies: a massive tree needs an axe at most two tiers below it (an ebony tree needs a redwood, cedar or ebony axe). The field area scrolls sideways past the fourth section. Massive-tree art for the four new woods follows Part 1's massive-tree recommendation (buttress roots, deep bark, two or three trunk shapes per species, a cut stump).

### 16.5 Neighbour houses, pet home and exotic pets

- **Neighbours:** the five neighbour houses keep the 1-3 discovered-regions rule and can now reach **level 16**, using the same 16 exteriors as the player's house. With 24 more regions the street gradually shows the new looks.

**Pet home levels 5-8 (proposal, user request November 5, 2026).** Part 1 section 13.5 adds pet home levels 1-4 (one to four active pets). Tiers 5-8 continue them; **each level admits one more active pet, up to eight**. The deposit is half the house amount of the same tier (rounded to 5), with planks, cloth and fittings, as in Part 1:

| Pet home level | Active pets | Requires | Deposit |
| --- | --- | --- | --- |
| 5 | 5 | House level 10 | 65 Maple planks + 65 Ramie cloth + 65 Zinc fittings |
| 6 | 6 | House level 12 | 75 Cedar planks + 75 Jute cloth + 75 Nickel fittings |
| 7 | 7 | House level 14 | 90 Redwood planks + 90 Sisal cloth + 90 Chromium fittings |
| 8 | 8 | House level 16 | 100 Ebony planks + 100 Lotus cloth + 100 Titanium fittings |

The pet home's look keeps growing in the tier's wood: a maple barn, cedar stable, redwood menagerie and ebony pavilion.

**Common pets:** the six Part 1 pets keep their output (one material per roll, 30 minutes, eight-hour cap, 16-entry container). Their supply groups now include the discovered tier-5-8 loose resources:
- **Wood:** Maple, Cedar, Redwood and Ebony branches.
- **Stone/ore:** Amber, Gneiss, Zinc ore, Magnetite, Gypsum, Agate, Nickel ore, Kaolin, Celestite, Almandine, Pumice, Chromium ore, Travertine, Malachite, Tourmaline, Red clay, Titanium ore, Serpentine, Moonstone, Aquamarine, Opal.
- **Fibers:** Ramie, Willow bast, Jute, Sisal, Lotus fiber, Indigo leaves, Mulberry leaves.
- **Produce/herbs:** Pears, Rye, Plums, Chestnuts, Calendula, Chicory root, Apricots, Buckwheat, Blackcurrants, Hazelnuts, Mountain thyme, Quinces, Peaches, Millet, Raspberries, Pecans, Tarragon, Pomegranates, Saffron, Mangoes, Sorghum, Passion fruit, Pistachios, Cardamom, Star anise, Dragon fruit, Dates, Teff.
- **Seeds:** Safflower, Fennel, Sesame, Amaranth and Black sesame seeds.
- **Excluded for common pets,** like pelts in tiers 1-4: hides, fur, Silk cocoons, Goat down and Maple sap. Some exotic pets can bring three of these (below).
- The 70/30 highest-tier roll now uses the highest discovered tier up to 8.

### 16.5.1 Exotic pets (proposal, user request November 5, 2026)

Six **exotic companions**, one theme per landscape of tiers 5-8, sold in the same home-store Pets section. They are the one place where Part 2 adds stronger output (user: "more exotic and with boosted resource acquisition"). Equipment, food and furniture caps stay at tier four.

**Boosts compared with a common pet:**

| Rule | Common pet (Part 1) | Exotic pet |
| --- | --- | --- |
| Roll interval (offline) | 30 min | **20 min** |
| Units per roll (Release) | 1 | **2** |
| MechanicsTest units per roll | 1,000 | 2,000 (two linked 1,000-unit entries per opportunity) |
| Collection container | 16 opportunities / 16 entries | **24 opportunities**: 24 entries in Release; 48 entries in MechanicsTest |
| Highest discovered tier / earlier tiers | 70% / 30% | **85% / 15%** |
| Specialty source | none (or Truffle for the Pig) | can bring one normally excluded item |

Everything else is unchanged:
- an exotic pet occupies one pet home slot
- no upkeep and no duplicates
- the eight-hour cap per absence still applies
- nothing is banked beyond the container
- rolls draw only from discovered sources, and an empty or undiscovered group is skipped and the rest renormalised
- no pet-only mandatory recipe: every specialty item also has a loose or tree source

**Capacity and transaction correction, November 6, 2026:** capacity counts **reward opportunities**, separately from inventory stacks. One exotic opportunity rolls one exact item/source/quality result: Release stores 2 units in one entry; MechanicsTest stores 2,000 units split into two linked 1,000-unit entries with the same result/profile. Persist one opportunity ID with both children atomically; never roll the second child independently. An empty exotic container accepts 24 opportunities over 480 offline minutes: 48 units in Release or 48,000 in MechanicsTest. The test queue needs 48 entry positions, not 24; bag stack cap stays 1,000. Common pets keep 16 opportunities / 16 entries.

Partial claims retain the opportunity and exact unclaimed children/remainders; its slot is freed only when fully claimed. Stop new opportunities at 24 occupied slots and discard capped time under Part 1's no-banking rule. Never regenerate claimed children on reload, merge opportunities to evade capacity, or clear queues on pet switches/home upgrades. Capacity belongs to each pet's saved queue, not a shared home admission slot. Migrate existing results/partial claims without inventing historical output or backfilling discarded time; reconstruct links only from recorded generation IDs. Switching settles the prior interval and retains its original timer/source snapshot; subsequent opportunities use the new pet's interval. Review 20/30-minute switches in both directions.

**Price and unlock:** **10,000 cash** each (ten times a common pet), no gold. Each needs **house level 9** and the discovery of its home region. These are saving goals for players already in tier five. Purchase and owned-ID registration stay atomic.

| Pet ID / species | Unlock region | Wood | Stone/ore | Fibers | Produce/herbs | Seeds | Specialty (exact item) | Role |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `pet.red_panda` — Red panda | 28 Golden Hollow | 15% | 5% | 5% | 45% | 20% | 10% Maple sap | Orchard foods and sap for syrup |
| `pet.alpaca` — Alpaca | 35 Whistling Escarpment | 5% | 0% | 45% | 15% | 5% | 30% Goat down | Cashmere supply for the tier-6 Set B |
| `pet.fennec_fox` — Fennec fox | 38 Ochre Prairie | 5% | 55% | 10% | 15% | 15% | 0% | Ores, stones and gems for house deposits |
| `pet.capybara` — Capybara | 44 Mirrorpetal Marsh | 15% | 5% | 35% | 35% | 10% | 0% | Fibers and grains for cloth and stews |
| `pet.peacock` — Peacock | 46 Starlit Gardens | 0% | 40% | 0% | 25% | 25% | 10% Silk cocoons | Spices, gems and silk |
| `pet.pangolin` — Pangolin | 47 Prismatic Inlet | 30% | 50% | 10% | 10% | 0% | 0% | Late building supplies (a stronger Dog) |

- **Rows:** each row totals 100%.
- **Specialty items:** Maple sap, Goat down and Silk cocoons stay gathered or tree items. An exotic pet that brings them adds a source, not the only source.
- **Stone/ore** includes the gems (Agate, Celestite, Almandine, Tourmaline, Malachite, Moonstone, Aquamarine, Opal), so the Fennec fox and the Peacock help with the gem-trimmed Set B garments and the level-12/14/16 house deposits.
- **Higher tiers:** exotic pets also roll tier 1-4 groups, but their 85% highest-tier share keeps them focused on the current block.
- **Art:** one faceted pet model per species, sized like the common pets, shown at the pet home.

**Checks:**
- six rows at 100%
- unlocks gated by house level 9 and the region
- the 20-minute and 2-unit boosts only on exotic IDs
- empty queue at 4 hours: 12 opportunities / 24 MechanicsTest entries; at 8 hours: 24 opportunities / 48 MechanicsTest entries; no generation beyond time or capacity caps
- Release at 8 hours: 24 entries / 48 units; MechanicsTest: 48 entries / 48,000 units; partial claims/reloads never duplicate children or free incomplete opportunities
- specialty items only after their region is discovered
- no exotic-only mandatory ingredient
- admission limited by the pet home level, with the settle-then-switch rule per slot
- reload without rerolls or double rewards

### 16.6 Checks

- every deposit material has a mission or loose source no later than the level's regions
- levels 9-16 cannot be completed before their regions
- no new buff or chest size above Part 1's caps; pet output above Part 1 only for the exotic pets (section 16.5.1)
- shop looks change once each at regions 25/31/37/43 and survive reload
- the lumber unlocks need discovery and gold
- the neighbours' levels stay at or below 16
