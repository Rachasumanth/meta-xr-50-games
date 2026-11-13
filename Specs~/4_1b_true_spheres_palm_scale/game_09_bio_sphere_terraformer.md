# Game 9 — Bio-Sphere Terraformer — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Bio-Sphere Terraformer |
| Slug | `bio-sphere-terraformer` |
| Physical prop | Apple with stem, true sphere, 72.0 mm diameter (r = 36.0 mm); 180 g; stem 9 mm |
| Family / sub-shape | Family 4 Spherical, Ellipsoidal & Domed / 4.1b True Spheres, Palm-Scale |
| Scale class | Palm-scale globe on the knee; prop 1:1; biosphere rendered 1:1 on the apple |
| Source concept | "Directional sun from the left, rain clouds on the right. Spin the apple to give trees equal sunlight and water while starving shadow weeds." |
| Physical input | Pinch the stem to spin the apple like a globe; tap the shell with the free index to plant |
| Player posture | Seated, apple on the right knee, left hand pinching the stem, right index tapping |
| Session length | 90.0 s season + 3 s brief + 8 s debrief |
| Win condition | ≥ 36 mature trees and weed coverage ≤ 12 % at t = 90 s |
| Lose condition | Weed coverage ≥ 35 % at any time |
| Primary metric | Mature trees per percent of weed coverage |
| Tracking budget | 2 stem-pinch fingertips, 1 planting index, 1 prop pose |

## 1. Fantasy & Core Loop

The apple is a small world with a real stem for a rotation handle. The player is its terraformer: the stem is the north pole, the skin is a 72-cell map, and the left hand can spin the world under a fixed sun on the left and fixed rain on the right. Because the sun and rain are directional and opposite, every cell receives a latitude-dependent mix of light and water over one rotation. Equatorial cells get a balanced 50/50; the pole tilted toward the sun gets permanent daylight and dies of drought; the pole tilted toward the rain gets permanent rain and grows shadow weeds.

The core loop is a balance problem with a physical spin. The player plants trees where the light/water product is high, spins to accumulate growth-days, and periodically tilts the planet's axis to burn the weeds that colonize the rain pole. Spinning faster grows the season faster but risks centrifugal uprooting; tilting too far creates a new weed pole while curing the old one.

1. **Set the axis (2–4 s).** Rock the apple on the knee until the stem leans 15–25° toward the sun; the terminator shifts and the cell tint updates.
2. **Plant (0.2 s per cell).** Tap a cell; a seed costs 1 and germinates in 4 s.
3. **Spin (continuous).** Pinch the stem and twist at 90–220 °/s; growth accumulates per simulated day.
4. **Harvest (0.2 s).** Tap a mature tree for +3 seeds and +120 score; the cell resets to a sapling.
5. **Weed (every 8–12 s).** A weed patch appears at the dark pole; tilt the axis to bring it into sunlight before coverage reaches 35 %.
6. **Season close (90 s).** Count mature trees and weed coverage; the next season unlocks at a higher level.

The fantasy is ecological rather than combative: the enemy is a geometry error, not a monster. A cell at 50/50 light is a rain forest; a cell at 100/0 is a desert; a cell at 0/100 is a swamp. The player is always trying to move the whole map toward the middle.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the center, poles at `±ẑ_P`, equator in `x̂_P`–`ŷ_P`, no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]` from the equator, longitude `λ ∈ [0°, 360°)` from +x̂_P toward +ŷ_P; `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; outward normal `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P-frame**: origin at the apple centre; `+ẑ_P` is the **stem axis** (stem protrudes along `+ẑ_P`), `−ẑ_P` the calyx. Poles `±ẑ_P`; equator in `x̂_P`–`ŷ_P`; `λ = 0°` through the stem scar. The apple's geometric long axis is the stem axis by registration definition; the sphere sub-shape is used for the body.
- **Radius and mass**: `r = 36.0 mm`, `m = 180 g`, `I = 2mr²/5 = 1.866 × 10⁻⁴ kg·m²`. Stem collider: capsule r = 1.2 mm, length 9 mm along `+ẑ_P`, used only for pinch.
- **Spin**: stem pinch plus wrist twist drives `ω_spin` about `+ẑ_P`, clamped to ±360 °/s. Any shell contact also contributes `ω = (n × v_t)/r`; the two estimates are fused 70 % stem / 30 % shell. The spin changes the day phase but not the cell latitudes.
- **V-frame anchoring**: `V` is anchored to `T` (the knee/lap surface). Origin `V_O = T_O + (0, 260 mm, 0)`; axes aligned with `W`. The apple is drawn at `V_O` with its true `P` orientation; the sun and rain are `W`-fixed directional sources. This keeps the light and clouds still while the apple spins.
- **Deviation 1 — knee support**: `T` is a lap plane, not a table. The lap detector supplies a 200 mm × 160 mm support patch; if no lap is detected, the game falls back to the table with a 15° higher gaze.
- **Deviation 2 — sphere body, real stem**: the apple is treated as a true sphere for exposure; the stem is a separate non-spherical collider used only for the spin gesture.
- **Deviation 3 — exposure is analytic, not ray-traced**: the daily sun fraction is a closed-form latitude integral; no per-cell shadow rays.
- **Deviation 4 — tilt is a control**: the axis tilt is measured from `P`, but only its component along the sun–rain line changes exposure; the perpendicular component changes the day phase and is ignored by the growth model.

## 3. Physical Play Space & Ergonomics

The player sits with the right knee slightly higher than the left, the apple resting on a small towel or directly on the trouser fabric. The left hand approaches from above; the thumb and index pinch the stem with 0.2–0.6 N. The right hand rests on the right thigh, index free to tap the apple's exposed left/upper shell. The head looks down 25–32°, and the apple is 260–320 mm below the eyes.

| Parameter | Value | Rationale |
|---|---|---|
| Support | Right lap `T`, 200 × 160 mm patch | Stable globe base, no table needed |
| Apple centre | 260 mm above `T`, 140 mm forward | Comfortable downward gaze |
| Stem pinch | 0.2–1.0 N, 6–14 mm aperture | Enough friction to twist, no crush |
| Spin rate | 60–240 °/s recommended | 1.5–6.0 s per simulated day |
| Axis tilt | 0–25° from vertical | Keeps the globe on the knee |
| Tap reach | exposed shell within 75° of the eye direction | Right index can reach most of the near side |
| Season break | 8 s after the run | Wrist and neck recovery |

The apple rolls if released. The lap support has a 4 mm deep cloth depression; the apple is stable up to 12° of knee tilt, which is why the tilt control is limited to 25° and the game warns at 20°. The player is never asked to balance the apple while spinning; the stem pinch is light enough that the apple stays seated.

## 4. Prop Geometry & Registration

- **Real dimensions**: 72.0 mm diameter, 180 g, stem 9.0 mm long and 2.0 mm at the base, calyx 12 mm across. Skin texture is the odometry feature set.
- **Sphere model**: `|p_P| = 36.0 mm`. The biosphere atlas is 6 latitude bands × 12 longitude cells = 72 cells. Band boundaries at `φ = −60°, −30°, 0°, +30°, +60°`; cell centres at `φ = ±75°, ±45°, ±15°`. Cell area = `4πr²/72 = 226.2 mm²`.
- **Surface features**: each cell has a 0.4 mm boundary groove and a 1.0 mm center glyph (seed, sapling, tree, or weed). The stem scar is at `λ = 0°`; the calyx at `−ẑ_P` carries a 3 mm dark rosette used as the south marker.
- **Colliders**: `COL_sphere` r = 36.0 mm (tap, shell contacts); `COL_stem` capsule r = 1.2 mm, l = 9.0 mm (pinch only); `COL_palm_L` capsule r = 17 mm, l = 62 mm (support for the pinch hand); `COL_index_R` swept capsule r = 5.5 mm, l = 15 mm. No collider for trees or weeds.
- **Registration**: the player presents the apple at 280 mm with the stem up. The system fits the centre over 8 frames (< 0.8 mm), finds the stem tip (the 9 mm protrusion), and sets `+ẑ_P` to the stem axis within 2°. `λ = 0°` is the stem scar's meridian. If the stem is not visible for 2.0 s, a "present the stem" card appears.
- **Tolerance**: centre < 0.8 mm; radius < 0.6 mm; stem axis < 2°; meridian < 6°. A meridian error over 6° is corrected by a 2 s "show the calyx" rotation.
- **Tracking**: visual odometry at 72 Hz on the skin speckle; the stem is a strong depth cue. Re-acquisition searches 65 mm for 1.5 s and blends 120 ms.
- **Stem detection**: the stem is segmented as a 2 mm × 9 mm cylinder; if the player's fingers occlude it, the system falls back to the calyx axis and shows a 1.0 s "stem hidden" hint.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Stem pinch | `L_thumb_tip` + `L_index_tip` | stem capsule | spin authority; grip valid |
| I5.2 | Twist clockwise | pinch roll `+dθ/dt` about `+ẑ_P` | stem | `ω_spin > 0`, day advances |
| I5.3 | Twist counter-clockwise | pinch roll `−dθ/dt` | stem | `ω_spin < 0`, day reverses |
| I5.4 | Release stem | pinch aperture > 14 mm | — | spin coasts to 0 at 180 °/s² |
| I5.5 | Rock the axis | left palm heel on the calyx, tilt | lap contact | `δ` (sun elevation) changes |
| I5.6 | Plant seed | `R_index_tip` tap on empty cell | shell | −1 seed, germination starts |
| I5.7 | Harvest | `R_index_tip` tap on mature tree | shell | +3 seeds, +120 score |
| I5.8 | Inspect cell | index hover < 8 mm for 0.4 s | shell | cell tooltip: sun %, water %, biomass |
| I5.9 | Two-hand cradle | left palm + right palm | shell | pause spin; no score |
| I5.10 | Re-seat | apple lifted and replaced on lap | `T` contact | axis re-anchored, season continues |

Per-finger roles: left thumb and index are the only spin authority; the left middle/ring/pinky stabilize the apple from below. The right index is the only planting/harvesting effector; the right middle is ignored to prevent accidental taps. Per-hand roles: left = SPIN/AXIS, right = PLANT.

## 6. Input Kinematics & Signal Processing

- **Stem pinch gate**: both `L_thumb_tip` and `L_index_tip` within 8.0 mm of the stem capsule surface; aperture 6–14 mm; held 80 ms. If either joint leaves by > 10 mm, the pinch releases.
- **Twist extraction**: the stem pinch vector `d̂` is the unit vector from thumb tip to index tip projected onto the plane normal to `ẑ_P`. `ω_stem = d/dt atan2(d̂·ŷ_P, d̂·x̂_P)`, filtered with a 40 ms EMA, clamped to ±360 °/s. A twist below 15 °/s is treated as zero.
- **Shell spin**: for a shell contact with normal `n` and tangential velocity `v_t`, `ω_shell = (n × v_t)/r`. Fuse `ω = 0.7·ω_stem + 0.3·ω_shell`; integrate `R_P ← expm([ω]× dt)·R_P` at 90 Hz.
- **Axis tilt**: `â = R_P·ẑ_P`; the sun direction is `d̂_sun = −x̂_W`, rain `d̂_rain = +x̂_W`. The effective sun elevation is `δ = asin(clamp(â·d̂_sun, −1, 1))`. The tilt magnitude is `|acos(â·ŷ_W)|`, clamped to 25°; beyond 25° the apple is considered lifted and the spin pauses.
- **Daily sun fraction**: for a cell at latitude `φ` on the spinning sphere, the fraction of one rotation for which the cell normal has a positive dot product with the sun is
  `f_sun(φ) = (1/π)·acos(clamp(−tanφ·tanδ, −1, 1))`.
  The rain fraction is `f_rain(φ) = 1 − f_sun(φ)` exactly, because the sun and rain directions are opposite and horizontal. At `δ = 23.5°`: `f_sun(0°) = 0.500`, `f_sun(+30°) = 0.581`, `f_sun(−30°) = 0.419`.
- **Balance**: `G(φ) = 4·f_sun·f_rain`, which is 1.000 at 50/50, 0.974 at ±30°, 0.704 at ±60°, and 0 at the poles. A tree needs `G ≥ 0.55` to grow.
- **Growth**: `db/dt = k_g·(G − 0.50)·s(ω)` for `b < 1`, with `k_g = 0.14/s` and `s(ω) = clamp(|ω|/120 °/s, 0, 1.5)`. At `G = 1`, `ω = 120 °/s`, biomass grows 0.07/s → 14.3 s from seed to mature. Biomass is clamped to [0, 1]; maturity at `b ≥ 0.90`.
- **Centrifugal limit**: `|ω| > 280 °/s` adds an uproot risk of `0.10/s` per sapling (`b < 0.30`); at 360 °/s the risk is 0.20/s. Mature trees are immune.
- **Weed growth**: `dw/dt = 0.060·(0.30 − f_sun)/0.30` when `f_sun < 0.30`, else `dw/dt = −0.120·(f_sun − 0.30)/0.70`. A weed at the rain pole (`f_sun = 0`) reaches full biomass in 16.7 s. Weeds spread to a neighboring cell at `0.020/s` when `w > 0.60`.
- **Planting tap**: contact `s < 5 mm`, `D = 5 − s ≥ 2 mm`, duration ≤ 140 ms, cell empty. Cost 1 seed; germination 4.0 s; the cell enters `SAPLING`.
- **Harvest tap**: same classifier on a cell with `b ≥ 0.90`; +3 seeds, +120 score, `b ← 0.25`, cell becomes `SAPLING` with a 2.0 s harvest cooldown.
- **Cell mapping**: `φ = asin(p_z/r)`, `λ = atan2(p_y, p_x)`; band index `floor((φ + 90°)/30°)`, clamped 0–5; longitude index `floor(λ/30°)`. A 0.5° hysteresis at boundaries prevents a tap from flipping cells.
- **Seed economy**: start 20, maximum 30, passive +1 every 4.0 s, harvest +3, planting −1.

## 7. Virtual World Layout & Coordinate Mapping

- **V frame**: `V_O = T_O + (0, 260, 0) mm`, axes equal to `W`. The apple centre is `V_O`; the apple is drawn with `R_P` so the stem and cells rotate correctly.
- **Sun**: a directional light from `d̂_sun = −x̂_W`, color 5,800 K at 3.2 cd/m² on the lit hemisphere, with a soft 8° penumbra. A 120 mm sun disc is drawn 600 mm to the player's left, anchored to `W`.
- **Rain**: a 180 mm cloud bank at `+x̂_W` 500 mm to the right, with 300 rain streaks falling along `−ŷ_W`; rain wets the right-facing hemisphere. Rain is visual only; the water fraction is analytic.
- **Terminator**: the boundary between lit and wet is a great circle on the apple, computed from `d̂_sun`; it is rendered as a 2 mm glowing seam that moves as the axis tilts.
- **Cells**: 72 tinted quads on the sphere, each with a 0.4 mm groove; tint mixes dry ochre (sun-rich), green (balanced), and blue-green (rain-rich). Trees are 1.2 mm cones with 3 tiers; weeds are 0.8 mm dark tufts.
- **Gauge**: a 70 mm arc above the apple showing mature trees (0–72) and weed coverage (0–100 %), with the win thresholds at 36 and 12 %.
- **Tooltip**: a 30 mm × 18 mm card at the hovered cell, offset 12 mm along the normal, showing `sun %`, `water %`, `G`, and biomass.
- **Season clock**: a 40 mm ring at `V_O + (0, 90, 0) mm`, draining clockwise over 90 s.
- **Camera**: no virtual camera; the player looks down at the apple. The sun and cloud are placed to the left and right of the apple at 25–35° above the table plane so they remain in peripheral view without requiring head turns.

## 8. Gameplay Systems & Rules

- **Entities**: `Apple` (P, stem, cells), `Cell` (φ, λ, biomass, weed, state, cooldown), `SunModel` (δ, f_sun, f_rain), `SeedBank`, `SeasonDirector` (90 s, trees, weeds, score), `TerraformRig` (cells, trees, weeds, terminator).
- **Cell states**: `EMPTY` → `SEED` (4 s) → `SAPLING` (b < 0.45) → `GROWING` (0.45 ≤ b < 0.90) → `MATURE` (b ≥ 0.90, harvestable). A weed overlay can coexist with any state except `MATURE`; a mature tree suppresses weeds in its cell (`dw/dt ×0.2`).
- **Tree growth**: `db/dt = 0.14·(G − 0.50)·s(ω)·(1 − w)`; if `G < 0.55`, growth is zero and biomass decays at 0.01/s. `s(ω) = clamp(|ω|/120, 0, 1.5)`.
- **Weed growth**: as in §6; weeds are suppressed by a mature tree and by `f_sun > 0.30`. Weed coverage is the fraction of the 72 cells with `w > 0.50`.
- **Weed seeding**: every 9.0 s, the director picks the reachable cell with the lowest `f_sun` and no mature tree, and sets `w = 0.15`. If all low-sun cells are occupied, it spreads from the highest-`w` neighbor.
- **Axis tilt**: `δ` is controlled by rocking the apple. The player can reach `δ ∈ [−25°, +25°]`; the game shows the resulting `f_sun` curve as a 5-point strip (poles and ±45°, equator).
- **Planting**: a tap on `EMPTY` costs 1 seed and starts a 4 s germination. A tap on `SEED` or `SAPLING` does nothing but shows the tooltip. A tap on `MATURE` harvests.
- **Scoring**: plant +10; mature tree +150 (awarded once at maturity); harvest +120 and +3 seeds; weed burned (w crosses below 0.10) +40; season end +20 per mature tree; weed penalty −30 per percent above 12 %.
- **Win/lose**: at t = 90.0 s, `mature ≥ 36` and `coverage ≤ 12 %` → win. `coverage ≥ 35 %` at any tick → immediate loss. A season with `mature < 36` is a loss but still advances the level down.
- **Season clock**: 90.0 s of wall time, frozen during pause, re-seat, and prop reacquisition. Growth uses the fixed 90 Hz tick and the spin integral, so a paused season resumes exactly.
- **Adaptive target**: at level 1 the win threshold is 26 trees; at level 5 it is 42. The HUD always shows the current threshold.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load season config | REGISTER |
| REGISTER | apple presented | centre < 0.8 mm | fit sphere, find stem | CALIBRATE |
| CALIBRATE | stem axis found | axis < 2° | set `P0`, meridian, zero clock | SEAT |
| SEAT | apple on lap 400 ms | lap `T` valid | anchor `V`, record tilt | BRIEF |
| BRIEF | stem pinch 300 ms | aperture 6–14 mm | controls card 3.0 s | SEASON |
| SEASON | tick | seated | spin, exposure, growth, weeds | SEASON |
| SEASON | planting tap | empty cell, seeds > 0 | −1 seed, `SEED` 4 s | SEASON |
| SEASON | harvest tap | `b ≥ 0.90` | +3 seeds, +120, `b = 0.25` | SEASON |
| SEASON | weed seeding timer | every 9.0 s | lowest-sun cell `w = 0.15` | SEASON |
| SEASON | coverage ≥ 35 % | — | blight bloom, fail | DEBRIEF |
| SEASON | t = 90 s | mature ≥ threshold, coverage ≤ 12 % | win, tally | DEBRIEF |
| SEASON | t = 90 s | otherwise | fail, tally | DEBRIEF |
| SEASON | apple lifted > 300 ms | — | freeze clock and growth | RESEAT |
| RESEAT | apple on lap | within 60 mm of `V_O` | re-anchor tilt, blend 120 ms | SEASON |
| RESEAT | 5.0 s timeout | — | "seat the apple" card | RESEAT |
| any | prop tracking loss > 150 ms | — | freeze season, ghost apple | REACQUIRE |
| REACQUIRE | relock ≤ 1.5 s | search 65 mm | blend 120 ms | prior |
| REACQUIRE | 1.5 s timeout | — | "present the apple" card | REACQUIRE |
| any | system pause | — | freeze all timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: germination 4.0 s; harvest cooldown 2.0 s; weed seed 9.0 s; passive seed 4.0 s; re-seat 5.0 s; season 90.0 s; brief 3.0 s; cell hover 0.4 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_sphere` r = 36 mm; `COL_stem` capsule r = 1.2 mm, l = 9 mm; `COL_palm_L` capsule r = 17 mm, l = 62 mm; `COL_index_R` swept capsule r = 5.5 mm, l = 15 mm; `COL_lap` plane at `T` with a 200 × 160 mm patch.
- **Pair matrix**: Index↔Sphere = plant/harvest tap; Thumb+Index↔Stem = pinch; Palm↔Sphere = support and axis rock; Sphere↔Lap = static support with rolling friction; Hand↔Hand = ignored; Tree/Weed↔anything = no collider.
- **Apple body**: kinematic while seated; if lifted, dynamic with gravity 9810 mm/s², restitution 0.30 (apple flesh), friction 0.55, and a 0.08/s linear damping. A dropped apple rolls on the lap but cannot leave the 200 × 160 mm patch without triggering `RESEAT`.
- **Stem**: rigidly attached; the pinch capsule is 9 mm long. The pinch force is not simulated; the twist is read from the finger roll, and the apple's response is a kinematic `ω_spin`.
- **Inertia**: `I = 1.866 × 10⁻⁴ kg·m²`; used for the 180 ms coast-down after release. A 240 °/s spin coasts to zero in 1.33 s at 180 °/s².
- **Forces**: the only real force is the pinch; the virtual sun, rain, and plants apply none. A harvest tap applies a 0.02 N·s visual impulse to the tree fragment.
- **CCD**: the planting index moves at ≤ 1.0 m/s = 11.1 mm/frame; swept capsule vs sphere is exact. A tap is a 140 ms window, so no tunneling is possible.
- **Determinism**: growth uses the exact analytic `f_sun`; weed seeding uses `hash(seasonSeed, tick)` for cell choice; a replay with the same spin/tilt/tap log is bit-identical.
- **Lap stability**: the lap plane is treated as a 200 × 160 mm rectangle with a 4 mm cloth depression; the apple is stable if its centre stays within 70 mm of `V_O`. Beyond that, it rolls and `RESEAT` triggers.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → lap/knee mesh → sun disc and cloud bank → apple surface → cell tint → trees/weeds → terminator seam → tooltip → hands → HUD.
- **Occlusion case 1 — left hand over the stem**: the pinch fingers cover the north pole and 8–12 % of the upper cells. The stem and the pole cells are rendered with a 2 mm outline; the player does not need to see the pole to spin.
- **Occlusion case 2 — right index over a cell**: the tapping fingertip covers one cell. The tooltip is offset 12 mm along the cell normal and the planting glyph flashes on the exposed rim.
- **Occlusion case 3 — apple on the lap**: the lower 25–35 % of the apple is hidden by the thigh/lap mesh. No gameplay cell is placed below `φ = −60°` for planting prompts; the south pole is visible only when the apple is lifted.
- **Occlusion case 4 — sun/cloud behind the hand**: the sun disc and cloud bank are W-fixed and may pass behind the left hand; they are depth-tested, but their light on the apple is unaffected (the lighting is analytic).
- **Occlusion case 5 — trees on the far side**: back-facing cells are culled; a mature tree on the far side contributes to the count but is not drawn through the apple. A 2 mm silhouette fringe marks the terminator.
- **Occlusion case 6 — tooltip vs HUD**: the tooltip yields to the HUD; if they overlap, the tooltip moves 20 mm toward the apple centre.
- **VFX budgets**: 72 cell quads, up to 72 trees (3 cones each = 216 draw calls merged into one instanced mesh), 300 rain streaks, 40 weed tufts, 24 harvest fragments, 12 terminator sparks; global cap 420 particles. The apple uses a 2K skin texture and a 1K cell mask.
- **Lighting**: one directional sun at 3.2 cd/m², one cool fill at 0.4 cd/m², and an emissive terminator seam. The cells are unlit albedo tinted by `G`, so the gameplay read does not depend on real-time shadows.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Stem pinch | stem | woody click | stem glow, spin ring appears |
| Spin | apple surface | airy globe whir, pitch ∝ |ω| | cell parallax, dust motes |
| Day tick | apple | soft clock tick every 90° | terminator seam pulse |
| Plant | cell | seed press into soil | green pulse, −1 seed |
| Germination | cell | tiny sprout pop | 2 mm sprout |
| Tree mature | cell | warm wooden chime | full cone, +150 |
| Harvest | cell | fruit pluck + chime | +3 seeds, fragments |
| Weed seed | cell | wet, low blip | dark tuft appears |
| Weed spread | cell | creeping rustle | adjacent tuft |
| Weed burned | cell | crisp sizzle | tuft turns to ash |
| Sun-rich warning | HUD | dry crackle | ochre cell tint |
| Rain-rich warning | HUD | drip loop | blue-green cell tint |
| Coverage 25 % | HUD | low blight tone | gauge amber |
| Coverage 35 % | HUD | blight bloom | fail, screen desaturates |
| Season win | HUD | rising four-note theme | tally card |

The spin and day-tick cues are the timing backbone: a 90° tick every 0.75 s at 120 °/s tells the player how many simulated days have passed without looking at the clock.

## 13. Difficulty Progression & Tuning

| Level | Mature target | Weed interval (s) | Weed growth (/s) | Growth k (/s) | Season (s) | Starting seeds | Centrifugal risk |
|---|---|---|---|---|---|---|---|
| 1 | 26 | 12.0 | 0.045 | 0.12 | 90 | 24 | 0.06 |
| 2 | 30 | 10.5 | 0.052 | 0.13 | 90 | 22 | 0.08 |
| 3 | 36 | 9.0 | 0.060 | 0.14 | 90 | 20 | 0.10 |
| 4 | 40 | 8.0 | 0.068 | 0.15 | 90 | 18 | 0.12 |
| 5 | 42 | 7.0 | 0.076 | 0.16 | 90 | 16 | 0.15 |

- **Adaptive rule** (`TERRA_ADAPT`): after each season, if mature ≥ target + 4 and coverage ≤ 6 %, level +1; if the season failed on coverage, level −1 and weed growth ×0.9. Clamp 1–5; one step per season.
- **Named constants**: `R_APPLE = 36 mm`, `G_CELLS = 72`, `K_GROW = 0.14/s`, `S_SPIN = 120 °/s`, `G_MIN = 0.55`, `WEED_SUN = 0.30`, `WEED_RATE = 0.060/s`, `WEED_DIE = 0.120/s`, `SPREAD = 0.020/s`, `SEED_START = 20`, `SEED_MAX = 30`, `SEED_TICK = 4.0 s`, `SEASON = 90.0 s`, `TILT_MAX = 25°`.
- **Tuning intent**: at level 3, a player who keeps `δ = 18–22°` and spins at 140 °/s can mature 36 trees in 70–80 s, leaving 10–20 s to tilt and burn the rain-pole weeds. A player who spins at 300 °/s matures faster but can lose 25–40 % of saplings to centrifugal risk.

## 14. Edge Cases & Failure Recovery

- **Stem not found**: fall back to the calyx axis; show a 1.0 s "stem hidden" hint; the spin gesture is disabled until the stem is visible, but planting still works.
- **Pinch slips**: if the aperture exceeds 14 mm, spin authority drops to 0 and `ω` coasts at 180 °/s²; no seed or score is lost. Re-pinching within 1.0 s resumes without a phase change.
- **Apple lifted**: the season freezes within one tick; the apple becomes dynamic; re-seating within 5.0 s resumes. If the apple is set down at a different yaw, the meridian is re-anchored to the stem scar and the cell biomass map rotates with `P`, so no cells are lost.
- **Apple rolls off the lap**: if the centre leaves the 200 × 160 mm patch, `RESEAT` triggers immediately; the apple is rendered as a ghost until retrieved. The season clock does not advance.
- **Tilt beyond 25°**: the apple is treated as lifted; the sun model holds the last valid `δ`, and the spin pauses. This prevents the player from "cheating" a 90° tilt that would make one pole permanently lit.
- **Spin at 360 °/s for > 3 s**: saplings have a 0.20/s uproot risk; the HUD shows a red centrifugal ring. Mature trees are immune, so late-season fast spinning is safe.
- **Weed at an unreachable cell**: weeds are simulated on all 72 cells, including the hidden south pole; coverage counts them. The player must tilt or lift the apple to see and burn them, which is the intended tension.
- **Tap while pinching**: the right index tap is independent of the left pinch; a tap during a spin is allowed and does not disturb `ω`.
- **Harvest spam**: a 2.0 s cooldown per cell prevents double-harvest; a second tap inside the cooldown shows a grey ring and no seed change.
- **Season ends mid-spin**: the final tick samples `ω`, growth, and coverage at t = 90.0 s exactly; the result is deterministic and unaffected by render frame rate.
- **Prop tracking loss > 150 ms**: freeze the season; on relock, blend 120 ms and keep `δ`, `ω`, and the cell map.
- **Coverage exactly at threshold**: 12.0 % counts as a win (≤); 35.0 % counts as a loss (≥); the comparison is on the exact integer cell count, not a rounded display.
- **Recovery UX**: one icon (apple / stem / lap) plus one line; no modal during a season.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropApple` — pose `P`, stem tip, calyx axis, lap support, dynamic/kinematic mode.
  - `StemTwist` — pinch gate, aperture, `ω_stem`, coast-down, fusion with shell spin.
  - `AxisModel` — `â`, `δ`, tilt clamp, sun/rain directions.
  - `ExposureField` — 72 cells, `f_sun`, `f_rain`, `G`, terminator.
  - `CellSim` — biomass, weed, state, germination, harvest cooldown.
  - `SeedBank` — count, passive tick, plant/harvest transactions.
  - `SeasonDirector` — 90 s clock, weed seeding, win/lose, score, adaptive level.
  - `TerraformRig` — cell quads, trees, weeds, terminator, sun disc, clouds.
  - `SeasonHud` — tree/coverage gauge, clock, tooltip, threshold markers.
- **Event flow**: `HeadsetTracker → PropApple → StemTwist / AxisModel → ExposureField → CellSim`; `SeasonDirector → CellSim`; `SeasonDirector` emits `EventBus {Pinch, Spin, Plant, Germinate, Mature, Harvest, WeedSeed, WeedBurn, CoverageWarn, Win, Fail}` to `TerraformRig / AudioBank / SeasonHud / ScoreKeeper`.
- **Pseudocode — exposure and growth (per tick)**:

```text
axis = normalize(R_P * (0, 0, 1))
delta = asin(clamp(dot(axis, SUN_DIR), -1, 1))
s = clamp(abs(omegaSpin) / (120 deg/s), 0, 1.5)
for cell in cells where cell.state != EMPTY:
    t = tan(cell.phi) * tan(delta)
    fSun = acos(clamp(-t, -1, 1)) / PI
    fRain = 1 - fSun
    G = 4 * fSun * fRain
    if cell.biomass < 0.90 and G >= 0.55:
        cell.biomass += 0.14 * (G - 0.50) * s * (1 - cell.weed) * dt
    elif cell.biomass < 0.90:
        cell.biomass = max(0, cell.biomass - 0.01 * s * dt)
    if cell.biomass >= 0.90: cell.state = MATURE
```

- **Pseudocode — weeds and season end (per tick)**:

```text
for cell in cells:
    fSun = sunFraction(cell.phi, delta)
    if fSun < 0.30 and not (cell.state == MATURE):
        cell.weed += 0.060 * (0.30 - fSun) / 0.30 * dt
    else:
        cell.weed -= 0.120 * max(0, fSun - 0.30) / 0.70 * dt
    cell.weed = clamp(cell.weed, 0, 1)
    if cell.weed > 0.60: spreadToNeighbors(cell, 0.020 * dt)
coverage = count(cells where weed > 0.50) / 72
if coverage >= 0.35: fail(BLIGHT)
if now >= 90.0:
    win = matureCount >= target and coverage <= 0.12
    debrief(win, matureCount, coverage)
```

- **Tick order**: pose → stem twist → axis → exposure → growth/weeds → seed passive → coverage check → season clock → HUD. Planting taps are resolved before growth so a seed planted on the final tick still counts as a seed, not a tree.
- **Telemetry**: per season log `meanDelta`, `meanOmega`, `daysElapsed`, `planted`, `matured`, `harvested`, `weedCells`, `coverageCurve`, and `uprooted`. The coverage curve is the primary tuning input.
- **Replay**: `hash(seasonSeed)` drives weed cell choice and starting cell tint; the full input log (tilt, spin, taps) reproduces the season exactly.

## 16. Acceptance Test Checklist

1. Registration fits the apple centre to < 0.8 mm, the stem axis to < 2°, and the meridian to < 6° in 20/20 presentations.
2. At `δ = 0°`, every cell reports `f_sun = 0.500 ± 0.001` and `G = 1.000 ± 0.002`.
3. At `δ = 23.5°`, `f_sun(+30°) = 0.581 ± 0.002`, `f_sun(−30°) = 0.419 ± 0.002`, and `f_sun + f_rain = 1.000 ± 1e-6` for all 72 cells.
4. At `δ = 23.5°`, `G(±60°) = 0.704 ± 0.005` and `G(±75°) = 0.000 ± 0.001`.
5. A cell at `G = 1`, `ω = 120 °/s`, no weeds, grows from `b = 0` to `b = 0.90` in 12.9 ± 0.4 s; at `ω = 60 °/s` it takes 25.7 ± 0.8 s.
6. A cell at `G = 0.50` does not grow; a mature tree at `G < 0.55` decays at 0.01/s.
7. A weed at `f_sun = 0` reaches `w = 1.0` in 16.7 ± 0.5 s; at `f_sun = 0.20` it grows at 0.020/s; at `f_sun = 0.30` it stops.
8. A mature tree is harvested by a 100 ms tap: +3 seeds, +120 score, `b = 0.25`, and a 2.0 s cooldown; a second tap inside the cooldown changes nothing.
9. Planting on an empty cell costs exactly 1 seed and starts a 4.0 s germination; planting with 0 seeds is rejected and shows the seed gauge.
10. Tilting the axis from `δ = +23.5°` to `−23.5°` flips the weed pole within one rotation of 120 °/s (≤ 3.0 s) and moves the terminator by 47°.
11. At `|ω| = 300 °/s` for 5 s, 35–45 % of saplings are uprooted; mature trees are never uprooted.
12. Coverage at 34.9 % does not end the season; coverage at 35.0 % ends it on the next tick.
13. At level 3, a scripted season with `δ = 20°`, `ω = 140 °/s`, and 36 plantings matures ≥ 36 trees with coverage ≤ 12 % in 5/5 replays.
14. 72 trees + 300 rain streaks + 24 harvest fragments + 40 weed tufts stays ≤ 420 particles and ≤ 11.1 ms frame time at 90 Hz.
