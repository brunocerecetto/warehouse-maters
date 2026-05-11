# Phase 02: Walk Around the Warehouse — Pattern Map

**Mapped:** 2026-05-11
**Files analyzed:** 17 (12 new, 5 modified)
**Analogs found:** 17 / 17 (every new file has a Phase-01 analog in this repo)

## File Classification

| New / Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `Assets/_Project/Scripts/Player/PlayerController.cs` | MonoBehaviour (player controller) | request-response (per-frame Update) | `Assets/_Project/Scripts/UI/SafeAreaPanel.cs` | role-match (sealed MonoBehaviour with `[RequireComponent]` + `Awake`/`Update`) |
| `Assets/_Project/Scripts/Player/PlayerStats.cs` | Plain C# runtime data class | transform (config → mutable mirror) | `Assets/_Project/Scripts/Core/Stubs/NullAnalyticsService.cs` | role-match (plain C# class, no Mono base, lives next to a MonoBehaviour) |
| `Assets/_Project/Scripts/Player/PlayerConfig.cs` | ScriptableObject definition | data-store (immutable tunables) | none in-repo (first SO definition) — use RESEARCH §Pattern 3 + `[CreateAssetMenu]` convention | no-analog (greenfield SO) |
| `Assets/_Project/Scripts/Player/CameraRelativeMotion.cs` *(optional helper)* | Pure-static math helper | transform (pure function) | `Assets/_Project/Scripts/Core/GameManager.cs` (style only — sealed class in namespace) | role-match (file-level static, no Mono base) |
| `Assets/_Project/Scripts/UI/VirtualJoystick.cs` | UGUI MonoBehaviour (input widget) | event-driven (`IPointer*`/`IDrag`) → polled property | `Assets/_Project/Scripts/UI/SafeAreaPanel.cs` | role-match (UGUI MonoBehaviour in `WM.UI`, `[RequireComponent(RectTransform)]`, polls per-frame state) |
| `Assets/_Project/ScriptableObjects/Player/PlayerConfig.asset` | SO asset instance | data-store | none — created by Editor builder via `AssetDatabase.CreateAsset` (see `Phase01SceneBuilder.cs:84-85`) | role-match-via-builder |
| `Assets/_Project/Scripts/Editor/Phase02PlayerWiring.cs` | Editor headless builder | scene-authoring (mutates Player GO) | `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs` | exact (D-22 builder convention) |
| `Assets/_Project/Scripts/Editor/Phase02JoystickBuilder.cs` | Editor headless builder | scene-authoring (mutates UICanvas/SafeAreaPanel) | `Assets/_Project/Scripts/Editor/Phase01CameraUiBuilder.cs` | exact (D-25 builder convention) |
| `Assets/_Project/Scripts/Editor/Phase02CameraConfigurator.cs` | Editor headless builder | scene-authoring (mutates CM vcam1) | `Assets/_Project/Scripts/Editor/Phase01CameraUiBuilder.cs` | exact (Cinemachine wiring lives in this analog) |
| `Assets/_Project/Scripts/Editor/Phase02CollisionBuilder.cs` | Editor headless builder | scene-authoring (creates walls + CameraBounds) | `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs` (`CreateStation`/`CreateEmpty` helpers) | exact (primitive-creation builder pattern) |
| `Assets/Tests/EditMode/VirtualJoystickMathTests.cs` | EditMode unit test (pure math) | request-response (static-fn assert) | `Assets/Tests/EditMode/BootstrapStructureTests.cs` | role-match (NUnit `[Test]` with no scene load) |
| `Assets/Tests/EditMode/CameraRelativeProjectionTests.cs` | EditMode unit test (pure math) | request-response (static-fn assert) | `Assets/Tests/EditMode/BootstrapStructureTests.cs` | role-match |
| `Assets/Tests/EditMode/PlayerStatsTests.cs` | EditMode unit test (SO+plain-C# isolation) | request-response | `Assets/Tests/EditMode/BootstrapStructureTests.cs` | role-match |
| `Assets/Tests/PlayMode/PlayerMovementSmokeTests.cs` | PlayMode integration test (scene-loaded) | event-driven (`yield return SceneManager.LoadSceneAsync`) | `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` | exact (`[UnityTest]` scene-load harness) |
| `Assets/Tests/PlayMode/PlayerCollisionSmokeTests.cs` | PlayMode integration test (scene-loaded) | event-driven | `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` | exact |
| `Assets/Tests/PlayMode/CameraFollowSmokeTests.cs` | PlayMode integration test (scene-loaded) | event-driven | `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` | exact |
| **MODIFIED** `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs` | Editor builder edit | scene-authoring | self | exact (idempotent: delete `Rigidbody` lines 141-143) |
| **MODIFIED** `Assets/Tests/EditMode/BootstrapSmokeTests.cs` | EditMode test relax | request-response | self | exact (relax `Follow == null` + FOV-40 assertions on lines 57-63) |
| **MODIFIED** `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef` | asmdef edit | metadata | self | exact (add `"WM.Player"` to `references`) |
| **MODIFIED** `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` | asmdef edit | metadata | self | exact (add `"WM.Player"`, `"WM.UI"`, `"Unity.Cinemachine"`) |
| **MODIFIED** `Assets/_Project/Scripts/Player/WM.Player.asmdef` | asmdef edit | metadata | self | exact (add `"WM.UI"` to `references`) |

---

## Shared Patterns (cross-cutting; apply to multiple new files)

### S1. Sealed class + `[RequireComponent]` + `Awake`/`Update` MonoBehaviour template

**Source:** `Assets/_Project/Scripts/UI/SafeAreaPanel.cs:10-32`
**Apply to:** `PlayerController.cs`, `VirtualJoystick.cs`

```csharp
using UnityEngine;

namespace WM.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastApplied;
        private Vector2Int _lastScreen;

        private void Awake() => _rt = GetComponent<RectTransform>();

        private void OnEnable() => Apply(Screen.safeArea);

        private void Update()
        {
            if (Screen.safeArea != _lastApplied ||
                Screen.width != _lastScreen.x || Screen.height != _lastScreen.y)
            {
                Apply(Screen.safeArea);
            }
        }
        // ...
    }
}
```

**How to apply:** Cache component reference in `Awake`, use `private readonly`/underscore-prefixed fields, mark class `sealed`, use file-scoped `namespace WM.<Subsystem>`. `PlayerController` mirrors this with `[RequireComponent(typeof(CharacterController))]`; `VirtualJoystick` keeps `[RequireComponent(typeof(RectTransform))]`.

### S2. File header comment with source citation + traceability

**Source:** every Phase 01 production file. Two representative excerpts:

`Assets/_Project/Scripts/UI/SafeAreaPanel.cs:1-9`:
```csharp
// Source: D-15 + RESEARCH §Pattern 4 (UI Canvas with Safe Area).
// Updates RectTransform anchors to Screen.safeArea each time the safe-area or
// resolution changes. Drop on a child of UICanvas with anchors stretched 0..1.
//
// Self-healing offsets: Apply() also resets offsetMin/offsetMax to Vector2.zero,
// so the runtime contract "rect equals Screen.safeArea" holds regardless of any
// edit-time RectTransform offset values.
```

`Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs:1-12`:
```csharp
// Source: Plan 01-02 (Warehouse_MVP scene composition).
// Phase 1 / Plan 02 Editor-only builder: programmatically creates the 7 placeholder
// ...
// Invoked headlessly:
//   <UNITY> -batchmode -quit -nographics -projectPath . \
//           -executeMethod WM.Editor.Phase01SceneBuilder.Build -logFile -
//
// Also available as Tools/Phase01/Build Warehouse_MVP Scene menu item for manual rebuilds.
// Idempotent: replaces existing materials/scene at known paths.
```

**How to apply:** First lines of every new Phase 02 production/editor/test file must cite (a) the Plan id (`Plan 02-XX`), (b) the relevant D-decisions from CONTEXT.md, (c) for Editor builders: the headless invocation command. This is the project's documentation convention.

### S3. Test file header citing VALIDATION + RESEARCH provenance

**Source:** `Assets/Tests/EditMode/BootstrapSmokeTests.cs:1-2` and `Assets/Tests/PlayMode/PlayModeSmokeTests.cs:1-2`

```csharp
// Source: VALIDATION.md rows 01-02-* (RequiredGameObject_IsPresent) and 01-03-* (Cinemachine, UICanvas, EventSystem checks).
// Covers BOOT-02 — required GameObjects + Cinemachine + UICanvas + EventSystem configuration.
```

**How to apply:** Every Phase 02 test file gets a 2-line header naming the `02-VALIDATION.md` row and the requirement id (MOVE-01 / MOVE-02). Keeps the validation matrix → test bidirectionally traceable.

---

## Pattern Assignments

### 1. `Assets/_Project/Scripts/UI/VirtualJoystick.cs` (UGUI MonoBehaviour, event-driven → polled)

**Analog:** `Assets/_Project/Scripts/UI/SafeAreaPanel.cs` (full file — only existing `WM.UI` MonoBehaviour)

**Imports + namespace pattern** (SafeAreaPanel.cs:10-15):
```csharp
using UnityEngine;

namespace WM.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPanel : MonoBehaviour
```
Apply: same shape; add `using UnityEngine.EventSystems;` and `using UnityEngine.UI;` for `IPointer*Handler` interfaces and `Image`. Implement: `IPointerDownHandler, IDragHandler, IPointerUpHandler`.

**Cached-reference pattern** (SafeAreaPanel.cs:17-21):
```csharp
private RectTransform _rt;
private Rect _lastApplied;
private Vector2Int _lastScreen;

private void Awake() => _rt = GetComponent<RectTransform>();
```
Apply: cache `background`/`handle` (serialized refs, not `GetComponent`), compute `_halfSize = background.rect.size * 0.5f` once on `Awake` to avoid per-frame rect reads (RESEARCH "Don't Hand-Roll" → "cache in Start").

**Public read surface (single property, no events)** — RESEARCH §Pattern 1 + D-04 direct property read:
- Expose `public Vector2 Direction { get; private set; }` mirroring SafeAreaPanel's "self-healing" private state contract.
- Provide pure static helper `public static Vector2 ApplyDeadZoneAndClamp(Vector2 raw, float deadZone)` (extracted from `UpdateDirection`) so EditMode tests don't need an EventSystem (mirrors how SafeAreaPanel's `Apply(Rect)` is conceptually pure but private — promote to `public static` because Wave 0 tests assert it).

**Critical detail not in any analog (from RESEARCH Pitfalls 4, 7):** pass `eventData.pressEventCamera` (NOT `null`, NOT `enterEventCamera`) to `RectTransformUtility.ScreenPointToLocalPointInRectangle` because Phase 01 D-15 Canvas is Screen Space - Camera (verified `Phase01CameraUiBuilder.cs:133` `canvas.renderMode = RenderMode.ScreenSpaceCamera`).

**Testing hook for PlayMode integration tests** (RESEARCH §"Plan note" line 844):
- Add an `internal void SetDirectionForTesting(Vector2 dir)` and pair with `[assembly: InternalsVisibleTo("WM.Tests.PlayMode")]` so PlayMode smokes can drive movement without an EventSystem.

---

### 2. `Assets/_Project/Scripts/Player/PlayerController.cs` (kinematic CC driver)

**Analog (composition style):** `Assets/_Project/Scripts/UI/SafeAreaPanel.cs` (sealed Mono + cached refs + `Awake`/`Update`)
**Analog (init-pattern semantics):** `Assets/_Project/Scripts/Core/GameManager.cs:9-26` (plain-C# services constructed and assigned in lifecycle)

**Init pattern from GameManager** (GameManager.cs:11-26):
```csharp
public sealed class GameManager : MonoBehaviour
{
    private IAnalyticsService _analytics;
    private IAdService _ads;
    private IIapService _iap;
    private ISaveService _save;

    public void Init(
        IAnalyticsService analytics,
        IAdService ads,
        IIapService iap,
        ISaveService save)
    {
        _analytics = analytics;
        _ads = ads;
        _iap = iap;
        _save = save;
    }
```
Apply: PlayerController instantiates `_stats = PlayerStats.FromConfig(config)` in `Awake` — equivalent "explicit construction" stance (no singletons, no service locators). Expose `public PlayerStats Stats => _stats;` so Phase 5's upgrade can mutate it without reflection (parallels how GameManager owns its services privately).

**Serialized-field pattern from Bootstrap** (`Bootstrap.cs:17`):
```csharp
[SerializeField] private GameManager gameManager;
```
Apply: same `[SerializeField] private` convention for `joystick`, `config`, `cameraTransform`. Phase 02 Editor builder wires these via `SerializedObject`/`FindProperty` exactly like Phase01SceneBuilder does for `Bootstrap.gameManager` (see "Editor wiring pattern" below).

**Core per-frame pattern** — see RESEARCH §Pattern 2 (lines 252-324). Critical guards from RESEARCH Anti-Patterns:
- Single `CharacterController.Move(...)` call per frame.
- Guard `horizontal.sqrMagnitude > 0.0001f` before `Quaternion.LookRotation(...)`.
- Read joystick in `Update`, never in `FixedUpdate`.

---

### 3. `Assets/_Project/Scripts/Player/PlayerStats.cs` (plain C#, runtime mirror)

**Analog:** `Assets/_Project/Scripts/Core/Stubs/NullAnalyticsService.cs` (sealed, plain C#, in same asmdef as the MonoBehaviour that constructs it)

**Imports + class shape** (NullAnalyticsService.cs:1-13):
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace WM.Core
{
    public sealed class NullAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
            Debug.Log($"[NullAnalyticsService] {eventName}");
        }
    }
}
```

**How to apply:** `PlayerStats` is `public sealed class` in `namespace WM.Player`, no Mono base, mutable public fields (`MoveSpeed`, `TurnRateDegPerSec`), one static factory `FromConfig(PlayerConfig)`. Lives in the same asmdef (`WM.Player`) as `PlayerController`. Matches "plain-C# service" convention from CLAUDE.md Implementation Style and the pattern Phase 01 used for the Null* stubs.

**D-16 isolation contract** (test-enforced — see `PlayerStatsTests.Mutating_Stats_DoesNotMutateConfig`): `FromConfig` must copy, never store the SO reference. Comment with `// D-16 — Phase 5 mutates stats, never the SO asset.`

---

### 4. `Assets/_Project/Scripts/Player/PlayerConfig.cs` (ScriptableObject definition)

**Analog:** none in repo — Phase 02 is the first SO definition. Use the **RESEARCH §Pattern 3 template** (lines 340-354) as the literal source.

**Convention to lift from Phase 01:** the SO asset will live at `Assets/_Project/ScriptableObjects/Player/PlayerConfig.asset`. Phase 01 D-05 / `BootstrapStructureTests.cs:22-28` already enumerate the SO root subfolders (`Boxes`, `Orders`, `Upgrades`, `Workers`, `Economy`). Phase 02 adds `Player/` — add a corresponding test row to `BootstrapStructureTests.RequiredFolders` (or a Phase02 scene-tests file), keeping the convention symmetric.

**`[CreateAssetMenu]` template** (from RESEARCH §Pattern 3):
```csharp
[CreateAssetMenu(fileName = "PlayerConfig",
                 menuName = "WarehouseMaster/Player/PlayerConfig",
                 order = 0)]
public sealed class PlayerConfig : ScriptableObject
{
    [Tooltip("Initial walk speed in meters/second. D-15 placeholder = 5.0.")]
    public float baseMoveSpeed = 5f;

    [Tooltip("Rotation rate in degrees/second toward velocity direction. D-15 placeholder = 720.")]
    public float turnRateDegPerSec = 720f;
}
```

---

### 5. `Assets/_Project/Scripts/Editor/Phase02PlayerWiring.cs` (Editor builder)

**Analog:** `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs` (the D-22 builder — opens scene, mutates Player primitive, saves scene).

**File-header + MenuItem + ScenePath const pattern** (Phase01SceneBuilder.cs:14-25):
```csharp
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WM.Core;

namespace WM.Editor
{
    public static class Phase01SceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Warehouse_MVP.unity";
```
Apply: same `using` block, `namespace WM.Editor`, `public static class Phase02PlayerWiring`, identical `ScenePath` const. Lives in `WM.Editor` asmdef which already references `WM.UI`, `Unity.Cinemachine`, `Unity.InputSystem` (verified `WM.Editor.asmdef:4`).

**Scene-open + save-or-throw pattern** (Phase01SceneBuilder.cs:101 vs. Phase01CameraUiBuilder.cs:45-63):
```csharp
Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
// ... mutate ...
bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
if (!saved)
{
    throw new System.InvalidOperationException(
        $"[Phase01CameraUiBuilder] Failed to save scene at {ScenePath}.");
}
AssetDatabase.SaveAssets();
AssetDatabase.Refresh();
Debug.Log("[Phase01CameraUiBuilder] Build complete.");
```
Apply verbatim with `[Phase02PlayerWiring]` log prefix.

**Idempotent component-replacement pattern** (Phase01SceneBuilder.cs:139-143):
```csharp
// Ensure a CapsuleCollider exists (CreatePrimitive adds one by default) and add Rigidbody.
if (player.GetComponent<CapsuleCollider>() == null) player.AddComponent<CapsuleCollider>();
Rigidbody rb = player.AddComponent<Rigidbody>();
rb.useGravity = true;
rb.isKinematic = false;
```
Apply (inverse): RESEARCH Pitfall 1 mandates this exact block must be **deleted** in the modified `Phase01SceneBuilder.cs` AND the new `Phase02PlayerWiring.Build()` must run a defensive `DestroyImmediate(player.GetComponent<Rigidbody>())` before `player.AddComponent<CharacterController>()` to handle scenes built by older Phase 01 commits.

**SerializedObject wiring for `[SerializeField] private` fields** (Phase01SceneBuilder.cs:158-166):
```csharp
SerializedObject so = new SerializedObject(boot);
SerializedProperty prop = so.FindProperty("gameManager");
if (prop == null)
{
    throw new System.InvalidOperationException(
        "[Phase01SceneBuilder] SerializedProperty 'gameManager' not found on Bootstrap component.");
}
prop.objectReferenceValue = gm;
so.ApplyModifiedPropertiesWithoutUndo();
```
Apply: identical block for each `[SerializeField] private` field on `PlayerController` (`joystick`, `config`, `cameraTransform`). The asset for `config` is loaded with `AssetDatabase.LoadAssetAtPath<PlayerConfig>("Assets/_Project/ScriptableObjects/Player/PlayerConfig.asset")` and assigned to `prop.objectReferenceValue`. Use one `SerializedObject` instance per component, `ApplyModifiedPropertiesWithoutUndo` once at the end.

**ScriptableObject asset creation pattern** (Phase01SceneBuilder.cs:80-95 — material creation; identical API shape for SO):
```csharp
string path = $"{MaterialsDir}/{spec.FileName}.mat";
Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
if (mat == null)
{
    mat = new Material(urpLit);
    AssetDatabase.CreateAsset(mat, path);
}
// ...
EditorUtility.SetDirty(mat);
```
Apply: same idempotent shape using `ScriptableObject.CreateInstance<PlayerConfig>()` instead of `new Material(...)`. Pre-create the `Assets/_Project/ScriptableObjects/Player/` directory exactly like `EnsureMaterialsDir()` (Phase01SceneBuilder.cs:60-67).

---

### 6. `Assets/_Project/Scripts/Editor/Phase02JoystickBuilder.cs` (Editor builder — UI authoring)

**Analog:** `Assets/_Project/Scripts/Editor/Phase01CameraUiBuilder.cs` (the D-25 builder — authored UICanvas + SafeAreaPanel; new joystick is a sibling of SafeAreaPanel's children).

**Imports for UGUI authoring** (Phase01CameraUiBuilder.cs:17-25):
```csharp
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WM.UI;
```
Apply: drop Cinemachine + EventSystem imports; keep `UnityEngine.UI` (Image), `UnityEngine.EventSystems` only if you need to reference `EventSystem` for sanity, and `WM.UI` for `VirtualJoystick`.

**Idempotent delete-before-create pattern** (Phase01CameraUiBuilder.cs:71-72 + 190-199):
```csharp
private static void DeleteAllRootGameObjectsNamed(Scene scene, string name)
{
    foreach (GameObject go in scene.GetRootGameObjects())
    {
        if (go.name == name)
        {
            Object.DestroyImmediate(go);
        }
    }
}
```
Apply: `DestroyImmediate` any existing `Joystick` GameObject under `SafeAreaPanel` before re-creating. Cross-builder idempotency is mandatory.

**RectTransform setup pattern** (Phase01CameraUiBuilder.cs:146-156):
```csharp
GameObject safeGo = new GameObject("SafeAreaPanel", typeof(RectTransform));
safeGo.transform.SetParent(uiGo.transform, worldPositionStays: false);

RectTransform srt = safeGo.GetComponent<RectTransform>();
srt.anchorMin = Vector2.zero;
srt.anchorMax = Vector2.one;
srt.offsetMin = Vector2.zero;
srt.offsetMax = Vector2.zero;
srt.localScale = Vector3.one;
srt.localPosition = Vector3.zero;
```
Apply: same shape for joystick. D-01 bottom-left anchoring → `anchorMin = anchorMax = new Vector2(0f, 0f)`, `pivot = new Vector2(0.5f, 0.5f)`, `anchoredPosition = new Vector2(joystickInset, joystickInset)`, `sizeDelta = new Vector2(joystickSize, joystickSize)`. Handle is a child with `anchorMin = anchorMax = pivot = (0.5, 0.5)` and `anchoredPosition = Vector2.zero`.

**Multi-component GameObject creation pattern** (Phase01CameraUiBuilder.cs:125-130):
```csharp
GameObject uiGo = new GameObject(
    "UICanvas",
    typeof(RectTransform),
    typeof(Canvas),
    typeof(CanvasScaler),
    typeof(GraphicRaycaster));
```
Apply: `new GameObject("Joystick", typeof(RectTransform), typeof(Image), typeof(VirtualJoystick))`. Handle child: `new GameObject("Handle", typeof(RectTransform), typeof(Image))`. RESEARCH §Open Question 4: leave `sprite = null` (UGUI `Image` renders solid color), set `color` to a visible placeholder (e.g., `new Color(1, 1, 1, 0.35f)` background, `Color.white` handle).

**Wire SerializeField references on the new component** — use the same `SerializedObject` pattern shown in §5 above:
```csharp
SerializedObject so = new SerializedObject(joystickComp);
so.FindProperty("background").objectReferenceValue = bgRt;
so.FindProperty("handle").objectReferenceValue = handleRt;
so.ApplyModifiedPropertiesWithoutUndo();
```

---

### 7. `Assets/_Project/Scripts/Editor/Phase02CameraConfigurator.cs` (Editor builder — Cinemachine)

**Analog:** `Assets/_Project/Scripts/Editor/Phase01CameraUiBuilder.cs:69-115` (the original CM vcam authoring — Phase 02 mutates the same vcam).

**Cinemachine 3.x lens-update pattern** (Phase01CameraUiBuilder.cs:106-114):
```csharp
CinemachineCamera vcam = vcamGo.AddComponent<CinemachineCamera>();
// Passive vcam: Tracking Target left null (D-12). Phase 2 will set it.
vcam.Target = default;

// Lens: Perspective, FOV 40 (locked here for cross-phase determinism).
LensSettings lens = LensSettings.Default;
lens.ModeOverride = LensSettings.OverrideModes.Perspective;
lens.FieldOfView = VcamFieldOfView;
vcam.Lens = lens;
```
Apply: find the existing vcam (`Object.FindFirstObjectByType<CinemachineCamera>()`), set `vcam.Follow = player.transform`, then **copy → mutate → reassign** the `LensSettings` struct (it's a value type, in-place mutation does nothing) flipping `ModeOverride = Orthographic` and `OrthographicSize = 6f` (D-11). Mirror the exact `LensSettings lens = vcam.Lens; ... ; vcam.Lens = lens;` pattern.

**AddComponent for CM body/extension** — see RESEARCH §Pattern 4 (lines 396-462) for the literal builder. Critical reminder from RESEARCH Pitfall 5: CM 3.x components are sibling `MonoBehaviour`s on the vcam GameObject, not a sub-array. Use `vcam.gameObject.AddComponent<CinemachinePositionComposer>()` + `vcam.gameObject.AddComponent<CinemachineConfiner3D>()`. Idempotency: `foreach (var c in vcam.GetComponents<CinemachineComponentBase>()) Object.DestroyImmediate(c);` before re-adding (same pattern as `DestroyAllChildren` in Phase01CameraUiBuilder.cs:201-213).

**ScreenPosition translation note** (RESEARCH Pitfall 3): CONTEXT.md D-12 says "0.55–0.60" which is legacy 0..1 range; translate to CM 3.x `ScreenPosition.y ≈ -0.10` (negative = player rendered LOW on screen). Document the translation in a code comment so future readers don't "fix" it back to 0.55.

---

### 8. `Assets/_Project/Scripts/Editor/Phase02CollisionBuilder.cs` (Editor builder — walls + bounds)

**Analog:** `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs:118-148` (the station-creation helpers `CreateStation`/`CreateEmpty`).

**Empty-GO-with-collider helper pattern** (Phase01SceneBuilder.cs:205-210):
```csharp
private static GameObject CreateEmpty(string name, Vector3 position)
{
    GameObject go = new GameObject(name);
    go.transform.position = position;
    return go;
}
```
Apply: build a `CreateWall(parent, name, pos, size)` helper that calls `new GameObject(name)`, `SetParent(parent)`, `transform.position = pos`, then `BoxCollider c = go.AddComponent<BoxCollider>(); c.size = size; c.isTrigger = false;`. **No `MeshRenderer`/`MeshFilter`** (D-18 invisible walls). See RESEARCH §Pattern 5 (lines 484-545) for full implementation.

**Floor footprint constants** — Phase 01 ground truth from `Phase01SceneBuilder.cs:120-124`: Plane primitive default mesh = 10×10 → `localScale = (3, 1, 3)` → effective 30×30 centered at origin → playable bounds `±15` on X and Z. RESEARCH §Assumption A1 verifies this. Encode in builder as `private const float HalfFloor = 15f;`.

**CameraBounds trigger collider** — RESEARCH §Pattern 5 (line 528): `isTrigger = true` is correct for `CinemachineConfiner3D.BoundingVolume` because Confiner uses `Collider.ClosestPoint` (verified safe on triggers; avoids physics interaction with the player capsule).

---

### 9. `Assets/Tests/EditMode/VirtualJoystickMathTests.cs` (pure-math unit test)

**Analog:** `Assets/Tests/EditMode/BootstrapStructureTests.cs` (NUnit pattern with no scene load).

**Test class shape + namespace** (BootstrapStructureTests.cs:1-12):
```csharp
// Source: VALIDATION.md Wave 0 row 01-01-* + RESEARCH Code Examples.
// Covers BOOT-01 — folder structure, asmdef presence, URP asset.
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace WM.Tests.EditMode
{
    public class BootstrapStructureTests
    {
```
Apply: 2-line header citing `02-VALIDATION.md` row, `using NUnit.Framework; using UnityEngine; using WM.UI;`, public unsealed class in `namespace WM.Tests.EditMode`. Each `[Test]` calls `VirtualJoystick.ApplyDeadZoneAndClamp(...)` and asserts via `Assert.That(... , Is.EqualTo(...).Within(0.001f))`. No scene load. See RESEARCH §"EditMode unit test — joystick math" lines 668-710 for the literal tests.

**`Assert.That` style with `.Within(...)` tolerance** (BootstrapStructureTests.cs:82 and BootstrapSmokeTests.cs:62):
```csharp
Assert.That(rp, Is.Not.Null, "GraphicsSettings.defaultRenderPipeline is null — URP not assigned");
// ...
Assert.That(vcams[0].Lens.FieldOfView, Is.EqualTo(40f).Within(0.001f), "...");
```
Apply: use `.Within(0.001f)` for float magnitudes, include diagnostic messages explaining the failure case.

---

### 10. `Assets/Tests/EditMode/CameraRelativeProjectionTests.cs` (pure-math unit test)

**Analog:** `Assets/Tests/EditMode/BootstrapStructureTests.cs` (same NUnit shape).

**How to apply:** Mirror `VirtualJoystickMathTests`. Tests call `WM.Player.CameraRelativeMotion.HorizontalVelocity(...)`. See RESEARCH lines 754-792 for the three required test cases (zero input, top-down degenerate forward, 45° isometric projection). Add `using WM.Player;` to imports. This file requires `WM.Player` in `WM.Tests.EditMode.asmdef` references (see Modified files §A).

---

### 11. `Assets/Tests/EditMode/PlayerStatsTests.cs` (SO + plain-C# isolation test)

**Analog:** `Assets/Tests/EditMode/BootstrapStructureTests.cs` (NUnit shape) + how Phase 01 creates Editor-time SO instances in builders (`Phase01SceneBuilder.cs:84`: `mat = new Material(urpLit)`).

**`ScriptableObject.CreateInstance` pattern for test fixtures** (RESEARCH §"EditMode unit test — PlayerStats" lines 727-734):
```csharp
PlayerConfig config = ScriptableObject.CreateInstance<PlayerConfig>();
config.baseMoveSpeed = 7.5f;
config.turnRateDegPerSec = 540f;

PlayerStats stats = PlayerStats.FromConfig(config);

Assert.That(stats.MoveSpeed, Is.EqualTo(7.5f));
```
Apply verbatim. The "mutating stats does NOT mutate config" test (D-16 contract) is non-negotiable.

---

### 12. `Assets/Tests/PlayMode/PlayerMovementSmokeTests.cs` (scene-loaded integration)

**Analog:** `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` (the only existing PlayMode test).

**Full PlayMode harness pattern** (PlayModeSmokeTests.cs:1-36 — entire file):
```csharp
// Source: VALIDATION.md row 01-02-* + RESEARCH §PlayMode smoke test code example.
// Covers BOOT-02 runtime gate — scene loads at runtime AND GameManager logs init line.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using WM.Core;

namespace WM.Tests.PlayMode
{
    public class PlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator Scene_Loads_GameManagerInitializes()
        {
            // Scene must be in Build Settings for SceneManager.LoadSceneAsync to find it.
            LogAssert.Expect(LogType.Log, "GameManager initialized");

            yield return SceneManager.LoadSceneAsync("Warehouse_MVP", LoadSceneMode.Single);
            // Wait one frame for Awake/Start chain to finish.
            yield return null;

            GameObject gm = GameObject.Find("GameManager");
            Assert.That(gm, Is.Not.Null, "GameManager GameObject not found in loaded scene");
            // ...
        }
    }
}
```

**How to apply:** Same `[UnityTest] IEnumerator` + `yield return SceneManager.LoadSceneAsync(...)` + `yield return null` warmup. Replace `LogAssert.Expect` with `joystick.SetDirectionForTesting(new Vector2(1f, 0f))` then `for (int i=0; i<30; i++) yield return null;` then position assertion. See RESEARCH §"PlayMode integration test — player moves" lines 798-841 for the literal test. Requires `WM.Player` + `WM.UI` in `WM.Tests.PlayMode.asmdef` (see Modified files §B).

---

### 13. `Assets/Tests/PlayMode/PlayerCollisionSmokeTests.cs` & `CameraFollowSmokeTests.cs`

**Analog:** `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` (same scene-load harness; one `[UnityTest]` per assertion).

**How to apply:** Mirror §12. For collision test: teleport player to `(13f, 1f, 0f)`, push east, assert `x < 15f` after 60 frames (RESEARCH lines 849-866). For camera test: load scene, push player +X for 30 frames, assert `Camera.main.transform.position.x > startCameraX` to prove follow is working.

---

### M-A. **MODIFIED** `Assets/_Project/Scripts/Editor/Phase01SceneBuilder.cs`

**Self-modification.** RESEARCH Pitfall 1 mandates removal of lines 141-143:
```csharp
// REMOVE these 3 lines (Phase 01 Rigidbody addition):
Rigidbody rb = player.AddComponent<Rigidbody>();
rb.useGravity = true;
rb.isKinematic = false;
```
Also remove the comment on line 139-140 referring to "add Rigidbody". The `CapsuleCollider` line (140) **stays** — `CharacterController` will need that collider geometry for visualization (though it adds its own internal collider).

Replace with a one-line comment: `// Phase 02 attaches CharacterController; no Rigidbody at scene-build time.`

---

### M-B. **MODIFIED** `Assets/Tests/EditMode/BootstrapSmokeTests.cs`

**Self-modification.** RESEARCH Pitfall 2: lines 57-63 will fail after Phase 02 runs. Replace:
```csharp
// CURRENT (lines 56-63) — will fail after Phase 02:
Assert.That(vcams[0].Follow, Is.Null,
    "Phase 1 CinemachineCamera must have no Follow target (D-12). Phase 2 will set it to Player.");

Assert.That(vcams[0].Lens.FieldOfView, Is.EqualTo(40f).Within(0.001f),
    "CinemachineCamera Lens FieldOfView must be 40 (locked here for Phase 2 follow damping determinism).");
```

With Phase 02-aware assertions:
```csharp
// Phase 02: Follow must point at Player GameObject (D-10).
GameObject player = GameObject.Find("Player");
Assert.That(vcams[0].Follow, Is.EqualTo(player.transform),
    "Phase 02 CinemachineCamera must Follow the Player transform (D-10).");

// Phase 02: lens switched to Orthographic (D-11).
Assert.That(vcams[0].Lens.ModeOverride, Is.EqualTo(LensSettings.OverrideModes.Orthographic),
    "Phase 02 lens must be Orthographic (D-11).");
Assert.That(vcams[0].Lens.OrthographicSize, Is.GreaterThan(0f),
    "Phase 02 OrthographicSize must be positive.");
```
Keep the `CinemachineBrain` and `CinemachineCamera` count assertions (lines 48-54) unchanged — they still hold.

---

### M-C. **MODIFIED** `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef`

**Self-modification.** Add `"WM.Player"` to the `references` array (currently lines 4-12):
```json
"references": [
    "WM.Core",
    "WM.Player",
    "WM.UI",
    "Unity.Cinemachine",
    "Unity.InputSystem",
    "UnityEngine.UI",
    "UnityEngine.TestRunner",
    "UnityEditor.TestRunner"
],
```

### M-D. **MODIFIED** `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef`

**Self-modification.** Add `"WM.Player"`, `"WM.UI"`, `"Unity.Cinemachine"` to the `references` array (currently lines 4-7 has only `WM.Core` and `UnityEngine.TestRunner`):
```json
"references": [
    "WM.Core",
    "WM.Player",
    "WM.UI",
    "Unity.Cinemachine",
    "UnityEngine.TestRunner"
],
```

### M-E. **MODIFIED** `Assets/_Project/Scripts/Player/WM.Player.asmdef`

**Self-modification.** Add `"WM.UI"` to `references` (currently `["WM.Core"]`):
```json
"references": ["WM.Core", "WM.UI"],
```
RESEARCH §"Alternatives Considered" line 56 + §Project Structure lines 161-162 verify no cycle is introduced (`WM.UI` references `WM.Core, WM.Economy, WM.Orders, WM.Upgrades` — none reference back to `WM.Player`).

---

## No Analog Found

| File | Role | Data Flow | Reason | Substitute |
|---|---|---|---|---|
| `Assets/_Project/Scripts/Player/PlayerConfig.cs` | ScriptableObject | data-store | Phase 02 is the first SO in the repo (none of the SO subfolders under `Assets/_Project/ScriptableObjects/` contain `.cs` definitions or `.asset` instances yet). | Use RESEARCH §Pattern 3 (lines 339-374) literal template; follow CLAUDE.md "ScriptableObjects for tunable game data" rule + Phase 01 D-05 folder convention (subfolder per system). |
| `Assets/_Project/ScriptableObjects/Player/PlayerConfig.asset` | SO asset instance | data-store | No existing SO assets in repo. | Generated by `Phase02PlayerWiring.Build()` via the AssetDatabase pattern shown in §5 above (loaded with `Phase01SceneBuilder.cs:81`'s `LoadAssetAtPath<Material>` idiom, swapped to `LoadAssetAtPath<PlayerConfig>`). |

---

## Metadata

**Analog search scope:**
- `Assets/_Project/Scripts/**/*.cs` (17 files — Bootstrap, GameManager, 4 service interfaces, 4 null stubs, SafeAreaPanel, 2 Phase01 builders, BuildScript)
- `Assets/Tests/**/*.cs` (3 files — BootstrapStructureTests, BootstrapSmokeTests, PlayModeSmokeTests)
- `Assets/**/*.asmdef` (15 files — 13 production + 2 test)
- `Assets/_Project/ScriptableObjects/**` (folders only — no `.cs` / `.asset` files)

**Files scanned:** 35 (17 prod + 3 test + 15 asmdef)

**Key patterns identified:**
1. **Sealed MonoBehaviour with `[RequireComponent]`** — single pattern shared by SafeAreaPanel, Bootstrap, GameManager; applies to PlayerController + VirtualJoystick.
2. **Editor headless builder with `[MenuItem("Tools/Phase0X/...")]` + `ScenePath` const + `EditorSceneManager.OpenScene` / `SaveScene`-or-throw** — established by Phase01SceneBuilder and Phase01CameraUiBuilder; applies to all four Phase02 builders.
3. **`[SerializeField] private` field wiring via `SerializedObject.FindProperty` / `objectReferenceValue` / `ApplyModifiedPropertiesWithoutUndo`** — established by Phase01SceneBuilder.cs:158-166 for `Bootstrap.gameManager`; applies to every Phase 02 component reference wiring (joystick, config, cameraTransform, joystick background/handle).
4. **File-header source citation** — every prod/editor/test file cites the originating decision (`Source: D-XX + RESEARCH §Pattern Y`); applies to all 12 new files.
5. **Test file shape: 2-line VALIDATION header + `Assert.That(..., Is...).Within(0.001f)` tolerance + diagnostic messages** — established by BootstrapStructureTests + BootstrapSmokeTests; applies to all 6 new test files.
6. **Idempotent builder pattern: delete-by-name (or by-component) before re-create** — established by `DeleteAllRootGameObjectsNamed` (Phase01CameraUiBuilder.cs:190-199) and `DestroyAllChildren` (lines 201-213); applies to all Phase02 builders (idempotency is non-negotiable per Phase01 conventions).

**Pattern extraction date:** 2026-05-11
