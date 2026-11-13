# Build Log: Tortoise Bastion (`tortoise_bastion`)

## Implementation Summary
- **Model**: `tortoise_bastion.glb` (source `175-sea-turtle.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/tortoise_bastion/Models/tortoise_bastion.glb`).
- **Domain Simulation**: Pure C# `TortoiseSimulation`, `ScutePlate`, and `DragonAgent`.
  - Scute grid: 6 concentric rings ($t_k = 6.0 - 0.6 \cdot k\text{ mm}$) and 12 sectors ($30^\circ$ each), 72 plates total.
  - Seam detection: 1.2 mm seam width rejects brace commands and suffers guaranteed penetration.
  - Fireball resolution:
    - Grazing hits ($> 65^\circ$) ricochet cleanly for +100 score with zero damage.
    - Frontal hits absorb if plate thickness $\ge$ power for +150 score.
    - Seam or under-thick impacts penetrate, costing 1 integrity and scorching the plate for 3 s.
  - Brace mechanic: Fingertip taps grant +30% effective thickness for 1.2 s.
  - Spirit cannon: 0.8 s apex press charges hitscan beam within $6^\circ$ cone, dealing 1 damage to dragons and awarding +400 score upon kill.
  - Siege conditions: 6 penetrations deplete integrity and trigger siege end.
- **Rig**: `TortoiseRig` MonoBehaviours connecting two-palm twist input to dome yaw orientation.
- **Acceptance Tests**: 14/14 tests implemented strictly following Section 16 specifications.
