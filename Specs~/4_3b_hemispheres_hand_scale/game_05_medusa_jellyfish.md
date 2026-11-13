# Medusa Jellyfish

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Medusa Jellyfish |
| Slug | `medusa_jellyfish` |
| Physical prop | Inverted bowl, dome-up in both hands; `R = 75.0 mm`, depth `55 mm`, wall `t = 4.0 mm`, mass `380 g` |
| Family / sub-shape | Hemisphere, dome-up, hand scale |
| Collider family | Convex dome + rim torus + palm press zones |
| Scale class | Prop 1:1 (150 mm bell); virtual trench 4 m deep scaled to 600 mm |
| Source concept quote | "Cup both hands lightly over the upside-down dome on your lap and rhythmically rock/press the outer rim downward with your palms... Rhythmic rim presses contract the bell to jet upward, while tilting the dome steers its trailing virtual tentacles away from sea mines." |
| Physical input | Both palms press the rim in a rhythmic contraction; whole-hand tilt steers |
| Session length | 7 min (5 dives × 70 s + onboarding) |
| Lose condition | Oxygen reaches 0 or three mine strikes |

## 1. Fantasy & Core Loop

The inverted bowl becomes the translucent bell of a giant bioluminescent jellyfish, its ceramic surface turned to gel and threaded with glowing canals. The player cups both hands over the dome, and the rim is the bell margin. Pressing both palms down on opposite rim sectors contracts the bell, expelling a ring of blue light and jetting the jellyfish upward. Between contractions the bell relaxes and the creature sinks. The trench is a vertical 600 mm column of dark water with kelp silhouettes and drifting sea mines. Tilting the whole dome steers the trailing tentacles left and right; a tentacle that grazes a mine detonates it and costs oxygen.

1. **Drift (1.0 s).** The bell relaxes; the jellyfish sinks 8 mm/s.
2. **Contract (0.25 s).** Both palms press the rim inward; the bell compresses 6 mm.
3. **Jet (0.6 s).** The bell snaps back and the creature rises 40–90 mm.
4. **Steer (continuous).** Dome tilt `τ` sets a lateral velocity; the tentacles trail 120 mm below.
5. **Avoid (0.4 s).** A mine beeps and drifts; the player tilts or jets past it.
6. **Depth resolve (2.0 s).** Oxygen tallies; the next stratum loads.

A contraction cycle is 1.6–2.4 s; a dive is 70 s; five dives fill the session.

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
- **Orientation**: dome-up. Convex exterior faces `+ẑ_P`; cavity opens `−ẑ_P`. Apex `A_P = (0,0,+75.0) mm`; rim at `z_P = +20 mm`.
- **P origin**: rim-circle centre. The bowl floats between the palms, so `P_O` is tracked, not simulated.
- **Bell margin**: the rim torus is the contraction ring; palm presses on the rim at azimuths `λ_L` and `λ_R` (left and right palms).
- **Contraction**: `κ ∈ [0, 1]` driven by the sum of palm normal forces; bell height compresses by `6·κ mm`; the snap-back impulse is proportional to `κ`.
- **V-frame**: anchored to `P_O` at the bell centre, with `V` vertically oriented to gravity: `ŷ_V = −ĝ`, `x̂_V` from the dome's forward axis projected horizontally. Trench depth `z_V ∈ [0, 600] mm`.
- **Deviation 1**: the bowl is hand-held; there is no free rigid-body motion. Vertical motion is entirely virtual.
- **Deviation 2**: "oxygen" is a 100-unit budget; jetting costs 2 units, drifting costs 0.6 units/s.
- **Deviation 3**: mines are `V`-space spheres on a 2-D scroll; tentacles are a 6-segment rope simulated with Verlet.
- **Deviation 4**: the bell gel shader ignores the ceramic's real specular, using a subsurface-scattering approximation for legibility.

## 3. Physical Play Space & Ergonomics

The player cups both hands over the bowl, palms on the rim at roughly 3 and 9 o'clock, thumbs on the dome. The elbows rest on the chair arms or the table. Contractions are small (8–12 mm of palm travel) but repeated at 0.4–0.6 Hz, so the motion is a gentle rocking, not a squeeze.

| Parameter | Value | Rationale |
|---|---|---|
| Hold | Both palms on rim, thumbs on dome | Stable and warm |
| Contraction travel | 8–12 mm per palm | Bell compression |
| Contraction rate | 0.4–0.6 Hz | Sustainable rhythm |
| Press force | 1.5–4.0 N per palm | Detectable, not tiring |
| Tilt range | ±20° | Steering authority |
| Gaze | 20–28° down, 350–500 mm | Trench visibility |
| Session | 7 min; 70 s dives; 12 s rest | Palm stamina |

Fatigue mitigation: the bowl rests partly on the lap, so the palms only add pressure; contractions are short; each dive ends with a 6 s hands-open rest; a contraction-rate meter flags > 0.8 Hz as inefficient thrashing.

## 4. Prop Geometry & Registration

A glazed bowl inverted so the foot ring faces down and the dome is up. The rim is the only part the palms touch, and its circularity makes the contraction axis easy to estimate.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 75.0 mm ± 1.0 | Silhouette |
| Depth | 55 mm ± 1.5 | Apex to rim |
| Wall `t` | 4.0 mm ± 0.5 | Rim step |
| Rim circle radius | 50.9 mm | `R·sin 42.8°` |
| Rim tube radius | 2.0 mm | Palm contact |
| Mass | 380 g ± 20 | Balance |
| Registration | rim ellipse + dome | 30 frames |
| Re-acquisition | `ε_R < 2.0 mm`, `ε_τ < 4°` | After blur |

Feature atlas (outer dome):

| Station | `θ` | Feature |
|---|---|---|
| S0 | 0–15° | Apex (thumbs rest) |
| S1 | 15–45° | Bell canals |
| S2 | 45–75° | Marginal ribs |
| S3 | 75–90° | Rim margin (palm press) |
| S4 | rim | Bell margin / contraction ring |

Colliders: outer dome, rim torus, a palm press band at `θ = 80–90°`. Registration publishes `P` at 72 Hz and the palm contact forces.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | both palms | S3 rim margin | Downward press | `Contract(κ)` |
| I5.2 | `L_thumb_tip`, `R_thumb_tip` | S0 apex | Rest / tilt | `Tilt(τ_x, τ_z)` |
| I5.3 | whole dome | — | Tilt ±20° | `Steer(v_lat)` |
| I5.4 | either palm | S3 | Quick double press | `PulseJet` (emergency) |
| I5.5 | both hands | — | Lift 20 mm | `Surface` |
| I5.6 | both hands | — | Open 800 ms | `Rest` |

Contraction `κ = clamp((F_L + F_R)/8.0 N, 0, 1)`. A jet fires when `κ` rises above 0.45 and then falls below 0.2 within 0.35 s (a press-release cycle). Steering uses the dome's tilt: `v_lat = 120·sinτ` mm/s.

## 6. Input Kinematics & Signal Processing

- **Bell compression**: `c_b = 6·κ` mm; a mass-spring model `ẍ = −k_b(x − c_b) − c_b_d·ẋ`, `k_b = 900 N/m`, `c_b_d = 18 N·s/m`, so the bell snaps back in ~0.18 s.
- **Jet impulse**: `J_j = 0.055·κ_peak` N·s; vertical velocity `v_y += J_j/m_v`, with virtual mass `m_v = 120 g`; a full contraction gives `Δv_y ≈ 0.46 m/s`.
- **Buoyancy/drag**: `v̇_y = −g_v − k_d·v_y·|v_y| + J_j`, `g_v = 0.18 m/s²` (sinking), `k_d = 0.9 m⁻¹`.
- **Steering**: `τ = atan2(n̂_P·x̂_W, n̂_P·ŷ_W)`; `v_x = 120·sinτ_x` mm/s, `v_z = 120·sinτ_z` mm/s.
- **Contraction rhythm**: detect `κ` peaks with a 0.25 s refractory; efficiency `η = 1` if peak `κ ≥ 0.7`, `0.6` if `0.45–0.7`.
- **Tentacle**: 6-segment Verlet rope, segment length 20 mm, gravity `g_v`, drag 0.04; the tip lags the bell by 120 mm.
- **Mine contact**: if a tentacle segment is within 6 mm of a mine sphere, detonate.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a vertical trench: `x_V ∈ [−200, 200] mm`, `z_V` is depth `[0, 600] mm` (0 = surface, 600 = seabed), and `y_V` is vertical up. The bell is at `z_bell`; the goal is to reach `z_V = 0` (the surface) before oxygen runs out.

| Entity | Anchoring | Notes |
|---|---|---|
| Trench | `P_O` horizontal, gravity vertical | 400 × 600 mm |
| Bell | `P_O` | follows hands |
| Tentacles | `P` | 6-segment rope |
| Mines | `V` | scroll upward |
| Kelp | `V` | parallax |
| Oxygen bar | screen-space | depth-tested |

The trench scrolls downward past the bell, giving the illusion of ascent while the bell stays near the lap.

## 8. Gameplay Systems & Rules

**Ascent.** Each efficient contraction raises the bell 40–90 mm of virtual depth; drifting sinks at 8 mm/s. Reaching `z_V = 0` clears the dive.

**Oxygen.** Starts at 100; jets cost 2; drifting costs 0.6/s; a mine strike costs 18. Oxygen 0 ends the dive.

**Mines.** Spheres of radius 12 mm drifting upward at 30–60 mm/s. A tentacle hit detonates: screen flash, oxygen −18, and the bell is stunned (no jets) for 1.0 s.

**Efficiency.** Contractions at 0.4–0.6 Hz with `κ ≥ 0.7` are "clean" and add `+10` each. Thrashing above 0.8 Hz yields `κ` decay (bell fatigue) and no bonus.

**Combo.** Five clean contractions in a row = "Bloom" (next jet +50% height, tentacles glow).

**Score.** `S = 100·depth_gained/100 + 10·clean + 50·bloom + 200·dive_clear`.

**Win.** Clear 5 dives. **Lose.** Oxygen 0 or three mine strikes in one dive.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | bowl seen | `Drift` | 2.0 s | `P` locked |
| `Drift` | calibration / jet end | `Contract` | continuous | sink |
| `Contract` | `κ ≥ 0.35` | `Jet` | 0.25 s | compress |
| `Jet` | press-release | `Drift` / `Stun` | 0.6 s | rise |
| `Stun` | mine strike | `Drift` | 1.0 s | flash |
| `Surface` | `z ≤ 0` | `DiveClear` | 1.0 s | breach |
| `DiveClear` | surface | `Drift` / `Victory` | 2.0 s | tally |
| `Victory` | 5 dives | `Score` | 2.0 s | fanfare |
| `Score` | victory / oxygen 0 / 3 strikes | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Rim torus | Palms | Press force | ceramic/skin |
| Dome apex | Thumbs | Tilt input | ceramic/skin |
| Bell | Virtual water | Drag + buoyancy | gel |
| Tentacle segment | Mine sphere | Detonate | soft |
| Bell | Trench walls | Soft steer limit | — |
| Mine | Trench floor | Despawn | — |

Virtual bell mass `m_v = 120 g`, drag `k_d = 0.9 m⁻¹`, buoyancy `g_v = 0.18 m/s²` downward. Jet impulse `J_j ≤ 0.055 N·s`. Verlet tentacles use 6 segments with 4 constraint iterations. CCD: mines move ≤ 0.7 mm/tick; tentacle segments ≤ 2 mm/tick, so a 6 mm proximity sphere is safe. The bowl never leaves the hands, so no prop CCD.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → lap → far kelp → trench fog → mines → jellyfish bell (translucent) → tentacles → near kelp → hand mesh → bioluminescent particles → UI.

Occlusion cases:
1. **Palms on the rim**: hands render in front of the bell margin; the gel glow bleeds around them.
2. **Tentacles behind hands**: tentacles that pass behind the palms are hidden, which sells the suspension.
3. **Bell translucency**: mines behind the bell are dimmed but visible through the gel (order-independent transparency).
4. **Mine behind kelp**: kelp occludes mines correctly.
5. **UI**: the oxygen bar floats just above the bell and is soft-occluded by the thumbs.
6. **Surface breach**: at `z = 0` the water plane clips the bell, showing the ceramic again.

VFX budget: bell 4 k tris (translucent), tentacles 6 × 200 tris, mines 8 × 300 tris, particles ≤ 500, kelp 12 quads.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Contract | Low pulse | Bell compresses |
| Jet | Water whoosh | Ring of light |
| Clean contraction | Chime | Canal glow |
| Bloom | Chord | Tentacles flare |
| Mine beep | Sonar ping | Red blink |
| Mine strike | Underwater boom | Flash + stun |
| Drift | Ambient bubbles | Sink |
| Surface | Breach splash | Sky |
| Dive clear | Whale song | Score card |

## 13. Difficulty Progression & Tuning

| Level | Mines | Mine speed (mm/s) | Oxygen drain | Jet gain (mm) | Trench depth (mm) |
|---|---|---|---|---|---|
| 1 | 3 | 30 | 0.4/s | 90 | 400 |
| 2 | 4 | 38 | 0.5/s | 82 | 450 |
| 3 | 5 | 45 | 0.6/s | 74 | 500 |
| 4 | 6 | 52 | 0.7/s | 66 | 550 |
| 5 | 8 | 60 | 0.8/s | 58 | 600 |

**Adaptive rule.** Track clear rate `c` over the last 3 dives. If `c ≥ 0.9`, add a mine or raise mine speed 4 mm/s. If `c ≤ 0.4`, remove a mine and restore 10 oxygen. Clamp mines to `[2, 10]`.

## 14. Edge Cases & Failure Recovery

- **Bowl dropped**: if palm contact is lost on both sides for > 400 ms, freeze the dive and ghost the bell; resume on re-grab.
- **One palm only**: contraction is half-strength; steering still works.
- **Tilt beyond 20°**: clamp and show a horizon warning; tentacles drag.
- **Tracking loss**: freeze tentacles for ≤ 300 ms; longer pauses.
- **Two mines at once**: each detonates separately but the stun does not stack.
- **Bell at the trench wall**: soft clamp with a 20 mm/s bounce; no damage.
- **Oxygen exactly 0 during a jet**: the jet completes, then the dive ends.
- **Pause mid-contraction**: `κ` freezes; resume continues the press.

## 15. Implementation Architecture

Classes: `PropRegistration`, `PalmPressSolver`, `BellSpring`, `JetModel`, `SteerModel`, `TentacleRope`, `MineField`, `OxygenModel`, `DiveDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P`; `PalmPressSolver → κ`; `BellSpring → c_b`; `JetModel → v_y`; `SteerModel → v_x`; `TentacleRope` follows `P`; `MineField` collides; `OxygenModel` updates; all → `DiveDirector`, `ScoreLedger`.

```text
onFixedTick(dt = 1/90):
    kappa = clamp((press(L) + press(R)) / 8.0, 0, 1)
    c_b = 6 * kappa
    x = bellSpring.step(x, c_b, dt)
    if kappaPeak(kappa) and releasedWithin(0.35):
        vy += 0.055 * peak / 0.120
        oxygen -= 2
        audio.play("jet")
    vy += (-g_v - kd*vy*abs(vy)) * dt
    z += vy * dt
    if z <= 0: state = DiveClear
```

```text
onTentacleCollision(segment, mine):
    mine.detonate()
    oxygen -= 18
    stunTimer = 1.0
    audio.play("underwater_boom"); vfx.flash()
    if oxygen <= 0 or strikes >= 3: state = Score
```

## 16. Acceptance Test Checklist

1. **Contraction mapping**: an 8 N combined press yields `κ = 1.0` and `c_b = 6 mm`.
2. **Jet impulse**: a full press-release raises `v_y` by 0.46 ± 0.05 m/s.
3. **Jet height**: a full jet gains 40–90 mm of virtual depth.
4. **Drift**: with no input the bell sinks at 8 ± 1 mm/s.
5. **Steering**: a 20° tilt produces 41 ± 5 mm/s lateral speed.
6. **Oxygen**: a jet costs exactly 2 units; drifting costs 0.6/s.
7. **Mine strike**: a tentacle within 6 mm detonates and costs 18 oxygen.
8. **Stun**: after a strike, jets are blocked for 1.0 s.
9. **Bloom**: five clean contractions grant a +50% jet height.
10. **Surface**: reaching `z ≤ 0` clears the dive exactly once.
11. **Lose**: oxygen 0 or three strikes end the dive.
12. **One palm**: half-strength contraction still produces a jet (reduced).
13. **Occlusion**: a mine behind the bell is dimmed but visible.
14. **Performance**: bell + 6 tentacles + 8 mines sustain 90 fps.
