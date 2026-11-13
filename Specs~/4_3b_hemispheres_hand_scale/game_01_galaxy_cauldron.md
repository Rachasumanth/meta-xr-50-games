# Galaxy Cauldron

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Galaxy Cauldron |
| Slug | `galaxy_cauldron` |
| Physical prop | Empty cereal/soup bowl, concave up, resting on the lap; inner sphere radius `R = 75 mm`, rim-to-floor depth `d = 55 mm`, wall `t = 4 mm`, mass 380 g |
| Family / sub-shape | Spherical-cap hemisphere, hand scale |
| Scale class | Prop 1:1; virtual nebula confined to the 195 mm bowl aperture; tabletop play volume < 40 cm |
| Source concept quote | "Stir the interior in circular orbits with the index finger at varying speed and depth; clockwise tightens the gravity spiral, counter-clockwise flings dark-matter comets over the rim." |
| Physical input | Non-dominant hand steadies the rim; dominant index fingertip stirs the interior at variable radius, speed, and depth |
| Session length | 9 min (5 galaxies × 90 s + 4 × 12 s interlude + onboarding) |
| Lose condition | Nebula luminosity reaches 0, or the 90 s galaxy clock expires before the target star count |

## 1. Fantasy & Core Loop

The bowl is a cauldron of primordial gas. The player looks down into a 195 mm aperture and sees a full spiral galaxy lying on the interior shell: violet hydrogen, amber helium, teal carbon, white oxygen, and iron-red dust, all suspended 2–6 mm above the ceramic. The index finger is a gravity wand. Moving it clockwise winds the differential rotation tighter, compressing clouds toward the core until matching species fuse into a star. Moving it counter-clockwise spins the disc outward, and dark-matter seeds that reach the rim are slung as comets across the room, trailing cold blue arcs over the lip.

1. **Ignition (6 s).** The bowl registers, the nebula fades in as a 2-D disc projected onto the interior shell, and the five species are labelled by color. Input is live but scoring is gated.
2. **Accretion (55 s).** The player alternates clockwise compression with counter-clockwise flaring. Matching clouds fuse; mismatched clouds nova and cost luminosity. The gravity-spiral overlay tightens or unwinds with the sign of the stir.
3. **Comet window (20 s).** Dark-matter seeds become visible as black discs with a violet fringe. Counter-clockwise flinging is the only way to clear them before they sink into the core.
4. **Collapse check (9 s).** Remaining clouds are scored; the galaxy either resolves into a star field or collapses into the core.
5. **Interlude (12 s).** The bowl is wiped clean by a white-out, the next galaxy's palette loads, and the player may rest the stirring hand.

A galaxy is won when the target number of stars is formed and all dark seeds are ejected. It is lost when luminosity hits 0 (three discordant merges) or the clock expires with fewer than the target stars. The full run is five galaxies with escalating cloud counts and tighter fusion windows.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Hemisphere sub-shape**: the prop is a hemispherical shell of radius `R` and wall thickness `t`; model in P with the rim circle in the `x̂_P`–`ŷ_P` plane at `z = 0` and the apex at `z = ±R`. For **bowl-up** props the cavity faces `+ẑ_P`; for **dome-up** props the convex dome faces `+ẑ_P`. Inner surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`, θ ∈ [0°, 90°], λ ∈ [0°, 360°). Rim torus: major radius `R`, minor radius `t/2`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **R and t**: `R = 75.0 mm` (inner sphere radius), `t = 4.0 mm` (wall), outer sphere radius `R + t = 79.0 mm`. The prop is a shallow cap, not a full hemisphere: rim-to-apex depth `d = 55.0 mm`. All geometry below therefore truncates at `θ_rim`.
- **P-frame**: origin at the rim-plane centre, `+ẑ_P` out of the bowl. The inner sphere centre sits at `(0, 0, +20.0) mm` in P; the interior apex is at `z = −55.0 mm`. The outer surface shares that centre with radius 79.0 mm.
- **Rim**: inner rim radius `a_in = √(R² − (R − d)²) = 72.28 mm`; outer rim radius `a_out = √((R+t)² − (R − d)²) = 76.43 mm`; rim torus major radius `r_rim = (a_in + a_out)/2 = 74.35 mm`, minor radius `t/2 = 2.0 mm`. Rim truncation angle `θ_rim = acos((R − d)/R) = 74.53°`.
- **Surfaces**: inner `p_in(θ,λ) = (R sinθ cosλ, R sinθ sinλ, 20 − R cosθ)`, θ ∈ [0°, 74.53°], λ ∈ [0°, 360°). Outer `p_out(θ,λ) = ((R+t) sinθ cosλ, (R+t) sinθ sinλ, 20 − (R+t) cosθ)`, θ ∈ [0°, 75.34°]. Inner geodesic radius at the rim is `s_rim = R·θ_rim = 97.58 mm`.
- **V-frame anchoring**: `V` is anchored to `P` with a 0.6 s orientation low-pass and a gravity correction that keeps `ẑ_V` within 25° of `ŷ_W`; origin `V_O = P_O + (0, 0, 4) mm`. The nebula therefore stays in the bowl when the lap shifts, but never rolls with a tilted bowl.
- **Deviation 1**: the "hemisphere" is clipped to a 55 mm-deep cap; a full `R`-deep hemisphere would put the floor below the prop's physical base and is not used in any collider or projection.
- **Deviation 2**: stir depth is a first-class input, measured as `h = −q_z` (0 at the rim, 55 mm at the floor); fusion gain depends on `h`, not only on tangential speed.
- **Deviation 3**: the non-dominant hand is a static rim clamp that contributes grip integrity but never drives the nebula.
- **Deviation 4**: all virtual matter is constrained to the interior shell; only ejected comets may leave the bowl volume, and they are rendered as additive arcs that never collide with the room.

## 3. Physical Play Space & Ergonomics

The player sits with the bowl centred on the lap, 180–260 mm in front of the sternum and 30–60 mm above the thighs. The non-dominant hand rests on the rim at the 10 o'clock position with a light four-finger curl; the dominant hand hovers over the aperture with the wrist in 15–25° flexion and the index extended. The head pitches 20–28° down. The bowl is not gripped: it is stabilized, so the wrist and finger flexors stay under 15% MVC.

| Parameter | Value | Rationale |
|---|---|---|
| Support | Lap or table `T`, bowl base fully supported | Eliminates active holding load during 90 s stirs |
| Play patch | 210 mm × 210 mm around the bowl | Covers stir radius 97.6 mm plus 10 mm hand margin |
| Stir radius | 8–92 mm from the floor axis | Keeps the fingertip inside the ceramic wall |
| Stir depth `h` | 8–50 mm below the rim | Avoids knuckle collision with the rim |
| Index duty | ≤ 20 s continuous, 3 s micro-rest per galaxy | Flexor tendon load at 0.4–1.2 N contact force |
| Wrist posture | 15–25° flexion, ≤ 10° ulnar deviation | Keeps the carpal tunnel clear during orbiting |
| Session | 9 min, 5 × 90 s galaxies | Five short bouts with 12 s rests |
| Camera | 20–28° downward pitch, 350–500 mm viewing distance | Bowl aperture subtends 22–29° |

Fatigue mitigations: the rim hand carries no weight; the stirring index uses a relaxed extension and is allowed to drag the middle finger for support; a soft "hand open" prompt appears at 20 s of continuous contact; the interlude is a hard 12 s gate so the player cannot skip recovery; and the stir target speed never exceeds 260 mm/s, which is below the 300 mm/s comfortable index circumduction ceiling.

## 4. Prop Geometry & Registration

Nominal prop constants: inner radius `R = 75.0 mm`, wall `t = 4.0 mm`, depth `d = 55.0 mm`, mass `m = 380 g`, ceramic density ≈ 2.4 g/cm³. The inner surface area of the cap is `A_in = 2πR d = 25,918 mm²`; the outer area is `A_out = 2π(R+t)d = 27,300 mm²`. Rim circumference at the torus centre is `2πr_rim = 467.1 mm`.

| Property | Nominal | Tolerance | Source |
|---|---|---|---|
| Inner radius `R` | 75.0 mm | ±0.4 mm | Silhouette + rim fit |
| Wall `t` | 4.0 mm | ±0.3 mm | Rim thickness fit |
| Depth `d` | 55.0 mm | ±0.5 mm | Scene-mesh interior probe |
| Inner rim radius | 72.28 mm | ±0.4 mm | Derived from `R`, `d` |
| Rim torus | 74.35 / 2.0 mm | ±0.4 / ±0.2 mm | Circle fit |
| Mass | 380 g | ±20 g | Factory nominal; not weighed at runtime |
| Pose RMS | — | < 0.5 mm | Registration fit residual |
| Yaw drift | — | < 0.3°/min | Static hold test |

Surface atlas (P-fixed, painted on the virtual shell, not on the ceramic):

| Zone | θ range | λ range | Purpose |
|---|---|---|---|
| Core basin | 0–18° | all | Black-hole core; fusion sink |
| Inner disc | 18–45° | all | Primary cloud orbit band |
| Outer disc | 45–68° | all | Comet acceleration band |
| Rim shelf | 68–74.53° | all | Ejection lip; 25 mm tap zone |
| Species meridian | all | 0°, 72°, 144°, 216°, 288° | Five color calibration spokes |
| Stir reticle | 20–70° | all | Faint guide ring, fades after 10 s |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Spherical cap | `R = 75.0 mm`, θ ≤ 74.53°, centre `(0,0,20)` | Interior contact and projection |
| C1 | Spherical cap | `R = 79.0 mm`, θ ≤ 75.34° | Exterior / palm support |
| C2 | Torus | major 74.35 mm, minor 2.0 mm, plane `z = 0` | Rim contact, tap classifier |
| C3 | Capsule | radius 4.0 mm, length 8.0 mm | `R_index_tip` stir effector |
| C4 | Capsule | radius 14 mm, length 50 mm | Rim-hand palm proxy |
| C5 | Disc | radius 97.58 mm at `z = −1.0 mm` | 2-D nebula simulation plane |
| C6 | Sphere | radius 3.0 mm | Ejected comet visual proxy, no room collision |

Registration procedure: present the bowl at 0.8–1.2 m, tilt it ±30° about two axes for 1.2 s, then hold still for 0.4 s. The fitter extracts ≥ 24 rim points and ≥ 60 interior depth samples from the scene mesh, solves the rim circle and plane, and derives `R`, `d`, and the P origin. Acceptance requires rim RMS < 0.5 mm, plane-normal error < 0.8°, and depth error < 0.7 mm. `P` then runs on visual odometry at 72 Hz with a 90 Hz prediction step.

Re-acquisition: after loss, search a 150 mm cube around the last pose for 1.5 s using the rim torus as the primary feature. Success requires 12 rim inliers for 5 frames with RMS < 0.8 mm; snap with a 150 ms ease. During loss the nebula freezes in `V`, not in `P`, so the simulation does not jump.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Rim steady | `L_thumb_tip`, `L_index_tip`, `L_middle_tip` on C2 | rim torus, λ 120°–180° | Grip integrity 0–1 |
| I5.2 | Stir orbit | `R_index_tip` on C0 | inner cap, θ 8–70° | Contact point, tangent velocity |
| I5.3 | Deep stir | `R_index_tip`, contact `h > 30 mm` | inner cap near floor | Fusion gain ×1.35 |
| I5.4 | Shallow stir | `R_index_tip`, contact `h < 18 mm` | inner cap near rim | Comet gain ×1.25 |
| I5.5 | Clockwise drive | `R_index_tip` | `Ω_s < −0.6 rad/s` for 120 ms | Inward compression field |
| I5.6 | Counter-clockwise flare | `R_index_tip` | `Ω_s > +0.6 rad/s` for 120 ms | Outward expansion field |
| I5.7 | Comet flick | `R_index_tip` | radial outward speed > 220 mm/s at `h > 40 mm` | Ejection boost ×1.6 for 250 ms |
| I5.8 | Support drag | `R_middle_tip`, `R_ring_tip` on C1 | exterior wall | Stabilization only, no control |
| I5.9 | Release | any active stir joint | signed distance > 8 mm | Contact ends; field decays over 250 ms |

Per-finger roles: the left thumb, index, and middle clamp the rim; the left ring and pinky hang free and are ignored. The right index is the only stirring effector. The right middle may touch the exterior wall for support and is treated as a brace; the right thumb, ring, and pinky are ignored. A stir contact is valid when its signed distance to C0 is < 5 mm, `h ∈ [8, 50] mm`, and age > 30 ms.

## 6. Input Kinematics & Signal Processing

The fingertip position `q` is projected to the inner cap. With `z_c = 20 mm`, `θ_q = acos(clamp((z_c − q_z)/R, −1, 1))` and `λ_q = atan2(q_y, q_x)`. The disc coordinate is `ρ = s = R·θ_q`, `ψ = λ_q`, with `ρ ∈ [0, 97.58] mm`. The tangential velocity `v_t` is the fingertip velocity minus the P-frame origin velocity, projected onto the cap tangent plane at `q`.

The signed stir rate is `Ω_s = (q_xy × v_t)_z / max(|q_xy|², 400 mm²)`, clamped to ±4.5 rad/s (258 °/s). Positive `Ω_s` is counter-clockwise viewed from `+ẑ_P`. The radial component is `v_ρ = v_t · ê_ρ`. A direction is "committed" after 120 ms of consistent sign; reversals inside 120 ms are treated as tremor. The depth is `h = −q_z`, and the deep/shallow gains are smooth ramps: `g_deep = smoothstep(18, 35, h)`, `g_shallow = 1 − smoothstep(18, 35, h)`.

Signal chain: raw joints at 72 Hz → One Euro filter (mincutoff 1.4 Hz, beta 0.03, derivative cutoff 9 Hz) → 90 Hz resample with 1-frame prediction clamped to 3 mm → projection to C0 → tangent velocity by central difference over 3 samples → `Ω_s` → second-order Butterworth low-pass at 11 Hz → dead zone 18 mm/s with 12 mm/s release → saturation at 260 mm/s. Contact normals are smoothed over 3 frames; a contact whose normal deviates > 55° from the local cap normal is rejected as a wall scrape.

Dead zones and gains: the 18 mm/s dead zone equals 1.6 mm of fingertip travel per 90 ms, above physiological tremor at rest (≈ 8–12 mm/s) and below deliberate slow stirs (≈ 45 mm/s). Saturation at 260 mm/s is the 95th percentile of comfortable index circumduction at a 90 mm radius. The field gain is `K_field = 1.0 + 0.35·g_deep`, and the comet gain is `K_comet = 1.0 + 0.25·g_shallow`.

Stir-direction sign convention: clockwise is negative `Ω_s`. The rotation field applied to a cloud at `(ρ, ψ)` is `v_θ,target = Ω_s·ρ`, relaxed with time constant `τ_θ = 0.35 s`. Radial acceleration is `a_ρ = −C_in·min(|Ω_s|, 3.0)·(0.2 + ρ/97.58)` for clockwise, and `a_ρ = +C_out·min(Ω_s, 3.0)·(0.2 + ρ/97.58)` for counter-clockwise, with `C_in = 260 mm/s²`, `C_out = 300 mm/s²`. All accelerations are clamped to 900 mm/s² to keep cloud motion legible.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to `P` as defined in §2.1: origin 4 mm above the rim-plane centre, `+ẑ_V` up out of the bowl, `x̂_V` aligned with the P yaw low-pass. The nebula is a 2-D simulation in disc coordinates `(ρ, ψ)` rendered onto the cap at a 3 mm normal offset. The mapping is azimuthal-equidistant: `θ = ρ/R`, `p_V = (R sinθ cosψ, R sinθ sinψ, 20 − R cosθ)`; the inverse is `ρ = R·acos((20 − p_z)/R)`.

| Entity | V-space placement | Notes |
|---|---|---|
| Nebula plane | shell offset +3 mm, `ρ ∈ [0, 97.58]` | 2-D fluid sim, 128² grid |
| Core | `(0, 0, −55)` | Sink at the floor apex, radius 14 mm |
| Star layer | shell offset +18 to +55 mm | Stars rise after fusion |
| Comet arcs | from rim to 450 mm outside aperture | Additive, no room collision |
| Gravity spiral | shell offset +1.5 mm | Pitch tightens with clockwise `Ω_s` |
| Species labels | rim shelf | Five color spokes |
| Camera | head-locked | 350–500 mm above the bowl |

Scale mapping: 1 mm of cloud motion equals 1 mm of shell arc; the 97.58 mm disc radius maps to 74.53° of cap, so 1.31 mm of arc per degree. A full stir at 200 mm/s completes a lap of the 70 mm orbit in 2.2 s and advects the outer disc at 285 mm/s, which is 3.2 disc radii per second — fast enough to read as a vortex, slow enough to track individual clouds.

The gravity-spiral overlay has 3.5 turns at rest. Clockwise drive reduces the pitch to 1.5 turns over 2.5 s; counter-clockwise increases it to 5.0 turns. The overlay is a guide, not a collider.

## 8. Gameplay Systems & Rules

Entities per galaxy: `2P` gas clouds in `P` matching pairs, 2–5 dark-matter seeds, 1 core, and the star field. Clouds spawn on the outer disc (`ρ ∈ [70, 95] mm`) with tangential velocity equal to the local rigid-body value plus `U(−25, 25) mm/s`. Each cloud carries a species `k ∈ {H, He, C, O, Fe}`, a mass `m_c ∈ [0.4, 1.0]`, and a collision radius `r_c = 3.0 + 1.5·m_c` mm.

Cloud dynamics per 90 Hz tick: semi-implicit Euler with `a_θ = (Ω_s·ρ − v_θ)/0.35`, `a_ρ` from §6, drag `a_d = −v/0.9 s`, and a soft core repulsion `a_core = 420·exp(−(ρ/22)²)` mm/s². Cloud speed is capped at 420 mm/s. Clouds are soft bodies for gameplay but point masses for collision.

Fusion rule: clouds `i,j` of the same species fuse when `|p_i − p_j| < r_f` and `|v_i − v_j| < v_f`. Base `r_f = 12.0 mm`, `v_f = 35 mm/s`. Deep stir multiplies `r_f` by `1 + 0.25·g_deep`; the fusion consumes both clouds and spawns one star at the midpoint with score `+100`. A star adds 10 mm to the local luminosity.

Discord rule: clouds of different species within `r_f_d = 10.0 mm` produce a nova: both clouds are consumed, luminosity drops by 12 percentage points, a shockwave pushes all clouds outward by 18 mm, and the pair's target star becomes unreachable. Three novas end the galaxy at 0 luminosity from a full 100.

Dark-matter seeds: 3 seeds of radius 5 mm, unaffected by fusion. Counter-clockwise drive pushes them outward. A seed becomes a comet when `ρ > 0.88·97.58 = 85.9 mm` and `v_ρ > 150 mm/s`; it is ejected when it crosses the rim with `v_ρ > 120 mm/s`, awarding `+75` and clearing one "shadow" stack. A comet that re-enters the disc loses its comet state. Each un-ejected seed at galaxy end costs 40 luminosity.

Scoring: star `+100`, comet `+75`, no-nova galaxy `+300`, time bonus `+5` per remaining second, five-galaxy clear `+1000`. The run score is the sum; the HUD shows luminosity, stars/target, seeds remaining, and the galaxy clock.

Win: reach the target star count and eject all seeds before 90 s. Lose: luminosity 0 or clock expiry below target stars. Partial: stars met but seeds remaining counts as a "dusty victory" with half the time bonus.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset tracked | load bowl shader, zero score | Register |
| Register | bowl presented | rim RMS < 0.5 mm | set `P`, `R`, `d`, `θ_rim` | Calibrate |
| Calibrate | rim hand + stir hand valid 30 ms | both contacts | paint species atlas, enable input | Ignition |
| Ignition | 6.0 s timer | galaxy ≤ 5 | fade nebula, spawn clouds | Accretion |
| Accretion | 55.0 s timer | luminosity > 0 | accept stir, fuse, nova | CometWindow |
| Accretion | luminosity = 0 | any | collapse cue | GalaxyFail |
| CometWindow | 20.0 s timer | seeds > 0 | boost outward gain, show seeds | CollapseCheck |
| CollapseCheck | 9.0 s timer | — | tally stars, eject check | Resolve |
| Resolve | galaxy < 5 | stars ≥ target, seeds = 0 | galaxy win, load next | Interlude |
| Resolve | stars < target or seeds > 0 | — | partial score | Interlude |
| Interlude | 12.0 s timer | — | white-out, rest prompt | Ignition |
| Any | prop lost > 1.5 s | — | freeze V, dim nebula | Recovery |
| Recovery | re-acquire + 400 ms stable | pose valid | restore, 2 s grace | previous state |
| Any | headset removed | — | save run | Paused |

All timers are wall-clock at 90 Hz and exclude the 12 s interlude. State transitions are idempotent and log tick index, `P`, luminosity, star count, and seed count for replay.

## 10. Physics & Collision

| Body | Shape | Mass | Friction | Restitution | Motion |
|---|---|---|---|---|---|
| Bowl | C0/C1/C2 | 380 g static | 0.55 | 0.05 | Kinematic via `P` |
| Index effector | Capsule r 4.0 mm | kinematic | 0.35 | 0.0 | Tracked joint |
| Gas cloud | Sphere r 3–4.5 mm | 0.4–1.0 (game unit) | 0.0 | 0.0 | 2-D advection |
| Dark seed | Sphere r 5.0 mm | 2.0 (game unit) | 0.0 | 0.0 | 2-D advection |
| Star | Sphere r 6.0 mm | static | — | — | Shell-anchored |
| Comet | Sphere r 3.0 mm | 0.2 | 0.0 | 0.0 | Ballistic arc |
| Core | Sphere r 14 mm | static | — | — | Sink trigger |

Pair matrix: index↔C0 (input contact), palm↔C1 (brace), rim↔C2 (grip), cloud↔cloud (fusion or nova), cloud↔core (absorbed, −5 luminosity if unmatched), seed↔rim (ejection), comet↔room (ignored). Cloud collisions use a uniform 8 mm spatial hash with 4 mm cells; at ≤ 40 clouds the pair count stays under 780 per tick.

CCD: clouds move ≤ 4.7 mm per 90 Hz tick at the 420 mm/s cap, below the 3 mm minimum cloud radius only at the fastest overlap, so a swept-sphere test is used for cloud↔cloud and seed↔rim. The comet's 600 mm/s exit speed gives 6.7 mm per tick, so its rim crossing is solved analytically against the torus plane and the arc is purely visual afterward.

Forces: the stir field is an acceleration field, not a force on a rigid body; `F = m_c·a` is used only for the nova shockwave impulse `J = 0.003 N·s` per cloud. The bowl itself receives a cosmetic rim vibration of 0.15 mm at 42 Hz when a star forms, driven by a damped spring `k = 1800 s⁻²`, `c = 26 s⁻¹`; it never moves the collider.

## 11. Rendering, Occlusion & Compositing

Layer order near to far: 7 HUD → 6 hand mesh → 5 near VFX (finger glow, star flares) → 4 nebula particles → 3 interior shell decals → 2 bowl rim/outer shell → 1 scene mesh depth → 0 passthrough. The interior shell is rendered as a depth-tested decal 1.5 mm above C0 so particles at +3 mm never z-fight.

Occlusion cases:

1. **Rim occludes the outer disc.** The ceramic rim correctly hides clouds at `h < 6 mm`; a 1.5 mm "lip glow" is drawn on the inner rim so the player can still see the boundary.
2. **Index finger over the nebula.** The tracked hand mesh cuts the particle layer; a 12 mm soft shadow is cast on the shell beneath the fingertip to preserve contact readability.
3. **Palm behind the bowl.** The exterior of the bowl is opaque; the stirring hand behind the far rim is fully hidden, and only its fingertip re-emerges inside the aperture.
4. **Star rising above the rim.** Stars at `h > 55 mm` are composited in front of the near rim only where their depth is nearer; the far rim still occludes them correctly.
5. **Comet arc crossing the room.** Ejected comets are additive and depth-test against the scene mesh, so they disappear behind a real table edge rather than painting over the room.
6. **Nebula seen through the ceramic.** No transmittance is allowed: the bowl shell is fully opaque, and the nebula is clipped by C0. This prevents the galaxy from leaking onto the lap.

VFX budget: 2,400 particles total (1,600 nebula, 300 fusion burst, 250 comet trail, 150 star twinkle, 100 ambient motes), 5 species color ramps, 1 spiral overlay (256 × 256 texture), 40 star sprites, 8 comet ribbons. Overdraw ≤ 2.0×; all sprites 64 × 64 atlas. The 128² fluid grid is advected on the CPU at 90 Hz (≈ 0.35 ms) and uploaded as a 128 × 128 RGBA texture.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Bowl registered | Rim | Ceramic tap, 1.8 kHz, short | Rim outline snaps cyan |
| Stir start | Contact point | Soft granular whoosh, pitch ∝ speed | Fingertip glow, spiral wakes |
| Clockwise commit | Bowl centre | Descending perfect fifth | Spiral tightens, clouds compress |
| Counter-clockwise commit | Bowl centre | Ascending perfect fifth | Spiral unwinds, rim flare |
| Fusion | Star midpoint | Bell chord, 3 partials | White flash, star rises |
| Discordant nova | Contact midpoint | Dissonant cluster + noise burst | Red shockwave, luminosity bar drops |
| Deep stir | Floor | Low cello swell | Floor rings pulse |
| Comet ejection | Rim crossing | Rising whip + doppler | Blue arc over the lip |
| Seed sinking | Core | Sub-bass throb | Core darkens, shadow ring |
| Galaxy win | Bowl centre | Major arpeggio, 5 notes | Star field blooms, score stamp |
| Clock warning | HUD | Two wooden ticks at 10 s and 5 s | Clock turns amber |
| Recovery | Rim | Sonar ping | Nebula desaturates, ghost rim |

All cues are spatialised at the bowl or the event point; only the clock and score tally are head-locked. Audio latency budget < 20 ms. The nova cue ducks the ambient bed by 8 dB for 300 ms, and the galaxy-win chord is allowed a 1.2 s tail.

## 13. Difficulty Progression & Tuning

| Galaxy | Name | Pairs `P` | Species | Seeds | Fusion `r_f` (mm) | Drift cap (mm/s) | Clock (s) | Target stars |
|---|---|---|---|---|---|---|---|---|
| 1 | Whisper Spiral | 4 | 3 | 2 | 13.0 | 220 | 90 | 4 |
| 2 | Ember Coil | 5 | 4 | 3 | 12.5 | 250 | 90 | 5 |
| 3 | Verdant Vortex | 6 | 4 | 3 | 12.0 | 280 | 90 | 6 |
| 4 | Iron Rose | 7 | 5 | 4 | 11.5 | 310 | 90 | 7 |
| 5 | Eventide Crown | 8 | 5 | 5 | 11.0 | 340 | 90 | 8 |

Adaptive rule: if the player suffers two discordant novas within 25 s, widen `r_f` by 2.0 mm and reduce drift by 10% for the current galaxy only. If no nova occurs for 40 s and at least half the stars are formed, narrow `r_f` by 1.0 mm and raise drift by 6%, capped at galaxy 5 values. Adaptation never changes the clock, so level length stays predictable.

Named constants: `R = 75.0 mm`, `t = 4.0 mm`, `d = 55.0 mm`, `THETA_RIM = 74.53°`, `S_RIM = 97.58 mm`, `DZ_STIR = 18 mm/s`, `DZ_REL = 12 mm/s`, `V_SAT = 260 mm/s`, `OMEGA_MAX = 4.5 rad/s`, `K_FIELD = 1.0`, `K_DEEP = 0.35`, `K_SHALLOW = 0.25`, `C_IN = 260 mm/s²`, `C_OUT = 300 mm/s²`, `TAU_THETA = 0.35 s`, `TAU_DRAG = 0.9 s`, `R_FUSE = 12.0 mm`, `V_FUSE = 35 mm/s`, `R_DISCORD = 10.0 mm`, `NOVA_COST = 12`, `SEED_EJECT = 120 mm/s`, `COMET_BONUS = 75`, `STAR_BONUS = 100`.

## 14. Edge Cases & Failure Recovery

- **Bowl tracking loss.** Freeze the cloud simulation in `V` for up to 1.5 s, desaturate the nebula, and keep the last stir field decaying over 250 ms. Re-acquire with 12 rim inliers; snap with a 150 ms ease and grant 2 s of no-nova grace.
- **Stir hand leaves the bowl.** Contact ends; the field decays with `τ = 250 ms`. The galaxy clock keeps running, but no nova can occur for 400 ms, so a hand-off never causes an accidental collision.
- **Rim hand slips.** Grip integrity drops to 0.3, the bowl visual gets a 1.5° wobble, and the stir gain is halved. Restoring three rim contacts for 300 ms returns integrity to 1.0.
- **Finger enters the bowl from the side and scrapes the wall.** Contacts with a normal > 55° from the local cap normal are rejected and produce a ceramic scrape cue; no field is applied until the fingertip is inside the aperture.
- **Two clouds fuse at the rim.** Fusion is disabled for `ρ > 92 mm`; instead the pair is flagged "rim-locked" and gets a 400 ms inward nudge. This prevents stars spawning half outside the bowl.
- **Seed trapped in the core.** If a seed reaches `ρ < 12 mm`, it is teleported to `ρ = 88 mm` with an audio "core spit" cue and a 600 ms outward velocity. It is never destroyed without a comet award.
- **Handedness swap.** The game mirrors the rim hand and stir hand, flips `λ` about the sagittal plane, and negates `Ω_s`; all thresholds are unchanged. The atlas species spokes are mirrored with the mapping.
- **Headset removed.** Save galaxy index, score, luminosity, `P`, and cloud state; on resume require re-registration and a 3 s settle before the clock restarts.
- **Low light / black bowl.** If rim inliers fall below 12 for 2 s, switch registration to the exterior matte bands and prompt the player to tilt the bowl toward a light source.
- **Run restart.** One restart per session at the current galaxy; the clock resets to 90 s, luminosity to 100, and the seed count to the galaxy table value.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `PropTracker` | Rim fit, `P` pose, loss/re-acquire FSM, gravity-corrected V |
| `BowlGeometry` | Cap equations, projection, rim torus, atlas UVs |
| `HandContacts` | Per-joint signed distance, hysteresis, contact records |
| `StirAnalyzer` | Projection, `Ω_s`, `v_ρ`, depth gains, reversal commit |
| `NebulaField` | 128² disc advection, vortex/radial acceleration, drag |
| `CloudRegistry` | Species, mass, pairs, spatial hash, fusion/nova tests |
| `SeedSystem` | Dark seeds, comet transition, rim ejection |
| `LuminosityModel` | Nova cost, seed sink, star gain, fail check |
| `GalaxyDirector` | Five-galaxy table, timers, win/lose resolution |
| `ScoreService` | Stars, comets, time bonus, persistence |
| `AudioDirector` | Event table, spatialisation, ducking |
| `SessionFSM` | State table, tick log, replay snapshots |

Event flow: `HandTracking (72 Hz) → HandContacts → StirAnalyzer → NebulaField → CloudRegistry → SeedSystem → LuminosityModel → GalaxyDirector → ScoreService → AudioDirector/Renderer`. `SessionFSM` subscribes to timers, luminosity, and pose loss and owns every state change. Rendering interpolates cloud positions between the last two 90 Hz ticks and extrapolates at most 1 frame.

```text
function update_stir(contact, P, dt):
    if contact.age < 30 ms or abs(contact.signed_distance) > 5 mm:
        return zero_field()
    q = project_to_cap(contact.point, R=75.0, zc=20.0)
    h = clamp(-q.z, 8.0, 50.0)
    vt = tangent(contact.vel - P.origin_vel, cap_normal(q))
    omega = clamp(cross(q.xy, vt).z / max(dot(q.xy, q.xy), 400.0), -4.5, 4.5)
    if sign(omega) != sign(previous_omega):
        reversal_timer += dt
        if reversal_timer < 120 ms:
            omega = previous_omega * 0.85
    else:
        reversal_timer = 0.0
    g_deep = smoothstep(18.0, 35.0, h)
    k_field = 1.0 + 0.25 * g_deep
    return Field(omega * k_field, radial_accel(omega), tau_theta=0.35, tau_drag=0.9)
```

```text
function resolve_cloud_pairs(clouds, dt):
    rebuild_spatial_hash(clouds, cell=4.0)
    for (a, b) in candidate_pairs(clouds):
        if a.dead or b.dead: continue
        if distance(a.p, b.p) > 13.0 + a.r + b.r: continue
        if relative_speed(a, b) > 35.0: continue
        if a.species == b.species and a.rho < 92.0:
            spawn_star(midpoint(a.p, b.p)); score += 100
            mark_dead(a, b); luminosity += 10
        elif a.species != b.species:
            spawn_nova(midpoint(a.p, b.p)); score -= 0
            mark_dead(a, b); luminosity -= 12
            apply_outward_impulse(clouds, midpoint(a.p, b.p), 18.0)
    if luminosity <= 0: fsm.to(GALAXY_FAIL)
```

State is snapshotted at each galaxy boundary: cloud positions, velocities, species, luminosity, seed states, timer, and `P`. A crash restores the snapshot and replays up to 2 s of input. All tuning values live in one hot-reloadable `TuningConfig`.

## 16. Acceptance Test Checklist

1. **Prop fit.** Present the bowl at 0.9 m and sample 500 frames. Pass: `R ∈ [74.6, 75.4] mm`, `d ∈ [54.5, 55.5] mm`, rim RMS < 0.5 mm, and no frame with pose error > 1.2 mm.
2. **Cap projection.** Feed a fingertip at `(0, 0, −55)` and at `(72.28, 0, 0)`. Pass: disc radius 0.0 mm and 97.58 mm ± 0.3 mm, with `θ` 0° and 74.53° ± 0.2°.
3. **Stir sign.** Move the fingertip at 100 mm/s along `+ŷ` at `x = 50 mm`. Pass: `Ω_s = −2.0 rad/s` ± 3% (clockwise), and the field tightens the spiral.
4. **Dead zone.** Sweep stir speed from 0 to 40 mm/s in 5 mm/s steps. Pass: no field below 18 mm/s, field starts by 19 mm/s, and release hysteresis keeps it alive to 12 mm/s.
5. **Depth gains.** Compare fusion radius at `h = 10 mm` and `h = 45 mm`. Pass: 12.0 mm and 15.0 mm ± 0.3 mm, with a monotonic ramp between.
6. **Fusion.** Place two H clouds 11 mm apart with relative speed 20 mm/s. Pass: exactly one star, +100 score, luminosity +10, and both clouds removed.
7. **Nova.** Place H and C clouds 9 mm apart. Pass: one nova, luminosity −12, no star, and the outward shockwave moves a third cloud at ≥ 120 mm/s initially.
8. **Comet.** Drive a seed to `ρ = 86 mm` with `v_ρ = 160 mm/s`, then to the rim at 130 mm/s. Pass: comet state at 86 mm, ejection at the rim, +75 score, and one shadow stack cleared.
9. **Clock.** Run a galaxy with a synthetic zero-input hand. Pass: ignition 6.0 s, accretion 55.0 s, comet window 20.0 s, collapse check 9.0 s, and a fail at 90.0 s ± 1 tick.
10. **Occlusion.** Place the rim hand between the camera and the near rim. Pass: no nebula pixels leak onto the hand or lap over 600 frames, and the lip glow remains visible.
11. **Adaptive tuning.** Trigger two novas within 25 s. Pass: `r_f` increases by exactly 2.0 mm for the current galaxy and drift drops 10%, with no change to the clock.
12. **Frame budget.** Run galaxy 5 with 16 clouds, 5 seeds, and 2,400 particles. Pass: GPU frame < 11.1 ms, CPU game thread < 6 ms, particle count ≤ 2,400.
13. **Tracking recovery.** Hide the bowl for 1.4 s, then re-present. Pass: re-acquire ≤ 1.5 s, nebula state unchanged, no score change, and input restored within 200 ms.
14. **Endurance.** Run the 9 min scripted session with synthetic hands. Pass: no accumulated pose error > 2 mm, no luminosity drift without gameplay events, and memory growth < 5 MB.
