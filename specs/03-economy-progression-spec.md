# Warehouse Master — Economy and Progression Spec

## 1. Overview

The MVP economy must create frequent early wins. The player should complete orders, earn cash, buy upgrades, and hire the first worker within the first 5 minutes.

The economy should be simple, cash-only, and locally configured.

## 2. Currency

### MVP Currency

| Currency | Type | Purpose |
|---|---|---|
| Cash | Soft currency | Buy upgrades and unlock early progression |

### Deferred Currencies

| Currency | Phase | Purpose |
|---|---|---|
| Gems | Post-MVP | Premium purchases, boosts, skins |
| Event Tickets | Live ops | Temporary event rewards |

## 3. Early Progression Targets

| Time From Start | Expected Milestone |
|---:|---|
| 0:30 | First order completed |
| 1:30 | First upgrade purchased |
| 3:00 | Second upgrade purchased |
| 5:00 | First worker hired |
| 8:00 | New shelf or station improvement unlocked |
| 12:00 | Conveyor or automation preview unlocked |
| 15:00 | First special order introduced |

## 4. Order Rewards

### Initial Reward Values

| Order Complexity | Example | Base Reward |
|---|---|---:|
| Very Small | 1 box | 5 cash |
| Small | 2 boxes | 10 cash |
| Medium | 3 boxes | 20 cash |
| Large | 4-5 boxes | 35-50 cash |

### Reward Formula Recommendation

For MVP, use table-based rewards rather than a complex formula.

Future formula candidate:

```text
reward = baseBoxValue * totalBoxes * complexityMultiplier * orderValueUpgradeMultiplier
```

## 5. Upgrade Cost Curve

### MVP Upgrade Levels

Each upgrade should have 3 to 5 levels in the prototype.

### Example Cost Curve

| Upgrade Level | Cost Multiplier |
|---:|---:|
| 1 | 1.0x |
| 2 | 2.0x |
| 3 | 3.5x |
| 4 | 6.0x |
| 5 | 10.0x |

### Initial Upgrade Costs

| Upgrade | Level 1 Cost | Notes |
|---|---:|---|
| Carry Capacity | 25 | First recommended upgrade |
| Movement Speed | 35 | Should be felt immediately |
| Order Value | 50 | Improves reward pace |
| Packing Speed | 60 | Reduces waiting |
| Shelf Capacity | 75 | Supports automation |
| Hire Worker | 150 | First major milestone |

## 6. Upgrade Effects

### Carry Capacity

| Level | Capacity |
|---:|---:|
| 0 | 3 boxes |
| 1 | 5 boxes |
| 2 | 8 boxes |
| 3 | 12 boxes |
| 4 | 16 boxes |
| 5 | 20 boxes |

### Movement Speed

| Level | Speed Multiplier |
|---:|---:|
| 0 | 1.00x |
| 1 | 1.10x |
| 2 | 1.22x |
| 3 | 1.35x |
| 4 | 1.50x |
| 5 | 1.65x |

### Order Value

| Level | Reward Multiplier |
|---:|---:|
| 0 | 1.00x |
| 1 | 1.15x |
| 2 | 1.35x |
| 3 | 1.60x |
| 4 | 1.90x |
| 5 | 2.25x |

### Packing Speed

| Level | Packing Duration Multiplier |
|---:|---:|
| 0 | 1.00x |
| 1 | 0.85x |
| 2 | 0.70x |
| 3 | 0.58x |
| 4 | 0.48x |
| 5 | 0.40x |

## 7. Worker Economy

### First Worker

- Unlock cost: 150 cash.
- Role: transport boxes from loading dock to shelves.
- Initial carry capacity: 2 boxes.
- Initial movement speed: 80% of player base speed.

### Worker Upgrade Candidates

Deferred until post-MVP:

- Worker speed.
- Worker carry capacity.
- Worker count.
- Specialized worker roles.
- Worker managers.

## 8. Progression Gates

The MVP should avoid hard gates. The player should always have something useful to do.

Recommended soft gates:

- Higher order rewards require completing a number of basic orders.
- Hire Worker requires buying at least one prior upgrade.
- New shelf capacity becomes useful once worker is hired.

## 9. Offline Earnings

Offline earnings are deferred from the prototype but should be considered for soft launch.

Future rules:

- Earnings accumulate while the player is away.
- Earnings are capped by time, such as 2 hours initially.
- Player can watch a rewarded ad to double offline earnings.

## 10. Economy Tuning Principles

- The first session should feel generous.
- The player should not grind before understanding upgrades.
- Cash rewards should create frequent purchase opportunities.
- Hire Worker should feel like the first major achievement.
- Rewarded ads should accelerate progress, not repair a broken economy.

## 11. Acceptance Criteria

- A new player can buy the first upgrade within 2 minutes.
- A new player can hire the first worker within 5 minutes.
- The player never waits more than 30 seconds without a meaningful action during the first 5 minutes.
- Upgrade effects are visible or immediately felt.
- Economy values are configurable without code changes.
