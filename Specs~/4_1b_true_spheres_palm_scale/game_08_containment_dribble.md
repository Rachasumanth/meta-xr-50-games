# Game 8 — Containment Dribble — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Containment Dribble |
| Slug | `containment-dribble` |
| Physical prop | Foam stress ball, true sphere, 64.0 mm diameter (r = 32.0 mm); 60 g |
| Family / sub-shape | Family 4 Spherical, Ellipsoidal & Domed / 4.1b True Spheres, Palm-Scale |
| Scale class | Palm-scale one-hand hold; prop 1:1; vertical shaft rendered above the ball |
| Source concept | "Each tap compresses an anti-gravity spring, bouncing a baby dragon up a vertical shaft to gobble fireflies and avoid ceiling spikes." |
| Physical input | Foam ball in the upturned left palm; right index and middle tap the top pole rhythmically |
| Player posture | Seated, left palm up at 230–290 mm; right hand above the ball |
| Session length | 60.0 s run + 3 s brief + 6 s debrief |
| Win condition | Survive 60 s with ≥ 20 fireflies collected |
| Lose condition | Three shield hearts lost, or fewer than 20 fireflies at 60 s |
| Primary metric | Fireflies collected per shield lost |
| Tracking budget | 2 right fingertips, 1 left palm cradle, 1 prop pose |

## 1. Fantasy & Core Loop

The stress ball is a containment spring. Inside it, a baby dragon is compressed against an anti-gravity bladder; every tap on the top pole stores energy in the bladder and launches the dragon up a 480 mm shaft. The shaft is hung with fireflies and roofed with descending spike plates. The dragon is a joyful, clumsy projectile: it cannot steer in the air, so all steering happens before the launch, by tapping a different sector around the pole.

The core loop is dribbling. The dragon falls back toward the ball, a shrinking ring on the top pole predicts the landing, and the player taps at the moment the ring collapses. A well-timed, fast tap sends the dragon high; a mistimed tap wastes the spring and the dragon rebounds with only 55 % of its speed. The rhythm is self-generated: a higher jump gives a longer fall, so the player sets the tempo by the height they choose.

1. **Read the ring (0.4–0.8 s).** A white ring contracts onto the top pole as the dragon descends.
2. **Choose a lane (0.2 s).** The tap's longitude around the pole selects left, centre, or right; the dragon drifts toward that lane after launch.
3. **Tap (0.08 s).** Index and/or middle strike the top cap; impact speed sets the spring compression.
4. **Launch (0.1 s).** The dragon leaves the pad at `v = x_c·173.2`; height follows ballistics.
5. **Harvest / dodge (0.6–1.4 s).** The dragon passes through firefly rings and must stay below spiked lanes.
6. **Recover (0.3 s).** The dragon falls; the next ring appears; the cycle repeats.

A 60 s run contains 20–35 taps. The scoring sweet spot is a 260–340 mm jump: high enough to reach the dense firefly band, low enough to stay under the spike plates that descend after t = 10 s. Over-tapping is the main failure mode, and the ceiling is there to punish it.

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

- **P-frame**: origin at the stress-ball centre; `+ẑ_P` is the **tap pole**, defined at registration as `+ŷ_W`. Poles `±ẑ_P`; equator in `x̂_P`–`ŷ_P`; `+x̂_P` is the player's right and `+ŷ_P` the player's forward at registration. The top cap is `φ > 45°`; tap effectors must contact inside it.
- **Radius and mass**: `r = 32.0 mm`, `m = 60 g`, `I = 2mr²/5 = 2.458 × 10⁻⁵ kg·m²`. Collider r = 32.0 mm; registration tolerance ±0.6 mm.
- **Rolling kinematics**: the ball is held, not rolled, but palm and finger shear still spin it. Any driving contact contributes `ω = (n × v_t)/r`; `|ω|` is clamped to 120 °/s and the resulting orientation is blended 70 % back toward the upright pole over 0.5 s when no effector is in contact. This keeps the tap pole under the fingers and the lane frame stable.
- **V-frame anchoring**: `V` follows the **translation** of `P` but uses **W orientation**. Origin `V_O = C + r·ẑ_P + (0, 8 mm, 0)`, axes `x̂_V = x̂_W`, `ŷ_V = ŷ_W`, `ẑ_V = ẑ_W`. The shaft is therefore always vertical even if the player tilts the ball. The ball's top pole is the launch pad.
- **Deviation 1 — vertical world frame**: a purely P-anchored shaft would tilt with the wrist and make the ceiling unreadable; V is translation-anchored to P and rotation-anchored to W.
- **Deviation 2 — virtual foam compression**: the ball does not visibly deform in the real world; a 0–18 mm virtual compression dish and a glow ring show stored spring energy on the top cap.
- **Deviation 3 — tap is an event, not a force**: the tap classifier produces a scalar impulse from the approach speed; the player never has to hold the ball down.
- **Deviation 4 — lane frame is yaw-locked**: lane sectors use the registration yaw of P and do not follow small ball spins, so a tap's lane is predictable across a 60 s run.

## 3. Physical Play Space & Ergonomics

The player sits with the left forearm supported, palm up, fingers relaxed, and the stress ball seated in the palm hollow. The right hand hovers 60–90 mm above, with the index and middle extended and the wrist in 20–30° flexion. The shaft is rendered directly above the ball, occupying 0–55° above the eye line; the player looks up 20–25° rather than craning the neck. The ball is 240–300 mm forward and 20–60 mm above the table.

| Parameter | Value | Rationale |
|---|---|---|
| Ball centre | 270 mm forward, 40 mm above table | Same workspace as the other palm-scale games |
| Left palm cup | 100–130° of lower hemisphere | Stable seat without gripping |
| Right tap height | 55–90 mm above ball top | Enough travel to reach 0.3–3.0 m/s |
| Shaft axis | vertical through `C` | Predictable ballistics |
| Shaft height | 480 mm above pad | Keeps the apex inside a 25° up-gaze |
| Shaft radius | 70 mm | Three 45 mm lanes plus 25 mm clearance |
| Tap cadence | 0.6–1.4 s per bounce | Comfortable wrist rhythm |
| Session break | 6 s after the run | Wrist extensor recovery |

Ergonomic guard: if the right wrist exceeds 45° flexion or the tap cadence exceeds 2.5 taps/s for 3.0 s, the spring gain drops to 70 % and a "slow the dribble" hint appears. The game never requires more than 2.0 taps/s for a successful run.

## 4. Prop Geometry & Registration

- **Real dimensions**: 64.0 mm diameter, 60 g, polyurethane foam, density 0.44 g/cm³, surface pore texture 0.8 mm. The pore pattern is the odometry feature set.
- **Sphere model**: `|p_P| = 32.0 mm`. The top cap `φ > 45°` is the tap zone; it carries a 0.4 mm virtual compression dish that scales with the last tap. The equator carries three lane glyphs at `λ = 0°, 120°, 240°` for the HUD.
- **Surface features**: a 1.2 mm ring at `φ = 60°` marks the tap-zone boundary; three 2 mm lane chevrons at the equator; a 0.6 mm pole dimple that is always visible under the finger. All are virtual and P-fixed.
- **Colliders**: `COL_sphere` r = 32.0 mm; `COL_index` and `COL_middle` swept capsules r = 5.5 mm, l = 15 mm; `COL_palm_L` capsule r = 17 mm, l = 65 mm (support only). No collider for the shaft or dragon against the ball; the dragon is virtual and above the pad.
- **Registration**: the player presents the ball in the left palm at 260 mm. The system fits the centre over 8 frames (< 0.6 mm), sets `+ẑ_P` to world up within 2°, and records the yaw of `+x̂_P` as the lane frame. If the ball is squashed or the fit residual exceeds 1.2 mm, a "round it in your palm" prompt appears.
- **Tolerance**: centre < 0.6 mm; radius < 0.6 mm; pole < 2°; yaw frame < 5°. A yaw error over 5° is corrected with a 3 s "show the chevrons" calibration.
- **Tracking**: visual odometry at 72 Hz; foam pores give 40–70 features at 300 mm. Re-acquisition searches 55 mm for 1.2 s and blends 100 ms.
- **Deformation**: the real ball compresses under the finger by 0.5–1.5 mm. This is ignored by the pose filter; the virtual dish shows 0–18 mm of spring compression, which is a game abstraction, not a measured deformation.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Palm seat | `L_palm` capsule | lower hemisphere | support valid; enables spring |
| I5.2 | Index tap | `R_index_tip` | top cap `φ > 45°` | tap impulse, lane from `λ` |
| I5.3 | Middle tap | `R_middle_tip` | top cap `φ > 45°` | tap impulse, lane from `λ` |
| I5.4 | Double tap | index + middle within 45 ms | top cap | `v_eff ×1.12`, one launch |
| I5.5 | Lane left | tap at `λ ∈ [60°, 180°)` | top cap | dragon target lane L |
| I5.6 | Lane centre | tap at `λ ∈ [−60°, 60°)` | top cap | dragon target lane C |
| I5.7 | Lane right | tap at `λ ∈ [180°, 300°)` | top cap | dragon target lane R |
| I5.8 | Rest | no tap > 1.2 s | — | idle hint; dragon still bounces |
| I5.9 | Re-seat | left palm lost > 300 ms | — | pause spring, hold run timer |
| I5.10 | Re-grip | palm seat restored | — | resume, no shield change |

Per-finger roles: index and middle are identical tap sources; the index is primary, the middle is the alternate for fatigue. Both fingers tapping within 45 ms is a deliberate "double tap" and is the strongest input in the game. The ring finger and pinky are ignored. The left thumb only stabilizes the ball; it cannot tap because it is under the palm plane.

## 6. Input Kinematics & Signal Processing

- **Contact gate**: `s = |x_j − C| − r`; contact if `s < 5 mm`, release if `s > 8 mm`, debounce 30 ms. Tap effectors must also satisfy `φ > 45°`.
- **Penetration proxy**: `D = clamp(5.0 − s, 0, 12.0) mm`. A tap must reach `D ≥ 3.0 mm`; the peak `D` is recorded for the compression visual.
- **Approach speed**: at the frame `D` crosses 2.0 mm, `v_n = max(0, −v_j · n̂)` where `n̂` is the surface normal at the contact. Clamp `v_n ∈ [0.15, 3.0] m/s`.
- **Tap extraction**: a tap starts when `D ≥ 2.0 mm` for 30 ms and ends when `D < 1.0 mm` or 160 ms elapse. Only taps whose peak `D ≥ 3.0 mm` launch the dragon. A contact longer than 160 ms is a "press", not a tap, and produces no launch.
- **Double tap**: if the second finger's tap onset is within 45 ms of the first, they fuse: `v_eff = 1.12·max(v_n1, v_n2)`. Only one launch is produced; the second tap does not stack.
- **Spring compression**: `x_c = v_eff·sqrt(m_h/k_b)`, with `m_h = 0.09 kg`, `k_b = 2400 N/m`, so `x_c = 6.124 mm` per m/s of tap speed. Clamp `x_c ≤ 18.0 mm`. The real foam's stiffness is not measured; these are design constants chosen to give a 0–18 mm visual dish.
- **Dragon launch speed**: `v_launch = m_t·x_c·sqrt(k_b/m_d)`, with `m_d = 0.08 kg`, `sqrt(k_b/m_d) = 173.2 rad/s`. Thus `v_launch = 1.061·v_eff·m_t` (before clamp), and the dragon's apex is `h = v_launch²/(2g)` above the launch point.
- **Rhythm multiplier**: let `t_land` be the predicted time at which the dragon reaches the pad (`y = 0`) under gravity, and `e = t_tap − t_land`. If `|e| ≤ 0.18 s`, `m_t = 1.25 − 0.90·(|e|/0.18)²`; at `e = 0` the multiplier is 1.25, at the window edge 0.35. If `|e| > 0.18 s` but the dragon is within 70 mm of the pad and descending, `m_t = 0.30` and an "offbeat" cue plays. If the dragon is outside 70 mm, the tap is an air tap: no launch, combo reset.
- **Landing prediction**: with `y` measured from the pad and `v_y < 0`, `t_land = (−v_y + sqrt(v_y² + 2g·y))/g`; at `y ≤ 0.5 mm` the dragon is considered landed.
- **Lane extraction**: `λ = atan2(p_y, p_x)` of the contact point in the lane frame; map to three 120° sectors: C `[−60°, 60°)`, L `[60°, 180°)`, R `[180°, 300°)`. The lane is sampled at the tap's peak `D` and held for that launch.
- **No-tap bounce**: if the dragon reaches `y = 0` without a launch, `v_y ← −0.55·v_y`; the apex retains 30 % of its height. Three consecutive no-tap bounces trigger a "keep tapping" hint.
- **Filtering**: `v_n` is the raw two-frame estimate; no smoothing is applied because the tap lasts 80–140 ms. A 3-tap median rejects single-frame tracker spikes. `x_c` is quantized to 0.25 mm for deterministic replays.
- **Cadence guard**: taps closer than 180 ms apart are merged; the second is discarded. This prevents double-counting a single physical strike.

## 7. Virtual World Layout & Coordinate Mapping

- **V frame**: origin `V_O = C + r·ẑ_P + (0, 8 mm, 0)`; `x̂_V = x̂_W` (player right), `ŷ_V = ŷ_W` (up), `ẑ_V = ẑ_W` (toward player). The shaft rises along `+ŷ_V`.
- **Pad**: a 34 mm radius disc at `y = 0`, with a 0–18 mm compression dish and three lane arcs at `x = −45, 0, +45 mm`.
- **Shaft**: 480 mm tall, 70 mm radius, rendered as a translucent cylinder with 12 vertical ribs. Walls are visual only; the dragon cannot leave the shaft because it has no horizontal velocity except lane drift.
- **Dragon**: a 20 mm radius body, 34 mm long, with a 6 mm tail and two 4 mm wings. Body centre `(x_d, y_d, 0)`; `x_d` is the lane coordinate, `y_d` the height above the pad.
- **Lanes**: `x = −45, 0, +45 mm`. Lane drift is 120 mm/s toward the target lane, applied only while `y_d > 20 mm`; below that the dragon snaps to the pad lane.
- **Fireflies**: 12 mm emissive spheres with a 22 mm collect radius, spawned at heights 80–430 mm in one of the three lanes. At most 7 are alive; spawn every 1.1–1.8 s at least 70 mm above the last collected height.
- **Ceiling spikes**: three plates, one per lane, each with five 14 mm spikes pointing down. The spike tip starts at 520 mm and descends 9 mm every 2.5 s after t = 10 s, reaching 340 mm at t = 60 s.
- **HUD**: a 56 mm × 20 mm panel at `V_O + (0, 500 mm, 0)` plus an eye-relative score chip; shows fireflies, shields, and the current spike height per lane.

## 8. Gameplay Systems & Rules

- **Entities**: `Dragon` (x, y, v_y, lane, targetLane), `SpringPad` (x_c, lastTap, rhythm), `TapClassifier` (two effectors), `FireflyPool` (7 agents), `SpikeBank` (3 plates), `RunDirector` (60 s, shields, score).
- **Launch**: on a valid tap, `v_launch = min(3.30 m/s, 1.061·v_eff·m_t)`; the dragon's `v_y` is set to `v_launch` at its current `y`, and `y` continues from there. The spring dish animates from `x_c` to 0 over 180 ms.
- **Ballistics**: `v_y ← v_y − 9810·dt` (mm/s²), `y ← y + v_y·dt`. Apex `h = y + v_y²/(2g)`. Horizontal drift `x ← x + clamp(target − x, ±120·dt)`.
- **Firefly collect**: if `|x_d − x_f| < 26 mm` and `|y_d − y_f| < 26 mm`, collect: +50, +1 firefly count, 18-particle burst. A firefly is collectible during both ascent and descent.
- **Spike hit**: if the dragon's top (`y_d + 20 mm`) exceeds the spike tip in its current lane, shield −1, `v_y ← −0.40·|v_y|`, spikes in that lane retract 40 mm, 1.2 s invulnerability. The dragon is pushed down, not killed.
- **Shields**: 3 hearts. At 0 the run ends immediately; score is tallied.
- **Run timer**: 60.0 s, frozen during pause and re-seat. The timer does not stop for a high bounce.
- **Win**: at 60.0 s, `fireflies ≥ 20`. **Lose**: shields 0, or `fireflies < 20` at 60.0 s.
- **Scoring**: firefly +50; high catch above 300 mm +25; three-lane sweep (collect in L, C, R within 6 s) +150; perfect tap (`|e| ≤ 0.04 s`) +20; no-spike run +300; shield remaining +150 each.
- **Combo**: consecutive fireflies without a missed tap build ×1.1, ×1.2, ×1.4, capped ×1.8. A missed tap (air tap) or spike hit resets the combo.
- **Spike schedule**: at t = 10 s the first plate drops to 500 mm; every 2.5 s all plates drop 9 mm. At t = 60 s the tips are at 340 mm. A 260–300 mm apex is safe for the entire run; a 420 mm apex becomes unsafe after t ≈ 32 s.
- **Firefly schedule**: one spawns at t = 0.8 s, then every 1.1–1.8 s, with a guaranteed spawn in the lane the dragon is not currently in 40 % of the time, to encourage lane changes.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load run config | REGISTER |
| REGISTER | ball presented | centre < 0.6 mm | fit sphere, set pole and yaw | CALIBRATE |
| CALIBRATE | palm seat 400 ms | pole < 2° | anchor `V`, zero run clock | BRIEF |
| BRIEF | seat held | — | controls card 3.0 s | RUN |
| RUN | tick | seat valid | gravity, lane drift, spawns | RUN |
| RUN | valid tap | `x_c ≥ 3 mm`, `m_t > 0` | launch, spring VFX | RUN |
| RUN | no tap at pad | `y ≤ 0` | bounce ×0.55, combo reset | RUN |
| RUN | firefly overlap | `|Δx|<26`, `|Δy|<26` | collect, +50, spawn next | RUN |
| RUN | spike overlap | top > tip, same lane | shield −1, push down, retract | RUN |
| RUN | shields = 0 | — | fail, tally | DEBRIEF |
| RUN | t = 60 s | fireflies ≥ 20 | win, tally | DEBRIEF |
| RUN | t = 60 s | fireflies < 20 | fail, tally | DEBRIEF |
| RUN | left palm lost 300 ms | — | freeze timer and dragon | RESEAT |
| RESEAT | palm restored | ball within 55 mm | blend 100 ms | RUN |
| RESEAT | 4.0 s timeout | — | "seat the ball" card | RESEAT |
| any | prop tracking loss > 150 ms | — | ghost ball, freeze | REACQUIRE |
| REACQUIRE | relock ≤ 1.2 s | search 55 mm | blend 100 ms | prior |
| REACQUIRE | 1.2 s timeout | — | "present the ball" card | REACQUIRE |
| any | system pause | — | freeze all timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: tap max 160 ms; tap merge 180 ms; rhythm window 180 ms; invulnerability 1.2 s; re-seat 4.0 s; run 60.0 s; spike interval 2.5 s; brief 3.0 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_sphere` r = 32 mm; 2 swept tap capsules r = 5.5 mm, l = 15 mm; 1 left palm capsule r = 17 mm, l = 65 mm; dragon sphere r = 20 mm; 7 firefly spheres r = 22 mm (trigger); 3 spike AABBs (lane × height); shaft walls analytic.
- **Pair matrix**: Tap capsule↔Sphere = tap classification; Palm↔Sphere = support; Dragon↔Firefly = trigger; Dragon↔Spike = damage; Dragon↔Shaft wall = none (no horizontal velocity beyond drift); Dragon↔Ball = launch pad only, no penetration; Hand↔Dragon = visual occlusion only.
- **Spring model**: `x_c = v_eff·sqrt(m_h/k_b)` is a linear energy model, not a simulated spring. The visual dish obeys `x(t) = x_c·exp(−t/0.060)·cos(2π·8·t)` for 180 ms and then clamps to 0.
- **Dragon dynamics**: 1D vertical with gravity 9810 mm/s²; no air drag (the shaft is a vacuum for predictability). Lane drift is kinematic at 120 mm/s with a 0.15 s ease-in/out.
- **CCD**: the dragon moves at most 36.7 mm per 90 Hz frame at 3.3 m/s, less than the 26 mm firefly radius? No: 36.7 > 26, so firefly collection uses a swept sphere test between the previous and current positions. Spike collision uses the maximum of the previous and current top height to avoid tunnelling.
- **Impulse on spike hit**: `v_y ← −0.40·|v_y|` is an instantaneous velocity change, not a force; the visual squash lasts 120 ms.
- **Timestep**: fixed 90 Hz, max 2 sub-steps. The 60 s run is exactly 5,400 ticks; tests replay the same seed and tap log to get bit-identical scores.
- **Forces**: the only real-world force is the finger tap, which is not fed back. The virtual spring applies no force to the prop. The ball's mass and inertia are used only for the drop model if the left palm is lost: gravity 9810 mm/s², restitution 0.45, foam friction 0.75.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → table/scene mesh → shaft back wall → fireflies → dragon → spike plates → shaft front ribs → compression dish → hands → HUD.
- **Occlusion case 1 — hand over the pad**: the right hand covers the top cap and the compression dish during a tap. The dish is rendered as a 3 mm ring at `φ = 62°`, outside the fingertip footprint, and the spring glow is drawn on the visible rim.
- **Occlusion case 2 — left palm under the ball**: the palm hides the lower hemisphere; the shaft starts at the top pole, so no gameplay element is hidden.
- **Occlusion case 3 — dragon behind the shaft front ribs**: the 12 ribs are 15 % opaque and depth-tested; the dragon is never fully hidden. If the dragon's silhouette is occluded for > 0.3 s, a 2 mm outline is drawn.
- **Occlusion case 4 — ceiling spikes above the HUD**: the spike plates are at 340–520 mm; the HUD is at 500 mm and may overlap. The HUD slides 10 mm toward the player when a spike is within 40 mm of it.
- **Occlusion case 5 — ball vs table**: if the ball is within 40 mm of the table, the table mesh clips the lower shaft; the pad remains 8 mm above the table by construction.
- **Occlusion case 6 — firefly behind a finger**: a firefly passing behind the tapping hand is occluded; a 4 mm halo is drawn around the finger so the player still sees the target.
- **VFX budgets**: 7 fireflies × 6 glow sprites = 42; dragon trail 30; collect burst 18 × 3 concurrent = 54; spike hit 24; spring dish 12; ambient motes 40; global cap 240. The shaft uses one 1K texture and a 12-rib mesh (144 triangles); the dragon is 1,800 triangles.
- **Readability**: fireflies are 12 mm and emissive at 3.0 cd/m²; the dragon is warm orange with a 4 mm rim light; spikes are dark metal with red tips only when the dragon is within 60 mm.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Seat valid | left palm | soft foam settle | pad ring lights |
| Tap | contact point | low "pock", pitch ∝ v_n | dish compresses, dust ring |
| Double tap | contact point | double pock, +1 semitone | brighter dish flash |
| Perfect rhythm | pad | glassy chime | gold ring at pad |
| Launch | pad | rising whoosh | dragon trail, spring recoil |
| Apex | dragon | airy float tone | wings spread |
| Firefly collect | firefly | bright bell | 18-particle burst, +50 |
| Lane shift | dragon | soft wing flap | banked dragon sprite |
| Spike warning | spike lane | low creak | red tips, shadow on pad |
| Spike hit | dragon | metal clang + roar | shield −1, squash |
| No-tap bounce | pad | dull thud | dim ring, combo reset |
| Air tap | contact point | hollow knock | grey ring, combo reset |
| Shield lost | HUD | descending heart tone | heart breaks |
| Win | HUD | rising fanfare | tally card |
| Fail | HUD | falling two-note | tally card |

The rhythm channel is audio-first: the landing ring has a synchronized tick at 0.18 s before landing and a stronger tick at 0.06 s, giving the player a two-stage audible countdown even if they are not looking at the pad.

## 13. Difficulty Progression & Tuning

| Level | Gravity (mm/s²) | Spike start (mm) | Spike drop (mm/2.5 s) | Firefly target | Spawn gap (s) | Shields |
|---|---|---|---|---|---|---|
| 1 | 7600 | 560 | 7 | 16 | 1.6 | 4 |
| 2 | 8600 | 540 | 8 | 18 | 1.5 | 3 |
| 3 | 9810 | 520 | 9 | 20 | 1.4 | 3 |
| 4 | 10800 | 500 | 10 | 22 | 1.3 | 3 |
| 5 | 11800 | 480 | 11 | 24 | 1.2 | 2 |

- **Adaptive rule** (`DRIBBLE_ADAPT`): after each run, if fireflies ≥ target + 6 and shields lost ≤ 1, level +1; if the run failed, level −1 and the rhythm window widens to 0.22 s. Clamp 1–5; one step per run.
- **Named constants**: `R_FOAM = 32 mm`, `K_BALL = 2400 N/m`, `M_HAND = 0.09 kg`, `M_DRAGON = 0.08 kg`, `OMEGA_D = 173.2 rad/s`, `X_MAX = 18 mm`, `V_TAP_MAX = 3.0 m/s`, `RHYTHM_WIN = 0.18 s`, `RHYTHM_MAX = 1.25`, `G = 9810 mm/s²`, `LANE_X = ±45 mm`, `DRIFT = 120 mm/s`.
- **Tuning intent**: at level 3, a 2.0 m/s tap with a perfect rhythm gives `x_c = 12.25 mm`, `v_launch = 2.65 m/s`, and a 358 mm apex. A 1.6 m/s tap gives 229 mm. The 300 mm firefly band is reachable by both, but the perfect 2.0 m/s tap is unsafe after t ≈ 42 s, which is the intended risk curve.

## 14. Edge Cases & Failure Recovery

- **Tap too slow**: `v_n < 0.15 m/s` or peak `D < 3 mm` produces no launch; the dragon falls and bounces with 0.55 restitution. No shield is lost.
- **Press instead of tap**: a contact longer than 160 ms is ignored as a tap; the pad shows a "tap, don't press" hint after two occurrences.
- **Tap outside the top cap**: `φ ≤ 45°` is ignored; a rim highlight shows the valid zone.
- **Double-tap ambiguity**: two fingers within 45 ms fuse; two taps by the same finger within 180 ms merge and the second is discarded.
- **Dragon outside the 70 mm launch window**: the tap is an air tap; no launch, combo reset, and a grey ring confirms the miss.
- **Dragon at the apex when tapped**: if `y > 70 mm`, no launch; if `y ≤ 70 mm` but ascending, the tap is ignored because the pad cannot catch an ascending dragon.
- **Ball dropped**: pause the run; on retrieval, re-register and restore the timer, shields, fireflies, and spike height. If the pose is lost > 4 s, the run restarts at the current score with no shield change.
- **Left palm lost**: the run freezes within one tick; the ball may roll. Re-seat within 4 s resumes; after 4 s the run restarts from the last 10 s checkpoint.
- **Prop tracking loss**: freeze the dragon at its current state; on relock, continue. A dragon in mid-air does not fall during the freeze.
- **Spike plate already at 340 mm**: it stops descending at 340 mm and pulses; it never reaches the pad (minimum clearance 340 mm).
- **Firefly spawns inside a spike plate**: the spawn is rejected and retried in the same lane 30 mm lower; after 3 rejections it moves to another lane.
- **Frame hitch**: if `dt > 50 ms`, the physics runs 2 sub-steps and the remainder is clamped; the 60 s clock uses wall time, so a hitch cannot extend the run.
- **Recovery UX**: one icon (ball / palm / hand) plus one line; no modal during the run.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropFoam` — pose `P`, pole stabilization, lane yaw frame, support state.
  - `TapClassifier` — two effectors, `s`, `D`, `v_n`, onset/offset, double-tap fusion, cadence merge.
  - `SpringModel` — `x_c`, `v_launch`, rhythm multiplier, dish animation.
  - `DragonBody` — `x, y, v_y`, lane target, drift, bounce, squash.
  - `FireflyPool` — 7 agents, spawn rules, swept collection.
  - `SpikeBank` — 3 plates, descent schedule, retraction, collision.
  - `RunDirector` — 60 s clock, shields, score, combo, adaptive level.
  - `ShaftRig` — shaft mesh, ribs, pad, fireflies, dragon, spikes.
  - `DribbleHud` — fireflies, shields, spike heights, cadence ring.
- **Event flow**: `HeadsetTracker → PropFoam → TapClassifier → SpringModel → DragonBody`; `RunDirector → FireflyPool / SpikeBank → DragonBody`; `RunDirector` emits `EventBus {Tap, Perfect, Launch, Collect, SpikeHit, Bounce, ShieldLost, Win, Fail}` to `ShaftRig / AudioBank / DribbleHud / ScoreKeeper`.
- **Pseudocode — tap classification (per tick, per effector)**:

```text
s = |tip - C| - R_FOAM
contact = contact ? (s < 8.0) : (s < 5.0)
if not contact: reset tap state; return
D = clamp(5.0 - s, 0, 12.0)
if D >= 2.0 and not tap.active:
    tap.active = true; tap.t0 = now; tap.peak = 0; tap.v = approachSpeed(tip, normal)
tap.peak = max(tap.peak, D)
if D < 1.0 or now - tap.t0 > 0.160:
    if tap.peak >= 3.0 and now - tap.t0 <= 0.160:
        lane = sectorFromLongitude(longitude(tip))
        emit(Tap, clamp(tap.v, 0.15, 3.0), lane)
    tap.active = false
```

- **Pseudocode — launch and ballistics (per tick)**:

```text
if launchRequested and dragon.y <= 70 and dragon.vy < 0:
    e = abs(tapTime - predictedLanding(dragon))
    mt = (e <= 0.18) ? 1.25 - 0.90 * (e / 0.18) ** 2 : 0.30
    v = min(3.30, 1.061 * tapSpeed * mt)
    dragon.vy = v
    dragon.targetLane = tapLane
    dragon.y = max(dragon.y, 0)
dragon.vy -= G * dt
dragon.y += dragon.vy * dt
dragon.x += clamp(laneX(target) - dragon.x, -120 * dt, 120 * dt)
if dragon.y <= 0 and dragon.vy < 0:
    if not launched: dragon.vy = -0.55 * dragon.vy; combo = 0
    dragon.y = 0
```

- **Tick order**: prop pose → tap classification → launch → dragon ballistics → firefly collection → spike collision → run clock → HUD. A tap is always resolved before gravity on the same tick, so the launch speed is not reduced by one frame of fall.
- **Telemetry**: per run log `taps`, `validTaps`, `airTaps`, `meanTapSpeed`, `meanRhythmError`, `apexHistogram`, `collects`, `spikeHits`, and `laneChanges`. The apex histogram is the primary tuning input for the spike schedule.

## 16. Acceptance Test Checklist

1. Registration fits the foam ball centre to < 0.6 mm and the tap pole to within 2° of world up in 20/20 presentations.
2. A 2.0 m/s tap with zero rhythm error yields `x_c = 12.25 ± 0.25 mm`, `v_launch = 2.65 ± 0.03 m/s`, and a 358 ± 5 mm apex.
3. A 1.0 m/s perfect tap yields a 89 ± 3 mm apex; a 3.0 m/s tap is clamped at `x_c = 18 mm` and a 3.30 m/s launch.
4. A tap at `|e| = 0.09 s` gives `m_t = 1.025 ± 0.01`; at `|e| = 0.18 s` it gives 0.35 ± 0.01.
5. A tap outside the 0.18 s window but within 70 mm of the pad gives `m_t = 0.30`; a tap above 70 mm produces no launch and resets the combo.
6. A contact held for 200 ms produces no launch; a contact with peak `D = 2.9 mm` produces no launch.
7. Index and middle taps 40 ms apart produce exactly one launch with `v_eff = 1.12×` the faster tap; taps 60 ms apart produce two separate events.
8. A no-tap landing returns `v_y` to exactly 0.55× the impact speed and reduces the apex to 30.25 ± 1 % of the prior apex.
9. Lane sectors map correctly: a tap at `λ = 0°` gives C, at `120°` gives L, at `240°` gives R, with ±1° boundary hysteresis.
10. A firefly at 300 mm is collected when the swept dragon sphere passes within 26 mm; a 30 mm miss is never collected at 3.3 m/s.
11. A spike hit at 3.0 m/s reverses `v_y` to −1.20 m/s, costs exactly 1 shield, and retracts that lane by 40 mm.
12. The run clock reaches exactly 60.0 s after 5,400 ticks; a 120 ms frame hitch does not extend it beyond 60.05 s.
13. At level 3, a scripted 25-tap run with 0.06 s mean rhythm error collects ≥ 20 fireflies and loses ≤ 1 shield in 5/5 replays.
14. Fireflies (42 sprites) + 3 collect bursts + 1 spike hit + 40 motes stays ≤ 240 particles and ≤ 11.1 ms frame time at 90 Hz.
