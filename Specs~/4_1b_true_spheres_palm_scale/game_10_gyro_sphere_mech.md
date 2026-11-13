# Game 10 — Gyro-Sphere Mech — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Gyro-Sphere Mech |
| Slug | `gyro-sphere-mech` |
| Physical prop | Orange, true sphere, 76.0 mm diameter (r = 38.0 mm); textured peel, 150 g |
| Family / sub-shape | Family 4 Spherical, Ellipsoidal & Domed / 4.1b True Spheres, Palm-Scale |
| Scale class | Palm-scale two-hand cup; prop 1:1; dome simulation R = 500 mm, rendered at 0.35× |
| Source concept | "Armored hamster-ball mech; both palms cup the sphere and twist pitch/roll/yaw to drive and aim twin chin lasers at waves of cyber-bugs on a virtual dome." |
| Physical input | Both palms cup the orange; wrist twist yaws the mech, palm shear drives it, squeeze sets throttle, right index taps the chin to fire |
| Player posture | Seated, ball low at 150 mm above the table, dome 450 mm forward |
| Session length | 5 waves × 35–55 s + 3 s inter-wave ≈ 4 min |
| Win condition | Clear all 5 waves with at least 1 armor pip |
| Lose condition | Armor reaches 0 |
| Primary metric | Bugs killed per armor pip lost |
| Tracking budget | 2 palm capsules, 2 index tips, 1 prop pose |

## 1. Fantasy & Core Loop

The orange is the cockpit of a gyro-sphere mech: a 60 mm armored hamster ball with twin chin lasers, rolling across a 500 mm battle dome. The player's two palms are the mech's gyro gimbal. Cupping the ball and twisting the wrists pitches, rolls, and yaws the machine; squeezing the palms drives power to the omni-wheels; a tap of the right index on the chin patch fires the lasers. The dome is a translucent planetoid on the table, its surface crawling with cyber-bugs that converge on the mech from every azimuth.

The control fantasy is direct: the ball's orientation is the mech's heading, the shear between the palms is its velocity, and the squeeze is the throttle. There are no buttons and no abstraction layers. The player looks at the dome, turns the ball to aim, leans the palms to strafe, and squeezes to close distance. The chin lasers are fixed forward, so aiming and driving are the same act.

1. **Spin up (0.8 s).** The wave card counts down; palms cup; throttle at 0.
2. **Read the dome (1.0 s).** Bugs spawn at 180–260 mm geodesic distance; red arcs show their approach azimuth.
3. **Turn (0.2–0.6 s).** Wrist yaw rotates the mech; heading follows the prop's relative yaw.
4. **Drive (continuous).** Palm shear rolls the ball; forward/back and lateral speed follow the two shear axes.
5. **Fire (0.28 s cadence).** Tap the chin patch; twin geodesic beams sweep 170 mm forward.
6. **Clear (3 s).** Wave ends when all spawned bugs die; armor +1; next wave seeds.

The tension is between aiming and moving: the mech's lasers only cover a 7.8° corridor, so the player must line up each bug while strafing away from the others. Turning the ball too far twists the wrists; the clutch re-grip lets the player reset without losing heading.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the center, poles at `±ẑ_P`, equator in `x̂_P`–`ŷ_P`, no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]` from the equator, longitude `λ ∈ [0°, 360°)` from +x̂_P toward +ŷ_P; `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; outward normal `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P-frame**: origin at the orange centre; `+ẑ_P` is the **chin** (mech forward), `+ŷ_P` is world up, `+x̂_P` is the player's left so that `x̂_P × ŷ_P = ẑ_P`. At registration the chin points along `−ẑ_W` (away from the player). Poles `±ẑ_P`; equator in `x̂_P`–`ŷ_P`.
- **Radius and mass**: `r = 38.0 mm`, `m = 150 g`, `I = 2mr²/5 = 8.664 × 10⁻⁵ kg·m²`. Collider r = 38.0 mm; registration tolerance ±0.6 mm.
- **Rolling kinematics**: palm shear spins the ball; each palm contact contributes `ω = (n × v_t)/r`. The two contributions are averaged, projected onto `P`, and decomposed as `ω_x` (forward/back shear), `ω_y` (yaw twist), `ω_z` (lateral shear). `ω` is fused with peel-texture odometry at 72 Hz and clamped to ±420 °/s. The mech's drive uses the **shear** components `ω_x, ω_z`; the mech's heading uses the **relative yaw** of `P`.
- **V-frame anchoring**: `V` is anchored to `T`, not to `P`. Origin `V_O = T_O + (0, 300 mm, 0)`; axes aligned with `W`. The dome is centred at `V_O` with simulation radius `R_dome = 500 mm`; the prop is only a controller and never moves the arena.
- **Deviation 1 — render scale**: all gameplay distances, speeds, and collision tests use the 500 mm simulation dome. The render transform scales dome-relative positions by `S = 0.35`, so the visible dome is 350 mm across and sits inside the tabletop envelope. HUD values are quoted in simulation mm.
- **Deviation 2 — the prop is not the mech**: the orange stays in the palms; the mech is a separate 60 mm ball on the dome. Prop orientation drives the mech but does not place it.
- **Deviation 3 — clutch re-grip**: the wrists cannot yaw indefinitely. Releasing both palms for > 250 ms and re-cupping resets `P0` and the heading reference `Ψ0` without changing the mech's position or wave state.
- **Deviation 4 — lasers are geodesic**: chin lasers follow great circles on the dome, not straight world rays, because the arena is a sphere.

## 3. Physical Play Space & Ergonomics

The player sits with both elbows supported and both palms cupping the orange at 150–180 mm above the table, 240–300 mm forward of the sternum. The palms face each other and slightly inward; the fingers wrap the sides, leaving the chin patch (the forward 60° cap) exposed to the right index. The dome is rendered 450 mm forward and 300 mm above the table, so the line of sight passes over the ball. The head pitches down 18–24°; the dome top is at 10–15° below the eye line.

| Parameter | Value | Rationale |
|---|---|---|
| Ball centre | 270 mm forward, 165 mm above table | Below the sight line to the dome |
| Palm separation `D` | 96–128 mm | Cups a 76 mm ball without crushing |
| Neutral `D0` | measured at calibration | Personalizes throttle to hand size |
| Throttle range | 22 mm of squeeze | Large enough to modulate, small enough to reach |
| Wrist yaw | ±150° before clutch | Avoids forearm over-rotation |
| Dome centre | 450 mm forward, 300 mm above table | Visible without neck strain |
| Visible dome | 350 mm diameter at `S = 0.35` | Fits the tabletop envelope |
| Inter-wave rest | 3.0 s | Wrist extensor recovery |

Ergonomic guard: if either wrist exceeds 55° of ulnar deviation or 60° of pronation for 2.0 s, steering gain drops to 65 % and a "re-grip" prompt appears. The clutch is always available and costs no score.

## 4. Prop Geometry & Registration

- **Real dimensions**: 76.0 mm diameter, 150 g, textured orange peel with 1.2 mm dimples at 4 mm pitch. The peel is the odometry feature set.
- **Sphere model**: `|p_P| = 38.0 mm`. The chin patch is the cap within 30° of `+ẑ_P`; it carries a 2.0 mm twin-barrel glyph at `±5 mm` from centre and a 0.5 mm trigger ring at 30°. The rest of the surface is a dark armored shell with mech insignia.
- **Mech model**: 60 mm diameter (30 mm radius) armored ball, rendered on the dome surface at `S = 0.35` → 21 mm visible. Twin chin barrels at `±5 mm` simulation offset, 4 mm long.
- **Colliders**: `COL_sphere` r = 38.0 mm (both palms and the firing index); `COL_palm_L/R` capsules r = 17 mm, l = 62 mm; `COL_index_R` swept capsule r = 5.5 mm, l = 15 mm. The mech is a 30 mm sphere; bugs are 6 mm spheres; both are analytic on the dome.
- **Registration**: the player presents the orange with the twin-barrel glyph facing away from the body. The system fits the centre over 8 frames (< 0.6 mm), detects the glyph, and sets `+ẑ_P` to the glyph axis within 2°. `P0` is recorded as the neutral orientation; `Ψ0 = 0` points along the dome's local north.
- **Palm baseline**: at calibration the player cups the ball naturally; the system records `D0` as the mean distance between `L_palm` and `R_palm` over 1.0 s. Throttle is relative to `D0`, so hand size does not affect gain.
- **Tolerance**: centre < 0.6 mm; radius < 0.6 mm; chin axis < 2°; `D0` repeatability < 3 mm. A chin error over 2° triggers a 1.5 s "show the barrels" prompt.
- **Tracking**: peel odometry at 72 Hz; re-acquisition searches 60 mm for 1.2 s and blends 100 ms.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Two-palm cup | `L_palm` + `R_palm` capsules | west/east hemispheres | controller valid; enables drive |
| I5.2 | Wrist yaw | both palms, opposite tangential drag | shell | mech heading `Ψ` |
| I5.3 | Palm shear forward | both palms drag along `−x̂_P` | shell | `ω_x > 0`, forward drive |
| I5.4 | Palm shear lateral | both palms drag along `±ŷ_P` | shell | `ω_z`, lateral strafe |
| I5.5 | Squeeze | `D` between palm centres | shell | throttle `T ∈ [0,1]` |
| I5.6 | Chin tap | `R_index_tip` inside 30° of `+ẑ_P` | chin patch | fire salvo, 0.28 s cooldown |
| I5.7 | Brace tap | `R_middle_tip` on chin | chin patch | alternate fire, same cooldown |
| I5.8 | Clutch re-grip | both palms lost > 250 ms, then re-cup | shell | reset `P0`, `Ψ0`, `D0` |
| I5.9 | Release | both palms lost > 400 ms | — | throttle 0, mech brakes |
| I5.10 | One-hand hold | one palm only | shell | drive disabled, fire enabled |

Per-finger roles: the right index is the trigger; the right middle is an alternate trigger for fatigue; the left index and both thumbs are braces and never fire. Per-hand roles: both hands are symmetric for drive and throttle; the right hand is the gun hand. A one-handed hold disables drive (the mech brakes) but keeps the lasers live, which is a deliberate recovery mode.

## 6. Input Kinematics & Signal Processing

- **Cup gate**: at least one palm capsule within 6 mm of the shell and a palm-to-shell contact area ≥ 120 mm². Two palms enable drive; one palm enables fire only.
- **Contact gate for the trigger**: `s = |x_index − C| − r`; contact if `s < 5 mm`, release at `s > 8 mm`, debounce 30 ms.
- **Trigger tap**: `D = clamp(5.0 − s, 0, 12.0) mm`; a tap requires peak `D ≥ 4.0 mm`, duration ≤ 150 ms, and the contact normal within 30° of `+ẑ_P`. Cooldown 0.28 s; a held press does not auto-fire.
- **Shear angular velocity**: for each palm contact, `ω_k = (n_k × v_tk)/r`. Average `ω = 0.5(ω_L + ω_R)`; project onto `P`: `ω_x = ω·x̂_P`, `ω_y = ω·ŷ_P`, `ω_z = ω·ẑ_P`. Deadband 8 °/s; EMA `τ = 35 ms`; clamp each component to ±420 °/s.
- **Drive velocity**: `v_fwd = clamp(r·ω_x, −220, 220) mm/s`, `v_lat = clamp(r·ω_z, −220, 220) mm/s`. At 60 °/s these are 39.8 mm/s; to reach the 220 mm/s cap the player must shear at 332 °/s.
- **Throttle**: `T = clamp((D0 − D)/22.0, 0, 1)`, where `D` is the current palm-centre distance in mm. At `D = D0` throttle is 0; at `D = D0 − 22` throttle is 1; a 2 mm squeeze gives 0.09. The 22 mm range spans the comfortable palm compression for a 76 mm ball.
- **Commanded velocity**: `v_cmd = T·(v_fwd·ĥ + v_lat·ê_r)`, where `ĥ` is the mech heading tangent and `ê_r = n̂ × ĥ` is its right. Clamp `|v_cmd| ≤ 220 mm/s`.
- **Heading**: relative yaw `ψ = atan2(f·x̂_W, −f·ẑ_W)` where `f = R_P·ẑ_P`, unwrapped and clamped to ±150°. `Ψ = Ψ0 + ψ`; a soft spring pulls `ψ` toward 0 at 4 °/s when no palm contact is present, preventing drift while still allowing a held heading.
- **Pitch and roll**: `θ = asin(f·ŷ_W)`, and `ρ` is the signed roll of `R_P·ŷ_P` about `f`. These are not used for driving (drive comes from shear), but they are used for the mech's visual banking and for the ergonomic guard.
- **Bug hit test**: a bug at unit position `n̂_b` is in the laser corridor if the geodesic angle from the mech `n̂_m` along `ĥ` is ≤ `170/R_dome = 19.48°` and the perpendicular angular offset is ≤ `3.9°` (3.2° beam half-width + 0.69° bug radius). Twin barrels add ±0.57° but the corridor is evaluated as one 3.9° band.
- **Laser cadence**: 0.28 s between salvos; each salvo applies 1 damage to every bug in the corridor. A 2-HP bug requires two salvos.
- **Clutch detection**: both palm contacts lost for 250–400 ms, then both re-established within 150 ms. The clutch resets `P0 = P`, `Ψ0 = Ψ`, `D0 = D`; it does not reset the wave, armor, or score.

## 7. Virtual World Layout & Coordinate Mapping

- **V frame**: anchored to `T`; `V_O = T_O + (0, 300, 0) mm`; axes equal to `W`. Dome centre at `V_O`.
- **Dome**: simulation hemisphere radius 500 mm; only `n̂_y ≥ 0.15` is playable, giving a 4.2° rim above the equator and keeping every entity visible from above. Rendered at `S = 0.35` (175 mm radius) with a hex-grid surface, a 2 mm rim, and a translucent 18 % shell.
- **Mech**: 30 mm simulation sphere at `R_dome·n̂_m`; heading `ĥ`; rendered at 10.5 mm. Its chin barrels are at `±5 mm` simulation, aligned with `ĥ`.
- **Bugs**: 6 mm simulation spheres at `R_dome·n̂_b`; rendered at 2.1 mm. Each has a 3 mm red eye and a 5 mm approach trail. Up to 12 alive.
- **Laser**: a geodesic ribbon 3.2° half-width, 170 mm long, 0.6 mm thick, rendered as a 60 ms pulse. The twin barrels produce two 0.8 mm ribbons at ±5 mm, but the hit test uses the combined 3.9° corridor.
- **Dome grid**: 24 meridians × 12 parallels, 0.4 mm lines, used for range reading. Every 10th line is brighter; a 50 mm ring marks the mech's spawn.
- **HUD**: a 64 mm × 22 mm strip at 18° below the eye line, showing armor pips, wave, bugs remaining, throttle bar, and heading compass. A red arc at the dome rim shows off-screen bug azimuths.
- **Scale note**: the HUD reports simulation distances in mm; the rendered dome is 0.35×, so a 170 mm laser looks 59.5 mm long on the table.

## 8. Gameplay Systems & Rules

- **Entities**: `Mech` (n̂_m, ĥ, armor, throttle), `Bug` (n̂_b, hp, speed, spawn phase), `WaveDirector` (5 waves, spawns, timers), `LaserSystem` (corridor test, cadence), `GyroController` (shear, yaw, squeeze).
- **Mech motion**: `n̂_m ← normalize(n̂_m + (v_cmd·dt)/R_dome)`; the heading angle `Ψ` is held in the local dome frame. The mech cannot leave `n̂_y ≥ 0.15`; at the rim it slides along the boundary and the HUD shows "rim".
- **Bug motion**: each bug crawls along the geodesic toward the mech at its speed. Tangential direction `t̂ = normalize(n̂_m − (n̂_m·n̂_b)n̂_b)`; `n̂_b ← normalize(n̂_b + (speed·dt/R_dome)·t̂)`. Bugs never stop, never collide with each other, and ignore the rim.
- **Bug contact**: if the geodesic distance `R_dome·acos(n̂_m·n̂_b) < 36 mm`, armor −1, the bug explodes, and a 0.6 s armor-flash plays. The mech is not pushed.
- **Laser salvo**: on a valid chin tap, every bug within the corridor takes 1 damage; hp 0 bugs explode (+100 for hp1, +250 for hp2). One salvo can kill multiple bugs; this is the "line them up" skill.
- **Waves**: 5 waves. Wave 1 spawns 6 bugs (max 5 alive), wave 5 spawns 18 (max 12 alive). Spawn interval 1.4–0.8 s; spawn distance 180–260 mm; spawn azimuth uniform but at least 25° from the mech's current heading.
- **Wave clear**: all spawned bugs dead and none alive → +400, armor +1 (cap 6), 3.0 s inter-wave. Wave 5 clear → win.
- **Armor**: 6 pips, no regeneration during a wave. At 0 the mech collapses and the session ends at the current wave.
- **Throttle**: at `T = 0` the mech brakes to 0 in 0.25 s; at `T = 1` the full shear speed is available. The throttle bar is the primary HUD readout after armor.
- **Scoring**: bug +100/+250; multi-kill (≥3 in one salvo) +150; no-damage wave +300; wave clear +400; armor remaining ×200; accuracy bonus `1 + kills/(salvos·2)` capped ×1.5.
- **Win/lose**: win after 5 waves with armor ≥ 1; lose at armor 0. A failed wave can be retried once per session from the wave start with armor reset to 3.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load wave table | REGISTER |
| REGISTER | prop presented | centre < 0.6 mm | fit sphere, find chin glyph | CALIBRATE |
| CALIBRATE | cup held 1.0 s | palm area ≥ 120 mm² | record `P0`, `D0`, `Ψ0` | BRIEF |
| BRIEF | cup stable | — | wave card 2.5 s | WAVE |
| WAVE | wave start | — | spawn queue, armor check | WAVE |
| WAVE | chin tap | cooldown 0, corridor | fire salvo, damage bugs | WAVE |
| WAVE | bug contact | distance < 36 mm | armor −1, bug dies | WAVE |
| WAVE | armor = 0 | — | mech collapse, fail | DEBRIEF |
| WAVE | all bugs dead | spawned = killed | +400, armor +1, 3 s rest | REST |
| REST | 3.0 s elapsed | wave < 5 | next wave seed | WAVE |
| REST | 3.0 s elapsed | wave = 5 | win, tally | DEBRIEF |
| WAVE | both palms lost 400 ms | — | throttle 0, brake | COAST |
| COAST | re-cup < 1.5 s | palm area ≥ 120 mm² | restore drive, clutch | WAVE |
| COAST | 1.5 s timeout | — | "re-cup the orange" card | COAST |
| WAVE | one palm only | — | drive disabled, fire enabled | WAVE |
| any | prop tracking loss > 150 ms | — | freeze bugs, ghost ball | REACQUIRE |
| REACQUIRE | relock ≤ 1.2 s | search 60 mm | blend 100 ms | prior |
| REACQUIRE | 1.2 s timeout | — | "present the orange" card | REACQUIRE |
| DEBRIEF | wave < 5, retry unused | — | retry card, armor = 3 | WAVE |
| DEBRIEF | wave = 5 or retry used | — | session tally | COMPLETE |
| any | system pause | — | freeze all timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: fire cooldown 0.28 s; clutch 250–400 ms; brake 0.25 s; inter-wave 3.0 s; re-cup 1.5 s; reacquisition 1.2 s; brief 2.5 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_sphere` r = 38 mm; 2 palm capsules r = 17 mm, l = 62 mm; 1 right-index swept capsule r = 5.5 mm, l = 15 mm; mech sphere r = 30 mm on the dome; up to 12 bug spheres r = 6 mm; dome shell analytic (R = 500 mm).
- **Pair matrix**: Palm↔Sphere = drive/shear/throttle; Index↔Sphere = fire; Mech↔Bug = armor damage at 36 mm; Laser↔Bug = geodesic corridor; Bug↔Bug = ignored; Mech↔Dome = constrained to the shell and the `n̂_y ≥ 0.15` cap; Hand↔Mech = visual only.
- **Mech body**: kinematic on the dome; its velocity is the commanded tangent vector, with 0.25 s brake and 0.15 s acceleration ramp. No gravity, no rolling inertia; the "hamster ball" look is visual.
- **Bug body**: kinematic geodesic crawler; speed constant per wave; no steering beyond the great-circle direction to the mech. Contact is a distance threshold, not a physical collision.
- **Laser**: analytic geodesic corridor; no projectile travel time. The 60 ms ribbon is visual only; damage is applied on the fire tick.
- **CCD**: the mech moves ≤ 2.44 mm/tick at 220 mm/s; bugs ≤ 0.47 mm/tick at 42 mm/s; the mech-bug distance test is exact at tick resolution. The index tap is swept at ≤ 1.0 m/s = 11.1 mm/frame.
- **Materials**: orange peel µ = 0.62, palm µ = 0.55, dome µ = 0.4 (visual only). No force feedback; a bug hit applies a 0.02 N·s visual impulse to the mech shell.
- **Dome constraint**: `n̂_m` is renormalized after every move. At `n̂_y < 0.15`, the tangential component pointing outward is removed, so the mech slides along the rim rather than leaving the visible hemisphere.
- **Scale transform**: simulation positions `p_sim` map to render `p_render = V_O + S·(p_sim − V_O)` with `S = 0.35`; velocities are not scaled in the simulation, only in the render interpolation.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → table/scene mesh → dome back shell → dome grid → bugs → mech → laser ribbons → dome front rim → hands → HUD.
- **Occlusion case 1 — ball held below the sight line**: the orange and both palms sit below the dome; they occlude only the dome's lower rim. If the ball rises above 220 mm and covers > 15 % of the dome, a "lower the ball" hint appears and the dome brightens.
- **Occlusion case 2 — hands over the chin patch**: the right index covers the trigger ring during a tap. The twin barrels are drawn 2 mm above the shell and the muzzle flash is rendered at the barrels, not under the finger.
- **Occlusion case 3 — bugs behind the dome rim**: back-facing bugs are culled; a red arc at the rim shows the azimuth of every off-screen bug within 40 mm of the rim. Bugs never become untargetable without a cue.
- **Occlusion case 4 — mech behind the dome horizon**: the mech is constrained to `n̂_y ≥ 0.15`, so it is always on the visible hemisphere; at the rim, its lower half is clipped and a 1.5 mm outline is drawn.
- **Occlusion case 5 — laser ribbon through the dome**: the ribbon is depth-tested against the dome shell and clamped to the surface; it never floats in front of the rim.
- **Occlusion case 6 — HUD vs hands**: the HUD is 18° below the eye line; if a palm covers > 20 % of it, the HUD slides 10° toward the screen edge.
- **VFX budgets**: 12 bugs × 8 glow sprites = 96; mech trail 20; laser 2 ribbons; muzzle flash 8; bug explosion 24 × 3 = 72; armor flash 6; rim arcs 8; global cap 260. The dome is 1,152 triangles; the mech is 2,400 triangles; each bug is 320 triangles.
- **Readability**: the dome is desaturated blue-grey; bugs are hot magenta with a 4 mm red eye; the mech is warm amber; lasers are cyan. The color separation survives passthrough at 60 % virtual opacity.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Cup valid | both palms | soft servo lock | throttle bar appears |
| Yaw turn | ball surface | gyro whir, pitch ∝ |ψ̇| | heading compass rotates |
| Drive | ball surface | rolling rumble, pitch ∝ speed | mech trail, dome grid streaks |
| Squeeze | palms | pneumatic hiss | throttle bar fills |
| Chin tap | chin patch | sharp trigger click | muzzle flash, 2 barrels |
| Laser fire | mech | twin zap, 60 ms | cyan geodesic ribbons |
| Bug hit | bug | chitin crack | 24-particle burst |
| Multi-kill | mech | rising triple chime | +150, screen sparkle |
| Bug contact | mech | armor crunch + alarm | armor −1, red vignette |
| Armor 2 | HUD | low warning pulse | last 2 pips blink |
| Wave clear | HUD | bronze fanfare | +400, armor +1 |
| Clutch re-grip | ball | magnetic re-center | heading preserved |
| Rim | dome edge | low scrape | "rim" HUD tag |
| Win | HUD | five-note ascent | tally card |
| Fail | HUD | collapsing gyro whine | mech collapse |

The laser cadence is audio-led: the 0.28 s cooldown is exactly one beat at 214 BPM, and the trigger click is layered with a 60 ms tail so the player can hear when the next salvo is ready without watching the HUD.

## 13. Difficulty Progression & Tuning

| Level | Wave 1 bugs | Wave 5 bugs | Max alive | Bug speed (mm/s) | Bug HP | Spawn gap (s) | Armor |
|---|---|---|---|---|---|---|---|
| 1 | 4 | 12 | 8 | 16 | 1 | 1.8 | 6 |
| 2 | 5 | 14 | 9 | 20 | 1 | 1.6 | 6 |
| 3 | 6 | 18 | 12 | 22 | 1 (w1–3), 2 (w4–5) | 1.4 | 6 |
| 4 | 7 | 20 | 13 | 30 | 2 | 1.2 | 6 |
| 5 | 8 | 24 | 14 | 42 | 2 | 0.9 | 5 |

- **Adaptive rule** (`GYRO_ADAPT`): after each wave, if armor lost ≤ 1 and clear time < 35 s, level +1; if the wave is failed, level −1 and bug speed ×0.9. Clamp 1–5; one step per wave.
- **Spawn rule**: azimuth is uniform but at least 25° from the current heading, so the player is forced to turn; no more than 3 bugs spawn within a 40° sector at once.
- **Named constants**: `R_ORANGE = 38 mm`, `R_DOME = 500 mm`, `S_RENDER = 0.35`, `R_MECH = 30 mm`, `R_BUG = 6 mm`, `V_MAX = 220 mm/s`, `OMEGA_REF = 60 °/s`, `TAP_D = 4 mm`, `FIRE_CD = 0.28 s`, `LASER_RANGE = 170 mm`, `BEAM_HALF = 3.2°`, `BUG_HIT = 36 mm`, `ARMOR = 6`.
- **Tuning intent**: at level 3, a player who fires every 0.35 s and turns at 90 °/s kills 18 bugs in 40–55 s with 2–3 armor lost. The 220 mm/s drive cap is intentionally faster than any bug, so repositioning is always possible; the limiting resource is aim time, not speed.

## 14. Edge Cases & Failure Recovery

- **Chin tap outside the 30° cap**: ignored; a rim highlight shows the valid trigger zone. No cooldown is consumed.
- **Tap held for > 150 ms**: treated as a brace, not a shot; the trigger ring turns grey and no salvo fires.
- **Both palms lost mid-wave**: throttle goes to 0 within one tick and the mech brakes over 0.25 s; fire remains enabled for 1.5 s, then the wave pauses. Re-cupping within 1.5 s restores drive and performs a clutch reset.
- **One palm only**: drive is disabled but fire works; this is the recovery mode if one hand is occluded. The HUD shows a one-hand icon.
- **Clutch during a fire tap**: if both palms are lost while the index is still on the chin, the clutch waits 100 ms for the tap to resolve; the shot is not lost.
- **Wrist yaw beyond ±150°**: heading clamps; a "re-grip to turn" prompt appears. The player can clutch and continue in the same direction with a new `P0`.
- **Bug reaches the rim**: bugs are not constrained to `n̂_y ≥ 0.15`; if a bug crawls below the visible rim, it is drawn as a red arc at the rim and continues to the mech, so it never becomes invisible.
- **Mech at the rim**: the outward velocity component is removed, the mech slides along the rim, and the drive speed is reduced by 40 %. The player can reverse out.
- **Prop tracking loss > 150 ms**: freeze bugs and mech; keep heading `Ψ` and throttle; on relock, blend 100 ms. In-flight lasers are already resolved and are not re-applied.
- **Ball dropped**: if the ball leaves both palms and falls, the wave pauses at the current tick; on retrieval, re-register and resume. If the pose is lost > 3 s, the wave restarts with armor unchanged.
- **Scale confusion**: the HUD always shows simulation mm; the dome's 50 mm ring is labelled "50" and the render scale is stated in the brief. Tests assert simulation values, never rendered pixels.
- **Armor overflow**: armor is capped at 6; a wave clear at 6 shows "+0" rather than hiding the reward.
- **Recovery UX**: one icon (ball / palm / hand) plus one line; no modal during a wave.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropOrange` — pose `P`, peel odometry, chin glyph, `P0`, radius.
  - `GyroController` — palm contacts, shear `ω`, yaw `ψ`, throttle `T`, clutch, ergonomic guard.
  - `DomeFrame` — `V_O`, `R_dome`, local north/east basis, rim constraint.
  - `MechBody` — `n̂_m`, `ĥ`, `Ψ`, armor, brake/accel ramps.
  - `BugPool` — 14 agents, geodesic crawl, hp, spawn/despawn.
  - `LaserSystem` — corridor test, twin barrels, cadence, multi-kill scoring.
  - `WaveDirector` — 5 waves, spawn queue, clear/rest, adaptive level.
  - `DomeRig` — dome shell, grid, mech, bugs, laser ribbons, rim arcs.
  - `MechHud` — armor, wave, throttle, compass, bug count.
- **Event flow**: `HeadsetTracker → PropOrange → GyroController → MechBody`; `WaveDirector → BugPool → LaserSystem`; `LaserSystem / BugPool` emit `EventBus {Fire, BugHit, MultiKill, BugContact, WaveClear, ArmorLow, Win, Fail}` to `DomeRig / AudioBank / MechHud / ScoreKeeper`.
- **Pseudocode — gyro control (per tick)**:

```text
if bothPalmsContact:
    omega = 0.5 * (cross(nL, vtL) / R + cross(nR, vtR) / R)
    ox = dot(omega, xP); oy = dot(omega, yP); oz = dot(omega, zP)
    vFwd = clamp(R * ox, -220, 220)
    vLat = clamp(R * oz, -220, 220)
    T = clamp((D0 + 8.0 - D) / 22.0, 0, 1)
    psi = unwrap(atan2(dot(f, xW), -dot(f, zW)))
    psi = clamp(psi, -150 deg, 150 deg)
    heading = psi0 + psi
    vCmd = T * (vFwd * hHat + vLat * cross(nM, hHat))
    vCmd = clampLength(vCmd, 220)
else:
    vCmd = 0; T = 0
nM = normalize(nM + (vCmd * dt) / R_DOME)
if dot(nM, up) < 0.15: projectToRim(nM)
```

- **Pseudocode — laser and bugs (per tick)**:

```text
if chinTap and fireCooldown <= 0:
    fireCooldown = 0.28; salvos += 1
    for bug in bugs where bug.alive:
        d = R_DOME * acos(clamp(dot(nM, bug.n), -1, 1))
        if d > 170: continue
        along = normalize(bug.n - dot(bug.n, nM) * nM)
        if dot(along, hHat) <= 0: continue
        off = acos(clamp(dot(along, hHat), -1, 1))
        if off <= 3.9 deg:
            bug.hp -= 1; emit(BugHit, bug)
            if bug.hp <= 0: bug.alive = false; kills += 1; score += bug.value
for bug in bugs where bug.alive:
    t = normalize(nM - dot(nM, bug.n) * bug.n)
    bug.n = normalize(bug.n + (bug.speed * dt / R_DOME) * t)
    if R_DOME * acos(dot(nM, bug.n)) < 36: armor -= 1; bug.alive = false
```

- **Tick order**: prop pose → gyro control → mech move → bug move → bug contact → fire input → laser damage → wave state → HUD. Moving bugs before the laser test means a shot is evaluated against the positions the player last saw plus one tick of motion, which is the intended 11 ms lead.
- **Telemetry**: per wave log `salvos`, `kills`, `multiKills`, `armorLost`, `meanThrottle`, `meanSpeed`, `turnIntegral`, `clutchCount`, and `rimTime`. The clutch count is the primary ergonomic tuning input.

## 16. Acceptance Test Checklist

1. Registration fits the orange centre to < 0.6 mm and the chin axis to < 2° in 20/20 presentations; `D0` repeats within 3 mm across 10 calibrations.
2. A steady palm shear of 120 °/s about `x̂_P` yields `v_fwd = 79.6 ± 1.0 mm/s`; about `ẑ_P` it yields `v_lat = 79.6 ± 1.0 mm/s`.
3. A shear of 400 °/s clamps each drive component at 220 mm/s; the commanded vector never exceeds 220 mm/s in magnitude.
4. Throttle at `D = D0` is 0.00; at `D = D0 − 22` it is 1.00; at `D = D0 − 14` it is 0.64 ± 0.01; a 2 mm squeeze produces 0.09 ± 0.01.
5. A wrist yaw of +90° sets heading to +90° ± 1°; a yaw beyond +150° clamps at +150° and shows the re-grip prompt.
6. A clutch re-grip (both palms off 300 ms, re-cup 100 ms) resets `P0`, `Ψ0`, and `D0` while preserving mech `n̂`, armor, and wave state.
7. A chin tap with peak `D = 4.1 mm` and 100 ms duration fires one salvo; a 200 ms press does not fire; the 0.28 s cooldown blocks a second tap at 0.20 s.
8. A bug at 100 mm geodesic distance and 3.5° off heading takes 1 damage; at 4.1° it takes none; at 171 mm it is out of range.
9. A 2-HP bug dies on the second salvo, not the first; a salvo that catches 3 bugs awards exactly +150 multi-kill.
10. A bug reaching 36 mm costs exactly 1 armor and dies; armor never exceeds 6 on a wave clear.
11. The mech cannot cross `n̂_y = 0.15`; at the rim, outward velocity is removed and speed is reduced by 40 %.
12. Spawn azimuth is always ≥ 25° from the current heading; no more than 3 bugs spawn within a 40° sector in one wave.
13. At level 3, a scripted 18-bug wave is clearable in ≤ 55 s with ≥ 3 armor remaining in 5/5 replays at 0.35 s fire cadence.
14. 12 bugs + 3 explosions + 2 laser ribbons + 20 trail sprites stays ≤ 260 particles and ≤ 11.1 ms frame time at 90 Hz; the 0.35 render scale keeps the visible dome ≤ 350 mm across.
