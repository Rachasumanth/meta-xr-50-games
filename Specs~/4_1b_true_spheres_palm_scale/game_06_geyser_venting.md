# Game 6 — Geyser Venting — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Geyser Venting |
| Slug | `geyser-venting` |
| Physical prop | Orange, true sphere, 76.0 mm diameter (r = 38.0 mm); textured peel, 150 g |
| Family / sub-shape | Family 4 Spherical, Ellipsoidal & Domed / 4.1b True Spheres, Palm-Scale |
| Scale class | Palm-scale two-hand hold; prop 1:1; virtual crust and plumes at 1:1 on the prop |
| Source concept | "Unstable volcanic moon; magma fissures bubble up. Rotate the sphere in both hands and press erupting vents to cap them before the internal pressure gauge hits critical." |
| Physical input | Both palms cradle the orange; left/right thumb and index tips press discrete lat/long patches |
| Player posture | Seated, elbows supported, ball 260–320 mm in front of the sternum |
| Session length | 5 phases × 40 s + 5 × 6 s debrief ≈ 230 s |
| Win condition | Seal all 8 vents in each of 5 phases without eruption |
| Lose conditions | Pressure reaches 100.0, or three crust cracks accumulate |
| Primary metric | Pressure headroom = 100 − peak pressure per phase |
| Tracking budget | 4 fingertip effectors, 2 palm cradles, 1 prop pose at 72 Hz |

## 1. Fantasy & Core Loop

The orange is a volcanic moon held in a gravity cradle. Its peel is a 24-plate crust, and the eight vent sockets under the surface are the only pressure relief the moon has. The player is the venting engineer: rotate the moon in both palms until a glowing fissure faces the fingers, then pin it shut with a thumb or index and hold long enough for the plug to sinter. The pressure gauge is a ring of light around the moon; it never stops climbing while a vent is open.

The fantasy is deliberately tactile: the crust is the physical peel, the vents are hot spots that bloom under the fingertips, and the only way to reach the far side is to roll the real fruit between the palms. A wrong press does not fail silently; it cracks a visible plate and adds a sharp pressure spike, so the player learns to read the patch boundary before committing.

1. **Read the crust (0.5–1.5 s).** The phase opens with 3 incipient vents and the pressure ring at 12. Incipient vents show as 3° dark pores.
2. **Rotate (0.4–1.2 s).** Palm friction spins the moon; the player brings a blooming vent into the reachable cap under the fingers.
3. **Press and hold (0.30 s).** A thumb or index tip enters the vent disc with ≥ 5 mm penetration proxy; a radial progress arc fills.
4. **Seal (0.1 s).** The plug sinter-flashes, pressure drops 4.0 points, and the vent becomes inert crust.
5. **Triage (continuous).** Up to 5 vents bloom concurrently; the player chooses between a nearby vent and a far, fast-growing one.
6. **Phase resolve (6 s).** Peak pressure is banked, cracks are repaired one step, and the next 8 vents are seeded.

A phase is a 40 s pressure-management puzzle, not a reflex test: the optimal route caps the highest-flux vent first, but the highest-flux vent is usually the one on the far side of the moon. The 5-phase run escalates bloom rate and initial pressure until the player is rotating almost continuously.

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

- **P-frame**: origin at the orange center; `+ẑ_P` is the **volcanic north pole**, defined at registration as the world-up direction `+ŷ_W` projected into the prop. Poles `±ẑ_P`; equator in `x̂_P`–`ŷ_P`. The 24 crust plates, fissures, and all vent coordinates are `P`-fixed, so the geology rotates with the real fruit.
- **Radius and mass**: `r = 38.0 mm`, `m = 150 g`, `I = 2mr²/5 = 8.664 × 10⁻⁵ kg·m²`. The collider radius is 38.0 mm; registration tolerance ±0.6 mm.
- **Rolling kinematics**: palm and finger contacts drive the prop spin with `ω = (n × v_t)/r`, where `n` is the outward contact normal and `v_t` the tangential contact-point velocity relative to the center. Contributions from both palms are averaged with weights proportional to contact area; `|ω|` is clamped to 300 °/s and integrated into `R_P` at 90 Hz. This is the only way vents on the far side become reachable.
- **V-frame anchoring**: `V = P` for the moon, crust, vents, plugs, and plume roots. Only the pressure ring and phase HUD are re-oriented toward the head each frame; their positions are `C + 96 mm · ĥ` where `ĥ` is the unit vector from the moon center to the cyclopean eye. The moon therefore never swims when the head moves.
- **Deviation 1 — dual palm cradle instead of one hand**: both palms support the lower hemisphere; only the four thumb/index tips are effectors. Palm contacts are used for grip validity and spin torque, never for capping.
- **Deviation 2 — pressure is a scalar ODE, not rigid-body physics**: the crust is kinematic and cannot be deformed by the player. "Penetration" is a tracker proxy, not a physical dent.
- **Deviation 3 — unreachable vents subside rather than teleport**: a vent that remains outside the reachable cap for 8.0 s loses 60 % flux and a replacement seeds elsewhere; geology never jumps discontinuously.
- **Deviation 4 — no haptics**: cap completion is communicated by a 90 ms low-frequency audio thump, a 4 mm plug flash, and a pressure-ring pulse, not by vibration.

## 3. Physical Play Space & Ergonomics

The player sits with both forearms resting on the chair arms or the table edge. The orange is held at sternum height, 260–320 mm forward, with the elbows at 95–110° and the wrists in 10–20° extension. Both palms face each other and slightly upward; the lower hemisphere rests in the thenar pads, leaving the upper 150° cap free for the thumbs and index fingers. The head pitches down 20–28°; the pressure ring is rendered at 0–8° below the eye line.

| Parameter | Value | Rationale |
|---|---|---|
| Support | Table or lap `T` detected by scene mesh | Keeps elbows below shoulder height |
| Ball centre | 280 mm forward, 40 mm above sternum | Inside the comfortable two-hand workspace |
| Palm separation | 82–104 mm | Cradles a 76 mm sphere without clamping the wrists |
| Reachable cap | 78° half-angle about `+ŷ_W` at the ball | Thumbs and index tips can reach this band without regripping |
| Max simultaneous effectors | 4 (2 per hand) | More contacts would occlude the vent field |
| Head-to-ball distance | 320–420 mm | Vent discs subtend 5–9° and stay legible |
| Continuous spin budget | ≤ 300 °/s, ≤ 8 s sustained | Avoids wrist supination beyond 60° |

Ergonomic rule: if either wrist exceeds 55° of supination or 35° of ulnar deviation for 2.0 s, the HUD shows a rotate-the-forearm hint and spin gain drops to 60 %. This protects the wrist without stopping play.

## 4. Prop Geometry & Registration

- **Real dimensions**: 76.0 mm diameter, 150 g, peel thickness ≈ 4 mm, surface micro-texture 1.2 mm deep at 4 mm pitch. The texture is the odometry feature set; no printed markers are used.
- **Sphere model**: `|p_P| = 38.0 mm`. Surface atlas is a 24-cell geodesic partition (spherical Fibonacci, 24 seeds, Voronoi cells) with 5 baked fissure polylines and 8 vent sockets generated per phase. Vent socket centres obey a 34° minimum angular separation.
- **Surface features**: each plate has a 0.8 mm raised rim; fissures are 1.5 mm wide emissive channels; vent discs are flat, matte, and 0.3 mm recessed. None of these are physical; all are rendered at the crust surface with a 0.5 mm depth offset to avoid z-fighting.
- **Colliders**: `COL_sphere` r = 38.0 mm (all hand contacts and ray tests); `COL_finger_L/R` capsules r = 6.0 mm, length 18 mm, one per distal phalanx, swept for fast presses; `COL_palm_L/R` capsules r = 18 mm, length 70 mm, used only for grip and spin torque. Vent discs have no collider; matching is analytic in angle.
- **Registration**: the player presents the orange at 300 mm with the stem scar facing themselves. The system fits the centre over 8 frames (centre error < 0.8 mm), estimates radius scale (error < 0.6 mm), and sets `+ẑ_P` to the world-up projection with a 0.5 s gravity settle. The stem scar defines `λ = 0°`; if the scar is not visible, `λ = 0°` is set to the player's right and a 1.0 s "rotate to show the scar" prompt appears.
- **Tracking**: visual odometry at 72 Hz on the peel texture, with a 6-DoF Kalman filter (process noise 0.02 mm²/s, measurement noise 0.35 mm²). Re-acquisition searches a 70 mm sphere around the last pose for 1.5 s and blends over 120 ms.
- **Drift**: orientation drift is bounded to 0.4 °/s by re-anchoring to the gravity vector whenever the ball is stationary for 400 ms.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Two-palm cradle | `L_palm` + `R_palm` capsules | lower hemisphere | grip valid; enables pressure clock |
| I5.2 | Left-thumb press | `L_thumb_tip` | vent disc | cap hold, weight 1.0 |
| I5.3 | Left-index press | `L_index_tip` | vent disc | cap hold, weight 1.0 |
| I5.4 | Right-thumb press | `R_thumb_tip` | vent disc | cap hold, weight 1.0 |
| I5.5 | Right-index press | `R_index_tip` | vent disc | cap hold, weight 1.0 |
| I5.6 | Pinch-cap | any two effectors within 12 mm on the same vent | vent disc | hold rate ×1.8 |
| I5.7 | Palm roll | both palm contacts, tangential drag | lower hemisphere | spin `ω = (n × v_t)/r` |
| I5.8 | Crust press | any effector, no vent within `a_i + 10°` | bare crust | crack +1 after 300 ms |
| I5.9 | Release cradle | both palm contacts lost > 350 ms | — | drop / pause |
| I5.10 | Re-cradle | both palms re-contact, ball still | — | resume, pressure frozen |

Per-finger roles: both thumbs and both index fingers are equivalent pressors; middle, ring, and pinky are ignored as effectors but contribute to palm grip if their metacarpals contact the ball. Per-hand roles: both hands are symmetric; there is no dominant hand. A press by two fingers on one vent is encouraged for fast-growing vents and is the intended way to rescue a vent at 90 % bloom.

## 6. Input Kinematics & Signal Processing

- **Surface distance**: for joint position `x_j` and prop centre `C`, `s = |x_j − C| − r`. Contact exists when `s < 5.0 mm`; release when `s > 8.0 mm` (8 mm hysteresis); state changes are debounced 30 ms.
- **Penetration proxy**: `D = clamp(5.0 − s, 0, 12.0) mm`. A press gate requires `D ≥ 5.0 mm`, i.e. the tracked tip joint centre is at or inside the shell. This is the literal "≥ 5 mm penetration proxy" of the design brief and absorbs the 5–7 mm offset between the tip joint and the finger pad.
- **Depth filter**: `D_f ← D_f + α(D − D_f)`, `α = 1 − exp(−dt/0.040)`; the 40 ms EMA removes tracker jitter without smoothing the 300 ms hold.
- **Contact angular position**: `û = (x_j − C)/|x_j − C|`; vent match if `acos(û · v̂_i) ≤ a_i + 2°`, where `a_i` is the current vent angular radius and `v̂_i` its unit centre. The 2° tolerance equals 1.3 mm at the surface.
- **Hold accumulator**: `h_i += w · dt` while matched and `D_f ≥ 5.0 mm`, where `w = 1.0` for one effector and `1.8` for two effectors on the same vent. `h_i` decays at 4.0 × real time when unmatched for more than 100 ms; a cap completes at `h_i ≥ 0.30 s`.
- **Crack classifier**: when `h_i` would complete but no vent is within `a_i + 10°`, the hold is re-labelled a crust press: crack +1, pressure +2.0, and all hold accumulators reset.
- **Spin extraction**: for each palm contact, `v_t = v_contact − (v_contact · n)n`, `ω_k = (n × v_t)/r`. Weighted mean `ω = Σ A_k ω_k / Σ A_k`, with `A_k` the contact patch area estimated from palm capsule overlap. Clamp `|ω| ≤ 300 °/s`; integrate `R_P ← expm([ω]× dt) · R_P` and renormalize each tick.
- **Spin deadband**: `|ω| < 6 °/s` is treated as zero to stop jitter-driven crust creep while the player is only holding.
- **Pressure filter**: displayed pressure uses a 120 ms EMA; the simulated pressure is unfiltered so tests can assert exact values.
- **Vent reachability**: a vent is reachable while its centre lies within 78° of `+ŷ_W` as seen from `C`. Time outside this cap is accumulated per vent; at 8.0 s the vent subsides (flux ×0.4) and a replacement seeds at a reachable location.

## 7. Virtual World Layout & Coordinate Mapping

- **V frame**: identical to `P`; origin `C`, axes `x̂_P, ŷ_P, ẑ_P`. All geology uses `p_P = r·(cosφ cosλ, cosφ sinλ, sinφ)` directly; there is no additional world transform for the crust.
- **Crust**: 24 plates rendered as a 0.6 mm shell with 0.8 mm rim displacement, an emissive fissure network, and a dark basalt albedo. Plate IDs are stable for the session.
- **Vent field**: each vent is a disc of angular radius `a_i(t)` on the sphere, rendered with a 2.0 mm raised lip, an inner lava pool, and a 3-ring progress arc at `a_i + 4°`.
- **Plumes**: a capped vent emits a 40–140 mm tapered plume along the surface normal, with a 12 mm base radius and 60 particles/s at peak. Plume length scales with the vent flux at the moment of capping.
- **Pressure ring**: a 108 mm torus centred at `C + 96 mm·ĥ`, always facing the head, with a 270° arc; the gap at the bottom marks 0. Tick marks every 5 points; red zone from 85 to 100.
- **Phase card**: a 70 mm × 40 mm panel at `C + 130 mm·ĥ + 55 mm·ŷ_W`, showing phase number, vents sealed, cracks, and peak pressure.
- **Camera mapping**: the player's gaze is expected at 20–28° down; the ball is not camera-locked. Passthrough scale is 1:1, so the orange looks exactly like the object in hand.

## 8. Gameplay Systems & Rules

- **Entities**: `Vent` (φ, λ, state, a_i, flux, hold), `PressureModel` (P, peak, warning flags), `CrustModel` (3 segments), `PhaseDirector` (8 vents, timers, score), `Effector` (4 fingertips, D_f, contact age).
- **Vent states**: `DORMANT` (3° pore, pressable) → `BLOOM` (radius grows) → `ACTIVE` (full flux) → `CAPPING` (hold in progress) → `SEALED` (inert). A `DORMANT` vent can be pre-capped for a skill bonus; a `SEALED` vent is inert and pressing it is a crust press.
- **Bloom law**: `a_i(t) = a_max − (a_max − a_0)·exp(−(t − t_bloom)/τ)`, with `a_0 = 3.0°`, `a_max = 18.0°`, `τ = 5.5 s` at level 3. Bloom reaches 90 % of `a_max` in 12.7 s.
- **Flux law**: `f_i = F_0·(a_i/18°)²` while `ACTIVE` or `BLOOM`, `F_0 = 2.4 pressure/s`. A vent at 18° contributes 2.4/s; five such vents add 12.0/s.
- **Pressure ODE**: `dP/dt = Σ f_i − L·[N_active = 0]`, `L = 1.2/s`; clamp `P ∈ [0, 100]`. At `P ≥ 100.0` the phase erupts and the run ends. Capping subtracts 4.0 pressure instantly.
- **Capping**: a 0.30 s hold at `D ≥ 5 mm` inside the vent disc seals it. Two effectors cut the hold to 0.167 s. Releasing for > 100 ms decays the hold at 4×.
- **Wrong press**: a completed hold outside every unsealed vent cracks one crust segment, adds +2.0 pressure, and plays a 55 Hz crack cue. Three cracks end the run. Cracks repair one segment per phase clear.
- **Pre-cap**: sealing a `DORMANT` vent is worth 300 points and prevents its bloom entirely; it is the highest-value action in the game.
- **Scoring**: cap +150; pre-cap +300; fast cap within 2.5 s of bloom +75; phase clear +500; pressure bonus `10 × (100 − peak_P)`; no-crack phase +400. Consecutive caps within 4.0 s build a combo ×1.2, ×1.4, ×1.6, capped at ×2.0.
- **Phase timing**: 8 vents, 40 s soft limit. Vents 1–3 seed at t = 0, vents 4–5 at t = 10 s, vents 6–8 at t = 22 s. At t = 40 s, each unsealed vent flares for +18 pressure, then the phase resolves.
- **Win/lose**: clear all 5 phases. Eruption at any phase or the third crack ends the session at the current phase.
- **Pressure warnings**: 70 = amber ring and 1.0 s klaxon; 85 = red pulse at 1.5 Hz; 95 = screen-edge heat haze and a rising tone.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load level table | REGISTER |
| REGISTER | prop presented | centre error < 0.8 mm | fit sphere, set `+ẑ_P` | CALIBRATE |
| CALIBRATE | both palms cradle 400 ms | palm separation 82–104 mm | gravity settle, anchor `V=P` | BRIEF |
| BRIEF | cradle held | — | phase card 2.5 s | PHASE |
| PHASE | tick | grip valid | integrate spin, vents, pressure | PHASE |
| PHASE | vent hold reaches 0.30 s | inside disc, D ≥ 5 mm | seal, P −= 4.0, +score | PHASE |
| PHASE | hold completes off-disc | no vent within a+10° | crack +1, P += 2.0 | PHASE |
| PHASE | P ≥ 100 | — | eruption VFX, fail | DEBRIEF |
| PHASE | cracks = 3 | — | crust collapse, fail | DEBRIEF |
| PHASE | 8 vents sealed | — | bank peak, repair 1 crack | DEBRIEF |
| PHASE | t = 40 s | unsealed vents > 0 | flare +18 each, resolve | DEBRIEF |
| PHASE | both palms lost 350 ms | — | freeze pressure, ghost ball | REGRIP |
| REGRIP | both palms re-contact | ball within 70 mm of last pose | blend 120 ms | PHASE |
| REGRIP | 3.0 s timeout | — | "pick up the orange" card | REGRIP |
| DEBRIEF | phase < 5, no fail | — | next phase card | BRIEF |
| DEBRIEF | phase = 5 | — | session tally | COMPLETE |
| DEBRIEF | fail | — | retry card, one retry per session | BRIEF |
| any | prop tracking loss > 150 ms | — | freeze vents, ghost crust | REACQUIRE |
| REACQUIRE | relock ≤ 1.5 s | search 70 mm | blend 120 ms | prior |
| REACQUIRE | 1.5 s timeout | — | "present the orange" card | REACQUIRE |
| any | system pause | — | freeze all timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: cap hold 0.30 s; decay grace 100 ms; crack debounce 300 ms; vent subsidence 8.0 s; phase 40 s; reacquire 1.5 s; regrip 3.0 s; brief 2.5 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_sphere` (r = 38 mm), 4 swept fingertip capsules (r = 6 mm, l = 18 mm), 2 palm capsules (r = 18 mm, l = 70 mm), 8 analytic vent discs, 3 crust-crack decals (no collider).
- **Pair matrix**: Fingertip↔Sphere = contact and press; Palm↔Sphere = grip and spin torque; Fingertip↔Palm = ignored; Vent↔Fingertip = analytic angular test; Plume↔Hand = particle culling only; Crust↔Table = visual occlusion, no collision.
- **Prop body**: kinematic while cradled; `I = 8.664 × 10⁻⁵ kg·m²` is used only for the visual spin response. If both palms are lost for > 350 ms, the prop becomes dynamic: gravity 9810 mm/s², restitution 0.35, friction 0.62, linear damping 0.08/s, angular damping 0.6/s, and the pressure clock freezes. On re-cradle it returns to kinematic mode with the measured pose.
- **Materials**: peel µ = 0.62 (high-friction citrus), palm µ = 0.55, plug µ = 0.9. Restitution 0.35 for peel, 0.15 for plugs. No deformation model; the "squish" of the press is visual only.
- **CCD**: fingertip motion is capped at 1.5 m/s, i.e. 16.7 mm per 90 Hz frame, less than the 38 mm radius, so a swept capsule test is sufficient. The solve is analytic: closest approach of a segment to a sphere.
- **Pressure integration**: semi-implicit Euler at 90 Hz, `P_{n+1} = clamp(P_n + dt·(Σf_i − L·[N=0]), 0, 100)`. The 90 Hz step keeps the maximum per-tick pressure change at 0.16 points, so a cap cannot be lost to a single-frame overshoot.
- **Cap impulse**: on seal, a 0.15 N·s impulse is applied to the visual plug fragment pool; it does not move the prop.
- **Crack impulse**: a crack spawns 12 decal fragments with 0.04 N·s each; no force on the prop.
- **Collision budget**: 4 fingertip tests + 2 palm tests + 8 angular vent tests per tick = 14 narrow-phase pairs, well under 1 ms on the target SoC.
- **Drop case**: if the prop is released above the table, it falls 300–500 mm in 0.25–0.32 s; the phase is already frozen, and the ball is re-cradled from rest. The impact is rendered but never damages the prop.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → table/scene mesh → crust shell → lava fissures → vent discs → plugs → plume particles → hands → pressure ring/HUD.
- **Occlusion case 1 — palms over the south hemisphere**: the two palm meshes correctly hide 35–45 % of the crust. Vents under a palm are not rendered through skin; instead a 3 mm amber rim appears around the palm silhouette after 1.2 s, telling the player to rotate.
- **Occlusion case 2 — fingertip on a vent**: the pressing fingertip occludes the vent centre. The progress arc is drawn at `a_i + 4°`, outside the 10–14 mm fingertip footprint, and a 2 mm ring glows on the visible rim.
- **Occlusion case 3 — plume behind a finger**: plume particles within 8 mm of the hand mesh are culled, so lava never glows through a finger. The plume root remains visible on the exposed side.
- **Occlusion case 4 — crust against the table**: when the ball is held within 60 mm of the table, the table mesh depth-tests the lower crust and the plume; the upper crust is unaffected.
- **Occlusion case 5 — pressure ring behind the hands**: the ring is rendered at 96 mm from the centre, beyond the fingertips, and is depth-tested; if a hand covers more than 30 % of the ring, the ring switches to a 2 mm top-edge strip at `C + 110 mm·ŷ_W`.
- **Occlusion case 6 — vent on the far side**: back-facing vents are culled. Their glow is not leaked through the sphere; the only far-side cue is a 4 mm secondary glow at the silhouette when flux > 2.0/s.
- **VFX budgets**: 60 plume particles/s/vent (max 300), 120 ambient embers, 40 crack shards, 24 plug sparks, 8 ring pulses; global cap 700 particles. Overdraw is limited to 2.5× screen area by capping plume sprite size at 12 mm.
- **Crater material**: base color 0.09 albedo, emissive lava 4.2 cd/m² at the vent core, falling to 1.1 at the rim. The emissive pass is rendered after opaque geometry with a 0.5 mm depth bias.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Cradle valid | both palms | low stone-on-stone settle | crust brightens 4 % |
| Spin | ball surface | granular rumble, pitch ∝ |ω| | fissure parallax, dust motes |
| Vent bloom | vent position | rising steam whistle | disc expands, rim glows |
| Vent at 90 % | vent position | double whistle | red rim pulse at 2 Hz |
| Press start | fingertip | soft granular press | progress arc appears |
| Cap complete | vent position | 55 Hz thump + sinter hiss | plug flash, P −4.0 |
| Pre-cap | vent position | glassy chime | dormant pore freezes |
| Wrong press | contact point | sharp crust crack | decal + red flash, P +2.0 |
| Crack 2 of 3 | ball | low warning toll | one third of the ring goes dark |
| Pressure 70 | HUD ring | single klaxon | amber ring |
| Pressure 85 | HUD ring | 1.5 Hz pulse | red ring, edge heat |
| Pressure 95 | HUD ring | rising sine | screen-edge haze |
| Eruption | ball | sub-bass boom + rockfall | white flash, run ends |
| Phase clear | HUD | bronze gong | tally card, cracks −1 |
| Drop | table | soft fruit thud | ghost ball, pressure freeze |

All cues are spatialized at the vent position with a 0.9 m rolloff; the HUD cues are non-spatial and limited to −12 dBFS. Every gameplay event above has exactly one audio cue and one visual cue; there is no audio-only state.

## 13. Difficulty Progression & Tuning

| Level | Vents | Initial active | Bloom τ (s) | F0 (/s) | Phase time (s) | Crack cost | Starting P |
|---|---|---|---|---|---|---|---|
| 1 | 8 | 2 | 7.5 | 1.7 | 50 | 1 | 8 |
| 2 | 8 | 3 | 6.5 | 2.0 | 46 | 1 | 10 |
| 3 | 8 | 3 | 5.5 | 2.4 | 40 | 1 | 12 |
| 4 | 8 | 4 | 4.6 | 2.8 | 36 | 1 | 15 |
| 5 | 8 | 5 | 3.8 | 3.2 | 34 | 1 | 18 |

- **Adaptive rule** (`GEYSER_ADAPT`): after each phase, if peak pressure < 65 and no cracks occurred, level +1; if the phase erupted, level −1 and bloom τ ×1.15. Clamp 1–5; at most one step per phase.
- **Mercy rule**: after two consecutive failed attempts at the same phase, the next attempt gets F0 ×0.85 and one free crack repair.
- **Named constants**: `R_ORANGE = 38 mm`, `A_MIN = 3°`, `A_MAX = 18°`, `HOLD = 0.30 s`, `HOLD_TWO = 0.167 s`, `HOLD_DECAY = 4.0×`, `CAP_RELIEF = 4.0`, `LEAK = 1.2/s`, `CRACK_P = 2.0`, `CRACKS_MAX = 3`, `REACH_CAP = 78°`, `SUBSIDE = 8.0 s`, `SPIN_MAX = 300 °/s`.
- **Tuning intent**: at level 3, a player who caps one vent every 4.0 s and never cracks clears a phase with a peak pressure of 62–74. A player who ignores the far-side vents reaches 100 at t ≈ 31 s, which is the fail-forward signal to rotate earlier.

## 14. Edge Cases & Failure Recovery

- **Vent unreachable for 8 s**: it subsides to 40 % flux and a replacement seeds within the reachable cap. The sealed count and total vent count stay at 8; the HUD shows a 1.0 s "vent migrated" tick.
- **Vent under a palm**: no contact is possible, so reachability is computed from the vent centre, not from visibility. The palm-rim silhouette cue fires at 1.2 s, well before the 8 s subsidence.
- **Both palms lost**: pressure freezes within one tick; the prop becomes a 40 % ghost and falls if unsupported. Re-cradling within 3.0 s resumes at the same pressure; after 3.0 s the phase restarts from its opening state, with no score penalty.
- **One hand lost**: the remaining hand still contributes two effectors and half spin torque. If the lost hand is the only one holding the ball, the prop is treated as unsupported after 350 ms and the regrip rule applies.
- **Prop tracking loss > 150 ms**: freeze the vent field and pressure; keep the last pose; on relock, blend 120 ms. Vent hold progress is preserved for 1.0 s, then reset.
- **Crack at 2 segments when a cap completes**: the cap resolves first, then the crack; this ordering prevents a legitimate rescue press from being counted as the third crack.
- **Pressure exactly 100 while a cap is mid-hold**: if `h_i ≥ 0.28 s` at the moment `P` crosses 99.5, a 0.15 s critical-flare grace clamps `P` at 99.5 so the cap can finish. The grace is granted once per phase.
- **Player presses two vents at once**: both holds accumulate; if one completes and the other is on bare crust, the completed cap resolves first and the crust press is still counted. This is intentional risk.
- **Ball squeezed hard**: palm separation below 82 mm does not crack the prop; it reduces spin gain to 50 % and shows a "loosen grip" hint after 1.5 s.
- **False contact from a passing hand**: the 30 ms debounce plus the 5 mm press gate rejects a brush; a brush that reaches `D ≥ 5 mm` for < 0.30 s produces no crack.
- **Recovery UX**: one icon (orange / palm / hand) and one line of text; no modal dialog during a phase. Every recovery preserves phase index and score.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropOrange` — pose `P`, radius, texture odometry, dynamic/kinematic mode.
  - `CradleModel` — palm contacts, grip validity, spin torque weighting.
  - `ContactSampler` — per-effector `s`, `D`, `D_f`, debounce, contact age.
  - `VentField` — 8 vents, Fibonacci seeding, bloom law, reachability, subsidence.
  - `PressArbiter` — angular matching, hold accumulators, two-finger weighting, crack classification.
  - `PressureIntegrator` — pressure ODE, peak tracking, warning bands, critical grace.
  - `CrustModel` — 3 crack segments, repair, eruption state.
  - `PhaseDirector` — 8-vent schedule, phase timer, scoring, adaptive level.
  - `VolcanoRig` — crust mesh, fissure shader, vent discs, plugs, plumes.
  - `RingHud` — pressure ring, phase card, warning pulses.
- **Event flow**: `HeadsetTracker → PropOrange → ContactSampler → PressArbiter → VentField → PressureIntegrator`; `PhaseDirector → VentField → VolcanoRig`; `PressureIntegrator → EventBus {VentBloom, CapStart, CapDone, Crack, Warning70, Warning85, Eruption, PhaseClear} → VolcanoRig / AudioBank / RingHud / ScoreKeeper`. `VolcanoRig` never writes gameplay state.
- **Pseudocode — press arbitration (per tick)**:

```text
for e in effectors:
    s = |e.pos - C| - R_ORANGE
    e.contact = e.contact ? (s < 8.0) : (s < 5.0)
    if not e.contact: e.hold_target = none; continue
    D = clamp(5.0 - s, 0, 12.0)
    e.Df += (1 - exp(-dt / 0.040)) * (D - e.Df)
    if e.Df < 5.0: e.hold_target = none; continue
    v = argmin_i acos(normalize(e.pos - C) . vhat_i)
    if angle(v) <= a[v] + 2deg: e.hold_target = v
    else: e.hold_target = BARE_CRUST
for vent in vents:
    holders = [e for e in effectors if e.hold_target == vent]
    if holders:
        w = 1.8 if len(holders) >= 2 else 1.0
        vent.hold += w * dt
        if vent.hold >= (0.167 if w > 1 else 0.30): seal(vent)
    elif now - vent.last_hold > 0.100:
        vent.hold = max(0, vent.hold - 4.0 * dt)
```

- **Pseudocode — pressure and crust (per tick)**:

```text
flux = sum(F0 * (a[i] / 18deg) ** 2 for i in vents if i.state in (BLOOM, ACTIVE))
if no vent in (BLOOM, ACTIVE): flux -= LEAK
P = clamp(P + dt * flux, 0, 100)
peak = max(peak, P)
if P >= 100 and not critical_grace_used and any_cap_hold(0.28):
    P = 99.5; critical_grace_used = true
elif P >= 100: eruption()
if pending_crack:
    crust.cracks += 1; P = min(100, P + CRACK_P); pending_crack = false
    if crust.cracks >= 3: collapse()
```

- **Determinism**: all gameplay uses the fixed 90 Hz tick; rendering interpolation is read-only. Vent seeds are derived from `hash(sessionSeed, phaseIndex, ventIndex)` so a test can replay an exact phase.
- **Telemetry**: per phase, log `peakP`, `caps`, `preCaps`, `cracks`, `meanHold`, `spinIntegral`, and `unreachableTime` per vent. These are the tuning inputs.

## 16. Acceptance Test Checklist

1. Registration fits the orange centre to < 0.8 mm and radius to < 0.6 mm in 20/20 presentations; `+ẑ_P` is within 3° of world up after the 0.5 s settle.
2. A fingertip held at `s = 0 mm` for 300 ms seals a matched vent; at `s = 4.9 mm` (D = 0.1 mm) it never seals, even after 5 s.
3. A single-effector hold seals in 300 ± 20 ms; a two-effector hold on the same vent seals in 167 ± 20 ms.
4. Releasing a hold for 120 ms and re-pressing restarts the hold from 0 (decay 4×); releasing for 80 ms preserves it.
5. A completed hold 11° from every vent centre increments cracks by exactly 1 and pressure by exactly 2.0.
6. With 5 vents at `a = 18°` and F0 = 2.4, pressure rises at 12.0 ± 0.1/s; with no active vents it falls at 1.2 ± 0.05/s.
7. Sealing a vent subtracts exactly 4.0 pressure in the same tick and never below 0.
8. A phase with 8 vents ends early at 8 seals; a phase that reaches t = 40 s adds exactly 18.0 pressure per unsealed vent.
9. The third crack ends the run on the next tick, regardless of pressure; two cracks repair to one after a phase clear.
10. Rotating the ball by 90° about `+ẑ_P` moves a vent from the front to the right side; `|ω|` never exceeds 300 °/s and the P quaternion stays normalized within 1e-6.
11. A vent kept outside the 78° reachable cap for 8.0 s drops to 40 % flux and is replaced within 0.5 s; total unsealed vents remain 8.
12. Covering the prop for 1.0 s freezes pressure to within 0.1 and preserves all hold accumulators; relock blends in ≤ 120 ms.
13. At level 3, a scripted optimal route (cap highest flux first) clears all 8 vents in ≤ 34 s with peak pressure ≤ 78 in 5/5 runs.
14. Particle count stays ≤ 700 with 5 plumes at peak plus one crack burst and one plug flash; frame time on the target SoC stays ≤ 11.1 ms at 90 Hz.
