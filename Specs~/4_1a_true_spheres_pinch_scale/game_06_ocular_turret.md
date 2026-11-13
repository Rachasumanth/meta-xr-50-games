# Game 06 — Ocular Turret — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Ocular Turret |
| Slug | `ocular-turret` |
| Physical prop | Glass marble, 16 mm diameter, ~5 g, held in a left thumb–index O-ring |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.1a True Spheres — Pinch-Scale (1–4 cm) |
| Scale class | Pinch-scale, one-hand cradle, free-space aiming |
| Source concept | "A biomechanical cyber-eye overlays the marble, firing a continuous targeting beam out of its pupil in whatever direction you roll the sphere. Sweep the gaze across your 40cm personal space to petrify incoming gargoyle swarms." |
| Source physical input | Hold the marble inside a circle formed by your left thumb and forefinger, and roll the exposed front face of the marble using your right index fingertip. |

## 1. Fantasy & Core Loop

**Fantasy.** The marble is the eyeball of a biomechanical sentinel. Your left thumb and forefinger form its socket; the right index finger rolls the glass, and a pupil of red light tracks across the room. Whatever the gaze touches turns to stone — gargoyles caught in the beam stiffen mid-wingbeat and drop like statues.

**Core loop** (one gargoyle ≈ 1.6–2.6 s; one vigil ≈ 70–100 s):

1. **Hold the socket** (continuous) — left thumb and index form an O-ring around the marble; the exposed crown faces the player's right.
2. **Acquire** (0.3–0.8 s) — a gargoyle enters the 400 mm personal sphere with a wing-flap telegraph and a flight arc.
3. **Track** (0.2–0.6 s) — roll the marble so the pupil axis sweeps onto the target; the beam is a visible red ray with a 4 mm spot.
4. **Hold** (0.4 s) — keep the gaze within 6° of the gargoyle to fill its petrification meter.
5. **Petrify** (0.15 s) — meter reaches 1.0; the gargoyle turns to stone, falls, and shatters.
6. **Recover** (0.2–0.5 s) — beam heat vents; the next gargoyle telegraphs.
7. **Vigil tally** (2.5 s) — after 12 gargoyles, shields restore, next vigil.

The challenge is angular precision with a rolling sphere: small fingertip motions produce large gaze sweeps, and over-rolling overshoots the target.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the center, poles at `±ẑ_P`, equator in `x̂_P`–`ŷ_P`, no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]` from the equator, longitude `λ ∈ [0°, 360°)` from +x̂_P toward +ŷ_P; surface point `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; outward normal `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P frame**: origin = marble center; `+ẑ_P` = the **pupil axis** at registration (the pupil is painted at the north pole of the virtual eyeball); `+x̂_P` and `+ŷ_P` complete the equator. The marble's real glass has no features, so the pupil is a virtual decal locked to `P`.
- **Numbers**: r = 8 mm, m = 5 g, glass, µ_glass-skin ≈ 0.30, rolling resistance negligible.
- **Gaze**: `ĝ_W = R_P · (0, 0, 1)` (the pupil's world direction). The beam origin is the marble center, range 400 mm, angular spot radius 4 mm at 200 mm.
- **Rolling kinematics**: fingertip contact at unit surface normal `n̂` (outward at the contact) with tangential slip velocity `v_t = v_tip − v_P − ω × r·n̂`; rolling without slip gives `ω = (n̂ × v_tip)/r`. The marble is held, so `v_P ≈ 0`; the dominant term is the fingertip's tangential motion.
- **V frame**: anchored to **P** (the eye and beam are P-fixed), while the gargoyle swarm is anchored to **W** (the room). This split is deliberate: rolling the eye sweeps the beam across a world-fixed swarm.
- **Deviations**: (a) a real sphere has no pupil, so the virtual pupil is a locked decal and the *gaze* is defined by `P`, not by a visible real feature; (b) the O-ring grip constrains the sphere's center but not its orientation, so all input is pure rotation; (c) the beam is a game raycast, not a physics laser — it ignores real-world geometry and is occluded only by virtual entities and the hand mesh.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated or standing; left hand held at 250–330 mm in front of the sternum, palm facing right, thumb and index forming a 16 mm O-ring; right index hovers over the exposed crown.
- **Support surface**: none required; calibration uses a table to establish the pupil's initial forward direction.
- **Play volume**: a 400 mm radius sphere centred on the marble, clipped to the forward hemisphere (the beam does not fire backward through the player).
- **Handedness**: mirrored — right-hand O-ring, left index roller.
- **Reach envelope**: fingertips 180–330 mm from the shoulders; the right index travels only ~30 mm across the marble crown.
- **Session length**: 7 min default (5 vigils), hard stop 10 min.
- **Fatigue mitigations**: (1) the O-ring grip is isometric and light; (2) the beam has no recoil; (3) the right index can rest between targets; (4) a 5 s "blink" rest between vigils, during which the eye closes and the beam is off.

## 4. Prop Geometry & Registration

- **Real dimensions**: diameter 16.0 mm (r = 8.0 mm), mass 5.0 g, soda-lime glass, refractive index 1.52 (visual only).
- **Sphere model** (P frame, mm): surface `|p| = 8`; pupil decal centered at `(0, 0, +8)` with angular radius 9°; iris ring at 9°–22°; sclera elsewhere. Equator reference marks every 30° for visual roll feedback (virtual only).
- **Colliders**: `COL_sphere` r = 8 mm (hand contact); `COL_socket` torus at the O-ring (r_major = 7.5 mm, r_minor = 3 mm) representing the thumb–index grip; `COL_crown` spherical cap on the +z side used for roller contact; no other colliders.
- **Registration**: (1) player holds the marble in the O-ring with the pupil facing away; (2) system fits the sphere center over 3 frames (center error < 0.6 mm) and estimates `R_P` from the O-ring plane plus the fingertip contact normal; (3) because the glass is featureless, `R_P` is initialized so `+ẑ_P` = the player's forward gaze at the moment of a "look here" calibration tap; (4) `P` published at 72 Hz; orientation is integrated from rolling and re-anchored to the O-ring plane every 2 s when the marble is still.
- **Re-acquisition**: search 60 mm around the last pose; re-fit the sphere center; orientation is *not* re-derived from vision (impossible on blank glass) but held at the last integrated value, with a 150 ms blend to the O-ring plane's normal. A "present the marble" card after 1.5 s.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | O-ring socket | `L_thumb_tip` + `L_index_tip` opposed | `COL_socket` | socket integrity; enables beam |
| I5.2 | Roll up/down | `R_index_tip` tangential sweep toward crown top/bottom | `COL_crown` | pitch gaze ±80° |
| I5.3 | Roll left/right | `R_index_tip` tangential sweep across crown | crown | yaw gaze ±100° |
| I5.4 | Circular scrub | `R_index_tip` small circles | crown | fine aim (0.2° per mm) |
| I5.5 | Hold still | no roller contact > 200 ms | — | gaze locks (no drift) |
| I5.6 | Blink | left thumb + index pinch closed 120 ms | socket | beam off 300 ms, heat −20 |
| I5.7 | Re-seat | socket reopened | socket | resume; gaze preserved |
| I5.8 | Double-tap crown | two taps < 250 ms | crown | fire a 3-round petrify burst (cooldown 8 s) |

Per-finger roles: left thumb + index form the socket; right index is the only roller; right middle is an alternate roller at 70 % gain; ring/pinky ignored. Per-hand roles: left = SOCKET, right = AIM. If the socket opens, the eye "falls" and the beam cuts.

## 6. Input Kinematics & Signal Processing

- **Rolling integration**: at each tick, for roller contact with outward normal `n̂` and tangential tip velocity `v_t`: `ω = (n̂ × v_t)/r`; `R_P ← expm([ω]× dt) · R_P`; renormalize. Contact slip guard: if `|v_t| > 300 mm/s`, scale `ω` by `300/|v_t|` (the fingertip skates rather than spinning the marble).
- **Gaze extraction**: `ĝ_W = R_P · ẑ_P`; yaw `= atan2(ĝ_W·ŷ_W, ĝ_W·x̂_W)`, pitch `= asin(ĝ_W·ŷ_W_up)`. Filter the gaze direction with a 3-tap median on the unit vector then renormalize; no extra IIR (the physical roll already filters).
- **Aim gain**: 1 mm of tangential fingertip travel = 7.2° of gaze (`1/r` radians); this is physical, not a tuning knob, but the *effective* gain is reduced 30 % by the slip guard for fast strokes.
- **Beam hit test**: angular distance `α = acos(ĝ_W · û)` where `û` is the unit direction to a gargoyle; beam hits if `α < atan(4/|p|)` plus the gargoyle's angular radius. At 200 mm, spot radius 4 mm → 1.15°.
- **Petrification**: `P_g ← clamp(P_g + 2.5·dt, 0, 1)` while hit, `P_g ← clamp(P_g − 1.2·dt, 0, 1)` otherwise; at 1.0 the gargoyle is petrified. A 400 ms continuous hit is required to fill from 0.
- **Heat**: beam on `+30/s`; beam off `−20/s`; at 100 OVERHEAT locks the beam for 2.0 s and vents at 60/s. Blink (I5.6) subtracts 20 instantly.
- **Socket gate**: both `L_thumb_tip` and `L_index_tip` within 6 mm of the socket torus and separated by 12–22 mm; otherwise socket = OPEN and the beam is disabled.
- **Drift correction**: when no roller contact for 500 ms and socket stable, `R_P` is blended 2 %/frame toward the orientation whose `+ẑ_P` lies in the socket plane; this removes slow integration drift without fighting active aiming.
- **Burst (I5.8)**: three rapid raycasts at 0°, ±4° yaw over 120 ms; each applies `P_g +0.5`; 8 s cooldown.

## 7. Virtual World Layout & Coordinate Mapping

- **V frames**: `V_eye = P` (pupil, iris, beam origin); `V_swarm = W` (gargoyles, room haze).
- **Personal sphere**: radius 400 mm centred on the marble; gargoyles spawn on the shell at `(r=400, yaw ∈ [−70°, +70°], pitch ∈ [−35°, +35°])` relative to the player's forward.
- **Gargoyle flight**: each follows a quadratic Bézier from its spawn point to a "dive point" 60 mm in front of the marble, over 2.5–4.0 s; wing flaps at 3 Hz; on reaching the dive point it attacks (socket damage).
- **Beam**: a 400 mm ray from the marble center along `ĝ_W`, rendered as a 2 mm core + 6 mm glow; the spot is an 8 mm disc at the hit point.
- **Haze**: a 420 mm radius fog shell at 15 % density, giving the beam visible shafts and depth cues.
- **Mapping formulas**: gargoyle world position from spawn spherical coordinates `p_W = C + 400·(cos p·cos y, sin p, cos p·sin y)` where `C` = marble center and axes are W. Beam endpoint `e_W = C + 400·ĝ_W`.
- **Camera note**: the beam is drawn from the marble, not from the head, so it visually diverges from the player's line of sight; this reinforces that the eye is the aiming device, not the headset.

## 8. Gameplay Systems & Rules

- **Entities**: `Eye` (R_P, ĝ_W, heat, socket), `Gargoyle` (flight spline, P_g, state), `Beam` (ray, hit set), `VigilDirector`.
- **Gargoyle states**: SPAWN → FLY → DIVE → ATTACK → PETRIFIED → SHATTER. One beam hit per tick; a gargoyle can be hit while flying or diving, but not while already petrified.
- **Attack**: on reaching the dive point, the gargoyle claws the socket: socket integrity −1 (3 pips). At 0, the eye closes and the vigil fails. A petrified gargoyle never attacks.
- **Petrify reward**: +100; petrify during the last 0.5 s of a dive +50 ("stone cold"); petrify three in 4 s → "Marble Gaze" combo ×1.3.
- **Shields**: 3 socket pips, restored 1 per vigil clear. A blink does not spend a pip.
- **Waves**: 12 gargoyles per vigil; spawn interval 2.2 s at level 1 down to 1.1 s at level 5; up to 4 simultaneous.
- **Scoring**: petrify +100 (+50 stone cold); combo ×1.3; vigil clear +300; no-damage vigil +500; remaining sockets ×100; accuracy bonus = 1 + (petrifies with < 1.0 s beam time)/12.
- **Win/lose**: vigil cleared at 12 petrified; failed at 0 sockets. Session = 5 vigils; win if ≥ 3 cleared.
- **Heat interaction**: a player who holds the beam on one target overheats after 3.3 s; sweeping across targets and blinking is the intended rhythm.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, UI | REGISTER |
| REGISTER | sphere fit | center error < 0.6 mm | set pupil = forward at tap | CALIBRATE |
| CALIBRATE | socket closed 300 ms | thumb/index 12–22 mm apart | anchor swarm shell | BRIEF |
| BRIEF | blink once | — | vigil card 2.5 s | VIGIL |
| VIGIL | roller contact | socket closed | integrate R_P, sweep beam | VIGIL |
| VIGIL | beam hit | heat < 100 | P_g += 2.5·dt | VIGIL |
| VIGIL | P_g ≥ 1.0 | — | petrify, shatter, +100 | VIGIL |
| VIGIL | gargoyle reaches dive point | not petrified | socket −1 | VIGIL |
| VIGIL | socket = 0 | — | eye closes, fail | DEBRIEF |
| VIGIL | 12 petrified | — | tally, shield +1 | DEBRIEF |
| VIGIL | heat ≥ 100 | — | beam locked 2.0 s | OVERHEAT |
| VIGIL | socket OPEN | — | beam disabled, "hold the ring" hint | VIGIL |
| OVERHEAT | 2.0 s elapsed | — | heat = 0, resume | VIGIL |
| VIGIL | double-tap crown | burst ready | 3 raycasts, P_g +0.5 each | VIGIL |
| any | prop tracking loss > 150 ms | — | freeze swarm, ghost eye | REACQUIRE |
| REACQUIRE | relock ≤ 1.5 s | search r = 60 mm | blend orientation 150 ms | prior |
| REACQUIRE | 1.5 s timeout | — | "present the marble" card | REACQUIRE |
| DEBRIEF | vigil < 5, sockets > 0 | — | next vigil seed | BRIEF |
| DEBRIEF | vigil = 5 or 3 fails | — | session stats | COMPLETE |
| any | idle 20 s / system pause | — | freeze timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: overheat 2.0 s; burst cooldown 8 s; blink 300 ms; drift blend 2 %/frame after 500 ms idle; gargoyle flight 2.5–4.0 s; reacquire 1.5 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_sphere` r = 8 mm, `COL_socket` torus, `COL_crown` cap, up to 4 `Gargoyle` capsules (r = 14 mm, length 30 mm), beam ray (analytic), 3 socket pips.
- **Pair matrix**: Beam↔Gargoyle (analytic angular test), Beam↔nothing else (ignores real geometry), Gargoyle↔Socket (attack trigger at dive point), Hand↔Sphere (contacts only), Gargoyle↔Gargoyle (soft separation 20 mm).
- **Material values**: gargoyle separation impulse 0.01 N·s; no gravity while flying; petrified gargoyles get gravity 9800 mm/s² and shatter on the table plane at `T`.
- **Physical constants**: m = 5 g; r = 8 mm; µ = 0.30; `I = 2mr²/5 = 1.28 × 10⁻⁷ kg·m²` (used only for the visual spin inertia; the integration is kinematic).
- **CCD**: the beam is a ray, so no tunneling; gargoyle movement is spline-driven and its angular radius is evaluated continuously.
- **Force formulas**: none for the beam; the petrified shatter uses `v = sqrt(2·g·h)` from the petrify height, capped at 1200 mm/s.
- **Socket attack model**: a gargoyle within 60 mm of the marble center and within 25° of the socket axis attacks; the 60 mm standoff prevents the virtual creature from clipping through the real hand.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → haze shell → gargoyles → beam → eye/pupil → VFX → hands → HUD.
- **Occlusion cases**:
  1. *O-ring fingers*: the left thumb and index wrap the marble; their tips occlude a 12–20° arc of the sphere. The pupil is placed at the exposed crown, and if the gaze points behind a finger, the beam is visually clipped by the hand mesh — this is intentional and teaches the player to keep the pupil clear.
  2. *Rolling fingertip*: the right index covers 8–12° of the crown; the beam origin is offset 2 mm forward so it never starts inside the fingertip.
  3. *Beam through fingers*: a beam that would pass through the roller finger is depth-tested and dimmed 60 % in that 10 mm segment; it still hits the target (gameplay is not blocked by fingers).
  4. *Gargoyle behind the hand*: a gargoyle that dives behind the holding hand is occluded; a red outline is drawn on top so the player can still aim.
  5. *Haze vs near plane*: the haze shell is clipped at 80 mm from the camera to prevent fogging the hands.
- **VFX budgets**: beam core (no particles), spot glow 6, petrify 24, shatter 30, socket hit 18, overheat vent 40/s, mote drift 20/s ambient, global cap 400.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Socket closed | O-ring | soft servo lock | ring glow, pupil opens |
| Roll | marble | glassy whisper, pitch ∝ speed | iris parallax |
| Beam on | pupil | rising sine + crackle | red ray, spot |
| Beam hit | gargoyle | stone-grind hum | petrify meter ring |
| Petrify | gargoyle | granite crunch + wing snap | full grey, +100 |
| Shatter | table | stone shards | 30-particle burst |
| Socket attack | O-ring | claw scrape + alarm | pip breaks, red vignette |
| Overheat | pupil | steam hiss + descending tone | beam sputters |
| Blink | eye | eyelid click | 300 ms black iris |
| Vigil clear | room | low bronze gong | tally card |
| Vigil failed | room | closing iris whir | screen iris-out |

## 13. Difficulty Progression & Tuning

| Level | Gargoyles | Spawn gap (s) | Max simultaneous | Flight time (s) | Angular radius (°) | Socket pips |
|---|---|---|---|---|---|---|
| 1 | 8 | 2.4 | 2 | 4.0 | 2.6 | 3 |
| 2 | 10 | 2.0 | 3 | 3.6 | 2.4 | 3 |
| 3 | 12 | 1.7 | 3 | 3.2 | 2.2 | 3 |
| 4 | 14 | 1.4 | 4 | 2.9 | 2.0 | 3 |
| 5 | 16 | 1.1 | 4 | 2.6 | 1.8 | 2 |

- **Adaptive rule** (`OCULAR_ADAPT`): after each vigil, if sockets lost ≤ 1 and clear time < 80 s, level +1; after a failed vigil, level −1 and flight time ×1.1. Clamp 1–5; one step per vigil.
- **Named constants**: `R_MARBLE = 8 mm`, `AIM_GAIN = 7.2 °/mm`, `SLIP_V = 300 mm/s`, `BEAM_RANGE = 400 mm`, `SPOT_R = 4 mm`, `FILL = 2.5/s`, `DECAY = 1.2/s`, `HEAT_ON = 30/s`, `HEAT_OFF = 20/s`, `OVERHEAT = 100`, `BLINK = 300 ms`, `BURST_CD = 8 s`, `SOCKET_SEP = [12, 22] mm`.

## 14. Edge Cases & Failure Recovery

- **Prop tracking loss > 150 ms**: freeze gargoyle flight and the beam; the marble becomes a 40 % ghost. Orientation is preserved by integration (it cannot be re-derived from blank glass). On relock, blend to the O-ring plane over 150 ms; no petrification progress is lost.
- **Hand tracking loss**: losing the roller freezes gaze (no drift) but keeps the beam on the last direction; losing the socket hand cuts the beam and pauses attacks after 1.5 s. Both lost > 300 ms pauses the vigil.
- **Marble dropped**: socket OPEN > 400 ms → pause; on re-seat, the gaze is held, shields unchanged, no penalty.
- **Orientation drift**: the 2 %/frame blend when idle corrects slow drift; if the gaze error exceeds 15° between two "hold still" moments, the game re-anchors to the socket plane and shows a one-line "re-centered" toast.
- **Socket too wide/narrow**: separation outside 12–22 mm does not count as a socket; the eye "closes" and a hint shows the correct pinch width.
- **Beam through the roller finger**: allowed and dimmed, never blocked, because blocking would make the game unplayable with a finger on the crown.
- **Overheat during a petrify**: the in-progress meter holds (does not decay) during the 2 s lock, then resumes; this avoids losing a near-complete petrify to the lock.
- **Gargoyle stuck behind the hand**: if a gargoyle is occluded for > 1.0 s, its outline brightens and it re-routes its dive by 15°; it never becomes untargetable.
- **Recovery UX**: one icon (marble / O-ring / hand) plus one line of text; no modal dialogs mid-vigil.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropMarble` — owns `poseP`, sphere center, O-ring plane, rolling contacts.
  - `RollIntegrator` — `ω = (n × v_t)/r`, slip guard, quaternion renormalization, drift blend.
  - `GazeModel` — `ĝ_W`, yaw/pitch, beam ray, heat.
  - `SocketModel` — thumb/index separation and contact, integrity pips.
  - `GargoylePool` — 6 agents, Bézier flight, dive point, petrify meter, shatter.
  - `BeamJudge` — angular hit test, per-tick fill/decay.
  - `VigilDirector` — spawn cadence, waves, scoring, adaptive level.
  - `EyeRig` — pupil decal, iris, beam core/glow, spot.
  - `SwarmRig` — gargoyle meshes, stone material swap on petrify.
- **Event flow**: `HeadsetTracker → PropMarble → RollIntegrator → GazeModel → BeamJudge`; `VigilDirector → GargoylePool → BeamJudge`; `BeamJudge → EventBus {BeamHit, Petrify, Shatter, SocketHit, Overheat, VigilClear} → VFXPool / AudioBank / ScoreKeeper / HUD`. `EyeRig` and `SwarmRig` read pose and EventBus only.
- **Pseudocode — rolling + gaze (per tick)**:

```text
if rollerContact:
    n = normalize(contactPoint - center)          # outward
    vt = tipVelocity - dot(tipVelocity, n) * n    # tangential
    if |vt| > SLIP_V: vt *= SLIP_V / |vt|
    omega = cross(n, vt) / R_MARBLE
    R_P = normalize(expm(skew(omega) * dt) * R_P)
elif now - lastRoll > 500 ms and socketStable:
    R_P = slerp(R_P, socketPlaneOrientation, 0.02)
gaze = R_P * (0, 0, 1)
```

- **Pseudocode — beam judgement (per tick)**:

```text
if socketClosed and heat < 100 and not blinking:
    best = None
    for g in gargoyles where g.alive:
        u = normalize(g.center - marbleCenter)
        alpha = acos(clamp(dot(gaze, u), -1, 1))
        if alpha < deg2rad(g.angularRadius) + atan(SPOT_R / |g.center - marbleCenter|):
            best = g; break
    if best:
        best.meter += FILL * dt
        if best.meter >= 1: petrify(best)
    heat = min(100, heat + HEAT_ON * dt)
else:
    heat = max(0, heat - HEAT_OFF * dt)
for g in gargoyles where not hit: g.meter = max(0, g.meter - DECAY * dt)
```

## 16. Acceptance Test Checklist

1. Registration fits the marble center < 0.6 mm in 20/20 presentations; the pupil initializes to the calibrated forward direction within 2°.
2. A 1 mm tangential fingertip travel rotates the gaze by 7.2° ± 0.5° for a slow stroke; a 400 mm/s stroke is slip-clamped to a 300 mm/s equivalent.
3. Rolling up then down by the same amount returns the gaze within 1° of its start (integration symmetry).
4. Holding still for 500 ms engages the drift blend and changes the gaze by < 0.5° over 2 s.
5. A gargoyle at 200 mm is hit when the gaze is within 1.15° + its angular radius, and missed at 2× that angle.
6. A stationary beam fills a gargoyle meter from 0 to 1 in 400 ms ± 20 ms.
7. Beam heat reaches 100 after 3.33 s of continuous fire and clears in 2.0 s ± 0.1 s of lockout.
8. A blink cuts the beam for 300 ms and subtracts exactly 20 heat.
9. Opening the socket disables the beam within one tick and restores it within one tick of re-closing.
10. A gargoyle reaching its dive point costs exactly 1 socket pip; a petrified gargoyle never attacks.
11. Covering the marble for 1 s freezes the swarm and preserves gaze and meters; relock blends in ≤ 150 ms.
12. A level-3 vigil of 12 gargoyles is clearable with ≥ 2 sockets remaining in 5/5 scripted runs under 90 s.
13. The double-tap burst applies exactly +0.5 to three targets and respects an 8 s cooldown.
14. Particle count stays ≤ 400 during a 4-gargoyle shatter + overheat vent.
