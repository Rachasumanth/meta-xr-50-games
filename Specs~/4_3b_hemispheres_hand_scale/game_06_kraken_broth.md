# Game 06 — Kraken Broth — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Kraken Broth |
| Slug | `kraken-broth` |
| Physical prop | Upright soup bowl: rim radius 80 mm, cavity depth 60 mm, wall 4 mm, ~400 g, foot ring 36 mm radius; sits stationary on the lap |
| Family / sub-shape | Family 4 / 4.3b Hemispheres — Hand-Scale / bowl-up hemispherical shell, exterior-belly drum input |
| Scale class | 1:1 physical prop; virtual ocean disc 164 mm across; seated two-hand play |
| Source concept | "A stormy ocean fills the bowl. Miniature pirate ships sail the rim, and drumming a wall quadrant summons a Kraken tentacle from the centre to slam that rim sector." |
| Source physical input | Drum the fingertips on the exterior belly in the front / left / back / right quadrants; rhythm and impact speed set the charge |
| Session length | 8 min: onboarding 40 s + 5 broths × 55 s + 4 intermissions × 8 s + results 30 s |
| Win condition | Land the fifth broth with storm surge below 100 % |
| Lose condition | Storm surge reaches 100 % on any broth |

## 1. Fantasy & Core Loop

**Fantasy.** The soup is a black storm sea. A lantern-lit pirate fleet rides the rim, and somewhere under the broth the Kraken waits for a drumbeat. Each quadrant of the bowl's belly is a drum head wired to one of the beast's arms: strike the left wall and the left arm rises; strike it hard and fast and the arm comes down on the left rim before the ships can tack away. The bowl is not a screen. It is the hull of the world, and the player is both storm and god.

**Core loop** (one drum phrase ≈ 2.0–3.0 s; one broth ≈ 55 s):

1. **READ** (0.4–0.8 s) — count ships in the four 90° rim sectors and note armoured hulls.
2. **CHOOSE** (0.2–0.4 s) — pick the quadrant whose sector holds the highest-value cluster; the sector is drawn as a surface tide-line on the belly.
3. **DRUM** (0.9–1.6 s) — 4–7 fingertip strikes on that exterior quadrant; alternating fingers earn a 15 % rhythm bonus per strike.
4. **ARM** (0.45 s) — at 100 charge the quadrant glows abyssal red and a tentacle tip breaks the broth surface at the bowl centre.
5. **SLAM** (0.12 s) — the arm whips out along the quadrant's bisector and crushes every ship inside that 90° rim sector.
6. **SETTLE** (0.6 s) — the arm withdraws, the broth splashes, surge drops 6 %, the quadrant cools for 1.8 s.
7. **SURGE** — the sea state climbs continuously; every escaped ship adds 12 %. The player must keep slamming just to hold the storm back.

A clean broth is eight slams, each killing 2–4 ships, with surge never above 60 %. A panicked broth is single-ship panic slams, empty sectors, and a boil-over at 100 %.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Hemisphere sub-shape**: the prop is a hemispherical shell of radius `R` and wall thickness `t`; model in P with the rim circle in the `x̂_P`–`ŷ_P` plane at `z = 0` and the apex at `z = ±R`. For **bowl-up** props the cavity faces `+ẑ_P`; for **dome-up** props the convex dome faces `+ẑ_P`. Inner surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`, θ ∈ [0°, 90°], λ ∈ [0°, 360°). Rim torus: major radius `R`, minor radius `t/2`; foot ring (if present) is a torus of major radius `0.45R` on the opposite side.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P frame**: origin at the rim-circle centre, `+ẑ_P` = cavity opening (up), `+x̂_P` = the player's horizontal forward at calibration, `+ŷ_P` = player's left. Rim plane `z = 0`; cavity apex at `z = −d`.
- **Numbers**: rim radius `R = 80.0 mm`, cavity depth `d = 60.0 mm`, wall `t = 4.0 mm`, mass `m = 400 g`, foot-ring major radius `36.0 mm` (= 0.45R), minor radius `2.0 mm`. The inner concave surface is a spherical cap of radius `R_s = (R² + d²)/(2d) = 83.33 mm` centred at `(0, 0, R_s − d) = (0, 0, 23.33 mm)`; the rim half-angle is `θ_max = asin(R/R_s) = 73.74°`.
- **Surfaces**: inner `p_i(θ,λ) = (R_s sinθ cosλ, R_s sinθ sinλ, R_s(1 − cosθ) − d)`, θ ∈ [0°, 73.74°]; outer `p_o(θ,λ)` uses `R_s + t` with the same centre, giving a 4.15 mm-wide rim annulus at `z = 0`. Rim torus major `R = 80 mm`, minor `t/2 = 2 mm`, in the `z = 0` plane. Foot ring major `36 mm`, minor `2 mm`, centred on the outer apex at `z = −62 mm` so its contact plane is `z = −64 mm`.
- **V frame**: anchored rigidly to `P`. The ocean, ships, tentacle, and sky all live in `P`, so moving the lap moves the whole diorama coherently. Only the HUD and the storm vignette are camera-anchored.
- **Deviations**: (a) the bowl is stationary and never simulated as a free body; `P` is tracked, and lap breathing is low-pass filtered at 0.7 Hz for the virtual sea plane so the horizon does not jitter at respiratory rate; (b) drum contacts are tested against the **outer** shell SDF, not the rim or cavity, with a dedicated belly band `θ ∈ [35°, 62°]`; (c) quadrant assignment uses ±4° hysteresis at the 45° boundaries so a finger rolling across a seam cannot fire two arms; (d) the broth surface is a flattened ellipsoid 6 mm above the rim plane, and ships are drawn with a 2 mm waterline cut so they read as floating, not glued.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated, bowl on the lap 170–240 mm below the chin, centred on the sternum; both forearms free, elbows at 100–120°.
- **Bowl stability**: the foot ring sits on the lap; a silicone lap mat is not required, because the bowl is never pushed laterally, only struck downward-inward on the belly.
- **Hand access**: the rim is 80 mm from the axis, so the belly band at `z ∈ [−49, −18] mm` is reachable by both hands without crossing the midline.
- **Strike geometry**: fingertips approach the belly at 35–60° below horizontal; the valid contact normal points inward and slightly up. A flat palm slap is legal but capped (I5.7).
- **Reach envelope**: fingertips 190–360 mm from the shoulders; the maximum cross-body reach is 150 mm, so the back quadrant is struck with the same-side hand rotating the forearm, not by reaching around.
- **Session length**: 8 min default; hard stop 11 min. One broth is 55 s, which is 25–40 strikes at 0.7–1.4 strikes/s — below the 2 Hz fatigue threshold for finger drumming.
- **Fatigue mitigations**: (1) alternating-finger bonus rewards relaxed rolls, not force; (2) the palm-slap alternate distributes load to the thenar pad; (3) a 8 s intermission lowers both hands below the rim; (4) no sustained press is ever required.

| Parameter | Value | Rationale |
|---|---|---|
| Bowl axis height | 170–240 mm below chin | Keeps the rim in the lower 25° of the FOV, ocean readable |
| Belly strike band | `z = −49 … −18 mm` | Above the lap occlusion, below the rim spill |
| Preferred strike rate | 0.7–1.4 /s | Max voluntary fingertip drumming without forearm recruitment |
| Max simultaneous ships | 7 | Keeps the 90° sector readable at 164 mm ocean diameter |
| Hand-to-bowl clearance | ≥ 25 mm | Avoids accidental rim contact during approach |

## 4. Prop Geometry & Registration

| Property | Value | Notes |
|---|---|---|
| Rim radius `R` | 80.0 mm | Measured at the inner lip; ±1.0 mm tolerance |
| Cavity depth `d` | 60.0 mm | Vertex to rim plane |
| Wall `t` | 4.0 mm | Ceramic; 4.15 mm rim annulus |
| Inner sphere radius `R_s` | 83.33 mm | Derived from `R` and `d` |
| Inner sphere centre | `(0, 0, 23.33) mm` | In `P` |
| Rim half-angle `θ_max` | 73.74° | `asin(R/R_s)` |
| Mass | 400 g | Filled to 60 % with water in play |
| Foot ring | major 36 mm, minor 2 mm | Contact plane `z = −64 mm` |
| Glaze friction | µ = 0.42 dry, 0.25 wet | Wet hands reduce tap energy 15 % |

**Surface-feature atlas (P-fixed; λ from `+x̂_P` toward `+ŷ_P`):**

| Feature | θ | λ | Extent | Purpose |
|---|---|---|---|---|
| Ocean centre | — | — | r < 20 mm | Tentacle emergence point |
| Front quadrant | 35°–62° | 315°–45° | 90° belly patch | Q_F drum target |
| Left quadrant | 35°–62° | 45°–135° | 90° belly patch | Q_L drum target |
| Back quadrant | 35°–62° | 135°–225° | 90° belly patch | Q_B drum target |
| Right quadrant | 35°–62° | 225°–315° | 90° belly patch | Q_R drum target |
| Rim sector 0–7 | 90° | 45° steps | 45° each | Ship lanes and slam sectors |
| Rim torus | 90° | all | 4 mm band | Bell / spill line |
| Foot ring | — | all | r = 36 mm | Lap contact, no gameplay |
| Quadrant seam | 35°–62° | 0°, 90°, 180°, 270° | 3 mm line | Visible drum boundary |
| Tide gauge | 62°–70° | 90° | 12 × 30 mm | Surge readout, emissive |

The atlas is sampled at 64 × 32; at the equator one texel spans `2πR/64 = 7.85 mm`, larger than a fingertip contact (≈ 9 mm), so the seam lines are drawn as geometry, not texture, to stay crisp.

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Outer shell SDF | `R_s + t` about `(0,0,23.33)` | Belly drum surface |
| C1 | Inner shell SDF | `R_s` about the same centre | Ocean boundary, rejects inside taps |
| C2 | Rim torus | major 80, minor 2 mm | Spill line, palm rest |
| C3 | Foot ring torus | major 36, minor 2 mm | Lap contact only |
| C4 | Quadrant volumes | 4 × 90° × θ 35°–62° | Tap classification |
| C5 | Rim sector patches | 8 × 45° annuli, width 6 mm | Slam hit test |
| C6 | Tentacle capsule | r = 6 mm, length 90–150 mm | Slam sweep |
| C7 | Ship capsule | r = 4 mm, length 12 mm | Target |
| C8 | Hand capsules | 26 joints | Contact only |

**Registration.** The player sets the bowl down and holds both palms at the rim for 0.8 s. The fitter samples 48 rim points and 96 belly points, fits the sphere `(R_s + t)` and the rim plane by least squares, and accepts at RMS < 1.2 mm with rim-normal error < 2.0°. Because the bowl is rotationally symmetric, `λ = 0` is defined by the head's horizontal forward vector at the instant the fit completes; a 3 s "front" arrow confirms it. `P` is published at 72 Hz. If the bowl is later rotated on the lap, `P` rotates with it and the quadrants rotate too — correct, because the ocean is in the bowl.

**Re-acquisition.** Search an 80 mm sphere around the last `P` for 1.2 s; refit with 24 rim points; accept at RMS < 1.6 mm; blend pose over 150 ms. A bowl lifted more than 120 mm above `T` pauses the broth (the ocean would spill).

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Front drum | any of `L/R_index_tip`, `middle_tip`, `ring_tip` | C0, θ 35°–62°, λ 315°–45° | +charge to Q_F |
| I5.2 | Left drum | same, same-side or cross-body | C0, λ 45°–135° | +charge to Q_L |
| I5.3 | Back drum | same | C0, λ 135°–225° | +charge to Q_B |
| I5.4 | Right drum | same | C0, λ 225°–315° | +charge to Q_R |
| I5.5 | Alternating roll | two different fingertips, inter-tap 90–260 ms | any quadrant | +15 % energy per strike |
| I5.6 | Palm slap | `L/R_palm` centre | C0, any quadrant | +34 charge, 350 ms quadrant lockout |
| I5.7 | Rim rest | `L/R_thumb_*` on C2 | rim torus | grip reference; no charge |
| I5.8 | Lift / spill | bowl apex > 120 mm above `T` | — | pause broth, freeze surge |

Per-finger roles: index, middle, and ring are the drumsticks; the pinky is legal but contributes ×0.7 because its typical strike speed is 30 % lower. The thumb never drums — it is the rim anchor that keeps the hand from sliding into the cavity. Per-hand roles: both hands are symmetric drummers; the left hand owns Q_L/Q_F by default and the right hand Q_R/Q_B, but any hand may strike any quadrant.

Contact gating: a strike is valid when signed distance < 5 mm, contact age < 90 ms, inward normal component `n̂·(v_j − v_P) < −40 mm/s`, and the contact point lies in the belly band. Contacts on the rim (`|z| < 3 mm`) or the cavity are rejected with a dull "thud" so the player learns the surface.

## 6. Input Kinematics & Signal Processing

- **Contact coordinates**: for contact `p_P`, `λ = atan2(p_P·ŷ_P, p_P·x̂_P)` wrapped to [0°, 360°), and `θ = acos(clamp((c_z − p_P·ẑ_P)/|p_P − C|, −1, 1))` with `c_z = 23.33 mm`.
- **Quadrant**: base index `floor(((λ + 45°) mod 360°)/90°)`; if `λ` is within 4° of a boundary, keep the previous quadrant of that finger unless the contact is > 12 mm inside the new quadrant. This is the seam hysteresis.
- **Strike energy**: `E = 0.7 · (8 + 16 · clamp((v_n − 40)/360, 0, 1))`, where `v_n` is the inward normal speed in mm/s; at 40 mm/s `E = 5.6`, at 400 mm/s `E = 16.8`. Pinky multiplies by 0.7; palm slap sets `E = 34`.
- **Charge**: `c_Q ← clamp(c_Q + E·(1 + 0.15·alternating) − 14·dt, 0, 100)`. A steady 1.2 strikes/s at 200 mm/s nets `+19.2 − 14 = +5.2` per second, so a lazy roll never arms; a 2.5 strikes/s roll nets `+34`/s and arms in 3 s.
- **Rhythm filter**: strike times are debounced at 110 ms per finger and 70 ms globally; contact velocity is low-passed with a 9 Hz second-order Butterworth. The filter's 18 ms group delay is negligible against the 450 ms arm telegraph.
- **Arming**: when `c_Q` reaches 100, Q is armed and locked (no further charge) for 0.45 s; the tentacle tip rises at the ocean centre. Charge cannot be transferred between quadrants; decay continues during arming.
- **Slam resolution**: at `t_arm + 0.45 s` the tentacle sweeps the 90° rim sector centred on the quadrant bisector. A ship is hit if its rim angle is within ±45° of the bisector **and** its radius is within 4 mm of the rim band at the moment of the sweep. The sweep is a 0.12 s capsule cast from the centre to the rim, so a ship moving 22°/s (0.38 mm/ms) moves 46 mm during the sweep — deliberately less than the 6 mm capsule radius plus 4 mm ship radius? No: it moves far more, so the sweep is evaluated at the midpoint instant only, and the 0.12 s is presentation time, not a swept window. This keeps the hit test readable and deterministic.
- **Empty slam**: if no ship is in the sector, surge `+5` and the quadrant still enters its 1.8 s cooldown. This is the core cost of panic drumming.
- **Ship motion**: `λ_s(t) = λ_s(0) + ω_s·t`, `ω_s = ±(10–22)°/s`; direction is fixed per ship and shown by its wake. A ship escapes after 360° of travel.
- **Surge**: `S ← clamp(S + 2.2·dt + 12·escapes − 6·slams, 0, 100)`. At level 3 the passive term alone is 121 % per 55 s, so ~20 slams are required per broth just to break even; each slam also clears ships, which is the only way to stop the +12 escape spikes.

## 7. Virtual World Layout & Coordinate Mapping

`V = P`. The ocean is a disc of radius 82 mm at `z = +6 mm`, rendered as a dark ellipsoid cap 2 mm thick; its centre is 6 mm above the rim so the meniscus hides the rim's inner edge. The tentacle emerges from `(0, 0, −12 mm)` and its rest coil is a 40 mm torus below the surface.

| Entity | V-space position | Size | Notes |
|---|---|---|---|
| Ocean disc | `z = +6 mm` | r = 82 mm | P-anchored, 2 mm meniscus |
| Ship | rim band `r = 74 mm`, `z = +4 mm` | 12 × 4 × 10 mm | 8 lanes, ±10–22°/s |
| Tentacle | centre → rim bisector | r = 6 mm, 90–150 mm | 0.45 s arm, 0.12 s slam |
| Rim sector glyph | rim `z = +1 mm` | 45° arc, 3 mm wide | Shows quadrant mapping |
| Storm sky | dome r = 600 mm | gradient + rain | Camera-anchored, not P |
| Surge gauge | rim `λ = 90°`, `z = +2 mm` | 12 × 30 mm bar | Emissive tide gauge |
| Broth name card | 350 mm camera-forward | 120 × 60 mm | Between broths only |

Mapping formulas: a ship at rim angle `λ_s` is at `p_V = (74 cosλ_s, 74 sinλ_s, 4) mm`; the quadrant bisector for Q is `b̂_Q = (cos(90q + 45°), sin(90q + 45°), 0)`; the tentacle tip path is a quadratic Bézier from `(0,0,−12)` to `(78·b̂_Q, 0)` with control point `(30·b̂_Q, 30 mm)` so the arm bows outward before the slam.

The camera reads the diorama from 350–500 mm; at that distance the 164 mm ocean subtends 18–26°, and a 12 mm ship subtends 1.4–2.0°, which is above the 0.9° minimum for reliable shape recognition. Ships are therefore never smaller than 10 mm in any dimension.

## 8. Gameplay Systems & Rules

**Entities.** `BrothDirector` (5 broths, quota, surge), `QuadrantDrum` ×4 (charge, cooldown, arm state), `TentacleRig` (one pooled arm), `ShipFleet` (7 ships, lanes, armour), `SlamResolver`, `SurgeModel`, `ScoreKeeper`.

**Broths.** Five recipes, each 55 s or until its quota of ships is destroyed: Bouillabaisse (12), Bisque (15), Chowder (18), Gumbo (21), Consommé (24). Quota progress is shown as a ladle filling around the rim. A broth that times out with quota unmet ends the run (the fleet escapes).

**Ships.** Seven live ships. Base hull 1 HP; from broth 3, 25 % of spawns are barnacled (2 HP, slower by 20 %, +40 score). A slam deals 1 damage to every ship in the sector; a 2 HP ship survives the first slam with a cracked wake. Respawn 1.4 s after destruction at a random rim angle at least 60° from the last slam sector. A ship that completes 360° escapes: surge +12, −150 score, respawn after 1.0 s.

**Quadrants.** Four 90° sectors; charge 0–100; decay 14/s; cooldown 1.8 s after a slam; global arm lock 250 ms so two arms cannot overlap. A drum strike on a cooling quadrant still shows a spark but adds no charge.

**Slam window.** 0.45 s from full charge; the sector is locked at the moment of arming, so a ship that sails out during the telegraph is genuinely safe. Multi-kill: 2 ships = ×1.2 score, 3 = ×1.5, 4+ = ×1.8; the multiplier applies to the whole slam.

**Storm surge.** 0–100; passive +2.2/s (level-scaled), +12 per escape, −6 per slam (hit or miss). At 85 the broth darkens and the rain becomes horizontal; at 100 the broth boils over and the run ends immediately.

**Scoring.** Ship +120 (barnacled +160); multi-kill multiplier; broth clear +400 + 6 × seconds remaining; escape −150; empty slam −40; run clear bonus +1000; final score floors at 0.

**Win/lose.** Clear all five broths with surge < 100 to win. Surge 100 at any time is a loss. A broth timeout with quota unmet is a loss.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load broth table, start tracker | REGISTER |
| REGISTER | bowl presented | rim RMS < 1.2 mm | fit `P`, set λ = 0 to head forward | CALIBRATE |
| CALIBRATE | both palms on rim 0.8 s | contacts stable | show front arrow, zero surge | BRIEF |
| BRIEF | timer 2.5 s | broth ≤ 5 | show recipe and quota | DRUM |
| DRUM | valid strike | quadrant not cooling | add charge, VFX, tick | DRUM |
| DRUM | `c_Q ≥ 100` | not already armed | lock sector, start 0.45 s | ARM |
| ARM | timer 0.45 s | — | tentacle rises, sector flashes | SLAM |
| SLAM | timer 0.12 s | — | resolve ships, surge −6, score | RECOVER |
| RECOVER | timer 0.6 s | — | retract arm, quadrant cd 1.8 s | DRUM |
| DRUM | ship escapes | — | surge +12, −150 | DRUM |
| DRUM | quota met | — | broth clear, bank score | BROTH_END |
| DRUM | timer 55 s | quota unmet | fleet escapes, run fail | DEBRIEF |
| DRUM | surge ≥ 100 | — | boil-over, white-out | DEBRIEF |
| BROTH_END | timer 2.0 s | broth < 5 | surge −15 grace, restore 2 ships | INTERMISSION |
| BROTH_END | broth = 5 | surge < 100 | results | COMPLETE |
| INTERMISSION | timer 8 s | — | recipe tip, hands below rim | BRIEF |
| any | bowl lifted > 120 mm | — | freeze ships and surge | SPILL |
| SPILL | bowl restored | `P` relock < 1.2 s | 1.0 s grace, no charge loss | DRUM |
| any | tracking loss > 200 ms | — | freeze, ghost bowl | REACQUIRE |
| REACQUIRE | relock | RMS < 1.6 mm | blend 150 ms | prior |
| any | idle 25 s | — | freeze timers | SUSPENDED |
| SUSPENDED | both palms hover 500 ms | — | 2 s countdown | prior |

Timers: arm 0.45 s; slam 0.12 s; recover 0.6 s; quadrant cooldown 1.8 s; global arm lock 250 ms; strike debounce 110 ms/finger, 70 ms global; ship respawn 1.4 s; broth 55 s; spill grace 1.0 s; reacquire 1.2 s. All timers tick at the fixed 90 Hz step and are wall-clock latched, so a dropped frame cannot shorten a cooldown.

## 10. Physics & Collision

- **Collider inventory**: C0 outer shell SDF (analytic), C1 inner shell SDF, C2 rim torus, C3 foot ring, C4 four quadrant volumes, C5 eight rim sectors, C6 one tentacle capsule, C7 seven ship capsules, C8 hand capsules per joint.
- **Pair matrix**:

| Pair | Test | Response |
|---|---|---|
| Hand × C0 | SDF < 5 mm, belly band | drum contact, energy |
| Hand × C1 | SDF < 5 mm | reject with thud |
| Hand × C2 | SDF < 5 mm | rim rest; no charge |
| Tentacle × C7 | sector angle ≤ 45°, radial band | ship damage 1 |
| Tentacle × C5 | sector lookup | slam VFX, surge −6 |
| Ship × rim end | λ ≥ 360° | escape, surge +12 |
| Ship × ship | none | pass-through (lanes are separated by 45°) |
| Hand × hand | none | no self-collision |

- **Materials**: ceramic belly = "hard drum" restitution 0.15, no slip; rim = "glaze" µ 0.42; ship = "balsa" 1 HP, no restitution; tentacle = "flesh" no restitution, 0.2 s squash; broth = "fluid proxy", no collider.
- **CCD**: ship speed ≤ 22°/s = 0.38 mm/ms; at 11.1 ms/tick a ship moves 4.2 mm, below the 6 mm tentacle radius, so a discrete mid-sweep test cannot tunnel. The tentacle capsule itself is tested once at the 0.12 s midpoint.
- **Forces**: no rigid-body force on the bowl — it rests on the lap and the player's strikes are absorbed. The drum energy is a signal, not an impulse. The virtual tentacle applies a purely kinematic sweep; ships are removed, not launched. This avoids a feedback loop in which hitting the bowl harder would move the ocean under the ships.
- **Surge integration**: `S += (2.2 + 0.25·(level−1))·dt`; clamp [0, 100]. The level term is added at broth start, not per frame, to keep the tuning table exact.

## 11. Rendering, Occlusion & Compositing

Layer order, near to far: passthrough → lap/table scene mesh → bowl depth proxy → interior ocean and ships → tentacle → rim tide gauge → broth spray VFX → hand mesh → storm vignette → HUD. The bowl proxy writes depth at the outer shell, so the virtual ocean is clipped by the real ceramic lip; the ocean is drawn 0.5 mm inside the inner shell to avoid z-fighting.

**Occlusion cases:**

1. **Hand over the belly** — the drumming hand is depth-tested against the bowl proxy. Fingers in front of the belly are visible; the part of the hand that would be inside the ceramic is clipped, which reads as contact rather than penetration.
2. **Ship behind the rim** — a ship on the far rim is half-occluded by the near rim; its mast and wake remain visible above the lip, and a 2 mm waterline glow marks the hull. The player never loses a ship entirely.
3. **Tentacle behind the hand** — the arm is drawn behind the hand mesh, so a slam that passes under the drumming hand is correctly hidden for 60–90 ms; the impact ring is drawn on top so the hit is still confirmed.
4. **Ocean near the rim** — the meniscus is clipped by the inner shell; a 1.5 mm foam line is drawn at the intersection to sell the waterline.
5. **HUD over the broth** — the surge gauge is a rim-mounted emissive bar, not a screen-space overlay, so it is occluded by the hand only when the hand is genuinely on that rim arc; a duplicate thin arc at 30 % opacity sits above the bowl for readability.
6. **Storm rain** — rain is a camera-anchored particle sheet drawn in front of the scene mesh but behind the hand; it never occludes a ship because its alpha is capped at 0.22 within the bowl's screen bounds.

**VFX budgets**: drum spark 10 per strike, max 40; tentacle splash 70 per slam, max 2 concurrent; ship wreck 24 per kill, max 5; rain 900 particles; surge foam 120; rim tide pulse 1 quad. Global cap 1,400 particles, 6 transparent layers, < 2.2× screen overdraw.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Strike | contact point | Ceramic tom, pitch rises 180→320 Hz with charge | 10 spark flecks, quadrant rim pulse |
| Alternating roll | contact point | Second tom a fifth higher | Two-tone ripple on the belly |
| Quadrant armed | quadrant bisector | Low kraken growl, 0.45 s | Abyssal red glow, suck-hole at centre |
| Tentacle rise | ocean centre | Water displacement whoosh | Coil breaks surface, 70 droplets |
| Slam | rim sector | 60 Hz impact + shell crack | Radial splash, sector flash, camera 2 mm shake |
| Multi-kill | rim sector | Added chime per ship, +2 semitones | Wreck bursts, score popups |
| Empty slam | rim sector | Dull wet thud + surge alarm | Grey splash, gauge jumps +5 |
| Ship escape | rim edge | Foghorn descending | Wake crosses the rim, gauge +12 |
| Barnacle hit | ship | Wood crack, no break | Shell plate flies, ship limps |
| Surge warning | HUD | 1.5 Hz pulse above 85 | Rim gauge red, rain horizontal |
| Broth clear | ocean | Major sea-shanty cadence | Ladle fills, fleet bows |
| Boil-over | ocean | 40 Hz rumble + steam | White-out, run ends |

Spatialisation: world cues use a 1.2 m rolloff; HUD cues are dry. The strike tom is the primary feedback loop — its pitch is the charge meter, so the player can drum without looking at the gauge.

## 13. Difficulty Progression & Tuning

| Level | Broth time (s) | Quota | Ships | Speed range (°/s) | Barnacled share | Passive surge (/s) | Arm time (s) |
|---|---|---|---|---|---|---|---|
| 1 | 55 | 12 | 5 | 10–14 | 0 % | 1.6 | 0.55 |
| 2 | 55 | 15 | 6 | 12–16 | 10 % | 1.9 | 0.50 |
| 3 | 55 | 18 | 6 | 14–18 | 25 % | 2.2 | 0.45 |
| 4 | 50 | 21 | 7 | 16–20 | 35 % | 2.5 | 0.40 |
| 5 | 50 | 24 | 7 | 18–22 | 45 % | 2.8 | 0.36 |

- **Adaptive rule** (`KRAKEN_ADAPT`): after each broth, if quota was met with ≥ 12 s left and surge < 45, raise level by 1; if the broth was failed or surge ended > 85, lower level by 1 and extend the next broth by 5 s. Clamp 1–5; one step per broth. Adaptation never changes `R`, the belly band, or the 90° sector width, because the physical drum map must stay stable.
- **Named constants**: `R = 80 mm`, `D = 60 mm`, `T = 4 mm`, `R_S = 83.33 mm`, `THETA_MAX = 73.74°`, `BELLY = 35°–62°`, `Q_WIDTH = 90°`, `SEAM_HYST = 4°`, `CHARGE_MAX = 100`, `DECAY = 14/s`, `E_MIN = 8`, `E_MAX = 24`, `V_REF = 400 mm/s`, `ARM = 0.45 s`, `SLAM = 0.12 s`, `Q_CD = 1.8 s`, `SURGE_PASSIVE = 2.2/s`, `SURGE_ESCAPE = +12`, `SURGE_SLAM = −6`, `SURGE_EMPTY = +5`, `SHIPS = 7`, `BROTHS = 5`.

## 14. Edge Cases & Failure Recovery

- **Bowl rotated on the lap** — `P` yaw follows the ceramic; quadrants and ships rotate together. If the yaw jumps > 35° in one frame, it is treated as a tracker glitch, held for 120 ms, and the ships are frozen rather than teleported.
- **Bowl lifted** — apex > 120 mm above `T` pauses the broth; on return, a 1.0 s grace lets the player re-seat it before surge resumes. No charge is lost, but armed quadrants are disarmed.
- **Hand tracking loss** — losing one hand keeps the other drumming; losing both for > 300 ms pauses charge decay and surge. Re-acquisition restarts with 0.5 s grace.
- **Strike exactly on a seam** — ±4° hysteresis assigns the previous quadrant of that finger; with no history, the nearer bisector wins. A strike within 3 mm of the rim is rejected as a rim hit.
- **Two quadrants armed at once** — the global 250 ms arm lock and the 1.8 s cooldown prevent overlap; if a second reaches 100 during the lock, it is capped at 99 and arms 50 ms after the lock releases.
- **All ships destroyed mid-broth** — quota is checked immediately; remaining time converts to +6/s and the next broth starts after the 2 s clear card.
- **Ship escapes during a slam telegraph** — the sector is locked at arming, so the escape is real and the slam can be empty. This is intentional; it rewards reading the wake.
- **Surge at 99 with a slam in flight** — the −6 is applied at the slam instant, not at arming; if surge reaches 100 during the 0.45 s telegraph, the boil-over resolves first and the slam is cancelled.
- **Recovery UX** — one icon (bowl / hand / lap) plus one line of text; no modal dialog mid-broth.

## 15. Implementation Architecture

**Classes/structs:**

- `PropBowl` — owns `poseP`, fits rim sphere and plane, publishes `P` at 72 Hz.
- `ContactRouter` — 26 joints → `Contact {joint, p_P, n_P, v_n, age}`, 5/8 mm hysteresis, 30 ms debounce.
- `DrumClassifier` — belly-band test, quadrant with seam hysteresis, alternating-finger bonus.
- `QuadrantCharge` ×4 — charge, decay, cooldown, arm lock.
- `ArmDirector` — 0.45 s telegraph, 0.12 s slam, pool of one tentacle.
- `ShipFleet` — 7 ships, lanes, armour, escape, respawn.
- `SlamResolver` — sector angle test, multi-kill multiplier, empty-slam penalty.
- `SurgeModel` — passive + escape − slam, clamp, warning thresholds.
- `BrothDirector` — 5 recipes, quota, 55 s timer, clear/fail.
- `KrakenRig` — ocean, tentacle, splash, rim glyphs.
- `FeedbackRouter` — event enum → one audio clip + one VFX prefab.
- `Compositor` — layer order and cases O1–O6.

**Event flow**: `HeadsetTracker → PropBowl → ContactRouter → DrumClassifier → QuadrantCharge → ArmDirector → SlamResolver → ShipFleet / SurgeModel / ScoreKeeper → FeedbackRouter → KrakenRig / AudioBank / VFXPool`. `BrothDirector` reads only `quotaProgress`, `surge`, and `brothTimer`; it never reads raw joints.

**Pseudocode 1 — drum strike to charge:**

```text
on ContactBegin(c):
    if not inBellyBand(c.p) or c.age > 90ms: return
    vn = dot(c.n, c.v_joint - v_prop)
    if vn > -40 mm/s: return
    q = quadrantWithHysteresis(c.lambda, c.finger)
    if now < q.cooldownUntil or now < lastGlobalStrike + 70ms: return
    if now < q.lastStrike[finger] + 110ms: return
    e = 0.7 * (8 + 16 * clamp((abs(vn) - 40) / 360, 0, 1))
    if finger == PINKY: e *= 0.7
    alt = (finger != q.lastFinger) and (90ms < dt < 260ms)
    q.charge = clamp(q.charge + e * (1 + 0.15*alt), 0, 100)
    q.lastFinger = finger; q.lastStrike[finger] = now
    lastGlobalStrike = now
    emit DrumHit(q, e, alt)
    if q.charge >= 100 and not q.armed: arm(q)
```

**Pseudocode 2 — slam resolution:**

```text
function arm(q):
    if now < globalArmLockUntil: q.charge = 99; return
    q.armed = true
    q.sectorLock = q.bisector          # 90 deg sector frozen now
    globalArmLockUntil = now + 250ms
    armTimer[q] = 0.45s
    emit ArmStart(q)

function resolveSlam(q):
    killed = []
    for s in fleet.live():
        if abs(angleDiff(s.lambda, q.sectorLock)) <= 45deg:
            s.hp -= 1
            if s.hp <= 0: killed.append(s)
            else: emit Cracked(s)
    for s in killed: fleet.destroy(s); score += 120 * multiplier(killed)
    surge = clamp(surge - 6 + (killed.empty() ? 5 : 0), 0, 100)
    q.armed = false; q.charge = 0; q.cooldownUntil = now + 1.8s
    emit Slam(q, killed.size())
```

**Determinism**: ship spawn angles, speeds, and armour rolls use PCG32 keyed by `(session_id, broth, spawn_index)`; slam resolution iterates ships by stable ID. A recorded contact trace therefore replays bit-identically at 90 Hz.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Bowl fit | Present the bowl; inject 48 rim points with 0.4 mm noise | RMS < 1.2 mm; rim normal < 2.0°; `R_s` within ±1.5 mm of 83.33 |
| AT-02 | Belly band | Strike at `z = −49`, `−18`, `−17`, `−50` mm | First two accepted; last two rejected |
| AT-03 | Quadrant map | Strike at λ = 0°, 90°, 180°, 270° | Q_F, Q_L, Q_B, Q_R respectively, 20/20 |
| AT-04 | Seam hysteresis | 20 strikes at λ = 45° ± 3° with a prior Q_F history | All assigned Q_F; no double arm |
| AT-05 | Energy scaling | Strike at 40, 200, 400 mm/s | Charge +5.6, +13.1, +16.8 ± 0.5 |
| AT-06 | Alternating bonus | Two strikes 160 ms apart, different fingers | Second energy ×1.15 ± 0.02 |
| AT-07 | Arm and slam | Fill Q_F to 100; wait 0.45 s | One tentacle; sector frozen at arm time; 90° hit arc |
| AT-08 | Empty slam | Arm Q_B with no ships in sector | Surge +5, −40 score, cooldown 1.8 s |
| AT-09 | Multi-kill | Place 3 ships at λ = 80°, 90°, 100°; slam Q_L | All 3 destroyed; multiplier 1.5 ± 0.05 |
| AT-10 | Escape | Run one ship through 360° | Surge +12, score −150, respawn 1.0 s |
| AT-11 | Surge math | 10 s at level 3, no events | Surge +22.0 ± 0.2 |
| AT-12 | Quota clear | Destroy quota with 12 s left | Broth clear, +400 + 72 score |
| AT-13 | Lift pause | Raise bowl 130 mm for 3 s | Ships and surge frozen; no charge lost |
| AT-14 | Perf | Slam with 4 wrecks + rain + 2 hands | ≤ 1,400 particles; frame ≤ 11.1 ms on target device |
