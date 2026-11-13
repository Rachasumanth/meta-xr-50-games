# Build Log - galaxy_cauldron

## Chapter 4.3b: Hemispheres (Hand Scale) - Game 01

### 1. Specification & Asset Provenance
- **Prop**: Empty cereal/soup bowl, concave-up (`R = 75.0 mm`, `d = 55.0 mm`, `t = 4.0 mm`, `m = 380 g`).
- **Asset**: `702-soup-bowl.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/galaxy_cauldron/Models/galaxy_cauldron.glb`.
- **Collider**: Spherical cap cavity, rim torus, outer spherical cap.

### 2. Architecture & Pure C# Domain Decoupling
- `GalaxyCauldronConfig.cs`: Physical constants (radii, depth, rim truncation 74.53°, geodesic rim distance 97.58 mm, stir dead zones [12, 18, 19] mm/s, duration 90s).
- `CauldronModel.cs`:
  - `PropBowl`: Spherical cap fitting and 2D disc projection.
  - `StirAnalyzer`: Angular stir velocity `- (x * vy - y * vx) / r^2`, speed hysteresis window, depth-dependent fusion radius.
  - `Cloud`: Stellar fusion (matching species) and supernova (mismatched species) interactions.
  - `DarkMatterSeed`: Radial integration and comet ejection.
- `CauldronRig.cs`: Thin MonoBehaviour coordinating visual nebula entities and game clock.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Prop Fit (R, d, rim RMS).
- AT02: Cap Projection (apex -> 0.0 mm, rim -> 97.58 mm).
- AT03: Stir Sign (100 mm/s at x = 50 yields -2.0 rad/s clockwise).
- AT04: Dead Zone (inactive <= 18 mm/s, active >= 19 mm/s, hysteresis to 12 mm/s).
- AT05: Depth Gains (12.0 mm at h = 10, 15.0 mm at h = 45).
- AT06: Fusion (matching clouds form star).
- AT07: Nova (mismatched clouds nova, luminosity -12).
- AT08: Comet (rho = 86 mm comet, rim ejection).
- AT09: Clock (90s total duration).
- AT10: Occlusion logic test.
- AT11: Adaptive tuning (nova expansion).
- AT12: Frame budget (particle count <= 2400).
- AT13: Tracking recovery (<= 1.5s).
- AT14: Session endurance test.
