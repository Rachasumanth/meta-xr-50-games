# Build Log - phantom_shell_game

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 01

### 1. Specification & Asset Provenance
- **Prop**: Half walnut shell, dome-up (`R = 22.0 mm`, `t = 1.8 mm`, `m = 4.0 g`).
- **Asset**: `702-soup-bowl.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/phantom_shell_game/Models/walnut_shell.glb`.
- **Collider**: Hemisphere exterior, rim torus, inner cavity sensor.

### 2. Architecture & Pure C# Domain Decoupling
- `PhantomShellConfig.cs`: Named physical constants matching Section 13 & 15.
- `ShellModel.cs`:
  - `PropShell`: Hemisphere least-squares fitting, rim plane normal verification, apex offset measurement.
  - `SlideModel`: First-order response with 90 ms tau, Coulomb friction deceleration (2747 mm/s²), soft patch boundary at 95 mm.
  - `LiftModel`: Dual threshold state classifier (lift at 12 mm, reseat below 4 mm), exposure timer accumulator.
  - `StrikeResolver`: Exact distance metrics (Block < 36 mm, Near Miss < 48 mm, Clean Evade >= 48 mm, Lift Snatch < 16 mm or exposure > 1.2s).
  - `HologramModel`: Critically damped lag tracking reaching 63% in 180 ms.
  - `FeintDirector`: Seeded level-4 feint distribution (55%).
- `ShellRig.cs`: Thin MonoBehaviour wiring XR poses, transforms, and fairy state.
- `AcceptanceTests.cs`: 14 automated NUnit tests covering all Section 16 requirements.

### 3. Acceptance Test Mapping
- AT01: Hemisphere Fit (R, t, normal error, apex error).
- AT02: Slide Law (108 mm/s steady state, stopping travel).
- AT03: Friction Constant (stopping distance from 220 mm/s).
- AT04: Patch Wall (never exceeds 96.5 mm, soft deceleration).
- AT05: Lift Classifier (12 mm lift, 4 mm reseat, peek count).
- AT06: Exposure Timer (1.2s threshold detection).
- AT07: Block Geometry (35, 36, 47, 48, 49 mm classification).
- AT08: Lift Snatch (fairy snatch evaluation).
- AT09: Feint Determinism (level 4 55% feint rate, seed determinism).
- AT10: Hologram Lag (63% in 180 ms, no overshoot).
- AT11: Occlusion logic test.
- AT12: Performance benchmark (700 simulation ticks).
- AT13: Recovery from tracking outage.
- AT14: Session endurance test.
