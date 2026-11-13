# Build Log - temple_bell

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 10

### 1. Specification & Asset Provenance
- **Prop**: Metal bottle cap balanced dome-up on a fingertip (`R = 15.0 mm`, `t = 0.5 mm`, `m = 2.0 g`).
- **Asset**: `704-teacup-and-saucer.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/temple_bell/Models/temple_bell.glb`.
- **Collider**: Convex dome, rim torus, fingertip support point.

### 2. Architecture & Pure C# Domain Decoupling
- `TempleBellConfig.cs`: Physical constants (pendulum frequency 4.1 Hz, damping zeta = 0.14, tap impulse 0.4 rad/s, fall threshold 20° for 120 ms, acoustic ring speed 220 mm/s, debounce 150 ms).
- `BellModel.cs`:
  - `PropBell`: Registration and rim orientation check.
  - `PendulumDynamics`: Second-order damped harmonic oscillator, tap impulse accumulator, fall timeout detector.
  - `AcousticRing`: Expanding ring wavefront, `cos^4(psi)` directional acoustic gain, spirit push computation (8.4 mm at r = 150 mm).
- `BellRig.cs`: Thin MonoBehaviour coordinating ring entities and scene temple courtyard.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Pendulum Frequency (4.1 ± 0.4 Hz, zeta = 0.14).
- AT02: Tap Impulse (0.15 N for 40 ms raises beta_dot by 0.4 ± 0.06 rad/s).
- AT03: Fall Threshold (beta = 21° for 120 ms triggers fall).
- AT04: Ring Speed (expands at 220 ± 10 mm/s).
- AT05: Gain Law (psi = 0, 15, 30, 45 deg yields 1.00, 0.87, 0.56, 0.25 ± 0.02).
- AT06: Resonant Hit (G > 0.85 deals 2 damage, +50 bonus).
- AT07: Push (8.4 ± 1 mm push at 150 mm with G = 0.9).
- AT08: Spirit Loss condition (r < 30 mm).
- AT09: Debounce window (150 ms).
- AT10: Tap Force Window (0.04 N ignored, 0.35 N registered).
- AT11: Brace wobble reduction (30%).
- AT12: Pause functionality.
- AT13: Occlusion logic test.
- AT14: Performance benchmark.
