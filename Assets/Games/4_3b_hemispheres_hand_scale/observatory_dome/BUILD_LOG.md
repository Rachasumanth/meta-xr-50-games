# Build Log - observatory_dome

## Chapter 4.3b: Hemispheres (Hand Scale) - Game 04

### 1. Specification & Asset Provenance
- **Prop**: Inverted cereal bowl, dome-up (`R = 75.0 mm`, `depth = 55.0 mm`, `t = 4.0 mm`, `m = 380 g`).
- **Asset**: `971-satellite-dish.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/observatory_dome/Models/observatory_dome.glb`.
- **Collider**: Convex dome, rim torus, foot ring.

### 2. Architecture & Pure C# Domain Decoupling
- `ObservatoryDomeConfig.cs`: Physical constants (radii, depth, shutter range [8°, 60°], lock time 1.2s, break timeout 0.4s, slew limit 90°/s, escape time 12s).
- `ObservatoryModel.cs`:
  - `PropDome`: Spherical cap fitting.
  - `YawTracker`: Dome yaw calculation.
  - `SlitController`: 30 mm geodesic finger separation mapping to 40° aperture.
  - `Target`: Containment testing, lock accumulation, slew guard suppression, and uncontained escape timer.
- `ObservatoryRig.cs`: Thin MonoBehaviour handling scene updates and capture state.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Yaw Mapping (90° -> 90° ± 2°).
- AT02: Yaw Wrap (360° unwrap).
- AT03: Slit Width (30 mm -> 40° ± 4°).
- AT04: Containment (contained at 40°, rejected at 12°).
- AT05: Lock Timing (1.2s captures target).
- AT06: Break logic (0.4s non-containment resets lock).
- AT07: Closed shutter (w = 5° blocks lock).
- AT08: Slew guard (120 °/s blocks lock).
- AT09: Escape timer (12s uncontained escapes).
- AT10: Chain multiplier (1.3x).
- AT11: Re-anchor state.
- AT12: Occlusion logic test.
- AT13: Pause functionality.
- AT14: Performance benchmark.
