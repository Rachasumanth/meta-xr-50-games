# Temple Bell

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Temple Bell |
| Slug | `temple_bell` |
| Physical prop | Metal bottle cap balanced dome-up on a fingertip; `R = 15.0 mm`, `t = 0.5 mm`, mass `2.0 g` |
| Family / sub-shape | Hemisphere, dome-up on a dynamic base, pinch scale |
| Collider family | Convex dome + rim torus + fingertip support point |
| Scale class | Prop 1:1 (30 mm diameter); virtual temple 300 mm radius |
| Source concept quote | "Balance the dome-up metal cap on the upturned pad of your left middle finger and lightly tap its outer rim with your right fingernail... Each light fingernail tap emits a visible golden acoustic ring that pushes back the shadows; angle your left finger to focus the sound wave toward boss spirits." |
| Physical input | Left middle fingertip balances the cap; right fingernail taps the rim |
| Session length | 6 min (5 meditations × 55 s + onboarding) |
| Lose condition | Cap falls three times or shadows reach the bell |

## 1. Fantasy & Core Loop

A candle-lit temple courtyard is rendered around the player's hands. The bottle cap becomes a miniature bronze bell, its knurled rim turning into a ring of Sanskrit glyphs. Shadow spirits advance in slow rings from the courtyard edge, and each tap of a fingernail on the rim sends out a golden acoustic ring. The ring's power and direction depend on where the cap is struck and how the left finger is tilted: the cap is also an antenna, and its dome normal focuses the wave toward whichever spirit is brightest. But the cap is balanced on one fingertip, so every tap perturbs it; the player must re-centre it with tiny finger tilts before it topples.

1. **Balance (continuous).** The left middle finger holds the cap; small corrections keep `β < 12°`.
2. **Read the advance (2–4 s).** Three to six spirits drift inward at 8–20 mm/s.
3. **Tap (0.15 s).** The right nail strikes the rim at azimuth `λ_t`; an acoustic ring emits from the strike point.
4. **Focus (0.5 s).** The ring's directional gain is `cos⁴(ψ)` where `ψ` is the angle between the dome normal and the spirit direction.
5. **Push (0.6 s).** Spirits inside the ring's radius are pushed back 20–50 mm and may dissipate.
6. **Recover (0.8 s).** The tap imparts a wobble; the player re-centres before the next wave.

A meditation is 55 s with 8–14 taps; five meditations fill the session.

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

- **Radius and wall**: `R = 15.0 mm`, `t = 0.5 mm`, mass `m = 2.0 g`. The cap is modelled as a spherical cap with a 24 mm rim.
- **Orientation**: dome-up. Convex exterior faces `+ẑ_P`; the cap rests on the fingertip at its dome apex `A_P = (0,0,+R)`.
- **P origin**: geometric centre of the rim circle. The support point is `A_P`; the balance pivot is the fingertip pad centre `S_W`.
- **Balance model**: the cap is an inverted pendulum about `S_W` with `COM` at `C_P = (0,0,0)`, `d_com = 7.5 mm` above the pivot when upright. Tilt `β` is the angle between `ẑ_P` and `Ŷ_W`.
- **V-frame**: anchored to `T` at the temple centre `Q`; `V` is a 300 mm-radius courtyard disc at `z_V = 0` (lap level). The bell is drawn at `P`, which floats above the lap.
- **Deviation 1**: the physical fingertip balance is real; the virtual cap is simulated as a 2-DoF inverted pendulum coupled to the tracked fingertip position `S_W`.
- **Deviation 2**: taps are detected on the rim but never apply a real impulse; they inject a virtual perturbation `Δω` into the pendulum.
- **Deviation 3**: acoustic rings are 2-D annuli in `V`; their directional gain uses the dome normal `d̂ = q_P·ẑ_P`.
- **Deviation 4**: spirits are `V`-space agents; they never collide with the real hand.

## 3. Physical Play Space & Ergonomics

The left hand is held palm-up at chest height, elbow supported; the cap sits on the middle-finger pad, which is kept as flat as possible. The right hand reaches in with the index fingernail. The player's head tilts 15–22° down. The whole pose is low-load except for the constant micro-corrections of the balancing finger.

| Parameter | Value | Rationale |
|---|---|---|
| Hold height | 250–350 mm below eye | Comfortable |
| Balance tilt | ≤ 12° gameplay, 20° fall | Inverted-pendulum margin |
| Finger support | Left middle pad, 14 mm contact | Stable base |
| Tap force | 0.08–0.25 N | Nail tap, no topple |
| Tap rate | ≤ 1.2 taps/s | Prevents constant perturbation |
| Gaze | 15–22° down, 300–420 mm | Both hands in view |
| Session | 6 min; 55 s meditations; 15 s rest | Balance stamina |

Fatigue mitigation: the elbow is supported; the cap is 2 g so the finger carries almost nothing; the balance tolerance is generous (12°); each meditation ends with a 5 s "lower hand" rest; a wobble meter warns before a fall.

## 4. Prop Geometry & Registration

A knurled crown cap balanced on its convex side. Because it is small and shiny, registration relies on the rim ellipse and the dome highlight, with a fallback to the fingertip anchor.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 15.0 mm ± 0.4 | Rim |
| Dome sagitta | 2.5 mm | Balance geometry |
| Wall `t` | 0.5 mm ± 0.1 | Steel |
| Mass | 2.0 g ± 0.2 | Balance |
| COM height above pivot | 7.5 mm | `d_com` |
| Moment of inertia `I_p` | `0.5·m·R² = 2.25×10⁻⁷ kg·m²` | About pivot |
| Registration | rim ellipse + dome normal | 24 frames |
| Re-acquisition | `ε_R < 1.0 mm`, `ε_β < 3°` | After blur |

Feature atlas:

| Station | Region | Feature |
|---|---|---|
| S0 | dome apex | Balance point (smooth) |
| S1 | dome 0–50° | Specular band |
| S2 | dome 50–80° | Knurl transition |
| S3 | rim torus | Tap targets / glyphs |
| S4 | inner cavity | Sensor only |

Colliders: dome cap (support), rim torus (tap band), and a point support at `A_P`. Registration publishes `P` and the dome normal at 72 Hz; if confidence < 0.5, the fingertip anchor `S_W` is used with a 4° uncertainty.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `L_middle_tip` | S0 apex | Support + micro-tilt | `BasePose(S_W, n̂_b)` |
| I5.2 | `R_index_tip` | S3 rim | Nail tap 0.08–0.25 N | `Tap(λ_t, F)` |
| I5.3 | `R_index_tip` | S2 dome | Soft brush | `Damp` (reduces wobble) |
| I5.4 | `L_index_tip` | S3 rim (left side) | Steadying press | `Stabilize` |
| I5.5 | `L_ring_tip` | S3 rim | Second support | `Brace` |
| I5.6 | both hands | — | 1.0 s open | `Rest` |

Tap azimuth `λ_t = atan2(y_t − 0, x_t − 0)` in `P`. Tap force from the nail contact estimate; taps below 0.05 N are ignored as noise.

## 6. Input Kinematics & Signal Processing

- **Pendulum**: `β̈ = (g·d_com/I_p)·sinβ − (c_p/I_p)·β̇ + u_tap`, with `g·d_com = 0.0736 m²/s²`, `c_p = 1.6×10⁻⁵ N·m·s/rad`. Natural frequency `ω_n = sqrt(g·d_com·m/I_p) ≈ sqrt(0.0736/ (2.25×10⁻⁷/0.002))`; numerically `ω_n ≈ 25.6 rad/s` (4.1 Hz). Damping ratio `ζ ≈ 0.14`.
- **Base correction**: the player's fingertip acceleration `a_S` enters as `u_base = −(a_S × ẑ_P)/d_com`; the virtual cap stays on the fingertip because `S_W` is tracked.
- **Tap impulse**: `Δω = (r_t × F_t·n̂_t)/I_p`; with `r_t = 15 mm`, `F_t = 0.15 N`, `Δt = 40 ms`, `Δω ≈ 0.40 rad/s` — a small wobble that decays in ~1.4 s.
- **Tap detection**: contact dwell 20–60 ms, `F ∈ [0.08, 0.30] N`; debounce 140 ms.
- **Ring emission**: at tap, `R_ring(t) = 40 + 220·t` mm (expands at 220 mm/s for 0.55 s).
- **Directional gain**: `G(ψ) = cos⁴ψ`; `ψ` between `d̂` and the spirit direction from `P_O`.
- **Spirit push**: `Δs = 22·G·(1 − r/R_ring)` mm along the outward radial, plus 1 hp if `G > 0.6`.
- **Fall test**: `β > 20°` for 120 ms ⇒ fall.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 300 mm-radius courtyard disc centred at `Q` below the bell. The bell floats at `P_O = Q + (0, 0, 180 mm)`; the courtyard is a stone mandala. Spirits spawn at `r = 260 mm` and advance at 8–20 mm/s toward `r = 0`.

| Entity | Anchoring | Notes |
|---|---|---|
| Courtyard | `T` at `Q` | radius 300 mm |
| Bell | `P` | floats 180 mm up |
| Spirits | `V` | 3–6 agents |
| Acoustic rings | `P_O` | expanding annuli |
| Focus cone | `P` | `cos⁴` lobe |
| Candle FX | `V` | ambience |

The dome normal `d̂` is the focus direction; the ring is a flat annulus perpendicular to `d̂`, so tilting the finger aims the wave like a searchlight.

## 8. Gameplay Systems & Rules

**Spirits.** Each has `r`, `θ`, `hp = 2`, speed 8–20 mm/s. They advance toward the bell; if any reaches `r < 30 mm`, the bell is "darkened" and the meditation fails.

**Tap ring.** Emits an annulus with radius `R_ring(t)` and thickness 18 mm. A spirit is inside if `|r_s − R_ring| < 9 mm`. Push `Δs = 22·G·(1 − r_s/300)`; if `G > 0.6`, hp −1.

**Focus.** `G = cos⁴ψ`; a perfect focus (`G > 0.85`, ψ < 13.6°) awards `+50` and a "resonant" ring that deals 2 hp.

**Wobble management.** Each tap adds `Δω`; if `β > 12°`, the ring radius shrinks by `cosβ` and the gain is multiplied by `0.7`. Damping brush (`R_index` on dome) reduces `β` at 6°/s.

**Score.** `S = 100·spirit_kills + 50·resonant_rings + 30·(1 − maxβ/12°)`. A fall costs one of three "bells".

**Win.** Complete 5 meditations with ≥ 1 bell left. **Lose.** Three falls or a spirit reaches the bell.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | cap seen | `Balance` | 2.0 s | `P`, `d̂` |
| `Balance` | calibration | `Tap` / `Fall` | continuous | wobble |
| `Tap` | wrist tap | `Ring` | 0.15 s | ring emitted |
| `Ring` | emission | `Balance` | 0.55 s | push |
| `Fall` | `β > 20°` 120 ms | `Recover` | 0.4 s | cap drops |
| `Recover` | fall | `Balance` / `Score` | 1.2 s | re-balance |
| `MeditationClear` | 55 s | `Balance` / `Victory` | 2.0 s | tally |
| `Victory` | 5 meditations | `Score` | 2.0 s | gong |
| `Score` | victory / 3 falls | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Dome apex | `L_middle_tip` | Support (tracked) | steel/skin |
| Rim torus | `R_index_tip` | Tap impulse | steel/nail |
| Cap body | Virtual floor | Fall trigger at `z < 20 mm` | — |
| Ring annulus | Spirit | Push trigger | — |
| Focus cone | Spirit | Gain test | — |
| Dome | Damping brush | `β̇` reduction | — |

Pendulum constants: `I_p = 2.25×10⁻⁷ kg·m²`, `m = 2 g`, `d_com = 7.5 mm`, `c_p = 1.6×10⁻⁵ N·m·s/rad`. The cap is a 2-DoF body (tilt `β`, azimuth `φ`). No translation dynamics: `P_O` follows the fingertip `S_W` plus the 180 mm display offset. CCD is not required (tilt rates ≤ 30 °/s, 0.33°/tick). A fall is a state transition, not a rigid-body drop.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → lap → courtyard mandala → spirits → candles → bell (cap) → glyph rim → rings (additive) → hand mesh → petals → UI.

Occlusion cases:
1. **Finger under the cap**: the middle fingertip is hidden by the dome; only the pad edge shows.
2. **Nail on the rim**: the right index nail renders in front of the rim; the tap spark is clipped by the nail.
3. **Spirit behind the bell**: correct occlusion as a spirit passes behind the floating cap.
4. **Ring through the hand**: the additive ring is depth-tested so it fades behind the fingers.
5. **UI**: the meditation timer sits at `z_V = 260 mm` and is soft-occluded by the forearm.
6. **Fall**: when the cap falls, the virtual bell fades and a ghost ring marks the last stable pose.

VFX budget: bell 1.2 k tris, 6 spirits × 400 tris, rings 1 additive mesh pool, petals ≤ 300 particles.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Balance wobble | Low hum rising | Bell tilt glow |
| Tap | Bronze ping | Ring emits |
| Resonant tap | Pure bell tone | Gold ring |
| Spirit push | Airy whoosh | Spirit recoils |
| Spirit dissipate | Chime | Ash petals |
| Fall | Bell clatter | Cap tumbles |
| Recover | Soft gong | Cap re-centres |
| Meditation clear | Temple choir | Lanterns rise |
| Victory | Great gong | Golden mandala |

## 13. Difficulty Progression & Tuning

| Level | Spirits | Spirit speed (mm/s) | Fall threshold | Ring speed (mm/s) | Tap window (ms) |
|---|---|---|---|---|---|
| 1 | 3 | 8 | 24° | 200 | 80 |
| 2 | 4 | 11 | 21° | 210 | 70 |
| 3 | 5 | 14 | 18° | 220 | 60 |
| 4 | 6 | 17 | 16° | 230 | 55 |
| 5 | 6 | 20 | 14° | 240 | 50 |

**Adaptive rule.** Track hits per tap `h`. If `h ≥ 0.75` over 10 taps, raise spirit speed 1 mm/s or add one spirit (cap 6). If `h ≤ 0.35`, slow spirits and widen the fall threshold by 1°. Never change both in one meditation.

## 14. Edge Cases & Failure Recovery

- **Cap dropped**: on `β > 20°` the virtual bell falls; the real cap is still in hand, so re-balance is prompted with a ghost pose.
- **Tracking loss of the left middle finger**: hold the pendulum state for ≤ 300 ms; longer pauses.
- **Tap too hard**: `F > 0.30 N` injects a large `Δω` and may cause a fall; no artificial clamp, to preserve risk.
- **Double tap**: debounce 140 ms; the second tap within the window is ignored.
- **Spirit at the bell**: fail the meditation, but keep the bell count.
- **Cap slips off the finger**: detected by `S_W` no longer supporting `P`; treat as a fall.
- **Player uses the ring finger as a brace**: `Brace` reduces wobble by 30% but disables resonant bonus.
- **Pause during a ring**: rings freeze and resume; no score change.

## 15. Implementation Architecture

Classes: `PropRegistration`, `BalancePendulum`, `TapDetector`, `RingEmitter`, `FocusModel`, `SpiritAgent`, `SpiritDirector`, `MeditationDirector`, `BellInventory`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P, d̂`; `TapDetector → TapEvent`; `RingEmitter → Ring`; `FocusModel → G`; `Ring + SpiritAgent → PushEvent`; `BalancePendulum → β`; `MeditationDirector → ScoreLedger`, `AudioCueBus`.

```text
onFixedTick(dt = 1/90):
    aS = fingertipAccel(L_middle_tip)
    u = -(cross(aS, P.axis)) / d_com
    betaDD = (g*d_com/Ip)*sin(beta) - (cp/Ip)*betaD + u
    betaD += betaDD * dt
    beta  += betaD * dt
    if beta > 20deg: fallTimer += dt else fallTimer = 0
    if fallTimer > 120ms: state = Fall
    ring = ringEmitter.step(dt)
    for s in spirits:
        if ring.contains(s.r):
            G = pow(max(0, dot(P.axis, dirTo(s))), 4)
            s.push(22 * G * (1 - s.r/300))
            if G > 0.6: s.hp -= 1
```

```text
onTap(lambda_t, F):
    r = R * (cos(lambda_t), sin(lambda_t), 0)
    impulse = cross(r, F * rimNormal(lambda_t))
    betaD += length(impulse) / Ip
    ringEmitter.emit(P.position, P.axis, speed=220mm/s)
    audio.play("bronze_ping", pan=lambda_t)
```

## 16. Acceptance Test Checklist

1. **Pendulum frequency**: a 5° initial tilt oscillates at 4.1 ± 0.4 Hz and decays with `ζ ≈ 0.14`.
2. **Tap impulse**: a 0.15 N, 40 ms tap raises `β̇` by 0.4 ± 0.06 rad/s.
3. **Fall threshold**: `β = 21°` for 120 ms triggers a fall exactly once.
4. **Ring speed**: the annulus radius grows at 220 ± 10 mm/s.
5. **Gain law**: `ψ = 0°, 15°, 30°, 45°` gives `G = 1.00, 0.87, 0.56, 0.25 ± 0.02`.
6. **Resonant**: `G > 0.85` deals 2 hp and awards `+50`.
7. **Push**: a spirit at `r = 150 mm` with `G = 0.9` is pushed 8.4 ± 1 mm.
8. **Spirit loss**: a spirit reaching `r < 30 mm` fails the meditation.
9. **Debounce**: taps 100 ms apart register once; 200 ms apart register twice.
10. **Tap force window**: 0.04 N is ignored; 0.35 N is registered as a hard tap.
11. **Brace**: the ring finger reduces wobble 30% and disables the resonant bonus.
12. **Pause**: palm cover freezes rings and spirits without score change.
13. **Occlusion**: the supporting fingertip is hidden by the dome in 100% of calibrated frames.
14. **Performance**: 6 spirits + rings + bell sustain 90 fps.
