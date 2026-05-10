# Phase 1: Project Bootstrap & Empty Warehouse Scene - Research

**Researched:** 2026-05-10
**Domain:** Unity 6 LTS iOS bootstrap (URP, New Input System, Cinemachine 3.x, IL2CPP, asmdefs)
**Confidence:** HIGH (Unity-published docs are primary), MEDIUM on a few mobile-aggressive specifics

## Summary

This phase stands up an empty Unity iOS project with a scaffolded `Warehouse_MVP` scene that builds and launches via Xcode. CONTEXT.md locks the major decisions (Unity 6 LTS, URP mobile-aggressive, New Input System, Cinemachine virtual camera, IL2CPP/ARM64/Metal/iOS 15, plain-C# composition root, 13+2 asmdefs, Screen-Space Camera UI 1080×1920 with safe area, .gitignore + LFS). Research focuses on filling implementation gaps: exact package versions, URP asset settings, Cinemachine 3.x API, asmdef wiring, iOS Player Settings checklist, BuildScript exit-code semantics, .gitattributes content, EditMode/PlayMode smoke test patterns, and Unity 6 + iOS-specific pitfalls.

**Primary recommendation:** Pin Unity **6000.3.7f1** (Unity 6.3 LTS, latest as of Feb 2026) [VERIFIED]. Use the URP package shipped with that editor. Install Cinemachine 3.1.6 and Input System 1.15.0 explicitly via Package Manager (neither is pre-installed in a fresh URP 3D template) [VERIFIED]. Build the scene with primitives + colored URP/Lit materials, wire a single CinemachineCamera with no follow target ("Do Nothing" position control), set Active Input Handling to "Input System Package (New)" only, and ship a `WM.Editor` asmdef containing `BuildScript.cs` that returns explicit exit codes via `EditorApplication.Exit()`. Scaffold both EditMode and PlayMode test asmdefs from day one with a single passing smoke test each, plus one EditMode test that opens the scene and asserts the required GameObjects are present.

## User Constraints (from CONTEXT.md)

### Locked Decisions

**Engine & Render Pipeline**
- **D-01:** Unity 6 LTS (6000.x). Pin via `ProjectSettings/ProjectVersion.txt` after init. Recommended for new iOS projects in 2026 — RenderGraph URP, GPU Resident Drawer, better mobile perf headroom than 2022.3 LTS.
- **D-02:** URP **Mobile-aggressive** quality preset: MSAA off, no realtime shadows (baked only), HDR off, no post-FX. Targets 60fps on iPhone 8/SE class. Single URP asset for MVP.

**Input**
- **D-03:** New Input System package (`com.unity.inputsystem`). Player Settings → Active Input Handling = `Input System Package (New)`.

**Project Structure & Asmdefs**
- **D-04:** One asmdef per system folder. Concrete names: `WM.Core`, `WM.Player`, `WM.Boxes`, `WM.Orders`, `WM.Stations`, `WM.Upgrades`, `WM.Workers`, `WM.Economy`, `WM.Save`, `WM.Analytics`, `WM.Monetization`, `WM.UI`, `WM.Editor`.
- **D-05:** Folder layout follows `specs/06-technical-architecture-spec.md` §3 verbatim under `Assets/_Project/`. ScriptableObject folders created empty in this phase.
- **D-06:** Test asmdefs scaffolded from day one: `WM.Tests.EditMode` and `WM.Tests.PlayMode`. Unity Test Framework installed. Each contains a single passing smoke test.

**Service Composition / Bootstrap**
- **D-07:** Plain C# composition root pattern (`Bootstrap.cs` MonoBehaviour in `WM.Core`). On `Awake` it news up plain-C# services behind their interfaces using mock/stub implementations and injects them via explicit `Init(...)` calls. No singletons, no DI container, no SO service locator.
- **D-08:** `GameManager` MonoBehaviour exists in scene as thin orchestrator; in Phase 1 only logs "GameManager initialized" on `Start`. Receives services from `Bootstrap` via `Init(...)`.
- **D-09:** Service interfaces (`IAnalyticsService`, `IAdService`, `IIapService`, `ISaveService`) defined in this phase under `WM.Core` with no-op stub implementations.

**Scene Composition**
- **D-10:** Single root scene `Assets/_Project/Scenes/Warehouse_MVP.unity`.
- **D-11:** Required GameObjects: `Player`, `LoadingDock`, `PackingStation`, `DeliveryZone`, `UpgradeStation`, `ShelfArea`, `WorkerSpawn`, `CameraRig`, `UICanvas`, `EventSystem`, `GameManager`, `Bootstrap`. Each station with placeholder colored material.
- **D-12:** Camera rig uses Cinemachine virtual camera. Fixed isometric framing, ~45° pitch, no follow target in this phase.

**Placeholder Geometry**
- **D-13:** Unity primitives + colored materials only. Floor: scaled `Plane`. Stations: `Cube`/`Cylinder` with distinct colors (loading dock = grey, packing = blue, delivery = green, upgrade = yellow, shelf = brown, worker spawn = magenta).
- **D-14:** Placeholder materials in `Assets/_Project/Materials/Placeholder/`. URP/Lit shader, single base color each.

**UI Canvas**
- **D-15:** UI canvas: Screen Space - Camera mode, reference resolution 1080×1920 (portrait), `Match Width Or Height = 0.5`. Safe-area aware via `SafeAreaPanel` component.
- **D-16:** EventSystem uses Input System UI Input Module.

**Build & Player Settings**
- **D-17:** Player Settings: portrait orientation only, iOS bundle identifier placeholder `com.warehousemaster.mvp`, minimum iOS 15.0, Metal API only, IL2CPP scripting backend, ARM64 architecture.
- **D-18:** `BuildScript.cs` skeleton in `WM.Editor`. Single `BuildIOS` static method. Phase 11 fleshes out signing/archive — Phase 1 only ensures the entry point compiles and produces an Xcode project.

**Repo Hygiene**
- **D-19:** Unity-flavored `.gitignore` at repo root. Keep `Packages/manifest.json` and all `.meta` files tracked.
- **D-20:** Git LFS configured for binary asset extensions. `.gitattributes` committed.

### Claude's Discretion

- **D-07** composition root choice (Plain C# over Singleton/SO locator/VContainer) — confirmed by user.
- Cinemachine inclusion (D-12), placeholder palette (D-13), UI scaling rules (D-15), iOS minimum version (D-17), `.gitignore`/LFS rule set (D-19, D-20) — Claude defaults, confirmed during deferred-items step.

### Deferred Ideas (OUT OF SCOPE)

- Quality tier variants (low/mid/high URP profiles) — single mobile-aggressive preset only.
- Cinemachine follow target — Phase 2 alongside joystick.
- Real bundle identifier + signing — Phase 11.
- Localization scaffolding — not in MVP.
- `Resources/` vs Addressables — not needed in Phase 1; decide when first prefab loading need surfaces (likely Phase 3).

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| BOOT-01 | Unity project initialized for iOS with URP and `Assets/_Project/...` folder structure (Scripts subfolders per system, ScriptableObjects subfolders per data type) | Standard Stack §URP, §Input System, §Cinemachine; Project Structure tree; Asmdef wiring matrix; iOS Player Settings checklist |
| BOOT-02 | `Warehouse_MVP` scene created with all required objects (player spawn, loading dock, packing station, delivery zone, upgrade station, shelf area, worker spawn, camera rig, UI canvas, event system, `GameManager`) | Scene Composition pattern; Cinemachine 3.x setup; UI Canvas + Safe Area pattern; EventSystem + Input System UI Input Module; Smoke test asserting GameObject presence |

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Project skeleton (folders, asmdefs) | Editor / Project metadata | — | Pure repo and editor configuration |
| Scene composition (`Warehouse_MVP.unity`) | Unity Scene / Asset | Runtime | Static scene asset; comes alive only at Play |
| Service interfaces (`I*Service`) | Runtime / `WM.Core` library | — | Plain C# in core asmdef, referenced by all consumers |
| `Bootstrap` composition root | Runtime / Scene MonoBehaviour | `WM.Core` services | MonoBehaviour orchestrator; news up services and injects them |
| `GameManager` | Runtime / Scene MonoBehaviour | `WM.Core` | Thin scene-side orchestrator (Phase 1 = log line only) |
| Camera rig (Cinemachine) | Runtime / Scene | URP graphics | CinemachineCamera + CinemachineBrain on Main Camera |
| UI Canvas + Safe Area | Runtime / Scene UI | `WM.UI` asmdef | Screen-Space Camera at 1080×1920; safe-area script lives in `WM.UI` |
| EventSystem | Runtime / Scene UI | Input System | Uses `InputSystemUIInputModule`, paired with new Input System |
| `BuildScript.BuildIOS` | Editor-only | `WM.Editor` asmdef | Editor utility; not shipped at runtime |
| Tests | Editor-only / Play-mode | `WM.Tests.EditMode`, `WM.Tests.PlayMode` | Test assemblies; don't ship in build |

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Unity Editor | **6000.3.7f1** (Unity 6.3 LTS) | Engine | Latest 6.3 LTS sub-version released Feb 5 2026; LTS supported until Dec 2027; production-ready for iOS [VERIFIED: unity.com whats-new] |
| Universal Render Pipeline | **17.x** (ships with 6000.3.x) | Render pipeline | Mobile-friendly SRP; only one supported by Unity 6 mobile-aggressive workflows; RenderGraph default-on [VERIFIED: docs.unity3d.com 6000.3 URP] |
| New Input System (`com.unity.inputsystem`) | **1.15.0** | Touch / joystick input + UI input module | Required by D-03; standard for new Unity projects since 2022; iOS touch handling is robust and well-documented [VERIFIED: discussions.unity.com release thread Oct 2025] |
| Cinemachine (`com.unity.cinemachine`) | **3.1.6** | Virtual camera framing | Free, official Unity package; Cinemachine 3 is the supported track on Unity 6.x; 3.1.6 minimum requirement is "2022.3 LTS and later" so 6.3 LTS is in range [VERIFIED: docs.unity3d.com Cinemachine 3.1] |
| Unity Test Framework (`com.unity.test-framework`) | **2.0.x** (Unity 6 default) | EditMode + PlayMode tests | Required by D-06; ships with Unity 6; brings NUnit + UnityEngine.TestRunner refs [VERIFIED: docs.unity3d.com test-framework 2.0] |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| TextMeshPro | bundled with Unity 6 (UGUI package) | UI text rendering | Standard for any UI text widget; no widgets in Phase 1, but EventSystem wiring + Canvas creation will pull it in automatically |
| URP/Lit shader | bundled with URP | Placeholder materials | D-14: single base color, no advanced features |

**Note on what we are NOT installing in Phase 1:** No DI container (VContainer/Zenject) — D-07 is plain C# composition root. No third-party tween/audio/save libraries — out of scope until owning phases. No ProBuilder, no Asset Store packs (D-13).

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Unity 6.3 LTS | Unity 2022.3 LTS | 2022.3 still supported but lacks RenderGraph default, GPU Resident Drawer, and modern URP Mobile features. Locked out by D-01. |
| Cinemachine 3.x | Plain Camera + script | Hand-rolled isometric camera works for Phase 1 (no follow needed yet) but Phase 2 immediately needs follow + damping. Pre-installing Cinemachine avoids re-wiring later. |
| Plain C# composition root | VContainer / Zenject | DI container is unjustified at MVP scale and contradicts CLAUDE.md "no unspecified packages." Locked by D-07. |
| Cinemachine `CinemachineCamera` (3.x) | Legacy `CinemachineVirtualCamera` (2.x) | 3.x is the API on Unity 6; 2.x components still load via the upgrader but new code should not target them. |

**Installation:**

```bash
# Done via Unity Package Manager UI or Packages/manifest.json edit (no shell command).
# manifest.json entries to add (versions must match the editor's manifest pinning rules):
#   "com.unity.inputsystem": "1.15.0"
#   "com.unity.cinemachine": "3.1.6"
#   "com.unity.test-framework": (already in default manifest)
#   "com.unity.render-pipelines.universal": (already in default manifest when URP template is chosen)
```

**Version verification:** Versions confirmed against Unity package release threads (Oct 2025 for Input System 1.15.0; Cinemachine 3.1.6 docs page references Unity 6000.6 build). For final pinning, consult `Packages/manifest.json` after Unity Hub creates the project — Unity will inject the editor-matched versions automatically.

## Architecture Patterns

### System Architecture Diagram

```
[Unity Hub creates project]
        ↓
[ProjectSettings/ProjectVersion.txt = 6000.3.7f1]
        ↓
[Packages/manifest.json: URP + Input System 1.15 + Cinemachine 3.1.6 + Test Framework]
        ↓
[Assets/_Project/ folder tree (Scripts/*, ScriptableObjects/*, Scenes/, Materials/, Settings/)]
        ↓
[13 runtime asmdefs (WM.Core, WM.Player, ..., WM.UI, WM.Editor) + 2 test asmdefs]
        ↓
[Warehouse_MVP.unity scene]
   ├─ Bootstrap (MonoBehaviour, Awake): news up stub services → injects via Init(...)
   ├─ GameManager (MonoBehaviour, Start): logs "GameManager initialized"
   ├─ CameraRig
   │     ├─ MainCamera (CinemachineBrain)
   │     └─ CM vcam1 (CinemachineCamera, no Tracking Target, ~45° pitch)
   ├─ UICanvas (Screen-Space Camera, 1080×1920)
   │     └─ SafeAreaPanel (anchors to Screen.safeArea)
   ├─ EventSystem (InputSystemUIInputModule)
   ├─ Player (placeholder primitive)
   └─ LoadingDock / PackingStation / DeliveryZone / UpgradeStation / ShelfArea / WorkerSpawn (positioned primitives, colored URP/Lit materials)
        ↓
[BuildScript.BuildIOS] — produces build/ios/ Xcode project
        ↓
[Xcode opens, archives, runs on simulator/device → empty warehouse from configured camera]
```

### Recommended Project Structure

Follows `specs/06-technical-architecture-spec.md` §3 verbatim.

```
warehouse-maters/
├── .gitignore                          # Unity-flavored (D-19)
├── .gitattributes                      # Git LFS rules (D-20)
├── Assets/
│   └── _Project/
│       ├── Art/                        # (empty in Phase 1)
│       ├── Audio/                      # (empty)
│       ├── Materials/
│       │   └── Placeholder/            # URP/Lit color materials
│       │       ├── Mat_Floor.mat
│       │       ├── Mat_LoadingDock.mat (grey)
│       │       ├── Mat_PackingStation.mat (blue)
│       │       ├── Mat_DeliveryZone.mat (green)
│       │       ├── Mat_UpgradeStation.mat (yellow)
│       │       ├── Mat_Shelf.mat (brown)
│       │       └── Mat_WorkerSpawn.mat (magenta)
│       ├── Prefabs/                    # (empty)
│       ├── Scenes/
│       │   └── Warehouse_MVP.unity     # the only scene
│       ├── Scripts/
│       │   ├── Core/                   # asmdef WM.Core
│       │   │   ├── Bootstrap.cs
│       │   │   ├── GameManager.cs
│       │   │   ├── IAnalyticsService.cs
│       │   │   ├── IAdService.cs
│       │   │   ├── IIapService.cs
│       │   │   ├── ISaveService.cs
│       │   │   └── Stubs/
│       │   │       ├── NullAnalyticsService.cs
│       │   │       ├── NullAdService.cs
│       │   │       ├── NullIapService.cs
│       │   │       └── NullSaveService.cs
│       │   ├── Player/                 # WM.Player (empty in Phase 1)
│       │   ├── Boxes/                  # WM.Boxes
│       │   ├── Orders/                 # WM.Orders
│       │   ├── Stations/               # WM.Stations
│       │   ├── Upgrades/               # WM.Upgrades
│       │   ├── Workers/                # WM.Workers
│       │   ├── Economy/                # WM.Economy
│       │   ├── Save/                   # WM.Save
│       │   ├── Analytics/              # WM.Analytics
│       │   ├── Monetization/           # WM.Monetization
│       │   ├── UI/                     # WM.UI
│       │   │   └── SafeAreaPanel.cs
│       │   └── Editor/                 # WM.Editor (Editor-only platform)
│       │       └── BuildScript.cs
│       ├── ScriptableObjects/
│       │   ├── Boxes/                  # (empty)
│       │   ├── Orders/                 # (empty)
│       │   ├── Upgrades/               # (empty)
│       │   ├── Workers/                # (empty)
│       │   └── Economy/                # (empty)
│       └── Settings/                   # URP asset, Renderer asset, Quality settings
│           ├── URP_Mobile.asset
│           └── URP_Mobile_Renderer.asset
├── Assets/Tests/                       # test root, separate from _Project to keep test code excluded from build
│   ├── EditMode/                       # WM.Tests.EditMode asmdef (Editor platform only)
│   │   └── BootstrapSmokeTests.cs
│   └── PlayMode/                       # WM.Tests.PlayMode asmdef
│       └── PlayModeSmokeTests.cs
├── Packages/
│   └── manifest.json
├── ProjectSettings/                    # all .asset files committed
└── specs/, .planning/, CLAUDE.md       # pre-existing
```

> **Note:** Tests can also live under `Assets/_Project/Scripts/Tests/` — the location only matters that asmdef references resolve. Placing them at `Assets/Tests/` is a common convention because it visually separates production code from tests.

### Pattern 1: Asmdef Dependency Graph

**What:** Unidirectional asmdef references rooted at `WM.Core`. Cyclic refs are forbidden by Unity [VERIFIED: docs.unity3d.com asmdef].
**When to use:** Always for production code in this project.
**Wiring matrix:**

| Asmdef | References (asmdef-level) | Notes |
|--------|---------------------------|-------|
| `WM.Core` | (none — root) | Defines service interfaces and Bootstrap |
| `WM.Player` | `WM.Core` | Phase 2 will add input dependency |
| `WM.Boxes` | `WM.Core` | |
| `WM.Orders` | `WM.Core`, `WM.Boxes` | Orders reference box types |
| `WM.Stations` | `WM.Core`, `WM.Boxes`, `WM.Orders` | Packing station consumes orders |
| `WM.Upgrades` | `WM.Core`, `WM.Economy` | Upgrades deduct cash |
| `WM.Workers` | `WM.Core`, `WM.Boxes` | |
| `WM.Economy` | `WM.Core` | CurrencyManager |
| `WM.Save` | `WM.Core` | ISaveService implementation later |
| `WM.Analytics` | `WM.Core` | IAnalyticsService implementation later |
| `WM.Monetization` | `WM.Core`, `WM.Analytics` | Ads emit analytics |
| `WM.UI` | `WM.Core`, `WM.Economy`, `WM.Orders`, `WM.Upgrades` | UI reads game state |
| `WM.Editor` | `WM.Core` (only when needed); platform = Editor only | BuildScript |
| `WM.Tests.EditMode` | `WM.Core`, `WM.UI`, plus `nunit.framework`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner` | Editor platform only; "Test Assemblies" toggle on |
| `WM.Tests.PlayMode` | `WM.Core`, plus `nunit.framework`, `UnityEngine.TestRunner` (NO UnityEditor.TestRunner) | Any Platform; "Test Assemblies" toggle on |

**Key rules** [VERIFIED: docs.unity.cn test-framework]:
- Test assemblies have references to `nunit.framework.dll`, `UnityEngine.TestRunner`, and `UnityEditor.TestRunner` (last one is **EditMode-only**).
- The "Test Assemblies" toggle in the asmdef inspector adds the `TestAssemblies` define automatically and excludes the asmdef from non-test builds.
- `WM.Editor` must have **Platforms → Editor** checked exclusively, otherwise `BuildScript` symbols leak into player builds.

### Pattern 2: Bootstrap Composition Root

**What:** Single `Bootstrap` MonoBehaviour in the scene constructs services (plain C# classes implementing `I*Service` from `WM.Core`) and hands them to MonoBehaviour consumers via explicit `Init(...)` calls. No singletons, no statics, no DI container.
**When to use:** Always — D-07 locks this.
**Example:**

```csharp
// Source: Decision D-07 + spec 06 §4 (GameManager: "Initialize services")
// Assets/_Project/Scripts/Core/Bootstrap.cs (asmdef WM.Core)
using UnityEngine;

namespace WM.Core
{
    [DefaultExecutionOrder(-100)] // run before everything else
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            IAnalyticsService analytics = new NullAnalyticsService();
            IAdService ads = new NullAdService();
            IIapService iap = new NullIapService();
            ISaveService save = new NullSaveService();

            gameManager.Init(analytics, ads, iap, save);
        }
    }

    public sealed class GameManager : MonoBehaviour
    {
        private IAnalyticsService _analytics;
        // ... other service fields

        public void Init(IAnalyticsService analytics, IAdService ads, IIapService iap, ISaveService save)
        {
            _analytics = analytics;
            // store others...
        }

        private void Start()
        {
            Debug.Log("GameManager initialized");
        }
    }
}
```

`DefaultExecutionOrder(-100)` ensures Bootstrap.Awake runs before consumer Awakes, so consumer Start can rely on `Init` having been called. [CITED: docs.unity3d.com DefaultExecutionOrder]

### Pattern 3: Cinemachine 3.x Camera Rig

**What:** A CameraRig GameObject containing the Main Camera (with `CinemachineBrain` component) and a child empty GameObject with a `CinemachineCamera` component. No Tracking Target in Phase 1.
**When to use:** Phase 1 — fixed isometric framing. Phase 2 will set Tracking Target on the same vcam.
**Example setup:**

```
CameraRig (empty GameObject at world origin)
├── Main Camera (Camera + CinemachineBrain + AudioListener)
│       Position: free; Brain drives it
│       Clear Flags: Skybox or Solid Color (placeholder)
│       Renderer: URP_Mobile_Renderer (no post-FX)
└── CM vcam1 (empty GameObject + CinemachineCamera)
        Position: chosen for isometric look (e.g., (10, 12, -10))
        Rotation: ~(45, -45, 0) → ~45° pitch, 45° yaw
        Tracking Target: (none — leave null)
        Position Control: (none / "Do Nothing" — transform is authored, not procedural)
        Rotation Control: (none)
        Lens: Field of View ~40 (orthographic optional; perspective with low FOV reads isometric)
```

**Key API note:** In Cinemachine 3.x the runtime component is `CinemachineCamera` (one word, no "Virtual"), namespace `Unity.Cinemachine`. The 2.x `CinemachineVirtualCamera` is deprecated; 3.x has many breaking changes from 2.x [VERIFIED: docs.unity3d.com Cinemachine 3.0/3.1]. With no procedural Position/Rotation Control component, the CinemachineCamera is **passive** — it acts as a transform reference for the Brain, which is exactly what Phase 1 needs.

### Pattern 4: UI Canvas with Safe Area

**What:** Screen-Space Camera Canvas at 1080×1920 reference, plus a `SafeAreaPanel` MonoBehaviour that resizes a child RectTransform to `Screen.safeArea` so notch/Dynamic Island areas are avoided.
**Example:**

```csharp
// Source: D-15 + community pattern (Screen.safeArea API since Unity 2017.2)
// Assets/_Project/Scripts/UI/SafeAreaPanel.cs (asmdef WM.UI)
using UnityEngine;

namespace WM.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake() => _rt = GetComponent<RectTransform>();

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastScreen.x || Screen.height != _lastScreen.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastSafeArea = Screen.safeArea;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            Vector2 anchorMin = _lastSafeArea.position;
            Vector2 anchorMax = _lastSafeArea.position + _lastSafeArea.size;
            anchorMin.x /= Screen.width;  anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;  anchorMax.y /= Screen.height;

            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
        }
    }
}
```

**Canvas wiring:**
- Canvas → Render Mode: Screen Space - Camera, Render Camera = Main Camera, Plane Distance = 1.
- CanvasScaler → UI Scale Mode: Scale With Screen Size, Reference Resolution: 1080×1920, Match: 0.5, Reference Pixels Per Unit: 100.
- GraphicRaycaster on Canvas (default).
- Add a child empty RectTransform "SafeAreaPanel" (anchors stretched, full size) with the `SafeAreaPanel` component.

[VERIFIED: docs.unity3d.com Screen.safeArea]

### Pattern 5: EventSystem with Input System UI Input Module

**What:** A single EventSystem in the scene using `InputSystemUIInputModule` (drop-in replacement for the legacy `StandaloneInputModule`).
**Setup steps** [CITED: Input System docs UISupport]:
1. Create empty GameObject named `EventSystem`.
2. Add `EventSystem` component.
3. Add `Input System UI Input Module` component (Unity offers a one-click "Replace with InputSystemUIInputModule" button when adding an EventSystem in a project where the new Input System is active).
4. Leave the default Input Actions asset reference (DefaultInputActions ships with the package).

### Pattern 6: BuildScript.BuildIOS (skeleton)

**What:** A static method invoked by `Unity -batchmode ... -executeMethod WM.Editor.BuildScript.BuildIOS`. Phase 1 ships only a skeleton that exits the editor with a non-zero code on failure.
**Why explicit exit code:** Default `BuildPipeline.BuildPlayer` does **not** propagate failure into Unity's process exit code [VERIFIED: support.unity.com 211195263]. CI pipelines silently report success unless we call `EditorApplication.Exit(1)`.

```csharp
// Source: Unity scripting reference + support article 211195263
// Assets/_Project/Scripts/Editor/BuildScript.cs (asmdef WM.Editor, Editor-platform-only)
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WM.Editor
{
    public static class BuildScript
    {
        // Invoked from CLI:
        //   Unity -batchmode -quit -projectPath . -buildTarget iOS \
        //         -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -
        public static void BuildIOS()
        {
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "build", "ios");
            Directory.CreateDirectory(outputPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/Warehouse_MVP.unity" },
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildIOS] Succeeded. Output: {outputPath}, size: {summary.totalSize} bytes, time: {summary.totalTime}");
            }
            else
            {
                Debug.LogError($"[BuildIOS] Build result: {summary.result}. Errors: {summary.totalErrors}");
                EditorApplication.Exit(1);
            }
        }
    }
}
```

[VERIFIED: docs.unity3d.com BuildPipeline.BuildPlayer; support.unity.com 211195263]

### Anti-Patterns to Avoid

- **Singleton `GameManager`** — contradicts D-07; hides global state and breaks testability.
- **`ScriptableObject` service locator** — same problem; D-07 explicitly chose plain C# composition root.
- **Cyclic asmdef references** — Unity rejects them at compile time; if A references B, B can never reference A [VERIFIED: docs.unity3d.com ScriptCompilationAssemblyDefinitionFiles].
- **Cinemachine 2.x `CinemachineVirtualCamera`** — deprecated in 3.x; use `CinemachineCamera`.
- **Mixing legacy Input Manager + InputSystemUIInputModule without setting Active Input Handling = New** — produces "two event systems" symptoms [VERIFIED: discussions.unity.com EventSystem threads].
- **`BuildPipeline.BuildPlayer` without `EditorApplication.Exit(code)`** — CI silently reports success on failed builds [VERIFIED: support.unity.com 211195263].
- **Tracking entire `Assets/` with Git LFS** — accidentally LFS-tracks `.meta` files and corrupts Unity asset relationships. Use targeted patterns by extension.
- **Forgetting to add the scene to Build Settings** — IL2CPP iOS build will produce an Xcode project that opens to a black screen because no scene is loaded at startup. Always include `Assets/_Project/Scenes/Warehouse_MVP.unity` in `EditorBuildSettings.scenes` (the BuildScript above passes it explicitly via `BuildPlayerOptions.scenes`, which bypasses the Build Settings list — but for editor Play and manual builds the list still matters).
- **Test asmdef without "Test Assemblies" toggle** — production build fails because production code can't see test framework refs.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Camera follow / framing | Custom `LateUpdate` lerp script | Cinemachine 3.x `CinemachineCamera` + `CinemachineBrain` | Cinemachine handles damping, screen composition, transitions, look-ahead, target groups out of the box. Phase 1 doesn't use those, but Phase 2+ will. |
| Touch input + UI input | Read `Input.touches` and write event dispatch | New Input System + `InputSystemUIInputModule` | Multi-touch, gamepad, keyboard, palm-rejection edge cases handled. iOS Safe Area gestures are respected. |
| Safe-area handling | Hard-code iPhone X cutout dimensions | `Screen.safeArea` + a `SafeAreaPanel` script | Apple changes safe-area shapes per device generation; `Screen.safeArea` returns runtime-correct rect. |
| Service location / DI | Singleton or service locator | Plain C# composition root in `Bootstrap` (D-07) | Zero magic, fully testable, no third-party package, matches CLAUDE.md guidance. |
| Test assembly skeleton | Hand-edit `.asmdef` JSON | `Window > General > Test Runner` "Create EditMode/PlayMode Test Assembly Folder" | Auto-adds correct `nunit.framework` / `UnityEngine.TestRunner` / `UnityEditor.TestRunner` references. |
| Build CLI exit codes | Trust Unity's process exit code | `EditorApplication.Exit(1)` on `summary.result != Succeeded` | Unity's default behavior reports success on a failed `BuildPlayer` call [VERIFIED: support.unity.com 211195263]. |
| `.gitignore` for Unity | Hand-write from scratch | Copy GitHub's official `Unity.gitignore` | Maintained, covers `Library/`, `Temp/`, `Logs/`, `Build/`, `*.csproj`, `*.sln`, IL2CPP/Addressables temp dirs. |

**Key insight:** Phase 1 is mostly Unity boilerplate that has well-known correct answers. The only places to be careful are (1) asmdef wiring (one-directional, test asmdefs flagged correctly), (2) iOS Player Settings completeness, and (3) BuildScript exit-code semantics for CI. Everything else is "use the package and follow the doc."

## Common Pitfalls

### Pitfall 1: BuildPlayer success-on-failure exit code
**What goes wrong:** CI pipeline reports green even though the iOS build crashed mid-IL2CPP.
**Why it happens:** `BuildPipeline.BuildPlayer` returns a `BuildReport` rather than throwing; Unity's `-batchmode -quit` exits 0 by default regardless of build result [VERIFIED: support.unity.com 211195263].
**How to avoid:** Always check `summary.result` and call `EditorApplication.Exit(1)` on non-Succeeded. See Pattern 6.
**Warning signs:** CI green, but `build/ios/` directory empty or missing the `Unity-iPhone.xcodeproj`.

### Pitfall 2: Cinemachine 3.x API breaking changes vs. tutorials
**What goes wrong:** Following a 2024 tutorial that uses `CinemachineVirtualCamera` produces "type or namespace not found" errors.
**Why it happens:** Cinemachine 3.x renames and restructures most components. Old tutorials still rank in search [VERIFIED: docs.unity3d.com Cinemachine 3.0 upgrade notes].
**How to avoid:** Use the namespace `Unity.Cinemachine` (not `Cinemachine`), the component `CinemachineCamera` (not `CinemachineVirtualCamera`), and procedural components via Add Component (not `Body`/`Aim` properties).
**Warning signs:** Compile errors mentioning `CinemachineVirtualCamera` / `Cinemachine.CinemachineBrain`.

### Pitfall 3: Cinemachine not pre-installed in Unity 6 templates
**What goes wrong:** Fresh URP 3D template has no Cinemachine package; opening a scene with a CinemachineCamera reference shows "missing script."
**Why it happens:** Cinemachine is not in the default Unity 6 manifest [VERIFIED: discussions.unity.com 1538713].
**How to avoid:** Add `"com.unity.cinemachine": "3.1.6"` to `Packages/manifest.json` (or install via Package Manager UI) **before** building the camera rig.
**Warning signs:** "The referenced script ... is missing" yellow warning on the vcam GameObject.

### Pitfall 4: Asmdef circular reference
**What goes wrong:** Compiler error "Cyclic dependency detected" — entire project fails to compile.
**Why it happens:** Two asmdefs reference each other (e.g., `WM.UI` references `WM.Player`, `WM.Player` references `WM.UI`).
**How to avoid:** Apply the dependency graph in Pattern 1. Anything that's truly cross-cutting (interfaces, common DTOs) lives in `WM.Core` and gets referenced by both consumers.
**Warning signs:** Console error "Cyclic assembly definition references."

### Pitfall 5: Test asmdef forgot "Test Assemblies" flag → production build fails
**What goes wrong:** `BuildScript.BuildIOS` fails with "The type or namespace name 'NUnit' could not be found."
**Why it happens:** Test asmdef referenced `nunit.framework` but didn't tick the "Test Assemblies" override, so Unity tries to compile it into the player [VERIFIED: docs.unity3d.com test-framework].
**How to avoid:** When creating EditMode/PlayMode test asmdefs, **always tick "Test Assemblies" in the asmdef Inspector** (or equivalently set `"defineConstraints": [], "optionalUnityReferences": ["TestAssemblies"]` in the asmdef JSON for older formats; in 2.0+ it's the "Test Assemblies" toggle that adds `UNITY_INCLUDE_TESTS` and excludes from non-test builds).
**Warning signs:** Player build fails on NUnit references; tests run fine in Editor but not from CLI.

### Pitfall 6: EventSystem missing → UI button taps do nothing
**What goes wrong:** App boots fine but UI is unresponsive on iOS device.
**Why it happens:** Either no EventSystem in scene, or the EventSystem still uses legacy `StandaloneInputModule` while Active Input Handling is "Input System Package (New)" only [VERIFIED: discussions.unity.com EventSystem threads].
**How to avoid:** Single EventSystem GameObject in scene, with `InputSystemUIInputModule` component. If you accidentally create a default EventSystem (which adds `StandaloneInputModule`), Unity shows an Inspector button "Replace with InputSystemUIInputModule" — click it.
**Warning signs:** Buttons visible but tap callbacks never fire; console may log warnings about missing input module.

### Pitfall 7: Active Input Handling = "Old (Legacy)" with new Input System installed
**What goes wrong:** New Input System code compiles but no input is received at runtime.
**Why it happens:** The `ENABLE_INPUT_SYSTEM` define is gated by Active Input Handling [VERIFIED: docs.unity3d.com Input System Migration].
**How to avoid:** Edit > Project Settings > Player > Other Settings > **Active Input Handling = "Input System Package (New)"** (D-03). Setting requires editor restart.
**Warning signs:** All Input.GetAxis returns 0 OR new InputAction callbacks never fire; console message about input handler mismatch on play.

### Pitfall 8: iOS build with default Bundle Identifier
**What goes wrong:** Xcode build fails: "The bundle identifier 'com.Company.ProductName' is not available."
**Why it happens:** Unity's default placeholder collides with App Store reservations.
**How to avoid:** Set Player Settings → Identification → Bundle Identifier to `com.warehousemaster.mvp` (D-17) before first build. Phase 11 finalizes the real ID.
**Warning signs:** Xcode signing error referencing the default `com.Company.ProductName`.

### Pitfall 9: Scene not in EditorBuildSettings → black screen on device
**What goes wrong:** Build runs, app launches, but only a clear color is visible.
**Why it happens:** No scene is in the build → Unity has nothing to load at startup.
**How to avoid:** Either (a) add `Warehouse_MVP.unity` to File > Build Profiles > Scene List, or (b) explicitly pass it in `BuildPlayerOptions.scenes` (Pattern 6 does this). Both is safest.
**Warning signs:** Logs show "No scenes in build settings" warning; app boots to clean Camera background.

### Pitfall 10: Render Graph Compatibility Mode flips on auto-upgrade and tanks frame rate
**What goes wrong:** New Unity 6 project unexpectedly runs in compatibility mode (slower path).
**Why it happens:** Compatibility Mode is enabled automatically when Unity 6 opens a project that was created in a URP version without RenderGraph [VERIFIED: docs.unity3d.com 6000.0 URP]. Brand-new Unity 6 projects start with RenderGraph on.
**How to avoid:** Project Settings > Graphics > Render Graph → ensure Compatibility Mode is **off** for a fresh Unity 6 project. Verify in URP asset.
**Warning signs:** "Render Graph compatibility mode" badge in Game view debugger; perf 30-50% slower than expected.

### Pitfall 11: Forward+ + 32 reflection probes Render Graph crash on Android (and some iOS edge cases)
**What goes wrong:** Editor and Game View stop rendering at 32 probes; 31 works fine [VERIFIED: discussions.unity.com 1713042].
**Why it happens:** Known Unity 6.3 bug.
**How to avoid:** Phase 1 has zero reflection probes — out of scope. Mentioning here so later phases don't add 32+ reflection probes blindly.

### Pitfall 12: IL2CPP build fails after Xcode major upgrade
**What goes wrong:** `Unity.IL2CPP.Bee.BuildLogic.ToolchainNotFoundException` or `Command PhaseScriptExecution failed` errors during Xcode build.
**Why it happens:** New Xcode versions occasionally break Unity's parsing of `version.plist` or change linker behavior [VERIFIED: issuetracker.unity3d.com IL2CPP Xcode 15+].
**How to avoid:** (a) Pin Unity 6.3.7f1 (which post-dates Xcode 15.x stability fixes); (b) use `xcode-select --switch /Applications/Xcode.app` if multiple Xcode installs exist; (c) keep Unity LTS and Xcode within one major version of each other.
**Warning signs:** IL2CPP-specific stack traces, "ld64" linker errors.

## Code Examples

### iOS Player Settings checklist (manual or scripted)

Manual: Edit > Project Settings > Player > iOS tab.

| Section | Field | Value |
|---------|-------|-------|
| Identification | Bundle Identifier | `com.warehousemaster.mvp` |
| Identification | Version | `0.1.0` |
| Identification | Build | `1` |
| Identification | Signing Team ID | (leave blank Phase 1; set Phase 11) |
| Configuration | Scripting Backend | **IL2CPP** |
| Configuration | Api Compatibility Level | .NET Standard 2.1 |
| Configuration | Architecture | **ARM64** |
| Configuration | Target SDK | Device SDK |
| Configuration | Target minimum iOS Version | **15.0** |
| Configuration | Active Input Handling | **Input System Package (New)** |
| Resolution and Presentation | Default Orientation | **Portrait** |
| Resolution and Presentation | Allowed Orientations for Auto Rotation | only Portrait checked (auto-rotate disabled is also fine) |
| Other Settings | Auto Graphics API | Off |
| Other Settings | Graphics APIs | **Metal only** |
| Other Settings | Color Space | Linear |

[VERIFIED: docs.unity3d.com 6000.3 PlayerSettingsiOS]

Scripted equivalents (for reference, not required in Phase 1):
```csharp
PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1); // ARM64
PlayerSettings.iOS.targetOSVersionString = "15.0";
PlayerSettings.applicationIdentifier = "com.warehousemaster.mvp";
PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
PlayerSettings.allowedAutorotateToPortrait = true;
PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
PlayerSettings.allowedAutorotateToLandscapeLeft = false;
PlayerSettings.allowedAutorotateToLandscapeRight = false;
```

### URP Mobile-Aggressive Asset settings

Edit `Assets/_Project/Settings/URP_Mobile.asset`:

| Section | Field | Value | Source |
|---------|-------|-------|--------|
| General | Depth Texture | Off | [VERIFIED: docs configure-for-better-performance] |
| General | Opaque Texture | Off | same |
| Quality | Anti Aliasing (MSAA) | Disabled | same |
| Quality | HDR | Off | same |
| Quality | Render Scale | 1.0 (drop to 0.85 if perf testing demands) | recommended baseline |
| Lighting | Main Light | Per Pixel | default |
| Lighting | Main Light > Cast Shadows | Off (baked only — D-02) | D-02 |
| Lighting | Additional Lights | **Disabled** or Per Vertex | [VERIFIED: docs] |
| Lighting | Additional Lights > Cast Shadows | Off | same |
| Shadows | Soft Shadows | Off | same |
| Post-processing | Enabled | **Off** (D-02) | same |
| Advanced | SRP Batcher | On | same |
| Advanced | Dynamic Batching | On (off if SRP Batcher conflicts) | same |
| Advanced | Depth Priming Mode | Disabled | same |
| Advanced | LOD Cross Fade | Disabled | same |

Renderer asset (`URP_Mobile_Renderer.asset`):
- Renderer Features: empty list (no SSAO, no decals, no screen-space shadows in Phase 1).
- Rendering Path: **Forward** (avoid Forward+ on mobile-aggressive — Forward+ has the 32-probe Render Graph bug, and aggressive baseline doesn't need clustering).

### EditMode smoke test (asserts required GameObjects)

```csharp
// Source: docs.unity3d.com 6000.2 test-framework scene-based-tests
// Assets/Tests/EditMode/BootstrapSmokeTests.cs (asmdef WM.Tests.EditMode)
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            Assert.That(go, Is.Not.Null, $"Required GameObject '{name}' missing from {ScenePath}");
        }

        [TearDown]
        public void Cleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
    }
}
```

### PlayMode smoke test (asserts scene loads, GameManager logs)

```csharp
// Assets/Tests/PlayMode/PlayModeSmokeTests.cs (asmdef WM.Tests.PlayMode)
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WM.Tests.PlayMode
{
    public class PlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator Scene_Loads_GameManagerInitializes()
        {
            // Scene must be in Build Settings for SceneManager.LoadScene to find it at play time.
            yield return SceneManager.LoadSceneAsync("Warehouse_MVP", LoadSceneMode.Single);
            yield return null; // wait one frame for Awake/Start

            var gm = GameObject.Find("GameManager");
            Assert.That(gm, Is.Not.Null);
            Assert.That(gm.GetComponent<MonoBehaviour>(), Is.Not.Null);
        }
    }
}
```

> Asserting on the "GameManager initialized" Debug.Log requires `LogAssert.Expect(LogType.Log, "GameManager initialized")` before the play step.

### .gitignore (Unity-flavored)

Use the official GitHub Unity .gitignore [VERIFIED: github.com/github/gitignore Unity.gitignore]. Key entries:

```gitignore
.utmp/
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
*.log
/[Mm]emoryCaptures/
/[Rr]ecordings/
/[Aa]ssets/Plugins/Editor/JetBrains*
*.DotSettings.user
.vs/
.gradle/
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
sysinfo.txt
mono_crash.*
*.apk
*.aab
*.unitypackage
*.unitypackage.meta
*.app
crashlytics-build.properties
InitTestScene*.unity*
/ServerData
/[Aa]ssets/StreamingAssets/aa*
/[Aa]ssets/AddressableAssetsData/link.xml*
```

Plus repo-specific (Phase 1):
```gitignore
# iOS Xcode build artifacts (BuildScript output)
/build/
/build/ios/
```

### .gitattributes (Git LFS for binaries)

[VERIFIED: gist.github.com/nemotoo Unity .gitattributes; community pattern]

```gitattributes
## Unity text assets — keep diffable, force LF, use Unity's YAML merge
*.cs       text diff=csharp eol=lf
*.cginc    text eol=lf
*.shader   text eol=lf
*.unity    merge=unityyamlmerge eol=lf
*.prefab   merge=unityyamlmerge eol=lf
*.mat      merge=unityyamlmerge eol=lf
*.anim     merge=unityyamlmerge eol=lf
*.controller merge=unityyamlmerge eol=lf
*.physicMaterial merge=unityyamlmerge eol=lf
*.physicsMaterial2D merge=unityyamlmerge eol=lf
*.meta     merge=unityyamlmerge eol=lf

## Binary .asset files (lighting, terrain, navmesh) — LFS, NOT yaml-merged
LightingData.asset filter=lfs diff=lfs merge=lfs -text
*TerrainData.asset filter=lfs diff=lfs merge=lfs -text
NavMeshData.asset  filter=lfs diff=lfs merge=lfs -text

## Images
*.png  filter=lfs diff=lfs merge=lfs -text
*.jpg  filter=lfs diff=lfs merge=lfs -text
*.jpeg filter=lfs diff=lfs merge=lfs -text
*.gif  filter=lfs diff=lfs merge=lfs -text
*.psd  filter=lfs diff=lfs merge=lfs -text
*.tga  filter=lfs diff=lfs merge=lfs -text
*.exr  filter=lfs diff=lfs merge=lfs -text
*.tif  filter=lfs diff=lfs merge=lfs -text
*.tiff filter=lfs diff=lfs merge=lfs -text

## Audio
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text
*.aif filter=lfs diff=lfs merge=lfs -text

## Video
*.mp4 filter=lfs diff=lfs merge=lfs -text
*.mov filter=lfs diff=lfs merge=lfs -text

## 3D
*.fbx  filter=lfs diff=lfs merge=lfs -text
*.FBX  filter=lfs diff=lfs merge=lfs -text
*.blend filter=lfs diff=lfs merge=lfs -text
*.obj  filter=lfs diff=lfs merge=lfs -text
*.dae  filter=lfs diff=lfs merge=lfs -text

## Misc binary
*.unitypackage filter=lfs diff=lfs merge=lfs -text
*.ttf  filter=lfs diff=lfs merge=lfs -text
*.otf  filter=lfs diff=lfs merge=lfs -text
```

> **Important:** CONTEXT.md D-20 mentions `.unity`, `.asset`, `.prefab` as LFS targets. Best practice diverges from that for `.unity` and `.prefab`: those are **YAML text** files and should be merge=unityyamlmerge, not LFS. Putting them in LFS breaks Unity's `unityyamlmerge` tool and prevents diffs in PRs. Recommend the planner override D-20 to: LFS the binary `.asset` subtypes (LightingData, TerrainData, NavMeshData) and binary media files; keep `.unity`, `.prefab`, generic `.asset` as text+yaml-merge. **This is a deviation from D-20** — flag for user confirmation in plan-checker step.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Built-in Render Pipeline | Universal Render Pipeline (URP) with RenderGraph | Unity 6 (2024) — RenderGraph default-on for new projects | Better mobile perf; old camera stack code (`OnRenderImage`) deprecated |
| Cinemachine 2.x `CinemachineVirtualCamera` | Cinemachine 3.x `CinemachineCamera` + procedural components | Cinemachine 3.0 (Unity 2022.3+, fully on Unity 6) | Tutorials pre-2024 use deprecated API |
| Legacy Input Manager | New Input System (`com.unity.inputsystem`) + InputSystemUIInputModule | Standard since Unity 2022; Unity 6 strongly recommends new only | Different API, scriptable input actions |
| ARMv7 + ARM64 universal iOS builds | ARM64-only | iOS 11+ era; Apple App Store rejects ARMv7-only | Player Settings Architecture = ARM64 |
| OpenGL ES on iOS | Metal-only | Apple deprecated OpenGL ES; Unity 6.x supports Metal only on iOS | Player Settings Graphics APIs = Metal |

**Deprecated/outdated:**
- `CinemachineVirtualCamera` (Cinemachine 2.x): replaced by `CinemachineCamera`.
- `StandaloneInputModule`: replaced by `InputSystemUIInputModule` for new projects.
- `BuildPipeline.BuildPlayer(string[] scenes, string locationPathName, BuildTarget target, BuildOptions options)` overload (string-based): superseded by `BuildPlayerOptions` struct overload.
- `Application.LoadLevel`: replaced by `SceneManager.LoadScene` years ago.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | Unity Test Framework 2.0.x (NUnit + UnityEngine.TestRunner) — ships with Unity 6 |
| Config file | `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef`, `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` (both must be created in Wave 0) |
| Quick run command | `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` |
| Full suite command | `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` followed by the same with `-testPlatform PlayMode` |

> Replace `<UNITY>` with the Unity Editor binary path matching `ProjectSettings/ProjectVersion.txt` (`6000.3.7f1`). On macOS that's typically `/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity`.

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| BOOT-01 | Project initialized for iOS with URP and `_Project/` folder structure | unit (EditMode) | EditMode test that asserts presence of `Assets/_Project/Scripts/Core/`, asmdef files exist, URP asset present in `Settings/` | ❌ Wave 0 — `BootstrapStructureTests.cs` |
| BOOT-01 | iOS build target compiles | smoke (CLI) | `Unity -batchmode -quit -buildTarget iOS -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -` (verifies BuildScript exit code) | ❌ Wave 0 — manual smoke for Phase 1; can be a doc step in `01-VERIFICATION.md` |
| BOOT-02 | `Warehouse_MVP` scene contains all required GameObjects | unit (EditMode) | `BootstrapSmokeTests.RequiredGameObject_IsPresent` parameterized over 12 names | ❌ Wave 0 — `BootstrapSmokeTests.cs` (Code Examples §) |
| BOOT-02 | Scene loads at runtime, GameManager initializes | smoke (PlayMode) | `PlayModeSmokeTests.Scene_Loads_GameManagerInitializes` | ❌ Wave 0 — `PlayModeSmokeTests.cs` |

### Sampling Rate

- **Per task commit:** `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` (≤ 30s on a small project).
- **Per wave merge:** EditMode + PlayMode full sweep.
- **Phase gate:** EditMode + PlayMode green AND a successful manual `BuildScript.BuildIOS` invocation that produces `build/ios/Unity-iPhone.xcodeproj`.

### Wave 0 Gaps

- [ ] `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef` — Editor platform only; Test Assemblies toggle on; references `WM.Core`, `nunit.framework`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`.
- [ ] `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` — Any Platform; Test Assemblies toggle on; references `WM.Core`, `nunit.framework`, `UnityEngine.TestRunner`. **Must NOT reference `UnityEditor.TestRunner`** (PlayMode runs without editor).
- [ ] `Assets/Tests/EditMode/BootstrapSmokeTests.cs` — covers BOOT-02 (required GameObjects).
- [ ] `Assets/Tests/EditMode/BootstrapStructureTests.cs` — covers BOOT-01 (folder + asmdef presence). Uses `AssetDatabase.IsValidFolder` and `File.Exists`.
- [ ] `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` — covers BOOT-02 (runtime load).
- [ ] Framework install: Unity Test Framework ships with Unity 6 by default, so no `npm install` equivalent — verify `com.unity.test-framework` in `Packages/manifest.json` after Hub creates the project.

## Security Domain

### Applicable ASVS Categories

Phase 1 has no runtime authentication, no data persistence, no network, no cryptography, and no input validation surface beyond Unity's framework defaults. The phase is pure scaffolding.

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | no | (none — no auth in MVP) |
| V3 Session Management | no | (none) |
| V4 Access Control | no | (single-user local app) |
| V5 Input Validation | no | (no inputs accepted in Phase 1; new Input System is the future surface) |
| V6 Cryptography | no | (no crypto; never roll own when added — use Unity's `System.Security.Cryptography` standard libs) |
| V7 Error Handling | yes (basic) | `EditorApplication.Exit(1)` on build failure; Debug.LogError for misconfiguration; no PII to leak |
| V14 Configuration | yes (low) | Bundle Identifier placeholder set to non-default (`com.warehousemaster.mvp`) — final ID is privacy-relevant only at TestFlight in Phase 11 |

### Known Threat Patterns for Unity iOS bootstrap

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Default Bundle Identifier collision | Information Disclosure / Denial of Service (build) | Set `PlayerSettings.applicationIdentifier` to a project-specific value before any signed build |
| Committed signing certs / .p12 / .mobileprovision | Spoofing | `.gitignore` excludes `*.mobileprovision`, `*.p12`, `*.cer`; signing happens at TestFlight time only (Phase 11) |
| LFS-tracked `.meta` files corrupting Unity asset graph | Tampering (asset integrity) | `.gitattributes` keeps `.meta` as `merge=unityyamlmerge eol=lf`, NOT in LFS |
| Build artifacts (`build/ios/`) committed | Information Disclosure | `.gitignore` excludes `build/` |

## Sources

### Primary (HIGH confidence)
- [Unity 6.3 LTS release page](https://unity.com/blog/unity-6-3-lts-is-now-available) — confirms Unity 6.3 LTS available, recommended for production iOS in 2026
- [Unity 6000.3.7f1 release notes](https://unity.com/releases/editor/whats-new/6000.3.7f1) — latest 6.3 LTS sub-version (Feb 5 2026)
- [Unity Manual: iOS Player Settings (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/class-PlayerSettingsiOS.html) — IL2CPP / ARM64 / Metal / iOS 15 / portrait settings
- [Unity Manual: Configure for better performance in URP (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/configure-for-better-performance.html) — mobile-aggressive URP asset settings
- [Unity Scripting API: BuildPipeline.BuildPlayer](https://docs.unity3d.com/ScriptReference/BuildPipeline.BuildPlayer.html) — current API + BuildReport pattern
- [Unity Support: BuildPipeline.BuildPlayer exit code](https://support.unity.com/hc/en-us/articles/211195263-Why-doesn-t-a-failed-BuildPipeline-BuildPlayer-return-an-error-code-in-the-command-line) — `EditorApplication.Exit(1)` necessity
- [Cinemachine 3.1 Installation and Upgrade](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/InstallationAndUpgrade.html) — version 3.1.6, Unity 2022.3+ supported
- [CinemachineCamera (3.0)](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.0/manual/CinemachineCamera.html) — modern API, passive vcam pattern
- [Unity Manual: Render Graph (6000.0)](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/render-graph.html) — Compatibility Mode flip behavior
- [Unity Manual: Test Framework Create Test Assembly (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/test-framework/workflow-create-test-assembly.html) — test asmdef references
- [GitHub Unity.gitignore](https://github.com/github/gitignore/blob/main/Unity.gitignore) — canonical ignore list
- [Unity Scripting API: Screen.safeArea](https://docs.unity3d.com/ScriptReference/Screen-safeArea.html) — safe area runtime API
- [Unity Manual: Script compilation and assembly definition files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) — cyclic dep prohibition

### Secondary (MEDIUM confidence)
- [Input System 1.15.0 Release thread](https://discussions.unity.com/t/release-input-system-1-15-0/1688570) — version + Unity 6.3 availability (Oct 2025 forum post)
- [Cinemachine 3.1.6 release context](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/) — package version for Unity 6.3
- [Migrating from old Input Manager](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/manual/Migration.html) — `ENABLE_INPUT_SYSTEM` define
- [Unity .gitattributes gist (nemotoo)](https://gist.github.com/nemotoo/b8a1c3a0f1225bb9231979f389fd4f3f) — community-blessed LFS rules
- [Scene-based tests (test-framework 1.4)](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/course/scene-based-tests.html) — EditorSceneManager.OpenScene pattern in tests
- [Cinemachine install errors thread](https://discussions.unity.com/t/cant-install-cinemachine-in-unity-6-project/1538713) — confirms Cinemachine not pre-installed in Unity 6 templates

### Tertiary (LOW confidence — flagged for verification when implementing)
- The exact iOS 15.0 minimum being the Unity 6.3 LTS default (vs. iOS 13/14) — Unity docs say "iOS 15 and above" is supported, but the in-Inspector default minimum may be lower. Verify in Inspector after init; D-17 locks 15.0 anyway.
- Whether `WM.Editor` should reference `WM.Core` at Phase 1. Not strictly needed since BuildScript only does BuildPipeline calls. Add the reference only if BuildScript ever needs to introspect production code.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | The user's mac has Unity Hub installed and Unity 6000.3.7f1 (or close 6.3 LTS sub-version) installable. | Standard Stack | Phase blocked until install; planner should add a Wave 0 task to confirm Unity Hub + 6.3 LTS install. [ASSUMED] |
| A2 | Xcode 15.x or 16.x is installed for iOS builds; `xcode-select -p` points at it. | Build pipeline | iOS build will fail; Phase 1's "build runs on simulator" success criterion blocked. Planner should add a precheck. [ASSUMED] |
| A3 | The recommended deviation from D-20 (don't LFS `.unity` / generic `.asset` / `.prefab`) is acceptable to user. | .gitattributes | If user insists on D-20 verbatim, Unity YAML merge breaks; PR diffs become opaque. **Planner should surface this as a question for plan-checker.** [ASSUMED] |
| A4 | The user wants both EditMode and PlayMode test asmdefs even though BOOT requirements don't mandate tests. | Validation Architecture | If user wants to skip PlayMode for Phase 1, drop `WM.Tests.PlayMode` and the PlayMode smoke test. D-06 explicitly says both, so risk is low. [VERIFIED via D-06] |
| A5 | Cinemachine 3.1.6 is the version Unity 6.3.7f1 will offer in Package Manager. | Cinemachine version | If 6.3.7f1 ships with a slightly different version (e.g., 3.1.4 or 3.2.x), accept whatever the manifest auto-pins. [ASSUMED] |
| A6 | The `.planning/config.json` `nyquist_validation: true` means a full Validation Architecture section is required. | Validation Architecture | Confirmed via config.json read. [VERIFIED] |

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Unity Hub | Project creation | unverified (planner / executor must check) | should be 3.x | Install via https://unity.com/download |
| Unity Editor 6000.3.7f1 | All editor work | unverified | 6.3 LTS | Any 6000.3.x, then upgrade. **Do not** use 2022.3 LTS (locked out by D-01). |
| Xcode | iOS build | unverified | 15.x or 16.x | Install from Mac App Store; `xcode-select --switch /Applications/Xcode.app` |
| Xcode CommandLineTools | IL2CPP toolchain | unverified | matching Xcode version | `xcode-select --install` |
| iOS Simulator runtime | Debug build run | unverified | iOS 15+ runtime | Open Xcode > Settings > Platforms |
| Git LFS | `.gitattributes` LFS tracking | unverified | 3.x | `brew install git-lfs && git lfs install` (per-repo: `git lfs install --local`) |
| `git` | Repo state | ✓ (we're in a git repo per env) | — | — |

**Missing dependencies with no fallback:** None definitive — all are install-on-demand on macOS. Planner should add a Wave 0 verification task to run `command -v unity-hub`, `xcodebuild -version`, `git lfs version`.

**Missing dependencies with fallback:** None.

## Open Questions (RESOLVED)

1. **[RESOLVED 2026-05-10] D-20 conflict: `.unity`/`.prefab`/`.asset` in LFS or yaml-merged?**
   - **Resolution:** User approved the deviation from D-20 verbatim. `.unity`, `.prefab`, generic `.asset` stay as YAML text with `merge=unityyamlmerge`. LFS tracks only true binaries (`.psd`, `.fbx`, `.png`, `.jpg`, `.wav`, `.mp3`, `.ogg`) plus three special assets (`LightingData.asset`, `*TerrainData.asset`, `NavMeshData.asset`).
   - **Rationale:** LFS-tracked YAML breaks `unityyamlmerge` and removes PR diffs. Best practice across Unity community + official tooling expects YAML text + merge driver for scenes/prefabs.
   - **Effect on D-20:** Plan 01-01 `.gitattributes` content reflects this resolution. CONTEXT.md D-20 is amended in spirit (binaries-only) without rewriting the locked-decision text — deviation documented in 01-SKELETON.md and 01-01 plan rationale.

2. **Should `BuildScript` exist as `WM.Editor.BuildScript` or namespaceless `BuildScript`?**
   - What we know: CLAUDE.md examples write `BuildScript.BuildIOS` without namespace.
   - What's unclear: Whether downstream `-executeMethod` arg is `WM.Editor.BuildScript.BuildIOS` or `BuildScript.BuildIOS`.
   - Recommendation: Use `WM.Editor.BuildScript.BuildIOS` (namespaced) — matches asmdef-per-folder discipline and avoids global namespace pollution. The `-executeMethod` flag accepts fully-qualified names, so this is fine.

3. **Phase 1 test for "build produces an Xcode project" — is a PlayMode/EditMode test the right shape?**
   - What we know: Tests run inside the editor; they can't easily verify a CLI-invoked iOS build's output.
   - What's unclear: Whether to include this as a manual verification step in `01-VERIFICATION.md` or a CI-time check.
   - Recommendation: Treat as a manual verification step for Phase 1 (executor runs the CLI command and asserts the output dir contains `Unity-iPhone.xcodeproj`). Phase 11 will make it CI-grade.

## Metadata

**Confidence breakdown:**
- Standard stack (Unity 6.3.7f1, URP 17, Input System 1.15.0, Cinemachine 3.1.6, Test Framework 2.0): **HIGH** — all verified against Unity-published docs and recent release threads.
- Architecture patterns (asmdef wiring, Bootstrap, Cinemachine setup, Canvas+SafeArea, EventSystem, BuildScript): **HIGH** — sourced from official docs and CONTEXT.md decisions.
- iOS Player Settings checklist: **HIGH** — verified against `docs.unity3d.com/6000.3/Documentation/Manual/class-PlayerSettingsiOS.html`.
- Pitfalls: **HIGH** — most reference Unity issue tracker / official support articles.
- `.gitattributes` divergence from D-20 verbatim: **MEDIUM** — community best practice contradicts D-20; flagged as Open Question 1.
- Cinemachine 3.1.6 being the exact pinned version Unity 6.3.7f1 ships: **MEDIUM** — version is correct in Package Manager but exact pin is editor-dependent (assumption A5).
- Active Input Handling enum mapping ("Input System Package (New)" = `ENABLE_INPUT_SYSTEM` only): **HIGH**.
- Validation Architecture (Wave 0 test files): **HIGH** — patterns verified against test-framework docs; specific test file names are conventions.

**Research date:** 2026-05-10
**Valid until:** 2026-06-10 (30 days — Unity 6.3 LTS and the listed packages are stable; only update if Unity ships a new sub-version that breaks IL2CPP iOS builds).

---

*Phase: 1-project-bootstrap-empty-warehouse-scene*
*Research completed: 2026-05-10*
