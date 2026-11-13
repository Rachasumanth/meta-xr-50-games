# Game 10 — Electron Deflector — Technical Game Design & Architecture Specification

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Electron Deflector |
| Slug | `electron-deflector` |
| Physical prop | Ping-pong ball, 40 mm diameter, ~2.7 g, pinched at the poles by the left hand |
| Family / sub-shape | Family 4: Spherical, Ellipsoidal & Domed / 4.1a True Spheres — Pinch-Scale (1–4 cm) |
| Scale class | Palm-scale two-hand, pole-pinched sphere, equatorial tap targeting |
| Source concept | "The ball becomes an atomic nucleus surrounded by frantic electron shells. Tapping a point on the equator fires a localized positron burst from that coordinate to deflect incoming rogue neutrons into orbital collection rings." |
| Source physical input | Pinch the bottom and top poles of the ping-pong ball with your left hand and rapidly tap different spots around its equator with your right index finger. |

## 1. Fantasy & Core Loop

**Fantasy.** The ping-pong ball is a nucleus held in a magnetic containment pinch. Electron shells spin around it in frantic rings, and rogue neutrons come streaking in from the dark. Each tap on the equator fires a positron burst from that exact longitude — a bright spike of antimatter that kicks the neutron sideways into one of three orbital collection rings.

**Core loop** (one neutron ≈ 1.2–2.2 s; one containment run ≈ 70–100 s):

1. **Hold the nucleus** (continuous) — left thumb and index pinch the top and bottom poles; the ball's equator is fully exposed.
2. **Read the inbound** (0.3–0.7 s) — a neutron spawns with a radial approach line and a 1.6 s impact countdown.
3. **Choose the sector** (0.1–0.3 s) — 12 equatorial sectors; the correct burst sector is opposite the neutron's approach azimuth.
4. **Tap** (0.08–0.15 s) — right index taps the equator inside that sector; a burst fires.
5. **Deflect** (0.3–0.6 s) — the neutron curves under the burst impulse toward a ring.
6. **Collect / absorb** (0.2 s) — crossing a ring collects it (+score); reaching the ball costs a containment charge.
7. **Shell surge** (2.5 s) — every 10 neutrons the shells flare; all sectors cool faster for 3 s.

The game is a rapid spatial mapping task: see the approach azimuth, map it to the opposite sector, and hit a 30° patch on a 40 mm ball before the countdown ends.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Sphere sub-shape**: the prop is a true sphere of radius `r`; model in the P frame with origin at the center, poles at `±ẑ_P`, equator in `x̂_P`–`ŷ_P`, no distinguishable faces. Surface parameterization: latitude `φ ∈ [−90°, +90°]`, longitude `λ ∈ [0°, 360°)`; `p_P = r·(cosφ·cosλ, cosφ·sinλ, sinφ)`; normal `p_P/r`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **P frame**: origin = ball center; `+ẑ_P` = **north pole** (the pole held by the left index finger by default); `−ẑ_P` = south pole (left thumb); equator in `x̂_P`–`ŷ_P`; longitude `λ` from `+x̂_P` toward `+ŷ_P`.
- **Numbers**: r = 20 mm, m = 2.7 g, celluloid, µ ≈ 0.35. Pole grip caps: `|φ| > 60°` (two 60° caps, one per hand digit); equator band: `|φ| ≤ 35°` (the tap target).
- **Sectors**: 12 equatorial sectors, `S_j = [30j, 30(j+1))` in longitude; each has its own 1.2 s cooldown and a visible gloss ring.
- **V frame**: anchored to **P** for the nucleus and shells (they spin with the ball's *virtual* frame but not with the physical ball's spin — the physical orientation is ignored), and anchored to **W** for the neutron stream and collection rings. The ball's equator maps to the nucleus equator 1:1.
- **Deviations**: (a) the physical ball's own rotation is ignored — the sector is defined by the **contact longitude in P**, and the left-hand pole pinch fixes P's poles, so the mapping is stable; (b) the tap target is only 12 patches on a 40 mm ball, so the game uses a **hysteretic sector assignment** with a 3° dead band at boundaries; (c) the neutron stream is W-anchored so the player's hand motion does not drag the enemies.

## 3. Physical Play Space & Ergonomics

- **Posture**: seated; left hand at 230–320 mm in front of the sternum, ball pinched between thumb (south pole) and index (north pole); right index free to tap the equator.
- **Support surface**: none; calibration uses a table to establish the ball's initial pole axis.
- **Play volume**: a 500 mm radius sphere around the ball for neutrons and rings; the ball itself stays nearly still.
- **Handedness**: mirrored — right hand pinches poles, left index taps.
- **Reach envelope**: fingertips 180–330 mm from the shoulders; the right index travels ~60 mm around the equator.
- **Session length**: 7 min default (5 runs), hard stop 10 min.
- **Fatigue mitigations**: (1) the pole pinch is light (2.7 g); (2) taps are single-index and can alternate with the middle finger; (3) sector cooldowns force a natural rhythm rather than frantic tapping; (4) a 5 s "shell cool-down" between runs.

## 4. Prop Geometry & Registration

- **Real dimensions**: diameter 40.0 mm (r = 20.0 mm), mass 2.7 g, celluloid, µ ≈ 0.35, very low inertia.
- **Sphere model** (P frame, mm): `|p| = 20`; pole caps at `φ > +60°` and `φ < −60°`; equator band `|φ| ≤ 35°`; 12 sectors at 30° longitude; sector boundaries at `λ = 30j` with a 3° dead band.
- **Colliders**: `COL_sphere` r = 20 mm; `COL_poleN` / `COL_poleS` spherical caps (grip zones, no collision beyond contact); `COL_equator` torus band (r_major = 20, r_minor = 7 mm) for tap classification; `COL_sector_j` twelve spherical patches used for sector assignment.
- **Registration**: (1) player pinches the ball at the poles, north = index, south = thumb, and presents the equator; (2) system fits the sphere center (error < 1.0 mm) and the pole axis from the two pinch contacts; (3) because the ball is symmetric, the longitude origin `λ = 0` is set at a "tap here" calibration point; (4) `P` published at 72 Hz; (5) the pole axis is re-fit every 300 ms; if it drifts > 12°, the longitude origin is rotated with it to keep the sector map stable relative to the hand.
- **Re-acquisition**: search 70 mm around the last pose; re-fit the sphere; if the pole axis cannot be recovered, hold the last P and show a "re-pinch the poles" card. Blend over 150 ms.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints used | Contact surface | Semantic output |
|---|---|---|---|---|
| I5.1 | Pole pinch north | `L_index_tip` | `COL_poleN` | north pole held; P north defined |
| I5.2 | Pole pinch south | `L_thumb_tip` | `COL_poleS` | south pole held; P south defined |
| I5.3 | Equator tap | `R_index_tip` (alt `R_middle_tip`) | `COL_equator`, sector j | fire positron burst in sector j |
| I5.4 | Slide-tap | rapid drag across the equator | equator | fires at the sector under the contact at each 30° crossing (max 3/s) |
| I5.5 | Double-tap sector | two taps < 220 ms in the same sector | sector j | heavy burst (1.6× impulse, 2× cooldown) |
| I5.6 | Hold burst | contact held 350 ms | sector j | sustained field (low impulse, 0.8 s) |
| I5.7 | Pole release | either pole contact lost | — | sector map freezes; taps disabled |
| I5.8 | Re-pinch | both poles restored 150 ms | — | resume with the same sector origin |

Per-finger roles: left index/thumb are the pole clamps; right index is the primary tapper; right middle is the equal alternate; ring/pinky ignored. Per-hand roles: left = CLAMP, right = FIRE.

## 6. Input Kinematics & Signal Processing

- **Contact longitude**: for a tap contact at `p_P`, `λ = atan2(p_P·ŷ_P, p_P·x̂_P)` in degrees, wrapped [0, 360); `φ = asin(p_P·ẑ_P / r)`.
- **Sector assignment**: `j = floor(λ / 30)`; if `λ` is within 3° of a boundary, assign to the sector whose previous tap was most recent (hysteresis), else to the nearer sector center.
- **Tap validation**: impact normal speed `v_imp = max(0, −(v_j − v_P)·n̂) > 25 mm/s`, contact age < 100 ms, `|φ| ≤ 35°`, and the contact point not within 4 mm of a pole cap. Per-sector cooldown 1.2 s; global cooldown 150 ms.
- **Burst impulse**: a burst at sector j applies, to every neutron within 120 mm and within 35° of the sector center direction `û_j`, an impulse `Δv = A·(û_j × r̂_n)` with `A = 160 mm/s` (normal) or `256 mm/s` (heavy), where `r̂_n` is the neutron's radial unit vector; the cross product produces a sideways curve. The impulse is applied over 0.30 s (a shaped force, not a single kick) to keep the visual arc readable.
- **Neutron approach**: each neutron spawns at radius 480 mm with a random unit direction `d̂`; velocity `v = −120·d̂ mm/s`; it is "on a collision course" if its impact parameter `b = |d̂ × (−p)| < 26 mm` (ball radius 20 + neutron radius 6).
- **Correct sector heuristic**: the ideal burst sector is the one whose center is within ±15° of `normalize(û × r̂)` direction; the game does not enforce this, but the sector that produces the largest deflection is the one 90° from the approach in the plane of travel.
- **Collection test**: a neutron is collected when it crosses a ring plane within the torus tube (major radius 150 mm, minor radius 15 mm) at any time. A neutron that leaves the 500 mm sphere without collection is a miss (no penalty beyond the lost opportunity).
- **Absorption**: a neutron that reaches `|p| < 26 mm` is absorbed: containment −1 charge and a red flash. Heavy neutrons absorb 2 charges.
- **Heavy neutrons**: 20 % of spawns (level-scaled) have 2 HP; a normal burst only curves them, a heavy burst or two normal bursts are needed to deflect them enough to collect.

## 7. Virtual World Layout & Coordinate Mapping

- **V frames**: `V_nucleus = P` (nucleus, shells, sector gloss); `V_stream = W` (neutrons, rings, starfield).
- **Nucleus**: a 20 mm sphere at the ball center, with three electron shells at radii 34, 52, and 74 mm, each a tilted torus spinning at 90–240 °/s (visual only).
- **Collection rings**: three tori of major radius 150 mm and minor radius 15 mm, centered on the ball, with normals along `x̂_W`, `ŷ_W`, `ẑ_W` (three orthogonal rings); each is color-coded and slowly rotates about its normal at 8 °/s.
- **Neutron stream**: spawn shell at 480 mm; neutrons are 12 mm spheres with a 40 mm trail; approach speed 120 mm/s (96–168 by level).
- **Starfield**: a 520 mm radius dim sphere with parallax dots, giving depth.
- **Mapping formulas**: neutron world position integrates in W; sector center `û_j = (cos(30j+15°), sin(30j+15°), 0)` in P, mapped to W by `R_P`; tap contact `p_W = R_P·p_P + c`.
- **Camera note**: the nucleus is near the hand, while the rings are 150 mm out; the player sees the ball in the foreground and the orbital rings as glowing hoops around it, which makes the tap-to-deflection relationship legible.

## 8. Gameplay Systems & Rules

- **Entities**: `Nucleus` (containment 4 charges, shell flare), `Sector` (12, cooldown), `Neutron` (HP 1 or 2, radial path, speed), `Ring` (3 collection tori), `ContainmentDirector`.
- **Containment**: 4 charges; a normal neutron absorbed costs 1, a heavy costs 2. At 0 the nucleus collapses and the run ends. Charges restore 1 per 10 collected neutrons.
- **Sectors**: each 1.2 s cooldown; a heavy burst has 2.4 s cooldown; a sustained field has 0.8 s duration.
- **Collection**: +100 per neutron; heavy +250; a neutron collected on the *first* burst +50 ("clean deflection"); 3 clean deflections in a row → "Resonance" ×1.3 for 5 s.
- **Miss**: a neutron that leaves the play sphere is a miss; no charge lost, but the clean streak resets.
- **Shell surge**: every 10 collected neutrons, the electron shells flare for 3 s; all sector cooldowns are halved and burst impulse is ×1.15.
- **Scoring**: collect +100 (+250 heavy); clean +50; resonance ×1.3; run clear +400; remaining containment ×120; 100 % collection bonus +600.
- **Win/lose**: run ends at 0 containment or when 30 neutrons have spawned; cleared if ≥ 24 collected. Session = 5 runs; win if ≥ 3 cleared.
- **Difficulty**: spawn interval 2.0 s → 0.9 s; heavy share 5 % → 30 %; approach speed 96 → 168 mm/s.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | app start | — | load config, UI | REGISTER |
| REGISTER | sphere + pole fit | center < 1.0 mm, poles opposed | set λ = 0 via calibration tap | CALIBRATE |
| CALIBRATE | both poles held 300 ms | north + south contacts | build shells/sectors | BRIEF |
| BRIEF | tap once | sector valid | run card 2.0 s | CONTAIN |
| CONTAIN | sector tap | cooldown ok, poles held | spawn burst, sector cd | CONTAIN |
| CONTAIN | neutron in burst cone | — | apply shaped impulse 0.30 s | CONTAIN |
| CONTAIN | neutron crosses ring | within tube | collect, +100 | CONTAIN |
| CONTAIN | neutron reaches ball | — | containment −1 (or −2 heavy) | CONTAIN |
| CONTAIN | neutron leaves 500 mm | — | miss, streak reset | CONTAIN |
| CONTAIN | 10 collected | — | shell surge 3 s | SURGE |
| CONTAIN | containment = 0 | — | collapse, run fail | DEBRIEF |
| CONTAIN | 30 spawned | — | tally | DEBRIEF |
| SURGE | 3.0 s elapsed | — | cooldowns restore | CONTAIN |
| CONTAIN | pole released | — | sector map frozen, taps off | HOLD |
| HOLD | both poles re-pinched | 150 ms | resume, same origin | CONTAIN |
| any | prop tracking loss > 150 ms | — | freeze neutrons, ghost ball | REACQUIRE |
| REACQUIRE | relock ≤ 1.5 s | search r = 70 mm | blend P 150 ms | prior |
| REACQUIRE | 1.5 s timeout | — | "re-pinch the poles" card | REACQUIRE |
| DEBRIEF | run < 5, containment > 0 | — | next run seed | BRIEF |
| DEBRIEF | run = 5 or 3 fails | — | session stats | COMPLETE |
| any | idle 20 s / system pause | — | freeze timers | SUSPENDED |
| SUSPENDED | palm hover 500 ms | — | 2 s countdown | prior |

Timers: sector cooldown 1.2 s (heavy 2.4 s); global tap cooldown 150 ms; burst shaping 0.30 s; shell surge 3.0 s; pole re-grip 150 ms; reacquire 1.5 s.

## 10. Physics & Collision

- **Collider inventory**: `COL_sphere` r = 20 mm, `COL_poleN/S` caps, `COL_equator` torus, 12 sector patches, up to 12 `Neutron` spheres (r = 6 mm), three ring tori (major 150, minor 15), nucleus trigger sphere r = 26 mm.
- **Pair matrix**: Neutron↔Nucleus (absorb), Neutron↔Ring (collect), Neutron↔Burst cone (impulse), Hand↔Ball (contacts only), Neutron↔Neutron (none).
- **Material values**: neutron mass 4 (arbitrary units), no gravity, linear drag 0.02 s⁻¹; ring collection is a trigger, not a bounce.
- **Physical constants**: m_ball = 2.7 g; r = 20 mm; µ = 0.35; the ball is held, so it does not translate.
- **CCD**: neutron max speed 168 mm/s → 1.9 mm/tick; swept-sphere vs ring plane is still used to avoid a grazing miss.
- **Force formulas**: burst `F(t) = A·(1 − t/0.30)·(û × r̂)` for `t ∈ [0, 0.30] s`, so `Δv = A·0.30/2`; with `A = 160 mm/s²`, a full pass yields ≈ 24 mm/s of lateral velocity — enough to curve a neutron by ~40 mm over the remaining 0.6 s, which is the intended "one burst = one ring crossing" budget.
- **Absorption**: `|p| < 26 mm`; heavy neutrons require 2 HP, where a heavy burst removes 2 HP and a normal burst removes 1.

## 11. Rendering, Occlusion & Compositing

- **Layer order**: passthrough → starfield → collection rings → neutrons → electron shells → nucleus → sector gloss → VFX → hands → HUD.
- **Occlusion cases**:
  1. *Pole fingers*: the left thumb and index clamp the poles; their tips occlude the two 60° caps, which are outside the tap band, so no gameplay information is hidden.
  2. *Tapping finger*: the right index covers one sector at a time; the sector gloss ring is drawn as a 2 mm annulus that extends 6 mm beyond the fingertip silhouette, so the target remains visible.
  3. *Neutron behind the ball*: a neutron approaching from behind is occluded by the ball; a red approach arc is drawn around the ball's silhouette, and the countdown ring is always on top.
  4. *Rings behind the hand*: the far half of each 150 mm ring is occluded by the hands; the near half plus a dashed far-half guide keeps the geometry readable.
  5. *Burst at the equator*: the burst VFX is exempt from hand occlusion for 80 ms so the origin is unmistakable.
- **VFX budgets**: burst 18, sector gloss (no particles), neutron trail 6/s, collection spark 20, absorption flash 24, shell surge 40/s, ring pulse 10/s, global cap 420.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Poles locked | ball | magnetic clamp thunk | north/south caps glow |
| Sector ready | sector | soft tick when cooldown ends | gloss ring fills |
| Tap/burst | sector | positron snap + rising zap | 18-particle spike |
| Neutron inbound | spawn point | low Doppler whoosh | approach arc, countdown |
| Deflection | neutron | rubber-band twang | curved trail |
| Collection | ring | bright orbital chime | spark burst, +100 |
| Heavy collection | ring | deeper chime + sub | gold burst, +250 |
| Absorption | ball | crunch + alarm | red flash, charge pip breaks |
| Miss | play edge | soft fizzle | trail fades |
| Shell surge | nucleus | rising electron choir | shells flare, sectors cool |
| Run clear | nucleus | atomic fanfare | tally card |
| Run failed | nucleus | collapse implosion | white-out |

## 13. Difficulty Progression & Tuning

| Level | Spawn gap (s) | Heavy share | Neutron speed (mm/s) | Sector cooldown (s) | Containment | Max simultaneous |
|---|---|---|---|---|---|---|
| 1 | 2.0 | 5 % | 96 | 1.4 | 5 | 3 |
| 2 | 1.8 | 10 % | 110 | 1.3 | 5 | 4 |
| 3 | 1.5 | 18 % | 120 | 1.2 | 4 | 4 |
| 4 | 1.2 | 25 % | 144 | 1.1 | 4 | 5 |
| 5 | 0.9 | 30 % | 168 | 1.0 | 3 | 6 |

- **Adaptive rule** (`DEFLECTOR_ADAPT`): after each run, if collected ≥ 24 and containment lost ≤ 2, level +1; after a failed run, level −1 and spawn gap ×1.1. Clamp 1–5; one step per run.
- **Named constants**: `R_BALL = 20 mm`, `TAP_V = 25 mm/s`, `SECTOR = 30°`, `DEAD_BAND = 3°`, `SECTOR_CD = 1.2 s`, `TAP_CD = 150 ms`, `BURST_CONE = 35°`, `BURST_RANGE = 120 mm`, `BURST_A = 160 mm/s²`, `BURST_T = 0.30 s`, `HEAVY_A = 256 mm/s²`, `RING_R = 150 mm`, `RING_TUBE = 15 mm`, `ABSORB_R = 26 mm`.

## 14. Edge Cases & Failure Recovery

- **Prop tracking loss > 150 ms**: freeze neutrons and burst timers; the ball becomes a 40 % ghost. On relock, the pole axis is re-fit and the longitude origin is rotated with it so the sector map does not jump.
- **Hand tracking loss**: losing the tapping hand disables bursts but keeps neutrons moving for 1.5 s, then pauses; losing the clamp hand freezes the sector map and disables taps immediately. Both lost > 300 ms pauses.
- **Ball dropped**: pole release > 400 ms → HOLD; on re-pinch, the sector origin is preserved if the pole axis is within 25°, else re-zeroed with a one-line notice.
- **Boundary taps**: within 3° of a sector boundary, the hysteresis rule assigns the sector of the most recent tap; if there is no history, the nearer center wins. A tap exactly on the pole cap is rejected.
- **Neutron spawned behind the ball with a straight-through path**: its approach arc and countdown are always drawn on top; it is never unreadable.
- **Burst spam**: the global 150 ms cooldown plus per-sector 1.2 s prevents more than ~6.7 taps/s, and a 3-tap burst within 300 ms triggers a 0.5 s "finger heat" lockout to prevent mashing.
- **Heavy neutron with 1 HP remaining**: a normal burst can finish it, but only a heavy burst produces enough impulse to collect it in one hit; this is intentional depth.
- **Containment at 1 and a heavy neutron absorbed**: costs 2 → 0, but the run ends with the current score; no negative charge.
- **Recovery UX**: one icon (ball / poles / hand) plus one line of text; no modal dialogs mid-run.

## 15. Implementation Architecture

- **Classes/structs**:
  - `PropBall` — owns `poseP`, sphere center, pole axis, equator contacts.
  - `PoleClamp` — north/south contact validation, map freeze, re-pinch.
  - `SectorMapper` — longitude → sector, boundary hysteresis, cooldowns.
  - `BurstSystem` — impulse shaping, cone test, heavy/normal, VFX hooks.
  - `NeutronPool` — 12 agents, radial paths, HP, drag, collection/absorption.
  - `RingSet` — three tori, plane crossing, tube containment.
  - `ContainmentDirector` — charges, spawn cadence, shell surge, scoring.
  - `NucleusRig` — nucleus, three shells, sector gloss, burst VFX.
  - `StreamRig` — starfield, neutrons, rings, approach arcs.
- **Event flow**: `HeadsetTracker → PropBall → PoleClamp → SectorMapper → BurstSystem`; `ContainmentDirector → NeutronPool → RingSet → EventBus {Burst, Deflect, Collect, Absorb, Miss, Surge, RunClear} → NucleusRig / StreamRig / VFXPool / AudioBank / ScoreKeeper`.
- **Pseudocode — tap → burst (contact event)**:

```text
on ContactBegin(c):
    if role(c.joint) not in {INDEX, MIDDLE} or not polesHeld: return
    phi = asin(clamp(dot(normalize(c.point - center), zhat_P), -1, 1))
    if abs(phi) > 35 deg: return                      # pole cap, reject
    lam = atan2(dot(c.point, yhat_P), dot(c.point, xhat_P))
    j = sectorWithHysteresis(lam)
    if now < sectorReady[j] or now < lastTap + 150 ms: return
    if tapsInLast300ms >= 3: fingerHeat(0.5 s); return
    heavy = (lastTapSector == j and now - lastTap < 220 ms)
    impulse = heavy ? HEAVY_A : BURST_A
    sectorReady[j] = now + (heavy ? 2*SECTOR_CD : SECTOR_CD)
    lastTap = now; lastTapSector = j
    for n in neutrons within BURST_RANGE and 35 deg of u_j:
        n.force += impulse * cross(u_j, normalize(n.pos - center))
    emit Burst(j, heavy)
```

- **Pseudocode — neutron integration (per tick)**:

```text
for n in neutrons:
    n.vel += n.force * dt
    n.force *= exp(-8 * dt)
    n.vel *= exp(-0.02 * dt)
    n.pos += n.vel * dt
    if |n.pos| < ABSORB_R: absorb(n); continue
    for ring in rings:
        if crossedPlane(n.prev, n.pos, ring) and radialDist(n, ring) < RING_TUBE:
            collect(n); break
    if |n.pos| > 500: miss(n); despawn(n)
```

## 16. Acceptance Test Checklist

1. Registration fits the ball center < 1.0 mm and the pole axis within 5° of the pinch line in 20/20 presentations; the calibration tap sets `λ = 0` within 2°.
2. All 12 sectors map correctly for taps at their center longitudes, and boundary taps within 3° use hysteresis rather than flipping.
3. Taps at `|φ| > 35°` are rejected; taps at `|φ| = 34°` are accepted.
4. A valid tap fires exactly one burst; a second tap in the same sector within 1.2 s is rejected.
5. A burst applies the specified shaped impulse over 0.30 s and curves a level-3 neutron by 40 ± 8 mm at 0.6 s.
6. A neutron crossing a ring plane within 15 mm of the torus center is collected exactly once.
7. A neutron reaching 26 mm from the center costs exactly 1 charge (2 for heavy) and never double-counts.
8. A heavy neutron requires 2 HP of burst damage; a normal burst removes 1.
9. Shell surge halves sector cooldowns for exactly 3.0 s and boosts impulse by 15 %.
10. Releasing a pole freezes the sector map and disables taps within one tick; re-pinching resumes the same origin if within 25°.
11. Covering the ball for 1 s freezes neutrons and timers; relock rotates the sector origin with the pole axis and introduces no false tap.
12. Finger heat activates after 3 taps in 300 ms and locks taps for 0.5 s.
13. A level-3 run of 30 neutrons is clearable with ≥ 24 collected and ≥ 1 charge remaining in 5/5 scripted runs.
14. Particle count stays ≤ 420 during shell surge + 3 simultaneous collections + one absorption.
