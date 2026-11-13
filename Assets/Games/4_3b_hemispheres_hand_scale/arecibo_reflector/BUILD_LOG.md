# Build Log: Arecibo Reflector (`arecibo_reflector`)

## Implementation Summary
- **Model**: `arecibo_dish.glb` (source `971-satellite-dish.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/arecibo_reflector/Models/arecibo_dish.glb`).
- **Domain Simulation**: Pure C# `AreciboSimulation`, `DishProjector`, `GridCell`, and `Rover`.
  - Dish geometry: $R = 75.0\text{ mm}$, $R_{in} = 71.0\text{ mm}$, depth $55\text{ mm}$, 7 x 7 coordinate grid on inner spherical cap.
  - Normal & Reflection: Analytic spherical cap surface normal and exact $\vec{d}_{out} = \vec{d}_{in} - 2(\vec{d}_{in}\cdot\hat{n})\hat{n}$.
  - Receiver: Suspended at 450 mm above dish bottom, radius 25 mm.
  - Two-bounce scoring: 1.5x multiplier on two-bounce paths; chain streak awards up to 1.3x multiplier.
  - Charge & Occupancy: 5 charges max, costs 1 per placement, recharges at 1 per 4 s. Occupied cells reject placement with zero charge penalty.
  - Autonomous Rover C: Patrols cyclic 4-cell sequence every 4.0 s.
  - Re-anchor: Preserves grid-relative rover placements upon bowl displacement.
- **Rig**: `AreciboRig` MonoBehaviours connecting rover position sync and user input grid targeting.
- **Acceptance Tests**: 14/14 tests implemented strictly following Section 16 specifications.
