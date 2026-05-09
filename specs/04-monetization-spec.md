# Warehouse Master — Monetization Spec

## 1. Overview

Warehouse Master should use hybrid monetization:

- Rewarded ads as the primary ad format.
- Interstitial ads only at natural breaks.
- In-app purchases for removing ads and accelerating progress.
- Premium currency and advanced offers deferred until after MVP validation.

The MVP should include simulated monetization hooks but should not require production ads or live IAP.

## 2. Monetization Goals

- Preserve early retention.
- Use rewarded ads as optional accelerators.
- Avoid forced monetization during the tutorial.
- Create natural moments where players want extra cash, speed, or convenience.
- Prepare the game for soft launch monetization without blocking prototype development.

## 3. MVP Monetization Scope

### Included in MVP

- Fake rewarded ad buttons.
- Rewarded ad placement logic.
- Analytics events for ad offer shown/clicked/completed.
- Remove Ads product placeholder.
- Starter Pack placeholder.

### Not Included in MVP

- Production ad network integration.
- Real IAP transactions.
- Premium currency store.
- Subscription.
- Battle pass.
- Piggy bank.
- Live offers.

## 4. Rewarded Ads

### Rewarded Ad Principles

Rewarded ads must be optional and clearly valuable.

They should never be required to complete the tutorial or make basic progress.

### MVP Rewarded Placements

| Placement ID | Trigger | Reward | Priority |
|---|---|---|---|
| `double_order_reward` | After delivering a high-value order | 2x cash for that order | High |
| `missing_cash_boost` | Player is near upgrade cost | Grant missing cash up to capped amount | High |
| `speed_boost` | Player opens boost prompt | 2x player speed for 3 minutes | Medium |
| `worker_boost` | Worker is unlocked | 2x worker speed for 3 minutes | Medium |
| `instant_pack` | Packing timer is active | Complete current packing instantly | Low |

### Acceptance Criteria

- Rewarded ad offers are not shown during the tutorial.
- Rewarded ad offers can be completed in fake mode during MVP.
- Completing a fake rewarded ad grants the configured reward.
- Rewarded ad events are emitted for shown, clicked, completed, and skipped states.

## 5. Interstitial Ads

### MVP Status

Interstitials are not active in MVP. They are defined for soft launch.

### Soft Launch Rules

- No interstitials in the first 2 to 3 minutes.
- No interstitials during the tutorial.
- No interstitials during active box movement.
- No interstitial immediately after a rewarded ad.
- Minimum cooldown: 90 to 120 seconds.
- Prefer natural breakpoints.

### Candidate Breakpoints

| Breakpoint | Priority |
|---|---|
| After area unlock | High |
| After completing a batch of orders | Medium |
| After closing upgrade menu | Medium |
| After returning from idle/offline summary | Low |

## 6. In-App Purchases

### Product 1 — Remove Ads

| Field | Value |
|---|---|
| Product ID | `remove_ads` |
| Type | Non-consumable |
| Suggested Price | USD 2.99 to 4.99 |
| Removes | Interstitial ads and banners if added |
| Does Not Remove | Optional rewarded ads |

### Product 2 — Starter Pack

| Field | Value |
|---|---|
| Product ID | `starter_pack_01` |
| Type | Consumable or non-consumable depending on implementation |
| Suggested Price | USD 1.99 to 2.99 |
| Contents | Cash, temporary boost, cosmetic item, optional permanent small bonus |
| Display Timing | After player buys at least one upgrade |

### Product 3 — Gem Packs

Deferred until premium currency exists.

### Product 4 — Piggy Bank

Deferred until phase 3.

## 7. Store Design Principles

- The store should not appear before the player understands the core loop.
- First purchase prompts should be contextual, not aggressive.
- Remove Ads should be easy to find.
- Rewarded ad options should remain available to non-paying players.
- Purchases should feel like acceleration, not mandatory progression.

## 8. Monetization Events

| Event | Trigger |
|---|---|
| `ad_offer_shown` | Rewarded ad prompt appears |
| `ad_offer_clicked` | Player taps ad button |
| `rewarded_ad_completed` | Player completes rewarded ad |
| `rewarded_ad_skipped` | Player closes or declines |
| `interstitial_shown` | Interstitial appears |
| `iap_offer_shown` | IAP prompt appears |
| `iap_purchase_started` | Player begins purchase flow |
| `iap_purchase_completed` | Purchase succeeds |
| `iap_purchase_failed` | Purchase fails |

## 9. Acceptance Criteria

- Fake rewarded ads can be used in the MVP without any ad SDK.
- Reward configuration is data-driven.
- Ad and IAP placeholders do not block normal progression.
- A player can complete the first 5 minutes without seeing forced monetization.
- The codebase has clear `AdManager` and `IAPManager` boundaries for later SDK integration.
