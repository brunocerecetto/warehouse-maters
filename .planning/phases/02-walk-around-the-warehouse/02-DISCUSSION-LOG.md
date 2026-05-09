# Phase 2: Walk Around the Warehouse - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-05-09
**Phase:** 2-walk-around-the-warehouse
**Areas discussed:** Joystick design, Movement controller, Camera follow, Speed config

---

## Joystick design

### Q1: Joystick placement style?

| Option | Description | Selected |
|--------|-------------|----------|
| Floating left-half | Touch anywhere on left half spawns joystick at touch point. Hybrid-casual standard. One-thumb friendly. | |
| Fixed bottom-left | Anchored at fixed safe-area position. Always visible. Predictable. | ✓ |
| Hybrid (snap-to-touch) | Visible at fixed home; jumps to finger on first touch, snaps back on release. | |

**User's choice:** Fixed bottom-left
**Notes:** Reinforces learnability for first-time player; pairs with always-visible decision below.

### Q2: Joystick visibility?

| Option | Description | Selected |
|--------|-------------|----------|
| Always visible | Discoverable for new player. Reinforces fixed-position learnability. | ✓ |
| Visible only on touch | Cleaner screen; risk of new player missing tap target. | |
| Fade in on first touch, persist after | Compromise. | |

**User's choice:** Always visible

### Q3: Joystick implementation source?

| Option | Description | Selected |
|--------|-------------|----------|
| Custom UGUI component | Hand-rolled MonoBehaviour. No third-party dependency. Matches CLAUDE.md no-unspecified-packages rule. | ✓ |
| Joystick Pack (Asset Store) | Polished feel, supports floating/fixed/dynamic. External dependency. | |
| On-Screen Stick from Input System | Built-in; ties into Input System actions. Limited customization. | |

**User's choice:** Custom UGUI component

### Q4: Joystick output to PlayerController?

| Option | Description | Selected |
|--------|-------------|----------|
| Direct property read | Controller reads `joystick.Direction` each Update. Simple, no event plumbing. | ✓ |
| C# event / Action callback | Joystick fires `OnDirectionChanged(Vector2)`. More decoupled. | |
| Input System binding (composite) | Custom Input Action wired through OnScreenStick-style integration. | |

**User's choice:** Direct property read

---

## Movement controller

### Q1: Player movement controller type?

| Option | Description | Selected |
|--------|-------------|----------|
| CharacterController.Move | Kinematic capsule, manual collision. Hybrid-casual standard for top-down. | ✓ |
| Rigidbody (kinematic) + manual collision | More control, more code. | |
| Rigidbody (dynamic) + AddForce | Real physics. Tunneling risk, slippery feel. | |
| NavMeshAgent for player + worker | Player on NavMesh too. Feels indirect for direct-input. | |

**User's choice:** CharacterController.Move
**Notes:** Worker AI in Phase 7 stays free to use NavMeshAgent independently.

### Q2: Camera-relative direction translation?

| Option | Description | Selected |
|--------|-------------|----------|
| Project camera forward to ground plane | `Vector3.ProjectOnPlane(cam.forward, Vector3.up)`. Works at any pitch. | ✓ |
| World-space mapping (joystick = XZ direct) | Simpler. Breaks if camera ever rotates. | |
| Camera transform forward (ignore pitch) | Pitched camera tilts motion vector — player drifts up/down. | |

**User's choice:** Project camera forward to ground plane

### Q3: Player rotation behavior?

| Option | Description | Selected |
|--------|-------------|----------|
| Face movement direction with smoothing | `Quaternion.Slerp` toward velocity. Configurable turn rate. | ✓ |
| Snap-rotate to direction | Instant facing. Twitchy on reversals. | |
| Keep fixed forward (camera-locked) | Player always faces camera up. Kills carry-stack visual variety. | |

**User's choice:** Face movement direction with smoothing

### Q4: Acceleration model?

| Option | Description | Selected |
|--------|-------------|----------|
| Instant (joystick = velocity directly) | Snappy, mobile-readable, hybrid-casual standard. | ✓ |
| Linear acceleration / damping | Smooth ramp-up/ramp-down. Adds tuning surface. | |
| Velocity smoothing via SmoothDamp | Buttery but can feel laggy on touch input. | |

**User's choice:** Instant

---

## Camera follow

### Q1: Camera follow behavior?

| Option | Description | Selected |
|--------|-------------|----------|
| Cinemachine VCam follow with damping | Phase 1 D-12 VCam gets `Follow = Player`. Smooth lag, readable motion. | ✓ |
| Rigid follow (zero damping) | Camera glued to player offset. Crisp but jittery on reversals. | |
| Fixed-no-follow (frame whole arena) | Camera never moves. Risk: small player on screen, hurts readability. | |

**User's choice:** Cinemachine VCam follow with damping

### Q2: Camera projection?

| Option | Description | Selected |
|--------|-------------|----------|
| Orthographic isometric | Top-down ~45° pitch + ortho. Hybrid-casual idle/tycoon norm. Stable across iPhone aspects. | ✓ |
| Perspective with narrow FOV | FOV ~30–40° to mimic ortho. Slight depth cue. | |
| Perspective with default FOV | FOV ~60°. Strong depth, breaks readability. | |

**User's choice:** Orthographic isometric

### Q3: Follow offset / framing?

| Option | Description | Selected |
|--------|-------------|----------|
| Player slightly below screen center | Composer Screen Position Y ≈ 0.55–0.60. Frees upper screen for HUD. | ✓ |
| Player exact center | Symmetrical framing. Less HUD headroom. | |
| Lookahead toward joystick direction | Camera leads movement direction. Risk of disorientation on small screen. | |

**User's choice:** Player slightly below screen center

### Q4: Camera bounds / soft limits?

| Option | Description | Selected |
|--------|-------------|----------|
| Cinemachine Confiner with bounds collider | Camera stops at warehouse edges. Standard CM pattern. | ✓ |
| No bounds | Camera follows freely. Risk of seeing void at edges. | |
| Defer to later phase | Skip in Phase 2; add when arena layout firms up. Risk: feels broken in playtest. | |

**User's choice:** Cinemachine Confiner with bounds collider
**Notes:** Captured as `CinemachineConfiner3D` since the warehouse is 3D (Confiner2D is for 2D collider bounds). Claude's discretion on the 3D vs 2D variant.

---

## Speed config

### Q1: Where does player movement speed live?

| Option | Description | Selected |
|--------|-------------|----------|
| PlayerConfig ScriptableObject | Data-driven. Phase 5 upgrade modifies runtime stat layered on top. Matches CLAUDE.md SO rule. | ✓ |
| Serialized field on PlayerController | Inspector-tunable per-instance. Phase 5 retrofits later. | |
| Code constant | Hardcoded. Violates CLAUDE.md no-hardcoded-balance rule. | |

**User's choice:** PlayerConfig ScriptableObject

### Q2: Runtime speed mutation surface (anticipating Phase 5 upgrade)?

| Option | Description | Selected |
|--------|-------------|----------|
| PlayerStats runtime instance | Runtime class initialized from PlayerConfig. Phase 5 upgrade calls into PlayerStats. SO stays immutable. | ✓ |
| Mutate the ScriptableObject directly | Phase 5 writes back to SO asset. SO mutations dirty asset between Play sessions. | |
| Decide in Phase 5 — only expose getter now | Defer the mutation surface. Phase 5 wires the indirection later. | |

**User's choice:** PlayerStats runtime instance

### Q3: PlayerConfig asset location and naming?

| Option | Description | Selected |
|--------|-------------|----------|
| `Assets/_Project/ScriptableObjects/Player/PlayerConfig.asset` | New `Player/` SO subfolder. Matches per-system convention. | ✓ |
| `Assets/_Project/ScriptableObjects/Economy/PlayerConfig.asset` | Co-locate with Economy SO. Awkward — player config isn't economy data. | |
| Inline on PlayerController prefab | Skip SO. Conflicts with SO-first decision. | |

**User's choice:** `Assets/_Project/ScriptableObjects/Player/PlayerConfig.asset`

### Q4: Initial PlayerConfig values?

| Option | Description | Selected |
|--------|-------------|----------|
| baseMoveSpeed=5 m/s, turnRate=720°/s | Hybrid-casual mobile defaults. Snappy. | ✓ |
| baseMoveSpeed=3.5 m/s, turnRate=540°/s | Calmer tycoon pace. Risk: feels sluggish. | |
| Leave 0 — force tuning in playtest | Empty SO, no defaults. Forces mandatory inspector edit. | |

**User's choice:** baseMoveSpeed=5 m/s, turnRate=720°/s
**Notes:** Placeholder — final values come from playtesting.

---

## Claude's Discretion

- D-09 (gravity model) — Claude chose minimal `Physics.gravity.y * dt` to keep the capsule grounded. No jump.
- D-13 (Confiner type) — Claude chose `CinemachineConfiner3D` since the warehouse is 3D; user picked "Cinemachine Confiner" generically.
- D-15 (placeholder speed values) — Claude defaults; playtest will retune.
- D-18 (invisible wall colliders for outer warehouse boundary) — Claude default to keep player on the floor plane.

## Deferred Ideas

- Editor keyboard fallback (WASD/arrow) for non-device testing — defer; add only if device-only testing slows iteration.
- NavMesh bake — defer to Phase 7 (worker AI).
- Joystick dead-zone tuning, knob travel cap, snap-back animation — implementation details for the planner; defaults captured in CONTEXT.md deferred section.
- Quality tier camera ortho size / pitch variants for non-iPhone aspects — single ortho size for MVP.
- Lookahead camera offset toward joystick direction — considered, deferred to playtest.
- Player visual prefab beyond placeholder primitive — outside MVP art scope.
