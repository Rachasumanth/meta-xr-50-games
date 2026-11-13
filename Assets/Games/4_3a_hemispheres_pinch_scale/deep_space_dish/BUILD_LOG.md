# Build Log - deep_space_dish

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 04

### 1. Specification & Asset Provenance
- **Prop**: Contact-lens case bowl, concave-up (`R = 14.0 mm`, `t = 1.0 mm`, `m = 3.0 g`).
- **Asset**: `971-satellite-dish.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/deep_space_dish/Models/satellite_dish.glb`.
- **Collider**: Concave inner spheroid cap, rim torus, outer grip band.

### 2. Architecture & Pure C# Domain Decoupling
- `DeepSpaceDishConfig.cs`: Physical constants (radii, gain thresholds, break timeouts, slew guard limit, escape timeout).
- `DishModel.cs`:
  - `PropDish`: Geometric fitting for radius and plane orientation.
  - `GainEvaluator`: Closed-form `cos^4(psi)` directional antenna gain.
  - `Satellite`: Lock accumulation, break timer, slew guard, and escape timer.
  - `AimFilter`: Slerp filter smoothing pointing dynamics.
- `DishRig.cs`: Thin MonoBehaviour handling satellite collections and score state.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Aim Accuracy (20° yaw gives 20° ± 1.5° in 250 ms).
- AT02: Gain Law (cos^4 evaluation at 0°, 15°, 30.6°, 45°).
- AT03: Lock Timing (1.5s hold triggers crack).
- AT04: Break Logic (below 0.35 for 0.8s resets lock).
- AT05: Slew Guard (120 °/s wrist flick does not increase lock).
- AT06: Escape Timer (25s escape threshold).
- AT07: Chain Multiplier (1.3x).
- AT08: Escape Loss condition (3 escapes).
- AT09: Tracking Loss re-acquisition.
- AT10: Cup Drop pause handling.
- AT11: Occlusion logic test.
- AT12: Performance benchmark.
- AT13: Second-finger brace stabilization.
- AT14: Registration accuracy (20 presentations R within ±0.4 mm).
