# Game 7 — Trench Run Globe — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Trench Run Globe |
| Slug | `trench-run-globe` |
| Physical prop | Tennis ball, true sphere, 67.0 mm diameter (r = 33.5 mm); felt surface, 58 g |
| Family / sub-shape | Family 4 Spherical, Ellipsoidal & Domed / 4.1b True Spheres, Palm-Scale |
| Scale class | Palm-scale one-hand spin; prop 1:1; trench rendered 1:1 on the felt |
| Source concept | "A metallic battle-station trench wraps the ball. The right fingertip is a starfighter skimming the surface; rotate the ball beneath it to dodge turbolaser towers and dive into the exhaust port." |
| Physical input | Left palm continuously rotates the ball; right index rests on the top point and reads the surface track |
| Player posture | Seated, left hand palm-up at 240–300 mm, right index on the top pole |
| Session length | 5 sectors × (2 laps + 3 s approach) ≈ 55 s active, 3 min with debriefs |
| Win condition | Hit the exhaust port in at least 3 of 5 sectors |
| Lose condition | Shields reach 0 in a sector; the sector is failed but the session continues |
| Primary metric | Port hits, then total shield pips remaining |
| Tracking budget | 1 prop pose, 1 left palm cradle, 1 right index read head |

## 1. Fantasy & Core Loop

The tennis ball is a 160 km battle station shrunk into the palm. A dark trench is cut along one great circle; its walls are 12° of latitude tall, its floor is 24° wide, and turbolaser towers sit on both rims at fixed longitudes. The player's right index is the starfighter: it never leaves the top of the ball, but the felt slides beneath it as the left palm rolls the sphere. Every millimetre of longitude the contact travels is a kilometre of trench.

The control is a physical read head. The ball rotates under a nearly stationary fingertip; the fingertip's latitude and longitude in the prop frame are the fighter's cross-track and down-range position. Rolling the ball forward scrolls the trench; tilting the ball sideways eases the fighter toward a rim. The exhaust port is a 6° disc at the registration meridian; it opens only after two complete laps, so the player must survive the tower fire of two orbits before the dive.

1. **Seat the read head (0.3 s).** The right index settles on the top point; contact quality and airspeed readout appear.
2. **Spin up (0.5–1.5 s).** The left palm rolls the felt; the trench begins to scroll and the starfield streaks.
3. **Dodge (continuous).** Towers within 38 mm ahead telegraph for 0.55 s; the player shifts cross-track into a shoulder lane to let the bolt pass.
4. **Hold the line (2 laps).** A full lap is 210.49 mm of trench; two laps arm the port.
5. **Dive (0.8 s).** Crossing the port within 3° of centre at 40–170 mm/s triggers the torpedo sequence.
6. **Debrief (4 s).** Distance, dodges, shields and port result; the next sector seeds towers.

The fantasy collapses if the finger presses hard: a hard press pins the felt and the ball stops reading. The player must keep the index light, using the left palm for all torque. The game is therefore a two-hand coordination task disguised as a rail shooter.

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

- **P-frame**: origin at the tennis-ball centre. `+ẑ_P = −x̂_W` (the player's left pole), `+ŷ_P = +ŷ_W` (world up), `+x̂_P = +ẑ_W` (toward the player). Poles `±ẑ_P`; equator is the world-vertical `ŷ_P`–`x̂_P` plane at registration. The **read head anchor** is `A = C + r·ŷ_P`, which is exactly `φ = 0°, λ = 90°`.
- **Radius and mass**: `r = 33.5 mm`, `m = 58 g`, `I = 2mr²/5 = 2.603 × 10⁻⁵ kg·m²`. Collider r = 33.5 mm; registration tolerance ±0.5 mm.
- **Trench coordinates**: `u = r·(90° − λ)` unwrapped forward, `v = r·φ`. The lap length is `L = 2πr = 210.49 mm`. The port sits at `u ≡ 0 (mod L)`, `v = 0`, with a 3.0° angular radius (6° diameter).
- **Rolling kinematics**: the ball is spun by the left palm and lightly read by the right index. For each driving contact, `ω = (n × v_t)/r`; palm contacts dominate with weight 0.8 and the read-head contact contributes 0.2, because the index is deliberately light. `ω` is fused with felt-texture odometry at 72 Hz, clamped to 900 °/s, and integrated into `R_P`. The contact point's prop coordinates evolve as `ṗ_P = −ω × p_P`.
- **V-frame anchoring**: `V = P` for the trench, towers, bolts, port and station surface. The starfield and the distant battle backdrop are anchored to `W` and parallax with head motion. The HUD is head-relative but clipped to a 22° box below the eye line.
- **Deviation 1 — `+ẑ_W` means world up**: the brief's "finger stays near `+ẑ_W`" is realized as `+ŷ_W` because the platform declares `W` Y-up. The read-head anchor is therefore `C + r·ŷ_W`; all formulas use `ŷ_P`, never a raw `ẑ_W`.
- **Deviation 2 — time compression**: a 210 mm lap represents 160 km of trench. The mapping is 1 mm of surface travel = 760 m of virtual travel; speeds on the HUD are shown in km/s but computed in mm/s.
- **Deviation 3 — the read head may slip**: the index is not a no-slip pivot. Contact slip is expected and is the control signal; only the prop's odometry and the contact's `P`-frame track define motion.
- **Deviation 4 — no yaw authority from the finger**: the fighter's heading is always the local trench tangent `ê_u`. The player steers only cross-track; there is no free-flight mode.

## 3. Physical Play Space & Ergonomics

The player sits with the left forearm supported and the left palm up, fingers gently cupped. The tennis ball rests in the palm hollow; the left thumb and ring finger form a shallow cradle that lets the palm roll the felt without gripping. The right hand approaches from above, index extended, middle relaxed, and the index pad rests on the top of the ball. The right wrist is in 15–25° extension; the elbow is supported. The head looks down 18–24°, and the ball sits 240–300 mm in front of the sternum, 30–70 mm above the support surface.

| Parameter | Value | Rationale |
|---|---|---|
| Ball centre | 270 mm forward, 55 mm above table | Keeps both elbows below shoulder |
| Left palm wrap | 120–150° of the lower hemisphere | Enough friction to spin, not enough to hide the trench |
| Right index contact | within 12° of `+ŷ_W` from the centre | Keeps the read head on the trench |
| Index normal force (inferred) | 0.15–0.6 N | Light enough to slip, heavy enough to track |
| Finger travel in world | ≤ 25 mm from the anchor | Prevents the player from "drawing" the path |
| Max ball spin | 900 °/s (≈ 526 mm/s surface speed) | Upper bound of comfortable palm roll |
| Session posture breaks | 6 s between sectors | Wrist recovery |

Ergonomic guard: if the right index contact is more than 18° from the anchor for > 1.0 s, the game pauses the trench scroll and shows a "return to the top" ring. If the left wrist exceeds 60° pronation for 2.0 s, spin gain is halved and a rotate-the-forearm hint appears.

## 4. Prop Geometry & Registration

- **Real dimensions**: 67.0 mm diameter, 58 g, felt nap 1.1 mm, seam 0.4 mm wide. The felt texture is the odometry feature set; the seam is a natural landmark but is not required for tracking.
- **Sphere model**: `|p_P| = 33.5 mm`. The station atlas is a 1:1 wrap of the felt: a 24°-wide trench band centred on `φ = 0`, two 3° ramps, and armored plating elsewhere. Towers are 3.0 mm raised boxes; the port is a 3.0° disc with a 0.4 mm recess.
- **Trench geometry**: floor `|φ| ≤ 9.5°` (5.55 mm half-width), shoulder lanes `4.8° < |φ| ≤ 9.5°`, wall at `|φ| = 9.5°` rising 1.6 mm. The danger lane for bolts is `|v| < 3.2 mm` (5.47°); the safe shoulder is `3.2 mm < |v| ≤ 5.55 mm`.
- **Tower hardpoints**: 8 per lap at `φ = ±12.5°`, evenly spaced every 26.31 mm in `u`, alternating rims. A sector arms between 4 and 8 of them.
- **Colliders**: `COL_sphere` r = 33.5 mm for the read head and the left palm; `COL_index` swept capsule r = 6.0 mm, l = 16 mm; `COL_palm_L` capsule r = 18 mm, l = 68 mm. Towers and walls are analytic angular bands, not rigid colliders.
- **Registration**: the player presents the ball at 280 mm with the seam horizontal. The system fits the centre over 8 frames (error < 0.7 mm) and sets `+ŷ_P` to world up, `+ẑ_P` to the player's left, and the seam to `λ = 0°`. This puts the read head at `λ = 90°` and the port at the start meridian.
- **Tolerance**: centre < 0.7 mm; radius < 0.5 mm; trench plane within 2.0° of the computed vertical plane. If the trench plane is off by more than 2°, a 1.5 s "level the seam" card appears before the first sector.
- **Re-acquisition**: search 60 mm around the last pose for 1.2 s; on relock, preserve the integrated orientation, do not re-derive it from the seam, and blend the position over 100 ms.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Palm cradle | `L_thumb_tip`, `L_ring_tip`, `L_palm` | lower hemisphere | support; enables spin torque |
| I5.2 | Read head | `R_index_tip` | trench top | contact track `(u, v)`, airspeed |
| I5.3 | Roll forward | left palm tangential drag along `−x̂_P` | felt | `+u` scroll, forward airspeed |
| I5.4 | Roll reverse | left palm drag along `+x̂_P` | felt | `−u` scroll, braking |
| I5.5 | Tilt left/right | left palm drag along `±ŷ_P` | felt | cross-track `v` steer |
| I5.6 | Light read | `R_index_tip` normal force proxy `D < 2 mm` | trench top | low-friction read, full speed |
| I5.7 | Hard press | `R_index_tip` `D ≥ 4 mm` | trench top | friction brake, speed ×0.35 |
| I5.8 | Manual dive | `R_index_tip` press `D ≥ 3 mm` inside port | port disc | dive if aligned, +250 bonus |
| I5.9 | Lift read head | `R_index_tip` lost | — | stall; speed decays 3.0/s |
| I5.10 | Re-seat | read head re-contact within 15° of anchor | trench top | resume, no distance jump |

Per-finger roles: the left thumb and ring finger stabilize the ball but do not steer; the left palm is the only torque source. The right index is the sole read head; the right middle can substitute at 0.7 gain if the index is occluded, but switching costs a 0.4 s re-seat. The right thumb is ignored. Per-hand roles: left = DRIVE, right = READ.

## 6. Input Kinematics & Signal Processing

- **Contact gate**: `s = |x_index − C| − r`; contact if `s < 5 mm`, release at `s > 8 mm`, debounce 30 ms. A read head that is not in contact produces zero airspeed after a 120 ms coast.
- **Contact point**: `p_P = R_Pᵀ(x_index − C)`; normalize to `p̂`. Latitude `φ = asin(p̂_z)`, longitude `λ = atan2(p̂_y, p̂_x)`. Then `v = r·φ` and `u_raw = r·(π/2 − λ)`.
- **Unwrap**: `u_n = u_{n−1} + wrap(u_raw − u_{n−1}, −L/2, +L/2)`. A single-frame jump with `|Δu| > 0.35L` is classified as a re-seat, not travel; distance is not awarded and no tower is triggered.
- **Airspeed**: `a = clamp((u_n − u_{n−1})/dt, −180, 520) mm/s`, then EMA with `τ = 25 ms`. Reverse is allowed at up to 180 mm/s but earns no distance. The HUD shows `a × 760 m/mm` in km/s.
- **Cross-track rate**: `v̇ = (v_n − v_{n−1})/dt`, clamped to ±240 mm/s; used only for the steering VFX, not for collisions.
- **Trench classifier**: on the floor if `|v| ≤ 5.55 mm`; on the shoulder if `3.2 < |v| ≤ 5.55`; scraping if `|v| > 5.55`; outside the trench if `|v| > 7.0` (a 2° buffer) and held for 150 ms.
- **Tower approach**: `Δu = (u_t − u_n) mod L`. A tower is eligible when `10 mm ≤ Δu ≤ 38 mm`, it is armed, and its reload is complete. Eligibility triggers a 0.55 s telegraph.
- **Bolt model**: at telegraph end the tower locks `u_lock = u_n` and spawns a bolt with a 0.55 s flight. At impact, the player is hit if `|u_n − u_lock| < 5.0 mm` **and** `|v| < 3.2 mm`. Moving 5 mm along the trench or 3.2 mm across it avoids the hit; the shoulder is the intended dodge.
- **Port classifier**: port is armed when `u_progress ≥ 2L`. The player is aligned when the angular distance to the port centre is `< 3.0°`, `|v| < 2.0 mm`, and `40 ≤ a ≤ 170 mm/s`. Crossing `u ≡ 0` while aligned triggers the dive; if `a > 170 mm/s` the dive is an overshoot and the port stays armed for the next lap.
- **Scrape**: `|v| > 5.55 mm` applies shield −0.8/s and sets airspeed gain to 0.75. A 2° buffer prevents chatter at the wall.
- **Read-head force proxy**: `D = clamp(5.0 − s, 0, 12.0) mm`; `D ≥ 4.0` classifies a hard press and multiplies airspeed by 0.35. This gives the player a physical brake without a button.
- **Deadband**: `|a| < 4 mm/s` reads as zero so the trench does not creep while the player holds still.

## 7. Virtual World Layout & Coordinate Mapping

- **V frame**: `V = P`. Origin `C`, axes `x̂_P, ŷ_P, ẑ_P`. The station surface is the ball surface; nothing is scaled up.
- **Trench ribbon**: a 24°-wide band on `φ = 0`, 1.6 mm walls, 0.7 mm floor inset, emissive edge strips at `|φ| = 5.2°` and `9.5°`. The ribbon is P-fixed and therefore scrolls exactly with the felt.
- **Starfield**: a 900 mm radius W-anchored shell of 1,200 points, rendered at 20 % brightness behind the ball. It does not rotate with the station, so the player perceives the ball spinning.
- **Fighter marker**: a 2.2 mm chevron at the contact point, 0.5 mm above the felt, oriented along `ê_u = ẑ_P × p̂`. A 14 mm exhaust trail points backward along `−ê_u`.
- **Towers**: 3.0 mm × 1.8 mm × 1.2 mm boxes on the rims at `φ = ±12.5°`, with a 1.2 mm red emitter facing the trench. Eight per lap, alternating rims.
- **Bolts**: 1.0 mm × 0.6 mm capsules that cross the trench transversely at `u_lock`; flight time 0.55 s, visible from 0.25 s before impact.
- **Port**: a 3.0° disc at `u = 0`, `v = 0`, with a 2.0 mm concentric ring and a 0.6 mm recess. It is dark until armed, then pulses at 0.8 Hz.
- **HUD**: a 60 mm × 24 mm strip at 22° below the eye line, showing speed in km/s, shields as 3 chevrons, lap count, and port range in mm. The HUD is not attached to the ball.
- **Camera**: no virtual camera; the player looks at the ball. The fighter is always under the index, so the player's gaze naturally follows the fingertip. A 4° fisheye-free magnifier loupe is available by holding the ball at 180 mm from the eye, but it is off by default.

## 8. Gameplay Systems & Rules

- **Entities**: `Station` (R_P, u_progress), `ReadHead` (contact, u, v, airspeed), `Tower` (u_t, rim, armed, reload, telegraph), `Bolt` (u_lock, t_impact, resolved), `Port` (armed, aligned), `SectorDirector` (lap count, shields, score).
- **Sector flow**: 5 sectors. Each begins with a 3.0 s approach during which the trench is visible but towers are safe. Then the player must complete 2 laps (`u_progress ≥ 2L = 420.97 mm`) and cross the armed port.
- **Tower fire**: each armed tower has a 3.0 s reload. It can fire only when the read head is 10–38 mm behind it and at most 2 bolts are in flight. Telegraph 0.55 s (emitter charge + rising tone), then a bolt.
- **Bolt hit**: at impact, `|u − u_lock| < 5.0 mm` and `|v| < 3.2 mm` → shield −1, 1.4 s invulnerability, screen flash, speed ×0.5 for 0.6 s. A near miss with `3.2 ≤ |v| < 4.2 mm` awards +80 and a "shave" cue.
- **Wall scrape**: `|v| > 5.55 mm` drains 0.8 shield/s and slows the ship to 75 %. At `|v| > 7.0 mm` for 150 ms, the ship is "off-trench": shield −1 and a 0.8 s auto-recenter to `v = 0`.
- **Port dive**: armed after 2 laps. Crossing `u ≡ 0` with `|v| < 2.0 mm` and 40–170 mm/s triggers a 2.5 s torpedo sequence: the camera does not move, but the trench lights up, the port iris opens, and the station explodes in a 1.2 s bloom. Sector cleared.
- **Overshoot**: crossing the port above 170 mm/s or with `2.0 ≤ |v| < 3.0 mm` causes a "missed shot" cue; the port remains armed and the player must fly one more lap. Crossing with `|v| ≥ 3.0 mm` is a clean miss with no penalty beyond the extra lap.
- **Shields**: 3 per sector, reset at sector start. At 0 the sector fails; the session advances to the next sector. Hits during the 1.4 s invulnerability are ignored.
- **Scoring**: +10 per 10 mm of forward travel; +80 shave; +150 port hit; +250 manual dive; +40 per remaining shield; sector clear +1,000; speed bonus `max(0, 300 − sectorTime·10)`.
- **Win/lose**: session win if ≥ 3 of 5 sectors are cleared. A failed sector still counts toward the 5; there is no retry inside a session.
- **Minimum speed**: below 8 mm/s for 4.0 s, a "spin the ball" prompt appears; the trench does not self-scroll.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load 5 sector seeds | REGISTER |
| REGISTER | ball presented | centre < 0.7 mm | fit sphere, set trench plane | CALIBRATE |
| CALIBRATE | seam horizontal | trench plane ≤ 2° | set `P0`, arm read head | BRIEF |
| BRIEF | read head contact | `s < 5 mm` | sector card 2.5 s | APPROACH |
| APPROACH | 3.0 s elapsed | — | towers arm, speed HUD live | RUN |
| RUN | tick | read head contact | integrate u/v, fire towers | RUN |
| RUN | tower eligible | reload ready, ≤ 2 bolts | telegraph 0.55 s | RUN |
| RUN | bolt impact | `|u−u_lock|<5` and `|v|<3.2` | shield −1, invuln 1.4 s | RUN |
| RUN | shield = 0 | — | sector failed, explosion | DEBRIEF |
| RUN | `|v| > 5.55` | — | scrape −0.8/s, speed ×0.75 | RUN |
| RUN | `|v| > 7.0` for 150 ms | — | off-trench: shield −1, recenter | RUN |
| RUN | port crossing | armed, aligned, 40–170 mm/s | torpedo, +1,000 | DIVE |
| RUN | port crossing | armed, `a > 170` | overshoot cue, +1 lap | RUN |
| RUN | read head lost | — | stall, speed → 0 at 3.0/s | STALL |
| STALL | read head re-contact | within 15° of anchor, < 1.5 s | resume, no jump | RUN |
| STALL | 1.5 s timeout | — | "place index on top" card | STALL |
| DIVE | 2.5 s elapsed | — | bloom, sector tally | DEBRIEF |
| DEBRIEF | sector < 5 | — | next sector card | BRIEF |
| DEBRIEF | sector = 5 | — | session tally | COMPLETE |
| any | prop tracking loss > 150 ms | — | freeze trench, ghost ball | REACQUIRE |
| REACQUIRE | relock ≤ 1.2 s | search 60 mm | blend 100 ms | prior |
| REACQUIRE | 1.2 s timeout | — | "present the ball" card | REACQUIRE |
| any | system pause | — | freeze timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: telegraph 0.55 s; bolt flight 0.55 s; invulnerability 1.4 s; reload 3.0 s; stall coast 0.12 s; off-trench 150 ms; reacquisition 1.2 s; dive 2.5 s; approach 3.0 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_sphere` r = 33.5 mm; `COL_index` swept capsule r = 6 mm, l = 16 mm; `COL_palm_L` capsule r = 18 mm, l = 68 mm; 8 tower emitter points; 1 port disc; up to 2 bolt capsules.
- **Pair matrix**: Index↔Sphere = contact track; Palm↔Sphere = spin torque and support; Bolt↔Ship = analytic `(u,v)` test; Tower↔Ship = approach interval; Ship↔Trench wall = `|v|` threshold; Bolt↔Tower = ignored; Hand↔Tower = visual occlusion only.
- **Prop body**: kinematic while cradled in the left palm. If the palm loses contact for > 250 ms, the ball becomes dynamic (gravity 9810 mm/s², restitution 0.55, felt friction 0.55) and the sector pauses. A dropped ball is retrieved from the table or lap with no penalty.
- **Inertia**: `I = 2.603 × 10⁻⁵ kg·m²`; used for the spin-up blend, not for gameplay. The angular response is `ω_target` from palm drag, blended with `τ = 60 ms` to avoid odometry spikes.
- **CCD**: the read head can move at 1.2 m/s = 13.3 mm/frame, less than `r`; the contact is resolved by a swept segment-sphere test. Bolts are analytic time-to-impact and cannot tunnel because the hit check is evaluated at the exact impact tick.
- **Materials**: felt µ = 0.55, palm µ = 0.55, station wall µ = 0.2 (visual only). The read head is modelled as low-friction (µ_eff = 0.15) because the finger is deliberately light; a hard press raises µ_eff to 0.7 and triggers the brake.
- **Forces**: no force feedback. The palm spin torque is limited to 0.02 N·m to avoid implying a physical resistance the prop does not have. A tower hit applies a 0.03 N·s visual impulse to the fighter marker only.
- **Coordinate singularity**: near `|φ| = 90°` the longitude is ill-conditioned. The read head is constrained to `|φ| ≤ 35°` by the ergonomic anchor; if it exceeds 35°, `u` stops advancing and a "return to trench" cue fires, so the singularity is unreachable in normal play.
- **Determinism**: tower fire decisions use the fixed 90 Hz tick and the sector seed; bolt impact is evaluated on the tick nearest `t_fire + 0.55 s`.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → table/scene mesh → starfield backdrop → station surface → trench ribbon → tower geometry → bolts → fighter marker → hands → HUD.
- **Occlusion case 1 — index over the trench**: the right index covers 10–14 mm of the felt, including the fighter marker. The marker is rendered 0.5 mm above the surface and is depth-tested; when the fingertip covers it, a 4 mm chevron outline is drawn at the contact rim so the player can still read heading.
- **Occlusion case 2 — left palm under the ball**: the palm hides the lower 35–45 % of the station. Towers on the far rim are still visible because they are on the upper hemisphere; no x-ray is used, and the player is never required to see through the palm.
- **Occlusion case 3 — port under the index**: as the port approaches the contact, it passes under the fingertip. The port ring expands to 5° and a 2 mm beacon rises 4 mm above the surface, so the alignment cue stays visible around the finger.
- **Occlusion case 4 — bolt behind a tower**: a bolt that crosses behind a 3 mm tower is correctly occluded for 2–3 frames; its impact flash is drawn at the trench floor and remains visible.
- **Occlusion case 5 — ball vs table**: if the ball is held within 50 mm of the table, the table mesh clips the lower station and the starfield; the trench remains above the table plane by construction.
- **Occlusion case 6 — HUD vs hands**: the HUD is at 22° below the eye line, outside the hands' typical silhouette; if a hand covers > 25 % of the HUD, it slides 8° toward the screen edge.
- **VFX budgets**: 1,200 star points (static), 60 tower charge motes, 24 bolt sprites, 40 explosion shards, 120 spark particles, 12 port rings; global cap 1,400. The station shader uses one 2K texture atlas and one emissive mask; no per-frame texture updates.
- **Trench shader**: baked ambient occlusion in the ribbon, emissive edge strips at 1.5 cd/m², and a 0.4 mm parallax offset for the floor inset. The ball's felt normal map is reused for micro-shading so the virtual trench looks seated on the real felt.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Read head contact | fingertip | soft felt press | chevron appears, HUD live |
| Spin-up | ball surface | low turbine whine, pitch ∝ speed | starfield streaks |
| Forward scroll | trench | rhythmic rail clack at 4 Hz | trench texture scroll |
| Steer | ball surface | airy whoosh, pan ∝ v | shoulder edge glow |
| Tower charge | tower | rising two-tone | emitter red, ring contracts |
| Bolt fire | tower | sharp zap | 1 mm bolt, 0.55 s trail |
| Bolt impact (hit) | ship | shield crack + alarm | white flash, chevron −1 |
| Near miss | ship | close pass whoosh | +80, blue shave streak |
| Wall scrape | wall | metal grind | sparks, speed gauge red |
| Off-trench | ship | loss-of-control wobble | auto-recenter, shield −1 |
| Port armed | port | deep arming thunk | iris ring pulses 0.8 Hz |
| Port aligned | port | lock tone | ring turns green |
| Dive | port | torpedo launch + explosion | 1.2 s bloom |
| Overshoot | port | descending "miss" tone | port recedes, +1 lap |
| Sector clear | HUD | bronze fanfare | tally card |

The tower charge and bolt cues are the primary readability channel: every bolt is telegraphed by both a spatial tone and a visible emitter charge, so a player who is looking at the finger can still hear the threat.

## 13. Difficulty Progression & Tuning

| Level | Active towers | Telegraph (s) | Reload (s) | Bolt hit half-width | Scrape (/s) | Port window (mm/s) | Shields |
|---|---|---|---|---|---|---|---|
| 1 | 4 | 0.80 | 3.6 | 2.6 mm | 0.5 | 40–200 | 3 |
| 2 | 5 | 0.70 | 3.3 | 2.9 mm | 0.6 | 40–190 | 3 |
| 3 | 6 | 0.55 | 3.0 | 3.2 mm | 0.8 | 40–170 | 3 |
| 4 | 7 | 0.45 | 2.7 | 3.5 mm | 1.0 | 50–160 | 3 |
| 5 | 8 | 0.35 | 2.4 | 3.8 mm | 1.2 | 60–150 | 2 |

- **Adaptive rule** (`TRENCH_ADAPT`): after each sector, if shields lost ≤ 1 and the port was hit on the first armed crossing, level +1; if the sector failed, level −1 and telegraph ×1.15. Clamp 1–5; one step per sector.
- **Tower seeding**: hardpoints are fixed; the director arms a subset with a minimum 2-hardpoint gap, alternating rims, and never arms two adjacent towers on the same rim in the same lap.
- **Named constants**: `R_TENNIS = 33.5 mm`, `L_LAP = 210.49 mm`, `LAPS_TO_ARM = 2`, `V_MAX = 520 mm/s`, `V_REVERSE = 180 mm/s`, `BOLT_T = 0.55 s`, `HIT_U = 5.0 mm`, `HIT_V = 3.2 mm`, `SHOULDER = 5.55 mm`, `PORT_R = 3.0°`, `SCRAPE_V = 5.55 mm`.
- **Tuning intent**: at level 3, a player rolling at 120–160 mm/s completes a lap in 1.3–1.8 s and faces 3–5 bolts per lap. A player who hugs one shoulder permanently will be hit by the alternating-rim seeding within two laps.

## 14. Edge Cases & Failure Recovery

- **Read head lifts**: airspeed decays at 3.0/s to zero in 0.17 s from 520 mm/s. The trench freezes; towers do not fire at a stalled ship. Re-contact within 1.5 s resumes at the current `u`; after 1.5 s the sector pauses with a placement card.
- **Read head slides off the trench**: `|v| > 7.0 mm` for 150 ms counts as off-trench, costs 1 shield, and auto-recenters `v` to 0 over 0.4 s. The ball's physical orientation is unchanged; only the virtual fighter recenters.
- **Hard press**: `D ≥ 4 mm` brakes the ship to 35 % speed and raises felt friction; this is a legitimate tactic for tight tower patterns and is taught in the brief.
- **Port overshoot**: if the player crosses above the speed window, the port stays armed and `u_progress` continues; one extra lap is required. No shield is lost.
- **Reverse across the port**: crossing `u ≡ 0` backwards never triggers a dive; the port classifier requires positive `u̇`.
- **Coordinate wrap**: `Δu` is wrapped to `[−L/2, L/2]`; a legitimate 210 mm lap is never mistaken for a re-seat. A re-seat is flagged only when the contact is lost for > 80 ms or the jump exceeds 0.35L in one frame.
- **Prop tracking loss > 150 ms**: freeze bolts and tower timers; keep `u, v` and `R_P`; on relock blend 100 ms. A bolt in flight is cancelled without damage.
- **Ball dropped**: pause the sector; on retrieval, re-register and restore `u_progress`, shields, and tower reloads. The ball must be re-presented if the pose is lost for > 3 s.
- **Left hand lost**: spin torque goes to zero and the ball may roll; the right index can still read but airspeed decays. After 2.0 s the sector pauses.
- **Both hands lost > 300 ms**: full pause, no timers advance.
- **Player spins too fast**: clamp 900 °/s; above 700 °/s the felt odometry confidence drops and the HUD shows a "slow down" texture-jitter cue. No penalty beyond reduced tracking confidence.
- **Recovery UX**: one icon (ball / index / hand) plus one line; never a modal during a run.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropTennis` — pose `P`, felt odometry, spin clamp, kinematic/dynamic mode.
  - `ReadHeadModel` — contact gate, `p_P`, `u`, `v`, unwrap, airspeed filter, press brake.
  - `TrenchMap` — lap length, wall bands, tower hardpoints, port disc.
  - `TowerArray` — armed subset, reload, telegraph, eligibility window.
  - `BoltSystem` — lock `u`, flight timer, impact resolution, near-miss scoring.
  - `PortJudge` — arm condition, alignment classifier, dive/overshoot result.
  - `SectorDirector` — 5 sectors, laps, shields, score, adaptive level.
  - `StationRig` — trench mesh, tower meshes, bolts, port iris, fighter chevron.
  - `Starfield` — W-anchored 1,200-point shell.
  - `RunHud` — speed, shields, lap, port range.
- **Event flow**: `HeadsetTracker → PropTennis → ReadHeadModel → TrenchMap`; `TowerArray → BoltSystem → PortJudge`; `SectorDirector` owns the tick order and emits `EventBus {Contact, Spin, TowerCharge, BoltFire, BoltHit, NearMiss, Scrape, OffTrench, PortArmed, PortAligned, Dive, Overshoot, SectorClear}` to `StationRig / AudioBank / RunHud / ScoreKeeper`.
- **Pseudocode — contact track (per tick)**:

```text
if not readHead.contact:
    if now - lastContact > 0.120: airspeed = moveToward(airspeed, 0, 3.0 * dt)
    return
p = normalize(R_P.T * (indexPos - C))
phi = asin(clamp(p.z, -1, 1)); lam = atan2(p.y, p.x)
v = R_TENNIS * phi
uRaw = R_TENNIS * (PI/2 - lam)
du = wrap(uRaw - uPrev, -L_LAP/2, L_LAP/2)
if abs(du) > 0.35 * L_LAP or contactAge < 0.080:
    u = uRaw; uPrev = uRaw; airspeed = 0; return
u += du; uPrev = uRaw
rawA = clamp(du / dt, -180, 520)
airspeed += (1 - exp(-dt / 0.025)) * (rawA - airspeed)
if D >= 4.0: airspeed *= 0.35
```

- **Pseudocode — towers and bolts (per tick)**:

```text
for t in towers where t.armed:
    gap = mod(t.u - u, L_LAP)
    if t.reload <= 0 and 10 <= gap <= 38 and boltsInFlight < 2 and not t.telegraphing:
        t.telegraphing = true; t.charge = 0.0; emit(TowerCharge, t)
    if t.telegraphing:
        t.charge += dt
        if t.charge >= telegraphTime:
            bolts.append(Bolt(uLock=u, impact=now + 0.55)); t.reload = reloadTime
            t.telegraphing = false; emit(BoltFire, t)
for b in bolts:
    if now >= b.impact:
        if abs(u - b.uLock) < 5.0 and abs(v) < 3.2: damage(1)
        elif abs(v) < 4.2: score(80); emit(NearMiss, b)
        b.resolved = true
```

- **Tick order**: prop pose → contact track → tower timers → bolt impacts → wall/scrape → port classifier → sector state. This order guarantees a bolt impact is evaluated against the same `(u,v)` the player saw on the previous rendered frame plus one interpolation step.
- **Telemetry**: per sector log `laps`, `meanSpeed`, `boltsFired`, `hits`, `nearMisses`, `scrapeTime`, `portCrossings`, and `portSpeed`. Used by the adaptive rule and for regression tests.

## 16. Acceptance Test Checklist

1. Registration fits the ball centre to < 0.7 mm and sets the trench plane within 2° of vertical in 20/20 presentations.
2. At `r = 33.5 mm`, a 90° ball rotation about `+ẑ_P` advances `u` by 52.62 ± 0.5 mm and keeps `v` within 0.2 mm.
3. A 360° rotation advances exactly one lap (210.49 ± 1.0 mm); two laps arm the port at 420.97 mm.
4. A stationary index with the ball spinning at 120 °/s reads 70.2 ± 1.0 mm/s airspeed; the HUD shows 53.4 ± 1.0 km/s using the 760 m/mm mapping.
5. Lifting the index decays airspeed from 520 mm/s to < 5 mm/s in 0.17 ± 0.02 s and freezes `u`.
6. Re-seating the index at a point 40 mm away does not jump `u`; a 0.35L jump is classified as a re-seat and awards no distance.
7. A tower within 10–38 mm ahead telegraphs for exactly 0.55 s and fires one bolt; a tower outside that window never fires.
8. A bolt with `u_lock = u` hits when `|v| = 3.1 mm` and misses at `|v| = 3.3 mm`; a 5.1 mm longitudinal offset always misses.
9. A hard press (`D = 4 mm`) reduces airspeed to 35 % within 100 ms and restores it within 100 ms of release.
10. Wall scrape at `|v| = 6.0 mm` drains exactly 0.8 shield/s; `|v| = 7.1 mm` for 150 ms costs exactly 1 shield and recenters `v` within 0.4 s.
11. The port dive triggers at 40–170 mm/s and `|v| = 1.9 mm`; at 171 mm/s it registers an overshoot and the port remains armed.
12. A sector with 6 active towers is clearable in ≤ 12 s with ≥ 1 shield remaining in 5/5 scripted runs at 130 mm/s mean speed.
13. Covering the ball for 1.0 s freezes bolts and preserves `u, v, u_progress`; relock blends within 100 ms and cancels in-flight bolts without damage.
14. Starfield + 2 bolts + 40 shards + 120 sparks stays ≤ 1,400 particles and ≤ 11.1 ms frame time at 90 Hz on the target SoC.
