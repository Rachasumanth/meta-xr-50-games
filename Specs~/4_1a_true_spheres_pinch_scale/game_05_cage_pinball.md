# Cage Pinball

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Cage Pinball |
| Slug | `cage_pinball` |
| Physical prop | Regulation ping-pong ball, true sphere; 40.0 mm diameter (r = 20.0 mm); celluloid/ABS, 0.084 g/cm³; mass 2.7 g; surface µ = 0.25 |
| Family / sub-shape | True sphere, free-rolling on a tray |
| Scale class | Prop 1:1; cage 140–180 mm across on a 300 mm tray |
| Source concept quote | "Place the ball on a tray table and arch your open hand over it like a spider cage so the ball stays trapped inside your five fingertips; nudge it back and forth between your thumb and pinky." |
| Physical input | Open-hand arch; five fingertip fences; thumb-to-pinky nudges |
| Session length | 7 min (5 waves × 26 s + 5 s rests) |
| Lose condition | Three ball escapes, or the ball leaves the tray |

## 1. Fantasy & Core Loop

The player's hand becomes a neon pinball cabinet. A ping-pong ball rolls freely on a tray; the player arches an open hand over it, fingertips planted like the legs of a spider, and the five contact points become bumper mushrooms. The ball is physically fenced by the fingers, and every tap, flick, and nudge fires a virtual bumper, lights a combo, and launches a pinball at a ring of crystal targets around the cage. The tension is real: if the ball slips past a fingertip, the cage fails, a life is lost, and the player must re-form the hand.

1. **Place and arch (2.0 s).** The ball is set on the tray; the hand arches over it; the cage radius is measured and the bumper mushrooms sprout at the five fingertip projections.
2. **Rally (15.0 s).** The player nudges the ball between thumb and pinky and around the fingers. Each fingertip contact with sufficient ball speed scores a bumper hit and alternates the combo.
3. **Crystal break (4.0 s).** Bumper hits fire virtual pinballs outward; aligned crystals shatter. The ring respawns after a short delay.
4. **Reset (3.0 s).** The ball is recentered by the player or by the reset assist; the cage re-forms; the next wave's crystal layout rotates into place.
5. **Resolve (2.0 s).** Tally, lives, combo peak, and a 5 s hand-open rest.

One wave is 26.0 s; five waves plus rests total 2 min 10 s of active play inside a 7 min session. A rally can chain indefinitely; the wave clock, not the combo, ends the wave.

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

- **P-frame**: origin at the ping-pong ball center; `+ẑ_P` is set at registration to `+ŷ_T` projected into `P`, so the equator is parallel to the tray and the seam lies in the tray plane. Poles `±ẑ_P`; the ball has no tracked faces, but the real seam is visible and is used only for spin readability.
- **Radius**: `r = 20.0 mm` for the collider and kinematics; visual radius 20.0 mm ± 0.25 mm.
- **Equator and poles**: equator is parallel to the tray; `+ẑ_P` is up. Rolling on the tray is planar, so the ball's contact normal is `n = +ŷ_T` (the tray normal pointing up into the ball).
- **Rolling kinematics**: the canonical law is `ω = (n × v_t)/r`; free rolling on the tray uses the same form with `v_t = v_P`, `ω = (n × v_P)/r`, `n = +ŷ_T` and `v_P` the ball-center velocity in the tray plane. Fingertip shear adds a spin term via the per-contact form `ω_i = (n_i × v_ti)/r`; the vertical component `ω·ẑ_P` is retained as the ball's spin. `|ω|` is clamped to 1080 °/s.
- **V-frame anchoring**: `V` is anchored to `T`, fixed at the tray-patch centre for the whole wave: `V_O` = the initial cage centre projected onto `T`, `x̂_V = x̂_T` (forward), `ŷ_V = +ŷ_T` (up), `ẑ_V = ẑ_T` (right). The cage itself is dynamic and moves with the palm; only the play plane is fixed.
- **Deviation 1**: the real ball is never held; it is fenced by real fingertips and can escape. There is no release state, only an escape state.
- **Deviation 2**: two physics tiers coexist. The real ball uses table rolling and measured fingertip impulses; virtual pinballs are lightweight ballistic spheres that exist only to break crystals and never push the real ball.
- **Deviation 3**: fingertip contacts are simultaneously physical collisions (real ball) and virtual bumper triggers (score events); the trigger is gated by ball speed so a resting ball cannot farm points.
- **Deviation 4**: the cage is a dynamic circle fitted to the palm and fingertips, not a collision wall; the fingertips themselves are the walls, so the cage radius is a *readout*, not a force field.

## 3. Physical Play Space & Ergonomics

The player sits or stands at a tray table. The tray is 300 mm × 220 mm, its surface within 2° of horizontal at 100–200 mm above the lap. The active hand arches over the ball with the palm 60–90 mm above the tray, the wrist neutral, and the five fingertips planted or hovering 0–15 mm above the tray around the ball. The elbow is supported; the shoulder is relaxed. The head looks 20–30° down, which is the largest downward gaze in this collection and is why session length is capped.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Tray table `T`, 300 × 220 mm | Real ball needs a bounded, planar arena |
| Play patch | 220 mm × 220 mm around the ball | Covers cage drift and nudge travel |
| Cage radius | 70–90 mm typical; clamp 60–110 mm | Matches an adult open-hand arch |
| Palm height | 60–90 mm above `T` | Clears the 40 mm ball and the finger arch |
| Session | 8 min; 26 s waves; 5 s rests | Wrist extension and gaze load |
| Hand switch | Allowed between waves | Unloads the arch hand |
| Nudge rate | 0.5–2.0 Hz | Keeps wrist deviation in comfort |

Arch-fatigue mitigations: the hand is open, so there is no pinch or grip load; the forearm is fully supported; the fingertip fence needs only light contact, and the game accepts a 0–15 mm hover for the non-active fingers; each wave ends with a 5 s rest and a prompt to shake out the hand; the player may switch hands between waves without losing progress; the HUD "arch load" meter rises while the palm is held above 60 mm and falls when the hand is lowered; and the downward gaze is limited to 30° with the tray raised to chest height for players with neck sensitivity.

## 4. Prop Geometry & Registration

The prop is a 40.0 mm ping-pong ball rolling on a tray. Mass 2.7 g, moment of inertia `I = 2/5·m·r² = 4.32 × 10⁻⁷ kg·m²`, surface µ = 0.25, rolling resistance coefficient `C_rr = 0.015` on a melamine tray. The ball is free, not held.

| Sphere property | Value |
|---|---|
| Diameter | 40.0 mm ± 0.5 mm |
| Radius `r` | 20.0 mm |
| Mass | 2.7 g ± 0.2 g |
| Collider | Perfect sphere, r = 20.0 mm |
| Visual mesh | 64 × 32 UV sphere, seam + matte shell |
| Pose source | Visual odometry + table-rolling `ω` |
| Tray planarity | ≤ 2.0 mm deviation over 300 mm |

Surface-feature atlas (P-fixed; the ball spins freely, so features are used for visual read only):

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| Equator seam | 0° ± 1.5° | all | Real seam; spin readability |
| Spin chevron 0 | +25° to +50° | 0° ± 8° | Spin readout |
| Spin chevron 120 | +25° to +50° | 120° ± 8° | Spin readout |
| Spin chevron 240 | +25° to +50° | 240° ± 8° | Spin readout |
| North calotte | +80° to +90° | all | Matte; no gameplay |
| South calotte | −90° to −80° | all | Contact patch with tray |
| Brand patch | −20° to +20° | 60°–100° | Registration aid, not required |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 20.0 mm at `P` | Real ball |
| C1–C5 | Capsules | r = 2.4 mm, length 8 mm at each fingertip | Fingertip fences |
| C6 | Plane | tray surface `T`, 300 × 220 mm | Rolling support |
| C7 | Disc | r = 12 mm, thickness 1 mm at each fingertip projection | Bumper |
| C8 | Capsule | r = 5 mm, arc 18 mm at cage perimeter | Crystal target |
| C9 | Sphere | r = 6 mm, speed 600 mm/s | Virtual pinball |
| C10 | Circle | dynamic, radius `R_cage` | Cage readout / escape test |

Registration: present the ball at 0.8–1.2 m, rotate ≥ 90° for 0.8 s, hold 0.3 s. Acceptance: RMS < 0.5 mm, radius error < 0.25 mm, ≥ 14 rim points. The tray is registered from the scene mesh; if planarity exceeds 2.0 mm, the game warns and asks for a flatter surface. The cage is calibrated at wave start from five fingertip projections; the radius is the mean radial distance and is clamped to [60, 110] mm.

Re-acquisition: after occlusion (hand over the ball is the common case), search a 150 mm cube around the last `P` for 1.5 s; succeed at RMS < 0.9 mm for 4 frames. Because the hand covers the ball during play, the system also predicts `P` from `ω` and the last velocity for up to 400 ms, then shows a ghost.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long) | Semantic output |
|---|---|---|---|---|
| I5.1 | Cage arch | all five `L_*_tip` | nearest ball points: thumb φ −10° λ 250°, index +20° λ 40°, middle +10° λ 0°, ring 0° λ 320°, pinky −10° λ 290° | Cage radius, bumper set |
| I5.2 | Thumb nudge | `L_thumb_tip` | φ −20° to +10°, λ 230°–270° | Ball impulse toward pinky |
| I5.3 | Pinky nudge | `L_pinky_tip` | φ −20° to +10°, λ 270°–310° | Ball impulse toward thumb |
| I5.4 | Fingertip flick | any `L_*_tip` | ball surface within 5 mm | Bumper hit, impulse |
| I5.5 | Cage widen | all tips | radius trend +10 mm/s | Wider cage, lower bumper density |
| I5.6 | Cage narrow | all tips | radius trend −10 mm/s | Tighter cage, higher precision |
| I5.7 | Escape warning | ball vs cage | `|b − c| > R + 10 mm` | Alarm, 5 mm margin |
| I5.8 | Hand switch | all `L_*` / `R_*` | confidence comparison | Swap cage hand between waves |
| I5.9 | Reset assist | open palm 300 ms | palm above ball, no contact | Recenter ball at next wave |

Per-finger roles: thumb and pinky are the primary nudge rails and define the long axis of the cage; index and middle are the "upper bumpers" that catch fast rebounds; ring is the backstop. The active hand may be either; the other hand rests. Each fingertip maps to one bumper disc, so the five-finger chord is also the five-bumper layout.

Contact gating: a bumper hit requires a fingertip–ball contact (signed distance < 5 mm) with ball speed > 40 mm/s and a 120 ms per-bumper cooldown. A flick is a contact whose relative approach speed exceeds 60 mm/s; the impulse is measured, not applied.

## 6. Input Kinematics & Signal Processing

Ball state comes from `P`: position `b = (P_x, P_z)` in `V`, velocity `v_P` from a One Euro derivative (mincutoff 4 Hz, beta 0.04, derivative cutoff 12 Hz), and speed clamped to 1200 mm/s. Table rolling gives `ω = (n × v_P)/r` with `n = +ŷ_T` and `r = 20 mm`; the vertical spin component `ω_z = ω·ẑ_P` is fused with fingertip shear estimates and low-passed at 12 Hz. Rolling residual `|v_c|` above 25 mm/s flags slip; on a smooth tray this occurs during hard flicks and is clamped by blending 50% with the previous frame for 100 ms.

Flick classifier: contact with approach speed `u_n = −(v_tip − v_ball)·n_contact`; a flick fires when `u_n > 60 mm/s` and contact age is 30–120 ms. Measured impulse is `J = m·|Δv_ball|` with `m = 2.7 g`; bands are soft (60–200 mm/s), medium (200–450 mm/s), hard (> 450 mm/s). The impulse is *measured from the ball's actual velocity change*, never synthesized, so the physical ball remains the authority.

Bumper trigger: when a flick or sustained contact occurs with `|v_ball| > 40 mm/s`, the corresponding bumper fires. Per-bumper cooldown 120 ms; global bumper rate cap 12 hits/s. Combo alternation: consecutive hits on *different* bumpers within 800 ms increase the combo chain; the same bumper twice does not break the chain but does not extend it.

Cage fit: project `L_palm` centroid and all five fingertips onto `T`. `c = (palm_x, palm_z)`, `R_raw = mean(|f_i − c|)`, `R_cage` low-passed at 3 Hz with a 2 mm dead band and clamped to [60, 110] mm. Escape test: `|b − c| > R_cage + 15 mm` for 100 ms; re-entry at `R_cage + 5 mm`. The 10 mm band prevents a ball grazing the fence from flickering the fail state.

Signal chain: joints → confidence gate 0.65 → One Euro → contact and velocity at 90 Hz → flick/bumper classifiers → cage fit → escape test. Event rates: 72 Hz input, 90 Hz simulation, bumper decisions at 90 Hz, one escape event per 500 ms maximum.

## 7. Virtual World Layout & Coordinate Mapping

`V` is the tray frame from §2.1, fixed for the wave. The cage centre and radius are dynamic; the bumper discs are at the five fingertip projections, and the crystal ring sits 30 mm outside the cage radius so it is always visible beyond the fingers.

| Entity | V-space position | Notes |
|---|---|---|
| Ball | `(b_x, 20, b_z)` | r = 20 mm, on the tray |
| Cage centre | `(c_x, 0, c_z)` | Palm projection |
| Cage radius | 60–110 mm | Mean fingertip radial distance |
| Bumpers | `(f_i,x, 2, f_i,z)` | r = 12 mm discs, one per fingertip |
| Crystals | radius `R_cage + 30`, 8 targets | 18 mm arc each, respawn 4 s |
| Virtual pinball | bumper origin, radial outward | r = 6 mm, 600 mm/s, 0.6 s life |
| HUD ring | radius `R_cage + 60` | Score, lives, combo |

Mapping `P` orientation to camera-facing visuals: the real ball's seam and virtual chevrons rotate at `ω`; the ball is rendered 1:1 and the seam is the primary spin cue. Bumpers are flat discs lying on the tray, so they read as part of the playfield and are occluded by fingers correctly. Crystals stand vertically (billboarded 35% toward the camera) so they are legible from the player's downward gaze. Cage radius is drawn as a thin neon circle on the tray; the escape margin is a dashed circle at `R + 15 mm`.

Pinball aim: direction is the radial unit vector from cage centre through the firing bumper, with a 3° random spread. Crystal hit test is angular: `|wrap(θ_pinball − θ_crystal)| < 9°` when the pinball reaches `R_cage + 30 mm`. This makes the bumper-to-crystal relationship readable without requiring pixel-perfect physics.

## 8. Gameplay Systems & Rules

Entities: one real ball, one dynamic cage, five bumpers, eight crystals, a pool of virtual pinballs, and three lives. The ball is the only physical dynamic body; pinballs are game tokens.

Wave rules: each wave requires 6 crystal breaks within 26 s. Crystals respawn 4 s after being broken, at the next free slot around the ring, so the ring is never empty. A wave ends at the timer or when the quota is met; meeting the quota early grants `+200` and the remaining time is converted to rest.

Scoring: bumper hit `+10·C`, where `C` is the combo multiplier `1 + 0.25·(chain − 1)` capped at ×4.0; crystal break `+100·C`; wave clear `+200`; perfect wave (no escape) `+300`. Chain resets on escape or on a gap > 800 ms between bumper hits. Ball speed bonus: a bumper hit above 600 mm/s adds `+5`.

Damage and failure: an escape (ball centre beyond `R_cage + 15 mm` for 100 ms) costs one life, resets the ball to the cage centre, clears the combo, and grants 1.5 s of no-escape grace while the cage re-forms. Three escapes end the run. If the ball leaves the tray (falls off an edge or is out of tracking below table height), it counts as an escape and the ball must be re-placed by the player.

Win: clear five waves. Lose: lives reach 0. A no-escape run awards 900 and the "spider sovereign" badge. The fantasy says the ball never escapes; the fail state is the moment the fantasy breaks, which is why it is loud, immediate, and recoverable.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset tracked | load tray, zero lives lost | Register |
| Register | ball presented | fit RMS < 0.5 mm | set `P`, tray plane, seed | CageCheck |
| CageCheck | five tips projected | radius ∈ [60, 110] | spawn bumpers, enable score | WaveIntro |
| CageCheck | 6.0 s timeout | hand not arched | ghost hand prompt | Register |
| WaveIntro | 1.5 s timer | wave ≤ 5 | banner, crystals, 26 s clock | Rally |
| Rally | bumper hit | speed > 40 mm/s, cooldown 120 ms | score, combo, fire pinball | Rally |
| Rally | crystal hit | pinball angle within 9° | break, score, respawn timer | Rally |
| Rally | escape | `> R + 15` for 100 ms | life −1, reset ball, grace 1.5 s | Reset |
| Rally | 26 s or quota | lives > 0 | tally, rest 5 s | WaveIntro |
| Rally | 26 s or quota | wave = 5 | victory if lives > 0 | Victory |
| Reset | 3.0 s timer | lives > 0 | re-form cage, recenter ball | Rally |
| Any | lives = 0 | — | cage collapses, fade | GameOver |
| Any | ball off tray | — | escape cue, re-place prompt | Reset |
| Any | prop lost > 1.5 s | — | freeze ball, ghost path | Recovery |
| Recovery | prop found | re-fit | re-anchor, 1.5 s grace | Rally |
| Any | headset removed | — | save wave/lives | Paused |

The 26 s wave clock pauses during Reset and Recovery; the combo timer does not pause, so an escape always breaks the chain.

## 10. Physics & Collision

| Body | Shape | Mass | Friction | Restitution |
|---|---|---|---|---|
| Ping-pong ball | Sphere r 20 mm | 2.7 g | 0.25 | 0.85 vs tray |
| Fingertip fence | Capsule r 2.4 mm | kinematic | 0.5 | 0.4 |
| Tray | Plane | static | 0.25 | 0.85 |
| Bumper disc | Disc r 12 mm | virtual | — | — |
| Crystal | Capsule arc 18 mm | virtual | — | — |
| Pinball | Sphere r 6 mm | virtual | — | — |

Pair matrix: ball↔tray (rolling), ball↔fingertips (physical fence + trigger), ball↔cage (readout only, no force), pinball↔crystal (break), pinball↔hand (ignored), bumper↔ball (trigger only). No virtual entity applies force to the real ball; the player's fingers do.

Real-ball constants: `m = 2.7 g`, `I = 4.32 × 10⁻⁷ kg·m²`, `µ = 0.25`, `C_rr = 0.015`, linear air drag negligible at < 1.2 m/s, `ω_max = 1080 °/s`. Rolling deceleration on the tray is `a_r = C_rr·g = 0.147 m/s²`, so a ball at 400 mm/s coasts 545 mm before stopping.

Flick impulse: measured `J = m·|Δv_ball|`; a 300 mm/s delta gives `J = 8.1 × 10⁻⁴ N·s`. The game classifies impulse bands but never adds energy. Virtual pinball: speed 600 mm/s, life 0.6 s, CCD swept sphere against the crystal arc; a hit at the ring plane is evaluated by angle, and the pinball despawns on hit or on life expiry.

Cage fit is a least-squares circle through the five fingertip projections, but the *centre* is fixed to the palm projection to keep the readout stable; only the radius is fitted. Escape detection uses the ball centre in the tray plane, not the 3D centre, so a bouncing ball cannot falsely escape.

## 11. Rendering, Occlusion & Compositing

Layer order: 8 UI → 7 hand mesh → 6 near VFX (sparks, trails) → 5 pinballs → 4 crystals → 3 bumpers → 2 cage ring → 1 ball → 0 passthrough. The hand mesh is depth-authoritative; the ball is small and low, so finger occlusion is the dominant compositing case.

Occlusion cases:

1. **Fingers in front of the ball.** Each fingertip capsule's real mesh occludes the ball; the ball is not x-rayed. A 2 mm-offset contact glow marks a live bumper even when the pad hides the ball.
2. **Palm directly over the ball.** The ball is hidden from the camera; the game draws a 25% ghost sphere and a seam-rotation hint on the tray so the player can still read position and spin.
3. **Ball in the gap between two fingers.** The ball is visible through the gap; the renderer does not fill the gap with a cage membrane, preserving the real passthrough read.
4. **Bumper discs under the fingertips.** Each disc is depth-tested against its finger and is drawn as a ring around the pad rather than a filled disc, so the pad never looks painted.
5. **Crystals behind the hand.** The ring is clipped by the hand with a 1.5 mm bias; crystals on the far side are dimmed to 40% rather than hidden, so the player knows the full target set.
6. **Pinball trails crossing the tray.** Trails are additive but clipped by the tray plane and fade within 5 mm of the surface; no z-fighting with the real table, no glow painted on the passthrough beyond the cage.

VFX budget: 850 particles (300 bumper sparks, 250 crystal shards, 150 pinball trails, 100 escape embers, 50 ambient), 5 bumper rings, 8 crystal meshes, 1 cage ring, 1 ball ghost. Global cap 850; additive overdraw ≤ 2.3×; shards are GPU-instanced and capped at 250.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Cage formed | Palm centre | Soft servo lock, two notes | Bumper rings sprout, cage ring draws |
| Bumper hit | Fingertip | Punchy neon blip, pitch by finger | Ring pulse, +10 popup |
| Combo step | Cage centre | Rising arpeggio, semitone per link | Combo meter climbs, color shifts |
| Pinball launch | Bumper | Electric zip, doppler | Bright streak, 3° spread |
| Crystal break | Crystal | Glass shatter + chord | Shards, +100, ring gap |
| Crystal respawn | Ring slot | Reverse shimmer | Crystal grows over 0.4 s |
| Escape | Cage edge | Alarm klaxon + ball rattle | Cage ring snaps red, life pip cracks |
| Reset | Ball | Soft recenter thump | Ball ghost slides to centre |
| Grace period | Ball | Ticking clock, 1.5 s | Dashed ring pulses |
| Wave clear | HUD | Warm arcade cadence | Tally, rest prompt |
| Hand switch | HUD | Two-tone chime | New cage forms, old fades |
| Game over | Cage | Descending neon power-down | Lights out, score card |

Spatialisation is at the fingertip, crystal, or cage centre; the HUD is head-locked. The bumper blip uses five pitches (one per finger) so the player can hear which bumper fired; the escape klaxon ducks all other audio by 10 dB for 600 ms.

## 13. Difficulty Progression & Tuning

| Wave | Crystals required | Crystal respawn (s) | Bumper cooldown (ms) | Combo window (ms) | Pinball speed (mm/s) | Escape margin (mm) |
|---|---|---|---|---|---|---|
| 1 | 4 | 5.0 | 180 | 1000 | 500 | 20 |
| 2 | 5 | 4.5 | 160 | 900 | 540 | 18 |
| 3 | 6 | 4.0 | 140 | 800 | 580 | 16 |
| 4 | 6 | 3.5 | 130 | 750 | 620 | 15 |
| 5 | 7 | 3.0 | 120 | 700 | 660 | 14 |

Adaptive rule: after an escape, widen the escape margin by 3 mm and lengthen the combo window by 100 ms for the next wave only. After a no-escape wave with 8+ combo peak, narrow the margin by 1 mm and shorten the combo window by 50 ms, capped at wave-5 values. The crystal count never exceeds 8 because the ring has 8 slots.

Named tuning constants: `CAGE_MIN = 60 mm`, `CAGE_MAX = 110 mm`, `ESCAPE_MARGIN = 15 mm`, `ESCAPE_HOLD = 100 ms`, `BUMPER_R = 12 mm`, `BUMPER_SPEED = 40 mm/s`, `BUMPER_CD = 120 ms`, `FLICK_SPEED = 60 mm/s`, `COMBO_WINDOW = 800 ms`, `COMBO_CAP = 4.0`, `PINBALL_SPEED = 600 mm/s`, `PINBALL_LIFE = 0.6 s`, `CRYSTAL_ARC = 18 mm`, `CRYSTAL_TOL = 9°`, `LIVES = 3`.

## 14. Edge Cases & Failure Recovery

- **Ball under the palm.** The most common occlusion. Predict `P` from `ω` and the last velocity for 400 ms; draw the ghost at 25%; if the ball is still hidden at 400 ms, freeze scoring but keep the cage live. A fingertip lift that reveals the ball restores full rendering instantly.
- **Ball escapes the cage.** Confirmed at `R + 15 mm` for 100 ms; life −1, combo reset, ball recentred by a ghost-guided nudge (the player may also roll it back), and 1.5 s of grace where escape is disabled.
- **Ball leaves the tray.** Counts as an escape and requires the player to re-place the ball; the wave clock pauses during re-placement.
- **Ball tracking lost.** Hold the last `P` for 300 ms, then show a ghost path predicted from `ω`. Re-acquire within 1.5 s; otherwise pause. A ball that was moving when tracking was lost continues on the predicted path for scoring only after re-acquisition confirms it.
- **Hand tracking lost mid-rally.** Freeze bumpers and escape detection for 500 ms; the ball keeps rolling physically. If the hand returns, re-fit the cage over 300 ms; if not, pause.
- **Cage radius collapse.** If the player closes the hand, `R` falls below 60 mm; the cage clamps at 60 mm, the bumpers crowd, and a "cage too tight" cue plays. The ball is likely to be squeezed out, which is a legitimate escape.
- **Cage radius balloon.** Above 110 mm the bumpers separate beyond useful pinball angles; clamp at 110 mm and show a "widen less" cue.
- **Simultaneous bumper hits.** If two fingers contact on the same tick, resolve in order of approach speed; the faster one scores, the other is queued on the next tick. No double combo from one physical event.
- **Handedness swap.** Allowed between waves; the cage, bumpers, and crystals mirror about the tray's `z` axis, thresholds unchanged.
- **Tray bump or tilt.** Re-solve `T` if planarity changes by > 3 mm; pause scoring for 500 ms and re-seat the cage. The virtual play plane follows `T`, so a tilted tray tilts the game rather than breaking it.
- **Headset removed.** Save wave, lives, score, combo, and `P`; on resume, require re-registration and a 2 s settle.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `PropTracker` | Ball fit, `P`, velocity, loss/re-acquire, ghost prediction |
| `TrayFrame` | `T` plane, planarity check, `V` origin, tilt re-solve |
| `CageFitter` | Palm projection, radius fit, clamp, hysteresis, escape test |
| `FingertipMap` | Five bumper projections, contact gating, flick impulse |
| `BumperSystem` | Cooldowns, combo chain, hit resolution |
| `PinballSystem` | Launch, spread, swept sphere, crystal hit |
| `CrystalRing` | Eight slots, break/respawn, wave quota |
| `BallPhysics` | Table rolling `ω`, drag, tray bounds |
| `ScoreService` | Combo, wave bonus, lives, badges |
| `FeedbackDirector` | Cue table, particles, audio mix |
| `SessionFSM` | States, timers, replay log |

Event flow: `HandTracking(72 Hz) → TrayFrame/CageFitter + FingertipMap → BumperSystem → PinballSystem → CrystalRing → ScoreService → FeedbackDirector`. `SessionFSM` owns escape, reset, lives, and wave transitions; `BallPhysics` runs inside the 90 Hz loop and only writes `P`-derived state.

```text
function bumper_step(contacts, ball, cage, now):
    for c in contacts sorted by approach_speed desc:
        if c.dist > 5 mm or ball.speed < 40 mm/s: continue
        if now - last_hit[c.finger] < 120 ms: continue
        J = ball.mass * |ball.delta_v|
        fire_bumper(c.finger, J)
        last_hit[c.finger] = now
        if c.finger != last_finger and now - last_time < 800 ms:
            combo = min(combo + 1, 8)
        last_finger = c.finger; last_time = now
```

```text
function escape_step(ball, cage):
    if cage.radius < 60 or cage.radius > 110: return HOLD
    d = |ball.xy - cage.center|
    if d > cage.radius + 15:
        if escape_since == 0: escape_since = now
        if now - escape_since >= 100 ms: return ESCAPE
    else if d < cage.radius + 5:
        escape_since = 0
    return SAFE
```

The replay log stores ball position/velocity, fingertip projections, cage radius, bumper events, and escape timers at 90 Hz. `V` is fixed per wave, so replay is deterministic given the same hand input.

## 16. Acceptance Test Checklist

1. **Ball fit.** Present the ball at 1.0 m for 120 frames. Pass: `r` ∈ [19.75, 20.25] mm, RMS < 0.5 mm.
2. **Rolling law.** Drive the ball at `v_P = (200, 0, 0) mm/s`. Pass: `ω = (0, 0, −10) rad/s` ± 2%, and the seam rotates 572.9° in 1.0 s.
3. **Cage radius.** Place fingertips at mean radii of 59, 70, 90, and 111 mm. Pass: `R_cage` clamps to 60, 70, 90, 110 mm, low-pass settle < 200 ms.
4. **Bumper trigger.** Contact the ball at 39, 41, and 40 mm/s with a 130 ms cooldown. Pass: no hit at 39, hit at 41, cooldown blocks a second hit at 100 ms.
5. **Combo.** Alternate five different bumpers at 700 ms spacing. Pass: chain 5, multiplier 2.0; a 900 ms gap resets the chain.
6. **Same-finger spam.** Hit the same bumper five times at 200 ms. Pass: score accrues, combo chain stays at 1.
7. **Pinball aim.** Fire from bumper angles 0°, 9°, and 10° relative to a crystal. Pass: break at 0° and 9°, miss at 10°.
8. **Escape.** Move the ball to `R + 14 mm` and `R + 16 mm` for 120 ms. Pass: no escape at 14, escape at 16; re-entry at `R + 5` clears the timer.
9. **Escape grace.** Trigger an escape, then drive the ball to `R + 20 mm` during the 1.5 s grace. Pass: no second life loss, and the grace ends on time.
10. **Occlusion.** Place the palm fully over the ball. Pass: 25% ghost visible, seam hint rotates, no x-ray, scoring frozen after 400 ms.
11. **Tracking loss.** Hide the ball for 1.2 s. Pass: ghost path follows `ω`, re-acquire ≤ 1.5 s, no phantom bumper hits.
12. **Frame budget.** Run wave 5 with 8 crystals, 850 particles, and 12 pinballs. Pass: GPU < 11.1 ms, CPU < 5 ms, particle count ≤ 850.
13. **Tray tilt.** Tilt the tray 4° and re-solve `T`. Pass: game plane follows, no score event during the 500 ms re-seat, cage re-forms within 300 ms.
14. **Endurance.** Run the 7 min scripted session. Pass: `P` drift < 1.0 mm/min, cage radius jitter < 2 mm RMS, memory growth < 5 MB.
