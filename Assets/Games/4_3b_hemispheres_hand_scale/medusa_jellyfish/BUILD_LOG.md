# Build Log: Medusa Jellyfish (`medusa_jellyfish`)

## Implementation Summary
- **Model**: `jellyfish.glb` (source `186-jellyfish.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/medusa_jellyfish/Models/jellyfish.glb`).
- **Domain Simulation**: Pure C# `JellyfishSimulation`, `PalmPressSolver`, `JetModel`, `OxygenModel`, `TentacleRope`, and `SeaMine`.
  - Contraction mapping: 8 N combined palm press -> $\kappa = 1.0$, compression $c_b = 6.0\text{ mm}$.
  - Jet dynamics: $\Delta v_y = 0.4583\text{ m/s}$ ($0.46 \pm 0.05\text{ m/s}$), jet depth gain $70\text{ mm}$ ($40-90\text{ mm}$).
  - Drift & Steering: Drift sink rate $8\text{ mm/s}$; $20^\circ$ tilt gives $41\text{ mm/s}$ lateral speed.
  - Oxygen: Base budget 100, jet cost 2, drift drain 0.6/s, mine strike damage 18.
  - Combo Bloom: 5 clean contractions ($\kappa \ge 0.7$) yield Bloom (+50% jet height).
  - Tentacle collision: 6-segment Verlet chain detects mines within 6 mm, triggers 1.0 s stun.
- **Rig**: `JellyRig` MonoBehaviours connecting hand tracking inputs (palm press force, dome tilt) and synchronizing bell transform and tentacle joints.
- **Acceptance Tests**: 14/14 tests implemented strictly following Section 16 specifications.
