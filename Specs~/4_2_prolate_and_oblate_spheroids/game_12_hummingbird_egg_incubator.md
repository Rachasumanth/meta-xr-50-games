# Hummingbird Egg Incubator

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Hummingbird Egg Incubator |
| Slug | `hummingbird_egg_incubator` |
| Physical prop | Prolate polished hardwood egg (fine-grain beechwood or maple); major polar longitudinal semi-axis `a = 26.0 mm` (52.0 mm total length), equatorial transverse semi-axes `b = c = 16.0 mm` (32.0 mm equatorial diameter); mass 28.0 g; polished satin finish resting on a 120 mm circular wool-felt nest mat |
| Sub-shape & Family | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate and Oblate Spheroids (prolate, handheld / felt-mat rolling and rocking) |
| Scale class | Tabletop nest scale; nest saucer radius 60 mm (`r_nest = 60 mm`); embryonic zoom HUD 180 mm diameter; physical rocking angle `θ ∈ [−25°, +25°]`, axial roll `ψ ∈ [0°, 360°]` |
| Source concept quote | "Iridescent hummingbird egg. Rocking the egg gently along its prolate axis on soft felt distributes maternal body heat evenly to the fragile embryo, avoiding thermal stress cracks and cool drafts." |
| Physical input | Gentle, calibrated multi-finger rolling and longitudinal rocking using dominant thumb, index, and middle fingertips, coupled with cupped palm warmth shielding from non-dominant hand against icy alpine drafts |
| Session length | 5 developmental incubation stages (waves), 40–55 s each; total session duration ≈4.5 minutes |
| Lose condition | Embryo vitality index reaches 0 % (`V_embryo ≤ 0`), shell thermal stress fractures exceed 100 % (`CrackStress ≥ 1.0`), embryonic core temperature drops below 33.0 °C (hypothermic arrest) or exceeds 41.0 °C (hyperthermic denaturation) |

## 1. Fantasy & Core Loop

High in the mist-shrouded canopy of the Andean cloud forest, you are the avian guardian of an iridescent, gossamer-shelled hummingbird egg—the smallest and most delicate warm-blooded life on Earth. Resting in a cup of woven spider silk and moss, the miniature 28 g prolate wooden egg between your fingers represents a living, translucent embryo whose microscopic heart flutters at 720 beats per minute.

The cloud forest climate is merciless: sudden arctic gusts howl through the mossy branches, plunging quadrants of the egg into hypothermic shock, while the tiny embryo inside constantly sinks toward the bottom shell due to gravity. If the egg sits motionless for more than 4.5 seconds, the fragile blastoderm adheres to the inner shell membrane, causing fatal developmental arrest.

To nurture life into the world, you must execute the timeless maternal craft of incubation:
1. Rock the egg rhythmically along its elongated prolate axis (`x̂_P`) and roll it across the felt nest bed to distribute warmth evenly across all 8 thermal quadrants.
2. Cup your non-dominant hand as an aerodynamic windbreak against howling alpine drafts.
3. Lightly press warm fingertip pads onto cold thermal patches to conduct maternal body heat directly through the shell without crushing its delicate calcium lattice.

### 30–60s Active Incubation Cycle

1. **Thermal Scan & Heartbeat Readout (0.0–1.5 s)**: The holographic embryo overlay visualizes inner yolk vascularization and core temperature (target: 37.5 °C). A 3D heat-map projects cold hypothermic zones (blue/violet, < 35.0 °C) and overheating hotspots (crimson, > 39.0 °C).
2. **Longitudinal Rocking & Axial Roll (1.5–6.0 s)**: Player gently oscillates the egg along its prolate axis (`x̂_P`) at 0.8–1.8 Hz while simultaneously rolling the equatorial perimeter. Rolling continuously mobilizes the vitelline fluid, resetting the embryo adhesion timer and equalizing temperature between the top and bottom poles.
3. **Draft Interception & Cupped Shielding (6.0–10.0 s)**: An icy mountain draft sweeps into the nest from a randomized azimuth vector `d_wind`. Non-dominant hand cups into a concave shield upwind, while dominant fingers tilt the narrower prolate pole toward the wind vector to minimize aerodynamic exposure.
4. **Targeted Fingertip Thermal Transfer (10.0–15.0 s)**: Identifying a lingering hypothermic quadrant (e.g. Posterior Ventral pole), the player rests the warm pad of `R_index_tip` on the cold sector (`F_contact ≈ 0.4 N`). Maternal bloodflow warmth conducts into the shell at `+1.8 °C/s` until the sector glows golden-amber.
5. **Stress Mitigation & Pressure Modulation (15.0–20.0 s)**: If the player presses too hard (> 0.9 N) or creates an excessive thermal gradient (> 4.2 °C across adjacent sectors), microscopic stress cracks spiderweb across the shell with alarming creaks. Player immediately reduces grip force and rocks gently to diffuse the stress.
6. **Developmental Milestone Assessment (20.0–25.0 s)**: Vitality scores accumulate; the embryo's capillary network expands visibly, the heartbeat steadies, and the incubation cycle transitions into the next developmental clutch wave.

### 5-Wave Developmental Clutch Progression

- **Wave 1 (Day 3: Cleavage & Blastoderm)**: Stable baseline conditions. Calibrated rolling tutorials; target temperature 37.5 ± 0.6 °C; prevents early membrane adhesion.
- **Wave 2 (Day 6: Primitive Streak & Neural Tube)**: Gentle alpine breezes (wind speed 1.5 m/s); requires basic non-dominant hand cupping and steady 1.2 Hz rocking.
- **Wave 3 (Day 9: Chorioallantoic Vascularization)**: Rapid capillary branching. Shifting draft vectors require swift re-orientation of the prolate poles and precision fingertip heat injection.
- **Wave 4 (Day 12: Feather Buds & Torpor Defense)**: Nighttime cold snap drops ambient nest temperature to 18.0 °C. High-cadence rotational rolling required to prevent rapid edge chilling.
- **Wave 5 (Day 14: Pipping & Hatching Finale)**: The chick inside begins rhythmic pipping (tapping the shell from within with its egg tooth). Player must maintain exact thermal equilibrium (37.5 ± 0.3 °C) and gentle rocking to guide the chick as the shell cracks open into a glowing golden birth.

## 2. Platform Baseline

- **Hardware**: Standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No physical controllers; the only tangible prop is the registered prolate wooden egg.
- **Joint Naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc, mcp, pip, dip, tip (thumb: cmc, mcp, ip, tip). Example: `R_thumb_tip`, `L_palm`.
- **Prop Registration**: At session start, the player places the prolate egg on the felt nest saucer; the vision pipeline detects the table support plane `T`, fits an oriented 3D ellipsoid of revolution (`a = 26.0 mm`, `b = c = 16.0 mm`) from multi-view silhouette contours at 72 Hz, and computes 6-DoF pose `P`.
- **Coordinate Frames**:
  - `W`: Room world frame (Y up, cm/meters).
  - `T`: Support surface frame (detected table/nest plane; origin at nest center, `+ŷ_T` normal pointing up, `+x̂_T` forward, `+ẑ_T = x̂_T × ŷ_T` to right).
  - `P`: Prop local frame (origin at center of prolate spheroid; `+x̂_P` major longitudinal polar axis pointing toward narrow apex, `+ŷ_P` and `+ẑ_P` equatorial transverse axes).
  - `V`: Virtual play frame (nest arena coordinate system, centered on nest saucer in `T`).
- **Contact Detection**: Joint contact detected when signed distance from hand joint proxy sphere to the prop's prolate collider is < 4.5 mm; release hysteresis 7.5 mm; debounce threshold 30 ms.
- **Physics Engine**: Fixed 90 Hz timestep, max 2 sub-steps (11.1 ms / 5.55 ms); prolate rolling and rocking equations integrated on compliant felt mat.
- **Rendering & Compositing**: Color passthrough base layer; holographic subsurface scattering egg shader; 8-zone thermal gradient textures; 3D vascularized embryo model visible through semi-translucent shell; skeletal hand mesh occlusion.
- **Feedback**: Dynamic binaural spatial audio (delicate embryonic heartbeat, rushing mountain wind drafts, calcium shell stress creaks, maternal cooing) and visual VFX (heat shimmer, cold frost particles, golden capillary pulses).

### 2.1 Game-Specific Overrides

- **Prop Frame `P` & Exact Spheroid**: Prolate spheroid with major longitudinal semi-axis `a = 26.0 mm` (52.0 mm total length) and equatorial semi-axes `b = c = 16.0 mm` (32.0 mm equatorial diameter). Aspect ratio `a/b = 1.625`. Spheroid equation in `P`:
  `x²/a² + y²/b² + z²/c² = x²/(26.0)² + (y² + z²)/(16.0)² = 1.0`
- **Prolate Rolling & Rocking Kinematics**:
  On a flat horizontal nest surface, the contact point `p_c` depends on longitudinal pitch angle `θ_pitch`:
  - When lying flat (`θ_pitch = 0°`), contact is on the circular equator where meridional radius of curvature is `R_meridian = a²/b = 26.0²/16.0 = 42.25 mm`, and transverse radius of curvature is `R_transverse = b = 16.0 mm`.
  - When rocking toward either pole (`|θ_pitch| → 90°`), contact approaches the apex where radius of curvature sharpens to `R_apex = b²/a = 16.0²/26.0 = 9.846 mm`.
  - The restoring gravitational torque on the compliant felt mat produces a natural rocking frequency:
    `ω_rock = √[ (m · g · (R_meridian - b)) / I_pitch ] = √[ (0.028 · 9.81 · (0.04225 - 0.016)) / 5.219 × 10⁻⁶ ] ≈ 37.1 rad/s (5.9 Hz)`.
    On the compliant wool-felt mat (`k_felt = 14 N/mm`, damping `ζ = 0.32`), the rocking frequency is cushioned to an ergonomic **1.2–2.2 Hz**.
- **Virtual Frame `V` Anchoring**: `V` is anchored to `T` at the nest saucer center: `origin_V = origin_T`. The virtual nest has an outer boundary radius `r_nest = 60.0 mm`. If the physical egg rolls beyond 55 mm, gentle soft-spring boundary forces nudge it back toward the nest center.
- **Deviations from Baseline**:
  - The real wooden egg is completely opaque; the system projects a multi-pass holographic subsurface scattering shader that matches physical lighting and reveals the glowing inner embryo inside the physical shell.
  - Maternal thermal transfer is a virtual thermodynamic simulation running across an 8-zone finite element mesh mapped over the registered spheroid surface.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Play Posture | Seated at desk/table with both elbows and forearms comfortably resting | Stabilizes fine digital motor control; prevents arm fatigue during delicate manipulation |
| Nest Saucer Patch | 160 mm diameter circular felt mat placed directly in front of sternum | Prevents hardwood egg from skittering or clattering; provides compliant tactile friction |
| Prop Rolling Envelope | `r ≤ 50 mm` from nest center, `θ_pitch ∈ [−25°, +25°]` | Keeps movements within comfortable three-finger fingertip rolling range |
| Finger Grip Type | Delicate digital tripod grip (thumb pad, index pad, middle pad) | Distributes 0.2–0.6 N contact forces across digital pads; avoids pinching or crushing fatigue |
| Thermal Shielding Posture | Non-dominant hand curved into a semi-spherical cup 40–70 mm upwind | Passive static brace posture; low muscular load |
| Maximum Normal Force | 0.85 N (visual warning at 0.70 N, fracture at 0.95 N) | Well below human finger fatigue threshold; reinforces delicate infant handling fantasy |
| Inter-Wave Breathing Pause | 12 s between incubation stages | Allows player to relax hand muscles while viewing embryonic growth summary |
| Viewing Angle & Distance | 350–450 mm eye-to-egg distance, 40° downward line-of-sight | Optimal depth resolution for color passthrough and skeletal joint tracking stability |

### Fatigue Mitigations
1. **Low Inertia Prop (28 g)**: Hardwood prop requires less than 0.15 N of tangential force to roll or rock, minimizing tendon stress.
2. **Dynamic Rocking Damping**: Compliant felt mat absorbs high-frequency vibrations; player only needs to provide gentle periodic impulses at 1.0–1.5 Hz.
3. **Bimanual Specialization**: Dominant hand performs agile digital manipulation; non-dominant hand acts as a stationary thermal wind shield, distributing cognitive and motor fatigue evenly.

## 4. Prop Geometry & Registration

```
                           +x̂_P (Longitudinal Polar Axis, a = 26.0 mm)
                                     ▲
                                   .-│-.
                                 .'  │  '.   Narrow Apex (Pole 1)
                                /    │    \  R_apex = 9.85 mm
                               |     │     |
              ◄────────────────┼─────┼─────┼────────────────►
              -b = 16.0 mm     |     │     |   +b = 16.0 mm (Equator)
              (+ŷ_P / +ẑ_P)     \    │    /    R_meridian = 42.25 mm
                                 '.  │  .'
                                   '-│-'     Blunt Base (Pole 2)
                                     ▼
```

### Prop Dimensions & Mass Properties

- **Major Longitudinal Polar Semi-Axis**: `a = 26.0 mm` (length = 52.0 mm).
- **Equatorial Transverse Semi-Axes**: `b = c = 16.0 mm` (diameter = 32.0 mm).
- **Prolate Aspect Ratio**: `γ = a / b = 26.0 / 16.0 = 1.625`.
- **Prolate Eccentricity**: `e = √(1 - b²/a²) = √(1 - 16²/26²) = √(1 - 0.3787) = √0.6213 ≈ 0.7882`.
- **Prop Mass**: `m = 28.0 g` (0.028 kg).
- **Volume**: `V = (4/3) · π · a · b · c = (4/3) · π · 26.0 · 16.0 · 16.0 = 27,876 mm³ ≈ 27.88 cm³`.
- **Mean Density**: `ρ = 28.0 g / 27.88 cm³ ≈ 1.004 g/cm³` (solid high-density beechwood).
- **Moments of Inertia**:
  - Spin Axis (Longitudinal Polar `x̂_P`):
    `I_x = (2/5) · m · b² = 0.4 · 0.028 kg · (0.016 m)² = 2.867 × 10⁻⁶ kg·m²`.
  - Transverse Pitch/Yaw Axes (`ŷ_P`, `ẑ_P`):
    `I_y = I_z = (1/5) · m · (a² + b²) = 0.2 · 0.028 kg · (0.026² + 0.016²) m² = 5.219 × 10⁻⁶ kg·m²`.

### Surface-Feature Atlas (8-Zone Thermal Grid)

The egg surface is partitioned into **8 Thermodynamic Sectors** (4 Longitudinal Quadrants × 2 Hemispheres) for thermal diffusion and embryonic viability monitoring:

```
            Polar Cap 1: Narrow Apex (x_P ∈ [12, 26] mm)
            ┌──────────────┬──────────────┐
            │  Zone 1: AN  │  Zone 2: PN  │  (Anterior / Posterior Narrow)
            ├──────────────┼──────────────┤
            │  Zone 3: AB  │  Zone 4: PB  │  (Anterior / Posterior Blunt)
            └──────────────┴──────────────┘
            Polar Cap 2: Blunt Base (x_P ∈ [-26, -12] mm)
            (Equatorial Flanks: Left / Right subdivided around Y/Z axes)
```

| Zone ID | Name | Coordinate Extent in `P` | Embryonic Association | Thermal Sensitivity |
|---|---|---|---|---|
| `Z1_AN` | Anterior-Narrow | `x > 0, y > 0` | Cephalic Neural Tube | High (`±0.5 °C` variance induces bradycardia) |
| `Z2_PN` | Posterior-Narrow| `x > 0, y ≤ 0` | Optical Vesicles & Cranial Crest | High |
| `Z3_AL` | Anterior-Left Flank | `x ∈ [−12, +12], z > 0`| Vitelline Artery Network | Medium |
| `Z4_AR` | Anterior-Right Flank| `x ∈ [−12, +12], z ≤ 0`| Vitelline Venous Sinus | Medium |
| `Z5_AB` | Anterior-Blunt | `x < 0, y > 0` | Yolk Sac Nutritive Core | Low (thermal reservoir) |
| `Z6_PB` | Posterior-Blunt| `x < 0, y ≤ 0` | Allantoic Waste Sac | Low |
| `Z7_PL` | Posterior-Left Flank| `x ∈ [−12, +12], z > 0`| Inner Shell Air Cell | Critical (oxygen exchange) |
| `Z8_PR` | Posterior-Right Flank| `x ∈ [−12, +12], z ≤ 0`| Chorioallantois | Critical |

### Collider Inventory

| Collider ID | Type | Geometry / Dimensions | Coordinate Frame | Functional Purpose |
|---|---|---|---|---|
| `COL_EGG_CORE` | Prolate Spheroid | `a = 26.0 mm`, `b = c = 16.0 mm` | Prop Frame `P` | Primary physical contact, rolling dynamics, and visual occluder |
| `COL_APEX_CAP` | Sphere | `r = 9.85 mm` | Offset `(+16.15, 0, 0)` in `P` | Narrow pole contact point and draft stagnation zone |
| `COL_BASE_CAP` | Sphere | `r = 13.5 mm` | Offset `(−12.5, 0, 0)` in `P` | Blunt pole contact point and air-cell thermal buffer |
| `COL_NEST_MAT` | Cylinder / Saucer | `r = 60.0 mm`, `h = 8.0 mm` | Nest Frame `V` | Compliant wool-felt contact plane with friction `μ = 0.38` |
| `COL_WIND_SHIELD`| Curved Hull Mesh| 120 mm wide, 60 mm high arc | Tracked `L_palm` | Non-dominant aerodynamic windbreak volume |

### Optical Registration & Pose Fitting

1. **Nest Surface Identification**: The flat table plane is detected with plane normal residual < 0.8 mm.
2. **Ellipse Feature Extraction**: The camera feed extracts the prolate egg's silhouette at 72 Hz. The ratio of major to minor projected axes verifies prolate identity:
   `γ_proj = a_proj / b_proj ∈ [1.0, 1.625]` depending on viewing pitch.
3. **Levenberg-Marquardt Spheroid Solver**: 6-DoF pose `P` is optimized by minimizing edge reprojection error. Pose confidence gate requires RMS error < 0.55 mm and major axis orientation variance < 1.8°.
4. **Axial Spin Yaw Tracking**: Because a prolate wooden egg has natural microscopic wood grain lines along its longitudinal axis, an optical flow patch tracker tracks spin angle `ψ_roll` around `x̂_P` at 72 Hz.

## 5. Hand Tracking Input Map

| ID | Gesture / Input | Active Hand / Joints | Egg Surface Region | Semantic Gameplay Output |
|---|---|---|---|---|
| `IN_LONG_ROCK` | Longitudinal Rocking | `R_index_tip`, `R_middle_tip` | Prolate Flank (`Z3/Z4/Z7/Z8`) | Imparts longitudinal pitch oscillation `θ_pitch ∈ [−25°, +25°]`; stirs vitelline fluid |
| `IN_AXIAL_ROLL` | Fingertip Axial Roll | `R_thumb_tip`, `R_index_pad` | Equatorial Perimeter | Imparts axial roll `ω_roll` around `x̂_P`; prevents membrane adhesion |
| `IN_WARM_PRESS` | Steady Fingertip Touch | `R_index_tip` pad (`F ∈ [0.2, 0.7] N`)| Cold Zone (`Z1`–`Z8`) | Conducts maternal heat `dQ/dt` directly into target sector |
| `IN_PALM_SHIELD` | Cupped Windbreak | `L_palm`, `L_fingers` curved arc | Air space 40–80 mm upwind of nest | Creates aerodynamic wind shadow, cutting draft chill by 85 % |
| `IN_OVERPRESSURE`| Excessive Grip Force | `R_thumb_tip` + `R_index_tip` > 0.85 N | Any Egg Region | Triggers structural stress; audio crack warning; risks shell fracture |
| `IN_DUAL_CUP` | Bimanual Hover Warming | Both palms cupped over egg (`dist < 35 mm`)| Entire Upper Shell | Uniform ambient warmth bonus (+0.4 °C/s to all top zones) |
| `IN_PEEK_EMBRYO` | Gaze Fixation on Core | Head pose gaze directed at egg center | Egg Interior | Shifts holographic shell alpha from 0.85 to 0.35, revealing embryo anatomy |
| `IN_REST_NEST` | Hands Off / Neutral | All joints > 30 mm away from egg | — | Egg coasts on felt mat; ambient convection and draft cooling apply |

## 6. Input Kinematics & Signal Processing

```
72 Hz Hand Tracking ────► OneEuro Filter ────► Contact Force Estimator ────► Thermal Conduction
[R_index_tip / thumb]     (fc=1.8, β=0.03)      F = k_pad · Δd               (dQ_hand = k_c·A·ΔT)
                                                                                     │
72 Hz Egg Pose P ──────► Pitch / Roll Extractor ──► Adhesion Timer ────────► Embryo Viability
(x_P, θ_pitch, ψ_roll)    Bandpass Filter           Reset if |ω_roll| > 0.4        Model
```

### Prolate Kinematic Extraction

1. **Longitudinal Pitch Angle (`θ_pitch`)**:
   Measured as the elevation angle of major axis `x̂_P` relative to the nest table plane:
   `θ_pitch = arcsin(clamp(x̂_P · ŷ_T, −1.0, 1.0))`
   Rocking velocity: `ω_pitch = dθ_pitch / dt`.
2. **Axial Roll Velocity (`ω_roll`)**:
   Angular velocity around the major longitudinal axis `x̂_P`:
   `ω_roll = ω_P · x̂_P` [rad/s].
3. **Rocking Cadence Bandpass Filter**:
   Longitudinal rocking is filtered through a 2nd-order Butterworth bandpass filter with passband `[0.6, 2.5] Hz`. Rhythmic rocking within this band generates vitelline circulation:
   `CirculationFactor = clamp(|ω_pitch_filt| / 1.5, 0.0, 1.0)`.

### Thermodynamic Simulation Model (8-Zone Finite Element)

Each of the 8 surface sectors `i ∈ [1, 8]` tracks temperature `T_i(t)`. The heat equation for sector `i` is:

`C_shell · (dT_i / dt) = q_hand,i + q_diff,i - q_draft,i - q_nest,i`

Where:
- Shell sector heat capacity `C_shell = 4.2 J/°C`.
- **Maternal Hand Conduction**:
  `q_hand,i = k_conduction · A_contact,i · (T_hand - T_i)`
  with `T_hand = 38.2 °C`, `k_conduction = 12.5 W/(m²·°C)`, and contact area `A_contact,i` derived from fingertip penetration depth:
  `A_contact,i = π · (r_tip² - dist_joint_to_shell²)`.
- **Inter-Sector Thermal Diffusion**:
  Thermal conduction between adjacent sectors:
  `q_diff,i = ∑_{j ∈ Neighbors(i)} k_diff · (T_j - T_i)`
  with `k_diff = 0.85 W/°C`. Rolling the egg actively increases `k_diff` by up to 300 % due to vitelline fluid convection!
- **Convective Draft Chill**:
  `q_draft,i = h_wind · A_zone · (1.0 - ShieldFactor) · max(n̂_i · d_wind, 0) · (T_i - T_ambient)`
  Where:
  - Wind heat transfer coefficient `h_wind = 8.0 + 14.0 · √(v_wind)`.
  - `ShieldFactor ∈ [0.0, 0.85]` determined by non-dominant hand position between wind source and egg.
  - `n̂_i` is the outward normal of sector `i`.
- **Felt Nest Insulative Loss**:
  Contact with the felt mat provides insulation: `q_nest,i = 0.15 · (T_i - T_nest)`.

### Membrane Adhesion Dynamics & Vitality Index

The embryo inside must not stay static against the lower shell:

1. **Adhesion Accumulator (`A_adhesion`)**:
   `dA_adhesion / dt = (|ω_roll| < 0.35 rad/s ∧ |ω_pitch| < 0.25 rad/s) ? +0.22 s⁻¹ : −0.45 s⁻¹`
   Clamped to `[0.0, 1.0]`. If `A_adhesion > 0.80`, embryonic membrane binding warning initiates. At `1.0`, vascular rupture occurs.
2. **Embryonic Core Temperature (`T_core`)**:
   `T_core(t + dt) = T_core(t) + (1 / τ_core) · (T_mean_shell - T_core) · dt`
   with thermal inertia time constant `τ_core = 8.5 s`.
3. **Vitality Index (`V_embryo ∈ [0, 100] %`)**:
   `dV_embryo / dt = +1.2 %/s · (1.0 - |T_core - 37.5 °C| / 1.5) - 4.5 %/s · (A_adhesion > 0.7 ? A_adhesion : 0)`
4. **Shell Structural Stress & Fracture Mechanics**:
   `Stress(t) = k_force · (F_finger - 0.70 N) + k_thermal · max_{i, j}(|T_i - T_j| - 4.0 °C)`
   If `Stress > 0`, micro-crack accumulator advances:
   `CrackDamage += Stress · dt`. If `CrackDamage ≥ 1.0`, shell shatters (immediate fail).

## 7. Virtual World Layout & Coordinate Mapping

```
                                NEST ARENA FRAME (V)
                   (Top-Down View of 120 mm Circular Nest Saucer)
                                     
                               [Wind Vector d_wind]
                                      ▼  ▼  ▼
                             ┌──────────────────────┐
                             │   L_palm Shield Arc  │
                             │      ( ) ) ) )       │
                             └──────────┬───────────┘
                                        ▼
                                  .-----------.
                                .'             '.
                               /    Zone 1 / 2   \
                              |   Narrow Apex     |
                             |  [=======●=======]  | ◄── Physical Prolate Egg
                              |    Blunt Base     |     (52 mm × 32 mm)
                               \    Zone 5 / 6   /
                                '.             .'
                                  '-----------'
                                        ▲
                             ┌──────────┴───────────┐
                             │ Dominant R_fingers   │
                             │ (Rocking & Warming)  │
                             └──────────────────────┘
```

### Coordinate Transforms

- **`P_to_T`**: `p_T = R_P_to_T · p_P + p_P_in_T`.
- **`T_to_V`**: The virtual nest is co-located with table frame `T`: `p_V = p_T`.
- **Embryo Visual Anchor**: The holographic 3D embryo model is anchored directly at prop origin `P`, matching real egg rotations and wobbles with zero latency:
  `p_embryo_V = p_P_in_V`, `q_embryo_V = q_P_in_V`.
- **Holographic HUD Placards**:
  - `HUD_Heartbeat`: Anchored 45 mm above egg center (`p_P + (0, 45, 0)_V`). Displays real-time BPM waveform.
  - `HUD_ThermalCompass`: Ring around nest saucer showing wind vector azimuth and chill intensity.

### Virtual Entity Inventory

| Entity Name | Frame | Local Offset `(x, y, z)` [mm] | Dimensions | Visual Description |
|---|---|---|---|---|
| Nest Saucer Mat | `V` | `(0, 0, 0)` | Radius 60 mm, thickness 4 mm | Woven lichen, green moss, and soft hummingbird feathers |
| Shell Thermal Mesh | `P` | `(0, 0, 0)` | Prolate 52 × 32 mm | Semi-translucent calcite shell with 8-zone dynamic thermographic emission |
| Embryo Chick Model | `P` | `(0, 0, 0)` | 30 × 18 mm anatomical model | Curved iridescent chick embryo with pulsing vitelline blood vessels |
| Yolk Sac Core | `P` | `(−5, −2, 0)` | Ellipsoid 16 × 12 mm | Translucent golden sphere glowing with maternal warmth |
| Alpine Draft Streamlines| `V` | Height 10–30 mm above nest | 180 mm long flow ribbons | Pale blue wisps with frost crystal particles flowing along `d_wind` |
| Palm Wind Shadow Hull | `V` | Centered on `L_palm` | 90 × 40 mm curved barrier | Semi-transparent azure forcefield showing aerodynamic deflection |
| Shell Stress Crack VFX | `P` | Dynamic at high-stress points | Procedural line segments | Glowing white hairline fractures with calcium dust particles |

## 8. Gameplay Systems & Rules

### Stage Objectives & Incubation Scoring

Each of the 5 developmental clutch stages requires surviving for **45.0 seconds** while maintaining embryo vitality and preventing shell fractures.

1. **Vitality Score Formula**:
   At the end of a 45-second stage, points are awarded based on final vitality `V_embryo` and thermal precision:
   `StageScore = (V_embryo / 100) · 1000 + Bonus_Equilibrium - Penalty_Stress`
   Where:
   - `Bonus_Equilibrium = max(0, 500 - 150 · σ_T)` (where `σ_T` is the standard deviation of temperatures across all 8 zones).
   - `Penalty_Stress = 400 · CrackDamage`.
   - Maximum possible score per stage: **1,500 Points** (Total match max: **7,500 Points**).

2. **Thermal Target Window**:
   - Optimal Band: `37.2 °C ≤ T_core ≤ 37.8 °C` (vitality grows at maximum `+1.2 %/s`).
   - Warning Band: `35.5 °C ≤ T_core < 37.2 °C` or `37.8 °C < T_core ≤ 39.0 °C` (vitality growth stalls, embryonic heart slows).
   - Danger Hypothermia: `T_core < 35.5 °C` (vitality drains at `−2.5 %/s`).
   - Danger Hyperthermia: `T_core > 39.0 °C` (protein degradation; vitality drains at `−3.5 %/s`).

3. **Loss Conditions**:
   - `FAIL_VITALITY`: `V_embryo ≤ 0 %` (embryo ceases vital signs).
   - `FAIL_CRACK`: `CrackDamage ≥ 1.0` (shell suffers catastrophic structural fracture).
   - `FAIL_ADHESION`: `A_adhesion = 1.0` for > 2.0 continuous seconds (blastoderm permanently fuses to shell membrane).

4. **Victory Criteria**:
   Successfully complete Stage 5 (Pipping & Hatching) with `V_embryo ≥ 60 %`.

## 9. Game State Machine

```
┌─────────┐      ┌──────────────┐      ┌────────────┐      ┌─────────────┐
│  BOOT   ├─────►│  REGISTER_P  ├─────►│ NEST_SET   ├─────►│ STAGE_INTRO │
└─────────┘      └──────────────┘      └────────────┘      └──────┬──────┘
                                                                  │
┌──────────────┐      ┌──────────────┐      ┌───────────┐         │
│ HATCH_VICTORY│◄─────┤ STAGE_CLEAR  │◄─────┤ INCUBATE_ ├─────────┘
└──────────────┘      └──────┬───────┘      │ ACTIVE    │
                             │              └─────┬─────┘
                             ▼                    │
                      ┌──────────────┐            ▼
                      │ STAGE_NEXT   │     ┌─────────────┐
                      └──────────────┘     │ FAIL_RESET  │
                                           └─────────────┘
```

### State Machine Transition Table

| State Name | Entry Conditions | Continuous Operations | Exit Guard / Transition Condition | Next State |
|---|---|---|---|---|
| `BOOT` | Scene loaded | Initialize thermal grid, load spatial audio cues | Tracking pipelines ready | `REGISTER_P` |
| `REGISTER_P` | Nest saucer identified | Fit prolate spheroid contour; calculate `a = 26.0`, `b = 16.0 mm` | Spheroid fitted (`RMS < 0.55 mm`, held 1.0 s) | `NEST_SET` |
| `NEST_SET` | Prop pose valid | Guide player to center prolate egg on felt mat; display welcome UI | Egg at nest center (`r < 15 mm`) | `STAGE_INTRO` |
| `STAGE_INTRO` | Stage start | Announce developmental day (e.g. "Day 6: Neural Tube"); establish initial wind | Intro sequence timer `t ≥ 3.0 s` | `INCUBATE_ACTIVE` |
| `INCUBATE_ACTIVE`| Stage begins | Execute 90 Hz thermodynamic diffusion, sample finger contacts, evaluate wind chill | Stage clock `t ≥ 45.0 s` AND `V_embryo > 0` | `STAGE_CLEAR` |
| `INCUBATE_ACTIVE`| Vitality drops to zero | Fade audio to muted silence; display flatline ECG | `V_embryo ≤ 0` | `FAIL_RESET` |
| `INCUBATE_ACTIVE`| Excessive force applied | Play loud calcium snap sound; crack decal covers shell | `CrackDamage ≥ 1.0` | `FAIL_RESET` |
| `STAGE_CLEAR` | Stage timer elapsed | Tally stage score, play maternal trill sound, display vascular growth animation | Player holds egg steady (`||v|| < 5 mm/s`) 1.5 s | Stage < 5: `STAGE_NEXT`; Stage = 5: `HATCH_VICTORY` |
| `STAGE_NEXT` | Score acknowledged | Advance stage counter `S ← S + 1`; increase wind complexity | Egg returned to nest center | `STAGE_INTRO` |
| `FAIL_RESET` | Failure condition met | Display developmental loss reason; offer instant restart | Player taps virtual "Try Again" button | `STAGE_INTRO` |
| `HATCH_VICTORY`| Stage 5 completed | Shell breaks away in glowing golden shards; hummingbird chick takes first breath | Player reaches out to gently touch chick | `SESSION_END` |

## 10. Physics & Collision

### Collider Interaction Matrix

| Collider Pair | Interaction Type | Physics Parameters | Gameplay Response |
|---|---|---|---|
| `COL_EGG_CORE` × `COL_NEST_MAT` | Compliant Rolling Contact | Restitution `e = 0.05`, rolling friction `μ_r = 0.04`, static friction `μ_s = 0.38` | Prolate rolling & rocking; felt mat damping absorbs abrupt impacts |
| `COL_EGG_CORE` × Dominant Fingertips | Soft-Body Penalty Spring | Contact stiffness `k_pad = 280 N/m`, max force 1.2 N | Translates hand motion into rolling torque; computes contact area for thermal conduction |
| `COL_WIND_SHIELD` × Alpine Draft Particles | Velocity Deflection Field | Deflection normal `n̂_shield`, damping 0.85 | Deflects draft streamlines away from egg; computes `ShieldFactor` |
| `COL_EGG_CORE` × Table Surface | Hard Boundary Clamp | Restitution `e = 0.12`, `y_min = 16.0 mm` | Prevents egg from clipping through physical table |

### Dynamics Constants & Solver Details

- **Time Step**: 90 Hz fixed timestep with 2 sub-steps (`dt = 5.55 ms`).
- **Prolate Rocking Equation of Motion**:
  `I_pitch · (d²θ / dt²) + c_felt · (dθ / dt) + m · g · (R_eff(θ) - b) · sin(θ) = τ_finger + τ_felt`
  Where effective radius of curvature `R_eff(θ)` interpolates between `a²/b` (42.25 mm) at equator and `b²/a` (9.85 mm) at apex:
  `R_eff(θ) = (a² · b²) / (a² · sin²θ + b² · cos²θ)¹·⁵`.
- **Felt Compliance Sink**: The felt mat depresses by `Δy = F_normal / k_felt` (up to 1.5 mm), creating an angular cradle that stabilizes rocking and prevents runaway skittering.

## 11. Rendering, Occlusion & Compositing

### Multi-Pass Translucent Shell Shader

1. **Pass 1 (Depth Mask)**: Renders the prolate spheroid depth proxy into the Z-buffer to occlude virtual elements behind the physical prop.
2. **Pass 2 (Inner Embryo Geometry)**: 3D embryonic chick, heart, and pulsating vitelline capillary tree rendered inside the egg volume.
3. **Pass 3 (Subsurface Scattering Shell)**: Custom HLSL shader sampling an 8-zone thermal color LUT. Translucency varies with shell thickness (thicker at blunt base, gossamer-thin at apex).
4. **Pass 4 (Thermal Heatmap Glow)**: Additive emissive pass mapping cold hypothermia (violet/blue), optimal warmth (golden-amber), and overheating hotspots (crimson).
5. **Pass 5 (Surface Caustics & Stress Cracks)**: Parallax-mapped shell micro-textures and dynamic crack decal geometry.

### Occlusion Hierarchy

- **Real Hand Occlusion**: Skeletal hand mesh writes depth. When the player cradles or rolls the egg, fingers render *in front* of the translucent shell and internal embryo.
- **Palm Shield Occlusion**: Non-dominant cupped palm is semi-occluded by aerodynamic draft streamlines, making wind flow and boundary shielding instantly legible.
- **Nest Mat Contact Shadow**: Dynamic ambient-occlusion contact ellipse beneath the prolate egg anchors it physically onto the felt mat.

### VFX Triangles & Particle Budgets

- **Embryo Chick Model**: 3,840 triangles with 30-bone skeletal rig for breathing and heart dilation.
- **Prolate Shell Mesh**: 960 triangles (smooth UV sphere deformed by `x · 1.625`).
- **Draft Streamline Particles**: Max 250 concurrent GPU ribbon particles with soft alpha fade.
- **Calcium Dust & Spark Particles**: Max 80 particles spawned only during stress cracks or pipping.
- **Total Transparent Overdraw**: Peak 1.9× screen area.

## 12. Audio & Feedback

| Event Trigger | Spatial Sound Cue | Frequency / Synthesis Profile | Visual Cue |
|---|---|---|---|
| Embryo Heartbeat | Binaural rhythmic pulse | Dual-tone thump: 65 Hz fundamental + 140 Hz slap; cadence reflects `T_core` (400–850 bpm) | Golden pulsing glow radiating from egg center; ECG waveform on HUD |
| Alpine Draft Incoming | Howling wind whistle | 320–900 Hz bandpass wind noise with spatialized azimuth direction | Swirling pale-blue draft streamlines blowing across nest |
| Successful Wind Shielding | Muffled breeze flutter | Low-pass filter cut at 350 Hz, volume dropped by −12 dB | Azure windbreak barrier lights up; streamlines deflect smoothly around nest |
| Longitudinal Rocking | Soft felt brush | Velvet friction scrub (180–420 Hz noise), gain proportional to `|ω_pitch|` | Gentle gold ripple wave sweeps through vitelline fluid |
| Fingertip Heat Conduction | Harmonic warmth chime | Warm organ chord (330 Hz, 392 Hz, 494 Hz), gain proportional to `q_hand` | Contact point glows warm amber; thermal gradient visibly diffuses |
| Overpressure Warning | Tense calcium creak | High-frequency stress squeak (2.8–4.2 kHz FM scrape) | White hairline stress crack appears; shell pulses warning amber |
| Shell Fracture (Fail) | Sharp eggshell crack | Dual impact: 3.5 kHz brittle snap + 90 Hz acoustic thump | Shell shatters into flying shards; red screen vignette |
| Adhesion Warning | Muted fluid gurgle | Low wet wobble (120 Hz FM pulse) | Vitelline membrane darkens; "ROTATE EGG" warning banner flashes |
| Hatching Finale (Win) | Triumphant dawn chorus | Lush Andean flute arpeggio + baby hummingbird chirp (4.8 kHz) | Shell splits in golden light; iridescent chick stretches tiny wings |

## 13. Difficulty Progression & Tuning

### 5-Stage Developmental Incubation Matrix

| Stage | Developmental Milestone | Ambient Temp `T_amb` | Wind Speed `v_wind` | Draft Dynamics | Adhesion Risk | Target Incubation Goal |
|---|---|---|---|---|---|---|
| Stage 1 | Day 3: Blastoderm | 22.0 °C | 0.5 m/s | Steady northern breeze | Low (`τ_adh = 6.0 s`) | Master 1.2 Hz rocking; balance all 8 zones |
| Stage 2 | Day 6: Neural Tube | 20.0 °C | 1.8 m/s | 45° wind shifts every 12 s | Medium (`τ_adh = 5.0 s`)| Coordinate non-dominant windbreak shielding |
| Stage 3 | Day 9: Vascularization | 18.0 °C | 3.2 m/s | Gusty drafts with 2.5 s pulses | High (`τ_adh = 4.0 s`) | Target fingertip warmth to cold narrow apex |
| Stage 4 | Day 12: Torpor Defense | 14.0 °C | 4.5 m/s | Swirling vortex wind shifts | Very High (`τ_adh = 3.2 s`)| Continuous dual-axis rocking and rolling |
| Stage 5 | Day 14: Pipping Finale | 16.0 °C | 3.8 m/s | Erratic gust fronts + internal chick pips | Critical (`τ_adh = 3.0 s`)| Hold exact 37.5 ± 0.3 °C equilibrium to assist hatching |

### Adaptive Assistance Rules

- If `V_embryo` drops below 35 %: Maternal instinctive surge activates—hand conduction efficiency `k_conduction` increases by +40 % for 8 seconds, giving the player rapid thermal recovery.
- If player maintains `T_core` within 37.5 ± 0.2 °C for > 15 seconds: Embryo vitality earns a "Harmony Growth Surge" awarding +20 % bonus score.

### Named Tuning Constants

```csharp
public static class IncubatorTuningConstants
{
    // Prop Dimensions
    public const float PROLATE_SEMI_AXIS_A = 0.026f; // 26 mm
    public const float PROLATE_SEMI_AXIS_B = 0.016f; // 16 mm
    public const float PROP_MASS_KG = 0.028f;        // 28 g
    public const float MOMENT_INERTIA_PITCH = 5.219e-6f;
    public const float MOMENT_INERTIA_ROLL = 2.867e-6f;

    // Thermodynamic Constants
    public const float OPTIMAL_TEMP_C = 37.5f;
    public const float BASE_SHELL_HEAT_CAPACITY = 4.2f;   // J/C
    public const float THERMAL_DIFFUSION_K = 0.85f;      // W/C
    public const float HAND_CONDUCTION_K = 12.5f;        // W/(m^2*C)
    public const float HAND_TEMPERATURE_C = 38.2f;

    // Biomechanical & Adhesion Thresholds
    public const float ADHESION_ROTATION_THRESHOLD = 0.35f; // rad/s
    public const float MAX_SAFE_FORCE_N = 0.70f;
    public const float FRACTURE_FORCE_N = 0.85f;
    public const float MAX_SAFE_ZONE_DELTA_C = 4.0f;
}
```

## 14. Edge Cases & Failure Recovery

### Failure Modes & Recovery Strategies

- **EC-INC-01: Prop Rolls Off the Felt Nest Mat**:
  - *Symptom*: Over-vigorous rolling causes the physical egg to escape the 60 mm nest saucer and roll across the hard desk (`r_nest > 70 mm`).
  - *Detection*: Prop pose `p_P` distance from nest origin exceeds 65 mm.
  - *Recovery*: Virtual nest boundaries deploy a gentle magnetic recovery guide force (`F_restore = 0.25 N · normalize(-p_P)`). Holographic mother hummingbird appears, nudging the egg back to center with soft wing flutters. Incubation timers pause for up to 3.0 seconds to allow repositioning.
- **EC-INC-02: Severe Hand Tracking Occlusion During Cupped Shielding**:
  - *Symptom*: Player completely encloses the egg with both hands, blinding camera vision of the prop.
  - *Detection*: Optical prolate silhouette lost for > 90 ms while both hands are within 40 mm of prop.
  - *Recovery*: System switches to "Bimanual Cradling State": latches egg pose to the geometric center between both palms, keeps internal thermal simulation running, and rewards uniform bimanual body warming (+0.5 °C/s).
- **EC-INC-03: Thermal Sensor Inversion / Rapid Freeze Glitch**:
  - *Symptom*: Frame drop causes large `dt`, leading to numerical instability in finite-difference heat diffusion.
  - *Detection*: Any sector `dT / dt > 15.0 °C/s`.
  - *Recovery*: Heat equation clamps maximum temperature change per substep to `|ΔT_max| = 0.45 °C`. Substep resolution doubles automatically if thermal gradient exceeds 6.0 °C.
- **EC-INC-04: Accidental Prop Inversion (Blunt vs. Narrow Pole)**:
  - *Symptom*: Player turns the egg 180° so the narrow apex points opposite its original registration orientation.
  - *Detection*: Optical flow tracking detects major axis reversal.
  - *Recovery*: The 8-zone thermal grid is symmetric across the equatorial plane; the internal embryo model smoothly rotates 180° over 400 ms via slerp interpolation to align naturally with the physical egg's new orientation.

## 15. Implementation Architecture

### Class Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────────┐
│                        IncubationDirector                              │
│    (Stage Progression, Vitality Score, FSM, Audio-VFX Orchestration)   │
└───────────────▲────────────────────────▲────────────────────────▲──────┘
                │                        │                        │
┌───────────────┴────────┐  ┌────────────┴───────────┐  ┌─────────┴──────┐
│  PropTrackerIncubator  │  │  ThermalDiffusionGrid  │  │ EmbryoViability│
│  - 72 Hz Prolate Fit   │  │  - 8-Zone FE Heat Sim  │  │    Model       │
│  - Pitch & Roll Solver │  │  - Wind Convection     │  │ - Adhesion Sim │
│  - Woodgrain Spin Track│  │  - Maternal Conduction │  │ - Heartbeat BPM│
└────────────────────────┘  └────────────────────────┘  └────────────────┘
```

### Concrete C# Implementation Scripts

#### Script 1: `ThermalDiffusionGrid.cs`

```csharp
using System;
using UnityEngine;

public class ThermalDiffusionGrid : MonoBehaviour
{
    [Header("8-Zone Thermal State (deg C)")]
    public float[] zoneTemperatures = new float[8];

    [Header("Core & Ambient")]
    public float embryoCoreTemp = 37.5f;
    public float ambientAirTemp = 20.0f;
    public Vector3 draftWindVelocity = new Vector3(1.5f, 0f, 0f);
    public float handShieldFactor = 0.0f; // [0, 0.85]

    private const float SHELL_HEAT_CAP = 4.2f;
    private const float K_DIFF_BASE = 0.85f;
    private const float K_HAND = 12.5f;
    private const float T_HAND = 38.2f;

    // Topology: 4 narrow zones (0..3) adjacent to 4 blunt zones (4..7)
    private static readonly int[][] Neighbors = new int[8][]
    {
        new int[] { 1, 3, 4 }, // Zone 0 (AN)
        new int[] { 0, 2, 5 }, // Zone 1 (PN)
        new int[] { 1, 3, 6 }, // Zone 2 (PL)
        new int[] { 0, 2, 7 }, // Zone 3 (PR)
        new int[] { 5, 7, 0 }, // Zone 4 (AB)
        new int[] { 4, 6, 1 }, // Zone 5 (PB)
        new int[] { 5, 7, 2 }, // Zone 6 (AL)
        new int[] { 4, 6, 3 }  // Zone 7 (AR)
    };

    public void InitializeGrid()
    {
        for (int i = 0; i < 8; i++)
        {
            zoneTemperatures[i] = 37.5f;
        }
        embryoCoreTemp = 37.5f;
    }

    public void StepThermodynamics(float[] fingerContactAreas, float vitellineFlowSpeed, float dt)
    {
        float[] dTemp = new float[8];
        float kDiffActive = K_DIFF_BASE * (1.0f + 2.5f * Mathf.Clamp01(vitellineFlowSpeed));

        for (int i = 0; i < 8; i++)
        {
            // 1. Hand conduction
            float qHand = K_HAND * fingerContactAreas[i] * (T_HAND - zoneTemperatures[i]);

            // 2. Inter-sector diffusion
            float qDiff = 0f;
            for (int n = 0; n < Neighbors[i].Length; n++)
            {
                int neighborIdx = Neighbors[i][n];
                qDiff += kDiffActive * (zoneTemperatures[neighborIdx] - zoneTemperatures[i]);
            }

            // 3. Draft wind chill
            float windSpeed = draftWindVelocity.magnitude;
            float hWind = 8.0f + 14.0f * Mathf.Sqrt(windSpeed);
            float exposure = (1.0f - handShieldFactor);
            float qDraft = hWind * 0.0035f * exposure * Mathf.Max(0.1f, (zoneTemperatures[i] - ambientAirTemp));

            float netPower = qHand + qDiff - qDraft;
            dTemp[i] = (netPower / SHELL_HEAT_CAP) * dt;
        }

        // Apply updates with safety clamp
        float sumTemp = 0f;
        for (int i = 0; i < 8; i++)
        {
            zoneTemperatures[i] = Mathf.Clamp(zoneTemperatures[i] + dTemp[i], 12.0f, 44.0f);
            sumTemp += zoneTemperatures[i];
        }

        // Core temperature lags shell mean
        float meanShellTemp = sumTemp / 8.0f;
        embryoCoreTemp += (meanShellTemp - embryoCoreTemp) * (dt / 8.5f);
    }
}
```

#### Script 2: `EmbryoViabilityModel.cs`

```csharp
using System;
using UnityEngine;

public class EmbryoViabilityModel : MonoBehaviour
{
    [Header("Embryo Health")]
    public float vitalityPercent = 100.0f; // [0, 100]
    public float heartRateBPM = 720.0f;
    public float adhesionProgress = 0.0f;  // [0, 1]
    public float crackDamage = 0.0f;       // [0, 1]

    public event Action OnEmbryoPerished;
    public event Action OnShellCracked;

    public void UpdateEmbryoState(float coreTemp, float rollSpeed, float pitchSpeed, float maxNormalForce, float maxZoneTempDelta, float dt)
    {
        // 1. Membrane Adhesion Integration
        bool isMotionless = (Mathf.Abs(rollSpeed) < 0.35f) && (Mathf.Abs(pitchSpeed) < 0.25f);
        if (isMotionless)
        {
            adhesionProgress = Mathf.Clamp01(adhesionProgress + 0.22f * dt);
        }
        else
        {
            adhesionProgress = Mathf.Clamp01(adhesionProgress - 0.45f * dt);
        }

        // 2. Heart Rate Modulation vs. Temperature
        float tempDelta = coreTemp - 37.5f;
        heartRateBPM = Mathf.Clamp(720.0f + tempDelta * 65.0f, 180.0f, 980.0f);

        // 3. Vitality Accrual / Drain
        float thermalScore = 1.0f - Mathf.Abs(tempDelta) / 1.5f;
        float dVitality = 1.2f * thermalScore * dt;
        if (adhesionProgress > 0.70f)
        {
            dVitality -= 4.5f * adhesionProgress * dt;
        }
        if (coreTemp < 35.5f || coreTemp > 39.0f)
        {
            dVitality -= 3.0f * dt;
        }
        vitalityPercent = Mathf.Clamp(vitalityPercent + dVitality, 0f, 100f);

        // 4. Shell Structural Stress
        float forceStress = Mathf.Max(0f, maxNormalForce - 0.70f) * 4.0f;
        float thermalStress = Mathf.Max(0f, maxZoneTempDelta - 4.0f) * 0.25f;
        float totalStressRate = forceStress + thermalStress;
        crackDamage = Mathf.Clamp01(crackDamage + totalStressRate * dt);

        // Evaluate Failure Events
        if (vitalityPercent <= 0f)
        {
            OnEmbryoPerished?.Invoke();
        }
        if (crackDamage >= 1.0f)
        {
            OnShellCracked?.Invoke();
        }
    }
}
```

## 16. Acceptance Test Checklist

| Test ID | Test Description & Inputs | Target Verification Procedure | Quantitative Pass Threshold |
|---|---|---|---|
| `TC-INC-01` | Prolate Spheroid Fitting | Present 52.0 × 32.0 mm prolate egg at 0.35–0.50 m distance | Ellipsoid fit converges < 1.0 s; major axis length `52.0 ± 1.0 mm`, diameter `32.0 ± 0.8 mm` |
| `TC-INC-02` | Longitudinal Rocking Frequency | Rhythmic physical rocking on felt mat at 1.5 Hz | Measured rocking cadence `1.5 ± 0.15 Hz`; passband filter retains 100 % signal |
| `TC-INC-03` | Adhesion Reset on Roll | Leave egg static for 3.0 s (`A_adh → 0.66`), then impart axial roll `ω_roll = 1.2 rad/s` | Adhesion accumulator resets below 0.10 in < 1.5 s of continuous rolling |
| `TC-INC-04` | Non-Dominant Wind Shielding | Direct 3.0 m/s virtual draft; place cupped `L_palm` 50 mm upwind | Measured `ShieldFactor ≥ 0.80`; draft convective heat loss drops by ≥ 80 % |
| `TC-INC-05` | Fingertip Heat Conduction Rate | Apply single fingertip contact (`F = 0.5 N`) to cold zone (`T = 32.0 °C`) | Local sector temperature warms at rate `1.8 ± 0.3 °C/s`; reaches 37.0 °C in < 3.0 s |
| `TC-INC-06` | Inter-Sector Thermal Gradient Stress | Induce artificial 5.5 °C gradient between Zone 0 and Zone 1 | Stress warning sound triggers immediately; `CrackDamage` accumulates at `≥ 0.35 s⁻¹` |
| `TC-INC-07` | Structural Overpressure Fracture | Apply normal pinch force `F = 0.95 N` for 0.4 s | Catastrophic fracture triggered; `FAIL_CRACK` state entered; audio snap plays |
| `TC-INC-08` | Heart Rate Temperature Tracking | Modulate core temperature from 37.5 °C down to 34.0 °C | Embryonic heart rate drops proportionally from 720 bpm down to `492 ± 20 bpm` |
| `TC-INC-09` | Felt Mat Compliance Sink | Measure resting egg elevation on felt mat under gravity | Physical sink `Δy = 1.2 ± 0.3 mm`; no bouncing or jitter in resting pose |
| `TC-INC-10` | 5-Stage Complete Soak Test | Run complete 5-stage incubation session (4.5 min) at 90 Hz render | Zero frame drops below 85 Hz; GC pause < 4.0 ms; vitality accrues smoothly |

---
