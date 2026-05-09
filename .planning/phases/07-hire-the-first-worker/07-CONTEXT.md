# Phase 7: Hire the First Worker - Context

**Gathered:** 2026-05-09 (--auto mode)
**Status:** Ready for planning

<domain>
## Phase Boundary

Spawn an automated `Worker` when `PlayerStats.WorkerUnlocked` is true (set by Phase 5's `HireWorkerEffect`, persisted by Phase 6 save). The worker autonomously cycles `loading dock → pickup → shelf area → drop → repeat`, using the existing `LoadingDock` station-driven trigger (Phase 3 D-01) and the `ShelfArea` MonoBehaviour (Phase 5 D-15). Pathing via `NavMeshAgent` on a runtime-baked NavMesh. Worker uses the same `ICarrier` seam (Phase 3 D-04 / Phase 4 D-14 generalization) — no duplicate inventory state.

Covers requirements: **WORK-01**, **WORK-02**.

</domain>

<decisions>
## Implementation Decisions

### Pathing
- **D-01:** **`NavMeshSurface` from Unity AI Navigation package** (`com.unity.ai.navigation`). Single bake at scene load via `surface.BuildNavMesh()` on `Start`. Walls + station box colliders contribute. NavMesh excludes the player and worker themselves (their colliders not marked NavMesh-static). One surface for the whole `Warehouse_MVP` scene.
- **D-02:** **`NavMeshAgent` on Worker.prefab.** Default `radius = 0.4`, `height = 1.8`, `speed` from `WorkerConfig.baseSpeed`. No off-mesh links. Auto-braking on. Stopping distance 0.3 (close enough to dock/shelf to drip-feed boxes).

### Worker AI Architecture
- **D-03:** **Plain C# state machine** in a `WorkerAI` MonoBehaviour. Enum `State { GoingToDock, AtDock, GoingToShelf, AtShelf, Idle }`. `Update` switches on state, drives `NavMeshAgent.SetDestination(...)`, transitions on conditions:
  - `Idle` → `GoingToDock` if dock has boxes AND worker has free capacity.
  - `GoingToDock` → `AtDock` when `agent.remainingDistance < stoppingDistance`.
  - `AtDock` → `GoingToShelf` when `worker.CarrySystem.FreeCapacity == 0` OR dock empty.
  - `GoingToShelf` → `AtShelf` when arrived.
  - `AtShelf` → `Idle` when `worker.CarrySystem.Carried.Count == 0` OR shelf full.
  - `Idle` → re-enter `GoingToDock` on tick.
  Behavior tree rejected — over-engineered for one worker type with five states.
- **D-04:** **Pickup/drop reuse station-driven triggers.** Worker walks into `LoadingDock`'s child trigger (Phase 3 D-01); the dock drip-feeds boxes via the `ICarrier` interface. Worker walks into `ShelfArea`'s child trigger (NEW this phase); shelf drip-feeds boxes off the worker. Zero changes to dock code; minor `ShelfArea` extension.

### `WorkerCarry` (worker's carry implementation)
- **D-05:** **`Worker` GameObject has a `CarrySystem` MonoBehaviour** (the same component class as the player, Phase 3 D-09) — composability across `ICarrier` consumers. Initialized with `WorkerStats.CarryCapacity` (parallel to `PlayerStats.CarryCapacity`). Visual stack on `Worker/CarryAnchor` Transform — same reparenting model (Phase 3 D-11).
- **D-06:** **`WorkerStats` plain C# class** mirrors `PlayerStats` shape. Fields: `MoveSpeed`, `CarryCapacity`. Initialized from `WorkerConfig.baseSpeed` / `baseCarryCapacity`. No upgrade hooks in MVP — worker stats are static.

### Worker Spawn / Lifecycle
- **D-07:** **`WorkerSpawner` MonoBehaviour on `WorkerSpawn` GameObject** (Phase 1 D-11 already placed). On Bootstrap-Init:
  - If `playerStats.WorkerUnlocked == true` (loaded from save), instantiate `Worker.prefab` immediately at `WorkerSpawn` position.
  - Subscribe to `IUpgradeService.OnLevelChanged`; on `("hire_worker", 1)`, instantiate `Worker.prefab` if not already present.
  No despawn — worker stays for session lifetime.
- **D-08:** **`Worker.prefab` composition:** root has `NavMeshAgent` + `WorkerAI` + `CarrySystem` + `WorkerStats` initialization (via small `WorkerInit` MonoBehaviour that calls `Init(WorkerConfig, ShelfArea, LoadingDock)`). Visual: Unity primitive `Capsule` with magenta material (placeholder, distinct from player's color).

### `WorkerConfig` ScriptableObject
- **D-09:** **`WorkerConfig` SO** at `Assets/_Project/ScriptableObjects/Workers/WorkerConfig.asset`:
  - `baseSpeed = 3.0` (slower than player default 5.0).
  - `baseCarryCapacity = 2` (smaller than player base 3).
  - `idleTickIntervalSeconds = 0.5` (how often Idle state re-checks dock for boxes).
  Adding more worker types later = additional SOs + variant prefabs; out of MVP.

### Player-Collision Avoidance
- **D-10:** **Player has `NavMeshObstacle` (carve = false)** so the worker's `NavMeshAgent` avoids the player without triggering NavMesh re-bake. Carve=true would be expensive on every player step.
- **D-11:** **Worker `avoidancePriority = 50`** (default); player has no agent so avoidance is between worker and obstacle (unidirectional — worker yields to player). Spec: "Workers must not block the player excessively." Player can shove past via `CharacterController.Move` regardless.
- **D-12:** **Stuck recovery** — if `agent.velocity.sqrMagnitude < 0.01` for `> 2 seconds` while `state != AtDock && state != AtShelf`, force `agent.ResetPath()` and re-issue destination.

### `ShelfArea` Extension (drop target)
- **D-13:** **`ShelfArea` (Phase 5 D-15) gains `ICarrier`-receive logic this phase.** Child trigger `BoxCollider` (`isTrigger = true`). On `OnTriggerEnter`/`OnTriggerStay` with anything implementing `ICarrier`: shelf drip-feeds boxes off the carrier (via a new `ICarryable TryRemoveAny(out ICarryable item)` API on `CarrySystem` — adds an unfiltered remove that returns whatever's on top of the stack). Boxes reparent to next free shelf slot Transform; collider re-enabled; `_currentCount++`.
- **D-14:** **Shelf full → workers stop dropping.** When `shelf.CurrentCount >= shelf.Capacity`, shelf rejects further drops; worker AI transitions `AtShelf → Idle` and waits. Phase 5 ECON-07 upgrades capacity, freeing space. (Phase 7 doesn't add a "consume from shelf" mechanic — boxes accumulate; if shelf fills with base capacity, player must upgrade to keep automation running.)
- **D-15:** **`CarrySystem.TryRemoveAny`** added now (extends Phase 3 D-09 / Phase 4 D-14 API). Picks first carryable in the list. Used by `ShelfArea`. `TryRemove(BoxType)` (typed, used by packing) and `TryRemovePackage` (Phase 4 D-14) remain unchanged.

### Asmdef Layout
- **D-16:** **Type homes:**
  - `WM.Workers` — `WorkerAI`, `WorkerStats`, `WorkerConfig` SO, `WorkerSpawner`, `WorkerInit`. References `WM.Boxes` (for `CarrySystem`/`ICarrier`/`ICarryable`/`Box`), `WM.Stations` (for `ShelfArea`/`LoadingDock` references), `WM.Upgrades` (for `IUpgradeService`).
  - `WM.Stations` — `ShelfArea` extended with drop logic.
  - `WM.Boxes` — `CarrySystem` extended with `TryRemoveAny`.

### Bootstrap Wiring
- **D-17:** **Bootstrap (Phase 7 additions):**
  - Build NavMesh on `Awake` (via `NavMeshSurface.BuildNavMesh()`).
  - Inject `WorkerConfig`, `LoadingDock`, `ShelfArea`, `IUpgradeService`, `playerStats` into `WorkerSpawner.Init(...)`.
  - WorkerSpawner handles per-worker init when instantiating Worker.prefab.

### Save Persistence
- **D-18:** **Worker unlock already handled by Phase 6** via `SaveDataV1.upgradeLevels.hire_worker` (Phase 6 D-03). On load, Phase 5's `HireWorkerEffect.Apply(level=1)` flips `playerStats.WorkerUnlocked`; Phase 7 `WorkerSpawner.Init` reads it and spawns.

### Claude's Discretion
- D-09 worker speed `3.0` and capacity `2` are placeholders; playtest should retune.
- D-10 NavMeshObstacle vs reciprocal-agent on player — chose obstacle for simplicity; if worker dodging looks weird, swap player to an `NavMeshAgent` with `updatePosition=false` and shared avoidance.
- D-12 stuck-recovery threshold `2s` is a default.
- D-13 unfiltered `TryRemoveAny` chosen over a "shelf-knows-box-types" filter; shelf in MVP doesn't care about types.
- All five gray areas auto-resolved with recommended defaults.

</decisions>

<canonical_refs>
## Canonical References

### Project Source of Truth
- `CLAUDE.md` — small MonoBehaviours; one worker for MVP; no unspecified packages (note: AI Navigation is an official Unity package, allowed).
- `.planning/PROJECT.md` — One worker type only in MVP; first worker before first area expansion.
- `.planning/REQUIREMENTS.md` — WORK-01 (unlock via upgrade, persist), WORK-02 (dock→shelf AI, share box objects, no excessive blocking).
- `.planning/ROADMAP.md` — Phase 7 goal, success criteria, plan stubs (07-01 ShelfArea, 07-02 NavMesh, 07-03 WorkerAI, 07-04 hire-worker spawn).

### Prior Phase Substrate
- `.planning/phases/01-...` — `WorkerSpawn` GameObject placed (D-11); `WM.Workers` asmdef (D-04).
- `.planning/phases/03-...` — `ICarrier` / `CarrySystem` / `BoxType` (D-04, D-09); dock drip-feed pattern (D-01).
- `.planning/phases/04-...` — `ICarryable` generalization (D-14); `TryRemove` API.
- `.planning/phases/05-...` — `ShelfArea` MonoBehaviour and capacity (D-15); `IUpgradeService.OnLevelChanged` (D-04); `playerStats.WorkerUnlocked` (D-07).
- `.planning/phases/06-...` — `SaveDataV1.upgradeLevels.hire_worker` already persisted; `Bootstrap` load-then-init pattern (D-06).

### Architecture & Spec
- `specs/02-gameplay-systems-spec.md` §10 — Worker System: requirements + acceptance criteria.
- `specs/06-technical-architecture-spec.md` §4 — `WorkerAI` core system.
- `specs/07-mvp-backlog-acceptance-criteria.md` — WM-019, WM-020 (worker AI + dock-to-shelf loop).

### Reference (non-binding)
- `warehouse_master_plan.md`.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets (paper-locked)
- `CarrySystem` — same component class powers worker carry; only the `Init(WorkerStats)` differs.
- `ICarrier` / `ICarryable` — worker plugs into station triggers identically to player.
- `LoadingDock`'s drip-feed loop — works for any `ICarrier` (designed for this in Phase 3 D-01/D-04).
- `ShelfArea` MonoBehaviour stub from Phase 5 D-15 — extended with drop logic.

### Established Patterns
- **Per-system asmdef.**
- **Plain-C# state machine over behavior tree** for one-actor-type AI.
- **Bootstrap composition root** — instantiate worker prefab from a service, not via `FindObjectOfType`.
- **`CarrySystem` is reused across actors** — single component, two consumers (player, worker).

### Integration Points
- `WorkerSpawn` GameObject — `WorkerSpawner` MonoBehaviour attached.
- `LoadingDock` — already drip-feeds via `ICarrier`; zero changes.
- `ShelfArea` GameObject — child trigger BoxCollider added; drop logic added on existing `ShelfArea` MonoBehaviour.
- Player GameObject — `NavMeshObstacle` (carve=false) component added.
- `Bootstrap.Awake` — NavMesh build call + `WorkerSpawner` init.
- `CarrySystem` — `TryRemoveAny` API added; existing typed/Package APIs unchanged.

</code_context>

<specifics>
## Specific Ideas

- **NavMesh-bake-at-runtime trades a tiny startup hit for zero-edit-time-binding.** Dropping a new station into the scene doesn't require remembering to bake; `BuildNavMesh()` re-includes it.
- **Worker is dumb on purpose.** No predictive scheduling, no order-aware routing. The bootstrap "warehouse runs itself" feeling lands fastest if the worker's behavior is legible: "go get boxes, put them on shelf."
- **Shelf full = stop is a deliberate friction beat.** Player notices the worker idling, recognizes the bottleneck, and is motivated to upgrade `ShelfCapacity`. This is the upgrade station's emergent loop hook.

</specifics>

<deferred>
## Deferred Ideas

- **Multiple worker types (forklift, packer, dispatcher)** — out of MVP (PROJECT.md).
- **Worker shelf-to-packing-station hand-off** — out of MVP scope; player still does packing manually.
- **Worker upgrade tree (worker speed, worker capacity)** — out of MVP.
- **Worker boost rewarded ad** — Phase 10 (`worker_boost` placement, deferred to soft launch per spec §4).
- **Worker hire animation / spawn fanfare** — Phase 8 feedback polish.
- **Worker pathing visualization in Editor** — Editor tooling, defer.
- **Player-blocks-worker mitigation tuning** — defer to playtest. Current default: worker yields, player ignores.
- **Stuck recovery via repath ping every 2s** — D-12 default; could use a smarter "ask shelf for nearest free slot" approach if simple repath fails.
- **`first_worker_hired` analytics event** — Phase 9.

</deferred>

---

*Phase: 7-hire-the-first-worker*
*Context gathered: 2026-05-09 (--auto)*
