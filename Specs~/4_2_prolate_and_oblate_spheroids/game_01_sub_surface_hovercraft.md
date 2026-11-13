# Sub-Surface Hovercraft

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Sub-Surface Hovercraft |
| Slug | `sub_surface_hovercraft` |
| Physical prop | Computer mouse, switched off, optical sensor disabled; 110 × 62 × 38 mm; 95 g; matte ABS shell, silicone scroll wheel, two tactile button caps |
| Registered contact spheroid | Oblate; `a = b = 31.0 mm`, `c = 19.0 mm` (62 × 62 × 38 mm fitted shell); the 110 mm mouse hull is a P-fixed visual extension |
| Family / sub-shape | 4.2 Prolate and Oblate Spheroids / oblate, palm-scale, planar sliding |
| Scale class | Physical patch 120 mm; virtual channel 900 mm long; motion gain 3.0×; tabletop scale |
| Source concept quote | "An armored swamp hovercraft in a bioluminescent marsh. Sliding steers with inertia; left-click fires forward harpoons; right-click is a drift boost; scroll adjusts sonar depth." |
| Physical input | Right palm and heel push the mouse over a 120 mm patch of left thigh or lap tray; right index operates the left button; right middle operates the right button; right index rubs the scroll wheel |
| Session length | 8 min: onboarding 50 s + 5 channels × 70 s + 4 intermissions × 10 s + results 30 s |
| Lose condition | Hull integrity reaches 0, or the channel clock reaches 0 before the extraction gate |

## 1. Fantasy & Core Loop

The marsh is black water under a ceiling of luminous spores. Your vehicle is a squat armored hovercraft with a fan skirt, a bow harpoon rail, and a sonar drum that can be tuned to five depth layers. The mouse under your right hand is the helm: the whole shell slides on a slick patch of thigh, and every shove you give it becomes thrust through the skirt. Eels hunt at one depth, mines anchor at another, and the channel's leviathan is never where its silhouette says it is. You must read the sonar, choose a depth, commit the craft, and fire before the water closes over the bow.

1. **SCAN (0.6 s).** The sonar sweep paints the next 220 mm of channel. Contacts at the selected depth are solid white; contacts at other depths are 20 % ghosts.
2. **PUSH (0.4–1.8 s).** The player drives the physical mouse across the 120 mm patch. Motion gain 3.0× moves the craft; inertia and skirt drag mean a released mouse coasts 18–30 mm.
3. **SET DEPTH (0.18 s per band).** A scroll-wheel rub steps the sonar band 0–4. The craft's harpoon depth follows the sonar depth; hazards only collide inside the selected band.
4. **FIRE (0.15 s).** A left-button contact longer than 40 ms launches a harpoon at 800 mm/s along the craft heading with lead prediction and 0.32 s cooldown.
5. **DRIFT (0.30 s).** A right-button contact injects a 0.020 N·s lateral impulse and drops skirt friction for 0.45 s, at a heat cost of 26 points.
6. **RESOLVE (0.40 s).** Hit, miss, mine, or gate events resolve; the next sonar sweep starts. A channel is 6–10 hazards and one extraction gate.

A clean channel takes 45–60 s; a cautious channel takes 70 s. The loop is deliberately a *driving* loop, not a turret loop: the craft is always moving, the patch is small, and the mouse must be clutched back to centre every 2–4 seconds.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Spheroid sub-shape**: the prop is an ellipsoid of revolution with equatorial semiaxes `a = b` in the `x̂_P`–`ŷ_P` plane and polar semiaxis `c` along `ẑ_P`; prolate if `c > a`, oblate if `c < a`. Surface: `p_P = (a·cosφ·cosλ, a·cosφ·sinλ, c·sinφ)`, `φ ∈ [−90°, +90°]`, `λ ∈ [0°, 360°)`; outward normal `n̂ ∝ (cosφ·cosλ/a, cosφ·sinλ/a, sinφ/c)` normalized. Equator in `x̂_P`–`ŷ_P`, poles at `±ẑ_P`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

`P` origin is the centre of the fitted contact spheroid, `+ẑ_P` points out of the mouse top shell, `+x̂_P` points toward the front (cable end), and `+ŷ_P` points to the player's left; poles at `±ẑ_P`, equator in `x̂_P`–`ŷ_P`. Exact semiaxes are `a = b = 31.0 mm` and `c = 19.0 mm`; this is the 62 × 62 × 38 mm spheroid inscribed in the real shell, while the 110 mm hull is a P-fixed extension outside the collider. The local rolling model is `ω = (n × v_t)/r_eff`, with `r_eff = |p_P|`, the distance from the spheroid centre to the contact point along the contact normal: `r_eff = 31.0 mm` at the equator and `19.0 mm` at the poles. Rolling is used only for tip and skid feedback; drive is planar. `V` is anchored to `T`, with origin at the centre of the 120 mm tactile patch, `x̂_V` forward, `ŷ_V` left, `ẑ_V` up. Four deviations:

1. **Hull extension.** The 110 mm mouse length, button caps, and scroll wheel lie outside the 62 × 62 × 38 mm contact spheroid. They receive the atlas features and fingertip contacts but are excluded from the depth-proxy collider, which is the spheroid only.
2. **Planar drive with clutch.** The prop does not roll freely; it slides on the patch with a virtual mass, drag, and Coulomb friction. Tracked tilt is clamped to 8° for visual feedback, and when the physical mouse reaches ±48 mm from patch centre the input is clutched (150 ms freeze, 32 mm virtual re-centre).
3. **Virtual depth layers.** Sonar depth bands are virtual vertical layers, not physical height. The real mouse never moves down; only its rendered sonar ring and harpoon depth change.
4. **Compliant thigh/tray.** The support surface is soft tissue or a 4 mm foam lap tray. The drive model includes a 1.5 mm sink and a 0.42 friction coefficient, and the prop may be lifted without registering as a drop.

## 3. Physical Play Space & Ergonomics

The player sits with the right forearm supported and the mouse under the right palm. The 120 mm patch is on the left thigh, 60–90 mm in front of the left hand, or on a lap tray at the same height. The left hand rests on the tray edge as a brace. The mouse is oriented with its long axis forward; the player's wrist is neutral (10–20° extension), and the elbow is flexed 95–110°. The patch is deliberately short: it forces a clutch every 2–4 seconds, which keeps the wrist from drifting into ulnar deviation and makes the drive feel like repeated strokes rather than a mouse drag.

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated, both forearms supported | Removes shoulder load from fine steering |
| Patch | 120 × 90 mm on thigh or lap tray | Fits the mouse plus 5 mm margin; matches natural forearm sweep |
| Patch height | 20–40 mm below right elbow | Keeps the wrist neutral at the top of the mouse |
| Hand travel | ±48 mm from patch centre | Clutch before the wrist reaches 25° ulnar deviation |
| Mouse yaw | ±12° from channel forward | Prevents the hull from fighting the hand |
| Session | 8 min with 10 s intermissions | Five channels at 45–70 s each |
| Break cue | Left-hand open-palm overlay after channel 3 | Releases the flexor grip before fatigue |
| Left hand | Brace on tray edge, no mouse contact | Gives a stable reference for the clutch |

The mouse is used with a *flat palm*, not a claw grip: the thenar pad and the heel supply the push, and the index/middle fingers only tap buttons. This distributes the 0.4–1.4 N drive force over 18 cm², well below the 2.0 N/cm² comfort threshold for a 8 min session. A 5 mm dead zone suppresses tremor; a deliberate push must exceed 12 mm/s for 70 ms.

## 4. Prop Geometry & Registration

| Property | Value | Notes |
|---|---|---|
| Overall hull | 110 × 62 × 38 mm | Real mouse, including button caps |
| Contact spheroid | `a = b = 31.0 mm`, `c = 19.0 mm` | 62 × 62 × 38 mm fitted to the top and sides |
| Mass | 95 g | Includes one AA cell, retained for correct inertia feel |
| Inertia about `ẑ_P` | `I_z = 2ma²/5 = 3.65 × 10⁻⁵ kg·m²` | Used for skid yaw only |
| Hull friction on skin | µ = 0.42 | Measured on dry thigh; tray foam 0.48 |
| Wheel friction | µ = 0.90 | Silicone wheel, used for detent detection |
| Shell colour | Matte graphite | Low specularity avoids passthrough glare |

**Surface-feature atlas (P-fixed; λ from `+x̂_P` toward `+ŷ_P`, φ from equator):**

| Feature | φ | λ | Extent | Purpose |
|---|---|---|---|---|
| Scroll wheel | +52° | 0° | 8 × 3 mm | Sonar depth detents |
| Left button pad | +22° | +28° | 22 × 14 mm | Harpoon fire |
| Right button pad | +22° | −28° | 22 × 14 mm | Drift boost |
| Palm pad | −30° | 180° | 34 × 26 mm | Primary push contact |
| Front bumper | 0° | 0° | 12 mm band | Hull-forward reference |
| Rear bumper | 0° | 180° | 12 mm band | Clutch/reverse reference |
| Left flank | 0° | +90° | 20 mm band | Two-finger fine steer |
| Right flank | 0° | −90° | 20 mm band | Two-finger fine steer |
| Top crest | +90° | any | 6 mm cap | Tilt reference and tip pivot |
| Button split line | +22° | 0° | 3 mm groove | Separates left/right click zones |

The atlas cell size at the equator is `2πa/64 = 3.04 mm`; features are at least 3 cells wide so that contact classification is never ambiguous at the 5 mm threshold.

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Spheroid | `a = b = 31.0`, `c = 19.0 mm` at `P` | Depth proxy and hand-contact surface |
| C1 | Capsule | r = 15 mm, length 110 mm, along `x̂_P` | Visual hull only; excluded from depth and contact |
| C2 | Two discs | r = 11 mm, at the button pads | Click contact gates |
| C3 | Torus segment | major r = 4 mm, minor r = 1.5 mm | Wheel detent contact |
| C4 | Capsules | r = 1.8 mm, length 14 mm, per fingertip | Contact generator |
| C5 | Sphere | r = 4 mm | Harpoon head |
| C6 | Annulus | inner 216 mm, outer 224 mm, in `V` | Sonar ping ring |
| C7 | Spheres | r = 6–14 mm | Eels, mines, leviathan segments |

Registration: the player presents the mouse at 0.9–1.3 m, rotates it through at least 120° of yaw, and holds still for 0.4 s. The fitter uses the 110 mm silhouette for yaw and the 62 × 38 mm profile for roll and height; acceptance requires RMS residual < 0.6 mm, yaw error < 1.5°, and at least 28 rim samples. Re-acquisition searches a 160 mm cube around the last `P` for 1.2 s; success needs 8 rim samples with RMS < 1.0 mm over 6 consecutive frames, followed by a 100 ms pose ease-in. During loss, `P` is frozen after 200 ms and drive thrust is zeroed so the craft does not run blind.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long or axis) | Semantic output |
|---|---|---|---|---|
| I5.1 | Palm push | `R_palm`, `R_heel`, `R_middle_mcp` | φ −20° to +45°, λ 140°–220° | Planar hand velocity `v_hand` and drive force |
| I5.2 | Left click | `R_index_tip` | Left button pad, φ 15°–30°, λ +18°–+38° | `FIRE` if contact 40–120 ms |
| I5.3 | Right click | `R_middle_tip` | Right button pad, φ 15°–30°, λ −18°–−38° | `DRIFT` if contact 40–120 ms |
| I5.4 | Scroll up/down | `R_index_tip` | Wheel band, φ 45°–60°, λ −12°–+12° | `DEPTH ±1` per 3.0 mm tangential travel |
| I5.5 | Two-finger steer | `R_index_tip` + `R_middle_tip`, separation 10–24 mm | Top crest, φ ≥ +50° | Fine steer at 0.45× drive gain |
| I5.6 | Lift/coast | All right-hand contacts lost > 120 ms | — | `COAST`, thrust = 0, inertia retained |
| I5.7 | Brace | `L_palm`, `L_heel` on tray edge | `T` plane, outside prop | Anchors the clutch reference; no drive |
| I5.8 | Recentre cup | Both palms contact the mouse for > 250 ms | Any | Manual clutch: freeze 150 ms, re-centre virtual craft |

**Per-finger roles.** The right palm and heel are the only drive effectors; they must retain at least two of the three listed contacts for thrust to be valid. The right index is the fire and scroll finger; the right middle is the drift finger. The right ring and pinky never command the craft; their contacts are logged as palm support and add 0.15 to the push confidence. The left hand is a brace only: touching the mouse with the left hand triggers I5.8, not drive, so a two-handed player cannot accidentally steer with both hands.

Contact gating: a drive contact is valid when signed distance < 5 mm, age > 30 ms, and the contact normal has a positive `ẑ_T` component (top or side of the shell, not the underside). Button contacts require the same gate plus a 30 ms debounce; wheel contacts require tangential speed > 12 mm/s to reject resting contact.

## 6. Input Kinematics & Signal Processing

Drive uses the palm centroid `p_h` and its filtered velocity `v_hand`. The relative velocity is `v_rel = v_hand,t − v_P,t`, where `v_P` is the tracked prop-centre velocity in `T`. The virtual drive force is

`F = clamp(k_p · v_rel, −F_max, +F_max) · ĉ`, with `ĉ` the unit direction of `v_rel`,

where `k_p = 6.0 N·s/m`, `F_max = 1.4 N`, and a 5 mm/s dead zone with an 8 mm/s release threshold. The craft acceleration is

`a_V = (F − c_v · v_V)/m − µ·g·v̂_V` when `|v_V| > 1 mm/s`,

with `m = 0.095 kg`, `c_v = 0.55 N·s/m`, `µ = 0.42`, and `g = 9.81 m/s²`. Static friction (0.391 N) holds the craft until `|F|` exceeds it, which gives a deliberate 0.07 s breakaway at a 0.12 m/s hand push. At a 0.22 m/s hand speed the terminal physical speed is 0.142 m/s; with gain 3.0 the craft tops out at 0.43 m/s, crossing a 900 mm channel in 21 s of pure travel.

Signal chain: 72 Hz joints → One Euro filter (mincutoff 0.9 Hz, beta 0.020, derivative cutoff 6 Hz) → interpolation to the 90 Hz tick → palm centroid → finite difference → tangent projection → drive force. The filter is reset on clutch to avoid a velocity spike. Tilt is `θ_tip = acos(clamp(ẑ_P·ẑ_T, −1, 1))`; it drives a visual roll of the cockpit but is clamped to 8° and never feeds the drive.

**Motion gain and clutch.** `p_V = 3.0·(p_P − p_patch) + c_offset`. When `|p_P − p_patch|` exceeds 48 mm on either horizontal axis, the game enters CLUTCH for 150 ms: `c_offset` is updated by the virtual displacement that would have occurred, `p_V` is held, and a 32 mm re-centre animation plays under the mouse. The player can also clutch voluntarily with I5.8. Clutching is free; it costs no heat and does not cancel a sonar ping.

**Scroll detent classifier.** Tangential travel along the wheel's local `ŷ_P` is accumulated; each 3.0 mm of signed travel emits one `DEPTH ±1` event, with a 40 ms debounce and a 150 ms lockout. A 12 mm/s minimum rejects a resting finger. Five bands are available; at band 0 or 4 further scrolls play a 120 ms rubber-band tone and do not wrap.

**Click classifier.** A button event fires on release if contact duration is 40–120 ms; contacts longer than 120 ms are treated as a held button and do not auto-repeat. Minimum inter-event interval is 320 ms. A click that begins on the split line (λ within ±4°) is rejected to prevent ambiguous left/right fires.

**Drift.** A valid right click injects `J_drift = 0.020 N·s` along `ĥ × ẑ_T`, where `ĥ` is the craft heading, plus 0.008 N·s along `ĥ`. The lateral impulse changes craft velocity by 0.21 m/s; skirt friction is multiplied by 0.55 for 0.45 s. Heat rises by 26 points; heat cools at 8 points/s; at 100 points the skirt overheats for 3.0 s, thrust is capped at 0.5 N, and drift is disabled.

**Harpoon lead.** At fire time the aim direction is `d̂ = normalize(p_target − p_muzzle)`, where `p_target = p_hazard + v_hazard·t_flight` and `t_flight = |p_hazard − p_muzzle|/800 mm/s`. Lead is capped at 35 mm so a fast eel cannot drag the shot outside the 12° bow arc.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to `T` at the patch centre. `x̂_V` points down-channel, `ŷ_V` points left, `ẑ_V` is the table normal. The craft's virtual position is `p_V = 3.0·(p_P − p_patch) + c_offset`; heading `ψ = atan2(v_V·ŷ_V, v_V·x̂_V)` lags the velocity vector by 80 ms, which gives the hovercraft its characteristic sideways drift. The camera is the head pose; the channel is drawn as a 900 × 320 mm inset plane 40 mm above the physical patch, so the player sees the marsh lying on their thigh without the virtual water intersecting the leg.

| Entity | V-space position | Size | Notes |
|---|---|---|---|
| Craft | `p_V`, `z = 12 mm` | 34 × 22 mm hull | Player-controlled |
| Harpoon | muzzle `p_V + 18 mm·ĥ` | r = 4 mm, 14 mm trail | 800 mm/s, 300 mm range |
| Eel | seeded path | 26 × 8 mm | Depth band 1–4, HP 68 |
| Mine | seeded anchor | r = 9 mm | Depth band 0–3, lethal on contact |
| Leviathan | sinusoidal path | 80 × 26 mm, 6 segments | Depth band 2–4, HP 204 |
| Extraction gate | `x = 900 mm` | 90 mm wide arch | Channel goal |
| Sonar ring | centred on craft | r = 220 mm | Ping expands at 500 mm/s |
| Depth column | under craft | 5 × 16 mm bands | HUD scale, not a collider |

Mapping from prop to world is deliberately *not* 1:1. The 120 mm patch maps to 360 mm of virtual travel, and the remaining 540 mm is covered by clutch re-centring. A 1:1 mapping would make the channel 120 mm long and the craft feel like a toy; a 6× mapping would make the dead zone and tremor visible. Gain 3.0 is the largest value at which a 5 mm hand tremor stays below 2 mm of virtual jitter after filtering.

Depth is a virtual coordinate: band `k` sits at `z_V = −20·k mm`, with band 0 at the waterline. The craft and harpoon occupy exactly one band at a time. Hazards in other bands are rendered at 20 % alpha with a 1.5 mm parallax offset, which reads as "submerged" without implying that the player can hit them.

## 8. Gameplay Systems & Rules

**Entities.** Craft hull 100 HP. Harpoon damage 34. Eel HP 68 (two hits), speed 30–55 mm/s, sine amplitude 18 mm. Mine is destroyed by one harpoon hit and deals 25 hull on contact. Leviathan HP 204 (six hits), speed 22 mm/s, body radius 13 mm. Extraction gate is inert. Each channel seeds 2–4 eels, 2–4 mines, and one leviathan; the leviathan is optional on channels 1–2.

**Channel rule.** The craft starts at `x = 0`, the gate at `x = 900 mm`. The channel clock is 75 s. The gate opens only when the leviathan is dead or the craft reaches `x = 780 mm`; reaching the gate ends the channel. Hitting the gate at any speed is safe.

**Harpoon ballistics.** Position follows `p(t) = p₀ + (v₀/λ)(1 − e^(−λt))·d̂`, with `v₀ = 800 mm/s`, `λ = 0.4 s⁻¹`. The harpoon expires at `t = 0.42 s` (range 300 mm). Collision is a swept sphere of r = 4 mm against hazard spheres; a hit removes the harpoon and applies 34 damage. The harpoon is not affected by depth drag once fired; it inherits the firing band and can only hit hazards in that band.

**Sonar.** A ping fires every 1.4 s automatically and on every depth change. The ring expands at 500 mm/s to 220 mm. Contacts inside the ring are promoted from ghost to solid for 0.6 s. The ring is also the depth readout: the selected band is drawn as a bright arc, and a hazard whose band differs by one is drawn as a dashed outline. Sonar does not reveal the leviathan's true head position; its silhouette is offset by ±20 mm until the leviathan is within 120 mm.

**Depth gating.** Craft–hazard collision requires `band_craft = band_hazard`. Harpoon–hazard hit requires `band_harpoon = band_hazard`. A mismatch produces a 180 ms "muted thud" and no damage, which teaches the depth rule without a fail state. The craft can change band while moving; the 0.18 s transition has no invulnerability, so a mine at the destination band can still hit.

**Drift boost.** A drift adds 0.21 m/s lateral velocity, rotates the heading toward the drift direction by 12°, and reduces skirt friction for 0.45 s. It is the only way to clear a 26 mm mine cluster without slowing. Heat is shared with harpoon fire: each harpoon adds 7 heat, each drift adds 26, cooling is 8/s.

**Combos.** A harpoon hit within 3.0 s of the previous hit increments the combo. `multiplier = 1 + 0.25·min(combo, 4)`, capped at 2.0. A miss or a hull hit resets the combo to 0. The multiplier applies to harpoon damage and score, so a four-hit chain kills a leviathan in five harpoons instead of six.

**Score.**

`hit_score = 50 · multiplier · (1 + 0.5·(1 − |miss|/40 mm))`

`kill_score = 150 (eel) / 600 (leviathan) / 75 (mine)`

`channel_score = 500 + 5·hull_remaining`

`run_score = Σ channel_score + Σ kill_score + Σ hit_score − 25·hull_lost`

The hit precision term uses the perpendicular miss distance of the harpoon at the hazard plane, capped at 40 mm; a centre hit is worth 1.5×. The run score floors at 0.

**Win/lose.** Complete five channels with hull > 0 to win. Hull 0 is immediate loss. Channel clock 0 is a loss of that channel and costs 20 hull; the run continues if hull remains. This makes the timer a resource rather than a hard fail, and prevents a single slow channel from ending an otherwise good run.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load marsh atlas, start tracker | PRESENT |
| PRESENT | prop registered | RMS < 0.6 mm | Show mouse ghost on patch | CALIBRATE |
| CALIBRATE | palm push held 1.0 s | `|v_hand| < 10 mm/s` | Set patch centre, zero offset | CHANNEL_INTRO |
| CHANNEL_INTRO | timer 2.5 s | channel ≤ 5 | Show channel number and depth legend | RUN |
| RUN | tick | clock > 0 | Drive, sonar, fire, hazards | RUN |
| RUN | `x ≥ 900` and gate open | hull > 0 | Gate VFX, bank score | CHANNEL_END |
| RUN | hull ≤ 0 | — | Explosion, freeze controls | GAME_OVER |
| RUN | clock = 0 | — | Penalty 20 hull, show "channel lost" | CHANNEL_END |
| RUN | patch edge | `|Δp| > 48 mm` | Freeze 150 ms, re-centre | CLUTCH |
| CLUTCH | timer 150 ms | — | Restore drive, resume clock | RUN |
| CHANNEL_END | timer 2.0 s | channel < 5 | Tally, repair 10 hull (cap 100) | INTERMISSION |
| CHANNEL_END | channel = 5 | hull > 0 | Results panel | RESULTS |
| INTERMISSION | timer 10 s | — | Show depth tip, next channel | CHANNEL_INTRO |
| GAME_OVER | tap "retry" | — | Reset hull 100, keep channel 1 | CHANNEL_INTRO |
| ANY | tracker loss > 1.5 s | — | Pause, zero thrust, prompt | RECOVERY |
| RECOVERY | re-acquired | RMS < 1.0 mm | Resume with 0.5 s grace | previous |

Timers advance on the 90 Hz fixed tick. The channel clock is wall-clock-latched and is not extended by a frame stall; a stall is clamped to two substeps (22.2 ms), and the state machine processes the elapsed time afterward. The clutch timer is the only timer that pauses the channel clock, because it is an input-mechanics event, not a gameplay event.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 prop | Spheroid 31/31/19 mm | Tracked `P` | Depth proxy; no rigid-body response |
| C5 harpoon | Sphere r = 4 mm | Kinematic ballistic | 800 mm/s, 0.42 s life |
| C7 eel | Capsule 26 × 8 mm | Seeded sine path | 30–55 mm/s |
| C7 mine | Sphere r = 9 mm | Static | Contact trigger |
| C7 leviathan | 6 spheres r = 13 mm | Sinusoidal chain | 22 mm/s |
| C4 fingertip | Capsule r = 1.8 mm | Tracked | Contact only |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C5 × C7 hazard | swept sphere vs. sphere/capsule, same depth band | 34 damage, harpoon despawns, spark burst |
| C5 × gate | segment vs. plane | Harpoon despawns, no score |
| C0 craft × C7 mine | circle vs. circle, same band | 25 hull, mine despawns, 0.3 s control shake |
| C0 craft × C7 eel | circle vs. capsule, same band | 12 hull, eel knocked back 40 mm |
| C0 craft × C7 leviathan | circle vs. chain, same band | 18 hull, craft pushed 60 mm |
| C4 × C0 | signed distance < 5 mm | Contact event, drive/click gate |
| C7 × C7 | ignored | Hazards pass through each other |
| C5 × C0 | ignored | Harpoon originates outside the hull |

**Materials.** Water = drag only, no restitution. Hull = "armored skirt": restitution 0.10 against hazards, friction 0.20. Eel = "soft body": restitution 0.0, absorbs 60 % of harpoon momentum (cosmetic). Mine = "steel drum": restitution 0.15, spawns 20 debris points. Gate = "energy arch": no collision, absorbs the harpoon.

**CCD.** A harpoon moves 8.9 mm per 90 Hz tick, larger than the 8 mm eel radius, so harpoon–hazard tests are swept spheres. The craft moves at most 4.8 mm/tick at 0.43 m/s, below the 9 mm mine radius, so discrete circle tests are sufficient but a 2 mm skin is added to prevent tunnelling at clutch boundaries. Hazards move at most 0.9 mm/tick and use discrete tests.

**Forces.** The drive force is capped at 1.4 N, giving a 14.7 m/s² peak acceleration on a 95 g virtual craft. Skirt drag `c_v·v` is 0.24 N at top speed, and Coulomb friction is 0.391 N; the craft coasts 17 mm after a release from 0.43 m/s. Drift injects 0.21 m/s laterally and the skirt's 0.45 s low-friction window reduces the lateral deceleration to 1.8 m/s², producing the characteristic 38 mm slide. The 2.5 mm visual tilt is cosmetic; no tipping torque is simulated.

## 11. Rendering, Occlusion & Compositing

Layer order, near to far: passthrough base → scene mesh (thigh, tray) → virtual channel plane (40 mm above the patch) → water surface → submerged hazards → prop depth proxy → near-water reeds and spray → hand mesh → HUD and sonar ring. The prop proxy writes depth at the fitted spheroid only, not at the 110 mm hull, so the mouse's front and rear ends do not occlude the channel incorrectly.

**Occlusion cases.**

- **O1 — Channel behind the real mouse.** The water plane is depth-tested against the 62 × 62 × 38 mm spheroid. The hull hides the marsh directly under it; a 3 mm contact shadow is drawn on the water at the spheroid's equator to seat the craft visually.
- **O2 — Hand in front of the cockpit.** The tracked hand mesh wins the depth test. The palm is drawn over the hull and HUD; a 1.5 mm rim light on the hull prevents the hand from looking detached.
- **O3 — Harpoon behind a reed.** Reeds are virtual and lie in the near-water layer. A harpoon that passes behind a reed is clipped by the reed's depth; its trail fades over 60 ms so the player does not lose the shot direction.
- **O4 — Sonar ring under the hull.** The ring is drawn at `z = 1 mm` above the water and clipped by the prop proxy, so the half of the ring under the mouse is hidden and the half in front reads as a sweep.
- **O5 — Thigh/tray below the water.** The scene mesh is opaque; the water plane never draws over the leg. A 2 mm soft edge at the patch border hides the render-plane seam.
- **O6 — HUD versus hand.** The depth column and combo counter are rendered at 260 mm from the eye and depth-tested against the hand. A raised index finger correctly covers the depth column, which is why the column is duplicated as a 2 mm strip along the near edge of the patch.

**VFX budgets.** Water caustics: one 512 × 512 scrolling normal map, 0.8 ms. Sonar ring: 96 quads, 0.3 ms. Harpoon trail: 12 quads per harpoon, max 4 active. Eel bioluminescence: 2 sprites per eel, max 4. Mine blink: 1 sprite each, max 4. Explosion: 180 particles, max 2 concurrent. Total transparent overdraw target < 2.2× screen area. No refraction over passthrough; the water is an additive tint plus caustics so the real thigh remains visible and the craft feels anchored.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Thrust start | skirt | 90 Hz fan whoosh, 180 ms, gain ∝ `|F|` | Skirt dust puffs, 3 sprites |
| Thrust stop | skirt | 60 Hz decay, 120 ms | Dust settles over 200 ms |
| Harpoon fire | bow | 1.8 kHz pneumatic snap + 300 Hz thump | 14 mm muzzle flash, 3 sparks |
| Harpoon hit eel | impact point | Wet 220 Hz thud + 2.2 kHz sizzle | Cyan splash, 10 droplets |
| Harpoon hit mine | impact point | 70 Hz metal boom, 300 ms | Orange flash, 20 debris points |
| Harpoon miss | water | 400 Hz plop, 90 ms | 6 mm ripple ring |
| Depth change | sonar drum | Detent click, pitch +40 Hz per band | Depth column snaps, band ring brightens |
| Sonar ping | craft | 1.1 kHz sonar pong, 500 ms decay | Expanding ring, 220 mm |
| Drift boost | skirt | Rising 200→600 Hz whoosh, 250 ms | Lateral spray fan, 12 sprites |
| Overheat | skirt | 1.4 kHz hiss + 80 Hz rattle | Skirt glows red, heat bar flashes |
| Hull hit | hull | 45 Hz sub drop + crunch, 400 ms | Red vignette, 2 mm camera shake |
| Gate reach | gate | Major arpeggio, 900 ms | Arch flares, channel score flies up |
| Channel lost | HUD | Descending minor third, 600 ms | Clock cracks, hull −20 flash |
| Tracker loss | head-locked | 180 Hz muted pulse, repeating | "Show the mouse" prompt |

All world cues are spatialised with a 1.2 m rolloff; HUD cues are dry. The sonar ping is deliberately the loudest cue in the mix (−6 dBFS peak) because it is the primary information channel. The game never uses haptics; the harpoon "snap" and the mine "boom" are the substitutes for the physical recoil of a bow rail.

## 13. Difficulty Progression & Tuning

| Level | Eels | Mines | Leviathan | Hazard speed | Channel clock | Sonar period | Notes |
|---|---|---|---|---|---|---|---|
| 1 | 2 | 2 | 0 | 25 mm/s | 90 s | 1.2 s | Teaching: depth gate is generous |
| 2 | 3 | 3 | 1 | 32 mm/s | 80 s | 1.4 s | First leviathan, 4 hits to kill |
| 3 | 3 | 4 | 1 | 40 mm/s | 75 s | 1.4 s | Baseline spec |
| 4 | 4 | 4 | 1 | 48 mm/s | 70 s | 1.6 s | Denser mines, faster eels |
| 5 | 4 | 5 | 1 | 55 mm/s | 65 s | 1.8 s | Finale: 5 mines, 2 concurrent eels |

Named constants: `MASS = 0.095 kg`, `K_PUSH = 6.0 N·s/m`, `F_MAX = 1.4 N`, `MU = 0.42`, `DRAG = 0.55 N·s/m`, `GAIN = 3.0`, `PATCH_EDGE = 48 mm`, `CLUTCH_TIME = 0.150 s`, `HARPOON_SPEED = 800 mm/s`, `HARPOON_RANGE = 300 mm`, `FIRE_COOLDOWN = 0.320 s`, `DRIFT_IMPULSE = 0.020 N·s`, `HEAT_DRIFT = 26`, `HEAT_FIRE = 7`, `COOL_RATE = 8/s`, `OVERHEAT_TIME = 3.0 s`, `HULL = 100`, `BANDS = 5`.

Adaptive rule: after each channel, compute hull remaining and time remaining. If hull ≥ 70 and the channel was cleared with ≥ 15 s left, raise hazard speed by 4 mm/s (cap 55) and add one mine (cap 5) for the next channel. If hull ≤ 30 or the channel was lost, lower hazard speed by 4 mm/s (floor 25) and remove one mine (floor 2). Adaptation never changes `K_PUSH`, `MU`, or `GAIN`, because the physical feel of the mouse must stay constant across the run.

## 14. Edge Cases & Failure Recovery

- **Mouse lifted.** All drive contacts lost > 120 ms → `COAST`. If the mouse is lifted > 40 mm above `T` for > 300 ms, thrust is zeroed and the craft brakes at 2.0 m/s²; the game continues, because lifting is a legitimate way to stop. A 2.5 s lift triggers a "set the mouse down" prompt but does not pause.
- **Patch edge during a clutch.** If the player is already at the edge and the clutch re-centre would place the virtual craft inside a hazard, the re-centre is deferred up to 400 ms; if still blocked, the craft is placed at the nearest safe point 20 mm upstream and a 120 ms invulnerability is granted.
- **Tracking loss mid-channel.** `P` freezes after 200 ms, thrust goes to zero, and the channel clock continues for 1.5 s; then the state machine pauses. Re-acquisition restores 0.5 s of grace. If loss exceeds 8 s, the channel restarts at the last checkpoint (every 300 mm) with hull unchanged.
- **Hand occlusion of the wheel.** If the index tip disappears behind the palm for > 150 ms during a scroll, the partial detent is discarded and no depth change is emitted. This prevents a phantom band step when the player regrips.
- **Scroll wheel physically stuck.** The real mouse is switched off, so there is no wheel encoder; only contact motion is read. A stuck wheel is therefore invisible to the game. If the index finger rests on the wheel with > 12 mm/s tremor for 2.0 s, the detector raises its threshold to 20 mm/s until the finger lifts.
- **Left-handed player.** Mirror mode places the patch on the right thigh, swaps the fire/drift fingers (middle fires, index drifts), mirrors the atlas λ about 0°, and flips the sonar ring's chirality. The drive math is unchanged because it uses `T`-frame vectors.
- **Dark room / low texture.** The mouse's matte graphite shell can starve visual odometry. The tracker falls back to the two button caps and the wheel as features; if confidence drops below 0.4, motion gain is reduced to 2.0 and a "hold still to re-fix" prompt appears after 3 s.
- **Player pushes faster than 0.6 m/s.** The One Euro filter saturates the drive at `F_max`; the craft cannot exceed 0.43 m/s. The excess hand motion is absorbed by the clutch at the patch edge, so a fast shove never teleports the craft through a mine.
- **Frame stall > 22.2 ms.** Physics clamps to two substeps, and the channel clock is advanced by wall time. The harpoon uses the substep position at the impact time, not the frame endpoint, so a stall cannot make a hit tunnel through an eel.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — registers the mouse, emits `PoseFrame {p_P, q_P, confidence, mode}` at 72 Hz.
- `ContactBus` — converts 26 joints into `Contact {joint, p_P, n_P, v_rel, age}` with 5 mm enter / 8 mm exit and 30 ms debounce.
- `PlanarDrive` — computes `v_rel`, `F`, `a_V`, clutch state, and `p_V`; owns the 3.0× gain.
- `DepthSonar` — band state, scroll detents, ping timer, contact promotion.
- `HarpoonSystem` — cooldown, lead solve, ballistic integration, swept collision, damage.
- `HazardDirector` — seeds eels, mines, and the leviathan per channel; owns sine paths.
- `ChannelStateMachine` — the table in §9; owns clocks and checkpoints.
- `ScoreCombo` — hit precision, combo, channel and run score.
- `FeedbackRouter` — maps every event enum to one audio clip and one VFX prefab.
- `Compositor` — layer order, spheroid depth proxy, occlusion masks O1–O6.

**Event flow.** `PropTracker` → `PoseFrame` → `PlanarDrive`; `ContactBus` → `PlanarDrive` and `DepthSonar`; `PlanarDrive` → `HarpoonSystem` and `HazardDirector`; `HarpoonSystem` → `ScoreCombo`, `HazardDirector`, `FeedbackRouter`; `HazardDirector` → `ChannelStateMachine` → `FeedbackRouter`. The state machine reads only `PlanarDrive.p_V`, `HarpoonSystem.hits`, `HazardDirector.remaining`, and timer values; it never reads raw joints.

**Pseudocode 1 — per-tick drive and clutch.**

```
function tick_drive(dt):
    pose = PropTracker.latest()
    if pose.mode == LOST:
        thrust = 0
        if now - pose.last_good > 1.5s: enter(RECOVERY)
        return

    palm = ContactBus.centroid(DRIVE_JOINTS)        # requires >= 2 of 3
    v_hand = OneEuro.velocity(palm, dt)
    v_rel = v_hand - PlanarDrive.v_P
    if |v_rel| < 5 mm/s: v_rel = 0
    F = clamp(6.0 * v_rel, -1.4 N, +1.4 N) * normalize(v_rel)

    if |PlanarDrive.v_P| > 1 mm/s:
        a = (F - 0.55*PlanarDrive.v_P) / 0.095 - 0.42*9.81*normalize(v_P)
    else:
        a = 0 if |F| < 0.391 N else (F - 0.391*normalize(F)) / 0.095
    PlanarDrive.v_P += a*dt
    PlanarDrive.p_P += PlanarDrive.v_P*dt

    offset = pose.p_P - patch_centre
    if |offset.x| > 48mm or |offset.y| > 48mm:
        enter(CLUTCH)                                # 150 ms freeze
        c_offset += PlanarDrive.p_P - clutch_anchor
        PlanarDrive.p_P = clutch_anchor
```

**Pseudocode 2 — harpoon, depth, and hit resolution.**

```
function fire():
    if now < next_fire or heat >= 100: return
    muzzle = craft.p_V + 18mm*heading
    band = sonar.band
    for h in hazards_in_band(band):
        t_lead = |h.p - muzzle| / 800mm/s
        aim = normalize(h.p + h.v*t_lead - muzzle)
        if angle(aim, heading) <= 12deg:
            target = h; break
    if target == null: aim = heading
    harpoons.add(Harpoon(muzzle, 800mm/s*aim, band, life=0.42s))
    heat += 7
    next_fire = now + 0.320s
    FeedbackRouter.play(FIRE, muzzle)

function resolve_harpoon(hp, dt):
    p0 = hp.p
    hp.p += hp.v*dt
    hp.v *= exp(-0.4*dt)
    for hz in hazards_in_band(hp.band):
        if swept_sphere_hit(p0, hp.p, 4mm, hz):
            hz.hp -= 34
            FeedbackRouter.play(HIT, hp.p)
            if hz.hp <= 0: HazardDirector.kill(hz)
            return DESPAWN
    if hp.age > 0.42s: return DESPAWN
```

**Determinism.** Hazard seeds use a PCG32 stream keyed by `(session_id, channel, hazard_index)`. Given a recorded `PoseFrame` and `Contact` trace, the simulation replays bit-identically at 90 Hz, which the acceptance tests rely on.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Spheroid registration | Present the mouse at 1.0 m; inject 28 synthetic rim points with 0.3 mm noise | RMS < 0.6 mm; yaw error < 1.5°; fit converges < 1.5 s |
| AT-02 | Drive terminal speed | Synthetic palm at 0.22 m/s for 2 s on a static patch | Physical speed 0.142 ± 0.010 m/s; virtual 0.426 ± 0.030 m/s |
| AT-03 | Static friction breakaway | Ramp palm velocity from 0 to 0.12 m/s over 1 s | No motion below `F = 0.391 N`; motion starts within 0.12 s after |
| AT-04 | Clutch | Drive to +50 mm, hold 300 ms, release | Freeze 150 ± 10 ms; re-centre 32 mm; no virtual displacement during freeze |
| AT-05 | Harpoon range | Fire into empty water from a stationary craft | Harpoon despawns at 0.42 s ± 1 tick; maximum travel 300 ± 8 mm |
| AT-06 | Depth gating | Place a mine at band 2; fire at band 1, then band 2 | Band 1: no damage, thud cue; band 2: mine destroyed, 34 damage event |
| AT-07 | Scroll detent | Move index 3.1 mm along wheel; repeat at 2.8 mm | 3.1 mm emits one step; 2.8 mm emits none; 40 ms debounce holds |
| AT-08 | Drift impulse and heat | Right click at rest; log velocity and heat | Δv = 0.21 ± 0.02 m/s lateral; heat +26 ± 1; friction low for 0.45 s |
| AT-09 | Overheat | Drift five times in 2 s | Heat reaches 100; drift disabled 3.0 ± 0.1 s; thrust cap 0.5 N |
| AT-10 | Combo and damage | Hit four eels within 3 s, then miss | Multiplier reaches 2.0 on fourth; miss resets combo to 0 |
| AT-11 | Hull loss | Drive into three mines at band 0 | Hull 100 → 25; `GAME_OVER` on the fourth mine |
| AT-12 | Occlusion readback | Place the mouse over a harpoon; read depth buffer at hull centre | Harpoon fragment contribution = 0 inside the spheroid silhouette; trail fades at limb |
| AT-13 | Tracking loss | Drop `PoseFrame` for 2.0 s during RUN | Thrust 0 after 200 ms; pause at 1.5 s; 0.5 s grace on re-acquire |
| AT-14 | 10-minute soak | Scripted five-channel run at 90 Hz | Frame p99 < 11.1 ms; allocation < 1 MB/tick; no harpoon tunnelling |

All tests run headless against the deterministic simulation with synthetic pose and contact streams. A test passes only when both the numeric assertion and the emitted event log match the expected sequence. AT-12 is validated by a depth-buffer readback, not a screenshot.
