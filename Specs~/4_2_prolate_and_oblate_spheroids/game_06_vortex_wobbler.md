# Game 06 — Vortex Wobbler — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Vortex Wobbler |
| Slug | `vortex-wobbler` |
| Physical prop | Smooth oblate pebble (Mentos form), 19 mm equatorial diameter, 11 mm thick, 3.0 g, at rest on an A5 notebook cover |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate & Oblate Spheroids |
| Scale class | Tabletop, single-finger rim tap, one prop at rest |
| Source concept | A wobbling oblate disc spins a gravity whirlpool that pulls pods into a central airlock while debris must be swatted away |
| Source physical input | Rest the pebble flat; press the index fingertip on the rim and walk the press around the perimeter, leading the wobble by a quarter turn |
| Session shape | 5 waves, 20–35 s each, ≈3.5 min |
| Primary skill | Phase-locked rhythmic pressing against a precessing target |

## 1. Fantasy & Core Loop

**Fantasy.** The pebble is the containment cap of a derelict salvage ring. Its wobble pumps a gravity whirlpool into the notebook cover. Astronaut pods fall out of the dark and spiral toward the airlock at the disc's base; debris falls with them and must be knocked aside before it cracks the containment ring.

**Core loop** (wave 20–35 s; session 5 waves):

1. **Seed** (0.8 s) — pebble settles flat, vortex idles at 4 % strength, airlock ring lights.
2. **Pump** (continuous) — press the rim 90° ahead of the wobble azimuth; each in-phase press adds 6° of tilt amplitude.
3. **Hold the band** — amplitude 12–20° is stable; below 8° the vortex collapses, above 22° it overdrives.
4. **Read the field** — pods (blue) and debris (amber) spawn on the field rim and spiral inward at the pitch set by amplitude.
5. **Swat** — a rim tap aimed at a debris azimuth fires a 40° deflection wedge that pushes debris outward; pods resist 70 % of it.
6. **Dock** — pods crossing r < 14 mm are captured; debris crossing r < 12 mm costs 1 hull and adds 18 heat.
7. **Wave clear** — all pods captured or lost and all debris resolved; 3 s breath; next wave.

The tension is that pumping harder speeds *all* infall, so the player must pump in bursts: charge the vortex while debris is far, coast on stored amplitude while pods dock, and tap deflections without breaking the pump rhythm.

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

- **Exact spheroid**: `a = b = 9.5 mm`, `c = 5.5 mm`, oblate (`c/a = 0.579`), `m = 3.0 g`, uniform density 1.44 g/cm³. P origin = geometric center; `+ẑ_P` = polar axis normal to the two flat faces; poles at `(0, 0, ±5.5) mm`; equator = rim circle of radius 9.5 mm in the `x̂_P`–`ŷ_P` plane. Resting pose has `+ẑ_P` ≈ `+ŷ_T`.
- **Local rolling model**: `ω = (n̂ × v_t)/r_eff`. On the flat face the meridional radius of curvature at the pole is `r_eff = a²/c = 16.41 mm`; on the rim edge it is `r_eff = c²/a = 3.18 mm`; the model selects the branch from the contact latitude `|φ|` (face branch for `|φ| > 55°`). `v_t` is the tangential fingertip velocity at the contact, slip-clamped at 260 mm/s.
- **V frame**: anchored to **T**. Origin = the pebble's contact-point projection on the notebook cover at registration; `+ŷ_T` = cover normal; `+x̂_T` = player forward projected on the cover; `+ẑ_T = x̂_T × ŷ_T`. The pebble pose `P` only modulates field strength and direction; it never moves the field origin.
- **Deviations**: (a) the support is compliant (foam-backed notebook, `k_s = 35 N/mm`, `ζ = 0.16`), so the rocking frequency is 12 % below the rigid-table value; (b) the airlock is a virtual well drawn on a real cover, so it cannot cut passthrough — it is an additive ring plus a dark core at 85 % opacity; (c) the pebble is featureless, so azimuth is measured in T, not from any real mark.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated; notebook A5 (148 × 210 mm) centred 240–320 mm in front of the sternum, long edge away from the player; forearms resting on the table.
- **Play field**: the notebook cover, 210 × 148 mm, origin at the pebble contact. Usable radius 68 mm (short-axis half-width minus 6 mm margin).
- **Prop placement**: pebble at the field origin ±8 mm; flat face down; the player may re-centre it between waves.
- **Hand roles**: the index fingertip of the dominant hand is the only presser; the non-dominant hand rests on the table and is ignored except for pause.
- **Reach envelope**: fingertip travel is a 19 mm rim circle plus a 40 mm approach arc; wrist stays within 120 mm of neutral.
- **Forces**: press displacement 4–8 mm tangential over 40–120 ms with only 0.05–0.2 N of contact force; the cover carries the normal load, so a 3 g pebble is steered by displacement, not by force. Fatigue is low because the finger never bears weight.
- **Session**: 5 waves, hard stop 6 min; 30 s rest between waves.
- **Visual ergonomics**: the vortex is drawn at 0–35 mm above the cover so the player can look down at 35–50° without neck strain; a floating HUD ring sits at 12° above the field.

## 4. Prop Geometry & Registration

- **Real dimensions**: 19.0 mm diameter, 11.0 mm thick, 3.0 g; tolerance ±0.3 mm equatorial, ±0.2 mm polar, ±0.3 g. Model volume `(4/3)πa²c = 2079 mm³`.
- **Inertia** (uniform spheroid): `I_z = (2/5)ma² = 1.08 × 10⁻⁷ kg·m²`; `I_x = I_y = (1/5)m(a² + c²) = 7.23 × 10⁻⁸ kg·m²`. Used for the rocking ODE only.
- **Surface atlas**: 24 azimuthal sectors of 15° (`S00`–`S23`), each split into face (`|φ| > 55°`), shoulder (`25° < |φ| ≤ 55°`), and rim (`|φ| ≤ 25°`). Sector `S00` is centred on `+x̂_P`, increasing toward `+ŷ_P`.
- **Colliders**: `COL_disc` = the spheroid (hand contact and tap normal); `COL_rim` = torus at the equator (`R = 9.5 mm`, `r = 0.8 mm`) used only for rim-tap classification; `COL_cover` = static plane at `y_T = 0` (the notebook). No collider for the real fingertips beyond the joint spheres.
- **Registration**: (1) player lays the pebble flat and still for 1.0 s; (2) system fits the spheroid center over 12 frames, accepting center error < 0.5 mm and axis error < 3°; (3) `+ẑ_P` is set to the cover normal `+ŷ_T`, and `+x̂_P` is set to the player's forward projection, giving a deterministic sector zero; (4) `P` is published at 72 Hz and predicted to 90 Hz with a 1-frame linear extrapolation.
- **Re-acquisition**: search a 50 mm sphere around the last contact point; refit center and flat-face normal; keep the sector phase from the last reliable pose (a blank pebble cannot recover yaw from vision). If the pebble is on its rim, wait for it to settle flat before relocking.
- **Drift policy**: yaw drift is bounded by re-anchoring `+x̂_P` to the cover's `+x̂_T` whenever the pebble is at rest for 400 ms; no visible pop because the sector atlas is symmetric and gameplay uses relative azimuth.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Rim press | `R_index_tip` (dominant) | `COL_rim`, `|φ| ≤ 25°` | one pump/deflect event at azimuth λ |
| I5.2 | Face tap | `R_index_tip` | `COL_disc`, `|φ| > 55°` | "stabilise": amplitude −2°, no sector |
| I5.3 | Circular walk | `R_index_tip` trace around rim | `COL_rim` | continuous λ tracking for pump phase |
| I5.4 | Two-finger brace | `R_index_tip` + `R_middle_tip` on rim | `COL_rim` | locks amplitude for 0.6 s (deflection mode) |
| I5.5 | Lift-off | no contact > 120 ms | — | pump decays; no penalty |
| I5.6 | Long press | rim contact > 400 ms | `COL_rim` | overdrive warning, amplitude +0 if held |
| I5.7 | Re-seat | both hands off 500 ms then flat | `COL_cover` | re-zero field, keep score |
| I5.8 | Pause | `L_palm` faces up 700 ms | — | suspend timers |

Per-finger roles: dominant index = pump and deflect; dominant middle = brace/deflection modifier; all other fingers ignored. Per-hand roles: dominant = ACTUATOR, non-dominant = IDLE/PAUSE. The game never requires two simultaneous contacts except I5.4.

## 6. Input Kinematics & Signal Processing

- **Rocking state**: tilt `θ` (pole angle from `+ŷ_T`), azimuth `ψ` (direction of lean in T). Pole `u_T = (sinθ cosψ, cosθ, sinθ sinψ)`.
- **Rocking ODE**: `θ̈ = −ω_n²θ − 2ζω_n θ̇ + τ_press/I_c + η`, with `ω_n = 18.5 rad/s` (2.94 Hz; rigid value `√(5g/7R) = 20.7 rad/s` softened 12 % by the cover), `ζ = 0.16`, `I_c = I_x + m(a² + c²) = 4.35 × 10⁻⁷ kg·m²`, and `η` a 0.4° RMS process noise injected only while a finger is in contact.
- **Press input**: the contact is displacement-controlled. A 3 g pebble cannot absorb a finger's force — a 0.1 N·s impulse would give it 33 m/s — so the game integrates the *rim travel* `Δs` and only requires 0.05–0.2 N to maintain contact against the cover's static friction. Accepted press: `Δs ∈ [4, 8] mm` tangential, duration 40–120 ms. `ΔA = K_PUMP·q·(Δs/5 mm)` with `K_PUMP = 6°`, clamped to 26°.
- **Spin from the rim walk**: the same tangential travel turns the disc: `ω_spin += 0.55·(Δs/5 mm)` rad/s per press, capped at ±9.0 rad/s, decaying with `τ_spin = 2.0 s`. This is a calibrated gain, not a free-body impulse, because the finger–rim contact is a stick-slip walk, not a clean tangential impulse.
- **Pump phase**: target azimuth `λ* = ψ + 90°·sign(Ω_prec)`. Quality `q = cos(λ − λ*)`, clamped to [−1, 1]; a press at `λ*` adds the full 6°, a press 180° out of phase removes 6°.
- **Precession**: `Ω_prec = K_OMEGA·ω_spin`, `K_OMEGA = 1.0`; nominal `Ω_prec = 6.9 rad/s` (1.1 Hz) at `ω_spin = 6.9 rad/s`. The 1.1 Hz precession is the readable phase; the 2.94 Hz rocking is rendered as an edge shimmer, because a 3 Hz phase is not trackable by the eye.
- **Decay**: `A ← A·e^(−dt/1.4)` when no contact; `ψ ← ψ + Ω_prec·dt`; if `A < 3°` the vortex is OFF.
- **Signal chain**: joint positions → 3-tap median → one-euro filter (min-cutoff 1.2 Hz, beta 0.02) → contact query → rim-displacement integration. Sector λ is `atan2(p_y, p_x)` in P projected to T, quantised to 15°.
- **Debounce**: a new press cannot start within 30 ms of the previous release; two contacts on the same joint within 30 ms merge.
- **Latency budget**: tracking 14 ms + physics 11 ms + render 11 ms + audio 20 ms = 56 ms; the pump window is ±90° of phase, so 56 ms is 22 % of a 0.9 s precession period and is acceptable.

## 7. Virtual World Layout & Coordinate Mapping

- **T axes**: origin at the pebble contact on the cover; `+ŷ_T` = cover normal (up); `+x̂_T` = player forward projected on the cover; `+ẑ_T = x̂_T × ŷ_T` (player's right). All gameplay coordinates below are T.
- **Field**: radius `R_FIELD = 68 mm`, clipped to the notebook rectangle `x_T ∈ [−100, 100]`, `z_T ∈ [−68, 68]`.
- **Airlock**: centred at T origin, outer ring radius 14 mm, throat radius 10 mm, depth −6 mm (rendered as a well). Capture radius 14 mm; debris breach radius 12 mm.
- **Vortex**: Rankine core `r_c = 22 mm`; `v_θ(r) = (Γ/2πr)(1 − e^(−(r/r_c)²))`; `v_r = −0.55·v_θ` (spiral pitch 29°). `Γ = 3.9 × 10⁴ · sin(A) · (Ω_prec/6.9) mm²/s`, giving `Γ = 1.20 × 10⁴ mm²/s` at `A = 18°`, `v_θ(40 mm) = 48 mm/s`, `v_r = 26 mm/s`.
- **Spawns**: debris at `r = 58–68 mm`, pods at `r = 48–62 mm`, both at random azimuth with a 20° exclusion around the current press sector (so the player is not asked to swat through their own finger).
- **Deflection wedge**: half-angle 20°, radial band `r ∈ [16, 68] mm`, applied for 0.45 s with `Δv_r = +90 mm/s` to debris and `+27 mm/s` to pods, falling off as `cos(π·Δλ/40°)`.
- **Mapping formulas**: `p_T = (r cosα, 0, r sinα)`; entity height above cover `y_T = 4 mm` (debris) or 7 mm (pods); vortex ribbon particles at `y_T = 1 + 3·(r/R_FIELD) mm`.
- **Camera note**: the field is world-locked to the notebook, so leaning in does not scale it; this keeps the 68 mm radius readable as a physical object.

## 8. Gameplay Systems & Rules

- **Entities**: `Pebble` (P, θ, ψ, A, Ω), `VortexField` (Γ, direction), `Debris` (8–12 per wave), `Pod` (3–5 per wave), `Airlock` (hull, heat), `WaveDirector`.
- **Debris**: massless tracers with drag `k_d = 0.12/s`, so they follow the field; breach on `r < 12 mm`; deflected out of `r > 68 mm` → despawn and +25.
- **Pods**: same field plus a docking thruster `a_in = 8 mm/s²` inward when `r < 30 mm`, so a pod near the lock always completes; capture on `r < 14 mm` → +100.
- **Hull/heat**: 3 hull pips; debris breach = −1 hull, +18 heat. Heat ≥ 100 = containment breach → wave failed. Heat decays 4/s when no breach for 2 s.
- **Amplitude bands**: `< 8°` = COLLAPSED (pods drift outward at 12 mm/s, debris stalls); `8–12°` = weak (50 % field); `12–20°` = nominal; `20–22°` = hot (field ×1.25, heat +2/s); `> 22°` = OVERDRIVE (field ×1.6, debris inflow ×2, heat +6/s, screen shake).
- **Deflection cost**: each wedge costs 0.9° of amplitude (energy is diverted), so spamming deflections starves the vortex.
- **Scoring**: pod +100; pod within 1.5 s of a debris deflection +40 (convoy); debris deflected +25; wave clear +250; perfect wave (no hull loss, all pods) +400; hull remaining ×150.
- **Wave composition**: 5 waves; pods 3/4/4/5/5; debris 6/8/9/10/12; spawn cadence 1.4 s down to 0.8 s.
- **Win/lose**: wave fails at 0 hull or heat 100; session ends after 5 waves or 3 failures; win if ≥ 3 waves cleared.
- **Idle rule**: if no press for 3 s, an arrow traces the rim at the target azimuth; the hint stops after the first in-phase press.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, sensor self-test | REGISTER |
| REGISTER | pebble flat, still 1.0 s | center error < 0.5 mm | set P, sector zero, field origin | CALIBRATE |
| CALIBRATE | cover plane locked | plane RMS < 1.5 mm | build T, place airlock | BRIEF |
| BRIEF | 3 s card | — | wave 1 composition | PUMP |
| PUMP | rim press | `4 ≤ Δs ≤ 8 mm`, 40–120 ms | classify sector, apply ΔA | PUMP |
| PUMP | in-phase press | `q > 0.5` | `A += K_PUMP·q·(Δs/5 mm)` | PUMP |
| PUMP | tap in deflection band | cooldown 0.25 s | spawn wedge, `A −= 0.9°` | PUMP |
| PUMP | `A > 22°` | — | overdrive, heat +6/s | OVERDRIVE |
| OVERDRIVE | `A ≤ 20°` for 1.0 s | — | resume nominal, heat decays 4/s | PUMP |
| PUMP | pod `r < 14` | — | capture, +100, VFX | PUMP |
| PUMP | debris `r < 12` | — | hull −1, heat +18 | PUMP |
| PUMP | heat ≥ 100 | — | containment breach | FAIL |
| PUMP | wave resolved | no live pods/debris | tally, +250 | DEBRIEF |
| FAIL | breach anim 2.5 s | — | wave lost, level −1 | DEBRIEF |
| DEBRIEF | wave < 5, hull > 0 | — | 30 s rest, next wave | BRIEF |
| DEBRIEF | wave = 5 or 3 fails | — | session stats | COMPLETE |
| any | prop lost > 150 ms | — | freeze field, ghost disc | REACQUIRE |
| REACQUIRE | relock ≤ 2.0 s | search 50 mm | hold A, resume | prior |
| REACQUIRE | 2.0 s timeout | — | "present the pebble" card | REACQUIRE |
| any | system pause / palm up 700 ms | — | freeze timers | SUSPENDED |
| SUSPENDED | palm down 500 ms | — | 1.5 s countdown | prior |

Timers: pump decay τ = 1.4 s; spin decay τ = 2.0 s; deflection cooldown 0.25 s; wedge 0.45 s; overdrive exit 1.0 s; reacquire 2.0 s; rest 30 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_disc` (spheroid), `COL_rim` (torus), `COL_cover` (plane), 12 debris spheres (`r = 3 mm`), 5 pod capsules (`r = 5 mm`, length 10 mm), 1 airlock disc (`r = 14 mm`).
- **Pair matrix**: Finger↔Disc (contact only, no dynamics); Finger↔Cover (reject, prevents phantom presses through the notebook); Debris↔Pod (elastic separation, restitution 0.3); Debris↔Airlock (breach event); Pod↔Airlock (capture event); Debris↔Debris (ignore, they are tracers).
- **Field integration**: semi-implicit Euler at 90 Hz, 2 substeps if `|v| > 260 mm/s`; `v ← (v + a_field·dt)·e^(−k_d dt)`, `p ← p + v·dt`. `a_field = (v_θ²/r + v_r·|v_r|/r)` radial plus tangential `v_θ·v_r/r`.
- **CCD**: debris max speed 310 mm/s → 3.44 mm/substep, less than the 3 mm radius plus 1 mm skin; a swept-sphere test runs on the airlock and pod pairs. Pods max 140 mm/s need no CCD.
- **Materials**: cover µ = 0.55 (debris slides, does not roll); pebble µ = 0.30 against skin; notebook compliance `k_s = 35 N/mm`, damping 0.16.
- **Forces**: deflection impulse `Δv = 90 mm/s` (debris) and `27 mm/s` (pods); docking thruster 8 mm/s²; no gravity in the field plane (entities are magnetically levitated tracers), but a 2 mm sinusoidal bob at 0.6 Hz for depth.
- **Rocking coupling**: the field is recomputed from the *filtered* amplitude at 20 Hz, not per frame, to avoid visual jitter from the 2.94 Hz rock.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → cover decal (vortex, airlock) → volumetric ribbons → debris/pods → deflection wedges → pebble ghost ring → hands → HUD.
- **Occlusion cases**:
  1. *Pressing fingertip*: the index tip covers 6–9 mm of the 19 mm disc; the rim sector under the finger is drawn 40 % transparent so the player still sees the sector highlight.
  2. *Pebble vs cover*: the pebble is real and occludes its own decal; the decal is drawn only outside the projected disc silhouette (analytic ellipse test), avoiding z-fighting on the cover.
  3. *Hand over the field*: a palm crossing the vortex dims the ribbons 50 % in the occluded region and shows a soft shadow; entities are not hidden, so gameplay stays readable.
  4. *Airlock under the pebble*: the airlock ring is 14 mm radius and the pebble is 9.5 mm, so a 4.5 mm annulus is always visible; when the pebble is centred, the ring is drawn above the disc in the near plane with a 2 mm lift.
  5. *Notebook edge*: ribbons are clipped at the cover rectangle; a 3 mm falloff prevents a hard cut on the real edge.
  6. *Deflection wedge*: drawn as an additive fan at `y_T = 2 mm`, depth-tested against the hand; when the finger is inside the wedge the fan is pushed to `y_T = 6 mm` to avoid clipping into skin.
- **VFX budgets**: vortex ribbons 220 segments; debris trail 6 per entity; pod trail 8; capture burst 26; breach 40; wedge 18; ambient dust 40; global cap 420 particles and 900 ribbon vertices.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Press accepted | rim sector | dry ceramic tick, pitch ∝ q | sector flashes cyan, amplitude bar + |
| Out-of-phase press | rim sector | dull thud, 20 % quieter | sector flashes grey, bar − |
| Wobble in band | disc | low 2.9 Hz hum, stereo width ∝ A | rim glow pulses at precession rate |
| Vortex collapsed | airlock | descending airy whoosh | ribbons fade to 10 %, ring dims |
| Overdrive | disc | rising turbine + sub-bass | red rim, screen shake 1.5 mm |
| Pod spawn | field rim | soft sonar ping | blue comet trail |
| Pod captured | airlock | water-drop plop + chime | ring flash, +100 float |
| Debris spawn | field rim | gravel crackle | amber trail |
| Deflection | wedge | whip crack + air punch | fan sweeps out, debris streak |
| Debris breach | airlock | metal crunch + alarm | hull pip cracks, red vignette |
| Wave clear | room | bronze gong + pump down | tally card |
| Wave failed | room | containment siren | iris-out |

## 13. Difficulty Progression & Tuning

| Level | Debris | Pods | Spawn gap (s) | Field gain | Deflect cooldown (s) | Hull | Band width (°) |
|---|---|---|---|---|---|---|---|
| 1 | 6 | 3 | 1.5 | 0.85× | 0.30 | 3 | 12–20 |
| 2 | 8 | 4 | 1.3 | 1.00× | 0.28 | 3 | 12–20 |
| 3 | 9 | 4 | 1.1 | 1.10× | 0.25 | 3 | 13–19 |
| 4 | 10 | 5 | 0.9 | 1.20× | 0.22 | 2 | 13–19 |
| 5 | 12 | 5 | 0.8 | 1.30× | 0.20 | 2 | 14–18 |

- **Adaptive rule** (`VORTEX_ADAPT`): after each wave, if hull lost = 0 and clear time < 22 s, level +1; if the wave failed, level −1, spawn gap ×1.15, and band width +2°. Clamp 1–5; one step per wave.
- **Named constants**: `A_NOM = 18°`, `A_COLLAPSE = 8°`, `A_MAX = 26°`, `A_OVERDRIVE = 22°`, `K_PUMP = 6°`, `PRESS_DS = 4–8 mm`, `OMEGA_N = 18.5 rad/s`, `ZETA = 0.16`, `TAU_DECAY = 1.4 s`, `TAU_SPIN = 2.0 s`, `GAMMA_GAIN = 3.9e4 mm²/s`, `SPIRAL = 0.55`, `R_CORE = 22 mm`, `R_LOCK = 14 mm`, `R_BREACH = 12 mm`, `DEFLECT_DV = 90 mm/s`, `HEAT_PER_HIT = 18`.

## 14. Edge Cases & Failure Recovery

- **Pebble rolls off the notebook**: if the contact point leaves the cover rectangle, freeze the field, mark the wave "paused", and show a 1-line "re-centre the pebble" hint; no hull or score change.
- **Pebble on its rim**: registration holds the last flat pose; the field runs at 30 % until the pebble settles; a settle detector requires θ < 4° for 250 ms.
- **Two fingers press at once**: only the contact with the larger rim displacement is accepted; the second is logged as a brace and reduces amplitude by 1° (it disturbs the rock).
- **Press through the notebook**: palm contact below the cover plane is rejected by the `COL_cover` half-space; this prevents phantom presses when the hand rests on the table edge.
- **Tracking loss > 150 ms**: freeze the field and all entities; keep θ, ψ, A and Γ; the disc becomes a 40 % ghost. On relock, hold A and resume; no wave progress is lost.
- **Hand tracking loss**: if the actuator hand vanishes, decay continues naturally (no presses); if the idle hand vanishes, nothing changes.
- **Very fast tapping (> 5 Hz)**: rim walks arriving faster than the 120 ms rocking period are merged into one with `Δs = ΣΔs` capped at 8 mm, preventing numerical blow-up.
- **Deflection spam**: cooldown 0.25 s plus the 0.9° amplitude cost means sustained spam drains 3.6°/s and collapses the vortex in < 4 s; this is intended and taught by the OVERDRIVE warning.
- **Non-dominant hand play**: the handedness flag mirrors I5.1–I5.4; the field geometry is unchanged.
- **Cover slip**: if the notebook moves > 8 mm during a wave (detected from the plane fit), re-anchor T to the new cover pose and keep relative entity positions.
- **Recovery UX**: one icon (pebble / hand / notebook) plus one line of text; no modal dialogs mid-wave.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropPebble` — pose P, center, flat-face normal, sector atlas, rest detector.
  - `RockModel` — θ, ψ, A, ω_spin, ODE integration, pump quality, decay.
  - `PressClassifier` — rim travel Δs, duration, sector λ, brace/deflect disambiguation.
  - `VortexField` — Γ, direction, `v_θ/v_r`, Rankine core, sampling for entities.
  - `FieldEntity` — debris/pod state, field integration, CCD, capture/breach tests.
  - `DeflectionWedge` — sector, timer, falloff, impulse application.
  - `WaveDirector` — composition, spawn cadence, scoring, adaptive level.
  - `AirlockModel` — hull pips, heat, breach state.
  - `VortexRig` — ribbons, airlock decal, wedge fan, capture VFX.
- **Event flow**: `HandTracker → PressClassifier → RockModel → VortexField`; `WaveDirector → FieldEntity`; `FieldEntity → EventBus {PodCaptured, DebrisBreached, DebrisDeflected, WaveClear, Overdrive}`; `EventBus → AudioBank / VFXPool / ScoreKeeper / HUD`. `VortexRig` reads `VortexField` and `EventBus` only.
- **Pseudocode — rocking + pump (per tick)**:

```text
dt = 0.0111
for c in contacts where c.joint == ACTUATOR_TIP:
    if c.travel >= 4mm and c.travel <= 8mm and c.duration in [40ms, 120ms]:
        lam = sector_of(c.point)
        q = clamp(cos(lam - (psi + 90deg * sign(omega_prec))), -1, 1)
        A = clamp(A + K_PUMP * q * (c.travel / 5mm), 0, A_MAX)
        omega_spin += 0.55 * (c.travel / 5mm)
        c.consumed = true
omega_prec = K_OMEGA * clamp(omega_spin, -9, 9)
psi += omega_prec * dt
A = A * exp(-dt / TAU_DECAY) if no_contact else A
theta = A * sin(OMEGA_N * t)          # 2.94 Hz rock, rendered as shimmer
if A < A_COLLAPSE: field_state = COLLAPSED
else if A > A_OVERDRIVE: field_state = OVERDRIVE
else: field_state = NOMINAL
```

- **Pseudocode — field + scoring (per tick)**:

```text
Gamma = GAMMA_GAIN * sin(deg2rad(A)) * (omega_prec / OMEGA_NOM)
for e in entities:
    vt = abs(Gamma) / (2*pi*max(r, 1)) * (1 - exp(-(r/R_CORE)^2))
    vr = -SPIRAL * vt * sign(Gamma)
    e.v += (radial(e) * vr + tangent(e) * vt * sign(Gamma)) * dt
    e.v *= exp(-e.drag * dt)
    e.p += e.v * dt
    if e.kind == POD and r < R_LOCK: capture(e); score += 100
    if e.kind == DEBRIS and r < R_BREACH: breach(); hull -= 1; heat += 18
    if e.kind == DEBRIS and r > R_FIELD: despawn(e); score += 25
heat = max(0, heat - 4 * dt) if now - last_breach > 2 s else heat
if heat >= 100: fail_wave()
```

## 16. Acceptance Test Checklist

1. Registration fits the pebble center to < 0.5 mm and the flat-face normal to < 3° in 20/20 presentations; sector zero is stable to ±7.5° across re-registration.
2. A 5 mm in-phase rim walk over 80 ms raises amplitude by 6.0° ± 0.6°; the same 5 mm walk 180° out of phase lowers it by 6.0° ± 0.6°.
3. With no presses, amplitude decays from 18° to 6.6° in 1.40 s ± 0.10 s (one time constant).
4. Three consecutive in-phase presses at 3 Hz reach the 12–20° band within 0.9 s and do not exceed 26°.
5. Holding amplitude at 18° produces `Γ = 1.20 × 10⁴ ± 5 % mm²/s`, `v_θ(40 mm) = 48 ± 3 mm/s`, and a debris infall from 60 mm to 14 mm in 1.8 ± 0.3 s.
6. A deflection at azimuth λ moves a debris at the same λ from 40 mm to > 68 mm within 0.65 s, and a pod at the same λ by < 30 % of that displacement.
7. A debris crossing r < 12 mm costs exactly 1 hull and adds 18 heat; a pod crossing r < 14 mm scores exactly +100 and cannot also breach.
8. Overdrive at A = 24° doubles debris inflow (measured over 2 s) and raises heat at 6/s ± 0.5/s.
9. Deflection spam at the minimum cooldown drains amplitude at 3.6 ± 0.4°/s.
10. Covering the pebble for 1 s freezes all entities and preserves A, hull, and heat; relock blends in ≤ 200 ms with no score change.
11. A 15° per-sector press is classified into the correct sector in 24/24 scripted trials, and a press 7.5° off a boundary does not flicker between sectors.
12. A level-2 wave (8 debris, 4 pods) is clearable with 3 hull remaining in 5/5 scripted runs under 30 s.
13. The full 5-wave session completes in ≤ 6 min with 0 dropped frames caused by particle count (cap 420 respected).
14. Removing the actuator hand for 5 s decays amplitude to < 1° and sets the field to OFF; restoring the hand does not auto-restore amplitude.
