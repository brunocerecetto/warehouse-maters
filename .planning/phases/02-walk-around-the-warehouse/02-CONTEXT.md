# Phase 2: Walk Around the Warehouse - Context

**Gathered:** 2026-05-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Make the player walk. Add a one-thumb on-screen virtual joystick to the existing portrait UI canvas, drive a `PlayerController` with camera-relative movement, hook a Cinemachine virtual camera to follow the player, and ensure walls and station colliders block movement. Speed is data-driven via a `PlayerConfig` ScriptableObject so Phase 5's movement-speed upgrade can mutate it through a runtime stats indirection.

No pickup, no carry rendering, no order UI, no analytics events, no save persistence. Only: input → translation → motion → camera → collision.

Covers requirements: **MOVE-01**, **MOVE-02**.

</domain>

<decisions>
## Implementation Decisions

### Joystick (UI Input)
- **D-01:** Joystick is **fixed bottom-left**, anchored inside the SafeAreaPanel set up in Phase 1 D-15. Predictable position for a tutorial-driven first session and reinforces learnability over the floating-touch-anywhere pattern.
- **D-02:** Joystick is **always visible** from scene load. Removes the discoverability risk for a first-time player before the Phase 8 tutorial lands.
- **D-03:** Joystick is a **custom UGUI MonoBehaviour** in `WM.UI` (e.g., `VirtualJoystick.cs`): `Image` background + `Image` handle, implements `IPointerDownHandler` / `IDragHandler` / `IPointerUpHandler`, exposes `Vector2 Direction { get; }` (normalized, magnitude in [0,1]). No third-party joystick package — matches `CLAUDE.md` "no unspecified third-party packages" rule and avoids Asset Store dependencies.
- **D-04:** Joystick → `PlayerController` connection is **direct property read**: `PlayerController` holds a serialized `[SerializeField] VirtualJoystick joystick` reference and reads `joystick.Direction` each frame in `Update`. No event plumbing for a single consumer.

### Movement Controller
- **D-05:** `PlayerController` uses Unity's built-in **`CharacterController`** component and calls `CharacterController.Move(...)` per frame. Kinematic capsule, manual collision, no physics jitter, clean wall-stop against station box colliders. Worker AI in Phase 7 stays free to use `NavMeshAgent` independently — player and worker do not share a controller.
- **D-06:** Movement direction is **camera-relative via ground-plane projection**:
  ```csharp
  Vector3 fwd   = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
  Vector3 right = Vector3.ProjectOnPlane(camera.transform.right,   Vector3.up).normalized;
  Vector3 dir   = (fwd * joystick.Direction.y + right * joystick.Direction.x);
  ```
  Joystick-up always moves the player toward the top of the screen regardless of camera pitch (~45°).
- **D-07:** **Acceleration model is instant** — `dir * stats.MoveSpeed * dt`. No SmoothDamp, no ramp-up. Snappy, mobile-readable, hybrid-casual standard. Tunable via `PlayerConfig` later if playtest demands weighting.
- **D-08:** Player rotation **faces movement direction with `Quaternion.Slerp`** smoothing toward the velocity vector. Turn rate is configurable via `PlayerConfig.turnRateDegPerSec`. Reads naturally and supports Phase 3's visible carry-stack readability from any angle.
- **D-09:** Gravity is applied minimally — single downward `Physics.gravity.y * dt` term inside `CharacterController.Move` so the capsule stays grounded on the floor plane. No jump, no slope handling beyond what `CharacterController` provides automatically.

### Camera (Framing)
- **D-10:** Camera follow uses the **Cinemachine virtual camera scaffolded in Phase 1 D-12** with `Follow = Player`. Body component is `CinemachinePositionComposer` (Cinemachine 3.x) — equivalent to legacy `Framing Transposer`. X/Y/Z damping ~0.5s for smooth lag without feeling laggy.
- **D-11:** **Orthographic isometric** projection — Main Camera `orthographic = true`, virtual camera lens orthographic, ~45° pitch on the camera rig (carried over from Phase 1 framing). No perspective distortion of stations; stable across iPhone aspect ratios. Initial ortho size tuned in playtest; placeholder ~6.0.
- **D-12:** Player framing offset: **slightly below screen center** — Composer Screen Position Y ≈ 0.55–0.60 (player ~10–20% below center). Frees upper screen for HUD widgets (cash, order UI in later phases) and keeps stations "ahead" of the player visible.
- **D-13:** **Camera bounds via `CinemachineConfiner3D`** with a box collider sized to the warehouse footprint. Prevents the camera from showing void beyond the floor plane. One bounds collider in the scene, hand-positioned around the playable area.

### Speed Configuration & Upgrade Path
- **D-14:** Player tunables live in a **`PlayerConfig` ScriptableObject** at `Assets/_Project/ScriptableObjects/Player/PlayerConfig.asset`. Adds a new `Player/` subfolder under the SO root established in Phase 1 D-05. Single asset for MVP. Matches the "ScriptableObjects for tunable data" rule in `CLAUDE.md`.
- **D-15:** Initial `PlayerConfig` placeholder values: `baseMoveSpeed = 5 m/s`, `turnRateDegPerSec = 720` (half-second full rotation). Tunable in Inspector; final values come from playtesting.
- **D-16:** **Runtime mutation surface = `PlayerStats`** — a plain C# class (or struct) instantiated by `PlayerController` from `PlayerConfig` on `Awake`. Holds `MoveSpeed`, `TurnRateDegPerSec` as mutable runtime properties. Phase 5's movement-speed upgrade will call into `PlayerStats` (e.g., `playerStats.MoveSpeed = config.baseMoveSpeed * (1 + upgradeLevel * pctPerLevel)`). The `PlayerConfig` SO asset stays immutable — never mutated at runtime, never dirtied in Editor between Play sessions.

### Walls & Collision
- **D-17:** Stations placed in Phase 1 (loading dock, packing, delivery, upgrade, shelf area) already have `BoxCollider` components by virtue of being primitives. `CharacterController` collides against them automatically — no extra collision setup beyond confirming each placeholder GameObject has its collider enabled and not marked `isTrigger`.
- **D-18:** Outer warehouse boundary uses **invisible wall colliders** around the floor plane perimeter so the player cannot walk off the floor. Empty `GameObject` with a `BoxCollider`, no renderer, four placed around the playable area. Layer = `Default`.

### Claude's Discretion
- D-09 (gravity model), D-13 (Confiner type — chose 3D over 2D since the warehouse is 3D), D-15 (placeholder speed values — playtest will retune), and D-18 (invisible wall implementation) are Claude defaults; override anytime in planning if a constraint surfaces.
- All decisions converged on the recommended option during discussion; no "you decide" deferrals.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project Source of Truth
- `CLAUDE.md` — Spec-driven development, language=English, small MonoBehaviours, no unspecified packages, ScriptableObjects for tunable data.
- `.planning/PROJECT.md` — Project charter, Active requirements, constraints, key decisions (portrait orientation, visible stack genre signal).
- `.planning/REQUIREMENTS.md` — Phase 2 covers **MOVE-01** (joystick + camera-relative + collision + configurable speed) and **MOVE-02** (fixed isometric/top-down camera, UI not obscuring play area).
- `.planning/ROADMAP.md` — Phase 2 goal, success criteria, plan stubs (02-01 joystick, 02-02 PlayerController, 02-03 collisions).

### Phase 1 Locked Substrate (read before planning)
- `.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md` — Locks Unity 6 LTS + URP mobile-aggressive (D-01/D-02), New Input System (D-03), `WM.Player` and `WM.UI` asmdefs (D-04), `Bootstrap` composition root (D-07), Cinemachine VCam present with no follow target yet (D-12), portrait 1080×1920 SafeAreaPanel canvas (D-15), `Input System UI Input Module` on EventSystem (D-16), placeholder primitives + colored materials (D-13/D-14), SO folder convention under `Assets/_Project/ScriptableObjects/` (D-05).

### Architecture & Spec
- `specs/02-gameplay-systems-spec.md` §2 — Player Movement System requirements and acceptance criteria (joystick, camera-relative, collision, configurable + upgradeable speed).
- `specs/02-gameplay-systems-spec.md` §3 — Carry System (Phase 3 — informs that PlayerController must remain decoupled from carry logic).
- `specs/06-technical-architecture-spec.md` §3 — Folder structure (where `Player/` SO folder belongs).
- `specs/06-technical-architecture-spec.md` §4 — Core systems list (`PlayerController` lives in `WM.Player`).
- `specs/01-product-requirements-spec.md` — Target session shape (first order < 60s) — informs movement should feel snappy, not weighty.
- `specs/07-mvp-backlog-acceptance-criteria.md` — MOVE-01 / MOVE-02 acceptance criteria source.

### Reference (non-binding)
- `warehouse_master_plan.md` — Original Spanish master plan; reference for feel/intent only. English specs above are authoritative.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **None yet.** Phase 1 plans not yet executed at the time this context was gathered — repo contains only `CLAUDE.md`, `specs/`, `.planning/`, `warehouse_master_plan.md`. Phase 2 starts after Phase 1 lands the Unity skeleton.

### Established Patterns
- **Phase 1 substrate (paper-locked, not yet code):** `WM.Player` asmdef will host `PlayerController` and `PlayerStats`; `WM.UI` asmdef will host `VirtualJoystick`. `Bootstrap` composition root (Phase 1 D-07) injects services via explicit `Init(...)` — Phase 2 does not need any service yet, but `PlayerController` must remain compatible with that pattern (no singleton lookups, no static state).
- **ScriptableObject convention** (Phase 1 D-05): per-system SO subfolders. Adding `Player/` is consistent.

### Integration Points
- `Player` GameObject already positioned in the `Warehouse_MVP` scene by Phase 1 D-11 — Phase 2 attaches `CharacterController` + `PlayerController` + visual placeholder primitive to it.
- `CameraRig` GameObject (Phase 1 D-12) already has the Cinemachine VCam — Phase 2 wires `Follow = Player`, swaps Body to `CinemachinePositionComposer`, sets composition values, and adds `CinemachineConfiner3D` with a bounds collider.
- `UICanvas` (Phase 1 D-15, Screen Space - Camera, 1080×1920 portrait, SafeAreaPanel) — Phase 2 adds the joystick UI as a child of SafeAreaPanel anchored bottom-left.
- `EventSystem` (Phase 1 D-16) already uses `Input System UI Input Module` — joystick `IPointer*` handlers fire correctly out of the box.

</code_context>

<specifics>
## Specific Ideas

- **One-thumb portrait ergonomics drive joystick placement (D-01).** The PROJECT.md "portrait orientation" + "visible stacked carry on character" combo means the lower-left thumb zone must be reliable; the rest of the screen is for reading carry stack and station progress.
- **Hybrid-casual feel** — instant acceleration (D-07) and snappy turn rate (D-08, D-15) are deliberate to keep the 5-minute loop feel "satisfying" per Core Value, not "weighty".
- **Phase 5 forward-compatibility is the reason for `PlayerStats` (D-16).** Discussion explicitly anticipated the movement-speed upgrade path so Phase 5 doesn't need to refactor `PlayerController`.
- **Cinemachine 3.x `CinemachinePositionComposer`** is the modern equivalent of legacy `Framing Transposer`. If the project is on an older Cinemachine version, the planner can substitute `CinemachineFramingTransposer` — the framing math is identical.

</specifics>

<deferred>
## Deferred Ideas

- **Editor keyboard fallback (WASD / arrow keys) for non-device testing** — not in scope for Phase 2. Add a thin `EditorInputAdapter` later if device-only testing slows iteration.
- **NavMesh bake** — defer to Phase 7 (worker AI). Phase 2 player uses `CharacterController`, no NavMesh needed.
- **Joystick dead-zone tuning, knob travel cap, snap-back animation** — implementation details for the planner. Defaults: 10% dead zone, full travel mapped to magnitude 1.0, no animation. Revisit in Phase 8 (Tutorial & Feel) if joystick feel needs polish.
- **Quality tier camera ortho size / pitch variants for different iPhone aspect ratios** — single ortho size for MVP. Add aspect-ratio handling if testing on iPad / non-iPhone-15 devices reveals framing problems.
- **Lookahead camera offset toward joystick direction** — considered, deferred. Add only if static framing feels stale during playtest.
- **Player visual prefab beyond placeholder primitive** — outside MVP art scope per PROJECT.md "validate gameplay before art investment".

</deferred>

---

*Phase: 2-walk-around-the-warehouse*
*Context gathered: 2026-05-09*
