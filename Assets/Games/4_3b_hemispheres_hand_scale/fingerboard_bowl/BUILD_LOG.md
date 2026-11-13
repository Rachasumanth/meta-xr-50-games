# Build Log - fingerboard_bowl

## Chapter 4.3b: Hemispheres (Hand Scale) - Game 03

### 1. Specification & Asset Provenance
- **Prop**: Empty cereal bowl, concave-up (`R = 75.0 mm`, `depth = 55.0 mm`, `t = 4.0 mm`, `m = 380 g`).
- **Asset**: `702-soup-bowl.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/fingerboard_bowl/Models/skate_bowl.glb`.
- **Collider**: Concave inner hemisphere, rim torus, foot ring.

### 2. Architecture & Pure C# Domain Decoupling
- `FingerboardConfig.cs`: Physical constants (radii, depth, terminal speed 3.9 m/s at 30°, launch thresholds, ballistic gravity 2.4 m/s², trick distances).
- `SkateModel.cs`:
  - `PropBowl`: Spherical cap geometry fitting and fingertip mapping.
  - `SkaterParticle`: Terminal wall velocity, launch eligibility, ballistic airtime calculation (0.833s at 1.0 m/s).
  - `TrickClassifier`: Dual fingertip span classification (Kickflip <= 8 mm, HandPlant >= 40 mm).
  - `LandingJudge`: Landing synchronization window (clean <= 180 ms, bail > 180 ms) and flat slam detector.
- `SkateRig.cs`: Thin MonoBehaviour handling scene skater visual and score state.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Projection (re-projects within 1.5 mm).
- AT02: Terminal Speed (3.9 ± 0.2 m/s at 30°).
- AT03: Launch Eligibility (rejects 35°, launches 40° at 1.8 m/s).
- AT04: Airtime (0.833s at 1.0 m/s with g = 2.4).
- AT05: Kickflip (crossed tips <= 8 mm).
- AT06: Hand-Plant (spread >= 40 mm).
- AT07: Landing (180 ms clean, 250 ms bails).
- AT08: Slam (theta < 4° at v > 2.0 m/s).
- AT09: Gate passing bonus (+80).
- AT10: Combo multiplier (1.25x).
- AT11: Lives loss condition (3 bails).
- AT12: Re-anchor pause state.
- AT13: Occlusion logic test.
- AT14: Performance benchmark.
