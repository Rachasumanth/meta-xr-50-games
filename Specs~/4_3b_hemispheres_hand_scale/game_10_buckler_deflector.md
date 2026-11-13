# Buckler Deflector

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Buckler Deflector |
| Slug | `buckler_deflector` |
| Physical prop | Small bowl gripped by its raised foot ring; `R = 60.0 mm`, depth `45 mm`, wall `t = 4.0 mm`, mass `260 g`, foot-ring radius `27 mm` |
| Family / sub-shape | Hemisphere, bowl-up as a concave shield, hand scale |
| Collider family | Concave inner cavity + rim torus + foot-ring grip |
| Scale class | Prop 1:1 (120 mm buckler); virtual arena 600 mm across |
| Source concept quote | "Pinch or grip the raised circular foot-ring on the underside of the bowl (or hold its rim) like a shield handle, angling the concave interior outward by 10 degrees... the concave bowl catches incoming virtual fireballs inside its curved cavity, swirling them around the interior rim before slinging them back out in a concentrated spiral beam when you flick your wrist by 2cm." |
| Physical input | Left hand grips the foot ring; wrist angles the concave interior outward; a 20 mm wrist flick releases the beam |
| Session length | 6 min (5 waves × 60 s + onboarding) |
| Lose condition | Buckler overload (5 captive fireballs) or three body hits |

## 1. Fantasy & Core Loop

The small bowl becomes a bronze buckler strapped to the player's left forearm, its concave face etched with a spiral labyrinth. Fireball-throwing wraiths ring the player in a 600 mm arena. Instead of deflecting projectiles away, the buckler swallows them: a fireball that enters the cavity is caught by the curved wall and spirals inward, orbiting the rim with decreasing radius and increasing brightness. The player can hold up to five captive fireballs; each adds charge. A sharp 20 mm wrist flick slings all of them back out as a single concentrated spiral beam that punches through wraiths along a narrow cone.

1. **Guard (continuous).** Angle the concave face toward incoming fire; the cavity normal is the shield facing.
2. **Catch (0.2 s).** A fireball entering the rim cone is captured and begins to orbit.
3. **Swirl (1.0–3.0 s).** Captive fireballs spiral at 240–520 °/s; charge accumulates.
4. **Aim (0.5 s).** Rotate the wrist to point the cavity normal at a wraith cluster.
5. **Fling (0.15 s).** A 20 mm flick along the normal releases the beam; damage scales with captive count.
6. **Wave resolve (2.0 s).** Kills and overloads tally.

A wave is 60 s with 8–14 fireballs; five waves fill the session.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Hemisphere sub-shape**: the prop is a hemispherical shell of radius `R` and wall thickness `t`; model in P with the rim circle in the `x̂_P`–`ŷ_P` plane at `z = 0` and the apex at `z = ±R`. For **bowl-up** props the cavity faces `+ẑ_P` (inner surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`); for **dome-up** props the convex dome faces `+ẑ_P` (outer surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`, cavity facing `−ẑ_P`). Rim torus: major radius `R`, minor radius `t/2`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-VFX cue.

### 2.1 Game-Specific Overrides

- **Radius and wall**: `R = 60.0 mm`, depth `45 mm`, wall `t = 4.0 mm`, mass `m = 260 g`, foot-ring radius `r_f = 27 mm`.
- **Orientation**: bowl-up, used as a shield. Cavity faces `+ẑ_P`; the foot ring is on `−ẑ_P` and is the grip. The player holds the buckler so that `+ẑ_P` points away from the body, tilted 10° outward.
- **P origin**: rim-circle centre. `P_O` is at the hand; `P` is free (held), tracked at 72 Hz.
- **Cavity**: inner surface `p_P = R_in·(sinθ cosλ, sinθ sinλ, cosθ)`, `R_in = 56.0 mm`, `θ ∈ [0°, 48.6°]`; rim circle at `z_P = 0`.
- **Capture cone**: half-angle `γ = 32°` about `+ẑ_P`; a fireball is captured if it crosses the rim plane inside `r ≤ 50 mm` with `cosψ ≥ cosγ`.
- **Orbit model**: captive fireballs are polar `(ρ_i, φ_i)` on a virtual disc at `z_P = 8 mm`; `ρ` decays at `6 mm/s`, `φ̇ = 240 + 40·n_captive °/s`.
- **V-frame**: anchored to `P`; the arena is a 600 mm-radius disc around the player's chest, `V` origin at the buckler centre.
- **Deviation 1**: the buckler is tracked, not simulated; it follows the hand.
- **Deviation 2**: capture is an analytic cone test, not a full cavity collision.
- **Deviation 3**: captive fireballs are 2-D orbiters, not 3-D bodies; they render as glowing motes on the cavity face.
- **Deviation 4**: the release beam is a hitscan cone, not a projectile spray.

## 3. Physical Play Space & Ergonomics

The left hand grips the foot ring from below, knuckles toward the enemy, so the concave face points outward. The wrist holds a 10° outward tilt; aiming is wrist flexion/extension and radial/ulnar deviation. The right hand is free for balance or a brace. The head faces forward with 10–18° down-tilt.

| Parameter | Value | Rationale |
|---|---|---|
| Grip | Left hand, foot ring | Shield handle |
| Buckler face | 10° outward | Comfortable guard |
| Aim range | ±40° yaw, ±30° pitch | Wrist range |
| Flick distance | 18–24 mm along normal | Release gesture |
| Flick speed | 0.3–0.8 m/s | Detectable, not a throw |
| Captive max | 5 | Overload threshold |
| Gaze | 10–18° down | Arena visible |
| Session | 6 min; 60 s waves; 12 s rest | Wrist stamina |

Fatigue mitigation: the buckler is light (260 g) and braced against the forearm; the wrist holds a static guard between events; each wave ends with a 6 s arm-lower rest; a wrist-angle meter warns after 40 s of continuous extension.

## 4. Prop Geometry & Registration

A small glazed bowl, inverted grip, used as a buckler. The foot ring is a natural handle; the inner glaze gives a strong specular cue for the cavity normal.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 60.0 mm ± 1.0 | Silhouette |
| Depth | 45 mm ± 1.2 | Apex to rim |
| Inner radius `R_in` | 56.0 mm | `R − t` |
| Rim circle radius | 42.0 mm | `R_in·sin 48.6°` |
| Foot-ring radius | 27.0 mm ± 0.8 | Grip |
| Wall `t` | 4.0 mm ± 0.5 | Rim step |
| Mass | 260 g ± 15 | Balance |
| Registration | rim ellipse + specular | 30 frames |
| Re-acquisition | `ε_R < 2.0 mm`, axis < 4° | After blur |

Feature atlas (inner cavity):

| Station | `θ` | Feature |
|---|---|---|
| S0 | 0–10° | Spiral centre |
| S1 | 10–25° | Inner spiral groove |
| S2 | 25–40° | Capture band |
| S3 | 40–48.6° | Rim funnel |
| S4 | rim | Capture mouth |

Colliders: inner cap (capture trigger), rim torus (catch lip), foot ring (grip). Registration publishes `P` and the cavity normal `d̂ = q_P·ẑ_P`.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | `L_thumb_tip`, `L_index_tip`, `L_middle_tip` | foot ring | Grip hold | `Held` |
| I5.2 | `L_wrist` | — | Flex/extend + deviation | `Aim(d̂)` |
| I5.3 | `P` translation | — | 18–24 mm flick along `d̂` | `Fling` |
| I5.4 | `R_index_tip` | rim (optional) | Tap | `Vent` (dumps one captive) |
| I5.5 | `R_hand` | — | Raised palm | `Brace` (+10% capture cone) |
| I5.6 | both hands | — | Open 800 ms | `Rest` |

Aim `d̂` is the cavity normal in `V`. Fling detection uses the projection of the `P` displacement over 120 ms onto `d̂`; `Δ = 18–24 mm` and `v ≥ 0.3 m/s` fire the beam.

## 6. Input Kinematics & Signal Processing

- **Cavity normal**: `d̂ = q_P·ẑ_P`, low-passed at 10 Hz with a 60 ms latency cap.
- **Capture**: a fireball at position `p_f` with velocity `v_f` is captured if it crosses the rim plane (`z_P` sign change) inside `r ≤ 50 mm`, and `acos(d̂·(−v̂_f)) ≤ 32°`.
- **Orbit**: `φ̇_i = 240 + 40·n_captive °/s`; `ρ̇_i = −6 mm/s`; captured motes stack in brightness.
- **Charge**: `Q = Σ_i (1 + 0.2·i)`; the beam damage is `D = 40·Q`.
- **Fling**: `Δ = (p_P(t) − p_P(t−120 ms))·d̂`; if `Δ ≥ 18 mm` and `|Δ|/0.12 ≥ 0.3 m/s`, release all captives as one beam.
- **Beam cone**: half-angle `5°`, range 600 mm; a wraith hit deals `D` damage.
- **Overload**: if `n_captive > 5`, the buckler overheats: all captives detonate, the player takes 1 hit, and the buckler is disabled for 2 s.
- **Body hit**: a fireball that passes the rim plane outside the capture cone and within 120 mm of the chest deals 1 hit.

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 600 mm-radius arena disc centred at the buckler, with the player's chest at `r = 0, z_V = 0`. Wraiths orbit at `r = 350–550 mm`, `z_V = −80…+120 mm`. Fireballs travel inward at 300–500 mm/s.

| Entity | Anchoring | Notes |
|---|---|---|
| Buckler | `P` | cavity normal |
| Captive motes | `P` | 2-D orbit |
| Wraiths | `V` | 3–6 agents |
| Fireballs | `V` | inbound |
| Beam | `P` | hitscan cone |
| Arena wall | `V` | 600 mm |

The buckler is drawn at true size; the arena is compressed to a 600 mm disc so wraiths remain in the field of view.

## 8. Gameplay Systems & Rules

**Wraiths.** Each has `hp = 100`, a slow orbit, and a throw interval of 2.0–3.5 s. Fireballs are 20 mm spheres at 300–500 mm/s.

**Capture.** A caught fireball becomes a captive mote. The catch window is the rim plane crossing; late or wide fireballs hit the body.

**Charge.** Each captive adds 40 damage (stacking `+8` per extra). Five captives = 240 damage.

**Fling.** A wrist flick releases the beam; wraiths within the 5° cone and 600 mm take full damage, and a splash ring deals 25% within 10°.

**Overload.** A sixth captive triggers an overload: 1 body hit, 2 s disable.

**Body hits.** Three body hits end the wave.

**Wave.** 60 s or all wraiths dead. Killing a wraith awards `+150`; a wave clear awards `+300`.

**Score.** `S = 150·kills + 20·captives_flung + 300·wave_clear − 200·body_hits`.

**Win.** Clear 5 waves. **Lose.** Three body hits or three overloads.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | buckler seen | `Guard` | 2.0 s | `P` locked |
| `Guard` | calibration / wave start | `Catch` / `Hit` | fireball inbound | aim |
| `Catch` | rim crossing | `Swirl` | 0.2 s | captive |
| `Swirl` | captive added | `Guard` / `Fling` | 1.0–3.0 s | orbit |
| `Fling` | wrist flick | `Guard` | 0.15 s | beam |
| `Overload` | 6th captive | `Guard` / `WaveEnd` | 2.0 s | disable |
| `Hit` | body fireball | `Guard` / `WaveEnd` | 0.5 s | damage |
| `WaveEnd` | 60 s / all dead / 3 hits | `Guard` / `Victory` | 2.0 s | tally |
| `Victory` | 5 waves | `Score` | 2.0 s | fanfare |
| `Score` | victory / failure | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Rim plane disc | Fireball | Capture trigger | — |
| Foot ring | Left hand | Grip | ceramic/skin |
| Cavity | Captive motes | Orbit constraint | — |
| Beam cone | Wraith | Hitscan damage | — |
| Fireball | Body sphere | Body hit | — |
| Buckler | Incoming fireball | Deflect (if edge-on) | bronze |

The buckler is tracked, not simulated. Fireball capture is a plane-crossing test with a 2 mm tolerance; CCD is not needed because the rim plane is infinite in the capture test and the cone is evaluated at crossing. Captive motes are analytic orbiters. The beam is hitscan. A guard prevents capture if the fireball has already crossed the plane on a previous tick.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → arena floor → wraiths → inbound fireballs → buckler exterior → cavity spiral → captive motes → beam → hand mesh → sparks → UI.

Occlusion cases:
1. **Left hand behind the buckler**: the grip hand is mostly hidden by the bowl; the fingers show at the rim.
2. **Captive motes**: motes render on the cavity surface and are occluded by the near rim when orbiting the far side.
3. **Fireball entering**: a fireball is drawn in front until it crosses the rim, then clipped by the cavity.
4. **Beam through the hand**: the beam is depth-tested and dims behind fingers.
5. **UI**: the charge pips orbit the rim and are soft-occluded by the thumb.
6. **Overload**: the buckler glows red and a shockwave is depth-tested to the hand.

VFX budget: buckler 3 k tris, 6 wraiths × 700 tris, motes ≤ 5 sprites, beam 1 mesh, sparks ≤ 400.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Guard hum | Low drone | Spiral glows |
| Catch | Whoosh + click | Mote appears |
| Swirl | Rising whir | Motes orbit faster |
| Charge full | Harmonic ring | Rim brightens |
| Fling | Bronze roar | Beam fires |
| Wraith hit | Shatter | Wraith dissolves |
| Body hit | Thud + crack | Screen jar |
| Overload | Explosion | Red shockwave |
| Wave clear | Horn | Arena dims |

## 13. Difficulty Progression & Tuning

| Level | Wraiths | Fireball speed (mm/s) | Throw interval (s) | Capture cone | Captive max |
|---|---|---|---|---|---|
| 1 | 3 | 300 | 3.5 | 40° | 6 |
| 2 | 4 | 350 | 3.0 | 36° | 5 |
| 3 | 4 | 400 | 2.6 | 32° | 5 |
| 4 | 5 | 450 | 2.2 | 28° | 4 |
| 5 | 6 | 500 | 2.0 | 25° | 4 |

**Adaptive rule.** Track catch rate `c` over the last 8 fireballs. If `c ≥ 0.8`, narrow the capture cone 2° or raise fireball speed 20 mm/s. If `c ≤ 0.35`, widen the cone and slow the fireballs. Clamp the cone to `[20°, 45°]`.

## 14. Edge Cases & Failure Recovery

- **Buckler lowered**: if `d̂` points > 60° away from the chest normal, capture is disabled and a "raise guard" cue plays.
- **Fireball hits the edge**: treated as a body hit if within 120 mm of the chest, otherwise a miss.
- **Fling with zero captives**: a dry click; no beam.
- **Fling while overloaded**: disabled for 2 s; the flick is ignored.
- **Tracking loss**: hold the buckler pose for ≤ 300 ms; longer pauses.
- **Two fireballs in one tick**: both are captured if both cross inside the cone; `n_captive` increments twice.
- **Pause during a swirl**: motes freeze and resume.
- **Wraith behind the player**: it still throws; the player must turn the head or wrist; the arena is 360°.

## 15. Implementation Architecture

Classes: `PropRegistration`, `BucklerPose`, `CaptureCone`, `CaptiveOrbit`, `ChargeModel`, `FlingDetector`, `BeamCaster`, `WraithAgent`, `Fireball`, `BodyHitModel`, `WaveDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P, d̂`; `CaptureCone` tests fireballs; `CaptiveOrbit` updates motes; `ChargeModel → Q`; `FlingDetector → BeamCaster`; `BeamCaster` damages wraiths; all → `WaveDirector`, `ScoreLedger`.

```text
onFixedTick(dt = 1/90):
    d = registration.cavityNormal()
    for fb in fireballs:
        if crossedRimPlane(fb):
            r = distance(fb.p.xy, P.origin.xy)
            if r <= 50mm and acos(dot(d, -normalize(fb.v))) <= captureCone:
                captives.append(new Captive(fb))
                audio.play("catch")
            else if withinBody(fb, 120mm):
                bodyHits += 1; audio.play("body_hit")
    for c in captives:
        c.phi += (240 + 40*len(captives)) * dt
        c.rho = max(0, c.rho - 6*dt)
    if len(captives) > maxCaptive: overload()
```

```text
onFlick():
    disp = (P.position - P.positionPrev120ms)
    delta = dot(disp, d)
    if delta >= 18mm and length(disp)/0.12 >= 0.3:
        damage = 40 * sum(1 + 0.2*i for i in 1..len(captives))
        hits = wraithsInCone(d, 5deg, 600mm)
        for w in hits: w.hp -= damage
        captives.clear()
        audio.play("bronze_roar"); vfx.beam(d)
```

## 16. Acceptance Test Checklist

1. **Cavity normal**: the reported `d̂` matches the tracked bowl axis within 2°.
2. **Capture cone**: a fireball at 30° incidence is captured; at 35° it is not (level 3).
3. **Rim radius**: a crossing at `r = 48 mm` captures; at 55 mm it does not.
4. **Orbit rate**: five captives orbit at 440 ± 20 °/s.
5. **Charge**: five captives yield 240 ± 8 damage.
6. **Fling**: a 20 mm, 0.5 m/s flick releases the beam; a 10 mm flick does not.
7. **Cone**: a wraith at 4° takes full damage; at 8° it takes none.
8. **Overload**: a sixth captive triggers a 2 s disable and 1 body hit.
9. **Body hit**: a fireball outside the cone within 120 mm deals 1 body hit.
10. **Dry fling**: a flick with no captives produces no beam.
11. **Wave**: 60 s or all wraiths dead ends the wave.
12. **Re-anchor**: losing tracking for 300 ms preserves captive orbits.
13. **Occlusion**: far-side motes are hidden by the near rim.
14. **Performance**: 6 wraiths + fireballs + beam sustain 90 fps.
