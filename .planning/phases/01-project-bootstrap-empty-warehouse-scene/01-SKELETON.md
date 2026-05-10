# Walking Skeleton — Warehouse Master

**Phase:** 1
**Generated:** 2026-05-10

## Capability Proven End-to-End

> One sentence: the smallest user-visible capability that exercises the full stack.

A developer can open the Unity 6.3.7f1 project, press Play in the Editor to load the `Warehouse_MVP` scene with the full set of placeholder GameObjects framed by a Cinemachine isometric camera, and run `Unity -batchmode -quit -buildTarget iOS -executeMethod WM.Editor.BuildScript.BuildIOS` to produce a buildable iOS Xcode project at `build/ios/Unity-iPhone.xcodeproj` — with green EditMode and PlayMode test runs proving the project compiles, the scene wires the required objects, and the scripting backend is intact.

## Architectural Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Engine | Unity 6.3.7f1 (Unity 6 LTS, RenderGraph URP) | D-01 — modern URP, GPU Resident Drawer, mobile perf headroom; LTS through Dec 2027 |
| Render pipeline | Universal Render Pipeline 17.x mobile-aggressive (MSAA off, no realtime shadows, HDR off, no post-FX) | D-02 — 60fps target on iPhone 8/SE class |
| Input system | New Input System 1.15.0 + `InputSystemUIInputModule` | D-03 — required for Phase 2 joystick; legacy Input Manager off |
| Camera | Cinemachine 3.1.6 (`CinemachineCamera` + `CinemachineBrain`), no follow target in P1, Lens Mode = Perspective, FOV = 40 (locked) | D-12 — Phase 2 will add follow without re-wiring; FOV pinned for follow-damping determinism |
| Scripting backend | IL2CPP, ARM64, Metal-only, iOS 15.0 minimum | D-17 — App Store-grade defaults |
| Composition root | Plain C# `Bootstrap` MonoBehaviour news up `I*Service` stubs and injects via `Init(...)` calls | D-07 — no singletons, no DI container, fully testable |
| Asmdef topology | 13 production asmdefs (`WM.Core` root, system-per-folder) + 2 test asmdefs (`WM.Tests.EditMode`, `WM.Tests.PlayMode`) — all unidirectional, rooted at `WM.Core` | D-04, D-06 — matches `specs/06-technical-architecture-spec.md` §3, prevents cyclic refs |
| Scene strategy | Single scene `Assets/_Project/Scenes/Warehouse_MVP.unity` with 12 required GameObjects | D-10, D-11 — no additive scenes for MVP |
| UI stack | Screen-Space Camera Canvas at 1080×1920 portrait + `SafeAreaPanel` script (Apply() resets offsetMin/offsetMax to Vector2.zero so rect == Screen.safeArea is self-enforcing) | D-15 — iPhone notch / Dynamic Island aware |
| Build tooling | `WM.Editor.BuildScript.BuildIOS` calls `BuildPipeline.BuildPlayer`, calls `EditorApplication.Exit(1)` on failure | D-18 + RESEARCH Pitfall 1 — Unity does not propagate build failure to CLI exit code by default |
| Tests | Unity Test Framework 2.0.x — EditMode structure + scene smoke + PlayMode runtime smoke from day one | D-06 + VALIDATION.md Wave 0 — prevents blind regressions |
| Repo hygiene | GitHub Unity `.gitignore` + `.gitattributes` with **D-20 deviation approved by user 2026-05-10 (RESEARCH OQ-1 RESOLVED)**: keep `.unity`, `.prefab`, generic `.asset` as `merge=unityyamlmerge` text (not LFS); LFS only true binaries (`.psd`, `.fbx`, `.png`, `.jpg`, `.wav`, `.mp3`, `.ogg`, `LightingData.asset`, `*TerrainData.asset`, `NavMeshData.asset`) | RESEARCH Open Question 1 RESOLVED — LFS-tracked YAML breaks `unityyamlmerge` and removes PR diffs; community best practice |
| Folder layout | `Assets/_Project/{Art,Audio,Materials,Prefabs,Scenes,Scripts,ScriptableObjects,Settings}` per architecture spec §3 verbatim; tests live at `Assets/Tests/{EditMode,PlayMode}` to keep test code visually separate from production | D-05 + RESEARCH §Project Structure |

## Stack Touched in Phase 1

- [x] **Project scaffold** — Unity 6.3.7f1 project, URP_Mobile asset configured, manifest packages pinned
- [x] **Routing** (Unity equivalent: scene graph) — `Warehouse_MVP.unity` is the only scene; in `EditorBuildSettings.scenes` and explicitly passed in `BuildPlayerOptions.scenes`
- [x] **Database** (Unity equivalent: persistence boundary) — `ISaveService` interface defined in `WM.Core` with `NullSaveService` stub; real persistence wired in Phase 6
- [x] **UI** — Screen-Space Camera Canvas + `SafeAreaPanel`, EventSystem on `InputSystemUIInputModule` (no widgets yet — wiring proven, content deferred)
- [x] **Deployment** — `WM.Editor.BuildScript.BuildIOS` produces `build/ios/Unity-iPhone.xcodeproj` from CLI; manual Xcode archive deferred to Phase 11

## Out of Scope (Deferred to Later Slices)

> Anything that is *not* in the skeleton. Be explicit — this list prevents future phases from re-litigating Phase 1's minimalism.

- **Player input / movement** — joystick + `PlayerController` are Phase 2; no input is read in P1 beyond the Input System bootstrap.
- **Box spawning, carry, orders, packing, delivery, cash** — Phases 3–4. P1 only positions the GameObjects these systems will attach to; no scripts on the placeholder primitives.
- **Upgrades, workers, save load, analytics emission, ad/IAP flows** — Phases 5–10. Service interfaces exist with no-op stubs; real implementations live in their owning phases.
- **Cinemachine follow target / damping** — Phase 2. P1 vcam has no Tracking Target. Phase 2 must re-confirm Lens Mode = Perspective + FOV = 40 before adding follow damping (handoff note in plan 01-03).
- **Real bundle identifier, signing team, provisioning profile, Xcode archive automation, TestFlight upload** — Phase 11. P1 uses placeholder `com.warehousemaster.mvp` and the `BuildScript` only produces the Xcode project.
- **Quality tier variants** (low/mid/high URP profiles) — single mobile-aggressive preset only.
- **Localization** — English-only per CLAUDE.md.
- **`Resources/` vs Addressables for prefab loading** — no runtime prefab loading needed; revisit when Phase 3 box prefabs land.
- **Tutorial flow, feedback effects, audio** — Phase 8.
- **Real ads / IAP / mediation / SKAdNetwork / ATT prompts** — out of MVP scope per CLAUDE.md.
- **Multiplayer / backend / cloud save / leaderboards** — out of MVP scope.

## Subsequent Slice Plan

Each later phase adds one vertical slice on top of this skeleton without altering its architectural decisions:

- **Phase 2:** Walk around the warehouse — joystick UI, `PlayerController` reading new Input System actions, Cinemachine follow target (re-confirm Lens Mode + FOV before adding damping), wall/station collisions.
- **Phase 3:** Pick up & carry boxes — Box ScriptableObjects (red/blue/yellow), `BoxSpawner` at loading dock, `CarrySystem` with capacity + visible stack.
- **Phase 4:** Complete an order — Order templates, `OrderManager`, `PackingStation` exact-match + timer, `DeliveryZone`, `CurrencyManager`, cash UI.
- **Phase 5:** Buy an upgrade — Upgrade SO definitions, `UpgradeManager`, six effect handlers, upgrade-station UI.
- **Phase 6:** Save & persist — `SaveManager` real impl behind the `ISaveService` stub from this phase.
- **Phase 7:** Hire the first worker — Shelf storage, NavMesh, `WorkerAI` state machine.
- **Phase 8:** Tutorial & feel — Tutorial state machine, indicator UI, feedback particles + audio.
- **Phase 9:** Analytics wiring — Real `IAnalyticsService` impl behind the stub from this phase.
- **Phase 10:** Fake monetization hooks — `IAdService` real-fake impl + simulated rewarded-ad placements.
- **Phase 11:** TestFlight build pipeline — extends `WM.Editor.BuildScript` from this phase with signing, archive, README.

## Confidence Checkpoints

Each plan's verification gate proves a layer of the skeleton works:

| Plan | Gate | What It Proves |
|---|---|---|
| 01-01 | EditMode test `BootstrapStructureTests.AssetsProject_FoldersExist` and `AsmdefsPresent` and `UrpAssetAssigned` green; project compiles; `BuildScript.BuildIOS` produces `build/ios/Unity-iPhone.xcodeproj`; `ProjectSettings/ProjectSettings.asset` grep gates pass for IL2CPP / Metal / iOS 15 / New Input / Bundle Id / Portrait / Linear (or executor-validated equivalents recorded in `01-01-SUMMARY.md`) | The project skeleton compiles end-to-end and the iOS toolchain is healthy. |
| 01-02 | EditMode test `BootstrapSmokeTests.RequiredGameObject_IsPresent` (parameterized over 12 names) green; PlayMode test `PlayModeSmokeTests.Scene_Loads_GameManagerInitializes` green | The scene loads at runtime, all required GameObjects are present, and the Bootstrap composition root injects services into `GameManager` without exception. |
| 01-03 | EditMode tests `BootstrapSmokeTests.CinemachineCamera_IsConfigured` (with `Lens.FieldOfView == 40` pin) and `UICanvas_IsSafeAreaConfigured` and `EventSystem_UsesInputSystemUIInputModule` green; scene YAML contains exactly one `m_Name: UICanvas` and exactly one `m_Name: EventSystem` (one-of-a-kind regression gate); manual visual verification (operator opens scene in Editor, sees isometric framing of empty warehouse and full-screen Canvas with SafeAreaPanel anchored to safe area) | The user-facing visual end-state matches phase success criterion 3 (camera + UI ready for Phase 2 widgets). |
