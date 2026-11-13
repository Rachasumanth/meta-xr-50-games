# Build Log: Katamari Harvest (`katamari_harvest`)

## 1. Specifications & Metadata
- **Slug**: `katamari_harvest`
- **Family**: True sphere, palm-scale (6–12 cm class), rolling on support surface
- **Prop**: Tennis ball, true sphere, r = 33.5 mm (67 mm diameter)
- **Virtual Field**: 300 × 220 mm meadow tabletop surface

## 2. Implemented Components
- `KatamariHarvestConfig.cs`: Mass floor (60), radius clamp (14–60mm), chain timeout (1.2s), season duration (90s).
- `PropTracker.cs`: Sphere pose tracking, seam orientation, tray registration.
- `PalmContact.cs`: Multi-joint palm contact filter, tangent velocity computation.
- `TwoSurfaceKinematics.cs`: Two-surface no-slip rolling kinematics (v_C = v_palm / 2, omega = (n × v_palm) / (2r)).
- `StrokeAnalyzer.cs`: Circular stroke radius and quality factor (Q >= 0.9).
- `StickySphere.cs`: Mass-to-radius growth law R = 22 * (M/100)^(1/3), mine damage (12% loss).
- `MeadowDirector.cs`: Entity seeding (sheep, fences, barns, mines), respawn floor (>= 45 active), >60mm clearance.
- `HarvestResolver.cs`: Capture boundary calculation, mass/score calculation, chain multiplier scaling.
- `SeasonStateMachine.cs`: 90-second season lifecycle, quota evaluation, off-tray pause/resume.
- `FeedbackRouter.cs`: 13 distinct audio-visual feedback events mapped to spatial cues.
- `KatamariHarvestRig.cs`: MonoBehaviour runtime wiring.

## 3. Test Suite
- `AcceptanceTests.cs`: 14 tests verifying AT-01 through AT-14.
  - Automated tests: AT-01, AT-02, AT-03, AT-04, AT-05, AT-06, AT-07, AT-08, AT-09, AT-10, AT-12, AT-14.
  - Deferred tests: AT-11 (ball occlusion depth buffer), AT-13 (deterministic replay trace hash).
