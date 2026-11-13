# Game 07 — Elliptical Orbit — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Elliptical Orbit |
| Slug | `elliptical-orbit` |
| Physical prop | Plastic kiwi/egg, prolate 62 × 45 × 45 mm, 75 g, laid on its side on a flat mat |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate & Oblate Spheroids |
| Scale class | Tabletop, two-finger nudging of a free-rolling body |
| Source concept | A rogue moon rolls a curved, wobbling path around a hovering planetary core, sweeping comet tails and herding stardust |
| Source physical input | Lay the egg on its side; nudge it gently so it rolls in a curved path, then keep nudging to close an elliptical orbit |
| Session shape | 5 orbits, 18–26 s each, ≈2.5 min plus briefings |
| Primary skill | Impulse budgeting and curvature control on a self-steering rolling body |

## 1. Fantasy & Core Loop

**Fantasy.** A dead star's core hangs 120 mm above the table, leaking a ring of stardust. The egg is a rogue moon captured by its gravity. Every roll of the moon drags a tractor wake through the dust and shears the comet tails that feed the core. The player is not driving the moon; they are nudging it, correcting a body that insists on curving away.

**Core loop** (orbit 18–26 s; session 5 orbits):

1. **Place** (1.2 s) — set the egg on its side, long axis at a chosen angle α₀ to the player's forward.
2. **Launch** (0.2 s) — one nudge at the trailing end gives 130–170 mm/s and starts the loop.
3. **Read curvature** — the egg turns at `κ = 0.008 + 0.012·sin(2α) mm⁻¹`; α is the angle between the long axis and velocity.
4. **Correct** — 6 nudge tokens per orbit; a nudge at the trailing end adds speed, a nudge on the flank rotates α and changes the turn radius.
5. **Sweep** — the contact wake captures dust within 14 mm and cuts comet tails within 9 mm.
6. **Close** — after ≥ 300° of angular travel, return within 25 mm of the start point to close the orbit.
7. **Score** — closure, ellipticity, centering, dust, tails; 3 s breath; next orbit.

The core tension: the egg's natural turn radius is 50–125 mm, but a scoring orbit needs 88–118 mm. The player spends a finite nudge budget fighting the egg's own geometry while the dust ring rotates away.

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

- **Exact spheroid**: `a = b = 22.5 mm`, `c = 31.0 mm`, prolate (`c/a = 1.378`), `m = 75 g`, density 1.14 g/cm³. P origin = geometric center; `+ẑ_P` = long axis; poles at `(0, 0, ±31.0) mm`; equator = circle of radius 22.5 mm in the `x̂_P`–`ŷ_P` plane. At rest the long axis lies in the table plane, so `+ẑ_P` is horizontal.
- **Local rolling model**: `ω = (n̂ × v_t)/r_eff`, with `r_eff` from Euler's theorem at the equator: `1/r_eff = cos²ψ/(c²/a) + sin²ψ/a`, where `ψ` is the angle between `v_t` and the long axis. Thus `r_eff = 42.71 mm` rolling along the long axis and `22.50 mm` rolling across it. `n̂` is the table normal at the contact; `v_t` is the contact-point velocity in T, slip-clamped at 900 mm/s.
- **Path curvature**: measured on the real prop class, `κ(α) = 0.008 + 0.012·sin(2α) mm⁻¹` where `α` is the angle between the long axis and `v`. Range: `R = 50 mm` at `α = +45°`, `R = 125 mm` at `α = 0°`, and `R = −250 mm` (opposite turn) at `α = −45°`. The 0.008 mm⁻¹ bias comes from the egg's seam and off-axis centre of mass; it is a prop-class constant, not measured per session.
- **V frame**: anchored to **T**. Origin = the table point directly below the core; `+ŷ_T` = table normal; `+x̂_T` = player forward projected on the table; `+ẑ_T = x̂_T × ŷ_T`. The egg's live pose `P` is projected to T for contact and path metrics.
- **Deviations**: (a) the contact is a 0.60 × 0.40 mm Hertzian ellipse (diameters), not a point, so a 0.002 mm² scrub term is added to rolling drag; (b) the egg's real long axis is not a painted feature, so `+ẑ_P` is fitted from the silhouette each 200 ms and blended, not integrated; (c) the core, dust, and comet tails are T-anchored and unaffected by table vibration; (d) the game does not simulate gravity on the egg — the table is flat and the egg is driven only by nudges and rolling resistance.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated; a 400 × 400 mm low-friction mat centred 280 mm in front of the sternum; forearms on the table.
- **Play field**: the mat, radius 180 mm; scoring orbits use r ∈ [70, 150] mm, so the egg never leaves the mat at nominal speed.
- **Prop placement**: egg on its side at the field's near edge (r ≈ 150 mm), long axis at α₀ ∈ [−30°, +30°] to the player's forward.
- **Hand roles**: dominant hand = nudger (index + middle); non-dominant hand = guide/brake, resting at the mat edge.
- **Reach envelope**: fingertip travel ≤ 160 mm from the near edge; the player does not chase the egg around the table — a 300 mm slide of the palm is allowed once per orbit.
- **Forces**: nudge impulse 1.5–4.5 mN·s over 60–160 ms (peak 25–90 mN); light enough to repeat 6× per 20 s without fatigue.
- **Session**: 5 orbits, hard stop 5 min; 25 s rest between orbits.
- **Visual ergonomics**: the core floats at 120 mm, requiring a 20° downward gaze; the dust ring is drawn at 2–6 mm above the table so it reads as a flat map.
- **Safety**: the egg is 75 g and ≤ 170 mm/s; if it leaves the mat, it is caught by the scene mesh or rolls < 200 mm on the table, with no fall risk at tabletop height.

## 4. Prop Geometry & Registration

- **Real dimensions**: 62 mm long, 45 mm wide, 45 mm high, 75 g; tolerance ±0.8 mm length, ±0.5 mm width, ±1.0 g. Model volume `(4/3)πa²c = 65,745 mm³`.
- **Inertia**: `I_z = (2/5)ma² = 1.52 × 10⁻⁵ kg·m²` about the long axis; `I_x = I_y = (1/5)m(a² + c²) = 2.20 × 10⁻⁵ kg·m²` transverse. Used for the yaw kick from flank nudges.
- **Surface atlas**: 36 meridians (10°) × 12 latitude bands (15°); the equator band (`|φ| ≤ 7.5°`) is the "rolling stripe" and carries a virtual seam highlight so the player can see the long axis.
- **Colliders**: `COL_egg` = the spheroid; `COL_table` = plane at `y_T = 0`; `COL_core` = sphere r = 22 mm at `(0, 120, 0)` mm (visual only, no hand collision); `COL_mat` = 360 mm disc used to detect out-of-bounds.
- **Registration**: (1) the egg is laid on its side and held still for 0.8 s; (2) the system fits the spheroid to the silhouette over 15 frames, accepting center error < 0.7 mm and axis error < 4°; (3) the long axis is disambiguated by the egg's taper (the pointed end is `+ẑ_P`), resolved from a 2 s slow rotation the player performs during calibration; (4) `P` publishes at 72 Hz with a 1-frame prediction to 90 Hz.
- **Re-acquisition**: search a 90 mm sphere around the last pose; refit the silhouette; re-derive the long axis from the taper. If the egg is end-on (axis within 15° of the view ray), hold the last axis and show a "turn the egg" hint; end-on silhouettes are not fitted.
- **Tolerances**: a 0.8 mm length error changes `r_eff` by 1.3 %, which is below the 5 % gameplay tolerance; no per-session calibration of `κ` is performed.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Two-hand settle | `L_index_tip` + `R_index_tip` opposed on flanks | `COL_egg` | registration hold, no impulse |
| I5.2 | Prograde nudge | `R_index_tip` on trailing pole cap | `COL_egg`, `|φ| > 60°` | `+Δv` along the long axis, cost 1 token |
| I5.3 | Flank nudge | `R_index_tip` on the equator flank | `COL_egg`, `|φ| < 30°` | `+Δv` lateral, yaw kick `Δα`, cost 1 token |
| I5.4 | Brake | `R_middle_tip` pressed on the top of the shell | `COL_egg` | +0.02 friction, no token |
| I5.5 | Cup / stop | open palm 30 mm above the egg | none | freeze the egg for 1.0 s |
| I5.6 | Guide | light contact < 0.3 mN·s | `COL_egg` | no impulse, draws the predicted arc |
| I5.7 | Re-seat | lift egg and place on side | `COL_table` | re-zero orbit, keep score |
| I5.8 | Reset orbit | `L_thumb_tip` + `L_index_tip` pinch 1.2 s | — | abandon orbit, no penalty |

Per-finger roles: dominant index = all nudges; dominant middle = brake; non-dominant index = guide during settle; all others ignored. Per-hand roles: dominant = DRIVE, non-dominant = SETTLE/BRAKE. A nudge during a brake is rejected.

## 6. Input Kinematics & Signal Processing

- **State**: position `p_T`, velocity `v_T`, long-axis angle `α` (in the table plane), spin `ω_s` about the long axis, nudge tokens `N ∈ {0..6}`.
- **Speed cap**: `|v| ≤ 190 mm/s`; impulses that would exceed it are scaled. Rationale: above 190 mm/s the egg crosses the 180 mm mat in < 1 s and tracking of the silhouette degrades.
- **Rolling constraint**: `v = ω_s × r_eff` in magnitude, so `ω_s = |v|/r_eff`; the renderer uses this to spin the seam highlight. `r_eff` is evaluated from `ψ = α − atan2(v_z, v_x)`.
- **Nudge impulse**: `J = ∫F dt` accepted for 1.5–4.5 mN·s and 60–160 ms. `Δv = J/m` (20–60 mm/s). A prograde nudge adds `Δv` along `+ẑ_P`; a flank nudge adds `Δv` along the contact normal and a yaw kick `Δα = (r_c × J)_y·τ_c/I_x` with `τ_c = 0.1 s`, capped at ±0.35 rad.
- **Curvature integration**: `dα/ds = −κ(α)·...` is not integrated directly; instead `α` is a state variable driven by the yaw kick and by the geometric alignment torque `α̇ = −0.35·sin(2α)` (the body tends to roll along its long axis). Path curvature is then `κ_path = κ(α)`.
- **Path integration**: heading `β̇ = v·κ_path`; `p ← p + v·dt`; `v ← v·e^(−dt/τ_v)` plus `−v̂·(µ_r g + 0.06·|κ|·v²)dt`, with `τ_v = 0.9 s` (air/seam wobble), `µ_r = 0.0035`, `g = 9810 mm/s²`.
- **Wobble**: `α_w = 5.5°·e^(−t/2.8)·sin(2π·1.45·t)` added to the rendered axis and to the contact width; a lateral force `F_w = 0.4 mN·sin(2π·1.45·t)` perturbs the path by ±1.5 mm.
- **Orbit tracking**: accumulate unwrapped polar angle `Φ` around the core; orbit is "closed" when `Φ ≥ 300°` and `|p − p_start| < 25 mm`.
- **Signal chain**: joints → 3-tap median → one-euro filter (1.0 Hz min cutoff, β = 0.015) → impulse integration. The egg pose is filtered more heavily (2.5 Hz low-pass) because silhouette fitting is noisier than hand tracking.
- **Latency**: 14 ms tracking + 11 ms physics + 11 ms render + 20 ms audio = 56 ms; a nudge is a feed-forward impulse, so latency does not affect aim.

## 7. Virtual World Layout & Coordinate Mapping

- **T axes**: origin under the core; `+ŷ_T` = table normal; `+x̂_T` = player forward; `+ẑ_T` = player's right. Units mm.
- **Core**: sphere r = 22 mm at `(0, 120, 0) mm`; emissive amber-white; a 3 mm corona; a 60 mm gravity-lens ring drawn at `y_T = 1 mm`.
- **Dust ring**: 8 arcs at radii 72, 84, 96, 108, 118, 128, 138, 148 mm, each 40–70° long, rotating at the local Keplerian rate `Ω_K = sqrt(µ/r³)` with `µ = 3.2 × 10⁵ mm³/s²` (at r = 100 mm, `Ω_K = 0.566 rad/s`, period 11.1 s).
- **Comet tails**: 4 radial ribbons from r = 58 to r = 132 mm at azimuths 0°, 95°, 190°, 285°, drifting at 0.05 rad/s; each is 40 particles.
- **Orbit criterion**: scoring ellipse has semi-major 118 mm, semi-minor 88 mm, rotated 20°; the player's actual loop is scored on its own `r_min/r_max` and centroid, not on matching this template.
- **Mapping**: `p_T = (r cosΦ, 0, r sinΦ)`; dust at `y_T = 2 mm`; tails at `y_T = 4 mm`; core at 120 mm. The egg's contact footprint is `(p_x, 0, p_z)`.
- **Capture**: dust within 14 mm of the contact point is captured; a tail is cut when the contact crosses its spline within 9 mm and the crossing is transverse (angle > 30°).

## 8. Gameplay Systems & Rules

- **Entities**: `Egg` (p, v, α, ω_s), `DustField` (8 arcs, 400 particles), `CometTail` (4), `Core`, `OrbitJudge`, `NudgeBudget`.
- **Dust capture**: each captured particle adds +2; the particle then spirals to the core over 0.8 s and adds a +4 "fed core" bonus. Target 60 particles per orbit.
- **Tail cut**: +80 and all 40 tail particles become capturable in a 0.6 s burst; a tail can be cut once per orbit.
- **Closure**: +300. **Ellipticity** `e = (r_max − r_min)/(r_max + r_min)`: in [0.22, 0.48] → +200, else `+200·(1 − |e − 0.35|/0.35)` clamped at 0.
- **Centering**: `|centroid| < 18 mm` → +150, linearly to 0 at 40 mm.
- **Energy**: 6 nudge tokens; a token is refunded only on a closed orbit with `e` in band (+1, max 6). If `|v| < 25 mm/s` and no nudge is spent within 1.2 s, the orbit fails (stall) but dust already captured is kept.
- **Orbit timer**: 26 s soft cap; after 20 s the HUD ring turns amber; at 26 s the orbit is scored as-is (no closure bonus).
- **Scoring**: dust +2 each, fed +4, tail +80, closure +300, ellipticity +200, centering +150; flawless orbit (closure + e in band + ≥ 3 tails) +500.
- **Win/lose**: session = 5 orbits; win if total ≥ 4,000; no hard fail, but a stalled orbit counts as 0 closure.
- **Core health**: cosmetic only — the core brightens with dust fed and dims if an orbit ends with < 20 particles; never ends the session.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, sensor test | REGISTER |
| REGISTER | egg on side, still 0.8 s | center error < 0.7 mm | fit P, set α₀ | CALIBRATE |
| CALIBRATE | slow rotation 2 s | silhouette valid 80 % | resolve taper, set `+ẑ_P` | BRIEF |
| BRIEF | 2.5 s card | — | orbit 1 seed | AIM |
| AIM | guide contact | < 0.3 mN·s | draw predicted arc | AIM |
| AIM | prograde nudge | token ≥ 1 | launch, `v = Δv` | ROLL |
| ROLL | flank nudge | token ≥ 1 | Δv + yaw kick | ROLL |
| ROLL | brake | — | friction +0.02 | ROLL |
| ROLL | dust captured | r_contact < 14 mm | +2, feed animation | ROLL |
| ROLL | tail crossed | transverse, < 9 mm | +80, burst | ROLL |
| ROLL | `|v| < 25` for 1.2 s | no nudge | stall | SCORE |
| ROLL | `Φ ≥ 300°` and close | dist < 25 mm | closure +300 | SCORE |
| ROLL | 26 s elapsed | — | score as-is | SCORE |
| ROLL | egg off mat | r > 180 mm | freeze, out-of-bounds | RECOVER |
| RECOVER | egg returned in 10 s | on mat | resume, keep orbit | ROLL |
| RECOVER | 10 s timeout | — | orbit fails | SCORE |
| SCORE | orbit < 5 | — | tally, 25 s rest | BRIEF |
| SCORE | orbit = 5 | — | session stats | COMPLETE |
| any | prop lost > 200 ms | — | freeze dust, ghost egg | REACQUIRE |
| REACQUIRE | relock ≤ 2.0 s | search 90 mm | hold α, resume | prior |
| REACQUIRE | 2.0 s timeout | — | "present the egg" card | REACQUIRE |
| any | cup gesture 1.0 s | — | freeze | SUSPENDED |
| SUSPENDED | cup released | — | 1.0 s countdown | prior |

Timers: settle 0.8 s; calibration rotation 2.0 s; stall 1.2 s; orbit cap 26 s; rest 25 s; reacquire 2.0 s; cup 1.0 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_egg` (spheroid), `COL_table` (plane), `COL_mat` (180 mm disc), 400 dust points (r = 1.2 mm), 4 tail splines, 1 core sphere.
- **Pair matrix**: Finger↔Egg (impulse, no penetration); Egg↔Table (rolling constraint, no slip); Egg↔Dust (capture event, no force); Egg↔Tail (cut event); Egg↔MatEdge (out-of-bounds event); Dust↔Core (feed event); Dust↔Dust (ignore).
- **Rolling model**: semi-implicit Euler, 90 Hz, 2 substeps when `|v| > 140 mm/s`. `v ← v·e^(−dt/τ_v) − v̂·(a_r + a_s)dt`, `a_r = µ_r g = 34.3 mm/s²`, `a_s = 0.06·|κ|·v²` (scrub, 8.6 mm/s² at κ = 0.01, v = 120).
- **CCD**: max speed 190 mm/s → 2.11 mm/substep; dust capture radius 14 mm and tail radius 9 mm are larger than the step, so no CCD is needed for capture. The mat edge uses a swept point-in-disc test.
- **Materials**: egg↔mat µ_r = 0.0035, restitution 0 (rolling, no bounce); egg↔skin µ = 0.45; mat↔table static friction high (the mat does not slide).
- **Forces**: nudge impulses only; wobble lateral force 0.4 mN; no gravity in the T plane (the table is level); vertical normal force 0.736 N.
- **Contact patch**: Hertzian ellipse 0.60 × 0.40 mm (`F = 0.736 N`, `E* = 2.28 GPa`, `R_eff = 22.5 mm`); the patch centre is the capture point, not the spheroid centre.
- **Core**: no physical effect on the egg; `COL_core` exists only to stop the hand from visually intersecting the hologram, and is disabled for fingertips.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → mat grid → dust/tails → egg ghost axis → core → capture VFX → hands → HUD.
- **Occlusion cases**:
  1. *Egg over dust*: the real egg depth-occludes dust behind it; dust in front (r < egg silhouette) is drawn at `y_T = 6 mm` so it reads as passing over the shell.
  2. *Nudging fingertip*: the tip covers 8–12 mm of the shell; the predicted-arc ribbon is drawn at `y_T = 8 mm` above the finger so the player can still see the aim line.
  3. *Core behind the hand*: the core is 120 mm up; a hand crossing it is depth-tested, and the core's corona gets a 2 mm outline on the hand silhouette to keep its position readable.
  4. *Egg under the palm*: when the palm cups the egg (I5.5), the egg is fully hidden; a wireframe ghost is drawn through the hand at 25 % opacity.
  5. *Mat edge*: dust and tails are clipped at r = 180 mm with a 6 mm alpha falloff; no hard edge on the real mat.
  6. *Egg silhouette vs hologram*: the ghost axis is a 1.5 mm tube drawn only outside the real silhouette (analytic ellipse test) to avoid z-fighting.
- **VFX budgets**: dust 400 points; tails 4 × 40 = 160; capture sparkle 6 per particle (pooled); tail burst 60; core corona 1 shader; predicted arc 24 segments; global cap 700 points / 1,200 ribbon vertices.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Place / settle | egg | soft wooden settle | seam highlight calibrates |
| Nudge | contact point | felt mallet tap, pitch ∝ J | arc line flashes, token pip dims |
| Rolling | egg contact | low granular rumble, rate ∝ v | dust lifts in the wake |
| Curve change | egg | subtle whoosh bend | predicted arc redraws |
| Dust captured | contact point | glass bead tick | +2 float, particle spirals up |
| Tail cut | tail spline | ribbon shear + choir | 40-particle burst, +80 |
| Core fed | core | warm sub-bass swell | core brightens 8 % |
| Orbit closed | core | rising major chord | ring closes, +300 |
| Stall | egg | deflating sigh | arc collapses, token HUD blinks |
| Orbit scored | HUD | paper stamp | tally card |
| Session win | room | long shimmer | core supernova bloom |

## 13. Difficulty Progression & Tuning

| Level | Nudge tokens | Dust particles | Tails | Friction µ_r | Orbit cap (s) | Target e band | Tail drift (°/s) |
|---|---|---|---|---|---|---|---|
| 1 | 8 | 50 | 3 | 0.0028 | 30 | 0.20–0.50 | 2.0 |
| 2 | 7 | 55 | 4 | 0.0031 | 28 | 0.22–0.48 | 2.5 |
| 3 | 6 | 60 | 4 | 0.0035 | 26 | 0.24–0.46 | 3.0 |
| 4 | 6 | 64 | 5 | 0.0039 | 24 | 0.26–0.44 | 3.5 |
| 5 | 5 | 68 | 5 | 0.0043 | 22 | 0.28–0.42 | 4.0 |

- **Adaptive rule** (`ORBIT_ADAPT`): after each orbit, if closure achieved with ≥ 50 % dust and e in band, level +1; if the orbit stalled or missed closure, level −1 and the cap +2 s. Clamp 1–5; one step per orbit.
- **Named constants**: `A_EQ = 22.5 mm`, `C_POLAR = 31.0 mm`, `M_EGG = 75 g`, `MU_R = 0.0035`, `KAPPA_BIAS = 0.008 mm⁻¹`, `KAPPA_SHAPE = 0.012 mm⁻¹`, `V_MAX = 190 mm/s`, `NUDGE_J = 1.5–4.5 mN·s`, `CAPTURE_R = 14 mm`, `TAIL_R = 9 mm`, `MU_GRAV = 3.2e5 mm³/s²`, `ORBIT_ARC = 300°`, `CLOSE_R = 25 mm`.

## 14. Edge Cases & Failure Recovery

- **Egg rolls off the mat**: freeze the orbit, mark OUT_OF_BOUNDS; if the egg returns within 10 s, resume from the last pose with the same Φ; otherwise score as-is. No token refund.
- **Egg hits the mat edge and stops**: treated as a stall after 1.2 s; the HUD shows the remaining tokens.
- **Egg end-on to the camera**: silhouette fit fails; hold the last long axis, extrapolate position with the last velocity for up to 400 ms, then freeze and prompt a turn.
- **Egg lifted mid-roll**: contact lost for > 120 ms → freeze; on re-placement, resume only if within 40 mm of the last pose, else restart the orbit with tokens preserved.
- **Hand tracking loss**: if the drive hand is lost, the egg keeps rolling and decaying (no nudges); if the guide hand is lost, nothing changes.
- **Nudge double-count**: two contacts within 40 ms on the same joint merge into one impulse; a prograde and a flank nudge within 60 ms are both accepted but the second is scaled 0.6×.
- **Brake + nudge conflict**: a nudge while the middle finger is braking is rejected and the token is not spent; a 1-line hint says "release the brake".
- **Speed clamp**: a 4.5 mN·s nudge at 170 mm/s would exceed 190; the impulse is scaled to the remaining headroom and the token is still spent (the player sees a "capped" flash).
- **Token exhaustion**: at 0 tokens the egg coasts; if it stalls, the orbit fails with dust kept. The HUD never blocks the view.
- **Mat slip**: if the mat moves > 10 mm (plane-fit residual), re-anchor T and keep relative positions; the core and orbit metrics shift with it.
- **Recovery UX**: one icon (egg / hand / mat) and one line of text; no modal dialogs mid-orbit.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropEgg` — pose P, long axis, taper sign, silhouette fitter, contact patch.
  - `RollModel` — p, v, α, ω_s, `r_eff(ψ)`, curvature `κ(α)`, friction and scrub.
  - `NudgeClassifier` — impulse, duration, contact zone (pole/flank), token accounting.
  - `OrbitTracker` — unwrapped Φ, r_min/r_max, centroid, closure test.
  - `DustSystem` — 8 arcs, Keplerian rotation, capture query.
  - `TailSystem` — 4 splines, transverse-crossing test, burst.
  - `OrbitJudge` — scoring, ellipticity, centering, adaptive level.
  - `CoreRig` — core shader, corona, feed animation.
- **Event flow**: `HandTracker → NudgeClassifier → RollModel → OrbitTracker`; `RollModel → DustSystem/TailSystem`; `OrbitJudge` consumes `OrbitTracker` + capture counts; `EventBus {Nudge, DustCaptured, TailCut, CoreFed, OrbitClosed, Stall, OrbitScored}` → `AudioBank / VFXPool / ScoreKeeper / HUD`.
- **Pseudocode — roll + curvature (per tick)**:

```text
dt = 0.0111
for n in nudges where not n.consumed:
    if 1.5mN*s <= n.J <= 4.5mN*s and 60ms <= n.dur <= 160ms:
        dv = n.J / M_EGG
        v += n.dir * min(dv, V_MAX - |v|)
        if n.zone == FLANK:
            alpha += clamp((n.r_cross * n.J) / I_X * 0.1, -0.35, 0.35)
        tokens -= 1; n.consumed = true
psi = alpha - atan2(v.z, v.x)
r_eff = 1 / (cos(psi)^2 / (C*C/A) + sin(psi)^2 / A)
omega_s = |v| / max(r_eff, 1)
alpha += -0.35 * sin(2*alpha) * dt          # axis tends to align with rolling
kappa = KAPPA_BIAS + KAPPA_SHAPE * sin(2*alpha)
kappa = clamp(kappa, -0.02, 0.02)
heading += |v| * kappa * dt
v *= exp(-dt / TAU_V)
v -= normalize(v) * (MU_R * G + 0.06 * abs(kappa) * |v|^2) * dt
p += v * dt
```

- **Pseudocode — capture + orbit scoring (per tick)**:

```text
contact = p + r_eff * normalize(cross(up, normalize(v)))   # patch centre
for d in dust where not d.captured:
    if |d.pos - contact| < CAPTURE_R:
        d.captured = true; captured += 1; score += 2
for t in tails where not t.cut:
    if distance_point_to_spline(contact, t) < TAIL_R:
        if angle_between(v, t.tangent) > 30deg:
            t.cut = true; cut += 1; score += 80
Phi += signed_angle(p_prev, p, core) 
r = |p - core_xz|
r_min = min(r_min, r); r_max = max(r_max, r)
if Phi >= 300deg and |p - p_start| < CLOSE_R:
    e = (r_max - r_min) / (r_max + r_min)
    score += 300 + ellipticity_bonus(e) + centering_bonus(centroid)
    state = SCORE
```

## 16. Acceptance Test Checklist

1. Registration fits the spheroid center to < 0.7 mm and the long axis to < 4° in 20/20 presentations; the taper sign is correct in 10/10.
2. `r_eff` = 42.71 mm ± 0.5 mm at ψ = 0° and 22.50 mm ± 0.3 mm at ψ = 90°.
3. A 3.0 mN·s prograde nudge changes speed by 40 ± 2 mm/s on a 75 g egg and spends exactly 1 token.
4. Rolling with no nudges from 160 mm/s travels 295 ± 25 mm before stopping (µ_r = 0.0035 plus scrub at κ ≈ 0.01).
5. A path with α = +45° has radius 50 ± 5 mm; with α = 0° it has radius 125 ± 8 mm; with α = −45° the turn sign reverses.
6. A flank nudge of 3.0 mN·s at a 20 mm lever changes α by 0.27 ± 0.03 rad.
7. Dust within 14.0 mm of the contact patch is captured on the next tick; dust at 15.5 mm is not.
8. A transverse crossing of a tail spline within 9 mm cuts it; a tangential crossing at < 30° does not.
9. An orbit with 300° of unwrapped angle and a 24 mm return distance scores the +300 closure bonus; a 27 mm return does not.
10. Ellipticity e = 0.35 scores +200 ± 5; e = 0.10 scores < 60; e = 0.60 scores < 60.
11. A stalled egg (< 25 mm/s for 1.2 s) ends the orbit with dust retained and no closure bonus.
12. Covering the egg for 1 s freezes dust and preserves Φ, tokens, and score; relock resumes within 200 ms.
13. A level-3 orbit (60 particles, 4 tails, 6 tokens) is completable in 5/5 scripted runs with closure and e in band under 26 s.
14. The full 5-orbit session completes in ≤ 5 min with the 700-point particle cap never exceeded.
