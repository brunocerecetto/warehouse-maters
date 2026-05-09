# Phase 8: Tutorial & Feel - Context

**Gathered:** 2026-05-09 (--auto mode)
**Status:** Ready for planning

<domain>
## Phase Boundary

Add a sub-60-second onboarding flow guiding a fresh save through: move to dock → pick up boxes → move to packing station → complete first order → deliver → buy first upgrade. Tutorial completion persists via Phase 6 save (`SaveDataV1.tutorialCompleted`). Add visible/audible feedback effects for pickup, drop, delivery, cash gain, upgrade purchase via a single `IFeedbackService` abstraction so SDK calls don't scatter.

Covers requirements: **TUT-01**, **TUT-02**.

</domain>

<decisions>
## Implementation Decisions

### Tutorial Step Framework
- **D-01:** **`TutorialStep` ScriptableObject schema:**
  ```csharp
  [CreateAssetMenu(menuName = "WM/Tutorial/TutorialStep")]
  public class TutorialStep : ScriptableObject {
      public string id;                       // e.g., "step_move_to_dock"
      [TextArea] public string instructionText;
      public TutorialTarget target;            // enum: Dock, Packing, Delivery, Upgrade, None
      public TutorialCompletion completion;    // enum: BoxPickedUp, OrderPacked, PackageDelivered, UpgradePurchased, EnteredZone(target), Manual
  }
  ```
  Six SOs at `Assets/_Project/ScriptableObjects/Tutorial/Step_<n>_<name>.asset`.
- **D-02:** **`TutorialFlow` ScriptableObject** holds `List<TutorialStep> steps` in order. Single asset `TutorialFlow_MVP.asset`. Adding/reordering steps is a data edit; `TutorialController` reads the list verbatim.

### Tutorial Step Sequence (MVP)
- **D-03:** **Six steps, matching `specs/02 §11`:**
  1. `step_move_to_dock` — Target: Dock; Completion: EnteredZone(dock).
  2. `step_pick_up_boxes` — Target: Dock; Completion: BoxPickedUp.
  3. `step_move_to_packing` — Target: Packing; Completion: EnteredZone(packing).
  4. `step_complete_order` — Target: Packing; Completion: OrderPacked.
  5. `step_deliver_package` — Target: Delivery; Completion: PackageDelivered.
  6. `step_buy_upgrade` — Target: Upgrade; Completion: UpgradePurchased.

### `TutorialController` Composition
- **D-04:** **`TutorialController` MonoBehaviour on `GameManager`** (mirrors other orchestrators). Implements `ITutorialService`:
  ```csharp
  public interface ITutorialService {
      bool IsActive { get; }
      bool IsCompleted { get; }
      TutorialStep CurrentStep { get; }
      event Action<TutorialStep> OnStepStarted;
      event Action<TutorialStep> OnStepCompleted;
      event Action OnTutorialStarted;
      event Action OnTutorialCompleted;
  }
  ```
- **D-05:** **Bootstrap-injected with `TutorialFlow`, `IActiveOrder`, `ICurrencyService`, `IUpgradeService`, `CarrySystem` (player), trigger zones**. On `Init(saveData.tutorialCompleted)`: if completed → `IsCompleted = true`, no UI; else → start at step 0, fire `OnTutorialStarted`.
- **D-06:** **Step transition on event match.** Each step type subscribes to a domain event:
  - `BoxPickedUp` → `CarrySystem.OnStackChanged` (Phase 3 D-09) — first time count goes 0→1.
  - `OrderPacked` → `IActiveOrder.OnPackingCompleted` (Phase 4 D-08).
  - `PackageDelivered` → DeliveryZone fires `OnDelivered` event (added Phase 4 D-17 deferred-list).
  - `UpgradePurchased` → `IUpgradeService.OnLevelChanged` (Phase 5 D-04) — first event.
  - `EnteredZone(target)` → station-specific trigger zones added this phase (lightweight `TutorialTriggerZone` MonoBehaviour on each station's existing trigger; emits `OnPlayerEntered`).
  Match found → fire `OnStepCompleted`, advance to next step.

### Indicator UI
- **D-07:** **World-space arrow** above target station + **HUD bubble** with instruction text.
  - Arrow: a small `Quad` with arrow material, positioned ~1m above the target station, bobbing (sine-wave Y offset, ~0.2 amplitude, ~1Hz). Pre-spawned arrow per station type (4 arrows total: Dock, Packing, Delivery, Upgrade); only the current step's arrow is active.
  - HUD bubble: panel anchored bottom-center inside `SafeAreaPanel` (above joystick area). Shows `instructionText`. Fades in on step start, out on completion.
- **D-08:** **`TutorialIndicators` MonoBehaviour** owns the four arrows + the bubble. Subscribes to `OnStepStarted` to swap visible arrow + bubble text. On `OnStepCompleted` fades both out.

### Feedback Service
- **D-09:** **`IFeedbackService` interface** in `WM.Core` (or a new `WM.Feedback` asmdef if we want isolation):
  ```csharp
  public enum FeedbackId {
      BoxPickedUp, BoxDropped, PackageDelivered, CashEarned, UpgradePurchased, TutorialStepCompleted
  }
  public interface IFeedbackService {
      void Play(FeedbackId id, Vector3 worldPos);
      void SetMuted(bool muted);
  }
  ```
- **D-10:** **`FeedbackService` impl** = MonoBehaviour on `GameManager`. Holds a `FeedbackBank` SO with:
  - One `ParticleSystem` prefab per `FeedbackId`.
  - One `AudioClip` per `FeedbackId`.
  Pools both at startup (pre-instantiates 4 of each particle prefab; pooled via simple `Queue<ParticleSystem>`). On `Play`, dequeues, sets position, plays; auto-returns to pool on stop.
- **D-11:** **Audio routing** — single shared `AudioSource` on the `FeedbackService` GameObject, `PlayOneShot(clip)` for SFX. Spatial blend = 0 (UI-style), no 3D positioning for MVP.
- **D-12:** **Feedback emission sites:**
  - `CarrySystem.TryAccept(box)` → `feedback.Play(BoxPickedUp, position)` after stack updates.
  - `PackingStation` accepted box → `feedback.Play(BoxDropped, position)`.
  - `DeliveryZone.OnDelivered` → `feedback.Play(PackageDelivered, position)` AND `feedback.Play(CashEarned, position)`.
  - `UpgradeManager.TryPurchase` succeeds → `feedback.Play(UpgradePurchased, position)`.
  - `TutorialController.OnStepCompleted` → `feedback.Play(TutorialStepCompleted, target.position)`.

### Asset Manifest
- **D-13:** **Particle prefabs** at `Assets/_Project/Prefabs/FX/`:
  - `FX_PickupPop.prefab` (small upward pop, +1 sparkle).
  - `FX_DropDust.prefab` (puff at floor).
  - `FX_DeliveryBurst.prefab` (radial burst).
  - `FX_CashSparkle.prefab` (small coin sparkle).
  - `FX_UpgradeConfetti.prefab` (vertical confetti).
  - `FX_StepComplete.prefab` (subtle ring pulse).
- **D-14:** **Audio clips** at `Assets/_Project/Audio/`:
  - Pickup, drop, delivery, cash, upgrade, step_complete.
  - Free-licensed placeholder set; final mix in soft launch. Clips < 1s each.

### Tutorial Skip / Reset
- **D-15:** **Skip-tutorial debug button** visible only when `Debug.isDebugBuild`, attached to `OrderHUD` corner alongside Phase 6 D-10 reset button. Tapping calls `tutorialService.SkipToCompleted()` (sets flag, fires `OnTutorialCompleted`, autosaves).
- **D-16:** **Tutorial reset** is a side-effect of `SaveManager.ResetSave()` (Phase 6 D-10) — `tutorialCompleted` flips to false.

### Step-Target Highlight
- **D-17:** **Outline shader on target station** when current step targets it. Use Unity URP outline (Renderer Feature) or simple emission boost on the station's material. Default: emission boost — toggle `material.SetColor("_EmissionColor", highlightColor)` at step start, restore at step end. No new shader files; minimal impact.

### Asmdef Layout
- **D-18:** **Type homes:**
  - `WM.Tutorial` — `TutorialStep` SO, `TutorialFlow` SO, `TutorialController`, `ITutorialService`, `TutorialIndicators`, `TutorialTriggerZone`, `TutorialTarget` enum, `TutorialCompletion` enum. References many other asmdefs (consumer of all gameplay events).
  - `WM.Core` (or new `WM.Feedback`) — `IFeedbackService`, `FeedbackId` enum, `FeedbackService`, `FeedbackBank` SO.
  - `WM.UI` — `TutorialBubble` widget.
- **D-19:** **Default placement of feedback in `WM.Core`** — keeps the asmdef count from blooming for a single service. Move to `WM.Feedback` later if it grows beyond five files.

### Bootstrap Wiring
- **D-20:** **Bootstrap (Phase 8 additions):**
  - Instantiate `FeedbackService` (or rather wire its existing GameObject) and inject `FeedbackBank` SO.
  - Inject `IFeedbackService` into `CarrySystem`, `PackingStation`, `DeliveryZone`, `UpgradeManager`, `TutorialController`.
  - Inject `TutorialFlow`, services, and `saveData.tutorialCompleted` into `TutorialController.Init(...)`.
  - Inject `ITutorialService` into `TutorialIndicators` and `TutorialBubble`.

### Claude's Discretion
- D-08 indicator: world-space arrow + HUD bubble combo chosen over single approach for clarity.
- D-13/D-14 placeholder asset set is minimal; final art outside MVP scope (PROJECT.md).
- D-17 emission boost vs full outline shader — emission is one line of code, outline is a Renderer Feature setup. Default: emission boost; planner can swap.
- D-19 IFeedbackService home — `WM.Core` for MVP simplicity.
- All five gray areas auto-resolved with recommended defaults.

</decisions>

<canonical_refs>
## Canonical References

### Project Source of Truth
- `CLAUDE.md` — short tutorial; analytics-first stance (events scattered? no — IFeedbackService isolates SFX/VFX too); ScriptableObjects for tunable data.
- `.planning/PROJECT.md` — Sub-60-second tutorial; first order < 60s; first upgrade < 2min.
- `.planning/REQUIREMENTS.md` — TUT-01 (sub-60s flow, completion saved, never re-triggers), TUT-02 (feedback for pickup/delivery/cash/upgrade).
- `.planning/ROADMAP.md` — Phase 8 goal, success criteria, plan stubs (08-01 state machine, 08-02 indicators, 08-03 feedback effects).

### Prior Phase Substrate
- `.planning/phases/03-...` — `CarrySystem.OnStackChanged` event (D-09).
- `.planning/phases/04-...` — `IActiveOrder` events (D-08); `DeliveryZone.OnDelivered` deferred-idea hook.
- `.planning/phases/05-...` — `IUpgradeService.OnLevelChanged` (D-04).
- `.planning/phases/06-...` — `SaveDataV1.tutorialCompleted` (D-03); autosave subscribes (D-07).

### Architecture & Spec
- `specs/02-gameplay-systems-spec.md` §11 — Tutorial requirements; six guided actions.
- `specs/02-gameplay-systems-spec.md` §13/§14 — Feedback / Audio (if present).
- `specs/06-technical-architecture-spec.md` §4 — Tutorial as core system; `IAnalyticsService` event hooks.
- `specs/05-analytics-metrics-spec.md` §4 — `tutorial_started`, `tutorial_step_completed`, `tutorial_completed` analytics events (Phase 9 wires).
- `specs/07-mvp-backlog-acceptance-criteria.md` — WM-021, WM-022 (tutorial + feedback).

### Reference (non-binding)
- `warehouse_master_plan.md`.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets (paper-locked)
- All gameplay events from prior phases — `OnStackChanged`, `OnPackingCompleted`, `OnDelivered`, `OnLevelChanged` — feed step transitions.
- `Bootstrap` composition root — Phase 8 extends with feedback + tutorial wiring.
- Save persistence (Phase 6) — tutorial completion field already in schema.
- `SafeAreaPanel` canvas — bubble lives here.

### Established Patterns
- **MonoBehaviour-on-GameManager** for orchestrators.
- **Service interface + plain-C# / MonoBehaviour impl** — `IFeedbackService`, `ITutorialService`.
- **ScriptableObject for tunable data** — `TutorialFlow`, `FeedbackBank`.
- **Event-driven, no polling** — step transitions via subscriptions.

### Integration Points
- `GameManager` GameObject — `TutorialController` and `FeedbackService` MonoBehaviours.
- Each station — `TutorialTriggerZone` reuses the existing child trigger or adds a sibling. Outline emission boost on station materials.
- `UICanvas/SafeAreaPanel` — `TutorialBubble` panel added.
- `OrderHUD` — debug-build skip-tutorial button.
- `CarrySystem`, `PackingStation`, `DeliveryZone`, `UpgradeManager` — receive `IFeedbackService` injection; emit `feedback.Play(...)` at the right moments.

</code_context>

<specifics>
## Specific Ideas

- **Sub-60s budget drives D-03 step count.** Six steps, each ~10s of player attention; spec §11's six bullets land naturally inside the budget.
- **Feedback service is the second SDK-isolation seam.** First was `IAnalyticsService` (Phase 9); `IFeedbackService` does the same for FX/SFX so we can swap to FMOD/Wwise later without touching gameplay.
- **Particle/audio pooling at startup.** Hybrid-casual budget (60fps on iPhone 8) demands no per-event allocation; pre-pooling 4 instances per FeedbackId is the cheap reliable choice.
- **Outline = emission boost** keeps shader complexity out of the project for one phase's worth of need. Real outline can be a polish-phase swap.
- **Step-completion fires both OnStepCompleted AND a feedback.Play(TutorialStepCompleted)** so the visual confirmation lands whether or not analytics is enabled.

</specifics>

<deferred>
## Deferred Ideas

- **Tutorial replay / "show tutorial again" menu option** — out of MVP.
- **Multi-language tutorial text** — out of MVP per `CLAUDE.md` (English-only).
- **Tutorial gating of non-essential features (no upgrade station before step 6)** — Phase 8 doesn't lock UI; just guides. Spec doesn't require gating.
- **Voice-over / animated character mascot** — outside MVP art scope.
- **Tutorial branching based on player actions** — linear flow only for MVP.
- **Tutorial analytics events** (`tutorial_started`, `tutorial_step_completed`, `tutorial_completed`) — Phase 9 wires.
- **Suppress-rewarded-ads-during-tutorial** — Phase 10 (ADS-02) reads `ITutorialService.IsActive`.
- **Haptic feedback (iOS taptic)** — defer; could add to `IFeedbackService` later.
- **FX pool sizing tuning beyond 4** — defer; revisit on profiling.
- **Particle system warm-up** — defer; first-fire jitter in MVP is acceptable.
- **Camera shake / hit-stop on delivery / upgrade** — Phase 8 default is restrained; can layer later.
- **Tutorial UI animations (slide-in/scale-bounce on bubble)** — basic fade only for MVP.
- **First-order seeding to a deterministic template** (so tutorial step 4 always sees the simplest order) — `TutorialController.Init` can call `OrderManager.SeedFirstOrder("order_001")` if playtest finds randomness confusing for the first order; default keeps Phase 4 D-01 weighted-random.

</deferred>

---

*Phase: 8-tutorial-feel*
*Context gathered: 2026-05-09 (--auto)*
