# Meta XR 50 Production Games (Spherical, Ellipsoidal & Domed)

A complete production suite of **50 XR Games** built for Unity **6000.0.64f1** with **Meta XR Simulator** runtime integration, custom 3D models (`.glb`), automated physics/kinematics rigs, edit-mode acceptance tests, and a user experience (UX) simulation test harness.

---

## Architecture & Chapter Overview

The collection contains 50 distinct games partitioned across 5 spatial geometry chapters:

| Chapter | Geometry Classification | Scale / Interaction Paradigm | Game Count | Gate Status |
|---|---|---|---|---|
| **Chapter 4.1a** | True Spheres | Pinch Scale (20 mm – 80 mm) | 10 Games | **Certified (140/140 Tests)** |
| **Chapter 4.1b** | True Spheres | Palm Scale (90 mm – 160 mm) | 10 Games | **Certified (140/140 Tests)** |
| **Chapter 4.2** | Prolate & Oblate Spheroids | Tabletop / Freeform Ergonomics | 10 Games | **Certified (140/140 Tests)** |
| **Chapter 4.3a** | Hemispheres & Domed Bowls | Pinch Scale Precision Interaction | 10 Games | **Certified (140/140 Tests)** |
| **Chapter 4.3b** | Hemispheres & Domed Cauldrons | Hand Scale Dynamic Gestures | 10 Games | **Certified (140/140 Tests)** |

---

## 50 Games Directory

### Chapter 4.1a: True Spheres (Pinch Scale)
1. `socket_trackball` – Precision omnidirectional rolling sphere in calibrated socket.
2. `clam_pearl_guard` – Clam shell occlusion defense around levitating pearl.
3. `bernoulli_levitator` – Fluid dynamic nozzle air stream Pearl suspension.
4. `micro_planet_scanner` – Spherical orbital scan with latitude surface mapping.
5. `cage_pinball` – Spherical wireframe containment bouncing pinball.
6. `ocular_turret` – Eyeball orb trackball aiming turret mechanism.
7. `fusion_core_stabilizer` – Magnetic pinch ring thermal plasma stabilizer.
8. `scarab_canyon` – Guided rolling dung beetle boulder obstacle navigation.
9. `chrono_sphere_scrub` – Time-scrubbing rotatable sphere mechanism.
10. `electron_deflector` – Charged atomic nucleus orbital magnetic deflector.

### Chapter 4.1b: True Spheres (Palm Scale)
11. `atlas_defense` – Planetary globe missile interception and orbital shields.
12. `pulse_core_reactor` – Multi-frequency spherical harmonic resonance core.
13. `katamari_harvest` – Magnetic rolling surface accretion sphere.
14. `mystic_seer` – Crystal scrying sphere with palm proximity auric glow.
15. `equatorial_particle_ring` – Palm-tilt equatorial accelerator ring.
16. `geyser_venting` – Volcanic hydrothermal pressure sphere valve.
17. `trench_run_globe` – Miniature trench navigation spherical globe diorama.
18. `containment_dribble` – Palm-guided zero-G spherical containment bouncing.
19. `bio_sphere_terraformer` – Microclimate planetary atmospheric terraformer.
20. `gyro_sphere_mech` – Internal flywheel torque stabilized gyro cockpit.

### Chapter 4.2: Prolate & Oblate Spheroids
21. `weeble_sumo` – Self-righting prolate spheroid sumo wrestler with restoring torque.
22. `bobsled_chute` – Aerodynamic prolate bobsled descending banked curves.
23. `worry_stone_spirit` – Oblate river pebble thumb-friction thermal spirit ignition.
24. `sub_surface_hovercraft` – Oblate aerodynamic craft skimming tabletop air cushion.
25. `paddlewheel_pod` – Twin paddlewheel aquatic displacement spheroid pod.
26. `phantom_stone_skipper` – Oblate skipping stone skimming over water plane.
27. `scarab_buckler` – Forearm mounted oblate carapace shield.
28. `trilobite_march` – Segmented oblate chitinous crawler ambulation.
29. `vortex_wobbler` – Precessing spinning top on tabletop rim.
30. `elliptical_orbit` – Dual-foci celestial elliptical gravity assist simulation.

### Chapter 4.3a: Hemispheres (Pinch Scale)
31. `phantom_shell_game` – Triple dome cup sliding shuffle with ruby pearl reveal.
32. `micro_wok_toss` – Miniature cast iron wok wrist flick bead toss and catch.
33. `deep_space_dish` – Parabolic receiver dish tracking stellar pulsar radio signals.
34. `temple_bell` – Miniature domed bronze bell resonant mallet striker.
35. `bey_cap_clash` – Inverted battle arena dome top clashing battle.
36. `tardigrade_pond` – Microscopic droplet inverted dome aquatic ecosystem.
37. `paratrooper_basket` – Inverted parachute dome payload aerial descent.
38. `micro_mortar_alchemist` – Convex alchemical mortar pestle grinding basin.
39. `limpet_suction` – Inverted suction cup table adhesion and release.
40. `snapping_turtle_bunker` – Retractable domed carapace defensive bunker.

### Chapter 4.3b: Hemispheres (Hand Scale)
41. `galaxy_cauldron` – Ceremonial cauldron, cosmic broth, 220 mm/s vortex core ignition.
42. `medusa_jellyfish` – Bioluminescent bell, dual-palm compression, 70 mm jet ascent.
43. `kraken_broth` – Sea basin, quadrant belly drumming, tentacle sector slam.
44. `foghorn_buoy` – Inverted pendulum buoy, ocean wave tilt, sonic blast hazard repel.
45. `buckler_deflector` – Concave buckler shield, fireball cavity intercept, 440°/s beam release.
46. `singing_bowl_sanctuary` – Tibetan brass bowl, mallet rim friction trace, acoustic wave.
47. `fingerboard_bowl` – Miniature skate park deep hemisphere bowl carving.
48. `observatory_dome` – Rotating astronomical dome slit aperture telescope tracking.
49. `arecibo_reflector` – Giant spherical dish reflector antenna feed sweep.
50. `tortoise_bastion` – Giant vaulted tortoise shell shield fortress.

---

## Quality Gate Certification
* **Total Acceptance Tests**: 700 / 700 Passing (14 per game, 0 compilation errors).
* **Quality Authority**: `Campaign~/xr-gate.ps1` enforcing anti-cheat linting, zero `NotImplementedException`, valid test bodies, and Meta XR simulation requirements.
* **Status Manifest**: Verified complete in `Campaign~/manifest.json`.

---

## User Experience (UX) Simulation Suite
The project includes an automated end-to-end UX simulation harness (`Assets/Editor/UXSimulator.cs`):
* **PBR Shaders & Realistic Tabletop**: Walnut workstation grounded at $Y = 0.00\text{ m}$ with zero ground penetration.
* **HMD Perspective**: Ergonomic eye-level camera ($28^\circ$ pitch-down, $72^\circ$ FOV).
* **Synthetic Gestures**: Translucent cyan MR ghost pinch and open-palm indicators at physical contact points.
* **Verification Screenshots**: Over 45 high-resolution PNG captures stored in `Screenshots/` along with `UX_TEST_REPORT.json`.

### Running UX Simulation
Execute via Unity batchmode using PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File .\run_ux_sim.ps1
```
Or inside Unity Editor via menu: **XR > Run UX Simulation Suite**.

---

## Project Structure
```
MetaXRSim/
├── Assets/
│   ├── Editor/
│   │   └── UXSimulator.cs              # End-to-end UX simulation test harness
│   └── Games/                          # 50 production games (Models, Scripts, Tests)
│       ├── 4_1a_true_spheres_pinch_scale/
│       ├── 4_1b_true_spheres_palm_scale/
│       ├── 4_2_prolate_and_oblate_spheroids/
│       ├── 4_3a_hemispheres_pinch_scale/
│       └── 4_3b_hemispheres_hand_scale/
├── Campaign~/                          # Quality gate runner, manifest.json & test results
├── Specs~/                             # Game design markdown specifications
├── Screenshots/                        # UX verification screenshots & UX_TEST_REPORT.json
├── Packages/                           # Unity package manifest & Meta XR SDK dependencies
├── ProjectSettings/                    # Unity physics, graphics, tags & XR settings
├── run_ux_sim.ps1                      # Automated batchmode test runner script
└── README.md
```
