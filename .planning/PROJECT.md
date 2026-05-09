# Warehouse Master

## What This Is

Warehouse Master is a free-to-play iOS hybrid-casual idle/tycoon prototype built in Unity. The player operates a small warehouse — receives boxes, prepares orders, delivers packages, earns cash, buys upgrades, and hires automated workers. The MVP validates whether the core loop is engaging for 5–10 minutes before any soft launch effort.

## Core Value

The 5-minute warehouse loop must feel satisfying: pick up boxes, complete an order, earn cash, buy an upgrade, watch the warehouse get faster.

## Requirements

### Validated

<!-- Shipped and confirmed valuable. -->

(None yet — ship to validate)

### Active

<!-- Current scope. Building toward these. -->

- [ ] Player can move with a one-thumb virtual joystick (portrait orientation)
- [ ] Player automatically picks up boxes when in range, up to a carry capacity
- [ ] Boxes are visually stacked on the character's back
- [ ] Loading dock spawns three box types (red, blue, yellow) at a configurable rate
- [ ] Order queue presents an active order requiring exact-match box types and quantities
- [ ] Packing station converts completed order requirements into a deliverable package
- [ ] Delivery zone awards cash on package delivery and starts the next order
- [ ] Upgrade station offers six upgrades: carry capacity, movement speed, order value, packing speed, shelf capacity, hire worker
- [ ] First worker (purchased via upgrade) automates loading-dock → shelf transport
- [ ] Local save/load persists cash, upgrade levels, tutorial state, worker unlock
- [ ] Sub-60-second tutorial guides first order, first delivery, first upgrade
- [ ] Analytics wrapper emits MVP events through a single abstraction (`IAnalyticsService`)
- [ ] Fake rewarded-ad flow (button + simulated completion) grants configured rewards
- [ ] iOS TestFlight-ready build pipeline produces an installable build

### Out of Scope

<!-- Explicit boundaries. -->

- Multiplayer — Adds backend, matchmaking, networking. Out of MVP scope.
- Backend services / cloud save — Local save sufficient to validate fun.
- Real ads (AdMob, mediation, SKAN) — Validate intent with simulated rewarded ads first.
- Real IAP / App Store products — No monetization validation in MVP.
- Leaderboards, battle pass, daily rewards, login calendar — Live-ops phase, not MVP.
- Multiple warehouses, zones, themes (port, airport, factory) — Phase 3+.
- Multiple currencies (gems, tickets) — Cash only for MVP.
- Complex physics, manual inventory, drivable vehicles — Out of design scope.
- Skins, decorations, social login — Not core to loop validation.
- Complex worker types (forklift, drone, packer, dispatcher) — One worker for MVP.
- Special box mechanics (fragile, frozen, heavy, gold, mystery) — Phase 3+.
- Order chaining combos, layout editor, express orders — Phase 3+ differentiators.
- Custom art beyond temporary low-poly assets — Validate gameplay before art investment.

## Context

- **Pre-Unity-init repo.** Only `CLAUDE.md`, `specs/` (8 specs), `warehouse_master_plan.md`. No Unity project, no commits.
- **Spec-driven development.** `CLAUDE.md` mandates specs as source of truth with priority order: user request → MVP backlog → gameplay → architecture → economy → monetization → analytics → roadmap.
- **MVP backlog already enumerated.** `specs/07-mvp-backlog-acceptance-criteria.md` lists WM-001 through WM-025 with acceptance criteria. GSD Active requirements derived from those epics.
- **Original master plan.** `warehouse_master_plan.md` (Spanish) details gameplay feel, monetization strategy, weekly production plan, creative briefs, risks. GSD planning treats this as starting context, not frozen.
- **Target session shape.** First order < 60s, first upgrade < 2min, first worker < 5min, first session 8–10min.
- **Analytics-first stance.** Every MVP event routes through one `AnalyticsManager` abstraction; no SDK calls scattered in gameplay code.
- **Monetization-friendly, not monetization-first.** Design natural rewarded-ad placements; defer real ads/IAP to soft launch.

## Constraints

- **Engine**: Unity (URP recommended) — Required by spec; broadest mobile monetization ecosystem.
- **Language**: C# — Unity standard.
- **Target platform**: iOS first, iPhone, portrait orientation — Spec recommendation locked.
- **Backend**: None for MVP — Local save only; backend deferred to soft launch.
- **Persistence**: Local save data — Validates loop without infrastructure cost.
- **Analytics**: Wrapper-first, mock implementation in MVP — Avoid SDK lock-in before validating fun.
- **Ads / IAP**: Simulated interfaces only in MVP (`IAdService`, `IIapService`) — No real SDKs until soft launch.
- **Architecture style**: Small MonoBehaviours + plain C# services + ScriptableObjects for data — No god-managers, no hardcoded balance.
- **Build target**: TestFlight-ready iOS build — Internal validation, not App Store release.
- **Timeline reference**: Master plan suggests 4-week internal prototype — Used as sizing signal, not contractual.
- **Language**: All code, comments, docs in English (per `CLAUDE.md`) — Master plan in Spanish is reference only.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Specs are input, not frozen | GSD planning may refine spec decisions when better information surfaces during phases | — Pending |
| Portrait orientation | One-thumb mobile ergonomics; spec §12 recommendation | — Pending |
| Visible stacked carry on character | Iconic to genre, more visceral than cart; spec §12 recommendation | — Pending |
| Exact-match order requirements from day one | Three box colors give clear signal; loose-match would waste box-type design | — Pending |
| First worker before first area expansion | "Warehouse runs itself" arc lands earlier; matches spec §12 recommendation | — Pending |
| Unity over Godot | iOS ads/IAP/mediation ecosystem; mobile monetization tooling | — Pending |
| No backend in MVP | Local save sufficient; backend not justified before retention/monetization validation | — Pending |
| Interface-first external integrations | `IAnalyticsService` / `IAdService` / `IIapService` / `ISaveService` from day one — swap to real SDKs in soft launch without touching gameplay | — Pending |
| Three box types only in MVP | Special boxes (fragile, frozen, heavy, gold) deferred — minimum needed for order variety | — Pending |
| Six upgrades only in MVP | Carry, speed, order value, packing speed, shelf capacity, hire worker — covers progression curve without scope creep | — Pending |
| One worker type only in MVP | Validates automation arc without specialized AI variants | — Pending |
| ScriptableObjects for tunable data | Box/order/upgrade/worker/economy values data-driven from day one | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-05-09 after initialization*
