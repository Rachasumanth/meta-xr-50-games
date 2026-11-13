# Game 06 — Tardigrade Pond — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Tardigrade Pond |
| Slug | `tardigrade-pond` |
| Physical prop | Single contact-lens case bowl, hemispherical shell, 28 mm rim diameter, 1.0 mm wall, ≈3.0 g, concave up, at rest on a table |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.3a Hemispheres, pinch scale |
| Scale class | Tabletop, 28 mm prop, four-point fingernail rim tapping |
| Source concept | A water droplet in a lens case becomes a microscopic pond; rim taps launch capillary waves that herd tardigrades into algae while virus mines drift among them |
| Source physical input | Tap four fixed points around the rim with a fingernail; each tap is a wave source at that azimuth |
| Session shape | 5 droplets, 34–42 s each, ≈3.8 min plus debriefs |
| Primary skill | Spatial wave steering: choose source azimuth, amplitude, and timing to push entities along a gradient |

## 1. Fantasy & Core Loop

**Fantasy.** The contact-lens case is a lens into a drop of pond water. Inside the 26 mm cavity a whole ecosystem lives at 4.4:1 magnification: eight-legged tardigrades graze, spiked virus mines tumble in the current, and three algae clusters glow like soft green islands. The player is the pond's weather. A fingernail tap on the rim is a seismic event that sends a capillary wave across the water, and the tardigrades ride the wave gradient.

**Core loop** (droplet 34–42 s; session 5 droplets):

1. **Condense** (2.5 s) — the droplet blooms into the cavity; algae clusters light up; tardigrades and mines fade in at the rim.
2. **Read** (continuous) — watch entity drift and choose a rim pad whose wave pushes a tardigrade toward an algae cluster.
3. **Tap** (cooldown 0.16 s per pad) — a nail strike on one of four rim pads injects a Gaussian wave source at that azimuth. Strike speed sets source amplitude 0.55–1.20 mm.
4. **Herd** — the wave gradient `−∇η` advects tardigrades away from the source. Two taps from opposite pads create a converging push; adjacent pads create a diagonal push.
5. **Avoid** — virus mines are also advected, at 35 % strength. A mine touching a tardigrade deals 1 HP and knocks it back; three hits kill it.
6. **Save** — a tardigrade entering a 4.2 mm algae radius attaches, stops steering, and scores. The droplet ends when no live tardigrade remains or the 34–42 s timer expires.
7. **Evaporate** — the waterline drops 3 mm over the droplet; unsaved tardigrades are lost, then the next droplet condenses.

The tension is that a wave strong enough to move a tardigrade 10 mm also moves every mine, and the four discrete source points cannot produce an arbitrary push direction. The player must plan two-tap sequences, use the rim's reflecting boundary, and accept that the safest route to an algae cluster may require one diagonal wave and one delay.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Hemisphere sub-shape**: the prop is a hemispherical shell of radius `R` and wall thickness `t`; model in P with the rim circle in the `x̂_P`–`ŷ_P` plane at `z = 0` and the apex at `z = ±R`. For **bowl-up** props the cavity faces `+ẑ_P` (inner surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`); for **dome-up** props the convex dome faces `+ẑ_P`. Rim torus: major radius `R`, minor radius `t/2`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **Exact shell**: outer radius `R_o = 14.0 mm`, inner radius `R_i = 13.0 mm`, wall `t = 1.0 mm`, mass `m = 3.0 g`, effective density 2.62 g/cm³. P origin = rim-circle centre; `+ẑ_P` = cavity opening direction (up when resting); outer apex at `(0, 0, −14.0) mm`; inner apex at `(0, 0, −13.0) mm`; rim circle radius `R_m = 13.5 mm` at `z = 0`; rim torus major radius 13.5 mm, minor radius 0.5 mm.
- **Inner surface** (cavity side): `p_P = R_i·(sinθ·cosλ, sinθ·sinλ, −cosθ)`, `θ ∈ [0°, 90°]`, `λ ∈ [0°, 360°)`; outward-from-cavity normal `n̂_i = −(sinθ·cosλ, sinθ·sinλ, −cosθ)`. **Outer surface**: `p_P = R_o·(sinθ·cosλ, sinθ·sinλ, −cosθ)` with normal `n̂_o = (sinθ·cosλ, sinθ·sinλ, −cosθ)`. The baseline `+cosθ` form is mirrored in z so the cavity opens `+ẑ_P`, which is the bowl-up embodiment.
- **V frame**: anchored to **P**; origin `O_V = P + (0, 0, −2.0) mm` (meniscus centre 2 mm below the rim); `x̂_V = x̂_P`, `ŷ_V = ŷ_P`, `ẑ_V = ẑ_P`. The gameplay plane is `z_V = 0`; the inner apex is at `z_V = −11.0 mm`, the rim at `z_V = +2.0 mm`. Entity positions are 2-D `(x_V, y_V)`; render depth is added separately.
- **Deviations**: (a) the capillary wave is deliberately slowed to `c_w = 90 mm/s` (an effective 58 mm wavelength mode) so a rim-to-rim transit is 144 ms and visually trackable; the physical capillary speed at 6 mm wavelength would be 275 mm/s; (b) the nail is represented by the `tip` joint, not a separate nail mesh, because the skeleton has no nail landmark; (c) the water surface is a virtual mesh 0.3 mm above the real cavity to avoid z-fighting with the plastic; (d) mines have 35 % of the creatures' wave mobility so they remain a threat rather than being trivially evacuated.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated; the bowl sits on a table 260–340 mm in front of the sternum, centred within ±30 mm of the body midline; forearms rest on the table edge.
- **Play field**: the bowl's 26 mm inner diameter. Usable water radius 12.5 mm (rim torus inner edge minus 0.5 mm meniscus clearance).
- **Prop placement**: bowl upright on its convex outer apex, which is a single-point rest. The player may steady it with the non-dominant hand between droplets; during play the bowl must remain still.
- **Hand roles**: dominant index or middle fingertip is the only tapper; the non-dominant hand is IDLE and may pause. Both hands may not tap simultaneously.
- **Reach envelope**: fingertip travel is a 27 mm rim circle plus a 25 mm approach arc; wrist stays within 90 mm of neutral. The tap is a 2–4 mm nail strike, not a press.
- **Forces**: a 3 g bowl is displaced by any real press, so the game classifies a *tap* (contact < 140 ms, approach speed > 60 mm/s) and rejects sustained presses. Accepted tap impulse is 0.04–0.12 mN·s; the bowl's static friction on the table (µ = 0.45, `F_f = 13.2 mN`) exceeds the tangential component, so the bowl stays put.
- **Session**: 5 droplets, hard stop 7 min; 25 s rest between droplets.
- **Visual ergonomics**: the droplet surface is at table height, viewed at 35–55°; the HUD ring floats 12° above the table plane. Entities are 2.2–2.6 mm long, subtending ≈0.42° at 320 mm, above the 0.25° acuity limit at 90 Hz.

## 4. Prop Geometry & Registration

- **Real dimensions**: rim outer diameter 28.0 mm, height 14.0 mm, wall 1.0 mm, mass 3.0 g; tolerances ±0.25 mm radius, ±0.10 mm wall, ±0.25 g. Shell volume `(2/3)π(R_o³ − R_i³) = 1145.7 mm³`; centre of mass on the axis at `z = −7.0 mm` (hemispherical-shell centroid at `R/2` from the rim).
- **Inertia** (thin shell): `I_axis = (2/3)mR_o² = 3.92 × 10⁻⁷ kg·m²`; `I_transverse ≈ (5/12)mR_o² = 2.45 × 10⁻⁷ kg·m²`. Used only for table-bump and tip-over checks; the bowl is otherwise static.
- **Surface atlas**: 24 azimuthal sectors of 15° (`S00`–`S23`), each with three latitude bands: floor (`θ < 35°`), wall (`35° ≤ θ < 70°`), lip (`θ ≥ 70°`). `S00` is centred on `+x̂_P`, increasing toward `+ŷ_P`. The four tap pads are `S00`, `S06`, `S12`, `S18` (0°, 90°, 180°, 270°).
- **Colliders**: `COL_inner` = sphere `R_i = 13.0 mm` (cavity); `COL_outer` = sphere `R_o = 14.0 mm`; `COL_rim` = torus major 13.5 mm, minor 0.5 mm (tap classification only); `COL_water` = disc radius 12.5 mm at `z_V = 0` (no hand collision, entity plane); `COL_table` = plane `y_T = 0`.
- **Registration**: (1) player presents the bowl upright and still for 0.9 s; (2) fit the rim circle and cavity axis over 12 frames, accept centre error < 0.4 mm and axis error < 2.5°; (3) resolve the 180° axis ambiguity from which side shows the inner surface (cavity up), set `+ẑ_P` up; (4) set `+x̂_P` to the player's forward projection on the rim plane, fixing pad zero. P publishes at 72 Hz and is extrapolated to 90 Hz with one-frame linear prediction.
- **Re-acquisition**: search a 45 mm sphere around the last known pose; refit the rim ellipse; if the bowl is tipped (axis > 60° from table normal) hold the last frame and show "set the bowl upright"; no gameplay runs while tipped.
- **Drift policy**: the bowl is symmetric, so yaw is unobservable. `+x̂_P` is re-anchored to the player-forward projection whenever the bowl is at rest for 500 ms; pad positions are re-drawn rather than jumping, using a 200 ms cross-fade.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Pad tap | `R_index_tip` or `R_middle_tip` | `COL_rim` within ±22° of a pad centre | wave source at nearest pad, amplitude from impact speed |
| I5.2 | Off-pad tap | same | `COL_rim`, > 22° from every pad | 35 % splash source at exact azimuth |
| I5.3 | Outer-dome tap | same | `COL_outer`, `θ > 60°` | rejected; "tap the rim" hint, no wave |
| I5.4 | Sustained press | same | `COL_rim`, age > 180 ms | rejected, no wave; prevents table shove |
| I5.5 | Two-finger tap | index + middle within 40 ms | `COL_rim` | merges to one tap, amplitude ×1.15 |
| I5.6 | Steady bowl | both hands on outer wall, 700 ms | `COL_outer` | re-centres V, no score effect |
| I5.7 | Lift bowl | `COL_table` contact lost > 150 ms | — | pause droplet, freeze entities |
| I5.8 | Pause | `L_palm` faces up 700 ms | — | suspend timers |

Per-finger roles: dominant index is primary tapper; dominant middle is alternate tapper for diagonal pads; thumb, ring, and pinky are ignored. Per-hand roles: dominant = TAPPER, non-dominant = STABILISER/PAUSE. A tap is accepted only when the other hand is not in `COL_rim` or `COL_inner`.

## 6. Input Kinematics & Signal Processing

- **Tap classification**: contact on `COL_rim`; contact age at release `≤ 140 ms`; peak approach speed `v_n ≥ 60 mm/s` measured along the rim normal; tangential speed at contact `≤ 90 mm/s` (a scrape is not a tap). Debounce 30 ms; a second tap on the same pad within 160 ms is merged.
- **Tap amplitude**: `A_tap = 0.55 + 0.65·clamp((v_n − 60)/240, 0, 1)` mm, so 60 mm/s → 0.55 mm, 300 mm/s → 1.20 mm. Pad gain 1.0; off-pad splash 0.35.
- **Wave field**: 2-D damped wave equation on the meniscus, `η_tt = c_w²∇²η − 2γ_w η_t + S`, with `c_w = 90 mm/s`, `γ_w = 1.4 s⁻¹`, `S` the tap source. Polar grid: `N_r = 8` rings from 0 to 12.5 mm (`dr = 1.625 mm`), `N_λ = 48` (`Δλ = 7.5°`, rim arc 1.70 mm). CFL: `c_w·dt/dr = 90 × 0.01111 / 1.625 = 0.615 < 1`, so one substep is stable at 90 Hz.
- **Laplacian**: `∇²η = η_rr + (1/r)η_r + (1/r²)η_λλ`; at `r = 0` use the regular limit `2η_rr(0) + (1/r²)η_λλ`. Rim boundary is absorbing (first-order Mur): `η_rim^{n+1} = η_{rim−1}^{n} + ((c_w dt − dr)/(c_w dt + dr))(η_{rim−1}^{n+1} − η_rim^{n})`.
- **Source injection**: Gaussian in grid space, `S = A_tap·exp(−(Δr)²/(2·2.0²))·exp(−(Δλ)²/(2·10²))`, applied as a displacement at the pad azimuth over 3 radial cells and 3 angular cells; no initial velocity, so the first wavefront leaves the rim at `t = 0` and reaches the centre in 139 ms.
- **Gradient**: central differences in polar coordinates, converted to `(x_V, y_V)`; magnitude clamped to 0.18 (mm/mm). Advection `v_entity = −K_w∇η`, `K_w = 200 mm/s`, speed clamped to 26 mm/s. Entity drag `τ_v = 0.35 s`, so a 0.4 s wave pulse produces a 6–9 mm displacement.
- **Creature integration**: semi-implicit Euler at 90 Hz, `v ← (v + a_wave·dt)·e^(−dt/τ_v)`, `p ← p + v·dt`; a 3 mm/s random swim vector is added with a 1.2 s direction hold. Mines use `K_m = 70 mm/s` (35 %).
- **Signal chain**: 26 joints → 3-tap median → one-euro filter (min-cutoff 1.0 Hz, beta 0.015) → contact query → tap classifier → wave source. Entity field sampled bilinearly at 90 Hz; render interpolates at frame rate.
- **Latency budget**: hand tracking 14 ms + tap classification 1 frame 11 ms + wave propagation 0–144 ms + render 11 ms + audio 20 ms = 56–200 ms; the readable wavefront is the delayed cue, so the tap-to-source flash (11 ms) confirms input before the wave arrives.

## 7. Virtual World Layout & Coordinate Mapping

- **V axes**: origin at the meniscus centre 2 mm below the rim; `+x̂_V` = player forward projected on the meniscus; `+ŷ_V` = player right; `+ẑ_V` = cavity up. All gameplay coordinates below are V.
- **Water disc**: radius `R_W = 12.5 mm`; waterline `z_V = 0`; depth at centre 11.0 mm (inner apex `z_V = −11.0`); meniscus is rendered as a shallow paraboloid with 0.25 mm edge rise.
- **Algae clusters**: 3 per droplet, centres at radius 5.5–10.5 mm, pairwise angular separation ≥ 50°; capture radius 4.2 mm; each cluster has 5 lobes and a 0.6 mm glow shell. A cluster cannot overlap a rim pad exclusion wedge of ±18°.
- **Rim pads**: four 3.5 mm × 2.0 mm luminous pads centred at V azimuths 0°, 90°, 180°, 270° on the rim torus; active pad flashes for 120 ms after a tap; pad arc half-width 22°.
- **Entity scale**: real tardigrade 0.5 mm → rendered 2.2 mm body length (4.4:1); virus mine 1.8 mm → rendered 2.4 mm radius including 12 spikes; algae cluster 4.2 mm radius. Heights: creatures `z_V = 0.25 mm`, mines `z_V = 0.35 mm`, algae `z_V = 0.10 mm`.
- **Mapping formulas**: `p_V = (r cosλ, r sinλ, z)`; world position `p_W = M_V→W·p_V`, where `M_V→W` is the P pose composed with the V offset. Screen-space size is fixed in millimetres; leaning in does not scale entities, preserving the "look into a lens" illusion through parallax only.
- **Camera note**: the droplet is world-locked to the bowl. If the player's head moves 100 mm, the parallax across the 11 mm cavity is 9.1°, which reads as depth; no head-locked magnification is used because it would break the prop illusion.

## 8. Gameplay Systems & Rules

- **Entities**: `BowlProp` (pose P, pads), `WaveField` (η, η̇ grid), `Tardigrade` (position, velocity, HP, state), `VirusMine` (position, velocity, spike phase), `AlgaeCluster` (centre, saved count), `DropletDirector` (composition, timer, score).
- **Tardigrades**: 3 HP; body capsule 2.2 × 1.2 mm; max speed 26 mm/s; contact with a mine deals 1 HP and applies a 14 mm/s knockback along the mine normal, with 0.6 s immunity. At 0 HP the creature curls and sinks (death VFX, −80 score). At an algae cluster within 4.2 mm it attaches (saved, +120).
- **Virus mines**: 12 spikes; drift 2–6 mm/s plus 35 % wave advection; bounce elastically off the rim (`e = 0.55`); cannot enter an algae cluster (repelled at 4.8 mm); a mine–mine contact swaps 20 % of velocity (visual churn only, no damage).
- **Waves and herding**: a tap source is active for 0.6 s; the field persists until damped (τ = 0.71 s). Wave crests are rendered as 5 concentric contour rings with 0.15 mm relief. Waves do not damage entities; only mines do.
- **Droplet timer**: 42/40/38/36/34 s by level. At `t = 0` the waterline drops 3 mm over 2.5 s; every unsaved creature is lost (−40 each) and the droplet ends. The timer pauses while the bowl is lifted or the system is suspended.
- **Clear condition**: a droplet ends when every live creature is saved or dead, or the timer expires. It is **cleared** if ≥ 70 % of the spawned creatures are saved. A perfect droplet (100 % saved, zero deaths) awards +350.
- **Scoring**: saved +120; death −80; droplet clear +200; perfect +350; time bonus +2 per whole second remaining when all creatures are saved early. Session score is the sum; a session win requires ≥ 20 of 36 creatures saved.
- **Idle hint**: if no tap for 4 s, a faint arrow traces from the nearest pad to the nearest unsaved creature; it disappears after the next accepted tap.
- **Difficulty coupling**: tap amplitude is capped at 1.20 mm so a single tap cannot cross the full 25 mm dish; the player must chain two sources. The reflecting rim turns a mistimed wave into a secondary push after 280 ms, which is a recoverable error at levels 1–2 and a mine hazard at levels 4–5.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, sensor self-test | REGISTER |
| REGISTER | bowl upright, still 0.9 s | centre error < 0.4 mm, axis < 2.5° | set P, pads, V origin | CALIBRATE |
| CALIBRATE | table plane locked | plane RMS < 1.2 mm | build T, set waterline | BRIEF |
| BRIEF | 2.5 s card | — | load droplet composition | CONDENSE |
| CONDENSE | droplet bloom 2.5 s | — | spawn entities at rim, start timer | PLAY |
| PLAY | accepted tap | cooldown per pad ≥ 0.16 s | inject source, flash pad | PLAY |
| PLAY | creature at algae | distance < 4.2 mm | attach, +120, stop steering | PLAY |
| PLAY | mine–creature contact | creature immunity ≤ 0 | −1 HP, knockback, 0.6 s immunity | PLAY |
| PLAY | creature HP = 0 | — | death VFX, −80 | PLAY |
| PLAY | all creatures resolved | — | tally, clear check | DEBRIEF |
| PLAY | timer = 0 | — | begin evaporation | EVAPORATE |
| EVAPORATE | 2.5 s drop | — | lose unsaved, −40 each | DEBRIEF |
| DEBRIEF | droplet < 5 | — | 25 s rest, next composition | BRIEF |
| DEBRIEF | droplet = 5 | — | session stats | COMPLETE |
| any | bowl lifted > 150 ms | — | freeze field and timer, dim entities | REACQUIRE |
| REACQUIRE | relock ≤ 2.0 s | search 45 mm | restore, 3 s countdown | prior |
| REACQUIRE | timeout 2.0 s | — | "set the bowl upright" card | REACQUIRE |
| any | palm up 700 ms | — | freeze timers and audio | SUSPENDED |
| SUSPENDED | palm down 500 ms | — | 1.5 s countdown | prior |

Timers: tap debounce 30 ms; pad cooldown 160 ms; source life 0.6 s; wave damping 0.71 s; creature immunity 0.6 s; evaporation 2.5 s; brief 2.5 s; debrief 25 s; reacquire 2.0 s; suspension countdown 1.5 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_inner` sphere (13.0 mm), `COL_outer` sphere (14.0 mm), `COL_rim` torus, `COL_table` plane, 8 creature capsules (2.2 × 1.2 mm), 4 mine spheres (2.4 mm), 3 algae spheres (4.2 mm), 1 water disc (12.5 mm).
- **Pair matrix**: Finger↔`COL_rim` (tap classification); Finger↔`COL_outer` (reject); Finger↔water (ignore, no hand splash); Creature↔Mine (damage + knockback); Creature↔Algae (capture); Creature↔Rim (slide along boundary, no escape); Mine↔Rim (elastic bounce, `e = 0.55`); Mine↔Mine (velocity exchange 20 %); Mine↔Algae (repel at 4.8 mm); Creature↔Creature (soft separation, 2 mm).
- **Field solve**: explicit second-order wave equation at 90 Hz, one substep, CFL 0.615. Absorbing rim; damping `2γ_w = 2.8 s⁻¹`. Stability is checked at load: if `c_w·dt/dr > 0.9`, `dt` is halved internally and the grid is sub-stepped twice.
- **Entity motion**: semi-implicit Euler; advection sampled from the filtered η at the entity centre with bilinear interpolation; speed clamp 26 mm/s (creature), 9 mm/s (mine). Rim constraint: if `r > R_W − r_entity`, project back and reflect the radial velocity with restitution 0.35.
- **CCD**: maximum entity speed 26 mm/s → 0.29 mm per tick, far below the 1.2 mm creature radius; no swept test is needed for creature–mine. Mine–rim uses a swept circle because a wave can push a mine at 9 mm/s into the torus within one 0.35 mm step; the test is analytic segment–circle.
- **Water surface**: kinematic; η is a height field, not a fluid simulation. Gravity–capillary dispersion is replaced by a constant `c_w` with the stated deviation. No water leaves the bowl because η is clamped to ±1.5 mm and the rim is absorbing.
- **Forces**: wave advection is a velocity target, not a force; the equivalent acceleration for a 26 mm/s terminal speed is 74 mm/s². Mine knockback 14 mm/s applied as an impulse to the creature; table friction on the bowl `F_f = 13.2 mN` prevents tap translation.
- **Table bumps**: if the table plane fit moves > 2 mm in one frame, all entities keep V-relative positions but the bowl pose is re-anchored; a 3 mm bump triggers a 0.4 s settle and no score change.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → table shadow → bowl outer shell (real) → meniscus water mesh → algae → tardigrades → mines → wave contours → rim pads → hands → HUD.
- **Occlusion cases**:
  1. *Tapping fingertip over the rim*: the fingertip covers 5–8 mm of the 28 mm rim; the covered pad arc is drawn at 45 % opacity and a 1.5 mm source ring is lifted 0.5 mm above the nail so the flash remains visible.
  2. *Near wall versus water*: at a 35° view the near inner wall hides 18–24 % of the far water; the water mesh is depth-tested against the real bowl mesh and clipped at the inner surface, so no water appears to float above the rim.
  3. *Hand over the droplet*: a palm crossing the cavity casts a soft 35 % shadow and dims entities by 20 %; entities remain visible through the hand's silhouette edge to preserve playability.
  4. *Tardigrade under the meniscus*: creatures render at 0.88 opacity with a 0.35 mm refraction offset; their legs are drawn 0.15 mm above the body to avoid z-fighting at grazing angles.
  5. *Mine spikes through the surface*: spike tips that exceed `z_V = 1.2 mm` are clipped and replaced by a 0.6 mm ripple ring, so the mine never appears to pierce the water plane.
  6. *Bowl moved while paused*: the entire virtual layer is re-parented to P with a 200 ms cross-fade; entities are not left floating over the table.
- **VFX budgets**: meniscus mesh 1,024 triangles; wave contours 5 × 48 segments; tardigrades 8 × 180 triangles (body 120 + 8 legs × 8); mines 4 × 96 triangles plus 12 spikes × 6; algae 3 × 80 particles; tap splash 18 particles; capture burst 24; death curl 16; rim glow 4 × 12 quads. Global cap 500 particles, 6,000 triangles.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Pad tap accepted | pad azimuth | glassy rim tick, pitch rises with amplitude | pad flashes cyan, wave ring leaves source |
| Off-pad tap | tap azimuth | dull plastic knock, −8 dB | small ripple, no pad flash |
| Rejected press | rim | muted thud | rim outline pulses red once |
| Wave passes creature | creature | soft water swish, stereo-panned | body banks with the gradient |
| Creature saved | algae | bubbly chime + rising third | algae lobe brightens, +120 float |
| Creature damaged | creature | wet crackle + low thump | red flash, 3-segment HP ring |
| Creature death | creature | descending gurgle | curl animation, dark sink |
| Mine proximity | mine | 1.8 Hz pulsed buzz, volume ∝ 1/r | spikes pulse amber |
| Droplet start | bowl | water-drop bloom + low pad | waterline rises, entities fade in |
| Evaporation | bowl | airy hiss, pitch falling | waterline drops 3 mm |
| Droplet clear | room | green bell + water chord | tally card |
| Droplet failed | room | dry evaporation rasp | entities fade to grey |

## 13. Difficulty Progression & Tuning

| Level | Creatures | Mines | Timer (s) | Wave gain `K_w` (mm/s) | Mine drift (mm/s) | Algae radius (mm) | Pad cooldown (s) |
|---|---|---|---|---|---|---|---|
| 1 | 6 | 2 | 42 | 170 | 2.0 | 4.6 | 0.18 |
| 2 | 7 | 3 | 40 | 185 | 2.8 | 4.4 | 0.17 |
| 3 | 7 | 3 | 38 | 200 | 3.6 | 4.2 | 0.16 |
| 4 | 8 | 4 | 36 | 215 | 4.5 | 4.0 | 0.15 |
| 5 | 8 | 4 | 34 | 230 | 5.5 | 3.8 | 0.14 |

- **Adaptive rule** (`POND_ADAPT`): after each droplet, if clear and no deaths, level +1; if failed (saved < 70 %), level −1, timer +3 s, mine drift ×0.85. Clamp 1–5; one step per droplet. No adaptation inside a droplet.
- **Named constants**: `R_o = 14.0 mm`, `R_i = 13.0 mm`, `R_W = 12.5 mm`, `H_W = 11.0 mm`, `C_W = 90 mm/s`, `GAMMA_W = 1.4 s⁻¹`, `N_R = 8`, `N_LAMBDA = 48`, `DT = 0.01111 s`, `A_TAP_MIN = 0.55 mm`, `A_TAP_MAX = 1.20 mm`, `K_W = 200 mm/s`, `V_CREATURE_MAX = 26 mm/s`, `V_MINE_MAX = 9 mm/s`, `HP_CREATURE = 3`, `R_ALGAE = 4.2 mm`, `R_SAVE = 4.2 mm`, `DROPLET_TIMER = 42 s`, `PERFECT_BONUS = 350`, `SAVE_SCORE = 120`, `DEATH_SCORE = −80`.

## 14. Edge Cases & Failure Recovery

- **Bowl tipped over**: if the cavity axis leaves 60° of table normal, freeze all entities and the timer, show "set the bowl upright"; on upright relock, restore at the same droplet time. No spill penalty because the water is virtual.
- **Bowl pushed across the table**: if P translation exceeds 8 mm during a droplet, re-anchor V to the new P and carry all entity positions in V; the wave grid is translated, not reset. A 20 mm shove triggers a 0.5 s "settle" pause.
- **Prop tracking loss > 150 ms**: freeze η, entities, and timers; render the bowl as a 40 % ghost and show a soft edge outline. Relock within 2.0 s resumes with no state loss; timeout shows the re-present card.
- **Hand tracking loss**: if the tapper hand is lost, taps simply stop and the wave decays; if the stabiliser hand is lost, nothing changes. Re-acquisition is automatic; no pause is forced.
- **Very fast tapping (> 6 Hz)**: pads are debounced at 160 ms; taps arriving inside the debounce are merged into one with amplitude `max(A_i)`, capped at 1.20 mm, preventing grid energy blow-up.
- **Two pads tapped in the same frame**: both sources are injected at 50 % amplitude and the pads flash together; this is a legitimate "squeeze" technique but costs two cooldowns.
- **Tap on the outer dome**: rejected by `COL_outer`; if the bowl moves > 1 mm from the tap, a 0.3 s wobble animation plays but no wave is generated.
- **Creature trapped at the rim**: if a creature is pinned against the rim by a standing wave for > 3 s, a 4 mm/s centripetal bias nudges it inward; this prevents an unwinnable droplet.
- **Algae cluster occluded by a mine**: the algae glow renders through the mine at 60 % additive; capture still works because the trigger is distance, not visibility.
- **Non-dominant play**: handedness mirrors I5.1–I5.6; pads and algae layout are unchanged, so no re-learning of the field.
- **Recovery UX**: one icon (bowl / hand / table) plus one line of text; never a modal dialog during a droplet.

## 15. Implementation Architecture

- **Classes/structs**:
  - `BowlProp` — pose P, rim fit, pad atlas, tip-over detector, V transform.
  - `TapClassifier` — contact age, approach speed, pad assignment, amplitude, debounce.
  - `WaveField` — polar η/η̇ grids, Laplacian, Mur boundary, source injection, gradient sampling.
  - `PondEntity` — creature/mine common motion, drag, rim constraint, HP/immunity.
  - `AlgaeField` — cluster centres, capture tests, glow state.
  - `DropletDirector` — composition, timer, evaporation, scoring, adaptive level.
  - `PondRig` — water mesh, wave contours, creature/mine/algae instancing, occlusion masks.
  - `PondAudio` — event-to-cue bank with spatial panning by V azimuth.
- **Event flow**: `HandTracker → TapClassifier → WaveField`; `WaveField → PondEntity`; `PondEntity → EventBus {Saved, Damaged, Died, DropletClear, DropletFailed}`; `EventBus → PondAudio / VFXPool / ScoreKeeper / HUD`. `PondRig` reads `WaveField` and `EventBus` only; it never mutates gameplay state.
- **Pseudocode — tap + wave step (per tick)**:

```text
dt = 1/90
for c in rim_contacts:
    if c.age <= 140ms and c.v_normal >= 60mm/s and c.tangential <= 90mm/s:
        pad = nearest_pad(c.azimuth)
        if pad and now - pad.last_tap >= PAD_COOLDOWN:
            A = 0.55 + 0.65 * clamp((c.v_normal - 60)/240, 0, 1)
            inject_source(eta, eta_dot, pad.azimuth, A, sigma_r=2.0mm, sigma_l=10deg)
            pad.last_tap = now
            emit(TapAccepted, pad, A)
for each cell (r, lambda):
    lap = eta_rr + eta_r/r + eta_ll/(r*r)
    eta_dot += (C_W*C_W*lap - 2*GAMMA_W*eta_dot) * dt
    eta += eta_dot * dt
apply_absorbing_rim(eta, eta_dot)
```

- **Pseudocode — entity update + scoring (per tick)**:

```text
for e in entities:
    g = gradient(eta, e.pos)                 # (d eta/dx, d eta/dy)
    v_target = -K_w * clamp_norm(g, 0.18)
    e.v += (v_target - e.v) * (dt / TAU_V)
    e.v += random_swim(e) * dt
    e.p += e.v * dt
    clamp_to_disc(e, R_W - e.radius, restitution=0.35)
    if e.kind == CREATURE:
        for m in mines:
            if dist(e.p, m.p) < e.radius + m.radius and e.immunity <= 0:
                e.hp -= 1; e.immunity = 0.6s
                e.v += 14mm/s * normalize(e.p - m.p)
                emit(CreatureDamaged, e)
        if e.hp <= 0: emit(CreatureDied, e); score -= 80
        for a in algae:
            if dist(e.p, a.p) < R_SAVE:
                e.state = ATTACHED; score += 120; emit(CreatureSaved, a)
if all_creatures_resolved():
    score += 200 + (350 if perfect else 0) + 2*floor(time_left)
    emit(DropletClear)
```

## 16. Acceptance Test Checklist

1. Registration fits the rim centre to < 0.4 mm and the cavity axis to < 2.5° in 20/20 presentations; `+x̂_P` is within ±7.5° of player forward after re-registration.
2. A 300 mm/s nail tap on pad S00 produces `A_tap = 1.20 ± 0.05 mm`; a 60 mm/s tap produces 0.55 ± 0.05 mm; an off-pad tap at 45° produces a source at 0.35× amplitude.
3. A single pad tap produces a wavefront that reaches the opposite rim in 139 ± 8 ms, measured from the first non-zero η at the source to the first non-zero η at the far rim.
4. With no taps, peak |η| decays from 1.0 mm to 0.37 mm in 0.71 ± 0.08 s (one damping time).
5. A 1.0 mm source at pad S00 moves a free creature at the centre by 7.0 ± 1.5 mm in the −x_V direction over 0.6 s, and never exceeds 26 mm/s.
6. A mine at the same position moves by 2.4 ± 0.8 mm (35 % ± 8 %) under the same wave.
7. Three mine contacts on one creature reduce HP from 3 to 0, apply 0.6 s immunity between hits, and score exactly −80 on death.
8. A creature entering an algae radius of 4.2 mm attaches, scores +120, and stops responding to subsequent waves.
9. Timer expiry at 42 s evaporates all unsaved creatures at −40 each and never double-counts a creature already saved or dead.
10. Lifting the bowl for 1 s freezes η, entity positions, and the timer; relock within 2.0 s restores all three with < 1 % state change and no score change.
11. Tapping the outer dome at θ = 75° produces zero wave sources and zero bowl translation > 1.0 mm.
12. Eight creatures plus four mines plus three algae solve at 90 Hz with ≤ 2.0 ms CPU on the target device and no CFL instability over a 10 min soak.
