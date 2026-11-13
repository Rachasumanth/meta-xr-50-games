# Build Log: Geyser Venting (`geyser-venting`)

## 1. Specifications & Metadata
- **Slug**: `geyser-venting`
- **Family**: True sphere, palm-scale (6–12 cm class), two-hand cradle
- **Prop**: Orange, true sphere, 76.0 mm diameter (r = 38.0 mm), 150 g
- **Virtual Setup**: Volcanic moon with 24-plate crust and 8 vent sockets

## 2. Implemented Components
- `GeyserVentingConfig.cs`: Sphere radius (38mm), hold durations (single 300ms, double 167ms), pressure/flux parameters.
- `PropOrange.cs`: Sphere tracking, dynamic rotation, cover detection.
- `CradleModel.cs`: Two-palm cradle grip validation.
- `ContactSampler.cs`: 4-effector contact tracking and filtered penetration proxy.
- `VentField.cs`: 8 vents, quadratic flux accumulation (sum F0*(a/18)^2), reachable cap timeout (8s -> 40% flux).
- `PressArbiter.cs`: Angular hold classification, decay on release (>100ms), bare-crust crack detection.
- `PressureIntegrator.cs`: Continuous pressure ODE, peak tracking, eruption detection.
- `CrustModel.cs`: 3-crack structural integrity, repair on phase clear.
- `PhaseDirector.cs`: 5-phase manager, 40s phase clock, unsealed vent timeout penalty (18.0/vent).
- `FeedbackRouter.cs`: 15 audio-visual events mapped to spatial cues.
- `VolcanoRig.cs`: MonoBehaviour runtime wiring.

## 3. Test Suite
- `AcceptanceTests.cs`: 14 tests verifying AT-01 through AT-14.
  - Automated tests: AT-01, AT-02, AT-03, AT-04, AT-05, AT-06, AT-07, AT-08, AT-09, AT-10, AT-11, AT-12, AT-14.
  - Deferred tests: AT-13 (scripted route SoC benchmark).
