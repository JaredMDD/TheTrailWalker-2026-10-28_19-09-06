# TheTrailWalker: diseño revisado y plan de desarrollo paso a paso

**Revisión de lógica de progresión - 6 de noviembre de 2026:** puentes de construcción antes del asentamiento, orden separado de códigos estables, despensa explícita para llenado y capacidad exótica por oportunidades separadas de pilas. Véanse Parte 1 secciones 9.0.1 y 11.6.1; Parte 2 secciones 10.1, 15 y 16.5.1. **Diseño actualizado; esta edición no certifica implementación, pruebas, compilación ni dispositivo.** La otra herramienta debe revisar [la nota de lógica y pruebas](Docs/PROJECT_HANDOFF.md#logic-review).

Actualizado: 6 de noviembre de 2026 (decisiones del 2 de noviembre sobre el asentamiento en la sección 13: parcelas y casas vecinas, tenderos y fachadas, orden de la calle, mejoras de tiendas y campo maderero; interior de la tienda del globo en la sección 14; bebidas de barril resueltas en las secciones 9.0.1 y 9.4.5; lapislázuli pulido en las secciones 9.5.1 y 11.6). Antes: 5 de noviembre de 2026 (planificación de los rangos 2-4 en las secciones 9.6 y 11.6; depósitos de casa y casita de mascotas en la sección 13).

**Progresión posterior al rango 4:** [Recomendaciones Parte 2](PROJECT_RECOMENDATIONS_PART_2_ES.md) reúne avances reservados de rangos 5–8, datos pendientes e hitos propuestos de expansión. La implementación actual conserva la base de cuatro rangos.

**Revisión confirmada de recolección en primera persona:** los Hitos 1 y 2 usan vista en primera persona y permiten recoger objetos visibles y sin obstáculos hasta el alcance de la cámara y de la detección táctil. Los límites anteriores de 8 m y 15 m quedan sustituidos. Conservar mirada independiente de 360 grados, marcha solo hacia delante, parada/reanudación, pilas completas, propiedad y comprobación de obstáculos. Los gestos de recursos deben comenzar debajo del área superior de notificaciones del sistema, respetando el área segura y la altura de pantalla. La superficie izquierda SPEED permite controlar la velocidad sin conflicto con la selección de recursos. Quedan pendientes pruebas de uso a velocidad máxima y en más dispositivos.

**Alcance confirmado — solo depuración y pruebas:** las 24 regiones y los cuatro rangos de recursos definidos son la base inicial, no el producto completo ni final. Aún falta definir e implementar materiales y contenido de rangos superiores. Mantener recogidas de 1,000 unidades, concesiones de depuración, selectores de viaje/rango y demás herramientas de prueba durante los hitos actuales. Completar un hito de pruebas no autoriza reequilibrar, retirar/desactivar concesiones de depuración, cambiar a cantidades de recolección de lanzamiento, preparar una versión final ni desplegar/publicar. Cada una de esas fases requiere una instrucción explícita futura del usuario. Compilar e instalar APK de desarrollo en dispositivos sigue siendo parte de las pruebas. Las referencias a `Release` describen intención de diseño aplazada, no requisitos activos de implementación o aceptación; este alcance sustituye las transiciones automáticas anteriores después de las pruebas.

Versión en español de [PROJECT_RECOMENDATIONS.md](PROJECT_RECOMENDATIONS.md), preparada para compartir y recibir comentarios. Conserva las reglas, propuestas, cifras, tablas e identificadores técnicos del original. No se han modificado el documento en inglés, `Trail.md`, el código ni la configuración de Unity.

Terminología: **rango** significa la categoría de progresión de recursos/equipo (`tier`); **nivel** se utiliza para el dominio de una receta o la casa; **calidad** conserva Q1–Q5. Una **pila** es un grupo de unidades de inventario; un **lote de botín** es un objeto recogible del mundo. `Misc` significa artesanías/objetos varios, como esculturas, juguetes y joyería. Los identificadores entre comillas invertidas y las rutas se mantienen para relacionar ambas versiones.

**Últimas incorporaciones confirmadas:** las cajas del globo ofrecen más recetas opcionales de comida con mayor recuperación e ingredientes de regiones descubiertas; el mapa del campamento muestra los campamentos/tramos de la región actual y, debajo de su primer campamento, las regiones descubiertas anteriormente. Viajar dentro de la región lleva al campamento elegido; cambiar de región lleva al primer campamento del destino. Los alimentos/bebidas consumidos regeneran resistencia progresivamente mientras se camina y conservan la recuperación pendiente que supera la capacidad actual del medidor. Véanse las secciones 4.4.3, 9.4.2 y 12.3. Las cantidades, velocidades de recuperación y selecciones de panadería siguientes son valores iniciales propuestos.

**Respuestas confirmadas de la sección 20:** la fabricación sigue abierta y muestra el producto en el centro de la cuadrícula 3 x 3 antes de dejarlo caer a la mochila con su temporizador; el inventario usa colocación libre sin rotación; la durabilidad depende de familia/rango y se duplica para Élite; se aplican descuentos NPC con equipo de nivel uno, calidad aleatoria y sin bonificaciones de muebles; las misiones solo comprueban ID exacto/cantidad; los cofres usan un contenedor con desplazamiento horizontal; el almacén acepta cualquier artículo listo y vendible y paga 100% en efectivo más 1 de oro por cada 1.000 de efectivo por cada diez cajas completadas (revisado el 31 de octubre de 2026; antes 3%), sin bonificaciones adicionales. Las pruebas siguen con pilas completas; la recogida unitaria queda aplazada hasta una solicitud explícita, sin cambio automático al terminar las pruebas. Las secciones siguientes incorporan estas respuestas y los valores delegados de las preguntas 5 y 9.

**Última revisión de mascotas/trufas:** conservar tienda de mascotas desde nivel de casa tres, casita frontal para elegir una mascota propia y contenedor frontal separado para recompensas inactivas. La sección 13.5 propone seis mascotas con pesos distintos, compra y filtros de origen. Añadir Trufa exacta como ingrediente recolectable de rango dos; la asignación propuesta en región 10 sustituye Lavanda suelta, conservando lavanda en catálogo de granja descubierto y tres recursos sueltos por región. La sección 9.4.3 añade una receta de misión y dos de caja; la colección pasa a 26 recetas (21 Food, 5 Misc). Especies, pesos, precios y valores de recetas son datos iniciales propuestos.

## 1. Propósito, autoridad y estado actual

Este documento analiza `Trail.md`, resuelve reglas contradictorias, propone recursos y recetas adicionales y organiza la implementación para una persona con experiencia en programación que está aprendiendo Unity. `Trail.md` sigue siendo el registro original del diseño y no se ha editado. Esta evaluación no ejecutó Unity ni produjo una compilación Android.

Etiquetas utilizadas:

- **Requisito:** regla de `Trail.md` que debe conservarse.
- **Confirmado:** instrucción posterior explícita del desarrollador.
- **Recomendación:** ajuste propuesto; no equivale a sustituir un requisito ya aprobado.
- **Valor inicial:** dato editable para pruebas, no una promesa de balance definitivo.
- **Pospuesto:** fuera del alcance actual de depuración/pruebas; completarlas no lo activa. Reequilibrio, retirada de depuración, preparación de lanzamiento y despliegue requieren una instrucción explícita futura del usuario.

**Prioridad de pruebas confirmada:** cada recurso apilable recogido del sendero entrega una pila completa de **1,000 unidades**. Mantenerlo durante las pruebas de mecánicas y límites. Todavía no reducir las cantidades para lograr progresión natural o mayor diversión. Las herramientas y prendas siguen siendo instancias individuales no apilables. El balance posterior será una fase independiente.

**Revisión de fabricación confirmada:** las recetas de materiales no tienen nivel de dominio. Ejemplos: 3 ramas → 1 tronco y 3 troncos → 1 tabla. Si el material también puede obtenerse mediante recolección o interacción, su conversión es instantánea. Las demás recetas tienen niveles 1–10 según la cantidad fabricada. La calidad llega como máximo a Q5; después del nivel cinco siguen cambiando precio, durabilidad aplicable y temporizador. La durabilidad suma tres componentes: base según nivel de receta, bonificación por calidad del producto y bonificación del mueble correspondiente. Los muebles de calidad influyen en la calidad final, no son la única fuente de durabilidad por calidad. Ningún mueble cambia el tiempo de fabricación.

**Regla actual de misiones:** la sección más reciente de `Trail.md`, **revisions missions and region resources**, sustituye las tres ramas anteriores y el orden rígido por categoría de receta. Ahora hay cuatro: Recolector —sombrero—, Leñador —botas—, Cazador —camisa— y Explorador —pantalones—. Cada rama tiene una secuencia propia de recetas variadas con prerrequisitos explícitos. Solo puede haber una misión activa. La sección 11 conserva los ejemplos iniciales y propone después materiales avanzados, mochilas, comida y artesanías.

**Reglas actuales de herramientas:** tres familias: hachas, armas de caza y cañas de pescar. El rango uno solo tiene variante Normal; desde el dos existen Normal y Élite. Élite tiene mayor durabilidad base, necesita menos acciones contra objetivos de su rango o inferiores —respetando el mínimo de una acción— y mejora el botín de **todos los objetivos elegibles**, incluidos los de rango superior. Esto sustituye la regla anterior de acciones iguales. Ambas variantes pueden trabajar como máximo dos rangos por encima del propio; sin una herramienta válida transportada, la interacción no comienza.

**Última revisión de mejoras Élite y cantidades de misión:** una herramienta Élite consume **una herramienta Normal concreta del mismo rango más materiales avanzados del rango anterior**. No se vuelve a pagar otro conjunto avanzado del rango actual. Se consume la instancia Normal y se fabrica una Élite nueva, siempre x1 y con ingredientes exactos. Las misiones empiezan en diez objetos para el rango uno y llegan hasta **150 para el cuatro**. La sección 11.4 propone los valores intermedios. Las recogidas siguen entregando 1,000 unidades durante las pruebas.

**Resistencia, misiones y comercio confirmados:** los corazones representan únicamente resistencia. Caminar la consume; alimentos/bebidas la restauran; campamentos y asentamiento la regeneran gradualmente, incluso durante el comercio. Los animales son objetivos pasivos que se mueven: hay que apuntar, pero no atacan ni dañan al jugador. Las misiones consumen solo las cantidades necesarias y pueden tomar herramientas o prendas de repuesto directamente. La ropa y mochila actualmente equipadas están protegidas. Las herramientas en mochila/ganchos no están equipadas y sí pueden consumirse. Se elimina la fabricación por lotes de objetos no apilables. Los muebles desbloquean su bonificación únicamente con la primera compra. El comercio ocupa una sesión exclusiva de 15 segundos; Volver la cancela, vende los anuncios propios pendientes al 70% y agota esa oportunidad de la visita.

**Cámara, desgaste y movimiento confirmados:** mantener/deslizar horizontalmente gira la cámara 360° sin detener ni dirigir al personaje. El gesto vertical sobre el avatar sigue controlando reducir, detener y reiniciar la marcha. Las prendas equipadas pierden durabilidad por distancia; al llegar a cero desaparecen y su ranura vuelve inmediatamente a sin sombrero, sin botas, camisa/pantalones/mochila de harapos, perdiendo sus bonificaciones. La base sigue siendo tres corazones y marcha lenta. Las mejores botas llegan como máximo a paso ligero y los mejores sombreros a doce corazones. Talado, caza y pesca paran al jugador al comenzar la animación y lo dejan parado para recoger hasta un nuevo gesto ascendente. Abrir inventario detiene temporalmente; cerrarlo restaura la velocidad previa, dentro de los límites y demás condiciones vigentes.

**Capacidad y alcance confirmados:** camisa/pantalones de harapos comienzan con **cero bolsillos/cero ganchos**. Los límites independientes son **diez bolsillos y diez ganchos**. Las pilas admiten hasta 1,000 unidades y el modelo crece como máximo **1.5 veces sus dimensiones lineales base**. El inventario considera tamaño/capacidad, nunca peso. El objetivo actual son **24 regiones y cuatro rangos de madera/animales**. Otros cuatro rangos quedan en la lista de futuras expansiones, sin dependencias obligatorias.

**Fabricación y tienda confirmadas:** los materiales sirven para recetas del jugador, recipientes avanzados y mejoras de casa; no fabrican muebles. Los muebles, incluidas estaciones decorativas y almacenamiento, se compran al desbloquear su nivel de casa. Simplificar procesos reales a pocos ingredientes: queso con leche y sal; la espera representa los pasos omitidos. Son válidos ladrillos/paneles de vidrio para mejorar la casa, no fabricar directamente ventanas u hornos como muebles.

**Identidad de recetas confirmada:** cada receta tiene ID propio y cantidades de ingredientes con IDs exactos. No se intercambian variedades: leche de vaca no sustituye a la de cabra, abedul no sustituye a pino y un material superior no reemplaza automáticamente a otro. Una receta mixta enumera ambas variedades y exige cada cantidad. Las variantes son recetas/desbloqueos/contadores distintos, no una receta con selector. Los diagramas de familias no son recetas ejecutables.

**Precios confirmados:** el material más básico del rango uno y Q1 empieza en **1 de efectivo por unidad**. Un objeto de referencia `Misc` de nivel diez tiene **precio base unitario equivalente a Q1 de 6,000**, antes de calidad/durabilidad. El orden base es **artesanías → mochilas → ropa → herramientas → materiales avanzados → comida → otros materiales/ingredientes → básicos**. Calidad y durabilidad aplicable aumentan el precio final. Los 6,000 no son un límite de venta Q5. Los repartos por rango y porcentajes son propuestas de prueba.

**Calidad de tiendas confirmada:** todos sus productos son **Q5**, tanto de efectivo como de oro. La excepción de compra es la cinta de comercio del campamento, donde se conserva la calidad anunciada. La tienda del globo es Q5 aunque esté en un campamento. Q5 no implica rango cuatro, dominio diez ni durabilidad infinita.

**Última revisión regional y de recuperación:** cada una de las 24 regiones destaca tres recursos sueltos diferentes: 18 por bloque de seis regiones, 72 en total. También hay árboles, animales y puntos de pesca de su rango. Ramas y piel son coincidencias base; se permiten también ingredientes de árbol con doble fuente explícita (7.1.1); en el rango tres se usan **plumas de pavo**, confirmado posteriormente. Troncos, carne, otros productos de caza y peces no añaden posiciones a la lista suelta. Las primeras regiones tienen 10–14 campamentos y las posteriores llegan a 24. Se desbloquea comida en la primera región. Llegar a cero sin recuperación pendiente ni comida/bebida utilizable provoca desmayo, recuperación y pérdida aleatoria de 2–4 entradas/pilas transportadas, excepto equipo puesto.

**Límite de distancia más reciente:** un tramo entre campamentos del rango superior consume **como máximo 26 corazones al caminar**. El límite del sombrero sigue en doce: el resto se obtiene con comida, no ampliando la barra. La sección 6.2.4 propone distancias y tasas intermedias.

**Herramientas, pesca y cocina:** la sección de herramientas incluye explícitamente hacha, resortera y caña Normales de pino. Se propone ampliar los peces de ocho a dieciséis: cuatro por rango, con seis parejas distintas por bloque de seis regiones. Asar carne/pescado consume la madera nombrada como combustible/brocheta. El asado inicial usa una carne y una rama de pino; presas mayores pueden necesitar más ramas o tronco y ramas.

**Regla de rango de receta confirmada:** desbloqueada en rango uno, usa ingredientes del uno; en dos, del uno/dos; en tres, del dos/tres; en cuatro, del tres/cuatro. Depende del rango de desbloqueo diseñado, no del dominio o ubicación actual. **Excepción de cocina:** ingredientes globales explícitos, como harina y queso, siguen utilizándose en comidas posteriores. Se mantienen IDs exactos; no se concede acceso a ingredientes futuros ni se extiende a herramientas/materiales/artesanías.

**Pescadería y cajas:** cada renovación ofrece **dos pilas de pescado y una pila aleatoria de marisco exclusivo**, todas Q5 y de efectivo, con máximo de 1,000 unidades por oferta/renovación. Camarón, cangrejo y langosta son ejemplos exclusivos; se propone calamar como cuarto tipo. Sus recetas y otras comidas/artesanías opcionales pueden salir en cajas del globo. Una receta premia conocimiento, no un producto Q5 ni 1,000 pergaminos; comienza en dominio uno. Los premios físicos de las cajas siguen siendo Q5.

La marcha solo hacia delante sustituye el antiguo consejo de movimiento libre en un corredor. El tamaño sustituye al peso como capacidad en todo el juego. No se necesita joystick para caminar normalmente.

### 1.1 Base del proyecto comprobada

| Área | Estado observado | Consecuencia |
| --- | --- | --- |
| Unity | `6000.4.7f1`, Unity 6.4 | Mantener versión estable durante el prototipo |
| Renderizado | URP `17.4.0` | Base adecuada para móvil |
| Calidad | Android elige Mobile; la selección guardada y pipeline por defecto usan PC | Previsualizar/perfilar Mobile expresamente |
| Renderizador móvil | Forward, escala 0.8, una cascada, sin sombras suaves ni texturas depth/opaque obligatorias | Conservar valores económicos |
| Extras móviles | HDR activo, distancia de sombras 50, posprocesado de cámara de ejemplo | Desactivar efectos innecesarios para la medición inicial |
| Android | IL2CPP, ARM64, Vulkan y después OpenGL ES 3 | Base razonable; falta probar en dispositivo |
| SDK Android | API mínima 25; objetivo automático/mayor instalada | Elegir cobertura y verificar requisitos de publicación después |
| Entrada | Input System `1.19.0`, solo sistema nuevo | Construir sobre él los gestos |
| UI/pruebas | uGUI `2.0.0`, Test Framework `1.6.0` | Suficiente para interfaces/pruebas iniciales |
| Escenas de compilación | Solo `SampleScene.unity` | Sustituir por arranque y escenas de prototipo al implementar |
| Mecánicas | Sin scripts, prefabs ni definiciones de ensamblado de juego | Crear fundamentos antes de producir mucho contenido |

El proyecto continúa siendo una plantilla URP. No se verificaron herramientas Android instaladas, destino activo ni rendimiento real; no se ejecutó Unity ni un APK.

## 2. Juego previsto y alcance

Ciclo principal:

1. Salir del campamento y avanzar por el sendero definido.
2. Controlar velocidad mediante gesto vertical sobre el avatar y mirar alrededor para alcanzar recursos casi perdidos.
3. Recoger pilas completas e interactuar con árboles, animales y pesca.
4. Organizar mochila espacial, bolsillos y ganchos.
5. Administrar resistencia, comida, desgaste y usos de herramientas.
6. Llegar al campamento, comerciar, reclamar misiones, fabricar y prepararse.
7. Pagar una vez el paso fluvial para descubrir otra región.
8. Tras la región cuatro, establecer la casa y sus sistemas duraderos.

Inicialmente será individual y funcionará sin conexión. Los PNJ simulan el comercio y las casas vecinas son decorativas. Quedan fuera redes, mercados entre jugadores reales, compras con dinero real, IA de combate y mundos procedurales.

### 2.1 Límites de entregables

| Entregable | Incluye | Se excluye hasta después |
| --- | --- | --- |
| Prueba de movimiento | Ruta curva, velocidad, cámara 360°, pausa de inventario, parada de campamento | Recursos/economía |
| Prueba de recogida | Pilas completas, geometría, pausa/reanudación, guardado | Catálogo extenso |
| Corte de mecánicas | Dos tramos, campamento, tres interacciones, cuatro ramas, fabricación/comercio/equipo/desmayo | Las 24 regiones terminadas |
| Prueba de asentamiento | Acceso de prueba, casa, almacenamiento, almacén comercial, un rango de arboleda | Todo el arte de muebles/tiendas |
| Versión completa de mecánicas | Todos los tipos de sistemas, contenido provisional, acceso de depuración a límites | Balance natural |
| Fase de progresión | Contenido regional, precios, cantidades, ritmo, presentación | Sistemas nuevos sin necesidad demostrada |

Probar asentamiento/rangos altos mediante viaje de depuración antes de construir las 24 regiones. Empezar con un tramo representativo por rango; las regiones son el plan de contenido, no un requisito para validar mecánicas. El asentamiento sigue entre regiones cuatro y cinco, dentro del primer rango.

## 3. Decisiones y contradicciones del diseño

| Tema | Análisis | Interpretación o recomendación |
| --- | --- | --- |
| Pilas y cantidad aleatoria | Es ambiguo obtener 2–5 recursos si cada recogida son 1,000 | **Confirmado:** 1,000 por objeto; **propuesta:** el número aleatorio cuenta lotes |
| Capacidad/peso | El peso está excluido | Solo huella, límites y área de mochila; sin sobrecarga por peso |
| Recursos/ganchos | Una frase parece intercambiar los destinos | Recursos pequeños a bolsillos, herramientas a ganchos |
| Familias/rangos/variantes | Se sustituyen acciones iguales y recetas Élite costosas del rango actual | Normal exacta + avanzado anterior; Élite ahorra acciones hasta su rango y mejora botín de todos los objetivos válidos |
| Eficacia y duplicación de usos | Tres usos y 5–6 golpes de hacha no coinciden con una regla universal | Usar matriz de sección 8; curva alternativa de duplicación solo como configuración de prueba |
| Salud/resistencia | Salud separada contradice caza pasiva | Solo resistencia; caminar consume, comida/zonas seguras recuperan |
| Consumo de misiones | Pilas e instancias requieren tratamiento distinto | Cantidad exacta; herramientas/repuestos elegibles; ropa/mochila puestas protegidas |
| Recompensas | Completar en casa no debe iniciar otra misión silenciosamente | Marcar terminada y reclamar al PNJ de campamento |
| Lotes de equipo | Se eliminaron expresamente | Herramientas/prendas/mochilas solo x1, tanto en UI como dominio |
| Temporizadores | Distintos instantes impiden apilar pendientes | Lotes separados hasta estar listos |
| Materiales y dominio | Conversión y producto terminado progresan distinto | Material sin nivel; instantáneo si también se obtiene en mundo; demás recetas 1–10 |
| Q5 en niveles 5–10 | Misma calidad no implica mismo precio/durabilidad | Conservar perfiles de estadísticas por producto |
| Orden de misiones | Revisión añade Explorador y mezcla categorías | Cuatro ramas con prerrequisitos; sin puerta universal por categoría |
| Renovación | Las visitas controlan stock y comercio | Sesión terminada/cancelada se renueva tras salir y regresar o llegar a otro campamento |
| Oro del almacén | Antes 3% | **Revisado el 31 de octubre de 2026:** 1 de oro por cada 1.000 de efectivo cada diez cajas; resto entero en efectivo; registrar el ciclo antes de balancear |
| Muebles | Primera compra confirmada | Desbloqueo permanente por definición, sin duplicarlo al vender/recomprar |
| Fabricación/estaciones | Estaciones obligatorias podrían bloquear campamentos | Los muebles mejoran; recetas básicas conocidas siguen en campamento/casa salvo futuras marcas explícitas |
| Cajas y mochila llena | Seis pilas pueden no caber | Bandeja persistente; sin pérdida ni colocación forzada |
| Bolsillos | Un objeto pequeño puede tener pila enorme | Una pila elegible por bolsillo, compresión deliberada |
| Tiempo de interacción | Menús/comercio no deben gastar resistencia ni mover al jugador | Estados y motivos de pausa; no congelar globalmente todo |
| Cámara | Mirar atrás no cambia la marcha | Horizontal solo cámara; vertical en avatar controla velocidad |
| Parada de interacción | Reanudar automáticamente aleja al jugador del botín | La interacción borra intención de marcha; inventario restaura solo su propia velocidad previa |
| Equipo agotado | Antes se conservaba almacenamiento roto | Borrar instancia/bonos, restaurar harapos y transferir contenidos de forma segura |

Son valores de implementación para probar; mantenerlos en datos cuando sea práctico. La recogida de 1,000 está confirmada y no debe reducirse por estas propuestas.

## 4. Movimiento, gestos, cámara y resistencia

### 4.1 Desplazamiento por el sendero solo hacia delante

Representar el sendero mediante una spline diseñada o una ruta muestreada. Guardar el ID del segmento y la distancia recorrida sobre la ruta, en metros. Avanzar con `speedMetersPerSecond * deltaTime`; obtener la posición y orientación a partir del sendero. Usar una tabla de longitudes de arco para mantener una velocidad aparente constante en las curvas. Unity Splines es una opción razonable para diseñar rutas; para la primera prueba basta una ruta de puntos de paso tras una interfaz `ITrailPath`.

No usar NavMeshAgent para el jugador. CharacterController es opcional para colisiones y adaptación al suelo, pero no decide la ruta. El jugador no dirige libremente al personaje hacia los lados ni hacia atrás. Resolver las pendientes con alturas de terreno muestreadas o una ruta elevada diseñada. Mantener la cámara independiente del movimiento y suavizar su posición y rotación, evitando inclinaciones excesivas en las curvas.

### 4.2 Asignación y prioridad de los gestos

Usar un único `GestureRouter`: al comenzar un toque, asignarlo a su objetivo y mantener esa asignación hasta que se suelte o cancele:

| Objetivo de toque inicial | Gesto permitido | Efecto |
| --- | --- | --- |
| Área de impacto del espacio de pantalla de Avatar | Arrastre vertical; rueda del mouse sobre avatar en Editor | Cambiar la velocidad de marcha solicitada; gesto hacia arriba puede reiniciarse después de la cosecha |
| Avatar o área mundial desocupada | Mantener y arrastrar horizontalmente o deslizar horizontalmente | Orbita la cámara hacia la izquierda/derecha en 360 grados sin cambiar de movimiento |
| Recurso | Mantenga presionado y luego arrastre | Recoger y colocar en la mochila |
| Recurso | Deslizamiento rápido hacia abajo | Enviar hacia la apertura de almacenamiento rápido de la mochila |
| Artículo de inventario | Mantener sin arrastrar | Mostrar nombre, calidad, cantidad, disponibilidad, durabilidad; para comida, mostrar Comer |
| Artículo de inventario | Mantenga presionado y luego supere el umbral de arrastre | Mover/dividir/fusionar elemento |
| Árbol/animal/estanque | Toque o mantenga presionado brevemente | Iniciar la interacción de cosecha coincidente |
| Control de interfaz de usuario | Toque/arrastre manejado por la interfaz de usuario | Nunca controles también el movimiento o la recogida del mundo |

Umbrales iniciales: mantener 250 ms; umbral de arrastre adaptado a la densidad de la pantalla; Desliza rápidamente el recurso aproximadamente el 10% de la altura de la pantalla en menos de 250 ms. Ajustar en un dispositivo. Mantenga separadas las regiones afectadas por avatar/recursos. La interfaz de usuario gana, luego un recurso/objetivo de interacción seleccionado, luego la superficie de gestos del avatar/mundo. Las retenciones/arrastres de recursos nunca se reasignan a la órbita de la cámara en medio del toque. En el avatar, bloquee el gesto en la órbita horizontal de la cámara o en la velocidad vertical después de un umbral direccional inicial (se recomienda un eje dominante al menos 1.5 veces el otro). Un gesto diagonal no debe cambiar la velocidad y la cámara al mismo tiempo. Un arrastre vertical que comienza en un mundo vacío no cambia la velocidad. Dos toques pueden poseer gestos separados de cámara/velocidad/recursos, pero dos dedos no pueden poseer el mismo elemento.

**Detención temporal del inventario:** en la primera apertura del inventario, guarde la velocidad de caminata solicitada actualmente y establezca el movimiento real en cero mediante un motivo de pausa de InventoryOpen. Los paneles de detalles/equipos de elementos anidados no sobrescriben esa instantánea. Al cerrar el panel de inventario final se restaura la velocidad solicitada guardada, sujeta al límite del equipo actual, y se elimina solo el motivo de la pausa. Si el jugador ya estaba detenido, la velocidad guardada es cero. La resistencia vacía, la recolección activa, la llegada al campamento, la parada de recolección poscosecha, la carga de la escena y la suspensión de la aplicación aún impiden el movimiento; El cierre del inventario nunca los borra.

**Parada persistente al recolectar:** iniciar una interacción válida de talado, caza o pesca detiene el movimiento al comenzar su animación y pone la velocidad solicitada en cero. Una vez iniciada, terminar, fallar, cancelar, romper una herramienta o cerrar la interfaz de interacción nunca restaura la marcha anterior. El jugador permanece quieto para recoger el botín. Solo un nuevo gesto ascendente de velocidad sobre el avatar, después de terminar la interacción de recolección o apuntado, elimina esta parada y solicita una velocidad positiva. Girar la cámara, recoger objetos, comer o cerrar inventario no la eliminan. Ignorar los gestos de reanudación durante una animación de recolección ya iniciada; no dejarlos en cola para una marcha automática. Tocar un recurso no válido que nunca inicia una interacción no cambia el movimiento.

Modele los motivos de pausa temporal por separado de la intención de caminar persistente/parada poscosecha. Ejemplo: caminar a 1.0 m/s -> cosechar -> detener -> abrir/cerrar inventario -> permanecer detenido -> gesto de velocidad hacia arriba -> caminar. Por el contrario: camine a 1.0 m/s -> abrir/cerrar inventario -> reanudar a 1.0 m/s si aún está permitido. Guarde esta distinción en lugar de un único booleano móvil/no móvil que se sobrescriba fácilmente.

### 4.2.1 Órbita independiente de cámara de 360 grados

**Confirmado:** permitir una órbita horizontal completa alrededor del avatar para mirar atrás y recoger recursos que casi se pasaron. Mantener y arrastrar horizontalmente cambia el ángulo horizontal mientras se mueve el dedo; deslizar lateralmente también lo cambia. Se recomienda detener el giro al soltar, sin inercia, conservar el ángulo elegido y evitar el recentrado automático durante la recogida. La inclinación vertical puede quedar fija inicialmente o recibir un ajuste limitado opcional después; 360 grados se refiere al giro horizontal, no a voltear verticalmente la cámara.

Mantenga el objetivo de seguimiento anclado al avatar mientras la orientación de la cámara es independiente de la trayectoria tangente. La caminata continúa avanzando por el sendero a la velocidad solicitada actual durante el movimiento de la cámara. La órbita de la cámara no puede rotar la dirección de viaje del avatar, reducir la velocidad, agregar una pausa de movimiento o detener el desgaste de resistencia/distancia mientras camina. El jugador debe usar el gesto de avatar vertical separado para reducir la velocidad o detenerse. Mantenga el área de velocidad en pantalla del avatar utilizable desde todos los ángulos de visión.

Implementación recomendada: `CameraOrbitController` controla el ángulo horizontal; `FollowCameraRig`, el seguimiento, suavizado y prevención de colisiones con paredes/suelo; `TrailMotor` no consulta el ángulo ni los gestos de cámara. Las colisiones de cámara no deben empujar al jugador ni mover recursos del mundo. Guardar el ángulo en el estado de sesión/partida. No forzar saltos de cámara al terminar la recolección o cerrar inventario; recentrar solo mediante un control explícito o una regla de entrada a escena, fuera de un gesto activo. Desactivar la órbita durante el comercio exclusivo y las interfaces bloqueantes de inventario/transición, respetando sus reglas de entrada.

### 4.2.2 Recursos detrás del jugador

Mirar hacia atrás debe revelar elementos coleccionables cercanos en lugar de solo paisajes. No caduque/desaparezca el botín lógico simplemente porque su posición en el camino o el producto punto orientado a la cámara está detrás del avatar. Mantenga una ventana de recolección trasera configurable dentro del segmento cargado actualmente; Recomendación inicial: 15 metros de sendero detrás, sujeto a un radio físico de recogida y una línea de visión modestos. Muestre retroalimentación fuera de alcance en lugar de permitir recogidas a través de paredes o desde secciones adyacentes distantes de un camino circular.

Renderice/transmita por rayos los objetivos de recursos cercanos desde el ángulo actual de la cámara, incluso detrás del avatar, preservando al mismo tiempo las cantidades completas y el presupuesto de vista agrupada. La ocultación fuera del campo de visión (frustum culling) puede ocultar sus puntos de vista, pero no debe consumir ni eliminar el botín subyacente. Orbit no amplía el rango de recolección, no rebobina el jugador, no mantiene el segmento anterior cargado permanentemente ni recolecta automáticamente. Si todavía camina, el jugador puede eventualmente pasar más allá de la ventana trasera, por lo que es importante reducir la velocidad o detenerse deliberadamente. El segmento de autor sale para que la descarga inmediata del punto de control no bloquee las recolecciones de último segundo accesibles sin previo aviso.

### 4.3 Capacidades iniciales del equipo

Utilice cuatro rangos de capacidad de recursos/equipo activo, independientes de cinco grados de calidad y diez niveles de dominio de recetas. El avance regional dentro de un rango de seis regiones desbloquea recetas/variantes sin otro rango de especies de madera/animales. Hacer que los límites globales sean alcanzables dentro del actual plan de clasificación de cuatro rangos. Se proponen valores intermedios a continuación; Se confirman cero bolsillos/ganchos iniciales y diez como máximo de cada uno.

| Clasificación | Velocidad máxima de las botas, m/s | Sombrero max corazones | Bolsillos de camisa | Ganchos para pantalones | Ancho x alto de la mochila, unidades lógicas |
| --- | --- | --- | --- | --- | --- |
| Inicial, sin botas ni sombrero | 1,5 | 3 | 0 | 0 | 6 x 4 |
| 1 | 2,25 | 4 | 2 | 2 | 8 x 4 |
| 2 | 3,0 | 6 | 4 | 4 | 10 x 4 |
| 3 | 3,75 | 9 | 7 | 7 | 12 x 4 |
| 4 | 4,5 | 12 | 10 | 10 | 14 x 4 |

**Dos conjuntos por rango desde el rango dos (usuario, 5 de noviembre de 2026):** los valores de esta tabla son el límite de cada rango, que alcanza el **Conjunto B**. El **Conjunto A**, fabricado con las tres primeras regiones del bloque, queda a mitad de camino desde el límite anterior, redondeando hacia arriba. Por eso las prendas guardan valores de capacidad explícitos; ver la sección 9.6.1.

La velocidad solicitada varía desde cero hasta el límite de velocidad para caminar actual proporcionado por las botas. **Valores predeterminados confirmados:** sin botas da una marcha lenta (**1,5 m/s**, confirmado el 31 de octubre de 2026); ningún sombrero da tres corazones de resistencia; La camisa/pantalones de harapos no tienen bolsillos/ganchos y la mochila de harapos proporciona un área de inicio. Inicialmente, las herramientas/recursos deben caber en la mochila. El límite más alto de velocidad al caminar en el rango activa lo proporcionan las botas para caminar a paso ligero (**4,5 m/s** con botas de rango cuatro; cada rango de botas suma 0,75 m/s), los sombreros con doce corazones, las camisas con diez bolsillos y los pantalones con diez ganchos. El equipo comercial no puede exceder esos límites. Los trapos iniciales son representaciones alternativas que no se agotan y no tienen instancia vendible ni bonificaciones adicionales. Las dimensiones de la mochila son unidades de diseño virtuales, no kilogramos ni ranuras visibles.

### 4.4 Resistencia y desgaste

**Confirmado:** los corazones representan resistencia, no salud. Sólo la distancia real del camino hacia adelante lo agota; alimentos/bebidas reponerlo. Mantenga los costos basados en la distancia para que caminar lentamente o detenerse en la cosecha no supongan costos adicionales. Los **2 corazones/km anteriores son una frecuencia de prueba de región de apertura**, no una frecuencia global obligatoria. A ese precio un tramo de sendero tutorial de 250 m cuesta 0.5 corazón. Los segmentos posteriores generan un presupuesto fijo de distancia/drenaje, limitado a **26 corazones por sección de sendero** en el rango más alto; La sección 6.2.4 proporciona propuestas. El tiempo de estar de pie/menú, apuntar, cargar y teletransportarse nunca agotan la resistencia.

Con resistencia cero, detente. Si la comida/bebida preparada y accesible puede restaurar la resistencia, muestra Comer/Beber y deja que el jugador la use; no te desmayes inmediatamente. **Último requisito:** sin recuperación pendiente ni dicho consumible, desmayarse y aplicar el procedimiento de recuperación/pérdida en la sección 4.4.2. Se elimina la propuesta anterior de rescate gratuito en campamentos y sin pérdidas. La regeneración del campamento/asentamiento permanece continua, incluido el comercio/menús, sin acción de descanso. La tasa confirmada es 0.25 corazones/segundo, por lo que 15 segundos de intercambio activo restauran hasta 3.75 corazones según el límite. Los alimentos/bebidas inician una recuperación progresiva al consumirse, que continúa al cerrar el inventario y volver a caminar; la recuperación no utilizada se conserva (sección 4.4.3). El máximo global del medidor se mantiene en doce.

La regeneración es un servicio de simulación, independiente de los bloqueos de entrada de la interfaz de usuario y la animación comercial. Recomendación predeterminada: la recuperación pasiva de zona segura cuenta solo el tiempo activo de campamento/asentamiento en primer plano; la recuperación por comida también cuenta el tiempo activo en el sendero. Suspender la aplicación congela ambos servicios y la cuenta regresiva comercial restante. Esta revisión no requiere la recuperación de resistencia sin conexión. Conserve la resistencia actual y el tiempo comercial restante para que la reapertura no pueda reiniciar ninguno de los intervalos.

Los animales nunca atacan ni dañan al jugador. Pueden caminar dentro de un área de recursos local; La caza requiere que el usuario apunte al objetivo en movimiento y luego realice un disparo. Utilice el progreso de la recolección de recursos, no un sistema de combate/salud del jugador. Recomendación: un error cometido consume un uso de herramienta pero no avanza en la cosecha; cancelar antes del compromiso de disparo no consume nada. Valide la elegibilidad de la herramienta antes de ingresar al modo de apuntar y nuevamente antes de realizar un disparo.

Las herramientas pierden usos solo para acciones comprometidas. **Confirmado:** los sombreros, botas, camisas, pantalones y mochilas equipados pierden durabilidad según la distancia real recorrida, nunca por la rotación de la cámara, el tiempo del menú, la cosecha estacionaria, la teletransportación o el tiempo del asentamiento/campamento. La durabilidad del rango de ejemplo sigue siendo `2 + 2 * rank` km antes de las bonificaciones de dominio/calidad/muebles. Los prendas de repuesto que se llevan consigo no se desgastan. El equipo comercial de durabilidad infinita sigue estando exento.

### 4.4.1 Agotamiento del equipo y restauración inmediata del equipo predeterminado

**Confirmado:** cuando la durabilidad finita del elemento equipado llegue a cero, elimine esa instancia de elemento por completo, elimine todas las bonificaciones de capacidad/estadísticas proporcionadas por él y reemplace su espacio equipado con el valor predeterminado correspondiente de inmediato. No mantengas equipado un artículo roto, no conserves su capacidad, no autoequipes un repuesto, no crees una carcasa reparable ni ofrezcas un reembolso por el rescate.

| Ranura agotada | Respaldo inmediato | Cambio de capacidad |
| --- | --- | --- |
| Sombrero | Sin sombrero | La resistencia máxima se convierte en tres corazones; limita los corazones actuales a tres sin conceder una recarga |
| Botas | Sin botas | La velocidad máxima para caminar se convierte en la velocidad lenta predeterminada; límite de velocidad solicitado/real inmediatamente |
| Camisa | Camisa de harapos | El recuento de bolsillos llega a ser cero; transferir todo desde los bolsillos retirados |
| Pantalones | Pantalones de harapos | El número de ganchos pasa a cero; transferir cada herramienta enganchada |
| Mochila | mochila de harapos | Regrese al área de la mochila inicial inmediatamente |

Las bonificaciones permanentes de estadísticas de muebles por primera compra permanecen desbloqueadas; Las bonificaciones perdidos aquí son los del equipo desaparecido. El equipo de repuesto no es un sustituto hasta que el jugador lo equipa deliberadamente. Las representaciones de trapo predeterminadas no se pueden vender, consumir en una misión ni equipar en un ciclo de pérdida de equipo.

**Regla de conservación de contenido recomendada:** retire el equipo agotado mientras transfiere su contenido de forma transaccional. La camisa/pantalones predeterminados no tienen bolsillos/ganchos supervivientes: mueva todo su contenido al espacio válido de la mochila y el resto a la bandeja de desbordamiento persistente. Si la mochila también caduca, utiliza la zona de la mochila de harapos. El artículo agotado nunca entra a la bandeja. El desbordamiento no proporciona capacidad activa adicional; su contenido no puede ser usado, vendido ni consumido por las misiones hasta que se reclame en un almacenamiento válido. Notifique al jugador sin retener las bonificaciones de elementos caducados para evitar la degradación.

Para el vencimiento de una bolsa, retire el contenedor/instancia antiguo y transfiera su contenido a la mochila/bandeja de trapo en la misma operación. Para varios elementos que vencen en la misma actualización de movimiento, calcule todos los valores predeterminados y planifique una transferencia antes de confirmar, en lugar de descartar elementos durante los cambios de tamaño secuenciales. Continúe con la eliminación del equipo, los límites recalculados, la velocidad/resistencia fijada y la nueva propiedad juntos para que la recarga no pueda restaurar un elemento destruido ni duplicar su contenido. Se reemplaza la recomendación anterior de dejar equipado el equipo de almacenamiento roto.

### 4.4.2 Desmayo, pérdida de inventario y recuperación

**Condición confirmada:** en el sendero, la resistencia llega a cero, no queda recuperación positiva por comida consumida y no hay comida/bebida preparada y accesible que recupere una cantidad positiva. No cuentan fabricaciones pendientes, contenido de cofres/estantería/bandejas, ingredientes crudos no comestibles ni comida ya entregada a una misión. Es una recuperación, no combate ni muerte. Detenerse en la posición actual del sendero, borrar la intención de marcha y reproducir desmayo/recuperación/despertar. Bloquear recogida, recolección, fabricación, viaje y cambios de equipo durante este estado para impedir operaciones simultáneas con la pérdida; permitir pausar la aplicación y consultar información no bloqueante sobre la pérdida/recuperación.

Sortear una pérdida de 2-4 **entradas de inventario** distintas y elegibles, sin repetirlas. Una pila seleccionada pierde toda su cantidad restante, no solo 2-4 unidades. Son elegibles recursos/comida de la mochila, pilas de bolsillos, herramientas de mochila/ganchos y ropa/mochilas de repuesto. Excluir sombrero, botas, camisa, pantalones y mochila puestos, saldos monetarios, almacenamiento doméstico, recompensas no reclamadas y reservas de fabricación/comercio. Si hay menos entradas que la cantidad sorteada, perder solo las disponibles; el inventario vacío no pierde nada. La selección uniforme por entrada es una propuesta inicial, no una selección basada en peso o valor.

Guardar un ID único del desmayo, los IDs y cantidades de objetos/pilas seleccionados, sus retiradas confirmadas, el estado de recuperación y la posición **antes** de mostrar las animaciones de pérdida. Animar hasta las 2-4 entradas realmente seleccionadas saliendo y desapareciendo: son objetos visuales, no botín recogible. Una pila de 990 unidades es una entrada y pierde las 990 si se selecciona. Al cargar o reanudar, no volver a sortear ni aplicar otra penalización, crear herramientas de repuesto o revertir entregas de misiones ya realizadas.

La resistencia se recarga progresivamente mientras estás desmayado, como excepción a la ausencia normal de regeneración al estar quieto en el sendero. Recuperar 0.25 corazones por segundo activo hasta el máximo actual del sombrero: desde cero, tres corazones tardan 12 segundos y doce corazones 48 segundos. Congelar/guardar al suspender la aplicación. Despertar solo después de completar recuperación y penalización, conservar bonificaciones y permanecer detenido hasta un nuevo gesto ascendente. Sin teletransporte, caminata automática ni recompensa de receta/moneda. La pérdida de herramientas puede impedir la recolección, pero la recolección suelta sigue siendo posible para la reconstrucción mediante recetas conocidas.

### 4.4.3 Recuperación progresiva por comida sin desperdiciar corazones

**Confirmado:** comer/beber consume una porción y añade toda su recuperación definida, incluida la bonificación de recuperación de los muebles correspondiente, a una cola persistente de recuperación pendiente. Nunca añade todos los corazones de golpe. Las tablas indican **corazones totales por porción a lo largo del tiempo**, sin aumentar el límite de resistencia. El jugador puede abrir el inventario, comer, cerrarlo y recuperar la velocidad válida previa mientras la comida regenera resistencia y la distancia recorrida la consume. Las paradas por cosecha, agotamiento o decisión manual siguen teniendo prioridad.

**Propuesta inicial de implementación:** una cola FIFO a **0.25 corazones por segundo activo**, compartida por alimentos y bebidas; seis corazones requieren 24 segundos de entrega efectiva. Varias porciones suman sus cantidades pendientes sin multiplicar la velocidad ni reemplazar la porción activa. Mostrar resistencia actual/máxima, corazones pendientes y velocidad de recuperación en el HUD y la ficha del objeto. Guardar la recuperación efectiva de cada porción al consumirla; cambiar muebles después no modifica ese valor.

Integrar continuamente el gasto al caminar y la regeneración en el mismo paso de simulación, dividiéndolo cuando se agote una porción o se alcance el máximo o cero. Descontar de la cola solo los corazones realmente aportados al medidor o al gasto simultáneo al caminar. Al estar detenido con resistencia máxima, pausar la entrega sin caducar ni descartar la cola; caminar crea espacio y reactiva la entrega. La recuperación pasiva de zona segura contribuye por separado a su velocidad existente y tiene prioridad para el espacio libre; la comida aporta el resto. La recuperación pasiva nunca gasta la cola. Perder un sombrero limita solo la resistencia actual y conserva la comida pendiente. Esa reserva consumible nunca amplía el medidor visible.

Ejemplos: con 3/3 corazones, comer una porción de seis deja 3/3 y seis pendientes. Estar detenido conserva los seis; caminar gasta resistencia y aprovecha la reserva progresivamente. Con 2/3, un gasto de 0.10 corazones/segundo y la recuperación propuesta de 0.25 dan +0.15 corazones/segundo hasta el máximo; después se entrega solo lo gastado al caminar. Si el gasto supera la recuperación, la resistencia baja: comer no garantiza caminar indefinidamente. Con cero y recuperación positiva pendiente, detenerse sin desmayo, seguir regenerando y permitir un nuevo gesto hacia arriba cuando haya resistencia positiva. El desmayo solo ocurre con cero, sin recuperación pendiente y sin consumibles preparados accesibles; esta condición precisa el activador de la sección 4.4.2. Resolver el último aporte antes de evaluar el desmayo.

La comida sigue regenerando durante inventario, cosecha, menús de campamento/asentamiento y comercio en primer plano; estar detenido en el sendero no da regeneración pasiva gratuita. Congelar la entrega durante cargas/suspensión de la aplicación; guardar saldos/orden, valores por porción, tiempo fraccionario e ID de transacción. Reanudar una sola vez, sin recuperación por comida sin conexión, doble consumo ni reinicio de cola al viajar. El consumo y la inserción en cola se guardan atómicamente. La vista previa de salida simula la reserva pendiente y la comida disponible, el ritmo de marcha y los tiempos; no descuenta excedentes nominales como desperdicio ni cuenta dos veces una porción. Un tramo de 26 corazones exige provisiones suficientes **y** una programación viable de recuperación.

## 5. Inventario, pilas e identidad de los objetos

### 5.1 Inventario espacial determinista

**Confirmado:** el tamaño del artículo, la no superposición, los límites y la capacidad de la mochila determinan el ajuste. El peso no es un límite, una penalización de velocidad, una penalización de resistencia o un sistema de carga secundario. No agregue un presupuesto de peso para transferir la validación. El desmayo es la mecánica explícita de pérdida de inventario; La falla de capacidad nunca destruye silenciosamente un artículo.

Mantenga la apariencia sin ranuras, pero evite pilas de cuerpo rígido simuladas en la interfaz de usuario. Representa cada entrada mediante una posición 2D y una huella rectangular dentro de un contenedor lógico. La precisión de la colocación oculta es aceptable; No se requieren celdas de ranura visibles. Validar límites y superponerlos de forma determinista. **Confirmado:** solo colocación libre, con orientación fija y sin controles de rotación. Validar límites/solapamiento con esa huella fija.

El almacenamiento rápido primero intenta una combinación compatible y luego busca espacio libre. El arrastre manual valida la ubicación deseada y se fusiona cuando se tocan las huellas de pila compatibles. Si una combinación amplía el resultado, valide la nueva huella antes de consumir la fuente. Se permite la fusión parcial hasta 1,000; Deje el resto en su ubicación válida anterior. Una recogida fallida permanece en el camino y no se puede recoger dos veces mediante entradas superpuestas.

Clave de pila: ID de definición + calidad + estado listo + perfil de estadísticas de salida y cualquier variación relevante para el juego. Incluya el precio intrínseco por unidad y cualquier otra estadística funcional diferente en ese perfil: dos productos Q5 fabricados en los niveles de receta cinco y diez pueden tener precios diferentes y no deben fusionarse silenciosamente. Los perfiles equivalentes pueden fusionarse incluso si la procedencia difiere. Los lotes de artesanía pendientes nunca se fusionan con artículos listos. Las herramientas, la ropa y las mochilas tienen ID de instancia únicos y nunca se fusionan.

### 5.2 Crecimiento limitado del tamaño de las pilas

**Confirmado:** las cantidades aumentan la huella/el tamaño del modelo hasta **1.5 veces la escala del modelo lineal base** para cada artículo apilable. El límite de cantidad se mantiene en **1,000 unidades**; La cantidad y la escala visual son límites separados.

Curva recomendada: `linearScale(q) = clamp(1 + 0.5 * log(q) / log(1000), 1, 1.5)` para cantidades 1-1,000. Produce 1x en una unidad y 1.5x en 1,000, con un área de huella rectangular de base como máximo de 2.25x; el límite de 1.5 es por dimensión, no un multiplicador de área. Elimine las entradas de cantidad cero antes de evaluar el logaritmo. Utilice el mismo crecimiento limitado para modelos y ajuste; Nunca lo apliques dos veces. Las recogidas full-stack utilizan la escala máxima durante las pruebas. El ajuste de estantes y el escalado de toda la interfaz de usuario son transformaciones de presentación, no capacidad adicional.

Formas nominales iniciales: gema/huevo pequeño 0.6 x 0.6; comida 0.8 x 0.8; tela/piel 1 x 1; ramas 1.6 x 0.6; troncos 1.8 x 0.9; herramienta 1.8 x 0.7. Anulaciones de autor por elemento para artesanías inusualmente grandes. No cree instancias de 1,000 modelos; renderice un ícono/modelo y una insignia de cantidad.

Los bolsillos aceptan una pila de recursos/alimentos elegibles o una unidad individual cada uno. Se pueden insertar pilas existentes, pero los bolsillos no combinan pilas separadas internamente. Mover una pila de bolsillos nuevamente a la mochila permite una fusión normal. Los ganchos contienen una instancia de herramienta compatible cada uno. Los equipables no se pueden esconder en un bolsillo de pequeños recursos.

limita la capacidad de bolsillo/gancho por separado a 0-10. Muestra solo el recuento actual, no diez posiciones utilizables en los trapos iniciales. A las diez, utilice un panel lateral legible de dos columnas/cinco filas o desplazable en lugar de objetivos táctiles reducidos. Destruir una mejora de camisa/pantalones devuelve su cuenta a cero; El contenido de diez bolsillos/diez ganchos debe transferirse de forma segura sin retener ranuras invisibles.

### 5.3 Propiedad de los objetos y transacciones

Cada artículo o cantidad tiene exactamente un propietario: paquete mundial, mochila, bolsillo, gancho, ranura para equipo, cofre de almacenamiento, estante, lote de artesanía, depósito en garantía de misión, depósito en garantía comercial, tienda o bandeja de recompensas. Las transferencias se comprometen por completo o dejan la propiedad sin cambios. Nunca utilice una animación de recogida o un elemento de la interfaz de usuario como registro de artículo autorizado.

La recolección de pila completa puede desviar las unidades requeridas por la misión antes de colocar la mochila. Ejemplo: un haz de 1,000 unidades de pino, faltando todavía 12 de pino, aporta 12 y deja 988 para colocar. Muestra ambos números. Si el resto no cabe, no comprometer ni la colecta ni el aporte misional; el paquete permanece intacto. Esto preserva una recolección atómica sin dejar caer las unidades sobrantes.

Las bandejas de recompensas son colas de entrega persistentes para elaboración, cajas, desbordamiento y recuperación de carga. No son almacenamiento consumible/comerciable y no cuentan para las misiones hasta que se reclamen. Exponga las cantidades en cola y nunca las trunque silenciosamente. Establezca límites a la creación de nuevos lotes cuando las colas de reclamos estén llenas, preservando al mismo tiempo las recompensas ya obtenidas.

## 6. Calidad, progresión de recursos y clasificación de objetos

### 6.1 Cinco grados de calidad y tres ejes independientes de progresión

| Grado | Nombre para mostrar | Línea base a nivel de receta para artesanías no materiales |
| --- | --- | --- |
| Q1 | Tosco | Nivel 1 |
| Q2 | Funcional | Nivel 2 |
| Q3 | Fino | Nivel 3 |
| Q4 | Superior | Nivel 4 |
| Q5 | Obra maestra | Niveles 5-10 |

Mantenga separados **número de región/rango de equipo de recursos**, **nivel de dominio de recetas** y **calidad de salida**. Un hacha de rango uno puede alcanzar el dominio diez/Q5 sin convertirse en un hacha de rango cuatro. Hay 24 números de región pero cuatro rangos de recursos activos. La velocidad, los bolsillos, los ganchos, el área de la bolsa y las límites de corazones provienen del rango del equipo, no del dominio o la calidad. La recuperación de alimentos permanece definida por la receta, aparte de su bonificación de muebles separada. Los alimentos/materiales/misceláneos no tienen durabilidad.

**Confirmado:** las recetas de materiales no se nivelan y su calidad artesanal está influenciada únicamente por muebles que mejoran la calidad de fabricación. Para los no materiales, el nivel de receta establece una durabilidad y calidad base hasta Q5; Los niveles seis a diez conservan el Q5 pero continúan cambiando el precio/durabilidad/temporizador base. La calidad del acabado añade durabilidad incluso sin muebles, y los muebles que mejoran la calidad de fabricación pueden aumentar esa calidad. Los muebles de durabilidad de la categoría correspondiente añaden un plus de durabilidad final. El precio ahora utiliza la base equivalente a Q1, una prima por calidad final y una prima de durabilidad aplicable, como se define en la sección 12.2. Ninguno cambia el tiempo de artesanía.

Utilice el cálculo determinista de estadísticas de salida, no la probabilidad de mejora de calidad aleatoria anterior ni la herencia de calidad de ingrediente más baja. La calidad de los ingredientes sigue siendo valiosa para la venta directa y los requisitos mínimos explícitos de calidad; no determina silenciosamente el resultado elaborado. El autocompletar aún permite una política de selección de ingredientes visible para que los materiales de alto valor no se gasten accidentalmente. Muestre el grado, el nivel de receta, el valor intrínseco y la durabilidad por separado en detalles.

Los porcentajes exactos y la fórmula de combinación de calificaciones que aparecen a continuación son valores iniciales recomendados que hacen que esas reglas sean ejecutables. No son requisitos de balance adicionales ya confirmados.

**Alcance confirmado de muebles:** las bonificaciones sobre artículos solo se aplican a productos fabricados por el jugador. Persistir `CreationSource` y modificadores de creación; compras, NPC, botín y cajas no reciben calidad/durabilidad/comida/venta de muebles por pertenecer al jugador. Las bonificaciones propias de receta/calidad sí se aplican. Equipo NPC usa L1 y Q aleatoria con `C = U = 0`; ofertas del jugador conservan estadísticas ya creadas. Al consumir comida, fijar una sola vez la bonificación elegible (cero si no fue fabricada por jugador). La venta de muebles solo beneficia artículos fabricados por jugador en ventas ordinarias, **nunca** pagos de almacén. Separar calidad de ingredientes e identidad de misión.

### 6.1.1 Orden recomendado para calcular las estadísticas

Entradas:

- `L`: nivel de dominio de recetas 1-10 para una receta no material; ausente para una receta material.
- `C`: resistencia total de muebles que mejoran la calidad de fabricación expresada como un nivel entero de 0 a 5, con un límite de cinco.
- `U`: bonificación total aplicable por durabilidad del equipo y mobiliario, como una fracción del tipo 0.20. Cero para materiales, alimentos y misceláneos.
- `Q`: calidad del producto terminado, con un límite de Q5, después de combinar la calidad a nivel de receta y los muebles que mejoran la calidad de fabricación.
- `PriceReferenceAtLevel10`: anclaje de precio base equivalente a Q1 del artículo en dominio diez, para objetos que no son materiales. En cambio, los materiales utilizan un `MaterialBasePrice` fijo sin dominio.
- `D0`: base máxima durabilidad en usos o distancia, sólo donde corresponda.

Curvas iniciales: `priceLevelFactor(L) = (L + 35) / 45`, que van desde 0.80 en el nivel uno hasta 1.00 en el nivel diez, y `durabilityLevelMultiplier(L) = 1 + 0.08 * (L - 1)`. La curva de precios propuesta por los autores hace referencia directamente al nivel diez. Aplicar el multiplicador de precio de calidad final del apartado 12.2; Los muebles que mejoran la calidad de fabricación aumentan el precio a través de la calidad del acabado, no de una capa `+5% price per C` directa separada.

Bonificación de durabilidad propuesta por calidad final:

| Calidad final | Fracción de bonificación `qualityDurabilityBonus(Q)` |
| --- | --- |
| Q1 | 0.00 |
| Q2 | 0.10 |
| Q3 | 0.20 |
| Q4 | 0.30 |
| Q5 | 0.40 |

**Estructura confirmada:** base ajustada en nivel + bono calidad del producto + bono durabilidad mueble. **Interpretación porcentual recomendada:** calcular ambos bonos a partir de la base ajustada por nivel y sumarlos; no aumente el porcentaje de muebles además de la bonificación de calidad. Estos porcentajes siguen siendo datos de prueba.

Calcular en este orden:

1. Establezca la puntuación de calidad base en `min(L, 5)` para objetos que no son materiales o `1` para materiales.
2. Establecer la calidad final mostrada `Q = clamp(baselineQualityScore + C, 1, 5)`. Q5 es el máximo incluso cuando el dominio y el mobiliario juntos superan cinco. La calidad del material fabricado no recibe aportaciones del dominio ni de la calidad de sus ingredientes.
3. Calcular el precio unitario base equivalente a Q1 `Pbase = PriceReferenceAtLevel10 * priceLevelFactor(L)` para objetos que no son materiales. Los materiales utilizan `Pbase = MaterialBasePrice`, sin factor de dominio.
4. Calcular la durabilidad base ajustada por nivel `Dbase = D0 * durabilityLevelMultiplier(L)` solo para herramientas/prendas.
5. Agregue la contribución de calidad del producto terminado: `DqualityBonus = Dbase * qualityDurabilityBonus(Q)`. Esto se aplica tanto a la calidad derivada de recetas como a la calidad asistida por muebles; no está condicionado a `C > 0`.
6. Añade el aporte de muebles de la categoría correspondiente: `DfurnitureBonus = Dbase * U`; La durabilidad final es `Dfinal = Dbase + DqualityBonus + DfurnitureBonus`. Tampoco multiplique la durabilidad directamente por `C` ni vuelva a aplicar la bonificación Q.
7. Aplicar el multiplicador de precio de calidad final una vez: `Pquality = Pbase * qualityPriceMultiplier(Q)`. Para herramientas, prendas y mochilas de diseño finito, configure `durabilityPriceMultiplier = Dfinal / Dbase = 1 + qualityDurabilityBonus(Q) + U` y `Pintrinsic = Pquality * durabilityPriceMultiplier`. Para artículos sin durabilidad, `Pintrinsic = Pquality`. Utilice el cálculo/las reglas completos de la sección 12.2; sin bonificación directa del precio C y sin segunda multiplicación por U.
8. Calcule el temporizador independientemente del rango/categoría/dominio/tamaño de lote de progresión como lo especifica la sección 10. Ni `C` ni `U` lo cambian.
9. Obtenga una instantánea del grado resultante, el valor unitario intrínseco, la durabilidad base, ambas bonificaciones de durabilidad, la durabilidad máxima final, el nivel y el perfil modificador al inicio de la elaboración. Utilice intermediarios de punto fijo/alta precisión y dinero/usos redondos una vez según una regla documentada.

La fórmula de durabilidad compacta es `Dfinal = D0 * durabilityLevelMultiplier(L) * (1 + qualityDurabilityBonus(Q) + U)`. Hay exactamente una bonificación por la calidad del producto y una bonificación equivalente por la durabilidad de los muebles. La etiqueta de calidad puede seguir siendo Q5 mientras que niveles adicionales de receta mejoran la base y, por tanto, la durabilidad final. Los muebles que mejoran la calidad de fabricación no tienen ningún efecto de durabilidad adicional una vez que se alcanza Q5; Los muebles que mejoran la durabilidad aún pueden agregar su propia ventaja. Estas mejoras no crean Q6-Q10. Los muebles de precio de venta y los multiplicadores de ubicación/comercio siguen siendo modificadores de precio posteriores en `Pintrinsic`, no la calidad artesanal.

Ejemplo de durabilidad: una herramienta de nivel seis con `D0 = 100 uses`, `C = 2` y `U = 0.20` tiene Q5; nivel base 140 + calidad 56 + mobiliario 28 = **224 usos**. Sin ninguno de las bonificaciones de los muebles, esa receta todavía rinde Q5 y **196 usos**. Calcule su precio por separado utilizando las capas base/calidad/durabilidad de la sección 12.2. Un material con Q1 fijo en base 10 y `C = 2` tiene Q3 y, con el multiplicador de calidad propuesto de 1.35, un valor unitario intrínseco de 13.50 antes del redondeo de la transacción; sin dominio/durabilidad.

Para la interfaz de usuario, muestre un desglose legible como `140 base + 56 quality + 28 furniture = 224 uses`; La durabilidad de las prendas utiliza la distancia en lugar de los usos. Trate la durabilidad inicial/actual finita como el máximo final redondeado en el momento de la creación. Nunca restablezcas el equipo existente a un máximo recién calculado cuando su receta suba de nivel o cambien los muebles.

Los productos recolectados utilizan sus tablas de calidad de origen. **Las compras en tienda son siempre Q5**; Las compras de transportadores de campamento conservan la calidad del listado. Crear perfiles intrínsecos/de durabilidad del proveedor independientemente del dominio/equipamiento del jugador; comprar nunca sube de nivel ni desbloquea una receta. Aplique la prima del Q5 una vez a las estadísticas de referencia del proveedor o almacene un perfil del Q5 ya calculado, nunca ambas cosas. El equipo de proveedores finitos obtiene la bonificación de durabilidad Q5 en su base de autor, sin la bonificación de muebles artesanales del jugador. El equipo comercial especial de durabilidad infinita utiliza su precio/bandera de autor. Ser Q5 no hace que el equipo sea indestructible ni aumenta su rango de capacidad.

### 6.2 Veinticuatro regiones y cuatro rangos activos de recursos

**Alcance confirmado:** 24 regiones comparten cuatro rangos de madera/animal. **Distribución recomendada:** seis regiones consecutivas por rango. El rango de recursos es distinto del número de región, variante de equipo, dominio de recetas y nivel de casa. Las clasificaciones de especies son progresión del juego, no afirmaciones sobre fuerza/valor en el mundo real.

| Rango de progresión de recursos | Regiones | Madera / animal salvaje | Banda de peces / rango de herramienta efectiva | Materiales introducidos dentro del bloque |
| --- | --- | --- | --- | --- |
| 1 | 1-6 | Pino / conejo | A/1 | Primero las manzanas, luego el cáñamo/pedernal, la ortiga/arcilla/sal, la piedra caliza/hierbas/frutas, la avena, el trigo/juncos/arena; lácteos del asentamiento |
| 2 | 7-12 | Abedul / castor | B/2 | Lino/cobre, cebada/cuarzo, arenisca, arroz/tomate/miel, estaño tardío/ocre/totora; materiales refinados de castor |
| 3 | 13-18 | mezquite / pavo | C/3 | Plumas/maíz, algodón/hierro/frijoles, piedra/cultivos, turquesa/soja, azúcar/chile/semillas oleaginosas, plata/menta/agave; bronce utiliza estaño anterior |
| 4 | 19-24 | Roble / jabalí | D/4 | Acero, mineral de oro, mármol/basalto, gemas seleccionadas, tela/cuero fino de rango activa, alimentos/contenedores avanzados |

Introduzca los recursos de cada bloque gradualmente en seis regiones en lugar de entregar todo el catálogo en el primer campamento. Acceda a materiales sueltos más antiguos volviendo a visitar sus regiones o tiendas descubiertas; La recolección posterior puede proporcionar solo resultados/superposiciones de interacción explícitamente permitidos, no un cuarto espacio de recolección libre. Las recetas/abundancia local proporcionan progresión sin que otra progresión de madera/animal clasifique cada región.

**Nombres confirmados por temática:** región/campamento describen terreno, clima y lugares locales, independientemente del rango de recursos. Catálogo localizado propuesto de 24 nombres:

| Rango | Pares región/nombre |
| --- | --- |
| 1 | 1 Llanuras del Alba; 2 Colinas del Viento; 3 Bosque de Bruma; 4 Valle de Helechos; 5 Montañas de las Nubes; 6 Delta de los Juncos |
| 2 | 7 Marisma Susurrante; 8 Riberas Ámbar; 9 Cuenca Azul; 10 Altiplano Soleado; 11 Lagos Espejados; 12 Taiga del Velo Helado |
| 3 | 13 Praderas Doradas; 14 Cañones de Ascua; 15 Desierto de Dunas Rojas; 16 Oasis Oculto; 17 Meseta del Vendaval; 18 Tierras Altas del Deshielo |
| 4 | 19 Sabana del Ocaso; 20 Acantilados de Tormenta; 21 Playas de la Media Luna; 22 Costa de la Selva; 23 Estuario de Aguas Plateadas; 24 Litoral de Coral |
La asentamiento

permanece entre las regiones **4 y 5**, no entre los rangos de recursos cuatro y cinco. Por lo tanto, la primera casa debe utilizar pino/materiales de rango uno, no roble o arce diferido. Un río/peaje todavía separa regiones individuales, incluidas regiones adyacentes en el mismo rango de recursos.

Cuatro bandas de peces todavía se asignan a los rangos de herramientas/recursos 1-4. Ampliar el **catálogo completo propuesto** a cuatro especies por rango y utilizar las seis parejas únicas por bloque de la sección 6.2.5; no agregue dos nuevas especies para cada región. Un par regional es la unión de los resultados de sus nodos pesqueros, no una afirmación de que ambas especies compartan un estanque. Preserva nodos separados de agua dulce, costeros y de aguas abiertas y las reglas de botín de élite/pila completa. Los peces siguen siendo productos de interacción, nunca un cuarto contó con un recurso suelto.

Comience con un segmento representativo por rango. El último diseño reemplaza el antiguo modelo de región de tres segmentos y cuatro campamentos con 10 a 14 campamentos en las primeras regiones, y crecerá hasta llegar a 24 más adelante. El diseño completo propuesto a continuación tiene 437 campamentos/413 segmentos de senderos internos; trátelos como objetivos de datos/contenido, nunca escenas cargadas simultáneamente o un requisito previo para las pruebas de mecánica. Reutilice los kits de entorno/campamento y transmita una pequeña cantidad de segmentos. El orden de las misiones es el nuevo gráfico de requisitos previos de cuatro ramas, no una fase de herramienta fija.

### 6.2.1 Rangos pospuestos y límites de contenido

Véase [Parte 2: Progresión posterior al rango 4](PROJECT_RECOMENDATIONS_PART_2_ES.md) para el catálogo consolidado de expansión pendiente.

| Clasificación de progresión pendiente | Madera reservada / animal salvaje | Estado |
| --- | --- | --- |
| 5 | Arce / ciervo | Sólo trabajos pendientes; sin asignación de región ni receta requerida |
| 6 | Cedro / cabra montés | Sólo trabajos pendientes |
| 7 | Secoya / búfalo | Sólo trabajos pendientes |
| 8 | Ébano / oso | Sólo trabajos pendientes |

No completes el botín de tiempo de ejecución, las herramientas, las misiones, las tiendas, las cajas misteriosas, las recompensas de mascotas o los costos de la casa de estos rangos en la versión actual. Mantener sus ideas en un trabajo pendiente de diseño; Ahora no se necesitan artes/recetas/clases de expansión. No reserve los límites de velocidad/corazones/bolsillo/gancho actuales para esos rangos futuros. Las identificaciones existentes en una prueba guardada deben migrarse o ponerse en cuarentena de forma segura si el contenido está deshabilitado, nunca reinterpretarse silenciosamente como otra especie.

### 6.2.2 Tres recursos sueltos destacados por región

**Interpretación confirmada:** seis regiones x tres recursos destacados = **18 ID distintos de artículos de recolección suelta por rango**, incluida su rama y entrada de piel/pieles. No son 18 más esos dos. El conjunto de tres elementos de cada región es diferente, sin elementos sueltos destacados repetidos dentro de su rango. El borrador a continuación también utiliza 72 identificaciones distintas en los cuatro rangos. Las recetas existentes deben utilizar estos ID con nombre exacto, no materiales intercambiables genéricos.

Cada región contiene por separado su árbol de rango actual, animal pasivo y elementos interactivos de pesca adecuados. Troncos, carne cruda, pescado, grasa, huesos, huevos/savia y otras recompensas solo de interacción no ocupan un espacio suelto destacado. Rama + piel son coincidencias base; se permiten coincidencias alimentarias de árbol explícitas (7.1.1); **La segunda superposición del rango tres es la pluma de pavo**, confirmada explícitamente en lugar de una piel de pavo inexistente. La piel de jabalí es el equivalente de la familia de pieles del rango cuatro. El ingrediente alimentario de árbol es obligatorio (7.1.1). Se permite doble fuente TreeHarvest/LooseGathering explícita con el mismo ID, sin añadir espacios sueltos ni otras coincidencias animales.

**Campamentos por región (confirmado el 31 de octubre de 2026):** nivel uno 10-14 campamentos por región, nivel dos 12-16, nivel tres 14-18, nivel cuatro 16-20. La cantidad de cada región es aleatoria dentro del rango de su nivel, no creciente (por ejemplo 13, 10, 11, 14). Las cantidades de la tabla se sortearon una vez y se mantienen para planificar los mapas de las regiones; Llanuras del Alba ya tiene 10. **Después del rango cuatro el número no aumenta (usuario, 5 de noviembre de 2026):** los rangos 5-8 también usan 16-20 campamentos por región ([Parte 2](PROJECT_RECOMENDATIONS_PART_2_ES.md), sección 7).

| Región | rango | Tres ID de artículos sueltos destacados, mostrados por nombre para mostrar | Usos previstos de la receta | Campamentos propuestos |
| --- | --- | --- | --- | --- |
| 1 Llanuras del Alba | 1 | Ramas de pino; Piel de conejo; Manzanas | Troncos/herramientas de pino; resortera/juguete; jugo de manzana | 10 |
| 2 Colinas del Viento | 1 | Fibra de cáñamo; Pedernal; Piñas de pino | tela de cáñamo; hacha de pino; adorno de piña de pino | 12 |
| 3 Bosque de Bruma | 1 | Fibra de ortiga; Arcilla; Sal de roca | Hilo/relleno; ladrillos/tazas; curado/alimentos | 11 |
| 4 Valle de Helechos | 1 | Caliza; Fresas; Hierbas de té | Azulejos; bebida de yogur; té | 13 |
| 5 Montañas de las Nubes | 1 | Avena; Guijarros; Bayas rojas | Tortas de avena; amuleto de guijarros pulidos; pasteles de bayas | 10 |
| 6 Delta de los Juncos | 1 | Cañas; Trigo; Arena | Trenza de caña; harina de trigo; mortero de mejora | 14 |
| 7 Marisma Susurrante | 2 | Ramas de abedul; Piel de castor; Moras | Troncos de abedul; cuero/juguete de castor; jugo de mora | 12 |
| 8 Riberas Ámbar | 2 | Fibra de lino; Mineral de cobre; Zanahorias | Tela de lino; lingotes/accesorios de cobre; sopa de zanahoria | 13 |
| 9 Cuenca Azul | 2 | Cebada; Cuarzo; Arándanos | harina/sopa de cebada; vidrio/cuarzo tallado; mermelada de arándanos | 13 |
| 10 Altiplano Soleado | 2 | Arenisca; Trufa; Semillas de lino | Azulejos; platos de trufa; aceite de lino | 15 |
| 11 Lagos Espejados | 2 | Arroz; Tomates; Miel | Peceras; guiso; bebidas de miel | 13 |
| 12 Taiga del Velo Helado | 2 | Mineral de estaño; Ocre amarillo; Fibra de totora | Bronce; talla de abedul pintada; cordón de totora/amuleto de pesca | 13 |
| 13 Praderas Doradas | 3 | ramas de mezquite; plumas de pavo; Maíz | Troncos de mezquite; prenda/juguete de plumas; harina de maíz | 14 |
| 14 Cañones de Ascua | 3 | Fibra de algodón; Mineral de hierro; Frijoles | Tela de algodón; herramientas de hierro; guiso/chile | 18 |
| 15 Desierto de Dunas Rojas | 3 | Granito; garbanzos; Calabazas | Azulejos; guiso de garbanzos; gachas de avena | 18 |
| 16 Oasis Oculto | 3 | Pizarra; Turquesa; Soja | Azulejos; joyas de turquesa; leche de soja | 14 |
| 17 Meseta del Vendaval | 3 | Caña de azúcar; Chile verde; Semillas de girasol | Azúcar; chile de pavo; aceite de girasol | 16 |
| 18 Tierras Altas del Deshielo | 3 | Mineral de plata; Menta salvaje; Fibra de agave | Joyas de plata; té de menta; cordón de agave/amuleto del desierto | 15 |
| 19 Sabana del Ocaso | 4 | Ramas de roble; Piel de jabalí; Patatas | Troncos de roble; cuero de jabalí; guiso | 16 |
| 20 Acantilados de Tormenta | 4 | Mármol; Mineral de oro; Granos de cacao | Azulejos; joyas de oro; cacao en polvo/bebidas | 17 |
| 21 Playas de la Media Luna | 4 | Basalto; Esmeralda; Granos de café | Piedra pulida; joyas de esmeraldas; café | 20 |
| 22 Costa de la Selva | 4 | Obsidiana; Rubí; Vainas de vainilla | Colgante de obsidiana; joyas de rubíes; bebida de vainilla | 19 |
| 23 Estuario de Aguas Plateadas | 4 | Zafiro; lapislázuli; Algas | Colgante de zafiro; pulsera de lapislázuli; paquete de arroz con algas | 17 |
| 24 Litoral de Coral | 4 | Diamante; Cocos; Fibra de bambú | Collar de diamantes; bebida de coco; tela de bambú/camisa costera | 19 |

Los recursos destacados son la tabla de generación libre normal para esa región; sus pesos pueden diferir entre segmentos. Los materiales más antiguos pueden permanecer disponibles al volver a visitar regiones, los artículos ofrecidos en los campamentos, el inventario de asentamientos descubierto y las coincidencias base y las alimentarias de árbol explícitas (7.1.1). No agregue casualmente un cuarto tipo suelto ni repita identificaciones antiguas destacadas en mapas posteriores. Las tarjetas de mapa muestran exactamente los tres íconos de recursos en esta tabla, por separado de los indicadores de árbol/animal/pesca, además del estado bloqueado/descubierto. Un punto de pesca puede utilizar las especies válidas de su rango local sin convertir al pez en un cuarto recurso destacado.

Los nombres son una propuesta escrita, no todos los activos de producción nuevos se construirán de inmediato. Cada elemento tiene un ejemplo de uso inicial útil; preservar ese requisito antes de finalizar un espacio. Si se elimina un material, actualice los íconos de la región, las misiones, las tablas de botín, los filtros de la tienda y las recetas exactas juntos en lugar de dejar un objeto coleccionable decorativo/inútil.

**Propuesta de origen de trufa:** `ingredient.truffle` (nombre Trufa), rango material dos, es un ingrediente crudo apilable de cocina, no un consumible listo. Es uno de los tres recursos sueltos normales de región 10, con peso de aparición propuesto de 15% (Arenisca 45%, Semillas de lino 40%). No exige herramienta, cerdo, nueva mecánica de excavación ni ingrediente comprado. Recoger una por objeto en Release y 1,000 en MechanicsTest. Cerdo y otras mascotas elegibles solo pueden aportarla tras descubrir región 10. Usar el mismo ID exacto; no sustituir por hongo común.

Para conservar tres por región/18 por rango/72 totales, se propone una **reasignación de origen**: Lavanda sale de la tabla suelta de región 10 y pasa a hierba Q5 por efectivo del catálogo de granja, desbloqueada al descubrir región 10. Mantener ID/rango existentes y receta de Bolsa de lavanda; actualizar etiquetas y futuras solicitudes opcionales a ese catálogo. Lavanda no entra en tablas de materiales sueltos de mascotas. Validadores e iconos de región 10 sustituyen Lavanda por Trufa juntos. Trufa sigue como ingrediente regional principal, no excepción global de despensa; sus recetas tienen rango de desbloqueo dos o tres.

### 6.2.3 Etiquetas de origen y propuestas adicionales de recetas

Utilice fuentes independientes `LooseGathering`, `TreeHarvest`, `AnimalHarvest`, `Fishing`, `CraftOnly` y `Store`. El mismo ID de artículo puede tener más de una fuente sólo bajo reglas explícitas; rama + piel/piel/pluma confirmada son coincidencias base; 7.1.1 permite además ingredientes de árbol con doble fuente explícita. Un artículo elaborado se puede comprar sin que se pierda y se pueda recolectar. Los troncos de pino son TreeHarvest y se pueden fabricar, por lo que la conversión es instantánea; las manzanas son recolección suelta, no una recompensa por cortar un árbol. La sal de roca es el ID de sal exacto que ya se utiliza en las recetas. Piedras de pedernal significa el ID de pedernal, y piel de conejo/piel de conejo cruda se refieren al mismo `animal.rabbit.pelt` sin procesar, no a ID de ingredientes alternativos. Normalice estas descripciones de visualización de forma coherente al crear. El Berry genérico se convierte en la identificación exacta de los frutos rojos; La hierba genérica se convierte en hierbas de té cuando se especifique. Estas aclaraciones de nombres nunca permiten la sustitución entre diferentes variedades.

Útiles **ejemplos iniciales de recetas de entrada fija**, para desbloquear a través de las cuatro ramas (no todas disponibles desde el principio):

| Salida | Entradas exactas -> rendimiento | Regla/uso |
| --- | --- | --- |
| Jugo de manzana | 3 manzanas -> 1 | Receta de comida temprana, temporizador de nivel uno de 3 segundos propuesto, restaura 1 corazón |
| Adorno de piña de pino | 2 Piñas de pino + 1 Rama de pino -> 1 | Varios apilables; da uso de venta a las piñas |
| Trenza de caña | 3 Cañas -> 1 | Material; entrada de mejora de la casa |
| Mortero | 2 Arena + 1 Caliza -> 1 | Material; entrada para mejorar la casa, sin elaboración de muebles |
| Jugo de mora | 3 Moras -> 1 | Receta/producción/dominio de alimentos separados del jugo de manzana |
| Tela de lino | 4 Fibra de lino -> 2 | Receta de material fijo separado |
| Sopa de zanahoria | 3 Zanahorias + 1 Sal de roca -> 1 | Comida |
| Mermelada de arándanos | 3 Arándanos -> 1 | Comida |
| Bolsa de lavanda | 2 Lavanda + 1 Tela de cáñamo -> 1 | Varios |
| Aceite de lino | 3 Semillas de lino -> 1 | Material ingrediente, temporizador fijo |
| Pájaro de abedul pintado | 2 tablas de abedul + 1 ocre amarillo -> 1 | Varios |
| Cordón de totora | 3 Fibra de totora -> 1 | Materiales |
| Amuleto de pesca | 1 Cordón de totora + 1 Accesorio de cobre -> 1 | Varios apilables; la espadaña tiene un uso de consumo de recursos fuera de las mejoras Élite |
| Guiso de garbanzos | 3 Garbanzos + 1 Sal de roca -> 1 | Comida |
| Chile de pavo | 2 Carne cruda de pavo + 2 Frijoles + 1 Chile verde -> 1 | Comida |
| Aceite de girasol | 3 Semillas de girasol -> 1 | Material/producción diferente del aceite de lino; sin sustitución |
| Té de menta | 2 Menta silvestre + 1 Agua -> 1 | Comida |
| Cordón de agave | 3 Fibra de agave -> 1 | Materiales |
| Amuleto del desierto | 1 Cordón de Agave + 1 Turquesa -> 1 | Varios apilables; agave tiene un uso de consumo de recursos fuera de las mejoras Élite |
| Colgante de plata | 1 Lingote de plata + 1 Cordón de lino -> 1 | rango tres Misceláneos, entradas de los rangos tres/dos |
| Cacao en polvo | 2 granos de cacao -> 2 | Materiales |
| Café tostado | 2 Granos de café + 1 Rama de roble -> 2 | rango cuatro Ingrediente material; combustible para asar explícito |
| Talla esmeralda/rubí/zafiro/diamante | Cada uno tiene su propia receta fija: 2 gemas en bruto con nombre -> 1 gema tallada con el mismo nombre | Cuatro definiciones de materiales separadas; receta de tiempo de ejecución sin comodines |
| Lapislázuli pulido | 2 Lapislázuli -> 1 | Material de rango cuatro (6 de noviembre de 2026). El lapislázuli es opaco, así que se pule en lugar de tallarse. Lo usa el joyero premium de lapislázuli; la pulsera ordinaria de lapislázuli conserva las cuentas en bruto |
| Anillo esmeralda | 1 Esmeralda tallada + 1 Lingote de oro -> 1 | Varios |
| Anillo de rubí | 1 Rubí tallado + 1 Lingote de oro -> 1 | Varios |
| Colgante de zafiro | 1 Zafiro tallado + 1 Lingote de plata -> 1 | Varios |
| Pulsera de lapislázuli | 2 Lapislázuli + 1 Cordón de algodón -> 1 | rango cuatro Miscelánea usando cable de rango tres |
| Colgante de obsidiana | 2 Obsidiana + 1 Cordón de Algodón -> 1 | rango cuatro Miscelánea usando cable de rango tres |
| Paquete de arroz con algas | 2 Algas + 2 Arroces + 1 Atún -> 1 | Comida |
| Collar de diamantes | 1 Diamante tallado + 1 Lingote de Oro + 1 Cordón de algodón -> 1 | Rango cuatro Miscelánea usando solo los rangos cuatro/tres |
| Bebida de coco | 2 Cocos + 1 Botella de cristal -> 1 | Comida |
| Tela de bambú | 4 Fibra de bambú -> 2 | Materiales |
| Camisa costera | 4 Tela de bambú + 2 Cuero refinado de jabalí -> 1 | rango cuatro segundo conjunto opcional Usable, máximo diez bolsillos |

El aceite de lino y el de girasol tienen usos distintos: las galletas de aceite de semillas usan 2 Harinas de trigo + 1 Aceite de lino; las tortas de avena con chile usan 2 Avenas + 1 Aceite de girasol + 1 Chile verde. Nunca aceptar un aceite intercambiable. La tabla propuesta de mejoras de casa usa cuatro Trenzas de caña para el nivel dos y seis Morteros para el tres; sus desbloqueos, con materiales de la región seis, se proponen en las misiones posteriores. Las cantidades siguen siendo datos de prueba; los materiales no tienen dominio y los muebles se compran exclusivamente en tienda.

### 6.2.4 Distancia entre campamentos y límite de 26 corazones

Contar los campamentos, incluidos los campamentos de entrada/salida de una región; Las secciones internas del sendero para caminar son `campCount - 1`. Las transiciones en ferry no añaden costo de caminata. Los recuentos propuestos anteriormente son de 10 a 14 en las regiones iniciales, de 18 a 20 cerca del final del segundo rango y de 20 a 24 en los rangos posteriores. Inicialmente, cree solo cadenas de prueba cortas y representativas.

Para cada sección del sendero, cree la distancia mundial `D` y un `drainHeartsPerMeter` fijo, luego calcule `requiredHearts = D * drainHeartsPerMeter`. Este presupuesto es independiente de la entrada de la cámara, el tiempo que pasa inmóvil, las cantidades transportadas, el equipo actual o la resistencia actual. Los sombreros elevan la tapa del medidor, las botas reducen el tiempo transcurrido de caminata y la comida agrega resistencia utilizable durante la sección del sendero; Ninguno cambia silenciosamente el costo de caminata de su autor.

**Máximo confirmado: se requiere el rango más alto Corazones <= 26 por sección del sendero.** Mantenga el límite en doce corazones. En un inicio completo de doce corazones, una sección de sendero de 26 corazones requiere al menos catorce corazones adicionales de alimentos/bebidas. Tres porciones normales de seis corazones suministran dieciocho, lo que permite un acabado de cuatro corazones si se consumen sin desperdicio; No se requiere comida dorada premium. Si comienza por debajo de su resistencia máxima, el presupuesto para alimentos debe aumentar en consecuencia.

Datos de ritmo de juego propuestos (los rangos son ejemplos, no mediciones de nivel implementadas):

| Progresión | Distancia típica de la sección del sendero | Costo de caminata por autor | Intención |
| --- | --- | --- | --- |
| Campamentos de apertura de la Región 1 | 100-250 metros | 0.2-0.5 corazones a 2 corazones/km | A salvo con tres corazones iniciales y sin comida |
| Resto de la región 1 | 150-400m | Como máximo 1.5 corazones | Obtén manzanas, termina Recolector G1/G2, elabora jugo antes de la región 2 |
| Regiones 2-6 | 350-600m | 3.5-6 corazones | Cada vez más distancia, primera planificación alimentaria obligatoria |
| Regiones 7-12 | 500-900 metros | 6.5-12 corazones | Más alimentos y mayor acceso a los sombreros |
| Regiones 13-18 | 750-1,200 m | 10-20 corazones | Múltiples comidas en tramos de senderos más largos |
| Regiones 19-24 | 1,000-1,600 m | 14-26 corazones, nunca más de 26 | La comida durante el viaje sigue siendo necesaria incluso con doce corazones |

Derive la tasa de drenaje de cada sección del sendero a partir de la distancia y el presupuesto elegidos; Nunca combine la distancia/velocidad máxima a ciegas. Ejemplo de sección de sendero con el rango más alto: 1,300 m a 20 corazones/km = **26 corazones**. A 1.6 m/s, se necesitan aproximadamente 13 minutos y 33 segundos de caminata antes de que se detenga la recolección. Mantener un ritmo universal de 2 corazones/km requeriría 13 km/alrededor de 2 horas y 15 minutos por el mismo costo; Por lo tanto, las tasas de drenaje regionales variables creadas son una **recomendación** para el ritmo de juego en móvil, mientras que se confirma el límite de 26 corazones.

Proporciona una vista previa de salida con los corazones requeridos, los corazones actuales/máximos y la recuperación de alimentos listos **utilizables**, incluyendo la comida pendiente conservada y simulando los tiempos de recuperación frente al gasto al caminar; el excedente al alcanzar el máximo permanece pendiente en vez de desperdiciarse. Advertir si no se dispone de suficientes provisiones; no bloquee de forma invisible la salida ni compre alimentos automáticamente. Garantizar manzanas de la primera región antes de la entrega de recetas y reclamaciones de campamentos G1/G2 antes de la salida. La regeneración del campamento debe permitir una siguiente sección de apertura segura del sendero incluso si se omiten los comercios/menús. Simule tramos de recorrido inicial contra tres corazones y tramos de recorrido avanzados contra 26, incluidas salidas parciales y programación de comidas de seis corazones sin desperdicio. La capacidad máxima del sombrero sigue siendo de doce, incluso cuando el consumo acumulado por sección del sendero llega a 26.

### 6.2.5 Parejas de peces por región, sin repetir parejas dentro de un rango

**Distribución recomendada:** cuatro especies de peces por rango dan exactamente seis pares desordenados (`4 choose 2`), uno para cada región. Conserve los ocho originales y agregue ocho, en lugar de crear 48 peces únicos. Las adiciones/parejas de especies a continuación son propuestas de diseño; Se mantienen fijos cuatro rangos de herramientas, pesca en todas partes, recetas exactas y tres íconos de recursos sueltos por región.

| Rango de progresión de peces | Catálogo, con nuevas especies marcadas | Rango de progresión de herramientas de pesca |
| --- | --- | --- |
| 1 | Trucha arcoiris; Lobina negra; **Mojarra azul (nuevo)**; **Perca amarilla (nueva)** | 1 |
| 2 | carpa común; lucio del norte; **Bagre de canal (nuevo)**; **Lucioperca americana (nueva)** | 2 |
| 3 | Salmón; Esturión; **Trucha marrón (nueva)**; **Trucha de lago (nueva)** | 3 |
| 4 | Pejelagarto de hocico largo; Atún; **Corvina roja (nueva)**; **Huachinango rojo (nuevo)** | 4 |

| Región | Par de peces exacto | Presentación del nodo creado |
| --- | --- | --- |
| 1 Llanuras del Alba | Trucha arcoiris + Mojarra azul | Nodos de arroyo/poza |
| 2 Colinas del Viento | Lobina negra + Perca amarilla | Nodos de lago/estanque |
| 3 Bosque de Bruma | Trucha arcoíris + Lobina negra | Nodos de arroyos/lagos separados |
| 4 Valle de Helechos | Mojarra azul + Perca amarilla | Nodos de estanque/lago |
| 5 Montañas de las Nubes | Trucha arco iris + Perca amarilla | Nodos de arroyos/lagos |
| 6 Delta de los Juncos | Lobina negra + Mojarra azul | Nodos de remanso/poza |
| 7 Marisma Susurrante | Carpa común + Bagre de canal | Nodos de aguas lentas |
| 8 Riberas Ámbar | Lucioperca americana + Lucioperca americana | Nodos de ríos/lagos |
| 9 Cuenca Azul | Carpa común + lucio del norte | Nodos de humedales/lagos |
| 10 Altiplano Soleado | Bagre de canal + Lucioperca americana | Nodos de embalse/río |
| 11 Lagos Espejados | Carpa común + Lucioperca americana | Nodos del lago |
| 12 Taiga del Velo Helado | Lucio del norte + Bagre de canal | Nodos separados de lagos y ríos |
| 13 Praderas Doradas | Salmón + Trucha Marrón | Autor alcance del río de agua fría |
| 14 Cañones de Ascua | Esturión + Trucha de lago | Nodos de ríos/embalses profundos |
| 15 Desierto de Dunas Rojas | Salmón + Esturión | Alcance del río, no un charco del desierto |
| 16 Oasis Oculto | Trucha marrón + Trucha de lago | Nodos lacustres fríos/profundos |
| 17 Meseta del Vendaval | Salmón + Trucha de lago | Nodos de arroyos/embalses |
| 18 Tierras Altas del Deshielo | Esturión + Trucha Marrón | Nodos fluviales/afluentes separados |
| 19 Sabana del Ocaso | Pejelagarto de hocico largo + Corvina roja | Acceso separado a agua dulce/estuario |
| 20 Acantilados de Tormenta | Pejelagarto de hocico largo + Huachinango rojo | Aguas continentales más una parada de pesca en la costa baja de autor |
| 21 Playas de la Media Luna | Corvina roja + Huachinango rojo | Acceso a estuarios/pesca costera |
| 22 Costa de la Selva | Pejelagarto de hocico largo + Atún | Muelle separado de agua dulce y de costa abierta |
| 23 Estuario de Aguas Plateadas | Corvina roja + Atún | Estuario más presentación en aguas abiertas |
| 24 Litoral de Coral | Huachinango rojo + Atún | Paradas de pesca costera/en aguas abiertas |

Estos son objetivos de creación de rutas/hábitats de caza, no garantías de convivencia biológica. En particular, Corvina roja está asociada con aguas costeras/estuarinas y Huachinango rojo con hábitat marino; No coloque a la pareja en un estanque tierra adentro. El arte de la ruta de rango cuatro debe incluir el acceso a la costa listado, incluso cuando la región más amplia sea bosque/tierra alta. Mantenga los peces más profundos/de alta mar como presentación en el muelle/punto de lanzamiento en lugar de un nuevo sistema de jugador-barco. Referencias de hábitat: [NC DEQ Corvina roja](https://www.deq.nc.gov/about/divisions/marine-fisheries/public-information-and-education/species-profiles/red-drum), [NOAA Huachinango rojo](https://www.fisheries.noaa.gov/species/red-snapper). Las presentaciones de agua fría/río para el rango tres son afluentes/embalses escritos, no una afirmación de que todo el bioma de mezquite tenga agua fría.

Comience con algunas vistas de peces/marcadores de posición; cada especie a la que hace referencia un par regional habilitado aún necesita una definición de elemento válida antes de que ese par se active. Agregue arte/regiones restantes de forma incremental. Cada pescado tiene un ID exacto/asado con nombre en la sección 9.4.1, entrada de pescado nunca intercambiable. Los pesos/nodos regionales pueden diferir, pero ambas especies necesitan fuentes accesibles antes de que una misión local requerida las solicite. La pescadería ofrece únicamente especies descubiertas, siempre Q5. Mantenga tres íconos sueltos y muestre el par en un panel de detalles de pesca separado.

### 6.2.6 Temas de paisaje y nombres de campamentos

**Confirmado:** los nombres ya no repiten Pino/Abedul/Mezquite/Roble para indicar rango. Separar rango, orígenes, parejas de peces, cantidades de campamentos y progresión de los nombres visibles. Nieve/desierto/costa son temas visuales, no nuevos peligros de frío/sed ni rangos. Conservar nodos acuáticos y zonas protegidas de cultivo; región 18 combina cumbres nevadas y huertos de estribación cálida; región 20 mantiene agua interior y acceso costero.

| Región | Tema del mapa | Campamentos 1, 2 y final (ejemplos de la regla completa) |
| --- | --- | --- |
| 1 Llanuras del Alba | Llanuras templadas abiertas | Refugio de la Pradera del Alba; Refugio de la Pradera del Silencio; Refugio de la Pradera del Descenso |
| 2 Colinas del Viento | Colinas onduladas ventosas | Refugio de la Colina del Alba; Refugio de la Colina del Silencio; Refugio de la Colina del Descenso |
| 3 Bosque de Bruma | Bosque templado con niebla | Refugio de la Arboleda del Alba; Refugio de la Arboleda del Silencio; Refugio de la Arboleda de la Lejanía |
| 4 Valle de Helechos | Valle verde y laderas de helechos | Refugio de la Hondonada del Alba; Refugio de la Hondonada del Silencio; Refugio de la Hondonada del Resplandor |
| 5 Montañas de las Nubes | Pasos montañosos rocosos | Refugio de la Cresta del Alba; Refugio de la Cresta del Silencio; Refugio de la Cresta de la Calma |
| 6 Delta de los Juncos | Delta e islas de juncos | Refugio del Canal del Alba; Refugio del Canal del Silencio; Refugio del Canal de la Sombra |
| 7 Marisma Susurrante | Humedales y pasarelas | Refugio de la Marisma del Alba; Refugio de la Marisma del Silencio; Refugio de la Marisma del Resplandor |
| 8 Riberas Ámbar | Terrazas fluviales cálidas | Refugio de la Ribera del Alba; Refugio de la Ribera del Silencio; Refugio de la Ribera de la Calma |
| 9 Cuenca Azul | Cuenca baja y estanques | Refugio de la Cuenca del Alba; Refugio de la Cuenca del Silencio; Refugio de la Cuenca de la Sombra |
| 10 Altiplano Soleado | Altiplano soleado y hondonadas boscosas | Refugio del Otero del Alba; Refugio del Otero del Silencio; Refugio del Otero de la Sombra |
| 11 Lagos Espejados | Lagos enlazados e islas | Refugio del Lago del Alba; Refugio del Lago del Silencio; Refugio del Lago del Sur |
| 12 Taiga del Velo Helado | Taiga nevada y orillas descongeladas | Refugio de la Taiga del Alba; Refugio de la Taiga del Silencio; Refugio de la Taiga de la Lluvia |
| 13 Praderas Doradas | Praderas secas doradas | Refugio del Pastizal del Alba; Refugio del Pastizal del Silencio; Refugio del Pastizal de la Lluvia |
| 14 Cañones de Ascua | Cañones rojos y río profundo | Refugio del Cañón del Alba; Refugio del Cañón del Silencio; Refugio del Cañón de la Lluvia |
| 15 Desierto de Dunas Rojas | Dunas y corredor fluvial desértico | Refugio de la Duna del Alba; Refugio de la Duna del Silencio; Refugio de la Duna de los Meandros |
| 16 Oasis Oculto | Manantiales desérticos y lagos fríos profundos | Refugio del Manantial del Alba; Refugio del Manantial del Silencio; Refugio del Manantial de los Meandros |
| 17 Meseta del Vendaval | Meseta seca y embalse | Refugio de la Meseta del Alba; Refugio de la Meseta del Silencio; Refugio de la Meseta del Crepúsculo |
| 18 Tierras Altas del Deshielo | Cumbres nevadas y estribaciones de deshielo | Refugio del Nevero del Alba; Refugio del Nevero del Silencio; Refugio del Nevero del Crepúsculo |
| 19 Sabana del Ocaso | Sabana y borde de estuario | Refugio de la Aguada del Alba; Refugio de la Aguada del Silencio; Refugio de la Aguada del Crepúsculo |
| 20 Acantilados de Tormenta | Acantilados costeros y arroyos interiores | Refugio del Acantilado del Alba; Refugio del Acantilado del Silencio; Refugio del Acantilado del Crepúsculo |
| 21 Playas de la Media Luna | Calas arenosas y playas de marea | Refugio de la Cala del Alba; Refugio de la Cala del Silencio; Refugio de la Cala del Crepúsculo |
| 22 Costa de la Selva | Selva con muelle hacia mar abierto | Refugio de la Selva del Alba; Refugio de la Selva del Silencio; Refugio de la Selva del Crepúsculo |
| 23 Estuario de Aguas Plateadas | Canales de estuario y miradores marinos | Refugio del Estuario del Alba; Refugio del Estuario del Silencio; Refugio del Estuario del Crepúsculo |
| 24 Litoral de Coral | Playas tropicales y vistas de coral | Refugio de la Costa del Alba; Refugio de la Costa del Silencio; Refugio de la Costa del Crepúsculo |

**Regla completa de nombres:** cada campamento combina su lugar regional con esta lista ordenada de sufijos, tomando solo los primeros `campCount`: del Alba, del Silencio, del Viento, del Sol, del Escondite, del Viejo Sendero, de la Luna, del Abrigo, de las Alturas, del Descenso, de la Lejanía, del Resplandor, de la Calma, de la Sombra, del Este, del Oeste, del Norte, del Sur, de las Estrellas, de la Lluvia, de la Soledad, de los Meandros, del Último Paso, del Crepúsculo. Formar `Refugio de [lugar regional] [sufijo]`, contrayendo `de el` a `del`; arriba se muestran ordinales 1, 2 y final. Lugares regionales en orden: la Pradera, la Colina, la Arboleda, la Hondonada, la Cresta, el Canal, la Marisma, la Ribera, la Cuenca, el Otero, el Lago, la Taiga, el Pastizal, el Cañón, la Duna, el Manantial, la Meseta, el Nevero, la Aguada, el Acantilado, la Cala, la Selva, el Estuario, la Costa. Así cada campamento existente (10-24 por región) recibe nombre fijo con variaciones locales, sin prefijo de rango ni sorteo por visita. Mostrar ordinal como etiqueta secundaria. Puede pulirse gramática conservando CampId estable.

Los nombres son campos localizados visibles. Conservar ID regional/campamento, descubrimientos, peajes, lastTrailCampId, posiciones y orígenes. Incluso `region.pine_meadows` permanece estable mientras su nombre se muestra como Llanuras del Alba. No deducir rango/bioma/recursos del texto. Migrar claves visibles/alias sin reiniciar desbloqueos. Encabezados, tablas, marcadores y vistas de viaje usan el mismo catálogo.

### 6.3 Catálogo de recursos y formas de obtención

| Familia | Materiales para conservar/añadir | Origen y finalidad de la progresión |
| --- | --- | --- |
| Madera | Activo: pino, abedul, mezquite, roble, cada uno con ramas/troncos/tablas | Árboles de senderos, cuatro filas de progresión de madera; herramientas, tallas, mejoras de la casa. Arce/cedro/secuoya/ébano se aplazan |
| Fibra | Ortiga, cáñamo, algodón, juncos; añadir lino e hilo | Recolección temprana, ganado agrícola seleccionado; cordajes, telas, juguetes |
| Textiles animales | Activo: pieles de conejo/castor, cuero de jabalí, lana de rancho opcional; cáñamo/algodón/tela fina | Aplazar las pieles de venado/cabra/búfalo, pieles de oso y cadenas de seda/cachemira a menos que se aprueben por separado como importaciones actuales |
| Carne | Salvaje: conejo, castor, pavo, jabalí. La carne de rancho nacional puede utilizar inicialmente una pequeña selección genérica | Se aplazan las recetas de venado, cabra montesa cazada, búfalo y oso; la leche de cabra doméstica no implica necesariamente la caza de cabras montesas |
| Subproductos | Activo: plumas de pavo, huevos, colmillos de jabalí, huesos, grasa animal | Aplazar la cornamenta de venado, los cuernos de caza de búfalo/cabra y la garra de oso hasta que existan sus rangos salvaje |
| Lechería/colmenar | Leche de vaca/cabra, nata, mantequilla, yogur, queso, cera de abejas, miel | Leche de rancho; procesamiento de lácteos; nodos de recolección/árboles melíferos asociados a las abejas, cuando corresponda |
| Cultivos | Grupo de candidatos: trigo, maíz, patatas, arroz, cebada, avena, fresas, tomates, frijoles, soja, calabazas, caña de azúcar, café, cacao, vainilla | Comience con algunos cultivos que respalden las recetas implementadas. Agregar importaciones tardías seleccionadas dentro del rango cuatro; esto no es un requisito para crear todos los cultivos ahora |
| Adiciones de alimentos | Bayas, champiñones, manzanas, nueces, sal, té de hierbas, agua, Trufa exacta de rango dos | Trufa se recoge en región 10 y sirve para tres platos (sección 9.4.3); sin catálogo obligatorio de levaduras/cultivos/enzimas; agua recargable en campamento |
| Metales | Minerales de cobre, hierro, plata, oro; añadir estaño y carbón/carbón vegetal | Rastrear el mineral suelto inicialmente, extraerlo luego; bronce/acero y joyería |
| Piedra/tierra | Caliza, arenisca, granito, mármol, basalto, pizarra, arcilla, obsidiana, cuarzo | Tallado, azulejos, cerámica, vidrio; deduplicar la entrada repetida de basalto |
| Gemas | Diamante, esmeralda, rubí, zafiro, turquesa, lapislázuli | Joyas tardías; ninguna herramienta ordinaria cerrada detrás del diamante |
| Ayudas tecnológicas | Carbón, cordón/hilo, cera de abejas, vidrio, clavos/accesorios | Mantenga un juego pequeño con varios usos; omita las subcadenas de tanino/cultivo/pegamento inicialmente. Los recipientes de vidrio/arcilla pueden admitir recetas avanzadas de alimentos/bebidas |

Normalice los nombres/ID y mantenga el concepto de garra de oso diferido como una entrada del trabajo pendiente. Trate la piel de conejo como insumo crudo y la piel/fieltro de conejo como productos procesados. Mantenga las identificaciones de cuero específicas de cada especie solo cuando sean útiles para las recetas. El mineral/lingote de oro siguen siendo materiales, nunca moneda de billetera. Evite recompensas duplicadas de pieles de un solo cuerpo a menos que se configuren deliberadamente.

Mantenga las ideas de recursos originales como un trabajo pendiente, no como un requisito para implementar cada cultivo/gema/importación de inmediato. Cree primero un subconjunto pequeño de región, luego solo los recursos necesarios para los cuatro rangos activos. Agregue cada material con una fuente y una receta útil/uso de consumo de recursos domésticos. Los materiales pueden convertirse en ingredientes, contenedores o mejoras para la casa; nunca se convierten en muebles creados por los jugadores. Los números de región más altos dentro de un rango pueden agregar recetas y patrones de escasez sin agregar otra especie o subcadena de procesamiento.

## 7. Recogida de pilas completas y límites del botín

**Diseño de recolección aplazado (no es un hito activo):** mantener `MechanicsTest` con 1,000 unidades por recogida apilable durante depuración/pruebas. El diseño anterior de `Release` transfiere un artículo por objeto recolectado, incluido el gesto rápido; los objetos por nodo y las bonificaciones Élite determinan su número, y la capacidad de pila sigue en 1,000. Ese diseño no cambia rendimientos de recetas, pilas de tiendas/cajas, cantidades de misión ni pilas guardadas. Conservarlo solo como referencia futura. No implementar/activar el cambio a lanzamiento, reajustar cantidades, dominio, costes de herramientas, cuotas de misión, provisión de comida, densidad o ritmo, ni exigir una partida nueva de lanzamiento salvo que un mensaje futuro del usuario solicite explícitamente esa fase. Las pruebas actuales registran defectos y mediciones con el perfil de pilas completas sin cambios.

### 7.1 Cantidad de lotes de botín frente a cantidad de unidades

**Predeterminado confirmado:** `WorldPickupStackQuantity = 1000` para cada recolección de senderos apilable, incluidos los recursos arrojados por la recolección. Mantén esto en la configuración principal de prueba de mecánica, no detrás de un interruptor de depuración que está desactivado de forma predeterminada.

| Evento | Número de paquetes | Unidades en cada paquete | Resultado |
| --- | --- | --- | --- |
| Recogida de recursos sueltos | 1 | 1,000 | Una pila completa, sujeta a desvío de misión y capacidad de fusión |
| Ramas de árboles normales | Aleatorio 2-5 | 1,000 | 2,000-5,000 ramas |
| Troncos de árboles normales | Aleatorio 2-5 | 1,000 | 2,000-5,000 unidades logarítmicas |
| Ingredientes adicionales del árbol | Un tipo obligatorio, segundo opcional; 2-4 paquetes/tipo | 1,000 | Alimento o savia específica de especie/bioma |
| Madera de árbol de herramientas Élite | 4-10 paquetes por cada salida de madera | 1,000 | Las recompensas completas permanecen sin cambios |
| Ingredientes del árbol de herramientas de élite | 4-6 paquetes/tipo | 1,000 | Un tipo obligatorio, máximo dos |
| Caza normal | 2-5 paquetes para cada producción seleccionada; grasa 1-3 | 1,000 | Utilice tablas de botín específicas para cada especie, no todos los subproductos posibles |
| Búsqueda de herramientas de élite | 4-10 paquetes de productos/producción; grasa 1-6 | 1,000 | Caso de alta carga para inventario espacial y colas de botín |
| Finalización de la pesca | 2-5 paquetes extraídos de la mesa de pescado local | 1,000 | Cada paquete contiene una especie de pez y una calidad |
| Pesca con herramientas de élite | Propuso 4-10 paquetes de la misma mesa de pescado local | 1,000 | Impulsar todos los objetivos elegibles; menos acciones en o por debajo del rango de herramientas, restricciones de especies sin cambios |
| Árbol enorme | Mismas reglas de generación de paquetes, multiplicadas por 50 | 1,000 | Recompensas en cola; nunca los renderices todos a la vez. **Tras el reequilibrio (31 de octubre de 2026): unos 450 objetos por árbol**, no 450.000 |
| Compra de NPC del campamento | 1 unidad para una oferta apilable | 1 | No es una recogida de senderos; herramientas/equipos son una instancia |
| compra de pila de asentamiento | 1 pila completa | 1,000 | Persistir el desbordamiento en una bandeja de entrega si procede |
| Caja misteriosa | 6 entradas de recompensa con tipo definido | 1,000 para apilables físicos | El equipo es individual; el desbloqueo de recetas es una concesión de conocimientos, no una pila |

Ejemplo: 3 pilas de ramas + 4 de troncos + 2 de ingrediente dan 9,000 unidades. El mismo resultado masivo da 150 + 200 + 100 pilas = 450,000 unidades. El ingrediente es obligatorio; sigue siendo prueba de carga deliberada.

Sortear las recompensas una sola vez cuando se completa la recolección. Conserve el manifiesto agregado resultante y un identificador de reclamo único antes de mostrar el botín. La recarga no debe cambiar la calidad, multiplicar las recompensas ni restablecer un nodo parcialmente cosechado.

### 7.1.1 Ingredientes alimentarios obligatorios del talado

**Revisión de recetas:** los cuatro ingredientes activos tienen usos exactos abajo. Piñones y Savia de abedul entran directamente en Food; Vainas de mezquite y Bellotas primero pasan por sus conversiones Material y luego a Food. No hace falta otro ID de receta para cubrirlos. Parte 2 sección 12 revisa los cuatro ingredientes futuros.

**Aceptación de ingredientes:** comprobar ID canónico y cantidad, independientemente de origen autorizado de árbol, recolección suelta o transferencia. Un ingrediente crudo recién recolectado está listo para receta; “crudo” no significa fabricación pendiente ni comida preparada automática. Aplicar comprobaciones normales de disponibilidad/reserva. Pilas de distinta calidad/perfil del mismo ID pueden aportar cantidades exactas según reglas normales; calidad del ingrediente no se hereda automáticamente al producto. Descubrimiento sigue limitando ofertas/aprendizaje aparte del consumo de objetos poseídos. Nunca sustituir savia de abedul por arce, vainas por harina de mezquite, bellotas por harina tostada ni savia de arce por jarabe. Mantener combustible exacto y escalar costes/rendimientos en lotes.

**Corrección confirmada:** cada interacción de talado completada produce ramas, troncos **y al menos un tipo comestible o ingrediente de cocina** de su tabla exacta. Aplica a árboles de sendero y árboles masivos. Puede configurarse un segundo ingrediente (máximo dos); una lista vacía es inválida. No sustituye madera por azar ni se concede por golpe o por descubrir un árbol. Guardar ingredientes y madera en el mismo manifiesto.

**Asignaciones propuestas para rangos activos:**

| Rango / árbol | Productos obligatorios | ID / estado del ingrediente | Receta Food exacta, desbloqueo no aleatorio |
| --- | --- | --- | --- |
| 1 / Pino | Ramas de pino + Troncos de pino + Piñones | `ingredient.pine_nuts`, ingrediente crudo | `recipe.food.toasted_pine_nuts`: 2 Piñones + 1 Rama de pino -> 1 Piñones tostados, 2 corazones totales |
| 2 / Abedul | Ramas de abedul + Troncos de abedul + Savia de abedul | `ingredient.birch_sap`, ingrediente crudo | `recipe.food.birch_sap_milk_drink`: 2 Savia de abedul + 1 Leche de vaca -> 1 Bebida de savia y leche, 3 corazones |
| 3 / Mezquite | Ramas de mezquite + Troncos de mezquite + Vainas de mezquite | `ingredient.mesquite_pods`, ingrediente crudo | `recipe.food.mesquite_pod_cakes`: 2 Harina de vainas de mezquite + 1 Mantequilla de vaca -> 2 Tortitas, 4 corazones por porción |
| 4 / Roble | Ramas de roble + Troncos de roble + Bellotas | `ingredient.acorns`, ingrediente crudo | `recipe.food.acorn_biscuits`: 1 Harina de bellota tostada + 1 Harina de trigo + 1 Leche de vaca -> 1 Galletas de bellota, 5 corazones |

**Conversiones exactas:** rango 3 `recipe.material.mesquite_flour`: 3 Vainas de mezquite -> 2 Harina de vainas de mezquite (`ingredient.mesquite_flour`); rango 4 `recipe.material.roasted_acorn_meal`: 2 Bellotas + 1 Rama de roble -> 2 Harina de bellota tostada (`ingredient.roasted_acorn_meal`). Sin sustituir harina/combustible. Son Material sin dominio, temporizador fijo propuesto de 10 segundos por lote; los productos no son recolectables directamente. IDs de comida coinciden con sufijo de receta bajo `item.`. Food conserva dominio/calidad, ausencia de durabilidad y recuperación progresiva; proponer base de 20 segundos por lote individual a dominio uno, con límites Food existentes. Desbloquear sin azar tras descubrir árbol y demás fuentes/conversiones/tiendas exactas; ubicación de tareas de rama pendiente. Son recetas normales adicionales, fuera de cantidades de panadería/cajas. Ingredientes crudos no son comida preparada ni se comen automáticamente al llegar a cero.

**Cantidades:** Normal entrega 2–5 pilas de ramas, 2–5 de troncos y 2–4 por ingrediente; Élite entrega 4–10 por producto de madera y 4–6 por ingrediente. Cada pila sigue con 1.000 unidades en MechanicsTest. Árbol masivo multiplica **todas las categorías** por 50 una vez tras sorteo Normal/Élite. Ejemplo: 3 ramas + 4 troncos + 2 ingredientes = 9.000 unidades; versión masiva 150 + 200 + 100 pilas = 450.000. Conservar vistas limitadas y pendientes persistentes.

**Fuentes y descubrimiento:** los cuatro ingredientes propuestos son exclusivos TreeHarvest en nodos descubiertos de su rango; no cambian 72 recursos sueltos ni añaden cuarto icono. Se permiten fuentes dobles explícitas (savia/frutas de Parte 2), usando el **mismo ID**. Para cada ingrediente requerido, basta una fuente autorizada descubierta y accesible; no exigir todas sus fuentes. Entre ingredientes diferentes sí cumplir cada requisito. Poseer/regalar/comprar no concede descubrimiento. Ingrediente de árbol exige fuente descubierta y acceso a herramienta elegible; cocina/refinado conserva dependencias. Mostrar fuentes alternativas correctamente.

**Aceptación e hitos:** Hito 6 define cuatro tablas, recetas/fuentes necesarias y comprueba ingrediente obligatorio. Hito 7 integra cocina/conversiones normales. Hito 13 usa tablas en seis árboles masivos por rango activo. Probar Normal/Élite, objetivo superior elegible, rechazo de lista vacía, máximo dos ingredientes, cantidades exactas, x50, interrupción, capacidad y recarga sin repetir sorteos/pagos. Hito 14 usa fuentes alternativas al comprobar recetas. Parte 2 aplica mismas pruebas en P2.1–P2.4. Son requisitos documentados, no implementación ni pruebas aprobadas.

### 7.2 Botín visible y persistencia

- Comience con un máximo de 24 vistas de paquetes mundiales activas por área cargada; Este es un objetivo de creación de perfiles, no un límite de dispositivo garantizado.
- Representa cada vista mediante un objeto agrupado, independientemente de la cantidad.
- Mantenga el resto en una cola de origen lógica. Genera el siguiente paquete solo cuando se abra espacio.
- En árboles masivos, agrupe las pilas visibles por tipo de salida; una pila puede anunciar cientos de paquetes completos pendientes.
- Mantenga cada uno de los seis árboles por rango de madera independiente: estado del nodo, cantidades pendientes y marca de tiempo de reaparición.
- Registre recompensas pendientes persistentes de árboles masivos hasta que ese árbol reaparezca; luego elimine las recompensas no reclamadas según lo solicitado, con un tiempo de vencimiento visible.
- **Confirmado:** interactuables y puntos de recolección del sendero tienen posiciones fijas definidas. Volver al campamento anterior/inicial del tramo crea un nuevo ciclo de botín para ese tramo, sin cambiar aleatoriamente posiciones. Conservar ciclo al suspender/recargar; reabrir la aplicación no renueva botín. Véase sección 7.3.
- Al abandonar un segmento de sendero, advierte si se abandonarán el botín no reclamado. Conserva el botín del segmento actual hasta que se realice la transición.

Las pruebas de pila completa deben ejercitar muchas unidades y muchas pilas de formas diferentes. Un millón de unidades en un número agregado es barato; cientos de entradas, gestos y elementos de interfaz de usuario distintos constituyen el verdadero desafío del rendimiento.

### 7.3 Puntos fijos del sendero y ciclos de botín al volver al campamento

**Distribución confirmada:** definir ID y posiciones estables para interactuables (árboles, zonas de encuentro animal, puntos de pesca) y puntos de recolección suelta. Ni ubicación ni tipo definido cambian aleatoriamente al repetir. Animales pueden moverse dentro de su zona fija según caza pasiva; eso no desplaza el punto de encuentro. Vistas del botín usan posiciones locales/zona de montón definida, no nuevos puntos aleatorios en el sendero.

**Activador confirmado:** para tramo `Campamento A -> Campamento B`, **A es el campamento anterior/inicial**. Primera entrada inicia un ciclo de botín. Cada regreso real posterior a A, incluido viaje desbloqueado desde B, sortea un ciclo nuevo para ese tramo de salida. Guardar `SegmentId`, `StartCampId`, `DropCycleId`, ID de transacción de regreso/llegada y semilla. Un regreso confirmado incrementa ciclo una vez; cargar/vincular la misma llegada no repite. Salir desde ese campamento al sendero usa ciclo preparado sin segundo sorteo. Regresar a otro campamento renueva su propio tramo de salida, no todos los tramos/regiones. Seleccionar campamento actual, abrir mapas/inventario/tiendas, renovar globo, mover cámara, desmayarse en sitio o reabrir/reanudar aplicación no son regresos y no renuevan botín.

Para cada punto fijo suelto, usar semilla de ciclo e ID estable para sortear recurso/calidad de la tabla regional elegible, más cantidades expresamente definidas. Mantener exactamente los tres ID posibles de esa región; resultados pueden repetirse entre ciclos. Guardar manifiesto por punto, cantidades recogidas e ID de reclamación antes de mostrar objetos. Nuevo ciclo sustituye botín ordinario anterior sin recoger; mostrar advertencia de abandono antes de confirmar viaje. Inventario ya recogido, entregas de misión, productos fabricados y bandejas siguen perteneciendo al jugador y no se sortean. Pruebas mantienen 1,000 por objeto y Release uno; cambia momento de renovación, no cantidades.

Árboles/caza/pesca mantienen tablas exactas y reglas de herramienta/botín; no convertir árbol fijo en interactuable aleatorio distinto. Sigue regla de sección 7.1: al completar recolección, confirmar manifiesto antes de mostrar botín, sin sorteo repetido por finalización/recarga en mismo ciclo. Usar contexto estable de punto/ciclo para estado aleatorio. Conservar progreso parcial al reemplazar herramienta/recargar en mismo ciclo. Árboles enormes del asentamiento conservan regeneración independiente de 72 horas y caducidad de pendientes; mascotas/tiendas mantienen activadores separados.

**Implementación/aceptación:** separar `GatheringPointDefinition` (ID/posición/tipo/tabla fija) de `SegmentDropCycleState` (semilla, manifiestos, progreso, reclamaciones). Añadir confirmación de ciclo por regreso al guardado de viaje e hito de recolección. Probar A -> tramo -> B -> A cambia ciclo una vez con mismos puntos/tipos; resultado aleatorio nuevo puede coincidir. Reabrir/recargar en A conserva ciclo preparado; reiniciar a mitad conserva botín/recogidas. Verificar otros campamentos, selección actual sin efecto, llegada interrumpida, filtro de tres recursos, recogida parcial, abandono y ambos modos sin duplicar ni perder objetos propios.

## 8. Herramientas, dificultad de recolección y durabilidad

### 8.1 Dimensiones independientes de las herramientas

- **Familia:** hacha, herramienta de caza a distancia, caña de pescar.
- **Rango de recurso activo:** 1-4 para madera/animales; Las bandas de peces se asignan A-D a los rangos efectivos 1/2/3/4.
- **Rango/rango de herramienta:** su rango de recurso coincidente previsto; También puede funcionar hasta dos rangos por encima de él.
- **Variante:** Normal solo en el rango uno; Normal y Élite en cada rango desde dos en adelante.
- **Calidad:** Precio/durabilidad Q1-Q5, independiente del rango.

Con cuatro rangos activos de herramientas hay **7 definiciones por familia**, 21 entre las tres: una en el rango uno y parejas Normal/Élite en los rangos dos a cuatro. Todas usan el mismo servicio de recolección. No crear todavía contenido para los rangos cinco a ocho. Comenzar con herramientas Normales de rango uno y una pareja Normal/Élite de rango dos; no existe receta Élite de rango uno.

### 8.2 Tabla recomendada de efectividad

Dejemos que `difference = resourceRank - toolRank`.

| Rango de progresión objetivo: rango de la herramienta | Hacha normal | Hacha de élite | Golpes de caza normales | Golpes de caza de élite | Pesca normal | Pesca de élite | Elegibilidad |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0, rango coincidente | 6 | **3** | 3 | **1** | 3 | **1** | Permitido |
| -1 o menos, objetivo de rango más bajo | 3 | **2** | 1 | **1** | 1 | **1** | Permitido; piso de una acción |
| +1, apunta a un rango más alto | 12 | 12 | 6 | 6 | 6 | 6 | Permitido |
| +2, objetivo de dos rangos más altos | 24 | 24 | 12 | 12 | 12 | 12 | Permitido |
| +3 o más | Ninguno | Ninguno | Ninguno | Ninguno | Ninguno | Ninguno | No hay interacción sin una herramienta elegible diferente |

Los recuentos numéricos son **valores de prueba propuestos**. **Confirmado:** Élite reduce los usos/acciones requeridos contra objetivos en o por debajo de su propio rango, no solo rangos inferiores. Mientras que Normal ya realiza una acción, Élite permanece en el mínimo de una acción. La reducción no es un aumento oculto en el rango de la herramienta. Contra objetivos por encima del rango de la herramienta, mantén los recuentos normales por ahora, mientras que Élite sigue aumentando el botín. El límite máximo +2 sigue siendo idéntico para ambas variantes.

Élite selecciona la tabla de botín mejorada para **cada cosecha elegible exitosa**, ya sea que el rango objetivo sea inferior, igual, +1 o +2. Una herramienta Normal de rango excesiva nunca obtiene el impulso. Aplicarlo a productos válidos de madera/ingredientes adicionales/caza/pesca una vez; no hay recompensa o impulso fuera de rango en caso de una cosecha fallida o cancelada. Cada paquete mundial apilable todavía contiene 1,000 unidades; Las tablas mejoradas cambian el número de paquetes, no la cantidad de pilas.

**Bases confirmadas de herramientas (rango T, nivel de receta uno, antes de calidad/muebles):** hacha/caña Normal `D0 = 10 + 10 * (T - 1)`; resortera Normal `D0 = 6 + 10 * (T - 1)`. Élite `D0 = 2 * sameFamilyNormalD0`; no existe Élite de rango uno. Aplicar el multiplicador de dominio existente y después calidad y muebles elegibles de fabricación del jugador una sola vez. Con idénticos dominio/calidad/muebles, Élite conserva exactamente el doble de durabilidad; el ahorro de acciones y aumento de botín siguen separados.

| Rango | Hacha Normal | Resortera Normal | Caña Normal | Hacha Élite | Resortera Élite | Caña Élite |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 10 | 6 | 10 | — | — | — |
| 2 | 20 | 16 | 20 | 40 | 32 | 40 |
| 3 | 30 | 26 | 30 | 60 | 52 | 60 |
| 4 | 40 | 36 | 40 | 80 | 72 | 80 |

Cada acción/fallo confirmado gasta un uso. Conservar máximos fraccionarios de modificadores internamente; permitir actuar con usos positivos, restar uno con mínimo cero y romper al llegar a cero tras resolver la acción. Probar hacha inicial con un árbol de seis acciones y cuatro usos restantes, resortera con dos conejos de tres aciertos sin fallos y caña con tres capturas de tres acciones más un uso restante. Un objetivo superior puede requerir reemplazar la herramienta: conservar progreso de recolección. Estas bases sustituyen la propuesta anterior de 120/180.

Los árboles masivos siguen el mismo alcance de elegibilidad/reducción. Conteos propuestos:

| Rango de progresión objetivo: rango de la herramienta | Acciones normales del hacha | Acciones de hacha de élite |
| --- | --- | --- |
| +2 | 80 | 80 |
| +1 | 40 | 40 |
| 0 | 20 | **10** |
| -1 | 10 | **5** |
| -2 | 5 | **3** |
| -3 o menos | 3 | **2** |

Élite aumenta el botín de árboles enormes incluso en +1/+2. Contra objetivos en o por debajo de su rango, estos recuentos Élite propuestos son `ceil(NormalActions / 2)`, mínimo uno, mientras que la curva Normal retenida llega a tres. Aplica el árbol masivo x50 una vez y la tabla de botín mejorada Élite una vez. Estos conteos reemplazan la anterior regla de acción igualitaria.

La búsqueda incluía herramientas de mochila/gancho para la familia coincidente, usos restantes positivos y rango elegible antes de comenzar. Los ganchos son lugares de almacenamiento, no ranuras para equipos. Se recomienda mostrar la herramienta seleccionada y permitir una preferencia Normal/Élite cuando se califican varias herramientas. Una extracción de recursos sin una herramienta válida no ingresa a la recolección/apuntación, no consume usos, no gasta resistencia ni inicia una animación; Se permite una sugerencia de requisito discreta. Revalidar después del consumo de la misión o cambios de inventario, incluso entre acciones comprometidas.

**Los árboles masivos se pueden volver a visitar después de talarlos (usuario, 5 de noviembre de 2026):** la interacción no es de un solo uso. Mientras está en pie, un árbol masivo funciona como los demás (tocar, pantalla enfocada, talar). Al terminar, las recompensas aparecen en la misma pantalla para recogerlas y solo el botón de volver la cierra. Una vez talado, tocar el árbol caído (su tocón y sus montones de recompensa) **vuelve a abrir la pantalla de recogida**, para que un jugador con la mochila llena pueda volver y seguir recogiendo. Volver a entrar nunca sortea ni añade recompensas: muestra exactamente los montones pendientes. El árbol caído sigue siendo interactivo hasta que sus montones se vacían o caducan con su ciclo de 72 horas; después solo muestra el tocón de rebrote hasta que reaparece.

### 8.3 Progresión de fabricación de herramientas

#### 8.3.1 Conjunto inicial completo de herramientas de pino

Las tres recetas de progresión de rango uno son explícitas, distintas, **artesanías x1 solo normales**:

| Salida exacta de la herramienta | Entradas de recetas fijas -> una salida | Desbloquear/acceder |
| --- | --- | --- |
| Hacha de pino normal | 2 ramas de pino + 2 pedernal -> 1 | Leñador L1; El pedernal aparece a la región dos |
| Resortera de pino normal | 2 Ramas de pino + 1 Piel de conejo cruda -> 1 | Cazador H1; ambos insumos sueltos disponibles en la región uno |
| Caña de pino normal | 2 Ramas de pino + 2 Cordón de cáñamo + 1 Hueso -> 1 | Explorador E5, según la receta del cordón del E4 y el acceso a los huesos de caza |

Todos los ingredientes son de rango uno; Bone es una salida de conejo de rango uno solo de interacción, no otro recurso suelto destacado. La caña es una herramienta de pesca, no otra receta de hacha. El primer alimento desbloqueado sigue siendo el jugo de manzana, por lo que la caña no es necesaria para sobrevivir en la primera región. Estas recetas utilizan los mismos ID que la sección 9.3 y la tabla de misiones; No existe ninguna variante Élite de pino. Sus bases Normales confirmadas a nivel uno son hacha 10, resortera 6 y caña 10 usos antes de dominio/calidad/muebles elegibles (sección 8.2).

#### 8.3.2 Materiales de los rangos posteriores

| Clasificación | Tema material hacha | Tema material de caza | Tema material de caña |
| --- | --- | --- | --- |
| 1 | Pino, pedernal, cordón de cáñamo | Tenedor de pino, cordón, tira de cuero | Pino, cordón, gancho de hueso |
| 2 | Abedul, herraje de cobre, borde de piedra | Agarre de cuero de abedul y castor | Abedul, gancho de cobre |
| 3 | Mezquite, borde de bronce | Arco de mezquite, hilo de algodón | Mezquite, gancho de bronce, empuñadura de algodón |
| 4 | Roble, borde de hierro | Arco de roble, cuero de jabalí | Roble, gancho de hierro, empuñadura de cuero |

Las ramas/troncos/tablas proporcionan manijas y marcos; El metal proporciona accesorios/bordes en lugar de dar a entender que mejora automáticamente porque es más caro. No agregue inventario de municiones en la primera implementación de caza; Los usos de la herramienta ya tienen en cuenta el costo de la acción.

### 8.4 Las mejoras Élite consumen una herramienta Normal y materiales avanzados del rango anterior

**Dirección confirmada:** una Élite es una receta de mejora, no una segunda construcción costosa con madera/piel/tela avanzada de su propio rango. Consume **una instancia Normal de la misma familia y rango** más una pequeña cantidad fija de material avanzado del **rango anterior**. No existe Élite de rango uno. Mantener los IDs exactos de ingredientes: no sirven una Normal de otro rango, una Élite ni otra especie de material.

Tabla de actualización activa completa propuesta, un resultado por fabricación:

| Producción de élite | Instancia normal consumida exacta | Material adicional de rango anterior |
| --- | --- | --- |
| rango 2 Hacha de abedul de élite | 1 Hacha de abedul normal | 2 tablas de pino (rango 1) |
| rango 2 Resortera de abedul Élite | 1 Resortera normal de abedul | 2 Pieles refinadas de conejo (rango 1) |
| Rango 2 Élite caña de abedul | 1 caña de abedul normal | 2 Tela fina de cáñamo (rango 1) |
| rango 3 Hacha de mezquite Élite | 1 hacha de mezquite normal | 2 tablas de abedul (rango 2) |
| Rango 3 Arco de mezquite Élite | 1 Arco de mezquite normal | 2 Pieles refinadas de castor (rango 2) |
| Rango 3 caña de mezquite Élite | 1 Caña de mezquite normal | 2 Tela de lino fino (rango 2) |
| rango 4 Hacha de roble Élite | 1 Hacha de roble normal | 2 tablas de Mezquite (rango 3) |
| rango 4 Arco de roble Élite | 1 Arco normal de roble | 2 Tejido de plumas de pavo (rango 3) |
| Rango 4 Élite caña de roble | 1 Caña de roble normal | 2 Tela fina de algodón (rango 3) |

Los pavos de rango tres no tienen pieles: por lo tanto, la mejora de caza de rango cuatro utiliza un material avanzado de plumas de pavo. Su receta de Material fijo es 3 plumas de pavo + 1 hilo de algodón -> 1 tejido de plumas de pavo. La tela de lino fino es 3 telas de lino + 1 hilo de cáñamo -> 1. La tela de cáñamo fino usa 3 telas de cáñamo + 1 hilo de cáñamo -> 1, nunca progrese cera de abejas de rango dos. Los materiales permanecen sin nivel de dominio/calidad determinada solo por muebles. Desbloquee tablas de pino de rango uno/piel de conejo fina/tela de cáñamo fina antes de las solicitudes de élite de rango dos; rango dos Tablas de abedul/Pieles finas de castor/Tela de lino fina antes del rango tres; y rango: tres tablas de mezquite/tejido de plumas/tela de algodón fino antes del rango cuatro. Agregue sus misiones de refinamiento exactas al bloque anterior, no un requisito de rango actual inaccesible. Los usos de consumo de la vivienda y otros recursos siguen siendo usos competitivos.

**Política de condición de entrada recomendada:** permitir una herramienta Normal parcialmente utilizada con usos positivos restantes; no requiere una copia nueva o de plena durabilidad. Mostrar su identidad/usos/calidad/valor y producción Élite prevista antes de comprometerse; Deje que el jugador elija entre instancias elegibles de ID exacta. Autocompletar debe preferir la instancia calificada menos valiosa, visiblemente seleccionada, nunca sacrificar silenciosamente una valiosa copia Q5. La mochila/ganchos son elegibles; Las herramientas no están equipadas. Se propone la condición/política de selección. Las herramientas de uso cero, consumidas por la misión, incluidas en listas comerciales o no disponibles de otro modo son entradas no válidas.

Al inicio de la artesanía, reserve y consuma una instancia Normal más los materiales exactos de forma atómica, luego registre una salida Élite pendiente. El Normal no sobrevive como repuesto ni se puede recolectar durante el tiempo. Al finalizar, cree una nueva instancia Élite única con estadísticas de **su propio dominio de recetas, calidad final y una instantánea de los muebles actuales**, no el grado/dominio/usos restantes de la herramienta de entrada. Acredite solo una vez la receta Élite terminada; reutilizar la entrada nunca otorga más dominio normal. La condición de entrada no tiene ningún efecto oculto de sustitución de ingredientes.

Élite tiene más durabilidad base que Normal con igual rango/dominio/calidad/modificadores. Un Q1 Élite sin dominar no hereda automáticamente la durabilidad total de un antiguo Q5 Normal con dominio de diez; obtener una vista previa de esa diferencia en lugar de cambiar silenciosamente las reglas estadísticas establecidas. Su reducción del recuento de acciones/aumento de botín se sigue aplicando independientemente de la calidad. Las estadísticas/precios de actualización se crean para el nuevo resultado; consumir un Normal caro es un costo real, no un reembolso en efectivo.

Las misiones mantienen el consumo automático de herramientas. Una solicitud de herramienta Normal puede consumir una Normal recién reclamada antes de que pueda actualizarse; complete ese requisito/reclamo y planifique la próxima misión aceptada antes de hacer un repuesto. Una actualización no puede aceptar una herramienta que ya se haya aportado. No ocultes herramientas de las misiones ni agregues actualizaciones masivas. Todas las actualizaciones permanecen **x1**. La reserva fallida, los materiales faltantes, la cola llena y la recarga no pueden duplicar ninguna instancia ni cobrar dos veces.

## 9. Cadenas de procesamiento y recetas

Todos los recuentos siguientes son **unidades**, no pilas. Una recogida de 1,000 unidades puede financiar muchas recetas durante las pruebas mecánicas; no inflar los costos para contrarrestarlo. Apunta a **uno a tres ingredientes distintos**, cuatro solo si existe una razón clara en contenido avanzado. La interfaz de usuario de nueve espacios es un límite superior, no un objetivo a llenar. Omitir reactivos/intermedios de la vida real cuando no agreguen ninguna opción útil; El tiempo de espera puede representar fermentación, cocción o curado. Las recetas de materiales no tienen dominio, con conversión instantánea cuando se puede recopilar su producción.

### 9.0 Ingredientes exactos y recetas únicas

**Confirmado:** cada requisito de ingrediente es un `ItemDefinitionId` exacto más una cantidad, nunca una lista OR, etiqueta de categoría, rango mínimo de progresión de recursos, selector de familia de materiales o alternativa seleccionada por el jugador. Por ejemplo, `recipe.cheese.cow` requiere `food.milk.cow`, no una etiqueta de leche genérica. Falla una solicitud para elaborarlo con leche de cabra sin gastar nada. La colocación manual y el Autocompletar obedecen la misma regla; la escasez sigue siendo escasez incluso si hay otra leche/madera/metal/pescado disponible.

Ejemplos de recetas separadas, no opciones dentro de una receta:

- Queso de vaca: 3 leches de vaca + 1 sal -> 1 queso de vaca.
- Queso de cabra: 3 leches de cabra + 1 sal -> 1 queso de cabra.
- Queso de mezcla de leches, si es de elaboración: 1 leche de vaca + 1 leche de cabra + 1 sal -> 1 queso de mezcla de leches. Dos leches de vaca o dos de cabra no pueden satisfacerlo.

Estas son definiciones de recetas candidatas con ID independientes; cree solo aquellos necesarios, sin una expansión combinatoria de cada combinación de ingredientes. Se recomiendan distintas producciones de queso, por lo que las recetas posteriores pueden requerir explícitamente queso de vaca, queso de cabra o el producto mixto. Una tarta de queso de vaca no puede aceptar los otros quesos a menos que se cree una receta de tarta separada para ellos.

El dominio, los desbloqueos y los temporizadores se basan en ID de receta no material: la elaboración de queso de vaca no nivela ni desbloquea el queso de cabra. Las conversiones de materiales tienen ID de receta únicas pero no dominio, como antes. Cada lote ejecuta sólo una receta; no puede mezclar ejecuciones de queso de vaca y queso de cabra en un lote de queso genérico.

Los grados de calidad del **mismo ID exacto de ingrediente** siguen regidos por el requisito de calidad explícito de la receta y la política visible de selección de inventario. Se permite combinar cantidades de varias pilas de leche de vaca calificadas; contar la leche de cabra como leche de vaca no lo es. La calificación de entrada no determina la calificación de salida según la fórmula de elaboración existente. Las etiquetas siguen siendo útiles para la elegibilidad del inventario/búsqueda/agrupación de UI, nunca para la sustitución de ingredientes.

Una plantilla de editor puede generar pino/abedul/etc. por separado. definiciones con identificaciones exactas; no debe dejar un comodín en tiempo de ejecución. Los diagramas familiares y un resumen de la fase de la misión no son recetas ejecutables. Los aportes concretos que faltan en un borrador deben resolverse antes de habilitar esa receta. Validar los requisitos fijos de una receta nuevamente en la reserva/compromiso; nunca sustituyas silenciosamente por una carrera o escasez.

### 9.0.1 Rango de desbloqueo de recetas y excepciones de cocina

**Regla estándar confirmada:** el rango de recursos de primer desbloqueo creado por una receta `R` determina su ventana de rango de ingredientes **directa**: rango uno -> `{1}`; rangos posteriores -> `{R - 1, R}`. el rango dos puede usar 1/2; rango tres 2/3; rango cuatro 3/4. Puede utilizar uno o ambos rangos permitidos, pero no puede utilizar materiales antiguos arbitrarios ni entradas de rangos futura. Una receta de progresión de rango uno que se aprende tarde o se elabora mientras se visita el rango cuatro sigue siendo una receta de progresión de rango uno. El dominio de recetas 1-10 y la calidad Q1-Q5 nunca cambian los rangos de ingredientes.

Asignar explícitos `RecipeUnlockTier` y `ItemDefinition.MaterialTier`; no los infiera a partir de la ubicación actual, la calidad, el precio de la tienda o un nombre localizado. Verifique las identificaciones directas de los ingredientes, no toda su cadena de suministro histórica: un material de rango dos permitido puede haberse elaborado utilizando entradas de rango uno en su propia receta válida de rango dos.

**Excepción de cocción confirmada:** Las recetas `Food` pueden usar ingredientes de cocina globales más antiguos designados explícitamente más allá de esa ventana. **Lista de despensa propuesta:** exacta Harina de trigo, harina de cebada, harina de maíz, avena, arroz, queso de vaca, queso de cabra, queso de leche mixta, leche de vaca, leche de cabra, crema de vaca, mantequilla de vaca, yogur de vaca, huevo, sal de roca, agua, azúcar, miel, té de hierbas, aceite de lino y aceite de girasol. Excepciones propuestas para el recipiente de recetas: taza de barro exacta y botella de vidrio. Estas siguen siendo identificaciones distintas: una receta que requiere queso de vaca no puede llevar queso de cabra aunque ambos estén marcados en la despensa. Más globales requieren un cambio de datos deliberado, no un comodín amplio de "cualquier ingrediente de cocina".

Las principales carnes/pescados, frutas/verduras regionales, madera/combustible y textiles/metales/gemas arbitrarios **no** son ingredientes culinarios globales. Por ejemplo, un asado de progresión de rango cuatro no puede usar una rama de pino solo porque una rama es combustible, y una receta de pescado de progresión de rango cuatro no puede sustituir a un pescado de progresión de rango uno. La harina y el queso pueden seguir siendo ingredientes compartidos sin que todos los alimentos regionales sean intercambiables.

Validación por artículo requerido: ID exacta/cantidad/controles de calidad **y** `(materialTier in allowedTierWindow OR (recipeRole == Food AND item.IsGlobalCookingIngredient AND materialTier <= RecipeUnlockTier))`. Aún se requiere disponibilidad: una bandera de despensa nunca introduce un ingrediente futuro no descubierto en una receta obligatoria temprana. Cocinar con alimentos básicos más antiguos no elimina el perfil de calidad/estadísticas de ese ingrediente ni vuelve a dominar su receta original. `IsGlobalCookingIngredient` es un permiso en una ID exacta, no un selector de entrada.

La excepción no se aplica a recetas de materiales/herramientas/prendas/varias. Los depósitos para mejorar la casa, la compra de muebles y las solicitudes de cobro de misiones no son recetas de elaboración; conservar sus costos de fuente/progresión creados por separado en lugar de aplicar accidentalmente reglas de receta a cada transacción. Por lo tanto, los paneles de vidrio más antiguos en un depósito de una casa posterior siguen siendo un uso válido de consumo de recursos a largo plazo, mientras que una prenda elaborada a mano de rango cuatro en progresión debe obedecer a 3/4 de las entradas.

**Permiso de llenado de barriles (revisado el 6 de noviembre de 2026):** usar el rol `BarrelFilling` en ambas partes, separado del temporizador Misc. Además de insumos dentro de {R-1, R}, permite (a) los IDs exactos de envejecimiento definidos por receta (fruta, cereal, caña o Honey descubiertos, de cualquier rango), y (b) su lista exacta de despensa para barriles: **Sugar, Honey y Maple syrup**. Las marcas Food por sí solas no autorizan estos insumos en Misc. Definir listas por receta, sin sustituciones por etiquetas. Honey se consume una sola vez. Madera, herrajes, carbón/combustible y otros insumos no listados mantienen la ventana normal; cada conversión previa se valida con su propio rol y rango.

Validación: `inTierWindow OR (role == Food AND approvedFoodPantryOrContainer AND materialTier <= RecipeUnlockTier) OR (role == BarrelFilling AND exactInputInAuthoredAgingOrBarrelPantryList)`, además de identidad, cantidades, calidad, fuentes y conversiones. Embotellar sigue siendo Food con barril exacto y Glass bottles, y requiere conocer llenado. **Pendiente:** aceptar Apricot brandy con Sugar y Plum wine con Maple syrup con fuentes y conversiones disponibles; rechazar fuentes/conversiones ausentes, sustituciones y madera/herrajes/combustible fuera de ventana. Es diseño, no resultado de prueba.

### 9.0.2 Materiales de enlace entre rangos para recetas no alimentarias

Para eliminar las fugas de rango anterior del borrador sin ingredientes intercambiables, use estas recetas de materiales fijos:

| Rango de progresión de producción/material | Ingredientes exactos -> rendimiento |
| --- | --- |
| Carbón de pino / 1 | 3 ramas de pino -> 2 |
| Carbón de abedul / 2 | 3 ramas de abedul -> 2 |
| Carbón de mezquite / 3 | 3 ramas de Mezquite -> 2 |
| Carbón de roble / 4 | 3 ramas de roble -> 2 |
| Cordón de lino / 2 | 3 Fibra de lino -> 2 |
| Cordón de algodón / 3 | 3 Fibra de algodón -> 2 |
| Relleno de lino / 2 | 3 Fibra de lino -> 2 |
| Relleno de algodón / 3 | 3 Fibra de algodón -> 2 |

Estas son nuevas identificaciones exactas, no opciones dentro de Carbón/Cable/Relleno genéricos. El relleno vegetal existente sigue siendo el rango uno de la fibra de ortiga. Los perfiles de origen establecen la cera de abejas en el rango dos, la lana cruda opcional en el rango tres y los productos refinados en el rango de recursos designado. El pequeño conjunto de puentes agregado reemplaza las excepciones profundas de la cadena de ingredientes; Solo implemente puentes utilizados por recetas activas. Utilice carbón de rango anterior para fundir donde esté incluido, cordones posteriores para herramientas/paneles posteriores y relleno posterior para juguetes posteriores. Ninguno agrega un quinto rango de recursos ni cambia la regla de Élite: Normal del mismo rango más material avanzado del rango anterior.

### 9.1 Cadenas de procesamiento compartidas

| Familia | Cadena de procesamiento | Usos posteriores |
| --- | --- | --- |
| Madera | 3 ramas coincidentes -> 1 tronco; 3 troncos iguales -> 1 tabla; tablas -> vigas de carpintería -> paneles tallados; ramas también pueden hacer carbón vegetal | Herramientas, esculturas, estructuras de casas |
| Pieles | Piel cruda -> piel curada -> piel fina; piel -> cuero -> cuero fino | Ropa, mochilas, peluches |
| Fibras | Fibra cruda -> hilo/cordón -> tela -> tela fina | Camisas, pantalones, bolsos, juguetes |
| Lana activa opcional | Lana cruda -> hilo -> tela de lana -> paño fino | Ropa de mayor rango activa; cadenas de cachemira/seda aplazadas |
| Metales | Mineral + carbón vegetal -> lingote -> accesorios/clavos; cobre + estaño -> bronce; hierro + carbón vegetal -> acero | Herramientas, artículos para el hogar y joyería |
| Piedra | Piedra en bruto -> bloques/tejas cortadas -> piedra pulida | Materiales para mejorar la casa y venta de esculturas; sin elaboración de muebles |
| Gemas | Gema en bruto -> gema tallada -> conjunto de joyas | Bienes comerciales de alto valor |
| Arcilla/vidrio | Arcilla -> ladrillos o tazas pequeñas; cuarzo + carbón -> vidrio -> botellas o paneles | Ladrillos/paneles para mejoras de casas; tazas/botellas para recetas avanzadas; sin ventanas directas/muebles de horno |
| Comida | Cultivo/carne/pescado -> plato cocido/básico -> comida o bebida compuesta | Recuperación y venta de resistencia |
| Lácteos | Leche -> nata/yogur/queso; nata -> mantequilla | Insumos alimentarios y de panadería |

Cuero abstracto a través de recetas válidas de rango fija: Piel de conejo + Sal de roca hace que la progresión sea de rango uno Cuero de conejo; Piel de castor + sal de roca hace que la progresión sea cuero de castor de rango dos; Piel de jabalí + Aceite de girasol hace que la progresión sea de rango cuatro. Cuero de jabalí con una ayuda de progresión de rango tres. La excepción para cocinar con sal de roca no se puede utilizar para eludir la regla de rango cuatro de productos no alimentarios. No se necesita sustitución de especies/cadena de taninos/duplicado de piel de conejo. Los juguetes utilizan relleno de rango con nombre válido. Los muebles siguen siendo exclusivos de la tienda.

### 9.2 Propuestas iniciales de recetas de procesamiento y construcción

**Reglas de materiales confirmadas:** sin niveles de recetas ni contadores de dominio. La conversión a troncos específicos de cada especie es instantánea porque los troncos también pueden obtenerse por recolección; 3 ramas producen 1 tronco. Tres troncos producen 1 tabla; Debido a que las tablas se fabrican únicamente en este catálogo, su receta de material puede tener un temporizador fijo creado por el rango. La calidad/valor de la producción de materiales utiliza solo su definición y muebles que mejoran la calidad de fabricación, no la calidad de los ingredientes ni el número de artesanías. Todos los costos de conversión son unidades, incluida la elaboración x1000.

| Salida | Ingredientes por fabricación | Rendimiento | Valor inicial del contexto/temporizador |
| --- | --- | --- | --- |
| Tronco de pino | 3 ramas de pino | 1 | Campamento/casa, instantáneo, sin dominio |
| Tronco de abedul | 3 ramas de abedul | 1 | Campamento/casa, instantáneo, sin dominio |
| Tronco de mezquite | 3 ramas de mezquite | 1 | Campamento/casa, instantáneo, sin dominio |
| Tronco de roble | 3 ramas de roble | 1 | Campamento/casa, instantáneo, sin dominio |
| Tabla de pino | 3 troncos de pino | 1 | Campamento/casa, temporizador de material fijo, ejemplo inicial 10 s |
| Tabla de abedul | 3 troncos de abedul | 1 | Campamento/casa, temporizador de material de rango dos fija |
| Tabla de mezquite | 3 troncos de mezquite | 1 | Campamento/casa, temporizador de material de rango tres fija |
| Tabla de roble | 3 troncos de roble | 1 | Campamento/casa, temporizador de material de rango cuatro fija |
| Carbón de pino | 3 ramas de pino | 2 | Material de progresión de rango uno, 10 s; no se acepta otra madera |
| Carbón de abedul | 3 ramas de abedul | 2 | rango dos receta de material fijo |
| Carbón de mezquite | 3 ramas de mezquite | 2 | rango tres receta de material fijo |
| Carbón de roble | 3 ramas de roble | 2 | rango cuatro receta de material fijo |
| Cordón de cáñamo | 3 fibras de cáñamo | 2 | Campamento/casa, 5 s |
| Cordón de lino | 3 fibras de lino | 2 | rango dos Material |
| Cordón de algodón | 3 fibras de algodón | 2 | rango tres Material |
| Relleno de lino | 3 fibras de lino | 2 | rango dos Material |
| Relleno de algodón | 3 fibras de algodón | 2 | rango tres Material |
| Hilo de ortiga | 3 fibras de ortiga | 3 | Campamento/hogar, 5 s; receta separada |
| Hilo de cáñamo | 3 fibras de cáñamo | 3 | Campamento/hogar, 5 s; receta separada |
| Hilo de algodón | 3 fibras de algodón | 3 | Campamento/casa, 5 s |
| Tela de ortiga | 4 fibras de ortiga | 2 | Campamento/casa, 10 s |
| Tela de cáñamo | 4 fibras de cáñamo | 2 | Campamento/casa, 10 s |
| Tela de algodón | 4 fibras de algodón | 2 | Campamento/casa, 10 s |
| Tela fino de cáñamo | 3 telas de cáñamo + 1 hilo de cáñamo | 1 | Campamento/hogar, 20 s; fuentes de rango uno en plena progresión |
| Tela de lino fino | 3 telas de lino + 1 hilo de cáñamo | 1 | Campamento/casa, temporizador de material fijo; progresión de tela avanzada de rango dos |
| Tela de algodón fino | 3 telas de algodón + 1 cera de abejas | 1 | Campamento/casa, 20 s |
| Tejido de plumas de pavo | 3 plumas de pavo + 1 hilo de algodón | 1 | Campamento/casa, temporizador de material fijo; progresión de material animal avanzado rango tres |
| Piel de conejo curada | 2 pieles de conejo crudas + 1 sal | 1 | Campamento/casa, 15 s |
| Piel de castor curada | 2 pieles crudas de castor + 1 sal | 1 | Campamento/casa, 15 s |
| Cuero de conejo | 2 pieles de conejo crudas + 1 sal | 1 | Campamento/casa, 15 s |
| Cuero de castor | 2 pieles crudas de castor + 1 sal | 1 | Campamento/casa, 15 s |
| Cuero de jabalí | 2 pieles de jabalí + 1 aceite de girasol | 1 | Progresión de rango cuatro Material con aceite de progresión de rango tres |
| Cuero refinado de conejo | 2 pieles de conejo + 1 hilo de cáñamo | 1 | Material de rango uno; sin usar cera de abejas de un rango posterior |
| Cuero refinado de castor | 2 pieles de castor + 1 cera de abejas | 1 | Campamento/casa, 20 s |
| Cuero refinado de jabalí | 2 pieles de jabalí + 1 hilo de algodón | 1 | Progresión de rango cuatro Material con hilo de progresión de rango tres |
| Piel refinada de conejo | 2 pieles de conejo curadas + 1 hilo de cáñamo | 1 | Campamento/casa, 20 s |
| Piel refinada de castor | 2 pieles curadas de castor + 1 hilo de cáñamo | 1 | Campamento/casa, 20 s |
| Hilo de lana | 3 lana cruda | 2 | Campamento/casa, 5 s |
| Tela de lana | 3 hilos de lana + 1 hilo de algodón | 2 | rango tres Material |
| Lingote de cobre | 3 minerales de cobre + 1 carbón de pino | 1 | rango dos Material |
| Lingote de estaño | 3 minerales de estaño + 1 carbón de pino | 1 | rango dos Material |
| Lingote de hierro | 3 minerales de hierro + 1 carbón de abedul | 1 | rango tres Material |
| Lingote de plata | 3 minerales de plata + 1 carbón de abedul | 1 | rango tres Material |
| Lingote de oro | 3 minerales de oro + 1 carbón de roble | 1 | Material de progresión de rango cuatro; sin conversión de billetera |
| Lingotes de bronce | 2 lingotes de cobre + 1 lingote de estaño | 3 | Campamento/casa, 20 s |
| Lingote de acero | 2 lingotes de hierro + 1 carbón de mezquite | 1 | Material de progresión de rango cuatro que utiliza entradas de progresión de rango tres |
| Clavos | 1 lingote de hierro | 6 | Campamento/casa, 5 s |
| Accesorios de cobre | 1 lingote de cobre | 3 | Campamento/casa, 10 s |
| Herrajes | 1 lingote de hierro | 3 | Campamento/casa, 10 s |
| Herrajes de acero | 1 lingote de acero | 3 | Campamento/casa, 10 s |
| Viga de pino | 4 tablas de pino + 1 cuerda de cáñamo | 1 | Campamento/casa, 20 s |
| Viga de abedul | 4 tablas de abedul + 1 cuerda de cáñamo | 1 | Campamento/casa, temporizador de material fijo |
| Viga de mezquite | 4 tablas de mezquite + 1 cordón de lino | 1 | rango tres Material |
| Viga de roble | 4 tablas de roble + 1 cordón de algodón | 1 | rango cuatro Material |
| Panel de pino | 3 tablas de pino | 1 | Material de rango uno; sin usar cera de abejas de un rango posterior |
| Panel de abedul | 3 tablas de abedul + 1 cera de abejas | 1 | Campamento/casa, temporizador de material fijo |
| Panel de mezquite | 3 tablas de mezquite + 1 cera de abejas | 1 | Campamento/casa, temporizador de material fijo |
| Panel de roble | 3 tablas de roble + 1 aceite de girasol | 1 | Progresión de rango cuatro Material con aceite de progresión de rango tres |
| Baldosas de piedra caliza | 3 piedra caliza | 2 | Campamento/casa, 15 s |
| Baldosas de arenisca | 3 arenisca | 2 | Campamento/casa, 15 s |
| Baldosas de granito | 3 granito | 2 | Campamento/casa, temporizador de material fijo |
| Baldosas de pizarra | 3 pizarra | 2 | Campamento/casa, temporizador de material fijo |
| Baldosas de mármol | 3 mármol | 2 | Campamento/casa, temporizador de material fijo |
| Basalto pulido | 2 basalto | 1 | Material de progresión de rango cuatro; omitir una progresión obsoleta abrasivo de rango dos |
| Guijarro pulido | 2 guijarros + 1 pedernal | 1 | Material de rango uno; sin usar cuarzo de un rango posterior |
| Ladrillos cocidos | 3 arcilla + 1 carbón de pino | 3 | rango uno Material |
| Vidrio | 3 cuarzos + 1 carbón de pino | 2 | rango dos Material |
| Taza de arcilla | 2 arcilla | 1 | Campamento/casa, temporizador de material fijo; contenedor avanzado de comida/bebida |
| Botella de vidrio | 2 Vidrios | 1 | Campamento/casa, temporizador de material fijo; contenedor de bebidas avanzado |
| Paneles de vidrio | 3 Vidrios | 1 | Campamento/casa, temporizador de material fijo; material para mejorar la casa, no un mueble para ventanas |
| Cuarzo tallado | 2 cuarzo | 1 | Campamento/casa, 20 s |
| Turquesa tallada | 2 turquesa | 1 | Campamento/casa, 20 s |
| Harina de trigo | 3 trigo | 2 | Campamento/casa, 5 s |
| Harina de cebada | 3 cebada | 2 | Campamento/hogar, 5 s; receta/salida separada |
| Azúcar | 3 cañas de azúcar | 2 | Campamento/casa, 10 s |
| Crema de vaca | 3 leches de vaca | 2 | Campamento/casa, Nivel de alimentación uno 10 s |
| Mantequilla de vaca | 2 cremas de vaca | 1 | Campamento/casa, Nivel de alimentación uno 10 s |
| Queso de vaca | 3 leches de vaca + 1 sal | 1 | Campamento/hogar, nivel de alimentación uno 5 min; sólo leche de vaca |
| Queso de cabra | 3 leches de cabra + 1 sal | 1 | Receta/dominio de alimentos separados, nivel uno 5 min |
| Queso de leche mixta, opcional | 1 leche de vaca + 1 leche de cabra + 1 sal | 1 | Receta/dominio de alimentos separados; se requieren ambas leches |
| Yogur de vaca | 2 leches de vaca | 2 | Campamento/hogar, nivel de alimentación uno 1 min; sin ingrediente de cultivo iniciador |

No se requiere cultivo iniciador, levadura, enzima ni tanino. Cada receta de queso utiliza su(s) leche(s) y sal exactas; la espera abstrae el proceso y sigue el límite de tiempo/dominio de los alimentos. La tela de ortiga y la tela de cáñamo son recetas de rango uno separadas, no insumos intercambiables. El algodón pertenece al tercer rango. Las variantes adicionales de gema tallada/madera/metal requieren sus propias definiciones explícitas de receta/resultado antes de su uso. Proporcionar fuentes tempranas garantizadas de fibra/sal para que se puedan realizar misiones en las que se prioricen los materiales.

Clasifique cada receta por su **rol de salida principal**, no por si aparece como ingrediente. Troncos, tablas, cuerdas/hilos/telas, cuero, lingotes, accesorios, tejas, vigas, paneles, tazas/botellas, harina y azúcar son recetas de materiales. La crema, la mantequilla, el queso y el yogur son recetas de comida: el uso de ingredientes no elimina sus reglas de dominio/cronómetro. Los tiempos de comida son ejemplos de nivel uno escritos. Las tazas y botellas utilizadas como recipientes son materiales artesanales apilables, no muebles o equipos de almacenamiento, y se utilizan en la receta final. No agregue inicialmente mecanismos de devolución/reciclaje de contenedores reutilizables. Usar comida preparada como ingrediente nunca garantiza el dominio de la receta dos veces.

Los tiempos distintos de cero de las filas de materiales se fijan por rango de receta/progresión y no mejoran con el uso. Agregue `CanGatherOutput` a la validación de recetas: cuando sea verdadero, una receta de material debe tener una duración cero. Una tienda que vende un material no significa por sí solo que sea coleccionable. Si una tabla de botín posterior agrega tablas recolectables, la receta de las tablas de esa especie también se vuelve instantánea. Los materiales avanzados puramente elaborados pueden tardar más en niveles de progresión más altos, pero aun así nunca alcanzan niveles de dominio.

### 9.3 Recetas de herramientas, ropa y mochilas

| Salida | Entradas de ejemplo | Regla de salida |
| --- | --- | --- |
| Hacha de pino normal | 2 ramas de pino + 2 pedernal | Una herramienta única; receta temprana de leñador L1 |
| Hacha de abedul normal | 2 ramas de abedul + 1 lingote de cobre + 1 cordón de cáñamo | rango dos Normal; propuesta de reconstrucción con bajos insumos, sin aumento de recompensa por el botín |
| Resortera de abedul normal | 2 ramas de abedul + 1 piel de castor cruda + 1 cordón de cáñamo | Rango dos Herramienta de caza normal |
| Caña de abedul normal | 2 ramas de abedul + 1 herraje de cobre + 1 cordón de cáñamo | Rango dos Caña normal |
| Hacha de abedul de élite | 1 Hacha de abedul normal + 2 tablas de pino | mejora de rango dos usando madera de rango uno |
| Resortera de abedul de élite | 1 Resortera de abedul normal + 2 Pieles refinadas de conejo | mejora de rango dos utilizando material animal de rango uno |
| Caña de abedul Élite | 1 caña de abedul normal + 2 paños finos de cáñamo | mejora de rango dos usando tela de rango uno |
| Resortera de pino normal | 2 ramas de pino + 1 piel de conejo cruda | Una herramienta de caza única; receta temprana de Cazador H1 |
| Caña de pino normal | 2 ramas de pino + 2 cuerdas de cáñamo + 1 hueso | Rango uno Caña normal, Explorador E5 |
| Hacha de mezquite normal | 2 ramas de mezquite + 1 lingote de hierro + 1 cordón de lino | rango tres Normal con rango dos cordón |
| Arco de mezquite normal | 2 ramas de mezquite + 2 plumas de pavo + 1 cordón de lino | rango tres Normal con rango dos cordón |
| Caña de mezquite normal | 2 ramas de mezquite + 1 herraje + 1 hilo de algodón | Rango tres Caña normal |
| Hacha de mezquite de élite | 1 hacha de mezquite normal + 2 tablas de abedul | mejora de rango tres usando madera de rango dos |
| Arco de mezquite élite | 1 arco de mezquite normal + 2 pieles finas de castor | mejora de rango tres utilizando material animal de rango dos |
| Caña de mezquite Élite | 1 Caña de mezquite normal + 2 telas de lino fino | mejora de rango tres usando tela de rango dos |
| Hacha de roble normal | 2 ramas de roble + 1 lingote de acero + 1 cordón de algodón | rango cuatro Normal con rango tres cordón |
| Arco de roble normal | 2 ramas de roble + 1 piel de jabalí + 1 hilo de algodón | Rango cuatro Herramienta de caza normal |
| Caña de roble normal | 2 ramas de roble + 1 herraje de acero + 1 hilo de algodón | Rango cuatro Caña normal |
| Hacha de roble de élite | 1 Hacha de roble normal + 2 tablas de mezquite | mejora de rango cuatro usando madera de rango tres |
| Arco de roble Élite | 1 lazo normal de roble + 2 tejidos de plumas de pavo | mejora de rango cuatro utilizando material animal de rango tres |
| Caña de roble Élite | 1 Caña de roble normal + 2 telas de algodón fino | mejora de rango cuatro usando tela de rango tres |
| Botas de rango uno | 2 pieles crudas de conejo + 1 fibra de cáñamo | Recompensa de leñador; capacidad de velocidad rango uno |
| Sombrero de rango uno | 2 pieles de conejo crudas | Recompensa del recolector; gorra de cuatro corazones |
| Camiseta de rango uno | 3 telas de cáñamo + 1 piel de conejo cruda | Recompensa del cazador; dos bolsillos |
| Pantalones de rango uno | 3 telas de cáñamo + 1 rama de pino | Recompensa del explorador; dos ganchos, trapo por defecto cero |
| Mochila de rango uno | 3 telas de cáñamo + 2 pieles de conejo + 2 ramas de pino | Área lógica 8 x 4 |
| prenda de plumas de rango tres | 3 telas de algodón + 2 plumas de pavo + 1 cuero de castor | Utilice el cuero existente más los nuevos materiales de pájaro/tela; sin piel de pavo inventada |
| camiseta de rango cuatro | 4 paños finos de algodón + 2 cueros finos de jabalí | Diez bolsillos máximo |
| pantalones de rango cuatro | 4 paños finos de algodón + 2 herrajes + 1 cuero de jabalí | Diez ganchos como máximo |
| sombrero de progresión de rango dos | 2 Pieles refinadas de castor + 1 hilo de cáñamo | Gorra de seis corazones |
| mochila de progresión de rango dos | 4 telas de cáñamo + 3 cueros de castor + 2 tablas de abedul + 2 herrajes de cobre | área de 10 x 4; receta avanzada fija de cuatro ingredientes |
| mochila de progresión de rango tres | 4 telas de algodón + 3 cueros de castor + 2 tablas de mezquite + 2 herrajes | área de 12 x 4; sin selector genérico de cuero/metal |
| mochila de rango cuatro | 4 finas telas de algodón + 3 finas pieles de jabalí + 2 tablas de roble + 2 herrajes de acero | área de 14 x 4; sin sustituciones |

**Sustituido para las prendas (5 de noviembre de 2026):** el sombrero y la mochila de rango dos, la prenda de plumas y la mochila de rango tres, y la camisa, los pantalones y la mochila de rango cuatro de esta tabla son ahora las prendas del **Conjunto A** de la sección 9.6.1, con capacidades del Conjunto A en lugar del límite del rango. La sección 9.6.1 enumera todas las prendas de los dos conjuntos de los rangos dos a cuatro. Estas son recetas individuales de ingredientes fijos. Una plantilla de creación puede ayudar a crear definiciones adicionales, pero nunca expone una elección de material en tiempo de ejecución. Los sombreros/botas/camisas/pantalones restantes necesitan sus propias recetas exactas antes de la activación; ninguna receta genérica de prendas de rango superior llena esos vacíos automáticamente. El equipo comercial indestructible sigue siendo no fabricable con los mismos límites globales.

Todas las herramientas son normales a menos que estén marcadas como Élite. La tabla ahora cubre las 21 definiciones de herramientas activas: tres Normales de rango uno más pares Normal/Élite en los rangos dos a cuatro. Las recetas normales propuestas utilizan ramas de rango actual/metal central o insumos animales en lugar de un segundo gran paquete de tablas de rango actual. Élite luego consume ese Normal más exactamente dos unidades avanzadas de rango anterior como lo especifica la sección 8.4. Élite mantiene el rango familiar/de progresión, pero tiene menos acciones hasta ese rango, mayor durabilidad base y `HasDropBoost = true` para cada objetivo elegible. Los normales nunca obtienen una bonificación de estadística de botín por la ventaja de rango excesiva. Todas las recetas de herramientas/prendas se quedan solo x1.

### 9.4 Alimentos y bebidas

| Receta | Entradas | Rendimiento | Corazones por ración |
| --- | --- | --- | --- |
| Jugo de manzana | 3 manzanas | 1 | 1; desbloqueo de región uno, temporizador de 3 segundos de nivel uno propuesto |
| Champiñones asados | 2 setas + 1 rama de pino | 1 | 1; se consume madera de rango uno |
| Tortas de avena y frutos rojos | 2 avena + 2 frutos rojos | 2 | 1 |
| Conejo asado | 1 carne cruda de conejo + 1 rama de pino | 1 | 2; Recompensa de explorador, sin requisito previo de sal |
| Castor asado | 1 carne cruda de castor + 2 ramas de abedul | 1 | 3; progresión de asado de rango dos |
| Pavo asado | 1 carne de pavo cruda + 3 ramas de mezquite | 1 | 4; progresión de asado de rango tres |
| Jabalí asado | 1 carne cruda de jabalí + 1 tronco de roble + 2 ramas de roble | 1 | 6; progresión de asado de rango cuatro |
| Trucha a las hierbas | 1 trucha arcoíris + 1 hierba de té + 1 rama de pino | 1 | 2; receta de rango uno separada, no se necesita sal |
| Pan plano de maíz | 2 harinas de maíz + 1 agua | 2 | 2 |
| Guiso de frijoles y pavo | 2 carnes de pavo + 2 frijoles + 1 tomate | 2 | 3 |
| Tortas de nueces de mezquite | 2 vainas de mezquite + 1 harina de trigo + 1 miel | 2 | 3 |
| Guiso de patatas con jabalí | 2 carne de jabalí + 2 patatas + 1 sal | 2 | 4 |
| Cuenco de arroz con carpa | 2 arroces + 2 carpas comunes + 1 sal | 2 | 4 |
| Cuenco de arroz con lucio | 2 arroces + 2 lucios del norte + 1 sal | 2 | 4; receta/salida independiente |
| Sopa de cebada de conejo | 2 carnes de conejo + 2 cebadas + 1 hierba de té | 2 | 5 |
| Tarta de queso de vaca | 2 harinas de trigo + 1 queso de vaca + 1 huevo | 2 | 5; No se aceptan quesos de cabra/mezcla |
| Chile de frijol de jabalí | 2 carnes de jabalí + 2 frijoles + 1 chile verde | 2 | 6; receta de progresión de rango cuatro, sin tomate de rango dos |
| Paquete de arroz con salmón | 2 salmón + 2 arroz + 1 té de hierbas | 2 | 6 |
| Olla de setas de castor | 2 carnes de castor + 2 champiñones + 1 zanahoria | 2 | 6; receta de progresión de rango dos usando hongos de progresión de rango uno |
| Té de hierbas con miel | 1 té de hierba + 1 miel + 1 agua | 1 | 1 |
| Bebida de avena y miel | 1 avena + 1 miel + 1 leche de vaca | 1 | 2 |
| Bebida de yogur de vaca con fresas | 1 yogur de vaca + 2 fresas | 1 | 3 |
| Café dulce | 1 café tostado + 1 azúcar + 1 agua | 1 | 3 |
| Cacao caliente | 1 cacao en polvo + 1 leche de vaca + 1 azúcar | 1 | 4 |
| Bebida de crema de vaca y vainilla | 1 crema de vaca + 1 vainilla + 1 azúcar | 1 | 4 |
| Leche de soja | 2 soja + 1 agua | 2 | 1 |
| Tazón de verduras con tofu | 2 tofu + 1 arroz + 1 tomate | 2 | 4 |
| Gachas de avena y calabaza | 2 calabazas + 1 avena + 1 leche de vaca | 2 | 3 |
| Té de miel embotellado | 1 té de hierbas + 1 miel + 1 botella de vidrio | 1 | 3 |
| Taza de cacao caliente | 1 cacao en polvo + 1 leche de vaca + 1 taza de barro | 1 | 4 |

Agregue harina de maíz (3 maíz -> 2) y tofu (2 leche de soja + 1 sal de roca -> 1) con roles explícitos de alimento/material; La sal más antigua del tofu es una excepción en la cocina. El café tostado es la progresión exacta rango cuatro Receta del material 2 granos de café + 1 rama de roble -> 2; sin combustible intercambiable. El cacao en polvo son 2 granos de cacao -> 2. Los alimentos de arce, venado, búfalo y oso permanecen pospuestos. El agua puede estar implícita excepto cuando se indique un ingrediente de agua exacto. La recuperación de bebidas no añade sed/temperatura/sistema de cafeína. Avena/arroz/harina/lácteos/etc. marcados en la despensa. permitir los insumos básicos más antiguos mencionados anteriormente; Los principales productos/proteínas y madera todavía obedecen a la ventana de rango. El desbloqueo de recetas/metadatos de origen, no el rango del jugador actual, controla estas recetas fijas.

Surtido de panadería: una pila por efectivo de pan/galletas de avena/rollos de calabaza con recuperación de 1-6 corazones, y dos pilas por oro elegidas entre brioche de miel (8), pasteles de crema y frutos rojos (10), pastel premium de manzana (12) pastel de celebración de vainilla y cacao (12), galette de calabaza y queso de vaca (10), rollos de menta y azúcar (11) y tarta de crema de coco (12). El pastel premium de manzana se propone como producto Q5 de panadería por oro con receta rara propia (sección 9.4.4), separado del jugo de manzana y de la compota de caja; comprarlo no enseña ninguna receta. Todos los valores son recuperación progresiva total; el pastel de doce corazones funciona con el medidor de tres sin sombrero porque el excedente queda pendiente (sección 4.4.3). Los productos exclusivos pueden definirse directamente sin una cadena de ingredientes para cada proceso real. Son opcionales; la comida artesanal ordinaria permite toda la progresión sin comida por oro.

**Ofertas propuestas de recetas premium:** Junto a cada oferta de comida premium artesanal elegible, la panadería puede ofrecer rara vez su receta como alternativa. La oferta de receta contiene exactamente un objeto de receta, nunca una pila; reemplaza la comida premium artesanal de ese espacio y cuesta oro equivalente a **10 veces el precio publicado de una porción** de esa comida. La comida premium sigue disponible en pilas completas; calcula el precio de una pila de 1.000 porciones con un **10% de descuento** sobre el precio por porción (por ejemplo, 5 de oro/porción -> 5.000 de oro antes del descuento -> **4.500 de oro** por la pila). Determina la sustitución al generar el inventario y guarda el resultado con las ofertas de la visita para que reabrir o recargar no vuelva a sortearlo. Comprar una receta la enseña/desbloquea; comprar comida preparada no.

### 9.4.1 Asados con combustible y recetas exactas de pescado

**Confirmado:** la madera en las recetas asadas representa combustible/material para brochetas y se consume como ingrediente. No se trata de la durabilidad de una herramienta, de un utensilio equipado reutilizable, de una decoración exclusiva para el temporizador o de un combustible sustituto genérico. Mantenga la ventana de ingredientes: pino de rango uno; rango dos Birch; Mezquite, rango tres; Roble de rango cuatro en los ejemplos. Los objetivos más grandes pueden requerir dos o tres ramas o un tronco más ramas. Sin requisito previo de estufa nueva; Las recetas desbloqueadas siguen siendo fabricaciones caseras o de campamento.

Cada fila a continuación es una receta/salida de alimento con nombre separado con su propio dominio e identificación exacta del pescado. Las cantidades de combustible propuestas utilizan la progresión animal-asado sin expandirse a cadenas realistas de condimento/procesamiento:

| Desbloquear rango | Producción exacta de pescado asado | Insumos fijos -> una ración |
| --- | --- | --- |
| 1 | Trucha arcoíris asada | 1 Trucha arcoíris + 1 Rama de pino -> 1 |
| 1 | Lubina asada | 1 Lobina negra + 1 Rama de pino -> 1 |
| 1 | Mojarra azul asada | 1 Mojarra azul + 1 Rama de pino -> 1 |
| 1 | Perca amarilla asada | 1 percha amarilla + 1 rama de pino -> 1 |
| 2 | Carpa común asada | 1 carpa común + 2 ramas de abedul -> 1 |
| 2 | Lucio del norte asado | 1 lucio del norte + 2 ramas de abedul -> 1 |
| 2 | Bagre de canal asado | 1 Bagre de canal + 2 Ramas de Abedul -> 1 |
| 2 | Lucioperca americana asada | 1 Lucioperca americana + 2 ramas de Abedul -> 1 |
| 3 | Salmón asado | 1 Salmón + 3 ramas de Mezquite -> 1 |
| 3 | Esturión asado | 1 Esturión + 3 ramas de Mezquite -> 1 |
| 3 | Trucha marrón asada | 1 trucha marrón + 3 ramas de mezquite -> 1 |
| 3 | Trucha de lago asada | 1 Trucha de lago + 3 ramas de Mezquite -> 1 |
| 4 | Pejelagarto de hocico largo asado | 1 Pejelagarto de hocico largo + 1 Tronco de Roble + 2 Ramas de Roble -> 1 |
| 4 | Atún asado | 1 Atún + 1 Tronco de Roble + 2 Ramas de Roble -> 1 |
| 4 | Corvina roja asada | 1 Corvina roja + 1 Tronco de Roble + 2 Ramas de Roble -> 1 |
| 4 | Huachinango rojo asado | 1 Huachinango rojo + 1 Tronco de Roble + 2 Ramas de Roble -> 1 |

**Rangos dos a cuatro (usuario, 5 de noviembre de 2026):** los asados simples de esta tabla se mantienen solo en el rango uno. Desde el rango dos cada especie de pez es una misión de recolección que enseña un plato variado (sección 9.6.5), así que las filas de los rangos 2-4 no se definen como recetas. Los valores iniciales de resistencia pueden ser dos/tres/cuatro/seis corazones por porción de rango: uno/dos/tres/cuatro, coincidiendo con la planificación alimentaria normal para la sección del sendero de 26 corazones; Se puede realizar un ajuste específico para cada especie sin sustituir ingredientes. Todos son apilables, admiten cantidades a granel existentes, de nivel 1 a 10, y se mantienen por debajo del límite del temporizador de alimentos de una hora. El granel multiplica exactamente los costes de madera y pescado: x100 jabalí asado necesita 100 carne de jabalí cruda + 100 troncos de roble + 200 ramas de roble; una solicitud x100 nunca trata una pieza de madera como combustible para todo el lote.

Los guisos, bebidas y platos no asados existentes no reciben automáticamente otro cargo oculto por combustible. La receta misma debe enumerar cada costo. La receta material del café tostado también incluye su rama de Roble. El jugo de manzana sigue siendo la receta de supervivencia temprana sin combustible, y la misión revisada del conejo asado temprano puede usar ramas de pino sobrantes sin sal ni un desbloqueo de procesamiento avanzado.

### 9.4.2 Recetas opcionales de las cajas misteriosas del globo

**Productos premium (confirmado el 31 de octubre de 2026):** toda receta de comida obtenida de una caja misteriosa o comprada en una tienda (caja del globo, panadería) produce un **producto premium**, como el Pastel premium de manzana: se proponen **8-12 corazones totales por porción**, en aumento por nivel para las recetas de caja (nivel uno 8-10, nivel dos 9-10, nivel tres 10-11, nivel cuatro 11-12); los productos de panadería mantienen sus valores de la sección 9.4.4 (8-12). Las recetas de misión mantienen los 1-6 corazones habituales. Las tablas de las secciones 9.4.2-9.4.3 usan estos valores.

**Dirección confirmada:** agregar desbloqueos de recetas a las recompensas de las cajas misteriosas, principalmente comida y misceláneos. Los siguientes **26 ejemplos iniciales de recetas** (21 de comida, 5 de misceláneos, incluidos ocho platos regionales de alta recuperación y dos platos de trufa de sección 9.4.3) son una propuesta, separada de las recetas de misión normales. Nunca reemplazan el primer jugo de manzana, el conejo asado, el pájaro de pino, el juguete de conejo o los desbloqueos de ropa. Conviértalos en descubrimientos opcionales, no en requisitos previos aleatorios para una progresión obligatoria. Cada identificación es única; Los nombres no crean espacios para ingredientes intercambiables.

| ID de receta/salida | Desbloquear rango | Ingredientes exactos -> rendimiento | Corazones iniciales por porción |
| --- | --- | --- | --- |
| `recipe.food.roasted_shrimp` — Camarones asados | 1 | 1 Camarón + 1 Rama de pino -> 1 | 8 |
| `recipe.food.shrimp_pie` — Pastel de camarones | 1 | 1 Camarón + 1 Harina de trigo + 1 Mantequilla de vaca -> 1 | 10 |
| `recipe.food.roasted_crab` — Cangrejo asado | 2 | 1 Cangrejo + 2 Ramas de Abedul -> 1 | 9 |
| `recipe.food.creamy_crab_soup` — Sopa cremosa de cangrejo | 2 | 1 Cangrejo + 1 Crema de vaca + 1 Zanahoria -> 1 | 10 |
| `recipe.food.honey_blackberry_biscuits` — Galletas de miel y moras | 2 | 2 Harina de trigo + 1 Miel + 1 Mora -> 2 | 9 |
| `recipe.food.roasted_squid` — Calamares asados | 3 | 1 Calamar + 3 ramas de Mezquite -> 1 | 10 |
| `recipe.food.squid_rice_bowl` — Bol de arroz con calamares | 3 | 1 Calamares + 1 Arroz + 1 Chile verde -> 1 | 11 |
| `recipe.food.mint_cow_cheese_flatbread` — Pan plano con queso de vaca y menta | 3 | 1 Harina de trigo + 1 Queso de vaca + 1 Menta silvestre -> 1 | 10 |
| `recipe.food.roasted_lobster` — Langosta asada | 4 | 1 Langosta + 1 Tronco de Roble + 2 Ramas de Roble -> 1 | 11 |
| `recipe.food.lobster_rice_bowl` — Bol de arroz con langosta | 4 | 1 Langosta + 1 Arroz + 1 Alga -> 1 | 12 |
| `recipe.food.cacao_coconut_tart` — Tarta de cacao y coco | 4 | 1 Cacao en polvo + 1 Coco + 1 Harina de trigo -> 1 | 12 |

**Platos regionales adicionales — valores propuestos, dirección de mayor recuperación confirmada:** estos ocho platos sin pastel amplían el mismo catálogo opcional. El jugo de manzana sigue siendo el desbloqueo inicial garantizado de un corazón; un plato más elaborado de manzana es un descubrimiento opcional de diez. Cada porción siguiente aporta diez o más corazones progresivos totales, sin durabilidad y con las reglas habituales de dominio/temporizador Food.

| ID de receta / producto | Rango de desbloqueo | Ingredientes exactos -> rendimiento | Corazones totales por porción | Regiones de ingredientes que deben estar descubiertas |
| --- | --- | --- | --- | --- |
| `recipe.food.apple_oat_compote` — Compota de manzana y avena | 1 | 3 Manzanas + 2 Avena + 1 Leche de vaca -> 1 | 10 | 1 y 5; tienda de leche de vaca accesible |
| `recipe.food.red_berry_oat_porridge` — Gachas de frutos rojos y avena | 1 | 3 Frutos rojos + 2 Avena + 1 Leche de vaca -> 1 | 10 | 5; tienda de leche de vaca accesible |
| `recipe.food.beaver_barley_hotpot` — Guiso de castor y cebada | 2 | 2 Carne cruda de castor + 2 Cebada + 1 Zanahoria -> 1 | 10 | Caza de castor en 7-12, Cebada en 9, Zanahoria en 8 |
| `recipe.food.blackberry_honey_oats` — Avena con moras y miel | 2 | 3 Moras + 2 Avena + 1 Miel -> 1 | 10 | 7, 5 y 11 |
| `recipe.food.turkey_bean_bowl` — Plato de pavo con frijoles | 3 | 2 Carne cruda de pavo + 2 Frijoles + 1 Harina de maíz -> 1 | 11 | Caza de pavo en 13-18, Frijoles en 14, Maíz en 13 y conversión de harina conocida |
| `recipe.food.pumpkin_chickpea_pot` — Guiso de calabaza y garbanzo | 3 | 2 Calabazas + 2 Garbanzos + 1 Queso de vaca -> 1 | 11 | 15; fuente exacta de queso de vaca accesible |
| `recipe.food.boar_potato_chili` — Chili de jabalí y papa | 4 | 2 Carne cruda de jabalí + 2 Papas + 1 Chile verde -> 1 | 12 | Caza de jabalí en 19-24, Papas en 19, Chile verde en 17 |
| `recipe.food.coconut_cacao_pudding` — Pudín de coco y cacao | 4 | 2 Cocos + 1 Cacao en polvo + 1 Leche de vaca -> 1 | 12 | 24 y 20, conversión de cacao conocida; tienda de leche de vaca accesible |

La columna regional es un filtro de elegibilidad, no otro ingrediente ni una obligación de llevar existencias. Al generar la caja, exigir descubrir **cada fuente regional nombrada**, acceso a la fuente exacta de lácteos/tienda y una conversión conocida o tienda accesible para harina/queso/cacao. Descubrir un rango no basta: llegar a la región 1 no habilita las recetas de avena de la región 5. Avena/Miel/lácteos/harina antiguos usan las excepciones exactas de despensa Food; frutas/verduras y carnes principales respetan el rango propio/anterior. Ninguna requiere ingredientes por oro, otra estación, combustible oculto ni otra receta aleatoria de caja. Volver a una región descubierta sigue siendo válido tras avanzar. Los valores de las recetas anteriores de mariscos y otros platos no cambian; estas son alternativas opcionales más nutritivas.

| ID de receta/venta buena | Desbloquear rango | Ingredientes exactos -> rendimiento | Rol |
| --- | --- | --- | --- |
| `recipe.misc.pinecone_rabbit` — Adorno de conejo con piña de pino | 1 | 1 Piña de pino + 1 Tronco de pino + 1 Fibra de cáñamo -> 1 | Escultura miscelánea |
| `recipe.misc.birch_wax_bee` — Juguete de abeja de cera de abedul | 2 | 1 tabla de abedul + 1 cera de abejas + 1 piel de castor cruda -> 1 | Juguete varios |
| `recipe.misc.painted_turkey` — Tallado de pavo pintado | 3 | 1 Tabla de mezquite + 1 Pluma de pavo + 1 Ocre amarillo -> 1 | Escultura miscelánea |
| `recipe.misc.silver_sun_brooch` — Broche sol plateado | 3 | 1 Lingote de plata + 1 Turquesa tallada + 1 Cordón de lino -> 1 | Joyas varias |
| `recipe.misc.obsidian_boar` — Talla de jabalí en obsidiana | 4 | 1 Tabla de Roble + 1 Obsidiana + 1 Aceite de Girasol -> 1 | Escultura miscelánea |

Food mantiene el cronómetro más corto con dominio en menos de una hora, sin durabilidad y con la recuperación de resistencia creada; Misc mantiene temporizadores más prolongados y precios de categoría más altos, sin durabilidad. Ambos se acumulan hasta 1,000 por perfil compatible y admiten cantidades masivas normales. Aplique los costos exactos de la madera a cada porción asada. Las excepciones de la despensa de cocina permiten harina/lácteos/arroz con nombres más antiguos en los alimentos; nunca permiten otra variedad de queso/harina, futuros insumos inaccesibles, combustible de madera viejo o exenciones de despensa en Misc. Cada ejemplo inicial de Varios sigue solo su rango de desbloqueo/rango anterior.

Utilice las curvas de temporizador de clasificación de categoría/progresión existentes y las referencias de precios para las definiciones iniciales, luego cree anulaciones por receta si es necesario. No se requieren nuevas estaciones, árboles de procesamiento de mariscos ni reactivos raros obligatorios. Estos son productos de venta/consumibles, no recetas de muebles ni ropa comercial especial. Marque las entradas de sus libros de recetas con **Fuente: Caja misteriosa de globos** y muestre los requisitos exactos de ingredientes/fuentes mientras está bloqueado.

Los ingredientes del marisco se compran Q5, pero su calidad no anula las reglas de producción artesanal: aprender una receta otorga un nivel de dominio uno, y su primera artesanía utiliza ese nivel más muebles que mejoran la calidad de fabricación. El desbloqueo de una receta en sí no tiene grado de artículo, durabilidad, huella de inventario, recuento de pilas ni producción gratuita. Las recompensas físicas compradas siguen la política Q5. El stock de ingredientes de la tienda puede aparecer antes de que se encuentre la receta; no es motivo para forzar la receta a través de una misión principal.

### 9.4.3 Recetas de trufa y orígenes exactos de desbloqueo

**Dirección confirmada:** añadir trufas recolectables y recetas útiles; los platos/desbloqueos exactos siguientes son propuestas. Usan temporizadores/dominio Food y recuperación progresiva de resistencia (sección 4.4.3); sin combustible oculto ni estación nueva. Trufa es ingrediente principal de rango dos, no sustituible por Hongos ni excepción de despensa de rango cuatro.

| ID de receta / producto | Rango de desbloqueo | Ingredientes exactos -> rendimiento | Corazones totales por porción | Origen y requisitos |
| --- | --- | --- | --- | --- |
| `recipe.food.truffle_barley_pot` — Guiso cremoso de trufa y cebada | 2 | 2 Trufa + 2 Cebada + 1 Crema de vaca -> 1 | 6 | Misión secundaria opcional de Recolector tras región 10, Cebada de región 9 y fuente exacta accesible de Crema de vaca; reunir 10 Trufa para aprender |
| `recipe.food.truffle_cow_cheese_flatbread` — Pan plano de trufa y queso de vaca | 2 | 2 Trufa + 1 Harina de trigo + 1 Queso de vaca -> 1 | 10 | Caja de globo; región 10 y Trigo de región 6/conversión conocida o tienda exacta de harina; fuente conocida/accesible de Queso de vaca |
| `recipe.food.turkey_truffle_bean_pot` — Guiso de pavo, trufa y frijoles | 3 | 1 Trufa + 2 Carne cruda de pavo + 2 Frijoles -> 1 | 11 | Caja de globo; región 10 descubierta, caza de pavo en 13-18 y Frijoles de región 14 |

La solicitud de diez Trufa es una excepción explícita de menor coste para misión opcional de rango dos, no requisito obligatorio nuevo. Comparte límite de una misión activa, consumo por ID exacto y entrega en campamento; ofrecer solo con todos los ingredientes accesibles. Otorga esa receta una sola vez con dominio uno. Completar o aplazar sin bloquear ramas principales/casa/regiones. Garantizar oportunidades suficientes de trufa suelta sin mascota. Primera receta solo por misión y excluida de cajas; las otras dos amplían sección 9.4.2 a **26 ID = 21 Food + 5 Misc**. No otorga comida terminada gratis. Mantener protección de duplicados, seis entradas, pesos propuestos, descubrimiento exacto y dominio uno. Añadir Trufa al catálogo de ingredientes y mostrar región 10 en vistas previas.

### 9.4.4 Recetas premium de panadería y precios propuestos

**Alcance ampliado:** 33 recetas opcionales coleccionables = 28 Food + 5 Misc: las 26 de globo y siete nuevas exclusivas de ofertas de recetas de panadería. La receta de misión de trufa es adicional (34 entre estos tres catálogos). Las recetas premium no entran en cajas del globo. Valores iniciales propuestos para revisión posterior.

| ID / comida | Rango | Ingredientes exactos -> rendimiento | Corazones por porción | Oro por porción | Oro por pila de 1.000 (-10%) | Oro por una receta |
| --- | --- | --- | --- | --- | --- | --- |
| `recipe.food.honey_brioche` — Brioche de miel | 2 | 2 Harina de trigo + 1 Mantequilla de vaca + 1 Miel -> 1 | 8 | 5 | 4.500 | 50 |
| `recipe.food.blackberry_cream_pastry` — Pastel de crema y moras (producto de crema y frutos rojos) | 2 | 2 Harina de trigo + 2 Moras + 1 Crema de vaca -> 1 | 10 | 7 | 6.300 | 70 |
| `recipe.food.premium_apple_pie` — Pastel premium de manzana | 1 | 2 Harina de trigo + 3 Manzanas + 1 Mantequilla de vaca -> 1 | 12 | 8 | 7.200 | 80 |
| `recipe.food.pumpkin_cow_cheese_galette` — Galette de calabaza y queso de vaca | 3 | 2 Harina de trigo + 2 Calabazas + 1 Queso de vaca + 1 Mantequilla de vaca -> 1 | 10 | 8 | 7.200 | 80 |
| `recipe.food.mint_sugar_rolls` — Rollos de menta y azúcar | 3 | 2 Harina de trigo + 1 Menta salvaje + 1 Azúcar + 1 Mantequilla de vaca -> 1 | 11 | 9 | 8.100 | 90 |
| `recipe.food.vanilla_cacao_celebration_cake` — Pastel de celebración de vainilla y cacao | 4 | 2 Harina de trigo + 1 Crema de vaca + 1 Cacao en polvo + 1 Vaina de vainilla + 1 Azúcar -> 1 | 12 | 10 | 9.000 | 100 |
| `recipe.food.coconut_cream_tart` - Tarta de crema de coco | 4 | 2 Harina de trigo + 2 Cocos + 1 Crema de vaca + 1 Azúcar -> 1 | 12 | 10 | 9.000 | 100 |

Usar IDs existentes: Vaina de vainilla es el recurso Vainas de vainilla de región 22, sin refinado nuevo. Exigir descubrimiento de todas las fuentes: brioche requiere Trigo 6 y Miel 11; pastel de moras, Trigo 6 y Moras 7; pastel de manzana, Manzanas 1 y Trigo 6; pastel de celebración, Trigo 6, Caña de azúcar 17, Granos de cacao 20 y Vainas de vainilla 22. Exigir fuentes lácteas exactas accesibles y conversiones conocidas o tiendas exactas accesibles para harina, mantequilla, crema, cacao en polvo y azúcar. Las excepciones de despensa permiten los ingredientes antiguos enumerados sin permitir sustituciones ni saltarse descubrimiento. El rango es de autor, independiente del rango actual. Conservar dominio/calidad Food normales, temporizadores inferiores a una hora, cantidades x1/x100/x1000, ingredientes/rendimiento escalados, ausencia de durabilidad y recuperación progresiva. La comida comprada sigue Q5. Sin ingredientes, estaciones, combustible oculto ni costes de fabricación en oro nuevos. Aprender otorga dominio uno/cero fabricaciones, sin comida gratis. No son requisitos obligatorios de progresión.

**Sustitución rara propuesta:** conservar un espacio común por efectivo y dos premium por oro. Tras elegir cada comida premium, sortear 1% por espacio elegible para reemplazar la pila completa por un solo desbloqueo de su misma receta. Excluir recetas aprendidas y duplicadas ya ofrecidas/reservadas en ese inventario; conservar comida sin otro sorteo si no es elegible. Guardar ID de comida/receta, tipo, cantidad (1 receta o 1.000 porciones), precio y compra. La receta se compra una vez y se aprende atómicamente con el pago incluso con mochila llena; nunca entrega una pila de pergaminos. Revisar si ya está aprendida antes de cobrar. Reabrir/recargar y navegación interna conservan resultados; salir y regresar al asentamiento renueva ofertas. Comprar comida no enseña recetas.

**Precios:** `recipeGold = servingGold * 10`; `fullStackGold = servingGold * 1000 * 90 / 100`. La receta usa precio por porción sin descuento. A 5 de oro por porción, receta 50 y pila completa 4.500. Aplicar una vez el descuento solo a pilas premium completas, nunca a recetas o comida común por efectivo. **Compras parciales (confirmado el 31 de octubre de 2026):** las pilas de tienda no son todo o nada. El jugador elige cualquier cantidad entre 1 y las existencias ofrecidas y paga `servingGold * cantidad` (oro entero); el descuento del 10% se aplica **solo** al comprar la pila completa de 1.000 porciones. Las pilas completas son una comodidad para jugadores avanzados con mejor botín e ingresos por fabricación; los jugadores nuevos compran lo que necesitan y pueden pagar. Persistir cotizaciones con enteros/punto fijo de 64 bits. Reventa artesanal usa valoración normal en efectivo y no genera el precio minorista en oro.

**Receta adicional de rango cuatro:** `recipe.food.coconut_cream_tart` — Tarta de crema de coco: **2 Harina de trigo + 2 Cocos + 1 Crema de vaca + 1 Azúcar -> 1 porción**, 12 corazones totales; rango 4; 10 de oro/porción, 9.000 por pila completa, 100 por una receta. Es distinta de la tarta de cacao y coco del globo y no requiere cacao.

**Distribución y comprobaciones de descubrimiento:** siete recetas: **una de rango 1** (manzana), **dos de rango 2** (brioche y pastel de moras), **dos de rango 3** (galette y rollos de menta) y **dos de rango 4** (pastel de vainilla/cacao y tarta de coco). Añadir las siete comidas al catálogo premium de panadería. Las fuentes se comprueban para sustituir por receta independientemente de la venta de comida preparada. Rango, dominio, oro, compra de comida, inventario de depuración e ingredientes regalados nunca sustituyen el descubrimiento de fuentes.

Al generar la oferta **y de nuevo antes de cobrar/desbloquear**, exigir rango de receta alcanzado mediante descubrimiento regional normal y todas las fuentes exactas descubiertas/accesibles. Seguir las cadenas fijas de refinados hasta la materia prima y exigir conversión conocida o tienda exacta accesible; encontrar refinados no evita descubrir su fuente regional original. Además de las fuentes anteriores: galette requiere Trigo 6, Calabazas 15 y fuentes exactas de Queso/Mantequilla de vaca; rollos requieren Trigo 6, Caña de azúcar 17, Menta salvaje 18 y Mantequilla de vaca; tarta de coco requiere Trigo 6, Caña de azúcar 17, Cocos 24 y Crema de vaca. Guardar IDs de fuentes requeridas y validar tras recarga/migración sin volver a sortear. Rechazar ofertas de receta inválidas/obsoletas sin cobrar y conservar la oferta guardada hasta una renovación legítima. Las vistas previas muestran fuentes pendientes sin otorgar conocimiento.

**Casos de aceptación:** comprobar distribución 1/2/2/2 e IDs únicos. Probar cada receta con todos los requisitos y luego retirando cada fuente individualmente. Rechazar galette antes de 15, rollos antes de 17 o 18, pastel antes de 20 o 22 y tarta de coco antes de 24; también rechazar sin Trigo 6 o fuente láctea/conversión exacta. Probar dominio/oro altos, ingredientes regalados y comida comprada con fuentes sin descubrir, recarga y compra obsoleta. Descubrir la primera región de un rango no habilita recetas de regiones posteriores. Cumplir requisitos permite el sorteo del 1%, sin garantizar oferta.

### 9.4.5 Bebidas en barril de roble (exclusivas de la caja misteriosa, propuesta del 31 de octubre de 2026; revisada el mismo día)

Bebidas en dos pasos: una **receta de llenado** pone fruta, cereal, caña o miel en un barril de roble para que envejezca, y una **receta de embotellado** distinta convierte ese barril lleno y botellas de vidrio en la bebida terminada. Ambas son recetas **exclusivas de la caja misteriosa** (se añaden al grupo de la caja de la sección 9.4.2) y se aprenden **por separado**: desbloquear la receta del barril nunca desbloquea su receta de embotellado.

**Los barriles llenos no son consumibles.** Un barril lleno no se puede comer, nunca se come automáticamente al quedarse sin resistencia y no da corazones. Es el ingrediente del embotellado o un **producto comercial valioso**: se vende por una buena suma que refleja los materiales y el envejecimiento (ver precios). El embotellado consume el barril lleno; no se devuelve un barril vacío (evita otro objeto y un ciclo de reutilización; se puede añadir más adelante si se desea).

**Madera (recomendación):** solo roble. El roble es el estándar real para barriles; el pino no sirve (su resina contamina la bebida), el abedul casi no se usa y el mezquite es sobre todo madera de ahumado (aquí carboniza el barril del whisky). Una variante "ahumada" de mezquite podría añadirse más adelante.

**Condición de aparición:** las recetas de barril y de embotellado solo pueden salir en una caja misteriosa cuando el jugador conoce la receta de **tablón de roble** (rango cuatro) y ha descubierto todas las fuentes de sus ingredientes (y cualquier conversión necesaria, como el azúcar o el carbón de mezquite). Una receta de embotellado además exige conocer su receta de llenado, para que la caja nunca entregue un embotellado sin forma de hacer su barril. Rango de desbloqueo: 4.

**Niveles de receta (confirmado):** las recetas de llenado tienen **niveles del 1 al 10 como las demás fabricaciones no materiales**: el dominio y los muebles de calidad suben la calidad final del barril y su valor (secciones 6.1 y 12.2), y una receta nueva empieza en dominio uno. El envejecimiento sigue la **dirección de tiempo de Misc**: a mayor dominio, más tiempo de envejecimiento (los mejores barriles tardan más), llegando a horas en niveles altos; las bonificaciones nunca cambian el tiempo. Las recetas de embotellado son recetas de comida normales con niveles (curva de tiempo de comida, calidad fabricada, recuperación progresiva; cada botella es una porción).

**Forma común:** llenado = **4 tablones de roble + 2 herrajes de hierro + los ingredientes de la bebida -> 1 barril lleno**. Embotellado = **1 barril lleno + 8 botellas de vidrio -> 8 botellas de la bebida**. Se aplican las reglas de ID exacto: no se sustituyen frutas, cereales, caña, miel, azúcar, carbón, herrajes ni botellas.

| Bebida | Receta de llenado (ID: ingredientes -> 1 barril lleno) | Receta de embotellado (ID) | Corazones por botella | Fuentes de ingredientes |
| --- | --- | --- | --- | --- |
| Sidra de manzana | `recipe.barrel.apple_cider`: 4 tablones de roble + 2 herrajes de hierro + 12 manzanas | `recipe.food.apple_cider` | 11 | Manzanas región 1 |
| Vino de fresa | `recipe.barrel.strawberry_wine`: 4 tablones + 2 herrajes + 12 fresas + 2 azúcar | `recipe.food.strawberry_wine` | 12 | Fresas 4, azúcar (caña de azúcar 17) |
| Vino de mora | `recipe.barrel.blackberry_wine`: 4 tablones + 2 herrajes + 12 moras + 2 azúcar | `recipe.food.blackberry_wine` | 12 | Moras 7, azúcar (17) |
| Vino de arándano | `recipe.barrel.blueberry_wine`: 4 tablones + 2 herrajes + 12 arándanos + 2 miel | `recipe.food.blueberry_wine` | 12 | Arándanos 9, miel 11 |
| Hidromiel especiada | `recipe.barrel.honey_mead`: 4 tablones + 2 herrajes + 10 miel + 1 menta silvestre | `recipe.food.honey_mead` | 12 | Miel 11, menta silvestre 18 |
| Cerveza de cebada (ale) | `recipe.barrel.barley_ale`: 4 tablones + 2 herrajes + 12 cebada + 2 miel | `recipe.food.barley_ale` | 11 | Cebada 9, miel 11 |
| Cerveza de trigo | `recipe.barrel.wheat_beer`: 4 tablones + 2 herrajes + 8 trigo + 4 cebada | `recipe.food.wheat_beer` | 11 | Trigo 6, cebada 9 |
| Ron de caña | `recipe.barrel.cane_rum`: 4 tablones + 2 herrajes + 12 caña de azúcar + 2 azúcar | `recipe.food.cane_rum` | 12 | Caña de azúcar 17 (y su azúcar) |
| Whisky de roble carbonizado | `recipe.barrel.corn_whiskey`: 4 tablones + 2 herrajes + 8 maíz + 4 cebada + 1 carbón de mezquite | `recipe.food.corn_whiskey` | 12 | Maíz 13, cebada 9, carbón de mezquite (rango 3) |

Toda receta de embotellado es `1 barril de [bebida] + 8 botellas de vidrio -> 8 [bebida]` (por ejemplo `recipe.food.apple_cider`: 1 barril de sidra de manzana + 8 botellas de vidrio -> 8 sidras de manzana).

**Precios (propuesta; los barriles usan el rango de precio de Misc, decisión del usuario):** los barriles llenos se valoran como **Misc** (la categoría de precio base más alta, como esculturas, juguetes y joyería), con referencias de nivel 10 equivalentes a Q1 por bebida en torno a la referencia de Misc de rango cuatro (6.000): sidras y cervezas las más baratas, vinos e hidromiel en medio, destilados envejecidos los más caros. Después se aplican las reglas normales: `Pbase(L) = referencia * (L + 35) / 45`, por el multiplicador de calidad final (Q5 = 2,00); sin prima de durabilidad. Las bebidas embotelladas son comida con sus propias referencias premium, fijadas para que embotellar añada valor frente a vender el barril (ocho botellas valen más que su barril más las botellas).

| Bebida | Referencia del barril (N10, equivalente Q1) | Barril N1 Q1 | Barril N10 Q5 | Referencia de la botella (N10) | 8 botellas N10 Q5 |
| --- | --- | --- | --- | --- | --- |
| Sidra de manzana | 5.000 | 4.000 | 10.000 | 700 | 11.200 |
| Cerveza de cebada (ale) | 5.200 | 4.160 | 10.400 | 720 | 11.520 |
| Cerveza de trigo | 5.200 | 4.160 | 10.400 | 720 | 11.520 |
| Vino de fresa | 5.600 | 4.480 | 11.200 | 780 | 12.480 |
| Vino de mora | 5.800 | 4.640 | 11.600 | 800 | 12.800 |
| Vino de arándano | 5.800 | 4.640 | 11.600 | 800 | 12.800 |
| Hidromiel especiada | 6.000 | 4.800 | 12.000 | 820 | 13.120 |
| Ron de caña | 6.600 | 5.280 | 13.200 | 900 | 14.400 |
| Whisky de roble carbonizado | 7.200 | 5.760 | 14.400 | 980 | 15.680 |

Los ingredientes son materiales baratos de los rangos uno a cuatro (tablones, herrajes y productos que suman unos pocos cientos de efectivo), así que un barril es un producto comercial fuerte de final de juego, como corresponde a un producto envejecido. Todos los valores son propuestas para el reequilibrio posterior.

**Otras candidatas (aún fuera del grupo):** vino de arroz (arroz 11), vino de frutos rojos (frutos rojos 5 + azúcar 17).

**Resuelto (6 de noviembre de 2026; el usuario dejó ambas decisiones a Claude):**
1. **Corazones:** las botellas dan 11-12 corazones, como el resto de la comida premium de caja del rango cuatro (sección 9.4.2). Sidras y cervezas dan 11; vinos, hidromiel, ron y whisky, 12. Los precios no cambian. Las bebidas de barril de la Parte 2 ya usan 11-12.
2. **Ventana de ingredientes:** se añade una **excepción de barril** explícita (sección 9.0.1). Las recetas de llenado pueden usar cualquier fruta, cereal, caña o miel descubierta de cualquier rango, porque el envejecimiento es la forma de que los productos antiguos conserven su valor al final del juego. El barril (tablones de roble, herrajes de hierro) y los insumos de conversión (carbón de mezquite) siguen dentro de la ventana de rango. El embotellado no necesita excepción: sus insumos son el barril y las botellas de vidrio.

**Efecto en el grupo de la caja:** el grupo de recetas coleccionables pasa de 26 a **44 recetas** (26 + 9 de llenado + 9 de embotellado), las ocho recetas premium del rancho (sección 13.4.2) lo llevan a 52, y los siete recuerdos de metales preciosos y gemas (sección 9.5.1) a 59; la probabilidad de receta por espacio (5%) y la estructura de seis entradas no cambian, así que cada receta concreta es algo más rara.

### 9.5 Esculturas, joyería y juguetes de peluche

| Clasificación | Receta de escultura | Receta de joyería | Receta de peluche |
| --- | --- | --- | --- |
| 1 | Pájaro de pino: 2 troncos de pino | Amuleto con cordón: 1 guijarro pulido + 1 cordón de cáñamo | Juguete de conejo: 2 pieles de conejo crudas + 1 fibra de cáñamo |
| 2 | Castor de abedul: 2 tablas de abedul + 1 cera de abejas | Colgante de cobre: 1 lingote de cobre + 1 cuarzo + 1 cordón de cáñamo | Juguete de castor: 2 Pieles refinadas de castor + 1 relleno de plantas + 1 hilo de cáñamo |
| 3 | Sol de mezquite: 2 tablas de mezquite + 1 turquesa | Pulsera de turquesas: 1 turquesa tallada + 1 lingote de bronce | Juguete de pavo: 2 telas de algodón + 2 plumas de pavo + 1 relleno de lino |
| 4 | Jabalí de roble: 2 tablas de roble + 1 aceite de girasol | Broche de rubí: 1 herraje + 1 rubí tallado | Juguete de jabalí: 2 cueros finos de jabalí + 1 tela de algodón + 1 relleno de algodón |

Todos los productos se acumulan hasta 1,000 por ID de artículo compatible/calidad/perfil de estadísticas. El relleno vegetal tiene una receta de material fijo: 3 fibras de ortiga -> 2 relleno vegetal. Las cañas no pueden reemplazar a la ortiga en esa receta. Las variantes adicionales de joyería de plata/oro/gemas necesitan definiciones fijas separadas si se introducen dentro del rango cuatro. Las especies diferidas permanecen únicamente en cartera. Venta Misc nunca es muebles funcionales. Preserve las pruebas de pila completa mientras difiere el equilibrio de precios.

### 9.5.1 Recuerdos de metales preciosos y gemas (exclusivos de la caja misteriosa, propuesta, petición del usuario, 5 de noviembre de 2026)

Siete recetas premium de **Misc** ligadas a los metales preciosos y gemas de cada rango, que solo se desbloquean en la caja misteriosa. **El rango uno no tiene ninguna** (no tiene metal precioso ni gema; decisión del usuario). Cada una sigue la regla premium de la sección 9.6.6: al menos tres ingredientes distintos, incluido un material refinado (lingote, gema tallada, herrajes o cuero fino), con precio en la **parte alta del rango Misc de su nivel** (sección 12.2). Quedan dentro de la ventana de rango no alimentaria {R-1, R}. Se aplican las reglas de Misc: niveles 1-10, temporizadores más largos con el dominio, pilas de 1.000, sin durabilidad y nunca muebles.

| Recipe ID / output | Tier | Exact ingredients -> 1 | Sources needed |
| --- | --- | --- | --- |
| `recipe.misc.quartz_prism_lantern` — Quartz prism lantern | 2 | 1 Cut quartz + 2 Copper fittings + 1 Glass | Quartz 9, Copper ore 8 (Cut quartz, Copper fittings and Glass recipes known) |
| `recipe.misc.tin_quartz_music_box` — Tin-and-quartz music box | 2 | 1 Tin ingot + 1 Cut quartz + 1 Birch plank | Tin ore 12, Quartz 9 |
| `recipe.misc.silver_turquoise_buckle` — Silver turquoise belt buckle | 3 | 1 Silver ingot + 1 Cut turquoise + 1 Fine beaver leather | Silver ore 18, Turquoise 16, Honey 11 (Beeswax) |
| `recipe.misc.bronze_turquoise_compass` — Bronze turquoise compass | 3 | 1 Bronze ingots + 1 Cut turquoise + 1 Glass | Copper 8 and Tin 12 (Bronze), Turquoise 16 |
| `recipe.misc.diamond_pocket_watch` — Diamond pocket watch | 4 | 1 Gold ingot + 1 Cut diamond + 1 Steel fittings | Gold ore 20, Diamond 24 |
| `recipe.misc.royal_gem_chalice` — Royal gem chalice | 4 | 1 Gold ingot + 1 Cut ruby + 1 Cut sapphire | Gold ore 20, Ruby 22, Sapphire 23 |
| `recipe.misc.lapis_jewel_box` — Lapis jewel box | 4 | 2 Oak planks + 1 Polished lapis + 1 Gold ingot | Lapis lazuli 23 (Polished lapis recipe known), Gold ore 20 |

**Elegibilidad** (sección 14): el rango de la receta está descubierto, cada origen indicado está descubierto y cada ingrediente refinado tiene receta conocida (todas son recompensas de misión de la sección 11.6) o una tienda exacta accesible. **Efecto en la caja:** el grupo coleccionable pasa de 52 a **59** (26 + 18 de barril + 8 del rancho + 7 recuerdos). La Parte 2, sección 10.3, continúa con los rangos 5-8.

### 9.6 Rangos 2-4: dos conjuntos de equipo, más comida y Misc (propuesta, petición del usuario, 5 de noviembre de 2026)

**Por qué (usuario, 5 de noviembre de 2026):** desde el rango dos las regiones son más largas (12-20 campamentos, tramos de hasta 26 corazones) y los recursos de cada bloque llegan a lo largo de seis regiones. Por eso, además de las herramientas Normal/Élite, **cada rango desde el dos tiene dos conjuntos de equipo**, y las misiones reciben **más recetas de Food y Misc** para que la progresión siga siendo constante en los tramos largos. La sección 11.6 coloca cada receta de abajo en las cuatro ramas. **Regla de ingredientes (usuario):** una receta usa solo los ingredientes reales más significativos que existen en el catálogo actual, de uno a tres ingredientes distintos (cuatro solo en mochilas avanzadas). Los condimentos, aglutinantes y pasos de proceso reales sin objeto en el catálogo se omiten. Siguen vigentes las reglas habituales: solo IDs exactos, la ventana de rango {R-1, R} para lo que no es comida (sección 9.0.1), las excepciones de despensa de Food, recetas Wearable/Tool solo x1 y temporizadores por categoría y rango (sección 10.2). Las tablas nuevas mantienen los nombres e IDs en inglés para conservar su identidad, como en la Parte 2.

#### 9.6.1 Los dos conjuntos: Conjunto A (regiones tempranas) y Conjunto B (regiones tardías)

- El **Conjunto A** se fabrica con las tres primeras regiones del bloque (7-9, 13-15, 19-21) más materiales del rango anterior. Llega **a la mitad del camino entre el límite del rango anterior y el de este rango, redondeando hacia arriba** (botas en pasos de 0,05 m/s).
- El **Conjunto B** necesita al menos un material de las tres últimas regiones (10-12, 16-18, 22-24). Llega al **límite del rango** de la tabla de la sección 4.3 y tiene **un 25% más de durabilidad base** que el Conjunto A con igual dominio, calidad y muebles. El objetivo de calibración "equipo de rango tres con dominio 10 y Q5 de unos 10 km" se aplica al Conjunto B de rango tres; el Conjunto A queda cerca de 8 km. El Conjunto B de rango cuatro con dominio 10 y Q5 da unos 14 km.
- **No es Normal/Élite:** los dos conjuntos son prendas distintas, con recetas, dominio, arte y nombres propios. No hay receta de mejora y no se consume nada del Conjunto A. El rango uno mantiene un solo conjunto (ya llega al límite del rango uno).
- **El equipo especial del globo (sección 14.1)** redondea el mismo punto medio **hacia abajo** y nunca se desgasta, así que el Conjunto A siempre es igual o algo mejor, mientras el equipo especial conserva su durabilidad infinita. Los límites globales no cambian.

| Tier | Set | Hat hearts | Boots m/s | Shirt pockets | Pants hooks | Backpack | Source regions |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 2 | A Marsh | 5 | 2.65 | 3 | 3 | 9 x 4 | 7-9 |
| 2 | B Taiga | 6 | 3.0 | 4 | 4 | 10 x 4 | 10-12 |
| 3 | A Prairie | 8 | 3.4 | 6 | 6 | 11 x 4 | 13-15 |
| 3 | B Highland | 9 | 3.75 | 7 | 7 | 12 x 4 | 16-18 |
| 4 | A Savanna | 11 | 4.15 | 9 | 9 | 13 x 4 | 19-21 |
| 4 | B Coastal | 12 | 4.5 | 10 | 10 | 14 x 4 | 22-24 |

**Impacto en la implementación:** las capacidades ya no pueden salir solo del rango del espacio (`EquipmentState.Hearts[Rank]` y similares), porque el Conjunto A queda entre rangos. Cada definición de prenda guarda su valor de capacidad explícito (corazones, velocidad, bolsillos, ganchos o tamaño de mochila). El rango sigue indicando el nivel para durabilidad, precios y los límites de la sección 4.3. El equipo guardado sigue funcionando porque los objetos de rango uno conservan sus valores.

**Recetas de los conjuntos** (IDs `recipe.wearable.<id>` -> `item.<id>`; ramas como en la sección 11.1: sombreros para Recolector, botas para Leñador, camisas para Cazador, pantalones y mochilas para Explorador). Las filas marcadas *existing* son las prendas de la sección 9.3, ahora asignadas al Conjunto A con la capacidad del Conjunto A; sus ingredientes no cambian.

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

El Conjunto B de rango cuatro depende del Bamboo fiber de Coral Shores, la última región, así que es el conjunto final de la fase uno. Toda prenda del Conjunto B necesita además materiales de las regiones tardías de su bloque, así que el Conjunto A es el que se lleva durante la primera mitad de cada bloque. El arte sigue el sistema facetado de piezas ajustadas (`Docs/Art/Characters/PlayerDefault/player-avatar-designs.txt`): un modelo de objeto y una prenda ajustada por pieza, con el tema del paisaje de cada conjunto.

#### 9.6.2 Nueva receta de Material: Beeswax

Beeswax se usa en Birch panel, Birch beaver, Fine beaver leather, Fine cotton cloth, Mesquite panel y el juguete Birch-wax bee, pero no tenía origen. **Propuesta:** `recipe.material.beeswax`, rango dos: **2 Honey -> 1 Beeswax**, temporizador fijo de material y sin dominio. Solo se fabrica, a partir del Honey de la región 11. Por eso todas las recetas con cera se pueden fabricar desde la región 11, lo que coincide con el momento del Conjunto B. **Revisión:** el Beaver toy (sección 9.5) usa **1 Flax stuffing** en lugar de Plant stuffing, porque Plant stuffing no tiene forma de desbloquearse en el rango uno y Flax stuffing la recibe en la sección 11.6.

#### 9.6.3 Nuevas recetas de Food (recompensas de misión, 3-6 corazones ordinarios)

La comida de misión mantiene el rango ordinario (la premium de 8-12 sigue en cajas y panadería). Los valores por porción suben con el rango, así que los tramos del rango cuatro (14-26 corazones) se pueden cruzar con comidas de 6 corazones, como supone la sección 6.2.4.

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

Avena, miel, sal gema, huevo, agua y lácteos más antiguos entran por la lista de despensa existente. Green chili (rango tres) es una verdura regional dentro de la ventana del rango cuatro. Sin combustible oculto: solo el asado de jabalí incluye madera, como exige la sección 9.4.1.

#### 9.6.4 Nuevas recetas Misc (bienes de venta, recompensas de misión)

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

Boar tusk es el subproducto exclusivo de interacción del jabalí (sección 6.3); necesita una definición como producto de caza del jabalí antes de activar esta receta.

#### 9.6.5 Misiones de recolección de pescado y platos de pescado variados (usuario, 5 de noviembre de 2026)

**Cada especie de pez desde el rango dos tiene su propia misión de recolección** (sección 11.6): el jugador entrega esa especie y aprende un plato de pescado. Descubrir la especie, por sí solo, no enseña nada.

**Variedad en lugar de asados simples (usuario):** los rangos dos a cuatro no usan las recetas de pescado simplemente asado de la sección 9.4.1. La misión de cada especie enseña un tipo de plato distinto (tortitas, bol de arroz, sopa, guiso, olla, crema, horneado, envoltorio o curry), con ingredientes del mismo bloque. **El rango uno no cambia:** sus cuatro asados simples y la Herb trout se quedan como están diseñados. Los asados de animales (castor, pavo, jabalí) siguen siendo la primera comida de caza, sencilla, de cada rango, con guisos, pasteles y chiles después.

| Tier | Species -> dish (mission) |
| --- | --- |
| 2 | Common carp -> Carp rice bowl; Northern pike -> Pike barley cakes; Channel catfish -> Catfish-tomato stew; Walleye -> Walleye-barley soup (a Rice mission also teaches the Pike rice bowl) |
| 3 | Salmon -> Salmon rice parcel; Sturgeon -> Sturgeon-chickpea pot; Brown trout -> Brown-trout mint bake; Lake trout -> Lake-trout corn chowder |
| 4 | Longnose gar -> Gar fish cakes; Tuna -> Seaweed rice parcel; Red drum -> Potato red-drum bake; Red snapper -> Coconut snapper curry |

Las recetas de comida de árbol y sus conversiones (sección 7.1.1) de abedul, mezquite y roble también son recompensas de misión (pedidos de Birch sap, Mesquite pods y Acorns en la sección 11.6), no desbloqueos por descubrimiento. Las Toasted pine nuts del rango uno se quedan como están diseñadas.

#### 9.6.6 Toda receta tiene misión; regla de tiendas y caja misteriosa

**Todas las recetas de los rangos dos a cuatro son recompensas de misión (usuario, 5 de noviembre de 2026),** incluidos los materiales y las recetas que antes no tenían origen:
- **Rango 2:** Honey herbal tea, Honey oat drink, Bottled honey tea, Rabbit barley soup, Pike rice bowl, Copper pendant, Birch panel, Glass panels, Cut quartz.
- **Rango 3:** Pumpkin oat porridge, Mesquite nut cakes, Bronze ingots, Turquoise bracelet, Nails, Mesquite panel.
- **Rango 4:** Clay cup y Hot cacao cup, Oak panel, Ruby brooch.

La cadena opcional de lana (Raw wool, Wool yarn, Wool cloth) no tiene origen ni receta que la necesite, así que queda **aplazada** al listado pendiente en lugar de colocarse. La sección 11.6.5 comprueba que ninguna receta de los rangos 2-4 quede sin misión.

**Regla de tiendas y caja misteriosa (usuario):** una receta solo puede pasar de una misión a una tienda o a la caja misteriosa si se rehace como **receta avanzada (premium)**:
- **Food:** 8-12 corazones en total (sección 9.4.2) y al menos tres ingredientes distintos, uno de ellos refinado o de otra región.
- **Misc:** al menos tres ingredientes distintos, incluido un material refinado (lingote, gema tallada, tela o cuero fino), con precio de Misc superior.

Las recetas sencillas de uno o dos ingredientes y todas las recetas de Material, Tool y Wearable siguen siendo recompensas de misión. Así las tiendas y cajas siguen siendo fuentes opcionales de recetas fuertes, nunca el único camino hacia una receta básica.

## 10. Interfaz de fabricación, temporizadores y fabricación por lotes

**Presentación confirmada:** mantener abiertos fabricación e inventario adyacente. Conservar cuadrícula 3 x 3, Fabricar/Autocompletar a la derecha, recetario arriba a la derecha y Volver abajo a la derecha. Elegir receta conocida, ingredientes exactos y x1/x10/x100/x1000 para apilables; herramientas/ropa/mochilas solo x1. Cada celda representa un tipo de ingrediente y cantidad reservada, no una unidad. Mostrar calidad, durabilidad aplicable, cantidad total, temporizador y espacio previsto.

Al pulsar Fabricar, validar ubicación/ingredientes/propiedad/capacidad/cola y consumir ingredientes atómicamente con la creación del producto y registro de lote. Mostrar el objeto o lote **centrado en la cuadrícula 3 x 3**, con calidad, durabilidad cuando corresponda (si no, sin durabilidad) y temporizador restante durante **1 segundo propuesto**; después dejarlo caer directamente a la mochila. El menú permanece abierto para la siguiente receta. La presentación no añade espera: el temporizador empieza al confirmar el consumo. Los resultados instantáneos muestran Instantáneo. Impedir que otro toque gaste los mismos ingredientes durante la presentación breve.

El producto con temporizador **ocupa espacio real en la mochila inmediatamente después de caer**. Puede moverse libremente, pero no comerse, equiparse, venderse, usarse como ingrediente, entregarse a misiones, colocarse en estantería ni depositarse en almacén hasta estar listo. Los apilables de un lote comparten temporizador; dividir resultados mayores de 1,000 en pilas legales con ID de lote compartido. Lotes pendientes diferentes no se mezclan; los listos pueden unirse según metadatos normales. Herramientas/equipo siempre son instancias únicas x1. Conservar indicador de temporizador en el producto y aviso HUD al terminar sin cerrar/reabrir fabricación.

**Valor delegado para pregunta 5:** buscar espacio determinísticamente después de retirar ingredientes y reservarlo antes de confirmar. No reorganizar objetos del jugador. Si no cabe, indicar que va a la bandeja persistente y animar hacia su indicador junto a la mochila; el temporizador sigue allí. Un resultado tiene un solo propietario (mochila o bandeja), con referencia al lote, nunca una copia en ambos. **Sin límite de espacios de fabricación (confirmado el 31 de octubre de 2026):** puede haber cualquier cantidad de lotes temporizados a la vez, para que las herramientas y el equipo x1 del juego avanzado nunca esperen un espacio libre. Resultados listos en mochila no requieren reclamación; transferir de bandeja exige espacio. La finalización acredita dominio una vez en cualquier contenedor; las misiones solo reciben productos listos y transportados.

Factor propuesto de duración apilable: `1 + 0.2 * log2(batchCount)`, incluido x1000; equipo x1 usa factor uno. Mantener curvas por categoría/dominio y límite Food/Tool menor de una hora. Materiales no tienen dominio y conversiones de materiales recolectables siguen instantáneas. Guardar estadísticas/muebles al iniciar. Persistir propietario/posición, temporizador compartido, transacción de finalización/dominio y presentación atómicamente; recargar reanuda sin duplicar resultados, consumo ni dominio. Puede omitirse la animación al recargar. Fabricar solo en campamentos y casa del jugador.

### 10.1 Niveles de dominio de recetas del 1 al 10

**Confirmado:** cada receta no material tiene su propia progresión de dominio a partir de la cantidad elaborada. Las conversiones de materiales nunca se nivelan. El nivel de la receta es independiente del rango de región/herramienta/equipo, la calidad del artículo de cinco grados y el nivel de la casa.

Umbrales acumulativos recomendados para **unidades de producción terminada**:

| Nivel de receta | Total de unidades terminadas requeridas | Calidad básica antes que muebles | Factor de precio base versus referencia de nivel diez | Multiplicador de durabilidad, cuando corresponda |
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

Estos umbrales son datos de prueba propuestos, no ritmo de progresión definitivo confirmada. Un lote que produce 2,000 porciones de comida terminada cuenta 2,000 unidades, ni siquiera presionar un botón. Por lo tanto, una fabricación apilable x1000 puede alcanzar el nivel diez rápidamente, lo que resulta útil para las pruebas de límites solicitadas. Las herramientas/prendas avanzan a través de fabricaciones repetidas x1; Las solicitudes masivas siguen no disponibles. No desaceleres el dominio solo para compensar las recolecciones de pila completa.

Instantánea de dominio y mobiliario al inicio del lote; cada salida en ese lote usa esa instantánea incluso si termina suficientes unidades para ganar varios niveles. Acredite las unidades terminadas una vez cuando un lote exitoso esté listo, no al inicio, transferencia de inventario, reclamo repetido, venta o uso. Reclamar parcialmente un lote listo no otorga más dominio. Los materiales instantáneos no aportan nada. Si varios lotes están listos juntos, cada uno conserva su propia instantánea de creación. Una transacción guardada registra tanto las unidades completadas como las acreditadas, por lo que la reapertura no puede cultivar el dominio.

Sólo los lotes futuros se beneficiarán de una subida de nivel o de muebles nuevos. Los artículos existentes conservan sus valores intrínsecos y su durabilidad. El libro de recetas muestra el nivel, el conteo hasta el siguiente nivel, las estadísticas actuales/siguientes y el cambio de hora específico de la categoría; Las recetas de materiales muestran **Sin dominio** e **Instantáneo** o su temporizador fijo en lugar de un nivel falso.

### 10.2 Temporizadores según categoría, rango y dominio

**Dirección confirmada:** Los niveles de herramientas y alimentos acortan los temporizadores. Los niveles de las prendas y varios alargan los temporizadores. Las fabricaciones de rango de baja progresión toman unos segundos; Los prendas/misceláneos avanzados pueden tardar horas. Los cronómetros de herramientas y alimentos siempre deben permanecer por debajo de una hora. Los materiales no tienen ajuste de tiempo basado en niveles y los materiales recolectables son instantáneos.

Fórmula recomendada para duración de una sola fabricación:

- Material recolectable: `0`, independientemente del rango, cantidad, muebles o número de artesanías anteriores.
- Material exclusivo: el autor corrigió el `materialBaseSeconds` para esa receta/rango.
- Herramienta/Alimento: `tierBaseSeconds / (1 + 0.08 * (L - 1))`.
- Prendas/Artesanías: `tierBaseSeconds * (1 + 0.10 * (L - 1))`.

Luego aplique el factor de lote recomendado una vez. Mantenga el tiempo de alta resolución internamente y redondee solo la pantalla, de modo que el redondeo de la interfaz de usuario no invierta la dirección del temporizador. Los muebles que mejoran la calidad de fabricación y todas las bonificaciones de durabilidad tienen un efecto **cero** en los temporizadores. Los muebles de recuperación de alimentos y de precio de venta tampoco los cambian.

Ejemplos de duración base x1, nivel uno inicial:

| Rango | Herramienta | Comida | Prendas, incluida la mochila | Varios, incluidos juguetes, joyas y esculturas |
| --- | --- | --- | --- | --- |
| 1 | 5 segundos | 3 segundos | 15 segundos | 10 segundos |
| 2 | 20 segundos | 20 segundos | 3 minutos | 2 minutos |
| 3 | 1 minuto | 90s | 30 minutos | 15 minutos |
| 4 | 5 minutos | 10 minutos | 2 horas | 1 hora |

Se permiten anulaciones específicas de definición dentro de cuatro rangos activos; El número de región no es un rango adicional de temporizadores. Los tiempos de ejemplo iniciales son anulaciones de recetas. En el rango activo cuatro/dominio diez, una sola prenda tarda 3 horas y 48 minutos, una herramienta unos 174 segundos y la comida unos 349 segundos. El lote de alimentos x1000 más lento de la lista permanece por debajo de los 30 minutos en el dominio uno. Las recetas dentro de un bloque de seis regiones pueden usar diferentes tiempos de creación sin otra especie o subcadena de procesamiento de la vida real.

Valide `maximumSingleDuration * maximumSupportedBatchFactor < 3600 seconds` para recetas de herramientas/alimentos, utilizando el factor 1 para herramientas (solo x1) y aproximadamente 2.993 para alimentos (se admite x1000). Rechace las definiciones no válidas en lugar de permitir un cronómetro de alimentos/herramientas de más de una hora o limitar muchos niveles de dominio al mismo tiempo. Los prendas pueden tardar horas en fabricarse por artículo, pero no pueden fabricarse en masa. Los lotes de Misc apilables pueden tardar más que las fabricaciones individuales, incluidas varias horas; Obtenga una vista previa del tiempo de finalización real antes de consumir materiales. El tiempo de espera en la cola es independiente del temporizador del propio artículo; Los ocho registros de fabricación pendientes se ejecutan de forma independiente hasta que estén listos.

Ejemplos para verificar:

- 3 ramas de pino -> 1 tronco de pino: calidad de salida instantánea, sin dominio, solo para muebles.
- 3 troncos de pino -> 1 tabla de pino: tiempo de material de creación fijo, sin dominio, calidad de salida solo de muebles.
- 1,000 ejecuciones de conversión a troncos: 3,000 ramas consumidas, 1,000 troncos producidos instantáneamente; sin contador de XP/nivel.
- 1,000 ramas: como máximo 333 conversiones a troncos, lo que produce 333 troncos con una rama restante; sin redondear a una pila completa de resultados elaborados.
- Versiones de nivel cinco y diez del mismo prenda: ambos Q5 básico, nivel diez tienen mayor precio/durabilidad y un temporizador más largo.
- Versiones de nivel cinco y diez de la misma receta de comida: ambas Q5 de referencia, el nivel diez tiene un precio más alto y un temporizador más corto, sin durabilidad.
- Agregar muebles que mejoran la calidad de fabricación o de vida útil del equipo cambia las estadísticas de producción y el precio apropiados, pero deja el temporizador previsto idéntico.

## 11. Misiones y dependencias de desbloqueo

**Confirmado:** cuatro ramas ordenadas independientes: **Recolector, Leñador, Cazador, Explorador**. Sólo una misión está activa a nivel mundial. Cada rama mantiene su propio rango de próxima misión/progresión; Después de reclamar un campamento, elige la próxima misión de cualquier rama elegible. Explorador cubre exploración, fibras, desbloqueos relacionados con la pesca y su función de vestimenta, no otro sistema de movimiento libre. Los nuevos ejemplos de rama reemplazan los estrictos Materiales -> Herramientas -> Alimentos -> Ropa -> Puerta de fase miscelánea.

El NPC permanente aparece en el campamento inicial y en las visitas al campamento con una finalización reclamable. No ocupa asiento de troncos. La primera misión se puede aceptar antes de la salida. El reclamo otorga la receta especificada una vez, guarda la recompensa y luego ofrece las siguientes misiones elegibles.

### 11.1 Misiones del primer rango y prendas asignadas a cada rama

Conserva los nuevos ejemplos como doce misiones específicas. Las palabras genéricas en `Trail.md` se resuelven en los ID de artículo exactos que aparecen a continuación; no se introduce ninguna regla de entrada intercambiable. Cada requisito es **10 unidades**, o diez instancias individuales de herramientas. Todas las recompensas son desbloqueos de recetas, no resultados elaborados de forma gratuita.

| Rama/ranura para ropa | Misión 1: consumir -> recompensa de receta | Misión 2: consumir -> recompensa de receta | Misión 3: consumir -> recompensa de receta |
| --- | --- | --- | --- |
| Recolector/sombrero | 10 Ramas de pino -> Tronco de pino | 10 Manzanas -> Jugo de manzana | 10 esculturas de pájaros de pino -> sombrero de rango uno |
| Leñador / botas | 10 Piedras de pedernal -> Hacha de pino normal | 10 Troncos de pino -> Escultura de pájaro de pino | 10 hachas de pino normales -> botas de rango uno |
| Cazador / camisa | 10 Pieles de conejo -> Resortera de pino normal | 10 Resortera de pino normales -> Juguete Conejo | 10 juguetes para conejos -> camiseta de rango uno |
| Explorador / pantalones | 10 Fibra de cáñamo -> Tela de cáñamo | 10 Carne de conejo cruda -> Conejo asado | 10 Tela de cáñamo -> pantalones de rango uno |

Estas cuatro recompensas de ropa completan un conjunto de sombrero/botas/camisa/pantalones; La mochila viene en una misión posterior. Las asignaciones de rama a espacio siguen siendo consistentes en los rangos posteriores. El cambio de rama reanuda su índice de misión guardada; no crea una nueva puerta de fase de categoría ni restablece otra rama. **Desde el rango dos cada rango tiene dos conjuntos de ropa (usuario, 5 de noviembre de 2026):** el Conjunto A de las regiones tempranas del bloque y el Conjunto B de las tardías, con recetas y estadísticas independientes dentro de los límites existentes (sección 9.6.1). No añaden un quinto rango de recursos. El rango uno mantiene un solo conjunto.

### 11.2 Cómo garantizar que las misiones iniciales puedan completarse

Varios ejemplos de recetas iniciales anteriores dependían de materiales avanzados que se desbloquearon más adelante. **Revisiones de recetas recomendadas para el primer conjunto**, conservando las identificaciones exactas y el nuevo orden de las misiones:

| Producción temprana | Ingredientes fijos -> salida | Razón |
| --- | --- | --- |
| Hacha de pino normal | 2 ramas de pino + 2 pedernal -> 1 | Leñador L1 puede suministrar su propia receta sin receta de cordón bloqueado |
| Resortera de pino normal | 2 Ramas de pino + 1 Piel de conejo -> 1 | Cazador H1 bootstraps de los dos materiales sueltos compartidos |
| Escultura de pájaro de pino | 2 Troncos de pino -> 1 | L2 lo desbloquea antes de la receta de tabla posterior; G1 ya desbloquea troncos |
| Juguete de conejo | 2 Pieles de conejo + 1 Fibra de cáñamo -> 1 | H2 no depende de una cadena posterior de piel fina/relleno/hilo |
| sombrero de rango uno | 2 pieles de conejo -> 1 | La primera recompensa de ropa se puede utilizar sin recetas de curado avanzadas |
| botas de rango uno | 2 Pieles de conejo + 1 Fibra de cáñamo -> 1 | Sin requisito previo para el procesamiento de cuero/cordón bloqueado |
| camiseta de rango uno | 3 Telas de cáñamo + 1 Piel de conejo -> 1 | Explorador E1 proporciona la receta exacta de tela |
| pantalones de rango uno | 3 Telas de cáñamo + 1 Rama de pino -> 1 | Sin cordón bloqueado ni requisito de interacción ósea únicamente |
| Conejo asado | 1 Carne cruda de conejo + 1 Rama de pino -> 1 | La comida Early Explorador utiliza combustible de rango uno, sin requisito previo de sal |

Estos reemplazan las recetas de ejemplo iniciales de primer rango correspondientes anteriores en la sección 9, no opciones de ingredientes alternativos dentro de la misma receta. Las variantes refinadas posteriores obtienen nuevos ID de receta.

Bordes importantes de rama transversal/fuente:

- G1/G2 puede terminar en **región uno**, cuyos recursos sueltos son ramas/pieles/manzanas. Acepte G1 en la entrada, entréguelo en el siguiente campamento, acepte G2 y entregue manzanas en otro campamento temprano para desbloquear jugo. Garantiza manzanas antes de esa entrega; retener lo suficiente después de la contribución de la misión de diez unidades para fabricar. No se necesita ninguna herramienta ni compra en el taller.
- La Región dos presenta el pedernal y la fibra de cáñamo, lo que habilita Leñador L1 y Explorador E1. No pretenda que esas fuentes estén en la región uno ni requiera una tienda de campamento aleatoria para ellas.
- Recolector G3 requiere la receta del pájaro pino del leñador L2, que a su vez necesita troncos de pino de G1 o la interacción de un árbol. Muestre G3 como bloqueado hasta que se cumpla la dependencia de receta/fuente y ofrezca otras ramas en lugar de activar una misión imposible.
- Cazador H2 consume **diez resortera fabricadas x1**, incluidas las herramientas transportadas, si están presentes; no hay fabricación a granel de equipo ni protección de última herramienta. El jugador puede crear un repuesto adicional después de la contribución cuando sea necesario. H3 utiliza la receta de juguete de H2 y fibra de cáñamo de la región dos.
- Explorador E2 requiere carne de conejo cruda procedente de la caza, no una ranura de recolección suelta. Cazador H1 proporciona primero la receta del resortera. Los conejos de la región uno existen como elementos interactivos incluso antes de que el jugador pueda usarlos. E3 requiere tela de cáñamo lista para fabricaciones de E1, luego otorga pantalones.
- Se pueden intercalar cuatro ramas, pero solo una misión está activa. La finalización solicita un reclamo de campamento antes de la próxima aceptación. Tomar herramientas para una misión nunca revierte su receta conocida ni confisca el equipo equipado.

### 11.3 Misiones posteriores, disponibilidad regional y validación de fuentes

Después de las primeras recompensas de ropa, amplía cada rango con tabla/piel refinada/cuero/tela, mochilas, comida, venta miscelánea y pesca. Extensiones de rango uno propuestas:

- Recolector G4: 10 Troncos de pino -> Receta de tablas de pino; G5: 10 cañas -> Trenza de caña (región seis).
- Leñador L4: 10 Tablas de pino -> Viga de pino, con la receta de cordón disponible a través de Explorador; L5: 10 Arena -> Mortero (región seis).
- Cazador H4: 10 Pieles de conejo -> Cuero de conejo; luego piel/refinamiento y variantes de juguetes según sea necesario.
- Explorador E4: 10 Fibra de cáñamo -> Cordón de cáñamo; E5: 10 ramas de pino -> Caña de pino normal, después del acceso al cordón/hueso; E6: 10 Trucha arcoíris -> Trucha con hierbas después de la región cuatro Se puede acceder a las hierbas de té; E7: 10 Tela de cáñamo -> mochila inicial después de desbloquear el cuero Cazador's Rabbit.

Estas son adiciones propuestas después de los doce iniciadores especificados. Nunca solicite pescado antes de que exista una receta/fuente de caña. Valida la elaborabilidad de la receta de recompensa, así como el elemento solicitado de la misión. El gráfico permite entrelazar ramas; no restablece la antigua secuencia de categorías universales.

**Los rangos dos a cuatro se planifican misión por misión en la sección 11.6 (5 de noviembre de 2026)**, incluidas las misiones puente del rango uno que desbloquean los refinados que necesita el rango dos. Desde el rango dos, distribuye los desbloqueos de hacha/caza/caña Normal/Élite y hasta dos conjuntos de ropa en el bloque de seis regiones. Conserve los sombreros de recolector, las botas de leñador, las camisas de cazador y los pantalones de explorador; asignar mochila y materiales avanzados explícitamente a una rama. Un segundo juego puede mejorar la durabilidad o la presentación respetando diez bolsillos/ganchos, doce corazones y límite de marcha a paso ligero. Utilice la tabla de fuentes destacadas de 18 elementos y el botín solo de interacción para programar la disponibilidad de la misión; nunca solicite un material de la región 14 mientras la región 14 no se haya descubierto.

Cada definición de misión almacena rama, rango de recursos, índice de secuencia, cantidad/artículo solicitado exacto, ID de misión/receta de requisitos previos, región de origen más temprana accesible e ID de recompensa de receta. El ingrediente compartido de caza del rango tres son las plumas de pavo, no una piel inexistente. Las tiendas son suplementos opcionales; El stock de la tienda del Q5 no es un atajo obligatorio para evitar una fuente/receta inaccesible. Ninguna misión hace referencia a un rango diferido de animal/madera.

Mantenga en diez el recuento de las primeras doce misiones especificadas. Las pruebas mecánicas adicionales pueden utilizar requisitos de 999/1,000/1,001/10,000 unidades, independientemente de las misiones de campaña creadas. Las solicitudes de herramientas/ropa cuentan casos únicos; Las solicitudes apilables consumen solo las unidades necesarias. Una misión que solicita un producto elaborado se ofrece sólo cuando se puede conocer su receta y los insumos requeridos tienen fuentes alcanzables. Si las dependencias bloquean una rama, mantenga visible otra rama elegible en lugar de forzar la aceptación y exigir la cancelación.

### 11.4 Cantidades de misión: de 10 a 150 objetos

**Puntos finales confirmados:** las misiones de rango más bajo requieren diez elementos, y los requisitos de rango más alto aumentan hasta **150**. Los conteos son unidades/instancias únicas, nunca pilas completas. Calendario predeterminado propuesto:

| Rango de progresión de recursos activos | Bloque de región | Cantidad de misión predeterminada | Estado |
| --- | --- | --- | --- |
| 1 | 1-6 | **10** | Recuento de rango más bajo confirmado; conservar las doce misiones iniciales |
| 2 | 7-12 | **50** | Valor intermedio propuesto |
| 3 | 13-18 | **100** | Valor intermedio propuesto |
| 4 | 19-24 | **150 máximo** | Techo confirmado; misión exacta puede ser menor cuando su costo de creación lo amerita |

Aplique el valor predeterminado por rango de recursos, no por número de región o dominio de recetas. Cada misión guarda su cantidad explícita; cualquier anulación inferior para una costosa fabricación de rango alto debería ser una opción de equilibrio creada por el autor, no un descuento de tiempo de ejecución silencioso. No superar los 150 en la campaña. Las misiones de prueba de mecánicas que solo limitan los límites pueden superarlo, pero no son una progresión normal. Las cajas del almacén siguen siendo de diez unidades y no se ven afectadas.

**Pedidos de instancias (propuesta, 5 de noviembre de 2026):** las misiones que piden herramientas o prendas usan cantidades definidas menores, **20 / 30 / 40** en los rangos dos / tres / cuatro (sección 11.6), porque cada instancia es una fabricación x1 aparte. Los pedidos apilables mantienen 50 / 100 / 150. Mantenga las misiones de instancia de herramientas honestas: una solicitud de 150 herramientas significa 150 herramientas únicas terminadas a través de artesanías x1, no 150 usos de herramientas, 150 unidades de durabilidad parcial o una receta masiva recientemente introducida. Las solicitudes deben nombrar explícitamente las identificaciones Normal o Élite. Un Élite no puede completar una solicitud de herramienta Normal, y la Normal consumida en una mejora tampoco puede contar para la misión activa.

Presupuesto **costos de recetas recursivas y usos de consumo de recursos competitivos**, no solo la pantalla de la última receta. Por ejemplo, 150 mejoras de hacha de élite al costo propuesto de dos tablas consumen **150 hachas normales del mismo rango + 300 tablas de rango anterior**. A través de las conversiones de 3 troncos/3 ramas, 300 tablas equivalen a 900 troncos o 2,700 ramas si toda la madera se convierte a partir de ramas, antes de contar los ingredientes propios de las herramientas normales. Los troncos de árboles directos proporcionan otra vía de adquisición. Las mejoras en el hogar, los materiales enviados a las misiones, otros equipos y el desgaste esperado de las herramientas deben permanecer en el mismo libro mayor.

Durante las pruebas mecánicas, las recogidas de 1,000 unidades siguen siendo las predeterminadas y hacen que estas cuotas sean económicas; **no confunda eso con una economía final validada**. Conserve un informe/preajuste de análisis de equilibrio separado que modele futuras cantidades de recolección más bajas y tasas de recolección reales sin cambiar el perfil de prueba de pila completa en vivo. Mida las acciones de recolección/viajes, el costo de las herramientas normales, el costo de refinamiento del rango anterior, las unidades de misión consumidas, el stock de mejoras disponible, las reservas de la casa, el tiempo de cola de creación y el reemplazo de herramientas fallidas/usadas. En 150 instancias de equipo, la carga de trabajo de interacción/temporizador de solo x1 también es un costo de ritmo; márquelo para una revisión de equilibrio posterior en lugar de agregar fabricación de equipo a granel detrás de la regla del usuario.

Desbloquea recetas de refinamiento de rangos anteriores antes de las misiones Élite de un nuevo rango y desbloquea su herramienta Normal antes de su receta Élite. Evite exigir un Élite como elemento requerido para la misión de desbloqueo de recompensa. Prefiera las solicitudes iniciales sin procesar/Normal del rango actual para desbloquear recetas Élite, luego solicitudes posteriores de salida Élite cuando la cadena ya esté utilizable. Las cuatro asignaciones de ramas/ropa y la regla exacta de los ingredientes permanecen sin cambios.

### 11.5 Reglas de recolección y entrega de objetos

Reglas de recolección:

- Consumir automáticamente artículos transportados elegibles para la misión activa, incluso al aceptar la misión y después de la adquisición/transferencia. No se necesita confirmación de Contribute por artículo por separado. Muestre las cantidades solicitadas y las herramientas/equipo de repuesto elegibles en la vista previa de la misión para que el jugador pueda planificar. El contenido del cofre de almacenamiento existente nunca se escanea ni se elimina automáticamente.
- Mover un artículo de un cofre de almacenamiento al inventario personal cuenta como la oportunidad de adquisición solicitada. Los movimientos repetidos no pueden contar las mismas unidades porque las unidades aportadas se consumen.
- Consuma solo el requisito restante, no una pila completa: una misión que necesita 10 unidades reduce 1,000 a **990**. Los tipos de requisitos ya completados no consumen nada más. Emitir un recibo de cantidad/instancia.
- Las herramientas no están equipadas: las herramientas elegibles en la mochila o en los ganchos de los pantalones se pueden consumir directamente, incluida la última herramienta utilizable de esa familia. No los perdones en silencio, no fabriques reemplazos ni eximas una herramienta porque era la opción de cosecha preferida. Las interacciones con las herramientas faltantes permanecen no disponibles hasta que se obtenga otra herramienta elegible.
- Sombreros, botas, camisas, pantalones y mochilas de repuesto en el inventario llevado son instancias elegibles. **La ropa actualmente equipada y la mochila actualmente equipada están protegidas**: nunca las desequipes ni las consumas automáticamente. Una vez reemplazados y almacenados como repuestos, pueden volverse elegibles.
- Consuma solo el número solicitado de instancias únicas coincidentes y conserve todas las demás. **Confirmado:** comprobar solo ID exacto y cantidad restante; sin umbral de calidad, dominio ni durabilidad. Rango/variante importan solo por tener ID distintos. Los productos pendientes y objetos no disponibles/reservados siguen excluidos; se protege el equipo puesto. Recomendación: elija primero el menor valor cotizado entre objetos del ID exacto y muestre su identidad en el recibo; agregue una anulación de selección más adelante solo si las pruebas demuestran que es necesario.
- Las salidas de artesanía pendientes, las bandejas de recompensa no reclamadas, el contenido de cofres/estanterías de almacenamiento, el depósito en garantía comercial y las cantidades de transacciones ya comprometidas no se consideran contribuciones. Las herramientas/equipos listos reclamados se vuelven elegibles de inmediato. Serializar acciones en competencia para que una instancia no pueda dispararse, venderse, comerse o reservarse en la misma transacción; un arrendamiento breve por acción no debe convertirse en una protección de misión permanente para una herramienta transportada.
- Las misiones de elementos creados cuentan las salidas listas cuando se reclaman, no solo cuando comienza el cronómetro.
- El progreso completado es monótono; La recompensa del NPC es idempotente en todas las recargas.
- Oculte el contador de HUD en el asentamiento mientras continúa procesando contribuciones elegibles y muestre una notificación de finalización.
- Nunca elijas una misión de región futura inaccesible mediante progresión ordinaria. La excepción de la caja misteriosa para una misión actual no justifica que sea obligatorio comprar cajas doradas.

Cada desbloqueo necesita una ruta desde los recursos/recetas disponibles. Cree un informe de dependencia del editor para ciclos, fuentes faltantes, referencias de rango deshabilitadas, salidas de muebles e ingredientes excesivos (advertir sobre tres ingredientes distintos, rechazar arriba de nueve). Compruebe que los ocho niveles de la casa se puedan alcanzar con los cuatro rangos activos y que la primera casa después de la región cuatro no requiera materiales de rango dos o superior.

### 11.6 Plan de misiones para los rangos 2-4 (propuesta, petición del usuario, 5 de noviembre de 2026)

Cada fila es una misión: **consumir lo pedido -> aprender una receta**, como en el rango uno. Las reglas de las secciones 11.1-11.5 y la puerta por región de misión implementada no cambian:
- una misión activa
- las misiones se abren por **región de misión** (la columna Region): las misiones de la región N se abren cuando la región N-1 ya no tiene ninguna que ofrecer
- una misión queda bloqueada, mostrando su primer requisito pendiente, hasta que su receta de recompensa se pueda fabricar

Las recetas vienen de las secciones 8.4, 9.2-9.6 y 9.0.2. Las recompensas en **negrita** son los dos conjuntos de equipo (sección 9.6.1).

**Cantidades:** los pedidos apilables usan los valores de la sección 11.4: **50** en el rango dos, **100** en el tres y **150** en el cuatro. Los pedidos de herramientas y prendas son instancias únicas x1, así que usan un valor menor definido: **20 / 30 / 40** (la sección 11.4 permite valores menores definidos; el motivo es la carga de fabricación x1). Los requisitos de otra rama se anotan como en el rango uno (por ejemplo G3 necesita la receta Pine bird de L2).

**Reglas de orden que se mantienen:**
- la herramienta Normal antes que su Élite
- los refinados del rango anterior antes de las mejoras Élite que los consumen (sección 8.4)
- todo Conjunto A antes que el Conjunto B del mismo espacio
- cada bloque termina los refinados que necesitan las Élite del bloque siguiente: Birch planks, Fine beaver pelts y Fine flax cloth en el rango dos; Mesquite planks, Turkey feather weave y Fine cotton cloth en el rango tres

#### 11.6.1 Misiones puente del rango uno (regiones 3-4 para construcción; región 6 para los demás puentes)

Las mejoras Élite del rango dos consumen Pine planks, Fine rabbit pelts y Fine hemp cloth, y los primeros niveles de casa necesitan Fired bricks y Limestone tiles. Fundir cobre y estaño necesita Pine charcoal, y varias recetas del rango dos necesitan Hemp thread y Wheat flour. Ninguna de estas recetas la desbloquean las 22 misiones implementadas del rango uno. Diez unidades cada una:

| Code | Request | Reward recipe | Note |
| --- | --- | --- | --- |
| G6 | 10 Wheat | Wheat flour | Pantry flour for later Food |
| L6 | 10 Pine logs | Pine charcoal | Mission region 3; before L7; fuel for bricks and later smelting |
| H5 | 10 Rabbit leather | Cured rabbit pelt | Needs Rock salt (region 3) |
| H6 | 10 Cured rabbit pelts | Fine rabbit pelt | Needs Hemp thread (E9) |
| E9 | 10 Hemp cord | Hemp thread | |
| E10 | 10 Hemp thread | Fine hemp cloth | |
| L7 | 10 Clay | Fired bricks | Mission region 3; needs Pine charcoal (L6); house levels 1-2 |
| L8 | 10 Limestone | Limestone tiles | Mission region 4 (house level 3) |

**Corrección de progresión, 6 de noviembre de 2026 (diseño; revisión de implementación pendiente):** L6 y L7 van en región de misión 3, en ese orden; L8 en región 4. G6, H5, H6, E9 y E10 siguen en región 6. Los códigos son IDs estables, no claves de orden. Lumberjack del rango uno ejecuta **L1 -> L2 -> L3 -> L4 -> L6 -> L7 -> L8 -> L5**, seguido del rango dos. L5 (Mortar) sigue en región 6. Conservar el orden inicial y las dependencias entre ramas; no ordenar globalmente por código ni región.

La primera casa queda accesible por sus fuentes al llegar al asentamiento tras región 4: G4 Pine planks; L6 Pine charcoal de ramas de región 1; L7 Fired bricks de Clay de región 3. L8 usa Limestone de región 4 para la futura casa nivel 3. La casa nivel 2 sigue esperando Reed braid de región 6 y la nivel 3 sus refinados de rango dos. No hace falta regalo ni salto de depuración para demostrar esta ruta de diseño.

**Orden y migración:** definir ordinal/lista de IDs por rama separado del código. Mantener IDs y cantidades. Validar el guardado antiguo con su catálogo original; conservar recompensas y misión activa/progreso. Una activa migrada puede terminar antes de nuevos predecesores; después ofrecer la primera pendiente del orden nuevo. No completar misiones nuevas por deducción ni repetir recompensas/cobros. Versionar la migración y aplicarla una vez; los guardados nuevos usan el orden nuevo.

**Pendiente:** demostrar acceso por fuentes a carbón -> ladrillos -> primera casa al llegar tras región 4, sin saltos; conservar las puertas posteriores de Mortar, Reed braid y rango dos. Revisar cambios de rama y guardados anteriores/posteriores a L5 con activa y entregas parciales. Mantener pilas completas y herramientas de prueba. [Nota para revisión de lógica y pruebas](Docs/PROJECT_HANDOFF.md#logic-review).

#### 11.6.2 Rango dos (regiones 7-12; 50 unidades, 20 instancias)

**Rango dos, lote 26 entregado (7 de noviembre de 2026; arte pasivo):** Honey brioche, Blackberry cream pastry tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 868 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-26). Progreso 26/45 lotes de arte; cobertura 80/82, faltan 2 diseños. Trabajo detenido al completar el lote 26; no iniciar el lote 27 hasta que el usuario lo solicite.

**Rango dos, lote 25 entregado (7 de noviembre de 2026; arte pasivo):** Honey-roast duck, Duck blueberry rice tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 866 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-25). Progreso 25/45 lotes de arte; cobertura 78/82, faltan 4 diseños. Trabajo detenido al completar el lote 25; no iniciar el lote 26 hasta que el usuario lo solicite.

**Rango dos, lote 24 entregado (7 de noviembre de 2026; arte pasivo):** Painted birch bird, Fishing charm, Beeswax candle, Sandstone owl tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 862 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-24). Progreso 24/45 lotes de arte; cobertura 76/82, faltan 6 diseños. Trabajo detenido al completar el lote 24; no iniciar el lote 25 hasta que el usuario lo solicite.

**Rango dos, lote 23 entregado (7 de noviembre de 2026; arte pasivo):** Copper pendant, Birch beaver sculpture, Beaver toy, Lavender pouch tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 858 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-23). Progreso 23/45 lotes de arte; cobertura 72/82, faltan 10 diseños. Trabajo detenido al completar el lote 23; no iniciar el lote 24 hasta que el usuario lo solicite.

**Rango dos, lote 22 entregado (7 de noviembre de 2026; arte pasivo):** Pike barley cakes, Walleye-barley soup, Carp rice bowl, Pike rice bowl, Catfish-tomato stew tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 853 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-22). Progreso 22/45 lotes de arte; cobertura 68/82, faltan 14 diseños. Trabajo detenido al completar el lote 22; no iniciar el lote 23 hasta que el usuario lo solicite.

**Rango dos, lote 21 entregado (7 de noviembre de 2026; arte pasivo):** Blackberry-glazed beaver, Beaver-carrot stew, Rabbit barley soup, Beaver mushroom pot, Creamy truffle-barley pot tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 848 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-21). Progreso 21/45 lotes de arte; cobertura 63/82, faltan 19 diseños. Trabajo detenido al completar el lote 21; no iniciar el lote 22 hasta que el usuario lo solicite.

**Rango dos, lote 20 entregado (7 de noviembre de 2026; arte pasivo):** Bottled honey tea, Carrot soup, Barley bread, Tomato rice bowl, Honey-glazed carrots tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 791 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-20). Progreso 20/45 lotes de arte; cobertura 58/82, faltan 24 diseños. Trabajo detenido al completar el lote 20; no iniciar el lote 21 hasta que el usuario lo solicite.

**Rango dos, lote 19 entregado (7 de noviembre de 2026; arte pasivo):** Blackberry juice, Birch sap milk drink, Blueberry jam, Honey herbal tea, Honey oat drink tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 786 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-19). Progreso 19/45 lotes de arte; cobertura 53/82, faltan 29 diseños. Trabajo detenido al terminar el lote 19 por instrucción del usuario; el lote 20 no se inició. Reanudar solo cuando lo solicite.

**Rango dos, lote 18 entregado (7 de noviembre de 2026; arte pasivo):** Taiga frame pack tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 783 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-18). Progreso 18/45 lotes de arte; cobertura 48/82, faltan 34 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 17 entregado (7 de noviembre de 2026; arte pasivo):** Taiga waxed shirt, Taiga trousers tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 777 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-17). Progreso 17/45 lotes de arte; cobertura 47/82, faltan 35 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 16 entregado (7 de noviembre de 2026; arte pasivo):** Taiga fur hat, Taiga boots tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 771 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-16). Progreso 16/45 lotes de arte; cobertura 45/82, faltan 37 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 15 entregado (7 de noviembre de 2026; arte pasivo):** Marsh pack tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 768 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-15). Progreso 15/45 lotes de arte; cobertura 43/82, faltan 39 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 14 entregado (7 de noviembre de 2026; arte pasivo):** Flax work shirt, Flax trousers tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 762 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-14). Progreso 14/45 lotes de arte; cobertura 42/82, faltan 40 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 13 entregado (7 de noviembre de 2026; arte pasivo):** Beaver-fur cap, Marsh boots tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 756 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-13). Progreso 13/45 lotes de arte; cobertura 40/82, faltan 42 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 12 entregado (7 de noviembre de 2026; arte pasivo):** Normal birch rod, Elite birch rod tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 754 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-12). Progreso 12/45 lotes de arte; cobertura 38/82, faltan 44 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 11 entregado (7 de noviembre de 2026; arte pasivo):** Normal birch slingshot, Elite birch slingshot tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 752 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-11). Progreso 11/45 lotes de arte; cobertura 36/82, faltan 46 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 10 entregado (7 de noviembre de 2026; arte pasivo):** Normal birch axe, Elite birch axe tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 750 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-10). Progreso 10/45 lotes de arte; cobertura 34/82, faltan 48 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 09 entregado (7 de noviembre de 2026; arte pasivo):** Barley flour, Flax oil, Beeswax tienen escena Blender, OBJ/paletas, prefabs/materiales Unity, vistas revisadas y validación de reimportación. Se conservaron 747 mallas anteriores y el atlas. Enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes; sin cambios de recetas, estadísticas, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-09). Progreso 9/45 lotes de arte; cobertura 32/82, faltan 50 diseños. Continuar en orden hasta que el usuario pida parar.

**Rango dos, lote 08 entregado (7 de noviembre de 2026; arte Material pasivo):** Glass, Glass bottle, Glass panels, Cut quartz y Sandstone tiles tienen escena Blender, OBJ/paletas, prefabs/materiales Unity y vistas revisadas. Pasaron las comprobaciones nativas/reimportadas de geometría y dimensiones y Build/Capture; se conservaron 742 mallas anteriores y el atlas. El vidrio conserva el tratamiento facetado opaco; la arenisca usa grano mineral. IDs, enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes con la otra herramienta. Conservar Pine charcoal del rango anterior, la identidad exacta de Glass bottle y Glass panels como Material. Sin cambios de recetas, tiempos, valores, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-08). Cobertura: 29/82, faltan 53; lotes 01-08 completos para arte, 37 planificados. Lote 09 (Barley flour, Flax oil, Beeswax) no iniciado.

**Rango dos, lote 07 entregado (6 de noviembre de 2026; arte Material pasivo):** Copper ingot, Copper fittings y Tin ingot tienen escena Blender, OBJ/paletas, prefabs/materiales Unity y vistas revisadas. Pasaron las comprobaciones nativas/reimportadas de geometria y dimensiones y Build/Capture; se conservaron 739 mallas anteriores y el atlas. IDs, enlaces activos y pruebas actuales de juego/dispositivo siguen pendientes con la otra herramienta. Ambos lingotes requieren Pine charcoal del rango anterior; fittings consume un Copper ingot para tres resultados, aunque T2-L4 solicita Copper ore para desbloquear. Sin cambios de recetas, tiempos, valores, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-07). Cobertura: 24/82, faltan 58; lotes 01-07 completos para arte, 38 planificados. Lote 08 no iniciado.

**Rango dos, lote 06 entregado (6 de noviembre de 2026; arte Material pasivo):** Flax cloth, Flax cord, Flax stuffing, Fine flax cloth y Cattail cord tienen escena Blender, OBJ/paletas, prefabs/materiales Unity y vistas revisadas. Pasaron las comprobaciones nativas/reimportadas y Build/Capture; se conservaron 734 mallas anteriores y el atlas. IDs, recetas activas y pruebas actuales de juego/dispositivo siguen pendientes con la otra herramienta. T2-E4 solicita cord para desbloquear stuffing, pero la receta consume Flax fiber; Fine flax cloth requiere Hemp thread. Sin cambios de recetas, tiempos, valores, huellas o guardados. [Archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-06). Cobertura: 21/82, faltan 61; lotes 01-06 completos para arte, 39 planificados. Lote 07 no iniciado.

**Rango dos, lote 05 entregado (6 de noviembre de 2026; arte Material pasivo):** Beaver leather, Cured beaver pelt, Fine beaver pelt y Fine beaver leather tienen escena Blender, OBJ/paletas aislados, prefabs/materiales Unity y vistas revisadas. Pasaron comprobaciones nativas/reimportadas y Build/Capture selectivos. IDs y enlaces de refinados/resultados siguen pendientes; se conservaron recetas, tiempos, valores, huellas, guardados, mallas anteriores y atlas. [Archivo único de relevo](Docs/PROJECT_HANDOFF.md#batch-05). Cobertura de resultados con malla: 16/82, faltan 66. Lotes 01-05 completos para arte; otros 40 planificados. Lote 06 no iniciado.

**Rango dos, lote 04 entregado (6 de noviembre de 2026; arte Material pasivo):** Birch plank, Birch beam, Birch charcoal y Birch panel tienen escena Blender, OBJ/paletas aislados, prefabs/materiales Unity y vistas revisadas. Pasaron comprobaciones nativas/reimportadas y Build/Capture selectivos. IDs de objetos/recetas y enlaces de juego siguen pendientes; se conservaron recetas, tiempos, valores, huellas, guardados, mallas anteriores y atlas. [Archivo único de relevo](Docs/PROJECT_HANDOFF.md#batch-04). Cobertura de resultados con malla: 12/82, faltan 70. Lotes 01-04 completos para arte; otros 41 planificados. Lote 05 no iniciado.

**Rango dos, lote 03 entregado (6 de noviembre de 2026; arte pasivo):** Lavender, Crab crudo y Duck meat tienen escena Blender, OBJ/paletas aislados, prefabs/materiales Unity y vistas revisadas. Pasaron las comprobaciones nativas/reimportadas y Build/Capture selectivos. Fuentes de granja/pescadería/rancho y enlaces de juego siguen pendientes; `ranch.duck_meat` es un ID de diseño, no registro nuevo en ejecución. Se conservaron mallas anteriores, atlas, recetas, valores, fuentes, huellas y guardados. [Archivo único de relevo](Docs/PROJECT_HANDOFF.md#batch-03). Lotes 01-03 completos para arte; otros 42 planificados. Lote 04 no iniciado.

**Rango dos, lote 02 y notas unificadas (6 de noviembre de 2026):** Common carp, Northern pike, Channel catfish y Walleye tienen mallas crudas distintas, paletas aisladas, escena Blender y prefabs/materiales Unity pasivos. Pasaron las comprobaciones nativas/reimportadas y Build/Capture selectivos; IDs, enlaces de juego y pruebas de dispositivo siguen pendientes. Los siete informes anteriores y esta entrega se integran en [un solo archivo de relevo](Docs/PROJECT_HANDOFF.md#batch-02), conservando fechas y limitaciones. Los archivos anteriores solo redirigen. Lotes 01/02 completos para arte, otros 43 planificados; lote 03 no iniciado. No cambiaron recetas, valores, fuentes, huellas, guardados, mallas anteriores ni atlas compartido.

**Rango dos, lote 01 entregado (6 de noviembre de 2026; solo arte pasivo):** [45 lotes reanudables](Docs/Art/Tier02/tier2-production-batches-2026-11-06.md) limitan el trabajo por uso diario. Lote 01: Birch log, Birch sap y Raw beaver meat, escena Blender, tres OBJ/paletas, tres prefabs/materiales Unity y vistas revisadas. [Informe del lote 01](Docs/PROJECT_HANDOFF.md#batch-01). Pasaron las comprobaciones de geometría nativa/reimportada y Build/Capture selectivos; enlaces de juego y pruebas de dispositivo siguen pendientes con la otra herramienta. Birch sap conserva su ID; los IDs de tronco/carne siguen sin resolver. Ocho de 82 resultados tienen malla fuente y faltan 74. Lote 02 (cuatro peces crudos) no iniciado.

**Preparación de diseños restantes del rango dos (6 de noviembre de 2026; solo inventario/plan):** [plan de objetos, recetas y tiendas](Docs/Art/Tier02/remaining-tier2-design-plan-2026-11-06.md). Se inspeccionaron mallas fuente y prefabs: 18 recursos sueltos, 120 elementos decorativos regionales, siete resultados opcionales y cinco objetos especiales del mercader con quince representaciones de inventario/cuerpo. Los 68 resultados de misión son 25 Material, seis Tool, diez Wearable, diecinueve Food y ocho Misc, sin mallas dedicadas encontradas para esos nombres. Con la receta lateral de trufa, once recetas de caja y dos de panadería son 82 resultados: siete con malla y 75 pendientes. Insumos de recolección/tienda, doce muebles de casa nivel 3-4 y mascotas comunes se planifican aparte; los estudios de muebles son proxies. La otra herramienta debe revisar corazones antiguos del manifiesto exclusivo, elegibilidad premium de Roasted crab, orden de T2-G21/T2-H13 e identidad de la misión lateral de trufa. No se cambian arte, valores ni lógica, ni se afirman pruebas Unity/dispositivo. Los modelos de casita de mascotas siguen aplazados según la decisión del 6 de noviembre.

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
| T2-H2 | 7 | Raw beaver meat | Roasted beaver | Hunted with the pine slingshot (one tier above) |
| T2-H3 | 7 | Beaver pelts | Beaver leather | |
| T2-H4 | 7 | Beaver pelts | Cured beaver pelt | |
| T2-H5 | 8 | Cured beaver pelts | Fine beaver pelt | |
| T2-H6 | 8 | 20 Normal birch slingshots | Elite birch slingshot | Fine rabbit pelt (H6) |
| T2-H7 | 8 | Beaver leather | **Flax work shirt (2A shirt)** | Flax cloth (T2-E1) |
| T2-H8 | 8 | Carrots | Beaver-carrot stew | |
| T2-H9 | 9 | Fine beaver pelts | Beaver toy | Flax stuffing (T2-E4) |
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

#### 11.6.3 Rango tres (regiones 13-18; 100 unidades, 30 instancias)

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
| T3-H2 | 13 | Raw turkey meat | Roasted turkey | |
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

#### 11.6.4 Rango cuatro (regiones 19-24; 150 unidades, 40 instancias)

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
| T4-H2 | 19 | Raw boar meat | Roasted boar | |
| T4-H3 | 19 | Boar hides | Boar leather | |
| T4-H4 | 19 | Potatoes | Boar potato stew | |
| T4-H5 | 20 | Boar leather | Fine boar leather | |
| T4-H6 | 20 | 40 Normal oak bows | Elite oak bow | |
| T4-H7 | 20 | Fine boar leather | **Savanna shirt (4A shirt)** | |
| T4-H8 | 20 | Boar tusks | Carved tusk pendant | Boar tusk harvest output (section 9.6.4) |
| T4-H9 | 21 | Raw boar meat | Boar bean chili | |
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
| T4-E9 | 23 | Lapis lazuli | Lapis bracelet | |
| T4-E10 | 23 | Cut sapphire | Sapphire pendant | |
| T4-E11 | 24 | Bamboo fiber | Bamboo cloth | |
| T4-E12 | 24 | Bamboo cloth | **Coastal trousers (4B pants)** | |
| T4-E13 | 24 | 40 Coastal trousers | **Coastal pack (4B backpack)** | |
| T4-E14 | 24 | Red snapper | Coconut snapper curry | |
| T4-E15 | 24 | Cut diamond | Diamond necklace | |

#### 11.6.5 Resumen y comprobaciones

- **Rangos 5-8:** siguen las mismas reglas, sin estadísticas nuevas por encima del rango cuatro: [Parte 2](PROJECT_RECOMENDATIONS_PART_2_ES.md), secciones 13-16 (180 misiones, incluidas nueve misiones de procesado de gemas añadidas el 6 de noviembre de 2026).
- **Total:** 8 misiones puente del rango uno más 68 / 69 / 68 misiones en los rangos dos / tres / cuatro (213 nuevas; 235 con las 22 del rango uno). El rango cuatro incluye el lapislázuli pulido, añadido el 6 de noviembre de 2026.
- **Recompensas por rango:**
  - los dos conjuntos de equipo (10 prendas) y las seis herramientas
  - los materiales y refinados del bloque
  - todas las recetas de Food, incluido un plato por especie de pez y las comidas de árbol
  - todas las recetas Misc
- **Densidad de misiones:** unas 0,6-0,85 misiones por campamento. El rango dos tiene 79 campamentos, el tres 95 y el cuatro 108 (sección 6.2.2); el rango uno tiene 22 misiones en 70 campamentos. Cada región sigue abriendo un grupo nuevo mediante la puerta por región.
- **Carga:** la última región de cada bloque lleva las prendas del Conjunto B, así que el bloque termina con su mejor equipo. Coral Shores, en el rango cuatro, tiene 12 misiones.
- **Comprobaciones automáticas (cuando se implemente):**
  - cada pedido tiene un origen no posterior a su región de misión
  - cada recompensa se puede fabricar cuando se ofrece
  - la herramienta Normal antes que su Élite, y el refinado del rango anterior antes que su Élite
  - el Conjunto A antes que el Conjunto B en cada espacio
  - ninguna recompensa repetida
  - toda receta de los rangos 2-4 es recompensa de misión, salvo que sea premium de tienda o caja (sección 9.6.6)
  - las cuatro especies de pez de cada rango tienen misión de recolección
  - cada valor de los Conjuntos A y B como en la sección 9.6.1
  - pedidos de instancias de 20/30/40
  - comida de misión de 6 corazones como máximo

## 12. Campamentos, comercio, monedas y viajes

### 12.1 Campamentos y comercio mediante cinta transportadora

Cada campamento tiene una fogata y cuatro asientos de troncos: un jugador y tres comerciantes de NPC. El NPC de la misión está separado. **Aparición de la dama de las misiones (confirmado el 31 de octubre de 2026):** aparece en el campamento inicial para **elegir la primera misión** y, después, solo en el campamento donde se **completa** la misión activa, para entregar la receta al jugador y ofrecer la siguiente misión; en los demás campamentos no está. El flujo exacto de entrega y elección se define con la implementación de las misiones. La sesión de negociación de 15 segundos es una oportunidad por visita al campamento. Irse y regresar crea una nueva oportunidad según lo solicitado; reiniciar la aplicación durante la misma visita no lo hace.

**Flujo de misiones (confirmado el 31 de octubre de 2026):** en el primer campamento, tocar a la dama alta abre el **menú de selección de misiones**: una pila de papeles desgastados con separadores (como los del libro de recetas), una pestaña por rama (Recolector, Leñador, Cazador, Explorador) con la siguiente misión de cada una, lo que pide, la receta de recompensa y, si aún no es posible, el motivo. Después, al llegar al campamento donde la misión activa está completa, la dama **entrega la receta automáticamente** y el menú **se abre solo**; **se cierra solo** al elegir la siguiente misión. Los objetos que se llevan encima se entregan automáticamente a la misión activa (solo lo que falta: una pila de 1.000 queda en 990); la ropa puesta nunca se entrega y los objetos que aún se están fabricando no cuentan hasta estar listos. **Las manzanas se pueden comer (31 de octubre de 2026):** las manzanas son comida cruda de **1 corazón** que se come manualmente desde la mochila para ayudar con la resistencia; como la otra comida cruda, **nunca se comen automáticamente** para evitar el desmayo (solo la comida preparada). Siguen siendo el objeto de la misión G2 y el ingrediente del jugo de manzana. **Primera misión (decidido el 31 de octubre de 2026):** la primera selección ofrece solo la misión G1 del Recolector (ramas de pino -> receta de tronco de pino), para que el inicio siempre siga G1 -> G2 -> jugo de manzana antes del primer tramo que necesita comida. **Desbloqueo por región de misión:** las misiones se abren por región de misión, no por las regiones a las que ya llegó el jugador: primero las de la región uno; cuando ya no queda ninguna misión de la región uno por ofrecer, se abren las de la región dos aunque el jugador aún no haya llegado allí, y así sucesivamente. El desbloqueo solo avanza, de modo que siempre hay una misión que elegir (y siempre una misión activa) mientras queden misiones. Las dependencias de recetas, herramientas o asentamientos siguen bloqueando misiones concretas, mostrando el motivo.
Cada uno de los NPC

Cada NPC ofrece 3-5 entradas con semilla persistente de recursos/herramientas/ropa de regiones descubiertas. Cada compra apilable transfiere una unidad y cada equipo una instancia. **Confirmado:** aplicar descuentos por posición; curva inicial `buyMultiplier = 1.1 - 0.3 * normalizedBeltPosition`: 110% al entrar y 80% antes de la trituradora. Cotizar según el perfil real y redondear solo al pagar. Conservar calidad/perfil y curva al reabrir/recargar.

Herramientas/ropa/mochilas generadas por NPC siempre tienen **nivel de receta de creación uno**, calidad Q1-Q5 aleatoria con semilla (distribución uniforme propuesta), condición completa y cero bonificaciones de muebles. Calcular precio/durabilidad base de nivel uno y prima de calidad real una vez; no usar dominio ni muebles del jugador. Comprarlas no otorga recetas/dominio. Las ofertas del jugador en cinta conservan nivel/calidad/durabilidad/perfil reales. Tiendas normales de asentamiento/globo siguen Q5: la calidad aleatoria es excepción del NPC de cinta.

**Bloqueo comercial confirmado:** la sesión del transportador dura 15 segundos. Durante el mismo, solo están disponibles las acciones comerciales y el botón dedicado Devolver/cancelar. No permita la elaboración, comida/bebida, cambios de equipo, interacción con misiones, cosechas, mapas, viajes a asentamientos o paneles de configuración/inventario general. No hay pausa de superposición de detalles ni extensión del temporizador controlada por el usuario. Los detalles de los artículos específicos del comercio deben permanecer sin bloqueo y mantener el cronómetro en marcha.
Reglas de liquidación del comercio:

- Compras de ofertas del jugador por parte de NPC: conserve el precio de venta cotizado del 100% recomendado para la cantidad indicada.
- Una oferta del jugador que llega a la trituradora de forma natural: conserva el 80% de respaldo del diseño original. Los elementos propiedad de NPC no le dan nada al jugador cuando se destruyen.
- Vencimiento natural de 15 segundos: se recomienda liquidar las ofertas del jugador restantes sin vender en un 80 % y luego marcar la sesión de visita como finalizada. Esta alternativa de fin de sesión es una recomendación; es independiente de la penalización por cancelación confirmada.
- **Cancelación de devolución en cualquier momento antes del vencimiento:** detenga la animación, venda inmediatamente todos los listados propiedad de jugadores aún sin vender en el transportador por **70% de su valor de venta cotizado sin descuento**, elimine esos listados y regrese a la interfaz de usuario normal del campamento. Las cantidades que nunca figuran en la lista permanecen intactas. Los listados ya vendidos/triturados mantienen su resultado liquidado; no vuelvas a pagar ni cambiar el precio. Los listados propiedad de NPC no generan dinero para los jugadores.
- La cancelación es un resultado terminal, al igual que la finalización natural. Esa visita al campamento no puede volver a intercambiarse hasta que el jugador se vaya y regrese, o llegue a otro campamento. Cerrar el panel de comercio, reabrir menús, regresar desde un acceso directo a casa sin finalizar la visita al campamento o reiniciar la aplicación no puede restablecerlo.

Una salida realmente completa hacia el asentamiento u otro lugar del sendero finaliza la visita al campamento; regresar crea una nueva visita según sea necesario. Simplemente abrir un menú de viaje o cancelar una transición no cuenta como salida. El regreso en la pantalla de intercambio finaliza el intercambio y regresa a la interfaz de usuario del campamento; él mismo no abandona el campamento ni aprovecha la oportunidad.

Persistir `TradeSessionState` (No iniciado/Activo/Completado/Cancelado), duración restante, propiedad y el ID/motivo del asentamiento de cada listado. Confirme los pagos de cancelación y el estado Cancelado juntos antes de salir de la pantalla. Serializa la caducidad, las compras de NPC, los eventos de destrucción y la devolución para que un artículo se liquide una vez incluso cuando ocurran en el mismo marco. Si ya han transcurrido los 15 segundos, finalizar el recorrido de caducidad natural; una Devolución tardía no podrá liquidarlo nuevamente.

Ejemplo de cancelación: una lista de jugadores con un valor de 100 en efectivo aún no se ha vendido, otra con un valor de 200 ya se ha vendido y una lista de NPC todavía está en el cinturón. La devolución agrega 70 en efectivo solo por el listado de jugadores del juego no vendido. No paga el listado de NPC ni vuelve a pagar el listado ya vendido. La oportunidad comercial se gasta incluso si el jugador no tenía anuncios sin vender que liquidar.

**Confirmado:** la resistencia sigue regenerándose durante toda la animación de comercio, también al colocar o comprar ofertas. A 0.25 corazones/segundo, 15 segundos activos recuperan hasta 3.75 corazones; cancelar a los cinco recupera hasta 1.25, sujetos al máximo actual. El bloqueo de entrada no detiene la regeneración ni la finalización de fabricaciones en segundo plano. Posponer avisos ajenos al comercio hasta que termine. Al suspender la aplicación, congelar y guardar tanto el tiempo restante como la regeneración en primer plano, y continuar la misma sesión al volver; nunca conceder otra oportunidad nueva de 15 segundos.

### 12.2 Definiciones de monedas y cálculos

Utilice dos carteras de números enteros con iconos e ID distintos:

- `Cash`: comercio de campamentos, peajes en ríos, tiendas básicas, mejoras/muebles para el hogar cuando se especifique.
- `Gold`: desbloqueos de rango de madera de asentamiento, stock de globos, productos de panadería premium, desbloqueos seleccionados a largo plazo.
- `GoldOre` y `GoldIngot`: definiciones de elementos; Nunca mute la billetera Gold al fundir.

Almacene la moneda como unidades menores enteras y saldos/totales de 64 bits. **Cada precio a continuación es por unidad de artículo, no por pila.** Conserve los valores unitarios intermedios fraccionarios en forma de punto fijo y redondee una vez en la transacción final, no una vez por modificador/unidad. Recomendación: mostrar los totales de efectivo redondeados hacia abajo; La unidad de material Q1 ordinaria más barata es un efectivo. Instantánea del valor intrínseco en la creación de artesanías, nunca recalcule el precio de un artículo antiguo a partir del último nivel de receta/muebles del jugador.

### 12.2.1 Referencias de precios, orden de categorías y alcance

**Pedido de precio base confirmado:** Misc (esculturas/juguetes/joyas) > Mochilas > Ropa > Herramientas > Materiales avanzados > Comida > Otros materiales/ingredientes > Básicos. Las mochilas son recetas portátiles para reglas de temporizador/durabilidad, pero tienen su propia categoría de precio más alto. La leche cruda/los huevos/la fibra/el mineral pueden ser Otros ingredientes; el queso/las comidas/bebidas preparadas son alimentos; Los contenedores de tela/cuero/vigas/vidrio refinados pueden ser materiales avanzados. La función de sincronización de una receta y su categoría de precio son campos explícitos separados.

**Anclas confirmadas:** un efectivo para el material Q1 de rango uno más simple y 6,000 efectivo como base equivalente Q1 de la referencia Miscelánea de nivel diez, antes de la calidad/durabilidad final. Una fabricación de un objeto no material de nivel diez normalmente tiene al menos Q5, por lo que equivalente a Q1 significa una base de cálculo, no un producto Q1 de nivel diez que se puede obtener de forma natural. El precio final puede superar los 6,000. Varios no tienen prima de durabilidad.

Distribución recomendada: coloque la referencia 6,000 en el rango cuatro activa y escale los productos de referencia del rango anterior debajo de ella. Esta ubicación/espaciado de rango es una propuesta, no un requisito de que todas las recetas misceláneas tengan el mismo precio. Seis regiones por rango de recursos no introducen rangos de precios adicionales; los valores por receta pueden variar dentro de ese bloque.

| Categoría de precio | referencia de rango 1 | referencia de rango 2 | referencia de rango 3 | referencia de rango 4 | Interpretación de dominio |
| --- | --- | --- | --- | --- | --- |
| Varios | 600 | 1,800 | 3,600 | **6,000** | Referencia equivalente a Q1 en el nivel de receta 10 |
| Mochila | 350 | 1,050 | 2,100 | 3,500 | Referencia equivalente a Q1 en el nivel de receta 10 |
| Ropa | 220 | 660 | 1,320 | 2,200 | Referencia equivalente a Q1 en el nivel de receta 10 |
| Herramienta | 140 | 420 | 840 | 1,400 | Referencia equivalente a Q1 en el nivel de receta 10 |
| Material avanzado | 100 | 300 | 600 | 1,000 | Referencia de material fija, **sin niveles de receta** |
| Comida/bebida preparada | 60 | 180 | 360 | 600 | Referencia equivalente a Q1 en el nivel de receta 10 |
| Otro material/ingrediente | 20 | 60 | 120 | 200 | Referencia de material/ingrediente fijo |
| Material básico más sencillo | **1** | 3 | 6 | 10 | Referencia fija de materia prima |

Estos son valores de referencia de categoría, no un comando para hacer que cada material cueste 100 o cada comida cueste 60. Cada definición exacta de artículo obtiene su propia base de autor, conservando el orden previsto para productos de referencia de rango comparable. Conceptos básicos de ejemplo: rama de pino de rango uno/fibra de ortiga/guijarro común pueden comenzar cada uno con un dinero en efectivo; Los troncos de pino pueden ser tres y las tablas de pino nueve, lo que refleja las conversiones 3:1. Telas finas, vigas y contenedores ocupan la banda avanzada en lugar de forzar el ingreso de cada material procesado a ella. Elaborar recetas económicas junto con sus aportaciones exactas y señalar pérdidas involuntarias de valor artesanal en los informes; no cambie las identidades de los ingredientes ni las recogidas completas para ocultarlas.

Compare la jerarquía en condiciones base y rango equivalente. Los bienes de rango más alto y las bonificaciones de calidad/durabilidad pueden superponerse a categorías de rango más bajo; no limite globalmente todas las herramientas por debajo de la ropa más barata o todos los precios finales por debajo de 6,000. Las referencias propuestas también mantienen Misc de rango cuatro igualmente máxima por encima de una mochila con las bonificaciones de prueba de durabilidad actuales de Q5/+20 %.

### 12.2.2 Precio base según nivel de receta

Para una receta no material, cree su `PriceReferenceAtLevel10` y luego calcule:

`Pbase(L) = PriceReferenceAtLevel10 * (L + 35) / 45`, para niveles 1-10.

Por lo tanto, el nivel uno tiene el 80% de su referencia de nivel diez, y cada paso de dominio aumenta la base equivalente a Q1 hasta el nivel diez. Mantenga internamente la aritmética exacta racional/de punto fijo. Los materiales/ingredientes tienen un `MaterialBasePrice` fijo; no ganan niveles simplemente para ajustarse a esta tabla de precios. Los porcentajes siguen siendo datos de prueba editables, separados de los anclajes confirmados de 6,000/un efectivo y el orden de categoría.

### 12.2.3 Incrementos de precio por calidad final y durabilidad

Multiplicadores de precios de calidad final propuestos:

| Calidad final | Multiplicador de precio intrínseco |
| --- | --- |
| Q1 | 1.00 |
| Q2 | 1.15 |
| Q3 | 1.35 |
| Q4 | 1.60 |
| Q5 | 2.00 |

1. Calcule la Q terminada a partir del dominio de recetas más muebles que mejoran la calidad de fabricación, con un límite de cinco, como se define en la sección 6.1. La calidad del material sigue siendo únicamente de muebles para la producción de material artesanal.
2. Aplicar la prima de precio una vez: `Pquality = Pbase * qualityPriceMultiplier(Q)`.
3. Para herramientas, ropa y mochilas de fabricación finita, aplique un multiplicador de valor de durabilidad: `1 + qualityDurabilityBonus(Q) + U`. Esto equivale a la durabilidad adicional elaborada en relación con la **base ajustada por nivel**, utilizando proporciones de estadísticas no redondeadas. `Pintrinsic = Pquality * (1 + qualityDurabilityBonus(Q) + U)`.
4. Misc/Food/materials/ingredientes no tienen durabilidad, por lo que `Pintrinsic = Pquality`; no les dé durabilidad ficticia ni la prima por herramienta/vestible.
5. Conservar el valor unitario intrínseco, el grado, el nivel de receta, el precio base y el desglose exacto del modificador en el perfil de estadísticas de salida.

La calidad tiene dos efectos intencionales en las artesanías duraderas: una mayor calidad aumenta el valor intrínseco y la vida útil adicional resultante también agrega valor. No agregue accidentalmente el **mismo** efecto dos veces: sin capa de precio directa `+5% per C`, sin segundo multiplicador U, sin segundo multiplicador calidad-precio y sin multiplicación por `Dfinal / D0` que repite el crecimiento de durabilidad basado en niveles ya reflejado por la curva de nivel base. Los muebles que mejoran la calidad de fabricación actúan a través del acabado Q; una vez que se alcanza el Q5, no se otorga ningún rango de precio adicional oculto. Los muebles de durabilidad de la categoría correspondiente aún aumentan tanto la vida útil como el precio adicional de durabilidad.
Las herramientas Normal/Élite mantienen referencias de precio/durabilidad base creadas por el autor; una referencia Élite puede reflejar una vida más larga, ahorros de acción y capacidad de botín. No inventes otro multiplicador Élite automático. El equipo comercial de durabilidad infinita utiliza su precio de autor. Esto define el valor del artículo nuevo; cualquier descuento por condición de equipo usado es independiente y nunca muta el perfil de artesanía original. La economía de las recetas también incluye el combustible de madera listado explícitamente por porción/lote, no sólo carne/pescado; El estatus culinario global no significa que un ingrediente esté libre.

Ejemplos resueltos antes de los modificadores de ubicación de venta:

| Artículo | Base equivalente a Q1 | Terminado Q | Factor de precio de durabilidad | Valor unitario intrínseco |
| --- | --- | --- | --- | --- |
| Material de progresión básica de rango uno | 1 | Q1 | Ninguno | **1 efectivo** |
| Progresión de nivel diez rango cuatro Referencia miscelánea | 6,000 | Q5 (2.00) | Ninguno | **12,000 en efectivo** |
| Referencia de mochila de rango cuatro de nivel diez | 3,500 | Q5 (2.00) | 1 + 0.40 + 0.20 = 1.60 | **11,200 en efectivo** |
| Referencia de ropa de rango cuatro de nivel diez | 2,200 | Q5 (2.00) | 1.60 | **7,040 efectivo** |
| Referencia de herramienta de rango cuatro de nivel diez | 1,400 | Q5 (2.00) | 1.60 | **4,480 efectivo** |
| Material avanzado, base fija | 100 | Q3 (1.35) | Ninguno | **135 en efectivo** |
| Progresión de nivel diez, referencia de alimentos de rango cuatro | 600 | Q5 (2.00) | Ninguno | **1,200 en efectivo** |

### 12.2.4 Ventas, pilas e inflación durante las pruebas

Para ventas ordinarias, aplicar muebles de precio de venta solo a fabricación elegible del jugador: `quotedSaleTotal = sum(conditionAdjustedIntrinsicValue) * (1 + eligibleSaleBonus)`. Otros orígenes usan bonificación cero. Después aplicar una sola vez factor de compra NPC/respaldo 80%/cancelación 70%. El almacén usa `sum(conditionAdjustedIntrinsicValue)` al 100%, con bonificación de venta **cero** y ciclo separado de oro (1 de oro por cada 1.000 de efectivo, sección 12.2.4). Conservar precisión de punto fijo y redondear efectivo solo al pagar; usar efectivo realmente pagado como base del ciclo. Conservar primas de creación/calidad/durabilidad del perfil sin recalcular. Vender nunca otorga dominio.

Una pila completa de Misc Q5 de 1000 unidades con una referencia base de 6000 vale **12,000,000 de efectivo** antes de los bonificaciones de venta, no 6000 en efectivo por toda la pila. Una pila completa de conceptos básicos de un solo efectivo vale 1000. Estos grandes totales son consecuencias deliberadas de pruebas mecánicas confirmadas. Mantenga las recolecciones de 1000 unidades sin cambios, admita totales de 64 bits y pantallas de interfaz de usuario/libro mayor, e informe el rendimiento extremo de dinero/oro para una revisión posterior del balance. Los precios de las tiendas de oro siguen siendo elaborados por separado; Los minerales/lingotes de oro nunca se convierten directamente en moneda.

Las ofertas de la tienda deben cotizar su **producto Q5** desde el principio. Cualquier margen de beneficio creado o precio fijo del oro es independiente del valor de reventa intrínseco. No muestre un precio Q1 y aplique secretamente Q5 después de la compra, ni multiplique un perfil de tienda que ya tiene Q5 por el factor Q5 por segunda vez. Con el factor propuesto, un material básico con base Q1 de un efectivo tiene un valor intrínseco Q5 de dos efectivo antes del margen de beneficio de la tienda; el ancla de la base de un solo efectivo permanece sin cambios para los bienes del Q1 provenientes de otras fuentes. La calidad de la tienda no altera las cantidades completas ni otorga dominio. Los listados de comercio de campamentos se cotizan según su propia calidad/perfil real y no se convierten en Q5 en el momento de la transferencia.

**Liquidación confirmada del almacén:** cada caja completa de diez unidades paga **100% del valor de venta en efectivo de lo depositado**, sin bonificación de venta de muebles, categoría, pago adicional ni denominador de conversión efectivo/oro. Registrar el efectivo realmente pagado en cada ciclo de diez cajas. Al completar la décima, otorgar `gold = floor((cycleCash + cashRemainder) / 1000)`; conservar como entero `cashRemainder = (cycleCash + cashRemainder) mod 1000` y reiniciar efectivo/contador atómicamente. **Revisado el 31 de octubre de 2026 (antes 3%):** el cambio es **1 de oro por cada 1.000 de efectivo**; la mecánica no cambia (el oro solo llega tras cada diez cajas completadas, sin bonificaciones). Ejemplo: diez cajas con 10.000 de efectivo producen **10 de oro**; 12.750 producen 12 de oro y guardan 750. Conservar el resto evita pérdida repetida por redondeo; sin recompensa ni dominio adicionales.

**Solo números enteros (confirmado el 31 de octubre de 2026):** toda cantidad de efectivo u oro que recibe el jugador es un número entero. Los valores internos por unidad pueden conservar precisión de punto fijo, pero cada pago (venta en campamento, respaldo del 80%, cancelación del 70%, caja del almacén, ciclo de oro) se redondea hacia abajo una sola vez, en la transacción final; el almacén guarda su resto de efectivo como entero. Los monederos nunca tienen fracciones.

No reequilibre las grandes recompensas causadas por las pruebas de pila completa. Agregue registros de transacciones e informes de compras, artesanías, costos de desgaste, ventas de cajas, conversión de oro e ingresos de rutas repetibles. Luego compare los resultados netos para detectar ciclos de compra-venta/almacén-venta o destrucción que generen valor libre de riesgos. Se deben medir las repetidas tiradas gratuitas y los atajos de ferry/mapa antes de cambiar el comportamiento requerido.

### 12.3 Ríos, mapas y viaje rápido

**Aspecto del mapa de campamentos (confirmado el 31 de octubre de 2026):** el mapa parece un mapa de papel real y desgastado, con una hoja temática por región. El fondo muestra el **camino** que sube desde el primer campamento de la región hasta el río y el ferri; cada campamento es un **botón con un icono de fogata y su nombre al lado**, colocado en el lado del camino donde está el campamento. **Los campamentos no descubiertos se muestran como botones inactivos**; el campamento actual aparece resaltado. La hoja muestra arriba los iconos de los tres recursos destacados de la región; las demás regiones descubiertas son otras páginas (viajar a ellas lleva a su primer campamento). **Llanuras del Alba tiene ya sus diez campamentos.**

- Las fronteras de la región terminan en un campamento, con un río, un ferry y un NPC de peaje justo afuera.
- Paga cada peaje fronterizo una vez. Persiste en el desbloqueo antes de cambiar de escena; La falta de efectivo deja al jugador en el campamento con opciones para intercambiar o volver a visitar lugares descubiertos.
- **Tabla de peajes del ferry (propuesta, 31 de octubre de 2026):** los peajes suben con fuerza para que cada nuevo rango sea una verdadera meta de ahorro; el cruce que sale del rango tres llega a unos 2,2 millones de efectivo. La alternativa en oro sigue el cambio del almacén: `goldToll = max(1, floor(cashToll / 1000))`. La región 24 no tiene ferry de salida.

| Cruce desde la región | Peaje en efectivo | Peaje en oro |
| --- | --- | --- |
| 1 -> 2 | 500 | 1 |
| 2 -> 3 | 1.000 | 1 |
| 3 -> 4 | 2.000 | 2 |
| 4 -> 5 (el asentamiento está entre ambas) | 4.000 | 4 |
| 5 -> 6 | 8.000 | 8 |
| 6 -> 7 (al rango 2) | 15.000 | 15 |
| 7 -> 8 | 25.000 | 25 |
| 8 -> 9 | 40.000 | 40 |
| 9 -> 10 | 60.000 | 60 |
| 10 -> 11 | 90.000 | 90 |
| 11 -> 12 | 130.000 | 130 |
| 12 -> 13 (al rango 3) | 180.000 | 180 |
| 13 -> 14 | 250.000 | 250 |
| 14 -> 15 | 400.000 | 400 |
| 15 -> 16 | 650.000 | 650 |
| 16 -> 17 | 1.000.000 | 1.000 |
| 17 -> 18 | 1.500.000 | 1.500 |
| 18 -> 19 (al rango 4) | 2.200.000 | 2.200 |
| 19 -> 20 | 3.000.000 | 3.000 |
| 20 -> 21 | 4.000.000 | 4.000 |
| 21 -> 22 | 5.000.000 | 5.000 |
| 22 -> 23 | 6.500.000 | 6.500 |
| 23 -> 24 | 8.000.000 | 8.000 |

Razonamiento: los primeros cruces cuestan unas pocas pilas completas de materiales básicos o un puñado de objetos fabricados, así que nadie se queda atascado al principio; en el rango tres un cruce equivale a cientos de los mejores objetos fabricados, en línea con los ingresos de pilas completas al final del juego (una pila completa de Misc de rango tres, nivel diez y Q5 vale más de 7 millones de efectivo). Valores propuestos para el reequilibrio posterior.
- En el mapa del asentamiento, muestra todas las regiones descubiertas y la siguiente silueta no descubierta. Sólo las regiones descubiertas son destinos de viaje; llegar al primer campamento de esa región.
- **Distribución confirmada del mapa del campamento:** mostrar nombre/número de la región actual, marcador del campamento actual y sus tramos/campamentos; permitir elegir destinos ya desbloqueados y mostrar los bloqueados deshabilitados. **Debajo del primer campamento de la sección de esa región**, listar todas las otras regiones descubiertas anteriormente como destinos regionales. Usar el historial de descubrimiento, incluida una región de número mayor descubierta antes de volver a una anterior; no filtrar solo números menores. Distinguir las entradas regionales de los botones de campamento. Se propone viaje gratuito durante las pruebas.
- **Selección dentro de la región:** llegar exactamente al campamento desbloqueado elegido, incluso uno posterior visitado antes de retroceder. Elegir un tramo usa su campamento de entrada definido y desbloqueado; no aparece a mitad de camino ni desbloquea el siguiente campamento. Elegir el campamento actual no produce viaje, nueva visita/oportunidad comercial ni renovación de existencias.
- **Selección de otra región:** llegar al **primer campamento** de la región descubierta elegida, nunca al último visitado ni a un tramo arbitrario. Actualizar el mapa a esa región y sus campamentos desbloqueados, con las otras regiones descubiertas debajo de su primer campamento. Ejemplo: región 12 campamento 8 -> región 12 campamento 3 llega al 3; región 12 -> región 7 descubierta llega a región 7 campamento 1, aunque antes se visitara el 6.
- Validar región y campamento exacto desbloqueados antes de confirmar la transacción; descubrir una región no desbloquea todos sus campamentos. Guardar destino/punto de llegada e ID de transición; una carga interrumpida reanuda ese destino una sola vez. Actualizar región/campamento actual y `lastTrailCampId` atómicamente al llegar. No viajar durante comercio exclusivo o recuperación de desmayo; el viaje no gasta resistencia/durabilidad de marcha, conserva la comida pendiente y nunca evita un peaje sin pagar.
- Viajar hacia atrás según el mapa no permite caminar hacia atrás a lo largo de un segmento y no evita la frontera de un río bloqueado.
- Graba `lastTrailCampId` por separado de la vista de casa/asentamiento del jugador. Return to Trail usa ese campamento.
- Los atajos a casa/almacén/madera no están disponibles hasta que se descubre el asentamiento. En un campamento, pueden viajar al asentamiento; en el camino entre los campamentos están escondidos.
- Preservar el inventario actual, los lotes de artesanía activos, el estado de la misión, los peajes pagados y el estado de las visitas a tiendas/campamentos en todas las rutas de viaje.

### 12.4 Interfaz adaptada al contexto

| Contexto | Acciones disponibles |
| --- | --- |
| Entre campos | Gestos de velocidad vertical del avatar, órbita de cámara horizontal independiente de 360 grados, recursos cercanos, incluido detrás del jugador, cosecha, inventario, comida contextual, pausa/configuración |
| Campamento, no comercio | Inventario, elaboración, equipo, mapa regional, escenarios; atajos de asentamiento después del descubrimiento; la resistencia se regenera |
| calle del asentamiento | Inventario, equipo, mapa mundial, configuración, Inicio/Almacén/Madera/Regreso al sendero; sin elaboración |
| Dentro de casa | acciones del asentamiento más artesanía, tienda de muebles, mejoras de la casa, almacenamiento, mascotas después del nivel tres |
| tienda del asentamiento | La interfaz de usuario contextual posee la entrada; impedir el acceso a otros edificios; la resistencia se regenera |
| Comercio de campamento activo | Acciones de negociación/lista y devolución/cancelación solo durante 15 segundos; todas las demás acciones bloqueadas; la resistencia se sigue regenerando |

Conserve los dos menús de acciones expandibles al lado del botón de inventario. El menú inferior se expande hacia arriba para Artesanía/Equipo/Mapa/Configuración; la parte superior se expande hacia abajo para Inicio/Almacén/Madera/Devolución. Deshabilite las acciones no disponibles con un breve motivo en el que la visibilidad sea útil; ocultar Regreso fuera del asentamiento. Primero se recomienda una orientación vertical para el prototipo porque los movimientos verticales de los recursos y la apertura inferior de la mochila se ajustan a esa composición; valide el diseño completo del inventario/elaboración en un teléfono físico antes de fijar la orientación. Esto reemplaza la recomendación anterior de priorizar la orientación horizontal.

### 12.5 Libro coleccionista de objetos (planificado, petición del 31 de octubre de 2026)

Un libro de colección, que se abre desde el menú del usuario (icono de libro) siempre que el menú esté disponible (no durante una interacción de recolección ni una sesión de comercio), con todos los objetos del juego.

- **Secciones por tipo:** materiales (básicos y avanzados), ingredientes (crudos y refinados, incluido el pescado), comida y bebidas (fabricadas y compradas), herramientas, ropa, mochilas, misceláneos (esculturas, juguetes, joyería, barriles llenos) y equipo especial. Dentro de cada sección las entradas se **ordenan por rango** (1-4) y luego por nombre.
- **Página de cada objeto:** un **visor del modelo 3D** del objeto (arrastrar para girarlo libremente; pellizcar para acercar es opcional), con su nombre, rango, categoría y descripción; su **región o regiones de origen** y cómo se obtiene (recolección suelta en la región N, árbol/caza/pesca en una región, tienda del asentamiento, caja misteriosa, fabricación); y las **recetas desbloqueadas que lo usan** (como ingrediente) o lo fabrican. Las recetas bloqueadas no se muestran.
- **Descubrimiento (aclarado):** antes de desbloquear una entrada, la página muestra **solo cómo se obtiene el objeto** (por ejemplo: recolección, tala / caza / pesca, receta de misión, caja misteriosa, tienda del asentamiento, fabricación); todo lo demás, **incluido el modelo 3D**, los detalles, las regiones y las recetas, queda oculto tras una silueta oscura. Una entrada se desbloquea con la **primera recolección** en los objetos que se recogen, y al **desbloquear su receta** en los objetos fabricados (o la primera vez que se obtiene de otra forma, como una caja o una compra en tienda). Los productos de tala, caza y pesca cuentan como recolectados: se desbloquean cuando el jugador los **recoge** por primera vez de un árbol talado, un animal cazado o una captura; encontrar el árbol, el animal o el estanque de la región no desbloquea nada. Los desbloqueos se guardan por ID de objeto y nunca se pierden (desmayarse, vender o usar objetos no borra entradas). Son independientes del descubrimiento de regiones y fuentes y del conocimiento de recetas.
- **Visor del modelo:** se reutiliza el enfoque del avatar del equipo (una cámara propia que dibuja el modelo del objeto en una textura, solo mientras la página está abierta), con luz neutra y una luz principal suave; herramientas y equipo usan su modelo de mochila, así que el libro coincide siempre con lo que lleva el jugador.
- **Recuentos:** cada sección muestra recogidos / total y el libro muestra un total general, como objetivo de colección.
- **Cuándo:** después de la primera revisión de mecánicas (Hito 9 o posterior), cuando existan las fuentes regionales y los desbloqueos de recetas; usa los mismos catálogos de objetos, fuentes y recetas, añade un campo de guardado con los ID de objetos recogidos y no necesita reglas nuevas.

## 13. Asentamiento, casa, muebles, almacenamiento y mascotas

El asentamiento es una escena de selección de desplazamiento lateral; no hay movimiento directo del personaje. Los edificios son destinos seleccionables. Las casas vecinas utilizan variantes visuales determinadas por una semilla y niveles de casa, pero inicialmente no tienen economía simulada ni propietarios reales.

**Distribución confirmada del asentamiento y casas vecinas (usuario, 2 de noviembre de 2026; sustituye al párrafo anterior donde difieran):**
- **Seis parcelas de casa: tres a la izquierda del centro del asentamiento y tres a la derecha.** Las tiendas y el campo maderero quedan entre los dos grupos de casas.
- **El jugador elige parcela.** Al descubrir el asentamiento por primera vez, el jugador elige una de las seis parcelas para su casa; las otras cinco pasan a ser vecinas. La elección se guarda y no cambia después (no se prevé mover la parcela).
- **Las seis empiezan en el nivel de casa 1.**
- **La casa del jugador** solo sube de nivel con las mejoras de materiales de la sección 13.1.
- **Cada casa vecina** sortea al principio un número **de 1 a 3**: las regiones nuevas que el jugador debe descubrir antes de que mejore. Una región cuenta como descubierta cuando el jugador llega a **su primer campamento**. Cada región nueva descubierta resta uno a la cuenta de cada vecina. Una vecina cuya cuenta llega a cero sube un nivel y sortea una nueva cuenta de 1 a 3. Ejemplo: una vecina que sacó 1 mejora en la región siguiente; una que sacó 2, en la de después.
- **Exteriores compartidos.** Todas las casas, la del jugador y las vecinas, usan el **mismo diseño exterior para su nivel**, así que la calle muestra una mezcla de niveles. Los 8 exteriores (sección 13.1) se hacen junto con los interiores de la casa del jugador de cada nivel, como un solo conjunto.
- *Valores delegados (propuestas, ajustables):*
  - Las vecinas se detienen en el nivel 8. Su nivel puede estar por encima o por debajo del del jugador; son solo decoración, sin economía ni dueños.
  - Las cuentas y los sorteos usan semilla y se guardan como el resto del estado del asentamiento, así que recargar nunca vuelve a sortearlos.
  - Las regiones descubiertas antes del asentamiento no cuentan; la cuenta empieza al elegir la parcela.
  - Si se descubren varias regiones sin visitar el asentamiento, se aplican una a una en la siguiente visita.
- **Orden de construcción:** primero las tiendas; después los exteriores e interiores de las casas.

### 13.1 Progresión de la casa y entrega de materiales para mejoras

Mantenga ocho **niveles de casa**, distintos de cuatro rangos de recursos: dos niveles de casa utilizan los materiales de cada rango activo. Los dos primeros crecen horizontal/ligeramente verticalmente; después del nivel tres, mantenga el ancho fijo y crezca verticalmente. Utilice un interior desplazable. el asentamiento tras la región cuatro permanece dentro del rango uno, por lo que la primera casa/actualización utiliza pino/materiales antiguos únicamente.

| Nivel | Escenario visual | Rejilla interior | Depósito de mejora para alcanzarlo (25 x rango por tipo de material) |
| --- | --- | --- | --- |
| 1 | Pequeño cobertizo | 8 x 5 | rango 1: misión de bienvenida; 25 tablas de pino + 25 ladrillos cocidos |
| 2 | Cobertizo ampliado | 10 x 6 | rango 1: 25 vigas de pino + 25 ladrillos + 25 telas de cáñamo + 25 trenzas de caña |
| 3 | Casa de madera adecuada; desbloqueo de mascotas | 12 x 7 | rango 2: 50 vigas de abedul + 50 tejas de piedra caliza + 50 Cuero refinado de castor + 50 paneles de vidrio + 50 mortero |
| 4 | Loft | 12 x 9 | rango 2: 50 paneles de abedul + 50 tejas de arenisca + 50 telas finas de cáñamo + 50 accesorios de cobre |
| 5 | Dos plantas | 12 x 11 | rango 3: 75 vigas de mezquite + 75 tejas de granito + 75 telas de algodón + 75 herrajes |
| 6 | Habitaciones superiores ampliadas | 12 x 13 | rango 3: 75 paneles de mezquite + 75 tejas de pizarra + 75 telas finas de algodón + 75 paneles de vidrio |
| 7 | Mansión fronteriza | 12 x 15 | rango 4: 100 vigas de roble + 100 basalto pulido + 100 Cuero refinado de jabalí + 100 paneles de vidrio |
| 8 | Finca terminada | 12 x 17 | rango 4: 100 paneles de roble + 100 losas de mármol + 100 cuarzo tallado + 100 herrajes de acero |

**Crecimiento de los depósitos (usuario, 5 de noviembre de 2026):** cada mejora pide la misma cantidad de cada material indicado: **25 x el rango de recurso del nivel**, así que el rango uno pide 25, el dos 50, el tres 75 y **el rango cuatro 100** por tipo de material. La Parte 2 continúa la regla (125 / 150 / 175 / 200 en los rangos 5-8) y añade los niveles 9-16 ([Parte 2](PROJECT_RECOMENDATIONS_PART_2_ES.md), sección 16). Fired bricks y Limestone tiles, necesarios en los niveles 1-3, se desbloquean con las misiones puente L7 y L8 del rango uno (sección 11.6.1). Los niveles de casa 3-4 necesitan rango dos (región siete en adelante), niveles de progresión 5-6 rango tres (región trece en adelante), niveles de progresión 7-8 rango cuatro (región diecinueve en adelante), además de misiones relevantes de desbloqueo de material en el nuevo gráfico de ramas. No se requiere ninguna fase diferida de especies/materiales globales. La trenza de caña/Mortero hace que las actualizaciones de nivel dos/tres sean útiles para el consumo de recursos para sus fuentes de región seis; La construcción de viviendas de nivel uno sigue siendo posible al llegar al asentamiento de la región cuatro. Las mejoras de la casa desbloquean listados de tiendas, no salidas gratuitas de hornos, ventanas o cofres de almacenamiento.

El menú de actualización se encuentra al lado del botón de tienda de muebles y muestra el nivel actual, el siguiente nivel, las unidades depositadas/requeridas y las fuentes faltantes. Los depósitos transfieren los materiales al depósito en garantía para mejoras de la vivienda de inmediato; la cancelación los devuelve a través de una bandeja. La finalización de la actualización, el consumo de costos, el crecimiento de la red y los desbloqueos se comprometen juntos. Utilice una regla de migración si los costos cambian después de que un ahorro ya contenga depósitos.

### 13.2 Distribución de muebles y bonificaciones

Hay diez categorías de bonificación requeridas: calidad artesanal, recuperación de alimentos, precio de venta, vida de botas, vida de camisas, vida de sombreros, vida de pantalones, vida de hacha, vida de herramientas de caza y vida de caña. Agregue la vida útil de la mochila como undécima categoría útil y la capacidad de almacenamiento como la duodécima asignación. Cuatro ciclos de doce entradas dan **48 definiciones de muebles, seis en cada uno de los ocho niveles**. De este modo, cada categoría tiene cuatro oportunidades con una distribución uniforme.

Cada fila a continuación desbloquea exactamente seis tipos. C = calidad artesanal, F = recuperación de alimentos, S = precio de venta, B = botas, Sh = camisa, H = sombrero, P = pantalones, A = hacha, Rg = herramienta de caza a distancia, Rd = caña, Bg = mochila, St = almacenamiento. Las bonificaciones de equipo afectan a los elementos recién creados, no al equipo existente. Las bonificaciones de alimentos se aplican en el momento del consumo, las bonificaciones de venta en el momento de la venta y las bonificaciones de calidad de la artesanía/vida del equipo se capturan al inicio de la artesanía. Los efectos de la calidad artesanal son deterministas y preceden a los efectos de durabilidad específicos del equipo; ninguno cambia el cronómetro.

| Nivel de la casa | Seis tipos y efectos de muebles desbloqueados |
| --- | --- |
| 1 | Mesa de trabajo (C), petate (F), cartel del mercado (S), banco para botas (B), costurero (Sh), percha para sombrero (H) |
| 2 | Porta pantalones (P), tabla de cortar (A), soporte para resorteras (Rg), soporte para cañas (Rd), marco para bolsa (Bg), cofre de almacenamiento de pino (St) |
| 3 | Telar (C), estufa de barro (F), mostrador (S), sofá (B), escritorio de costura (Sh), sombrerero (H) |
| 4 | Taburete de sastre (P), soporte para piedra de afilar (A), banco de trabajo de arcos (Rg), escritorio para aparejos (Rd), mesa de cuero (Bg), cofre de almacenamiento de abedul (St) |
| 5 | Banco artesanal (C), cómoda despensa (F), vitrina de trofeos (S), mueble para botas (B), prensa de telas (Sh), expositor de sombreros (H) |
| 6 | Armario para ropa (P), forja de herramientas (A), armario de arcos (Rg), taller de carretes (Rd), banco para hacer paquetes (Bg), cofre de almacenamiento de mezquite (St) |
| 7 | Telar maestro (C), mesa de comedor (F), pedestal de galería (S), sofá de viaje (B), estación de trabajo para telas finas (Sh), banco de sombrerera (H) |
| 8 | Armario Frontier (P), estación principal de herramientas (A), armario de cazador (Rg), armario de pescador (Rd), banco de trabajo de expedición (Bg), cofre de almacenamiento de roble reforzado (St) |

Incrementos recomendados por primera compra: vida útil del equipo +5%, recuperación de alimentos +5%, precio de venta +2%; cuatro compras dan +20% de vida/recuperación y +8% de valor de venta. Intensidad de la bonificación de calidad de fabricación: mesa de trabajo +1, telar +1, banco artesanal +1, telar maestro +2, con límite de `C = 5`. Eleva el Q acabado (max Q5), que determina la prima calidad-precio y la bonificación calidad-durabilidad del producto; ya no otorga un precio directo de +5% por C. Los muebles que combinan con la vida útil del equipo agregan su porcentaje de durabilidad base ajustada al nivel y contribuyen una vez al factor durabilidad-precio. El bonificación de precio de venta se aplica al vender, no al fabricar. No hay cambios de bonificación en los temporizadores de artesanía. Las entradas de almacenamiento desbloquean contenedores y la comida aún no puede exceder la resistencia máxima. Los valores son datos de ajuste propuestos.

**Confirmado:** los desbloqueos de bonificación se otorgan una vez por definición de mueble en su primera compra y permanecen cuando los muebles se guardan o se mueven. Los duplicados son sólo decorativos/almacenamiento. Vender/recomprar, mover, colocar o cargar el artículo nunca vuelve a desbloquear la bonificación. Las alfombras decorativas, las lámparas, los cuadros y el estante de doce posiciones pueden ser variantes dentro de los tipos desbloqueados o extras opcionales; No los cuentes accidentalmente como un séptimo tipo requerido por nivel.

Huellas: la mayoría de los muebles 1 x 1, 2 x 1 o 2 x 2; mayor 3 x 2. Ninguna pieza individual ocupa más del 15% del interior del nivel uno. Permitir que los muebles se almacenen en un catálogo cuando no estén colocados. Una pieza que parece producida otorga inicialmente la bonificación indicada; Agregar minijuegos para estaciones de trabajo sería un alcance adicional.

**Adquisición confirmada solo en tienda:** los muebles tienen un precio/desbloqueo de tienda, no una receta de ingredientes; Las compras nunca solicitan materiales. Por ejemplo, los ladrillos y los paneles de vidrio contribuyen a mejorar la casa; ese nivel puede desbloquear una estufa de arcilla o una ventana decorativa, que el jugador compra. Las tazas de barro o las botellas de vidrio son recipientes para recetas, no muebles. También se compran cofres de almacenamiento/estanterías de exhibición. El botín material no pueden pasar por alto la tienda mediante la elaboración de muebles. Ocho niveles de desbloqueo de muebles no implican ocho rangos de recursos.

Los muebles/compañeros de tienda son productos Q5 según la regla de la tienda, pero esto no agrega niveles de artesanía, desgaste ni un segundo multiplicador a sus bonificaciones definidas por primera compra. Mantener la fuerza/capacidad adicional de sus estadísticas creadas y el precio de tienda; su etiqueta de calidad no duplica un desbloqueo permanente. Las recompensas por mascotas fuera de línea son resultados obtenidos, no compras, y aún usan sus propias definiciones de recompensa.
### 13.3 Distribución de cofres y estantería

**Confirmado:** cada cofre es un solo contenedor espacial continuo con **desplazamiento horizontal**, como la mochila; sin páginas independientes, desplazamiento vertical ni rotación de objetos. Altura fija y tamaños por rango propuestos de 6 x 6, 12 x 6, 24 x 6 y 48 x 6 unidades lógicas. El mayor mantiene 288 unidades cuadradas, no 288 casillas. La colocación libre puede cruzar cualquier límite de vista: no hay fronteras de página. Guardar ID de contenedor, coordenadas y posición horizontal, no propiedad por página. Migrar páginas antiguas a franjas horizontales consecutivas sin perder contenido.

Representar/virtualizar solo objetos visibles y un margen pequeño; recortar vistas no cambia propiedad. Usar objetivos táctiles legibles (unos 48 dp), áreas seguras y cajón de transferencia o pestañas si cofre/mochila no caben juntos. El desplazamiento automático al arrastrar al borde es una comodidad propuesta; separar gesto de desplazar fondo y arrastrar objeto. Empezar con cuatro cofres colocados e índices para buscar contenido fuera de pantalla.

La estantería conserva tres filas de cuatro posiciones, un objeto listo por posición. Separar una unidad de la pila, conservar metadatos/identidad y escalar vista sin cambiar huella de inventario.

### 13.4 Negocios del asentamiento

| Edificio | Mecánicas a implementar |
| --- | --- |
| Almacén/casa comercial | Dos cajas de diez unidades del mismo ID; cualquier artículo listo vendible, incluido equipo único; sumar valores reales; 100% efectivo, 1 de oro por cada 1.000 de efectivo cada diez cajas; sin extras |
| Campo maderero | Dos hileras, seis árboles enormes por rango de madera; desbloqueo en oro por rango (propuesta: pino 50, abedul 300, mezquite 750, roble 1.500 de oro; cada uno exige haber descubierto su rango de madera); marcas de tiempo de reaparición independientes de 72 horas y botín pendiente persistente |
| Panadería | Tres ofertas: una pila común por efectivo y dos espacios premium por oro; cada espacio premium puede ofrecer una sola receta (sección 9.4.4) |
| Rancho | Tres ofertas en efectivo completas: dos de lácteos/huevos y una de carne exclusiva del rancho (sección 13.4.2); sin cueros/pieles salvajes |
| Granja | Tres ofertas completas de cultivos comerciales; el descubrimiento de regiones controla la disponibilidad |
| Mina | Tres ofertas completas de mineral/piedra en efectivo; evitar el desvío de la elaboración de regiones futuras |
| Pescadería | Exactamente dos ofertas distintas de pescado descubierto + una oferta exclusiva de marisco elegible al azar; Q5, solo efectivo, cada uno con un límite de una pila de 1,000 unidades por reinicio |
| Tienda de material a medida | Tres ofertas en efectivo completas de cáñamo/algodón/cuero de rango activa y lana opcional; no hay dependencia pendiente de la seda/cachemira/especies cazadas |

**Confirmado:** el almacén acepta **cualquier artículo listo, propio y vendible**, incluidos materiales, comida, Misc, herramientas, ropa y mochilas; conocimiento de recetas, monedas, equipo predeterminado, productos pendientes y objetos comprometidos no son vendibles. Desequipar antes el equipo puesto. Mantener dos cajas parciales, diez unidades/instancias de un ID exacto por caja. **Valor delegado:** permitir distintas calidades/dominios/condiciones del mismo ID y sumar el valor guardado por unidad; una herramienta cuenta como una, nunca como sus usos. El ID distingue rango/variante. Valoración propuesta del equipo usado: `cashUnitValue = savedIntrinsicValue * clamp(remainingDurability / maximumDurability, 0, 1)` para herramientas/ropa finitas; valor guardado completo para no durables/indestructibles. No aplicar bonificación de venta de muebles ni otro factor adicional. Mostrar cotización exacta y fijarla al depositar. Sin pago de caja parcial. Transferir propiedad a depósito, reemplazar cajas completas, pagar efectivo y avanzar el ciclo de oro una sola vez. Empezar con una caja/acción y después depósito masivo limitado; una pila de prueba llena 100 cajas sin 100 animaciones simultáneas.

**Renovación confirmada del asentamiento (sustituye propuesta de 30 minutos):** generar existencias al entrar y renovar ofertas de todas las tiendas normales de recursos/ingredientes cada vez que el jugador realmente **sale del asentamiento y regresa**. Sin temporizador, espera ni límite diario. Un nuevo `SettlementVisitId` persistente confirma semilla/ofertas y cantidades completas una vez al volver; permite comprar materiales de inmediato en lugar de recolectar. Moverse entre casa, tiendas, almacén y otras vistas internas no es salir. Reabrir tienda en la misma visita conserva stock/compras; suspensión/recarga o viaje cancelado también conservan visita. Mostrar existencias/precios sin cuenta regresiva. Catálogo de propiedad de mascotas sigue permanente. Globo usa su regla independiente de cerrar/reabrir en sección 14.

**Las seis tiendas del asentamiento entregan Q5:** materiales de panadería, rancho, granja, mina, pesca y sastrería. Los bienes ordinarios en efectivo y los bienes de oro premium siguen la misma regla. La identidad, la cantidad y el precio del artículo pueden variar según la oferta, pero la calidad se fija en cinco y persiste durante la compra, la entrega adicional y la recarga. Los ingredientes Q5 comprados en la tienda aún no pueden reemplazar otra variedad de material en una receta.

El alcance del campo maderero sigue siendo de cuatro rangos x seis árboles = 24 registros, desde pino hasta roble. Representa solo filas visibles. La sección 8.2 define curvas de acción separadas entre Normal y Élite: Élite es más rápido en o por debajo de su rango y aumenta el botín en cada árbol elegible, incluido +1/+2. Aplique recompensas x50 una vez y conserve la reaparición/vencimiento independiente de 72 horas. Las especies pendientes siguen aplazadas; Las limitaciones de UTC/reloj local persisten.

**Razonamiento del desbloqueo maderero (propuesta, 31 de octubre de 2026):** tras el reequilibrio, un árbol enorme dará **unos 450 objetos**. Eso equivale a unos 450-4.500 de efectivo en materiales por árbol, y los seis árboles de un rango reaparecen cada 72 horas, así que el campo maderero es un suministro constante de madera para fabricar y construir, no una fuente de dinero. Los desbloqueos tienen precio de comodidad temprana en el asentamiento: pino 50 de oro (menos que un sombrero especial de rango uno), abedul 300, mezquite 750 y roble 1.500 de oro.

**Tenderos de las tiendas (usuario, 2 de noviembre de 2026):** cada tienda tiene un **PNJ estático visto de cintura para arriba detrás de su mostrador** (sin piernas modeladas), vestido para su tienda, en el estilo facetado de los personajes. Es una persona fija por tienda (no se sortea como los comerciantes de campamento), para que el jugador reconozca cada tienda.
- **Panadería:** panadera, gorro blanco de harina, delantal con peto sobre camisa crema, una mancha de harina.
- **Rancho:** ranchero, sombrero tostado de ala ancha, camisa vaquera, chaleco de cuero abierto, pañuelo rojo, barba gris.
- **Granja:** granjera, sombrero de paja con cinta verde, blusa oliva, peto con botones de latón.
- **Mina:** minero, casco de cuero con lámpara de latón, chaqueta oscura con hombreras de cuero, pañuelo rojo, hollín, barba negra.
- **Pescadería:** pescador, gorro de punto azul marino, jersey crema de cuello alto, peto amarillo de hule, barba gris.
- **Tienda de materiales de sastrería:** sastra, chaleco ciruela sobre camisa blanca, cinta métrica al cuello, acerico en la muñeca, gafas.
- **Almacén/casa de comercio:** comerciante, visera verde, camisa blanca con ligas rojas en las mangas, chaleco marrón con cadena de reloj de latón, pajarita, lápiz tras la oreja.
- **Campo maderero:** es un campo, no un mostrador, así que por ahora no tiene tendero (podría añadirse un guardabosques si tiene caseta).
- **Notas de diseño:** mostrador de unos **1,02 m** de alto, con el tendero a 0,3-0,45 m de su borde y las manos apoyadas encima, de modo que el mostrador oculta todo lo que hay bajo la cintura. Encuadrar la vista de la tienda a la altura del pecho y el sombrero (los sombreros son la señal temática más fuerte). Cada tienda repite la paleta y los objetos de su tendero. Basta una animación sutil (respiración, giros de cabeza) y un saludo con la cabeza al completar una compra. Las ofertas, existencias y precios van sobre el mostrador o en un tablero al lado, nunca sobre la cara.

**Diseño de las tiendas (2 de noviembre de 2026):** cada tienda tiene una **fachada en la calle** (en la escena lateral del asentamiento, entre los dos grupos de tres parcelas) y una **vista de mostrador** (al tocar la tienda: el tendero tras el mostrador, las tres ofertas encima y estanterías temáticas detrás).
- **Kit compartido** para que la calle parezca un solo pueblo: entramados de madera, zócalos de piedra, un mismo juego de ventanas, puertas y contraventanas, tejados a dos aguas hacia la calle y un **cartel con dibujo y sin texto** orientado a la cámara. Cada tienda tiene su color de toldo. Las fachadas miden unos 6-7 m de ancho.
- **Panadería:** casa de piedra y madera de dos plantas con piso superior volado, contraventanas verdes, tejado de teja roja, chimenea con humo, toldo a rayas rojas y crema sobre un escaparate de panes y cartel de gavilla de trigo. Mostrador: estantes de hogazas, horno de ladrillo encendido, sacos de harina y pala. La oferta común en efectivo va en una cesta de pan; las dos ofertas premium de oro, bajo campanas de cristal con base de latón.
- **Rancho:** granero rojo con tejado a dos aguas, valla, pacas de heno, lecheras y cartel de cencerro. Mostrador: carne curada colgada, ruedas de queso y lecheras.
- **Granja:** puesto de mercado abierto con tejado de paja, cajas de productos inclinadas, toldo verde, espantapájaros y cartel de brote. Mostrador: cajas de productos y manojos de hierbas.
- **Mina:** entrada con entramado de madera excavada en una pared de roca, vagoneta sobre raíles, faroles y cartel de pico. Mostrador: cubos de mineral, balanza y piedra iluminada por faroles.
- **Pescadería:** caseta de muelle sobre pilotes con redes y boyas en azul marino y amarillo, y cartel de pez. Mostrador: bandejas de hielo para los dos pescados ordinarios y una nasa de langosta para el marisco exclusivo.
- **Tienda de materiales de sastrería:** casa estrecha y alta con detalles ciruela, rollos de tela y un maniquí en el escaparate, y cartel de carrete. Mostrador: rollos de tela en la pared y una regla de medir.
- **Almacén/casa de comercio:** la fachada más grande, con puertas dobles de granero, grúa de carga, cajas apiladas y cartel de balanza de latón. Mostrador: mostrador de libro de cuentas con las dos cajas de depósito como cajones y una balanza de oro.
- **Campo maderero (usuario, 2 de noviembre de 2026):** como las tiendas, tiene una **entrada en la calle** y su **propia zona interior**, con estas diferencias:
  - **Entrada:** una puerta en una valla de madera con cartel de guardabosques (hacha y tronco), al final de la fila de tiendas.
  - **Zona:** una escena o vista aparte a la que se entra por la puerta, no un mostrador. Contiene las **dos filas de seis árboles enormes por rango de madera**, una sección por rango (pino, abedul, mezquite, roble).
  - **Rangos bloqueados:** sus secciones siguen valladas, con un cartel de desbloqueo con oro.
  - **Árboles en reaparición:** un árbol en su espera de 72 horas muestra su gran tocón cortado y una marca de rebrote.
  - **Tala:** tocar un árbol abre la pantalla de tala existente. Los árboles enormes mantienen la pantalla abierta tras el último golpe para recoger, y solo Volver la cierra. Un árbol talado con restos pendientes se puede tocar de nuevo para reabrir la recogida (por si la mochila se llenó; sección 8.2.1, usuario, 5 de noviembre de 2026).
  - **Sin tendero ni ofertas.** *Propuesta:* un guardabosques junto a la puerta da el diálogo de presentación único (tutorial, paso del asentamiento) y más adelante podría vender los desbloqueos.
  - **Mejoras:** la zona crece por rango con sus secciones desbloqueadas, no con los aspectos de tienda por rango.
- **Orden de la calle (delegado a Claude por el usuario, 2 de noviembre de 2026; confirmado):** entre las tres parcelas de la izquierda y las tres de la derecha, la calle sigue este orden: **almacén, panadería, rancho, granja, pescadería, mina, tienda de materiales de sastrería, campo maderero**. Primero el edificio más usado (el almacén, para vender). Después las tiendas de comida, de los productos diarios de la panadería a los frescos y el pescado. Después las de materiales (piedra y mineral, luego tela). El ancho campo maderero queda al final, donde sus filas de árboles enormes tienen sitio.
- **Mejoras de tiendas por rango (usuario, 2 de noviembre de 2026):** las tiendas mejoran cuando el jugador llega a las **regiones del rango siguiente**: rango 2 desde la región 7, rango 3 desde la 13 y rango 4 desde la 19. Por ahora es sobre todo visual (otras mecánicas solo si el usuario las propone).
  - *Propuesta:* cuatro aspectos por tienda, cada uno con los materiales de su rango, a juego con las maderas: pino, después abedul, mezquite y roble en entramados y remates, con tejados más sólidos, más género expuesto y carteles más ricos en cada rango.
  - La vista de mostrador mejora junto con su fachada, y el tendero sigue siendo la misma persona.
  - La mejora ocurre una sola vez, la primera vez que el jugador llega a una región de ese rango, y se guarda.
  - Las casas mantienen su propia regla de 8 niveles, aparte de esta.

**Árboles del campo maderero (recomendación, 2 de noviembre de 2026):** hacer **modelos nuevos de árboles enormes** en lugar de solo escalar el tronco del sendero, que a ese tamaño se ve tosco y no parece antiguo. Un árbol enorme necesita:
- raíces contrafuerte, corteza con surcos profundos, musgo y nudos, y una copa o una copa rota;
- las cuatro maderas (pino, abedul, mezquite, roble);
- un gran tocón cortado para el rebrote de 72 horas;
- dos o tres formas de tronco por especie, con ligeras inclinaciones y giros, para que las dos filas de seis no parezcan clonadas.

Se conservan los puntos de anclaje de la tala (tronco inferior, pivote de caída, zona de corte, indicación del hacha), para que la interacción de tala actual los mueva sin cambios, además de los colores de corteza de cada especie y el estilo facetado.

### 13.4.1 Productos exclusivos de la pescadería y renovación de existencias

**Confirmado:** mantén tres espacios, pero reserva **dos para pescado común y uno para un ingrediente exclusivo**. Seleccione al azar dos peces distintos cuyas regiones de origen hayan sido descubiertas y luego uno exclusivo de su catálogo elegible. Nunca llenes el espacio exclusivo con un tercer pez ni cobres oro por él. La pescadería sigue siendo una tienda de efectivo, a diferencia de las tragamonedas de oro premium de la panadería.

Catálogo exclusivo compacto propuesto:

| Ingrediente exacto | Rango de progresión material | Elegibilidad de acciones más temprana | Fuente de adquisición inicial |
| --- | --- | --- | --- |
| Camarones | 1 | asentamiento/pescadería desbloqueado después de la cuarta región | Exclusivo de la pescadería |
| Cangrejo | 2 | progresión descubrimiento de región de rango dos | Exclusivo de la pescadería |
| Calamares, propuesta de incorporación | 3 | progresión descubrimiento de región de rango tres | Exclusivo de la pescadería |
| Langosta | 4 | progresión descubrimiento de la región de rango cuatro | Exclusivo de la pescadería |

Seleccione uno de manera uniforme del grupo exclusivo elegible para la prueba inicial; La ponderación del rango actual se puede ajustar más adelante. Los tipos previamente desbloqueados siguen siendo elegibles, por lo que alcanzar el rango cuatro aún puede mostrar camarones/cangrejo/calamar/langosta. La ubicación actual del jugador nunca cambia el MaterialTier del ingrediente. Cada oferta es Q5, con una cotización en efectivo escrita que utiliza su perfil de calidad real exactamente una vez.

Cada espacio comienza con **una pila completa/1,000 unidades de existencias por reinicio**, no compras repetidas ilimitadas. Comprar esa pila agota su espacio. Si las compras parciales se exponen posteriormente, todas las compras juntas aún no pueden exceder su presupuesto restante de 1,000 unidades; La interfaz de usuario de compra parcial es opcional y no es necesaria para la primera implementación. Conservar los ID seleccionados, la cantidad restante, el precio cotizado en efectivo, el ID de visita/restablecimiento y las transacciones de compra. La reapertura/recarga no puede reabastecerse ni volver a lanzarse; solo salir realmente del asentamiento y regresar renueva existencias.

La exclusividad es una restricción de fuente inicial: los mariscos crudos no aparecen como botín suelto, capturas de nodos de peces, recompensas de árboles/caza, recompensas de mascotas o recompensas de artículos de cajas misteriosas ordinarias. No se suma a los dieciséis peces de la ruta ni a los 72 recursos sueltos destacados. los jugadores pueden posteriormente intercambiar/vender sus bienes comprados a través de reglas de propiedad normales; un NPC generado por el sistema no debería convertirse en otro suministro de productos del mar crudos. Las recompensas de recetas pueden aparecer en cuadros, **no en el ingrediente crudo exclusivo en sí**, a menos que esa regla de origen se cambie explícitamente más adelante. Una preferencia de elemento de misión en una casilla no debe eludir la exclusividad.

Estos son ingredientes para recetas de cocina coleccionables opcionales, no entradas obligatorias para el progreso de región/ropa/casa. La pescadería puede mostrar un ingrediente antes de que se aprenda su receta; muestra las entradas de su libro de recetas como bloqueadas o disponibles en caja. El progreso principal se puede lograr a través de recetas de comida secundaria/recetas normales de pescado. Los ingredientes de Store Q5 no pasan su grado a la artesanía resultante según la regla de calidad de salida establecida.

### 13.4.2 Productos del rancho y recetas premium del rancho (propuesta, petición del usuario, 5 de noviembre de 2026)

El rancho vende productos **domésticos** que ninguna fuente del sendero da. Como el marisco exclusivo de la pescadería (sección 13.4.1), cada rango tiene una **carne exclusiva del rancho**: un ingrediente crudo con origen `Store` únicamente.
- **De dónde nunca sale:** recolección suelta, caza u otros elementos interactivos, recompensas de mascotas, premios físicos de la caja misteriosa ni suministro de NPC procedurales. El jugador sí puede comerciar con lo que compró.
- **Qué la usa:** solo **recetas premium opcionales de la caja misteriosa** (abajo), nunca la progresión obligatoria.
- **Sin misión:** el rancho puede mostrar una carne antes de conocer su receta, y eso no es motivo para añadir una misión (sección 9.4.2).
- **No son animales de caza:** el conejo, el castor, el pavo y el jabalí salvajes siguen siendo solo de caza, y el rancho nunca vende su carne ni sus pieles.

| Exact ingredient (ID) | Material tier | Ranch stock eligibility | Proposed Q5 cash / unit |
| --- | --- | --- | --- |
| Chicken meat (`ranch.chicken_meat`) | 1 | Settlement and ranch unlocked (after region four) | 50 |
| Duck meat (`ranch.duck_meat`) | 2 | A tier-two region discovered | 100 |
| Lamb meat (`ranch.lamb_meat`) | 3 | A tier-three region discovered | 180 |
| Beef (`ranch.beef`) | 4 | A tier-four region discovered | 300 |

Los productos domésticos existentes se mantienen: Cow milk, Goat milk y Egg (ingredientes de despensa). La cadena opcional de Raw wool sigue aplazada. **Existencias del rancho:** tres ofertas Q5 en efectivo por visita al asentamiento, cada una de una pila completa de 1.000 unidades:
- **dos ofertas de lácteos/huevos** (dos objetos distintos entre Cow milk, Goat milk y Egg)
- **una oferta de carne** (una carne de rancho elegible; los rangos anteriores siguen elegibles)

Se aplican las reglas habituales: renovación al salir y volver, compras parciales al precio por unidad, y ofertas y compras guardadas. Los precios son cotizaciones Q5, no se multiplican otra vez por calidad, y no tienen descuento de pila de panadería. Los ingredientes Q5 de tienda no transmiten su calidad al plato fabricado.

**Recetas premium del rancho (exclusivas de la caja misteriosa, ocho recetas, dos por rango).** Siguen los corazones premium por rango de la sección 9.4.2 (rango uno 8-10, dos 9-10, tres 10-11, cuatro 11-12) y la regla premium de la sección 9.6.6: al menos tres ingredientes distintos, uno refinado o de otro origen. Se indica la madera como combustible cuando el plato es asado.

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

**Elegibilidad** (igual que las demás recetas de caja, sección 14):
- el rango de la receta está descubierto
- el rancho es accesible y vende ese rango de carne
- cada otro origen indicado está descubierto, con la conversión necesaria conocida o una tienda exacta accesible
- ninguna caja ofrece la receta antes de eso

El recetario muestra **Origen: caja misteriosa del globo** e indica el rancho como origen de la carne. **Efecto en la caja:** estas ocho recetas se suman al grupo coleccionable: 26 + 18 de barril + 8 del rancho = **52** en la Parte 1 (la Parte 2, sección 10.2, añade las recetas del rancho de los rangos 5-8). La probabilidad de receta del 5% por espacio no cambia.

### 13.5 Variedad de mascotas, casita y recolección inactiva

**Distribución confirmada:** nivel de casa tres desbloquea la sección **Mascotas de la tienda del hogar** para comprar compañeros. La **casita a la izquierda del frente del hogar** permite elegir una mascota propia, solo una activa; un **contenedor frontal separado** guarda materiales inactivos. Comprar no cambia automáticamente la activa. Tienda muestra especie, precio, propiedad y pesos; selección muestra compañero activo y estado de recolección. Son compañeros del juego sin combate, usos de herramienta, crianza, hambre ni desgaste.

**Primer catálogo propuesto:** seis `PetDefinitionId` distintos. Compra única de **1,000 efectivo propuestos** por mascota, sin oro obligatorio, duplicados ni mantenimiento. Todas desde casa tres; especialidades no descubiertas aparecen bloqueadas, sin acceso a regiones futuras. Igual precio/frecuencia centra la elección en suministros. Primera implementación puede usar Perro y Cerdo provisionales antes de añadir las otras cuatro. Son compras Q5 según etiqueta de tienda; no convierte botín inactivo en Q5 ni otorga muebles. Variantes de color cosméticas comparten tabla de especie.

| ID / mascota | Madera | Piedra/mineral | Fibras | Productos/hierbas | Semillas | Trufa | Especialidad |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `pet.dog` — Perro | 45% | 40% | 5% | 5% | 5% | 0% | Construcción; 85% madera/minerales |
| `pet.cat` — Gato | 10% | 10% | 40% | 25% | 15% | 0% | Plantas textiles e ingredientes |
| `pet.rabbit` — Conejo | 5% | 5% | 20% | 45% | 25% | 0% | Frutas, hierbas y semillas |
| `pet.pig` — Cerdo | 10% | 15% | 5% | 20% | 10% | 40% | Especialista en trufas tras región 10 |
| `pet.squirrel` — Ardilla | 25% | 10% | 15% | 20% | 30% | 0% | Semillas y ramas |
| `pet.tortoise` — Tortuga terrestre | 10% | 60% | 15% | 10% | 5% | 0% | Piedra, tierra y mineral crudo |

**Mejoras de la casita de mascotas (propuesta, petición del usuario, 5 de noviembre de 2026):** la casita tiene sus propios niveles y **cada nivel admite una mascota activa más**. Cada mascota activa conserva su propio temporizador de tiradas, sus pesos y su contenedor de 16 entradas. La producción por mascota, la tirada de 30 minutos, el tope de ocho horas y la tirada de rango 70/30 no cambian, así que más mascotas significa más mascotas trabajando, no mascotas más fuertes. Un espacio de admisión aloja una mascota propia. El cambio sigue la regla existente de liquidar y luego cambiar por mascota, y un espacio vacío no produce nada. Las mejoras son depósitos de materiales con las reglas de la casa (depósito en garantía, cancelación por la bandeja, finalización atómica), con **la mitad de la cantidad de la casa del mismo rango** (redondeada a 5) y tres tipos: tablas para el corral, tela para las camas y herrajes para las puertas.

| Pet home level | Active pets | Requires | Deposit |
| --- | --- | --- | --- |
| 1 | 1 | House level 3 (pets unlock) | none |
| 2 | 2 | House level 4 | 25 Birch planks + 25 Flax cloth + 25 Copper fittings |
| 3 | 3 | House level 6 | 40 Mesquite planks + 40 Cotton cloth + 40 Iron fittings |
| 4 | 4 | House level 8 | 50 Oak planks + 50 Fine cotton cloth + 50 Steel fittings |

Con seis mascotas en el catálogo, cuatro pueden trabajar a la vez al final del rango cuatro; la [Parte 2](PROJECT_RECOMENDATIONS_PART_2_ES.md) (sección 16.5) continúa hasta ocho y añade seis mascotas exóticas. El aspecto de la casita crece con su nivel (caseta, corral cercado, corral con refugio, pequeño granero) y su arte forma parte del conjunto de la casa.

La tienda de mascotas usa catálogo permanente de propias/no compradas, sin tres ofertas aleatorias ni renovación por visita de las tiendas de recursos. Compra y registro de propiedad son atómicos. Mascotas no ocupan mochila, se eligen por ID sin equiparse como objeto y no son mercancía del almacén. Mascota inactiva/no poseída no produce nada.

**Grupos de suministros definidos, no ecología literal:** Madera = ramas sueltas exactas descubiertas, nunca troncos/tablas. Piedra/mineral = ID sueltos descubiertos de piedra/tierra/mineral crudo/gemas (Guijarros, Arcilla, Mineral de cobre). Fibras = solo vegetales sueltos (Cáñamo, Lino, Algodón), sin tela ni caza. Productos/hierbas = Manzanas, Hierbas de té, Cebada, Zanahorias, Calabazas, etc. exactas sueltas, excluyendo Semillas/Trufa. Semillas = Semillas de lino y de girasol exactas; sin objeto semilla genérico. Trufa = solo `ingredient.truffle`. Crear listas explícitas sin solapamientos con región/rango existentes. Excluir pieles/plumas, carne, pescado, lácteos/Lavanda exclusivos de tienda, procesados, monedas, recetas, equipo y mariscos exclusivos. Mascotas aportan materiales adicionales, nunca ingredientes nuevos ni evasión de origen.

**Orden del sorteo:** fijar ID/pesos de mascota y fuentes descubiertas al iniciar cada intervalo sin conexión. Por oportunidad completada, eliminar grupos vacíos/no descubiertos y renormalizar pesos; dentro del grupo proponer 70% al rango elegible descubierto más alto/30% a anteriores, renormalizando si uno falta; pesos iguales por ID exacto dentro del rango elegido como inicio. No multiplicar peso de grupo por cantidad de objetos/especies. Sin grupos elegibles, no generar ni guardar sorteo futuro. Antes de región 10 Cerdo no encuentra Trufa y renormaliza otros pesos; descubrir después no cambia recompensas antiguas. Mostrar porcentajes efectivos y originales: un cerdo en región 7 tiene 0% efectivo de Trufa. Mascotas no desbloquean región/material/receta y ninguna misión o receta exige poseer una.

**Modelo inactivo:** solo aplicación cerrada/suspendida gana oportunidades; juego activo/menús no. Valores iniciales: un sorteo cada **30 minutos sin conexión**, máximo **ocho horas por ausencia** = 16 sorteos, **un material por sorteo en Release**, con ajuste explícito de 1,000 por sorteo en MechanicsTest para límites. Contenedor admite **16 entradas**, conservando cantidad/calidad/semilla. No mezclar entradas generadas para evitar límite; al reclamar sí pueden unirse pilas compatibles. Calidad sigue tablas de origen, no Q5 de mascota/tienda ni muebles del jugador. Cantidades independientes de recogida de sendero y pilas de tienda/caja.

Guardar resultado de cada oportunidad una vez antes de mostrar. Entradas sin reclamar persisten sin caducar/reemplazarse. Dejar de generar al ocupar 16; no acumular tiempo/sorteos perdidos para reclamarlos después. Mostrar capacidad, contenido y límite antes de cerrar. Reclamar individualmente o Todo hasta donde quepa; conservar resto exacto y permitir consumo por misión solo tras llegar al inventario transportado. Contenedor es cola de recompensas, no cofre para depositar objetos.

**Tiempo/cambio:** liquidar intervalo anterior antes de comprar, seleccionar, cambiar fuentes descubiertas o reclamar. Cambiar mascota afecta siguiente oportunidad: progreso fraccionario guardado conserva mascota/fuentes originales hasta completarse; oportunidades posteriores usan la nueva. Conservar fracciones inferiores a 30 minutos entre ausencias cortas; descartar tiempo sobre límite de ocho horas/espacio. Sin mascota activa no hay acumulación ni pago retroactivo. Al reanudar usar `max(0, nowUtc - offlineStartedUtc)`, limitar una vez y confirmar atómicamente ID/semillas, fracción, ID de intervalo consumido y marca de tiempo que no retrocede. Reloj hacia atrás da cero y no retrocede marca; reanudar repetidamente no repite intervalo. Conservar resultados al cambiar/recargar. Manipular reloj local sigue limitación sin conexión; verificar con reloj inyectado.

**Guardado/aceptación:** persistir mascotas/compras, activa, versión de pesos, intervalo/fuentes, fracciones, transacción/resultado por oportunidad, entradas/restos y descarte por límite. Validar seis filas sumando 100%, especialidades, Perro 85% construcción, Trufa de Cerdo bloqueada hasta región 10, filtros exactos, ninguna receta obligatoria exclusiva de mascota, sin filtración de calidad/muebles, reclamación parcial sin espacio, contenedor lleno sin nuevo sorteo, cambio a 29/31 minutos, ocho horas, reloj inverso y recarga tras generar/reclamar.

## 14. Comerciante del globo y cajas misteriosas

**Frecuencia del globo (confirmado el 31 de octubre de 2026):** el comerciante aparece en **un campamento de cada 4-6** a lo largo de la ruta (definido por región, determinista y nunca en dos campamentos seguidos). En sus campamentos, muestra exactamente tres ofertas de oro: una prenda de vestir indestructible, una caja misteriosa y una mochila. Definir explícitamente si la mochila del comerciante es indestructible; Recomendación: sí, como equipo especial en ese rango. No infieras que todo objeto comprado con oro es indestructible.

**Interior de la tienda del globo (usuario, 2 de noviembre de 2026):** el globo permite interactuar: el jugador **entra en la tienda y ve el mostrador**, con el comerciante del globo detrás, de cintura para arriba como en las tiendas del asentamiento (notas de diseño en la sección 13.4). El interior es una vista aparte, no la cabina exterior. La puerta exterior mide solo unos 1,3 m, así que el comerciante nunca se muestra de pie en ella; entrar es una transición al interior. El interior y su mostrador siguen las mismas notas que las tiendas (mostrador de unos 1,02 m; ofertas sobre el mostrador o en un tablero, nunca sobre la cara del comerciante), decorados con laca carmesí, remates de latón y farolillos de papel cálidos a juego con la cabina. **Aspecto del comerciante:** levita cruzada de laca carmesí con botones de latón, ribetes dorados y charreteras, pañuelo de seda crema y alfiler con el emblema del globo en latón (la firma del comerciante, sección 14.1); gorro de aviador de cuero con orejeras y gafas de latón subidas; bigote de manillar.

**Presentación de la caja misteriosa (confirmado el 31 de octubre de 2026):** la caja tiene una **animación de apertura y de cierre**, y su **juego de colores depende del rango de la región donde se encuentra el comerciante del globo** (un juego de colores en los campamentos de regiones de rango uno, otro en los de rango dos, y así sucesivamente). El color es solo presentación; el contenido sigue las reglas de generación siguientes. Los modelos del equipo especial (sección 14.1) se crean junto con ella; ambos quedan para la interacción y la lógica de la tienda.

**Renovación confirmada del globo:** conservar tres ofertas (ropa especial indestructible, caja misteriosa, mochila). Generar nuevas ofertas al abrir la tienda después de cerrarla, incluso en la misma visita al campamento. Cerrar termina sesión de ofertas; reabrir vuelve a sortear la **prenda especial mostrada** y otras ofertas no compradas del catálogo/rango elegible. El sorteo puede repetir legítimamente identidad. Sin temporizador, pago, salida del campamento ni compra para renovar. Usar `BalloonShopSessionId`/revisión de apertura persistida, no solo semilla de visita. Redibujar interfaz, ver detalles o eventos Open duplicados estando abierta no sortean; recargar restaura sesión abierta hasta cerrar/reabrir realmente. Cobrar/confirmar contra revisión mostrada para impedir compra de otra oferta con toque obsoleto.

Productos físicos de tienda/caja siguen Q5; ropa especial indestructible dentro de límites y equipo ordinario de caja finito. Conocimiento de receta no tiene calidad/objeto físico y empieza dominio uno. **Manifiestos y compras ya pagadas son inmutables** al renovar: cambiar ofertas, nunca contenido/calidad/reservas de recetas de caja pagada ni recompensas excedentes. Cada compra nueva obtiene manifiesto atómico de seis entradas; oferta sin comprar no revela contenido generado para volver a sortear gratis. Renovar globo nunca renueva oportunidad de comercio en cinta.

Generación de seis entradas propuesta, manteniendo el total existente en lugar de agregar una séptima recompensa:

1. Reserve una entrada de artículo de misión físico apilable elegible. Su origen debe permitir la entrega en caja misteriosa: **el marisco crudo exclusivo de la pescadería se excluye incluso cuando lo solicite una misión**. Si ninguno es elegible, utilice un material descubierto de la región actual.
2. Sortear los otros cinco espacios de forma independiente utilizando los pesos propuestos a continuación.
3. Para identidades elegibles, prefiera el rango desbloqueado actual al 70%, los rangos anteriores al 30%, con un respaldo válido cuando un subgrupo está vacío. Una receta debe tener `MysteryBoxEligible` y pasar las comprobaciones no aprendidas/no reservadas a continuación; No omitas los desbloqueos de recetas de misiones normales.
4. Los apilables físicos ofrecen 1,000 unidades, el equipo, una instancia y una ranura RecipeUnlock una ID de receta exacta. Los espacios para recetas no se convierten en pilas de pergaminos físicos y no consumen área de la mochila. Ninguna ropa especial ingresa al grupo de artículos.
5. Mantener filtros de origen/región. La excepción existente de elementos de misión activa nunca anula las restricciones de fuentes de pesca exclusivas. Los desbloqueos de recetas deben estar dentro de los rangos descubiertos y tener fuentes exactas de ingredientes alcanzables; las excepciones de su despensa no otorgan ingredientes no disponibles en el futuro.
6. Generar previamente las seis entradas con tipo definido, perfiles de estadísticas físicas del Q5, ID de recetas y cualquier recompensa alternativa duplicada; cargar oro y persistir el manifiesto atómicamente. La reapertura/recarga/desbordamiento nunca vuelve a generar resultados. Las reclamaciones de artículos utilizan reglas normales de inventario/bandeja; La obtención de recetas actualizan el estado de receta conocida una vez.

| Categoría para cada uno de los cinco espacios aleatorios | Peso propuesto |
| --- | --- |
| Materiales/alimentos elegibles | 80% |
| Instancia de herramienta | 10% |
| Ejemplo de ropa/mochila ordinaria | 5% |
| Desbloqueo opcional de recetas de alimentos/varios | **5%** |

Con un grupo de recetas elegibles no vacío, cinco tiradas independientes del 5 % dan aproximadamente un **22.6 % de probabilidad de obtener al menos una receta** por caja (`1 - 0.95^5`). Esta es una propuesta inicial, no una probabilidad de botín confirmada ni una garantía de receta. El espacio de preferencia de misión no es una sexta tirada de recetas. Todas las recompensas físicas siguen siendo Q5; Los desbloqueos de recetas no reciben calidad, durabilidad, reventa en efectivo ni dominio automático.

Selección de recetas/política duplicada:

- Grupo coleccionable inicial: las veintiséis ID de recetas de alimentos/misceláneas opcionales de las secciones 9.4.2-9.4.3, incluidas recetas de mariscos, más las dieciocho recetas de llenado/embotellado en barril de roble de la sección 9.4.5 las ocho recetas premium del rancho de la sección 13.4.2 y los siete recuerdos de metales preciosos y gemas de la sección 9.5.1 (59 en total; propuestas del 31 de octubre y del 5 de noviembre de 2026; antes 44, propuesta del 31 de octubre de 2026). No muevas silenciosamente las misiones obligatorias de jugo de manzana/herramientas/ropa a este grupo aleatorio.
- Requiere `RecipeUnlockTier` descubierto y fuentes de ingredientes accesibles, incluido un rango de catálogo/pesca desbloqueado para mariscos. Un ingrediente refinado se puede suministrar a través de una receta ya conocida o de un catálogo de tienda descubierto elegible; no es necesario que esté en la bolsa al momento de la compra. No hay ingredientes obligatorios de regiones futuras ni cadenas de elaboración inaccesibles.
- Excluye recetas ya conocidas y recetas reservadas en otras cajas compradas no reclamadas. Reserve también las ID recién seleccionadas dentro del cuadro actual, de modo que dos espacios no puedan otorgar la misma receta.
- Si no queda ninguna receta no aprendida elegible, convierta ese espacio en una recompensa de comida o material físico elegible. No inventes un duplicado inútil, un bucle de reembolso o 1,000 pergaminos.
- Si una receta seleccionada se conoce antes de la reclamación a través de otra fuente autorizada, use su **respaldo preenrollado persistente**. No otorgues dominio, no elijas una nueva receta aleatoria ni descartes silenciosamente una recompensa pagada.
- Reclamar una nueva receta otorga conocimiento una vez en el nivel de dominio uno sin conteo de elaboración. Las fabricaciones posteriores utilizan las reglas normales de grado/muebles/cronómetro/valor. La reclamación y la actualización de ID conocido comparten un ID de transacción, por lo que la recarga no puede repetir concesiones/respaldos.

Los duplicados físicos pueden fusionarse después de la reclamación si sus perfiles de elementos/estadísticas coinciden; Se evitan duplicados de recetas como se indicó anteriormente. La contribución a la misión ocurre cuando se reclama una recompensa física elegible, nunca por conocimiento de recetas. Una bolsa llena no impide aprender una receta, mientras que el exceso físico permanece en la bandeja persistente. Una misión completada no es elegible para la preferencia de misión. Los mariscos crudos siguen siendo una compra pesquera al contado, incluso cuando su receta se obtiene de una caja comprada en oro.

Agregar una receta no agrega automáticamente su producto terminado al botín de la caja física. Mantenga indicadores de desbloqueo de receta/fuente de artículo separados; Inicialmente, estas veintiséis adiciones son recompensas de recetas, no una fuente trasera de platos de mariscos gratuitos o productos en oferta antes de que se aprendan sus recetas. Cualquier recompensa posterior por artículo terminado debe crearse deliberadamente bajo los filtros de origen normales.

Estas probabilidades son valores iniciales propuestos, no datos del juego de referencia. En este plan, el oro se gana en el juego. La monetización de dinero real está fuera del alcance de implementación actual.

### 14.1 Equipo especial del globo (propuesta, 31 de octubre de 2026)

Las dos ofertas de equipo del globo son el **equipo especial**: una prenda (sombrero, botas, camisa o pantalones) en el espacio de ropa y una mochila en el espacio de mochila. Esto resuelve la pregunta anterior: **la mochila del comerciante también es equipo especial (durabilidad infinita)**. El equipo especial no se fabrica, no tiene receta, nunca aparece en cajas misteriosas, siempre es Q5, nunca se desgasta (la insignia del equipo muestra el símbolo de infinito) y respeta los límites globales. Valores propuestos para revisión; los precios asumen la nueva tasa del almacén (1 de oro por cada 1.000 de efectivo) y se reequilibrarán junto con todo lo demás cuando se retiren las herramientas de depuración.

**Elegibilidad de las ofertas:** cada renovación sortea el tipo de prenda de forma uniforme entre las cuatro y luego su rango: el rango descubierto más alto con 70% y los rangos descubiertos anteriores con 30% (el mismo reparto que las recompensas de las cajas), con alternativa válida si un lado está vacío. La oferta de mochila sortea su rango igual. Un objeto especial ya poseído puede volver a ofrecerse (una segunda copia sirve de repuesto).

**Regla de estadísticas (confirmada):** el equipo especial cambia un poco de potencia por no desgastarse nunca.

- El equipo especial de rango 1 alcanza el **límite del rango 1**.
- Desde el rango 2, el equipo especial llega **como máximo a la mitad** del camino entre el límite del rango anterior y el límite de su propio rango. Las estadísticas enteras (corazones, bolsillos, ganchos, ancho de mochila) se redondean **hacia abajo**; la velocidad de las botas usa pasos de 0,05 m/s.
- Las prendas fabricadas del **Conjunto A** (sección 9.6.1) usan el mismo punto medio redondeado **hacia arriba**, así que igualan o superan un poco al equipo especial, pero se desgastan. El Conjunto B llega al límite del rango.

| Rango especial | Corazones del sombrero | Velocidad de botas, m/s | Bolsillos de camisa | Ganchos de pantalón | Mochila (unidades lógicas) | Límite del rango normal, para comparar |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 4 | 2,25 | 2 | 2 | 8 x 4 | 4 / 2,25 / 2 / 2 / 8 x 4 |
| 2 | 5 | 2,6 | 3 | 3 | 9 x 4 | 6 / 3,0 / 4 / 4 / 10 x 4 |
| 3 | 7 | 3,35 | 5 | 5 | 11 x 4 | 9 / 3,75 / 7 / 7 / 12 x 4 |
| 4 | 10 | 4,1 | 8 | 8 | 13 x 4 | 12 / 4,5 / 10 / 10 / 14 x 4 |

**Razonamiento de precios (oro, números enteros):** el oro se obtiene en el almacén a razón de 1 de oro por cada 1.000 de efectivo, cada diez cajas (100 unidades). Un ciclo del mejor producto de un rango, con receta de nivel 10 y Q5 (referencias de la sección 12.2), da unos **120 de oro en el rango 1** (Misc, 1.200 de efectivo cada uno), **360 en el rango 2**, **720 en el rango 3** y **1.200 en el rango 4**; los ciclos de ropa o comida dan mucho menos y los de materias primas casi nada. Los precios especiales equivalen a unos 1,5-3 ciclos del mejor producto de su rango: son una meta de ahorro, no una compra impulsiva. Las botas (velocidad) y las mochilas (espacio) cuestan más porque son las que más cambian el viaje y la carga.

**Reventa (propuesta):** el equipo especial se vende en campamentos y en el almacén por un valor fijo en efectivo igual a la referencia de nivel 10 y Q5 de su rango, sin prima de durabilidad (ropa 440 / 1.320 / 2.640 / 4.400 de efectivo; mochilas 700 / 2.100 / 4.200 / 7.000 para los rangos 1-4), de modo que comprar y revender nunca devuelve una cantidad de oro significativa.

**Identidad visual:** todo objeto especial lleva la firma del comerciante del globo, **detalles en laca carmesí con remates de latón/oro y un pequeño emblema de globo de latón**, sobre el tema de paisaje y los materiales de su rango, para reconocerlo a simple vista. Cada objeto necesita dos modelos: el modelo de objeto (mochila, espacios del equipo y arrastre) y una versión ajustada para el avatar del jugador, construida con los puntos de anclaje del avatar predeterminado. Los sombreros deben funcionar con los cuatro peinados y las mochilas deben despejar los omóplatos.

**Rango 1 — Caminante** (llanuras templadas, colinas ventosas, bosques de niebla, valles de helechos, pasos de montaña, delta de juncos; fieltro, lona encerada, cuero aceitado):

| ID | Objeto | Espacio | Estadística | Oro | Diseño |
| --- | --- | --- | --- | --- | --- |
| `special.t1.hat` | Sombrero de fieltro del caminante | Sombrero | 4 corazones | 150 | Sombrero de ala ancha de fieltro oliva, cinta carmesí con alfiler de globo de latón, una pluma de faisán |
| `special.t1.boots` | Botas de sendero del caminante | Botas | 2,25 m/s | 200 | Botines de cuero marrón aceitado, cordones carmesí, ojales y punteras de latón |
| `special.t1.shirt` | Camisa encerada del caminante | Camisa | 2 bolsillos | 150 | Camisa de lona encerada color avena, dos bolsillos con solapa y botones de latón, ribete carmesí en el cuello |
| `special.t1.pants` | Pantalón de campo del caminante | Pantalones | 2 ganchos | 150 | Sarga verde musgo, rodillas reforzadas, cinturón de cuero con dos anillas de latón |
| `special.t1.pack` | Mochila de cresta del caminante | Mochila | 8 x 4 | 250 | Mochila de lona con manta enrollada arriba, solapa carmesí, hebillas de latón y pequeño colgante de globo |

**Rango 2 — Orilla del lago** (pantanos, riberas templadas, cuencas, tierras altas soleadas, lagos enlazados, taiga nevada; tela encerada, cuero impermeable, ribetes de piel):

| ID | Objeto | Espacio | Estadística | Oro | Diseño |
| --- | --- | --- | --- | --- | --- |
| `special.t2.hat` | Sombrero de lluvia con pluma de garza | Sombrero | 5 corazones | 500 | Sombrero impermeable estilo marinero, forro carmesí, remaches de latón, pluma gris de garza |
| `special.t2.boots` | Botas de pantano | Botas | 2,6 m/s | 650 | Botas altas impermeables, puño de piel, tiradores carmesí, hebillas de latón |
| `special.t2.shirt` | Sobrecamisa encerada de la orilla | Camisa | 3 bolsillos | 500 | Sobrecamisa corta encerada verde azulado, tres bolsillos, botones de palanca carmesí con puntas de latón |
| `special.t2.pants` | Pantalón de pasarela | Pantalones | 3 ganchos | 500 | Pantalón encerado color pizarra con espinilleras impermeables, cinturón con tres anillas de latón |
| `special.t2.pack` | Mochila de taiga con ribete de piel | Mochila | 9 x 4 | 800 | Mochila de armazón con parte superior de piel, funda encerada carmesí y herrajes de latón |

**Rango 3 — Nómada del sol** (praderas doradas, cañones rojos, dunas, oasis ocultos, meseta, tierras altas con deshielo; envolturas de lino, protección solar, cuero de caravana):

| ID | Objeto | Espacio | Estadística | Oro | Diseño |
| --- | --- | --- | --- | --- | --- |
| `special.t3.hat` | Turbante de dunas | Sombrero | 7 corazones | 1.100 | Envoltura de lino arena con velo ligero, pliegue interior carmesí, broche de latón |
| `special.t3.boots` | Botas de arena del cañón | Botas | 3,35 m/s | 1.400 | Botas envueltas de desierto en ante ocre, ribete carmesí, tachuelas de latón en el tobillo |
| `special.t3.shirt` | Túnica de lino del nómada | Camisa | 5 bolsillos | 1.100 | Túnica-camisa crema de manga larga, faja carmesí, cinco bolsillos ocultos con botones de latón |
| `special.t3.pants` | Pantalón envuelto de caravana | Pantalones | 5 ganchos | 1.100 | Pantalón holgado terracota, pantorrillas envueltas, cinturón con cinco anillas de latón |
| `special.t3.pack` | Alforjas de caravana | Mochila | 11 x 4 | 1.800 | Alforjas dobles de cuero repujado, borlas carmesí, cantoneras de latón |

**Rango 4 — Vigía de mareas** (sabana, acantilados costeros, playas, costa de selva, estuario, costas de coral; lona de vela, cuerda, conchas y latón):

| ID | Objeto | Espacio | Estadística | Oro | Diseño |
| --- | --- | --- | --- | --- | --- |
| `special.t4.hat` | Sombrero de sol con conchas | Sombrero | 10 corazones | 2.000 | Sombrero ancho de paja tejida, cinta carmesí con pequeñas conchas, alfiler de globo de latón |
| `special.t4.boots` | Botas de marea | Botas | 4,1 m/s | 2.600 | Botas de arrecife en lona azul marino con suela de goma, cordones de cuerda carmesí, ojales de latón |
| `special.t4.shirt` | Camisa de lona de vela del marinero | Camisa | 8 bolsillos | 2.000 | Camisa blanca de lona de vela con ribete azul marino, pañuelo carmesí, ocho bolsillos con botones de latón |
| `special.t4.pants` | Pantalón de arrecife | Pantalones | 8 ganchos | 2.000 | Pantalón azul marino remangado, cinturón de cuerda con ocho anillas de latón |
| `special.t4.pack` | Baúl marinero del estuario | Mochila | 13 x 4 | 3.200 | Pequeño baúl de madera con correas, tapa lacada en carmesí, esquinas y cerradura de latón |

**Precios de panadería tras el cambio de tasa:** los precios premium en oro se mantienen (5-10 de oro por porción, 50-100 por receta, 4.500-9.000 por pila completa con descuento). Los jugadores compran cantidades parciales al precio por porción (sección 9.4.4); el descuento por pila completa es para jugadores avanzados. **Ferry y campo maderero (resuelto el 31 de octubre de 2026):** la tabla de peajes del ferry y su alternativa en oro (efectivo / 1.000) están en la sección 12.3; los desbloqueos en oro del campo maderero (50 / 300 / 750 / 1.500 de oro, pensados para unos 450 objetos por árbol enorme), en la sección 13.4.

## 15. Arquitectura C# mantenible

### 15.1 Límites entre capas y dirección de dependencias

Utilice una división pequeña de dominio/aplicación/adaptador Unity. Plain C# posee reglas y estado guardado; MonoBehaviours traduce entradas, transformaciones y vistas de Unity. ScriptableObjects crea definiciones inmutables y las convierte en registros de tiempo de ejecución. No coloque el estado mutable del jugador actual del juego en activos ScriptableObject compartidos.

| Asamblea | Responsabilidades | Referencias de proyectos permitidas |
| --- | --- | --- |
| `TheTrailWalker.Domain` | Identidad del artículo, contenedores, billetera, misiones, estado de elaboración, reglas de herramientas | Ninguno; idealmente no hay dependencia de UnityEngine |
| `TheTrailWalker.Application` | Comandos de transacción, casos de uso, coordinación de sesiones, guardar instantáneas, interfaces de reloj/aleatorias | Dominio |
| `TheTrailWalker.Unity` | Entrada, motor de ruta, cámara, adaptadores de escena, carga de definiciones, audio, representaciones del mundo reutilizadas mediante pooling, adaptador de persistencia | Aplicación, Dominio |
| `TheTrailWalker.UI` | Presentadores uGUI, vistas de arrastre, paneles, HUD, accesibilidad | Aplicación, Dominio; referencias estrechas del adaptador Unity solo cuando sea necesario |
| `TheTrailWalker.Editor` | Creación de contenido, validación de recetas/fuentes, informes de equilibrio | Ensamblajes de ejecución relevantes; Sólo editor |
| Ensamblajes de pruebas | Reglas de dominio, flujos de aplicaciones, integración de Unity por separado | Sólo capas bajo prueba |

Comience con ensamblajes de Dominio y Unity más pruebas en la primera prueba si la creación de los seis inmediatamente ralentiza el aprendizaje. Divida la aplicación/UI cuando exista el segmento de la colección. No se requiere ningún marco de inyección de dependencia: un `GameBootstrapper` construye servicios y pasa referencias explícitamente. Evite un localizador de servicios global y un bus de eventos que oculte la propiedad.

### 15.2 Clases y registros de datos sugeridos

| Sistema | Principales tipos y responsabilidades importantes |
| --- | --- |
| Definiciones | `ItemDefinition` con MaterialTier/permiso global de cocina explícito; ID exacto `RecipeDefinition` con RecipeUnlockTier; curvas de dominio/tiempo, `RegionGatheringProfile` de tres ID, `RegionFishingProfile` de dos especies, botín específico de fuente, presupuesto de sección de campamento/sendero/misión/equipo/muebles/definiciones de tienda |
| Validación de alcance/tienda | `ContentScopeProfile`: 24 espacios regionales, cuatro rangos de recursos activos, catálogo de expansión deshabilitado. `FurnitureDefinition` tiene precio/desbloqueo de casa pero no tiene receta de material; validar todas las referencias y fuentes activas |
| Política de calidad de compra | Ofertas de tiendas físicas/entrega de recompensas use `FixedQ5`; El conocimiento de recetas no tiene calificación. `CampConveyorTrade` conserva el perfil de listado real. Elija por tipo de transacción/recompensa, no por moneda o ubicación |
| Definiciones de pescadería y recompensas de cajas | Dos ranuras de oferta `DiscoveredFish` + una ranura `FisheryExclusiveSeafood` con presupuestos de reinicio de 1,000 unidades; Escribió `RewardEntry` como pila física/equipo/`RecipeUnlock(RecipeId)` con respaldo persistente. La calidad se aplica a las entradas físicas, nunca al conocimiento |
| Definiciones de precios | `PriceCategory` explícito, rango de recursos activo, referencia equivalente a Q1 de nivel diez no material o base material fija; curva de precio de calidad final y curva de valor de durabilidad aplicable; ninguna categoría se infiere del nombre para mostrar |
| Identidad/estado | `ItemStack`, `ItemStatProfile`, `EquipmentInstance`, `ContainerState`, `PlayerState`, `WorldBundle`, ID de definición estable e ID de instancia únicos |
| Inventario | `InventoryService`, `SpatialPlacementRules`, `ContainerTransferCommand`, `StackCompatibility`; sin coordenadas de UI en las reglas de dominio |
| Movimiento/cámara | `ITrailPath`, `TrailMotor`, `SpeedController`, `MovementPauseReasons`, `PostHarvestStopState`, `CameraOrbitController`, `FollowCameraRig`; guiñada independiente, reinicio explícito de la cosecha, instantánea de la velocidad del inventario |
| Entrada | `GestureRouter`, `AvatarSpeedGesture`, `CameraOrbitGesture`, `ResourceGesture`, `InventoryDragController`; bloquear la propiedad horizontal/vertical y nunca cambiar el movimiento desde la órbita |
| Colección | `PickupService`, `ResourcePointState`, `HarvestService`, `LootManifest`, `WorldDropPresenter`, `WorldDropPool` |
| Equipos | `EquipmentService`, `CapabilityCalculator`, `DurabilityService`, `EquipmentExpirationCommand`; eliminar instancias equipadas con durabilidad cero, restaurar valores predeterminados/límites, transferir atómicamente contenidos desplazados |
| Resistencia/recuperación | `StaminaService`, `SegmentStaminaBudget`, `FaintRecoveryService`, `FaintEvent`: drenaje creado solo a distancia (<=26 corazones/sección de sendero), recuperación de comida/campamento, activación por desmayo de cero más no comida, pérdida exacta de 2 a 4 entradas elegibles y estado de vigilia; sin salud de combate |
| Objetivo de caza | `PassiveAnimalPresenter`, `HuntingAimController`: objetivos pasivos móviles, puerta de herramientas transportada elegible, eventos de acierto/error cometidos; no hay ataques al jugador |
| Elaboración | `CraftingService`, `ExactIngredientMatcher`, servicios con reconocimiento de instancias `IngredientReservation`, `EliteUpgradeValidator`, dominio por receta/estadísticas/hora y vistas de reclamos; las actualizaciones consumen atómicamente una instancia Normal exacta más material de rango anterior, nunca equipo a granel |
| Misiones | `MissionService`, `MissionProgress`, `MissionPrerequisiteGraph`, `MissionContributionPolicy`, `MissionNpcPresenter`; cuatro índices de secuencia de rama/rango/requisitos previos de fuente independientes y una misión activa; sin puerta de fase de categoría de receta global |
| Economía | `WalletService`, `CraftPriceCalculator`, `SaleQuoteCalculator`, `TradeSession`, `TradeListing`, `TradeInputGate`, `ShopVisitState`, `WarehouseService`; valor por unidad de punto fijo, totales de 64 bits, calidad/durabilidad artesanal separada y factores de transacción |
| Viajes | `TravelService`, `TrailSceneLoader`, `RegionUnlockState`, `CampVisitState` |
| asentamiento | `HouseService`, `FurniturePlacement`, `BuffRegistry`, `LumberFieldState`, `PetRewardService` |
| Persistencia | `SaveService`, DTO versionados, `SaveMigrator`, `IClock`, `IRandomSource`, ID de transacción |

Utilice IDs con tipo definido o convenciones claras como `wood.pine.log`, `recipe.pine.plank`, `recipe.cheese.cow`, `recipe.cheese.goat`, `recipe.cheese.mixed` y `region.pine_meadows`. Nunca utilice nombres localizados como claves para guardar. La composición/etiquetas ayudan a clasificar la elegibilidad de bolsillo, herramientas, alimentos o grupos de UI; Las etiquetas nunca satisfacen los requisitos de ingredientes de la receta. Las distintas variedades de materiales son ID de artículos distintos, no un artículo genérico con una etiqueta libremente intercambiable.

Guardar el ID de región y el rango de recursos en campos separados. Para la distribución propuesta, las regiones 1-6/7-12/13-18/19-24 se asignan a los rangos 1/2/3/4; nunca introduzca la región 24 en una fórmula de rango/durabilidad/temporizador. Valide que los costos de botín/recetas/misiones/tiendas/casas activas no hagan referencia a especies futuras deshabilitadas. Advierte cuando una receta agrega más de tres ingredientes distintos, rechaza más de nueve y rechaza los resultados de muebles de las recetas de los jugadores. Ningún presupuesto de peso pertenece a las reglas de transferencia/estado del inventario.

Valida exactamente 18 ID sueltas distintas por rango, tres por región, con superposiciones de fuentes limitadas a su rama + piel/piel/pluma confirmada. Los troncos/carne/pescado no pueden aparecer como identificaciones sueltas destacadas. Los íconos de mapas provienen de las mismas definiciones que los de desove. Valide la disponibilidad de fuentes por región, la accesibilidad de recetas de cuatro ramas, segundos conjuntos opcionales contra límites, accesibilidad temprana al campamento/comida y un presupuesto máximo de caminata de 26 corazones para cada sección del sendero. El recuento de campamentos se escala a partir de la plantilla de datos, no del recuento de escenas cargadas.

También valide las nueve recetas Élite: una Normal de exactamente la misma familia/rango, ingrediente extra avanzado del rango `toolTier - 1`, refinamiento conocido del rango anterior, ningún complemento avanzado del mismo rango, cantidad x1 y usos de insumos elegibles positivos según la política de condiciones propuesta. Las tablas de acción Normal/Élite difieren en o por debajo del rango de la herramienta, pero comparten el límite de +2; Solo Élite tiene la bonificación de botín para todos los objetivos elegibles. Las cantidades de misiones de campaña son datos explícitos de rango con diez al inicio y un límite de 150; Las pruebas de límitess del desarrollador son independientes. Los informes de costos incluyen recursivamente las reservas sacrificadas de Normal, refinamiento y casa/misión.

La validación de recetas también rechaza ID de recetas duplicadas, marcadores de posición de familia no resueltos, selectores de etiquetas/listas OR de ingredientes e ID de artículos desconocidos. Cada variedad requerida en una receta mixta intencional debe tener su propia cantidad. Autocompletar puede agregar pilas calificadas del mismo ID de artículo requerido; no podrá utilizar un rango superior ni otra especie. Desbloquea/domina/guarda la clave de registros por ID de receta, por lo que las familias de resultados de apariencia igual nunca fusionan el progreso de recetas distintas.

Aplique la ventana de rango de desbloqueo en cada ingrediente directo: `{1}` para el rango uno, `{R-1,R}` en caso contrario. Solo las identificaciones exactas de despensa/contenedores aprobadas explícitamente como cooking-global pueden omitir el límite de rango anterior, solo para Alimentos y solo cuando se puede acceder a ellos en o antes del rango de desbloqueo. Nunca permita que esa bandera cambie la identidad de entrada o ayude a una receta que no sea alimentaria. Valida las tres recetas de la familia de los pinos, los puentes de cuerda/combustible del rango dos/tres/cuatro y las nueve entradas de Élite contra esta regla. Los depósitos de viviendas y las compras en tiendas siguen siendo tipos de transacciones separados.

Validación de pesca: cuatro ID de peces/rango, seis pares desordenados distintos en las seis regiones de cada rango y exactamente dos ID de peces regionales por perfil. Validar las fuentes de nodos específicos del hábitat y la disponibilidad de la misión; Los tres íconos de recursos sueltos no cambian. Cada pescado propuesto tiene su propia receta de asado exacta utilizando la madera nombrada de su rango, y cada porción repetida/a granel gasta ese combustible. Los filtros de descubrimiento de fuentes se aplican a las ofertas de pesca/caja misteriosa, no solo al desbloqueo amplio de bandas de peces.

La validación de pesca por separado requiere dos ofertas de pescado elegibles distintas y una exclusiva elegible, todas Q5/efectivo, cada una <=1,000 unidades por reinicio con conservación de compra/recarga. Los productos del mar exclusivos nunca entran en los perfiles de peces regionales, en las tablas sueltas, en el suministro de mascotas/NPC ni en las recompensas misteriosas crudas. Valide las veintiséis ID de recetas coleccionables (alimentos/varios, reglas de rango, entradas exactas, disponibilidad de fuentes), seis espacios de caja con tipos de recompensa definidos en total, pesos que suman el 100 %, exclusión de duplicados y afirmaciones de conocimiento/retroceso de una sola vez. El desbloqueo de la misión principal/casa/rango no debe requerir una recompensa de receta aleatoria.

UI se suscribe a eventos de cambio de estado como InventoryChanged, CraftBatchReady y MissionCompleted, y se cancela la suscripción al eliminarlos. Los servicios de dominio no llaman a métodos de UI. El resultado de un comando contiene éxito/fracaso y un motivo de cara al jugador; el presentador decide cómo mostrarlo.

### 15.3 Estructura de carpetas

**Aplicado:** por solicitud del desarrollador, ya existe la estructura siguiente de 52 directorios, con `.meta` de carpeta Unity únicos y `.gitkeep` para conservar directorios finales vacíos en control de versiones. Los recursos/ajustes existentes conservan sus rutas. `Bootstrap.unity` será una escena futura, no un archivo vacío provisional; clases y definiciones de ensamblaje se añadirán con sus hitos. Las revisiones posteriores acompañarán el avance del proyecto.

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

Coloque cada definición de ensamblaje en la raíz del subárbol previsto. Mantenga guardados/compilaciones generados fuera de Assets. Conserve `.meta` en control de versiones y mueva recursos URP/Input con herramientas de Unity para preservar GUID. La estructura de directorios está expresamente aprobada; añadir clases solo cuando las necesite su hito.

### 15.4 Ciclo de vida de escenas y presentación

Bootstrap posee una única raíz de sesión persistente: servicios, estado del jugador, presentación del jugador/cámara del juego, según corresponda, y shell HUD. Las escenas de segmentos de senderos contienen rutas, puntos de recursos, entorno, anclajes de campamento e ID de entidades estables. el asentamiento reemplaza la presentación del sendero y utiliza vistas de edificios seleccionables; no recrea inventario/servicios.

Cargar escenas de forma asincrónica. Vincula al jugador de la partida guardada al ancla de ruta/campamento correcto solo después de que se complete la carga; luego habilite los gestos. Conserve el destino de transición antes de descargar la escena anterior. Evite comandos de transición duplicados y raíces persistentes duplicadas. Mantenga un segmento actual y como máximo un segmento precargado; posponga los Addressables hasta que se demuestre que la carga de escenas incorporada es insuficiente.

**Requisitos adicionales de implementación/validación:** `StaminaService` administra `FoodRecoveryQueue` y registros atómicos de consumo/inserción, integra el balance de marcha/recuperación sin desperdicio por orden de actualización y expone corazones pendientes a desmayo/salida/HUD. `TravelService` separa destinos de campamento de la región actual y destinos de regiones descubiertas, resolviendo explícitamente campamento elegido frente a primero. Validar las 33 recetas opcionales coleccionables (26 globo + 7 bakery) y el acceso regional exacto a cada ingrediente nuevo, no solo al rango. El pastel premium de manzana tiene receta rara de panadería; las siete recetas premium quedan fuera de las 26 recetas de caja (sección 9.4.4).

**Adiciones de implementación/aceptación de sección 20:** guardar procedencia (fabricación/NPC/tienda/botín), nivel de creación, estadísticas inmutables, propietario/temporizador/huella de producto pendiente, coordenadas/desplazamiento horizontal de cofres, depósito/valor por instancia y resto entero en efectivo del ciclo de oro, modo de recolección y estado de visita/salida del asentamiento más revisión de apertura del globo. Migrar páginas y conversión antigua de almacén explícitamente sin volver a pagar recompensas pasadas. Rechazar rotación y filtros de calidad/durabilidad de misiones. Verificar tabla de herramientas y duplicación Élite con mismos modificadores; NPC L1/Q aleatoria/cero muebles frente a fabricación; centro/caída con menú abierto y tiempo avanzando; finalización en mochila/bandeja acredita una vez; equipo usado aceptado en misiones/almacén con identidad y valoración separadas; diez herramientas únicas llenan caja; perfiles mixtos del mismo ID conservan valor; 10.000 de efectivo paga 10 de oro una vez; sin extras; recogidas unitarias y pruebas de 1,000 conservan manifiestos. Sustituir verificaciones antiguas de páginas/bloqueo por calidad/denominación por estos requisitos. Hito 11 crea almacenamiento horizontal, 12 vende todas las categorías con el ciclo de 1 de oro por cada 1.000 de efectivo, 9 persiste perfiles aleatorios/descuentos NPC y 16-17 prueban solo el modo MechanicsTest de 1,000 unidades; el trabajo del modo de lanzamiento queda aplazado hasta una solicitud explícita.

**Verificación de mascotas/trufas:** implementar `PetDefinition`, `PetLootWeightGroup`, `OwnedPets`, `PetOfflineInterval`, `PetRewardContainer` y servicios de compra/reclamación una sola vez. Sección 13.5 define fracciones/fuentes; nunca volver a sortear recompensas pendientes. Añadir Trufa a mapas/orígenes y filtros de NPC/cajas materiales/solicitudes opcionales solo tras región 10; mover Lavanda a catálogo de granja. Validar 72 ID sueltos/18 por rango, tres iconos de región 10 y 26 recetas de caja más una de misión de trufa separada. Implementar misión secundaria de Recolector sin avanzar/reordenar índice obligatorio; mantener una sola misión activa. Verificar recetas sin mascota, rangos/despensa y seis/ocho/nueve corazones progresivos. Guardar ID de misión/desbloqueo y migrar manifiestos antiguos de Lavanda conservando objetos poseídos/confirmados en lugar de convertirlos a Trufa. Nuevas apariciones usan versión nueva.

**Implementación/verificación de nombres y renovación:** localizar 24 regiones y todos los campamentos con regla ordinal de sección 6.2.6; sin prefijos de rango ni cambios en CampId, orígenes, peces o cantidades. Probar idiomas, mapas/Return to Trail de guardados existentes y nombres estables tras recargar. Sustituir cuentas regresivas por visita/salida de asentamiento y revisión de apertura de globo; sin espera ni planificador de renovación. Verificar salida/regreso repone límites de pescadería una vez; vistas internas/reinicio no. Cerrar/reabrir globo vuelve a sortear especial en mismo campamento; apertura duplicada no, compra obsoleta falla sin cobrar y cajas/reservas pagadas permanecen una sola vez. Hito 14 y matriz de tiendas/cajas usan activadores distintos.

## 16. Datos de guardado, temporizadores y recuperación

Construya la persistencia desde el principio porque las cantidades, los lotes pendientes, el botín de nodos, las recompensas únicas y los temporizadores de tres días son mecanismos centrales.

Guardar como mínimo:

- Versión del esquema, versión del contenido, ID del perfil y revisión guardada.
- Contexto actual, ID de región/segmento/campamento, distancia a lo largo del camino, revisión del presupuesto de la sección del sendero creada, velocidad solicitada, estado poscosecha/reinicio manual, último campamento del sendero, orientación de la cámara, instantánea de reanudación del inventario. Nunca cambies la escala del presupuesto restante del equipo actual ni vuelvas a realizar un evento de desmayo mientras restauras la posición.
- Resistencia, estado de tiempo de regeneración en primer plano, familia de herramientas/rango/variante Normal o Élite, instancias de equipo, usos/distancia restante, indicadores de durabilidad infinita y estado de transacción de vencimiento del equipo. Las instancias equipadas con durabilidad cero permanecen eliminadas al recargar, y se conservan las ranuras predeterminadas y la propiedad del contenido desplazado. Los ganchos identifican las herramientas transportadas, no las herramientas equipadas.
- Cada contenedor, posición/orientación fija/cantidad/calidad del artículo, perfiles de estadísticas de salida inmutables (precio unitario base equivalente a Q1, factor de precio de calidad final, factor de valor de durabilidad aplicable, valor unitario intrínseco, durabilidad base ajustada por nivel, bonificaciones de durabilidad de calidad/muebles, durabilidad máxima final, dominio en creación/instantánea de muebles), bolsillos, ganchos, estantes y colas de entrega. Los valores permanecen en punto fijo, los saldos/totales de transacciones son de 64 bits.
- Misión activa, rango de cada una de las cuatro ramas/índice de próxima misión, ID de finalización de requisitos previos, cantidades aportadas, ID de recetas completadas/recompensadas, desbloqueos de variantes de conjuntos de ropa; sin antiguo estado de fase de categoría.
- Recetas conocidas con revisión de contenido/rango de desbloqueo creada por autor, recuentos de dominio por no material, clasificación/categoría/nivel/estadísticas de progresión instantánea pendiente y reservas exactas de ingrediente/combustible, ID de finalización/reclamo e instantáneas de modificador. El permiso global de cocina son datos de definición, no una habilidad otorgada por el jugador o una sustitución almacenada. Los materiales nunca ganan dominio.
- Pendiente Actualización de Élite ID de instancia de entrada/transacción de consumo y cantidades de material anterior, receta de salida/instantánea de estadísticas e ID de reclamo; el Normal consumido no puede reaparecer ni otorgar dominio Normal al recuperarse. Conserve la cantidad creada por misión, no un diez codificado que se aplica después de la recarga.
- Ambas billeteras, peajes pagados, regiones/campamentos descubiertos, cajas parciales del almacén, totales/restos de las últimas diez cajas.
- Visitas al campamento/disponibilidad comercial y registros de liquidación por cancelación; ID de visitas/restablecimiento de tiendas, roles de espacios de oferta seleccionados/ID/cotizaciones del Q5, existencias restantes y compras. La selección de dos peces/uno exclusivo de la pescadería y los límites de población de 1,000 unidades persisten hasta el reinicio real.
- ID de compra de Box y las seis entradas de recompensa con tipo definido, perfiles físicos, ID de recetas reservadas/reclamadas, respaldos duplicados pre-generados previamente y transacciones de reclamo de una sola vez. Los ID de recetas conocidas/nuevos estados de dominio uno persisten atómicamente con las concesiones; no se inventa ninguna cantidad de conocimientos ni resultados elaborados en el Q5.
- Nivel de casa/depósitos, colocación de muebles, desbloqueo permanente de bonificación de primera compra, contenedores horizontales/desplazamientos de cofres de almacenamiento.
- Desbloqueos de rango de madera, todos los progresos/cronómetros de los árboles, manifiestos de botín restantes.
- Mascota activa, marca de tiempo de liquidación sin conexión y recompensas de mascotas pendientes.
- ID de evento de desmayo, ID/cantidades de entrada elegibles seleccionadas, indicador de compromiso de pérdida, progreso/objetivo de recuperación y fase de animación. Recargar reanuda ese evento una vez, nunca reembolsa/elimina nuevamente y nunca selecciona equipo equipado.

Campos adicionales obligatorios de guardado: entradas de comida pendiente (ID de consumo, recuperación de la porción, corazones restantes/orden, tiempo fraccionario) y estado atómico de consumo; selección regional del mapa, tramos/campamentos desbloqueados, ID de regiones descubiertas y destino/confirmación de viaje en curso. Cargar un guardado anterior inicia una cola vacía sin conceder recuperación ni cambiar descubrimientos.

Utilice inicialmente un formato DTO de instantánea versionada, como JSON, con serialización y migraciones de enumeración/ID explícitas. Escriba en un archivo temporal, vacíe y reemplace el archivo guardado principal mediante operaciones admitidas por la plataforma y conserve una última copia de seguridad en buen estado. Valide recuentos, identificaciones, límites y duplicados durante la carga; Dirija los problemas de colocación recuperables a una bandeja. Nunca sobrescriba un archivo guardado con un esquema más reciente desconocido sin que surja la incompatibilidad.

Guardar las transacciones críticas inmediatamente o mediante una cola breve que agrupe escrituras y termine antes de confirmar recompensas locales irreversibles. Guardar también al pausar la aplicación o perder el foco: el sistema operativo móvil puede terminar el proceso sin ejecutar su cierre normal. Al reanudar, reconciliar las marcas de tiempo una sola vez y actualizar las vistas. Usar tiempo monotónico transcurrido durante la sesión activa y UTC para temporizadores entre sesiones. Inyectar el reloj y la fuente aleatoria para obtener pruebas repetibles.

Ejemplos de pruebas de recuperación de fallas: forzar el cierre después de gastar los ingredientes pero antes de las actualizaciones de la interfaz de usuario; después de que un anuncio llegue a la trituradora pero antes de la animación de pago; después de cargar el oro pero antes de que se abra la caja misteriosa; durante una transición en ferry; después de que se obtengan las recompensas de los árboles pero antes de que aparezcan los paquetes. Cada recarga debe producir un resultado comprometido válido sin elementos duplicados, cargos dobles o recompensas desaparecidas.

## 17. Configuración y rendimiento en Android

### 17.1 Configuración inicial

1. Verifique la compatibilidad con la compilación de Android de Unity Hub, las herramientas SDK/NDK y OpenJDK para esta versión del editor.
2. Seleccione/cree un perfil de compilación de Android; establezca un identificador de aplicación específico del proyecto.
3. Conserve IL2CPP, ARM64 y Vulkan/OpenGL ES 3 inicialmente; verifique en al menos un teléfono compatible con Vulkan y un dispositivo de respaldo apropiado, si está disponible.
4. Habilite el ritmo de cuadros optimizado y elija un objetivo inicial explícito de 30 FPS.
5. Vista previa con el activo de calidad móvil. Desactive HDR y posprocesamiento para la primera pasada; reduzca la distancia de la sombra a aproximadamente 20-30 metros y use luz unidireccional.
6. Prototipo vertical con diseño que reconoce el área segura; Pruebe la legibilidad de elaboración/inventario antes de bloquear la orientación.
7. Cree un APK de desarrollo inmediatamente, antes de cualquier producción de contenido importante. La configuración AAB/firma de tienda y la distribución quedan fuera del alcance actual y requieren una instrucción explícita futura; seguir usando APK de desarrollo locales para pruebas.

Los paquetes instalados pueden permanecer anclados. Agregue una versión de Splines compatible con Unity cuando las rutas curvas creadas lo necesiten; Mantenga un adaptador de waypoint para las pruebas. Cinemachine 3 es opcional para la cámara. La navegación AI no es necesaria para el jugador. Visual Scripting, Multiplayer Center, Timeline e integraciones IDE no utilizadas se pueden eliminar más adelante después de verificar las dependencias; La limpieza del paquete no es un requisito previo para el juego.

### 17.2 Objetivos de medición de rendimiento

Estos son objetivos de ingeniería iniciales a medir, resultados aún no alcanzados:

| Área | Objetivo inicial/prueba |
| --- | --- |
| Estimulación del cuadro | Estable 30 FPS, presupuesto de cuadro de 33.3 ms; comprobar el juego sostenido durante 15-20 minutos |
| Asignación gestionada | No hay asignaciones rutinarias por cuadro en actualizaciones de HUD estáticas, de entrada y de movimiento |
| Botín mundial | 24 vistas de paquetes agrupadas simultáneas; manifiestos pendientes lógicos mucho más grandes |
| Inventario | Una representación por pila/instancia; virtualizar entradas fuera de pantalla y filas de recetas |
| Elaboración a granel | Apilable x1000 completa/reclama sin 1,000 GameObjects/tarjetas UI; se rechazan las solicitudes no acumulables distintas de x1 |
| Ciclo de vida de la escena | Sin crecimiento repetido en objetos de escena cargados, oyentes o memoria después de 20 transiciones |
| Guardar/cargar | Perfil con los 24 árboles madereros activos, 24 registros de descubrimiento de regiones, cofres de almacenamiento completos y artesanías pendientes; medir los bloqueos del hilo principal |
| Toque/cámara | Los gestos de UI/recursos siguen respondiendo durante las partidas guardadas/generación de botín; La órbita de 360 grados no cambia la velocidad ni asigna cada cuadro; los recursos cercanos situados detrás siguen pudiendo seleccionarse |

Utilice materiales compartidos, atlas de texturas, colisionadores simples, iluminación estática o horneada cuando sea adecuado, LOD de malla, follaje modesto e instancias de GPU para mallas de entorno compatibles repetidas. Prefiera imágenes estilizadas de bajo costo en lugar de agregar efectos de pantalla completa. No simules cada unidad caída con física; los full stacks confirmados deben permanecer baratos en representación.

Registre el modelo del dispositivo, el sistema operativo, la revisión de compilación, la API de gráficos, el tiempo de fotograma de CPU/GPU, la memoria, la temperatura/comportamiento térmico y el escenario de prueba. Establezca el presupuesto de memoria real después de medir el dispositivo mínimo previsto en lugar de inventar un límite universal. Utilice la compresión de texturas de Android apropiada para la cobertura objetivo, pruebe ASTC cuando sea compatible y proporcione un respaldo compatible si esa cobertura lo requiere.

## 18. Plan de acción paso a paso

Implemente un hito a la vez. Cada uno debe producir una pequeña construcción utilizable o una prueba de reglas demostrable antes de que comience el siguiente subsistema importante. Las estimaciones deben realizarse después de los dos primeros hitos de aprendizaje de Unity; fechas aquí implicarían una certeza injustificada.

### Hito 0: Seguridad del proyecto, escena de aprendizaje y medición inicial en dispositivo

Dependencias: ninguna.

1. Confirme/revise el estado de la plantilla y mantenga este documento más `Trail.md` en control de versiones.
2. Verifique la versión/cadena de herramientas de Unity y cree el perfil de compilación de Android.
3. Cree una escena prototipo desechable para aprender GameObjects, componentes, prefabricados, serialización, carga de escenas, Canvas y el sistema de entrada.
4. Configure la calidad móvil, el área segura en orientación vertical provisional, la ID de la aplicación, la límite de FPS y cadencia de fotogramas.
5. Produzca e instale un APK de desarrollo con una primitiva iluminada y un botón de interfaz de usuario.

**Comprobación de salida:** la escena se inicia en un teléfono, el botón responde y una breve captura del perfilador establece una línea de base. No se necesita publicación en la tienda.

**Punto de recuperación — 29 de octubre de 2026: comprobación de salida del Hito 0 superada en dispositivo.**

- Se revisaron los commits de plantilla existentes (`8a2c4da`, `5f0fd2f`); el plan en inglés y `Trail.md` ya están versionados. La implementación nueva y los cambios de configuración siguen sin commit.
- Unity `6000.4.7f1`; perfil Android Development; IL2CPP/ARM64; identificador `com.thetrailwalker.prototype`; calidad Mobile, área segura vertical, límite de 30 FPS y ritmo de fotogramas optimizado.
- Escena: `Assets/_Game/Scenes/Prototypes/AndroidBaseline.unity`. Comandos del editor: **The Trail Walker > Milestone 0 > 1 - Prepare Android Baseline** y **2 - Build Development APK**.
- APK generado e instalado en nubia NX769J (Android 16, Adreno 750, Vulkan, 1116 x 2480). El usuario confirmó que el botón cambia el color del cubo.
- Captura del dispositivo a las `2026-10-30T03:10:32Z`: 299 fotogramas en 10.005 segundos, **29.88 FPS**, **33.46 ms de intervalo medio**, 136,416,138 bytes de memoria asignada por Unity y un toque registrado antes de terminar. Incluye sobrecarga del profiler; el intervalo no equivale al tiempo de ejecución CPU/GPU ni constituye una prueba térmica.
- Archivos locales: `Builds/Android/TheTrailWalker-Milestone0.apk` y `Builds/Android/Baseline-2026-10-29/milestone0.json` / `milestone0.raw` (13,624,745 bytes). Se recuperó la captura, pero aún no se ha inspeccionado en Unity Profiler. Git ignora los artefactos de compilación; conservar una copia aparte antes de borrar `Builds`.
- SHA-256 del APK: `AE4A519D63D0C36D847846C4964FDB2F82F286CA11C12FA5CF9C49EDB8536BF9`.
- Siguiente implementación: Hito 1. Conservar esta referencia funcional para comparar.

### Hito 1: Movimiento hacia delante y asignación de gestos
Dependencias: 0.

**BANDERA DE REPETICIÓN — comprobación condicional de integración; base completada conservada.** Las tablas nuevas no cambian camino/velocidad, pero recogida/comida comparten gestos y estado de parada. Al integrar objetos/controles nuevos: recoger ingrediente de árbol mientras se camina/gira, abrir detalles, inspeccionar crudo frente a usar comida preparada y cerrar mochila. Solo reanudar intención válida; ningún gesto debe además recoger/comer/cambiar velocidad involuntariamente. Parada por cosecha/desmayo/manual conserva prioridad. Finalizar talado real se prueba con Hito 6. Estado: pendiente de integración, sin resultado nuevo de pruebas.

**Adiciones de revisión — 30 de octubre de 2026 (requisitos, no verificación nueva):** usar cámara en primera persona y zona SPEED central de la revisión 13:17 del Hito 4; las instrucciones originales de avatar/franja izquierda son históricas. Mantener exclusión de notificaciones/área segura y propiedad del gesto por eje. Comprobar velocidad/giro/recogida con mochila, paneles laterales y ventana de comida: Comer, arrastrar o cerrar una ventana no cambia además velocidad ni recoge recursos. Respetar intención válida al cerrar inventario y prioridad de paradas por desmayo, campamento, recolección y agotamiento. Comprobar límites al caducar botas: base 0.8 m/s y máximo actual 2.0 m/s; girar cámara no cambia distancia del camino. Son regresiones de los controles vigentes.

1. Cree Bootstrap y un camino curvo de 100 a 250 m con un ancla de campamento en su extremo.
2. Implemente una evaluación de ruta basada en distancia, velocidad solicitada, límites y una cámara de seguimiento con órbita horizontal completa independiente de 360 grados.
3. Implemente gestos de velocidad vertical solo para avatar, órbita de retención/deslizamiento horizontal, propiedad de gestos direccionales y controles del editor; sin palanca de mando. Orbit nunca detiene ni dirige al jugador.
4. Agregue un marcador de posición de inventario con instantánea/reanudación de velocidad previa a la apertura, motivos de pausa temporal, estado de reinicio manual persistente posterior a la cosecha y manejo de suspensión de aplicaciones.
5. Agregue la parada automática del campamento y una visualización de depuración de los motivos de distancia/velocidad/pausa.

**Comprobación de salida:** sin movimiento hacia atrás o hacia los lados; la órbita completa de la cámara en reposo/mientras camina deja la velocidad/dirección del camino sin cambios; los gestos horizontales y verticales no interfieren; soltar el gesto de cámara conserva la guiñada elegida; el inventario restaura solo su velocidad guardada sujeta a límites/otras paradas; cerrarlo durante una parada poscosecha no reinicia; La llegada al campamento nunca se excede.

**Punto de implementación — 29 de octubre de 2026: comprobación de salida del Hito 1 superada.**

- `Bootstrap.unity` crea una raíz de sesión persistente y carga `MovementTrail.unity` de forma asíncrona. El sendero curvo de 158.53 m usa Catmull-Rom muestreado por longitud de arco, avatar orientado según el camino, marcadores y ancla de campamento final. Abrir `Assets/_Game/Scenes/Bootstrap.unity` para probar en el editor.
- `MovementState`, en el ensamblado Domain independiente del motor, conserva distancia, velocidad solicitada, límite inicial de 0.8 m/s (máximo 1.6), instantánea del inventario anidado, pausas independientes e intención posterior a la recolección. `TrailMotor` depende de la distancia, nunca del ángulo de cámara. El prototipo comienza detenido; un gesto ascendente sobre el avatar inicia la marcha.
- Enhanced Touch da prioridad a la interfaz y fija el eje dominante de cada contacto. Los gestos verticales en el mundo no cambian velocidad; dos dedos pueden controlar velocidad y cámara por separado. Soltar/cancelar, inventario, pérdida de foco y suspensión limpian la asignación. La cámara conserva su ángulo al soltar. En el editor se puede arrastrar con ratón o usar la rueda sobre el avatar.
- La interfaz muestra distancia, velocidades real/solicitada, límite, ángulo, pausas y parada posterior a recolección. **INVENTORY** prueba pausa/reanudación. **TEST HARVEST** simula una interacción de dos segundos (otro toque cancela), tras la cual hace falta un gesto ascendente nuevo. **CAP: 0.8 / 1.6** y **RESTART TRAIL** son controles de prueba. La mochila aún no gestiona objetos.
- **The Trail Walker > Milestone 1** ofrece preparación, validación y compilación. Preparar conserva escenas existentes. Compilar ejecuta primero las **34 comprobaciones de movimiento/camino/gestos** de `Assets/_Game/Editor/Milestone1Validation.cs`. Informe: `Builds/Validation/Milestone1-checks.txt`.
- Instalado y probado en nubia NX769J: velocidad vertical; órbita caminando/detenido; bloqueo vertical fuera del avatar; restauración de inventario y bloqueo de cámara; cierre de inventario después de recolección y reinicio ascendente; suspensión/reanudación. El usuario confirmó buena respuesta de ambos gestos. Unos 20 segundos en segundo plano conservaron exactamente la distancia. La llegada al campamento detuvo al jugador en **158.529296875 m**, igual a la longitud del camino, con velocidades real y solicitada en cero.
- La primera compilación reveló eliminación de tipos de colisión creados en ejecución; `Assets/_Game/link.xml` los conserva y el APK reconstruido superó las pruebas. APK: `Builds/Android/TheTrailWalker-Milestone1.apk`. Evidencia JSON/capturas: `Builds/Android/Milestone1-Verification/`. Git ignora los artefactos de compilación. La aplicación de desarrollo escribe `milestone1.json` en su directorio persistente para diagnóstico.
- La intención de sesión sobrevive a pausas, no al cierre del proceso; el guardado duradero corresponde al Hito 3. Inventario real, gasto de energía, recolección real, evitación de obstáculos altos y arte final quedan pendientes. Los cambios de los hitos siguen sin commit.

### Hito 2: Modelo de objetos, capacidad espacial y recogida de pilas completas

**BANDERA DE REPETICIÓN — requerida al integrar definiciones; base completada conservada.** Motivo: cuatro IDs crudos activos, dos refinados y sus comidas amplían catálogo; frutas/savia futuras comparten fuentes. Con objetos de prueba, comprobar reconocimiento, categoría/disponibilidad, ID estable, cantidad, calidad/perfil/huella, pilas 1/999/1.000, recolección parcial exacta y mochila llena sin pérdida. Al activar expansión, mismo ID/perfil de árbol y suelto debe fusionarse; procedencia no crea variedad nueva. Savias/harinas diferentes no se fusionan. Crudo sirve como ingrediente, no comida preparada; conocimiento de receta no ocupa pila. Estado: pendiente al integrar catálogo. Manifiestos reales de cosecha pertenecen a Hito 6 y gravedad sigue aplazada.

**Adiciones de revisión — 30 de octubre de 2026 (requisitos, no verificación nueva):** el contenedor de posiciones fijas es la base implementada; gravedad/colisiones, caída central, banda de sobrecarga del 20% de altura interior y derrame por vencimiento pertenecen al trabajo de inventario aplazado descrito en Hito 4. Separar regresiones de ambas versiones. Recoger un objeto soltado por el jugador conserva cantidad restante, calidad, perfil intrínseco y durabilidad; nunca concede otras 1.000 unidades. Soltar/recoger transfiere un único propietario atómicamente y no duplica tras interrupción. Diferenciar comida preparada e ingrediente crudo; recuperación depende de definición/perfil, no solo calidad. Las ofertas de recetas de panadería otorgan un conocimiento, nunca pilas físicas de 1.000 pergaminos ni comida Q5. Catálogo/pago pertenecen al Hito 14. Conservar comprobaciones de pilas 1/999/1.000 y fusiones compatibles/incompatibles para las futuras comidas.

**Punto de revisión de primera persona — 30 de octubre de 2026:** el Hito 0 sigue como medición independiente. Los Hitos 1 y 2 usan cámara a la altura de los ojos y giro independiente de 360 grados. La superficie izquierda SPEED recibe el gesto vertical incluso si hay un recurso detrás. El router no captura contactos que comienzan en el 8% superior o por encima del área segura. La recogida depende del alcance visible y de obstáculos desde la cámara, no de los límites anteriores de 8 m/15 m. Se separaron los lotes de prueba sin cambiar contenido regional ni economía.

Se recompilaron ambos APK de desarrollo. Pasaron 34 comprobaciones de movimiento y 35 de inventario/gestos. En nubia NX769J se recogió una pila de piedra Q3 de 1,000 unidades más allá del radio anterior; SPEED cambió la marcha sin recoger el objeto detrás; y el usuario confirmó notificaciones y recogida debajo del borde. Evidencia: `Builds/Android/FirstPerson-Verification/` (ignorada por Git). Quedan pendientes otras alturas/insets, objetos superpuestos u ocultos, legibilidad lejana y reacción humana a velocidad máxima. El punto anterior es histórico; sus cámara y límites quedan sustituidos.

Dependencias: 1.

1. Agregar ensamblaje de dominio e ID de elementos estables; Rama/tronco de pino de autor, piedra, cáñamo, bayas y una herramienta inicial.
2. Implemente pilas con límite de 1,000 unidades, cinco grados de calidad, instancias de equipos únicas y ajuste determinista de contenedores 2D.
3. Agregue paquetes mundiales, mantenga presionado y arrastre, deslice rápido hacia abajo, apertura de mochila y presentación de recogida. Conserva el botín cercano detrás del avatar dentro de una ventana de recolección trasera configurable; apunte a ellos a través de la cámara rotada sin detener la marcha ni pasar por alto el rango/oclusión.
4. Agregue crecimiento de pila, combinación táctil compatible, comentarios de rechazo e insignias de cantidad.
5. Agregue comandos de depuración para formas mixtas, cantidades 1/999/1000 y una bolsa completamente llena.

**Comprobación de salida:** cada recogida apilable de sendero comienza en 1,000; sin duplicación de toques repetidos; la colocación fallida lo deja intacto; las diferencias de calidad/perfil permanecen separadas; las herramientas no se pueden apilar; el cambio de tamaño no puede superponerse/escapar de la bolsa; La órbita de la cámara revela recursos cercanos perdidos para su recolección sin detener al jugador o ir más allá de la ventana permitida.

**Punto de implementación — 30 de octubre de 2026: comprobación de salida del prototipo del Hito 2 superada.**

- Abrir `Assets/_Game/Scenes/CollectionBootstrap.unity`; carga `CollectionTrail.unity`. Se conservan escenas/APK del Hito 1. **The Trail Walker > Milestone 2** ofrece preparación, validación y compilación Android de desarrollo.
- Domain define ID estables de rama/tronco de pino, piedra, cáñamo, bayas y hacha Normal; Q1-Q5; estado listo/perfiles funcionales inmutables; identidad única de equipo; y propiedad World/Backpack/Consumed. Los objetos apilables del mundo empiezan en 1,000 y las herramientas son individuales. Troncos/herramientas son datos de prueba, no cambios a recursos sueltos regionales.
- Mochila de 6 x 4 con posiciones deterministas en milésimas de unidad, orientación fija, sin casillas visibles ni peso. Huella/modelo crecen hasta 1.5x. Guardado rápido intenta fusionar y después buscar espacio. Arrastre manual fusiona huellas compatibles que se tocan. Se valida crecimiento antes de modificar; restos parciales quedan en origen y rechazos conservan propiedad, cantidad y posición.
- Deslizar un recurso hacia abajo aproximadamente 10% de pantalla en 250 ms guarda rápido; mantener al menos 250 ms y arrastrar permite colocarlo en mochila/abertura automática. Abrir mochila pausa y permite mover contenido; cerrar restaura intención válida. El contacto de recurso no puede convertirse en entrada de cámara/velocidad. Recoger del mundo no pausa la marcha.
- Los registros sobreviven a giros de cámara y cambios de visibilidad. Valores de prueba: ventana trasera de 15 m, radio físico de 8 m, visibilidad desde cámara/jugador verificada otra vez al transferir. Solo 18 modelos de lotes con etiquetas, nunca un modelo por unidad.
- **EMPTY**, **MIXED** y **FULL BAG** reemplazan deliberadamente inventario y regeneran lotes cerca de la ubicación actual. MIXED cubre 1/999/1000, formas, calidades, valores diferentes y herramienta única; FULL BAG llena el contenedor. **RESTART** vuelve al inicio con mochila vacía. Siguen habilitadas depuración y pilas completas; no hubo reequilibrio ni preparación/despliegue de lanzamiento.
- Superadas **30 comprobaciones de inventario**, incluidas **1,000 operaciones aleatorias con semilla**, y las **34 de regresión del Hito 1**. Informes: `Builds/Validation/Milestone2-checks.txt` y `Milestone1-checks.txt`. En nubia NX769J pasaron **12 comprobaciones registradas**: recogida rápida/manual, movimiento/rechazo, fusión 1+999, perfiles separados, mochila llena, recogida trasera caminando a 0.2 m/s, rechazo por distancia, selección ocluida, conservación y límites sin solapamiento. El usuario confirmó ambos gestos.
- APK: `Builds/Android/TheTrailWalker-Milestone2.apk`. Capturas, instantáneas y `device-checks.json`: `Builds/Android/Milestone2-Verification/`. Git ignora estos artefactos. El archivo del dispositivo `milestone2.json` solo es diagnóstico, no una partida restaurable.
- Al completar el Hito 2, inventario/botín se reiniciaban al cerrar el proceso; el Hito 3 siguiente ya añade persistencia. División de pilas, misiones, recolección real, equipo y contenido de producción quedan para después. Cambios sin commit.

### Hito 3: Bases del guardado y recuperación

**BANDERA DE REPETICIÓN — requerida para catálogo/guardado; base completada conservada.** Motivo: validación de IDs conocidos puede rechazar definiciones nuevas, reinterpretar identidad o duplicar propiedad al añadir ingredientes/refinados/comidas/fuentes. Guardar/recargar cada objeto nuevo activo con cantidades parciales y perfiles compatibles/incompatibles; cerrar proceso tras recoger/soltar/recoger y recuperar copia de respaldo. Conservar IDs, cantidades, propietarios, perfil original y progreso de comida consumida; no restaurar entrega ya gastada o desperdiciada. Cargar ingrediente/procedencia no concede receta ni descubrimiento. Hito 6 amplía interrupciones de reclamación madera/ingrediente; expansión prueba IDs de doble fuente y migración/aislamiento de definiciones desactivadas. Descubrimiento/recetas/transacciones comerciales siguen en Hitos 5/7/14. Estado: pendiente de integración, sin invalidar prueba histórica.

**Adiciones de revisión — 30 de octubre de 2026 (requisitos, no verificación nueva):** Hito 4 amplía guardado con consumo/inserción en cola atómicos, recuperación por porción, saldo/orden pendiente, tiempos fraccionarios, desgaste/límites de equipo y estado/transacción de desmayo. Guardar procedencia de objetos soltados y cantidades exactas; recargar no crea ciclo de botín nuevo. Añadir casos de interrupción/migración al implementar cada función, conservar perfil original y evitar consumo/pérdida repetidos. Preparar extensiones para IDs de recetas, conocimiento/dominio y fuentes descubiertas: Hito 5 persiste descubrimiento y Hito 14 guarda tipo de oferta, ID de comida/receta, stock, precio, visita y transacción pago/desbloqueo. Cargar un objeto o vista previa bloqueada no concede descubrimiento ni receta. Migración sin descubrimiento lo conserva sin descubrir; fuentes exigidas provienen de definiciones. Comida comprada o ingredientes regalados no prueban descubrimiento regional. Guardados de tiendas conservan ofertas y vuelven a validar fuentes antes de cobrar, sin sortear al recargar. Son obligaciones de integración futura de los hitos indicados, no funciones ya implementadas en Hito 3.

**Seguimiento de primera persona:** guardar y restaurar ángulo de mirada y configuración aplicable de alcance/área táctil segura junto a movimiento y objetos del mundo; cargar no debe cambiar el alcance ni duplicar recogidas.

Dependencias: 2.

1. Guarda inventario, paquetes de mundo cercano que incluyen botín situado detrás del jugador, posición de ruta, velocidad solicitada, orientación de la cámara, instantánea de reanudación del inventario según corresponda, estado de reinicio posterior a la cosecha y estado del jugador.
2. Implemente DTO versionados, respaldo/recuperación, ID de instancia estable y registros de transacciones cuando sea necesario.
3. Guardar tras recogidas críticas y al pausar la aplicación; restaurar sin actualizar el segmento actual.
4. Agregue reloj inyectado/fuente aleatoria y una acción de reinicio del perfil del desarrollador con confirmación.

**Comprobación de salida:** forzar el cierre después de la recopilación y volver a abrir; las cantidades y la ubicación sobreviven; la instantánea principal dañada puede recuperar la copia de seguridad; La carga nunca duplica la propiedad del mundo y del inventario.

**Punto de implementación — 30 de octubre de 2026: comprobación de salida del prototipo del Hito 3 superada.**

- El prototipo de recolección guarda una instantánea versionada con suma de verificación en `Application.persistentDataPath/PrototypeSession/session-v1.json`: distancia, velocidad solicitada/límite, mirada, pausa/restauración de mochila, parada posterior a recolección, campamento, identidades/calidades/perfiles/cantidades/posiciones/propietarios y posiciones de lotes del mundo. Se omiten fuentes consumidas. Al abrir conserva el ciclo y no genera lotes nuevos.
- Las transferencias exitosas se guardan inmediatamente; cambios de sesión se guardan periódicamente cuando cambian y al suspender/perder foco/salir. Un temporal vaciado se reemplaza atómicamente con respaldo `.bak` previo. La carga valida suma, versión, definiciones conocidas, ID únicos, cantidades, movimiento y ajuste de mochila. Si falla el principal se recupera el respaldo; pueden perderse cambios posteriores a este. Un guardado nuevo incompatible o irrecuperable se conserva y bloquea escrituras hasta el reinicio de depuración elegido por el usuario.
- **RESET SAVE** requiere dos toques en cinco segundos y archiva los archivos antes de crear un perfil nuevo. EMPTY/MIXED/FULL BAG siguen como pruebas explícitas que reemplazan la sesión. `IClock`, `IRandomSource` y un generador con semilla son bases inyectables; los lotes actuales son deterministas.
- **The Trail Walker > Milestone 3** valida y compila `Builds/Android/TheTrailWalker-Milestone3.apk`. Pasaron 13 pruebas de persistencia y las 34 de movimiento / 35 de inventario. En nubia NX769J se registraron siete comprobaciones: recogida de 1,000 tras cierre forzado, lotes restantes en mundo, pausa de mochila/distancia y mirada tras recarga, respaldo tras corrupción deliberada, parada posterior a recolección tras recarga y cierre de mochila, e identidades únicas. Evidencia: `Builds/Android/Milestone3-Verification/` (ignorada por Git).
- Es el primer esquema del prototipo; `milestone2.json` sigue siendo solo diagnóstico y no se migra. Equipo, misiones, transiciones posteriores y sistemas futuros deben añadir sus datos y migraciones al implementarse. Cambios sin commit.

### Hito 4: Equipo, bolsillos, ganchos y resistencia

**BANDERA DE REPETICIÓN — requerida para clasificación/recuperación; base completada conservada.** Motivo: ingredientes crudos de árbol no deben heredar Comer/consumo automático de comida preparada. A cero, probar solo piñones/savias/vainas/bellotas: sin consumo automático ni prevención de desmayo. Con comida preparada lista, consumir automáticamente una sola porción; harina/comida en fabricación pendiente no se usa. Mostrar Comer manual solo si definición lo permite; retirar una porción sin otorgar conocimiento. Probar platos nuevos de 2/3/4/5 corazones y objeto de doce, pilas completas, tasa de 1/3 corazón/segundo activo, suspensión/recarga, caducidad de sombrero y transacciones de desmayo. Cierre de mochila conserva prioridad de paradas.

**Resultado vigente esperado:** revisión posterior sin reserva de sección 4.4.3 del documento inglés: entrega continúa a tasa fija y excedente sobre máximo se pierde. A 3/3 detenido, porción de doce se desperdicia progresivamente durante 36 segundos activos; suspensión congela entrega restante. Bajo máximo recupera progresivamente; caminar aprovecha entrega simultánea. Varias porciones no multiplican tasa. Comprobaciones antiguas de conservar reserva/pausar al máximo quedan sustituidas como criterio de aceptación; la copia española antigua necesita reconciliarse con esa revisión antes de probar. Esta nota no redefine caducidad de sombrero: comprobar cambio de máximo contra regla vigente de 4.4.3. Estado: pendiente de repetición focalizada; compras de panadería siguen Hito 14 y fabricación real Hito 7.

**Adiciones de revisión — 30 de octubre de 2026 (requisitos, no verificación nueva):** tasa actual compartida de campamento/desmayo/comida: **1/3 corazón por segundo activo**; desde cero, tres corazones tardan 9 segundos y doce tardan 36. Los valores 0.25 y 12/48 segundos siguientes corresponden a versiones anteriores. Drenaje actual del tramo de prueba: 12 corazones/km; máximo de botas: 2.0 m/s; drenaje regional sigue configurable. Antes de cerrar el hito, comprobar sección 4.4.3: consumir comida de doce corazones a 3/3, conservar doce pendientes estando lleno, caminar y entregar progresivamente, varias porciones sin multiplicar tasa, caducar sombrero sin borrar reserva y cerrar proceso tras consumo sin duplicarlo. Usar comida temporal de prueba si no existe panadería/fabricación, sin desbloquear receta ni declarar panadería implementada. Resolver recuperación pendiente antes de comer automáticamente comida preparada o desmayarse; ingredientes crudos no evitan desmayo y productos pendientes no se comen. Probar una sola porción automática, fin de cola/límite cero y tolerancia de 0.0001 corazón, recuperación en menús activos, pausa en suspensión y despertar parado. Verificar perfil original, vencimiento simultáneo y propiedad de paneles. Derrame/sobrecarga por gravedad siguen aplazados al hito dedicado; desbordamiento de depuración es temporal. Hito 14 reutiliza pruebas con siete comidas reales y verifica fuentes/pago aparte. Registrar evidencia nueva de dispositivo antes de declarar aprobadas estas adiciones.

**Punto de recuperación — 30 de octubre de 2026: Hito 4 sigue en curso.** El prototipo separado `SurvivalBootstrap.unity` / `SurvivalTrail.unity` integra resistencia, equipo de prueba, bolsillos/ganchos, desbordamiento, comida, vencimiento y manifiesto persistente de pérdida por desmayo. El esquema 2 usa `session-v2.json` junto al `session-v1.json` del Hito 3 sin modificarlo; la migración copia el perfil anterior, limita la velocidad de depuración al equipo inicial y conserva el original. Continúan pilas completas y herramientas de depuración.

- El usuario confirmó regeneración de resistencia de **0.25 corazones/segundo**. Aplicar a zonas seguras y recuperación de desmayo; la comida también se entrega a 0.25 corazones/segundo. Desde cero, recuperar tres corazones tarda 12 segundos y doce corazones 48. Quedarse quieto en sendero no regenera gratis. Esta instrucción sustituye las propuestas anteriores de 0.05/segundo y desmayo fijo de 12 segundos.
- Un APK anterior superó pruebas en teléfono de migración, cinco ranuras de rango cuatro, bolsillos/ganchos, vencimiento simultáneo conservando contenido, desgaste por distancia, cero resistencia con comida accesible, comida gradual y manifiesto de 2–4 entradas que sobrevivió al cierre forzado sin cobrar dos veces. Un sombrero de repuesto se equipó, restauró tras cierre y venció. En campamento se observó regeneración de aproximadamente 0.25 corazones/segundo. Evidencia: `Builds/Android/Milestone4-SurvivalTest/` (ignorada por Git); el v1 original se copió a `01-m3-original.json` antes de probar.
- Después de esas pruebas se añadió conservación de equipo usado al cambiar lotes de depuración, ventana de detalles/Comer, límite preciso de vencimiento, selección/retirada por ranura y persistencia de durabilidad fraccional. **Estos cambios recientes aún no han superado una nueva compilación Unity/Android ni pruebas en dispositivo.** La última petición de compilación devolvió `FAILED: Milestone 4 build failed: Failed`; aún no se ha diagnosticado la causa detallada de Unity. El APK guardado a las 11:00:43 es anterior y no verifica el código actual. El conjunto local del modelo pasó 54 comprobaciones; el último informe Unity de migración completado contiene 16, y las comprobaciones nuevas esperan una compilación/ejecución correcta.
- Próxima sesión: revisar diagnósticos completos de Unity y compilación de ensamblados, corregir el fallo, repetir comprobaciones de Hitos 1–4 e instalar/probar la compilación final. Verificar retiro y reequipamiento reales con durabilidad fraccional tras cierre forzado, conservación de equipo al usar MIXED/FULL BAG, desbordamiento por pérdida simultánea de capacidad, ventana de detalles, desmayo de doce corazones/48 segundos, recuperación en zonas seguras/comida con menús abiertos, respaldo y conservación del perfil v1. Comercio corresponde a un hito posterior y aún no se puede probar. No dar por superada la comprobación de salida del Hito 4 hasta terminar todo esto.

**Prueba Android solo del modelo:** `Builds/Android/TheTrailWalker-Milestone4-ModelTest.apk` usa el paquete independiente `com.thetrailwalker.milestone4.modeltest` y no abre el perfil del Hito 3. En nubia NX769J pasaron las 15 comprobaciones independientes; evidencia en `Builds/Android/Milestone4-ModelTest/` (ignorada por Git). No es una versión jugable del Hito 4 ni prueba transferencias de inventario, migración o pérdidas por desmayo. El paquete del prototipo y el identificador Android del proyecto siguen siendo `com.thetrailwalker.prototype`.
Dependencias: 3.

1. Agregue cinco ranuras de equipo, harapos iniciales, límites de botas y sombrero y durabilidad ajustada a la calidad.
2. Implemente cero bolsillos/ganchos iniciales, límites independientes de diez cada uno, elegibilidad, navegación lateral de inventario/paneles legibles de diez posiciones y detalles contextuales/ventana emergente Eat. Sin límite de peso.
3. Agregue corazones fraccionarios, uso de alimentos, drenaje de distancia de sección por sendero creado por autor, regeneración progresiva de área segura y recuperación del desmayo sin resistencia/sin alimentos con 2-4 pérdidas de entrada llevadas elegibles. Mantenga la regeneración independiente de la interfaz de usuario/bloqueos comerciales y mantenga el manifiesto de pérdida antes de la animación.
4. Implementar la eliminación de equipos de distancia cero, respaldo inmediato sin sombrero/sin botas/camisa de harapos/pantalones de harapos/mochila de harapos, bonificaciones por equipo perdido, ajuste de los límites y transferencia de contenido atómico a un almacenamiento/desbordamiento predeterminado válido. Depurar-caducar cada ranura y todas las ranuras juntas.

**Comprobación de salida:** límites iniciales/globales, ajuste sin peso y límite de escala de pila de 1.5x. El equipo caducado transfiere contenidos sin pérdida accidental. La pérdida por desmayo es deliberada: 2-4 entradas/pilas elegibles reales, equipo equipado protegido, sin penalización por duplicado en la recarga, activador sin comida distinto de cero con parada de comida; recuperarse/despertarse en su lugar y esperar la entrada de velocidad ascendente. La regeneración/comercio siguen siendo independientes.

### Hito 5: Dos segmentos, campamentos, mapa y peaje

Dependencias: 4.

1. Agregue una transición asincrónica entre dos segmentos del sendero y una vista del campamento con cuatro asientos.
2. Persiste los campamentos visitados, el descubrimiento de la región, los ID de las visitas al campamento y el último campamento de ruta.
3. Agregue un mapa regional mínimo y un NPC de ferry con un peaje de prueba único.
4. Bloquea la entrada durante las transiciones y vincula al jugador persistente en el ancla de llegada.

**Comprobación de salida:** los viajes repetidos crean solo un jugador y una sesión, un peaje pagado permanece pagado después del reinicio, no se puede viajar rápidamente a las regiones bloqueadas y la ubicación de regreso es correcta.

**Adiciones a los hitos 4-5:** implementar la sección 4.4.3 antes de aceptar resistencia/desmayo: consumir con inventario abierto, reanudar marcha, conservar excedentes al máximo y guardar/recargar la cola. Construir el mapa con campamentos de la región actual y las otras regiones descubiertas debajo del primero; comprobar llegada al campamento elegido o al primero de otra región, incluso tras cargas interrumpidas. El hito 14 define/filtra los ocho platos nuevos de caja y el pastel premium de manzana separado; conservar seis entradas de caja y las probabilidades propuestas existentes.

**Registro de arte posterior a la finalización — 31 de octubre de 2026:** el hito 5 sigue completado. Las siguientes entradas incluyen bibliotecas nuevas y revisiones de prefabs de personajes existentes; el montaje de las bibliotecas nuevas en las escenas y la validación en dispositivo quedan pendientes según se indica abajo.

- **Revisión de mallas de los NPC comunes del campamento (revisión 3):** se reconstruyeron los avatares masculinos y femeninos con proporciones adultas de cabeza, contornos faciales integrados, orejas menores, detalles de ojos independientes del tono de piel, cabello más continuo, manos y extremidades afinadas, mejores uniones y zapatos, y superficies de ropa más uniformes. Se actualizaron los diez presets existentes conservando seis tonos de piel, cuatro colores de cabello, soporte de ropa/mochilas, poses sentadas, pivotes y vínculos de reposo existentes. Las normales de personajes usan suavizado a 65 grados; no se modificaron las reglas de selección ni descubrimiento de apariencia.
- **Revisión de mallas de animales (revisión 3, conejo):** se reconstruyó el conejo existente con torso/cuello continuos y más esbeltos, hocico corto, ojos laterales pequeños, orejas estrechas, patas delanteras conectadas, patas traseras plegadas y afinadas, pies posteriores más largos y cola menor. Se actualizaron las seis piezas y el prefab existente conservando las referencias del rig y los límites de selección. Las vistas de pie/olfateo y pastoreo con cabeza baja son estudios estáticos; no se implementaron locomoción, animaciones de pastoreo ni especies adicionales. Ambas revisiones pasaron importación/generación de prefabs en Unity e inspección visual; se verificó que 242 fuentes de arte ajenas permanecieran intactas. Tras la próxima compilación, volver a probar en dispositivo la apariencia y el reposo de NPC a distancia de cámara del campamento, además de la silueta, selección y separación de poses del conejo. No se afirma una nueva prueba de APK o rendimiento. Véanse las [notas de revisión de personajes](Docs/Art/Characters/Revision03/character-revision-notes.txt).

- **Mallas generales para las seis regiones de nivel 1:** se añadieron 120 mallas/prefabs reutilizables, 20 por región: Dawn Plains (praderas, arbustos, pinos y elevaciones suaves), Windward Hills (pinos inclinados por el viento, matorrales, salientes y crestas), Mistwood (pinos oscuros, musgo, helechos, raíces y troncos caídos), Fern Hollow (bosque abierto, helechos, hiedra, rocas claras y depresiones), Cloudstep Mountains (pinos alpinos, pedreras, acantilados y siluetas montañosas) y Reedwater Delta (juncos, cárices, madera arrastrada, orillas de barro/arena y agua estática). `Tier01EnvironmentLibrary` registra materiales, límites, huellas, separaciones y sombras para el futuro diseño del sendero. Se conservan las importaciones legibles, las paletas compatibles y la separación de sombras de la vegetación alta. La biblioteca todavía no puebla los diez segmentos de campamento ni sustituye la decoración activa; terreno y agua requieren montaje y unión deliberados. Véanse las [notas de las mallas regionales](Docs/Art/Environment/Tier01/environment-library-notes.txt) y la [vista de las seis regiones](Docs/Art/Environment/Tier01/six-biome-overview.png).
- **Mallas de la dama de misiones y el barquero:** se añadió una dama alta y misteriosa con sombrero amplio, vestido ciruela, abrigo oscuro, detalles marfil y ribetes dorados. Cuatro prefabs de poses estáticas muestran reposo de lado, conversación de frente, receta sostenida y ofrecimiento con el brazo extendido. Los pivotes separados y el anclaje del pergamino preparan la futura animación. El barquero estático lleva gorra, chaleco, cinturón de cuerda, botas, barba y remo de madera con una mano modelada para sujetarlo. `SpecialNpcArtCatalog` reúne actores y accesorios. Quedan pendientes la lógica de misiones, las animaciones de giro/reposo/entrega, las recompensas y la sustitución de los NPC provisionales en las escenas. Se verificaron la generación en Unity y las vistas renderizadas; estas adiciones no cuentan con una nueva prueba de APK/rendimiento en dispositivo. Véanse las [notas de los NPC](Docs/Art/Characters/CampSpecial/mesh-design-notes.txt) y las [poses de la dama](Docs/Art/Characters/CampSpecial/mission-lady-pose-studies.png).

### Hito 6: Tres familias de recolección y botín pendiente
Dependencias: 5.

**Adición de talado:** crear y validar cuatro tablas de ingrediente alimentario obligatorio (7.1.1), junto a madera; guardar todos los tipos en un manifiesto. Rechazar nodos sin ingrediente y comprobar cantidades Normal/Élite.

1. Implemente tablas de acción Normal/Élite separadas por diferencia de rango/familia, rango uno solo Normal, ahorros de acción Élite en/por debajo de su rango, aumento de botín Élite para todos los objetivos elegibles y desgaste de un uso por acción confirmada. Detenerse al inicio de la animación/permanecer detenido hasta la entrada de velocidad ascendente.
2. Autor de un tocón, conejo pasivo en movimiento y punto de pesca; agregar apuntar/sin ataques de animales. Comience con las recetas explícitas de hacha de pino, resortera y caña; pruebe la pareja de trucha arcoíris y mojarra azul de la región uno utilizando vistas de marcador de posición sin esperar a que aparezcan los dieciséis peces.
3. Desarrollar y conservar manifiestos de paquetes; Los recuentos de pila completa siguen la sección 7.
4. Agregue botín visibles agrupadas, pilas pendientes agrupadas por tipo y recuperación de cosecha parcial.
5. Agregue rangos de recursos/herramientas de depuración y comparaciones Normal/Élite; La progresión del objetivo del bloque se clasifica por encima del rango de la herramienta + 2 antes de que comience una interacción.

**Comprobación de salida:** rango coincidente El hacha/caza/caña de élite necesita menos acciones; los objetivos más bajos se reducen por encima del piso de una sola acción; Los objetivos +1/+2 siguen siendo elegibles y reciben botín mejoradas Élite incluso sin un descuento por acción; +3 no puede comenzar. Lo normal nunca obtiene un aumento de botín, cada acción comprometida tiene un uso y las reglas de parada de cosecha, apuntamiento, cola y recarga siguen siendo correctas.

### Hito 7: Conversión de materiales, dominio, temporizadores y fabricación x1000

**Arte fabricado de rango uno:** `Assets/_Game/Art/Crafting/Tier01/Tier01Recipes.json` y `Resources/Tier01/RecipeArtCatalog` contienen 59 diseños exactos, incluida la propuesta opcional de queso de leche mixta, con 63 mallas nuevas de productos/ingredientes de tienda y reutilización del tronco de pino. Véanse `Docs/Art/Tier01/crafted-item-designs.txt` y sus galerías. Es integración de arte/catálogo; fabricación y aprendizaje siguen pendientes. Respetar recetas revisadas de misiones iniciales, herramientas de pino solo Normales, separación de caja/panadería y requisitos recursivos de descubrimiento/conversión. Champiñones asados queda bloqueado hasta asignar su fuente de rango uno. Los tiempos Material sin valor previo son propuestas identificadas; no se añaden recursos sueltos regionales.

**Repetición focalizada pendiente:** H2 iconos, dimensiones/rotación y apilables frente a herramientas/equipo x1; H3 presentación del equipo de rango uno, equipar/quitar y perfiles de durabilidad finita; H4 compatibilidad de IDs/orden anteriores y persistencia de nuevos IDs, calidad/perfil y excedentes. No equivale a validación en dispositivo. H1 no cambia movimiento/energía. H7 implementará transacciones, dominio, temporizadores y aprendizaje; H14 ofertas con fuentes descubiertas, receta única y descuento por pila completa.

**Adición de cocina de árbol:** integrar cuatro recetas Food normales y dos conversiones Material exactas (7.1.1), desbloqueadas sin azar con fuentes accesibles. No cuentan en colecciones de caja/panadería. Probar combustible, lotes, calidad y distinción crudo/preparado.

Dependencias: 6.

1. Agregue panel de recetas de nueve ranuras, selector de recetas, autocompletar, selección de cantidad y vistas previas de salida/nivel/temporizador.
2. Implemente conversiones de materiales no nivelados con ID exacta, resultados recopilables instantáneos y MaterialTier/RecipeUnlockTier por definición. Agrega los puentes de cordón/carbón/relleno usados. Una variedad equivocada o una entrada directa fuera de ventana fracasan sin coste alguno; Los ingredientes de rangos futuros nunca se filtran en las recetas iniciales.
3. Implemente niveles no materiales del 1 al 10 codificados por ID de receta única, orden estadístico/límite Q5 y perfiles de salida inmutables. Agregue el precio base equivalente a Q1, la prima de calidad final y la prima de durabilidad de engranajes finitos; conserve los anclajes Misceláneos de nivel diez básico de 1 efectivo/6,000 efectivo. El queso de vaca/cabra/mezcla nunca comparte dominio ni sustituye ingredientes.
4. Implementar temporizadores de categoría/rango/dominio: comida/herramienta abajo con nivel, usable/varios arriba, horas para usable/varios avanzados, comida/herramienta siempre por debajo de una hora incluso en x1000; Las bonificaciones nunca cambian de tiempo.
5. Agregue reservas, costos atómicos, lotes pendientes, crédito de dominio exacto, restricciones de estado listo y entrega persistente.
6. Restringir herramientas/prendas/mochilas a x1 tanto en la interfaz de usuario como en los comandos de dominio; rechazar x10/x100/x1000 sin gastar materiales. Mantenga declaraciones únicas de un solo artículo y combine elementos apilables listos solo cuando los perfiles de estadísticas de salida coincidan.
7. Agregue las nueve mejoras Élite que consumen instancias y recetas de materiales de rango anterior. Rechazar familia normal incorrecta/rango/Élite como entrada, materiales anteriores faltantes, entrada agotada/propiedad de la misión o cola completa sin costos. Guardar/reanudar no puede devolver el Normal sacrificado ni duplicar el Élite; La salida utiliza su propia instantánea de receta.
8. Ejemplos de entrada fija del autor, trío explícito de herramientas de pino, un asado de carne/pescado por rango y una receta de panadería posterior que utiliza harina de trigo/queso de vaca global más antiguo. Verifique las excepciones de cocción permitidas, el uso rechazado de la despensa no alimentaria, el consumo exacto de madera en x1/x100/x1000, la prohibición de sustitución de especias/combustible y el límite del temporizador de alimentos existente.

**Verificación de salida:** ID/cantidades exactas y retención de dominio de recetas por separado; Las nueve mejoras Élite utilizan material avanzado de rango Normal + progresión anterior una vez, sin herencia de estadísticas de ingredientes ni XP Normal adicional. La política de desgaste parcial es explícita, prevista; sin costos en caso de falla o artículos duplicados al recargar. La conversión de materiales, las reglas de Q5/temporizador, el volumen solo apilable y el manejo de desbordamiento siguen siendo correctos.

### Hito 8: Cuatro ramas de misiones, recursos destacados y seguridad del recorrido

Dependencias: 7.

1. Agregue cuatro definiciones de ramas, las doce misiones de rango de primera progresión y requisitos previos entre ramas. Mantenga una misión activa; no se permiten materiales/herramientas/alimentos/etc. puerta de fase.
2. Cree datos de origen para tres elementos destacados/región y 18/rango, con íconos de mapa e interactuables separados. Valide las 24 ranuras como datos; Utilice inicialmente sólo escenas cortas y representativas.
3. Verificar consumo/reclamaciones de misión automática, diez unidades iniciales y cantidades posteriores propuestas 50/100/hasta 150, cuatro asignaciones de ropa, protección de equipo equipado y herramientas solo x1. Pruebe la competencia entre las contribuciones a la misión de herramientas normales y las reservas de mejora Élite.
4. Juega la nueva introducción de la región uno: ramas -> desbloqueo de registros, manzanas -> desbloqueo de jugo, artesanías de campamento accesibles y suficiente resistencia antes de comer; La región dos introduce fibra de pedernal/cáñamo y secciones de senderos más largas que requieren alimentos.
5. Simule presupuestos de ruta de rango más bajo y la sección de sendero máxima de 26 corazones con doce corazones más comida común. Agregue animaciones de desmayo/recuperación/pérdida/activación y comprobaciones de reinicio/recarga; muestra HUD/mapa/información de salida a partir de los mismos datos.
6. Valide entre 10 y 14 diseños de campamento tempranos y hasta 24 posteriores con transmisión; cree un representante del rango activa antes de escalar al contenido completo de la región. Confirme las plumas de rango tres como fuente compartida aprobada.

**Comprobación de salida:** persisten cuatro secuencias de bifurcación independientes; las primeras doce recompensas coinciden con la tabla y son alcanzables; G1/G2 desbloquea alimentos utilizables en la región uno; Las dependencias inaccesibles muestran ramas bloqueadas en lugar de misiones imposibles activas. Tres íconos de mapa coinciden con engendros sueltos, 18 ID distintos/rango y solo se mantienen las superposiciones permitidas. Las misiones de diez unidades dejan 990; Las misiones de instancia de herramienta/protección de equipo siguen siendo correctas. Ningún tramo de sendero supera los 26 corazones; Las secciones de senderos iniciales funcionan sin comida, las avanzadas funcionan con comidas normales y las pérdidas leves no pueden volver a tirar ni duplicarse al recargar.

### Hito 9: Comercio en campamentos y monederos

Dependencias: 8.

1. Implemente billeteras en efectivo/oro, datos de precio base de categoría/rango/dominio y la fórmula de calidad/durabilidad de la sección 12.2, cotizaciones de pila de 64 bits, tres conjuntos de ofertas de NPC y una sesión de visita exclusiva de 15 segundos. Cree las referencias de precios ahora; El equilibrio económico final sigue siendo una fase posterior.
2. Agregue vistas de cinta transportadora, depósito en garantía de cantidad, posición de precio de compra, compra de NPC, respaldo natural de destrucción/vencimiento del 80 % y cancelación de devolución en el 70 % de los listados de jugadores restantes.
3. Bloquear todas las acciones de los jugadores del juego no comerciales; Mantenga en funcionamiento la regeneración de resistencia del campamento. Persistir ID de pago, tiempo restante, estados Completado/Cancelado y estado de visita realizada; manejar la pausa del sistema operativo sin actualizar la sesión.
4. Agregue informes de transacciones tanto para unidades individuales como para listados completos.

**Comprobación de salida:** solo las acciones de intercambio/devolución funcionan durante los 15 segundos; la resistencia aún se regenera; trituradora natural/caducidad paga el 80% una vez; cancelar paga el 70% solo de los ofertas del jugador no vendidos y luego finaliza la oportunidad de esa visita; Los bienes de los NPC nunca pagan dinero en efectivo al jugador; sin vencimiento/cancelación doble asentamiento; reiniciar/reabrir no puede actualizarse; salir/regresar o llegar a un campamento diferente crea una nueva oportunidad.

**Revisión del primer segmento mecánico:** recorra secciones de senderos de prueba de apertura segura/que requieran alimentos, recopile pilas completas, vea tres íconos destacados versus elementos interactivos, use tres familias de herramientas, organice/equipe, coma, cree lotes compatibles, avance en cuatro ramas, intercambie, debilite/recupere intencionalmente y recargue. Corrija la corrección antes de la expansión del asentamiento.

### Hito 10: Estructura del asentamiento y primera casa

Dependencias: revisión exitosa del segmento mecánico.

1. Agregue selección de edificios de asentamiento de desplazamiento lateral y acceso de depuración para pruebas.
2. Agregue vista frontal de la casa, cuadrícula interior de nivel uno, botones de tienda/actualización y seis tipos de muebles comprados en tienda. No requiere recetas de muebles ni costos de compra de materiales; Las mejoras de la primera casa utilizan pino/ladrillos/tela de rango uno.
3. Implementar registros de bonificación de primera compra y depósitos de la casa. Verificar nivel base + calidad + durabilidad de muebles, incluyendo calidad sin muebles. El precio utiliza una base equivalente a Q1 -> prima de calidad final -> prima de durabilidad aplicable -> modificadores de venta posterior; sin capas C/U duplicadas ni cambios en el temporizador de artesanía.
4. Agregar mapa mundial y navegación contextual Inicio/Almacén/Madera/Retorno.

**Comprobación de salida:** el desbloqueo normal permanece después de la región cuatro, el acceso de depuración está claramente separado, el regreso va al campamento registrado, las bonificaciones no se duplican mediante la recompra y las actualizaciones persisten con los materiales depositados.

### Hito 11: Colocación de muebles y límites del almacenamiento doméstico

Dependencias: 10.

1. Implemente un contenedor de cofre con desplazamiento horizontal y valide su vista en el teléfono de destino.
2. Agregue estantes con tres filas/cuatro entradas y división en una unidad.
3. Agregue transacciones de colisión/ubicación de muebles y transferencia de almacenamiento a inventario.
4. Nivel ocho de depuración y desbloqueo y capacidad máxima del cofre de almacenamiento sin cambiar las reglas de progresión normales.

**Comprobación de salida:** todo el contenido permanece dentro de límites seguros en el tamaño de pantalla más pequeño admitido; los estantes rechazan pilas sin perder unidades; un gran almacenamiento oculto no crea enormes árboles de UI activos; las transferencias tienen exactamente un propietario en todas las recargas.

### Hito 12: Almacén comercial y conversión a oro

Dependencias: 11 y billeteras a partir de 9.

1. Agregue dos cajas de diez unidades, bloqueo de ID exacto para cualquier categoría vendible, depósitos parciales, cajas de reemplazo y pago en efectivo.
2. Agregue contabilidad de diez cajas, liquidación de oro a 1 por cada 1.000 de efectivo y persistencia del resto entero en efectivo.
3. Agregue un depósito de cajas múltiples limitado opcional después de que se demuestre la corrección de una sola caja.
4. Registre entradas, salidas y totales para análisis económicos posteriores.

**Comprobación de salida:** diez unidades de una pila completa dejan 990; sin pago de caja parcial; cada décima casilla completada paga una vez el oro esperado; la recarga durante el pago no puede repetir recompensas; La multiplicación de cantidad/valor no puede desbordarse.

### Hito 13: Campo maderero y trabajos de larga duración

**Adición de alimento masivo:** cada rango activo usa ingrediente obligatorio de 7.1.1. Multiplicar sus pilas por 50 una vez junto a ramas/troncos; persistir reclamaciones parciales y caducar con el ciclo del mismo árbol.

Dependencias: 12.

1. Construya un rango con seis árboles independientes en dos filas y desbloqueo del rango dorado.
2. Implemente las curvas de acción Normal/Élite del árbol masivo de la sección 8.2, techo +2, Élite más rápido en o por debajo de su propio rango, bonificación de botín Élite incluso en +1/+2 elegible, manifiestos x50, pilas de tipos y reapariciones de 72 horas.
3. Agregue advertencias simuladas de tiempo/caducidad, luego expanda solo a cuatro rangos de madera activos (24 registros de árboles). Deje los otros cuatro rangos en el trabajo pendiente de diseño.

**Comprobación de salida:** los seis temporizadores son independientes, las recompensas x50 permanecen en pilas completas, al salir o volver a entrar se conservan las cantidades pendientes, el avance del tiempo reaparece solo los árboles elegibles y caduca solo sus recompensas anteriores, y el procesamiento permanece limitado.

### Hito 14: Tiendas del asentamiento y comerciante del globo

Dependencias: 13.

1. Implementar la política de servicio de tienda de tres ofertas/Q5. Configure la pesca como exactamente dos peces descubiertos distintos + un marisco exclusivo elegible al azar, todo en efectivo; una pila/1,000 unidades por ranura/reinicio. Agregue el catálogo propuesto de camarones/cangrejos/calamares/langosta sin agregar peces de rastro/entradas sueltas.
2. Persistir roles de stock/espacio, filtros de región, cotizaciones del Q5, compras/desbordamiento. Reabrir/recargar nunca actualiza el stock; el reinicio válido del asentamiento lo hace. La compra de ingredientes nunca otorga bonificaciones a las estadísticas de receta/dominio o de artesanía del jugador.
3. Agregue equipo/mochila de globo y seis entradas de recompensa **tipadas**; integre 21 recetas Food/5 Misc propuestas. Renovar ofertas no compradas al cerrar/reabrir, incluida ropa especial; conservar manifiestos pagados y desbloqueos principales.
4. Agregue una categoría de receta configurable (5% propuesto de cada espacio aleatorio), elegibilidad de fuente de ingrediente/descubierto, exclusión de reserva no aprendida/pendiente, respaldo duplicado persistente y reclamo de una sola vez. Las entradas físicas permanecen Q5; El nuevo conocimiento de recetas comienza a dominar las fabricaciones uno/cero y no ocupa espacio en la bolsa. Las materias primas exclusivas siguen siendo exclusivas de la pesca.

5. Crear las siete recetas premium de sección 9.4.4, filtros de fuentes y precios propuestos. Comprobar distribución 1/2/2/2 y ejecutar los casos de fuentes faltantes de 9.4.4 al generar ofertas y antes de cobrar/desbloquear, también tras recarga/migración. Implementar sustitución del 1% por espacio elegible, stock de una receta, duplicados y pago/desbloqueo atómico. Persistir tipo/cantidad/precio hasta salir/regresar. Aplicar una vez el descuento del 10% a pilas premium completas.

**Salida de panadería:** comprobar siete IDs nuevos, 33 coleccionables totales y 26 en globos; elegibilidad, aprendidas, duplicados, agotamiento, mochila llena, compra interrumpida, recarga y salida/regreso. Verificar todas las filas de ingredientes/rendimiento/calidad/dominio y precios; 5 oro/porción implica receta 50 y pila 4.500.

**Comprobación de salida:** la pescadería siempre tiene dos peces/uno exclusivo a Q5/efectivo y ningún presupuesto reiniciado puede exceder los 1,000; Se mantienen las restricciones sobre las fuentes de mariscos. Las cajas mantienen seis entradas, las recetas elegibles no aprendidas se otorgan una vez incluso con bolsas llenas, los duplicados/grupos agotados retroceden de manera determinista y la recarga nunca se repite. Todas las recompensas físicas permanecen en Q5; el conocimiento no crea una subvención de producto/pila/calidad/dominio-diez. El equipo ordinario sigue siendo finito, la calidad del transportador no cambia y ninguna progresión obligatoria depende de la suerte.

### Hito 15: Mascotas, reconciliación sin conexión y muebles restantes

Dependencias: 14.

1. Agregue compra de mascotas desde casa tres, selección en casita frontal izquierda y contenedor separado; implemente primero Perro/Cerdo y después las seis tablas/vistas previas.
2. Implemente recompensas limitadas fuera de línea, cambio de mascotas y conciliación exacta una vez.
3. Complete los niveles de muebles restantes utilizando la tabla de bonificación distribuida uniformemente.
4. Agregue variantes decorativas de casas vecinas sin simulación.

**Comprobación de salida:** no se permite el acceso de mascotas antes del nivel tres en el juego normal, un intervalo fuera de línea se paga una vez, el cambio de mascota no puede duplicar el tiempo, el movimiento hacia atrás del reloj no produce ningún progreso negativo, las bonificaciones de la primera compra permanecen limitadas.

### Hito 16: Revisión completa de mecánicas y límites

Dependencias: 15; mantener recogidas de pilas completas y herramientas de depuración habilitadas.

1. Usar MechanicsTest con recogidas de 1,000 unidades, concesiones de depuración, selectores de viaje/rango, reloj simulado y semillas reproducibles.
2. Ejecutar la matriz de límites: materiales instantáneos x1000, dominio 1/5/6/10, temporizadores, Wearable/Misc de horas, rechazo de equipo por lotes, diferencias Normal/Élite, nueve mejoras, misiones diez/50/100/150, propiedad de herramientas para misiones/mejoras, límites +2/+3, comercio/regeneración y manifiestos masivos. Conservar los valores de prueba existentes.
3. Capturar rendimiento/memoria sostenidos en Android y repetir transiciones, guardado/carga y suspensión en compilaciones de desarrollo.
4. Corregir defectos de propiedad, legibilidad, desbordamiento, entrada y validación. Registrar observaciones de economía/ritmo sin cambiar cantidades, costes, tasas o curvas para equilibrar.
5. Clasificar cada regla probada como verificada, defectuosa o pendiente de definición/implementación. Distinguir defectos de reglas definidas, atajos de depuración y contenido futuro indefinido.

**Comprobación de salida:** los sistemas definidos superan las pruebas de mecánicas/límites sin duplicación ni pérdida involuntaria; registrar dispositivo/compilación y defectos reproducibles. Superar este hito continúa las pruebas; no inicia equilibrio de progresión ni retirada de depuración.

### Hito 17: Contenido regional base y pruebas de progresión con depuración

Dependencias: auditoría de mecánicas/límites aceptada para el alcance implementado; conservar herramientas de depuración y recogidas de 1,000 unidades.

**Plan de contenido de los rangos 2-4 (propuesta, petición del usuario, 5 de noviembre de 2026):** las recetas de la sección 9.6 (dos conjuntos de equipo por rango, Beeswax, 21 Food y 10 Misc nuevas, platos de pescado variados) y las misiones de la sección 11.6 (8 misiones puente del rango uno más 68 / 69 / 67; toda receta de los rangos 2-4 es recompensa de misión) son el contenido que este hito define al construir las regiones de cada rango. Pasos de implementación:
- valores de capacidad explícitos en las prendas (el Conjunto A queda entre rangos)
- datos de misiones de los rangos 2-4 y las misiones puente del rango uno
- el producto de caza Boar tusk
- las comprobaciones de la sección 11.6.5
- arte ajustado de cada prenda con el sistema de prendas del jugador

Las misiones puente del rango uno se pueden añadir antes, con el resto del rango uno, porque solo usan objetos del rango uno.

1. Validar las tablas de tres recursos de las 24 regiones iniciales (72 ID) y cuatro rangos definidos con tramos representativos; crear plantillas de prueba de 10-24 campamentos con elementos reutilizados/carga por tramos. Son la base, no el límite final del contenido. Registrar materiales/regiones/recetas superiores pendientes de definición e implementación; no inventar reglas ni declarar completo el producto.
2. Incorporar las recetas de ID fijos de rangos activos definidas y necesarias para pruebas. Conservar ingredientes exactos, cantidades/temporizadores existentes, variantes de leche y entradas exactas de vasos/botellas. Muebles solo de tienda; mejoras de casa con materiales nombrados. Registrar definiciones faltantes sin inventar reajustes para cerrar huecos.
3. Probar dependencias de recetas/misiones/fuentes, pares/hábitats de pesca, tramos <=26 corazones, comida/desmayo, cálculo de precios, desgaste y existencias con MechanicsTest. Registrar consumo de herramientas Normal, refinamiento previo, reservas de casa y trabajo x1. Son pruebas de corrección/cobertura, no evaluación final de economía o ritmo.
4. Mantener referencias de precio, curvas, costes, botín, temporizadores, cuotas y valores de dominio/ritmo. Anotar posibles problemas de equilibrio y dependencias superiores incompletas para trabajo futuro solicitado por separado. Registrar cada acceso de depuración como un salto de prueba, no como prueba de una ruta natural alcanzable.
5. Empezar con un guardado de prueba nuevo y concesiones/selectores disponibles. Usar concesiones reproducibles o desbloqueos dirigidos para probar cada región, receta y requisito de casa definidos; registrar prerrequisitos, transiciones permitidas/rechazadas y saltos utilizados. Probar por separado el cumplimiento de requisitos y el acceso mediante depuración. Marcar rutas indefinidas/no implementadas como pendientes. No exigir una partida sin concesiones ni retirar/desactivar herramientas.

**Comprobación de salida:** cada requisito implementado del alcance base tiene pruebas reproducibles y resultados; identificar dependencias circulares y fuentes/usos faltantes del contenido definido como defectos, dejando explícitamente pendientes los rangos superiores indefinidos. La cobertura con depuración no certifica progresión natural, equilibrio, contenido final completo ni preparación para lanzamiento.

### Hito 18: Presentación del prototipo, accesibilidad y pruebas en dispositivos

Dependencias: cobertura documentada del contenido base y limitaciones del Hito 17, no una partida completa sin concesiones.

1. Mejorar legibilidad del prototipo con elementos provisionales coherentes, iconos, temporizadores, objetivos de recursos, audio y efectos que ayuden a probar. El arte final no es un requisito de salida.
2. Probar movimiento reducido, escala de texto/interfaz, toque alternativo para recoger y detalles de comercio legibles. Conservar los 15 segundos exclusivos y la regla de cancelación Return.
3. Implementar/probar ajustes de audio, gráficos, controles e idioma con claves estables de localización.
4. Probar proporciones de pantalla, interrupciones, poco almacenamiento, migración de guardados, compatibilidad gráfica y rendimiento sostenido en compilaciones de desarrollo locales con depuración disponible.
5. Preparar APK de desarrollo locales, escenarios, capturas e informes de problemas para repetir pruebas. No iniciar preparación de SDK/firma de tienda, paquetes de lanzamiento, despliegue, publicación, retirada de concesiones ni reequilibrio sin un mensaje futuro que solicite explícitamente la fase correspondiente.

**Comprobación de salida:** compilación de desarrollo utilizable, guardados de prueba recuperables, cobertura de dispositivos documentada y lista actualizada de defectos/definiciones pendientes. Continuar iteraciones de depuración/pruebas; no pasar automáticamente a equilibrio, producto final o despliegue.

### Próxima tarea de implementación inmediata

Comience solo con los hitos 0 y 1: compilación para Android, luego movimiento de ruta solo hacia adelante, gestos de velocidad del avatar, órbita de cámara independiente de 360 grados y las distintas reglas de parada/reanudación. A continuación, implemente las recolecciones espaciales completas del hito 2, incluidos los recursos accesibles detrás del avatar. No empiece por crear el catálogo de recetas completo, todas las regiones o la interfaz de usuario completa del asentamiento.

## 19. Matriz de verificación de mecánicas y límites

Utilice pruebas EditMode enfocadas para reglas deterministas y pruebas PlayMode/dispositivo para interacción y ciclo de vida. Evite pruebas que simplemente repitan una tarea de campo trivial. Los escenarios de prueba de alto valor son:

| Área | Casos requeridos |
| --- | --- |
| Cantidad | 0 rechazado; 1/999/1000 válido; 1001 divide o rechaza por operación; comprobado aritmética de lotes grandes |
| Crecimiento de la pila | Los límites se ajustan, la fusión se amplía más allá de los vecinos, calidad mixta, mismo Q5 con diferentes valores intrínsecos, estado listo/pendiente, pila completa tocando pila completa |
| Movimiento/cámara | Órbita completa de 360 grados mientras camina/detiene, propiedad del eje horizontal/vertical, no hay parada/dirección inducida por la cámara, la orientación persiste, la órbita está deshabilitada en la interfaz de usuario exclusiva/de bloqueo, el área de impacto del avatar funciona desde todos los ángulos de orientación |
| Detener/reanudar | El inicio de la cosecha restablece la velocidad; éxito/cancelación/fallo/rotura de herramienta permanecen detenidos; sólo se reanuda un nuevo gesto hacia arriba; el viaje de ida y vuelta del inventario restablece la velocidad anterior si se permite; el inventario después de la cosecha permanece en cero; el límite de velocidad al caminar actual proporcionado por las botas anula la instantánea obsoleta |
| Colección | El botín de los jugadores cercanos detrás del juego permanecen raycastables/en el alcance después de la órbita; sin eliminación lógica detrás de cámara, sin alcance distante/ocluido; dos toques en un paquete, ubicación fallida, pausa de la aplicación durante la animación, resto de misión completa |
| Contenedores | Solo tamaño/ajuste y sin rechazo de peso; cero ganchos/bolsillos de arranque; diez máximo cada uno; transferencia de diez a cero; Escala lineal apilada de 1.5x/tapa de área rectangular de 2.25x; límites de mochila, pantallas pequeñas, estante dividido en uno, desbordamiento |
| Equipos | La instancia equipada con distancia cero desaparece; valor predeterminado exacto por ranura, bonificaciones de equipo eliminadas, sin autoequipamiento de repuesto, límites de tres corazones/sin botas, límite de resistencia actual sin recarga, transferencia de contenido de trapo/bolsillo/gancho, vencimiento simultáneo, sin pérdida de contenido/capacidad retenida, recuperación de guardado, equipo infinito exento |
| Resistencia/animales pasivos | Drenaje exclusivo para caminar; apuntar/cosechar estacionario sin drenaje; recuperación progresiva de comida/bebida con excedente conservado; regeneración progresiva del campamento/asentamiento durante los menús/comercio; límite máximo; movimiento pasivo/apuntar aciertos y fallos; sin daños al jugador |
| Espaciado/comida | Primeros campamentos de apertura accesibles con tres corazones/sin comida; desbloqueo de jugo de manzana de la región uno; región más larga: dos secciones de senderos requieren alimentos; tarifa de autor fija independiente del equipo; cada sección del sendero <=26 corazones; inicio de doce corazones + tres porciones ordinarias de seis corazones cubren de forma segura el ejemplo de 26 corazones sin desperdicio |
| Recuperación del desmayo | Zero con comida preparada se detiene sin desmayarse; cero/ninguna comida disponible desmayos en el lugar; 2 a 4 entradas distintas al azar, pérdida total de la pila, ganchos y repuestos elegibles/equipo equipado excluidos; menos de dos casos/casos vacíos; el manifiesto de penalización se comete una vez antes de la animación; recuperar/despertar sin caminar automáticamente ni recargar nuevamente |
| Cosecha | Progresión normal, rango uno; Ahorros en acciones de élite en rangos igualados/inferiores con piso de una acción; un desgaste/uso por acción comprometida; todos los rangos objetivo elegibles obtienen un aumento de botín Élite que incluye +1/+2; Lo normal nunca aumenta; +3 sin inicio; curvas masivas revisadas/x50 |
| Elaboración de materiales | 3 ramas -> 1 registro, 3 registros -> 1 tabla, 1,000 entradas dejan el resto correcto, sin dominio, recetas de salida recopiladas instantáneas en x1000, duración fija solo elaborada, la calidad de la entrada no controla la salida |
| Dominio artesanal/estadísticas | Niveles 1/5/6/10, recuentos de unidades terminadas a granel, sin crédito al inicio/reclamo repetido, límite del Q5, crecimiento continuo de la durabilidad base basado en niveles, bonificación de calidad sin muebles, base aditiva + calidad + desglose de muebles coincidentes, sin multiplicador de durabilidad C directo, sin desbordamiento del Q5, estadísticas de producción antiguas sin cambios |
| Precios | Progresión de un efectivo, rango uno Q1 básico; referencia de nivel diez Base miscelánea exactamente 6,000 antes de Q/durabilidad; orden de categorías en cuatro tablas de referencia de rangos comparables; el factor de dominio continúa más allá del nivel cinco; Q5 Varios 12,000; mochila 11,200 con bonificaciones indicadas; los materiales nunca se nivelan; los artículos no duraderos nunca obtienen una prima de durabilidad; C/U/Q no se aplica doblemente; los totales de pila completa/caja utilizan punto fijo y aritmética de 64 bits |
| Temporizadores artesanales | Disminución de alimentos/herramientas por nivel, aumento de prendas/varios, progresión de segundos a horas, alimentos/herramientas por debajo de 3,600 segundos para todos los tamaños de lote, los materiales instantáneos permanecen en cero, los muebles nunca cambian de tiempo |
| Transacciones de elaboración | Apilable x1/x10/x100/x1000; Herramienta/Usable/Mochila x1 solamente, solicitud masiva falsificada rechazada sin costos; escasez/desbordamiento de ingredientes, instantánea del mismo lote, instancia única, recarga en tiempo listo, límite de cola |
| Actualizaciones de élite | Nueve recetas avanzadas exactas de instancia normal + rango anterior; Rango de progresión/familia/entrada Élite incorrecta rechazada; propuesta de política normal parcialmente utilizada; sin complemento avanzado de rango actual, sin herencia directa de estadísticas de entrada, sin XP normal adicional, sin gasto doble de misión/actualización, la entrada desaparece al inicio/salida una vez que está lista, recuperación de cola completa/recarga |
| Ingredientes exactos de la receta | Receta de vaca rechaza la leche de cabra; la receta mixta rechaza dos unidades de vaca o dos de cabra; las variantes de madera/fibra/metal/pescado nunca se sustituyen; Autocompletar/manual/masivo utiliza coincidencias de ID idénticas; las pilas del mismo ID se pueden combinar según reglas de calidad; el desbloqueo/dominio de recetas permanece separado |
| Rangos de progresión de recetas/cocción | Desbloquear el rango uno solo acepta 1; dos aceptan 1/2; tres aceptan 2/3; cuatro acepta 3/4. Aprobado exactamente harina/queso/etc. trabajar sólo en Alimentos con acceso a la fuente; sin sustitución/fuga futura o excepción no alimentaria. Los asados cobran madera exacta x cantidad, después la madera no puede ser de Pino por conveniencia; dominio/ubicación/Q5 la calidad de la tienda nunca cambia MaterialTier |
| Herramientas de pino/pesca | Tres recetas normales de progresión explícita de rango uno, x1/no Élite; cuatro ID de peces por rango, seis pares regionales únicos, dos especies de perfil, nodos específicos de hábitat, dieciséis definiciones distintas de pescado asado, filtros de tienda de especies descubiertas; no hay un cuarto icono suelto |
| Misiones | Cuatro ramas/una activa, recompensas/espacios/requisitos previos iniciales; cantidades diez/50/100/hasta 150 por rango de recursos; 150 deja 850 de 1,000; la herramienta única cuenta, no utiliza/apila; los aportes de actualización tampoco pueden contribuir; protección equipada; refinamiento previo y desbloqueo Normal antes de la solicitud Élite; sin recompensas duplicadas |
| Alcance del contenido | 24 regiones/cuatro rangos de seis regiones; exactamente tres ID/región flexibles y 18 rangos únicos/de progresión; coincidencias base rama+piel/pluma de pavo más ingredientes de árbol explícitos (7.1.1); no troncos/carne/pescado sueltos; tablas de mapas/generación idénticas; campamentos tempranos de 10 a 14 años y posteriores hasta 24; siete herramientas/familia, 24 árboles madereros, sin dependencia de especies pendientes |
| Recetas simplificadas/tienda | Cada queso usa su ID de leche fija + sal con temporizador/dominio de alimentos; sin cultivo/tanino obligatorio; tazas/botellas son insumos materiales exactos; mejora de la casa con ladrillos/paneles; los muebles siguen siendo sólo tiendas |
| Comercio | Exclusivo 15 segundos; todas las demás acciones bloqueadas; la regeneración continúa; La devolución liquida los ofertas del jugador no vendidos al 70% una vez; triturador natural/caducidad 80%; NPC/bienes previamente liquidados excluidos; cancelar/vencer carrera; la sesión gastada sobrevive al reinicio; actualizaciones reales de salida/regreso |
| Almacén | Todas las categorías vendibles, dos cajas parciales de diez unidades por ID, valores por perfil/condición, 100% efectivo/sin extras, 10.000 de efectivo -> 10 de oro en la décima caja, resto entero, recarga una vez, depósito de instancias únicas |
| Tiendas/cajas | Pesca con dos peces distintos + uno exclusivo, Q5/efectivo, <=1,000 stock/espacio/reinicio; filtrado de fuente y sin repetición/recarga al volver a abrir/recargar. Seis espacios de caja mecanografiados, concesión de receta física Q5 versus dominio de una receta, duplicados aprendidos/reservados/dentro de la caja excluidos, sin respaldo de receta elegible, respaldo de receta conocida antes de la reclamación, aprendizaje completo, afirmaciones exactas una vez, sin puerta trasera de producto terminado exclusivo/desconocido, progresión principal nunca aleatoria |
| Viajes | Peaje único, destino en región prohibida, carga de escena repetida, transición interrumpida, campamento de regreso correcto |
| asentamiento | Ocho tamaños de cuadrícula, seis tipos de muebles/nivel, bonificación de compra duplicada, un contenedor horizontal de hasta 48 x 6, 12 entradas de estantes |
| Temporizadores | Finalización de fabricaciones, árboles independientes de 72 horas, límite de mascotas, avance/retroceso del reloj del dispositivo, reanudaciones repetidas |
| Guardar recuperación | Falta definición, esquema antiguo, primario corrupto, esquema más nuevo no compatible, interrupción en cada transacción crítica |
| Rendimiento | 24 paquetes en vivo más cola enorme de pendientes, bolsa llena/almacenamiento horizontal grande, fabricaciones x1000, todos los registros de los árboles, 20 viajes de ida y vuelta a escenas |

Comprobaciones adicionales de aceptación:

- **Comida:** consumir seis corazones con 3/3; verificar seis pendientes sin aumento instantáneo, sin pérdida al estar detenido al máximo y entrega progresiva al caminar tras cerrar inventario. Probar gasto igual/mayor/menor que recuperación, agotamiento de cola dentro de un fotograma, distintos pasos temporales, varias porciones sin multiplicar velocidad, prioridad de zona segura, pastel premium de doce con límite de tres, rotura de sombrero, cero con comida pendiente, suspensión/viaje/recarga y consumo/inserción una sola vez. Simular el tramo de 26 corazones con la velocidad propuesta y excedentes conservados, incluso con inicio parcial.
- **Recetas:** 33 ID opcionales coleccionables = 28 Food + 5 Misc (26 globo + 7 bakery); los ocho platos nuevos aportan 6/6/7/7/8/8/9/10 corazones totales por porción. Región 1 sin región 5 rechaza platos de avena; descubrir fuentes y acceder a leche exacta los habilita. Comprobar regiones posteriores, cantidades/rendimientos, rango/despensa, conversiones conocidas, prevención de duplicados y exclusión de las siete recetas de panadería del catálogo del globo.
- **Mapa:** verificar encabezado/marcador de región actual, tramos/campamentos bloqueados y lista de regiones descubiertas debajo del primer campamento. Viajar dentro de la región usa el campamento desbloqueado exacto; cambiar de región siempre usa el primero, incluida una región descubierta de número mayor. Verificar selección actual sin efecto, rechazo de peaje pendiente, mapa renovado al llegar, Return to Trail correcto, comida pendiente conservada y viaje interrumpido sin doble llegada/renovación de visita.

Herramientas útiles para desarrolladores: navegador de recetas/artículos con capacidad de búsqueda; llenar/vaciar el contenedor seleccionado; conceder monedas; equipar rango; selector de misión; vista previa del botín generado a partir de una semilla; reloj UTC simulado; teletransportarse al campamento; mostrar el libro mayor de propiedad/transacciones; Informe de economía exportadora. Mantener los comandos de depuración separados de la interfaz normal y disponibles en las compilaciones actuales de desarrollo/prueba. Retirarlos o desactivarlos queda aplazado hasta una solicitud explícita; completar un hito no lo activa automáticamente.

## 20. Respuestas confirmadas y valores de implementación delegados

El desarrollador respondió las preguntas anteriores; estas decisiones sustituyen propuestas contradictorias. Las preguntas 5 y 9 delegan valores de implementación, sin nuevas aprobaciones.

1. **Presentación:** mantener abiertos fabricación/inventario. Al fabricar, centrar objeto/lote en cuadrícula 3 x 3 con calidad, durabilidad aplicable y temporizador un momento, luego dejarlo caer a mochila. Presentación propuesta: un segundo. Productos pendientes ocupan espacio y no se usan hasta terminar; el temporizador no retrasa la entrega. Orientación vertical sigue propuesta; comprobar legibilidad en dispositivo.
2. **Colocación:** libre solamente, orientación fija, sin rotación de objetos.
3. **Durabilidad:** Normal de rango uno/nivel uno: hacha 10, resortera 6, caña 10 usos. Sumar diez por rango dentro de cada familia. Élite duplica exactamente la base Normal del mismo rango; solo hay Élite en 2-4. Después aplicar dominio/calidad/muebles elegibles. Acciones de recolección siguen como propuesta de sección 8.2.
4. **NPC:** descuentos hacia trituradora confirmados; usar curva propuesta 110%-80%. Equipo generado por NPC tiene nivel de creación uno, Q1-Q5 aleatoria y cero bonificaciones de muebles. Ofertas del jugador conservan perfil; tiendas normales siguen Q5. Las bonificaciones sobre artículos de muebles corresponden solo a fabricación del jugador.
5. **Pendientes/excedentes — valor delegado:** productos pendientes en mochila con indicador de tiempo; ocho registros abiertos. Reservar espacio sin reorganizar objetos; excedentes reales van visiblemente a bandeja persistente. Un propietario y una acreditación de finalización/dominio incluso al recargar.
6. **Misiones:** solo ID exacto y cantidad, sin umbral de calidad/durabilidad/dominio. Mantener disponibilidad, producto listo y protección de equipo puesto; herramientas parcialmente usadas del ID requerido cuentan.
7. **Cofres:** un contenedor de colocación libre con desplazamiento horizontal como mochila, sin páginas. Dimensiones propuestas: ancho 6/12/24/48, altura seis; virtualizar vistas ocultas.
8. **Almacén:** cualquier artículo listo vendible, incluido equipo único. Dos cajas, diez unidades/instancias del mismo ID. Pagar cotización completa en efectivo sin extras/muebles y 1 de oro por cada 1.000 de efectivo de las últimas diez cajas (revisado el 31 de octubre de 2026; antes 3%): 10.000 de efectivo -> 10 de oro. Guardar el resto entero en efectivo. Mezclar perfiles por ID y valorar equipo usado por condición son valores delegados, no filtros de misión.
9. **Viajes/renovación — actualización confirmada:** mapas desbloqueados siguen gratuitos con reglas de destino/peaje. Comercio de campamento solo se renueva tras salida/regreso real u otro campamento; seleccionar el actual no renueva. Tiendas de ingredientes/materiales se renuevan al salir realmente del asentamiento y volver, sin temporizador. Globo vuelve a sortear al cerrar/reabrir, incluida prenda especial mostrada, en misma visita. Reabrir/recargar tienda normal conserva visita; recargar globo conserva sesión abierta. Compras/manifiestos pagados nunca se sortean de nuevo. Sustituye propuesta de 30 minutos; no añadir espera para comprar ingredientes/materiales.

**Diseño de lanzamiento aplazado y prioridad de alcance:** la recogida unitaria previamente definida permanece solo como intención futura. Los hitos actuales conservan recogidas de 1,000 y herramientas de depuración; no exigen partida nueva sin concesiones/en modo lanzamiento ni reequilibrio. Las 24 regiones/cuatro rangos iniciales son la base de pruebas, no el producto final; materiales/contenido superiores siguen pendientes de definición e implementación. Conservar mecánicas definidas: doce corazones, tramos <=26, ingredientes exactos, seis recompensas por caja y equipo x1. Reequilibrar, retirar/desactivar concesiones, activar recogidas de lanzamiento, preparar versión final o desplegar/publicar requieren cada uno una instrucción explícita futura del usuario.

## 21. Fuentes y limitaciones de las referencias

- Entradas principales del proyecto: [`Trail.md`](Trail.md), `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/QualitySettings.asset`, `Packages/manifest.json` y los recursos del renderizador móvil.
- [Referencia del elemento de la comunidad The Trail](https://the-trail.fandom.com/wiki/Items): el acceso directo falló durante esta revisión; extractos indexados estaban disponibles. Respaldan comparaciones amplias, como recursos crudos, equipos de alimentación, bienes de venta y materiales para el hogar. Este documento no pretende realizar una auditoría completa de la wiki ni reproducir su catálogo de recetas. Las regiones, cantidades, recetas y tablas de equilibrio propuestas arriba son sugerencias de diseño originales derivadas de los requisitos de este proyecto.
- [documentación de Unity Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.8/manual/index.html): opción de creación de ruta para el motor de solo avance. Seleccione una versión de paquete compatible con el editor a través del Administrador de paquetes en lugar de asumir un parche en particular.
- [Guía de rendimiento URP de Unity 6.4](https://docs.unity3d.com/6000.4/Documentation/Manual/urp/configure-for-better-performance.html): base para deshabilitar HDR/profundidad/texturas opacas innecesarias y el costo de generación de perfiles. El rendimiento real sigue sin medirse.
- [Configuración del entorno Unity Android](https://docs.unity3d.com/6000.4/Documentation/Manual/android-sdksetup.html): referencia de la cadena de herramientas para el hito 0.
- [Controles en pantalla del sistema de entrada](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/OnScreen.html): controles opcionales para accesibilidad; El movimiento del joystick estándar no forma parte de este diseño.
- [Cinemachine: seguimiento en tercera persona](https://docs.unity.cn/Packages/com.unity.cinemachine%403.1/manual/CinemachineThirdPersonFollow.html): implementación opcional de seguimiento de cámara.
- [NC DEQ Corvina roja](https://www.deq.nc.gov/about/divisions/marine-fisheries/public-information-and-education/species-profiles/red-drum) y [NOAA Huachinango rojo](https://www.fisheries.noaa.gov/species/red-snapper): guía de hábitat para presentación separada de nodos de pesca costera. Los rangos/emparejamientos de progresión de dieciséis peces son propuestas de diseño de juegos, no clasificaciones biológicas del mundo real.

Este documento es un plan de diseño e implementación. Las verificaciones de finalización describen el trabajo futuro, no las pruebas ya superadas. En esta tarea solo se cambió la documentación.
