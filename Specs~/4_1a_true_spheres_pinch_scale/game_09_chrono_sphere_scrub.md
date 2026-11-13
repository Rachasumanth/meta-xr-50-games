# Game 09 — Chrono-Sphere Scrub — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Chrono-Sphere Scrub |
| Slug | `chrono-sphere-scrub` |
| Physical prop | Glass marble, 16 mm diameter, ~5 g, held between the left thumb and middle finger and twisted with the right thumb and index |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.1a True Spheres — Pinch-Scale (1–4 cm) |
| Scale class | Pinch-scale two-hand, free-space, rotational time-scrub |
| Source concept | "A miniature frozen explosion diorama hovers 15cm in front of the marble. Twisting the marble clockwise scrubs time forward; twisting counter-clockwise rewinds time—find the exact millisecond where a virtual laser deflects off falling debris to hit the boss." |
| Source physical input | Hold the marble securely between your left thumb and middle finger and twist it clockwise or counter-clockwise using your right thumb and index finger. |

## 1. Fantasy & Core Loop

**Fantasy.** The marble is a chrono-core: a glass sphere with a captured instant frozen inside it. Held between two fingers, it can be twisted like a dial, and every degree of rotation drags a frozen explosion forward or backward through time. A diorama hangs in the air — debris suspended mid-burst, a laser caught as a bright thread, a boss frozen in its death-frame. Your job is to find the one instant where the thread bends off a tumbling fragment and lands in the boss's weak point.

**Core loop** (one scenario ≈ 30–55 s; one session ≈ 4–6 min):

1. **Hold the chrono-core** (continuous) — left thumb + middle finger pinch the marble; right thumb + index grip the equator.
2. **Scrub** (0.2–1.5 s bursts) — twist clockwise to advance time, counter-clockwise to rewind; 360° = 1.000 s of scene time.
3. **Read the diorama** (continuous) — debris, beam, and boss move along deterministic trajectories; proximity to the solution is cued by a rising harmonic and beam glints.
4. **Fine-tune** (0.3–1.0 s) — small twists at ±30°/s for ±0.08 s precision.
5. **Lock** (0.6 s) — hold the marble still; the timeline commits at the current T.
6. **Resolve** (0.8 s) — if T is inside the resonance window, the laser deflects and the boss dies; otherwise the diorama resets with a delta readout.
7. **Scenario tally** (2.5 s) — up to 3 attempts per scenario; score by precision and attempts.

The physical twist is the only control. Fast scrubbing overshoots; the skill is decelerating into the target window.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `L_middle_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the center, poles at `±ẑ_P`, equator in `x̂_P`–`ŷ_P`, no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]`, longitude `λ ∈ [0°, 360°)`; `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; normal `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P frame**: origin = marble center; `+ẑ_P` = the **hold axis**, defined as the line through the left thumb and middle-finger contact points (the axis about which the right hand twists the marble); `+x̂_P` = the diorama direction (toward the player's forward at calibration); `+ŷ_P` completes the frame.
- **Numbers**: r = 8 mm, m = 5 g, glass, µ_glass-skin ≈ 0.30.
- **Hold axis**: `â = normalize(p_Lthumb − p_Lmiddle)`; the left pinch must be near-opposed (angle between the two contact normals > 150°) for the axis to be stable.
- **Twist extraction**: the scrub rate is the component of the marble's angular velocity about `â`: `ω_s = ω · â`. Clockwise (viewed from the right-hand side along `+â`) is positive and advances time; counter-clockwise rewinds.
- **V frame**: anchored to **W**; the diorama hangs 150 mm in front of the marble center at the calibration pose, and stays world-fixed while the marble twists. The marble itself is P-anchored; only its rotation feeds the timeline.
- **Deviations**: (a) a true sphere has no features, so the *twist* is measured from the right-hand contact motion, not from visual features; (b) the left pinch is a two-point hold, so the axis is estimated and low-pass filtered rather than read from a keyed feature; (c) the diorama is deterministic — every scrub to the same T produces the exact same frame, which is what makes the puzzle fair.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated; left hand at 250–320 mm in front of the sternum, palm facing right, thumb and middle finger forming a two-point pinch; right hand above with thumb and index on the marble's equator.
- **Support surface**: none; calibration uses a table to establish the initial forward diorama direction.
- **Play volume**: a 300 mm cube around the marble; the diorama occupies a 220 mm sphere at +150 mm forward.
- **Handedness**: mirrored — right hand pinches, left hand twists.
- **Reach envelope**: fingertips 180–330 mm from the shoulders; the twist is a ±45° wrist rotation, repeated.
- **Session length**: 6 min default (5 scenarios), hard stop 9 min.
- **Fatigue mitigations**: (1) the left pinch is isometric and light (5 g); (2) scrub bursts are short, with 1 s of read time between; (3) the right thumb/index alternate as the twisting pair; (4) a 5 s "temporal rest" between scenarios.

## 4. Prop Geometry & Registration

- **Real dimensions**: diameter 16.0 mm (r = 8.0 mm), mass 5.0 g, glass, µ ≈ 0.30.
- **Sphere model**: `|p| = 8` mm; no features. The game adds a virtual chrono-dial decal on the equator (24 tick marks), which rotates with `P` and gives the player a visual sense of twist.
- **Colliders**: `COL_sphere` r = 8 mm; `COL_holdL` = two 4 mm spheres at the left thumb and middle contacts; `COL_twistR` = a torus band at the equator for the right thumb/index; `COL_dial` = the virtual tick ring (visual only, no collision).
- **Registration**: (1) player holds the marble in the two-point left pinch and rests the right thumb/index on the equator; (2) system fits the sphere center (error < 0.6 mm) and the hold axis `â` from the left contacts; (3) the diorama direction `+x̂_P` is set to the player's forward at a "look here" calibration tap; (4) `P` published at 72 Hz; (5) the hold axis is re-estimated every 200 ms and low-passed at 3 Hz — if it moves more than 15°, the scrub rate is re-projected to avoid a false time jump.
- **Re-acquisition**: search 60 mm around the last pose; re-fit the sphere center; orientation is preserved by integration; blend over 150 ms. A "present the marble" card after 1.5 s.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Two-point hold | `L_thumb_tip` + `L_middle_tip` | `COL_holdL` | hold valid; defines `â` |
| I5.2 | Equator grip | `R_thumb_tip` + `R_index_tip` | `COL_twistR` | twist authority |
| I5.3 | Scrub forward | right-hand pair rotates CW about `â` | equator band | `ω_s > 0`, time advances |
| I5.4 | Scrub backward | CCW rotation | equator band | `ω_s < 0`, time rewinds |
| I5.5 | Fine scrub | small ±15° twists | equator | ±0.04 s per 15° |
| I5.6 | Lock | both hands still 600 ms | — | commit current T |
| I5.7 | Release hold | left pinch opens | — | scrub freezes, timeline holds |
| I5.8 | Re-grip | left pinch closes | — | resume, no time jump |

Per-finger roles: left thumb + middle are the HOLD; right thumb + index are the TWIST. The left index/ring/pinky are ignored. Per-hand roles: hold vs twist, not mirrored during a scenario.

## 6. Input Kinematics & Signal Processing

- **Angular velocity**: from the right-hand pair's rotation about `â`: `ω = (q_now · q_prev⁻¹)` converted to an axis-angle, projected onto `â`; `ω_s = ω·â`. Filter: 3-tap median → IIR `f_c = 10 Hz`.
- **Scrub mapping**: `dT/dt = ω_s / 360°` per second, i.e. one full revolution = 1.000 s of scene time. Clamp `|ω_s| ≤ 540 °/s` (1.5 s of scene time per second) to keep the motion readable.
- **Time domain**: `T ∈ [0, 8.000] s`, stored at 1 ms resolution. At the ends, `T` clamps with a 0.15 s elastic overshoot and a soft "end of tape" cue.
- **Hold-axis stability**: `â` is re-estimated every 200 ms; if it rotates > 15°, `ω_s` is re-projected and a 120 ms grace prevents a spurious jump.
- **Lock validation**: `|ω_s| < 20 °/s` and `|dâ/dt| < 10 °/s` for 600 ms; the committed time is the median of the last 12 samples (133 ms) to reject jitter.
- **Resonance window**: each scenario has a hidden `t*` and a window half-width `w` (20–60 ms by level). Success if `|T_lock − t*| ≤ w`.
- **Proximity cue**: `Δ = |T − t*|`; the harmonic cue pitch = `220 + 880·exp(−Δ/0.5)` Hz; beam glints appear when `Δ < 0.25 s`; a "ghost frame" overlay appears when `Δ < 0.08 s`.
- **Attempts**: 3 per scenario; each failure shows `Δ` and the sign (early/late) and resets `T` to `t* ± 1.2 s` (random side) for the next attempt.
- **Overshoot model**: there is no inertia on the timeline; the marble's rotation maps directly. This is deliberate — the difficulty is human motor precision, not simulated momentum.

## 7. Virtual World Layout & Coordinate Mapping

- **V frame**: anchored to W; origin `O_V` = marble center + 150 mm along the calibrated forward `x̂_V`; `ŷ_V` = right, `ẑ_V` = up.
- **Diorama**: a 220 mm sphere of frozen debris around `O_V`; the boss anchor at `O_V + (0, 60, 30) mm`; the laser emitter at `O_V + (0, −90, 60) mm`.
- **Debris**: 12 fragments with precomputed trajectories `p_i(T)` (ballistic + one floor bounce at `T_i`), rendered with motion streaks that lengthen with `|ω_s|`.
- **Laser**: emitted at `T = 0`, travels at 1200 mm/s along a fixed direction; it strikes fragment #7 at `T ≈ 3.4 s` only if the fragment's position at that instant matches the beam line; the deflection is deterministic and the resulting secondary beam hits the boss only for `T ∈ [t* − w, t* + w]`.
- **Boss**: a 70 mm armored figure with a 16 mm weak point; on a successful lock it plays a 0.8 s death animation at normal speed.
- **Timeline ribbon**: a 200 mm horizontal ribbon below the diorama showing `T` and the scrub direction; the resonance window is **not** shown (the player must find it), but a proximity glow is.
- **Mapping formulas**: fragment world position `p_W = O_V + p_i(T)`; laser endpoint = ray from the emitter; the diorama never moves with the marble, only its state `T` changes.
- **Camera note**: the diorama is at a fixed world position; the marble is held 150 mm in front of it, so the player sees the marble in the foreground and the frozen explosion behind, reinforcing the "time dial" metaphor.

## 8. Gameplay Systems & Rules

- **Entities**: `ChronoCore` (T, ω_s, hold valid), `Diorama` (12 debris, beam, boss), `Scenario` (t*, w, attempts), `SessionDirector`.
- **Scrub**: clockwise advances `T`, counter-clockwise rewinds; `T` is the single state variable for the whole diorama.
- **Lock**: commits `T_lock`; success if inside the resonance window; failure otherwise. A lock during an invalid hold is rejected with a "hold the core" cue.
- **Attempts**: 3 per scenario. A success ends the scenario; three failures move to the next scenario with 0 points for this one.
- **Scoring**: success +500 × precision, where precision = `1 − |Δ|/w`; attempt 1 success +200 bonus; attempt 2 +100; attempt 3 +50; perfect (|Δ| ≤ 0.1w) +300; scenario clear time bonus +100 if < 35 s.
- **Session**: 5 scenarios; win if ≥ 4 solved. Total score is the sum; a "chronomancer" rank requires 3 first-attempt solves.
- **Difficulty**: window half-width shrinks with level; debris count and trajectory complexity rise; the proximity cue becomes subtler.
- **Combo**: two consecutive first-attempt solves → "Time Weaver" (precision ×1.2) for the next scenario.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, UI | REGISTER |
| REGISTER | sphere + hold fit | center < 0.6 mm, pinch opposed > 150° | set `â`, `+x̂_P` forward | CALIBRATE |
| CALIBRATE | diorama direction tap | — | spawn diorama at +150 mm | BRIEF |
| BRIEF | twist once | \|ω_s\| > 30 °/s | scenario card 2.0 s | SCRUB |
| SCRUB | twist | hold valid | advance/rewind T, update diorama | SCRUB |
| SCRUB | hold still 600 ms | \|ω_s\| < 20 °/s | commit T_lock | RESOLVE |
| RESOLVE | lock in window | \|Δ\| ≤ w | boss death, +score | DEBRIEF |
| RESOLVE | lock outside | attempts < 3 | show Δ, reset T | SCRUB |
| RESOLVE | attempts = 3 | — | scenario failed | DEBRIEF |
| SCRUB | hold released | — | freeze T, hold timeline | HOLD |
| HOLD | re-grip | pinch valid 150 ms | resume, T preserved | SCRUB |
| any | prop tracking loss > 150 ms | — | freeze T, ghost core | REACQUIRE |
| REACQUIRE | relock ≤ 1.5 s | search r = 60 mm | blend orientation 150 ms | prior |
| REACQUIRE | 1.5 s timeout | — | "present the marble" card | REACQUIRE |
| DEBRIEF | scenario < 5 | — | next scenario seed | BRIEF |
| DEBRIEF | scenario = 5 or 4 solved | — | session stats | COMPLETE |
| any | idle 20 s / system pause | — | freeze T | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: lock hold 600 ms; lock median 133 ms; end-of-tape overshoot 150 ms; hold-axis regrace 120 ms; scenario time bonus threshold 35 s; reacquire 1.5 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_sphere` r = 8 mm, `COL_holdL` two spheres r = 4 mm, `COL_twistR` torus band, 12 debris spheres (r = 6–14 mm, visual/temporal only), laser ray (analytic), boss capsule (r = 35 mm) and weak point sphere (r = 8 mm).
- **Pair matrix**: Hand↔Sphere (contacts only), Laser↔Debris (deterministic at fixed T), Laser↔Boss (deterministic), Debris↔nothing (they are a recording, not a live sim), Hand↔Diorama (none).
- **Material values**: none for the diorama — it is a precomputed trajectory sampled at T; the marble is the only physical body, and it is fully constrained by the two pinches.
- **Physical constants**: m = 5 g; r = 8 mm; µ = 0.30; `I = 2mr²/5 = 1.28 × 10⁻⁷ kg·m²`.
- **CCD**: the laser is analytic; no swept collision is needed. The timeline is sampled, not integrated, so there is no numerical drift.
- **Force formulas**: none. The scrub is a direct kinematic mapping; the only "force" is the human wrist torque, which the game neither measures nor resists.
- **Determinism requirement**: `p_i(T)` and the laser path must be pure functions of T, seeded per scenario; the same T must always produce the same frame (this is asserted in tests).

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → diorama backdrop → debris → laser → boss → marble overlay → VFX → hands → HUD.
- **Occlusion cases**:
  1. *Marble in front of the diorama*: the marble is 150 mm nearer than the diorama and correctly occludes it; the chrono-dial decal on the near hemisphere is fully visible.
  2. *Fingers over the marble*: the left thumb/middle and right thumb/index cover up to 40 % of the sphere; the dial ticks are drawn on the visible hemisphere only, and a floating mini-dial is drawn above the hand as a fallback.
  3. *Debris behind the marble*: a fragment passing behind the marble is occluded; a thin outline is drawn on the marble's silhouette so the player can track the fragment's position.
  4. *Laser through the hands*: the primary laser is emitted behind the hands and passes beside the marble; where it would cross a finger, it is depth-tested and dimmed, never hidden entirely.
  5. *Boss death animation*: the explosion VFX is exempt from occlusion for 0.8 s so the payoff is never blocked by the player's hands.
- **VFX budgets**: debris streak 6/fragment, laser glint 8, proximity shimmer 10/s, lock flash 20, boss death 60, time-ribbon tick 4, global cap 420.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Hold valid | marble | soft gyro lock | dial ticks light up |
| Scrub forward | marble | rising tape-spool whir | debris streaks forward |
| Scrub backward | marble | reversed tape whir | streaks reverse |
| Fine scrub | marble | tick per 5° | dial needle steps |
| Proximity | diorama | harmonic rises with closeness | beam glints, ghost frame |
| End of tape | diorama | tape-stop thunk | ribbon clamps |
| Lock | marble | mechanical shutter | T freezes, ribbon brightens |
| Success | boss | laser crack + boss roar | deflection, death anim |
| Failure (early) | boss | dull miss + low tone | blue "EARLY" delta |
| Failure (late) | boss | dull miss + high tone | amber "LATE" delta |
| Scenario clear | diorama | clockwork fanfare | tally card |
| Scenario failed | diorama | winding-down whir | grey tally |

## 13. Difficulty Progression & Tuning

| Level | Window half-width (ms) | Debris count | Max scrub rate (°/s) | Proximity cue | Attempts |
|---|---|---|---|---|---|
| 1 | 60 | 8 | 540 | strong (glints at 0.4 s) | 3 |
| 2 | 45 | 10 | 540 | strong | 3 |
| 3 | 35 | 12 | 480 | medium | 3 |
| 4 | 25 | 14 | 420 | subtle | 3 |
| 5 | 18 | 16 | 360 | very subtle | 2 |

- **Adaptive rule** (`CHRONO_ADAPT`): after each scenario, if solved on attempt 1, level +1; if failed, level −1 and window ×1.25. Clamp 1–5; one step per scenario.
- **Named constants**: `R_MARBLE = 8 mm`, `TIME_RANGE = [0, 8.000] s`, `DEG_PER_SECOND = 360`, `MAX_OMEGA = 540 °/s`, `LOCK_OMEGA = 20 °/s`, `LOCK_HOLD = 600 ms`, `LOCK_MEDIAN = 133 ms`, `PROX_EXP = 0.5 s`, `GHOST_DELTA = 0.08 s`.

## 14. Edge Cases & Failure Recovery

- **Prop tracking loss > 150 ms**: `T` freezes instantly; the diorama holds its frame; the marble becomes a 40 % ghost. On relock, the scrub rate is re-zeroed and no phantom rotation is integrated (a lost frame can never jump time).
- **Hand tracking loss**: losing the twist hand freezes `T` but keeps the diorama visible; losing the hold hand also freezes `T` and dims the timeline; both lost > 300 ms pauses.
- **Hold-axis wobble**: if `â` moves > 15° in 200 ms, the scrub rate is re-projected and a 120 ms grace prevents a jump; if it moves > 35°, the scrub is disabled until the pinch re-stabilizes.
- **Lock during hold release**: a release inside the 600 ms lock window cancels the lock (no commit) and shows "keep holding"; no attempt is consumed.
- **T at the ends**: clamping produces a 150 ms elastic overshoot and a tape-stop cue; no attempt is consumed.
- **Both hands twist opposite directions**: the right hand's pair is the only authority; the left hand is a hold, so opposing motion is impossible to express and is ignored.
- **Scenario reset after failure**: `T` is placed `t* ± 1.2 s` on a random side, never exactly on `t*`, so the retry is not a freebie.
- **Determinism check failure**: if a sampled frame at the same T differs between renders (float drift), the engine snaps T to the 1 ms grid before sampling, guaranteeing reproducibility.
- **Recovery UX**: one icon (marble / pinch / present marble) plus one line of text; no modal dialogs mid-scenario.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropMarble` — owns `poseP`, sphere center, hold axis `â`.
  - `PinchModel` — left thumb/middle opposition, right equator grip, validity.
  - `TwistIntegrator` — angular velocity projection, filter, scrub rate.
  - `Timeline` — `T` in ms, clamping, elastic overshoot, median lock.
  - `DioramaModel` — pure `sample(T)` returning 12 fragment poses, laser path, boss state.
  - `ResonanceJudge` — hidden `t*`, window `w`, proximity, success.
  - `ScenarioDirector` — 5 scenarios, attempts, level table, scoring.
  - `DioramaRig` — debris, laser, boss render from `sample(T)`.
  - `ChronoDial` — tick decal, mini-dial HUD, timeline ribbon.
- **Event flow**: `HeadsetTracker → PropMarble → PinchModel → TwistIntegrator → Timeline`; `ScenarioDirector → ResonanceJudge → EventBus {Scrub, Lock, Success, Fail, ScenarioClear} → DioramaRig / ChronoDial / VFXPool / AudioBank / ScoreKeeper`.
- **Pseudocode — scrub + lock (per tick / still event)**:

```text
if holdValid and twistValid:
    omega = project(angularVelocity(prop), holdAxis)
    omega = iir(median3(omega), 10 Hz)
    omega = clamp(omega, -MAX_OMEGA, MAX_OMEGA)
    T = clamp(T + (omega / 360.0) * dt, 0, 8.000)
    if T hits a bound: elasticOvershoot()

if abs(omega) < LOCK_OMEGA and abs(dHoldAxis) < 10 deg/s:
    stillTimer += dt
    if stillTimer >= 0.600:
        T_lock = median(last 12 T samples)
        emit Lock(T_lock)
        stillTimer = 0
else:
    stillTimer = 0
```

- **Pseudocode — resonance judge (on Lock)**:

```text
def onLock(T_lock):
    delta = T_lock - scenario.tStar
    if abs(delta) <= scenario.w:
        precision = 1 - abs(delta) / scenario.w
        score(500 * precision + attemptBonus(scenario.attempts))
        if abs(delta) <= 0.1 * scenario.w: score(300)
        emit Success(precision)
        scenario.solved = true
    else:
        scenario.attempts -= 1
        emit Fail(delta)
        if scenario.attempts <= 0:
            scenario.failed = true
        else:
            T = scenario.tStar + randomSign() * 1.2
```

## 16. Acceptance Test Checklist

1. Registration fits the sphere center < 0.6 mm and the hold axis within 5° of the left pinch line in 20/20 presentations.
2. A 360° clockwise twist advances T by exactly 1.000 s ± 10 ms; a 360° counter-clockwise twist rewinds by the same.
3. A 15° fine twist changes T by 0.0417 s ± 2 ms.
4. `|ω_s|` is clamped to the level's max (360–540 °/s) with no time overshoot beyond 1 tick.
5. Holding still for 600 ms commits the median of 12 samples; the commit is repeatable within ±1 ms for identical input.
6. The diorama `sample(T)` is deterministic: 1000 renders at the same T produce identical fragment positions (bitwise or < 0.001 mm).
7. A lock at `|Δ| = 0.5w` succeeds with precision 0.5 ± 0.02; a lock at `|Δ| = 1.01w` fails.
8. A perfect lock at `|Δ| ≤ 0.1w` awards the +300 perfect bonus exactly once.
9. Three failures mark the scenario failed and move on; a retry places T at `t* ± 1.2 s`, never on `t*`.
10. Releasing the pinch freezes T within one tick and never changes it while released.
11. Covering the marble for 1 s freezes T and shows a ghost; relock introduces no phantom rotation.
12. The proximity harmonic pitch follows `220 + 880·exp(−Δ/0.5)` within ±10 Hz.
13. A level-3 scenario (w = 35 ms) is solvable on attempt 1 in 5/5 trials by a scripted optimal scrub.
14. All 12 audio cues fire within 50 ms of their EventBus timestamps over a 60 s log.
