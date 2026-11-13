# Fingerboard Bowl

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Fingerboard Bowl |
| Slug | `fingerboard_bowl` |
| Physical prop | Empty cereal bowl, concave-up; `R = 75.0 mm`, depth `55 mm`, wall `t = 4.0 mm`, mass `380 g` |
| Family / sub-shape | Hemisphere, bowl-up, hand scale |
| Collider family | Concave inner hemisphere + rim torus + foot ring |
| Scale class | Prop 1:1 (150 mm aperture); virtual pool 600 mm diameter |
| Source concept quote | "Slide your index and middle fingertips down and up the smooth curved interior walls of the bowl, tapping the rim lip at the apex of each curve... Your two fingers control a miniature hover-skater carving around the curved walls; tap the rim lip to execute aerial hand-plants and kickflips over floating holographic drones." |
| Physical input | Index + middle fingertips slide on the inner wall; rim-lip tap to launch |
| Session length | 7 min (5 runs × 70 s + onboarding) |
| Lose condition | Three bail-outs |

## 1. Fantasy & Core Loop

The bowl becomes a neon-lit concrete skatepark pool at 1:4 scale: a deep kidney bowl with graffiti, coping, and floating holographic drones hovering over the lip. The player's index and middle fingertips are the skater's feet. Dragging them down the curved wall builds speed; dragging them sideways carves; lifting and tapping the lip within 25 mm of the edge launches an aerial. A kickflip requires the two fingertips to cross while airborne. The skater is a hover-board with a glowing trail, and each drone is a gate that must be passed through or flipped over.

1. **Drop in (1.0 s).** The fingertips start at the rim; a downward slide accelerates the skater.
2. **Carve (3–6 s).** Sideways fingertip drag sets the wall angle and speed; the skater's path is a 2-D curve on the hemisphere.
3. **Build (1.5 s).** Reaching the coping at speed arms an aerial.
4. **Launch (0.2 s).** A rim tap within 25 mm of the lip fires the aerial; vertical speed from the carve speed.
5. **Trick (0.4–0.8 s).** Crossed fingers = kickflip; spread fingers = hand-plant; pass a drone gate for score.
6. **Land (0.3 s).** Both fingertips must re-enter the wall within 180 ms of each other or the run bails.

A line is 6–11 s; a run is 70 s; five runs fill the session.

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

- **Radius and wall**: `R = 75.0 mm`, depth `55 mm`, wall `t = 4.0 mm`, mass `m = 380 g`. The bowl is deeper than a true hemisphere (`depth < R`), so the inner surface is a spherical cap with `θ_max = acos(1 − 55/75) = 42.8°` from the apex.
- **Orientation**: bowl-up. Cavity faces `+ẑ_P`; inner surface `p_P = R·(sinθ cosλ, sinθ sinλ, cosθ)`, `θ ∈ [0°, 42.8°]`. Apex (bottom) `A_P = (0,0,+R)`; rim at `z_P = R − 55 = +20 mm` above the geometric centre.
- **P origin**: rim-circle centre at `z_P = 0`; the apex lies at `z_P = +75 mm`? No — set `P` origin at the cavity bottom instead: `P_O = A_P = (0,0,0)`, rim at `z_P = −55 mm`, outward `+ẑ_P` pointing *down* into the bowl; this is the recurrence used by the skater solver. The rim circle radius is `R_rim = R·sin(42.8°) = 50.9 mm`.
- **V-frame**: anchored to `P` (the bowl interior rotates with the bowl). `V` origin at the cavity bottom, `x̂_V = x̂_P`, `ŷ_V = ŷ_P`, `ẑ_V = −ẑ_P` (up out of the bowl). A pxel-to-mm scale of ×4 is used for the skatepark visual.
- **Skater state**: a point on the inner hemisphere `(θ_s, λ_s)`, with tangential speed `v_s`.
- **Deviation 1**: the two fingertips are not physically constrained; their path on the inner surface is raycast to `(θ, λ)` and drives a damped 2-D particle on the spherical cap.
- **Deviation 2**: vertical aerial motion is a ballistic offset `h(t)` above the surface, not a full 3-D rigid body.
- **Deviation 3**: tricks are classified from finger contact geometry (crossed/spread) during the aerial window.
- **Deviation 4**: drones are virtual gates on a 600 mm pool sphere; they never collide with the real bowl.

## 3. Physical Play Space & Ergonomics

The bowl rests on the lap or a low tray, tilted 10° toward the player so the interior is visible. The right hand reaches in with index and middle extended, thumb braced on the rim. The left hand steadies the bowl. The head tilts 25–32° down.

| Parameter | Value | Rationale |
|---|---|---|
| Bowl position | Lap, 200–280 mm in front of sternum | Reachable interior |
| Bowl tilt | 10° toward player | Visibility |
| Finger extension | Index/middle MCP 20–40° | Comfortable reach |
| Slide speed | 20–180 mm/s | Carve control |
| Rim tap force | 0.4–1.2 N | Detectable, not destabilizing |
| Aerial window | 0.4–0.8 s | Trick time |
| Gaze | 25–32° down, 350–500 mm | Neck comfort |
| Session | 7 min; 70 s runs; 12 s rest | Finger stamina |

Fatigue mitigation: the bowl is on the lap so the arm hangs naturally; the index/middle fingers do the work, the wrist stays neutral; each run ends with a 6 s hand-open rest; a fingertip-load meter warns after 45 s of continuous wall contact.

## 4. Prop Geometry & Registration

A standard glazed cereal bowl: `150 mm` aperture, `55 mm` deep, with a small foot ring. The glaze is glossy, giving strong specular cues for the inner-surface fit.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 75.0 mm ± 1.0 | Silhouette |
| Depth | 55 mm ± 1.5 | Apex to rim |
| Wall `t` | 4.0 mm ± 0.5 | Rim step |
| Inner radius `R_in` | 71.0 mm | `R − t` |
| Rim circle radius | 50.9 mm | `R·sin 42.8°` |
| Mass | 380 g ± 20 | Balance |
| Foot ring | 36 mm radius, 3 mm high | Support |
| Registration | rim ellipse + inner specular fit | 30 frames |
| Re-acquisition | `ε_R < 2.0 mm`, `ε_θ < 3°` | After blur |

Feature atlas (inner surface):

| Station | `θ` from apex | Feature |
|---|---|---|
| S0 | 0–8° | Flat bottom / pool floor |
| S1 | 8–25° | Lower transition (carve zone) |
| S2 | 25–38° | Vertical wall |
| S3 | 38–42.8° | Coping (launch zone) |
| S4 | rim | Lip (tap target) |

Colliders: inner spherical cap (`R_in`), rim torus, foot ring. Registration publishes `P` and the interior normal at 72 Hz.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `R_index_tip` | S1–S2 wall | Slide | `FootL(θ, λ, v)` |
| I5.2 | `R_middle_tip` | S1–S2 wall | Slide | `FootR(θ, λ, v)` |
| I5.3 | `R_index_tip` + `R_middle_tip` | S3 coping | Rim tap within 25 mm | `Launch` |
| I5.4 | `R_index_tip`/`R_middle_tip` | aerial | Cross paths | `Kickflip` |
| I5.5 | `R_index_tip`/`R_middle_tip` | aerial | Spread > 40 mm | `HandPlant` |
| I5.6 | `L_thumb_tip` | rim (outer) | Steady press | `Hold` |
| I5.7 | either hand | — | 800 ms off-bowl | `Rest` |

The skater position blends the two fingertips: `θ_s = (θ_i + θ_m)/2`, `λ_s` likewise, with the separation `d_f = |p_i − p_m|` used for trick classification.

## 6. Input Kinematics & Signal Processing

- **Surface projection**: each fingertip `p` is raycast from `P_O` onto the `R_in` sphere: `θ = acos(p̂·ẑ_P)`, `λ = atan2(p̂·ŷ_P, p̂·x̂_P)`.
- **Skater dynamics**: `v̇_s = g_eff·sinθ_s − c_d·v_s² − µ·g_eff·cosθ_s`, with `g_eff = 2.4 m/s²` (toy gravity), `c_d = 0.08 m⁻¹`, `µ = 0.12`. Terminal speed ≈ `sqrt(g_eff·sinθ/c_d)`; at `θ = 30°`, `v_max ≈ 3.9 m/s`.
- **Steering**: `λ̇_s = k_steer·(λ_f − λ_s)`, `k_steer = 6 s⁻¹`; the fingertip azimuth leads the skater.
- **Launch**: rim tap with `θ ≥ 38°` and `v_s ≥ 1.6 m/s`; vertical speed `v_h = 0.42·v_s·sin(θ_s − 38°)` clamped to `[0.3, 2.2] m/s`.
- **Aerial**: `h(t) = v_h·t − 0.5·g_eff·t²`; apex at `t = v_h/g_eff`, airtime `2v_h/g_eff = 0.4–0.8 s`.
- **Trick classifier**: during airtime, if `|p_i − p_m| < 8 mm` and the tips cross in `λ`, `Kickflip`; if `|Δp| > 40 mm`, `HandPlant`; else `Straight`.
- **Landing**: both tips must return within 180 ms; else `Bail`.

## 7. Virtual World Layout & Coordinate Mapping

`V` is the bowl interior scaled ×4 for the skatepark render, but physics runs on the true `R_in`. The pool is a spherical cap; the coping is the `θ = 38–42.8°` band; the rim circle is the tap plane.

| Entity | Anchoring | Notes |
|---|---|---|
| Pool | `P` | `R_in = 71 mm` cap |
| Skater | `P` | `(θ_s, λ_s)` + `h` |
| Drones | `P` | floating at `θ ≈ 30°, h ≈ 40 mm` |
| Trail | `P` | last 2 s of path |
| Coping band | `P` | launch zone |
| Score gates | `P` | 3 per run |

The camera follows the skater with a 12° downward bias; the bowl itself is always visible in the lower field.

## 8. Gameplay Systems & Rules

**Speed.** `v_s` from the dynamics above; displayed as `v_s/4` m/s "scale speed".

**Lines.** A line is a continuous carve without bailing. Each 1 s of carving scores `10·v_s`.

**Aerial score.** Base `120`; kickflip `×1.5`; hand-plant `×1.8`; passing a drone gate during the aerial `+80`. A clean landing (both tips within 180 ms) adds `+40`.

**Bail.** If one fingertip leaves the bowl for > 180 ms, or the skater's `θ < 4°` at `v_s > 2.0 m/s` (slams the flat), the run bails: lose one of three lives and reset to the rim.

**Combo.** Consecutive clean tricks multiply by `1 + 0.25·n` (cap 4). A bail resets.

**Run.** 70 s or 3 bails. Score is the total.

**Win.** Clear 5 runs with ≥ 1 life left. Final bonus `+300·lives`.

**Gates.** Three holographic drones per run at fixed `(θ, λ)`; passing through the 40 mm gate during an aerial awards the gate bonus.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | bowl seen | `DropIn` | 2.0 s | `P` locked |
| `DropIn` | calibration / reset | `Carve` | 1.0 s | skater at rim |
| `Carve` | drop-in | `Launch` / `Bail` | continuous | `v_s`, `θ` |
| `Launch` | coping tap | `Aerial` | 0.2 s | `v_h` |
| `Aerial` | launch | `Trick` / `Land` | 0.4–0.8 s | `h(t)` |
| `Trick` | aerial, fingers cross/spread | `Land` | 0.2 s | trick type |
| `Land` | `h = 0` | `Carve` / `Bail` | 0.18 s | landing check |
| `Bail` | miss/slam | `DropIn` / `RunEnd` | 1.2 s | tumble |
| `RunEnd` | 70 s or 3 bails | `DropIn` / `Victory` | 2.5 s | tally |
| `Victory` | 5 runs | `Score` | 2.0 s | fanfare |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Inner cap | `R_index_tip` | Slide sensor | ceramic/skin |
| Inner cap | `R_middle_tip` | Slide sensor | ceramic/skin |
| Rim torus | Fingertip tap | Launch trigger | ceramic/skin |
| Skater particle | Drone gate | Score trigger | — |
| Skater particle | Coping band | Launch eligibility | — |
| Bowl | Lap/tray | Support | ceramic |

The skater is a point mass on a sphere; no rigid-body rotation. Contacts are penalties only for presence, not force. CCD not required: fingertip speed ≤ 400 mm/s → 4.4 mm/tick. The `θ < 4°` slam test uses a 2 mm band. Airtime is analytic, so no integration error accumulates.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → lap → bowl exterior → pool surface shader → graffiti → skater + trail → drones → coping glow → hand mesh → spark particles → UI.

Occlusion cases:
1. **Fingers in the bowl**: the tips are rendered inside the cavity; the near rim occludes them when they are on the far wall.
2. **Rim over the wrist**: the wrist is hidden behind the near rim, which sells the depth.
3. **Skater behind the rim**: the skater's trail is clipped at the rim when it is low in the bowl.
4. **Drones above the lip**: drones float above the rim and render in front of the near hand only when closer.
5. **UI**: the run timer floats 120 mm above the bowl and is soft-occluded by the forearm.
6. **Aerial**: at the apex the skater is drawn above the rim, so the hand can pass behind it.

VFX budget: skater + trail 1.2 k tris, 3 drones × 400 tris, graffiti 1 decal, sparks ≤ 300.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Drop in | Wheel rumble | Speed lines |
| Carve | Continuous grinding | Trail bend |
| Coping tap | Hollow knock | Coping flash |
| Launch | Whoosh | Skater rises |
| Kickflip | Board flip clack | Red rotation arc |
| Hand-plant | Palm slap | Blue shockwave |
| Gate pass | Chime | Gate dissolves |
| Land | Wheel thud | Dust ring |
| Bail | Crash + slide | Tumble FX |
| Run end | Crowd cheer | Score card |

## 13. Difficulty Progression & Tuning

| Level | Toy gravity | Drone count | Gate size (mm) | Airtime window (s) | Bail threshold (ms) |
|---|---|---|---|---|---|
| 1 | 2.0 | 2 | 50 | 0.8 | 260 |
| 2 | 2.2 | 3 | 45 | 0.7 | 220 |
| 3 | 2.4 | 3 | 40 | 0.6 | 180 |
| 4 | 2.6 | 4 | 35 | 0.5 | 150 |
| 5 | 2.8 | 4 | 30 | 0.45 | 120 |

**Adaptive rule.** Track clean-trick rate `c` over the last 8 aerials. If `c ≥ 0.8`, shrink the gate 5 mm or add a drone. If `c ≤ 0.35`, widen the gate and increase the landing window by 30 ms. Never change both in one run.

## 14. Edge Cases & Failure Recovery

- **One fingertip only**: the skater steers on the single active tip; the other is ghosted until it re-enters.
- **Both tips off the bowl > 180 ms**: bail.
- **Bowl moved mid-run**: re-anchor `V` to `P`; the skater keeps `(θ, λ)` relative coordinates.
- **Fingers collide**: no hand–hand collision; the trick classifier uses the crossing.
- **Tracking loss**: freeze skater for ≤ 300 ms; longer pauses.
- **Tap on the outer rim**: not a launch; treated as a steadying tap.
- **Skater stuck at the apex**: if `v_s < 0.2 m/s` for 2 s, nudge to `θ = 12°` to restart.
- **Bowl tipped > 25°**: launch is disabled and the run pauses until re-levelled.

## 15. Implementation Architecture

Classes: `PropRegistration`, `FingertipProjector`, `SkaterParticle`, `CopingDetector`, `AerialModel`, `TrickClassifier`, `DroneGate`, `BailJudge`, `RunDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P`; `FingertipProjector → (θ, λ)`; `SkaterParticle` integrates; `CopingDetector → Launch`; `AerialModel → h(t)`; `TrickClassifier → TrickEvent`; `BailJudge → RunEnd`; all → `ScoreLedger`, `AudioCueBus`.

```text
onFixedTick(dt = 1/90):
    (ti, tm) = project(R_index_tip, R_middle_tip, P)
    theta = 0.5*(ti.theta + tm.theta)
    lam   = 0.5*(ti.lam   + tm.lam)
    v += (g_eff*sin(theta) - cd*v*v - mu*g_eff*cos(theta)) * dt
    lam += k_steer*(lam_finger - lam) * dt
    if launched:
        h = vh*t - 0.5*g_eff*t*t
        if h <= 0: land()
    if theta < 4deg and v > 2.0: bail("slam")
```

```text
onRimTap(tip):
    if theta_s >= 38deg and v >= 1.6:
        vh = clamp(0.42*v*sin(theta_s - 38deg), 0.3, 2.2)
        launched = true; t = 0
        audio.play("coping_knock"); vfx.copingFlash()
    else:
        audio.play("rim_tick")
```

## 16. Acceptance Test Checklist

1. **Projection**: a fingertip at a known `(θ, λ)` re-projects within 1.5 mm on `R_in`.
2. **Terminal speed**: at `θ = 30°`, `v_s` converges to 3.9 ± 0.2 m/s.
3. **Launch eligibility**: a rim tap at `θ = 35°` does not launch; at `θ = 40°`, `v = 1.8 m/s` it does.
4. **Airtime**: `v_h = 1.0 m/s` yields 0.83 s of air with `g_eff = 2.4`.
5. **Kickflip**: crossed tips within 8 mm classify as `Kickflip` in ≥ 95% of trials.
6. **Hand-plant**: tips spread > 40 mm classify as `HandPlant`.
7. **Landing**: both tips returning within 180 ms is clean; 250 ms bails.
8. **Slam**: `θ < 4°` at `v_s > 2.0 m/s` bails the run.
9. **Gate**: passing within the gate radius during an aerial awards `+80`.
10. **Combo**: two clean tricks yield a 1.25× multiplier.
11. **Lives**: three bails end the run; two bails continue.
12. **Re-anchor**: moving the bowl mid-run preserves relative `(θ, λ)`.
13. **Occlusion**: a far-wall fingertip is hidden by the near rim.
14. **Performance**: skater + 4 drones + trail sustain 90 fps.
