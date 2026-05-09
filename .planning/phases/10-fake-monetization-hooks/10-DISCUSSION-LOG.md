# Phase 10: Fake Monetization Hooks - Discussion Log

> **Audit trail only.** Generated in --auto mode.

**Date:** 2026-05-09
**Phase:** 10-fake-monetization-hooks
**Mode:** --auto
**Areas auto-resolved:** Ad placement trigger pattern, Reward delivery mechanism, Tutorial suppression, Cap on missing-cash placement, UX for refusal

---

## Ad placement trigger pattern

| Option | Description | Selected |
|---|---|---|
| AdManager subscribes to game events; full-screen prompt overlay | Decouples placement logic from IAdService impl. | ✓ |
| Inline UI buttons in each station | Scatters ad logic; harder to suppress. | |
| Prompt-on-cooldown poll loop | Wasteful; ignores game state. | |

**[auto] Selected:** Event-driven AdManager + overlay.

## Reward delivery mechanism

| Option | Description | Selected |
|---|---|---|
| `Action<RewardedAdResult>` callback on ShowRewarded | Synchronous closure carries reward context. | ✓ |
| Static event fired post-completion | Loses placement-specific context. | |
| Promise-style task | Async-await on Unity main thread is awkward. | |

**[auto] Selected:** Callback.

## Tutorial suppression

| Option | Description | Selected |
|---|---|---|
| AdManager checks ITutorialService.IsActive before any prompt | Centralized; spec-aligned. | ✓ |
| Per-placement check | Repeated logic. | |
| Build flag | Inflexible. | |

**[auto] Selected:** Centralized check.

## Cap on missing-cash placement

| Option | Description | Selected |
|---|---|---|
| Configurable cap (default 100) + threshold (offer only if deficit ≤ 50) | Spec-aligned; tunable. | ✓ |
| No cap | Breaks economy curve. | |
| Cap = full upgrade cost | Trivializes purchase. | |

**[auto] Selected:** Cap + threshold via SO config.

## UX for refusal

| Option | Description | Selected |
|---|---|---|
| "Watch ad" + "No thanks" + close-button = Skipped; gameplay paused | Non-pushy; never traps player. | ✓ |
| Auto-close after timeout | Annoying without rationale. | |
| Force-watch on accept (no cancel mid-ad) | Hostile UX. | |

**[auto] Selected:** Three-button overlay with paused gameplay.

---

## Claude's Discretion

- `Time.timeScale = 0` while overlay open — chosen so player isn't dropping boxes mid-prompt.
- Single 60s cooldown across all rewarded placements.
- `doubleOrderRewardProbability = 1.0` for prototype; tune down post-internal-test.
- Inline boost button on UpgradeStationPanel chosen over auto-prompt.

## Deferred Ideas

- Real ads (AdMob/AppLovin/ironSource/Unity LevelPlay), real IAP, interstitials, other rewarded placements (speed/worker/instant-pack), SKAdNetwork/ATT, mediation, persistence of ad-tracking, per-placement cooldowns, frequency capping by session, IAP receipt validation, premium currency products, `iap_offer_shown` firing path, server-side reward verification — all out of MVP / soft launch.
