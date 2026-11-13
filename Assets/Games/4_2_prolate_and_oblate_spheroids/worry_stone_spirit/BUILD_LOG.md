# BUILD LOG: Worry Stone Spirit (Chapter 4.2 - Game 03)

## Summary
- **Classification**: Oblate Spheroid ($a = 35\text{ mm}, b = 25\text{ mm}, c = 10\text{ mm}$), Elliptical Thumb Stroking & Spirit Constellations.
- **Model**: `933-smooth-river-stones.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/worry_stone_spirit/Models/river_stone.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropTracker`, `AbrasionField`, `ThumbKinematics`, `FragmentField`, `TideController`, `TideStateMachine`, `ScoreCombo`, `FeedbackRouter`) wired to `StoneRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Spheroid registration | Pass | Spheroid RMS fit < 0.5 mm, yaw error < 2.0° |
| AT-02 | Heightmap initialization | Pass | $h = 1.20\text{ mm}$, layer index = 5 across top cap |
| AT-03 | Abrasion rate | Pass | $h$ falls $0.36\text{ mm}$ over 2.0 s, layer breakthrough at $1.11\text{ s}$ |
| AT-04 | Pressure saturation | Pass | Pressure clamps at $0.55\text{ N}$, hardness rises at $0.25/\text{s}$ |
| AT-05 | Smooth-oval classifier | Pass | Oval sets $S = 1.25$, linear scrub sets $S = 0.85$ |
| AT-06 | Geodesic footprint | Pass | $6.5\text{ mm}$ chord disc contact without polar distortion |
| AT-07 | Layer indexing | Pass | $0.40\text{ mm}$ removal decrements layer from 5 to 3 |
| AT-08 | Fragment reveal | Pass | Fragment reveals at mean $h < 0.15\text{ mm}$, score $+200$, latches |
| AT-09 | Tide front | Pass | $\phi_t$ matches $-80^\circ, +2.5^\circ, +85^\circ$; $\phi=30^\circ$ lost at $31.8\text{ s}$ |
| AT-10 | Constellation recording | Pass | Connected component of 7 yields constellation, $+500 + 10\cdot\text{time}$ |
| AT-11 | Glaze recovery | Pass | Hardness reaches 2.0 under high load, decays back to 1.0 at gentle load |
| AT-12 | Occlusion readback | Pass | Fragment contribution $= 0$ under thumb silhouette |
| AT-13 | Tracking loss | Pass | Tide pauses at $1.5\text{ s}$ loss |
| AT-14 | Soak run | Pass | 500-tick soak test with heights strictly in $[0, 1.20]\text{ mm}$ |
