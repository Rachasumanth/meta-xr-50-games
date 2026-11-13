# Micro-Mortar Alchemist

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Micro-Mortar Alchemist |
| Slug | `micro_mortar_alchemist` |
| Physical prop | Concave bottle cap (+ opposite index fingertip as pestle); `R = 15.0 mm`, `t = 0.5 mm`, mass `2.0 g` |
| Family / sub-shape | Hemisphere, bowl-up, pinch scale |
| Collider family | Concave cavity + rim torus + pestle sphere |
| Scale class | Prop 1:1 (30 mm aperture); virtual mortar ×8, golem 90 mm tall |
| Source concept quote | "Hold the concave cap in your left pinch and press the rounded pad of your right index fingertip into the hollow cavity, twisting back and forth... Grind glowing virtual crystal shards inside the cap at the exact rhythm indicated by rising spark colors, then tilt the cap to pour the liquid mana onto a miniature golem." |
| Physical input | Left thumb/index pinch the rim; right index presses and twists in the cavity; left wrist tilts to pour |
| Session length | 7 min (5 elixirs × 70 s + onboarding) |
| Lose condition | Three spoiled elixirs |

## 1. Fantasy & Core Loop

An alchemist's bench is rendered at 1:8 scale around the cap. The cap becomes a stone mortar veined with gold, and the right index fingertip becomes a pestle of polished obsidian. Glowing crystal shards — violet, amber, and viridian — tumble in the bowl. A rhythm band at the rim pulses with rising spark colors: grind on the amber pulse to release amber essence, on the violet to release violet. Grinding off-beat cracks the shards and spoils the batch. When the shard is fully dissolved, the player tilts the cap over a waiting clay golem and pours the liquid mana along a glowing channel into its mouth.

1. **Load (1.0 s).** A shard drops into the bowl and settles with a chime.
2. **Read the rhythm (1.5 s).** Three-color pulses establish a 1.6–2.2 Hz tempo.
3. **Grind (6–12 s).** The right index presses and twists; each on-beat grind adds essence, each off-beat grind adds impurity.
4. **Dissolve (0.8 s).** The shard collapses into liquid at 100% essence.
5. **Pour (1.2–2.0 s).** The left wrist tilts the cap; mana flows over the rim into the golem's mouth.
6. **Elixir resolve (2.0 s).** Dosage accuracy scores; the golem reacts.

An elixir is 12–20 s; five elixirs fill 7 min with retries.

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

- **Radius and wall**: `R = 15.0 mm`, `t = 0.5 mm`, inner radius `R_in = 14.5 mm`, mass `m = 2.0 g`.
- **Orientation**: bowl-up. Cavity faces `+ẑ_P`; inner surface with `R_in`. Apex `A_P = (0, 0, +R_in)`, rim at `z_P = 0`.
- **P origin**: rim-circle centre, held in the left pinch. `P_O` floats.
- **Pestle**: a virtual sphere of radius `r_p = 5.0 mm` rigidly attached to `R_index_tip` with a 2 mm pad offset along the fingertip normal.
- **Pour rim**: the lowest rim point under gravity, `λ_pour = atan2(−g_P.y, −g_P.x)`; the lip height is `z_lip = −R·sinβ` where `β` is the cap tilt.
- **V-frame**: anchored to `P` for the mortar interior (the grind field rotates with the cap) but the golem and pour channel are anchored to `T` at `Q_g`. This dual anchoring is intentional.
- **Deviation 1**: the cap interior is a 2-D polar grind field `(ρ, λ)` on the inner hemisphere; the pestle maps to `(ρ_p, λ_p)` with `ρ_p = R_in·sinθ_p`.
- **Deviation 2**: shards are 2-D discs in this field, not 3-D bodies; their state is `(essence, impurity, size)`.
- **Deviation 3**: pouring is a fluid-free analytic stream: mass flow `ṁ = k_p·sinβ` once `β > 35°`.
- **Deviation 4**: the golem is a render-only target with a 10 mm mouth trigger; no physical collision.

## 3. Physical Play Space & Ergonomics

The left hand holds the cap at chest height, palm up, elbow supported. The right index reaches in from above at a 60–75° cone and makes small 6–10 mm twists. The cap is small, so the motion is mostly in the index MCP and PIP joints, with the wrist only for pouring.

| Parameter | Value | Rationale |
|---|---|---|
| Hold height | 200–320 mm below eye line | Comfortable |
| Pestle entry cone | 60–75° from rim plane | Index reach |
| Grind stroke | 6–10 mm, 1.6–2.2 Hz | Fine motor, low fatigue |
| Pestle force | 0.3–0.9 N | Grind pressure |
| Pour tilt | 35–70° | Controlled stream |
| Gaze | 25–32° down, 300–420 mm | Both hands in view |
| Session | 7 min; 70 s elixirs; 15 s rest | Fine-motor stamina |

Fatigue mitigation: grinding uses wrist rotation rather than finger flexion where possible; each elixir has a 4 s "stir rest"; the left hand can rest the cap on the table edge between elixirs; a fine-motor tremor meter suggests a break after 50 s of continuous grinding.

## 4. Prop Geometry & Registration

A crown cap inverted to form a mortar. The interior enamel is glossy and slightly domed, giving good specular tracking of the pestle tip.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 15.0 mm ± 0.4 | Rim |
| Wall `t` | 0.5 mm ± 0.1 | Steel |
| Inner radius `R_in` | 14.5 mm | `R − t` |
| Interior depth | 2.8 mm (shallow) | Grind field is nearly flat |
| Mass | 2.0 g ± 0.2 | Balance |
| Pestle radius `r_p` | 5.0 mm | Index pad |
| Registration | rim ellipse + plane | 20 frames |
| Re-acquisition | `ε_R < 1.0 mm`, normal < 3° | After blur |

Feature atlas:

| Station | `θ` | Region | Feature |
|---|---|---|---|
| S0 | 0–20° | cavity centre | grind field origin |
| S1 | 20–70° | inner wall | specular ring |
| S2 | 70–88° | inner rim | pour lip band |
| S3 | 88–90° | rim torus | left-hand grip |
| S4 | 90° | outer lip | knurl |

Colliders: inner hemisphere (sensor for the pestle), rim torus (grip), pour-lip arc (±25° around `λ_pour`). Registration publishes `P` and the rim normal at 72 Hz.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `L_thumb_tip`, `L_index_tip` | S3 rim | Pinch hold | `Held` |
| I5.2 | `R_index_tip` | S1 inner wall | Press + twist | `Pestle(ρ_p, λ_p, F, ω_twist)` |
| I5.3 | `R_index_tip` | S0 centre | Lift 8 mm | `Reload` |
| I5.4 | `L_wrist` | — | Tilt > 35° | `Pour(β, λ_pour)` |
| I5.5 | `R_middle_tip` | S2 rim | Tap | `TempoTap` (manual beat) |
| I5.6 | both hands open | — | 600 ms | `Rest` |

The grind contact is valid when the pestle sphere intersects the `R_in` hemisphere within 2 mm and `F ∈ [0.2, 1.2] N`.

## 6. Input Kinematics & Signal Processing

- **Pestle contact**: `d = |p_tip − C_P| − R_in`; contact if `d < 2 mm`; normal force `F = k_p·max(0, −d)`, `k_p = 600 N/m`.
- **Grind velocity**: `v_g = (p_tip(t) − p_tip(t−1))/Δt` projected tangentially; `ω_twist = |v_g| / ρ_p`.
- **Rhythm clock**: beat period `T_b = 0.45–0.62 s` (1.6–2.2 Hz); phase `φ = (t mod T_b)/T_b`.
- **On-beat test**: grind stroke peak within `φ ∈ [0.9, 1.1]` (a ±10% window, ±45 ms at 2 Hz).
- **Essence gain**: `ΔE = 7.0·q_beat·q_force`, `q_beat = 1.0` on-beat, `0.35` off-beat; `q_force = clamp(F/0.6, 0.3, 1.2)`.
- **Impurity**: `ΔI = 3.0·(1 − q_beat)` per stroke; impurity > 40 spoils the batch.
- **Dissolve**: shard size `s` decays `ṡ = −0.055·ΔE`; at `s ≤ 0` the shard becomes liquid.
- **Pour**: `ṁ = 0.9·sinβ` g/s for `β > 35°`; flow stops at `β < 30°` (hysteresis 5°).
- **Golem dosage**: ideal dose `D* = 10.0 g`; tolerance ±1.5 g.

## 7. Virtual World Layout & Coordinate Mapping

The mortar interior uses `P`-anchored polar coordinates: `ρ ∈ [0, R_in]`, `λ ∈ [0°, 360°)`; the grind field maps to a flat disc for physics and is projected back onto the hemisphere for rendering. The golem stands on `T` at `Q_g = Q + (0, 70, 0) mm`, facing the player. The pour channel is a bezier from the lip to the golem's mouth.

| Entity | Anchoring | Notes |
|---|---|---|
| Mortar field | `P` | 2-D disc `R_in` |
| Shards | `P` | polar discs |
| Pestle | `R_index_tip` | sphere |
| Golem | `T` at `Q_g` | 90 mm tall |
| Pour stream | `P` lip → `T` mouth | gravity-bent |
| Rhythm band | `P` rim | color pulses |

Because the mortar rotates with the hand, the grind field rotates too; the player's twist is thus *relative* to the bowl, which is the intended alchemical feel.

## 8. Gameplay Systems & Rules

**Shards.** Each shard has color `c ∈ {violet, amber, viridian}`, size `s ∈ (0, 1]`, essence `E`, impurity `I`. The rhythm band pulses the target color; grinding on that color's beat yields full essence.

**Batch.** A shard dissolves at `E ≥ 100`; the liquid has color purity `P_c = 1 − I/40`. Purity < 0.6 yields a "murky" elixir worth half.

**Pour.** Tilting > 35° starts flow. The player must stop within ±1.5 g of `D*`. Over-pour > 2.0 g floods the golem (fail); under-pour < 8.0 g leaves it inert (half score).

**Golem reaction.** Correct color + correct dose = "transmutation" (full score + combo). Wrong color = "rejection" (no score, no penalty). Over-pour = spoiled batch.

**Score.** `S = 200·P_c·q_dose + 100·q_color + 50·n_perfect`, where `q_dose = 1 − |D − D*|/2.0` clamped, `q_color = 1` if correct.

**Spoil.** Impurity > 40 or over-pour: lose one of three "elixir slots". Three spoils end the session.

**Rhythm assist.** Tapping `R_middle_tip` on the rim sets a manual beat; grinding to it gives `q_beat = 1.0` even if the visual pulse is missed, but the assist has a 6 s cooldown.

**Win.** Brew 5 perfect elixirs.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | prop seen | `Load` | 2.0 s | `P` locked |
| `Load` | calibration / elixir clear | `Rhythm` | 1.0 s | shard drop |
| `Rhythm` | shard settled | `Grind` | 1.5 s | tempo set |
| `Grind` | tempo ready | `Dissolve` / `Spoil` | `E ≥ 100` | essence |
| `Dissolve` | essence full | `Pour` | 0.8 s | liquid |
| `Pour` | dissolve | `DoseCheck` / `Spoil` | `β > 35°` | stream |
| `DoseCheck` | flow stops | `GolemReact` | ±1.5 g | dose |
| `GolemReact` | dose check | `ElixirClear` / `Spoil` | 1.5 s | reaction |
| `ElixirClear` | success | `Load` / `Victory` | 2.0 s | score |
| `Victory` | 5 perfect | `Score` | 2.0 s | fanfare |
| `Spoil` | impurity/over-pour | `Load` / `Score` | 2.0 s | smoke |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Inner hemisphere | Pestle sphere | Sensor + force | stone/obsidian |
| Rim torus | `L_thumb/index` | Pinch hold | steel/skin |
| Pour-lip arc | Liquid stream | Emit trigger | — |
| Shard disc | Pestle sphere | Grind interaction | crystal |
| Stream | Golem mouth | Dose accumulation | — |
| Cap | `T` (if rested) | Support | steel/wood |

No rigid-body dynamics for the cap (held) and none for shards (2-D field). Pestle force uses a penalty spring. Pour is analytic. CCD not required: the pestle moves ≤ 1.2 mm/tick at 100 mm/s, and the sphere–hemisphere test uses a 2 mm margin. A guard clamps `ω_twist` to 40 rad/s to prevent "blender" exploits.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → table → golem → mortar exterior → mortar interior grind field → shards → liquid → rim rhythm band → pestle (fingertip) → hand mesh → vapour → UI.

Occlusion cases:
1. **Pestle inside the bowl**: the right index is hidden below the rim plane; only the portion above `z_P = 0` draws over the interior.
2. **Left fingers on the rim**: drawn in front of the rim torus; the rhythm band is clipped by the hand depth.
3. **Pour stream over the lip**: the stream is depth-tested against the rim so it appears to spill over, not through.
4. **Golem behind the cap**: correct occlusion when the player tilts the cap between the eye and the golem.
5. **Vapour**: additive vapour is depth-tested so it does not wash over the hand.
6. **UI**: the dose gauge floats beside the golem and is soft-occluded by the left forearm.

VFX budget: pestle 1.2 k tris, shards 8 × 80 tris, liquid 1 shader plane, vapour ≤ 250 particles, golem 3 k tris.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Shard drop | Glass chime | Shard settles |
| Rhythm pulse | Color-coded bell | Rim band glows |
| On-beat grind | Warm rasp | Essence stream |
| Off-beat grind | Harsh scrape | Red impurity fleck |
| Dissolve | Rising shimmer | Shard melts |
| Pour start | Liquid glug | Stream forms |
| Dose perfect | Golem hum | Gold eyes |
| Over-pour | Golem cough | Overflow splash |
| Spoil | Shatter + hiss | Smoke puff |
| Elixir clear | Alchemical chord | Rune bloom |

## 13. Difficulty Progression & Tuning

| Level | Tempo (Hz) | Beat window | Shard size | Impurity cap | Dose tolerance |
|---|---|---|---|---|---|
| 1 | 1.6 | ±12% | 0.8 | 50 | ±2.0 g |
| 2 | 1.8 | ±11% | 0.9 | 45 | ±1.8 g |
| 3 | 2.0 | ±10% | 1.0 | 40 | ±1.5 g |
| 4 | 2.1 | ±9% | 1.1 | 35 | ±1.3 g |
| 5 | 2.2 | ±8% | 1.2 | 30 | ±1.1 g |

**Adaptive rule.** Track perfect-brew rate `p` over the last 3 elixirs. If `p ≥ 0.9`, raise tempo 0.1 Hz or tighten the beat window 1%. If `p ≤ 0.4`, lower tempo and widen the window. Clamp tempo to `[1.4, 2.4] Hz`.

## 14. Edge Cases & Failure Recovery

- **Pestle leaves the bowl**: grind pauses; `Reload` requires an 8 mm lift and re-entry within 1.5 s or the shard settles.
- **Left pinch lost**: freeze the cap pose, ghost it, pause pouring; resume on re-grab.
- **Both hands collide**: hand–hand collision is not simulated, but the pestle is ignored when `|p_tip − L_fingers| < 8 mm`.
- **Cap tipped past 80°**: liquid spills immediately (spoiled); reset tilt spring to level.
- **Rhythm band missed**: the visual pulse continues; no penalty beyond off-beat impurity.
- **Tracking loss**: hold the grind field for ≤ 300 ms; longer pauses the elixir.
- **Golem mouth missed**: the stream lands on the floor; dose counts as 0 (inert) but not spoiled.
- **Over-pour exactly at boundary**: `D − D* = 2.0 g` counts as success (inclusive).

## 15. Implementation Architecture

Classes: `PropRegistration`, `PestleTracker`, `GrindField`, `Shard`, `RhythmClock`, `BeatScorer`, `LiquidModel`, `PourController`, `DoseMeter`, `GolemReactor`, `ElixirDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P`; `PestleTracker → contact, F, v_g`; `RhythmClock → φ`; `BeatScorer → ΔE, ΔI`; `Shard` updates; `PourController → ṁ`; `DoseMeter → DoseEvent`; `GolemReactor → ElixirEvent`.

```text
onFixedTick(dt = 1/90):
    p = pestleTracker.tip()
    rho = length(p.xy - mortarCenter.xy)
    in_bowl = rho <= R_in + 2mm
    F = max(0, -(distance(p, C) - R_in)) * 600
    vg = (p - p_prev) / dt
    omega = length(tangential(vg)) / max(rho, 1mm)
    onBeat = abs(phase(t, Tb) - 0.5) < 0.1
    if in_bowl and 0.2 <= F <= 1.2 and omega > 0.05:
        qb = onBeat ? 1.0 : 0.35
        shard.E += 7.0 * qb * clamp(F/0.6, 0.3, 1.2)
        shard.I += 3.0 * (1 - qb)
```

```text
onPour(dt):
    beta = tiltAngle(P)
    if beta > 35deg:
        mdot = 0.9 * sin(beta)
        dose += mdot * dt
        if dose > D_star + 2.0: spoil("overpour")
    else if beta < 30deg and pouring:
        pouring = false
        evaluateDose(dose)
```

## 16. Acceptance Test Checklist

1. **Pestle contact**: a 1 mm penetration registers `F ≈ 0.6 N` within 1 tick.
2. **Rhythm**: a stroke at `φ = 0.0` scores `q_beat = 1.0`; at `φ = 0.3` scores 0.35.
3. **Essence rate**: 10 on-beat strokes at 0.6 N add `70 ± 5` essence.
4. **Impurity**: 10 off-beat strokes add `30 ± 3` impurity.
5. **Dissolve**: a size-1.0 shard with `E = 100` becomes liquid in < 0.1 s.
6. **Pour threshold**: `β = 34°` emits 0 g/s; `β = 40°` emits `0.58 ± 0.05 g/s`.
7. **Dose tolerance**: 10.0 ± 1.5 g succeeds; 13.0 g spoils.
8. **Color match**: correct color yields `q_color = 1`; wrong color yields 0.
9. **Spoil count**: three spoils end the session with the score card.
10. **Reload**: lifting the pestle 8 mm pauses grinding and resumes on re-entry.
11. **Grind clamp**: a 60 rad/s twist is clamped to 40 rad/s and does not inflate `ΔE`.
12. **Occlusion**: the pestle below the rim is hidden by the interior in 100% of frames.
13. **Adaptive**: 3 perfect brews raise tempo by 0.1 Hz (cap 2.4 Hz).
14. **Performance**: shards + liquid + golem sustain 90 fps.
