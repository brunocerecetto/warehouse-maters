# Warehouse Master — Plan de prototipo, gameplay, stack y monetización

**Documento de trabajo**  
**Concepto:** juego mobile iPhone free-to-play, hybrid-casual idle/tycoon, monetizado con publicidad e in-app purchases.

---

## 1. Resumen ejecutivo

**Warehouse Master** es un juego mobile donde el jugador administra un depósito: recibe cajas, las ordena, prepara pedidos, entrega paquetes, gana dinero y expande el warehouse con mejoras, empleados automáticos, máquinas y nuevas zonas.

La recomendación es construirlo como un **hybrid-casual idle/tycoon**, no como un hypercasual puro. El juego tiene que ser entendible en 3 segundos, pero con suficiente progresión para que la gente vuelva.

La idea base:

> Administrás un depósito: recibís cajas, las ordenás, preparás pedidos, entregás paquetes, ganás dinero y expandís tu warehouse con máquinas, empleados y zonas nuevas.

El objetivo del prototipo no es tener un juego enorme. Es validar si el loop básico es divertido durante 5-10 minutos.

---

## 2. Tipo de juego recomendado

### Género

**Hybrid-casual idle tycoon con gameplay “satisfying”.**

No conviene arrancar con un puzzle complejo ni con un simulador realista. El juego debería sentirse más cerca de un mini tycoon mobile: simple, visual, rápido, con upgrades constantes y automatización progresiva.

### Referencia de sensación

El juego debería sentirse como:

- Mini tycoon casual.
- Idle supermarket.
- Warehouse/logistics manager simplificado.
- Juego satisfying de ordenar, cargar, entregar y mejorar.
- Progresión visible estilo “empecé con nada y ahora todo funciona solo”.

---

## 3. Decisión de gameplay para el prototipo

### Cámara

**3D isométrico/top-down.**

Ventajas:

- Es fácil de entender.
- Funciona bien en iPhone.
- Permite mostrar muchas cajas, máquinas, empleados y upgrades.
- Es más marketinero que una UI 2D pura.
- Permite videos atractivos para TikTok/Reels/Meta Ads.
- Se puede prototipar rápido con assets low-poly.

### Control

**Joystick virtual de un dedo.**

El jugador no debería tener que apretar muchos botones. El personaje se mueve y las acciones ocurren automáticamente al acercarse a zonas interactivas.

### Acciones básicas

El personaje debe:

- Recoger cajas automáticamente al acercarse.
- Apilar cajas visualmente.
- Soltar cajas automáticamente en zonas válidas.
- Preparar pedidos llevando cajas a la mesa correcta.
- Entregar paquetes en la zona de despacho.
- Comprar upgrades caminando hacia una zona de mejora.

---

## 4. Core loop principal

El loop central del juego debería ser:

1. Llegan cajas al **loading dock**.
2. El jugador recoge cajas.
3. Las lleva a estanterías o a la mesa de packing.
4. Aparecen pedidos simples.
5. El jugador prepara el pedido.
6. Lo entrega en la zona de despacho.
7. Gana dinero.
8. Compra upgrades.
9. El depósito se vuelve más eficiente.
10. Se desbloquea una zona nueva.

El objetivo no es que el jugador pierda. El objetivo es que sienta progreso constante.

### Loop emocional

El jugador debería sentir:

1. “Entiendo qué hacer.”
2. “Estoy ganando plata.”
3. “Puedo mejorar algo.”
4. “Ahora soy más rápido.”
5. “Quiero desbloquear lo siguiente.”
6. “Mi warehouse se está automatizando.”
7. “Una entrega más.”

---

## 5. Alcance del prototipo MVP

### Duración recomendada

**4 semanas para una versión jugable interna.**

No conviene apuntar directamente a App Store. Primero hay que validar si el loop de juego funciona.

### Objetivo del MVP

Responder esta pregunta:

> ¿Es divertido ordenar, preparar y entregar paquetes durante 5 minutos?

Si la respuesta es sí, se puede pasar a una fase de soft launch. Si la respuesta es no, no vale la pena agregar más features todavía.

---

## 6. Mapa del prototipo

El prototipo debería tener un solo depósito pequeño.

### Zonas del mapa

| Zona | Función |
|---|---|
| Loading Dock | Donde entran cajas |
| Shelves | Estanterías para guardar cajas |
| Packing Table | Mesa para preparar pedidos |
| Delivery Zone | Zona donde se despachan pedidos |
| Upgrade Station | Lugar donde comprar mejoras |
| Worker Area | Zona donde aparecen empleados automáticos |

### Visual

El depósito debe parecer real, pero simplificado:

- Cajas de colores.
- Estanterías.
- Palets.
- Camiones.
- Cintas transportadoras.
- Scanners.
- Máquinas de empaquetado.
- Señales visuales claras.
- Dinero y recompensas visibles.

No hace falta realismo. Hace falta claridad y satisfacción visual.

---

## 7. Mecánicas obligatorias del prototipo

### Movimiento

El jugador controla un personaje con joystick virtual.

El personaje debe:

- Recoger cajas automáticamente.
- Apilar cajas en la espalda o en un carrito.
- Soltar cajas automáticamente en zonas válidas.
- Tener capacidad máxima inicial.

Ejemplo:

- Nivel inicial: carga 3 cajas.
- Upgrade 1: carga 5 cajas.
- Upgrade 2: carga 8 cajas.
- Upgrade 3: carga 12 cajas.

### Cajas

Para el MVP usar solamente 3 tipos:

- Caja roja.
- Caja azul.
- Caja amarilla.

Más adelante se pueden agregar:

- Caja frágil.
- Caja refrigerada.
- Caja pesada.
- Caja premium.
- Caja express.
- Caja peligrosa.
- Caja internacional.

### Pedidos

Los pedidos aparecen en una cola simple.

Ejemplos:

- Pedido 1: 2 rojas + 1 azul.
- Pedido 2: 3 amarillas.
- Pedido 3: 1 roja + 1 azul + 1 amarilla.

El jugador tiene que llevar las cajas correctas a la mesa de packing. Cuando se completa el pedido, aparece un paquete listo para entregar.

### Dinero

Cada pedido entrega cash.

Ejemplo:

| Tipo de pedido | Recompensa |
|---|---:|
| Pedido simple | $10 |
| Pedido mediano | $25 |
| Pedido grande | $50 |
| Pedido express | $100 |

En el prototipo no hace falta una economía perfecta. Hace falta que el jugador pueda comprar upgrades cada 30-90 segundos.

---

## 8. Upgrades del prototipo

Los upgrades son lo que convierte el juego en un hybrid-casual y no en un minijuego aislado.

Para el prototipo pondría solo 6 upgrades:

| Upgrade | Efecto |
|---|---|
| Carry Capacity | El jugador puede cargar más cajas |
| Movement Speed | El jugador camina más rápido |
| Order Value | Cada pedido paga más |
| Packing Speed | La mesa prepara paquetes más rápido |
| Shelf Capacity | Las estanterías guardan más cajas |
| Hire Worker | Agrega un empleado automático |

El upgrade más importante es **Hire Worker**, porque es el primer momento en el que el jugador siente que el warehouse empieza a crecer solo.

---

## 9. Empleados automáticos

Para el prototipo pondría un solo tipo de worker.

### Worker básico

Tarea:

> Agarra cajas del loading dock y las lleva a las shelves.

Más adelante se pueden agregar:

- Worker de packing.
- Worker de delivery.
- Forklift driver.
- Drone.
- Manager.
- Robot arm.
- Truck dispatcher.

La clave es que el jugador vea progreso visual. El depósito empieza vacío y lento, y con el tiempo se vuelve más automático.

---

## 10. Onboarding

El tutorial debe durar menos de 60 segundos.

### Secuencia ideal

1. Flecha hacia las cajas.
2. Texto: “Pick up boxes”.
3. Flecha hacia packing table.
4. Texto: “Prepare order”.
5. Flecha hacia delivery zone.
6. Texto: “Deliver package”.
7. El jugador cobra dinero.
8. Flecha hacia upgrade station.
9. Compra primer upgrade.
10. Aparece el primer objetivo: “Complete 5 orders”.

No usar mucho texto. Mejor usar:

- Flechas.
- Íconos.
- Manito animada.
- Zonas brillantes.
- Feedback visual inmediato.

---

## 11. Qué NO incluir en el prototipo

Para no quemar tiempo, evitar:

- Multiplayer.
- Login.
- Cloud save.
- Leaderboards.
- Skins complejas.
- Daily rewards.
- Battle pass.
- Varias monedas.
- Varias ciudades.
- Publicidad real en producción.
- IAP reales.
- Física compleja de cajas.
- Vehículos manejables.
- Inventario manual.
- Mapas enormes.
- Historia.
- Diálogos.
- Backend.

El prototipo tiene que ser chico y enfocado.

---

## 12. Stack recomendado

### Motor

**Unity.**

Para este tipo de juego, Unity es la opción más práctica por:

- Ads.
- Mediation.
- Analytics.
- IAP.
- SDKs mobile.
- Publicación iOS.
- Ecosistema de assets.
- Mayor cantidad de ejemplos para monetización mobile.

Godot puede servir para prototipos simples, pero para un juego free-to-play mobile con ads e IAP, Unity es la opción más segura.

### Lenguaje

**C#**

### Arte

Para el prototipo:

- Low-poly 3D.
- Personajes simples.
- Cajas con colores claros.
- UI grande y limpia.
- Animaciones simples.
- Assets temporales de buena calidad.

No gastaría mucho en arte custom al principio. Primero validaría gameplay.

### Analytics

Desde el primer build usar:

- Firebase Analytics.
- Eventos custom.
- Remote Config más adelante.

Fuente útil:  
https://firebase.google.com/docs/analytics/unity/events

### Ads

Para MVP interno:

- No usar ads reales.
- Usar botones simulados tipo “Watch Ad”.
- Medir intención de click.
- Luego integrar test ads.

Para soft launch:

- AdMob.
- AppLovin MAX o Unity LevelPlay para mediation.
- Rewarded ads primero.
- Interstitials después.

Fuente útil:  
https://developers.google.com/admob/ios/rewarded

### IAP

Para iOS:

- StoreKit.
- Unity IAP.
- Productos configurados en App Store Connect.

Productos iniciales:

- Remove Ads.
- Starter Pack.
- Gem packs más adelante.

Fuente útil:  
https://developer.apple.com/app-store/review/guidelines/

### Backend

Para el prototipo:

**No usar backend.**

Guardar localmente:

- Dinero.
- Upgrades.
- Nivel de warehouse.
- Tutorial completado.
- Workers comprados.

Backend recién tendría sentido si se agregan:

- Eventos live.
- Cloud save.
- Leaderboards.
- Economy server-authoritative.
- Anti-cheat serio.
- Cross-device progress.

---

## 13. Arquitectura técnica simple

En Unity separaría sistemas así:

### Game Systems

- `PlayerController`
- `CarrySystem`
- `BoxSpawner`
- `OrderManager`
- `PackingStation`
- `DeliveryZone`
- `UpgradeManager`
- `WorkerAI`
- `CurrencyManager`
- `SaveManager`
- `AnalyticsManager`
- `AdManager`

### Data

Usaría `ScriptableObjects` para:

- Tipos de cajas.
- Configuración de pedidos.
- Costos de upgrades.
- Balance de economía.
- Configuración de workers.
- Rewards de ads.

Esto permite ajustar economía sin tocar código constantemente.

---

## 14. Eventos de analytics desde el día uno

Medir estos eventos:

| Evento | Para qué sirve |
|---|---|
| `tutorial_started` | Ver si el usuario arranca bien |
| `tutorial_completed` | Medir fricción inicial |
| `first_order_completed` | Validar loop básico |
| `upgrade_purchased` | Ver qué upgrades importan |
| `worker_hired` | Medir llegada al primer gran hito |
| `session_length` | Ver cuánto aguanta el jugador |
| `order_completed` | Medir ritmo |
| `ad_offer_shown` | Medir oportunidades de monetización |
| `rewarded_ad_clicked` | Medir intención |
| `rewarded_ad_completed` | Medir opt-in |
| `level_area_unlocked` | Medir progresión |

El evento más importante del prototipo:

> ¿Cuánta gente llega a contratar el primer worker?

Ese momento debería ocurrir rápido, idealmente en los primeros 3-5 minutos.

---

## 15. Plan de producción del prototipo

### Semana 1 — Core loop básico

**Objetivo:** que el jugador pueda moverse, agarrar cajas y completar pedidos.

Entregables:

- Escena del warehouse.
- Movimiento con joystick.
- Cajas que aparecen en loading dock.
- Sistema de pickup/drop.
- Mesa de packing.
- Zona de delivery.
- Pedidos básicos.
- Cash por pedido.
- UI mínima.

Al final de la semana 1, el juego ya debería ser jugable aunque se vea feo.

---

### Semana 2 — Upgrades y progresión

**Objetivo:** que haya sensación de crecimiento.

Entregables:

- Upgrade station.
- Carry capacity.
- Movement speed.
- Order value.
- Packing speed.
- Save/load local.
- Primer balance de costos.
- Tutorial básico.

Al final de la semana 2, el jugador debería poder jugar 5-7 minutos y sentir progreso.

---

### Semana 3 — Worker y polish de feedback

**Objetivo:** agregar automatización y hacer que el juego se sienta satisfactorio.

Entregables:

- Worker básico.
- Animaciones simples.
- Partículas al cobrar dinero.
- Sonidos de pickup, cash, delivery, upgrade.
- Mejoras visuales en UI.
- Flechas de guía.
- Objetivos tipo “Complete 10 orders”.
- Primer pase de economía.

Al final de la semana 3, el juego debería empezar a parecer producto, no demo técnica.

---

### Semana 4 — Test interno y métricas

**Objetivo:** probar si el loop retiene.

Entregables:

- Analytics.
- Build TestFlight.
- Eventos principales.
- Pantalla de fake ad rewards.
- Ajuste de onboarding.
- Ajuste de upgrade costs.
- 15-30 minutos de contenido.
- Fixes de bugs.

Fuente útil para TestFlight y revisión:  
https://developer.apple.com/app-store/review/guidelines/

---

## 16. Criterios para decidir si seguir o matar el prototipo

No seguir por intuición. Seguir por señales.

### Señales buenas

- El jugador entiende qué hacer sin explicación larga.
- Completa el primer pedido en menos de 60 segundos.
- Compra el primer upgrade en menos de 2 minutos.
- Contrata el primer worker en menos de 5 minutos.
- Juega 8-10 minutos en la primera sesión.
- Pide “una más”.
- Quiere desbloquear otra zona.
- Acepta voluntariamente recompensas tipo “x2 cash”.

### Señales malas

- No entiende dónde llevar cajas.
- Se aburre antes del primer upgrade.
- El movimiento se siente lento.
- Las cajas no se sienten satisfactorias.
- Los pedidos parecen trabajo, no juego.
- La economía se siente grindy desde el minuto 1.
- El depósito se ve desordenado, pero no de forma divertida.

---

## 17. Estrategia de monetización

### Modelo recomendado

**Free-to-play con hybrid monetization:**

- Rewarded ads.
- Interstitial ads moderados.
- IAP para remover ads.
- IAP para acelerar progreso.
- Starter packs.
- Packs pequeños.
- Más adelante: battle pass o eventos.

No conviene monetizar fuerte desde el día uno. Primero hay que diseñar el juego para que tenga lugares naturales donde ofrecer ads e IAP sin romper la experiencia.

---

## 18. Rewarded ads

Los rewarded ads son el formato principal para este juego.

Google define los rewarded ads como anuncios que el usuario elige ver a cambio de recompensas dentro de la app. Esto encaja perfecto con boosts, monedas y aceleradores.

Fuente útil:  
https://developers.google.com/admob/ios/rewarded

### Ubicaciones recomendadas

| Momento | Reward |
|---|---|
| Al completar un pedido grande | Duplicar cash |
| Al comprar upgrade caro | 20% descuento |
| Cuando el packing tarda | Terminar instantáneo |
| Cuando el worker está lento | Worker boost x2 por 3 min |
| Al volver al juego | Duplicar offline earnings |
| Al desbloquear zona | Bonus cash |
| Cuando falta poco para upgrade | “Watch ad to get missing cash” |

La mejor ubicación probablemente sea:

> “Double earnings” después de completar un pedido grande.

Es fácil de entender y no interrumpe.

---

## 19. Interstitial ads

Los interstitials deben usarse con cuidado.

Regla:

> Nunca durante acción activa. Solo en cortes naturales.

### Momentos posibles

- Después de completar cierto número de pedidos.
- Después de desbloquear una zona.
- Al cerrar el menú de upgrades.
- Al terminar una “warehouse day”.
- Al volver al mapa después de una pantalla de progreso.

### Cadencia inicial recomendada

- No mostrar interstitial en los primeros 2-3 minutos.
- No mostrar durante tutorial.
- Cooldown mínimo: 90-120 segundos.
- Cortar si el jugador acaba de ver un rewarded ad.
- Reducir interstitials para compradores.

Apple advierte contra apps diseñadas principalmente para mostrar anuncios o inflar impresiones/clicks artificialmente.

Fuente útil:  
https://developer.apple.com/app-store/review/guidelines/

---

## 20. IAP iniciales

### Producto 1 — Remove Ads

Precio tentativo:

**US$2.99 - US$4.99**

Remueve:

- Interstitials.
- Banners si existieran.

No remueve:

- Rewarded ads opcionales, porque esos son parte del sistema de recompensas.

Nombre posible:

**No Ads Pack**

---

### Producto 2 — Starter Pack

Precio tentativo:

**US$1.99 - US$2.99**

Incluye:

- Cash inicial.
- Gems.
- Skin simple.
- Worker boost temporal.
- Quizás “permanent +10% order value”.

Debe aparecer después de que el jugador entienda el valor de los upgrades, no en los primeros 10 segundos.

---

### Producto 3 — Gem Pack

Fase posterior.

Uso de gems:

- Comprar skins.
- Acelerar upgrades.
- Activar boosts.
- Desbloquear decoraciones.
- Comprar workers especiales.

No metería gem packs en el primer prototipo. Primero validaría si hay deseo de progresar.

---

### Producto 4 — Piggy Bank

Fase 2 o 3.

Funcionamiento:

- Mientras jugás, se llena una caja fuerte.
- Cuando llega a cierto valor, podés comprarla.
- Precio bajo.
- Alto valor percibido.

Este sistema funciona bien en casual/tycoon porque se siente como progreso acumulado.

---

## 21. Economía recomendada

### Monedas

Para el MVP:

- **Cash**: moneda principal.

Nada más.

Para fases siguientes:

- **Cash**: upgrades normales.
- **Gems**: premium currency.
- **Tickets**: eventos temporales.

No agregar gems en la primera versión jugable. Muchas monedas muy temprano confunden.

### Curva de upgrades

El jugador debería comprar upgrades frecuentemente al inicio.

Ejemplo:

| Minuto | Hito |
|---:|---|
| 0:30 | Completa primer pedido |
| 1:30 | Compra primer upgrade |
| 3:00 | Compra segundo upgrade |
| 5:00 | Contrata primer worker |
| 8:00 | Desbloquea nueva shelf |
| 12:00 | Desbloquea cinta transportadora |
| 15:00 | Llega primer pedido especial |

La primera sesión tiene que tener varios mini wins.

---

## 22. Fases siguientes

### Fase 1 — Prototype / proof of fun

**Objetivo:** validar que el loop básico es divertido.

Contenido:

- 1 warehouse.
- 3 tipos de cajas.
- 1 worker.
- 6 upgrades.
- 15 minutos de gameplay.
- Analytics.
- TestFlight.

Métrica principal:

> ¿La gente juega 8-10 minutos sin que le expliques demasiado?

---

### Fase 2 — Soft launch

**Objetivo:** validar retención y monetización inicial.

Agregar:

- Ads reales con test correcto y luego producción.
- Mediation.
- Primeros IAP.
- 30-60 minutos de contenido.
- 2-3 zonas nuevas.
- Offline earnings.
- Daily reward simple.
- Mejor tutorial.
- Store listing.
- 5-10 videos creativos para UA.

Métricas:

- D1 retention.
- D3 retention.
- D7 retention.
- Session length.
- Rewarded ad opt-in.
- Ads per DAU.
- ARPDAU.
- Crash-free sessions.
- CPI de primeras campañas.
- LTV estimado.

---

### Fase 3 — Hybrid casual completo

**Objetivo:** pasar de minijuego a producto con progresión.

Agregar:

- Nuevo warehouse: puerto, aeropuerto, supermercado, fábrica.
- Workers especializados.
- Managers.
- Máquinas automáticas.
- Cintas transportadoras.
- Forklift automático.
- Misiones.
- Achievements.
- Skins.
- Gems.
- Piggy bank.
- Starter packs.
- Remote Config para balance.

En esta fase el juego empieza a parecer negocio.

---

### Fase 4 — Live ops

**Objetivo:** dar motivos para volver cada semana.

Agregar:

- Eventos temporales.
- Warehouse temáticos.
- Temporadas.
- Battle pass liviano.
- Leaderboards simples.
- Misiones diarias.
- Login calendar.
- Ofertas segmentadas.
- Skins estacionales.
- Decoraciones de depósito.

Fuente útil sobre tendencias mobile/live ops:  
https://sensortower.com/blog/state-of-mobile-gaming-2025

---

### Fase 5 — Escalado

**Objetivo:** comprar usuarios de forma rentable.

Agregar:

- MMP: AppsFlyer, Adjust o similar.
- SKAdNetwork/SKAN setup.
- A/B testing de onboarding.
- A/B testing de economía.
- A/B testing de ads.
- Campañas en TikTok, Meta, Apple Search Ads, AppLovin.
- Creatives semanales.
- UA por país.
- ROAS tracking.

En iOS hay que tener especial cuidado con privacidad y tracking. Apple exige usar App Tracking Transparency cuando una app comparte datos con otras compañías para tracking entre apps y sitios web.

Fuente útil:  
https://developer.apple.com/documentation/apptrackingtransparency

---

## 23. Creatives para conseguir descargas

El marketing tiene que estar pensado desde el principio.

### Creative 1 — Satisfying stack

Mostrar al personaje cargando 20 cajas en una pila enorme y entregándolas todas juntas.

Hook:

> “Can you run the fastest warehouse?”

---

### Creative 2 — Antes/después

Inicio: depósito vacío y lento.  
Final: cintas, workers, forklifts y dinero explotando.

Hook:

> “From tiny storage room to logistics empire.”

---

### Creative 3 — Error visual

Mostrar cajas mal ubicadas, caos y pedidos acumulándose.

Hook:

> “Fix this warehouse!”

---

### Creative 4 — Upgrade dopamine

Cada 2 segundos se compra un upgrade y todo se acelera.

Hook:

> “Every upgrade makes it faster.”

---

### Creative 5 — Pedido express

Timer visual, muchas cajas y entrega urgente.

Hook:

> “Pack the order before the truck leaves.”

Los videos tienen que funcionar sin sonido y entenderse en 2-3 segundos.

---

## 24. Riesgos principales

### Riesgo 1 — Se siente como trabajo

Mover cajas puede volverse aburrido si no hay feedback constante.

Solución:

- Mucho cash visual.
- Sonidos satisfactorios.
- Upgrades frecuentes.
- Animaciones rápidas.
- Objetivos cortos.
- Automatización visible.

---

### Riesgo 2 — Demasiado idle, poco gameplay

Si los workers hacen todo, el jugador se queda mirando.

Solución:

- El jugador siempre debe ser el más eficiente.
- Workers ayudan, pero no reemplazan todo.
- Pedidos especiales requieren intervención manual.
- Eventos express activan gameplay activo.

---

### Riesgo 3 — Demasiado manual

Si el jugador tiene que caminar demasiado, se vuelve tedioso.

Solución:

- Distancias cortas.
- Speed upgrades tempranos.
- Unlocks de conveyor.
- Capacity upgrades.
- Autopickup generoso.
- Layout compacto.

---

### Riesgo 4 — Ads molestos

Si metés interstitials demasiado pronto, matás retención.

Solución:

- Rewarded primero.
- Interstitial después de cortes naturales.
- No interstitial en tutorial.
- No interstitial después de rewarded.
- No forzar ads para progresar.

---

### Riesgo 5 — Parecer copia directa

Apple advierte contra apps que simplemente copian juegos populares con cambios menores de nombre o UI.

Solución:

- Diferenciar con theme de warehouse/logistics.
- Hacer una mecánica propia de pedidos y rutas.
- Branding propio.
- Visuales propios.
- Progresión clara alrededor de automatización logística.

Fuente útil:  
https://developer.apple.com/app-store/review/guidelines/

---

## 25. Diferenciadores para fases posteriores

### 1. Order chaining

Si entregás pedidos en secuencia correcta, ganás combo.

Ejemplo:

- 3 pedidos seguidos sin error = x1.5 cash.
- 5 pedidos seguidos = bonus crate.
- 10 pedidos seguidos = special truck.

### 2. Warehouse layout

Más adelante, permitir pequeñas decisiones de layout:

- Dónde poner shelves.
- Dónde poner conveyor.
- Qué máquina priorizar.
- Qué ruta usan workers.

No hacer un city-builder complejo. Solo decisiones simples.

### 3. Pedido express

Pedidos con timer corto y recompensa alta.

Sirven para romper la monotonía.

### 4. Special boxes

- Fragile: no puede mezclarse.
- Frozen: hay que entregarla rápido.
- Heavy: ocupa 2 espacios.
- Gold: da bonus.
- Mystery: reward aleatorio.

### 5. Automation fantasy

El jugador empieza cargando cajas a mano y termina con:

- Cintas.
- Robots.
- Drones.
- Forklifts.
- Camiones automáticos.
- Sorting machines.

Ese arco visual es muy potente.

---

## 26. Roadmap resumido

| Fase | Duración | Objetivo | Resultado esperado |
|---|---:|---|---|
| Prototype | 4 semanas | Validar diversión | Build jugable de 15 min |
| Internal test | 1 semana | Detectar fricción | Ajustes de onboarding/economía |
| Soft launch | 4-8 semanas | Medir retención y monetización | Ads + IAP + 60 min contenido |
| Hybrid expansion | 2-3 meses | Profundizar progresión | Workers, zonas, gems, piggy bank |
| Live ops | Continuo | Retener usuarios | Eventos, skins, temporadas |
| Scale | Continuo | Comprar usuarios rentable | UA + ROAS positivo |

---

## 27. Backlog concreto del MVP

### Must-have

- Movimiento.
- Pickup/drop automático.
- 3 tipos de cajas.
- Sistema de pedidos.
- Packing table.
- Delivery zone.
- Cash.
- Upgrades.
- Worker básico.
- Save/load.
- Tutorial corto.
- UI de objetivos.
- Analytics.
- TestFlight build.

### Should-have

- Sonidos.
- Partículas.
- Animaciones simples.
- Fake rewarded ad.
- Offline earnings básico.
- Segunda zona desbloqueable.
- Upgrade visual del warehouse.

### Could-have

- Skins.
- Gems.
- Daily reward.
- Conveyor belt.
- Forklift.
- Pedido express.
- Special box.

### Not now

- Multiplayer.
- Backend.
- Battle pass.
- Leaderboards.
- Social login.
- Multiple worlds.
- Complex economy.
- Real-time events.

---

## 28. Recomendación final

Construir **Warehouse Master** como:

> Un idle tycoon 3D de logística, con control simple, pedidos rápidos, upgrades constantes, workers automáticos y monetización híbrida basada en rewarded ads + remove ads + starter packs.

Para el prototipo, enfocar todo en tres momentos:

1. **Primer pedido completado.**
2. **Primer upgrade comprado.**
3. **Primer worker contratado.**

Si esos tres momentos se sienten bien, hay producto.

Si no se sienten bien, no vale la pena agregar más features todavía.

La versión 1 no tiene que ser grande. Tiene que ser adictiva.

---

## 29. Checklist de validación del prototipo

Antes de avanzar a soft launch, revisar:

- [ ] El jugador entiende el objetivo sin explicación larga.
- [ ] El primer pedido se completa en menos de 60 segundos.
- [ ] El primer upgrade ocurre antes de los 2 minutos.
- [ ] El primer worker se contrata antes de los 5 minutos.
- [ ] La primera sesión dura al menos 8-10 minutos en testers interesados.
- [ ] El movimiento se siente rápido y claro.
- [ ] El feedback de pickup/drop es satisfactorio.
- [ ] Hay suficientes mini wins en los primeros 5 minutos.
- [ ] Los rewarded ads simulados generan interés.
- [ ] El juego no se siente como trabajo repetitivo.
- [ ] Los testers entienden qué quieren desbloquear después.

---

## 30. Links de referencia

- Firebase Analytics para Unity: https://firebase.google.com/docs/analytics/unity/events
- AdMob Rewarded Ads iOS: https://developers.google.com/admob/ios/rewarded
- Apple App Store Review Guidelines: https://developer.apple.com/app-store/review/guidelines/
- Apple App Tracking Transparency: https://developer.apple.com/documentation/apptrackingtransparency
- Sensor Tower — State of Mobile Gaming 2025: https://sensortower.com/blog/state-of-mobile-gaming-2025
- Liftoff — Casual Gaming Apps Report: https://liftoff.ai/2025-casual-gaming-apps-report/

---
