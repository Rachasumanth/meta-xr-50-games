# Game 09 — Phantom Stone Skipper — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Phantom Stone Skipper |
| Slug | `phantom-stone-skipper` |
| Physical prop | Flat pebble, 45 × 38 × 12 mm, 25 g, pinched flat between thumb and index |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.2 Prolate & Oblate Spheroids |
| Scale class | Tabletop, pinch-held prop, ballistic wrist snap |
| Source concept | A horizontal wrist snap launches a phantom stone across a holographic pond; tap the real stone at each water touch to chain skips |
| Source physical input | Pinch the flat pebble between thumb and index, snap the wrist 30 mm horizontally, release, then tap the stone's top face on every touch |
| Session shape | 5 ponds, 20–35 s each, ≈3 min |
| Primary skill | Release timing plus a 70 ms tap window repeated 10–14 times |

## 1. Fantasy & Core Loop

**Fantasy.** The pebble is a fragment of a drowned moon. A horizontal snap throws its phantom across a pond of standing light; every touch is a step, every lotus pad a gate. The real stone in the hand is the anchor: tap it at the exact moment the phantom kisses the water and the next skip is born.

**Core loop** (pond 20–35 s; session 5 ponds):

1. **Pinch** (continuous) — thumb and index clamp the flat faces; separation 11–15 mm.
2. **Aim** (0.3–0.8 s) — the pond's near edge lights; a launch reticle shows the predicted first skip.
3. **Snap** — 30 mm horizontal wrist travel in 90–160 ms; peak speed 0.4–1.8 m/s; pitch 6–24°.
4. **Release** — the pinch opens within 120 ms of the peak; the phantom spawns and the first hop begins.
5. **Tap** — at each water touch (every 0.42 s), tap the real stone's top face inside a ±70 ms perfect window.
6. **Gate** — each touchdown within 16 mm of a lotus pad scores; perfect taps restore skip energy.
7. **Finish** — 10–14 hops to the far bank; all pads plus no misses = perfect chain ×2.

The challenge is a metronome: the hop clock is fixed at 0.42 s, so the player must lock a 2.38 Hz tap rhythm while the phantom's landing points drift with each imperfect tap.

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

- **Exact spheroid**: `a = b = 19.0 mm`, `c = 6.0 mm`, oblate (`c/a = 0.316`), `m = 25 g`, density 2.76 g/cm³ (stone). P origin = fitted center; `+ẑ_P` = flat-face normal and pinch axis; poles at `(0, 0, ±6.0) mm`; equator = circle of radius 19.0 mm in the `x̂_P`–`ŷ_P` plane; `+x̂_P` = the pebble's long dimension (the real 45 mm axis).
- **Local rolling model**: `ω = (n̂ × v_t)/r_eff`. At the pinch face the meridional radius is `r_eff = a²/c = 60.2 mm` (used to reject micro-slip); at release the spin-transfer radius is `r_eff = a = 19.0 mm` because the rim is what leaves the fingers. Slip clamp 700 mm/s.
- **V frame**: anchored to **T**, with a fixed vertical offset of `+60 mm`. Origin = the pond's near edge centre; `+ŷ_T` = up; `+x̂_T` = throw direction (player forward); `+ẑ_T` = player's right. The pond is a 400 × 220 mm plane in `V` at `y_V = 0`, parallel to the table.
- **Deviations**: (a) the real pebble is 45 mm long but the fitted spheroid is 38 mm, so 3.5 mm at each `±x̂_P` tip lies outside the collider and is ignored; (b) the phantom's flight is time-dilated (0.42 s per hop, 26–58 mm per hop) rather than real ballistic scale, so the tap window is humanly reachable; (c) the real stone never leaves the hand — the release only spawns a virtual copy; (d) the pond is a virtual plane with no real water, and splash VFX never touches the table.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated; forearm rested on the table, hand at 260–340 mm in front of the sternum and 60–120 mm above the table.
- **Pinch**: thumb tip on the `+ẑ_P` face, index tip on the `−ẑ_P` face; separation 11–15 mm (12 mm stone plus 0–3 mm finger compliance); the pinch centroid is the tracked launch point.
- **Snap volume**: a 60 mm cube around the hand; the 30 mm horizontal travel must stay inside it so the wrist does not hit the table.
- **Pond**: 400 × 220 mm, drawn at 60 mm above the table, near edge 180 mm from the player; the far edge is 580 mm away, within comfortable gaze at 25° down.
- **Hand roles**: dominant hand = pinch/launch; non-dominant index = touch tapper. After launch the dominant hand holds the real stone still at the anchor point; the non-dominant index taps its top face.
- **Reach envelope**: the tapper travels ≤ 120 mm; the launcher ≤ 60 mm. No shoulder motion is required.
- **Forces**: pinch 2–6 N (static), snap peak 4–10 N at the fingertips, tap impulse 0.02–0.06 N·s; all well below discomfort.
- **Session**: 5 ponds, hard stop 6 min; 20 s rest between ponds.
- **Fatigue**: the 2.38 Hz tap rhythm is the limiting factor; ponds are capped at 14 hops and rest is enforced.

## 4. Prop Geometry & Registration

- **Real dimensions**: 45 × 38 × 12 mm, 25 g; tolerance ±0.8 mm length, ±0.6 mm width, ±0.4 mm thickness, ±1.0 g. Model volume `(4/3)πa²c = 9,073 mm³`.
- **Inertia**: `I_z = (2/5)ma² = 3.61 × 10⁻⁶ kg·m²`; `I_x = (1/5)m(a² + c²) = 1.99 × 10⁻⁶ kg·m²`. Used for the spin estimate at release, not for integration.
- **Surface atlas**: top face (`+ẑ_P`, `|φ| > 60°`) = pinch pad and tap target, a 14 mm disc; bottom face (`−ẑ_P`) = opposing pinch pad; rim band (`|φ| ≤ 30°`) = spin-transfer surface; 24 rim sectors for release diagnostics only.
- **Colliders**: `COL_stone` = spheroid; `COL_face_top`, `COL_face_bot` = 14 mm discs offset 0.2 mm above the surface for stable pinch/tap detection; `COL_pond` = 400 × 220 mm plane in V; `COL_table` = plane at `y_T = 0`.
- **Registration**: (1) the player pinches the pebble and holds it flat for 1.0 s; (2) the system fits the spheroid center over 12 frames, accepting center error < 0.6 mm and face-normal error < 4°; (3) the long axis is resolved from the 45 mm silhouette; (4) `P` publishes at 72 Hz, predicted to 90 Hz.
- **Re-acquisition**: search 50 mm around the pinch; refit the face plane; if the stone is edge-on (face normal within 20° of the view ray), hold the last pose and prompt a re-flatten. Registration is repeated after any drop.
- **Tolerances**: a 0.4 mm thickness error changes the pinch separation by 0.4 mm, inside the 11–15 mm gate; a 0.6 mm width error changes the spin radius by 3 %, inside the 5 % gameplay tolerance.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Flat pinch | `R_thumb_tip` + `R_index_tip` on opposite faces | `COL_face_top/bot` | armed; separation 11–15 mm |
| I5.2 | Horizontal snap | pinch centroid velocity, 160 ms window | — | peak speed, yaw rate, pitch |
| I5.3 | Release | pinch separation > 18 mm within 120 ms of peak | — | launch the phantom |
| I5.4 | Touch tap | `L_index_tip` on the top face | `COL_face_top` | one tap timestamp |
| I5.5 | Re-grip | pinch re-closed 11–15 mm | faces | re-arm for the next pond |
| I5.6 | Dry swing | snap without release | — | no launch, 0.6 s cooldown |
| I5.7 | Abort | pinch opened before the snap peak | — | cancel, no penalty |
| I5.8 | Pause | non-dominant palm up 700 ms | — | suspend timers |

Per-finger roles: dominant thumb + index = pinch/launch; non-dominant index = tap; all others ignored. Per-hand roles: dominant = LAUNCH, non-dominant = RHYTHM. If the non-dominant hand is unavailable, the player may tap with the dominant middle finger at 0.85× window width (assist).

## 6. Input Kinematics & Signal Processing

- **Pinch state**: separation `d = |p_thumb − p_index|`; ARMED if `11 ≤ d ≤ 15 mm` for ≥ 80 ms; RELEASED if `d > 18 mm` with 8 mm hysteresis. The pinch centroid `c = (p_thumb + p_index)/2`.
- **Snap extraction**: buffer `c` for 200 ms; `v(t) = (c(t) − c(t−55 ms))/55 ms`; peak horizontal speed `v_s = max|v_xz|`; `t_peak` = its time; yaw rate `ω_y` from the hand's rotation about `+ŷ_T`; pitch `θ_p = atan2(v_y, |v_xz|)` at `t_peak`.
- **Launch gate**: release must occur in `[t_peak − 40 ms, t_peak + 120 ms]`; otherwise the snap is a dry swing. `v_s ∈ [0.4, 1.8] m/s`; `θ_p ∈ [6°, 24°]`; a pitch < 6° is a dive (stone sinks on hop 1), > 24° is a stall (hop 1 lands short).
- **Launch mapping**: first skip length `S_0 = 26 + 14·(v_s/1.0) mm`, capped 58 mm; spin `ω_s = 12 + 3.4·ω_y rad/s`, capped 70 rad/s; `ω_y` measured over the same 55 ms window.
- **Hop clock**: `T_hop = 0.42 s` fixed; hop `k` lands at `x_k = x_{k−1} + S_k`, lateral `z_k = z_{k−1} + δ_k` where `δ_k` is the tap-timing error mapped to 0.18 mm/ms (a 70 ms late tap drifts 12.6 mm right).
- **Tap detection**: contact on `COL_face_top` with 25–90 ms duration and impulse 0.02–0.06 N·s; timestamp at peak force; corrected by −22 ms for audio latency so the player taps to the heard plink.
- **Tap windows**: perfect `|t − t_touch| ≤ 70 ms`; good `≤ 130 ms`; outside = miss. A tap more than 210 ms before a touch is "early" and is consumed by the next touch (no double count).
- **Energy**: `h_{k+1} = h_k·e^(−δ_eff)` with `δ_eff = 0.11·(1 − 0.35·s)`, `s = clamp((ω_s − 20)/50, 0, 1)`; a perfect tap restores `h` and `S` to 92 % of the previous hop, good to 75 %, miss to 45 %. Two consecutive misses sink the stone.
- **Signal chain**: joints → 3-tap median → one-euro filter (1.8 Hz min cutoff, β = 0.04) → pinch state → snap buffer. The 55 ms velocity window is 5 render frames and 4 tracking frames, giving ±11 ms timing resolution.
- **Latency**: 14 ms tracking + 11 ms physics + 11 ms render + 22 ms audio = 58 ms; the perfect window ±70 ms leaves 12 ms of margin, so the audio cue is scheduled 22 ms early to compensate.

## 7. Virtual World Layout & Coordinate Mapping

- **T axes**: origin at the pond's near-edge centre projected on the table; `+ŷ_T` = up; `+x̂_T` = throw direction; `+ẑ_T` = player's right. **V** = T translated `+60 mm` along `ŷ_T`.
- **Pond**: rectangle `x_V ∈ [0, 400]`, `z_V ∈ [−110, +110]`, water plane `y_V = 0`; the near bank is a 12 mm lip; the far bank is a 30 mm glowing shelf.
- **Lotus pads**: 8–14 discs, radius 14 mm, placed on a deterministic seeded layout with a minimum 38 mm separation and a 24 mm edge margin; each pad has a 2 mm rim and a 6 mm bloom.
- **Phantom stone**: a 19 mm × 6 mm disc (the fitted spheroid), rendered with a 1.5 mm rim glow; its arc per hop is `y(u) = 4·h·u(1−u)`, `u ∈ [0,1]`, with `h = 16 mm` at launch.
- **Touch splash**: at `u = 1`, an 18 mm ring and 8 droplets; splash height ≤ 12 mm so it never reaches the real table.
- **Trajectory ribbon**: the predicted path of the next 3 hops is drawn at 20 % opacity; it updates with each tap grade.
- **Mapping**: `x_k` accumulates along `+x̂_T`; `z_k` drifts with tap error; the stone's world position is `V + (x_k, 4h·u(1−u), z_k)`.
- **Camera note**: the pond is not head-locked; leaning in changes perspective but not the 400 mm scale, so pad sizes stay physically readable.

## 8. Gameplay Systems & Rules

- **Entities**: `Stone` (pinch, P), `SnapDetector`, `PhantomStone` (hop index, x, z, h, ω_s), `Pond` (pads, banks), `TapJudge`, `PondDirector`.
- **Pads**: a touchdown within 16 mm of a pad centre is a gate (+0.2 chain multiplier); within 16–26 mm is a graze (+0.05); outside is open water (no penalty, no bonus).
- **Chain multiplier**: starts at 1.0; each perfect tap +0.1; each gate +0.2; a miss resets to 1.0. Cap 3.0.
- **Sink**: two consecutive misses, or a hop with `h < 4 mm`, ends the run; the score is banked at the last gate.
- **Perfect chain**: all pads gated and zero misses → ×2 final pond score and a unique VFX.
- **Scoring**: per hop +10; perfect tap +15; gate +40; far bank +150; pond clear +200; perfect chain ×2; spin bonus `+floor(ω_s/10)` per hop.
- **Ponds**: 5; lengths 400 mm; pads 8/9/10/12/14; hop clock 0.44/0.43/0.42/0.41/0.40 s.
- **Wind**: from pond 4, a lateral drift acceleration of 0.20–0.35 mm/s² (level-scaled) with a 0.7 rad/s gust, which the player counters with tap timing (early = left, late = right).
- **Win/lose**: 5 ponds; win if ≥ 3 cleared; a failed pond may be retried once at the same level.
- **Assist**: after two consecutive failed ponds, the perfect window widens to ±90 ms and the good window to ±160 ms for the next pond only.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, sensor test | REGISTER |
| REGISTER | peg flat, pinched 1.0 s | center < 0.6 mm | set P, face normal | CALIBRATE |
| CALIBRATE | pond plane locked | table plane RMS < 1.5 mm | build V at +60 mm | BRIEF |
| BRIEF | 2.5 s card | — | pond 1 seed | ARM |
| ARM | pinch 11–15 mm | 80 ms stable | reticle on, snap buffer | ARM |
| ARM | snap detected | `v_s ∈ [0.4, 1.8]` | record peak, pitch | RELEASE |
| ARM | dry swing | no release in 120 ms | 0.6 s cooldown | ARM |
| RELEASE | pinch > 18 mm | within window | spawn phantom, hop 1 | SKIP |
| RELEASE | pinch held | — | abort, no penalty | ARM |
| SKIP | hop lands | — | splash, open tap window | SKIP |
| SKIP | perfect tap | `|Δt| ≤ 70 ms` | restore 92 %, +15, chain +0.1 | HOP |
| SKIP | good tap | `|Δt| ≤ 130 ms` | restore 75 %, +10 | HOP |
| SKIP | miss | `|Δt| > 130 ms` | restore 45 %, chain = 1.0 | HOP |
| HOP | hop < 10 and h ≥ 4 mm | — | advance x, z, hop++ | SKIP |
| HOP | h < 4 mm or 2 misses | — | sink animation | SCORE |
| HOP | x ≥ 400 mm | — | far bank +150 | SCORE |
| SCORE | pond < 5 | — | tally, 20 s rest | BRIEF |
| SCORE | pond = 5 or 3 fails | — | session stats | COMPLETE |
| any | prop lost > 200 ms | — | freeze phantom, ghost stone | REACQUIRE |
| REACQUIRE | relock ≤ 2.0 s | search 50 mm | hold hop state | prior |
| REACQUIRE | 2.0 s timeout | — | "present the stone" card | REACQUIRE |
| any | palm up 700 ms | — | freeze timers | SUSPENDED |
| SUSPENDED | palm down 500 ms | — | 1.5 s countdown | prior |

Timers: arm stable 80 ms; release window 120 ms; hop 0.42 s; perfect 70 ms; good 130 ms; dry-swing cooldown 0.6 s; pond rest 20 s; reacquire 2.0 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_stone` (spheroid), `COL_face_top/bot` (14 mm discs), `COL_pond` (plane), `COL_table` (plane), 14 pad discs, 1 far-bank box.
- **Pair matrix**: Thumb/Index↔Faces (pinch); Tapper↔Top face (tap); Phantom↔Pond (touch/splash); Phantom↔Pad (gate); Phantom↔Bank (finish); Phantom↔Hand (ignore — the phantom is virtual and may pass through the real hand); Stone↔Table (rest).
- **Phantom motion**: scripted per-hop parabola, not integrated gravity. `x_k`, `z_k`, `h_k` are state; `y(u) = 4h·u(1−u)`; `u = (t − t_k)/T_hop`. This guarantees a readable, tappable arc and avoids tunneling.
- **Drag / spin**: spin affects decay only, through `δ_eff`; no Magnus force (the lateral drift is wind, not spin).
- **CCD**: the phantom moves 26–58 mm per 0.42 s → 1.4 mm/frame; touchdown is a discrete event at `u = 1`, so no CCD is needed. Pad checks are point-in-disc at touchdown.
- **Materials**: stone↔skin µ = 0.42 (dry), 0.55 (damp); stone↔table µ = 0.5; the pond has no friction (virtual).
- **Forces**: pinch 2–6 N static; the wrist snap develops 0.15–0.45 N·s into the hand–arm (the 25 g stone never leaves the pinch, so no launch impulse is applied to the prop); tap impulse 0.02–0.06 N·s.
- **Pitch physics (virtual)**: `θ_p < 6°` sets `h_1 = 0` (dive); `6–24°` sets `h_1 = 16 mm`; `> 24°` sets `h_1 = 6 mm` and `S_1 ×0.6` (stall). This is a gameplay mapping of the real skip's pitch tolerance, not a fluid simulation.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → pond plane → pads → trajectory ribbon → phantom + splash → real-stone highlight → hands → HUD.
- **Occlusion cases**:
  1. *Pinching fingers*: thumb and index cover the stone's faces; the top-face tap target is drawn as a 10 mm ring on the fingertip's opposite side so the tapper can aim at the real stone, not at a hidden decal.
  2. *Phantom behind the hand*: the phantom is 60 mm above the table and may pass behind the holding hand; it is depth-tested and gets a 1.5 mm outline when occluded for > 200 ms.
  3. *Pond under the hand*: the pond plane is semi-transparent (35 %); the real table shows through, and the hand's shadow is drawn on the plane at 25 %.
  4. *Splash vs table*: splash droplets are clipped at `y_V = −2 mm` so they never appear to hit the real table; a 2 mm dark contact ring sells the touch.
  5. *Pads vs phantom*: pads are opaque and drawn before the phantom; a phantom over a pad draws its reflection at 25 % on the pad surface.
  6. *HUD at the far bank*: the score lane is at `y_V = 40 mm` and depth-tested; if the hand occludes it, it fades to 35 % rather than drawing through.
- **VFX budgets**: pond ripples 40; splash 8 per touch; pad bloom 24; dust motes 60; trail ribbon 64 segments; far-bank glow 1; global cap 320 particles / 700 ribbon vertices.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Pinch armed | stone | soft stone-on-stone click | face ring turns cyan |
| Snap | hand | air whoosh, pitch ∝ v_s | reticle streak, wind lines |
| Release | hand | short rising zing | phantom spawns, hop 1 arc |
| Dry swing | hand | hollow thud | reticle greys, cooldown arc |
| Water touch | pond | bright plink, pitch rises per hop | 18 mm ring, 8 droplets |
| Perfect tap | stone | glass chime + plink harmony | gold ring, chain +0.1 |
| Good tap | stone | wooden tock | white ring, +10 |
| Miss | stone | dull clack | red flash, chain resets |
| Pad gate | pad | marimba note, pitch per pad | pad blooms, +40 |
| Sink | pond | low bloop + fade | stone dissolves, score banks |
| Far bank | bank | gong + water surge | +150, tally card |
| Pond clear | room | shimmering chord | score card |

## 13. Difficulty Progression & Tuning

| Level | Pads | Hop clock (s) | Perfect window (ms) | Good window (ms) | Wind (mm/s²) | Launch gate (m/s) | Pitch band (°) |
|---|---|---|---|---|---|---|---|
| 1 | 8 | 0.44 | 90 | 160 | 0.00 | 0.35–1.8 | 5–26 |
| 2 | 9 | 0.43 | 80 | 145 | 0.00 | 0.35–1.8 | 5–26 |
| 3 | 10 | 0.42 | 70 | 130 | 0.00 | 0.40–1.8 | 6–24 |
| 4 | 12 | 0.41 | 70 | 130 | 0.20 | 0.40–1.8 | 6–24 |
| 5 | 14 | 0.40 | 60 | 115 | 0.35 | 0.45–1.8 | 7–23 |

- **Adaptive rule** (`SKIP_ADAPT`): after each pond, if cleared with ≥ 80 % pads and < 3 misses, level +1; if failed, level −1, hop clock +0.02 s, and the perfect window +15 ms. Clamp 1–5; one step per pond.
- **Named constants**: `A_STONE = 19.0 mm`, `C_STONE = 6.0 mm`, `M_STONE = 25 g`, `HOP = 0.42 s`, `H_0 = 16 mm`, `S_0 = 26 + 14·v_s mm`, `SPIN_GAIN = 3.4`, `OMEGA_S_MAX = 70 rad/s`, `DELTA = 0.11`, `PERFECT = 70 ms`, `GOOD = 130 ms`, `PAD_R = 14 mm`, `GATE_R = 16 mm`, `POND_L = 400 mm`, `DRIFT_MM_PER_MS = 0.18`.

## 14. Edge Cases & Failure Recovery

- **Snap without release**: dry swing, 0.6 s cooldown, no phantom, no score change; the reticle shows a grey arc.
- **Release without snap**: the pinch opens while `v_s < 0.4 m/s`; the phantom spawns with `S_0 = 18 mm` and a "soft toss" message; it is still playable but cannot reach the far bank.
- **Tap before the first touch**: if a tap occurs before hop 1 lands and within 210 ms of it, it is credited to that touch; earlier taps are ignored.
- **Double tap**: two taps within 120 ms are merged; the earlier timestamp is used, preventing a double energy restore.
- **Stone dropped**: registration lost; pause; on re-pinch, re-register and resume at the current hop with the current chain (no penalty for the drop itself).
- **Hand tracking loss**: if the tapper is lost, taps stop and the phantom proceeds on the miss path; if the launcher is lost, the pinch state freezes. Both lost > 1.0 s pauses the pond.
- **Phantom off the side of the pond**: if `|z| > 110 mm`, the stone is out; treat as a sink but keep the score up to the last gate.
- **Pond anchor drift**: if the table plane moves > 10 mm, re-anchor V and keep the phantom's relative x/z; a drift > 30 mm cancels the pond with a retry.
- **Very high spin**: `ω_s` is capped at 70 rad/s (11 rev/s); above this the virtual stone's rim would exceed 1.33 m/s and read as a blur, so the cap is a rendering constraint, not a physics one.
- **Pitch < 6°**: the dive is intentional and clearly signalled by a red pitch gauge during the snap; a first-time player gets a one-line "snap level" hint.
- **Recovery UX**: one icon (stone / hand / pond) and one line of text; no modal dialogs during a pond.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropStone` — pose P, face normal, long axis, pinch state, fit/reacquire.
  - `SnapDetector` — 200 ms centroid buffer, 55 ms velocity, peak speed, yaw rate, pitch, release gate.
  - `PhantomStone` — hop index, x/z/h, spin, `u(t)`, touchdown events.
  - `PondModel` — pond rectangle, pads, gating, far bank, wind.
  - `TapJudge` — tap timestamps, perfect/good/miss, energy restore, chain.
  - `PondDirector` — composition, scoring, adaptive level.
  - `SkipRig` — phantom mesh, arc, splash, pads, ribbon, banks.
- **Event flow**: `HandTracker → PropStone → SnapDetector → PhantomStone`; `HandTracker → TapJudge`; `PhantomStone → PondModel → TapJudge`; `TapJudge/PondModel → EventBus {Launch, Touch, PerfectTap, GoodTap, Miss, Gate, Sink, Bank, PondClear}` → `AudioBank / VFXPool / ScoreKeeper / HUD`.
- **Pseudocode — snap + release (per tick)**:

```text
dt = 0.0111
c = (p_thumb + p_index) / 2
buf.push({t: now, c: c})
buf = buf where t > now - 200ms
v = (buf[-1].c - buf_at(now - 55ms).c) / 0.055
v_h = |v.xz|; v_y = v.y
if v_h > peak:
    peak = v_h; t_peak = now; yaw = hand_yaw_rate(); pitch = atan2(v_y, v_h)
armed = (11mm <= pin_sep <= 15mm) for >= 80ms
if armed and pin_sep > 18mm and (now - t_peak) in [-40ms, 120ms]:
    if 0.4 <= peak <= 1.8 and 6deg <= pitch <= 24deg:
        launch(peak, yaw, pitch)
    else:
        soft_toss_or_dive(peak, pitch)
elif armed and not released and now - t_peak > 120ms:
    dry_swing(); cooldown = 0.6s
```

- **Pseudocode — hop + tap scoring (per tick)**:

```text
dt = 0.0111
if phantom.active:
    u = (now - hop_start) / HOP
    phantom.y = 4 * HOP_H * u * (1 - u)
    if u >= 1 and not hop_resolved:
        resolve_hop()
        open_window(center = now - 22ms, half = PERFECT, good = GOOD)
    if tap and window_open:
        dt_err = tap.t - window.center
        if |dt_err| <= PERFECT:
            HOP_H *= 0.92; S *= 0.92; score += 15; chain += 0.1; last = PERFECT
        elif |dt_err| <= GOOD:
            HOP_H *= 0.75; S *= 0.75; score += 10; last = GOOD
        else:
            HOP_H *= 0.45; S *= 0.45; chain = 1.0; last = MISS
        misses = (last == MISS) ? misses + 1 : 0
    if hop_resolved:
        hop_start = now; hop += 1
        x += S * cos(heading); z += S * sin(heading) + wind_drift(now)
        if x >= POND_L: bank(); state = SCORE
        elif HOP_H < 4mm or misses >= 2: sink(); state = SCORE
```

## 16. Acceptance Test Checklist

1. Registration fits the stone center to < 0.6 mm and the face normal to < 4° in 20/20 presentations; the 45 mm long axis is resolved correctly in 10/10.
2. Pinch separation 11–15 mm arms the stone; 10 mm or 16 mm does not; release at 18 mm fires with 8 mm hysteresis.
3. A 30 mm horizontal snap at 1.2 m/s produces `S_0 = 42.8 ± 1.5 mm` and `ω_s = 12 + 3.4·ω_y ± 3 rad/s`.
4. A release 150 ms after the peak is a dry swing; a release 60 ms after the peak launches.
5. Pitch 4° dives on hop 1; pitch 12° gives `h_1 = 16 mm`; pitch 30° stalls with `h_1 = 6 mm` and `S_1 ×0.6`.
6. A tap at `t_touch` (after −22 ms audio correction) scores perfect; at +80 ms it scores good; at +150 ms it misses.
7. A perfect tap restores hop height and length to 92 % ± 2 %; good to 75 % ± 3 %; miss to 45 % ± 3 %.
8. A touchdown within 16 mm of a pad centre gates it; at 20 mm it grazes; at 28 mm it is open water.
9. Two consecutive misses sink the stone; a perfect tap between them resets the miss count.
10. The perfect chain (all pads, zero misses) applies ×2 to the pond score and awards the unique bloom.
11. The hop clock is 0.42 s ± 5 ms at level 3; the tap window opens at `t_touch − 22 ms`.
12. Covering the stone for 1 s freezes the phantom and preserves the hop/chain; relock resumes within 200 ms.
13. A level-3 pond (10 pads, 0.42 s hop) is clearable with ≥ 8 gates in 5/5 scripted runs under 30 s.
14. The 5-pond session completes in ≤ 6 min with particle count ≤ 320 and no splash rendered below the table plane.
