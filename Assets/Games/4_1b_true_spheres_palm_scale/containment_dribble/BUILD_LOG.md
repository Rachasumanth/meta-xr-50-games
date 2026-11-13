# Build Log: Containment Dribble (`containment-dribble`)

## 1. Specifications & Metadata
- **Slug**: `containment-dribble`
- **Family**: True sphere, palm-scale (6–12 cm class), one-hand hold with rhythmic fingertip taps
- **Prop**: Foam stress ball, true sphere, 64.0 mm diameter (r = 32.0 mm), 60 g
- **Virtual Setup**: Vertical anti-gravity launch shaft (480 mm) with fireflies and ceiling spikes

## 2. Implemented Components
- `ContainmentDribbleConfig.cs`: Ball radius (32mm), gravity (9.81 m/s^2), rhythm window (0.18s), double-tap fusion window (50ms).
- `PropFoam.cs`: Sphere pose tracking and orientation frame.
- `TapClassifier.cs`: Lateral sector mapping (C/L/R), tap duration and peak penetration thresholds, double-tap fusion (1.12x).
- `SpringModel.cs`: Spring compression (up to 18mm), quadratic rhythm multiplier (1.25 -> 0.30), ballistic apex calculation.
- `DragonBody.cs`: Height, vertical velocity, launch resolution, unassisted landing bounce (55%), spike rebound.
- `FireflyPool.cs`: 7 firefly agents, 26mm collection radius.
- `SpikeBank.cs`: 3-lane descending spike plates with 40mm retraction on hit.
- `RunDirector.cs`: 60.0s run clock (5,400 ticks at 90Hz), shield pool (3 hearts), combo tracking, win condition (>= 20 fireflies).
- `FeedbackRouter.cs`: 15 audio-visual events mapped to spatial cues.
- `ShaftRig.cs`: MonoBehaviour runtime wiring.

## 3. Test Suite
- `AcceptanceTests.cs`: 14 tests verifying AT-01 through AT-14.
  - Automated tests: AT-01, AT-02, AT-03, AT-04, AT-05, AT-06, AT-07, AT-08, AT-09, AT-10, AT-11, AT-12, AT-14.
  - Deferred tests: AT-13 (scripted 25-tap run replay on SoC).
