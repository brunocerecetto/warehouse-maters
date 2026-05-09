# Phase 11: TestFlight Build Pipeline - Context

**Gathered:** 2026-05-09 (--auto mode)
**Status:** Ready for planning

<domain>
## Phase Boundary

Flesh out the `BuildScript` skeleton from Phase 1 D-18 into a working CLI build entry point that produces a TestFlight-ready Xcode project. Finalize Player Settings (bundle id, version, signing scaffold, icons, splash). Document the manual archive + upload steps in `build/README.md`. CI / GitHub Actions explicitly out of scope for MVP.

Covers requirements: **BUILD-01**.

</domain>

<decisions>
## Implementation Decisions

### Build Script Entry Point
- **D-01:** **`BuildScript.BuildIOS` static method** in `WM.Editor` (already at `Assets/_Project/Scripts/Editor/BuildScript.cs` from Phase 1 D-18). Public static, no parameters — Unity's `-executeMethod` requires that signature. Reads command-line args via `Environment.GetCommandLineArgs()`:
  - `-buildOutput <path>` — defaults to `build/ios/`.
  - `-bundleVersion <version>` — defaults to `Application.version` (the value in Player Settings).
  - `-buildNumber <int>` — defaults to current `PlayerSettings.iOS.buildNumber`, incremented if not provided.
  - `-development` — flag, enables development build (Debug.Log + script debugging).
- **D-02:** **Exit codes:** `EditorApplication.Exit(0)` on success, `EditorApplication.Exit(1)` on failure. Catches `BuildPipeline.BuildPlayer` failure via `BuildSummary.result == BuildResult.Failed`.
- **D-03:** **Logging:** prints structured lines `[Build] {step}` so log-scrapers can parse. Final line: `[Build] OK output=<path>` or `[Build] FAIL reason=<msg>`.

### Output Directory
- **D-04:** **`build/ios/` at repo root.** Listed in `.gitignore` (Phase 1 D-19 already covers `Build/` — extend to `build/`). Each invocation wipes & recreates this dir before BuildPipeline writes the Xcode project. Predictable for downstream Xcode workflows.
- **D-05:** **`build/README.md`** documents:
  - Manual archive + upload steps (Xcode Organizer or `xcodebuild -exportArchive`).
  - Bundle id placeholder (`com.warehousemaster.mvp`) and how to change.
  - Code signing setup (Manual signing for prototype; Apple ID + provisioning profile required).
  - Minimum supported Xcode version (matches Unity 6 LTS iOS export requirements; planner verifies).

### Player Settings Finalization
- **D-06:** **PlayerSettings configuration in `BuildScript`** (programmatic, idempotent — re-applies on every build):
  - `companyName = "Warehouse Master"`.
  - `productName = "Warehouse Master"`.
  - `bundleVersion` = arg or current.
  - `iOS.buildNumber` = arg or auto-increment.
  - `applicationIdentifier` = `com.warehousemaster.mvp` (placeholder confirmed Phase 1 D-17).
  - `iOS.targetOSVersionString = "15.0"` (Phase 1 D-17).
  - Orientation: portrait only (Phase 1 D-17 confirmed).
  - Scripting backend: IL2CPP (Phase 1 D-17 confirmed).
  - Architecture: ARM64 (Phase 1 D-17 confirmed).
  - Graphics API: Metal only (Phase 1 D-17 confirmed).
  - `iOS.appInBackgroundBehavior = Suspend` (default; analytics needs no background).
  - `iOS.requiresPersistentWiFi = false`.
  - `iOS.allowHTTPDownload = false` (no HTTP for MVP).
- **D-07:** **App icons** at `Assets/_Project/Art/AppIcon/`. Provide a 1024×1024 master + iOS app-icon set (Unity auto-generates required sizes from Player Settings). MVP placeholder: solid color tile with "WM" text. Final art outside MVP scope per `PROJECT.md`.
- **D-08:** **Splash screen:** disabled if Personal license forces "Made with Unity" splash (acceptable for prototype); else show single Warehouse Master logo splash. `BuildScript` doesn't toggle this — set in Editor; documented in `build/README.md`.

### Code Signing
- **D-09:** **Manual signing only.** `BuildScript` does NOT handle provisioning profiles, certificates, or `xcodebuild` archive — those steps happen in Xcode after Unity exports. Documented in `build/README.md`. Auto-signing/CI deferred.
- **D-10:** **`AppleDeveloperTeamID`** is empty in source-controlled Player Settings (set per-developer locally, gitignored override mechanism if needed; for MVP, accept that local value-set is required after fresh clone).

### Capabilities & Frameworks
- **D-11:** **Minimal capabilities.** No Push, no Background Modes, no In-App Purchase capability (MVP fakes IAP), no Sign in with Apple. Each can be toggled later via `XcodePostProcessBuild` callbacks.
- **D-12:** **No SKAdNetwork integration.** No `Info.plist` SKAN entries. Real ads (Phase 12+) require these — out of MVP scope.
- **D-13:** **No App Tracking Transparency (ATT) framework.** No `NSUserTrackingUsageDescription` key. Real ads with personalized targeting need this; MVP doesn't.
- **D-14:** **Required `Info.plist` entries** added via `XcodePostProcessBuild`:
  - `NSAppTransportSecurity` — default secure (no exceptions).
  - `UIRequiresFullScreen = YES` (portrait-only convenience).
  - Disable iPad multitasking if needed (`UISupportedInterfaceOrientations` portrait-only).

### Build Validation Pre-Flight
- **D-15:** **Pre-build assertions** in `BuildScript` (fail-fast before invoking `BuildPipeline.BuildPlayer`):
  - `Warehouse_MVP` scene exists in `EditorBuildSettings.scenes` and is enabled.
  - `1080×1920` reference resolution + `SafeAreaPanel` exist (Phase 1 D-15).
  - `Bootstrap` GameObject exists in scene root.
  - `GameManager` GameObject exists with `OrderManager`, `UpgradeManager`, `SaveManager`, `TutorialController`, `FeedbackService`, `SessionTracker` components attached.
  - At least one `OrderTemplate`, six `UpgradeDef`, three `BoxType` SOs exist (per phase invariants).
  Validation logs include offenders; one failed assertion → exit 1.

### Smoke Run Documentation
- **D-16:** **Smoke run command (in `build/README.md`):**
  ```bash
  /Applications/Unity/Hub/Editor/<VERSION>/Unity.app/Contents/MacOS/Unity \
    -batchmode -quit -projectPath . -buildTarget iOS \
    -executeMethod BuildScript.BuildIOS \
    -logFile build/ios/build.log
  open build/ios/Unity-iPhone.xcodeproj
  ```
  Plus Xcode steps: Select team, Archive, Validate, Distribute App → TestFlight.

### Asmdef Layout
- **D-17:** **`WM.Editor`** asmdef (Phase 1 D-04, editor-only via `includePlatforms = [Editor]`) hosts `BuildScript.cs` + new `BuildPostProcessor.cs` (the `XcodePostProcessBuild` callback for Info.plist edits) + `BuildAssertions.cs`. Stays editor-only — never ships in player binary.

### Versioning
- **D-18:** **`Application.version` = "0.1.0"** for first TestFlight build. Manual bump for each TestFlight upload (semver patch for fixes; minor for content). `iOS.buildNumber` auto-increments.

### Claude's Discretion
- D-04 wipe-and-recreate output dir — chosen over incremental builds. Predictable + simple; rebuild time on iOS is dominated by IL2CPP not Unity-side.
- D-07 placeholder icon ("WM" text) — final art outside MVP scope.
- D-10 `AppleDeveloperTeamID` per-developer-local — easiest path; CI later.
- D-15 pre-flight assertions list — chosen invariants are the load-bearing ones; other phases' invariants checked only via integration test pass at runtime.
- D-18 version "0.1.0" — placeholder; user can override at build time.
- All five gray areas auto-resolved with recommended defaults.

</decisions>

<canonical_refs>
## Canonical References

### Project Source of Truth
- `CLAUDE.md` — TestFlight-ready iOS build; CLI build script with `-executeMethod BuildScript.BuildIOS`.
- `.planning/PROJECT.md` — TestFlight-ready iOS build; internal validation, not App Store release.
- `.planning/REQUIREMENTS.md` — BUILD-01 (CLI batch build, Xcode archive uploads cleanly).
- `.planning/ROADMAP.md` — Phase 11 goal, success criteria, plan stubs (11-01 BuildScript editor class, 11-02 player settings + bundle id + signing, 11-03 docs + smoke run).

### Prior Phase Substrate
- `.planning/phases/01-...` — `BuildScript` skeleton (D-18); IL2CPP / ARM64 / Metal / iOS 15+ / portrait-only (D-17); `.gitignore` extension (D-19); `WM.Editor` asmdef (D-04); 1080×1920 SafeAreaPanel (D-15).
- All other phase CONTEXT.md files — their components must exist for D-15 pre-flight assertions to pass.

### Architecture & Spec
- `specs/06-technical-architecture-spec.md` §2 — iOS first; URP; Metal; IL2CPP; ARM64.
- `specs/06-technical-architecture-spec.md` §3 — Folder structure (`Editor/` for build script).
- `specs/01-product-requirements-spec.md` — Target platform iPhone portrait.
- `specs/07-mvp-backlog-acceptance-criteria.md` — Build-related WM items.

### Reference (non-binding)
- Apple TestFlight documentation (planner reads at implementation time, not committed).
- Unity manual: `BuildPipeline.BuildPlayer`, `XcodePostProcessBuild`.
- `warehouse_master_plan.md`.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets (paper-locked)
- `BuildScript.BuildIOS` skeleton from Phase 1 D-18 — Phase 11 fleshes out.
- All system invariants (GameManager components, SOs, scene composition) — assertable by `BuildAssertions.cs`.
- `.gitignore` already covers `Build/`; Phase 11 extends to `build/`.

### Established Patterns
- **`WM.Editor` asmdef** (Phase 1 D-04) editor-only — Phase 11 keeps additions there.
- **Spec-driven Player Settings** (Phase 1 D-17) — Phase 11 only re-asserts; doesn't change.
- **Scriptable, idempotent build** — `BuildScript` re-applies Player Settings on every invocation.

### Integration Points
- `Assets/_Project/Scripts/Editor/BuildScript.cs` — Phase 11 expands.
- `Assets/_Project/Scripts/Editor/BuildPostProcessor.cs` — new this phase.
- `Assets/_Project/Scripts/Editor/BuildAssertions.cs` — new this phase.
- `Assets/_Project/Art/AppIcon/` — new this phase (placeholder icon set).
- `build/README.md` — new this phase.
- `.gitignore` — Phase 11 extends.

</code_context>

<specifics>
## Specific Ideas

- **Pre-flight assertions are the cheapest CI substitute.** Without GitHub Actions, this phase needs the build itself to enforce invariants. Cheap to add; catches "hey, scene is missing GameManager" before a 5-minute IL2CPP build wastes time.
- **Manual signing is correct for MVP.** Auto-signing requires storing the team ID in source — fine for solo prototype, problematic if anyone else clones. Defer to soft launch.
- **No SKAN, no ATT** is deliberate scope discipline. Real ads land later; the MVP build target is "show the warehouse to TestFlight reviewers."
- **`build/` not `Build/`** for output dir — lowercase, distinct from Unity's auto-generated `Build/` (player binary cache). Avoids confusion when grepping logs.

</specifics>

<deferred>
## Deferred Ideas

- **GitHub Actions / Jenkins / CI matrix** — out of MVP. Build runs on developer machine.
- **Auto-signing + provisioning profile management** — soft launch.
- **`xcodebuild archive + exportArchive` automation** — soft launch (current MVP requires Xcode UI step).
- **Fastlane integration** — soft launch.
- **TestFlight upload via `xcrun altool` / Transporter CLI** — soft launch.
- **Multi-build configurations (Debug, Release, AppStore)** — soft launch.
- **Build-version bump automation** — manual for MVP.
- **Crash reporting (Bugsnag, Sentry, Firebase Crashlytics)** — soft launch.
- **Symbol upload for crash de-symbolication** — soft launch.
- **App Store Connect API integration** — soft launch.
- **Privacy nutrition labels** — soft launch (real ads / IAP land then).
- **App Privacy Report (`PrivacyInfo.xcprivacy`)** — soft launch.
- **Final app icon and splash art** — outside MVP art scope.
- **Localized App Store metadata** — soft launch.
- **iPad-specific layout / TestFlight target** — out of MVP (iPhone only).
- **App thinning / on-demand resources** — out of MVP.
- **Bitcode** — Apple deprecated; not needed.

</deferred>

---

*Phase: 11-testflight-build-pipeline*
*Context gathered: 2026-05-09 (--auto)*
