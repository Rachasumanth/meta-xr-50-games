# Weeble Sumo

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Weeble Sumo |
| Slug | `weeble_sumo` |
| Physical prop | Oval river stone, broad face down; 85 × 70 × 34 mm nominal; 150 g; smooth basalt, matte finish, no markings |
| Registered contact spheroid | Oblate; `a = b = 37.5 mm`, `c = 17.0 mm` (75 × 75 × 34 mm fitted spheroid); the stone's 5 mm end lobes lie outside the collider |
| Family / sub-shape | 4.2 Prolate and Oblate Spheroids / oblate, palm-scale, rocking on a broad pole |
| Scale class | Physical stone 1:1; virtual ring 260 mm diameter; tabletop scale |
| Source concept quote | "A round-bellied sumo spirit in a circular ring; press-release makes it lunge and rock upright with heavy momentum, belly-bumping shadow imps out." |
| Physical input | Right index or thumb presses the top pole of the upright stone and releases; left hand braces the tray |
| Session length | 6 min: onboarding 40 s + 5 bouts × 50 s + 4 intermissions × 8 s + results 30 s |
| Lose condition | Stability meter reaches 0, or the stone topples past 40° three times in one bout |

## 1. Fantasy & Core Loop

The stone is a sumo spirit named Dobu. Its belly is a boulder, its topknot is a curl of moss, and its feet are the single broad face it stands on. Five shadow imps circle the ring, chittering, waiting for the spirit to commit its weight. Dobu cannot step. It can only *rock*: press the topknot, let the whole mass tip, and release the stored lean into a belly-first lunge that sends the imps tumbling over the ring's chalk line. Then gravity and the stone's own roundness pull the spirit upright again with a slow, heavy wobble that the player must ride rather than fight.

1. **READ (0.6 s).** Imps spawn at the ring rim. Their shadow trails point inward; the nearest three are highlighted with a 4 mm ground ring.
2. **AIM (0.3–1.2 s).** The player presses the top pole at a chosen latitude and longitude. The press point's longitude selects the lunge direction; the polar angle selects how much tilt authority the press can deliver.
3. **CHARGE (0.08–0.25 s).** Holding the press compresses the finger pad and winds the spirit. Charge is the integral of press force over time, capped at 0.30 N·s.
4. **RELEASE (0.05 s).** Release converts charge into a horizontal impulse and an angular impulse about the contact patch. The spirit tips and slides in the opposite direction to the press point.
5. **LUNGE (0.35–0.60 s).** The belly sweeps through a 28 mm contact radius. Any imp inside that arc takes a collision impulse and is knocked outward.
6. **ROCK BACK (0.8–1.6 s).** The spheroid's curvature and low centre of mass restore the spirit. The player may chain a second press at the extreme of the rock for a 1.35× counter-lunge.

A bout is 6–14 imps and a 40 s clock. A perfect bout is four lunges that each ring out two imps, with the spirit never exceeding 22° of tilt. The skill is not timing alone: the press point's longitude is the aim, and the stone's current tilt direction biases where the next lunge can go.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Spheroid sub-shape**: the prop is an ellipsoid of revolution with equatorial semiaxes `a = b` in the `x̂_P`–`ŷ_P` plane and polar semiaxis `c` along `ẑ_P`; prolate if `c > a`, oblate if `c < a`. Surface: `p_P = (a·cosφ·cosλ, a·cosφ·sinλ, c·sinφ)`, `φ ∈ [−90°, +90°]`, `λ ∈ [0°, 360°)`; outward normal `n̂ ∝ (cosφ·cosλ/a, cosφ·sinλ/a, sinφ/c)` normalized. Equator in `x̂_P`–`ŷ_P`, poles at `±ẑ_P`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

`P` origin is the stone's centre; `−ẑ_P` is the broad bottom face (the contact pole), `+ẑ_P` is the top press pole, `+x̂_P` is the calibration lunge reference (initially facing the player), and `+ŷ_P` is to the player's left. Exact semiaxes are `a = b = 37.5 mm` and `c = 17.0 mm`; this is a 75 × 75 × 34 mm spheroid fitted to the real 85 × 70 × 34 mm stone, with the stone's end lobes outside the collider. Because `c < a`, the prop is oblate. The bottom pole's local radius of curvature is `R_p = a²/c = 82.72 mm`, which is greater than the 17.0 mm centre-of-mass height; the upright pose is therefore self-righting, which is why this game uses an oval stone rather than a prolate egg. The local rolling model is `ω = (n × v_t)/r_eff`, with `r_eff = |p_P|`; at the bottom pole `r_eff = 17.0 mm`, at the equator `37.5 mm`. `V` is anchored to `T` at the ring centre, with `x̂_V` aligned to the player's forward calibration and `ŷ_V` left. Four deviations:

1. **Press force is a pad-deformation proxy.** There is no force sensor; normal force is `F = k_f·δ`, where `δ` is the fingertip-pad penetration estimated from the tracked joint and the spheroid surface, and `k_f = 250 N/m`.
2. **Virtual amplification.** The virtual sumo's tilt is rendered at 1.6× the physical stone's tilt and its translation at 1.4× the tracked stone translation. The physical pose seeds the state; the game integrates its own inverted-pendulum state for consistency.
3. **Precomputed rocking table.** The exact spheroid support function gives `z_c(θ)` and `τ_g(θ) = −m g dz_c/dθ`. The local `ω_n = 11.76 rad/s` is used inside ±25°; outside that, the table is sampled so the stone never gains energy from a linearisation error.
4. **Re-seating is free.** If the physical stone tips onto its side or is lifted, the player may stand it up again without penalty; the virtual bout pauses for up to 3.0 s while the stone is re-seated.

## 3. Physical Play Space & Ergonomics
The stone stands on a 120 × 120 mm square of felt-backed tray, or directly on a table, 250–300 mm in front of the seated player's sternum. The left hand rests on the tray edge, palm down, as a brace and as a physical stop if the stone slides. The right hand approaches from above at 35–55° from vertical; the index or thumb pad presses the top pole, and the wrist stays within 20° of neutral. The press is a *fingertip* action, not a palm slam: the pad deforms 3–8 mm, which is the physical range that maps to the 0.10–0.30 N·s charge window.

| Parameter | Value | Rationale |
|---|---|---|
| Support | Table or 120 mm felt tray on `T` | Felt adds 0.06 friction and stops 40 mm slides |
| Stone centre | 270 mm forward, 30 mm above table | Comfortable downward press without shoulder flexion |
| Standing height | 34 mm (c = 17 mm above centre) | Top pole is reachable with a 35–55° finger approach |
| Press cap | φ ≥ +55°, any λ | Off-pole press is what creates the lunge direction |
| Max press force | 3.0 N | Above this the pad bottoms out and force is not measurable |
| Charge window | 0.10–0.30 N·s | Below 0.10 N·s the stone only wobbles; above 0.30 N·s it topples |
| Ring diameter | 260 mm | Fits the tabletop and gives imps 130 mm to travel |
| Session | 6 min; 8 s rests between bouts | Press-and-hold is a low-load but repetitive thumb task |
| Break cue | Open-hand overlay after bout 3 | Releases the thumb flexor before fatigue |

The right hand never grips the stone during a press; only the distal pad touches. A full-hand grip would add an uncontrolled horizontal force and make the lunge direction ambiguous. If the palm contacts the stone (more than the distal pad), the press is classified as a *brace* and produces no charge. The left brace hand is mandatory for the first bout; after that it is optional but recommended, because a 34 mm slide can walk the stone 40 mm across the tray in a vigorous bout.

## 4. Prop Geometry & Registration

| Property | Value | Notes |
|---|---|---|
| Nominal stone | 85 × 70 × 34 mm | Oval river stone, long axis horizontal at rest |
| Fitted spheroid | `a = b = 37.5 mm`, `c = 17.0 mm` | 75 × 75 × 34 mm; 10 mm length excess is outside the collider |
| Mass | 150 g | Basalt, measured |
| Transverse inertia | `I_C = m(a² + c²)/5 = 5.086 × 10⁻⁵ kg·m²` | Through the centre, about a horizontal diameter |
| Contact-pole curvature | `R_p = a²/c = 82.72 mm` | Greater than COM height, so upright is stable |
| Rocking frequency | `ω_n = sqrt(m g d / I_O) = 11.76 rad/s` | `d = R_p − c = 65.72 mm`; `f = 1.872 Hz` |
| Damping ratio | ζ = 0.055 | Basalt on felt; 63 % decay in 1.55 s |
| Contact ellipse | 4.0 × 4.0 mm | Hertzian patch at 1.5 N normal load |
| Friction | µ = 0.38 stone/table, 0.44 stone/felt | Felt tray is the recommended surface |

**Surface-feature atlas (P-fixed; λ from `+x̂_P` toward `+ŷ_P`, φ from equator):**

| Feature | φ | λ | Extent | Purpose |
|---|---|---|---|---|
| Bottom contact pole | −90° | any | 4 mm ellipse | Pivot; must stay on the support surface |
| Top press cap | +55° to +90° | any | 35° cap | Press input; longitude selects lunge aim |
| Sumo belt line | −10° | any | 3 mm band | Readable tumble reference |
| Belt knot | −10° | 180° | 12 × 8 mm | Back marker, helps orientation recovery |
| Left grip band | −20° to +20° | +90° ± 18° | 30 mm | Left-hand brace contact |
| Right grip band | −20° to +20° | −90° ± 18° | 30 mm | Right-hand stabilise contact |
| Front chalk mark | 0° | 0° | 6 × 2 mm | Lunge reference `+x̂_P` |
| Rear notch | 0° | 180° | 6 × 2 mm | Calibration yaw reference |
| Rim line | −55° | any | 2 mm band | Base-cap edge; visual topple warning |
| Topknot | +90° | any | 5 mm cap | Charge glow emitter |

The atlas cell size at the equator is `2πa/64 = 3.68 mm`; the press cap spans 35° of latitude and is sampled at 10° resolution by the classifier, giving at least three discrete aim sectors even for a small thumb pad.

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Spheroid | `a = b = 37.5`, `c = 17.0 mm` at `P` | Stone body and depth proxy |
| C1 | Disc | r = 4.0 mm, at `−ẑ_P` | Contact ellipse / pivot |
| C2 | Spherical cap | 35° half-angle about `+ẑ_P` | Press gate |
| C3 | Capsule | r = 6.0 mm, length 16 mm | Fingertip pad |
| C4 | Sphere | r = 6.0 mm | Shadow imp body |
| C5 | Sphere | r = 22.0 mm, centre at `z = +8 mm` | Sumo belly bump volume |
| C6 | Annulus | inner 128 mm, outer 132 mm, in `V` | Ring-out chalk line |
| C7 | Capsule | r = 18 mm, length 60 mm | Left brace palm |

Registration: the player presents the stone at 0.8–1.2 m, rotates it through at least 90° of yaw, and sets it down on the tray. The fitter uses the 85 mm silhouette and the 34 mm height; acceptance requires RMS < 0.5 mm, yaw error < 2.0°, and a stable bottom-pole normal within 3° of `−ẑ_T`. Re-acquisition searches a 120 mm cube for 1.0 s; success needs 6 rim samples with RMS < 0.9 mm over 6 frames. The calibration lunge reference `+x̂_P` is set to the direction from the stone centre to the player's sternum, so "press the near side" lunges away from the player on the first bout.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long or axis) | Semantic output |
|---|---|---|---|---|
| I5.1 | Press top | `R_index_tip` or `R_thumb_tip` | φ +55° to +90°, any λ | `AIM(λ)` and force proxy `F` |
| I5.2 | Charge hold | Same joint, contact > 80 ms | same | `CHARGE = ∫F dt`, capped 0.30 N·s |
| I5.3 | Release | Contact lost after charge > 0.10 N·s | — | `LUNGE` with impulse `J` |
| I5.4 | Direction trim | Index tip slides 2–6 mm along the cap | Δλ within ±20° | Aim adjustment at 4°/mm |
| I5.5 | Two-finger press | `R_index_tip` + `R_middle_tip`, separation 10–24 mm | same cap | Charge ×1.25, aim averaged |
| I5.6 | Left brace | `L_palm`, `L_heel` on tray edge | `T` plane, outside stone | Stability +8 %, no input |
| I5.7 | Re-seat | Both palms cup the stone for > 400 ms | any | Pause bout, reset tilt to < 3° |
| I5.8 | Cancel | Thumb tap on the base rim | φ −60° to −40° | Discard charge, no lunge |

**Per-finger roles.** The right index is the primary press effector; the right thumb is the alternate for players who prefer a side approach. The right middle finger may join for I5.5 but never presses alone. The right ring and pinky are ignored. The left palm is a brace and a stability reference; it must not touch the press cap, or the press is invalidated. A press that begins on the belt line (φ between −10° and +10°) is rejected because the normal is nearly horizontal and the resulting force direction is ambiguous.

Contact gating: a press contact is valid when signed distance < 5 mm, age > 30 ms, the contact normal has a positive `ẑ_T` component, and the contact point projects into the press cap. Release hysteresis is 8 mm; a lost contact within 30 ms of acquisition is treated as a graze and produces no charge.

## 6. Input Kinematics & Signal Processing
The fingertip pad is modelled as a spring: `F = min(k_f·δ, 3.0 N)`, with `k_f = 250 N/m`. Penetration `δ` is the signed distance from the tracked tip to the spheroid surface, clamped to `[0, 12] mm`; the 5 mm contact threshold means a valid press begins at `F = 1.25 N` and rises to 3.0 N at 12 mm. This is a proxy, not a measurement, so it is low-pass filtered at 8 Hz and rate-limited to 20 N/s to reject tracker jitter.

The press point `p` and surface normal `n` come from the contact bus. The press impulse is

`J = ∫ F dt`, capped at `J_max = 0.30 N·s`.

The horizontal component is `J_h = J·|n_h|`, where `n_h = (n_x, n_y, 0)`; the vertical component is `J_z = J·n_z`. The lunge direction is `d̂ = −normalize(n_h)`, i.e. *away* from the side that was pressed. The angular impulse about the contact point `C = (0, 0, −c)` is `J_θ = |(p − C) × (−J·n)|`, and the initial angular rate is `θ̇₀ = J_θ/I_O`.

At a press at φ = 60°, the normal is `n = (0.253·cosλ, 0.253·sinλ, 0.967)` and the moment arm gives `J_θ = 10.1 mm·J`. With `J = 0.24 N·s`, `θ̇₀ = 3.46 rad/s` and the free-rock amplitude is `θ_max = θ̇₀/ω_n = 0.294 rad = 16.8°`. The horizontal impulse is `J_h = 0.061 N·s`, giving the stone a 0.405 m/s slide; friction at 0.38·m·g = 0.559 N decelerates it at 3.73 m/s² and it travels 22 mm. A full 0.30 N·s press reaches 0.51 m/s and 34 mm of slide, which is the difference between a bump that grazes an imp and one that rings it out.

The rocking state is a damped inverted pendulum driven by gravity about the instantaneous contact:

`θ̈ = [τ_g(θ) + τ_ext]/I_O − 2ζω_n θ̇`,

with `τ_g(θ) = −m·g·dz_c/dθ`, `ζ = 0.055`, and `I_O = I_C + m·d² = 6.987 × 10⁻⁴ kg·m²`. The centre-of-mass height is `z_c(θ) = R_p − d·cosθ` for the local model, and `d = R_p − c = 65.72 mm`. The small-angle natural frequency is

`ω_n = sqrt(m·g·d/I_O) = 11.76 rad/s`, `f = 1.872 Hz`, `T = 0.534 s`.

This period is the core tuning number: a lunge that peaks at 0.13 s and returns to upright at 0.40 s leaves 0.13 s of overlap with the next imp wave. The player can chain a counter-lunge at the first return peak, where the tilt rate is +3.4 rad/s and the charge gain is 1.35×.

**Signal chain.** 72 Hz joints → One Euro filter (mincutoff 1.2 Hz, beta 0.025, derivative cutoff 8 Hz) → 90 Hz interpolation → spheroid signed distance → `δ` → force filter (8 Hz Butterworth) → charge integrator. The charge integrator is reset on release, on I5.7, and on any contact gap > 120 ms. The tilt state is fused from the tracked `q_P` and the model: `θ_fused = 0.65·θ_model + 0.35·θ_tracked`, because the model is smoother but the tracked pose is the ground truth for the physical stone.

**Classifiers.** *Press* = contact in the cap for ≥ 30 ms; *charge* = press held 80–250 ms; *release* = contact lost with charge ≥ 0.10 N·s; *trim* = tangential motion 2–6 mm during charge, aim shifted 4°/mm; *two-finger* = index and middle contacts separated by 10–24 mm for ≥ 40 ms; *re-seat* = both palms within 60 mm of the stone for ≥ 400 ms. All classifiers are debounced 30 ms and emit at most one event per tick.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to `T` at the ring centre. `x̂_V` is the calibration direction from the stone to the player's sternum projected onto `T`, `ŷ_V` is left, `ẑ_V` is the table normal. The ring is a 260 mm diameter chalk annulus with a 4 mm line and a 2 mm raised lip; the lip is visual only. The sumo stands at `V` origin with its bottom pole at `z = 0` and its body radius 22 mm. The camera is the head pose; the ring is drawn 2 mm above the felt so it does not z-fight with the tray.

| Entity | V-space position | Size | Notes |
|---|---|---|---|
| Sumo body | `p_V`, `z = 17 mm` | r = 37.5 mm spheroid, belly r = 22 mm | Rocking state `θ, λ` |
| Belly collider | `p_V + 8 mm·ẑ_V` | r = 22 mm | Bump volume |
| Ring line | centre `V` origin | r = 130 mm, 4 mm wide | Ring-out boundary |
| Imp spawn | ring at 122 mm | r = 6 mm | 8–14 per bout |
| Imp path | toward sumo | 25–55 mm/s | Shadow trail 20 mm |
| Chalk dust | ring plane | 600 particles | Cosmetic |
| Stability meter | 40 mm above ring, player-facing | 3 pips | HUD, dry |
| Aim fan | top pole | 35° wedge | Shows λ and charge |

Mapping: the physical stone's translation is amplified 1.4× and clamped to a 90 mm radius around the ring centre; if the real stone slides farther, the virtual sumo stops at the clamp and a 2 mm/s drift pulls it back. The tilt is amplified 1.6× for the mesh but the physics uses the physical-equivalent angle. The aim longitude maps directly: pressing at λ presses the near side, and the lunge goes to λ + 180°. The 35° press cap spans 7 aim sectors of 5°, but the classifier reports a continuous λ with ±4° resolution, so the player can thread between two imps.

## 8. Gameplay Systems & Rules
**Entities.** Sumo: tilt `θ ∈ [−40°, +40°]`, azimuth `λ`, stability 100. Shadow imps: mass 20 g, radius 6 mm, HP 1, speed 25–55 mm/s, spawn at 122 mm and walk inward. Ring radius 130 mm. Each bout has a quota of 6–14 imps and a 40 s clock.

**Lunge.** On release, the game applies `J_h` along `d̂` and `J_θ` about the contact. The virtual sumo integrates the rocking ODE and a planar slide with friction `µ = 0.38`. The belly bump volume is active while `θ ≥ 8°`; the bump check runs every tick against each imp. A bump requires horizontal distance < 22 + 6 = 28 mm. The imp impulse is

`J_imp = (1 + e)·m_eff·v_rel`, with `e = 0.35`, `m_eff = m_s·m_i/(m_s + m_i) = 0.01765 kg`,

and `v_rel` is the closing speed of the belly surface. The belly surface speed is `v_belly = v_slide + h_belly·θ̇`, with `h_belly = 8 mm`; at a 0.4 m/s slide and a 3.4 rad/s rock, `v_belly ≈ 0.43 m/s` and `J_imp = 0.0103 N·s`, giving the 20 g imp a 0.515 m/s outward speed. An imp at 100 mm from centre crosses the 130 mm line in 58 ms and then slides on the felt until friction stops it.

**Ring-out.** An imp is out when its centre crosses 130 mm. It plays a 300 ms tumble animation and is removed. Ring-outs increment the bout quota and the combo. If the imp is pushed inward, it can pass through the sumo's centre and must be bumped again from the other side; this is intentional, because a careless lunge can save an imp.

**Imp push.** An imp that reaches within 18 mm of the sumo centre and is not bumped applies a 0.0012 N·m·s angular impulse in the direction from the imp to the sumo, plus a 0.0004 N·s horizontal push. This tilts the spirit by up to 4° and drains 8 stability. Three imp pushes in a row can set up a topple, so the player must clear the inner ring, not just the rim.

**Stability.** Stability starts at 100 and regenerates 4/s while `|θ| < 5°`. An imp push costs 8; a topple costs 34; a ring-out restores 3. At 0 the bout is lost. The stability meter is also a tilt warning: it pulses amber at `|θ| > 22°`, which is the scoring envelope, and red at `|θ| > 32°`, which is the physical topple warning.

**Topple.** If `|θ| > 40°` or the contact point leaves the 55° base cap, the stone topples. The bout pauses for 1.2 s, the spirit plays a comic flop, stability −34, and the stone is re-seated. Three topples in one bout end the bout.

**Combo.** Consecutive ring-outs within 2.5 s build a combo. `multiplier = 1 + 0.25·min(combo, 4)`, max 2.0. A missed lunge (no bump) resets the combo; an imp push resets it.

**Score.**

`bump_score = 100 · multiplier · (1 + 0.5·min(θ_bump, 22°)/22°)`

`ringout_score = 150 · multiplier`

`bout_score = 400 + 4·stability_remaining + 10·time_remaining`

`run_score = Σ bout_score + Σ ringout_score + Σ bump_score`

The tilt term rewards a committed lunge: a bump at 22° is worth 1.5× a bump at 0°. The formula reads `θ_bump` at the moment of contact, before the imp impulse is applied.

**Win/lose.** Clear five bouts to win. A bout ends when the quota is reached or the 40 s clock expires; expiring with stability > 0 still counts as a clear, but awards no time bonus. Stability 0 or three topples loses the bout; losing two bouts ends the run. The run score is the sum of cleared bouts only.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load ring, start tracker | PRESENT |
| PRESENT | stone registered | RMS < 0.5 mm | Show stand-up ghost | CALIBRATE |
| CALIBRATE | stone still 1.0 s on `T` | bottom normal within 3° | Set `+x̂_P` toward sternum | BOUT_INTRO |
| BOUT_INTRO | timer 2.0 s | bout ≤ 5 | Show imp quota and ring | SPAWN |
| SPAWN | timer 0.8 s | imps remaining > 0 | Spawn imp at 122 mm, random λ | READ |
| READ | tick | no active press | Update imp paths, aim fan | AIM |
| AIM | press in cap | `F > 1.25 N` | Start charge, show λ wedge | CHARGE |
| CHARGE | tick | contact held | Integrate `J`, trim aim | CHARGE |
| CHARGE | release | `J ≥ 0.10 N·s` | Apply `J_h`, `J_θ`, play lunge | LUNGE |
| CHARGE | release or gap > 120 ms | — | Discard charge, 6° wobble | READ |
| LUNGE | tick | `|θ| < 40°` | Integrate rock, test belly bumps | LUNGE |
| LUNGE | peak return | `θ̇` changes sign | Allow counter-lunge window 0.20 s | READ |
| LUNGE | `|θ| ≥ 40°` | — | Topple, stability −34, re-seat | RESEAT |
| READ/LUNGE | imp reaches 18 mm | not bumped | Imp push, stability −8 | READ |
| READ/LUNGE | imp > 130 mm | — | Ring-out, combo++, quota−− | READ |
| BOUT_END | quota = 0 or clock = 0 | stability > 0 | Tally, restore 15 stability | INTERMISSION |
| BOUT_END | stability = 0 or topples = 3 | — | Bout lost, run ends at 2 losses | GAME_OVER |
| INTERMISSION | timer 8 s | bout < 5 | Show next quota | BOUT_INTRO |
| INTERMISSION | timer 0 s | bout = 5 | Results | RESULTS |
| GAME_OVER | tap "retry" | — | Reset stability, keep bout 1 | BOUT_INTRO |
| ANY | tracker loss > 1.5 s | — | Pause, zero charge | RECOVERY |
| RECOVERY | re-acquired | RMS < 0.9 mm | Re-seat check, 0.5 s grace | previous |

Timers run on the 90 Hz fixed tick. The bout clock pauses during `RESEAT` but not during `LUNGE`; a topple therefore costs stability and wall-clock time, which keeps the 40 s bout honest. The charge integrator is advanced at 90 Hz and clamped to `J_max`; a frame stall cannot produce an over-charge because the integrator is clamped and the press force is rate-limited.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 stone | Spheroid 37.5/37.5/17 mm | Tracked `P` + virtual rock | Depth proxy; virtual state is authoritative for gameplay |
| C1 contact | Disc r = 4 mm | Fixed at `−ẑ_P` | Pivot; normal load 1.5 N |
| C2 press cap | Cap 35° | Fixed at `+ẑ_P` | Input only |
| C4 imp | Sphere r = 6 mm | Kinematic path | 25–55 mm/s inward |
| C5 belly | Sphere r = 22 mm at `z = +8 mm` | Follows rock | Bump volume |
| C6 ring | Annulus 128–132 mm | Static | Ring-out test |
| C3 finger | Capsule r = 6 mm | Tracked | Press contact |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C5 belly × C4 imp | distance < 28 mm and `θ ≥ 8°` | Impulse `J_imp`, imp knocked along lunge, bump score |
| C4 imp × C6 ring | imp centre > 130 mm | Ring-out, imp removed, +150 |
| C4 imp × C0 stone | distance < 18 mm | Imp push, 0.0012 N·m·s tilt impulse, stability −8 |
| C3 finger × C2 cap | signed distance < 5 mm | Press, charge |
| C3 finger × C0 | contact outside cap | Brace, no charge |
| C0 × `T` | bottom pole on plane | Contact ellipse, rocking constraint |
| C4 × C4 | ignored | Imps pass through each other |
| C4 × C6 inner lip | imp centre < 128 mm and outward | No effect; lip is visual |

**Materials.** Stone = "basalt": restitution 0.12 against felt, friction 0.38. Imp = "shadow": restitution 0.35 against belly, friction 0.45 against felt; it deforms 2 mm on impact and loses 35 % of its speed to the felt over 150 ms. Ring = "chalk": no collision, only a boundary test. Felt tray = "compliant": 1.0 mm sink, adds 0.06 friction.

**CCD.** The imp moves at most 0.61 mm/tick at 55 mm/s, far below the 6 mm radius, so discrete sphere tests are sufficient. The belly sweeps at up to 0.8 m/s, or 8.9 mm/tick, which is smaller than the 28 mm bump radius, so the bump test is discrete with a 2 mm skin. The stone's own translation is clamped to 0.6 m/s and uses a swept circle against the ring lip if the player pushes it off-centre.

**Forces and integration.** The rocking ODE is integrated semi-implicitly at 90 Hz:

`θ̇ ← θ̇ + dt·([τ_g(θ) + τ_ext]/I_O − 2ζω_n θ̇)`, `θ ← θ + dt·θ̇`.

With `dt = 11.11 ms`, the highest stable mode is 11.76 rad/s (0.13 rad/tick), so the integrator is well inside its stability limit. `τ_g` is sampled from a 181-point table at 0.5° resolution with cubic interpolation; the table is generated once at load from the spheroid support function. The slide is integrated with `a = −µg·v̂` and stops when `|v| < 2 mm/s`. The press force is applied as an impulse at release rather than a continuous force, because the 80–250 ms charge window is long compared with the 11 ms tick; this avoids a press that is accidentally double-counted across a frame boundary.

## 11. Rendering, Occlusion & Compositing
Layer order, near to far: passthrough base → table/felt scene mesh → ring chalk line and lip → shadow imps → sumo body → press aim fan → hand mesh → HUD and stability pips. The stone depth proxy is written at the fitted spheroid, so the virtual sumo body is clipped by the real stone only where the two overlap; the sumo mesh is drawn 0.4 mm outside the proxy to avoid z-fighting with the real surface.

**Occlusion cases.**

- **O1 — Ring under the real stone.** The chalk ring is depth-tested against the 75 mm spheroid; the arc behind the stone is hidden, which seats the stone on the table and preserves the 3-D read of the ring.
- **O2 — Pressing finger in front of the top pole.** The tracked hand mesh wins the depth test. The fingertip is visible over the summoning glow; a 1.2 mm contact halo is drawn under the hand mesh so the pad does not appear to float above the stone.
- **O3 — Imp behind the stone.** An imp on the far side of the stone is clipped by the spheroid proxy. Its shadow trail is also clipped, but its chitter audio continues, so the player can track it by ear.
- **O4 — Belly sweep over the ring.** The belly mesh is drawn 1.5 mm above the ring plane; where it crosses the chalk line, the line is hidden by the belly. This makes the ring-out moment read as a physical push, not a particle effect.
- **O5 — Left brace hand over the ring.** The brace palm is opaque and occludes the ring and imps behind it. The aim fan is clipped by the hand, so the player cannot aim through their own brace; the fan is duplicated as a 2 mm dry arc at the stone's base for redundancy.
- **O6 — HUD behind the stone.** The stability pips are rendered at 300 mm from the eye and depth-tested against the stone. A raised stone (re-seat) correctly covers the pips; the pips fade to 40 % rather than disappearing, so the player still sees the state.

**VFX budgets.** Ring chalk: 1 procedural annulus, 0.4 ms. Imp shadow trail: 12 quads per imp, max 14. Sumo dust: 40 sprites per lunge. Belly impact: 60 particles per bump, max 3 concurrent. Topple: 120 particles, 1 per bout. Aim fan: 1 translucent wedge, 24 vertices. Total transparent overdraw target < 1.8× screen area. The sumo body uses a two-tone toon shader with a 1.5 mm ink line; no refraction, because the real stone must remain visible as the anchor.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Press start | top pole | Low 120 Hz creak, 90 ms | Topknot glows amber, aim fan appears |
| Charge rising | top pole | Rising 180→420 Hz hum, gain ∝ `J/J_max` | Fan fills clockwise, stone squashes 0.4 mm |
| Release/lunge | stone centre | 55 Hz thump + 700 Hz whoosh, 220 ms; weak release 140 Hz tap, 60 ms | Dust ring, belly flash; weak: 6° rock, no dust |
| Belly bump | contact point | 90 Hz body thud + 1.6 kHz imp squeak | Imp squashes, 12 shadow motes |
| Ring-out | ring line | 440 Hz chalk chime + 220 Hz tumble | Imp tumbles out, chalk puff |
| Imp push | stone base | 160 Hz scrape, 120 ms | Stability pip drains, stone tilts 2° |
| Near-topple | stone base | 80 Hz warning pulse, repeating at 2 Hz | Rim line flashes red |
| Topple | stone side | 60 Hz boom + 300 Hz clatter, 500 ms | Stone flops, dust cloud, screen dip |
| Counter window | top pole | 900 Hz single ping, 40 ms | Fan turns gold for 0.20 s |
| Bout clear | ring centre | Taiko hit + major arpeggio, 800 ms | Imps bow, quota counter bursts |
| Bout lost | HUD | Descending 220→110 Hz tone, 700 ms | Stability pips crack |
| Re-seat | stone base | Soft 200 Hz settle, 200 ms | Stone upright, tilt zeroed |
| Tracker loss | head-locked | 180 Hz muted pulse | "Stand the stone" prompt |

All world cues are spatialised with a 1.0 m rolloff; HUD cues are dry. The stone thump is the substitute for the physical jolt of a 150 g rock landing, and it is intentionally the lowest-frequency cue in the mix so it reads through the music. The imp squeak is pitched up 15 % on every combo step, which gives the player an audible combo counter without looking at the HUD.

## 13. Difficulty Progression & Tuning

| Level | Imp quota | Imp speed | Ring radius | Bout clock | Stability | Press cap | Notes |
|---|---|---|---|---|---|---|---|
| 1 | 6 | 25 mm/s | 140 mm | 45 s | 120 | 40° | Teaching: wide ring, slow imps |
| 2 | 8 | 32 mm/s | 135 mm | 42 s | 110 | 38° | First two-imp convoys |
| 3 | 10 | 40 mm/s | 130 mm | 40 s | 100 | 35° | Baseline spec |
| 4 | 12 | 48 mm/s | 120 mm | 38 s | 90 | 32° | Tight ring, faster pushes |
| 5 | 14 | 55 mm/s | 110 mm | 35 s | 80 | 30° | Finale: 3 concurrent imp pushes |

Named constants: `MASS = 0.150 kg`, `A = 0.0375 m`, `C = 0.017 m`, `I_C = 5.086 × 10⁻⁵ kg·m²`, `I_O = 6.987 × 10⁻⁴ kg·m²`, `R_P = 82.72 mm`, `OMEGA_N = 11.76 rad/s`, `ZETA = 0.055`, `K_F = 250 N/m`, `J_MAX = 0.30 N·s`, `J_MIN = 0.10 N·s`, `MU = 0.38`, `E_IMP = 0.35`, `IMP_MASS = 0.020 kg`, `IMP_RADIUS = 6 mm`, `BELLY_RADIUS = 22 mm`, `RING_R = 130 mm`, `TOPPLE_ANGLE = 40°`, `WARN_ANGLE = 22°`.

Adaptive rule: after each bout, if the player cleared with ≥ 8 s left and ≤ 1 topple, raise imp speed by 3 mm/s (cap 55) and shrink the ring by 5 mm (floor 110) for the next bout. If the player lost stability below 30 or toppled twice, lower imp speed by 3 mm/s (floor 25) and restore 5 mm of ring radius (cap 140). Adaptation never changes `OMEGA_N`, `K_F`, or `J_MAX`, because the physical feel of the stone must remain constant.

## 14. Edge Cases & Failure Recovery
- **Stone tips onto its side.** The game detects `|θ| > 55°` and enters RESEAT. The bout clock pauses, stability −34 once, and the HUD shows a 3.0 s "stand it up" countdown. If the player re-seats within 3.0 s, the bout resumes at the same imp quota; otherwise the bout is lost.
- **Press exactly on the top pole.** A press within 2 mm of the pole has `|n_h| < 0.05`, so `J_h` and `J_θ` are both small. The classifier treats it as a *charge-only* press: it builds charge but produces a 0.04 N·s wobble and no lunge. A 200 Hz "dead press" tone warns the player to press off-centre.
- **Press too hard.** A press that drives `δ > 12 mm` saturates at 3.0 N and `J_max`; the excess is discarded. The stone does not physically compress, so no visual squash beyond 0.4 mm is shown. This prevents a heavy thumb from producing a topple on every press.
- **Hand occludes the stone.** If the pressing fingertip is lost behind the palm for > 120 ms, the charge is cancelled and the aim is held. The game does not invent a press from the last known position, because a phantom lunge would be worse than a no-op.
- **Stone slides off the tray.** The virtual translation is clamped to 90 mm from ring centre. If the real stone moves > 120 mm from its calibration point, the game pauses and asks the player to re-centre the stone; the virtual sumo walks back at 20 mm/s during the pause.
- **Table not detected.** The game falls back to a horizontal plane at the stone's bottom pole, with a warning banner. Ring and imp heights are then relative to that plane; registration still works, but the felt friction is assumed to be 0.38.
- **Tracking loss mid-lunge.** The virtual lunge continues on the model for up to 0.6 s, because the impulse has already been applied; input is frozen. After 1.5 s the game pauses. On re-acquire, the tracked tilt is compared with the model; if they differ by > 8°, the model is snapped to the tracked pose and stability is unchanged.
- **Left-handed player.** Mirror mode swaps the press hand to the left, mirrors the atlas λ about 0°, flips the `+x̂_P` calibration, and mirrors the aim fan. The physics is unchanged because the lunge direction is derived from the contact normal, not from a fixed hand side.
- **Two players / extra hands.** Any hand other than the registered right press hand and left brace hand is ignored. If a second person touches the stone, the contact is classified as a brace and cannot charge; the game does not pause, to avoid griefing in a shared room.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — registers the oval stone, emits `PoseFrame {p_P, q_P, confidence, mode}` at 72 Hz.
- `ContactBus` — 26 joints → `Contact {joint, p_P, n_P, v_rel, age}` with 5 mm enter / 8 mm exit and 30 ms debounce.
- `PressModel` — computes pad penetration `δ`, force `F`, charge `J`, aim `λ`, and release events.
- `RockingBody` — owns `θ, θ̇, λ`; integrates the ODE with the precomputed `z_c(θ)` table; detects topple.
- `LungeSolver` — converts `J` into `J_h`, `J_θ`, applies slide friction, and exposes `v_belly`.
- `BellyCollider` — sphere sweep against imps; computes `J_imp` and ring-out.
- `ImpDirector` — spawns imps, paths them inward, handles imp pushes and removal.
- `BoutStateMachine` — the table in §9; owns the 40 s clock and quotas.
- `StabilityMeter` — 100-point meter, regen, pip display, loss condition.
- `ScoreCombo` — bump, ring-out, bout, and run scores.
- `FeedbackRouter` — every event enum to one audio clip and one VFX prefab.
- `Compositor` — layer order, spheroid depth proxy, occlusion masks O1–O6.

**Event flow.** `PropTracker` → `PoseFrame` → `PressModel` and `RockingBody`; `ContactBus` → `PressModel`; `PressModel` → `LungeSolver` → `RockingBody` and `BellyCollider`; `BellyCollider` → `ImpDirector`, `ScoreCombo`, `FeedbackRouter`; `ImpDirector` → `BoutStateMachine` → `FeedbackRouter`. The state machine reads only `RockingBody.theta`, `ImpDirector.remaining`, `StabilityMeter.value`, and the bout clock; it never reads raw joints.

**Pseudocode 1 — press, charge, and release.**

```
function tick_press(dt):
    c = ContactBus.nearest(PRESS_JOINTS)
    if c == null or not in_cap(c.p_P):
        if charging and (now - last_contact > 120ms):
            charging = false; charge = 0
        return

    delta = clamp(signed_distance(c.p_P, spheroid), 0, 12mm)
    F = min(250 N/m * delta, 3.0 N)
    F = lowpass(F, 8 Hz, dt)
    if not charging:
        charging = true; charge = 0; aim = atan2(c.p_P.y, c.p_P.x)
    charge = min(charge + F*dt, 0.30 N*s)
    aim += 4 deg/mm * tangential_slide(c, dt)

    if c.lost and charge >= 0.10 N*s:
        n = surface_normal(c.p_P)
        J_h = charge * length(n.xy)
        d_hat = -normalize(n.xy)
        J_theta = length(cross(c.p_P - (0,0,-C), -charge*n))
        LungeSolver.apply(J_h, d_hat, J_theta)
        charging = false; charge = 0
        FeedbackRouter.play(LUNGE, stone_centre)
    elif c.lost: charging = false; charge = 0; FeedbackRouter.play(WOBBLE, stone_centre)
```

**Pseudocode 2 — rocking, belly bump, and ring-out.**

```
function tick_lunge(dt):
    tau_g = -MASS * G * dzc_dtheta(theta)       # table lookup, 0.5 deg
    theta_ddot = (tau_g + tau_ext) / I_O - 2*ZETA*OMEGA_N*theta_dot
    theta_dot += theta_ddot * dt
    theta += theta_dot * dt
    tau_ext = 0                                  # impulses are applied instantly

    if abs(theta) > 40deg:
        enter(TOPPLE); return

    if theta >= 8deg:
        p_belly = stone_pos + 8mm*up
        v_belly = slide_v + 8mm*theta_dot*belly_tangent
        for imp in ImpDirector.active():
            if distance(p_belly, imp.p) < 22mm + 6mm:
                m_eff = 0.150*0.020 / (0.150+0.020)
                J = (1+0.35) * m_eff * dot(v_belly - imp.v, d_hat)
                imp.v += (J / 0.020) * d_hat
                imp.hit = true
                ScoreCombo.on_bump(theta)
                FeedbackRouter.play(BUMP, imp.p)

    for imp in ImpDirector.active():
        if length(imp.p) > 130mm:
            ImpDirector.remove(imp)
            ScoreCombo.on_ringout()
            FeedbackRouter.play(RINGOUT, imp.p)
```

**Determinism.** Imp spawn longitudes and speeds use a PCG32 stream keyed by `(session_id, bout, imp_index)`. Given a recorded pose/contact trace, the 90 Hz simulation replays bit-identically, including the rocking table interpolation.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Spheroid registration | Present the stone at 1.0 m; inject 24 synthetic rim points with 0.3 mm noise | RMS < 0.5 mm; yaw error < 2.0°; bottom normal within 3° of `−ẑ_T` |
| AT-02 | Natural rocking frequency | Displace the model to θ = 5°, release with zero velocity, log 10 s | `f = 1.872 ± 0.020 Hz`; period 0.534 ± 0.006 s |
| AT-03 | Damping ratio | Fit the log decrement over 10 cycles | `ζ = 0.055 ± 0.005`; no negative damping at any amplitude ≤ 25° |
| AT-04 | Press force proxy | Inject δ = 2, 6, 12, 16 mm | F = 0.50, 1.50, 3.00, 3.00 N ± 3 %; rate limit holds |
| AT-05 | Press impulse and tilt | Press at φ = 60°, J = 0.24 N·s, λ = 0° | `J_θ = 2.42 × 10⁻³ N·m·s ± 4 %`; peak θ = 16.8° ± 1.5° |
| AT-06 | Lunge slide | Same press on µ = 0.38 | Slide 22 ± 3 mm; direction λ + 180° ± 3° |
| AT-07 | Belly bump | Place an imp at 26 mm in the lunge path, `θ = 12°` | Imp speed after impact 0.45–0.65 m/s; one bump score event |
| AT-08 | Ring-out | Bump an imp from 100 mm outward at 0.5 m/s | Imp crosses 130 mm; exactly one ring-out; +150 ± 1 |
| AT-09 | Imp push | Let an imp reach 18 mm with no lunge | Stability −8; tilt impulse 0.0012 N·m·s; no ring-out |
| AT-10 | Topple and re-seat | Press with J = 0.40 N·s (clamped to 0.30) and add a 0.05 N·m·s cross-impulse | Topple at |θ| > 40°; stability −34; RESEAT timer 3.0 s |
| AT-11 | Stability loss | Apply 13 imp pushes with no regen | Stability reaches 0 on the 13th; bout lost; `GAME_OVER` at second loss |
| AT-12 | Occlusion readback | Place an imp behind the stone; read depth buffer at stone centre | Imp fragment contribution = 0 inside the 75 mm silhouette; trail fades at limb |
| AT-13 | Tracking loss | Drop `PoseFrame` for 2.0 s during LUNGE | Model continues 0.6 s; pause at 1.5 s; snap on re-acquire if error > 8° |
| AT-14 | 10-minute soak | Scripted five-bout run at 90 Hz | Frame p99 < 11.1 ms; no energy gain in the rocking table; no imp tunnelling |

All tests run headless with synthetic pose and contact streams. AT-12 is a depth-buffer readback, not a screenshot. AT-02 and AT-03 use the same 181-point `z_c(θ)` table that ships in the build, so a table regression fails the physics tests rather than silently changing the feel.
