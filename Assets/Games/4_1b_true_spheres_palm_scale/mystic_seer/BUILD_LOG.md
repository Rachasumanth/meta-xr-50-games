# Build Log: Mystic Seer (`mystic_seer`)

## 1. Specifications & Metadata
- **Slug**: `mystic_seer`
- **Family**: True sphere, palm-scale (6–12 cm class), stationary scrying orb
- **Prop**: Apple or smooth rubber ball, true sphere, r = 36.0 mm (72 mm diameter)
- **Virtual Setup**: Fog shell 0.8 mm above surface; hand envelope 120 mm above ball

## 2. Implemented Components
- `MysticSeerConfig.cs`: Sphere radius (36mm), fog equilibrium (0.05 s^-1), aura regen (0.20 s^-1), tap distance/height thresholds.
- `PropTracker.cs`: Sphere pose tracking, rest disc verification on support surface.
- `HandShell.cs`: 26 joint spherical projection, standoff calculation, curvature gain.
- `FogField.cs`: 128x64 spherical density grid, Gaussian splat dissipation, aura regeneration, wrong-tap smothering.
- `ThiefDirector.cs`: Spectral thief spawning, 100ms clear-sight reveal gate, 10s escape timers, 3-relic life pool.
- `TapJudge.cs`: Lateral (< 10mm) and height (<= 5mm) tap classification, banish vs smother handling.
- `SeanceScoring.cs`: Base score, clarity bonus, wrong-tap penalties (-25).
- `RoundStateMachine.cs`: 75s round lifecycle, escape counting, game over / round clear transitions.
- `FeedbackRouter.cs`: 13 distinct audio-visual feedback events mapped to spatial cues.
- `MysticSeerRig.cs`: MonoBehaviour runtime wiring.

## 3. Test Suite
- `AcceptanceTests.cs`: 14 tests verifying AT-01 through AT-14.
  - Automated tests: AT-01, AT-02, AT-03, AT-04, AT-05, AT-06, AT-07, AT-08, AT-09, AT-10, AT-13, AT-14.
  - Deferred tests: AT-11 (fog determinism SHA-256), AT-12 (fog occlusion depth buffer).
