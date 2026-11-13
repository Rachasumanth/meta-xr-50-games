# Build Log - limpet_suction

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 09

### 1. Specification & Asset Provenance
- **Prop**: Half walnut shell, dome-up (`R = 22.0 mm`, `t = 1.8 mm`, `R_in = 20.2 mm`, `m = 4.0 g`).
- **Asset**: `071-alien-ufo-saucer.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/limpet_suction/Models/limpet.glb`.
- **Collider**: Convex dome, rim torus, flat-seal sensor.

### 2. Architecture & Pure C# Domain Decoupling
- `LimpetSuctionConfig.cs`: Physical constants (radii, max tilt 4°, max rim gap 1.5 mm, seal force 6.0 N for 250 ms, unsealed mu = 0.30 vs sealed mu = 3.0, overpress limit 12 N).
- `LimpetModel.cs`:
  - `PropShell`: Hemisphere fitting and rim plane orientation.
  - `SealDetector`: Hysteresis gating on normal force, tilt angle, and gap distance with 250 ms dwell qualification.
  - `WaveDynamics`: Surge displacement calculation and overpress crack detection.
- `LimpetRig.cs`: Thin MonoBehaviour handling scene reef interaction and state updates.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Seal Force (6.0 N, beta = 3° seals in 250 ms).
- AT02: Flat Tolerance (beta = 5° never seals).
- AT03: Rim Gap (2 mm gap blocks seal).
- AT04: Friction (unsealed displaces >= 18 mm).
- AT05: Sealed Hold (displaces < 1 mm).
- AT06: Overpress (12.5 N cracks shell, loses 1 integrity).
- AT07: Grazing cell consumption rate.
- AT08: Regrowth timer (125 s).
- AT09: Dislodge tracking.
- AT10: Tide Clear evaluation.
- AT11: Combo multiplier (1.6x).
- AT12: Pause functionality.
- AT13: Occlusion logic test.
- AT14: Performance benchmark.
