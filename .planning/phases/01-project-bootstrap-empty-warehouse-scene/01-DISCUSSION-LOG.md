# Phase 1: Project Bootstrap & Empty Warehouse Scene - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-05-09
**Phase:** 1-project-bootstrap-empty-warehouse-scene
**Areas discussed:** Unity version & URP profile, Input package choice, Asmdef granularity, GameManager bootstrap pattern

---

## Area Selection

| Option | Description | Selected |
|--------|-------------|----------|
| Unity version & URP profile | LTS choice + URP mobile preset | ✓ |
| Input package choice | New Input System vs legacy Input Manager | ✓ |
| Asmdef granularity | Per-system vs grouped vs none | ✓ |
| GameManager bootstrap pattern | Singleton vs SO locator vs DI vs composition root | ✓ |

**User's choice:** All four areas.

---

## Unity Version

| Option | Description | Selected |
|--------|-------------|----------|
| Unity 6 LTS (6000.x) | Latest LTS. RenderGraph URP, GPU Resident Drawer, better mobile perf | ✓ |
| Unity 2022.3 LTS | Mature plugin ecosystem, less iOS perf headroom | |
| Latest stable at init time | Defer choice to Unity Hub recommendation | |

**User's choice:** Unity 6 LTS (6000.x).

---

## URP Mobile Quality Preset

| Option | Description | Selected |
|--------|-------------|----------|
| Mobile-aggressive (60fps target) | MSAA off, no realtime shadows, HDR off, no post-FX. iPhone 8/SE class | ✓ |
| Mobile-balanced (60fps mid devices) | MSAA 2x, soft shadows, optional bloom. iPhone 11+ | |
| Mobile-quality (60fps newer devices) | MSAA 4x, realtime shadows + cascades, bloom + tonemapping | |

**User's choice:** Mobile-aggressive.

---

## Input Package

| Option | Description | Selected |
|--------|-------------|----------|
| New Input System (com.unity.inputsystem) | Action assets, multi-device, cleaner API. Slight setup overhead | ✓ |
| Legacy Input Manager | Built-in, simpler for touch-only MVP, limits future input expansion | |
| Both enabled | Active Input Handling = Both. Adds complexity | |

**User's choice:** New Input System.

---

## Asmdef Granularity

| Option | Description | Selected |
|--------|-------------|----------|
| One asmdef per system folder | `WM.Core`, `WM.Player`, `WM.Boxes`, ... + `WM.Editor`. Strict deps, fast incremental compile | ✓ |
| 3 grouped asmdefs | `WM.Gameplay`, `WM.Services`, `WM.UI`, `WM.Editor`. Less ceremony | |
| No asmdefs initially | Default Assembly-CSharp, add later | |

**User's choice:** One asmdef per system folder.

---

## Test Asmdefs

| Option | Description | Selected |
|--------|-------------|----------|
| Both EditMode + PlayMode test asmdefs scaffolded | UTF installed, smoke tests in each, CLI test runs work day one | ✓ |
| EditMode only | PlayMode added later when scene logic appears | |
| No tests scaffolded yet | Defer UTF setup | |

**User's choice:** Both scaffolded from day one.

---

## GameManager Bootstrap Pattern

| Option | Description | Selected |
|--------|-------------|----------|
| Plain singleton GameManager | MonoBehaviour, Inspector/Find wiring, hardest to test | |
| ScriptableObject service locator | SO registry, swap via assets, Unity-native | |
| DI container (VContainer) | Lifetime scopes, constructor injection, package dep | |
| Plain C# composition root | `Bootstrap.cs` news up services + Init(...) injection, no third-party | ✓ (Claude's pick) |

**User's choice:** "you decide" → Claude picked Plain C# composition root.
**Notes:** Rationale documented in CONTEXT.md D-07. Aligns with `specs/06-technical-architecture-spec.md` ("small MonoBehaviours + plain C# services") and `CLAUDE.md` warning against unspecified third-party packages. No hidden global state, no DI learning curve mid-MVP, services remain testable via constructor.

---

## Deferred-Items Confirmation

| Default | Confirmed |
|---------|-----------|
| Camera = Cinemachine virtual cam (fixed iso, 45° pitch placeholder) | ✓ |
| Placeholder geometry = Unity primitives + colored materials | ✓ |
| UI canvas = Screen Space - Camera, 1080×1920 ref, safe-area aware | ✓ |
| Build script stub in Editor asmdef from day 1 | ✓ |

**User's choice:** All defaults confirmed, no overrides.

---

## Claude's Discretion

- **D-07 — Service composition / bootstrap pattern** (Plain C# composition root). User said "you decide".
- D-12 (Cinemachine), D-13 (primitive geometry palette), D-15 (Screen Space Camera + 1080×1920 + safe area), D-17 (iOS 15.0 min, IL2CPP, ARM64, Metal-only), D-18 (BuildScript skeleton in Editor asmdef from day 1), D-19/D-20 (.gitignore + Git LFS rules) — Claude defaults presented and confirmed by user.

## Deferred Ideas

- Quality tier variants (low/mid/high URP profiles) — defer to polish phase if device testing surfaces headroom.
- Cinemachine follow target — Phase 2 (movement).
- Real bundle identifier + signing config — Phase 11.
- Localization scaffolding — not in MVP.
- Resources/ vs Addressables for prefab loading — decide when first runtime-loaded asset appears (likely Phase 3, but Inspector refs probably suffice).
