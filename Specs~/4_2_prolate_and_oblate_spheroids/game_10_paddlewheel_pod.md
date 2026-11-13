# Game 10 — Paddlewheel Pod — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Paddlewheel Pod |
| Slug | `paddlewheel-pod` |
| Physical prop | Prolate lemon stone, 70 × 48 × 48 mm, 90 g, pinched at both poles by left thumb and middle finger |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate & Oblate Spheroids |
| Scale class | Tabletop, two-hand asymmetric grip, continuous spin input |
| Source concept | Wooden paddlewheels sprout at the equator; spinning the belly drives a steam bathysphere up a kelp forest while tilting the left hand steers past electric eels |
| Source physical input | Clamp the egg's poles between left thumb and middle finger; stroke the exposed belly with the right index and middle fingers; tilt the left hand to steer |
| Session shape | 5 dives, 30–70 s each, ≈5 min |
| Primary skill | Sustained two-hand coordination: spin rate for ascent, tilt for lateral dodge |

## 1. Fantasy & Core Loop

**Fantasy.** The egg is the pressure hull of a steam bathysphere. Wooden paddlewheels unfold around its equator; every stroke of the right hand turns them, and every turn pumps the pod up through a black kelp forest. Electric eels patrol the lanes, and the only rudder is the tilt of the left hand that holds the hull.

**Core loop** (dive 30–70 s; session 5 dives):

1. **Clamp** (0.8 s) — left thumb and middle finger pinch the poles; the pod arms.
2. **Spin** (continuous) — right index/middle stroke the belly tangentially; spin `ω_s` drives ascent `v_up = 4.0·ω_s mm/s`.
3. **Steer** (continuous) — tilt the long axis 0–22°; lateral speed `v_lat = 240·sinθ mm/s`.
4. **Manage steam** — spinning drains the boiler; coasting regenerates it; at 0 steam the pod sinks.
5. **Read eels** — 0.35 s glow wind-up, then a 40 mm lunge; dodge with tilt or hide in a kelp clump.
6. **Surface** — reach depth 0; hull intact; bank the score.
7. **Next dive** — 1,200 → 1,600 → 2,000 → 2,400 → 2,800 mm.

The challenge is a two-rate control problem: ascent is proportional to spin, but the boiler only regenerates when spin is near zero, so the player must alternate 6–8 s spin bursts with 4–5 s glides while still dodging.

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

- **Exact spheroid**: `a = b = 24.0 mm`, `c = 35.0 mm`, prolate (`c/a = 1.458`), `m = 90 g`, density 1.07 g/cm³. P origin = geometric center; `+ẑ_P` = long axis; poles at `(0, 0, ±35.0) mm`; equator = circle of radius 24.0 mm in the `x̂_P`–`ŷ_P` plane. At dive start the long axis is vertical: `+ẑ_P` ≈ `+ŷ_V`.
- **Local rolling model**: `ω = (n̂ × v_t)/r_eff`, with `r_eff` from Euler's theorem: `1/r_eff = cos²ψ/(c²/a) + sin²ψ/a`, where `ψ` is the angle between the tangential stroke and the long axis. Around the equator (the paddlewheel direction) `r_eff = a = 24.0 mm`; along the meridian `r_eff = c²/a = 51.0 mm`. Slip clamp 450 mm/s.
- **V frame**: anchored to **P at the instant of dive start** (`P_0`), a frozen copy. Origin = egg center at dive start; `+ŷ_V` = the long axis at dive start (vertical); `+x̂_V` = player forward projected perpendicular to `+ŷ_V`; `+ẑ_V = x̂_V × ŷ_V`. The submarine moves in V; the live `P` is compared to `P_0` to produce steering tilt.
- **Deviations**: (a) the egg is smooth and featureless, so the real spin `ω_s` is integrated from belly strokes and decays with `τ = 1.9 s`; there is no visual spin reference on the prop; (b) the pole pinch is a two-finger clamp and the left hand remains free to tilt, so the pinch axis doubles as the steering stick; (c) the submarine hull is drawn around the real egg, which stays visible inside at 70 % opacity; (d) the water column is virtual and has no real fluid forces.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated or standing; left hand held at 240–320 mm in front of the sternum, elbow supported; right hand above and to the side of the egg.
- **Pinch**: `L_thumb_tip` on `+ẑ_P` and `L_middle_tip` on `−ẑ_P` (or mirrored), separation 65–78 mm; the egg's long axis is vertical.
- **Belly access**: the exposed equatorial band is 48 mm wide; the right index and middle stroke a 60–90° arc without crossing the left fingers.
- **Steering envelope**: ±22° tilt of the long axis in any direction; the left wrist supplies it, the fingers do not release.
- **Play volume**: the pod's virtual shaft is a 600 mm diameter cylinder, rendered 150–450 mm in front of the player; no real movement beyond the hands is required.
- **Forces**: pinch 3–8 N static; belly stroke 0.2–0.8 N; no impact, so fatigue is low except for the sustained pinch (mitigated by the 20 s rest).
- **Session**: 5 dives, hard stop 9 min; 20 s rest between dives.
- **Fatigue mitigations**: (1) the left pinch can be relaxed to 2 N between bursts; (2) the right hand rests during glides; (3) a 20 s rest every dive; (4) the pod holds depth when spin is zero only if steam > 10 (neutral buoyancy), so the player can fully relax the right hand.

## 4. Prop Geometry & Registration

- **Real dimensions**: 70 mm long, 48 mm wide, 48 mm high, 90 g; tolerance ±1.0 mm length, ±0.6 mm width, ±2 g. Model volume `(4/3)πa²c = 84,450 mm³`.
- **Inertia**: `I_z = (2/5)ma² = 2.07 × 10⁻⁵ kg·m²` about the long axis; `I_x = I_y = (1/5)m(a² + c²) = 3.24 × 10⁻⁵ kg·m²` transverse. Used for the spin and tilt estimates only.
- **Surface atlas**: 4 latitude bands — pole caps (`|φ| > 70°`, pinch zones), shoulder (`35° < |φ| ≤ 70°`), belly (`|φ| ≤ 35°`, paddlewheel band), and a 6 mm equator stripe for stroke registration. 24 meridians for stroke direction.
- **Colliders**: `COL_egg` = spheroid; `COL_pole_top`, `COL_pole_bot` = 12 mm discs at the poles (pinch); `COL_belly` = the equatorial band (stroke); `COL_hull` = a virtual capsule around the egg (r = 40 mm) used for eel and kelp collision; `COL_table` = plane for the resting pose.
- **Registration**: (1) the player holds the egg vertically by the poles for 1.0 s; (2) the system fits the spheroid over 12 frames, accepting center error < 0.7 mm and axis error < 3°; (3) the long axis is assigned `+ẑ_P` toward the thumb (up); (4) `P` publishes at 72 Hz, predicted to 90 Hz; `P_0` is snapshotted at the dive-start countdown.
- **Re-acquisition**: search 60 mm around the pinch; refit the center and long axis; if the pinch is lost, hold the last `P` for 400 ms and then pause. A re-grip requires a 65–78 mm pole separation to re-arm.
- **Tolerances**: a 1.0 mm length error changes `r_eff` along the meridian by 2.8 %, inside the 5 % gameplay tolerance; the equatorial `r_eff` (the paddlewheel radius) is unaffected by length.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Pole clamp | `L_thumb_tip` + `L_middle_tip` | `COL_pole_top/bot` | armed; separation 65–78 mm |
| I5.2 | Belly stroke | `R_index_tip` / `R_middle_tip` tangential | `COL_belly`, `|φ| ≤ 35°` | spin impulse `ω += (n×v_t)/a` |
| I5.3 | Reverse stroke | stroke against current spin | `COL_belly` | braking torque, −40 % efficiency |
| I5.4 | Tilt steer | live `+ẑ_P` vs `+ŷ_V` | — | `v_lat = 240·sinθ` in the tilt direction |
| I5.5 | Pinch tighten | thumb–middle separation −6 mm for 200 ms | poles | "brace": immune to one eel bump, 4 s cooldown |
| I5.6 | Release | pole separation > 85 mm | — | pause, pod holds depth for 1.5 s |
| I5.7 | Re-grip | separation 65–78 mm | poles | resume |
| I5.8 | Pause | right palm up 700 ms | — | suspend timers |

Per-finger roles: left thumb + middle = clamp and steering; right index = primary stroke; right middle = secondary stroke (70 % gain, used for sustained spin); right ring/pinky ignored. Per-hand roles: left = HULL/STEER, right = DRIVE. If the left clamp opens, the pod loses steering within one tick and holds depth for 1.5 s before sinking.

## 6. Input Kinematics & Signal Processing

- **Spin state**: `ω_s` about `+ẑ_P`, integrated from belly strokes. `Δω = (r_b × J_t)/I_z`, with `r_b = 24 mm` and `J_t` the tangential impulse; equivalently `ω += (n̂ × v_t)/a · dt` while in contact. Cap ±18 rad/s.
- **Spin decay**: `ω_s ← ω_s·e^(−dt/1.9)` plus a paddlewheel drag term `−0.06·ω_s·|ω_s|/18·dt`; at 12 rad/s the combined decay is 6.8 rad/s², so the player must stroke at ≈3 Hz (each 0.2 s stroke at 300 mm/s adds 2.5 rad/s) to hold speed. This is what makes the spin a continuous two-hand effort rather than a one-off flick.
- **Ascent**: `v_up = 4.0·ω_s` mm/s, capped 60 mm/s; at 12 rad/s, 48 mm/s. If `ω_s < 2`, neutral buoyancy holds depth while steam > 10; at steam 0, `v_up = −24 mm/s` (sink).
- **Steering**: `u = normalize(project(+ẑ_P, plane ⟂ +ŷ_V))`; `θ = acos(+ẑ_P·+ŷ_V)`; `v_lat = 240·sin(min(θ, 22°))·u` mm/s, capped 90 mm/s. A 22° tilt gives 90 mm/s; a 10° tilt gives 42 mm/s.
- **Tilt filtering**: the live `P` axis is low-passed at 4 Hz (the hand is noisy at 2–3 Hz), then the tilt is computed; a 120 ms lead predictor compensates for the eel lunge.
- **Steam**: `S` 0–100. Drain `5.5·(ω_s/12)/s` when `ω_s > 2`; regen `5.0/s` when `ω_s < 2`; kelp contact drains an extra 3 instantly; each eel hit −15.
- **Pressure**: `p = 1 + D/10,330 atm` (D in mm; 10.33 m of water per atm); at 2,800 mm, 1.27 atm. Cosmetic HUD only; no hull-stress mechanic.
- **Depth**: `D ← D − v_up·dt`; starts at the dive depth; surface at `D ≤ 0`. The pod's lateral position integrates `v_lat`; the shaft is clamped to `|x_V|, |z_V| ≤ 300 mm`.
- **Eel detection**: each eel has a patrol lane at depth `D_e`; it lunges when the pod is within 90 mm and within ±25 mm of its lane; wind-up 0.35 s (glow + click), then 40 mm toward the pod at 220 mm/s. A hit requires center distance < 55 mm.
- **Kelp**: clumps at fixed `(D, x, z)` with a 40 mm radius; entering slows ascent 25 % for 1.5 s, drains 3 steam, and halves the eel detection radius while inside.
- **Signal chain**: joints → 3-tap median → one-euro filter (1.5 Hz min cutoff, β = 0.03) → contact → impulse integration; the egg pose is fitted at 72 Hz and blended with the integrated spin at 30 % to correct drift.
- **Latency**: 14 ms tracking + 11 ms physics + 11 ms render + 22 ms audio = 58 ms; steering has a 120 ms predictor, so the effective dodge latency is 0 ms at the design speed.

## 7. Virtual World Layout & Coordinate Mapping

- **V axes**: origin at the egg center at dive start; `+ŷ_V` = up (the initial long axis); `+x̂_V` = player forward; `+ẑ_V` = player's right. Units mm.
- **Shaft**: a 600 mm diameter cylinder, `y_V ∈ [−2800, 0]`; rendered 150–450 mm in front of the player's chest; the pod's `y_V = −D`.
- **Pod**: a 140 mm diameter brass bathysphere with the real egg visible inside; two paddlewheels of 96 mm diameter at the equator; a steam stack; a 20 mm porthole.
- **Kelp**: 40–70 stalks per dive, each a 12-segment ribbon from the floor to 200 mm below the surface; clumps of 5–8 stalks form cover volumes.
- **Eels**: 3–6 per dive, each 180 mm long, patrol lane at a fixed depth, lateral sinusoid amplitude 60–110 mm, period 3–5 s, speed 40–70 mm/s.
- **Surface**: a bright plane at `y_V = 0` with a 40 mm wave band; crossing it ends the dive.
- **Floor**: at `y_V = −(dive depth + 120) mm`; touching it is a soft fail (hull −1, bounce up 30 mm).
- **Mapping formulas**: `p_V = (x, −D, z)`; pod tilt from `P`; eel `p_V = (A·sin(2πt/T + φ), −D_e, lane_z)`; kelp sway `x += 6·sin(0.8t + phase)`.
- **Camera note**: the shaft is vertical in V and does not rotate with the head; the player looks slightly down at the pod and up the shaft, so the surface is visible at the top of the view.

## 8. Gameplay Systems & Rules

- **Entities**: `Egg` (P, ω_s, θ), `Pod` (x, z, D, steam, hull), `Paddlewheel` (visual spin), `Eel` (lane, phase, state), `KelpField`, `DiveDirector`.
- **Hull**: 3 pips; eel hit −1 and 1.2 s invulnerability; floor hit −1; at 0 the dive fails and the pod surfaces automatically.
- **Steam**: 100 units; drain/regen as §6; at 0 the pod sinks at 24 mm/s and cannot ascend until steam > 8.
- **Neutral buoyancy**: with `ω_s < 2` and steam > 10, `v_up = 0`; this is the glide state and the only time steam regenerates.
- **Eel dodge bonus**: passing within 90 mm but outside 55 mm during a lunge awards +20 and a "close call" cue.
- **Kelp cover**: inside a clump, eel detection radius ×0.5; entering costs 3 steam and 25 % ascent for 1.5 s.
- **Scoring**: depth 1 point per 10 mm; dodge +20; kelp cover +10; dive clear +300; no-hit dive +250; hull remaining ×150; steam remaining ×2.
- **Dives**: depths 1,200 / 1,600 / 2,000 / 2,400 / 2,800 mm; eels 3/4/4/5/6; steam drain ×1.0/1.05/1.1/1.15/1.2.
- **Win/lose**: 5 dives; win if ≥ 3 surfaced; a failed dive may be retried once at the same level.
- **Assist**: after two failed dives, eel wind-up extends to 0.55 s and the lunge speed drops to 160 mm/s for the next dive only.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, sensor test | REGISTER |
| REGISTER | egg vertical, still 1.0 s | center < 0.7 mm | set P, long axis up | CALIBRATE |
| CALIBRATE | pole clamp stable | 65–78 mm, 300 ms | snapshot P_0, build V | BRIEF |
| BRIEF | 3 s card | — | dive 1 depth, eel set | CLAMP |
| CLAMP | pole clamp held | 65–78 mm | arm pod, steam = 100 | DESCEND |
| DESCEND | belly stroke | contact `|φ| ≤ 35°` | `ω_s += (n×v_t)/a·dt` | DESCEND |
| DESCEND | `ω_s > 2` | steam > 0 | `v_up = 4·ω_s`, steam −5.5·ω/12 | DESCEND |
| DESCEND | `ω_s < 2` | steam > 10 | neutral buoyancy, steam +5/s | DESCEND |
| DESCEND | steam = 0 | — | sink 24 mm/s, alarm | DESCEND |
| DESCEND | tilt > 4° | — | `v_lat = 240·sinθ` | DESCEND |
| DESCEND | eel within 90 mm | wind-up 0.35 s | lunge 40 mm at 220 mm/s | EEL |
| EEL | hit < 55 mm | not invulnerable | hull −1, steam −15, 1.2 s i-frames | DESCEND |
| EEL | miss > 55 mm | — | +20 dodge | DESCEND |
| DESCEND | kelp clump entered | radius < 40 mm | −3 steam, speed ×0.75 1.5 s | DESCEND |
| DESCEND | D ≤ 0 | hull > 0 | surface, +300 | SCORE |
| DESCEND | hull = 0 | — | auto-surface, dive failed | SCORE |
| DESCEND | floor contact | — | hull −1, bounce +30 mm | DESCEND |
| DESCEND | pole clamp opened | > 85 mm | hold depth 1.5 s then sink | RECOVER |
| RECOVER | re-clamp | 65–78 mm | resume, steam unchanged | DESCEND |
| RECOVER | 5 s timeout | — | dive failed | SCORE |
| SCORE | dive < 5 | — | tally, 20 s rest | BRIEF |
| SCORE | dive = 5 or 3 fails | — | session stats | COMPLETE |
| any | prop lost > 200 ms | — | freeze pod, ghost egg | REACQUIRE |
| REACQUIRE | relock ≤ 2.0 s | search 60 mm | hold D, steam | prior |
| REACQUIRE | 2.0 s timeout | — | "present the egg" card | REACQUIRE |
| any | palm up 700 ms | — | freeze timers | SUSPENDED |
| SUSPENDED | palm down 500 ms | — | 1.5 s countdown | prior |

Timers: clamp 300 ms; spin decay τ = 1.9 s; eel wind-up 0.35 s; invulnerability 1.2 s; kelp slow 1.5 s; clamp-open hold 1.5 s; dive rest 20 s; reacquire 2.0 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_egg` (spheroid), `COL_pole_top/bot` (12 mm discs), `COL_belly` (band), `COL_hull` (capsule r = 40 mm), `COL_table` (plane), 6 eel capsules (r = 18 mm, length 180 mm), 70 kelp capsules (r = 6 mm), surface plane, floor plane.
- **Pair matrix**: Fingers↔Egg (contact/impulse); Pod↔Eel (hit event); Pod↔Kelp (slow/drain/cover); Pod↔Floor (bounce); Pod↔Surface (clear); Eel↔Kelp (none, eels pass through); Eel↔Eel (none); Hand↔Hull (ignore — the hull is virtual).
- **Kinematics**: the pod is a point in V integrated at 90 Hz; `D ← D − v_up·dt`; `x,z ← x,z + v_lat·dt`; no rigid-body rotation (the hull is a billboarded capsule).
- **CCD**: pod vertical speed ≤ 60 mm/s → 0.67 mm/frame; eel lunge 220 mm/s → 2.4 mm/frame; both far below the 55 mm hit radius, so no CCD is needed. Eel hits use a capsule-point distance at each tick.
- **Materials**: egg↔skin µ = 0.38; egg↔table µ = 0.45; hull↔water drag is encoded in the spin decay; kelp is soft (no bounce).
- **Forces**: belly stroke 0.2–0.8 N; pinch 3–8 N; no gravity on the pod (buoyancy model); eel lunge 220 mm/s; floor bounce restores 30 mm of depth.
- **Steam model**: first-order drain/regen with hard clamps at 0 and 100; drain scales linearly with `ω_s` above 2 rad/s; no thermal inertia (kept readable).
- **Surface check**: `D ≤ 0` triggers the clear exactly once per dive via a latched flag.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → shaft water volume → kelp back layer → eels → pod hull + egg → paddlewheels → steam bubbles → kelp front layer → hands → HUD.
- **Occlusion cases**:
  1. *Left thumb and middle finger*: they clamp the poles and occlude the top and bottom 12 mm of the egg; the paddlewheels are drawn only on the exposed belly band, never under the fingers.
  2. *Egg inside the hull*: the real egg is visible through the 70 % opaque hull; the hull is depth-tested against the egg, so the egg reads as inside rather than in front.
  3. *Kelp in front of the pod*: front-layer stalks occlude the pod; a 2 mm brass outline keeps the pod readable through dense cover.
  4. *Eel behind kelp*: eels are depth-tested; their electric glow is an additive pass that survives one layer of kelp at 40 % so the wind-up is still visible.
  5. *Steam vs hand*: bubbles are clipped at the hull radius and never draw over the right hand; a 4 mm exclusion sphere around each fingertip prevents bubble-finger z-fighting.
  6. *HUD depth gauge*: drawn at 120 mm to the player's left, depth-tested; if the left hand occludes it, it shifts 20 mm outward rather than drawing through the arm.
- **VFX budgets**: kelp 70 ribbons × 12 segments; bubbles 60; eel arcs 6 × 8 bolts; steam plume 40; surface caustics 1 shader; dodge flash 12; global cap 600 particles / 1,400 ribbon vertices.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Pole clamp | egg poles | brass clamp thunk | pod arms, steam gauge fills |
| Belly stroke | equator | wooden paddle thump, rate ∝ v_t | paddlewheel spins, water churn |
| Spin up | pod | rising steam hiss | gauge climbs, hull vibrates 1 mm |
| Neutral buoyancy | pod | soft bubble bed | gauge steady, "glide" label |
| Steam low | pod | kettle whistle, 2 Hz | gauge amber, bubbles sparse |
| Steam empty | pod | descending groan | gauge red, pod sinks |
| Tilt steer | pod | water rush, stereo pan ∝ tilt | hull banks, heading marker |
| Eel wind-up | eel | electric crackle + click | 0.35 s blue glow |
| Eel lunge | eel | zap + water snap | 40 mm arc, screen flash |
| Eel hit | pod | hull clang + alarm | hull pip breaks, red vignette |
| Dodge | pod | whoosh + close-call chime | +20, white streak |
| Kelp enter | kelp | muffled fronds | green tint, speed bar drops |
| Surface | surface | breach roar + gull | daylight bloom, tally card |

## 13. Difficulty Progression & Tuning

| Level | Depth (mm) | Eels | Eel speed (mm/s) | Lunge (mm) | Steam drain × | Kelp clumps | Detection (mm) |
|---|---|---|---|---|---|---|---|
| 1 | 1200 | 3 | 40 | 30 | 1.00 | 6 | 80 |
| 2 | 1600 | 4 | 48 | 34 | 1.05 | 7 | 85 |
| 3 | 2000 | 4 | 55 | 38 | 1.10 | 8 | 90 |
| 4 | 2400 | 5 | 62 | 40 | 1.15 | 9 | 95 |
| 5 | 2800 | 6 | 70 | 44 | 1.20 | 10 | 100 |

- **Adaptive rule** (`PADDLE_ADAPT`): after each dive, if surfaced with ≥ 2 hull and < 3 steam warnings, level +1; if failed, level −1, wind-up +0.15 s, and steam drain ×0.9. Clamp 1–5; one step per dive.
- **Named constants**: `A_EQ = 24.0 mm`, `C_POLAR = 35.0 mm`, `M_EGG = 90 g`, `OMEGA_MAX = 18 rad/s`, `K_UP = 4.0 mm/s per rad/s`, `V_UP_MAX = 60 mm/s`, `K_STEER = 240 mm/s`, `TILT_MAX = 22°`, `TAU_SPIN = 1.9 s`, `STEAM_MAX = 100`, `DRAIN = 5.5/s`, `REGEN = 5.0/s`, `SINK = 24 mm/s`, `HULL = 3`, `EEL_R = 55 mm`, `LUNGE = 220 mm/s`.

## 14. Edge Cases & Failure Recovery

- **Pinch slip**: if the pole separation drifts outside 65–78 mm for > 200 ms, the pod loses steering and holds depth for 1.5 s; re-clamping resumes without losing steam. A separation > 85 mm is a release and triggers RECOVER.
- **Egg dropped**: registration lost; pause; on re-grip, re-register and resume at the current depth with steam and hull preserved; a drop during an eel wind-up cancels the lunge.
- **One-finger pinch**: the clamp requires both pole discs; a single contact is not armed and the pod holds depth, with a "use thumb and middle" hint.
- **Over-spin**: `ω_s` caps at 18 rad/s; further strokes are absorbed and the paddlewheel visually cavitates, with a "cavitation" cue; no extra ascent.
- **Steam empty at depth**: the pod sinks at 24 mm/s but can still steer; reaching the floor costs 1 hull and bounces; a dive with 0 steam and 0 hull is a fail.
- **Eel at the surface**: eels never spawn within 150 mm of the surface, so the final ascent is a safe corridor; this prevents an unavoidable hit at the clear plane.
- **Kelp clump at the surface**: clumps end 200 mm below the surface; the last 200 mm is open water.
- **Tilt beyond 22°**: clamped; the hull banks visually but the lateral speed does not exceed 90 mm/s, preventing the pod from being slammed into the shaft wall.
- **Shaft wall**: `|x|` or `|z| > 300 mm` clamps position and zeroes lateral velocity; no hull damage (soft wall), to avoid punishing a tilt that is part of normal steering.
- **Hand tracking loss**: launcher lost → spin decays naturally; steer lost → tilt holds the last value for 400 ms then neutralises; both lost > 1.0 s pauses the dive.
- **Recovery UX**: one icon (egg / hand / both) and one line of text; no modal dialogs during a dive.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropEgg` — pose P, long axis, pole discs, belly band, fit/reacquire.
  - `PinchModel` — thumb/middle contact, separation gate, brace, slip detection.
  - `SpinModel` — `ω_s`, stroke impulses, decay, cap, paddlewheel phase.
  - `SteerModel` — tilt from `P` vs `P_0`, low-pass, lead predictor, lateral velocity.
  - `PodModel` — x, z, D, steam, hull, invulnerability, surface/floor latch.
  - `EelSystem` — lanes, phase, detection, wind-up, lunge, hit test.
  - `KelpField` — clumps, cover test, slow/drain.
  - `DiveDirector` — depth ladder, composition, scoring, adaptive level.
  - `PodRig` — hull, egg pass-through, paddlewheels, bubbles, HUD gauge.
- **Event flow**: `HandTracker → PinchModel → SpinModel → SteerModel → PodModel`; `EelSystem/KelpField → PodModel`; `PodModel → EventBus {Stroke, Glide, SteamLow, SteamEmpty, EelWindup, EelHit, Dodge, KelpEnter, Surface, FloorHit, DiveClear}` → `AudioBank / VFXPool / ScoreKeeper / HUD`.
- **Pseudocode — spin + ascent (per tick)**:

```text
dt = 0.0111
clamped = (65mm <= pole_sep <= 78mm) for >= 300ms
if stroke_contact and |phi| <= 35deg:
    n = surface_normal(stroke_point)
    vt = stroke_vel - dot(stroke_vel, n) * n
    if |vt| > 450mm/s: vt *= 450 / |vt|
    omega_s += dot(cross(n, vt), z_axis_P) / A_EQ * dt
    paddle_phase += omega_s * dt
omega_s *= exp(-dt / TAU_SPIN)
omega_s -= 0.06 * omega_s * abs(omega_s) / 18 * dt
omega_s = clamp(omega_s, -18, 18)
if omega_s > 2 and steam > 0:
    v_up = min(K_UP * omega_s, V_UP_MAX)
    steam -= DRAIN * (omega_s / 12) * dt
elif steam > 10:
    v_up = 0
    steam += REGEN * dt
else:
    v_up = -SINK
steam = clamp(steam, 0, 100)
D -= v_up * dt
```

- **Pseudocode — steering + eel hit (per tick)**:

```text
axis = lowpass(pose_P.z_axis, 4Hz)
u = normalize(axis - dot(axis, up_V) * up_V)
theta = acos(clamp(dot(axis, up_V), -1, 1))
v_lat = K_STEER * sin(min(theta, TILT_MAX)) * u
x += v_lat.x * dt; z += v_lat.z * dt
x = clamp(x, -300, 300); z = clamp(z, -300, 300)
for e in eels:
    dist = |pod_pos - e.pos|
    if dist < e.detect and e.state == PATROL:
        e.state = WINDUP; e.t_windup = now; glow(e)
    if e.state == WINDUP and now - e.t_windup >= WINDUP:
        e.target = pod_pos; e.state = LUNGE
    if e.state == LUNGE:
        e.pos += normalize(e.target - e.pos) * LUNGE_SPEED * dt
        if |pod_pos - e.pos| < EEL_R and now > invuln_until:
            hull -= 1; steam -= 15; invuln_until = now + 1.2s; e.state = RETREAT
        if e.travel > 40mm: e.state = RETREAT
if D <= 0 and hull > 0: surface(); state = SCORE
```

## 16. Acceptance Test Checklist

1. Registration fits the egg center to < 0.7 mm and the long axis to < 3° in 20/20 presentations; `P_0` snapshots within 1° of the initial vertical.
2. `r_eff` = 24.0 mm ± 0.3 mm for an equatorial stroke and 51.0 mm ± 0.6 mm for a meridional stroke.
3. A 300 mm/s equatorial stroke for 100 ms raises `ω_s` by 1.25 ± 0.10 rad/s; the cap at 18 rad/s is respected.
4. Spin decays from 12 rad/s to 4.0 rad/s in 1.9 s ± 0.15 s with no strokes (exponential plus quadratic paddle drag).
5. At `ω_s = 12`, `v_up = 48 ± 1 mm/s` and steam drains at 5.5 ± 0.3/s; at `ω_s = 0` with steam > 10, depth holds for ≥ 5 s.
6. A 22° tilt gives 90 ± 3 mm/s lateral; a 10° tilt gives 42 ± 2 mm/s; the shaft clamp stops the pod at 300 mm.
7. Eel wind-up lasts 0.35 s ± 0.02 s; a lunge travels 40 ± 2 mm at 220 ± 10 mm/s.
8. A lunge passing at 50 mm costs 1 hull, 15 steam, and grants 1.2 s of invulnerability; passing at 60 mm awards a dodge and no damage.
9. A kelp clump halves the eel detection radius while inside and drains 3 steam on entry; ascent is 75 % for 1.5 s.
10. Steam 0 forces a 24 mm/s sink and blocks ascent until steam > 8; if the pod is already in the last 150 mm, the sink is still applied and the dive fails unless the player regenerates steam first.
11. Pinch 65–78 mm arms the pod; 84 mm is a slip that holds depth after 200 ms; 86 mm is a release and pauses; re-clamping within 5 s preserves steam and hull.
12. Covering the egg for 1 s freezes depth, steam, and eels; relock resumes within 200 ms with no position jump.
13. A level-3 dive (2,000 mm, 4 eels) is clearable with ≥ 2 hull in 5/5 scripted runs under 55 s.
14. The 5-dive session completes in ≤ 9 min with ≤ 600 particles and no bubble rendered inside a fingertip's 4 mm exclusion sphere.
