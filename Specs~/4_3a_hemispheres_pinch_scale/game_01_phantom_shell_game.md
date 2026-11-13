# Phantom Shell Game

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Phantom Shell Game |
| Slug | `phantom_shell_game` |
| Physical prop | Half walnut shell, dome-up; `R = 22.0 mm`, `t = 1.8 mm`, mass `4.0 g`; dark walnut shell, matte, µ ≈ 0.28 against melamine |
| Family / sub-shape | Hemisphere, dome-up, pinch scale |
| Collider family | Convex hemisphere + rim torus + inner cavity sensor |
| Scale class | Prop 1:1 (44 mm aperture); virtual play patch 190 mm × 160 mm |
| Source concept quote | "Slide it in 8 cm arcs with two fingertips and lift slightly to reveal. Two holographic shells appear beside the real one; a fairy hides under the real shell while phantom hands try to snatch it; slide in rhythmic evasions to protect the fairy." |
| Physical input | Right thumb + right index pinch the rim; horizontal co-motion slides, vertical co-motion lifts 12–30 mm |
| Session length | 6 min (5 rounds × 4–6 reaches + onboarding and score review) |
| Lose condition | Fairy trust reaches 0 (three successful snatches) |

## 1. Fantasy & Core Loop

The tabletop is a moonlit walnut grove rendered at 1:12 scale. The real half shell is the fairy's house: a brown dome with a warm seam of light escaping under the rim. Two holographic shells flicker into being beside it, identical in silhouette but 35% translucent, and they slide with the real shell like echoes. Phantom hands rise from the table edge — long, ink-black, wristless — and stab at whichever shell they believe is real. The player keeps the fairy alive by sliding the true shell in rhythmic 80 mm arcs, letting the phantoms strike holograms and empty felt, and occasionally lifting the shell 12–30 mm to let the fairy peek out and whisper the next strike direction. Lift too long and the fairy is exposed; slide too little and the lead-predicted strike lands on the shell.

1. **Read the telegraph (0.9 s).** A phantom wrist condenses at the patch edge, its finger traces a 40 mm pre-strike arc. The entry azimuth and the fingertip quiver encode the strike type: straight, feint, or pincer.
2. **Evade (0.45 s).** The player slides the shell along an 80 mm arc; the two holograms lag 180 ms behind, producing a three-body parallax that misdirects the phantom's lead solution.
3. **Strike (0.16 s).** The phantom hand traverses 90 mm and lands. The shell either blocks, grazes, or the strike passes cleanly; the fairy flinches on every near miss.
4. **Peek (0.2–0.9 s, optional).** A two-finger lift above 12 mm reveals the fairy, scores a scry, and previews the next telegraph 500 ms early. The shell must be reseated within 1.2 s.
5. **Recover (0.8 s).** The shell settles, the holograms re-phase, and the chain multiplier resolves.
6. **Round resolve (1.5 s).** Remaining reaches cancel, trust and score tally, and the grove dims for the next round.

One reach costs 2.2–3.3 s; a round is 13–19 s; five rounds plus onboarding, two allowed retries, and a score card fill the 6 min session.

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

- **Radius and wall**: `R = 22.0 mm`, `t = 1.8 mm`, inner radius `R_in = 20.2 mm`. Mass `4.0 g`; the shell is assumed uniform with areal density `σ = m/(2πR²) = 1.32 g/mm² × 10⁻³`.
- **Orientation**: dome-up. Convex exterior faces `+ẑ_P`, cavity opens toward `−ẑ_P`. Apex `A_P = (0, 0, +22.0) mm`. Rim circle at `z_P = 0`.
- **P origin**: rim-circle centre at `(0,0,0)`; at rest on the table `P_O` lies in the `T` plane (`h = 0`), and the apex sits 22.0 mm above the table.
- **Outer surface**: `p_P = R·(sinθ cosλ, sinθ sinλ, cosθ)`, `θ ∈ [0°, 90°]`, `λ ∈ [0°, 360°)`; outward normal `n = p_P/R`. Inner surface: same equation with `R_in`, normals pointing into the cavity (`−p_P/R_in`). The rim annulus between `R_in` and `R` is closed by the rim torus.
- **Rim**: torus with major radius `R = 22.0 mm`, minor radius `t/2 = 0.9 mm`, lying in `z_P = 0`.
- **V-frame**: anchored to `T`, origin at the play-patch centre `Q`, axes `x̂_V = x̂_T`, `ŷ_V = ŷ_T`, `ẑ_V = ẑ_T`. The patch is 190 mm × 160 mm with its long axis along `x̂_V`. `P` is expressed in `V`; the shell is free to translate on the `y_V = 0` plane.
- **Deviation 1**: `P` is not table-locked. Sliding is simulated with a 4 g virtual body, Coulomb friction `µ = 0.28`, and a soft 95 mm patch boundary.
- **Deviation 2**: the fairy is a 9 mm emissive mesh parented to `P` at `(0, 0, −4.0) mm`, inside the cavity, and never collides with the shell.
- **Deviation 3**: the two holographic shells are non-physical render entities parented to `P` with a 180 ms critically damped lag and ±55 mm lateral offsets; they are excluded from all gameplay collision tests.
- **Deviation 4**: lift is accepted only in `L ∈ [12, 55] mm`; at `L > 55 mm` the shell enters Fumble (cosmetic slip, no drop) and the fairy clings for 400 ms.

## 3. Physical Play Space & Ergonomics

The player sits at a table with the right forearm resting on the table edge, wrist neutral, and the shell 160–220 mm in front of the sternum. The right thumb and index approach the rim from above at a 55–70° cone, leaving the other three fingers curled and relaxed. The left hand rests palm-down beside the patch as a stabilizer and is ignored by gameplay except for a brace cue. The head tilts 18–25° down; the patch is rendered 40 mm above the physical table so the shell never appears buried.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Table `T`, 720–780 mm high | Keeps shoulder flexion < 30° |
| Patch | 190 mm × 160 mm, centred 190 mm from table edge | Contains 80 mm arcs plus overshoot |
| Pinch aperture | 30–46 mm at the rim | Comfortable thumb–index span for a 44 mm shell |
| Fingertip force | 0.15–0.6 N normal, ≤ 0.25 N tangential | Below 1 N pinching fatigue threshold |
| Sliding speed | 0–220 mm/s, typical 120 mm/s | 80 mm arc completes in 0.67 s |
| Lift range | 12–30 mm gameplay, 55 mm hard cap | Wrist extension stays < 20° |
| Gaze | 18–25° down, 300–450 mm head-to-patch | Avoids sustained neck flexion > 30° |
| Session | 6 min; 5 rounds; 20 s rest between rounds | Pinch stamina and attention |

Fatigue mitigations: (a) the shell needs only 0.15 N to hold because it rests on the table, so the pinch is a guide, not a clamp; (b) each round ends with a mandatory 4 s "hands open" rest during which input is ignored; (c) the lift phase is optional and capped at 0.9 s of scoring exposure, so the wrist never holds a pinch aloft; (d) the left hand is explicitly a rest hand, preventing symmetric overuse; (e) an on-table "grip load" arc appears after 35 s of continuous contact and pulses amber, prompting the player to release during the recover phase.

## 4. Prop Geometry & Registration

The prop is a `Juglans regia` half shell, sanded at the rim, with a natural 0.35–0.55 mm shell thickness variation that the registration fitter absorbs into a single effective `t`. The interior is unfinished and darker, which gives the tracker a strong inner/outer intensity step at the rim.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 22.0 mm ± 0.4 mm | Least-squares silhouette fit |
| Wall `t` | 1.8 mm ± 0.25 mm | Rim-step estimate |
| Inner radius `R_in` | 20.2 mm | `R − t` |
| Aperture | 44.0 mm | Rim diameter |
| Height (apex to rim) | 22.0 mm | Dome-up |
| Mass | 4.0 g ± 0.4 g | Kitchen scale, session calibration |
| Rim torus | major 22.0 mm, minor 0.9 mm | Contact collider |
| Surface friction | µ = 0.28 shell–melamine, 0.42 shell–skin | Measured, dry |
| Restitution | 0.15 normal, 0.05 tangential | Shell on table |

Surface-feature atlas (P-fixed; the walnut pattern also provides optical flow):

| Feature | θ from apex | λ | Purpose |
|---|---|---|---|
| Apex crown | 0°–18° | all | Lift-grip lower bound; no slide input |
| Slip band | 18°–55° | all | Accepted slide contact, low friction |
| Rim shelf | 55°–90° | all | Primary pinch band |
| Seam meridian | all | 0° ± 3° | Orientation readout; glows when fairy speaks |
| Fairy sigil | 63°–77° | 172°–188° | Hidden rune visible through the shell at 35% opacity |
| Inner rim lip | 90° | all | Seating datum for `h = 0` |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Convex hemisphere | `R = 22.0 mm`, `z ≥ 0` | Shell exterior, slide and block |
| C1 | Torus | major 22.0 mm, minor 0.9 mm, `z = 0` | Rim contact, lift grip |
| C2 | Concave hemisphere (sensor) | `R_in = 20.2 mm`, `z ≤ 0` | Fairy occupancy, peek exposure |
| C3 | Capsule | r = 1.4 mm, length 6 mm at each fingertip | Finger contact volumes |
| C4 | Plane | `y_T = 0` | Table support |
| C5 | Sphere | r = 14 mm | Phantom hand strike volume |
| C6 | Cylinder | r = 95 mm, h = 2 mm | Soft patch boundary |

Registration procedure: the player presents the shell 0.9–1.4 m from the head and rotates it through ≥ 100° of yaw plus 30° of pitch over 1.2 s, then holds still for 0.4 s. The fitter estimates `R`, `t`, rim plane, and apex from silhouette conics plus the rim intensity ring. Acceptance requires rim radius RMS < 0.35 mm, rim-plane normal error < 1.5°, apex error < 0.5 mm, and ≥ 20 inlier rim points. `P` then updates at 72 Hz and is interpolated to the 90 Hz physics tick.

Re-acquisition: after loss, search a 100 mm cube around the last `P` for 1.5 s. Success requires 14 rim inliers with RMS < 0.8 mm for 5 consecutive frames; the pose blends in over 120 ms. During loss, `P` continues on a constant-velocity prediction for 250 ms, then freezes. Loss > 1.2 s enters Recovery and damps slide gain to 0.2.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact region | Semantic output |
|---|---|---|---|---|
| I5.1 | Rim pinch | `R_thumb_tip`, `R_index_tip` | θ 55°–90°, opposed normals (`n_t·n_i < −0.65`) | Grip valid, slide enabled |
| I5.2 | Arc slide | same, midpoint velocity | θ 18°–90°, horizontal `|v_t| > 6 mm/s` | Planar shell velocity `v_s` |
| I5.3 | Lift | same, vertical co-motion | rim shelf; `v_y > 25 mm/s` together | Lift state, `L` in mm |
| I5.4 | Peek hold | same | `L ∈ [12, 30] mm` for ≥ 150 ms | Reveal fairy, scry preview |
| I5.5 | Reseat | same | `L` falls below 4 mm with `|v_y| < 15 mm/s` | Cover fairy, exposure reset |
| I5.6 | Rim tap | `R_middle_tip` | θ 70°–90°, λ ±20° from thumb | Sonar ring, reveals phantom type |
| I5.7 | Over-lift | I5.3 | `L > 55 mm` | Fumble cue, no drop |
| I5.8 | Release | any | all contacts lost > 80 ms | Slide inertia only, grip invalid |
| I5.9 | Brace | `L_index_tip`, `L_middle_tip` | table within 5 mm | Left-hand rest detection, no control |
| I5.10 | Handedness mirror | all | start menu option | Swap R/L roles, mirror `V` about `x̂_V = 0` |

Per-finger roles: the right thumb and index are a two-point rim clamp; the right middle is the sonar key; the right ring and pinky are curled and ignored. The left hand never touches the shell in the primary mapping; if it does, its contacts are logged as "assist" and contribute no input. Grip is valid only when both right contacts are inside the rim shelf, their normals oppose (`n_t·n_i < −0.65`), and each contact age exceeds 30 ms. Slide input uses the midpoint of the two tips; lift input uses their mean vertical velocity, which cancels differential tremor.

## 6. Input Kinematics & Signal Processing

Raw joints pass through a One Euro filter (mincutoff 1.4 Hz, beta 0.025, derivative cutoff 8 Hz), then finite-difference velocities at 90 Hz. Contact points are projected onto the rim torus and the outer hemisphere to obtain a surface point `c`, outward normal `n`, and tangential relative velocity `v_t = (v_finger − v_P) − ((v_finger − v_P)·n)n`. Slide uses the horizontal component only: `v_t,xy = (v_t·x̂_V, 0, v_t·ẑ_V)`.

Slide law: the virtual shell is a 4 g body with target velocity `v* = 0.90·v_t,xy`, first-order coupling `dv_s/dt = (v* − v_s)/τ_s` with `τ_s = 90 ms`, acceleration clamp `6000 mm/s²`, and speed clamp `|v_s| ≤ 220 mm/s`. With no valid grip, Coulomb friction applies along `−v̂_s` with `a_f = µ g = 0.28 × 9810 = 2747 mm/s²`; at 120 mm/s the coast distance is `v²/(2a_f) = 2.6 mm`, which is intentionally small so the 80 mm arcs remain player-driven rather than ballistic. The 6 mm/s dead zone is 0.5 mm per 83 ms of travel and rejects physiological tremor; the 220 mm/s ceiling is the 95th-percentile comfortable index–thumb sweep.

Lift law: `L = (P_O − T_O)·ŷ_T`, low-passed at 10 Hz. Lift is true when `L > 12 mm` for 150 ms and `v_L > 20 mm/s` at onset. Hysteresis reseats at `L < 4 mm` with `|v_L| < 15 mm/s`. The 12 mm threshold is 55% of `R`, enough to expose a 9 mm fairy plus a 3 mm sight line; the 4 mm reseat gap prevents chatter from table-contact jitter.

Exposure timer: `T_exp` starts when `L > 12 mm` and accumulates only while lifted. At `T_exp = 1.2 s` the fairy is "fully exposed" and the next strike is guaranteed to target her. A peek scores if the shell is reseated with `T_exp ∈ [0.2, 0.9] s`.

Strike prediction: the phantom solves `x_hit = x_shell + v_s · t_lead`, where `t_lead = k_lead · min(0.35 s, t_remaining)` and `k_lead = 0.55` at level 1 rising to 0.8 at level 5. Feint strikes compute the mirrored target `x_hit' = 2x_patch − x_hit` with 65% probability. The prediction is sampled 300 ms before impact and then frozen, so late player motion beats the phantom — this is the core skill.

Strike kinematics: the hand centre follows `x(t) = x_0 + (x_hit − x_0)·smoothstep(t/T_s)` with `T_s = 160 ms`; the strike volume C5 has radius 14 mm. The block test is planar distance `d = |x_hit − x_shell,xy|`; block if `d < R + 14 = 36 mm` and seated, near miss if `36 ≤ d < 48 mm`, clean evade if `d ≥ 48 mm`. A lifted shell exposes the fairy when `d_fairy = |x_hit − x_fairy,xy| < 16 mm`.

Signal chain: joints → One Euro → contact projection → tangential velocity → dead zone (6 mm/s, release 4 mm/s) → coupling → friction → patch spring `a_b = −k_b·(r − 95 mm)ẑ_r` with `k_b = 60 s⁻²`, clamped to 3000 mm/s² → integration. All classifiers debounce 30 ms and emit at most one event per tick.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to `T` at the patch centre as defined in §2.1. The physical shell at rest is `(0, 0, 0)` in `V`; the patch extends `x_V ∈ [−95, 95] mm`, `z_V ∈ [−80, 80] mm`. The player sits on the `−z_V` side, so "forward" on the table is `+z_V`.

| Entity | V-space position | Notes |
|---|---|---|
| Real shell rest | `(0, 0, 0)` | `P_O`, rim plane on table |
| Hologram A | `P_O + (−55, 0, 0) mm` + lag | 180 ms lag, 35% opacity |
| Hologram B | `P_O + (+55, 0, 0) mm` + lag | 180 ms lag, 35% opacity |
| Fairy | `P_O + (0, −4, 0) mm` in `P` | 9 mm tall, emissive |
| Phantom entry ring | radius 150 mm | Azimuth 0–360°, elevation 8–14° |
| Strike plane | `y_V = 3 mm` | Fingertips skim the table |
| Grove backdrop | `y_V = −1 mm` decal | 0.4 mm above table, no depth write |
| Patch boundary | cylinder r = 95 mm | Soft spring, visible as moss line |

The shell mesh is rendered at tracked `P` with no visual offset; the 22 mm dome is thick enough that passthrough depth does not fight. Holograms are rendered with a 2 mm normal offset and a Fresnel rim so they read as projections rather than solid shells. Their lag state is a critically damped second-order follower (`ω_n = 11 rad/s`, `ζ = 1.0`), which yields ~180 ms of visible delay at 120 mm/s without overshoot.

Scale mapping: 1 mm of fingertip travel maps to 1 mm of shell travel. An 80 mm arc at 120 mm/s takes 0.67 s, crosses 42% of the patch width, and leaves 9 mm of margin to the 95 mm soft wall. The phantom's 90 mm strike traverse at 160 ms is 562 mm/s, fast enough to read as a stab but slow enough (18 frames at 90 Hz) to be trackable. The fairy is rendered at 9 mm, which subtends 1.2° at 450 mm and remains resolvable without stereo stress.

## 8. Gameplay Systems & Rules

Entities: one real shell (state: seated/lifted/fumbled), two holograms (non-physical), one fairy (trust 3), 4–6 phantom reaches per round, and 0–2 pincer hands. Rounds are 1–5; reach counts are 4, 4, 5, 5, 6.

Fairy trust: starts at 3. A successful snatch (strike within 16 mm of the fairy while `L > 12 mm` and `T_exp < 1.2 s`) removes 1 trust and triggers a 1.5 s "grief" slowdown in which phantoms telegraph 30% longer. Trust 0 ends the run immediately.

Scoring per reach:
- Clean evade (seated, `d ≥ 48 mm`): `+100 × M_chain`.
- Near miss (seated, `36 ≤ d < 48 mm`): `+40 × M_chain`, fairy flinch VFX.
- Block (seated, `d < 36 mm`): `+30 × M_chain`, shell impulse, no trust loss.
- Peek scry (valid 0.2–0.9 s lift, reseated): `+25`, next telegraph revealed 500 ms early.
- Snatch: `−1 trust`, chain resets, `−150` score floor at 0.
- Round clear: `+200 × round`.

Chain multiplier: each consecutive non-snatch reach increments `n_chain`; `M_chain = min(3.0, 1 + 0.15·n_chain)`. A snatch, a fumble (`L > 55 mm`), or an exposure overrun resets `n_chain = 0`.

Phantom archetypes:
- **Straight** (rounds 1–5): one hand, lead prediction `k_lead`, no feint.
- **Feint** (rounds 2–5): telegraph azimuth `λ_t`; actual strike at `λ_t + 180°` with 65% probability, else `λ_t`.
- **Pincer** (rounds 3–5): two hands 140° apart; both must satisfy `d ≥ 36 mm`; blocking one still leaves the other live.
- **Double-tap** (round 4–5): two strikes 260 ms apart from the same entry; the second uses a fresh 200 ms prediction.

Win: clear round 5 with trust ≥ 1. Lose: trust 0. Perfect round (no block, no near miss, all clean evades) grants `+400` and a "ghost dancer" badge. The run is deterministic from a 64-bit seed; phantom azimuths, feint coin flips, and reach ordering are all seed-derived for replay.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset tracked | load grove, zero score | Register |
| Register | shell presented | rim RMS < 0.35 mm | set `P`, `R`, `t`, patch | Calibrate |
| Calibrate | both rim contacts valid 30 ms | grip normals opposed | paint rim reticle | RoundIntro |
| Calibrate | 4.0 s timeout | no grip | ghost-hand prompt | Register |
| RoundIntro | 1.5 s timer | round ≤ 5 | banner, reach schedule | Telegraph |
| Telegraph | reach scheduled | not last reach | show wrist, 550–900 ms | Strike |
| Strike | telegraph timer end | — | traverse 160 ms, resolve | Resolve |
| Resolve | 0.8 s timer | trust > 0 | score, chain, hologram re-phase | Telegraph |
| Resolve | reach count exhausted | round < 5 | round tally, 4 s rest | RoundIntro |
| Resolve | round = 5 | trust > 0 | victory tally | Victory |
| Any | trust = 0 | — | grief cue, freeze | GameOver |
| Any | `L > 55 mm` | — | fumble cue, 400 ms cling | same state |
| Any | `P` lost > 1.2 s | — | damp slide to 0.2, freeze phantoms | Recovery |
| Recovery | rim inliers 5 frames | prop visible | blend 120 ms, 1.5 s grace | previous state |
| Any | headset removed | — | save round, trust, seed | Paused |

All timers are wall-clock at the 90 Hz tick and are excluded from pause. The exposure timer `T_exp` is game-time and stops during Recovery and Paused. Transitions are idempotent and log `(tick, state_from, state_to, event)` for deterministic replay.

## 10. Physics & Collision

| Body | Shape | Mass | Friction | Restitution |
|---|---|---|---|---|
| Walnut shell | Hemisphere `R 22` + rim torus | 4.0 g | 0.28 table / 0.42 skin | 0.15 |
| Fingertip proxy | Capsule r 1.4 mm, L 6 mm | kinematic | 0.6 | 0.0 |
| Phantom hand | Sphere r 14 mm | kinematic | — | 0.0 |
| Table | Plane | static | 0.28 | 0.15 |
| Patch wall | Cylinder r 95 mm | static soft | — | 0.0 |

Moments: about the symmetry axis `I_z = (2/3)mR² = 1.29 × 10⁻⁶ kg·m²`; transverse `I_x = I_y ≈ (5/12)mR² = 8.07 × 10⁻⁷ kg·m²`. The shell's tilt is a damped spring to the table normal: `I_x θ̈ = −k_θ θ − c_θ θ̇ + τ_finger`, with `k_θ = 0.020 N·m/rad`, `c_θ = 1.6 × 10⁻⁴ N·m·s/rad`, and `θ` clamped to 25° while seated. Lip contact at the rim produces the audible wobble but never lets the shell flip.

Pair matrix: shell↔table (support, friction), shell↔fingertips (input), shell↔phantom (block only while seated), fairy↔phantom (snatch test, no rigid response), hologram↔anything (disabled), left hand↔shell (assist log only), shell↔patch wall (soft spring). The phantom strike is a swept sphere test from `x_0` to `x_hit` at 562 mm/s; per-tick travel is 6.2 mm, below the 14 mm strike radius, so discrete tests are sufficient, but a swept test is used anyway.

Block impulse: on a block, the shell receives `J = 0.0008 N·s` along the strike direction, giving `Δv = J/m = 0.20 m/s` (200 mm/s), which visibly jostles it but does not dislodge it from the pinch. A block also adds `ω_z = 7 rad/s` spin about `ŷ_V` for the wobble cue. Finger tangential force is clamped to 0.25 N; normal pinch force is reported but does not drive physics. The patch wall applies `F = −k_w m (r − 95)`, `k_w = 60 s⁻²`, capped at `4 m g`.

## 11. Rendering, Occlusion & Compositing

Layer order (near to far): 7 UI/scry glyphs → 6 hand mesh → 5 near VFX (phantom ink, sparks) → 4 real shell + holograms → 3 fairy and cavity glow → 2 grove decal → 1 scene-mesh depth → 0 passthrough. The real shell and holograms share layer 4 but sort by depth; holograms never write depth.

Occlusion cases:

1. **Fingers over the rim.** Hand-mesh depth hides the pinched rim band; the rim reticle is a 1.5 mm normal-offset decal and is depth-tested against the fingers, so it disappears under them rather than floating on top.
2. **Shell lifted, fairy visible.** The fairy is inside the cavity and is correctly occluded by the shell's near rim when viewed from a low angle. At lift angles above 20° the cavity opens and the fairy becomes visible; the cavity receives a warm bounce light so the silhouette reads.
3. **Hologram behind the real shell.** The hologram is depth-tested and disappears behind the opaque shell; its Fresnel rim remains faintly visible at the silhouette edge, preserving the "echo" read.
4. **Phantom hand in front of the shell.** The ink hand is a translucent volumetric shell (α = 0.72) that writes depth only at 30% coverage; the shell behind it is visible but tinted, avoiding a hard cut.
5. **Shell over the table edge.** The grove decal is clipped by the scene mesh with a 1.2 mm bias; the shell's contact shadow is a separate blob projected onto `T`, so no shadow floats off the table.
6. **Fairy behind the near rim while seated.** The fairy is fully hidden; a 2 mm warm seam under the rim and a 35% sigil on the dome communicate that she is inside, so the player never thinks she vanished.

VFX budget: 700 particles (300 phantom ink, 180 shell dust, 120 fairy motes, 100 spark), 4 decals, 2 hologram shells, 1 cavity light, 1 contact shadow. Overdraw ≤ 2.2×; all sprites in a 64 × 64 atlas at 90 Hz. Holograms use a single-pass additive Fresnel shader with no refraction; the real shell uses a two-layer shell shader (outer bark + inner rim glow) with a 0.6 ms budget.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Grip valid | Rim pinch point | Soft wooden tick, 900 Hz | Rim reticle closes to amber |
| Slide start | Shell contact | Dry walnut glide, noise burst 80 ms | Dust motes trail the rim |
| Arc peak | Shell | Pitch rises with speed, 220→480 Hz | Motion streak, 0.2 s |
| Lift | Shell | Rising airy shimmer | Cavity glow spills onto table |
| Peek scry | Fairy | Three-note whisper chime | Next phantom entry glyph appears |
| Reseat | Shell | Muted wooden knock | Seam dims, exposure ring empties |
| Phantom entry | Entry azimuth | Low ink rush, 90 Hz bed | Wrist condenses, finger quiver |
| Clean evade | Strike point | Whoosh + felt tap | White arc flare, +100 popup |
| Near miss | Strike point | Sharp whip, 1.8 kHz | Fairy flinch, vignette pulse 0.3° |
| Block | Shell | Hollow clang, 420 Hz | Shell jolt, spin wobble |
| Snatch | Fairy | Glass shatter + descending sigh | Trust pip cracks, color drains |
| Round clear | Patch centre | Warm lute cadence, 4 notes | Grove brightens, tally stamp |
| Fumble | Shell | Ceramic scrape | Shell ghost outline, "hold lower" |
| Recovery | Shell | Sonar ping, 660 Hz | Holograms dissolve, reticle returns |

All cues spatialise at the physical contact or the virtual strike; the scry chime is the only non-diegetic cue and is panned 15° toward the next entry. Audio latency budget < 20 ms; the block clang ducks the grove bed by 5 dB for 200 ms.

## 13. Difficulty Progression & Tuning

| Level (round) | Reaches | Telegraph (ms) | Lead `k_lead` | Feint chance | Pincer | Exposure limit (s) |
|---|---|---|---|---|---|---|
| 1 | 4 | 900 | 0.55 | 0% | no | 1.4 |
| 2 | 4 | 800 | 0.62 | 30% | no | 1.3 |
| 3 | 5 | 700 | 0.68 | 45% | yes, 1 pair | 1.2 |
| 4 | 5 | 620 | 0.74 | 55% | yes, 1 pair | 1.1 |
| 5 | 6 | 550 | 0.80 | 65% | yes, 2 pairs | 1.0 |

Adaptive rule: if the player suffers 2 snatches in a round, the next round adds 120 ms to every telegraph, multiplies `k_lead` by 0.60, and suppresses pincers for the first two reaches. If the player takes no snatch and no block for two consecutive rounds, subtract 80 ms from telegraphs and add 0.10 to `k_lead`, capped at level-5 values. Adaptation never changes reach count by more than ±1, so round length stays predictable.

Named tuning constants: `R = 22.0 mm`, `T_WALL = 1.8 mm`, `M_SHELL = 4.0 g`, `MU_TABLE = 0.28`, `A_FRICTION = 2747 mm/s²`, `V_SLIDE_MAX = 220 mm/s`, `TAU_SLIDE = 90 ms`, `DZ_SLIDE = 6 mm/s`, `DZ_RELEASE = 4 mm/s`, `LIFT_ON = 12 mm`, `LIFT_OFF = 4 mm`, `LIFT_MAX = 55 mm`, `T_EXPOSE = 1.2 s`, `PEEK_MIN = 0.2 s`, `PEEK_MAX = 0.9 s`, `BLOCK_D = 36 mm`, `NEAR_D = 48 mm`, `STRIKE_R = 14 mm`, `STRIKE_T = 160 ms`, `FAIRY_SNATCH = 16 mm`, `CHAIN_CAP = 3.0`, `PATCH_R = 95 mm`.

## 14. Edge Cases & Failure Recovery

- **Prop tracking loss.** Keep constant-velocity prediction for 250 ms, then freeze `P` and damp slide gain to 0.2. Holograms freeze with `P` so the misdirection does not lie. If re-acquisition fails in 1.5 s, pause and prompt with a rim glow at the last pose.
- **Finger tracking loss mid-slide.** The shell coasts with Coulomb friction and stops in ≤ 2.6 mm from 120 mm/s. Grip invalidates after 80 ms; when both tips return, require 30 ms of fresh contact before resuming, preventing a velocity snap.
- **Thumb/index rollover.** If the two contact normals cease to oppose (`n_t·n_i > −0.4`), keep sliding but scale gain to 0.5 and play a "regrip" tick. If normals invert, swap the two contact roles over 100 ms; never reverse the slide direction.
- **Lift beyond 55 mm.** Enter Fumble: the shell visual lags 40 mm below the fingers, the fairy clings to the cavity, and input is damped 60% for 400 ms. No drop physics, because a dropped 4 g shell is a real-world distraction.
- **Exposure overrun.** If `T_exp > 1.2 s`, the next strike is forced onto the fairy regardless of prediction, but the player still has the normal telegraph time. This makes the cost legible rather than random.
- **Feint at patch edge.** Mirrored targets are clamped into the 190 × 160 mm patch with a 10 mm inset; if the mirrored point would fall outside, the feint resolves to the original target and the coin flip is refunded (no wasted reach).
- **Pincer with one hand occluded.** If the second phantom is occluded by the player's hand mesh for > 400 ms, its strike is delayed 120 ms and its ink becomes 85% opaque, so the player is not hit by an invisible hand.
- **Handedness swap.** Mirror `V` about `x̂_V = 0`, swap all R/L joint roles, and negate feint azimuths. Geometry, thresholds, and scoring are unchanged.
- **Headset removed.** Save round, trust, score, seed, and `P`. On resume, require re-registration and a 2 s settle; the exposure timer is reset so the player is not punished for the pause.
- **Table surface changes mid-session** (lap to table). Re-anchor `T`, re-project `P`, and preserve the shell's patch-relative position; if `T` normal changes more than 10°, re-run registration.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `PropTracker` | Hemisphere fit (`R`, `t`, rim plane), 72 Hz pose, loss/re-acquire FSM |
| `ShellBody` | 4 g planar body, friction, coupling, patch spring, tilt spring, clamp |
| `HandContacts` | Per-joint signed distance to C0/C1, debounce, contact records |
| `PinchClassifier` | Rim pinch validity, lift/reseat/peek/tap classifiers |
| `FairyState` | Trust, exposure timer, cavity parenting, scry availability |
| `HologramRig` | Two lagged follower transforms, non-physical flags |
| `PhantomDirector` | Seeded reach schedule, archetype selection, telegraph timing |
| `StrikeResolver` | Lead prediction, swept strike, block/near/clean/snatch tests |
| `ScoreService` | Chain multiplier, peek bonus, round tally, badge logic |
| `AudioDirector` | Cue table, spatialisation, ducking, latency budget |
| `SessionFSM` | State table in §9, timers, pause, deterministic replay log |

Event flow: `HandTracking(72 Hz) → HandContacts → PinchClassifier → ShellBody → PhantomDirector → StrikeResolver → FairyState/ScoreService → AudioDirector/Renderer`. `SessionFSM` owns all transitions and subscribes to trust changes, reach completion, timer expiry, and tracker health. Rendering interpolates `P` and the hologram followers between the last two 90 Hz ticks; gameplay reads only tick-snapshotted state.

```text
function update_shell(contacts_R, P, dt):
    grip = pinch_valid(contacts_R.thumb, contacts_R.index)
    if grip:
        v_f = midpoint_velocity(contacts_R.thumb, contacts_R.index)
        v_t = tangent_xy(filter(v_f), normal=T.y)
        if |v_t| < 6 mm/s and |v_s| < 4 mm/s:
            v_s = 0
        else:
            v_star = clamp_mag(0.90 * v_t, 220 mm/s)
            v_s += (v_star - v_s) * (dt / 90 ms)
        v_s = clamp_mag(v_s, 220 mm/s)
    else:
        v_s -= normalize(v_s) * 2747 mm/s^2 * dt
        if |v_s| < 1 mm/s: v_s = 0
    P_O += v_s * dt
    r = |P_O.xz|
    if r > 95 mm:
        P_O -= normalize(P_O.xz) * (r - 95) * (60 * dt^2)
    return P_O, v_s
```

```text
function resolve_strike(x0, x_hit, P, fairy, lift_mm, t_exp):
    x = swept_point(x0, x_hit, speed=562 mm/s, dt=1/90 s)
    d = planar_distance(x_hit, P_O)
    if lift_mm > 12 mm and distance(x_hit, fairy.xy) < 16 mm:
        return SNATCH
    if lift_mm <= 12 mm and d < 36 mm:
        apply_impulse(P, J=0.0008 N*s, dir=normalize(x_hit - x0))
        return BLOCK
    if lift_mm <= 12 mm and d < 48 mm:
        return NEAR_MISS
    if lift_mm > 12 mm and t_exp > 1.2 s:
        return SNATCH
    return CLEAN_EVADE
```

State is snapshotted at each reach boundary. A crash restores the snapshot and replays up to 2.0 s of recorded joint data. All constants live in a single `TuningConfig` asset, hot-reloadable in development, with the values in §13 as the shipping defaults.

## 16. Acceptance Test Checklist

1. **Hemisphere fit.** Present the shell at 1.1 m and sample 500 frames. Pass: `R ∈ [21.6, 22.4] mm`, `t ∈ [1.55, 2.05] mm`, rim-plane normal error < 1.5°, apex error < 0.5 mm.
2. **Slide law.** Inject a 120 mm/s horizontal fingertip velocity for 0.5 s, then release. Pass: steady-state `v_s = 108 ± 5 mm/s`; after release the shell travels 2.6 ± 0.4 mm and stops within 220 ms.
3. **Friction constant.** Release from 220 mm/s on the bare table. Pass: stopping distance 8.8 ± 1.0 mm, consistent with `v²/(2 × 2747)`.
4. **Patch wall.** Drive the shell at 300 mm/s into the boundary. Pass: centre never exceeds 96.5 mm; the return acceleration peaks below 3000 mm/s²; no oscillation > 1 mm after 400 ms.
5. **Lift classifier.** Raise the shell 0→20 mm in 250 ms, hold 500 ms, lower. Pass: lift fires at 12 ± 1 mm, peek scores once, reseat fires below 4 mm, and no duplicate events.
6. **Exposure timer.** Hold the shell at 20 mm for 1.5 s. Pass: `T_exp` reaches 1.2 s at 1200 ± 30 ms, the "exposed" cue fires once, and the next strike targets the fairy.
7. **Block geometry.** Place strike centres at 35, 36, 47, 48, and 49 mm from a seated shell. Pass: block, block, near miss, near miss, clean evade respectively, with exactly one event per strike.
8. **Lift snatch.** Lift 18 mm, place the strike 15 mm from the fairy at `T_exp = 0.5 s`. Pass: snatch fires once, trust decrements 3→2, and the grief telegraph is +30%.
9. **Feint determinism.** Run 200 seeded reaches at level 4. Pass: feint rate = 55% ± 2%, mirrored target is clamped inside the patch, and replaying the same seed reproduces every azimuth and outcome bit-exactly.
10. **Hologram lag.** Move the shell in a 40 mm step. Pass: each hologram reaches 63% of the step in 180 ± 20 ms with no overshoot > 2%, and neither enters collision.
11. **Occlusion.** Place the right palm between the camera and the shell for 120 frames. Pass: the shell is fully hidden, the 30% rim reticle remains, and no depth inversion or z-fighting appears.
12. **Performance.** Run level 5 with two pincers and 700 particles. Pass: GPU frame < 11.1 ms, CPU game thread < 6 ms, particle count ≤ 700, overdraw ≤ 2.2×.
13. **Recovery.** Hide the shell for 1.0 s, then re-present it. Pass: re-acquisition in ≤ 1.5 s, slide gain restored to 1.0 within 200 ms, and trust/score unchanged during the outage.
14. **Session endurance.** Run the 6 min scripted session with a synthetic hand at 72 Hz. Pass: no `P` drift > 1.0 mm, no accumulated score error, and memory growth < 4 MB.
