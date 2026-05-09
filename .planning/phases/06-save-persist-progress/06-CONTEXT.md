# Phase 6: Save & Persist Progress - Context

**Gathered:** 2026-05-09 (--auto mode)
**Status:** Ready for planning

<domain>
## Phase Boundary

Replace the no-op `ISaveService` stub (Phase 1 D-09) with a JSON-file-backed implementation. Persist cash, upgrade levels, tutorial state, worker unlock, warehouse progression. Autosave on key events with debounced async writes. Provide a debug reset hook. Schema is versioned; migration scaffold exists for future schema bumps.

Tutorial state field is reserved (Phase 8 writes it). Worker unlock writes from Phase 5 D-05's `PlayerStats.WorkerUnlocked` (`hire_worker` upgrade level).

Covers requirements: **SAVE-01**.

</domain>

<decisions>
## Implementation Decisions

### Storage Backend
- **D-01:** **JSON file at `Application.persistentDataPath/save.json`.** Inspectable via Xcode Organizer / Finder; future-friendly for migration tooling. PlayerPrefs rejected — opaque, harder to inspect; not idiomatic for non-trivial structured data.
- **D-02:** **Atomic write** — write to `save.json.tmp`, then `File.Move` over `save.json`. Prevents partial-file corruption on crash mid-write.

### Schema
- **D-03:** **Schema mirrors `specs/06 §6` verbatim.** `SaveDataV1` plain C# `[Serializable]` class:
  ```csharp
  [Serializable]
  public class SaveDataV1 {
      public int    version = 1;
      public int    cash = 0;
      public bool   tutorialCompleted = false;
      public UpgradeLevels upgradeLevels = new UpgradeLevels();
      public List<string>  workersUnlocked = new List<string>();
      public int    warehouseProgressionLevel = 0;
  }
  [Serializable]
  public class UpgradeLevels {
      public int carry_capacity = 0;
      public int movement_speed = 0;
      public int order_value = 0;
      public int packing_speed = 0;
      public int shelf_capacity = 0;
      public int hire_worker = 0;
  }
  ```
  `UpgradeLevels` uses fixed fields (not a `Dictionary<string,int>`) because `JsonUtility` cannot serialize dictionaries. Adding a 7th upgrade requires a schema-bump migration — acceptable for MVP (only six upgrades).
- **D-04:** **Versioning** — `int version` is the first field. On load, if `version < currentVersion`, run migration chain `Migrate_1_to_2`, `Migrate_2_to_3`, etc. Phase 6 has no migrations to run; the chain is empty.

### `SaveManager` Composition
- **D-05:** **`SaveManager` MonoBehaviour on `GameManager`** (mirrors `OrderManager`/`UpgradeManager` pattern). Implements actual `ISaveService`:
  ```csharp
  public interface ISaveService {
      bool HasSave { get; }
      void   Save();
      Task   SaveAsync();
      SaveDataV1 Load();           // returns default if no file
      void   ResetSave();
      event Action<SaveDataV1> OnLoaded;
      event Action OnSaved;
  }
  ```
- **D-06:** **`Bootstrap` swaps in `SaveManager`** as the `ISaveService` implementation, replacing the Phase 1 no-op stub. Bootstrap loads on Awake (`var data = saveService.Load()`) and injects the loaded data into `CurrencyManager.Init(data.cash)`, `UpgradeManager.Init(data.upgradeLevels)`, and (Phase 8) `TutorialController.Init(data.tutorialCompleted)`.

### Autosave Triggers
- **D-07:** **Autosave subscribes to:**
  - `ICurrencyService.OnChanged` — every cash change (debounced).
  - `IUpgradeService.OnLevelChanged` — every purchase.
  - `ITutorialService.OnCompleted` — Phase 8.
  - Unity `OnApplicationPause(pause==true)` — backgrounding.
  - Unity `OnApplicationFocus(focus==false)` — tab switch on Editor / lock screen on iOS.
  - Unity `OnApplicationQuit` — clean shutdown (best-effort).
- **D-08:** **Debounce 1.0s.** On any trigger, schedule a save 1.0s out; if another trigger fires before the timer, reset. Coalesces bursts (e.g., rapid cash gains) into one file write.
- **D-09:** **Async writes** via `Task.Run` for `File.WriteAllText`. Main thread builds the `SaveDataV1` snapshot synchronously (cheap) and dispatches the I/O.

### Reset Hook
- **D-10:** **`[ContextMenu("Reset Save")]` on `SaveManager`** for Editor-side debug. Plus a hidden debug button visible only when `Debug.isDebugBuild`, attached to `OrderHUD` corner. Tapping calls `saveService.ResetSave()` then `Application.Quit()` (or `EditorApplication.isPlaying = false` in Editor).
- **D-11:** **No keystroke combo** — iOS doesn't have keyboards in normal play; not worth a special gesture for MVP.

### Migration Strategy
- **D-12:** **Forward-only migration.** Migrations are non-reversible (no downgrade). Each migration file in `Assets/_Project/Scripts/Save/Migrations/` is a static method `Migrate_N_to_M(string rawJson) → string newRawJson`. Chain runs in order on load if `version < current`.
- **D-13:** **Schema rules:**
  - Never remove fields; deprecated fields stay readable.
  - New fields default-initialize on load (Unity `JsonUtility` already does this).
  - Bump `version` only when a logical-meaning change occurs (e.g., renaming an upgrade id).

### Asmdef Layout
- **D-14:** **Type homes:**
  - `WM.Save` — `ISaveService` (already from Phase 1 D-09), `SaveManager` MonoBehaviour, `SaveDataV1`, `UpgradeLevels`, migration static methods.
  - References none beyond core. `WM.Economy`, `WM.Upgrades`, etc. remain unaware of save internals — they speak via `ISaveService` only.

### Claude's Discretion
- D-08 debounce of 1.0s is a default; planner can tune after profiling burst write impact.
- D-10 reset-hook placement on `OrderHUD` corner is arbitrary; any in-game debug menu is acceptable.
- D-09 async via `Task.Run` vs Unity Coroutine — `Task.Run` chosen for true off-main-thread; coroutines run on the main thread. Iff `Task.Run` introduces issues on iOS IL2CPP, fall back to coroutine + frame-spread serialization.
- All five gray areas auto-resolved with recommended defaults.

</decisions>

<canonical_refs>
## Canonical References

### Project Source of Truth
- `CLAUDE.md` — Local save data, plain C# services, no unspecified third-party packages.
- `.planning/PROJECT.md` — No backend in MVP; local save sufficient.
- `.planning/REQUIREMENTS.md` — SAVE-01 acceptance criteria.
- `.planning/ROADMAP.md` — Phase 6 goal, success criteria, plan stubs (06-01 SaveManager + ISaveService, 06-02 wire systems, 06-03 reset hook + autosave).

### Prior Phase Substrate
- `.planning/phases/01-...` — `ISaveService` interface stubbed (D-09); `WM.Save` asmdef (D-04); `Bootstrap` injects services (D-07).
- `.planning/phases/04-...` — `ICurrencyService.OnChanged` (D-19) — autosave subscriber.
- `.planning/phases/05-...` — `IUpgradeService.OnLevelChanged` (D-04) — autosave subscriber. `UpgradeLevels` schema mirrors the six SO ids.

### Architecture & Spec
- `specs/02-gameplay-systems-spec.md` §12 — Save System MVP scope, acceptance criteria.
- `specs/06-technical-architecture-spec.md` §6 — Save Data Schema (drives D-03 verbatim).
- `specs/06-technical-architecture-spec.md` §2 — "Local JSON or PlayerPrefs wrapper".
- `specs/07-mvp-backlog-acceptance-criteria.md` — WM-018 (save).

### Reference (non-binding)
- `warehouse_master_plan.md`.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets (paper-locked)
- `ISaveService` interface stub from Phase 1 D-09.
- `Bootstrap` composition root extension point — Phase 6 swaps stub for `SaveManager`.
- `ICurrencyService` and `IUpgradeService` events — autosave subscribes via Bootstrap injection (subscribers, not the service itself).

### Established Patterns
- **MonoBehaviour-on-GameManager** for orchestrators.
- **Stub-replaced-at-real-impl-phase** — Phase 1 D-09 noop stubs are explicit "fill in here" markers; Phase 6 fills `ISaveService`.

### Integration Points
- `CurrencyManager` — Phase 6 wraps with save: on `Init(int initialCash)`, sets cash from save; on `OnChanged`, triggers autosave.
- `UpgradeManager` — Phase 6 wraps: on `Init(UpgradeLevels)`, applies all stored levels (re-runs each `IUpgradeEffect.Apply` to restore runtime state); on `OnLevelChanged`, triggers autosave.
- `Bootstrap.Awake` — load save before initializing systems; pass loaded data to each system's `Init`.

</code_context>

<specifics>
## Specific Ideas

- **Migrations need a home from day one.** Even if Phase 6 has zero migrations, the migration chain code path must exist so adding migration #1 doesn't require restructuring the loader.
- **Save schema is a contract surface.** All six upgrade ids in `UpgradeLevels` must match the `id` strings on the `Upgrade_*.asset` SOs (Phase 5 D-01). Validation: `SaveManager` on load asserts each id resolves to an `UpgradeDef` known to `UpgradeManager`; logs warning if an unknown id appears.
- **Re-applying upgrade effects on load** is the right way to restore runtime state (e.g., `PlayerStats.CarryCapacity = 8` for level-2 carry capacity) — the SO `effectCurve` is the source of truth, not the saved value. Keeps stat data and save data decoupled.

</specifics>

<deferred>
## Deferred Ideas

- **Cloud save** — out of MVP (PROJECT.md).
- **Save encryption / anti-cheat** — out of MVP; local save is trust-the-player for prototype.
- **Save slots / multi-profile** — single save only for MVP.
- **Save export/import** — could be added to debug menu later.
- **Save inspector window in Editor** — `[ContextMenu]` on SaveManager covers basics.
- **Migration testing harness** — defer until first real migration is needed.
- **Save schema diff tooling** — defer.
- **In-game "reset progress" with confirmation dialog** — debug-only for MVP; full UI is post-MVP polish.
- **Telemetry on save failures** — Phase 9 can add `save_failed` analytics event if needed.

</deferred>

---

*Phase: 6-save-persist-progress*
*Context gathered: 2026-05-09 (--auto)*
