# Bobsled Chute

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Bobsled Chute |
| Slug | `bobsled_chute` |
| Physical prop | Oval soap bar, 90 × 60 × 30 mm nominal; 110 g; smooth, slightly convex faces, matte finish |
| Registered contact spheroid | Oblate; `a = b = 30.0 mm`, `c = 15.0 mm` (60 × 60 × 30 mm fitted body); the soap's 90 mm long axis overhangs the collider by 15 mm at each end |
| Family / sub-shape | 4.2 Prolate and Oblate Spheroids / oblate, palm-held, sliding controller |
| Scale class | Physical prop 1:1; virtual chute 900 mm arc folded into a 300 × 160 mm palm-anchored footprint; tabletop scale |
| Source concept quote | "A miniature Olympic bobsled on a twisting ice chute flowing through the hand. Palm tilt = bank angle; fore/aft slide = brake/accelerate; chute curvature from the palm frame; centrifugal flip risk; rim collision; five descents with split times." |
| Physical input | Soap lies lengthwise across the upturned left palm; right index and middle slide it 20 mm fore/aft; left wrist rolls and pitches |
| Session length | 3 min 40 s: onboarding 60 s + 5 descents × 3 heats × (1.2 s run + 4 s reset) + 4 intermissions × 8 s + results 30 s |
| Lose condition | Two crashes in one heat (DNF), or all three heats of a descent DNF |

## 1. Fantasy & Core Loop

The chute is a ribbon of black ice that starts at the wrist and pours between the fingers. It is one of five Olympic descents, each 900 mm of serpentine ice folded into the palm, and the soap is the sled's tiller: slide it toward the fingers to tuck and accelerate, drag it back toward the wrist to brake, roll the palm to bank the whole chute against gravity. The sled is 14 mm long and weighs 220 g in the fiction; it hits 2 m/s in the straights and 9.7 m/s² of lateral load in the hairpins. Bank too little and it slides into the rim; bank too much and it flips. The run is short enough to memorize and long enough that a 0.05 s mistake shows up in the split.

1. **INSPECT (2.0 s).** The chute unrolls across the palm. Three sector gates glow at 300, 600, and 900 mm; the two hairpins are marked with blue ice.
2. **TUCK (0.2 s).** The player slides the soap 15–20 mm toward the fingers. The sled's drag drops and a 1.4 m/s² tuck acceleration engages.
3. **BANK (0.3–0.6 s).** The left wrist rolls 10–25° into the first hairpin. The chute banks with the palm; gravity supplies the centripetal force.
4. **BRAKE (0.2 s).** Dragging the soap back 10–20 mm applies up to 2.5 m/s² of braking before the second hairpin.
5. **SPLIT (0.05 s).** The sled crosses each 300 mm gate and the split is latched to 1 ms. Sector splits and total time are shown in the HUD.
6. **RESET (4.0 s).** The sled returns to the start; the player may adjust the soap and bank before the next heat. Best of three heats counts.

A clean descent is 0.75–1.05 s. The run is a *control* problem, not a reflexes problem: the chute is fixed in the palm frame, so the player learns the curvature pattern and banks predictively. The physical soap never leaves the palm; only its 20 mm slide and the wrist's 25° roll matter.

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

`P` origin is the centre of the fitted body spheroid; `+ẑ_P` points out of the soap's convex top face, `+x̂_P` runs along the 90 mm long axis toward the thumb, and `+ŷ_P` runs across the bar toward the pinky. Exact semiaxes are `a = b = 30.0 mm` and `c = 15.0 mm`; the 90 mm bar overhangs the 60 mm fitted body by 15 mm at each end of `x̂_P`, and that overhang is a grip feature outside the collider. Because `c < a`, the prop is oblate, and the top face's local radius of curvature is `R_p = a²/c = 60.0 mm`. The local rolling model is `ω = (n × v_t)/r_eff`, with `r_eff = |p_P|`; at the pole `r_eff = 15.0 mm`, at the equator `30.0 mm`. `V` is anchored to the left palm frame: origin at the palm centre, `x̂_V` from wrist to middle MCP (the chute's forward direction), `ŷ_V` from thumb to pinky, `ẑ_V` along the palm normal. The soap's long axis lies along `ŷ_V`; its slide is measured along `x̂_V`. Four deviations:

1. **Grip overhang.** The 15 mm end overhangs and the palm's grip are outside the 60 × 60 × 30 mm body spheroid. The overhangs are never tested against the chute or the sled.
2. **The soap controls a virtual sled.** The prop is a tiller, not the sled. The sled is 14 × 8 × 6 mm, 0.220 kg, and its motion is integrated in `V`; the soap supplies a 20 mm slide and a wrist bank, not a 1:1 position.
3. **Palm-anchored serpentine.** The 900 mm chute is a fixed serpentine in the palm frame, folded into a 300 × 160 mm footprint. Its slope is a virtual 6° plus a ±4° palm-pitch trim, not the physical palm's true slope.
4. **No prop rigid body.** The soap is tracked, not simulated. A dropped soap pauses the heat and re-centres the sled; it never becomes a physics body on the table.

## 3. Physical Play Space & Ergonomics

The player sits with the left forearm supported and the left palm upturned at sternum height, 250–320 mm in front of the chest. The soap lies across the palm, long axis from thumb to pinky, with 15 mm overhanging each side. The right index and middle rest on the soap's top face; the right thumb may brace the near long edge. The left wrist is free to roll ±30° and pitch ±20°; the right hand slides the soap 20 mm along the palm's wrist-to-finger axis. The chute is drawn 10 mm above the palm, so the sled appears to run *through* the hand rather than on top of it.

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated, left forearm supported | Keeps the chute stable through a 1.2 s run |
| Palm centre | 250–320 mm forward, 20 mm below eye line | Comfortable near-focus for a 300 mm ribbon |
| Soap orientation | Long axis across the palm, `ŷ_V` | Leaves the 60 mm width free to slide along `x̂_V` |
| Slide range | ±20 mm | Fits the palm width without the soap leaving the hand |
| Wrist roll | ±30° usable, ±25° mapped | Full bank without forearm supination |
| Wrist pitch | ±20° usable, ±15° mapped | Slope trim of ±4° |
| Session | 5 descents × 3 heats; 8 s rests | Short, high-frequency wrist work |
| Break cue | Lower the palm overlay after descent 3 | Releases the wrist and thumb |

The right hand must not grip the soap: a grip adds a normal force that changes the slide friction and makes the brake non-linear. The right index and middle only *push* along `x̂_V`; the soap's own friction against the palm (µ ≈ 0.34) supplies the return force when the fingers relax. If the soap leaves the ±20 mm range, a soft rubber-band force pulls it back and a 120 ms haptic-substitute tone plays; the sled's control saturates at ±20 mm.

## 4. Prop Geometry & Registration

| Property | Value | Notes |
|---|---|---|
| Nominal soap | 90 × 60 × 30 mm | Oval bar, both broad faces convex |
| Fitted body spheroid | `a = b = 30.0 mm`, `c = 15.0 mm` | 60 × 60 × 30 mm; 15 mm end overhang each side |
| Mass | 110 g | Cured vegetable soap |
| Face curvature | `R_p = a²/c = 60.0 mm` | Top face, used only for depth and contact |
| Body inertia about `ẑ_P` | `I_z = 2ma²/5 = 3.96 × 10⁻⁵ kg·m²` | Not simulated; used for slide-filter tuning |
| Soap/palm friction | µ = 0.34 dry, 0.20 wet | Wet soap slides too freely; the game warns |
| Slide stiffness | 0.08 N/mm return | Palm friction plus a soft rubber-band recentre |
| Slide damping | 0.02 N·s/mm | Prevents a flick from overshooting the 20 mm stop |

**Surface-feature atlas (P-fixed; λ from `+x̂_P` toward `+ŷ_P`, φ from equator):**

| Feature | φ | λ | Extent | Purpose |
|---|---|---|---|---|
| Top face | +90° | any | 5 mm cap | Depth proxy and convex reference |
| Bottom face | −90° | any | lower cap | Palm contact |
| Long-axis end A | 0° | 0° | 8 mm | Thumb-side overhang |
| Long-axis end B | 0° | 180° | 8 mm | Pinky-side overhang |
| Left edge | 0° | +90° | 6 mm band | Right index contact |
| Right edge | 0° | −90° | 6 mm band | Right middle contact |
| Centre line | +90° | 0°–180° | 1 mm line | Slide zero reference |
| Witness +10 | +90° | +90° | 1 mm line | Accelerate mark |
| Witness −10 | +90° | −90° | 1 mm line | Brake mark |
| Witness ±20 | +90° | ±90° | 2 mm line | Saturation marks |
| Slide track | +60° to +90° | ±60° | 30 mm band | Right-finger push zone |

The atlas is used for registration and for the right-finger push classifier; the chute itself is not painted on the soap. The witness marks are 1 mm virtual lines that let the player see the slide without a numeric HUD.

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Spheroid | `a = b = 30.0`, `c = 15.0 mm` at `P` | Body and depth proxy |
| C1 | Capsule × 3 | r = 12 mm, lengths 20–40 mm | Left palm cradle |
| C2 | Capsule × 2 | r = 5 mm, lengths 16 mm | Right index/middle push |
| C3 | Box | 14 × 8 × 6 mm | Virtual sled |
| C4 | Ribbon | 900 mm arc, 48 mm wide, κ(s) | Chute surface (analytic) |
| C5 | Two walls | 2 mm high, offset ±24 mm | Rim collision |
| C6 | Three gates | 40 mm wide, at 300/600/900 mm | Split timing |
| C7 | Thumb brace | Capsule r = 6 mm | Left thumb on the near end |

Registration: the player presents the soap at 0.8–1.2 m, rolls it through 120° about `x̂_P`, and settles it across the left palm. The fitter uses the 90 mm silhouette for the long axis and the 60 × 30 mm profile for the body; acceptance requires RMS < 0.6 mm, long-axis error < 2.0°, and top-face normal within 12° of the palm normal. Re-acquisition searches a 130 mm cube for 1.2 s; success needs 8 rim samples with RMS < 0.9 mm over 6 frames. The palm frame is calibrated from `L_wrist`, `L_middle_mcp`, and `L_index_mcp`; if any joint is missing, the previous frame is held for 300 ms and the heat pauses.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long or axis) | Semantic output |
|---|---|---|---|---|
| I5.1 | Palm cradle | `L_palm`, `L_heel`, `L_middle_mcp` | lower cap, ≥ 3 contacts | `HOLD`; `V` anchor valid |
| I5.2 | Wrist roll | `q_P` about `x̂_V` | — | Bank angle `φ_b`, ±25° mapped |
| I5.3 | Wrist pitch | `q_P` about `ŷ_V` | — | Slope trim `θ_trim`, ±4° |
| I5.4 | Slide accelerate | `R_index_tip` + `R_middle_tip` push +`x̂_V` | slide track, φ +60° to +90° | `s > 0`, up to +20 mm |
| I5.5 | Slide brake | Same joints pull −`x̂_V` | same | `s < 0`, down to −20 mm |
| I5.6 | Thumb brace | `L_thumb_tip` on the near end | λ 0° ± 20°, φ 0° ± 20° | Stabilises the long axis, no control |
| I5.7 | Re-centre | Both hands cup the soap > 300 ms | any | Slide zero, sled reset |
| I5.8 | Start heat | Right index taps the top face twice | φ > +70° | Start run if state = READY |

**Per-finger roles.** The left palm carries the soap and defines `V`; the left thumb braces the near end and prevents the bar from yawing during a hard brake. The right index and middle are the only slide effectors; they must move together within 4 mm or the slide input is rejected as a twist. The right ring and pinky are ignored. A right-hand contact outside the slide track (φ < +50°) is a brace, not a control, and does not move the sled.

Contact gating: a slide contact is valid when signed distance < 5 mm, age > 30 ms, and the contact normal has a positive `ẑ_P` component. The slide is computed from the *soap's* position relative to the palm, not from the fingertip position alone, so a stationary finger on a sliding soap still reads the slide. Release hysteresis is 8 mm. A slide that changes sign faster than 40 mm/s is filtered to 40 mm/s to prevent a finger flick from reversing the sled in one frame.

## 6. Input Kinematics & Signal Processing

The palm frame is built each tick from the tracked joints:

`x̂_V = normalize(L_middle_mcp − L_wrist)`, `ẑ_V = normalize((L_index_mcp − L_wrist) × (L_pinky_mcp − L_wrist))`, `ŷ_V = ẑ_V × x̂_V`.

The soap slide is `s = clamp((p_P − p_palm)·x̂_V − s₀, −20 mm, +20 mm)`, where `s₀` is the calibration zero. A 2 mm dead zone and a 0.020 N·s/mm damping keep the resting soap from jittering. The control acceleration is

`a_ctrl = +1.4·(s/20 mm) m/s²` for `s ≥ 0`, and `a_ctrl = +2.5·(s/20 mm) m/s²` for `s < 0` (negative),

so a full tuck adds 1.4 m/s² and a full brake subtracts 2.5 m/s². The asymmetric gains make braking stronger than accelerating, which is what lets a 2.0 m/s sled shed speed before a hairpin without spinning.

The bank angle is `φ_b = asin(clamp(ŷ_V·ẑ_W, −1, 1))`, positive when the palm's thumb side is up. It is low-passed at 8 Hz and clamped to ±25°. The slope is `θ = 6° + 4°·(pitch/15°)`, where `pitch = asin(clamp(−x̂_V·ẑ_W, −1, 1))`. The sled's longitudinal dynamics are

`v̇ = g·sinθ − µ·g·cosθ − c·v + a_ctrl`,

with `g = 9.81 m/s²`, `µ = 0.02` (ice), and `c = 0.12 s⁻¹`. At neutral, `v̇ = 1.025 − 0.196 − 0.12v`, giving a terminal speed of 6.9 m/s; the 900 mm chute is short enough that the sled reaches 1.26 m/s, not terminal. At full tuck, `v̇ ≈ 2.23 m/s²` and the sled reaches 2.00 m/s; the run time drops from 0.99 s to 0.70 s.

The lateral dynamics are

`ÿ = v²·κ(s) − g·sinφ_b − c_lat·ẏ`, with `c_lat = 6.0 s⁻¹`,

where `κ(s)` is the chute curvature at the sled's arc position. The first term is the centrifugal demand, the second is the bank's gravity component, and the third is the ice's lateral damping. A hairpin with `κ = 2.4 m⁻¹` at `v = 1.3 m/s` demands `4.06 m/s²`; a 10° bank supplies `1.70 m/s²`, leaving `2.36 m/s²` outward, which the 6 s⁻¹ damping turns into a 0.39 m/s lateral drift. That is enough to reach the 24 mm rim in 62 ms if uncorrected, so the player must bank before the hairpin, not in it.

**Flip and rim.** The sled flips when `|v²κ − g·sinφ_b| > 6.5 m/s²` for 80 ms, or when `|φ_b| > 65°`. It hits the rim when `|y| > 24 mm`; a rim hit removes 35 % of speed, adds a 0.25 s scrape, and counts as a crash. Two crashes in a heat is a DNF. The 6.5 m/s² flip threshold is deliberately above the 4.06 m/s² hairpin demand, so a correctly banked hairpin never flips; only an over-speed or under-bank entry does.

**Split timing.** The sled's arc position `s_arc` is integrated from `v`; gates at 300, 600, and 900 mm latch the elapsed time with 1 ms resolution. The run clock starts when the sled crosses `s_arc = 0` with `v ≥ 0.55 m/s`, not when the player taps; this prevents a tap-to-start reaction-time advantage. The best of three heats is the descent time; the sector splits are diagnostic.

**Signal chain.** 72 Hz joints → One Euro (mincutoff 1.0 Hz, beta 0.020, derivative cutoff 7 Hz) → 90 Hz interpolation → palm frame → slide and bank → 8 Hz Butterworth on bank, 10 Hz on slide → sled integrator. The bank filter's 16 ms group delay is small compared with the 0.3 s hairpin entry, so predictive banking is not penalised.

## 7. Virtual World Layout & Coordinate Mapping

`V` is the left palm frame. The chute is a serpentine ribbon in the `x̂_V`–`ŷ_V` plane, 10 mm above the palm along `ẑ_V`, with a 900 mm arc length, 48 mm width, and three 300 mm sectors. Its centreline is

`y_c(s) = 38·sin(2πs/300) + 12·sin(2πs/150)` mm,

which gives two hairpins and a final straight while keeping the bounding box at 300 × 160 mm. Curvature is `κ(s) = |y_c''|/(1 + y_c'²)^(3/2)`, sampled into a 256-entry table at load; the peak is 2.42 m⁻¹ at the hairpin apex. The sled starts at `s = 0`, `y = 0`, with `v = 0.55 m/s`.

| Entity | V-space position | Size | Notes |
|---|---|---|---|
| Chute ribbon | `z = +10 mm`, `s ∈ [0, 900]` | 48 mm wide, 2 mm lip | Analytic surface |
| Sled | `(s, y, +12 mm)` | 14 × 8 × 6 mm | 0.220 kg |
| Sector gates | `s = 300, 600, 900` | 40 mm wide | Split latches |
| Hairpin 1 | `s ≈ 150` | κ = 2.42 m⁻¹ | Left-hand bend |
| Hairpin 2 | `s ≈ 450` | κ = 2.42 m⁻¹ | Right-hand bend |
| Final straight | `s ∈ [600, 900]` | κ < 0.3 m⁻¹ | Tuck finish |
| Rim walls | `y = ±24 mm` | 2 mm high | Collision |
| Bank indicator | above the chute | 28 mm arc | HUD, dry |
| Split board | player-facing | 40 × 24 mm | HUD, dry |

Mapping: the physical soap's long axis is `ŷ_V`, so a wrist roll about `x̂_V` banks the chute. A wrist pitch about `ŷ_V` changes the slope. The chute does not rotate with the soap's own yaw; only the palm frame matters, which is why the soap can be re-gripped without changing the course. The sled's screen position is `V_from_W` of the palm anchor plus the ribbon offset; the camera is the head pose, and the chute is drawn at 250–320 mm.

## 8. Gameplay Systems & Rules

**Entities.** One sled (0.220 kg, 14 × 8 × 6 mm), one 900 mm serpentine chute, three gates, two rim walls, five descents. Each descent has three heats; the best heat counts. A heat ends at `s = 900` or on a second crash.

**Descent structure.** Each descent uses a different chute phase and curvature scale, so the hairpins are in different places: descent 1 is the base serpentine; descent 2 adds a 1.1× curvature scale; descent 3 narrows the chute to 44 mm; descent 4 raises the initial speed to 0.70 m/s; descent 5 combines all three. The five descents are played in order, with an 8 s intermission between them.

**Longitudinal control.** The sled starts at 0.55 m/s (0.70 on descent 4+). Neutral acceleration is `1.025 − 0.196 − 0.12v`; full tuck adds 1.40 m/s²; full brake subtracts 2.50 m/s². The sled cannot reverse; a brake that would make `v < 0.15 m/s` holds at 0.15 m/s, because a stopped sled on a 6° chute is not a stable state in this game. The finish gate is at 900 mm; a run under the descent's gold time earns gold, silver is gold +0.10 s, and bronze is gold +0.25 s.

**Lateral control.** The bank angle is the palm roll, mapped 1:1 to ±25°. The lateral acceleration is `v²κ − g·sinφ_b − 6.0·ẏ`. The sled's lateral position is integrated at 90 Hz; the rim is at ±24 mm. A rim hit costs 35 % of speed, adds 0.25 s to the run, and counts as a crash. The sled can recover from a rim hit if it has not crashed twice; the speed loss usually ends the gold attempt but preserves the heat.

**Crash and DNF.** A crash is a rim hit or a flip. A flip occurs when the lateral load exceeds 6.5 m/s² for 80 ms or the bank exceeds 65°. On the first crash, the sled is placed back on the centreline at the last gate with 0.55 m/s and a 0.5 s penalty; on the second, the heat is a DNF. A descent with three DNF heats is lost, but the run continues to the next descent; losing two descents ends the run.

**Split times.** Each gate latches `t_split`; the sector time is the difference between consecutive splits. The HUD shows the current split against the player's best and against the gold target. A sector that is 0.10 s faster than the player's best lights the gate green; 0.10 s slower lights it amber. The split board is dry and player-facing, not spatialised, so it is readable at a glance.

**Score.**

`heat_score = max(0, 1000 − 500·(t − 0.80 s))` for a clean heat

`sector_bonus = 100` per sector under the gold split

`clean_bonus = 250` per heat with no crash

`descent_bonus = 500` for a gold best-of-three

`run_score = Σ heat_score + Σ sector_bonus + Σ clean_bonus + Σ descent_bonus`

The heat score is capped at 1000 for a 1.00 s run and floors at 0 at 3.50 s. The scoring rewards consistency across three heats, not a single lucky run.

**Win/lose.** Complete all five descents with at least three non-DNF descents to win. Two DNF descents end the run. A descent is DNF if all three heats crash twice; a single DNF descent is survivable, but it forfeits its descent bonus.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load chute table, start tracker | PRESENT |
| PRESENT | soap registered | RMS < 0.6 mm | Show palm ghost | CALIBRATE |
| CALIBRATE | soap across palm 1.0 s | long axis within 12° of `ŷ_V` | Set `s₀`, zero bank | DESCENT_INTRO |
| DESCENT_INTRO | timer 2.0 s | descent ≤ 5 | Show curvature map and gold time | READY |
| READY | double tap top face | `v = 0`, no crash | Arm run, start 0.5 s countdown | COUNTDOWN |
| COUNTDOWN | timer 0.5 s | — | Sled at start, bank live | RUN |
| RUN | tick | `v ≥ 0.15 m/s` | Integrate `v`, `y`, `s_arc`; latch splits | RUN |
| RUN | gate crossed | `s_arc ≥ 300/600/900` | Latch split, play gate cue | RUN |
| RUN | `s_arc ≥ 900` | not crashed twice | Finish, bank heat time | HEAT_END |
| RUN | rim hit | first crash | Speed −35 %, +0.25 s, re-centre at last gate | RUN |
| RUN | rim hit / flip | second crash | DNF heat, sled tumble | HEAT_END |
| RUN | bank > 65° | — | Flip, crash | RUN |
| HEAT_END | timer 2.0 s | heat < 3 | Show split, reset sled | READY |
| HEAT_END | heat = 3 | descent < 5 | Best of three, bank descent score | INTERMISSION |
| HEAT_END | heat = 3 | descent = 5 | Results | RESULTS |
| INTERMISSION | timer 8 s | — | Show next descent, rest wrist | DESCENT_INTRO |
| RESULTS | timer 0 s | DNF descents ≤ 1 | Win panel | RESULTS |
| RESULTS | timer 0 s | DNF descents ≥ 2 | Loss panel | RESULTS |
| ANY | tracker loss > 1.5 s | — | Pause run, freeze sled | RECOVERY |
| RECOVERY | re-acquired | RMS < 0.9 mm | Resume with 0.5 s grace | previous |

Timers run at 90 Hz; the run clock is wall-clock-latched at the start gate, and a frame stall cannot slow the sled. The countdown is fixed at 0.5 s so the player cannot tap-to-start with a reaction-time advantage. The physics clamps to two substeps and then advances the state machine by the elapsed time.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 soap | Spheroid 30/30/15 mm | Tracked `P` | Controller and depth proxy |
| C3 sled | Box 14 × 8 × 6 mm | Integrated in `V` | 0.220 kg |
| C4 chute | Ribbon 900 × 48 mm | Static in `V` | Analytic height and curvature |
| C5 rim | Two walls at ±24 mm | Static | Collision at `|y| = 24 mm` |
| C6 gates | Three planes | Static | Split latch |
| C1 palm | 3 capsules | Tracked | `V` anchor |
| C2 fingers | 2 capsules | Tracked | Slide input |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C3 sled × C4 chute | `0 ≤ s_arc ≤ 900`, `|y| ≤ 24` | Supported; apply slope, friction, curvature |
| C3 sled × C5 rim | `|y| > 24 mm` | Speed ×0.65, +0.25 s, crash counter++ |
| C3 sled × C6 gate | `s_arc` crosses 300/600/900 | Latch split, 1 ms resolution |
| C3 sled × C3 | — | Single body, no self-collision |
| C1 palm × C0 | signed distance < 5 mm | Cradle validity |
| C2 finger × C0 | signed distance < 5 mm | Slide control |
| C0 × C4 | — | The soap is not the sled; no collision |

**Materials.** Ice = "black ice": µ = 0.02, restitution 0.0, lateral damping 6.0 s⁻¹. Sled = "steel": no deformation, 0.220 kg. Rim = "snow bank": restitution 0.05, absorbs 35 % of speed, spawns 20 snow particles. Soap = "grip": µ = 0.34 against skin; not part of the sled physics.

**CCD.** The sled moves at most 22.2 mm/tick at 2.00 m/s, which is larger than the 8 mm sled width and the 2 mm rim, so the rim test uses a swept segment from the previous to the current `y` and the gate test uses a swept `s_arc` crossing. The lateral integration is substepped at 180 Hz when `|v²κ| > 4.0 m/s²`, because the hairpin load changes quickly and a 90 Hz explicit integrator can overshoot the rim. The longitudinal integrator is stable at 90 Hz because `c = 0.12 s⁻¹` and the acceleration is bounded.

**Forces.** Longitudinal: gravity along the 6° slope (1.025 m/s²), ice friction (0.196 m/s²), linear drag (0.12v), tuck (+1.40 m/s²), brake (−2.50 m/s²). Lateral: centrifugal `v²κ` (up to 9.7 m/s² at 2.00 m/s and κ = 2.42), bank `g·sinφ_b` (up to 4.15 m/s² at 25°), and damping `6.0·ẏ`. The sled's normal force is `m·g·cosθ ≈ 2.15 N`; the lateral friction limit is `µ_lat·N = 0.08 × 2.15 = 0.172 N`, which is why the sled cannot simply grip the ice through a hairpin and must be banked. The rim collision is a speed multiplier, not an impulse, because the sled is a game object, not a rigid body with a contact solver.

## 11. Rendering, Occlusion & Compositing

Layer order, near to far: passthrough base → left palm scene mesh → chute ribbon → rim walls → sled → speed lines → right-hand mesh → HUD split board and bank arc. The chute is drawn 10 mm above the palm and is depth-tested against the hand; the sled is drawn 2 mm above the ribbon, so it never z-fights with the ice.

**Occlusion cases.**

- **O1 — Chute over the palm.** The ribbon is a thin translucent surface with a 0.6 alpha ice shader. The palm is visible through it, which sells the "flowing through the hand" fantasy; the rim walls are opaque and hide the palm at the edges.
- **O2 — Right fingers over the sled.** The right index and middle are drawn in front of the sled when they are on the near side of the chute. The sled is clipped by the finger mesh, so a finger passing over it hides it briefly; the split board duplicates the speed readout so the player does not lose the run.
- **O3 — Sled behind a hairpin wall.** The rim walls are 2 mm high and opaque; a sled on the far side of a banked hairpin is partially hidden by the near wall. This is correct and is why the chute is banked: the player sees the sled's rooster tail over the wall.
- **O4 — Soap under the chute.** The soap is below the ribbon by 10 mm; its depth proxy still occludes the palm and any virtual ice that passes behind it. The soap's convex top face is drawn with a 0.3 mm frost rim so it reads as the tiller even when the chute covers it.
- **O5 — HUD versus hand.** The split board is rendered at 240 mm from the eye and depth-tested against the right hand. A raised finger covers it; the current split is duplicated as a 2 mm strip along the chute's near rim.
- **O6 — Speed lines versus passthrough.** Speed lines are additive and are clipped by the hand mesh and the chute rim. They never draw over the real soap's silhouette, because a line through the prop would break the depth cue that the soap is the controller.

**VFX budgets.** Chute ribbon: 900 mm × 48 mm, 2.4 k triangles, 0.7 ms. Ice shader: one scrolling normal map, 0.4 ms. Sled: 240 triangles, 1 draw. Speed lines: 48 quads, 0.3 ms. Snow spray: 60 particles per rim hit, max 2 concurrent. Split sparks: 12 per gate. Bank arc: 1 translucent arc, 32 segments. Total transparent overdraw target < 1.5× screen area. The chute is the only transparent surface over passthrough; the sled and rim are opaque, so the player's hand remains the visual anchor.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Sled start | chute start | 120 Hz whoosh + 800 Hz click, 200 ms | Sled drops, gate flashes |
| Ice scrape | sled | Continuous granular scrape, 400–1600 Hz, gain ∝ `v` | 12 speed lines, frost trail |
| Tuck | chute | 200→500 Hz wind swell, 180 ms | Sled nose drops, drag lines shorten |
| Brake | chute | 1.2 kHz metallic hiss, 150 ms | Sled nose lifts, snow spray |
| Bank into hairpin | chute | 300 Hz low whoosh, gain ∝ `|φ_b|` | Chute tilts, rim glow |
| Gate split | gate | 1.5 kHz beep, 60 ms | Split number snaps, gate green/amber |
| Rim hit | rim | 90 Hz thud + 600 Hz snow crunch, 300 ms | Snow burst, speed bar drops |
| Flip | sled | 70 Hz boom + 1.8 kHz tumble, 500 ms | Sled spins, screen dip |
| Heat finish | gate | 1.1 kHz chime + 220 Hz gong, 500 ms | Time board stamps, medal preview |
| Gold split | HUD | Rising major third, 300 ms | Split board gold |
| Descent clear | HUD | Crowd swell + arpeggio, 900 ms | Five-ring progress lights |
| DNF | HUD | Descending minor third, 700 ms | Sled greyed, "DNF" stamp |
| Tracker loss | head-locked | 180 Hz muted pulse | "Show the soap" prompt |

World cues are spatialised with a 1.0 m rolloff; HUD cues are dry. The ice scrape is the primary speed cue: its brightness tracks `v`, and its stereo width tracks the sled's lateral offset, so the player can hear a drift toward the rim before seeing it. There is no haptic hardware; the 90 Hz rim thud and the 70 Hz flip boom are the substitutes for the physical jolt of a crash.

## 13. Difficulty Progression & Tuning

| Descent | Curvature scale | Chute width | Initial speed | Gold time | Heats | Notes |
|---|---|---|---|---|---|---|
| 1 | 0.85× | 52 mm | 0.55 m/s | 1.05 s | 3 | Teaching: gentle hairpins |
| 2 | 1.00× | 48 mm | 0.55 m/s | 0.98 s | 3 | Baseline serpentine |
| 3 | 1.10× | 44 mm | 0.60 m/s | 0.92 s | 3 | Narrower rim, tighter line |
| 4 | 1.20× | 42 mm | 0.70 m/s | 0.86 s | 3 | Faster start, less setup |
| 5 | 1.30× | 40 mm | 0.75 m/s | 0.80 s | 3 | Finale: all modifiers |

Named constants: `M_SLED = 0.220 kg`, `V0 = 0.55 m/s`, `SLOPE = 6°`, `MU_ICE = 0.02`, `DRAG = 0.12 s⁻¹`, `TUCK_A = 1.40 m/s²`, `BRAKE_A = 2.50 m/s²`, `BANK_MAX = 25°`, `KAPPA_MAX = 2.42 m⁻¹`, `WIDTH = 48 mm`, `HALF_WIDTH = 24 mm`, `FLIP_LOAD = 6.5 m/s²`, `FLIP_TIME = 0.080 s`, `BANK_FLIP = 65°`, `LATERAL_DAMP = 6.0 s⁻¹`, `RIM_SPEED_LOSS = 0.35`, `RIM_PENALTY = 0.25 s`, `SECTORS = 3`, `DESCENTS = 5`, `HEATS = 3`.

Adaptive rule: after each descent, if the player's best heat is under the gold time and no heat crashed, raise the curvature scale by 0.05 (cap 1.35) and reduce the width by 2 mm (floor 38 mm) for the next descent. If the player DNFs or the best heat is over 1.80 s, lower the curvature scale by 0.05 (floor 0.80) and add 3 mm of width (cap 54 mm). Adaptation never changes `M_SLED`, `TUCK_A`, `BRAKE_A`, or `BANK_MAX`, because the physical feel of the soap must stay constant.

## 14. Edge Cases & Failure Recovery

- **Soap slides out of the ±20 mm range.** The control saturates and a 0.08 N/mm rubber-band force pulls the soap back toward zero; the sled does not receive more than the maximum tuck or brake. A 120 ms elastic tone plays once per saturation episode. If the soap leaves the palm entirely, the heat pauses and the sled freezes at its current `s_arc` for up to 3.0 s.
- **Soap yaws in the palm.** The long axis can rotate ±15° inside the grip. The palm frame is built from the hand, not the soap, so the chute does not yaw; only the slide projection is corrected by `cos(15°) = 0.966`. Beyond 25° of soap yaw, the slide input is rejected and a "straighten the bar" cue plays.
- **Wrist roll beyond 30°.** The bank maps only ±25°; the excess is clamped, and a 200 Hz warning tone plays at 28° so the player knows the bank is saturated. A physical wrist beyond 45° triggers a 2 s ergonomic prompt and the run continues at the clamp.
- **Palm frame jitter.** The palm frame is filtered at 8 Hz. If the frame's angular velocity exceeds 200°/s (a fast re-grip), the chute is frozen in its last valid orientation for 120 ms rather than snapping; the sled continues on its current heading.
- **Tracking loss.** The soap pose freezes after 200 ms, the bank and slide hold their last values, and the sled coasts for 0.5 s; then the run pauses. Re-acquisition restores 0.5 s of grace. If loss exceeds 2.0 s, the heat restarts from the last gate with the same speed, so a tracker dropout does not cost a gold time.
- **Rim hit at the finish line.** If the sled crosses `s = 900` in the same tick as a rim hit, the finish wins: the gate is latched before the crash test. This 1-tick priority prevents a visible finish from being scored as a DNF.
- **Flip at low speed.** The flip test requires `v > 0.40 m/s`; below that, a large bank just slides the sled to the rim, because a slow sled cannot generate enough centrifugal load to flip. This prevents a stationary soap wobble from ending a heat.
- **Wet soap.** A wet palm lowers friction to 0.20 and makes the slide overshoot. The game detects a 20 % drop in slide damping and reduces the control gain to 0.75×, with a "dry your hands" prompt. It does not pause, because the soap is still trackable.
- **Left-handed player.** Mirror mode swaps the chute to the right palm, the slide fingers to the left hand, and mirrors `y_c(s)` about `y = 0`; the bank sign flips. The physics is unchanged because it is computed in the palm frame.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — registers the soap, emits `PoseFrame {p_P, q_P, confidence, mode}` at 72 Hz.
- `ContactBus` — 26 joints → `Contact {joint, p_P, n_P, v_rel, age}` with 5 mm enter / 8 mm exit and 30 ms debounce.
- `PalmFrame` — builds `x̂_V, ŷ_V, ẑ_V` from `L_wrist`, `L_middle_mcp`, `L_index_mcp`, `L_pinky_mcp`; filters at 8 Hz.
- `SlideControl` — projects the soap onto `x̂_V`, dead zone, clamp, saturation, rubber-band return.
- `BankControl` — roll/pitch extraction, ±25° clamp, 2° warning.
- `ChuteModel` — 256-entry curvature table, arc length, gate positions, rim width.
- `SledBody` — `v`, `y`, `s_arc`; longitudinal and lateral integrators; flip and rim tests.
- `SplitTimer` — 1 ms gate latches, sector and total times, best-of-three.
- `DescentStateMachine` — the table in §9.
- `ScoreCombo` — heat, sector, clean, descent, and run scores.
- `FeedbackRouter` — every event enum to one audio clip and one VFX prefab.
- `Compositor` — layer order, spheroid depth proxy, occlusion masks O1–O6.

**Event flow.** `PropTracker` → `PoseFrame`; `ContactBus` → `PalmFrame` and `SlideControl`; `PalmFrame` → `BankControl` and `ChuteModel`; `SlideControl` and `BankControl` → `SledBody`; `SledBody` → `SplitTimer`, `DescentStateMachine`, `FeedbackRouter`; `DescentStateMachine` → `FeedbackRouter`. The state machine reads only `SledBody.v`, `SledBody.s_arc`, `SplitTimer`, and the crash count; it never reads raw joints.

**Pseudocode 1 — palm frame and controls.**

```
function tick_controls(dt):
    palm = PalmFrame.from_joints(L_wrist, L_middle_mcp, L_index_mcp, L_pinky_mcp)
    if palm == null:
        return last_valid_controls

    s_raw = dot(pose.p_P - palm.centre, palm.x_hat) - s_zero
    s = clamp(s_raw, -20mm, +20mm)
    s = deadzone(s, 2mm)
    s = lowpass(s, 10 Hz, dt)

    phi_b = asin(clamp(dot(palm.y_hat, WORLD_UP), -1, 1))
    phi_b = lowpass(clamp(phi_b, -25deg, +25deg), 8 Hz, dt)
    pitch = asin(clamp(-dot(palm.x_hat, WORLD_UP), -1, 1))
    theta = 6deg + 4deg * (clamp(pitch, -15deg, +15deg) / 15deg)

    a_ctrl = (1.40 m/s^2) * (s/20mm) if s >= 0 else (2.50 m/s^2) * (s/20mm)
    return {s, phi_b, theta, a_ctrl}
```

**Pseudocode 2 — sled integration and gate timing.**

```
function tick_sled(dt):
    c = controls
    a_long = 9.81*sin(c.theta) - 0.02*9.81*cos(c.theta) - 0.12*v + c.a_ctrl
    v = max(0.15 m/s, v + a_long*dt)
    s_arc += v*dt

    kappa = ChuteModel.curvature(s_arc)
    a_lat = v*v*kappa - 9.81*sin(c.phi_b) - 6.0*y_dot
    y_dot += a_lat*dt
    y += y_dot*dt

    if abs(a_lat) > 6.5 m/s^2:
        flip_timer += dt
        if flip_timer > 0.080s: crash(FLIP)
    else:
        flip_timer = 0

    if abs(y) > 24mm:
        y = sign(y)*24mm; y_dot = 0
        v *= 0.65; penalty += 0.25s
        crash(RIM)
        FeedbackRouter.play(RIM, sled_p)

    for gate in [300mm, 600mm, 900mm]:
        if not gate.latched and s_arc >= gate.s:
            gate.latched = true
            SplitTimer.latch(gate, now)
            FeedbackRouter.play(SPLIT, gate.p)
```

**Determinism.** Chute phases, descent modifiers, and initial speeds are pure functions of `(session_id, descent)`. The sled integrator uses a fixed 90 Hz tick with 180 Hz substeps inside high-curvature segments, so a recorded control trace replays bit-identically.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Soap registration | Present at 1.0 m; inject 24 rim points with 0.3 mm noise | RMS < 0.6 mm; long-axis error < 2.0°; top normal within 12° of palm normal |
| AT-02 | Palm frame | Inject known wrist/MCP joints at 0°, 15°, 30° roll | `x̂_V, ŷ_V, ẑ_V` orthonormal within 0.5°; bank sign correct |
| AT-03 | Slide mapping | Move the soap from −25 to +25 mm along `x̂_V` | Output clamps at ±20 mm; dead zone 2 mm; 10 Hz filter settles in 0.18 s |
| AT-04 | Neutral run | No slide, no bank, descent 2 | Finish 0.99 ± 0.03 s; speed at 900 mm 1.26 ± 0.05 m/s |
| AT-05 | Full tuck | `s = +20 mm`, no bank | Finish 0.70 ± 0.03 s; speed at 900 mm 2.00 ± 0.08 m/s |
| AT-06 | Full brake | `s = −20 mm` from 1.26 m/s | Stops (v ≤ 0.15 m/s) in 0.49 ± 0.05 m; does not reverse |
| AT-07 | Bank gravity | 10° bank at rest | Lateral acceleration −1.70 ± 0.05 m/s²; sign toward the thumb side |
| AT-08 | Hairpin load | `v = 1.3 m/s`, κ = 2.42, 10° bank | `a_lat = 2.36 ± 0.10 m/s²`; no flip within 80 ms |
| AT-09 | Flip threshold | `v = 2.1 m/s`, κ = 2.42, 0° bank | Flip at 6.5 m/s² ± 0.1; crash event at 80 ± 5 ms |
| AT-10 | Rim collision | Drive `y` to 25 mm | `y` clamps to 24 mm; speed ×0.65; +0.25 s; one crash |
| AT-11 | Split timing | Cross gates at known times | Each split latched to 1 ms; sector sums equal total ± 1 ms |
| AT-12 | Best of three | Run 1.40, 1.32, 1.55 s | Best = 1.32 s; descent score uses 1.32; heat scores all banked |
| AT-13 | Occlusion readback | Place the sled behind the right hand; read depth buffer | Sled fragment contribution = 0 under the hand silhouette; split board remains readable |
| AT-14 | Tracking loss | Drop `PoseFrame` for 2.0 s during RUN | Controls freeze after 200 ms; sled coasts 0.5 s; pause; restart at last gate with same speed |

All tests run headless with synthetic pose/contact streams. AT-13 is a depth-buffer readback, not a screenshot. AT-04 through AT-09 use the shipped 256-entry curvature table and the exact constants in §13, so a tuning change that alters the gold times fails the tests rather than silently changing the feel.
