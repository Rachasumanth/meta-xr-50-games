# Pulse Core Reactor

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Pulse Core Reactor |
| Slug | `pulse_core_reactor` |
| Physical prop | Polyurethane foam stress ball (default), true sphere, r = 32.0 mm (64 mm diameter), 60 g, E ≈ 60 kPa; tennis ball alternate r = 33.5 mm, 58 g |
| Family / sub-shape | True sphere, palm-scale (6–12 cm class), compliant compression |
| Scale class | Prop 1:1; shield radius 40–160 mm around the fist; threat envelope 900 mm, rendered at tabletop comfort scale |
| Source concept quote | "Squeeze the stress ball in one hand. The harder you close your fist, the wider the plasma shield around it blooms; release and it collapses into a dense armour shell. Distant snipers need the wide shield, close bombs need the dense one, and your grip is the only dial between them." |
| Physical input | Single-hand power grip; palm-to-pad aperture drives compression 0–6 mm; off-hand double-tap purges heat |
| Session length | 7 min: onboarding 40 s + 5 waves (≈55 s each) + 4 rests (10 s) + results |
| Lose condition | Core integrity reaches 0 HP |

## 1. Fantasy & Core Loop

Your right hand is the reactor. The stress ball crushed inside it is a biomechanical pump, and every newton you close into the foam is converted into a plasma shield wrapped around your fist. Squeeze until the knuckles whiten and the shield blooms to 160 mm, wide enough to swallow a sniper beam; let go and the field implodes into a 40 mm shell denser than lead, the only thing that survives a contact bomb. The arena is built around the fist: beams spear in from 900 mm, bombs tumble in from 200 mm, and the pump's heat gauge is the sound of your own effort catching up with you.

1. **CHARGE (0.10–0.35 s).** The hand closes; aperture falls from 64 mm toward 52 mm; penetration `δ` rises 0–6 mm; the shield radius tracks `40 + 120·p` mm with a 13 ms lag.
2. **HOLD (0.2–1.4 s).** The player matches the announced threat: wide for a beam, dense for a bomb. A hold arc shows the usable window.
3. **RESOLVE (0.15 s).** The beam arrives or the bomb fuse ends; the shield test is evaluated once, at the exact timestamp, using the current radius or density.
4. **RELEASE (0.2–0.5 s).** The hand opens below `p = 0.25`; the shield contracts and heat begins to vent. A valid press-hold-release triple is one pump cycle.
5. **VENT (as needed).** Off-hand double-tap on the exposed lateral face dumps 35 heat with a 20 s cooldown; the pump hisses and the shield flickers.

A threat pair (one beam, one bomb) resolves in 6–9 s; a wave of 8–12 threats lasts 40–70 s. The full loop from charge to release is 0.8–1.6 s, which is the natural frequency of a comfortable power grip.

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

`P` origin is the undeformed sphere centre; poles are `±ẑ_P`, aligned with the grip axis (palm plate at `−ẑ_P`, finger pads at `+ẑ_P`); radius `r = 32.0 mm`. The rolling law `ω = (n × v_t)/r` is evaluated for every contact and used for two things only: rejecting accidental ball spin and driving the cosmetic coolant-impeller swirl. `V` is anchored to `P` with a 25 ms critically damped follow, so the shield is always centred on the fist. Deviations:

1. **Compliance is the control channel.** The tracked sphere is the *undeformed reference*; pad penetration up to 6 mm is measured against it and becomes `p`. No rigid-body rotation command is accepted.
2. **Radius, not rotation.** Shield radius `R_s = 40 + 120·p` mm; density `D = 1.00 − 0.60·p`. The two threat classes require opposite ends of the same axis.
3. **Deformation-aware registration.** Because the ball visibly squashes, the tracker fits an ellipsoid and reports the undeformed radius from the major-axis average; re-acquisition uses the relaxed silhouette only.
4. **No prop physics.** The real foam is never simulated; only shield radius, heat, beams, and bombs are. A dropped ball triggers a prediction cue, not a virtual bounce.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated or standing; elbow flexed 85–100°, forearm supported | Grip force is highest near mid-range elbow flexion |
| Working hand | Dominant hand at sternum height, 250–350 mm from the chest | Keeps the shield in the central visual field |
| Off-hand | Relaxed at waist or braced on the table | Reserved for the double-tap vent |
| Grip aperture | 64 mm rest → 52 mm maximum squeeze | 12 mm of hand closure, within a comfortable power-grip range |
| Grip force | 0–17 N total (0–8.3 N per pad) | Below the 25 N sustained-grip comfort limit |
| Session | 7 min; 10 s rests between waves | Forearm flexor fatigue is the binding constraint |
| Duty cycle | ≤ 55% of each wave under load; vent phases enforce release | Prevents static grip ischemia |

The ball is held with the palm as the reaction plate and the four finger pads opposing it. The thumb wraps the side and contributes 25% of the force; it never opposes the fingers across the poles, which would create a shear that the deformation model cannot resolve. The wrist stays neutral; the shield visuals are rendered around the fist so the player does not need to aim the hand. A grip-load arc appears at the edge of the HUD after 35 s of cumulative squeeze and turns amber at 50 s, prompting the 10 s rest.

Because the foam is soft, the fingers sink into it and the skeletal tracker loses the distal pads. The system therefore fuses two estimates: direct tip-to-palm distance when ≥ 3 tips are visible, and a kinematic grip model driven by `mcp`/`pip` angles when they are not. The fusion is described in §6.

## 4. Prop Geometry & Registration

| Property | Stress ball (default) | Tennis ball |
|---|---|---|
| Diameter | 64.0 mm | 67.0 mm |
| Radius `r` | 32.0 mm | 33.5 mm |
| Mass | 60 g | 58 g |
| Inertia `I = 2/5·m·r²` | 2.46 × 10⁻⁵ kg·m² | 2.60 × 10⁻⁵ kg·m² |
| Young's modulus `E` | 60 kPa | 450 kPa (shell) |
| Poisson ratio `ν` | 0.45 | 0.30 |
| Effective modulus `E* = E/(1−ν²)` | 75.2 kPa | 495 kPa |
| Max pad penetration | 6.0 mm | 6.0 mm |
| Force at max `δ` | 8.3 N per pad | 56.1 N per pad |

The stress ball is the intended prop: the tennis ball reaches 56 N per pad at 6 mm, which exceeds the comfortable one-hand grip and would make the top of the shield unreachable for most players. The game caps the measurable penetration at 6 mm and treats any deeper squash as `p = 1` with an over-pump warning.

**Sphere parameterization and grip zones.** The undeformed surface follows `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`. The palm plate contacts the south cap `φ ∈ [−90°, −55°]`; the four finger pads contact the north cap `φ ∈ [+55°, +90°]`; the thumb contacts the side band `φ ∈ [−20°, +35°], λ ∈ [70°, 110°]` (right hand). The equator is a free bulge zone and carries the heat-glow shader.

**Surface-feature atlas (P-fixed):**

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| Palm plate | −90° to −55° | all | Reaction surface; no command |
| Finger plate | +55° to +90° | all | Primary compression input |
| Thumb pad | −20° to +35° | 70°–110° | Secondary force, 25% weight |
| Equator bulge | −10° to +10° | all | Heat glow, deformation readout |
| Vent port | +20° to +40° | 250°–290° | Off-hand double-tap target |
| Coolant impeller | +25° | 270° | Cosmetic swirl driven by `ω` |
| Seam reference | 0° | 0° | Orientation ruler |

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere (undeformed) | r = 32 mm at `P` | Reference for penetration |
| C1 | Ellipsoid (deformed) | semi-axes `(r−δ, r+0.35δ, r−δ)` | Visual/anchor only, not a collider |
| C2 | Sphere | `R_s` = 40–160 mm at `P` | Plasma shield |
| C3 | Capsule | r = 1.8 mm, length 12 mm, per fingertip | Contact/penetration |
| C4 | Plane pad | 22 × 30 mm at palm | Reaction plate |
| C5 | Sphere | r = 4 mm | Bomb |
| C6 | Cylinder | length 900 mm, radius `w_b/2` | Sniper beam |

Registration: present the relaxed ball at 0.9–1.3 m, rotate through 120° for 1.0 s, hold 0.3 s. Acceptance: RMS < 0.5 mm, radius error < 0.25 mm, ≥ 20 rim samples. The fitter rejects frames where the ball is visibly squeezed (major/minor axis ratio < 0.90) because the undeformed radius would be biased low.

Re-acquisition during a squeeze: the tracker searches a 120 mm cube for 1.2 s, fits the *relaxed* silhouette from the previous frame's undeformed radius, and snaps with a 120 ms ease. If the hand is fully occluding the ball, the shield remains at its last radius for 400 ms and then decays toward 40 mm at 120 mm/s; the game does not pause.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Power grip | `R_thumb_tip`, `R_index_tip`, `R_middle_tip`, `R_ring_tip`, `R_pinky_tip`, palm | fingers φ ≥ +55°, palm φ ≤ −55°, thumb λ 70–110° | Aperture `a`, penetration `δ`, `p` |
| I5.2 | Full crush | all five tips + palm, ≥ 5 valid contacts | same | `δ` uses the weighted mean; max `p = 1` |
| I5.3 | Tripod squeeze | thumb + index + middle + palm, ≥ 4 contacts | north cap + thumb band | 0.8× force weight; used for fine control |
| I5.4 | Release | ≤ 1 finger contact or aperture > 60 mm | — | `p` decays, shield contracts |
| I5.5 | Double-tap vent | `L_index_tip`, twice within 320 ms | vent port φ +20–40°, λ 250–290° | `VENT`: −35 heat, 20 s cooldown |
| I5.6 | Off-hand brace | `L_palm` on table or own forearm | no prop contact | Stability cue only |
| I5.7 | Swirl | `L_index_tip` tangential drag on equator | −10° to +10°, any λ | Cosmetic impeller spin from `ω` |
| I5.8 | Drop | all `R_*` prop contacts lost > 150 ms | — | `DROPPED`, shield decays |
| I5.9 | Over-squeeze | `δ > 5.6 mm` sustained 1.5 s | finger plate | Over-pump damage, warning |

**Per-finger force weights** (sum 1.00): index 0.30, middle 0.30, thumb 0.25, ring 0.10, pinky 0.05. The weighted penetration is `δ = Σ w_i·δ_i / Σ w_i` over valid contacts. The palm plate does not contribute to `δ`; it is the zero reference. The ring and pinky are structurally weak in a power grip, so their low weight prevents a single noisy joint from spiking the shield.

Contact gating: a finger contact contributes only if distance < 5 mm, age > 30 ms, and the contact normal has `n·ẑ_P > 0.30`. The palm contact must satisfy `n·ẑ_P < −0.30`. Thumb contacts outside the 70–110° longitude band are treated as side bracing and ignored. Classifiers emit at most one event per 90 Hz tick and debounce at 30 ms.

## 6. Input Kinematics & Signal Processing

The control variable is the grip aperture `a`. The palm reference point is `h_palm = mean(R_index_mcp, R_middle_mcp, R_ring_mcp) + 12 mm·n̂_palm`, where `n̂_palm` is the palm-plane normal. The pad reference is `h_pads = mean(R_index_tip, R_middle_tip, R_ring_tip, R_pinky_tip)` projected onto `n̂_palm`. Then:

`a = |(h_pads − h_palm)·n̂_palm|`, `δ = clamp((2r − a)/2, 0, 6 mm)`, `p = δ/6 mm`.

The factor 1/2 is geometric: the two opposing contacts each indent the sphere by `δ`, so the hand closes by `2δ`. At rest `a = 64 mm`, `δ = 0`; at maximum squeeze `a = 52 mm`, `δ = 6 mm`, `p = 1`.

**Force and pressure model.** Each pad is a Hertzian flat-on-sphere contact with `F = (4/3)·E*·√r·δ^{3/2}`. For the stress ball, `k_p = (4/3)·75.2 kPa·√32 mm = 0.567 N/mm^{1.5}` (17.94 kN/m^{1.5}). At `δ = 3 mm`, `F = 2.95 N` per pad; at `δ = 6 mm`, `F = 8.34 N` per pad. Total grip force is twice the per-pad force because the palm reacts: 5.9 N at mid-squeeze, 16.7 N at full. Contact radius `a_c = √(r·δ)` is 9.8 mm at 3 mm and 13.9 mm at 6 mm; mean contact pressure is `F/(π·a_c²)` = 9.8 kPa and 13.8 kPa respectively; peak Hertz pressure is 1.5× mean, 14.7 kPa and 20.7 kPa.

These are the numbers used for the "grip effort" HUD and for audio loudness; they are not used as a damage input. The player never has to hit an absolute force, only a normalized squeeze, which keeps the game playable on a stiffer tennis ball.

**Signal chain.** Raw joints at 72 Hz → occlusion-aware fusion (if ≥ 3 tips visible, weight 0.75 direct / 0.25 kinematic; if < 3, weight 0.25 direct / 0.75 kinematic) → One Euro filter (mincutoff 2.0 Hz, beta 0.03, derivative cutoff 6 Hz) → resample to 90 Hz with linear interpolation → aperture `a` → `δ` → second-order Butterworth low-pass at 5 Hz → hysteresis (enter at `δ+`, exit at `δ−2 mm`) → `p`.

The 2 mm hysteresis prevents shield chatter at the density threshold: a player holding `p = 0.50` does not flicker between "dense" and "not dense" because of 0.2 mm of tracking noise. `dp/dt` is computed over a 55 ms window and clamped to ±6 /s.

**Rhythm classifier.** One pump cycle has three phases:
- **PRESS**: `p` crosses 0.25 upward and reaches 0.55 within 120–400 ms.
- **HOLD**: `p ≥ 0.55` for 150–600 ms with `|dp/dt| < 0.8 /s`.
- **RELEASE**: `p` falls below 0.25 within 120–450 ms of the hold ending.
A cycle is *valid* if all three phases are in order and total duration ≤ 1.4 s. Four consecutive valid cycles grant **resonance**: shield efficiency ×1.15 for 6 s. A static hold with `p > 0.60` and `|dp/dt| < 0.15 /s` for > 3.0 s is *stagnation*: efficiency ×0.80 and heat +4%/s until the player pumps again.

**Heat model.** `H ∈ [0, 100]`. While gripping:

`dH/dt = 5 + 32·max(0, (p − 0.80)/0.20)^{1.5} + 18·max(0, (|dp/dt| − 1.2)/1.2)`

While `p < 0.55`: `dH/dt = −(10 + 12·(0.55 − p)/0.55)`. Overheat at `H = 100`: shield collapses to `R_s = 40 mm` for 2.0 s, core takes 12 damage, `H` drains to 40 at 30%/s, and the player cannot charge above `p = 0.4` during the lockout. Over-pump is separate: `δ > 5.6 mm` for 1.5 s deals 4 HP/s and adds 6 heat/s.

At `p = 1.0` the heat rate is 37%/s, so a full-width beam hold of 1.4 s costs about 52 heat; two such holds without a vent overheat the core. At `p = 0` cooling is 22%/s. This is the intended rhythm: wide shield is a burst, dense shield is the rest posture.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to `P` with a 25 ms follow: `V_origin = lowpass(P_centre, 25 ms)`, and `V` axes equal `W` axes (not the ball's spin axes) so the arena does not rotate when the ball turns in the hand. The shield is a sphere at `V_origin` with radius `R_s`. The player's fist sits inside it.

| Entity | V-space position | Notes |
|---|---|---|
| Shield centre | `(0, 0, 0)` | 25 ms lag behind the fist |
| Shield surface | radius `R_s` ∈ [40, 160] mm | Fresnel + hex-cell shader |
| Sniper emitters | 900 mm from centre, elevation −10° to +25°, azimuth 12 slots | Fixed slots around the player |
| Beam | cylinder from emitter to shield centre | Radius `w_b/2`, 60–140 mm width |
| Bomb spawn | 200 mm from centre, azimuth 12 slots | Closing at 150–260 mm/s |
| Bomb detonation | 45 mm from centre | Proximity fuse; 0.6–1.0 s remaining |
| Core HUD | 220 mm in front of the eye, lower-left | HP, heat, density |
| Vent port (HUD) | projected onto the ball at φ +30°, λ 270° | Off-hand target |

Mapping formulas: shield radius `R_s = 40 + 120·p` mm; density `D = 1.00 − 0.60·p`; beam test `R_s ≥ w_b`; bomb test `D ≥ 0.70`. The density threshold corresponds to `p ≤ 0.50` and therefore `R_s ≤ 100 mm`. A bomb at 45 mm is always inside a "wide" shield, which is why wide does not save you: the field is too diffuse to stop it.

Arena scale: threats live between 45 mm and 900 mm from the fist. The 900 mm beam distance is beyond the tabletop class, but it is a *virtual* sky anchored to the hand, so the player never reaches for it; the shield and the HUD remain inside 200 mm. This is the one deliberate scale exception in the game.

## 8. Gameplay Systems & Rules

**Entities.** Core integrity 100 HP; shield with continuous radius and density; two threat types; heat 0–100; vent cooldown 20 s. A wave is 10 threats: 5 beams and 5 bombs in a scripted alternation, with two overlapping at higher levels.

**Beam rules.** A sniper emitter charges for 1.4 s (telegraph ring whose diameter equals `w_b`). At `t_arrival`, the shield passes if `R_s ≥ w_b` using the radius at that exact tick. Pass: deflect VFX, +120 score, heat +3. Fail: 18 damage, shield flares red, heat +8. Beam widths per wave: 60, 75, 90, 110, 140 mm.

**Bomb rules.** A bomb spawns at 200 mm and closes at `v_b` to a 45 mm proximity fuse, detonating on arrival or at the 1.2 s fuse, whichever comes first. At detonation, the shield passes if `D ≥ 0.70` (`p ≤ 0.50`). Pass: bomb crushes against the dense shell, +120 score, heat +2. Fail: 22 damage, screen flash. Bomb speed 150–260 mm/s gives a 0.6–1.0 s time to detonation, comparable to the beam telegraph, so the player must choose a posture rather than react late.

**Overlap.** From wave 3, a beam and a bomb can be live simultaneously with arrivals 0.35–0.60 s apart. The correct play is to hold wide until the beam resolves, then dump the grip in under 0.35 s to be dense for the bomb. The maximum reliable release rate is 6 /s, so a 0.35 s gap is just enough.

**Score.**

`threat_score = round(120 · posture_mult · timing_mult · resonance_mult)`

`posture_mult = 1 + 0.25·(margin / threshold)` where margin is `R_s − w_b` for beams or `D − 0.70` for bombs, capped at 1.25.

`timing_mult = 1 + 0.20·(1 − |t_resolve − t_commit| / 0.25 s)` clamped to [1.0, 1.2], where `t_commit` is when the player first entered the passing posture.

`resonance_mult = 1.15` while resonance is active, else 1.0.

`wave_clear_bonus = 300 + 50·(100 − H_final)/100`, rounded.

**Win/lose.** Five waves, 52 threats. Core 0 HP is a loss. Surviving all five with core > 0 is a win; the results screen ranks on score and remaining HP. There is no time limit per threat beyond its telegraph.

**Worked example.** Wave 3, beam `w_b = 110 mm` and bomb arriving 0.40 s later. The player holds `p = 0.92` → `R_s = 150.4 mm`; beam passes with margin 40.4 mm, `posture_mult = 1.092`. Release to `p = 0.35` in 0.32 s → `D = 0.79`; bomb passes with margin 0.09, `posture_mult = 1.032`. Both threats score about 131 and 124 before resonance.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load arena, start tracker | PRESENT |
| PRESENT | prop registered | RMS < 0.5 mm | Show relaxed-ball ghost | CALIBRATE |
| CALIBRATE | 3 open/close cycles | aperture swing ≥ 10 mm | Fit `a₀`, zero heat, set core 100 | WAVE_INTRO |
| WAVE_INTRO | 2.0 s timer | wave ≤ 5 | Announce threat list | THREAT |
| THREAT | `t ≥ t_spawn` | active < 2 | Spawn beam or bomb, start telegraph/fuse | THREAT |
| THREAT | beam arrival | `R_s ≥ w_b` | Deflect, score, heat +3 | RESOLVE |
| THREAT | beam arrival | else | 18 damage, heat +8, red flare | RESOLVE |
| THREAT | bomb detonation | `D ≥ 0.70` | Crush, score, heat +2 | RESOLVE |
| THREAT | bomb detonation | else | 22 damage, flash | RESOLVE |
| RESOLVE | 0.35 s timer | — | Despawn threat, check count | THREAT or WAVE_END |
| WAVE_END | 10 resolved | core > 0 | Bonus, show heat report | REST |
| WAVE_END | core ≤ 0 | — | Collapse VFX, dirge | GAME_OVER |
| REST | 10 s timer | wave < 5 | Cool at 22%/s, vent cooldown ticks | WAVE_INTRO |
| REST | timer 0 | wave = 5 | Results | RESULTS |
| ANY | `H ≥ 100` | — | OVERHEAT: collapse 2.0 s, −12 HP, lockout | THREAT |
| ANY | vent double-tap | cooldown = 0 | −35 heat, 20 s cooldown | previous |
| ANY | prop dropped | — | Shield decays 120 mm/s, pause threats 0.5 s | DROPPED |
| DROPPED | re-acquired < 4 s | — | Restore last `p`, 0.5 s grace | previous |

The `OVERHEAT` transition is checked before threat resolution in the same tick, so a player who overheats exactly as a beam arrives fails the beam. This is intentional and is called out in the onboarding.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C2 shield | Sphere, dynamic radius | Follows `P` + 25 ms | Radius rate-limited to 600 mm/s |
| C5 bomb | Sphere r = 4 mm | Kinematic closing | Speed 150–260 mm/s |
| C6 beam | Cylinder r = `w_b/2` | Static during 1.4 s charge | Analytic, no rigid body |
| C0 ball | Sphere r = 32 mm | Tracked | Not simulated |
| C3 fingers | Capsules | Tracked | Penetration source |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C6 × C2 | `R_s ≥ w_b` at arrival | Deflect; beam despawns |
| C5 × C2 | bomb at 45 mm, `D ≥ 0.70` | Crush; bomb despawns |
| C5 × C0 | bomb never reaches the ball (detonates at 45 mm) | Ignore |
| C3 × C0 | signed distance < 5 mm | Penetration `δ_i` |
| C4 × C0 | palm plane < 5 mm | Reference plate |
| C5 × C5 | ignored | Bombs pass |

**Materials.** Shield = "magnetic plasma": restitution 0.0, no friction, absorbs all momentum; bomb = "iron casing": restitution 0.25 if it somehow reaches the ground; beam = "photon lance": no mass, pure test. The real foam ball has µ = 0.85 against dry skin and a compression set of 3% after 10 minutes; neither is simulated because the prop is real.

**CCD.** Bomb displacement at 260 mm/s is 2.9 mm/tick, and the detonation shell is 45 mm, so discrete tests are safe; a swept sphere is used anyway to catch a bomb that crosses 45 mm between ticks at low frame rate. The shield radius is integrated at 90 Hz and clamped to ±600 mm/s, so a full 40→160 mm expansion takes exactly 0.2 s if the grip slams; the 13 ms first-order filter adds ≈ 9 ms of perceptual lag.

**Force balance.** The player's grip must overcome the foam's Hertzian reaction and the ball's internal damping. The reaction at `δ = 6 mm` is 16.7 N total, well below the 25 N comfort ceiling, but the *rate* of force rise is limited: a full squeeze in 0.10 s implies 167 N/s, which most players can produce; a full squeeze in 0.05 s is a slam and triggers the over-pump check. The shield radius is deliberately rate-limited to 600 mm/s so that even a slam cannot teleport the field across a beam.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → scene mesh → far arena (emitters, distant beams) → shield back hemisphere → real hand mesh → shield front hemisphere → bombs/beam impact → HUD. The shield is rendered as two hemispheres so the hand can be sandwiched correctly inside it.

**Occlusion cases.**

- **O1 — Hand inside the shield.** The front hemisphere is drawn at 0.18 base alpha with a 0.55 fresnel rim; a 55 mm capsule around the fist multiplies alpha by 0.35 ("fist keyhole") so the real hand reads clearly through the field. The back hemisphere is culled where the hand covers it.
- **O2 — Beam behind the fist.** A beam that passes behind the hand is depth-tested against the hand mesh; only the segment in front of the knuckles is drawn. The impact flash is drawn at the shield surface, not at the hand, so it never appears to burn the skin.
- **O3 — Bomb behind the hand.** The bomb is occluded by the hand mesh; its beep continues, and a 2 mm red proximity ring is drawn on the shield surface at the bomb's bearing so the player can still judge the fuse.
- **O4 — Shield intersecting the real forearm.** The shield is clipped by the forearm mesh; where the arm exits the field, the shader fades over 8 mm instead of showing a hard intersection line.
- **O5 — Sniper emitter behind the player's shoulder.** Emitters are depth-tested against the scene mesh and hand; a beam whose origin is occluded still shows its charge ring on the shield, so the threat is never invisible.
- **O6 — HUD versus shield.** The HUD is rendered after the shield at 220 mm, but depth-tested against the hand. A raised palm covers the heat bar; the bar fades to 40% rather than drawing through the hand.

**VFX budgets.** Shield shader ≤ 1 draw call, 3.2 ms; beam ≤ 2 simultaneous, 1.0 ms each; bomb trails ≤ 16 quads; impact particles ≤ 220; heat shimmer ≤ 1 full-screen pass at 0.35 ms. The shield uses a hex-cell pattern with an animated radius uniform; no refraction, because refraction would distort the passthrough hand and break the sense that the field is around a real fist.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Grip press | fist | Low thump 60 Hz, loudness ∝ `dp/dt` | Shield expands with a leading edge ring |
| Shield radius change | shield surface | Hum 90–240 Hz, pitch ∝ `R_s` | Hex cells stretch radially |
| Beam charge | emitter | Rising saw 200→1400 Hz over 1.4 s | Telegraph ring diameter = `w_b` |
| Beam deflect | impact point | Metallic clang 1.8 kHz + 80 Hz body | White cone flash, 20 sparks |
| Beam penetrates | chest | 55 Hz sub + glass crack | Red full-screen rim, HP bar shake |
| Bomb spawn | 200 mm bearing | Two-tone beep at 1.2 Hz | Red sphere + closing range ring |
| Bomb fuse < 0.4 s | bomb | Beep accelerates to 4 Hz | Ring turns white |
| Bomb crushed | 45 mm bearing | Foam squelch + metal crunch | Dense shell ripple, grey puff |
| Bomb detonates | 45 mm bearing | 40 Hz boom + debris | Orange flash, 0.6 mm camera shake |
| Heat warning | HUD | 880 Hz double beep at 2 Hz | Heat bar pulses amber |
| Overheat | fist | Steam burst + 120 Hz alarm | Shield collapses, HUD red |
| Vent | vent port | 6 kHz hiss for 500 ms | Steam jet, heat bar drops |
| Resonance | fist | Major fifth pad, 1.2 s | Shield gains a slow gold pulse |
| Wave clear | arena | 3-note rising arpeggio | Emitters power down |

Every gameplay event enum maps 1:1 to a row. Grip force is sonified continuously; the shield hum is the primary feedback for radius, and the heat beep rate is the primary feedback for the hidden `H` state.

## 13. Difficulty Progression & Tuning

| Level | Beam widths (mm) | Bomb speed (mm/s) | Time to detonation (s) | Overlap gap (s) | Heat multiplier | Threats |
|---|---|---|---|---|---|---|
| 1 | 60, 75 | 150 | 1.0 | none | 0.85 | 8 (4/4) |
| 2 | 60, 90, 110 | 180 | 0.9 | none | 0.95 | 10 (5/5) |
| 3 | 75, 90, 110 | 200 | 0.8 | 0.60 | 1.00 | 10 (5/5) |
| 4 | 90, 110, 140 | 230 | 0.7 | 0.45 | 1.10 | 12 (6/6) |
| 5 | 110, 140 | 260 | 0.6 | 0.35 | 1.20 | 12 (6/6) |

Named constants: `R_MIN = 40 mm`, `R_MAX = 160 mm`, `R_SPAN = 120 mm`, `D_MAX = 1.00`, `D_MIN = 0.40`, `D_THRESHOLD = 0.70`, `DELTA_MAX = 6.0 mm`, `HEAT_MAX = 100`, `OVERHEAT_LOCKOUT = 2.0 s`, `VENT_COOL = 35`, `VENT_CD = 20 s`, `CORE_HP = 100`.

Adaptive rule: track a 6-threat window. If 5 of 6 pass, add 10 mm to the next beam width (cap 140 mm) or +20 mm/s to bomb speed (cap 260). If 3 of 6 fail, subtract 10 mm / 20 mm/s (floors 60 mm / 150 mm/s) and lengthen the time to detonation by 0.1 s by dropping bomb speed. Heat multiplier is never adapted; the player's own rhythm governs heat. The adaptation is silent but the next telegraph visibly reflects it.

## 14. Edge Cases & Failure Recovery

- **Ball squashed flat and tracker loses it.** The ellipse fit reports a 0.85 axis ratio; the tracker holds the last undeformed radius and switches to the kinematic grip model. If the ball is still lost for 2.0 s, `DROPPED` fires even though it is in the hand. Recovery: open the hand for 0.5 s; the relaxed ball re-registers.
- **Hand tracking loses the fingertips behind the palm.** The kinematic model uses `mcp`/`pip` angles with a 60 ms lag. The shield radius is rate-limited, so a 60 ms stale estimate can move it at most 36 mm; the beam test uses the stale value only if the tracker flags `DEGRADED`, and the HUD shows a small "grip uncertain" icon.
- **Player squeezes with the off-hand.** The off-hand is not in the grip map; a full off-hand squeeze on the ball produces only the vent double-tap if it lands in the vent port. Elsewhere it is ignored with a soft "wrong hand" cue.
- **Vent spam.** Double-tap within the 20 s cooldown plays a dry click and applies no cooling; the heat bar flashes its cooldown arc.
- **Both threats arrive in the same tick.** Beams resolve before bombs in a fixed order. If both pass, both score; if the shield is wide for the beam and a bomb detonates in the same tick, the bomb fails because density is evaluated with the same `p`. The HUD warns with a split ring when arrivals are < 0.08 s apart.
- **Overheat at the same tick as a beam.** Overheat is applied first, forcing `R_s = 40 mm`; the beam then fails. The player sees both the steam burst and the penetration flash.
- **Tennis ball selected.** The force model uses the tennis constants, but `p` remains normalized by the same 6 mm, so the gameplay is identical; only the effort HUD and audio loudness scale up. A warning recommends the foam ball for session length > 5 min.
- **Player uses two hands to squeeze.** Both hands in the grip map would double the apparent force. The system accepts only the dominant hand's contacts; off-hand contacts are vent/swirl only.
- **Tracking loss during a beam charge.** The beam clock continues (it is wall-clock), but the shield freezes at its last radius for 400 ms. After 2 s the game pauses; on resume the beam is re-telegraphed from 0.6 s remaining.
- **Player stands and walks.** `V` follows the fist, so the arena travels with the player; the passthrough scene moves but the threat geometry stays fist-relative. A 1.5 m/s walking speed is tolerated; above that, the 25 ms follow lag exceeds 37 mm and the game prompts the player to stand still.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — ellipsoid fit, undeformed radius, `PoseFrame` at 72 Hz with `DEGRADED` flag.
- `GripSolver` — palm plane, pad centroid, aperture `a`, per-finger penetration `δ_i`, weighted `δ`.
- `DeformationModel` — Hertz constants per prop, `F(δ)`, contact radius, pressure; drives HUD only.
- `ShieldController` — `p → R_s`, `D`, rate limit, 25 ms `V` follow, resonance/stagnation multipliers.
- `HeatSystem` — `H` integration, overheat lockout, over-pump damage, vent cooldown.
- `RhythmClassifier` — PRESS/HOLD/RELEASE state machine, valid-cycle counter, resonance timer.
- `ThreatDirector` — wave script, beam/bomb spawn, overlap scheduling, arrival timestamps.
- `ThreatResolver` — exact-tick beam and bomb tests, damage, score.
- `ScoreSystem` — posture, timing, resonance multipliers.
- `CoreStateMachine` — §9 table and timers.
- `FeedbackRouter` — event enum → audio + VFX, coverage assertion at boot.

**Event flow.** `PropTracker` → `GripSolver` → `DeformationModel` → `ShieldController`; `GripSolver` → `RhythmClassifier` → `ShieldController`; `ShieldController` → `ThreatResolver`; `ThreatDirector` → `ThreatResolver` → (`HeatSystem`, `ScoreSystem`, `CoreStateMachine`, `FeedbackRouter`); `HeatSystem` → `ShieldController` (lockout) and `FeedbackRouter`.

**Pseudocode 1 — grip and shield tick.**

```
function tick(dt):
    pose = PropTracker.latest()
    if pose.mode == LOST:
        shield.freeze_for(400ms)
        if now - pose.last_seen > 2.0s: enter(DROPPED)
    contacts = ContactBus.valid(hand=R)
    tips_visible = count(contacts on finger plate)
    if tips_visible >= 3:
        a_direct = aperture_from_tips(contacts)
        a = 0.75*a_direct + 0.25*a_kinematic
    else:
        a = 0.25*last_a + 0.75*a_kinematic

    delta = clamp((2*r - a) / 2, 0, 6mm)
    delta = hysteresis(delta, enter=+0mm, exit=-2mm)
    p_raw = delta / 6mm
    p = butterworth5hz(p_raw)

    heat_rate = 5 + 32*pow(max(0,(p-0.80)/0.20),1.5)
               + 18*max(0,(abs(dp)-1.2)/1.2)
    if p < 0.55: heat_rate = -(10 + 12*(0.55-p)/0.55)
    H = clamp(H + heat_rate*dt, 0, 100)
    if H >= 100: overheat()

    R_target = 40mm + 120mm*p
    R_s = rate_limit(R_target, max_rate=600mm/s, dt)
    D = 1.00 - 0.60*p
    RhythmClassifier.update(p, dp, dt)
    ShieldController.apply(R_s, D, resonance, stagnation)
```

**Pseudocode 2 — threat resolution.**

```
function resolve(threat):
    if threat.type == BEAM:
        if shield.R_s >= threat.w_b:
            score = 120 * posture(shield.R_s, threat.w_b)
                        * timing(threat)
                        * resonance()
            Heat.add(+3)
            Feedback.play(BEAM_DEFLECT, threat.impact)
        else:
            Core.damage(18); Heat.add(+8)
            Feedback.play(BEAM_PENETRATE, chest)
    else:  # BOMB
        if shield.D >= 0.70:
            score = 120 * posture(shield.D, 0.70)
                        * timing(threat)
                        * resonance()
            Heat.add(+2)
            Feedback.play(BOMB_CRUSH, threat.impact)
        else:
            Core.damage(22)
            Feedback.play(BOMB_DETONATE, threat.impact)

    Score.add(score)
    if Core.hp <= 0: enter(GAME_OVER)
```

**Determinism.** Threat schedules are seeded per `(session, wave)`; the shield integration and heat integration are fixed-step. A recorded grip trace replays identically, which lets the acceptance harness assert exact HP and score values.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Aperture mapping | Inject palm/pad poses at `a = 64, 58, 52 mm` | `δ = 0, 3, 6 mm ± 0.15`; `p = 0, 0.5, 1.0 ± 0.025` |
| AT-02 | Hertz force | Evaluate `F(δ)` at 3 mm and 6 mm | 2.95 N ± 0.10 and 8.34 N ± 0.25 per pad |
| AT-03 | Shield radius | Hold `p = 0.5` for 1 s | `R_s = 100 mm ± 1`; full 40→160 mm ramp takes 0.20 s ± 0.02 |
| AT-04 | Density threshold | Hold `p = 0.49` and `p = 0.51`, detonate a bomb each | 0.49 passes (D = 0.706); 0.51 fails (D = 0.694) |
| AT-05 | Beam boundary | Arrive with `R_s = w_b − 0.1 mm`, then `+0.1 mm` | Fail then pass; damage 18 on fail, score on pass |
| AT-06 | Heat rate | Hold `p = 1.0` from H = 0 | H ≈ 37 ± 3 after 1.0 s; overheat at 2.7 s ± 0.2 |
| AT-07 | Overheat lockout | Force H = 100 during a beam | 12 damage, `R_s` forced 40 mm, no charge > 0.4 for 2.0 s |
| AT-08 | Over-pump | Hold `δ = 5.8 mm` for 1.6 s | 4 HP/s applied after 1.5 s; warning event emitted once |
| AT-09 | Rhythm valid cycle | Feed 300 ms press, 250 ms hold, 300 ms release | Classified valid; total 850 ms; no resonance until 4th cycle |
| AT-10 | Stagnation | Hold `p = 0.8`, `dp = 0` for 3.2 s | Efficiency 0.80 after 3.0 s; heat +4%/s |
| AT-11 | Vent | Double-tap vent port with 0 heat, then at 80 heat | First: −35 to 45, cooldown 20 s; second within cooldown: unchanged |
| AT-12 | Simultaneous arrivals | Beam and bomb resolve on the same tick with `p = 0.9` | Beam passes, bomb fails, damage exactly 22 |
| AT-13 | Shield occlusion | Place hand inside a 160 mm shield; read depth buffer at the palm centre | Shield alpha at palm ≤ 0.09; rim alpha ≥ 0.45; no shield over the knuckle silhouette |
| AT-14 | Audio coverage | Enumerate `FeedbackEvent` | 14/14 events mapped; no null clip; no duplicate missing VFX |

Tests run headless with synthetic hand skeletons and a deterministic clock. Numeric tolerances are inclusive; boundary tests AT-04 and AT-05 use `±0.1` of the threshold so a floating-point implementation cannot pass by accident.
