# Phase 4: Complete an Order - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-05-09
**Phase:** 4-complete-an-order
**Areas discussed:** Order Generation & Template Pool, Packing-station drop mechanism, Package representation, Order/packing/cash HUD composition, Order flow & reward source

---

## Order Generation & Template Pool

### Q1: How should orders be selected from the template pool?

| Option | Description | Selected |
|--------|-------------|----------|
| Weighted random | Each OrderTemplate has 'weight' float; cumulative-weight random pick. Mirrors Phase 3 D-14 typeWeights. | ✓ |
| Uniform random | Pick any template uniformly. Simpler. | |
| Sequenced cycle | Hardcoded sequence. Predictable but boring. | |
| You decide | Claude picks. | |

**User's choice:** Weighted random (recommended).

### Q2: Can the same order template repeat back-to-back?

| Option | Description | Selected |
|--------|-------------|----------|
| Yes, allow repeats | Simple; spec doesn't forbid; weighted random already gives variety. | ✓ |
| No — avoid same twice in a row | Re-roll if last picked. Slight complexity. | |
| You decide | Claude picks. | |

**User's choice:** Yes, allow repeats.

### Q3: Initial OrderTemplate set for MVP?

| Option | Description | Selected |
|--------|-------------|----------|
| Spec table verbatim | Four orders from specs/02 §7: 2R→10, 1R+1B→15, 2Y+1B→25, 2R+2B+1Y→50. | ✓ |
| Three small only (1-3 boxes) | Drop the 5-box template. Avoids capacity edge case. | |
| You decide | Claude picks. | |

**User's choice:** Spec table verbatim.

### Q4: Behavior when total order boxes > carry capacity?

| Option | Description | Selected |
|--------|-------------|----------|
| Multi-trip allowed | Drop 3, return for rest. Packing waits for all required boxes. Spec-aligned. | ✓ |
| Filter out >capacity orders | Only pick templates with total ≤ current carry capacity. | |
| You decide | Claude picks. | |

**User's choice:** Multi-trip allowed.

---

## Packing-station drop mechanism

### Q1: How should the packing station detect player arrival and consume boxes?

| Option | Description | Selected |
|--------|-------------|----------|
| Station-driven trigger | Mirror dock pattern (Phase 3 D-01); child trigger BoxCollider; OnTriggerEnter/Stay drives drop loop. | ✓ |
| Player polls nearby station | PlayerController checks overlap. Couples player to stations. | |
| You decide | Claude picks. | |

**User's choice:** Station-driven trigger.

### Q2: What does the station consume from the carrier?

| Option | Description | Selected |
|--------|-------------|----------|
| Required-only, drip-feed | Take only required types every dropIntervalSeconds (0.15s). Spec-aligned. | ✓ |
| Take stack, reject non-matching | Take all, reject non-matching. More transfers. | |
| You decide | Claude picks. | |

**User's choice:** Required-only, drip-feed.

### Q3: What seam should PackingStation use to read order requirements and report progress?

| Option | Description | Selected |
|--------|-------------|----------|
| OrderManager exposes IActiveOrder | Plain C# interface (RemainingFor, Submit, OnRequirementsMet). Decouples station. | ✓ |
| Direct OrderManager reference | Couples station to concrete type. | |
| Event bus / global | Static events. Contradicts no-singleton rule. | |
| You decide | Claude picks. | |

**User's choice:** OrderManager exposes IActiveOrder.

### Q4: Where do consumed boxes go physically?

| Option | Description | Selected |
|--------|-------------|----------|
| Reparent to station, snap to drop slot, destroy when timer starts | Visible accumulation; supports multi-trip feedback. | ✓ |
| Destroy immediately on consume | No visual accumulation. | |
| Reparent and keep visible until package delivered | Visual clutter; complicates worker reuse. | |
| You decide | Claude picks. | |

**User's choice:** Reparent then destroy on timer start.

---

## Package representation

### Q1: What is a 'Package' in code/scene?

| Option | Description | Selected |
|--------|-------------|----------|
| New Package prefab + component | Single Package.prefab with distinct visual identity. | ✓ |
| Reuse one of the consumed Box GameObjects (re-skin) | Saves a prefab; muddles identity. | |
| Package as data (no GameObject) carried as a flag | Violates 'visible carry stack' genre principle. | |
| You decide | Claude picks. | |

**User's choice:** New Package prefab + component.

### Q2: How does the player pick up the spawned package?

| Option | Description | Selected |
|--------|-------------|----------|
| ICarrier-driven via station, occupies 1 carry slot | Station extends drip-feed; reuses CarryAnchor stack positioning. | ✓ |
| Dedicated PackageHolder slot on player | Carry Package + boxes simultaneously. More complex. | |
| You decide | Claude picks. | |

**User's choice:** ICarrier-driven via station, 1 slot.

### Q3: Can the player still pick up boxes while carrying a package?

| Option | Description | Selected |
|--------|-------------|----------|
| Yes — package is just another carry slot | Allows opportunistic next-order pickup. Frictionless. | ✓ |
| No — package locks the stack until delivered | Forces dedicated trip. Adds state. | |
| You decide | Claude picks. | |

**User's choice:** Yes — package is just another carry slot.

### Q4: Package's TryAccept seam — how does it ride on ICarrier without being a 'Box'?

| Option | Description | Selected |
|--------|-------------|----------|
| Generalize ICarrier to ICarryable | ICarryable interface; Box and Package both implement; CarrySystem stores ICarryable. Phase 3 retro-edit (paper-only). | ✓ |
| Make Package a Box subclass | Bends BoxType semantics. | |
| Separate package-carry path on CarrySystem | Asymmetric API; more surface. | |
| You decide | Claude picks. | |

**User's choice:** Generalize ICarrier to ICarryable.

---

## Order/packing/cash HUD composition

### Q1: How should the active order display look?

| Option | Description | Selected |
|--------|-------------|----------|
| Top-center HUD: per-type icon + remaining/total | Horizontal row of colored icons; counters tick down. | ✓ |
| Right-side panel with full text list | Verbose, harder to read. | |
| Single progress bar (overall %) | Hides which type is missing. | |
| You decide | Claude picks. | |

**User's choice:** Top-center HUD, per-type icons + counters.

### Q2: How should the packing timer be presented?

| Option | Description | Selected |
|--------|-------------|----------|
| Radial fill above PackingStation in world-space | World-space Canvas child; reads spatially. | ✓ |
| HUD progress bar in OrderHUD | Always visible, less spatial. | |
| Both world-space + HUD bar | Doubled feedback; visual noise. | |
| You decide | Claude picks. | |

**User's choice:** Radial fill above station, world-space.

### Q3: How should cash display update?

| Option | Description | Selected |
|--------|-------------|----------|
| Top-right corner: ¤ value with rolling-number tween | Tween 0.4s ease-out on change. Coin icon prefix. | ✓ |
| Static counter, instant updates | Loses 'numbers go up' beat. | |
| You decide | Claude picks. | |

**User's choice:** Rolling-number tween.

### Q4: Where do these widgets live in the canvas hierarchy?

| Option | Description | Selected |
|--------|-------------|----------|
| Three sibling panels under SafeAreaPanel: OrderHUD, CashHUD, Joystick | Each its own MonoBehaviour. | ✓ |
| Single 'GameHUD' MonoBehaviour orchestrates all | Heavier coupling; violates 'small MonoBehaviours'. | |
| You decide | Claude picks. | |

**User's choice:** Three sibling panels.

---

## Order flow & reward source

### Q1: Where does the cash reward come from?

| Option | Description | Selected |
|--------|-------------|----------|
| OrderTemplate.baseCashReward field, table-based | Spec §4 recommends table-based for MVP; values 10/15/25/50. | ✓ |
| Computed via formula at delivery time | Spec calls out as 'future formula candidate'. | |
| You decide | Claude picks. | |

**User's choice:** OrderTemplate.baseCashReward, table-based.

### Q2: When does the next order spawn?

| Option | Description | Selected |
|--------|-------------|----------|
| Instantly on package delivery | Zero-latency next-order; HUD updates immediately. | ✓ |
| Instantly when packing finishes (before delivery) | Confusing UX with carried package. | |
| After short delay (~0.5s) for feedback | Lets cash-pop land first. | |
| You decide | Claude picks. | |

**User's choice:** Instantly on delivery.

### Q3: Where does OrderManager live and how is it composed?

| Option | Description | Selected |
|--------|-------------|----------|
| MonoBehaviour on GameManager GameObject, injected by Bootstrap | Templates as serialized field; implements IActiveOrder. | ✓ |
| Plain C# service, instantiated by Bootstrap, no scene presence | Loses Inspector tunability. | |
| ScriptableObject as runtime state | Avoid — SOs are tunable data, not runtime state. | |
| You decide | Claude picks. | |

**User's choice:** MonoBehaviour on GameManager.

### Q4: Where does CurrencyManager live and how does it expose cash?

| Option | Description | Selected |
|--------|-------------|----------|
| Plain C# service via Bootstrap, exposes int Cash + event OnChanged | Phase 6 swaps to save-aware variant; matches IService pattern. | ✓ |
| MonoBehaviour on GameManager | No Inspector-tunable fields. | |
| Static class | Contradicts no-singleton rule. | |
| You decide | Claude picks. | |

**User's choice:** Plain C# service via Bootstrap.

---

## Claude's Discretion

- D-09 DropZone slot layout — vertical stack offset; planner can revise.
- D-10 default `3.0s` packing duration — placeholder; first-order-<60s budget; playtest will retune.
- D-12 Package visual identity — placeholder brown cube + darker stripe material; outside MVP art scope.
- D-21 `Package` asmdef placement (`WM.Orders` vs `WM.Boxes`) — default `WM.Orders` to avoid cycles; planner verifies during implementation.
- D-25 `BoxType.icon` field addition — Claude default for HUD readability; fallback is colored quad with letter.
- D-26 rolling-tween duration `0.4s` — hand-tuned default; Phase 8 may polish.

## Deferred Ideas

- Cash-pop / particle / SFX feedback — Phase 8 (TUT-02).
- `first_order_completed` / `order_completed` analytics events — Phase 9.
- Cash save persistence — Phase 6.
- Order Value upgrade (ECON-05) and Packing Speed upgrade (ECON-06) — Phase 5.
- Order queue / multi-order parallelism — out of MVP scope (spec §7).
- Order chaining combos / express orders — out of MVP scope.
- OrderTemplate `minLevel` / unlock gating — out of MVP scope.
- Package mid-flight visual polish — Phase 8.
- BoxType.icon proper artwork — outside MVP art scope.
- DOTween or similar tween library — defer; revisit only if Phase 8 tween needs grow.
- Order generation seeding for tutorial determinism — Phase 8 may override Init with a seeded template.
- Anti-streak rule (no same template twice in a row) — revisit only if playtest finds repetition boring.
- PackingStation / DeliveryZone audio cues — Phase 8.
