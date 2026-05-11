---
phase: 02
slug: walk-around-the-warehouse
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-05-11
---

# Phase 02 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Test Framework (NUnit) — EditMode + PlayMode |
| **Config file** | `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef`, `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` |
| **Quick run command** | `unity -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` |
| **Full suite command** | `unity -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -` (PlayMode runs full set after EditMode passes) |
| **Estimated runtime** | EditMode ~30s; PlayMode ~90s (Unity cold-start dominated) |

---

## Sampling Rate

- **After every task commit:** Run EditMode suite
- **After every plan wave:** Run EditMode + PlayMode suites
- **Before `/gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** ~120s (PlayMode wall-clock)

---

## Per-Task Verification Map

> Populated by planner. Each task maps to a `Test Type` (unit / playmode / manual), an `Automated Command`, and a `File Exists` check (✅ if covered by Wave 0, ❌ otherwise).

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| {02-XX-YY} | XX | W | MOVE-01 / MOVE-02 | — | N/A (no external attack surface in this phase) | unit / playmode | `unity -batchmode -runTests -testPlatform EditMode -testFilter WM.Tests.*` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Wave 0 establishes test infrastructure before any production code lands. Per RESEARCH §Validation Architecture:

- [ ] `Assets/Tests/EditMode/VirtualJoystickMathTests.cs` — pure-function tests for joystick screen→local→direction math (dead zone, magnitude clamp, normalization)
- [ ] `Assets/Tests/EditMode/CameraRelativeProjectionTests.cs` — `Vector3.ProjectOnPlane` derivation tests; verify camera pitch ~45° produces expected forward/right basis; zero-input safety
- [ ] `Assets/Tests/EditMode/PlayerStatsTests.cs` — `PlayerStats` instantiates from `PlayerConfig` with expected `MoveSpeed`, `TurnRateDegPerSec`; mutation surface honors Phase 5 upgrade contract
- [ ] `Assets/Tests/PlayMode/PlayerMovementSmokeTests.cs` — load Warehouse_MVP scene; drive `VirtualJoystick.Direction` synthetically; assert `Player.transform.position` changes along expected camera-relative vector
- [ ] `Assets/Tests/PlayMode/PlayerCollisionSmokeTests.cs` — drive player into a station box and an invisible perimeter wall; assert position does not penetrate collider (within `CharacterController.skinWidth`)
- [ ] `Assets/Tests/PlayMode/CameraFollowSmokeTests.cs` — assert `CinemachineCamera.Follow == Player`; assert active body is `CinemachinePositionComposer`; assert `CinemachineConfiner3D` BoundingVolume is assigned
- [ ] Update `Assets/Tests/EditMode/BootstrapSmokeTests.cs::CinemachineCamera_IsConfigured` — relax the `Follow == null` assertion (Phase 01 D-12 invariant intentionally inverted by Phase 02)
- [ ] Update `WM.Tests.PlayMode.asmdef` to reference `WM.Player`, `WM.UI`, `Unity.Cinemachine` (verify edges per RESEARCH §asmdef map)
- [ ] Add `[assembly: InternalsVisibleTo("WM.Tests.PlayMode")]` to `WM.Player` if PlayerStats / internal hooks need test reach

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Joystick feel on physical iPhone (thumb reach, drag latency, dead-zone comfort) | MOVE-01 | Touch ergonomics not reproducible in batchmode | Run iOS device build; drag joystick 30s; confirm responsive, no stuck handle, comfortable in bottom-left |
| Camera framing on 1080×1920 portrait at iPhone-15 aspect | MOVE-02 | Visual framing judgment | Walk player to each station; confirm player ~10% below center, no void visible, stations remain readable |
| `iOSSupport.m_BuildTargetGraphicsAPIs.m_Automatic` regression check after PlayMode run | — | Unity tooling flips field 0→1 (STATE.md known regression) | After full suite run, `git diff ProjectSettings/ProjectSettings.asset`; restore manually if flipped, document in plan SUMMARY |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 120s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
