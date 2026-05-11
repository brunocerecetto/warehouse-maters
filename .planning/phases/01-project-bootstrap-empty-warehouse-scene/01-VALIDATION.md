---
phase: 1
slug: project-bootstrap-empty-warehouse-scene
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-10
---

# Phase 1 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Test Framework 2.0.x (NUnit + UnityEngine.TestRunner) — bundled with Unity 6.3 LTS |
| **Config file** | `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef`, `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` (created in Wave 0) |
| **Quick run command** | `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` |
| **Full suite command** | EditMode then PlayMode: `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile - && <UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -` |
| **Estimated runtime** | ~30 s EditMode, ~60 s PlayMode (cold open) |

> Replace `<UNITY>` with the editor binary matching `ProjectSettings/ProjectVersion.txt` (`6000.3.7f1`). On macOS: `/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity`.

---

## Sampling Rate

- **After every task commit:** Run quick command (EditMode only).
- **After every plan wave:** Run full suite (EditMode + PlayMode).
- **Before `/gsd-verify-work`:** Full suite green AND a successful manual `BuildScript.BuildIOS` invocation that produces `build/ios/Unity-iPhone.xcodeproj`.
- **Max feedback latency:** ~30 s (EditMode); ~90 s (full suite).

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 01-01-* | 01 | 0 | BOOT-01 | T-01-04 (build artifacts ignored) | `.gitignore` excludes `Library/`, `Temp/`, `build/`, signing certs | structure | `BootstrapStructureTests.AssetsProject_FoldersExist` | ❌ W0 — `BootstrapStructureTests.cs` | ⬜ pending |
| 01-01-* | 01 | 0 | BOOT-01 | — | All 13 production + 2 test asmdefs present | structure | `BootstrapStructureTests.AsmdefsPresent` | ❌ W0 | ⬜ pending |
| 01-01-* | 01 | 0 | BOOT-01 | — | URP asset configured in Quality settings | structure | `BootstrapStructureTests.UrpAssetAssigned` | ❌ W0 | ⬜ pending |
| 01-02-* | 02 | 1 | BOOT-02 | T-01-03 (LFS/`.meta` integrity) | All 12 required GameObjects exist in Warehouse_MVP scene | unit (EditMode) | `BootstrapSmokeTests.RequiredGameObject_IsPresent` parameterized | ❌ W0 — `BootstrapSmokeTests.cs` | ⬜ pending |
| 01-02-* | 02 | 1 | BOOT-02 | — | Scene loads at runtime, GameManager logs "GameManager initialized" | smoke (PlayMode) | `PlayModeSmokeTests.Scene_Loads_GameManagerInitializes` | ❌ W0 — `PlayModeSmokeTests.cs` | ⬜ pending |
| 01-03-* | 03 | 2 | BOOT-02 | — | CinemachineCamera present in scene with isometric framing | unit (EditMode) | `BootstrapSmokeTests.CinemachineCamera_IsConfigured` | ❌ W0 | ⬜ pending |
| 01-03-* | 03 | 2 | BOOT-02 | — | UICanvas in Screen-Space Camera mode, 1080×1920 reference, SafeAreaPanel attached | unit (EditMode) | `BootstrapSmokeTests.UICanvas_IsSafeAreaConfigured` | ❌ W0 | ⬜ pending |
| 01-build | — | manual | BOOT-01 | T-01-01 (default Bundle ID) | `BuildScript.BuildIOS` exits 0 and produces `build/ios/Unity-iPhone.xcodeproj` | smoke (CLI) | `<UNITY> -batchmode -quit -buildTarget iOS -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -` | ❌ W0 (manual gate, recorded in `01-VERIFICATION.md`) | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

> Threat refs are placeholders pending PLAN.md `<threat_model>`. T-01-01 .. T-01-04 follow the four patterns in `01-RESEARCH.md` § Security Domain.

---

## Wave 0 Requirements

- [ ] `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef` — Editor platform only; `Test Assemblies` flag on; references `WM.Core`, `nunit.framework`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`.
- [ ] `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` — Any Platform; `Test Assemblies` flag on; references `WM.Core`, `nunit.framework`, `UnityEngine.TestRunner`. **Must NOT reference `UnityEditor.TestRunner`**.
- [ ] `Assets/Tests/EditMode/BootstrapStructureTests.cs` — covers BOOT-01 (folder + asmdef + URP-asset presence). Uses `AssetDatabase.IsValidFolder` and `File.Exists`.
- [ ] `Assets/Tests/EditMode/BootstrapSmokeTests.cs` — covers BOOT-02 (required GameObjects in scene, Cinemachine + UICanvas configuration). Opens scene via `EditorSceneManager.OpenScene`.
- [ ] `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` — covers BOOT-02 (runtime scene load + GameManager initialization log).
- [ ] Verify `com.unity.test-framework` present in `Packages/manifest.json` after Hub creates the project (no install action expected; bundled).

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Debug iOS build runs on simulator/device showing the empty warehouse from configured camera | BOOT-01 + BOOT-02 (success criterion 3) | Requires Xcode build + simulator/device launch — outside Unity Test Runner scope | 1) Run `<UNITY> -batchmode -quit -buildTarget iOS -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -` exits 0. 2) Open `build/ios/Unity-iPhone.xcodeproj` in Xcode. 3) Build & run on iPhone simulator (default scheme). 4) Confirm warehouse scene renders from configured Cinemachine camera. Record outcome in `01-VERIFICATION.md`. |
| URP Mobile-aggressive preset values match D-02 (MSAA off, no realtime shadows, HDR off, no post-FX) | BOOT-01 | Inspector-only audit; not assertable cheaply | Open URP asset, screenshot/inspect each setting, attach to `01-VERIFICATION.md`. |

---

## Validation Sign-Off

- [ ] All tasks have automated verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 90 s (full suite)
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
