# Build Log - paratrooper_basket

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 07

### 1. Specification & Asset Provenance
- **Prop**: Inverted bottle cap, concave-up (`R = 15.0 mm`, `t = 0.5 mm`, `R_net = 13.0 mm`, `m = 2.0 g`).
- **Asset**: `064-parachute.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/paratrooper_basket/Models/paratrooper_basket.glb`.
- **Collider**: Concave cavity, rim torus, virtual trampoline net.

### 2. Architecture & Pure C# Domain Decoupling
- `ParatrooperBasketConfig.cs`: Physical constants (radii, net restitution e = 0.45, speed bounds [-260, -120] mm/s, tilt limit 20°, ground miss 40 mm).
- `BasketModel.cs`:
  - `PropCap`: Registration and rim orientation fitter.
  - `NetCollider`: Tilt-dependent effective net radius `R_eff = R_net * cos(beta)`.
  - `Trooper`: Kinematic integration, landing evaluation, soft catch score multipliers, rebound and recovery mechanics.
- `BasketRig.cs`: Thin MonoBehaviour handling scene updates and score ledger.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Catch Geometry (r = 12 mm caught, r = 15 mm missed).
- AT02: Speed Window (passes -100 mm/s, catches -200 mm/s, rejects -300 mm/s).
- AT03: Tilt Rule (beta = 25° -> R_eff = 0).
- AT04: Soft Catch Score (1.5x multiplier at -140 mm/s).
- AT05: Bounce Chain (2 bounces to recover).
- AT06: Wind Telegraph (1.5s telegraph).
- AT07: Miss detection below z = 40 mm.
- AT08: Guide Ping penalty (-25 points).
- AT09: Release pause window (1.2s).
- AT10: Soft boundary recovery (0.6s).
- AT11: Drop Timer (300 s).
- AT12: Occlusion logic test.
- AT13: Adaptive difficulty scaling.
- AT14: Performance benchmark.
