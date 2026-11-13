# Bernoulli Levitator

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Bernoulli Levitator |
| Slug | `bernoulli_levitator` |
| Physical prop | Regulation ping-pong ball, true sphere; 40.0 mm diameter (r = 20.0 mm); celluloid/ABS, 0.084 g/cm³ effective; mass 2.7 g; surface µ = 0.25 |
| Family / sub-shape | True sphere, pinch-scale upper bound (4 cm) |
| Scale class | Prop 1:1; virtual nozzle on the lap, orb column 300 mm tall |
| Source concept quote | "Pinch the ping-pong ball gently between your thumb and index finger, moving it vertically up and down 8cm while rotating it slightly." |
| Physical input | Thumb–index pinch; 80 mm vertical stroke; small twist about the pinch axis |
| Session length | 5 min (5 waves × 25 s + rests) |
| Lose condition | Nozzle pressure reaches 0 (three missed rings) or ball lost to the floor |

## 1. Fantasy & Core Loop

The player holds a ping-pong ball in a light pinch above a virtual wind-tunnel nozzle that sits on the lap. The nozzle throws a column of glowing blue air upward, and the physical ball is the handle of a tandem plasma orb riding that column: lift the ball and the orb climbs; lower it and the orb sinks; twist the ball and the orb spins. Electrified rings scroll out of the dark toward the player, and the orb must be threaded through them without touching the hoop. The physical ball never leaves the pinch until the player chooses to let it drop, and the virtual orb cannot teleport to the ring; it must be flown there with the hand.

1. **Pinch check (1.0 s).** The nozzle boots, the pinch is confirmed at < 26 mm tip separation, and the orb condenses above the mouth.
2. **Lift and settle (5.0 s).** The first ring approaches slowly; the player learns the 1:1 vertical mapping and the servo lag.
3. **Thread (12.0 s).** Three rings arrive at 150–260 mm/s; the player pumps the ball up and down the 80 mm stroke and twists for the spin bonus.
4. **Gust (4.0 s).** A crosswind vortex pushes the orb laterally; the player counter-steers by twisting and by small height changes that shift the orb into the shear layer.
5. **Resolve (3.0 s).** Rings clear, pressure tallies, next wave teaser.

A wave cycle is 25.0 s; five waves plus 5 s rests give a 2 min 5 s active run inside a 5 min session. A "surge wave" every third wave adds a second ring lane offset laterally, doubling the read without changing the vertical mapping.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the sphere center, poles at `±ẑ_P`, equator in the `x̂_P`–`ŷ_P` plane, and no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]` from the equator, longitude `λ ∈ [0°, 360°)` from +x̂_P toward +ŷ_P; surface point `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; outward normal = `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P-frame**: origin at the ping-pong ball center; `+ẑ_P` is set at registration to the pinch axis (from the index contact toward the thumb contact), so spin about the pinch axis is exactly spin about `ẑ_P`. Poles at `±ẑ_P`; equator in the `x̂_P`–`ŷ_P` plane.
- **Radius**: `r = 20.0 mm` for the collider and kinematics; visual radius 20.0 mm ± 0.25 mm. The real seam is cosmetic and is *not* used for pose, consistent with the featureless-sphere baseline.
- **Equator and poles**: the seam circle is at `φ = 0°`; `+ẑ_P` points from index pad toward thumb pad. The two polar calottes are visually undecorated except for virtual spin chevrons.
- **Rolling kinematics**: the canonical law is `ω = (n × v_t)/r`; each fingertip contact contributes `ω_i = (n_i × v_ti)/r` with `r = 20.0 mm`, and the two estimates are fused 50/50 when both contacts are stable, otherwise the stable one is used. The body-frame spin about `ẑ_P` is `ω_z = ω · ẑ_P`. Magnitude is clamped to 720 °/s and low-passed at 10 Hz.
- **V-frame anchoring**: `V` is anchored to `T`. `V_O` is the nozzle mouth, placed at `T_O + (0, 0, −250) mm` with `x̂_V = −ẑ_T` (player right), `ŷ_V = +ŷ_T` (up), `ẑ_V = −x̂_T` (toward the player); the nozzle jet is `+ŷ_V`, and rings scroll along `+ẑ_V`. The nozzle transform is re-solved whenever `T` updates, with a 4 mm dead band so the column does not jitter.
- **Deviation 1**: the orb height is driven by `P` relative to the nozzle, but the orb itself is a soft body with a critically damped servo, never a direct copy. This is the mandated "no teleport" behaviour.
- **Deviation 2**: the ball is airborne, so the engine uses two-contact angular fusion instead of a single table contact. The mandated formula is still the per-contact primitive.
- **Deviation 3**: release is recoverable for 2.5 s; the virtual orb falls but does not despawn, and re-pinching the physical ball re-engages control without a wave reset.
- **Deviation 4**: the pinch threshold uses fingertip *centres*, `d_ti < r + 6 = 26 mm`, which corresponds to roughly 7 mm of pad compression per side on the 40 mm ball.

## 3. Physical Play Space & Ergonomics

The player sits with the nozzle on the lap or on the table edge, 220–280 mm in front of the sternum, and the right or left hand (player choice) held 100–160 mm above the nozzle mouth. The pinching forearm is supported at the elbow; the wrist is neutral, the thumb and index form a light O, and the remaining fingers are relaxed and slightly curled. The head looks 10–20° down; the ring corridor is rendered at 0–15° below eye line.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Lap or table `T`; nozzle re-solved from `T` | Keeps the column vertical in the player's frame |
| Nozzle placement | 250 mm in front of `T` origin, mouth at lap height | Reachable without shoulder flexion |
| Play patch | 140 mm wide × 120 mm deep × 90 mm tall | Covers the 80 mm stroke plus tremor |
| Orb vertical range | 60–160 mm above the mouth | 1:1 with the 80 mm stroke plus margin |
| Ring corridor | `z ∈ [−900, +200] mm`, `y ∈ [60, 170] mm` | Rings pass the orb plane at chest height |
| Session | 6 min; 25 s waves; 5 s rests | Short enough to avoid static pinch fatigue |
| Pinch force | < 1.0 N; no squeeze required | The ball is held, not clamped |
| Stroke rate | 0.2–1.2 Hz | Below 1.5 Hz to avoid wrist tendinopathy |

Pinch-fatigue mitigations: the pinch is a light contact, not a force grip; the game accepts any thumb–index separation below 26 mm, so the player can rotate which pad bears the load; the ball is only 2.7 g, so the required force is dominated by posture, not weight; every wave ends with a 5 s "open hand" rest where the HUD explicitly prompts the player to release; the orb holds its last height for 400 ms after release so the player is never punished for a rest between rings; and a fatigue monitor watches pinch-force proxy (contact area × tremor) and auto-extends the rest to 8 s after three waves.

## 4. Prop Geometry & Registration

The prop is a 40.0 mm ping-pong ball. Mass 2.7 g, effective density 0.084 g/cm³, moment of inertia `I = 2/5·m·r² = 4.32 × 10⁻⁷ kg·m²`, surface µ = 0.25. The seam is a real visual feature but is not tracked; the runtime uses silhouette and shading only, then dead-reckons spin from contact motion.

| Sphere property | Value |
|---|---|
| Diameter | 40.0 mm ± 0.5 mm |
| Radius `r` | 20.0 mm |
| Mass | 2.7 g ± 0.2 g |
| Collider | Perfect sphere, r = 20.0 mm |
| Visual mesh | 64 × 32 UV sphere, thin translucent shell, seam normal map |
| Pose source | Visual odometry + contact-fused `ω` integration |
| Spin drift budget | 1.0 °/s with periodic optical-flow correction |

Surface-feature atlas (P-fixed; seam is real, other marks are virtual):

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| Equator seam | 0° ± 1.5° | all | Real seam; visual spin readability |
| Spin chevron 0 | +30° to +55° | 0° ± 8° | Virtual spin readout |
| Spin chevron 120 | +30° to +55° | 120° ± 8° | Virtual spin readout |
| Spin chevron 240 | +30° to +55° | 240° ± 8° | Virtual spin readout |
| North calotte | +80° to +90° | all | Thumb pad seat; matte |
| South calotte | −90° to −80° | all | Index pad seat; matte |
| Grip band | −15° to +15° | all | Acceptable pad zone; warning if outside |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 20.0 mm at `P` | Prop, pinched ball |
| C1 | Capsule | r = 4.0 mm, length 10 mm at `L_thumb_tip` | Pinch contact |
| C2 | Capsule | r = 4.0 mm, length 10 mm at `L_index_tip` | Pinch contact |
| C3 | Cylinder | r = 35 mm, h = 60 mm | Nozzle body |
| C4 | Cone | 18° half-angle, 300 mm tall | Air column, VFX trigger |
| C5 | Torus | major 45 mm, minor 4 mm | Scoring ring |
| C6 | Sphere | r = 12 mm | Virtual plasma orb |
| C7 | Capsule | r = 10 mm, length 150 mm | Gust vortex core |

Registration: present the ball at 0.8–1.2 m, rotate through ≥ 100° of arc for 1.0 s, hold 0.3 s. Fit acceptance: RMS < 0.5 mm, radius error < 0.25 mm, ≥ 16 rim points. At registration the pinch axis is captured as `+ẑ_P`; if the player re-pinches with a different axis, the game re-defines the *control* axis smoothly over 200 ms but keeps the P-frame for rendering.

Re-acquisition: search a 150 mm sphere around the last `P` for 1.5 s; succeed on 10 rim points at RMS < 0.9 mm for 4 frames. Because the ball is large and translucent, specular edges are strong and re-acquisition typically completes in 250 ms. Spin is preserved from the last `ω` and decays at 0.5 °/s² during loss.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long) | Semantic output |
|---|---|---|---|---|
| I5.1 | Pinch hold | `L_thumb_tip`, `L_index_tip` | thumb at φ +80°±10°, λ 0°±40°; index at φ −80°±10°, λ 180°±40° | `d_ti`, pinch state |
| I5.2 | Vertical lift | `L_thumb_tip`, `L_index_tip` | same | `h = P_y − nozzle_y` |
| I5.3 | Vertical drop | same | same | `dh/dt < 0` |
| I5.4 | Spin twist | same | pad shear; equator band φ ±15° | `ω_z` |
| I5.5 | Loose pinch | same | contact depth < 1 mm | warning, drift |
| I5.6 | Release | same | contact age > 80 ms after last valid | orb free-fall |
| I5.7 | Re-pinch | same | `d_ti < 26 mm` for 150 ms | control restore |
| I5.8 | Off-axis grip | same | thumb or index outside grip band | 0.6× spin gain, cue |

Per-finger roles: thumb is the upper pad, index the lower pad; middle/ring/pinky are relaxed and ignored. Either hand may be used; the runtime selects the hand with the higher pinch confidence and locks to it for the wave. The contact surface is the two polar calottes because that is where a 40 mm ball is most naturally pinched; off-calotte contacts are still accepted but produce a "slippery" cue and reduced spin authority.

## 6. Input Kinematics & Signal Processing

Vertical control: `h = clamp(P_y − V_{O,y}, 0, 300 mm)`, low-passed with a One Euro filter (mincutoff 2.5 Hz, beta 0.03) and rate-limited to 1.2 m/s. The orb target is `y* = h`, but the orb follows a critically damped servo rather than snapping: `ÿ = ω_n²(y* − y) − 2ζω_nẏ`, `ω_n = 8 rad/s`, `ζ = 1.0`, with `|ẏ| ≤ 1.2 m/s` and `|ÿ| ≤ 12 m/s²`. At 8 rad/s the 63% settle is 125 ms and the 95% settle is 375 ms, which reads as responsive but never teleports.

Spin control: per contact, `ω_i = (n_i × v_ti)/r` with `r = 20 mm`; fusion `ω = 0.5ω_thumb + 0.5ω_index` when both contacts are stable, else the single stable estimate. The spin sent to the orb is `s = (ω · ẑ_P)` in °/s, low-passed at 10 Hz and clamped to ±720 °/s. Contact-slip: residual `s_r = |v_finger,t − (v_P + ω × (r n))_t|`; if `s_r > 25 mm/s`, the contact is slipping and `ω` is blended 40% with the previous frame, then decayed with a 120 ms time constant.

Pinch classifier: `d_ti = |p_thumb_tip − p_index_tip|`. Pinch is valid when `d_ti < 26 mm` (r + 6 mm) for 30 ms; release is declared at `d_ti > 32 mm` for 80 ms (6 mm hysteresis). A "secure pinch" additionally requires both contact normals within 35° of `±ẑ_P`, which gates the spin gain. Pinch force proxy is `F_p ≈ k_pad·max(0, 26 mm − d_ti)` with `k_pad = 0.08 N/mm`; the HUD warns above 1.2 N.

Signal chain: joints → confidence gate (0.6) → One Euro → finite difference at 90 Hz → pinch distance, `h`, and per-contact `ω` → fusion → low-pass → dead zone. Height dead zone is ±2 mm with 1 mm release, enough to reject breathing but preserve 1 mm precision. Vertical velocity is computed over a 33 ms window (3 frames) to suppress 72 Hz tracking noise. Spin dead zone is 8 °/s.

Gesture rates: pinch state at 90 Hz, height at 90 Hz, spin at 90 Hz, ring pass detection at 90 Hz. A "pump" event fires when vertical direction reverses with `|dh/dt| > 40 mm/s`; it is used only for VFX and combo timing, not for physics.

## 7. Virtual World Layout & Coordinate Mapping

`V` is the nozzle frame from §2.1. The nozzle mouth is at `V = (0, 0, 0)`; the jet rises along `+ŷ_V`; rings spawn at `z = −900 mm` and travel toward the player along `+ẑ_V` at `v_ring = 150–290 mm/s` per the wave table in §13.

| Entity | V-space position | Notes |
|---|---|---|
| Nozzle mouth | `(0, 0, 0)` | Cylinder 35 mm radius, 60 mm tall below mouth |
| Orb rest height | `(0, 120, 0)` | 120 mm above mouth with the ball at neutral pinch |
| Orb stroke | `y ∈ [60, 160]` | 1:1 with the 80 mm hand stroke |
| Ring spawn | `z = −900`, `y ∈ [60, 170]`, `x ∈ [−25, 25]` | Ring plane normal `+ẑ_V` |
| Ring pass plane | `z = 0` | Distance judged in the `x`–`y` plane |
| Camera | `V` origin + head offset | Looks along `+ẑ_V` |
| Wind column | from `y = 0` to `y = h` | Cylinder radius 22 mm, additive |

Mapping `P` orientation to camera-facing visuals: the orb is rendered as a 24 mm glowing plasma sphere at `(0, y_orb, 0)`, with a surface flow shader whose phase rate is `s/60` revolutions per second. The physical ball's virtual skin shows the spin chevrons rotating at `ω_z`; because the ball is 40 mm and the orb 24 mm, the orb reads as a concentrated core rather than a copy. The orb is not head-locked; it stays in the nozzle frame so that moving the head reveals parallax between the column and the rings.

Height mapping is intentionally 1:1: a 10 mm hand lift is a 10 mm orb lift. The servo adds 125 ms of lag, so a 1 Hz, 40 mm stroke produces a 38 mm orb stroke with 9° of phase lag, which is the designed feel. Ring spacing is 1.6 s apart at 150 mm/s, giving the player 0.8 s of pre-warning before each pass plane.

## 8. Gameplay Systems & Rules

Entities: one orb, 3–5 rings per wave, one nozzle with pressure 3, and gust vortices on waves 3 and 5. Ring radius is 45 mm at wave 1, tapering to 40 mm at wave 5; the orb radius is 12 mm; pass tolerance is the orb *center* within the current ring radius in the ring plane.

Spawn rules: rings spawn every `T_r = 2.2 − 0.1·(wave − 1) s`, at `y = 80 + 60·U(0,1) mm`, `x = ±25 mm` alternating, speed `v_r` from the §13 table (150–290 mm/s). A wave ends after its ring quota passes the player or is missed. Waves 3 and 5 add a gust that applies a lateral acceleration of 0.6–1.0 m/s² toward `±x̂_V` for 1.2 s, telegraphed by a swirling vortex.

Scoring: pass `+100·M`; perfect pass (center within 15 mm) `+50`; spin match (|s − s_ring| < 90 °/s, where `s_ring` is the ring's marked required spin, ±180 or ±360 °/s) `+75`; consecutive passes build a combo `×1.0, ×1.25, ×1.5, ×1.75, ×2.0` capped at ×2.0; missing a ring costs 1 pressure and resets combo. `M = 1 + 0.08·(wave − 1)`.

Pressure and failure: pressure 3. A missed ring removes 1. Pressure 0 ends the run with a nozzle sputter. Releasing the pinch does not cost pressure immediately; the orb falls under `g_eff = 3.0 m/s²`, and if the player re-pinches within 2.5 s the orb is caught and play continues. If the orb falls below the nozzle mouth, the wave fails but the run continues with pressure −1.

Win: clear five waves. Lose: pressure 0 or the physical ball hits the floor (detected by a 1.2 m free fall). A no-miss run awards 600 and the "laminar" badge.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset tracked | load nozzle, zero pressure | Register |
| Register | ball presented | fit RMS < 0.5 mm | set `P`, `+ẑ_P`, nozzle from `T` | PinchCheck |
| PinchCheck | `d_ti < 26 mm` 30 ms | pinched | condense orb, enable height | WaveIntro |
| PinchCheck | 4.0 s timeout | no pinch | ghost hand prompt | Register |
| WaveIntro | 1.5 s timer | wave ≤ 5 | banner, ring schedule | Thread |
| Thread | ring pass plane | orb within ring radius | score, combo, VFX | Thread |
| Thread | ring pass plane | orb outside ring radius | pressure −1, combo reset | Thread |
| Thread | wave quota done | wave < 6 | tally, rest | WaveIntro |
| Thread | wave quota done | wave = 5 | victory | Victory |
| Thread | `d_ti > 32 mm` 80 ms | pinched | orb free-fall | Released |
| Released | re-pinch < 2.5 s | `d_ti < 26 mm` | catch orb, restore control | Thread |
| Released | orb `y < 0` | — | pressure −1, orb reset | Thread |
| Any | pressure = 0 | — | sputter, fade | GameOver |
| Any | ball free-fall 1.2 m | — | floor impact cue | GameOver |
| Any | prop lost > 1.5 s | — | freeze orb, show ghost | Recovery |
| Recovery | prop found | re-fit | re-anchor, 2 s grace | Thread |
| Any | headset removed | — | save wave/pressure | Paused |

The 2.5 s recovery window starts at release and is shown as a shrinking ring around the orb; re-pinching at any point inside it restores control with zero score penalty.

## 10. Physics & Collision

| Body | Shape | Mass | Drag / friction | Restitution |
|---|---|---|---|---|
| Ping-pong ball (input) | Sphere r 20 mm | 2.7 g | `C_d = 0.5`, µ 0.25 | 0.85 (not simulated) |
| Virtual orb | Sphere r 12 mm | 50 g (virtual) | linear damping 0.8 s⁻¹ | 0.0 |
| Ring | Torus 45/4 mm | static | — | — |
| Nozzle | Cylinder r 35 mm | static | — | — |
| Gust vortex | Capsule r 10 mm, L 150 mm | static | — | — |

Pair matrix: ball↔fingers (input), orb↔ring (score plane), orb↔nozzle (soft floor), orb↔gust (lateral acceleration), ball↔floor (failure only). The physical ball is never force-fed; all lift comes from the player. No collision impulse is applied to the real ball.

Orb servo constants: virtual mass `m_v = 50 g`, `k = m_v·ω_n² = 3.2 N/m`, `c = 2m_v·ω_n = 0.8 N·s/m`, giving `ω_n = 8 rad/s` and `ζ = 1.0`. Maximum servo force is `m_v·12 m/s² = 0.6 N`; this is a virtual force only. Ring pass test is `(x_orb − x_ring)² + (y_orb − y_ring)² < 45²` evaluated at the tick where `z_ring` crosses 0; the crossing time is interpolated to ±5.5 ms.

Air column model: jet velocity at the mouth is 12 m/s, decaying as `v(y) = 12·exp(−y/180 mm)`; this drives only the column shader and the orb's ambient wobble (0.4 mm amplitude at 3.7 Hz). Real ping-pong aerodynamics are irrelevant because the ball is hand-held.

CCD: rings move 2.9 mm per tick at 290 mm/s, far below the 40 mm minimum pass tolerance, so discrete plane-crossing is exact enough; the orb is integrated with 2 substeps for smooth servo motion.

## 11. Rendering, Occlusion & Compositing

Layer order: 7 UI → 6 hand mesh → 5 near VFX (sparks, wind motes) → 4 orb → 3 rings → 2 wind column → 1 nozzle → 0 passthrough. The hand is always depth-authoritative.

Occlusion cases:

1. **Thumb and index pinching the ball.** Both finger meshes occlude the ball correctly; the pad contact patches are highlighted at 2 mm offset so the player can see the pinch even when the pads cover the surface.
2. **Ball in front of the nozzle.** The ball is nearer and wins depth; the nozzle's mouth glow is clipped by the ball silhouette, which sells the column origin.
3. **Ring passing behind the hand.** The torus is cut by the real hand with a 1.5 mm depth bias; the player can judge ring distance by watching it disappear behind the fingers.
4. **Wind column crossing the palm.** The additive column is depth-tested and fades to 0 within 8 mm of the hand mesh, preventing a glowing sheet over the skin.
5. **Orb occluded by the ball.** The orb is drawn at the ball's centre but scaled 0.6; where the ball is nearer, the orb is hidden and only its corona shows, preserving the "core inside the handle" read.
6. **Gust vortex between orb and camera.** The vortex is drawn in front of the rings but behind the hand; it refracts the ring only within its capsule, not the passthrough.

VFX budget: 650 particles (250 wind motes, 200 ring sparks, 120 orb corona, 80 gust streaks), 1 column mesh, 5 ring meshes, 1 orb, 1 nozzle. Global cap 650; additive overdraw ≤ 2.2×; rings use a two-pass glow (core + halo) at half resolution. The wind column is a single raymarched cylinder at 45 Hz update, 90 Hz reprojection.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Pinch valid | Ball | Soft rubber chirp | Pinch ring closes, orb condenses |
| Lift start | Nozzle | Rising air whoosh, pitch ∝ `dh/dt` | Column brightens, motes accelerate |
| Spin twist | Ball | Airy whir, pitch ∝ `|s|` | Chevrons blur, orb flow speeds |
| Ring approach | Ring | Electric hum, proximity-faded | Ring arcs brighten, depth ticks |
| Perfect pass | Ring centre | Clean chime + sparkle | White flash, +50 popup |
| Pass | Ring centre | Warm blip | Ring blooms, combo stamp |
| Miss | Ring plane | Dull thud + air leak | Pressure pip cracks, ring fades |
| Gust | Vortex | Low crosswind howl | Streaks, orb wobble |
| Release | Ball | Rubber slip, falling whistle | Orb drops, catch ring appears |
| Catch | Ball | Soft boing | Orb snaps back, ring closes |
| Sputter | Nozzle | descending motor whine | Nozzle dims, pressure 0 |
| Victory | Nozzle | Rising four-note cadence | Column turns gold, tally |

Spatialisation is at the ball, ring, or nozzle; the HUD is the only head-locked element. The whir pitch tracks `|s|` from 180 to 900 Hz; the ring hum uses distance attenuation at 1/r with a 0.3 m floor.

## 13. Difficulty Progression & Tuning

| Wave | Rings | Ring speed (mm/s) | Spawn interval (s) | Ring radius (mm) | Spin target range (°/s) | Gust |
|---|---|---|---|---|---|---|
| 1 | 3 | 150 | 2.2 | 45 | ±180 | no |
| 2 | 3 | 180 | 2.1 | 45 | ±180, ±270 | no |
| 3 | 4 | 210 | 2.0 | 44 | ±180, ±270 | 0.6 m/s² |
| 4 | 4 | 250 | 1.9 | 43 | ±270, ±360 | no |
| 5 | 5 | 290 | 1.8 | 42 | ±270, ±360 | 0.8 m/s² |

Adaptive rule: if the player misses 2 rings in a wave, reduce ring speed by 15% and widen the pass radius by 3 mm for the next wave only. If the player passes 8 consecutive rings, increase ring speed by 8% and narrow the radius by 2 mm, capped at wave-5 values. Gust strength never adapts.

Named tuning constants: `R_PINCH_ON = 26 mm`, `R_PINCH_OFF = 32 mm`, `H_MIN = 0 mm`, `H_MAX = 300 mm`, `H_RATE = 1.2 m/s`, `OMEGA_N = 8 rad/s`, `ZETA = 1.0`, `ORB_R = 12 mm`, `RING_R = 45 mm`, `PERFECT_R = 15 mm`, `SPIN_MATCH = 90 °/s`, `SPIN_MAX = 720 °/s`, `G_EFF = 3.0 m/s²`, `CATCH_WINDOW = 2.5 s`, `PRESSURE = 3`.

## 14. Edge Cases & Failure Recovery

- **Pinch release.** The orb free-falls at 3.0 m/s² and a catch ring shrinks over 2.5 s. Re-pinch inside the window restores control; outside, the orb drops below the nozzle, costs 1 pressure, and resets to 120 mm.
- **Ball dropped to the floor.** A 1.2 m free fall or loss of tracking below table height ends the run with a floor-impact cue. The game does not attempt to track the ball on the floor.
- **Ball tracking lost while pinched.** Hold the last `h` for 400 ms, freeze the orb, and show a translucent ball ghost. If re-acquisition succeeds in 1.5 s, blend `h` over 200 ms; otherwise pause.
- **Hand tracking lost.** The pinch is held at its last state for 500 ms; then the orb gently settles to 120 mm and the game pauses. Recovery requires a fresh pinch, not just a hand reappearance.
- **Occlusion by the other hand or sleeve.** The system uses the high-confidence hand and ignores the occluder; if both are occluded, pause.
- **Thumb/index swap.** If the player rolls the ball 180° and re-pinches, the control axis flips; the spin sign inverts smoothly over 200 ms and a "flip" cue plays. No penalty.
- **Handedness swap.** The game accepts either hand; switching mid-wave requires a 1.0 s fresh pinch and keeps score.
- **Nozzle drift from `T`.** A table bump re-solves `V` with a 4 mm dead band and a 250 ms blend; rings already in flight keep their world positions, so no pass is invalidated.
- **Double ring at the same plane.** If two rings overlap in `z`, the nearer one scores first; the second is evaluated on the next tick, so a single pass cannot double-score.
- **Spin drift after occlusion.** Spin is re-estimated from optical flow on the seam when it becomes visible; correction is limited to 15 °/s per frame to avoid a visual snap.
- **Headset removed.** Save wave, pressure, combo, and orb height; on resume, require pinch re-validation and a 2 s settle.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `NozzleFrame` | Builds `V` from `T`, dead band, blend on table bump |
| `BallRegistration` | Sphere fit, `+ẑ_P` capture, loss/re-acquire |
| `PinchTracker` | `d_ti`, hysteresis, force proxy, secure-pinch gate |
| `HeightController` | `h`, rate limit, One Euro, servo target |
| `SpinFusion` | Per-contact `ω`, 50/50 fusion, slip clamp |
| `OrbBody` | Critically damped servo, orb position, fall state |
| `RingField` | Spawn schedule, scroll, plane crossing, pass test |
| `GustField` | Vortex timing, lateral acceleration, telegraph |
| `PressureState` | Pressure, combo, score |
| `WaveDirector` | Six-wave table, adaptation, rest |
| `FeedbackDirector` | Cue table, audio mix, VFX budget |
| `SessionFSM` | States, timers, replay log |

Event flow: `HandTracking(72 Hz) → PinchTracker + BallRegistration → HeightController/SpinFusion → OrbBody → RingField/GustField → PressureState → FeedbackDirector`. `WaveDirector` owns spawn timing and reads `RingField` results; `SessionFSM` owns release/catch and pressure transitions.

```text
function orb_step(y, ydot, target, dt):
    wn = 8.0; zeta = 1.0
    acc = wn*wn*(target - y) - 2*zeta*wn*ydot
    acc = clamp(acc, -12.0, 12.0)          # m/s^2
    ydot = clamp(ydot + acc*dt, -1.2, 1.2) # m/s
    y = clamp(y + ydot*dt, 0.0, 0.300)
    return y, ydot
```

```text
function pinch_step(d_ti, was_pinched, t):
    if not was_pinched:
        if d_ti < 0.026 and stable(30 ms):
            return PINCHED, t
        return RELEASED, t
    else:
        if d_ti > 0.032 and stable(80 ms):
            return RELEASED, t
        return PINCHED, t
```

The orb, rings, and gust are deterministic functions of wave time and player input; the replay log stores `h`, `s`, pinch transitions, and ring outcomes at 90 Hz.

## 16. Acceptance Test Checklist

1. **Sphere fit.** Present the ball at 1.0 m for 120 frames. Pass: `r` ∈ [19.75, 20.25] mm, RMS < 0.5 mm.
2. **Pinch thresholds.** Drive `d_ti` to 25, 26, 27, 31, 32, and 33 mm. Pass: pinch at 25/26, no pinch at 27; release only at 32/33; hysteresis stable over 100 cycles.
3. **Height mapping.** Move the prop from 80 to 160 mm in 0.5 s. Pass: orb target equals `h` ± 0.5 mm; no overshoot > 2 mm; settle < 400 ms.
4. **Servo lag.** Apply a 1 Hz, 80 mm sine. Pass: orb amplitude 78–82 mm, phase lag 8–12°.
5. **No teleport.** Step `h` by 100 mm. Pass: orb velocity ≤ 1.2 m/s and acceleration ≤ 12 m/s², measured over every tick.
6. **Spin.** Roll the ball at 360 °/s about `ẑ_P`. Pass: orb spin 360 ± 18 °/s, direction correct, 10 Hz low-pass corner measured.
7. **Slip clamp.** Command 1000 °/s. Pass: `|s|` ≤ 720 °/s and no quaternion flip.
8. **Ring pass.** Place the orb at 44 mm and 46 mm from a ring centre at crossing. Pass: score at 44, miss at 46; no double score at 45.5 mm.
9. **Perfect bonus.** Pass at 14 mm and 16 mm. Pass: +50 at 14, 0 at 16.
10. **Release and catch.** Release at `y = 120 mm`, re-pinch at 1.8 s. Pass: catch succeeds, orb returns to 120 mm in < 500 ms, no pressure loss.
11. **Release failure.** Release and do not re-pinch. Pass: orb reaches `y = 0` in 2.5 ± 0.2 s, pressure −1, orb reset.
12. **Occlusion.** Pass the open hand between camera and ball. Pass: ball hidden, ghost visible at 30%, no x-ray, 300 frames without z-fighting.
13. **Frame budget.** Run wave 5 with 650 particles and 5 rings. Pass: GPU < 11.1 ms, CPU < 5 ms, particle count ≤ 650.
14. **Endurance.** Run the 5 min scripted session. Pass: spin drift < 2°/min, height drift < 1 mm, memory growth < 4 MB.
