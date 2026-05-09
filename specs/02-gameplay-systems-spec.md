# Warehouse Master — Gameplay Systems Spec

## 1. Overview

This document specifies the core gameplay systems for the Warehouse Master MVP.

The MVP gameplay is built around a single loop:

1. Boxes arrive at the loading dock.
2. The player picks up boxes.
3. The player delivers boxes to the packing station.
4. Orders are completed.
5. Packages are delivered.
6. Cash is awarded.
7. Cash is spent on upgrades.
8. The warehouse becomes faster and more automated.

## 2. Player Movement System

### Requirements

- The player moves using a virtual joystick.
- Movement must support mobile touch input.
- Movement direction must be relative to camera orientation.
- The player must stop when colliding with blocking objects.
- Movement speed must be configurable.
- Movement speed must be upgradeable.

### Acceptance Criteria

- Given the player drags the joystick upward, the character moves toward the top of the screen.
- Given the player releases the joystick, the character stops moving.
- Given movement speed is upgraded, the player moves faster immediately.
- Given the player collides with a wall or station, the player does not pass through it.

## 3. Carry System

### Description

The player can carry a limited number of boxes. Boxes are picked up automatically when the player enters a pickup zone and has available capacity.

### Requirements

- The player starts with a carry capacity of 3 boxes.
- Carry capacity must be configurable.
- Carry capacity must be upgradeable.
- Carried boxes must be visually stacked on the player.
- The stack must update when boxes are picked up or dropped.
- The player cannot pick up boxes beyond current capacity.

### Acceptance Criteria

- Given the player has empty capacity and enters the loading dock, boxes are added to the carried stack.
- Given the player is at max capacity, no additional boxes are picked up.
- Given the player drops boxes at a valid station, the visual stack decreases.
- Given carry capacity is upgraded, the new max capacity is used immediately.

## 4. Box System

### MVP Box Types

| Box Type | Visual Identifier | Gameplay Role |
|---|---|---|
| Red Box | Red color | Basic order ingredient |
| Blue Box | Blue color | Basic order ingredient |
| Yellow Box | Yellow color | Basic order ingredient |

### Requirements

- Each box must have a type identifier.
- Each box must have a visual representation matching its type.
- Box types must be data-driven.
- The system must support adding future box types without rewriting order logic.

### Acceptance Criteria

- Given an order requires red boxes, only red boxes count toward the red requirement.
- Given a new box type is defined in data, the order system can reference it.

## 5. Loading Dock System

### Description

The loading dock generates boxes for the player and workers to collect.

### Requirements

- Boxes spawn at the loading dock over time.
- The loading dock has a maximum visible capacity.
- Spawn rate must be configurable.
- Spawned box types must be configurable.
- Workers and players can both collect boxes from the loading dock.

### Acceptance Criteria

- Given the loading dock is not full, boxes spawn over time.
- Given the loading dock is full, no additional boxes spawn until space is available.
- Given a player or worker collects a box, the dock count decreases.

## 6. Packing Station System

### Description

The packing station receives boxes required by the active order. Once requirements are met, it generates a package ready for delivery.

### Requirements

- The packing station accepts only boxes required by the current order.
- The packing station displays order progress.
- Packing must take a configurable amount of time.
- Packing speed must be upgradeable.
- Once packing completes, a package appears for delivery.

### Acceptance Criteria

- Given the active order requires 2 red boxes and 1 blue box, the station tracks those requirements separately.
- Given all requirements are met, packing begins.
- Given packing finishes, a package appears.
- Given packing speed is upgraded, the packing duration decreases.

## 7. Order System

### Description

The order system generates simple box requirements and tracks completion.

### MVP Order Rules

- Orders require 1 to 5 total boxes.
- Orders can include one or more box types.
- Only one active order is required for MVP.
- A visible queue can be added later.

### Requirements

- Orders must be generated from configurable templates.
- Each order must define required box quantities by type.
- Each order must define a cash reward.
- Completing an order generates a package.
- Delivering the package grants cash.

### Example Orders

| Order ID | Requirements | Reward |
|---|---|---:|
| order_001 | 2 red | 10 cash |
| order_002 | 1 red, 1 blue | 15 cash |
| order_003 | 2 yellow, 1 blue | 25 cash |
| order_004 | 2 red, 2 blue, 1 yellow | 50 cash |

### Acceptance Criteria

- Given an order is active, the UI displays required box types and quantities.
- Given the player provides the required boxes, the order becomes ready for packing.
- Given the package is delivered, the player receives the configured reward.
- Given the order is completed, a new order is generated.

## 8. Delivery Zone System

### Description

The delivery zone converts packed packages into cash rewards.

### Requirements

- Packages can be delivered by walking into the delivery zone.
- The delivery zone must grant cash for completed packages.
- Delivery must trigger visual and audio feedback.
- Delivery must trigger analytics events.

### Acceptance Criteria

- Given the player carries a package and enters the delivery zone, the package is removed and cash is added.
- Given cash is added, the UI updates immediately.
- Given a package is delivered, the `order_completed` analytics event is emitted.

## 9. Upgrade Station System

### MVP Upgrades

| Upgrade | Effect |
|---|---|
| Carry Capacity | Increases player max boxes |
| Movement Speed | Increases player speed |
| Order Value | Increases cash earned per order |
| Packing Speed | Reduces packing duration |
| Shelf Capacity | Increases storage capacity |
| Hire Worker | Adds one automated worker |

### Requirements

- Upgrades must be purchased using cash.
- Upgrade costs must be configurable.
- Upgrade effects must apply immediately.
- Upgrade levels must be saved locally.
- Locked upgrades must be visually distinct.

### Acceptance Criteria

- Given the player has enough cash, the player can buy an upgrade.
- Given the player does not have enough cash, the purchase is blocked.
- Given an upgrade is purchased, cash is deducted and the effect is applied.
- Given the game is restarted, purchased upgrade levels persist.

## 10. Worker System

### MVP Worker Behavior

The first worker moves boxes from the loading dock to shelf storage.

### Requirements

- Workers are unlocked through the Hire Worker upgrade.
- Workers move automatically.
- Workers have configurable speed and carry capacity.
- Workers must use the same box objects as the player.
- Workers must not block the player excessively.

### Acceptance Criteria

- Given the player buys Hire Worker, a worker appears in the warehouse.
- Given boxes exist at the loading dock, the worker moves toward them.
- Given the worker reaches the loading dock, it picks up boxes.
- Given the worker carries boxes, it moves to shelf storage.
- Given the worker reaches shelf storage, it drops boxes.

## 11. Tutorial System

### Requirements

The tutorial must guide the player through:

1. Moving to the loading dock.
2. Picking up boxes.
3. Moving to the packing station.
4. Completing the first order.
5. Delivering the first package.
6. Buying the first upgrade.

### Acceptance Criteria

- Given a new save, the tutorial starts automatically.
- Given the player completes a tutorial step, the next step appears.
- Given the tutorial is completed, it does not appear again on restart.

## 12. Save System

### MVP Save Data

- Cash amount.
- Upgrade levels.
- Tutorial completion state.
- Worker unlock state.
- Basic warehouse progression.

### Acceptance Criteria

- Given the player earns cash and closes the app, cash persists after reopening.
- Given the player buys upgrades and closes the app, upgrades persist after reopening.
- Given the tutorial is completed, it remains completed after reopening.
