# Game 08 — Trilobite March — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Trilobite March |
| Slug | `trilobite-march` |
| Physical prop | Computer mouse, 110 × 62 × 38 mm, 95 g, stationary on a table; the dorsal dome is the drum skin |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate & Oblate Spheroids |
| Scale class | Tabletop, three-finger percussion on a fixed prop |
| Source concept | An armored trilobite marches through ocean mud driven by alternating finger gallops; both side buttons curl it into an armor ball when leviathans pass |
| Source physical input | Drum index, middle, and ring fingertips across the curved mouse back in gallop triplets; squeeze both flanks to curl |
| Session shape | 5 migrations, 45–70 s each, ≈5 min |
| Primary skill | Asymmetric triple-tap rhythm and a bimanual curl interrupt |

## 1. Fantasy & Core Loop

**Fantasy.** The mouse's back is a trilobite carapace. Each gallop triplet drives a wave of twenty-two pairs of legs through black mud; the body surges forward while the mud scrolls beneath it. When a leviathan's shadow crosses the sun, the only survival is to clamp both flanks and roll into a ball until the shadow passes.

**Core loop** (migration 45–70 s; session 5 migrations):

1. **Settle** (0.6 s) — the trilobite unrolls; three drum zones glow on the dome.
2. **Gallop** (continuous) — play triplets of 3 taps in 55–150 ms intervals, with one finger repeated (`I-I-M`, `I-I-R`, `M-M-I`, `M-M-R`, `R-R-I`, `R-R-M`, and the mirrored forms).
3. **Advance** — each valid gallop = one 24 mm stride; cadence × rhythm quality = speed.
4. **Read the shadow** — 1.2 s telegraph: a leviathan shadow grows on the mud and a rumble pans overhead.
5. **Curl** — pinch both flank pads for ≥ 0.6 s of the 0.9 s sweep; taps are ignored while curled.
6. **Recover** (0.5 s) — uncurl, resume the gallop; a stumble costs speed, a missed curl costs a carapace plate.
7. **Migrate** — reach `L_mig` (3,200 mm at level 3); checkpoint every quarter of the route; 3 s breath; next migration.

The tension is that the curl interrupt arrives on a fixed clock while the gallop rhythm wants to continue; stopping the hands and squeezing both flanks is a deliberate gross-motor break from a fine-motor loop.

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

- **Exact spheroid**: `a = b = 31.0 mm`, `c = 19.0 mm`, oblate (`c/a = 0.613`), fitted to the dorsal dome. `m = 95 g` is the whole mouse; the dome's virtual mass is irrelevant because it never moves. P origin = dome geometric center, 19 mm above the table when the mouse rests; `+ẑ_P` = dome apex normal (up); poles at `(0, 0, ±19.0) mm`; equator = circle of radius 31 mm in the `x̂_P`–`ŷ_P` plane. `+x̂_P` = mouse front (buttons, away from the player); `+ŷ_P` = player's right.
- **Local rolling model**: `ω = (n̂ × v_t)/r_eff`, used only to reject false taps. At the apex the meridional radius is `r_eff = a²/c = 50.6 mm`; at the equator it is `c²/a = 11.6 mm`; the branch is chosen by `|φ|` (apex branch for `|φ| > 50°`). A tap whose tangential drag produces `|ω| > 6 rad/s` is reclassified as a stroke, not a drum hit.
- **V frame**: anchored to **T**. Origin = the mouse's dome apex projected on the table at registration; `+ŷ_T` = table normal; `+x̂_T` = mouse front direction (so the trilobite marches along `+x̂_T`); `+ẑ_T = x̂_T × ŷ_T`. The mouse is static, so `P` and `T` differ only by the 19 mm vertical offset and a fixed yaw.
- **Deviations**: (a) the real mouse is 110 mm long but the fitted spheroid covers only the 62 mm dome, so `COL_body` is a 110 × 62 mm capsule for hand clearance and the march body; (b) the side buttons are real mechanics but are read as two virtual flank pads under fingertip contact, not by radio or Bluetooth; (c) the shell is textured, so contact normals come from the analytic spheroid, never from the scene mesh; (d) the scroll wheel and primary buttons are inert in gameplay.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated; mouse on a 300 × 300 mm mat, 260–320 mm in front of the sternum, long axis pointing away; forearm supported.
- **Play volume**: a 140 mm wide × 180 mm deep region above the mouse, enough for three fingertips to drum without lifting the forearm.
- **Drum zones**: index at `x_P = +16 mm`, middle at `0 mm`, ring at `−16 mm`; all at `y_P = 0`; dome height at those stations is 16.3, 19.0, 16.3 mm above the dome center.
- **Flank pads**: left at `(0, −31, 0)`, right at `(0, +31, 0)`, each a 16 × 10 mm ellipse; a thumb can reach the left pad and the index the right pad with the hand spanning 62 mm.
- **Forces**: tap impulse 0.015–0.06 N·s over 40–110 ms (peak 1.0–2.5 N); the shell is rigid, so there is no travel and no impact discomfort at 3–4 Hz.
- **Session**: 5 migrations, hard stop 8 min; 20 s rest between migrations.
- **Fatigue**: triplets at 2.5–3.5 Hz use finger flexors only; the wrist stays planted; a 20 s rest every 60 s is enforced by the migration structure.
- **Visual ergonomics**: the mud plane and trilobite are drawn at table level and 20–40 mm above, so the gaze stays 30–40° down; the leviathan shadow is drawn on the mud, never overhead in the player's central view.

## 4. Prop Geometry & Registration

- **Real dimensions**: 110 × 62 × 38 mm, 95 g; tolerance ±1.0 mm length, ±0.6 mm width, ±2 g. The dome fit uses `a = b = 31.0 mm`, `c = 19.0 mm`.
- **Dome volume**: `(4/3)πa²c = 76,500 mm³` (a bounding shape, not the mouse's mass distribution).
- **Surface atlas**: 3 drum zones (`D_I`, `D_M`, `D_R`) as 18 mm discs; 2 flank pads (`P_L`, `P_R`) as 16 × 10 mm ellipses; a 6 mm guard band around each pad; the rest of the dome is inert.
- **Colliders**: `COL_dome` = fitted spheroid; `COL_body` = capsule along `x_P` from −55 to +55 mm, radius 31 mm (hand clearance, march body); `COL_table` = plane at `y_T = 0`; `COL_pad_L`, `COL_pad_R` = flank ellipsoids protruding 2 mm.
- **Registration**: (1) mouse placed with its front away from the player and held still for 0.8 s; (2) the system fits the dome over 12 frames, accepting center error < 0.8 mm and axis error < 4°; (3) the front direction is resolved from the button-end silhouette (the mouse is asymmetric); (4) `P` publishes at 72 Hz, predicted to 90 Hz.
- **Re-acquisition**: search 60 mm around the last pose; refit the dome and the body capsule; if the mouse has been rotated > 25°, re-anchor `+x̂_P` to the new front and warn "mouse turned" because the march direction changes.
- **Tolerances**: a 1 mm dome-height error changes the tap normal by 3° at `x = 16 mm`, inside the 30° acceptance cone; no per-session calibration of the drum zones.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Drum index | `R_index_tip` | `D_I` | tap event, finger = I |
| I5.2 | Drum middle | `R_middle_tip` | `D_M` | tap event, finger = M |
| I5.3 | Drum ring | `R_ring_tip` | `D_R` | tap event, finger = R |
| I5.4 | Gallop | 3 taps in 420 ms | any 2 zones | stride, speed += 24 mm × q |
| I5.5 | Curl | any tip on `P_L` + any tip on `P_R` | both pads | curl state, holds while contact ≥ 120 ms |
| I5.6 | Uncurl | both pads released | — | resume, 0.5 s recovery |
| I5.7 | Stumble | 3 distinct fingers or interval > 200 ms | — | speed ×0.4 for 0.6 s |
| I5.8 | Re-seat | mouse lifted and replaced | `COL_table` | checkpoint reset prompt |

Per-finger roles: index/middle/ring of the dominant hand are the drummers; thumb is available for the left flank; the non-dominant hand is optional for the right flank. Per-hand roles: dominant = DRUM + left-flank curl; either hand = right-flank curl. Curl is a two-contact AND over pads, not a specific finger pair, so left- and right-handed players are symmetric.

## 6. Input Kinematics & Signal Processing

- **Tap detection**: contact with `D_x` for 40–110 ms, impulse 0.015–0.06 N·s, contact normal within 30° of the analytic surface normal, and peak normal force 1.0–2.5 N. A contact longer than 110 ms is a press, not a tap, and is ignored.
- **Tap timestamp**: the time of peak force, not the contact onset, because onset jitter is ±18 ms and peak jitter is ±7 ms. Timestamps are corrected by the measured 22 ms audio-output latency for cue alignment.
- **Gallop window**: 3 taps with `t3 − t1 ∈ [110, 420] ms` and both intervals in [55, 150] ms. Pattern is the ordered finger triple; valid iff exactly one adjacent pair repeats (`f1 = f2` or `f2 = f3`) and the third differs. This yields 12 valid patterns.
- **Rhythm quality**: target interval 90 ms. `q_i = clamp(1 − |Δt_i − 90|/90, 0, 1)`; gallop `q = (q_1 + q_2)/2`. A perfect 90/90 ms gallop scores 1.0; a 55/125 ms gallop scores 0.61.
- **Stride and speed**: one gallop = 24 mm of virtual advance, applied at the third tap. `v_target = 24·cadence·q̄`, capped at 96 mm/s. `v` approaches `v_target` with `τ = 0.25 s`.
- **Mud drag**: if no gallop for 0.8 s, `v ← v·e^(−dt/0.35)`; if no gallop for 1.6 s, sink rate `+0.25/s`.
- **Sink meter**: 0–1; at 1 the trilobite is stuck, loses 1 plate, and restarts at the last checkpoint after a 2 s dig-out. Any valid gallop reduces sink by 0.35.
- **Curl**: both pads contacted simultaneously for ≥ 120 ms → CURLED. While curled, drum taps are logged but produce no stride; `v = 0`; the ball radius is 34 mm. Uncurl requires both pads released for ≥ 80 ms, then a 0.5 s recovery during which speed ramps from 0.
- **Curl window**: a leviathan sweep is protected if the CURLED state overlaps ≥ 0.6 s of the 0.9 s sweep. Entering curl 0.3 s before the sweep and holding through it always qualifies.
- **Signal chain**: joints → 3-tap median → one-euro filter (1.5 Hz min cutoff, β = 0.03) → contact → peak-force timestamp. Tap classification runs on the contact edge, not per frame, to avoid double counts.
- **Latency**: 14 ms tracking + 11 ms physics + 11 ms render + 22 ms audio = 58 ms; the gallop window is 110–420 ms, so 58 ms is well inside tolerance.

## 7. Virtual World Layout & Coordinate Mapping

- **T axes**: origin at the dome apex on the table; `+ŷ_T` = up; `+x̂_T` = mouse front; `+ẑ_T` = player's right.
- **Mud plane**: a 400 × 400 mm virtual plane at `y_T = 0` (the table), rendered as a dark parallax-mapped surface with a 2 mm relief. Texture offset scrolls by `−s` along `+x̂_T`, where `s` is the trilobite's virtual advance, giving the illusion of motion under a static prop.
- **Trilobite body**: 128 mm long × 74 mm wide × 34 mm high, drawn around the mouse; the carapace is a translucent shell over the real dome so the player sees their fingers through it. Legs: 22 pairs, 18 mm long, 3 mm thick, animated by a metachronal wave at `1.6 × cadence`.
- **March coordinate**: `s ∈ [0, L_mig]` per migration; `L_mig = 3,200 mm` at level 3 (2,400–4,000 mm across levels); checkpoints at `L_mig/4` intervals (800 mm at level 3); the finish is a glowing trench lip at `s = L_mig`.
- **Leviathan shadow**: an elliptical shadow of 260 × 120 mm, crossing the mud along `+ẑ_T` at 180 mm/s; spawn 900 ms before arrival at the player's `x_T`; the shadow darkens the mud and the trilobite's shell.
- **HUD**: a 120 mm arc above the mouse showing speed, sink, plates, and the migration progress; anchored to T, tilted 25° toward the player.
- **Mapping formulas**: `p_T = (s mod 400 − 200, 0, lateral)` for the body; mud UV `u = (x_T + s)/400`; shadow `p_T = (s + 60, 0, −260 + 180·t)`.

## 8. Gameplay Systems & Rules

- **Entities**: `Trilobite` (s, v, sink, plates, curl), `GallopClassifier` (tap buffer, patterns, quality), `MudField` (scroll, relief), `Leviathan` (telegraph, sweep, shadow), `MigrationDirector`.
- **Plates**: 3 per migration; a missed curl −1; a sink-out −1 and a checkpoint restart. At 0 plates the migration fails and restarts from the last checkpoint with 1 plate.
- **Checkpoints**: 800 mm intervals; crossing one restores 1 plate (max 3) and clears sink.
- **Speed**: `v_target = 24·cadence·q̄`; cadence is measured over the last 4 gallops; `q̄` is the mean quality. A rhythm streak of 10 valid gallops grants ×1.2 speed for 4 s.
- **Stumble**: an invalid pattern (3 distinct fingers, or an interval > 200 ms) sets `v ×0.4` for 0.6 s and breaks the streak; it does not cost a plate.
- **Leviathan cadence**: first at 12 s, then every 9–14 s (random), never within 3 s of a stumble or 2 s of a checkpoint.
- **Scoring**: distance 1 point per 10 mm; valid gallop +5; perfect gallop (q > 0.9) +8; successful curl +40; checkpoint +100; migration clear +300; no-damage migration +250.
- **Win/lose**: 5 migrations; win if ≥ 3 cleared; a failed migration can be retried once per session at the same level.
- **Assist**: if the player misses 2 consecutive curls, the telegraph extends to 1.8 s and the required curl overlap drops to 0.45 s for the next leviathan only.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, sensor test | REGISTER |
| REGISTER | mouse still 0.8 s | dome fit < 0.8 mm | set P, front axis | CALIBRATE |
| CALIBRATE | front resolved | button-end silhouette | build T, V, mud plane | BRIEF |
| BRIEF | 3 s card | — | migration 1 seed | MARCH |
| MARCH | tap on drum zone | 40–110 ms, 0.015–0.06 N·s | buffer tap | MARCH |
| MARCH | 3-tap gallop | window 110–420 ms | stride 24 mm, `v_target` update | MARCH |
| MARCH | invalid pattern | 3 distinct or > 200 ms | stumble, ×0.4 0.6 s | MARCH |
| MARCH | no gallop 1.6 s | — | sink +0.25/s | SINKING |
| SINKING | valid gallop | — | sink −0.35, resume | MARCH |
| SINKING | sink = 1 | — | plate −1, respawn checkpoint | RECOVER |
| MARCH | leviathan telegraph | — | shadow grows, rumble | MARCH |
| MARCH | sweep starts | — | 0.9 s protection check | SWEEP |
| SWEEP | curled ≥ 0.6 s | both pads held | +40, no damage | MARCH |
| SWEEP | not curled | — | plate −1, stumble | MARCH |
| MARCH | checkpoint crossed | s mod 800 | plate +1, +100 | MARCH |
| MARCH | s ≥ L_mig | — | migration clear +300 | DEBRIEF |
| RECOVER | 2 s dig-out | — | resume at checkpoint, 1 plate | MARCH |
| DEBRIEF | migration < 5 | — | 20 s rest | BRIEF |
| DEBRIEF | migration = 5 or 3 fails | — | session stats | COMPLETE |
| any | prop lost > 200 ms | — | freeze march, ghost mouse | REACQUIRE |
| REACQUIRE | relock ≤ 2.0 s | search 60 mm | keep s, plates | prior |
| REACQUIRE | 2.0 s timeout | — | "present the mouse" card | REACQUIRE |
| any | idle 25 s | — | freeze timers | SUSPENDED |
| SUSPENDED | any tap | — | 1.5 s countdown | prior |

Timers: gallop window 420 ms; stumble 0.6 s; sink threshold 1.6 s; curl 120 ms; curl window 0.6 s of 0.9 s; checkpoint 800 mm; migration rest 20 s; reacquire 2.0 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_dome` (spheroid), `COL_body` (capsule), `COL_table` (plane), `COL_pad_L/R` (ellipsoids), 3 drum discs (18 mm), 22 leg pairs (visual only, no collision), 1 leviathan shadow (analytic ellipse).
- **Pair matrix**: Finger↔Dome (contact/tap); Finger↔Pad (curl contact); Finger↔Body (reject, prevents taps on the mouse's flanks from counting as dome hits); Body↔Table (static); Shadow↔Trilobite (protection test, no force); Legs↔Mud (visual only).
- **Statics**: the mouse does not move; `COL_body` is fixed in P. No rigid-body integration for the prop.
- **Dynamics**: the trilobite is a 1-D kinematic body along `s`; legs are procedural; mud particles are 180 instanced quads with a 0.4 s lifetime; no gravity, no collisions between legs and mud.
- **CCD**: taps are contact events, not moving bodies; the fingertip moves ≤ 400 mm/s → 4.4 mm/frame, so the 5 mm contact skin plus 30 ms debounce is sufficient. No swept tests needed.
- **Materials**: mouse shell µ = 0.35 (matte plastic), skin µ = 0.45; flank pads µ = 0.6 (rubberized) so a curl grip is stable; mud is visual only.
- **Forces**: tap impulses 0.015–0.06 N·s are absorbed by the rigid shell and never move the mouse; the virtual stride is a kinematic advance of 24 mm, not a force.
- **Curl geometry**: ball radius 34 mm, centered 20 mm above the dome; the real mouse is inside the ball and fully hidden.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → mud plane → legs → carapace shell → tap ripples → leviathan shadow → curl ball → hands → HUD.
- **Occlusion cases**:
  1. *Fingers on the dome*: the real fingertips occlude the carapace; a 12 mm ripple ring is drawn around the contact point on the shell surface so the hit is visible even under the finger.
  2. *Mouse body vs legs*: the real mouse extends 24 mm beyond the dome at each end; legs behind it are depth-tested and hidden, while legs outside the silhouette are drawn. This makes the trilobite read as wrapping the mouse.
  3. *Carapace over fingers*: the shell is drawn at 35 % opacity over the hand so the player can see their own fingers; depth writes are off for the shell, and it is sorted after the hand mesh.
  4. *Leviathan shadow under the hand*: the shadow is projected on the mud and on the table, and is dimmed 50 % under the hand silhouette; it never draws over the hand.
  5. *Curl ball*: when curled, the ball is opaque and the real mouse is hidden; a 15 % ghost outline of the mouse is kept so the player does not lose the prop.
  6. *HUD vs mud*: the HUD arc is at 120 mm above the table and is depth-tested against the hand; if occluded, it fades to 30 % rather than drawing through the palm.
- **VFX budgets**: mud particles 180; tap ripple 12 per tap; leg segments 44; shadow 1; curl dust 40; checkpoint bloom 30; global cap 500 particles.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Tap | drum zone | dry carapace click, pitch per finger | ripple ring, zone flash |
| Valid gallop | body | low chitin thump + leg scuttle | stride surge, speed bar + |
| Perfect gallop (q > 0.9) | body | brighter thump, +1 semitone | gold ripple, streak +1 |
| Stumble | body | mud squelch + descending tone | shell wobble, speed bar drops |
| Sink warning | body | bubbling mud | sink meter pulses amber |
| Leviathan telegraph | overhead pan | sub-bass rumble, 1.2 s | shadow grows on mud |
| Sweep | overhead | whoosh + water displacement | shadow crosses |
| Curl | body | armor plates clack shut | ball forms, mouse ghost |
| Successful curl | body | muffled impact + relief chord | +40, plates flash |
| Failed curl | body | plate crack + roar | plate pip breaks |
| Checkpoint | trench | bronze bell | plate +1, bloom |
| Migration clear | room | tide swell + gong | tally card |

## 13. Difficulty Progression & Tuning

| Level | Migration (mm) | Leviathan gap (s) | Telegraph (s) | Curl overlap (s) | Mud drag | Plate loss | Speed cap (mm/s) |
|---|---|---|---|---|---|---|---|
| 1 | 2400 | 14–18 | 1.6 | 0.45 | 0.8× | 1 | 80 |
| 2 | 2800 | 12–16 | 1.4 | 0.50 | 0.9× | 1 | 88 |
| 3 | 3200 | 10–14 | 1.2 | 0.60 | 1.0× | 1 | 96 |
| 4 | 3600 | 9–13 | 1.1 | 0.65 | 1.1× | 2 | 96 |
| 5 | 4000 | 8–12 | 1.0 | 0.70 | 1.2× | 2 | 96 |

- **Adaptive rule** (`TRILOBITE_ADAPT`): after each migration, if cleared with ≥ 2 plates and < 60 s, level +1; if failed, level −1 and the leviathan gap +3 s. Clamp 1–5; one step per migration.
- **Named constants**: `A_DOME = 31.0 mm`, `C_DOME = 19.0 mm`, `STRIDE = 24 mm`, `TAP_MS = [40, 110]`, `TAP_J = [0.015, 0.06] N·s`, `GALLOP_WIN = 420 ms`, `GALLOP_GAP = [55, 150] ms`, `TARGET_GAP = 90 ms`, `CURL_HOLD = 120 ms`, `SWEEP = 900 ms`, `CURL_NEED = 600 ms`, `SINK_TIME = 1.6 s`, `MIGRATION = 3200 mm`, `CHECKPOINT = 800 mm`.

## 14. Edge Cases & Failure Recovery

- **Mouse rotated mid-run**: if the fit yaw changes > 25°, re-anchor `+x̂_P`; the march direction rotates with the mouse, so the HUD shows a "direction reset" toast and `s` is preserved. This prevents an accidental nudge from silently changing drum-zone assignment.
- **Mouse moved > 40 mm**: treated as a re-seat; pause, prompt to return the mouse, and resume at the same `s` if it returns within 10 s.
- **Hand tracking loss**: if the drum hand is lost, taps stop and the trilobite coasts/sinks; the non-dominant hand can still curl. If both hands are lost > 1.0 s, pause.
- **Curl with one pad only**: not a curl; a single-pad contact is logged as a brace and does not protect. The HUD shows the missing pad in red.
- **Tap on the body capsule**: rejected by the `COL_body` pair matrix, so a tap on the mouse's long tail does not count as a drum hit; a small grey ripple confirms the rejection.
- **Tap during curl**: ignored for stride but buffered for 200 ms; on uncurl, a buffered valid gallop is applied once so the player is not punished for an early tap.
- **Double-count**: one physical tap produces one contact edge; the 30 ms debounce merges contacts that re-open within 30 ms; a second tap within 55 ms of the first is treated as the same gallop position and does not create a 2-tap pattern.
- **Leviathan while sinking**: sinking pauses; the shadow still sweeps, but a failed curl during a sink does not cost a second plate (only the sink penalty applies).
- **Plate overflow**: checkpoints restore to max 3; excess is converted to +50 score.
- **Recovery UX**: one icon (mouse / hand / both) and one line of text; no modal dialogs during a migration.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropMouse` — pose P, dome fit, front axis, pad ellipsoids, body capsule.
  - `TapDetector` — contact edge, peak-force timestamp, impulse, normal cone, per-finger buffer.
  - `GallopClassifier` — 3-tap window, 12 valid patterns, interval quality, stumble.
  - `GaitModel` — cadence, q̄, speed, sink, stride advance, streak.
  - `CurlModel` — two-pad AND, hold timer, protection window, ball state.
  - `LeviathanDirector` — gap scheduler, telegraph, sweep, protection test.
  - `MigrationDirector` — checkpoints, plates, scoring, adaptive level.
  - `TrilobiteRig` — carapace, 44 leg segments, mud scroll, shadow, curl ball.
- **Event flow**: `HandTracker → TapDetector → GallopClassifier → GaitModel`; `HandTracker → CurlModel`; `LeviathanDirector → CurlModel` (protection query); `GaitModel/LeviathanDirector → EventBus {Tap, Gallop, Stumble, Sink, Curl, CurlSuccess, PlateLost, Checkpoint, MigrationClear}` → `AudioBank / VFXPool / ScoreKeeper / HUD`.
- **Pseudocode — gallop classification (on tap edge)**:

```text
on tap(finger, t_peak, J, normal_err):
    if J < 0.015 or J > 0.06 or normal_err > 30deg: return
    buf.push({finger, t_peak})
    buf = buf where t_peak > now - 420ms
    if buf.length < 3: return
    a, b, c = buf[-3], buf[-2], buf[-1]
    if not (110ms <= c.t - a.t <= 420ms): return
    dt1 = b.t - a.t; dt2 = c.t - a.t - dt1
    if not (55ms <= dt1 <= 150ms and 55ms <= dt2 <= 150ms): stumble(); return
    repeated = (a.f == b.f) xor (b.f == c.f)
    distinct_third = (a.f == b.f and c.f not in {a.f}) or (b.f == c.f and a.f not in {b.f})
    if not (repeated and distinct_third): stumble(); return
    q = (clamp(1 - abs(dt1 - 90ms)/90ms, 0, 1) + clamp(1 - abs(dt2 - 90ms)/90ms, 0, 1)) / 2
    gallop(q, c.t)
    buf.clear()
```

- **Pseudocode — gait + leviathan (per tick)**:

```text
dt = 0.0111
if curl.active:
    v = 0
else:
    v_target = min(24 * cadence * q_mean * streak_mult, 96)
    v += (v_target - v) * dt / 0.25
if now - last_gallop > 1.6s: sink += 0.25 * dt else sink = max(0, sink - 0.35 * on_gallop)
s += v * dt
if s >= next_checkpoint: plates = min(3, plates + 1); score += 100; next_checkpoint += 800
if leviathan.state == SWEEP:
    protected = curl.overlap(leviathan.sweep_start, leviathan.sweep_end) >= 0.6s
    if not protected: plates -= 1; stumble(); leviathan.state = IDLE
if sink >= 1: plates -= 1; respawn(last_checkpoint); sink = 0
if plates <= 0: fail_migration()
if s >= migration_length: clear_migration()
```

## 16. Acceptance Test Checklist

1. Registration fits the dome center to < 0.8 mm and the axis to < 4° in 20/20 presentations; the front axis is correct in 10/10.
2. A tap of 0.03 N·s over 70 ms on `D_M` is classified as finger M; a 150 ms press is not a tap; a tap 15 mm outside the zone is rejected.
3. A 90/90 ms `I-I-M` triplet is classified as a valid gallop with q = 1.0 ± 0.05; a 55/125 ms triplet yields q = 0.61 ± 0.05.
4. Three distinct fingers (`I-M-R`) produce a stumble, not a gallop, in 10/10 trials.
5. A valid gallop advances exactly 24 mm and updates speed toward `24·cadence·q̄` with τ = 0.25 s ± 0.03 s.
6. Cadence 3.0 Hz with q = 0.9 yields 64.8 mm/s; 10 consecutive gallops trigger the ×1.2 streak for 4.0 s ± 0.1 s.
7. No gallop for 1.6 s starts sinking; a valid gallop reduces sink by 0.35 and stops the sink timer.
8. Two-pad contact held 120 ms enters CURLED; one pad does not, in 10/10 trials.
9. A curl overlapping 0.60 s of a 0.90 s sweep prevents plate loss; 0.55 s does not.
10. A missed curl costs exactly 1 plate and applies a stumble; a curl during a sink costs no extra plate.
11. A checkpoint at 800 mm restores 1 plate (max 3) and awards +100; crossing at 3 plates converts the excess to +50.
12. Covering the mouse for 1.5 s freezes the march and preserves s, plates, and sink; relock resumes within 200 ms.
13. A level-3 migration (3,200 mm, 10–14 s leviathan gap) is clearable with ≥ 2 plates in 5/5 scripted runs under 70 s.
14. The full 5-migration session completes in ≤ 8 min and the particle count never exceeds 500.
