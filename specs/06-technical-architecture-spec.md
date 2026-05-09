# Warehouse Master — Technical Architecture Spec

## 1. Overview

Warehouse Master should be implemented in Unity with a modular architecture that separates gameplay systems, data configuration, persistence, analytics, ads, and IAP integration boundaries.

The MVP must be simple enough to build quickly but structured enough to support soft launch and later monetization.

## 2. Technology Stack

| Area | Recommendation |
|---|---|
| Engine | Unity |
| Language | C# |
| Platform | iOS first |
| Analytics | Firebase Analytics through wrapper |
| Ads | Fake ads in MVP; mediation later |
| IAP | Placeholder in MVP; Unity IAP / StoreKit later |
| Save Data | Local JSON or PlayerPrefs wrapper |
| Backend | None for MVP |
| Art | Low-poly 3D assets |

## 3. Project Structure

Recommended Unity folder structure:

```text
Assets/
  _Project/
    Art/
    Audio/
    Materials/
    Prefabs/
    Scenes/
    Scripts/
      Core/
      Player/
      Boxes/
      Orders/
      Stations/
      Upgrades/
      Workers/
      Economy/
      Save/
      Analytics/
      Monetization/
      UI/
    ScriptableObjects/
      Boxes/
      Orders/
      Upgrades/
      Workers/
      Economy/
    Settings/
```

## 4. Core Systems

### `GameManager`

Responsible for game initialization and high-level state.

Responsibilities:

- Initialize services.
- Load save data.
- Start tutorial or normal gameplay.
- Coordinate game state transitions.

### `PlayerController`

Responsible for player movement.

Responsibilities:

- Read virtual joystick input.
- Move player character.
- Apply movement speed modifiers.
- Handle collision-safe movement.

### `CarrySystem`

Responsible for carrying boxes and packages.

Responsibilities:

- Track carried items.
- Enforce capacity.
- Update visual stack.
- Add and remove carried items.

### `BoxSpawner`

Responsible for generating boxes at the loading dock.

Responsibilities:

- Spawn boxes over time.
- Enforce dock capacity.
- Select box type from configured list.

### `OrderManager`

Responsible for order generation and lifecycle.

Responsibilities:

- Generate active order.
- Track order requirements.
- Mark order ready for packing.
- Start next order after completion.

### `PackingStation`

Responsible for accepting boxes and producing packages.

Responsibilities:

- Accept valid boxes.
- Track box requirements.
- Run packing timer.
- Spawn package.

### `DeliveryZone`

Responsible for converting packages into rewards.

Responsibilities:

- Detect package delivery.
- Grant cash reward.
- Emit completion events.

### `UpgradeManager`

Responsible for upgrade definitions and purchases.

Responsibilities:

- Load upgrade configs.
- Validate purchase affordability.
- Deduct cash.
- Apply upgrade effects.
- Persist upgrade levels.

### `WorkerAI`

Responsible for automated worker behavior.

Responsibilities:

- Select task.
- Move to loading dock.
- Pick up boxes.
- Move to shelf.
- Drop boxes.

### `CurrencyManager`

Responsible for cash state.

Responsibilities:

- Add cash.
- Spend cash.
- Validate affordability.
- Notify UI.
- Persist cash.

### `SaveManager`

Responsible for persistence.

Responsibilities:

- Save local state.
- Load local state.
- Version save files.
- Provide reset capability for testing.

### `AnalyticsManager`

Responsible for analytics abstraction.

Responsibilities:

- Send events.
- Validate event parameters.
- Support disabled mode.
- Prevent analytics failures from affecting gameplay.

### `AdManager`

MVP fake ad implementation.

Responsibilities:

- Show fake rewarded ad prompts.
- Simulate ad completion.
- Grant configured rewards.
- Emit monetization analytics.

### `IAPManager`

MVP placeholder implementation.

Responsibilities:

- Define product IDs.
- Expose purchase interface.
- Remain disabled until real IAP integration.

## 5. Data-Driven Configuration

Use ScriptableObjects for:

- Box types.
- Order templates.
- Upgrade definitions.
- Worker definitions.
- Economy values.
- Rewarded ad placements.

### Box Type Config

Fields:

- `id`
- `displayName`
- `prefab`
- `color`
- `baseValue`

### Order Template Config

Fields:

- `id`
- `requiredBoxes`
- `baseReward`
- `weight`
- `minProgressionLevel`

### Upgrade Config

Fields:

- `id`
- `displayName`
- `maxLevel`
- `baseCost`
- `costCurve`
- `effectType`
- `effectValuesByLevel`

### Worker Config

Fields:

- `id`
- `displayName`
- `prefab`
- `speed`
- `carryCapacity`
- `taskType`

## 6. Save Data Schema

Initial save data:

```json
{
  "version": 1,
  "cash": 0,
  "tutorialCompleted": false,
  "upgradeLevels": {
    "carry_capacity": 0,
    "movement_speed": 0,
    "order_value": 0,
    "packing_speed": 0,
    "shelf_capacity": 0,
    "hire_worker": 0
  },
  "workersUnlocked": [
  ],
  "warehouseProgressionLevel": 0
}
```

## 7. Scene Requirements

### MVP Scene

Scene name: `Warehouse_MVP`

Required objects:

- Player spawn point.
- Loading dock.
- Packing station.
- Delivery zone.
- Upgrade station.
- Shelf/storage area.
- Worker spawn point.
- Camera rig.
- UI canvas.
- Event system.
- Game manager.

## 8. UI Architecture

MVP UI components:

- Cash display.
- Active order display.
- Carry capacity display.
- Upgrade panel.
- Tutorial arrow/instruction text.
- Fake rewarded ad prompt.
- Basic settings/debug menu.

## 9. Performance Requirements

- Target frame rate: 60 FPS on recent iPhones.
- MVP object count should remain low.
- Box pooling should be used if many boxes are spawned.
- Analytics and save operations must not block gameplay.

## 10. Testing Requirements

### Manual Test Coverage

- New game tutorial flow.
- Box pickup and drop.
- Order completion.
- Upgrade purchase.
- Worker hiring.
- Save/load.
- Fake rewarded ad reward.

### Automated Test Candidates

- Upgrade cost calculation.
- Currency add/spend validation.
- Order requirement matching.
- Save data serialization.
- Data config validation.

## 11. Acceptance Criteria

- Gameplay systems are separated into independent components.
- Economy values can be tuned without code changes.
- Save/load works across app restarts.
- Analytics calls are centralized.
- Fake ad functionality is isolated behind `AdManager`.
- No backend dependency exists in MVP.
