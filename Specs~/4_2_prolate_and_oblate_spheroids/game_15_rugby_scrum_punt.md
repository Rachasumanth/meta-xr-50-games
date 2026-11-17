# Rugby Scrum Punt

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Rugby Scrum Punt |
| Slug | `rugby_scrum_punt` |
| Physical prop | Prolate mini rugby / football prop; major longitudinal polar semi-axis `a = 45.0 mm` (90.0 mm tip-to-tip length), equatorial transverse semi-axes `b = c = 26.0 mm` (52.0 mm equatorial diameter); mass 70.0 g; high-density microcellular elastomer foam with pebble-grain leatherette texture and raised tactile seam laces |
| Sub-shape & Family | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate and Oblate Spheroids (prolate, handheld axial twirling, elevation aiming, and forward flick release) |
| Scale class | Tabletop tee to stadium ballistic scale; physical flick launch stroke 120 mm; virtual championship stadium 70 meters long; ballistic launch gain 40.0×; goalpost crossbar height 3.0 m, upright width 5.6 m |
| Source concept quote | "Championship stadium spiral kick. Spinning the ball along its prolate longitudinal axis charges gyroscopic stability; flicking forward launches a soaring spiral kick through crosswind gusts between stadium uprights." |
| Physical input | Non-dominant hand supports or steadies the rubber kicking tee on the table/lap surface; dominant thumb, index, and middle fingertips twirl the prolate ball along its longitudinal axis `x̂_P` to impart high-speed gyroscopic spin (`ω_x ∈ [40, 130] rad/s`), adjust pitch elevation angle (35°–48°), and snap forward in a ballistic flick release |
| Session length | 5 championship penalty kicks / conversions (waves), 35–50 s per kick; total session duration ≈4.0 minutes |
| Lose condition | Scoring 0 conversions across 5 championship attempts, failing to reach the crossbar (`y_goal < 3.0 m`), wide ball drift beyond uprights (`|z_goal| > 2.8 m`), or ball charge-down blocked by rushing defensive scrum forwards |

## 1. Fantasy & Core Loop

Under the blinding floodlights of an 80,000-seat championship rugby arena, eighty minutes of grueling battle come down to a single clutch moment: the dying seconds of the World Rugby Championship Final. You are the fly-half standing poised before the kicking tee. Rain mists through the glare of the lights, and stadium banners snap violently in swirling crosswinds gusting down from the upper grandstands. In the distance, fifty meters downfield, the towering yellow goalpost uprights loom against the roaring crowd.

Between your fingers sits the official match ball—a perfectly weighted, 70 g prolate spheroid prop. Its aerodynamic prolate shape is engineered for the legendary "spiral punt," an athletic kick where the ball rotates bullet-like along its sharp longitudinal axis. If kicked without spin, the prolate ball tumbles end-over-end, fluttering wildly in the crosswind and falling dead short of the crossbar. But when spun with ferocious axial rpm, the ball's gyroscopic angular momentum (`L_spin = I_x · ω_x`) locks its longitudinal apex like a gyroscope, cutting cleanly through 10 m/s crosswind gusts, generating Magnus lift, and sailing true between the stadium uprights!

### 30–60s Active Kicking Cycle

1. **Tee Placement & Wind Vector Assessment (0.0–3.0 s)**: Player places the prolate ball onto the virtual kicking tee on the table (`origin_V`). Headset HUD displays wind vane direction and gust velocity (e.g. 8.5 m/s blowing 65° left-to-right) and goalpost distance (32 to 58 meters).
2. **Gyroscopic Axial Spin Charge (3.0–8.0 s)**: Using dominant thumb, index, and middle fingertips, the player twirls the ball along its prolate longitudinal axis `x̂_P`. Rhythmic digital flicking spins the ball up from 0 to 1200 rpm (`ω_x ∈ [40, 125] rad/s`). As spin surpasses 60 rad/s, a shimmering golden spiral wake ignites along the leather seams, indicating gyroscopic rigidity.
3. **Elevation Pitch & Wind Lead Aiming (8.0–12.0 s)**: Holding stored spin, the player tilts the nose of the ball up to the optimal ballistic launch angle (`θ_pitch = 42°`) and yaws the ball heading 8° into the windward flank to compensate for aerodynamic wind drift.
4. **Scrum Rush Countdown & Charge-Down Warning (12.0–14.0 s)**: The referee blows the whistle. A holographic defensive scrum wall charges forward from 10 meters out. Player must execute the kick before the 2.2-second charge-down timer expires!
5. **Ballistic Flick Release (14.0–15.0 s)**: A crisp, explosive forward finger snap / palm flick launches the prolate ball (`v_physical ∈ [0.40, 0.85] m/s`). Ballistic gain (40.0×) scales physical launch speed into virtual stadium exit velocity (`v_virtual ∈ [16.0, 34.0] m/s`).
6. **Spiral Flight & Goalpost Resolution (15.0–19.0 s)**: Camera tracks behind the soaring rugby ball as it arcs gracefully through the rainy stadium air. Gyroscopic stability combats turbulence; the ball pierces the crosswind and sails high above the crossbar right between the uprights!
7. **Stadium Roar & Conversion Tally (19.0–24.0 s)**: Two assistant referees raise their flags; 80,000 spectators erupt into a deafening stadium roar; score flashes (+3 or +5 points), tee resets for the next championship kick.

### 5-Wave Championship Kick Progression

- **Wave 1 (25m Center Conversion)**: Baseline test. Distance 25.0 m, zero wind. Basic axial spin charging (target `ω_x ≥ 50 rad/s`) and 40° launch pitch.
- **Wave 2 (38m Left Touchline Penalty)**: Extreme sharp angle from left touchline. Distance 38.0 m, 4.5 m/s crosswind blowing right. Requires wind lead aiming and steady 75 rad/s spin.
- **Wave 3 (45m Headwind Drive)**: Stiff 9.0 m/s headwind pushing downward. Demands a piercing low-trajectory spiral (`θ_pitch = 35°`, `ω_x ≥ 95 rad/s`) to prevent stalling.
- **Wave 4 (52m Rushing Scrum Charge-Down)**: Long distance 52.0 m, shifting swirling gusts. Defensive flanker charges down at 5.5 m/s; kick must be released in under 1.8 s with exit velocity `v_virtual ≥ 28.0 m/s`.
- **Wave 5 (60m Championship Winning Drop-Punt)**: Final kick from 60.0 meters in driving rain with 12.0 m/s turbulent crosswinds. Requires maximum gyroscopic spin (`ω_x ≥ 115 rad/s`, 1100 rpm) and pinpoint trajectory alignment to split the uprights for the World Championship trophy.

## 2. Platform Baseline

- **Hardware**: Standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No physical controllers; the only tangible prop is the registered prolate rugby prop.
- **Joint Naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc, mcp, pip, dip, tip (thumb: cmc, mcp, ip, tip). Example: `R_thumb_tip`, `R_index_tip`, `R_middle_tip`, `L_palm`.
- **Prop Registration**: At session start, the player holds or places the mini rugby prop on the table; vision pipeline segments the 90.0 × 52.0 mm prolate silhouette, computes 6-DoF pose `P` at 72 Hz, and continuously tracks longitudinal axis `x̂_P` and axial spin `ω_x`.
- **Coordinate Frames**:
  - `W`: Room world frame (Y up, cm/meters).
  - `T`: Support surface frame (detected table or lap tray; origin at kicking tee base, `+ŷ_T` surface normal pointing up, `+x̂_T` field forward pointing toward goalposts, `+ẑ_T = x̂_T × ŷ_T` pointing to player's right).
  - `P`: Prop local frame (origin at geometric center of prolate spheroid; `+x̂_P` major polar axis pointing from blunt base to sharp tip, `+ŷ_P` and `+ẑ_P` equatorial transverse axes passing through leather seam stitches).
  - `V`: Virtual play frame (stadium coordinate system, anchored to `T` at the kicking tee).
- **Contact Detection**: Joint contact detected when signed distance from hand joint proxy spheres to the prop's prolate collider is < 4.5 mm; release hysteresis 7.5 mm; debounce threshold 25 ms.
- **Physics Engine**: Fixed 90 Hz timestep, max 2 sub-steps (11.1 ms / 5.55 ms); 6-DoF aerodynamic projectile flight incorporating gyroscopic angular momentum, precession, and Magnus lift forces.
- **Rendering & Compositing**: Color passthrough base layer; volumetric stadium floodlights, dynamic rain streaks, and stadium crowd mesh; holographic spiral wake ribbon; hand skeletal mesh depth occlusion.
- **Feedback**: Multi-channel spatial audio (explosive boot punt boom, spinning spiral air whistle, stadium roar reverberation, goalpost metal ping) and visual VFX (golden spiral aerodynamic trails, rain spray parting, fireworks).

### 2.1 Game-Specific Overrides

- **Prop Frame `P` & Geometry**: Prolate spheroid of revolution with major longitudinal polar semi-axis `a = 45.0 mm` (90.0 mm total tip-to-tip length) and equatorial transverse semi-axes `b = c = 26.0 mm` (52.0 mm equatorial diameter). Prolate elongation ratio `a / b = 45.0 / 26.0 = 1.7308`.
- **Kinematic Rolling & Gyroscopic Model**:
  - When spinning freely on its longitudinal axis `x̂_P`, the ball's polar moment of inertia is `I_x = (2/5) · m · b² = 1.893 × 10⁻⁵ kg·m²`.
  - When tumbling end-over-end (transverse pitch/yaw `ŷ_P`, `ẑ_P`), the moment of inertia is significantly larger: `I_y = I_z = (1/5) · m · (a² + b²) = 3.781 × 10⁻⁵ kg·m²` (almost exactly 2.0× larger).
  - Gyroscopic Angular Momentum:
    `L_spin = I_x · ω_x · x̂_P`.
  - Aerodynamic overturning torque `τ_aero` induced by angle of attack `α_aero` causes gyroscopic precession rather than tumbling:
    `Ω_prec = τ_aero / (I_x · ω_x)`.
    If `ω_x` exceeds critical spin threshold `ω_crit = 55.0 rad/s` (525 rpm), the gyroscopic stability factor `S_g > 1.0`, locking the prolate nose along the velocity flight path!
- **Virtual Frame `V` Anchoring & Ballistic Gain**:
  - `origin_V` is anchored to `T` at the physical kicking tee.
  - Physical flick stroke covers `x_T ∈ [0, 120] mm` with physical launch velocity `v_rel ∈ [0.35, 0.85] m/s`.
  - Virtual exit velocity applies **40.0× Ballistic Gain**:
    `v_0_V = 40.0 · v_rel_physical` (yielding virtual exit speeds of `14.0` to `34.0 m/s`, exactly matching international rugby punt ballistics).
  - Virtual stadium stretches 70.0 meters downfield (`x_V ∈ [0, 70] m`).
- **Specific Deviations**:
  - While physical rugby kicks use feet and heavy boots, this MR micro-mechanic translates the biomechanics of digital finger snapping and axial flicking into true full-scale stadium punt trajectory physics.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Play Posture | Seated at desk or standing before table with kicking tee patch centered | Allows stable wrist elevation and comfortable digital twirling without shoulder hunching |
| Physical Kicking Patch | 200 mm wide × 150 mm deep flat table area with 40 mm rubber tee ring | Keeps prolate ball securely seated during wind aiming; prevents ball rolling away |
| Flick Launch Stroke | 80–120 mm forward digital flick distance | Natural wrist flexion and finger snap range; avoids hitting desk or knocking table items |
| Axial Twirl Cadence | 2.0–4.0 Hz digital flick strokes (generating 40–125 rad/s spin) | Achieved via agile thumb-to-index/middle rubbing; low joint fatigue |
| Peak Launch Force | 0.8–2.0 N forward flick force | Well within finger extensor capabilities; high-density foam absorbs impact |
| Elevation Angle Range | `θ_pitch ∈ [30°, 55°]` (optimal sweet spot 38°–45°) | Natural wrist extension posture while resting forearm on desk edge |
| Rest Intervals | 15 s between championship kicks | Hand relaxation, knuckle flexing, and tactical wind reading |
| Viewing Direction | 30° downward gaze toward tee, transitioning to 10° upward gaze downfield | Optimal for tracking ball flight through stadium floodlights without neck strain |

### Fatigue Mitigations
1. **Lightweight Foam Prop (70 g)**: Inertial mass is perfectly balanced—heavy enough for satisfying gyroscopic feel, light enough to prevent digital strain during repeated twirls.
2. **Digital Twirl Multiplier**: Optical flow tracking detects micro-rotations and applies a 1.5× algorithmic spin-charge acceleration, enabling players to reach 1000 rpm with only 3–4 brisk finger rubs.
3. **Automatic Tee Reset**: Virtual rubber tee auto-levels the ball when placed, eliminating tedious physical balancing.

## 4. Prop Geometry & Registration

```
                           +x̂_P (Longitudinal Polar Axis, a = 45.0 mm)
                                     ▲
                                   .-│-.
                                 .'  │  '.   Sharp Nose Tip (R_tip = 15.02 mm)
                                /    │    \
                               |     │     |
              ◄────────────────┼─────┼─────┼────────────────►
              -b = 26.0 mm     |   (●●●)   |   +b = 26.0 mm (Equator)
              (Transverse ŷ/ẑ) \  Laces    /   Equatorial Ø = 52.0 mm
                                '.   │   .'    R_meridian = 77.88 mm
                                  '-.│.-'
                                     ▼       Blunt Rear Tail
```

### Prop Dimensions & Mass Properties

- **Major Longitudinal Polar Semi-Axis**: `a = 45.0 mm` (length = 90.0 mm tip-to-tip).
- **Equatorial Transverse Semi-Axes**: `b = c = 26.0 mm` (diameter = 52.0 mm).
- **Aspect Ratio**: `γ = a / b = 45.0 / 26.0 ≈ 1.7308`.
- **Prolate Eccentricity**: `e = √(1 - b²/a²) = √(1 - 26²/45²) = √(1 - 0.3338) = √0.6662 ≈ 0.8162`.
- **Prop Mass**: `m = 70.0 g` (0.070 kg).
- **Volume**: `V = (4/3) · π · a · b · c = (4/3) · π · 45 · 26 · 26 = 127,423 mm³ ≈ 127.42 cm³`.
- **Mean Density**: `ρ = 70.0 g / 127.42 cm³ ≈ 0.549 g/cm³` (closed-cell polyurethane microcellular foam).
- **Moments of Inertia**:
  - Spin Axis (Longitudinal Polar `x̂_P`):
    `I_x = (2/5) · m · b² = 0.4 · 0.070 kg · (0.026 m)² = 1.893 × 10⁻⁵ kg·m²`.
  - Transverse Pitch/Yaw Axes (`ŷ_P`, `ẑ_P`):
    `I_y = I_z = (1/5) · m · (a² + b²) = 0.2 · 0.070 kg · (0.045² + 0.026²) m² = 3.781 × 10⁻⁵ kg·m²`.
  - Inertia Ratio: `I_t / I_x = 3.781 / 1.893 = 1.997` (transverse inertia is double the spin inertia).

### Surface-Feature Atlas

| Feature ID | Coordinate Bounds in `P` | Physical Texture / Marker | Functional Purpose |
|---|---|---|---|
| `F1: NOSE_APEX` | `x ∈ [+35, +45] mm, r ≤ 12 mm` | Molded rubber tip point | Longitudinal heading indicator; aerodynamic stagnation point |
| `F2: REAR_TAIL` | `x ∈ [−45, −35] mm, r ≤ 12 mm` | Molded inflation valve indent | Rear flick contact notch for thumb pad |
| `F3: LEATHER_LACES`| `x ∈ [−20, +20] mm, y ≥ +22 mm` | 8 raised tactile urethane cross-stitches| High-friction digital twirl grip; optical yaw feature |
| `F4: SEAM_CHANNELS`| 4 longitudinal seams at 90° azimuths| 1.5 mm recessed grooves | Aerodynamic trip-wire inducing turbulent boundary layer |
| `F5: EQUATOR_FLANK`| `x ∈ [−15, +15] mm, r = 26 mm` | Micro-dimpled pebble leatherette grain | Palm support contact and elevation grip |

### Collider Inventory

| Collider ID | Type | Dimensions | Coordinate Frame | Role |
|---|---|---|---|---|
| `COL_RUGBY_CORE` | Prolate Spheroid | `a = 45.0 mm`, `b = c = 26.0 mm` | Prop Frame `P` | Primary hand contact, tee seat proxy, occluder depth mask |
| `COL_LACE_STRIP` | Box | `40 × 10 × 4 mm` | Offset `(0, +24, 0)` in `P` | Raised grip zone for axial spin charging |
| `COL_REAR_PUSH` | Sphere | `r = 14.0 mm` | Offset `(−35, 0, 0)` in `P` | Launch flick impulse contact detector |
| `COL_KICKING_TEE`| Cone / Ring | Base `r = 30 mm`, top `r = 18 mm`, `h = 22 mm`| Table Frame `T` | Table resting cradle with compliant rubber spring |
| `COL_GOALPOST_BAR`| Cylinder | `r = 60 mm`, length 5600 mm | Virtual Stadium `V` | Crossbar collision (3.0 m height downfield) |
| `COL_GOALPOST_L` | Cylinder | `r = 60 mm`, height 12000 mm | Virtual Stadium `V` | Left upright post (at `z = −2.8 m`) |
| `COL_GOALPOST_R` | Cylinder | `r = 60 mm`, height 12000 mm | Virtual Stadium `V` | Right upright post (at `z = +2.8 m`) |

### Optical Registration & Pose Fitting

1. **Table Tee Base Lock**: System detects the kicking tee ring on the table surface `T` with normal residual < 0.6 mm.
2. **Prolate Silhouette Extraction**: Vision extracts the elongated 90 × 52 mm prolate contour at 72 Hz. Major axis line fit identifies longitudinal vector `x̂_P`.
3. **Lace Optical Flow Tracking**: High-contrast white lace stitching on the dark leatherette hull allows tracking of high-speed axial rotation `ω_x` up to **150 rad/s (1430 rpm)** with zero aliasing.
4. **Pose Optimization**: Levenberg-Marquardt solver estimates 6-DoF pose `P`. Acceptance criteria: RMS reprojection error < 0.58 mm, longitudinal axis alignment variance < 1.4°.

## 5. Hand Tracking Input Map

| ID | Gesture / Input | Active Joints | Surface Region | Semantic Gameplay Output |
|---|---|---|---|---|
| `IN_TEE_SEAT` | Two-Handed Tee Mount | `L_palm`, `R_fingers` | `COL_RUGBY_CORE` onto Tee | Places ball on tee; arms system for spin charging |
| `IN_SPIN_TWIRL` | Digital Axial Snap / Roll | `R_thumb_tip`, `R_index_pad`, `R_middle_pad` | Leather Laces (`F3`) + Flank | Spins ball along `x̂_P`; charges gyroscopic stability `ω_x` |
| `IN_ELEVATION_AIM` | Wrist Pitch Tilting | `R_thumb_tip`, `R_index_tip` pinch | Equator Flank (`F5`) | Adjusts launch pitch angle `θ_pitch ∈ [30°, 55°]` |
| `IN_WIND_LEAD` | Forearm Yaw Steering | `R_wrist`, `R_palm` | Ball longitudinal heading `x̂_P` | Aims lateral yaw heading `ψ_yaw` into crosswind vector |
| `IN_FLICK_RELEASE` | Explosive Forward Finger Snap | `R_index_tip`, `R_thumb_tip` flick | Rear Tail (`F2`) | Triggers ballistic punt launch; captures `v_rel_physical` and exit angle |
| `IN_TEE_STEADY` | Non-Dominant Base Hold | `L_index_tip`, `L_thumb_tip` on tee | `COL_KICKING_TEE` base | Stabilizes tee during high-cadence spin charging |
| `IN_ABORT_LIFT` | Vertical Pick-Up | All right hand joints lift ball > 50 mm | Off Tee > 150 ms | Cancels current kick countdown; resets charge |

## 6. Input Kinematics & Signal Processing

```
72 Hz Hand Tracking ────► OneEuro Filter ────► Optical Flow Spin Meter ──► Gyroscopic Momentum
[R_index_tip, thumb]      (fc=2.2, β=0.04)      ω_x = dψ_laces/dt           L = I_x · ω_x
                                                                                 │
72 Hz Prop Pose P ──────► Release Velocity ──► Ballistic Scaler (40x) ───────────┴──► 6-DoF Flight
(p_P, q_P, θ_pitch)       Linear Regressor      v_0 = 40.0 · v_phys                    Trajectory
```

### Gyroscopic Stability & Precession Dynamics

The spinning prolate ball behaves as a rigid symmetric top:

1. **Spin Velocity Accumulation (`ω_x`)**:
   Measured from lace angular displacement around longitudinal axis `x̂_P`:
   `ω_x(t) = dψ_roll / dt` [rad/s].
   Digital flick inputs accumulate spin:
   `ω_x(t + dt) = ω_x(t) · exp(−c_spin_friction · dt) + Δω_flick`.
   Maximum spin is clamped to **130.0 rad/s (1240 rpm)**.
2. **Gyroscopic Stability Factor (`S_g`)**:
   Under aerodynamic overturning moment `M_alpha`:
   `S_g = (I_x² · ω_x²) / (4 · I_t · M_alpha · v_virtual²)`
   Where:
   - Polar inertia `I_x = 1.893 × 10⁻⁵ kg·m²`.
   - Transverse inertia `I_t = 3.781 × 10⁻⁵ kg·m²`.
   - Overturning moment coefficient: `M_alpha = 0.5 · ρ_air · S_ref · L_ref · C_mα · α_aero`.
   - **Stability Threshold**:
     - If `S_g ≥ 1.0` (Stable Spiral): The ball maintains bullet-like gyroscopic stability; angle of attack remains tight (`|α| < 4°`), aerodynamic drag stays minimal (`C_D ≈ 0.12`), and crosswind drift is suppressed by 75 %.
     - If `S_g < 1.0` (Unstable End-Over-End Tumble): The ball begins wild gyroscopic tumbling; drag triples (`C_D → 0.38`), velocity bleeds rapidly, and wind drift blows the ball off course.

### Ballistic Flight Integration & Magnus Aerodynamics

Upon release, the virtual rugby ball leaves the tee at `p_0 = (0, 0, 0)_V` with virtual exit velocity:
`v_0_V = 40.0 · [v_rel_x · cos(θ_pitch), v_rel_x · sin(θ_pitch), v_rel_z]`.

During free flight downfield (`t > 0`):

1. **Relative Wind Velocity (`v_rel_wind`)**:
   `v_rel_wind = v_ball - v_wind_field(p_ball)`.
2. **Aerodynamic Drag**:
   `F_drag = −0.5 · ρ_air · ||v_rel_wind||² · S_ref · C_D(S_g) · normalize(v_rel_wind)`.
3. **Magnus Lift Force**:
   Spinning prolate balls generate transverse Magnus force when spin vector `ω` is non-parallel to relative air flow:
   `F_magnus = S_magnus · (ω × v_rel_wind)`
   with Magnus lift coefficient `S_magnus = 0.0018 kg`.
4. **Trajectory Integration**:
   `d(v_ball) / dt = (F_drag + F_magnus) / m_virtual + g_gravity`.
   `d(p_ball) / dt = v_ball`.

## 7. Virtual World Layout & Coordinate Mapping

```
                                VIRTUAL STADIUM FRAME (V)
          ◄────────────────── Field Length 70.0 Meters ──────────────────►
┌────────────────────────────────────────┬─────────────────────────────────┐
│ TEE & SCRUM ZONE (0 to 15 Meters)      │ DOWNFIELD & GOALPOSTS (25-65 m) │
│                                        │                                 │
│   [Physical Table Tee]                 │   Left Upright (z = -2.8 m)  │  │
│          (●)                           │   Right Upright (z = +2.8 m) │  │
│   Origin (0, 0, 0)                     │   Crossbar Height y = 3.0 m ─── │
│                                        │                                 │
│   Rushing Defensive Scrum (10 m)       │   Swirling Upper Grandstand     │
│   ◄─── [===###===] ───►                │   Crosswind Gusts (v_wind)      │
└────────────────────────────────────────┴─────────────────────────────────┘
▲                                        ▲
x_V = 0 m (Kicking Tee)                  x_V = 25 to 65 m (Target Goalposts)
```

### Coordinate Transforms

- **`P_to_T`**: `p_T = R_P_to_T · p_P + p_P_in_T`.
- **`T_to_V (Tee Launch Anchor)`**: Co-located with physical tee: `origin_V = origin_T`.
- **Field Scale Transformation**:
  - Longitudinal axis: `1 mm physical flick = 40 mm virtual ball displacement at launch`.
  - Stadium Field Length: `70.0 virtual meters`.
  - Goalpost Placement: Adjustable along downfield axis `x_V ∈ [25.0, 65.0] m` depending on wave difficulty.

### Virtual Entity Inventory

| Entity Name | Frame | Field Position `(x, y, z)` [m] | Dimensions | Visual Description |
|---|---|---|---|---|
| Kicking Tee Base | `T` | `(0, 0, 0)` | Base Ø 60 mm, height 22 mm | Red rubber four-prong kicking tee |
| Virtual Match Ball | `V` | Dynamic `p_V(t)` | Length 280 mm, Ø 160 mm (scale) | Top-tier match ball with team heraldry and wet leather sheen |
| Golden Spiral Wake Ribbon| `P` | Trailing from ball tip | 1.8 m long helical ribbon | Glowing golden vortex spiral reflecting axial rpm intensity |
| Stadium Goalposts | `V` | `x = Distance, y = 0, z = 0` | Width 5.6 m, bar 3.0 m, posts 12.0 m| Gleaming yellow padded stadium posts with red target flags |
| Rushing Scrum Wall | `V` | Spawns at `x = 10.0 m` | Width 4.0 m, height 1.9 m | Three charging defensive forwards with outstretched arms |
| Stadium Wind Vane HUD | `V` | Floats 1.2 m above tee | Radius 250 mm compass ring | 3D dynamic arrow showing wind speed and gust turbulence |
| Stadium Grandstands & Rain| `V` | Surrounding arena | 80,000 spectator coliseum | Volumetric rain streaks, stadium floodlights, and cheering crowd |

## 8. Gameplay Systems & Rules

### Championship Scoring & Match Format

The match consists of **5 High-Stakes Championship Kicks** (Conversions and Penalty Goals).

1. **Conversion Scoring Formula**:
   At the point where the ball crosses the goalpost plane (`x_ball_V ≥ x_goalposts`):
   - **Crossbar Clearance Gate**:
     `y_ball_V ≥ 3.0 m` (must pass *above* the horizontal crossbar).
   - **Upright Boundary Gate**:
     `|z_ball_V| ≤ 2.8 m` (must pass *between* the vertical upright posts).
   - **Points Awarded on Success**:
     - *Clean Center Split (`|z_ball| ≤ 0.8 m`)*: **500 Points** + "Bullseye Conversion" bonus (+250 pts).
     - *Standard Split (`0.8 < |z_ball| ≤ 2.4 m`)*: **350 Points**.
     - *Inside Edge Post Tap (`2.4 < |z_ball| ≤ 2.8 m`)*: **200 Points** (drama bonus).
     - *Miss / Wide / Under Bar*: **0 Points**.
   - **Distance Multiplier**:
     `Score = Points · (1.0 + (Distance_Meters - 25.0) / 40.0) · (1.0 + ω_x / 100.0)`.

2. **Charge-Down Rule**:
   Once the referee whistle sounds, a holographic defensive forward charges from 10 meters out at 5.5 m/s. The kick must be executed within **2.2 seconds**; otherwise, the defender's outstretched arm intercepts the ball at release, resulting in a "CHARGE-DOWN BLOCKED" fault (0 points).

3. **Loss Conditions**:
   - Zero successful kicks across 5 attempts.
   - 2 Charge-down blocks.

4. **Victory Criteria**:
   Aggregate score across 5 championship kicks ≥ **1,800 Points** (Wins the World Championship Trophy).

## 9. Game State Machine

```
┌─────────┐      ┌──────────────┐      ┌────────────┐      ┌─────────────┐
│  BOOT   ├─────►│  REGISTER_P  ├─────►│ TEE_CALIB  ├─────►│ KICK_INTRO  │
└─────────┘      └──────────────┘      └────────────┘      └──────┬──────┘
                                                                  │
┌──────────────┐      ┌──────────────┐      ┌───────────┐         │
│ MATCH_VICTORY│◄─────┤ SCORE_TALLY  │◄─────┤ BALLISTIC_├─────────┘
└──────────────┘      └──────┬───────┘      │ FLIGHT    │
                             │              └─────┬─────┘
                             ▼                    │
                      ┌──────────────┐            ▼
                      │ NEXT_KICK    │     ┌─────────────┐
                      └──────────────┘     │ CHARGE_BLOCK│
                                           └─────────────┘
```

### State Machine Transition Table

| State Name | Entry Conditions | Continuous Operations | Exit Guard / Transition Condition | Next State |
|---|---|---|---|---|
| `BOOT` | Application launch | Load stadium environment, rain shaders, initialize 72 Hz ball tracker | Tracking pipelines ready | `REGISTER_P` |
| `REGISTER_P` | Prop presented on tee | Fit 90 × 52 mm prolate spheroid; lock lace optical flow | Prop registered (`RMS < 0.58 mm`) | `TEE_CALIB` |
| `TEE_CALIB` | Prop registered | Confirm table plane `T` and tee center; render 3D wind vane HUD | Ball settled upright on tee | `KICK_INTRO` |
| `KICK_INTRO` | Kick start | Display kick number, distance, and wind vector (e.g. "Kick 3: 45m Headwind") | Intro sequence timer `t ≥ 2.5 s` | `AIM_AND_CHARGE` |
| `AIM_AND_CHARGE`| Whistle blown | Sample axial spin twirling `ω_x`, track elevation angle `θ_pitch`, advance charge timer | Release detected (`dist > 7.5 mm`) OR charge timer `t > 2.2 s` | Released: `BALLISTIC_FLIGHT`; Timeout: `CHARGE_BLOCK` |
| `CHARGE_BLOCK` | Charge timer exceeded | Play heavy defensive block smack; ball bounces backward | Player resets ball on tee | `AIM_AND_CHARGE` |
| `BALLISTIC_FLIGHT`| Release validated | Integrate 90 Hz 6-DoF aerodynamics, compute gyroscopic stability, track camera | Ball crosses goalpost plane `x_V ≥ x_goal` OR hits ground | `GOAL_EVALUATE` |
| `GOAL_EVALUATE`| Goalpost crossed | Test crossbar height (`y ≥ 3.0 m`) and upright width (`|z| ≤ 2.8 m`); play audio | Calipers and flags resolved (`t ≥ 2.0 s`) | `SCORE_TALLY` |
| `SCORE_TALLY` | Goal evaluated | Award points, display stadium banner, trigger crowd eruption | Player taps table to proceed | Kick < 5: `NEXT_KICK`; Kick = 5: `MATCH_VICTORY` |
| `NEXT_KICK` | Score tallied | Advance kick index `K ← K + 1`; reposition goalposts further back downfield | Ball seated on tee | `KICK_INTRO` |
| `MATCH_VICTORY`| Kick 5 completed | Grand stadium fireworks; golden confetti shower; World Cup trophy presentation | Session completes | `SESSION_END` |

## 10. Physics & Collision

### Collider Interaction Matrix

| Collider Pair | Interaction Type | Physics Parameters | Response Description |
|---|---|---|---|
| `COL_REAR_PUSH` × Dominant Fingertips | Penalty Impulse Contact | Stiffness `k = 520 N/m`, restitution `e = 0.45` | Captures physical flick velocity `v_rel` and exit angle |
| `COL_RUGBY_CORE` × `COL_KICKING_TEE` | Kinematic Soft Cradle | Restitution `e = 0.05`, friction `μ = 0.65` | Holds ball steady during aiming; releases freely on upward/forward flick |
| `COL_RUGBY_CORE` × `COL_GOALPOST_BAR` | 3D High-Speed Impact | Restitution `e = 0.72`, friction `μ = 0.15` | Crossbar rebound; acoustic metal clang; deflects ball over or under |
| `COL_RUGBY_CORE` × `COL_GOALPOST_L/R` | 3D High-Speed Impact | Restitution `e = 0.72`, friction `μ = 0.15` | Upright post deflection; dramatic rebound test |
| `COL_RUGBY_CORE` × Stadium Turf | Inelastic Ground Bounce | Restitution `e = 0.38`, rolling friction `μ = 0.42` | Ball hits turf short of goal; terminates flight with 0 points |

### Aerodynamic Drag & Magnus Solver Details

- **Time Step**: 90 Hz fixed timestep with 2 sub-steps (`dt = 5.55 ms`).
- **Drag Curve vs. Gyroscopic Stability**:
  `C_D(S_g) = S_g ≥ 1.0 ? 0.12 : (0.12 + 0.26 · (1.0 - S_g))`.
  A properly spun spiral punt experiences **less than one-third** the drag of a tumbling ball!
- **Continuous Collision Sweep**: Given virtual exit speeds up to 34.0 m/s (188 mm per substep), swept capsule continuous collision detection tests ensure the prolate ball never tunnels through the 120 mm diameter goalpost uprights.

## 11. Rendering, Occlusion & Compositing

### Floodlight Atmosphere & Stadium Immersion

1. **Layer 0 (Passthrough)**: Real table, kicking tee patch, and hands.
2. **Layer 1 (Volumetric Floodlight Cones)**: Sweeping stadium light shafts piercing evening mist and rain.
3. **Layer 2 (Downfield Stadium Pitch)**: 70-meter lush green turf pitch with white field lines (22m line, 10m line, try line).
4. **Layer 3 (Yellow Goalposts & Flags)**: Goalposts with wind flags fluttering according to local turbulence vectors.
5. **Layer 4 (Virtual Match Ball Model)**: High-resolution rugby ball with anisotropic leatherette shading and normal-mapped lace seams.
6. **Layer 5 (Golden Spiral Wake VFX)**: Additive GPU ribbon trailing behind the ball's tip, glowing with intensity proportional to `ω_x`.
7. **Layer 6 (Prop Occluder Proxy)**: Prolate spheroid depth mask (`COL_RUGBY_CORE`) rendered at tracked pose `P`.
8. **Layer 7 (Hand Skeletal Occlusion)**: Hand depth proxy ensures fingers occlude the ball and tee naturally during aiming.
9. **Layer 8 (Stadium HUD)**: Trajectory prediction arc (during practice mode), wind vane, and crossbar height indicator.

### VFX Triangles & Particle Budgets

- **Championship Stadium Pitch & Uprights**: 3,600 triangles with LOD downfield.
- **Rugby Match Ball**: 1,680 triangles.
- **Golden Spiral Wake Mesh**: 320 quad ribbon vertices updated per tick.
- **Volumetric Rain Streaks**: 600 GPU instanced quad rain streaks blowing with wind vector.
- **Total Transparent Overdraw**: Peak 1.7× screen area.

## 12. Audio & Feedback

| Event Trigger | Spatial Sound Cue | Frequency / Synthesis Profile | Visual Cue |
|---|---|---|---|
| Axial Spin Twirling | Granular leather friction | Bandpass noise 600–1800 Hz, pitch scaled to `ω_x` | Golden helical sparks ignite along lace seams |
| Gyroscopic Lock (`S_g ≥ 1.0`)| Resonant tonal whistle | Pure sine whistle (1.4 kHz), volume proportional to rpm | Golden spiral wake ribbon flares brightly; ball stabilizes |
| Ballistic Flick Launch | Explosive boot punt "THUMP"| Dual impact: 55 Hz sub-bass boom + 2.2 kHz leather slap | Muzzle-style rain spray ring blows outward from tee |
| Spiral Air Flight | Whistling vortex drone | Ascending high-speed spiral whistle (800 Hz → 2.4 kHz) | Streamlined rain parting trails along ballistic arc |
| Crossbar / Upright Hit | Resonant metal ping | 2.8 kHz hollow steel tubular clang + 450 ms decay | Golden spark burst at post contact; post vibrates |
| Conversion Goal (Success) | 80,000 fan stadium roar | Massive 88 dB coliseum roar + air horn blast | Two referee flags raise; green "GOAL!" banner; fireworks |
| Wide / Short (Miss) | Collective stadium groan | Descending crowd sigh ("Ooooh...") + muted wind | Red "WIDE" or "SHORT" banner; ball bounces away |
| Charge-Down Block | Dull body crunch | 140 Hz flesh thud + referee double whistle | Red impact shield flashes; defensive player celebrates |

## 13. Difficulty Progression & Tuning

### 5-Wave Championship Kick Matrix

| Kick | Scenario Archetype | Downfield Distance | Wind Speed `v_wind` | Wind Azimuth | Target Launch Window | Minimum Spin `ω_crit` |
|---|---|---|---|---|---|---|
| Kick 1 | 25m Center Conversion | 25.0 m | 0.0 m/s (Calm) | None | `θ_pitch = 40° ± 5°` | `50 rad/s` (475 rpm) |
| Kick 2 | 38m Touchline Penalty | 38.0 m | 4.5 m/s | 90° Crosswind (Left to Right)| `θ_pitch = 42° ± 4°`, lead 6° left | `75 rad/s` (715 rpm) |
| Kick 3 | 45m Headwind Drive | 45.0 m | 9.0 m/s Headwind | 180° Direct Headwind | `θ_pitch = 36° ± 3°` (Low Piercing) | `95 rad/s` (900 rpm) |
| Kick 4 | 52m Rushing Scrum Charge| 52.0 m | 6.5 m/s Shifting | 45° Quartering Wind | Release in `< 1.8 s` before block | `105 rad/s` (1000 rpm)|
| Kick 5 | 60m Championship Decider| 60.0 m | 12.0 m/s Gale | Swirling Grandstand Gusts | Pinpoint launch: `v_0 ≥ 30 m/s`, pitch 41°| `120 rad/s` (1145 rpm)|

### Adaptive Tuning Rules

- If player misses 2 consecutive kicks: The kicking tee gains a holographic "Optimal Aim Guide Arrow" projecting the exact wind-compensated elevation and azimuth vector.
- If player splits the uprights dead center (`|z| ≤ 0.4 m`) on 2 consecutive kicks: The crossbar raises dynamically by +0.5 meters for greater precision challenge.

### Named Tuning Constants

```csharp
public static class RugbyTuningConstants
{
    // Prop Dimensions & Inertia
    public const float PROLATE_SEMI_AXIS_A = 0.045f; // 45 mm
    public const float PROLATE_SEMI_AXIS_B = 0.026f; // 26 mm
    public const float PROP_MASS_KG = 0.070f;        // 70 g
    public const float INERTIA_POLAR_IX = 1.893e-5f;
    public const float INERTIA_TRANSVERSE_IT = 3.781e-5f;

    // Kinematic Ballistic Mapping
    public const float BALLISTIC_VELOCITY_GAIN = 40.0f;
    public const float MIN_PHYSICAL_FLICK_SPEED = 0.35f; // m/s
    public const float MAX_PHYSICAL_FLICK_SPEED = 0.85f; // m/s

    // Aerodynamics & Gyroscope
    public const float AIR_DENSITY = 1.225f;          // kg/m^3
    public const float REFERENCE_AREA_S = 0.021f;     // m^2
    public const float DRAG_COEFF_STABLE = 0.12f;
    public const float DRAG_COEFF_TUMBLE = 0.38f;
    public const float CRITICAL_STABILITY_SPIN = 55.0f; // rad/s
    public const float MAGNUS_LIFT_FACTOR = 0.0018f;
}
```

## 14. Edge Cases & Failure Recovery

### Failure Recovery & Edge Case Policies

- **EC-RUG-01: Prop Twirled Off Kicking Tee Before Kick**:
  - *Symptom*: Aggressive finger twirling knocks the physical ball off the tee before launch velocity is achieved (`v_x < 0.20 m/s`).
  - *Detection*: Ball falls off tee cradle without a valid forward flick vector.
  - *Recovery*: The system does NOT count this as a kick attempt. The kick clock freezes, the referee holds his whistle, and a green holographic tee guide highlights the re-mount position.
- **EC-RUG-02: Finger Tracking Loss During High-Speed Twirl**:
  - *Symptom*: Digital fingers move at 4.0 Hz across the laces, causing camera motion blur and temporary skeletal joint dropout.
  - *Detection*: Skeletal joint confidence drops below 0.30 for > 50 ms while lace optical flow remains active.
  - *Recovery*: The system falls back entirely to optical flow patch tracking on the prop's surface laces, calculating `ω_x` directly from the high-contrast seam texture without needing skeletal joint positions.
- **EC-RUG-03: Backward or Upward False Flick**:
  - *Symptom*: Player lifts or pulls the ball backward toward their chest.
  - *Detection*: Launch velocity vector has negative `x̂_T` component (`v_x < 0`).
  - *Recovery*: Launch trigger is rejected; kick state resets to aiming without penalty.
- **EC-RUG-04: Severe Headwind Drift Boundary Overflow**:
  - *Symptom*: In Wave 5 gale conditions, extreme wind blows the ball sideways out of stadium render limits.
  - *Recovery*: Virtual stadium boundaries are clamped at `z = ±25.0 m`; if crossed, the ball is ruled "Out on the Full", crowd sighs, and the scoring engine marks the attempt as missed.

## 15. Implementation Architecture

### Class Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────────┐
│                        RugbyGameDirector                               │
│     (Kick Progression, Match Score, Rules Engine, Stadium Crowd)       │
└───────────────▲────────────────────────▲────────────────────────▲──────┘
                │                        │                        │
┌───────────────┴────────┐  ┌────────────┴───────────┐  ┌─────────┴──────┐
│  PropTrackerRugby      │  │  GyroscopicSpinMeter   │  │ Aerodynamic    │
│  - 72 Hz Prolate Fit   │  │  - Lace Optical Flow   │  │   FlightSim    │
│  - Elevation Pitch Aim │  │  - Stability Factor S_g│  │ - 40x Scaler   │
│  - Flick Release Trigger│ │  - Wake VFX Intensity  │  │ - Magnus Lift  │
└────────────────────────┘  └────────────────────────┘  └────────────────┘
```

### Concrete C# Implementation Scripts

#### Script 1: `GyroscopicSpinMeter.cs`

```csharp
using System;
using UnityEngine;

public class GyroscopicSpinMeter : MonoBehaviour
{
    [Header("Spin Metrics")]
    public float currentSpinRadS = 0.0f; // rad/s along x_P
    public float gyroscopicStabilitySg = 0.0f;
    public bool isGyroscopicallyStable = false;

    // Inertia constants
    public const float INERTIA_POLAR_IX = 1.893e-5f;
    public const float INERTIA_TRANSVERSE_IT = 3.781e-5f;
    public const float CRITICAL_SPIN_RAD_S = 55.0f; // ~525 rpm
    public const float MAX_SPIN_RAD_S = 130.0f;     // ~1240 rpm
    public const float SPIN_DECAY_RATE = 0.15f;     // s^-1

    public void RegisterDigitalTwirlImpulse(float deltaAngularVelocity)
    {
        currentSpinRadS = Mathf.Clamp(currentSpinRadS + deltaAngularVelocity * 1.5f, 0f, MAX_SPIN_RAD_S);
    }

    public void UpdateSpinState(float virtualAirspeed, float angleOfAttackRad, float dt)
    {
        // Decay spin under air/hand friction
        currentSpinRadS = Mathf.MoveTowards(currentSpinRadS, 0f, dt * (SPIN_DECAY_RATE * currentSpinRadS + 2.0f));

        // Overturning aerodynamic moment estimation
        float qDynamic = 0.5f * 1.225f * virtualAirspeed * virtualAirspeed;
        float overturningMoment = qDynamic * 0.021f * 0.090f * 0.45f * Mathf.Abs(Mathf.Sin(angleOfAttackRad));

        // Gyroscopic stability condition: S_g = (Ix^2 * w^2) / (4 * It * M_alpha)
        float denominator = 4.0f * INERTIA_TRANSVERSE_IT * Mathf.Max(overturningMoment, 0.0001f);
        float numerator = (INERTIA_POLAR_IX * currentSpinRadS) * (INERTIA_POLAR_IX * currentSpinRadS);
        gyroscopicStabilitySg = numerator / denominator;

        isGyroscopicallyStable = (currentSpinRadS >= CRITICAL_SPIN_RAD_S) || (gyroscopicStabilitySg >= 1.0f);
    }
}
```

#### Script 2: `AerodynamicFlightSim.cs`

```csharp
using System;
using UnityEngine;

public class AerodynamicFlightSim : MonoBehaviour
{
    [Header("Flight State")]
    public Vector3 ballPosition;
    public Vector3 ballVelocity;
    public Vector3 ballHeading;
    public bool isBallInFlight = false;

    [Header("Goalpost Target")]
    public float goalpostDistanceX = 35.0f;
    public float crossbarHeightY = 3.0f;
    public float uprightHalfWidthZ = 2.8f;

    public event Action<bool, float, float> OnKickResolved; // success, height, lateralOffset

    private const float BALLISTIC_GAIN = 40.0f;
    private const float BALL_MASS = 0.070f;
    private const float GRAVITY = 9.81f;
    private const float AIR_DENSITY = 1.225f;
    private const float REF_AREA = 0.021f;
    private const float MAGNUS_FACTOR = 0.0018f;

    public void LaunchPunt(Vector3 physicalReleaseVelocity, float releasePitchDeg, float releaseYawDeg, float initialSpinRadS)
    {
        // Apply 40x ballistic gain
        Quaternion launchRot = Quaternion.Euler(-releasePitchDeg, releaseYawDeg, 0f);
        Vector3 scaledSpeed = launchRot * Vector3.forward * (physicalReleaseVelocity.magnitude * BALLISTIC_GAIN);

        ballPosition = Vector3.zero;
        ballVelocity = scaledSpeed;
        ballHeading = launchRot * Vector3.forward;
        isBallInFlight = true;
    }

    public void StepFlightSim(GyroscopicSpinMeter spinMeter, Vector3 currentWindVelocity, float dt)
    {
        if (!isBallInFlight) return;

        // Relative airspeed vector
        Vector3 vRel = ballVelocity - currentWindVelocity;
        float speedRel = vRel.magnitude;

        // Angle of attack between ball nose and relative wind
        float angleOfAttack = Vector3.Angle(ballHeading, vRel) * Mathf.Deg2Rad;
        spinMeter.UpdateSpinState(speedRel, angleOfAttack, dt);

        // Drag calculation based on gyroscopic stability
        float cd = spinMeter.isGyroscopicallyStable ? 0.12f : 0.38f;
        Vector3 dragForce = -0.5f * AIR_DENSITY * speedRel * speedRel * REF_AREA * cd * vRel.normalized;

        // Magnus lift force: F_m = S_m * (w x vRel)
        Vector3 spinAxis = ballHeading * spinMeter.currentSpinRadS;
        Vector3 magnusForce = MAGNUS_FACTOR * Vector3.Cross(spinAxis, vRel);

        // Integrate acceleration
        Vector3 totalForce = dragForce + magnusForce + new Vector3(0f, -BALL_MASS * GRAVITY, 0f);
        ballVelocity += (totalForce / BALL_MASS) * dt;
        ballPosition += ballVelocity * dt;

        // Align heading with velocity if gyroscopically stable
        if (spinMeter.isGyroscopicallyStable)
        {
            ballHeading = Vector3.Slerp(ballHeading, ballVelocity.normalized, dt * 4.0f);
        }

        // Test goalpost plane intersection
        if (ballPosition.x >= goalpostDistanceX)
        {
            isBallInFlight = false;
            bool clearedCrossbar = ballPosition.y >= crossbarHeightY;
            bool insideUprights = Mathf.Abs(ballPosition.z) <= uprightHalfWidthZ;
            bool success = clearedCrossbar && insideUprights;

            OnKickResolved?.Invoke(success, ballPosition.y, ballPosition.z);
        }
        else if (ballPosition.y <= 0f)
        {
            // Ball struck turf short of goal
            isBallInFlight = false;
            OnKickResolved?.Invoke(false, 0f, ballPosition.z);
        }
    }
}
```

## 16. Acceptance Test Checklist

| Test ID | Test Scenario & Verification Inputs | Quantitative Pass Threshold |
|---|---|---|
| `TC-RUG-01` | Prolate Spheroid Prop Fit | Present 90.0 × 52.0 mm prolate prop at 0.40 m distance | Ellipsoid fit converges < 0.9 s; `a = 45.0 ± 0.9 mm`, `b = 26.0 ± 0.6 mm` |
| `TC-RUG-02` | Lace Optical Flow Spin Tracking | Spin ball at synthetic 900 rpm (94.2 rad/s) along `x̂_P` | Measured `ω_x = 94.2 ± 3.5 rad/s`; zero tracking stutter or reverse aliasing |
| `TC-RUG-03` | Gyroscopic Stability Threshold | Twirl ball to `ω_x = 65.0 rad/s`; measure aerodynamic drag in flight | Ball achieves `S_g ≥ 1.0`; drag coefficient drops to `0.12 ± 0.01` (stable spiral) |
| `TC-RUG-04` | Unstable Tumble Penalty | Launch ball with zero spin (`ω_x = 0 rad/s`) into 5 m/s crosswind | Drag jumps to `0.38 ± 0.03`; ball flutters wildly; lateral drift exceeds 4.5 m |
| `TC-RUG-05` | Ballistic Velocity Scaling (40×) | Forward finger flick at physical `v_phys = 0.50 m/s` | Virtual exit velocity `v_0 = 20.0 ± 0.8 m/s` (40.0× gain ±4 %); launch delay < 35 ms |
| `TC-RUG-06` | Elevation Pitch Alignment | Tilt ball to `θ_pitch = 42.0°`; release at 0.60 m/s (24 m/s virtual) | Flight apex reaches `14.8 ± 0.6 m`; maximum range `44.0 ± 2.0 m` |
| `TC-RUG-07` | Crossbar Clearance Evaluation | Virtual ball passes goalpost plane at `y = 4.2 m, z = 0.5 m` | System tags valid conversion; plays coliseum roar; awards 500 points |
| `TC-RUG-08` | Upright Post High-Speed Collision | Direct kick trajectory into upright cylinder at `z = 2.8 m` | Rebound impulse triggers; audio clang plays; post vibrates with 12 mm amplitude |
| `TC-RUG-09` | Rushing Scrum Charge-Down Gate | Delay physical flick launch for 2.3 seconds after whistle | Charge-down block triggers; defensive player deflects ball; 0 score awarded |
| `TC-RUG-10` | 5-Kick Championship Match Soak | Complete all 5 championship kicks (4.0 minutes) at 90 Hz render | Zero frame drops below 87 Hz; memory allocation < 1.5 MB/min; stable trajectory |

---
