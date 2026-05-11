# Requirements: Warehouse Master

**Defined:** 2026-05-09
**Core Value:** The 5-minute warehouse loop must feel satisfying — pick up boxes, complete an order, earn cash, buy an upgrade, watch the warehouse get faster.

## v1 Requirements

Requirements for the MVP prototype. Each maps to roadmap phases.

### Bootstrap

- [x] **BOOT-01**: Unity project initialized for iOS with URP and `Assets/_Project/...` folder structure (Scripts subfolders per system, ScriptableObjects subfolders per data type) — Plan 01-01, 2026-05-10
- [x] **BOOT-02**: `Warehouse_MVP` scene created with all required objects (player spawn, loading dock, packing station, delivery zone, upgrade station, shelf area, worker spawn, camera rig, UI canvas, event system, `GameManager`) — Plan 01-02, 2026-05-10

### Movement

- [ ] **MOVE-01**: Player can move with a one-thumb virtual joystick; movement is camera-relative, configurable speed, and blocked by walls and stations
- [ ] **MOVE-02**: Fixed isometric/top-down camera rig keeps player and nearby interactive zones visible without manual control, with UI not obscuring the play area

### Boxes

- [ ] **BOX-01**: Three box types (red, blue, yellow) defined as data-driven ScriptableObjects with id, prefab, color, base value; order logic references them by type
- [ ] **BOX-02**: Player auto-picks up boxes when entering pickup range, up to a configurable carry capacity (default 3); carried boxes render as a visible stack on the character
- [ ] **BOX-03**: Player auto-drops boxes at valid stations (e.g., packing station accepts only required types); visual stack updates immediately
- [ ] **BOX-04**: Loading-dock spawner generates boxes over time with configurable rate, type distribution, and dock capacity ceiling

### Orders

- [ ] **ORD-01**: Order templates defined as ScriptableObjects with required box quantities by type, base cash reward, and weight
- [ ] **ORD-02**: One active order at a time tracks exact-match requirements (e.g., "2 red + 1 blue" requires those specific types); progress and remaining counts are visible in UI
- [ ] **ORD-03**: Packing station accepts only currently-required boxes, runs a configurable packing timer when requirements are met, and emits a deliverable package
- [ ] **ORD-04**: Delivery zone consumes the carried package, awards configured cash, triggers visual/audio feedback, and starts the next order

### Economy

- [ ] **ECON-01**: `CurrencyManager` tracks player cash; UI updates immediately on add/spend; cash persists via local save
- [ ] **ECON-02**: Upgrade station opens an upgrade UI, validates affordability, deducts cash, applies effects immediately, and persists upgrade levels
- [ ] **ECON-03**: Carry-capacity upgrade increases max carried boxes; new value applies immediately and persists
- [ ] **ECON-04**: Movement-speed upgrade increases player speed; effect is immediately noticeable and persists
- [ ] **ECON-05**: Order-value upgrade multiplies cash earned per order; reward UI reflects multiplier and persists
- [ ] **ECON-06**: Packing-speed upgrade reduces packing duration; effect applies to future packs and persists
- [ ] **ECON-07**: Shelf-capacity upgrade increases shelf storage; new capacity applies immediately and persists

### Workers

- [ ] **WORK-01**: Hire-worker upgrade unlocks the first automated worker; unlock state is saved; worker does not appear pre-purchase
- [ ] **WORK-02**: Basic worker AI loops dock → pickup → shelf → drop autonomously; worker uses the same box objects as the player and does not block the player excessively

### Tutorial & Feedback

- [ ] **TUT-01**: Tutorial flow guides a new save through first pickup, first order, first delivery, first upgrade in under 60 seconds; completion is saved and never re-triggers
- [ ] **TUT-02**: Feedback effects (visual and/or audio) for pickup, delivery, cash gain, and upgrade purchase

### Save

- [ ] **SAVE-01**: Local save persists cash, upgrade levels, tutorial state, worker unlock, warehouse progression level; versioned save schema; reset capability for testing

### Analytics

- [ ] **TEL-01**: `IAnalyticsService` wrapper centralizes all event emission; can be disabled; gameplay continues if analytics fails
- [ ] **TEL-02**: MVP events emitted: `tutorial_started`, `tutorial_completed`, `first_order_completed`, `order_completed`, `upgrade_purchased`, `worker_hired`, `area_unlocked`, `ad_offer_shown`, `rewarded_ad_clicked`, `rewarded_ad_completed`

### Monetization (Simulated)

- [ ] **ADS-01**: Fake rewarded-ad flow shows a "Watch Ad" prompt, simulates completion on accept, grants configured reward, emits analytics
- [ ] **ADS-02**: Missing-cash placement: when player is close to an upgrade cost, fake-ad offer can grant missing cash up to a cap; suppressed during tutorial

### Build

- [ ] **BUILD-01**: iOS TestFlight-ready build via Unity batch-mode CLI (`-buildTarget iOS -executeMethod BuildScript.BuildIOS`); Xcode project archives and uploads cleanly

## v2 Requirements

Deferred to soft launch. Tracked but not in current roadmap.

### Real Monetization

- **ADS-V2-01**: Real rewarded ads via AdMob (or mediation: AppLovin MAX / Unity LevelPlay)
- **ADS-V2-02**: Interstitial ads at natural breaks with cooldown rules
- **IAP-V2-01**: "Remove Ads" IAP product (`$2.99–$4.99`)
- **IAP-V2-02**: "Starter Pack" IAP product

### Retention & Live Ops

- **RETN-V2-01**: Offline earnings on app return
- **RETN-V2-02**: Daily reward / login calendar
- **RETN-V2-03**: Second warehouse zone unlockable
- **RETN-V2-04**: Goals/objectives UI ("Complete 10 orders")

### Gameplay Depth

- **BOX-V2-01**: Special box types (fragile, frozen, heavy, premium, express)
- **ORD-V2-01**: Express orders with timer + bonus reward
- **ORD-V2-02**: Order chaining combos
- **WORK-V2-01**: Specialized worker types (packer, dispatcher, forklift)

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Multiplayer | Adds backend, networking, matchmaking — out of MVP scope |
| Backend services / cloud save | Local save sufficient to validate fun |
| Real ads / IAP / mediation / SKAN | Defer to soft launch after fun is validated |
| Leaderboards, battle pass, social login | Live-ops phase, not MVP |
| Multiple warehouses, world themes | Phase 3+ expansion |
| Multiple currencies (gems, tickets) | Cash only for MVP — multi-currency confuses early players |
| Complex physics, drivable vehicles, manual inventory | Outside MVP design scope |
| Skins, decorations | Not core to loop validation |
| Layout editor, conveyor belts, drones, forklifts | Phase 3+ automation tier |
| Custom art beyond temporary low-poly | Validate gameplay before art investment |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| BOOT-01 | Phase 1 | Complete |
| BOOT-02 | Phase 1 | Complete |
| MOVE-01 | Phase 2 | Pending |
| MOVE-02 | Phase 2 | Pending |
| BOX-01 | Phase 3 | Pending |
| BOX-02 | Phase 3 | Pending |
| BOX-04 | Phase 3 | Pending |
| BOX-03 | Phase 4 | Pending |
| ORD-01 | Phase 4 | Pending |
| ORD-02 | Phase 4 | Pending |
| ORD-03 | Phase 4 | Pending |
| ORD-04 | Phase 4 | Pending |
| ECON-01 | Phase 4 | Pending |
| ECON-02 | Phase 5 | Pending |
| ECON-03 | Phase 5 | Pending |
| ECON-04 | Phase 5 | Pending |
| ECON-05 | Phase 5 | Pending |
| ECON-06 | Phase 5 | Pending |
| ECON-07 | Phase 5 | Pending |
| SAVE-01 | Phase 6 | Pending |
| WORK-01 | Phase 7 | Pending |
| WORK-02 | Phase 7 | Pending |
| TUT-01 | Phase 8 | Pending |
| TUT-02 | Phase 8 | Pending |
| TEL-01 | Phase 9 | Pending |
| TEL-02 | Phase 9 | Pending |
| ADS-01 | Phase 10 | Pending |
| ADS-02 | Phase 10 | Pending |
| BUILD-01 | Phase 11 | Pending |

**Coverage:**
- v1 requirements: 29 total
- Mapped to phases: 29
- Unmapped: 0 ✓

---
*Requirements defined: 2026-05-09*
*Last updated: 2026-05-09 after initial definition*
