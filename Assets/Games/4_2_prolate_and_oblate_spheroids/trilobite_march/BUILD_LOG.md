# BUILD LOG: Trilobite March (Chapter 4.2 - Game 08)

## Summary
- **Classification**: Oblate Dome ($a = 32\text{ mm}, b = 24\text{ mm}, c = 14\text{ mm}$), Three-Finger Metachronal Gallop & Defensive Curling.
- **Model**: `188-crab.glb` copied to `Assets/Games/4_2_prolate_and_oblate_spheroids/trilobite_march/Models/trilobite.glb`.
- **Implementation**: Pure decoupled C# domain logic (`PropMouse`, `TapDetector`, `GallopClassifier`, `GaitModel`, `CurlModel`, `MigrationDirector`) wired to `TrilobiteRig`.
- **Test Suite**: 14 automated unit and integration tests covering AT-01 through AT-14.

## Acceptance Tests Status
| Test ID | Description | Status | Verification Detail |
|---|---|---|---|
| AT-01 | Dome registration | Pass | Spheroid RMS fit < 0.8 mm, axis error < 4.0° |
| AT-02 | Tap classification | Pass | $70\text{ ms}$ is tap, $150\text{ ms}$ is press, $15\text{ mm}$ outside rejected |
| AT-03 | Gallop triplet quality | Pass | 90/90 ms yields $q = 1.0$, 55/125 ms yields $q = 0.61$ |
| AT-04 | Distinct fingers stumble | Pass | Three distinct fingers ($I-M-R$) produce stumble, not gallop |
| AT-05 | Stride advance & tau | Pass | Advances exactly $24\text{ mm}$, speed reaches target with $\tau = 0.25\text{ s}$ |
| AT-06 | Cadence & streak | Pass | $3\text{ Hz}$ at $q=0.9 \to 64.8\text{ mm/s}$, 10 gallops triggers x1.2 streak |
| AT-07 | Sink timer & recovery | Pass | $> 1.6\text{ s}$ idleness causes sink, gallop reduces sink by 0.35 |
| AT-08 | Two-pad curl | Pass | Two pads held $120\text{ ms}$ curls, one pad does not |
| AT-09 | Sweep protection window | Pass | $0.60\text{ s}$ overlap protects, $0.55\text{ s}$ does not |
| AT-10 | Missed curl plate loss | Pass | Missed curl costs 1 plate |
| AT-11 | Checkpoint & plate bonus | Pass | Checkpoint at $800\text{ mm}$ restores 1 plate ($+100$), excess awards $+50$ |
| AT-12 | Cover freeze | Pass | 1.5s cover pauses march without state loss |
| AT-13 | Level 3 migration | Pass | Clearable with $\ge 2$ plates |
| AT-14 | Session soak | Pass | 500-tick stable integration |
