# BUILD LOG: Sub-Surface Hovercraft (Chapter 4.2 - Game 01)

## Summary
- **Classification**: Oblate Spheroid ($a = b = 31\text{ mm}, c = 19\text{ mm}$), Palm-Scale Hovercraft simulation.
- **Model**: `091-hovercraft.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/sub_surface_hovercraft/Models/hovercraft.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropTracker`, `ContactBus`, `PlanarDrive`, `DepthSonar`, `HarpoonSystem`, `HazardDirector`, `ScoreCombo`, `ChannelStateMachine`, `FeedbackRouter`) wired to `HovercraftRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Spheroid registration | Pass | Spheroid RMS fit < 0.6 mm, yaw error < 1.5° |
| AT-02 | Drive terminal speed | Pass | Physical speed 0.142 m/s, virtual speed 0.426 m/s (3.0x gain) |
| AT-03 | Static friction breakaway | Pass | Static breakaway at F = 0.391 N, motion within 0.12 s |
| AT-04 | Clutch freeze & recenter | Pass | 150 ms freeze, 32 mm recentering offset |
| AT-05 | Harpoon ballistics range | Pass | 0.42 s lifetime, 300 mm range |
| AT-06 | Depth gating | Pass | Mismatched depth emits thud, matched depth inflicts 34 damage |
| AT-07 | Scroll detent | Pass | 3.1 mm triggers depth band step, 2.8 mm rejected with 40 ms debounce |
| AT-08 | Drift impulse & heat | Pass | Lateral Δv = 0.21 m/s, +26 heat, low friction for 0.45 s |
| AT-09 | Overheat lockout | Pass | 5 drifts reaches 100 heat, 3.0 s lockout, 0.5 N thrust cap |
| AT-10 | Combo multiplier | Pass | 4 chained hits reach 2.0x multiplier, miss resets |
| AT-11 | Hull loss & Game Over | Pass | 4 mines at 25 HP each trigger GAME_OVER |
| AT-12 | Occlusion readback | Pass | Spheroid silhouette occlusion depth readback |
| AT-13 | Tracking loss recovery | Pass | Thrust zeroed after 200 ms, pause at 1.5 s |
| AT-14 | Soak run | Pass | 500-tick deterministic soak run with 0 tunnelling |
