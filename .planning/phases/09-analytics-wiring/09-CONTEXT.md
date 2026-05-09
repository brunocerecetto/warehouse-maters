# Phase 9: Analytics Wiring - Context

**Gathered:** 2026-05-09 (--auto mode)
**Status:** Ready for planning

<domain>
## Phase Boundary

Replace the no-op `IAnalyticsService` stub (Phase 1 D-09) with a debug-log MVP implementation. Wrap all event emission in a single `AnalyticsManager` (per spec/PROJECT.md no-scattered-SDKs rule). Each gameplay system's existing domain events feed into one subscriber that translates them into spec-defined analytics events with the right parameters. Provide a disable toggle (config-driven) and failure isolation so analytics never breaks gameplay. Firebase deferred to soft launch.

Covers requirements: **TEL-01**, **TEL-02**.

</domain>

<decisions>
## Implementation Decisions

### Provider Implementation
- **D-01:** **`DebugLogAnalyticsService` for MVP.** Writes each event as a `Debug.Log("[Analytics] {event} {{params}}")`. No external SDK. Easy to inspect in Editor + iOS Console.app.
- **D-02:** **`NoOpAnalyticsService`** for the disabled state. Implements `IAnalyticsService` with empty methods. Bootstrap chooses which impl to wire based on `AnalyticsConfig.enabled`.
- **D-03:** **Firebase deferred.** Keep the `IAnalyticsService` interface stable so a `FirebaseAnalyticsService` can be added in soft launch without touching gameplay.

### Event Emission Pattern
- **D-04:** **Domain-event subscription, single translation point.** `AnalyticsManager` (a plain C# class instantiated by Bootstrap, NOT a MonoBehaviour — no scene presence) subscribes on `Init` to events from:
  - `ITutorialService.OnTutorialStarted/Completed/StepCompleted`
  - `IActiveOrder.OnNewOrder`, `OnPackingCompleted`
  - `DeliveryZone.OnDelivered`
  - `IUpgradeService.OnLevelChanged`
  - `WorkerSpawner.OnWorkerSpawned` (Phase 7 — added now if not present)
  - `IAdService` events (Phase 10 will fire these — Phase 9 stubs the subscription so Phase 10 just emits)
  - `CarrySystem.OnStackChanged` for `box_picked_up`
  Each subscription handler builds the parameter dictionary from spec §4–§7 and calls `analyticsService.Track(eventName, params)`. **Gameplay systems never call `IAnalyticsService` directly.**
- **D-05:** **`IAnalyticsService` interface:**
  ```csharp
  public interface IAnalyticsService {
      void Track(string eventName, Dictionary<string,object> parameters);
      void SetUserProperty(string key, object value);
  }
  ```
  Minimal API; the wrapper builds the contract.

### Parameter Validation
- **D-06:** **Runtime validation in `AnalyticsManager`.** Per-event whitelist of required parameters (defined in code as static `Dictionary<string, string[]>`). On track call, check all required keys present; if missing, `Debug.LogWarning` and drop the event. Never throws to gameplay.
- **D-07:** **`session_id` and `build_version` auto-injected.** AnalyticsManager generates a `session_id` GUID at boot; reads `Application.version` for `build_version`. Both attached to every event.
- **D-08:** **Funnel-event "first time" tracking** uses an in-memory `HashSet<string> _firstEventsFired`. Phase 9 doesn't persist this — `first_*` events fire once per session per fresh load. Phase 6 save persists what *happened* (cash, levels, tutorial) but the analytics first-event flag is recomputed: e.g., on load, if `saveData.tutorialCompleted == false`, then `first_tutorial_completed` will fire on next completion. Acceptable for MVP.

### Failure Isolation
- **D-09:** **Try/catch inside `AnalyticsManager.Track` only.** Wraps the actual `analyticsService.Track(...)` call. On exception, `Debug.LogError("[Analytics] track failed: {ex}")` and continue. Gameplay code never sees the exception. Validates spec acceptance: "gameplay continues if analytics fails."

### Disable Mechanism
- **D-10:** **`AnalyticsConfig` ScriptableObject** at `Assets/_Project/ScriptableObjects/Analytics/AnalyticsConfig.asset`:
  - `bool enabled = true`
  - `bool logToConsoleInDebugBuild = true`
  Bootstrap reads `enabled`; instantiates `DebugLogAnalyticsService` if true, `NoOpAnalyticsService` if false.
- **D-11:** **No runtime toggle.** Config baked at boot. Adding a settings menu is post-MVP.

### Event Catalog (specs §4–§7 verbatim)
- **D-12:** **MVP events emitted (all per `specs/05`):**
  - **Onboarding:** `tutorial_started`, `tutorial_step_completed`, `tutorial_completed`.
  - **First-session funnel:** `first_box_picked_up`, `first_order_completed`, `first_upgrade_purchased`, `first_worker_hired`.
  - **Gameplay:** `box_picked_up`, `box_dropped`, `order_started`, `order_packed`, `order_completed`, `package_delivered`.
  - **Economy:** `cash_earned`, `cash_spent`, `upgrade_purchased`, `upgrade_blocked_insufficient_cash`, `worker_hired`.
  - **Monetization (Phase 10 fires):** `ad_offer_shown`, `ad_offer_clicked`, `rewarded_ad_completed`, `rewarded_ad_skipped`.
  - **Session:** `session_started`, `session_ended`, `app_backgrounded`, `app_resumed`.
  Phase 9 wires every one whose source event already exists. Monetization events stay defined but unfired until Phase 10 wires `IAdService`. ROADMAP.md mentions a sub-set; spec is authoritative.
- **D-13:** **Parameter contracts** copy spec §4–§7 verbatim. Implementation lives in `AnalyticsManager` as a per-event `Build_<event>(args)` method that returns the `Dictionary<string,object>`. Concentrates parameter assembly; one file diff to add a parameter.

### Session Lifecycle
- **D-14:** **Session events** emitted from a `SessionTracker` MonoBehaviour on `GameManager`:
  - `Awake` → `session_started`.
  - `OnApplicationPause(true)` → `app_backgrounded`.
  - `OnApplicationPause(false)` → `app_resumed`.
  - `OnApplicationQuit` → `session_ended` with `session_duration`, `orders_completed`, `cash_earned` (read from CurrencyManager + a counter in OrderManager).
- **D-15:** **`time_since_start`** parameter is `Time.realtimeSinceStartup - sessionStartTime`. Available to any first-event handler.

### Asmdef Layout
- **D-16:** **Type homes:**
  - `WM.Analytics` — `IAnalyticsService` (already from Phase 1 D-09), `DebugLogAnalyticsService`, `NoOpAnalyticsService`, `AnalyticsManager` (plain C#), `AnalyticsConfig` SO, `SessionTracker` MonoBehaviour. References every other gameplay asmdef as a subscriber.
  - This asmdef has the most downstream references; that's expected for the central translation point.

### Bootstrap Wiring
- **D-17:** **Bootstrap (Phase 9 additions):**
  - Read `AnalyticsConfig`.
  - Instantiate `DebugLogAnalyticsService` or `NoOpAnalyticsService` based on `enabled`.
  - Instantiate `AnalyticsManager(IAnalyticsService, services...)` and call `Init` to subscribe.
  - Attach `SessionTracker` to `GameManager`.

### Claude's Discretion
- D-08 first-event tracking is in-memory only; persisting first-event flags would require save schema bump. Acceptable trade-off for MVP — duplicates are tolerable.
- D-13 per-event `Build_<event>` methods over a generic builder — explicit > clever for analytics contracts.
- D-14 session_ended on `OnApplicationQuit` is best-effort on iOS (the OS may kill before fire); spec doesn't mandate guaranteed delivery in MVP.
- D-16 single `WM.Analytics` asmdef vs multiple — single is simpler.
- All five gray areas auto-resolved with recommended defaults.

</decisions>

<canonical_refs>
## Canonical References

### Project Source of Truth
- `CLAUDE.md` — Analytics-first stance: every MVP event routes through one `AnalyticsManager`; no scattered SDK calls.
- `.planning/PROJECT.md` — Wrapper-first analytics; mock impl in MVP; avoid SDK lock-in.
- `.planning/REQUIREMENTS.md` — TEL-01 (wrapper centralizes; can be disabled; gameplay continues on failure), TEL-02 (event list).
- `.planning/ROADMAP.md` — Phase 9 goal, success criteria, plan stubs (09-01 IAnalyticsService + debug-log impl, 09-02 AnalyticsManager toggle/validation/isolation, 09-03 hook event sites).

### Prior Phase Substrate
- All prior CONTEXT.md files — Phase 9 subscribes to events from every prior system.
- `.planning/phases/01-...` — `IAnalyticsService` stub already exists (D-09); `WM.Analytics` asmdef (D-04).

### Architecture & Spec
- `specs/05-analytics-metrics-spec.md` §2 — Provider stance: Firebase recommended, deferred.
- `specs/05-analytics-metrics-spec.md` §3 — Eight core questions analytics must answer (drives event selection).
- `specs/05-analytics-metrics-spec.md` §4 — Funnel events.
- `specs/05-analytics-metrics-spec.md` §5 — Gameplay events.
- `specs/05-analytics-metrics-spec.md` §6 — Economy events.
- `specs/05-analytics-metrics-spec.md` §7 — Monetization events (Phase 10 fires).
- `specs/05-analytics-metrics-spec.md` §8 — Session events.
- `specs/05-analytics-metrics-spec.md` §9 — Validation metrics (under-60s targets).
- `specs/06-technical-architecture-spec.md` §4 — `AnalyticsManager` core system.
- `specs/07-mvp-backlog-acceptance-criteria.md` — WM-023, WM-024 (analytics wrapper + events).

### Reference (non-binding)
- `warehouse_master_plan.md`.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets (paper-locked)
- `IAnalyticsService` stub from Phase 1.
- All domain events fired by prior phases — single subscription point in `AnalyticsManager`.
- `Bootstrap` extension surface — Phase 9 wires services + AnalyticsManager.

### Established Patterns
- **Stub-replaced-at-real-impl-phase** (Phase 1 D-09 is the seam).
- **Plain C# service via Bootstrap** for stateful infra (`AnalyticsManager`).
- **Domain-event subscription** over polling for cross-system coupling.

### Integration Points
- `Bootstrap.Awake` — wire `IAnalyticsService` impl + `AnalyticsManager` subscriptions.
- `GameManager` — `SessionTracker` MonoBehaviour added.
- All gameplay services already expose the events Phase 9 needs — zero edits to gameplay code.

</code_context>

<specifics>
## Specific Ideas

- **`AnalyticsManager` is the SDK firewall.** No `Firebase.Analytics.LogEvent(...)` will ever appear outside `WM.Analytics`. Soft-launch swap: implement `FirebaseAnalyticsService : IAnalyticsService`, change one Bootstrap line.
- **Event names are spec-bound.** Event names and parameter contracts are part of the analytics SLA — any deviation gets caught in a CI test (Phase 11 budget) by asserting the static dictionary matches the spec table.
- **Failure isolation is gameplay's contract**, not analytics' problem. Try/catch inside Track + LogError out keeps gameplay deterministic regardless of remote backend health. (Validates the "gameplay continues if analytics fails" acceptance criterion.)
- **First-event flags being session-scoped** is a deliberate MVP shortcut. If validation finds duplicate `first_order_completed` events from same player skewing funnel data, persist flags in Phase 6 save (one schema bump).

</specifics>

<deferred>
## Deferred Ideas

- **Firebase Analytics integration** — soft launch (PROJECT.md / REQUIREMENTS.md v2).
- **AppsFlyer / GameAnalytics SDKs** — soft launch.
- **Persisted first-event flags** — defer until validation needs it.
- **Event batching / offline queue** — debug-log impl doesn't need it; real SDK handles internally.
- **A/B test integration** — out of MVP.
- **User properties beyond build_version** — out of MVP.
- **PII / consent UI** (ATT prompt etc.) — Phase 11 / soft launch.
- **`area_unlocked` event** (mentioned in REQUIREMENTS.md TEL-02) — no area-unlock feature in MVP; event defined but unfired.
- **Analytics validation harness in CI** — defer; spec compliance test is sufficient for MVP.
- **Runtime analytics-toggle UI for QA** — out of MVP.

</deferred>

---

*Phase: 9-analytics-wiring*
*Context gathered: 2026-05-09 (--auto)*
