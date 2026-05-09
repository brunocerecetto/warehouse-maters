# Phase 8: Tutorial & Feel - Discussion Log

> **Audit trail only.** Generated in --auto mode.

**Date:** 2026-05-09
**Phase:** 8-tutorial-feel
**Mode:** --auto
**Areas auto-resolved:** Tutorial step framework, Indicator UI, Step trigger detection, Feedback effect choice, Tutorial state persistence + reset

---

## Tutorial step framework

| Option | Description | Selected |
|---|---|---|
| TutorialStep + TutorialFlow ScriptableObjects | Data-driven, designer-tunable, matches spec §11. | ✓ |
| Hardcoded step list in TutorialController | No tuning surface. | |
| External JSON / config file | Loses Inspector integration. | |

**[auto] Selected:** SO-driven step framework.

## Indicator UI

| Option | Description | Selected |
|---|---|---|
| World-space arrow over target + HUD bubble with text | Spatial + textual; covers visual readability. | ✓ |
| HUD-only directional indicator | No spatial cue. | |
| Minimap with highlight | Adds minimap surface for one phase. | |

**[auto] Selected:** Arrow + bubble combo.

## Step trigger detection

| Option | Description | Selected |
|---|---|---|
| Subscribe to existing domain events (OnStackChanged, OnPackingCompleted, OnDelivered, OnLevelChanged) + lightweight per-station enter triggers | Reuses prior-phase events; no polling. | ✓ |
| Polling (Update checks state every frame) | Wasteful; fragile. | |
| Static event bus | Contradicts no-singleton rule. | |

**[auto] Selected:** Event-subscription.

## Feedback effect choice

| Option | Description | Selected |
|---|---|---|
| Unity ParticleSystem prefabs + AudioClips behind IFeedbackService, pooled | SDK-free; isolated seam; pool-friendly. | ✓ |
| FMOD/Wwise integration | Adds 3rd-party dep before validating loop. | |
| Custom shader-based effects | Engineering cost > MVP value. | |

**[auto] Selected:** Particle + audio pooled behind IFeedbackService.

## Tutorial state persistence + reset

| Option | Description | Selected |
|---|---|---|
| Save Phase 6's tutorialCompleted bool; reset via SaveManager.ResetSave; debug skip button | Reuses save infrastructure. | ✓ |
| Separate tutorial save file | Extra file; pointless duplication. | |
| Always re-trigger on launch (skip-with-button) | Annoying for repeat sessions. | |

**[auto] Selected:** Phase 6 save reuse + debug skip.

---

## Claude's Discretion

- Outline = emission boost over Renderer-Feature outline.
- IFeedbackService home = WM.Core (revisit if it grows).
- Particle pool size = 4 per FeedbackId.

## Deferred Ideas

- Tutorial replay menu, multi-language, branching, voice-over, haptics, camera shake, FX pool tuning, first-order seeding for tutorial determinism — deferred to post-MVP or specific later phases.
