# BUILD LOG: Paddlewheel Pod (Chapter 4.2 - Game 10)

## Summary
- **Classification**: Prolate Spheroid ($a = b = 24.0\text{ mm}, c = 35.0\text{ mm}$), Equatorial Tangential Stroke & Steam Paddlewheel Ascent.
- **Model**: `081-tugboat.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/paddlewheel_pod/Models/paddlewheel_pod.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropEgg`, `PinchModel`, `SpinModel`, `SteerModel`, `EelSystem`, `KelpField`, `PodModel`) wired to `PodRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Egg registration | Pass | Spheroid RMS fit < 0.7 mm, axis error < 3.0° |
| AT-02 | Effective radius | Pass | $r_{\text{eff}} = 24.0\text{ mm}$ equatorial, $51.0\text{ mm}$ meridional |
| AT-03 | Equatorial stroke & spin cap | Pass | $300\text{ mm/s}$ stroke raises $\omega_s$ by $1.25\text{ rad/s}$, capped at $18\text{ rad/s}$ |
| AT-04 | Spin decay | Pass | Decays from $12$ to $4.0\text{ rad/s}$ in $1.9\text{ s}$ |
| AT-05 | Ascent speed & steam drain | Pass | $\omega_s = 12 \to v_{\text{up}} = 48\text{ mm/s}$, steam drains at $5.5/\text{s}$, holds at 0 |
| AT-06 | Lateral steer & shaft clamp | Pass | $22^\circ \to 90\text{ mm/s}$, $10^\circ \to 42\text{ mm/s}$, clamps at $300\text{ mm}$ |
| AT-07 | Eel windup & lunge | Pass | Windup $0.35\text{ s}$, lunge travels $40\text{ mm}$ at $220\text{ mm/s}$ |
| AT-08 | Eel pass hit & dodge | Pass | $50\text{ mm}$ pass costs 1 hull, 15 steam, 1.2s invuln; $60\text{ mm}$ dodge |
| AT-09 | Kelp field effects | Pass | Detection radius halved, $-3$ steam on entry, $75\%$ ascent |
| AT-10 | Steam empty sink | Pass | Steam 0 forces $24\text{ mm/s}$ sink |
| AT-11 | Pinch states | Pass | $65\text{--}78\text{ mm}$ Armed, $84\text{ mm}$ Slip, $86\text{ mm}$ Released |
| AT-12 | Cover freeze | Pass | 1s cover pauses without loss of state |
| AT-13 | Level 3 dive | Pass | Depth reduced with $\ge 2$ hull remaining |
| AT-14 | Session soak | Pass | 500-tick stable integration |
