# TheTrailWalker: Recomendaciones Parte 2 — Progresión posterior al rango 4

**Revisión de lógica de progresión - 6 de noviembre de 2026:** puentes de construcción antes del asentamiento, orden separado de códigos estables, despensa explícita para llenado y capacidad exótica por oportunidades separadas de pilas. Véanse Parte 1 secciones 9.0.1 y 11.6.1; Parte 2 secciones 10.1, 15 y 16.5.1. **Diseño actualizado; esta edición no certifica implementación, pruebas, compilación ni dispositivo.** La otra herramienta debe revisar [la nota de lógica y pruebas](Docs/PROJECT_HANDOFF.md#logic-review).

Actualizado: 6 de noviembre de 2026 (las gemas se tallan o pulen antes de usarse en recuerdos y conjuntos B: secciones 10.3, 13.1, 14.1 y 15). Antes: 5 de noviembre de 2026. Las secciones 13-16 añaden las mejoras de casa y asentamiento, los conjuntos de equipo, las herramientas, las recetas añadidas y el plan completo de misiones de los rangos 5-8, siguiendo las secciones 9.6 y 11.6 de la Parte 1; la sección 10.1 añade las bebidas en barril.

## 1. Alcance y estado

Esta parte reúne avances futuros reservados en [Parte 1](PROJECT_RECOMENDATIONS_ES.md), secciones 6.2.1, 6.3, 8, 9 y 13, y propone cómo definirlos. Cubre **rangos de recursos 5–8 y sus regiones futuras**, posteriores a las 24 regiones/cuatro rangos actuales. Región, rango de recurso, dominio, calidad y nivel de casa son distintos: región 5 es rango 1 y no inicia la expansión.

**Estado: diseño pendiente, sin implementar.** Parte 1 conserva autoridad sobre mecánicas/pruebas actuales. Crear Parte 2 no activa contenido ni declara hitos completos. “Heredado” significa ya mencionado; “propuesto” es recomendación nueva; “abierto” requiere datos/decisión.

## 2. Avances heredados

| Rango futuro | Madera / animal reservado | Materiales y comida aplazados en Parte 1 |
| --- | --- | --- |
| 5 | Arce / ciervo | Madera de arce, piel de ciervo, recetas de venado y asta. Sección 8 propone savia de arce y conversión exacta de jarabe, antes sin definir en Parte 1. |
| 6 | Cedro / cabra montés | Madera de cedro, piel/carne de cabra cazada y cuernos. Cabra montés es distinta de cabra doméstica lechera; leche no concede acceso a caza. |
| 7 | Secuoya / búfalo | Madera de secuoya, piel/carne de búfalo y cuernos. |
| 8 | Ébano / oso | Madera de ébano, pelaje/carne de oso y garra. Conservar un concepto único de garra en catálogo. |

Parte 1 reservó las parejas; secciones 7–12 ahora proponen regiones, fuentes, recetas, rendimientos y precios seleccionados. Seda/cachemira estaban sin asignar; esta propuesta establece fuentes y conversiones de rango 6 explícitas, con Goat down recolectado separadamente de caza. Lana opcional aprobada conserva su lugar. Todas las asignaciones nuevas siguen siendo propuestas de diseño.

**Propuesta:** extender ramas/troncos/tablas a cada madera cuando existan IDs, fuentes y costes. Definir piel/pelaje crudos/procesados útiles sin recompensas duplicadas salvo configuración explícita. Cada material necesita fuente y uso en receta/mejora de casa. Hitos actuales no requieren arte, recetas ni clases de expansión.

## 3. Reglas para expansión

- IDs exactos. Extensión propuesta de ventana de ingredientes: rango 5 acepta 4/5; 6 acepta 5/6; 7 acepta 6/7; 8 acepta 7/8. Solo despensa Food explícita puede ser anterior. Definir cada receta antes de usar la extensión como datos ejecutables.
- Usar comportamiento Normal/Élite existente: Normal en cada rango; Élite consume Normal exacta del mismo rango y materiales avanzados explícitos del anterior. La sección 13 define las herramientas 5–8 (Normal/Élite) y **dos conjuntos de equipo por rango** (Conjunto A de las regiones tempranas del bloque, Conjunto B de las tardías).
- Cuatro ramas de misiones, una activa global. Exigir IDs/cantidades y requisitos accesibles; recetas aleatorias no bloquean progreso obligatorio. La sección 15 planifica cada misión de los rangos 5–8: **150 unidades** en pedidos apilables y **40 instancias** en herramientas y prendas, sin superar nunca el límite de campaña.
- Tres recursos sueltos destacados por región. Este borrador propone seis regiones por rango futuro: 24 regiones y 72 entradas nuevas (sección 7). Carne/troncos/peces no añaden espacios sueltos. Sección 9 propone especies y parejas regionales.
- Límites actuales ya disponibles: doce corazones, diez bolsillos, diez ganchos, pilas de 1.000 y crecimiento 1.5x. No reservarlos para expansión ni aumentar automáticamente velocidad, capacidad o presupuesto de 26 corazones por tramo. **Sin estadísticas nuevas (usuario, 5 de noviembre de 2026): el rango cuatro es el límite de capacidades**; los rangos 5–8 añaden durabilidad, alcance de herramientas y variedad, no corazones, velocidad, bolsillos, ganchos ni espacio (sección 13).
- **Toda receta es recompensa de misión (usuario, 5 de noviembre de 2026)**, incluidos materiales, Food y Misc, sin desbloqueos por descubrimiento. Cada especie de pez es una misión de recolección que enseña un plato variado, no un asado simple (sección 14.2). Una receta solo puede venderse en tienda o salir en caja si es premium (sección 14.4).
- Mantener recuperación progresiva y reserva pendiente. La comida de misión normal llega como máximo a 6 corazones por porción (nivel del rango cuatro); la comida premium de caja usa la banda del rango cuatro de la Parte 1, 11–12, y la de panadería 12 (sección 14.2). Secciones 8–11 proponen precios, conservando probabilidades existentes. Las siete recetas de rangos 1–4 siguen elegibles; ocho nuevas amplían panadería a quince.
- Muebles comprados; materiales sirven a ingredientes, recipientes y mejoras de casa. Maderas/trofeos no implican fabricar muebles. La sección 16 añade los niveles de casa 9–16, muebles decorativos, cuatro aspectos más de tienda y cuatro secciones del campo maderero, sin bonificaciones más fuertes.

## 4. Descubrimiento y guardado

Exigir descubrimiento normal del rango de receta y **cada ingrediente exacto, con al menos una fuente autorizada descubierta y accesible por ingrediente** antes de ofrecer/desbloquear. Seguir refinados hasta materias primas y exigir conversiones conocidas o tiendas exactas accesibles pertinentes. Dominio, oro, regalos, comida comprada o productos de cabra doméstica no prueban descubrimiento futuro.

Validar al generar ofertas y antes de cobrar/desbloquear, también tras recarga/migración. Vistas bloqueadas muestran fuentes pendientes sin otorgar conocimiento. Guardar descubrimiento, recetas/dominio, stock, precios, visitas y transacciones; no deducir descubrimiento de inventario, sortear premios pagados ni cobrar dos veces. Conservar reglas de rangos 1–4.

Excluir contenido futuro de botín, herramientas, misiones, tiendas, cajas, mascotas y costes actuales hasta implementación explícita. Migrar/aislar IDs desactivados en guardados sin reinterpretar especies ni eliminar derechos pagados. Conservar IDs, cantidades y perfiles.

## 5. Hitos de expansión propuestos

Posteriores a sistemas base pertinentes; no renumeran Parte 1 ni declaran sus hitos aprobados.

| Paso | Trabajo | Aceptación |
| --- | --- | --- |
| P2.1 — Definir muestra de rango 5 | Usar las secciones 7–8 y 13–15 (regiones, fuentes, IDs de Arce/ciervo, recetas, herramientas Normal/Élite, los dos conjuntos de equipo y las misiones T5-G1…T5-E14); acceso desde rango 4. | Fuentes/recetas exactas; sin ingrediente obligatorio inaccesible ni dependencia anterior ausente. |
| P2.2 — Validar ruta de rango 5 | Integrar tramo/campamento, descubrimiento, recolección, comida, equipo, misiones y migración. | Guardados anteriores conservados; fuentes pendientes bloquean recetas; transferencias/pérdidas/ciclos una vez. Mantener pilas completas/depuración. |
| P2.3 — Completar rango 5 aprobado | Tablas regionales, rutas, economía, filtros de tiendas/cajas/mascotas y usos en casa. | Cantidades coinciden; recetas accesibles; catálogos anteriores conservan significado. |
| P2.4 — Repetir rangos 6–8 | Revisar/implementar Cedro/cabra, Secuoya/búfalo y Ébano/oso con secciones 7–12; revisar fuentes propuestas de seda/cachemira de rango 6. | Probar fuentes/regiones ausentes; impedir contenido futuro en recompensas/requisitos anteriores. |

## 6. Pendientes y comprobaciones

Resuelto en las secciones 13–16 (5 de noviembre de 2026): tareas/recompensas de misiones, herramientas/durabilidad, conjuntos de equipo, campamentos por región (16–20) y niveles de casa 9–16 con sus costes. Abiertos: geometría/drenaje de rutas, cantidades detalladas de recolección, temporizadores/valoración, tablas generales de efectivo y grupos/pesos de mascotas para materiales nuevos. Secciones 7–12 aportan catálogos iniciales propuestos de regiones/recursos/peces/recetas y precios seleccionados. Revisarlos antes de implementar.

Probar cada receta con requisitos completos y cada fuente/conversión ausente individualmente. Incluir regalos, refinados de tienda, duplicados, vistas bloqueadas, pagos interrumpidos y recarga/migración. Verificar consumo exacto, reemplazo Normal/Élite, propiedad, totales de 64 bits y rendimiento móvil con pilas completas.

Continúa política de pruebas: recogidas generadas de 1.000 y depuración disponibles. Planificar expansión o terminar pruebas no activa equilibrio, retirada de depuración, recogida unitaria, preparación de lanzamiento ni publicación.

**Actualización de catálogo:** las secciones 7–12 ahora proponen datos concretos para regiones 25–48. Las notas de pendientes anteriores describen el estado inicial; tablas nuevas resuelven regiones/recetas listadas como propuestas, mientras herramientas, misiones y costes de casa detallados siguen abiertos.

## 7. Catálogo propuesto de regiones y recursos

**Primera propuesta poblada; diseño, sin activación en runtime.** Se adoptan como propuesta seis regiones por rango: regiones 25–48, tres recursos sueltos distintos por región, 18 por rango y 72 nuevos. Total diseñado: 48 regiones y 144 recursos sueltos. Los nombres e IDs técnicos de tablas se mantienen en inglés en ambos documentos para conservar identidad; localizar nombres visibles al implementar. **Campamentos por región (confirmado por el usuario, 5 de noviembre de 2026): el número no aumenta después del rango cuatro.** Cada región sortea 16–20 campamentos como el rango cuatro (semilla 20261105, columna de la tabla; 104–110 por rango); máximo 26 corazones por tramo, sin subir límites ni introducir peligros climáticos.


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



Ramas y piel/pelaje son coincidencias base; los ingredientes de árbol de sección 8 son coincidencias adicionales explícitas LooseGathering/TreeHarvest. Troncos, carne, astas/cuernos/garras y peces son resultados de interacción, sin iconos sueltos adicionales. En este catálogo, los plurales/singulares de tablas posteriores apuntan al ID exacto anterior, sin crear variantes. Goat down es fibra muda recolectable en región 35, no segundo botín de piel de caza. Silk cocoons se recolectan en región 34; Mulberry leaves tienen uso artesanal, sin criar gusanos ni mascota nueva. Estas fuentes y la asignación seda/cachemira a rango 6 son propuestas explícitas nuevas.


## 8. Procesamiento, caza y recetas regionales

Ingredientes exactos: IDs crudos explícitos en sección 7; productos usan `item.<sufijo_de_receta>`, excepto conversiones de troncos: `item.maple_log`, `item.cedar_log`, `item.redwood_log`, `item.ebony_log`, también usados por árboles. Singular/plural de una misma variedad resuelve al mismo ID: `Maple branch`/`Maple branches` apuntan a `resource.maple_branches`; nunca permite sustituir variedades. Carnes: `item.raw_deer_meat`, `item.raw_mountain_goat_meat`, `item.raw_buffalo_meat`, `item.raw_bear_meat`. Trofeos: `item.deer_antler`, `item.mountain_goat_horn`, `item.buffalo_horn`, `item.bear_claw`. Piel/pelaje de caza usa los mismos IDs de recursos sueltos. Peces y mariscos tienen IDs en sección 9.

Material no tiene dominio y usa temporizador fijo; Food/Misc conservan reglas de Parte 1. No fabricar muebles. Costes simplificados sin combustible oculto. **Toda receta de esta sección es recompensa de misión (sección 15, 5 de noviembre de 2026).** Nunca exigir receta aleatoria de caja/panadería para estas conversiones o comidas normales. Definir temporizadores/valoración de rangos 5–8 antes de activar, sin extrapolar silenciosamente curvas numéricas de cuatro rangos.


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



**Interacciones y conversiones exactas propuestas:**


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



Las cuatro conversiones de ramas a tronco están enumeradas explícitamente en la tabla; las tablas de madera consumen troncos de árbol o conversión. Las recetas Misc de trofeos consumen el trofeo crudo exacto, sin refinado oculto. Tablas de cantidad/Élite conservan comportamiento del servicio existente; no se inventan multiplicadores por tier. Herramientas exactas, desgaste y requisitos de misiones de expansión siguen pendientes de autoría y se necesitan antes de activar regiones.


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
| `recipe.food.roasted_deer` | 5 | Roasted deer | 1 Raw deer meat + 1 Maple log + 2 Maple branches -> 1 | 6 |
| `recipe.food.roasted_mountain_goat` | 6 | Roasted mountain goat | 1 Raw mountain goat meat + 1 Cedar log + 2 Cedar branches -> 1 | 6 |
| `recipe.food.roasted_buffalo` | 7 | Roasted buffalo | 1 Raw buffalo meat + 1 Redwood log + 2 Redwood branches -> 1 | 6 |
| `recipe.food.roasted_bear` | 8 | Roasted bear | 1 Raw bear meat + 1 Ebony log + 2 Ebony branches -> 1 | 6 |



**Despensa Food ampliada explícita:** Wheat flour, lácteos exactos, Honey y Sugar siguen disponibles como ingredientes anteriores; Maple syrup (rango 5), Sesame oil (7) y Black sesame oil (8) se añaden para estas recetas Food y posteriores cuando sus fuentes estén descubiertas. Ninguna excepción se aplica a Material/Misc. Fruta, carne, pescado, especias regionales y combustible permanecen en rango propio/anterior. No sustituir aceites ni leche/queso. Temporizadores Food inferiores a una hora; lotes x1/x100/x1000 escalan ingredientes/rendimiento exactos y no necesitan 1.000 objetos visuales.


**Corrección de ingredientes obligatorios y fuentes alternativas:** aplicar Parte 1 sección 7.1.1 a todos los árboles futuros, incluidos masivos: ramas + troncos + un ingrediente obligatorio, con segundo opcional hasta máximo dos. Arce entrega Maple sap; Cedro, Apricots; Secuoya, Peaches; Ébano, Mangoes. Son los mismos IDs de recursos sueltos, no variantes nuevas; conservan fuentes sueltas en regiones 30/31/37/43. No añadir cuarto icono ni contar ID dos veces. Son ingredientes crudos, no comida preparada automática; sus recetas de jarabe, platos, cajas y panadería proporcionan usos.

Descubrir y tener acceso de cosecha con herramienta válida a árbol en 25–30 / 31–36 / 37–42 / 43–48 habilita su fuente alimentaria; poseer regalo no. Usar OR entre fuentes válidas de un ingrediente, AND entre ingredientes diferentes. Árbol de arce en 25 permite savia para tarta sin descubrir 30; árbol de cedro en 33 permite albaricoque sin exigir fuente suelta 31. Mantener conversión conocida, otros ingredientes y fuentes exactas de harina/lácteos. Revalidar ofertas guardadas con tabla corregida sin sortear ni cobrar otra vez.

Aplicar cantidades de Parte 1: Normal 2–4 pilas de ingrediente, Élite 4–6, 1.000 unidades por pila; masivo x50 en comida y madera. Guardar manifiesto antes de mostrar botín. P2.1–P2.4 prueban cuatro árboles, alimento obligatorio, máximo dos, cantidades Normal/Élite/masivo y reclamación/recarga una vez. Probar fuentes alternativas positivas (árbol descubierto sin fuente suelta; fuente suelta descubierta sin acceso a árbol) y negativas (ninguna accesible). Corrige tabla anterior de solo madera y exigencia de descubrir todas las fuentes de un mismo ingrediente.

## 9. Peces y pescadería

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



Dieciséis peces nuevos, cuatro por rango; seis parejas distintas por rango en tabla regional. Total 32 especies de sendero con Parte 1. Descubrir solo una región habilita únicamente las especies de su pareja, no las cuatro del rango. Cada pez usa su nodo/hábitat; no se mezcla agua dulce y mar sin acceso adecuado. **Revisado (usuario, 5 de noviembre de 2026):** los rangos 5–8 no tienen pescados simplemente asados. Cada especie es una misión de recolección que enseña su plato de la tabla (sección 15); ninguno es receta de caja.


| Exclusive ID | Tier | Ingredient | Required discovered region | Proposed Q5 cash/unit | Cash/full 1,000 stack |
| --- | --- | --- | --- | --- | --- |
| `fishery.scallop` | 5 | Scallop | 25 | 500 | 500000 |
| `fishery.mussel` | 6 | Mussel | 31 | 800 | 800000 |
| `fishery.octopus` | 7 | Octopus | 37 | 1200 | 1200000 |
| `fishery.oyster` | 8 | Oyster | 43 | 1800 | 1800000 |



Estos cuatro ingredientes exclusivos sirven a recetas premium opcionales de cajas; siguen siendo **efectivo, no oro**. Con los cuatro anteriores hay ocho exclusivos. Mantener tres ofertas Q5: dos peces distintos de especies descubiertas y un exclusivo elegible aleatorio, 1.000 unidades por espacio/visita. Los exclusivos anteriores siguen elegibles. Salir/regresar renueva; reabrir/recargar no. Marisco exclusivo no sale en sendero, pesca, mascotas, NPC como suministro inicial ni premios físicos de cajas. Caja otorga receta, no materia prima. Precios Q5 de tabla ya cotizados: no multiplicar calidad otra vez. Sin descuento de pila de panadería en pescadería.


## 10. Recetas premium opcionales de cajas misteriosas

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



**Revisión premium (5 de noviembre de 2026):** los cuatro asados simples de marisco exclusivo se sustituyen por platos compuestos (vieiras, mejillones, pulpo y ostras de la tabla), así que toda receta de caja cumple la regla premium de la sección 14.4. Dieciséis recetas nuevas = 12 Food + 4 Misc, todas opcionales de caja y sin comida gratis al aprender. Con las 26 originales de la Parte 1 suman 42; con las bebidas en barril de ambas partes (Parte 1 sección 9.4.5 y sección 10.1) la caja combinada tiene **116 recetas** (secciones 10.1-10.3). Conservar seis premios tipados, probabilidad propuesta del 5% de categoría receta por espacio aleatorio, exclusión de conocidas/reservadas y alternativa determinista ante duplicados. Premios físicos Q5; conocimientos sin calidad/espacio de mochila, dominio uno/cero fabricaciones. No garantizan progreso obligatorio ni alteran recetas de misiones. Estas recetas Misc no fabrican muebles.


### 10.1 Bebidas en barril para los rangos 5-8 (exclusivas de la caja misteriosa, propuesta del 5 de noviembre de 2026)

La Parte 1 (sección 9.4.5) tiene nueve bebidas en barril de roble (rango cuatro). La Parte 2 no tenía ninguna. Estas **doce bebidas (tres por rango)** siguen el mismo diseño en dos pasos:
- una **receta de llenado** pone los ingredientes de la bebida en un barril para que envejezcan
- una **receta de embotellado** aparte convierte el barril lleno y las botellas de vidrio en ocho botellas

**Regla de ingredientes (revisada el 6 de noviembre de 2026):** el llenado usa el permiso `BarrelFilling` de Parte 1 sección 9.0.1, no el rol Food. Sugar, Honey y Maple syrup exactos se permiten por la lista de despensa de cada receta; los productos de envejecimiento tienen permiso separado. No cambian recetas ni cantidades; se exigen fuentes y conversiones. Madera, herrajes y combustible no listado conservan la ventana no alimentaria. Ambos pasos son exclusivos de caja, aprendidos por separado; embotellar requiere conocer llenado y sigue siendo Food. El barril lleno es mercancía Misc con envejecimiento, no comida; se consume con Glass bottles para producir ocho botellas, sin devolver barril vacío. La excepción no alcanza conversiones previas ni Misc ordinario.

**Madera y herrajes del barril por rango:** la Parte 1 usa solo roble, la madera real de los barriles. Desde el rango seis el roble queda fuera de la ventana de recetas, así que cada rango usa una madera con tradición real de barricas y sus propios herrajes:

| Tier | Barrel shape (filling) | Real-world basis |
| --- | --- | --- |
| 5 | 4 Oak planks + 2 Zinc fittings + ingredients | Oak casks (tier four oak is inside the {4,5} window) |
| 6 | 4 Cedar planks + 2 Nickel fittings + ingredients | Cedar casks, as used for sake |
| 7 | 4 Redwood planks + 2 Chromium fittings + ingredients | Redwood fermenting tanks |
| 8 | 4 Redwood planks + 2 Titanium fittings + ingredients | Redwood again (ebony is too dense for cooperage) |

El embotellado es siempre **1 barril lleno + 8 Glass bottles -> 8 botellas** (Glass bottle es una excepción de recipiente, Parte 1 sección 9.0.1). Sugar y Honey entran como productos de despensa, y la fruta, el cereal o la especia principal es del bloque del propio rango.

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

**Condición de aparición** (como en la Parte 1): una receta de barril puede salir en una caja solo cuando el jugador conoce la **receta de tablas** de la madera del barril y la de herrajes de ese rango (misiones T5-L4, T6-L4, T7-L4 y T8-L4). También hace falta haber descubierto el origen de cada ingrediente, con sus conversiones conocidas (Maple syrup, Sugar). Una receta de embotellado requiere además su receta de llenado, así que una caja nunca entrega una receta de botella sin su barril. El rango de desbloqueo es el de la bebida.

**Regla premium:** cada bebida cuenta como un par premium. La receta de llenado cumple el requisito premium (madera del barril, herrajes e ingredientes regionales, al menos tres ingredientes distintos), y cada botella da 11-12 corazones, la banda premium del rango cuatro (sección 14.4). El embotellado de dos ingredientes se acepta porque su barril es el ingrediente refinado.

**Precios (propuesta; los barriles se valoran como Misc, igual que en la Parte 1):** las referencias de nivel 10, equivalentes a Q1, continúan el rango del rango cuatro de la Parte 1 (de 5.000 la sidra a 7.200 el whisky) y suben unos 1.000 por rango. Dentro de cada rango, sidras y cervezas son las más baratas, vinos e hidromieles van en medio y los destilados son los más caros. Las botellas mantienen la proporción de la Parte 1, cerca del 14% de la referencia del barril cada una, así que ocho botellas valen más que el barril más las botellas. Se aplica el multiplicador de calidad final (Q5 = 2,00); no hay prima de durabilidad.

| Tier | Barrel references (L10, Q1-equivalent) | Bottle reference (L10) |
| --- | --- | --- |
| 5 | Pear cider 7,500; Plum wine 8,000; Maple rye whiskey 8,600 | 1,050; 1,120; 1,200 |
| 6 | Blackcurrant wine 9,000; Quince thyme mead 9,300; Apricot brandy 9,800 | 1,260; 1,300; 1,370 |
| 7 | Raspberry millet beer 10,200; Pomegranate wine 10,600; Peach brandy 11,200 | 1,430; 1,480; 1,570 |
| 8 | Date palm wine 11,800; Cardamom sorghum spirit 12,400; Spiced mango rum 13,000 | 1,650; 1,740; 1,820 |

Son propuestas para las tablas de precios de la Parte 2 (aún abiertas en la sección 6).

**Aspecto de la caja misteriosa en los rangos 5-8:** la Parte 1 (sección 14) colorea la caja según el rango de la región donde está el comerciante del globo. Siguen cuatro juegos de color más para los rangos 5-8 (tonos de arce, cedro, secuoya y ébano con el carmesí y el latón del comerciante), solo de presentación. El contenido mantiene las reglas de la Parte 1: seis entradas sorteadas de antemano, 70% del rango actual / 30% de rangos anteriores, el 5% de receta por espacio, premios físicos Q5 y nunca marisco de la pescadería como premio físico.

**Efecto en la caja:** la Parte 2 añade **24 recetas** (12 de llenado + 12 de embotellado) a sus 16 recetas de caja, así que tiene 40. Con las 44 de la Parte 1 (26 + 18 de barril), la caja combinada tiene 84 recetas; las recetas del rancho (Parte 1, sección 13.4.2, y sección 10.2) la llevan a **100**. El 5% de probabilidad de receta por espacio y la caja de seis premios no cambian, así que cada receta es más rara.

### 10.2 Productos del rancho y recetas premium del rancho para los rangos 5-8 (propuesta, 5 de noviembre de 2026)

La Parte 1 (sección 13.4.2) define las carnes exclusivas del rancho: una por rango, solo de tienda como el marisco de la pescadería, usadas solo por recetas premium opcionales de caja. Los rangos 5-8 continúan con una carne más por rango, elegida según el paisaje de cada bloque y separada de los animales salvajes de caza (ciervo, cabra montés, búfalo, oso). El cerdo y el pavo real son mascotas, así que no son animales del rancho.

| Exact ingredient (ID) | Material tier | Ranch stock eligibility | Proposed Q5 cash / unit |
| --- | --- | --- | --- |
| Goose meat (`ranch.goose_meat`) | 5 | A tier-five region discovered | 400 |
| Quail meat (`ranch.quail_meat`) | 6 | A tier-six region discovered | 600 |
| Ostrich meat (`ranch.ostrich_meat`) | 7 | A tier-seven region discovered | 900 |
| Guinea fowl meat (`ranch.guinea_fowl_meat`) | 8 | A tier-eight region discovered | 1,200 |

El rancho mantiene las tres ofertas por visita de la Parte 1 (dos de lácteos/huevos y una de carne de cualquier rango elegible). Los precios de la carne quedan por debajo del marisco exclusivo del rango (sección 9). Como el marisco, las carnes nunca salen del sendero, la caza, las mascotas ni los premios físicos de caja.

**Recetas premium del rancho (exclusivas de la caja misteriosa, ocho recetas, dos por rango).** Usan la banda premium del rango cuatro (**11-12 corazones**, sin estadísticas por encima del rango cuatro) y la regla premium de la sección 14.4:

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

La elegibilidad, la etiqueta de origen del recetario y las reglas de reclamación siguen la Parte 1 (sección 13.4.2) y la sección 4 de este documento. **Efecto en la caja:** el grupo de la Parte 2 pasa a 16 + 24 de barril + 8 del rancho = **48**. Con las 52 de la Parte 1, el grupo combinado tiene 100 recetas; los recuerdos de la sección 10.3 lo llevan a 116.

### 10.3 Recuerdos de metales preciosos y gemas para los rangos 5-8 (exclusivos de la caja misteriosa, propuesta, petición del usuario, 5 de noviembre de 2026)

Nueve recetas premium de **Misc** que continúan la Parte 1 (sección 9.5.1), con la misma regla premium (sección 14.4) y las reglas de Misc. La Parte 2 añade gemas pero ningún metal precioso, y el oro y la plata (rangos cuatro y tres) quedan fuera de las ventanas a partir del rango seis. Por eso los recuerdos usan el metal propio de cada rango:
- **Rango 5:** oro, todavía dentro de la ventana {4,5}, y magnetita, la hematita de la bisutería real de cuentas.
- **Rango 6:** una aleación de **plata alemana** (abajo).
- **Rango 7:** herrajes de cromo.
- **Rango 8:** titanio, frecuente en la joyería moderna.

**Las gemas se procesan primero (decisión del usuario, 6 de noviembre de 2026):** cada gema de la Parte 2 tiene una versión tallada o pulida (sección 14.1), y la usan los recuerdos y las prendas del conjunto B con gemas. Así se sigue a la Parte 1, donde el cuarzo, la turquesa, el rubí, el zafiro y el diamante tallados alimentan las mejores recetas. Las gemas en bruto conservan sus usos sencillos: las fichas Misc regionales de la sección 8 y los depósitos de casa de la sección 16.1.

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

**Nuevo Material:** `recipe.material.nickel_silver_ingot`, rango seis: **2 Nickel ingots + 1 Zinc ingot -> 2 Nickel silver ingots**. Es una aleación real de color plateado y no necesita metal precioso. Temporizador fijo de material y sin dominio. Como toda receta, es recompensa de misión (T6-L10, sección 15.2).

La **elegibilidad** sigue la Parte 1 (sección 9.5.1). **Efecto en la caja:** el grupo de la Parte 2 pasa a **57** (16 + 24 de barril + 8 del rancho + 9 recuerdos); con las 59 de la Parte 1 el grupo combinado tiene **116 recetas**, o 131 recetas opcionales con las 15 de panadería.

## 11. Nuevas recetas premium de panadería

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



Ocho nuevas recetas, dos por rango 5–8. Total panadería: **15** con distribución 1/2/2/2/2/2/2/2. Mantener 1% de sustitución por espacio premium elegible: una receta de esa misma comida reemplaza la pila, sin 1.000 pergaminos. Precio de receta 10× porción sin descuento; pila completa de comida 90% de 1.000 porciones. Dos espacios premium y uno común, persistentes hasta salida/regreso. Comprar comida no aprende receta. Doce corazones es recuperación total progresiva, no límite nuevo; las variantes dan variedad de ingredientes y oro, sin exigir subir recuperación/capacidad.


## 12. Filtros exactos, dependencias y aceptación

**Revisión de recetas para ingredientes de árbol:** cada ingrediente futuro ya tiene receta de entrada exacta y cada refinado llega a comida terminada. Conservar definiciones sin duplicar desbloqueos.

| ID canónico recolectado | Receta exacta / cadena útil | Otros usos existentes |
| --- | --- | --- |
| `resource.maple_sap` | `recipe.material.maple_syrup`: 3 Maple sap + 1 Maple branch -> 1 Maple syrup; luego `recipe.food.chicory_maple_drink`: 2 Chicory root + 1 Maple syrup + 1 Cow milk -> 1 | `recipe.food.maple_pear_tart`; `recipe.food.venison_pear_pot` usan jarabe |
| `resource.apricots` | `recipe.food.apricot_buckwheat_pot`: 2 Apricots + 2 Buckwheat + 1 Cow milk -> 1 | `recipe.food.apricot_hazelnut_cake`; `recipe.food.goat_apricot_pot` |
| `resource.peaches` | `recipe.food.peach_millet_pot`: 2 Peaches + 2 Millet + 1 Cow milk -> 1 | `recipe.food.peach_pecan_galette` |
| `resource.mangoes` | `recipe.food.mango_sorghum_pot`: 2 Mangoes + 2 Sorghum + 1 Cow milk -> 1 | `recipe.food.mango_passionfruit_tart` |

Árbol y recolección suelta usan mismos IDs. Cada receta acepta ambas fuentes y mezclas válidas de pilas del mismo ID; no crear “albaricoque de árbol” ni receta por procedencia. Ingredientes crudos recolectados están listos para fabricar, sin ser comida preparada automática. Productos pendientes no están disponibles hasta terminar. Aplicar cantidades, reservas, perfiles/calidad y reglas de calidad de salida normales. Convertir savia a `item.maple_syrup` cuando se exige jarabe; savia de abedul tiene otro ID y no sustituye. Elegibilidad usa OR entre fuentes autorizadas por ingrediente y AND entre ingredientes distintos.

**Comprobaciones de aceptación:** fabricar cada fila con origen árbol, suelto y mezcla válida; mismo perfil de fabricación conserva rendimiento/producto. Para cuatro ingredientes exclusivos de árbol de Parte 1, usar TreeHarvest y conversión exacta. Rechazar savia/fruta/harina incorrecta, cantidades insuficientes, reservas o refinados pendientes sin gastar. Comprobar cadenas crudo/refinado/Food, costes x1/x100/x1000, combustible y consumo/recarga una vez. Ingrediente Q5 no fuerza producto Q5. Validar que todo ingrediente configurado de árbol tenga receta de entrada y uso Food final accesible; aplicar también al añadir segundo ingrediente. Revisión sin IDs nuevos ni cambios de cantidades de panadería/cajas.

Para cada receta, guardar `RequiredRegionIds`, IDs exactos de fuentes comerciales/interacciones y conversiones. Las fuentes se derivan del catálogo regional y recursivamente de cada refinado: por ejemplo Maple syrup exige su conversión y acceso a Maple sap y Maple branches. Savia viene de árbol descubierto en 25–30 o suelta en 30; ramas del árbol o sueltas en 25. Basta una fuente válida por ingrediente. No basta entrar en rango 5. Ingredientes de Parte 1 conservan fuentes exactas: Wheat 6, Honey 11, Sugar cane 17 y lácteos accesibles. No exigir poseer stock al sortear; sí acceso a cada fuente/conversión. El acceso a pescado es unión de parejas de regiones descubiertas, no todos los peces del rango.

Ejemplos obligatorios: bloquear tarta de arce/pera sin Pears 25, fuente válida de savia (árbol 25–30 o suelta 30), conversión y Wheat 6/lácteos; brioche de ciruela sin 27 o 28; pastel de albaricoque sin Hazelnuts 33 y Apricots de árbol 31–36 o sueltos 31; pastel de grosella sin 33 o 36; galette de melocotón sin Pecans 39 y Peaches de árbol 37–42 o sueltos 37; pastel de frambuesa sin 39 o 42; tarta de mango sin Passion fruit 45 y Mangoes de árbol 43–48 o sueltos 43; pastel de fruta del dragón sin 46 o 47. Para Scallop/Mussel/Octopus/Oyster exigir 25/31/37/43 y pescadería accesible; cada plato además requiere fuentes de cereal/hierba/lácteos indicadas. Trofeos requieren caza exacta descubierta, herramienta elegible y refinados alcanzables.

Comprobar fuentes al generar y antes de cobrar/desbloquear/reclamar. Regalos, dominio, calidad, oro, refinados comprados, vista previa o comida preparada no otorgan descubrimiento. Persistir IDs, versión de catálogo, ofertas/cotizaciones, visitas, reservas, manifiestos pagados y conocimiento. Rechazar desbloqueo inválido sin cobrar/consumir premio; conservar manifiesto hasta poder reclamar válidamente, sin sortear de nuevo. Activar por tier y versión explícitos; contenido desactivado no entra en catálogos actuales.

Los hitos P2.1–P2.4 incorporan estas tablas de forma incremental: recursos/procesamiento y fuentes primero; luego ruta/herramientas/caza/pesca; después misiones y tiendas/cajas/panadería; finalmente migración/regresiones. Herramientas y tareas exactas siguen pendientes; ninguna tabla implica implementación terminada. Antes de activar: comprobar 24 regiones, 72 recursos distintos, 18 por rango, parejas sin repetir y 16 peces; 16 recetas de caja y ocho de panadería nuevos, IDs únicos y filtros de cada ingrediente con todas sus fuentes alternativas inaccesibles y cada alternativa accesible por separado. Comprobar precios ×10/−10%, dominio uno, calidad artesanal independiente de ingrediente Q5, lotes/combustible exactos y no duplicación tras recarga. El catálogo combinado de estos dos orígenes es **131 recetas opcionales** (116 de caja, incluidas las 21 de llenado y 21 de embotellado de barril, las 16 del rancho y los 16 recuerdos de metales preciosos y gemas de ambas partes, más 15 de panadería); receta de misión de trufa y recetas regionales normales son adicionales y no se cuentan aquí.

## 13. Equipo y herramientas de los rangos 5-8 (propuesta, 5 de noviembre de 2026)

**Sin estadísticas nuevas (usuario, 5 de noviembre de 2026):** el rango cuatro es el límite. Los rangos cinco a ocho **no** añaden valores de capacidad: los sombreros dan como máximo 12 corazones, las botas 4,5 m/s, las camisas 10 bolsillos, los pantalones 10 ganchos y las mochilas 14 x 4. El presupuesto de 26 corazones por tramo y el medidor de 12 corazones tampoco cambian. Lo que crece con cada rango:
- **la durabilidad**
- **el alcance de las herramientas**: una herramienta funciona hasta dos rangos por encima del suyo (Parte 1, sección 8.2), así que los recursos posteriores necesitan herramientas posteriores
- **el valor de las recetas**
- **la variedad**

**Dos conjuntos de equipo por rango, como en la Parte 1 (sección 9.6.1):**
- El **Conjunto A** se fabrica con las tres primeras regiones del bloque (25-27, 31-33, 37-39, 43-45).
- El **Conjunto B** necesita al menos un material de las tres últimas regiones.
- Los dos tienen las capacidades del rango cuatro (valores del Conjunto B: 12 corazones, 4,5 m/s, 10 bolsillos, 10 ganchos, 14 x 4). Se diferencian en sus fuentes, su arte y la **durabilidad: el Conjunto B tiene un 25% más de durabilidad base que el Conjunto A**.
- Objetivos de durabilidad propuestos con dominio 10 y Q5, continuando los unos 14 km del Conjunto B del rango cuatro de la Parte 1:

| Tier | Set A (km) | Set B (km) |
| --- | --- | --- |
| 5 | 14.4 | 18 |
| 6 | 16.8 | 21 |
| 7 | 19.2 | 24 |
| 8 | 21.6 | 27 |

El **equipo especial del globo** mantiene su catálogo de rangos 1-4. En los rangos cinco a ocho su sorteo de rango trata el rango cuatro como el más alto, así que no hacen falta objetos especiales ni estadísticas nuevas.

### 13.1 Recetas de los conjuntos

Los IDs son `recipe.wearable.<id>` -> `item.<id>`. Las ramas siguen la Parte 1: sombreros para Recolector, botas para Leñador, camisas para Cazador, pantalones y mochilas para Explorador. Todos los ingredientes quedan dentro de la ventana de rango ({4,5}, {5,6}, {6,7}, {7,8}).

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

Los materiales del Conjunto B que marcan cada rango:
- **Rango 5:** Fine ramie cloth (Willow cord, región 30), Fine deer leather (Safflower oil, 28), Polished agate y Magnetite (29).
- **Rango 6:** seda (34), cachemira (35), Cut almandine (36).
- **Rango 7:** Fine buffalo leather (Sesame oil, 40), Fine sisal cloth (Indigo dye, 41), Cut tourmaline (41) y Polished malachite (40).
- **Rango 8:** Fine bear felt (Black sesame oil, 47), Polished moonstone (46), Cut aquamarine (47), Polished opal (48).

### 13.2 Herramientas

Se aplican las reglas de la Parte 1: herramientas Normales en todos los rangos; una Élite consume una Normal del mismo rango y 2 unidades avanzadas del rango anterior; solo x1; la tabla de eficacia (Parte 1, sección 8.2) no cambia. Desde el rango tres la herramienta de caza es un arco. Las durabilidades base siguen las fórmulas de la Parte 1: hacha y caña Normales `10 + 10 * (T - 1)`, arco Normal `6 + 10 * (T - 1)`, Élite el doble de la Normal.

| Tier | Normal axe | Normal bow | Normal rod | Elite inputs (axe / bow / rod) | Normal / Elite uses (axe, bow, rod) |
| --- | --- | --- | --- | --- | --- |
| 5 Maple | 2 Maple branches + 1 Zinc ingot + 1 Ramie cord | 2 Maple branches + 1 Deer hide + 1 Ramie cord | 2 Maple branches + 1 Zinc fittings + 1 Ramie cord | 2 Oak planks / 2 Fine boar leather / 2 Bamboo cloth | 50, 46, 50 / 100, 92, 100 |
| 6 Cedar | 2 Cedar branches + 1 Nickel ingot + 1 Jute cord | 2 Cedar branches + 1 Mountain goat hide + 1 Jute cord | 2 Cedar branches + 1 Nickel fittings + 1 Jute cord | 2 Maple planks / 2 Fine deer leather / 2 Fine ramie cloth | 60, 56, 60 / 120, 112, 120 |
| 7 Redwood | 2 Redwood branches + 1 Chromium ingot + 1 Sisal cord | 2 Redwood branches + 1 Buffalo hide + 1 Sisal cord | 2 Redwood branches + 1 Chromium fittings + 1 Sisal cord | 2 Cedar planks / 2 Fine goat leather / 2 Fine jute cloth | 70, 66, 70 / 140, 132, 140 |
| 8 Ebony | 2 Ebony branches + 1 Titanium ingot + 1 Lotus cord | 2 Ebony branches + 1 Bear fur + 1 Lotus cord | 2 Ebony branches + 1 Titanium fittings + 1 Lotus cord | 2 Redwood planks / 2 Fine buffalo leather / 2 Fine sisal cloth | 80, 76, 80 / 160, 152, 160 |

El rango cuatro ya desbloquea los ingredientes de las Élite del rango cinco (Oak plank T4-L1, Fine boar leather T4-H5, Bamboo cloth T4-E11), así que no hacen falta misiones puente adicionales en el rango cuatro. Cada bloque posterior desbloquea los ingredientes de las Élite del bloque siguiente (sección 15).

### 13.3 Temporizadores (definidos, no extrapolados)

Duraciones base propuestas de nivel uno y x1, manteniendo la regla de la Parte 1 de que un temporizador de Tool o Food queda por debajo de una hora (en Food, x1000 con nivel 1 queda por debajo de 54 minutos):

| Tier | Tool | Food | Wearable | Misc |
| --- | --- | --- | --- | --- |
| 5 | 8 min | 12 min | 3 h | 1.5 h |
| 6 | 12 min | 14 min | 4 h | 2 h |
| 7 | 16 min | 16 min | 5 h | 2.5 h |
| 8 | 20 min | 18 min | 6 h | 3 h |

Las recetas de Material mantienen temporizadores fijos definidos; los productos que se pueden recolectar siguen siendo instantáneos.

## 14. Recetas añadidas para los rangos 5-8 (propuesta, 5 de noviembre de 2026)

Se aplica la misma regla de ingredientes que en la Parte 1 (sección 9.6): de uno a tres ingredientes reales significativos que existan en el catálogo (cuatro solo en mochilas y en la bolsa de especias existente), IDs exactos y la ventana de rango. **Toda receta de esta sección y de la sección 8 es recompensa de misión** (sección 15); ninguna se desbloquea al descubrir.

### 14.1 Materiales

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

**Procesado de gemas (decisión del usuario, 6 de noviembre de 2026):** funciona como el cuarzo y la turquesa tallados de la Parte 1 (2 en bruto -> 1, campamento/casa, temporizador fijo de material). Las gemas facetadas (celestina, almandino, turmalina, aguamarina) se **tallan**. Las piedras opacas o blandas que se trabajan en cabujón o cuentas (ámbar, ágata, malaquita, piedra de luna, ópalo) se **pulen**. Cada una tiene su propio ID de material dentro de su rango, y cada una es recompensa de misión (sección 15).

**Corrección:** las tablas usan **3 troncos -> 1**, como en la Parte 1 (sección 9.2); la sección 8 decía 2.

### 14.2 Comida: platos de pescado, platos de carne y corazones

**Pescado (usuario, 5 de noviembre de 2026):** cada especie es una **misión de recolección** que enseña un **plato variado**. Los rangos cinco a ocho no tienen pescados simplemente asados; la columna de asados de la sección 9 se sustituye por estos platos.

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

**Carne:** cada rango conserva su asado como primera comida de caza (sección 8) y añade un guiso:
- Venison rye stew: 2 Raw deer meat + 2 Rye + 1 Rock salt -> 2
- Mountain goat thyme stew: 2 Raw mountain goat meat + 2 Buckwheat + 1 Mountain thyme -> 2
- Buffalo millet stew: 2 Raw buffalo meat + 2 Millet + 1 Rock salt -> 2
- Bear sorghum stew: 2 Raw bear meat + 2 Sorghum + 1 Rock salt -> 2

**Corazones (sin estadísticas aumentadas):** la comida de misión de los rangos cinco a ocho sigue el rango ordinario de misión y llega como máximo a **6 corazones por porción**, el nivel del rango cuatro:
- las ollas regionales, la Chicory maple drink y los cuatro asados de la sección 8 pasan a **6** (la bebida a **5**)
- los platos de pescado y los guisos de arriba dan 6, o 5 cuando la receta rinde 2
- las recetas premium de caja usan la banda del rango cuatro de la Parte 1, **11-12** (Parte 1, sección 9.4.2: los corazones premium suben con el rango hasta 11-12 en el rango cuatro, y las estadísticas dejan de subir ahí); las de panadería mantienen 12

Las comidas del rango cuatro ya cubren el presupuesto de 26 corazones por tramo, y ese presupuesto no crece.

### 14.3 Misc

Las catorce recetas Misc regionales de la sección 8 se mantienen, ahora como recompensas de misión. Cada rango añade una talla de trofeo (Misc, 2 tablas + 1 trofeo). La caja conserva los tokens de trofeo más ricos, de tres ingredientes.
- Maple deer sculpture: 2 Maple planks + 1 Deer antler
- Cedar goat carving: 2 Cedar planks + 1 Mountain goat horn
- Redwood buffalo totem: 2 Redwood planks + 1 Buffalo horn
- Ebony bear carving: 2 Ebony planks + 1 Bear claw

### 14.4 Regla de tiendas y caja misteriosa

Como en la Parte 1 (sección 9.6.6), una receta solo puede ser un desbloqueo de tienda o de caja si es **premium**:
- **Food:** 11-12 corazones (la banda premium del rango cuatro), con al menos tres ingredientes distintos, uno refinado o de otro origen.
- **Misc:** al menos tres ingredientes, uno de ellos un material refinado.

Las ocho recetas de panadería, los tokens de trofeo y las bebidas en barril (sección 10.1) ya cumplen la regla. Los cuatro asados simples de marisco exclusivo de la sección 10 no la cumplían, así que se **rehacen** allí como platos premium compuestos.

## 15. Plan de misiones para los rangos 5-8 (propuesta, 5 de noviembre de 2026)

Se aplican las convenciones de la Parte 1 (sección 11.6):
- una receta por misión
- las misiones se abren por **región de misión** (la columna Region)
- una misión queda bloqueada hasta que su recompensa se pueda fabricar
- la herramienta Normal antes que su Élite, y el refinado del rango anterior antes de una Élite
- el Conjunto A antes que el Conjunto B

**Las cantidades nunca superan el límite de campaña:** **150** unidades en pedidos apilables y **40** instancias en herramientas y prendas en todos los rangos del cinco al ocho (los valores del rango cuatro). Las recompensas en **negrita** son los conjuntos de equipo.

**Campamentos por región (confirmado, 5 de noviembre de 2026):** 16-20 por región como en el rango cuatro (sección 7), 104-110 por rango, así que hay unas 0,4 misiones por campamento.

**Corrección de orden, 6 de noviembre de 2026:** los códigos son IDs estables; definir orden por rama siguiendo las filas, sin ordenar por número de código. Lumberjack del rango seis: **T6-L1 -> T6-L2 -> T6-L3 -> T6-L4 -> T6-L10 -> T6-L5 -> T6-L6 -> T6-L7 -> T6-L8 -> T6-L9**. Nickel silver se desbloquea en región de misión 32 tras Nickel ingot y Zinc ingot anterior, sin esperar región 36. Los códigos nuevos de gemas también ejecutan en su posición mostrada. Aplicar la migración versionada de Parte 1 sección 11.6.1 si existen IDs en guardados, conservando recompensas y activa. Revisar dependencias de pedidos y recompensas, no solo regiones/códigos.

### 15.1 Rango cinco (regiones 25-30)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T5-G1 | 25 | Maple branches | Maple log | |
| T5-G2 | 26 | Rye | Pear rye porridge | |
| T5-G3 | 26 | Amber | Amber gneiss token | |
| T5-G12 | 26 | Amber | Polished amber | |
| T5-G4 | 27 | Plums | **Deer-hide hat (5A hat)** | Deer leather (T5-H2), Ramie cloth (T5-E3) |
| T5-G5 | 28 | Chestnuts | Plum chestnut pot | |
| T5-G6 | 28 | Calendula | Calendula chestnut keepsake | Ramie cloth (T5-E3) |
| T5-G7 | 28 | Safflower seeds | Safflower oil | |
| T5-G8 | 29 | Agate | Agate gypsum medallion | |
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
| T5-H1 | 25 | Raw deer meat | Roasted deer | Hunted with the oak bow (one tier above) |
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

### 15.2 Rango seis (regiones 31-36)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T6-G1 | 31 | Cedar branches | Cedar log | |
| T6-G2 | 32 | Buckwheat | Apricot buckwheat pot | Apricots from cedar trees or loose in 31 |
| T6-G3 | 32 | Apricot buckwheat pot | **Goat-hide cap (6A hat)** | Mountain goat leather (T6-H2), Jute cloth (T6-E2) |
| T6-G4 | 33 | Blackcurrants | Blackcurrant hazelnut pot | |
| T6-G5 | 33 | Kaolin | Kaolin hazelnut charm | Jute cloth (T6-E2) |
| T6-G6 | 34 | Celestite | Mulberry celestite pouch | Silk cloth (T6-E12) |
| T6-G9 | 34 | Celestite | Cut celestite | |
| T6-G7 | 36 | Quinces | Quince thyme compote | |
| T6-G8 | 36 | Quince thyme compote | **Cashmere hood (6B hat)** | Cashmere cloth (T6-E14), Silk cloth (T6-E12) |
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
| T6-L9 | 36 | Almandine | **Escarpment boots (6B boots)** | Fine goat leather (T6-H6), Cashmere cloth (T6-E14) |
| T6-H1 | 31 | Raw mountain goat meat | Roasted mountain goat | |
| T6-H2 | 31 | Mountain goat hide | Mountain goat leather | |
| T6-H3 | 32 | Mountain goat hide | Normal cedar bow | Jute cord (T6-E3) |
| T6-H4 | 32 | Mountain goat leather | **Jute shirt (6A shirt)** | Jute cloth (T6-E2) |
| T6-H5 | 33 | 40 Normal cedar bows | Elite cedar bow | Fine deer leather (T5-H7) |
| T6-H6 | 33 | Mountain goat leather | Fine goat leather | Safflower oil (T5-G7) |
| T6-H7 | 33 | Mountain goat horns | Cedar goat carving | |
| T6-H8 | 35 | Mountain thyme | Mountain goat thyme stew | |
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

### 15.3 Rango siete (regiones 37-42)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T7-G1 | 37 | Redwood branches | Redwood log | |
| T7-G2 | 38 | Millet | Peach millet pot | Peaches from redwood trees or loose in 37 |
| T7-G3 | 38 | Peach millet pot | **Buffalo-hide hat (7A hat)** | Buffalo leather (T7-H2), Sisal cloth (T7-E1) |
| T7-G4 | 39 | Raspberries | Raspberry pecan pot | |
| T7-G5 | 39 | Travertine | Pecan travertine keepsake | Sisal cloth (T7-E1) |
| T7-G6 | 40 | Tarragon | Malachite tarragon pouch | |
| T7-G12 | 40 | Malachite | Polished malachite | |
| T7-G7 | 40 | Sesame seeds | Sesame oil | |
| T7-G8 | 41 | Tourmaline | Tourmaline indigo token | Indigo dye (T7-E10) |
| T7-G13 | 41 | Tourmaline | Cut tourmaline | |
| T7-G9 | 42 | Amaranth seeds | Pomegranate amaranth pot | |
| T7-G10 | 42 | Red clay | Saffron clay keepsake | |
| T7-G11 | 42 | Pomegranate amaranth pot | **Painted hat (7B hat)** | Fine sisal cloth (T7-E11), Fine buffalo leather (T7-H8), Cut tourmaline (T7-G13) |
| T7-L1 | 37 | Redwood logs | Redwood plank | |
| T7-L2 | 38 | Chromium ore | Chromium ingot | |
| T7-L3 | 38 | Chromium ingots | Normal redwood axe | Sisal cord (T7-E2) |
| T7-L4 | 38 | Chromium ore | Chromium fittings | |
| T7-L5 | 38 | Redwood planks | Redwood beam | Sisal cord (T7-E2) |
| T7-L6 | 38 | Redwood beams | **Bluff boots (7A boots)** | Buffalo leather (T7-H2) |
| T7-L7 | 39 | 40 Normal redwood axes | Elite redwood axe | Cedar plank (T6-L1) |
| T7-L8 | 41 | Chromium fittings | **Gorge boots (7B boots)** | Fine buffalo leather (T7-H8) |
| T7-H1 | 37 | Raw buffalo meat | Roasted buffalo | |
| T7-H2 | 37 | Buffalo hide | Buffalo leather | |
| T7-H3 | 38 | Buffalo hide | Normal redwood bow | Sisal cord (T7-E2) |
| T7-H4 | 38 | Buffalo leather | **Sisal shirt (7A shirt)** | Sisal cloth (T7-E1) |
| T7-H5 | 38 | Millet | Buffalo millet stew | |
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

### 15.4 Rango ocho (regiones 43-48)

| Code | Region | Request | Reward recipe | Needs |
| --- | --- | --- | --- | --- |
| T8-G1 | 43 | Ebony branches | Ebony log | |
| T8-G2 | 44 | Sorghum | Mango sorghum pot | Mangoes from ebony trees or loose in 43 |
| T8-G3 | 44 | Mango sorghum pot | **Bear-fur hat (8A hat)** | Bear felt (T8-H2), Lotus cloth (T8-E2) |
| T8-G4 | 45 | Pistachios | Passion fruit pistachio pot | |
| T8-G5 | 45 | Serpentine | Pistachio serpentine token | Lotus cloth (T8-E2) |
| T8-G6 | 46 | Cardamom | Moonstone spice pouch | |
| T8-G11 | 46 | Moonstone | Polished moonstone | |
| T8-G7 | 47 | Black sesame seeds | Black sesame oil | |
| T8-G8 | 47 | Aquamarine | Aquamarine lotus charm | |
| T8-G12 | 47 | Aquamarine | Cut aquamarine | |
| T8-G9 | 48 | Teff | Date teff pot | |
| T8-G13 | 48 | Opal | Polished opal | |
| T8-G10 | 48 | Date teff pot | **Moonstone hat (8B hat)** | Fine lotus cloth (T8-E9), Fine bear felt (T8-H8), Polished moonstone (T8-G11) |
| T8-L1 | 43 | Ebony logs | Ebony plank | |
| T8-L2 | 44 | Titanium ore | Titanium ingot | |
| T8-L3 | 44 | Titanium ingots | Normal ebony axe | Lotus cord (T8-E3) |
| T8-L4 | 44 | Titanium ore | Titanium fittings | |
| T8-L5 | 44 | Ebony planks | Ebony beam | Lotus cord (T8-E3) |
| T8-L6 | 44 | Ebony beams | **Nightbloom boots (8A boots)** | Bear felt (T8-H2) |
| T8-L7 | 45 | 40 Normal ebony axes | Elite ebony axe | Redwood plank (T7-L1) |
| T8-L8 | 48 | Opal | Opal keepsake | |
| T8-L9 | 48 | Titanium fittings | **Starlit boots (8B boots)** | Fine bear felt (T8-H8) |
| T8-H1 | 43 | Raw bear meat | Roasted bear | |
| T8-H2 | 43 | Bear fur | Bear felt | |
| T8-H3 | 44 | Bear fur | Normal ebony bow | Lotus cord (T8-E3) |
| T8-H4 | 44 | Bear felt | **Lotus shirt (8A shirt)** | Lotus cloth (T8-E2) |
| T8-H5 | 44 | Sorghum | Bear sorghum stew | |
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

### 15.5 Resumen y comprobaciones

- **Total:** 46 / 46 / 44 / 44 misiones en los rangos cinco / seis / siete / ocho, **180** en total, incluidas las nueve misiones de procesado de gemas añadidas el 6 de noviembre de 2026 (los códigos nuevos se añaden al final; los existentes no cambian). Entre las dos partes hay 415 misiones: rango uno 30 (22 + 8 puente), rangos 2-4 205, rangos 5-8 180.
- **Cada rango recibe:**
  - los dos conjuntos de equipo (10 prendas) y las seis herramientas
  - sus refinados, incluidos los ingredientes de las Élite del bloque siguiente
  - una misión de recolección de pescado por especie (16 en total)
  - el asado y un guiso
  - todas las ollas, bebidas y recetas Misc regionales de la sección 8
- **Comprobaciones:**
  - cada fuente se alcanza no más tarde que su región de misión
  - cada recompensa se puede fabricar cuando se ofrece
  - la herramienta Normal antes que su Élite, y el refinado del rango anterior antes de una Élite
  - el Conjunto A antes que el Conjunto B
  - ninguna recompensa repetida
  - ninguna receta sin misión, salvo las premium de caja o panadería
  - ninguna capacidad por encima de los límites del rango cuatro
  - cantidades de 150 / 40 como máximo
  - comida de misión de 6 corazones como máximo
- **Abierto:** precios de los nuevos materiales y prendas; el arte de las 40 prendas con el sistema de piezas ajustadas de la Parte 1.

## 16. Mejoras de casa y asentamiento para los rangos 5-8 (propuesta, petición del usuario, 5 de noviembre de 2026)

El asentamiento sigue donde está, entre las regiones 4 y 5, con viaje rápido (Parte 1, sección 12.3). La sección 13 de la Parte 1 sigue vigente: las casas suben de nivel solo con depósitos de materiales, los muebles se compran y nunca se fabrican, y las tiendas se renuevan al salir y volver. Los rangos 5-8 continúan el mismo patrón de **dos niveles de casa por rango de recurso**. El nivel inferior de cada par usa los materiales de las regiones tempranas del bloque, y el superior los de las regiones tardías, como los dos conjuntos de equipo.

**Sin estadísticas nuevas:** igual que el equipo (sección 13), los niveles nuevos añaden **espacio, aspecto y organización del almacenamiento**, no bonificaciones más fuertes. Las bonificaciones de los muebles y los tamaños de cofre mantienen los límites de la Parte 1. La excepción, a petición del usuario, son las mascotas: la casita de mascotas admite hasta ocho mascotas activas y las mascotas exóticas recolectan más (sección 16.5).

### 16.1 Niveles de casa 9-16

La casa mantiene los 12 de ancho fijos desde el nivel 3 y sigue creciendo hacia arriba (dos filas por nivel), con interior desplazable. Los costes son depósitos, no recetas, así que las piedras y gemas sueltas en bruto sirven como destino. Eso da a las piedras del bloque (Gneiss, Gypsum, Kaolin, Pumice, Travertine, Red clay, Serpentine) un segundo uso además de su token Misc. **Crecimiento de los depósitos (usuario, 5 de noviembre de 2026):** cada tipo de material pide **25 x el rango de recurso**, como en la Parte 1 (sección 13.1, rango cuatro 100), así que los rangos 5-8 piden **125 / 150 / 175 / 200** por tipo.

| Level | Visual stage | Interior grid | Upgrade deposit to reach it (25 x tier per type) | Needs regions |
| --- | --- | --- | --- | --- |
| 9 | Maple lodge (stone base, maple timber upper floor) | 12 x 19 | 125 Maple beams + 125 Gneiss + 125 Ramie cloth + 125 Zinc fittings | 25-27 |
| 10 | Maple lodge with veranda and orchard porch | 12 x 21 | 125 Maple planks + 125 Gypsum + 125 Fine ramie cloth + 125 Magnetite fittings | 28-30 |
| 11 | Cedar chalet (steep roof, balconies) | 12 x 23 | 150 Cedar beams + 150 Kaolin + 150 Jute cloth + 150 Nickel fittings | 31-33 |
| 12 | Cedar chalet with lookout tower | 12 x 25 | 150 Cedar planks + 150 Pumice + 150 Silk cloth + 150 Almandine | 34-36 |
| 13 | Redwood hall (tall timber frame) | 12 x 27 | 175 Redwood beams + 175 Travertine + 175 Sisal cloth + 175 Chromium fittings | 37-39 |
| 14 | Redwood hall with painted gallery | 12 x 29 | 175 Redwood planks + 175 Red clay + 175 Fine sisal cloth + 175 Tourmaline | 40-42 |
| 15 | Ebony estate (dark timber, stone terraces) | 12 x 31 | 200 Ebony beams + 200 Serpentine + 200 Lotus cloth + 200 Titanium fittings | 43-45 |
| 16 | Grand homestead (final) | 12 x 33 | 200 Ebony planks + 200 Fine lotus cloth + 200 Titanium fittings + 200 Opal | 46-48 |

Cada material del depósito es una recompensa de misión de la sección 15 o un recurso suelto de su bloque, así que cada nivel se puede alcanzar cuando se alcanzan sus regiones. El menú de mejora, el depósito en garantía, la cancelación a través de la bandeja y la finalización atómica siguen como en la Parte 1 (sección 13.1). **Arte:** los 16 exteriores y sus interiores forman un solo conjunto, como la Parte 1 ya prevé para los niveles 1-8, así que los ocho exteriores nuevos se diseñan junto con los ocho primeros.

### 16.2 Muebles de los niveles 9-16

Los 48 tipos de mueble con bonificación de la Parte 1 (seis por nivel; cada una de las doce categorías se compra cuatro veces para su bonificación completa) siguen siendo el **conjunto completo de bonificaciones**. Los niveles 9-16 no añaden tipos con bonificación ni copias más fuertes. En su lugar, cada nivel desbloquea **seis tipos decorativos con la madera y el paisaje de su tema**:
- **Cama, mesa, lámpara y alfombra** (un juego por madera: arce, cedro, secuoya, ébano).
- **Una vitrina de trofeo** para el trofeo de su rango (soporte de astas, colgador de cuernos, placa de cráneo de búfalo, vitrina de garra). Es solo decorativa; los objetos de trofeo siguen siendo Misc de venta.
- **Un aspecto temático de cofre** con un tamaño existente de la Parte 1 (ningún cofre mayor de 48 x 6).

El interior más grande permite colocar más cofres y vitrinas, no cofres más grandes. Los tipos decorativos se compran una vez por objeto, como los demás muebles, y no dan nada al comprarlos.

### 16.3 Tiendas del asentamiento

- **Aspecto por rango:** las tiendas de la Parte 1 cambian de aspecto en las regiones 7, 13 y 19 (pino, abedul, mezquite, roble). Les siguen cuatro aspectos más en el primer campamento de las regiones **25, 31, 37 y 43** (maderas y adornos de arce, cedro, secuoya y ébano, con letreros más ricos y más mercancía a la vista). Cada mostrador cambia con su fachada y el tendero sigue siendo la misma persona. Cada cambio ocurre una vez y se guarda, como en la Parte 1.
- **Existencias:** los filtros de existencias de cada tienda se amplían a los objetos descubiertos de los rangos 5-8. La regla de tres ofertas, Q5, las pilas completas y la renovación al salir y volver no cambian.
  - Mina: los minerales y piedras del bloque (Zinc, Magnetite, Nickel, Chromium y Titanium ore; Gneiss, Gypsum, Kaolin, Pumice, Travertine, Red clay, Serpentine).
  - Granja: los nuevos productos, hierbas y semillas.
  - Tienda de materiales de sastre: Ramie, Jute, Sisal y Lotus fiber, Willow bast y los cueros del bloque. Silk cocoons y Goat down siguen siendo solo de recolección.
  - Pescadería: los peces de los rangos 5-8 de las parejas descubiertas y los exclusivos de la sección 9.
  - Rancho: dos ofertas de lácteos/huevos y una de carne exclusiva del rancho (sección 10.2); las carnes de caza siguen siendo solo de caza.
  - Panadería: sección 11.
  - Las gemas siguen siendo sueltas o solo de caja (no hay tienda de gemas).
- **Almacén:** sin cambios (dos cajas de diez unidades, 1 de oro por cada 1.000 de efectivo). Los bienes de los rangos 5-8 se depositan por su propio valor.

### 16.4 Campo maderero: cuatro secciones más

El campo pasa de cuatro a **ocho secciones de madera**, cada una con dos filas de seis árboles masivos (48 árboles), con las mismas reglas que la Parte 1 (sección 13.4):
- reaparición independiente de 72 horas por árbol
- recompensas x50, incluida la comida del árbol (Maple sap, Apricots, Peaches, Mangoes)
- la pantalla de corte enfocada, que se puede volver a abrir en un árbol talado para recoger los restos (Parte 1, sección 8.2.1)

Cada sección nueva se desbloquea con oro y requiere haber descubierto su rango de madera. Precios propuestos, continuando pino 50 / abedul 300 / mezquite 750 / roble 1.500:

| Section | Gold unlock | Requires |
| --- | --- | --- |
| Maple | 2,200 | Region 25 |
| Cedar | 3,000 | Region 31 |
| Redwood | 3,900 | Region 37 |
| Ebony | 5,000 | Region 43 |

El alcance de las herramientas sigue vigente: un árbol masivo necesita un hacha de como mucho dos rangos por debajo (un ébano necesita un hacha de secuoya, cedro o ébano). La zona del campo se desplaza de lado a partir de la cuarta sección. El arte de los árboles masivos de las cuatro maderas nuevas sigue la recomendación de la Parte 1 (raíces de contrafuerte, corteza profunda, dos o tres formas de tronco por especie, un tocón cortado).

### 16.5 Casas vecinas, casita de mascotas y mascotas exóticas

- **Vecinos:** las cinco casas vecinas mantienen la regla de 1-3 regiones descubiertas y ahora pueden llegar al **nivel 16**, con los mismos 16 exteriores que la casa del jugador. Con 24 regiones más, la calle muestra poco a poco los nuevos aspectos.

**Niveles 5-8 de la casita de mascotas (propuesta, petición del usuario, 5 de noviembre de 2026).** La Parte 1 (sección 13.5) añade los niveles 1-4 de la casita (de una a cuatro mascotas activas). Los rangos 5-8 los continúan; **cada nivel admite una mascota activa más, hasta ocho**. El depósito es la mitad del de la casa del mismo rango (redondeado a 5), con tablas, tela y herrajes, como en la Parte 1:

| Pet home level | Active pets | Requires | Deposit |
| --- | --- | --- | --- |
| 5 | 5 | House level 10 | 65 Maple planks + 65 Ramie cloth + 65 Zinc fittings |
| 6 | 6 | House level 12 | 75 Cedar planks + 75 Jute cloth + 75 Nickel fittings |
| 7 | 7 | House level 14 | 90 Redwood planks + 90 Sisal cloth + 90 Chromium fittings |
| 8 | 8 | House level 16 | 100 Ebony planks + 100 Lotus cloth + 100 Titanium fittings |

El aspecto de la casita sigue creciendo con la madera del rango: un granero de arce, un establo de cedro, una casa de animales de secuoya y un pabellón de ébano.

**Mascotas comunes:** las seis mascotas de la Parte 1 mantienen su producción (un material por tirada, 30 minutos, tope de ocho horas, contenedor de 16 entradas). Sus grupos de suministro incluyen ahora los recursos sueltos descubiertos de los rangos 5-8:
- **Madera:** Maple, Cedar, Redwood y Ebony branches.
- **Piedra/mineral:** Amber, Gneiss, Zinc ore, Magnetite, Gypsum, Agate, Nickel ore, Kaolin, Celestite, Almandine, Pumice, Chromium ore, Travertine, Malachite, Tourmaline, Red clay, Titanium ore, Serpentine, Moonstone, Aquamarine, Opal.
- **Fibras:** Ramie, Willow bast, Jute, Sisal, Lotus fiber, Indigo leaves, Mulberry leaves.
- **Productos/hierbas:** Pears, Rye, Plums, Chestnuts, Calendula, Chicory root, Apricots, Buckwheat, Blackcurrants, Hazelnuts, Mountain thyme, Quinces, Peaches, Millet, Raspberries, Pecans, Tarragon, Pomegranates, Saffron, Mangoes, Sorghum, Passion fruit, Pistachios, Cardamom, Star anise, Dragon fruit, Dates, Teff.
- **Semillas:** Safflower, Fennel, Sesame, Amaranth y Black sesame seeds.
- **Excluidos para las mascotas comunes,** como las pieles en los rangos 1-4: cueros, pelaje, Silk cocoons, Goat down y Maple sap. Algunas mascotas exóticas pueden traer tres de ellos (abajo).
- La tirada 70/30 del rango más alto usa ahora el rango más alto descubierto, hasta el 8.

### 16.5.1 Mascotas exóticas (propuesta, petición del usuario, 5 de noviembre de 2026)

Seis **compañeros exóticos**, uno por tema de paisaje de los rangos 5-8, a la venta en la misma sección de Mascotas de la tienda de la casa. Son el único lugar donde la Parte 2 añade una producción más fuerte (usuario: "más exóticas y con adquisición de recursos aumentada"). Los límites de equipo, comida y muebles siguen en el rango cuatro.

**Mejoras frente a una mascota común:**

| Rule | Common pet (Part 1) | Exotic pet |
| --- | --- | --- |
| Roll interval (offline) | 30 min | **20 min** |
| Units per roll (Release) | 1 | **2** |
| MechanicsTest units per roll | 1,000 | 2,000 (two linked 1,000-unit entries per opportunity) |
| Collection container | 16 opportunities / 16 entries | **24 opportunities**: 24 entries in Release; 48 entries in MechanicsTest |
| Highest discovered tier / earlier tiers | 70% / 30% | **85% / 15%** |
| Specialty source | none (or Truffle for the Pig) | can bring one normally excluded item |

Todo lo demás no cambia:
- una mascota exótica ocupa un espacio de la casita
- sin mantenimiento ni duplicados
- el tope de ocho horas por ausencia sigue vigente
- no se acumula nada más allá del contenedor
- las tiradas solo usan fuentes descubiertas, y un grupo vacío o sin descubrir se salta y el resto se reparte de nuevo
- ninguna receta obligatoria depende de una mascota: todo objeto de especialidad tiene también un origen suelto o de árbol

**Corrección de capacidad, 6 de noviembre de 2026:** contar **oportunidades** separadas de pilas. Cada oportunidad exótica tira un único objeto/fuente/calidad: Release guarda 2 unidades en una entrada; MechanicsTest 2,000 en dos entradas vinculadas de 1,000 con el mismo resultado/perfil. Persistir ID de oportunidad y ambos hijos atómicamente, sin segunda tirada. En 480 minutos la cola vacía recibe 24 oportunidades: 48 unidades Release o 48,000 MechanicsTest. La cola de prueba requiere 48 posiciones; la mochila sigue en pilas de 1,000. Las comunes mantienen 16 oportunidades/entradas.

La reclamación parcial conserva oportunidad y restos; el espacio se libera solo al reclamar todo. Detener con 24 oportunidades ocupadas y descartar tiempo limitado sin acumular. No regenerar hijos, fusionar oportunidades para eludir el límite ni borrar colas al cambiar mascota/mejorar casita. La cola pertenece a cada mascota. Migrar resultados sin inventar recompensas: reconstruir vínculos solo de IDs de generación registrados. Liquidar el intervalo anterior al cambiar, conservar temporizador/fuentes originales; las siguientes oportunidades usan el intervalo nuevo. Revisar cambios 20/30 minutos en ambas direcciones.

**Precio y desbloqueo:** **10.000 de efectivo** cada una (diez veces una mascota común), sin oro. Cada una necesita **la casa en nivel 9** y haber descubierto su región de origen. Son metas de ahorro para jugadores que ya están en el rango cinco. La compra y el registro de la mascota siguen siendo atómicos.

| Pet ID / species | Unlock region | Wood | Stone/ore | Fibers | Produce/herbs | Seeds | Specialty (exact item) | Role |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `pet.red_panda` — Red panda | 28 Golden Hollow | 15% | 5% | 5% | 45% | 20% | 10% Maple sap | Orchard foods and sap for syrup |
| `pet.alpaca` — Alpaca | 35 Whistling Escarpment | 5% | 0% | 45% | 15% | 5% | 30% Goat down | Cashmere supply for the tier-6 Set B |
| `pet.fennec_fox` — Fennec fox | 38 Ochre Prairie | 5% | 55% | 10% | 15% | 15% | 0% | Ores, stones and gems for house deposits |
| `pet.capybara` — Capybara | 44 Mirrorpetal Marsh | 15% | 5% | 35% | 35% | 10% | 0% | Fibers and grains for cloth and stews |
| `pet.peacock` — Peacock | 46 Starlit Gardens | 0% | 40% | 0% | 25% | 25% | 10% Silk cocoons | Spices, gems and silk |
| `pet.pangolin` — Pangolin | 47 Prismatic Inlet | 30% | 50% | 10% | 10% | 0% | 0% | Late building supplies (a stronger Dog) |

- **Filas:** cada fila suma 100%.
- **Objetos de especialidad:** Maple sap, Goat down y Silk cocoons siguen siendo objetos de recolección o de árbol. Una mascota exótica que los trae añade una fuente, no la única.
- **Piedra/mineral** incluye las gemas (Agate, Celestite, Almandine, Tourmaline, Malachite, Moonstone, Aquamarine, Opal), así que el Fennec fox y el Peacock ayudan con las prendas del Conjunto B con adornos de gemas y con los depósitos de los niveles 12/14/16 de la casa.
- **Rangos superiores:** las mascotas exóticas también tiran en los grupos de los rangos 1-4, pero su 85% del rango más alto las mantiene centradas en el bloque actual.
- **Arte:** un modelo facetado por especie, del tamaño de las mascotas comunes, visible en la casita.

**Comprobaciones:**
- seis filas al 100%
- desbloqueos con casa nivel 9 y la región
- las mejoras de 20 minutos y 2 unidades solo en IDs exóticos
- cola vacía a 4 horas: 12 oportunidades / 24 entradas MechanicsTest; a 8 horas: 24 oportunidades / 48 entradas MechanicsTest; sin exceder tiempo/capacidad
- a 8 horas Release: 24 entradas / 48 unidades; MechanicsTest: 48 entradas / 48,000 unidades; reclamación parcial/recarga sin duplicar hijos ni liberar oportunidades incompletas
- objetos de especialidad solo tras descubrir su región
- ningún ingrediente obligatorio exclusivo de mascotas exóticas
- admisión limitada por el nivel de la casita, con la regla de liquidar y luego cambiar en cada espacio
- recarga sin nuevas tiradas ni recompensas dobles

### 16.6 Comprobaciones

- cada material de depósito tiene una misión o un origen suelto no más tarde que las regiones de su nivel
- los niveles 9-16 no se pueden completar antes de sus regiones
- ninguna bonificación nueva ni tamaño de cofre por encima de los límites de la Parte 1; producción de mascotas por encima de la Parte 1 solo en las exóticas (sección 16.5.1)
- el aspecto de las tiendas cambia una vez en las regiones 25/31/37/43 y se mantiene tras recargar
- los desbloqueos del campo maderero necesitan descubrimiento y oro
- los niveles de los vecinos no pasan de 16
