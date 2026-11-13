# Build Log - snapping_turtle_bunker

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 03

### 1. Specification & Asset Provenance
- **Prop**: Half walnut shell, dome-up (`R = 22.0 mm`, `t = 1.8 mm`, `m = 4.0 g`).
- **Asset**: `175-sea-turtle.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/snapping_turtle_bunker/Models/turtle_bunker.glb`.
- **Collider**: Convex hemisphere, rim torus, apex press sensor.

### 2. Architecture & Pure C# Domain Decoupling
- `SnappingTurtleConfig.cs`: Physical parameters (radii, tilt limits, calibrated 0.8N tilt, seal force/dwell, frenzy thresholds).
- `TurtleModel.cs`:
  - `PropShell`: Hemisphere fitting and rim plane orientation test.
  - `TiltDynamics`: First-order lag (tau = 180 ms), tilt clamp [0, 8°], natural decay on contact release.
  - `SectorClassifier`: 4 cardinal sectors and opposite sector resolver.
  - `ApexSealDetector`: 150 ms dwell with force >= 0.35 N seal qualification.
  - `BiteController`: Reachability and angular alignment check for beetle snaps.
  - `ChainTracker`: Frenzy trigger on 3 bites in 4 seconds.
- `TurtleRig.cs`: Thin MonoBehaviour connecting domain state to visuals and game loop.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Registration Accuracy (20 presentations R within ±0.5 mm, tilt < 1.5°).
- AT02: Tilt Mapping (0.8 N produces 3.91° ± 0.4°).
- AT03: Sector Classification (cardinal sectors >= 96%).
- AT04: Bite Timing (beetle on raised sector at 26 mm killed >= 95%).
- AT05: Wrong Sector Bite (opposite sector never killed).
- AT06: Seal Window (150 ms apex press absorbs acorn 100%).
- AT07: Unsealed Hit (no press causes integrity loss and stun).
- AT08: Tilt Clamp (never exceeds 8°).
- AT09: Chain Logic (Snap Frenzy 3 bites in 4s).
- AT10: Hand Loss Decay (tau = 180 ms decay).
- AT11: Tide Timer (300 s total for 5 tides).
- AT12: Pause functionality.
- AT13: Occlusion logic test.
- AT14: Performance benchmark.
