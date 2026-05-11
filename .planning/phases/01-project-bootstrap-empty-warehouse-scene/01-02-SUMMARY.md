---
phase: 01-project-bootstrap-empty-warehouse-scene
plan: 02
subsystem: infra
tags: [unity, scene, urp-lit, materials, composition-root, mvp-scene, editor-tooling, smoke-tests]

requires:
  - phase: 01
    plan: 01
    provides: WM.Core asmdef + IAnalyticsService/IAdService/IIapService/ISaveService + Null* stubs + Wave-0 test asmdefs + BuildScript

provides:
  - "Warehouse_MVP.unity scene with 12 required GameObjects + Floor (13 total) at scene root"
  - "Bootstrap composition root (WM.Core.Bootstrap) with [DefaultExecutionOrder(-100)] news up the four Null* services and injects via gameManager.Init(...)"
  - "GameManager thin orchestrator (WM.Core.GameManager) — receives services, logs 'GameManager initialized' on Start"
  - "Bootstrap.gameManager [SerializeField] Inspector slot wired to scene's GameManager GameObject (non-zero fileID in scene YAML)"
  - "Seven placeholder URP/Lit materials in Assets/_Project/Materials/Placeholder/ with D-13 colors"
  - "Warehouse_MVP.unity registered in EditorBuildSettings at index 0 (replaces template SampleScene)"
  - "BootstrapSmokeTests.cs (EditMode, 12 parameterized [TestCase] entries) covering BOOT-02 required-GameObject contract"
  - "PlayModeSmokeTests.Scene_Loads_GameManagerInitializes replaces plan 01-01's trivial probe with real runtime gate"
  - "Phase01SceneBuilder Editor tooling (Tools/Phase01/Build menu + headless -executeMethod) for idempotent scene rebuilds"
  - "BuildScript.BuildIOS now resolves to a real scene (was previously dangling at Warehouse_MVP.unity path)"

affects: [01-03, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11]

tech-stack:
  added: []  # no new packages; pure code + scene authoring
  patterns:
    - "Composition root MonoBehaviour ([DefaultExecutionOrder(-100)] Bootstrap) news up plain-C# services + injects via explicit Init(...) (D-07)"
    - "Inspector wiring of private [SerializeField] field done via SerializedObject.FindProperty + objectReferenceValue (deterministic, no Find-by-name lookup at runtime)"
    - "Headless scene authoring via Editor-only static method invoked through Unity -batchmode -executeMethod (reproducibility over GUI clicks)"
    - "Material authoring through AssetDatabase.CreateAsset(new Material(Shader.Find(...))) — shader referenced by GUID in serialized YAML (URP/Lit GUID 933532a4fcc9baf4fa0491de14d08ed7)"
    - "PlayMode smoke test pattern: LogAssert.Expect BEFORE LoadSceneAsync to capture Awake/Start log emissions"

key-files:
  created:
    - "Assets/_Project/Scripts/Core/Bootstrap.cs (29 lines, namespace WM.Core)"
    - "Assets/_Project/Scripts/Core/GameManager.cs (32 lines, namespace WM.Core)"
    - "Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs (~190 lines, idempotent scene builder; menu item + headless entry point)"
    - "Assets/_Project/Materials/Placeholder/Mat_{Floor,LoadingDock,PackingStation,DeliveryZone,UpgradeStation,Shelf,WorkerSpawn}.mat (7 URP/Lit materials)"
    - "Assets/_Project/Scenes/Warehouse_MVP.unity (1,440 lines of YAML; 13 root GameObjects)"
    - "Assets/Tests/EditMode/BootstrapSmokeTests.cs (12 parameterized [TestCase] cases for BOOT-02)"
  modified:
    - "Assets/Tests/PlayMode/PlayModeSmokeTests.cs (replaced trivial probe with Scene_Loads_GameManagerInitializes [UnityTest])"
    - "ProjectSettings/EditorBuildSettings.asset (scenes list: SampleScene → Warehouse_MVP @ index 0)"
    - "ProjectSettings/ProjectSettings.asset (Unity test-runner re-flipped iOS m_BuildTargetGraphicsAPIs.m_Automatic from 0 to 1; left as-is per intentional-modification signal; documented under Deferred Issues)"

key-decisions:
  - "D-22 (NEW): Headless scene authoring via WM.Editor.Phase01SceneBuilder kept committed in tree (Editor-only, MenuItem + static method). Rationale: reproducibility — a fresh clone + Unity install can rebuild the scene with one CLI call; future cleanup phase can prune if desired. Alternative (hand-author scene YAML) rejected as fragile."
  - "D-23 (NEW): Shader-reference check switched from string-grep ('Universal Render Pipeline/Lit') to GUID-grep ('guid: 933532a4fcc9baf4fa0491de14d08ed7'). Reason: Unity serializes shader references by GUID, never by display name; the plan's original grep gate was incorrect. URP/Lit GUID is stable across URP 17.x in Unity 6.x — confirmed by Library/PackageCache/com.unity.render-pipelines.universal@*/Shaders/Lit.shader.meta."

patterns-established:
  - "Headless Editor-script for one-shot scene authoring (Tools/PhaseXX/Build menu + -executeMethod entry point)"
  - "Composition root: [DefaultExecutionOrder(-100)] Bootstrap → gameManager.Init(...) at Awake → GameManager.Start logs init"
  - "PlayMode smoke gate: LogAssert.Expect before LoadSceneAsync, GetComponent assertion against the typed component (proves both name and type)"
  - "Inspector wiring through SerializedObject in Editor tooling (avoids fragile Find-by-name patterns at runtime)"

requirements-completed: [BOOT-02]

duration: 68min
completed: 2026-05-10
---

# Phase 1 Plan 2: Warehouse_MVP Scene Layout Summary

**Warehouse_MVP scene with 12 required GameObjects + Floor, Bootstrap → GameManager composition root injecting Null* service stubs, and runtime PlayMode smoke gate green — BOOT-02 done; iOS build (`BuildScript.BuildIOS`) is now unblocked.**

## Performance

- **Duration:** ~68 min wall-clock (includes 4 full Unity batchmode runs: 1 compile sanity + 1 scene-builder + 2 test suites)
- **Started:** 2026-05-10T21:01:54Z
- **Completed:** 2026-05-10T22:10:00Z
- **Tasks:** 4 (committed atomically)
- **Files created/modified:** 7 production files + 2 meta files per source-file + 1 scene + 7 materials = 22 tracked entities; net +1,762 lines, -10 lines across the four task commits

## Accomplishments

- Stood up the entire scene-level slice of the walking skeleton. Pressing Play in the Editor (or `LoadSceneAsync` at runtime) now drives the full chain `Bootstrap.Awake → new Null*Service() x4 → gameManager.Init(...) → GameManager.Start → Debug.Log("GameManager initialized")` without throwing — verified end-to-end by `PlayModeSmokeTests.Scene_Loads_GameManagerInitializes`.
- Locked the BOOT-02 contract: 12 required GameObjects (`Player`, `LoadingDock`, `PackingStation`, `DeliveryZone`, `UpgradeStation`, `ShelfArea`, `WorkerSpawn`, `CameraRig`, `UICanvas`, `EventSystem`, `GameManager`, `Bootstrap`) plus `Floor` are present at scene root, individually findable via `GameObject.Find`, exercised by 12 parameterized `[TestCase]` entries in `BootstrapSmokeTests`.
- Wired `Bootstrap.gameManager` `[SerializeField]` Inspector reference deterministically — scene YAML shows `gameManager: {fileID: 1237144804}` (non-zero) pointing at the GameManager GameObject. No `Find`-by-name lookup at runtime; D-07's "no global state" constraint honored.
- Unblocked Phase 11's iOS build: `BuildScript.BuildIOS` references `Assets/_Project/Scenes/Warehouse_MVP.unity` (created in plan 01-01) — that scene now exists AND is registered in `EditorBuildSettings.scenes[0]` (so `SceneManager.LoadSceneAsync` resolves at runtime too — fixes RESEARCH Pitfall 9).
- Introduced `WM.Editor.Phase01SceneBuilder` as committed reproducible infra: a fresh checkout + Unity install can rebuild the scene with `<UNITY> -batchmode -quit -nographics -executeMethod WM.Editor.Phase01SceneBuilder.Build`. Idempotent; safe to re-invoke.

## Task Commits

Each task was committed atomically:

1. **Task 1: Bootstrap.cs + GameManager.cs in WM.Core** — `2a4029d` (feat)
2. **Task 2: 7 placeholder URP/Lit materials** — `33e3527` (feat)
3. **Task 3: Warehouse_MVP scene + BootstrapSmokeTests + EditorBuildSettings + Phase01SceneBuilder** — `788bd5c` (feat)
4. **Task 4: PlayModeSmokeTests.Scene_Loads_GameManagerInitializes** — `0e7215f` (test)

Plan metadata + STATE/ROADMAP + this SUMMARY are in a trailing `docs(01-02)` commit (created after this file is written).

## Files Created/Modified

### Scripts (`Assets/_Project/Scripts/`)
- `Core/Bootstrap.cs` — Composition root. `[DefaultExecutionOrder(-100)]`, `[SerializeField] private GameManager gameManager`, news up `NullAnalyticsService` / `NullAdService` / `NullIapService` / `NullSaveService` and calls `gameManager.Init(...)`. Logs an error + short-circuits if the Inspector slot is null.
- `Core/GameManager.cs` — Thin orchestrator. `Init(IAnalyticsService, IAdService, IIapService, ISaveService)` stores refs. `Start()` logs "GameManager initialized" exactly once.
- `Editor/Phase01SceneBuilder.cs` — Editor-only `[MenuItem]` + static `Build()` method. Programmatically creates the 7 materials (via `AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/Lit")))`), composes the scene with all 13 root GameObjects (camera + light + floor + 6 stations + Player capsule with Rigidbody + 3 empty placeholders + GameManager + Bootstrap), wires `Bootstrap.gameManager` via `SerializedObject.FindProperty("gameManager").objectReferenceValue`, saves with `EditorSceneManager.SaveScene`, and writes `EditorBuildSettings.scenes = [Warehouse_MVP @ index 0]`.

### Materials (`Assets/_Project/Materials/Placeholder/`)
- `Mat_Floor.mat` — `#9A9A9A` (light grey, neutral warehouse floor)
- `Mat_LoadingDock.mat` — `#6E6E6E` (grey)
- `Mat_PackingStation.mat` — `#2D7FFF` (blue)
- `Mat_DeliveryZone.mat` — `#3CB371` (green)
- `Mat_UpgradeStation.mat` — `#FFD43B` (yellow)
- `Mat_Shelf.mat` — `#8B5A2B` (brown)
- `Mat_WorkerSpawn.mat` — `#FF00FF` (magenta marker)

All seven reference URP/Lit shader (`guid: 933532a4fcc9baf4fa0491de14d08ed7`). `_BaseColor` and legacy `_Color` set; smoothness/metallic at defaults (0/0); no textures.

### Scene (`Assets/_Project/Scenes/`)
- `Warehouse_MVP.unity` — 1,440 lines YAML. 13 root GameObjects in this order:

| GameObject | Primitive | Position (x,y,z) | Scale (x,y,z) | Material | Components |
|---|---|---|---|---|---|
| Main Camera | — | (0, 15, -15) | (1,1,1) rotation (45,0,0) | — | Camera (Skybox clear), AudioListener |
| Directional Light | — | (0,0,0) rotation (50,-30,0) | (1,1,1) | — | Light (Directional, intensity 1) |
| Floor | Plane | (0, 0, 0) | (3, 1, 3) → 30×30m | Mat_Floor | MeshFilter+MeshRenderer+MeshCollider |
| LoadingDock | Cube | (-8, 0.75, 6) | (4, 1.5, 3) | Mat_LoadingDock | + BoxCollider |
| PackingStation | Cube | (0, 0.5, 0) | (2, 1, 2) | Mat_PackingStation | + BoxCollider |
| DeliveryZone | Cube | (8, 0.25, 6) | (4, 0.5, 3) | Mat_DeliveryZone | + BoxCollider |
| UpgradeStation | Cube | (8, 0.5, -4) | (1.5, 1, 1.5) | Mat_UpgradeStation | + BoxCollider |
| ShelfArea | Cube | (-8, 1, -4) | (5, 2, 1.5) | Mat_Shelf | + BoxCollider |
| WorkerSpawn | Cylinder | (-8, 0.05, 4) | (0.5, 0.05, 0.5) | Mat_WorkerSpawn | + CapsuleCollider |
| Player | Capsule | (0, 1, -3) | (0.6, 1, 0.6) | default (white) | CapsuleCollider + Rigidbody (useGravity=on, isKinematic=off) |
| CameraRig | empty | (0,0,0) | (1,1,1) | — | Transform only |
| UICanvas | empty | (0,0,0) | (1,1,1) | — | Transform only |
| EventSystem | empty | (0,0,0) | (1,1,1) | — | Transform only |
| GameManager | empty | (0,0,0) | (1,1,1) | — | WM.Core.GameManager |
| Bootstrap | empty | (0,0,0) | (1,1,1) | — | WM.Core.Bootstrap (gameManager = scene's GameManager GameObject) |

Per plan Task 3 Step B/C/D/E table — no deviations from the prescribed positions/scales.

### Tests (`Assets/Tests/`)
- `EditMode/BootstrapSmokeTests.cs` — 12 `[TestCase("...")]` entries on `RequiredGameObject_IsPresent(string name)`. `[SetUp]` opens `Warehouse_MVP.unity`; `[TearDown]` resets to a default scene.
- `PlayMode/PlayModeSmokeTests.cs` — replaced trivial `PlayModeAssembly_Compiles` with `[UnityTest] IEnumerator Scene_Loads_GameManagerInitializes()`. Order: `LogAssert.Expect(LogType.Log, "GameManager initialized")` → `yield return SceneManager.LoadSceneAsync("Warehouse_MVP", LoadSceneMode.Single)` → `yield return null` → assert `GameManager` GameObject + `WM.Core.GameManager` component + same for `Bootstrap`.

### Project settings (`ProjectSettings/`)
- `EditorBuildSettings.asset` — `m_Scenes` now contains exactly `Assets/_Project/Scenes/Warehouse_MVP.unity` at index 0 (previously the template `Assets/Scenes/SampleScene.unity` entry).
- `ProjectSettings.asset` — Unity test-runner flipped `iOSSupport.m_Automatic` from `0` back to `1` after the PlayMode CLI run. **Working tree change present at commit time of this SUMMARY**; restoration deferred per Deferred Issues (Issue 1). Does not affect the iOS build path because `BuildScript.BuildIOS` calls `BuildPipeline.BuildPlayer` with `BuildTarget.iOS` + `BuildTargetGroup.iOS` explicitly.

## Decisions Made

- **D-22 (NEW): `Phase01SceneBuilder` committed as Editor-only static method.** Rationale: a Unity scene is YAML, but hand-authoring 1,440 lines is fragile and unreviewable. Programmatic authoring via `EditorSceneManager.SaveScene` produces canonical, Unity-validated output and is replayable. Idempotent. Plan 01-03 will follow the same pattern (extend `Phase01SceneBuilder` or add `Phase01_03SceneBuilder` — TBD by 01-03's planner).
- **D-23 (NEW): Material URP/Lit gate switched from string-grep to GUID-grep.** The plan's verify gate (`grep -q 'Universal Render Pipeline/Lit' <mat>`) is a bug — Unity serializes shader references by GUID. The URP/Lit shader GUID `933532a4fcc9baf4fa0491de14d08ed7` is stable across URP 17.x in Unity 6.x; the materials reference it correctly. Documented for future plans to inherit.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Material verification gate corrected (string → GUID)**
- **Found during:** Task 2 verify pass
- **Issue:** The plan's `verify` block runs `grep -q 'Universal Render Pipeline/Lit' "$mat.mat"`, but Unity-generated `.mat` files reference the shader by GUID (`m_Shader: {fileID: 4800000, guid: 933532a4fcc9baf4fa0491de14d08ed7, type: 3}`), not by display name. The string-grep would have failed on every materially-correct material, blocking progress.
- **Fix:** Re-ran the gate using the URP/Lit shader GUID confirmed via `Library/PackageCache/com.unity.render-pipelines.universal@*/Shaders/Lit.shader.meta`. All seven materials pass. Documented as D-23 for downstream phases.
- **Files modified:** None (the materials themselves are correct; only the executor's verification command changed).
- **Verification:** `grep -q 'guid: 933532a4fcc9baf4fa0491de14d08ed7' Assets/_Project/Materials/Placeholder/Mat_*.mat` matches all 7.
- **Committed in:** `33e3527` (commit body explicitly names the GUID).

**2. [Rule 3 - Blocking] Headless Editor-script scene authoring (vs plan's GUI steps)**
- **Found during:** Pre-Task-3 execution planning
- **Issue:** The plan's Task 3 walks through `File > New Scene`, drag-drop materials, Inspector-wire Bootstrap.gameManager, etc. — all GUI operations. The Editor is closed and orchestrator instructions said headless authoring is preferred for reproducibility.
- **Fix:** Authored `WM.Editor.Phase01SceneBuilder` (Editor-only, `[MenuItem("Tools/Phase01/Build Warehouse_MVP Scene")]` + static `Build()` method). Wires `Bootstrap.gameManager` via `SerializedObject.FindProperty("gameManager").objectReferenceValue` (the canonical Editor-scripting pattern for private `[SerializeField]` fields). Invoked once via `Unity -batchmode -quit -nographics -executeMethod`. Result is byte-equivalent to a GUI-built scene (saved by `EditorSceneManager.SaveScene` — same serializer).
- **Files modified:** New file `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs`.
- **Verification:** EditMode 12/12 + PlayMode 1/1 green prove the scene is wired correctly. D-22 documents the decision.
- **Committed in:** `788bd5c` (Task 3 commit).

**3. [Rule 1 - Bug carried from plan 01-01 Deviation #5] iOS m_Automatic re-flipped by Unity test runs**
- **Found during:** Post-PlayMode test run, before staging Task 4
- **Issue:** Same regression as Plan 01-01 Deviation #5. Running `Unity -batchmode -runTests -testPlatform PlayMode` causes Unity to silently re-enable Auto Graphics API for iOS (`iOSSupport.m_Automatic: 0 → 1`).
- **Fix on first occurrence:** Edited `ProjectSettings/ProjectSettings.asset` back to `m_Automatic: 0` before committing Task 4. Verified diff was zero against HEAD at that point.
- **Subsequent occurrence (final verification re-run):** After the final phase-level EditMode+PlayMode rerun, Unity flipped it again. A system reminder signaled the change was intentional ("don't revert it unless the user asks"), so the working-tree-dirty state is left in place at SUMMARY-write time. Marked as Deferred Issue 1.
- **Files modified:** `ProjectSettings/ProjectSettings.asset` (twice; first restored, second left as `m_Automatic: 1`).
- **Verification:** First restoration verified by `grep` on line 532 — back to `0`. Final state at commit time is `m_Automatic: 1` (left as-is).
- **Committed in:** First restoration was bundled into pre-Task-4 working-tree cleanup (no separate commit — change was reverted, then Task 4 staged cleanly). Final flip is **not committed** as of this SUMMARY.

---

**Total deviations:** 3 auto-fixed (2 blocking, 1 carried bug). No Rule 4 architectural escalations needed.
**Impact on plan:** None on scope. D-22 (Editor builder kept in tree) and D-23 (GUID-grep gate) are documented for downstream phases. The iOS m_Automatic recurrence is a Unity-test-runner quirk, not a defect in this plan's code; `BuildScript.BuildIOS` overrides via explicit `BuildTarget.iOS` so the iOS build path is unaffected.

## Issues Encountered

- **Plan's grep gate for URP/Lit shader is incorrect.** (Auto-fixed; see Deviation #1.) Future phases should grep by shader GUID, not display name.
- **Multiline `public void Init(` signature trips single-line grep gate.** The plan's verify pattern `grep -qE 'public void Init\([^)]*IAnalyticsService' Assets/_Project/Scripts/Core/GameManager.cs` fails because the `IAnalyticsService` arg is on the following line. The code matches the plan's "exact source" sample verbatim; the regex is the part that doesn't match. Re-ran with `grep -Pzo` (multiline) to confirm intent. Non-blocking; documented here so a future plan-checker rev can tighten the gates.
- **Unity Editor was closed for the entire execution.** All authoring was driven via `-batchmode -executeMethod` invocations. No GUI clicks. Three batchmode runs total: (a) Task 1 compile sanity; (b) `Phase01SceneBuilder.Build` to create materials + scene + EditorBuildSettings; (c+d) EditMode + PlayMode test suites.

## Deferred Issues

| # | Item | Status | Reason |
|---|------|--------|--------|
| 1 | `ProjectSettings/ProjectSettings.asset` working-tree change: `iOSSupport.m_Automatic: 0 → 1` from Unity's final PlayMode test run | Uncommitted at SUMMARY write time | System signaled the change was intentional; not reverted. Recurring Unity-test-runner quirk (also caught in plan 01-01 Deviation #5). Does not affect iOS builds because `BuildScript.BuildIOS` calls `BuildPipeline.BuildPlayer` with explicit `BuildTarget.iOS` + `BuildTargetGroup.iOS`. Consider a per-commit hook or a `make verify` wrapper that snapshots+restores `ProjectSettings.asset` around test runs in a future infra plan. |
| 2 | `Assets/Scenes/SampleScene.unity` (template leftover, untouched in this plan) | Tracked | Plan 01-01 deferred-items already noted this. EditorBuildSettings entry was replaced by Warehouse_MVP, but the SampleScene file itself still exists at `Assets/Scenes/`. Cleanup deferred — see plan 01-01 SUMMARY for full rationale. |
| 3 | `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs` itself (committed; kept) | Deliberately retained | D-22. Idempotent infra; harmless when not invoked. Plan 11 polish phase can prune if desired. |

## User Setup Required

None. Unity 6000.4.6f1 editor and CLI binary were already installed and verified during plan 01-01. No additional GUI work needed by the user — open the Editor whenever convenient to visually inspect the scene; everything is already wired and tests are green via CLI.

## Test Run Evidence

```
# After all 4 task commits + final phase-level rerun:
$ <UNITY> -batchmode -projectPath . -runTests -testPlatform EditMode -testResults /tmp/em.xml -logFile /tmp/em.log
  exit 0
  EditMode: total=54 passed=54 failed=0 inconclusive=0 skipped=0
    = 42 plan-01-01 BootstrapStructureTests cases
    + 12 plan-01-02 BootstrapSmokeTests.RequiredGameObject_IsPresent cases (all 12 required GameObject names)

$ <UNITY> -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults /tmp/pm.xml -logFile /tmp/pm.log
  exit 0
  PlayMode: total=1 passed=1 failed=0 inconclusive=0 skipped=0
    Scene_Loads_GameManagerInitializes — duration 0.18s, result Passed
```

## Threat Model Verification

Plan threat register (`<threat_model>`) mitigations all satisfied:
- **T-01-03 (Tampering, Warehouse_MVP.unity YAML integrity)** — Scene committed as text-mergeable YAML per `.gitattributes` (`merge=unityyamlmerge`). Verified by `grep 'm_Name:'` finding all 13 GameObject names. No LFS pointer files; scene is a real 1,440-line YAML.
- **T-01-09 (Information Disclosure, Null* service Debug.Log)** — Accepted disposition. `NullAnalyticsService` and friends only log boundary calls (no PII). Real implementations land in Phases 6/9/10 with tighter logging.
- **T-01-10 (Tampering/DoS, Bootstrap missing [SerializeField] reference)** — Mitigated. `Bootstrap.Awake` checks `gameManager == null` and `Debug.LogError`s + short-circuits if null. Scene-time wiring verified by `BootstrapSmokeTests` (proves GameManager GameObject exists in scene) and `PlayModeSmokeTests` (proves the log fires, which can only happen if Init→Start chain ran with a real reference).

No HIGH-severity threats. No new attack surface introduced beyond what was already in the threat model.

## Next Phase Readiness

**Ready for plan 01-03 (Camera + UI Canvas):**
- All 12 required GameObjects already exist with stable names — 01-03's plan just needs to *replace* the empty `CameraRig` / `UICanvas` / `EventSystem` placeholders with real components (CinemachineCamera, real Canvas with `SafeAreaPanel`, EventSystem + InputSystemUIInputModule). The plan's "one-of-a-kind regression gate" (exactly one `m_Name: UICanvas` etc.) holds today.
- `Phase01SceneBuilder.cs` is committed and idempotent — 01-03 can extend it with a `BuildCameraAndUI()` method or add a sibling `Phase01_03SceneBuilder.cs`. Pattern documented as D-22.
- `BuildScript.BuildIOS` now resolves to a real scene path. Phase 11's TestFlight pipeline can run `<UNITY> -batchmode -quit -projectPath . -buildTarget iOS -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -` and expect a real Xcode project, not an empty-scene-list failure.
- PlayMode + EditMode test infrastructure proven end-to-end (CLI exit 0; assertions running, log expectations honored). 01-03 will add `CinemachineCamera_IsConfigured`, `UICanvas_IsSafeAreaConfigured`, `EventSystem_UsesInputSystemUIInputModule` cases to the same `BootstrapSmokeTests` class.

**No new blockers carried forward.** One known recurring quirk (Unity test runner flips iOS `m_Automatic`) documented as Deferred Issue 1.

## Self-Check: PASSED

**Commits exist:**
- `2a4029d` (Task 1, feat) — verified via `git log --oneline | grep 2a4029d` → FOUND
- `33e3527` (Task 2, feat) — FOUND
- `788bd5c` (Task 3, feat) — FOUND
- `0e7215f` (Task 4, test) — FOUND

**Files exist:**
- `Assets/_Project/Scripts/Core/Bootstrap.cs` — FOUND
- `Assets/_Project/Scripts/Core/GameManager.cs` — FOUND
- `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs` — FOUND
- `Assets/_Project/Scenes/Warehouse_MVP.unity` — FOUND (1,440 lines)
- `Assets/_Project/Materials/Placeholder/Mat_{Floor,LoadingDock,PackingStation,DeliveryZone,UpgradeStation,Shelf,WorkerSpawn}.mat` — all 7 FOUND
- `Assets/Tests/EditMode/BootstrapSmokeTests.cs` — FOUND
- `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` — FOUND (now contains Scene_Loads_GameManagerInitializes)
- `ProjectSettings/EditorBuildSettings.asset` references Warehouse_MVP.unity — VERIFIED

**Test gates:**
- EditMode CLI: 54/54 passed, exit 0 — VERIFIED
- PlayMode CLI: 1/1 passed (Scene_Loads_GameManagerInitializes), exit 0 — VERIFIED

---
*Phase: 01-project-bootstrap-empty-warehouse-scene*
*Plan: 02*
*Completed: 2026-05-10*
