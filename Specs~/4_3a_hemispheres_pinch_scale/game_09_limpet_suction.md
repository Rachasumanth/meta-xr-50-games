# Limpet Suction

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Limpet Suction |
| Slug | `limpet_suction` |
| Physical prop | Half walnut shell or cap, dome-up; `R = 22.0 mm`, `t = 1.8 mm`, mass `4.0 g` |
| Family / sub-shape | Hemisphere, dome-up, pinch scale |
| Collider family | Convex dome + rim torus + flat-seal sensor |
| Scale class | Prop 1:1 (44 mm aperture); virtual reef 220 mm × 160 mm |
| Source concept quote | "Slide the dome-up shell across a flat surface between two fingers and press it firmly flat against the surface when virtual waves approach... Slide to eat algae while the tide is out, and press down firmly to engage vacuum suction before crashing virtual surf waves sweep the reef clean." |
| Physical input | Two-finger slide across the table; firm flat press for suction |
| Session length | 6 min (5 tides × 55 s + onboarding) |
| Lose condition | Limpet dislodged three times |

## 1. Fantasy & Core Loop

A tidal reef is rendered on the table at 1:8 scale: wet basalt, barnacles, and a glowing cyan algal mat. The walnut shell is an armored limpet, its dome etched with growth rings and a pale mantle peeking at the rim. When the tide is out, the player slides the limpet across the reef to graze algae, which regrows in glowing patches. Every 9–14 s a surf wave telegraphs with a rising wall of white water; the player must stop, flatten the shell perfectly, and press for 250 ms to engage a vacuum seal. A sealed limpet rides out the wave; an unsealed one is torn off and tumbles.

1. **Graze (2–6 s).** Slide the dome; algae under the rim is consumed at 6 mm²/s while moving.
2. **Telegraph (1.5 s).** A foam line and a low rumble announce the wave; the reef darkens.
3. **Flatten (0.4 s).** Align the rim plane to within 4° of the table.
4. **Seal (0.25 s).** Press with a 6 N-equivalent force for 250 ms; a vacuum ring snaps shut.
5. **Wave (1.2 s).** White water passes; a sealed limpet loses no ground; an unsealed one is swept 40 mm and loses algae.
6. **Recover (1.0 s).** The seal releases and grazing resumes.

A wave cycle is 9–14 s; a tide is 55 s; five tides fill the session.

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
- **Orientation**: dome-up. Convex exterior faces `+ẑ_P`, cavity opens `−ẑ_P`. Apex `A_P = (0,0,+22.0) mm`, rim at `z_P = 0`.
- **P origin**: rim-circle centre. At rest `P_O` is in the `T` plane and the shell may translate freely.
- **Seal test**: `β = angle(n̂_P, ẑ_T)` must satisfy `β ≤ 4°`; the rim must be in contact with `T` (mean rim gap ≤ 1.5 mm); press force `F_p ≥ 6.0 N` for 250 ms.
- **V-frame**: anchored to `T` at the reef centre `Q`; axes `x̂_V = x̂_T`, `ŷ_V = ŷ_T`, `ẑ_V = ẑ_T`. Reef is 220 × 160 mm.
- **Deviation 1**: the shell is a free rigid body on the table with Coulomb friction `µ = 0.30` and rolling resistance when tilted.
- **Deviation 2**: "vacuum seal" is virtual: on seal, the shell's friction coefficient is raised to `µ_seal = 3.0` and a counter-surge force up to 9 N is applied; no real adhesion occurs.
- **Deviation 3**: wave force is applied as a horizontal pressure field on the shell, not a fluid sim.
- **Deviation 4**: the algal field is a `64 × 64` heightfield anchored to `T`, regenerating at 0.4 %/s.

## 3. Physical Play Space & Ergonomics

The player sits with the shell on a table, right index on the apex and right thumb plus left index pinching the rim for the slide. For the seal, both hands press down on opposite sides of the dome. The table is the reef; the shell travels 60–120 mm per graze.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Table `T`, 720–780 mm | Shoulder flexion < 30° |
| Reef | 220 × 160 mm | Grazing range |
| Slide speed | 0–180 mm/s, typical 90 | Algae consumption |
| Press force | 6–9 N for seal | Measurable via contact sum |
| Flatten tolerance | `β ≤ 4°` | Distinguishes press from slap |
| Gaze | 20–28° down, 320–460 mm | Neck comfort |
| Session | 6 min; 55 s tides; 12 s rest | Press stamina |

Fatigue mitigation: the seal is brief (250 ms) and uses both hands, halving the load; grazing needs almost no force; the shell can rest anywhere on the reef; a "press load" bar shows cumulative force so the player can moderate.

## 4. Prop Geometry & Registration

A walnut half with a flat sanded rim; flatness is essential because the seal test compares the rim plane to `T`.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 22.0 mm ± 0.5 | Silhouette |
| Wall `t` | 1.8 mm ± 0.3 | Rim step |
| Inner radius `R_in` | 20.2 mm | `R − t` |
| Rim flatness | ≤ 0.5 mm | Seal calibration |
| Mass | 4.0 g ± 0.6 | Balance |
| Rim contact band | 1.5 mm width | Seal ring |
| Registration | 4-point rim fit | 30 frames |
| Re-acquisition | `ε_R < 1.5 mm`, `ε_β < 3°` | After blur |

Feature atlas:

| Station | `θ` | Feature |
|---|---|---|
| S0 | 90° | Apex (sliding finger) |
| S1 | 55–85° | Growth-ring ridges |
| S2 | 15–55° | Mottled dome |
| S3 | 0–15° | Rim countersink |
| S4 | 0° | Rim seal band |

Colliders: outer hemisphere, rim torus (seal ring), and a 2-D footprint disc of radius `R` for frictional sliding. Registration publishes `P` at 72 Hz and the mean rim gap to `T`.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `R_index_tip` | S0 apex | Slide 60–120 mm | `Graze(v)` |
| I5.2 | `R_thumb_tip`, `L_index_tip` | S4 rim | Two-finger slide | `SlideAssist` |
| I5.3 | both palms | S1 dome | Flat press 6–9 N | `SealHold` |
| I5.4 | `R_index_tip` | S0 | Single tap | `Pulse` (visual ping) |
| I5.5 | either hand | — | Lift 10 mm (all joints off) | `Release` |
| I5.6 | both hands | — | Open 800 ms | `Rest` |

Seal force is the sum of normal forces from all contacts on the dome, `F_p = Σ F_n,i`. A seal requires `F_p ≥ 6.0 N` and `β ≤ 4°` simultaneously.

## 6. Input Kinematics & Signal Processing

- **Contact force**: penalty model `F_n = k_c·max(0, d_pen)`, `k_c = 1200 N/m` per contact, low-passed `α = 0.25` (≈ 2.2 Hz).
- **Rim gap**: mean over 8 sampled rim points of `(p_rim − proj_T(p_rim))·ẑ_T`; seal requires `gap ≤ 1.5 mm`.
- **Tilt**: `β = acos(clamp(n̂_P·ẑ_T, −1, 1))`; low-passed at 15 Hz.
- **Seal timer**: `t_seal += Δt` while `F_p ≥ 6.0 N` and `β ≤ 4°`; at `t_seal ≥ 250 ms` fire `Sealed`; hysteresis release at `F_p < 4.0 N` or `β > 7°`.
- **Slide friction**: `a = −µ_eff·g·v̂` when sliding; `µ_eff = 0.30` normally, `3.0` when sealed. Stop threshold `|v| < 4 mm/s`.
- **Wave force**: `F_w = 0.5·ρ·C_d·A·v_w²` with `ρ = 1000 kg/m³`, `C_d = 1.2`, `A = πR² = 1.52×10⁻³ m²`, `v_w = 1.4–2.6 m/s` scaled to the virtual reef; applied horizontally for 1.2 s with a sinusoidal envelope.
- **Dislodge test**: if the shell's horizontal displacement during a wave exceeds 18 mm, it is dislodged.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 220 × 160 mm reef at `z_V = 0` (the tabletop). The algae field is a `64 × 64` heightfield with cell size `3.4 × 2.5 mm`; each cell has `biomass ∈ [0, 1]`. Wave crests travel along `+x_V` at 400 mm/s from `x_V = −110 mm`.

| Entity | Anchoring | Notes |
|---|---|---|
| Reef | `T` at `Q` | 220 × 160 mm |
| Algae field | `T` | 64 × 64 cells |
| Shell | `P` | free body |
| Wave front | `V` | line moving +x |
| Foam telegraph | `V` | 1.5 s early |
| Graze FX | `P` rim | particle ring |

Grazing consumes cells whose centres fall within `r ≤ R − 2 mm` of `P_O` while `|v| > 8 mm/s`: `biomass −= 0.9·Δt`.

## 8. Gameplay Systems & Rules

**Algae.** Each cell regrows `+0.004/s`; grazing removes `0.9/s` under the rim. Score `+1` per cell fully consumed. A tide requires 260 cells consumed to clear.

**Waves.** Scheduled every `T_w = 9–14 s`; 1.5 s telegraph. During the wave (`1.2 s`), `F_w` up to 9 N equivalent. Sealed limpet: displacement 0 (friction holds). Unsealed: displacement `18–40 mm`, algae score −40, and a `Dislodged` strike.

**Seal bonus.** A clean seal (no displacement, `F_p` held 250–500 ms) awards `+30` and extends the combo. Over-press (`F_p > 12 N`) cracks the shell: `−1` integrity, no bonus.

**Dislodge.** Three dislodges end the session.

**Combo.** Consecutive seals without a dislodge multiply graze score by `1 + 0.2·n` (cap 3).

**Win.** Clear 5 tides with integrity > 0. Final bonus `+200·integrity`.

**Integrity.** Starts at 3; each dislodge and each over-press removes 1.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | prop seen | `Graze` | 2.0 s | `P` locked |
| `Graze` | calibration / wave end | `Telegraph` / `Seal` | wave scheduled | algae |
| `Telegraph` | wave scheduled | `Wave` | 1.5 s | foam line |
| `Seal` | press + flat | `Wave` / `Crack` | 250 ms | vacuum ring |
| `Wave` | telegraph end | `Graze` / `Dislodge` | 1.2 s | white water |
| `Dislodge` | displacement > 18 mm | `Graze` | 1.0 s | tumble |
| `Crack` | `F_p > 12 N` | `Graze` | 1.0 s | crack VFX |
| `TideClear` | cells ≥ 260 + integrity > 0 | `Graze` / `Victory` | 2.5 s | tally |
| `Victory` | 5 tides | `Score` | 2.0 s | fanfare |
| `Score` | victory / 3 dislodges | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Dome hemisphere | Table `T` | Support, friction | walnut/wet basalt |
| Rim torus | Table `T` | Seal contact band | walnut/rubber |
| Dome | Hand joints | Press force | walnut/skin |
| Shell footprint | Wave field | Horizontal force | — |
| Shell | Reef bounds | Soft wall at 110/80 mm | — |
| Algae cells | Rim footprint | Consume trigger | — |

Rigid body: `m = 4 g`, `I_tilt = 0.42·m·R² = 8.1×10⁻⁷ kg·m²`. Static friction peak `F_s = µ_s·N`; with a 6 N press and `µ_seal = 3.0`, holding force is 18 N > the 9 N wave load. The unsealed `µ = 0.30` gives only 1.2 N, hence displacement. CCD: at 400 mm/s the shell moves 4.4 mm/tick; a 10 mm sweep suffices. The seal ring uses 8 rim points at 45° spacing.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → table → reef albedo → algae heightfield → shell shadow → shell dome → rim seal ring → wave plane → hand mesh → foam → UI.

Occlusion cases:
1. **Fingertip on apex**: the index tip renders in front of the dome; the algae highlight under the shell is clipped.
2. **Hand behind the shell**: the far hand is hidden by the dome silhouette.
3. **Algae under the shell**: a "x-ray ring" shows algae just under the rim at 25% opacity, so the player can see grazing progress.
4. **Wave over the shell**: the wave plane is depth-tested; sealed limpet has a bubble void around it.
5. **UI**: the tide timer floats at `z_V = 90 mm` and is soft-occluded by the forearm.
6. **Foam telegraph**: the foam line is drawn on the table plane and occluded by the shell correctly.

VFX budget: algae 64 × 64 heightfield (1 draw), wave 1 shader plane, foam 200 particles, seal ring 1 mesh.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Graze | Wet rasp | Algae dims |
| Wave telegraph | Low rumble | Foam line |
| Seal engage | Vacuum thunk | Cyan ring snaps |
| Over-press | Crack | Red fissure |
| Wave pass (sealed) | Muffled roar | Bubble void |
| Dislodge | Tumble + splash | Shell slides |
| Combo | Rising chime | Ring stacks |
| Tide clear | Reef bloom | Algae regrows |
| Victory | Ocean swell | Gold shell |

## 13. Difficulty Progression & Tuning

| Level | Wave period (s) | Wave speed (m/s) | Seal window (ms) | Flat tolerance | Algae target |
|---|---|---|---|---|---|
| 1 | 14 | 1.4 | 300 | 6° | 200 |
| 2 | 12 | 1.7 | 275 | 5° | 230 |
| 3 | 11 | 2.0 | 250 | 4° | 260 |
| 4 | 10 | 2.3 | 225 | 3.5° | 290 |
| 5 | 9 | 2.6 | 200 | 3° | 320 |

**Adaptive rule.** Track seal success `s` over the last 5 waves. If `s ≥ 0.9`, shorten the period by 0.5 s or raise wave speed 0.1 m/s. If `s ≤ 0.4`, lengthen the period and widen the flat tolerance by 0.5°. Clamp the period to `[8, 16] s`.

## 14. Edge Cases & Failure Recovery

- **Shell lifted during a wave**: instantly dislodged (no seal); the lift is detected by all joints > 10 mm off the dome.
- **Tracking loss**: freeze wave physics for ≤ 400 ms; longer pauses the tide.
- **Seal on a tilted shell**: `β > 4°` blocks the seal; a red rim arc shows the offending side.
- **Over-press at the same time as a wave**: crack takes priority; the shell is unsealed and dislodged.
- **Shell at the reef edge**: wave force still applies; the soft wall prevents leaving the reef but does not count as a seal.
- **Two seals in one wave**: only the first 250 ms counts; the second is ignored.
- **Player uses one hand with 6 N**: allowed; the force sum is hand-agnostic.
- **Pause mid-wave**: resume at the wave's start, not mid-crest.

## 15. Implementation Architecture

Classes: `PropRegistration`, `ContactForceSolver`, `SealDetector`, `SlideDynamics`, `AlgaeField`, `WaveDirector`, `WaveForce`, `DislodgeJudge`, `TideDirector`, `IntegrityModel`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P`; `ContactForceSolver → F_p, β`; `SealDetector → Sealed`; `SlideDynamics` integrates; `WaveDirector → WaveForce → DislodgeJudge`; `AlgaeField` consumes; all → `ScoreLedger`, `AudioCueBus`.

```text
onFixedTick(dt = 1/90):
    Fp = sum(normalForce(c) for c in contacts)
    beta = angle(registration.normal, T.up)
    gap = meanRimGap(registration, T)
    if Fp >= 6.0N and beta <= 4deg and gap <= 1.5mm:
        sealTimer += dt
        if sealTimer >= 0.25: sealed = true
    else:
        sealTimer = 0; if Fp < 4.0N or beta > 7deg: sealed = false
    mu = sealed ? 3.0 : 0.30
    a = -mu * g * normalize(velocity) + waveForce()/m
    integrate(position, velocity, a, dt)
```

```text
onWaveTick(dt):
    if phase == CREST:
        if not sealed:
            displacement += waveForce()/m * dt*dt
            if displacement > 18mm: dislodge()
        algae.consumeUnderRim(P, dt)
    if waveEnded: evaluateSealBonus()
```

## 16. Acceptance Test Checklist

1. **Seal force**: a 6.0 N two-hand press with `β = 3°` seals in exactly 250 ms.
2. **Flat tolerance**: `β = 5°` never seals at any force.
3. **Rim gap**: a 2 mm gap blocks the seal even at 9 N.
4. **Friction**: an unsealed shell at 9 N wave load displaces ≥ 18 mm.
5. **Sealed hold**: a sealed shell displaces < 1 mm under the same load.
6. **Over-press**: 12.5 N cracks the shell and removes 1 integrity.
7. **Grazing**: sliding 100 mm at 90 mm/s consumes the expected cell count ±10%.
8. **Regrowth**: a consumed cell returns to 0.5 biomass in 125 s ± 5 s.
9. **Dislodge**: a lifted shell during a wave is dislodged immediately.
10. **Tide clear**: 260 consumed cells and integrity > 0 trigger TideClear.
11. **Combo**: three consecutive seals yield a 1.6× graze multiplier.
12. **Pause**: palm cover freezes the wave at its start and resumes cleanly.
13. **Occlusion**: the far hand is hidden by the dome.
14. **Performance**: algae field + wave + shell sustain 90 fps.
