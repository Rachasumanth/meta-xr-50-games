# Build Log: Kraken Broth (`kraken-broth`)

## Implementation Summary
- **Model**: `kraken_bowl.glb` (source `702-soup-bowl.glb` copied to `Assets/Games/4_3b_hemispheres_hand_scale/kraken-broth/Models/kraken_bowl.glb`).
- **Domain Simulation**: Pure C# `KrakenSimulation`, `DrumClassifier`, `QuadrantState`, and `Ship`.
  - Bowl fitting: $R = 80.0\text{ mm}$, $d = 60.0\text{ mm}$, derived $R_s = 83.33\text{ mm}$.
  - Drum classification: Belly band $z \in [-49, -18]\text{ mm}$, 4 quadrants with $\pm 4^\circ$ seam hysteresis.
  - Energy scaling: 40 mm/s -> +5.6, 200 mm/s -> +13.1, 400 mm/s -> +16.8 charge.
  - Alternating fingers: 160 ms alternating strike awards 1.15x energy multiplier.
  - Arm & Slam: 0.45 s telegraph, frozen sector, 90° hit arc.
  - Empty slam penalty: +5 surge, -40 score, 1.8 s cooldown.
  - Multi-kill: 3-ship slam awards 1.5x score multiplier.
  - Escapes: $360^\circ$ ship lap adds +12 surge and -150 score penalty.
  - Surge rate: Level 3 passive surge at 2.2%/s (+22.0 in 10 s).
  - Quota clear: Destroys target quota and awards +400 base and +72 time bonus (at 12 s remaining).
  - Lift pause: Bowl raised > 100 mm freezes simulation state.
- **Rig**: `KrakenRig` MonoBehaviours connecting drum strikes and synchronizing visual tentacle emergence.
- **Acceptance Tests**: 14/14 tests implemented strictly following Section 16 specifications.
