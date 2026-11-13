# Equatorial Particle Ring

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Equatorial Particle Ring |
| Slug | `equatorial_particle_ring` |
| Physical prop | Baseball, true sphere, r = 37.0 mm (74 mm diameter), 145 g, leather seam; alternate orange r = 38.0 mm, 150 g |
| Family / sub-shape | True sphere, palm-scale (6–12 cm class), hand-held with a traced equator |
| Scale class | Prop 1:1; accelerator ring radius 39.2 mm; target belt 300–500 mm; tabletop scale |
| Source concept quote | "Hold the baseball in one hand and trace its equator with the other index finger. A synchrotron ring ignites around the ball; the faster you trace, the more energy the plasma bolt carries. Lift your finger at the tangent angle and sling the bolt at the floating targets." |
| Physical input | Non-dominant hand cradle; dominant index traces the equator band; lift at the tangent to fire |
| Session length | 8 min: onboarding 50 s + 3 runs × 120 s + 2 rests × 20 s + results |
| Lose condition | Fewer than 18 hits across 24 targets, or ring overheats 4 times in a run |

## 1. Fantasy & Core Loop

You are holding a miniature synchrotron in your palm. The baseball is the storage ring: a hair-thin torus of light wraps its equator, and your index finger is the injector. Drag it along the seam and particles pile up in the ring; drag faster and the stack energy climbs. The ring does not shoot where you point — it shoots *where you let go*. Lift at the right longitude and the tangent vector is the launch bearing; the bolt leaves the equator and crosses the room to a floating target. Trace too long and the ring cooks; trace too little and the bolt arrives too weak to break the target's shell.

1. **CRADLE (0.5 s).** The non-dominant hand closes around the ball, thumb and two fingers, and holds it still. A level indicator confirms the equator is within 15° of horizontal.
2. **TRACE (0.35–0.9 s).** The dominant index sweeps the equator band at 40–260 mm/s. Arc length `L` and peak angular speed `ω` accumulate into charge `Q`.
3. **AIM (continuous).** The tangent at the current longitude is drawn as a thin launch ray. A ghost sector on the target belt shows where a release now would go.
4. **RELEASE (0.08 s).** Lifting the index at `L ≥ 25 mm` and `ω_peak ≥ 2.5 rad/s` fires. The bolt energy is locked at lift; the launch azimuth is the tangent bearing at the lift longitude.
5. **IMPACT (0.3–2.8 s flight).** The bolt crosses to the target; the hit test is an angular sector at the predicted target position at impact. Hit, graze, or miss resolves with a heat penalty on a miss.
6. **COOL (0.5–2.0 s).** With no contact the ring cools at 9%/s. A north-pole double-tap vents 25 heat on a 15 s cooldown.

A shot cycle is 1.0–1.6 s including flight; eight targets take 60–100 s, inside the 120 s run.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the center, poles at `±ẑ_P`, equator in `x̂_P`–`ŷ_P`, no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]` from the equator, longitude `λ ∈ [0°, 360°)` from +x̂_P toward +ŷ_P; surface point `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; outward normal `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

`P` origin is the baseball centre; poles are `±ẑ_P`; radius `r = 37.0 mm`. The rolling law `ω = (n × v_t)/r` is computed at the tracing contact and converted into a reaction-torque estimate: at `|v_t| = 100 mm/s` the free-ball spin would be 2.70 rad/s (154.8 °/s), but the cradle holds the ball, so any tracked prop spin above 45 °/s is treated as grip slip and leaks charge. `V` is anchored to `P` in *position* and gravity-aligned in orientation: `V_origin = P_centre`, `ẑ_V` = up, `x̂_V` = the cradle's frozen forward bearing at round start. The accelerator ring is the circle `x² + y² = (1.06r)²` in the `z = 0` plane of `V`. Deviations:

1. **Ring is `V`-fixed, not `P`-fixed.** The ball may rotate inside the grip; the accelerator stays horizontal and world-stable, so the aim geometry does not tumble.
2. **Trace is a surface integral.** The control variable is contact longitude `λ(t)` on the equator band; `L = ∫ r|dλ|` and `ω = |dλ/dt|`, not rigid-body rotation.
3. **Bolt test is angular, not ballistic.** The bolt is a ray from the ring centre; gravity and drag are zero for the 0.3–2.8 s flight.
4. **Grip stability is a resource.** Prop spin or translation above threshold leaks charge, making the cradle a real physical task rather than a passive mount.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated or standing, elbows supported | The cradle must be stable for 120 s runs |
| Cradle position | 280–340 mm from the sternum, 80–110 mm below eye line | Keeps the ring in the foveal field |
| Ball orientation | Equator within 15° of horizontal | The ring is horizontal; a tilted equator breaks the seam mapping |
| Tracing hand | Index extended, wrist neutral, elbow-led sweep | Finger-only tracing fatigues the flexors in < 60 s |
| Trace speed | 40–260 mm/s (1.1–7.0 rad/s) | Covers slow aiming and fast charging |
| Trace band | φ ∈ [−12°, +12°] | 15.5 mm wide at r = 37 mm; comfortable pad contact |
| Session | 8 min; 3 × 120 s runs; 20 s rests | Static cradle + repetitive trace |
| Break cue | 20 s hand-open overlay between runs | Relieves both cradle and trace fatigue |

The cradle hand holds the ball with thumb, index, and middle fingers, leaving the equator band clear. The tracing index approaches from the front; the wrist stays within 20° of neutral. The player may rotate the torso to bring a different longitude to the front, but the ring is world-fixed, so turning the body turns the *launch bearing* relative to the targets. This is intentional: aiming is a whole-arm skill, not a finger skill. The off-hand (cradle) is not allowed to trace; contacts from its fingers are ignored except for the north-pole vent double-tap.

## 4. Prop Geometry & Registration

| Property | Baseball (default) | Orange |
|---|---|---|
| Diameter | 74.0 mm | 76.0 mm |
| Radius `r` | 37.0 mm | 38.0 mm |
| Mass | 145 g | 150 g |
| Inertia `I = 2/5·m·r²` | 7.94 × 10⁻⁵ kg·m² | 8.66 × 10⁻⁵ kg·m² |
| Surface | Leather + raised seam, µ = 0.65 | Peel, µ = 0.55 |
| Ring radius | 1.06r = 39.22 mm | 40.28 mm |
| Trace band width | 15.5 mm | 15.9 mm |

The seam is the registration anchor: the baseball's two great-circle seam arcs give ≥ 80 tracked features, which makes `q_P` observable and lets the game detect cradle slip. The orange is acceptable but has no seam; its rotation is dead-reckoned and the slip threshold tightens to 30 °/s.

**Ring geometry.** The accelerator torus lies in the `V` equator plane at radius `R_ring = 1.06r`, tube radius 1.2 mm. It is rendered as a 128-segment tube; the front 180° is drawn over the ball, the back 180° is depth-tested against the ball proxy. The injector node is the point on the ring at the tracing longitude `λ`.

**Surface-feature atlas (P-fixed):**

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| Equator trace band | −12° to +12° | all | Valid contact zone; 1 mm guide line at φ = 0 |
| Seam arc A | +35° to +55° sinusoid | 0°–180° | Registration feature, +80 points |
| Seam arc B | −35° to −55° sinusoid | 180°–360° | Registration feature |
| North pole vent | +70° to +90° | all | Double-tap coolant purge |
| Cradle pads | φ ≤ −35° | 120°–300° | Cradle hand contact, input ignored |
| Level reference | +15° | 0° | Tilt gate visual |
| Longitude ticks | 0° | every 30° | Aim ruler, 1 mm |

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 37 mm at `P` | Physical ball / depth proxy |
| C1 | Torus | major 39.22 mm, minor 1.2 mm | Accelerator ring visual/collider |
| C2 | Capsule | r = 1.8 mm, length 12 mm | Tracing fingertip |
| C3 | Sphere | r = 6–14 mm | Target body |
| C4 | Angular sector | half-width 5°–9°, elevation ±6° | Target hit volume |
| C5 | Disc | 8 mm radius at north pole | Vent double-tap target |
| C6 | Sphere | r = 37 mm | Cradle collision reference |

Registration: present at 0.9–1.3 m for 1.0 s, rotate through 150°, hold 0.3 s. Acceptance: RMS < 0.4 mm, radius error < 0.15 mm, seam visible ≥ 70% of frames. Re-acquisition: 120 mm cube, 1.2 s, 10 rim samples with RMS < 0.8 mm over 6 frames, snap 100 ms. If the seam is occluded for > 1.5 s, `q_P` freezes and the slip detector uses hand-pose deltas instead.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Cradle | `L_thumb_tip`, `L_index_tip`, `L_middle_tip`, `L_palm` | φ ≤ −35°, λ 120°–300°; ≥ 3 contacts | `HELD` state, ring armed |
| I5.2 | Forward trace | `R_index_tip` | φ −12° to +12°; `dλ/dt > 0` | Arc `L += r·dλ`, `ω` |
| I5.3 | Reverse trace | `R_index_tip` | same; `dλ/dt < 0` | Arc `L += r·|dλ|`, sign flips |
| I5.4 | Fire release | `R_index_tip` lift | `L ≥ 25 mm`, `ω_peak ≥ 2.5 rad/s` | Bolt with energy `E(L, ω)` |
| I5.5 | Soft release | `R_index_tip` lift | `L < 25 mm` or `ω_peak < 2.5 rad/s` | Re-grip, no shot, no heat |
| I5.6 | Aim hold | `R_index_tip` stationary | `ω < 0.5 rad/s`, contact > 0.4 s | Tangent ray steadies, aim assist +2° |
| I5.7 | Vent | `L_index_tip` double-tap | north pole φ ≥ +70° | −25 heat, 15 s cooldown |
| I5.8 | Middle brace | `R_middle_tip` on side | φ 25°–55° | No command, stabilises wrist |
| I5.9 | Grip slip | tracked `|ω_P| > 45 °/s` or `|v_P| > 60 mm/s` | — | Charge leaks 40%/s, "hold steady" cue |
| I5.10 | Drop | cradle contacts < 2 | — | `DROPPED`, ring disarms |

**Per-finger roles.** Cradle hand: thumb, index, and middle clamp the lower hemisphere; ring and pinky support the palm and are ignored. Tracing hand: the index is the only effector; the middle braces on the side to stabilise the wrist; the thumb is tucked and never contacts the ball (a thumb contact pauses tracing with a "thumb out" cue). The tracing hand's ring and pinky must stay clear of the equator band; if they contact it, the touch is ignored but the shot is disarmed for 200 ms.

Contact gating for trace: signed distance < 5 mm, age > 30 ms, latitude within ±12°, and the contact normal satisfies `n·ẑ_V > 0.55` (upper-side contact). Contacts on the underside are cradle-only. The index must approach from the front hemisphere; a rear-hemisphere contact is rejected with no cue.

## 6. Input Kinematics & Signal Processing

The tracing contact projects to longitude `λ = atan2(p_y, p_x)` in `V`, with latitude `φ = asin(p_z/r)`. The contact is valid only inside the band. Per 90 Hz tick:

`dλ = wrap(λ_t − λ_{t−1})`, `ds = r·|dλ|·cosφ`, `L ← L + ds`, `ω = |dλ|/dt`.

`L` is the *accumulated* arc since contact began, not the net displacement, so a back-and-forth scrub charges the ring but at the cost of heat and time. Direction `s = sign(lowpass(dλ/dt, 80 ms))` determines the tangent sign at release. Reversing direction inside a stroke costs a 60 ms settle during which the launch ray is greyed out.

**Energy.** `q_L = clamp(L/120 mm, 0, 1)`, `q_ω = clamp(ω_peak/10 rad/s, 0, 1)`, `Q = 0.6·q_L + 0.4·q_ω`, `E = 30 + 70·Q` (E ∈ [30, 100]). `ω_peak` is the maximum 50 ms moving average during the stroke. A full-charge stroke is 120 mm of arc at ≥ 10 rad/s; at r = 37 mm, 120 mm is 3.24 rad = 185.8°, which is just over a front-hemisphere sweep. A 0.6 s sweep at 200 mm/s gives `L = 120 mm`, `ω = 5.4 rad/s`, `Q = 0.816`, `E = 87`.

**Bolt speed.** `v_bolt = 180 + 5.2·E` mm/s (180–700 mm/s). Flight time to distance `D` is `t_f = D/v_bolt`; at 400 mm and E = 60, `t_f = 0.81 s`.

**Launch bearing.** At lift, `λ_R` is the contact longitude and `s` the trace sign. The tangent is `t̂ = s·(−sinλ_R, cosλ_R, 0)`; launch azimuth `α_b = atan2(t̂_y, t̂_x) = λ_R + s·90°`. The bolt ray starts at the ring centre `(0,0,0)` for the hit test; the visual bolt is drawn from the surface point `1.06r·(cosλ_R, sinλ_R, 0)` along `t̂`, which is the same line to within 0.8° at 400 mm.

**Heat.** `dH/dt = 4 + 22·(ω/10 rad/s)²` while tracing; cooling `−9%/s` when no contact. At `ω = 10 rad/s`, heat rises 26%/s, so a continuous maximum-speed trace overheats from 0 in 3.8 s. A typical 0.7 s stroke adds 18 heat; the ring can fire 5–6 charged shots before a vent. Overheat at `H = 100`: `RING_JAM` for 2.0 s, charge dumped, no trace accepted, 8 heat/s residual; after the jam `H = 45`.

**Grip slip.** From the tracked prop, `ω_P` and `v_P` are compared with the cradle hand's palm frame. If `|ω_P| > 45 °/s` or `|v_P| > 60 mm/s` for 100 ms, charge leaks at `0.40·L/s` and `Q` is clamped to the leaked value; the launch ray flashes red. The fix is to squeeze the cradle, which raises the palm contact count and stiffens the hold.

Signal chain: joints at 72 Hz → One Euro (mincutoff 1.4 Hz, beta 0.02, derivative cutoff 7 Hz) → resample to 90 Hz → latitude/longitude projection → unwrap → arc/speed → 50 ms moving average for `ω_peak` → energy → heat → release classifier. All thresholds debounce 30 ms; the classifier emits at most one fire or soft-release event per 90 Hz tick, and a lift is only evaluated on the first tick where contact age exceeds the 8 mm release hysteresis.

## 7. Virtual World Layout & Coordinate Mapping

`V` origin is the ball centre; `ẑ_V` is up; `x̂_V` is the cradle's forward bearing frozen at round start. The ring is horizontal. Targets sit on a belt of radius 300–500 mm at elevation ±6°, so they appear as a halo around the ball at arm's length.

| Entity | V-space position | Notes |
|---|---|---|
| Ring centre | `(0,0,0)` | Ball centre |
| Ring | `R = 39.22 mm`, `z = 0` | 1.2 mm tube, 128 segments |
| Injector node | `R·(cosλ, sinλ, 0)` | Follows tracing contact |
| Target belt | `D ∈ [300, 500] mm`, `z ∈ [−31, +31] mm` | 8 targets per run |
| Target sector | half-width 5°–9°, elevation ±6° | Angular hit volume |
| Launch ray | from centre along `t̂` | 600 mm visual length |
| HUD ring | 240 mm from the eye | Charge, heat, hits |
| Cradle | tracked hand | Not rendered virtually |

Mapping formulas: `α_t(t) = α_t0 + ω_t·t`; at impact `α_t(t_f)` is compared with `α_b` using `wrap` to (−180°, +180°]. Hit if `|wrap(α_t(t_f) − α_b)| ≤ θ_half + 0.4°` and `E ≥ T`. The 0.4° aim assist compensates for the 72 Hz resampling of `λ_R`; it is fixed and not difficulty-scaled.

Scale: the target belt is 300–500 mm, the largest virtual reach in the set, but it is rendered as a halo around a hand-held ball, so the player never reaches for it. The 8 targets span the full 360° azimuth, and the reachable launch bearings cover all azimuths except within 12° of `λ_R = ±90°` relative to the front, where the tangent points at or away from the player; target azimuths avoid those two singular sectors.

## 8. Gameplay Systems & Rules

**Entities.** Eight targets per run, each with `(α, D, T, θ_half, ω_t)`. Ring charge `L`, energy `E`, heat `H`. One bolt in flight at a time. Run 120 s.

**Target table (level 3 baseline).**

| # | Azimuth α | Distance D (mm) | Toughness T | Half-width θ_half | Drift ω_t (°/s) |
|---|---|---|---|---|---|
| 1 | 20° | 300 | 35 | 9.0° | 2.0 |
| 2 | 60° | 350 | 45 | 8.0° | 2.0 |
| 3 | 100° | 400 | 55 | 7.5° | 3.0 |
| 4 | 140° | 450 | 65 | 7.0° | 3.0 |
| 5 | 200° | 500 | 75 | 6.5° | 4.0 |
| 6 | 240° | 460 | 85 | 6.0° | 4.0 |
| 7 | 280° | 380 | 90 | 5.5° | 5.0 |
| 8 | 320° | 320 | 95 | 5.0° | 5.0 |

Targets orbit slowly in azimuth; `ω_t` is signed and constant per target. They do not change distance or toughness. A hit removes the target with an explosion; a miss leaves it alive and adds heat.

**Fire gate.** A release fires only if `L ≥ 25 mm` and `ω_peak ≥ 2.5 rad/s`. Below either threshold it is a soft release: no bolt, no heat, charge resets, and the ring stays armed. This lets the player re-grip without wasting a shot.

**Hit test.** At `t_f = D/v_bolt`, compute the target's azimuth `α_t(t_f)`, then the angular error `e = |wrap(α_t(t_f) − α_b)|`. Hit if `e ≤ θ_half + 0.4°` and `E ≥ T`. Graze if `e ≤ θ_half + 1.5°` and `E ≥ T`: half score, target survives. Miss otherwise. A toughness shortfall is a "bounce": the bolt shatters on the shell, target survives, no score, heat +3.

**Score.**

`hit_score = round(E·(0.6 + T/100)) + round(40·(1 − e/(θ_half + 0.4°))) + 30·(e < 1° ? 1 : 0)`

`graze_score = round(0.5·hit_score)`

`chain_mult = 1 + 0.15·min(chain, 6)`; chain increments on hit or graze, resets on miss. Final target score is `round(score·chain_mult)`.

`run_bonus = 200·hits + 100·(H < 50 ? 1 : 0)`.

**Overheat and jams.** Each overheat counts toward the run's 4-jam limit. A jam dumps charge, locks tracing for 2.0 s, and sets `H = 45`. Four jams in a run ends the run early with the current hit count.

**Win/lose.** Three runs, 24 targets. A run ends at 8 hits, 120 s, or 4 jams. Win with ≥ 18 total hits; otherwise lose. A target not hit by the run clock is simply unscored; there is no escape penalty beyond the lost opportunity.

**Worked shot.** Player traces 110 mm at `ω_peak = 8.0 rad/s`: `q_L = 0.917`, `q_ω = 0.80`, `Q = 0.87`, `E = 90.9`. Releases at `λ_R = 30°` with positive trace → `α_b = 120°`. Target 3 at α = 100°, drift +3°/s, D = 400 mm; `v = 653 mm/s`, `t_f = 0.61 s`, `α_t = 101.8°`, `e = 18.2°` → miss. The player should have released at `λ_R = 11.8°` to put the tangent on the predicted bearing. This is the core skill: lead the drift and pick the longitude.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load ring, start tracker | PRESENT |
| PRESENT | prop registered | RMS < 0.4 mm | Freeze `x̂_V`, show ring ghost | CALIBRATE |
| CALIBRATE | cradle ≥ 3 contacts | — | Confirm hold, zero heat | RUN_INTRO |
| RUN_INTRO | 2.5 s timer | run ≤ 3 | Spawn 8 targets, ring armed | TRACE |
| TRACE | contact on band | cradle valid | Accumulate `L`, `ω`, heat | TRACE |
| TRACE | `L ≥ 25 mm` and `ω_peak ≥ 2.5` | — | Launch ray goes live | ARMED |
| ARMED | index lift | — | Fire bolt, lock `E` and `α_b` | FLIGHT |
| ARMED | index lift | `L < 25` or `ω_peak < 2.5` | Soft release, no shot | TRACE |
| FLIGHT | `t ≥ t_f` | hit and `E ≥ T` | Explode target, score, chain++ | TRACE |
| FLIGHT | `t ≥ t_f` | graze | Half score, target survives | TRACE |
| FLIGHT | `t ≥ t_f` | miss or bounce | Heat +3, chain = 0 | TRACE |
| TRACE | `H ≥ 100` | — | `RING_JAM`, dump charge | JAM |
| JAM | 2.0 s timer | jams < 4 | `H = 45`, resume | TRACE |
| JAM | jams = 4 | — | Ring scorches, run ends | RUN_END |
| TRACE | 8 hits | — | Bonus, tally | RUN_END |
| TRACE | run clock = 0 | — | Tally current hits | RUN_END |
| RUN_END | 3.0 s timer | run < 3 | Next run, 20 s rest | REST |
| RUN_END | run = 3 | — | Results | RESULTS |
| ANY | cradle < 2 contacts | — | Disarm, bolt in flight continues | DROPPED |
| DROPPED | re-cradled < 4 s | — | Re-arm, 0.5 s grace | TRACE |

The run clock is wall-clock. A jam or a drop never pauses it; only a tracker loss > 2 s pauses.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 ball | Sphere r = 37 mm | Tracked | Real prop; depth proxy |
| C1 ring | Torus | Static in `V` | Visual only |
| C2 fingertip | Capsule r = 1.8 mm | Tracked | Trace contact |
| C3 target | Sphere r = 6–14 mm | Azimuth drift | Visual body |
| C4 sector | Angular wedge | Follows C3 | Hit volume |
| C5 vent | Disc r = 8 mm | Static at north pole | Coolant purge |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C2 × C0 | signed distance < 5 mm and φ within ±12° | Trace contact |
| C2 × C0 | contact outside band | Ignored (cradle or side) |
| Bolt × C4 | angular sector at `t_f`, `E ≥ T` | Hit / graze / bounce |
| Bolt × C0 | bolt starts at centre | No self-hit |
| C5 × C2 | double-tap within 8 mm, 320 ms | Vent |
| C3 × C3 | ignored | Targets pass |

**Materials.** Ball = leather: µ 0.65, restitution 0.55; ring = "magnetic guide": frictionless, no collision response; bolt = "plasma": no mass, no gravity, constant speed; target = "shell": restitution 0.1 on a bounce.

**CCD.** The fingertip can move 800 mm/s (8.9 mm/tick), so the 5 mm contact shell is swept: the system samples the minimum signed distance along the segment and uses that point for `λ`. The bolt is a ray tested once at `t_f`; target drift is integrated analytically (`α_t0 + ω_t·t_f`), so no sub-stepping is needed. A bolt cannot hit two targets: the first angular match in target index order claims it.

**Force model.** The trace finger must overcome leather friction to slide: `F = µ·N`, with a comfortable normal load `N ≈ 0.6 N` and µ = 0.65, so `F ≈ 0.39 N`. The cradle must resist the reaction torque `τ = r·F = 0.0144 N·m`; a three-finger cradle with a 12 mm effective lever supplies `0.6 N × 0.012 m = 0.0072 N·m` per contact, and three contacts give 0.0216 N·m, enough with a 1.5× margin. This is why a two-finger cradle slips: the margin drops below 1.0 and the ring leaks charge.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → scene mesh → far target belt → ring back half → ball → ring front half → near targets → tracing hand → bolt → HUD. The ring is split at the `z = 0` plane into two draw calls so the ball can occlude its far half.

**Occlusion cases.**

- **O1 — Ring behind the ball.** The rear 180° of the torus is depth-tested against the 37 mm ball proxy and hidden. The front half is drawn 0.5 mm above the leather so it never z-fights.
- **O2 — Tracing finger over the ring.** The fingertip is drawn after the ring and occludes it; the injector node is drawn at 1.4 mm above the surface so the glow remains visible around the pad.
- **O3 — Target behind the ball.** A target whose bearing is within the ball's 37 mm silhouette is depth-tested and hidden. Its sector indicator is still drawn on the HUD compass so the player can aim without seeing the body.
- **O4 — Cradle fingers over the equator.** Cradle contacts are below φ = −35°, but a wrapped thumb can cross the band; the hand mesh occludes the ring and the trace band, and the band's guide line fades under the thumb.
- **O5 — Bolt passing behind the hand.** The bolt is depth-tested against both hand meshes; the segment behind the cradle hand disappears, and a small impact flash is drawn at the exit point.
- **O6 — HUD versus ring.** The charge arc and heat bar are at 240 mm and depth-tested against the hands; a raised palm covers them and they fade to 30%.

**VFX budgets.** Ring 128 segments, 1 draw call, 1.1 ms; bolt trail ≤ 24 quads; target explosions ≤ 2 concurrent, 120 particles; launch ray 1 quad; heat shimmer 0.3 ms. The ring uses an emissive tube with an additive core and no refraction, so the baseball's seam stays readable through it.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Cradle locked | ball | Soft click + 220 Hz hum | Ring ignites, level ring turns white |
| Trace start | contact point | Scratchy friction, brightness ∝ speed | Injector node brightens |
| Charge rise | ring | Hum 120→680 Hz, pitch ∝ Q | Ring fills clockwise, particles multiply |
| Aim steady | ring | 880 Hz pure tone, gain ∝ stability | Launch ray solidifies |
| Fire | ring node | Whoosh + 1.1 kHz launch chirp | Bolt leaves along the tangent |
| Hit | target | 90 Hz boom + 2.6 kHz shatter | Target bursts, ring flashes |
| Graze | target | Half-volume boom, metallic scrape | Target sparks, stays alive |
| Bounce | target | Dull 300 Hz thud | Bolt shatters on the shell |
| Miss | target bearing | Low 180 Hz fizzle | Bolt fades, heat bar jumps |
| Heat warning | HUD | 740 Hz double beep at 2.5 Hz | Heat bar pulses amber |
| Overheat | ring | Steam burst + 100 Hz alarm | Ring goes dark, HUD red |
| Vent | north pole | 5 kHz hiss, 400 ms | Steam plume, heat drops |
| Grip slip | cradle | Rattling 60 Hz buzz | Charge bar leaks, ray flashes red |
| Run clear | arena | Ascending 5-note arpeggio | Targets orbit out, score tally |

Every event maps to one row. The ring hum is the primary charge readout; the aim tone is the primary bearing readout, rising in gain as the launch ray stabilizes.

## 13. Difficulty Progression & Tuning

| Level | Targets | Distance range (mm) | Toughness range | Half-width range | Drift (°/s) | Heat gain | Run clock (s) |
|---|---|---|---|---|---|---|---|
| 1 | 8 | 300–380 | 30–60 | 9.0°–12.0° | 0–1.0 | 0.80× | 120 |
| 2 | 8 | 300–420 | 30–75 | 8.0°–11.0° | 1.0–2.0 | 0.90× | 120 |
| 3 | 8 | 300–500 | 35–95 | 5.0°–9.0° | 2.0–5.0 | 1.00× | 120 |
| 4 | 8 | 320–500 | 45–100 | 4.5°–8.0° | 3.0–7.0 | 1.10× | 110 |
| 5 | 8 | 340–500 | 55–100 | 4.0°–7.0° | 4.0–9.0 | 1.20× | 100 |

Named constants: `R_RING = 1.06r`, `L_REF = 120 mm`, `OMEGA_REF = 10 rad/s`, `E_MIN = 30`, `E_MAX = 100`, `V_MIN = 336 mm/s` (at `E = 30`), `V_MAX = 700 mm/s`, `FIRE_L = 25 mm`, `FIRE_W = 2.5 rad/s`, `HEAT_MAX = 100`, `JAM_TIME = 2.0 s`, `JAM_LIMIT = 4`, `VENT_COOL = 25`, `VENT_CD = 15 s`, `HITS_TO_WIN = 18`.

Adaptive rule: after each run, if the hit rate is > 75%, increase target drift by 1.0°/s and shrink half-widths by 0.5° (floors 4.0° and 4.0°/s); if < 40%, reduce drift by 1.0°/s and widen half-widths by 0.5° (ceilings 9.0° and 9.0°/s). Energy and heat coefficients are never adapted; the physical trace stays the same.

## 14. Edge Cases & Failure Recovery

- **Cradle slips during a trace.** `|ω_P| > 45 °/s` for 100 ms → charge leaks 40%/s and the heat rate is unchanged. If the slip persists > 0.5 s, the shot is disarmed and the player must re-cradle. No bolt is fired.
- **Player releases at a singularity.** If `α_b` is within 12° of the direction toward or away from the player, the tangent is nearly radial and the bolt would fly at the player's chest. The launch is blocked with a "no bearing" cue, the charge is retained for 300 ms, and the player can re-contact to fire.
- **Tracing finger leaves the band.** A contact at `|φ| > 12°` is ignored, but `L` is retained for 250 ms; returning to the band resumes the stroke. This tolerates a 3 mm vertical wobble at r = 37 mm.
- **Hand tracker loses the index behind the ball.** The last valid `λ` is held for 120 ms; if contact is not re-established, a soft release fires if the gate is met, otherwise the charge resets. The game never invents a lift.
- **Two targets overlap in azimuth.** The nearer target (smaller D) is tested first; if the bolt passes it, the farther target can still be hit only if the angular error is inside its sector. A single bolt can only score once.
- **Overheat during flight.** The bolt continues and can still hit; the jam only blocks new tracing. This prevents a hit from being stolen by a heat event in the same tick.
- **Ball knocked from the cradle.** `DROPPED` disarms the ring, but the in-flight bolt resolves normally. Re-cradling within 4 s restores `L` and `H`; after that, the run continues with zero charge.
- **Player uses the cradle hand to trace.** The cradle hand's index contacts are ignored and the ring disarms for 200 ms with a "one hand holds" cue. The other hand must be the tracer.
- **Baseball seam is wet or glossy.** Feature tracking degrades; after 1.5 s of poor seam visibility, `q_P` freezes and the slip detector uses hand-pose deltas with a tightened 30 °/s threshold.
- **Run clock expires mid-flight.** The bolt resolves if it launched before the run clock reached zero; the run-end check runs after impact resolution. A last-moment hit counts.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — sphere pose, seam confidence, re-acquisition.
- `CradleMonitor` — cradle contact count, palm frame, prop slip detection.
- `EquatorTracer` — latitude/longitude projection, unwrap, `L`, `ω`, direction, band gating.
- `RingCharge` — `Q`, `E`, heat integration, vent, jam.
- `LaunchSolver` — tangent bearing, fire gate, bolt spawn.
- `BoltFlight` — constant-speed ray, impact time, target sector test.
- `TargetDirector` — 8 targets, azimuth drift, per-run table.
- `RunScoring` — hit/graze/bounce, chain, bonuses.
- `RunStateMachine` — §9 table, per-level run clock.
- `FeedbackRouter` — event → audio + VFX, coverage assertion.

**Event flow.** `PropTracker` → `CradleMonitor`; `EquatorTracer` → `RingCharge`; `RingCharge` → `LaunchSolver`; `LaunchSolver` → `BoltFlight`; `TargetDirector` → `BoltFlight` → `RunScoring`; `RunScoring` → `RunStateMachine` and `FeedbackRouter`; `CradleMonitor` → `RingCharge` (slip leak).

**Pseudocode 1 — trace, charge, and fire.**

```
function tick(dt):
    pose = PropTracker.latest()
    cradle = CradleMonitor.state()          # HELD / SLIP / DROPPED
    if cradle == DROPPED: disarm(); return

    c = EquatorTracer.contact()
    if c.valid and cradle == HELD:
        dlam = wrap(c.lambda - last_lambda)
        ds = r * abs(dlam) * cos(c.phi)
        L += ds
        omega = abs(dlam) / dt
        omega_peak = max(omega_peak, moving_avg(omega, 50ms))
        s = sign(lowpass(dlam/dt, 80ms))
        Heat.add((4 + 22*pow(omega/10, 2)) * dt)

    if cradle == SLIP:
        L *= exp(-0.40 * dt)                # 40%/s leak
        Feedback.play(GRIP_SLIP)

    if c.lifted:
        if L >= 25mm and omega_peak >= 2.5rad_s and not near_singularity():
            t_hat = s * vec(-sin(lambda_R), cos(lambda_R), 0)
            alpha_b = atan2(t_hat.y, t_hat.x)
            E = 30 + 70 * (0.6*clamp(L/120mm) + 0.4*clamp(omega_peak/10))
            BoltFlight.launch(alpha_b, E, D_of_target)
            Heat.add(2)
        L = 0; omega_peak = 0
```

**Pseudocode 2 — impact and target resolution.**

```
function launch(alpha_b, E, D):
    v = 180 + 5.2 * E
    t_f = D / v
    state = FLIGHT

function resolve_flight(t):
    if t < t_f: return
    target = TargetDirector.next_by_distance()
    alpha_pred = target.alpha0 + target.omega_t * t_f
    e = abs(wrap(alpha_pred - alpha_b))
    if E < target.T:
        Feedback.play(BOUNCE, target.pos); chain = 0; Heat.add(3)
    elif e <= target.half_width + 0.4deg:
        RunScoring.hit(target, e, E); TargetDirector.remove(target)
    elif e <= target.half_width + 1.5deg:
        RunScoring.graze(target, e, E)
    else:
        Feedback.play(MISS, target.bearing); chain = 0; Heat.add(3)
    state = TRACE
```

**Determinism.** Target drift is analytic; `L` and heat are integrated at 90 Hz; target selection is index-ordered. A recorded hand trace plus a fixed target table reproduces every hit and score bit-for-bit.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Trace speed | Index sweeps 100 mm/s at r = 37 mm, φ = 0 | `ω = 2.70 rad/s ± 2%`; `L` after 0.5 s = 50 mm ± 1 |
| AT-02 | Arc accumulation | Forward 40 mm, reverse 20 mm | `L = 60 mm`; direction sign flips after 60 ms settle |
| AT-03 | Energy mapping | `L = 120 mm`, `ω_peak = 10 rad/s`; then `L = 60`, `ω = 5` | E = 100 ± 0.5; E = 65 ± 0.5 |
| AT-04 | Fire gate | Release at `L = 24.9, ω = 3`; then `L = 25.1, ω = 3`; then `L = 40, ω = 2.4` | No shot; shot; no shot |
| AT-05 | Tangent bearing | `λ_R = 0°`, positive trace; then `λ_R = 30°`, negative | `α_b = 90°`; `α_b = −60°` (300°), ±0.2° |
| AT-06 | Sector boundary | Target `θ_half = 7°`, `e = 7.3°`; then `e = 7.5°` | First is a hit (7 + 0.4); second is a graze |
| AT-07 | Toughness | `E = 54` vs `T = 55`; then `E = 56` | Bounce (no damage); hit |
| AT-08 | Drift lead | Target at 400 mm, drift 3°/s, E = 60 (`v = 492 mm/s`, `t_f = 0.81 s`); aim at `α_t0` | Miss by ≈ 2.4°; aim at predicted `α_t(t_f)` hits |
| AT-09 | Heat rate | Hold `ω = 10 rad/s` from H = 0 | H ≈ 26 after 1.0 s ± 2; jam at 3.8 s ± 0.2 |
| AT-10 | Jam limit | Force 4 overheats in a run | Run ends on the 4th; `H = 45` after each jam; no trace during 2.0 s |
| AT-11 | Grip slip | Spin the prop at 50 °/s during a 100 mm charge | Charge leaks to ≈ 67 mm after 1.0 s (40%/s decay); ray flashes red; no fire |
| AT-12 | Singularity block | `α_b` within 12° of the player bearing | No launch; charge retained 300 ms; no heat penalty |
| AT-13 | Ring occlusion | Read depth at the rear ring segment behind the ball | Rear contribution = 0 inside the 37 mm disc; front visible |
| AT-14 | Determinism and audio | Replay a 120 s trace twice; enumerate events | Identical hit/score hash; 14/14 events mapped with clip and VFX |

Tests run headless with synthetic hand skeletons, a fixed target table, and a deterministic 90 Hz clock. AT-13 reads the depth buffer; AT-08 asserts the sign and magnitude of the lead error, not just a hit.
