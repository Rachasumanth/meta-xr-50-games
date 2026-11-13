# Micro-Wok Toss

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Micro-Wok Toss |
| Slug | `micro_wok_toss` |
| Physical prop | Measuring-spoon bowl with 50 mm stem; bowl `R = 18.0 mm`, `t = 0.8 mm`, mass `6.0 g`; polished 18/10 stainless steel, µ ≈ 0.31 against skin |
| Family / sub-shape | Hemisphere, bowl-up, pinch scale; asymmetric handle |
| Collider family | Concave inner sphere + convex outer sphere + rim torus + stem capsule |
| Scale class | Prop 1:1 (36 mm bowl); virtual burner and plate at tabletop scale |
| Source concept quote | "A miniature burner flames beneath; tiny ingredients sizzle; swirl and micro-tilt to flip them without burning or losing them over the rim." |
| Physical input | Right thumb + index pinch the stem or near rim; wrist tilts up to 10 mm rim rise and circular swirls of 8–15 mm radius |
| Session length | 7 min (5 dishes × 40–55 s + preheat and plating) |
| Lose condition | Three dishes burned or emptied below the serve minimum |

## 1. Fantasy & Core Loop

The player holds a thimble-sized wok over a blue gas burner no larger than a coin. The bowl is the real measuring spoon; the flame, the oil sheen, and the ingredients are virtual, scaled so that a 2.5 mm sesame seed reads as a dumpling and a 3 mm caper reads as a whole comet shrimp. The burner roars with a low filtered hiss; the steel in the hand warms by 2–3 °C over a dish, a real thermal cue that sells the fiction without any haptic actuator. Ingredients roll on the inner steel, brown on the contact side, and leap when the wrist flicks. The skill is a two-beat rhythm: swirl to gather, micro-tilt to launch, catch them again before they clear the rim or scorch on the hot spot.

1. **Preheat (3.0 s).** The burner ignites; the bowl's virtual hot spot ramps from 24 °C to 180 °C. The HUD ring fills; ingredients cannot be added until 150 °C.
2. **Charge (1.5 s).** Six to fourteen ingredients drop in from a dispenser and settle at the bowl bottom. Their cook meters start at 0.
3. **Swirl (4–8 s).** A 10 mm-radius circular hand path at 1.0–1.6 Hz rolls the ingredients around the inner sphere; contact with the hot lower band raises cook level. The player keeps them moving so no single ingredient sits on the hot spot.
4. **Toss (0.6 s per launch).** A 10 mm upward wrist tilt produces 2.5–4.5 g of vertical bowl acceleration; ingredients detach when the normal reaction vanishes, tumble for 180–320 ms, and land. Airborne time adds "wok hei" sear and evens cook level.
5. **Plate (2.0 s).** At the recipe's toss count and cook band, the player tilts the bowl over the plate; ingredients slide out and score. Late plating burns; early plating is raw.
6. **Resolve (2.5 s).** The dish scores, the burner cools, and the next recipe card deals.

A dish runs 40–55 s; five dishes plus preheat, plating, and two retries fill 7 min. The session is continuous — no round reset unless a dish fails.

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

- **Radius and wall**: `R = 18.0 mm`, `t = 0.8 mm`, `R_in = 17.2 mm`. Bowl mass `6.0 g`; the stem accounts for 1.4 g of that, modelled as a 50 mm capsule of radius 2.5 mm along `−x̂_P` from `x = −6` to `x = −56 mm`.
- **Orientation**: bowl-up. Cavity opens toward `+ẑ_P`; convex exterior apex at `A_P = (0, 0, −18.0) mm`; rim circle at `z_P = 0`.
- **P origin**: rim-circle centre. The stem lies in the `z_P = 0` plane, so the rig is a bowl plus a flat handle.
- **Inner surface**: `p_P = R_in·(sinθ cosλ, sinθ sinλ, cosθ)` with `θ ∈ [90°, 180°]`, `λ ∈ [0°, 360°)`. Outer surface uses `R` over the same range. Rim torus major `R = 18.0 mm`, minor `t/2 = 0.4 mm`.
- **V-frame**: anchored to `P` in position and yaw only. `V_O` is a 8 Hz low-pass of `P_O`; `ŷ_V = ŷ_W`; yaw is the projection of `x̂_P` onto the table. Pitch and roll of `P` are discarded, so the virtual burner stays level while the bowl tilts inside `V`. Lag 40 ms.
- **Deviation 1**: the prop is hand-held, not table-supported. `T` is used only for the burner stand and the plate; `P` drives the bowl.
- **Deviation 2**: the ingredients are a deterministic sphere-on-inner-sphere simulation in `P`, not tabletop rigid bodies. They have no mutual collision; spawn spacing is Poisson-disc with 1.2 mm minimum.
- **Deviation 3**: bowl temperature is a lumped 1.2 J/K virtual hot spot driven by a 28 W burner, not a physical heat solve. The steel's real 2–3 °C warming is a cosmetic side effect tracked from session time.
- **Deviation 4**: the burner flame, oil, and steam are view-locked to `V`; they never inherit `P` roll, which would make the flame pour sideways.

## 3. Physical Play Space & Ergonomics

The player sits or stands at a table, holding the spoon by the stem between the right thumb and index, bowl 180–240 mm in front of the sternum and 60–120 mm above the table. The left hand rests on the table and is used only to tap the "plate" pad or steady the right wrist. The wrist starts neutral; tilts are ±25° pitch and ±20° roll, with the 10 mm rim rise occurring at 18–22° of pitch. The head looks 20–28° down. The burner is drawn 26 mm below the bowl apex, so the flame never intersects the hand.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Table `T`, 720–780 mm | Elbow at 90–100° flexion |
| Bowl hold | Stem pinch, aperture 8–12 mm | Stem is 5 mm wide; light 0.4 N grip |
| Bowl position | 180–240 mm from sternum, 60–120 mm above table | Avoids lap collision and neck flexion |
| Tilt range | Pitch −25°…+30°, roll ±20° | Keeps ingredients recoverable |
| Rim rise per tilt | 10 mm at 20° pitch | Matches source gesture |
| Swirl | 8–15 mm radius, 1.0–1.6 Hz | Below wrist tremor band, above visual persistence |
| Toss height | 15–45 mm above rim | Visible at 0.22 m without divergence |
| Dish length | 40–55 s; 5 dishes | Grip stamina; 7 min total |
| Rest | 5 s between dishes, hand open | Prevents thumb pad fatigue |

Fatigue mitigations: (a) the stem grip needs only 0.4 N because the bowl is light and the pinch is a locate-and-lift, not a clamp; (b) the virtual bowl is massless to the hand — the player never supports 6 g against gravity in a sustained way; (c) each dish ends with a 5 s open-hand rest; (d) the swirl is a wrist motion, not a finger motion, so the thumb pad is not worked continuously; (e) a "grip load" dial appears after 40 s and suggests lowering the bowl to the table rest pose between dishes.

## 4. Prop Geometry & Registration

The prop is a 1/2 teaspoon measuring spoon, stamped from 0.8 mm 18/10 stainless. The bowl is a true hemispherical cup; the stem is flat with a 4 mm-wide neck and a 9 mm eye at the end. The eye gives the tracker a hole feature that disambiguates yaw under specular highlights.

| Property | Value | Notes |
|---|---|---|
| Bowl outer radius `R` | 18.0 mm ± 0.3 mm | Silhouette fit |
| Wall `t` | 0.8 mm ± 0.08 mm | Stamped sheet |
| Inner radius `R_in` | 17.2 mm | `R − t` |
| Rim diameter | 36.0 mm | Aperture |
| Bowl depth | 18.0 mm | Apex to rim |
| Stem | 50 mm × 9 mm × 0.8 mm, eye 4 mm | Capsule proxy r 2.5 mm |
| Mass | 6.0 g ± 0.4 g | 1.4 g stem, 4.6 g bowl |
| Friction | µ = 0.31 skin, 0.18 steel–ingredient | Dry, oiled |
| Restitution | 0.22 ingredient–steel | Sesame-scale |

Surface-feature atlas (P-fixed):

| Feature | θ from +z | λ | Purpose |
|---|---|---|---|
| Inner hot spot | 145°–180° | all | Burner projection; cook rate ×1.0 |
| Inner wall | 100°–145° | all | Cook rate ×0.55, rolling band |
| Rim lip | 88°–92° | all | Overflow gate |
| Outer base | 150°–180° | all | Burner proximity readout |
| Stem neck | — | `x ∈ [−14, −6] mm` | Primary pinch zone |
| Stem eye | — | `x = −50 mm` | Registration hole |
| Pour lip | 90° | 0° ± 15° | Plating direction marker |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Concave hemisphere | `R_in = 17.2 mm`, `θ 90–180°` | Ingredient support |
| C1 | Convex hemisphere | `R = 18.0 mm`, `θ 90–180°` | Outer shell, hand collision |
| C2 | Torus | major 18.0 mm, minor 0.4 mm, `z = 0` | Rim and overflow gate |
| C3 | Capsule | r 2.5 mm, length 50 mm, along `−x_P` | Stem pinch |
| C4 | Capsule | r 1.4 mm, length 6 mm | Fingertip proxies |
| C5 | Sphere | r 2.0–3.2 mm | Ingredients |
| C6 | Disc | r 40 mm at `y_V = −26 mm` | Burner face |

Registration procedure: present the spoon 0.9–1.3 m away, rotate through 120° yaw and ±25° pitch, then hold 0.4 s. The fitter uses the bowl silhouette plus the stem-eye ellipse. Acceptance: `R` RMS < 0.3 mm, stem axis error < 1.5°, rim plane error < 1.2°, ≥ 16 rim inliers. Re-acquisition: 80 mm search cube, 1.2 s, 12 inliers RMS < 0.7 mm for 5 frames, 100 ms blend.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact region | Semantic output |
|---|---|---|---|---|
| I5.1 | Stem pinch | `R_thumb_tip`, `R_index_tip` | stem capsule `x ∈ [−40, −8] mm`, opposed normals | Hold valid, bowl follows hand |
| I5.2 | Rim pinch | same | θ 70°–90°, `λ ±45°` from pour lip | Alternate hold, same validity |
| I5.3 | Micro-tilt | hold + wrist pitch/roll | — | `q_P` change, 0–30° |
| I5.4 | Circular swirl | hold + hand translation | 8–15 mm radius, 1.0–1.6 Hz | Swirl energy `E_s` |
| I5.5 | Toss flick | hold + upward wrist accel | rim rise 8–14 mm in ≤ 180 ms | Toss impulse, detach |
| I5.6 | Pour | hold + roll toward lip | roll > 28° toward `λ = 0°` | Plate gate opens |
| I5.7 | Tap plate | `L_index_tip` | plate pad 40 mm disc | Confirm plating / skip |
| I5.8 | Release | all right contacts | lost > 80 ms | Bowl physics handoff, pause toss |
| I5.9 | Regrip | same as I5.1 | contact age > 30 ms | Resume with 0.3 s settle |
| I5.10 | Handedness mirror | all | menu option | Swap R/L, mirror stem to `+x_P` |

The right thumb and index are the only hold contacts. The right middle, ring, and pinky are curled and ignored; if they touch the bowl, they produce a "brace" tick but no input. The left index is the plate key. Hold validity requires both contacts within the stem or rim zones, normals opposed (`n_t·n_i < −0.6`), and 30 ms of age. If the hold is lost, the virtual bowl does not fall; it freezes in `V` for 1.5 s and the ingredients keep cooking, which is a legible penalty without a physical drop.

## 6. Input Kinematics & Signal Processing

Pose `P` is filtered with a One Euro filter (mincutoff 1.5 Hz, beta 0.03) and differentiated to angular velocity `Ω_P`. In the P frame, `Ω_B = q_P⁻¹ · Ω_P`. The bowl's linear acceleration `a_O` is the second derivative of the filtered `P_O`, low-passed at 6 Hz and clamped to 20 m/s² to reject tracker jitter. These are the only inputs to the ingredient simulation; there is no synthetic "tilt gain" because the physical bowl already carries the tilt.

Swirl detection: project the stem midpoint path onto the `x_V–z_V` plane, subtract its 0.5 s mean, and compute `E_s = (1/N)Σ|v_perp|²`. A swirl is valid when the path radius is 8–15 mm, the angular rate is 1.0–1.6 Hz, and `E_s > 400 mm²/s²` for 400 ms. Swirl adds a tangential bias to ingredients: `a_swirl = k_s·E_s^0.5·t̂`, with `k_s = 1.8 s⁻¹` and `t̂` the local swirl tangent. The bias is capped at 1200 mm/s² so the player cannot centrifuge everything into the rim.

Ingredient state: position `p` in `P`, velocity `v`, radius `r_i`, cook `C_i ∈ [0, 120]`, airborne flag. Contact radius `ρ_i = R_in − r_i`. The outward radial unit is `e = p/|p|`; the inward surface normal is `n = −e`.

Effective gravity in the bowl frame:
`g_eff = g_B − a_O,B − Ω̇_B × p − Ω_B × (Ω_B × p)`, with `g_B = q_P⁻¹·(0, −9810, 0) mm/s²`. The Coriolis term `−2Ω_B × v` is included in the tangential integration but omitted from the normal-force estimate because it is radial only at the poles.

Tangential acceleration: `a_t = g_eff − (g_eff·e)e − 2Ω_B × v`. Rolling without slip on the concave surface uses `a_roll = (5/7)·a_t` for a solid sphere. Tangential velocity integrates `v_t += a_roll·dt`, then `v = v_t + v_r·e`, and the position is re-projected to `|p| = ρ_i`.

Normal reaction per mass: `N/m = g_eff·e + |v_t|²/ρ_i`. Detachment occurs when `N < 0` and the outward radial velocity `v·e > 0`; this is the toss. The threshold is physical rather than a scripted impulse. Airborne integration uses `a = g_eff`, and re-contact occurs when `|p| ≤ ρ_i` and `v·e < 0`, with restitution 0.22 and tangential friction 0.18.

Toss quality: launch speed `v_l = |v·e|`; the ideal window is 90–220 mm/s, giving 180–320 ms of air and 16–45 mm of rise. Below 60 mm/s the ingredient "hops" and gains no sear; above 280 mm/s it is likely to clear the rim and overflow.

Overflow test: `p_z > 0` and `sqrt(p_x² + p_y²) > R_in − r_i` with `v·e > 0`. Once true, the ingredient leaves the bowl, arcs under gravity in `V`, and is removed after 0.5 s or when it falls below `y_V = −60 mm`.

Thermal model: `dT_b/dt = (P_burner − h_loss·(T_b − 24))/C_b`, `P_burner = 28 W`, `h_loss = 0.12 W/K`, `C_b = 1.2 J/K`, clamped to `T_b ∈ [24, 240] °C`. Preheat reaches 150 °C in 5.8 s and 180 °C in 7.4 s; the flame is reduced to 18 W after charge so a dish does not runaway. Ingredient cook: `dC_i/dt = K_c·contact·H`, `K_c = 22 s⁻¹`, `contact ∈ {0,1}`, `H = clamp((T_b − 60)/140, 0, 1.4)`. Airborne time adds `+4` cook per launch ("wok hei"), capped once per 0.8 s.

## 7. Virtual World Layout & Coordinate Mapping

`V` follows the bowl's position and yaw, with `ŷ_V` always up. The bowl rim plane sits at `y_V = 0` when level; the bowl apex is at `y_V = −18 mm`; the burner face is a 40 mm disc at `y_V = −26 mm`. The plate is a 70 mm disc at `(0, −22, 120) mm` in `V`, in front of the player.

| Entity | V-space position | Notes |
|---|---|---|
| Bowl rim centre | `(0, 0, 0)` | Follows `P_O` with 40 ms lag |
| Bowl apex | `(0, −18, 0)` | Lowest inner point |
| Burner face | `(0, −26, 0)`, r 40 mm | Flame height 18 mm |
| Flame core | `(0, −24, 0)`, r 9 mm | Additive, 90 Hz |
| Plate | `(0, −22, 120)`, r 70 mm | Plating target |
| Recipe card | `(−95, 15, 90)` | 60 mm × 40 mm, billboarded |
| Dispenser | `(0, 55, 30)` | Ingredient spawn point |
| Camera | Player head | Looks down 22° at bowl |

Coordinate mapping: the physical bowl is drawn at tracked `P`; virtual contents are drawn in the same `P` frame so they never slide relative to the steel. The burner is drawn in `V` and clipped by the bowl's outer mesh, so the flame appears to lick the real apex from below. Ingredients spawn at the dispenser and fall 55 mm into the bowl under `g_B`; they are inert until their first contact, which prevents spawn-time cook.

Scale: the 36 mm bowl maps to a 360 mm virtual wok at 10:1 for the burner and plate props, but ingredients stay at 1:1 physical size (2–3 mm) so they remain visible at 0.22 m. This deliberate scale mismatch is hidden by rendering the burner as a "macro lens" inset: the flame is drawn at 0.4× the wok's apparent scale, reading as a distant stove under a giant wok. The inset is fixed to `V` and never rotates with `P` roll.

## 8. Gameplay Systems & Rules

Entities: one bowl, 6–14 ingredients, one burner (thermal state), one plate, one recipe card, and a flame particle system. Each dish is defined by ingredient count `N`, toss target `N_t`, cook band `[C_lo, C_hi]`, time limit `T_dish`, and allowed losses.

Per-ingredient scoring at plate:
- Served in band: `+100`.
- Perfect sear (`C ∈ [C_mid − 6, C_mid + 6]` and ≥ 2 launches): `+50`.
- Raw (`C < C_lo`): `+20`, "raw" stamp.
- Burned (`C > 100`): `−150`, smoke burst.
- Overflowed: `−80` and the ingredient is lost.
- Dish clear (all remaining ingredients plated, losses ≤ allowance): `+300`, recipe stamp.
- All-perfect dish: `+500` bonus.

Cook level dynamics: `C` rises only on bowl contact, faster in the hot spot (`θ > 145°`, ×1.0) than on the wall (`100° < θ < 145°`, ×0.55). Airborne time adds the wok-hei bonus. Burn begins at 100; between 85 and 100 the ingredient smokes and the audio bed gains a warning overtone.

Toss rules: a launch counts when at least half the ingredients detach within a 250 ms window. One launch per 0.8 s is scored; extra flicks still toss but do not add the wok-hei bonus. The dish's toss target is a soft goal: reaching it adds `+40 × (1 + 0.1·N_t)`, missing it by more than two costs the "even sear" badge but does not fail the dish.

Overflow: every ingredient that leaves the rim increments `losses`. If `losses > allowance` (1 for dishes 1–2, 2 for 3–5) the dish fails immediately with a "floor" card. Burn-out: if more than half the ingredients exceed 100 simultaneously, the dish fails with a smoke-out.

Plating: valid when the bowl rolls > 28° toward `λ = 0°` (the pour lip) and the plate is within 60 mm of the lip's projected point. Each ingredient then leaves with `v_out = 60–120 mm/s` along the lip; its landing point is animated to the plate. Plating is irreversible for that dish.

Win: clear all five dishes with at least one star each. Lose: three failed dishes. Stars: 1 for clear, 2 for ≤ 1 overflow and no burn, 3 for all-perfect.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset tracked | load kitchen, zero score | Register |
| Register | spoon presented | `R` RMS < 0.3 mm | set `P`, `R_in`, stem | Preheat |
| Preheat | dish start | hold valid | burner 28 W, target 180 °C | Charge |
| Preheat | `T_b < 150 °C` after 6 s | hold invalid | prompt "pick up the wok" | Register |
| Charge | 1.5 s timer | `T_b ≥ 150 °C` | spawn `N` ingredients | Cook |
| Cook | recipe timer | losses ≤ allowance | integrate, cook, toss, plate | Serve |
| Cook | losses > allowance | — | floor cue, fail card | Resolve |
| Cook | burned majority | — | smoke-out cue | Resolve |
| Serve | pour gate + plate | all plated or timer | tally, stars | Resolve |
| Resolve | 2.5 s timer | dish < 5 | card, 5 s rest | Preheat |
| Resolve | dish = 5 | ≥ 3 clears | victory tally | Victory |
| Resolve | fails = 3 | — | kitchen closes | GameOver |
| Any | hold lost > 1.5 s | — | freeze bowl, keep cooking | Recovery |
| Recovery | pinch valid 30 ms | prop visible | 0.3 s settle, resume | previous state |
| Any | headset removed | — | save dish, cook states | Paused |

Timers run at 90 Hz. The cook timer excludes the 5 s rest and any Paused interval. `T_b` cools at `h_loss/C_b` while paused, which is cosmetic only.

## 10. Physics & Collision

| Body | Shape | Mass | Friction | Restitution |
|---|---|---|---|---|
| Spoon bowl | Concave sphere `R_in 17.2` + convex `R 18` | 6.0 g | 0.31 skin | 0.22 |
| Stem | Capsule r 2.5, L 50 mm | included | 0.31 | 0.1 |
| Ingredient | Sphere r 2.0–3.2 mm | 0.05–0.15 g | 0.18 | 0.22 |
| Burner | Disc r 40 mm | static | — | — |
| Plate | Disc r 70 mm | static | 0.4 | 0.2 |

Ingredient moments: `I = (2/5)m r²`; the rolling factor 5/7 follows from `a = g sinθ/(1 + I/(m r²))` for a solid sphere. Collision pairs: ingredient↔inner bowl (support), ingredient↔rim torus (overflow gate), ingredient↔plate (serve trigger), ingredient↔ingredient (disabled, deterministic spacing), bowl↔hand (input), bowl↔burner (visual only, no physical contact), bowl↔table (ignored unless the player lowers it; a table contact pauses the toss and plays a wooden knock).

Integration: fixed 90 Hz, max 2 substeps. Ingredients step at 90 Hz inside the bowl frame; if `|v| > 600 mm/s` the step subdivides to 180 Hz for the contact test. The bowl itself is kinematic from `P`; it does not react to ingredient momentum (ingredient mass is < 1% of bowl mass). Overflow arcs integrate in `V` under `(0, −9810, 0) mm/s²`.

Forces: swirl bias ≤ 1200 mm/s²; gravity 9810 mm/s²; bowl linear acceleration clamp 20 m/s²; angular speed clamp 320 °/s (beyond this the tracker is rejected as a shake). Contact restitution 0.22, tangent friction impulse `J_t ≤ 0.18·J_n`. Toss detachment is computed from `N < 0`, with no artificial impulse. Burner thermal constants: `P = 28 W` preheat, `18 W` cook, `h_loss = 0.12 W/K`, `C_b = 1.2 J/K`.

## 11. Rendering, Occlusion & Compositing

Layer order: 7 recipe card/UI → 6 hands → 5 near VFX (flame, steam, oil) → 4 ingredients → 3 bowl mesh → 2 burner and plate → 1 scene-mesh depth → 0 passthrough. The bowl is drawn twice: a back-face inner surface pass for the cavity and a front-face outer pass, with a 0.5 mm depth bias to avoid z-fighting at the 0.8 mm rim.

Occlusion cases:

1. **Near rim over the cavity.** The front rim correctly hides the far inner wall; ingredients below the rim are visible only through the 36 mm aperture, which is the intended "look into the wok" framing.
2. **Fingers over the stem.** Hand depth hides the stem; the hold reticle is a 1.2 mm offset decal and is depth-tested, so it vanishes under the thumb.
3. **Flame under the bowl.** The flame is drawn in `V` and occluded by the convex outer bowl, so only the fringe escapes around the silhouette; no flame appears inside the cavity.
4. **Ingredient above the rim.** Airborne ingredients are drawn at layer 4 in front of the far rim but behind the near rim when they pass behind it; per-pixel depth handles the crossover at 90 Hz.
5. **Steam between eye and bowl.** Steam is a soft additive volume with depth write off and a 0.35 alpha cap; it never hides the cook meters, which are drawn at layer 7 with a 2 mm offset.
6. **Bowl over the table edge.** The contact shadow is a projected blob on `T`; when the bowl is lifted the shadow fades with height and the passthrough table remains un-tinted outside the flame's soft glow.

VFX budget: 500 particles (220 flame, 120 steam, 80 oil sparkle, 80 overflow splash), 2 volume quads, 1 shadow blob, 14 ingredient meshes, 1 plate ring. Overdraw ≤ 2.0×. Flame is a 3-layer scrolling noise quad; steam is 8 billboards; no screen-space blur. GPU budget 8.5 ms, leaving 2.6 ms of the 11.1 ms frame for the compositor.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Preheat ignite | Burner | Whoosh + low blue roar | Flame blooms, ring fills |
| Hold valid | Stem | Small steel tick | Stem reticle closes |
| Swirl start | Bowl | Oil sizzle bed rises | Ingredients trail arcs |
| Sizzle intensity | Bowl | Noise band 2–6 kHz, gain ∝ contact count | Hot-spot glow pulses |
| Toss launch | Bowl lip | Short upward whip, 400–900 Hz sweep | Ingredients stretch, air ring |
| Airborne peak | Above bowl | Tiny chime per ingredient | Sparkle at apex |
| Landing | Bowl | Micro clink, pitch ∝ speed | Dust puff, sear flash |
| Burn warning | Ingredient | Acrid crackle + low pulse | Ingredient edge darkens, smoke |
| Overflow | Rim azimuth | Falling whistle + tiny splash | Ingredient arcs off, red tick |
| Plate | Plate | Ceramic tap + warm chime | Ingredient lands, score stamp |
| Dish clear | Card | Four-note kitchen cadence | Stars stamp, card flips |
| Dish fail | Card | Flat descending two-note | Smoke wash, card tears |
| Recovery | Bowl | Sonar ping, 620 Hz | Ghost bowl outline |

All cues are spatialised at the bowl or ingredient; the recipe card is the only head-locked UI. Audio latency < 20 ms. The sizzle bed ducks 4 dB during toss launch so the whip reads; burn warnings duck the bed 6 dB and add a 2 Hz pulse.

## 13. Difficulty Progression & Tuning

| Dish | Ingredients | Toss target | Cook band | Burner power (W) | Time (s) | Loss allowance |
|---|---|---|---|---|---|---|
| 1 Petal rice | 8 | 3 | 62–86 | 24 | 45 | 1 |
| 2 Firefly noodles | 8 | 4 | 66–88 | 26 | 45 | 1 |
| 3 Comet dumplings | 10 | 5 | 70–90 | 27 | 50 | 2 |
| 4 Nebula omelette | 10 | 6 | 62–82 | 28 | 50 | 2 |
| 5 Dragon-pearl medley | 14 | 8 | 72–88 | 30 | 55 | 2 |

Adaptive rule: if the player burns more than two ingredients in a dish, reduce burner power by 12% and widen the cook band by 4 points for the next dish only. If the player clears a dish with no burns and no overflow, raise power by 6% and narrow the band by 2 points, capped at dish-5 values. Adaptation never changes ingredient count by more than ±2, preserving the toss rhythm.

Named tuning constants: `R = 18.0 mm`, `R_IN = 17.2 mm`, `M_BOWL = 6.0 g`, `MU_SKIN = 0.31`, `K_SWIRL = 1.8 s⁻¹`, `SWIRL_CAP = 1200 mm/s²`, `TOSS_V_MIN = 60 mm/s`, `TOSS_V_IDEAL = [90, 220] mm/s`, `TOSS_V_MAX = 280 mm/s`, `K_COOK = 22 s⁻¹`, `T_BURN = 100`, `T_BAND_MID = 78`, `P_PREHEAT = 28 W`, `P_COOK = 18 W`, `H_LOSS = 0.12 W/K`, `C_BOWL = 1.2 J/K`, `OVERFLOW_R = 17.2 mm`, `POUR_ANGLE = 28°`, `DISH_REST = 5 s`.

## 14. Edge Cases & Failure Recovery

- **Spoon tracking loss mid-toss.** Predict `P` for 200 ms with the last `Ω` and `a_O`, then freeze the bowl and contents in `V`. Ingredients continue their ballistic arcs in the frozen frame, so a tracking dropout cannot teleport them out. Re-acquire within 1.2 s or pause with a stem-eye glow.
- **Pinch lost while ingredients are airborne.** The bowl freezes but the ingredients keep falling; because `P` is frozen, they land back in the bowl. The player loses no food but the dish clock keeps running, making the fumble costly but not unfair.
- **Tracker yaw flip near 90° roll.** If the quaternion solution jumps > 45° in one frame, reject the frame, hold the previous `q_P`, and blend back at 12 Hz. Ingredient velocities are preserved through the rejection.
- **Tilt beyond 55°.** All ingredients slide to the downhill rim and overflow over 0.3 s, with a "spill" cue. There is no artificial clamp, because the physical spoon can really be inverted.
- **Ingredient trapped under the rim torus.** If an ingredient's centre is within `r_i` of the torus for > 100 ms, nudge it 0.6 mm inward along `−e`; this resolves the geometric corner without a visible pop.
- **Burner occluded by the hand.** The flame and its glow are depth-tested against the hand; if occluded for > 300 ms, a small burner status pip appears on the recipe card so heat state remains legible.
- **Plate out of reach.** If `L_index_tip` taps outside the 40 mm pad, the tap is ignored and a 100 ms "miss" tick plays; the pour gate stays open for 3 s before closing.
- **Handedness swap.** Mirror the stem to `+x_P`, swap R/L roles, and mirror the pour lip to `λ = 180°`. The ingredient sim is unchanged because it is expressed in `P`.
- **Headset removed.** Save `T_b`, per-ingredient `(p, v, C)`, dish timer, and score. On resume, re-register, hold at 120 °C for 1 s to re-stabilise, and reset any airborne ingredients to the bowl bottom.
- **Real-world spill or drop.** A free-fall detector on `P` (|a| > 0.7 g for 120 ms with no hold contact) pauses the dish and shows "recover the spoon"; on re-presentation, the dish resumes with a 2 s grace and no score penalty.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `PropTracker` | Spoon fit (bowl + stem eye), 72 Hz pose, loss/re-acquire |
| `BowlPose` | Filtered `P`, `Ω_B`, `a_O`, V anchor (position + yaw) |
| `HoldClassifier` | Stem/rim pinch, regrip, release, hold validity |
| `SwirlDetector` | Path radius, angular rate, `E_s`, swirl tangent |
| `IngredientSolver` | Inner-sphere contact, rolling, detachment, overflow arcs |
| `ThermalModel` | Burner power, bowl temperature, per-ingredient cook |
| `RecipeDirector` | Five dish definitions, charge, timers, plate gate |
| `PlateResolver` | Pour angle, plate proximity, landing and scoring |
| `ScoreService` | Per-ingredient scoring, stars, failures, persistence |
| `AudioDirector` | Sizzle bed, cue table, ducking, spatialisation |
| `SessionFSM` | State table, pause, recovery, deterministic replay |

Event flow: `HandTracking(72 Hz) → HoldClassifier → BowlPose → SwirlDetector → IngredientSolver → ThermalModel → PlateResolver → ScoreService → AudioDirector/Renderer`. `SessionFSM` owns dish transitions and subscribes to overflow, burn-majority, plate, and timer events. The ingredient solver runs at 90 Hz and renders interpolated positions between ticks.

```text
function step_ingredient(p, v, r, P, dt):
    rho = 17.2 - r
    e = normalize(p)
    g_eff = q_inv(P.q) * (0, -9810, 0) - P.a_O
          - cross(P.omega_dot, p)
          - cross(P.omega, cross(P.omega, p))
    a_t = g_eff - dot(g_eff, e) * e - 2 * cross(P.omega, v)
    v_t = v - dot(v, e) * e + (5/7) * a_t * dt
    N_over_m = dot(g_eff, e) + |v_t|^2 / rho
    if N_over_m < 0 and dot(v, e) > 0:
        return AIRBORNE(p, v)
    v = v_t
    p = p + v * dt
    p = normalize(p) * rho          # project to inner sphere
    return CONTACT(p, v)
```

```text
function step_thermal(T_b, ingredients, dt, plating):
    P_heat = plating ? 0 : (charged ? 18 W : 28 W)
    T_b += (P_heat - 0.12 * (T_b - 24)) / 1.2 * dt
    T_b = clamp(T_b, 24, 240)
    H = clamp((T_b - 60) / 140, 0, 1.4)
    for ing in ingredients:
        if ing.contact and not ing.plated:
            zone = (ing.theta > 145 deg) ? 1.0 : 0.55
            ing.C += 22 * zone * H * dt
            if ing.C > 100: burn_flags += 1
    if count(burn_flags) > len(ingredients) / 2:
        return SMOKE_OUT
    return COOKING
```

State is snapshotted at charge and at plate. A crash restores the charge snapshot and replays up to 2.0 s of recorded pose; all recipe and thermal constants are in one `KitchenTuning` asset.

## 16. Acceptance Test Checklist

1. **Geometry fit.** Present the spoon at 1.0 m, sample 400 frames. Pass: `R ∈ [17.7, 18.3] mm`, `R_in ∈ [16.9, 17.5] mm`, stem-axis error < 1.5°, rim-plane error < 1.2°.
2. **Detachment.** Hold the bowl level, then apply a synthetic 4.0 m/s² upward acceleration for 120 ms. Pass: at least 80% of ingredients detach, launch speed ∈ [90, 220] mm/s, and airtime ∈ [180, 320] ms.
3. **No-detach band.** Apply 1.5 m/s² upward. Pass: zero detachments; all ingredients remain in contact and the toss counter does not increment.
4. **Rolling law.** Place one ingredient at `θ = 160°`, level bowl, no swirl. Pass: tangential acceleration = `(5/7)·g·sin20° = 2395 mm/s²` ± 5%, and it reaches the rim in 0.42 ± 0.05 s.
5. **Normal force.** At rest at the bowl bottom, `N/m = 9810 mm/s²` ± 1%. Tilt to 45° and spin at 8 rad/s; pass: ingredient detaches at the predicted threshold within one tick.
6. **Overflow.** Drive an ingredient over the lip at 200 mm/s. Pass: exactly one overflow event, `−80`, and the ingredient is removed within 0.5 s.
7. **Thermal ramp.** Preheat from 24 °C. Pass: 150 °C at 5.8 ± 0.4 s, 180 °C at 7.4 ± 0.5 s, and the clamp never exceeds 240 °C.
8. **Cook rate.** Hold a bowl at 180 °C with one ingredient in contact for 3.0 s. Pass: `ΔC = 22 × 1.0 × 0.857 × 3.0 = 56.6 ± 3` points.
9. **Burn.** Hold an ingredient on the hot spot from `C = 90` with `T_b = 200 °C`. Pass: burn flag at 100 within 0.45 s and the smoke cue fires once.
10. **Swirl classifier.** Trace a 12 mm-radius circle at 1.3 Hz for 1 s. Pass: swirl valid at 400 ms; a 5 mm tremor circle at 4 Hz is rejected.
11. **Plate gate.** Roll 29° toward the lip with the plate at 55 mm. Pass: plating fires; at 27° or 65 mm it does not.
12. **Ingredient count.** Spawn 14 ingredients. Pass: no overlap > 0.2 mm at spawn, all settle in the bowl, and the 90 Hz solver uses < 0.8 ms CPU.
13. **Occlusion.** Place the palm between the eye and the bowl. Pass: the near rim hides the cavity, ingredients behind the palm are hidden, the flame fringe is clipped, and no z-fighting appears in 600 frames.
14. **Endurance.** Run the 7 min five-dish script with a synthetic hand. Pass: no ingredient escapes without an overflow event, cook totals match the analytic replay within 1%, and memory growth < 4 MB.
