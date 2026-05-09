# Phase 5: Buy an Upgrade - Discussion Log

> **Audit trail only.** Generated in --auto mode (recommended defaults).

**Date:** 2026-05-09
**Phase:** 5-buy-an-upgrade
**Mode:** --auto (Claude picked recommended option for every gray area)
**Areas auto-resolved:** UpgradeDef schema & cost curve, UpgradeManager composition, Effect application, Upgrade station UI, Hire Worker gating

---

## UpgradeDef schema & cost curve representation

| Option | Description | Selected |
|---|---|---|
| Single SO per upgrade with `costMultipliers` list + `effectCurve` list | Self-contained data; matches spec §5/§6 tables. | ✓ |
| Single SO per upgrade with formulaic curve fields (a, b, growth) | Less data; harder to match spec table verbatim. | |
| Central UpgradeTable SO holding all upgrades | One file but loses Inspector-per-upgrade tunability. | |

**[auto] Selected:** Per-upgrade SO with curves (recommended default).

## UpgradeManager composition

| Option | Description | Selected |
|---|---|---|
| MonoBehaviour on GameManager, Bootstrap-injected | Mirrors OrderManager (Phase 4 D-22). | ✓ |
| Plain C# service, no scene presence | Loses Inspector tunability of UpgradeDef[] field. | |

**[auto] Selected:** MonoBehaviour on GameManager.

## Effect application mechanism

| Option | Description | Selected |
|---|---|---|
| One IUpgradeEffect impl per upgrade | Single responsibility; testable; easy to extend. | ✓ |
| Central switch statement on UpgradeEffectKind enum | Grows with every new upgrade; switch fatigue. | |
| Event-broadcast pattern (UpgradeManager fires OnLevelChanged; subscribers act) | Fire-and-forget; harder to know if effect applied. | |

**[auto] Selected:** One IUpgradeEffect impl per upgrade.

## Upgrade station UI

| Option | Description | Selected |
|---|---|---|
| Full-screen overlay panel triggered by trigger zone | Best readability on phone; matches affordability spec. | ✓ |
| World-space UI floating above station | Cool but cramped on small screens. | |
| Bottom-sheet partial overlay | Compromise; less screen space than full overlay. | |

**[auto] Selected:** Full-screen overlay panel.

## Hire Worker upgrade representation

| Option | Description | Selected |
|---|---|---|
| Single-purchase: maxLevel=1, sets PlayerStats.WorkerUnlocked | Schema-uniform; one flag suffices for Phase 7. | ✓ |
| Distinct UI button (not in upgrade list) | Asymmetric UI flow. | |
| Multi-level (level 2 = second worker, etc.) | Out of MVP (one worker only). | |

**[auto] Selected:** Single-purchase via maxLevel=1.

---

## Claude's Discretion

- Movement-speed cost (`30`) and curve table inferred placeholders if spec §5/§6 are silent — planner reads spec verbatim.
- Overlay-blocks-input chosen over time-pause; revisit on playtest.

## Deferred Ideas

- Save persistence (Phase 6), analytics events (Phase 9), worker spawn (Phase 7), visual feedback (Phase 8), missing-cash ad (Phase 10), upgrade tooltips, refunds, multi-worker types, prerequisites — all out of MVP or other phases.
