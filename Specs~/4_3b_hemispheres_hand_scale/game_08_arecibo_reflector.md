# Arecibo Reflector

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Arecibo Reflector |
| Slug | `arecibo_reflector` |
| Physical prop | Upright cereal bowl; `R = 75.0 mm`, depth `55 mm`, wall `t = 4.0 mm`, mass `380 g` |
| Family / sub-shape | Hemisphere, bowl-up, hand scale |
| Collider family | Concave inner spherical cap + rim torus + foot ring |
| Scale class | Prop 1:1 (150 mm dish); virtual dish 900 mm, receiver 450 mm above |
| Source concept quote | "Rest the bowl on your lap and slide your thumb and index finger to specific coordinates across the concave inner dish surface... Incoming alien laser beams strike the dish from above; position your fingers on the curved inner wall to place mobile mirror rovers that bounce the beam directly into the suspended central receiver above the bowl." |
| Physical input | Thumb + index place and slide mirror rovers on the inner dish |
| Session length | 7 min (5 contacts × 70 s + onboarding) |
| Lose condition | Three beams lost to the jungle floor |

## 1. Fantasy & Core Loop

The bowl interior becomes a vast sunken spherical radio dish embedded in a jungle sinkhole, vines crawling over its rim and mist pooling at the bottom. Three mirror rovers — small six-wheeled drones with polished faces — sit on the dish surface. Alien beams stab down from orbit; each beam is a bright line that reflects off a rover's mirror according to the local surface normal. The player places the thumb and index fingers at two points on the inner dish to position two rovers; the third rover is autonomous and follows a patrol. The reflected beam must reach the suspended receiver hanging at the dish's focus. Rovers can only be placed at integer grid coordinates, so the player must solve a geometry puzzle in real time.

1. **Detect (1.0 s).** A beam telegraphs its incoming direction with a shimmering line.
2. **Place (0.5 s).** The thumb and index set rover A and rover B positions on a 7 × 7 grid on the inner dish.
3. **Reflect (instant).** The beam reflects from rover A's mirror; if that ray misses the receiver, it can bounce again off rover B.
4. **Align (1.5 s).** The player slides a rover to steer the second bounce; a lock ring shows proximity.
5. **Collect (0.6 s).** The receiver absorbs the beam and scores.
6. **Contact resolve (2.0 s).** Rovers recharge and the next beam arrives.

A beam cycle is 6–9 s; five contacts per session.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Hemisphere sub-shape**: the prop is a hemispherical shell of radius `R` and wall thickness `t`; model in P with the rim circle in the `x̂_P`–`ŷ_P` plane at `z = 0` and the apex at `z = ±R`. For **bowl-up** props the cavity faces `+ẑ_P` (inner surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`); for **dome-up** props the convex dome faces `+ẑ_P` (outer surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`, cavity facing `−ẑ_P`). Rim torus: major radius `R`, minor radius `t/2`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **Radius and wall**: `R = 75.0 mm`, depth `55 mm`, wall `t = 4.0 mm`, mass `m = 380 g`.
- **Orientation**: bowl-up. Cavity faces `+ẑ_P`; inner surface `p_P = R_in·(sinθ cosλ, sinθ sinλ, cosθ)`, `θ ∈ [0°, 42.8°]`, `R_in = 71.0 mm`. The `P` origin is at the cavity bottom: `A_P = (0,0,0)`, the rim at `z_P = −55 mm`, and `+ẑ_P` points up out of the bowl.
- **Rover grid**: a 7 × 7 grid on the inner surface, indexed `(i, j) ∈ [0,6]²`; grid spacing maps to `λ = i·60°/6` and `θ = 6° + j·(36.8°/6)`.
- **Mirror normal**: the local outward normal of the inner surface at the rover, `n̂ = (sinθ cosλ, sinθ sinλ, cosθ)` in `P` (pointing *into* the cavity).
- **V-frame**: anchored to `P` for the dish, but the receiver and incoming beams are anchored to `W`'s gravity: the receiver is at `C_V = P_O + (0, 450, 0) mm`. A dual anchor again.
- **Deviation 1**: rovers are points on the grid, not physical bodies; placement is discrete.
- **Deviation 2**: beam reflection is analytic ray–plane reflection at each rover, with a 2-bounce limit.
- **Deviation 3**: the dish surface is the `R_in` sphere; the mirror normal is exact, so the puzzle is deterministic.
- **Deviation 4**: incoming beams are parallel rays from a directional source, not point sources.

## 3. Physical Play Space & Ergonomics

The bowl rests on the lap, tilted 12° toward the player. The right thumb and index reach into the cavity and slide to grid cells; the left hand steadies the bowl. The motion is a 10–40 mm slide with a light touch, no force.

| Parameter | Value | Rationale |
|---|---|---|
| Bowl position | Lap, 200–280 mm forward | Reachable interior |
| Bowl tilt | 12° toward player | Visibility |
| Grid cells | 7 × 7 across the dish | Solvable but rich |
| Touch force | 0.1–0.4 N | Placement, no press |
| Slide speed | 10–80 mm/s | Fine control |
| Gaze | 22–30° down, 350–500 mm | Dish + receiver |
| Session | 7 min; 70 s contacts; 12 s rest | Fine-motor stamina |

Fatigue mitigation: the bowl is on the lap; placement is a light touch, not a press; the grid snaps so the player can lift and re-place; each contact ends with a 5 s hand-open rest; a "grid dwell" meter discourages continuous dragging.

## 4. Prop Geometry & Registration

A glazed cereal bowl; the inner specular highlight is the primary registration feature, and the foot ring gives the vertical axis.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 75.0 mm ± 1.0 | Silhouette |
| Depth | 55 mm ± 1.5 | Apex to rim |
| Wall `t` | 4.0 mm ± 0.5 | Rim step |
| Inner radius `R_in` | 71.0 mm | `R − t` |
| Rim circle radius | 50.9 mm | `R_in·sin 42.8°` |
| Mass | 380 g ± 20 | Balance |
| Registration | rim ellipse + specular fit | 30 frames |
| Re-acquisition | `ε_R < 2.0 mm`, `ε_θ < 3°` | After blur |

Feature atlas (inner dish):

| Station | `θ` from apex | Grid rows | Feature |
|---|---|---|---|
| S0 | 0–8° | j = 0 | Dish floor |
| S1 | 8–20° | j = 1–2 | Lower bowl |
| S2 | 20–33° | j = 3–4 | Main reflector |
| S3 | 33–42.8° | j = 5–6 | Upper rim |
| S4 | rim | — | Placement limit |

Colliders: inner spherical cap, rim torus, foot ring. Registration publishes `P` and the interior normal at 72 Hz.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `R_thumb_tip` | S0–S3 | Touch + slide | `RoverA(i, j)` |
| I5.2 | `R_index_tip` | S0–S3 | Touch + slide | `RoverB(i, j)` |
| I5.3 | `R_thumb_tip` + `R_index_tip` | same cell | Pinch | `SwapRovers` |
| I5.4 | `L_index_tip` | S4 rim | Tap | `Ping` (highlight receiver) |
| I5.5 | either hand | — | 600 ms off-bowl | `Rest` |
| I5.6 | `R_middle_tip` | S2 | Tap | `RotateRover` (90° mirror yaw) |

Grid snapping: `i = round(λ / 10°)`, `j = round((θ − 6°) / 6.13°)`, clamped to `[0,6]`. A rover can only be placed on a free cell; two rovers cannot share a cell.

## 6. Input Kinematics & Signal Processing

- **Projection**: `p̂ = (p_tip − P_O)/|p_tip − P_O|`; `θ = acos(p̂·ẑ_P)`, `λ = atan2(p̂·ŷ_P, p̂·x̂_P)`.
- **Snap**: to the nearest grid cell; commit after 60 ms dwell.
- **Mirror normal**: `n̂ = (sinθ cosλ, sinθ sinλ, cosθ)`; the rover's mirror yaw adds a rotation `R_y(φ_r)` about `n̂` (default `φ_r = 0`).
- **Incoming beam**: direction `d̂_in` from the source; parallel rays with origin offset `o`.
- **Reflection**: `d̂_out = d̂_in − 2(d̂_in·n̂)n̂`; the ray starts at the rover point `r_V`.
- **Receiver test**: the ray must pass within `r_rec = 25 mm` of the receiver centre `C_V` within 600 mm; distance from line to point `d = |(C_V − r_V) × d̂_out|`.
- **Second bounce**: if the first ray hits another rover (within 10 mm of a cell point), reflect again; a 2-bounce limit prevents infinite loops.
- **Lock**: the receiver absorbs the beam when `d ≤ 25 mm` for 0.3 s.
- **Rover charge**: 5 placements per contact; each placement costs 1 charge; recharges at 1 per 4 s.

## 7. Virtual World Layout & Coordinate Mapping

`V` is centred at the dish bottom `P_O`; the dish is the `R_in` cap; the receiver hangs at `C_V = P_O + (0, 450, 0) mm` in world-up coordinates. Incoming beams come from elevations `ε_in ∈ [25°, 75°]` and azimuths `α_in`.

| Entity | Anchoring | Notes |
|---|---|---|
| Dish | `P` | `R_in` cap |
| Rovers | `P` grid | 3 rovers |
| Receiver | `W`-up | 450 mm above |
| Beams | `W` | parallel rays |
| Jungle | `V` | decorative ring |
| Grid overlay | `P` | 7 × 7 |

The mismatch between the `P` dish frame and the `W` receiver frame is the core puzzle: the player must place mirrors in the tilted bowl such that a world-vertical beam reaches a world-fixed point.

## 8. Gameplay Systems & Rules

**Rovers.** Three rovers: A and B are player-controlled, C is autonomous and patrols a fixed cell sequence every 4 s. Each has a mirror with yaw `φ_r` (0°, 90°, 180°, or 270° via `RotateRover`).

**Beam.** Each contact fires one beam. The beam reflects up to twice. A hit on the receiver within 25 mm scores:
`S = 300 · (1 − d/25) · (1 + 0.5·n_bounces)` — a two-bounce solution is worth more.

**Charge.** Each placement costs 1 charge (max 5); recharges at 1 per 4 s. Moving a rover off-grid or swapping counts as one placement.

**Autonomous rover.** Rover C moves every 4 s; if the beam hits it, the reflection uses its current normal — a moving element the player must account for.

**Jungle floor.** A beam that misses the receiver and leaves the dish hits the jungle floor: lose one of three "beams".

**Score chain.** Consecutive receiver hits without a miss multiply by `1 + 0.3·n` (cap 4).

**Win.** Land 5 beams in the receiver before 3 misses.

**Assist ping.** `L_index_tip` tap highlights the receiver's projection onto the dish and costs 50 points.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | bowl seen | `Detect` | 2.0 s | `P` locked |
| `Detect` | calibration / hit | `Place` | 1.0 s | beam telegraph |
| `Place` | detect end | `Reflect` | 0.5 s | rover moves |
| `Reflect` | placement committed | `Align` / `Miss` | instant | ray cast |
| `Align` | ray near receiver | `Collect` | 0.3 s | lock ring |
| `Collect` | receiver hit | `Detect` | 0.6 s | score |
| `Miss` | ray hits floor | `Detect` / `Score` | 1.0 s | beam lost |
| `ContactClear` | 5 hits | `Victory` | 2.0 s | tally |
| `Victory` | 5 beams | `Score` | 2.0 s | fanfare |
| `Score` | victory / 3 misses | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Inner cap | Fingertips | Placement sensor | ceramic/skin |
| Rim torus | `L_index_tip` | Ping trigger | ceramic/skin |
| Beam ray | Rover mirror | Reflection | mirror |
| Beam ray | Receiver sphere | Absorb | — |
| Beam ray | Dish surface | Reflect / escape | ceramic |
| Beam ray | Jungle floor | Miss | — |

No rigid-body dynamics: rovers are grid points and beams are analytic rays. The reflection is exact by construction, so there is no numerical drift. A 2-bounce cap prevents runaway recursion. CCD not required. The receiver is a 25 mm sphere; at the dish edge (71 mm radius) the angular tolerance is `atan(25/450) ≈ 3.2°`, which is the puzzle's difficulty knob.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → lap → jungle ring → dish interior → grid overlay → rovers → beam lines (additive) → receiver → hand mesh → sparkles → UI.

Occlusion cases:
1. **Fingers on the dish**: fingertips render in front of the inner surface; the grid cell highlight is clipped by the hand.
2. **Rovers behind the rim**: a rover near the far rim is partially hidden by the near rim.
3. **Beam through the hand**: the additive beam is depth-tested so it dims behind fingers.
4. **Receiver above**: the receiver is drawn above the bowl and parallaxes correctly with head motion.
5. **UI**: the charge pips sit at the rim and are soft-occluded by the thumb.
6. **Grid**: the overlay is drawn on the surface and depth-tested so it never floats.

VFX budget: dish 3 k tris, 3 rovers × 400 tris, beam pool 4 additive lines, receiver 800 tris, particles ≤ 400.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Beam telegraph | Rising synth | Shimmer line |
| Rover place | Click | Cell locks |
| Mirror rotate | Ratchet | Mirror turns |
| Reflect | Ping | Beam bends |
| Near receiver | Harmonic hum | Lock ring closes |
| Collect | Receiver chime | Beam absorbed |
| Miss | Distant thud | Jungle flash |
| Chain | Rising arpeggio | Ring stacks |
| Victory | Alien choir | Dish bloom |

## 13. Difficulty Progression & Tuning

| Level | My beams | Receiver radius | Rover C period | Incoming elevation | Charges |
|---|---|---|---|---|---|
| 1 | 3 | 35 mm | 6 s | 45° | 6 |
| 2 | 3 | 30 mm | 5 s | 50° | 5 |
| 3 | 3 | 25 mm | 4 s | 55° | 5 |
| 4 | 2 | 22 mm | 3 s | 60° | 4 |
| 5 | 2 | 18 mm | 2.5 s | 65° | 4 |

**Adaptive rule.** Track hit rate `h` over the last 4 beams. If `h ≥ 0.8`, shrink the receiver 3 mm or speed up rover C by 0.5 s. If `h ≤ 0.3`, enlarge the receiver and slow rover C. Clamp the receiver to `[15, 40] mm`.

## 14. Edge Cases & Failure Recovery

- **Two rovers on one cell**: the second placement is rejected with a red flash; no charge spent.
- **Beam hits the rim**: reflect once at the rim torus, then escape; counts as a miss if it leaves the dish.
- **Beam hits rover C**: allowed; rover C acts as a third mirror.
- **Bowl moved**: re-anchor `P`; the grid and rovers move with the bowl, but the receiver stays world-fixed, so the puzzle re-solves.
- **Tracking loss**: hold placements for ≤ 300 ms; longer pauses.
- **Both fingertips on the same cell**: treated as a swap gesture, not two placements.
- **Charge exhausted**: no placement until recharge; the beam telegraph still runs.
- **Pause during a beam**: the analytic ray is recomputed on resume.

## 15. Implementation Architecture

Classes: `PropRegistration`, `DishProjector`, `GridModel`, `Rover`, `Mirror`, `BeamCaster`, `Receiver`, `ChargeModel`, `ContactDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P`; `DishProjector → (θ, λ)`; `GridModel → cell`; `Rover` updates; `BeamCaster → ray hits`; `Receiver → Collect`; `ChargeModel` gates placements; all → `ContactDirector`, `ScoreLedger`.

```text
onBeam():
    origin = sourceOrigin()
    d = incomingDir()
    for bounce in 0..2:
        hit = firstRoverOrReceiver(origin, d)
        if hit == RECEIVER: return collect(distanceToCenter)
        if hit == ROVER:
            n = mirrorNormal(hit.rover)
            d = reflect(d, n)
            origin = hit.point
            bounces += 1
        else: return miss()
    return miss()
```

```text
onPlaceRover(tip, rover):
    if charges <= 0: return
    (i, j) = snapToGrid(projectToDish(tip))
    if occupied(i, j): return
    rover.cell = (i, j); charges -= 1
    audio.play("click"); grid.flash(i, j)
    recomputeBeam()
```

## 16. Acceptance Test Checklist

1. **Projection**: a fingertip at a known `(θ, λ)` snaps to the correct grid cell in ≥ 98% of trials.
2. **Normal**: the mirror normal at `(θ, λ)` equals the analytic inner-surface normal within 0.5°.
3. **Reflection**: a 45° incidence on a flat mirror reflects at 45° ± 0.5°.
4. **Receiver**: a ray passing within 25 mm of `C_V` collects; at 30 mm it misses.
5. **Two-bounce**: a two-bounce ray scores 1.5× a one-bounce ray.
6. **Charge**: each placement costs 1 and recharges at 1 per 4 s.
7. **Occupancy**: a second rover on an occupied cell is rejected with no charge spent.
8. **Miss**: a beam leaving the dish costs one of three beams.
9. **Chain**: two hits without a miss yield a 1.3× multiplier.
10. **Re-anchor**: moving the bowl preserves grid-relative rover positions.
11. **Rover C**: the autonomous rover moves every 4 s and reflects correctly.
12. **Occlusion**: a far-rim rover is hidden by the near rim.
13. **Pause**: palm cover freezes placement and resumes cleanly.
14. **Performance**: dish + 3 rovers + beams sustain 90 fps.
