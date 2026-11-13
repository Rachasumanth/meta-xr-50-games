# Atlas Defense

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Atlas Defense |
| Slug | `atlas_defense` |
| Physical prop | Orange (default), apple, or tennis ball; true sphere; 76 mm nominal diameter (r = 38 mm); orange 150 g, apple 180 g, tennis 58 g; matte peel or felt surface |
| Family / sub-shape | True sphere, palm-scale (6–12 cm class), 360° free rotation |
| Scale class | Prop 1:1; planet visual 1:1 (r = 38 mm); meteor engagement envelope 340 mm radius, inside the < 40 cm tabletop class |
| Source concept quote | "Cradle an orange in your left palm and spin it with your right fingertips. The fruit becomes a living planet: five cities glow on its skin, and a single orbital shield satellite hangs above the north pole. Roll the world so the satellite's 25° shield cone intercepts each incoming meteor before it strikes a city." |
| Physical input | Left palm cradle (static) + right fingertip rolling strokes (index, optionally index+middle) |
| Session length | 6 min: onboarding 60 s + 5 waves (≈34 s each) + 4 intermissions (8 s) + results 30 s |
| Lose condition | Four of five cities reach 0 HP |

## 1. Fantasy & Core Loop

You are the last geoplanetary warden. The orange resting in your left palm is not fruit; it is Atlas, a living world of five beacon-lit cities, and the tiny satellite parked above its north pole is the only shield generator left. Meteors fall from every direction of the room around you. You cannot move the planet through space — you can only *turn it*, rolling the whole world under your right fingertips until the polar shield cone stares down the incoming rock. The satellite is small and the sky is huge; every interception is a race between your fingertips and a two-second countdown.

1. **READ (2.0 s).** A meteor spawns at 340 mm and paints a dashed incoming ray. A ring at the impact point pulses in the meteor's colour; a countdown arc collapses toward the planet. Duration fixed at 2.0 s.
2. **TURN (0.4–1.6 s).** The player strokes the globe to bring `+ẑ_P` onto the meteor direction. Each 60 mm stroke yields roughly 90.5° of roll at r = 38 mm. Long tumbles require two strokes because a single rolling contact cannot spin the ball about its own contact normal.
3. **LOCK (0.25 s).** When the shield axis is within 20° of the meteor direction, a lock ring fills for 250 ms. Releasing alignment beyond 25° cancels the lock (5° hysteresis).
4. **INTERCEPT (0.10 s).** At the impact timestamp the meteor is tested against the 25° cap. A locked meteor inside the cap detonates on the shield with a white-green flare; a meteor outside the cap strikes the surface and damages the nearest city.
5. **RESOLVE (0.45 s).** Score and combo update; the city beacon flashes or the shield satellite re-arms. The next meteor is already inbound during this resolve window.

A single meteor occupies 2.8–3.2 s including resolve; 12 meteors per wave spawn 2.6 s apart with two to four concurrent, so a wave lasts 32–38 s. Intermission is 8 s.

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

`P` origin is the geometric centre of the prop; poles are `±ẑ_P`; radius `r = 38.0 mm` (orange default). The rolling law is `ω = (n × v_t)/r`, where `n` is the outward surface normal at the fingertip contact and `v_t` is the fingertip velocity relative to the prop centre, projected into the tangent plane. At `|v_t| = 120 mm/s` this yields `ω = 3.16 rad/s = 181 °/s`; a 60 mm stroke yields 1.579 rad = 90.5°. `V` is anchored to `T` at a fixed cradle centre `C₀` located 180 mm in front of the seated player's sternum and 55 mm above the table plane. The planet centre is drawn at `P` transformed into `V`, clamped to ±15 mm from `C₀`. Deviations:

1. **Split anchoring.** `V` is table-anchored so the meteor sky does not spin with the ball; cities are `P`-fixed and meteors are `V`-fixed. This split is the whole game: rotating `P` under a stable `V` aims the shield.
2. **Orientation authority.** A featureless true sphere has no observable yaw, so `q_P` is a fusion of peel micro-texture odometry (60% weight) and the integrated contact `ω` (40% weight), with drift bounded by §4.
3. **No prop rigid body.** The real prop is tracked, not simulated; virtual physics covers meteors, shield cap, and debris only.
4. **Prop depth proxy.** A 38 mm sphere is written into the depth buffer at the tracked `P` so meteor trails behind the physical fruit are correctly occluded (case O1).

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated, both forearms supported | Removes shoulder load during 6 min of precision rolling |
| Table height | 680–740 mm | Keeps the cradle 50–70 mm below elbow height |
| Cradle position | 260–340 mm from eyes, 20–40 mm below eye line | Comfortable 2.5 D focus at a 38 mm object |
| Left palm | Supinated, fingers gently cupped, 3–5 contact points | A stable cup with the smallest possible friction lever |
| Right hand | Hovering 30–60 mm above the globe, wrist 10–25° extended | Fingertip reach covers the front 200° of the sphere |
| Stroke envelope | 24 cm × 18 cm × 8 cm above the table | Covers full tumbles and re-grips |
| Session | 6 min; 8 s rests every wave | Sustained fingertip rolling is a low-load but repetitive task |
| Break cue | 20 s hand-open overlay after wave 3 | Prevents flexor fatigue accumulation |

The left palm is an *open cup*, not a grip: the contact normals at the four finger bases point within 25° of the palm normal, so the friction lever arm is at most `r·sin 25° = 16.1 mm`. This makes yaw about the palm normal easy and tumbling about a horizontal axis costlier. The design uses that asymmetry deliberately: azimuth changes are cheap, elevation tumbles are the skill move.

Right-hand strokes longer than 80 mm are discouraged by a soft audio cue because the fingertip loses reliable contact beyond the front 100° of longitude. Re-gripping is free: lifting the finger does not alter `q_P`, and the last orientation is held by palm friction.

## 4. Prop Geometry & Registration

| Property | Orange (default) | Apple | Tennis ball |
|---|---|---|---|
| Diameter | 76.0 mm | 72.0 mm | 67.0 mm |
| Radius `r` | 38.0 mm | 36.0 mm | 33.5 mm |
| Mass | 150 g | 180 g | 58 g |
| Inertia `I = 2/5·m·r²` | 8.66 × 10⁻⁵ kg·m² | 9.33 × 10⁻⁵ kg·m² | 2.60 × 10⁻⁵ kg·m² |
| Skin friction µ | 0.55 | 0.50 | 0.62 (felt) |
| Restitution (drop test) | 0.20 | 0.18 | 0.72 |

The collider is a perfect sphere at `P` with `r` from the table above. Because the prop is rigid relative to the 5 mm contact threshold, no deformation term enters the input model. The visual shell is a 96 × 48 UV sphere; the city layer is a separate 2048 × 1024 equirectangular texture atlas addressed by `(φ, λ)`.

**Surface-feature atlas (P-fixed, rotates with the globe):**

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| City A "Meridian" | +18° | 30° | HP target; beacon cyan |
| City B "Sable" | −22° | 96° | HP target; beacon amber |
| City C "Highvault" | +41° | 204° | HP target; beacon violet |
| City D "Tidewash" | −8° | 278° | HP target; beacon teal |
| City E "Lastlight" | +6° | 341° | HP target; beacon rose |
| Shield cap | +65° to +90° | all | 25° half-angle around +ẑ_P; intercept volume |
| Prime meridian | all | 0° | Orientation ruler, 1 mm white line |
| Equator | 0° | all | Tumble reference, 1 mm dim line |
| City exclusion zone | any | ±12° around each city | Cities are never closer than 24° to each other |
| Impact scar | any | any | Decal spawned at the strike point, persists one wave |

City angular separation check: the five cities above are pairwise separated by 49°–121°, so "nearest city" is unambiguous by at least 24° at any impact point.

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 38 mm at `P` | Physical prop / depth proxy |
| C1a–e | Spherical disc | 6.0 mm radius, on surface at each city | City damage target |
| C2 | Spherical cap | 25° half-angle about +ẑ_P | Shield intercept volume |
| C3 | Capsule | r = 1.8 mm, length 12 mm, at each fingertip | Contact generator |
| C4 | Sphere cluster | 5 × r = 4 mm palm pads | Cradle contact |
| C5 | Sphere | r = 3.0–5.0 mm | Meteor visual/collision |

Registration: the player presents the prop 0.9–1.4 m from the head and rotates it through ≥ 150° for 1.2 s, then holds still 0.4 s. The silhouette fitter solves centre and radius by least squares; acceptance requires RMS residual < 0.5 mm, radius error < 0.2 mm, and ≥ 24 rim samples. Micro-texture odometry needs ≥ 40 tracked peel features; if the prop is too uniform, the session falls back to contact-only rotation integration.

Re-acquisition after loss: search a 150 mm cube around the last `P` for 1.5 s. Success requires 10 rim samples with RMS < 0.9 mm over 6 consecutive frames; the pose snaps with a 100 ms ease-in. During loss, `q_P` is frozen after 200 ms (no free spinning), and meteors are held in a 0.75× time dilation so the player is not punished for a tracker dropout.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long or axis) | Semantic output |
|---|---|---|---|---|
| I5.1 | Palm cradle | `L_palm` pads, `L_thumb_tip`, `L_index_tip`, `L_middle_tip` | φ ≤ −30°, any λ; ≥ 3 valid contacts | `HOLD` state, prop supported, `q_P` retained |
| I5.2 | Index roll | `R_index_tip` | φ −55° to +55°, λ within 100° of the player-facing meridian | 2-DoF tangential velocity `v_t` |
| I5.3 | Two-finger roll | `R_index_tip` + `R_middle_tip`, separation 8–22 mm | same band; normals within 30° | `v_t` with 1.55× gain |
| I5.4 | Precision nudge | `R_index_tip` | contact 60–130 ms, travel 3–10 mm | 6.0° ± 1.5° impulse about `n × d` |
| I5.5 | Re-grip | `R_index_tip` lift + re-contact | any front band | No rotation; orientation held |
| I5.6 | Brace | `R_ring_tip`, `R_pinky_tip` contact on globe | any | Audio-only thud, no command |
| I5.7 | Two-hand stabilise | `L_thumb_tip` + `L_index_tip` additionally touch the north cap | φ ≥ +55° | Damps `ω` by 60%, cancels meteor lock |
| I5.8 | Release cradle | all `L_*` contacts lost > 120 ms | — | `DROP` state, physics fall prediction |

**Per-finger roles.** Left thumb, index, middle, and ring form the static cup; the left pinky is ignored. Right index is the primary rolling effector; right middle is a boost finger for fast tumbles; right ring and pinky are brakes/braces and never command rotation. The left hand never rotates the globe in this game; touching the north cap with the left hand is an explicit *stabilise* action that arrests spin, useful when a meteor is directly overhead.

Contact gating: a roll contact is valid only when signed distance < 5 mm, contact age > 30 ms, and the contact normal satisfies `n·(P_eye − P_centre) > 0.2` (front-facing). Contacts on the back hemisphere are accepted only for the palm cup, with their normals ignored.

## 6. Input Kinematics & Signal Processing

The authoritative rotation is the tracked `q_P`. The contact model supplies a predictive `ω` used to bridge the 72 Hz tracking stream onto the 90 Hz physics tick:

`ω = (n × v_t) / r`, with `v_t = (v_joint − v_centre) − [(v_joint − v_centre)·n]·n`.

Here `v_joint` is the One-Euro-filtered fingertip velocity in `W`, and `v_centre` is the prop-centre velocity. For a static cradle `|v_centre| < 8 mm/s`, so `v_t ≈ v_joint,t`. The two estimates are fused: `q_P ← slerp(q_P_tracked, q_P + ω·dt, 0.35)` only when the tracker reports `TRACKING_GOOD`; when the tracker reports `PREDICTED`, the weight rises to 0.85.

`ω` decomposition for aiming feedback: `ω_pitch = ω·x̂_V`, `ω_yaw = ω·ŷ_V`, `ω_roll = ω·ẑ_V`. The game does not steer directly from these; they drive the "spin compass" HUD and the fingertip trail.

Signal chain: raw joints → One Euro filter (mincutoff 1.0 Hz, beta 0.015, derivative cutoff 7 Hz) → finite difference at 90 Hz → tangent projection → `ω` → second-order Butterworth low-pass 11 Hz → saturation at 720 °/s. A dead zone of 9 mm/s is applied to `v_t` with an 11 mm/s release threshold; below it the globe is considered held still. This rejects physiological tremor (≈ 2–4 mm/s at the fingertip) while preserving deliberate 30 mm/s nudges.

Slip handling: residual `s = |v_joint,t − (v_centre + ω × (r·n))_t|`. If `s > 30 mm/s` for 3 consecutive frames, the contact is flagged slipping and `ω` is blended 50/50 with the previous valid estimate; if slip persists > 140 ms the stroke is rejected (a "scrub") and the globe is not rotated by that contact. Maximum single-stroke rotation is capped at 140° to prevent a flick from teleporting the shield across the sky.

Classifier definitions: **roll** = `|v_t| > 9 mm/s` for ≥ 60 ms; **nudge** = contact 60–130 ms with 3–10 mm travel; **stabilise** = two left contacts on `φ ≥ +55°` for ≥ 150 ms; **release** = all left contacts lost for 120 ms. All classifiers are debounced 30 ms and emit at most one event per tick. Events are timestamped in the 90 Hz physics clock by linear interpolation of the two adjacent 72 Hz samples.

## 7. Virtual World Layout & Coordinate Mapping

`V` origin is `C₀` = 180 mm in front of the seated sternum, 55 mm above the table plane, with `x̂_V` pointing away from the player, `ŷ_V` right, `ẑ_V` up (right-handed conversion from `T`). The planet centre is `c_V = clamp(P_centre − V_origin, ±15 mm)` per axis, and the planet radius is the game radius (38 mm) regardless of the small tracking offset.

| Entity | V-space position | Notes |
|---|---|---|
| Planet centre | `c_V`, clamped ±15 mm | Visual sphere r = 38 mm |
| Shield satellite | `c_V + 44 mm · ŝ`, `ŝ = q_P·ẑ_P` | Billboard, 4 mm, always on the pole |
| Shield cone | apex at `c_V + 44 mm·ŝ`, half-angle 25° | Rendered as a translucent cap |
| Meteor spawn shell | radius 340 mm about `c_V` | Full sphere, biased away from the player's head |
| City beacons | `c_V + 38.8 mm · q_P·p̂_i` | 1.8 mm emissive dot + label |
| HUD ring | plane ⟂ view at 260 mm from the eye | Never intersects the planet |
| Camera | `V` origin + head pose | Planet at 26–34 cm |

Mapping formulas: a meteor direction `d̂_V` converts to prop coordinates by `d̂_P = q_P⁻¹·d̂_V`. The shield axis in `V` is `ŝ = q_P·ẑ_P`. The alignment angle is `θ = acos(clamp(ŝ·d̂_V, −1, 1))`. The impact point is `p_imp = c_V + 38 mm·d̂_V`; the nearest city minimises `acos(clamp((q_P·c_i)·d̂_V, −1, 1))`, where `c_i` is the city unit vector in `P`.

Scale mapping: the meteor spawn radius is 340 mm, about 8.9 planet radii. A meteor crosses the 302 mm gap in 2.6 s at 116 mm/s, which is slow enough for a deliberate two-stroke tumble and fast enough that a player cannot simply chase every rock with a lazy wrist roll. A 20° alignment error at the planet surface is 13.3 mm of arc, comparable to the 6 mm city disc, so the 20° gate is visually legible.

## 8. Gameplay Systems & Rules

**Entities.** Five cities (HP 2 each); one shield satellite with a 25° cap; one meteor stream of 12 per wave; debris particles (cosmetic). Cities never move in `P`. Meteors are kinematic on straight rays.

**Spawn rule.** At wave start, `N = 12` directions are drawn from a stratified Fibonacci distribution over the full sphere, then rejected if within 12° of the player's eye direction (so nothing spawns behind the headset's own occlusion). Spawn interval `T_spawn = 2.6 s`; concurrency cap `C = 2 + floor((wave − 1)/2)`, max 4. A new meteor waits if the cap is reached.

**Alignment and lock.** `θ = acos(ŝ·d̂)`. Lock accumulates while `θ < 20°`; it resets to 0 when `θ > 25°`. Lock is true at 250 ms. The shield cap is 25°, giving a 5° guard band so a locked meteor is always physically inside the cap unless the player moves during the final tick.

**Interception.** At `t_impact` (the §13 flight time after spawn), if `LOCK` is true and `θ < 25°`, the meteor is intercepted. Otherwise it strikes at `p_imp`. If `LOCK` is false but `θ < 25°`, the meteor still strikes: the satellite needs the 250 ms charge, not mere proximity.

**Damage.** Each impact removes 1 HP from the nearest city. City HP is 2. At 0 HP the city goes dark and emits smoke; it cannot be targeted again but remains visible. The wave continues until all 12 meteors are resolved. Fail when 4 cities are at 0 HP.

**Repair.** During the 8 s intermission, every damaged but surviving city regains 1 HP (cap 2). Destroyed cities are not rebuilt. This makes early leaks survivable but not free.

**Score.**

`intercept_score = round(100 · (1 + 0.25·min(combo, 8)) · (1 + 0.5·(1 − θ/20°)))`

`impact_penalty = 75 + 25·(city_index_already_damaged ? 1 : 0)`

`wave_bonus = 250 · surviving_cities` if all 12 are intercepted, else 0.

Combo increments on every interception and resets to 0 on every impact. The formula reads the combo value *before* the increment, so the first interception uses multiplier 1.0 and the fifth consecutive uses 2.0. The precision term is capped at 1.5× for a perfect 0° alignment, so perfect play is worth 150 × multiplier per meteor. The run score subtracts `impact_penalty` for every impact and is floored at 0; the penalty is diagnostic as well as punitive, so the results screen can show leaks separately from missed alignment.

**Win/lose.** Survive five waves (60 meteors) with at least 2 cities alive to win. Four dead cities is an immediate loss. There is no timer loss; the meteor schedule is the clock.

**Example wave math.** 12 meteors, 2 concurrent, 2.6 s spawn spacing, 2.0 s telegraph, 0.45 s resolve → last spawn at 28.6 s, last impact at 30.6 s, wave ends ≈ 31.1 s. With 8 s intermission, five waves ≈ 195 s plus onboarding, comfortably inside 6 min.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load city atlas, start tracker | PRESENT |
| PRESENT | prop registered | RMS < 0.5 mm | Freeze `q_P`, show "cradle" ghost | CALIBRATE |
| CALIBRATE | left cup ≥ 3 contacts for 1.0 s | — | Set `C₀` from `P`, zero combo | WAVE_INTRO |
| WAVE_INTRO | timer 2.5 s | wave ≤ 5 | Show wave number, spawn schedule | SPAWN |
| SPAWN | `t ≥ next_spawn` | active < C and spawned < 12 | Instantiate meteor, start 2.0 s telegraph | INBOUND |
| INBOUND | tick | `t < t_impact` | Update θ, lock accumulator, HUD | INBOUND |
| INBOUND | `t ≥ t_impact` | lock && θ < 25° | Intercept VFX, score, combo++ | RESOLVE |
| INBOUND | `t ≥ t_impact` | else | Impact VFX, nearest city −1 HP, combo = 0 | RESOLVE |
| RESOLVE | timer 0.45 s | — | Despawn meteor, check city count | SPAWN or WAVE_END |
| WAVE_END | spawned = 12 and active = 0 | deaths < 4 | Tally, repair cities +1 | INTERMISSION |
| WAVE_END | deaths ≥ 4 | — | Darken globe, dirge | GAME_OVER |
| INTERMISSION | timer 8.0 s | wave < 5 | Show repair, next wave | WAVE_INTRO |
| INTERMISSION | timer 0 s | wave = 5 | Results panel | RESULTS |
| GAME_OVER | tap "retry" | — | Reset cities, keep wave 1 | WAVE_INTRO |
| ANY | tracker loss > 2.0 s | — | Pause, 0.75× dilation, "show the globe" | RECOVERY |
| RECOVERY | re-acquired | RMS < 0.9 mm | Resume, 0.5 s grace | previous |

Timers are advanced on the 90 Hz fixed tick. `t_impact` is latched at spawn and never extended by frame drops; if the app stalls, the state machine processes at most 2 substeps and then clamps `dt` to 22.2 ms.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 prop | Sphere r = 38 mm | Tracked `P` | Depth proxy only; not simulated |
| C2 shield cap | Spherical cap 25° | Rigid to `q_P` | Purely angular test |
| C5 meteor | Sphere r = 3–5 mm | Kinematic ray | Speed 116 mm/s |
| C1 city | Disc r = 6 mm | Fixed in `P` | Damage receiver |
| C3 fingertip | Capsule r = 1.8 mm | Tracked | Contact generator |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C5 × C2 | angle between `d̂` and `ŝ` < 25° at impact and lock | Intercept; meteor despawns |
| C5 × C0 | ray–sphere, `|c_V + t·d̂| = r` | Impact at `p_imp` |
| C5 × C1 | nearest city within 6 mm of `p_imp` | City −1 HP |
| C3 × C0 | signed distance < 5 mm | Publish contact, feed `ω` |
| C4 × C0 | signed distance < 5 mm | Cradle support, friction |
| C5 × C5 | ignored | Meteors pass through each other |

**Materials.** Meteor = "hot rock": restitution 0.0 against the shield, 0.15 against the planet; shield = "energy cap": no restitution, absorbs all momentum, spawns 12–20 spark particles. Planet = "peel": restitution 0.2, friction 0.55. Because the physical orange is real, the simulated restitution is used only for the *predicted drop* path if the player loses the cradle.

**CCD.** Meteor displacement per tick is `116/90 = 1.3 mm`, far below the 6 mm city disc, so discrete tests are sufficient; a swept sphere is still used for the city disc to avoid a tangential miss. Debris particles move at up to 400 mm/s (4.4 mm/tick) and are cosmetic, so they use discrete point tests.

**Force model.** The fingertip must supply torque `τ = r·F_t` to overcome palm friction. With a palm normal load of 0.8 N, µ_eff = 0.15, and a friction lever of 16.1 mm, the resisting torque is `0.8 × 0.15 × 0.0161 = 1.93 × 10⁻³ N·m`. A 1.0 N tangential fingertip force at r = 38 mm supplies `3.8 × 10⁻² N·m`, twenty times the palm drag, so deliberate strokes are never blocked. The rolling resistance of the peel against the palm is `C_rr·F_N = 0.02 × 0.8 = 0.016 N`, negligible.

If the cradle is lost, the predicted fall uses `a = −9.81 m/s²`, table contact at `y_T + r`, restitution 0.2, and a 0.35 s settle. The real prop is unaffected; the prediction only drives the "catch it" audio cue and the pause prompt.

## 11. Rendering, Occlusion & Compositing

Layer order (near to far in the compositor): passthrough base → scene mesh depth → prop depth proxy (colourless, at radius r) → virtual far sky (meteors behind the planet) → planet skin and city decals, inset 0.3 mm → shield cap → virtual near sky (meteors in front) → hand mesh → HUD/particles. The proxy writes depth at the tracked sphere radius; the planet skin is drawn 0.3 mm inside it, so the skin wins the depth test while anything beyond the surface is occluded.

**Occlusion cases.**

- **O1 — Meteor behind the physical globe.** The meteor's depth is tested against the 38 mm prop proxy. A meteor on the far side is fully hidden; its trail fades over 120 ms as it crosses the limb, so the player reads the direction from the telegraph ring rather than a see-through ball.
- **O2 — Right fingertip in front of the planet.** The tracked hand mesh wins the depth test; the fingertip stays visible over the city layer. A 2 mm "contact halo" is drawn *under* the hand mesh so the fingertip does not appear to float.
- **O3 — City on the far hemisphere.** Cities with `(q_P·p̂_i)·(eye − c_V) < 0` are culled by both normal test and depth. Their labels are hidden, not dimmed, to avoid a confusing x-ray look.
- **O4 — Shield cap intersecting the palm.** The translucent cap is clipped by the hand mesh depth. Where the cap passes behind the left palm, it disappears; where it passes in front, it blends at 0.35 alpha. No bloom is allowed to bleed over the hand silhouette.
- **O5 — Meteor passing behind the left fingers.** The hand mesh occludes the meteor and its 3 mm trail. The meteor's spatial audio continues, so the player can still track it by ear.
- **O6 — HUD ring versus the globe.** The HUD is rendered at 260 mm, in front of the 340 mm sky but behind the hand. It is depth-tested against the hand, so a raised palm correctly covers the combo counter.

**VFX budgets.** Meteor trail ≤ 64 quads, 1.2 ms; shield flare ≤ 3 simultaneous, 180 particles; city beacon ≤ 5 × 2 sprites; debris ≤ 400 points; impact scar decal 1 per impact, lifetime one wave. Total transparent overdraw target < 2.5× screen area. The shield cap uses a single-pass fresnel shader with no refraction, because refraction over passthrough breaks the depth cue that the fruit is real.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Meteor spawn | `c_V + 340 mm·d̂` | Descending two-tone ping, 880→660 Hz, 120 ms | Dashed ray + collapsing countdown arc |
| Alignment improving | shield axis | Rising sine 300→900 Hz, gain ∝ (1 − θ/90°) | Lock ring fills clockwise |
| Lock acquired | satellite | 1.2 kHz crystal chime, 90 ms | Ring snaps white, satellite flares |
| Lock lost | satellite | Soft 400 Hz drop, 60 ms | Ring drains in 120 ms |
| Intercept | impact point | 70 Hz thump + 2.4 kHz sparkle, 350 ms | White-green cap flare, 18 sparks |
| Impact | `p_imp` | 45 Hz sub drop + rock crunch, 500 ms | Orange flash, dust ring, camera 0.4 mm shake |
| City damaged | city beacon | 220 Hz alarm beep ×2 | Beacon stutters, smoke plume |
| City destroyed | city beacon | Descending minor third, 700 ms | Beacon dies, ash decal |
| Wave clear | globe | Major arpeggio, 900 ms | Cities pulse in sequence |
| Repair | each city | Warm 330 Hz hum, 400 ms | HP pip refills |
| Combo milestone | satellite | Rising chime, pitch +2 semitones per step | Combo number scales 1.0→1.3 |
| Tracker loss | head-locked | Muted 180 Hz pulse | "Show the globe" prompt |

All cues are spatialised at the listed origin with a 1.2 m rolloff; the HUD and lock cues are dry (non-spatial) to keep them legible. The game never uses haptics, so the intercept "thump" is the substitute for the physical kick of a shield impact.

## 13. Difficulty Progression & Tuning

| Level | Flight time (s) | Telegraph (s) | Concurrency cap | Spawn jitter | Meteor radius (mm) | Notes |
|---|---|---|---|---|---|---|
| 1 | 3.2 | 2.4 | 2 | ±0.15 s | 5.0 | Teaching wave: all 12 meteors, slower flight |
| 2 | 2.9 | 2.2 | 2 | ±0.20 s | 4.5 | Full 12-meteor wave |
| 3 | 2.6 | 2.0 | 3 | ±0.25 s | 4.0 | Baseline spec |
| 4 | 2.4 | 1.8 | 3 | ±0.30 s | 3.5 | Faster flight, tighter aim window |
| 5 | 2.2 | 1.6 | 4 | ±0.35 s | 3.0 | Finale: 12 meteors, 4 concurrent |

Named constants: `THETA_LOCK = 20°`, `THETA_DROP = 25°`, `LOCK_HOLD = 0.250 s`, `CAP = 25°`, `CITY_HP = 2`, `CITIES = 5`, `FAIL_CITIES = 4`, `METEORS_PER_WAVE = 12`, `SPAWN_INTERVAL = 2.6 s`, `REPAIR = 1 HP`, `INTERMISSION = 8.0 s`.

Adaptive rule: maintain a rolling 6-meteor window. If the player intercepts 5 of 6, reduce flight time by 0.15 s (floor 2.2 s) and raise concurrency by 1 (cap 4) for the next wave. If the player takes 2 impacts in a row, add 0.25 s to flight time (ceiling 3.2 s) and drop concurrency by 1 (floor 2). Adaptation never changes `THETA_LOCK` or `CAP`; the aiming rule stays learnable.

Difficulty is selected before the session but the adaptive rule may move one level up or down inside a run. The HUD never announces the change; only the meteor cadence reveals it.

## 14. Edge Cases & Failure Recovery

- **Prop dropped.** Cradle contacts drop to zero for > 120 ms → `DROP`, pause meteor clocks for 0.4 s, play a "catch it" cue. If the prop is re-registered within 4 s, resume at the same meteor phase with a 0.5 s grace. If not, return to `PRESENT`.
- **Tracking loss mid-wave.** `q_P` freezes after 200 ms; meteors run at 0.75× speed for up to 4 s. After 2 s the game pauses and shows a silhouette prompt. Re-acquisition restores 0.5 s of lock grace.
- **Hand occlusion of the globe.** The tracker can lose the right fingertips behind the left palm. The last valid `v_t` is decayed to zero over 120 ms; no rotation is invented.
- **Two-hand tumble.** If the left hand leaves the cup and contacts the globe while the right hand rolls, the two contacts are averaged only if their `ω` estimates agree within 60°/s; otherwise the left hand wins (stabilise) and spin is damped 60%.
- **Meteor spawns behind the player's head.** Rejection sampling guarantees ≥ 12° from the eye axis; if a wave's stratified set cannot satisfy this after 20 tries, the offending direction is rotated +30° in azimuth.
- **All cities destroyed except one.** The game continues; the last city's beacon is enlarged 1.4× and its HP pips are doubled in size so the player can read the stakes.
- **Frame stall > 22.2 ms.** Physics clamps to 2 substeps; meteor clocks are wall-clock-driven, so a stall cannot slow the sky. The alignment test uses the last tracked `q_P`, interpolated to the impact timestamp.
- **Left-handed player.** The mirror mode swaps the cup to the right hand and the roller to the left, mirrors city longitudes about λ = 0°, and flips the sign of the `ω_yaw` HUD. The gameplay math is unchanged because cities and meteors are stored in `P` and `V` respectively.
- **Low light / dark prop.** Texture odometry degrades; the fusion weight shifts to contact integration and a 0.5 mm/s² drift cap is applied. The game warns "hold still to re-fix" after 20 s of poor texture.
- **Player rotates the globe faster than 720 °/s.** `ω` saturates; the visual spin lags the hand by up to 80 ms but the shield axis is taken from the tracked pose, so no false intercepts occur.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — presents/registers the sphere, emits `PoseFrame {q_P, p_P, confidence, mode}` at 72 Hz.
- `ContactBus` — converts 26 joints into `Contact {joint, point_P, normal_P, v_rel, age}` with 5 mm enter / 8 mm exit and 30 ms debounce.
- `RollKinematics` — computes `ω = (n×v_t)/r`, fuses with tracked `q_P`, applies filters and the 720 °/s saturation.
- `GlobeController` — owns `q_P`, clamps the planet offset to ±15 mm, exposes `shield_axis_V()`.
- `MeteorDirector` — stratified spawn, telegraph timers, concurrency cap, impact callbacks.
- `ShieldSystem` — evaluates θ, the 20/25° lock hysteresis, 250 ms hold, 25° cap test.
- `CityGrid` — five cities in `P`, nearest-city query, HP, repair, destruction.
- `ScoreCombo` — intercept score, precision bonus, combo, wave bonus.
- `WaveStateMachine` — the table in §9; owns timers.
- `FeedbackRouter` — maps every event enum to an audio clip and a VFX prefab; guarantees 1:1 coverage.
- `Compositor` — layer order, prop depth proxy, occlusion masks O1–O6.

**Event flow.** `PropTracker` → `PoseFrame` → `GlobeController`; `ContactBus` → `RollKinematics` → `GlobeController`; `GlobeController` → `ShieldSystem`; `MeteorDirector` → `ShieldSystem` → (`ScoreCombo`, `CityGrid`, `FeedbackRouter`); `CityGrid` → `WaveStateMachine` → `FeedbackRouter`. The state machine never reads raw joints; it reads only `ShieldSystem.locked`, `CityGrid.deaths`, and timer values.

**Pseudocode 1 — per-tick aim and intercept.**

```
function tick(dt):
    pose = PropTracker.latest()                 # q_P, p_P, mode
    if pose.mode == LOST:
        if now - pose.last_good > 2.0s: enter(RECOVERY)
        q_hold = q_hold                        # frozen, no integration
    else:
        q_hold = RollKinematics.fuse(pose.q_P, contacts, dt)

    c_V = clamp(pose.p_P - V_origin, -15mm, +15mm)
    s_hat = rotate(q_hold, +Z)                 # shield axis in V
    for m in MeteorDirector.active():
        theta = acos(clamp(dot(s_hat, m.d_hat), -1, 1))
        if theta < 20deg:
            m.lock_time += dt
        elif theta > 24deg:
            m.lock_time = 0
        m.locked = (m.lock_time >= 0.250s)

        if now >= m.t_impact:
            if m.locked and theta < 25deg:
                ShieldSystem.intercept(m)
                ScoreCombo.on_intercept(theta)
                FeedbackRouter.play(INTERCEPT, m.impact_point_V)
            else:
                city = CityGrid.nearest(rotate(inverse(q_hold), m.d_hat))
                CityGrid.damage(city, 1)
                ScoreCombo.on_impact()
                FeedbackRouter.play(IMPACT, m.impact_point_V)
            MeteorDirector.retire(m)
    WaveStateMachine.advance(dt)
```

**Pseudocode 2 — wave spawn scheduler.**

```
function start_wave(w):
    remaining = 12
    active = []
    next_spawn = now + 2.5s
    cap = min(2 + floor((w - 1) / 2), 4)
    dirs = fibonacci_stratified(12)            # 12 unit vectors
    dirs = [d for d in dirs if angle(d, eye_dir_V) > 12deg]
    while len(dirs) < 12: dirs.append(rotate_az(dirs[0], 30deg))

function update_spawn(dt):
    if remaining > 0 and now >= next_spawn and len(active) < cap:
        d = dirs.pop(0)
        m = Meteor(d, spawn=c_V + 340mm*d, t_impact=now + flight_time(w))
        m.telegraph = min(telegraph_time(w), flight_time(w))
        active.append(m)
        remaining -= 1
        next_spawn = now + 2.6s + jitter(±0.25s)
        FeedbackRouter.play(SPAWN, m.spawn)
    if remaining == 0 and len(active) == 0:
        end_wave()
```

**Determinism.** All randomness uses a seeded PCG32 stream keyed by `(session_id, wave, meteor_index)`. Given a recorded pose/contact trace, the simulation replays bit-identically at 90 Hz, which is what the acceptance tests rely on.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Sphere registration | Present a 76 mm orange at 1.0 m; inject 24 synthetic rim points with 0.3 mm noise | Radius error < 0.2 mm; RMS < 0.5 mm; `P` converges in < 1.5 s |
| AT-02 | Rolling law | Synthetic contact at `n = +ẑ_V`, `v_t = 120 mm/s·x̂_V`, r = 38 mm, 90 ticks | `|ω| = 3.16 rad/s ± 3%`; axis = `+ŷ_V ± 2°` |
| AT-03 | Full 360° reach | Drive four 60 mm strokes with re-grips; log `q_P` | Unwrapped yaw covers ≥ 360°; no gimbal flip; drift < 3° |
| AT-04 | Lock hold | Hold θ = 19° for 240 ms, then 260 ms | No lock at 240 ms; lock true at 250 ms ± 1 tick |
| AT-05 | Lock hysteresis | Lock at 19°, increase θ to 24°, then 26° | Lock survives 24°; drops at 25.1° ± 0.5° |
| AT-06 | Intercept boundary | Impact at θ = 24.9° with lock; repeat at 25.1° with lock | 24.9° intercepts; 25.1° impacts the nearest city despite the lock |
| AT-07 | Nearest-city damage | Impact at the midpoint between City A and B (great-circle) | Exactly one city loses 1 HP; the nearer one by < 0.1° |
| AT-08 | City HP and fail | Apply 2 impacts to four cities | Each city reaches 0 after 2 hits; `GAME_OVER` fires on the 8th hit |
| AT-09 | Wave composition | Run wave 3 to completion with no player input | Exactly 12 spawns; concurrency never exceeds 3; wave ends ≤ 38 s |
| AT-10 | Combo and score | Intercept 5 in a row at θ = 0°, then take 1 impact | Combo = 0 after impact; 5th intercept score = 100×2.0×1.5 = 300 |
| AT-11 | Prop occlusion | Place a meteor at `c_V − 60 mm·d̂` behind the globe; capture frame | Meteor fragment contribution = 0 inside the 38 mm disc; trail fades at the limb |
| AT-12 | Tracking loss | Drop `PoseFrame` for 3 s during INBOUND | Clock runs 0.75×; state = RECOVERY at 2.0 s; resume at 0.5 s grace on re-acquire |
| AT-13 | Audio-VFX coverage | Enumerate all `FeedbackEvent` values | Every enum has a non-null clip and prefab; 0 unmapped events |
| AT-14 | 10-minute soak | Run 10 min of scripted waves at 90 Hz | No allocation > 1 MB/tick; frame time p99 < 11.1 ms; `q_P` drift < 2° |

All tests run headless against the deterministic simulation with synthetic `PoseFrame` and `Contact` streams. A test passes only when both the numeric assertion and the emitted event log match the expected sequence; visual criteria (AT-11) are validated by a depth-buffer readback, not by screenshot inspection.
