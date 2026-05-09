# Phase 3: Pick Up & Carry Boxes - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-05-09
**Phase:** 3-pick-up-carry-boxes
**Areas discussed:** Pickup detection mechanism, Box prefab + ScriptableObject shape, Carry system (capacity + visible stack), Loading dock spawner + capacity

---

## Pickup detection mechanism

### Q1: Pickup detection mechanism?

| Option | Description | Selected |
|--------|-------------|----------|
| Trigger zone on dock; player enters → dock pushes boxes | Logic stays on dock, works identically for Phase 7 worker. | ✓ |
| Trigger zone on player; player detects boxes | Couples player to box detection. | |
| Distance polling on dock | No physics triggers; more overhead. | |

**User's choice:** Trigger zone on dock; dock pushes to ICarrier

### Q2: Pickup tempo?

| Option | Description | Selected |
|--------|-------------|----------|
| Drip-feed at configurable interval | Tactile rhythm, hybrid-casual feel. | ✓ |
| Fill to capacity instantly on enter | Snappy but undersells the pickup feel. | |
| Per-physics-tick (one box / FixedUpdate) | Too fast — pickup feels invisible. | |

**User's choice:** Drip-feed at configurable interval

### Q3: Box selection order when multiple types on dock?

| Option | Description | Selected |
|--------|-------------|----------|
| FIFO queue (oldest spawn first) | Predictable, easy to reason about. | ✓ |
| Type-balanced round-robin | Adds policy that may fight order-driven flow. | |
| Closest-to-player | Adds spatial fairness; per-frame distance sort. | |

**User's choice:** FIFO

### Q4: ICarrier abstraction shape?

| Option | Description | Selected |
|--------|-------------|----------|
| ICarrier interface in WM.Boxes | Player + Phase 7 worker implement same interface. Zero coupling. | ✓ |
| Direct PlayerController dependency | Phase 7 needs refactor. | |
| Static CarryRegistry lookup | Adds global state for single-player MVP. | |

**User's choice:** ICarrier interface

---

## Box prefab + ScriptableObject shape

### Q1: Box prefab strategy?

| Option | Description | Selected |
|--------|-------------|----------|
| One shared prefab; BoxType SO drives material/color | Adding 4th type = new SO, no new prefab. Matches BOX-01 data-driven. | ✓ |
| Three distinct prefabs | Simpler authoring; adds N prefabs per future type. | |
| Hybrid: shared prefab + visual variant prefabs | More indirection. | |

**User's choice:** One shared prefab + BoxType SO

### Q2: BoxType ScriptableObject fields?

| Option | Description | Selected |
|--------|-------------|----------|
| id, displayName, color, material, baseValue | Covers BOX-01 + ORD-01 needs. | ✓ |
| id, color, baseValue (minimal) | Color drives runtime material via MPB. | |
| Defer schema | Risk of Phase 4 retrofit. | |

**User's choice:** id, displayName, color, material, baseValue

### Q3: Box runtime identity after instantiation?

| Option | Description | Selected |
|--------|-------------|----------|
| Box MonoBehaviour with `BoxType Type { get; }` | Direct SO reference, no string lookups. | ✓ |
| String id only on Box | Loses type safety. | |
| Enum BoxColor on Box | Conflicts with future-extensibility. | |

**User's choice:** Box MonoBehaviour with BoxType reference

### Q4: Box asset locations?

| Option | Description | Selected |
|--------|-------------|----------|
| SO under ScriptableObjects/Boxes/, prefab under Prefabs/Boxes/, materials under Materials/Boxes/ | Matches Phase 1 D-05 layout. | ✓ |
| All co-located | Convenient; breaks per-system folder convention. | |
| Defer materials — MaterialPropertyBlock from SO color | Smaller asset count; less Inspector-friendly. | |

**User's choice:** Per-folder split

---

## Carry system (capacity + visible stack)

### Q1: Where does carry state live?

| Option | Description | Selected |
|--------|-------------|----------|
| Dedicated CarrySystem MonoBehaviour on Player | Worker can attach the same component in Phase 7. | ✓ |
| Inside PlayerController | Couples movement and carry. | |
| Inside PlayerStats | Mixes runtime stats with item state. Wrong layer. | |

**User's choice:** Dedicated CarrySystem MonoBehaviour

### Q2: Carry capacity source?

| Option | Description | Selected |
|--------|-------------|----------|
| PlayerStats.CarryCapacity, mirroring Phase 2 pattern | Same shape as movement speed. Phase 5 ECON-03 writes to PlayerStats. | ✓ |
| CarryConfig SO + CarryStats class | Cleaner separation; more files. | |
| Serialized field on CarrySystem | Conflicts with Phase 2 SO-first decision. | |

**User's choice:** PlayerStats.CarryCapacity

### Q3: Visible stack rendering approach?

| Option | Description | Selected |
|--------|-------------|----------|
| Reparent the actual Box GameObject to a CarryAnchor | Single Box instance flows dock → player → station. | ✓ |
| Pre-instantiated N visual proxy slots; toggle visibility | Decouples visuals from logical count. | |
| Instantiate visual per pickup; destroy on drop | Allocation churn. | |

**User's choice:** Reparent actual Box GameObject

### Q4: Stack layout?

| Option | Description | Selected |
|--------|-------------|----------|
| Vertical stack on player back, fixed offset per box | Iconic to genre per PROJECT.md. | ✓ |
| Pyramid / brick-stack arrangement | Adds layout complexity. | |
| Single visible box + counter UI badge | Sacrifices warehouse fantasy visual signature. | |

**User's choice:** Vertical stack with fixed offset

---

## Loading dock spawner + capacity

### Q1: Spawn cadence model?

| Option | Description | Selected |
|--------|-------------|----------|
| Fixed-interval timer with config seconds | Predictable, designer-friendly. | ✓ |
| Probabilistic per-tick | Variability; less predictable for tuning. | |
| Adaptive: spawn faster when dock empties | Adds policy that fights playtest tuning clarity. | |

**User's choice:** Fixed-interval timer

### Q2: Type distribution model?

| Option | Description | Selected |
|--------|-------------|----------|
| Weighted entries: List<TypeWeight> | Designer-tunable; scales to weighted future types. | ✓ |
| Fixed equal split (round-robin) | Doesn't scale to weighted types. | |
| Pure uniform random | Simple; no weights. | |

**User's choice:** Weighted entries

### Q3: Dock capacity ceiling — visual representation?

| Option | Description | Selected |
|--------|-------------|----------|
| Pre-defined slot Transforms; spawn into next free slot | Clear visual full/not-full state. Matches FIFO. | ✓ |
| Stack physically on top of each other | Physics churn. | |
| Logical capacity, scattered random positions | Looks messy. | |

**User's choice:** Pre-defined slot Transforms

### Q4: Initial LoadingDockConfig values?

| Option | Description | Selected |
|--------|-------------|----------|
| spawnInterval=1.0s, dockCapacity=8 slots, equal 1/1/1 weights | Reads briskly with carry capacity 3. | ✓ |
| spawnInterval=0.5s, dockCapacity=12 slots | Risk: dock always full. | |
| Defer initial values | Forces tuning before first run. | |

**User's choice:** spawnInterval=1.0s, dockCapacity=8, equal weights

---

## Claude's Discretion

- D-02 default `pickupIntervalSeconds = 0.15` — Claude default; playtest will retune.
- D-12 CarryAnchor coordinates (~y=1.0, z=−0.3) and `carryStackSpacing = 0.05` — Claude default for placeholder capsule; planner can adjust when player visual lands.
- D-16 initial `LoadingDockConfig` values — Claude default placeholders.
- D-11 box collider disabled while carried — Claude default to avoid CharacterController shoving.

## Deferred Ideas

- Box pickup feedback (audio, particle, scale-pop) — Phase 8 / TUT-02.
- Dock-full visual feedback — Phase 8.
- Multiple loading docks / second dock unlock — out of MVP scope.
- Special box types (fragile, frozen, heavy, gold) — REQUIREMENTS.md v2.
- Box pooling — defer until profiling shows GC pressure.
- Stack jiggle / IK during walk — Phase 8.
- Re-enable box collider while carried — defer until a feature needs it.
- Drop-while-walking — not in MVP.
- Closest / type-balanced pickup ordering — FIFO chosen; revisit only on playtest signal.
