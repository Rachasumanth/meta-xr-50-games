# Clam Pearl Guard

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Clam Pearl Guard |
| Slug | `clam_pearl_guard` |
| Physical prop | Opaque gumball, true sphere; 17.0 mm diameter (r = 8.5 mm); matte sugar shell over gum base, 1.24 g/cm³ effective; mass 6.2 g; surface µ = 0.55 |
| Family / sub-shape | True sphere, pinch-scale, palm-cradled |
| Scale class | Prop 1:1; virtual clam 320 mm across, mapped to the left hand |
| Source concept quote | "Rest the small sphere in the deep center hollow of your upturned palm; curl your five fingers inward like a cage or open them flat." |
| Physical input | Palm cradle + five-finger curl/extension; no rolling, no pinching |
| Session length | 7 min (5 tides, 3 lunges each, 6 s charge windows) |
| Lose condition | Pearl integrity reaches 0 |

## 1. Fantasy & Core Loop

The player is a reef warden holding a living pearl in the hollow of an upturned palm. The hand becomes the hinge of a giant virtual clam: fingers open and the shell yawns wide, letting five shafts of pelagic sunlight pour onto the pearl; fingers snap shut and the shell claps into a nacre cage just before a barracuda's jaws arrive. The physical gumball never moves relative to the palm. All drama is in the hand silhouette, the shell angle, and the timing of the closing fist.

1. **Present the pearl (3.0 s).** The palm is scanned and the pearl registers into the palm hollow; the clam grows around the hand with a soft bloom.
2. **Charge (8.0 s).** Fingers spread; shell opens to 70°; five sunlight shafts strike the pearl. Charge accrues at 12 units/s at full openness.
3. **Telegraph (0.5 s).** A barracuda materialises at the reef edge: beak glow, rising tone, and a shadow line across the palm. The player must read the direction and prepare.
4. **Cage (0.12 s + 0.9 s hold).** All five fingertips close inside 35 mm of the palm hollow. The shell slams shut; a blocked lunge cracks the water with nacre light. The fist is held 900 ms while the fish thrashes.
5. **Resolve (2.5 s).** The barracuda retreats, the shell reopens, and charge resumes. Two to four lunges make a tide (see §13); five tides make a run.

One lunge cycle is 15.0 s (8.0 charge + 0.5 telegraph + 0.12 closure + 0.9 hold + 2.5 resolve + 3.0 slack), three cycles per tide give 45 s, and five tides plus onboarding and rest total 7 min. The full-cycle duration the tuning loop optimises is 11.52 s from telegraph start to the next telegraph start during a surge (0.5 + 0.12 + 0.9 + 2.5 + 7.5 charge).

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

- **P-frame**: origin at the gumball center, which sits `r + 1.0 = 9.5 mm` above the palm plane. `P` is rigidly slaved to the left palm: `P = L_palm · T_offset`, with `T_offset` solved once at registration. Poles are `±ẑ_P` with `+ẑ_P` along the palm's volar normal (upward for an upturned palm); the equator lies parallel to the palm plane.
- **Radius**: `r = 8.5 mm` for the collider and all surface mapping; visual radius 8.5 mm ± 0.12 mm.
- **Equator and poles**: `+ẑ_P` points out of the palm, so latitude `φ = +90°` is the exposed crown of the pearl; `φ = −90°` is the hidden contact patch against the palm. Longitude `λ = 0` points from wrist toward the middle MCP.
- **Rolling kinematics**: the sphere is palm-locked and does not roll during play; by design `ω ≡ 0`, and `ω = (n × v_t)/r` is evaluated only to reject residual slip. If the measured residual `|v_c|` exceeds 25 mm/s, the pearl is treated as having shifted in the palm, `T_offset` is re-solved over 300 ms, and the clam re-seats with a visual settle.
- **V-frame anchoring**: `V` is anchored to `P` (therefore to the left palm). `V_O = P_O + 120 mm·ẑ_P`; axes `x̂_V = x̂_P` (distal), `ŷ_V = ŷ_P` (radial), `ẑ_V = ẑ_P` (up). The clam shell, sunlight shafts, and barracuda tracks are all expressed in `V` so they travel with the hand.
- **Deviation 1**: the prop is not an input transducer for motion; it is a tracked anchor and a visual pearl. All control comes from finger curl.
- **Deviation 2**: the 320 mm clam is much larger than the prop, so `V` renders at the hand scale while the pearl stays 1:1; the shell is intentionally non-physical and can intersect the real forearm without collision.
- **Deviation 3**: because `P` follows the palm with a 1.5 Hz low-pass, fast wrist rolls do not make the clam swim; a deliberate wrist tilt still shifts the sunlight focus within ±25°.
- **Deviation 4**: the five-finger cage is a gesture state, not a contact state; fingertip proximity to the sphere is not required and is not scored.

## 3. Physical Play Space & Ergonomics

The player sits with the left forearm fully supported, elbow at 80–100° flexion, palm up and open at 200–260 mm in front of the sternum, 60–90 mm above the table. The right hand rests on the thigh or table and is not used. The head tilts 10–15° down. The left palm normal starts within 15° of vertical; the game tolerates ±25° of wrist roll and ±20° of wrist pitch before the sunlight focus degrades.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Table `T` or lap; left ulna supported | Palm-up holding is biceps-loaded without support |
| Play patch | 170 mm × 170 mm around the left palm | Covers palm drift and finger spread |
| Reach envelope | Left wrist within 280 mm of sternum | Avoids shoulder elevation |
| Palm height | 60–90 mm above `T` | Keeps the clam out of the table mesh |
| Session | 7 min; 45 s tides; 8 s rest between tides | Five tides fit grip stamina |
| Cage hold | 900 ms maximum, never repeated faster than 1.2 s | Prevents sustained fist clenching |
| Open-phase rest | ≥ 2.5 s of open hand per cycle | Forearm and intrinsic muscle recovery |

Pinch-fatigue is replaced by **grip-fatigue** management: the cage is a brief, ballistic close, not a sustained squeeze; the hold is capped at 900 ms; each tide ends with a mandatory 8 s open-hand rest; the HUD shows a "tendon load" meter that fills only while the fist is closed and drains twice as fast when open. The game never requires thumb-to-fingertip pinching, so the thenar eminence is unloaded. If the player has limited finger mobility, a comfort mode lowers the cage threshold to a personal calibrated fist and reduces the required hold to 600 ms.

## 4. Prop Geometry & Registration

The prop is a 17.0 mm opaque gumball. Nominal mass 6.2 g, effective density 1.24 g/cm³ (hollow gum core), moment of inertia `I = 2/5·m·r² = 1.79 × 10⁻⁷ kg·m²`, surface µ = 0.55 against dry skin. It rests in the palm hollow and is not rolled, thrown, or pinched.

| Sphere property | Value |
|---|---|
| Diameter | 17.0 mm ± 0.3 mm |
| Radius `r` | 8.5 mm |
| Mass | 6.2 g ± 0.4 g |
| Collider | Perfect sphere, r = 8.5 mm |
| Visual mesh | 64 × 32 UV sphere, nacre shader, subsurface 0.4 |
| Pose source | Palm-locked offset `T_offset`, low-passed at 1.5 Hz |
| Palm offset | 9.5 mm along `+ẑ_L`, 0.0 mm lateral |

Surface-feature atlas (P-fixed; the pearl does not spin, so features are stable):

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| Charge crown | +55° to +90° | all | Sunlight absorption aperture; glows with charge |
| Nacre ring A | +30° | all | Charge-level readout |
| Nacre ring B | 0° | all | Charge-level readout |
| Nacre ring C | −30° | all | Charge-level readout |
| Seam 0 | all | 0° ± 2° | Alignment with middle finger |
| Sun groove | −10° to +10° | 85°–95° | Decorative; catches shaft light |
| Palm shadow | −90° to −60° | all | Hidden contact patch; never lit |
| Bite scar | −5° to +15° | 200°–230° | Flashes red at integrity < 40 |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 8.5 mm at `P` | Pearl, anchor |
| C1–C5 | Capsules | r = 2.0 mm, length 6.0 mm at each fingertip | Cage fingers |
| C6 | Plane | palm plane, 60 × 80 mm | Palm support |
| C7 | Two half-shells | radius 160 mm, thickness 3 mm, hinge at wrist | Virtual clam |
| C8 | Capsule | r = 14 mm, length 80 mm | Barracuda body |
| C9 | Cone | 30° half-angle, 90 mm length | Barracuda bite volume |

Registration: the player presents the open palm with the gumball at 0.6–1.0 m from the head for 0.8 s. The fitter uses silhouette plus shading, accepts RMS < 0.35 mm and radius error < 0.12 mm, and solves `T_offset` in `L_palm` over 10 frames. The palm frame is defined by `L_middle_mcp`, `L_index_mcp`, `L_pinky_mcp`, and `L_wrist` if available, else the MCP centroid and palm normal from the hand mesh.

Re-acquisition: if the sphere is lost (hand rotation, glare, occlusion), the system falls back to `P = L_palm · T_offset` immediately, because the sphere cannot leave the palm without a detected shift. A visual re-fit runs every 500 ms; when it succeeds with RMS < 0.6 mm for 5 frames, `T_offset` is blended over 300 ms. Loss of the palm itself triggers Recovery.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface / region | Semantic output |
|---|---|---|---|---|
| I5.1 | Open palm | all five `L_*_tip` | pads shadow φ +5° to +45°, λ spread 260°–100° | Openness `O` |
| I5.2 | Full cage | all five `L_*_tip` | tips converge over φ −15° to +45°, λ 260°–110° | Cage state `C` |
| I5.3 | Thumb curl | `L_thumb_cmc/mcp/ip/tip` | shadow φ −20° to +10°, λ 70°–110° | `θ_thumb` |
| I5.4 | Index curl | `L_index_mcp/pip/dip/tip` | shadow φ +5° to +35°, λ 20°–60° | `θ_index` |
| I5.5 | Middle curl | `L_middle_*` | shadow φ +15° to +45°, λ 340°–20° | `θ_middle` |
| I5.6 | Ring curl | `L_ring_*` | shadow φ +5° to +35°, λ 300°–340° | `θ_ring` |
| I5.7 | Pinky curl | `L_pinky_*` | shadow φ −15° to +15°, λ 260°–300° | `θ_pinky` |
| I5.8 | Wrist tilt | `L_wrist` + palm normal | palm normal vs `+ŷ_W` | Sun focus `F` ∈ [0.8, 1.2] |
| I5.9 | Cage tighten | all five tips | tip radius < 25 mm | "Lock" bonus, ×1.25 block score |
| I5.10 | Open flash | all five tips | mean curl < 20° within 150 ms | Charge burst +8 units |

The "shadow region" is where each fingertip's orthogonal projection lands on the pearl surface, not a contact patch; it is used by the VFX to paint finger-shaped light blockers on the shell. Per-finger roles: the thumb sets the shell's lower jaw; index and middle set the upper jaw; ring and pinky drive the cage closure speed. The right hand is idle and its gestures are ignored.

## 6. Input Kinematics & Signal Processing

Per-finger curl is `θ_i = angle(mcp→pip, pip→tip)` for index/middle/ring/pinky and `θ_thumb = angle(cmc→mcp, mcp→tip)`. Straight fingers give 0°; a tight fist gives 140–160°. Curl is clamped to [0°, 170°] and low-passed at 9 Hz. Mean curl `Θ = mean(θ_i)`. Openness is `O = clamp((120° − Θ)/(120° − 20°), 0, 1)`, so `O = 1` exactly when `Θ ≤ 20°`, matching the design rule.

Cage geometry uses the palm hollow `H = C_palm + 12 mm·ẑ_L`, where `C_palm` is the MCP centroid. The per-player fist calibration records `d_i^fist` during onboarding and defines normalized distances `d̃_i = d_i · 35 / mean(d_i^fist)`, so a personal fist always reads ≤ 35 mm. Cage `C = 1` when all `d̃_i < 35 mm` for 30 ms; hysteresis releases at 42 mm. Closure time is measured from the first fingertip crossing to the last, and must be ≤ 120 ms for a "fast cage".

Sun focus is `F = clamp(1 + 0.35·sin(2ψ), 0.8, 1.2)` where `ψ` is the palm tilt from vertical in radians; charge rate is `R_c = 12.0·O·F` units/s nominal (per-tide 11.0–13.0, §13) with capacity 100. A full open palm at neutral tilt therefore fills the pearl in 8.33 s, matching the 8 s charge window; a badly tilted palm takes up to 10.4 s, which is the intended skill expression. Open flash adds 8 units once per 1.5 s.

Signal chain: raw joints → confidence gate (hold last good pose if joint confidence < 0.6) → One Euro filter (mincutoff 1.5 Hz, beta 0.015, derivative cutoff 10 Hz) → curl and distances at 90 Hz → 30 ms debounce → classifiers. Angular velocity of curl is clamped to 900 °/s to reject tracking pops; a fist that closes faster than 40 ms is treated as a tracking artefact and ignored (human closure floor is ~60 ms).

Barracuda timing: telegraph opens at `t = 0` and lasts 500 ms; strike resolves at `t = 620 ms`. A cage that is complete by 620 ms blocks fully; complete between 620 and 740 ms gives a late block (half damage, half score); after 740 ms the bite lands. These windows are evaluated on the 90 Hz tick with ±11 ms resolution. Event rates: 72 Hz input, 90 Hz game logic, one classifier decision per tick.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to the palm as defined in §2.1, with `x̂_V` distal, `ŷ_V` radial (toward the thumb), and `ẑ_V` out of the palm. The pearl sits at `V = (0, 0, −111.5) mm` (because `P` is 9.5 mm above the palm and `V` is 120 mm above the palm). The clam hinge is at `V = (−160, 0, −120) mm`; each half-shell is a 160 mm radius spherical cap that rotates about the hinge axis `ŷ_V`.

| Entity | V-space position | Notes |
|---|---|---|
| Pearl | `(0, 0, −111.5)` | 1:1, r = 8.5 mm |
| Clam hinge | `(−160, 0, −120)` | 220 mm from pearl |
| Shell opening | 0° closed, 70° open | Driven by `O` with 8 Hz servo |
| Sun shafts | `x = −60…+60`, `y = −45…+45`, `z = +300` | 5 cylinders, radius 18 mm |
| Barracuda spawn | `x = +420`, `y = ±160`, `z = +20` | Approaches along `−x̂_V` |
| Bite point | `(60, 0, −40)` | 90 mm cone reaches the pearl |
| HUD ring | radius 210 mm about `V` origin | Palm-locked score and integrity |

Mapping `P` orientation to camera-facing visuals: because `V` is palm-locked, the clam rolls with the wrist. To keep the pearl readable, the renderer applies a 35% counter-rotation to the shell's *shading* (not geometry) so the sun highlight stays near the top of the screen while the physical hand tilts. The pearl's own orientation is identity in `V`; its charge rings are billboarded 15% toward the camera so the player can read charge at a glance. When the palm is edge-on to the camera, the HUD ring rotates into a vertical arc to remain legible.

The sunlight shafts are placed in `V` and therefore move with the hand, which preserves the "sun above the reef" illusion while the player repositions. Barracudas approach from the distal side, so the player reads the threat direction from the hand silhouette rather than from world coordinates.

## 8. Gameplay Systems & Rules

Entities: pearl (charge 0–100, integrity 100), clam (opening 0–70°), five sunlight shafts, barracuda (one active at a time, three per tide), nacre shield (one charge at 60+ bloom), and score pickups emitted by perfect blocks.

Spawn rules: each tide schedules its lunge count from §13 (2–4) at 6.5 s intervals starting at `t = 0`, alternating approach sides `+ŷ_V` and `−ŷ_V`. Lunge speed is `v_b = 820 + 80·(tide − 1) mm/s`; the bite resolves at 620 ms after telegraph. A tide ends after its final lunge resolves and the shell has reopened for 2.5 s.

Charge and bloom: `charge += R_c·dt` while no lunge is active and `O > 0.5`. At 100 the pearl blooms for 2.0 s, converts to score at 2 points per unit, grants one nacre shield charge, and resets to 0. Charge is not lost on a hit.

Damage: full bite `−22` integrity nominal (18–26 by tide, §13); late block `−11`; blocked `0`. If integrity reaches 0 the pearl cracks and the run ends. Nacre shield absorbs one full bite if charged and then shatters with a distinctive cue.

Score: bloom `+200`; block `+150·(1 + 0.25·fast + 0.25·tight)`, where `fast = 1` if closure ≤ 120 ms and `tight = 1` if the final tip radius < 25 mm; late block `+60`; tide clear without damage `+300`; consecutive fast blocks multiply by 1.25 per link up to ×3. Integrity bonus at run end is `10 × integrity`.

Win: survive all five tides. Lose: integrity 0. Perfect run (no damage across five tides) unlocks the "nacre sovereign" badge and a golden shell palette.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset + hands tracked | zero counters, load shell | Register |
| Register | palm presented | sphere fit RMS < 0.35 mm | solve `T_offset`, fist calibration | PalmLock |
| PalmLock | 0.8 s stable palm | palm confidence > 0.8 | grow clam, enable charge | TideIntro |
| TideIntro | 2.5 s timer | tide ≤ 5 | banner, schedule tide lunges | Charge |
| Charge | charge = 100 | no active lunge | bloom, score, shield | Charge |
| Charge | lunge timer fires | `O > 0.5` or any `O` | spawn fish, telegraph cue | Telegraph |
| Telegraph | 500 ms elapsed | — | open damage window, beak glow | CageWindow |
| CageWindow | `C = 1` by 620 ms | closure ≤ 120 ms | block, score, hold | Hold |
| CageWindow | `C = 1` 620–740 ms | — | late block, half damage | Hold |
| CageWindow | 740 ms elapsed | `C = 0` | full bite, integrity −22 | Resolve |
| Hold | 900 ms timer | — | fish thrashes, then retreats | Resolve |
| Resolve | 2.5 s timer | lunge < 3 | tally, reopen shell | Charge |
| Resolve | lunge = 3 | tide < 5 | tide clear bonus, rest 8 s | TideIntro |
| Resolve | tide = 5 | integrity > 0 | victory cue | Victory |
| Any | integrity ≤ 0 | — | crack, shell opens, fade | GameOver |
| Any | palm lost > 1.0 s | — | freeze timers, damp | Recovery |
| Recovery | palm valid 500 ms | sphere found | re-seat, 2 s grace | previous |
| Any | headset removed | — | save tide and integrity | Paused |

Timer resolution is 11.1 ms (90 Hz). The telegraph-to-strike window is never scaled by difficulty; only the lunge count, speed, and damage change, so the 500 ms read stays fair.

## 10. Physics & Collision

| Body | Shape | Mass | Friction | Restitution |
|---|---|---|---|---|
| Pearl | Sphere r 8.5 mm | 6.2 g | 0.55 vs skin | 0.05 |
| Fingertip cage | 5 capsules r 2.0 mm | kinematic | — | — |
| Clam shells | 2 half-shells r 160 mm | kinematic | — | — |
| Barracuda | Capsule r 14 mm, L 80 mm | 1.8 kg (virtual) | — | — |
| Bite cone | Cone 30°, 90 mm | virtual | — | — |

Pair matrix: pearl↔palm (resting contact, no dynamics), fingertip↔pearl (input, no force), barracuda↔pearl (bite), barracuda↔clam shell (block), shell↔forearm (visual only, no collision). The real pearl is palm-locked, so the engine treats C0 as kinematic; no impulse is ever applied to it.

Bite resolution: if the bite cone intersects C0 at `t = 620 ms`, damage is applied according to the cage state. Block impulse is cosmetic: `J = 0.02 N·s` along `−x̂_V`, used to drive shell recoil (`2°` at 12 Hz damped). Shell servo: `θ̈ = −k(θ − θ_t) − cθ̇`, `k = 64 s⁻²`, `c = 11 s⁻¹`, giving an 8 Hz settle; closing from 70° to 0° takes 118 ms at `k = 64`, which matches the 120 ms design target.

Pearl shift handling: if the visual fit moves `T_offset` by more than 1.5 mm, the engine re-solves over 300 ms and emits a "pearl settles" cue. A shift beyond 6.0 mm is a drop: the pearl is considered out of the palm and the Recovery flow starts. Table and lap have no physics interaction beyond the support plane at `z = −120 mm` in `V`.

## 11. Rendering, Occlusion & Compositing

Layer order: 7 UI/HUD → 6 hand mesh → 5 near VFX (sun motes, nacre sparks) → 4 clam shell upper → 3 pearl → 2 barracuda → 1 clam shell lower → 0 passthrough. The hand mesh is tracked geometry and is always depth-authoritative over virtual content.

Occlusion cases:

1. **Curling fingers in front of the pearl.** Each finger mesh occludes the pearl where it passes in front; the charge crown remains visible through the gaps, and a 2 mm-offset rim light traces the pearl's silhouette so it never disappears completely.
2. **Pearl resting in the palm.** The palm mesh naturally hides the lower hemisphere; the hidden patch is never shaded, and the visible terminator matches the sunlight direction.
3. **Closed fist around the pearl.** The pearl is fully enclosed; the renderer does not x-ray it. Instead, a 20% emissive outline and the HUD charge ring carry the state, and a soft internal glow leaks through finger gaps.
4. **Clam shell behind the hand.** The lower shell is depth-tested against the palm and forearm; where the real hand is nearer, the shell is cut cleanly with a 1.5 mm bias to avoid z-fighting.
5. **Barracuda crossing the palm.** The fish is drawn in front of the shell but behind the hand; when it passes between palm and face, the hand occludes it exactly, which sells the depth of the reef.
6. **Sun shafts intersecting the fingers.** Volumetric shafts are clipped by the hand mesh and by the clam shell; finger shadows fall across the pearl as five dark bands, driven by the fingertip shadow regions from §5.

VFX budget: 700 particles (300 sun motes, 200 nacre sparks, 120 bubble trails, 80 bite spray), 5 shaft meshes, 2 shell meshes, 1 pearl subsurface pass, 1 barracuda. Global cap 700; overdraw ≤ 2.0×; shell translucency is a single pre-multiplied layer, no per-fragment refraction. The pearl is rendered with a custom nacre shader (thin-film interference, 3 lobes) at 90 Hz.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Palm registered | Left palm | Wet shell opening, slow | Clam grows, rim light |
| Sun shaft hit | Shaft base | Shimmering chime, 5 pitches | Nacre ring brightens |
| Charge tick | Pearl | Soft pulse every 25 units | Ring A/B/C fill |
| Bloom | Pearl | Rising choir swell | Golden shell palette, shield orb |
| Telegraph | Barracuda spawn | Low fin swish + rising tone | Beak glow, shadow line |
| Fast cage | Left hand | Sharp shell clap, 90 Hz thump | Nacre shockwave, +150 |
| Late cage | Left hand | Muffled clap | Half ring, amber flash |
| Bite | Pearl | Crunch + water rush | Red bite scar, integrity drop |
| Block | Shell | Glassy gong + fish screech | Shell recoil, sparks |
| Tide clear | HUD ring | Warm reef cadence | Score tally, rest prompt |
| Pearl cracked | Pearl | Deep crack, pitch down | Fracture lines, fade to blue |
| Recovery | Palm | Sonar ping | Ghost palm, re-seat animation |

Spatialisation is at the palm, the bite point, or the shell hinge; only the HUD tally is head-locked. The bite crunch ducks the ambient reef bed by 8 dB for 400 ms. All cues have visual twins so the game is playable with audio muted.

## 13. Difficulty Progression & Tuning

| Tide | Lunges | Telegraph (ms) | Lunge speed (mm/s) | Full bite damage | Charge rate (units/s) | Cage threshold (mm) | Fast-cage window (ms) |
|---|---|---|---|---|---|---|---|
| 1 | 2 | 650 | 820 | 18 | 13.0 | 37 | 150 |
| 2 | 3 | 600 | 900 | 20 | 12.5 | 36 | 140 |
| 3 | 3 | 550 | 980 | 22 | 12.0 | 35 | 130 |
| 4 | 3 | 500 | 1060 | 24 | 11.5 | 34 | 120 |
| 5 | 4 | 500 | 1140 | 26 | 11.0 | 33 | 110 |

Adaptive rule: after a tide with 2+ bites, widen the cage threshold by 1.5 mm and lengthen the telegraph by 50 ms for the next tide only. After a tide with no damage and all fast blocks, shorten the telegraph by 25 ms and add 0.03 to the charge rate, capped at the table's tide-5 values. Adaptation is disabled in the final tide so the authored climax is preserved.

Named tuning constants: `R_CHARGE = 12.0 /s`, `CHARGE_MAX = 100`, `OPEN_MEAN = 20°`, `CURL_FULL = 120°`, `CAGE_R = 35 mm`, `CAGE_RELEASE = 42 mm`, `CAGE_WINDOW = 120 ms`, `TELEGRAPH = 500 ms`, `STRIKE = 620 ms`, `LATE_END = 740 ms`, `HOLD = 900 ms`, `FOCUS_MIN = 0.8`, `FOCUS_MAX = 1.2`, `BLOOM = 100`, `DAMAGE = 22`.

## 14. Edge Cases & Failure Recovery

- **Palm tracking loss during a lunge.** Freeze the strike timer for up to 250 ms. If the palm returns, resume with the same remaining telegraph; if not, treat the lunge as blocked with no score (fail-safe, never a phantom bite) and re-enter Recovery.
- **Hand tracking loss during charge.** Charge pauses; the shell holds its angle; a "hand lost" ping plays. Recovery restores charge at the same value and grants 1.5 s of invulnerability.
- **Pearl rolls out of the palm.** A `T_offset` shift > 6 mm or a free-fall signature triggers the drop flow: timers freeze, the shell opens, and the player has 10 s to re-present. Success resumes the tide at the last lunge count; failure ends the run softly with a "pearl lost" card.
- **Palm edge-on to the camera.** HUD ring rotates to vertical; charge rate is unaffected; the pearl outline thickens to stay visible.
- **Occlusion by a sleeve or blanket.** The hand mesh becomes unreliable; the system drops to palm-only tracking and freezes curl at the last confident value for 800 ms, then pauses.
- **Pinky tracking failure.** Cage is evaluated on the four reliable fingers plus a 1.0× pinky proxy; the visual shell shows a brief "4-finger" glyph so the player knows the cage is softer.
- **Handedness swap.** Mirror the whole setup to the right hand; `T_offset`, curl signs, and barracuda spawn sides mirror, thresholds unchanged.
- **Fatigue or tremor.** A 3 s mean curl oscillation > 12° with no commanded motion triggers a "shake out" prompt; the current lunge is auto-blocked and the next tide starts after a 10 s rest.
- **Repeated late cages.** After three late blocks in one tide, add a 80 ms early warning tick to help the player recalibrate; do not change the strike timing.
- **Headset removed.** Save tide, lunge index, charge, integrity, and `T_offset`; on resume, require re-registration and a 2 s settle.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `PalmFrame` | Builds `L_palm` from MCPs, low-pass, confidence, drop detection |
| `PearlRegistration` | Sphere fit, `T_offset`, re-solve, shift/drop classification |
| `CurlEstimator` | Per-finger curl, mean curl, openness, fist calibration |
| `CageClassifier` | Normalized tip distances, 35/42 mm hysteresis, 120 ms closure |
| `SunCharger` | Focus from palm tilt, charge integration, bloom |
| `BarracudaDirector` | Spawn sides, telegraph/strike timers, bite cone |
| `PearlState` | Charge, integrity, shield, score |
| `ClamRenderer` | Shell servo, occlusion, shadow bands, nacre shader |
| `TideDirector` | Five-tide table, lunge schedule, rest, adaptation |
| `FeedbackDirector` | Audio/VFX cue table, ducking |
| `SessionFSM` | States, timers, replay log |

Event flow: `HandTracking(72 Hz) → PalmFrame → CurlEstimator/CageClassifier + PearlRegistration → SunCharger/BarracudaDirector → PearlState → ClamRenderer/FeedbackDirector`. `TideDirector` owns the lunge schedule and reads `CageClassifier` only at the 620 ms and 740 ms decision points.

```text
function cage_state(tips, H, fist_cal):
    d = []
    for i in 0..4:
        d[i] = |tips[i] - H| * 35 / fist_cal[i]
    closed = all(d[i] < 35) for 30 ms
    opened = all(d[i] > 42) for 30 ms
    if closed and not was_closed:
        closure_ms = now - first_tip_cross
        was_closed = true
        emit CageClosed(closure_ms, max(d))
    if opened: was_closed = false
    return was_closed
```

```text
function resolve_strike(t, cage, closure_ms, integrity, score):
    if t < 620 ms:
        if cage: return block(score + 150 * fast_bonus(closure_ms))
        return WAIT
    if t < 740 ms:
        if cage: integrity -= 11; return block(score + 60)
        return WAIT
    if cage: return block(0)
    integrity -= 22
    if integrity <= 0: return GAME_OVER
    return BITTEN
```

The implementation keeps `T_offset`, fist calibration, and all tuning constants in one serialized profile per player. All state transitions log tick, `V` pose, `O`, `C`, and decision reason for replay.

## 16. Acceptance Test Checklist

1. **Sphere fit.** Present the gumball at 0.8 m for 120 frames. Pass: `r` ∈ [8.38, 8.62] mm, RMS < 0.35 mm, `T_offset` stable within 0.5 mm.
2. **Palm lock.** Rotate the palm ±40° at 90 °/s. Pass: pearl pose error < 0.6 mm in `L_palm`, and rendered clam lag < 2 frames.
3. **Openness.** Drive synthetic curls at mean 19°, 20°, 21°, 70°, and 121°. Pass: `O = 1.0, 1.0, 0.99, 0.50, 0.0` ± 0.01.
4. **Charge rate.** Hold `O = 1`, neutral tilt for 5.0 s. Pass: charge = 60 ± 1 units; full bloom at 8.33 s ± 0.1 s.
5. **Tilt focus.** Set palm tilt to 0°, 10°, and 30°. Pass: `F = 1.00, 1.17, 0.80` ± 0.02.
6. **Cage threshold.** Close all tips to normalized 34 mm, 36 mm, and 41 mm. Pass: cage at 34 mm, no cage at 36 mm, release only above 42 mm.
7. **Closure window.** Script a 90 ms and a 150 ms closure. Pass: 90 ms → fast bonus; 150 ms → no fast bonus; classification stable over 100 trials.
8. **Bite timing.** Resolve a bite with cage complete at 610 ms, 700 ms, and 800 ms. Pass: full block, late block, full bite respectively; damage 0, 11, 22.
9. **No phantom bite.** Drop hand tracking from 300 ms to 700 ms. Pass: no integrity loss, no score gain, Resume state entered, and the lunge re-armed once.
10. **Occlusion.** Place the palm between camera and pearl and close the fist. Pass: pearl geometry fully hidden, 20% outline visible, no x-ray, no z-fighting over 300 frames.
11. **Drop detection.** Move the sphere 7 mm in the palm over 200 ms. Pass: shift re-solve fires, no drop; at 12 mm, drop flow starts within 150 ms.
12. **Frame budget.** Run tide 5 with 700 particles and two shell meshes. Pass: GPU < 11.1 ms, CPU < 5 ms, particle count ≤ 700.
13. **Session endurance.** Run the 7 min scripted session. Pass: no `T_offset` drift > 1.0 mm, no charge discontinuity, memory growth < 4 MB.
14. **Accessibility.** Enable comfort mode with a personal fist calibration at 44 mm. Pass: cage still triggers, closure window 160 ms, and all five tides completable.
