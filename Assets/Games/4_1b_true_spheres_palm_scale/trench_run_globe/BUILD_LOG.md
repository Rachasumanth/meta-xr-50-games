# Build Log: Trench Run Globe (`trench-run-globe`)

## 1. Specifications & Metadata
- **Slug**: `trench-run-globe`
- **Family**: True sphere, palm-scale (6–12 cm class), one-hand spin with stationary read head
- **Prop**: Tennis ball, true sphere, 67.0 mm diameter (r = 33.5 mm), 58 g
- **Virtual Setup**: 160 km battle station trench wrapped along great circle, 1:1 on felt

## 2. Implemented Components
- `TrenchRunGlobeConfig.cs`: Ball radius (33.5mm), lap length (210.49mm), two laps port arming (420.97mm), speed and telegraph limits.
- `PropTennis.cs`: Sphere pose tracking, dynamic rotation odometry, cover detection.
- `ReadHeadModel.cs`: Contact gating, u/v down-range and cross-track unwrapping, airspeed integration, hard press brake (0.35x), lift decay.
- `TrenchMap.cs`: Great circle trench map, exhaust port gate after 2 laps.
- `TowerArray.cs`: Turbolaser tower placement, 10–38mm telegraph window, bolt projectile spawning.
- `BoltSystem.cs`: Projectile impact arbitration (hit if |v| <= 3.2mm, longitudinal < 5mm).
- `PortJudge.cs`: Exhaust port dive classification (40–170 mm/s and |v| <= 1.9mm) vs overshoot.
- `SectorDirector.cs`: 5 sectors, 3 shield pips, wall scrape damage.
- `FeedbackRouter.cs`: 15 audio-visual events mapped to spatial cues.
- `StationRig.cs`: MonoBehaviour runtime wiring.

## 3. Test Suite
- `AcceptanceTests.cs`: 14 tests verifying AT-01 through AT-14.
  - Automated tests: AT-01, AT-02, AT-03, AT-04, AT-05, AT-06, AT-07, AT-08, AT-09, AT-10, AT-11, AT-13, AT-14.
  - Deferred tests: AT-12 (scripted 6-tower run replay on SoC).
