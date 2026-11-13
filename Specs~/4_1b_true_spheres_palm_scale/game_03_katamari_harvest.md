# Katamari Harvest

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Katamari Harvest |
| Slug | `katamari_harvest` |
| Physical prop | Tennis ball, true sphere, r = 33.5 mm (67 mm diameter), 58 g, felt surface, visible seam; alternate smooth rubber ball r = 32 mm, 60 g |
| Family / sub-shape | True sphere, palm-scale (6–12 cm class), rolling on a support surface |
| Scale class | Prop 1:1; virtual meadow 300 × 220 mm on the tray/lap, tabletop class |
| Source concept quote | "Rest a tennis ball on a tray across your lap and roll it under a flat palm in small five-centimetre circles. The ball becomes a sticky magnetic sphere in a holographic meadow, and every sheep, fence, and barn it touches is harvested into its growing mass." |
| Physical input | Flat palm rolling on the top hemisphere; ball rolls on the tray below; circular strokes 50 mm diameter |
| Session length | 6 min: onboarding 50 s + 3 seasons × 90 s + 2 rests × 15 s + results |
| Lose condition | Season ends below the mass quota, or mass falls under 60 units |

## 1. Fantasy & Core Loop

A holographic meadow unfolds across your lap. The tennis ball under your palm is a harvest engine: a sticky magnetic sphere that rolls through the grass and swallows everything smaller than itself. Sheep bleat and vanish into the surface; fences fold in with a wooden clank; barns tear out of the ground and compress into the growing ball. Spiky mines are the only thing the field cannot digest — touch one and the harvest coughs mass back into the meadow. Each season is ninety seconds of rolling, and the ball only grows if you keep it moving through denser and denser loot.

1. **SCOUT (1–2 s).** The meadow reveals the densest cluster within the 300 × 220 mm field. A faint hoop marks the recommended 50 mm palm orbit.
2. **ROLL (3–8 s).** The flat palm orbits at 0.8–1.2 Hz; the ball centre traces a 25 mm circle under the palm while the hand translates across the tray. The sticky sphere follows the tracked ball with a 30 ms filter.
3. **HARVEST (0.25 s per object).** An object inside the sticky radius and below the size tier spirals in, adds its mass, and increments the chain. The sphere radius recomputes as `R = 22·(M/100)^{1/3}` mm.
4. **CHAIN (up to 1.2 s gaps).** Consecutive harvests raise the score multiplier to a maximum of 2.5×. A gap longer than 1.2 s resets the chain.
5. **DODGE (continuous).** Mines drift at 20 mm/s. A mine contact costs 12% of mass, stuns the sticky field for 0.8 s, and resets the chain.
6. **RESOLVE (season end).** At 90 s the ball's mass is compared with the quota; surviving mass converts to score and the next season's quota rises.

A typical harvest chain is 4–7 objects over 6 s, with a dodge every 10–15 s. A season contains 60–90 harvests and 4–8 mine encounters.

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

`P` origin is the ball centre; poles are `±ẑ_P`, with `+ẑ_P` normal to the tray at rest; radius `r = 33.5 mm`. The rolling law `ω = (n × v_t)/r` holds at each contact. This game has *two* contacts, palm above and tray below, so the no-slip constraint gives `v_C = v_palm/2` and `ω = (n_T × v_palm)/(2r)`: a 100 mm/s palm sweep moves the ball centre at 50 mm/s and spins it at 1.49 rad/s (85.5 °/s). `V` is anchored to `T` at the tray centre, axes aligned with `T`; the sticky sphere centre is the tracked ball centre projected onto the tray plane. Deviations:

1. **Two-surface kinematics.** The factor 1/2 and the tray normal replace the single-contact law; the palm's tangential velocity is the input, the ball's tracked centre is the truth.
2. **Position-only gameplay.** A felt ball has a seam, but the harvest reads only `P` translation. Rotation drives the seam texture and the "roll quality" metric, never the hit test.
3. **Meadow is `T`-fixed.** Entities do not rotate with the ball; the sticky sphere is a mobile collector over a static field.
4. **Soft capture, no rigid bodies.** Harvest is a distance-and-tier query with a 0.25 s spiral animation; only mines and entity drift use simple integration.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated, tray on lap or table | The ball must be trapped between palm and a rigid plane |
| Support surface | Rigid tray 380 × 280 mm, or a lap board | Soft fabric absorbs the rolling constraint and biases `v_C` |
| Tray height | 660–720 mm | Forearm rests at 15–25° below horizontal |
| Ball position | 240–320 mm from the sternum, centred | Keeps the palm stroke inside the shoulder's neutral cone |
| Palm | Flat, fingers extended, 0–15° wrist extension | Maximizes contact area and reduces slip |
| Stroke | 50 mm diameter circle at 0.8–1.2 Hz | Produces 25 mm ball-centre orbit at 40–75 mm/s |
| Translation | Hand may traverse 280 × 200 mm | Covers the full meadow without trunk lean |
| Session | 6 min; 3 seasons; 15 s rests | Palm rolling is low-force but repetitive |
| Break cue | 15 s hand-open overlay between seasons | Prevents thenar fatigue |

The tray is mandatory. On a lap without a board, the game detects `T` as soft (surface compliance estimated from mesh vibration) and reduces the two-surface gain by 15% because the ball sinks and slips. The palm should be flat, not cupped: a cupped hand creates a second contact ring that violates the two-surface model and makes `v_C` unpredictable. The onboarding draws a 50 mm hoop and scores three practice orbits before the first season.

## 4. Prop Geometry & Registration

| Property | Tennis ball (default) | Rubber ball |
|---|---|---|
| Diameter | 67.0 mm | 64.0 mm |
| Radius `r` | 33.5 mm | 32.0 mm |
| Mass | 58 g | 60 g |
| Inertia `I = 2/5·m·r²` | 2.60 × 10⁻⁵ kg·m² | 2.46 × 10⁻⁵ kg·m² |
| Friction vs tray µ | 0.62 | 0.70 |
| Friction vs skin µ | 0.58 | 0.65 |
| Rolling resistance `C_rr` | 0.012 | 0.010 |
| Restitution | 0.72 | 0.60 |

The ball is a perfect sphere collider at `P`. Because it is trapped between two surfaces, the effective inertia for palm-driven motion is `I_eff = I + m·r² = 7/5·m·r² = 9.10 × 10⁻⁵ kg·m²`; this is the value used to convert palm force into acceleration in the assist model, not a simulated rigid body.

**Surface-feature atlas (P-fixed):**

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| North contact patch | +75° to +90° | all | Palm contact; low-friction cap |
| South contact patch | −90° to −75° | all | Tray contact |
| Seam great circle | all | 0° and 180° | Rotation readout |
| Equator band | −10° to +10° | all | "Roll quality" stripe |
| Balance dot | +45° | 0° | Confirms spin direction |
| Grip-free zone | +60° to +90° | all | Palm must not pinch here |

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 33.5 mm at `P` | Physical ball |
| C1 | Plane | tray plane `z_T = 0` | Rolling support |
| C2 | Disc | r = `R` (22–60 mm), on tray plane | Sticky harvest field |
| C3 | Capsule | r = 1.8 mm, length 12 mm, per fingertip | Contact |
| C4 | Disc | r = 9–11 mm | Sheep / mine |
| C5 | Box proxy | 18 × 6 mm footprint | Fence |
| C6 | Disc | r = 38 mm | Barn |

Registration: present at 0.8–1.2 m for 1.0 s, rotate 120°, hold 0.3 s. Acceptance: RMS < 0.5 mm, radius error < 0.2 mm, seam visible for at least 60% of frames. Because a sphere's orientation is not observable from silhouette alone, `q_P` is dead-reckoned from the two contact `ω` estimates and corrected by seam odometry when the seam is visible.

Re-acquisition: search a 140 mm × 140 mm × 80 mm box above the last tray contact for 1.2 s. Success requires 8 rim samples, RMS < 0.8 mm, across 6 frames; snap 100 ms. If the ball rolls off the tray, the game pauses and draws a recovery circle at the last known position.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long or axis) | Semantic output |
|---|---|---|---|---|
| I5.1 | Flat palm roll | `R_palm` pads + `R_index_tip` … `R_pinky_tip` | φ +55° to +90°, all λ; ≥ 4 contacts | Palm velocity `v_palm`, rolling drive |
| I5.2 | Circular stroke | same; orbit fit over 1.2 s | orbital radius 25 ± 8 mm | Stroke quality 0–1, +15% drive |
| I5.3 | Steer translate | same; low-frequency palm centroid | tray plane | Meadow steering vector |
| I5.4 | Lift / coast | all palm contacts lost > 80 ms | — | Ball coasts, drive = 0 |
| I5.5 | Brake press | palm contact, downward velocity > 40 mm/s | φ +70° to +90° | Damping ×3 for 200 ms |
| I5.6 | Finger brace | `R_thumb_tip`, `R_index_tip` on the side | φ 0° to +40° | No command; audio cue |
| I5.7 | Off-hand guard | `L_palm` hovering beside the tray | no contact | Pauses mine drift within 40 mm |
| I5.8 | Pinch lift | thumb + index pinch on the ball | any | Illegal; triggers "flat palm" coaching |
| I5.9 | Drop | ball leaves tray bounds | — | `OFF_TRAY` recovery |

**Per-finger roles.** The whole palm is the drive surface; the index and middle pads carry 60% of the contact area and therefore most of the friction. The thumb is a side brace and must not pinch. The ring and pinky trail and are ignored unless they are the only contacts, in which case the stroke is classified as unstable and the drive is halved. The off-hand is a passive guard: hovering within 40 mm of a mine freezes that mine's drift, giving the player a way to protect a fragile early ball without touching the prop.

Contact gating: a palm contact counts only if signed distance < 5 mm, age > 30 ms, and the local normal satisfies `n·ẑ_P > 0.5`. Side contacts (`n·ẑ_P < 0.3`) are braces. The system requires at least three palm contacts to enter `ROLL`; two contacts allow steering but no drive.

## 6. Input Kinematics & Signal Processing

At the palm contact, `n_p ≈ +ẑ_P`; at the tray, `n_t ≈ −ẑ_P`. No-slip at both contacts gives, for a rigid sphere of radius `r`:

`v_C = (v_palm + v_tray)/2 = v_palm/2` (tray stationary),

`ω = (n_p × v_palm)/(2r) = (ẑ_P × v_palm)/(2r)`.

At `|v_palm| = 100 mm/s`, `|v_C| = 50 mm/s` and `|ω| = 1.49 rad/s = 85.5 °/s`. The ball centre therefore traces half the palm's orbit: a 50 mm palm circle produces a 25 mm ball circle, which is the number shown in onboarding.

The game uses the *tracked* ball centre velocity `v_P` as truth. The model above supplies a predicted velocity used to bridge tracking gaps and to compute slip:

`s_palm = |v_palm,t − (v_C + ω × r·n_p)_t|`, `s_tray = |v_C + ω × r·n_t|`.

If `s_palm > 35 mm/s` or `s_tray > 25 mm/s` for 3 frames, the stroke is flagged slipping. Drive is scaled by `1 − clamp(s/80 mm/s, 0, 0.7)`, so a skidding palm still moves the ball but at reduced authority. Slip is common on fabric or with a cupped hand; the HUD shows a small "grip" bar.

**Circular-stroke quality.** Buffer 1.2 s of palm centroid positions in `T`. Fit a circle by algebraic least squares. Quality `Q = clamp(1 − |ρ − 25 mm|/8 mm, 0, 1) · clamp(1 − |f − 1.0 Hz|/0.4 Hz, 0, 1)`, where `ρ` is the fitted radius and `f` the orbit frequency. `Q > 0.6` grants a 1.15× drive multiplier and a soft "hum" that rises with quality. The quality term never overrides the physical ball; it only scales the assist that keeps the virtual sphere on the tracked path.

Signal chain: joints at 72 Hz → One Euro (mincutoff 1.8 Hz, beta 0.02, derivative cutoff 6 Hz) → palm centroid and plane → resample to 90 Hz → circle fit (1.2 s window, 108 samples) → `v_palm` → predicted `v_C`, `ω` → slip → drive scale → low-pass 8 Hz. Ball pose `P` is filtered separately with a 30 ms critically damped follow so the sticky sphere does not jitter.

Thresholds: rolling deadband 6 mm/s; lift 80 ms; brake 200 ms; mine-guard radius 40 mm. All classifiers debounce 30 ms and emit at most one event per 90 Hz tick.

## 7. Virtual World Layout & Coordinate Mapping

`V` origin is the tray centre; `x̂_V` points away from the player, `ŷ_V` right, `ẑ_V` up. The meadow is the rectangle `x ∈ [−150, 150] mm`, `y ∈ [−110, 110] mm`, giving a 300 × 220 mm field inside a 380 × 280 mm tray. The sticky sphere centre is `(P_x, P_y)` clamped to the meadow; its radius is `R`.

| Entity | V-space position | Notes |
|---|---|---|
| Sticky sphere | ball centre, 30 ms follow | Radius `R = 22·(M/100)^{1/3}` mm |
| Sheep | random in meadow, drift 8 mm/s | 9 mm disc, 12 mass |
| Fence | clustered in rows of 3–5 | 18 × 6 mm box, 40 mass |
| Barn | 5 fixed sites, ≥ 80 mm apart | 38 mm disc, 180 mass |
| Mine | 6 drifting at 20 mm/s | 11 mm disc, hazard |
| Meadow border | 300 × 220 mm | Fence rail visual |
| Season clock | HUD at 200 mm | 90 s countdown |

Mapping formulas: `R = clamp(22·(M/100)^{1/3}, 14, 60) mm`; `tier_ok = r_obj ≤ 0.85·R`; `capture = (d_2D ≤ R + 0.3·r_obj) && tier_ok`; `M ← M + m_obj`; `score ← score + round(m_obj · (1 + 0.15·min(chain,10)))`.

Scale: the meadow is 1:1 with the physical tray, so a 50 mm palm circle moves the collector 25 mm, about 8% of the field width. A full traverse takes 2.5–3.0 s at 100 mm/s palm speed, which is the pacing unit of a season (about 30 traverses).

## 8. Gameplay Systems & Rules

**Entities.** Sheep (r 9 mm, m 12), fences (18 × 6 mm, m 40), barns (r 38 mm, m 180), mines (r 11 mm, hazard). The sticky sphere starts at `M = 100`, `R = 22 mm`, and can immediately eat sheep and fences but not barns (`0.85·22 = 18.7 < 38`). A barn becomes edible at `R ≥ 44.7 mm`, i.e. `M ≥ 100·(44.7/22)³ = 839`.

**Spawning.** Season start seeds 60 sheep, 16 fences (4 rows), 5 barns, 6 mines. Respawn maintains ≥ 45 sheep, ≥ 10 fences, ≥ 3 barns, and exactly 6 mines; a new entity enters at a random edge point at least 60 mm from the sticky sphere and walks/drifts inward. Spawn budget is capped at 120 entities to bound draw cost.

**Capture.** For each entity, `d = |p_entity − p_sticky|` in `T`. If `d ≤ R + 0.3·r_obj` and `r_obj ≤ 0.85·R`, capture begins: the entity spirals to the sphere centre over 0.25 s, mass is added at the midpoint, and the entity is removed. Capture is idempotent; an entity can only be captured once.

**Chain and score.** `chain` increments on each capture; the timer resets to 1.2 s. Multiplier `1 + 0.15·min(chain,10)`, capped at 2.5. Score for a capture is `round(m_obj · multiplier)`. A mine hit resets `chain = 0`, and a gap > 1.2 s resets it too. `barn_bonus = 150` per barn, awarded once.

**Mines.** A mine triggers when `d ≤ R + 11 mm`. Effect: `M ← max(60, 0.88·M)`, `R` recomputes, sticky capture is disabled for 0.8 s, chain resets, mine is consumed and respawned at the far edge after 2.0 s. The mass floor of 60 prevents an unrecoverable death spiral but still leaves the ball below its starting size.

**Quota and win/lose.** Each season has a mass quota: 700, 1000, 1300, 1600, 1900 for levels 1–5. At 90 s, `M ≥ quota` clears the season. `M < 60` at any time ends the run immediately (the ball has been stripped). Three cleared seasons in the default session are a win; an uncleared season is a loss with a retry at the same level.

**Worked season.** 90 s, 100 mm/s average palm speed, 55% capture uptime, mean chain 5 (multiplier 1.75). A player captures ~70 sheep and 12 fences: mass gain `70·12 + 12·40 = 1320`, final `M = 1420`, score `≈ 70·21 + 12·70 = 2310` plus 2 barns (300) = 2610. Quota 1300 is cleared with margin; quota 1600 requires barns.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load meadow, start tracker | PRESENT |
| PRESENT | prop registered | RMS < 0.5 mm | Show tray outline | TUTORIAL |
| TUTORIAL | 3 circular strokes | `Q > 0.6` twice | Confirm two-surface roll | SEASON_INTRO |
| TUTORIAL | 45 s elapsed | — | Skip with reduced assist | SEASON_INTRO |
| SEASON_INTRO | 2.0 s timer | season ≤ 3 | Seed entities, show quota | ROLL |
| ROLL | tick | `M ≥ 60` | Update sphere, capture, drift | ROLL |
| ROLL | capture event | tier_ok | Spiral, add mass, chain++ | CHAIN |
| CHAIN | 0.25 s timer | — | Award score, check tier-up | ROLL |
| ROLL | mine contact | d ≤ R+11 mm | Mass ×0.88, stun 0.8 s, chain = 0 | MINE_HIT |
| MINE_HIT | 0.8 s timer | M ≥ 60 | Resume capture | ROLL |
| ROLL | season clock = 0 | M ≥ quota | Bonus, score tally | SEASON_END |
| ROLL | season clock = 0 | M < quota | Slow zoom, "quota missed" | RESULT |
| ROLL | M < 60 | — | Ball collapse VFX | GAME_OVER |
| SEASON_END | 3.0 s timer | season < 3 | Next quota, rest 15 s | REST |
| REST | 15 s timer | — | Hand-open overlay | SEASON_INTRO |
| RESULT | tap retry | — | Reset mass to 100 | SEASON_INTRO |
| ANY | ball off tray | — | Pause, recovery ring | OFF_TRAY |
| OFF_TRAY | re-registered < 5 s | — | Restore mass, 1.0 s grace | previous |

Season clocks are wall-clock driven and are not extended by hitches. Capture and mine checks run on the 90 Hz tick after the ball pose is filtered.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 ball | Sphere r = 33.5 mm | Tracked | Real prop; no simulation |
| C1 tray | Plane | Static | `z_T = 0` |
| C2 sticky | Disc, dynamic radius | Follows ball | 30 ms lag |
| C4 sheep/mine | Disc | Drift 8 / 20 mm/s | 2D on tray |
| C5 fence | Box proxy | Static | 18 × 6 mm |
| C6 barn | Disc r = 38 mm | Static | 5 sites |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C2 × C4 | `d ≤ R + 0.3r`, `r ≤ 0.85R` | Capture or mine trigger |
| C2 × C5 | same with footprint radius 12 mm | Capture |
| C2 × C6 | same with r = 38 mm | Capture at `R ≥ 44.7` |
| C0 × C1 | tracked height vs tray | Rolling constraint; no virtual response |
| C3 × C0 | signed distance < 5 mm | Palm contact |
| C4 × C4 | ignored | Entities overlap visually, no physics |
| C4 × C5/C6 | ignored | Static scenery |

**Materials.** Ball = felt over rubber: µ_tray 0.62, µ_skin 0.58, restitution 0.72, rolling resistance 0.012. Sheep = "soft hologram": no bounce, 0.25 s spiral. Fence = "wood": 0.1 s clatter then spiral. Barn = "heavy": 0.45 s strain, camera 0.3 mm shake. Mine = "spike": 0.0 restitution, consumed on trigger.

**CCD.** At a peak ball speed of 400 mm/s the ball moves 4.4 mm/tick. The smallest capture radius is 14 mm and the smallest gap between a sheep and a mine is 22 mm, so discrete tests are safe; a swept segment test is still used for C2 × C4 to prevent a fast pass through a sheep at the capture boundary.

**Rolling force.** The palm must supply `F = (I_eff/r²)·a_C + C_rr·m·g` to accelerate the ball, where `I_eff = I + m·r² = 9.10 × 10⁻⁵ kg·m²` and `I_eff/r² = 0.081 kg`. With `m = 0.058 kg`, `r = 0.0335 m`, the rolling resistance is `0.012 × 0.058 × 9.81 = 0.0068 N`. A 0.10 N tangential force therefore accelerates the ball at `(0.10 − 0.0068)/0.081 = 1.15 m/s²`, reaching 100 mm/s in 0.087 s. That is why the stroke feels light: the ball's effective inertial mass is only ~81 g.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → tray/table mesh → meadow ground → far entities → sticky sphere back shell → ball → sticky sphere front shell → near entities → hand mesh → HUD.

**Occlusion cases.**

- **O1 — Ball over the meadow.** The physical ball occludes the meadow and any entity behind it. A 0.4 mm depth bias keeps the seam texture from z-fighting with the sticky shell.
- **O2 — Palm over the ball.** The hand mesh is drawn after the ball and wins depth; the palm's contact patch gets a soft 2 mm shadow so it reads as resting on the ball, not floating.
- **O3 — Entity behind the sticky shell.** Entities inside the sticky radius but behind the ball are rendered into the shell's refraction-free inner layer, dimmed 40%; entities outside are fully visible until captured.
- **O4 — Barn behind the hand.** A barn occluded by the palm is invisible; its 38 mm capture ring is still drawn on the tray so the player can judge the approach.
- **O5 — Mine under the sticky sphere.** A mine entering the sticky radius before triggering is drawn as a red spike silhouette through the translucent shell at 0.6 alpha; once triggered, it is fully occluded by the flash.
- **O6 — HUD versus tray.** The season clock and mass bar are drawn at 200 mm and depth-tested against the hand; a raised palm covers them and they fade to 30%, never draw through.

**VFX budgets.** Entities ≤ 120 (60 sheep, 16 fences, 5 barns, 6 mines, plus respawns); capture spirals ≤ 12 concurrent; sticky shell 1 draw call with a 0.22 alpha rim; meadow grass 1 instanced draw; particles ≤ 300. The shell uses a screen-space "sticky noise" shader with no refraction so the real ball remains legible inside it.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Roll start | ball contact | Low felt rumble, pitch ∝ speed | Dust motes at the contact line |
| Circular stroke quality | palm | Soft hum 180 Hz, gain ∝ Q | Hoop glows gold |
| Sheep capture | entity | Bleat pitched down 1 octave, 120 ms | Wool puff, spiral trail |
| Fence capture | entity | Wooden clank ×2 | Splinter particles |
| Barn capture | entity | Deep strain + roof crack, 600 ms | Dust ring, camera 0.3 mm shake |
| Chain step | sticky sphere | Chime, +1 semitone per link | Multiplier number pops |
| Chain break | HUD | Dry click | Chain bar drains |
| Mine guard | off-hand | Soft 440 Hz hold tone | Mine outline freezes blue |
| Mine trigger | mine | Spike hiss + 60 Hz boom | Red flash, mass numbers fly out |
| Tier up | sticky sphere | Rising major third | Sphere pulses, new size ring |
| Quota met | HUD | 3-note fanfare | Quota bar fills green |
| Season end | meadow | Wind-down sweep | Meadow folds into the ball |
| Off tray | ball | Rubber squeak | Recovery ring on tray |

Every event maps to one row. The rolling rumble is continuous and is the player's main speed cue; the chain chime is the main score cue.

## 13. Difficulty Progression & Tuning

| Level | Duration (s) | Quota (mass) | Sheep | Fences | Barns | Mines | Mine drift (mm/s) |
|---|---|---|---|---|---|---|---|
| 1 | 90 | 700 | 60 | 16 | 5 | 4 | 12 |
| 2 | 90 | 1000 | 60 | 16 | 5 | 5 | 16 |
| 3 | 90 | 1300 | 60 | 16 | 5 | 6 | 20 |
| 4 | 90 | 1600 | 55 | 14 | 4 | 7 | 24 |
| 5 | 90 | 1900 | 50 | 12 | 3 | 8 | 28 |

Named constants: `M0 = 100`, `R0 = 22 mm`, `R_MAX = 60 mm`, `MASS_FLOOR = 60`, `CHAIN_WINDOW = 1.2 s`, `CHAIN_STEP = 0.15`, `CHAIN_CAP = 2.5`, `MINE_LOSS = 0.12`, `STUN = 0.8 s`, `SEASON = 90 s`, `BARN_TIER = 44.7 mm`.

Adaptive rule: if the player clears by > 35%, raise the next quota by 10% (cap 2100); if they miss by > 20%, lower it by 10% (floor 650). Mine count is never adapted downward; the dodge is the skill. The mass floor of 60 is fixed so a bad start is recoverable but still punishing.

## 14. Edge Cases & Failure Recovery

- **Ball rolls off the tray.** `OFF_TRAY` pauses the season clock, draws a recovery ring at the last in-bounds point, and waits up to 5 s. Re-registration restores mass and gives 1.0 s of mine immunity. If the ball is lost, the run resets to the season start.
- **Palm loses contact mid-stroke.** The ball coasts with rolling resistance `a = −C_rr·g = −0.118 m/s²`, decaying 100 mm/s to rest in ~0.85 s. No capture is possible without palm contact; the sticky field is tied to `ROLL`.
- **Cupped hand / pinch.** Detected when ≥ 2 side contacts have `n·ẑ_P < 0.3` and the palm contact patch area drops below 60% of nominal. Drive halves and a "flat palm" coach appears. The ball is not dropped.
- **Tray on soft lap.** `T` compliance > 4 mm/N reduces the two-surface gain by 15% and widens the slip threshold to 45 mm/s. The HUD shows a "soft surface" icon.
- **Ball spinning in place.** If `|v_C| < 6 mm/s` but `|ω| > 3 rad/s` for 1.5 s, the player is spinning the ball under a stationary palm. Capture is disabled (the sticky field needs translation) and a "roll, don't spin" cue plays.
- **Mine at the spawn point.** Respawn rejects positions within 80 mm of the sticky sphere and within 30 mm of a barn. If no valid point exists in 20 tries, the mine waits 1.0 s and retries.
- **Two entities captured on the same tick.** Resolve in ascending entity ID; both count, chain increments twice, and the second capture uses the already-updated `R`. Deterministic and fair.
- **Mass crosses the barn tier mid-chain.** The tier check uses `R` at the start of the tick, so a barn cannot be captured by the growth caused by the same tick's sheep. This avoids order-dependent surprises.
- **Player uses the off-hand to push the ball.** Only the dominant palm drives; off-hand contact on the ball triggers the pinch coach and contributes no velocity.
- **Tracking loss > 2 s.** Season clock runs at 0.5×, sticky field freezes, and the game prompts for the ball. Re-acquisition resumes at 1.0 s grace.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — sphere pose and seam orientation at 72 Hz.
- `PalmContact` — palm plane, contact patch, `v_palm`, side-contact classification.
- `TwoSurfaceKinematics` — `v_C = v_palm/2`, `ω`, slip residuals, drive scale.
- `StrokeAnalyzer` — 1.2 s circle fit, quality `Q`, frequency/radius estimates.
- `StickySphere` — mass, radius, tier, capture query, chain timer.
- `MeadowDirector` — seeding, respawn, entity drift, mine guard.
- `HarvestResolver` — capture, mass, score, tier-up, mine trigger.
- `SeasonStateMachine` — §9 table, 90 s clock, quota.
- `FeedbackRouter` — event → audio + VFX, coverage assertion.

**Event flow.** `PropTracker` → `TwoSurfaceKinematics`; `PalmContact` → `TwoSurfaceKinematics` → `StickySphere`; `StrokeAnalyzer` → `StickySphere` (drive multiplier); `StickySphere` → `HarvestResolver`; `MeadowDirector` → `HarvestResolver`; `HarvestResolver` → (`SeasonStateMachine`, `FeedbackRouter`).

**Pseudocode 1 — rolling and capture tick.**

```
function tick(dt):
    pose = PropTracker.latest()
    palm = PalmContact.latest()
    if pose.mode == LOST or palm.contacts < 3:
        drive = 0
    else:
        v_pred = palm.v_tangent / 2
        slip = abs(palm.v_tangent - (pose.v_C + cross(omega, r*n_palm)))
        drive = 1 - clamp(slip / 80mm_per_s, 0, 0.7)
        drive *= (1.15 if StrokeAnalyzer.Q > 0.6 else 1.0)

    p_sticky = follow(pose.p_xy, tau=30ms)
    M = clamp(M, 60, inf)
    R = clamp(22mm * pow(M/100, 1/3), 14mm, 60mm)
    for e in Meadow.entities_near(p_sticky, R + 40mm):
        d = length(e.p_xy - p_sticky)
        if e.kind == MINE:
            if d <= R + 11mm: trigger_mine(e)
        else:
            if d <= R + 0.3*e.r and e.r <= 0.85*R:
                capture(e, at=now)
```

**Pseudocode 2 — capture and chain.**

```
function capture(e, now):
    if e.captured: return
    e.captured = true
    if now - last_capture <= 1.2s: chain += 1
    else: chain = 0
    last_capture = now

    mass_gain = e.mass
    M += mass_gain
    mult = 1 + 0.15 * min(chain, 10)
    score += round(mass_gain * mult)
    if e.kind == BARN: score += 150

    old_R = R
    R = clamp(22mm * pow(M/100, 1/3), 14mm, 60mm)
    if floor(R/0.85) > floor(old_R/0.85):
        Feedback.play(TIER_UP, p_sticky)
    Feedback.play(CAPTURE_##e.kind, e.p)
    animate_spiral(e, 0.25s)
```

**Determinism.** Entity seeding and respawn use a PCG32 keyed by `(session, season)`. Capture order is entity-ID sorted within a tick, so mass and score are reproducible from a recorded ball trace.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Two-surface law | Palm sweep 100 mm/s along `+x_V`, r = 33.5 mm, 90 ticks | `v_C = 50 mm/s ± 2%`; `|ω| = 1.49 rad/s ± 3%`; axis `+y_V ± 2°` |
| AT-02 | Stroke geometry | Drive a 50 mm diameter palm circle at 1.0 Hz | Ball-centre circle = 25 mm ± 3 mm diameter; `Q ≥ 0.9` |
| AT-03 | Growth law | Set `M = 100, 800, 6400` | `R = 22, 44.0, 88 → capped 60 mm` (tolerance ±0.3 mm) |
| AT-04 | Tier gate | Ball at `R = 44.0 mm`, barn at `d = 30 mm` | Barn not captured; at `R = 44.8 mm` it is captured |
| AT-05 | Capture boundary | Sheep centre at `d = R + 2.7 mm` and `R + 3.3 mm` | First passes (0.3·9 = 2.7), second fails |
| AT-06 | Mine damage | `M = 1000`, trigger one mine | `M = 880 ± 1`; stun 0.8 s; chain = 0; capture disabled |
| AT-07 | Mass floor | Trigger 10 mines from `M = 200` | `M` reaches the 60 floor and never goes below it; `GAME_OVER` does not fire while `M = 60` |
| AT-08 | Chain multiplier | Capture 5 sheep in 1.0 s gaps, then wait 1.5 s | Multipliers 1.0, 1.15, 1.30, 1.45, 1.60; reset after 1.2 s |
| AT-09 | Season timing | Run a season with a 90 s scripted trace | Clock hits 0 at 90.0 s ± 1 tick; quota evaluated once |
| AT-10 | Respawn floor | Capture 40 sheep in 10 s | Sheep count stays ≥ 45 after 1.5 s; no spawn within 60 mm of sphere |
| AT-11 | Ball occlusion | Place a sheep 20 mm behind the ball; read depth | Sheep contribution 0 inside the 33.5 mm disc; ring visible around it |
| AT-12 | Off-tray recovery | Teleport the ball 250 mm off the tray for 3 s | Clock pauses; recovery ring shown; mass restored on re-entry |
| AT-13 | Determinism | Replay a 90 s trace twice with the same seed | Identical mass, score, capture order, and entity positions |
| AT-14 | Audio coverage | Enumerate `FeedbackEvent` | 13/13 mapped; capture variants all resolve to a clip |

Headless tests use synthetic palm skeletons and a synthetic ball trace; AT-11 reads the depth buffer, and AT-13 compares serialized state hashes at every 1 s boundary.
