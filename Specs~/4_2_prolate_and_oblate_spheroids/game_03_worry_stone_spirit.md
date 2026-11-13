# Worry-Stone Spirit

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Worry-Stone Spirit |
| Slug | `worry_stone_spirit` |
| Physical prop | Oblate river stone, 85 × 70 × 34 mm nominal; 150 g; smooth basalt with a broad, gently convex top face |
| Registered contact spheroid | Oblate; `a = b = 37.5 mm`, `c = 17.0 mm` (75 × 75 × 34 mm fitted spheroid); the real stone's 5 mm end lobes lie outside the collider |
| Family / sub-shape | 4.2 Prolate and Oblate Spheroids / oblate, palm-scale, thumb-rubbed top face |
| Scale class | Palm-cradled 1:1; abrasion field 75 mm across; tabletop scale |
| Source concept quote | "A stone encased in petrified ash; rubbing polishes it away layer by layer, revealing star-charts before the shadow tide returns." |
| Physical input | Left palm cradles the stone; right thumb rubs the broad top face in smooth ovals; right index braces the side |
| Session length | 6 min: onboarding 40 s + 5 tides × 50 s + 4 intermissions × 10 s + results 30 s |
| Lose condition | 3 or fewer of 5 constellations recorded |

## 1. Fantasy & Core Loop

The stone was a sky-recording before it was a stone. Five times a night, the shadow tide climbs its rim and erases whatever the ash still hides. Under the ash are five constellations, each one a memory of a sky that no longer exists. The player is the keeper: cradle the stone, wet the thumb with a breath, and rub. The ash does not vanish at once. It comes off in six grey layers, each one revealing a little more of the chart, and the thumb's own pressure and speed decide whether the layer lifts cleanly or burnishes into a hard glaze.

1. **READ (1.0 s).** The tide front is a dark rim at φ = −80°. The next constellation's ten fragment sites glow faintly through the ash; the nearest three are outlined.
2. **CRADLE (0.3 s).** The left palm settles under the stone. The V frame locks to the palm; the chart is P-fixed, so turning the stone brings a fragment under the thumb.
3. **RUB (1.5–4.0 s per fragment).** The right thumb traces a 12–22 mm oval over the top face at 70–160 mm/s. Abrasion removes 0.18 mm/s at nominal pressure and speed; each 0.20 mm layer takes 1.1 s of contact.
4. **REVEAL (0.2 s).** When local ash thickness falls below 0.15 mm, the fragment's star ignites and links to its revealed neighbours within 30°.
5. **TRIAGE (continuous).** The tide front climbs 3.3°/s. Fragments at low latitude are lost first; the player must decide whether to polish a near fragment or rotate the stone to save a far one.
6. **RECORD (0.5 s).** At tide end, a constellation with ≥ 7 of 10 fragments is recorded. Four of five recorded constellations win the run.

A tide is a 50 s race between the thumb and a 3.3°/s shadow front. A focused player reveals 6–8 fragments; a player who scrubs too hard burnishes the ash and reveals 3–4. The design punishes force, not hesitation.

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

`P` origin is the stone's centre; `+ẑ_P` is the broad top face being rubbed, `−ẑ_P` rests in the left palm, and `+x̂_P` is the calibration direction toward the base of the left thumb; `+ŷ_P` is to the player's left. Exact semiaxes are `a = b = 37.5 mm` and `c = 17.0 mm`; the real 85 × 70 × 34 mm stone's end lobes sit outside this 75 × 75 × 34 mm fitted spheroid. Because `c < a`, the prop is oblate, and the top pole's local radius of curvature is `R_p = a²/c = 82.72 mm`, so the rubbing surface is a gentle dome, not a sphere. The local rolling model is `ω = (n × v_t)/r_eff`, with `r_eff = |p_P|`; at the top pole `r_eff = 17.0 mm`, at the equator `37.5 mm`. `V` is anchored to the left palm frame: origin 12 mm above the palm centre along the palm normal, `x̂_V` from wrist to middle MCP, `ŷ_V` from thumb to pinky, `ẑ_V` along the palm normal. Four deviations:

1. **Palm-anchored V, P-fixed chart.** The star chart and ash heightmap are fixed to `P`; the tide curtain and HUD are fixed to `V` (the palm). Rotating the stone inside the cradle therefore moves the chart under the thumb without moving the tide.
2. **Virtual ablation, not real material.** The ash is a 96 × 48 heightmap with 6 layers of 0.20 mm virtual relief. The real stone is unchanged; contact is against the spheroid, and the heightmap only modulates rendering and the reveal test.
3. **Pressure proxy.** Thumb pressure is estimated from pad penetration `δ` and a 60 N/m pad stiffness; there is no force sensor. The proxy is low-passed and clamped to 0.08–0.55 N.
4. **Glaze, not a hard fail.** Over-pressure or over-speed raises local ash hardness instead of blocking the stroke. The player can recover by slowing down; the glaze decays at 0.10 hardness/s.

## 3. Physical Play Space & Ergonomics

The player sits with the left forearm supported and the left palm supinated, fingers relaxed. The stone rests across the thenar pad and the middle finger, with the broad top face angled 15–25° toward the player. The right hand approaches from above; the thumb pad contacts the top face at 55–75° from the surface tangent, and the wrist stays within 15° of neutral. The stone is not gripped: the left hand is a cradle, and the right thumb is the only rubbing effector. The right index may brace the stone's near side to stop it rotating, but it must not rub.

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated, left forearm supported | Keeps the cradle stable for 5 min |
| Stone centre | 260–320 mm from eyes, 30 mm above palm | Comfortable near-focus and thumb reach |
| Top-face tilt | 15–25° toward player | Keeps the thumb pad flat on the dome |
| Thumb path | 12–22 mm oval, 70–160 mm/s | Covers 3–5 fragments per tide without fatigue |
| Contact pad | 6.5 mm radius | Matches a relaxed thumb pad and gives a 13 mm polish track |
| Session | 5 × 50 s tides, 10 s rests | Repetitive thumb friction is the limiting load |
| Break cue | Open-hand overlay after tide 3 | Releases the thumb and wrist |
| Max continuous rub | 12 s, then a 2 s soft cue | Prevents skin heat and flexor fatigue |

The left palm must remain under the stone for `V` to stay anchored; if the cradle contacts drop below three pads, the game pauses the tide and shows a "cradle the stone" prompt. The right thumb's contact angle is deliberately steep: a flat thumb drags skin and produces inconsistent pressure, while a 55–75° pad contact gives a repeatable 4–8 mm penetration and a readable force proxy.

## 4. Prop Geometry & Registration

| Property | Value | Notes |
|---|---|---|
| Nominal stone | 85 × 70 × 34 mm | Oval river stone, broad face up |
| Fitted spheroid | `a = b = 37.5 mm`, `c = 17.0 mm` | 75 × 75 × 34 mm; end lobes outside collider |
| Mass | 150 g | Basalt |
| Top-face curvature | `R_p = a²/c = 82.72 mm` | Gentle dome; the thumb sees a nearly flat polish plane |
| Ash relief | 1.20 mm total | 6 layers × 0.20 mm |
| Heightmap | 96 λ × 48 φ | 3.75° cells; 2.45 mm arc at the equator |
| Bedrock chart | 2048 × 1024 equirectangular | Emissive star chart under the ash |
| Surface friction | µ = 0.46 dry skin on basalt | Talc-free; a damp thumb raises it to 0.62 |

**Surface-feature atlas (P-fixed; λ from `+x̂_P` toward `+ŷ_P`, φ from equator):**

| Feature | φ | λ | Extent | Purpose |
|---|---|---|---|---|
| Top polish cap | +5° to +90° | all | 85° cap | Rubbable region; abrasion is active here |
| Prime meridian | all | 0° | 1 mm dim line | Chart orientation ruler |
| Equator | 0° | all | 1 mm band | Tide reference |
| Cradle contact | −90° to −30° | all | lower cap | Left palm support, never rubbed |
| Thumb rest | +35° | 0° | 8 mm | Initial thumb placement at calibration |
| Fragment field | +10° to +70° | 10 per tide | 3 × 3 cells each | Star sites; min 12° separation |
| Layer escarpment | any | any | 0.2 mm step | Visual layer boundary in the ash shader |
| Tide lip | −80° | all | 2 mm band | Start of the shadow front |
| Chart axis | +90° | any | 2 mm cap | North star anchor |

The heightmap stores `h(φ, λ)` in millimetres, initialized to 1.20 mm over the whole top cap and tapered to 0.0 mm at φ < −20° so the lower stone looks clean. The layer index is `L = clamp(floor(h/0.20), 0, 5)`; `L = 5` is the untouched surface, `L = 0` is bedrock with the chart at full emission. The 10 fragment cells in each tide are chosen from a seeded set with a minimum 12° great-circle separation and φ between +10° and +70°.

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Spheroid | `a = b = 37.5`, `c = 17.0 mm` at `P` | Stone body and depth proxy |
| C1 | Capsule | r = 6.5 mm, length 14 mm | Thumb pad; abrasion effector |
| C2 | Capsule × 3 | r = 12 mm, lengths 20–40 mm | Left palm cradle pads |
| C3 | Heightfield | 96 × 48, 0–1.2 mm | Visual only; no collision |
| C4 | Disc × 10 | r = 3.5 mm, on the top cap | Fragment reveal volume |
| C5 | Curtain | φ = φ_t, annular | Tide front; no collision |
| C6 | Index brace | Capsule r = 5 mm | Right index side contact |

Registration: the player presents the stone at 0.8–1.2 m, turns it through 120° of yaw, and settles it into the left palm. The fitter uses the 85 mm silhouette and the 34 mm profile; acceptance requires RMS < 0.5 mm, yaw error < 2.0°, and a top-pole normal within 10° of the palm normal. Re-acquisition searches a 130 mm cube around the last `P` for 1.2 s; success needs 8 rim samples with RMS < 0.9 mm over 6 frames. The chart's `λ = 0` is set to the direction from the stone centre to the base of the left thumb at calibration, so the first fragment is never under the palm.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long or axis) | Semantic output |
|---|---|---|---|---|
| I5.1 | Thumb rub | `R_thumb_tip` | φ +5° to +90°, any λ | Contact path `p_P(t)`, tangential speed `v` |
| I5.2 | Pressure | `R_thumb_tip` pad | same | `δ` → `p = 60 N/m·δ`, clamped 0.08–0.55 N |
| I5.3 | Smooth oval | Thumb path over 0.8–2.5 s | top cap | Closed-loop classifier; abrasion ×1.25 |
| I5.4 | Index brace | `R_index_tip` | φ −20° to +20°, λ = player side | Stabilises stone, no abrasion |
| I5.5 | Turn stone | Both hands cup and rotate | any | Reorient `P`; chart moves under thumb |
| I5.6 | Regrip | Thumb lift > 120 ms | any | Abrasion pauses; layer state retained |
| I5.7 | Cradle | `L_palm`, `L_heel`, `L_middle_mcp` | lower cap, ≥ 3 contacts | `HOLD`; keeps `V` anchored |
| I5.8 | Clear thumb | Rub thumb on tray edge | `T` plane, outside stone | Clears ash decal; cosmetic only |

**Per-finger roles.** The right thumb is the only abrasion effector. The right index is a brace and a stone-turn assist; its contact never removes ash. The right middle, ring, and pinky are ignored. The left palm's three pads (thenar, hypothenar, middle-finger base) must maintain at least three contacts for `V` to stay anchored; if the cradle is lost, the tide pauses. A right thumb contact that begins on the equator or below (φ < +5°) is rejected, because the thumb cannot apply a stable pad normal there and the polish would be a scrape.

Contact gating: an abrasion contact is valid when signed distance < 5 mm, age > 30 ms, the contact point is inside the top cap, and the contact normal has a positive `ẑ_P` component. Release hysteresis is 8 mm. The thumb's tangential speed is computed from the contact point's motion across the spheroid surface, not from the raw joint velocity, so a stationary thumb on a rotating stone does not falsely abrade.

## 6. Input Kinematics & Signal Processing

Abrasion follows a local Archard-like law. For each heightmap cell `i` within the thumb pad's 6.5 mm geodesic radius:

`dh_i/dt = −k_w · (p/p₀) · clamp(v/v₀, 0.25, 1.60)^0.7 · S · (1/H_i)`,

with `k_w = 0.18 mm/s`, `p₀ = 0.40 N`, `v₀ = 80 mm/s`, `S = 1.25` for a smooth oval and `S = 1.0` otherwise, and `H_i` the local hardness (initial 1.0). At nominal pressure and speed, `dh/dt = −0.18 mm/s`, so one 0.20 mm layer takes 1.11 s of cumulative contact. The exponent 0.7 is deliberately sub-linear: doubling the rub speed raises removal by only 1.62×, while doubling pressure doubles it. This makes pressure the dominant skill variable and speed a secondary one.

Pressure proxy: `p = min(60 N/m · δ, 0.55 N)`, with `δ` the pad penetration clamped to `[1.3, 9.2] mm`; the lower clamp is the 0.08 N floor. The proxy is low-passed at 6 Hz and rate-limited to 12 N/s. A thumb that presses harder than 0.55 N does not remove ash faster; instead it raises hardness:

`dH/dt = +0.25/s` while `p > 0.50 N` or `v > 200 mm/s`, capped at `H = 2.0`; `dH/dt = −0.10/s` otherwise, floored at 1.0.

Glaze is local: `H_i` is stored per cell, so a burnished patch can be avoided while the rest of the stone rubs clean. The visual shader tints glazed cells 12 % darker and adds a 0.05 mm specular sheen.

**Thumb path and oval classifier.** The contact point is projected to `(φ, λ)` and unwrapped. A stroke is an *oval* when: the path closes within 4 mm of its start; angular coverage around the path centroid is ≥ 300°; the bounding aspect ratio is 0.5–2.0; duration is 0.8–2.5 s; and the coefficient of variation of speed is < 0.35. A qualifying oval sets `S = 1.25` for the next 2.0 s and emits a `POLISH` event. Back-and-forth linear scrubbing sets `S = 0.85` and does not glaze; it is simply less efficient.

**Geodesic contact.** The thumb pad is a capsule; its effect on the heightmap is a disc of radius 6.5 mm measured as 3-D chord distance on the spheroid: `d_i = |p_thumb − p_i|`. Cells with `d_i < 6.5 mm` receive abrasion with a smooth falloff `w_i = 1 − (d_i/6.5)²`. This avoids the pole singularity of a pure lat/long distance and keeps the polish footprint circular near the top pole.

**Layer and reveal.** `h_i` is clamped to `[0, 1.20]`. The layer index `L_i = floor(h_i/0.20)`. A layer breakthrough emits `LAYER` when `L_i` decreases. A fragment at `(φ_f, λ_f)` is revealed when the mean `h` over its 3 × 3 cell patch is < 0.15 mm; the reveal is latched, so a later glaze cannot un-reveal it. The fragment's star connects to any revealed fragment within 30° great-circle distance; a connected component of ≥ 7 fragments is a recorded constellation at tide end.

**Tide front.** The front is `φ_t(t) = −80° + 165°·(t/T_tide)`, with `T_tide = 50 s`; it moves at 3.3°/s. A cell is shadowed when `φ < φ_t`. Shadowed ash has `H_i` raised by 0.5 and abrasion multiplied by 0.30, so late polishing is possible but expensive. A fragment is lost when `φ_t > φ_f − 5°`; the 5° guard band gives the player a 1.5 s visual warning.

**Signal chain.** 72 Hz joints → One Euro (mincutoff 1.0 Hz, beta 0.018, derivative cutoff 7 Hz) → 90 Hz interpolation → nearest point on the spheroid → pad penetration and tangential path → 6 Hz pressure filter → abrasion integrator at 90 Hz. The heightmap is updated in a 32 × 16 active window around the thumb to keep the per-tick cost under 0.4 ms; a full-map pass runs only at tide boundaries.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to the left palm: origin 12 mm above the palm centre along `ẑ_palm`, `x̂_V` from wrist to middle MCP, `ŷ_V` from thumb to pinky, `ẑ_V` along the palm normal. The stone is drawn at its tracked pose `P` inside `V`, clamped to ±6 mm from the anchor so a small regrip does not make the chart swim. The chart, ash heightmap, and fragments are `P`-fixed; the tide curtain, fragment labels, and constellation lines are `V`-fixed. This split is the core spatial idea: turning the stone moves the sky under your thumb, but the tide stays at the rim of your palm.

| Entity | V-space position | Size | Notes |
|---|---|---|---|
| Stone | `V` origin + tracked offset | 75 × 75 × 34 mm | Clamped ±6 mm |
| Ash surface | spheroid `P` surface + 0–1.2 mm | 96 × 48 grid | Displaced shell |
| Star chart | spheroid `P` surface + 0.3 mm | 2048 × 1024 texture | Under the ash |
| Fragment marker | `P` surface at `(φ_f, λ_f)` | 3.5 mm disc | 10 per tide |
| Tide curtain | `φ = φ_t`, radius 44–60 mm | Annular volume | V-fixed, dark, 0.45 alpha |
| Constellation line | between revealed fragments | 0.6 mm emissive line | P-fixed |
| Thumb halo | contact point | 7 mm ring | Shows pressure and speed |
| Tide clock | above the stone, player-facing | 28 mm arc | HUD, dry |
| Layer pips | left of the stone | 6 pips | Shows mean layer |

Mapping formulas: the thumb contact `p_P` converts to `(φ, λ)` by `φ = asin(clamp(p_P.z/c, −1, 1))` and `λ = atan2(p_P.y, p_P.x)`. The heightmap cell is `(floor((λ/360°)·96), floor(((φ+90°)/180°)·48))`. The tide shadow test uses the same `φ`. The fragment's screen position is `V_from_P(q_P)·p_f`; its label is billboarded toward the eye but its anchor stays on the stone, so labels do not drift when the stone turns.

## 8. Gameplay Systems & Rules

**Entities.** Stone with a 96 × 48 ash heightmap; 10 fragments per tide; one tide front; one constellation record per tide. The player's thumb is the only abrasive effector. There are no enemies and no physics bodies; the tension is a timer plus a finite abrasion budget.

**Tide structure.** Each tide starts with `h = 1.20 mm` over the top cap and a fresh set of 10 fragments. The tide lasts 50 s. At `t = 0` the front is at −80°; at `t = 50 s` it is at +85°. Fragments at φ = 15°, 30°, 45°, 60° are covered at 27.3 s, 31.8 s, 36.4 s, 42.4 s respectively, so the low-latitude fragments are the urgent ones. The player can turn the stone to bring a high-latitude fragment under the thumb, but turning costs 0.4–0.8 s and risks losing the cradle.

**Abrasion budget.** A focused 20 mm oval at 120 mm/s covers a 3 × 3 fragment patch with ~85 % duty. At nominal pressure, a fragment needs 6 × 1.11 s / 0.85 = 7.8 s of rubbing; with smooth-oval `S = 1.25`, 6.3 s. Ten fragments at 6.3 s each is 63 s, longer than the 50 s tide, so the player cannot save all ten. The intended target is 7 of 10; the remaining three are deliberately sacrificed to the tide. A player who maintains `H = 1.0` and avoids glaze can save 7–8.

**Glaze.** If the player presses > 0.50 N or rubs > 200 mm/s, `H` rises; at `H = 2.0` the local removal rate halves. A glazed patch is visible as a darker, shinier area. The player can recover by rubbing at < 0.40 N and < 100 mm/s for 2–5 s, which decays `H` at 0.10/s. Glaze is the game's "too much effort is counterproductive" mechanic.

**Reveal and constellation.** A fragment reveals at mean `h < 0.15 mm`. The reveal is worth 200 points and emits a 0.5 s star ignition. At tide end, the largest connected component of revealed fragments within 30° is counted; ≥ 7 records the constellation and awards 500 points. Four of five recorded constellations win. A tide with ≤ 6 revealed is "faded" but still counts toward the run's fragment total.

**Shadow tide.** The curtain is a translucent dark annulus that climbs the stone. Shadowed cells are 70 % harder to abrade and lose their chart emission. A fragment covered by the front is lost and its star turns to a grey cinder. The tide never recedes within a tide; it resets between tides.

**Score.**

`fragment_score = 200 · (1 + 0.15·min(combo, 5))`

`layer_score = 25` per layer breakthrough across the whole stone

`polish_score = 50` per qualifying smooth oval, max 8 per tide

`constellation_score = 500 + 10·time_remaining`

`run_score = Σ fragment + Σ layer + Σ polish + Σ constellation`

Combo increments on each fragment revealed within 4.0 s of the previous one; it resets when a fragment is lost to the tide or when the thumb lifts for > 1.0 s. The combo term rewards a smooth route across adjacent fragments, which is also the most efficient abrasion path.

**Win/lose.** Record four of five constellations to win. Record three or fewer after five tides to lose. A tide with zero revealed fragments is an immediate loss of the run, because the chart has gone completely dark. There is no hull or health meter; the tide is the only pressure.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load chart, start tracker | PRESENT |
| PRESENT | stone registered | RMS < 0.5 mm | Show cradle ghost | CALIBRATE |
| CALIBRATE | left cradle ≥ 3 pads for 1.0 s | top normal within 10° of palm normal | Set `V` anchor, `λ = 0` | TIDE_INTRO |
| TIDE_INTRO | timer 2.5 s | tide ≤ 5 | Seed 10 fragments, reset ash | RUB |
| RUB | tick | tide clock > 0 and cradle valid | Abrasion, reveal, tide front | RUB |
| RUB | fragment mean `h < 0.15 mm` | not already revealed | Ignite star, score, combo++ | RUB |
| RUB | `φ_t > φ_f − 5°` | fragment unrevealed | Cinder VFX, fragment lost | RUB |
| RUB | tide clock = 0 | — | Freeze front, count constellation | TIDE_END |
| RUB | cradle < 3 pads | > 250 ms | Pause tide, show cradle prompt | CRADLE_HOLD |
| CRADLE_HOLD | cradle restored | 3 pads for 0.4 s | Resume at same clock | RUB |
| TIDE_END | constellation ≥ 7 | tide < 5 | Record, 500 + time bonus | INTERMISSION |
| TIDE_END | constellation < 7 | tide < 5 | Fade, no record | INTERMISSION |
| TIDE_END | tide = 5 | records ≥ 4 | Results, win | RESULTS |
| TIDE_END | tide = 5 | records ≤ 3 | Results, loss | RESULTS |
| INTERMISSION | timer 10 s | — | Show chart progress, next tide | TIDE_INTRO |
| ANY | tracker loss > 1.5 s | — | Pause tide, zero abrasion | RECOVERY |
| RECOVERY | re-acquired | RMS < 0.9 mm | Resume with 0.5 s grace | previous |

Timers run on the 90 Hz tick; the tide clock is wall-clock-latched and pauses only in `CRADLE_HOLD` and `RECOVERY`, because those are tracking failures, not gameplay. The tide front is recomputed from the clock, not integrated per frame, so a frame stall cannot make the shadow jump unevenly.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 stone | Spheroid 37.5/37.5/17 mm | Tracked `P` | No rigid-body simulation |
| C1 thumb | Capsule r = 6.5 mm | Tracked | Abrasion effector |
| C2 palm | 3 capsules r = 12 mm | Tracked | Cradle validity |
| C3 ash | Heightfield 96 × 48 | Virtual | Rendering only; no collision |
| C4 fragment | Disc r = 3.5 mm | Fixed in `P` | Reveal test |
| C5 tide | Annular front | Clock-driven | Shadow test |
| C6 index | Capsule r = 5 mm | Tracked | Brace only |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C1 × C0 | signed distance < 5 mm, `φ > +5°` | Abrasion update in a 6.5 mm disc |
| C1 × C3 | same as above | Heightmap `h_i` decreases; no physical contact |
| C2 × C0 | signed distance < 5 mm | Cradle support; `V` validity |
| C6 × C0 | signed distance < 5 mm | Brace; damps stone rotation 40 % |
| C4 × C3 | mean `h` over 3 × 3 cells < 0.15 mm | Fragment reveal |
| C5 × C4 | `φ_t > φ_f − 5°` | Fragment lost |
| C0 × C0 | — | No self-collision |

**Materials.** Stone = "basalt": µ = 0.46 dry, 0.62 damp; no restitution because it is never dropped in play. Ash = "chalk": virtual only; its removal is the abrasion law, not a contact response. Thumb pad = "skin": viscoelastic, modelled as a 60 N/m spring with 0.08 N preload. The tide = "shadow": no collision, only a scalar coverage test.

**CCD.** The thumb moves at most 2.2 mm/tick at 200 mm/s, below the 6.5 mm pad radius, so discrete distance tests are sufficient. The heightmap is updated at the contact point each tick; there is no swept abrasion because a 2.2 mm step is small relative to the 2.45 mm cell. A thumb flick > 300 mm/s is clamped to 200 mm/s for abrasion, which also prevents a single frame from carving a trench.

**Forces and feedback.** The pressure proxy `p = 60·δ` is the only force in the game. The normal load on the stone is the sum of the thumb and palm contacts, but it is not simulated; the stone is tracked, not pushed. The virtual ash removal is an integration, not a force response:

`h_i ← clamp(h_i − k_w·(p/p₀)·(v/v₀)^0.7·S·(1/H_i)·w_i·dt, 0, 1.20)`.

The smooth falloff `w_i = 1 − (d_i/6.5)²` gives a polished crater with a soft edge, which reads correctly under the ash shader.

## 11. Rendering, Occlusion & Compositing

Layer order, near to far: passthrough base → left palm scene mesh → stone depth proxy → bedrock star chart (emissive) → ash shell (displaced 0–1.2 mm) → fragment stars and constellation lines → tide curtain → thumb and right-hand mesh → HUD and layer pips. The ash shell is drawn 0.2 mm outside the stone proxy; the chart is drawn 0.3 mm inside the proxy, so the chart is visible only where the ash has been removed.

**Occlusion cases.**

- **O1 — Thumb over the polish crater.** The tracked thumb mesh wins the depth test. The thumb is visible over the ash; a 1.5 mm contact halo is drawn under the thumb mesh so the pad does not appear to float. The halo color maps pressure (amber at 0.1 N, white at 0.5 N).
- **O2 — Stone over the palm.** The stone proxy occludes the palm; the cradle pads are drawn around the stone's lower cap, not through it. A 2 mm soft shadow on the palm seats the stone.
- **O3 — Tide curtain behind the stone.** The curtain is an annulus at the stone's rim; it is depth-tested against the spheroid, so the far half is hidden and the near half reads as a rising shadow. The curtain never draws over the thumb.
- **O4 — Far-side chart.** Star chart cells with `(q_P·p̂)·(eye − centre) < 0` are culled by the normal test and the depth test. Labels for far-side fragments are hidden, not dimmed, to avoid an x-ray look.
- **O5 — HUD versus thumb.** The layer pips and tide clock are rendered at 250 mm from the eye and depth-tested against the hand. A raised thumb correctly covers the pips; the tide clock is duplicated as a 2 mm arc on the palm so the player never loses the timer.
- **O6 — Ash particles.** The 0.2 mm ash flakes are drawn between the ash shell and the hand. They are clipped by the thumb mesh; a flake that would appear inside the thumb is culled rather than alpha-blended, because a flake through skin breaks the illusion.

**VFX budgets.** Ash shell: one 96 × 48 displacement mesh, 4.6 k triangles, 0.9 ms. Chart: one 2048 × 1024 emissive texture, 0.3 ms. Fragment stars: 10 sprites + 9 line segments per constellation. Ash flakes: 180 particles max, 2.0 ms. Tide curtain: one annulus, 96 segments, 0.2 ms. Contact halo: 1 ring, 32 segments. Total transparent overdraw target < 1.6× screen area. The ash shader is opaque with a 3-band toon ramp, not transparent, so the real stone's silhouette stays readable.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Thumb contact | thumb | Soft 40 Hz pad touch, 60 ms | 7 mm contact halo appears |
| Rub loop | thumb | Granular chalk texture, 200–900 Hz, gain ∝ `v` | Ash flakes, 6/s |
| Layer breakthrough | contact point | 600 Hz paper-tear tick, 80 ms | One layer pip goes dark |
| Glaze warning | contact point | 1.8 kHz glassy tone, 120 ms | Patch darkens, sheen appears |
| Fragment reveal | fragment | 1.3 kHz star chime + 300 Hz bloom, 400 ms | Star ignites, line to neighbour |
| Combo step | fragment | Chime pitch +2 semitones per step | Combo number scales 1.0→1.2 |
| Tide advance | stone rim | Low 55 Hz rumble, continuous, gain ∝ `φ_t` | Curtain climbs, chart dims |
| Fragment lost | fragment | 180 Hz cinder hiss, 300 ms | Star greys out, small smoke |
| Tide 10 s warning | HUD | 2 × 880 Hz pulse | Clock arc flashes |
| Tide end | stone | Warm 220 Hz gong, 700 ms | Curtain freezes, constellation draws |
| Constellation recorded | chart | Major arpeggio, 900 ms | 7 stars connect, record stamp |
| Cradle lost | palm | 120 Hz muted thud | "Cradle the stone" prompt |
| Tracker loss | head-locked | 180 Hz pulse | "Show the stone" prompt |

World cues are spatialised with a 1.0 m rolloff; HUD cues are dry. The rub loop is the primary feedback channel: its granular density tracks thumb speed, and its pitch drops 15 % when the thumb is over a glazed patch, so the player can hear hardness without looking. There is no haptic substitute for the polish itself; the chalk texture and the layer tick are the stand-ins.

## 13. Difficulty Progression & Tuning

| Level | Tide (s) | Fragments | Need | Ash hardness | Tide start | Rub gain | Notes |
|---|---|---|---|---|---|---|---|
| 1 | 60 | 8 | 5 | 0.80 | −80° | 1.15× | Teaching: fewer fragments, slow front |
| 2 | 55 | 9 | 6 | 0.90 | −80° | 1.08× | First glaze risk |
| 3 | 50 | 10 | 7 | 1.00 | −80° | 1.00× | Baseline spec |
| 4 | 45 | 11 | 8 | 1.10 | −70° | 0.94× | Front starts higher, less time |
| 5 | 40 | 12 | 9 | 1.20 | −60° | 0.88× | Finale: 12 fragments, fast front |

Named constants: `A = 0.0375 m`, `C = 0.017 m`, `R_P = 82.72 mm`, `ASH_TOTAL = 1.20 mm`, `LAYERS = 6`, `LAYER_H = 0.20 mm`, `GRID = 96 × 48`, `K_WEAR = 0.18 mm/s`, `P0 = 0.40 N`, `V0 = 80 mm/s`, `SMOOTH_GAIN = 1.25`, `HARD_MAX = 2.0`, `GLAZE_RATE = 0.25/s`, `RECOVERY_RATE = 0.10/s`, `TIDE_BASE = 50 s`, `FRONT_START = −80°`, `FRONT_END = +85°`, `REVEAL_H = 0.15 mm`, `FRAGMENTS = 10`, `CONSTELLATION_MIN = 7`, `TIDE_WIN = 4`.

Adaptive rule: after each tide, if the player recorded the constellation with ≥ 8 fragments and ≥ 5 s left, reduce the next tide by 3 s (floor 38 s) and add one fragment (cap 12). If the player recorded ≤ 5 fragments or ran out of time with ≥ 3 fragments still at layer 5, add 4 s (ceiling 62 s) and remove one fragment (floor 8). Adaptation never changes `K_WEAR`, `P0`, or the 0.20 mm layer height, because the physical abrasion feel must stay consistent.

## 14. Edge Cases & Failure Recovery

- **Thumb slips off the top cap.** Contact is lost; abrasion stops immediately and the charge-like "stroke" state resets. The heightmap keeps its current values, so a slip costs time but never progress. A 180 ms muted tick confirms the slip.
- **Stone rotates in the cradle.** `P` is tracked, so the chart rotates with the stone and the thumb's contact point is recomputed in `P`. If the rotation rate exceeds 90°/s, abrasion is suspended for 150 ms to avoid smearing a cell across the heightmap; the player sees a 2 mm motion-blur ring.
- **Cradle lost.** If fewer than three left-palm pads are within 5 mm for > 250 ms, the tide pauses and the game shows a cradle prompt. The pause is capped at 6 s; after that the tide resumes with a 0.5× front speed for 2 s, so a player who cannot re-cradle is not stuck forever.
- **Over-pressure.** `p` saturates at 0.55 N. The excess is converted to hardness, not removal, and a 1.8 kHz glassy tone warns the player. A fully glazed patch (`H = 2.0`) can still be polished at half rate; it is never permanently locked.
- **Tracking loss.** `P` freezes after 200 ms; abrasion is zeroed; the tide clock continues for 1.5 s, then pauses. Re-acquisition restores 0.5 s of grace. If loss exceeds 8 s, the tide restarts from `t = 0` with the current heightmap preserved but the front reset, so the player does not lose a tide to a tracker dropout.
- **Fragment covered during reveal.** If the tide front reaches a fragment in the same tick as its reveal threshold, the reveal wins; the fragment is latched before the loss test. This 1-tick priority is deterministic and prevents a visible star from turning into a cinder.
- **Left-handed player.** Mirror mode swaps the cradle to the right palm, the rubbing thumb to the left, mirrors `λ` about 0°, and flips the tide front's climb direction in the HUD. The heightmap and P-frame math are unchanged because they are defined on the spheroid, not on a hand side.
- **Heightmap saturation.** `h` is clamped at 0; a cell cannot go negative. If the player rubs the same cell for 30 s, the shader holds bedrock and emits a 400 Hz "already clean" tick once per 2 s, rather than carving through the chart.
- **Dark stone / low texture.** Basalt can starve visual odometry. The tracker falls back to the heightmap's own rendered ash features; if confidence < 0.4, the tide speed drops to 0.8× and a "hold the stone still" prompt appears after 3 s.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — registers the stone, emits `PoseFrame {p_P, q_P, confidence, mode}` at 72 Hz.
- `ContactBus` — 26 joints → `Contact {joint, p_P, n_P, v_rel, age}` with 5 mm enter / 8 mm exit and 30 ms debounce.
- `CradleMonitor` — three left-palm pads; `V` anchor validity; pause/resume.
- `AbrasionField` — 96 × 48 heightmap, hardness map, 6-layer indexing, 32 × 16 active window.
- `ThumbKinematics` — pad penetration, pressure proxy, geodesic disc, speed, oval classifier.
- `FragmentField` — 10 seeded fragments, reveal latching, 30° graph, constellation count.
- `TideController` — clock, front `φ_t`, shadow mask, fragment loss, 10 s warning.
- `TideStateMachine` — the table in §9.
- `ScoreCombo` — fragment, layer, polish, constellation scores.
- `FeedbackRouter` — every event enum to one audio clip and one VFX prefab.
- `Compositor` — layer order, spheroid depth proxy, occlusion masks O1–O6.

**Event flow.** `PropTracker` → `PoseFrame`; `ContactBus` → `ThumbKinematics` and `CradleMonitor`; `ThumbKinematics` → `AbrasionField`; `AbrasionField` → `FragmentField`; `TideController` → `FragmentField` and `TideStateMachine`; `FragmentField` → `ScoreCombo`, `FeedbackRouter`; `TideStateMachine` → `FeedbackRouter`. The abrasion field never reads the tide state directly; the tide supplies a shadow mask, which is the only coupling.

**Pseudocode 1 — per-tick abrasion.**

```
function tick_abrasion(dt):
    if not CradleMonitor.valid or TideController.paused:
        return
    c = ContactBus.nearest(R_THUMB_TIP)
    if c == null or c.phi < 5deg or c.n.z <= 0:
        return

    delta = clamp(c.penetration, 1.3mm, 9.2mm)
    p = min(60 N/m * delta, 0.55 N)
    p = lowpass(p, 6 Hz, dt)
    v = clamp(c.tangential_speed, 0, 200mm/s)
    S = 1.25 if ThumbKinematics.oval_active else 1.0

    for cell in AbrasionField.window(c.p_P, 6.5mm):
        d = distance(c.p_P, cell.p_P)
        w = max(0, 1 - (d/6.5mm)^2)
        shadow = 0.30 if cell.phi < TideController.phi_t else 1.0
        rate = K_WEAR * (p/0.40N) * (v/80mm/s)^0.7 * S * shadow
        cell.h = clamp(cell.h - rate*(1/cell.H)*w*dt, 0, 1.20mm)
        if p > 0.50N or v > 200mm/s:
            cell.H = min(2.0, cell.H + 0.25*dt)
        else:
            cell.H = max(1.0, cell.H - 0.10*dt)
        if cell.layer() < cell.last_layer:
            FeedbackRouter.play(LAYER, cell.p_P)
            cell.last_layer = cell.layer()
```

**Pseudocode 2 — tide, reveal, and constellation.**

```
function tick_tide(dt):
    phi_t = -80deg + 165deg * (TideController.t / 50s)
    TideController.set_front(phi_t)

    for f in FragmentField.active():
        mean_h = average([AbrasionField.h(c) for c in f.cells_3x3])
        if not f.revealed and mean_h < 0.15mm:
            f.revealed = true
            ScoreCombo.on_fragment(f)
            FeedbackRouter.play(REVEAL, f.p_P)
        if not f.revealed and phi_t > f.phi - 5deg:
            f.lost = true
            FeedbackRouter.play(LOST, f.p_P)

    if TideController.t >= 50s:
        comp = FragmentField.largest_component(30deg, revealed_only=true)
        record = (len(comp) >= 7)
        ScoreCombo.on_tide_end(record, TideController.time_left)
        enter(TIDE_END)
```

**Determinism.** Fragment seeds use a PCG32 stream keyed by `(session_id, tide, fragment_index)`. The heightmap is a dense float array with a fixed iteration order, and the active-window update is deterministic, so a recorded pose/contact trace replays bit-identically at 90 Hz.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Spheroid registration | Present the stone at 1.0 m; inject 24 synthetic rim points with 0.3 mm noise | RMS < 0.5 mm; yaw error < 2.0°; top normal within 10° of palm normal |
| AT-02 | Heightmap initialization | Start a tide; sample 100 cells at φ > +5° | `h = 1.20 mm ± 0.001`; layer index = 5 everywhere on the cap |
| AT-03 | Abrasion rate | Hold thumb at `p = 0.40 N`, `v = 80 mm/s`, `S = 1`, `H = 1` for 2.0 s | `h` falls 0.36 ± 0.02 mm; one layer breakthrough at 1.11 ± 0.05 s |
| AT-04 | Pressure saturation | Sweep `δ` from 1 to 12 mm | `p` clamps at 0.55 N; no removal above the clamp; hardness rises at 0.25/s |
| AT-05 | Smooth-oval classifier | Inject a 16 mm oval at 120 mm/s for 1.8 s; then a linear scrub | Oval sets `S = 1.25`; scrub sets `S = 0.85`; no false oval on a 0.5 s arc |
| AT-06 | Geodesic footprint | Contact at φ = 80°, λ = 0; read the affected cells | Affected set is a 6.5 mm chord disc; no lat/long ellipse at the pole |
| AT-07 | Layer indexing | Remove exactly 0.40 mm from a cell | Layer falls 5 → 3; one `LAYER` event; pip count matches |
| AT-08 | Fragment reveal | Rub a 3 × 3 patch to mean `h = 0.14 mm` | Fragment reveals once; +200 ± 1; star latches even if `h` is later raised |
| AT-09 | Tide front | Run 50 s with no input; sample `φ_t` at 0, 25, 50 s | −80°, +2.5°, +85° ± 0.5°; fragment at φ = 30° is lost at 31.8 ± 0.3 s |
| AT-10 | Constellation | Reveal 7 of 10 fragments within 30°; end the tide | Component = 7; constellation recorded; +500 + 10·time_left |
| AT-11 | Glaze recovery | Hold `p = 0.55 N` for 4 s, then `p = 0.30 N` for 10 s | `H` reaches 2.0, then decays to 1.0 ± 0.05; removal rate recovers |
| AT-12 | Occlusion readback | Place the thumb over a revealed star; read the depth buffer | Star fragment contribution = 0 under the thumb silhouette; halo is under the hand |
| AT-13 | Tracking loss | Drop `PoseFrame` for 2.0 s during RUB | Abrasion 0 after 200 ms; tide pauses at 1.5 s; 0.5 s grace on re-acquire |
| AT-14 | 10-minute soak | Scripted five-tide run at 90 Hz | Frame p99 < 11.1 ms; heightmap deterministic; no cell below 0 or above 1.20 |

All tests run headless with synthetic pose/contact streams. AT-12 is a depth-buffer readback, not a screenshot. AT-02, AT-03, and AT-07 share the same shipped heightmap and layer constants, so a tuning change that breaks the abrasion budget fails the tests rather than silently changing the tide balance.
