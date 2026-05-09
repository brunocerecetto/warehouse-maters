# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project
Warehouse Master is an iOS-first Unity prototype: a hybrid-casual idle/tycoon warehouse game where the player receives boxes, prepares orders, delivers packages, earns cash, buys upgrades, hires workers, and automates the warehouse.

## Repository State
Pre-Unity-init. Repo currently contains only this file — no `Assets/`, no `ProjectSettings/`, no `/specs` files yet, no commits.
First task is likely either: (a) bootstrap the Unity project (iOS target, URP recommended) or (b) create the `/specs` skeleton listed under "Source of Truth". Until the Unity project is bootstrapped, build/test commands below do not apply.

## Build & Test
Fill in once Unity project is bootstrapped. Reference invocations (replace `<UNITY>` with the Unity Editor binary matching `ProjectSettings/ProjectVersion.txt`):
- Open in Editor: launch via Unity Hub against the project root.
- CLI iOS build: `<UNITY> -batchmode -quit -projectPath . -buildTarget iOS -executeMethod <BuildScript.BuildIOS> -logFile -`
- Edit Mode tests (all): `<UNITY> -batchmode -runTests -testPlatform EditMode -projectPath . -logFile -`
- Play Mode tests (all): `<UNITY> -batchmode -runTests -testPlatform PlayMode -projectPath . -logFile -`
- Single test: append `-testFilter <Namespace.Class.Method>` to the test command.
Once a real `BuildScript` and assembly definitions exist, replace placeholders with concrete names.

## Development Method
Use spec-driven development.

Before implementing any feature:
1. Read the relevant files in `/specs`.
2. Identify scope and acceptance criteria.
3. Propose a short implementation plan.
4. Implement only the requested scope.
5. Validate against the spec.
6. Update specs only when explicitly requested or when a correction is required.

Do not invent gameplay rules, currencies, monetization, backend features, or systems not described in the specs or current task.

## Source of Truth
The `/specs` directory is the source of truth for product, gameplay, economy, monetization, analytics, architecture, backlog, and roadmap.

Expected files:
- `/specs/README.md`
- `/specs/01-product-requirements-spec.md`
- `/specs/02-gameplay-systems-spec.md`
- `/specs/03-economy-progression-spec.md`
- `/specs/04-monetization-spec.md`
- `/specs/05-analytics-metrics-spec.md`
- `/specs/06-technical-architecture-spec.md`
- `/specs/07-mvp-backlog-acceptance-criteria.md`
- `/specs/08-roadmap-future-phases-spec.md`

Conflict priority:
1. Current user request
2. MVP backlog and acceptance criteria
3. Gameplay systems spec
4. Technical architecture spec
5. Economy and progression spec
6. Monetization spec
7. Analytics spec
8. Roadmap spec

## Language
All documentation, code comments, task notes, and implementation notes must be in English.

## Tech Stack
- Engine: Unity
- Language: C#
- Target: iOS first
- Backend: none for MVP
- Persistence: local save data
- Analytics: abstraction/stubs first
- Ads/IAP: simulated interfaces first

## MVP Scope
Allowed MVP systems:
- Player movement
- Virtual joystick input
- Automatic pickup/drop
- Carry capacity
- Three box types
- Basic order queue
- Packing table
- Delivery zone
- Cash rewards
- Upgrade station
- Core upgrades
- One basic worker type
- Local save/load
- Short tutorial
- Basic analytics events or stubs
- TestFlight-ready build preparation

Do not add unless explicitly requested:
- Multiplayer
- Backend
- Cloud save
- Leaderboards
- Battle pass
- Complex live ops
- Multiple warehouses
- Real-money IAP
- Production ads
- Complex physics
- Procedural maps
- Social login

## Architecture Guidelines
Prefer small, explicit systems over large manager classes.

Separate these responsibilities:
- Player control
- Carrying/inventory
- Box spawning
- Order management
- Packing stations
- Delivery zones
- Currency
- Upgrades
- Worker AI
- Save/load
- Analytics
- Ads
- IAP

Use ScriptableObjects for tunable game data:
- Box types
- Order definitions
- Upgrade definitions
- Worker definitions
- Economy values
- Reward values

Do not hardcode balance values in gameplay logic unless the task is explicitly throwaway prototype work.

## Implementation Style
Prefer:
- Explicit names
- Small MonoBehaviours
- Plain C# services where suitable
- Interfaces for external integrations
- Serialized fields for tunable references
- Clear event names
- Minimal dependencies

Avoid:
- Over-engineering
- Global mutable state without reason
- Hidden side effects
- Large inheritance chains
- Unspecified third-party packages
- Premature optimization
- Feature creep

## External Integrations
Wrap external systems behind interfaces before real SDK integration.

Use abstractions such as:
- `IAnalyticsService`
- `IAdService`
- `IIapService`
- `ISaveService`

For MVP, prefer mock/simulated implementations unless real SDK setup is explicitly requested.

## Analytics Rules
Route analytics through one abstraction. Do not scatter SDK calls across gameplay code.

Important MVP events:
- `tutorial_started`
- `tutorial_completed`
- `first_order_completed`
- `order_completed`
- `upgrade_purchased`
- `worker_hired`
- `area_unlocked`
- `ad_offer_shown`
- `rewarded_ad_clicked`
- `rewarded_ad_completed`

## Monetization Rules
For MVP, monetization is simulated. Rewarded ads may use mock buttons or fake completions. Do not implement real ads, IAP, mediation, SKAdNetwork, ATT prompts, or App Store products unless explicitly requested. Do not block core progression behind ads or purchases.

## Gameplay Priorities
The game must be understandable quickly. Prioritize a short first-session loop, fast first order, fast first upgrade, clear feedback, satisfying pickup/drop, visible warehouse growth, low-friction onboarding, and early first worker.

## Task Workflow
For each task:
1. Restate the target feature in one sentence.
2. List specs consulted.
3. List assumptions only if needed.
4. Implement the smallest complete version.
5. Provide validation steps.
6. Mention spec mismatches or open questions.

Do not ask for clarification when a reasonable MVP assumption can be made from the specs.

## Testing and Validation
When possible, include Unity Editor validation steps: scene setup, inspector references, expected player behavior, expected UI feedback, and expected save/analytics behavior. Use automated tests when logic is isolated from Unity scene state.

## File Discipline
Keep files organized by system. Do not create unrelated folders, rename architecture, move specs, or duplicate specs unless requested.

## Completion Criteria
A task is complete when requested scope is implemented, relevant specs and acceptance criteria are followed, validation steps are provided, and no unrelated features were added.

<!-- GSD:project-start source:PROJECT.md -->
## Project

**Warehouse Master**

Warehouse Master is a free-to-play iOS hybrid-casual idle/tycoon prototype built in Unity. The player operates a small warehouse — receives boxes, prepares orders, delivers packages, earns cash, buys upgrades, and hires automated workers. The MVP validates whether the core loop is engaging for 5–10 minutes before any soft launch effort.

**Core Value:** The 5-minute warehouse loop must feel satisfying: pick up boxes, complete an order, earn cash, buy an upgrade, watch the warehouse get faster.

### Constraints

- **Engine**: Unity (URP recommended) — Required by spec; broadest mobile monetization ecosystem.
- **Language**: C# — Unity standard.
- **Target platform**: iOS first, iPhone, portrait orientation — Spec recommendation locked.
- **Backend**: None for MVP — Local save only; backend deferred to soft launch.
- **Persistence**: Local save data — Validates loop without infrastructure cost.
- **Analytics**: Wrapper-first, mock implementation in MVP — Avoid SDK lock-in before validating fun.
- **Ads / IAP**: Simulated interfaces only in MVP (`IAdService`, `IIapService`) — No real SDKs until soft launch.
- **Architecture style**: Small MonoBehaviours + plain C# services + ScriptableObjects for data — No god-managers, no hardcoded balance.
- **Build target**: TestFlight-ready iOS build — Internal validation, not App Store release.
- **Timeline reference**: Master plan suggests 4-week internal prototype — Used as sizing signal, not contractual.
- **Language**: All code, comments, docs in English (per `CLAUDE.md`) — Master plan in Spanish is reference only.
<!-- GSD:project-end -->

<!-- GSD:stack-start source:STACK.md -->
## Technology Stack

Technology stack not yet documented. Will populate after codebase mapping or first phase.
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

Architecture not yet mapped. Follow existing patterns found in the codebase.
<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->
## Project Skills

No project skills found. Add skills to any of: `.claude/skills/`, `.agents/skills/`, `.cursor/skills/`, `.github/skills/`, or `.codex/skills/` with a `SKILL.md` index file.
<!-- GSD:skills-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd-debug` for investigation and bug fixing
- `/gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->

<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
