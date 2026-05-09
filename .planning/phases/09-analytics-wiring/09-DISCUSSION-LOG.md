# Phase 9: Analytics Wiring - Discussion Log

> **Audit trail only.** Generated in --auto mode.

**Date:** 2026-05-09
**Phase:** 9-analytics-wiring
**Mode:** --auto
**Areas auto-resolved:** Provider impl, Event emission pattern, Parameter validation, Failure isolation, Disable mechanism

---

## Provider implementation

| Option | Description | Selected |
|---|---|---|
| `DebugLogAnalyticsService` writing to Debug.Log | SDK-free; inspectable in Editor + iOS Console. | ✓ |
| Firebase Analytics for Unity | Adds SDK before validating loop. | |
| Both (debug + Firebase) | Premature lock-in; defeats wrapper-first stance. | |

**[auto] Selected:** DebugLogAnalyticsService.

## Event emission pattern

| Option | Description | Selected |
|---|---|---|
| Domain events → AnalyticsManager subscribes once and translates | Zero scattered SDK calls; matches PROJECT.md. | ✓ |
| Each system calls IAnalyticsService.Track directly | Scatters SDK; harder to audit. | |
| Static event bus | Contradicts no-singleton rule. | |

**[auto] Selected:** Domain events + single subscriber.

## Parameter validation

| Option | Description | Selected |
|---|---|---|
| Runtime per-event whitelist; warn + drop if missing | Catches drift; never throws to gameplay. | ✓ |
| Compile-time validation via codegen | Heavy tooling for MVP. | |
| No validation | Spec drift goes undetected. | |

**[auto] Selected:** Runtime whitelist.

## Failure isolation

| Option | Description | Selected |
|---|---|---|
| Try/catch inside Track wrapping the SDK call | Spec-aligned; gameplay never sees failures. | ✓ |
| No isolation; let exceptions propagate | Violates spec acceptance. | |
| Async fire-and-forget without exception handling | Loses visibility on failures. | |

**[auto] Selected:** Try/catch wrap.

## Disable mechanism

| Option | Description | Selected |
|---|---|---|
| AnalyticsConfig SO with `enabled` bool; Bootstrap swaps in NoOpAnalyticsService | Clean DI swap; no runtime branching. | ✓ |
| Boolean flag inside AnalyticsManager | Branching everywhere; verbose. | |
| Build-time #define | Less flexible. | |

**[auto] Selected:** SO config + DI swap.

---

## Claude's Discretion

- Session-scoped first-event flags (in-memory) — saves schema bump; duplicate first-events are tolerable for MVP.
- Per-event `Build_<event>` methods over a generic builder — explicit > clever.
- `session_ended` on `OnApplicationQuit` is best-effort on iOS.
- Single `WM.Analytics` asmdef.

## Deferred Ideas

- Firebase, AppsFlyer, GameAnalytics SDKs (soft launch); persisted first-event flags; event batching; A/B integration; richer user properties; ATT/consent UI; `area_unlocked` event (no MVP feature for it); CI validation harness; runtime QA toggle.
