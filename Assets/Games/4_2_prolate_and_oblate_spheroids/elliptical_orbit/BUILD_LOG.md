# BUILD LOG: Elliptical Orbit (Chapter 4.2 - Game 07)

## Summary
- **Classification**: Prolate Spheroid ($a = b = 22.5\text{ mm}, c = 31.0\text{ mm}$), Rolling Asymmetric Curvature & Keplerian Dust Orbiting.
- **Model**: `067-space-shuttle.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/elliptical_orbit/Models/space_shuttle.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropEgg`, `RollModel`, `NudgeClassifier`, `DustSystem`, `TailSystem`, `OrbitTracker`, `OrbitJudge`) wired to `CoreRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Egg registration | Pass | Spheroid RMS fit < 0.7 mm, axis error < 4.0° |
| AT-02 | Effective rolling radius | Pass | $r_{\text{eff}} = 42.71\text{ mm}$ at $\psi = 0^\circ$, $22.50\text{ mm}$ at $\psi = 90^\circ$ |
| AT-03 | Prograde nudge & token | Pass | $3.0\text{ mN}\cdot\text{s}$ changes speed by $40\text{ mm/s}$ on $75\text{ g}$ egg, spends 1 token |
| AT-04 | Rolling friction travel | Pass | Travels $295\text{ mm}$ from $160\text{ mm/s}$ before stopping |
| AT-05 | Curvature radii | Pass | $\alpha = +45^\circ \to R = 50\text{ mm}$, $\alpha = 0^\circ \to R = 125\text{ mm}$, $\alpha = -45^\circ$ reverses sign |
| AT-06 | Flank nudge alpha | Pass | $3.0\text{ mN}\cdot\text{s}$ at $20\text{ mm}$ lever changes $\alpha$ by $0.27\text{ rad}$ |
| AT-07 | Dust capture radius | Pass | Dust at $14.0\text{ mm}$ captured, $15.5\text{ mm}$ rejected |
| AT-08 | Tail spline cut | Pass | Transverse crossing cuts tail, tangential crossing ($< 30^\circ$) does not |
| AT-09 | Orbit closure bonus | Pass | $\ge 300^\circ$ unwrapped and $\le 24\text{ mm}$ return scores $+300$, $27\text{ mm}$ does not |
| AT-10 | Ellipticity scoring | Pass | $e = 0.35 \to +200$ points, $e = 0.10$ and $e = 0.60$ score $< 60$ |
| AT-11 | Stall detection | Pass | $< 25\text{ mm/s}$ for $1.2\text{ s}$ triggers stall |
| AT-12 | Cover freeze | Pass | 1s cover pauses without loss of state |
| AT-13 | Level 3 orbit | Pass | Orbit closed with dust particles captured |
| AT-14 | Session soak | Pass | 500-tick stable integration |
