# Roadmap: Warehouse Master

## Overview

Vertical-MVP slice plan that drives Warehouse Master from an empty Unity repo to a TestFlight-ready hybrid-casual prototype. Each phase delivers an end-to-end user-observable capability — start by getting any build to run, layer in movement, then the full pickup-to-delivery loop, then the upgrade and worker fantasy that defines the genre, then save/tutorial/feedback/analytics polish, and end with a TestFlight-installable build. The 5-minute fun question is answerable as soon as Phase 5 lands; everything after raises confidence.

## Phases

- [ ] **Phase 1: Project Bootstrap & Empty Warehouse Scene** — Unity iOS project + scaffolded scene that builds and launches
- [ ] **Phase 2: Walk Around the Warehouse** — One-thumb joystick movement and camera framing
- [ ] **Phase 3: Pick Up & Carry Boxes** — Loading dock spawns boxes; player auto-picks up with visible stack
- [ ] **Phase 4: Complete an Order** — Full pickup → packing → delivery → cash loop with one active order
- [ ] **Phase 5: Buy an Upgrade** — Upgrade station with all six MVP upgrades applying immediately
- [ ] **Phase 6: Save & Persist Progress** — Local save persists cash, upgrades, tutorial, worker unlock
- [ ] **Phase 7: Hire the First Worker** — Worker automates dock → shelf transport
- [ ] **Phase 8: Tutorial & Feel** — Sub-60s onboarding plus pickup/delivery/cash/upgrade feedback effects
- [ ] **Phase 9: Analytics Wiring** — `IAnalyticsService` wrapper emits all MVP events through one path
- [ ] **Phase 10: Fake Monetization Hooks** — Simulated rewarded-ad placements granting in-game rewards
- [ ] **Phase 11: TestFlight Build Pipeline** — CLI build script produces an installable iOS archive

## Phase Details

### Phase 1: Project Bootstrap & Empty Warehouse Scene
**Goal**: A Unity iOS project exists with the agreed folder structure and a placeholder `Warehouse_MVP` scene that builds and launches.
**Mode:** mvp
**Depends on**: Nothing (first phase)
**Requirements**: BOOT-01, BOOT-02
**Success Criteria** (what must be TRUE):
  1. Opening the project in Unity Editor loads the `Warehouse_MVP` scene with all required objects positioned (loading dock, packing station, delivery zone, upgrade station, shelf area, worker spawn, camera rig, UI canvas, event system, GameManager)
  2. Folder structure under `Assets/_Project/` matches the architecture spec, with assembly definitions per system folder
  3. A debug iOS build runs on simulator/device and shows the empty warehouse scene from the configured camera
**Plans**: 3 plans

Plans:
- [ ] 01-01-PLAN.md — Initialize Unity 6.3.7f1 project, pin Cinemachine/Input System/Test Framework packages, create _Project/ folder tree, all 13 production asmdefs + 2 test asmdefs, service interfaces and Null* stubs in WM.Core, BuildScript.BuildIOS skeleton, .gitignore + .gitattributes, Wave-0 BootstrapStructureTests
- [ ] 01-02-PLAN.md — Compose Warehouse_MVP scene with 12 required GameObjects (Player, LoadingDock, PackingStation, DeliveryZone, UpgradeStation, ShelfArea, WorkerSpawn, CameraRig, UICanvas, EventSystem, GameManager, Bootstrap), placeholder URP/Lit materials, Bootstrap-GameManager composition root wiring, register scene in Build Settings, BootstrapSmokeTests + PlayModeSmokeTests
- [ ] 01-03-PLAN.md — Configure Cinemachine 3.x camera rig (CinemachineBrain + passive CinemachineCamera at isometric pose), Screen-Space Camera UICanvas (1080x1920, Match=0.5) with SafeAreaPanel, EventSystem with InputSystemUIInputModule, extend BootstrapSmokeTests with Cinemachine/UICanvas/EventSystem assertions

### Phase 2: Walk Around the Warehouse
**Goal**: Player can drag a virtual joystick and walk the character around the warehouse, blocked by walls and stations.
**Mode:** mvp
**Depends on**: Phase 1
**Requirements**: MOVE-01, MOVE-02
**Success Criteria** (what must be TRUE):
  1. Touching and dragging the on-screen joystick moves the character in the dragged direction relative to the camera
  2. Releasing the joystick stops the character; collisions with walls/stations prevent passing through
  3. Movement speed reads from a configurable value that can later be modified by upgrades
**Plans**: TBD

Plans:
- [ ] 02-01: Virtual joystick UI component with touch handling
- [ ] 02-02: `PlayerController` with camera-relative movement and configurable speed
- [ ] 02-03: Collision setup for walls and stations

### Phase 3: Pick Up & Carry Boxes
**Goal**: Boxes spawn at the loading dock; the player auto-picks them up to a capped capacity and carries them as a visible stack.
**Mode:** mvp
**Depends on**: Phase 2
**Requirements**: BOX-01, BOX-02, BOX-04
**Success Criteria** (what must be TRUE):
  1. Walking onto the loading dock auto-adds boxes to the carried stack until carry capacity is reached
  2. Each carried box renders on the character's back with the correct color (red, blue, yellow); stack updates immediately on add
  3. Dock respects its capacity ceiling and spawn rate; spawned types match the configured distribution
**Plans**: TBD

Plans:
- [ ] 03-01: Box ScriptableObject definitions (red, blue, yellow) with prefabs
- [ ] 03-02: `BoxSpawner` with configurable rate, type distribution, dock capacity
- [ ] 03-03: `CarrySystem` with capacity, pickup zone trigger, visual stack updates

### Phase 4: Complete an Order
**Goal**: Player can complete one full order — pick up the right boxes, drop them at the packing station, deliver the resulting package for cash, and a new order spawns.
**Mode:** mvp
**Depends on**: Phase 3
**Requirements**: BOX-03, ORD-01, ORD-02, ORD-03, ORD-04, ECON-01
**Success Criteria** (what must be TRUE):
  1. UI displays the active order's required types and remaining counts; only matching boxes are accepted at the packing station (exact-match)
  2. When all required boxes arrive, the packing timer runs to completion and a deliverable package appears for the player to carry
  3. Walking the package into the delivery zone awards the configured cash, updates the cash UI, emits feedback, and spawns the next order
**Plans**: TBD

Plans:
- [ ] 04-01: Order template ScriptableObjects + initial set of templates
- [ ] 04-02: `OrderManager` (active order, exact-match tracking, completion → next order)
- [ ] 04-03: `PackingStation` (accept-required-only, packing timer, package spawn)
- [ ] 04-04: `DeliveryZone` (consume package, award cash, feedback hook)
- [ ] 04-05: `CurrencyManager` + cash UI display

### Phase 5: Buy an Upgrade
**Goal**: Player can spend cash at the upgrade station to purchase any of the six MVP upgrades, with effects applying immediately.
**Mode:** mvp
**Depends on**: Phase 4
**Requirements**: ECON-02, ECON-03, ECON-04, ECON-05, ECON-06, ECON-07
**Success Criteria** (what must be TRUE):
  1. Walking onto the upgrade station opens an upgrade UI listing all six upgrades with current level, next-level effect, and cost; affordability is visually distinct
  2. Buying an upgrade deducts cash, increments the upgrade level, and applies the effect within the same play session (carry capacity grows, character moves faster, order rewards multiply, packing timer shortens, shelf capacity grows, hire-worker becomes available)
  3. Levels and applied effects survive the immediate play session even before formal save/load (in-memory persistence); shelf-capacity upgrade visibly changes shelf storage
**Plans**: TBD

Plans:
- [ ] 05-01: Upgrade ScriptableObject definitions (id, max level, base cost, cost curve, effect handler)
- [ ] 05-02: `UpgradeManager` (purchase validation, cash deduction, level tracking, effect application)
- [ ] 05-03: Upgrade station UI with affordability state
- [ ] 05-04: Six upgrade effect handlers (carry, speed, order value, packing speed, shelf capacity, hire-worker gate)

### Phase 6: Save & Persist Progress
**Goal**: Closing and reopening the game preserves cash, upgrade levels, tutorial state, worker unlock, and warehouse progression.
**Mode:** mvp
**Depends on**: Phase 5
**Requirements**: SAVE-01
**Success Criteria** (what must be TRUE):
  1. Earning cash and buying upgrades, then quitting and relaunching the app, restores cash and upgrade levels exactly
  2. Save file uses a versioned JSON schema and a debug "reset save" hook is available for testing
  3. Save/load operations do not block gameplay (no visible hitch on autosave)
**Plans**: TBD

Plans:
- [ ] 06-01: `SaveManager` with versioned JSON schema and `ISaveService`
- [ ] 06-02: Wire cash, upgrade levels, tutorial state, worker unlock to save/load
- [ ] 06-03: Debug reset hook + autosave on key events

### Phase 7: Hire the First Worker
**Goal**: Buying the hire-worker upgrade spawns a worker that automates the dock → shelf transport loop.
**Mode:** mvp
**Depends on**: Phase 6
**Requirements**: WORK-01, WORK-02
**Success Criteria** (what must be TRUE):
  1. Before purchase, no worker exists; after purchase, a worker spawns and the unlock persists across restarts
  2. The worker autonomously moves to the loading dock, picks up boxes, walks to the shelf area, drops them, and repeats — without freezing when the player is in the path
  3. Worker uses the same box objects as the player (no duplicate inventory state); navigation handled via NavMesh or equivalent
**Plans**: TBD

Plans:
- [ ] 07-01: Shelf area as a drop target with capacity (consumed by ECON-07 upgrade)
- [ ] 07-02: NavMesh / pathing setup for worker
- [ ] 07-03: `WorkerAI` state machine (find box → pickup → walk to shelf → drop → repeat)
- [ ] 07-04: Hire-worker upgrade spawns/persists the worker

### Phase 8: Tutorial & Feel
**Goal**: A new save guides the player through the first loop in under 60 seconds, and core interactions feel satisfying.
**Mode:** mvp
**Depends on**: Phase 7
**Requirements**: TUT-01, TUT-02
**Success Criteria** (what must be TRUE):
  1. New save → arrows/highlights guide player to dock, packing station, delivery zone, and upgrade station in sequence; each step advances on completion; total time under 60 seconds for an attentive tester
  2. Pickup, drop, delivery, cash gain, and upgrade purchase each have a visible and/or audible feedback effect
  3. Tutorial completion persists; closing and reopening does not re-trigger the tutorial
**Plans**: TBD

Plans:
- [ ] 08-01: Tutorial state machine + step definitions
- [ ] 08-02: Arrow/indicator UI and highlight system
- [ ] 08-03: Feedback effects (particles + audio) for pickup, drop, delivery, cash, upgrade

### Phase 9: Analytics Wiring
**Goal**: All MVP analytics events flow through `IAnalyticsService`; gameplay continues even when analytics is disabled or fails.
**Mode:** mvp
**Depends on**: Phase 8
**Requirements**: TEL-01, TEL-02
**Success Criteria** (what must be TRUE):
  1. A debug-log analytics implementation receives `tutorial_started`, `tutorial_completed`, `first_order_completed`, `order_completed`, `upgrade_purchased`, `worker_hired`, `area_unlocked`, `ad_offer_shown`, `rewarded_ad_clicked`, `rewarded_ad_completed` in the expected sequence during a normal play session
  2. No gameplay system calls an analytics SDK directly — every emission goes through the wrapper
  3. Disabling analytics via configuration or simulating a failing implementation does not affect gameplay
**Plans**: TBD

Plans:
- [ ] 09-01: `IAnalyticsService` interface + debug-log implementation
- [ ] 09-02: `AnalyticsManager` (toggle, parameter validation, failure isolation)
- [ ] 09-03: Hook event sites in tutorial, orders, upgrades, workers, ads

### Phase 10: Fake Monetization Hooks
**Goal**: Rewarded-ad placements simulate completion and grant configured in-game rewards while emitting analytics.
**Mode:** mvp
**Depends on**: Phase 9
**Requirements**: ADS-01, ADS-02
**Success Criteria** (what must be TRUE):
  1. After a delivery, a "Watch ad to double cash" prompt can appear; accepting simulates completion and doubles the cash for that delivery; analytics events fire
  2. At the upgrade station, when the player is close to (but cannot afford) an upgrade, a "Watch ad for missing cash" offer can appear and grants missing cash up to a cap on simulated completion
  3. No fake-ad prompt appears during the tutorial; rewarded-ad UI lives behind `IAdService` and never references a real SDK
**Plans**: TBD

Plans:
- [ ] 10-01: `IAdService` interface + fake implementation (button + simulated completion)
- [ ] 10-02: Post-delivery double-cash placement
- [ ] 10-03: Missing-cash upgrade-station placement (cap, tutorial suppression)

### Phase 11: TestFlight Build Pipeline
**Goal**: A CLI command produces an iOS archive ready for upload to TestFlight.
**Mode:** mvp
**Depends on**: Phase 10
**Requirements**: BUILD-01
**Success Criteria** (what must be TRUE):
  1. `Unity -batchmode -quit -projectPath . -buildTarget iOS -executeMethod BuildScript.BuildIOS -logFile -` produces an Xcode project in a known output directory
  2. The Xcode project archives in Xcode (or via `xcodebuild`) and uploads to TestFlight without manual scene fixups
  3. A short README in `/build` documents signing, bundle identifier, and the manual upload step
**Plans**: TBD

Plans:
- [ ] 11-01: `BuildScript` editor class with iOS build entry point
- [ ] 11-02: Player settings, bundle id, signing config
- [ ] 11-03: Build documentation + smoke run

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9 → 10 → 11

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Project Bootstrap & Empty Warehouse Scene | 0/3 | Not started | - |
| 2. Walk Around the Warehouse | 0/3 | Not started | - |
| 3. Pick Up & Carry Boxes | 0/3 | Not started | - |
| 4. Complete an Order | 0/5 | Not started | - |
| 5. Buy an Upgrade | 0/4 | Not started | - |
| 6. Save & Persist Progress | 0/3 | Not started | - |
| 7. Hire the First Worker | 0/4 | Not started | - |
| 8. Tutorial & Feel | 0/3 | Not started | - |
| 9. Analytics Wiring | 0/3 | Not started | - |
| 10. Fake Monetization Hooks | 0/3 | Not started | - |
| 11. TestFlight Build Pipeline | 0/3 | Not started | - |
