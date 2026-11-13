# Paratrooper Basket

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Paratrooper Basket |
| Slug | `paratrooper_basket` |
| Physical prop | Inverted bottle cap, concave-up; `R = 15.0 mm`, `t = 0.5 mm`, mass `2.0 g`; steel crown cap |
| Family / sub-shape | Hemisphere, bowl-up, pinch scale |
| Collider family | Concave cavity + rim torus + virtual trampoline net |
| Scale class | Prop 1:1 (30 mm aperture); virtual airspace 150 mm × 150 mm × 260 mm |
| Source concept quote | "Hold the cap concave-side-up between thumb and forefinger and move it smoothly within a 15cm horizontal square above your lap... Position the concave cavity of your bottle cap directly beneath falling paratroopers to catch them safely on a virtual trampoline net stretched across the cap rim." |
| Physical input | Thumb + index pinch the rim; planar translation in a 150 mm square above the lap |
| Session length | 6 min (5 drops × 60 s + onboarding) |
| Lose condition | Five troopers lost |

## 1. Fantasy & Core Loop

A burning troop-transport blimp hangs in the sky above the lap, its envelope peeling into embers. Tiny paratroopers leap from a side hatch and sway down through crosswinds, their chutes snapping open at 140 mm above the landing plane. The bottle cap is the recovery basket: a virtual trampoline net is stretched across its 30 mm rim, and a trooper who lands inside the rim with a soft enough vertical speed bounces once, cheers, and is recovered. The player slides the cap in a 150 mm square, reading each trooper's wind-drift vector from the chute's lean and the drifting smoke ribbons.

1. **Spot the jump (1.0 s).** A hatch flash and a smoke puff mark the drop point; the trooper steps into the void.
2. **Track the drift (1.4–2.2 s).** The trooper falls at 120 mm/s while the wind pushes laterally; the chute lean encodes drift.
3. **Position (0.6 s).** The player slides the cap so its rim centre is under the projected landing point.
4. **Catch (0.15 s).** Net contact if the trooper's landing point is within `r ≤ 14 mm` and `v_z ≥ −260 mm/s`.
5. **Bounce (0.4 s).** The net reflects the trooper upward at `e = 0.45`; a second catch scores double.
6. **Drop resolve (2.0 s).** Recovered troopers tally; the blimp banks for the next pass.

A trooper cycle is 4–6 s; a drop is 60 s; five drops fill the session.

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

- **Radius and wall**: `R = 15.0 mm`, `t = 0.5 mm`, inner radius `R_in = 14.5 mm`, mass `m = 2.0 g`.
- **Orientation**: bowl-up (inverted cap). Cavity faces `+ẑ_P`; inner surface `p_P = R·(sinθ cosλ, sinθ sinλ, cosθ)`, `θ ∈ [0°, 90°]`. Rim at `z_P = 0`.
- **P origin**: rim-circle centre. The cap is held aloft, so `P_O` floats. The *catch plane* is the rim plane `z_P = 0`.
- **Net plane**: virtual trampoline at `z_P = +1.0 mm`, radius `R_net = 13.0 mm` (inside the rim torus).
- **V-frame**: anchored to `T` at the lap centre `Q`; axes `x̂_V = x̂_T`, `ŷ_V = ŷ_T`, `ẑ_V = ẑ_T`. The allowed cap centre is the square `|x_V| ≤ 75 mm`, `|y_V| ≤ 75 mm`, `z_V ∈ [120, 260] mm`.
- **Deviation 1**: the cap's translation is tracked, not simulated; there is no gravity on the prop because the hand holds it. A 12 mm/s velocity estimate is used for "scoop" assists.
- **Deviation 2**: the cavity is a *trigger* volume, not a force collider. Troopers pass through the mesh and interact only with the net disc.
- **Deviation 3**: the cap's tilt is ignored for catching; only `P_O` and the rim plane orientation within 20° of horizontal count. Beyond 20°, the net's effective radius shrinks by `cosβ`.
- **Deviation 4**: the 150 mm square is soft — the cap may overshoot 15 mm and is pulled back with a spring of `k = 2.0 N/m`.

## 3. Physical Play Space & Ergonomics

The player holds the cap in the right hand at 150–230 mm above the lap, elbow resting on the chair arm. The left hand stays relaxed. The play square is 150 mm on a side, centred over the thighs; the player slides the whole forearm, not just the wrist, to cover it. The head tilts 20–28° down to see the falling troopers.

| Parameter | Value | Rationale |
|---|---|---|
| Hold height | 150–230 mm above lap | Within arm's comfortable range |
| Play square | 150 mm × 150 mm | Forearm sweep, not wrist-only |
| Cap attitude | within 20° of horizontal | Net must face up |
| Pinch force | 0.1–0.35 N | 2 g prop |
| Slide speed | 0–260 mm/s, typical 140 | Crossing 150 mm in ~1.1 s |
| Gaze | 20–28° down, 380–520 mm | Neck comfort |
| Session | 6 min; 60 s drops; 12 s rest | Arm elevation stamina |

Fatigue mitigation: the elbow is supported, so the shoulder carries little load; the cap can be lowered into a "rest ring" between drops where catch scoring pauses; a hold-timer dims the sky after 50 s of continuous elevation; the left hand is free.

## 4. Prop Geometry & Registration

An inverted crown cap: the concave side is smooth enamel, the rim is crimped, and the outer surface is knurled. Held concave-up, the crimped rim gives a strong circular feature for registration.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 15.0 mm ± 0.4 | Rim circle |
| Wall `t` | 0.5 mm ± 0.1 | Steel |
| Inner radius `R_in` | 14.5 mm | `R − t` |
| Rim crimp depth | 0.8 mm | Registration feature |
| Mass | 2.0 g ± 0.2 | Balance |
| Net radius `R_net` | 13.0 mm | 2 mm inside rim |
| Registration | rim ellipse + plane normal | 20 frames |
| Re-acquisition | `ε_R < 1.0 mm`, normal < 3° | After blur |

Feature atlas:

| Station | `θ` | Region | Feature |
|---|---|---|---|
| S0 | 0–20° | cavity bottom | smooth enamel |
| S1 | 20–70° | inner wall | specular ring |
| S2 | 70–88° | inner rim | shadow band |
| S3 | 88–90° | rim torus | crimp teeth (grip) |
| S4 | 90° | outer lip | knurl |

Colliders: rim torus (grip), a trigger net disc at `R_net`, and a cavity mesh used only for visual depth. Registration publishes `P_O` and the rim normal `n̂_P`.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `R_thumb_tip`, `R_index_tip` | S3 rim | Pinch hold | `Held` |
| I5.2 | `P` translation | — | Planar slide | `CapPos(p_V)` |
| I5.3 | `R_wrist` | — | Vertical raise | `CapHeight(z_V)` |
| I5.4 | `L_index_tip` | outside | Tap | `GuidePing` (highlights landing point) |
| I5.5 | both hands open | — | 600 ms | `LowerArm` |
| I5.6 | `R_middle_tip` | S4 outer | Second-finger brace | `Stabilize` (tilt damping) |

The catch test uses `p_land = (p_trooper.x, p_trooper.y)` projected onto the `P` rim plane and `r_land = |p_land − P_O|`. `GuidePing` shows a 1 s ghost ring at the projected landing point and costs 25 points.

## 6. Input Kinematics & Signal Processing

- **Cap position filter**: one-euro filter with `f_c = 2 Hz` at rest, `10 Hz` during motion, `β = 0.5`; output `p_V`.
- **Cap velocity**: central difference over 3 frames; used for a "scoop assist" that adds up to 20 mm of lateral lead when `|v| > 120 mm/s`.
- **Tilt**: `β = acos(n̂_P · ẑ_V)`; effective net radius `R_eff = R_net·cosβ` for `β ≤ 20°`; beyond 20°, `R_eff = 0`.
- **Soft boundary**: outside the 75 mm half-square, `a = −k/m_b·(p − clamp(p))` with `k = 2 N/m`; the cap is never hard-stopped.
- **Trooper drift**: `v_wind = 30 + 18·sin(2π·t/7) mm/s` along a slowly rotating direction; chute lean `= atan2(v_wind, v_fall)`.
- **Catch condition**: `r_land ≤ R_eff`, `v_z ≤ −120 mm/s` (downward), and `v_z ≥ −260 mm/s`; slower is a "soft catch" (×1.5), faster is a "hard catch" (trooper stunned, ×0.5).
- **Net bounce**: `v_z' = −e·v_z`, `e = 0.45`, horizontal `v' = v + 0.3·v_cap`.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 150 × 150 × 260 mm airspace above the lap, with `z_V = 0` at the lap plane. The blimp hovers at `z_V = 240 mm`, drifting slowly along `x_V`. Troopers spawn at the hatch and fall under `g_V = 9.81 m/s²` scaled by 0.06 (a "toy gravity" of 0.59 m/s²) so the fall reads at 120 mm/s terminal.

| Entity | Anchoring | Notes |
|---|---|---|
| Airspace | `T` at `Q` | 150 × 150 × 260 mm |
| Blimp | `V` | slow drift |
| Troopers | `V` | 12 concurrent max |
| Catch net | `P` | `R_eff` disc |
| Wind ribbons | `V` | visual drift cue |
| Ghost ring | `V` | guide ping |

Vertical mapping: a trooper at `z_V = 120 mm` is at chest height; the catch plane is wherever the cap is, so the player "receives" troopers rather than chasing them vertically.

## 8. Gameplay Systems & Rules

**Troopers.** Each has `p`, `v`, chute state, and `value = 100`. Terminal fall speed 120 mm/s; horizontal drift 30–48 mm/s.

**Catch.** On crossing the net plane `z = z_cap`, if `r_land ≤ R_eff` and speed in `[−260, −120] mm/s`, the trooper is caught. Score:
`S = 100 · q_speed · (1 + 0.5·n_bounce) · q_drift`, where `q_speed = 1.5` for a soft catch, `1.0` normal, `0.5` hard; `n_bounce` counts net bounces without leaving the rim.

**Bounce chain.** Each net bounce within `r ≤ R_eff` awards `+50·n_bounce`; the trooper leaves after the second bounce with `+150` bonus.

**Miss.** A trooper whose `z_V < 40 mm` outside the net falls into the "splash zone": lose one trooper slot. Five losses end the session.

**Wind shifts.** Every 7 s the wind direction rotates 60° and speed changes by ±12 mm/s; a smoke ribbon telegraphs the change 1.5 s early.

**Guide ping.** `L_index_tip` tap costs 25 points and shows the projected landing point for 1.0 s.

**Win.** Recover 45 troopers across 5 drops (9/drop average). **Lose.** Five misses.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | prop seen | `DropIntro` | 2.0 s | `P` locked |
| `DropIntro` | calibration / drop clear | `Jump` | 2.0 s | blimp approach |
| `Jump` | drop active | `Fall` | hatch flash | trooper spawn |
| `Fall` | trooper spawned | `Catch` / `Miss` | `z < 40` | drift |
| `Catch` | net plane | `Bounce` / `Fall` | 0.15 s | score |
| `Bounce` | catch | `Fall` | 0.4 s | trooper up |
| `Miss` | below splash | `Jump` | 1.0 s | slot loss |
| `DropClear` | 60 s | `DropIntro` / `Victory` | 2.0 s | tally |
| `Victory` | 45 troopers | `Score` | 2.0 s | fanfare |
| `Score` | victory / 5 misses | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Net disc | Trooper capsule | Bounce `e = 0.45` | elastic |
| Rim torus | Fingertip | Pinch hold | steel/skin |
| Cavity mesh | Trooper | Trigger only (no force) | — |
| Trooper | Airspace floor | Splash/miss | — |
| Wind field | Trooper | `a = 0.4·(v_wind − v)` | — |
| Cap soft wall | `P` | Spring return | — |

Net behaves as a massless membrane with stiffness `k_net = 40 N/m` and damping `c_net = 0.8 N·s/m`; the bounce is an impulse model rather than a full soft-body sim. Gravity is scaled to `0.59 m/s²`. CCD: troopers move ≤ 2.2 mm/tick at 200 mm/s; the net is a plane test with a 1 mm tolerance, so no sweep is needed. Cap motion is tracked, not simulated, so no CCD applies to the prop.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → lap/legs → airspace sky gradient → blimp → troopers → chutes → smoke ribbons → net ring → cap rim → hand mesh → sparks → UI.

Occlusion cases:
1. **Cap over lap**: the virtual net is clipped by the real legs so it does not draw through the thighs.
2. **Fingers through the cavity**: fingertips holding the rim are drawn in front, and a trooper passing "inside" the cap is hidden by the cavity depth mesh.
3. **Trooper behind the cap**: correct occlusion as the trooper falls past the near rim.
4. **Chute over hand**: chutes are soft-occluded by the hand; the trooper silhouette remains visible as a glow.
5. **UI**: the drop timer floats at `z_V = 260 mm` and is occluded by the blimp.
6. **Ghost ring**: depth-tested to the net plane so it never floats above the rim.

VFX budget: 12 troopers × 500 tris, 12 chutes × 200 tris, blimp 1.5 k tris, particles ≤ 400.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Hatch jump | Pneumatic puff | Hatch flash |
| Chute open | Cloth snap | Chute blooms |
| Wind shift | Rising whoosh | Ribbons bend |
| Soft catch | Cheer + spring boing | Blue net flash |
| Hard catch | Thud + rattle | Amber net flash |
| Bounce | Rubber bounce | Net ripple |
| Miss | Distant crash | Red splash |
| Drop clear | Radio chatter | Blimp banks |
| Victory | Marching band | Medal card |

## 13. Difficulty Progression & Tuning

| Level | Troopers/drop | Fall speed (mm/s) | Wind speed (mm/s) | Catch radius (mm) | Wind period (s) |
|---|---|---|---|---|---|
| 1 | 6 | 100 | 25 | 13 | 10 |
| 2 | 7 | 110 | 32 | 12 | 9 |
| 3 | 8 | 120 | 38 | 11 | 8 |
| 4 | 9 | 130 | 43 | 10 | 7 |
| 5 | 10 | 140 | 48 | 9 | 6 |

**Adaptive rule.** Track catch rate `c` over the last 10 troopers. If `c ≥ 0.85`, increase wind by 4 mm/s or shorten the wind period by 0.5 s. If `c ≤ 0.5`, widen the catch radius by 1 mm (cap 14) and slow the wind. Never change two parameters in the same drop.

## 14. Edge Cases & Failure Recovery

- **Cap released**: if the pinch ends, the cap is ghosted at its last pose for 1.2 s; catching pauses; resume on re-grab.
- **Cap tilted > 20°**: `R_eff = 0`; a subtle red ring warns the player to level the cap.
- **Trooper spawned inside the rim**: instant catch at `n_bounce = 0` with a "lucky" cue.
- **Tracking loss**: freeze trooper physics for ≤ 400 ms; longer resets the drop.
- **Player's hand occludes the cap**: the net ring remains visible through a depth-ignoring overlay at 30% opacity.
- **Simultaneous catches**: both troopers are caught; the net is treated as infinitely wide for that tick.
- **Cap outside the square**: spring return is applied, but catches still count to avoid punishing overshoot.
- **Miss at the exact rim**: if `r_land ∈ (R_eff, R_eff + 2 mm]`, treat as a rim graze: trooper hangs on the rim for 0.5 s and can be scooped by a small slide.

## 15. Implementation Architecture

Classes: `PropRegistration`, `CapPoseFilter`, `AirspaceModel`, `BlimpDirector`, `Trooper`, `WindField`, `NetCollider`, `CatchResolver`, `GuidePing`, `DropDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P_O`; `CapPoseFilter → p_V`; `WindField → trooper accel`; `Trooper` integrates; `NetCollider + CatchResolver → CatchEvent/MissEvent`; `DropDirector → ScoreLedger`, `AudioCueBus`.

```text
onFixedTick(dt = 1/90):
    p_cap = capFilter.update(registration.position, dt)
    n = registration.rimNormal
    beta = acos(dot(n, up))
    R_eff = (beta <= 20deg) ? R_net * cos(beta) : 0
    for t in troopers:
        t.v += (g_toy + windDrag(t)) * dt
        t.p += t.v * dt
        if t.p.z <= p_cap.z and t.prev_z > p_cap.z:
            r = distance(t.p.xy, p_cap.xy)
            if r <= R_eff and -260 <= t.v.z <= -120:
                catch(t, r, t.v.z)
            elif t.p.z < 40mm:
                miss(t)
```

```text
onCatch(trooper, r, vz):
    q = (vz >= -160) ? 1.5 : (vz >= -220 ? 1.0 : 0.5)
    score.add(100 * q * (1 + 0.5*bounceCount))
    trooper.v.z = -0.45 * vz
    trooper.v.xy += 0.3 * capVelocity
    bounceCount += 1
    vfx.netRipple(capPose); audio.play("bounce")
```

## 16. Acceptance Test Checklist

1. **Catch geometry**: a trooper landing at `r = 12 mm` is caught; at `r = 15 mm` it is a graze, not a catch.
2. **Speed window**: `v_z = −100 mm/s` passes through; `−200 mm/s` catches; `−300 mm/s` is a hard catch.
3. **Tilt rule**: at `β = 25°` the effective radius is zero and no catch registers.
4. **Soft catch score**: `v_z = −140 mm/s` yields a 1.5× multiplier.
5. **Bounce chain**: a second bounce on the net adds `+50` and the trooper leaves after bounce 2.
6. **Wind telegraph**: the smoke ribbon changes 1.5 s before the wind vector.
7. **Miss**: a trooper reaching `z < 40 mm` outside the net costs exactly one slot.
8. **Guide ping**: an `L_index_tip` tap shows the landing ghost for 1.0 s and deducts 25 points.
9. **Release**: losing the pinch pauses catching for 1.2 s, then resumes.
10. **Soft boundary**: a cap 20 mm outside the square is pulled back within 0.6 s.
11. **Drop timer**: five 60 s drops complete in 300 s ± 1 s.
12. **Occlusion**: a trooper behind the cap is hidden by the cavity mesh.
13. **Adaptive**: 10 straight catches increase wind by 4 mm/s.
14. **Performance**: 12 troopers + chutes + blimp sustain 90 fps.
