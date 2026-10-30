# TheTrailWalker: Recommendations Part 2 — Progression Beyond Tier 4

Updated: October 30, 2026.

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
- Reuse existing Normal/Elite tool behavior as the basis: Normal tools at every tier; Elite recipes consume an exact same-tier Normal instance plus explicit previous-tier advanced materials. Tier-5–8 tools, costs, action tables and durability are not yet authored.
- Keep four mission branches and one active mission globally. New missions require exact IDs/counts and accessible prerequisites. Random recipe discoveries must not block mandatory progression. Expansion mission order/quantities are open; do not silently exceed the current 150-unit campaign ceiling.
- Preserve three featured loose resources per region. This draft proposes six regions per new tier, adding 24 regions and 72 loose-resource entries (section 7). Hunting outputs, logs and fish do not occupy extra loose-resource slots. Section 9 proposes the fish species and regional pairs.
- Existing caps are already available in the base game: twelve hearts, ten pockets, ten hooks, 1,000-unit stacks and 1.5x stack model growth. Do not reserve them for future tiers. Expansion does not automatically raise speed/capacity or the current 26-heart segment budget.
- Keep progressive food recovery and pending hearts. Sections 8–11 propose food values and gold prices while retaining existing shop/box probabilities. The seven tier-1–4 bakery recipes remain eligible; eight expansion additions bring the proposed catalog to fifteen.
- Furniture stays purchased. Materials may support ingredients, containers and house upgrades; future woods/trophies do not imply player-crafted furniture. Additional house costs/levels remain open.

## 4. Discovery and persistence requirements

Require normal discovery of a recipe's authored tier and **every exact regional ingredient source** before offering/unlocking it. Follow refined inputs to raw sources and require known conversions or accessible exact shops as applicable. High mastery, gold, gifted inputs, bought food or domestic goat products never prove discovery of future regions/wild species.

Validate at offer generation and again before payment/unlock, including after reload/migration. Locked previews show unmet sources without granting knowledge. Persist discovery, recipe IDs/mastery, stock, quotes, visit IDs and transactions; never infer discovery from owned items, reroll paid rewards or charge twice. Existing tier-1–4 rules continue to apply.

Until implemented explicitly, exclude expansion definitions from runtime loot, tools, missions, shops, boxes, pets and house costs. If disabled IDs already exist in test saves, migrate or quarantine them safely; never silently reinterpret a species or delete paid entitlements. Preserve existing IDs, quantities and intrinsic profiles.

## 5. Proposed expansion milestones

These planning steps follow the relevant working base systems. They do not renumber Part 1 or claim its milestones passed.

| Step | Work | Acceptance |
| --- | --- | --- |
| P2.1 — Define the tier-5 slice | Choose actual regions/sources, Maple/deer IDs, recipes, Normal/Elite tools and mission prerequisites; define access from tier 4. | Every output has an exact source/recipe; no inaccessible mandatory ingredient or missing previous-tier dependency. |
| P2.2 — Validate one tier-5 route | Integrate one representative route/camp, discovery, harvesting, food, gear, missions and save migration. | Tier-4 saves survive; undiscovered sources block unlocks; transfers/loss/drop cycles settle once. Keep full-stack pickups/debug tools. |
| P2.3 — Complete approved tier-5 content | Fill approved regional tables, routes, economy, shop/box/pet filters and house sinks. | Counts match approved tables; sources/recipes are reachable; earlier catalogs retain their meaning. |
| P2.4 — Repeat for tiers 6–8 | Review/implement Cedar/goat, Redwood/buffalo, then Ebony/bear using sections 7–12; review the proposed tier-6 silk/cashmere sources. | Missing-region/source tests per tier; no future content leaks into earlier rewards or requirements. |

## 6. Open data and checks

Still open: exact mission tasks/rewards, house costs, route geometry/drain tuning, tool definitions/durability, detailed harvesting quantities and timing/valuation overrides, general cash-price tables and pet weight/source-group assignment for new materials. Sections 7–12 supply initial proposed region/resource/fish and recipe catalogs, plus selected shop quotes. Review these proposals before runtime implementation.

For each new recipe, check all prerequisites satisfied, then remove each required source/conversion individually. Include gifted inputs, refined shop stock, duplicate offers, locked previews, interrupted payment and reload/migration. Check exact ingredient consumption, Normal-to-Elite instance replacement, output ownership, 64-bit totals and full-stack performance on mobile.

The debug/testing policy continues: 1,000-unit generated pickups and debug tools remain available. Planning expansion or completing testing does not activate rebalancing, debug removal, one-item collection, release preparation or publication.

**Catalog update:** sections 7–12 now propose concrete region-25–48 data. Earlier open-item notes describe the initial backlog; the new tables resolve the listed region/recipe proposals while detailed tools, missions and house costs remain open.

## 7. Proposed region and resource catalog

**First populated proposal; design data, not runtime activation.** Propose six regions per tier: regions 25–48, three distinct loose resources per region, 18 per tier and 72 additions. Combined design target: 48 regions and 144 loose resources. English catalog names and IDs are stable across both language documents; localize display names during implementation. Each new region proposes 24 camps (23 internal legs), with a maximum 26-heart walking budget per leg; no new capacity caps or climate hazards.


| Region / ID | Tier | Three exact loose resources / IDs | Fish pair | Water-node presentation |
| --- | --- | --- | --- | --- |
| `region.25` Copperleaf Vale | 5 | Maple branches (`resource.maple_branches`); Deer hide (`resource.deer_hide`); Pears (`resource.pears`) | Brook trout + Smallmouth bass | stream and lake |
| `region.26` Lantern Meadows | 5 | Rye (`resource.rye`); Amber (`resource.amber`); Gneiss (`resource.gneiss`) | White crappie + Sauger | river and reservoir |
| `region.27` Veiled Ravine | 5 | Ramie fiber (`resource.ramie_fiber`); Zinc ore (`resource.zinc_ore`); Plums (`resource.plums`) | Brook trout + White crappie | stream and river |
| `region.28` Golden Hollow | 5 | Chestnuts (`resource.chestnuts`); Calendula (`resource.calendula`); Safflower seeds (`resource.safflower_seeds`) | Smallmouth bass + Sauger | lake and reservoir |
| `region.29` Glasswater Reach | 5 | Magnetite (`resource.magnetite`); Gypsum (`resource.gypsum`); Agate (`resource.agate`) | Brook trout + Sauger | stream and reservoir |
| `region.30` Sunset Orchard | 5 | Maple sap (`resource.maple_sap`); Chicory root (`resource.chicory_root`); Willow bast (`resource.willow_bast`) | Smallmouth bass + White crappie | river and lake |
| `region.31` Cloudgate Pass | 6 | Cedar branches (`resource.cedar_branches`); Mountain goat hide (`resource.mountain_goat_hide`); Apricots (`resource.apricots`) | Arctic char + Grayling | cold stream and alpine lake |
| `region.32` Highland Terrace | 6 | Jute fiber (`resource.jute_fiber`); Nickel ore (`resource.nickel_ore`); Buckwheat (`resource.buckwheat`) | Whitefish + Dolly Varden | tributary and deep lake |
| `region.33` Silvermist Basin | 6 | Blackcurrants (`resource.blackcurrants`); Hazelnuts (`resource.hazelnuts`); Kaolin (`resource.kaolin`) | Arctic char + Whitefish | cold stream and tributary |
| `region.34` Silken Canopy | 6 | Mulberry leaves (`resource.mulberry_leaves`); Silk cocoons (`resource.silk_cocoons`); Celestite (`resource.celestite`) | Grayling + Dolly Varden | alpine lake and deep lake |
| `region.35` Whistling Escarpment | 6 | Goat down (`resource.goat_down`); Mountain thyme (`resource.mountain_thyme`); Fennel seeds (`resource.fennel_seeds`) | Arctic char + Dolly Varden | cold stream and deep lake |
| `region.36` Moonlit Foothills | 6 | Almandine (`resource.almandine`); Pumice (`resource.pumice`); Quinces (`resource.quinces`) | Grayling + Whitefish | tributary and alpine lake |
| `region.37` Dawnwood Expanse | 7 | Redwood branches (`resource.redwood_branches`); Buffalo hide (`resource.buffalo_hide`); Peaches (`resource.peaches`) | Striped bass + Mullet | estuary and sheltered coast |
| `region.38` Ochre Prairie | 7 | Sisal fiber (`resource.sisal_fiber`); Chromium ore (`resource.chromium_ore`); Millet (`resource.millet`) | Flounder + Cobia | tidal inlet and open-coast pier |
| `region.39` Echoing Bluffs | 7 | Raspberries (`resource.raspberries`); Pecans (`resource.pecans`); Travertine (`resource.travertine`) | Striped bass + Flounder | estuary and tidal inlet |
| `region.40` Spicewind Flats | 7 | Tarragon (`resource.tarragon`); Sesame seeds (`resource.sesame_seeds`); Malachite (`resource.malachite`) | Mullet + Cobia | sheltered coast and open-coast pier |
| `region.41` Painted Gorge | 7 | Pomegranates (`resource.pomegranates`); Tourmaline (`resource.tourmaline`); Indigo leaves (`resource.indigo_leaves`) | Striped bass + Cobia | estuary and open-coast pier |
| `region.42` Saffron Horizon | 7 | Saffron (`resource.saffron`); Amaranth seeds (`resource.amaranth_seeds`); Red clay (`resource.red_clay`) | Mullet + Flounder | tidal inlet and sheltered coast |
| `region.43` Nightbloom Coast | 8 | Ebony branches (`resource.ebony_branches`); Bear fur (`resource.bear_fur`); Mangoes (`resource.mangoes`) | Grouper + Mahi-mahi | rocky shore and coastal pier |
| `region.44` Mirrorpetal Marsh | 8 | Lotus fiber (`resource.lotus_fiber`); Titanium ore (`resource.titanium_ore`); Sorghum (`resource.sorghum`) | Amberjack + Wahoo | sheltered bay and offshore pier |
| `region.45` Emerald Lagoon | 8 | Passion fruit (`resource.passion_fruit`); Pistachios (`resource.pistachios`); Serpentine (`resource.serpentine`) | Grouper + Amberjack | rocky shore and sheltered bay |
| `region.46` Starlit Gardens | 8 | Cardamom (`resource.cardamom`); Star anise (`resource.star_anise`); Moonstone (`resource.moonstone`) | Mahi-mahi + Wahoo | coastal pier and offshore pier |
| `region.47` Prismatic Inlet | 8 | Dragon fruit (`resource.dragon_fruit`); Black sesame seeds (`resource.black_sesame_seeds`); Aquamarine (`resource.aquamarine`) | Grouper + Wahoo | rocky shore and offshore pier |
| `region.48` Lastlight Shores | 8 | Dates (`resource.dates`); Teff (`resource.teff`); Opal (`resource.opal`) | Mahi-mahi + Amberjack | sheltered bay and coastal pier |



Only branches and hide/fur overlap loose gathering and interactions. Logs, meat, antlers/horns/claws and fish are interaction outputs, never extra loose icons. Singular/plural ingredient labels below resolve to the exact resource ID above, not separate variants. Goat down is a shed-fiber gathering resource in region 35, not a second hunted pelt drop. Silk cocoons gather in region 34; Mulberry leaves have an authored craft sink without adding silkworm farming or a pet. These sources and tier-6 silk/cashmere placement are explicit new proposals.


## 8. Processing, hunting and regional recipes

All inputs below are exact. Regional raw IDs are written explicitly in section 7; processed outputs use `item.<recipe_suffix>`, except branch-to-log conversions output `item.maple_log`, `item.cedar_log`, `item.redwood_log` and `item.ebony_log`. Tree logs use these same IDs. `Maple branch`/`Maple branches` refer to the one `resource.maple_branches` ID; apply the same singular/plural alias rule to other resources. Singular/plural labels never permit variety substitution. Wild meat IDs are `item.raw_deer_meat`, `item.raw_mountain_goat_meat`, `item.raw_buffalo_meat` and `item.raw_bear_meat`; trophies are `item.deer_antler`, `item.mountain_goat_horn`, `item.buffalo_horn` and `item.bear_claw`. Hunted hide/fur outputs use the same exact resource IDs as their loose counterparts. Fishery ingredient and fish IDs are explicit in section 9.

Material recipes are unlevelled with fixed timers; Food/Misc retain Part 1 mastery/quality rules. Nothing crafts furniture. Costs represent simplified processes with no hidden fuel. Initially grant nonrandom regional recipes through an appropriate optional branch task after all sources are discovered, without replacing/reordering main missions; individual task design remains open. Do not require a random box or bakery recipe to unlock these conversions or ordinary meals. Tier-5–8 timer/valuation overrides must be authored before activation; do not silently extrapolate the four-tier numeric curves past their defined range.


| Recipe ID | Tier | Output | Exact inputs -> yield | Category |
| --- | --- | --- | --- | --- |
| `recipe.material.maple_log_from_branches` | 5 | Maple log | 3 Maple branches -> 1 | Material |
| `recipe.material.cedar_log_from_branches` | 6 | Cedar log | 3 Cedar branches -> 1 | Material |
| `recipe.material.redwood_log_from_branches` | 7 | Redwood log | 3 Redwood branches -> 1 | Material |
| `recipe.material.ebony_log_from_branches` | 8 | Ebony log | 3 Ebony branches -> 1 | Material |
| `recipe.material.maple_plank` | 5 | Maple plank | 2 Maple logs -> 1 | Material |
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
| `recipe.material.cedar_plank` | 6 | Cedar plank | 2 Cedar logs -> 1 | Material |
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
| `recipe.material.redwood_plank` | 7 | Redwood plank | 2 Redwood logs -> 1 | Material |
| `recipe.material.buffalo_leather` | 7 | Buffalo leather | 2 Buffalo hide -> 1 | Material |
| `recipe.material.sisal_cloth` | 7 | Sisal cloth | 4 Sisal fiber -> 2 | Material |
| `recipe.material.chromium_ingot` | 7 | Chromium ingot | 2 Chromium ore + 1 Cedar branch -> 1 | Material |
| `recipe.misc.pecan_travertine_keepsake` | 7 | Pecan travertine keepsake | 1 Pecan + 2 Travertine + 1 Sisal cloth -> 1 | Misc |
| `recipe.material.sesame_oil` | 7 | Sesame oil | 3 Sesame seeds -> 1 | Material |
| `recipe.misc.malachite_tarragon_pouch` | 7 | Malachite tarragon pouch | 1 Malachite + 2 Tarragon + 1 Sisal cloth -> 1 | Misc |
| `recipe.material.indigo_dye` | 7 | Indigo dye | 3 Indigo leaves -> 1 | Material |
| `recipe.misc.tourmaline_indigo_token` | 7 | Tourmaline indigo token | 1 Tourmaline + 1 Indigo dye + 1 Sisal cloth -> 1 | Misc |
| `recipe.misc.saffron_clay_keepsake` | 7 | Saffron clay keepsake | 1 Saffron + 2 Red clay + 1 Sisal cloth -> 1 | Misc |
| `recipe.material.ebony_plank` | 8 | Ebony plank | 2 Ebony logs -> 1 | Material |
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
| 5 | `tree.maple` | Maple | Maple branches + Maple logs; logs are interaction-only |
| 5 | `hunt.deer` | deer | Deer hide + Raw deer meat + Deer antler; one hide/fur type per animal |
| 6 | `tree.cedar` | Cedar | Cedar branches + Cedar logs; logs are interaction-only |
| 6 | `hunt.mountain_goat` | mountain goat | Mountain goat hide + Raw mountain goat meat + Mountain goat horn; one hide/fur type per animal |
| 7 | `tree.redwood` | Redwood | Redwood branches + Redwood logs; logs are interaction-only |
| 7 | `hunt.buffalo` | buffalo | Buffalo hide + Raw buffalo meat + Buffalo horn; one hide/fur type per animal |
| 8 | `tree.ebony` | Ebony | Ebony branches + Ebony logs; logs are interaction-only |
| 8 | `hunt.bear` | bear | Bear fur + Raw bear meat + Bear claw; one hide/fur type per animal |



Each wood also has fixed Material recipe `recipe.material.<wood>_log_from_branches`: 3 exact branches -> 1 exact log. Planks consume tree or converted logs. Trophy Misc recipes consume the exact raw trophy directly, with no hidden cutting/polishing step. Harvest quantities/Elite boosts use the existing service's authored behavior, not an invented per-tier multiplier. Exact expansion tools, durability and mission requirements still require authoring before region activation.


| Recipe ID | Tier | Regional Food output | Exact inputs -> yield | Total hearts |
| --- | --- | --- | --- | --- |
| `recipe.food.pear_rye_porridge` | 5 | Pear rye porridge | 2 Pears + 2 Rye + 1 Cow milk -> 1 | 8 |
| `recipe.food.plum_chestnut_pot` | 5 | Plum chestnut pot | 2 Plums + 2 Chestnuts + 1 Honey -> 1 | 8 |
| `recipe.food.chicory_maple_drink` | 5 | Chicory maple drink | 2 Chicory root + 1 Maple syrup + 1 Cow milk -> 1 | 7 |
| `recipe.food.apricot_buckwheat_pot` | 6 | Apricot buckwheat pot | 2 Apricots + 2 Buckwheat + 1 Cow milk -> 1 | 9 |
| `recipe.food.blackcurrant_hazelnut_pot` | 6 | Blackcurrant hazelnut pot | 2 Blackcurrants + 1 Hazelnut + 1 Cow yogurt -> 1 | 9 |
| `recipe.food.quince_thyme_compote` | 6 | Quince thyme compote | 2 Quinces + 1 Mountain thyme + 1 Fennel seed -> 1 | 8 |
| `recipe.food.peach_millet_pot` | 7 | Peach millet pot | 2 Peaches + 2 Millet + 1 Cow milk -> 1 | 10 |
| `recipe.food.raspberry_pecan_pot` | 7 | Raspberry pecan pot | 2 Raspberries + 1 Pecan + 1 Cow yogurt -> 1 | 10 |
| `recipe.food.pomegranate_amaranth_pot` | 7 | Pomegranate amaranth pot | 2 Pomegranates + 2 Amaranth seeds + 1 Honey -> 1 | 10 |
| `recipe.food.mango_sorghum_pot` | 8 | Mango sorghum pot | 2 Mangoes + 2 Sorghum + 1 Cow milk -> 1 | 11 |
| `recipe.food.passionfruit_pistachio_pot` | 8 | Passion fruit pistachio pot | 2 Passion fruit + 1 Pistachio + 1 Cow yogurt -> 1 | 11 |
| `recipe.food.date_teff_pot` | 8 | Date teff pot | 2 Dates + 2 Teff + 1 Cow milk -> 1 | 11 |
| `recipe.food.roasted_deer` | 5 | Roasted deer | 1 Raw deer meat + 1 Maple log + 2 Maple branches -> 1 | 8 |
| `recipe.food.roasted_mountain_goat` | 6 | Roasted mountain goat | 1 Raw mountain goat meat + 1 Cedar log + 2 Cedar branches -> 1 | 9 |
| `recipe.food.roasted_buffalo` | 7 | Roasted buffalo | 1 Raw buffalo meat + 1 Redwood log + 2 Redwood branches -> 1 | 10 |
| `recipe.food.roasted_bear` | 8 | Roasted bear | 1 Raw bear meat + 1 Ebony log + 2 Ebony branches -> 1 | 11 |



**Explicit Food pantry extension:** exact Wheat flour/dairy/Honey/Sugar remain available as older inputs; add Maple syrup (tier 5), Sesame oil (7) and Black sesame oil (8) for these and later Food recipes after their sources are discovered. No pantry exception applies to Material/Misc. Main produce, meat, fish, regional spices and fuel remain own/previous tier. No oil or dairy substitution. Food timers remain below one hour; x1/x100/x1000 scale exact costs/yields without 1,000 visual objects.


## 9. Fish and fishery

| Fish ID | Tier | Species | Exact normal roast recipe | Total hearts |
| --- | --- | --- | --- | --- |
| `fish.brook_trout` | 5 | Brook trout | `recipe.food.roasted_brook_trout`: 1 Brook trout + 1 Maple log + 2 Maple branches -> 1 | 8 |
| `fish.smallmouth_bass` | 5 | Smallmouth bass | `recipe.food.roasted_smallmouth_bass`: 1 Smallmouth bass + 1 Maple log + 2 Maple branches -> 1 | 8 |
| `fish.white_crappie` | 5 | White crappie | `recipe.food.roasted_white_crappie`: 1 White crappie + 1 Maple log + 2 Maple branches -> 1 | 8 |
| `fish.sauger` | 5 | Sauger | `recipe.food.roasted_sauger`: 1 Sauger + 1 Maple log + 2 Maple branches -> 1 | 8 |
| `fish.arctic_char` | 6 | Arctic char | `recipe.food.roasted_arctic_char`: 1 Arctic char + 1 Cedar log + 2 Cedar branches -> 1 | 9 |
| `fish.grayling` | 6 | Grayling | `recipe.food.roasted_grayling`: 1 Grayling + 1 Cedar log + 2 Cedar branches -> 1 | 9 |
| `fish.whitefish` | 6 | Whitefish | `recipe.food.roasted_whitefish`: 1 Whitefish + 1 Cedar log + 2 Cedar branches -> 1 | 9 |
| `fish.dolly_varden` | 6 | Dolly Varden | `recipe.food.roasted_dolly_varden`: 1 Dolly Varden + 1 Cedar log + 2 Cedar branches -> 1 | 9 |
| `fish.striped_bass` | 7 | Striped bass | `recipe.food.roasted_striped_bass`: 1 Striped bass + 1 Redwood log + 2 Redwood branches -> 1 | 10 |
| `fish.mullet` | 7 | Mullet | `recipe.food.roasted_mullet`: 1 Mullet + 1 Redwood log + 2 Redwood branches -> 1 | 10 |
| `fish.flounder` | 7 | Flounder | `recipe.food.roasted_flounder`: 1 Flounder + 1 Redwood log + 2 Redwood branches -> 1 | 10 |
| `fish.cobia` | 7 | Cobia | `recipe.food.roasted_cobia`: 1 Cobia + 1 Redwood log + 2 Redwood branches -> 1 | 10 |
| `fish.grouper` | 8 | Grouper | `recipe.food.roasted_grouper`: 1 Grouper + 1 Ebony log + 2 Ebony branches -> 1 | 11 |
| `fish.mahi_mahi` | 8 | Mahi-mahi | `recipe.food.roasted_mahi_mahi`: 1 Mahi-mahi + 1 Ebony log + 2 Ebony branches -> 1 | 11 |
| `fish.amberjack` | 8 | Amberjack | `recipe.food.roasted_amberjack`: 1 Amberjack + 1 Ebony log + 2 Ebony branches -> 1 | 11 |
| `fish.wahoo` | 8 | Wahoo | `recipe.food.roasted_wahoo`: 1 Wahoo + 1 Ebony log + 2 Ebony branches -> 1 | 11 |



Sixteen new fish, four per tier, with all six distinct unordered pairs per tier in the regional table. Combined trail catalog: 32 species. Discovering one region unlocks only its pair, not all four species. Nodes use suitable freshwater/coastal access as listed. Each ordinary roast unlock is nonrandom after its species/fuel sources become accessible; these 16 roasts are not box recipes.


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
| `recipe.food.roasted_scallop` | 5 | Roasted scallop | 1 Scallop + 1 Maple log + 2 Maple branches | 10 |
| `recipe.food.scallop_rye_chowder` | 5 | Scallop rye chowder | 2 Scallop + 2 Rye + 1 Cow cream | 12 |
| `recipe.food.venison_pear_pot` | 5 | Venison pear pot | 2 Raw deer meat + 2 Pears + 1 Maple syrup | 11 |
| `recipe.food.goat_apricot_pot` | 6 | Mountain goat apricot pot | 2 Raw mountain goat meat + 2 Apricots + 1 Fennel seed | 11 |
| `recipe.misc.goat_horn_silk_charm` | 6 | Mountain goat horn silk charm | 1 Mountain goat horn + 1 Cedar plank + 1 Silk cloth | Misc |
| `recipe.food.mussel_buckwheat_bowl` | 6 | Mussel buckwheat bowl | 2 Mussel + 2 Buckwheat + 1 Mountain thyme | 12 |
| `recipe.food.roasted_mussel` | 6 | Roasted mussel | 1 Mussel + 1 Cedar log + 2 Cedar branches | 10 |
| `recipe.food.buffalo_amaranth_pot` | 7 | Buffalo amaranth pot | 2 Raw buffalo meat + 2 Amaranth seeds + 1 Sesame oil | 12 |
| `recipe.misc.buffalo_horn_indigo_token` | 7 | Buffalo horn indigo token | 1 Buffalo horn + 1 Redwood plank + 1 Indigo dye | Misc |
| `recipe.food.octopus_millet_bowl` | 7 | Octopus millet bowl | 2 Octopus + 2 Millet + 1 Tarragon | 12 |
| `recipe.food.roasted_octopus` | 7 | Roasted octopus | 1 Octopus + 1 Redwood log + 2 Redwood branches | 10 |
| `recipe.misc.bear_claw_lotus_charm` | 8 | Bear claw lotus charm | 1 Bear claw + 1 Ebony plank + 1 Lotus cloth | Misc |
| `recipe.food.bear_date_pot` | 8 | Bear date pot | 2 Raw bear meat + 2 Dates + 1 Black sesame oil | 12 |
| `recipe.food.oyster_sorghum_chowder` | 8 | Oyster sorghum chowder | 2 Oyster + 2 Sorghum + 1 Cow cream | 12 |
| `recipe.food.roasted_oyster` | 8 | Roasted oyster | 1 Oyster + 1 Ebony log + 2 Ebony branches | 10 |



Sixteen additions = 12 Food + 4 Misc, all optional box unlocks with no free finished output. Combined with Part 1: **42 box recipes = 33 Food + 9 Misc**. Retain six typed rewards, the proposed 5% recipe-category roll per random slot, learned/reserved exclusion and deterministic duplicate fallback. Physical rewards remain Q5; knowledge has no grade/bag footprint and starts mastery one/zero crafts. No mandatory progression depends on these discoveries. Misc outputs are sale goods, not furniture.


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

Each recipe must author `RequiredRegionIds`, exact shop/interaction source IDs and conversion prerequisites. Resolve these from the region table and recursively through each refined input: Maple syrup needs region 30 plus its conversion, and Maple branches need 25. Merely entering tier 5 is insufficient. Part 1 ingredients retain their exact sources: Wheat 6, Honey 11, Sugar cane 17 and accessible exact dairy. Source access, not currently holding ingredients, gates the roll. Fish eligibility is the union of discovered regional pairs, not the entire tier catalog.

Required negative cases: block maple pear tart without 25 or 30 (plus Wheat 6/dairy); plum brioche without 27 or 28; apricot cake without 31 or 33; blackcurrant pastry without 33 or 36; peach galette without 37 or 39; raspberry cake without 39 or 42; mango tart without 43 or 45; dragon fruit cake without 46 or 47. Scallop/Mussel/Octopus/Oyster require region 25/31/37/43 and reachable fishery; each dish additionally requires its cereal/herb/dairy sources. Trophy crafts require discovered exact hunting sources, eligible tools and accessible refinements.

Check gates at generation and before payment/unlock/claim. Gifts, mastery, quality, gold, purchased refined goods, previews and prepared food never grant discovery. Persist IDs/catalog version, offers/quotes, visits, reservations, paid manifests and recipe knowledge. Reject invalid unlocks without charging/consuming the reward; preserve paid manifests until a valid claim without rerolling. Activate tiers/catalog versions explicitly; disabled expansion data stays outside current catalogs.

P2.1–P2.4 incorporate these tables incrementally: resources/refinements/source gates first; then route/tools/hunting/fishing; then missions and shop/box/bakery integration; finally migration/regressions. Exact tools/tasks still need authoring; none of these tables claims implementation completion. Before activation, check 24 regions/72 distinct resources/18 per tier, six unique pairs per tier and 16 fish; 16 new box and eight new bakery IDs with no collisions; every missing ingredient source individually. Check ×10/−10% prices, mastery-one knowledge, crafted quality independent of Q5 inputs, exact bulk/fuel costs and no reload duplication. Combined two-source collection: **57 optional recipes = 48 Food + 9 Misc** (42 box + 15 bakery); the truffle mission and ordinary regional recipes are additional and excluded from that count.
