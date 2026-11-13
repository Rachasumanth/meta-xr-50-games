# Build Log - micro_mortar_alchemist

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 08

### 1. Specification & Asset Provenance
- **Prop**: Concave bottle cap (+ fingertip pestle), concave-up (`R = 15.0 mm`, `t = 0.5 mm`, `R_in = 14.5 mm`, `m = 2.0 g`).
- **Asset**: `702-soup-bowl.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/micro_mortar_alchemist/Models/micro_mortar.glb`.
- **Collider**: Concave cavity, rim torus, pestle sphere.

### 2. Architecture & Pure C# Domain Decoupling
- `MicroMortarConfig.cs`: Physical constants (radii, pour threshold 35°, flow rate multiplier 0.9, dose target 10g ± 1.5g).
- `MortarModel.cs`:
  - `PropMortar`: Registration geometry fitting.
  - `Pestle`: Penetration depth to normal force mapping (1 mm -> 0.6 N).
  - `RhythmClock`: Phase scoring for on-beat vs off-beat grinds.
  - `Shard`: Essence extraction, impurity accumulation, and dissolution transition.
  - `PourController`: Tilt-driven flow rate (0.58 g/s at 40°) and dose evaluation.
- `MortarRig.cs`: Thin MonoBehaviour handling scene state and pour integration.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Pestle Contact (1 mm produces 0.60 N).
- AT02: Rhythm Scoring (phi = 0 scores 1.0, phi = 0.3 scores 0.35).
- AT03: Essence Rate (10 on-beat strokes add 70 ± 5 essence).
- AT04: Impurity (10 off-beat strokes add 30 ± 3 impurity).
- AT05: Dissolve (E = 100 becomes liquid).
- AT06: Pour Threshold (34° gives 0, 40° gives 0.58 ± 0.05 g/s).
- AT07: Dose Tolerance (10.0 ± 1.5 g succeeds, 13.0 g spoils).
- AT08: Color Match scoring.
- AT09: Spoil Count loss condition.
- AT10: Reload pausing on pestle lift.
- AT11: Grind Clamp (40 rad/s cap).
- AT12: Occlusion logic test.
- AT13: Adaptive difficulty scaling.
- AT14: Performance benchmark.
