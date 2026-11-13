# Micro-Planet Scanner

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Micro-Planet Scanner |
| Slug | `micro_planet_scanner` |
| Physical prop | Soda-lime glass marble, true sphere; 16.0 mm diameter (r = 8.0 mm); glass, 2.50 g/cm³; mass 5.0 g; surface µ = 0.30 |
| Family / sub-shape | True sphere, pinch-scale, three-finger tripod |
| Scale class | Prop 1:1; virtual moon 60 mm diameter, a 3.75× holographic expansion of the marble |
| Source concept quote | "Hold the marble securely in a three-finger tripod pinch with your left hand and rotate its surface under the pad of your right index finger." |
| Physical input | Left three-finger tripod; right index rolls and scans the surface |
| Session length | 6 min (5 survey waves, 15 worms) |
| Lose condition | Core corruption reaches 100% |

## 1. Fantasy & Core Loop

The player is a xenogeologist examining a forested alien moon that has been compressed into a glass marble. The marble swells into a 60 mm holographic moon centred on the real prop, its forests a green haze and its core a slow orange pulse. The left hand holds the marble in a precise three-finger tripod; the right index is an orbital magnifier and laser. Magma worms burrow beneath the surface, and the player must roll the moon under the right fingertip until the scanner reticle sits on a worm, hold it there for 400 ms, and burn the creature out before it reaches the core.

1. **Tripod check (2.0 s).** Thumb, index, and middle pads settle on the marble; the hologram unfolds from the glass core.
2. **Survey (8.0 s).** Worms surface at vents and drift along latitude tracks. The player rotates the marble with the left tripod and fine-tunes with the right index.
3. **Purge (3.0 s).** A 400 ms lock fires the laser; the worm dissolves into embers and the corruption meter ticks down.
4. **Rotate (2.0 s).** The moon's terminator sweeps; new vents rotate into view; the next wave teaser lights up.

One worm cycle is 15.0 s; a wave is 45 s; five waves plus onboarding and rests total 6 min. The core loop's shortest successful interaction is 0.4 s (lock) and the longest is 12 s (a worm that surfaces far from the current reticle and must be chased around the moon).

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the sphere center, poles at `±ẑ_P`, equator in the `x̂_P`–`ŷ_P` plane, and no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]` from the equator, longitude `λ ∈ [0°, 360°)` from +x̂_P toward +ŷ_P; surface point `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; outward normal = `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P-frame**: origin at the marble center; poles `±ẑ_P` are inertial and arbitrary. The marble is featureless for registration, so `+ẑ_P` is frozen at session start from the projection of `+ŷ_W` into `P`; the hologram's north pole inherits it.
- **Radius**: `r = 8.0 mm` for the collider, the rolling law, and surface projection. The virtual moon radius is `R_h = 30.0 mm`, giving a uniform hologram scale `k = R_h/r = 3.75`.
- **Equator and poles**: equator is the `x̂_P`–`ŷ_P` great circle; latitude and longitude follow the baseline parameterization. Worms, vents, and craters are P-fixed, so rotating the marble brings different terrain under the scanner.
- **Rolling kinematics**: the marble is rotated by both hands. The canonical law is `ω = (n × v_t)/r`; each contact contributes `ω_i = (n_i × v_ti)/r` with `r = 8.0 mm`; the left tripod contacts are weighted 0.35 each in aggregate and the right index contact 0.65 for fine control. The fused `ω` is clamped to 540 °/s and integrated into `P`. Contact slip above 25 mm/s is blended out over 120 ms.
- **V-frame anchoring**: `V` is anchored to `P` and uniformly scaled: `V_O = P_O`, `x̂_V = x̂_P`, `ŷ_V = ŷ_P`, `ẑ_V = ẑ_P`, and `p_V = k·(p_P − P_O) + V_O`. The virtual moon is co-located with the real marble, so the marble is the visible core of the hologram.
- **Deviation 1**: `V` is a scaled P frame, not a 1:1 frame. All gameplay tolerances below are in V millimetres on the 30 mm moon; divide by 3.75 for the equivalent distance on the real marble.
- **Deviation 2**: scanner aiming uses the fingertip *direction* from the marble center, so the finger may hover up to 25 mm away and still aim; contact is required only for rolling, not for pointing.
- **Deviation 3**: the left grip is a three-point tripod, not a C-ring or a cage. The tripod is validated by three contact distances and three pairwise normal angles, not by a containment torus.
- **Deviation 4**: the hologram is additive and depth-faded around the real marble, so the glass core remains visible; the moon is not an opaque replacement for the prop.

## 3. Physical Play Space & Ergonomics

The player sits upright, left forearm supported, elbow at 85–100° flexion, and holds the marble 240–280 mm in front of the sternum at 80–120 mm above the table. The left thumb, index, and middle pads form a tripod around the equator; the ring and pinky are curled clear. The right hand approaches from the player's right, index extended, wrist neutral; the right index pad rests on the marble with 0–2 mm of contact. The head looks 15–20° down.

| Parameter | Value | Rationale |
|---|---|---|
| Support surface | Table `T`; both forearms supported | Precision work needs a stable base |
| Play patch | 150 mm × 150 mm × 100 mm around the marble | Covers tripod rotation and index sweep |
| Reach envelope | Both wrists within 320 mm of sternum | Avoids shoulder fatigue |
| Marble height | 80–120 mm above `T` | Keeps the hologram clear of the table mesh |
| Session | 6 min; 45 s waves; 6 s rests | Precision grip fatigue is the limiting factor |
| Tripod duty | ≤ 40 s continuous, then 6 s open-hand rest | Relieves thumb/index/middle loading |
| Scan rate | 0.3–1.5 rev/s | Keeps wrist pronation within comfort |

Tripod-fatigue mitigations: the three-finger grip is the most demanding posture in this document, so the game (a) caps each wave at 45 s, (b) forces a 6 s open-hand rest between waves, (c) accepts any three contacts within 9 mm rather than requiring a specific pad placement, (d) lets the player re-seat the tripod at any time without losing score, (e) shows a "tripod load" meter that rises with continuous hold and falls twice as fast when the marble is released, and (f) auto-pauses if contact quality drops below 0.4 for 1.0 s. The right index does no gripping, only pointing and light rolling, so it has no sustained load.

## 4. Prop Geometry & Registration

The prop is a 16.0 mm glass marble. Mass 5.0 g, density 2.50 g/cm³, moment of inertia `I = 2/5·m·r² = 1.28 × 10⁻⁷ kg·m²`, surface µ = 0.30. It is held in a tripod and rotated, not rolled on a table.

| Sphere property | Value |
|---|---|
| Diameter | 16.0 mm ± 0.2 mm |
| Radius `r` | 8.0 mm |
| Mass | 5.0 g ± 0.3 g |
| Collider | Perfect sphere, r = 8.0 mm |
| Visual mesh | 64 × 32 UV sphere, glass core + 60 mm hologram shell |
| Hologram radius | 30.0 mm (scale 3.75) |
| Pose source | Visual odometry + fused contact `ω` |

Surface-feature atlas (P-fixed; the moon's terrain):

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| North calotte | +80° to +90° | all | Ice cap; low worm activity |
| Forest belt A | +20° to +45° | all | Green biome, vent spawn band |
| Forest belt B | −35° to −5° | all | Green biome, vent spawn band |
| Equator scar | −2° to +2° | all | Visual prime meridian; no spawn |
| Vent V1 | +18° | 40° | Worm spawn point |
| Vent V2 | −22° | 115° | Worm spawn point |
| Vent V3 | +36° | 200° | Worm spawn point |
| Vent V4 | +6° | 305° | Worm spawn point |
| Vent V5 | −41° | 330° | Worm spawn point |
| Core shaft | 0° | all | Corruption readout at the center |

Collider inventory:

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 8.0 mm at `P` | Marble, rolling |
| C1 | Capsule | r = 1.8 mm, length 4 mm at `L_thumb_tip` | Tripod |
| C2 | Capsule | r = 1.8 mm, length 4 mm at `L_index_tip` | Tripod |
| C3 | Capsule | r = 1.8 mm, length 4 mm at `L_middle_tip` | Tripod |
| C4 | Sphere | r = 30.0 mm at `V` | Hologram shell |
| C5 | Sphere | r = 4.0 mm at each worm | Worm body |
| C6 | Ray | origin `R_index_tip`, direction to `P_O` | Scanner |
| C7 | Sphere | r = 8.0 mm at `V` | Core |

Registration: present the marble at 0.8–1.2 m, rotate ≥ 120° for 1.0 s, hold 0.3 s. Acceptance: RMS < 0.35 mm, radius error < 0.12 mm, ≥ 18 rim points. The tripod calibration runs next: the player touches thumb, index, and middle to the marble, and the system records the contact angles so the tripod gate can be personalized.

Re-acquisition: search a 100 mm cube around the last `P` for 1.2 s; succeed at RMS < 0.7 mm for 5 frames. Because the marble is small and shiny, the system prefers specular-rim fitting and rejects frames with fewer than 8 rim points. During loss, `ω` integrates for 200 ms then freezes; the hologram fades to 60% opacity.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long) | Semantic output |
|---|---|---|---|---|
| I5.1 | Tripod thumb | `L_thumb_tip` | φ −70° ± 15°, λ 200°–240° | Tripod contact 1 |
| I5.2 | Tripod index | `L_index_tip` | φ +10° ± 15°, λ 10°–50° | Tripod contact 2 |
| I5.3 | Tripod middle | `L_middle_tip` | φ −20° ± 15°, λ 320°–360° | Tripod contact 3 |
| I5.4 | Coarse roll | `L_thumb_tip`, `L_index_tip`, `L_middle_tip` | same tripod regions | `ω_coarse` |
| I5.5 | Fine roll | `R_index_tip` | any point; typically φ +20°, λ 0° | `ω_fine` |
| I5.6 | Scanner aim | `R_index_tip` | direction from `P_O`; lat/long output | `(φ_s, λ_s)` |
| I5.7 | Lock hold | `R_index_tip` | scanner point within 8 mm V of worm | `lock_t` |
| I5.8 | Purge tap | `R_middle_tip` | any contact, tap 40–120 ms | Manual purge attempt |
| I5.9 | Tripod break | any of C1–C3 | surface distance > 11 mm | Grip warning |
| I5.10 | Re-seat | all three | valid 30 ms | Resume |

Per-finger roles: left thumb, index, and middle form the tripod and can apply a coarse two-axis rotation by shearing the contact triangle; the left ring and pinky are curled and ignored. The right index is the scanner and fine-roll effector; the right middle is an optional manual purge (auto-purge fires at 400 ms regardless). The right thumb, ring, and pinky are ignored.

The scan reticle is the projection of `R_index_tip` onto the hologram shell. It is valid whenever the fingertip is within 25 mm of the marble surface and the ray from fingertip to center intersects C4; no contact is required to aim, but contact is required to roll.

## 6. Input Kinematics & Signal Processing

Rolling fusion: `ω = Σ w_i·(n_i × v_ti)/r`, with `r = 8 mm`, and weights `w = 0.1167` for each of the three tripod contacts (0.35 total) and `w = 0.65` for the right index. When a tripod contact is stale, its weight is redistributed to the other contacts proportionally. `ω` is clamped to 540 °/s and low-passed at 14 Hz; the quaternion is integrated at 90 Hz with renormalization every tick.

Contact-slip: `s_i = |v_i,t − (v_P + ω × (r n_i))_t|`. If `s_i > 25 mm/s`, that contact's estimate is blended 60% toward the previous frame and flagged "slipping"; if all contacts slip for > 150 ms, the marble is considered to have spun loose and `ω` is zeroed until at least one contact is stable for 60 ms.

Scanner projection: `u = normalize(p_tip − P_O)` in `P`; then `φ_s = asin(u_z)`, `λ_s = atan2(u_y, u_x) mod 360°`. The corresponding moon point is `q_V = R_h·u`. Angle to a worm at unit vector `w` is `Δ = acos(clamp(u·w, −1, 1))`, and surface distance is `d_s = R_h·Δ` (V millimetres). Aim is low-passed with a One Euro filter (mincutoff 1.0 Hz, beta 0.05); the effective latency is 18 ms, under the 25 ms perceptual threshold.

Lock classifier: while `d_s < 8.0 mm` nominal (9.0→7.0 mm by wave, §13), `lock_t` accumulates by `dt`; when `d_s` exceeds the current gate, `lock_t` drains at 1.5×`dt`. At `lock_t ≥ 400 ms` the worm is purged and the timer resets. The 8 mm gate on a 30 mm sphere is 15.28°, and the 1.5× drain prevents dithering at the boundary from parking a lock indefinitely.

Tripod gate: each tip must be within 9.0 mm of the marble surface (hysteresis 11.0 mm), and the three contact normals must be pairwise separated by > 60° (hysteresis 55°). The pairwise angle is `acos(n_i·n_j)`. All three conditions must hold for 30 ms; a break requires 80 ms. The 60° separation guarantees a stable three-point grasp rather than three fingers on one side.

Signal chain: joints → confidence gate 0.6 → One Euro → velocities at 90 Hz → contact normals and slip → `ω` fusion → quaternion → hologram; separately fingertip → projection → aim filter → lock accumulator. Event rates: 72 Hz input, 90 Hz simulation, 90 Hz lock evaluation, one purge event maximum per 120 ms.

## 7. Virtual World Layout & Coordinate Mapping

`V` is the scaled P frame from §2.1. The moon shell is the sphere of radius 30 mm centred on the marble. The camera sits 400–600 mm away in `W`; the moon subtends 3.4–5.7°, comparable to a coin at arm's length, which is deliberately small enough to read as a hologram but large enough to resolve 4 mm worms.

| Entity | V-space position | Notes |
|---|---|---|
| Marble core | `(0, 0, 0)`, r = 8 mm | Real glass, visible through the hologram |
| Hologram shell | r = 30 mm | Additive, 0.7 opacity at the rim |
| Worms | r = 30 mm, 4 mm body | Surface agents with depth `d_w` |
| Core | r = 8 mm at origin | Glows with corruption |
| Scanner reticle | `31 mm·u` | Billboarded ring, 10 mm radius |
| Magnifier | `32 mm·u` | 12 mm ring + 6 mm lens disc |
| HUD arc | radius 90 mm around `V` | Corruption and wave |

Mapping formula: `p_V = 3.75·p_P`. A worm at `(φ, λ)` maps to `30·(cosφcosλ, cosφsinλ, sinφ)` in `V`; its burrow depth `d_w` moves it radially inward to `(30 − d_w)·u`. The scanner reticle is at `31·u`, just above the shell, so it is never z-fought by the terrain.

Mapping `P` orientation to camera-facing visuals: the moon rotates rigidly with the marble; the player sees terrain rotate under the fixed reticle, which is the core read. The hologram's forest shader is lit from a virtual sun fixed in `V` at 30° above the equator, so the terminator moves with the marble and gives an unambiguous rotation cue. The reticle and magnifier are camera-facing billboards; the HUD arc is palm-locked but counter-rotated 50% so it stays legible when the wrist rolls.

Terrain scale: a 4 mm worm on a 30 mm moon is 7.6° across, and the 8 mm lock gate is 15.3°; the player therefore has a target roughly half the lock-gate width, which is tight but readable at 0.5 m.

## 8. Gameplay Systems & Rules

Entities: one moon (corruption 0–100), a pool of 15 worms (3–5 active), five vents, one scanner, and one core. Worms spawn at vents and drift along great circles at `v_w = 12 + 6·(wave − 1) °/s`, capped at 36 °/s. Surface time follows §13 (8.0 s tapering to 5.0 s), after which a worm burrows inward at 2.0–3.0 mm/s for up to 12 s; at `d_w = 30 mm` it reaches the core, deals `+20` corruption nominal (15–25 by wave, §13), and respawns at a vent with full surface time.

Purge: a 400 ms lock removes the worm, awards score, and heals 3 corruption. A purge also emits a "seed" particle that flies to the core and reduces corruption by 3, so skilled play can outrun the worms.

Spawn rules: waves 1–2 start with 3 active worms and 2 spare; waves 3–4 with 4 active and 1 spare; wave 5 with 5 active and 1 spare. Spares spawn when an active worm is purged or reaches the core, with a 1.2 s fade-in at the vent. Waves are 45 s or until the wave's quota of purges is met, whichever comes first.

Scoring: purge `+200·M`; precision bonus `+50` if the lock began within 5.0 mm of the worm centre; chain `×1.5` per purge within 6.0 s, capped ×3.0; core heal `+10` per 3 corruption removed. `M = 1 + 0.1·(wave − 1)`. End-of-run corruption penalty is `−8 × corruption`.

Win: purge all 15 worms with corruption < 100. Lose: corruption reaches 100; the moon cracks and the run ends. A flawless run (no worm reaches the core) awards 800 and the "core warden" badge. The corruption meter is always visible as a molten ring around the core; it is not a hidden resource.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| Boot | app start | headset tracked | load moon, zero corruption | Register |
| Register | marble presented | fit RMS < 0.35 mm | set `P`, `+ẑ_P`, tripod calibration | TripodCheck |
| TripodCheck | three contacts valid 30 ms | distances < 9 mm, angles > 60° | unfold hologram, enable scan | WaveIntro |
| TripodCheck | 4.0 s timeout | invalid | ghost tripod prompt | Register |
| WaveIntro | 1.5 s timer | wave ≤ 5 | banner, spawn worms | Survey |
| Survey | worm purged | lock 400 ms | score, heal, spawn spare | Survey |
| Survey | worm reaches core | `d_w ≥ 30 mm` | corruption +20, respawn | Survey |
| Survey | wave timer 45 s or quota | wave < 5 | tally, rest 6 s | WaveIntro |
| Survey | wave timer 45 s or quota | wave = 5 | victory if corruption < 100 | Victory |
| Any | corruption ≥ 100 | — | crack, core flare | GameOver |
| Survey | tripod broken | any contact > 11 mm | pause scan, warn | TripodRecover |
| TripodRecover | contacts valid 30 ms | 3 contacts | resume, 1 s grace | Survey |
| TripodRecover | 2.0 s timeout | invalid | full pause | Paused |
| Any | prop lost > 1.2 s | — | freeze moon, ghost | Recovery |
| Recovery | prop found | re-fit | re-anchor, 2 s grace | Survey |
| Any | headset removed | — | save wave/corruption | Paused |

The 400 ms lock runs only in Survey; entering TripodRecover freezes `lock_t` rather than draining it, so a brief grip adjustment does not erase progress mid-purge.

## 10. Physics & Collision

| Body | Shape | Mass | Friction | Restitution |
|---|---|---|---|---|
| Marble (input) | Sphere r 8 mm | 5.0 g | 0.30 | 0.05 |
| Tripod fingertips | 3 capsules r 1.8 mm | kinematic | 0.4 | — |
| Scanner ray | Ray, 35 mm | virtual | — | — |
| Worm | Sphere r 4 mm | kinematic | — | — |
| Moon shell | Sphere r 30 mm | kinematic | — | — |

Pair matrix: marble↔tripod (input), marble↔right index (input), scanner↔worm (lock/purge), worm↔core (corruption), worm↔shell (clamp), hologram↔hand (visual only). The marble is never force-fed; all rotation comes from the player. The scanner is a ray cast from `R_index_tip` toward the marble centre, intersected with C4 at radius 30 mm; the hit is `d_s < 8 mm` from a worm centre.

Worm motion: `u(t+dt) = normalize(R(axis, v_w·dt)·u(t))`, where the rotation axis is fixed per worm in `P` and chosen perpendicular to `u` at spawn; this gives a great-circle track. Burrow depth is `d_w += (2.0–3.0 mm/s per §13)·dt` while burrowing, clamped to 30 mm. Purging removes C5 from the world immediately; the VFX ember is cosmetic.

Purge laser: 120 ms dwell, 8 mm spot, 3 W virtual power; no thermal model because the interaction is gated by the 400 ms lock, not by heat. The scanner has no ammo, but a 250 ms cooldown after each purge prevents double-counting the same worm across two ticks.

Real-prop constants: `m = 5.0 g`, `I = 1.28 × 10⁻⁷ kg·m²`, `µ = 0.30`, `ω_max = 540 °/s`, `α_max = 7200 °/s²`, rolling resistance `C_rr = 0.010`. The marble is held, not rolled on a surface, so `C_rr` is used only to settle the visual spin after the fingers stop.

## 11. Rendering, Occlusion & Compositing

Layer order: 8 UI/HUD → 7 hand mesh → 6 near VFX (embers, scanner beam) → 5 reticle/magnifier → 4 worms → 3 hologram shell → 2 hologram atmosphere → 1 marble core → 0 passthrough. The hand mesh is depth-authoritative.

Occlusion cases:

1. **Tripod fingers over the marble.** The three finger meshes occlude both the glass core and the hologram shell; the shell is not drawn through the fingers. A 2 mm shell cutback near each fingertip prevents a glowing rim from bleeding onto the pads.
2. **Right index over the scanner point.** The fingertip correctly hides the reticle; a 1.5 mm-offset reticle ghost at 40% opacity is drawn on the near side so the aim point is still readable under the pad.
3. **Worm passing behind a finger.** The worm is depth-tested against the hand and disappears cleanly; its burrow trail continues on the visible side, so the player can track it around the occlusion.
4. **Hologram shell intersecting the palm.** The shell is clipped by the palm mesh with a 1.5 mm bias; the additive atmosphere fades to zero within 10 mm of skin, avoiding a green wash over the hand.
5. **Marble core vs hologram.** The core is drawn first and the shell is additive with a 0.7 rim opacity; the real glass remains visible as a bright point at the centre, which anchors the hologram to the prop.
6. **Scanner beam crossing the table.** The beam is clipped by the scene mesh; it never paints the tabletop outside the moon silhouette. A 1.5 mm depth bias avoids z-fighting with the support surface.

VFX budget: 800 particles (300 embers, 200 forest spores, 150 corruption motes, 100 scanner sparks, 50 core flares), 1 hologram shell, 5 worm meshes, 1 beam, 1 magnifier lens. Global cap 800; additive overdraw ≤ 2.4×; the hologram uses a 3-pass shell (terrain, atmosphere, rim) at half resolution for the atmosphere.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Tripod locked | Marble | Three soft glass ticks | Hologram unfolds, tripod arcs |
| Roll | Marble | Low glassy rumble, pitch ∝ `|ω|` | Terrain parallax, terminator sweep |
| Scanner acquire | Reticle | Rising sine, 400→800 Hz | Reticle contracts, lens brightens |
| Lock 50% | Worm | Heartbeat thump | Worm glows, ring fills |
| Lock 100% | Worm | Sharp chirp + sizzle | Laser flash, embers |
| Purge | Worm | Glass shatter + relief chord | Corruption ring recedes, +200 |
| Worm burrows | Vent | Low grinding | Terrain dimples, dust plume |
| Core hit | Core | Deep sub thump, 40 Hz | Core flare, crack lines |
| Precision bonus | Reticle | Bright ping | +50 stamp |
| Tripod break | Marble | Ceramic scrape | Tripod arcs red, scan freezes |
| Re-seat | Marble | Soft click | Arcs re-form, 1 s grace |
| Corruption 80% | Core | Distorted heartbeat | Screen edge molten, alarm pulse |

Spatialisation is at the marble, the worm, or the core; the HUD is head-locked. The scanner sine is panned to the reticle by 20% so the player can hear which side of the moon is being scanned.

## 13. Difficulty Progression & Tuning

| Wave | Active worms | Worm speed (°/s) | Burrow start (s) | Burrow rate (mm/s) | Lock gate (mm) | Purge quota | Core damage |
|---|---|---|---|---|---|---|---|
| 1 | 3 | 12 | 8.0 | 2.0 | 9.0 | 3 | 15 |
| 2 | 3 | 18 | 7.0 | 2.2 | 8.5 | 3 | 18 |
| 3 | 4 | 24 | 6.0 | 2.5 | 8.0 | 4 | 20 |
| 4 | 4 | 30 | 5.5 | 2.8 | 7.5 | 4 | 22 |
| 5 | 5 | 36 | 5.0 | 3.0 | 7.0 | 5 | 25 |

Adaptive rule: if corruption rises above 70, slow worm speed by 15% and extend the surface time by 1.0 s for the next wave only. If the player purges 6 worms without a core hit, raise worm speed by 8% and shorten surface time by 0.5 s, capped at wave-5 values. The lock gate never adapts below 7.0 mm.

Named tuning constants: `R_H = 30 mm`, `SCALE = 3.75`, `LOCK_DIST = 8 mm`, `LOCK_TIME = 400 ms`, `LOCK_DRAIN = 1.5`, `TRIPOD_DIST = 9 mm`, `TRIPOD_RELEASE = 11 mm`, `TRIPOD_ANGLE = 60°`, `TRIPOD_ANGLE_OFF = 55°`, `OMEGA_MAX = 540 °/s`, `SLIP = 25 mm/s`, `WORM_R = 4 mm`, `CORE_R = 8 mm`, `CORRUPTION_MAX = 100`.

## 14. Edge Cases & Failure Recovery

- **Marble tracking lost.** Freeze `ω` after 200 ms, fade the hologram to 60%, and keep the last moon orientation. Re-acquire within 1.2 s and blend over 200 ms; otherwise pause.
- **Tripod break.** Scan freezes immediately (lock timer holds), the tripod arcs flash red, and a 2.0 s re-seat window opens. Re-seat within the window resumes with 1 s of grace; otherwise pause.
- **Right index occluded by the left hand.** The scanner holds the last aim for 400 ms; if the finger reappears, aim blends over 150 ms. A persistent occlusion > 1.0 s pauses the scan but not the worms, so the player can lose corruption; this is intentional pressure to keep the index visible.
- **Worm at the pole.** Longitude is singular at `±90°`; the worm's great-circle track is used directly and the HUD longitude readout is suppressed within 3° of the pole. No gimbal artifacts.
- **Two worms overlap.** Each has an independent sphere; the scanner locks the nearest by 3D distance. A purge cannot remove two worms at once.
- **Marble slips in the tripod.** A sudden `ω` spike > 800 °/s or a 5 mm center jump triggers a "slip" event: the hologram freezes for 250 ms, then re-seats; no corruption penalty.
- **Handedness swap.** Mirror the tripod to the right hand and the scanner to the left; the P-frame, hologram, and worms are unchanged. The tripod calibration re-runs.
- **Worm reaches the core during a purge.** The purge wins if `lock_t ≥ 400 ms` on the same tick; the worm is removed before corruption is applied. This is deterministic and tested.
- **Corruption at 99.** A hit sets it to 100 and ends the run; healing cannot overshoot below 0 or above 100.
- **Headset removed.** Save wave, corruption, active worms, and `P`; on resume, require re-registration and tripod recalibration.

## 15. Implementation Architecture

| Type | Responsibility |
|---|---|
| `PropTracker` | Marble fit, `P`, loss/re-acquire |
| `TripodGate` | Three contacts, pairwise angles, hysteresis, load meter |
| `RollFusion` | Per-contact `ω`, weights, slip, quaternion integration |
| `ScannerProjector` | Fingertip direction, lat/long, One Euro aim |
| `LockAccumulator` | 8 mm gate, 400 ms fill, 1.5× drain, purge event |
| `WormField` | Spawn, great-circle tracks, burrow depth, core hits |
| `MoonState` | Corruption, heal, score, chain |
| `HologramRenderer` | Scaled shell, terrain, atmosphere, occlusion |
| `WaveDirector` | Five waves, quotas, rest, adaptation |
| `FeedbackDirector` | Cue table, beams, embers, audio mix |
| `SessionFSM` | States, timers, replay log |

Event flow: `HandTracking(72 Hz) → TripodGate + RollFusion + ScannerProjector → LockAccumulator → WormField → MoonState → HologramRenderer/FeedbackDirector`. `WaveDirector` owns spawn quotas; `SessionFSM` owns tripod break, purge, and corruption transitions.

```text
function scanner_lock(tip, P, worms, lock_t, dt):
    u = normalize(to_P(tip) - P.center)
    best = argmin(w in worms) acos(clamp(dot(u, w.u), -1, 1))
    d_s = 30.0 * acos(clamp(dot(u, best.u), -1, 1))
    if d_s < 8.0:
        lock_t = min(0.400, lock_t + dt)
    else:
        lock_t = max(0.0, lock_t - 1.5 * dt)
    if lock_t >= 0.400:
        return PURGE(best), 0.0
    return AIM(best, d_s), lock_t
```

```text
function tripod_step(contacts, P):
    if any(contact.surface_dist(P) > 11.0 mm): return BROKEN
    if any(contact.age < 30 ms): return HOLD
    n = [contact.normal for contact in contacts]
    if min(angle(n[i], n[j]) for i < j) < 55°: return BROKEN
    return LOCKED
```

All worm tracks and spawn orders are seeded per run; replay logs store `P` quaternion, tip positions, lock timers, and purge order at 90 Hz.

## 16. Acceptance Test Checklist

1. **Sphere fit.** Present the marble at 1.0 m for 120 frames. Pass: `r` ∈ [7.88, 8.12] mm, RMS < 0.35 mm.
2. **Hologram scale.** Place a fiducial at `p_P = (8, 0, 0)`. Pass: rendered at `p_V = (30, 0, 0)` ± 0.1 mm, and the core stays at the marble centre ± 0.2 mm.
3. **Scanner projection.** Put `R_index_tip` at directions for (φ, λ) = (0, 0), (0, 90), (90, 0), (−45, 270). Pass: reported angles within 0.5°, longitude modulo 360°.
4. **Lock fill.** Hold the reticle at 2 mm from a worm. Pass: purge at 400 ± 8 ms.
5. **Lock drain.** Hold at 2 mm for 200 ms, then move to 12 mm. Pass: lock reaches 0 in 133 ± 8 ms, no purge.
6. **Lock boundary.** Sweep distance across 7.9 mm and 8.1 mm at 30 mm/s. Pass: no dither purge; fill/drain follow the sign of the distance.
7. **Tripod distance.** Set tip distances to 8.9, 9.1, 10.9, and 11.1 mm. Pass: valid, invalid, valid (hysteresis), invalid.
8. **Tripod angle.** Set minimum pairwise angle to 59° and 61°. Pass: invalid at 59°, valid at 61°; release at 55°.
9. **Rolling fusion.** Rotate the marble at 180 °/s with all three tripod contacts plus the index. Pass: fused `ω` 180 ± 10 °/s, quaternion drift < 0.5° over 10 s.
10. **Slip.** Command 900 °/s. Pass: `|ω|` ≤ 540 °/s and no quaternion flip or gimbal artifact at the poles.
11. **Worm core hit.** Allow a worm to burrow to 30 mm. Pass: corruption +20 exactly once, worm respawns at a vent, and a purge on the same tick wins.
12. **Occlusion.** Put the left index over the scanner point. Pass: reticle hidden, 40% ghost visible, no shell bleed onto the pad, 300 frames without z-fighting.
13. **Frame budget.** Run wave 5 with 5 worms and 800 particles. Pass: GPU < 11.1 ms, CPU < 5 ms, particle count ≤ 800.
14. **Endurance.** Run the 6 min scripted session. Pass: `P` drift < 0.5°/min, hologram scale error < 0.2%, memory growth < 5 MB.
