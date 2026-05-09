# Phase 1: Project Bootstrap & Empty Warehouse Scene - Context

**Gathered:** 2026-05-09
**Status:** Ready for planning

<domain>
## Phase Boundary

Stand up the Unity iOS project skeleton and a placeholder `Warehouse_MVP` scene. On completion, opening the project in Unity Editor loads the scene with all required GameObjects (player spawn, loading dock, packing station, delivery zone, upgrade station, shelf area, worker spawn, camera rig, UI canvas, EventSystem, GameManager) positioned in placeholder geometry, and a debug iOS build runs on simulator/device showing the empty warehouse from the configured camera. No gameplay logic, no input handling beyond input-system bootstrap, no save/analytics/ads beyond interface stubs.

Covers requirements: **BOOT-01**, **BOOT-02**.

</domain>

<decisions>
## Implementation Decisions

### Engine & Render Pipeline
- **D-01:** Unity 6 LTS (6000.x). Pin via `ProjectSettings/ProjectVersion.txt` after init. Recommended for new iOS projects in 2026 — RenderGraph URP, GPU Resident Drawer, better mobile perf headroom than 2022.3 LTS.
- **D-02:** URP **Mobile-aggressive** quality preset: MSAA off, no realtime shadows (baked only), HDR off, no post-FX. Targets 60fps on iPhone 8/SE class. Single URP asset for MVP — quality tiers can be added if/when older device perf becomes a concern.

### Input
- **D-03:** New Input System package (`com.unity.inputsystem`). Player Settings → Active Input Handling = `Input System Package (New)`. Decided here because Phase 2 joystick implementation inherits this choice.

### Project Structure & Asmdefs
- **D-04:** One asmdef per system folder. Concrete names:
  - `WM.Core` (Assets/_Project/Scripts/Core)
  - `WM.Player` (Assets/_Project/Scripts/Player)
  - `WM.Boxes` (Assets/_Project/Scripts/Boxes)
  - `WM.Orders` (Assets/_Project/Scripts/Orders)
  - `WM.Stations` (Assets/_Project/Scripts/Stations)
  - `WM.Upgrades` (Assets/_Project/Scripts/Upgrades)
  - `WM.Workers` (Assets/_Project/Scripts/Workers)
  - `WM.Economy` (Assets/_Project/Scripts/Economy)
  - `WM.Save` (Assets/_Project/Scripts/Save)
  - `WM.Analytics` (Assets/_Project/Scripts/Analytics)
  - `WM.Monetization` (Assets/_Project/Scripts/Monetization)
  - `WM.UI` (Assets/_Project/Scripts/UI)
  - `WM.Editor` (Assets/_Project/Scripts/Editor) — editor-only, includes `BuildScript.cs`
- **D-05:** Folder layout follows `specs/06-technical-architecture-spec.md` §3 verbatim under `Assets/_Project/`. ScriptableObject folders (`ScriptableObjects/Boxes`, `Orders`, `Upgrades`, `Workers`, `Economy`) created empty in this phase.
- **D-06:** Test asmdefs scaffolded from day one: `WM.Tests.EditMode` and `WM.Tests.PlayMode`. Unity Test Framework package installed. Each contains a single passing smoke test so CLI test runs return success immediately.

### Service Composition / Bootstrap
- **D-07:** Plain C# composition root pattern (`Bootstrap.cs` MonoBehaviour in `WM.Core`). On `Awake` it news up plain-C# services behind their interfaces (`IAnalyticsService`, `IAdService`, `IIapService`, `ISaveService`) using mock/stub implementations and injects them into MonoBehaviour consumers via explicit `Init(...)` calls. No singletons, no third-party DI container, no `ScriptableObject` service locator.
- **D-08:** `GameManager` MonoBehaviour exists in the scene as a thin orchestrator. In Phase 1 it only logs "GameManager initialized" on `Start` — no gameplay state machine yet. It receives services from `Bootstrap` via `Init(...)`.
- **D-09:** Service interfaces (`IAnalyticsService`, `IAdService`, `IIapService`, `ISaveService`) are defined in this phase under `WM.Core` with **no-op stub implementations**. Real implementations are added in their respective phases (Save in 6, Analytics in 9, Ads/Monetization in 10).

### Scene Composition
- **D-10:** Single root scene `Assets/_Project/Scenes/Warehouse_MVP.unity`. No additive scenes for MVP.
- **D-11:** Required GameObjects in scene (positioned, not yet wired): `Player`, `LoadingDock`, `PackingStation`, `DeliveryZone`, `UpgradeStation`, `ShelfArea`, `WorkerSpawn`, `CameraRig`, `UICanvas`, `EventSystem`, `GameManager`, `Bootstrap`. Each station marked with a placeholder colored material so they're visually distinguishable in-Editor.
- **D-12:** Camera rig uses **Cinemachine virtual camera** (free package). Fixed isometric framing, ~45° pitch, no follow target in this phase. Player follow added in Phase 2.

### Placeholder Geometry
- **D-13:** Unity primitives + colored materials only — no ProBuilder, no Asset Store packs. Floor: scaled `Plane`. Stations: `Cube` or `Cylinder` with distinct color materials (loading dock = grey, packing = blue, delivery = green, upgrade = yellow, shelf = brown, worker spawn = magenta marker).
- **D-14:** Placeholder materials live in `Assets/_Project/Materials/Placeholder/`. URP/Lit shader, single base color each.

### UI Canvas
- **D-15:** UI canvas: **Screen Space - Camera** mode, reference resolution **1080×1920** (portrait), match-mode `Match Width Or Height = 0.5`. Safe-area aware via a `SafeAreaPanel` component (anchors adjust to `Screen.safeArea`) so iPhone notch/Dynamic Island don't clip UI. Empty in this phase — no widgets yet.
- **D-16:** EventSystem uses `Input System UI Input Module` (paired with the New Input System decision).

### Build & Player Settings
- **D-17:** Player Settings: portrait orientation only (auto-rotation disabled), iOS bundle identifier placeholder `com.warehousemaster.mvp` (final id picked at TestFlight time, Phase 11), minimum iOS version **15.0**, Metal API only, IL2CPP scripting backend, ARM64 architecture.
- **D-18:** `BuildScript.cs` skeleton lives in `WM.Editor` from day one. Single `BuildIOS` static method that calls `BuildPipeline.BuildPlayer` with iOS target into `build/ios/`. Phase 11 fleshes out signing/archive — Phase 1 only ensures the entry point compiles and produces an Xcode project.

### Repo Hygiene
- **D-19:** Add a Unity-flavored `.gitignore` at repo root (covers `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`, `.vs/`, `*.csproj`, `*.sln`, etc.). Keep `Packages/manifest.json` and all `.meta` files tracked.
- **D-20:** Git LFS configured for binary asset extensions (`*.psd`, `*.fbx`, `*.png`, `*.jpg`, `*.wav`, `*.mp3`, `*.ogg`, `*.unity`, `*.asset`, `*.prefab`). `.gitattributes` committed.

### Claude's Discretion
- **D-07 (composition root)** was a "you decide" — Claude picked Plain C# composition root over Singleton, SO service locator, and VContainer. Rationale: matches `specs/06-technical-architecture-spec.md` ("small MonoBehaviours + plain C# services"), matches `CLAUDE.md` warning against unspecified third-party packages, keeps services testable without a DI framework, and avoids hidden global state of singletons.
- Cinemachine inclusion (D-12), placeholder material palette (D-13), UI scaling rules (D-15), iOS minimum version (D-17), `.gitignore` / LFS rule set (D-19, D-20) — Claude's defaults, confirmed by user during the deferred-items confirmation step. Override anytime in planning if a constraint surfaces.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project Source of Truth
- `CLAUDE.md` — Project-wide rules: spec-driven development, language=English, small MonoBehaviours, no unspecified packages, MVP scope guardrails.
- `.planning/PROJECT.md` — Project charter, Active requirements, constraints, key decisions.
- `.planning/REQUIREMENTS.md` — Full v1 requirements list and traceability map. Phase 1 covers BOOT-01, BOOT-02.
- `.planning/ROADMAP.md` — Phase 1 goal, success criteria, plan stubs (01-01 init, 01-02 scene, 01-03 camera+UI).

### Architecture & Spec
- `specs/06-technical-architecture-spec.md` §3 — Folder structure (matches D-04, D-05).
- `specs/06-technical-architecture-spec.md` §4 — Core systems list (`GameManager`, `Bootstrap`, services). Phase 1 stubs/scaffolds these; full implementations come in their owning phases.
- `specs/06-technical-architecture-spec.md` §2 — Tech stack (Unity, C#, iOS, URP, local save, fake ads/IAP for MVP).
- `specs/01-product-requirements-spec.md` — Product vision, target session shape, monetization stance.
- `specs/02-gameplay-systems-spec.md` — Reference for downstream phases; Phase 1 only positions the GameObjects these systems will attach to.
- `specs/07-mvp-backlog-acceptance-criteria.md` — Acceptance criteria for BOOT-01 / BOOT-02.

### Reference (non-binding)
- `warehouse_master_plan.md` — Original Spanish master plan. Reference for feel/intent only; English specs above are authoritative.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **None.** Pre-Unity-init repo. Repo at workflow start contains only `CLAUDE.md`, `specs/`, `warehouse_master_plan.md`, and `.planning/`. No `Assets/`, no `ProjectSettings/`, no `Packages/manifest.json`.

### Established Patterns
- **Spec-driven development** (`CLAUDE.md`): read specs → propose minimal scope → implement → validate. Phase 1 establishes the codebase that all later patterns will build on, so the bootstrap output IS the precedent.

### Integration Points
- **None to integrate with — this phase IS the integration substrate.** Every later phase's first action will be "drop new MonoBehaviours into the right `WM.*` asmdef and reference services from `Bootstrap`."

</code_context>

<specifics>
## Specific Ideas

- iOS-first portrait orientation. iPhone is the target form factor. Portrait orientation locked.
- Visible stacked carry on player back is iconic to the genre — reinforces why CameraRig framing must keep player upper body visible from the configured pitch (D-12).
- Hybrid-casual perf budget: 60fps on iPhone 8/SE class — drives D-02 (mobile-aggressive URP preset) and D-17 (IL2CPP, ARM64, Metal-only).

</specifics>

<deferred>
## Deferred Ideas

- **Quality tier variants (low/mid/high URP profiles)** — Phase 1 ships single mobile-aggressive preset. If later device-class testing shows perf headroom on newer iPhones, add tiers in a polish phase.
- **Cinemachine follow target** — Player-follow framing happens in Phase 2 alongside joystick movement.
- **Real bundle identifier + signing config** — Phase 11 (TestFlight build pipeline). Phase 1 uses placeholder `com.warehousemaster.mvp`.
- **Localization scaffolding** — Not in MVP scope; English-only per `CLAUDE.md`.
- **`Resources/` vs Addressables for prefab loading** — No runtime prefab loading needed in Phase 1. Decide when first system needs it (likely Phase 3 box prefabs — direct Inspector references should suffice).

</deferred>

---

*Phase: 1-project-bootstrap-empty-warehouse-scene*
*Context gathered: 2026-05-09*
