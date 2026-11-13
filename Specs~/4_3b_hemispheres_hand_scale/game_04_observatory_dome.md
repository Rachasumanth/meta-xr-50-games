# Observatory Dome

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Observatory Dome |
| Slug | `observatory_dome` |
| Physical prop | Inverted cereal bowl, dome-up on the lap; `R = 75.0 mm`, depth `55 mm`, wall `t = 4.0 mm`, mass `380 g` |
| Family / sub-shape | Hemisphere, dome-up, hand scale |
| Collider family | Convex dome + rim torus + foot ring (upward) |
| Scale class | Prop 1:1 (150 mm aperture); virtual sky dome 900 mm radius |
| Source concept quote | "Place the bowl upside-down on your lap or tray, rotate it horizontally with both hands, and slide two fingers apart down one side of the dome... Sliding two fingers apart opens the dome shutter slit; rotating the bowl swivels the giant brass telescope inside to lock onto streaking UFOs and rogue asteroids." |
| Physical input | Two-hand horizontal rotation of the bowl; two-finger separation down one dome meridian |
| Session length | 6 min (5 nights × 60 s + onboarding) |
| Lose condition | Three targets lost before lock |

## 1. Fantasy & Core Loop

The inverted bowl becomes a whitewashed observatory dome on a mountain ridge, its panels and rivets rendering over the curved ceramic. Inside, a giant brass refractor rides on a counterweighted mount. The night sky is a 900 mm virtual sphere of stars, and three to five targets streak across it: saucer UFOs, tumbling asteroids, and the occasional comet. Rotating the bowl swivels the dome and the telescope together around the vertical axis. Sliding index and middle fingertips apart down one side of the dome opens a shutter slit; the telescope can only see through that slit. A target locks when it is inside the slit for 1.2 s.

1. **Sweep (2–4 s).** Rotate the bowl with both palms to bring the slit near a target's azimuth.
2. **Open (0.6 s).** Slide two fingertips apart along the dome meridian; slit width `w ∈ [0°, 60°]`.
3. **Track (1.2 s).** The target's azimuth must stay within the slit for the full lock time; elevation is handled by the telescope's internal gimbal.
4. **Lock (0.5 s).** The shutter clamps and the target is captured.
5. **Drift (0.3 s).** A new target spawns and the dome must be re-aimed.
6. **Night resolve (2.0 s).** Captures tally and the sky rotates.

A target cycle is 6–10 s; a night is 60 s; five nights fill the session.

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

- **Radius and wall**: `R = 75.0 mm`, depth `55 mm`, wall `t = 4.0 mm`, mass `m = 380 g`.
- **Orientation**: dome-up. Convex exterior faces `+ẑ_P`; cavity opens `−ẑ_P`. Apex `A_P = (0,0,+75.0) mm`; rim at `z_P = +20 mm` above the geometric centre.
- **P origin**: rim-circle centre. At rest on the lap, `P_O` is at the dome's centre and the apex is 75 mm above the lap.
- **Dome yaw**: `ψ = atan2` of the `x̂_P` projection onto the `T` plane; the dome rotates freely about `T`'s vertical.
- **Shutter**: a meridian slit centred at virtual azimuth `α_s` with half-width `w/2`; the slit cuts the dome from `θ = 0°` (apex) to `θ = 60°`.
- **V-frame**: anchored to `T` at the lap centre `Q`; sky sphere centred at `Q + (0,0,600) mm`, radius 900 mm. `P` yaw maps directly to telescope azimuth `α_t = ψ`.
- **Deviation 1**: dome yaw is tracked from `P`, not simulated; there is no rotational inertia because the hands hold it.
- **Deviation 2**: the shutter is a virtual cut-out; the physical dome is never modified. Slit width is a render + raycast mask.
- **Deviation 3**: targets move on the sky sphere in azimuth/elevation; lock is a 2-D azimuth containment test plus an elevation gimbal assumption.
- **Deviation 4**: the two-finger separation is measured as the geodesic distance on the dome surface, not the Euclidean fingertip distance.

## 3. Physical Play Space & Ergonomics

The bowl sits dome-up on the lap, slightly forward. Both palms rest on the dome's lower flanks and rotate it like a steering wheel; the index and middle fingers of the right hand slide apart along the right meridian to open the slit. The head tilts 18–25° down, with glances up at the sky sphere.

| Parameter | Value | Rationale |
|---|---|---|
| Bowl position | Lap, 180–260 mm forward | Two-hand reach |
| Dome height | 75 mm | Palm contact on flanks |
| Rotation range | ±180° continuous | Full sky sweep |
| Rotation rate | 20–90 °/s | Fine aim |
| Slit width | 0–60° (0–104 mm arc) | Opens over 0.6 s |
| Finger separation | 0–70 mm geodesic | Maps to slit width |
| Gaze | 18–25° down + sky glances | Neck comfort |
| Session | 6 min; 60 s nights; 12 s rest | Arm rotation stamina |

Fatigue mitigation: the bowl is on the lap so the arms rest; rotation uses the palms, not the fingers; each night ends with a 6 s hands-off rest; a rotation-odometer suggests reversing direction every 20 s to avoid shoulder twist.

## 4. Prop Geometry & Registration

A glazed ceramic bowl with a foot ring on the bottom, which is now facing up and serves as a convenient centre marker for the telescope mount.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 75.0 mm ± 1.0 | Silhouette |
| Depth | 55 mm ± 1.5 | Apex to rim |
| Wall `t` | 4.0 mm ± 0.5 | Rim step |
| Rim circle radius | 50.9 mm | `R·sin 42.8°` |
| Foot ring | 36 mm radius, 3 mm | Telescope mount marker |
| Mass | 380 g ± 20 | Balance |
| Registration | rim ellipse + dome specular | 30 frames |
| Re-acquisition | `ε_R < 2.0 mm`, yaw error < 4° | After blur |

Feature atlas (outer dome):

| Station | `θ` | Feature |
|---|---|---|
| S0 | 0–10° | Apex cap / foot ring |
| S1 | 10–35° | Upper panels |
| S2 | 35–60° | Shutter track band |
| S3 | 60–85° | Lower panels |
| S4 | 85–90° | Rim |

Colliders: outer dome hemisphere, rim torus, foot ring. The shutter slit is a mask for the telescope raycast, not a hole in the collider. Registration publishes `P` and yaw `ψ` at 72 Hz.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | both palms | S2–S3 flanks | Rotate | `DomeYaw(ψ)` |
| I5.2 | `R_index_tip`, `R_middle_tip` | S1–S2 meridian | Slide apart | `SlitWidth(w)` |
| I5.3 | `L_index_tip`, `L_middle_tip` | S1–S2 meridian | Slide apart | `SlitWidth(w)` (blended) |
| I5.4 | either palm | S0 apex | Press | `ClampLock` |
| I5.5 | both hands | — | Lift 10 mm | `Release` |
| I5.6 | both hands | — | Open 800 ms | `Rest` |

Yaw is the change in the `x̂_P` azimuth: `ψ = unwrap(atan2(x_P·ŷ_T, x_P·x̂_T))`. Slit width uses the right-hand pair if present, else the left.

## 6. Input Kinematics & Signal Processing

- **Yaw estimate**: `ψ` from the `P` quaternion, unwrapped with a 180° continuity guard; low-passed at 6 Hz with a 70 ms latency budget.
- **Yaw rate**: `ψ̇`; target tracking requires `|ψ̇| ≤ 90 °/s`; faster is "slew" and suspends lock accumulation.
- **Slit width**: geodesic separation `d_g = R·angle(p̂_i·p̂_m)`; `w = clamp(2·d_g/R_arc, 0°, 60°)` where `R_arc = 0.87R`; `w` is filtered with a 120 ms slew limit.
- **Aim**: telescope azimuth `α_t = ψ`; target azimuth `α_i`; containment if `|wrap(α_i − α_t)| ≤ w/2`.
- **Lock**: `t_lock += Δt` while contained and `w ≥ 8°` and `|ψ̇| ≤ 90 °/s`; at 1.2 s the target locks.
- **Break**: containment lost for 0.4 s resets `t_lock`.
- **Slit closed**: `w < 8°` blocks all locks (the shutter is shut).
- **Elevation**: targets are assigned an elevation `ε_i ∈ [10°, 70°]`; the telescope gimbal is assumed to follow, so elevation does not affect the player's task.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 900 mm-radius sky sphere centred 600 mm above the lap. Targets are placed by azimuth `α` and elevation `ε`: `s_V = C_V + 900·(cosε cosα, sinε, cosε sinα)`. The dome sits at the sphere's lower pole, small and bright.

| Entity | Anchoring | Notes |
|---|---|---|
| Sky sphere | `T` | 900 mm radius |
| Targets | `V` | 3–5 active |
| Dome | `P` | yaw `ψ` |
| Telescope | `P` + 40 mm up | brass refractor |
| Shutter slit | `P` meridian | `w` wide |
| Lock reticle | `V` | at target |

The dome's azimuthal meridian `x̂_P` is the shutter centre `α_s = ψ`; the slit is drawn from the apex down to `θ = 60°`.

## 8. Gameplay Systems & Rules

**Targets.** Each has `α`, `ε`, angular speed `ω_t = 5–18 °/s`, and `type ∈ {ufo, asteroid, comet}`. They drift in azimuth, occasionally reversing.

**Containment.** `|wrap(α − ψ)| ≤ w/2` and `w ≥ 8°`.

**Lock.** 1.2 s of containment with `|ψ̇| ≤ 90 °/s`. On lock, `ClampLock` fires, the shutters close to `w = 4°`, and the target is captured.

**Score.** `S = 150·(1 + 0.4·n_chain) · q_slit`, where `q_slit = 1.0` if `w ≤ 25°` (narrow slit is harder) and `0.8` otherwise.

**Chain.** Consecutive locks without a break multiply by `1 + 0.3·n` (cap 4).

**Break.** A lock in progress that loses containment for 0.4 s breaks; a target that stays uncontained for 12 s escapes, costing one of three "nerves".

**Night.** 60 s or 3 escapes. Captures score.

**Win.** Clear 5 nights with ≥ 1 nerve left.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | bowl seen | `Sweep` | 2.0 s | `P` locked |
| `Sweep` | calibration / lock | `Open` / `Track` | target present | yaw |
| `Open` | slit closed | `Track` | `w ≥ 8°` | shutters part |
| `Track` | slit open | `Lock` / `Break` | containment | reticle |
| `Lock` | 1.2 s contained | `Capture` | 1.2 s | clamp |
| `Capture` | lock | `Sweep` | 0.5 s | score |
| `Break` | 0.4 s uncontained | `Sweep` | 0.4 s | reticle snaps |
| `Escape` | 12 s uncontained | `Sweep` | 1.0 s | nerve lost |
| `NightClear` | 60 s | `Sweep` / `Victory` | 2.0 s | tally |
| `Victory` | 5 nights | `Score` | 2.0 s | fanfare |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Dome hemisphere | Palms | Rotation input | ceramic/skin |
| Dome hemisphere | Fingertips | Slit input | ceramic/skin |
| Foot ring | Lap/tray | Support (upward) | ceramic |
| Telescope ray | Shutter mask | Occlusion test | — |
| Target | Containment arc | Lock trigger | — |
| Target | Sky sphere | Motion constraint | — |

The dome is held, so no rigid-body dynamics; yaw is kinematic from `P`. The telescope raycast is analytic: a target is visible if its azimuth lies in the slit and its elevation is within the gimbal's `[10°, 70°]`. CCD not required. A yaw-rate clamp of 90 °/s prevents "whip-scan" lock inflation. Slit width is limited to 60° so the dome never appears fully open.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → lap → sky sphere (far) → stars → targets → dome exterior → shutter cut-out → telescope → hand mesh → reticle → UI.

Occlusion cases:
1. **Palms on the dome**: hands are drawn in front of the lower dome panels; the shutter mask does not clip them.
2. **Telescope inside the dome**: the refractor is only visible through the slit; elsewhere it is hidden by the dome depth.
3. **Target behind the dome**: a target at low elevation is occluded by the dome silhouette, which is the intended "can't see below the horizon" effect.
4. **Fingers over the slit**: fingertips sliding on the meridian render over the slit edges.
5. **UI**: the night timer floats at 900 mm on the sky sphere and parallaxes correctly.
6. **Shutter edge**: the cut edge is rendered as a thick white frame so the gap reads clearly.

VFX budget: sky sphere 1 shader, 5 targets × 500 tris, dome 3 k tris, telescope 2 k tris, stars 2 k points.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Dome rotate | Servo whir | Panel motion blur |
| Slit open | Mechanical clunk | Shutter parts |
| Containment | Rising tone | Reticle brightens |
| Lock | Brass latch | Shutter clamps |
| Capture | Camera shutter + chime | Target framed |
| Break | Tuning-drop | Reticle snaps |
| Escape | Low horn | Target fades |
| Night clear | Dawn birds | Sky lightens |
| Victory | Orchestral swell | Constellation map |

## 13. Difficulty Progression & Tuning

| Level | Targets | Target speed (°/s) | Lock time (s) | Slit max | Escape timer (s) |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 0.8 | 60° | 16 |
| 2 | 3 | 8 | 1.0 | 55° | 14 |
| 3 | 4 | 11 | 1.2 | 50° | 12 |
| 4 | 5 | 15 | 1.4 | 45° | 10 |
| 5 | 5 | 18 | 1.6 | 40° | 8 |

**Adaptive rule.** Track capture rate `c` over the last 6 targets. If `c ≥ 0.8`, raise target speed 2 °/s or shrink the slit max by 5°. If `c ≤ 0.35`, slow targets and widen the slit max by 5°. Clamp slit max to `[30°, 70°]`.

## 14. Edge Cases & Failure Recovery

- **Bowl lifted**: if `P` rises > 8 mm, pause yaw input and show a ghost outline; resume on reseat.
- **One hand only**: rotation is allowed with one palm, but slit width needs a two-finger pair; the other hand's last slit value is held for 1.5 s.
- **Slit fully closed during a lock**: `w < 8°` breaks the lock immediately.
- **Target crosses the zenith**: azimuth wrap causes a 360° jump; the unwrap guard handles it.
- **Tracking loss**: freeze yaw for ≤ 400 ms; longer pauses the night.
- **Bowl rotated > 720°**: unwrap continues; no mechanical limit.
- **Conflicting slit inputs**: right hand wins if both pairs are present; ties go to the wider slit.
- **Pause while shutters are moving**: slit freezes mid-travel and resumes.

## 15. Implementation Architecture

Classes: `PropRegistration`, `YawTracker`, `SlitController`, `ShutterMask`, `SkySphere`, `Target`, `TargetDirector`, `ContainmentTest`, `LockAccumulator`, `TelescopeModel`, `NightDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P, ψ`; `SlitController → w`; `Target` moves; `ContainmentTest(ψ, w, α) → contained`; `LockAccumulator → LockEvent`; `TargetDirector → NightDirector`; all → `ScoreLedger`, `AudioCueBus`.

```text
onFixedTick(dt = 1/90):
    psi = yawTracker.update(registration.quat, dt)
    w = slitController.width(hands, dt)
    for t in targets:
        t.alpha += t.omega * dt
        d = wrapPi(t.alpha - psi)
        contained = abs(d) <= w/2 and w >= 8deg
        if contained and abs(yawRate) <= 90deg/s:
            t.lock += dt
            if t.lock >= lockTime: capture(t)
        else:
            t.lock = max(0, t.lock - 2*dt)
            t.uncontained += dt
            if t.uncontained > 12s: escape(t)
```

```text
onCapture(target):
    score.add(150 * (1 + 0.4*chain) * (w <= 25deg ? 1.0 : 0.8))
    chain += 1
    shutter.clamp(4deg)
    audio.play("brass_latch"); vfx.frame(target)
```

## 16. Acceptance Test Checklist

1. **Yaw mapping**: rotating the bowl 90° produces `ψ = 90° ± 2°`.
2. **Yaw wrap**: a 360° rotation unwraps without a visible jump.
3. **Slit width**: a 30 mm geodesic finger separation yields `w = 40° ± 4°`.
4. **Containment**: a target at `α = 10°` is contained at `w = 40°` and not at `w = 12°`.
5. **Lock timing**: 1.2 s of containment captures exactly one target.
6. **Break**: 0.4 s of non-containment resets the lock.
7. **Closed shutter**: `w = 5°` blocks all locks.
8. **Slew guard**: a 120 °/s rotation does not accumulate lock time.
9. **Escape**: 12 s uncontained costs one nerve.
10. **Chain**: two captures without a break yield a 1.3× multiplier.
11. **Re-anchor**: lifting and reseating the bowl preserves yaw within 4°.
12. **Occlusion**: a low-elevation target is hidden by the dome silhouette.
13. **Pause**: palm cover freezes yaw and shutters without score change.
14. **Performance**: 5 targets + sky sphere + dome sustain 90 fps.
