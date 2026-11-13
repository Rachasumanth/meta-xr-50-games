# Build Log - tardigrade_pond

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 06

### 1. Specification & Asset Provenance
- **Prop**: Single contact-lens case bowl, concave-up (`R = 14.0 mm`, `t = 1.0 mm`, `R_i = 13.0 mm`, `m = 3.0 g`).
- **Asset**: `876-petri-dish.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/tardigrade_pond/Models/tardigrade_pond.glb`.
- **Collider**: Concave inner hemisphere cavity, rim torus, four rim tap sensors.

### 2. Architecture & Pure C# Domain Decoupling
- `TardigradePondConfig.cs`: Physical constants (radii, wave speed 90 mm/s, transit time 139 ms, damping tau 0.71s, algae radius 4.2 mm, creature speed 26 mm/s).
- `PondModel.cs`:
  - `PropBowl`: Hemisphere least squares fitting and cavity axis check.
  - `WaveField`: Tap amplitude mapping (0.55 mm to 1.20 mm), exponential wave damping.
  - `Creature`: Wave gradient advection, immunity window, health model, algae attachment.
  - `Mine`: Reduced mobility (35%) advection.
- `PondRig.cs`: Thin MonoBehaviour handling state updates and scene entities.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Registration (20/20 presentations).
- AT02: Tap Amplitude (300 mm/s -> 1.20 mm, 60 mm/s -> 0.55 mm, off-pad 0.35x).
- AT03: Wavefront Transit (139 ms).
- AT04: Damping Time (1.0 mm to 0.37 mm in 0.71s).
- AT05: Creature Push (moves 7.0 ± 1.5 mm, speed <= 26 mm/s).
- AT06: Mine Push (moves 2.4 ± 0.8 mm, 35% mobility).
- AT07: Mine Contacts (3 hits to kill, 0.6s immunity).
- AT08: Algae Save (within 4.2 mm attaches, scores +120).
- AT09: Timer Expiry (42s droplet timer).
- AT10: Lifting Freeze.
- AT11: Outer Dome Tap rejection.
- AT12: Performance benchmark (8 creatures + 4 mines).
- AT13: Occlusion logic test.
- AT14: Session Endurance test.
