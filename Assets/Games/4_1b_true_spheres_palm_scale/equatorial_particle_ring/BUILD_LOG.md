# Build Log: Equatorial Particle Ring (`equatorial_particle_ring`)

## 1. Specifications & Metadata
- **Slug**: `equatorial_particle_ring`
- **Family**: True sphere, palm-scale (6–12 cm class), hand-held with traced equator
- **Prop**: Baseball, true sphere, r = 37.0 mm (74 mm diameter)
- **Virtual Setup**: Accelerator ring radius 39.2 mm; target belt 300–500 mm

## 2. Implemented Components
- `EquatorialParticleRingConfig.cs`: Ball radius (37mm), min fire arc (25mm), min omega (2.5 rad/s), heat thresholds.
- `PropTracker.cs`: Sphere pose tracking and seam confidence.
- `CradleMonitor.cs`: Non-dominant hand cradle tracking, slip detection (decay 40%/s).
- `EquatorTracer.cs`: Dominant index equator tracing, arc accumulation, omega calculation, 60ms direction settle.
- `RingCharge.cs`: Plasma energy mapping (30–100), quadratic heat integration (4 + 22*(w/10)^2), jam logic (45 heat, 2s lock).
- `LaunchSolver.cs`: Tangent launch bearing calculation, 12° player singularity block, fire gate.
- `BoltFlight.cs`: Constant-velocity bolt flight, flight time prediction, target sector impact evaluation.
- `TargetDirector.cs`: 8 drifting targets on circular belt.
- `RunScoring.cs`: Hit, graze, bounce scoring and multiplier chains.
- `RunStateMachine.cs`: 120s run timer, jam limit tracking (max 4).
- `FeedbackRouter.cs`: 14 distinct audio-visual feedback events mapped to spatial cues.
- `EquatorialParticleRingRig.cs`: MonoBehaviour runtime wiring.

## 3. Test Suite
- `AcceptanceTests.cs`: 14 tests verifying AT-01 through AT-14.
  - Automated tests: AT-01, AT-02, AT-03, AT-04, AT-05, AT-06, AT-07, AT-08, AT-09, AT-10, AT-11, AT-12, AT-14.
  - Deferred tests: AT-13 (depth buffer ring occlusion).
