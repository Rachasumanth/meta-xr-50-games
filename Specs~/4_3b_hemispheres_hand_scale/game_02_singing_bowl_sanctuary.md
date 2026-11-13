# Singing Bowl Sanctuary

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Singing Bowl Sanctuary |
| Slug | `singing_bowl_sanctuary` |
| Physical prop | Ceramic/metal singing bowl, concave up, cradled in the non-dominant palm; inner radius `R = 85 mm`, depth `d = 60 mm`, wall `t = 6 mm`, mass 700 g |
| Family / sub-shape | Spherical-cap hemisphere, hand scale |
| Scale class | Prop 1:1; virtual harmonic dome 180–380 mm across; tabletop play volume < 40 cm |
| Source concept quote | "Hold the bowl steady with one hand; trace the rim continuously with the other index at a steady speed; the golden dome rises only while the trace stays in resonance." |
| Physical input | Non-dominant palm cradles the bowl; dominant index traces the rim at constant tangential speed |
| Session length | 8 min (5 sanctuaries × 80 s + 4 × 10 s rests + onboarding) |
| Lose condition | Lotus loses all 6 petals, or the sanctuary clock expires before the target dome radius |

## 1. Fantasy & Core Loop

A brass singing bowl becomes the foundation of a floating sanctuary. The non-dominant hand cradles it; the dominant index circles the rim. When the trace speed matches the bowl's resonant mode, a golden translucent dome unfolds upward from the rim, one harmonic ring at a time. Inside, a six-petal lotus glows at the bowl's floor. Outside, shadow locusts spiral down the dome, trying to reach the lotus. Each expansion ring of the dome is a pressure front that disintegrates every locust it sweeps through. If the trace slows, races, or wobbles, the dome sags; if it stops, the dome collapses and the locusts get a free dive.

The core loop is a continuous sensorimotor lock: **hear the drone, feel the rim, hold the band**. The player is not tapping a rhythm; they are maintaining a narrow speed band around 260 mm/s for tens of seconds while a swarm converges.

1. **Attune (8 s).** The bowl is registered and cradled. A low drone starts; the player makes three trial laps. The HUD shows the speed band as a ring that turns gold when the trace enters it.
2. **Rise (50 s).** Sustained resonance expands the dome from 90 mm to the sanctuary target. Rings pulse outward every 35 mm and kill locusts.
3. **Swarm (17 s).** Locust spawn rate doubles; the dome must keep expanding or the lotus takes hits.
4. **Resolve (5 s).** The dome settles, remaining locusts scatter, score tallies, petals are counted.
5. **Rest (10 s).** The bowl is set down; the hand opens; the next sanctuary loads.

A sanctuary is won when the dome reaches its target radius with at least one petal intact. It is lost when all six petals are consumed or the 80 s clock expires below target radius. Five sanctuaries form the run, with the target radius climbing from 150 mm to 190 mm and the resonance band narrowing.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Hemisphere sub-shape**: the prop is a hemispherical shell of radius `R` and wall thickness `t`; model in P with the rim circle in the `x̂_P`–`ŷ_P` plane at `z = 0` and the apex at `z = ±R`. For **bowl-up** props the cavity faces `+ẑ_P`; for **dome-up** props the convex dome faces `+ẑ_P`. Inner surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`, θ ∈ [0°, 90°], λ ∈ [0°, 360°). Rim torus: major radius `R`, minor radius `t/2`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **R and t**: `R = 85.0 mm` inner sphere radius, `t = 6.0 mm` wall, outer radius 91.0 mm; rim-to-apex depth `d = 60.0 mm`.
- **P-frame**: origin at the rim-plane centre, `+ẑ_P` out of the cavity. Inner sphere centre `(0, 0, +25.0) mm`; interior apex `(0, 0, −60.0) mm`; outer surface shares the centre at radius 91.0 mm.
- **Rim**: inner rim radius `a = √(R² − (R−d)²) = 81.24 mm`; outer rim radius `√((R+t)² − (R−d)²) = 87.52 mm`; rim torus major 84.38 mm, minor 3.0 mm. Rim truncation `θ_rim = acos((R−d)/R) = 72.94°`; rim trace circumference `2πa = 510.48 mm`.
- **Surfaces**: inner `p_in(θ,λ) = (R sinθ cosλ, R sinθ sinλ, 25 − R cosθ)`, θ ∈ [0°, 72.94°]. Outer `p_out(θ,λ) = (91 sinθ cosλ, 91 sinθ sinλ, 25 − 91 cosθ)`, θ ∈ [0°, 74.06°].
- **V-frame anchoring**: `V` is anchored to `P`, origin at `P_O + (0,0,2) mm`, with 0.5 s orientation low-pass and gravity correction; the dome and lotus follow the bowl, but the camera horizon does not inherit bowl tilt beyond 12°.
- **Deviation 1**: the bowl is a 60 mm-deep cap, not an `R`-deep hemisphere; the rim trace circle is `a = 81.24 mm`, not `R`, and all resonance math uses `a`.
- **Deviation 2**: the prop is cradled, not rigidly fixed; a 700 g bowl is allowed 3 mm of compliant drift before a slip event, and the dome is anchored to the filtered `P`, not the raw pose.
- **Deviation 3**: the trace hand is the only control; the cradle hand contributes a static grip factor that scales dome growth by 0.6–1.0, so a loose cradle never fully stops the fantasy but visibly weakens it.
- **Deviation 4**: dome expansion is quantized into 35 mm pressure rings; the rings, not a continuous force field, are what kill locusts, so timing the expansion matters.

## 3. Physical Play Space & Ergonomics

The player sits with the forearm of the cradle hand supported by the chair arm or table edge. The bowl rests in the palm at lap height, rim 90–130 mm above the lap, 200–260 mm in front of the sternum. The tracing hand is held with the index extended and the wrist in 10–20° flexion; the elbow is supported so the trace is driven by forearm rotation, not shoulder abduction. The head pitches 18–25° down. The bowl's 700 g mass is carried by the palm and forearm, never by finger flexion.

| Parameter | Value | Rationale |
|---|---|---|
| Support | Lap or table `T`; cradle forearm supported | 700 g held for 80 s needs skeletal load transfer |
| Play patch | 190 mm × 190 mm around the bowl | Rim diameter 162 mm plus hand clearance |
| Trace radius | 81.24 mm ± 7 mm | Torus band wide enough for normal radial drift |
| Trace height | rim plane ± 6 mm | Prevents side-wall scraping |
| Trace speed | 260 mm/s target; 213–307 mm/s band | One lap per 1.96 s |
| Index duty | ≤ 25 s continuous, 3 s micro-rest | Pad pressure 0.3–0.8 N |
| Cradle load | ≤ 7.5 N vertical, ≤ 4.1 N tangential | Below `µmg = 0.6 × 6.87 N` slip threshold |
| Session | 8 min, 5 × 80 s sanctuaries | Five bouts with 10 s rests |
| Camera | 18–25° down, 400–550 mm viewing distance | Dome subtends 18–25° |

Fatigue mitigations: the cradle elbow is supported, the bowl may touch the lap for partial load, the trace is a whole-forearm circumduction rather than isolated index motion, and the 10 s rest is a hard gate. The HUD shows a "cradle load" arc that turns amber at 6.5 N and prompts the player to set the bowl down between sanctuaries. The trace hand is allowed to drag the middle fingertip on the rim for stability; this does not affect resonance.

## 4. Prop Geometry & Registration

Nominal constants: `R = 85.0 mm`, `t = 6.0 mm`, `d = 60.0 mm`, mass 700 g, metal/ceramic composite. Inner cap area `A_in = 2πRd = 32,044 mm²`; outer area `A_out = 2π(R+t)d = 34,306 mm²`; rim circumference 510.5 mm. The 700 g mass gives a static weight of 6.87 N; with skin–ceramic friction `µ = 0.60`, the tangential slip threshold at the cradle is 4.12 N.

| Property | Nominal | Tolerance | Source |
|---|---|---|---|
| Inner radius `R` | 85.0 mm | ±0.4 mm | Rim + interior fit |
| Wall `t` | 6.0 mm | ±0.4 mm | Rim thickness |
| Depth `d` | 60.0 mm | ±0.6 mm | Scene-mesh probe |
| Inner rim radius | 81.24 mm | ±0.4 mm | Derived |
| Rim torus | 84.38 / 3.0 mm | ±0.4 / ±0.2 mm | Circle fit |
| Mass | 700 g | ±35 g | Nominal; not weighed |
| Pose RMS | — | < 0.5 mm | Registration residual |
| Yaw drift | — | < 0.25°/min | Static hold |

Surface atlas (virtual, P-fixed):

| Zone | θ range | λ range | Purpose |
|---|---|---|---|
| Lotus floor | 0–15° | all | Six-petal lotus anchor |
| Harmonic rings | 15–55° | all | Resonance readout rings |
| Trace guide | 55–72.94° | all | Speed-band ring |
| Rim segments | rim torus | 12 × 30° | Direction and lap counter |
| Cradle shadow | exterior, λ 150°–210° | — | Hand-occluded zone |
| Registration bands | exterior, λ 0°/120°/240° | 8 mm wide | Visual odometry features |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Spherical cap | `R = 85.0`, θ ≤ 72.94°, centre `(0,0,25)` | Inner surface, projection |
| C1 | Spherical cap | `R = 91.0`, θ ≤ 74.06° | Exterior cradle |
| C2 | Torus | major 84.38, minor 3.0, plane `z = 0` | Rim trace contact |
| C3 | Capsule | radius 4.5 mm, length 9 mm | `R_index_tip` trace effector |
| C4 | Box | 90 × 60 × 25 mm | Cradle palm proxy |
| C5 | Sphere shell | radius `D`, centre `(0,0,0)` | Dome pressure ring, non-colliding with hands |
| C6 | Sphere | radius 8 mm | Lotus core hit volume |

Registration: present at 0.8–1.2 m, tilt ±25° for 1.0 s, hold 0.4 s. Fit requires ≥ 20 rim inliers and ≥ 50 interior samples; accept rim RMS < 0.5 mm and plane-normal error < 0.8°. The bowl's exterior bands are used as fallback features when the interior is shadowed. `P` updates at 72 Hz; a 90 Hz prediction is clamped to 2 mm.

Re-acquisition: 1.2 s local search around the last pose; 10 rim inliers for 5 frames with RMS < 0.8 mm. During loss the dome freezes at its current radius and the lotus pulses red; resonance input is suspended so the player is not punished for a tracker outage.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Cradle hold | `L_palm`, `L_thumb_tip`, `L_index_tip`, `L_middle_tip` on C1/C4 | exterior lower third | Grip factor 0–1 |
| I5.2 | Rim trace | `R_index_tip` on C2 | torus band `|ρ − 81.24| < 7 mm`, `|z| < 6 mm` | Unwrapped rim angle `φ` |
| I5.3 | Speed lock | `R_index_tip` | band held 1.0 s | Resonance enabled |
| I5.4 | Perfect lock | `R_index_tip` | ±6% of 260 mm/s for 0.8 s | Growth ×1.25, gold dome |
| I5.5 | Direction commit | `R_index_tip` | consistent sign 1.2 s | Lap counter, no penalty |
| I5.6 | Reversal | `R_index_tip` | sign change held 200 ms | Resonance −0.15, wobble cue |
| I5.7 | Trace lift | `R_index_tip` | distance > 8 mm from C2 | Resonance decays, dome sags |
| I5.8 | Brace | `R_middle_tip` on C2 | same band, trailing index | Stability only |
| I5.9 | Cradle slip | `L_palm` | tangential force proxy > 4.1 N | Grip factor → 0.3 |

Only `R_index_tip` drives resonance. The right middle finger may rest on the rim and is ignored for speed; the right thumb, ring, and pinky are ignored. The left hand is a static cradle: palm plus thumb, index, and middle on the exterior; left ring and pinky are ignored. A trace contact is valid when its distance to C2 is < 5 mm, its radial position is within the 7 mm band, and its age > 30 ms.

## 6. Input Kinematics & Signal Processing

The trace point is projected onto the rim circle of radius `a = 81.24 mm`: `φ = atan2(q_y, q_x)`, unwrapped across ±π with a lap counter. Tangential speed is `v = a·dφ/dt`. The speed error is `e = (v − v*)/v*` with `v* = 260 mm/s`. The resonance target is `G = exp(−(e/0.223)²)`, where `0.223` is the Gaussian half-width corresponding to `σ_v = 58 mm/s`. `G = 1` at the target, `G = 0.37` at ±58 mm/s, and `G < 0.05` beyond ±100 mm/s.

Resonance amplitude follows a first-order lag: `A_dot = (G·g_grip − A)/0.45 s`, clamped to `[0, 1]`. `g_grip` is the cradle factor from I5.1: 1.0 for a full four-contact cradle, 0.6 for palm-only, 0.3 during a slip. `A > 0.65` is "locked" and permits growth. This 0.45 s time constant is long enough to reject a single bad lap and short enough that the player hears the consequence within two laps.

Signal chain: joints at 72 Hz → One Euro (mincutoff 1.2 Hz, beta 0.025, derivative cutoff 8 Hz) → 90 Hz resample with 2 mm clamp → rim projection → phase unwrap → 5-sample central difference for `dφ/dt` → second-order Butterworth low-pass at 9 Hz → radial/height gate → speed and `G`. The unwrap rejects a lap if the angular jump exceeds 40° in one tick, which catches tracking teleports without rejecting a legitimate 260 mm/s trace (18.4° per 90 Hz tick).

Wobble metric: `W = sqrt(mean(e²))` over a 0.6 s sliding window. If `W > 0.22` for 0.7 s, the dome enters "sag" (growth 0, decay 35 mm/s) and a dissonant cue plays. Wobble is also increased by direction reversals: each reversal adds `0.10` to `W` for 1.0 s. A steady trace at 260 mm/s has `W ≈ 0.02`; a 15% speed oscillation at 1.5 Hz reaches `W ≈ 0.11`; the 0.22 threshold therefore permits expressive human variation but rejects tempo drift.

Latency budget: contact-to-audio < 25 ms, contact-to-dome-growth < 40 ms. The rim contact is predicted 1 frame ahead (11.1 ms) with a 2 mm clamp; the prediction is disabled when `|dφ/dt|` changes sign, because reversal prediction causes overshoot.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to `P` as in §2.1. The dome is a hemisphere of radius `D` centred at `V` origin, opening upward (`+ẑ_V`). The lotus sits at the interior apex `(0, 0, −60) mm` and is 44 mm across. Locusts spawn on a sphere of radius `D + 40 mm` above the rim plane (`z > 0`) and dive toward the lotus.

| Entity | V-space placement | Notes |
|---|---|---|
| Dome shell | radius `D`, centre `(0,0,0)` | 90 → 190 mm, 35 mm rings |
| Lotus | `(0, 0, −60)`, 44 mm wide | 6 petals, hit radius 18 mm |
| Harmonic rings | radius `D − 6`, 12 mm pitch | Visual resonance readout |
| Locust spawn shell | radius `D + 40`, `z > 0` | 12–30 active |
| Trace guide ring | radius 81.24, `z = 0` | Speed band |
| Camera | head-locked | 400–550 mm from dome centre |
| Ambient dust | radius 240–400 mm | Parallax only |

Scale mapping: `D` is rendered 1:1 in millimetres; at the 450 mm viewing distance the 190 mm dome subtends 23.8°, which fits the central 30° of the display without head turning. The 81.24 mm rim circle has a 510.5 mm circumference, so 260 mm/s is 0.51 laps/s — one lap in 1.96 s, the intended meditative tempo. The dome growth of 45 mm/s at full resonance reaches 100 mm of growth in 2.2 s, giving one pressure ring every 0.78 s.

## 8. Gameplay Systems & Rules

Resonance: `A` from §6. Growth rate `dD/dt = 45·smoothstep(0.55, 0.95, A)·g_grip` mm/s when `A ≥ 0.55`; decay `dD/dt = −20 mm/s` when `A < 0.45`; hold between. `D` is clamped to `[90, 190] mm`. Each time `D` crosses a multiple of 35 mm upward, a pressure ring is emitted. A ring is a sphere shell of thickness 12 mm expanding at 420 mm/s for 0.28 s; any locust whose centre is inside the shell at emission is destroyed.

Locusts: spawn at rate `r_l` (0.6–1.6 per second) on the `D + 40` shell, with dive speed `v_l = 90 + 15·(sanctuary − 1) + U(−15, 15) mm/s`. They steer toward the lotus with a 1.2 s turn time constant; their path is a logarithmic spiral. A locust that reaches within 18 mm of the lotus core removes one petal, plays a crack cue, and despawns. Locusts are not affected by the dome shell except at pressure-ring emission; a locust that crosses the shell between rings passes through.

Lotus: 6 petals. Each hit removes one. Petal 6→5 is a warning; 3→2 changes the dome tint from gold to amber; 1 is critical and the drone detunes by 30 cents. At 0 petals the sanctuary fails immediately and the dome shatters.

Dome collapse: if `A < 0.15` for 1.2 s, the dome collapses to `D = 90` and one petal is lost. The player must re-lock; the collapse cannot happen twice within 4 s, so a single tracker glitch cannot chain into a loss.

Scoring: resonance time `+10/s` while `A > 0.65`; pressure-ring kill `+25` per locust; sanctuary clear `+500`; petal bonus `+100` per remaining petal; perfect sanctuary (no petals lost) `+750`. Counter-clockwise and clockwise traces score equally; only speed stability matters.

Win: `D ≥ D_target` before the 80 s clock expires with ≥ 1 petal. Lose: 0 petals or clock expiry. Partial: reaching `D_target` at the buzzer with 1 petal counts as a clear but with no petal bonus.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset tracked | load dome shader, zero score | Register |
| Register | bowl presented | rim RMS < 0.5 mm | set `P`, `R`, `a` | Attune |
| Attune | cradle + trace valid 30 ms | `g_grip > 0.6` | start drone, show speed band | Rise |
| Rise | 50.0 s timer | petals > 0 | accept resonance, grow dome | Swarm |
| Rise | `D ≥ D_target` | petals > 0 | early clear, tally | Resolve |
| Swarm | 17.0 s timer | petals > 0 | locust rate ×2, speed +20% | Resolve |
| Resolve | 5.0 s timer | sanctuary < 5 | score, rest prompt | Rest |
| Resolve | sanctuary = 5 | — | victory cue | Victory |
| Sag | `W > 0.22` for 0.7 s | petals > 0 | growth 0, decay 35 mm/s | Rise |
| Collapse | `A < 0.15` for 1.2 s | cooldown > 4 s | `D = 90`, petal −1 | Rise |
| Fail | petals = 0 | — | shatter, tally | GameOver |
| Any | prop lost > 1.2 s | — | freeze dome, suspend input | Recovery |
| Recovery | re-acquire + 400 ms | pose valid | restore, 1.5 s grace | previous |
| Any | headset removed | — | save run | Paused |

Timers run on the 90 Hz wall clock. The 80 s sanctuary clock excludes the 10 s rest. State transitions log tick, `D`, `A`, petals, and score.

## 10. Physics & Collision

| Body | Shape | Mass | Friction | Restitution | Motion |
|---|---|---|---|---|---|
| Bowl | C0/C1/C2 | 700 g | 0.60 | 0.05 | Kinematic via `P` |
| Cradle palm | C4 box | kinematic | 0.70 | 0.0 | Tracked |
| Trace index | C3 capsule | kinematic | 0.35 | 0.0 | Tracked |
| Dome shell | C5 sphere shell | non-physical | — | — | Radius `D` |
| Locust | Sphere r 5 mm | 2 g (game) | 0.1 | 0.0 | Kinematic spiral |
| Lotus | C6 sphere r 8 mm | static | — | — | Hit trigger |
| Pressure ring | Expanding shell | non-physical | — | — | 420 mm/s, 0.28 s |

Pair matrix: cradle↔C1 (grip), index↔C2 (trace), locust↔dome ring (kill), locust↔lotus (petal loss), locust↔locust (ignored), dome↔hands (no collision, hands render in front), bowl↔table (supported). The trace index is a capsule; its closest point to the torus determines contact, and a 5 mm signed-distance gate is used.

The 700 g bowl is not a rigid-body simulation. Its pose comes from tracking; the physics layer only estimates the tangential force proxy `F_t = m·|a_track|` over a 100 ms window, clamped to 8 N. If `F_t > 4.12 N` for 150 ms, a cradle slip is declared, `g_grip` drops to 0.3, and a 1.5° wobble is added to the dome anchor. This is a cosmetic and gameplay model, not a force-feedback claim.

CCD: locusts move ≤ 2.3 mm per 90 Hz tick at 200 mm/s, well below the 5 mm locust radius, so discrete sphere tests are sufficient. Pressure rings are tested analytically: a locust is killed if `| |p_l| − R_ring | < 6 mm` at the emission instant and `p_l,z > −10 mm`, which is a single shell-distance comparison, not a swept collision.

## 11. Rendering, Occlusion & Compositing

Layer order near to far: 7 HUD → 6 hand mesh → 5 near VFX (ring sparks, lotus glow) → 4 locusts → 3 dome shell → 2 bowl interior/exterior → 1 scene mesh depth → 0 passthrough. The dome is rendered with a Fresnel-weighted transparent shader (opacity 0.10 face-on, 0.55 at grazing), additive harmonic lines, and a depth prepass so locusts behind the dome are tinted, not hidden.

Occlusion cases:

1. **Cradle hand in front of the bowl.** The tracked palm hides the exterior and the lower interior; the lotus is at the floor and remains visible through the aperture. A soft amber rim light is drawn on the palm to keep the bowl readable.
2. **Trace finger crossing the rim.** The index mesh correctly occludes the rim torus and the speed-band ring; a 1.2 mm offset "contact bead" is drawn on the finger pad so the player can see the contact point even under occlusion.
3. **Locust passing behind the near rim.** The ceramic rim hides the locust body; its shadow on the interior shell remains, giving a 200 ms warning before it emerges.
4. **Locust in front of the dome but behind the hand.** Depth compositing puts the hand in front; the locust is dimmed to 40% where the hand mesh covers it, preventing a false "inside the hand" read.
5. **Dome intersecting the real table.** The dome is clipped by the scene mesh with a 1.5 mm bias; the passthrough is never tinted below the rim plane.
6. **Lotus glow through the bowl floor.** The floor is opaque; the glow is a shell decal at `z = −59 mm`, not a light leaking through ceramic. No additive light is composited on the lap.

VFX budget: 1,800 particles (900 harmonic motes, 350 ring sparks, 300 locust dissolve, 150 lotus pollen, 100 ambient), 12 harmonic ring ribbons, 1 dome shell (two-pass Fresnel), 30 locust meshes, 1 lotus mesh. Overdraw ≤ 2.2×. The dome shader is limited to 6 transparent layers; when `D` is large and the camera is close, the shell uses a single pass with a dithered opacity mask.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Bowl registered | Rim | Ceramic tap, 1.6 kHz | Rim ring snaps gold |
| Attune start | Bowl centre | Low drone, 196 Hz, slow attack | Dome seed glow |
| Speed lock | Rim | Drone gains a perfect fifth | Speed ring closes, gold |
| Perfect lock | Rim | Add octave, +3 dB | Dome turns bright gold |
| Trace too slow | Rim | Drone detunes −25 cents | Ring contracts, amber |
| Trace too fast | Rim | Drone detunes +25 cents | Ring expands, amber |
| Wobble | Bowl centre | Slow tremolo, 4 Hz | Dome surface ripples |
| Pressure ring | Dome shell | Rising chime, 3 notes | Golden shell expands |
| Locust killed | Locust position | Glassy shatter, short | Black motes dissolve |
| Lotus hit | Lotus | Ceramic crack + low thud | Petal falls, red vignette |
| Dome collapse | Dome apex | Descending glide + noise | Shell folds inward |
| Sanctuary clear | Bowl centre | Sustained major chord, 2 s | Lotus blooms, score stamp |
| Cradle slip | Cradle palm | Leather scrape | Grip arc drops, dome wobble |

The drone is the primary feedback channel: its pitch, fifth, and tremolo depth encode speed error, lock, and wobble before the player consciously reads the HUD. Audio latency budget < 25 ms; the drone is synthesized at 48 kHz with a 128-sample buffer. The lotus-hit cue ducks the drone by 12 dB for 200 ms so the damage is unmistakable.

## 13. Difficulty Progression & Tuning

| Sanctuary | Name | Target speed (mm/s) | Gaussian σ (mm/s) | Locust rate (1/s) | Locust speed (mm/s) | Target `D` (mm) | Clock (s) |
|---|---|---|---|---|---|---|---|
| 1 | Dawn Vault | 240 | 70 | 0.6 | 90 | 150 | 80 |
| 2 | Amber Cloister | 250 | 64 | 0.8 | 105 | 160 | 80 |
| 3 | Gilded Nave | 260 | 58 | 1.0 | 120 | 170 | 80 |
| 4 | Resonant Choir | 270 | 52 | 1.3 | 135 | 180 | 80 |
| 5 | Last Bell | 280 | 46 | 1.6 | 150 | 190 | 80 |

Adaptive rule: if two petals are lost before 30 s, widen `σ_v` by 8 mm/s and reduce locust speed by 10% for the current sanctuary. If no petal is lost by 60 s and `D ≥ 0.8·D_target`, narrow `σ_v` by 5 mm/s and raise locust speed by 8%, capped at sanctuary 5 values. Adaptation never changes the target speed, so the drone pitch stays a stable reference.

Named constants: `R = 85.0 mm`, `t = 6.0 mm`, `d = 60.0 mm`, `A_RIM = 81.24 mm`, `THETA_RIM = 72.94°`, `V_STAR = 260 mm/s`, `SIGMA_V = 58 mm/s`, `GAUSS_HALFWIDTH = 0.223`, `TAU_A = 0.45 s`, `A_LOCK = 0.65`, `A_SAG = 0.45`, `A_COLLAPSE = 0.15`, `D_MIN = 90 mm`, `D_MAX = 190 mm`, `GROWTH = 45 mm/s`, `DECAY = 20 mm/s`, `RING_STEP = 35 mm`, `RING_SPEED = 420 mm/s`, `RING_LIFE = 0.28 s`, `LOCUST_HIT = 18 mm`, `PETALS = 6`, `WOBBLE_MAX = 0.22`, `SLIP_FORCE = 4.12 N`.

## 14. Edge Cases & Failure Recovery

- **Trace finger occludes the rim from the headset.** Hand tracking continues through self-occlusion, but the visual speed ring is hidden. The contact bead is drawn on the dorsal side of the index with a 2 mm offset, and the drone remains the primary cue; no input is lost.
- **Bowl tracking loss.** Freeze `D`, suspend `A` decay, and tint the dome gray. Re-acquire within 1.2 s and restore with 1.5 s grace; if longer, the sanctuary pauses and a "present the bowl" prompt appears.
- **Cradle slip.** `F_t > 4.12 N` for 150 ms triggers `g_grip = 0.3`, a 1.5° dome wobble, and a scrape cue. Restoring a four-contact cradle for 500 ms returns `g_grip` to 1.0 over 300 ms.
- **Trace hand leaves the rim for more than 2.0 s.** `A` decays to 0, the dome sags but does not collapse until the 1.2 s collapse timer inside the Rise state; the player gets a full 2 s to re-engage before the collapse check.
- **Direction reversal.** A reversal held 200 ms subtracts 0.15 from `A` and adds 0.10 to `W` for 1 s. A reversal inside 200 ms is treated as a wobble, not a deliberate direction change.
- **Rim contact at the outer edge.** The band is `|ρ − 81.24| < 7 mm`; a contact at 8 mm is rejected with a "rim edge" scrape cue, preventing accidental speed spikes from the torus shoulder.
- **Ambient noise misdetected as speech.** The drone is synthesized, not mic-driven; there is no voice input. Background audio only affects the mix, with a −6 dB duck under the drone.
- **Handedness swap.** Mirror the cradle and trace hands, mirror `φ` about the sagittal plane, and negate `dφ/dt`; all thresholds are unchanged.
- **Headset removed.** Save sanctuary, `D`, petals, `A`, and `P`; on resume require re-registration and a 2 s settle before the drone restarts.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `PropTracker` | Rim fit, `P` pose, slip detection, re-acquisition |
| `BowlGeometry` | Cap/rim equations, trace projection, atlas UVs |
| `HandContacts` | Contact records, 5 mm gate, 8 mm release, debounce |
| `TraceAnalyzer` | Phase unwrap, speed, `G`, wobble, reversal |
| `ResonanceModel` | `A` ODE, grip factor, lock/sag/collapse thresholds |
| `DomeController` | Radius integration, 35 mm ring emission, shell VFX |
| `LocustSwarm` | Spawn, spiral dive, ring-kill test, lotus hit |
| `LotusState` | Petals, tint, fail check |
| `SanctuaryDirector` | Five-sanctuary table, 80 s timer, win/lose |
| `ScoreService` | Resonance seconds, kills, petals, persistence |
| `AudioDirector` | Drone synthesis, pitch/tremolo mapping, ducking |
| `SessionFSM` | State table, timers, replay log |

Event flow: `HandTracking (72 Hz) → HandContacts → TraceAnalyzer → ResonanceModel → DomeController → LocustSwarm → LotusState → SanctuaryDirector → ScoreService → AudioDirector/Renderer`. The audio director receives the resonance state one tick before the renderer so the drone leads the visual by ≤ 11 ms.

```text
function update_resonance(contact, P, dt):
    if contact.age < 30 ms or torus_distance(contact.point) > 5 mm:
        return decay_A(A, dt)
    phi = unwrap(atan2(contact.point.y, contact.point.x))
    v = 81.24 * (phi - phi_prev) / dt
    e = (v - 260.0) / 260.0
    G = exp(-(e / 0.223)^2)
    g_grip = cradle_factor(contacts_L)
    A = clamp(A + dt * (G * g_grip - A) / 0.45, 0.0, 1.0)
    W = rms(e, window=0.6 s)
    if W > 0.22 for 0.7 s: enter_sag()
    if A < 0.15 for 1.2 s and collapse_cooldown <= 0: collapse_dome()
    return A
```

```text
function emit_ring(D_old, D_new, locusts):
    if floor(D_new / 35.0) <= floor(D_old / 35.0):
        return
    ring_radius = 35.0 * floor(D_new / 35.0)
    for locust in locusts:
        d = length(locust.p)
        if abs(d - ring_radius) < 6.0 and locust.p.z > -10.0:
            kill(locust); score += 25; spawn_dissolve(locust.p)
    if ring_radius >= target_D - 1.0:
        fsm.to(RESOLVE)
```

State snapshots at each sanctuary boundary include `P`, `D`, `A`, petals, locust list, score, and timer. All constants live in `TuningConfig` and are hot-reloadable.

## 16. Acceptance Test Checklist

1. **Rim fit.** Present the bowl at 1.0 m and sample 500 frames. Pass: `R ∈ [84.6, 85.4] mm`, `a ∈ [80.8, 81.6] mm`, rim RMS < 0.5 mm.
2. **Trace projection.** Place the index at `(81.24, 0, 0)` and `(0, 81.24, 0)`. Pass: `φ = 0°` and 90° ± 0.5°, with speed 0.
3. **Speed law.** Move the index around the rim at 260 mm/s. Pass: measured `v = 260 ± 5 mm/s`, one lap in 1.96 s ± 0.03 s.
4. **Gaussian band.** Evaluate `G` at 260, 202, 318, and 360 mm/s. Pass: 1.00, 0.37, 0.37, and < 0.05 respectively.
5. **Resonance lag.** Step the input from 0 to 260 mm/s. Pass: `A` reaches 0.63 in 0.45 s ± 0.05 s and 0.86 in 0.9 s, with no overshoot above 1.0.
6. **Wobble.** Inject a 15% sinusoidal speed error at 1.5 Hz. Pass: `W` settles at 0.11 ± 0.03, below the 0.22 sag threshold.
7. **Collapse.** Hold speed at 100 mm/s for 2.0 s. Pass: `A < 0.15` by 1.0 s, collapse at 1.2 s ± 1 tick, `D` resets to 90 mm, one petal lost.
8. **Ring kill.** Place a locust at radius 35 mm and another at 45 mm when the first 35 mm ring emits. Pass: only the first is killed; score +25.
9. **Lotus hit.** Drive a locust into the 18 mm lotus sphere. Pass: exactly one petal lost, one crack cue, locust despawned.
10. **Cradle slip.** Apply a 5 N tangential proxy for 200 ms. Pass: `g_grip` drops to 0.30 ± 0.02, dome growth stops, scrape cue fires once.
11. **Early clear.** Reach `D_target` at 42 s. Pass: state moves to Resolve at the crossing tick, remaining time is not scored as failure.
12. **Occlusion.** Put the cradle palm between camera and lotus. Pass: no lotus pixels appear on the palm over 600 frames, and the contact bead remains visible.
13. **Frame budget.** Run sanctuary 5 with 30 locusts, 1,800 particles, and a 190 mm dome. Pass: GPU < 11.1 ms, CPU game thread < 6 ms, overdraw ≤ 2.2×.
14. **Recovery.** Hide the bowl for 1.0 s, then re-present. Pass: re-acquire ≤ 1.2 s, `D` unchanged, no petal lost, drone resumes within 250 ms.
