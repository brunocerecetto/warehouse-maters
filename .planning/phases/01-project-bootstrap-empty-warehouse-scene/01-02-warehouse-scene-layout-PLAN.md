---
phase: 01-project-bootstrap-empty-warehouse-scene
plan: 02
type: execute
wave: 2
depends_on:
  - 01
files_modified:
  - Assets/_Project/Scenes/Warehouse_MVP.unity
  - Assets/_Project/Materials/Placeholder/Mat_Floor.mat
  - Assets/_Project/Materials/Placeholder/Mat_LoadingDock.mat
  - Assets/_Project/Materials/Placeholder/Mat_PackingStation.mat
  - Assets/_Project/Materials/Placeholder/Mat_DeliveryZone.mat
  - Assets/_Project/Materials/Placeholder/Mat_UpgradeStation.mat
  - Assets/_Project/Materials/Placeholder/Mat_Shelf.mat
  - Assets/_Project/Materials/Placeholder/Mat_WorkerSpawn.mat
  - Assets/_Project/Scripts/Core/Bootstrap.cs
  - Assets/_Project/Scripts/Core/GameManager.cs
  - ProjectSettings/EditorBuildSettings.asset
  - Assets/Tests/EditMode/BootstrapSmokeTests.cs
  - Assets/Tests/PlayMode/PlayModeSmokeTests.cs
autonomous: true
requirements:
  - BOOT-02

must_haves:
  truths:
    - "Opening Assets/_Project/Scenes/Warehouse_MVP.unity in the Editor loads a scene with all 12 required GameObjects positioned and visible from the camera"
    - "Each placeholder station is visually distinguishable in-Editor via its colored URP/Lit material (loading dock=grey, packing=blue, delivery=green, upgrade=yellow, shelf=brown, worker spawn=magenta marker)"
    - "Pressing Play in the Editor on Warehouse_MVP scene logs 'GameManager initialized' to the Console once, with no NullReferenceException"
    - "Bootstrap.Awake runs before GameManager.Start (DefaultExecutionOrder enforces this)"
    - "Bootstrap news up NullAnalyticsService, NullAdService, NullIapService, NullSaveService and injects them into GameManager via Init(...) — verifiable by GameManager fields being non-null after Awake"
    - "Warehouse_MVP scene is registered in EditorBuildSettings so SceneManager.LoadScene resolves it at runtime"
  artifacts:
    - path: "Assets/_Project/Scenes/Warehouse_MVP.unity"
      provides: "The single MVP scene with all required GameObjects"
      contains: "Player", "LoadingDock", "PackingStation", "DeliveryZone", "UpgradeStation", "ShelfArea", "WorkerSpawn", "CameraRig", "UICanvas", "EventSystem", "GameManager", "Bootstrap"
    - path: "Assets/_Project/Materials/Placeholder/Mat_Floor.mat"
      provides: "Floor material (URP/Lit, neutral color)"
    - path: "Assets/_Project/Materials/Placeholder/Mat_LoadingDock.mat"
      provides: "Loading dock placeholder material (grey)"
    - path: "Assets/_Project/Materials/Placeholder/Mat_PackingStation.mat"
      provides: "Packing station placeholder material (blue)"
    - path: "Assets/_Project/Materials/Placeholder/Mat_DeliveryZone.mat"
      provides: "Delivery zone placeholder material (green)"
    - path: "Assets/_Project/Materials/Placeholder/Mat_UpgradeStation.mat"
      provides: "Upgrade station placeholder material (yellow)"
    - path: "Assets/_Project/Materials/Placeholder/Mat_Shelf.mat"
      provides: "Shelf placeholder material (brown)"
    - path: "Assets/_Project/Materials/Placeholder/Mat_WorkerSpawn.mat"
      provides: "Worker spawn marker material (magenta)"
    - path: "Assets/_Project/Scripts/Core/Bootstrap.cs"
      provides: "Composition root MonoBehaviour"
      contains: "namespace WM.Core", "[DefaultExecutionOrder(-100)]", "new NullAnalyticsService()", "new NullAdService()", "new NullIapService()", "new NullSaveService()", "gameManager.Init"
    - path: "Assets/_Project/Scripts/Core/GameManager.cs"
      provides: "Thin scene orchestrator that logs init"
      contains: "namespace WM.Core", "public void Init", "Debug.Log(\"GameManager initialized\")"
  key_links:
    - from: "Bootstrap (scene GameObject)"
      to: "GameManager (scene GameObject)"
      via: "[SerializeField] private GameManager gameManager + Inspector reference"
      pattern: "\\[SerializeField\\][^;]*GameManager"
    - from: "Bootstrap.Awake"
      to: "NullAnalyticsService / NullAdService / NullIapService / NullSaveService"
      via: "new T() construction + gameManager.Init(...)"
      pattern: "new Null(Analytics|Ad|Iap|Save)Service\\(\\)"
    - from: "Warehouse_MVP scene"
      to: "EditorBuildSettings.scenes"
      via: "Build Profiles scene list"
      pattern: "Warehouse_MVP\\.unity"
---

<objective>
Stand up the `Warehouse_MVP` scene with all 12 required placeholder GameObjects (`Player`, `LoadingDock`, `PackingStation`, `DeliveryZone`, `UpgradeStation`, `ShelfArea`, `WorkerSpawn`, `CameraRig`, `UICanvas`, `EventSystem`, `GameManager`, `Bootstrap`) positioned with primitives + colored URP/Lit materials, plus the runtime composition root: `Bootstrap` MonoBehaviour news up the four `Null*Service` stubs from plan 01-01 and injects them into `GameManager` via an explicit `Init(...)` call. `GameManager.Start` logs "GameManager initialized" to verify the wiring.

Purpose: This is the scene-level slice of the walking skeleton. After this plan, pressing Play in the Editor proves the runtime topology (Bootstrap → GameManager → service stubs) works end-to-end without throwing. It also unblocks the iOS build smoke test in plan 01-01's BuildScript (the BuildScript references this scene path).

Output: A loadable, runnable scene whose runtime smoke test (`PlayModeSmokeTests.Scene_Loads_GameManagerInitializes`) passes via Unity Test Framework CLI.
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
@CLAUDE.md
@specs/06-technical-architecture-spec.md

<interfaces>
<!-- Service interfaces & stubs created in plan 01-01 (already in WM.Core). -->
<!-- This plan CONSUMES them via Bootstrap; do not redeclare. -->

namespace WM.Core
{
    public interface IAnalyticsService { void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null); }
    public interface IAdService        { void ShowRewarded(System.Action<bool> onComplete); }
    public interface IIapService       { void Purchase(string productId, System.Action<bool> onComplete); }
    public interface ISaveService      { void Save(string key, string json); string Load(string key); bool HasKey(string key); }

    public sealed class NullAnalyticsService : IAnalyticsService { /* no-op + Debug.Log */ }
    public sealed class NullAdService        : IAdService        { /* no-op + Debug.Log */ }
    public sealed class NullIapService       : IIapService       { /* no-op + Debug.Log */ }
    public sealed class NullSaveService      : ISaveService      { /* in-memory dict */ }
}

<!-- New types this plan introduces, also in WM.Core: -->
namespace WM.Core
{
    [DefaultExecutionOrder(-100)]
    public sealed class Bootstrap : MonoBehaviour { /* news up stubs, calls gameManager.Init(...) */ }

    public sealed class GameManager : MonoBehaviour
    {
        public void Init(IAnalyticsService a, IAdService ad, IIapService iap, ISaveService save);
        // Start logs "GameManager initialized"
    }
}
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Author Bootstrap.cs and GameManager.cs in WM.Core</name>
  <files>
    Assets/_Project/Scripts/Core/Bootstrap.cs,
    Assets/_Project/Scripts/Core/GameManager.cs
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md (D-07, D-08, D-09)
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §Pattern 2 (Bootstrap Composition Root)
    - specs/06-technical-architecture-spec.md §4 `GameManager` (responsibilities)
    - 01-01-init-unity-project-PLAN.md `<interfaces>` block (existing service interfaces and Null* stubs)
  </read_first>
  <behavior>
    - Test 1 (compile): both files compile under namespace WM.Core in the WM.Core asmdef. Editor Console green.
    - Test 2 (runtime, covered by Task 4 PlayMode test): when scene plays, Bootstrap.Awake runs first (DefaultExecutionOrder = -100), creates the four Null* services, calls gameManager.Init(...), and GameManager.Start logs "GameManager initialized" exactly once.
    - Test 3 (manual via Editor Inspector): Bootstrap MonoBehaviour exposes a `[SerializeField]` GameManager field that can be wired in the Inspector to the GameManager GameObject in Task 3.
  </behavior>
  <action>
**Step A — Write `Assets/_Project/Scripts/Core/Bootstrap.cs`** (RESEARCH Pattern 2; D-07 plain-C# composition root). Use this exact source:

```csharp
// Source: D-07 + RESEARCH §Pattern 2 (Bootstrap Composition Root) + spec 06 §4
// Plain-C# composition root. No singletons. No DI container. No SO service locator.
using UnityEngine;

namespace WM.Core
{
    /// <summary>
    /// Scene-level composition root. Runs before all other MonoBehaviours
    /// (DefaultExecutionOrder = -100), constructs plain-C# service implementations,
    /// and injects them into GameManager via an explicit Init(...) call.
    /// In Phase 1 all services are Null*Service stubs; real implementations land
    /// in their owning phases (Save = Phase 6, Analytics = Phase 9, Ads = Phase 10).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class Bootstrap : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            if (gameManager == null)
            {
                Debug.LogError("[Bootstrap] GameManager reference is missing. Wire it in the Inspector.");
                return;
            }

            IAnalyticsService analytics = new NullAnalyticsService();
            IAdService        ads       = new NullAdService();
            IIapService       iap       = new NullIapService();
            ISaveService      save      = new NullSaveService();

            gameManager.Init(analytics, ads, iap, save);
        }
    }
}
```

**Step B — Write `Assets/_Project/Scripts/Core/GameManager.cs`** (D-08; spec 06 §4 — Phase 1 thin orchestrator). Use this exact source:

```csharp
// Source: D-08 + spec 06 §4 (GameManager: "Initialize services")
// Phase 1 = thin orchestrator: receives services, logs init.
// Later phases will add: load save data, start tutorial or normal gameplay,
// coordinate game state transitions.
using UnityEngine;

namespace WM.Core
{
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

        private void Start()
        {
            Debug.Log("GameManager initialized");
        }
    }
}
```

> **Why no auto-find or `GetComponent` fallback in `Bootstrap`?** D-07 explicitly forbids singletons and global state. The `[SerializeField]` reference is the only allowed wiring. Task 3 wires it in the Inspector. If the user accidentally clears the reference, the early Debug.LogError makes the regression loud rather than silent (NullReferenceException would be a worse signal in CI logs).

**Step C — Save and let Editor recompile.** Console must remain at 0 errors.
  </action>
  <verify>
    <automated>
test -f Assets/_Project/Scripts/Core/Bootstrap.cs
test -f Assets/_Project/Scripts/Core/GameManager.cs

# Bootstrap signature requirements
grep -q 'namespace WM.Core' Assets/_Project/Scripts/Core/Bootstrap.cs
grep -q '\[DefaultExecutionOrder(-100)\]' Assets/_Project/Scripts/Core/Bootstrap.cs
grep -q '\[SerializeField\] private GameManager gameManager' Assets/_Project/Scripts/Core/Bootstrap.cs
grep -q 'new NullAnalyticsService()' Assets/_Project/Scripts/Core/Bootstrap.cs
grep -q 'new NullAdService()' Assets/_Project/Scripts/Core/Bootstrap.cs
grep -q 'new NullIapService()' Assets/_Project/Scripts/Core/Bootstrap.cs
grep -q 'new NullSaveService()' Assets/_Project/Scripts/Core/Bootstrap.cs
grep -q 'gameManager.Init(' Assets/_Project/Scripts/Core/Bootstrap.cs

# GameManager signature requirements
grep -q 'namespace WM.Core' Assets/_Project/Scripts/Core/GameManager.cs
grep -qE 'public void Init\([^)]*IAnalyticsService' Assets/_Project/Scripts/Core/GameManager.cs
grep -q 'Debug.Log("GameManager initialized")' Assets/_Project/Scripts/Core/GameManager.cs
    </automated>
  </verify>
  <done>
    Both grep gates pass. Editor Console shows 0 errors. The two new MonoBehaviours are addable to GameObjects via "Add Component" search ("Bootstrap" / "GameManager").
  </done>
  <acceptance_criteria>
    - File `Assets/_Project/Scripts/Core/Bootstrap.cs` declares `namespace WM.Core`, has the `[DefaultExecutionOrder(-100)]` attribute, has `[SerializeField] private GameManager gameManager`, and constructs all four `Null*Service` instances inside `Awake` before calling `gameManager.Init(...)`.
    - File `Assets/_Project/Scripts/Core/GameManager.cs` declares `namespace WM.Core` and has a public `Init` method accepting all four service interfaces. `Start` calls `Debug.Log("GameManager initialized")`.
    - Editor Console has 0 compile errors.
  </acceptance_criteria>
</task>

<task type="auto" tdd="true">
  <name>Task 2: Create the seven placeholder URP/Lit materials</name>
  <files>
    Assets/_Project/Materials/Placeholder/Mat_Floor.mat,
    Assets/_Project/Materials/Placeholder/Mat_LoadingDock.mat,
    Assets/_Project/Materials/Placeholder/Mat_PackingStation.mat,
    Assets/_Project/Materials/Placeholder/Mat_DeliveryZone.mat,
    Assets/_Project/Materials/Placeholder/Mat_UpgradeStation.mat,
    Assets/_Project/Materials/Placeholder/Mat_Shelf.mat,
    Assets/_Project/Materials/Placeholder/Mat_WorkerSpawn.mat
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md (D-13, D-14)
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §Recommended Project Structure (Materials/Placeholder list)
  </read_first>
  <behavior>
    - Test 1: each material file exists at the expected path.
    - Test 2 (manual visual): in the Editor Material Inspector, each material uses Shader = `Universal Render Pipeline/Lit` and the `_BaseColor` matches the assigned palette below.
    - Test 3: when applied to a primitive in the scene (Task 3), the material renders as a solid color under URP_Mobile.
  </behavior>
  <action>
Create each material in the Editor (do NOT hand-author the YAML — let Unity generate it):

1. In Project view: navigate to `Assets/_Project/Materials/Placeholder/`.
2. For each material below, right-click → Create → Material → name it exactly. Open the Inspector and set Shader = `Universal Render Pipeline/Lit`. Click the swatch next to "Base Map" / "Base Color" and set the RGB:

| File | Color (D-13) | RGB hex (suggested) |
|---|---|---|
| `Mat_Floor.mat` | neutral light grey (warehouse floor) | `#9A9A9A` |
| `Mat_LoadingDock.mat` | grey | `#6E6E6E` |
| `Mat_PackingStation.mat` | blue | `#2D7FFF` |
| `Mat_DeliveryZone.mat` | green | `#3CB371` |
| `Mat_UpgradeStation.mat` | yellow | `#FFD43B` |
| `Mat_Shelf.mat` | brown | `#8B5A2B` |
| `Mat_WorkerSpawn.mat` | magenta | `#FF00FF` |

3. Leave Smoothness / Metallic at defaults (0 / 0). Do not assign any maps. Mobile-aggressive URP keeps these flat.

4. Save the scene/asset (Ctrl/Cmd-S). The `.mat` files appear under `Assets/_Project/Materials/Placeholder/` and are tracked by git as `merge=unityyamlmerge` text per the `.gitattributes` from plan 01-01.

> Why URP/Lit and not URP/Unlit? Phase 2+ may add baked or directional lighting; URP/Lit accepts both. Mobile-aggressive URP makes Lit cheap because shadows are off (URP_Mobile asset, Task 1 of plan 01-01).
  </action>
  <verify>
    <automated>
for mat in Mat_Floor Mat_LoadingDock Mat_PackingStation Mat_DeliveryZone Mat_UpgradeStation Mat_Shelf Mat_WorkerSpawn; do
  test -f "Assets/_Project/Materials/Placeholder/$mat.mat" || { echo "MISSING $mat.mat"; exit 1; }
  # All seven must reference the URP/Lit shader (string appears in serialized YAML m_Shader/m_Name)
  grep -q 'Universal Render Pipeline/Lit' "Assets/_Project/Materials/Placeholder/$mat.mat" || { echo "$mat.mat is not URP/Lit"; exit 1; }
done
    </automated>
  </verify>
  <done>
    All seven materials exist as `.mat` files. Each Material Inspector shows Shader = `Universal Render Pipeline/Lit` and the prescribed base color. No materials reference legacy Built-in/Standard shader.
  </done>
  <acceptance_criteria>
    - Files exist: `Mat_Floor.mat`, `Mat_LoadingDock.mat`, `Mat_PackingStation.mat`, `Mat_DeliveryZone.mat`, `Mat_UpgradeStation.mat`, `Mat_Shelf.mat`, `Mat_WorkerSpawn.mat` under `Assets/_Project/Materials/Placeholder/`.
    - Each `.mat` YAML contains the substring `Universal Render Pipeline/Lit` (grep gate).
    - Visual sanity: each material renders as a flat colored swatch on a default Cube/Plane preview.
  </acceptance_criteria>
</task>

<task type="auto" tdd="true">
  <name>Task 3: Compose Warehouse_MVP scene — 12 required GameObjects + Bootstrap-GameManager wiring + Build Settings registration</name>
  <files>
    Assets/_Project/Scenes/Warehouse_MVP.unity,
    ProjectSettings/EditorBuildSettings.asset,
    Assets/Tests/EditMode/BootstrapSmokeTests.cs
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md (D-10, D-11, D-13, D-14)
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §System Architecture Diagram, §Common Pitfalls 6, 9
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-VALIDATION.md (BootstrapSmokeTests.RequiredGameObject_IsPresent row)
    - specs/06-technical-architecture-spec.md §7 (Scene Requirements — MVP Scene)
  </read_first>
  <behavior>
    - Test 1 (BootstrapSmokeTests.RequiredGameObject_IsPresent — added by this task): parameterized over 12 names; each `GameObject.Find(name)` returns non-null after `EditorSceneManager.OpenScene(Warehouse_MVP)`.
    - Test 2 (manual visual in Editor): scene loads with floor, six colored station primitives, a Player capsule, an empty CameraRig hierarchy, an empty UICanvas, an EventSystem GameObject, and the empty `GameManager` and `Bootstrap` GameObjects. Bootstrap.gameManager Inspector slot is populated with a reference to the GameManager GameObject.
    - Test 3 (PlayMode — added in Task 4): `SceneManager.LoadSceneAsync("Warehouse_MVP")` resolves and the GameManager logs init.
  </behavior>
  <action>
**Step A — Create the scene.**
1. `File > New Scene > Basic (URP)` template (gives you Main Camera + Directional Light at start).
2. `File > Save As → Assets/_Project/Scenes/Warehouse_MVP.unity`.
3. The default Main Camera will be moved/replaced under CameraRig in plan 01-03; for this plan, leave it as-is (it gives us framing during Editor work).

**Step B — Author the floor and six placeholder stations.**

| GameObject name | Primitive | Position (x,y,z) | Scale (x,y,z) | Material |
|---|---|---|---|---|
| `Floor` | Plane | (0, 0, 0) | (3, 1, 3) → 30×30 m floor | `Mat_Floor` |
| `LoadingDock` | Cube | (-8, 0.75, 6) | (4, 1.5, 3) | `Mat_LoadingDock` (grey) |
| `PackingStation` | Cube | (0, 0.5, 0) | (2, 1, 2) | `Mat_PackingStation` (blue) |
| `DeliveryZone` | Cube | (8, 0.25, 6) | (4, 0.5, 3) | `Mat_DeliveryZone` (green) |
| `UpgradeStation` | Cube | (8, 0.5, -4) | (1.5, 1, 1.5) | `Mat_UpgradeStation` (yellow) |
| `ShelfArea` | Cube | (-8, 1, -4) | (5, 2, 1.5) | `Mat_Shelf` (brown) |
| `WorkerSpawn` | Cylinder | (-8, 0.05, 4) | (0.5, 0.05, 0.5) | `Mat_WorkerSpawn` (magenta) — flat marker disk |

**Naming is exact and case-sensitive** because `BootstrapSmokeTests.RequiredGameObject_IsPresent` uses `GameObject.Find` with these exact strings. Drag the matching material from `Assets/_Project/Materials/Placeholder/` onto each primitive. Floor is parented at the scene root; all stations are parented at the scene root (no nesting in Phase 1).

**Step C — Player placeholder.**

Add a Cylinder (or Capsule) named exactly `Player` at position `(0, 1, -3)`, scale `(0.6, 1, 0.6)` (roughly human-shaped). Material: leave default (will be replaced with art later). Add a `Rigidbody` component (Use Gravity = on, Is Kinematic = off — the gravity check ensures Phase 2 sees a sane physics setup) and a `CapsuleCollider` if not auto-added. **Do NOT add any input or movement scripts** — those are Phase 2 (MOVE-01).

**Step D — CameraRig, UICanvas, EventSystem placeholders.**

Create three empty GameObjects at the scene root with these exact names:
- `CameraRig` — empty GameObject at (0, 0, 0). Plan 01-03 will populate it with the Cinemachine vcam and re-parent the Main Camera into it.
- `UICanvas` — empty GameObject at (0, 0, 0). Plan 01-03 will replace this with a real Canvas. **Phase 1 just needs the name to exist** so the smoke test passes; plan 01-03 swaps the empty for the real Canvas without renaming.

  > Implementation note: if creating an empty GameObject named `UICanvas` and then plan 01-03 creates a real Canvas with the same name, Unity's `GameObject.Find` returns whichever Canvas exists first. To keep Phase 1 deterministic, **delete the empty UICanvas placeholder during plan 01-03 immediately before adding the real Canvas, ensuring there is exactly one `UICanvas` in the scene at any time**.
- `EventSystem` — empty GameObject at (0, 0, 0). Plan 01-03 will add the EventSystem + InputSystemUIInputModule components. Same one-of-a-kind rule applies.

**Step E — Bootstrap and GameManager GameObjects.**

Create two empty GameObjects at the scene root:
- `GameManager` at (0, 0, 0) — Add Component → search "GameManager" → select `WM.Core.GameManager`.
- `Bootstrap` at (0, 0, 0) — Add Component → search "Bootstrap" → select `WM.Core.Bootstrap`. Then in the Inspector, drag the `GameManager` GameObject from the Hierarchy onto the Bootstrap component's `Game Manager` slot. **This wiring is mandatory** — plan 01-01 Task 1 emits a `Debug.LogError` if it's null on Awake.

**Step F — Register the scene in Build Settings** (RESEARCH Pitfall 9 — black screen on device if scene isn't in EditorBuildSettings).
1. `File > Build Profiles > Scene List > Add Open Scenes`. Confirm `Assets/_Project/Scenes/Warehouse_MVP.unity` appears with index 0.
2. Save the scene (Cmd/Ctrl-S). Save the project (`File > Save Project`) so `ProjectSettings/EditorBuildSettings.asset` is written.

**Step G — Add `BootstrapSmokeTests.cs` to the EditMode test asmdef** (VALIDATION.md row 01-02-* + RESEARCH §Code Examples). This is the EditMode scene-shape test. Use this exact source:

`Assets/Tests/EditMode/BootstrapSmokeTests.cs`:
```csharp
// Source: VALIDATION.md row 01-02-* + RESEARCH §EditMode smoke test code example.
// Covers BOOT-02 — required GameObjects in Warehouse_MVP scene.
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

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
            Assert.That(go, Is.Not.Null,
                $"Required GameObject '{name}' missing from {ScenePath}");
        }

        [TearDown]
        public void Cleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
    }
}
```

**Sanity:** Save again; run `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -`. All 12 `RequiredGameObject_IsPresent` cases must pass on top of the 3 `BootstrapStructureTests` from plan 01-01.
  </action>
  <verify>
    <automated>
test -f Assets/_Project/Scenes/Warehouse_MVP.unity

# Scene YAML must reference each required GameObject name (the .unity scene is YAML text per gitattributes)
for name in Bootstrap GameManager CameraRig UICanvas EventSystem Player LoadingDock PackingStation DeliveryZone UpgradeStation ShelfArea WorkerSpawn Floor; do
  grep -q "m_Name: $name$" Assets/_Project/Scenes/Warehouse_MVP.unity || { echo "Scene missing GameObject named '$name'"; exit 1; }
done

# Scene registered in Build Settings
grep -q 'Warehouse_MVP.unity' ProjectSettings/EditorBuildSettings.asset

# EditMode smoke test source present
test -f Assets/Tests/EditMode/BootstrapSmokeTests.cs
grep -q 'class BootstrapSmokeTests' Assets/Tests/EditMode/BootstrapSmokeTests.cs
grep -q 'public void RequiredGameObject_IsPresent' Assets/Tests/EditMode/BootstrapSmokeTests.cs
# All 12 [TestCase] entries present
for name in Bootstrap GameManager CameraRig UICanvas EventSystem Player LoadingDock PackingStation DeliveryZone UpgradeStation ShelfArea WorkerSpawn; do
  grep -q "\[TestCase(\"$name\")\]" Assets/Tests/EditMode/BootstrapSmokeTests.cs || { echo "TestCase missing: $name"; exit 1; }
done

# CLI gate (replace UNITY)
# UNITY=/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity
# "$UNITY" -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -
echo "Run EditMode CLI command above; all RequiredGameObject_IsPresent cases must pass."
    </automated>
  </verify>
  <done>
    Scene exists with all 12 required GameObjects (plus Floor) at the listed positions and materials. Bootstrap's `gameManager` Inspector slot points at the GameManager GameObject (the YAML for `Warehouse_MVP.unity` shows a non-zero `fileID` reference under the Bootstrap component). Scene is in Build Settings. `BootstrapSmokeTests.cs` runs all 12 cases green via Unity Test Framework CLI.
  </done>
  <acceptance_criteria>
    - File `Assets/_Project/Scenes/Warehouse_MVP.unity` exists.
    - Scene contains GameObjects named exactly: `Player`, `LoadingDock`, `PackingStation`, `DeliveryZone`, `UpgradeStation`, `ShelfArea`, `WorkerSpawn`, `CameraRig`, `UICanvas`, `EventSystem`, `GameManager`, `Bootstrap`, plus `Floor` (helper, not strictly required by BOOT-02 but needed for visual sanity).
    - Each colored station primitive has the matching `Mat_*` material (loading dock=grey, packing=blue, delivery=green, upgrade=yellow, shelf=brown, worker spawn=magenta).
    - Bootstrap component's `Game Manager` Inspector field is populated (non-null reference to the GameManager GameObject in the same scene).
    - `Warehouse_MVP.unity` is registered in `EditorBuildSettings.scenes` (grep gate).
    - `BootstrapSmokeTests.cs` exists and contains 12 `[TestCase(...)]` entries covering the 12 required names.
    - CLI: `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` exits 0 with all 12 `RequiredGameObject_IsPresent` cases green.
  </acceptance_criteria>
</task>

<task type="auto" tdd="true">
  <name>Task 4: Replace PlayMode smoke test with real scene-load + GameManager-init runtime test</name>
  <files>
    Assets/Tests/PlayMode/PlayModeSmokeTests.cs
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §PlayMode smoke test (Code Examples)
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-VALIDATION.md (PlayModeSmokeTests row)
  </read_first>
  <behavior>
    - Test 1 (PlayModeSmokeTests.Scene_Loads_GameManagerInitializes): SceneManager.LoadSceneAsync("Warehouse_MVP") completes; LogAssert.Expect catches "GameManager initialized"; GameObject.Find("GameManager") returns non-null with a `WM.Core.GameManager` component attached.
    - Test 2: the trivial `PlayModeAssembly_Compiles` test from plan 01-01 is REPLACED by the real test. CLI PlayMode run still exits 0.
  </behavior>
  <action>
**Replace the entire content** of `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` (created in plan 01-01 with a trivial body) with this real runtime smoke test. Use this exact source:

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
            // Plan 01-02 Task 3 Step F adds Warehouse_MVP.unity to EditorBuildSettings.
            LogAssert.Expect(LogType.Log, "GameManager initialized");

            yield return SceneManager.LoadSceneAsync("Warehouse_MVP", LoadSceneMode.Single);
            // Wait one frame for Awake/Start chain to finish.
            yield return null;

            GameObject gm = GameObject.Find("GameManager");
            Assert.That(gm, Is.Not.Null, "GameManager GameObject not found in loaded scene");
            Assert.That(gm.GetComponent<GameManager>(), Is.Not.Null,
                "GameObject 'GameManager' is missing the WM.Core.GameManager component");

            GameObject bootstrap = GameObject.Find("Bootstrap");
            Assert.That(bootstrap, Is.Not.Null, "Bootstrap GameObject not found");
            Assert.That(bootstrap.GetComponent<Bootstrap>(), Is.Not.Null,
                "GameObject 'Bootstrap' is missing the WM.Core.Bootstrap component");
        }
    }
}
```

> Why `LogAssert.Expect` BEFORE `LoadSceneAsync`? Unity Test Framework requires the expectation to be registered before the log is emitted; `Awake` + `Start` happen during `LoadSceneAsync` so we must register first.

> Why does the test assembly need `using WM.Core`? `WM.Tests.PlayMode.asmdef` from plan 01-01 already references `WM.Core`, so this resolves cleanly.

Run `<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -`. Must exit 0 with `Scene_Loads_GameManagerInitializes` green.
  </action>
  <verify>
    <automated>
test -f Assets/Tests/PlayMode/PlayModeSmokeTests.cs
grep -q 'class PlayModeSmokeTests' Assets/Tests/PlayMode/PlayModeSmokeTests.cs
grep -q 'public IEnumerator Scene_Loads_GameManagerInitializes' Assets/Tests/PlayMode/PlayModeSmokeTests.cs
grep -q 'LogAssert.Expect(LogType.Log, "GameManager initialized")' Assets/Tests/PlayMode/PlayModeSmokeTests.cs
grep -q 'SceneManager.LoadSceneAsync("Warehouse_MVP"' Assets/Tests/PlayMode/PlayModeSmokeTests.cs
grep -q 'using WM.Core;' Assets/Tests/PlayMode/PlayModeSmokeTests.cs

# CLI gate (replace UNITY)
# UNITY=/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity
# "$UNITY" -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -
echo "Run PlayMode CLI command above; Scene_Loads_GameManagerInitializes must pass."
    </automated>
  </verify>
  <done>
    `<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -` exits 0 with `Scene_Loads_GameManagerInitializes` green. The Console (in batch log) emits "GameManager initialized" exactly once during the test, captured by `LogAssert.Expect`.
  </done>
  <acceptance_criteria>
    - `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` declares class `PlayModeSmokeTests` with `[UnityTest] public IEnumerator Scene_Loads_GameManagerInitializes()`.
    - Test calls `LogAssert.Expect(LogType.Log, "GameManager initialized")` before loading the scene.
    - Test asserts both `GameManager` and `Bootstrap` GameObjects are present with their respective `WM.Core.GameManager` / `WM.Core.Bootstrap` components attached.
    - CLI: `<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -` exits 0.
  </acceptance_criteria>
</task>

</tasks>

<threat_model>
## Trust Boundaries

| Boundary | Description |
|----------|-------------|
| Scene asset (.unity YAML) → repo | YAML scene must merge cleanly across collaborators (Unity YAML Merge tool); avoid LFS to preserve diffs |
| Bootstrap (Awake) → consumers (Init) | Composition root has implicit trust over service lifetime; injection failures must surface loudly |

## STRIDE Threat Register

| Threat ID | Category | Component | Disposition | Mitigation Plan |
|-----------|----------|-----------|-------------|-----------------|
| T-01-03 | Tampering | `Warehouse_MVP.unity` YAML integrity | mitigate | Inherits `.gitattributes` rule from plan 01-01 (`.unity merge=unityyamlmerge`). This plan does not LFS-track the scene. Verified by Task 3 verify gate (scene grep finds `m_Name:` entries — proves it's text). |
| T-01-09 | Information Disclosure | Service stub Debug.Log output | accept | `NullAnalyticsService` etc. log to Console at Phase 1 only. No PII, low risk. Real implementations in Phases 6/9/10 will tighten log levels. |
| T-01-10 | Tampering / DoS | Bootstrap missing `[SerializeField]` reference at runtime | mitigate | `Bootstrap.Awake` Debug.LogErrors and short-circuits if `gameManager == null` (Task 1 Step A). EditMode smoke test verifies the scene contains both GameObjects; PlayMode smoke test verifies `GameManager initialized` actually fires (which proves the reference was wired). |

No HIGH-severity threats — Phase 1 has no runtime auth, persistence, or network surface.
</threat_model>

<verification>
**Phase-level checks for plan 01-02:**

1. **Scene exists:** `Assets/_Project/Scenes/Warehouse_MVP.unity` is on disk and is text-mergeable YAML (grep finds `m_Name:` entries).
2. **All 12 required GameObjects present:** `BootstrapSmokeTests.RequiredGameObject_IsPresent` parameterized test green for all 12 names.
3. **Bootstrap-GameManager wiring:** Bootstrap component's `Game Manager` Inspector slot is populated (Phase 1 mandatory wiring).
4. **Runtime composition root works:** `PlayModeSmokeTests.Scene_Loads_GameManagerInitializes` green — proves Bootstrap.Awake runs first, services are injected, GameManager.Start logs "GameManager initialized".
5. **Build Settings:** `Warehouse_MVP.unity` registered in `ProjectSettings/EditorBuildSettings.asset` (grep gate). Without this, Phase 11's TestFlight build is a black screen (Pitfall 9).
6. **Materials:** All seven placeholder URP/Lit materials present and referenced by the scene's primitives.
</verification>

<success_criteria>
- Editor opens scene `Warehouse_MVP` with no Console errors.
- Pressing Play in the Editor logs "GameManager initialized" exactly once and does not throw.
- `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` exits 0 with both `BootstrapStructureTests.*` (from plan 01-01) and `BootstrapSmokeTests.RequiredGameObject_IsPresent` (12 cases) green.
- `<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -` exits 0 with `PlayModeSmokeTests.Scene_Loads_GameManagerInitializes` green.
- Running `<UNITY> -batchmode -quit -projectPath . -buildTarget iOS -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -` produces `build/ios/Unity-iPhone.xcodeproj` (BuildScript from plan 01-01 references this scene; this plan unblocks the actual build to succeed). Manual verification recorded in `01-VERIFICATION.md`.
</success_criteria>

<output>
After completion, create `.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-02-SUMMARY.md` recording: scene size in lines (sanity-check it's not surprisingly large), positions/scales actually used per station (in case the executor adjusted them for visual sanity), and any Editor warnings (must remain 0 errors). Note any deviation from the position/scale table in Task 3 Step B with rationale.
</output>
