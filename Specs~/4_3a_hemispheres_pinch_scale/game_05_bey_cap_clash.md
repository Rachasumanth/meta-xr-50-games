# Bey-Cap Clash

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Bey-Cap Clash |
| Slug | `bey_cap_clash` |
| Physical prop | Rounded bottle cap or acorn cap, convex-down; `R = 16.0 mm`, `t = 0.5 mm`, mass `2.0 g`; knurled steel crown cap |
| Family / sub-shape | Hemisphere, dome-down (convex contact), pinch scale |
| Collider family | Convex dome contact patch + rim torus + spin sensor |
| Scale class | Prop 1:1 (32 mm diameter); virtual stadium 150 mm diameter |
| Source concept quote | "Rest the rounded convex bottom on a smooth book cover and flick the rim with your thumb and index finger so it spins and wobbles in place... As it spins inside a 15cm virtual stadium on your book, tap the book surface near the cap to tilt the stadium floor and ram your spinning cap into AI phantom tops." |
| Physical input | Thumb/index flick the rim; fingertip taps on the book surface around the cap |
| Session length | 7 min (5 tournaments × 3 bouts + onboarding) |
| Lose condition | Player cap ring-out or spin reaches zero during a live bout |

## 1. Fantasy & Core Loop

The book cover becomes a neon stadium bowl at 1:8 scale, ringed by a roaring crowd of wireframe spectators. The bottle cap is the player's top: a knurled steel disc that throws a sawblade aura when spun. Three AI phantom tops orbit the stadium, each with its own spin direction and aggression. The player flicks the cap to spin it, then taps the book near the cap to tilt the stadium floor: taps on one side raise that side, and the resulting gravity component accelerates both the player's cap and any phantom in that half. Ramming a phantom with higher spin transfers angular momentum and ejects it; a cap that loses spin or crosses the rim is ringed out.

1. **Launch (0.8 s).** A flick imparts spin `ω₀` and a small translation; the cap wobbles as it settles.
2. **Orbit (2–5 s).** The cap precesses; the player reads phantom headings.
3. **Tilt (0.3 s per tap).** A tap within 55 mm of the cap sets a floor tilt impulse toward or away from the tap.
4. **Clash (0.2 s).** Cap–phantom collision resolves spin transfer and impulse.
5. **Ring-out (1.0 s).** A body whose center crosses `r = 75 mm` is ejected.
6. **Bout resolve (2.0 s).** Last top standing wins; spin and damage tally.

A bout is 12–22 s; three bouts per tournament; five tournaments fill 7 min.

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

- **Radius and wall**: `R = 16.0 mm`, `t = 0.5 mm`, mass `m = 2.0 g`. The cap is modelled as a thin spherical cap; the effective contact radius at rest is `r_c = 4.0 mm`.
- **Orientation**: convex-down. The dome faces `−ẑ_P` in the gameplay pose; the cap spins about `ẑ_P`. P origin at the dome apex `(0,0,−R)` is offset to the geometric centre `C_P = (0,0,0)`; rim at `z_P = +R·cos(α_rim)`.
- **Contact**: the cap rests on its convex dome at the point `c_P = (0, 0, −R)` when upright; when wobbling by `β`, contact moves to `c_P = R·(sinβ cosλ_w, sinβ sinλ_w, −cosβ)`.
- **Spin**: `ω_s` about the body axis `ẑ_P`; wobble angle `β` between `ẑ_P` and the world up `Ŷ_W`.
- **V-frame**: anchored to `T` at the book-cover centre `Q`; axes `x̂_V = x̂_T`, `ŷ_V = ŷ_T`, `ẑ_V = ẑ_T`. Stadium radius `R_st = 75 mm`.
- **Deviation 1**: the real cap's translation and spin are *estimated* from `P` + visual yaw, not simulated; a virtual twin of 2 g is simulated on a 90 Hz rigid-body solver and reconciled to `P` each frame with a soft spring.
- **Deviation 2**: `ω_s` is measured from the cap's textured rim optical flow, clamped to `[0, 220 rad/s]`; below 8 rad/s the cap is "dead".
- **Deviation 3**: floor tilt from taps is virtual and applies to all tops; the physical book never moves.
- **Deviation 4**: cap–phantom collisions use 2-D discs at the rim, not the full hemisphere, because the sawblade aura is a rim phenomenon.

## 3. Physical Play Space & Ergonomics

The book lies flat on the table, its long edge parallel to the player. The cap is centred on the book. The right hand flicks from the near side; the left hand taps the book on either flank. Both wrists stay low and neutral. The head tilts 22–30° down.

| Parameter | Value | Rationale |
|---|---|---|
| Book footprint | ≥ 180 mm × 240 mm | Stadium 150 mm + tap margin |
| Cap start | book centre `Q` | Symmetric launch |
| Flick contact | rim at `θ = 85–95°` | Tangential impulse |
| Flick force | 0.4–1.2 N tangential | Spin 60–180 rad/s |
| Tap zone | `r ∈ [20, 70] mm` from cap | Floor tilt control |
| Tap force | 0.5–1.5 N | Reliable tap detect |
| Gaze | 22–30° down, 350–480 mm | Neck comfort |
| Session | 7 min; 3 bouts/tournament; 20 s rest | Flick stamina |

Fatigue mitigation: the cap needs no holding force; the left hand alternates tap sides; each bout ends with a 6 s hands-open rest; a tap-frequency meter warns if the player exceeds 3 taps/s (unnecessary exertion).

## 4. Prop Geometry & Registration

A knurled crown cap with a smooth convex underside. The knurling gives strong optical-flow features on the rim, which is what makes spin estimation possible.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 16.0 mm ± 0.4 | Rim circle |
| Dome sagitta | 3.0 mm | Contact geometry |
| Effective contact radius `r_c` | 4.0 mm | Hertzian patch estimate |
| Wall `t` | 0.5 mm ± 0.1 | Steel |
| Mass | 2.0 g ± 0.2 | Balance |
| Moment of inertia `I_z` | `0.5·m·R² = 2.56×10⁻⁷ kg·m²` | Disc approximation |
| Registration | rim ellipse + dome normal | 24 frames |
| Re-acquisition | `ε_R < 1.0 mm`, `ε_β < 3°` | After blur |

Feature atlas:

| Station | Region | Feature |
|---|---|---|
| S0 | dome apex | Contact point (smooth) |
| S1 | dome 0–40° | Specular band |
| S2 | dome 40–75° | Knurl transition |
| S3 | rim torus | Knurled teeth (spin features) |
| S4 | inner cavity | Sensor only (not used) |

Colliders: convex dome cap (`R`, sagitta 3 mm), rim torus, and a 2-D rim disc of radius `R` for top-vs-top combat. Registration publishes `P` and a scalar spin `ω_s` with 15 ms latency.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `R_thumb_tip` + `R_index_tip` | S3 rim | Lateral flick 0.4–1.2 N | `Launch(ω₀, v₀)` |
| I5.2 | `L_index_tip` | book surface near cap | Tap 0.5 N | `TiltImpulse(p_tap)` |
| I5.3 | `R_middle_tip` | S3 rim | Second flick | `Boost(ω)` |
| I5.4 | `L_thumb_tip` | book surface | Tap opposite side | `TiltImpulse` |
| I5.5 | both palms | book | Flat press 400 ms | `Brace` (stops tilt) |
| I5.6 | any | — | 1.0 s palm cover | `Pause` |

Tap position `p_tap ∈ V`; tilt direction `t̂ = normalize(p_tap − cap_pos)`; impulse magnitude `J_t = 0.06·F_tap` N·s applied to every top's horizontal velocity.

## 6. Input Kinematics & Signal Processing

- **Spin estimate**: optical flow on the knurled rim yields `ω_s`; Kalman filter with process noise `q = 400 rad²/s³`, measurement noise `r = 25 rad²/s²`.
- **Launch**: tangential flick velocity `v_t` from the two fingertips over 60 ms; `ω₀ = v_t / R`, clamped to 220 rad/s. `v₀` (translation) = 0.25·`v_t` along the flick direction.
- **Wobble**: `β` from the angle between `q_P·ẑ_P` and `Ŷ_V`; wobble decays `β̈ = −ω_n²β − 2ζω_nβ̇` with `ω_n = 34 rad/s`, `ζ = 0.18`; nutation frequency `f_n = 8–14 Hz`.
- **Tap detect**: book-plane contact with `F_n > 0.5 N` and dwell 40–90 ms; taps are debounced 120 ms.
- **Tilt impulse**: `Δv = J_t/m_top`, `m_top = 2 g` → a 1.0 N tap gives `Δv = 30 mm/s`.
- **Spin decay**: `ω̇_s = −(τ_f + τ_w)/I_z`, friction torque `τ_f = µ_c·m·g·r_c` with `µ_c = 0.18`, wobble loss `τ_w = 0.02·I_z·ω_s·β`.
- **Dead cap**: `ω_s < 8 rad/s` for 500 ms ⇒ `Dead`.
- **Launch cooldown**: 700 ms after a flick before spin re-estimation settles.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 150 mm-diameter stadium disc at `z_V = 0` centred on the book. Gravity is `g_V = 9.81 m/s²` along `−ẑ_V`, tilted by the floor normal `n̂_f`. Tops are 2-D discs of radius `16 mm` in this plane.

| Entity | Anchoring | Notes |
|---|---|---|
| Stadium | `T` at `Q` | radius 75 mm |
| Player top | `P` mapped to `V` | soft-reconciled |
| Phantom tops | `V` | 2–4 bodies |
| Floor normal | `V` | tilt state |
| Tap ripples | `V` | 0.4 s decay |
| Rim wall | `V` | `r = 75 mm` |

The virtual cap position is `p_V = φ(P_O − Q)` with `φ` the projection onto the book plane; height is ignored for combat. The floor normal `n̂_f` tilts by up to 8° toward the last tap; tilt decays with `τ = 1.4 s`.

## 8. Gameplay Systems & Rules

**Tops.** Player and phantom tops have `ω_s`, position `p`, velocity `v`, mass 2 g, radius 16 mm, and `hp = 3`. Phantom AI steering: seek the player if `ω_s > 40`, flee otherwise.

**Collision impulse (equal masses).** Along the line of centres `n̂`, relative normal speed `v_n = (v_a − v_b)·n̂`. Exchange `v_a' = v_a − (1+e)/2·v_n·n̂`, `v_b' = v_b + (1+e)/2·v_n·n̂`, `e = 0.55`. Spin transfer: `Δω = 0.25·|v_n|/R·sign(ω_a − ω_b)`; the faster top loses spin, the slower gains.

**Spin advantage.** If `ω_a > 1.25·ω_b`, the hit deals 1 hp and knocks the target back at `1.4×`; otherwise both bounce with no hp loss.

**Ring-out.** A top whose `|p| > 75 mm` is ejected. Player ring-out ends the bout as a loss.

**Floor tilt.** Each tap adds `Δn̂ = 8°·t̂` with `τ = 1.4 s` decay. Gravity component `a = g·sinθ_f` along `t̂` accelerates all tops.

**Bout.** Last top with `hp > 0` and `|p| < 75 mm` wins. Three bouts per tournament; win 2 to advance.

**Score.** `S = 200·hp_left + 50·bouts_won + 10·(ω_at_end/10)`.

**Win.** Clear 5 tournaments. **Lose.** Lose 2 bouts in a tournament.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | prop seen | `Ready` | 2.0 s | `P`, `ω_s` |
| `Ready` | calibration | `Launch` | flick | idle spin 0 |
| `Launch` | flick | `Bout` | 0.8 s | spin set |
| `Bout` | launch | `Clash` / `RingOut` / `Dead` | live | AI active |
| `Clash` | overlap | `Bout` | 0.2 s | impulse |
| `RingOut` | `|p| > 75` | `BoutResolve` | 1.0 s | eject VFX |
| `Dead` | `ω_s < 8` 500 ms | `BoutResolve` | 0.5 s | cap still |
| `BoutResolve` | ring-out / dead / last | `Ready` / `TournamentEnd` | 2.0 s | score |
| `TournamentEnd` | 2 bouts won/lost | `Ready` / `Victory` | 3.0 s | bracket |
| `Victory` | 5 tournaments | `Score` | 2.0 s | fanfare |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Cap dome | Book `T` | Support + friction | steel/paper |
| Cap rim disc | Phantom rim disc | Impulse + spin transfer | steel/steel |
| Cap rim torus | Fingertip | Flick impulse | steel/skin |
| Cap body | Stadium wall | Eject (trigger) | energy |
| Tap ripple | Top body | `Δv` impulse | — |
| Floor normal | All tops | Gravity acceleration | — |

Rigid-body constants: `m = 2 g`, `I_z = 2.56×10⁻⁷ kg·m²`, `I_x = I_y ≈ 1.28×10⁻⁷ kg·m²`. Rolling friction torque `τ_f = 1.13×10⁻⁴ N·m` at rest. At `ω_s = 100 rad/s` the decay rate is `τ/I_z ≈ 0.44 rad/s²`, giving ~230 s of free spin — deliberately long, so the wobble and wall losses dominate. CCD: tops move ≤ 3 mm/tick at 250 mm/s, so a 20 mm/tick sweep suffices; wall ejection is a radius check with a 2 mm margin.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → book surface → stadium floor grid → sawblade auras (additive) → tops → rim wall → hand mesh → sparks → UI.

Occlusion cases:
1. **Finger over rim**: the flicking fingertip is drawn in front of the cap; the aura is clipped by the hand depth.
2. **Cap behind a phantom**: correct mutual occlusion between 2-D tops via depth offsetting (player at `z_V = +1 mm`).
3. **Tap hand over stadium**: the tapping hand occludes the floor grid but not the tops (tops render after the hand with a depth bias).
4. **Book edge**: the stadium is clipped to the detected book footprint; outside the book the virtual floor fades.
5. **UI**: the tournament bracket floats 120 mm above the book and is soft-occluded by the forearm.
6. **Aura**: the sawblade sprite is depth-tested against the book so it never floats detached when the cap lifts.

VFX budget: 4 tops × 900 tris, auras 4 additive quads, sparks ≤ 300, floor grid 1 shader.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Flick launch | Metallic zing | Spin trail |
| Spin decay | Descending whir | Aura shrinks |
| Tap | Book thump | Floor tilt ripple |
| Clash | Steel clang | Spark burst |
| Spin advantage | Power chord | Red aura flare |
| Ring-out | Falling whistle + crash | Top tumbles off |
| Dead cap | Wobble rattle | Aura dies |
| Bout win | Crowd roar | Confetti |
| Tournament win | Anthem | Trophy |

## 13. Difficulty Progression & Tuning

| Level | Phantom tops | AI seek speed (mm/s) | Phantom spin (rad/s) | Floor tilt max | Spin decay ×|
|---|---|---|---|---|---|
| 1 | 1 | 30 | 40 | 8° | 1.0 |
| 2 | 2 | 45 | 60 | 9° | 1.1 |
| 3 | 2 | 60 | 80 | 10° | 1.2 |
| 4 | 3 | 75 | 100 | 11° | 1.3 |
| 5 | 4 | 90 | 120 | 12° | 1.4 |

**Adaptive rule.** Track bout win rate `w` over the last 5 bouts. If `w ≥ 0.8`, add one phantom (cap 4) or +15 mm/s (cap 90). If `w ≤ 0.3`, remove one phantom (floor 1) or −15 mm/s. Spin decay multiplier scales `τ_f` and never drops below 1.0.

## 14. Edge Cases & Failure Recovery

- **Cap leaves the book**: ring-out is triggered at the stadium edge, not the book edge; if `P_O` leaves the book, freeze the bout and ghost the cap back to `Q` after 1 s.
- **Cap tips over**: if `β > 60°`, treat as `Dead` and prompt a re-launch.
- **Spin ambiguity at low `ω_s`**: below 20 rad/s, optical flow is unreliable; blend to `P` yaw integration and clamp.
- **Tracking loss**: hold the last `ω_s` for ≤ 300 ms; longer freezes the bout.
- **Two taps in one frame**: sum impulses but cap total `Δv` at 60 mm/s per frame.
- **Phantom stuck on wall**: apply a 12 mm/s inward nudge after 0.5 s of contact.
- **Player uses two hands to flick**: allowed; blends the two tangential velocities with equal weight.
- **Pause during a clash**: collision resolves to the pre-clash state on resume.

## 15. Implementation Architecture

Classes: `PropRegistration`, `SpinEstimator`, `RigidBody2D`, `TopAgent`, `PhantomAI`, `StadiumModel`, `FloorTiltController`, `TapDetector`, `ClashResolver`, `RingOutJudge`, `TournamentDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P, ω_s`; `SpinEstimator → ω_s`; `TapDetector → TiltImpulse`; `RigidBody2D` integrates tops; `ClashResolver` emits `ClashEvent`; `RingOutJudge → BoutResolve`; all → `ScoreLedger`, `AudioCueBus`.

```text
onFixedTick(dt = 1/90):
    n = tiltController.normal(lastTap, dt)        # up to 12deg, tau 1.4s
    a_g = g * horizontal_component(n)
    for top in tops:
        top.v += (a_g + top.aiAccel) * dt
        top.v -= top.v * drag * dt
        top.omega -= (frictionTorque(top) + wobbleLoss(top)) / I_z * dt
        top.p += top.v * dt
    for (a,b) in pairs(tops):
        if dist(a.p,b.p) < 2*R:
            n_hat = normalize(b.p - a.p)
            vn = dot(a.v - b.v, n_hat)
            a.v -= (1+e)/2 * vn * n_hat
            b.v += (1+e)/2 * vn * n_hat
            transferSpin(a, b, vn)
            if abs(a.omega) > 1.25*abs(b.omega): b.hp -= 1
```

```text
onFlick(tipA, tipB):
    vt = (velocity(tipA) + velocity(tipB)) / 2
    tangential = vt - dot(vt, up) * up
    omega0 = clamp(length(tangential) / R, 0, 220 rad/s)
    playerTop.omega = omega0
    playerTop.v = 0.25 * tangential
    state = Launch
```

## 16. Acceptance Test Checklist

1. **Spin law**: a 1.0 m/s tangential flick yields `ω₀ = 62.5 ± 4 rad/s`.
2. **Wobble frequency**: an initial `β = 10°` nutates at 8–14 Hz and decays to < 1° in < 2.5 s.
3. **Friction decay**: free spin at 100 rad/s decays at 0.44 ± 0.06 rad/s² with no wobble.
4. **Tap impulse**: a 1.0 N tap moves a 2 g top by 30 ± 4 mm/s.
5. **Tilt max**: repeated taps never exceed the level's floor-tilt cap.
6. **Clash impulse**: head-on equal-speed tops exchange velocities with `e = 0.55 ± 0.05`.
7. **Spin advantage**: a 1.3× spin ratio deals 1 hp; a 1.1× ratio deals 0.
8. **Ring-out**: a top crossing 75 mm is ejected within one tick.
9. **Dead cap**: spin below 8 rad/s for 500 ms ends the bout.
10. **Tournament**: winning 2 of 3 bouts advances; losing 2 ends the tournament.
11. **Occlusion**: the flicking fingertip hides the rim in 100% of calibrated frames.
12. **Pause**: palm cover freezes all tops and resumes at the pre-clash state.
13. **Adaptive**: 5 straight wins raises the phantom count by one (cap 4).
14. **Performance**: 4 tops + auras + sparks sustain 90 fps.
