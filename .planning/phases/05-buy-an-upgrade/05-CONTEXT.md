# Phase 5: Buy an Upgrade - Context

**Gathered:** 2026-05-09 (--auto mode)
**Status:** Ready for planning

<domain>
## Phase Boundary

Wire the upgrade station: walking onto its trigger opens an overlay UI listing all six MVP upgrades with current level, next-level effect, and cost. Buying an affordable upgrade deducts cash via `ICurrencyService.TrySpend`, increments the upgrade level, and applies the effect immediately. Five of the six upgrades mutate runtime stats; the sixth (Hire Worker) flips an `WorkerUnlocked` flag that Phase 7 reads. All levels persist in-memory this phase; Phase 6 wires save.

Covers requirements: **ECON-02**, **ECON-03**, **ECON-04**, **ECON-05**, **ECON-06**, **ECON-07**.

</domain>

<decisions>
## Implementation Decisions

### `UpgradeDef` ScriptableObject schema
- **D-01:** Schema (locked):
  ```csharp
  [CreateAssetMenu(menuName = "WM/Upgrades/UpgradeDef")]
  public class UpgradeDef : ScriptableObject {
      public string id;                       // "carry_capacity", "movement_speed", "order_value", "packing_speed", "shelf_capacity", "hire_worker"
      public string displayName;
      public Sprite icon;
      public int    maxLevel;                 // Phase 5: 5 for stat upgrades, 1 for hire_worker
      public int    baseCost;                 // see D-02
      public List<float> costMultipliers;     // length = maxLevel; [1.0, 2.0, 3.5, 6.0, 10.0] per spec §5
      public UpgradeEffectKind effect;        // enum: CarryCapacity, MovementSpeed, OrderValue, PackingSpeed, ShelfCapacity, HireWorker
      public List<float> effectCurve;         // length = maxLevel + 1; index 0 = base; per spec §6
  }
  ```
  Six SOs at `Assets/_Project/ScriptableObjects/Upgrades/Upgrade_<id>.asset`.
- **D-02:** **Initial costs from spec §5:** carry=20, speed=30, order_value=50, packing=60, shelf=75, hire_worker=150 (single-purchase). Cost curve `[1.0, 2.0, 3.5, 6.0, 10.0]` per spec §5. Hire Worker has `costMultipliers = [1.0]` (one level only).
- **D-03:** **Effect curves from spec §6** (verbatim):
  - CarryCapacity (index = level): `[3, 5, 8, 12, 16, 20]` — absolute capacity values; level 0 baseline must equal Phase 3 D-10 default.
  - MovementSpeed multiplier: `[1.00, 1.10, 1.22, 1.36, 1.52, 1.70]` (placeholders inferred from spec format — refer to spec §6 actual table at planning time).
  - OrderValue multiplier: `[1.00, 1.15, 1.35, 1.60, 1.90, 2.25]`.
  - PackingSpeed multiplier (duration scale): `[1.00, 0.85, 0.70, 0.58, 0.48, 0.40]`.
  - ShelfCapacity (absolute slots): planner reads spec §6 verbatim.
  - HireWorker `[0, 1]` — boolean.

### `UpgradeManager` Composition
- **D-04:** **`UpgradeManager` MonoBehaviour on `GameManager`** (mirrors `OrderManager` pattern, Phase 4 D-22). Bootstrap-injected: `UpgradeManager.Init(ICurrencyService currency, UpgradeDef[] defs, IUpgradeEffect[] effects)`. Implements `IUpgradeService`:
  ```csharp
  public interface IUpgradeService {
      int  GetLevel(string id);
      int  GetNextCost(string id);            // base * costMultipliers[currentLevel], 0 if maxed
      bool CanAfford(string id);
      bool TryPurchase(string id);            // deducts cash, increments level, applies effect, fires OnLevelChanged
      bool IsMaxed(string id);
      event Action<string,int> OnLevelChanged;  // (id, newLevel)
  }
  ```
- **D-05:** **Levels stored in `Dictionary<string,int>`**, in-memory only this phase. Phase 6 swaps to save-aware `UpgradeManager` via `ISaveService` integration; `IUpgradeService` interface unchanged.
- **D-06:** **One `IUpgradeEffect` impl per upgrade** for explicit composition (small classes, single responsibility). Bound by `id` in a `Dictionary<string,IUpgradeEffect>`.
  ```csharp
  public interface IUpgradeEffect {
      string Id { get; }
      void Apply(UpgradeDef def, int newLevel);
  }
  ```
  Six concrete impls in `WM.Upgrades`:
  - `CarryCapacityEffect`: `playerStats.CarryCapacity = (int)def.effectCurve[newLevel]`
  - `MovementSpeedEffect`: `playerStats.MoveSpeed = playerConfig.baseMoveSpeed * def.effectCurve[newLevel]`
  - `OrderValueEffect`: `playerStats.OrderValueMultiplier = def.effectCurve[newLevel]` (NEW field on `PlayerStats`)
  - `PackingSpeedEffect`: `playerStats.PackingDurationMultiplier = def.effectCurve[newLevel]` (NEW field on `PlayerStats`)
  - `ShelfCapacityEffect`: `shelfArea.SetCapacity((int)def.effectCurve[newLevel])`
  - `HireWorkerEffect`: `playerStats.WorkerUnlocked = true` (NEW bool on `PlayerStats`); Phase 7 reads on init or subscribes to `OnLevelChanged("hire_worker")`.
- **D-07:** **`PlayerStats` extensions for Phase 5:** add `OrderValueMultiplier`, `PackingDurationMultiplier` (floats, default 1.0), and `WorkerUnlocked` (bool, default false). Initialized from `PlayerConfig` baselines on Awake; `UpgradeManager` mutates after purchase.
- **D-08:** **`OrderManager` and `DeliveryZone` retro-bind to multipliers** (Phase 4 deferred-ideas list confirmed):
  - `OrderManager.StartPackingTimer`: `duration = packingConfig.basePackingDurationSeconds * playerStats.PackingDurationMultiplier`.
  - `DeliveryZone.OnDelivered`: `cashAwarded = Mathf.RoundToInt(package.SourceOrder.baseCashReward * playerStats.OrderValueMultiplier)`.
  Phase 4 plans MUST include these fields on `PlayerStats` from day one — Phase 5 only sets them.

### Upgrade Station UI
- **D-09:** **Full-screen overlay panel** on `UICanvas/SafeAreaPanel` named `UpgradeStationPanel`. Triggered by walking into a child trigger collider on `UpgradeStation` (mirror dock/packing trigger pattern, Phase 3 D-01). Player walks into trigger → `Time.timeScale` stays at 1 (no pause; spec doesn't require), but movement is blocked while panel is open via `playerController.InputEnabled = false`. Walking *out* of the trigger closes the panel, OR a Close button.
- **D-10:** **Layout:** vertical `VerticalLayoutGroup` of six `UpgradeRowWidget` instances. Each row:
  - Icon (left)
  - Name + "Lv {current}/{max}" (top)
  - "Next: {effect text}" (middle)
  - Cost button (right) showing "{cost} ¤" with affordability state: green = affordable, grey + disabled = unaffordable, gold = MAX
- **D-11:** **Affordability subscription:** each row subscribes to `ICurrencyService.OnChanged` and `IUpgradeService.OnLevelChanged`. Cost text updates immediately on cash change or level change. No per-frame polling.
- **D-12:** **Hire Worker row** shows "Hired" label after purchase; button disabled. After Phase 6 save lands, persists across restarts.

### Trigger & Composition
- **D-13:** **`UpgradeStation` MonoBehaviour** owns: child trigger collider (`isTrigger=true`), `[SerializeField] UpgradeStationPanel panel`. `OnTriggerEnter` with PlayerController → `panel.Open(); playerController.InputEnabled = false`. `OnTriggerExit` → `panel.Close(); playerController.InputEnabled = true`. Lives in `WM.Stations`.
- **D-14:** **Bootstrap wiring (Phase 5 additions):** instantiate six `IUpgradeEffect` impls injecting their dependencies (`PlayerStats`, `ShelfArea`). Inject `ICurrencyService` + `UpgradeDef[]` + `IUpgradeEffect[]` into `UpgradeManager.Init`. Inject `IUpgradeService` into `UpgradeStationPanel.Init` and `UpgradeStation.Init`. Inject `IUpgradeService` + `ICurrencyService` into each `UpgradeRowWidget`.

### Shelf Area (introduced in Phase 5)
- **D-15:** **`ShelfArea` MonoBehaviour** in `WM.Stations` (uses existing `ShelfArea` GameObject, Phase 1 D-11). Owns:
  - `int Capacity { get; private set; }` — driven by `ShelfCapacityEffect`. Default = `effectCurve[0]` from `Upgrade_shelf_capacity.asset`.
  - List of slot Transforms; visual expansion: extra slot Transforms exist as inactive children, activated as capacity grows.
  - `int CurrentCount { get; }` — Phase 7 worker drops boxes here.
  - `void SetCapacity(int newCapacity)` — updates capacity, activates additional slot Transforms.
  Phase 5 ships ShelfArea component but no drop logic — that lands in Phase 7.

### Asmdef Layout
- **D-16:** **Type homes:**
  - `WM.Upgrades` — `UpgradeDef` SO, `IUpgradeService`, `UpgradeManager`, `IUpgradeEffect` interface + six impls, `UpgradeEffectKind` enum.
  - `WM.Stations` — `UpgradeStation`, `ShelfArea`. References `WM.Upgrades` (for `IUpgradeService`) and `WM.Player` (for `PlayerController.InputEnabled`).
  - `WM.UI` — `UpgradeStationPanel`, `UpgradeRowWidget`. References `WM.Upgrades` + `WM.Economy`.
  - `WM.Player` — `PlayerStats` extended with new fields (D-07); `PlayerController` adds `InputEnabled` toggle.

### Claude's Discretion
- D-02 cost values for `carry_capacity` (20) and `movement_speed` (30) are inferred placeholders if spec §5 row is absent — planner should read spec §5 directly and override.
- D-03 MovementSpeed multiplier table is an inferred placeholder; planner reads spec §6 verbatim.
- D-09 overlay-blocks-input choice over time-pause — spec doesn't specify; pause-while-shopping risks confusing motion-resume. Revisit if playtest disagrees.
- D-12 hire-worker single-purchase representation as `maxLevel=1` rather than a one-shot button — keeps the schema uniform; HireWorkerEffect just sets the bool.
- All five gray areas auto-resolved with recommended defaults.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project Source of Truth
- `CLAUDE.md` — Spec-driven, no scattered SDK calls, ScriptableObjects for tunable data.
- `.planning/PROJECT.md` — Six MVP upgrades only; first worker before first area expansion.
- `.planning/REQUIREMENTS.md` — ECON-02..07 acceptance criteria.
- `.planning/ROADMAP.md` — Phase 5 goal, success criteria, plan stubs (05-01 SO defs, 05-02 UpgradeManager, 05-03 station UI, 05-04 six effect handlers).

### Prior Phase Substrate
- `.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md` — Bootstrap composition (D-07/D-09); `WM.Upgrades` asmdef (D-04); `UpgradeStation` (yellow), `ShelfArea` (brown) GameObjects positioned (D-11); `GameManager` exists (D-08).
- `.planning/phases/02-walk-around-the-warehouse/02-CONTEXT.md` — `PlayerStats` runtime mutation surface (D-16); `PlayerConfig` baselines (D-14/D-15); `PlayerController.InputEnabled` extension point.
- `.planning/phases/03-pick-up-carry-boxes/03-CONTEXT.md` — `PlayerStats.CarryCapacity` already mutable (D-10); station-driven trigger pattern (D-01).
- `.planning/phases/04-complete-an-order/04-CONTEXT.md` — `ICurrencyService` (D-19); `OrderManager` packing-duration consumption (D-08, D-11) — Phase 5 hooks `PlayerStats.PackingDurationMultiplier`; `DeliveryZone` cash award (D-17) — Phase 5 hooks `OrderValueMultiplier`. Phase 4 must include those `PlayerStats` fields from day one.

### Architecture & Spec
- `specs/02-gameplay-systems-spec.md` §9 — Upgrade Station: six MVP upgrades, requirements, acceptance criteria.
- `specs/03-economy-progression-spec.md` §5 — Cost curve [1.0, 2.0, 3.5, 6.0, 10.0]; initial costs.
- `specs/03-economy-progression-spec.md` §6 — Effect curves per upgrade.
- `specs/06-technical-architecture-spec.md` §4 — `UpgradeManager` core system.
- `specs/07-mvp-backlog-acceptance-criteria.md` — WM-012..017 (upgrade station + six upgrades).

### Reference (non-binding)
- `warehouse_master_plan.md` — Original Spanish master plan.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets (paper-locked from prior phases)
- `Bootstrap` composition root — Phase 5 extends with `UpgradeManager` + 6 effect handlers + `UpgradeStationPanel` wiring.
- `ICurrencyService.TrySpend` — used by `UpgradeManager.TryPurchase`.
- Station-driven trigger pattern — `UpgradeStation` reuses for entry detection.
- `PlayerStats` — extended with `OrderValueMultiplier`, `PackingDurationMultiplier`, `WorkerUnlocked`.
- `IActiveOrder.OnPackingStarted` (Phase 4 D-08) — multiplier applied at timer-start.

### Established Patterns
- **MonoBehaviour-on-GameManager** for orchestrators (`OrderManager`, `UpgradeManager`).
- **Plain-C# service via Bootstrap** for stateful infra (`CurrencyManager`).
- **One ScriptableObject per data instance** (six `Upgrade_*.asset` files mirror four `Order_*.asset` files).
- **Per-system asmdef** (Phase 1 D-04).

### Integration Points
- `UpgradeStation` GameObject (yellow placeholder, Phase 1 D-11) — Phase 5 attaches `UpgradeStation` MonoBehaviour + child trigger BoxCollider + serialized panel reference.
- `ShelfArea` GameObject (brown, Phase 1 D-11) — Phase 5 attaches `ShelfArea` MonoBehaviour + slot Transform children. Phase 7 adds drop trigger.
- `GameManager` GameObject — Phase 5 attaches `UpgradeManager` MonoBehaviour.
- `SafeAreaPanel` — Phase 5 adds `UpgradeStationPanel` (overlay, initially inactive).
- `PlayerStats` — Phase 5 mutates new fields.

</code_context>

<specifics>
## Specific Ideas

- **Spec-table costs and effect curves are canonical.** Planner reads `specs/03 §5–§6` directly and populates each `Upgrade_*.asset` from those tables. Code-level placeholders only if spec is silent.
- **`IUpgradeService` is the seam for Phase 9 analytics** (`upgrade_purchased` event). AnalyticsManager subscribes to `OnLevelChanged` from one place — gameplay code never calls analytics directly.
- **Hire Worker = boolean flag, not a stat scalar.** Phase 7 spawns the worker on flag flip; doesn't need integer levels.
- **One effect per impl class** keeps each effect testable in isolation and avoids a switch statement that grows with every new upgrade. Adding a 7th upgrade = new SO + new IUpgradeEffect impl + Bootstrap registration. Zero existing-code edits.

</specifics>

<deferred>
## Deferred Ideas

- **Save persistence for upgrade levels** — Phase 6 (SAVE-01).
- **`upgrade_purchased` analytics event** — Phase 9.
- **Worker spawn from `WorkerUnlocked` flag** — Phase 7 reads on init.
- **Visual feedback on purchase (confetti, sfx)** — Phase 8 (TUT-02).
- **Tutorial step "buy first upgrade"** — Phase 8 (TUT-01).
- **Missing-cash rewarded ad placement at upgrade station** — Phase 10 (ADS-02).
- **More than 5 levels per stat upgrade** — out of MVP (spec §5: "3 to 5 levels").
- **Upgrade tooltips / preview-effect inspector** — out of MVP UI scope.
- **Multiple worker types** — out of MVP (PROJECT.md).
- **Refund / respec mechanic** — out of MVP.
- **Upgrade unlock prerequisites (e.g., must own carry-3 before order-value-2)** — out of MVP; all six available from session start.
- **Animated cost-deduct tween in CashHUD** — Phase 4 D-26 already covers via OnChanged subscription.

</deferred>

---

*Phase: 5-buy-an-upgrade*
*Context gathered: 2026-05-09 (--auto)*
