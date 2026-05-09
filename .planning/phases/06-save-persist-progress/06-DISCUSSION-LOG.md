# Phase 6: Save & Persist Progress - Discussion Log

> **Audit trail only.** Generated in --auto mode (recommended defaults).

**Date:** 2026-05-09
**Phase:** 6-save-persist-progress
**Mode:** --auto
**Areas auto-resolved:** Storage backend, Schema versioning + migration, Autosave triggers, Reset hook delivery, Threading

---

## Storage backend

| Option | Description | Selected |
|---|---|---|
| JSON file at `Application.persistentDataPath/save.json` | Inspectable; future-friendly migration. | ✓ |
| `PlayerPrefs` wrapper | Opaque; not idiomatic for structured data. | |
| SQLite via Mono.Data.Sqlite | Overkill for MVP; adds package weight. | |

**[auto] Selected:** JSON file.

## Schema versioning + migration strategy

| Option | Description | Selected |
|---|---|---|
| `int version` first; forward-only migration chain `Migrate_N_to_M` | Standard pattern; aligns with spec §6 schema. | ✓ |
| No versioning; reset save on schema change | Loses player progress on every update. | |
| Hash-based schema check | Opaque; doesn't help migrate. | |

**[auto] Selected:** Forward-only migration chain.

## Autosave triggers

| Option | Description | Selected |
|---|---|---|
| Subscribe to currency / upgrade / tutorial / app-pause/quit, debounced 1s | Captures all meaningful state changes; debounce coalesces bursts. | ✓ |
| Periodic timer (e.g., every 30s) | Misses rapid events; can lose data on crash. | |
| Manual save buttons only | Bad UX for hybrid-casual; spec demands persistence. | |

**[auto] Selected:** Event-driven debounced autosave.

## Reset hook delivery

| Option | Description | Selected |
|---|---|---|
| `[ContextMenu]` + debug-build-only HUD button | Editor + on-device debug; safe for production builds. | ✓ |
| Hidden 7-tap gesture | Cute but unnecessary for MVP. | |
| Always-visible reset button | Risk: player wipes save accidentally. | |

**[auto] Selected:** ContextMenu + debug-build button.

## Threading

| Option | Description | Selected |
|---|---|---|
| Async via `Task.Run` for File.WriteAllText | True off-main-thread. | ✓ |
| Synchronous on main thread | Risk: visible hitch on save. | |
| Coroutine | Runs on main thread; doesn't actually offload. | |

**[auto] Selected:** Task.Run async.

---

## Claude's Discretion

- Debounce window 1.0s — tunable.
- Reset-hook UI placement on `OrderHUD` corner — arbitrary; any debug menu acceptable.
- `Task.Run` vs coroutine — fall back to coroutine if iOS IL2CPP issues arise.

## Deferred Ideas

- Cloud save, encryption, multi-slot, export/import, migration testing harness, in-game confirm dialog reset, save-failure telemetry — out of MVP or post-prototype.
