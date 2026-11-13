# Build Log: Bio-Sphere Terraformer (`bio-sphere-terraformer`)

## 1. Specifications & Metadata
- **Slug**: `bio-sphere-terraformer`
- **Family**: True sphere, palm-scale (6–12 cm class), stem pinch with free index planting
- **Prop**: Apple with stem, true sphere, 72.0 mm diameter (r = 36.0 mm), 180 g
- **Virtual Setup**: 72-cell globe with directional sun and rain sources

## 2. Implemented Components
- `BioSphereConfig.cs`: Sphere radius (36mm), 72 cells, biomass threshold (0.90), weed fail coverage (35%), season duration (90s).
- `PropApple.cs`: Sphere pose tracking, stem axis, lap support detection.
- `StemTwist.cs`: Stem pinch rotation (90–220 deg/s) and axis tilt.
- `ExposureField.cs`: Spherical solar/rain trigonometric exposure equations (t = tan phi * tan delta, G = 4*fSun*fRain).
- `CellSim.cs`: Biomass growth model, weed accretion and solar burning, germination and harvest cooldowns.
- `SeedBank.cs`: Seed transaction tracking, tree harvest (+3 seeds, +120 score).
- `SeasonDirector.cs`: 72-cell ecosystem lifecycle, weed blight failure condition (35%), win condition (>= 36 trees, <= 12% weeds).
- `FeedbackRouter.cs`: 15 audio-visual events mapped to spatial cues.
- `TerraformRig.cs`: MonoBehaviour runtime wiring.

## 3. Test Suite
- `AcceptanceTests.cs`: 14 tests verifying AT-01 through AT-14.
  - Automated tests: AT-01, AT-02, AT-03, AT-04, AT-05, AT-06, AT-07, AT-08, AT-09, AT-10, AT-12, AT-14.
  - Deferred tests: AT-11 (centrifugal uprooting), AT-13 (scripted season on SoC).
