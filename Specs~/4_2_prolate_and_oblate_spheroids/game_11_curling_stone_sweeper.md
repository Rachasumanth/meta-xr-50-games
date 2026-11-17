# Curling Stone Sweeper

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Curling Stone Sweeper |
| Slug | `curling_stone_sweeper` |
| Physical prop | Oblate smooth granite stone / heavy rubber hockey puck; equatorial diameter 80.0 mm (`a = b = 40.0 mm`), polar height 32.0 mm (`c = 16.0 mm`); mass 140 g; polished composite stone or dense vulcanized rubber with low-friction PTFE base ring |
| Sub-shape & Family | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate and Oblate Spheroids (oblate, palm-scale, planar sliding with spin curl) |
| Scale class | Tabletop / lap-tray scale; physical sweep zone 140 × 90 mm; virtual curling sheet 1200 mm long; forward motion gain 4.0×; lateral curl gain 2.5× |
| Source concept quote | "Olympic ice curling championship. Player shoves the oblate stone across the lap/table surface with an initial velocity vector, then uses dominant index+middle fingertips to sweep the virtual ice ahead to heat the surface, reduce drag, and curl the stone into the center house button." |
| Physical input | Non-dominant or dominant palm/fingers deliver the stone with initial velocity `v_0` and axial spin `ω_z` (in-turn / out-turn) from the hack; dominant index and middle fingertips rapidly alternate lateral sweeping strokes on the table surface 20–80 mm ahead of the sliding stone |
| Session length | 5 ends (waves), 45–65 s active time per end; total session duration ≈5.5 minutes including end resets and scoring ceremonies |
| Lose condition | Stone stalls short of the hog line (`x_V < 400 mm`), slides out past the back line (`x_V > 1250 mm`), exits lateral sheet boundaries (`|y_V| > 180 mm`), or scores 0 points across 5 ends against opponent AI ghost stones |

## 1. Fantasy & Core Loop

In a breathless Olympic curling coliseum bathed in crisp stadium floodlights, you stand in the hack on the frozen championship sheet. Under your fingers sits a miniaturized 140 g oblate granite curling stone, complete with its gleaming bronze gooseneck handle fixture. The virtual ice sheet stretches forward across your table, glistening with microscopic frozen droplets known as pebble ice. At the far end of the sheet lies the four-ring circular target: the 12-foot blue outer ring, 8-foot white ring, 4-foot red ring, and the coveted golden center button.

Delivering a winning stone demands two synchronized athletic skills: the gentle release of the stone with calibrated speed and axial twist (spin), followed by ferocious, high-cadence fingertip sweeping on the ice directly in front of the running stone. Rapidly oscillating your index and middle fingertips across the table surface heats the microscopic pebble peaks, melting a microscopic liquid water lubrication film that lowers friction by up to 68 %, holding the stone on a straight glide or delaying its natural directional curl until it breaks hard around opponent guard stones right into the button.

### 30–60s Active Gameplay Cycle

1. **Aim & Line Assessment (0.0–2.0 s)**: The skip's broom target appears in the house. Player aligns the oblate stone behind the hack line (`x_T = 0`), reading the ice sheet velocity profile and opponent guard stone positions.
2. **Delivery & Twist Release (2.0–3.5 s)**: Player pushes the stone along the launch vector with forward velocity `v_x ∈ [0.12, 0.45] m/s`, imparting a clockwise (in-turn, `ω_z < 0`) or counter-clockwise (out-turn, `ω_z > 0`) spin of 2.0–8.0 rad/s before releasing contact at the physical delivery line.
3. **Hog-Line Crossing (3.5–4.2 s)**: The stone must cross the virtual hog line (`x_V = 400 mm`) with velocity `v_x > 0.08 m/s`. Failure to reach the hog line results in a disqualified burned stone.
4. **Active Sweeping Cadence (4.2–12.0 s)**: As the stone glides down the 1200 mm virtual sheet, the player positions dominant index and middle fingertips 20–80 mm ahead of the moving stone, sweeping back and forth laterally at 3–7 Hz. Sweeping deposits thermal energy (`dQ/dt`), raising local ice temperature from −5.0 °C toward −0.2 °C, reducing kinetic friction coefficient `μ_k` from 0.038 down to 0.012 and suppressing lateral curl bite.
5. **Directional Sculpting & Guard Clearance (12.0–16.0 s)**: Player selectively sweeps only the left or right half of the ice corridor to create an asymmetrical thermal gradient, steering the stone's curl trajectory around protective opponent guard stones.
6. **House Deceleration & Impact Resolution (16.0–20.0 s)**: The stone enters the house rings (`x_V ∈ [950, 1150] mm`), colliding elastically with existing stones or gently coming to rest atop the golden button.
7. **Score Tally & End Reset (20.0–25.0 s)**: Measurement calipers render, verifying distance from the button center; points are awarded, house clears, and the next end commences.

### 5-Wave Match Structure

- **End 1 (Open Sheet Draw)**: Clean sheet with no guards. Target: Draw directly onto the 100 mm button with light sweeping calibration.
- **End 2 (Single Center Guard)**: Opponent stone blocks center line at `x_V = 750 mm`. Player must execute an out-turn curl with delayed late sweeping to hook around the guard into the 4-foot ring.
- **End 3 (Split House Takeout)**: Two opponent stones occupy the 8-foot ring. Player delivers a firm, high-velocity strike (`v_0 = 0.42 m/s`) with continuous high-cadence sweeping to maintain line and execute a double-takeout.
- **End 4 (Corner Guard & Tight Port)**: Two offset guards create a 65 mm narrow corridor. Requires precision delivery angle (±1.5°) and asymmetrical sweeping to thread the needle.
- **End 5 (Championship Final Stone)**: Opponent holds shot stone 22 mm from the button. Final stone requires heavy in-turn spin, rapid cadence sweeping (≥5.5 Hz) to maintain momentum, and a gentle tap to promote your stone while spilling opponent shot rock out of scoring rings.

## 2. Platform Baseline

- **Hardware**: Standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No physical controllers; the only tangible prop is the registered oblate stone/puck.
- **Joint Naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc, mcp, pip, dip, tip (thumb: cmc, mcp, ip, tip). Example: `R_index_tip`, `R_middle_tip`.
- **Prop Registration**: At session start, the player places the oblate stone on the table surface; the vision pipeline detects the flat support plane `T`, segments the 80.0 mm diameter circular rim silhouette, computes the 6-DoF prop pose `P` at 72 Hz, and continuously tracks planar translation `(x_P, y_P)` and axial yaw `ψ_P`.
- **Coordinate Frames**:
  - `W`: Room world frame (Y up, cm/meters).
  - `T`: Support surface frame (detected table plane; origin at table center, `+ŷ_T` table surface normal pointing up, `+x̂_T` pointing forward down-sheet, `+ẑ_T = x̂_T × ŷ_T` pointing to player's right).
  - `P`: Prop local frame (origin at geometric center of oblate spheroid; `+ẑ_P` along minor polar axis normal to flat faces, `+x̂_P` and `+ŷ_P` in the circular equatorial plane).
  - `V`: Virtual play frame (sheet coordinate system, anchored to `T` at the hack origin).
- **Contact Detection**: Joint contact detected when signed distance from hand joint proxy sphere to the prop's oblate collider is < 5.0 mm; release hysteresis 8.0 mm; debounce threshold 30 ms. Tabletop contact detected when joint distance to `y_T = 0` is < 4.0 mm.
- **Physics Engine**: Fixed 90 Hz timestep, max 2 sub-steps (11.1 ms / 5.55 ms); continuous collision detection (CCD) on swept stone proxy; tabletop scale constraint (< 1.4 m virtual length).
- **Rendering & Compositing**: Passthrough base layer; virtual curling sheet depth-composited against scene mesh and hand mesh; real-time pebble ice caustics, sweeping frost clearing, and thermal gradient heatmaps.
- **Feedback**: Multi-channel spatial audio (scraping stone roar, granular broom sweep swish, stone-on-stone granite crack) and visual VFX (ice pebble shaving particles, thermal steam plumes).

### 2.1 Game-Specific Overrides

- **Prop Frame `P` & Geometry**: The prop is an oblate spheroid of revolution with equatorial semi-axes `a = b = 40.0 mm` (80.0 mm outer diameter) and polar semi-axis `c = 16.0 mm` (32.0 mm total axial thickness). When resting flat on the table, `+ẑ_P` aligns closely with table normal `+ŷ_T`.
- **Curling Kinematics & Rolling Model**: Unlike a freely rolling sphere, an oblate curling stone slides on a narrow annular contact ring (radius `r_contact = 34.0 mm`) representing the hollow running band of a real curling rock. The contact normal is `n̂_T = +ŷ_T`. Motion is planar on the table surface with 3 degrees of freedom: forward position `x_T`, lateral position `z_T`, and axial rotation angle `ψ_T` (spin).
- **Virtual Frame `V` Anchoring**: `V` is anchored to `T` at the delivery hack position: `origin_V = (0, 0, 0)_T`.
  - Virtual X axis (`x̂_V`): Along table length, pointing toward the house. Length = 1200 mm.
  - Virtual Y axis (`ŷ_V`): Normal to table surface, pointing up.
  - Virtual Z axis (`ẑ_V`): Lateral sheet axis across the sheet width (width = 360 mm, `z_V ∈ [−180, +180] mm`).
- **Motion Gain & Sheet Scaling**: Physical table stroke zone is 140 mm long (`x_T ∈ [0, 140] mm`). Virtual curling sheet is 1200 mm long (`x_V ∈ [0, 1200] mm`). Motion gain is applied in two distinct operational phases:
  1. *Delivery Phase (`x_T ≤ 120 mm`)*: 1:1 direct tracking while held in hand.
  2. *Free-Slide Phase (after release)*: The stone's measured physical release velocity vector `v_rel = (v_x, v_z)_T` and angular velocity `ω_z` are mapped to virtual release velocity: `v_V = (4.0 · v_x, 2.5 · v_z)`. The physical stone coasts across the physical table patch (stopping within 120–160 mm due to table friction), while the virtual stone model glides along the full 1200 mm virtual sheet.
- **Specific Deviations**:
  - The real physical stone reaches rest within 140 mm due to acrylic/wood table friction (`μ_table ≈ 0.22`), while the virtual stone slides up to 1200 mm on low-friction virtual ice (`μ_ice ∈ [0.012, 0.038]`).
  - Physical sweeping occurs on the physical table surface directly in front of the resting/slowly sliding real stone, but its thermal effect is mapped directly into the virtual stone's path in frame `V`.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Play Posture | Seated at standard table (height 72–76 cm) or firm lap-tray desk | Forearm supported on table edge; minimizes shoulder fatigue during high-cadence sweeping |
| Active Table Surface | 450 mm wide × 300 mm deep flat rigid surface | Accommodates 140 mm physical stone slide plus 120 mm lateral sweeping strokes |
| Stone Delivery Zone | `x_T ∈ [0, 120] mm`, `z_T ∈ [−60, +60] mm` | Natural comfortable reach envelope for wrist extension/flexion without torso leaning |
| Fingertip Sweeping Zone | `x_T ∈ [80, 220] mm`, `z_T ∈ [−90, +90] mm` | Positioned 20–80 mm ahead of stone release; permits vigorous index/middle finger scrub |
| Fingertip Sweeping Cadence | 3.0–7.0 Hz lateral oscillation (peak 8.0 Hz) | Achievable via wrist and finger flexion without inducing tendonitis or repetitive strain |
| Sweeping Contact Force | 0.4–1.2 N downward pressure on table | Sufficient for stable fingertip skeletal tracking while avoiding table scuffing or joint fatigue |
| Rest Intervals | 12 s reset and score presentation between ends | Allows muscular relaxation of finger flexors and extensors |
| Neck Viewing Angle | 30°–45° downward pitch toward table plane | Ergonomic sweet spot for Quest 3 / Vision Pro passthrough camera tracking and comfort |

### Fatigue Mitigations
1. **Sweeping Cadence Windowing**: Sweeping is only effective when the stone is within active gliding velocity (`0.06 < v_V < 0.80 m/s`), preventing premature or useless over-sweeping.
2. **Dual-Fingertip Alternation**: The algorithm tracks both `R_index_tip` and `R_middle_tip`, allowing the player to sweep with both fingers coupled, spreading contact pressure over two digital pads.
3. **Sweeping Power Saturation**: Thermal deposition saturates at 6.0 Hz oscillation; stroking faster than 6.0 Hz yields diminishing returns (< 5 % extra heating), discouraging frantic strain.

## 4. Prop Geometry & Registration

```
          +ẑ_P (Minor Polar Axis, c = 16.0 mm)
            ▲
            │       Handle Boss (h = 10 mm, r = 8 mm)
       ┌────┴────┐  
    .-'  ┌─────┐  '-.
  .'     │     │     '.
 /       └─────┘       \  Equatorial Major Radius a = b = 40.0 mm
(───────────────────────) ◄─── Equator (Outer Rim Ø = 80.0 mm)
 \                     /
  '.   Running Band  .'   Hollow concave bottom with running band r = 34.0 mm
    '-.  [ooooo]  .-'
       └────┬────┘
            ▼
```

### Prop Dimensions & Mass Properties

- **Equatorial Semi-Axes**: `a = b = 40.0 mm` (equatorial diameter = 80.0 mm).
- **Polar Semi-Axis**: `c = 16.0 mm` (total axial height = 32.0 mm).
- **Spheroid Eccentricity**: `e = √(1 - c²/a²) = √(1 - 16²/40²) = √(1 - 0.16) = √0.84 ≈ 0.9165` (highly oblate).
- **Mass**: `m = 140.0 g` (0.140 kg).
- **Volume**: `V = (4/3) · π · a · b · c = (4/3) · π · 40 · 40 · 16 = 107,233 mm³ ≈ 0.1072 liters`.
- **Mean Density**: `ρ = m / V = 140 g / 107.23 cm³ ≈ 1.305 g/cm³`.
- **Moments of Inertia**:
  - Spin Axis (Polar `ẑ_P`): `I_z = (2/5) · m · a² = 0.4 · 0.140 kg · (0.040 m)² = 8.96 × 10⁻⁵ kg·m²`.
  - Transverse Axes (`x̂_P`, `ŷ_P`): `I_x = I_y = (1/5) · m · (a² + c²) = 0.2 · 0.140 kg · (0.040² + 0.016²) m² = 5.20 × 10⁻⁵ kg·m²`.

### Surface-Feature Atlas

| Feature ID | Latitude `φ` | Azimuth `λ` | Radial Extent | Description & Functional Purpose |
|---|---|---|---|---|
| F1: Top Handle Boss | +65° to +90° | 0° to 360° | `r ≤ 12.0 mm` | Cylindrical raised knob simulating stone handle; primary grip point for index/thumb pinch delivery |
| F2: Upper Shoulder | +25° to +65° | 0° to 360° | `12 < r ≤ 36 mm` | Smooth convex granite shoulder; secondary palm cup contact zone |
| F3: Striking Band (Rim) | −15° to +15° | 0° to 360° | `36 < r ≤ 40 mm` | Equatorial outer perimeter; visual optical tracking marker ring and inter-stone collision band |
| F4: Lower Bevel | −60° to −15° | 0° to 360° | `28 < r ≤ 38 mm` | Inward sloping lower flank; clearance for table surface irregularities |
| F5: Running Edge Ring | −75° to −60° | 0° to 360° | `r = 34.0 ± 2.0 mm` | Raised annular ring resting on table surface; physical PTFE contact ring providing true annular glide |
| F6: Concave Hollow Cup | −90° to −75° | 0° to 360° | `r < 32.0 mm` | Recessed bottom cavity; non-contact underbody preventing suction to table surface |

### Collider Inventory

| Collider ID | Type | Geometry / Semi-Axes | Offset in `P` | Purpose |
|---|---|---|---|---|
| `COL_PROP_BODY` | Oblate Spheroid | `a = b = 40.0 mm`, `c = 16.0 mm` | `(0, 0, 0)` | Primary hand contact and depth-proxy rendering occluder |
| `COL_HANDLE` | Cylinder | `r = 8.0 mm`, `h = 10.0 mm` | `(0, 0, +16.0 mm)` | Top handle contact volume for precision pinch detection |
| `COL_RUNNING_RING`| Torus | Major `R = 34.0 mm`, minor `r = 1.2 mm` | `(0, 0, −15.0 mm)` | Table plane contact and frictional torque generator |
| `COL_SWEEP_INDEX` | Capsule | `r = 6.0 mm`, length 18.0 mm | Tracked `R_index_tip` | Dominant index fingertip sweeping collider |
| `COL_SWEEP_MID` | Capsule | `r = 6.0 mm`, length 18.0 mm | Tracked `R_middle_tip`| Dominant middle fingertip sweeping collider |
| `COL_VIRTUAL_ROCK`| Cylinder | `r = 40.0 mm`, `h = 32.0 mm` | Virtual stone `p_V` | Virtual physics simulation and inter-stone collision resolver |

### Optical Registration & Pose Fitting

1. **Table Plane Lock**: Support surface detection identifies horizontal table plane `T` (`y_T = 0`) with plane normal error < 1.0°.
2. **Rim Contour Extraction**: Color passthrough edge-detection identifies the 80.0 mm circular silhouette at 72 Hz. An ellipse is fitted to the camera projection using direct least-squares fitting.
3. **Pose Optimization**: Levenberg-Marquardt solver estimates 6-DoF pose `P`. Acceptance criteria: RMS reprojection residual < 0.65 mm, height above table `|y_P - 16.0 mm| < 2.0 mm`.
4. **Axis Alignment**: Because the prop is an oblate spheroid with planar symmetry, polar axis `ẑ_P` is constrained to table normal `+ŷ_T`. Axial spin yaw `ψ_P` is initialized from the handle orientation feature or high-contrast top logo marker.

## 5. Hand Tracking Input Map

| ID | Gesture / Input | Active Joints | Prop / Surface Region | Semantic Gameplay Output |
|---|---|---|---|---|
| `IN_GRIP_DELIVERY` | Two-Finger Handle Pinch | `R_thumb_tip`, `R_index_tip` | Handle Boss (`F1`), `r ≤ 12 mm` | Grasp stone; initiates aiming and delivery alignment phase |
| `IN_PALM_CUP` | Palm Heel Cup & Push | `R_palm`, `R_index_mcp`, `R_thumb_ip` | Rear Upper Shoulder (`F2`), `λ ∈ [140°, 220°]` | Forward delivery push; injects forward momentum `v_x` |
| `IN_TWIST_RELEASE` | Axial Pronation / Supination | `R_thumb_tip`, `R_index_tip` release | Handle Boss (`F1`) + Rim (`F3`) | Imparts release spin `ω_z`: clockwise (`ω_z < 0`, in-turn) or counter-clockwise (`ω_z > 0`, out-turn) |
| `IN_RELEASE_DETECT` | Simultaneous Hand Breakaway | All right hand joints signed dist > 8.0 mm | All prop colliders | Delivery release trigger; captures `v_0` and `ω_z`; transitions from manual control to free curl slide |
| `IN_SWEEP_STROKE` | High-Speed Lateral Scrub | `R_index_tip`, `R_middle_tip` | Table surface `T` (`y_T < 4 mm`), 20–80 mm ahead of stone | Sweeping stroke event; generates thermal friction energy `dQ/dt` |
| `IN_ASYMM_SWEEP_L`| Left-Corridor Sweeping | `R_index_tip`, `R_middle_tip` | Table surface `z_T ∈ [−80, −10] mm` ahead of stone | Asymmetrical heating; induces rightward curl bias |
| `IN_ASYMM_SWEEP_R`| Right-Corridor Sweeping | `R_index_tip`, `R_middle_tip` | Table surface `z_T ∈ [+10, +80] mm` ahead of stone | Asymmetrical heating; induces leftward curl bias |
| `IN_SKIP_CALL_LOOK`| Head Pose Gaze Angle | Head forward vector | Virtual House (`x_V ∈ [950, 1150] mm`) | Zooms virtual house overlay; displays skip broom target and distance reticle |
| `IN_RESET_PALM` | Non-Dominant Palm Flat | `L_palm`, `L_fingers` flat on table > 1.0 s | Table surface `x_T < −50 mm` | Aborts current delivery; resets stone to hack line |

## 6. Input Kinematics & Signal Processing

```
72 Hz Hand Tracking ──► OneEuro Filter ──► Table Contact Gate ──► Cadence Integrator ──► Thermal Diffusion
[R_index_tip / mid]      (fc=1.5, β=0.04)    (dist_T < 4.0 mm)       (Zero-Crossings)      (T_ice, μ_ice)
                                                                                                  │
72 Hz Prop Pose P ────► 12-Frame Window ──► Release Velocity ───► Virtual Ballistics ◄────────────┘
(x_P, z_P, ψ_P)          Linear Fit          (v_0, ω_z)           (Maeno Curl Model)
```

### Sweeping Kinematics & Frequency Extraction

Dominant fingertip positions `p_index(t)` and `p_middle(t)` are sampled at 72 Hz and filtered using a OneEuro filter (`f_min = 1.5 Hz`, `β = 0.040`, derivative cutoff `f_d = 8.0 Hz`) to eliminate tracking tremor while preserving rapid directional reversals.

1. **Sweeping Centroid**:
   `p_sweep(t) = 0.5 · (p_index(t) + p_middle(t))`
2. **Table Normal Proximity Gate**:
   `ContactValid = (p_sweep.y - y_table < 4.0 mm) ∧ (|v_sweep.y| < 0.15 m/s)`
3. **Lateral Sweeping Speed**:
   `v_lat(t) = d(p_sweep.z)/dt`
4. **Cadence Zero-Crossing Detector**:
   A sweeping stroke reversal is registered when `v_lat(t)` changes sign and `|v_lat|` exceeds 0.18 m/s:
   `StrokeValid = (sign(v_lat(t)) ≠ sign(v_lat(t - Δt))) ∧ (|v_lat_peak| ≥ 0.18 m/s)`
   Sweeping frequency `f_sweep` is calculated over a rolling 3-stroke window:
   `f_sweep = 1.0 / (2.0 · Δt_stroke_avg)` (clamped to `[0.0, 8.0] Hz`).

### Sweeping Thermal Transfer Function

The virtual ice sheet maintains a 2D thermal grid `T_ice(x, z)` initialized to ambient ice temperature `T_ambient = −5.0 °C`. As fingertips scrub the table in front of the virtual stone:

`dQ_in/dt = η_sweep · f_sweep · F_press · |v_lat|`

Where:
- Mechanical conversion efficiency `η_sweep = 0.42 J/(N·m)`.
- Fingertip downward normal force `F_press = clamp(k_press · (4.0 mm - (p_sweep.y - y_table)), 0.3 N, 1.4 N)` with `k_press = 0.35 N/mm`.
- Ice thermal decay obeys Newton cooling:
  `dT_ice(x, z)/dt = (1 / C_ice) · (dQ_in/dt) - k_cool · (T_ice(x, z) - T_ambient)`
  with thermal capacitance `C_ice = 18.5 J/(m²·°C)` and cooling constant `k_cool = 0.85 s⁻¹`.
- Maximum surface temperature is clamped to `−0.15 °C` (below free water puddle threshold).

### Delivery Kinematics & Release Velocity Extraction

During delivery, the stone pose is monitored in table frame `T`. Release is triggered when all hand contacts detach (`signed_distance > 8.0 mm` for ≥ 30 ms).

1. **Release Linear Velocity**:
   A linear least-squares regression over the final 12 frames (166 ms) prior to release determines physical release velocity:
   `v_rel_x = slope(x_P(t)), v_rel_z = slope(z_P(t))`
   Virtual release velocity vector in frame `V`:
   `v_0_x = clamp(4.0 · v_rel_x, 0.10 m/s, 1.20 m/s)`
   `v_0_z = clamp(2.5 · v_rel_z, −0.40 m/s, +0.40 m/s)`
2. **Release Spin Angular Velocity**:
   `ω_z = slope(ψ_P(t))` over the same 12 frames, clamped to `[−10.0, +10.0] rad/s`.
   If `|ω_z| < 1.0 rad/s`, a minimum stabilizing curl spin of `sign(ω_z) · 1.0 rad/s` is automatically seeded based on delivery yaw bias.

### Curling Stone Ballistics & Asymmetrical Curl Physics

The virtual stone trajectory integrates the Maeno-Shegelski curling physics model. A spinning curling stone curls in the direction of its front-running band spin due to asymmetric friction across the annular contact ring:

1. **Friction Coefficient vs. Ice Temperature & Velocity**:
   `μ_k(x, z, v) = μ_base · [1.0 - α_T · (T_ice(x, z) - T_ambient)] / [1.0 + β_v · √(max(v, 0.01))]`
   Where:
   - Base pebble friction `μ_base = 0.038`.
   - Temperature sensitivity coefficient `α_T = 0.14 °C⁻¹` (sweeping lowers `μ_k` down to 0.012).
   - Hydrodynamic melt-film coefficient `β_v = 0.45 (m/s)⁻⁰·⁵`.
2. **Linear Deceleration**:
   `a_drag = −μ_k(x, z, v) · g · normalize(v_V)`
3. **Curl Lateral Acceleration**:
   The asymmetric friction between the advancing and retreating edges of the annular contact ring produces a net lateral force perpendicular to heading:
   `a_curl = sign(ω_z) · k_curl · [μ_k(x, z_retreat, v) - μ_k(x, z_advance, v)] · g · (ẑ_V × normalize(v_V))`
   Where:
   - Curl coupling constant `k_curl = 0.0185`.
   - `z_advance = z_stone + r_contact · sign(ω_z)`.
   - `z_retreat = z_stone - r_contact · sign(ω_z)`.
   Sweeping the *entire* path reduces both `μ_advance` and `μ_retreat`, diminishing friction and keeping the stone gliding further while delaying curl break. Sweeping *asymmetrically* (e.g. only the retreat side) drastically alters `(μ_retreat - μ_advance)`, allowing dynamic steering!
4. **Rotational Spin Decay**:
   `dω_z/dt = −(r_contact / I_z) · μ_k · m · g · sign(ω_z) · 0.045`

## 7. Virtual World Layout & Coordinate Mapping

```
                                VIRTUAL PLAY FRAME (V)
◄─────────────────────────────── Sheet Length 1200 mm ──────────────────────────────►
┌──────────────────────┬─────────────────────────┬──────────────────────────────────┐
│ HACK ZONE (0-150 mm) │ RUNNING SHEET (150-900) │ HOUSE RINGS (950-1150 mm)        │
│                      │                         │                                  │
│   [Real Prop Zone]   │     Hog Line (400 mm)   │      12-ft Ring (Ø 240 mm) [Blue]│
│        (●)           │            │            │       8-ft Ring (Ø 160 mm) [Wht] │
│   x_T: 0-140 mm      │            │            │       4-ft Ring (Ø 80 mm)  [Red] │
│                      │                         │      (◎) Button (Ø 30 mm)  [Gld] │
│                      │   Sweeping Thermal Path │       Tee Line (x = 1050 mm)     │
└──────────────────────┴─────────────────────────┴──────────────────────────────────┘
▲                      ▲                         ▲
x_V = 0                x_V = 400 mm              x_V = 1050 mm (Center Button)
```

### Coordinate Transforms

- `P_to_T`: `p_T = R_P_to_T · p_P + p_P_in_T`
- `T_to_V (Delivery Phase)`: `p_V = (p_T.x, 0.0, p_T.z)` (1:1 registration while grasped).
- `T_to_V (Free Flight Phase)`:
  `p_V(t) = p_V(t_release) + ∫ v_V(τ) dτ`
  The real prop stays on the physical table patch (`x_T ≤ 140 mm`), while the virtual stone is projected down the 1200 mm virtual sheet.
- **Sweeper Hand Mapping**: Physical table sweeping at `(x_T, z_T)` maps into virtual sheet coordinates directly ahead of the virtual stone:
  `x_sweep_V = x_stone_V + (x_sweep_T - x_prop_T) · 1.5`
  `z_sweep_V = z_stone_V + z_sweep_T · 1.2`

### Virtual Entity Anchoring Table

| Entity Name | Frame / Parent | Position `(x, y, z)` [mm] | Dimensions / Geometry | Visual Representation & Behavior |
|---|---|---|---|---|
| Hack Origin | `V` | `(0, 0, 0)` | 80 × 50 mm foot brace | Rubberized foothold with virtual granite footing |
| Delivery Line | `V` | `(120, 0, 0)` | Line across `z ∈ [−180, +180]` | White line; marks mandatory physical release point |
| Hog Line | `V` | `(400, 0, 0)` | 15 mm thick red stripe | Red boundary stripe; stone must cross before stopping |
| Center Line | `V` | `x ∈ [0, 1250], z = 0` | 2 mm blue central guide line | Sheet centerline guiding draw shots |
| Tee Line | `V` | `(1050, 0, 0)` | 5 mm blue cross line | Intersects center button; optimal stopping line |
| House Outer Ring | `V` | `(1050, 0, 0)` | Outer `r = 120 mm` (12-foot ring) | Translucent royal blue ring on ice sheet |
| House White Ring | `V` | `(1050, 0, 0)` | Outer `r = 80 mm` (8-foot ring) | Crisp white pebble ring |
| House Red Ring | `V` | `(1050, 0, 0)` | Outer `r = 40 mm` (4-foot ring) | Crimson ring |
| Center Button | `V` | `(1050, 0, 0)` | Radius `r = 15 mm` (button) | Gleaming gold reflective circle; target bulls-eye |
| Player Virtual Stone | `V` | Dynamic `p_V(t)` | Cylinder `r = 40 mm`, `h = 32 mm` | Granite stone with team colored red handle |
| Opponent Guard 1 | `V` | Fixed per end, e.g. `(750, 0, 25)` | Cylinder `r = 40 mm`, `h = 32 mm` | Granite stone with yellow handle; obstacle |
| Opponent House Stone | `V` | Fixed per end, e.g. `(1070, 0, −18)`| Cylinder `r = 40 mm`, `h = 32 mm` | Opponent scoring stone to be knocked out |
| Sweeping Thermal Ribbon | `V` | Ahead of virtual stone | 120 × 60 mm grid, 20 cells | Cyan-to-amber thermal heatmap overlaid on ice |

## 8. Gameplay Systems & Rules

### Match Format & Scoring Mechanics

The match consists of **5 Ends**. In each end, the player delivers 1 shot stone against an existing tactical arrangement of opponent stones (guards and house rocks).

1. **Scoring Formula per End**:
   At the conclusion of an end (when all stones reach zero velocity `v < 0.002 m/s`):
   - **Distance to Button**:
     `d_button = ||(x_stone_V - 1050, z_stone_V - 0)||` [mm]
   - **In-The-House Gate**:
     Stone scores only if `d_button < 120.0 mm` (inside the 12-foot ring).
   - **Points Awarded**:
     - *Center Button Shot (`d_button ≤ 15.0 mm`)*: **500 Points** + "Perfect Draw" bonus (+250 pts).
     - *Four-Foot Ring (`15.0 < d_button ≤ 40.0 mm`)*: **350 Points**.
     - *Eight-Foot Ring (`40.0 < d_button ≤ 80.0 mm`)*: **200 Points**.
     - *Twelve-Foot Ring (`80.0 < d_button ≤ 120.0 mm`)*: **100 Points**.
     - *Outside House (`d_button > 120.0 mm`)*: **0 Points**.
   - **Opponent Takeout Bonus**: Knocking an opponent stone out of the 120 mm house ring awards **+150 Points** per stone.
   - **Guard Collision Penalty**: Unintended clipping of a center guard that prevents entering the house deducts **−50 Points**.

2. **Hog-Line Rule**:
   If the stone's center fails to clear the hog line (`x_V < 400.0 mm`) before stopping, the stone is ruled "Hogged" (Burned) and is removed immediately with 0 points.

3. **Back-Line & Boundary Rules**:
   - If `x_V > 1250.0 mm` (crossed the back line behind the house), stone is out of play.
   - If `|z_V| > 180.0 mm` (crossed side lines), stone is out of play.

4. **Win / Lose Criteria**:
   - **Match Victory**: Cumulative score across 5 ends ≥ **1,200 Points**.
   - **Match Loss**: Cumulative score < 1,200 Points, or 2 or more hogged/burned stones.

## 9. Game State Machine

```
┌─────────┐      ┌───────────────┐      ┌────────────┐      ┌─────────────┐
│  BOOT   ├─────►│  REGISTER_T   ├─────►│ CALIBRATE  ├─────►│ END_INTRO   │
└─────────┘      └───────────────┘      └────────────┘      └──────┬──────┘
                                                                   │
┌──────────────┐      ┌───────────────┐      ┌───────────┐         │
│ MATCH_RESULT │◄─────┤ TALLY_SCORES  │◄─────┤ AT_REST   │◄────────┤
└──────────────┘      └───────────────┘      └─────▲─────┘         │
                             │                     │               │
                             ▼                     │               ▼
                      ┌───────────────┐            │        ┌─────────────┐
                      │ NEXT_END_INIT ├────────────┘        │ AIM_AND_SET │
                      └───────────────┘                     └──────┬──────┘
                                                                   │
                      ┌───────────────┐                     ┌──────▼──────┐
                      │ FREE_SLIDE_   │◄────────────────────┤ DELIVER_    │
                      │ SWEEPING      │                     │ STROKE      │
                      └───────────────┘                     └─────────────┘
```

### State Machine Transition Table

| State Name | Entry Conditions / Events | Active Processing & Updates | Exit Guard / Transition Condition | Next State |
|---|---|---|---|---|
| `BOOT` | Application launch | Load audio banks, compile shaders, initialize hand tracking | System tracking ready | `REGISTER_T` |
| `REGISTER_T` | Plane detection active | Detect table plane `T` and table normal `+ŷ_T` | Table plane confirmed (`residual < 1.0 mm`) | `CALIBRATE` |
| `CALIBRATE` | Prop placed on table | Spheroid contour fitting, register 6-DoF pose `P` | Prop fitted (`RMS < 0.65 mm`, held 1.0 s) | `END_INTRO` |
| `END_INTRO` | Session start or end cleared | Render tactical end number (End 1 to 5), spawn opponent stones, highlight skip broom | Timer `t ≥ 2.5 s` | `AIM_AND_SET` |
| `AIM_AND_SET` | Stone placed at hack `x_T = 0` | Render skip line reticle; track player hand pinch/cup grip on prop | Hand grasps prop and begins forward motion (`v_x > 0.05 m/s`) | `DELIVER_STROKE` |
| `DELIVER_STROKE`| Forward push initiated | Measure physical forward velocity `v_rel_x` and spin `ω_z`; evaluate release line | Hand release detected (`signed_dist > 8 mm`) at `x_T ≤ 140 mm` | `FREE_SLIDE_SWEEP` |
| `DELIVER_STROKE`| Physical stroke exceeds table | Abort delivery if pushed off edge without release | `x_T > 160 mm` while still held | `FAULT_ABORT` |
| `FREE_SLIDE_SWEEP`| Release validated | Integrate virtual ballistics, sample fingertip sweeping strokes, update thermal ice map | Virtual stone stops (`||v_V|| < 0.002 m/s`) OR stone out of bounds | `AT_REST` |
| `AT_REST` | Stone velocity zero | Lock stone resting coordinates, spawn measurement calipers, evaluate rings | Caliper animation complete (`t ≥ 2.0 s`) | `TALLY_SCORES` |
| `TALLY_SCORES`| Resting evaluation complete | Calculate end points, add takeout bonuses, display banner | End < 5: player taps table to proceed; End = 5: match done | End < 5: `NEXT_END_INIT`; End = 5: `MATCH_RESULT` |
| `NEXT_END_INIT`| Next end requested | Clear house stones, advance end index `E ← E + 1`, prompt player to return stone to hack | Physical stone detected at hack (`x_T ≤ 15 mm`) | `END_INTRO` |
| `FAULT_ABORT` | Delivery fault or out-of-bounds | Display "BURNED STONE / FAULT" warning banner, zero score for delivery | Player returns stone to hack (`x_T ≤ 15 mm`) | `AIM_AND_SET` |
| `MATCH_RESULT` | End 5 completed | Render medal podium, display aggregate score, gold/silver/bronze rating | UI button tap "Play Again" | `END_INTRO` (Reset) |

## 10. Physics & Collision

### Collider Interaction Matrix

| Collider Pair | Interaction Type | Friction / Restitution | Response Description |
|---|---|---|---|
| `COL_PROP_BODY` × Real Hand Joints | Penalty contact force | Rigid, non-penetrating | Finger contact detection; records grip and release events |
| `COL_RUNNING_RING` × Table Plane `T` | Kinematic planar clamp | `μ_table = 0.22`, `e = 0.0` | Constrains physical stone to slide flat without tipping |
| `COL_VIRTUAL_ROCK` × Virtual Ice Sheet | Continuous hydrodynamic friction | `μ_ice ∈ [0.012, 0.038]` | Integrated Maeno deceleration and curl equations |
| `COL_VIRTUAL_ROCK` × Opponent Guard Stone | 2D Elastic Rigid Body Impact | `e = 0.88`, tangential friction `μ_col = 0.08` | Momentum exchange: `m₁v₁ + m₂v₂ = m₁v₁' + m₂v₂'`; angular spin transfer |
| `COL_VIRTUAL_ROCK` × House Opponent Stone | 2D Elastic Rigid Body Impact | `e = 0.88`, tangential friction `μ_col = 0.08` | Takeout impact; displaces opponent stone toward back line |
| `COL_SWEEP_INDEX/MID` × Virtual Ice Sheet | Kinematic thermal trigger | Non-colliding trigger | Deposits friction heat `dQ/dt` into local 2D thermal grid |

### Dynamic Constants & Collision Solver

- **Substep Resolution**: 2 substeps per 90 Hz frame (`dt = 5.55 ms`).
- **Swept Volume CCD**: Given stone speed up to 1.2 m/s, maximum displacement per substep is `1.2 · 0.00555 = 6.66 mm`. Stone radius is 40.0 mm; swept cylinder tests prevent tunneling through opponent stones.
- **Inter-Stone Elastic Collision Solver**:
  Let normal vector between stone centers at contact be `n̂ = (p₂ - p₁) / ||p₂ - p₁||`.
  Relative contact velocity: `v_rel = v₁ - v₂`.
  Normal impulse magnitude:
  `J_n = −(1 + e) · (v_rel · n̂) / (1/m₁ + 1/m₂)`
  With `m₁ = m₂ = 0.140 kg` and restitution `e = 0.88`:
  `v₁' = v₁ + (J_n / m₁) · n̂`
  `v₂' = v₂ - (J_n / m₂) · n̂`
  Tangential friction impulse imparts reciprocal spin change:
  `Δω₁ = −(r_stone · J_t) / I_z`, `Δω₂ = −(r_stone · J_t) / I_z`.

## 11. Rendering, Occlusion & Compositing

### Render Layer Ordering

1. **Layer 0 (Passthrough)**: Real table, hands, and room environment.
2. **Layer 1 (Scene Mesh Occlusion)**: Table plane mesh writes depth buffer to prevent virtual elements rendering behind or through the table.
3. **Layer 2 (Curling Sheet Surface)**: Translucent ice shader with scrolling pebble normal maps, hog line, center line, and house rings drawn 1.5 mm above detected table.
4. **Layer 3 (Thermal Sweeping Heatmap)**: Dynamic additive ribbon displaying cyan (−5 °C) to golden-yellow (−0.5 °C) temperature trail left by fingertip sweeps.
5. **Layer 4 (Opponent Stones)**: Virtual granite models with colored handle textures and cast ambient-occlusion contact shadows onto the ice sheet.
6. **Layer 5 (Player Virtual Stone Shadow & Glow)**: Dynamic contact shadow and glide trajectory predictive guide arc.
7. **Layer 6 (Prop Occlusion Proxy)**: Oblate spheroid depth mask rendered at tracked pose `P`. Clips virtual elements passing behind the physical stone.
8. **Layer 7 (Hand Skeletal Mesh Occlusion)**: Hand depth proxy ensuring fingertips occlude the ice sheet and sweeping thermal trails naturally.
9. **Layer 8 (Particle VFX)**: Ice crystal shavings, broom sweep frost dust, and thermal vapor steam.
10. **Layer 9 (World-Space HUD & Reticles)**: Button distance calipers, speed readouts, and skip broom alignment arrows.

### Specific Occlusion Cases

- **Case OC-1 (Fingertips Sweeping Ahead of Virtual Stone)**: Hand skeletal tracking writes depth. When fingers sweep across the virtual sheet, the fingers correctly occlude the virtual ice surface, thermal trail, and any approaching stones.
- **Case OC-2 (Real Prop vs. Virtual Opponent Guard)**: The real prop stops on the physical table at `x_T ≤ 140 mm`. As the virtual stone glides forward into the opponent guard at `x_V = 750 mm`, the opponent guard is rendered in front of the real stone's resting position, avoiding visual spatial clash.
- **Case OC-3 (Table Edge Clipping)**: The virtual curling sheet bounds are clipped by a soft alpha feather mask at the physical table perimeter, preventing virtual ice from floating incongruously in mid-air off the table edge.

### VFX Tris & Particle Budgets

- **Curling Sheet Mesh**: Flat quad with 32-subdivision vertex grid (128 triangles), custom HLSL pebble ice shader.
- **Stone Granite Models**: 1,420 triangles per stone (1 player stone + max 3 opponent stones = 5,680 triangles).
- **Thermal Heatmap Dynamic Mesh**: 240 quads (480 triangles) updated on GPU via compute buffer.
- **Sweeping Frost Dust Particles**: Max 180 concurrent particles (soft circular quads, 0.4 s lifetime).
- **Total Transparent Overdraw**: Peak 1.8× screen fill area.

## 12. Audio & Feedback

| Event Trigger | Spatial Sound Cue | Frequency / Synthesis | Visual Feedback Cue |
|---|---|---|---|
| Stone In-Hand Aiming | Subtle granite rumble | Low-pass filtered 80 Hz drone, gain proportional to table slide speed | Blue directional guide line projects down center sheet |
| Delivery Release Snap | Crisp wood/stone click | 1.8 kHz transient snap, 45 ms duration | Green "RELEASED" flash on delivery line; spin direction arrow |
| Sweeping Stroke Reversal | Granular ice broom scrub | Bandpass noise 800–2400 Hz, volume scaled to `v_lat` | Fan of ice pebble crystal flakes fly outward from fingertips |
| High-Cadence Sweeping (≥5 Hz)| Heated ice sizzle | High-frequency whisper hiss (4.5 kHz), representing melt-film creation | Thermal trail glows bright amber; steam vapor wisps rise from ice |
| Guard Collision Impact | Solid granite clack | Dual-mode impact: 220 Hz low thump + 3.2 kHz granite ring, 120 ms | Radial spark ring + 1.5 mm screen shake impulse |
| Hog Line Violation | Buzzing horn fault | Low brass 110 Hz square wave buzz, 400 ms | Red flashing "HOGGED STONE" banner across sheet |
| Button Entry Glide | Resonant harmonic chord | E-major crystal chime chord (659 Hz, 830 Hz, 987 Hz) | Golden ring pulsation around house button |
| Perfect Draw Rest (≤15 mm) | Triumphant stadium roar | 85 dB coliseum applause + fanfare trumpets | Golden fireworks fountain from center button; +500 PTS banner |
| Tracking Loss Fallback | Muted electronic chime | Descending minor second (440 → 415 Hz) | Holographic amber ghost stone indicating last tracked hack pose |

## 13. Difficulty Progression & Tuning

### 5-End Level Progression Matrix

| End Number | Scenario Archetype | Opponent Stone Layout `(x_V, z_V)` [mm] | Target House Zone | Optimal Delivery Velocity `v_0` | Sweeping Requirement |
|---|---|---|---|---|---|
| End 1 | Open Draw | None (Open Sheet) | Button (`r ≤ 15 mm`) | `0.22 m/s`, light spin (2.5 rad/s) | Gentle sweeping (3–4 Hz) to fine-tune draw distance |
| End 2 | Single Center Guard | Guard at `(700, 0)` | 4-foot ring (`r ≤ 40 mm`) | `0.26 m/s`, heavy out-turn (5.0 rad/s) | Delayed sweeping: coast past guard, then hard sweep right corridor to curl |
| End 3 | Double Takeout | House rocks at `(1030, −25)` and `(1060, +30)` | Clear both opponent rocks | `0.42 m/s` (firm takeout), light in-turn | Continuous high-cadence sweeping (≥5.5 Hz) to maintain straight line |
| End 4 | Narrow Port Draw | Guards at `(650, −35)` and `(680, +35)` (70 mm gap) | Button (`r ≤ 15 mm`) | `0.24 m/s`, moderate spin (3.5 rad/s) | Precision asymmetrical sweeping to thread the 70 mm port without clipping |
| End 5 | Championship Tap-In | Opponent shot stone on button `(1050, 0)` | Tap opponent rock back 30 mm, keep shooter | `0.28 m/s`, heavy in-turn (6.0 rad/s) | Active cadence modulation: heavy sweep early to preserve line, release to bite |

### Adaptive Tuning System

- **Player Skill Metric**: Evaluated at the end of each end via button error: `E_dist = ||p_stone - (1050, 0)||`.
- **Assistance Scaling**:
  - If player fails to reach house in 2 consecutive ends (`E_dist > 120 mm`): Increase ice thermal responsiveness by +25 % (`η_sweep ← 0.52`), widening the allowable sweep window.
  - If player scores perfect button shots in 2 consecutive ends (`E_dist ≤ 15 mm`): Introduce slight pebble degradation (turbulent ice drift, adding ±0.003 N random lateral pebble flutter).

### Named Tuning Constants

```csharp
public static class CurlingTuningConstants
{
    // Prop & Mass Properties
    public const float PROP_MASS_KG = 0.140f;
    public const float EQUATORIAL_SEMI_AXIS_A = 0.040f; // 40 mm
    public const float POLAR_SEMI_AXIS_C = 0.016f;      // 16 mm
    public const float RUNNING_BAND_RADIUS = 0.034f;     // 34 mm
    public const float INERTIA_POLAR_IZ = 8.96e-5f;

    // Kinematic Velocity Mapping
    public const float FORWARD_VELOCITY_GAIN = 4.0f;
    public const float LATERAL_VELOCITY_GAIN = 2.5f;
    public const float MIN_RELEASE_VELOCITY = 0.10f;     // m/s
    public const float MAX_RELEASE_VELOCITY = 1.20f;     // m/s

    // Ice Friction & Curling Constants
    public const float BASE_PEBBLE_FRICTION = 0.038f;
    public const float MIN_SWEPT_FRICTION = 0.012f;
    public const float THERMAL_SENSITIVITY_ALPHA = 0.14f;
    public const float CURL_COUPLING_K = 0.0185f;
    public const float SPIN_DECAY_RATE = 0.045f;

    // Sweeping Input Thresholds
    public const float SWEEP_MIN_LATERAL_SPEED = 0.18f;  // m/s
    public const float SWEEP_TABLE_PROXIMITY_MAX = 0.004f; // 4 mm
    public const float SWEEP_SATURATION_CADENCE = 6.0f;  // Hz
    public const float SWEEP_HEAT_EFFICIENCY = 0.42f;
}
```

## 14. Edge Cases & Failure Recovery

### Edge Case Matrix & Recovery Policies

- **EC-1: Physical Prop Premature Table Edge Slip**:
  - *Symptom*: Player delivers the stone with excessive forward push, and the real stone slides past the physical table front edge (`x_T > 250 mm`).
  - *Detection*: Prop tracking loss while `v_x > 0.20 m/s` near table boundary.
  - *Recovery*: System latches the last valid physical release velocity vector `v_rel` measured prior to boundary crossing and continues the virtual stone flight smoothly down the virtual sheet. A subtle visual warning prompt displays: "Catch your stone before it leaves the table edge."
- **EC-2: Optical Tracking Occlusion During Delivery**:
  - *Symptom*: Player's delivery hand completely covers the stone top handle and marker rim during the forward push.
  - *Detection*: Optical silhouette confidence drops below 0.35 for > 60 ms.
  - *Recovery*: The kinematic solver switches to hand joint dead-reckoning, tracking the palm centroid `R_palm` velocity. When the hand breaks away at release, the stone's trajectory is initialized from the palm breakaway velocity.
- **EC-3: Sweeping Hand Intersects Virtual Stone**:
  - *Symptom*: Over-eager player sweeps too close to the approaching virtual stone, placing fingers directly into the virtual stone collider (`x_sweep_V < x_stone_V + 20 mm`).
  - *Detection*: Sweeping fingertip coordinates overlap the virtual stone cylinder proxy.
  - *Recovery*: The curling rules penalty "Burned Stone from Sweeper Foot/Broom Contact" is triggered. Stone flashes red, audio buzzer sounds, and the stone is immediately removed from play with 0 points.
- **EC-4: Low-Light or Poor Contrast Table Surface**:
  - *Symptom*: Table surface lacks distinctive texture, causing jitter in plane tracking.
  - *Recovery*: System utilizes the headset's point-cloud scene mesh and constrains table height `y_T` to the median plane detected during calibration. Spheroid fitting relies on circular edge contour rather than surface keypoints.
- **EC-5: Player Drops Stone onto Table from Height**:
  - *Symptom*: Prop is picked up and dropped, creating vertical bouncing impacts (`v_y < −0.4 m/s`).
  - *Recovery*: Vertical velocity is clamped; delivery state transitions are locked until the stone sits stationary on the table (`|y_P - 16 mm| < 2 mm`, `||v_P|| < 0.01 m/s`) for at least 0.5 seconds.

## 15. Implementation Architecture

### Class Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────────┐
│                        CurlingGameDirector                             │
│   (End Progression, Match Score, Match State Machine, Rules Engine)    │
└───────────────▲────────────────────────▲────────────────────────▲──────┘
                │                        │                        │
┌───────────────┴────────┐  ┌────────────┴───────────┐  ┌─────────┴──────┐
│  PropTrackerCurling    │  │  FingertipSweeper      │  │ VirtualStone   │
│  - 72 Hz Spheroid Fit  │  │  - Table Proximity Gate│  │   PhysicsSim   │
│  - Handle Pinch Detect │  │  - Cadence Frequency   │  │  - Maeno Curl  │
│  - Release Extractor   │  │  - Dynamic Thermal Map │  │  - Collision   │
└────────────────────────┘  └────────────────────────┘  └────────────────┘
```

### Event Flow

1. `PropTrackerCurling` computes 6-DoF pose `P` from 72 Hz camera frames and fits table plane `T`.
2. `PropTrackerCurling` monitors delivery push; on hand detachment, it raises `OnStoneDelivered(v_0, ω_z)`.
3. `CurlingGameDirector` transitions state from `DELIVER_STROKE` to `FREE_SLIDE_SWEEP` and initializes `VirtualStonePhysicsSim`.
4. `FingertipSweeper` continuously samples `R_index_tip` and `R_middle_tip`, extracts sweeping frequency `f_sweep`, and updates the 2D ice thermal grid `T_ice(x, z)`.
5. `VirtualStonePhysicsSim` samples local `T_ice`, computes instantaneous `μ_k`, applies Maeno curl lateral acceleration, and tests CCD against opponent stones.
6. When velocity falls below 0.002 m/s, `VirtualStonePhysicsSim` fires `OnStoneStopped(finalPosition)`.
7. `CurlingGameDirector` computes button distance, fires audio-visual celebration cues, and advances end index.

### Concrete C# Implementation Scripts

#### Script 1: `FingertipSweeper.cs`

```csharp
using System;
using UnityEngine;

public class FingertipSweeper : MonoBehaviour
{
    [Header("Table Calibration")]
    public float tableSurfaceY = 0.0f;
    public float tableProximityThreshold = 0.004f; // 4 mm

    [Header("Sweeping Metrics")]
    public float currentSweepingCadence = 0.0f; // Hz
    public float iceTemperatureUnderFingers = -5.0f; // deg C
    public bool isSweepingActive = false;

    // Internal tracking state
    private Vector3 previousCentroid;
    private float previousLateralVelocity = 0.0f;
    private float lastReversalTime = 0.0f;
    private float currentStrokePeriod = 0.25f;
    private const float MIN_REVERSAL_SPEED = 0.18f; // m/s

    // Thermal grid constants
    public const float AMBIENT_ICE_TEMP = -5.0f;
    public const float MAX_ICE_TEMP = -0.15f;
    public const float HEAT_CAPACITANCE = 18.5f; // J/(m^2 * C)
    public const float COOLING_CONSTANT = 0.85f; // s^-1

    public void UpdateSweeper(Vector3 rIndexTip, Vector3 rMiddleTip, float dt)
    {
        Vector3 centroid = 0.5f * (rIndexTip + rMiddleTip);
        float distanceToTable = Mathf.Abs(centroid.y - tableSurfaceY);

        if (distanceToTable > tableProximityThreshold)
        {
            isSweepingActive = false;
            currentSweepingCadence = Mathf.MoveTowards(currentSweepingCadence, 0f, dt * 8.0f);
            decayIceTemperature(dt);
            return;
        }

        isSweepingActive = true;
        float vLat = (centroid.z - previousCentroid.z) / Mathf.Max(dt, 0.001f);

        // Zero-crossing reversal detection
        if (Mathf.Sign(vLat) != Mathf.Sign(previousLateralVelocity) && Mathf.Abs(vLat) >= MIN_REVERSAL_SPEED)
        {
            float now = Time.time;
            float strokeDuration = now - lastReversalTime;
            if (strokeDuration > 0.05f && strokeDuration < 0.8f)
            {
                currentStrokePeriod = strokeDuration;
                currentSweepingCadence = 1.0f / (2.0f * currentStrokePeriod);
                currentSweepingCadence = Mathf.Clamp(currentSweepingCadence, 0f, 8.0f);
            }
            lastReversalTime = now;
        }

        // Thermal injection calculation
        float downwardForce = Mathf.Clamp((tableProximityThreshold - distanceToTable) * 350.0f, 0.3f, 1.4f);
        float heatPower = 0.42f * currentSweepingCadence * downwardForce * Mathf.Abs(vLat);
        
        float dTemp = (heatPower / HEAT_CAPACITANCE) * dt - COOLING_CONSTANT * (iceTemperatureUnderFingers - AMBIENT_ICE_TEMP) * dt;
        iceTemperatureUnderFingers = Mathf.Clamp(iceTemperatureUnderFingers + dTemp, AMBIENT_ICE_TEMP, MAX_ICE_TEMP);

        previousCentroid = centroid;
        previousLateralVelocity = vLat;
    }

    private void decayIceTemperature(float dt)
    {
        iceTemperatureUnderFingers = Mathf.MoveTowards(iceTemperatureUnderFingers, AMBIENT_ICE_TEMP, dt * COOLING_CONSTANT * 4.0f);
    }
}
```

#### Script 2: `VirtualStonePhysicsSim.cs`

```csharp
using System;
using UnityEngine;

public class VirtualStonePhysicsSim : MonoBehaviour
{
    [Header("Kinematic State")]
    public Vector3 positionV; // Sheet coordinates (x: down sheet, y: normal, z: lateral)
    public Vector3 velocityV;
    public float spinAngularVelocityZ; // rad/s (positive: out-turn, negative: in-turn)
    public bool isGliding = false;

    [Header("Ice Interaction")]
    public FingertipSweeper sweeper;
    private const float BASE_FRICTION = 0.038f;
    private const float MIN_FRICTION = 0.012f;
    private const float ALPHA_T = 0.14f;
    private const float K_CURL = 0.0185f;
    private const float GRAVITY = 9.81f;
    private const float STONE_MASS = 0.140f;
    private const float CONTACT_RING_RADIUS = 0.034f;
    private const float MOMENT_INERTIA_Z = 8.96e-5f;

    public event Action<Vector3> OnStoneStopped;

    public void LaunchStone(Vector3 initialVelocity, float initialSpin)
    {
        velocityV = initialVelocity;
        spinAngularVelocityZ = initialSpin;
        isGliding = true;
    }

    public void FixedUpdateSim(float dt)
    {
        if (!isGliding) return;

        float currentSpeed = velocityV.magnitude;
        if (currentSpeed < 0.002f)
        {
            velocityV = Vector3.zero;
            spinAngularVelocityZ = 0f;
            isGliding = false;
            OnStoneStopped?.Invoke(positionV);
            return;
        }

        // Query local ice temperature from sweeper
        float localTemp = sweeper.isSweepingActive ? sweeper.iceTemperatureUnderFingers : FingertipSweeper.AMBIENT_ICE_TEMP;

        // Friction calculation
        float tempDelta = localTemp - FingertipSweeper.AMBIENT_ICE_TEMP;
        float muBase = BASE_FRICTION * (1.0f - ALPHA_T * tempDelta);
        float muK = Mathf.Clamp(muBase / (1.0f + 0.45f * Mathf.Sqrt(currentSpeed)), MIN_FRICTION, BASE_FRICTION);

        // Linear drag deceleration
        Vector3 dragAccel = -muK * GRAVITY * velocityV.normalized;

        // Lateral curl acceleration (Maeno asymmetrical friction model)
        // Sweeping reduces friction differential if symmetric, alters curl bite
        float curlMagnitude = Mathf.Sign(spinAngularVelocityZ) * K_CURL * muK * GRAVITY;
        Vector3 curlDirection = Vector3.Cross(Vector3.up, velocityV.normalized);
        Vector3 curlAccel = curlMagnitude * curlDirection;

        // Integrate translational physics
        Vector3 totalAccel = dragAccel + curlAccel;
        velocityV += totalAccel * dt;
        positionV += velocityV * dt;

        // Rotational spin decay
        float spinDecay = (CONTACT_RING_RADIUS / MOMENT_INERTIA_Z) * muK * STONE_MASS * GRAVITY * 0.045f * dt;
        spinAngularVelocityZ = Mathf.MoveTowards(spinAngularVelocityZ, 0f, spinDecay);
    }
}
```

## 16. Acceptance Test Checklist

| Test ID | Quantitative Test Scenario | Target Inputs & Parameters | Expected Pass Criterion |
|---|---|---|---|
| `TC-CUR-01` | Spheroid Registration Accuracy | Place 80.0 × 32.0 mm oblate prop on table at 0.8–1.2 m distance | Pose converges in < 1.2 s; polar axis tilt error < 1.5°; center error < 0.8 mm |
| `TC-CUR-02` | Release Velocity Extraction | Shove prop at physical `v_x = 0.25 m/s`, release at `x_T = 100 mm` | Extracted `v_0_x ∈ [0.95, 1.05] m/s` (4.0× gain ±5 %); release triggered < 40 ms |
| `TC-CUR-03` | Spin Direction Discrimination | Impart clockwise rotation (`ω_z = −4.0 rad/s`) vs CCW (`+4.0 rad/s`) | System correctly tags in-turn vs out-turn with 100 % accuracy over 20 trials |
| `TC-CUR-04` | Sweeping Frequency Tracking | Synthetic 5.0 Hz oscillation of index/middle tips on table | Measured `f_sweep = 5.0 ± 0.3 Hz`; table proximity gate holds 100 % |
| `TC-CUR-05` | Thermal Friction Reduction | Stone launched at `v_0 = 0.25 m/s` with NO sweeping vs 6.0 Hz sweeping | Swept stone glides at least 35 % further than unswept stone (`x_swept ≥ 1.35 · x_unswept`) |
| `TC-CUR-06` | Lateral Curl Trajectory | Launch stone with `ω_z = +4.0 rad/s` (out-turn); measure lateral curl displacement | Total lateral curl `|z_final - z_start| = 85.0 ± 12.0 mm` over 900 mm travel |
| `TC-CUR-07` | Hog-Line Gate Enforcement | Stone launched with weak `v_0 = 0.06 m/s` stopping at `x_V = 360 mm` | System marks stone "HOGGED"; triggers fault buzz; awards 0 score |
| `TC-CUR-08` | Center Button Scoring Accuracy | Place virtual stone at `d_button = 12.0 mm` from center button | Awards exactly 500 Points + "Perfect Draw" bonus banner |
| `TC-CUR-09` | Inter-Stone Elastic Collision | Virtual stone collides head-on with stationary opponent stone at 0.30 m/s | Shooter transfers ≥ 80 % kinetic energy to target stone; restitution `e = 0.88 ± 0.04` |
| `TC-CUR-10` | 5-End Session Duration & Memory Soak | Complete 5 consecutive ends at 90 Hz render | Heap allocation < 1.5 MB/min; zero GC frame spikes > 11.1 ms; session completes in 4.5–6.0 min |

---
