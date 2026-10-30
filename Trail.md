# Trail Requirements

## Requirements 1
### Character
- The User avatar can only move forward along the preset path.
- It's speed can be controled by scrolling up or down directly over the avatar, anywhere else in the screen is not appropiate because it would interfere with the game resource pickup mechanics.
- The character has a base speed limit that can be expanded by equiping better items (boots), by default the character wears no boots.
- It should also have a max speed limit like that of brisk walking (the max limit applies for the best boots, could be a speed tier mechanic since there will be many different types), not sprinting.
- The character should have a base health/stamina represented by hearts that can be expanded by equiping better items (hats) by default the character wears no hat.
- The max health/stamina should be 12 hearts for the best hats. (also should be a tiered item mechanic).
- The other 3 character items will be backpack, shirt and pants: shirts add pockets to hold certain small items, pants add hooks to carry tools and backpacks control inventory. by default the character has rag pants, shirt and backpack.
- Said inventory should also have a base size (area).
- The inventory should also have a max size limit.
- The backpack inventory will be like a box without slots, the size will gradually be filled as the user collects resourses.
- The inventory should also allow side scrolling to see the shirt pockets to the right and pant hooks to the left.
- Tools and resources will be stored in the backpack inventory and can be moved to the pockets and hooks respectively.
- The backpack should have a slight aperture in the UI low side to allow pickup animation without having to open the inventory and a backpack button to open the inventory menu with the backpack, pockets and hooks allowing the player to move their resources as they see fit.
- When opening the inventory menu, if moving, the player will stop while the menu is open and resume walking when the menu closes.
- the player will have 2 types of currency: gold and cash.
- cash will work for trades and region unlocking. gold for settlement unlocks and aerostatic ballon store shopping.

### Resources
- resource collection will have 2 action types: hold will allow the user to pick up resources and place them in the backpack inventory as they like, and a quick downward swipe on the resourse will send it directly above the backpack for quick storage.
- resources of the same type will stack in the backpack inventory if they touch each other, this stacking can only occur when the items are in the backpack not in the pockets.
- tools and equipables (boots, hats, shirts, pants, backpacks) do not stack.
- resourses have a base size the grows when stacking (same model bigger size) and consecuently occupies more backpack space.
- pockets allow to place stacked or individual resources in them, their only condition is to hold a single item or stack per pocket.
- pockets should only allow certain resources to be placed in them. ex. stones, ore, eggs, food items or other small resources.
- each region will have set resources that can be found there.
- the next url list the resources from another game similar for reference: https://the-trail.fandom.com/wiki/Items
- if possible to analyze said url make a better resource progression system. (the game i'm using as reference was abandoned so i want to make my own progression system)
- resources should have a progression system that allows the player to obtain more or better resources as they progress through the game regions.
- stack size should be 1000.
- all items: food, materials, tools, etc. have 5 quality levels. said levels influence price and durability.
- durability only applies to tools and wearables: for tools it is measured as a number of uses left, while for wearables it is measured in distance (trail distance walked in kilometers).
- the original game only had 3 types of wood and animals each, but I would like to expand the progression of resources and items.
- for wood consider: pine, birch, mesquite, oak, maple, cedar, redwood, ebony.
- for animals consider: rabbit, beaver, turkey, boar, deer, mountain goat, buffalo, bear.
- for crops consider: wheat, corn, potatoes, rice, barley, oats, strawberries, tomatoes, beans, soybeans, pumpkins, sugar cane, coffe, cacao, vanilla. (some of them will only be sold at the settlement farm store)
- for animal produce consider: Rabbit Pelt, Turkey Feather, Deer Hide, Buffalo Hide, Bear Fur, Egg, Rabbit Fur, Deer Antler, Buffalo Horn, Bear Claw, Raw Rabbit Meat, Raw Turkey Meat, Raw Venison, Raw Goat Meat, Raw Buffalo Meat, Raw Bear Meat, Bear Claws, Raw Beaver Meat, Animal Fat, Boar Tusks, Bones, Goat Horn, Cow Milk, Goat Milk, Cream, Chicken Meat, Duck Meat, Beef, Pork, Lamb, Raw Wool, Beeswax, Honey, Cheese, Butter, Yogurt. (some can be farmed by hunting some crafting and some only bought in the settlement ranch).
- for fish consider: rainbow trout, largemouth bass, common carp, northern pike, salmon, sturgeon, longnose gar, tuna.
- for ores and stone consider: iron, copper, silver, gold, diamond, emerald, ruby, sapphire, quartz, limestone, sandstone, granite, marble, basalt, slate, clay, obsidian, basalt, turquoise, lapis lazuli. (gold ore is for crafting, shouldn't be mixed with gold currency)
- for clothing materials consider: hemp, cotton, leather (by animal type), wool, silk, cashmere, hemp cloth, cotton cloth, wool cloth, fine leather (by animal type), fine cloth, fine silk, fine cashmere fine pelts (by animal type). (some can be gathered, some crafted and some bought at the settlement)
- hunting an animal from the list will give you a random amount of animal produce (depending on the animal and between 2 and 5 items for each type, 4-10 if using tool with drop boost) and a random amount of animal fat (1-3, 1-6 if using tool with drop boost).
- chopping a tree stump will give you a random amount of wood branches and logs (depending on the type of tree and between 2 and 5 for each type, 4-10 if using tool with drop boost), also a random amount of edibles depending on the tree type (could be mushrooms, eggs, honey, sap, fruit, etc. between 2-4 for each type, 4-6 if using a tool with drop boost, at most 2 types per tree).
- chopping a massive tree in the settlement will follow the same logic but with rewards multiplied by 50. the drops will appear progressibely as the player picks up the ones already spawned.
- fishing will give between 2 and 5 fish selected depending the location of the pond. since there are 8 fish there will be 4 tiers based on the regions, 2 fish per region.
- all these actions will provide resources as droped items in the trail, they will not be sent to inventory directly. the user must pick them up.
- ores can be smelted to create ingots.
- tools will have 3 categories: hatchets/axes, slingshots/bows and fishing rods. with 3 tiers (wood/bone/stone, wood/metal, wood/metal/cloth or wood/metal/leather for better grip).
- for each new wood/animal/fish tier there will be 2 tools. for example: tier 1 axe requires 5-6 uses to chop a tier 1 tree, while tier 2 axe requieres 3 uses to chop tier 1 tree and 5-6 for tier 2 trees, and finally tier 3 axe requieres 3 uses for tier 1 and 2 trees and has better drop chances for tier 1, and requieres 5-6 uses for tier 3 trees. 
- tier 1 slingshot requieres 3 shots to fell an animal while tier 2 sligshot requires only 1 use for tier 1 animals, 3 uses for tier 2 animals, and tier 3 sling requieres 1 use to fell tier 1 and 2 animal with improved drop rate for tier 1, and 3 uses to fell a tier 3 animal.
- tier 1 fishing rod requieres 3 uses to catch a tier 1 fish, while tier 2 fishing rod requieres 1 use for tier 1 fish and 3 uses for tier 2 fish, and tier 3 fishing rod requieres 1 use to catch tier 1 and 2 fish with improved drop rate for tier 1, and 3 uses to catch a tier 3 fish.
- all tools require double the amount of uses per higher tier of resource. meaning tier 1 3 uses, tier 2 6 uses, and tier 3 12 uses.
- weareables will consider the previously mentioned clothing and backpacks, the main differences between tiers will be augmented durability, max speed boost (boots), max stamina boost (hats), number of pockets (shirts), number of tool hooks (pants), and more horizontal space for the backpack. special clothing and backpack will be sold by the balloon merchant for each boost tier (special clothing has no durability so it lasts forever)
- inside the inventory by holding touch on the item a small textbox with the name, quality, and quantity will appear. this box will have a close button and in case of food items a eat button.
- wood materials will have 3 levels: branches, logs (obtained from trees) and planks (obtained from crafting said logs).
- the same applies to animal pelts or fur: pelts/leather, fine pelts/leather.
- those final crafted material are adv material for higher tier recipes and house upgrades.
- try to create a crafting progression for each material type. be it for food, tools, clothes, backpacks or even sculptures, jewelry and stuffed toys (these were the main income sources in the original game).
- I repeat all materials, food (meat, fish, crafted food, etc.), sculptures, jewelry and toys are stackable. Tools, clothing and backpacks are not stackable.
- create a variety of crafted foods and beverages. and a few premium goods for the bakery shop.
- add resourses that you belive could integrate with the progression and make the game better.

### Trail
- the trail will be composed of different regions divided in segments market by checkpoints (camps).
- the progression system should be designed to encourage exploration and resource gathering, with rewards for collection missions.
- each cap should have a campfire surrounded by 4 log seats: 1 for the player and 3 for npcs (there will be a trading mechanic in said camps).
- the trading mechanic will involve a conveyor belt animation with the player and npcs placing items for sale. the conveyor will move to the right with a shredder at the end to destroy items that are not sold. destroyed items will give the player 80% of the cash price (only items sold by the player). items to the far left of the conveyor cost 110% of their price. the npcs will sell between 3-5 random items including resources, tools and clothing. trading time last for 15 seconds. only 1 trading oportunity per camp. resets when the player leaves.
- each camp should have a name referencing the terrain that the player will face when leaving or the resources most likely to be found.
- each region has set resources but each segment between camps could have more of a certain resource point or gathering source, thats why the camp previous to said segments should reference the resources or terrain that will be found in the next segment.
- along the trail, players will encounter resources to gather or resource points to interact with, ex. tree stumps to farm wood and certain ingredients, animals to hunt for meat and hides/pelts.
- resource gathering missions should be given by an npc.
- said npc will appear at the start, giving the player their first mission and subsequently each time they complete a mission at the next camp. (it will appear standing, not in one of the 4 seats)
- each time the npc appears, it will give the player a new mission and reward the player with crafting recipes.
- the trail regions should each have a distinct theme or aesthetic that reflects the resources and terrain found within them.
- each region will only reference the theme of said region in the name.
- between the fourth and fifth region the player will arrive to a settlement where he will build his house. 
- said house will work as the player base and will be the player's home for the rest of the game.
- each region limit will be demarcated by a river and require the player to pay a toll to cross on a boat.
- said crossing will be done on a boat that will be available just outside the camp.
- the player will stop in front of an npc that will charge the player a set ammount of money to cross the river.
- said price only needs to be paid the first time the player crosses the river for each region.
- the trail should have plains, hills, forests, and mountains based on the theme of each region.
- some camps will have an aerostatic balloon with a small shop.
- the shop will be managed by an npc and it should provide 3 different items for sale all sold for gold.
- 1 special clothing item (special clothing cannot be crafted and doesn't have durability, so its indestructible), 1 mistery box, and 1 backpack.
- the mistery box should have 6 random items inside with great probability of useful resources of the region, a small probability of clothing or tool items (no special clothing) and a good chace of obtaining tha current mission item.
- the mistery box should not have items from regions not yet visited unless it's the current collection mission item.
- it should also provide more items for the current tier of items unlocked.
- for stackable items the mistery box should provide complete stacks.
- picking up a resourse from the trail (be it lose resources or from farming points) will give you a full stack of it.
- resource trading at the camps will give you a single item.
- along the trail there will be resourse point that the user will interact with to farm for more. ex: chopping a tree stump will give you a random amount of wood branches and logs (depending on the type of tree and between 2 and 5 for each type), also a random amount of edibles depending on the tree type (could be mushrooms, eggs, honey, sap, etc. between 2 and 4).
- said resource points will be: trees (chopping), animals (hunting), ponds (fishing).

### Settlement
- when first arriving to the settlement the player will be greeted by an npc that will offer the player a house to build.
- the house will be a basic structure (like a small wooden shed for lvl 1 not even a house yet) that the player can expand upon as they progress through the game.
- inside the settlement the player cannot be controlled, instead the whole seccion will be controlled by scrolling sideways and selecting the settlement structure that you wish to enter.
- the player home should have a base size grid in which one can place furniture.
- Said furniture should be bought from a home store menu that will be opened with a button beside the inventory button.
- the furniture will provide buffs depending in the type of furniture. for example beds will provide a buff for stamina regeneration for food items, sofas will provide increased durability for boots crafted.
- the buffs are: crafted item quality, food stamina regen, selling price, boots durability, shirts durability, hats durability, pants durability, axe durability, slingshot durability.
- i want to add fishing so a fishing rod (tool) and furniture for durability buffs showld be added.
- distribute the buffs evenly across the different types of furniture.
- each buff only applies for the first buy of each furniture type.
- furniture unlocks based on house level. so the buffs are cumulative since there are multiple furniture types for each buff.
- the home store menu will show all furniture divided by house level. each furniture will show type, buffs, and price in their respective icon slot.
- locked levels will be shown as locked in each level divider.
- furniture should also include chests to store items.
- said chests can also hold and stack items like in the backpacks. there will be different tiers of chests with a taller grid size to hold more items. analyze what would be the optimal max storage space without sendinding items outside screen bounds.
- another furniture should be a shelf with 3 levels for holding 4 items in each level. item size should scale to fit in the rack. stacks cannot be placed in these, only individual items.
- the house upgrade menu should be besides the home store button.
- in this menu the user will add the resources required to upgrade the house.
- the house upgrade menu will also show the current house level and the next level requirements. said requirements must be advanced materials obtained from crafting.
- at house level 3 (a proper wooden house), the player will unlock the pets section in the home store menu.
- also at level 3 the home front will have a pet house to the left of the home front which can be selected and will open a new menu where the user can select a pet to be placed in the house. And a pet collection container in wich the pet will deposit useful resourses when the player closes the game for some time (idle resources).
- for lvl 1 and 2 of the house the inside grid will grow sideways and slightly upwards. the subsequent levels will grow upwards only.
- adjust furniture sizes to fit the grid. without single furniture overcrowding each level.
- each level should unlock 6 furniture types. Buffs that couldn't unlock at said level should be in the next level of furniture.
- furniture types should include a variety of items that fit the theme of the game, such as decorative items, production facilities and storage solutions.
- back to the settlement there should be a trading house. the house will present boxes for the player to fill with items. each box with a capacity of 10 items. 1 item type per box. 2 boxes at once can be filled. when one box is filled, an empty one takes its place and the player recieves 100% of the selling price for his items in cash. when 10 boxes have been filled the player will recieve an ammount of gold based on 3% of the price of the last ten boxes filled.
- another structure should be a lumber field, the field will show 2 rows of massive trees divided in tiers. 6 trees per tier. said trees will be harder to cut down but give the player massive rewards. base ammount of tool uses per tier is 20 and will halve for each higher tier of tool used. each tier of the lumber field will have a unique type of massive tree based on the progression. each tier locked until bought with gold (should be a fair amount since the user will no longer have to travel between camps to farm wood). when a tree is cut it will not respawn until 3 real days later. items not picked before respawn will be deleted.
- there should be a bakery, the bakery will sell bread and other baked goods in full stacks. the goods will update each time the player leaves the settlement. like the other store only 3 item stacks will be shown at once. selected at random each time the player enters the settlement. 1 item should be bought with cash (common food with stamina regen of 1 to 6 hearts) and 2 gold items (premium goods with stamina regen of up to 12 hearts)
- there should be a ranch, the ranch will sell animal products such as meat, milk and cheese (excluding leather and pelts since those can be farmed by hunting) in full stacks. the products will update each time the player leaves the settlement. like the other store only 3 item stacks will be shown at once. selected at random each time the player enters the settlement. these items will all be bought with cash.
- there should be a farm, the farm will sell crops such as wheat, corn and potatoes in full stacks. the crops will update each time the player leaves the settlement. like the other store only 3 item stacks will be shown at once. selected at random each time the player enters the settlement. these items will all be bought with cash.
- there should be a mine, the mine will sell stone and ore resources in full stacks. the resources will update each time the player leaves the settlement. like the other store only 3 item stacks will be shown at once. selected at random each time the player enters the settlement. these items will all be bought with cash.
- ther should be a fishery, the fishery will sell fish such as rainbow trout, largemouth bass, common carp, northern pike, salmon, sturgeon, longnose gar, tuna in full stacks. the fish will update each time the player leaves the settlement. like the other store only 3 item stacks will be shown at once. selected at random each time the player enters the settlement. these items will all be bought with cash.
- there should be a tailor material shop, selling clothing materials in full stacks. the materials will update each time the player leaves the settlement. like the other store only 3 item stacks will be shown at once. selected at random each time the player enters the settlement. these items will all be bought with cash.

### Mechanics extra details
- when picking a collection mission there will be 3 categories: gatherer, hunter, lumberjack. the player can only select one mission at a time and the 3 categories are 3 different gathering progressions. you must complete the current mission of the category selected to unlock the next. each category has a list of recipes including food, clothing and misc (sculpture, jewelry, toys), based on the progression of items. some missions require gathering, some require crafted materials or items. each mission unlocks a new crafting recipe.
- items required for the current mission will disappear from the player inventory and count towards the mission completion. items inside chests in the player house do not disappear until they are taken to the personal inventory. if the player finishes the mission while in the settlement, the mission npc will appear when the player moves to a camp.
- above the inventory button a mission counter will appear showing the current mission progress. said counter wont show while in the settlement but items taken will still count towards the mission completion. the counter will show the number of items required for the current mission and the number of items collected alongside the item icon.
- for the crafting mechanic there will be a menu with a square divided in 9 sections and 2 buttons to the right, one to craft and 1 to autofill the slots. said menu will show the item requirements for the current crafting recipe. alongside the crafting menu the inventory will open too allowing the user to place the materials in the crafting menu. the original game allowed only crafting items one at a time, I would like to add bulk crafting x1, x10, x100 and x1000.
- crafting recipes have a waiting timer, the waiting timer is shown in the crafted product. ex: crafting food gives you the item with a timer of 10 seconds, and you cannot eat it until the timer is over. the same for tools and wearables. items with active timer cannot be sold either.
- for bulk crafting the items should stack since they share the same timer, but when crafting one by one the items should not stack until the crafting timer is over. touching on the matter of bulk crafting and unstackable items, I would like to make the item stackable for bulk crafting but once an item is taken from the stack it returns to being unstackable (if said mechanic proves to difficult or impossible just hide bulk crafting when selecting unstackable item recipes).
- in the top right corner of the crafting menu a button for the recipe book should appear, allowing the user to select the recipe they want to craft. near the bottom right corner a return button should appear to close the crafting and inventory menus.
- crafting should only be available at camps and the player home.
- while at camps or in the settlement there should be 2 buttons apart from the inventory in the bottom left, these 2 buttons should be placed to the right one at the top, one at the bottom, the botom one should open a button list from the bottom up with the options for crafting, equipment, map and settings/configuration. the top one should show a list of buttons from top to bottom for home, settlement warehouse (trading boxes), lumber field, return to trail.
- the crafting button should open the crafting menu and the equipment button should open the equipment menu. both should open alongside the inventory menu.
- the map will show all discovered regions and the next undiscovered one when openen in the settlement allowing the player to travel by region (it will send you to the first camp of the region selected, only discovered regions). if opened from a camp it will show the regional map listing the discovered camps, allowing travel beteween discovered camps.
- settings will show the general settings for the game such as audio, graphics, controls, and language.
- the home button will send the player directly to the home front.
- the warehouse button will send the player to the settlement warehouse (trading boxes).
- the lumber field button will send the player to the lumber field.
- and the return to trail button will send the player back to the trail to the last camp visited before going to the settlement.
- return to trail button is only available at the settlement.
- crafting is only available at camps and the player home.
- crafting, home, warehouse, lumber field, and return to trail buttons are not available in between camps.
- random level neighbor houses should be added to the settlement too.
- when chopping massive trees in the lumber field the items will spawn in different piles by type. non collected items will disapear when the tree respawns. all 6 trees per tier of the lumber field can be cut and farmed individualy with individual respawn timers.
- for the crafting recipes, some of them are to creft materials for example, 3 branches to craft 1 log, 3 logs to craft 1 plank, those recipes have no level and
  the resulting product qualty is only influenced bu the craftign quality furniture buff, all other recipes that are not materials, have levels 1-10 depending on how manyy have
  been crafted, each level improves quality, price, durability (if tool or wearable) and crafting timer, the crafting timer mechanic varies by item type, for tools and food each
  level decreases crafting timer, for weareables and misc each level increases crafting timer, add to that that crafting timers get bigger based on progression, lower items take
  seconds to complete, higher items could take hours. tools and food have relatively short timers, none reaching an hour. quality is a mixture of the base set by level up to level
  5 since quality only has 5 levels, beyond level 5 only price, durability and timers see diffference and the crafting quality buff, said buff adds price and durability that
  counts towards quality and then durability buffs are added too for items that apply wich in turn raises the item price once more, while keeping the timer as is. so in resume
  each item has a base price and durability that grows with level + quality buff lvl 5 being the maximum + durability buff. also material recipes have no timer if the material can
  be gathered too, like logs. for each tier material recipes are the first collection missions.

### revisions missions and region resources
instead of 3 mission brenches lets make it 4: gatherer, lumberjack, hunter, explorer. each branch will give a clothing recipe of the respective tier to complete the set (hat, boots, shirt, pants).
for example tier 1;
gathering: mission 1: gather 10 branches - reward: logs recipe -> mission 2: gather 10 apples - reward: apple juice recipe -> mission 3: gather 10 sculptures - reward: tier 1 hat recipe
lumberjack: mission 1: gather 10 stones - reward: tier 1 axe -> mission 2 gather 10 logs - reward: sculpture recipe -> mission 3: gather 10 tier 1 axes - reward: tier 1 boots recipe
hunter: mission 1: gather 10 pelt - reward: tier 1 slingshot -> mission 2: gather 10 tier 1 slingshot - reward: stuffed toy recipe -> mission 3: gather 10 stuffed toys - reward: tier 1 shirt recipe
explorer: mission 1: gather 10 fiber - reward: cloth recipe -> mission 2: gather 10 meat - reward: roasted meat recipe -> mission 3: gather 10 tier 1 cloth - reward: tier 1 pants recipe
subsequent mission should unlock adv materials for each animal or wood type, and backpack, and fill in with more food recipes, misc crafting and starting from tier 2 normal and elite tools and even up to 2 clothing sets per tier.
in order to make that possible each region should have 3 materials most likely to be found in that region for gathering, and their respective tier of wood, animal and fishing interactibles. these 3 materials refer only to gathering, as mentiones tree chopping, animal hunting and fishing have interactibles in all zones.
the 3 resources should be different for each region, and should raise in tier as the player progresses. for example considering 24 zones divided in 4 tiers, the first 6 regions will have tier 1 interactibles and a total of 18 tier 1 gathering items. 2 of those items will be branches and pelts those are the only items that can be found gathering and interacting with ther respective object. meat (or other hunting reward), logs and fish are not part of that list of 18.
when opening the map, each region will display the icon for the 3 most likely gathering resources. for example, in the original game region 1 had branches, pelts and snail shells. in this case snail shells were useless so they shall be replaced with something usefull, region 2 had fiber (for cloth), stone and pinecones (again pinecones were useless but if a recipe using them can be made that would be for the best).
so apart from interactable resources there should be 18 gathering items. with tier distribution by region advancement. and the collection missions should follow said gathering list distribution and interactable adquisition.
apart from that tier 1 and 2 regions had less camps, between 10 and 14 camps per region, starting at the end of tier 2 and into tier 3. regions could go up to 24 camps per region.
and the distance between camps should take into account the stamina limit. for the first region starting camps the player has no food to replenish stamina, so they should be able to reach it without exhausting the 3 initial hearts preferably the first region will allow the player to unlock at least 1 food recipe so starting at region 2 the distance between camps increases forcing the player to eat.if stamina reaches 0 and the player has no food the player will faint and stamina will start recharging. but theres a penalty if the user faints a random amount of items/stacks will disappear from his inventory including tools or clothing in inventory and excluding items equipped.
after stamina has been recovered and the penalty applied the player will wake up and be able to continue playing. i believe an animation of the player fainting and waking up should be added. alongside an animation of between 2 and 4 items/ stacks slipping out of inventory and disappearing.


