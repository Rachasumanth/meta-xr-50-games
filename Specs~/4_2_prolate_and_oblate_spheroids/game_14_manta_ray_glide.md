# Manta Ray Glide

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Manta Ray Glide |
| Slug | `manta_ray_glide` |
| Physical prop | Oblate computer mouse shell (switched off, optical sensor disabled); 112 × 60 × 36 mm overall hull; mass 95.0 g; fitted contact spheroid `a = b = 32.0 mm`, `c = 18.0 mm` (64 × 64 × 36 mm contact core); matte ABS shell, low-friction PTFE skate pads, dual tactile front button flaps |
| Sub-shape & Family | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate and Oblate Spheroids (oblate, palm-scale, planar sliding with hydrodynamic pitch and bank tilt) |
| Scale class | Lap-tray / tabletop hydrofoil scale; physical sliding patch 140 × 100 mm; virtual abyssal trench 1200 mm long; forward motion gain 3.5×; vertical dive envelope 180 mm depth |
| Source concept quote | "Abyssal giant manta ray. Gliding across flat lap tray; pitching wingtips up/down and banking along major axis catches underwater thermal updrafts to filter-feed bioluminescent plankton swarms." |
| Physical input | Right palm rests flat across the oblate dorsal shell to slide across the lap tray; wrist pitch (`θ_pitch ∈ [−18°, +18°]`) tilts the hydrofoil nose up/down to ascend or dive; lateral palm roll banking (`ϕ_bank ∈ [−25°, +25°]`) steers sweeping banking turns; index and middle fingertips depress left/right buttons to unfurl cephalic filter-feeding fins |
| Session length | 5 abyssal trench zones (waves), 50–70 s active run per zone; total session duration ≈5.5 minutes |
| Lose condition | Manta ray bio-energy reaches 0 % (`E_bio ≤ 0`), ray collides with 3 toxic siphonophore curtain tentacles, or structural collision with volcanic hydrothermal spires drains hull integrity to 0 |

## 1. Fantasy & Core Loop

Deep in the midnight ocean abyss, miles beneath the surface sunlight, you are a colossal pelagic giant manta ray (*Mobula birostris*) soaring in silent grace through a glowing oceanic rift. The dormant computer mouse beneath your right palm is your hydrofoil carriage—its smooth, curved shell matches the exact biomechanical geometry of the manta's cephalic disc and muscular pectoral wings.

Sliding the mouse across your lap tray propels your 6-meter wingspan through the heavy, pressurized deep-sea brine. Unlike an airplane, an abyssal manta navigates a three-dimensional fluid world governed by neutral buoyancy, fluid inertia, and hydrodynamic lift:
- Pitching the mouse forward tilts your cephalic disc downward into steep dives, slicing toward rich abyssal sediment beds; tilting the mouse back pitches your wingtips upward, catching rising hydrothermal thermal updrafts that propel you effortlessly toward canyon ceilings.
- Rolling your palm to the left or right banks your enormous pectoral wings into sweeping, banked oceanic turns, banking lift vectors to trace winding trench walls.
- Depressing the mouse's left and right buttons uncurls your twin cephalic horn fins, funneling sparkling clouds of bioluminescent dinoflagellates and krill straight into your filter-feeding gill plates to replenish your dwindling bio-energy reserves!

### 30–60s Active Gliding Cycle

1. **Abyssal Trench Ingress (0.0–2.0 s)**: The ray glides into a new oceanic sector. Forward water current rushes past; sonar HUD highlights thermocline layers and distant glowing plankton ribbons.
2. **Planar Cruise & Clutching (2.0–6.0 s)**: Palm pushes the mouse smoothly forward across the 140 mm lap tray patch. A 3.5× forward motion gain translates physical hand glide into swift virtual cruising (`v_virtual ∈ [0.45, 1.20] m/s`). When reaching the patch border, the player executes an ergonomic clutch (lifting palm 15 mm, repositioning to center).
3. **Hydrofoil Bank & Roll Trim (6.0–12.0 s)**: Approaching a sharp canyon dogleg, the player banks the mouse along its major roll axis (`ϕ_bank = −22°`). The virtual ray banks gracefully, deflecting hydrodynamic lift laterally to execute a tight 180 mm radius turn without losing forward airspeed.
4. **Thermal Updraft Catching (12.0–16.0 s)**: A glowing cyan hydrothermal chimney erupts from the canyon floor. Player tilts the mouse nose up (`θ_pitch = +14°`), centering the ray directly above the thermal vent. The warm buoyancy plume injects a +0.85 m/s vertical lift impulse, rocketing the ray upward into high-density plankton strata while saving glide stamina.
5. **Cephalic Filter-Feeding Scoop (16.0–22.0 s)**: Entering a dense swarm of sparkling electric-blue krill, the player depresses both index and middle button flaps. Cephalic flaps unfurl, expanding the scoop intake aperture from 30 mm to 90 mm; plankton sparkles rush into the ray's mouth, surging bio-energy (+30 %) and activating the Bioluminescent Overdrive bar.
6. **Hazard Evasion & Barrel Roll (22.0–28.0 s)**: A curtain of toxic mauve siphonophores drifts across the channel. Player rapidly double-clicks the right button while rolling the mouse hard right (`|ω_roll| > 4.5 rad/s`), executing an acrobatic barrel roll that ducks under the stinging tentacles.
7. **Abyssal Gate Transition (28.0–35.0 s)**: Ray sails through an ancient bioluminescent coral archway; trench depth increments, banking bonus points into the session tally.

### 5-Wave Oceanic Depth Progression

- **Zone 1 (Epipelagic Drop: 200 m)**: Bright twilight waters, gentle horizontal currents. Wide open canyon. Tutorial for pitch climbs, dive sweeps, and dual-button filter-feeding.
- **Zone 2 (Mesopelagic Canyon: 600 m)**: Twisting volcanic rock walls. Introduces thermal updraft vents; requires 20° banked turns to navigate doglegs.
- **Zone 3 (Bathypelagic Chimneys: 1200 m)**: Pitch-black trench illuminated only by glowing volcanic black smokers. Dense krill ribbons flanked by jagged basalt spires; requires precision vertical altitude trim.
- **Zone 4 (Abyssopelagic Curtain: 2500 m)**: Drifting walls of stinging siphonophores and bioluminescent jellyfish curtains. Demands timed evasive barrel rolls and rapid clutching.
- **Zone 5 (Hadal Rift Finale: 6000 m)**: Roaring tectonic fault trench. Massive vertical updraft geysers; giant abyssal leviathan glides overhead; player must sustain continuous high-altitude filter-feeding loops to clear the final trench gate.

## 2. Platform Baseline

- **Hardware**: Standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No physical controllers; the only tangible prop is the registered mouse shell.
- **Joint Naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc, mcp, pip, dip, tip (thumb: cmc, mcp, ip, tip). Example: `R_palm`, `R_index_tip`, `R_middle_tip`.
- **Prop Registration**: At session start, the player sets the mouse on the lap tray; vision pipeline detects the table plane `T`, fits the 64 × 64 × 36 mm contact spheroid core within the 112 mm mouse silhouette at 72 Hz, and computes 6-DoF pose `P`.
- **Coordinate Frames**:
  - `W`: Room world frame (Y up, cm/meters).
  - `T`: Support surface frame (detected lap tray or table; origin at tray center, `+ŷ_T` normal pointing up, `+x̂_T` pointing forward down-trench, `+ẑ_T = x̂_T × ŷ_T` pointing to player's right).
  - `P`: Prop local frame (origin at center of fitted contact spheroid; `+x̂_P` longitudinal axis pointing toward mouse front cord end, `+ŷ_P` pointing out top dorsal dome, `+ẑ_P` pointing to player's right).
  - `V`: Virtual play frame (abyssal trench coordinate system, anchored to `T` at the trench entrance).
- **Contact Detection**: Joint contact detected when signed distance from hand joints to the mouse's contact spheroid collider is < 5.0 mm; release hysteresis 8.0 mm; debounce threshold 30 ms.
- **Physics Engine**: Fixed 90 Hz timestep, max 2 sub-steps (11.1 ms / 5.55 ms); hydrofoil lift and drag equations integrated with 6-DoF rigid-body aerodynamics.
- **Rendering & Compositing**: Passthrough base layer; volumetric underwater fog and caustics; glowing bioluminescent plankton particle compute buffer; skeletal hand mesh depth occlusion.
- **Feedback**: Multi-channel spatial audio (deep ocean hydrophone drone, rushing fluid glide whoosh, sonar pings, krill sparkle chime) and visual VFX (wingtip vortices, cavitation bubbles, plankton ingestion ribbons).

### 2.1 Game-Specific Overrides

- **Prop Frame `P` & Geometry**: The prop is an ergonomic mouse shell fitted with an oblate contact spheroid core: equatorial semi-axes `a = b = 32.0 mm` (64.0 mm equatorial diameter), polar semi-axis `c = 18.0 mm` (36.0 mm vertical height). The elongated front nose (button flaps) and rear palm taper extend outside the contact spheroid into a 112 mm visual hull.
- **Hydrofoil Kinematics & Tilt Model**:
  - Longitudinal Pitch (`θ_pitch`): Wrist flexion/extension tilts `+x̂_P` relative to the tray plane `T`:
    `θ_pitch = arcsin(clamp(x̂_P · ŷ_T, −1.0, 1.0))`.
    Pitch angle directly modulates hydrodynamic angle of attack `α_hydro`.
  - Lateral Bank Roll (`ϕ_bank`): Palm pronation/supination rolls the prop around its long axis `x̂_P`:
    `ϕ_bank = arctan2(ẑ_P · ŷ_T, ŷ_P · ŷ_T)`.
    Bank roll directs the lift vector `L` laterally, inducing coordinated hydrodynamic yaw turns:
    `dψ_yaw / dt = (g / v_forward) · tan(ϕ_bank)`.
- **Virtual Frame `V` Anchoring**: `V` is anchored to `T` at the center of the lap tray. Forward trench axis extends along `+x̂_V` for 1200 mm; trench vertical depth envelope spans `y_V ∈ [−100, +80] mm` relative to tray height.
- **Clutching & Forward Gain**:
  - Physical hand translation on the 140 mm lap tray maps to virtual trench position via motion gain `3.5×`:
    `p_V(t) = 3.5 · (p_P(t) - p_tray_origin) + c_offset`.
  - When the physical mouse reaches `|x_P - x_tray_origin| > 55 mm`, the system triggers an automatic clutch: during hand repositioning, the ray glides forward under fluid momentum (`a_glide = −c_drag · v²`), and `c_offset` smoothly absorbs the reposition displacement.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Play Posture | Seated with forearm comfortably supported on lap tray desk or table | Stabilizes wrist during fine pitch and roll tilt adjustments; prevents shoulder strain |
| Physical Sliding Patch | 140 mm long × 100 mm wide flat mousepad surface | Matches natural comfortable forearm sweep without requiring trunk flexion |
| Pitch Tilt Range | `θ_pitch ∈ [−18°, +18°]` (36° total range) | Stays well within safe human wrist flexion/extension limits (nominal wrist range is ±35°) |
| Bank Roll Range | `ϕ_bank ∈ [−25°, +25°]` (50° total range) | Natural forearm pronation/supination without ulnar or radial joint impingement |
| Button Actuation Force | 0.6–1.2 N downward index/middle pressure | Standard mouse microswitch tactile click; provides crisp physical feedback for cephalic scoop |
| Clutch Re-Centering | Every 3.5–5.0 seconds (150 ms lift-and-slide) | Prevents wrist from drifting into extreme extension; keeps hand centered in optimal camera FOV |
| Rest Intervals | 15 s calm thermocline glide between trench zones | Allows complete release of hand grip and wrist stretching |
| Eye-to-Prop Distance | 350–450 mm, 35° downward gaze | Optimal for headset depth camera tracking of fingertip button contacts |

### Fatigue Mitigations
1. **Neutral Forearm Support**: Forearm rests flat on the lap tray, taking 100 % of arm weight off the shoulder.
2. **PTFE Low-Friction Skates**: Mouse skates glide with kinetic friction `μ_k = 0.08`, requiring less than 0.12 N of forward pushing force.
3. **Clutch Assisted Gliding**: The fluid dynamic model includes substantial mass momentum (95 g virtual body with `c_drag = 0.22`), allowing the ray to coast up to 45 mm on virtual inertia during gentle hand rests.

## 4. Prop Geometry & Registration

```
                           +ŷ_P (Polar Minor Axis, c = 18.0 mm)
                                     ▲
                            .--------│--------.
                         .-'         │         '-.    Dorsal Shell (Palm Rest)
                       .'            │            '.
     ◄────────────────┼──────────────┼──────────────┼────────────────►
     -a = 32.0 mm     |              │              |   +a = 32.0 mm (Equator)
     (Lateral ẑ_P)     \             │             /    Fitted Core Ø = 64.0 mm
                        '-.          │          .-'     PTFE Skate Base
                           '---------│---------'
                                     ▼
                      -x̂_P (Tail) ◄─────► +x̂_P (Mouth / Nose 112 mm)
```

### Prop Dimensions & Mass Properties

- **Fitted Contact Spheroid Core**:
  - Equatorial Semi-Axes (Lateral / Planar): `a = b = 32.0 mm` (equatorial diameter = 64.0 mm).
  - Polar Semi-Axis (Vertical Height): `c = 18.0 mm` (vertical thickness = 36.0 mm).
  - Oblate Ratio: `c / a = 18.0 / 32.0 = 0.5625`.
  - Core Volume: `V_core = (4/3) · π · 32 · 32 · 18 = 77,208 mm³ ≈ 77.21 cm³`.
- **Overall Outer Hull**: 112.0 mm (length) × 60.0 mm (width) × 36.0 mm (height).
- **Prop Mass**: `m = 95.0 g` (0.095 kg, including internal ballast cell for authentic oceanic inertia).
- **Moments of Inertia**:
  - Polar Axis (Vertical `ŷ_P`):
    `I_y = (2/5) · m · a² = 0.4 · 0.095 kg · (0.032 m)² = 3.891 × 10⁻⁵ kg·m²`.
  - Transverse Pitch/Roll Axes (`x̂_P`, `ẑ_P`):
    `I_x = I_z = (1/5) · m · (a² + c²) = 0.2 · 0.095 kg · (0.032² + 0.018²) m² = 2.561 × 10⁻⁵ kg·m²`.

### Surface-Feature Atlas

| Feature ID | Coordinate Bounds in `P` | Physical Texture / Mechanism | Functional Gameplay Utility |
|---|---|---|---|
| `F1: LEFT_BUTTON_FLAP` | `x ∈ [30, 56], z ∈ [−28, −4] mm` | Matte PBT plastic, tactile microswitch click | Left cephalic horn scoop activation |
| `F2: RIGHT_BUTTON_FLAP`| `x ∈ [30, 56], z ∈ [+4, +28] mm` | Matte PBT plastic, tactile microswitch click | Right cephalic horn scoop activation |
| `F3: DORSAL_CREST` | `x ∈ [−25, +25], y ≥ +14 mm` | Smooth rubberized arch matching thenar pad | Primary palm contact and pitch/bank orientation sensor |
| `F4: LEFT_WING_FLANK` | `z ∈ [−30, −25], y ∈ [0, +16] mm` | Contoured thumb groove | Thumb grip; lateral roll stabilization |
| `F5: RIGHT_WING_FLANK`| `z ∈ [+25, +30], y ∈ [0, +16] mm` | Contoured pinky rest | Ring/pinky grip; lateral roll stabilization |
| `F6: PTFE_SKATE_PADS` | Base plane `y = −18 mm` (4 pads) | Virgin PTFE teflon glide pads | Low-friction planar table gliding (`μ_k = 0.08`) |

### Collider Inventory

| Collider ID | Type | Dimensions | Coordinate Frame | Role |
|---|---|---|---|---|
| `COL_SPHEROID_CORE` | Oblate Spheroid | `a = b = 32.0 mm`, `c = 18.0 mm` | Prop Frame `P` | Primary depth occluder, hand palm contact, and table baseline proxy |
| `COL_HULL_EXTENT` | Box / Convex Hull | `112 × 60 × 36 mm` | Prop Frame `P` | Physical clearance hull; excluded from depth-proxy to prevent clipping |
| `COL_BUTTON_L` | Box | `26 × 24 × 8 mm` | Tracked `F1` in `P` | Left index fingertip contact trigger for cephalic fin |
| `COL_BUTTON_R` | Box | `26 × 24 × 8 mm` | Tracked `F2` in `P` | Right middle fingertip contact trigger for cephalic fin |
| `COL_RAY_WINGS` | Swept Hydrofoil Mesh| 180 mm wingspan, 90 mm chord | Virtual Ray Frame `V` | Hydrodynamic lift, updraft collision, and siphonophore overlap detector |
| `COL_CEPHALIC_SCOOP`| Frustum Funnel | Mouth 30–90 mm, depth 40 mm | Virtual Ray Bow `V` | Ingestion volume for bioluminescent krill particles |

### Optical Registration & Pose Fitting

1. **Table Plane Calibrator**: Detects lap tray plane `T` (`y_T = 0`) with normal error < 0.8°.
2. **Mouse Contour Matcher**: Vision extracts the high-contrast 112 × 60 mm mouse silhouette at 72 Hz. An oriented bounding box identifies forward axis `x̂_P`.
3. **Spheroid Core Inscription**: The 64 × 36 mm contact spheroid is anchored to the geometric centroid of the mouse hull.
4. **Pitch/Roll Pose Refinement**: Levenberg-Marquardt solver determines pitch tilt `θ_pitch` and bank roll `ϕ_bank` with angular precision < 1.0° and RMS position residual < 0.55 mm.

## 5. Hand Tracking Input Map

| ID | Gesture / Input | Active Joints | Surface Region | Semantic Gameplay Output |
|---|---|---|---|---|
| `IN_PALM_GLIDE` | Planar Palm Translation | `R_palm`, `R_wrist`, `R_heel` | Dorsal Crest (`F3`) | Drives forward/lateral cruising velocity `v_virtual`; engages 3.5× motion gain |
| `IN_PITCH_TRIM` | Wrist Flexion / Extension | `R_wrist`, `R_mcp_joints` | Dorsal Crest (`F3`) tilt | Pitches nose up/down (`θ_pitch ∈ [−18°, +18°]`); commands climb / dive |
| `IN_BANK_ROLL` | Forearm Pronation / Supination| `R_thumb_tip`, `R_pinky_tip` | Left/Right Flanks (`F4/F5`) | Banks wings (`ϕ_bank ∈ [−25°, +25°]`); steers lateral turn rate |
| `IN_CEPHALIC_L` | Left Button Click / Hold | `R_index_tip` pad (`F > 0.6 N`)| Left Button Flap (`F1`) | Unfurls left cephalic fin (+30 mm scoop width); steers leftward krill inflow |
| `IN_CEPHALIC_R` | Right Button Click / Hold | `R_middle_tip` pad (`F > 0.6 N`)| Right Button Flap (`F2`) | Unfurls right cephalic fin (+30 mm scoop width); steers rightward krill inflow |
| `IN_DUAL_SCOOP` | Both Buttons Depressed | `R_index_tip` + `R_middle_tip` | Both Flaps (`F1` + `F2`) | Full Cephalic Funnel (90 mm intake); 2.0× krill ingestion rate; +15 % drag |
| `IN_BARREL_ROLL` | Rapid Double-Click + Hard Roll| `R_middle_tip` double tap + `|ϕ| > 20°`| Right Flap + Flank | Triggers evasive 360° barrel roll (invulnerable for 0.75 s) |
| `IN_CLUTCH_LIFT` | Palm Lift > 15 mm | All right hand joints | Off prop surface > 120 ms | Clutches input; ray coasts on momentum while hand re-centers on tray |

## 6. Input Kinematics & Signal Processing

```
72 Hz Hand Tracking ────► OneEuro Filter ────► Pitch / Roll Extractor ──► Hydrodynamic Lift
[R_wrist, R_palm]         (fc=1.2, β=0.02)      (θ_pitch, ϕ_bank)          & Turn Solver
                                                                                 │
72 Hz Prop Pose P ──────► Planar Motion Gain ──► Clutch State Machine ───────────┴──► Virtual Ray
(x_P, z_P, ψ_P)           Gain = 3.5×            Offset Integration                   Trajectory
```

### Hydrodynamic Flight Dynamics Model

The virtual manta ray's motion integrates three-dimensional hydrodynamics:

1. **Angle of Attack (`α_hydro`)**:
   `α_hydro = θ_pitch - arctan2(v_y, v_forward)`.
2. **Hydrodynamic Lift Force (`L`)**:
   `L = 0.5 · ρ_water · v_forward² · S_wing · C_L(α_hydro)`
   Where:
   - Seawater density `ρ_water = 1025 kg/m³` (scaled for tabletop simulation).
   - Effective wing surface area `S_wing = 0.018 m²`.
   - Lift coefficient: `C_L(α) = 2.0 · π · α_hydro` (for `|α| ≤ 16°`, stalling beyond 18°).
3. **Hydrodynamic Drag Force (`D`)**:
   `D = 0.5 · ρ_water · v_forward² · S_wing · [C_D0 + (C_L² / (π · e_aspect · AR))]`
   Where parasite drag `C_D0 = 0.045`, aspect ratio `AR = 2.8`, efficiency `e_aspect = 0.85`.
   Activating the cephalic scoop (`IN_DUAL_SCOOP`) adds `ΔC_D = +0.035` induced drag.
4. **Coordinated Turn Yawing**:
   Banking the mouse along `ϕ_bank` tilts the lift vector, producing lateral turning acceleration:
   `a_lateral = (L / m_virtual) · sin(ϕ_bank)`.
   Yaw turn rate:
   `dψ_yaw / dt = (g / max(v_forward, 0.2)) · tan(ϕ_bank)` (clamped to `±45°/s`).
5. **Thermal Updraft Injection**:
   When entering a hydrothermal plume column (radius `r_vent = 45 mm`), vertical convective force applies:
   `F_updraft = k_vent · (1.0 - r / r_vent) · [1.0 + 0.5 · sin(θ_pitch)] · ŷ_V`
   Injecting vertical climb rates up to `+0.85 m/s`.

### Motion Gain, Dead Zones & Clutching

1. **Planar Translation**:
   Physical mouse displacement on lap tray:
   `Δp_P = p_P(t) - p_P(t - dt)`.
   Virtual velocity contribution:
   `v_forward = clamp(3.5 · Δp_P.x / dt, 0.15 m/s, 1.40 m/s)`.
   A dead zone of 4.0 mm/s suppresses resting hand tremors.
2. **Clutch Integration**:
   When `||p_P - p_tray_center|| > 55.0 mm` on either axis, the system enters `CLUTCH_ACTIVE`:
   - Physical translation is uncoupled for 150 ms.
   - Virtual ray continues coasting along current heading with natural drag deceleration:
     `v_virtual(t + dt) = v_virtual(t) · exp(−c_drag · dt)`.
   - Re-centering the mouse to within 20 mm of tray center re-engages direct driving with a smooth 100 ms linear blend.

## 7. Virtual World Layout & Coordinate Mapping

```
                                VIRTUAL TRENCH FRAME (V)
                   ◄── Abyssal Trench Axis Length 1200 mm (+x_V) ──►
┌──────────────────────────────────────────────────────────────────────────┐
│ Mesopelagic Ceiling (y_V = +80 mm) - Bioluminescent Plankton Ribbon      │
│         ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦  ✦   │
│                                                                          │
│                     ┌──────────────────────────┐                         │
│   [Canyon Wall]     │ Virtual Manta Ray Glider │     [Canyon Wall]       │
│                     │      (Wingspan 180 mm)   │                         │
│                     └────────────┬─────────────┘                         │
│                                  │                                       │
│   (Hydrothermal Vent)            ▼            (Siphonophore Curtain)     │
│       ▲   ▲   ▲            [Ray Cruise]              │  │  │  │          │
│       │   │   │                                      │  │  │  │          │
├───────┴───┴───┴──────────────────────────────────────┴──┴──┴──┴──────────┤
│ Abyssal Floor (y_V = -100 mm) - Basalt Canyon Spires & Sediment Beds     │
└──────────────────────────────────────────────────────────────────────────┘
```

### Coordinate Transforms

- **`P_to_T`**: `p_T = R_P_to_T · p_P + p_P_in_T`.
- **`T_to_V`**: Trench entrance origin is locked to lap tray center: `origin_V = origin_T`.
- **Vertical Altitude Mapping**: Physical mouse pitch `θ_pitch` integrates into vertical virtual altitude `y_V`:
  `y_V(t) = y_V(t_0) + ∫ (v_forward · sin(θ_pitch) + v_updraft) dt`.
  Clamped strictly to `y_V ∈ [−100, +80] mm` (trench ceiling and floor).

### Virtual Entity Inventory

| Entity Name | Frame | Coordinates / Bounds | Dimensions | Visual Description & Behavior |
|---|---|---|---|---|
| Abyssal Trench Walls | `V` | `x ∈ [0, 1200], z = ±160 mm` | 1200 × 320 mm mesh | Basalt rock canyon textured with glowing bioluminescent lichen |
| Virtual Giant Manta | `V` | Dynamic `p_V(t)` | Wingspan 180 mm, length 120 mm | Midnight-blue dorsal disc with white ventral belly and undulating wingtips |
| Cephalic Horn Lobes | `V` | Muzzle flanks of ray | 2 lobes, 25 mm long each | Curled horns in cruise; uncurled flexible scoops when buttons pressed |
| Hydrothermal Vents | `V` | Spawns along trench floor | Radius 45 mm plume, height 120 mm| Glowing cyan thermal updraft chimney billowing hot mineral particles |
| Plankton Cloud Swarms| `V` | Stratified ribbons at `y = +40 mm`| 80 × 50 mm clouds | Sparkling electric-blue dinoflagellates and krill; food source |
| Siphonophore Curtains| `V` | Suspended from canyon ceiling | 140 mm long trailing tentacles | Translucent mauve stinging jellyfish colony; hazardous obstacle |
| Coral Archway Gate | `V` | `x = 1200 mm` | Arch 220 mm wide, 160 mm high | Glowing bioluminescent deep-sea coral gate; zone transition exit |

## 8. Gameplay Systems & Rules

### Bio-Energy Dynamics & Scoring

1. **Manta Bio-Energy Bar (`E_bio ∈ [0, 100] %`)**:
   - Initial Energy: 80.0 %.
   - Baseline Gliding Consumption: `−1.4 %/s`.
   - Cephalic Scoop Active Drain: `−0.8 %/s` (additional hydrodynamic drag).
   - Food Nutritional Intake:
     - Individual Dinoflagellate Spark: **+0.15 % Energy**, +5 Points.
     - Dense Krill Swarm Ingestion: **+25.0 % Energy**, +400 Points.
     - Hydrothermal Vent Lift Capture: **+10.0 % Energy** (thermal conservation bonus).

2. **Bioluminescent Overdrive**:
   When `E_bio ≥ 90 %`, the ray enters "Overdrive":
   - Wingtips glow radiant cyan.
   - Forward cruising speed increases by +35 % without additional energy drain.
   - Point multiplier doubles to **2.0×** for 8.0 seconds.

3. **Damage & Hazards**:
   - Siphonophore Tentacle Contact: Deals **−20.0 % Bio-Energy**, causes ray flinch (controls sluggish for 1.2 s), and deducts 150 points.
   - Basalt Spire Collision: Deals **−30.0 % Bio-Energy**, knocks ray back 40 mm, and halts forward momentum.

4. **Loss Conditions**:
   - `E_bio ≤ 0 %` (ray exhausts metabolic energy and stalls out in the abyss).
   - 3 Siphonophore collisions in a single trench zone.

5. **Victory Criteria**:
   Successfully traverse all 5 oceanic zones and pass through the final Hadal Rift Arch with `E_bio > 0 %`.

## 9. Game State Machine

```
┌─────────┐      ┌──────────────┐      ┌────────────┐      ┌─────────────┐
│  BOOT   ├─────►│  REGISTER_P  ├─────►│ TRAY_SET   ├─────►│ ZONE_INTRO  │
└─────────┘      └──────────────┘      └────────────┘      └──────┬──────┘
                                                                  │
┌──────────────┐      ┌──────────────┐      ┌───────────┐         │
│ HADAL_VICTORY│◄─────┤ ZONE_CLEAR   │◄─────┤ GLIDE_    │◄────────┘
└──────────────┘      └──────┬───────┘      │ ACTIVE    │
                             │              └─────┬─────┘
                             ▼                    │
                      ┌──────────────┐            ▼
                      │ NEXT_ZONE    │     ┌─────────────┐
                      └──────────────┘     │ FAIL_EXHAUST│
                                           └─────────────┘
```

### State Machine Transition Table

| State Name | Entry Conditions | Continuous Operations | Exit Guard / Transition Condition | Next State |
|---|---|---|---|---|
| `BOOT` | Application start | Load oceanic shaders, compile plankton particle system, start hydrophone audio | Tracking ready | `REGISTER_P` |
| `REGISTER_P` | Mouse placed on tray | Fit 64 × 36 mm contact spheroid core; identify mouse nose heading | Spheroid fitted (`RMS < 0.55 mm`) | `TRAY_SET` |
| `TRAY_SET` | Prop registered | Calibrate lap tray boundaries; display floating sonar HUD | Tray confirmed; hand rests on mouse | `ZONE_INTRO` |
| `ZONE_INTRO` | Zone start | Announce trench name (e.g. "Zone 2: Mesopelagic Canyon"); camera swoops to ray | Intro timer `t ≥ 3.0 s` | `GLIDE_ACTIVE` |
| `GLIDE_ACTIVE`| Zone active | Integrate hydrodynamics, sample pitch/bank tilt, spawn plankton and vents | Ray reaches gate `x_V ≥ 1200 mm` AND `E_bio > 0` | `ZONE_CLEAR` |
| `GLIDE_ACTIVE`| Energy exhausted | Play dying heart tone; ray sinks into seabed sediments | `E_bio ≤ 0 %` | `FAIL_EXHAUST` |
| `GLIDE_ACTIVE`| Lap edge reached | Freeze virtual translation; display clutch chevron | `||p_P - p_center|| > 55 mm` | `CLUTCH_ACTIVE` |
| `CLUTCH_ACTIVE`| Clutch triggered | Coast ray on inertia; blend camera offset to new physical mouse anchor | Mouse re-centered (`||p_P - p_center|| < 20 mm`) | `GLIDE_ACTIVE` |
| `ZONE_CLEAR` | Gate cleared | Tally zone score, play triumphant deep-sea chord, award Bio-Crest | Player holds mouse level 1.5 s | Zone < 5: `NEXT_ZONE`; Zone = 5: `HADAL_VICTORY` |
| `NEXT_ZONE` | Zone score tallied | Advance zone index `Z ← Z + 1`; increase water pressure and hazard density | Ray aligned at new trench entrance | `ZONE_INTRO` |
| `FAIL_EXHAUST`| Metabolic exhaustion | Display "ENERGY DEPLETED / RETURN TO SHALLOWS"; offer restart | Player taps restart shell icon | `ZONE_INTRO` |
| `HADAL_VICTORY`| Zone 5 cleared | Grand abyss eruption; pod of giant manta rays joins in synchronized glide | Player glides freely | `SESSION_END` |

## 10. Physics & Collision

### Collider Interaction Matrix

| Collider Pair | Interaction Type | Physics Parameters | Gameplay Response |
|---|---|---|---|
| `COL_RAY_WINGS` × Updraft Plume | Hydrodynamic Velocity Field | Vertical impulse `+0.85 m/s`, duration 2.0 s | Rocket climbs ray toward ceiling; awards vent bonus |
| `COL_CEPHALIC_SCOOP` × Plankton Cloud | Particle Ingestion Trigger | Intake cross-section 90 mm, collection radius 45 mm | Ingests plankton particles; replenishes `E_bio` |
| `COL_RAY_WINGS` × Siphonophore | Hazard Damage Trigger | Restitution `e = 0.05`, damage 20 % | Stings ray; screen flashes purple; control sluggishness |
| `COL_RAY_WINGS` × Basalt Spire | Elastic Hull Collision | Restitution `e = 0.35`, rebound speed 0.4 m/s | Bounces ray off rock wall; deals 30 % damage |
| `COL_SPHEROID_CORE` × Lap Tray Base | Planar Sliding Contact | Kinetic friction `μ_k = 0.08`, static friction `μ_s = 0.14` | Translates physical hand push into forward cruise |

### Hydrodynamic Solver Details

- **Time Step**: 90 Hz fixed timestep with 2 sub-steps (`dt = 5.55 ms`).
- **Coordinated Flight Numerical Integration**:
  ```
  v_forward += (F_thrust - Drag) / m_virtual * dt
  v_y += (Lift * cos(bank) + F_updraft - Gravity) / m_virtual * dt
  heading_yaw += (g / v_forward) * tan(bank) * dt
  position_V += Quaternion.Euler(pitch, heading_yaw, bank) * (Vector3.forward * v_forward + Vector3.up * v_y) * dt
  ```

## 11. Rendering, Occlusion & Compositing

### Abyssal Deep-Water Atmosphere

1. **Layer 0 (Passthrough)**: Real lap tray, room lighting, and player's physical arm.
2. **Layer 1 (Volumetric Deep-Sea Haze)**: Deep oceanic fog with exponential distance falloff (visibility 450 mm), tinting passthrough into deep abyssal blue.
3. **Layer 2 (Trench Architecture & Spires)**: Heavy basalt walls and hydrothermal chimneys rendered with normal-mapped rocky textures and emissive vent cracks.
4. **Layer 3 (Bioluminescent Plankton Particles)**: GPU compute buffer running 2,000 sparkling dinoflagellate point lights with pulsating soft-glow halos.
5. **Layer 4 (Virtual Giant Manta Model)**: Rigged 3D manta ray model with undulating vertex-shader wing flapping synchronized to cruising speed.
6. **Layer 5 (Prop Depth Occluder)**: Spheroid core depth mask (`COL_SPHEROID_CORE`) rendered at tracked mouse pose `P`, preventing underwater virtual entities from rendering through the physical mouse.
7. **Layer 6 (Hand Skeletal Mesh Occlusion)**: Hand depth proxy ensures player's palm and fingers occlude the virtual manta cockpit and plankton ribbons naturally.
8. **Layer 7 (Underwater HUD & Sonar)**: Holographic depth gauge, energy bar, and forward obstacle sonar ping ring.

### VFX Triangles & Particle Budgets

- **Manta Ray Model**: 4,200 triangles with 18-bone wing armature for realistic aquatic flapping.
- **Trench Canyon Walls**: 2,800 triangles per segment (streamed procedurally).
- **Hydrothermal Vent Plumes**: 180 continuous smoke-puff quad particles with additive cyan shading.
- **Bioluminescent Krill Buffer**: 2,000 GPU instanced points (zero CPU draw call overhead).
- **Total Transparent Overdraw**: Peak 2.1× screen area.

## 12. Audio & Feedback

| Event Trigger | Spatial Sound Cue | Frequency / Synthesis Profile | Visual Cue |
|---|---|---|---|
| Forward Planar Glide | Deep water flow whoosh | Pink noise filtered at 180–480 Hz, gain proportional to `v_forward` | Microscopic cavitation bubbles stream from wingtips |
| Wingtip Pitch Climb | Rising fluid hiss | Bandpass sweep 220 Hz → 680 Hz | Ray wings flex upward; seabed caustics stretch |
| Banked Roll Turn | Resonant oceanic groan | Low cello drone (92 Hz) + water eddy swirl | Wingtip vapor vortex ribbons spiral from trailing edges |
| Cephalic Scoop Depress | Pneumatic flap click | Crisp 1.4 kHz mechanical click + wet suction slurp | Cephalic horns uncurl into wide funnel; mouth lights up |
| Plankton Ingestion | Sparkling crystal chimes | Pentatonic glass harp arpeggios (880 Hz, 1175 Hz, 1318 Hz) | Electric-blue sparkles stream into mouth; energy bar surges |
| Thermal Vent Updraft Catch | Roaring geothermal surge | Sub-bass 45 Hz rumble + ascending steam hiss | Glowing cyan light column envelops ray; high-speed vertical streaks |
| Siphonophore Tentacle Hit | Toxic electric zap | Distorted 3.2 kHz saw wave buzz + wet sizzle | Mauve lightning arcs across wings; screen flashes toxic purple |
| Basalt Wall Impact | Heavy underwater thud | 55 Hz acoustic boom + rock crumble clatter | Screen shake impulse (2.5 mm); sediment cloud billows |
| Overdrive Activation | Triumphant sonar ping | 1.8 kHz pure crystal ping with 1.2 s reverb tail | Entire dorsal disc pulses radiant cyan; speed trails ignite |

## 13. Difficulty Progression & Tuning

### 5-Zone Oceanic Progression Matrix

| Zone | Trench Depth | Current Speed `v_current` | Canyon Width | Plankton Swarm Density | Hazard Profile | Target Energy Reserve |
|---|---|---|---|---|---|---|
| Zone 1: Epipelagic | 200 m | 0.0 m/s | 320 mm (Wide) | Dense (Easy Intake) | None (Tutorial) | End with `E_bio ≥ 80 %` |
| Zone 2: Mesopelagic | 600 m | 0.25 m/s | 260 mm | Medium | 2 Thermal Vents, Dogleg Turns | End with `E_bio ≥ 70 %` |
| Zone 3: Bathypelagic | 1200 m | 0.40 m/s | 200 mm | Clustered near Vents | 4 Vents, Basalt Spires | End with `E_bio ≥ 65 %` |
| Zone 4: Abyssopelagic| 2500 m | 0.55 m/s | 170 mm | Thin Ribbons | Siphonophore Tentacle Curtains| End with `E_bio ≥ 55 %` |
| Zone 5: Hadal Rift | 6000 m | 0.70 m/s | 140 mm (Tight)| Fast-Moving Swarms | Tectonic Geysers + Siphonophores| End with `E_bio ≥ 50 %` |

### Adaptive Balancing Systems

- **Thermal Compass Rescue**: If `E_bio < 25 %`, an emergency hydrothermal vent spawns 150 mm ahead on the canyon floor, marked by a bright cyan guidance beacon.
- **Overdrive Extension**: Flawless flight through a siphonophore gate without taking damage awards a +4.0 s extension to Bioluminescent Overdrive.

### Named Tuning Constants

```csharp
public static class MantaTuningConstants
{
    // Prop Dimensions & Core
    public const float SPHEROID_SEMI_AXIS_A = 0.032f; // 32 mm
    public const float SPHEROID_SEMI_AXIS_C = 0.018f; // 18 mm
    public const float MOUSE_MASS_KG = 0.095f;        // 95 g
    public const float MOTION_GAIN_FORWARD = 3.5f;

    // Aerodynamic / Hydrodynamic Parameters
    public const float SEAWATER_DENSITY = 1025.0f;    // kg/m^3
    public const float WING_SURFACE_AREA = 0.018f;    // m^2
    public const float BASE_PARASITE_DRAG = 0.045f;
    public const float SCOOP_INDUCED_DRAG = 0.035f;
    public const float MAX_CLIMB_RATE_MS = 0.85f;

    // Control Bounds
    public const float MAX_PITCH_DEG = 18.0f;
    public const float MAX_BANK_DEG = 25.0f;
    public const float CLUTCH_THRESHOLD_MM = 55.0f;
    public const float CLUTCH_RETURN_MM = 20.0f;
}
```

## 14. Edge Cases & Failure Recovery

### Failure Edge Cases & Mitigation Handlers

- **EC-MAN-01: Mouse Slips Past Lap Tray Outer Rim**:
  - *Symptom*: Player pushes forward too vigorously, sliding the mouse over the lap tray edge into mid-air.
  - *Detection*: Prop tracking detects sharp downward acceleration (`a_y < −4.0 m/s²`) or tracking loss near tray boundary.
  - *Recovery*: Virtual flight enters "Emergency Submersible Hover": forward translation is held steady on momentum, ray levels off horizontally, and an amber holographic prompt appears: "Place mouse back on tray to resume glide."
- **EC-MAN-02: Palm Completely Covers Left and Right Buttons**:
  - *Symptom*: Player grips mouse with a low palm heel, resting fingers flat across both buttons without intending to activate the cephalic scoop.
  - *Detection*: Large contact area registered across both button flaps without a downward actuation velocity trigger.
  - *Recovery*: Button clicks require a sharp microswitch depression velocity threshold (`v_tip_y < −0.08 m/s`) and release debounce; resting palm contact is filtered out as passive resting weight.
- **EC-MAN-03: Sudden Lighting Change in Dim Room**:
  - *Symptom*: Abyssal game visuals darken the user's room passthrough, degrading optical edge tracking of the matte black mouse.
  - *Detection*: Vision confidence score drops below 0.35.
  - *Recovery*: System fuses hand skeletal palm position with the last reliable mouse offset, maintaining stable relative tracking while increasing camera gain dynamically.
- **EC-MAN-04: Extreme Roll Over-Banking (Inverted Flight)**:
  - *Symptom*: Player rolls wrist past 45°, threatening to flip the virtual ray upside down.
  - *Recovery*: The hydrodynamic solver clamps physical input banking to `±25.0°`, preventing inverted disorientation while preserving full steering authority.

## 15. Implementation Architecture

### Class Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────────┐
│                        MantaGameDirector                               │
│      (Zone Progression, Bio-Energy Bar, Scoring, Hazard Spawner)       │
└───────────────▲────────────────────────▲────────────────────────▲──────┘
                │                        │                        │
┌───────────────┴────────┐  ┌────────────┴───────────┐  ┌─────────┴──────┐
│  PropTrackerManta      │  │  HydrodynamicsFlight  │  │ CephalicScoop  │
│  - 72 Hz Spheroid Core │  │  - Pitch/Bank Resolver │  │    System      │
│  - Wrist Pitch & Bank  │  │  - Updraft Convection  │  │ - Plankton Eat │
│  - Clutch State Engine │  │  - Coordinated Turning │  │ - Overdrive FSM│
└────────────────────────┘  └────────────────────────┘  └────────────────┘
```

### Concrete C# Implementation Scripts

#### Script 1: `HydrodynamicsFlight.cs`

```csharp
using System;
using UnityEngine;

public class HydrodynamicsFlight : MonoBehaviour
{
    [Header("Flight Dynamics State")]
    public Vector3 virtualPosition;
    public Vector3 virtualVelocity;
    public float currentPitchDeg = 0.0f;
    public float currentBankDeg = 0.0f;
    public float currentYawDeg = 0.0f;

    [Header("Cephalic Drag")]
    public bool isScoopActive = false;

    // Constants
    private const float FORWARD_GAIN = 3.5f;
    private const float GRAVITY = 9.81f;
    private const float WING_AREA = 0.018f;
    private const float SEAWATER_DENSITY = 1025.0f;
    private const float BASE_DRAG = 0.045f;
    private const float SCOOP_DRAG = 0.035f;

    public void StepFlightDynamics(Vector2 physicalMouseDelta, float targetPitch, float targetBank, float updraftLiftForce, float dt)
    {
        // Smooth input angles
        currentPitchDeg = Mathf.Clamp(targetPitch, -18.0f, 18.0f);
        currentBankDeg = Mathf.Clamp(targetBank, -25.0f, 25.0f);

        // Forward cruise speed derived from mouse translation
        float handSpeed = physicalMouseDelta.magnitude / Mathf.Max(dt, 0.001f);
        float targetForwardSpeed = Mathf.Clamp(handSpeed * FORWARD_GAIN, 0.25f, 1.40f);
        virtualVelocity.z = Mathf.MoveTowards(virtualVelocity.z, targetForwardSpeed, dt * 2.5f);

        // Hydrodynamic turn: banking tilts lift vector
        float bankRad = currentBankDeg * Mathf.Deg2Rad;
        float yawRateDeg = (GRAVITY / Mathf.Max(virtualVelocity.z, 0.2f)) * Mathf.Tan(bankRad) * Mathf.Rad2Deg;
        currentYawDeg += yawRateDeg * dt;

        // Vertical climb / dive
        float pitchRad = currentPitchDeg * Mathf.Deg2Rad;
        float climbSpeed = virtualVelocity.z * Mathf.Sin(pitchRad) + updraftLiftForce;
        virtualVelocity.y = Mathf.MoveTowards(virtualVelocity.y, climbSpeed, dt * 3.0f);

        // Drag integration
        float activeDragCoeff = BASE_DRAG + (isScoopActive ? SCOOP_DRAG : 0.0f);
        virtualVelocity.z -= 0.5f * activeDragCoeff * virtualVelocity.z * virtualVelocity.z * dt;

        // Position integration
        Quaternion orientation = Quaternion.Euler(-currentPitchDeg, currentYawDeg, -currentBankDeg);
        Vector3 forwardStep = orientation * new Vector3(0f, virtualVelocity.y, virtualVelocity.z) * dt;
        virtualPosition += forwardStep;

        // Clamp to vertical trench bounds
        virtualPosition.y = Mathf.Clamp(virtualPosition.y, -0.100f, 0.080f);
    }
}
```

#### Script 2: `CephalicScoopSystem.cs`

```csharp
using System;
using UnityEngine;

public class CephalicScoopSystem : MonoBehaviour
{
    [Header("Energy & Overdrive")]
    public float bioEnergyPercent = 80.0f; // [0, 100]
    public bool isOverdriveActive = false;
    public float overdriveTimer = 0.0f;

    [Header("Scoop Intake Width")]
    public float currentIntakeWidthMm = 30.0f; // 30 mm idle, 90 mm both buttons

    public event Action OnEnergyExhausted;
    public event Action OnOverdriveEngaged;

    public void UpdateScoopState(bool leftButtonPressed, bool rightButtonPressed, int collectedPlanktonCount, float dt)
    {
        // Determine scoop aperture
        if (leftButtonPressed && rightButtonPressed)
        {
            currentIntakeWidthMm = Mathf.MoveTowards(currentIntakeWidthMm, 90.0f, dt * 300.0f);
        }
        else if (leftButtonPressed || rightButtonPressed)
        {
            currentIntakeWidthMm = Mathf.MoveTowards(currentIntakeWidthMm, 60.0f, dt * 300.0f);
        }
        else
        {
            currentIntakeWidthMm = Mathf.MoveTowards(currentIntakeWidthMm, 30.0f, dt * 200.0f);
        }

        // Metabolic consumption
        float drainRate = 1.4f + (currentIntakeWidthMm > 40.0f ? 0.8f : 0.0f);
        if (isOverdriveActive) drainRate *= 0.5f;

        bioEnergyPercent -= drainRate * dt;

        // Nutrition intake
        if (collectedPlanktonCount > 0)
        {
            bioEnergyPercent += collectedPlanktonCount * 0.15f;
        }
        bioEnergyPercent = Mathf.Clamp(bioEnergyPercent, 0f, 100f);

        // Check Overdrive Trigger
        if (bioEnergyPercent >= 90.0f && !isOverdriveActive)
        {
            isOverdriveActive = true;
            overdriveTimer = 8.0f;
            OnOverdriveEngaged?.Invoke();
        }

        if (isOverdriveActive)
        {
            overdriveTimer -= dt;
            if (overdriveTimer <= 0f)
            {
                isOverdriveActive = false;
            }
        }

        if (bioEnergyPercent <= 0f)
        {
            OnEnergyExhausted?.Invoke();
        }
    }
}
```

## 16. Acceptance Test Checklist

| Test ID | Test Scenario & Target Verification | Quantitative Pass Threshold |
|---|---|---|
| `TC-MAN-01` | Oblate Mouse Spheroid Core Fitting | Present 112 × 60 × 36 mm mouse prop on tray at 0.40 m distance | Core fit converges < 1.0 s; `a = 32.0 ± 0.8 mm`, `c = 18.0 ± 0.6 mm` |
| `TC-MAN-02` | Pitch Tilt Control Linearity | Tilt mouse pitch `θ_pitch = +15.0°`; measure virtual climb angle | Virtual flight climb angle equals `15.0 ± 1.2°`; zero vertical jitter |
| `TC-MAN-03` | Coordinated Banked Turn Rate | Roll mouse `ϕ_bank = +20.0°` at forward speed `v = 0.80 m/s` | Yaw turning rate `dψ/dt = 25.8 ± 2.0 °/s`; radius of turn `178 ± 15 mm` |
| `TC-MAN-04` | Dual-Button Cephalic Flap Actuation | Depress left and right button flaps simultaneously | Scoop intake expands from 30 mm to 90 mm in < 200 ms; induced drag applies |
| `TC-MAN-05` | Plankton Ingestion Nutritional Yield | Cruise through 100-particle plankton cloud with dual scoop active | Ingestion captures ≥ 85 % of particles; bio-energy increases by `+12.8 ± 1.0 %` |
| `TC-MAN-06` | Thermal Updraft Lift Impulse | Fly directly over 45 mm hydrothermal chimney at `v = 0.60 m/s` | Vertical lift impulse injects `+0.85 ± 0.08 m/s`; climb altitude `+55 ± 8 mm` |
| `TC-MAN-07` | Automatic Clutch Boundary Trigger | Translate mouse > 55.0 mm from tray center | System enters `CLUTCH_ACTIVE`; ray coasts on momentum; zero positional teleportation |
| `TC-MAN-08` | Barrel Roll Evasion Invulnerability | Execute rapid double-click + hard 20° roll through siphonophore curtain | Barrel roll executes in 0.75 s; ray passes through tentacles with 0 damage |
| `TC-MAN-09` | Bioluminescent Overdrive Threshold | Increase bio-energy to 90.0 % | Overdrive activates for 8.0 ± 0.2 s; forward cruising speed surges by +35 % |
| `TC-MAN-10` | 5-Zone Complete Ocean Soak Test | Complete full 5-zone trench session (5.5 minutes) at 90 Hz render | Zero frame drops below 86 Hz; memory allocation < 1.6 MB/min; stable tracking |

---
