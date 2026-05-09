# Warehouse Master — Roadmap and Future Phases Spec

## 1. Overview

This document defines the phased roadmap after the MVP. The roadmap should not distract from MVP validation. Future phases should only be started after the MVP demonstrates a fun and understandable core loop.

## 2. Phase 1 — MVP Prototype

### Goal

Validate the core warehouse loop.

### Scope

- One warehouse.
- Three box types.
- Basic orders.
- Basic upgrades.
- One worker type.
- Local save.
- Tutorial.
- Analytics.
- Fake rewarded ads.

### Success Criteria

- First order completed in under 60 seconds.
- First upgrade purchased in under 2 minutes.
- First worker hired in under 5 minutes.
- First session lasts 8 to 10 minutes or more.

## 3. Phase 2 — Internal Playtest

### Goal

Find friction and improve early gameplay clarity.

### Scope

- TestFlight distribution.
- Internal testers.
- Tutorial tuning.
- Economy tuning.
- UX polish.
- Bug fixing.

### Key Questions

- Do players know where to go?
- Are station roles clear?
- Does movement feel good?
- Is the first worker exciting?
- Does progression feel too fast or too slow?

## 4. Phase 3 — Soft Launch Candidate

### Goal

Prepare for limited market release and initial monetization testing.

### Scope

- Production ad SDK integration.
- Rewarded ads.
- Limited interstitial ads.
- Remove Ads IAP.
- Starter Pack IAP.
- 30 to 60 minutes of content.
- Offline earnings.
- Daily rewards.
- Improved store listing.
- Basic creative testing.

### Metrics

- D1 retention.
- D3 retention.
- D7 retention.
- Average session length.
- Rewarded ad opt-in rate.
- Ads per DAU.
- ARPDAU.
- Crash-free sessions.
- CPI from test campaigns.

## 5. Phase 4 — Hybrid-Casual Expansion

### Goal

Turn the prototype into a deeper casual product.

### Scope

- Multiple warehouse zones.
- Additional worker roles.
- Conveyor belts.
- Forklifts.
- Sorting machines.
- Special box types.
- Express orders.
- Gems.
- Cosmetics.
- Piggy bank.
- Remote Config.
- A/B testing.

### New Systems

#### Special Boxes

| Box Type | Behavior |
|---|---|
| Fragile | Higher reward, cannot be mishandled |
| Frozen | Must be delivered quickly |
| Heavy | Uses extra carry capacity |
| Gold | Bonus reward |
| Mystery | Random reward |

#### Special Orders

| Order Type | Behavior |
|---|---|
| Express | Timer-based, high reward |
| Bulk | Large quantity, high cash payout |
| Combo | Rewards consecutive completion |
| VIP | Rare high-value order |

## 6. Phase 5 — Live Operations

### Goal

Improve long-term retention and monetization.

### Scope

- Temporary events.
- Themed warehouses.
- Seasonal skins.
- Daily missions.
- Weekly missions.
- Login calendar.
- Limited-time offers.
- Lightweight battle pass.
- Event currency.

### Event Examples

| Event | Description |
|---|---|
| Holiday Rush | Complete seasonal gift orders |
| Port Expansion | Temporary shipping container map |
| Robot Week | Unlock temporary robot boosts |
| Black Friday Rush | High-volume express orders |

## 7. Phase 6 — Scale and User Acquisition

### Goal

Scale user acquisition only if retention and monetization support it.

### Scope

- MMP integration such as AppsFlyer or Adjust.
- SKAN setup.
- Creative testing pipeline.
- TikTok ads.
- Meta ads.
- Apple Search Ads.
- AppLovin campaigns.
- Country-by-country testing.
- ROAS tracking.

### Creative Strategy

Creative concepts:

1. Satisfying box stacking.
2. Before/after warehouse upgrade.
3. Chaotic warehouse cleanup.
4. Rapid upgrade dopamine.
5. Express order countdown.

## 8. Deferred Feature Candidates

### Layout Editing

Allow the player to place shelves, conveyors, and machines.

Risk: Adds complexity. Should only be added if it improves retention.

### Multiple Warehouses

Unlock new locations such as:

- Airport warehouse.
- Port warehouse.
- Supermarket warehouse.
- Factory warehouse.
- E-commerce fulfillment center.

### Worker Management

Add worker stats, roles, and managers.

Potential roles:

- Loader.
- Packer.
- Dispatcher.
- Forklift driver.
- Robot technician.
- Manager.

### Cosmetic Customization

Potential cosmetics:

- Player skins.
- Worker uniforms.
- Warehouse themes.
- Truck skins.
- Box skins.

## 9. Roadmap Decision Gates

### Gate 1 — Continue After MVP

Continue only if:

- Players understand the loop.
- First worker unlock feels good.
- First session length is promising.
- Testers want another area or more upgrades.

### Gate 2 — Soft Launch

Soft launch only if:

- MVP loop is stable.
- Tutorial completion is high.
- Early retention signals are acceptable.
- Crash rate is low.
- Monetization hooks do not harm gameplay.

### Gate 3 — Scale UA

Scale only if:

- Retention supports acquisition.
- ARPDAU supports CPI.
- Creatives generate viable CTR/CPI.
- Monetization does not significantly reduce retention.

## 10. Long-Term Product Vision

Warehouse Master should evolve from a simple manual warehouse game into an automation fantasy:

- Start by carrying boxes manually.
- Add workers.
- Add machines.
- Add conveyors.
- Add robots.
- Add multiple facilities.
- Add events and seasonal content.

The product should remain simple to play but increasingly satisfying to optimize.
