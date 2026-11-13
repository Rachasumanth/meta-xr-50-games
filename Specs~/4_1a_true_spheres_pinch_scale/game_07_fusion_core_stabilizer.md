# Game 07 — Fusion Core Stabilizer — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Fusion Core Stabilizer |
| Slug | `fusion-core-stabilizer` |
| Physical prop | Small high-bounce rubber ball, 24 mm diameter, ~9 g, trapped between the flat centers of both palms |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.1a True Spheres — Pinch-Scale (1–4 cm) |
| Scale class | Palm-scale two-hand sandwich, synchronized circular rolling |
| Source concept | "A miniature star ignites between your palms, felt through the rolling ball. Adjust your circular rolling speed and palm pressure to counteract magnetic instabilities and keep the star from going supernova." |
| Source physical input | Trap the small sphere between the flat centers of both palms and roll your hands in tiny, synchronized 2 cm circular orbits. |

## 1. Fantasy & Core Loop

**Fantasy.** The rubber ball is a containment pellet, and the two palms are the magnetic constrictor coils of a fusion reactor. Between them a miniature sun ignites — a white-hot bead of plasma that flares through the gap between your fingers. Squeeze to raise containment pressure, orbit your palms to spin the field, and keep the two orbits phase-locked or the star tears itself apart.

**Core loop** (one pulse ≈ 8 s; one burn ≈ 80–110 s):

1. **Ignite** (3.0 s) — bring the palms together around the ball; the core lights as palm separation drops below 30 mm.
2. **Read the instability** (0.4–0.8 s) — a colored arc shows FLUX (red), SHEAR (blue), or WOBBLE (violet).
3. **Correct** (1.0–2.5 s) — squeeze harder for FLUX, orbit faster for SHEAR, phase-lock the orbits for WOBBLE.
4. **Hold** (3.0–5.0 s) — keep all three parameters inside their bands while the pulse timer drains.
5. **Pulse clear** (0.6 s) — the star flares cleanly, containment score banks, bands shift for the next pulse.
6. **Surge** (1.5 s) — every third pulse, a rapid double-band demand; then back to STABLE.
7. **Burn tally** (3.0 s) — after 8 pulses, the reactor shuts down; tally.

There is no discrete "button": every control is a continuous physical property of how you hold the ball, which is the point.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `L_palm`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the center, poles at `±ẑ_P`, equator in `x̂_P`–`ŷ_P`, no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]`, longitude `λ ∈ [0°, 360°)`; `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; normal `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P frame**: origin = ball center; `+ẑ_P` = the **palm axis**, pointing from the left palm toward the right palm; `+x̂_P` = up (world-up projected into the palm plane); `+ŷ_P` completes the frame. The ball's orientation is irrelevant to the game; only its center and spin about `ẑ_P` matter.
- **Numbers**: r = 12 mm, m = 9 g, rubber, µ_rubber-skin ≈ 0.9 (high friction — the palms grip the ball rather than slipping).
- **Palm axis**: `â = normalize(c_R − c_L)` where `c_L`, `c_R` are the palm centers; `d = |c_R − c_L|` is the palm separation.
- **V frame**: anchored to **W**, not P. The star and its containment rings are world-fixed between the palms, so squeezing does not spin the star. The ball is the hidden mechanism; the star is the visible one.
- **Deviations**: (a) the ball is fully enclosed by the palms, so it is mostly invisible — the game renders the star through a virtual "containment window" between the fingers; (b) because both palms contact the ball, the ball's center is constrained to the midpoint of the palm centers, which is the game's primary measurement; (c) the "2 cm circular orbits" are palm-center orbits in the plane perpendicular to `â`, measured from the palm meshes, not from the ball.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated or standing; both forearms raised, palms facing each other at 200–280 mm in front of the sternum; elbows tucked.
- **Support surface**: none; calibration uses a table only to establish the initial vertical palm axis.
- **Play volume**: a 120 mm cube between the palms; the palms orbit within a 2 cm radius disc, so the whole interaction fits in a 60 mm envelope.
- **Handedness**: symmetric; the palm axis direction defines left/right, not dominance.
- **Reach envelope**: palms at 180–300 mm from the shoulders; elbows stay below shoulder height.
- **Session length**: 7 min default (5 burns), hard stop 10 min.
- **Fatigue mitigations**: (1) the arms can rest on the table with the palms still vertical; (2) the required orbit radius is only 20 mm, so motion is mostly wrist; (3) a 6 s "cooldown" between burns with hands lowered; (4) pressure is capped at a comfortable 12 N-equivalent, and the game never asks for a harder squeeze than level 3's calibration.

## 4. Prop Geometry & Registration

- **Real dimensions**: diameter 24.0 mm (r = 12.0 mm), mass 9.0 g, hollow rubber, restitution 0.82 against skin, µ ≈ 0.9.
- **Sphere model** (P frame, mm): surface `|p| = 12`; no features. Contact patches: two spherical caps of half-angle 32° at `±ẑ_P`, where the palms compress the rubber by up to 3 mm.
- **Colliders**: `COL_ball` r = 12 mm; `COL_palmL` / `COL_palmR` are plane+curvature proxies of the palm surfaces; `COL_gap` is the lens-shaped free region between the fingers used for the containment-window render.
- **Registration**: (1) player sandwiches the ball between flat palms; (2) system fits the ball center as the midpoint of the two palm centers and estimates `â`; (3) radius is fixed at 12 mm (the ball class is known); (4) `P` published at 72 Hz, but the game uses `c_L`, `c_R`, `d`, and the palm-orbit phases as its real state; the ball pose is a derived convenience.
- **Re-acquisition**: search 60 mm around the last midpoint; re-fit from the two palm meshes; blend over 150 ms. A "hold the ball between your palms" card after 2 s. If the palms separate by > 60 mm, the ball is considered dropped and the burn pauses.
- **Compression estimate**: `δ = clamp((24 − d)/2, 0, 3) mm` per side; used for pressure and for the rubber-squash visual.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Palm sandwich | both palm meshes + `L/R_middle_tip` | ball, ±ẑ_P caps | containment present; d measured |
| I5.2 | Squeeze | decrease d | palms | pressure `p ∈ [0,1]` |
| I5.3 | Release | increase d | palms | pressure falls; star dims |
| I5.4 | Circular orbit | both palm centers trace 20 mm circles | none (pose) | spin `Ω` about `â` |
| I5.5 | Phase-lock | left/right orbit phases aligned | none | coherence `C = cos(Δφ)` |
| I5.6 | Counter-orbit | phases opposed | none | negative coherence; destabilizes |
| I5.7 | Tilt axis | change `â` direction | none | rotates the star's ring plane |
| I5.8 | Grip break | d > 45 mm | — | containment lost; 1.5 s grace |

Per-finger roles: the palms and middle fingers provide the sandwich; the thumb/index/pinky are free and ignored. Per-hand roles: symmetric; there is no dominant hand. The phase of each palm's orbit is measured independently, then compared.

## 6. Input Kinematics & Signal Processing

- **Palm centers**: `c_L`, `c_R` from the palm mesh centroid, filtered with a 5-tap Savitzky–Golay filter at 72 Hz.
- **Separation**: `d = |c_R − c_L|`; pressure `p = clamp((30 − d)/8, 0, 1)` (d = 30 mm → 0, d = 22 mm → 1).
- **Palm axis**: `â = normalize(c_R − c_L)`; tilt rate `|dâ/dt|` feeds the ring-plane visual.
- **Orbit plane**: basis `(û, v̂)` perpendicular to `â`, with `û = normalize(ẑ_W − (ẑ_W·â)â)`; palm offsets `o_L = c_L − mean(c_L,c_R)`, `o_R = c_R − mean(...)`.
- **Orbit phase**: `φ_L = atan2(o_L·v̂, o_L·û)`, `φ_R = atan2(o_R·v̂, o_R·û)`; `Δφ = wrap(φ_L − φ_R)`; coherence `C = cos Δφ`.
- **Orbit radius**: `ρ = (|o_L| + |o_R|)/2`; a valid orbit needs `ρ ∈ [12, 28] mm`; below 12 mm the spin estimate is unreliable, above 28 mm the palms are moving too far apart.
- **Spin**: `Ω = (dφ_L/dt + dφ_R/dt)/2`, in °/s, filtered at `f_c = 4 Hz`; the game uses `|Ω|` and its sign only for the ring rotation direction.
- **Coherence band**: `C > 0.70` is "locked"; `C < 0.30` is "opposed". Coherence is smoothed over 300 ms.
- **Pressure band**: target bands are narrow (e.g. [0.45, 0.60]); pressure is sampled at 90 Hz and must remain in-band for ≥ 85 % of a pulse window to count.
- **Instability growth**: each instability has a severity `S ∈ [0,1]`:
  - `S_flux += 0.55·dt` when `p < p_lo`, `+= 0.35·dt` when `p > p_hi`, decays `0.40·dt` in band.
  - `S_shear += 0.55·dt` when `|Ω| < Ω_lo` or `> Ω_hi`, decays in band.
  - `S_wobble += 0.50·dt` when `C < C_min`, decays in band.
  - Any `S = 1.0` → containment breach (supernova).
- **Containment meter**: `M = 1 − max(S_flux, S_shear, S_wobble)`; displayed as the star's brightness/radius. `M ≤ 0` ends the burn.

## 7. Virtual World Layout & Coordinate Mapping

- **V frame**: anchored to W, origin = midpoint of the palm centers `O_V = (c_L + c_R)/2`; `x̂_V = û`, `ŷ_V = v̂`, `ẑ_V = â`. The star sits at `O_V`.
- **Star**: a 16 mm glowing sphere with a 3-layer corona; its radius pulses with `M` (`r_star = 12 + 8·M mm`).
- **Containment rings**: three torus rings at radii 34, 48, and 62 mm in the `û`–`v̂` plane, each rotating at `Ω` with a phase offset of 120°; ring color encodes the dominant instability (red FLUX, blue SHEAR, violet WOBBLE).
- **Containment window**: a lens-shaped transparent aperture between the index and middle fingers of both hands, through which the star is visible; outside the window the star is rendered at 25 % alpha.
- **Instability arcs**: three arc gauges at 70 mm above the star, showing `p`, `|Ω|`, and `C` against their bands, color-coded.
- **Mapping formulas**: star world position = `O_V`; ring plane normal = `â`; ring point at angle θ = `O_V + R·(cosθ·û + sinθ·v̂)`.
- **Camera note**: because the star is exactly between the palms, it is often partially occluded by the hands; the containment window is the designed solution, and the arcs are always drawn on top.

## 8. Gameplay Systems & Rules

- **Entities**: `Reactor` (M, S_flux, S_shear, S_wobble, output), `Pulse` (bands, timer, phase), `BurnDirector`.
- **Pulse structure**: IGNITION 3 s → STABLE 6 s (bands fixed) → SURGE 2 s (bands tighten by 30 %) → repeat. Every third pulse is a SURGE immediately after STABLE.
- **Bands by level** (§13): pressure `[p_lo, p_hi]`, spin `[Ω_lo, Ω_hi]`, coherence `C_min`.
- **Scoring**: each pulse clear +150 × `M_end`; perfect pulse (all three ≥ 0.95 for the whole window) +250; burn clear +500; remaining containment ×100; no-breach burn +400.
- **Output**: the star's fusion output ramps while `M > 0.7`; output is the score multiplier bank (1.0 + 0.5·(output/100)).
- **Breach**: any `S = 1.0` → supernova; 1.5 s white-out; burn ends; containment resets to 0.5 for the next burn.
- **Win/lose**: burn cleared after 8 pulses; failed at a breach. Session = 5 burns; win if ≥ 3 cleared.
- **Combo**: three consecutive perfect pulses → "Ignition Lock" (bands widen 15 % for the next pulse only).

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, UI | REGISTER |
| REGISTER | ball sandwiched | d < 32 mm | fix r = 12 mm, `â` | CALIBRATE |
| CALIBRATE | pressure hold 1 s | p ∈ [0.3, 0.7] | zero orbit phases | BRIEF |
| BRIEF | squeeze once | p > 0.5 | burn card 2.5 s | IGNITION |
| IGNITION | 3.0 s elapsed | containment present | star ignites, output ramps | STABLE |
| STABLE | instability in band | — | S decays, M rises | STABLE |
| STABLE | instability out of band | — | S grows | STABLE |
| STABLE | 6.0 s elapsed | M > 0 | pulse clear, +score | SURGE |
| SURGE | 2.0 s elapsed | M > 0 | next pulse, bands relax | STABLE |
| any | S = 1.0 | — | supernova white-out | BREACH |
| STABLE/SURGE | d > 45 mm | — | containment lost | GRACE |
| GRACE | d < 35 mm within 1.5 s | — | resume same pulse | prior |
| GRACE | 1.5 s elapsed | — | containment lost, breach | BREACH |
| BREACH | 1.5 s elapsed | — | burn failed | DEBRIEF |
| any | S = 1.0 after 8 pulses? | — | burn clear instead | DEBRIEF |
| any | prop/hand tracking loss > 200 ms | — | freeze S, ghost star | REACQUIRE |
| REACQUIRE | relock ≤ 2 s | search r = 60 mm | blend midpoint 150 ms | prior |
| REACQUIRE | 2 s timeout | — | "hold the ball" card | REACQUIRE |
| DEBRIEF | burn < 5, cleared | — | next burn seed | BRIEF |
| DEBRIEF | burn = 5 or 3 fails | — | session stats | COMPLETE |
| any | idle 20 s / system pause | — | freeze timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: ignition 3.0 s; stable 6.0 s; surge 2.0 s; containment grace 1.5 s; breach white-out 1.5 s; reacquire 2 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_ball` r = 12 mm, `COL_palmL`, `COL_palmR` (curved plane proxies), `COL_gap` lens, three containment rings (visual only), star volume (visual only).
- **Pair matrix**: PalmL↔Ball (contact, no rigid response), PalmR↔Ball (contact), PalmL↔PalmR (soft constraint via the ball), Star↔nothing (visual), Hand↔Star (none).
- **Material values**: rubber restitution 0.82; compression stiffness `k = 0.8 N/mm` per side; damping `c = 0.06 N·s/mm`; the ball is in quasi-static compression, not bouncing.
- **Physical constants**: m_ball = 9 g; r = 12 mm; µ = 0.9; palm contact cap half-angle 32°; maximum comfortable squeeze 12 N.
- **CCD**: no fast bodies; the ball's center is derived from the palms each tick, so no tunneling.
- **Force formulas**: normal force `F = k·δ + c·δ̇` (per palm), `δ` from the palm separation; pressure `p` is a normalized proxy, deliberately not newtons, so the game does not require a force sensor.
- **Orbit model**: the palms are kinematic (tracked); the star's rings follow `Ω`; there is no simulated orbital mechanics beyond the displayed rotation.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → star corona → containment rings → VFX → hands → containment window mask → HUD arcs.
- **Occlusion cases**:
  1. *Palms enclose the star*: both palms physically block the star; the containment window (the lens between the fingers) is a stencil that reveals the star in the gap, and the palms are rendered with a 12 % edge glow so the star appears to shine *through* the flesh.
  2. *Fingers over the arcs*: the HUD arcs are exempt from occlusion and drawn at 100 % over the hands, so the player can always read the bands.
  3. *Star through the ball*: the real ball is inside the star volume; the star is rendered at 80 % alpha with a Fresnel core so the ball's silhouette is faintly visible, preserving the illusion that the ball *is* the core.
  4. *Ring behind the palms*: the outer 62 mm ring is larger than the palm gap and is occluded by the palms; its near half is drawn normally, its far half as a dashed guide.
  5. *Haze/vignette during breach*: the white-out is a full-screen composite over passthrough, fading over 1.5 s.
- **VFX budgets**: corona 40/s, ring sparks 8/s, pulse flare 30, breach flash (full-screen, no particles), instability wisps 12/s per active channel, global cap 400.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Containment | between palms | low reactor hum, volume ∝ M | star brightness |
| Squeeze | palms | leather creak + pressure hiss | ring contracts |
| Orbit | between palms | gyro whine, pitch ∝ Ω | rings spin |
| Phase lock | between palms | two tones beating into unison | coherence arc turns green |
| FLUX warning | star | low throb, tempo rises | red arc, corona flicker |
| SHEAR warning | star | rising whine | blue arc, ring wobble |
| WOBBLE warning | star | irregular flutter | violet arc, star jitter |
| Pulse clear | star | bright chime + flare | ring flash, +score |
| Perfect pulse | star | choir chord | gold halo |
| Breach | star | sub-bass boom + silence | white-out |
| Burn clear | room | reactor spin-down | tally card |
| Burn failed | room | descending alarm | red tally |

## 13. Difficulty Progression & Tuning

| Level | Pressure band | Spin band (°/s) | Coherence min | Stable (s) | Surge (s) | Breach grace (s) |
|---|---|---|---|---|---|---|
| 1 | [0.40, 0.70] | [40, 120] | 0.55 | 6.0 | 2.0 | 1.5 |
| 2 | [0.42, 0.66] | [50, 130] | 0.62 | 6.0 | 2.0 | 1.4 |
| 3 | [0.45, 0.62] | [60, 140] | 0.70 | 5.5 | 1.8 | 1.3 |
| 4 | [0.47, 0.60] | [70, 150] | 0.78 | 5.0 | 1.6 | 1.2 |
| 5 | [0.48, 0.58] | [80, 160] | 0.85 | 4.5 | 1.4 | 1.0 |

- **Adaptive rule** (`FUSION_ADAPT`): after each burn, if breaches = 0 and average M > 0.75, level +1; after a failed burn, level −1 and bands widen 10 %. Clamp 1–5; one step per burn.
- **Named constants**: `R_BALL = 12 mm`, `P_RANGE = [0, 1]`, `P_FORMULA = (30 − d)/8`, `COHERENCE_LOCK = 0.70`, `FILL_RATE = 0.55/s`, `DECAY_RATE = 0.40/s`, `M_STAR = 1 − max(S)`, `ORBIT_MIN = 12 mm`, `ORBIT_MAX = 28 mm`, `GRACE = 1.5 s`.

## 14. Edge Cases & Failure Recovery

- **Prop tracking loss > 200 ms**: freeze all instability growth and the pulse clock; the star becomes a 40 % ghost. The ball is enclosed, so tracking loss is common; the 2 s reacquire window is generous on purpose.
- **Hand tracking loss**: one palm lost → containment is "half", pressure is held at its last value, and instability still grows at 50 % rate for 2 s, then pauses. Both palms lost > 300 ms → auto-suspend.
- **Ball dropped**: `d > 45 mm` → GRACE; if not re-sandwiched in 1.5 s, the star breaches (intentional — dropping the core is a fail). At level 1–2 the grace is 1.5 s; at level 5 it is 1.0 s.
- **Palms too close**: `d < 20 mm` (over-squeeze) is out-of-band high; `S_flux` grows at 0.35/s, and the game shows "ease off" rather than a hard block.
- **Orbit radius too small**: `ρ < 12 mm` makes Ω unreliable; the game holds the last trustworthy Ω for up to 300 ms, then treats Ω as 0 (out-of-band).
- **Phase wrap at ±180°**: `Δφ` is wrapped to [−180°, 180°] before `cos`, so a phase crossing does not spike coherence.
- **One-handed cheating**: if only one palm contacts the ball, pressure is capped at 0.35 and coherence is forced to 0 — the game cannot be played one-handed.
- **Tilted palm axis**: if `â` deviates > 35° from horizontal for > 500 ms, the rings tilt but the game continues; no penalty, just a visual warning.
- **Recovery UX**: one icon (ball / palms / present ball) plus one line of text; no modal dialogs mid-burn.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PalmRig` — tracks `c_L`, `c_R`, palm normals, separation `d`, orbit phases.
  - `ContainmentModel` — pressure, spin, coherence, instability severities.
  - `PulseDirector` — ignition/stable/surge timing, band tables, pulse scoring.
  - `ReactorModel` — containment M, output ramp, breach state.
  - `StarRig` — corona, rings, brightness/radius from M.
  - `ArcHUD` — three band gauges, colors, lock indicators.
  - `BurnDirector` — burns, adaptive level, session scoring.
- **Event flow**: `HeadsetTracker → PalmRig → ContainmentModel → ReactorModel`; `PulseDirector → ReactorModel → EventBus {PulseClear, PerfectPulse, Warning, Breach, BurnClear} → StarRig / ArcHUD / VFXPool / AudioBank / ScoreKeeper`. `StarRig` reads M and EventBus only.
- **Pseudocode — containment update (per tick)**:

```text
p   = clamp((30 - d) / 8, 0, 1)
a   = normalize(c_R - c_L)
(u,v) = planeBasis(a)
phiL = atan2(dot(o_L, v), dot(o_L, u))
phiR = atan2(dot(o_R, v), dot(o_R, u))
C    = cos(wrapPi(phiL - phiR))
Omega = 0.5 * (dphiL + dphiR) / dt          # filtered 4 Hz

S_flux   += (p < p_lo or p > p_hi) ? 0.55*dt : -0.40*dt
S_shear  += (|Omega| < O_lo or |Omega| > O_hi) ? 0.55*dt : -0.40*dt
S_wobble += (C < C_min) ? 0.50*dt : -0.40*dt
for S in {S_flux, S_shear, S_wobble}: S = clamp(S, 0, 1)
M = 1 - max(S_flux, S_shear, S_wobble)
if max(S) >= 1: breach()
```

- **Pseudocode — pulse scoring (on pulse end)**:

```text
inBand = fraction of pulse window where p, |Omega|, C all in band
M_end  = M at pulse end
if M_end <= 0: breach()
score(150 * M_end)
if inBand >= 0.95: score(250); emit PerfectPulse; perfectStreak += 1
else: perfectStreak = 0
if perfectStreak >= 3: widenBands(0.15, pulses = 1)
```

## 16. Acceptance Test Checklist

1. Registration fixes r = 12 mm and computes `â` within 3° of the palm-normal axis in 20/20 presentations.
2. Pressure maps d = 30 → 0.00, d = 26 → 0.50, d = 22 → 1.00 within ±0.02.
3. A 20 mm circular palm orbit at 1 Hz yields `|Ω| = 360 °/s` ± 10 % after filtering.
4. Phase-locked orbits (Δφ = 0) give C = 1.00 ± 0.02; opposed orbits give C = −1.00 ± 0.02.
5. A phase crossing at ±180° does not spike C (cos continuity verified over 100 wraps).
6. Each instability grows at 0.55/s out of band and decays at 0.40/s in band, verified to ±0.01.
7. M = 1 − max(S) in every tick of a 60 s trace.
8. Separating the palms to 50 mm enters GRACE and breaches in exactly 1.5 s unless re-sandwiched.
9. One-handed contact caps pressure at 0.35 and forces C = 0.
10. Losing one palm halves instability growth for 2 s, then pauses; relock resumes with S preserved.
11. A level-3 stable window of 5.5 s at M > 0.7 is achievable in 5/5 scripted runs with the specified bands.
12. Three consecutive perfect pulses widen bands by exactly 15 % for exactly one pulse.
13. Covering the hands for 1 s freezes S and the pulse clock; relock blends in ≤ 150 ms.
14. All 12 audio cues fire within 50 ms of their EventBus timestamps over a 60 s log.
