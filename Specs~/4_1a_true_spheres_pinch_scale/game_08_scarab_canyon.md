# Game 08 — Scarab Canyon — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Scarab Canyon |
| Slug | `scarab-canyon` |
| Physical prop | Glass marble, 16 mm diameter, ~5 g, resting in the deep central crease of an open upturned palm |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.1a True Spheres — Pinch-Scale (1–4 cm) |
| Scale class | Palm-scale single-hand cradle, tilt-steered groove rolling |
| Source concept | "Your palm transforms into a sunken desert canyon, and a miniature golden dung beetle pushes the glowing marble along the ravine floor. Tilt your palm gently to help the beetle roll the orb past falling boulders and virtual scorpions." |
| Source physical input | Rest the marble in the deep central crease of your upturned palm and tilt your hand slightly so the marble rolls back and forth along your palm crease by 2cm without reaching the edge. |

## 1. Fantasy & Core Loop

**Fantasy.** Your palm becomes a sunken slot canyon at golden hour. A tiny golden scarab heaves a glowing sun-orb along the ravine floor, and the canyon walls are your own fingers. Tilting your hand is the only steering you have — the orb rolls in the crease, the scarab scrambles to keep it moving, and boulders crash down from the rim.

**Core loop** (one hazard beat ≈ 1.5–2.6 s; one run ≈ 70–100 s):

1. **Settle the orb** (0.6 s) — the marble rests in the crease; the scarab takes position behind it.
2. **Read the canyon** (0.5–1.0 s) — a boulder telegraph (shadow) or a scorpion telegraph (claw clicks) appears at a lateral offset.
3. **Tilt to steer** (0.2–0.5 s) — roll the palm about the crease axis; the orb accelerates laterally in the groove.
4. **Counter-tilt** (0.2–0.4 s) — arrest the orb before it reaches the crease edge; a good dodge is a controlled S-curve.
5. **Pass** (0.2 s) — the orb clears the hazard; near-miss bonus if within 12 mm.
6. **Scarab boost** (0.3 s) — a clean pass lets the scarab shove the orb forward, raising speed.
7. **Run tally** (3.0 s) — at the end of the canyon or 3 orb escapes, tally.

The physical marble really rolls in your palm crease — the game reads that motion and mirrors it. Over-tilting sends the orb over the crease edge (a "spill"), which costs a life and forces a re-settle.

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

- **P frame**: origin = marble center when centered in the crease; `+x̂_P` = along the palm crease toward the middle finger; `+ŷ_P` = across the crease toward the thumb side; `+ẑ_P` = palm normal (out of the palm). The **groove axis** is `x̂_P`; the **steering axis** is `ŷ_P`.
- **Numbers**: r = 8 mm, m = 5 g, glass, µ_glass-skin ≈ 0.35, crease depth ≈ 2.5 mm, crease half-width 24 mm.
- **Palm frame**: `F_palm` from the tracked palm mesh (`L_palm` origin, `x_palm` = wrist→middle-base, `y_palm` = toward thumb, `z_palm` = palm normal). The groove is the line `{p : p_y = 0, p_z = crease depth}` in `F_palm`.
- **Steering**: the palm's roll about `x_palm` (`θ`) tilts the groove; in-plane gravity along the groove is `g_steer = g·sinθ`. The marble's groove coordinate `s = p_y` obeys `s̈ = g sinθ − µ_r g cosθ · sign(ṡ)`.
- **V frame**: anchored to **W** (the canyon is world-fixed and scrolls past), while the palm groove is the input. The orb's V lateral coordinate maps 1:1 from the groove offset with a 4.1× visual scale.
- **Deviations**: (a) the marble is *not* pinched — it is free to roll in the palm, so the game must handle loss-of-contact and spill; (b) the palm crease is not a perfect groove, so the game uses a dead-band of ±1.5 mm around the crease center and a soft wall at ±24 mm; (c) the source's "2 cm back and forth" is the *idle* amplitude; during play the allowed range is ±20 mm, and exceeding ±24 mm is a spill.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated; the holding forearm rests on the table, palm up, fingers gently cupped; the palm is 200–300 mm in front of the sternum and roughly horizontal.
- **Support surface**: the table supports the forearm; the palm itself is the prop surface. No external surface is needed under the palm.
- **Play patch**: the palm crease plus 30 mm of margin; the virtual canyon is a 180 mm wide strip rendered 40 mm above the palm.
- **Handedness**: single-hand; either hand works. Default is the non-dominant hand so the dominant hand can rest (or tap the "boost" gesture, which is intentionally absent — the game is one-handed).
- **Reach envelope**: the palm stays at 200–300 mm; no reaching.
- **Session length**: 6 min default (4 runs), hard stop 9 min.
- **Fatigue mitigations**: (1) the forearm rests on the table; (2) tilts are ±12°, not large wrist motions; (3) the marble's own friction does most of the work; (4) a 6 s "oasis" rest between runs, with the palm flat.

## 4. Prop Geometry & Registration

- **Real dimensions**: diameter 16.0 mm (r = 8.0 mm), mass 5.0 g, glass, µ ≈ 0.35.
- **Sphere model**: `|p| = 8` mm; no features. The game adds a virtual glow orb skin and a meridian ring so the player can see the marble rolling.
- **Palm-groove model** (F_palm, mm): crease centerline `y = 0`, `z = 2.5`; groove walls at `y = ±24`; groove floor curvature radius 60 mm (a shallow valley); usable length `x ∈ [−45, +45]` (wrist to finger bases).
- **Colliders**: `COL_palm` curved plane (the palm mesh simplified to a 9 × 5 control grid); `COL_groove` capsule chain along the crease (r = 1.5 mm); `COL_wallL/R` soft walls at `y = ±24`; `COL_marble` r = 8 mm; `COL_fingers` five capsule chains (the real fingers are the canyon rim and must not be penetrated by the virtual orb).
- **Registration**: (1) player rests the marble in the palm crease, palm up; (2) system fits the marble center and the palm plane over 4 frames (center error < 0.8 mm, palm plane RMS < 1.2 mm); (3) the crease axis is derived from the palm mesh's principal axis and the middle-finger base; (4) `P` published at 72 Hz; (5) the groove is re-fit every 500 ms from the palm mesh so a shifting grip does not break the mapping.
- **Re-acquisition**: search 70 mm around the last pose; re-fit the marble as a sphere on the palm plane; blend over 150 ms. If the marble is off the palm (distance from the palm plane > 20 mm or outside the finger rim), treat as SPILLED and re-seat after 1.0 s.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Palm cradle | palm mesh + `L_ring_tip`, `L_pinky_tip` | marble rests in crease | cradle present; groove active |
| I5.2 | Tilt left | roll palm about crease axis toward thumb | `COL_palm` | groove acceleration +ŷ |
| I5.3 | Tilt right | roll palm away from thumb | `COL_palm` | groove acceleration −ŷ |
| I5.4 | Level | hold roll within ±2° | palm | marble decelerates via friction |
| I5.5 | Finger wall | curl a finger to raise the canyon rim | finger capsules | soft wall at that side moves in 6 mm |
| I5.6 | Re-seat | cup the palm and nudge the marble to center | palm | clears SPILL, 1.0 s |
| I5.7 | Palm lift | raise palm > 40 mm from rest | — | pauses scroll (no penalty) |
| I5.8 | Idle | no tilt > 400 ms | — | friction settles the marble |

Per-finger roles: the palm is the entire control surface; ring and pinky provide the resting cup; index and middle may curl to narrow the groove (I5.5). There is no second hand. Per-hand role: the holding hand is both STEER and SURFACE.

## 6. Input Kinematics & Signal Processing

- **Palm roll**: `θ = atan2(ŷ_palm·ẑ_W, ẑ_palm·ẑ_W)` about the crease axis, wrapped to [−90°, +90°]; filtered 3-tap median → IIR `f_c = 6 Hz`. Dead-band ±2°.
- **Groove coordinate**: `s = p_marble · ŷ_palm` (in mm, relative to the crease centerline), clamped to ±30 for display but allowed to exceed 24 for spill detection.
- **Marble dynamics**: `s̈ = 9800·sinθ − µ_r·9800·cosθ·sign(ṡ)`, with `µ_r = 0.035` (rolling resistance in the groove); integrate at 90 Hz with 2 sub-steps; speed cap 260 mm/s. If `|θ| < 2°`, apply extra stiction so the marble settles within 0.4 s.
- **Spill**: `|s| > 24 mm` for > 60 ms → SPILL; the marble is considered to have rolled over the crease wall. One life lost; 1.0 s re-seat.
- **Near-miss**: at the moment a hazard passes the orb, `|Δs| < 12 mm` → near-miss +20 and a scarab chirp; `|Δs| < 5 mm` → "graze" +40.
- **Steering authority**: the mapped canyon lateral is `x_V = 4.1·s`, so the ±20 mm groove maps to ±82 mm of canyon width; the canyon is 180 mm wide, so a full-range tilt can just reach the walls.
- **Scroll speed**: forward speed `v_f = 70 mm/s · (1 + 0.05·clean_passes)`, capped at 2.0×; the virtual orb sits at a fixed screen position and the canyon scrolls.
- **Hazard telegraphs**: boulders show a 700 ms rim shadow at `x_V`; scorpions show a 900 ms claw-click at `x_V` and then crawl laterally at 40 mm/s.
- **Marble-loss guard**: if the marble is not visible in the palm for 300 ms, pause the scroll and show a re-seat hint.

## 7. Virtual World Layout & Coordinate Mapping

- **V frame**: anchored to W; origin = the orb's current canyon position, `x̂_V` = canyon forward, `ŷ_V` = canyon lateral (right), `ẑ_V` = up. The orb is pinned at `x_V = 0`; the world scrolls toward −x_V.
- **Canyon**: a 180 mm wide floor strip extending `x_V ∈ [−400, +2000]`, with 120 mm walls on each side; the floor is rendered 40 mm above the palm plane so it reads as a hologram hovering over the hand.
- **Orb**: a 22 mm glowing sphere at `(0, x_V, 14)`; the real marble is visible through it as a dark rolling core (the orb is 65 % alpha).
- **Scarab**: a 14 mm golden beetle behind the orb at `x_V = −18`, animated pushing; it never collides.
- **Hazards**: BOULDER = a 30 mm sphere falling from `z_V = 180` at 900 mm/s onto a fixed `x_V` (impact ring 32 mm); SCORPION = a 24 mm crawler entering from one wall at `z_V = 4`, moving laterally at 40 mm/s; CRACK = a 40 mm floor crack that is harmless but marks the lane.
- **Mapping formulas**: groove offset → canyon lateral `x_V = 4.1·s`; palm roll → V roll visual `φ_V = 0.6·θ`; the canyon's horizon is locked to W so tilting the palm does not tilt the world.
- **Camera note**: the canyon is drawn in front of and above the palm; the palm's fingers form the near rim, so the player sees their own hand as the canyon's edge.

## 8. Gameplay Systems & Rules

- **Entities**: `Orb` (groove s, speed, glow), `Scarab` (boost state), `Hazard` (type, x_V, lateral, telegraph), `RunDirector`.
- **Lives**: 3 orb escapes (spills) per run; a hazard hit costs glow (not a life) and slows the scroll 20 % for 1.5 s.
- **Glow**: 100 %, drains 12 % per hazard hit and 2 %/s passively, recharges 20 % per clean pass. At 0 glow the orb dims and the scarab moves slowly (scroll ×0.7) until recharged.
- **Hazards**: boulder hit = 25 glow + slow; scorpion hit = 18 glow + slow; a hit also breaks the clean-pass streak. Spills are separate: the marble leaves the crease and the run loses a life.
- **Clean pass**: a hazard resolved without a hit; +50 score and +1 scroll step; 5 clean passes in a row → "Golden Path" ×1.4 until a hit.
- **Distance**: 1 m per 1000 mm of forward scroll; milestone every 100 m +40 score and a 0.3 s slow-mo.
- **Scoring**: clean pass +50 × mult; near-miss +20; graze +40; milestone +40; run end distance ×2; remaining lives ×80; glow remaining ×1.
- **Win/lose**: run ends at 0 lives or 1500 m; cleared at 750 m. Session = 4 runs; win if 3 cleared.
- **Difficulty**: hazard density rises every 150 m; boulder fall speed rises 8 % per 300 m.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, UI | REGISTER |
| REGISTER | marble + palm fit | center < 0.8 mm, palm RMS < 1.2 mm | derive crease axis | CALIBRATE |
| CALIBRATE | marble centered 500 ms | \|s\| < 4 mm | zero scroll, build canyon | BRIEF |
| BRIEF | tilt once | \|θ\| > 5° | run card 2.0 s | RUN |
| RUN | tilt | any | integrate s, steer orb | RUN |
| RUN | hazard passes | no hit | clean pass, +50 | RUN |
| RUN | hazard hit | — | glow −, slow, streak reset | RUN |
| RUN | \|s\| > 24 mm 60 ms | — | SPILL, life −1 | SPILL |
| RUN | 0 lives | — | tally | DEBRIEF |
| RUN | 1500 m | — | triumph tally | DEBRIEF |
| RUN | glow = 0 | — | scroll ×0.7 until glow > 25 | RUN |
| SPILL | 1.0 s elapsed | re-seat begun | marble recentered at s = 0 | RUN |
| RUN | palm lifted > 40 mm | — | pause scroll | PAUSED_PALM |
| PAUSED_PALM | palm lowered | — | resume 600 ms countdown | RUN |
| any | prop tracking loss > 200 ms | — | freeze scroll, ghost orb | REACQUIRE |
| REACQUIRE | relock ≤ 2 s | search r = 70 mm | blend P 150 ms | prior |
| REACQUIRE | 2 s timeout | — | "show the marble" card | REACQUIRE |
| DEBRIEF | run < 4, lives > 0 | — | next run seed | BRIEF |
| DEBRIEF | run = 4 or 3 fails | — | session stats | COMPLETE |
| any | idle 20 s / system pause | — | freeze timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: spill 1.0 s; clean-pass slow-mo 0.3 s; palm-lift grace 400 ms; hazard telegraph 700–900 ms; reacquire 2 s; hazard hit slow 1.5 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_marble` r = 8 mm, `COL_palm` 9 × 5 grid plane, `COL_groove` capsule chain, `COL_wallL/R` soft walls, five finger capsule chains, up to 8 `Hazard` spheres/capsules, orb sphere (visual + trigger), 3 life pips.
- **Pair matrix**: Marble↔Groove (rolling), Marble↔Wall (spill trigger), Marble↔Finger (soft bounce, no spill unless beyond wall), Orb↔Hazard (hit test in 2-D), Scarab↔nothing, Hand↔Marble (contact only).
- **Material values**: glass-skin µ = 0.35; rolling resistance in groove µ_r = 0.035; finger restitution 0.2; hazard collision is a 2-D overlap test (orb radius 11 mm + hazard radius).
- **Physical constants**: m = 5 g; r = 8 mm; groove floor radius 60 mm; crease depth 2.5 mm; `g = 9800 mm/s²`.
- **CCD**: the marble's max speed is 260 mm/s → 2.9 mm/tick, so no tunneling; hazards are evaluated as analytic lane overlaps, not swept geometry.
- **Force formulas**: `s̈ = g sinθ − µ_r g cosθ sign(ṡ)`; spill condition uses `|s| > 24 mm`; the visual marble spin is `ω = ṡ/r`.
- **Glow model**: `G ← clamp(G − 12·hits − 2·dt + 20·cleanPasses, 0, 100)`; scroll multiplier `0.7 + 0.3·(G/100)`.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → canyon far wall → hazards → orb/scarab → canyon near rim → VFX → hand → HUD.
- **Occlusion cases**:
  1. *Fingers as the near rim*: the real fingers occlude the near canyon wall; the game leans into this by rendering a rock-rim texture that fades into the real skin at the fingertip bases.
  2. *Marble inside the orb*: the real marble is visible through the 65 % alpha orb; where the palm crease meets the orb, a contact shadow anchors it.
  3. *Thumb over the canyon*: a thumb resting across the palm occludes a lateral slice of the canyon; the orb's HUD lane marker is exempt and floats above the thumb.
  4. *Canyon behind the hand*: the far canyon wall is depth-tested against the hand; if the hand is between camera and wall, the wall fades to 30 %.
  5. *Spill*: when the marble leaves the crease, the orb detaches and falls visually toward the palm edge; the real marble is briefly visible rolling on the palm, which sells the failure.
- **VFX budgets**: orb glow 16/s, scarab dust 8/s, boulder impact 26, scorpion skitter 10, near-miss spark 14, spill shards 22, milestone ring 18; global cap 380.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Cradle | palm | soft sand-wind bed | canyon hologram blooms |
| Tilt | palm | low stone grind, pitch ∝ \|θ\| | groove light shifts |
| Roll | marble | glassy rumble, pitch ∝ \|ṡ\| | orb trail |
| Clean pass | orb | scarab chirp + chime | +50 float, dust puff |
| Near-miss | orb | sharp whoosh | white spark ring |
| Graze | orb | metallic tick | gold flash, +40 |
| Boulder hit | orb | rock crunch + thud | glow dim, slow-mo |
| Scorpion hit | orb | chitin click + sting | violet flash |
| Spill | palm edge | marble clatter | life pip breaks |
| Golden Path | canyon | rising desert flute | gold trail |
| Milestone | canyon | horn + chime | distance ring |
| Run end | canyon | sunset wind-down | tally card |

## 13. Difficulty Progression & Tuning

| Level | Scroll base (mm/s) | Hazard density (per 100 m) | Boulder fall (mm/s) | Scorpion speed (mm/s) | Spill threshold (mm) | Lives |
|---|---|---|---|---|---|---|
| 1 | 60 | 6 | 700 | 30 | 26 | 3 |
| 2 | 70 | 8 | 800 | 34 | 25 | 3 |
| 3 | 80 | 10 | 900 | 38 | 24 | 3 |
| 4 | 90 | 12 | 1000 | 42 | 23 | 3 |
| 5 | 100 | 14 | 1100 | 46 | 22 | 2 |

- **Adaptive rule** (`SCARAB_ADAPT`): after each run, if distance ≥ 750 m and lives ≥ 2, level +1; after a failed run, level −1 and scroll ×0.9. Clamp 1–5; one step per run.
- **Named constants**: `R_MARBLE = 8 mm`, `GROOVE_HALF = 24 mm`, `STEER_SCALE = 4.1`, `MU_ROLL = 0.035`, `SPILL_MS = 60`, `NEAR_MISS = 12 mm`, `GRAZE = 5 mm`, `GLOW_HIT = 12`, `GLOW_DRAIN = 2/s`, `GLOW_RECHARGE = 20`, `SCROLL0 = 70 mm/s`.

## 14. Edge Cases & Failure Recovery

- **Prop tracking loss > 200 ms**: freeze scroll and hazard motion; the orb becomes a 40 % ghost but the real marble remains visible in passthrough. On relock, `s` is re-derived from the marble's palm position; no spill is charged for a tracking loss.
- **Hand tracking loss**: the palm is the input surface, so losing it pauses the run after 200 ms; a single-frame dropout is bridged by the last palm pose for up to 120 ms.
- **Marble rolls off the palm**: if the sphere center leaves the palm polygon or its distance from the palm plane exceeds 20 mm, treat as SPILL regardless of `s`; 1.0 s re-seat with no additional life loss beyond the spill.
- **Palm rotated too far**: roll beyond ±35° makes the groove unusable (the marble free-falls); the game clamps the *input* at ±30° and shows a "level your palm" hint, so the player is not punished for a tracking-scale error.
- **Finger curl changes the groove**: curling the ring/pinky narrows the soft walls by up to 6 mm; the groove re-fits every 500 ms, and a sudden curl > 30° triggers a 200 ms input freeze to avoid a false spill.
- **Marble at rest at the crease edge**: if `|s| > 20 mm` and `|ṡ| < 5 mm/s` for 1 s, the game nudges the orb back 2 mm (a "scarab push") so a run cannot stall.
- **Both hands present**: only the hand with the marble is used; the other hand is ignored entirely (no two-hand boost).
- **Recovery UX**: one icon (marble / hand / palm) plus one line of text; no modal dialogs mid-run.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PalmSurface` — palm mesh fit, crease axis, groove model, finger rims.
  - `MarbleTracker` — sphere center, `s`, `ṡ`, spill detection.
  - `TiltModel` — palm roll, dead-band, filtered θ.
  - `GroovePhysics` — `s̈` integration, stiction, speed cap.
  - `CanyonDirector` — scroll speed, hazard spawns, telegraphs, density tables.
  - `OrbJudge` — 2-D hazard overlap, clean/near-miss/graze events.
  - `ScarabRig` — beetle animation, push state, clean-pass boost.
  - `CanyonRig` — world-fixed canyon, near-rim hand blend, orb/scarab render.
  - `RunDirector` — lives, distance, glow, adaptive level, scoring.
- **Event flow**: `HeadsetTracker → PalmSurface → MarbleTracker → TiltModel → GroovePhysics`; `CanyonDirector → OrbJudge → EventBus {CleanPass, NearMiss, Graze, HazardHit, Spill, Milestone} → ScarabRig / CanyonRig / VFXPool / AudioBank / ScoreKeeper`.
- **Pseudocode — groove physics (per tick)**:

```text
theta = iir(median3(palmRoll()), 6 Hz)
if abs(theta) < DEADZONE: theta = 0
a = 9800 * sin(theta) - MU_ROLL * 9800 * cos(theta) * sign(sdot)
sdot += a * dt
if abs(theta) < DEADZONE and abs(sdot) < 8: sdot = 0   # stiction
s += sdot * dt
if abs(s) > GROOVE_HALF and abs(s) > GROOVE_HALF for 60 ms:
    emit Spill(); lives -= 1; schedule(1.0 s, reseat)
```

- **Pseudocode — hazard resolution (per tick)**:

```text
for h in hazards:
    h.x -= scrollSpeed * dt
    if not h.resolved and abs(h.x) < (ORB_R + h.r):
        d = abs(4.1 * s - h.lateral)
        if d < ORB_R + h.r:
            glow -= h.glowCost; slowUntil = now + 1.5 s; streak = 0
            emit HazardHit(h)
        else:
            score(50 * mult)
            if d < NEAR_MISS: score(20); emit NearMiss
            if d < GRAZE: score(40); emit Graze
            streak += 1; if streak % 5 == 0: mult = 1.4
        h.resolved = true
```

## 16. Acceptance Test Checklist

1. Registration fits the marble center < 0.8 mm and palm plane RMS < 1.2 mm in 20/20 presentations; the crease axis is within 4° of the palm's principal axis.
2. A 6° palm roll for 1 s moves the marble along the groove by the analytic `½·g·sinθ·t²` prediction within ±12 %.
3. At θ = 0, the marble settles from 40 mm/s within 0.4 s due to stiction.
4. The groove coordinate `s` maps to canyon lateral with exactly 4.1× scale.
5. A marble crossing ±24 mm for 60 ms triggers exactly one SPILL and one life loss.
6. A marble returning to |s| < 24 mm within 60 ms does not spill.
7. A hazard overlap costs the specified glow and does not cost a life; a spill costs a life and not glow.
8. A clean pass at 8 mm lateral offset awards +50 and a near-miss +20, totaling +70.
9. Five consecutive clean passes activate Golden Path ×1.4; one hit clears it.
10. Glow = 0 reduces scroll to 0.7× until glow > 25.
11. Lifting the palm > 40 mm pauses the scroll within 400 ms and resumes with a 600 ms countdown.
12. A level-3 run of 750 m is clearable with ≥ 2 lives in 5/5 scripted inputs.
13. Covering the hand for 1 s freezes scroll and shows a ghost; relock re-derives `s` with no spill.
14. Particle count stays ≤ 380 during a boulder hit + spill + milestone.
