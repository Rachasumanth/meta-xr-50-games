# BUILD LOG: Vortex Wobbler (Chapter 4.2 - Game 06)

## Summary
- **Classification**: Oblate Spheroid ($a = b = 30\text{ mm}, c = 12\text{ mm}$), Precessing Rim Wobble & Gravity Airlock Whirlpool.
- **Model**: `071-alien-ufo-saucer.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/vortex_wobbler/Models/vortex_wobbler.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropPebble`, `RockModel`, `PressClassifier`, `VortexField`, `FieldEntity`, `AirlockModel`, `WaveDirector`) wired to `VortexRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Pebble registration | Pass | Spheroid RMS fit < 0.5 mm, yaw error < 3.0° |
| AT-02 | Rim walk pumping | Pass | In-phase walk $+6.0^\circ$, out-of-phase walk $-6.0^\circ$ |
| AT-03 | Amplitude decay | Pass | Decays from $18^\circ$ to $6.6^\circ$ in $1.40\text{ s}$ ($\tau = 1.4\text{ s}$) |
| AT-04 | Consecutive pumps | Pass | 3 pumps reach $12\text{--}20^\circ$ stable band without exceeding $26^\circ$ |
| AT-05 | Vortex field & infall | Pass | $\Gamma = 1.2\times 10^4\text{ mm}^2/\text{s}$, $v_\theta = 48\text{ mm/s}$, infall $1.8\text{ s}$ |
| AT-06 | Deflection wedge | Pass | Debris pushed $> 68\text{ mm}$, pod moves $< 30\%$ of displacement |
| AT-07 | Airlock scoring & damage | Pass | Debris breach $-1$ hull $+18$ heat, pod dock $+100$ score |
| AT-08 | Overdrive heat & inflow | Pass | $A = 24^\circ$ doubles inflow speed, raises heat at $6/\text{s}$ |
| AT-09 | Deflection spam drain | Pass | Spam drains amplitude at $3.6^\circ/\text{s}$ |
| AT-10 | Cover freeze | Pass | 1s cover pauses without state loss |
| AT-11 | Sector classification | Pass | 24 sectors resolved with $7.5^\circ$ hysteresis guard |
| AT-12 | Level 2 clearable | Pass | Wave 2 clearable with $\ge 2$ hull remaining |
| AT-13 | Session duration soak | Pass | 500-tick stable integration |
| AT-14 | Hand removal field off | Pass | 5s hand loss decays $A < 1^\circ$ setting field to OFF |
