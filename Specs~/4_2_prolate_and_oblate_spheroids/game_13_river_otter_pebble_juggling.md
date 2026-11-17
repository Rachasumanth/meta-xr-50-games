# River Otter Pebble Juggling

## 0. Identity Block

| Field | Value |
|---|---|
| Title | River Otter Pebble Juggling |
| Slug | `river_otter_pebble_juggling` |
| Physical prop | Oblate smooth river pebble (dense natural basalt or polished river granite); equatorial semi-axes `a = b = 35.0 mm` (70.0 mm outer diameter), polar semi-axis `c = 12.0 mm` (24.0 mm polar thickness); mass 65.0 g; silky-smooth water-tumbled matte texture |
| Sub-shape & Family | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate and Oblate Spheroids (oblate, handheld bimanual palm juggling, rolling, and impact smashing) |
| Scale class | Lap-tray / bimanual chest scale; juggling transfer span 180 mm; physical strike zone 120 × 80 mm; virtual river width 380 mm, rapids length 1000 mm |
| Source concept quote | "Playful river otter cracking shells. Flipping and sliding the river stone across palms to juggle river rocks, cracking clams and catching jumping trout before river rapids sweep them away." |
| Physical input | Bimanual palm-to-palm sliding, rolling across digital phalanges, two-handed stone shuttling, downward flat-face shell smashing strikes against lap/table anvil, and quick edge flicks to volley jumping trout |
| Session length | 5 rapids runs (waves), 35–50 s each; total session duration ≈4.0 minutes |
| Lose condition | Otter hunger energy drops to 0 % (`E_hunger ≤ 0`), pebble slips and is lost in river rapids for > 4.0 s, or player crushes 3 toxic spine-urchins |

## 1. Fantasy & Core Loop

You are a buoyant, playful North American river otter (*Lontra canadensis*) floating on your back down a sun-dappled mountain river canyon. In otter culture, every self-respecting river otter possesses a single treasured possession: a perfectly flat, water-smoothed river pebble kept safely in a loose skin pouch under the left forearm. This oblate basalt stone is both your musical plaything and your essential survival tool.

As the crystal mountain stream rushes beneath your fur, swirling eddies wash up delicious river bounty: thick-shelled freshwater clams, snapping crayfish, spiny river urchins, and shimmering silver trout leaping through the spray. Floating on your back, your chest and lap serve as your dining table and anvil:
- You flip, slide, and shuttle your favorite stone between your left and right paws in mesmerizing figure-eight juggling routines.
- When a armored clam floats into your chest eddy, you clamp the bivalve securely against your belly anvil and smash downward with the flat polar face of your pebble to shatter the calcified shell and feast on the succulent meat.
- When silver trout leap high out of river rapids, you flick the rim of the pebble upward in a glancing volley strike, swatting the fish straight into your waiting whiskers!

### 30–60s Active Gameplay Cycle

1. **Rapids Drift & Drift Read (0.0–1.5 s)**: Floating downriver, water rapids rush past in MR passthrough. Upstream shadows reveal approaching river bounty (clams, trout, crabs) and hazards (toxic spiny urchins, jagged river driftwood).
2. **Bimanual Palm Shuttling & Trick Charge (1.5–5.0 s)**: Player slides and rolls the oblate pebble back and forth between cupped left and right palms (`v_shuttle ∈ [0.15, 0.40] m/s`). Rolling the stone across palm calluses builds the "Otter Agility Combo" multiplier (up to 4.0×) and charges kinetic smash potential.
3. **Clam Clamp & Shell Smash (5.0–9.0 s)**: A heavy river mussel floats onto the chest anvil (`x_V, z_V`). Non-dominant hand pins the clam virtual proxy against the lap tray. Dominant hand raises the oblate pebble 50–100 mm and drives downward with flat-face impact (`v_strike > 0.28 m/s`, `F_strike > 1.2 N`). The shell fractures with an acoustic crunch, awarding clam meat and restoring hunger energy (+25 %).
4. **Jumping Trout Volley Flick (9.0–13.0 s)**: A silver trout leaps in a parabolic arc 150 mm above the lap. Player rotates the pebble onto its thin equatorial rim (`c/a = 0.343`) and flicks upward (`v_y > 0.35 m/s`) to bat the fish mid-air directly into the otter's snout.
5. **Hazard Evasion / Urchin Dodge (13.0–16.0 s)**: A black spiny urchin drifts into striking range. Striking an urchin penalizes hunger (−30 %) and dulls the stone; player must shuttle the stone across to the opposite palm and sweep the urchin harmlessly into the river current.
6. **Rapids Eddy Rest & Feast (16.0–20.0 s)**: Run clears through a calm river pool; swallowed clam shells sink to river pebbles, hunger meter updates, and the next rapid wave roars ahead.

### 5-Wave Rapids Progression

- **Wave 1 (Whispering Shallows)**: Gentle current (river speed 0.25 m/s). 4 soft-shell river clams. Focus: Calibrated downward smashing and two-handed palm transfers.
- **Wave 2 (Trout Leap Canyon)**: Swifter current (0.35 m/s). 3 clams + 4 jumping silver trout leaping from alternating left/right banks. Introduces upward rim flick volleys.
- **Wave 3 (Boulder Chute & Crayfish)**: Fast chute (0.45 m/s). Armor-plated snapping crayfish requiring two consecutive precision smashes; first appearance of drifting river urchins.
- **Wave 4 (White-Water Rapids)**: Violent spray (0.60 m/s). Rapid bimanual juggling required to keep agility multiplier active while dodging floating driftwood branches and cracking heavy river mussels.
- **Wave 5 (Waterfall Plunge & Golden Sturgeon)**: Climax run. Player must juggle the stone continuously while descending a roaring cataract, smashing a giant golden river bivalve (requires 3 heavy smashes in rhythm) and catching the legendary jumping King Trout for the championship feast.

## 2. Platform Baseline

- **Hardware**: Standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No physical controllers; the only tangible prop is the registered oblate river pebble.
- **Joint Naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc, mcp, pip, dip, tip (thumb: cmc, mcp, ip, tip). Example: `L_palm`, `R_palm`, `R_index_tip`.
- **Prop Registration**: At session start, the player holds the oblate stone in either palm; the optical pipeline detects the water-tumbled basalt silhouette, fits an oblate spheroid of revolution (`a = b = 35.0 mm`, `c = 12.0 mm`) at 72 Hz, and computes 6-DoF pose `P`.
- **Coordinate Frames**:
  - `W`: Room world frame (Y up, cm/meters).
  - `T`: Support surface frame (detected lap tray or table plane; origin at player sternum projection, `+ŷ_T` surface normal pointing up, `+x̂_T` river downstream forward, `+ẑ_T = x̂_T × ŷ_T` across river to right).
  - `P`: Prop local frame (origin at pebble center; `+ẑ_P` minor polar axis through flat thickness, `+x̂_P` and `+ŷ_P` in circular equatorial plane).
  - `V`: Virtual play frame (river current coordinate system, anchored to `T` at the otter belly anvil).
- **Contact Detection**: Joint contact detected when signed distance from hand joint spheres to the pebble's oblate collider is < 4.0 mm; release hysteresis 7.0 mm; debounce threshold 25 ms.
- **Physics Engine**: Fixed 90 Hz timestep, max 2 sub-steps (11.1 ms / 5.55 ms); continuous collision detection (CCD) for high-speed strike impacts (`v > 0.6 m/s`).
- **Rendering & Compositing**: Passthrough base layer; dynamic river water simulation rendered over lap tray; translucent river splash VFX; 3D clams with fracture meshes; hand skeletal mesh depth occlusion.
- **Feedback**: Multi-channel spatial audio (wet water slaps, calcium shell fracture cracks, fish splash flops, playful otter chirps) and visual VFX (water droplets, shell fragments, sparkling pearl drops).

### 2.1 Game-Specific Overrides

- **Prop Frame `P` & Geometry**: Oblate spheroid with equatorial semi-axes `a = b = 35.0 mm` (equatorial diameter = 70.0 mm) and polar semi-axis `c = 12.0 mm` (axial thickness = 24.0 mm). Flatness ratio `c/a = 12.0 / 35.0 = 0.3429` (distinctly flattened disc-like pebble).
- **Kinematic Rolling & Impact Dynamics**:
  - When sliding across the palm, the pebble rests on its flat polar face (`|ẑ_P · ŷ_T| ≈ 1.0`). Polar radius of curvature is `R_pole = a²/c = 35.0²/12.0 = 102.08 mm`, providing a very stable, flat contact patch that glides smoothly across wet palm tissue.
  - When tilted onto its equator (`|ẑ_P · ŷ_T| ≈ 0.0`), the rim radius of curvature sharpens to `R_rim = c²/a = 12.0²/35.0 = 4.114 mm`, enabling snappy edge flicks and agile rolls across fingertips.
  - Downward strikes deliver impulsive normal force:
    `J_strike = ∫ F_n dt = m · Δv_y = 0.065 kg · (v_impact - 0)`.
- **Virtual Frame `V` Anchoring**: `V` is anchored to `T` at the center of the lap tray / table anvil. Origin `(0, 0, 0)_V` represents the center of the otter's chest. Upstream river current flows from `-x̂_V` toward `+x̂_V`.
- **Specific Deviations**:
  - The physical stone impacts a real lap tray, book, or table surface serving as the physical anvil, providing true tactile impact deceleration and acoustic feedback, while the virtual clam renders directly beneath the pebble.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Play Posture | Seated comfortably leaning slightly back (15°–20° reclined) with lap tray or low table | Mimics natural otter floating-on-back posture; positions lap as comfortable anvil |
| Physical Anvil Surface | Lap tray desk or table patch (300 × 200 mm) | Provides stable rigid or padded base for stone strikes without risk of bruising thighs |
| Palm Juggling Span | `z_T ∈ [−90, +90] mm` (180 mm total transfer width) | Natural bimanual shuttling envelope without elbow flare or shoulder abduction |
| Strike Elevation Height | 40–100 mm above anvil surface | Sufficient to generate required kinetic impact velocity (`v > 0.28 m/s`) with minimal joint strain |
| Impact Force Range | 0.8–2.2 N downward strike force | Well within safe human palm strike tolerances; felt or foam tray pad absorbs shock |
| Rest Breaks | 10 s calm water drift between rapids waves | Allows finger flexor relaxation and hand drying/shaking |
| Neck Ergonomics | 30° downward head tilt toward lap | Comfortable for Quest 3 / Vision Pro passthrough camera field-of-view |

### Fatigue Mitigations
1. **Lightweight Basalt Prop (65 g)**: Optimized mass provides satisfying tactile solidity without inducing wrist fatigue during repetitive shuttling.
2. **Compliant Anvil Contact**: The software calibrates to a 3 mm foam mousepad or lap-tray cushion, dissipating high-frequency impact shocks.
3. **Agility Multiplier Retention**: Shuttling rhythm does not require frantic speed; 1.0–1.8 Hz palm transfers maintain maximum combo, rewarding smooth rhythm over aggressive thrashing.

## 4. Prop Geometry & Registration

```
                           +ẑ_P (Minor Polar Axis, c = 12.0 mm)
                                     ▲
                           .---------│---------.
                        .-'          │          '-.   Polar Face (R_pole = 102.1 mm)
                      .'             │             '. Flat Anvil Striking Surface
                     /               │               \
     ◄──────────────(────────────────┼────────────────)──────────────►
     -a = 35.0 mm    \               │               /  +a = 35.0 mm (Equator)
     (+x̂_P / +ŷ_P)    '.             │             .'   Rim Ø = 70.0 mm
                        '-.          │          .-'     Rim Radius R_rim = 4.11 mm
                           '---------│---------'
                                     ▼
```

### Prop Dimensions & Mass Properties

- **Equatorial Semi-Axes**: `a = b = 35.0 mm` (equatorial diameter = 70.0 mm).
- **Polar Semi-Axis**: `c = 12.0 mm` (polar thickness = 24.0 mm).
- **Oblate Eccentricity**: `e = √(1 - c²/a²) = √(1 - 12²/35²) = √(1 - 0.1175) = √0.8825 ≈ 0.9394`.
- **Prop Mass**: `m = 65.0 g` (0.065 kg).
- **Volume**: `V = (4/3) · π · a · b · c = (4/3) · π · 35 · 35 · 12 = 61,575 mm³ ≈ 61.58 cm³`.
- **Mean Density**: `ρ = 65.0 g / 61.58 cm³ ≈ 1.056 g/cm³` (fine-grained river basalt / sedimentary siltstone).
- **Moments of Inertia**:
  - Spin Axis (Polar Normal `ẑ_P`):
    `I_z = (2/5) · m · a² = 0.4 · 0.065 kg · (0.035 m)² = 3.185 × 10⁻⁵ kg·m²`.
  - Transverse Pitch/Roll Axes (`x̂_P`, `ŷ_P`):
    `I_x = I_y = (1/5) · m · (a² + c²) = 0.2 · 0.065 kg · (0.035² + 0.012²) m² = 1.777 × 10⁻⁵ kg·m²`.

### Surface-Feature Atlas

| Feature ID | Latitude `φ` | Azimuth `λ` | Curvature `R_eff` | Description & Gameplay Utility |
|---|---|---|---|---|
| `F1: DORSAL_FACE` | +55° to +90° | 0° to 360° | `102.1 mm` | Smooth upper flat polar cap; primary clam smashing surface and palm rest |
| `F2: VENTRAL_FACE`| −90° to −55° | 0° to 360° | `102.1 mm` | Smooth lower flat polar cap; secondary resting face on lap anvil |
| `F3: EQUATOR_RIM` | −20° to +20° | 0° to 360° | `4.11 mm` | Thin rounded perimeter rim; agile fingertip rolling track and trout volley edge |
| `F4: TRANSITION_BEVEL`| +20° to +55°| 0° to 360° | `18.5 mm` | Rounded bevel shoulder; allows seamless finger roll transitions between face and rim |

### Collider Inventory

| Collider ID | Type | Dimensions | Parent Frame | Role |
|---|---|---|---|---|
| `COL_PEBBLE_CORE`| Oblate Spheroid | `a = b = 35.0 mm`, `c = 12.0 mm` | Prop Frame `P` | Hand tracking contact, physical table collision, occluder proxy |
| `COL_STRIKE_FACE` | Cylinder | `r = 30.0 mm`, `h = 4.0 mm` | Offset `(0, 0, +10 mm)` in `P` | Downward shell smash impact detector |
| `COL_VOLLEY_RIM` | Torus | Major `R = 34.0 mm`, minor `r = 2.0 mm` | Origin `(0, 0, 0)` in `P` | Upward edge flick trigger against leaping fish |
| `COL_PALM_L` | Ellipsoid | `45 × 35 × 18 mm` | Tracked `L_palm` | Left otter paw receiving cup |
| `COL_PALM_R` | Ellipsoid | `45 × 35 × 18 mm` | Tracked `R_palm` | Right otter paw receiving cup |
| `COL_CLAM_SHELL` | Convex Hull | `42 × 36 × 22 mm` | River Frame `V` | Target clam shell proxy resting on chest anvil |

### Optical Registration & Pose Fitting

1. **Dual-Palm Basalt Detection**: System segments the matte-gray 70 mm circular/elliptical silhouette resting in either palm at 72 Hz.
2. **Oblate Aspect Ratio Verification**: The projected minor-to-major axis ratio `c_proj / a_proj` confirms the flat pebble geometry (`0.34 ≤ ratio ≤ 1.0`).
3. **Levenberg-Marquardt Fit**: Optimizes 6-DoF pose `P`. Acceptance threshold: RMS residual < 0.60 mm, surface distance < 1.0 mm.
4. **Fast Re-Acquisition**: When shuttling rapidly between hands, the tracking pipeline maintains a 120 mm search sphere around the predicted inter-palm trajectory, relocking in < 25 ms upon hand transfer.

## 5. Hand Tracking Input Map

| ID | Gesture / Action | Active Joints | Surface Region | Semantic Gameplay Output |
|---|---|---|---|---|
| `IN_PALM_SHUTTLE` | Bimanual Lateral Transfer | `L_palm`, `R_palm`, `L/R_fingers` | Pebble Ventral/Dorsal Faces | Juggling shuttle event; increments Agility Combo (+0.25× per pass) |
| `IN_CLAM_CLAMP` | Single-Hand Pinning | `L_palm` or `L_fingers` | River Clam Proxy on Anvil | Clamps floating clam securely to lap anvil; arms clam for smashing |
| `IN_SHELL_SMASH` | Downward Kinetic Strike | `R_palm` driving pebble | `COL_STRIKE_FACE` onto Clam | Clam shell smash impact; shatters bivalve if `E_strike ≥ 0.045 J` |
| `IN_TROUT_FLICK` | Upward Rim Volley | `R_index_tip`, `R_thumb_tip` flick | `COL_VOLLEY_RIM` upward | Bats airborne leaping trout into mouth (+150 pts, +15 % hunger) |
| `IN_PALM_ROLL` | Digital Phalange Roll | `R_thumb_pad` to `R_pinky_pad` | `COL_PEBBLE_CORE` equator | Rolling trick; charges kinetic energy glow on pebble |
| `IN_URCHIN_SWEEP` | Lateral Paw Brush | `L_palm` or `R_palm` open sweep | Water surface 60 mm ahead of anvil| Sweeps toxic spiny urchin away downriver without striking |
| `IN_POUCH_STASH` | Clench to Left Armpit | `L_wrist`, `L_elbow` flexion | Pebble pressed to upper chest | Stashes pebble in otter pouch; emergency hold during turbulent rapids |
| `IN_TABLE_REST` | Both Hands Open on Lap | `L_palm`, `R_palm` resting flat | Lap tray plane | Passive drift; hunger drains at baseline rate |

## 6. Input Kinematics & Signal Processing

```
72 Hz Hand Tracking ────► OneEuro Filter ────► Palm Transfer Classifier ──► Agility Combo
[L_palm, R_palm]          (fc=2.0, β=0.04)      Signed Distance < 8 mm         Multiplier
                                                                                    │
72 Hz Pebble Pose P ────► Impact Derivative ──► Shell Fracture Solver ◄─────────────┘
(p_P, v_P, a_P)           E_k = 0.5·m·v²        Threshold E_crit = 0.045 J
```

### Juggling Shuttling & Transfer Classification

The system monitors signed distance from pebble center `p_P` to the left palm center `p_palm_L` and right palm center `p_palm_R`:

1. **Palm Ownership State**:
   - `Owner = LEFT` if `||p_P - p_palm_L|| < 35 mm` and `signed_dist(p_P, palm_plane_L) < 15 mm`.
   - `Owner = RIGHT` if `||p_P - p_palm_R|| < 35 mm` and `signed_dist(p_P, palm_plane_R) < 15 mm`.
   - `Owner = FLIGHT / TRANSIT` if between palms.
2. **Shuttle Event Trigger**:
   A valid shuttle is logged when `Owner` transitions `LEFT → FLIGHT → RIGHT` or `RIGHT → FLIGHT → LEFT` within a time window `Δt_transfer ∈ [0.12, 0.65] s`.
   Each valid shuttle increments the Agility Multiplier:
   `Multiplier(t + 1) = clamp(Multiplier(t) + 0.25, 1.0, 4.0)`.
   If the stone sits stationary in one palm for > 2.5 seconds, the multiplier decays at `−0.5 s⁻¹`.

### Shell Smashing Fracture Mechanics

During a downward strike, the pebble moves toward the lap tray anvil (`y_T = 0`):

1. **Impact Velocity Vector**:
   Finite difference over a 3-frame window prior to contact:
   `v_strike = ||v_P(t_contact)|| = ||(p_P(t) - p_P(t - 3Δt)) / (3Δt)||`.
2. **Kinetic Energy at Impact**:
   `E_impact = 0.5 · m_pebble · v_strike² = 0.5 · (0.065 kg) · v_strike²`.
3. **Flat-Face Alignment Penalty**:
   A glancing blow delivers less crushing energy than a flat polar strike:
   `AlignmentFactor = |ẑ_P · ŷ_T|` (where `1.0` is perfectly flat, `0.0` is purely on edge).
   `E_effective = E_impact · AlignmentFactor · Multiplier`.
4. **Fracture Threshold Criteria**:
   - Soft River Clam: `E_crit = 0.025 J` (requires `v_strike ≥ 0.28 m/s` at 1.0× multiplier).
   - Hard River Mussel: `E_crit = 0.045 J` (requires `v_strike ≥ 0.37 m/s` at 1.0×, or `v_strike ≥ 0.22 m/s` at 3.0× multiplier).
   - Armored Crayfish Carapace: `E_crit = 0.075 J` (requires two sequential smashes within 1.2 s).
   - If `E_effective ≥ E_crit`: Clam shell shatters into procedural fragments; meat is freed!
   - If `E_effective < E_crit`: Shell reflects pebble elastically with a hollow dull tap; shell HP reduced by `E_effective / E_crit`.

### Trout Volley Kinematics

Leaping trout follow a parabolic ballistic trajectory across the virtual lap:
`p_trout(t) = p_spawn + v_0 · t + 0.5 · g · t²`.

1. **Rim Volley Detection**:
   When the pebble rim collider `COL_VOLLEY_RIM` intersects the trout proxy:
   - Upward velocity condition: `v_P · ŷ_T ≥ 0.30 m/s`.
   - Rim orientation condition: `|ẑ_P · ŷ_T| ≤ 0.45` (pebble must be held on edge).
2. **Deflection Ballistics**:
   Trout velocity is reflected toward the otter's mouth `p_snout_V`:
   `v_trout_deflect = k_volley · normalize(p_snout_V - p_contact) · 1.8 m/s`.

## 7. Virtual World Layout & Coordinate Mapping

```
                                VIRTUAL RIVER FRAME (V)
                   ◄── Water Rapids Downstream Flow (+x_V) ──►
┌──────────────────────────────────────────────────────────────────────────┐
│ Upstream Shallows (-x_V = -500 mm)                                       │
│    [Clam Spawns]         [Leaping Trout Parabola]       [Urchin Drift]   │
│         ▼                          ╭──╮                        ▼         │
│         ▼                         │    │                       ▼         │
├───────────────────────────────────┼────┼─────────────────────────────────┤
│ LAP ANVIL ZONE (x_V ∈ [-60, +60], z_V ∈ [-90, +90] mm)                   │
│                                                                          │
│       ┌──────────────┐                       ┌──────────────┐            │
│       │ Left Paw Cup │     [Clam on Anvil]   │Right Paw Cup │            │
│       │   (L_palm)   │         (●●●)         │   (R_palm)   │            │
│       └──────┬───────┘                       └───────┬──────┘            │
│              │        ◄── Juggling Shuttling ──►     │                   │
│              └───────────────( Pebble )──────────────┘                   │
│                                (65 g)                                    │
└──────────────────────────────────────────────────────────────────────────┘
```

### Coordinate Transforms

- **`P_to_T`**: `p_T = R_P_to_T · p_P + p_P_in_T`.
- **`T_to_V`**: The lap tray coordinate system `T` is 1:1 mapped to the virtual river frame `V`: `origin_V = origin_T`.
- **River Current Surface**: Rendered at `y_V = −5.0 mm` (slightly beneath the resting palm planes), so the otter's chest appears comfortably semi-submerged in clear flowing mountain water.

### Virtual Entity Inventory

| Entity Name | Frame / Anchor | Position / Bounds | Dimensions | Visual Description & Behavior |
|---|---|---|---|---|
| Lap Water Rapids | `V` | `x ∈ [−500, +500], z ∈ [−190, +190]` | Plane 1000 × 380 mm | Flowing clear mountain water with animated normal maps and foamy eddies |
| Clam Shell Target | `V` | Spawns upstream, drifts to anvil `(0, 0, 0)` | 42 × 36 × 22 mm bivalve | Ribbed river mussel shell with pearlescent purple/black texture |
| Leaping Silver Trout| `V` | Parabolic arc from `z = −160` to `z = +160`| 110 mm long fish mesh | Shimmering iridescent silver salmonid fish with splashing water drops |
| Spiny River Urchin | `V` | Spawns upstream, drifts past lap | Radius 28 mm spiny ball | Dark purple-black sea urchin with sharp needle spines; hazard |
| Otter Whiskers / Snout| `V` | Fixed at `(80, 140, 0)_V` | 60 mm diameter volume | Cute otter nose and twitching sensory vibrissae; trout target goal |
| Shell Fragment VFX | `V` | Spawns at clam fracture point | 12 procedural shards | Shattered calcium pieces flying outward with white foam spray |
| Agility Combo Ribbon| `P` | Trailing behind pebble rim | 180 mm ribbon trail | Glowing golden fluid spiral trail that intensifies with combo multiplier |

## 8. Gameplay Systems & Rules

### Hunger Energy & Wave Scoring

1. **Otter Hunger Energy Bar (`E_hunger ∈ [0, 100] %`)**:
   - Initial Energy: 75.0 %.
   - Baseline Metabolism Drain: `−1.8 %/s` (rapid river metabolism).
   - Food Nutritional Rewards:
     - Shattered River Clam: **+22.0 % Energy**, +250 Base Points.
     - Caught Leaping Trout: **+18.0 % Energy**, +350 Base Points.
     - Armored Crayfish: **+35.0 % Energy**, +600 Base Points.
     - Spiny Urchin Strike Penalty: **−30.0 % Energy**, resets combo multiplier to 1.0×.

2. **Scoring Formula per Action**:
   `ActionScore = BasePoints · Multiplier · AlignmentFactor`
   Where `Multiplier ∈ [1.0, 4.0]` is charged by palm juggling shuttles.

3. **Loss Conditions**:
   - `FAIL_STARVATION`: `E_hunger ≤ 0.0 %` (otter runs out of energy and curls up to rest).
   - `FAIL_PEBBLE_LOST`: Pebble falls off the lap tray and remains untracked on the floor for > 4.0 seconds (treasured pebble swept away downriver).
   - `FAIL_URCHIN_POISON`: Cracking 3 toxic urchins in a single session.

4. **Victory Criteria**:
   Complete all 5 rapids waves with `E_hunger > 0 %`.

## 9. Game State Machine

```
┌─────────┐      ┌──────────────┐      ┌────────────┐      ┌─────────────┐
│  BOOT   ├─────►│  REGISTER_P  ├─────►│ ANVIL_SET  ├─────►│ WAVE_INTRO  │
└─────────┘      └──────────────┘      └────────────┘      └──────┬──────┘
                                                                  │
┌──────────────┐      ┌──────────────┐      ┌───────────┐         │
│ RAPIDS_CLEAR │◄─────┤ WAVE_SUCCESS │◄─────┤ DRIFT_    │◄────────┘
└──────────────┘      └──────┬───────┘      │ ACTION    │
                             │              └─────┬─────┘
                             ▼                    │
                      ┌──────────────┐            ▼
                      │ NEXT_WAVE    │     ┌─────────────┐
                      └──────────────┘     │ FAIL_HUNGER │
                                           └─────────────┘
```

### State Machine Transition Table

| State Name | Entry Conditions | Continuous Operations | Exit Guard / Transition Condition | Next State |
|---|---|---|---|---|
| `BOOT` | Application launch | Load water shaders, initialize bimanual hand tracking | System tracking ready | `REGISTER_P` |
| `REGISTER_P` | Pebble placed in palm | Fit 70 × 24 mm oblate basalt contour; calculate axes | Pebble registered (`RMS < 0.60 mm`) | `ANVIL_SET` |
| `ANVIL_SET` | Pebble pose valid | Calibrate lap tray / table anvil height `y_T`; display water stream | Anvil confirmed, hands ready | `WAVE_INTRO` |
| `WAVE_INTRO` | Wave start | Display wave title (e.g. "Wave 2: Trout Leap Canyon"); start river audio | Intro animation timer `t ≥ 2.5 s` | `DRIFT_ACTION` |
| `DRIFT_ACTION`| Wave active | Spawn clams, trout, urchins; track juggling shuttles; resolve smashes | Wave duration elapsed (35–50 s) AND `E_hunger > 0` | `WAVE_SUCCESS` |
| `DRIFT_ACTION`| Hunger drops to zero | Fade audio to muted underwater tone; play otter whine | `E_hunger ≤ 0 %` | `FAIL_HUNGER` |
| `DRIFT_ACTION`| Pebble dropped | Start 4.0 s countdown timer; display amber recovery beacon | Pebble untracked > 4.0 s | `FAIL_PEBBLE` |
| `WAVE_SUCCESS`| Wave targets cleared | Tally wave score, play celebratory otter chirp chorus, award pearl tokens | Player performs double palm clap | Wave < 5: `NEXT_WAVE`; Wave = 5: `RAPIDS_CLEAR` |
| `NEXT_WAVE` | Score acknowledged | Advance wave index `W ← W + 1`; increase river flow speed | Hands repositioned on lap | `WAVE_INTRO` |
| `FAIL_HUNGER` | Starvation trigger | Display "HUNGRY OTTER / REST NEEDED" overlay; offer restart | Player taps virtual restart shell | `WAVE_INTRO` |
| `RAPIDS_CLEAR`| Wave 5 completed | Grand waterfall plunge VFX; display legendary Golden Pebble Trophy | Player juggles stone 3 times | `SESSION_END` |

## 10. Physics & Collision

### Collider Interaction Matrix

| Collider Pair | Interaction Type | Physics Parameters | Gameplay Result |
|---|---|---|---|
| `COL_STRIKE_FACE` × `COL_CLAM_SHELL` | High-Impulse Discrete Impact | Restitution `e = 0.18`, friction `μ = 0.40` | Computes `E_impact`; shatters shell if threshold met; spawns meat |
| `COL_VOLLEY_RIM` × Leaping Trout | Kinematic Volley Trigger | Restitution `e = 0.65`, impulse vector toward snout | Deflects trout upward into mouth; awards points |
| `COL_PEBBLE_CORE` × `COL_PALM_L/R` | Continuous Penalty Spring | Stiffness `k = 420 N/m`, damping `ζ = 0.45` | Bimanual juggling shuttles; friction allows smooth finger rolls |
| `COL_STRIKE_FACE` × Spiny Urchin | Poison Hazard Impact | Restitution `e = 0.05` | Urchin crushes, releasing toxic green cloud; penalizes hunger −30 % |
| `COL_PEBBLE_CORE` × Lap Tray Anvil | Rigid Boundary Impact | Restitution `e = 0.22`, static friction `μ = 0.55` | Stops downward stone travel; produces crisp tactile impact thump |

### Fracture Solver Details

- **Impulse Integration**: Because downward strikes can reach `1.2 m/s`, the solver executes 2 substeps per 90 Hz tick (`dt = 5.55 ms`).
- **Swept Cylinder Impact**: The strike face cylinder tests swept penetration against the clam shell bounding box to eliminate tunneling.
- **Procedural Shard Spawner**: Shattered bivalves spawn 12 rigid-body shell shards with radial velocities `v_shard = (0.4 + 0.6 · Rand()) m/s` directed away from impact center.

## 11. Rendering, Occlusion & Compositing

### River Water & Dynamic Foam Compositing

1. **Water Surface Mesh**: 1000 × 380 mm planar ribbon resting on the lap tray with scrolling Gerstner wave displacement.
2. **Foam & Eddy Generation**: Contact between the physical pebble and virtual water injects dynamic foam texture into a 512 × 512 procedural render target.
3. **Subsurface Clam Rendering**: Target clams resting on the lap anvil are rendered through semi-translucent water with light refraction and underwater caustics.
4. **Pebble Depth Occlusion**: Oblate depth proxy (`COL_PEBBLE_CORE`) writes depth, ensuring the physical stone correctly occludes underwater clams, floating fish, and splash particles.
5. **Hand Skeletal Occlusion**: Tracked hand meshes write depth so otter paws and fingers remain visually in front of virtual water and clam shells.

### VFX Tris & Particle Budgets

- **River Surface Grid**: 180 quads (360 triangles) with custom HLSL shallow-water shader.
- **Clam Shell Models**: 840 triangles per clam (max 2 active concurrently = 1,680 triangles).
- **Leaping Silver Trout**: 1,220 triangles with vertex sine-wave spine animation.
- **Water Droplets & Splash Particles**: Max 200 concurrent quad particles (0.5 s lifetime).
- **Shell Shard Debris**: Max 36 active physics shards (12 per clam, max 3 broken sets).
- **Total Transparent Overdraw**: Peak 1.85× screen area.

## 12. Audio & Feedback

| Event Trigger | Spatial Sound Cue | Frequency / Synthesis Profile | Visual Cue |
|---|---|---|---|
| Bimanual Palm Transfer | Wet skin slap & glide | 240 Hz soft squelch + 1.2 kHz satin slide | Gold sparkle trail connects left and right palms |
| Downward Strike (Air Whoosh) | High-speed air swish | Bandpass noise 400–1200 Hz, volume proportional to `v_strike` | Motion blur wake trailing the pebble |
| Clam Shell Fracture (Success) | Crunchy calcium shatter | Dual impact: 3.4 kHz brittle ceramic snap + 120 Hz hollow thump | Shell explodes into shards; glowing pink clam meat pops up |
| Glancing / Weak Strike | Dull granite clink | High metallic ping (2.8 kHz, 45 ms), no bass thump | White deflection ring; "SMASH HARDER!" prompt |
| Trout Volley Hit | Crisp wet tail slap | 680 Hz wet pop + 2.4 kHz water spray | Trout flips end-over-end into snout with splash ring |
| Trout Swallowed | Satisfied otter chirp | Melodic two-tone whistle (1.8 kHz → 2.4 kHz) | Hunger bar pulses bright green (+18 %); floating point text |
| Spiny Urchin Hit | Sharp toxic squirt | Dissonant tritone screech (440 Hz + 622 Hz) + hiss | Toxic green vapor cloud; screen flashes purple warning |
| Pebble Lost Alarm | Distressed otter whistle | Rapid warbling chirps (3.2 kHz FM, 4 Hz rate) | Amber pulsing search beacon pointing to dropped pebble |
| Wave Complete Fanfare | Joyful river chorus | Playful wooden marimba melody + river waterfall crescendo | Golden pearl fireworks erupt from water; Agility Medallion |

## 13. Difficulty Progression & Tuning

### 5-Wave Rapids Progression Matrix

| Wave | River Velocity `v_river` | Clam Types & Armor | Trout Leaps | Urchin Hazards | Juggling Requirement | Hunger Drain Rate |
|---|---|---|---|---|---|---|
| Wave 1 | 0.25 m/s | 4 Soft Clams (`E_crit = 0.025 J`) | None | None | Basic shuttling (1.5× combo) | `−1.8 %/s` |
| Wave 2 | 0.35 m/s | 3 Soft Clams + 1 Hard Mussel | 4 Trout (Parabolic) | None | Shuttle between strikes (2.0×) | `−2.0 %/s` |
| Wave 3 | 0.45 m/s | 2 Hard Mussels + 1 Crayfish | 5 Fast Trout | 2 Drifting Urchins | Precision urchin brush + 2.5× combo | `−2.4 %/s` |
| Wave 4 | 0.60 m/s | 3 Hard Mussels + 2 Crayfish | 6 Cross-Leaping Trout | 4 Drifting Urchins | Continuous 3.0× juggling flow | `−2.8 %/s` |
| Wave 5 | 0.75 m/s | 1 Giant Golden Clam (3 smashes) | King Trout (High Arc) | 5 Rapid Urchins | Maximum 4.0× combo execution | `−3.2 %/s` |

### Adaptive Assistance Mechanics

- **Starvation Safeguard**: If `E_hunger < 20 %`, the river current slows by 30 %, and the next incoming clam drifts directly into the center anvil pocket with an amplified 1.5× hit box.
- **Agility Mastery Surge**: If player maintains a 4.0× combo for > 10 consecutive seconds, pebble strikes unleash a "River Shockwave" that automatically cracks any secondary clam within 60 mm.

### Named Tuning Constants

```csharp
public static class OtterTuningConstants
{
    // Prop Dimensions & Mass
    public const float PEBBLE_SEMI_AXIS_A = 0.035f; // 35 mm
    public const float PEBBLE_SEMI_AXIS_C = 0.012f; // 12 mm
    public const float PEBBLE_MASS_KG = 0.065f;     // 65 g
    public const float MOMENT_INERTIA_Z = 3.185e-5f;

    // Strike & Fracture Energy Thresholds (Joules)
    public const float ENERGY_CRIT_SOFT_CLAM = 0.025f;
    public const float ENERGY_CRIT_HARD_MUSSEL = 0.045f;
    public const float ENERGY_CRIT_CRAYFISH = 0.075f;
    public const float MIN_STRIKE_VELOCITY = 0.28f; // m/s
    public const float MIN_VOLLEY_VELOCITY = 0.30f; // m/s

    // Juggling & Agility Combo
    public const float SHUTTLE_WINDOW_MIN = 0.12f;  // seconds
    public const float SHUTTLE_WINDOW_MAX = 0.65f;  // seconds
    public const float COMBO_DECAY_DELAY = 2.5f;    // seconds
    public const float MAX_COMBO_MULTIPLIER = 4.0f;
}
```

## 14. Edge Cases & Failure Recovery

### Failure Edge Cases & Robustness Handlers

- **EC-OTT-01: Pebble Drops from Lap onto Floor**:
  - *Symptom*: Rapid shuttling causes player to fumble; the physical stone rolls off the lap tray onto the carpet.
  - *Detection*: Prop pose `p_P` drops > 150 mm below lap surface `y_T`.
  - *Recovery*: River current pauses immediately. A floating amber beacon with an animated 3D directional arrow points to the floor location of the stone. A 4.0 s grace recovery timer begins. If retrieved and placed back on the lap within 4.0 s, the session resumes with 0 combo penalty.
- **EC-OTT-02: Palm Tracking Fusion Clutter (Hands Clapped Together)**:
  - *Symptom*: Player holds the stone with both hands pressed together, merging left and right hand skeletal tracking into a single ambiguous cluster.
  - *Detection*: Skeletal tracking pipeline reports high palm overlap (`||p_palm_L - p_palm_R|| < 25 mm`).
  - *Recovery*: The classifier treats this as a "Two-Handed Safe Clench": preserves the current combo multiplier, locks stone position to the bimanual centroid, and suppresses false strike trigger events.
- **EC-OTT-03: Ultra-Fast Downward Smashes (Inter-Frame Tunneling)**:
  - *Symptom*: Player slams stone downward at 1.4 m/s; stone passes completely through the 22 mm clam shell between two visual frames.
  - *Detection*: Raycast between `p_P(t - dt)` and `p_P(t)` intersects the clam shell collider.
  - *Recovery*: Swept continuous collision detection (CCD) catches the raycast intersection, sets contact point to the surface entry normal, and executes the fracture solver at the exact collision point.
- **EC-OTT-04: Non-Dominant Strike Reversal**:
  - *Symptom*: Left-handed player prefers smashing with the left hand while clamping clams with the right.
  - *Recovery*: The system is completely ambidextrous; strike and clamp roles are dynamically assigned based on downward velocity and height without requiring manual left/right toggling.

## 15. Implementation Architecture

### Class Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────────┐
│                        OtterGameDirector                               │
│     (Wave Flow, Hunger Management, Scoring, River Current Director)    │
└───────────────▲────────────────────────▲────────────────────────▲──────┘
                │                        │                        │
┌───────────────┴────────┐  ┌────────────┴───────────┐  ┌─────────┴──────┐
│  PropTrackerOtter      │  │  BimanualJugglingFSM   │  │ ShellFracture  │
│  - 72 Hz Oblate Fit    │  │  - Palm Ownership      │  │    Solver      │
│  - Fast Re-Acquisition │  │  - Agility Multiplier  │  │ - Kinetic Energy│
│  - Strike Extractor    │  │  - Shuttle Detection   │  │ - Shard Spawner│
└────────────────────────┘  └────────────────────────┘  └────────────────┘
```

### Concrete C# Implementation Scripts

#### Script 1: `BimanualJugglingFSM.cs`

```csharp
using System;
using UnityEngine;

public class BimanualJugglingFSM : MonoBehaviour
{
    public enum PalmOwner { NONE, LEFT_PALM, RIGHT_PALM, IN_FLIGHT }

    [Header("Current Juggling State")]
    public PalmOwner currentOwner = PalmOwner.NONE;
    public float agilityMultiplier = 1.0f;
    public int consecutiveShuttles = 0;

    private float lastTransferTime = 0.0f;
    private float stationaryTimer = 0.0f;
    private PalmOwner previousOwner = PalmOwner.NONE;

    private const float PALM_PROXIMITY_RADIUS = 0.035f; // 35 mm
    private const float MIN_TRANSFER_TIME = 0.12f;
    private const float MAX_TRANSFER_TIME = 0.65f;
    private const float COMBO_DECAY_DELAY = 2.5f;

    public void UpdateJuggling(Vector3 pebblePos, Vector3 leftPalmPos, Vector3 rightPalmPos, float dt)
    {
        float distLeft = Vector3.Distance(pebblePos, leftPalmPos);
        float distRight = Vector3.Distance(pebblePos, rightPalmPos);

        PalmOwner detectedOwner = PalmOwner.IN_FLIGHT;
        if (distLeft < PALM_PROXIMITY_RADIUS && distLeft < distRight)
        {
            detectedOwner = PalmOwner.LEFT_PALM;
        }
        else if (distRight < PALM_PROXIMITY_RADIUS && distRight < distLeft)
        {
            detectedOwner = PalmOwner.RIGHT_PALM;
        }

        // Evaluate ownership transition
        if (detectedOwner != currentOwner)
        {
            float now = Time.time;
            if ((currentOwner == PalmOwner.LEFT_PALM && detectedOwner == PalmOwner.RIGHT_PALM) ||
                (currentOwner == PalmOwner.RIGHT_PALM && detectedOwner == PalmOwner.LEFT_PALM))
            {
                float transferDuration = now - lastTransferTime;
                if (transferDuration >= MIN_TRANSFER_TIME && transferDuration <= MAX_TRANSFER_TIME)
                {
                    // Valid shuttle completed!
                    consecutiveShuttles++;
                    agilityMultiplier = Mathf.Clamp(agilityMultiplier + 0.25f, 1.0f, 4.0f);
                    stationaryTimer = 0f;
                }
                lastTransferTime = now;
            }
            previousOwner = currentOwner;
            currentOwner = detectedOwner;
        }
        else if (currentOwner == PalmOwner.LEFT_PALM || currentOwner == PalmOwner.RIGHT_PALM)
        {
            stationaryTimer += dt;
            if (stationaryTimer > COMBO_DECAY_DELAY)
            {
                agilityMultiplier = Mathf.MoveTowards(agilityMultiplier, 1.0f, dt * 0.5f);
            }
        }
    }
}
```

#### Script 2: `ShellFractureSolver.cs`

```csharp
using System;
using UnityEngine;

public class ShellFractureSolver : MonoBehaviour
{
    [Header("Pebble Mass Properties")]
    public float pebbleMassKg = 0.065f; // 65 g

    [Header("Fracture Thresholds")]
    public float critEnergySoftClam = 0.025f; // Joules
    public float critEnergyHardMussel = 0.045f;
    public float minStrikeVelocity = 0.28f;   // m/s

    public event Action<Vector3, float> OnShellShattered;
    public event Action<Vector3> OnWeakStrikeGlance;

    public bool EvaluateClamStrike(Vector3 pebblePos, Vector3 strikeVelocity, Vector3 polarNormalZ, Vector3 anvilNormalY, float currentMultiplier, float targetShellCritEnergy)
    {
        float speed = strikeVelocity.magnitude;
        if (speed < minStrikeVelocity)
        {
            OnWeakStrikeGlance?.Invoke(pebblePos);
            return false;
        }

        // Downward kinetic energy
        float rawKineticEnergy = 0.5f * pebbleMassKg * speed * speed;

        // Alignment factor: how flat the oblate face hits the anvil
        float alignment = Mathf.Abs(Vector3.Dot(polarNormalZ.normalized, anvilNormalY.normalized));
        alignment = Mathf.Clamp(alignment, 0.2f, 1.0f);

        // Effective smashing energy
        float effectiveEnergy = rawKineticEnergy * alignment * currentMultiplier;

        if (effectiveEnergy >= targetShellCritEnergy)
        {
            OnShellShattered?.Invoke(pebblePos, effectiveEnergy);
            return true;
        }
        else
        {
            OnWeakStrikeGlance?.Invoke(pebblePos);
            return false;
        }
    }
}
```

## 16. Acceptance Test Checklist

| Test ID | Test Scenario & Verification Inputs | Quantitative Pass Threshold |
|---|---|---|
| `TC-OTT-01` | Oblate Basalt Spheroid Fitting | Present 70.0 × 24.0 mm oblate prop in palm at 0.35–0.50 m distance | Ellipsoid fit converges < 0.9 s; `a = 35.0 ± 0.8 mm`, `c = 12.0 ± 0.6 mm` |
| `TC-OTT-02` | Bimanual Transfer Classification | Shuttle prop between palms 10 times at 0.3 s intervals | System registers 10/10 valid shuttles; combo multiplier reaches 3.5× |
| `TC-OTT-03` | Combo Multiplier Decay Delay | Hold prop stationary in left palm for 4.0 s | Multiplier remains static for 2.5 s, then decays linearly at `0.5 ± 0.05 s⁻¹` |
| `TC-OTT-04` | Flat-Face Shell Smash Trigger | Drive stone downward onto target clam at `v = 0.38 m/s` with flat alignment | Impact energy `E_eff ≥ 0.045 J`; clam shell fractures into 12 shards; +250 pts awarded |
| `TC-OTT-05` | Glancing Angle Energy Attenuation | Drive stone at `v = 0.38 m/s` but with 60° tilt (`alignment ≈ 0.5`) | `E_eff < 0.030 J`; shell reflects stone without shattering; weak strike cue triggers |
| `TC-OTT-06` | Trout Parabolic Volley Flick | Rapid rim flick upward (`v_y = 0.40 m/s`) intersecting leaping fish | Trout deflects along trajectory toward snout; +150 pts and +18 % hunger awarded |
| `TC-OTT-07` | Toxic Urchin Avoidance | Sweep open paw laterally 50 mm ahead of drifting urchin | Urchin deflects out of river boundary; zero hunger penalty incurred |
| `TC-OTT-08` | Dropped Pebble Floor Grace Window | Drop stone onto floor; retrieve and place on lap within 3.2 s | System pauses river, triggers beacon, and resumes without starvation penalty |
| `TC-OTT-09` | Hunger Bar Metabolic Depletion | Run idle for 10.0 seconds without cracking food | Hunger drains exactly `18.0 ± 1.0 %` over 10 s (`1.8 %/s`) |
| `TC-OTT-10` | 5-Wave Complete Run Soak Test | Execute complete 5-wave river rapids run (4.0 minutes) at 90 Hz render | Zero frame drops below 88 Hz; memory footprint stable (< 1.8 MB/min allocation) |

---
