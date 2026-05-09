# Warehouse Master — Analytics and Metrics Spec

## 1. Overview

Analytics must validate whether the MVP loop is clear, engaging, and monetization-ready.

The MVP should track onboarding, order completion, upgrade behavior, worker unlock, session length, and fake monetization interactions.

## 2. Analytics Provider

Recommended provider for MVP:

- Firebase Analytics for Unity

The analytics implementation must be wrapped in an internal `AnalyticsManager` so providers can be changed or supplemented later.

## 3. Core Questions

Analytics should answer:

1. Do players understand what to do?
2. How quickly do players complete the first order?
3. How quickly do players buy the first upgrade?
4. How quickly do players hire the first worker?
5. How long is the first session?
6. Which upgrades are purchased first?
7. Do players show interest in rewarded ad offers?
8. Where do players stop progressing?

## 4. Funnel Events

### Onboarding Funnel

| Event | Required Parameters |
|---|---|
| `tutorial_started` | `session_id`, `build_version` |
| `tutorial_step_completed` | `step_id`, `time_since_start` |
| `tutorial_completed` | `duration_seconds` |

### First Session Funnel

| Event | Required Parameters |
|---|---|
| `first_box_picked_up` | `time_since_start`, `box_type` |
| `first_order_completed` | `time_since_start`, `order_id` |
| `first_upgrade_purchased` | `time_since_start`, `upgrade_id`, `upgrade_level` |
| `first_worker_hired` | `time_since_start`, `cash_spent` |

## 5. Gameplay Events

| Event | Description | Parameters |
|---|---|---|
| `box_picked_up` | Player picks up box | `box_type`, `current_stack_count` |
| `box_dropped` | Player drops box at station | `box_type`, `station_id` |
| `order_started` | New order becomes active | `order_id`, `required_box_count` |
| `order_packed` | Packing completes | `order_id`, `packing_duration` |
| `order_completed` | Package is delivered | `order_id`, `reward_cash`, `time_to_complete` |
| `package_delivered` | Package enters delivery zone | `package_id`, `reward_cash` |

## 6. Economy Events

| Event | Description | Parameters |
|---|---|---|
| `cash_earned` | Cash is granted | `amount`, `source` |
| `cash_spent` | Cash is spent | `amount`, `sink` |
| `upgrade_purchased` | Upgrade is bought | `upgrade_id`, `level`, `cost` |
| `upgrade_blocked_insufficient_cash` | Player tries to buy without enough cash | `upgrade_id`, `cash_owned`, `cost` |
| `worker_hired` | Worker is unlocked | `worker_type`, `cost` |

## 7. Monetization Events

Even if ads are fake in MVP, monetization intent should be measured.

| Event | Description | Parameters |
|---|---|---|
| `ad_offer_shown` | Fake rewarded prompt appears | `placement_id`, `reward_type`, `reward_amount` |
| `ad_offer_clicked` | Player accepts fake ad | `placement_id` |
| `rewarded_ad_completed` | Fake ad completes | `placement_id`, `reward_type`, `reward_amount` |
| `rewarded_ad_skipped` | Player declines or exits | `placement_id` |
| `iap_offer_shown` | IAP placeholder shown | `offer_id` |

## 8. Session Events

| Event | Description | Parameters |
|---|---|---|
| `session_started` | Game session begins | `session_id`, `build_version` |
| `session_ended` | Game session ends | `session_duration`, `orders_completed`, `cash_earned` |
| `app_backgrounded` | App goes to background | `session_duration` |
| `app_resumed` | App resumes | `time_away_seconds` |

## 9. Key MVP Metrics

### Validation Metrics

| Metric | Target Direction |
|---|---|
| Time to first box pickup | Lower is better |
| Time to first order completion | Under 60 seconds |
| Time to first upgrade | Under 2 minutes |
| Time to first worker | Under 5 minutes |
| First session length | 8 to 10 minutes or more |
| Tutorial completion rate | High |
| Upgrade purchase rate | High |

### Monetization Intent Metrics

| Metric | Target Direction |
|---|---|
| Rewarded ad offer click rate | Higher is better |
| Rewarded ad completion rate | Higher is better |
| Rewarded ad placement engagement | Identify best placement |

## 10. Event Naming Rules

- Use lowercase snake_case.
- Use stable event names.
- Avoid embedding variable data in event names.
- Put variable data in parameters.
- Do not send personally identifiable information.

## 11. Implementation Requirements

- All analytics calls must go through `AnalyticsManager`.
- Analytics must support disabled mode for local development.
- Analytics failures must not crash the game.
- Event parameters must be validated before sending.
- Build version must be included in session-level events.

## 12. Acceptance Criteria

- The MVP emits tutorial funnel events.
- The MVP emits first order, first upgrade, and first worker events.
- The MVP emits order completion events with duration and reward.
- The MVP emits fake rewarded ad events.
- Analytics can be disabled through configuration.
