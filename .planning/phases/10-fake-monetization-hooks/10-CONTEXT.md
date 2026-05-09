# Phase 10: Fake Monetization Hooks - Context

**Gathered:** 2026-05-09 (--auto mode)
**Status:** Ready for planning

<domain>
## Phase Boundary

Replace the no-op `IAdService` stub (Phase 1 D-09) with a `FakeAdService` that simulates rewarded-ad completion (button + 2-second "loading" → success). Wire two MVP placements:
1. **`double_order_reward`** — after delivery, prompt offers 2x cash for that order; on accept, double the cash awarded.
2. **`missing_cash_boost`** — at the upgrade station, when the player can't afford an upgrade by less than the cap, prompt offers missing cash up to a configured cap.

All ad UI lives behind `IAdService`; no real SDK references. Tutorial suppression: prompts never show during active tutorial. Analytics events fire via Phase 9's `AnalyticsManager` subscriptions (Phase 10 just emits `IAdService` events; Phase 9 already subscribed in D-04).

Covers requirements: **ADS-01**, **ADS-02**.

</domain>

<decisions>
## Implementation Decisions

### `IAdService` Implementation
- **D-01:** **`IAdService` interface** (refines Phase 1 D-09 stub):
  ```csharp
  public interface IAdService {
      void ShowRewarded(string placementId, Action<RewardedAdResult> onComplete);
      bool IsAvailable(string placementId);
      event Action<string> OnOfferShown;     // placementId
      event Action<string> OnOfferClicked;
      event Action<string> OnRewardCompleted;
      event Action<string> OnRewardSkipped;
  }
  public enum RewardedAdResult { Completed, Skipped, Failed }
  ```
- **D-02:** **`FakeAdService` MonoBehaviour** in `WM.Monetization`:
  - Holds reference to a single `AdPromptDialog` UI overlay.
  - `ShowRewarded`: fires `OnOfferShown(placementId)`. Opens `AdPromptDialog` with placement-specific text. Three buttons: "Watch ad", "No thanks", (close button = Skipped).
  - On "Watch ad": fires `OnOfferClicked`. Shows a 2-second "loading" spinner. Fires `OnRewardCompleted`. Invokes `onComplete(Completed)`. Closes dialog.
  - On "No thanks" / close: fires `OnRewardSkipped`. Invokes `onComplete(Skipped)`.
- **D-03:** **`AdPromptDialog`** = full-screen overlay panel inside `SafeAreaPanel`, initially inactive. CanvasGroup fade in/out. While open, `Time.timeScale = 0` (pauses gameplay). Pause-aware audio (`AudioListener.pause = true`). Re-enables on close.
- **D-04:** **No real SDK references anywhere.** No AdMob, Unity Ads, AppLovin, ironSource. Phase 12 (post-MVP) introduces real SDK behind the same interface.

### `MonetizationConfig` ScriptableObject
- **D-05:** **`MonetizationConfig` SO** at `Assets/_Project/ScriptableObjects/Monetization/MonetizationConfig.asset`:
  ```csharp
  public bool doubleOrderRewardEnabled = true;
  public float doubleOrderRewardProbability = 1.0f;   // chance to show after each delivery
  public bool missingCashBoostEnabled = true;
  public int  missingCashBoostCap = 100;              // max cash the boost can grant
  public int  missingCashBoostThreshold = 50;         // only offer if deficit ≤ this
  public float rewardedAdCooldownSeconds = 60f;
  ```

### `AdManager` (Placement Orchestrator)
- **D-06:** **`AdManager` plain C# service via Bootstrap** subscribes to game events, decides whether to show prompts, calls `IAdService.ShowRewarded`. Decouples placement logic from `IAdService` impl.
- **D-07:** **`double_order_reward` placement:**
  - Subscribes to `DeliveryZone.OnDelivered` (event already exists per Phase 4 D-17 deferred-list / Phase 8 D-06).
  - On delivery: check suppress conditions (tutorial active via `ITutorialService.IsActive`, cooldown expired, config enabled, RNG ≤ probability). If passes:
    - `IAdService.ShowRewarded("double_order_reward", result => { if (result == Completed) currency.Add(orderReward); })`.
  - Tracks `_lastAdTime` for cooldown.
- **D-08:** **`missing_cash_boost` placement:**
  - Subscribes to `IUpgradeService.OnPurchaseBlocked` (NEW event added Phase 10 — fired when `TryPurchase` fails because of insufficient cash). Phase 5 `IUpgradeService` interface gains `event Action<string,int,int> OnPurchaseBlocked` (id, cost, currentCash). Phase 5 plans must include this event from day one.
  - On block: compute `deficit = cost - currentCash`. If `deficit ≤ threshold` AND tutorial not active AND cooldown expired AND enabled: show prompt.
  - On accept: `currency.Add(min(deficit, cap))`. Player can then re-tap upgrade to buy.

### Tutorial Suppression
- **D-09:** **`AdManager` reads `ITutorialService.IsActive`** before any prompt. If active, no prompt — period. Spec: "Rewarded ad offers are not shown during the tutorial."
- **D-10:** **No suppression of analytics events** — placements that *would* fire still log `ad_offer_shown`-equivalent? Actually no: if no prompt shown, no `ad_offer_shown` event. Analytics only fires when a prompt actually displays.

### Cooldown & Frequency
- **D-11:** **Single global cooldown** across all rewarded placements. Tracked in `AdManager._lastAdTime = Time.realtimeSinceStartup`. New prompt only if `(now - _lastAdTime) >= config.rewardedAdCooldownSeconds`. Default 60s. Simpler than per-placement cooldowns for MVP.
- **D-12:** **Probability gate** on `double_order_reward` — `Random.value <= config.doubleOrderRewardProbability`. Default 1.0 for prototype validation; tunable.

### Analytics Hookup
- **D-13:** **Analytics subscription happens in Phase 9's `AnalyticsManager`** (already wired per Phase 9 D-04 — subscribed to `IAdService.OnOfferShown/Clicked/RewardCompleted/RewardSkipped`). Phase 10 doesn't emit analytics directly; firing the IAdService events is enough. AnalyticsManager translates to:
  - `OnOfferShown(placementId)` → `ad_offer_shown` with `placement_id`, `reward_type`, `reward_amount`.
  - `OnOfferClicked(placementId)` → `ad_offer_clicked` with `placement_id`.
  - `OnRewardCompleted(placementId)` → `rewarded_ad_completed` with `placement_id`, `reward_type`, `reward_amount`.
  - `OnRewardSkipped(placementId)` → `rewarded_ad_skipped` with `placement_id`.
  Reward params (type/amount) come from a static lookup in `AnalyticsManager` keyed by `placementId`.

### Upgrade-Station Integration
- **D-14:** **UpgradeStationPanel** (Phase 5 D-09) displays an inline "Get cash" ad button next to the disabled cost button when:
  - Player attempted purchase and was blocked OR
  - Player has been viewing the panel and `deficit ≤ threshold`.
  Button calls `AdManager.OfferMissingCash(upgradeId, currentCost)`. Visible only when conditions allow.

### Asmdef Layout
- **D-15:** **Type homes:**
  - `WM.Monetization` — `IAdService` (already from Phase 1), `FakeAdService`, `AdManager`, `MonetizationConfig` SO, `RewardedAdResult` enum, placement IDs constants.
  - `WM.UI` — `AdPromptDialog` widget.
  - `WM.Upgrades` — `IUpgradeService.OnPurchaseBlocked` event addition.

### Bootstrap Wiring
- **D-16:** **Bootstrap (Phase 10 additions):**
  - Read `MonetizationConfig`.
  - Instantiate `FakeAdService` MonoBehaviour (wire its `AdPromptDialog` reference) and inject as `IAdService` (replacing Phase 1 stub).
  - Instantiate `AdManager(config, IAdService, ICurrencyService, IUpgradeService, ITutorialService, DeliveryZone)` and call `Init` to subscribe.
  - Inject `IAdService` into `UpgradeStationPanel` for the inline boost button.

### Claude's Discretion
- D-03 `Time.timeScale = 0` while ad prompt open — chosen so player isn't dropping boxes during the prompt; spec doesn't mandate.
- D-11 single 60s cooldown is a placeholder; spec §4 doesn't specify.
- D-12 default `doubleOrderRewardProbability = 1.0` for prototype; intentionally aggressive so QA hits the placement; tune toward `0.4–0.6` after first internal session.
- D-08 `OnPurchaseBlocked` event addition to `IUpgradeService` — small refinement to Phase 5; planner for Phase 5 must include it from day one.
- D-14 inline boost button vs auto-prompt — auto-prompt risks feeling pushy; user-initiated reads better.
- All five gray areas auto-resolved with recommended defaults.

</decisions>

<canonical_refs>
## Canonical References

### Project Source of Truth
- `CLAUDE.md` — Simulated monetization in MVP; `IAdService`/`IIapService` from day one; never block core progression behind ads or purchases.
- `.planning/PROJECT.md` — No real ads, IAP, mediation, SKAdNetwork, ATT prompts in MVP; rewarded ads simulated.
- `.planning/REQUIREMENTS.md` — ADS-01 (fake rewarded flow + simulated completion + reward), ADS-02 (missing-cash placement with cap; tutorial suppression).
- `.planning/ROADMAP.md` — Phase 10 goal, success criteria, plan stubs (10-01 IAdService + fake impl, 10-02 double-cash, 10-03 missing-cash + cap + tutorial suppression).

### Prior Phase Substrate
- `.planning/phases/01-...` — `IAdService` stub (D-09); `WM.Monetization` asmdef (D-04).
- `.planning/phases/04-...` — `DeliveryZone.OnDelivered` deferred hook (D-17 deferred-list).
- `.planning/phases/05-...` — `IUpgradeService.TryPurchase` (D-04); `OnPurchaseBlocked` event added in Phase 10 D-08, planner must include from day one.
- `.planning/phases/08-...` — `ITutorialService.IsActive` (D-04).
- `.planning/phases/09-...` — `AnalyticsManager` subscribes to `IAdService` events (D-04, D-13).

### Architecture & Spec
- `specs/04-monetization-spec.md` §1 — Hybrid monetization stance.
- `specs/04-monetization-spec.md` §3 — MVP scope (fake rewarded buttons, placement logic, analytics events).
- `specs/04-monetization-spec.md` §4 — Rewarded principles + MVP placements (`double_order_reward`, `missing_cash_boost`, etc.).
- `specs/05-analytics-metrics-spec.md` §7 — Monetization analytics event contracts.
- `specs/06-technical-architecture-spec.md` §4 — `AdManager` core system.
- `specs/07-mvp-backlog-acceptance-criteria.md` — WM-025 (and adjacent monetization items).

### Reference (non-binding)
- `warehouse_master_plan.md`.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets (paper-locked)
- `IAdService` stub from Phase 1.
- `DeliveryZone.OnDelivered` event (Phase 4 D-17 deferred / wired in Phase 8 D-06).
- `IUpgradeService` (Phase 5 D-04) — `OnPurchaseBlocked` event added now.
- `ITutorialService.IsActive` for suppression (Phase 8 D-04).
- `AnalyticsManager` already subscribed to `IAdService` events (Phase 9 D-04).
- `Bootstrap` extension surface for service swaps.

### Established Patterns
- **Service interface + plain-C# / MonoBehaviour impl** with Bootstrap-injected concrete.
- **Stub-replaced-at-real-impl-phase** (Phase 1 D-09 → Phase 10 fills `IAdService`).
- **Configurable balance via SO** (`MonetizationConfig`).
- **One placement orchestrator** (`AdManager`) over scattered ad calls — same firewall pattern as `AnalyticsManager`.

### Integration Points
- `Bootstrap` — wire `FakeAdService`, `AdManager`, `MonetizationConfig`.
- `UpgradeStationPanel` (Phase 5 D-09) — gains "Get cash" inline ad button.
- `DeliveryZone` — emits `OnDelivered` event; `AdManager` listens.
- `IUpgradeService` — adds `OnPurchaseBlocked` event.
- `SafeAreaPanel` — `AdPromptDialog` overlay added.

</code_context>

<specifics>
## Specific Ideas

- **`AdManager` mirrors `AnalyticsManager`** as the second SDK firewall in the codebase. Two firewalls, both behind one-line Bootstrap swaps.
- **No core-progression block by ads.** Spec + PROJECT.md are explicit. Even `missing_cash_boost` is opt-in: player can decline and grind a few more orders.
- **`double_order_reward` probability = 1.0 in MVP** is intentional aggression for QA validation. Soft launch will tune down to whatever data supports.
- **Pause on prompt** (`Time.timeScale = 0`) is a small UX choice but it removes the "I dropped a box mid-prompt" failure mode. Cheap.

</specifics>

<deferred>
## Deferred Ideas

- **Real ads (AdMob, AppLovin MAX, ironSource, Unity LevelPlay)** — soft launch (REQUIREMENTS.md v2 ADS-V2-01).
- **Real IAP** (`remove_ads`, `starter_pack_01`) — soft launch (REQUIREMENTS.md v2 IAP-V2-01).
- **Interstitial ads at natural breakpoints** — soft launch (`specs/04 §5`).
- **Other rewarded placements** (`speed_boost`, `worker_boost`, `instant_pack`) — soft launch.
- **SKAdNetwork / ATT prompt** — Phase 11+ / soft launch.
- **Mediation layer** — soft launch.
- **Reward tracking persistence** (e.g., player has seen ad N times) — out of MVP.
- **Per-placement cooldowns** — single global suffices for MVP.
- **Frequency capping by session** — out of MVP.
- **In-app purchase product fetch / display / receipt validation** — soft launch.
- **Premium-currency products (gem packs, piggy bank)** — `specs/04 §6` deferred.
- **`iap_offer_shown` event firing path** — defined in spec §7 but no IAP placements wired in MVP.
- **Server-side reward verification** — out of MVP.

</deferred>

---

*Phase: 10-fake-monetization-hooks*
*Context gathered: 2026-05-09 (--auto)*
