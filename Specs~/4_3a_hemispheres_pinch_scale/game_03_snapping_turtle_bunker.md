# Snapping Turtle Bunker

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Snapping Turtle Bunker |
| Slug | `snapping_turtle_bunker` |
| Physical prop | Half walnut shell, dome-up; `R = 22.0 mm`, `t = 1.8 mm`, mass `4.0 g`; matte brown exterior, unfinished interior |
| Family / sub-shape | Hemisphere, dome-up, pinch scale |
| Collider family | Convex hemisphere + rim torus + apex press sensor |
| Scale class | Prop 1:1 (44 mm aperture); virtual arena 240 mm × 180 mm |
| Source concept quote | "Rest your index fingertip on the apex of the walnut dome on a table; press down on the front, left, or right rim so the opposite edge tilts up by 3mm... Tilting a rim edge pops the turtle's head out from under that side of the shell to chomp passing virtual beetles, while pressing the center apex pulls the turtle safely inside during aerial bombardments." |
| Physical input | Index fingertip on apex (retract), thumb/index press on rim at azimuth `λ_c` (tilt + bite) |
| Session length | 7 min (5 tides × 60 s + onboarding + score) |
| Lose condition | Shell integrity reaches 0 after three unblocked bombardments |

## 1. Fantasy & Core Loop

A rain-wet riverbank is rendered at 1:10 scale on the tabletop. The walnut shell is the turtle's carapace: a dark, ridged dome with moss along its seams and a warm amber glow leaking from the rim. Beneath it lives a miniature alligator snapping turtle with a beak like a chisel and a neck that unfolds sideways, not forward. Beetles the size of peppercorns crawl in from the arena edge on four radial lanes. The player cannot move the shell freely — it stays anchored — but rim pressure tilts it about the contact tangent, lifting the far edge 3 mm and letting the turtle's head dart out beneath the raised side to snap. When acorn bombardments whistle down, the player must press the apex and seal the shell flat before impact.

1. **Read the lane (1.2 s).** A beetle spawns at one of four rim azimuths (0°, 90°, 180°, 270°) and begins a 45 mm radial crawl toward the shell, antenna twitching.
2. **Choose the tilt (0.2 s).** The player presses the rim at the azimuth opposite the beetle. Contact point azimuth `λ_c`, tilt axis tangent at `λ_c`, raised side at `λ_c + 180°`.
3. **Tilt and bite (0.35 s).** The shell tilts to `θ ≈ 3.9°`; the head emerges beneath the raised edge, strikes along the raised-side radius, and retracts. Bite window is 140 ms.
4. **Seal (0.15 s).** Releasing the rim lets the shell fall back under gravity with restitution 0.15; the turtle retracts if the apex is pressed.
5. **Bombardment (2.0 s).** An acorn telegraph ring pulses for 700 ms at a predicted impact point; apex press must hold for 150 ms before impact.
6. **Tide resolve (3.0 s).** Surviving beetles score, integrity is tallied, and the river rises for the next tide.

A beetle cycle is 1.7–2.4 s; a tide is 60 s; five tides plus onboarding and two retries fill the 7 min session.

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

- **Radius and wall**: `R = 22.0 mm`, `t = 1.8 mm`, inner radius `R_in = 20.2 mm`, mass `m = 4.0 g`.
- **Orientation**: dome-up. Convex exterior faces `+ẑ_P`, cavity opens toward `−ẑ_P`. Apex `A_P = (0, 0, +22.0) mm`, rim circle at `z_P = 0`.
- **P origin**: rim-circle centre. At rest `P_O = (0, 0, 0)` in `V`, and the shell is always table-anchored (no free translation).
- **Outer surface**: `p_P = R·(sinθ cosλ, sinθ sinλ, cosθ)`, `θ ∈ [0°, 90°]`; inner surface with `R_in`; rim torus major `R = 22.0 mm`, minor `t/2 = 0.9 mm` in `z_P = 0`.
- **Tilt model**: the shell rotates about the tangent line at the rim contact point `c_P = (R cosλ_c, R sinλ_c, 0)`. Rotation axis `â = (−sinλ_c, cosλ_c, 0)`. Tilt angle `θ_t` from the opposite-edge lift `h` via `θ_t = asin(h / (2R))`; at `h = 3 mm`, `θ_t = 3.91°`.
- **V-frame**: anchored to `T`; origin at the arena centre `Q`; axes `x̂_V = x̂_T`, `ŷ_V = ŷ_T`, `ẑ_V = ẑ_T`. `P` is fixed at `Q` in translation and only rotates.
- **Deviation 1**: translation is disabled — the shell is a turret, not a vehicle. Rim contacts only produce torque about the tangent axis, clamped to `θ_t ∈ [0°, 8°]`.
- **Deviation 2**: the turtle head is a virtual articulated chain (neck 2 bones + beak) that is *never* physical; it is a render-only entity with a bite trigger volume.
- **Deviation 3**: acorn bombs are virtual only and are excluded from the hemisphere collider; they interact with a 26 mm-radius spherical "seal test" volume centred at `P_O + 18 mm·ẑ_V`.
- **Deviation 4**: apex press is read from `R_index_tip` (or `L_index_tip`), not the rim; it uses a separate 18 mm press plate collider at the apex.

## 3. Physical Play Space & Ergonomics

The player sits with the shell centred 200 mm in front of the sternum. The right index rests lightly on the apex; the right thumb and the index of the left hand press the rim on opposite sides so the tilt is a squeeze, not a shove. This two-hand arrangement halves the per-finger force and keeps the wrist near neutral. The head tilts 20–28° down.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Table `T`, 720–780 mm high | Shoulder flexion < 30° |
| Arena | 240 mm × 180 mm centred on `Q` | Four beetle lanes + 40 mm outer margin |
| Rim contact azimuths | quantized to 4 sectors ± 22.5° | Classifier robustness |
| Rim press force | 0.3–0.8 N for tilt; 1.2 N max | Below 1.5 N pinch fatigue |
| Apex press force | 0.35–0.9 N | Retract trigger |
| Tilt hold | ≤ 0.8 s per bite | Wrist deviation < 15° |
| Gaze | 20–28° down, 320–460 mm | Avoids neck flexion > 30° |
| Session | 7 min; 60 s tides; 15 s rest | Sustained-press stamina |

Fatigue mitigation: the rest state is a flat shell with no force required, so the hands can open between beetles; the apex is a rest perch for the index; each tide ends with a 15 s hands-open interlude; a "press load" ring appears at the apex after 40 s of cumulative contact.

## 4. Prop Geometry & Registration

The walnut half is chosen for a near-perfect hemispherical dome (`R = 22.0 ± 0.5 mm`) and a rim that is flat to ±0.3 mm so tilt is visually legible.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 22.0 mm ± 0.5 | Silhouette least-squares |
| Wall `t` | 1.8 mm ± 0.3 | Rim step |
| Inner radius `R_in` | 20.2 mm | `R − t` |
| Rim plane flatness | ≤ 0.3 mm | Tilt threshold calibration |
| Mass `m` | 4.0 g ± 0.6 | Balance scale |
| Apex press plate | 8 mm diameter, centred on `A_P` | Force-sensor region |
| Registration | 4-point rim fit, 30 frames | Initial |
| Re-acquisition | `ε_R < 1.5 mm`, `ε_θ < 3°` | After occlusion |

Surface feature atlas (used for visual anchoring and bite aim):

| Station | `θ` | `λ` range | Feature |
|---|---|---|---|
| S0 | 90° | any | Apex press plate |
| S1 | 60–85° | any | Dome ridges (6 great-circle seams) |
| S2 | 20–60° | any | Moss band |
| S3 | 0–20° | any | Rim countersink + amber seam |
| S4 | 0° | all | Rim torus (contact band) |

Colliders: (a) outer hemisphere triangle mesh decimated to 512 tris at `R`; (b) rim torus analytic; (c) apex plate disc at `z_P = R`, radius 4 mm; (d) inner cavity is a sensor-only concave hemisphere at `R_in`. Registration publishes `P` in `W` at 72 Hz with 11 ms latency.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `R_index_tip` | S0 apex plate | Press ≥ 0.35 N, 150 ms | `SealRequest` |
| I5.2 | `R_thumb_tip` + `L_index_tip` | S4 rim, opposite sectors | Sustained press ≤ 0.8 s | `TiltHold(λ_c)` |
| I5.3 | `R_thumb_tip` | S4 rim | Single 0.3 N tap | `TiltTap(λ_c)` |
| I5.4 | `R_index_tip` | S3 rim inner edge | Quick 120 ms tap | `PeekTilt(λ_c)` |
| I5.5 | both hands open | — | 400 ms no contact | `RestState` |
| I5.6 | `L_thumb_tip` | S4 rim | Press + slide 6 mm | `AimAdjust(Δλ_c)` |

Contact azimuth `λ_c = atan2(y_c, x_c)` of the contact point in `P`; sector index `s = round(λ_c / 90°) mod 4`. The bite direction is the opposite sector: `λ_bite = λ_c + 180°`.

## 6. Input Kinematics & Signal Processing

- **Rim normal force estimate**: `F_n = k_p·max(0, d_pen)` with `k_p = 900 N/m` and penetration `d_pen < 2 mm`; low-pass `α = 0.22` at 72 Hz (≈ 1.9 Hz cutoff).
- **Tilt angle**: target `θ_t* = clamp(asin(h*/(2R)), 0, 8°)`, where `h* = 0.35·F_n` mm/N (0.3 N → 1.05 mm, 0.8 N → 2.8 mm). Physical tilt follows a second-order system `θ̈_t = (θ_t* − θ_t)·ω_n² − 2ζω_n θ̇_t`, `ω_n = 22 rad/s`, `ζ = 0.55`.
- **Sector debounce**: a sector is committed after 30 ms of stable `s`; a sector change mid-hold is rejected unless `|Δλ_c| > 30°`.
- **Apex seal**: `SealRequest` requires `F_apex ≥ 0.35 N` for 150 ms; hysteresis release at 0.2 N.
- **Peek/tilt tap classifier**: contact dwell 90–160 ms and `θ_t ≥ 2.5°` ⇒ `TiltTap`; dwell > 250 ms ⇒ `TiltHold`.
- **Bite trigger**: fires when `θ_t ≥ 3.0°` and the head tip crosses `r = R + 3 mm` at `λ_bite`; refractory 260 ms.
- **Hand loss**: if the apex or rim contact is lost > 120 ms during a hold, the hold decays with `τ = 180 ms` instead of snapping shut.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 240 × 180 mm riverbank plane at `z_V = 0`. The shell sits at `Q = (0,0,0)`. Four beetle lanes run radially at `λ = 0°, 90°, 180°, 270°`, spawning at `r = 95 mm` and terminating at `r = 24 mm` (just outside the rim). The waterline is a shader band at `y_V = −60 mm`. Acorn bombardments originate at `z_V = +180 mm` and target a ground-projected impact point `I_V`.

| Entity | Anchoring | Coordinates |
|---|---|---|
| Shell | `P` fixed at `Q`, rotation only | `P_O = Q` |
| Beetles | `T`-plane | polar `(r, λ)` |
| Acorns | `V` world | ballistic in `(x_V, y_V, z_V)` |
| Turtle head | `P` (rotates with shell) | local chain from `(0,0,−4)` |
| Bite volume | `P` + `θ_t` | sphere `r = 7 mm` at raised edge |
| Arena FX | `V` | bounded by `r = 120 mm` |

Because the shell does not translate, `P` and `V` differ only by the tilt rotation `R_x(θ_t)·R_z(λ_c)` about the contact tangent.

## 8. Gameplay Systems & Rules

**Beetles.** Each beetle has `hp = 1`, speed `v_b ∈ [12, 40] mm/s`, and a lane. A bite kills if the beetle is inside the bite volume at strike time: `|p_b − p_bite| ≤ 7 mm` and the beetle's azimuth is within ±18° of `λ_bite`.

**Bite score.** `S_bite = 100 · (1 + 0.5·n_chain) · (1 + 0.25·perfect)`, where `n_chain` is consecutive bites without a miss (cap 8), and `perfect = 1` if the beetle was killed within 40 mm of the shell. A miss resets the chain and costs 1 integrity point.

**Bombardment.** Every 9–14 s a volley of 1–3 acorns is telegraphed 700 ms early with a red impact ring of radius `12 mm`. Acorns fall at `v_z = −900 mm/s`. If the apex is sealed (`F_apex ≥ 0.35 N` for 150 ms) at impact, the shell absorbs the hit (integrity −0, copper spark VFX). Otherwise integrity −1 and the turtle is stunned 1.2 s (no bites).

**Integrity.** Starts at 3. Hits remove 1. At 0 the session ends.

**Tide clear.** A tide clears when its 60 s timer expires and integrity > 0. Clearing awards `+250·(integrity/3)` and unlocks the next tide.

**Win condition.** Clear all 5 tides. Final score adds a `+500 · min(3, remaining_integrity)` bonus.

**Combos.** Three bites within 4 s = "Snap Frenzy" (beetles slow 25% for 2 s). Five sealed bombardments in a row = "Iron Shell" (next hit ignored).

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | session start | `Calibrate` | 1.0 s | prop detected |
| `Calibrate` | registration | `TideIntro` | 2.5 s or 4-point fit | `P` locked |
| `TideIntro` | calibration / tide clear | `Hunt` | 2.0 s | banner |
| `Hunt` | tide intro | `Bite` / `Bombard` | beetle present | lane audio |
| `Bite` | tilt ≥ 3° | `Hunt` | 0.35 s | bite VFX |
| `Bombard` | volley scheduled | `SealCheck` | 700 ms telegraph | ring pulse |
| `SealCheck` | impact −150 ms | `Hunt` / `Stun` | 150 ms | absorb or hit |
| `Stun` | unsealed impact | `Hunt` | 1.2 s | shell shake |
| `TideClear` | 60 s + integrity > 0 | `TideIntro` / `Victory` | 3.0 s | score tally |
| `Victory` | tide 5 cleared | `Score` | 2.0 s | fanfare |
| `Score` | victory / integrity 0 | `Boot` | 6.0 s or grab | card |
| `Pause` | palm cover 1.0 s | previous | — | dim |

Transitions are event-driven; timers run on the fixed 90 Hz physics tick, so a 150 ms seal is exactly 14 ticks (13.5 → 14).

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Shell outer hemisphere | Table `T` | Resting support, restitution 0.15 | walnut/wood |
| Shell rim torus | Table `T` | Tilt pivot contact | µ = 0.32 |
| Apex plate | `R_index_tip` | Force estimate only | soft sensor |
| Bite volume | Beetle capsule | Kill / no-kill | trigger |
| Acorn sphere | Seal test sphere | Absorb / hit | trigger |
| Acorn sphere | Arena floor | Bounce 0.35, despawn 1.5 s | chitin |

Tilt dynamics are a 1-DoF rigid body: `I_t = 0.42·m·R² = 0.42·0.004·(0.022)² = 8.1×10⁻⁷ kg·m²`. Restoring torque from gravity when tilted: `τ_g = m·g·d_com·sinθ_t`, with `d_com = 0.38R = 8.4 mm`. Damping `c_t = 3.2×10⁻⁵ N·m·s/rad`. The shell is clamped to `θ_t ≤ 8°` to avoid ever tipping over.

CCD: acorns at 900 mm/s move 10 mm/tick at 90 Hz, so a 20 mm/tick sweep + 3 substep test is required; the seal test volume `r = 26 mm` guarantees no tunnelling.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → scene mesh → virtual ground/water → beetles → turtle head → shell exterior shell-shader → rim glow → hand mesh → near-field VFX → UI.

Occlusion cases:
1. **Fingertip on apex**: the index tip renders in front of the shell; the apex plate glow is clipped by the hand depth mask.
2. **Finger behind rim**: the far-side finger is correctly hidden by the shell silhouette (stencil from the hemisphere proxy).
3. **Head under raised edge**: the head is drawn with the shell's depth so it only appears through the 3 mm gap; the gap widens visually to 6 mm for legibility.
4. **Acorn behind shell**: an acorn passing behind the dome is occluded by the shell depth, then re-emerges.
5. **Holographic UI**: tide banner renders at `z_V = +120 mm` and is soft-occluded by the hand.
6. **Bite volume**: never rendered as geometry, only as an emissive arc that is depth-tested against the hand.

VFX budgets: shell tilt ≤ 2 ms, head chain 1.5 k tris, beetles 12 × 400 tris, acorns 6 × 300 tris, particles ≤ 600 live.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Rim press | Wood tick, 1.2 kHz | Rim glow at `λ_c` |
| Tilt to 3° | Low creak | Opposite edge highlight |
| Bite hit | Chitin crunch + snap | Amber bite flash |
| Bite miss | Air snap | Dust puff |
| Beetle spawn | Chirp, pan by lane | Lane shimmer |
| Bombard telegraph | Rising whistle | Red impact ring |
| Sealed hit | Copper gong | Spark shower |
| Unsealed hit | Crack + shell rattle | Screen shake 3 mm |
| Tide clear | River swell | Water rises |
| Victory | Shell horn motif | Gold dome pulse |

All cues are spatialized at the entity position; the bite cue uses the `P` frame so it pans with the tilt.

## 13. Difficulty Progression & Tuning

| Level | Beetle speed (mm/s) | Volley interval (s) | Acorns/volley | Telegraph (ms) | Bite window (ms) |
|---|---|---|---|---|---|
| 1 | 12–18 | 14 | 1 | 900 | 180 |
| 2 | 16–24 | 12 | 1–2 | 800 | 160 |
| 3 | 20–30 | 10 | 2 | 700 | 140 |
| 4 | 26–36 | 9 | 2–3 | 650 | 130 |
| 5 | 32–40 | 8 | 3 | 600 | 120 |

**Adaptive rule.** Track a rolling 12-beetle window of bite accuracy `a`. If `a > 0.85` for two windows, raise level by one (cap 5). If `a < 0.55`, lower by one (floor 1). Volley interval scales by `1 − 0.05·(level−1)`. Constants are clamped so no single tide exceeds 4 volleys/min.

## 14. Edge Cases & Failure Recovery

- **Prop lift-off**: if `P_O` rises > 6 mm from `T`, pause bite scoring, show "return shell" ghost, and freeze acorns. Resume on reseat for 300 ms.
- **Tracking loss > 400 ms**: freeze the turtle head mid-strike, hold integrity, and re-acquire with the last known `P`; on failure re-run `Calibrate`.
- **Hand leaves frame during seal**: seal decays with `τ = 180 ms`; if impact occurs during decay, treat as unsealed only if `F_apex < 0.2 N`.
- **Two-hand sector conflict**: if both rim contacts name different sectors, the one with higher force wins; ties go to the most recent contact.
- **Beetle stuck at rim**: if `r < 24 mm` for > 3 s without a bite, the beetle is recycled with no penalty.
- **Acorn never lands**: despawn after 1.8 s; no integrity change.
- **Player presses too hard**: `F_n > 1.5 N` clamps tilt and shows a soft amber warning; no penalty.
- **Session pause**: palm-cover for 1.0 s pauses; acorns are frozen, not despawned.

## 15. Implementation Architecture

Classes: `PropRegistration`, `HemisphereCollider`, `RimContactSolver`, `TiltDynamics`, `ApexSealDetector`, `BiteController`, `BeetleSpawner`, `BeetleAgent`, `BombardmentDirector`, `AcornProjectile`, `IntegrityModel`, `TideDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P`; `RimContactSolver → TiltHold/TiltTap`; `TiltDynamics → θ_t`; `BiteController` reads `θ_t`, `λ_bite`, `BeetleAgent` positions → `BiteEvent`; `BombardmentDirector → AcornProjectile → SealCheck`; all events → `ScoreLedger` and `AudioCueBus`.

```text
onFixedTick(dt = 1/90):
    P = registration.pose()
    contacts = contactSolver.solve(P, hands)
    tiltTarget = 0
    for c in contacts where c.region == RIM:
        s = sector(c.azimuth)
        F = lowpass(c.normalForce)
        tiltTarget = max(tiltTarget, asin(0.35*F / (2*R)))
    theta = tiltDynamics.step(clamp(tiltTarget, 0, 8deg), dt)
    if apexSeal.held(0.15s):
        turtle.setState(RETRACTED)
    else if theta >= 3deg:
        strike = biteController.evaluate(theta, opposite(contactSector), beetles)
        if strike.hit: score.add(100 * (1 + 0.5*chain)); chain += 1
```

```text
onBombardmentImpact(acorn):
    sealed = apexSeal.force >= 0.35N and apexSeal.dwell >= 0.15s
    if sealed:
        vfx.spark(acorn.pos); audio.play("copper_gong")
        consecutiveSeals += 1
    else:
        integrity -= 1; stun(1.2s); vfx.shake(3mm)
        consecutiveSeals = 0
    if integrity <= 0: state = Score
```

## 16. Acceptance Test Checklist

1. **Registration accuracy**: 20 presentations of the walnut shell report `R` within ±0.5 mm and rim plane tilt < 1.5°.
2. **Tilt mapping**: a calibrated 0.8 N rim press produces `θ_t = 3.9° ± 0.4°` within 180 ms.
3. **Sector classification**: 100 presses at the four cardinal sectors classify correctly ≥ 96% of the time.
4. **Bite timing**: a beetle at `r = 26 mm` on the raised sector is killed in ≥ 95% of trials when the bite fires.
5. **Wrong-sector bite**: a beetle on the opposite sector is never killed by a bite in 50 trials.
6. **Seal window**: a 150 ms apex press exactly before impact absorbs the acorn in 100% of 30 trials.
7. **Unsealed hit**: no press ⇒ integrity −1 and a 1.2 s stun in 100% of 20 trials.
8. **Tilt clamp**: no combination of hand forces produces `θ_t > 8°` or shell tip-over.
9. **Chain logic**: three bites within 4 s triggers "Snap Frenzy" and slows beetles by 25% for 2 s.
10. **Hand-loss decay**: losing rim contact > 120 ms decays the hold with `τ = 180 ms` and never snaps.
11. **Tide timer**: five tides complete in 300 s ± 1 s of simulated time; victory fires at tide 5.
12. **Pause**: palm cover for 1.0 s freezes acorns and resumes without losing integrity.
13. **Occlusion**: the far fingertip is hidden by the shell silhouette in 100% of rendered frames from the calibrated viewpoint.
14. **Performance**: the full scene (shell + 12 beetles + 6 acorns + head chain) sustains 90 fps on the baseline headset.
