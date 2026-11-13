# Build Log: Foghorn Buoy (`foghorn_buoy`)

## Implementation Summary
- **Model**: `foghorn_buoy.glb` (source `702-soup-bowl.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/foghorn_buoy/Models/foghorn_buoy.glb`).
- **Domain Simulation**: Pure C# `FoghornSimulation`, `RockPendulum`, `SharkAgent`, and `SwellEvent`.
  - Inverted pendulum dynamics: $I_{eff} = 2.1 \times 10^{-3}\text{ kg}\cdot\text{m}^2$, $d_{com} = 26\text{ mm}$, tuned rock frequency $0.55\text{ Hz}$ ($\pm 0.06\text{ Hz}$).
  - Press torque: 2 N south lip press produces $0.16\text{ N}\cdot\text{m}$ restoring torque.
  - Counter-rock: opposite press reduces tilt by $\ge 60\%$ within 0.4 s.
  - Swell & warning: swell peaks at telegraph + 1.2 s; $|\theta| > 20^\circ$ triggers warning; $|\theta| > 28^\circ$ for 150 ms costs one life.
  - Sharks & foghorn: sharks close at $12 + 0.8|\theta|\text{ mm/s}$; reaching $< 40\text{ mm}$ fails storm. Foghorn repels sharks within 200 mm by 120 mm, costing 15 air (regenerating at 8/s).
  - Drift soft-clamp: 16 mm drift clamped to 15 mm.
- **Rig**: `FoghornRig` MonoBehaviours connecting hand tracking inputs and visual 3x tilt exaggeration.
- **Acceptance Tests**: 14/14 tests implemented strictly following Section 16 specifications.
