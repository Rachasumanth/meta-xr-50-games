# BUILD LOG: Weeble Sumo (Chapter 4.2 - Game 02)

## Summary
- **Classification**: Prolate Spheroid ($a = b = 25\text{ mm}, c = 38\text{ mm}$), Self-Righting Sumo Egg simulation.
- **Model**: `763-chicken-egg.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/weeble_sumo/Models/weeble_egg.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropTracker`, `PressModel`, `RockingBody`, `LungeSolver`, `BellyCollider`, `ImpDirector`, `StabilityMeter`, `BoutStateMachine`, `ScoreCombo`, `FeedbackRouter`) wired to `WeebleRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Spheroid registration | Pass | Spheroid RMS fit < 0.5 mm, yaw error < 2.0° |
| AT-02 | Natural rocking frequency | Pass | $f = 1.872\text{ Hz}$, period $0.534\text{ s}$ verified over 10s integration |
| AT-03 | Damping ratio | Pass | $\zeta = 0.055$, strictly decaying positive damping |
| AT-04 | Press force proxy | Pass | $F = 0.50, 1.50, 3.00, 3.00\text{ N}$ across $\delta = 2, 6, 12, 16\text{ mm}$ |
| AT-05 | Press impulse & tilt | Pass | $J_\theta = 2.42\times 10^{-3}\text{ N}\cdot\text{m}\cdot\text{s}$, peak $\theta = 16.8^\circ$ |
| AT-06 | Lunge slide | Pass | $22\text{ mm}$ slide in direction $\lambda + 180^\circ$ on $\mu = 0.38$ |
| AT-07 | Belly bump | Pass | Imp speed $0.45\text{--}0.65\text{ m/s}$, bump score event |
| AT-08 | Ring-out | Pass | Imp crossed $130\text{ mm}$, exactly one ring-out, $+150$ points |
| AT-09 | Imp push | Pass | Reached $18\text{ mm}$, stability $-8$, tilt impulse $0.0012\text{ N}\cdot\text{m}\cdot\text{s}$ |
| AT-10 | Topple and reseat | Pass | Topple at $|\theta| > 40^\circ$, stability $-34$, reseat timer $3.0\text{ s}$ |
| AT-11 | Stability loss | Pass | Stability reaches 0 on 13th push, GAME_OVER on second bout lost |
| AT-12 | Occlusion readback | Pass | Fragment contribution $= 0$ inside $75\text{ mm}$ silhouette |
| AT-13 | Tracking loss | Pass | Model pauses at $1.5\text{ s}$ tracking loss |
| AT-14 | Soak run | Pass | 500-tick deterministic soak run with 0 energy blow-up |
