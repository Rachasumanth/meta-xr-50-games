# Meta XR 50 Production Games (Spherical, Ellipsoidal & Domed)

> **Production build: ShapeForge 50**
> All 50 games are now combined into one gesture-driven Meta XR application.
> Draw a sphere / spheroid / dome in the air (or point at a real object), and the
> matching mini-game converts onto that object at real-world scale.
>
> - Scene: `Assets/Scenes/ShapeForge.unity`
> - Signed APK: [ShapeForge50_v1.0.0.apk](https://github.com/Rachasumanth/meta-xr-50-games/releases/latest/download/ShapeForge50_v1.0.0.apk)
> - Build / simulator / QA guide: [`PRODUCTION.md`](PRODUCTION.md)
> - Runtime self-test: `50/50 spawned, 50/50 rigs bound`
> - EditMode suite: `718 total, 702 passed, 0 failed, 16 NeedsDevice skipped`
> - Production gate: `261/261 checks passed`
> - Soundtrack: four streamed CC0 tracks with in-app credits and audio toggles
> - APK SHA-256: `A6A11D55734CAC294360586CCE3060FD0ADAAF79D5153C1C61466D425E555D9F`

A complete production suite of **50 XR Games** built for Unity **6000.0.64f1** with **Meta XR** runtime integration, custom 3D models (`.glb`), automated physics/kinematics rigs, edit-mode acceptance tests, and a user experience (UX) simulation test harness.

---

## Architecture & Chapter Overview

The collection contains 50 distinct games partitioned across 5 spatial geometry chapters:

| Chapter | Geometry Classification | Scale / Interaction Paradigm | Game Count | Gate Status |
|---|---|---|---|---|
| **Chapter 4.1a** | True Spheres | Pinch Scale (20 mm - 80 mm) | 10 Games | **Gate-complete (140 test cases)** |
| **Chapter 4.1b** | True Spheres | Palm Scale (90 mm - 160 mm) | 10 Games | **Gate-complete (140 test cases)** |
| **Chapter 4.2** | Prolate & Oblate Spheroids | Tabletop / Freeform Ergonomics | 10 Games | **Gate-complete (140 test cases)** |
| **Chapter 4.3a** | Hemispheres & Domed Bowls | Pinch Scale Precision Interaction | 10 Games | **Gate-complete (140 test cases)** |
| **Chapter 4.3b** | Hemispheres & Domed Cauldrons | Hand Scale Dynamic Gestures | 10 Games | **Gate-complete (140 test cases)** |

---

## 50 Games Directory

### Chapter 4.1a: True Spheres (Pinch Scale)
1. `socket_trackball` --- Precision omnidirectional rolling sphere in calibrated socket.
2. `clam_pearl_guard` --- Clam shell occlusion defense around levitating pearl.
3. `bernoulli_levitator` --- Fluid dynamic nozzle air stream Pearl suspension.
4. `micro_planet_scanner` --- Spherical orbital scan with latitude surface mapping.
5. `cage_pinball` --- Spherical wireframe containment bouncing pinball.
6. `ocular_turret` --- Eyeball orb trackball aiming turret mechanism.
7. `fusion_core_stabilizer` --- Magnetic pinch ring thermal plasma stabilizer.
8. `scarab_canyon` --- Guided rolling dung beetle boulder obstacle navigation.
9. `chrono_sphere_scrub` --- Time-scrubbing rotatable sphere mechanism.
10. `electron_deflector` --- Charged atomic nucleus orbital magnetic deflector.

### Chapter 4.1b: True Spheres (Palm Scale)
11. `atlas_defense` --- Planetary globe missile interception and orbital shields.
12. `pulse_core_reactor` --- Multi-frequency spherical harmonic resonance core.
13. `katamari_harvest` --- Magnetic rolling surface accretion sphere.
14. `mystic_seer` --- Crystal scrying sphere with palm proximity auric glow.
15. `equatorial_particle_ring` --- Palm-tilt equatorial accelerator ring.
16. `geyser_venting` --- Volcanic hydrothermal pressure sphere valve.
17. `trench_run_globe` --- Miniature trench navigation spherical globe diorama.
18. `containment_dribble` --- Palm-guided zero-G spherical containment bouncing.
19. `bio_sphere_terraformer` --- Microclimate planetary atmospheric terraformer.
20. `gyro_sphere_mech` --- Internal flywheel torque stabilized gyro cockpit.

### Chapter 4.2: Prolate & Oblate Spheroids
21. `weeble_sumo` --- Self-righting prolate spheroid sumo wrestler with restoring torque.
22. `bobsled_chute` --- Aerodynamic prolate bobsled descending banked curves.
23. `worry_stone_spirit` --- Oblate river pebble thumb-friction thermal spirit ignition.
24. `sub_surface_hovercraft` --- Oblate aerodynamic craft skimming tabletop air cushion.
25. `paddlewheel_pod` --- Twin paddlewheel aquatic displacement spheroid pod.
26. `phantom_stone_skipper` --- Oblate skipping stone skimming over water plane.
27. `scarab_buckler` --- Forearm mounted oblate carapace shield.
28. `trilobite_march` --- Segmented oblate chitinous crawler ambulation.
29. `vortex_wobbler` --- Precessing spinning top on tabletop rim.
30. `elliptical_orbit` --- Dual-foci celestial elliptical gravity assist simulation.

### Chapter 4.3a: Hemispheres (Pinch Scale)
31. `phantom_shell_game` --- Triple dome cup sliding shuffle with ruby pearl reveal.
32. `micro_wok_toss` --- Miniature cast iron wok wrist flick bead toss and catch.
33. `deep_space_dish` --- Parabolic receiver dish tracking stellar pulsar radio signals.
34. `temple_bell` --- Miniature domed bronze bell resonant mallet striker.
35. `bey_cap_clash` --- Inverted battle arena dome top clashing battle.
36. `tardigrade_pond` --- Microscopic droplet inverted dome aquatic ecosystem.
37. `paratrooper_basket` --- Inverted parachute dome payload aerial descent.
38. `micro_mortar_alchemist` --- Convex alchemical mortar pestle grinding basin.
39. `limpet_suction` --- Inverted suction cup table adhesion and release.
40. `snapping_turtle_bunker` --- Retractable domed carapace defensive bunker.

### Chapter 4.3b: Hemispheres (Hand Scale)
41. `galaxy_cauldron` --- Ceremonial cauldron, cosmic broth, 220 mm/s vortex core ignition.
42. `medusa_jellyfish` --- Bioluminescent bell, dual-palm compression, 70 mm jet ascent.
43. `kraken_broth` --- Sea basin, quadrant belly drumming, tentacle sector slam.
44. `foghorn_buoy` --- Inverted pendulum buoy, ocean wave tilt, sonic blast hazard repel.
45. `buckler_deflector` --- Concave buckler shield, fireball cavity intercept, 440--/s beam release.
46. `singing_bowl_sanctuary` --- Tibetan brass bowl, mallet rim friction trace, acoustic wave.
47. `fingerboard_bowl` --- Miniature skate park deep hemisphere bowl carving.
48. `observatory_dome` --- Rotating astronomical dome slit aperture telescope tracking.
49. `arecibo_reflector` --- Giant spherical dish reflector antenna feed sweep.
50. `tortoise_bastion` --- Giant vaulted tortoise shell shield fortress.

---

## Quality Gate Certification
* **EditMode suite**: 718 total, 702 passed, 0 failed, 16 `NeedsDevice` skipped (device/rendering items documented per game).
* **Unified production gate**: `ShapeForgeValidator` = 261/261 checks (catalogue, rig types, XR loaders, Meta manifest permissions, IL2CPP preservation, scene/build settings).
* **Runtime self-test**: 50/50 games spawned, 50/50 rigs bound, 50/50 objectives ticking; synthetic pinch grabbed 50/50 props; `-shapeforge-stress` plays all 50 games for 30 s each, churns enter/exit and measures the bus-stop / airplane-seat / enter-exit criteria (see PRODUCTION.md).
* **Status Manifest**: `Campaign~/manifest.json` remains the per-game gate record; the unified build does not alter it.

---

## Unified ShapeForge App
The production scene is `Assets/Scenes/ShapeForge.unity`. It contains one
`AppBootstrap`; the runtime creates the XR rig, passthrough layer, scene probe,
gesture hub, menu and game host.

* **Gesture conversion**: circle -> True Sphere, tall oval -> Prolate, flat oval ->
  Oblate, open arc -> Hemisphere / Domed Bowl, with a size/real-object-aware
  match into all 50 games.
* **Real-object conversion**: Meta scene volumes/planes are measured and used as
  the board anchor and scale.
* **All 50 rigs**: attached by a direct, IL2CPP-preserved type table; transform
  fields are bound to the board and live hand proxies.
* **Physical interaction**: shared pinch/controller grab, throw, guard and
  objective loop (spin, balance, catch, stir, aim, defend, collect, steady,
  pour, race).
* **Presentation**: animated welcome card, living Forge Orb, holographic family
  icons, gesture glow trail, scan cages, sparkles, score pop-ups and an animated
  HUD.
* **Soundtrack**: four CC0 tracks streamed from Resources, crossfaded by family,
  with persistent MUSIC / SOUND toggles.

See [`PRODUCTION.md`](PRODUCTION.md) for the full architecture, controls,
simulator instructions and signing/build commands.

### Runtime self-test
```powershell
& "Build\Windows\ShapeForge50.exe" -shapeforge-selftest -logFile selftest.log
```
Exit code 0 means all 50 boards spawned and all rigs bound. Add
`-shapeforge-capture` on a GPU machine to save PNG evidence.

### UI / music smoke
```powershell
& "Build\Windows\ShapeForge50.exe" -batchmode -nographics -shapeforge-ui-smoke -logFile ui_smoke.log
```
Expect `[UI_SMOKE] built=True ... musicClip=True` and `[UI_SMOKE] dismissed blocked=False menuActive=True`.

### Legacy UX harness
`Assets/Editor/UXSimulator.cs` is retained for the original campaign record but
is not the production runtime and is not part of the shipped scene.

---

## Project Structure
```
MetaXRSim/
  Assets/
    MetaXRSimApp/                # Unified ShapeForge runtime, editor gate, tests
      Core/                      # gesture recogniser, catalogue, matchmaker
      Runtime/                   # hub, host, input, session, visuals, audio, UI
      Editor/                    # setup, validator, APK builder
      Tests/Editor/              # 18 EditMode tests
    Games/                       # 50 original games (models, rigs, acceptance tests)
    Scenes/ShapeForge.unity      # the shipped scene
    Plugins/Android/             # Meta store-compatible manifest
    Resources/ShapeForgeMusic/   # 4 CC0 tracks + CREDITS.txt
    link.xml                     # IL2CPP preservation for all 50 rigs
  Campaign~/                     # per-game gate record, manifest.json, results
  Specs~/                        # game design specifications
  Releases/                      # signed APK, SHA-256, runtime self-test report
  Packages/                      # Unity package manifest & Meta XR SDK
  ProjectSettings/               # XR loader, player, quality settings
  PRODUCTION.md                  # production build / simulator / QA guide
  README.md
```






