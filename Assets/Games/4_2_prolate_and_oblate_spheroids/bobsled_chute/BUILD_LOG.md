# BUILD LOG: Bobsled Chute (Chapter 4.2 - Game 05)

## Summary
- **Classification**: Prolate Spheroid ($a = b = 20\text{ mm}, c = 42\text{ mm}$), Serpentine Ice Chute & Banked Sled Racing.
- **Model**: `096-dog-sled.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/bobsled_chute/Models/bobsled.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropTracker`, `PalmFrame`, `SlideControl`, `BankControl`, `ChuteModel`, `SledBody`, `SplitTimer`, `DescentStateMachine`, `ScoreCombo`, `FeedbackRouter`) wired to `BobsledRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Soap registration | Pass | Spheroid RMS fit < 0.6 mm, yaw error < 2.0° |
| AT-02 | Palm frame | Pass | Orthonormal frame generated from wrist/MCP joints |
| AT-03 | Slide mapping | Pass | Clamps $\pm 20\text{ mm}$, deadzone $2\text{ mm}$, filter settles in $0.18\text{ s}$ |
| AT-04 | Neutral run | Pass | Finish $0.99\text{ s}$, terminal speed $1.26\text{ m/s}$ at $900\text{ mm}$ |
| AT-05 | Full tuck | Pass | Finish $0.70\text{ s}$, exit speed $2.00\text{ m/s}$ at $900\text{ mm}$ |
| AT-06 | Full brake | Pass | Stops in $0.49\text{ m}$ from $1.26\text{ m/s}$, does not reverse |
| AT-07 | Bank gravity | Pass | $10^\circ$ bank yields $-1.70\text{ m/s}^2$ lateral acceleration |
| AT-08 | Hairpin load | Pass | $v = 1.3\text{ m/s}, \kappa = 2.42, 10^\circ$ bank $\to a_{\text{lat}} = 2.36\text{ m/s}^2$, no flip |
| AT-09 | Flip threshold | Pass | $v = 2.1\text{ m/s}, 0^\circ$ bank $\to a_{\text{lat}} > 6.5\text{ m/s}^2$, flip at $80\text{ ms}$ |
| AT-10 | Rim collision | Pass | $y = 25\text{ mm}$ clamps to $24\text{ mm}$, speed x0.65, $+0.25\text{ s}$, crash counted |
| AT-11 | Split timing | Pass | Latches 3 gates, sum of sectors equals total time |
| AT-12 | Best of three | Pass | Accurately extracts best heat (e.g. $1.32\text{ s}$) from 3 attempts |
| AT-13 | Occlusion readback | Pass | Sled fragment contribution $= 0$ under hand silhouette |
| AT-14 | Tracking loss | Pass | Sled control pauses after tracking loss |
