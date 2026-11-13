# Deep-Space Dish

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Deep-Space Dish |
| Slug | `deep_space_dish` |
| Physical prop | Contact-lens case bowl, concave-up; `R = 14.0 mm`, `t = 1.0 mm`, mass `3.0 g`; translucent polypropylene |
| Family / sub-shape | Hemisphere, bowl-up, pinch scale |
| Collider family | Concave inner spheroid cap + rim torus + outer grip band |
| Scale class | Prop 1:1 (28 mm aperture); virtual signal sphere 300 mm radius |
| Source concept quote | "Pinch the outer base of the tiny concave cup and angle its hollow interior dish toward different points in the air above your lap... Angle the dish to catch faint bouncing signal beams from invisible stealth satellites orbiting your personal space, holding the lock until their cloak shatters." |
| Physical input | Thumb + index pinch the outer base; 3-DoF wrist aim of the cavity axis |
| Session length | 6 min (5 contacts × 55 s + onboarding) |
| Lose condition | Three satellites escape before cloak-shatter |

## 1. Fantasy & Core Loop

A radio-quiet valley is rendered in the air above the player's lap. The contact-lens cup becomes a 30 m feed-horn dish, its translucent plastic turning into pale ceramic with a copper rim. Five cloaked satellites drift on a 300 mm virtual sphere around the player's head, each emitting a faint 12 Hz ping that only the dish can hear. The dish's gain is a narrow `cos⁴` lobe; the player must aim the cavity axis within 7° of a satellite and hold it while the satellite drifts and the cloak fractures in stages. Every re-aim is a wrist rotation of only a few degrees, so the challenge is steadiness, not speed.

1. **Scan (2–4 s).** The dish sweeps; the ping rate rises as the cavity axis approaches a satellite azimuth/elevation.
2. **Acquire (0.5 s).** Gain crosses 0.55 and the signal bar appears.
3. **Lock (1.5 s).** Hold gain ≥ 0.55 continuously; a lock ring closes.
4. **Fracture (1.0 s).** Cloak shatters in three plates; the satellite is revealed and scores.
5. **Drift (3–6 s).** The satellite glides 20–40° across the sphere; if the dish falls below gain 0.35 for 0.8 s the lock breaks.
6. **Contact resolve (2.0 s).** Score tallies and the next satellite fades in.

A contact is 7–13 s, five contacts plus onboarding fill the session.

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

- **Radius and wall**: `R = 14.0 mm`, `t = 1.0 mm`, inner radius `R_in = 13.0 mm`, mass `m = 3.0 g`.
- **Orientation**: bowl-up. Cavity faces `+ẑ_P`; inner surface `p_P = R·(sinθ cosλ, sinθ sinλ, cosθ)`, `θ ∈ [0°, 90°]`, `λ ∈ [0°, 360°)`. Apex (cavity bottom) `A_P = (0, 0, +R)`; rim circle at `z_P = 0`.
- **P origin**: rim-circle centre. The cup is hand-held, so `P_O` floats freely in `W`; no table contact is required.
- **Dish axis**: `d̂_P = +ẑ_P` (cavity axis of symmetry). The virtual feed horn sits at `A_P + 6 mm·ẑ_P`.
- **Rim**: torus major `R = 14.0 mm`, minor `t/2 = 0.5 mm`, `z_P = 0`.
- **V-frame**: anchored to `T`, but with a 300 mm-radius signal sphere centred at the *head* `H_W` (not the cup). `V` origin is `H_W`, axes aligned with `W`; `P` is expressed in `V`.
- **Gain model**: `G(ψ) = cos⁴ψ`, where `ψ` is the angle between `d̂` and the unit vector from `H` to a satellite.
- **Deviation 1**: `P` translation is ignored for aiming; only orientation matters. A 40 mm positional deadband prevents hand tremor from moving the signal sphere.
- **Deviation 2**: the dish collider is the *inner* surface only; the outer plastic is a render shell. Pinch contacts use an outer grip band at `θ = 70–90°`.
- **Deviation 3**: satellites are virtual `V`-space bodies on a 300 mm sphere and never collide with real geometry.
- **Deviation 4**: aim is smoothed with a 90 ms slerp to suppress tracker jitter; this smoothing is disabled during lock to avoid gain inflation.

## 3. Physical Play Space & Ergonomics

The player holds the cup in the right hand at eye level or slightly below, elbow supported on the chair arm. The left hand rests in the lap. Aiming is a wrist rotation of ±35° azimuth and ±25° elevation; the head turns ≤ 40° to follow satellites behind the shoulder. The cup is lightweight (3 g), so the pinch only stabilizes it; the wrist carries the aim.

| Parameter | Value | Rationale |
|---|---|---|
| Hold height | 250–400 mm below eye line | Comfortable wrist elevation |
| Elbow support | Chair arm or table edge | Reduces shoulder load |
| Aim range | azimuth ±110°, elevation −20°…+70° | Covers the signal sphere |
| Pinch force | 0.1–0.4 N | 3 g prop needs almost none |
| Wrist rotation rate | 10–40 °/s typical | Lock needs slow drift |
| Gaze | ±40° head turn, 15° down | Neck comfort |
| Session | 6 min; 55 s contacts; 12 s rest | Aiming fatigue |

Fatigue mitigation: the cup can be rested on the knee between contacts; a "lower arm" prompt appears after 45 s of continuous elevation; the lock does not require holding a fixed pose, only staying inside a 7° cone; the left hand is unused.

## 4. Prop Geometry & Registration

A standard soft contact-lens case cup: hemispherical bowl, flat rim flange, slightly frosted plastic that tracks well under IR.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 14.0 mm ± 0.4 | Silhouette fit |
| Wall `t` | 1.0 mm ± 0.15 | Rim step |
| Inner radius `R_in` | 13.0 mm | `R − t` |
| Mass | 3.0 g ± 0.4 | Balance |
| Rim flange width | 1.2 mm | Grip band |
| Grip band | `θ = 70–90°` outer surface | Pinch region |
| Registration | 3-point rim ellipse + axis | 20 frames |
| Re-acquisition | `ε_R < 1.2 mm`, axis error < 4° | After occlusion |

Feature atlas:

| Station | `θ` | Feature |
|---|---|---|
| S0 | 0–15° | Cavity bottom (feed horn mount) |
| S1 | 15–60° | Inner specular dish |
| S2 | 60–82° | Inner rim shadow band |
| S3 | 82–90° | Rim flange (grip) |
| S4 | 70–90° outer | Outer grip band (pinch) |

Colliders: inner concave cap at `R`, rim torus, outer grip band. The inner cap is a render-and-raycast collider but a *sensor* for contacts (no forces). Registration publishes orientation quaternion `q_P` at 72 Hz.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `R_thumb_tip`, `R_index_tip` | S4 outer grip | Pinch hold 0.1–0.4 N | `Held` |
| I5.2 | `P` orientation | — | Wrist rotation | `AimQuat(q_P)` |
| I5.3 | `R_wrist` | — | Rotation rate > 90 °/s | `Slew` (settle 400 ms) |
| I5.4 | `L_index_tip` | S3 rim (optional) | Tap | `ModeToggle` |
| I5.5 | both hands open | — | 500 ms | `LowerArm` |
| I5.6 | `R_middle_tip` | S4 grip | Second-finger brace | `Stabilize` (+20% gain hold) |

The aim vector is `d̂ = q_P · ẑ_P`; the signal angle to satellite `i` is `ψ_i = acos(clamp(d̂ · û_i, −1, 1))` with `û_i` the unit vector from the head to satellite `i`.

## 6. Input Kinematics & Signal Processing

- **Orientation filter**: one-euro filter on the quaternion with `f_c = 1.5 Hz` at rest, `f_c = 8 Hz` during motion, `β = 0.4`; then a 90 ms slerp toward the filtered pose.
- **Angular rate**: `ω = 2·log(q_t⁻¹ q_{t−1})/Δt`; if `|ω| > 90 °/s`, flag `Slew` and suspend lock accumulation for `max(400 ms, |ω|/200 °/s)`.
- **Gain**: `G_i = cos⁴ψ_i`; expressed in dB `G_dB = 40·log10(cosψ_i)`.
- **Acquire threshold**: `G_i ≥ 0.55` (ψ ≤ 30.6°); **lock threshold**: hold `G_i ≥ 0.55` for 1.5 s; **break threshold**: `G_i < 0.35` (ψ > 39.8°) sustained 0.8 s.
- **Drift compensation**: satellite angular velocity `ω_s ≤ 12 °/s`; the player must track within a lag of 60 ms, i.e. aim error < 1.5° from drift alone.
- **Jitter**: aim noise σ ≤ 0.6° RMS with filtering; the lock cone is 7°, giving > 10σ margin.
- **Held check**: pinch distance < 26 mm and both contacts present for 120 ms.

## 7. Virtual World Layout & Coordinate Mapping

`V` is centred on the head `H_W` with radius 300 mm. Satellites are placed by azimuth `α` and elevation `ε`: `s_V = 300·(cosε cosα, sinε, cosε sinα)` in a Y-up convention (`ẑ_V` forward). The dish model is drawn at `P`, 120 mm in front of the chest, scaled ×30 for legibility while the aim math uses the true `d̂`.

| Entity | Anchoring | Notes |
|---|---|---|
| Signal sphere | `H_W` | radius 300 mm |
| Satellites | `V` | 5 active, drift on great circles |
| Dish proxy | `P` | scaled ×30 visual |
| Feed horn | `P` + 6 mm `ẑ_P` | beam origin |
| Cloak plates | satellite-local | 3 plates |
| Lock ring | screen-space at dish | depth-tested |

The gain lobe is drawn as a translucent cone of half-angle 30.6° from the feed horn; its opacity is `G`.

## 8. Gameplay Systems & Rules

**Satellites.** Each has `cloak ∈ {intact, cracked, shattered}`, a drift path (great circle, `ω_s = 6–12 °/s`), and a required lock time `T_lock = 1.5 s`.

**Lock accumulation.** `L += Δt` while `G ≥ 0.55` and not `Slew`; `L` decays at `0.6·Δt` while `0.35 ≤ G < 0.55`; resets to 0 if `G < 0.35` for 0.8 s. At `L ≥ 1.5 s` the cloak cracks.

**Fracture.** Cracking spawns 3 cloak plates; a further `0.6 s` of `G ≥ 0.55` shatters them. Score `S = 300·G_at_lock + 150·(1 − L_over/1.5)`, where `L_over` is overshoot time.

**Escape.** If a satellite's cloak is intact for 25 s after spawn, it escapes: lose one of three "contacts". Three escapes end the session.

**Chain.** Consecutive shatters without a break multiply by `1 + 0.3·n` (cap 4). A break resets `n`.

**Win.** Shatter all 5 satellites before 3 escapes.

**Stabilize.** Holding a second finger on the grip adds +20% to hold tolerance (lock cone 8.4°) for 4 s, then goes on a 10 s cooldown.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | prop seen | `Scan` | 2.0 s / axis fit | `P` locked |
| `Scan` | contact start | `Acquire` | `G ≥ 0.55` | ping rate up |
| `Acquire` | gain threshold | `Lock` / `Scan` | 0.5 s | bar fill |
| `Lock` | acquire | `Fracture` / `Break` | 1.5 s at `G ≥ 0.55` | ring closes |
| `Fracture` | lock complete | `Drift` | 1.0 s | cloak plates |
| `Drift` | fracture | `Resolve` / `Break` | satellite path end | movement |
| `Break` | `G < 0.35` 0.8 s | `Scan` | 0.8 s | lock ring snaps |
| `Resolve` | shatter | `Scan` / `Victory` | 2.0 s | score |
| `Victory` | 5 shattered | `Score` | 2.0 s | fanfare |
| `Score` | victory / 3 escapes | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Inner dish cap | Render ray | Sensor only | ceramic |
| Rim torus | Fingertip | Pinch force estimate | plastic |
| Outer grip band | Thumb/index | Pinch hold | plastic |
| Dish proxy | Signal sphere | Trigger (aim cone) | none |
| Cloak plate | Beam volume | Shatter trigger | energy |
| Hand mesh | Cup | Soft avoidance (no forces) | — |

The cup has no simulated rigid-body dynamics beyond the tracked pose; because it is 3 g and hand-held, gravity is not modelled. Beam tests are analytic: a satellite is "in beam" if `ψ < 30.6°`. No CCD is required. A guard prevents `G` from being evaluated when `|ω| > 90 °/s` to stop "whip-lock" exploits.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → scene mesh → hand mesh → signal sphere (additive, depth-tested) → satellites → cloak plates → dish proxy → beam cone → lock ring → UI.

Occlusion cases:
1. **Cup in front of palm**: the dish proxy is occluded by the palm when the hand rotates it edge-on, preserving the real silhouette.
2. **Fingers over the rim**: fingertips are drawn over the rim flange; the lock ring is clipped by the hand mask.
3. **Satellite behind the head**: satellites behind the player's head are hidden by the head depth and reappear on turn.
4. **Beam through hand**: the additive cone is depth-tested so it dims behind fingers rather than drawing through them.
5. **UI**: the gain bar is placed 80 mm above the cup and soft-occluded by the forearm.
6. **Parallax**: satellites render at true 300 mm so accommodation and convergence match.

VFX budget: 5 satellites × 800 tris, cloak plates 15 × 60 tris, beam cone 1 additive mesh, particles ≤ 400.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Gain rise | Ping rate 1→7 Hz | Cone brightens |
| Acquire | Rising two-tone | Signal bar appears |
| Lock tick | Metronome 4 Hz | Ring closes |
| Lock complete | Crystal chord | Cloak cracks |
| Fracture | Glass shatter | Cloak plates fly |
| Break | Downward sweep | Ring snaps red |
| Slew warning | Soft whoosh | Cone dims |
| Escape | Low sonar | Satellite fades |
| Victory | Choir pad | Starfield bloom |

All cues are spatialized on the 300 mm sphere so the player can hear a satellite before acquiring it.

## 13. Difficulty Progression & Tuning

| Level | Lock cone | Lock time (s) | Drift (°/s) | Escape timer (s) | Satellites |
|---|---|---|---|---|---|
| 1 | 40° | 1.0 | 4 | 35 | 4 |
| 2 | 35° | 1.2 | 6 | 30 | 5 |
| 3 | 31° | 1.5 | 8 | 25 | 5 |
| 4 | 27° | 1.8 | 10 | 22 | 5 |
| 5 | 24° | 2.0 | 12 | 20 | 5 |

**Adaptive rule.** Track shatters per minute `r`. If `r > 0.6` and breaks < 1, narrow the cone by 3° (floor 24°). If `r < 0.25` or breaks > 3, widen by 4° (cap 40°). Lock time scales with `L = 1.0 + 0.25·(level−1)`.

## 14. Edge Cases & Failure Recovery

- **Cup dropped**: on free-fall acceleration > 6 m/s² with no hand contact, freeze scoring, ghost the cup at the last pose, and resume on re-grab.
- **Tracking loss**: re-acquire within 600 ms; the lock decays but is not reset unless loss > 1.5 s.
- **Whip-lock**: angular rate > 90 °/s suspends accumulation; the exploit cannot inflate `L`.
- **Satellite behind player**: elevation is clamped to `[−20°, +70°]`, so no satellite requires > 110° azimuth.
- **Two satellites in the beam**: the one with higher `G` accumulates; the other is unaffected.
- **Grip slip**: if pinch force < 0.05 N for 150 ms, mark `Dropped` and pause aim.
- **Ambient IR interference**: if registration confidence < 0.6 for 1 s, show a re-present prompt.
- **Player lowers the cup**: `LowerArm` pauses lock decay; no penalty.

## 15. Implementation Architecture

Classes: `PropRegistration`, `AimFilter`, `SignalSphere`, `Satellite`, `CloakModel`, `GainEvaluator`, `LockAccumulator`, `DishVisual`, `BeamRenderer`, `ContactDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → q_P`; `AimFilter → d̂`; `GainEvaluator(d̂, satellites)` → `G_i`; `LockAccumulator` → `LockEvent`/`BreakEvent`; `CloakModel` → `FractureEvent`; all → `ScoreLedger`, `AudioCueBus`.

```text
onFixedTick(dt = 1/90):
    q = aimFilter.update(registration.quat, dt)
    d = normalize(q * (0,0,1))
    for s in satellites:
        u = normalize(s.pos - head.pos)
        G = pow(max(0, dot(d,u)), 4)
        if abs(omega) > 90deg/s: s.lock = max(0, s.lock - 0.5*dt); continue
        if G >= 0.55: s.lock += dt; s.below = 0
        else if G >= 0.35: s.lock = max(0, s.lock - 0.6*dt); s.below += dt
        else: s.below += dt; if s.below > 0.8: s.lock = 0
        s.gain = G
```

```text
onLockComplete(s):
    s.cloak = CRACKED; vfx.crack(s); audio.play("crystal_chord")
    s.fractureTimer = 0.6
onFractureTick(s, dt):
    if s.cloak == CRACKED:
        s.fractureTimer -= dt
        if s.fractureTimer <= 0:
            s.cloak = SHATTERED
            score.add(300*s.gain + 150*clamp(1 - s.overshoot, 0, 1))
            score.chain += 1
```

## 16. Acceptance Test Checklist

1. **Aim accuracy**: rotating the cup by a known 20° yaw produces a filtered aim change of 20° ± 1.5° within 250 ms.
2. **Gain law**: at `ψ = 0°, 15°, 30.6°, 45°` the measured gain is 1.00, 0.87, 0.55, 0.25 ± 0.02.
3. **Lock timing**: holding `G = 0.6` for 1.5 s triggers the crack exactly once (n = 30).
4. **Break logic**: dropping below `G = 0.35` for 0.8 s resets `L` to 0 in 100% of trials.
5. **Slew guard**: a 120 °/s wrist flick does not increase `L` during the flick.
6. **Escape timer**: an untouched satellite escapes at 25 s ± 0.2 s.
7. **Chain**: two consecutive shatters yield a 1.3× multiplier on the second.
8. **Escape loss**: three escapes end the session with the score card.
9. **Tracking loss**: 600 ms occlusion re-acquires with axis error < 4°.
10. **Cup drop**: free-fall freeze prevents any score change until re-grab.
11. **Occlusion**: a satellite behind the head is not drawn until the head turns.
12. **Performance**: 5 satellites + beam + cloak FX sustain 90 fps.
13. **Stabilize**: second-finger brace widens the cone to 8.4° for exactly 4 s, then 10 s cooldown.
14. **Registration**: 20 presentations report `R` within ±0.4 mm.
