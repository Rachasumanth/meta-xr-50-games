# Build Log: Gyro-Sphere Mech (`gyro-sphere-mech`)

## 1. Specifications & Metadata
- **Slug**: `gyro-sphere-mech`
- **Family**: True sphere, palm-scale (6–12 cm class), two-hand cup gimbal
- **Prop**: Orange, true sphere, 76.0 mm diameter (r = 38.0 mm), 150 g
- **Virtual Setup**: Armored mech rolling on translucent 500 mm battle dome, twin chin lasers

## 2. Implemented Components
- `GyroSphereMechConfig.cs`: Ball radius (38mm), dome radius (500mm), max drive speed (220 mm/s), laser corridor (3.9 deg), laser range (170mm).
- `PropOrange.cs`: Sphere pose tracking and orientation frame.
- `GyroController.cs`: Palm shear drive velocity, squeeze throttle calculation (D0 - D) / 22, wrist yaw heading (-150 to +150 deg) with re-grip prompt, clutch re-grip reset.
- `MechBody.cs`: Unit normal dome position, heading, 4 armor points (repaired +1 on wave clear up to max 6), rim constraint (n_y >= 0.15).
- `BugPool.cs`: Cyber-bug spawn and geodesic distance tracking.
- `LaserSystem.cs`: Twin chin laser firing cadence (0.28s cooldown), 7.8° corridor hit query, multi-kill bonus (+150 for 3 kills).
- `WaveDirector.cs`: 5 wave progression, bug contact damage (at 36mm), win/fail transitions.
- `FeedbackRouter.cs`: 15 audio-visual events mapped to spatial cues.
- `DomeRig.cs`: MonoBehaviour runtime wiring.

## 3. Test Suite
- `AcceptanceTests.cs`: 14 tests verifying AT-01 through AT-14.
  - Automated tests: AT-01, AT-02, AT-03, AT-04, AT-05, AT-06, AT-07, AT-08, AT-09, AT-10, AT-11, AT-12, AT-14.
  - Deferred tests: AT-13 (scripted 18-bug wave replay on SoC).
