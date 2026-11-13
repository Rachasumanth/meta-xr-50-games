# Build Log - bey_cap_clash

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 05

### 1. Specification & Asset Provenance
- **Prop**: Rounded bottle cap, convex-down (`R = 16.0 mm`, `t = 0.5 mm`, `m = 2.0 g`).
- **Asset**: `071-alien-ufo-saucer.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/bey_cap_clash/Models/bey_cap.glb`.
- **Collider**: Convex dome contact patch, rim torus, spin sensor.

### 2. Architecture & Pure C# Domain Decoupling
- `BeyCapConfig.cs`: Physical constants (radii, stadium bounds, restitution e = 0.55, friction deceleration, dead spin threshold).
- `CapModel.cs`:
  - `PropCap`: Geometry fit for radius and rim plane tilt.
  - `TopEntity`: Flick spin conversion `omega = v_t / R`, tap impulse application, wobble exponential decay, dead spin timer, ring-out check.
  - `ClashResolver`: Elastic impact velocity exchange with coefficient e = 0.55, angular velocity ratio damage evaluation (> 1.25x deals 1 HP).
- `CapRig.cs`: Thin MonoBehaviour coordinating top entities and bout resolve.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Spin Law (1.0 m/s flick yields 62.5 ± 4 rad/s).
- AT02: Wobble Frequency (decay from 10° to < 1° in < 2.5s).
- AT03: Friction Decay (free spin at 100 rad/s decays at 0.44 ± 0.06 rad/s²).
- AT04: Tap Impulse (1.0 N moves top by 30 ± 4 mm/s).
- AT05: Tilt Max (floor tilt cap).
- AT06: Clash Impulse (restitution e = 0.55 ± 0.05).
- AT07: Spin Advantage (1.3x deals 1 damage, 1.1x deals 0).
- AT08: Ring-Out (crossing 75 mm triggers ring out).
- AT09: Dead Cap (omega < 8 rad/s for 500 ms ends bout).
- AT10: Tournament win/loss advance logic.
- AT11: Occlusion logic test.
- AT12: Pause functionality.
- AT13: Adaptive difficulty scaling.
- AT14: Performance benchmark.
