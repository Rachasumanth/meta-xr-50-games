# Build Log - singing_bowl_sanctuary

## Chapter 4.3b: Hemispheres (Hand Scale) - Game 02

### 1. Specification & Asset Provenance
- **Prop**: Ceramic/metal singing bowl, concave-up (`R = 85.0 mm`, `d = 60.0 mm`, `t = 6.0 mm`, `m = 700 g`).
- **Asset**: `777-ramen-noodle-bowl.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/singing_bowl_sanctuary/Models/singing_bowl.glb`.
- **Collider**: Spherical cap cavity, rim torus, outer spherical cap.

### 2. Architecture & Pure C# Domain Decoupling
- `SingingBowlConfig.cs`: Physical parameters (radii, depth, inner rim radius a = 81.24 mm, circumference 510.48 mm, resonant speed 260 mm/s, resonance tau = 0.450s, ring spacing 35 mm, lotus radius 18 mm).
- `SingingBowlModel.cs`:
  - `PropBowl`: Spherical cap fitting and trace angular projection.
  - `ResonanceModel`: Gaussian resonance curve `exp(-((v-260)/(260*0.223))^2)`, first-order response with 0.45s tau, sag/collapse timer.
  - `Locust`: Ring pulse sweep and collision elimination.
- `SingingBowlRig.cs`: Thin MonoBehaviour handling scene entities and petals ledger.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Rim Fit (R, a, rim RMS).
- AT02: Trace Projection (phi = 0° and 90°).
- AT03: Speed Law (lap in 1.96 ± 0.03s at 260 mm/s).
- AT04: Gaussian Band (evaluation at 260, 202, 318, 360 mm/s).
- AT05: Resonance Lag (0.63 in 0.45s).
- AT06: Wobble threshold.
- AT07: Collapse evaluation (100 mm/s collapses in 1.2s).
- AT08: Ring Kill (locust at 35 mm killed, 45 mm spared).
- AT09: Lotus Hit (1 petal lost).
- AT10: Cradle Slip (g_grip drops to 0.30).
- AT11: Early Clear.
- AT12: Occlusion logic test.
- AT13: Frame Budget (particles <= 1800).
- AT14: Tracking Recovery (<= 1.2s).
