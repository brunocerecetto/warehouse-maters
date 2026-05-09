# Phase 3: Pick Up & Carry Boxes - Context

**Gathered:** 2026-05-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Boxes spawn at the loading dock and the player auto-picks them up to a capped carry capacity, rendered as a visible stack on the player's back. Three box types (red, blue, yellow) defined as ScriptableObjects. Loading dock has configurable spawn rate, type distribution, and dock capacity ceiling. Pickup is via a trigger zone on the dock that drip-feeds boxes into anyone implementing `ICarrier`.

No order matching, no packing station accept logic, no delivery, no cash, no analytics events, no save persistence. Phase 4 consumes the `ICarrier` and box-on-player visuals to wire packing + delivery.

Covers requirements: **BOX-01**, **BOX-02**, **BOX-04**.

</domain>

<decisions>
## Implementation Decisions

### Pickup Detection
- **D-01:** Pickup is **driven by the dock**, not the player. `LoadingDock` has a child trigger `BoxCollider` (`isTrigger = true`). On `OnTriggerEnter`/`OnTriggerStay` with anything implementing `ICarrier`, the dock drives the pickup loop. Logic stays on the dock; works identically for the Phase 7 worker.
- **D-02:** **Drip-feed pickup tempo** — while the carrier overlaps the trigger AND has free capacity AND the dock has boxes, the dock transfers one box every `pickupIntervalSeconds`. Default `0.15s`. Tunable on `LoadingDockConfig` SO. Hybrid-casual tactile rhythm.
- **D-03:** **FIFO** box selection on the dock. Internal `Queue<Box>` (or ordered slot scan). Oldest spawn goes first. Predictable, easy to reason about.
- **D-04:** **`ICarrier` interface in `WM.Boxes`**:
  ```csharp
  public interface ICarrier {
      int FreeCapacity { get; }
      bool TryAccept(Box box); // returns false if at capacity or rejected
  }
  ```
  `PlayerController` exposes `ICarrier` by delegating to its `CarrySystem`. Phase 7 worker implements the same interface. Dock pushes via interface — zero coupling between dock and concrete carriers.

### Box Data & Prefabs
- **D-05:** **One shared `Box.prefab`** under `Assets/_Project/Prefabs/Boxes/Box.prefab`. Contains `Box` MonoBehaviour + `MeshRenderer` + `BoxCollider`. On instantiation, the spawner calls `box.Init(BoxType so)` which assigns `Type` and swaps the renderer's material to `BoxType.material`. Adding a 4th type = create new SO + material, no new prefab.
- **D-06:** **`BoxType` ScriptableObject schema** (locked):
  ```csharp
  [CreateAssetMenu(menuName = "WM/Boxes/BoxType")]
  public class BoxType : ScriptableObject {
      public string id;            // e.g., "red", "blue", "yellow"
      public string displayName;   // for Phase 4 order UI
      public Color  color;         // for icons / UI swatches
      public Material material;    // URP/Lit asset; assigned to MeshRenderer at Init
      public int    baseValue;     // Phase 4 cash reward seed
  }
  ```
  Three SO assets created this phase: `Box_Red.asset`, `Box_Blue.asset`, `Box_Yellow.asset` under `Assets/_Project/ScriptableObjects/Boxes/`.
- **D-07:** **Box runtime identity** — `Box` MonoBehaviour exposes `BoxType Type { get; }` referencing the SO directly. Phase 4 packing station compares via `box.Type == requiredType`. No string lookups, no enum.
- **D-08:** **Asset locations**:
  - SOs: `Assets/_Project/ScriptableObjects/Boxes/Box_Red.asset`, `Box_Blue.asset`, `Box_Yellow.asset`
  - Materials: `Assets/_Project/Materials/Boxes/Box_Red.mat`, `Box_Blue.mat`, `Box_Yellow.mat` (URP/Lit, base color matches SO `color`)
  - Prefab: `Assets/_Project/Prefabs/Boxes/Box.prefab`
  - Loading dock config: `Assets/_Project/ScriptableObjects/Stations/LoadingDockConfig.asset` (new `Stations/` SO subfolder)

### Carry System
- **D-09:** **Dedicated `CarrySystem` MonoBehaviour** on the Player GameObject (assembly: `WM.Boxes` for tight cohesion with `Box`/`ICarrier`; PlayerController in `WM.Player` references it). Owns `List<Box> carried`, implements `ICarrier`, exposes:
  ```csharp
  public IReadOnlyList<Box> Carried { get; }
  public int  FreeCapacity { get; }                 // = playerStats.CarryCapacity - Carried.Count
  public bool TryAccept(Box box);                   // adds to list, reparents to CarryAnchor, repositions stack
  public bool TryRemove(BoxType type, out Box box); // for Phase 4 packing station drop API
  public event Action OnStackChanged;               // for UI / feedback hooks
  ```
- **D-10:** **Carry capacity sourced from `PlayerStats.CarryCapacity`**, mirroring Phase 2 D-16. `PlayerConfig` SO gains `baseCarryCapacity = 3` (per spec §3). `PlayerStats` exposes mutable `CarryCapacity` (initialized from config). Phase 5 ECON-03 upgrade writes to `PlayerStats.CarryCapacity`. Same pattern as movement speed — no parallel CarryConfig.
- **D-11:** **Visible stack via reparenting the actual Box GameObject**. On `TryAccept`: dock removes box from its slot Transform, calls `box.transform.SetParent(carryAnchor)`, `CarrySystem` positions it at the next stack offset. On `TryRemove` / drop (Phase 4): unparent, place at station. No instantiate/destroy. Single Box instance flows dock → player → station. Box's collider is disabled while carried (so player movement doesn't shove it).
- **D-12:** **CarryAnchor positioning** — empty Transform parented to player at back/upper-torso (~y=1.0, z=−0.3 on a 1m placeholder capsule, tunable). Stack grows vertically with `boxHeight + spacing` per box. `boxHeight` derived from box prefab bounds; `spacing` configurable on `PlayerConfig` (`carryStackSpacing = 0.05f`).

### Loading Dock Spawner
- **D-13:** **Fixed-interval timer** spawning. `BoxSpawner` MonoBehaviour on `LoadingDock` accumulates `Time.deltaTime`; every `spawnIntervalSeconds` it spawns one box if `boxesOnDock < dockCapacity`. Predictable, designer-friendly.
- **D-14:** **Weighted type distribution** via `LoadingDockConfig.typeWeights : List<TypeWeight>` where `TypeWeight { BoxType type; float weight }`. Spawner picks via cumulative-weight random. Default equal weights (1/1/1 for red/blue/yellow). Adding a rare type later = add an entry with `weight = 0.1`.
- **D-15:** **Pre-defined slot Transforms on dock**, named `Slot_00`...`Slot_NN`, laid out in a grid on the dock's top surface. Spawner places the new box at the first free slot. On pickup, the dock clears that slot. Capacity is the slot count. Matches FIFO selection (D-03) — first slot to be vacated is the next slot to be refilled.
- **D-16:** **Initial `LoadingDockConfig` values** (placeholders, retunable):
  - `spawnIntervalSeconds = 1.0`
  - `dockCapacity = 8` (4×2 grid of slot Transforms on the dock)
  - `typeWeights = [ (Box_Red, 1), (Box_Blue, 1), (Box_Yellow, 1) ]`
  - `pickupIntervalSeconds = 0.15`

### Claude's Discretion
- D-02 default `0.15s` pickup tempo, D-12 CarryAnchor coordinates, D-16 initial dock values are placeholder defaults — playtest will retune.
- Box collider disabled while carried (D-11) — Claude default to avoid `CharacterController` shoving carried boxes; planner can revisit if a feature needs box collisions while carried.
- All four areas converged on the recommended option; no "you decide" deferrals.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project Source of Truth
- `CLAUDE.md` — Spec-driven development, ScriptableObjects for tunable data, no unspecified third-party packages, small MonoBehaviours.
- `.planning/PROJECT.md` — Constraint: visible stacked carry on character is iconic to genre. Three box types only in MVP. ScriptableObjects for tunable data.
- `.planning/REQUIREMENTS.md` — Phase 3 covers **BOX-01** (data-driven box SOs, future-extensible), **BOX-02** (auto-pickup, capped capacity, visible stack), **BOX-04** (loading dock spawner: rate, distribution, capacity ceiling).
- `.planning/ROADMAP.md` — Phase 3 goal, success criteria, plan stubs (03-01 SO defs, 03-02 BoxSpawner, 03-03 CarrySystem).

### Prior Phase Substrate (read before planning)
- `.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md` — `WM.Boxes` asmdef (D-04), `Assets/_Project/ScriptableObjects/Boxes/` folder created empty (D-05), `LoadingDock` GameObject placed in scene with grey placeholder material (D-11/D-13), Bootstrap composition root pattern (D-07), Cinemachine VCam follows player (Phase 2 D-10). New `Stations/` SO subfolder added in Phase 3 D-08.
- `.planning/phases/02-walk-around-the-warehouse/02-CONTEXT.md` — `PlayerController` in `WM.Player` (D-05), `PlayerStats` runtime mutation surface (D-16) — Phase 3 extends `PlayerConfig`/`PlayerStats` with `CarryCapacity` and `carryStackSpacing` rather than introducing a parallel CarryConfig.

### Architecture & Spec
- `specs/02-gameplay-systems-spec.md` §3 (Carry) — capacity 3, configurable, upgradeable, visible stack updates immediately.
- `specs/02-gameplay-systems-spec.md` §4 (Box) — type identifier, visual matches type, data-driven, supports future types without rewriting order logic.
- `specs/02-gameplay-systems-spec.md` §5 (Loading Dock) — spawns over time, max visible capacity, configurable rate + types, both player and worker collect.
- `specs/06-technical-architecture-spec.md` §3 — folder structure (Prefabs/, Materials/, ScriptableObjects/ subfolders).
- `specs/06-technical-architecture-spec.md` §4 — Core systems list.
- `specs/07-mvp-backlog-acceptance-criteria.md` — BOX-01 / BOX-02 / BOX-04 acceptance criteria.

### Reference (non-binding)
- `warehouse_master_plan.md` — Original Spanish master plan; reference for feel only.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **None in code yet.** Phase 1 + Phase 2 plans not executed at the time of this discussion. The substrate is paper-locked via prior CONTEXT.md files.

### Established Patterns (paper-locked)
- **`PlayerConfig` SO + `PlayerStats` runtime mutation surface** (Phase 2 D-14, D-16) — Phase 3 extends both rather than creating a parallel `CarryConfig`/`CarryStats` pair, keeping the upgrade-binding surface uniform across MOVE-01/MOVE-02 and BOX-02/ECON-03.
- **`Bootstrap` composition root** (Phase 1 D-07) — `BoxSpawner` and `CarrySystem` are plain MonoBehaviours; if either needs services later (analytics in Phase 9, save in Phase 6), `Bootstrap` injects via `Init(...)`. No singletons.
- **Per-system asmdef** (Phase 1 D-04) — `BoxType`, `Box`, `BoxSpawner`, `LoadingDockConfig`, `CarrySystem`, `ICarrier` live in `WM.Boxes`. `WM.Player` references `WM.Boxes` for the `CarrySystem` MonoBehaviour reference. `LoadingDockConfig` and slot Transforms attach to the existing `LoadingDock` GameObject in `Warehouse_MVP.unity`.

### Integration Points
- `LoadingDock` GameObject (Phase 1 D-11) — Phase 3 attaches `BoxSpawner` + `LoadingDockConfig` reference + `Slot_00`...`Slot_07` child Transforms + a child trigger `BoxCollider` (`isTrigger = true`).
- `Player` GameObject (Phase 1 D-11, Phase 2 D-05) — Phase 3 attaches `CarrySystem` MonoBehaviour + child `CarryAnchor` empty Transform. `PlayerController` adds a serialized `[SerializeField] CarrySystem carry` reference and exposes the `ICarrier` to outside callers via `carry`.
- `PlayerConfig` SO (Phase 2) — Phase 3 adds `baseCarryCapacity` (int = 3) and `carryStackSpacing` (float = 0.05).
- `PlayerStats` (Phase 2) — Phase 3 adds mutable `int CarryCapacity { get; set; }` initialized from `PlayerConfig.baseCarryCapacity`.

</code_context>

<specifics>
## Specific Ideas

- **Visible stacked carry is iconic to the genre** (PROJECT.md key decision) — D-11 reparents the actual Box GameObject so the same MeshRenderer that read on the dock is the one read on the player's back. No proxy meshes, no art parity bugs.
- **Pickup tempo of 0.15s** lands ~6.7 boxes/sec — at base carry 3, the entire stack fills in <0.5s, which keeps the loop "satisfying" per Core Value without invisibly-fast pickup.
- **`ICarrier` is the seam for Phase 7.** The worker AI plugs into the same dock without a single edit to `BoxSpawner` or `LoadingDock`.
- **Slot-based dock visual** maps directly to the spec's "max visible capacity" — players see exactly how full the dock is, which makes the upgrade-station "shelf capacity" upgrade later (Phase 5 ECON-07) read as a meaningful expansion.

</specifics>

<deferred>
## Deferred Ideas

- **Box pickup feedback (audio, particle, scale-pop)** — Phase 8 Tutorial & Feel + TUT-02. Phase 3 leaves `OnStackChanged` event open as the hook.
- **Dock-full visual feedback** (e.g., dock icon flashes red, idle animation) — Phase 8.
- **Multiple loading docks / second dock unlock** — out of MVP scope per PROJECT.md.
- **Special box types (fragile, frozen, heavy, gold)** — explicitly out of MVP per PROJECT.md / REQUIREMENTS.md v2.
- **Box pooling instead of instantiate/destroy** — Phase 3 destroys boxes when consumed at packing station (Phase 4). If profiling reveals GC pressure, add an `ObjectPool<Box>` later.
- **Stack jiggle / IK on player back during walk** — feel polish; defer to Phase 8.
- **Box collider re-enabled while carried** — Claude default disables it while carried (D-11). If a future feature needs box-on-box or box-on-station collisions during carry, revisit.
- **Drop-while-walking (player-initiated drop)** — not a MVP requirement. Drop only happens at stations via Phase 4's `TryRemove` API.
- **Pickup ordering preference (closest, type-balanced)** — FIFO chosen (D-03). Revisit only if playtest shows it's confusing.

</deferred>

---

*Phase: 3-pick-up-carry-boxes*
*Context gathered: 2026-05-09*
