---
phase: 01-project-bootstrap-empty-warehouse-scene
verified: 2026-05-11T09:18:00Z
status: human_needed
score: 18/19 must-haves verified (1 deferred to human verification)
overrides_applied: 0
human_verification:
  - test: "Open Unity Editor on Warehouse_MVP scene, press Play, confirm camera frames the empty warehouse from the isometric Cinemachine vcam (FOV=40) with all six colored stations visible and no NullReferenceException in Console"
    expected: "Game view shows the floor + 6 colored stations (loading dock grey, packing blue, delivery green, upgrade yellow, shelf brown, worker spawn magenta) framed from the (10, 12, -10) vcam at 45deg pitch; UI overlay empty but SafeAreaPanel rect is non-zero on iPhone simulator aspect"
    why_human: "Phase 1 Success Criterion 1 + 3 require visual + interactive Editor verification; cannot grep camera framing or skybox-vs-warehouse render output"
  - test: "Run BuildScript.BuildIOS via Unity batchmode CLI, then open build/ios/Unity-iPhone.xcodeproj in Xcode and build to iOS Simulator"
    expected: "CLI exits 0; build/ios/Unity-iPhone.xcodeproj produced; Xcode build succeeds; simulator boots to the empty warehouse scene from the Cinemachine vcam framing"
    why_human: "Phase 1 Success Criterion 3 ('debug iOS build runs on simulator/device and shows the empty warehouse scene from the configured camera') — explicitly deferred in 01-03-SUMMARY.md (Deferred Issue 3) to orchestrator/human owner; cannot validate Xcode archive + simulator launch from a shell verifier"
---

# Phase 1: Project Bootstrap & Empty Warehouse Scene Verification Report

**Phase Goal:** A Unity iOS project exists with the agreed folder structure and a placeholder `Warehouse_MVP` scene that builds and launches.
**Verified:** 2026-05-11T09:18:00Z
**Status:** human_needed
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

Truths derived from ROADMAP.md Success Criteria + PLAN frontmatter must_haves across plans 01-01, 01-02, 01-03.

| #   | Truth                                                                                                                                       | Status     | Evidence                                                                                                                                                                                |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------- | ---------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Opening project in Unity Editor loads `Warehouse_MVP` scene with all required objects (SC-1 from ROADMAP)                                   | VERIFIED   | EditMode CLI `BootstrapSmokeTests.RequiredGameObject_IsPresent` 12/12 PASS for all 12 required names; one-of-a-kind name counts UICanvas=1, EventSystem=1, Main Camera=1, CM vcam1=1     |
| 2   | Folder structure under `Assets/_Project/` matches architecture spec, with assembly definitions per system folder (SC-2 from ROADMAP)        | VERIFIED   | `BootstrapStructureTests.AssetsProject_FoldersExist` 28/28 PASS; `BootstrapStructureTests.AsmdefsPresent` 13/13 PASS; `find Assets/_Project/Scripts -name '*.asmdef'` returns 13 files   |
| 3   | A debug iOS build runs on simulator/device and shows the empty warehouse scene from the configured camera (SC-3 from ROADMAP)               | DEFERRED   | Requires manual Xcode build + simulator launch — see Human Verification table                                                                                                            |
| 4   | Project opens in Unity 6.x without compile errors (plan 01-01 must-have, adjusted for D-21 deviation)                                       | VERIFIED   | `ProjectVersion.txt` reads `m_EditorVersion: 6000.4.6f1`; EditMode + PlayMode CLI both exit 0 (no compile errors would break test compile); D-21 user-approved deviation                  |
| 5   | All 13 production asmdefs and 2 test asmdefs recognized by Editor                                                                           | VERIFIED   | `BootstrapStructureTests.AsmdefsPresent` 13/13 PASS; `.csproj` files exist for WM.Core, WM.Editor, WM.UI, WM.Tests.EditMode, WM.Tests.PlayMode (Unity solution-export confirmed)          |
| 6   | URP mobile-aggressive asset is the active render pipeline                                                                                   | VERIFIED   | `BootstrapStructureTests.UrpAssetAssigned` PASS; `GraphicsSettings.asset` `m_CustomRenderPipeline` → GUID `41ba64d3375ae47769f14ce4958f9ecb` = `URP_Mobile.asset`                         |
| 7   | Service interfaces (IAnalyticsService, IAdService, IIapService, ISaveService) compile and have Null* stub implementations in WM.Core        | VERIFIED   | 4 interfaces + 4 NullX stubs exist with sealed implementations under `Assets/_Project/Scripts/Core/` + `Stubs/`; PlayMode `Scene_Loads_GameManagerInitializes` PASS proves runtime wiring |
| 8   | Running Unity CLI EditMode test platform exits 0 with green test suite                                                                      | VERIFIED   | Re-ran `Unity -batchmode -runTests -testPlatform EditMode` on 2026-05-11T09:12Z → exit 0, 57/57 PASS                                                                                      |
| 9   | BuildScript.BuildIOS exists with explicit non-zero exit on failure (no silent CI green)                                                     | VERIFIED   | `Assets/_Project/Scripts/Editor/BuildScript.cs:40` calls `EditorApplication.Exit(1)` on `BuildResult != Succeeded` after `BuildLogError`                                                  |
| 10  | `.gitignore` excludes Library/, Temp/, Logs/, build/, signing certs (*.p12, *.mobileprovision)                                              | VERIFIED   | `grep` matches `/[Ll]ibrary/`, `/build/`, `*.mobileprovision`, `*.p12` in `.gitignore`                                                                                                    |
| 11  | `.gitattributes` keeps .unity/.prefab/generic .asset as text+yamlmerge and only LFS-tracks true binaries (D-20)                             | VERIFIED   | `*.unity merge=unityyamlmerge eol=lf`, `*.prefab merge=unityyamlmerge eol=lf`; no LFS filter on these; `*.png/*.psd` use `filter=lfs`                                                     |
| 12  | ProjectSettings reflect iOS Player Settings (IL2CPP, Metal-only, iOS 15+, New Input System, Bundle Id, Portrait, Linear)                    | VERIFIED   | YAML keys all present: `applicationIdentifier iPhone: com.warehousemaster.mvp`, `iOSTargetOSVersionString: 15.0`, `activeInputHandler: 1`, `defaultScreenOrientation: 0`, `m_ActiveColorSpace: 1`, `scriptingBackend: iPhone: 1`, `m_BuildTargetGraphicsAPIs iOSSupport m_Automatic: 0` (HEAD state) |
| 13  | Bootstrap.Awake runs before GameManager.Start (DefaultExecutionOrder enforces this) and Bootstrap news up Null* services + Init(GameManager) | VERIFIED   | `Bootstrap.cs:14` has `[DefaultExecutionOrder(-100)]`; constructs 4 NullX services + calls `gameManager.Init(...)`; `PlayModeSmokeTests.Scene_Loads_GameManagerInitializes` PASS via `LogAssert.Expect(LogType.Log, "GameManager initialized")` resolving after LoadSceneAsync |
| 14  | Bootstrap.gameManager [SerializeField] is wired in scene (non-zero fileID)                                                                  | VERIFIED   | `Warehouse_MVP.unity:517` `gameManager: {fileID: 1237144804}`; `&1237144804` is a MonoBehaviour with `m_EditorClassIdentifier: WM.Core::WM.Core.GameManager`                              |
| 15  | Warehouse_MVP scene registered in EditorBuildSettings (so SceneManager.LoadScene resolves)                                                  | VERIFIED   | `EditorBuildSettings.asset:9` `path: Assets/_Project/Scenes/Warehouse_MVP.unity` (enabled, index 0); PlayMode `Scene_Loads_GameManagerInitializes` LoadSceneAsync resolves                |
| 16  | CameraRig contains Main Camera (with CinemachineBrain) and CM vcam1 (CinemachineCamera, Lens.ModeOverride=Perspective, FieldOfView=40)      | VERIFIED   | Scene line 1341 = CinemachineBrain; line 1161 = CinemachineCamera; line 1174 = `FieldOfView: 40`; line 1179 = `ModeOverride: 2` (Perspective); test `CinemachineCamera_IsConfigured` PASS |
| 17  | UICanvas is Screen Space - Camera (RenderMode=1) at 1080x1920 with Match=0.5, with SafeAreaPanel child anchored to Screen.safeArea           | VERIFIED   | Scene `m_RenderMode: 1`, `m_MatchWidthOrHeight: 0.5`; test `UICanvas_IsSafeAreaConfigured` PASS (verifies Canvas + CanvasScaler + SafeAreaPanel parented under UICanvas)                  |
| 18  | EventSystem has Input System UI Input Module (no legacy StandaloneInputModule)                                                              | VERIFIED   | Scene `m_EditorClassIdentifier: Unity.InputSystem::UnityEngine.InputSystem.UI.InputSystemUIInputModule` at line 562; test `EventSystem_UsesInputSystemUIInputModule` PASS                |
| 19  | PlayMode CLI exits 0 with Scene_Loads_GameManagerInitializes green                                                                          | VERIFIED   | Re-ran `Unity -batchmode -runTests -testPlatform PlayMode` on 2026-05-11T09:14Z → exit 0, 1/1 PASS                                                                                        |

**Score:** 18/19 truths verified, 1 deferred to human verification (truth 3 is the only deferred — SC-3 requires Xcode/simulator)

### Required Artifacts

| Artifact                                                                            | Expected                                                              | Status     | Details                                                                                |
| ----------------------------------------------------------------------------------- | --------------------------------------------------------------------- | ---------- | -------------------------------------------------------------------------------------- |
| `ProjectSettings/ProjectVersion.txt`                                                | Pinned `6000.3.7f1` OR D-21-accepted `6000.4.x`                       | VERIFIED   | Reads `m_EditorVersion: 6000.4.6f1` (D-21 user-approved deviation)                     |
| `Packages/manifest.json`                                                            | Contains Cinemachine, Input System, URP, Test Framework               | VERIFIED   | All four package keys present; Cinemachine 3.1.6, Input System 1.15.0, URP 17.4.0      |
| `Assets/_Project/Scripts/Core/WM.Core.asmdef`                                       | Root asmdef                                                           | VERIFIED   | Exists; `"name": "WM.Core"`                                                            |
| `Assets/_Project/Scripts/Editor/WM.Editor.asmdef`                                   | Editor-only platform                                                  | VERIFIED   | `includePlatforms: ["Editor"]`; references expanded with WM.UI/Cinemachine/InputSystem/UGUI per plan 01-03 |
| `Assets/_Project/Scripts/Editor/BuildScript.cs`                                     | `namespace WM.Editor`, `public static void BuildIOS()`                | VERIFIED   | Both patterns present at lines 9 + 16; explicit `EditorApplication.Exit(1)` on failure |
| `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef`                                    | Editor-only + Test Assemblies semantics                               | VERIFIED   | `includePlatforms: ["Editor"]`, `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, NUnit ref |
| `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef`                                    | Any-platform Test Assembly                                            | VERIFIED   | `defineConstraints: ["UNITY_INCLUDE_TESTS"]`, NUnit ref, no UnityEditor.TestRunner ref  |
| `.gitignore`                                                                        | Unity ignore + signing cert exclusion                                 | VERIFIED   | Library/, build/, *.mobileprovision, *.p12 all present                                  |
| `.gitattributes`                                                                    | LFS rules (binaries only); YAML merge for Unity text                  | VERIFIED   | `*.unity merge=unityyamlmerge`, no LFS filter on .unity/.prefab; LFS on *.png/*.psd     |
| `Assets/_Project/Settings/URP_Mobile.asset` + `URP_Mobile_Renderer.asset`           | Active mobile-aggressive pipeline                                     | VERIFIED   | Both files exist; GraphicsSettings references URP_Mobile by GUID                       |
| `Assets/_Project/Scripts/Core/Bootstrap.cs`                                         | Composition root w/ [DefaultExecutionOrder(-100)] + 4 Null* + Init    | VERIFIED   | All required patterns present in file                                                  |
| `Assets/_Project/Scripts/Core/GameManager.cs`                                       | `Init(...)` + `Debug.Log("GameManager initialized")`                  | VERIFIED   | Both required patterns present                                                         |
| `Assets/_Project/Scenes/Warehouse_MVP.unity`                                        | All 12 required GameObjects + Floor + camera + light                  | VERIFIED   | 1,717 lines YAML; 12 required + Floor + Main Camera + CM vcam1 + Directional Light + SafeAreaPanel all confirmed via grep |
| 7 placeholder URP/Lit materials in `Assets/_Project/Materials/Placeholder/`         | URP/Lit GUID 933532a4fcc9baf4fa0491de14d08ed7                         | VERIFIED   | All 7 materials reference URP/Lit by GUID (D-23 GUID-grep gate)                        |
| `Assets/_Project/Scripts/UI/SafeAreaPanel.cs`                                       | `namespace WM.UI`, `Screen.safeArea`, RectTransform anchorer          | VERIFIED   | All required patterns present in file                                                  |
| `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs`                             | Idempotent scene builder (D-22)                                       | VERIFIED   | `[MenuItem("Tools/Phase01/Build Warehouse_MVP Scene")]` + `public static void Build()` at lines 48-49 |
| `Assets/_Project/Scripts/Editor/Phase01CameraUiBuilder.cs`                          | Idempotent camera+UI builder (D-25)                                   | VERIFIED   | `[MenuItem("Tools/Phase01/Build Camera+UI (Plan 01-03)")]` + `public static void Build()` at lines 42-43 |

### Key Link Verification

| From                                          | To                                                                | Via                                          | Status   | Details                                                                                                                          |
| --------------------------------------------- | ----------------------------------------------------------------- | -------------------------------------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `Packages/manifest.json`                      | Cinemachine 3.1.6 / Input System 1.15.0 / URP 17.4.0 / TestFW    | package manager pins                         | WIRED    | grep confirms 4 packages pinned in manifest                                                                                       |
| `ProjectSettings/GraphicsSettings.asset`      | `Assets/_Project/Settings/URP_Mobile.asset`                       | `m_CustomRenderPipeline` GUID reference      | WIRED    | GraphicsSettings.asset:40 `m_CustomRenderPipeline {fileID:11400000, guid: 41ba64d3375ae47769f14ce4958f9ecb}` → URP_Mobile.asset    |
| `ProjectSettings/ProjectSettings.asset`       | iOS Player Settings (Inspector)                                   | Serialized YAML keys                         | WIRED    | All 7 expected keys present and at expected values (BundleId, iOS 15, activeInputHandler 1, defaultScreenOrientation 0, etc.)    |
| `Null*Service.cs` (4 files)                   | `I*Service.cs` interfaces                                         | `class NullX : IX` declaration               | WIRED    | All 4 stubs declare `: IXService`; sealed implementations; no-op behavior with optional Debug.Log                                  |
| Bootstrap (scene) gameManager [SerializeField] | GameManager (scene component)                                     | Inspector reference + Scene YAML fileID      | WIRED    | `gameManager: {fileID: 1237144804}` → MonoBehaviour with `m_EditorClassIdentifier: WM.Core::WM.Core.GameManager`                  |
| Bootstrap.Awake                               | NullAnalyticsService/NullAdService/NullIapService/NullSaveService | `new T()` + `gameManager.Init(...)`          | WIRED    | Bootstrap.cs:27-32 news up 4 Nulls + calls Init; PlayMode test asserts `Debug.Log("GameManager initialized")` fires post-Init    |
| Warehouse_MVP scene                           | EditorBuildSettings.scenes                                        | Build Profiles scene list                    | WIRED    | EditorBuildSettings.asset registers `Assets/_Project/Scenes/Warehouse_MVP.unity` enabled at index 0                                |
| CameraRig/Main Camera                         | CameraRig/CM vcam1                                                | CinemachineBrain (Main Camera) drives vcam   | WIRED    | Main Camera has CinemachineBrain + AudioListener; CM vcam1 sibling has CinemachineCamera (Target=null) — driven by Brain default priority |
| UICanvas/SafeAreaPanel                        | `Screen.safeArea` runtime                                         | `Apply(safeArea)` updates anchorMin/Max      | WIRED    | SafeAreaPanel.cs:30 references `Screen.safeArea`; anchorMin/Max + offsetMin/Max reset in `Apply(Rect)`                            |
| EventSystem                                   | InputSystemUIInputModule (no StandaloneInputModule)               | Component on EventSystem GameObject          | WIRED    | Scene line 562 has InputSystemUIInputModule; test asserts presence + absence of StandaloneInputModule                            |

### Data-Flow Trace (Level 4)

Phase 1 is bootstrap/scaffolding — no dynamic-data renderers yet. The only runtime data-flow at this stage is the Bootstrap → GameManager service injection chain, which is covered by truth 13 and the PlayMode smoke test `Scene_Loads_GameManagerInitializes`. SafeAreaPanel reads `Screen.safeArea` (OS-provided), which is verified by truth 17. No HOLLOW/STATIC/DISCONNECTED findings.

| Artifact          | Data Variable    | Source             | Produces Real Data | Status   |
| ----------------- | ---------------- | ------------------ | ------------------ | -------- |
| Bootstrap.cs      | analytics/ads/iap/save | `new NullX()` constructors | Yes (Debug.Log boundary)  | FLOWING  |
| GameManager.cs    | _analytics/_ads/_iap/_save | Bootstrap.Init args | Yes (assigned in Init) | FLOWING  |
| SafeAreaPanel.cs  | _lastApplied / _lastScreen | `Screen.safeArea`, `Screen.{width,height}` | Yes (OS-provided) | FLOWING  |

### Behavioral Spot-Checks

Re-ran the Unity test suites on 2026-05-11 to confirm they actually exit 0 today, not just at SUMMARY-write time on 2026-05-10/11.

| Behavior                                                                                  | Command                                                                                                                                                | Result                                          | Status |
| ----------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------- | ------ |
| EditMode test suite exits 0 with 57/57 PASS                                              | `Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults /tmp/wm-verify-editmode.xml`                                            | exit 0; testcasecount=57 passed=57 failed=0     | PASS   |
| PlayMode test suite exits 0 with 1/1 PASS (Scene_Loads_GameManagerInitializes)           | `Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testResults /tmp/wm-verify-playmode.xml`                                            | exit 0; testcasecount=1 passed=1 failed=0; Scene_Loads_GameManagerInitializes Passed (duration 0.252s) | PASS   |
| All 13 production asmdefs discoverable on disk                                            | `find Assets/_Project/Scripts -name '*.asmdef'`                                                                                                        | 13 results                                      | PASS   |
| All 12 required GameObject names appear in scene YAML                                     | `grep -E 'm_Name: (Player|LoadingDock|...)' Assets/_Project/Scenes/Warehouse_MVP.unity \| sort -u`                                                       | 17 unique matches (12 required + 5 supporting) | PASS   |
| URP/Lit GUID used in all 7 placeholder materials                                          | `grep -c '933532a4fcc9baf4fa0491de14d08ed7' Assets/_Project/Materials/Placeholder/Mat_*.mat`                                                            | All 7 files: 1                                  | PASS   |
| Bootstrap.gameManager Inspector reference is non-zero                                     | `grep -A0 'gameManager:' Assets/_Project/Scenes/Warehouse_MVP.unity`                                                                                   | `gameManager: {fileID: 1237144804}` (non-zero) | PASS   |
| BuildScript.BuildIOS exists; sim/device archive verification deferred to human            | Read BuildScript.cs                                                                                                                                    | Compiles; calls BuildPipeline.BuildPlayer with explicit BuildTarget.iOS + Exit(1) on failure | PASS   |

### Requirements Coverage

| Requirement | Source Plan         | Description                                                                                                          | Status     | Evidence                                                                                                                                |
| ----------- | ------------------- | -------------------------------------------------------------------------------------------------------------------- | ---------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| BOOT-01     | Plan 01-01          | Unity project initialized for iOS with URP and `Assets/_Project/...` folder structure                                | SATISFIED  | 13 asmdefs + folder skeleton verified; URP_Mobile active; iOS Player Settings serialized; EditMode 42 BOOT-01 cases green                |
| BOOT-02     | Plan 01-02, 01-03   | `Warehouse_MVP` scene created with all required objects + GameManager wired                                          | SATISFIED  | 12 required GameObjects + Bootstrap → GameManager wired via [SerializeField]; PlayMode Scene_Loads_GameManagerInitializes PASS           |

Note: `.planning/REQUIREMENTS.md` traceability table line 120 still lists BOOT-01 as "Pending" while the requirement-line at the top (line 12) is checked `[x]`. Cosmetic inconsistency in REQUIREMENTS.md — implementation is complete; only the traceability table cell needs updating to "Complete". Not a blocker; not a `human_needed` either.

### Anti-Patterns Found

| File                                                                | Line                  | Pattern                                                          | Severity   | Impact                                                                              |
| ------------------------------------------------------------------- | --------------------- | ---------------------------------------------------------------- | ---------- | ----------------------------------------------------------------------------------- |
| `Assets/_Project/Scripts/Core/Stubs/Null*Service.cs`                | various               | No-op implementations + `Debug.Log` boundary                     | Info       | Intentional per D-09. Real implementations land in Phases 6/9/10. Not a stub-defect. |
| `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs` (committed) | full file (D-22)      | Editor-only headless scene builder retained in tree              | Info       | Acceptable per D-22 — idempotent reproducible-build infra. Cleanup deferred to Phase 11. |
| `Assets/_Project/Scripts/Editor/Phase01CameraUiBuilder.cs` (committed) | full file (D-25)   | Editor-only headless camera+UI builder retained in tree          | Info       | Acceptable per D-25 — sibling pattern to D-22. Cleanup deferred to Phase 11.         |
| `Assets/Settings/` template leftovers                               | external folder       | DefaultVolumeProfile.asset referenced by URPGlobalSettings        | Info       | Acceptable — deletion would break URP load. Tracked as deferred cleanup.            |
| `ProjectSettings/ProjectSettings.asset` working tree (post-test)    | line 532              | `m_Automatic: 0 → 1` flips on every `Unity -runTests`             | Info       | HEAD state has `m_Automatic: 0` (correct). Recurring Unity test-runner regression. iOS build path unaffected (BuildScript passes explicit BuildTarget.iOS). |

No TODO/FIXME/XXX/HACK/PLACEHOLDER comments in production code. No empty `return null`/`return {}`/`return []` stub returns in WM.Core.

### Human Verification Required

1. **Editor Play visual gate**
   - **Test:** Open `Assets/_Project/Scenes/Warehouse_MVP.unity` in Unity Editor 6000.4.6f1, press Play.
   - **Expected:** Console logs `"GameManager initialized"` exactly once with no NullReferenceException; Game view shows the floor + 6 colored stations (loading dock grey, packing blue, delivery green, upgrade yellow, shelf brown, worker spawn magenta) framed from the (10, 12, -10) Cinemachine vcam at 45deg pitch; UI overlay empty but SafeAreaPanel rect non-zero.
   - **Why human:** ROADMAP Success Criterion 1 requires visual confirmation of camera framing + station coloring; CLI grep cannot validate render output.

2. **iOS build + simulator run**
   - **Test:** Run `Unity -batchmode -quit -projectPath . -buildTarget iOS -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -`; open `build/ios/Unity-iPhone.xcodeproj` in Xcode 15+; build to iOS Simulator (iPhone 15 or later).
   - **Expected:** CLI exits 0; Xcode build succeeds; simulator boots to the empty warehouse scene rendered from the Cinemachine vcam framing.
   - **Why human:** ROADMAP Success Criterion 3 is explicitly deferred in 01-03-SUMMARY.md Deferred Issue 3. Cannot validate Xcode archive + simulator launch from a shell verifier.

### Deviations Documented (Accepted)

The following deviations from the original plan are documented in 01-CONTEXT.md / SUMMARYs and accepted per the verifier's context notes — they do NOT count against the score:

| ID    | Deviation                                                                                                                                                            | Accepted Source                                                                            |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| D-21  | Unity 6000.4.6f1 used instead of plan-pinned 6000.3.7f1 (6.4 LTS only available in Unity Hub at execution time)                                                       | User-approved verbally 2026-05-10 (recorded in STATE.md Decisions; verifier context note)  |
| D-22  | `Phase01SceneBuilder.cs` retained as committed Editor-only infra (headless scene authoring)                                                                          | Executor decision; verifier context note "kept committed per executor decision"            |
| D-23  | Material URP/Lit verification switched from string-grep to GUID-grep (URP/Lit GUID 933532a4fcc9baf4fa0491de14d08ed7)                                                  | Documented as D-23 in STATE.md Decisions                                                   |
| D-24  | Cinemachine 3.1.6 serializes Lens as `Lens:` (not `m_Lens:`); ModeOverride is int-enum (Perspective=2)                                                                 | Documented as D-24; verified against Library/PackageCache CinemachineCamera.cs:65          |
| D-25  | `Phase01CameraUiBuilder.cs` retained as sibling Editor-only builder (D-22 reaffirmed for plan 01-03)                                                                  | Documented as D-25; verifier context note                                                  |
| —     | `Assets/Settings/` template URP leftovers retained (DefaultVolumeProfile referenced by URPGlobalSettings — deletion would break URP)                                  | Deferred cleanup; verifier context note "Acceptable"                                       |
| —     | iOS `m_Automatic` regression (Unity test runner flips 0→1 on every `-runTests`)                                                                                       | Recurring Unity quirk; HEAD has correct `0` value; iOS build unaffected (verifier context note "ONLY flag if committed HEAD state diverges from plan" — HEAD is correct) |
| —     | Renderer override on Main Camera not set (defaults to URP_Mobile_Renderer via the URP asset's default)                                                                 | Documented in 01-03-SUMMARY.md; semantically equivalent                                    |
| —     | Test Framework resolved to `1.6.0` instead of manifest pin `2.0.1-pre.18` (pre-release not yet published for 6.4 LTS)                                                  | Non-blocking; tests pass on 1.6.0                                                          |

### Gaps Summary

Phase 1 walking-skeleton goals are fully achieved at the codebase level: 18 of 19 must-haves verified by direct code/scene/test inspection, plus re-ran the test suites today (2026-05-11T09:12-09:14Z) confirming EditMode 57/57 + PlayMode 1/1 green. The Bootstrap → GameManager composition root, Cinemachine 3.x rig, UICanvas Screen-Space-Camera, SafeAreaPanel, EventSystem with InputSystemUIInputModule, URP_Mobile pipeline, iOS Player Settings, and repo hygiene files are all in place and correctly wired.

The remaining gap (Phase 1 Success Criterion 3 — iOS build runs on simulator/device and shows the empty warehouse scene from the configured camera) is **NOT a codebase failure** but a deferred manual verification step. It requires opening Unity Editor (visual confirmation) and running the Xcode + iOS Simulator pipeline, which is outside what a shell verifier can attest. The phase is otherwise complete; STATE.md update to `phase_complete` should wait until the human-verification items pass.

---

_Verified: 2026-05-11T09:18:00Z_
_Verifier: Claude (gsd-verifier)_
