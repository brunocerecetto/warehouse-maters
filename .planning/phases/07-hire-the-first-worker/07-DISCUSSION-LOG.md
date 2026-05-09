# Phase 7: Hire the First Worker - Discussion Log

> **Audit trail only.** Generated in --auto mode.

**Date:** 2026-05-09
**Phase:** 7-hire-the-first-worker
**Mode:** --auto
**Areas auto-resolved:** Pathing tech, Worker AI architecture, Worker carry semantics, Player-collision avoidance, Worker spawn/lifecycle

---

## Pathing tech

| Option | Description | Selected |
|---|---|---|
| NavMeshSurface (AI Navigation pkg), runtime-baked | Idiomatic Unity; supports moving obstacles. | ✓ |
| Custom waypoint graph | Manual maintenance; brittle to scene edits. | |
| A* Pathfinding Project (Asset Store) | Adds 3rd-party dep — `CLAUDE.md` discourages. | |

**[auto] Selected:** NavMeshSurface.

## Worker AI architecture

| Option | Description | Selected |
|---|---|---|
| Plain C# state machine in WorkerAI MonoBehaviour | Right size for one worker type with five states. | ✓ |
| Behavior tree (e.g., NodeCanvas) | Over-engineered; adds dependency. | |
| Coroutine-driven sequence | Hard to interrupt; awkward for stuck-recovery. | |

**[auto] Selected:** Plain C# state machine.

## Worker carry semantics

| Option | Description | Selected |
|---|---|---|
| Reuse CarrySystem with WorkerStats | Single component for both actors; ICarrier already abstracts. | ✓ |
| Distinct WorkerCarry component | Duplicates logic; two carry codepaths. | |
| Worker carries via NavMeshAgent payload metadata | Hacky; doesn't render visually. | |

**[auto] Selected:** Reuse CarrySystem.

## Player-collision avoidance

| Option | Description | Selected |
|---|---|---|
| NavMeshObstacle (carve=false) on player + agent avoidance on worker | Cheap; spec says "not block the player excessively". | ✓ |
| NavMeshObstacle carve=true on player | Expensive; re-bakes navmesh constantly. | |
| Reciprocal NavMeshAgent on player | Conflicts with CharacterController motion (Phase 2 D-05). | |

**[auto] Selected:** NavMeshObstacle (carve=false) + worker yields.

## Worker spawn / lifecycle

| Option | Description | Selected |
|---|---|---|
| WorkerSpawner reads WorkerUnlocked at boot + subscribes OnLevelChanged | Handles both fresh-purchase and load-from-save. | ✓ |
| Spawn unconditionally; toggle visibility | Wastes runtime on locked playthroughs. | |
| Manual scene reference | Doesn't survive save/load asymmetry. | |

**[auto] Selected:** Spawner reads + subscribes.

---

## Claude's Discretion

- Worker speed `3.0` and capacity `2` placeholders.
- Stuck-recovery threshold `2s` default.
- Unfiltered `TryRemoveAny` chosen over typed shelf removal (shelf in MVP doesn't care about types).

## Deferred Ideas

- Multiple worker types, worker upgrades, worker_boost ad, hire animation, pathing visualization, smarter stuck-recovery, `first_worker_hired` analytics — out of MVP or other phases.
