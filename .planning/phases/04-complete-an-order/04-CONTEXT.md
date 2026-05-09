# Phase 4: Complete an Order - Context

**Gathered:** 2026-05-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Wire the full pickup → packing → delivery → cash loop. UI surfaces an active order; player drops the required boxes at the packing station; once requirements are met, a packing timer runs and a deliverable Package spawns. Player carries the Package to the delivery zone for cash, the cash UI updates, and the next order spawns instantly.

No analytics events (Phase 9), no save persistence (Phase 6), no upgrades (Phase 5), no feedback effects/audio polish (Phase 8), no worker (Phase 7). Cash is in-memory only this phase. Phase 4 is the first phase that introduces gameplay state outside the dock/carry substrate.

Covers requirements: **BOX-03**, **ORD-01**, **ORD-02**, **ORD-03**, **ORD-04**, **ECON-01**.

</domain>

<decisions>
## Implementation Decisions

### Order Generation & Templates
- **D-01: Weighted-random order pick.** `OrderTemplate` SO has `float weight`; `OrderManager` picks via cumulative-weight random — same shape as the dock's `typeWeights` (Phase 3 D-14). Designer-friendly difficulty tuning.
- **D-02: Repeats allowed.** No anti-streak filter. With four templates, weighted-random already gives natural variety.
- **D-03: Four MVP templates from `specs/02 §7` verbatim:**
  - `Order_001`: 2 red → 10 cash
  - `Order_002`: 1 red + 1 blue → 15 cash
  - `Order_003`: 2 yellow + 1 blue → 25 cash
  - `Order_004`: 2 red + 2 blue + 1 yellow → 50 cash
  Stored as SOs at `Assets/_Project/ScriptableObjects/Orders/Order_001.asset` … `Order_004.asset`.
- **D-04: Multi-trip allowed.** Total order box count may exceed `PlayerStats.CarryCapacity`. `OrderManager` does NOT filter by capacity. Packing timer waits for ALL required boxes to arrive at the station before starting (matches `specs/02 §6` — "all requirements are met → packing begins").

### `OrderTemplate` ScriptableObject schema
- **D-05: Schema (locked):**
  ```csharp
  [CreateAssetMenu(menuName = "WM/Orders/OrderTemplate")]
  public class OrderTemplate : ScriptableObject {
      public string id;                       // e.g., "order_001"
      public List<Requirement> requirements;  // [{ BoxType type; int count }, ...]
      public int   baseCashReward;            // table-based per spec §4
      public float weight;                    // for weighted-random pick
  }

  [Serializable]
  public struct Requirement {
      public BoxType type;
      public int count;
  }
  ```
  `BoxType` is the SO already locked in Phase 3 D-06 — referenced directly, no string lookups.

### Packing Station — Drop Detection
- **D-06: Station-driven trigger.** `PackingStation` has a child `BoxCollider` (`isTrigger = true`). On `OnTriggerEnter`/`OnTriggerStay` with anything implementing `ICarrier`, the station drives the drop loop. Mirrors the loading dock pattern (Phase 3 D-01) — same seam works for the Phase 7 worker if it ever needs to deposit.
- **D-07: Required-only drip-feed.** While carrier overlaps trigger AND order has remaining demand for a type the carrier holds: station calls `CarrySystem.TryRemove(requiredType, out box)` every `dropIntervalSeconds = 0.15f` (mirrors Phase 3 D-02 pickup tempo). Non-matching boxes stay on carrier — never re-shuffled, no jitter. Spec §6: "accepts only boxes required".
- **D-08: Order seam = `IActiveOrder` interface (`WM.Orders`).**
  ```csharp
  public interface IActiveOrder {
      OrderTemplate Template { get; }            // null if no active order
      int RemainingFor(BoxType type);            // 0 if not required or already met
      void Submit(BoxType type);                 // station calls on each accepted drop
      bool RequirementsMet { get; }
      event Action OnRequirementsMet;
      event Action OnPackingStarted;
      event Action OnPackingCompleted;
      event Action<OrderTemplate> OnNewOrder;    // fires when AdvanceToNext is called
  }
  ```
  `OrderManager` implements. Station and HUD read via the interface — zero coupling to concrete `OrderManager` type.
- **D-09: Consumed boxes reparent to a `DropZone` Transform on the station, then are destroyed when the packing timer starts.** Station has a child `DropZone` empty Transform; on accept, station unparents box from carrier, reparents to `DropZone`, snaps to a slot offset (similar to dock slots, simpler — single column stack), and disables the box collider. When `IActiveOrder.RequirementsMet` fires and the timer starts, `DropZone.DestroyAllChildren()` clears the visible boxes. Multi-trip players see their progress accumulate visually before the package replaces it.

### Packing Timer
- **D-10: Configurable duration on a `PackingStationConfig` SO.** `Assets/_Project/ScriptableObjects/Stations/PackingStationConfig.asset`. Field: `float basePackingDurationSeconds = 3.0f` (placeholder — playtest will retune). Phase 5's packing-speed upgrade reads `PlayerStats.PackingDurationMultiplier` (added in Phase 5) at timer-start time. Phase 4 only consumes the base value.
- **D-11: Timer is owned by `OrderManager`** (not the station). When station calls `Submit` and `RequirementsMet` fires, `OrderManager` starts a coroutine over `basePackingDurationSeconds`, then fires `OnPackingCompleted`. `PackingStation` listens to `OnPackingCompleted` and spawns the Package at its `PackageOutputSlot`. Centralizing timer in `OrderManager` makes Phase 5's multiplier hookup a one-line change in one file.

### Package
- **D-12: New `Package.prefab`** at `Assets/_Project/Prefabs/Packages/Package.prefab`. Contains `Package` MonoBehaviour + `MeshRenderer` + `BoxCollider`. Distinct visual identity (placeholder: brown `Cube` w/ tape-like darker stripe material). Sells the "this is the deliverable" beat — separate from any consumed Box visually.
- **D-13: `Package` MonoBehaviour** in `WM.Stations` (or `WM.Orders` — see D-21). Carries the source `OrderTemplate` reference: `package.SourceOrder` so `DeliveryZone` can read `package.SourceOrder.baseCashReward`.
- **D-14: Carry seam — generalize `ICarrier` to accept `ICarryable`.** This is a Phase 4 retro-edit of Phase 3 D-04 (paper-locked, no code yet — clean to revise here).
  ```csharp
  public interface ICarryable {
      Transform Transform { get; }       // for reparenting
      Collider  Collider  { get; }       // for disable-while-carried
  }

  public interface ICarrier {
      int  FreeCapacity { get; }
      bool TryAccept(ICarryable item);
  }
  ```
  `Box` implements `ICarryable`. `Package` implements `ICarryable`. `CarrySystem` stores `List<ICarryable>` instead of `List<Box>`. Existing `TryRemove(BoxType type, out Box box)` unchanged (still type-filtered for packing). New `bool TryRemovePackage(out Package package)` for delivery — picks the first carried `Package` (if any).
- **D-15: Package occupies one carry slot.** `Package` reuses `CarrySystem`'s stack offset logic (Phase 3 D-12). Stack rendering treats it like a single carryable; no special positioning. Player can carry boxes for the next order while delivering — no stack lock.
- **D-16: Package pickup driven by `PackingStation`.** Same drip-feed loop as the dock (Phase 3 D-01): if `Package` exists at the station's `PackageOutputSlot` AND carrier overlaps trigger AND has free capacity, station calls `carrier.TryAccept(package)`. Reuses `pickupIntervalSeconds` from Phase 3 (`0.15s` cadence is already the project rhythm).

### Delivery Zone
- **D-17: Delivery zone is also station-driven via a child trigger.** Same pattern as packing/dock. On `OnTriggerEnter`/`OnTriggerStay` with `ICarrier` carrying a `Package`: zone calls `carrier.TryRemovePackage(out package)`, awards `package.SourceOrder.baseCashReward` via `ICurrencyService.Add(...)`, fires `IActiveOrder.AdvanceToNext()`, and `Destroy(package.gameObject)`. No timer, no animation in Phase 4 — Phase 8 layers on cash-pop / particle / sfx.
- **D-18: Order finalization order:** `Add cash → AdvanceToNext order → Destroy package`. Order manager fires `OnNewOrder` so `OrderHUD` updates immediately as the player crosses the trigger — feels instant, no dead time.

### Currency Service
- **D-19: `ICurrencyService` plain-C# service via Bootstrap (Phase 1 D-07/D-09).**
  ```csharp
  public interface ICurrencyService {
      int Cash { get; }
      void Add(int amount);
      bool TrySpend(int amount);
      event Action<int> OnChanged;       // emits new total
  }
  ```
  Phase 4 implementation: `CurrencyManager` plain C# class in `WM.Economy`, in-memory `int _cash`. Phase 6 swaps to a save-aware variant. Phase 5's `UpgradeManager` calls `TrySpend`. Phase 9's analytics observes `Add` for the `order_completed` event's value field.
- **D-20: `Bootstrap` injection wiring (Phase 4 additions):**
  - `Bootstrap.Awake`: instantiate `CurrencyManager` (no-arg ctor), inject as `ICurrencyService` into `OrderManager.Init(...)`, `DeliveryZone.Init(...)`, `CashHUD.Init(...)`.
  - `Bootstrap.Awake`: load `OrderTemplate[]` (serialized field `[SerializeField] OrderTemplate[] orderTemplates`), inject into `OrderManager.Init(currencyService, orderTemplates)`. `OrderManager` picks the first order in `Init` and fires `OnNewOrder`.
  - `OrderHUD.Init(IActiveOrder)` and `CashHUD.Init(ICurrencyService)` — both subscribe to events on construction.

### Asmdef Layout & Type Locations
- **D-21: Type homes:**
  - `WM.Orders` — `OrderTemplate` SO, `IActiveOrder` interface, `OrderManager` MonoBehaviour, `Requirement` struct.
  - `WM.Stations` — `PackingStation`, `DeliveryZone`, `PackingStationConfig` SO. References `WM.Orders` (for `IActiveOrder`) and `WM.Boxes` (for `ICarrier`/`ICarryable`/`BoxType`).
  - `WM.Boxes` — `Package` MonoBehaviour and `Package.prefab`. `Package` references `WM.Orders` (for `OrderTemplate.SourceOrder`). Generalized `ICarrier`/`ICarryable` interfaces stay in `WM.Boxes` (already the home of `ICarrier` per Phase 3 D-04). `WM.Boxes` already references nothing else — the `OrderTemplate` reference is via `WM.Orders` → consider a `WM.Stations.Output` thin asmdef if cycles emerge.
  - `WM.Economy` — `ICurrencyService` interface, `CurrencyManager` implementation. No scene presence.
  - `WM.UI` — `OrderHUD`, `CashHUD` MonoBehaviours. References `WM.Orders` (for `IActiveOrder`) and `WM.Economy` (for `ICurrencyService`).

  **Cycle check:** `WM.Stations` → `WM.Orders` ✓, `WM.Stations` → `WM.Boxes` ✓, `WM.Boxes` → `WM.Orders` (Package's SourceOrder) ⚠ — if this introduces a cycle, move `Package` MonoBehaviour to `WM.Orders` (Package conceptually belongs to the order anyway). Planner to verify; default `Package` placement is `WM.Orders`.

### `OrderManager` Composition
- **D-22: `OrderManager` MonoBehaviour on the existing `GameManager` GameObject** (Phase 1 D-08 placed `GameManager` in scene). `Bootstrap` finds it via `GameManager.GetComponent<OrderManager>()` and calls `OrderManager.Init(currencyService, orderTemplates)`. Templates are an `[SerializeField] OrderTemplate[]` on the `OrderManager` component itself, populated in Inspector. Implements `IActiveOrder` directly.
- **D-23: `OrderManager` lifecycle:** On `Init`, picks first template via weighted-random, sets `Template`, fires `OnNewOrder`. On every `Submit(type)`, decrements `_remaining[type]`; if all hit zero → fire `OnRequirementsMet`, start packing timer coroutine. After timer → fire `OnPackingCompleted`. After delivery → `AdvanceToNext()` picks next template, fires `OnNewOrder`. No queue, no batching — single active order per `specs/02 §7` MVP rules.

### HUD Composition
- **D-24: Three sibling panels under `SafeAreaPanel`:** `OrderHUD` (top-center), `CashHUD` (top-right), `Joystick` (bottom-left, already added in Phase 2 D-01). Each its own MonoBehaviour in `WM.UI`. No `GameHUD` orchestrator MonoBehaviour — keeps each widget independently wired by `Bootstrap`.
- **D-25: `OrderHUD` layout** — horizontal `LayoutGroup` of `BoxRequirementWidget`s (Image + TMP_Text "remaining/total"). Subscribes to `IActiveOrder.OnNewOrder` (rebuilds widget set) and updates per-frame from `RemainingFor(type)` for each requirement (cheap; ≤3 types per order). Color comes from `BoxType.color` (Phase 3 D-06 already on the SO). Add `BoxType.icon : Sprite` field for the HUD icon — extends Phase 3 D-06 schema by one optional field; existing three SOs get a placeholder colored sprite asset.
- **D-26: `CashHUD`** — `TextMeshProUGUI` "¤ {value}" with rolling-number tween on `OnChanged`: lerp from `_displayedCash` → `OnChanged.newValue` over `0.4s` ease-out (`Mathf.Lerp` per-frame in `Update`, no DOTween — no unspecified third-party packages per `CLAUDE.md`). Coin icon Image to the left of the text. Anchored top-right inside `SafeAreaPanel`.
- **D-27: Packing-timer UI = world-space radial fill** child of `PackingStation`. World-space `Canvas` with `Image` (`Type = Filled`, `Fill Method = Radial360`). `OrderManager.OnPackingStarted` → `gameObject.SetActive(true)` and animate `fillAmount` 0→1 over duration; on `OnPackingCompleted` → hide. Hidden between orders. Reads spatially — player sees "this station is busy here".

### Claude's Discretion
- **D-09 (`DropZone` slot layout)**: simple vertical stack offset from `boxHeight + spacing`. Layout tuning is cosmetic; planner can revise.
- **D-10 default `3.0s` packing duration**: placeholder; first-order-< 60s session shape (`PROJECT.md`) gives a budget — playtest will retune.
- **D-12 Package visual identity**: placeholder brown cube w/ darker stripe material. Phase 8 may swap for a proper crate placeholder; outside MVP art scope per `PROJECT.md`.
- **D-21 `Package` asmdef placement** (`WM.Orders` vs `WM.Boxes`): default `WM.Orders` to avoid cycles; planner verifies the actual asmdef graph during implementation.
- **D-25 `BoxType.icon` field addition**: Claude default — needed for HUD readability. If artwork constraints prevent producing per-type icons in MVP, planner may fall back to flat-color quad with letter ("R"/"B"/"Y").
- **D-26 rolling-tween duration `0.4s`**: hand-tuned default. Phase 8 may polish.
- All five areas converged on the recommended option; no "you decide" deferrals.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project Source of Truth
- `CLAUDE.md` — Spec-driven development, language=English, small MonoBehaviours, no unspecified third-party packages, ScriptableObjects for tunable data.
- `.planning/PROJECT.md` — Visible stacked carry on character (key decision); first order < 60s session-shape; ScriptableObjects for tunable data; no scattered SDK calls.
- `.planning/REQUIREMENTS.md` — Phase 4 covers **BOX-03** (auto-drop at valid stations, visual stack updates), **ORD-01** (order templates SO with required quantities, base cash reward, weight), **ORD-02** (one active order, exact-match tracking, UI shows progress), **ORD-03** (packing station accepts only required boxes, configurable timer, deliverable package), **ORD-04** (delivery zone awards cash, feedback hook, next order), **ECON-01** (CurrencyManager tracks cash, UI updates, persists later via save).
- `.planning/ROADMAP.md` — Phase 4 goal, three success criteria, plan stubs (04-01 OrderTemplate SOs, 04-02 OrderManager, 04-03 PackingStation, 04-04 DeliveryZone, 04-05 CurrencyManager + cash UI).

### Prior Phase Substrate (read before planning)
- `.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md` — Asmdefs already created: `WM.Orders`, `WM.Stations`, `WM.Economy`, `WM.UI` (D-04). `PackingStation` (blue), `DeliveryZone` (green) GameObjects positioned in `Warehouse_MVP.unity` (D-11) — note Phase 1 used non-trigger BoxColliders; Phase 4 adds child trigger BoxColliders for detection without removing the blocking parent collider. `Bootstrap` composition root pattern (D-07/D-09) — Phase 4 extends Bootstrap to inject `ICurrencyService` and `OrderTemplate[]` into `OrderManager`. `GameManager` MonoBehaviour exists (D-08) — Phase 4 attaches `OrderManager` MonoBehaviour to it. `SafeAreaPanel` portrait 1080×1920 canvas with `Input System UI Input Module` (D-15/D-16) — Phase 4 adds `OrderHUD`, `CashHUD` siblings to `Joystick` under it. ScriptableObject folder convention `Assets/_Project/ScriptableObjects/{System}/` (D-05) — Phase 4 adds `Orders/` and reuses `Stations/` (added in Phase 3 D-08).
- `.planning/phases/02-walk-around-the-warehouse/02-CONTEXT.md` — `PlayerController` in `WM.Player` (D-05), `PlayerConfig` SO + `PlayerStats` runtime mutation surface (D-14/D-16). Phase 4 doesn't touch player movement, but `PlayerStats` is the established pattern for runtime mutables — Phase 5's order-value and packing-speed multipliers will live there.
- `.planning/phases/03-pick-up-carry-boxes/03-CONTEXT.md` — `ICarrier` interface and `CarrySystem` MonoBehaviour in `WM.Boxes` (D-04, D-09). **Phase 4 retro-generalizes `ICarrier` to accept `ICarryable`** — Phase 3 is paper-locked (no code yet) so this is a clean revision; the planner for Phase 3 must produce the generalized interface from day one. `Box` SOs (`Box_Red`, `Box_Blue`, `Box_Yellow`) with `BoxType` schema (D-06, D-07) — Phase 4 reads `box.Type` for matching and adds an `icon : Sprite` field for the OrderHUD. Drip-feed pickup tempo `0.15s` (D-02) — Phase 4 reuses for packing drop and package pickup. `LoadingDock` station-driven trigger pattern (D-01) — Phase 4 mirrors for `PackingStation` and `DeliveryZone`. Box reparenting model (D-11) — Phase 4 follows for both consumed boxes (→ DropZone) and Package (→ player CarryAnchor).

### Architecture & Spec
- `specs/02-gameplay-systems-spec.md` §6 (Packing Station) — Accepts only required boxes, configurable timer, packing speed upgradeable, package appears on completion.
- `specs/02-gameplay-systems-spec.md` §7 (Order System) — MVP rules: 1-5 boxes, 1+ types, single active order; example templates 001-004 (canonical for D-03); acceptance criteria.
- `specs/02-gameplay-systems-spec.md` §8 (Delivery Zone) — Walk-into-zone consumption, immediate cash, feedback + analytics hooks (analytics deferred to Phase 9).
- `specs/03-economy-progression-spec.md` §4 (Order Rewards) — Reward values 5/10/20/35-50; spec explicitly recommends table-based rewards over formula for MVP (drives D-01/D-05/D-19).
- `specs/06-technical-architecture-spec.md` §3 — Folder structure (`Prefabs/Packages/`, `ScriptableObjects/Orders/`).
- `specs/06-technical-architecture-spec.md` §4 — Core systems (`OrderManager`, `PackingStation`, `DeliveryZone`, `CurrencyManager`).
- `specs/06-technical-architecture-spec.md` §6 — Service abstractions: `ICurrencyService` mirrors the `IAnalyticsService`/`IAdService`/`IIapService`/`ISaveService` pattern (Phase 1 D-09).
- `specs/07-mvp-backlog-acceptance-criteria.md` — WM-005 (BOX-03 box drop), WM-007 (ORD-01 templates), WM-008 (ORD-02 active order), WM-009 (ORD-03 packing), WM-010 (ORD-04 delivery), WM-011 (ECON-01 cash).

### Reference (non-binding)
- `warehouse_master_plan.md` — Original Spanish master plan; reference for feel only.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **None in code yet.** Phases 1-3 are paper-locked via prior CONTEXT.md files; the substrate is a contract, not committed code. Phase 4 plans must respect those locks even though no compilation has happened.

### Established Patterns (paper-locked)
- **Station-driven trigger + drip-feed cadence (`0.15s`)** — Phase 3 D-01/D-02 establishes the pattern; Phase 4's `PackingStation` and `DeliveryZone` reuse it identically. Same `pickupIntervalSeconds` semantics.
- **Bootstrap composition root** — Phase 1 D-07. Phase 4 extends `Bootstrap.Awake` to wire `CurrencyManager` (as `ICurrencyService`) into `OrderManager`, `DeliveryZone`, and `CashHUD`. No singletons, no static state.
- **Per-system asmdef** — Phase 1 D-04 already created `WM.Orders`, `WM.Stations`, `WM.Economy`, `WM.UI`. Phase 4 only adds files to existing asmdefs — no new asmdef.
- **ScriptableObject for tunable data** — every magic number in Phase 4 (packing duration, drop tempo, weights, rewards) lives on a SO. `OrderTemplate`, `PackingStationConfig`, plus the existing `LoadingDockConfig`/`PlayerConfig`/`BoxType` SOs.
- **Reparenting model for carryables** — Phase 3 D-11 establishes "single instance flows dock → player → station". Phase 4 extends: dock → player → packing-DropZone → destroy; package spawns at PackageOutputSlot → player → delivery-zone → destroy.

### Integration Points
- `GameManager` GameObject (Phase 1 D-08, D-11) — Phase 4 attaches `OrderManager` MonoBehaviour. `Bootstrap` finds it via `GetComponent<OrderManager>()` and calls `Init(currencyService, orderTemplates)`.
- `PackingStation` GameObject (Phase 1 D-11, blue placeholder) — Phase 4 attaches `PackingStation` MonoBehaviour + `PackingStationConfig` reference + child `Trigger` (BoxCollider isTrigger=true) + child `DropZone` Transform + child `PackageOutputSlot` Transform + child world-space `Canvas` (radial-fill packing timer).
- `DeliveryZone` GameObject (Phase 1 D-11, green placeholder) — Phase 4 attaches `DeliveryZone` MonoBehaviour + child `Trigger` (BoxCollider isTrigger=true). No config SO needed for delivery — it pulls `cashReward` off the carried `Package.SourceOrder`.
- `Bootstrap` (Phase 1 D-07) — Phase 4 extends with `ICurrencyService` instantiation and `OrderTemplate[]` `[SerializeField]`.
- `CarrySystem` in `WM.Boxes` (Phase 3 D-09) — Phase 4 generalizes to store `List<ICarryable>` and adds `bool TryRemovePackage(out Package package)`. `TryRemove(BoxType, out Box)` API stays unchanged for the packing station.
- `BoxType` SO (Phase 3 D-06) — Phase 4 adds `Sprite icon` field. Existing three SOs need an icon Sprite assigned (placeholder colored quad with letter is acceptable).
- `SafeAreaPanel` under `UICanvas` (Phase 1 D-15, Phase 2 D-01) — Phase 4 adds `OrderHUD` (top-center anchor) and `CashHUD` (top-right anchor) sibling children of the `Joystick` panel.

</code_context>

<specifics>
## Specific Ideas

- **Spec-table rewards 10/15/25/50 are canonical for MVP** (`specs/02 §7`, `specs/03 §4`). Hardcoding these values into the four `Order_00X.asset` SOs is the right move — playtest can retune without code changes.
- **Hybrid-casual cadence target (first order < 60s, `PROJECT.md`)** drives:
  - Default `0.15s` drop tempo (D-07) so a 5-box order takes ~0.75s of dropping animation, not 5+ seconds.
  - Default `3.0s` packing duration (D-10) — short enough that the player walks back to the dock and the package is ready when they return; budget 5-10s round-trip plus 3s packing keeps the loop tight.
  - Instant next-order spawn on delivery (D-18) — zero-latency loop closure.
- **`IActiveOrder` is the seam for everything downstream** (D-08). Phase 5's UpgradeManager will read `IActiveOrder.OnPackingCompleted` for analytics in Phase 9; Phase 8 tutorial will read `OnNewOrder` to drive its prompts. Designing the interface here pays compound interest.
- **Generalizing `ICarrier` to `ICarryable` now (D-14) avoids a Phase 7 retrofit** — the worker AI in Phase 7 may want to carry items that aren't strictly Boxes (intermediate states, batched package transports). One refactor here, none later.
- **No `GameHUD` orchestrator** (D-24). Each HUD widget self-wires via Bootstrap. Matches "small MonoBehaviours" and avoids the "god UI manager" trap.

</specifics>

<deferred>
## Deferred Ideas

- **Cash-pop / particle / SFX feedback on delivery and packing completion** — Phase 8 (Tutorial & Feel) + TUT-02. Phase 4 leaves `DeliveryZone.OnDelivered` and `OrderManager.OnPackingCompleted` events open as the hooks.
- **`first_order_completed` and `order_completed` analytics events** — Phase 9 (Analytics Wiring). Phase 4 captures the data they'll need (cash awarded, order id, box counts) on the events themselves; Phase 9 adds the `IAnalyticsService` subscriber.
- **Save persistence for cash** — Phase 6 (Save). Phase 4 ships in-memory only; `CurrencyManager` is a plain class with no save hook beyond the existing `Bootstrap` injection seam.
- **`Order Value` upgrade (Phase 5 ECON-05)** — Phase 4 reads `package.SourceOrder.baseCashReward` directly. Phase 5 adds `PlayerStats.OrderValueMultiplier` and `DeliveryZone` becomes `cashAwarded = baseCashReward * playerStats.OrderValueMultiplier`. One-line change.
- **`Packing Speed` upgrade (Phase 5 ECON-06)** — Phase 4 reads `PackingStationConfig.basePackingDurationSeconds`. Phase 5 adds `PlayerStats.PackingDurationMultiplier`; `OrderManager.StartPackingTimer` becomes `duration = config.basePacking * playerStats.PackingDurationMultiplier`.
- **Order queue / multi-order parallelism** — Spec §7: "Only one active order is required for MVP. A visible queue can be added later." Out of MVP scope.
- **Order chaining combos / express orders** — `PROJECT.md` Out of Scope.
- **5-box order template variants where total > base carry capacity (3)** — Multi-trip allowed (D-04) covers this without scope change. Phase 5's carry-capacity upgrade (ECON-03) reduces multi-trip frequency naturally.
- **OrderTemplate "minLevel" / "unlock" gating** — Out of MVP scope. All four templates active from session start.
- **Package physics on drop / mid-flight visual** — No drop API in this phase; package only flows station → carrier → delivery-zone → destroy. Mid-flight visual polish (e.g., bobbing on player back) is Phase 8.
- **`BoxType.icon` field having proper artwork** — Placeholder per-type colored sprite acceptable for MVP. Final art is outside MVP scope per `PROJECT.md`.
- **DOTween / LeanTween / similar tween library for cash rolling animation (D-26)** — Plain `Mathf.Lerp` per-frame chosen to honor `CLAUDE.md` "no unspecified third-party packages". If tween needs grow (multiple HUD elements in Phase 8), revisit then.
- **Order generation seeding for tutorial determinism** — Phase 8 may want the first order to always be `Order_001` (simplest, 2 red). Phase 4 picks weighted-random from full set. Phase 8 can override `OrderManager.Init` with a seeded template.
- **Anti-streak rule (no same template twice in a row)** — D-02 explicitly allows repeats. Revisit if playtest finds it boring.
- **PackingStation and DeliveryZone audio cues** — Phase 8.
- **CashHUD currency icon swap** — Currently a coin Sprite; if a different visual identity emerges in art passes, swap the field. No code change.

</deferred>

---

*Phase: 4-complete-an-order*
*Context gathered: 2026-05-09*
