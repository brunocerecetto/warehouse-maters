# Phase 11: TestFlight Build Pipeline - Discussion Log

> **Audit trail only.** Generated in --auto mode.

**Date:** 2026-05-09
**Phase:** 11-testflight-build-pipeline
**Mode:** --auto
**Areas auto-resolved:** Build script entry point, Output directory, Player Settings finalization, Documentation location, CI integration

---

## Build script entry point

| Option | Description | Selected |
|---|---|---|
| `BuildScript.BuildIOS` static method with command-line arg parsing | CLAUDE.md / spec-aligned; idempotent. | ✓ |
| `MenuItem` Editor command only | No CLI invocation. | |
| Standalone shell script invoking Unity | Adds external surface. | |

**[auto] Selected:** Static method with arg parsing.

## Output directory

| Option | Description | Selected |
|---|---|---|
| `build/ios/` at repo root, gitignored, wiped per build | Predictable; clean state. | ✓ |
| Incremental builds (no wipe) | Risk of stale artifacts. | |
| Per-build timestamped dir | Disk bloat. | |

**[auto] Selected:** `build/ios/` wipe-and-recreate.

## Player Settings finalization

| Option | Description | Selected |
|---|---|---|
| Programmatic in `BuildScript` (idempotent) | Re-applies on every build; CI-friendly later. | ✓ |
| Editor-only manual setup | Drift between developers. | |
| External JSON read by BuildScript | Premature config tooling. | |

**[auto] Selected:** Programmatic idempotent setup.

## Documentation location

| Option | Description | Selected |
|---|---|---|
| `build/README.md` at output-dir level | Co-located with the artifact. | ✓ |
| Top-level `BUILD.md` | Mixed with project root. | |
| `docs/build.md` | Adds new docs/ folder. | |

**[auto] Selected:** `build/README.md`.

## CI integration

| Option | Description | Selected |
|---|---|---|
| Out of scope for MVP; document smoke run only | Spec scope discipline. | ✓ |
| GitHub Actions matrix (macOS + iOS) | Cost + setup overhead. | |
| Jenkins on local Mac | Maintenance burden for solo prototype. | |

**[auto] Selected:** No CI in MVP.

---

## Claude's Discretion

- Wipe-and-recreate output dir over incremental.
- Placeholder app icon ("WM" text) — final art outside MVP scope.
- `AppleDeveloperTeamID` per-developer-local.
- Pre-flight assertions list — chosen invariants are load-bearing only.
- Version "0.1.0" placeholder; user override at build time.

## Deferred Ideas

- GitHub Actions / Jenkins, auto-signing, fastlane, `xcodebuild` automation, TestFlight CLI upload, multi-build configs, version-bump automation, crash reporting, symbol upload, App Store Connect API, privacy labels, `PrivacyInfo.xcprivacy`, final art, localized metadata, iPad target, app thinning, Bitcode — all soft launch / out of MVP.
