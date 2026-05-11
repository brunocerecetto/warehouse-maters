---
phase: 01-project-bootstrap-empty-warehouse-scene
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - .gitignore
  - .gitattributes
  - ProjectSettings/ProjectVersion.txt
  - ProjectSettings/ProjectSettings.asset
  - ProjectSettings/QualitySettings.asset
  - ProjectSettings/GraphicsSettings.asset
  - ProjectSettings/EditorBuildSettings.asset
  - ProjectSettings/InputManager.asset
  - Packages/manifest.json
  - Packages/packages-lock.json
  - Assets/_Project/Art/.gitkeep
  - Assets/_Project/Audio/.gitkeep
  - Assets/_Project/Materials/.gitkeep
  - Assets/_Project/Materials/Placeholder/.gitkeep
  - Assets/_Project/Prefabs/.gitkeep
  - Assets/_Project/Scenes/.gitkeep
  - Assets/_Project/ScriptableObjects/Boxes/.gitkeep
  - Assets/_Project/ScriptableObjects/Orders/.gitkeep
  - Assets/_Project/ScriptableObjects/Upgrades/.gitkeep
  - Assets/_Project/ScriptableObjects/Workers/.gitkeep
  - Assets/_Project/ScriptableObjects/Economy/.gitkeep
  - Assets/_Project/Settings/URP_Mobile.asset
  - Assets/_Project/Settings/URP_Mobile_Renderer.asset
  - Assets/_Project/Scripts/Core/WM.Core.asmdef
  - Assets/_Project/Scripts/Core/IAnalyticsService.cs
  - Assets/_Project/Scripts/Core/IAdService.cs
  - Assets/_Project/Scripts/Core/IIapService.cs
  - Assets/_Project/Scripts/Core/ISaveService.cs
  - Assets/_Project/Scripts/Core/Stubs/NullAnalyticsService.cs
  - Assets/_Project/Scripts/Core/Stubs/NullAdService.cs
  - Assets/_Project/Scripts/Core/Stubs/NullIapService.cs
  - Assets/_Project/Scripts/Core/Stubs/NullSaveService.cs
  - Assets/_Project/Scripts/Player/WM.Player.asmdef
  - Assets/_Project/Scripts/Boxes/WM.Boxes.asmdef
  - Assets/_Project/Scripts/Orders/WM.Orders.asmdef
  - Assets/_Project/Scripts/Stations/WM.Stations.asmdef
  - Assets/_Project/Scripts/Upgrades/WM.Upgrades.asmdef
  - Assets/_Project/Scripts/Workers/WM.Workers.asmdef
  - Assets/_Project/Scripts/Economy/WM.Economy.asmdef
  - Assets/_Project/Scripts/Save/WM.Save.asmdef
  - Assets/_Project/Scripts/Analytics/WM.Analytics.asmdef
  - Assets/_Project/Scripts/Monetization/WM.Monetization.asmdef
  - Assets/_Project/Scripts/UI/WM.UI.asmdef
  - Assets/_Project/Scripts/Editor/WM.Editor.asmdef
  - Assets/_Project/Scripts/Editor/BuildScript.cs
  - Assets/Tests/EditMode/WM.Tests.EditMode.asmdef
  - Assets/Tests/EditMode/BootstrapStructureTests.cs
  - Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef
  - Assets/Tests/PlayMode/PlayModeSmokeTests.cs
autonomous: true
requirements:
  - BOOT-01
user_setup:
  - service: unity-hub
    why: "Unity Hub must be installed to provision the editor binary 6000.3.7f1"
    dashboard_config:
      - task: "Install Unity Hub from https://unity.com/download"
        location: "Local machine"
      - task: "Install Unity 6000.3.7f1 (6.3 LTS) via Hub > Installs > Add"
        location: "Unity Hub"
  - service: xcode
    why: "iOS build pipeline (BuildScript.BuildIOS) requires Xcode + Command Line Tools to compile the produced Xcode project later"
    dashboard_config:
      - task: "Install Xcode 15.x or 16.x from Mac App Store"
        location: "Mac App Store"
      - task: "Run `xcode-select --install` to install Command Line Tools"
        location: "Terminal"
      - task: "Run `xcode-select --switch /Applications/Xcode.app` if multiple Xcode installs exist"
        location: "Terminal"
  - service: git-lfs
    why: ".gitattributes uses git-lfs filters for binary assets"
    dashboard_config:
      - task: "Install git-lfs (`brew install git-lfs`)"
        location: "Terminal"
      - task: "Initialize for this repo (`git lfs install --local`)"
        location: "Repo root"

must_haves:
  truths:
    - "Project opens in Unity 6.3.7f1 without compile errors"
    - "All 13 production asmdefs and 2 test asmdefs are recognized by the Editor"
    - "The URP mobile-aggressive asset is set as the active render pipeline"
    - "Service interfaces (IAnalyticsService, IAdService, IIapService, ISaveService) compile and are referenced by NullStub implementations in WM.Core"
    - "Running the Unity CLI EditMode test platform exits 0 with at least the BootstrapStructureTests green"
    - "Running BuildScript.BuildIOS via -batchmode -executeMethod produces build/ios/Unity-iPhone.xcodeproj OR exits non-zero with a Debug.LogError on failure (no silent CI green)"
    - "Repo .gitignore excludes Library/, Temp/, Logs/, build/, signing certs (*.p12, *.mobileprovision)"
    - "Repo .gitattributes keeps .unity/.prefab/generic .asset as merge=unityyamlmerge text (NOT LFS) and only LFS-tracks true binaries"
    - "ProjectSettings/ProjectSettings.asset reflects iOS Player Settings configured via Inspector (IL2CPP, Metal-only, iOS 15+, Active Input Handling = New, Bundle Id, Portrait, Linear color space)"
  artifacts:
    - path: "ProjectSettings/ProjectVersion.txt"
      provides: "Pinned editor version"
      contains: "m_EditorVersion: 6000.3.7f1"
    - path: "Packages/manifest.json"
      provides: "Pinned Unity package versions"
      contains: "com.unity.cinemachine\", com.unity.inputsystem\", com.unity.test-framework\", com.unity.render-pipelines.universal"
    - path: "Assets/_Project/Scripts/Core/WM.Core.asmdef"
      provides: "Root asmdef for service interfaces and Bootstrap"
      contains: "\"name\": \"WM.Core\""
    - path: "Assets/_Project/Scripts/Editor/WM.Editor.asmdef"
      provides: "Editor-only asmdef hosting BuildScript"
      contains: "\"includePlatforms\": [\"Editor\"]"
    - path: "Assets/_Project/Scripts/Editor/BuildScript.cs"
      provides: "iOS build entry point invoked from CLI"
      contains: "namespace WM.Editor", "public static void BuildIOS()"
    - path: "Assets/Tests/EditMode/WM.Tests.EditMode.asmdef"
      provides: "Editor test assembly"
      contains: "\"includePlatforms\": [\"Editor\"]", "TestAssemblies"
    - path: "Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef"
      provides: "Play mode test assembly"
      contains: "TestAssemblies"
    - path: ".gitignore"
      provides: "Unity-flavored ignore + signing cert exclusion"
      contains: "/[Ll]ibrary/", "/build/", "*.mobileprovision", "*.p12"
    - path: ".gitattributes"
      provides: "LFS rules (binaries only) and YAML merge rules for Unity text assets"
      contains: "*.unity   merge=unityyamlmerge", "*.psd  filter=lfs", "*.png  filter=lfs"
    - path: "Assets/_Project/Settings/URP_Mobile.asset"
      provides: "Active render pipeline asset (mobile-aggressive)"
    - path: "ProjectSettings/ProjectSettings.asset"
      provides: "Serialized iOS Player Settings (Bundle Id, IL2CPP, Metal, iOS 15+, Active Input Handling = New, Portrait, Linear color space)"
      contains: "applicationIdentifier:", "com.warehousemaster.mvp", "iOSTargetOSVersionString: 15", "activeInputHandler: 1", "defaultScreenOrientation: 0", "m_ActiveColorSpace: 1"
  key_links:
    - from: "Packages/manifest.json"
      to: "Cinemachine 3.1.6 / Input System 1.15.0 / Test Framework 2.0.x / URP 17.x"
      via: "package manager pins"
      pattern: "com.unity.(cinemachine|inputsystem|test-framework|render-pipelines.universal)"
    - from: "ProjectSettings/GraphicsSettings.asset"
      to: "Assets/_Project/Settings/URP_Mobile.asset"
      via: "Default render pipeline reference"
      pattern: "m_CustomRenderPipeline.*URP_Mobile"
    - from: "ProjectSettings/ProjectSettings.asset"
      to: "iOS Player Settings (Inspector)"
      via: "Serialized YAML keys (applicationIdentifier, iOSTargetOSVersionString, activeInputHandler, defaultScreenOrientation, m_ActiveColorSpace)"
      pattern: "applicationIdentifier:|iOSTargetOSVersionString:|activeInputHandler:|defaultScreenOrientation:|m_ActiveColorSpace:"
    - from: "Assets/_Project/Scripts/Core/Stubs/NullAnalyticsService.cs"
      to: "Assets/_Project/Scripts/Core/IAnalyticsService.cs"
      via: "interface implementation"
      pattern: "class Null.*Service\\s*:\\s*I.*Service"
---

<objective>
Stand up the Unity 6.3.7f1 iOS project skeleton: pinned editor version, package manifest with Cinemachine 3.1.6 / Input System 1.15.0 / URP / Test Framework, the full `Assets/_Project/` folder tree, all 13 production asmdefs (one per system folder), the 2 test asmdefs (EditMode + PlayMode), service interfaces with no-op stub implementations under `WM.Core`, the `BuildScript.BuildIOS` CLI entry point with explicit failure exit codes, repo hygiene files (`.gitignore`, `.gitattributes`), and Wave-0 smoke tests that prove the project compiles and the structure exists.

Purpose: This is the bottom layer of the walking skeleton — without it, no later plan can compile a script, run a test, or produce an iOS build. It also locks the architectural topology (asmdef graph, plain-C# composition root via interface stubs in `WM.Core`) that Phases 2–11 build on top of.

Output: Compilable Unity project at the repo root, green EditMode test run, `BuildScript.BuildIOS` produces `build/ios/Unity-iPhone.xcodeproj` (or exits 1 with a meaningful log).
</objective>

<execution_context>
@/Users/brunocerecetto/repos/mine/warehouse-maters/.claude/get-shit-done/workflows/execute-plan.md
@/Users/brunocerecetto/repos/mine/warehouse-maters/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/PROJECT.md
@.planning/ROADMAP.md
@.planning/STATE.md
@.planning/REQUIREMENTS.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-VALIDATION.md
@.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-SKELETON.md
@CLAUDE.md
@specs/06-technical-architecture-spec.md
@specs/07-mvp-backlog-acceptance-criteria.md

<interfaces>
<!-- Asmdef wiring matrix from RESEARCH.md Pattern 1. Use these EXACT references. -->

WM.Core asmdef references: (none — root)
WM.Player → WM.Core
WM.Boxes → WM.Core
WM.Orders → WM.Core, WM.Boxes
WM.Stations → WM.Core, WM.Boxes, WM.Orders
WM.Upgrades → WM.Core, WM.Economy
WM.Workers → WM.Core, WM.Boxes
WM.Economy → WM.Core
WM.Save → WM.Core
WM.Analytics → WM.Core
WM.Monetization → WM.Core, WM.Analytics
WM.UI → WM.Core, WM.Economy, WM.Orders, WM.Upgrades
WM.Editor → WM.Core (Editor platform only)
WM.Tests.EditMode → WM.Core, nunit.framework, UnityEngine.TestRunner, UnityEditor.TestRunner (Editor platform only, "Test Assemblies" toggle ON)
WM.Tests.PlayMode → WM.Core, nunit.framework, UnityEngine.TestRunner (Any Platform, NO UnityEditor.TestRunner reference, "Test Assemblies" toggle ON)

Service interface signatures (from D-09 + RESEARCH Pattern 2):
- IAnalyticsService.LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null)
- IAdService.ShowRewarded(System.Action<bool> onComplete)
- IIapService.Purchase(string productId, System.Action<bool> onComplete)
- ISaveService.Save(string key, string json) and string Load(string key)

(Phase 1 stubs only need to compile; bodies are no-ops or default returns. Real signatures will be refined in their owning phases.)
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Create Unity 6.3.7f1 project, pin packages, scaffold _Project/ folder tree, write repo hygiene files</name>
  <files>
    ProjectSettings/ProjectVersion.txt,
    ProjectSettings/ProjectSettings.asset,
    ProjectSettings/QualitySettings.asset,
    ProjectSettings/GraphicsSettings.asset,
    ProjectSettings/EditorBuildSettings.asset,
    ProjectSettings/InputManager.asset,
    Packages/manifest.json,
    Packages/packages-lock.json,
    Assets/_Project/Art/.gitkeep,
    Assets/_Project/Audio/.gitkeep,
    Assets/_Project/Materials/.gitkeep,
    Assets/_Project/Materials/Placeholder/.gitkeep,
    Assets/_Project/Prefabs/.gitkeep,
    Assets/_Project/Scenes/.gitkeep,
    Assets/_Project/ScriptableObjects/Boxes/.gitkeep,
    Assets/_Project/ScriptableObjects/Orders/.gitkeep,
    Assets/_Project/ScriptableObjects/Upgrades/.gitkeep,
    Assets/_Project/ScriptableObjects/Workers/.gitkeep,
    Assets/_Project/ScriptableObjects/Economy/.gitkeep,
    Assets/_Project/Settings/URP_Mobile.asset,
    Assets/_Project/Settings/URP_Mobile_Renderer.asset,
    .gitignore,
    .gitattributes
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md (D-01..D-20)
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §Standard Stack, §Recommended Project Structure, §iOS Player Settings, §URP Mobile-Aggressive Asset settings, §.gitignore, §.gitattributes, §Common Pitfalls 1, 8, 10, §Open Questions (RESOLVED)
    - specs/06-technical-architecture-spec.md §3 (Project Structure)
    - CLAUDE.md (Tech Stack section, Architecture Guidelines, File Discipline)
  </read_first>
  <behavior>
    - Test 1 (BootstrapStructureTests.AssetsProject_FoldersExist): every required folder under Assets/_Project/ resolves via AssetDatabase.IsValidFolder.
    - Test 2 (BootstrapStructureTests.UrpAssetAssigned): GraphicsSettings.defaultRenderPipeline is non-null AND its name contains "URP_Mobile".
    - Test 3 (BootstrapStructureTests.PackagesPinned): Packages/manifest.json contains the four required package keys.
    - Test 4 (manual): repo `.gitignore` excludes Library/, Temp/, Logs/, build/, *.mobileprovision, *.p12 (grepable).
    - Test 5 (manual): repo `.gitattributes` does NOT have LFS filters on `*.unity` or `*.prefab` (these stay text+yamlmerge).
    - Test 6 (grep gates): `ProjectSettings/ProjectSettings.asset` contains the serialized iOS Player Settings keys configured via Inspector in Step D — IL2CPP scripting backend on iPhone, Metal-only Graphics API, iOSTargetOSVersionString = 15, activeInputHandler = 1 (New), applicationIdentifier = `com.warehousemaster.mvp`, defaultScreenOrientation = 0 (Portrait), m_ActiveColorSpace = 1 (Linear).
  </behavior>
  <action>
Use Unity Hub to create a new project. **Do not edit ProjectSettings/*.asset YAML by hand** — let the Editor produce them, then commit.

**Step A — Create project via Unity Hub (one-time, manual but scriptable):**
- Open Unity Hub.
- Click "New project" → Editor: **6000.3.7f1** → Template: **Universal 3D** (URP 3D core template) → Location: this repo root → Project name: `warehouse-maters` (matches existing repo folder).
- If Hub creates a `warehouse-maters/` subfolder, move generated `Assets/`, `ProjectSettings/`, `Packages/` up to repo root and delete the empty subfolder. The repo root must contain `Assets/`, `ProjectSettings/`, `Packages/` directly alongside existing `CLAUDE.md`, `specs/`, `.planning/`.
- Verify `ProjectSettings/ProjectVersion.txt` first line is `m_EditorVersion: 6000.3.7f1`. If a slightly different 6000.3.x sub-version was installed, accept it but document in commit message.

**Step B — Pin packages in `Packages/manifest.json`:**
Add (or update) these three entries to the `dependencies` block:
```
"com.unity.cinemachine": "3.1.6",
"com.unity.inputsystem": "1.15.0",
"com.unity.test-framework": "2.0.1-pre.18"
```
Leave the URP and other Unity-injected entries that the Universal 3D template adds. After saving, **return to the Editor and let it resolve packages** (Editor auto-resolves on focus). Confirm `Packages/packages-lock.json` updates to lock the resolved versions.

**Step C — Switch active render pipeline to a hand-authored URP_Mobile asset:**
1. In Editor: `Assets/_Project/Settings/` → right-click → Create → Rendering → URP Asset (with Universal Renderer). Name it `URP_Mobile.asset`. The renderer asset will be created as `URP_Mobile_Renderer.asset` in the same folder.
2. Open the URP asset and apply the mobile-aggressive settings from RESEARCH §URP Mobile-Aggressive Asset settings:
   - General: Depth Texture **Off**, Opaque Texture **Off**.
   - Quality: MSAA **Disabled**, HDR **Off**, Render Scale **1.0**.
   - Lighting: Main Light **Per Pixel**, Cast Shadows **Off**; Additional Lights **Disabled**; Soft Shadows **Off**.
   - Post-processing: **Off**.
   - Advanced: SRP Batcher **On**, Dynamic Batching **On**, Depth Priming Mode **Disabled**, LOD Cross Fade **Disabled**.
3. Open the renderer asset: Renderer Features list **empty**, Rendering Path **Forward**.
4. `Edit > Project Settings > Graphics > Default Render Pipeline` = `URP_Mobile`.
5. `Edit > Project Settings > Quality > [your single quality level] > Render Pipeline` = `URP_Mobile`.
6. `Edit > Project Settings > Graphics > Render Graph` → **Compatibility Mode = Off** (RESEARCH Pitfall 10).

**Step D — Configure iOS Player Settings (manual, one-time):**
`Edit > Project Settings > Player > iOS tab` (RESEARCH §iOS Player Settings checklist):
- Identification → Bundle Identifier: `com.warehousemaster.mvp` (RESEARCH Pitfall 8 / threat T-01-01); Version: `0.1.0`; Build: `1`; Signing Team ID: blank (Phase 11).
- Configuration → Scripting Backend: **IL2CPP**; Api Compatibility Level: **.NET Standard 2.1**; Architecture: **ARM64**; Target SDK: **Device SDK**; Target minimum iOS Version: **15.0**; Active Input Handling: **Input System Package (New)** (D-03; restart Editor when prompted — RESEARCH Pitfall 7).
- Resolution and Presentation → Default Orientation: **Portrait**; only Portrait checked under Allowed Orientations.
- Other Settings → Auto Graphics API: **Off**; Graphics APIs: **Metal** only (remove OpenGLES2/3 if present); Color Space: **Linear**.

**Step E — Switch active build target to iOS:**
`File > Build Profiles > iOS > Switch Profile`. Wait for the asset re-import.

**Step F — Create the `Assets/_Project/` folder tree:**
Create empty folders (use `.gitkeep` placeholder files so git tracks them):
```
Assets/_Project/Art/.gitkeep
Assets/_Project/Audio/.gitkeep
Assets/_Project/Materials/.gitkeep
Assets/_Project/Materials/Placeholder/.gitkeep
Assets/_Project/Prefabs/.gitkeep
Assets/_Project/Scenes/.gitkeep
Assets/_Project/ScriptableObjects/Boxes/.gitkeep
Assets/_Project/ScriptableObjects/Orders/.gitkeep
Assets/_Project/ScriptableObjects/Upgrades/.gitkeep
Assets/_Project/ScriptableObjects/Workers/.gitkeep
Assets/_Project/ScriptableObjects/Economy/.gitkeep
```
(`Settings/` already contains the URP assets from Step C, so no `.gitkeep` needed there.)

Folder layout matches `specs/06-technical-architecture-spec.md` §3 verbatim per D-05. ScriptableObject subfolders are intentionally empty in this phase per D-05.

**Step G — Write `.gitignore` at repo root** (RESEARCH §.gitignore — based on github/gitignore Unity.gitignore + project-specific additions). Use this exact content:

```
# === Unity ===
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/[Mm]emoryCaptures/

# Asset meta data should only be ignored when the corresponding asset is also ignored
!/[Aa]ssets/**/*.meta

# Uncomment this line if you wish to ignore the asset store tools plugin
# /[Aa]ssets/AssetStoreTools*

# Autogenerated Jetbrains Rider plugin
/[Aa]ssets/Plugins/Editor/JetBrains*

# Visual Studio cache directory
.vs/

# Gradle cache directory
.gradle/

# Autogenerated VS/MD/Consulo solution and project files
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

# Unity3D generated meta files
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.aab
*.unitypackage
*.app
/build/

# === iOS ===
*.mobileprovision
*.p12
*.cer
*.pem
*.certSigningRequest
*.dSYM.zip
*.dSYM
DerivedData/
*.xcworkspace/xcuserdata/

# === macOS ===
.DS_Store
.AppleDouble
.LSOverride
Icon
._*
.Spotlight-V100
.Trashes

# === IDE ===
.idea/
.vscode/
*.swp

# === Crashlytics generated file ===
crashlytics-build.properties
```

**Step H — Write `.gitattributes` at repo root** (RESEARCH §.gitattributes + Open Question 1 — RESOLVED).

**Rationale:** D-20 deviation approved by user 2026-05-10; RESEARCH.md OQ-1 marked RESOLVED. `.unity`/`.prefab`/`.asset` stay text-merged (`merge=unityyamlmerge`); LFS tracks only true binaries (`.psd`, `.fbx`, `.png`, `.jpg`, `.wav`, `.mp3`, `.ogg`) plus the three special binary `.asset` subtypes (`LightingData.asset`, `*TerrainData.asset`, `NavMeshData.asset`). LFS-tracked YAML breaks Unity's `unityyamlmerge` tool and removes PR diffs (threat T-01-03 — asset graph integrity), which is why the deviation is the correct call.

Use this exact content:

```
## Auto-detect text files (line-ending normalization)
* text=auto

## Unity text assets — keep diffable, force LF, use Unity's YAML merge (NOT LFS)
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
*.asmdef   text eol=lf

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

After writing both files, run `git lfs install --local` once (idempotent).

**Sanity check (do not move on without these):**
- `git status` shows tracked `Assets/_Project/`, `ProjectSettings/`, `Packages/manifest.json`, `Packages/packages-lock.json`, `.gitignore`, `.gitattributes`.
- Open Editor → no compile errors in Console.
- `Edit > Project Settings > Graphics` → Default Render Pipeline shows `URP_Mobile`.
- `Edit > Project Settings > Player > iOS > Other Settings` → Active Input Handling = "Input System Package (New)".
  </action>
  <rationale>
**ProjectSettings.asset grep-gate caveat.** The verify block below greps `ProjectSettings/ProjectSettings.asset` for serialized YAML keys that correspond to settings configured via the Inspector in Step D (IL2CPP, Metal-only Graphics API, iOS 15 minimum, Active Input Handling = New, Bundle Id, Portrait orientation, Linear color space). The exact serialized YAML key names DO drift across Unity versions — the patterns below are best-effort based on community conventions and observed `6000.x` outputs. **Before locking these gates, the executor MUST validate every pattern against the actual `ProjectSettings/ProjectSettings.asset` produced by 6000.3.7f1 after Step D completes.** If a pattern does not match the real serialized output, update the grep to the actual key/value pair as serialized by Unity 6.3 (e.g., `iPhone:` may be nested under `scriptingBackend:` with different indentation; `iOSTargetOSVersionString` may serialize as `iOSTargetOSVersionString: 15.0` rather than `15`). Record any pattern adjustment in `01-01-SUMMARY.md` so plan-checker and downstream phases inherit the verified gate.
  </rationale>
  <verify>
    <automated>
# Verify pinned editor version
grep -q '^m_EditorVersion: 6000\.3\.[0-9]' ProjectSettings/ProjectVersion.txt

# Verify package pins
grep -q '"com.unity.cinemachine"' Packages/manifest.json
grep -q '"com.unity.inputsystem"' Packages/manifest.json
grep -q '"com.unity.test-framework"' Packages/manifest.json
grep -q '"com.unity.render-pipelines.universal"' Packages/manifest.json

# Verify folder skeleton (count must match exactly 11 .gitkeep files under Assets/_Project)
test "$(find Assets/_Project -name .gitkeep -type f | wc -l | tr -d ' ')" -ge 11

# Verify URP assets present
test -f Assets/_Project/Settings/URP_Mobile.asset
test -f Assets/_Project/Settings/URP_Mobile_Renderer.asset

# Verify .gitignore excludes the right things (filter comments out before counting)
grep -v '^#' .gitignore | grep -q '^/\[Ll\]ibrary/'
grep -v '^#' .gitignore | grep -q '^/build/'
grep -v '^#' .gitignore | grep -q '^\*\.mobileprovision'
grep -v '^#' .gitignore | grep -q '^\*\.p12'

# Verify .gitattributes deviation from D-20: NO LFS filter on .unity/.prefab/generic .asset
# (specifically — *TerrainData.asset and LightingData.asset SHOULD have LFS; bare *.unity should NOT)
! grep -E '^\*\.unity\s+filter=lfs' .gitattributes
! grep -E '^\*\.prefab\s+filter=lfs' .gitattributes
grep -q '^\*\.unity    merge=unityyamlmerge' .gitattributes
grep -q '^\*\.prefab   merge=unityyamlmerge' .gitattributes
grep -q '^\*\.png  filter=lfs' .gitattributes
grep -q '^LightingData\.asset filter=lfs' .gitattributes

# === Step D Player Settings serialized gates ===
# IMPORTANT: validate every pattern below against the ACTUAL ProjectSettings.asset
# produced by Unity 6.3.7f1 after Step D before locking. See <rationale> above.
# The patterns are best-effort community conventions; real key names DO drift across Unity versions.

# IL2CPP scripting backend on iOS (iPhone:1 means IL2CPP per the scriptingBackend dict).
# Try the nested form first, then fall back to the flat key match.
grep -qE 'scriptingBackend:[[:space:]]*$' ProjectSettings/ProjectSettings.asset && \
  grep -qE '^[[:space:]]+iPhone:[[:space:]]*1' ProjectSettings/ProjectSettings.asset || \
  grep -qE 'iPhone:[[:space:]]*1' ProjectSettings/ProjectSettings.asset

# Graphics APIs configured per build target (iOS list should be Metal=4 only).
# Existence of the m_BuildTargetGraphicsAPIs block is the gate; visual confirmation in Inspector
# (Player > Other Settings > Graphics APIs) is the secondary check.
grep -q 'm_BuildTargetGraphicsAPIs:' ProjectSettings/ProjectSettings.asset

# iOS minimum version 15.x
grep -q 'iOSTargetOSVersionString: 15' ProjectSettings/ProjectSettings.asset

# Active Input Handling = Input System Package (New) (1)
grep -q 'activeInputHandler: 1' ProjectSettings/ProjectSettings.asset

# Bundle Identifier set to project value
grep -q 'applicationIdentifier:' ProjectSettings/ProjectSettings.asset
grep -q 'com.warehousemaster.mvp' ProjectSettings/ProjectSettings.asset

# Portrait orientation default (defaultScreenOrientation: 0 = Portrait)
grep -q 'defaultScreenOrientation: 0' ProjectSettings/ProjectSettings.asset

# Color Space = Linear (m_ActiveColorSpace: 1)
grep -q 'm_ActiveColorSpace: 1' ProjectSettings/ProjectSettings.asset
    </automated>
  </verify>
  <done>
    All grep/test commands above exit 0. Editor opens with no compile errors. URP_Mobile is the active render pipeline. iOS is the active build target. .gitignore and .gitattributes match the exact content above (D-20 deviation per OQ-1 RESOLVED). Step D Player Settings serialize into `ProjectSettings/ProjectSettings.asset` with the documented YAML keys (or with executor-verified equivalents recorded in `01-01-SUMMARY.md`).
  </done>
  <acceptance_criteria>
    - File `ProjectSettings/ProjectVersion.txt` exists and starts with `m_EditorVersion: 6000.3`.
    - `Packages/manifest.json` contains `"com.unity.cinemachine"`, `"com.unity.inputsystem"`, `"com.unity.test-framework"`, `"com.unity.render-pipelines.universal"`.
    - All 11 folders under `Assets/_Project/{Art,Audio,Materials,Materials/Placeholder,Prefabs,Scenes,ScriptableObjects/Boxes,Orders,Upgrades,Workers,Economy}` exist and are tracked via `.gitkeep`.
    - `Assets/_Project/Settings/URP_Mobile.asset` and `Assets/_Project/Settings/URP_Mobile_Renderer.asset` both exist.
    - Active render pipeline is `URP_Mobile` (Project Settings → Graphics).
    - Player Settings: iOS bundle id = `com.warehousemaster.mvp`, IL2CPP, ARM64, Metal-only, iOS 15 minimum, Portrait, Active Input Handling = "Input System Package (New)" — confirmed in Inspector AND grep-verified in `ProjectSettings/ProjectSettings.asset` per the verify block (or with executor-validated patterns recorded in `01-01-SUMMARY.md`).
    - `.gitignore` is the exact content from Step G (verified via grep).
    - `.gitattributes` is the exact content from Step H — `.unity`, `.prefab`, `.asset` are merge=unityyamlmerge text, NOT LFS (D-20 deviation per OQ-1 RESOLVED).
  </acceptance_criteria>
</task>

<task type="auto" tdd="true">
  <name>Task 2: Create 13 production asmdefs, service interfaces, no-op stubs, and BuildScript.cs</name>
  <files>
    Assets/_Project/Scripts/Core/WM.Core.asmdef,
    Assets/_Project/Scripts/Core/IAnalyticsService.cs,
    Assets/_Project/Scripts/Core/IAdService.cs,
    Assets/_Project/Scripts/Core/IIapService.cs,
    Assets/_Project/Scripts/Core/ISaveService.cs,
    Assets/_Project/Scripts/Core/Stubs/NullAnalyticsService.cs,
    Assets/_Project/Scripts/Core/Stubs/NullAdService.cs,
    Assets/_Project/Scripts/Core/Stubs/NullIapService.cs,
    Assets/_Project/Scripts/Core/Stubs/NullSaveService.cs,
    Assets/_Project/Scripts/Player/WM.Player.asmdef,
    Assets/_Project/Scripts/Boxes/WM.Boxes.asmdef,
    Assets/_Project/Scripts/Orders/WM.Orders.asmdef,
    Assets/_Project/Scripts/Stations/WM.Stations.asmdef,
    Assets/_Project/Scripts/Upgrades/WM.Upgrades.asmdef,
    Assets/_Project/Scripts/Workers/WM.Workers.asmdef,
    Assets/_Project/Scripts/Economy/WM.Economy.asmdef,
    Assets/_Project/Scripts/Save/WM.Save.asmdef,
    Assets/_Project/Scripts/Analytics/WM.Analytics.asmdef,
    Assets/_Project/Scripts/Monetization/WM.Monetization.asmdef,
    Assets/_Project/Scripts/UI/WM.UI.asmdef,
    Assets/_Project/Scripts/Editor/WM.Editor.asmdef,
    Assets/_Project/Scripts/Editor/BuildScript.cs
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §Pattern 1 (Asmdef Dependency Graph), §Pattern 2 (Bootstrap Composition Root), §Pattern 6 (BuildScript), §Anti-Patterns to Avoid
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md (D-04, D-07, D-09, D-18)
    - The asmdef wiring matrix in this PLAN's `<interfaces>` block above
    - Service interface signatures in `<interfaces>` block
  </read_first>
  <behavior>
    - Test 1 (BootstrapStructureTests.AsmdefsPresent — covered by Task 3 below): all 13 production asmdef JSON files exist with the expected `name` value.
    - Test 2 (compile gate): Project compiles with no errors after asmdef + script files are created. Editor Console green.
    - Test 3 (manual): Each Null*Service stub class implements its corresponding I*Service interface (grep-verifiable via `class Null.*Service\s*:\s*I.*Service`).
    - Test 4 (CLI build smoke — manual / VERIFICATION.md): `WM.Editor.BuildScript.BuildIOS` is invokable via `-executeMethod` and exits 0 producing `build/ios/Unity-iPhone.xcodeproj`, OR exits 1 on failure with a Debug.LogError.
  </behavior>
  <action>
**Step A — Create asmdef files** (use `File > New File` from the Editor's Project view → right-click each Scripts/* folder → Create → Assembly Definition; OR write the JSON directly with the Write tool, then let the Editor import). The 13 production asmdefs use these EXACT JSON contents:

`Assets/_Project/Scripts/Core/WM.Core.asmdef`:
```json
{
    "name": "WM.Core",
    "rootNamespace": "WM.Core",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

`Assets/_Project/Scripts/Player/WM.Player.asmdef`:
```json
{ "name": "WM.Player", "rootNamespace": "WM.Player", "references": ["WM.Core"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Boxes/WM.Boxes.asmdef`:
```json
{ "name": "WM.Boxes", "rootNamespace": "WM.Boxes", "references": ["WM.Core"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Orders/WM.Orders.asmdef`:
```json
{ "name": "WM.Orders", "rootNamespace": "WM.Orders", "references": ["WM.Core", "WM.Boxes"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Stations/WM.Stations.asmdef`:
```json
{ "name": "WM.Stations", "rootNamespace": "WM.Stations", "references": ["WM.Core", "WM.Boxes", "WM.Orders"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Economy/WM.Economy.asmdef`:
```json
{ "name": "WM.Economy", "rootNamespace": "WM.Economy", "references": ["WM.Core"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Upgrades/WM.Upgrades.asmdef`:
```json
{ "name": "WM.Upgrades", "rootNamespace": "WM.Upgrades", "references": ["WM.Core", "WM.Economy"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Workers/WM.Workers.asmdef`:
```json
{ "name": "WM.Workers", "rootNamespace": "WM.Workers", "references": ["WM.Core", "WM.Boxes"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Save/WM.Save.asmdef`:
```json
{ "name": "WM.Save", "rootNamespace": "WM.Save", "references": ["WM.Core"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Analytics/WM.Analytics.asmdef`:
```json
{ "name": "WM.Analytics", "rootNamespace": "WM.Analytics", "references": ["WM.Core"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Monetization/WM.Monetization.asmdef`:
```json
{ "name": "WM.Monetization", "rootNamespace": "WM.Monetization", "references": ["WM.Core", "WM.Analytics"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/UI/WM.UI.asmdef`:
```json
{ "name": "WM.UI", "rootNamespace": "WM.UI", "references": ["WM.Core", "WM.Economy", "WM.Orders", "WM.Upgrades"], "includePlatforms": [], "excludePlatforms": [], "allowUnsafeCode": false, "overrideReferences": false, "precompiledReferences": [], "autoReferenced": true, "defineConstraints": [], "versionDefines": [], "noEngineReferences": false }
```

`Assets/_Project/Scripts/Editor/WM.Editor.asmdef` (Editor-platform-only — RESEARCH Pattern 1):
```json
{
    "name": "WM.Editor",
    "rootNamespace": "WM.Editor",
    "references": ["WM.Core"],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**Step B — Service interfaces in `WM.Core`** (D-09):

`Assets/_Project/Scripts/Core/IAnalyticsService.cs`:
```csharp
// Source: D-09 (service interface stubs in WM.Core)
using System.Collections.Generic;

namespace WM.Core
{
    /// <summary>Analytics emission boundary. Real impl arrives in Phase 9 (TEL-01).</summary>
    public interface IAnalyticsService
    {
        void LogEvent(string eventName, IReadOnlyDictionary<string, object> parameters = null);
    }
}
```

`Assets/_Project/Scripts/Core/IAdService.cs`:
```csharp
using System;

namespace WM.Core
{
    /// <summary>Rewarded-ad boundary. Real impl arrives in Phase 10 (ADS-01).</summary>
    public interface IAdService
    {
        void ShowRewarded(Action<bool> onComplete);
    }
}
```

`Assets/_Project/Scripts/Core/IIapService.cs`:
```csharp
using System;

namespace WM.Core
{
    /// <summary>IAP boundary. Out of MVP scope but reserved here for parity with spec §4.</summary>
    public interface IIapService
    {
        void Purchase(string productId, Action<bool> onComplete);
    }
}
```

`Assets/_Project/Scripts/Core/ISaveService.cs`:
```csharp
namespace WM.Core
{
    /// <summary>Local persistence boundary. Real impl arrives in Phase 6 (SAVE-01).</summary>
    public interface ISaveService
    {
        void Save(string key, string json);
        string Load(string key);
        bool HasKey(string key);
    }
}
```

**Step C — No-op stub implementations** under `Assets/_Project/Scripts/Core/Stubs/`:

`NullAnalyticsService.cs`:
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

`NullAdService.cs`:
```csharp
using System;
using UnityEngine;

namespace WM.Core
{
    public sealed class NullAdService : IAdService
    {
        public void ShowRewarded(Action<bool> onComplete)
        {
            Debug.Log("[NullAdService] ShowRewarded (no-op)");
            onComplete?.Invoke(false);
        }
    }
}
```

`NullIapService.cs`:
```csharp
using System;
using UnityEngine;

namespace WM.Core
{
    public sealed class NullIapService : IIapService
    {
        public void Purchase(string productId, Action<bool> onComplete)
        {
            Debug.Log($"[NullIapService] Purchase {productId} (no-op)");
            onComplete?.Invoke(false);
        }
    }
}
```

`NullSaveService.cs`:
```csharp
using System.Collections.Generic;

namespace WM.Core
{
    public sealed class NullSaveService : ISaveService
    {
        private readonly Dictionary<string, string> _store = new Dictionary<string, string>();

        public void Save(string key, string json) => _store[key] = json;

        public string Load(string key) => _store.TryGetValue(key, out var v) ? v : null;

        public bool HasKey(string key) => _store.ContainsKey(key);
    }
}
```

**Step D — `BuildScript.cs` in `WM.Editor`** (RESEARCH Pattern 6 — Pitfall 1: Unity does not propagate failure exit codes; we MUST call `EditorApplication.Exit(1)` on non-Succeeded). Use this exact source:

`Assets/_Project/Scripts/Editor/BuildScript.cs`:
```csharp
// Source: RESEARCH §Pattern 6, support.unity.com 211195263 (BuildPlayer exit codes)
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WM.Editor
{
    public static class BuildScript
    {
        // Invoked from CLI:
        //   <UNITY> -batchmode -quit -projectPath . -buildTarget iOS \
        //           -executeMethod WM.Editor.BuildScript.BuildIOS -logFile -
        //
        // Phase 1 ships a skeleton: produces the Xcode project, no signing, no archive.
        // Phase 11 will extend with signing team, provisioning, post-build hooks.
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

**Step E — Sanity check**: open the Editor, wait for compilation. The Console must be **green** (zero errors, warnings acceptable). If any "Cyclic assembly definition references" error appears, audit the JSON files against the wiring matrix in `<interfaces>` above. If "The type or namespace name 'NUnit' could not be found" appears, the test asmdef from Task 3 is missing the "Test Assemblies" toggle — that's Task 3's concern, not this one.

The `Warehouse_MVP.unity` scene path referenced by `BuildScript.cs` does **not yet exist** — that's plan 01-02. The BuildScript will compile fine (string literal); attempting to actually run `BuildIOS` before plan 01-02 lands will fail at runtime, which is expected and acceptable for this plan's gate.
  </action>
  <verify>
    <automated>
# Asmdef name verification — every required asmdef present with correct name
for asmdef in Core Player Boxes Orders Stations Upgrades Workers Economy Save Analytics Monetization UI Editor; do
  test -f "Assets/_Project/Scripts/$asmdef/WM.$asmdef.asmdef" || { echo "MISSING WM.$asmdef.asmdef"; exit 1; }
  grep -q "\"name\": \"WM\\.$asmdef\"" "Assets/_Project/Scripts/$asmdef/WM.$asmdef.asmdef" || { echo "WRONG name in WM.$asmdef.asmdef"; exit 1; }
done

# WM.Editor must be Editor-platform-only
grep -q '"includePlatforms": \["Editor"\]' Assets/_Project/Scripts/Editor/WM.Editor.asmdef

# Service interfaces present
test -f Assets/_Project/Scripts/Core/IAnalyticsService.cs
test -f Assets/_Project/Scripts/Core/IAdService.cs
test -f Assets/_Project/Scripts/Core/IIapService.cs
test -f Assets/_Project/Scripts/Core/ISaveService.cs

# No-op stub classes implement their interfaces
grep -E 'class NullAnalyticsService\s*:\s*IAnalyticsService' Assets/_Project/Scripts/Core/Stubs/NullAnalyticsService.cs
grep -E 'class NullAdService\s*:\s*IAdService' Assets/_Project/Scripts/Core/Stubs/NullAdService.cs
grep -E 'class NullIapService\s*:\s*IIapService' Assets/_Project/Scripts/Core/Stubs/NullIapService.cs
grep -E 'class NullSaveService\s*:\s*ISaveService' Assets/_Project/Scripts/Core/Stubs/NullSaveService.cs

# BuildScript present, namespaced, with explicit exit on failure
grep -q 'namespace WM.Editor' Assets/_Project/Scripts/Editor/BuildScript.cs
grep -q 'public static void BuildIOS' Assets/_Project/Scripts/Editor/BuildScript.cs
grep -q 'EditorApplication.Exit(1)' Assets/_Project/Scripts/Editor/BuildScript.cs
grep -q 'BuildPipeline.BuildPlayer' Assets/_Project/Scripts/Editor/BuildScript.cs
grep -q 'Warehouse_MVP.unity' Assets/_Project/Scripts/Editor/BuildScript.cs

# Compile gate (CLI). Replace UNITY env var with editor path before running.
# Acceptable runtime: ~30s.
# UNITY=/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity
# "$UNITY" -batchmode -quit -projectPath . -logFile - -executeMethod UnityEditor.SyncVS.SyncSolution
echo "Compile gate: open Editor and verify Console has 0 errors (manual on first run; CLI gate runs in Task 3)"
    </automated>
  </verify>
  <done>
    All grep/test commands above exit 0. Editor Console is green (0 errors). All 13 production asmdefs exist with correct `name` values and the wiring matrix from `<interfaces>` (cross-check by opening each asmdef in the Inspector — references list matches). Service interfaces compile and are implemented by their `Null*Service` counterparts. `BuildScript.BuildIOS` is namespaced as `WM.Editor.BuildScript.BuildIOS`, calls `BuildPipeline.BuildPlayer`, and exits 1 on `result != BuildResult.Succeeded`.
  </done>
  <acceptance_criteria>
    - File `Assets/_Project/Scripts/Core/WM.Core.asmdef` contains `"name": "WM.Core"` and an empty `references` array.
    - File `Assets/_Project/Scripts/Editor/WM.Editor.asmdef` contains `"includePlatforms": ["Editor"]` and `"references": ["WM.Core"]`.
    - All 11 non-Core, non-Editor production asmdefs reference `WM.Core` (and the additional refs per the wiring matrix in `<interfaces>`).
    - All 4 service interfaces compile under namespace `WM.Core`.
    - All 4 `Null*Service` stubs compile and implement their corresponding `I*Service` (grep-verifiable).
    - `Assets/_Project/Scripts/Editor/BuildScript.cs` declares namespace `WM.Editor`, contains a public static `BuildIOS()` method that calls `BuildPipeline.BuildPlayer` and `EditorApplication.Exit(1)` on non-Succeeded result.
    - Editor Console shows 0 compile errors after this task.
  </acceptance_criteria>
</task>

<task type="auto" tdd="true">
  <name>Task 3: Wave-0 test scaffolding — EditMode + PlayMode test asmdefs and BootstrapStructureTests / PlayModeSmokeTests</name>
  <files>
    Assets/Tests/EditMode/WM.Tests.EditMode.asmdef,
    Assets/Tests/EditMode/BootstrapStructureTests.cs,
    Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef,
    Assets/Tests/PlayMode/PlayModeSmokeTests.cs
  </files>
  <read_first>
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-RESEARCH.md §Pattern 1 (test asmdef refs), §Code Examples (EditMode + PlayMode smoke tests), §Common Pitfalls 5
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-VALIDATION.md (entire file — Wave 0 requirements + Per-Task Verification Map)
    - .planning/phases/01-project-bootstrap-empty-warehouse-scene/01-CONTEXT.md (D-06)
  </read_first>
  <behavior>
    - Test BootstrapStructureTests.AssetsProject_FoldersExist: every required folder under Assets/_Project/ resolves via AssetDatabase.IsValidFolder (Art, Audio, Materials, Materials/Placeholder, Prefabs, Scenes, ScriptableObjects/{Boxes,Orders,Upgrades,Workers,Economy}, Scripts/{Core,Player,Boxes,Orders,Stations,Upgrades,Workers,Economy,Save,Analytics,Monetization,UI,Editor}, Settings).
    - Test BootstrapStructureTests.AsmdefsPresent: parameterized over the 13 production asmdef names; each File.Exists check passes.
    - Test BootstrapStructureTests.UrpAssetAssigned: GraphicsSettings.defaultRenderPipeline is not null AND its `name` contains "URP_Mobile".
    - Test PlayModeSmokeTests.SmokePass: a single trivial assertion that proves PlayMode test assembly compiles and runs (the scene-loading smoke test is added in plan 01-02 because the scene doesn't yet exist).
    - All three EditMode tests pass via `<UNITY> -batchmode -runTests -testPlatform EditMode` exit 0.
    - PlayMode trivial smoke test passes via `<UNITY> -batchmode -runTests -testPlatform PlayMode` exit 0.
  </behavior>
  <action>
**Step A — Test asmdefs.**

Create folder `Assets/Tests/EditMode/` and `Assets/Tests/PlayMode/` (the `Tests` root folder lives at `Assets/Tests/` per RESEARCH §Recommended Project Structure — separate from `_Project/` so test code stays visually distinct from production).

`Assets/Tests/EditMode/WM.Tests.EditMode.asmdef` — Editor platform only, "Test Assemblies" toggle ON (Pitfall 5). Use this exact JSON:
```json
{
    "name": "WM.Tests.EditMode",
    "rootNamespace": "WM.Tests.EditMode",
    "references": [
        "WM.Core",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": ["nunit.framework.dll"],
    "autoReferenced": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

`Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` — Any Platform, "Test Assemblies" toggle ON, MUST NOT reference `UnityEditor.TestRunner` (RESEARCH Pattern 1). Use this exact JSON:
```json
{
    "name": "WM.Tests.PlayMode",
    "rootNamespace": "WM.Tests.PlayMode",
    "references": [
        "WM.Core",
        "UnityEngine.TestRunner"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": ["nunit.framework.dll"],
    "autoReferenced": false,
    "defineConstraints": ["UNITY_INCLUDE_TESTS"],
    "versionDefines": [],
    "noEngineReferences": false
}
```

> The `defineConstraints: ["UNITY_INCLUDE_TESTS"]` + `overrideReferences: true` + explicit `precompiledReferences: ["nunit.framework.dll"]` is the modern equivalent of the legacy "Test Assemblies" toggle (Pitfall 5). Verify in Inspector after import: the asmdef should be marked as a Test Assembly and excluded from non-test player builds.

**Step B — `BootstrapStructureTests.cs`** (covers BOOT-01 — folder structure + asmdef presence + URP asset assignment per VALIDATION.md row 01-01-* / wave 0). Use this exact source:

`Assets/Tests/EditMode/BootstrapStructureTests.cs`:
```csharp
// Source: VALIDATION.md Wave 0 row 01-01-* + RESEARCH Code Examples
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
        // ---- BOOT-01: Assets/_Project/ folder skeleton ----
        private static readonly string[] RequiredFolders =
        {
            "Assets/_Project",
            "Assets/_Project/Art",
            "Assets/_Project/Audio",
            "Assets/_Project/Materials",
            "Assets/_Project/Materials/Placeholder",
            "Assets/_Project/Prefabs",
            "Assets/_Project/Scenes",
            "Assets/_Project/ScriptableObjects",
            "Assets/_Project/ScriptableObjects/Boxes",
            "Assets/_Project/ScriptableObjects/Orders",
            "Assets/_Project/ScriptableObjects/Upgrades",
            "Assets/_Project/ScriptableObjects/Workers",
            "Assets/_Project/ScriptableObjects/Economy",
            "Assets/_Project/Scripts",
            "Assets/_Project/Scripts/Core",
            "Assets/_Project/Scripts/Player",
            "Assets/_Project/Scripts/Boxes",
            "Assets/_Project/Scripts/Orders",
            "Assets/_Project/Scripts/Stations",
            "Assets/_Project/Scripts/Upgrades",
            "Assets/_Project/Scripts/Workers",
            "Assets/_Project/Scripts/Economy",
            "Assets/_Project/Scripts/Save",
            "Assets/_Project/Scripts/Analytics",
            "Assets/_Project/Scripts/Monetization",
            "Assets/_Project/Scripts/UI",
            "Assets/_Project/Scripts/Editor",
            "Assets/_Project/Settings"
        };

        [TestCaseSource(nameof(RequiredFolders))]
        public void AssetsProject_FoldersExist(string path)
        {
            Assert.That(AssetDatabase.IsValidFolder(path), Is.True,
                $"Required _Project folder missing: {path}");
        }

        // ---- BOOT-01: 13 production asmdefs present ----
        private static readonly string[] ProductionAsmdefs =
        {
            "Assets/_Project/Scripts/Core/WM.Core.asmdef",
            "Assets/_Project/Scripts/Player/WM.Player.asmdef",
            "Assets/_Project/Scripts/Boxes/WM.Boxes.asmdef",
            "Assets/_Project/Scripts/Orders/WM.Orders.asmdef",
            "Assets/_Project/Scripts/Stations/WM.Stations.asmdef",
            "Assets/_Project/Scripts/Upgrades/WM.Upgrades.asmdef",
            "Assets/_Project/Scripts/Workers/WM.Workers.asmdef",
            "Assets/_Project/Scripts/Economy/WM.Economy.asmdef",
            "Assets/_Project/Scripts/Save/WM.Save.asmdef",
            "Assets/_Project/Scripts/Analytics/WM.Analytics.asmdef",
            "Assets/_Project/Scripts/Monetization/WM.Monetization.asmdef",
            "Assets/_Project/Scripts/UI/WM.UI.asmdef",
            "Assets/_Project/Scripts/Editor/WM.Editor.asmdef"
        };

        [TestCaseSource(nameof(ProductionAsmdefs))]
        public void AsmdefsPresent(string path)
        {
            Assert.That(File.Exists(path), Is.True,
                $"Required asmdef missing: {path}");
        }

        // ---- BOOT-01: URP_Mobile is the active render pipeline ----
        [Test]
        public void UrpAssetAssigned()
        {
            RenderPipelineAsset rp = GraphicsSettings.defaultRenderPipeline;
            Assert.That(rp, Is.Not.Null, "GraphicsSettings.defaultRenderPipeline is null — URP not assigned");
            Assert.That(rp.name, Does.Contain("URP_Mobile"),
                $"Active render pipeline asset name was '{rp.name}', expected to contain 'URP_Mobile'");
        }
    }
}
```

**Step C — `PlayModeSmokeTests.cs` skeleton** (covers BOOT-02 runtime gate; the scene-loading body is filled in by plan 01-02 — for now we ship a passing smoke test so CLI test runs return 0). Use this exact source:

`Assets/Tests/PlayMode/PlayModeSmokeTests.cs`:
```csharp
// Source: VALIDATION.md Wave 0 row 01-02-* + RESEARCH Code Examples (PlayMode smoke).
// Plan 01-02 will append a Scene_Loads_GameManagerInitializes test that opens Warehouse_MVP.unity.
// In Phase 1 wave 1 we ship a trivial passing test so the PlayMode CLI returns exit 0.
using NUnit.Framework;
using UnityEngine;

namespace WM.Tests.PlayMode
{
    public class PlayModeSmokeTests
    {
        [Test]
        public void PlayModeAssembly_Compiles()
        {
            // Trivial assertion proves the test assembly was built and is runnable.
            // Real scene-load smoke test is added in plan 01-02 once Warehouse_MVP.unity exists.
            Assert.That(Application.isPlaying || !Application.isPlaying, Is.True);
        }
    }
}
```

**Step D — Run the test suite from CLI** (replace `<UNITY>` with the absolute path to the 6000.3.7f1 editor binary, e.g. `/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity`):

```
<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -
<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -
```

Both must exit 0.

> Why we do this from `Assets/Tests/` (not `Assets/_Project/Scripts/Tests/`): RESEARCH §Recommended Project Structure note — both locations work; `Assets/Tests/` is the common convention because it visually separates production code from tests and the asmdef references resolve correctly either way.
  </action>
  <verify>
    <automated>
# Test asmdefs present and configured
test -f Assets/Tests/EditMode/WM.Tests.EditMode.asmdef
test -f Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef

# EditMode asmdef references must include UnityEditor.TestRunner
grep -q '"UnityEditor.TestRunner"' Assets/Tests/EditMode/WM.Tests.EditMode.asmdef
# PlayMode asmdef references must NOT include UnityEditor.TestRunner (Pitfall 5 / Pattern 1)
! grep -q '"UnityEditor.TestRunner"' Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef
# Both must declare nunit.framework as a precompiled reference
grep -q '"nunit.framework.dll"' Assets/Tests/EditMode/WM.Tests.EditMode.asmdef
grep -q '"nunit.framework.dll"' Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef
# Both must be flagged as Test Assemblies via UNITY_INCLUDE_TESTS define constraint
grep -q '"UNITY_INCLUDE_TESTS"' Assets/Tests/EditMode/WM.Tests.EditMode.asmdef
grep -q '"UNITY_INCLUDE_TESTS"' Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef

# Test source files present with required class names
grep -q 'class BootstrapStructureTests' Assets/Tests/EditMode/BootstrapStructureTests.cs
grep -q 'public void AssetsProject_FoldersExist' Assets/Tests/EditMode/BootstrapStructureTests.cs
grep -q 'public void AsmdefsPresent' Assets/Tests/EditMode/BootstrapStructureTests.cs
grep -q 'public void UrpAssetAssigned' Assets/Tests/EditMode/BootstrapStructureTests.cs
grep -q 'class PlayModeSmokeTests' Assets/Tests/PlayMode/PlayModeSmokeTests.cs

# CLI test gate (replace UNITY with the absolute editor binary path).
# UNITY=/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity
# "$UNITY" -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -
# "$UNITY" -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -
# Both must exit 0. Estimated runtime ~30s EditMode, ~60s PlayMode.
echo "Run the EditMode + PlayMode CLI commands above; both must exit 0."
    </automated>
  </verify>
  <done>
    Both test asmdef JSON files exist with the exact references / platforms / define constraints above. `BootstrapStructureTests.cs` and `PlayModeSmokeTests.cs` compile. `<UNITY> -batchmode -runTests -testPlatform EditMode` exits 0 with `BootstrapStructureTests.AssetsProject_FoldersExist` (one case per required folder), `BootstrapStructureTests.AsmdefsPresent` (one case per production asmdef), and `BootstrapStructureTests.UrpAssetAssigned` all green. `<UNITY> -batchmode -runTests -testPlatform PlayMode` exits 0 with `PlayModeSmokeTests.PlayModeAssembly_Compiles` green. The PlayMode scene-load test will be added in plan 01-02; this plan ships a trivial passing PlayMode test so CI is meaningful from day one.
  </done>
  <acceptance_criteria>
    - File `Assets/Tests/EditMode/WM.Tests.EditMode.asmdef` contains `"includePlatforms": ["Editor"]`, references `WM.Core`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, declares `nunit.framework.dll` as precompiled reference, has `UNITY_INCLUDE_TESTS` in `defineConstraints`, and `overrideReferences: true`.
    - File `Assets/Tests/PlayMode/WM.Tests.PlayMode.asmdef` does NOT contain the string `UnityEditor.TestRunner`, otherwise has the same precompiled and define-constraint configuration.
    - `Assets/Tests/EditMode/BootstrapStructureTests.cs` declares class `BootstrapStructureTests` with three test methods: `AssetsProject_FoldersExist(string path)`, `AsmdefsPresent(string path)`, `UrpAssetAssigned()`.
    - `Assets/Tests/PlayMode/PlayModeSmokeTests.cs` declares class `PlayModeSmokeTests` with at least one passing `[Test]`.
    - `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -` exits 0.
    - `<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -` exits 0.
    - Player build (when invoked via `BuildScript.BuildIOS`) does NOT pull in the test assemblies (verify by inspecting the `WM.Tests.*` asmdefs are flagged as Test Assemblies in the Inspector — they should show "Used by tests only" badge).
  </acceptance_criteria>
</task>

</tasks>

<threat_model>
## Trust Boundaries

| Boundary | Description |
|----------|-------------|
| Repo → public git history | `.gitignore` and `.gitattributes` guard against secrets and binary corruption leaking into commits |
| Editor → CLI build (`-batchmode -executeMethod`) | `BuildScript.BuildIOS` exit code is the only signal CI gets — must propagate failure |
| Repo → Build artifacts (`build/ios/`) | Build outputs must never be committed |

## STRIDE Threat Register

| Threat ID | Category | Component | Disposition | Mitigation Plan |
|-----------|----------|-----------|-------------|-----------------|
| T-01-01 | Spoofing / Tampering | iOS Player Settings (Bundle Identifier) | mitigate | Set `PlayerSettings.applicationIdentifier = "com.warehousemaster.mvp"` in Task 1 Step D before any signed build. Default `com.Company.ProductName` collides with App Store reservations and Apple support holds. |
| T-01-02 | Spoofing / Information Disclosure | Repo (signing certs, provisioning profiles) | mitigate | `.gitignore` excludes `*.mobileprovision`, `*.p12`, `*.cer`, `*.pem`, `*.certSigningRequest` (Task 1 Step G). Final signing happens in Phase 11 only. |
| T-01-03 | Tampering | Asset graph integrity (.meta and YAML scenes/prefabs/materials) | mitigate | `.gitattributes` (Task 1 Step H) keeps `.meta`, `.unity`, `.prefab`, `.mat`, `.anim`, `.controller` as `merge=unityyamlmerge eol=lf` — explicitly NOT in LFS. D-20 deviation approved by user 2026-05-10 (RESEARCH OQ-1 RESOLVED); documented in commit message and SKELETON.md. |
| T-01-04 | Information Disclosure | Build artifacts (`build/ios/`) | mitigate | `.gitignore` excludes `/build/` and `/build/ios/` (Task 1 Step G). `BuildScript.cs` writes to that path so it stays untracked. |

All four threats are MEDIUM/LOW severity (build-hygiene). No HIGH-severity threats in Phase 1 because there is no runtime auth, no network, no persistence, no PII surface yet.
</threat_model>

<verification>
**Phase-level checks for plan 01-01:**

1. **Folder + package skeleton:** `BootstrapStructureTests.AssetsProject_FoldersExist` (parameterized over 28 folder paths) green. `Packages/manifest.json` contains the four required package keys (grep).
2. **Asmdef topology:** `BootstrapStructureTests.AsmdefsPresent` (parameterized over 13 production asmdefs) green.
3. **URP active:** `BootstrapStructureTests.UrpAssetAssigned` green — `GraphicsSettings.defaultRenderPipeline.name` contains "URP_Mobile".
4. **Test assemblies functional:** Both EditMode and PlayMode test asmdefs configured per RESEARCH Pattern 1; `<UNITY> -runTests -testPlatform {EditMode,PlayMode}` both exit 0.
5. **Service stubs compile:** Editor Console green; `Null*Service : I*Service` grep matches present in all four stubs.
6. **BuildScript skeleton:** `WM.Editor.BuildScript.BuildIOS` exists and contains `BuildPipeline.BuildPlayer` + `EditorApplication.Exit(1)`. Manual smoke (run before plan completes) in `01-VERIFICATION.md`: invoke from CLI; failure case (scene not yet present) exits 1; success case will be re-verified after plan 01-02.
7. **Repo hygiene:** `.gitignore` excludes Library/, Temp/, Logs/, build/, signing certs (grep). `.gitattributes` D-20 deviation per OQ-1 RESOLVED (no LFS on `*.unity`/`*.prefab`).
8. **Player Settings serialized:** `ProjectSettings/ProjectSettings.asset` grep gates pass for IL2CPP scripting backend on iPhone, Metal-only Graphics API block, iOSTargetOSVersionString = 15, activeInputHandler = 1, applicationIdentifier = `com.warehousemaster.mvp`, defaultScreenOrientation = 0, m_ActiveColorSpace = 1 — OR executor-validated equivalents recorded in `01-01-SUMMARY.md`.
</verification>

<success_criteria>
- Editor opens with **0 compile errors**.
- 13 production asmdefs + 2 test asmdefs all recognized in the Inspector with the wiring matrix from `<interfaces>`.
- `<UNITY> -batchmode -runTests -testPlatform EditMode` exits 0 with `BootstrapStructureTests.*` green (folders + asmdefs + URP).
- `<UNITY> -batchmode -runTests -testPlatform PlayMode` exits 0 with `PlayModeSmokeTests.PlayModeAssembly_Compiles` green.
- `git status` shows tracked: `ProjectSettings/`, `Packages/manifest.json`, `Packages/packages-lock.json`, `Assets/_Project/`, `Assets/Tests/`, `.gitignore`, `.gitattributes`. `Library/`, `Temp/`, `Logs/`, `build/` are untracked.
- `BuildScript.BuildIOS` is invokable via `-executeMethod WM.Editor.BuildScript.BuildIOS` (full success requires plan 01-02 — the Warehouse_MVP scene — to exist; this plan's gate is "the build script compiles and is reachable").
- `ProjectSettings/ProjectSettings.asset` grep gates pass — IL2CPP / Metal / iOS 15 / New Input / Bundle Id / Portrait / Linear (or executor-validated equivalents recorded in `01-01-SUMMARY.md`).
</success_criteria>

<output>
After completion, create `.planning/phases/01-project-bootstrap-empty-warehouse-scene/01-01-SUMMARY.md` recording: the resolved Unity sub-version (in case it differs from 6000.3.7f1), the resolved Cinemachine and Input System versions from `Packages/packages-lock.json`, the URP version that shipped with the editor, the `.gitattributes` content per OQ-1 RESOLVED, **the actual Player Settings serialized YAML keys** found in `ProjectSettings/ProjectSettings.asset` after Step D (with any deviation from the best-effort grep patterns in Task 1 verify block — record the exact pattern that worked so plan-checker and downstream phases inherit a verified gate), and any Editor warnings encountered (must be 0 errors).
</output>
</content>
</invoke>