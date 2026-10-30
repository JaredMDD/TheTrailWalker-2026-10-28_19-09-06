# TheTrailWalker: Project Recommendations

The current design analysis and detailed implementation plan are in [PROJECT_RECOMENDATIONS.md](PROJECT_RECOMENDATIONS.md), using the filename requested on October 29, 2026.

That document incorporates the original project assessment, reviews `Trail.md`, and preserves the confirmed rule that stackable trail pickups give full stacks of 1,000 units during mechanics and limits testing.

It also covers unlevelled material conversions, recipe mastery levels 1-10, capped quality and ordered furniture bonuses, and category-specific timers. The latest `Trail.md` revision supersedes the old category-order mission gate with four prerequisite-based branches: Gatherer, Lumberjack, Hunter, and Explorer, awarding hat/boots/shirt/pants respectively.

The latest rules include Normal/Elite tools from tier two, passive aimed hunting, safe-area stamina regeneration, precise mission consumption with worn-gear protection, x1-only unstackable crafting, first-purchase furniture buffs, and exclusive 15-second trading with 70% Return cancellation.

Camera orbit now allows 360-degree horizontal viewing without stopping movement. Worn gear disappears at zero distance durability and restores defaults. Harvesting leaves the player stopped until an upward speed gesture, while inventory close restores the valid pre-open speed.

Current scope is four active wood/animal tiers over 24 regions, with four future tiers deferred. Starter hooks/pockets are zero and both cap at ten; stack model growth caps at 1.5x, and inventory uses size only. Recipes are simplified, materials can support containers/house upgrades, and furniture remains store-purchased.

Every recipe now requires exact ingredient item IDs/quantities with no substitutions. Cow, goat, and mixed-milk cheeses are separate recipe examples; any mixed recipe explicitly requires both varieties, and nonmaterial mastery/unlocks are separate per recipe ID.

Pricing now distinguishes the one-cash basic-material floor and 6,000-cash level-ten Misc Q1-equivalent reference from finished value. The main plan defines the category order, proposed four-tier reference prices, quality premium, applicable durability premium, and 64-bit full-stack accounting.

All store purchases are Q5, including settlement/balloon goods and purchased mystery-box rewards. Camp conveyor trading preserves the listing's actual quality. Q5 does not automatically grant recipe mastery, a higher equipment tier, or indestructibility.

The current regional draft assigns three featured loose resources per region (18 distinct per tier), separate tree/hunting/fishing sources, the approved tier-three turkey-feather overlap, early 10-14/later up-to-24 camp layouts, region-one food unlocks, and faint/recovery with 2-4 eligible-entry loss. Highest-tier camp-to-camp walking cost is capped at 26 hearts; the stamina meter still caps at twelve and food supplies the difference.

Elite crafting now consumes one same-tier Normal tool plus previous-tier advanced materials. Elite tools need fewer actions at/below their tier and boost drops on all eligible targets, including up to two tiers above. Mission counts start at ten and rise up to 150, with proposed intermediate defaults 50/100 and recursive cost reports that include Normal tools, refinement, house reserves, and x1 crafting workload. Testing pickups remain full stacks.

The tools section now explicitly lists pine axe/slingshot/rod. A proposed sixteen-fish catalog gives six unique regional pairs per tier, and roasts consume named wood. Recipes use their unlock tier and previous tier (tier one uses only one), with exact-ID cooking pantry exceptions such as flour/cheese; global ingredients never mean interchangeable varieties.

Fishery resets now reserve two Q5 cash fish offers plus one random Q5 cash-exclusive seafood offer, capped at one 1,000-unit stack per slot/reset. The draft exclusive pool is Shrimp/Crab/Squid/Lobster. Balloon boxes can award optional recipe knowledge, with a proposed twenty-six-recipe collection (twenty-one Food/five Misc), source/tier filters, duplicate protection, and mastery-one unlocks; their physical rewards remain Q5.

The latest additions include eight optional regional meals restoring 6-10 total hearts and a proposed twelve-heart premium bakery apple pie. Food recovery is progressive while walking, retaining excess hearts in a saved queue. Camp maps show current-region camps and other discovered regions below the first camp; local travel uses the selected camp, cross-region travel the destination's first camp. Both EN and ES plans include persistence and acceptance checks.

Unity settings and gameplay code have not been changed by this documentation update.

Section 20 now records the confirmed answers: crafting stays open with center-grid product presentation and immediate timed-product delivery; free placement without rotation; family-specific tool durability (+10/tier, Elite double); discounted level-one random-quality NPC gear without furniture buffs; ID-only missions; horizontal chest storage; and warehouse acceptance of all sellable categories at full cash plus direct 3% gold per ten boxes, without extra bonuses. Delegated defaults cover overflow and free travel/shop resets. Gathering remains 1,000 per pickup throughout testing; the one-item collection design is deferred until explicitly requested, with no automatic post-testing switch.

Pets now have a proposed six-species roster with distinct reward weights, home-store purchases, pet-home selection and a separate idle collection container. Dog favors wood/minerals; Pig specializes in discovered truffles. Truffle is proposed for region 10 in place of loose Lavender (moved to the farm catalog), preserving three resources per region. Three exact truffle dishes add one mission recipe and two box recipes, raising the optional box collection to 26. Both languages include source filters, idle caps, switching, persistence and acceptance checks.

Region names now follow map landscapes rather than resource tiers, with 24 localized names and a complete ordinal naming rule for camps. Settlement ingredient/material shops refresh on actual departure and return, with no timer. Balloon offers, including the special clothing shown, reroll whenever the shop is closed and reopened; already purchased mystery-box manifests remain fixed. Section 20 and the implementation/save checks reflect these confirmed refresh rules.

Trail interactables and gathering-point positions are fixed. Returning to a segment's previous/start camp creates a new random drop cycle for that segment; reopening/reloading preserves the current cycle. Section 7.3 in both languages defines fixed point IDs, per-cycle manifests, camp-return persistence and verification.

The current milestone plan is explicitly limited to debug and testing. The 24 regions/four defined tiers are the initial base, with higher-tier materials/content still pending definition and implementation. Milestones 16-18 retain 1,000-unit pickups and debug grants/tools, validate implemented gates with recorded test bypasses, and produce local development builds only. Rebalancing, debug removal/disablement, release collection/preparation, and deployment/publishing remain deferred until explicitly requested in future prompts.

Milestone 2 is implemented as a separate CollectionBootstrap/CollectionTrail development prototype: full-stack pickups, deterministic spatial inventory, compatible merging, resource gesture ownership, rear-window/range/occlusion checks, and retained debug presets. The detailed EN/ES plans record 30 inventory checks, 1,000 randomized operations, 34 movement regressions, and 12 device checks. Milestone 3 now adds versioned session persistence and backup recovery for this prototype.

First-person gathering revision: extend practical pickup reach toward visible, reliably touchable unobstructed targets, reserve the top system-gesture area, and retest Milestones 1-3 at higher speeds on varied phone screens. The Milestone 1 and 2 prototypes now use first-person view and extended visible reach; broader device and maximum-speed usability testing remains pending.

Milestone 3 now saves the prototype session, inventory and world drops with checksummed atomic snapshots and a recoverable backup. Force-close, inventory pause, camera yaw, post-harvest stop and corrupted-primary recovery were verified on the connected phone; debug reset archives the old profile after confirmation.
