# Phase 2: Walk Around the Warehouse - Research

**Researched:** 2026-05-11
**Domain:** Unity 6 LTS / URP / Cinemachine 3.1.6 / Input System 1.15 — mobile virtual-joystick character locomotion + camera follow
**Confidence:** HIGH (Phase 01 substrate verified on disk, Cinemachine 3.1.6 + Input System 1.15 APIs verified against `Library/PackageCache` source, locked decisions in CONTEXT.md align with current Unity 6 conventions)

## Summary

Phase 02 implements one-thumb character locomotion on the Phase 01 substrate: a custom UGUI `VirtualJoystick` MonoBehaviour anchored to `UICanvas/SafeAreaPanel`, a `PlayerController` that reads its `Direction` each frame and drives a `CharacterController` with camera-relative `Move(...)` calls, a `PlayerConfig` ScriptableObject + `PlayerStats` runtime mirror to keep Phase 5's movement-speed upgrade decoupled from the SO asset, and Cinemachine 3.1.6 wiring (Follow target = Player, `CinemachinePositionComposer` body, `CinemachineConfiner3D` extension, four invisible perimeter wall colliders around the floor plane).

The locked decisions in `02-CONTEXT.md` are fully implementable inside Unity 6.4 LTS + Cinemachine 3.1.6 + Input System 1.15 — every component named in CONTEXT.md exists in the current package (`CinemachinePositionComposer.cs`, `CinemachineConfiner3D.cs`, `RectTransformUtility.ScreenPointToLocalPointInRectangle`, `CharacterController.Move`). Phase 01 already provisioned the Cinemachine vcam, UICanvas, EventSystem, asmdefs, and ScriptableObject root, so Phase 02 is wholly additive — no Phase 01 GameObject changes except the Player primitive (which has a Rigidbody that **must be removed** before adding `CharacterController`, per finding [VERIFIED: Phase01SceneBuilder.cs:141-143]).

**Primary recommendation:** Author Phase 02 as two Editor builders (one for the Player + collision substrate, one for the Cinemachine + UI wiring) following the Phase 01 D-22/D-25 sibling pattern; keep all joystick math + camera-relative projection extracted into pure-function static helpers so EditMode tests cover them without a PlayMode scene.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Touch input → Direction vector | UI / UGUI (`WM.UI`) | — | UGUI EventSystem owns `IPointer*` handler dispatch; joystick is a UI widget by definition (D-03). |
| Direction → world-space velocity | Player domain (`WM.Player`) | — | Pure C# math + per-frame `MonoBehaviour.Update`; no UI concern. |
| Camera-relative projection | Player domain (`WM.Player`) | — | Reads `Camera.main.transform.forward/right` once per frame; projection math co-located with the consumer (D-06). |
| Character motion & collision | Player domain (`WM.Player`) | Unity Physics (Built-in module) | `CharacterController` is a kinematic capsule from the built-in Physics module; physics queries handled by engine. |
| Camera follow + framing | Cinemachine (3rd-party but locked dependency) | Scene authoring (Editor) | `CinemachineCamera` + `CinemachinePositionComposer` own follow math; Editor builder wires Follow target + composer values. |
| Camera bounds clipping | Cinemachine (`CinemachineConfiner3D`) | Scene authoring (Editor) | Extension on the vcam; bounds GameObject is scene-authored. |
| Speed configuration data | ScriptableObject asset (`WM.Player`) | — | `PlayerConfig` SO at `Assets/_Project/ScriptableObjects/Player/PlayerConfig.asset` (D-14). |
| Runtime speed mutation | Plain C# class (`WM.Player.PlayerStats`) | — | Mutable mirror of `PlayerConfig`, instantiated in `Awake`; SO never dirtied (D-16). |
| Wall / station collision response | Unity Physics + `CharacterController.Move` return | — | Engine handles `CollisionFlags`; no per-station code (D-17, D-18). |

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `com.unity.cinemachine` | 3.1.6 | Camera follow + framing + bounds | Locked by Phase 01 D-12 + D-21; `CinemachinePositionComposer` and `CinemachineConfiner3D` are the modern 3.x replacements for `CinemachineFramingTransposer` and `CinemachineConfiner`. [VERIFIED: `Library/PackageCache/com.unity.cinemachine@285f38545487/CHANGELOG.md:328` "CinemachineConfiner is deprecated. New behaviour CinemachineConfiner3D"] |
| `com.unity.inputsystem` | 1.15.0 | Touch input dispatch via `InputSystemUIInputModule` | Locked by Phase 01 D-03 + D-16; EventSystem already configured. `IPointerDownHandler` / `IDragHandler` / `IPointerUpHandler` fire correctly out of the box. [VERIFIED: Phase01CameraUiBuilder.cs:166-169] |
| `UnityEngine.CharacterController` | Built-in (Physics module) | Kinematic capsule movement with built-in collision | Standard for hybrid-casual movement when player and AI use different controllers (D-05); avoids Rigidbody jitter. [CITED: docs.unity3d.com/ScriptReference/CharacterController.Move.html] |
| `UnityEngine.UI` (UGUI) | 2.0.0 | `Image` background + `Image` handle for joystick visuals | Already imported (Phase 01); `Image` supports `Graphic.raycastTarget` for pointer events. |
| `RectTransformUtility.ScreenPointToLocalPointInRectangle` | Built-in | Screen-space → joystick-local conversion | Standard helper for Screen Space - Camera canvases; must pass `PointerEventData.pressEventCamera` (NOT `null`) for Phase 01's Screen Space - Camera canvas. [CITED: docs.unity3d.com/ScriptReference/RectTransformUtility.ScreenPointToLocalPointInRectangle.html] |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `Unity.TestFramework` | 1.6.0 (resolved) | EditMode + PlayMode unit/integration tests | Existing Phase 01 pattern (`BootstrapStructureTests`, `PlayModeSmokeTests`). Extend, don't replace. |
| `UnityEditor.SceneManagement.EditorSceneManager` | Built-in (Editor) | Headless scene authoring inside Phase 02 builders | Established by Phase 01 D-22 / D-25. |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Custom `VirtualJoystick` (D-03 locked) | `UnityEngine.InputSystem.OnScreen.OnScreenStick` (ships in `com.unity.inputsystem`) | OnScreenStick would route through the Input Action asset (extra config + an Action asset to maintain). D-03 chose direct property read — keeps Phase 02 surface minimal. [VERIFIED: ctx7 docs `/websites/unity3d_packages_com_unity_inputsystem_1_15` confirms OnScreenStick exists] |
| `CharacterController` (D-05 locked) | `Rigidbody` + `Rigidbody.MovePosition` | Rigidbody competes with the existing Player primitive's `Rigidbody useGravity=true` from Phase 01 D-11; would need physics material tuning to avoid slide. CC is friction-free by design. [VERIFIED: Phase01SceneBuilder.cs:141-143 — Phase 01 added a non-kinematic Rigidbody that must be removed.] |
| `CinemachinePositionComposer` (D-10 locked) | Deprecated `CinemachineFramingTransposer` | Lives in `Runtime/Deprecated/` in 3.1.6; would emit upgrader warnings and migration prompts. [VERIFIED: `Library/PackageCache/com.unity.cinemachine@285f38545487/Runtime/Deprecated/CinemachineFramingTransposer.cs`] |
| `CinemachineConfiner3D` (D-13 locked) | `CinemachineConfiner2D` | 2D variant requires a `PolygonCollider2D` or `CompositeCollider2D`; the warehouse is a 3D scene with a `Plane` floor. Confiner3D matches the scene's collider topology. [VERIFIED: `CinemachineConfiner3D.cs:21` requires `Collider BoundingVolume`] |
| Asmdef cross-ref `WM.Player → WM.UI` | Interface `IJoystick` in `WM.Core` | D-04 chose a direct serialized reference. The `WM.Player → WM.UI` direction is unidirectional and acceptable; Phase 5's upgrade comment in CONTEXT.md does not affect joystick decoupling. The existing `WM.UI` asmdef already references `WM.Core`, `WM.Economy`, `WM.Orders`, `WM.Upgrades` — adding a back-edge `WM.Player → WM.UI` does not introduce cycles. |

**Installation:** No new packages required — all dependencies were locked in Phase 01.

**Version verification:**
- Cinemachine: [VERIFIED: `Packages/manifest.json` pins `3.1.6`; `packages-lock.json` confirms resolved `3.1.6`]
- Input System: [VERIFIED: `Packages/manifest.json` pins `1.15.0`; resolved `1.15.0`]
- Test Framework: [VERIFIED: manifest pins `2.0.1-pre.18`, resolved `1.6.0` per Phase 01 STATE.md blocker note — non-blocking, tests pass on 1.6.0]
- `com.unity.modules.physics 1.0.0` — present in manifest, so `CINEMACHINE_PHYSICS` is auto-defined and `CinemachineConfiner3D` compiles. [VERIFIED: `Unity.Cinemachine.asmdef:46-49`]

## Architecture Patterns

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                      Phase 02 Runtime Data Flow                      │
└─────────────────────────────────────────────────────────────────────┘

   [Touch Input]
        │
        ▼
┌──────────────────────┐    InputSystemUIInputModule
│   EventSystem        │  ──> dispatches IPointer*/IDrag events
└─────────┬────────────┘
          │ OnPointerDown / OnDrag / OnPointerUp
          ▼
┌──────────────────────────────────────────────────────┐
│  WM.UI.VirtualJoystick   (UICanvas/SafeAreaPanel)    │
│  - Image background (raycastTarget=true)             │
│  - Image handle (visual only)                        │
│  - State: Vector2 Direction { get; private set; }    │
│  - Pure math: ScreenPointToLocalPointInRectangle     │
│           → clamp magnitude → dead-zone → normalize  │
└──────────────────────────────┬───────────────────────┘
                               │ joystick.Direction
                               │ (read each Update)
                               ▼
┌──────────────────────────────────────────────────────┐
│  WM.Player.PlayerController  (on Player GameObject)  │
│  [SerializeField] VirtualJoystick joystick           │
│  [SerializeField] PlayerConfig config                │
│  [SerializeField] CharacterController controller     │
│  PlayerStats stats; // mutable runtime mirror        │
│                                                      │
│  Update():                                           │
│    1. dir2 = joystick.Direction                      │
│    2. (fwd,right) = camera-relative ground projection│
│    3. horizontal = (right*dir2.x + fwd*dir2.y) * spd │
│    4. vertical.y += Physics.gravity.y * dt           │
│    5. controller.Move((horizontal+vertical)*dt)      │
│    6. if (horizontal != 0) Slerp rotation toward it  │
└──────────────────────────────┬───────────────────────┘
                               │ Player.transform moved
                               ▼
┌──────────────────────────────────────────────────────┐
│  CinemachineCamera (CM vcam1 under CameraRig)        │
│  Follow = Player.transform                            │
│  Body   = CinemachinePositionComposer                 │
│           - CameraDistance ≈ vcam offset magnitude    │
│           - Damping = (0.5, 0.5, 0.5)                 │
│           - Composition.ScreenPosition.y ≈ +0.10      │
│             (player ~10% above center → appears low)  │
│           - Lens.ModeOverride = Orthographic, size=6  │
│  Extension = CinemachineConfiner3D                    │
│              BoundingVolume = CameraBounds.BoxCollider│
└──────────────────────────────┬───────────────────────┘
                               │ CinemachineBrain drives
                               ▼
                       ┌───────────────────┐
                       │   Main Camera     │
                       │   (orthographic)  │
                       └───────────────────┘

Collision substrate (passive, queried by CharacterController.Move):
  - Existing station BoxColliders (D-17, not isTrigger)
  - 4 invisible perimeter walls (empty GO + BoxCollider, layer=Default, D-18)
```

### Recommended Project Structure

```
Assets/_Project/
├── Scripts/
│   ├── Player/                       # WM.Player asmdef (already exists)
│   │   ├── VirtualJoystick.cs        # ❌ NO — joystick lives in WM.UI per D-03
│   │   ├── PlayerController.cs       # NEW — kinematic CC driver
│   │   ├── PlayerStats.cs            # NEW — plain C# runtime stats
│   │   ├── PlayerConfig.cs           # NEW — ScriptableObject definition
│   │   └── CameraRelativeMotion.cs   # NEW (optional) — pure-math helper for EditMode tests
│   ├── UI/                           # WM.UI asmdef (already exists)
│   │   └── VirtualJoystick.cs        # NEW — IPointer*/IDrag MonoBehaviour
│   └── Editor/                       # WM.Editor asmdef (already exists)
│       ├── Phase02PlayerWiring.cs    # NEW — wires CC + PlayerController on Player, removes Rigidbody
│       ├── Phase02CameraConfigurator.cs # NEW — Follow=Player, swap to PositionComposer, add Confiner3D
│       ├── Phase02JoystickBuilder.cs # NEW — creates joystick GameObject under SafeAreaPanel
│       └── Phase02CollisionBuilder.cs # NEW — creates 4 perimeter walls + CameraBounds GO
├── ScriptableObjects/
│   └── Player/                       # NEW subfolder (per D-14)
│       └── PlayerConfig.asset        # NEW — baseMoveSpeed=5, turnRateDegPerSec=720
└── Scenes/
    └── Warehouse_MVP.unity           # Phase 01 scene — mutated by Phase 02 builders
```

**Asmdef impact (verified):**
- `WM.Player.asmdef` already references `WM.Core` [VERIFIED: read from disk]. Phase 02 must add `"WM.UI"` to its `references` array so `PlayerController` can hold a typed `VirtualJoystick` field.
- `WM.UI.asmdef` already references `WM.Core, WM.Economy, WM.Orders, WM.Upgrades` [VERIFIED: read from disk]. No back-edge to `WM.Player` is needed — keeps dependency unidirectional `WM.Player → WM.UI`.
- `WM.Tests.EditMode.asmdef` already references `WM.Core, WM.UI, Unity.Cinemachine, Unity.InputSystem, UnityEngine.UI` [VERIFIED]. Add `"WM.Player"` so tests can construct `PlayerStats` / load `PlayerConfig`.
- `WM.Tests.PlayMode.asmdef` only references `WM.Core, UnityEngine.TestRunner` [VERIFIED]. Add `"WM.Player"` and `"WM.UI"` so PlayMode smoke tests can `Find` the joystick + assert player motion.

### Pattern 1: Custom Virtual Joystick (Screen Space - Camera canvas)

**What:** A MonoBehaviour implementing the three UGUI pointer interfaces, with all math contained in pure helpers so it's testable without a running EventSystem.

**When to use:** Always for Phase 02 — locked by D-03.

**Example:**
```csharp
// Source: D-03/D-04 locked decisions + RectTransformUtility docs (Unity 2026.x).
// File: Assets/_Project/Scripts/UI/VirtualJoystick.cs  (asmdef: WM.UI)
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WM.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class VirtualJoystick : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background; // self (or stretched child)
        [SerializeField] private RectTransform handle;     // child Image, anchored center

        [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.1f;

        // Public read surface for PlayerController (D-04 direct property read).
        // x = right, y = up, magnitude in [0,1].
        public Vector2 Direction { get; private set; }

        public void OnPointerDown(PointerEventData eventData)  => UpdateDirection(eventData);
        public void OnDrag(PointerEventData eventData)         => UpdateDirection(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }

        private void UpdateDirection(PointerEventData eventData)
        {
            // CRITICAL: pass pressEventCamera (NOT null) — Phase 01 canvas is Screen Space - Camera.
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, eventData.pressEventCamera,
                    out Vector2 local))
            {
                return;
            }

            // Map local-space point to [-1,1] joystick space using background half-size.
            Vector2 half = background.rect.size * 0.5f;
            Vector2 raw  = new Vector2(local.x / half.x, local.y / half.y);

            // Pure math — extracted for EditMode unit tests.
            Direction = ApplyDeadZoneAndClamp(raw, deadZone);

            // Visual handle follows direction, clamped to the background radius.
            handle.anchoredPosition = new Vector2(Direction.x * half.x, Direction.y * half.y);
        }

        // Pure static helper — covered by EditMode tests with no EventSystem needed.
        public static Vector2 ApplyDeadZoneAndClamp(Vector2 raw, float deadZone)
        {
            float mag = raw.magnitude;
            if (mag < deadZone) return Vector2.zero;
            if (mag > 1f) return raw / mag; // clamp to unit circle
            // Re-scale [deadZone..1] to [0..1] to remove dead-zone "step" at boundary.
            float scaled = (mag - deadZone) / (1f - deadZone);
            return (raw / mag) * scaled;
        }
    }
}
```

**Verified specifics:**
- `RectTransformUtility.ScreenPointToLocalPointInRectangle` requires the `Camera cam` parameter to be `PointerEventData.pressEventCamera` for Screen Space - Camera canvases [CITED: docs.unity3d.com — "Pass an actual Camera when: Using Screen Space - Camera mode"]. Passing `null` returns wrong coordinates on Screen Space - Camera, which Phase 01 D-15 uses.
- The `Image` component on the background **must have `raycastTarget = true`** for `IPointer*` events to fire. UI `Image` defaults to `raycastTarget = true`.
- Phase 01's `InputSystemUIInputModule` on the EventSystem is the right pairing — it dispatches touch events to UGUI pointer handlers identically to mouse. [VERIFIED: Phase01CameraUiBuilder.cs:166-169 + ctx7 docs]

### Pattern 2: PlayerController with CharacterController + camera-relative projection

**What:** A small MonoBehaviour that composes joystick input, camera basis vectors, gravity, and rotation into a single `CharacterController.Move(...)` call per frame.

**When to use:** Always for Phase 02 — locked by D-05 through D-09.

**Example:**
```csharp
// Source: D-05–D-09 + Unity docs (CharacterController.Move composition pattern).
// File: Assets/_Project/Scripts/Player/PlayerController.cs  (asmdef: WM.Player)
using UnityEngine;
using WM.UI;

namespace WM.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick joystick;     // D-04 direct ref
        [SerializeField] private PlayerConfig config;          // D-14 SO asset
        [SerializeField] private Transform cameraTransform;    // assigned in builder (Camera.main.transform)

        private CharacterController _cc;
        private PlayerStats _stats;
        private float _verticalVelocity;

        public PlayerStats Stats => _stats; // read-only exposure for Phase 5 upgrade

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            // D-16: PlayerStats is a plain C# runtime mirror; SO never mutated.
            _stats = PlayerStats.FromConfig(config);
        }

        private void Update()
        {
            Vector2 input = (joystick != null) ? joystick.Direction : Vector2.zero;
            Vector3 horizontal = CameraRelativeMotion.HorizontalVelocity(
                input, cameraTransform.forward, cameraTransform.right, _stats.MoveSpeed);

            // D-09: minimal gravity term so capsule stays grounded.
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
            if (_cc.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f; // small downward pin

            Vector3 motion = horizontal + new Vector3(0f, _verticalVelocity, 0f);
            _cc.Move(motion * Time.deltaTime);  // SINGLE Move() call per frame (D-09)

            // D-08: face movement direction via Quaternion.Slerp toward velocity.
            if (horizontal.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(horizontal, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, _stats.TurnRateDegPerSec * Time.deltaTime);
            }
        }
    }

    // Pure helper — EditMode-testable.
    public static class CameraRelativeMotion
    {
        public static Vector3 HorizontalVelocity(
            Vector2 joystick, Vector3 cameraForward, Vector3 cameraRight, float speed)
        {
            if (joystick == Vector2.zero) return Vector3.zero;

            Vector3 fwd   = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
            Vector3 right = Vector3.ProjectOnPlane(cameraRight,   Vector3.up);

            // Edge case: camera pitch ≈ 90° (top-down) collapses forward to ~zero.
            // Fall back to world Z so input is still meaningful.
            if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.forward;
            else                          fwd.Normalize();

            if (right.sqrMagnitude < 1e-6f) right = Vector3.right;
            else                            right.Normalize();

            return (right * joystick.x + fwd * joystick.y) * speed;
        }
    }
}
```

**Verified specifics:**
- `CharacterController.Move` does NOT apply gravity automatically; you must compose horizontal + vertical and call `Move()` once per frame. [CITED: docs.unity3d.com/ScriptReference/CharacterController.Move.html — "must manually manage velocity accumulation and compose horizontal/vertical components before each frame's movement call"]
- Calling `Move()` twice per frame is a documented anti-pattern — the second call doesn't get fresh `CollisionFlags` and the engine can desync. One call per frame.
- The Phase 01 Player primitive currently has a non-kinematic `Rigidbody` with `useGravity = true` [VERIFIED: Phase01SceneBuilder.cs:141-143]. `CharacterController` is incompatible with a sibling Rigidbody — Phase 02 must remove the Rigidbody in the Editor builder before adding CC.

### Pattern 3: PlayerConfig (SO) + PlayerStats (plain C#) split

**What:** ScriptableObject asset stores designer-tunable defaults; plain-C# `PlayerStats` is instantiated from it in `Awake` and is the only mutable surface. Phase 5's movement-speed upgrade will mutate `PlayerStats`, never `PlayerConfig`.

**When to use:** Always — locked by D-14, D-15, D-16.

**Example:**
```csharp
// File: Assets/_Project/Scripts/Player/PlayerConfig.cs  (asmdef: WM.Player)
using UnityEngine;

namespace WM.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "WarehouseMaster/Player/PlayerConfig", order = 0)]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Tooltip("Initial walk speed in meters/second. D-15 placeholder = 5.0.")]
        public float baseMoveSpeed = 5f;

        [Tooltip("Rotation rate in degrees/second toward velocity direction. D-15 placeholder = 720 (half-second full turn).")]
        public float turnRateDegPerSec = 720f;
    }
}

// File: Assets/_Project/Scripts/Player/PlayerStats.cs  (asmdef: WM.Player)
namespace WM.Player
{
    // Plain C# — mutable. Phase 5 mutates these fields, never PlayerConfig.
    public sealed class PlayerStats
    {
        public float MoveSpeed;
        public float TurnRateDegPerSec;

        public static PlayerStats FromConfig(PlayerConfig config)
        {
            return new PlayerStats
            {
                MoveSpeed = config.baseMoveSpeed,
                TurnRateDegPerSec = config.turnRateDegPerSec,
            };
        }
    }
}
```

### Pattern 4: Cinemachine 3.1.6 PositionComposer wiring (Editor builder)

**What:** A headless Editor builder that opens the scene, finds CM vcam1 (already created by Phase 01), assigns Follow target, adds `CinemachinePositionComposer` as the Body stage, sets damping + screen position + camera distance, and adds `CinemachineConfiner3D` as an extension.

**When to use:** Always — Phase 01 D-22 / D-25 established the headless Editor builder pattern.

**Verified API fields (read from `CinemachinePositionComposer.cs` source):**

| Field | Type | Default | Phase 02 Value | Notes |
|-------|------|---------|----------------|-------|
| `CameraDistance` | `float` | 10 | (from CM vcam1 offset magnitude, ~17.3 for the 10/12/-10 pose) | Distance maintained from target along camera Z. |
| `Damping` | `Vector3` | (1,1,1) | (0.5, 0.5, 0.5) | D-10: ~0.5s damping per axis. |
| `Composition` | `ScreenComposerSettings` | `Default` | `ScreenPosition = new Vector2(0f, +0.10f)` | **`ScreenPosition` range is -0.5..+0.5 with center=0** [VERIFIED: `ScreenComposerSettings.cs:13` "0 is screen center, and +0.5 or -0.5 is screen edge"]. CONTEXT.md D-12 says "Composer Screen Y ≈ 0.55–0.60 (player 10–20% below center)" — that wording is from the legacy `FramingTransposer` (0..1 range). **TRANSLATE to 3.x: player ~10–20% below center = `ScreenPosition.y ≈ +0.10..+0.20`** (positive Y moves the target UP on screen, so the camera positions the player ABOVE center; CONTEXT.md is asking for the player to appear LOW on screen, which means `ScreenPosition.y` should be NEGATIVE → use `-0.10` to `-0.20`). **CONFIRM with playtest.** |
| `Composition.DeadZone.Enabled` | `bool` | `false` | `false` (MVP) | Off — D-10 wants tight follow. |
| `CenterOnActivate` | `bool` | `true` | `true` | Default — vcam snaps to center when activated. |
| `TargetOffset` | `Vector3` | (0,0,0) | (0,0,0) | Default. |

[VERIFIED] The `Composition` field is a struct (`ScreenComposerSettings`). Assigning it directly via C# works (no `SerializedObject` ceremony needed because `Composition` is `public` on `CinemachinePositionComposer`).

**Example:**
```csharp
// File: Assets/_Project/Scripts/Editor/Phase02CameraConfigurator.cs  (asmdef: WM.Editor)
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WM.Editor
{
    public static class Phase02CameraConfigurator
    {
        private const string ScenePath = "Assets/_Project/Scenes/Warehouse_MVP.unity";

        [MenuItem("Tools/Phase02/Configure Camera Follow (Plan 02-0X)")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Find Phase 01 artifacts.
            CinemachineCamera vcam = Object.FindFirstObjectByType<CinemachineCamera>();
            if (vcam == null) throw new System.InvalidOperationException("CM vcam1 missing — run Phase01CameraUiBuilder.Build first.");

            GameObject player = GameObject.Find("Player");
            if (player == null) throw new System.InvalidOperationException("Player missing — run Phase01SceneBuilder.Build first.");

            // 1. Set Follow target (Tracking Target in CM 3.x → CameraTarget.TrackingTarget).
            vcam.Follow = player.transform;  // setter writes Target.TrackingTarget

            // 2. Swap lens to Orthographic (D-11).
            LensSettings lens = vcam.Lens;
            lens.ModeOverride     = LensSettings.OverrideModes.Orthographic;
            lens.OrthographicSize = 6f; // D-11 placeholder
            vcam.Lens = lens;

            // 3. Remove any prior Body component (idempotency) and add PositionComposer.
            //    CM 3.x: components live as Components on the vcam GameObject, NOT as a sub-pipeline.
            foreach (var existing in vcam.GetComponents<CinemachineComponentBase>())
            {
                Object.DestroyImmediate(existing);
            }
            var composer = vcam.gameObject.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = Vector3.Distance(vcam.transform.position, player.transform.position);
            composer.Damping        = new Vector3(0.5f, 0.5f, 0.5f);

            // D-12: player ~10% below screen center. In CM 3.x ScreenPosition is -0.5..+0.5.
            // Negative Y => target rendered BELOW screen center (camera lifts up).
            ScreenComposerSettings comp = ScreenComposerSettings.Default;
            comp.ScreenPosition = new Vector2(0f, -0.10f); // TUNE in playtest
            composer.Composition = comp;

            // 4. Confiner3D extension (D-13). Bounds collider provisioned by Phase02CollisionBuilder.
            // Remove any prior extension to keep idempotent.
            foreach (var ext in vcam.GetComponents<CinemachineExtension>())
            {
                Object.DestroyImmediate(ext);
            }
            var confiner = vcam.gameObject.AddComponent<CinemachineConfiner3D>();
            GameObject bounds = GameObject.Find("CameraBounds");
            if (bounds != null) confiner.BoundingVolume = bounds.GetComponent<BoxCollider>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }
    }
}
```

**Verified specifics:**
- CM 3.x stores body/aim components as **sibling `MonoBehaviour` components on the same GameObject** as the `CinemachineCamera`. Adding `CinemachinePositionComposer` via `AddComponent<>` is the correct API. [VERIFIED: `CinemachinePositionComposer.cs:25 [CameraPipeline(CinemachineCore.Stage.Body)]`]
- Setting `vcam.Follow = player.transform` writes through to the internal `Target.TrackingTarget` field. [VERIFIED: `CinemachineCamera.cs:107-111`]
- `Composition` field is `public` and can be assigned directly. [VERIFIED: `CinemachinePositionComposer.cs:47`]
- The Phase 01 vcam currently uses **`Perspective` FOV 40** [VERIFIED: Phase01CameraUiBuilder.cs:36, 111-114 and BootstrapSmokeTests.cs:62 — `LensSettings.FieldOfView == 40`]. **Phase 02 will change `ModeOverride` to `Orthographic` + set `OrthographicSize` — this WILL fail the existing test `CinemachineCamera_IsConfigured`** (asserts FOV==40). Plan must update that test.

### Pattern 5: Invisible perimeter walls + camera bounds (Editor builder)

**What:** Empty GameObjects with `BoxCollider` components, no renderer, parented under a `WarehouseColliders` empty root for organization. Coordinates derived from the Phase 01 Floor plane (positioned at `(0,0,0)` with `localScale = (3,1,3)` — Unity Plane default mesh is 10×10, so effective footprint = 30×30 with center at origin) [VERIFIED: Phase01SceneBuilder.cs:120-124].

**When to use:** Always — locked by D-18 (perimeter walls) and D-13 (camera bounds).

**Floor footprint math:**
- Unity primitive `PrimitiveType.Plane` has a default mesh size of 10 × 10 world units.
- Phase 01 floor `localScale = (3, 1, 3)` → effective footprint **30 × 30** centered at origin.
- Playable area extends from `-15` to `+15` on X and Z.
- Floor top surface at Y = 0.

**Example:**
```csharp
// File: Assets/_Project/Scripts/Editor/Phase02CollisionBuilder.cs  (asmdef: WM.Editor)
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WM.Editor
{
    public static class Phase02CollisionBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Warehouse_MVP.unity";
        private const float HalfFloor = 15f;       // 30x30 floor → ±15 perimeter
        private const float WallHeight = 3f;
        private const float WallThickness = 0.5f;

        [MenuItem("Tools/Phase02/Build Perimeter + Camera Bounds (Plan 02-0X)")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Idempotent: remove prior root if it exists.
            GameObject prior = GameObject.Find("WarehouseColliders");
            if (prior != null) Object.DestroyImmediate(prior);

            GameObject root = new GameObject("WarehouseColliders");

            // 4 perimeter walls (D-18: empty GO + BoxCollider, no renderer).
            CreateWall(root.transform, "Wall_North", new Vector3(0,  WallHeight/2,  HalfFloor),
                       new Vector3(HalfFloor*2 + WallThickness, WallHeight, WallThickness));
            CreateWall(root.transform, "Wall_South", new Vector3(0,  WallHeight/2, -HalfFloor),
                       new Vector3(HalfFloor*2 + WallThickness, WallHeight, WallThickness));
            CreateWall(root.transform, "Wall_East",  new Vector3( HalfFloor, WallHeight/2, 0),
                       new Vector3(WallThickness, WallHeight, HalfFloor*2));
            CreateWall(root.transform, "Wall_West",  new Vector3(-HalfFloor, WallHeight/2, 0),
                       new Vector3(WallThickness, WallHeight, HalfFloor*2));

            // CameraBounds (D-13): a BoxCollider inside which the camera position is constrained.
            // Slightly inset from the walls so the camera doesn't show void at the floor edges.
            GameObject bounds = new GameObject("CameraBounds");
            bounds.transform.SetParent(root.transform);
            bounds.transform.position = new Vector3(0, WallHeight/2, 0);
            BoxCollider bc = bounds.AddComponent<BoxCollider>();
            bc.size = new Vector3(HalfFloor * 1.6f, WallHeight * 2f, HalfFloor * 1.6f);
            bc.isTrigger = true; // Confiner only reads geometry via ClosestPoint; trigger avoids physics interaction.

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            BoxCollider c = go.AddComponent<BoxCollider>();
            c.size = size;
            c.isTrigger = false;     // D-18: solid wall, not trigger.
            // No MeshRenderer / no MeshFilter — walls are invisible.
        }
    }
}
```

**Verified specifics:**
- `CinemachineConfiner3D.BoundingVolume` is a `Collider` (any 3D collider type works); `BoxCollider` is the simplest choice. [VERIFIED: `CinemachineConfiner3D.cs:21`]
- `ClosestPoint` (used internally by Confiner3D) works correctly on triggers — `isTrigger = true` on `CameraBounds` is safe and avoids accidental physics collisions with the player capsule. [VERIFIED: `CinemachineConfiner3D.cs:120-126`]
- Phase 01 D-17 stations are `PrimitiveType.Cube` — `GameObject.CreatePrimitive` automatically adds a non-trigger `BoxCollider` [VERIFIED: standard Unity behavior, no special flag set in Phase01SceneBuilder.cs:127-131]. So station colliders are already correct; Phase 02 only needs to **verify** none of them have `isTrigger = true` (an EditMode test is straightforward).

### Anti-Patterns to Avoid

- **Calling `CharacterController.SimpleMove` + `Move` in the same frame.** `SimpleMove` already applies gravity and consumes the same `CollisionFlags` machinery; combining them produces inconsistent results. Use `Move` only.
- **Reading `joystick.Direction` in `FixedUpdate`.** Pointer events fire on the main thread tied to the Update loop; reading in FixedUpdate introduces a 1-frame lag and visible "step" jitter. `Update` is correct for `CharacterController.Move`.
- **Calling `Quaternion.LookRotation(Vector3.zero, ...)`.** Throws a "Look rotation viewing vector is zero" error and breaks the rotation. Guard with `horizontal.sqrMagnitude > 0.0001f` before rotating.
- **Passing `null` to `ScreenPointToLocalPointInRectangle` on a Screen Space - Camera canvas.** Returns wrong local coordinates. Always pass `eventData.pressEventCamera` (or `Canvas.worldCamera` if you have the canvas reference). [CITED: docs.unity3d.com — null is for Overlay only]
- **Setting `Composition.ScreenPosition.y = 0.55f` (legacy `FramingTransposer` value range).** In CM 3.x `ScreenComposerSettings`, the legal range is `-1.5..+1.5` with `0` = center [VERIFIED: `ScreenComposerSettings.cs:73-74` `Mathf.Clamp(ScreenPosition.x, -1.5f, 1.5f)`]. CONTEXT.md D-12's "0.55–0.60" wording corresponds to the legacy 0..1 range; translate to "≈ -0.10" in 3.x for "player slightly below center."
- **Leaving the Phase 01 `Rigidbody` on the Player primitive when adding `CharacterController`.** CC + Rigidbody on the same GameObject is a documented Unity warning — CC will be partially overridden by Rigidbody physics. The Phase 02 Editor builder must `DestroyImmediate` the Rigidbody before adding CC.
- **Using `CinemachineFramingTransposer` instead of `CinemachinePositionComposer`.** FramingTransposer is in `Runtime/Deprecated/` in 3.1.6; the upgrader recommends PositionComposer. [VERIFIED]
- **Hardcoding the bounds collider position to `(0, 0, 0)` with a tiny size.** Confiner3D constrains the **camera** position, not the player. Bounds must be sized to contain the camera's expected travel range, not the play area.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Touch → joystick coordinates | Manual touch tracking via `Input.GetTouch` or Input System actions | `RectTransformUtility.ScreenPointToLocalPointInRectangle` + IPointer handlers | Handles multi-touch routing through EventSystem, respects canvas scaling, works for mouse + touch + pen uniformly. |
| Camera follow with damping | Per-frame `Vector3.SmoothDamp` on Main Camera | `CinemachinePositionComposer` | Cinemachine handles aspect-ratio-aware screen positioning, dead-zones, hard limits, blend stacks, and re-frames automatically when the lens changes. |
| Camera bounds | AABB check + `Mathf.Clamp` per frame on camera transform | `CinemachineConfiner3D` | Confiner integrates into the CM pipeline at the right stage (Body, post-damping), supports slow-down zones, and uses `Collider.ClosestPoint` (handles non-axis-aligned and rotated bounds). |
| Capsule character motion | Manual `transform.position += delta` with raycast collision checks | `CharacterController.Move(...)` | Engine handles capsule sweep, stepOffset, slopeLimit, skinWidth, and `CollisionFlags`. Hand-rolled raycast solvers miss corner cases (tunneling at high speeds, slope traversal). |
| Visual handle clamp circle | Manually compare `magnitude > radius` and rescale, recomputing every frame | Same as Pattern 1: clamp `raw / mag` to unit and scale by `half.x`/`half.y` | Already trivial — the pattern itself IS the standard; just avoid recomputing `background.rect.size` every frame (cache in Start). |

**Key insight:** This phase is wholly inside Unity's standard ecosystem. There is no "smart-stack" or "library X vs Y" decision to make — every component named in CONTEXT.md is a verified, current, non-deprecated Unity API in the locked package versions.

## Runtime State Inventory

> Phase 02 is **additive feature work, not a rename or migration**. The inventory below is included for diligence but expected to be mostly "none."

| Category | Items Found | Action Required |
|----------|-------------|-----------------|
| Stored data | **None** — Phase 02 introduces no persistence (save system arrives in Phase 6). PlayerStats lives in memory only. | None. |
| Live service config | **None** — no external services touched in Phase 02. | None. |
| OS-registered state | **None.** | None. |
| Secrets / env vars | **None.** | None. |
| Build artifacts | The Phase 01 `Player` GameObject in `Warehouse_MVP.unity` will be **mutated** by `Phase02PlayerWiring.Build()` (Rigidbody removed, CharacterController + PlayerController added). The Phase 01 vcam `CM vcam1` will be mutated (Follow target set, Body swapped from "passive / no body" to `CinemachinePositionComposer`, extension `CinemachineConfiner3D` added, Lens switched Perspective → Orthographic). The Phase 01 test `BootstrapSmokeTests.CinemachineCamera_IsConfigured` asserts `vcams[0].Follow == null` and `Lens.FieldOfView == 40` — **both will fail after Phase 02 runs and must be updated/replaced.** | Plan must include a task that updates `BootstrapSmokeTests.CinemachineCamera_IsConfigured` (or replaces it with a Phase 02 equivalent) so the Phase 1 regression suite stays green. |

## Common Pitfalls

### Pitfall 1: Phase 01 left a Rigidbody on the Player primitive

**What goes wrong:** Phase 01's `Phase01SceneBuilder.cs:141-143` calls `player.AddComponent<Rigidbody>(); rb.useGravity = true; rb.isKinematic = false;`. When Phase 02 adds `CharacterController`, Unity logs "GameObject has both a CharacterController and a Rigidbody. The Rigidbody will be ignored for physics queries but may still react to forces, producing jitter."

**Why it happens:** The Phase 01 builder anticipated Rigidbody-based movement before the Phase 02 discussion locked `CharacterController` (D-05). The Player primitive's components weren't revisited.

**How to avoid:** The Phase 02 `Phase02PlayerWiring.Build()` builder must `DestroyImmediate(player.GetComponent<Rigidbody>())` **before** `player.AddComponent<CharacterController>()`. Idempotent: call `GetComponent` and check for null, destroy if present.

**Warning signs:** Unity log line "There is no 'Rigidbody' attached to the ..."; OR visible drift of player capsule when no input is applied; OR collision response feels mushy.

### Pitfall 2: Phase 1 BootstrapSmokeTests will fail after Phase 2 camera changes

**What goes wrong:** [VERIFIED: BootstrapSmokeTests.cs:57-58 + 62-63] The Phase 1 test asserts `vcams[0].Follow == null` (D-12: "no follow target yet") and `vcams[0].Lens.FieldOfView == 40`. Phase 02 sets `Follow = Player` (D-10) and switches lens to `Orthographic` (D-11), which renders `FieldOfView` irrelevant — Cinemachine `LensSettings.OnValidate` does not clear FOV when switching modes, but the live camera reads `OrthographicSize` not `FieldOfView`, so the literal assertion may still pass. **However:** the `Follow == null` assertion WILL fail.

**Why it happens:** Phase 1 test was written before Phase 2 wiring existed and locks the "no follow target" state of Phase 1.

**How to avoid:** Plan a task to update `BootstrapSmokeTests.CinemachineCamera_IsConfigured`:
- Drop the `Follow == null` assertion (was a Phase 1 boundary marker).
- Drop or change the FOV assertion to assert `Lens.ModeOverride == OverrideModes.Orthographic` and `Lens.OrthographicSize > 0`.
- OR move the entire test into a new `Phase02CameraTests` and have the Phase 1 test only check `CinemachineBrain` + `CinemachineCamera` count.

**Warning signs:** `Unity -runTests` returns non-zero exit code on first Phase 02 run; CI failure on what was a green commit.

### Pitfall 3: ScreenPosition range mismatch (0..1 vs -0.5..+0.5)

**What goes wrong:** Designer copy-pastes "0.55" from CONTEXT.md D-12 into `Composition.ScreenPosition.y`, intending "player at 55% screen height." In 3.x this clamps to the legal range `-1.5..+1.5` but is interpreted as "+0.55 units above screen center, where ±0.5 = screen edge." Player would render off-screen (above the top edge).

**Why it happens:** CONTEXT.md D-12's wording inherits legacy `FramingTransposer` ScreenY semantics (0..1 with 0.5 = center). CM 3.x uses `-0.5..+0.5` with 0 = center.

**How to avoid:** Document the translation in the plan task description. Use `ScreenPosition = new Vector2(0f, -0.10f)` for "player ~10% below center" (negative Y → camera lifts up → target appears low). Verify visually in playtest.

**Warning signs:** Player capsule clips or sits at screen top/bottom edge; camera framing looks dramatically wrong on first run.

### Pitfall 4: Joystick on Screen Space - Camera passing null camera

**What goes wrong:** `RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPoint, null, out local)` returns local coordinates assuming Overlay canvas. For Phase 01's Screen Space - Camera canvas (`worldCamera = Main Camera`), the result is offset and scaled incorrectly — joystick reads input but values are wrong.

**Why it happens:** Sample code online frequently uses `null` because Overlay is the simplest canvas mode.

**How to avoid:** Always pass `eventData.pressEventCamera` (set automatically by `InputSystemUIInputModule`). [VERIFIED: docs.unity3d.com — null only for Overlay]

**Warning signs:** Touching the joystick moves the handle but the resulting motion direction is wrong (e.g., touching the top moves down, or magnitude saturates immediately).

### Pitfall 5: Cinemachine 3.x components live on the vcam GameObject, not in a sub-array

**What goes wrong:** Code adapted from CM 2.x manuals tries to access `vcam.m_LookAt`, `vcam.GetCinemachineComponent<CinemachineComposer>()`, or `vcam.AddCinemachineComponent<...>()`. These APIs are removed in 3.x.

**Why it happens:** CM 3.x reworked the pipeline — body/aim/noise components are now `MonoBehaviour` instances added directly to the `CinemachineCamera` GameObject via `AddComponent`.

**How to avoid:** Use `vcam.gameObject.AddComponent<CinemachinePositionComposer>()`. Use `vcam.GetComponents<CinemachineComponentBase>()` to enumerate.

**Warning signs:** Compile error "CinemachineVirtualCamera does not contain a definition for GetCinemachineComponent" or "the type 'CinemachineComposer' could not be found."

### Pitfall 6: `iOSSupport.m_Automatic` toggles on every `Unity -runTests` invocation

**What goes wrong:** [VERIFIED: STATE.md "Blockers/Concerns" entry from Phase 01] Every `Unity -batchmode -runTests` run flips `ProjectSettings/ProjectSettings.asset` `iOSSupport.m_BuildTargetGraphicsAPIs.m_Automatic` 0 → 1. The git working tree becomes dirty even on a no-op test run.

**Why it happens:** Unity's iOS module re-detects graphics API capabilities at editor startup; the field is per-machine, not per-project, but it's serialized in tracked ProjectSettings.

**How to avoid options (choose one):**
- **A (defer, fastest):** Document the deviation in the Phase 02 SUMMARY's "Expected drift" section. Build path is unaffected per Phase 01 Plan 01-02 STATE.md note ("`BuildScript.BuildIOS` overrides via explicit `BuildTarget.iOS`"). Commit `m_Automatic = 0` manually after the final pre-commit test run.
- **B (defensive):** Add a small snapshot/restore wrapper script (`pre-test.sh` / `post-test.sh`) that captures the field before tests run and restores it after. Tracked in a future infra plan; not Phase 02 scope.

**Warning signs:** `git status` after a clean `runTests` shows `modified: ProjectSettings/ProjectSettings.asset` with only the `m_Automatic` line changed.

### Pitfall 7: PointerEventData.pressEventCamera vs enterEventCamera

**What goes wrong:** On `OnDrag`, the docs example sometimes uses `eventData.enterEventCamera`. For a drag event that started outside the joystick's hover area (player drags from background-down into another control), `enterEventCamera` may be null.

**Why it happens:** Drag events preserve the camera that was active at press time, not at drag time. The drag started with `pressEventCamera`.

**How to avoid:** Always use `pressEventCamera` in `OnDrag` callbacks. It's set when the pointer first goes down and remains stable through drag completion.

**Warning signs:** First-touch works; subsequent drag events return `pressed` null and joystick goes dead.

## Code Examples

### EditMode unit test — joystick math (pure function)

```csharp
// File: Assets/Tests/EditMode/VirtualJoystickMathTests.cs  (asmdef: WM.Tests.EditMode)
// Source: D-03 joystick contract + Pattern 1.
using NUnit.Framework;
using UnityEngine;
using WM.UI;

namespace WM.Tests.EditMode
{
    public class VirtualJoystickMathTests
    {
        [Test]
        public void ApplyDeadZoneAndClamp_BelowDeadZone_ReturnsZero()
        {
            Vector2 result = VirtualJoystick.ApplyDeadZoneAndClamp(new Vector2(0.05f, 0f), deadZone: 0.1f);
            Assert.That(result, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ApplyDeadZoneAndClamp_AboveUnitCircle_ClampedToUnit()
        {
            Vector2 result = VirtualJoystick.ApplyDeadZoneAndClamp(new Vector2(2f, 0f), deadZone: 0.1f);
            Assert.That(result.magnitude, Is.EqualTo(1f).Within(0.001f));
            Assert.That(result.x, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void ApplyDeadZoneAndClamp_AtDeadZoneBoundary_ReturnsNearZero()
        {
            Vector2 result = VirtualJoystick.ApplyDeadZoneAndClamp(new Vector2(0.1f, 0f), deadZone: 0.1f);
            Assert.That(result.magnitude, Is.LessThan(0.01f));
        }

        [Test]
        public void ApplyDeadZoneAndClamp_HalfwayBetweenDzAndUnit_ScaledLinearly()
        {
            // Raw magnitude 0.55, dead zone 0.1 → expected normalized output ≈ (0.55-0.1)/(1-0.1) = 0.5
            Vector2 result = VirtualJoystick.ApplyDeadZoneAndClamp(new Vector2(0.55f, 0f), deadZone: 0.1f);
            Assert.That(result.magnitude, Is.EqualTo(0.5f).Within(0.01f));
        }
    }
}
```

### EditMode unit test — PlayerStats from PlayerConfig

```csharp
// File: Assets/Tests/EditMode/PlayerStatsTests.cs  (asmdef: WM.Tests.EditMode)
using NUnit.Framework;
using UnityEngine;
using WM.Player;

namespace WM.Tests.EditMode
{
    public class PlayerStatsTests
    {
        [Test]
        public void FromConfig_CopiesAllFields()
        {
            PlayerConfig config = ScriptableObject.CreateInstance<PlayerConfig>();
            config.baseMoveSpeed = 7.5f;
            config.turnRateDegPerSec = 540f;

            PlayerStats stats = PlayerStats.FromConfig(config);

            Assert.That(stats.MoveSpeed, Is.EqualTo(7.5f));
            Assert.That(stats.TurnRateDegPerSec, Is.EqualTo(540f));
        }

        [Test]
        public void Mutating_Stats_DoesNotMutateConfig() // D-16 contract
        {
            PlayerConfig config = ScriptableObject.CreateInstance<PlayerConfig>();
            config.baseMoveSpeed = 5f;
            PlayerStats stats = PlayerStats.FromConfig(config);

            stats.MoveSpeed = 999f; // simulated Phase 5 upgrade

            Assert.That(config.baseMoveSpeed, Is.EqualTo(5f), "PlayerConfig SO must never be mutated at runtime");
        }
    }
}
```

### EditMode unit test — CameraRelativeMotion (degenerate cases)

```csharp
// File: Assets/Tests/EditMode/CameraRelativeMotionTests.cs
using NUnit.Framework;
using UnityEngine;
using WM.Player;

namespace WM.Tests.EditMode
{
    public class CameraRelativeMotionTests
    {
        [Test]
        public void HorizontalVelocity_ZeroInput_ReturnsZero()
        {
            Vector3 v = CameraRelativeMotion.HorizontalVelocity(
                Vector2.zero, Vector3.forward, Vector3.right, speed: 5f);
            Assert.That(v, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void HorizontalVelocity_TopDownCamera_FallsBackToWorldZ()
        {
            // Camera looking straight down (forward = -Y).
            Vector3 v = CameraRelativeMotion.HorizontalVelocity(
                new Vector2(0f, 1f), Vector3.down, Vector3.right, speed: 5f);
            Assert.That(v, Is.EqualTo(Vector3.forward * 5f), "Top-down camera must not collapse joystick-up to zero");
        }

        [Test]
        public void HorizontalVelocity_IsometricCamera_ProjectsCorrectly()
        {
            // 45° pitch camera: forward ≈ (0, -sin45, cos45). Project on Y=0 plane → forward = +Z.
            Vector3 camFwd = new Vector3(0f, -0.7071f, 0.7071f);
            Vector3 camRight = Vector3.right;
            Vector3 v = CameraRelativeMotion.HorizontalVelocity(
                new Vector2(0f, 1f), camFwd, camRight, speed: 5f);
            Assert.That(v.normalized, Is.EqualTo(Vector3.forward).Using<Vector3>((a,b) => (a-b).magnitude < 0.001f ? 0 : 1));
        }
    }
}
```

### PlayMode integration test — player moves under simulated joystick input

```csharp
// File: Assets/Tests/PlayMode/PlayerMovementSmokeTests.cs  (asmdef: WM.Tests.PlayMode)
// Requires WM.Player + WM.UI in test asmdef references.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using WM.Player;
using WM.UI;

namespace WM.Tests.PlayMode
{
    public class PlayerMovementSmokeTests
    {
        [UnityTest]
        public IEnumerator PlayerMovesUnderSimulatedJoystickInput()
        {
            yield return SceneManager.LoadSceneAsync("Warehouse_MVP", LoadSceneMode.Single);
            yield return null; // wait for Awake/Start

            GameObject player = GameObject.Find("Player");
            Assert.That(player.GetComponent<CharacterController>(), Is.Not.Null);
            Assert.That(player.GetComponent<PlayerController>(), Is.Not.Null);

            VirtualJoystick joystick = Object.FindFirstObjectByType<VirtualJoystick>();
            Assert.That(joystick, Is.Not.Null, "VirtualJoystick missing from scene");

            Vector3 startPos = player.transform.position;

            // To simulate input without an EventSystem-driven pointer, expose an internal setter
            // on VirtualJoystick guarded by [UnityEngine.TestTools.UnityTest] / InternalsVisibleTo,
            // OR inject a TestJoystickSource via an IJoystick interface (deferred — D-04 chose direct ref).
            // SIMPLEST: add an internal SetDirectionForTesting(Vector2) method to VirtualJoystick
            // visible to WM.Tests.PlayMode via InternalsVisibleTo("WM.Tests.PlayMode").
            joystick.SetDirectionForTesting(new Vector2(1f, 0f)); // push right

            for (int i = 0; i < 30; i++) yield return null; // ~half second at 60fps

            Vector3 delta = player.transform.position - startPos;
            Assert.That(delta.x, Is.GreaterThan(0.5f),
                "Player must have moved right after sustained joystick input");
        }
    }
}
```

> **Plan note:** The `SetDirectionForTesting` hook should be `internal` with `[assembly: InternalsVisibleTo("WM.Tests.PlayMode")]` in the `WM.UI` assembly, OR the joystick math should be exposed as a pure static function (already covered by Pattern 1) and PlayMode tests should write a wrapper that bypasses pointer events entirely. Either is acceptable.

### PlayMode integration test — walls block player

```csharp
[UnityTest]
public IEnumerator PlayerCannotPassThroughPerimeterWall()
{
    yield return SceneManager.LoadSceneAsync("Warehouse_MVP", LoadSceneMode.Single);
    yield return null;

    GameObject player = GameObject.Find("Player");
    // Teleport near the east perimeter wall (X = +15 floor edge).
    player.transform.position = new Vector3(13f, 1f, 0f);

    VirtualJoystick joystick = Object.FindFirstObjectByType<VirtualJoystick>();
    joystick.SetDirectionForTesting(new Vector2(1f, 0f)); // push east into wall

    for (int i = 0; i < 60; i++) yield return null; // ~1 second

    Assert.That(player.transform.position.x, Is.LessThan(15f),
        "Player must be stopped by the east perimeter wall (X < +15)");
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `CinemachineVirtualCamera` + sub-pipeline body components accessed via `GetCinemachineComponent<...>` | `CinemachineCamera` + sibling `MonoBehaviour` components on the same GameObject (`AddComponent<CinemachinePositionComposer>`) | Cinemachine 3.0 (2023) | Phase 02 builders must use `AddComponent<>`, not the deprecated pipeline accessor. |
| `CinemachineFramingTransposer` (Body) | `CinemachinePositionComposer` (Body, same role, ScreenPosition range -0.5..+0.5 instead of 0..1) | Cinemachine 3.0 | Renaming + range change; CONTEXT.md D-10 wording references legacy term, planner must translate. |
| `CinemachineConfiner` (single component for 2D+3D) | `CinemachineConfiner2D` and `CinemachineConfiner3D` (split per dimension) | Cinemachine 2.7 → 3.0 [VERIFIED: CHANGELOG entry "CinemachineConfiner is deprecated. New behaviour CinemachineConfiner3D"] | Phase 02 D-13 already uses the correct 3D variant. |
| Legacy `UnityEngine.Input.GetTouch` | New Input System (`com.unity.inputsystem`) + `InputSystemUIInputModule` | Unity 2019.3+ stable, default in 2022+ | Phase 01 locked the new system; Phase 02 inherits. UGUI `IPointer*` handlers fire identically — joystick code is unaware of which input system feeds it. |
| `CharacterController.SimpleMove` (auto-gravity) | `CharacterController.Move` (manual gravity composition) | Stable since 2017; current best practice | D-09 chose manual gravity for explicit control. |

**Deprecated / outdated:**
- `CinemachineVirtualCamera` class: still compiles (lives in `Runtime/Deprecated/`) but emits upgrader prompts; do NOT use in Phase 02.
- `CinemachineFramingTransposer`: deprecated; use `CinemachinePositionComposer`.
- `CinemachineConfiner` (no suffix): deprecated; use `CinemachineConfiner3D`.
- Per Phase 01 D-21 STATE.md note: Render Graph Compatibility Mode is absent in Unity 6.x — research that mentions it is for older Unity. Not Phase 02 relevant.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | The Phase 01 floor effective footprint is 30×30 centered at origin (Plane mesh × scale 3 × 3) | Pattern 5 / Perimeter walls | Walls placed at wrong distance. Easy to verify visually + by reading Phase01SceneBuilder.cs:120-124. Verified in this research — confidence is actually HIGH. |
| A2 | The CONTEXT.md D-12 "0.55–0.60" wording refers to legacy `FramingTransposer` 0..1 range, and the equivalent in CM 3.x is `ScreenPosition.y ≈ -0.10..-0.20` (negative = below center) | Pattern 4 + Pitfall 3 | Camera framing will be visibly wrong; player may render off-screen or framed too high. Mitigation: low-risk because it's tunable in Inspector at playtest, and the EditMode test for framing values can be loose (`abs(y) < 0.5`). |
| A3 | `CharacterController.Move` is the right choice over `Rigidbody.MovePosition` for hybrid-casual feel | D-05 (locked, not a research choice) | N/A — locked by CONTEXT.md. |
| A4 | `WM.Player → WM.UI` asmdef back-reference is acceptable (no circular dep introduced) | Project Structure section | Build break if cyclic. Verified: `WM.UI` references `WM.Core, WM.Economy, WM.Orders, WM.Upgrades` — none reference `WM.Player`. Safe. |
| A5 | Existing `BootstrapSmokeTests.CinemachineCamera_IsConfigured` will fail after Phase 02 (Follow != null and lens mode change) | Pitfall 2 | Test breakage on first Phase 02 run; remediation already documented (update test in plan task). LOW risk — the test will fail loudly and clearly. |

## Open Questions (RESOLVED)

1. **Should `Composition.ScreenPosition.y` be -0.10 or -0.20?**
   - What we know: CONTEXT.md D-12 says "player 10–20% below center."
   - What's unclear: Whether the target framing wants player at 10% below (Y = -0.10 in CM 3.x semantics) or 20% below (Y = -0.20).
   - Recommendation: Start with `-0.10` in the Editor builder; verify visually in playtest device run; commit final value once UI HUD widgets land in Phase 4/5 and reveal how much "headroom" is needed.

2. **Should `OrthographicSize` be 6.0 exactly?**
   - What we know: CONTEXT.md D-11 says "placeholder ~6.0." Floor is 30×30; with `OrthographicSize = 6`, vertical view = 12 world units → ~40% of the floor visible at any time, plus side aspect-ratio expansion on portrait 1080×1920 (aspect 0.5625 → horizontal view = 12 × 0.5625 = 6.75 units wide).
   - What's unclear: Whether 6 is comfortable on iPhone-class portrait screens.
   - Recommendation: Use 6.0 as placeholder; expose as a `[SerializeField]` on a Phase02-only ScriptableObject (or document the location in the Editor builder) so playtest tuning is fast.

3. **Should `PlayerController.cameraTransform` be wired to `Camera.main.transform` automatically in `Awake`, or set explicitly by the Editor builder?**
   - What we know: D-06 reads `camera.transform.forward/right`. Auto-find via `Camera.main` works but uses tag lookup which is slow.
   - What's unclear: Whether Phase 02 should prefer Editor-builder explicit wiring (consistent with Phase 01 D-22 Bootstrap.gameManager pattern) or runtime lookup.
   - Recommendation: Explicit serialized field wired by Editor builder. Matches Phase 01 pattern, avoids `Camera.main` tag lookup overhead.

4. **Where does the `Phase02JoystickBuilder` author the joystick visuals — bare `Image` components with placeholder colored sprites, or just two empty `RectTransform`s with `Image` components and a solid color?**
   - What we know: D-03 says "Image background + Image handle" but doesn't specify sprites.
   - What's unclear: Whether to ship without sprites (just colored solid backgrounds) or commit a minimal sprite asset.
   - Recommendation: Solid colored `Image` components (no sprite reference). UGUI `Image` renders as solid color when `sprite = null`. Matches Phase 01 "placeholder colored materials" pattern.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Unity Editor 6000.4.6f1 | All Phase 02 work | ✓ | 6000.4.6f1 (per ProjectSettings/ProjectVersion.txt, Phase 01 D-21) | — |
| `com.unity.cinemachine` | Camera follow + Confiner3D | ✓ | 3.1.6 | — (CONTEXT.md D-10/D-13 lock CM 3.x) |
| `com.unity.inputsystem` | EventSystem pointer dispatch | ✓ | 1.15.0 | — |
| `com.unity.modules.physics` | `CharacterController`, `BoxCollider`, `CinemachineConfiner3D` (`CINEMACHINE_PHYSICS` define) | ✓ | 1.0.0 | — |
| `com.unity.ugui` | `Image`, `Canvas`, `CanvasScaler`, `RectTransform`, `Graphic.raycastTarget` | ✓ | 2.0.0 | — |
| `com.unity.test-framework` | EditMode + PlayMode tests | ✓ | 1.6.0 resolved (manifest pins 2.0.1-pre.18; STATE.md notes pre-release not yet published for 6.4 LTS) | — (tests pass on 1.6.0 per Phase 01 evidence) |
| `iOSSupport` module | Phase 01 BuildScript path | ✓ | (per Phase 01 install) | — — *Pitfall 6 regression still present* |

**No missing dependencies.** All Phase 02 requirements satisfied by Phase 01 substrate.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | Unity Test Framework 1.6.0 (resolved); manifest pins 2.0.1-pre.18 |
| Config file | None — uses Unity defaults. Test asmdefs at `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef` + `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` |
| Quick run command | `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` |
| Full suite command (both modes) | `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` followed by `<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -` |
| Single test | append `-testFilter <Namespace.Class.Method>` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MOVE-01 | VirtualJoystick.Direction returns zero in dead zone | unit (EditMode) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.VirtualJoystickMathTests.ApplyDeadZoneAndClamp_BelowDeadZone_ReturnsZero` | ❌ Wave 0 |
| MOVE-01 | VirtualJoystick.Direction clamps to magnitude ≤ 1 | unit (EditMode) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.VirtualJoystickMathTests.ApplyDeadZoneAndClamp_AboveUnitCircle_ClampedToUnit` | ❌ Wave 0 |
| MOVE-01 | CameraRelativeMotion projects on ground plane, falls back on degenerate forward | unit (EditMode) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.CameraRelativeMotionTests.HorizontalVelocity_TopDownCamera_FallsBackToWorldZ` | ❌ Wave 0 |
| MOVE-01 | PlayerStats copies MoveSpeed + TurnRateDegPerSec from PlayerConfig | unit (EditMode) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.PlayerStatsTests.FromConfig_CopiesAllFields` | ❌ Wave 0 |
| MOVE-01 | Phase 5 forward-compat: mutating PlayerStats does NOT mutate PlayerConfig SO | unit (EditMode) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.PlayerStatsTests.Mutating_Stats_DoesNotMutateConfig` | ❌ Wave 0 |
| MOVE-01 | Player primitive in scene has CharacterController + PlayerController + NO Rigidbody | unit (EditMode, scene-loaded) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.Phase02SceneTests.Player_HasCharacterController_AndNoRigidbody` | ❌ Wave 0 |
| MOVE-01 | VirtualJoystick GameObject present under UICanvas/SafeAreaPanel | unit (EditMode, scene-loaded) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.Phase02SceneTests.VirtualJoystick_IsChildOfSafeAreaPanel` | ❌ Wave 0 |
| MOVE-01 | Sustained joystick input moves player forward in world | integration (PlayMode) | `runTests -testPlatform PlayMode -testFilter WM.Tests.PlayMode.PlayerMovementSmokeTests.PlayerMovesUnderSimulatedJoystickInput` | ❌ Wave 0 |
| MOVE-01 | Player cannot cross the east perimeter wall after sustained eastward input | integration (PlayMode) | `runTests -testPlatform PlayMode -testFilter WM.Tests.PlayMode.PlayerMovementSmokeTests.PlayerCannotPassThroughPerimeterWall` | ❌ Wave 0 |
| MOVE-01 | Player cannot push through a station BoxCollider (e.g. PackingStation) | integration (PlayMode) | `runTests -testPlatform PlayMode -testFilter WM.Tests.PlayMode.PlayerMovementSmokeTests.PlayerCollidesWithStation` | ❌ Wave 0 |
| MOVE-01 | All 5 station GameObjects have non-trigger BoxColliders (D-17 verification) | unit (EditMode, scene-loaded) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.Phase02SceneTests.AllStations_HaveSolidBoxColliders` | ❌ Wave 0 |
| MOVE-02 | CinemachineCamera Follow target == Player.transform | unit (EditMode, scene-loaded) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.Phase02SceneTests.CinemachineCamera_FollowsPlayer` | ❌ Wave 0 |
| MOVE-02 | CinemachineCamera Body is CinemachinePositionComposer with damping > 0 | unit (EditMode, scene-loaded) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.Phase02SceneTests.CinemachineCamera_UsesPositionComposer` | ❌ Wave 0 |
| MOVE-02 | CinemachineCamera Lens is Orthographic | unit (EditMode, scene-loaded) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.Phase02SceneTests.CinemachineCamera_LensIsOrthographic` | ❌ Wave 0 |
| MOVE-02 | CinemachineConfiner3D extension attached with non-null BoundingVolume | unit (EditMode, scene-loaded) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.Phase02SceneTests.CinemachineCamera_HasConfiner3D` | ❌ Wave 0 |
| MOVE-02 | 4 perimeter walls present (Wall_North/South/East/West) under WarehouseColliders root | unit (EditMode, scene-loaded) | `runTests -testPlatform EditMode -testFilter WM.Tests.EditMode.Phase02SceneTests.PerimeterWalls_AllPresent` | ❌ Wave 0 |
| MOVE-02 (regression update) | Existing `BootstrapSmokeTests.CinemachineCamera_IsConfigured` updated/relaxed to reflect Phase 02 Follow + lens changes | unit (EditMode) | existing test, plan task rewrites it | ⚠️ exists, needs edit |

### Sampling Rate

- **Per task commit:** `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` (fast — pure math + scene structure assertions, no PlayMode start-up cost)
- **Per wave merge:** Both EditMode and PlayMode runs.
- **Phase gate:** Full suite green before `/gsd-verify-work`. Manual device validation also required for MOVE-01 acceptance criterion "Movement works in an iPhone build" (deferred to Phase 11 build verifier, but Phase 02 should at least confirm the player moves in Editor Play mode).

### Wave 0 Gaps

- [ ] `Assets/Tests/EditMode/VirtualJoystickMathTests.cs` — pure dead-zone math
- [ ] `Assets/Tests/EditMode/CameraRelativeMotionTests.cs` — ground-plane projection edge cases
- [ ] `Assets/Tests/EditMode/PlayerStatsTests.cs` — config → stats copy + isolation
- [ ] `Assets/Tests/EditMode/Phase02SceneTests.cs` — scene structure assertions (joystick, CC, Confiner, walls, station colliders)
- [ ] `Assets/Tests/PlayMode/PlayerMovementSmokeTests.cs` — sustained input → world motion + wall collision blocking
- [ ] Update `Assets/Tests/EditMode/BootstrapSmokeTests.cs::CinemachineCamera_IsConfigured` — relax assertions that Phase 02 invalidates
- [ ] Add `WM.Player` to `WM.Tests.EditMode.asmdef` references
- [ ] Add `WM.Player` + `WM.UI` to `WM.Tests.PlayMode.asmdef` references
- [ ] Add `[assembly: InternalsVisibleTo("WM.Tests.PlayMode")]` to `WM.UI` (so PlayMode tests can call `VirtualJoystick.SetDirectionForTesting`)

## Sources

### Primary (HIGH confidence)

- **Cinemachine 3.1.6 source code** at `Library/PackageCache/com.unity.cinemachine@285f38545487/`:
  - `Runtime/Components/CinemachinePositionComposer.cs` — Body component API (verified field names, types, defaults)
  - `Runtime/Behaviours/CinemachineConfiner3D.cs` — Extension API (verified `BoundingVolume`, `SlowingDistance`)
  - `Runtime/Behaviours/CinemachineCamera.cs` — Vcam API (verified `Follow` property writes through to `Target.TrackingTarget`)
  - `Runtime/Core/ScreenComposerSettings.cs` — Composition struct (verified ScreenPosition range `-0.5..+0.5`)
  - `Runtime/Core/LensSettings.cs` — Lens API (verified `ModeOverride` enum values)
  - `Runtime/Core/CameraTarget.cs` — CameraTarget struct
  - `Runtime/Unity.Cinemachine.asmdef` — versionDefines (`CINEMACHINE_PHYSICS` requires `com.unity.modules.physics 1.0.0`, which is present)
  - `CHANGELOG.md` — verified `CinemachineConfiner is deprecated. New behaviour CinemachineConfiner3D`
  - `Runtime/Deprecated/` directory — verified `CinemachineFramingTransposer.cs`, `CinemachineConfiner.cs` are in deprecated folder
- **Phase 01 codebase** (verified end-state, not just CONTEXT.md):
  - `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs` — Player primitive has non-kinematic Rigidbody (lines 141-143), Floor scale 3×1×3 (line 124), 5 stations with colored materials
  - `Assets/_Project/Scripts/Editor/Phase01CameraUiBuilder.cs` — vcam at `(10,12,-10)` Euler `(45,-45,0)`, passive Perspective FOV 40, UICanvas Screen Space - Camera, SafeAreaPanel child, EventSystem with InputSystemUIInputModule
  - `Assets/_Project/Scripts/UI/SafeAreaPanel.cs` — self-healing offsetMin/Max enforcement
  - `Assets/Tests/EditMode/BootstrapSmokeTests.cs` — Phase 1 assertions that Phase 2 will break (`Follow == null`, `FOV == 40`)
  - `Assets/_Project/Scripts/Player/WM.Player.asmdef` — currently references only `WM.Core`
  - `Assets/_Project/Scripts/UI/WM.UI.asmdef` — references `WM.Core, WM.Economy, WM.Orders, WM.Upgrades`
- **Packages/manifest.json + packages-lock.json** — verified Cinemachine 3.1.6 resolved, Input System 1.15.0 resolved, Physics 1.0.0 present
- **.planning/STATE.md** — Phase 01 D-21 through D-25 + iOSSupport.m_Automatic regression note

### Secondary (MEDIUM confidence, cross-verified)

- Context7 `/websites/unity3d_packages_com_unity_inputsystem_1_15` — `OnScreenStick` API surface confirms IPointer pattern conventions (not directly used per D-03 but validates Pattern 1)
- docs.unity3d.com — `CharacterController.Move` documentation (verified manual gravity composition is the standard)
- docs.unity3d.com — `RectTransformUtility.ScreenPointToLocalPointInRectangle` (verified null-for-Overlay, camera-for-Camera-mode behavior)

### Tertiary (LOW confidence)

- None — all major claims verified against primary sources.

## Project Constraints (from CLAUDE.md)

Extracted directives the planner MUST honor:

- **Spec-driven development.** Phase 02 references specs §2 (Player Movement) and §3 (Carry — informs decoupling), and MVP backlog WM-001 + WM-002.
- **Small MonoBehaviours.** `PlayerController` should remain compact; extract math into pure-function statics (`CameraRelativeMotion`, `VirtualJoystick.ApplyDeadZoneAndClamp`).
- **Plain C# services where suitable.** `PlayerStats` is plain C# (not a MonoBehaviour, not a SO).
- **ScriptableObjects for tunable data.** `PlayerConfig` is a SO; `baseMoveSpeed` and `turnRateDegPerSec` are NOT hardcoded in `PlayerController`.
- **No unspecified third-party packages.** Joystick is a custom UGUI MonoBehaviour (D-03), not `Joystick Pack` or any Asset Store package. `OnScreenStick` from `com.unity.inputsystem` is allowed (it's part of an already-pinned package) but D-03 explicitly chose custom.
- **English only** for code/comments/docs.
- **Interfaces for external integrations.** No external integrations in Phase 02 (no save, no analytics, no ads). Pattern unaffected.
- **No global mutable state without reason.** `PlayerStats` is instance-scoped (per-player); not a singleton.
- **No hardcoded balance values.** `MoveSpeed`, `TurnRateDegPerSec`, joystick `deadZone` are all SerializedField / SO-driven.
- **MVP scope adherence.** Phase 02 covers MOVE-01, MOVE-02 only. No carry logic, no analytics, no save.
- **Implementation style: serialized fields for tunable references, explicit names, early returns.** Reflected in code samples above.

## Metadata

**Confidence breakdown:**
- Standard stack: **HIGH** — every component verified against package source on disk + ctx7 docs.
- Architecture: **HIGH** — Phase 01 patterns (D-22, D-25 Editor builders) are documented and the Phase 02 builders extend them directly.
- Pitfalls: **HIGH** — Pitfalls 1, 2, 3, 5 are directly verified against Phase 01 code and Cinemachine 3.x source; Pitfall 6 is verified in Phase 01 STATE.md.
- Validation Architecture: **HIGH** — every test command + asmdef change is verified against the actual disk state.
- Tunable values (`OrthographicSize = 6`, `ScreenPosition.y = -0.10`, perimeter wall thickness): **MEDIUM** — placeholder values per CONTEXT.md D-15-style "tune in playtest." Not blocking; designed to be Inspector-tweaked.

**Research date:** 2026-05-11
**Valid until:** 2026-06-11 (30 days; Cinemachine 3.x and Input System 1.15 are stable on Unity 6.4 LTS — no major changes expected in this window)

## RESEARCH COMPLETE
