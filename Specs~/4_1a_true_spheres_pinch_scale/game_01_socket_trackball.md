# Socket Trackball

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Socket Trackball |
| Slug | `socket_trackball` |
| Physical prop | Soda-lime glass marble, true sphere; 16.0 mm diameter (r = 8.0 mm); glass, 2.50 g/cm³; mass 5.0 g; matte-etched 1.5 mm equator band optional for grip |
| Family / sub-shape | True sphere, pinch-scale |
| Scale class | Prop 1:1; virtual asteroid field 600 mm deep at tabletop scale (< 40 cm play volume) |
| Source concept quote | "Trap the marble securely inside a tight C-ring formed by your left thumb and index finger (preventing any drop), and roll its exposed top crown with your right thumb." |
| Physical input | Left thumb+index C-ring cradle; right thumb rolls the exposed crown |
| Session length | 7 min (5 waves × 30 s + 5 s inter-wave rest) |
| Lose condition | Hull reaches 0, or C-ring integrity lost longer than 2.0 s |

## 1. Fantasy & Core Loop

The player becomes the pilot of a needle-nosed starfighter welded into a heavy analogue cockpit. The left hand is the cockpit socket: thumb and index form a machined C-ring that traps a glass marble, and the marble is the ship's zero-friction trackball. The right thumb is the only control surface. Rolling the exposed crown tilts and banks the fighter; the asteroid field hangs 600 mm in front of the sternum, lit by cockpit spill, and every near miss throws a streak of parallax across the canopy. There is no fire button: survival is pure steering, and the physical marble is the steering column.

1. **Boot the socket (1.5 s).** The cockpit wakes; the C-ring integrity gate confirms both left contacts and paints the crown reticle. Input is dead until integrity passes.
2. **Drift corridor (12 s).** Asteroids enter on rails at 350–500 mm/s. The player rolls the crown to thread the fighter through gaps; near misses charge the boost meter.
3. **Surge (8 s).** Spawn rate doubles for a fixed burst; asteroid speed rises to 650 mm/s. Shield taps and boost are the only outs.
4. **Boost gate (5 s).** A torus gate opens at 600 mm; a clean pass grants wave multiplier and refills 25% shield.
5. **Resolve (3.5 s).** Remaining asteroids clear, score tallies, hull and shield report, next wave teaser.

Full cycle is 30.0 s; five waves plus 5 s rests give a 2 min 30 s active run inside a 7 min session including onboarding and score review. A wave fails forward: losing all hull ends the run, but the player may restart at the current wave once per session.

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

- **P-frame**: origin at the marble center; poles at `±ẑ_P` are inertial and arbitrary (the glass has no tracked pattern). `P` is updated by dead-reckoned rolling, not by visual feature matching; the runtime converts `W`'s centimetre API to millimetres at the boundary, so all constants below are mm, g, s, N.
- **Radius**: `r = 8.0 mm` for the collider and all kinematic formulas; visual radius 8.0 mm ± 0.15 mm registration tolerance.
- **Equator and poles**: equator is the `x̂_P`–`ŷ_P` great circle; poles `±ẑ_P`. Because the marble is featureless, a session-start "north" is chosen as the projection of `+ŷ_W` into `P`; all surface artwork (meridian bands, calotte) is P-fixed and therefore spins with the marble.
- **Rolling kinematics**: for a right-thumb contact with outward normal `n` and tangential fingertip velocity `v_t` relative to the prop center, `ω = (n × v_t)/r`. `ω` is integrated into the P orientation at 90 Hz. Magnitude is clamped to 540 °/s.
- **Crown geometry**: the left C-ring defines grip axis `g` (unit, from the marble center toward the mean left contact normal) and crown axis `c = g`. With grip aperture `d_grip` measured along `c`, the exposed crown half-angle is `θ_cap = acos((r − d_grip/2)/r)`. Typical tight C-ring gives `d_grip = 14.0 mm` and `θ_cap = 82.8°`; only right-thumb contacts within 70° of `c` are accepted as steering input.
- **V-frame anchoring**: `V` is anchored to `T`, not to `P`. Origin `V_O = T_O + (0, 220, 0) mm`; axes `x̂_V = ẑ_T` (player right), `ŷ_V = ŷ_T` (up), `ẑ_V = −x̂_T` (toward the player). The asteroid field is centred at `(0, 0, −600) mm` in `V`; the cockpit is drawn around the left hand but its transform is low-passed so the field does not swim.
- **Deviation 1**: the marble never drives the camera or the world anchor; it is an input transducer only.
- **Deviation 2**: C-ring integrity failure damps all steering by 80% rather than pausing, preserving the "never drop" fantasy without a hard stop.
- **Deviation 3**: the right thumb is the only accepted effector; contacts from `R_index_tip`, `R_middle_tip`, `R_ring_tip`, `R_pinky_tip` on the crown are classified as "brace" and produce a haptic-substitute thud but no control.
- **Deviation 4**: shield and boost are discrete gestures on the same crown (tap and double tap), so the right hand never has to leave the ball.

## 3. Physical Play Space & Ergonomics

The player sits upright with both forearms supported by the chair arms or the table edge. The left hand is held palm-up, wrist neutral, 250–300 mm in front of the sternum and 30–80 mm above the support surface. The right hand hovers 40–60 mm above the left, thumb angled down toward the crown, wrist in 10–20° extension. The head looks 15° down at the field; the field itself is rendered at 0–10° below eye line so the neck does not collapse.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Table `T` or lap detected by scene mesh | Keeps both elbows below heart, reduces shoulder load |
| Play patch | 180 mm × 140 mm centred on the left C-ring | Covers C-ring drift plus right-thumb sweep |
| Reach envelope | Wrists within 300 mm of sternum; elbow flexion 75–100° | Avoids full-arm reaching and gorilla arm |
| Field depth | 600 mm in front of `V` origin | Comfortable vergence at 0.6 m, ~1.7° per 18 mm asteroid |
| Session | 8 min; 30 s waves; 5 s rests | Six short bursts fit attention and grip stamina |
| Left grip duty | ≤ 45 s continuous, then 5 s release | Static pinch fatigue mitigation |
| Right thumb duty | Rolling only, no sustained pinch | Avoids thumb pad ischemia |

Pinch-fatigue mitigations: (a) the C-ring needs only ~1.5 N of pad force because the marble is trapped, not squeezed; (b) a "grip load" HUD arc turns amber after 40 s and prompts a 5 s hand-open rest between waves; (c) the right thumb rolls with the pad, never pinches, and the game accepts a relaxed 15–25° thumb flexion; (d) the run auto-pauses if the left contact is lost for more than 2.0 s, so the player never fights a degrading grip mid-surge; (e) micro-breaks are skippable only after the 5 s timer, preventing cumulative overuse.

## 4. Prop Geometry & Registration

The physical prop is a 16.0 mm soda-lime glass marble. Nominal mass 5.0 g, density 2.50 g/cm³, moment of inertia `I = 2/5·m·r² = 1.28 × 10⁻⁷ kg·m²`. Surface friction coefficient against skin µ = 0.30; rolling resistance on glass-to-skin is negligible because the marble is cradled, not rolled on a surface.

| Sphere property | Value |
|---|---|
| Diameter | 16.0 mm ± 0.2 mm |
| Radius `r` | 8.0 mm |
| Mass | 5.0 g ± 0.3 g |
| Collider | Perfect sphere, r = 8.0 mm |
| Visual mesh | 64 × 32 UV sphere, matte glass shader, index 1.46 |
| Orientation source | Dead-reckoned `ω` integration with 0.5 °/min drift budget |

Surface-feature atlas (P-fixed, spins with the marble):

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| North calotte | +75° to +90° | all | Spin-readout anchor; tinted cyan |
| Meridian 0 | all | 0° ± 1° | Spin ruler; white |
| Meridian 90 | all | 90° ± 1° | Spin ruler; white |
| Meridian 180 | all | 180° ± 1° | Spin ruler; white |
| Meridian 270 | all | 270° ± 1° | Spin ruler; white |
| Crown reticle | +20° to +82.8° from crown axis | all | Accepted input zone; amber ring |
| Grip shadow | −90° to −55° from crown axis | all | Left fingers; input ignored |
| Equator safety line | 0° | all | Visual C-ring alignment aid |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 8.0 mm at `P` | Prop, rolling input |
| C1 | Capsule | r = 1.6 mm, length 5.0 mm at `L_thumb_tip` | C-ring contact |
| C2 | Capsule | r = 1.6 mm, length 5.0 mm at `L_index_tip` | C-ring contact |
| C3 | Torus proxy | ring radius 8.4 mm, tube 1.2 mm, plane ⟂ `g` | Containment integrity test |
| C4 | Capsule | r = 12 mm, length 60 mm at ship | Player avatar |
| C5 | Sphere | r = 9–17 mm | Asteroids |
| C6 | Torus | major 70 mm, minor 8 mm | Boost gate |

Registration procedure: the player presents the marble 0.8–1.5 m from the head, rotates it through ≥ 120° of visual arc for 1.2 s, and holds still for 0.3 s. The fitter solves center + radius by least squares over the silhouette and specular highlights; acceptance requires RMS residual < 0.4 mm, radius error < 0.15 mm, and ≥ 18 visible rim points. Pose `P` then enters visual odometry at 72 Hz.

Re-acquisition: after any tracking loss, the system searches a 120 mm cube around the last `P` for up to 1.5 s. Re-acquire succeeds when 12 rim points fit with RMS < 0.8 mm for 5 consecutive frames; the pose snaps with a 120 ms ease-in. During loss, the last `ω` continues to integrate for 250 ms, then freezes. Loss > 2.0 s triggers the Recovery state and damps input to zero.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long or axis) | Semantic output |
|---|---|---|---|---|
| I5.1 | C-ring cradle | `L_thumb_tip`, `L_index_tip` | φ −85° to −55°, all λ; normals opposed (`n₁·n₂ < −0.7`) | Grip integrity 0–1 |
| I5.2 | C-ring cinch | `L_thumb_tip`, `L_index_tip` | same; tip separation < 3 mm | Integrity boost, shield recharge +5%/s |
| I5.3 | Crown roll | `R_thumb_tip` | within 70° of crown axis `c` | 2-DoF roll vector `v_t` |
| I5.4 | Boost tap | `R_thumb_tip` | crown; contact age 40–120 ms, travel < 2 mm | Boost (1 per 4 s) |
| I5.5 | Boost double-tap | `R_thumb_tip` | crown; two taps, gap 90–280 ms | Boost + 400 ms invulnerability |
| I5.6 | Shield tap | `L_middle_tip` | φ −40° to 0°, λ 150°–210° | Shield toggle, 150 ms hold |
| I5.7 | Brace | `R_index_tip`, `R_middle_tip`, `R_ring_tip`, `R_pinky_tip` | any crown contact | Audio thud only, no control |
| I5.8 | Release | all left contacts | contact age > 80 ms since last frame | Integrity decay, damp 80% |

Per-finger roles: the left thumb and index are a static clamp; the left middle is the shield key; the left ring and pinky stabilize the palm and are ignored. The right thumb is the steering effector; the right index, middle, ring, and pinky are braced against the left hand or the chair and are ignored except for the brace cue.

Contact gating: a right-thumb contact is valid only if its normal satisfies `n·c > cos 70° = 0.342`, its signed distance to C0 is < 5 mm, and its age > 30 ms. The crown reticle is drawn at 2 mm above the surface so it remains visible under shallow occlusion.

## 6. Input Kinematics & Signal Processing

The core rolling law is `ω = (n × v_t)/r`, where `n` is the outward unit normal at the right-thumb contact, `r = 8.0 mm`, and `v_t` is the fingertip velocity minus the prop-center velocity, projected onto the tangent plane at the contact. For a stationary cradle `v_P ≈ 0`, so `v_t ≈ v_finger,t`. The resulting `ω` is expressed in `P`, transformed to `V`, and decomposed into the two control axes:

- `ω_bank = ω · ẑ_V` (roll about the view axis), and
- `ω_pitch = ω · x̂_V` (nose up/down).

Ship rates are `bank_rate = −K_bank · u · sign(ω_bank)` and `pitch_rate = K_pitch · u · sign(ω_pitch)`, with `K_bank = 120 °/s`, `K_pitch = 90 °/s`, and `u = clamp((|v_t| − 20)/(220 − 20), 0, 1)`. The 20 mm/s dead zone is 2.5 mm of thumb travel per 120 ms, large enough to reject tremor yet below deliberate micro-rolls; 220 mm/s is the comfortable thumb sweep ceiling.

Signal chain: raw joint positions → One Euro filter (mincutoff 1.2 Hz, beta 0.02, derivative cutoff 8 Hz) → finite-difference velocity at 90 Hz → tangent projection → `ω` → second-order Butterworth low-pass at 12 Hz → dead zone with 14 mm/s release hysteresis → saturation → quaternion integration.

Contact-slip handling: compute slip residual `s = |v_finger,t − (v_P + ω × (r·n))_t|`. If `s > 25 mm/s` the contact is flagged slipping; blend `ω` 60% from the measured finger motion and 40% from the previous frame, and clamp `|ω|` to 540 °/s. If slip persists > 150 ms, the input is considered a scrub and is zeroed until the thumb lifts.

Gesture classifiers: **roll** = `|v_t| > 20 mm/s` sustained 80 ms; **tap** = contact age 40–120 ms with travel < 2 mm; **double tap** = two taps with inter-tap gap 90–280 ms; **shield hold** = `L_middle_tip` contact ≥ 150 ms; **cage/grip** = both C-ring contacts valid for 30 ms. All classifiers debounce at 30 ms and emit at most one event per 90 Hz tick. Input runs at 72 Hz and is resampled to the 90 Hz physics tick with a 1-frame prediction; the correction is clamped to 3 mm to prevent jitter amplification.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to `T` as defined in §2.1. The asteroid field occupies the rectangular volume `x ∈ [−220, 220] mm`, `y ∈ [−140, 140] mm`, `z ∈ [−600, −60] mm`. The ship is pinned at `(0, 0, −120) mm` and only rotates plus translates within a ±110 mm × ±70 mm aperture, so the player never has to chase the fighter.

| Entity | V-space position | Notes |
|---|---|---|
| Ship | `(0, 0, −120)` + aperture offset | Capsule 12 mm × 60 mm |
| Asteroid field centre | `(0, 0, −600)` | Rails converge toward the player |
| Cockpit dashboard | `(0, −40, −60)` | Wraps the left hand at 60 mm offset |
| Gate | `(0, 0, −600)` | Torus radius 70 mm |
| Camera | `V` origin + head offset | Head-locked, 600 mm from field centre |

Mapping `P` orientation to visuals: the marble skin is rendered as a 16 mm glass sphere at the tracked `P`; `q_P` drives the meridian texture. Ship bank `φ_s` is the integral of `bank_rate` clamped to ±55°, and pitch `θ_s` is clamped to ±35°; the camera inherits 20% of ship bank so the horizon feels attached without inducing sickness. The crown reticle is drawn in `W` around the current crown axis `c`, not in `P`, because `c` is defined by the left hand, not by the marble's spin.

Scale mapping: 1 mm of thumb travel ≈ 0.55° of ship bank at the top of the sensitivity curve; a full 220 mm/s sweep reaches 120 °/s in ~0.4 s, which crosses the 600 mm field in 2.0 s at nominal asteroid speed. This keeps the control loop bandwidth (~1.2 Hz) well below the 90 Hz tick and above the visual persistence threshold.

## 8. Gameplay Systems & Rules

Entities: one ship (hull 3, shield 1 charge), 6–34 asteroids per wave, 1 gate per wave, and non-colliding score pickups. Asteroids spawn on rails at `z = −600`, in annular lanes of radius 80–380 mm around the field axis, with speed `s_a = 350 + 50·(wave − 1) + U(−30, 30) mm/s` and radius `r_a = 9 + 8·U(0,1) mm`. Spawn interval is `T_s = max(0.18, 0.8 − 0.10·(wave − 1)) s`; the surge multiplies spawn rate by 1.8 for 8 s.

Scoring: dodge `+10·M` when an asteroid passes `z = −80` without collision; near miss `+25·M` when minimum ship-to-asteroid distance is < 30 mm; shield block `+5`; gate `+40`; wave clear `+100·wave`. `M = 1 + 0.1·(wave − 1)`. Chain multiplier: consecutive near misses within 2.0 s raise a separate chain counter at ×1.5 per link, capped ×4.

Damage: asteroid–ship contact removes 1 hull unless the shield is up; the shield absorbs one hit then enters a 1200 ms cooldown. Boost grants 400 ms of invulnerability and a 1.6× score multiplier while active. Hull 0 ends the run. The gate must be crossed within its 5 s window; missing it costs 20% of current score but does not end the wave.

Win: clear all five waves. Lose: hull 0. Partial win: clearing a wave with 1 hull left grants a "glass cannon" badge and +500 score.

C-ring integrity: `G = G_pos · G_norm · G_cont`, where `G_pos = 1` if both left tips are within 12 mm of the sphere surface, `G_norm = 1` if `n_thumb·n_index < −0.7`, and `G_cont = 1` if the torus proxy C3 intersects the sphere. `G = 1` normal; any factor 0 for > 120 ms sets `G = 0.2`, damping steering to 20%. Integrity is restored after 400 ms of valid contact.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset tracked | load shaders, zero counters | Register |
| Register | prop presented | fit RMS < 0.4 mm | set `P`, `r`, crown | GripCheck |
| GripCheck | both C-ring contacts valid 30 ms | `G = 1` | paint reticle, enable input | WaveIntro |
| GripCheck | 3.0 s timeout | still invalid | show ghost hand, audio prompt | Register |
| WaveIntro | 1.5 s timer | wave ≤ 5 | banner, spawn schedule | Steer |
| Steer | 12.0 s timer | no hull loss | accept roll, shield, tap | Surge |
| Steer | hull 0 | any | explosion cue | GameOver |
| Surge | 8.0 s timer | — | spawn ×1.8, speed +150 mm/s | Gate |
| Gate | gate crossed | orb within 45 mm | score, refill shield 25% | Resolve |
| Gate | 5.0 s timeout | missed | score penalty, close gate | Resolve |
| Resolve | 3.5 s timer | wave < 6 | tally, rest prompt | WaveIntro |
| Resolve | wave = 5 | — | victory cue | Victory |
| Any | integrity lost > 2.0 s | — | damp 0, freeze field | Recovery |
| Recovery | both contacts valid 400 ms | prop found | re-arm, 3 s grace | previous state |
| Any | headset removed | — | save run | Paused |

Timers are wall-clock at 90 Hz; the 30.0 s wave clock excludes the 5 s rest. State transitions are idempotent and logged with tick index for test replay.

## 10. Physics & Collision

| Body | Shape | Mass | Friction | Restitution |
|---|---|---|---|---|
| Marble (input only) | Sphere r 8.0 mm | 5.0 g | 0.30 | 0.05 |
| Ship | Capsule r 12 mm, L 60 mm | kinematic | — | — |
| Asteroid small | Sphere r 9 mm | 30 g | 0.4 | 0.2 |
| Asteroid large | Sphere r 17 mm | 90 g | 0.4 | 0.15 |
| Gate | Torus 70/8 mm | static | — | — |

Pair matrix: ship↔asteroid (damage), ship↔gate (trigger), asteroid↔asteroid (ignored, rails), marble↔left digits (input), marble↔right thumb (input), marble↔table (none, cradled). Collision tests use sphere–capsule closest point; a hit is `d < r_a + 12 mm`. CCD is enabled for asteroids at 650 mm/s: per tick travel is 7.2 mm, under the 9 mm minimum radius, so discrete tests are sufficient; the gate uses a swept torus test to avoid tunneling at boost speed.

Real-prop constants: `m = 5.0 g`, `I = 1.28 × 10⁻⁷ kg·m²`, `µ = 0.30`, `ω_max = 540 °/s`, angular acceleration clamp `α_max = 7200 °/s²`, rolling resistance `C_rr = 0.015`. The physical marble is never force-fed; all motion comes from the player. The virtual shield impulse is cosmetic: `J = m_a·Δv·0.2` with `m_a = 30 g`, used only for the shield ripple and audio amplitude.

Ship aperture springs: `a = −k·x − c·v` with `k = 220 s⁻²`, `c = 28 s⁻¹`, critically damped (`c = 2√k`); lateral acceleration is clamped to 9 m/s² to keep the horizon readable. Asteroid rails are parametric, not rigid-body, so wave timing is deterministic and replayable.

## 11. Rendering, Occlusion & Compositing

Layer order (near to far in the compositor): 7 UI → 6 hand mesh → 5 near VFX → 4 cockpit → 3 ship → 2 asteroid field → 1 scene-mesh depth → 0 passthrough. The hand mesh is a real tracked mesh, not a proxy, so fingers correctly cut the marble and the cockpit.

Occlusion cases:

1. **Left fingers in front of the marble.** Hand-mesh depth hides the covered band of the glass; the crown reticle remains because it is a decal offset 2 mm along the surface normal and is depth-tested against the hand, not forced on top.
2. **Right thumb over the crown.** The thumb mesh occludes the contact highlight; a small "under-thumb" glow ring is rendered at 60% opacity with a 1.5 mm depth bias so the player still sees the contact point.
3. **Marble behind the left palm from eye line.** The sphere is fully hidden; a wireframe "trackball ghost" at 30% opacity is pinned to the palm so the player understands the input is still live.
4. **Asteroid between hand and eye.** The asteroid is drawn in front of the hand mesh only where its depth is nearer; the real hand correctly occludes asteroids behind it, which preserves the cockpit illusion.
5. **Cockpit dash crossing the marble.** The dash is a 60 mm-offset shell; the marble is nearer and wins the depth test, so the reticle and glass read as inside the cockpit.
6. **Shield bubble intersecting the table.** The bubble is clipped by the scene mesh with a 1.5 mm depth bias; no z-fighting on the tabletop, and the passthrough is never tinted outside the bubble silhouette.

VFX budget: 900 particles total (600 asteroid trail, 150 impact, 100 boost, 50 ambient), 6 ribbon trails, 3 gate glows, 1 shield mesh, 1 cockpit spill light. Global particle cap 900; overdraw ≤ 2.5×; all sprites are 64 × 64 atlas, 90 Hz. The glass marble uses a single-pass refraction approximation and a Fresnel rim; no screen-space refraction to keep the compositor under 2 ms.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| C-ring locked | Left palm | Two glassy clicks, 1.2 kHz | Reticle snaps to amber, ring closes |
| C-ring slip | Left palm | Low ceramic scrape | Reticle stutters, grip arc flashes |
| Roll start | Right thumb contact | Soft airy whoosh, pitch ∝ speed | Marble meridian blur, streak |
| Near miss | Asteroid closest point | Sharp whip + doppler | White edge flare, +25 popup |
| Shield block | Ship | Metallic gong, 180 Hz | Cyan hex ripple, screen shake 0.4° |
| Boost | Ship | Rising plasma roar 0.4 s | Speed lines, chromatic fringe |
| Gate pass | Gate centre | Chime arpeggio, 3 notes | Torus blooms, multiplier stamp |
| Hull hit | Ship | Crunch + alarm | Red vignette, hull pip cracks |
| Wave clear | Dashboard | Warm synth cadence | Score tally, rest prompt |
| Grip warning | Left palm | Slow pulse, 220 Hz | Grip arc amber, 5 s countdown |
| Re-acquire | Marble | Soft sonar ping | Ghost dissolves, reticle returns |
| Game over | Ship | Descending three-note motif | Cockpit lights fade, score card |

All cues are spatialised at the physical contact or the virtual event; no cue is head-locked except the UI tally. Audio latency budget < 20 ms; the shield gong ducks the ambient bed by 6 dB for 250 ms.

## 13. Difficulty Progression & Tuning

| Level | Asteroid speed (mm/s) | Spawn interval (s) | Lane count | Ship aperture (mm) | Gate radius (mm) | Surge duration (s) |
|---|---|---|---|---|---|---|
| 1 | 350 | 0.80 | 4 | ±130 × ±90 | 80 | 6.0 |
| 2 | 400 | 0.70 | 5 | ±120 × ±80 | 75 | 6.5 |
| 3 | 450 | 0.60 | 6 | ±110 × ±70 | 70 | 7.0 |
| 4 | 500 | 0.50 | 7 | ±100 × ±65 | 65 | 7.5 |
| 5 | 550 | 0.40 | 8 | ±90 × ±60 | 60 | 8.0 |

Adaptive rule: if the player loses 2 hull within 30 s, reduce asteroid speed by 12% and widen the gate by 8 mm for the next wave only. If the player takes no damage for 45 s, raise speed by 8% and narrow the gate by 4 mm, capped at level 5 values. Adaptation never changes spawn interval by more than ±10% so wave length stays predictable.

Named tuning constants: `DZ_ROLL = 20 mm/s`, `DZ_RELEASE = 14 mm/s`, `V_SAT = 220 mm/s`, `K_BANK = 120 °/s`, `K_PITCH = 90 °/s`, `BANK_MAX = 55°`, `PITCH_MAX = 35°`, `OMEGA_MAX = 540 °/s`, `ALPHA_MAX = 7200 °/s²`, `CROWN_GATE = 70°`, `GRIP_DAMP = 0.2`, `SHIELD_CD = 1200 ms`, `BOOST_CD = 4000 ms`, `NEAR_MISS = 30 mm`, `FIELD_DEPTH = 600 mm`.

## 14. Edge Cases & Failure Recovery

- **Marble tracking loss (visual).** Keep integrating `ω` for 250 ms; then freeze the marble skin and show the trackball ghost. If re-acquisition succeeds within 1.5 s, snap with a 120 ms ease; otherwise enter Recovery and damp input to zero.
- **Left-hand tracking loss.** `G` drops to 0.2 immediately (80% damping), grip arc flashes red, and the game continues for 2.0 s. If the hand returns, a 400 ms validation restores control; if not, the run auto-pauses.
- **Right-hand tracking loss.** Steering damps to zero over 150 ms, the ship leveler engages (`bank → 0` at 30 °/s), and asteroids continue. Recovery restores the last valid `ω` only after 200 ms of fresh contact.
- **Marble dropped.** A free-fall detector (|a| > 0.6 g for 120 ms with no cradle contact) flags a drop. The field slows to 0.3×, audio says "recover the marble", and the player has 8 s to present it again; success returns to the same wave with a 2 s grace. If the marble is out of view, the system guides with a sonar ping and a floor arrow.
- **Contact ambiguity at the crown edge.** If a right-thumb contact falls between 65° and 75° from the crown axis, apply a 0.5× gain and a "edge" scrape cue; outside 75° ignore. This avoids control chatter at the grip boundary.
- **Thumb/index swap.** If the left contacts cross sides (normals flip sign), keep integrity but invert `g` smoothly over 100 ms; never snap the cockpit.
- **Handedness swap.** A left-handed player may mirror the setup; the game remaps the C-ring to the right hand and the effector to the left, and mirrors `V` about `x̂_V = 0`. Registration and all thresholds are unchanged.
- **Occlusion of the marble by the palm for > 1.0 s.** The ghost wireframe becomes the primary readout, and input remains live from proprioception; no fail state.
- **Headset removed.** Save wave, score, hull, and `P`; on resume, require re-registration and a 3 s settle before unpausing.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `PropTracker` | Visual odometry, radius fit, `P` smoothing, loss/re-acquire FSM |
| `SphereKinematics` | `ω = (n × v_t)/r`, quaternion integration, slip clamp |
| `HandContacts` | Per-joint signed distance, debounce, contact records |
| `GripIntegrity` | C-ring geometry, `G_pos/G_norm/G_cont`, damping factor |
| `RollController` | Dead zone, sensitivity curve, `bank_rate`/`pitch_rate` |
| `ShipController` | Bank/pitch integration, aperture spring, leveler |
| `AsteroidField` | Deterministic rails, spawn schedule, near-miss bookkeeping |
| `CollisionWorld` | Sphere–capsule, swept gate, layer masks |
| `WaveDirector` | Six-wave table, surge, gate, resolve |
| `ScoreService` | Multipliers, chains, badges, persistence |
| `AudioDirector` | Cue table, spatialisation, ducking |
| `SessionFSM` | State table in §9, timers, replay log |

Event flow: `HandTracking(72 Hz) → HandContacts → GripIntegrity/RollController → SphereKinematics → ShipController → CollisionWorld → ScoreService → AudioDirector/Renderer`. The `SessionFSM` subscribes to collisions, timers, and integrity; it owns all state changes. Rendering interpolates `P` and ship pose between the last two 90 Hz ticks.

```text
function update_roll(contact, P, dt):
    if contact.age < 30 ms or contact.normal · crown_axis(P) < cos(70°):
        return ZERO
    v_t = tangent(contact.finger_vel - P.vel, contact.normal)
    if |v_t| < 20 mm/s:
        return ZERO
    u = clamp((|v_t| - 20) / (220 - 20), 0, 1)
    omega = (contact.normal × v_t) / 8.0
    omega = clamp_magnitude(omega, 540 °/s)
    bank_rate = -120 °/s * u * sign(dot(omega, z_V))
    pitch_rate =  90 °/s * u * sign(dot(omega, x_V))
    return integrate(omega, bank_rate, pitch_rate, dt)
```

```text
function update_grip(contacts_L, P):
    ok_pos = dist(contacts_L.thumb, P.surface) < 12 and dist(contacts_L.index, P.surface) < 12
    ok_norm = dot(contacts_L.thumb.n, contacts_L.index.n) < -0.7
    ok_cont = torus_intersects(C3, P)
    target = (ok_pos and ok_norm and ok_cont) ? 1.0 : 0.2
    G = slew(G, target, 120 ms)
    if G < 0.5: warn_pulse(220 Hz)
    return G
```

State is snapshotted at each wave boundary; a crash restores the snapshot and replays up to 2.0 s of input for debugging. All constants live in a single `TuningConfig` asset and are hot-reloadable in development builds.

## 16. Acceptance Test Checklist

1. **Radius fit.** Present the marble at 1.0 m and sample 500 frames. Pass: fitted `r` ∈ [7.85, 8.15] mm, center RMS < 0.4 mm, and no frame exceeds 1.2 mm error.
2. **Crown gate.** Inject right-thumb contacts at 60°, 69.9°, 70.1°, and 80° from the crown axis with `v_t = 100 mm/s`. Pass: gains are 1.0, 1.0, 0.0, 0.0 respectively.
3. **Dead zone.** Sweep `v_t` from 0 to 40 mm/s at 5 mm/s steps. Pass: no ship rotation below 20 mm/s; rotation starts by 21 mm/s; release hysteresis keeps rotation alive down to 14 mm/s.
4. **Rolling law.** Place a synthetic contact at `n = +ẑ_P`, `v_t = (80, 0, 0) mm/s`. Pass: `ω = (0, 10, 0) rad/s` ± 2%, and the integrated quaternion rotates 57.3° about `+ŷ_P` in 0.1 s.
5. **Slip clamp.** Feed `v_t` producing 900 °/s demand. Pass: `|ω|` never exceeds 540 °/s and `α` never exceeds 7200 °/s².
6. **Grip damping.** Remove `L_index_tip` for 1.0 s while rolling at full tilt. Pass: steering gain drops to 0.20 ± 0.02 within 120 ms, and the warning cue fires once.
7. **Grip restore.** Restore both left contacts. Pass: gain returns to 1.0 within 400 ms with no overshoot above 1.05.
8. **Collision.** Drive an asteroid of r = 12 mm at the ship capsule with a 2 mm overlap. Pass: exactly one hull loss, shield ripple plays once, and the asteroid is despawned.
9. **Near miss.** Pass an asteroid at 29 mm and 31 mm minimum distance. Pass: +25 at 29 mm, 0 at 31 mm.
10. **Gate.** Cross the gate with the ship centre at 44 mm and 46 mm from the torus centre. Pass: pass at 44 mm, miss at 46 mm.
11. **Occlusion.** Place the left palm between the camera and the marble. Pass: marble fragments are fully hidden, the 30% ghost is visible, and no depth inversion or z-fighting appears over 600 frames.
12. **Frame budget.** Run a level-6 surge with 34 asteroids. Pass: GPU frame < 11.1 ms at 90 Hz, CPU game thread < 6 ms, particle count ≤ 900.
13. **Recovery.** Hide the marble from tracking for 1.4 s, then re-present it. Pass: re-acquire in ≤ 1.5 s, input restored within 200 ms, and no score change during the outage.
14. **Session endurance.** Run the 7 min scripted session with a synthetic hand. Pass: no drift > 0.5° in marble orientation, no accumulated pose error > 2 mm, and memory growth < 5 MB.
