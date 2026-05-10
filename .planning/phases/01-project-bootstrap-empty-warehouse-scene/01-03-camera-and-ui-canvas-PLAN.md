---
phase: 01-project-bootstrap-empty-warehouse-scene
plan: 03
type: execute
wave: 3
depends_on:
  - 01
  - 02
files_modified:
  - Assets/_Project/Scenes/Warehouse_MVP.unity
  - Assets/_Project/Scripts/UI/SafeAreaPanel.cs
  - Assets/Tests/EditMode/BootstrapSmokeTests.cs
autonomous: true
requirements:
  - BOOT-02

must_haves:
  truths:
    - "CameraRig contains the Main Camera (with CinemachineBrain) and a child CinemachineCamera positioned for fixed isometric framing of the empty warehouse"
    - "UICanvas is a Screen Space - Camera canvas at 1080×1920 reference resolution with Match=0.5, with a SafeAreaPanel child anchored to Screen.safeArea"
    - "EventSystem GameObject has the Input System UI Input Module component (paired with Active Input Handling = Input System Package New from D-03)"
    - "Pressing Play in the Editor renders the warehouse from the Cinemachine virtual camera; UI overlay is empty but the SafeAreaPanel rect is non-zero on iPhone simulator"
    - "Scene contains exactly one GameObject named 'UICanvas' and exactly one named 'EventSystem' after Task 2 completes (the Phase 1 placeholders from plan 01-02 must be deleted before the real Canvas + EventSystem components are added)."
    - "Cinemachine vcam Lens Mode = Perspective, FOV = 40 (locked here to keep Phase 2 follow damping deterministic)."
  artifacts:
    - path: "Assets/_Project/Scripts/UI/SafeAreaPanel.cs"
      provides: "Runtime-aware safe-area RectTransform anchorer"
      contains: "namespace WM.UI", "Screen.safeArea", "RectTransform"
    - path: "Assets/_Project/Scenes/Warehouse_MVP.unity"
      provides: "Scene populated with CinemachineCamera + Canvas + SafeAreaPanel + InputSystemUIInputModule"
      contains: "CinemachineCamera", "CinemachineBrain", "Canvas", "InputSystemUIInputModule"
  key_links:
    - from: "CameraRig > Main Camera"
      to: "URP_Mobile_Renderer (renderer asset from plan 01-01)"
      via: "Camera component → Renderer field"
      pattern: "URP_Mobile_Renderer"
    - from: "CameraRig > Main Camera"
      to: "CameraRig > CM vcam1"
      via: "CinemachineBrain → CinemachineCamera Priority"
      pattern: "CinemachineBrain|CinemachineCamera"
    - from: "UICanvas > SafeAreaPanel"
      to: "Screen.safeArea (runtime)"
      via: "SafeAreaPanel.cs anchorMin/anchorMax assignment + offset reset"
      pattern: "Screen\\.safeArea"
    - from: "EventSystem"
      to: "Input System UI Input Module"
      via: "Component on EventSystem GameObject"
      pattern: "InputSystemUIInputModule"
---

<objective>
Configure the Cinemachine 3.x camera rig and the UI Canvas + Safe Area infrastructure on top of the scene from plan 01-02. The Main Camera lives under the existing `CameraRig` empty with a `CinemachineBrain`; a child `CM vcam1` GameObject with a passive `CinemachineCamera` (no Tracking Target) provides fixed isometric framing of the empty warehouse. The existing empty `UICanvas` placeholder is replaced with a real Screen-Space Camera Canvas (1080×1920, Match=0.5) plus a child `SafeAreaPanel` that adapts to iPhone notch/Dynamic Island via `Screen.safeArea`. The empty `EventSystem` placeholder gets `EventSystem` + `Input System UI Input Module` components so Phase 2's joystick can dispatch input.

Purpose: This is the visible end-state slice of the walking skeleton — phase success criterion 3 ("debug iOS build runs on simulator/device and shows the empty warehouse scene from the configured camera"). After this plan, the iOS Xcode project produced by `BuildScript.BuildIOS` will boot to a real isometric warehouse view rather than the default skybox.

Output: Cinemachine-driven camera frames the warehouse; UI Canvas is wired and safe-area aware; EventSystem is ready for Phase 2 input.
</objective>

<execution_context>
@/Users/brunocerecetto/repos/mine/warehouse-maters/.claude/get-shit-done/workflows/execute-plan.md
@/Users/brunocerecetto/repos/mine/warehouse-maters/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/REQUIREMENTS.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-VALIDATION.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-SKELETON.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-01-init-unity-project-PLAN.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-02-warehouse-scene-layout-PLAN.md
@CLAUDE.md
@specs/06-technical-architecture-spec.md

<interfaces>
<!-- Cinemachine 3.x runtime API surface (RESEARCH §Pattern 3) -->
<!-- Namespace: Unity.Cinemachine (NOT 'Cinemachine') -->
<!-- Component on Main Camera: CinemachineBrain -->
<!-- Component on child empty: CinemachineCamera (one word, no "Virtual") -->

namespace Unity.Cinemachine
{
    public class CinemachineBrain  : MonoBehaviour { /* drives Camera.transform from active vcam */ }
    public class CinemachineCamera : MonoBehaviour { /* Tracking Target = null in P1; passive transform reference */ }
}

<!-- New Input System UI module (RESEARCH §Pattern 5) -->
namespace UnityEngine.InputSystem.UI
{
    public class InputSystemUIInputModule : BaseInputModule { /* default DefaultInputActions asset */ }
}

<!-- New SafeAreaPanel.cs in WM.UI asmdef (RESEARCH §Pattern 4) -->
namespace WM.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPanel : MonoBehaviour
    {
        // Updates RectTransform anchorMin/anchorMax to Screen.safeArea every frame Screen.safeArea or Screen.{width,height} changes.
        // Apply() also resets offsetMin/offsetMax to Vector2.zero so the runtime contract "rect == Screen.safeArea"
        // is self-enforcing — independent of edit-time RectTransform offsets.
    }
}
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Author SafeAreaPanel.cs in WM.UI</name>
  <files>
    Assets/_Project/Scripts/UI/SafeAreaPanel.cs
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §Pattern 4 (UI Canvas with Safe Area)
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md (D-15)
  </read_first>
  <behavior>
    - Test 1 (compile): `SafeAreaPanel.cs` compiles under namespace `WM.UI` in the `WM.UI.asmdef` from plan 01-01.
    - Test 2 (runtime, manual via PlayMode in iPhone simulator): SafeAreaPanel's RectTransform anchorMin/anchorMax adjust to leave the notch/Dynamic Island uncovered.
    - Test 3 (EditMode, optional): adding the component to a GameObject without RectTransform raises a missing-component error (RequireComponent enforcement).
    - Test 4 (self-healing offsets): if a developer accidentally edits the SafeAreaPanel RectTransform's `offsetMin`/`offsetMax` to non-zero values in the Inspector, the runtime `Apply()` resets them to `Vector2.zero` so the rect equals `Screen.safeArea` regardless of edit-time state.
  </behavior>
  <action>
Use the source from RESEARCH §Pattern 4, with the self-healing-offsets revision: `Apply()` takes the safe-area `Rect` as a parameter, short-circuits when nothing changed, guards against zero-size screens (Editor preview / pre-init frames), and additionally resets `offsetMin` / `offsetMax` to `Vector2.zero` so the RectTransform rect always equals `Screen.safeArea` at runtime — independent of any edit-time offset state.

Write to `Assets/_Project/Scripts/UI/SafeAreaPanel.cs`:

```csharp
// Source: D-15 + RESEARCH §Pattern 4 (UI Canvas with Safe Area)
// Updates RectTransform anchors to Screen.safeArea each time the safe-area or
// resolution changes. Drop on a child of UICanvas with anchors stretched 0..1.
//
// Self-healing offsets: Apply() also resets offsetMin/offsetMax to Vector2.zero,
// so the runtime contract "rect equals Screen.safeArea" holds regardless of any
// edit-time RectTransform offset values. Operators do NOT need to manually set
// offsetMin/offsetMax to (0,0) in the Inspector — Apply() enforces it every frame
// the safe area changes.
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

        private void Apply(Rect safeArea)
        {
            if (safeArea == _lastApplied) return;
            if (Screen.width == 0 || Screen.height == 0) return;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;  anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;  anchorMax.y /= Screen.height;

            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
            _rt.offsetMin = Vector2.zero; // self-healing: enforce rect == safeArea
            _rt.offsetMax = Vector2.zero;

            _lastApplied = safeArea;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
```

> Why `Update` (not `OnRectTransformDimensionsChange`)? `Screen.safeArea` can change without the RectTransform itself changing (e.g., orientation lock break in Editor preview). Polling once per frame is cheap; the field comparisons short-circuit when nothing changed.

> Why reset `offsetMin`/`offsetMax` inside `Apply()`? Anchors alone do not pin the rect to the safe area if the RectTransform has non-zero offsets — the rect would be inset/outset from the safe-area rectangle by those offsets. Resetting them here makes the contract "rect == safeArea" self-enforcing, so it does not depend on the operator setting `offsetMin = (0,0)` and `offsetMax = (0,0)` in the Inspector at edit time.

Save and let Editor recompile. Console must remain at 0 errors.
  </action>
  <verify>
    <automated>
test -f Assets/_Project/Scripts/UI/SafeAreaPanel.cs
grep -q 'namespace WM.UI' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
grep -q 'class SafeAreaPanel' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
grep -q '\[RequireComponent(typeof(RectTransform))\]' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
grep -q 'Screen.safeArea' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
grep -q 'anchorMin' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
grep -q 'anchorMax' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
# Self-healing offsets: Apply() must reset both offsetMin and offsetMax to Vector2.zero
grep -q 'offsetMin = Vector2.zero' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
grep -q 'offsetMax = Vector2.zero' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
# Apply() must take the safe-area Rect as a parameter (not read Screen.safeArea inside the body)
grep -qE 'private void Apply\(Rect[[:space:]]+safeArea\)' Assets/_Project/Scripts/UI/SafeAreaPanel.cs
    </automated>
  </verify>
  <done>
    `SafeAreaPanel.cs` compiles. The Editor lists "Safe Area Panel" under Add Component → Scripts → WM.UI when adding to a RectTransform-bearing GameObject. `Apply(Rect safeArea)` resets `offsetMin`/`offsetMax` to `Vector2.zero` every change, so the runtime rect equals `Screen.safeArea` regardless of edit-time offset values.
  </done>
  <acceptance_criteria>
    - File `Assets/_Project/Scripts/UI/SafeAreaPanel.cs` declares `namespace WM.UI`, has `[RequireComponent(typeof(RectTransform))]`, and updates the RectTransform's `anchorMin`/`anchorMax` based on `Screen.safeArea`.
    - `Apply(Rect safeArea)` resets `offsetMin` and `offsetMax` to `Vector2.zero` so the rect equals `Screen.safeArea` at runtime — independent of any edit-time RectTransform offset state.
    - Editor Console has 0 compile errors.
  </acceptance_criteria>
</task>

<task type="auto" tdd="true">
  <name>Task 2: Build the CameraRig (Cinemachine 3.x), UICanvas (Screen-Space Camera + SafeAreaPanel), and EventSystem (Input System UI Input Module) in Warehouse_MVP scene</name>
  <files>
    Assets/_Project/Scenes/Warehouse_MVP.unity
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §Pattern 3 (Cinemachine 3.x Camera Rig), §Pattern 4 (Canvas wiring), §Pattern 5 (EventSystem with Input System UI Input Module), §Common Pitfalls 2, 3, 6, 7
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md (D-12, D-15, D-16)
  </read_first>
  <behavior>
    - Test 1 (BootstrapSmokeTests.CinemachineCamera_IsConfigured — added in Task 3 below): the CameraRig hierarchy contains a CinemachineCamera component and the Main Camera has a CinemachineBrain. The vcam Lens Mode is Perspective with FOV = 40.
    - Test 2 (BootstrapSmokeTests.UICanvas_IsSafeAreaConfigured — added in Task 3): UICanvas Canvas component is in Screen Space - Camera mode, CanvasScaler matchWidthOrHeight = 0.5, reference resolution = (1080, 1920); a child GameObject has a SafeAreaPanel component.
    - Test 3 (manual visual): pressing Play in Editor frames the warehouse isometrically; on iPhone simulator the SafeAreaPanel anchors leave the notch area uncovered.
    - Test 4 (one-of-a-kind names): the scene YAML contains exactly one `m_Name: UICanvas` and exactly one `m_Name: EventSystem` — proves the Phase 1 empty placeholders from plan 01-02 were deleted before the real components were added (catches the duplicate-name regression deterministically).
  </behavior>
  <action>
**Step A — Replace the placeholder CameraRig with the full Cinemachine rig** (RESEARCH Pattern 3; D-12).

1. Open `Assets/_Project/Scenes/Warehouse_MVP.unity`.
2. **Re-parent the Main Camera under CameraRig.** In Hierarchy, drag `Main Camera` (created with the scene by the URP template in plan 01-02 Task 3 Step A) under `CameraRig`. The MainCamera GameObject must remain named `Main Camera` so Cinemachine and other systems can find it.
3. Select `Main Camera`. In the Inspector:
   - Confirm Camera component exists. Renderer field → set to `URP_Mobile_Renderer` from plan 01-01.
   - **Add Component** → search "Cinemachine Brain" → add it (RESEARCH Pitfall 2 — namespace is `Unity.Cinemachine`).
   - Add Component → AudioListener (if not auto-added by the URP template).
4. **Create the virtual camera as a child of CameraRig:**
   - Right-click `CameraRig` in Hierarchy → Create Empty → name it `CM vcam1`.
   - Select `CM vcam1`. Add Component → search "Cinemachine Camera" → add it. **Verify the namespace is `Unity.Cinemachine.CinemachineCamera`** (Pitfall 2 — NOT `CinemachineVirtualCamera`, that's the deprecated 2.x API).
   - In the Inspector for `CM vcam1`:
     - Transform Position: `(10, 12, -10)` — RESEARCH Pattern 3 default isometric pose.
     - Transform Rotation: `(45, -45, 0)` — ~45° pitch, 45° yaw.
     - CinemachineCamera component: **Tracking Target = (none / null)** (D-12 — Phase 2 will populate this with the Player). **Do not add any Body or Aim procedural component** — leaving them blank makes the vcam passive (acts as a transform reference for the Brain).
     - Lens → **Mode = Perspective** and **Field of View = 40**. **This is locked** (was previously loose between perspective FOV 40 vs orthographic). Phase 2's Cinemachine follow damping is calibrated for Perspective FOV 40, so the choice is pinned here for cross-phase determinism.

5. **Sanity check the framing:** Save scene. Press Play. The Game view should show the floor and all six colored stations at a slight downward isometric angle, all visible in the frame. If a station is cut off, adjust the vcam's `Lens > Field of View` or `Position` until everything fits — but **prefer adjusting Position over FOV**: FOV is locked at 40 by D-12 + this plan; if FOV must change, record the deviation and the new value in `01-03-SUMMARY.md` so Phase 2 can re-tune follow damping.  **Do NOT change the station positions from plan 01-02** — adjust the camera to fit the scene, not the other way around.

**Step B — Replace the empty UICanvas placeholder with a real Screen-Space Camera Canvas + SafeAreaPanel** (RESEARCH Pattern 4; D-15).

1. **Delete the empty UICanvas placeholder GameObject** from plan 01-02 Task 3 Step D. **This step is mandatory** — without it the next sub-step creates a second `UICanvas` and the scene ends up with two GameObjects of the same name, which breaks `GameObject.Find` determinism and the one-of-a-kind name assertion in Task 3.
2. **Recreate UICanvas as a real Canvas:** GameObject menu → UI → Canvas. Rename the new Canvas to exactly `UICanvas` (the menu names it "Canvas" by default). The Canvas creation also auto-adds an EventSystem GameObject — **DELETE that auto-added EventSystem** (we already have ours from plan 01-02; we'll add the InputSystemUIInputModule to it in Step C). Same one-of-a-kind name reasoning applies.
3. Inspector for `UICanvas`:
   - Canvas component:
     - Render Mode: **Screen Space - Camera**
     - Render Camera: drag `CameraRig/Main Camera` from Hierarchy
     - Plane Distance: `1`
     - Sorting Layer: Default
     - Order in Layer: 0
   - Canvas Scaler component (auto-added):
     - UI Scale Mode: **Scale With Screen Size**
     - Reference Resolution: X=1080, Y=1920 (portrait)
     - Screen Match Mode: Match Width Or Height
     - Match: **0.5**
     - Reference Pixels Per Unit: 100
   - GraphicRaycaster: leave default.

4. **Add SafeAreaPanel as a child of UICanvas:**
   - Right-click `UICanvas` → Create Empty Child → name it `SafeAreaPanel`.
   - Inspector for SafeAreaPanel: confirm RectTransform exists. Set anchors to **stretch full** (anchor preset → bottom-right corner stretch icon → Alt + click to set anchors to fill parent: AnchorMin = (0,0), AnchorMax = (1,1)). **You do NOT need to manually set `offsetMin`/`offsetMax` to (0,0)** — `SafeAreaPanel.Apply()` (Task 1) resets both to `Vector2.zero` at runtime so the rect always equals `Screen.safeArea` regardless of any edit-time offsets.
   - Add Component → search "Safe Area Panel" → add the `WM.UI.SafeAreaPanel` component from Task 1.

**Step C — Configure the EventSystem with InputSystemUIInputModule** (RESEARCH Pattern 5; Pitfall 6).

1. Select the existing empty `EventSystem` GameObject from plan 01-02 Task 3 Step D. **Reuse it — do NOT create a second `EventSystem` GameObject** (the auto-added one from Step B's UI > Canvas was already deleted). The scene must contain exactly one `EventSystem` after this step.
2. Add Component → `EventSystem`.
3. Add Component → search "Input System UI Input Module" → add it (RESEARCH Pitfall 6 — must be `InputSystemUIInputModule`, not `StandaloneInputModule`). If Unity prompts "Replace with InputSystemUIInputModule" inline, accept.
4. The InputSystemUIInputModule auto-references `DefaultInputActions` (bundled with the Input System package). Leave that default reference.
5. Verify: after adding both components, the GameObject has exactly two components beyond Transform — `EventSystem` and `Input System UI Input Module`. **No `StandaloneInputModule`** must be present (Pitfall 6 — coexistence causes "two event systems" symptoms).

**Step D — Save the scene.** Save the project. The `Warehouse_MVP.unity` YAML now grows to include CinemachineBrain, CinemachineCamera, Canvas, CanvasScaler, GraphicRaycaster, SafeAreaPanel, EventSystem, and InputSystemUIInputModule serialized data.

**Final Editor sanity:**
- Press Play. Console shows "GameManager initialized" exactly once. Game view shows isometric warehouse framing. UI overlay is empty (no widgets yet) but the Canvas + SafeAreaPanel are present (you can see them in the Hierarchy with a non-zero RectTransform).
- Switch Game view aspect to "iPhone 14 Pro Portrait" (or any portrait preset). Re-press Play. The SafeAreaPanel rect should shrink slightly on phones with a notch/Dynamic Island.
  </action>
  <verify>
    <automated>
# Scene YAML must reference the new components (the .unity scene is text per gitattributes)
grep -q 'CinemachineBrain' Assets/_Project/Scenes/Warehouse_MVP.unity
grep -q 'CinemachineCamera' Assets/_Project/Scenes/Warehouse_MVP.unity
grep -q 'm_Name: CM vcam1' Assets/_Project/Scenes/Warehouse_MVP.unity
grep -q 'm_Name: Main Camera' Assets/_Project/Scenes/Warehouse_MVP.unity
grep -q 'm_Name: UICanvas' Assets/_Project/Scenes/Warehouse_MVP.unity
grep -q 'm_Name: SafeAreaPanel' Assets/_Project/Scenes/Warehouse_MVP.unity
grep -q 'CanvasScaler' Assets/_Project/Scenes/Warehouse_MVP.unity
# 1080x1920 reference resolution
grep -E 'm_ReferenceResolution: \{x: 1080, y: 1920\}' Assets/_Project/Scenes/Warehouse_MVP.unity
# Match = 0.5 (serialized as m_MatchWidthOrHeight)
grep -E 'm_MatchWidthOrHeight: 0\.5' Assets/_Project/Scenes/Warehouse_MVP.unity
# Screen Space - Camera (m_RenderMode: 1 — 0=Overlay, 1=Camera, 2=World)
grep -E 'm_RenderMode: 1' Assets/_Project/Scenes/Warehouse_MVP.unity
# InputSystemUIInputModule on EventSystem
grep -q 'InputSystemUIInputModule' Assets/_Project/Scenes/Warehouse_MVP.unity
# StandaloneInputModule must NOT exist (Pitfall 6)
! grep -q 'StandaloneInputModule' Assets/_Project/Scenes/Warehouse_MVP.unity

# === One-of-a-kind name assertions ===
# Phase 1 placeholders from plan 01-02 must have been deleted before this plan
# recreates UICanvas and adds components to EventSystem. Catches the duplicate-name
# regression deterministically.
test "$(grep -c '^  m_Name: UICanvas$' Assets/_Project/Scenes/Warehouse_MVP.unity)" -eq 1
test "$(grep -c '^  m_Name: EventSystem$' Assets/_Project/Scenes/Warehouse_MVP.unity)" -eq 1

# === Cinemachine vcam Lens FOV pin ===
# CM vcam1 Lens Mode = Perspective, FOV = 40. Cinemachine 3.x serializes the lens
# block under m_Lens with a FieldOfView (or FocalLength when Mode=Physical) entry.
# IMPORTANT: validate the exact key name + value form below against an actual
# Cinemachine 3.1.6 vcam YAML before locking — Cinemachine has historically renamed
# m_Lens fields between minor versions. Best-effort patterns:
grep -q 'm_Lens:' Assets/_Project/Scenes/Warehouse_MVP.unity
# Either FieldOfView: 40 OR (when serialized as float) FieldOfView: 40.0 / 40
grep -qE 'FieldOfView:[[:space:]]*40(\.0)?($|[^0-9])' Assets/_Project/Scenes/Warehouse_MVP.unity || \
  echo "WARN: FieldOfView pin pattern did not match — verify Cinemachine 3.1.6 m_Lens serialization and update this gate in 01-03-SUMMARY.md"
    </automated>
  </verify>
  <done>
    Scene YAML contains CinemachineBrain + CinemachineCamera (named "CM vcam1") with Lens Mode = Perspective and FOV = 40, Canvas in Screen Space - Camera mode at 1080×1920 with Match=0.5, SafeAreaPanel as a child, and EventSystem with InputSystemUIInputModule (no StandaloneInputModule). Scene contains exactly one `UICanvas` and exactly one `EventSystem` (the Phase 1 placeholders from plan 01-02 were deleted, and the auto-added EventSystem from Canvas creation was also deleted). Pressing Play shows isometric warehouse framing and "GameManager initialized" log. iPhone simulator portrait aspect shrinks the SafeAreaPanel rect appropriately.
  </done>
  <acceptance_criteria>
    - Scene YAML grep gates above all pass.
    - `Main Camera` is parented under `CameraRig` and has a `CinemachineBrain` component.
    - `CM vcam1` is parented under `CameraRig` (sibling of Main Camera) and has a `CinemachineCamera` component (3.x API, not 2.x `CinemachineVirtualCamera`). Lens Mode = Perspective, FOV = 40.
    - `UICanvas` is a real Canvas in Screen Space - Camera mode with reference resolution 1080×1920 and Match=0.5; `SafeAreaPanel` is a child with the `WM.UI.SafeAreaPanel` component attached.
    - `EventSystem` has both `EventSystem` and `Input System UI Input Module` components (no `StandaloneInputModule`).
    - Scene contains **exactly one** GameObject named `UICanvas` and **exactly one** named `EventSystem` (grep counts in verify block).
    - Pressing Play in the Editor frames the warehouse from the isometric vcam and logs "GameManager initialized".
  </acceptance_criteria>
</task>

<task type="auto" tdd="true">
  <name>Task 3: Extend BootstrapSmokeTests with Cinemachine + UICanvas + EventSystem assertions</name>
  <files>
    Assets/Tests/EditMode/BootstrapSmokeTests.cs
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-VALIDATION.md (rows 01-03-* — CinemachineCamera_IsConfigured, UICanvas_IsSafeAreaConfigured)
    - The current BootstrapSmokeTests.cs from plan 01-02 Task 3
  </read_first>
  <behavior>
    - Test 1 (BootstrapSmokeTests.CinemachineCamera_IsConfigured): scene contains a `Unity.Cinemachine.CinemachineCamera` component and `Unity.Cinemachine.CinemachineBrain` component (both findable via `Object.FindObjectsByType`). Vcam Lens Mode = Perspective, FOV = 40.
    - Test 2 (BootstrapSmokeTests.UICanvas_IsSafeAreaConfigured): UICanvas Canvas is in Screen Space - Camera mode, CanvasScaler reference resolution = (1080, 1920), matchWidthOrHeight = 0.5; a SafeAreaPanel component is present in the scene.
    - Test 3 (BootstrapSmokeTests.EventSystem_UsesInputSystemUIInputModule): EventSystem GameObject has both `EventSystem` and `InputSystemUIInputModule` components, and does NOT have a `StandaloneInputModule`.
    - All 12 existing `RequiredGameObject_IsPresent` cases from plan 01-02 still pass (no regression).
  </behavior>
  <action>
**Append three new tests** to the existing `Assets/Tests/EditMode/BootstrapSmokeTests.cs` from plan 01-02. The class structure is preserved; new test methods are added inside the same `BootstrapSmokeTests` class.

Replace the entire file content with this exact source (extends, does not delete, the 12 RequiredGameObject_IsPresent test cases):

```csharp
// Source: VALIDATION.md rows 01-02-* (RequiredGameObject_IsPresent) and 01-03-* (Cinemachine, UICanvas, EventSystem checks)
// Covers BOOT-02 — required GameObjects + Cinemachine + UICanvas + EventSystem configuration.
using System.Linq;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using WM.UI;

namespace WM.Tests.EditMode
{
    public class BootstrapSmokeTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Warehouse_MVP.unity";

        [SetUp]
        public void OpenScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        // ---- BOOT-02: 12 required GameObjects (from plan 01-02) ----
        [TestCase("Bootstrap")]
        [TestCase("GameManager")]
        [TestCase("CameraRig")]
        [TestCase("UICanvas")]
        [TestCase("EventSystem")]
        [TestCase("Player")]
        [TestCase("LoadingDock")]
        [TestCase("PackingStation")]
        [TestCase("DeliveryZone")]
        [TestCase("UpgradeStation")]
        [TestCase("ShelfArea")]
        [TestCase("WorkerSpawn")]
        public void RequiredGameObject_IsPresent(string name)
        {
            GameObject go = GameObject.Find(name);
            Assert.That(go, Is.Not.Null,
                $"Required GameObject '{name}' missing from {ScenePath}");
        }

        // ---- BOOT-02 (plan 01-03): Cinemachine 3.x rig ----
        [Test]
        public void CinemachineCamera_IsConfigured()
        {
            var brains = Object.FindObjectsByType<CinemachineBrain>(FindObjectsSortMode.None);
            Assert.That(brains.Length, Is.GreaterThanOrEqualTo(1),
                "Scene must contain at least one CinemachineBrain (expected on Main Camera under CameraRig).");

            var vcams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            Assert.That(vcams.Length, Is.GreaterThanOrEqualTo(1),
                "Scene must contain at least one CinemachineCamera (expected named 'CM vcam1' under CameraRig).");

            // Phase 1: Tracking Target must be null (D-12). Phase 2 sets it to Player.
            Assert.That(vcams[0].Follow, Is.Null,
                "Phase 1 CinemachineCamera must have no Follow target (D-12). Phase 2 will set it to Player.");

            // Phase 1 (this plan): Lens Mode = Perspective, FOV = 40 (locked).
            // Cinemachine 3.x exposes Lens.FieldOfView (perspective) on the LensSettings struct.
            Assert.That(vcams[0].Lens.FieldOfView, Is.EqualTo(40f).Within(0.001f),
                "CinemachineCamera Lens FieldOfView must be 40 (locked here for Phase 2 follow damping determinism).");
        }

        // ---- BOOT-02 (plan 01-03): UICanvas Screen-Space Camera + SafeAreaPanel ----
        [Test]
        public void UICanvas_IsSafeAreaConfigured()
        {
            GameObject ui = GameObject.Find("UICanvas");
            Assert.That(ui, Is.Not.Null, "UICanvas GameObject missing.");

            var canvas = ui.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null, "UICanvas missing Canvas component.");
            Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceCamera),
                "UICanvas must be Screen Space - Camera (D-15).");

            var scaler = ui.GetComponent<CanvasScaler>();
            Assert.That(scaler, Is.Not.Null, "UICanvas missing CanvasScaler.");
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1080, 1920)));
            Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(0.5f).Within(0.001f));

            var safeArea = Object.FindObjectsByType<SafeAreaPanel>(FindObjectsSortMode.None);
            Assert.That(safeArea.Length, Is.GreaterThanOrEqualTo(1),
                "Scene must contain a SafeAreaPanel (expected as child of UICanvas).");
            Assert.That(safeArea[0].transform.parent, Is.EqualTo(ui.transform),
                "SafeAreaPanel must be parented under UICanvas.");
        }

        // ---- BOOT-02 (plan 01-03): EventSystem uses InputSystemUIInputModule ----
        [Test]
        public void EventSystem_UsesInputSystemUIInputModule()
        {
            GameObject es = GameObject.Find("EventSystem");
            Assert.That(es, Is.Not.Null, "EventSystem GameObject missing.");
            Assert.That(es.GetComponent<EventSystem>(), Is.Not.Null,
                "EventSystem GameObject missing EventSystem component.");
            Assert.That(es.GetComponent<InputSystemUIInputModule>(), Is.Not.Null,
                "EventSystem must use InputSystemUIInputModule (D-16, RESEARCH Pitfall 6).");

            // StandaloneInputModule must NOT coexist (RESEARCH Pitfall 6).
            var legacy = es.GetComponent<StandaloneInputModule>();
            Assert.That(legacy, Is.Null,
                "EventSystem must not have a legacy StandaloneInputModule alongside InputSystemUIInputModule.");
        }

        [TearDown]
        public void Cleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
    }
}
```

> **Asmdef references update needed:** `WM.Tests.EditMode.asmdef` from plan 01-01 already references `WM.Core`. To use `Unity.Cinemachine` and `WM.UI` types, the asmdef needs additional references. Update `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef` `references` array to add: `"WM.UI"`, `"Unity.Cinemachine"`, `"Unity.InputSystem"`, `"UnityEngine.UI"`. Final references list:
> ```json
> "references": [
>     "WM.Core",
>     "WM.UI",
>     "Unity.Cinemachine",
>     "Unity.InputSystem",
>     "UnityEngine.UI",
>     "UnityEngine.TestRunner",
>     "UnityEditor.TestRunner"
> ]
> ```
> The Unity asmdef inspector exposes these via "Add Reference" dropdown — type each name and select. After saving, Editor will recompile.

Run `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -`. All cases must pass.
  </action>
  <verify>
    <automated>
test -f Assets/Tests/EditMode/BootstrapSmokeTests.cs

# Three new tests present
grep -q 'public void CinemachineCamera_IsConfigured' Assets/Tests/EditMode/BootstrapSmokeTests.cs
grep -q 'public void UICanvas_IsSafeAreaConfigured' Assets/Tests/EditMode/BootstrapSmokeTests.cs
grep -q 'public void EventSystem_UsesInputSystemUIInputModule' Assets/Tests/EditMode/BootstrapSmokeTests.cs

# Cinemachine FOV pin assertion present in the test source
grep -q 'Lens.FieldOfView' Assets/Tests/EditMode/BootstrapSmokeTests.cs

# Existing 12 RequiredGameObject_IsPresent cases preserved
for name in Bootstrap GameManager CameraRig UICanvas EventSystem Player LoadingDock PackingStation DeliveryZone UpgradeStation ShelfArea WorkerSpawn; do
  grep -q "\[TestCase(\"$name\")\]" Assets/Tests/EditMode/BootstrapSmokeTests.cs || { echo "Regression — TestCase missing: $name"; exit 1; }
done

# Asmdef references updated to include WM.UI + Unity.Cinemachine + Unity.InputSystem + UnityEngine.UI
grep -q '"WM.UI"' Assets/Tests/EditMode/WM.Tests.EditMode.asmdef
grep -q '"Unity.Cinemachine"' Assets/Tests/EditMode/WM.Tests.EditMode.asmdef
grep -q '"Unity.InputSystem"' Assets/Tests/EditMode/WM.Tests.EditMode.asmdef
grep -q '"UnityEngine.UI"' Assets/Tests/EditMode/WM.Tests.EditMode.asmdef

# CLI gate (replace UNITY)
# UNITY=/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity
# "$UNITY" -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -
echo "Run EditMode CLI command above. Required: 12 RequiredGameObject_IsPresent + CinemachineCamera_IsConfigured + UICanvas_IsSafeAreaConfigured + EventSystem_UsesInputSystemUIInputModule + 3 BootstrapStructureTests cases all green."
    </automated>
  </verify>
  <done>
    `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` exits 0 with all of the following green: 12 `BootstrapSmokeTests.RequiredGameObject_IsPresent` cases (from plan 01-02), 3 new `BootstrapSmokeTests.{CinemachineCamera_IsConfigured, UICanvas_IsSafeAreaConfigured, EventSystem_UsesInputSystemUIInputModule}` (Cinemachine assertion now also pins Lens.FieldOfView == 40), and the 3 `BootstrapStructureTests.*` from plan 01-01. PlayMode test from plan 01-02 (`Scene_Loads_GameManagerInitializes`) still green.
  </done>
  <acceptance_criteria>
    - File `Assets/Tests/EditMode/BootstrapSmokeTests.cs` contains all three new test methods AND all 12 existing `RequiredGameObject_IsPresent` cases.
    - `CinemachineCamera_IsConfigured` asserts `Lens.FieldOfView == 40f` (within 0.001 tolerance).
    - `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef` `references` includes `WM.UI`, `Unity.Cinemachine`, `Unity.InputSystem`, `UnityEngine.UI`.
    - `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` exits 0.
    - `<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -` continues to exit 0 (no regression).
  </acceptance_criteria>
</task>

</tasks>

<threat_model>
## Trust Boundaries

| Boundary | Description |
|----------|-------------|
| Active Input Handling setting → runtime input | Misconfigured value silently breaks all input on device |
| Scene → Build Settings | Missing scene registration → black screen on iOS device |
| Cinemachine namespace → script references | Wrong namespace (`Cinemachine` vs `Unity.Cinemachine`) silently fails to find component |

## STRIDE Threat Register

| Threat ID | Category | Component | Disposition | Mitigation Plan |
|-----------|----------|-----------|-------------|-----------------|
| T-01-05 | Tampering | Cinemachine API drift (2.x → 3.x) | mitigate | RESEARCH Pitfall 2 + 3. Task 2 explicitly requires `Unity.Cinemachine.CinemachineCamera` (3.x) and `Unity.Cinemachine.CinemachineBrain`. Task 3 EditMode test uses these exact types via `using Unity.Cinemachine` — compile failure surfaces drift early. |
| T-01-06 | DoS (input dead) | EventSystem misconfiguration | mitigate | RESEARCH Pitfall 6 + 7. Task 2 Step C explicitly removes `StandaloneInputModule` and adds `InputSystemUIInputModule`. Task 3 EditMode test asserts both presence of `InputSystemUIInputModule` AND absence of `StandaloneInputModule`. Plan 01-01 Task 1 Step D set Active Input Handling = "Input System Package (New)". |
| T-01-07 | DoS (black-screen ship) | Scene not in Build Settings | mitigate | Plan 01-02 Task 3 Step F adds `Warehouse_MVP.unity` to `EditorBuildSettings.scenes`. Plan 01-01 Task 2 BuildScript also passes the scene explicitly via `BuildPlayerOptions.scenes`. Both belt-and-braces (Pitfall 9). |
| T-01-08 | Information Disclosure | UICanvas `SafeAreaPanel` runtime calculation | accept | Reads `Screen.safeArea` (no PII, OS-provided rect). No mitigation required. |

No HIGH-severity threats — Phase 1 has no runtime auth, persistence, or network surface. All four threats are MEDIUM/LOW build/configuration hygiene.
</threat_model>

<verification>
**Phase-level checks for plan 01-03:**

1. **Cinemachine rig:** `BootstrapSmokeTests.CinemachineCamera_IsConfigured` green — `CinemachineBrain` on Main Camera, `CinemachineCamera` on `CM vcam1`, no Tracking Target (Phase 2 will set it), Lens Mode = Perspective and Lens.FieldOfView == 40 (locked).
2. **UI Canvas:** `BootstrapSmokeTests.UICanvas_IsSafeAreaConfigured` green. Screen Space - Camera, 1080×1920, Match=0.5, SafeAreaPanel as child.
3. **EventSystem:** `BootstrapSmokeTests.EventSystem_UsesInputSystemUIInputModule` green. No legacy StandaloneInputModule.
4. **One-of-a-kind names:** scene YAML grep counts confirm exactly one `m_Name: UICanvas` and exactly one `m_Name: EventSystem` (Task 2 verify block).
5. **No regression:** All tests from plan 01-01 (BootstrapStructureTests x3) and plan 01-02 (RequiredGameObject_IsPresent x12, PlayModeSmokeTests.Scene_Loads_GameManagerInitializes) remain green.
6. **Manual visual gate (recorded in 01-VERIFICATION.md):** Press Play → game view shows isometric framing of empty warehouse with all six colored stations visible. iPhone 14 Pro Portrait aspect → SafeAreaPanel rect shrinks to leave Dynamic Island uncovered.
7. **iOS build smoke (recorded in 01-VERIFICATION.md):** `<UNITY> -batchmode -quit -projectPath . -buildTarget iOS -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -` exits 0 and produces `build/ios/Unity-iPhone.xcodeproj`. Open in Xcode → Build & Run on iPhone 14 simulator → app boots showing the isometric warehouse view (phase success criterion 3).
</verification>

<success_criteria>
- All EditMode tests green: 3 BootstrapStructureTests (plan 01-01) + 12 RequiredGameObject_IsPresent + 3 new (CinemachineCamera_IsConfigured with FOV pin, UICanvas_IsSafeAreaConfigured, EventSystem_UsesInputSystemUIInputModule).
- PlayMode test green: `PlayModeSmokeTests.Scene_Loads_GameManagerInitializes`.
- Pressing Play in Editor: scene renders the empty warehouse from a fixed isometric camera; "GameManager initialized" logs once; no exceptions, no warnings about missing modules.
- Scene YAML contains exactly one `UICanvas` and exactly one `EventSystem` (one-of-a-kind regression gate).
- iPhone simulator (manual): `BuildScript.BuildIOS` exits 0, the resulting Xcode project builds, and the simulator shows the warehouse from the configured Cinemachine camera (phase success criterion 3 — recorded in `01-VERIFICATION.md`).
- ROADMAP Phase 1 success criteria 1–3 all observable end-to-end.
</success_criteria>

<output>
After completion, create `.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-03-SUMMARY.md` recording:
- The final Cinemachine vcam Position/Rotation/FOV used (in case the executor adjusted Position from the Pattern 3 defaults to fit the warehouse — FOV must remain 40; if FOV had to deviate, record the new value and surface for Phase 2 follow-damping re-tuning).
- Confirmation that Active Input Handling = "Input System Package (New)" (re-verify after this plan since it requires editor restart).
- The actual Cinemachine 3.1.6 `m_Lens` YAML serialization (key name + value form for `FieldOfView`) so the grep gate in Task 2 verify block can be locked into a deterministic pattern (or updated if the best-effort pattern did not match the real serialized output).
- The iPhone simulator screenshot path or note ("Visual gate recorded in 01-VERIFICATION.md").
- Note any deviation from D-12 (e.g., orthographic vs. perspective camera) with rationale.

## Phase 2 handoff note

Phase 2 (player movement + Cinemachine follow target) MUST re-confirm the vcam's Lens Mode and FieldOfView before adding Follow damping. Phase 1 locks **Lens Mode = Perspective, FOV = 40**; Phase 2's damping coefficients are calibrated against this exact lens. If Phase 2 needs to switch to orthographic or change FOV for follow framing, treat it as an explicit cross-phase decision, document it in the Phase 2 plan, and re-run the `CinemachineCamera_IsConfigured` test with the new expected value.
</output>
</content>
</invoke>