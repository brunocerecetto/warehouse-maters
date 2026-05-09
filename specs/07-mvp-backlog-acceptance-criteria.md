# Warehouse Master — MVP Backlog and Acceptance Criteria

## 1. Overview

This document defines the MVP backlog for spec-driven development. Each item should be implemented only when its acceptance criteria are clear.

## 2. Epic 1 — Player Movement

### WM-001 — Implement Virtual Joystick Movement

**Description:** Implement one-thumb player movement using a virtual joystick.

**Acceptance Criteria:**

- Player moves in the direction of joystick input.
- Player stops when joystick is released.
- Movement speed is configurable.
- Player cannot pass through walls or blocking stations.
- Movement works in an iPhone build.

### WM-002 — Implement Camera Rig

**Description:** Add fixed isometric/top-down camera suitable for mobile play.

**Acceptance Criteria:**

- Camera shows all nearby interactive zones clearly.
- Player remains visible during movement.
- Camera does not require player control.
- UI does not obscure the main interaction area.

## 3. Epic 2 — Boxes and Carrying

### WM-003 — Define Box Types

**Description:** Create data-driven red, blue, and yellow box types.

**Acceptance Criteria:**

- Each box type has a unique ID.
- Each box type has a distinct visual.
- Order logic can distinguish box types.

### WM-004 — Implement Box Pickup

**Description:** Player automatically picks up boxes when entering pickup range.

**Acceptance Criteria:**

- Player picks up boxes when capacity is available.
- Player cannot exceed carry capacity.
- Picked boxes are removed from the source area.
- Visual stack updates after pickup.

### WM-005 — Implement Box Drop

**Description:** Player automatically drops boxes at valid stations.

**Acceptance Criteria:**

- Boxes can be dropped at the packing station when required.
- Invalid boxes are not consumed by the current order.
- Visual stack updates after drop.

## 4. Epic 3 — Loading Dock

### WM-006 — Implement Loading Dock Spawner

**Description:** Spawn boxes at the loading dock over time.

**Acceptance Criteria:**

- Boxes spawn until dock capacity is reached.
- Spawn rate is configurable.
- Spawned box types are configurable.
- Boxes can be collected by the player.

## 5. Epic 4 — Orders and Packing

### WM-007 — Implement Order Templates

**Description:** Define configurable order templates.

**Acceptance Criteria:**

- Orders can require multiple box types.
- Orders define cash rewards.
- Active order appears in UI.

### WM-008 — Implement Active Order Logic

**Description:** Track active order requirements and progress.

**Acceptance Criteria:**

- Required quantities are tracked correctly.
- Delivered required boxes update progress.
- Order becomes ready when all requirements are met.

### WM-009 — Implement Packing Station

**Description:** Convert completed order requirements into a package.

**Acceptance Criteria:**

- Packing begins when order requirements are complete.
- Packing duration is configurable.
- Completed package appears after timer finishes.
- Packing speed upgrade affects duration.

### WM-010 — Implement Delivery Zone

**Description:** Deliver packages and grant cash rewards.

**Acceptance Criteria:**

- Player can deliver a package at the delivery zone.
- Cash reward is added immediately.
- A new order starts after delivery.
- Delivery feedback is shown.

## 6. Epic 5 — Economy and Upgrades

### WM-011 — Implement Cash System

**Description:** Track and display player cash.

**Acceptance Criteria:**

- Cash increases when orders are delivered.
- Cash decreases when upgrades are purchased.
- UI updates when cash changes.
- Cash value is saved locally.

### WM-012 — Implement Upgrade Station

**Description:** Allow player to buy upgrades using cash.

**Acceptance Criteria:**

- Upgrade station opens upgrade UI.
- Player can buy affordable upgrades.
- Player cannot buy unaffordable upgrades.
- Upgrade levels are saved.

### WM-013 — Implement Carry Capacity Upgrade

**Acceptance Criteria:**

- Upgrade increases max carried boxes.
- New capacity is applied immediately.
- Visual/UI capacity reflects new value.

### WM-014 — Implement Movement Speed Upgrade

**Acceptance Criteria:**

- Upgrade increases player movement speed.
- Effect is immediately noticeable.
- Speed value persists after restart.

### WM-015 — Implement Order Value Upgrade

**Acceptance Criteria:**

- Upgrade increases cash reward from orders.
- UI/reward values reflect the updated multiplier.
- Effect persists after restart.

### WM-016 — Implement Packing Speed Upgrade

**Acceptance Criteria:**

- Upgrade reduces packing duration.
- Effect applies to future packing operations.
- Effect persists after restart.

## 7. Epic 6 — Worker Automation

### WM-017 — Implement Hire Worker Upgrade

**Description:** Unlock the first automated worker.

**Acceptance Criteria:**

- Worker appears after purchase.
- Worker unlock state is saved.
- Worker does not appear before purchase.

### WM-018 — Implement Basic Worker AI

**Description:** Worker transports boxes from loading dock to shelves.

**Acceptance Criteria:**

- Worker moves to loading dock when boxes are available.
- Worker picks up boxes.
- Worker moves to shelf area.
- Worker drops boxes.
- Worker repeats behavior automatically.

## 8. Epic 7 — Tutorial and UX

### WM-019 — Implement Tutorial Flow

**Description:** Guide new players through first order and first upgrade.

**Acceptance Criteria:**

- Tutorial starts on new save.
- Tutorial shows clear direction to next target.
- Tutorial advances after each completed step.
- Tutorial completion is saved.

### WM-020 — Implement Feedback Effects

**Description:** Add basic feedback for pickup, delivery, cash, and upgrades.

**Acceptance Criteria:**

- Pickup has visual or audio feedback.
- Delivery has visual or audio feedback.
- Cash gain has visual feedback.
- Upgrade purchase has visual feedback.

## 9. Epic 8 — Save and Analytics

### WM-021 — Implement Local Save

**Acceptance Criteria:**

- Cash persists after restart.
- Upgrade levels persist after restart.
- Tutorial state persists after restart.
- Worker unlock persists after restart.

### WM-022 — Implement Analytics Wrapper

**Acceptance Criteria:**

- All events go through `AnalyticsManager`.
- Analytics can be disabled.
- Gameplay continues if analytics fails.

### WM-023 — Emit MVP Analytics Events

**Acceptance Criteria:**

- Tutorial events are emitted.
- First order event is emitted.
- First upgrade event is emitted.
- First worker event is emitted.
- Order completion events are emitted.

## 10. Epic 9 — Fake Monetization Hooks

### WM-024 — Implement Fake Rewarded Ad Flow

**Description:** Simulate a rewarded ad without SDK integration.

**Acceptance Criteria:**

- Fake ad prompt can be shown.
- Accepting prompt simulates completion.
- Configured reward is granted.
- Analytics events are emitted.

### WM-025 — Add Missing Cash Reward Placement

**Acceptance Criteria:**

- If player is close to an upgrade cost, fake ad offer can appear.
- Completing fake ad grants missing cash up to a cap.
- Offer does not appear during tutorial.

## 11. Definition of Done

A backlog item is done only when:

- Acceptance criteria pass.
- Feature works in an iPhone build or simulator.
- Relevant configuration is data-driven where specified.
- Relevant analytics events are emitted.
- No critical gameplay-blocking bugs remain.
- The implementation does not introduce backend or production SDK dependencies unless explicitly required.
