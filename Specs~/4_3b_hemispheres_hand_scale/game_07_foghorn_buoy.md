# Foghorn Buoy

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Foghorn Buoy |
| Slug | `foghorn_buoy` |
| Physical prop | Upright bowl resting on its rounded foot ring; `R = 80.0 mm`, depth `60 mm`, wall `t = 4.0 mm`, mass `400 g`, foot-ring radius `36 mm` |
| Family / sub-shape | Hemisphere, bowl-up on a foot ring, hand scale |
| Collider family | Foot-ring support + rim torus + four lip press sectors |
| Scale class | Prop 1:1 (160 mm bowl); virtual sea 900 mm across |
| Source concept quote | "Press down lightly on the North, South, East, or West lip of the upright bowl so it rocks slightly on its bottom foot ring... Rock the rim in counter-rhythm to incoming virtual waves to keep the central beacon upright and ring its sonic bell to scare off circling sharks." |
| Physical input | Light presses on the four cardinal lip points; rocking on the foot ring |
| Session length | 6 min (5 storms × 60 s + onboarding) |
| Lose condition | Beacon tilt exceeds 28° three times, or a shark reaches the buoy |

## 1. Fantasy & Core Loop

The upright bowl becomes a giant sea-buoy in a moonlit storm, its ceramic sides banded with rust and rivets, a rotating beacon lantern on top. The foot ring is the buoy's rounded keel, so the whole bowl rocks like an inverted pendulum on the table. Virtual ocean swells roll in on a timetable from four quadrants, each tilting the sea surface and pushing the buoy. The player presses the lip on the opposite side to counter-rock, keeping the beacon within 28° of vertical. A quick double-press on two opposite lips rings the sonic foghorn, which scatters sharks circling the buoy.

1. **Read the swell (1.2 s).** A wall of water rises from one quadrant; the buoy begins to lean.
2. **Counter-press (0.4 s).** Press the opposite lip to apply a restoring torque.
3. **Ride (0.8 s).** The swell passes; residual rocking decays.
4. **Ring (0.5 s, optional).** A double-press pair triggers the foghorn, pushing sharks back 120 mm.
5. **Shark approach (continuous).** Sharks circle at 260 mm and close when the beacon is tilted.
6. **Storm resolve (2.0 s).** Upright time and shark repels tally.

A swell cycle is 3.5–5 s; a storm is 60 s; five storms fill the session.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Hemisphere sub-shape**: the prop is a hemispherical shell of radius `R` and wall thickness `t`; model in P with the rim circle in the `x̂_P`–`ŷ_P` plane at `z = 0` and the apex at `z = ±R`. For **bowl-up** props the cavity faces `+ẑ_P` (inner surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`); for **dome-up** props the convex dome faces `+ẑ_P` (outer surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`, cavity facing `−ẑ_P`). Rim torus: major radius `R`, minor radius `t/2`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-AFX cue.

### 2.1 Game-Specific Overrides

- **Radius and wall**: `R = 80.0 mm`, depth `60 mm`, wall `t = 4.0 mm`, mass `m = 400 g`, foot-ring radius `r_f = 36 mm`.
- **Orientation**: bowl-up. Cavity faces `+ẑ_P`; the foot ring is on the `−ẑ_P` side and contacts the table at `z_P = −60 mm` (apex of the foot ring).
- **P origin**: rim-circle centre. At rest the foot-ring contact is directly below `P_O`.
- **Rocking model**: a 2-DoF inverted pendulum about the foot-ring contact. Tilt vector `θ⃗ = (θ_x, θ_z)` in the `T` plane; magnitude `|θ| ≤ 28°`.
- **Contact**: the foot ring touches `T` along an arc; the effective pivot is the foot-ring contact point `c_f = r_f·(sinθ_x, 0, sinθ_z)` projected onto `T`.
- **V-frame**: anchored to `T` at the buoy's rest centre `Q`; sea plane at `z_V = 0`; the buoy is at the origin. `P` rocks about `Q` but does not translate.
- **Deviation 1**: the real bowl rocks on its foot ring; the virtual buoy is simulated with a 2-DoF pendulum driven by measured lip presses and virtual wave torques.
- **Deviation 2**: wave forces are `V`-space torques, not fluid pressure.
- **Deviation 3**: sharks are `V`-space agents on a 260 mm circle; they never collide with the real bowl.
- **Deviation 4**: the foghorn is a virtual ring; the real bowl makes no sound.

## 3. Physical Play Space & Ergonomics

The bowl sits on a table or tray in front of the player. The player presses with the index finger of each hand on the near lip and the thumbs on the far lip, so all four cardinal points are reachable without moving the torso. The presses are light (1–3 N) and brief (0.2–0.5 s).

| Parameter | Value | Rationale |
|---|---|---|
| Support | Table `T`, 720–780 mm | Shoulder comfort |
| Bowl position | 180–240 mm in front of sternum | Four-point reach |
| Press points | N/S/E/W lip (±30° sectors) | Quadrant model |
| Press force | 1.0–3.0 N | Rock authority |
| Rock amplitude | 0–28° gameplay; 35° cap | Stability margin |
| Rock frequency | 0.3–0.8 Hz | Natural pendulum |
| Gaze | 18–25° down | Beacon and sea visible |
| Session | 6 min; 60 s storms; 12 s rest | Finger press stamina |

Fatigue mitigation: presses are light and alternating; the bowl's own weight provides most of the restoring torque, so the player only shapes the motion; each storm ends with a 6 s hands-off rest; a press-load meter warns after 40 s of continuous contact.

## 4. Prop Geometry & Registration

A glazed bowl with a rounded foot ring; the ring's curvature is what makes the rocking smooth and repeatable.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 80.0 mm ± 1.0 | Silhouette |
| Depth | 60 mm ± 1.5 | Rim to foot |
| Wall `t` | 4.0 mm ± 0.5 | Rim step |
| Foot-ring radius | 36.0 mm ± 1.0 | Pivot circle |
| Foot-ring tube radius | 4.0 mm | Contact curvature |
| Mass | 400 g ± 25 | Balance |
| COM height above pivot | 26 mm | `d_com` |
| Registration | rim ellipse + foot-ring ellipse | 30 frames |
| Re-acquisition | `ε_R < 2.0 mm`, tilt < 4° | After blur |

Feature atlas:

| Station | Region | Feature |
|---|---|---|
| S0 | foot ring | Pivot contact |
| S1 | lower body 0–30° | Rust bands |
| S2 | mid body 30–60° | Rivets |
| S3 | upper body 60–85° | Beacon mount |
| S4 | rim | Four press sectors |

Colliders: foot-ring torus (pivot), outer body (render + soft palm avoidance), rim torus (press), and four press sectors at `λ = 0°, 90°, 180°, 270°`. Registration publishes `P` and the tilt at 72 Hz.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `R_index_tip` | S4 south sector | Press | `Press(S, F)` |
| I5.2 | `L_index_tip` | S4 north sector | Press | `Press(N, F)` |
| I5.3 | `R_thumb_tip` | S4 east sector | Press | `Press(E, F)` |
| I5.4 | `L_thumb_tip` | S4 west sector | Press | `Press(W, F)` |
| I5.5 | two opposite sectors | S4 | Double-press pair | `Foghorn` |
| I5.6 | either hand | — | 800 ms off-bowl | `Rest` |

Press torque: `τ⃗ += (p_press − Q) × F·n̂` projected onto `T`. The horizontal lever arm is `R·(cosλ, sinλ)`, so a press at south produces a tilt toward north.

## 6. Input Kinematics & Signal Processing

- **Pendulum**: `θ̈ = (m·g·d_com/I_p)·sinθ + τ_wave/I_p + τ_press/I_p − (c_p/I_p)·θ̇`, with `I_p = m·(d_com² + r_f²/2) = 0.4·(0.026² + 0.036²/2) = 5.3×10⁻⁴ kg·m²`, `d_com = 26 mm`, `c_p = 0.020 N·m·s/rad`. Natural frequency `ω_n = sqrt(m·g·d_com/I_p) = sqrt(0.4·9.81·0.026/5.3e-4) ≈ 13.9 rad/s` (2.2 Hz) — but the pivot geometry gives an effective slower rock because the foot ring is a rolling contact; the tuned game value is `f_rock = 0.55 Hz` (`ω_n = 3.46 rad/s`).
- **Rocking feedback**: use `ω_n² = m·g·d_com/I_eff` with `I_eff = 2.1×10⁻³ kg·m²` (accounting for the rolling foot ring), giving `ω_n = 3.4 rad/s`.
- **Press torque**: `τ_p = F·R·sin(λ_p − θ_dir)`; a 2 N press at the south lip on a 6° tilt gives `τ_p ≈ 0.16 N·m`.
- **Wave torque**: each swell has a direction `λ_w`, amplitude `A_w`, period `T_w`; `τ_w = A_w·sin(2πt/T_w)·(sinλ_w, cosλ_w)` with `A_w = 0.05–0.18 N·m`.
- **Beacon tilt**: `|θ|`; warning at 20°, fail at 28°.
- **Foghorn**: two opposite sectors pressed within 150 ms, both `F ≥ 1.5 N`; repels sharks within 200 mm by 120 mm.
- **Shark approach**: `r_s` decreases at `v_s = 12 + 0.8·|θ|° mm/s`; at `r_s < 40 mm` it reaches the buoy.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 900 mm sea plane at `z_V = 0`, with the buoy at the origin and the beacon lantern at `z_V = 200 mm` (scaled). Swells approach from one of four quadrants every 3.5–5 s. Sharks circle at `r = 260 mm`.

| Entity | Anchoring | Notes |
|---|---|---|
| Sea plane | `T` | 900 mm |
| Buoy | `P` | rocks about `Q` |
| Beacon | `P` + 200 mm | lantern |
| Swells | `V` | 4 quadrants |
| Sharks | `V` | 3–5 agents |
| Foghorn ring | `V` | 200 mm radius |

The buoy's tilt is drawn at ×3 visual exaggeration so a 6° physical tilt reads as an 18° virtual lean.

## 8. Gameplay Systems & Rules

**Swells.** Each has `λ_w`, `A_w`, `T_w`, and a 1.2 s telegraph. The torque direction is fixed per swell; the counter is a press on the opposite sector.

**Upright time.** `|θ| ≤ 12°` counts as upright; accumulate `t_up`. Score `+10/s` while upright.

**Warning / fail.** `|θ| > 20°` flashes a red horizon; `|θ| > 28°` for 150 ms is a "capsize" (one of three lives).

**Sharks.** Each has `r_s`, `θ_s`, speed `12 + 0.8|θ|° mm/s`. A shark reaching `r_s < 40 mm` ends the storm. The foghorn repels all sharks within 200 mm by 120 mm and stuns them for 2 s.

**Foghorn.** Costs 15 "air" (of 100); air regenerates at 8/s. A perfect double-press (within 150 ms, each ≥ 1.5 N) is a "clean ring" and repels 160 mm.

**Score.** `S = 10·t_up + 80·clean_rings + 200·storm_clear − 150·capsizes`.

**Win.** Clear 5 storms with ≥ 1 life. **Lose.** Three capsizes or a shark reaches the buoy.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | bowl seen | `Calm` | 2.0 s | `P` locked |
| `Calm` | calibration / storm clear | `Swell` | telegraph | gentle rock |
| `Swell` | scheduled | `Ride` | 1.2 s | wave torque |
| `Ride` | swell crest | `Calm` / `Capsize` | 0.8 s | counter-press |
| `Capsize` | `|θ| > 28°` 150 ms | `Calm` / `StormEnd` | 0.6 s | life lost |
| `Ring` | double-press | `Calm` | 0.5 s | repel sharks |
| `StormClear` | 60 s | `Calm` / `Victory` | 2.0 s | tally |
| `Victory` | 5 storms | `Score` | 2.0 s | fanfare |
| `Score` | victory / 3 capsizes / shark | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Foot ring | Table `T` | Pivot + rolling | ceramic/wood |
| Rim torus | Fingertip | Press torque | ceramic/skin |
| Body | Palms | Soft avoidance | ceramic/skin |
| Buoy | Wave field | Torque | — |
| Shark | Buoy radius | End storm | — |
| Foghorn ring | Shark | Repel | — |

Pendulum: `I_eff = 2.1×10⁻³ kg·m²`, `m = 400 g`, `d_com = 26 mm`, `c_p = 0.020 N·m·s/rad`. `ω_n = 3.4 rad/s`, `ζ = 0.18`. The foot ring may slip if the tilt exceeds 35°, but the game caps the simulated tilt at 35° to prevent the real bowl from sliding. CCD not required: tilt rates ≤ 120 °/s → 1.3°/tick.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → table → sea plane → swells → far sharks → buoy body → beacon lantern → foghorn rings → hand mesh → spray → UI.

Occlusion cases:
1. **Fingers on the lip**: fingertips render in front of the rim; the press-point glow is clipped by the hand.
2. **Palms behind the bowl**: hands on the far lip are hidden by the near body.
3. **Shark behind the buoy**: correct occlusion as a shark circles behind.
4. **Swell over the buoy**: the swell plane is depth-tested and breaks around the hull.
5. **Beacon glow**: additive light bloom is depth-tested against the hand.
6. **UI**: the storm timer floats at `z_V = 240 mm` and is occluded by the beacon.

VFX budget: buoy 3 k tris, beacon 500 tris, 5 sharks × 600 tris, swell 1 shader plane, spray ≤ 400 particles.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Swell telegraph | Low horn | Water wall rises |
| Wave impact | Heavy slosh | Buoy leans |
| Counter-press | Wood thump | Press glow |
| Warning | Alarm beep | Horizon flashes red |
| Capsize | Crash | Buoy tilts over |
| Clean foghorn | Deep bellow | Ring expands |
| Shark repel | Screech | Shark recoils |
| Storm clear | Calm sea | Moon breaks |
| Victory | Harbor bell | Score card |

## 13. Difficulty Progression & Tuning

| Level | Swell period (s) | Swell torque (N·m) | Sharks | Shark speed base (mm/s) | Cap threshold |
|---|---|---|---|---|---|
| 1 | 5.0 | 0.05 | 2 | 10 | 32° |
| 2 | 4.5 | 0.08 | 3 | 12 | 30° |
| 3 | 4.0 | 0.11 | 4 | 14 | 28° |
| 4 | 3.8 | 0.14 | 4 | 16 | 26° |
| 5 | 3.5 | 0.18 | 5 | 18 | 24° |

**Adaptive rule.** Track upright fraction `u` over the last 20 s. If `u ≥ 0.85`, raise swell torque 0.02 N·m or shorten the period 0.3 s. If `u ≤ 0.45`, lower the torque and lengthen the period. Clamp the period to `[3.0, 6.0] s`.

## 14. Edge Cases & Failure Recovery

- **Bowl slides on the table**: if `P_O` moves > 15 mm, soft-clamp and re-centre; the pivot is reset.
- **Bowl lifted**: if `P` rises > 10 mm, freeze the storm and show a re-seat ghost.
- **Press on a non-cardinal sector**: snaps to the nearest cardinal within ±45°.
- **Two presses on the same sector**: take the stronger force; no double torque.
- **Foghorn during a capsize**: the capsize takes priority; the ring is refunded.
- **Tracking loss**: hold the pendulum state for ≤ 400 ms; longer pauses.
- **Shark at the buoy during a capsize**: the storm ends anyway; only one life is lost.
- **Bowl tipped past 35°**: clamp, but visually show a near-capsize; no capsize unless 28° persists.

## 15. Implementation Architecture

Classes: `PropRegistration`, `PressSolver`, `RockPendulum`, `SwellDirector`, `SharkAgent`, `Foghorn`, `AirBudget`, `StormDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P, θ`; `PressSolver → τ_press`; `RockPendulum` integrates; `SwellDirector → τ_wave`; `SharkAgent` closes on `θ`; `Foghorn → repel`; all → `StormDirector`, `ScoreLedger`.

```text
onFixedTick(dt = 1/90):
    tau = pressSolver.torque(hands) + swellDirector.torque(t)
    thetaDD = (m*g*d_com/I_eff)*sin(theta) + tau/I_eff - (c_p/I_eff)*thetaD
    thetaD += thetaDD * dt; theta += thetaD * dt
    theta = clamp(theta, -35deg, 35deg)
    if abs(theta) > 28deg: capsizeTimer += dt else capsizeTimer = 0
    if capsizeTimer > 150ms: capsize()
    for s in sharks:
        s.r -= (12 + 0.8*absDeg(theta)) * dt
        if s.r < 40: endStorm("shark")
```

```text
onFoghorn(pressA, pressB):
    if opposite(pressA.sector, pressB.sector) and dt < 150ms and air >= 15:
        air -= 15
        for s in sharks:
            if s.r < 200: s.r += 120; s.stun = 2.0
        audio.play("deep_bellow"); vfx.ring(200mm)
```

## 16. Acceptance Test Checklist

1. **Pendulum frequency**: after an initial 6° tilt the buoy rocks at 0.55 ± 0.06 Hz.
2. **Press torque**: a 2 N south press on a 6° tilt produces 0.16 ± 0.02 N·m.
3. **Counter-rock**: a well-timed opposite press reduces `|θ|` by ≥ 60% within 0.4 s.
4. **Wave torque**: a 0.11 N·m swell peaks at the telegraph + 1.2 s ± 0.1 s.
5. **Warning**: `|θ| > 20°` triggers the red horizon.
6. **Capsize**: `|θ| > 28°` for 150 ms costs exactly one life.
7. **Shark**: a shark reaches `r < 40 mm` and ends the storm.
8. **Foghorn**: a clean opposite double-press repels sharks within 200 mm by 120 mm.
9. **Air**: a foghorn costs 15 air and regenerates at 8/s.
10. **Storm timer**: five 60 s storms complete in 300 s ± 1 s.
11. **Slide clamp**: a 16 mm bowl drift is soft-clamped and re-centred.
12. **Pause**: palm cover freezes the pendulum and resumes cleanly.
13. **Occlusion**: a shark behind the buoy is hidden.
14. **Performance**: buoy + 5 sharks + swell sustain 90 fps.
