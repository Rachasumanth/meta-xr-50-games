# Mystic Seer

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Mystic Seer |
| Slug | `mystic_seer` |
| Physical prop | Apple or smooth rubber ball, true sphere, r = 36.0 mm (72 mm diameter), 180 g; matte skin; alternate stress ball r = 32 mm, 60 g |
| Family / sub-shape | True sphere, palm-scale (6–12 cm class), stationary scrying orb |
| Scale class | Prop 1:1; fog shell 0.8 mm above the surface; hand envelope 120 mm above the ball; tabletop class |
| Source concept quote | "Set an apple on the table and cup your hands around it without touching. The fruit becomes a crystal ball drowned in purple mist. Your palms part the fog as they pass two centimetres above the skin, revealing spectral thieves pressed against the inside of the glass — and a fingertip tap on the exact spot banishes them." |
| Physical input | Two-handed curving sweeps at 10–30 mm standoff; index-fingertip tap on the upper hemisphere |
| Session length | 6 min: onboarding 50 s + 3 rounds × 75 s + 2 rests × 15 s + results |
| Lose condition | Three thieves escape in a single round (three relics stolen) |

## 1. Fantasy & Core Loop

You are a seer working a crystal orb that is also an apple. Purple mist fills the glass, and the mist is not scenery: it is a density field that your hands push aside. Two-handed passes carve tunnels through it, and in those clearings you see them — spectral thieves pressed flat against the inside of the sphere, each one a small patch of cold light at a known latitude and longitude. You have one gesture that matters: find the patch, and tap it exactly, before its escape ring closes. Tap the wrong place and the mist rushes back, thicker than before.

1. **HOVER (0.5 s).** Both hands enter the interaction shell 10–30 mm above the upper hemisphere. The fog reacts immediately; the near-surface layer begins to thin under each finger.
2. **SWEEP (0.6–1.2 s).** A curving pass at 20 mm standoff and 100–250 mm/s carves a band of clarity. Curvature above 0.2 mm⁻¹ earns a 1.3× suppression bonus. Both hands sweep independently.
3. **REVEAL (0.10 s).** When the fog at a thief's centre stays below 0.30 for 100 ms, the thief materialises and its 10 s escape ring starts.
4. **TAP (0.05–0.16 s).** The index fingertip descends to ≤ 5 mm from the surface and contacts within 10 mm of the thief centre. A hit banishes the thief; a miss smothers the area and costs score.
5. **RECOVER (1.5–2.0 s).** The fog regrows at 0.05/s base and 0.20/s inside a thief aura. If the thief is not banished in 10 s, it escapes and steals one of three relics.

A round is five thieves in 75 s. A clean round is five reveal-tap pairs at 8–12 s each; a sloppy round spends its time re-clearing fog after wrong taps.

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

`P` origin is the apple centre; poles are `±ẑ_P`, with `+ẑ_P` up at registration; radius `r = 36.0 mm`. The rolling law `ω = (n × v_t)/r` is evaluated on every contact and used only to classify a swipe (tangential contact) versus a tap (normal approach). `V` is anchored to the *position* of `P` but orientation-stabilised to gravity: `V_origin = P_centre`, `V` axes = `W` axes. A true featureless sphere has no observable yaw, so freezing `V` to world axes prevents the mist and thieves from spinning when the apple rocks. Deviations:

1. **Frozen orientation.** The fog grid and thieves are stored in `V`, not `P`; rotating the prop does not rotate the seance.
2. **The control is proximity, not contact.** Fog suppression is a Gaussian of the distance from each hand joint to the sphere surface; contact is only used for taps.
3. **Fog is the hidden state.** A 128 × 64 equirectangular density field is the authoritative gameplay state and is updated at 30 Hz, decoupled from the 90 Hz contact tick.
4. **No dynamics on the prop.** The apple is stationary and tracked; virtual physics covers only fog, thieves, and tap impulses.

## 3. Physical Play Space & Ergonomics

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated, forearms supported on the table edge | Hovering both hands for 75 s needs a stable base |
| Ball position | 260–320 mm from the sternum, on the table or a knee board | Comfortable downward gaze of 25–35° |
| Ball height | 50–70 mm below eye line | Keeps the upper hemisphere fully visible |
| Hand standoff | 10–30 mm; sweet spot 18–22 mm | Within the fog kernel and outside contact jitter |
| Sweep speed | 100–250 mm/s | Fast enough to carve, slow enough for 72 Hz sampling |
| Sweep curvature | 0.10–0.40 mm⁻¹ | A natural wrist arc; the bonus threshold at 0.20 mm⁻¹ |
| Tap travel | 15–25 mm descent, 40–160 ms contact | Short enough to avoid palm collapse |
| Session | 6 min; 3 × 75 s rounds; 15 s rests | Hover fatigue is the binding constraint |
| Break cue | 15 s "shake out" overlay between rounds | Relieves wrist extensors |

Both hands hover symmetrically; there is no dominant-hand requirement for sweeping. The tapping hand is whichever index tip arrives first, but the other hand must stay at least 25 mm from the ball during a tap, otherwise its proximity keeps the fog too dense for a clean reveal. The wrist stays in neutral extension; the sweep is driven from the elbow, not the fingers, to keep the fingertip height stable within ±3 mm.

The apple is chosen over a smaller ball because the 36 mm radius gives a 10 mm tap tolerance that corresponds to 15.9° of arc, which is comfortably larger than the 72 Hz tracking jitter of 2–3 mm. A 16 mm marble would still accept a 10 mm lateral error (71.6° of arc) but leaves no room for two-handed sweeps inside the interaction shell.

## 4. Prop Geometry & Registration

| Property | Apple (default) | Stress ball |
|---|---|---|
| Diameter | 72.0 mm | 64.0 mm |
| Radius `r` | 36.0 mm | 32.0 mm |
| Mass | 180 g | 60 g |
| Inertia `I = 2/5·m·r²` | 9.33 × 10⁻⁵ kg·m² | 2.46 × 10⁻⁵ kg·m² |
| Surface | Matte skin, µ = 0.50 | Matte foam, µ = 0.85 |
| Restitution | 0.18 | 0.30 |
| Registration features | Specular rim + silhouette | Silhouette + seam |
| Tap tolerance | 10 mm (15.9°) | 10 mm (17.9°) |

**Fog-field parameterization.** The field is an equirectangular grid over the full sphere: `N_λ = 128`, `N_φ = 64`, cell size 2.8125° × 2.8125°. At `r = 36 mm` a cell is 1.77 mm on the surface. Cell `(i,j)` maps to `λ_i = (i + 0.5)·2.8125°`, `φ_j = −90° + (j + 0.5)·2.8125°`. The field stores `F ∈ [0,1]`; 0 is clear glass, 1 is opaque mist.

**Thief patches.** Each thief is a spherical cap of radius 7.0 mm (`11.1°` at r = 36 mm), centred at a stored `(φ, λ)`. Five thieves per round, spawned with a minimum separation of 30° and outside a 25° cap around the player's eye direction so they are never on the far side at spawn.

**Surface-feature atlas (V-fixed):**

| Feature | Latitude φ | Longitude λ | Purpose |
|---|---|---|---|
| Upper interaction cap | +5° to +90° | all | Sweep and tap zone |
| Lower reserve | −90° to +5° | all | Fog visible, not interactable |
| Thief spawn band | +15° to +80° | all | Reachable by both hands |
| Eye exclusion | within 25° of eye axis | — | No spawn on the far side |
| Relic ring | +45° | 0°, 120°, 240° | Three relic markers |
| Prime meridian | all | 0° | Fog-grid debug ruler |

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Sphere | r = 36 mm at `P` | Physical prop / depth proxy |
| C1 | Spherical shell | `r + 0.8 mm`, thickness 0.4 mm | Fog visual shell |
| C2 | Spherical cap | 7.0 mm radius | Thief patch |
| C3 | Capsule | r = 1.8 mm, length 12 mm, per fingertip | Proximity source |
| C4 | Sphere | r = 5 mm | Palm proximity source |
| C5 | Disc | r = 10 mm, tangent at tap point | Tap acceptance |
| C6 | Sphere | r = 36 mm | Ball collider for tap height |

Registration: present at 0.8–1.2 m for 1.0 s, rotate through 120°, hold 0.3 s. Acceptance: RMS < 0.4 mm, radius error < 0.2 mm, ≥ 20 rim samples. Because the apple is smooth, yaw is not solved; `V` is gravity-aligned and the fog grid is frozen to `W`, so the missing yaw is harmless.

Re-acquisition: search a 100 mm cube around the last `P` for 1.0 s; success is 8 rim samples with RMS < 0.7 mm over 6 frames; snap 80 ms. If the apple rolls more than 20 mm from its rest point, the round pauses and a "return the orb" ring is shown. The fog field is preserved across re-acquisition.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Two-hand sweep | all fingertips + palm centres of both hands | φ +5° to +90°; standoff 10–30 mm | Fog suppression splats |
| I5.2 | Curving pass | same; path curvature > 0.20 mm⁻¹ | same | 1.3× suppression bonus |
| I5.3 | Index tap | `L_index_tip` or `R_index_tip` | upper cap; contact < 5 mm | Tap point, height, duration |
| I5.4 | Palm smother | `L_palm` or `R_palm` centre < 8 mm | upper cap | Fog +0.25 in a 30 mm cap; "too close" cue |
| I5.5 | Guard hand | non-tapping hand held 25–60 mm away | upper cap | Suppression only; no tap |
| I5.6 | Withdraw | all joints > 45 mm from surface | — | Fog regrows, no input |
| I5.7 | Double tap | same index, two taps 120–320 ms apart | same point ±6 mm | Treated as two taps; second is usually a miss |
| I5.8 | Prop nudge | palm contact moves the ball > 6 mm | any | Pause, "return the orb" |
| I5.9 | Drop / roll away | ball leaves the 20 mm rest disc | — | `ORB_LOST` recovery |

**Per-finger roles.** The index tip is the tap effector on both hands. The middle, ring, and pinky tips are sweep paddles: their proximity suppresses fog but their contacts are ignored for taps. The thumb is a stabiliser and must stay at least 15 mm from the surface during a tap; a thumb contact is treated as a palm smother. The palm centre contributes a wide, weak suppression splat (radius 18 mm, amplitude 0.35×) so a flat hand clears a broad area, while fingers carve fine channels.

Contact gating for taps: index contact must have signed distance < 5 mm, age 40–160 ms, travel < 4 mm during contact, approach speed > 30 mm/s, and no other fingertip within 8 mm of the same point. If two index tips tap within 120 ms and 6 mm, the first is scored and the second is ignored with a "one seer, one tap" cue.

## 6. Input Kinematics & Signal Processing

**Fog suppression kernel.** For each active sensor point `k` with surface projection `x_k` (the radial projection of the joint onto the sphere), surface distance `d_k = r·angle(x_k, cell)` and height `h_k = ||joint − centre|| − r`, the suppression rate at cell `c` is:

`S_k(c) = A · exp(−d_k²/(2σ_h²)) · exp(−h_k²/(2σ_z²)) · C_k`

with `A = 12.0 s⁻¹`, `σ_h = 9.0 mm`, `σ_z = 12.0 mm`, and `C_k = 1.3` for a curving pass, else `1.0`. Sensor points are the five fingertips and the palm centre of each hand (12 points total).

**Field update** at 30 Hz:

`F ← clamp(F + Δt·[R(x)·(1 − F) − Σ_k S_k·F], 0, 1)`

`R(x) = 0.05 s⁻¹` base, plus `0.20 s⁻¹` within 12 mm of a thief centre (the thief's concealment aura). The `(1−F)` and `F` factors make the field relax toward `F* = R/(R + ΣS)`; at 20 mm standoff with `S = 2.99 s⁻¹`, `F* ≈ 0.016`, so a 0.4 s dwell drives a cell from 0.90 to 0.28. At 25 mm standoff `S = 1.37 s⁻¹` and the same dwell only reaches 0.53, which is why the 2 cm pass is the taught technique.

**Curvature.** Fit a circle to the last 0.35 s of each hand's centroid path in the tangent plane. Curvature `κ = 1/ρ`; `C = 1.3` when `κ > 0.20 mm⁻¹` and the path is monotone. The bonus is capped so a tight jitter circle cannot farm suppression.

**Tap classifier.** A tap is a contact with `40 ms ≤ age ≤ 160 ms`, `|Δp| < 4 mm` during contact, and normal approach speed `> 30 mm/s`. The tap point is the surface projection of the index tip at the minimum-distance sample. Tap height is the signed distance at that sample; acceptance requires `h ≤ 5 mm` and lateral distance `≤ 10 mm` from the thief centre. Lateral distance is `r·acos(clamp(x̂_tap·x̂_thief, −1, 1))`.

**Thief reveal.** A thief is `REVEALED` when `F(centre) < 0.30` for 100 ms continuously. The escape timer starts at reveal, not at spawn; an unrevealed thief escapes at round end. If `F(centre) > 0.45`, the sprite fades to 25% but the timer continues.

Signal chain: joints at 72 Hz → One Euro (mincutoff 1.5 Hz, beta 0.02, derivative cutoff 6 Hz) → resample to 90 Hz → surface projection → fog update at 30 Hz → tap classifier at 90 Hz. Hand positions are clamped to a 140 mm shell around the ball so a tracking glitch cannot splat fog across the whole sphere. Classifiers emit at most one event per 90 Hz tick and debounce at 30 ms; the fog clock is a fixed 30 Hz accumulator, so a dropped frame never applies two 33 ms steps at once.

## 7. Virtual World Layout & Coordinate Mapping

`V` origin is the apple centre; `x̂_V` points away from the player, `ŷ_V` right, `ẑ_V` up. The fog shell is a sphere of radius `r + 0.8 mm`. The interaction cap is `φ ≥ +5°`. There is no camera movement; the orb is fixed in `V` and the player's head moves around it naturally.

| Entity | V-space position | Notes |
|---|---|---|
| Fog shell | radius 36.8 mm | 128 × 64 density grid |
| Thief patch | `r·(cosφ·cosλ, cosφ·sinλ, sinφ)` | 7 mm cap, 5 per round |
| Escape ring | tangent plane at thief centre | Radius grows 0→14 mm over 10 s |
| Relic markers | +45° at λ 0°, 120°, 240° | 3 per round; darken on escape |
| Tap acceptance disc | 10 mm tangent at tap point | Debug-only visual |
| Hand shell | 10–30 mm above surface | Soft cyan boundary when hands enter |
| HUD | 200 mm from the eye | Relic pips, round clock, chain |

Mapping formulas: thief centre direction `x̂_t = (cosφ·cosλ, cosφ·sinλ, sinφ)`; tap direction `x̂_p`; lateral error `e = r·acos(x̂_t·x̂_p)`; tap height `h = ||p_tip − centre|| − r`. A tap passes when `e ≤ 10 mm`, `h ≤ 5 mm`, the thief is `REVEALED`, and no other tap is pending. The banish VFX snaps the thief sprite to the tap point when `e ≤ 10 mm` so the visual and the judgment agree.

Scale: a 10 mm lateral tolerance is 27.8% of the radius, a deliberately generous target; the difficulty comes from fog management and the 10 s timer, not from pixel hunting. The 20 mm standoff is 0.56 r, comfortably outside the 5 mm contact shell.

## 8. Gameplay Systems & Rules

**Entities.** Five thieves per round, each with `(φ, λ)`, state `HIDDEN | REVEALED | BANISHED | ESCAPED`, and a 10 s timer. Three relics per round. Fog field 128 × 64. Player score and chain.

**Spawn.** At round start, five directions are drawn with a minimum pairwise separation of 30° and at least 25° from the eye axis. All start `HIDDEN` with `F = 0.95` in a 9 mm cap around the centre. The rest of the field starts at `F = 0.75 ± 0.08` with low-frequency noise.

**Reveal.** As §6. On reveal: whisper sting, escape ring starts, thief sprite appears at 0.35 alpha rising to 1.0 over 120 ms, and the HUD marks one of five slots.

**Banish.** A valid tap on a revealed thief: score `150·(1 + 0.5·(1 − F_centre))·chain_mult + 50·(t_remaining/10)`. `chain_mult = 1 + 0.2·min(chain,5)`. Chain increments; timer stops; fog in a 12 mm cap is set to 0.15 (a clear hole that refills to the reveal threshold in ≈ 5 s).

**Wrong tap.** A valid tap that does not match a revealed thief within 10 mm: fog `+0.25·exp(−e²/(2·20²))` in a 30 mm cap; a 0.8 s smother floor of `F ≥ 0.45` in that cap; score −25; chain = 0. The smother floor prevents an immediate follow-up tap from succeeding, which is the point.

**Escape.** At 10 s after reveal, or at round end if never revealed, the thief escapes: one relic darkens, a purple shockwave raises fog by 0.35 in a 25 mm cap, and the round's escape count increments. Three escapes in a round ends the session.

**Round and session.** 75 s per round, five thieves. Clear all five to advance. Three rounds. The session is won if all three rounds are cleared; any round with three escapes is a loss. Unrevealed thieves at 75 s count as escapes, so a passive player cannot wait out the clock.

**Score.** Banish base 150, clarity bonus up to 75, speed bonus up to 50, chain up to 2.0×. A perfect round (five banishes, no wrong tap, all under 5 s) is ≈ 2000 plus a 500 round bonus.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load fog grid, start tracker | PRESENT |
| PRESENT | prop registered | RMS < 0.4 mm | Freeze `V`, show orb | CALIBRATE |
| CALIBRATE | both hands enter shell | standoff 10–30 mm | Teach one sweep, zero score | ROUND_INTRO |
| ROUND_INTRO | 2.0 s timer | round ≤ 3 | Spawn 5 thieves, fill fog | SEANCE |
| SEANCE | fog update tick | — | Apply splats, regen, check reveal | SEANCE |
| SEANCE | `F(centre) < 0.30` for 100 ms | thief HIDDEN | Set REVEALED, start 10 s timer | REVEAL |
| REVEAL | tick | timer < 10 s | Update ring, fade sprite by fog | REVEAL |
| REVEAL | valid tap within 10 mm | e ≤ 10, h ≤ 5 | Banish, score, chain++ | SEANCE |
| SEANCE | valid tap, no match | — | Smother, −25, chain = 0 | SMOTHER |
| SMOTHER | 0.8 s timer | — | Hold fog floor | SEANCE |
| REVEAL | timer = 10 s | — | Escape, relic −1, fog burst | ESCAPE |
| ESCAPE | 0.6 s timer | escapes < 3 | Resume | SEANCE |
| ESCAPE | escapes = 3 | — | Relic shatter, dirge | GAME_OVER |
| SEANCE | 5 thieves resolved | escapes < 3 | Round bonus | ROUND_END |
| ROUND_END | 2.5 s timer | round < 3 | Next round | REST |
| ROUND_END | round = 3 | — | Results | RESULTS |
| ANY | orb moved > 20 mm | — | Pause, return ring | ORB_LOST |
| ORB_LOST | re-registered < 5 s | — | Restore fog, 1 s grace | previous |

The fog update runs at 30 Hz but the reveal and tap checks run at 90 Hz against the latest field; a tap is judged with the field value from the most recent 30 Hz frame, which is at most 33 ms stale.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 orb | Sphere r = 36 mm | Static/tracked | Depth proxy |
| C1 fog shell | Sphere r = 36.8 mm | Static | Visual + field sample |
| C2 thief | Spherical cap 7 mm | Static | Reveal/banish target |
| C3 fingertip | Capsule r = 1.8 mm | Tracked | Proximity and tap |
| C4 palm | Sphere r = 5 mm | Tracked | Wide splat |
| C5 tap disc | Disc r = 10 mm | At tap point | Acceptance test |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C3 × C0 | signed distance < 5 mm | Contact; tap candidate |
| C3 × C2 | `e ≤ 10 mm` and `h ≤ 5 mm` | Banish if revealed |
| C4 × C0 | centre distance < `r + 8 mm` | Smother splat |
| C2 × C1 | field sample at thief centre | Reveal check |
| C2 × C3 | any contact outside 10 mm | Wrong-tap smother |
| C0 × table | tracked rest | No virtual response |

**Materials.** Orb = "cold glass": restitution 0.18, µ 0.50, no deformation. Fog = "phantom mist": no mass, pure scalar field, no momentum. Thief = "spectral": no collision response, despawns on banish. Tap impulse = cosmetic 0.4 mm surface ripple, 180 ms decay.

**CCD.** The fastest fingertip is 800 mm/s (7.2 mm per tick). The tap disc is 10 mm and the contact shell is 5 mm, so a fast stab could tunnel through the contact band. The system uses a swept capsule against the sphere and samples the minimum signed distance along the sweep; the tap height is taken at that minimum. Fog splats use the swept path, not just the endpoint, so a fast pass does not skip cells.

**Fog numerical stability.** The explicit update is stable because `Δt·(R + ΣS) ≤ 0.0333 × (0.25 + 12×1.3) = 0.52 < 1`. The kernel is truncated at 3σ (27 mm surface distance, 36 mm height), which covers an 11 × 11 cell neighbourhood; cells outside receive zero. A 3 × 3 blur runs every 0.5 s with weight 0.15 to prevent splat banding.

**Tap force.** A tap is a 40–160 ms contact decelerating the index from 30–120 mm/s to rest. With an effective finger mass of 8 g, the impulse is `J = m·v ≈ 0.008 × 0.08 = 6.4 × 10⁻⁴ N·s`; the cosmetic surface ripple amplitude is `0.4 mm × (J/6.4 × 10⁻⁴)`, capped at 0.6 mm. This is a visual proxy only; no force is fed back to the tap test, because the 5 mm contact gate is the authoritative threshold.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → table/scene mesh → orb body → fog back shell → thieves (back) → fog front shell → thieves (front) → hand mesh → tap ripple → HUD. The fog is a single shell with front and back faces rendered separately so hands can sit inside the mist.

**Occlusion cases.**

- **O1 — Hand inside the fog shell.** A hand at 20 mm standoff is *above* the shell; it is drawn after the fog and correctly occludes the mist behind it. A hand at < 0.8 mm would be inside; the shell fades to 0.25 alpha within 3 mm of any hand surface so fingers never look painted purple.
- **O2 — Thief behind the orb.** A thief on the far hemisphere is depth-tested against the 36 mm orb proxy and hidden. Its reveal state still advances if fog clears there, but the player cannot tap it; spawn rules keep all thieves in the upper cap.
- **O3 — Fog over the apple skin.** The shell is offset 0.8 mm and depth-tested; the peel texture remains visible through thin fog (opacity 0.15 at F = 0) and disappears at F > 0.85. No fog is drawn on the lower hemisphere below the table line.
- **O4 — Finger tapping through the fog.** The tapping index descends through the shell; the shell is clipped against the hand mesh, so the fingertip appears to pierce the mist rather than float on it. The tap ripple is drawn on the orb surface at 0.5 mm height.
- **O5 — Other hand behind the orb.** The guard hand on the far side is occluded by the orb body; its suppression splat still applies (proximity is 3D), but a faint cyan contour is drawn on the near shell so the player knows the far hand is contributing.
- **O6 — HUD versus hands.** Relic pips and the round clock are at 200 mm and depth-tested against both hands; a crossing hand covers them and they fade to 35%, never draw through.

**VFX budgets.** Fog shell: one 128 × 64 texture, 1 draw call, 2.8 ms; thief sprites ≤ 5; escape rings ≤ 5; tap ripples ≤ 4 concurrent; mist particles ≤ 300 (cosmetic only, never authoritative). The fog shader samples the density texture with bilinear filtering and adds a 0.5 mm parallax offset to give volume without a raymarch.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Hand enters shell | hand | Soft airy whoosh, 180 ms | Cyan contour on the shell |
| Sweep | hand path | Filtered noise, cutoff ∝ speed | Fog peels away in a band |
| Curving-pass bonus | hand path | Chime +2 semitones | Gold edge on the cleared band |
| Thief reveal | thief centre | Whisper + 1.6 kHz sting | Sprite fades in, ring starts |
| Thief breathing | thief centre | 0.8 Hz filtered breath, gain ∝ (1−F) | Ring pulses |
| Banish | tap point | 2.2 kHz bell + reverse reverb, 500 ms | Thief collapses into a light streak |
| Wrong tap | tap point | Muffled 120 Hz thud | Purple puff, fog floor ripple |
| Smother | tap point | Lowpass swell, 400 ms | Fog thickens, screen edges darken |
| Escape warning | thief centre | Ring tone accelerating 1→4 Hz | Ring turns red, sprite flickers |
| Thief escapes | thief centre | Descending gliss + relic crack | Relic pip shatters, shockwave |
| Round clear | orb | 4-note arpeggio | Fog lifts, relics glow |
| Relic lost | relic marker | Glass crack, 300 ms | Pip goes dark |
| Orb moved | ball | Wooden slide | Return ring on the table |

The fog is sonified: the noise cutoff rises with local clarity, so the player can hear a clearing before seeing it. The thief breath is the only continuous spatial cue and is deliberately quiet until the fog is below 0.6.

## 13. Difficulty Progression & Tuning

| Level | Thieves | Escape timer (s) | Base regen (s⁻¹) | Aura regen (s⁻¹) | Thief radius (mm) | Fog start |
|---|---|---|---|---|---|---|
| 1 | 4 | 14 | 0.03 | 0.12 | 8.0 | 0.65 |
| 2 | 5 | 12 | 0.04 | 0.16 | 7.5 | 0.70 |
| 3 | 5 | 10 | 0.05 | 0.20 | 7.0 | 0.75 |
| 4 | 6 | 9 | 0.06 | 0.24 | 6.5 | 0.80 |
| 5 | 6 | 8 | 0.07 | 0.28 | 6.0 | 0.85 |

Named constants: `GRID_L = 128`, `GRID_P = 64`, `SIGMA_H = 9 mm`, `SIGMA_Z = 12 mm`, `A_SPLAT = 12 s⁻¹`, `CURVE_GAIN = 1.3`, `REVEAL_F = 0.30`, `REVEAL_HOLD = 100 ms`, `HIDE_F = 0.45`, `TAP_LATERAL = 10 mm`, `TAP_HEIGHT = 5 mm`, `TAP_MS = 40–160`, `SMOTHER_FLOOR = 0.45`, `SMOTHER_TIME = 0.8 s`, `RELICS = 3`.

Adaptive rule: if a player clears a round with > 30 s left and no wrong taps, reduce the next escape timer by 1.0 s (floor 8 s) and raise aura regen by 0.02 (cap 0.28). If they lose two relics in a round, add 1.5 s to the next timer (ceiling 14 s) and lower the fog start by 0.05. The 10 mm tap tolerance never changes; the challenge is fog and time, not precision.

## 14. Edge Cases & Failure Recovery

- **Player taps with a curled finger.** The contact point is the index tip, but if the `dip` angle > 55° the tap is classified as a knuckle and rejected with a "fingertip" cue; no fog penalty. This prevents accidental smothers during a tight grip.
- **Both index fingers tap the same thief.** The first valid tap banishes it; the second lands on empty fog and is *not* penalised if it occurs within 120 ms, because it was aimed at a real target that vanished. The exemption is one per banish.
- **Tap at exactly 5.0 mm.** Inclusive by spec. The contact threshold is also 5 mm, so a tap that never enters contact is impossible; if the tracker reports 5.1 mm, it is a miss and a smother.
- **Fog never clears because both hands are too high.** At 30 mm the suppression is `12·exp(−900/288) = 0.53 s⁻¹`, below the aura regen of 0.20 plus base 0.05 only near a thief; away from a thief it still clears slowly. The HUD shows a standoff gauge that turns amber above 25 mm.
- **Hand tracking loses a finger behind the other hand.** The missing sensor point contributes no splat for up to 300 ms; after that the hand's last valid splats are decayed. The fog does not jump.
- **Orb knocked over.** Movement > 20 mm pauses the round, freezes the fog, and shows a return ring. The fog state is preserved exactly. If the orb is re-registered within 5 s, play resumes with a 1 s grace; otherwise the round restarts.
- **Thief spawns under the guard hand.** Spawn rejection checks current hand positions and rejects any point within 15 mm of a hand's surface projection; if all candidates fail, the spawn is delayed 0.5 s.
- **All fog cleared.** The field can reach 0; the shell becomes invisible but the thieves are fully lit. There is no bonus for total clearance, and regen immediately begins.
- **Player smothers the same spot repeatedly.** The smother floor stacks to a maximum of 0.75 and the score penalty is applied each time; the UI shows a "calm down" cue after three consecutive wrong taps.
- **Round clock expires during a tap.** The tap is resolved if its contact began before 75.0 s; the round-end check runs after tap resolution in the same tick. This avoids stealing a legitimate last-moment banish.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — sphere pose, rest-disc check, re-acquisition.
- `HandShell` — projects 26 joints to the sphere, computes standoff, curvature, tap candidates.
- `FogField` — 128 × 64 density buffer, splat kernel, regen, blur, sampling.
- `ThiefDirector` — spawn, reveal state, escape timers, relic count.
- `TapJudge` — tap classification, lateral/height tests, wrong-tap smother.
- `SeanceScoring` — chain, clarity/speed bonuses, penalties.
- `RoundStateMachine` — §9 table, 75 s clock.
- `FeedbackRouter` — event → audio + VFX, coverage assertion.

**Event flow.** `PropTracker` → `HandShell` → `FogField` (30 Hz splats) → `ThiefDirector` (reveal); `HandShell` → `TapJudge` → `ThiefDirector` and `FogField` (smother); `ThiefDirector` → `SeanceScoring`; `RoundStateMachine` reads timers and relic counts; all events → `FeedbackRouter`.

**Pseudocode 1 — fog tick.**

```
function fog_tick(dt):                      # 30 Hz
    for cell in grid:                       # 128*64
        R = 0.05
        for t in ThiefDirector.thieves:
            if angle(cell, t.dir) < 12mm/r: R += 0.20
        S = 0
        for p in HandShell.sensors:         # 12 max
            d = r * angle(cell, p.surface_dir)
            h = p.radius - r
            if d < 27mm and h < 36mm:
                g = exp(-d*d/(2*9mm^2)) * exp(-h*h/(2*12mm^2))
                S += 12.0 * g * p.curve_gain
        F[cell] += dt * (R*(1 - F[cell]) - S*F[cell])
        F[cell] = clamp(F[cell], 0, 1)
```

**Pseudocode 2 — reveal and tap.**

```
function tick(dt):
    update_thieves(dt)
    for t in thieves:
        f = FogField.sample(t.dir)
        if t.state == HIDDEN and f < 0.30:
            t.reveal_hold += dt
            if t.reveal_hold >= 0.10s:
                t.state = REVEALED
                t.timer = 0
                Feedback.play(REVEAL, t.pos, gain=1 - f)
        elif t.state == HIDDEN:
            t.reveal_hold = 0

        if t.state == REVEALED:
            t.timer += dt
            if t.timer >= escape_time: escape(t)

    for tap in TapJudge.pending():
        best = null
        for t in thieves where t.state == REVEALED:
            e = r * acos(dot(tap.dir, t.dir))
            if e <= 10mm and tap.height <= 5mm and e < best.e:
                best = t; best.e = e
        if best:
            banish(best, tap)
            TapJudge.consume(tap)
            SeanceScoring.on_banish(best, FogField.sample(best.dir))
        else:
            FogField.smother(tap)
            SeanceScoring.on_wrong(tap)
            Feedback.play(WRONG_TAP, tap.point)
```

**Determinism.** Thief spawn uses PCG32 keyed by `(session, round)`. Fog integration is fixed at 30 Hz, splats are accumulated in a deterministic order (left hand joints, then right, ascending joint ID), and tap resolution is sorted by contact timestamp. Replaying a joint trace reproduces the fog field and the banishes bit-for-bit.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Fog equilibrium | Hold one fingertip at 20 mm standoff over a cell for 2 s | `F` converges to 0.016 ± 0.005; no oscillation |
| AT-02 | Standoff falloff | Repeat at 5, 20, 25, 30 mm for 0.4 s | Drops to ≤ 0.05, 0.28, 0.53, 0.72 respectively ± 0.05 |
| AT-03 | Curvature bonus | Straight pass vs curvature 0.3 mm⁻¹ at 20 mm | Curved pass clears 30% ± 5% more cells below 0.30 |
| AT-04 | Reveal gate | Hold `F = 0.29` for 90 ms, then 110 ms | No reveal at 90 ms; reveal at 100 ms ± 1 fog frame |
| AT-05 | Tap lateral boundary | Tap at e = 9.9 mm and 10.1 mm, h = 3 mm | 9.9 banishes; 10.1 smothers and applies −25 |
| AT-06 | Tap height boundary | Tap at h = 4.9 mm and 5.1 mm, e = 2 mm | 4.9 banishes; 5.1 is not a contact and produces no banish |
| AT-07 | Wrong-tap smother | Tap empty fog twice within 0.5 s | First sets floor 0.45; second cannot reveal; score −50 total |
| AT-08 | Escape timer | Reveal a thief, apply no taps | Escape at 10.0 s ± 1 tick; relic count −1; fog burst +0.35 |
| AT-09 | Three escapes | Force three escapes in one round | `GAME_OVER` on the third; no fourth escape event |
| AT-10 | Unrevealed at round end | Leave one thief hidden to 75 s | It counts as an escape; round cannot clear with it hidden |
| AT-11 | Fog determinism | Replay a 30 s hand trace twice | Identical hashes of the 8192 cells quantised to 10⁻⁴ at every 0.5 s; identical reveal times |
| AT-12 | Fog occlusion | Place a hand at 1 mm standoff and a thief behind the orb | Shell alpha ≤ 0.25 at the hand; thief hidden by orb depth |
| AT-13 | Tap through shell | Descend index from 30 mm to 3 mm over 120 ms | Fog clears along the swept path, not just the endpoint; tap height uses the minimum |
| AT-14 | Audio coverage | Enumerate `FeedbackEvent` | 13/13 mapped; reveal gain tracks `1 − F` within 0.05 |

All tests run headless with synthetic two-hand skeletons and a deterministic 30 Hz fog clock. AT-11 compares SHA-256 hashes of the 8192-cell buffer; AT-12 reads the depth and alpha buffers at specified pixels.
