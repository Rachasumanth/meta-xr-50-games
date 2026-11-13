# BUILD LOG: Scarab Buckler (Chapter 4.2 - Game 04)

## Summary
- **Classification**: Oblate Spheroid ($a = b = 45\text{ mm}, c = 18\text{ mm}$), Optical Fan Reflection & Swarm Defense.
- **Model**: `354-heater-knight-shield.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/scarab_buckler/Models/scarab_buckler.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropTracker`, `BucklerOptics`, `FanSolver`, `WristAim`, `HeatCharge`, `BeamDirector`, `TargetField`, `WaveStateMachine`, `ScoreCombo`, `FeedbackRouter`) wired to `BucklerRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Spheroid registration | Pass | Spheroid RMS fit < 0.6 mm, yaw error < 2.0° |
| AT-02 | Reflection law | Pass | $r = (0,0,+1)$, energy 0.92 specular reflection |
| AT-03 | Fan spread centred | Pass | Centered beam generates $-22.9^\circ, -11.5^\circ, 0^\circ, +11.5^\circ, +22.9^\circ$ |
| AT-04 | Fan spread offset | Pass | Offset $18\text{ mm}$ produces one-sided fan $+11.5^\circ$ to $+57.3^\circ$ |
| AT-05 | Target cone | Pass | Target at $2.9^\circ$ hits once, $3.1^\circ$ misses outside $3.0^\circ$ cone |
| AT-06 | Wrist aim gain | Pass | Wrist roll $6^\circ \to 16.8^\circ$, $25^\circ \to 70.0^\circ$ with $1.5^\circ$ deadzone |
| AT-07 | Heat accumulation | Pass | 5 deflections over $0.5\text{ s}$ yield heat $55$, cooling at $10/\text{s}$ |
| AT-08 | Overheat lockout | Pass | 9 deflections trigger heat 100 with $2.5\text{ s}$ lockout |
| AT-09 | Charge nova | Pass | Charge 100 triggers 24-ray nova, targets $< 300\text{ mm}$ destroyed, heat $+20$ |
| AT-10 | Venting | Pass | Double-tap reduces heat by 40, charge by 30 without overheating |
| AT-11 | Miss damage | Pass | Hull $-12$ on lance, $-20$ on heavy, combo resets |
| AT-12 | Rim scatter | Pass | $\phi = 45^\circ$ triggers rim scatter in bevel |
| AT-13 | Occlusion readback | Pass | Target fragment contribution $= 0$ inside silhouette |
| AT-14 | Tracking loss | Pass | Buckler pauses at $0.8\text{ s}$ tracking loss |
