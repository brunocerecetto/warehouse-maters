# Warehouse Master — Product Requirements Spec

## 1. Product Summary

**Warehouse Master** is a free-to-play iPhone game where the player operates and expands a small warehouse. The player receives boxes, moves them through a simple logistics workflow, prepares customer orders, delivers packages, earns cash, purchases upgrades, and eventually automates parts of the warehouse with workers and machines.

The game should be built as a **hybrid-casual idle/tycoon** experience with satisfying moment-to-moment interactions and visible progression.

## 2. Product Goal

Create a playable prototype that validates whether the core warehouse loop is engaging for 5 to 10 minutes.

The prototype is not expected to validate full monetization, long-term retention, or live operations. It must validate the base interaction model, clarity, and early progression.

## 3. Target Platform

- Primary platform: iPhone
- Orientation: Portrait preferred for casual mobile usability
- Engine: Unity
- Input: Single-thumb virtual joystick
- Monetization model: Free-to-play, ads + in-app purchases in later phases

## 4. Target Player

The target player is a casual mobile player who enjoys simple progression games, idle tycoons, satisfying organization mechanics, and short play sessions.

The game should not require gaming expertise. A new player should understand the objective within the first 30 seconds.

## 5. Core Fantasy

The player starts with a small, inefficient warehouse and turns it into a fast, automated logistics operation.

The emotional arc should be:

1. I am doing everything manually.
2. I earn money by completing orders.
3. I buy upgrades and become faster.
4. I hire help.
5. The warehouse starts running on its own.
6. I unlock better systems, more space, and bigger rewards.

## 6. MVP Scope

The MVP includes:

- One warehouse map.
- One player character.
- Three box types.
- One order queue.
- One packing station.
- One delivery zone.
- One upgrade station.
- One worker type.
- Local save data.
- Basic analytics events.
- Tutorial guidance.
- Simulated rewarded ad placement.

## 7. Non-Goals for MVP

The MVP does not include:

- Real-money purchases.
- Real production ads.
- Multiplayer.
- Backend services.
- Cloud save.
- Leaderboards.
- Multiple warehouses.
- Battle pass.
- Social login.
- Complex worker management.
- Advanced warehouse layout editing.
- Complex physics.
- Realistic logistics simulation.

## 8. Primary Success Criteria

The MVP is successful if internal testers can:

- Complete the first order in under 60 seconds.
- Buy the first upgrade in under 2 minutes.
- Hire the first worker in under 5 minutes.
- Understand the core loop without lengthy explanation.
- Play for at least 8 to 10 minutes without losing interest.

## 9. Key User Stories

### US-001 — Move Around the Warehouse

As a player, I want to move my character with one thumb so that I can pick up boxes and deliver them to the right stations.

### US-002 — Pick Up Boxes

As a player, I want boxes to be picked up automatically when I approach them so that the game feels simple and fast.

### US-003 — Complete Orders

As a player, I want to deliver the correct boxes to a packing station so that I can complete orders and earn cash.

### US-004 — Buy Upgrades

As a player, I want to spend cash on upgrades so that my warehouse becomes faster and more efficient.

### US-005 — Hire a Worker

As a player, I want to hire a worker so that part of the warehouse becomes automated and I feel progression.

### US-006 — Save Progress

As a player, I want my progress to be saved locally so that I can leave and return without losing upgrades.

## 10. Design Principles

### Simplicity First

The player should not need menus to perform the main loop. Movement and proximity interactions should drive most actions.

### Visible Progression

Every upgrade should produce a visible or immediately felt improvement.

### Short Feedback Loops

The player should earn cash and make progress frequently during the first session.

### Satisfying Interactions

Picking up boxes, stacking them, packing orders, delivering packages, and receiving cash should have visual and audio feedback.

### Monetization-Friendly, Not Monetization-First

The game should be designed with natural rewarded ad opportunities, but the MVP should validate fun before production monetization.

## 11. Product Risks

| Risk | Description | Mitigation |
|---|---|---|
| The loop feels like work | Moving boxes can become tedious | Add fast feedback, upgrades, automation, and short goals |
| The player gets lost | The warehouse flow may not be obvious | Use arrows, highlights, and clear station visuals |
| Progression is too slow | The game may feel grindy | Give frequent early upgrades |
| Automation removes gameplay | Workers may make the player irrelevant | Workers assist but do not fully replace active play |
| Ads harm retention | Poor ad timing may frustrate users | Start with rewarded ads and delay interstitials |

## 12. Open Product Questions

- Should the game use portrait or landscape orientation? Current recommendation: portrait.
- Should the player carry boxes directly or push a cart? Current recommendation: carry boxes visually stacked.
- Should orders require exact matching from day one? Current recommendation: yes, but with only three box colors.
- Should the first worker unlock before or after the first area expansion? Current recommendation: before area expansion.
