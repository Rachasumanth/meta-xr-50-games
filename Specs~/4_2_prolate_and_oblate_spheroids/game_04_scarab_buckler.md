# Scarab Buckler

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Scarab Buckler |
| Slug | `scarab_buckler` |
| Physical prop | Oval soap bar, 90 × 60 × 30 mm nominal; 110 g; smooth, slightly convex broad faces, matte finish |
| Registered contact spheroid | Oblate; `a = b = 30.0 mm`, `c = 15.0 mm` (60 × 60 × 30 mm fitted face); the soap's 90 mm long axis is a grip extension outside the collider |
| Family / sub-shape | 4.2 Prolate and Oblate Spheroids / oblate, palm-held, convex reflector |
| Scale class | Physical prop 1:1; virtual engagement hemisphere 650 mm; tabletop-scale seated play |
| Source concept quote | "An iridescent beetle carapace; the doubly curved surface controls the spread angle of ricocheting laser beams to hit multiple targets." |
| Physical input | Left palm grips the soap with the convex broad face outward; shallow wrist tilts aim the face; right index braces the rim |
| Session length | 6 min: onboarding 45 s + 5 waves × 55 s + 4 intermissions × 8 s + results 30 s |
| Lose condition | Hull integrity reaches 0 |

## 1. Fantasy & Core Loop

The soap is the shed carapace of a scarab that ate light for a living. Held convex-side out, it is a buckler: every laser that strikes the dome splits into a fan of refracted beams, and the curvature decides how wide the fan opens. The player does not pull a trigger. The player *tilts*: a 6° wrist roll swings the fan across 20° of sky, and a 10 mm shift of the impact point opens or closes the fan from a needle to a 60° scythe. The carapace also stores what it deflects. At full charge, the next beam detonates into a 360° ring that clears the whole sky — but the same energy cooks the shell, and an overheated scarab is just a bar of soap.

1. **READ (0.5 s).** A targeting line paints from a gate to the buckler. The beam's radius and speed are coded by color: cyan 12 mm lance, amber 18 mm splitter, violet 24 mm heavy.
2. **PRESENT (0.3 s).** The player squares the convex face to the gate. The face normal must be within 55° of the incoming direction or the beam grazes the rim.
3. **TILT (0.2–0.6 s).** Wrist roll and pitch rotate the fan. A small amber reticle on the face shows the predicted fan center and half-angle.
4. **DEFLECT (0.05 s).** The beam hits; five sub-rays reflect from five points across the beam footprint. The fan's angular width is `4w/R` radians, where `w` is the beam radius and `R = 60 mm` is the face curvature.
5. **SWEEP (0.4 s).** The fan crosses the target belt. Each target inside a 3° cone takes one hit. A 3-target sweep builds a combo and adds charge.
6. **VENT (0.8 s).** Heat climbs with every deflect. The right index can tap the rim to dump 40 heat at the cost of 30 charge; the choice between saving for a nova and staying cool is the core tension.

A wave is 10–16 beams and 6–12 targets. A clean wave is four deflects that each sweep three targets, with one nova at the end. A panicked wave is six deflects at the rim, a scorched shell, and two beams through the shoulder.

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

`P` origin is the centre of the fitted face spheroid; `+ẑ_P` points out of the convex face (away from the palm), `+x̂_P` runs along the soap's long axis toward the thumb, and `+ŷ_P` runs across the bar toward the pinky. Exact semiaxes are `a = b = 30.0 mm` and `c = 15.0 mm`; the real 90 mm bar extends 15 mm beyond the fitted 60 mm face at each end of `x̂_P`, and that overhang is a grip feature outside the collider. Because `c < a`, the prop is oblate, and the face's local radius of curvature is `R_p = a²/c = 60.0 mm`. The local rolling model is `ω = (n × v_t)/r_eff`, with `r_eff = |p_P|`; at the face pole `r_eff = 15.0 mm`, at the equator `30.0 mm`. `V` is anchored to the seated player's torso, origin 250 mm in front of the sternum, `x̂_V` forward, `ŷ_V` left, `ẑ_V` up; the buckler is drawn at its tracked `P` inside `V`. Four deviations:

1. **Grip extension.** The 90 mm soap length and the rim are outside the 60 × 60 × 30 mm face spheroid. Only the central dome reflects; the overhanging ends are hand-held and never tested as optics.
2. **Tracked reflector, no rigid body.** The buckler is not simulated as a free body. Its orientation comes from `q_P`; wrist tilt is read, not derived from torque. A dropped soap is a tracking event, not a physics event.
3. **Five-ray fan model.** The beam is sampled at five points across its footprint; the reflected fan is a piecewise direction set, not a continuous wavefront. This is sufficient for 3° target cones and keeps the per-beam cost under 0.2 ms.
4. **Charge and heat are separate scalars.** Charge is banked deflection energy (0–100, decays 4/s); heat is thermal load (0–100, cools 10/s). A nova consumes charge but adds heat, so the two meters pull in opposite directions.

## 3. Physical Play Space & Ergonomics

The player sits with the left forearm supported and the left palm supinated at sternum height, 280–340 mm in front of the chest. The soap is held across the palm with its long axis roughly horizontal; the convex face points forward and slightly up. The right index braces the near rim; the right palm may cup the left wrist for extra stability. The wrist is free to roll ±30° and pitch ±20°; beyond those ranges the forearm rotates, and the tracker's confidence in the face normal drops. The game therefore maps only ±25° of wrist tilt to the full ±70° fan sweep, using a 2.8× gain.

| Parameter | Value | Rationale |
|---|---|---|
| Posture | Seated, left forearm supported | Keeps the buckler steady through a 55 s wave |
| Buckler centre | 280–340 mm forward, 40 mm below eye line | Leaves room for the 650 mm target hemisphere |
| Face normal | Within 55° of the gate direction | Beyond 55° the beam grazes the rim |
| Wrist roll | ±25° mapped to ±70° fan yaw | Full sweep without forearm rotation |
| Wrist pitch | ±20° mapped to ±45° fan elevation | Covers the target belt |
| Grip | Left palm + thumb; right index on rim | Two contacts keep the face square |
| Session | 5 waves × 55 s; 8 s rests | Sustained isometric hold is the limiting load |
| Break cue | Lower the buckler overlay after wave 3 | Releases the wrist extensors |

The soap is held with a *loose* palm, not a fist: the thenar and hypothenar pads contact the back face, the thumb wraps the near edge, and the fingers curl without touching the convex side. A firm grip increases tremor by 20–30 % and makes the 3° target cone unreadable. The right index brace is optional but reduces face-normal jitter from 1.4° to 0.8° RMS, which is the difference between a 3-target sweep and a 2-target sweep.

## 4. Prop Geometry & Registration

| Property | Value | Notes |
|---|---|---|
| Nominal soap | 90 × 60 × 30 mm | Oval bar, both broad faces convex |
| Fitted face spheroid | `a = b = 30.0 mm`, `c = 15.0 mm` | 60 × 60 × 30 mm; 15 mm grip overhang each end |
| Mass | 110 g | Cured vegetable soap, density ≈ 1.02 g/cm³ |
| Face curvature | `R_p = a²/c = 60.0 mm` | Paraxial mirror radius |
| Face cap | 50° half-angle | Usable optical aperture, 51.3 mm diameter |
| Inertia about `ẑ_P` | `I_z = 2ma²/5 = 3.96 × 10⁻⁵ kg·m²` | Used only for wrist-jerk filtering |
| Surface friction | µ = 0.34 dry, 0.18 wet | Wet soap is a drop hazard |
| Face reflectivity (virtual) | 0.92 at 650 nm | Carapace shader, not a real optical property |

**Surface-feature atlas (P-fixed; λ from `+x̂_P` toward `+ŷ_P`, φ from equator):**

| Feature | φ | λ | Extent | Purpose |
|---|---|---|---|---|
| Face pole | +90° | any | 5 mm cap | Optical centre; zero-offset beam gives symmetric fan |
| Optical cap | +40° to +90° | all | 50° cap | Reflection aperture; outside this the rim scatters |
| Carapace seam 1 | all | 0° | 1.5 mm line | Readable roll reference |
| Carapace seam 2 | all | 90° | 1.5 mm line | Readable pitch reference |
| Carapace seam 3 | all | 180° | 1.5 mm line | Long-axis reference |
| Carapace seam 4 | all | 270° | 1.5 mm line | Cross-axis reference |
| Rim bevel | +30° to +40° | all | 10° band | Scatter zone, 50 % energy |
| Grip back | −90° | any | lower cap | Palm contact, never optical |
| Heat vents | +35° | ±45°, ±135° | 4 × 6 mm | Vent VFX and heat readout |
| Charge sigil | +60° | 180° | 12 mm | Charge ring, emissive |

The atlas cell size at the equator is `2πa/64 = 2.95 mm`. The optical cap is sampled by the beam fan at five points across the beam diameter, so the effective optical resolution is the beam radius divided by two, not the texture resolution.

**Collider inventory:**

| ID | Shape | Dimensions | Role |
|---|---|---|---|
| C0 | Spheroid | `a = b = 30.0`, `c = 15.0 mm` at `P` | Buckler body and depth proxy |
| C1 | Spherical cap | 50° about `+ẑ_P` | Optical aperture |
| C2 | Ray set | 5 rays per beam, radius 12–24 mm | Reflection samples |
| C3 | Capsule | r = 7 mm, length 18 mm | Left palm grip |
| C4 | Capsule | r = 5 mm, length 14 mm | Right index brace |
| C5 | Sphere | r = 8 mm | Target mote |
| C6 | Capsule | r = 90 mm, length 240 mm | Player torso/head hit volume |
| C7 | Disc | r = 26 mm | Rim scatter annulus |

Registration: the player presents the soap at 0.8–1.2 m, rolls it through 120° about `x̂_P`, and holds the convex face toward the headset for 0.5 s. The fitter uses the 90 mm silhouette for the long axis and the 60 × 30 mm profile for the face; acceptance requires RMS < 0.6 mm, long-axis error < 2.0°, and face-normal error < 2.0°. Re-acquisition searches a 140 mm cube for 1.2 s; success needs 8 rim samples with RMS < 0.9 mm over 6 frames. During loss, the face normal is frozen and incoming beams are paused after 0.8 s so the player is not hit by an un-deflectable shot.

## 5. Hand Tracking Input Map

| ID | Gesture | Joints | Contact surface (lat/long or axis) | Semantic output |
|---|---|---|---|---|
| I5.1 | Palm grip | `L_palm`, `L_heel`, `L_thumb_mcp` | φ −90° to −40°, λ 90°–270° | `HOLD`; `V` anchor and face validity |
| I5.2 | Wrist roll | `q_P` about `x̂_P` | — | Fan yaw, ±70° at ±25° wrist |
| I5.3 | Wrist pitch | `q_P` about `ŷ_P` | — | Fan elevation, ±45° at ±20° wrist |
| I5.4 | Face present | Left palm pushes 10–25 mm forward | `P` translation | Hit aperture +15 %, heat +2/s |
| I5.5 | Rim brace | `R_index_tip` on the rim | φ +30° to +40° | Face-normal jitter −40 % |
| I5.6 | Two-hand support | `R_palm` under left wrist | no prop contact | Tremor −25 %, no aim change |
| I5.7 | Vent heat | Right index taps rim twice in 300 ms | φ +35°, λ ±45° | Heat −40, charge −30 |
| I5.8 | Release | All left-hand contacts lost > 150 ms | — | `DROP`; beams paused 0.8 s |

**Per-finger roles.** The left palm and heel carry the buckler; the left thumb wraps the near edge and supplies the roll reference. The left fingers never touch the convex face. The right index is the only rim effector: it braces (continuous contact) or vents (double tap). The right middle, ring, and pinky may support the left wrist but never command the face. A right-hand contact on the convex face is treated as a brace at 25 % weight and immediately triggers a "hand off the face" audio cue, because it blocks the optical aperture.

Contact gating: a grip contact is valid when signed distance < 5 mm, age > 30 ms, and the contact normal points into the palm. The face normal is taken from the tracked `q_P`, not from the grip contacts, because the soap can rotate slightly inside the palm. A face normal that changes faster than 180°/s is filtered as a wrist jerk and does not steer the fan.

## 6. Input Kinematics & Signal Processing

The face normal in `V` is `n̂₀ = q_P·ẑ_P`. An incoming beam has direction `d̂` (unit, pointing from the gate toward the player) and a footprint radius `w`. The central ray intersects the spheroid at `p` if `|p| = 1` on the unit-sphere mapping. The surface normal at the hit is

`n̂ = normalize(cosφ·cosλ/a, cosφ·sinλ/a, sinφ/c)`,

and the reflected direction is `r̂ = d̂ − 2(d̂·n̂)·n̂`. For a beam centred at an offset `e` from the face pole, the paraxial reflection angle is

`α ≈ 2·e/R_p`,

so a beam of radius `w` produces a fan from `2(e − w)/R_p` to `2(e + w)/R_p`. With `R_p = 60 mm`, a centred 12 mm lance spans `±22.9°` (45.8° total); a beam offset 18 mm spans `11.5°` to `57.3°` on one side. This is the entire aiming model: *offset opens the fan, tilt rotates it*.

The fan is sampled at five points across the beam footprint: `e_i = e + w·{−1, −0.5, 0, +0.5, +1}` along the beam's local offset axis. Each sample reflects independently. A target is hit when the angle between a reflected sub-ray and the target direction is < 3.0°, and the target is within 500 mm. A target can be hit by at most one sub-ray per beam; the first hit in the sorted sample order claims it. The 3.0° cone at 350 mm is an 18.3 mm spot, comfortably larger than the 8 mm target but smaller than the 26 mm face, so the fan reads as discrete beams rather than a spray.

**Face aim.** Let `n̂₀` be the face normal and `d̂` the incoming beam. The specular direction for a centred ray is `r̂₀ = d̂ − 2(d̂·n̂₀)n̂₀`. The fan's rotation about `r̂₀` is controlled by the wrist roll; its elevation by wrist pitch. The game maps `roll_wrist ∈ [−25°, +25°]` to `yaw_fan ∈ [−70°, +70°]` with a 2.8× gain and a 1.5° dead zone. The mapping is applied in the plane perpendicular to `r̂₀`, so a tilted face never produces a gimbal flip. A 6° wrist roll moves the fan 16.8°, which is 0.29 target-widths at 350 mm; the target belt is spaced 18°, so a 6° roll is one deliberate target step.

**Wrist jerk filter.** Raw `q_P` is differentiated at 72 Hz, converted to an axis-angle rate, and clamped to 180°/s. A second-order Butterworth at 9 Hz smooths the roll and pitch; the face normal is reconstructed from the filtered angles. This rejects the 4–7 Hz tremor of a held arm while preserving a 0.12 s flick. The filter's group delay is 18 ms, which is below the 0.35 s beam travel time and therefore does not affect aiming.

**Heat.** `H ← clamp(H + 12 + 0.30·|e| − 10·dt, 0, 100)` per deflect. A heavy beam adds 20 instead of 12. At `H ≥ 100`, the shell overheats for 2.5 s: the face becomes non-reflective, beams pass through, and the player takes normal miss damage. Heat vents (I5.7) subtract 40 heat and 30 charge, with a 0.8 s animation during which the face is still reflective but the fan gain drops to 0.7×.

**Charge.** `C ← clamp(C + 12 + 0.25·hits − 4·dt, 0, 100)` per deflect. At `C = 100`, the next deflection becomes a nova: a 360° ring of 24 sub-rays at 300 mm that destroys every target in range, consumes all charge, and adds 20 heat. Charge cannot be banked above 100; a player at 100 who does not deflect loses 4/s to decay, so the nova is a *now* decision, not a hoard.

## 7. Virtual World Layout & Coordinate Mapping

`V` is anchored to the torso: origin 250 mm in front of the sternum, `x̂_V` forward, `ŷ_V` left, `ẑ_V` up. The buckler is drawn at its tracked pose, clamped to a 60 mm cube around the chest anchor. Five gates sit on a 650 mm radius hemisphere in front of the player at azimuths −60°, −30°, 0°, +30°, +60° and elevations −10° to +25°. Targets orbit on a 300–450 mm shell between the gates and the buckler, spaced at least 18° apart. The camera is the head pose; the gates are rendered at 650 mm, beyond the hand, so the player reads depth from parallax rather than from a flat overlay.

| Entity | V-space position | Size | Notes |
|---|---|---|---|
| Buckler | `p_P` clamped to ±60 mm | 60 mm face, 90 mm bar | Player-held reflector |
| Gate | 650 mm, 5 azimuths | 40 mm ring | Beam origin, telegraph |
| Beam | gate → buckler | r = 12–24 mm, length 650 mm | 0.9–1.6 m/s |
| Target mote | 300–450 mm shell | r = 8 mm, HP 1–2 | Orbits at 4–9°/s |
| Fan sample | buckler face | 5 rays | Reflected directions |
| Nova ring | buckler centre | r = 300 mm | 24 rays, 360° |
| Torso hit volume | chest | capsule r = 90 mm | Miss damage receiver |
| Heat/charge HUD | above the buckler | 2 arcs, 28 mm | Dry, player-facing |

Mapping: the physical soap's translation is clamped to ±60 mm in `V`; the face normal is used directly, with no amplification beyond the wrist gain. The beam's incoming direction is computed from the gate to the buckler centre, not from the gate to the face hit point; this keeps the fan model stable when the player shifts the buckler. The hit point is the ray–spheroid intersection closest to the gate. The target belt is drawn as a faint 18° grid, which lets the player read fan width without a numeric HUD.

## 8. Gameplay Systems & Rules

**Entities.** Buckler with face normal and 50° optical cap. Player hull 100. Five gates. Targets: 6–12 motes per wave, HP 1 (80 %) or 2 (20 %), orbit speed 4–9°/s. Beams: lance (w = 12 mm, 1.6 m/s, 1 hull if missed), splitter (w = 18 mm, 1.2 m/s, splits into two 8 mm rays if it hits the rim), heavy (w = 24 mm, 0.9 m/s, 2 hull if missed, +20 heat on deflect).

**Wave structure.** Each wave seeds 10–16 beams from the five gates, 2.4–1.8 s apart, with 6–12 targets already orbiting. A wave lasts 55 s or until all beams are resolved and all targets are destroyed. Targets respawn only at wave boundaries; destroying all targets early ends the wave with a 5 s bonus. The buckler must survive; there is no dodging, only deflection.

**Deflection rule.** A beam hits the optical cap if its central ray intersects the spheroid within 50° of the face pole and `d̂·n̂ < 0`. If the hit is between 40° and 50°, the beam is in the rim bevel: it scatters with 50 % energy, producing three sub-rays in a 90° cone and dealing 1 heat. If the central ray misses the spheroid entirely, the beam hits the torso volume for 12 hull (lance/splitter) or 20 hull (heavy). A beam that hits the back face (`d̂·n̂ > 0`) is a miss.

**Fan resolution.** Five sub-rays are generated at offsets `e_i` across the beam footprint. For each, the reflected direction is tested against every live target within 3.0° and 500 mm. A target hit once takes 1 damage and is removed at 0 HP. A beam can destroy at most three targets; the fourth and fifth hits are converted to 25 score each and a "overkill" spark. This cap keeps a single centred heavy beam from clearing the whole belt and preserves the need for wrist aim.

**Charge nova.** At `C = 100`, the next deflection becomes a nova instead of a fan: 24 rays uniformly spaced in the plane perpendicular to the face normal, range 300 mm, 1 damage each, no target can be hit twice. The nova consumes all charge, adds 20 heat, and plays a 0.6 s white-out. It is the only way to clear targets behind the player's shoulder, which the fan cannot reach.

**Heat and vent.** Heat rises 12 per deflect (20 heavy), 20 per nova, and 2/s while presenting (I5.4). It cools 10/s. At 100, overheat for 2.5 s. Venting (I5.7) subtracts 40 heat and 30 charge; it is unavailable during overheat. The design keeps the player cycling: deflect → heat rises → vent or overheat → charge falls → rebuild.

**Combos.** Each target destroyed within 1.2 s of the previous one increments the combo. `multiplier = 1 + 0.20·min(combo, 5)`, max 2.0. A miss or a rim scatter resets the combo. The fan's five sub-rays resolve in order, so a well-aimed sweep can chain three targets in one beam and start the next beam at 1.6×.

**Score.**

`deflect_score = 20 · (1 + hits/5)`

`target_score = 100 · multiplier · (1 + 0.5·(1 − angle_error/3°))`

`nova_score = 300 + 50·targets_destroyed`

`wave_score = 500 + 4·hull_remaining + 10·time_remaining`

`run_score = Σ wave_score + Σ target_score + Σ deflect_score + Σ nova_score − 25·hull_lost`

The target precision term rewards a hit at the center of the 3° cone; a graze is worth 1.0×, a center hit 1.5×. The run score floors at 0.

**Win/lose.** Survive five waves with hull > 0 to win. Hull 0 is immediate loss. A wave that ends with all targets destroyed but hull ≤ 0 is still a loss; the score is recorded but the run ends.

## 9. Game State Machine

| State | Event | Guard | Action | Next |
|---|---|---|---|---|
| BOOT | session start | — | Load carapace, start tracker | PRESENT |
| PRESENT | soap registered | RMS < 0.6 mm | Show buckler ghost | CALIBRATE |
| CALIBRATE | face presented 1.0 s | face normal within 20° of forward | Set `V` anchor, zero heat/charge | WAVE_INTRO |
| WAVE_INTRO | timer 2.5 s | wave ≤ 5 | Show gate pattern and target count | ORBIT |
| ORBIT | tick | wave active | Orbit targets, run telegraphs | ORBIT |
| ORBIT | beam spawn timer | active beams < 3 | Spawn beam at gate, 0.6 s telegraph | INBOUND |
| INBOUND | tick | `t < t_impact` | Track face, show predicted fan | INBOUND |
| INBOUND | `t ≥ t_impact` | central ray in cap, `d·n < 0` | Reflect fan, resolve hits, heat/charge | SWEEP |
| INBOUND | `t ≥ t_impact` | rim bevel | Scatter 3 rays, heat +1, no charge | SWEEP |
| INBOUND | `t ≥ t_impact` | miss | Hull damage, combo reset | SWEEP |
| SWEEP | timer 0.4 s | — | Despawn beam, check nova | ORBIT |
| ORBIT | charge = 100 and deflect | — | Nova instead of fan | NOVA |
| NOVA | timer 0.6 s | — | 24 rays, clear targets, heat +20 | ORBIT |
| ORBIT | targets = 0 and beams = 0 | hull > 0 | Wave clear, bank score | WAVE_END |
| ORBIT | hull = 0 | — | Shell cracks, controls freeze | GAME_OVER |
| WAVE_END | timer 2.0 s | wave < 5 | Restore 10 hull, cool 30 heat | INTERMISSION |
| WAVE_END | wave = 5 | hull > 0 | Results | RESULTS |
| INTERMISSION | timer 8 s | — | Show heat/charge tips | WAVE_INTRO |
| GAME_OVER | tap "retry" | — | Reset hull, keep wave 1 | WAVE_INTRO |
| ANY | tracker loss > 1.5 s | — | Pause beams, freeze face | RECOVERY |
| RECOVERY | re-acquired | RMS < 0.9 mm | Resume with 0.5 s grace | previous |

Timers run at 90 Hz. Beam spawn and impact are wall-clock-latched; a frame stall cannot slow a beam. The telegraph is 0.6 s for all beam types, which is long enough to read the gate color and short enough that a player cannot re-grip. The state machine processes at most two substeps and then clamps `dt` to 22.2 ms.

## 10. Physics & Collision

| Collider | Type | Motion | Notes |
|---|---|---|---|
| C0 buckler | Spheroid 30/30/15 mm | Tracked `P` | Depth proxy and optical surface |
| C1 cap | 50° about `+ẑ_P` | Rigid to `q_P` | Deflect aperture |
| C2 beam | 5 rays, w = 12–24 mm | Kinematic | 0.9–1.6 m/s |
| C5 target | Sphere r = 8 mm | Orbiting | 4–9°/s |
| C6 torso | Capsule r = 90 mm, length 240 mm | Static in `V` | Miss receiver |
| C7 rim | Annulus 40°–50° | Rigid | 50 % scatter |
| C3/C4 hands | Capsules | Tracked | Grip/brace only |

**Pair matrix.**

| Pair | Test | Response |
|---|---|---|
| C2 × C1 | central ray intersects spheroid, `φ_hit < 50°`, `d·n < 0` | Five-ray fan, heat +12, charge +12 |
| C2 × C7 | `40° < φ_hit < 50°` | Three-ray 90° scatter, heat +1, no charge |
| C2 × C0 back | `d·n > 0` | Treated as a miss |
| C2 × C6 | ray misses the spheroid and hits the torso capsule | Hull −12 (heavy −20), combo = 0 |
| C2 fan × C5 | angle < 3.0°, distance < 500 mm | Target −1 HP, combo++, score |
| C1 × C5 nova | 24 rays, range 300 mm | Target −1 HP, no double hit |
| C3 × C0 | signed distance < 5 mm | Grip; face validity |
| C4 × C0 | signed distance < 5 mm | Brace; jitter −40 % |

**Materials.** Buckler = "carapace": reflectivity 0.92, no restitution, heat capacity 1.0 per point. Beam = "coherent light": no mass, no drag, constant speed per type. Target = "mote": 1 HP, no restitution, despawns with 12 sparks. Rim = "bevel": 50 % energy scatter, no damage to the buckler. Torso = "soft": absorbs the beam, hull damage only.

**CCD.** Beams are rays, not moving spheres; the ray–spheroid intersection is exact and continuous, so there is no tunnelling. The five fan sub-rays are also infinite rays, tested analytically against target spheres; a target moving 9°/s at 350 mm moves 0.55 mm per tick, far below the 18.3 mm cone, so discrete tests are sufficient. The nova uses 24 rays fired simultaneously; target hits are resolved in a fixed order to keep the result deterministic.

**Forces.** No rigid-body force is simulated. The wrist-tilt mapping is a kinematic gain, and the beam's momentum is not transferred to the prop (the real soap would not move). The only physical quantity is heat, which is a scalar with a 10/s cooling rate. The 50 % rim scatter is an energy rule, not a contact impulse. This is deliberate: a 110 g soap held in one hand cannot absorb a laser's momentum without a recoil model that would fight the player's wrist.

## 11. Rendering, Occlusion & Compositing

Layer order, near to far: passthrough base → torso/shoulder scene mesh → gate rings at 650 mm → incoming beam trails → target motes → buckler depth proxy → carapace shell and charge sigil → reflected fan → hand mesh → HUD heat/charge arcs. The buckler proxy writes depth at the 60 × 60 × 30 mm face spheroid; the carapace shell is drawn 0.5 mm outside it, and the fan originates 1.0 mm off the surface, so the beams do not z-fight with the soap.

**Occlusion cases.**

- **O1 — Beam behind the buckler.** The incoming beam trail is depth-tested against the spheroid; the segment behind the soap is hidden. The beam's leading tip remains visible at the rim until impact, so the player still reads the timing.
- **O2 — Fan in front of the hand.** The reflected fan is drawn in front of the carapace but behind the hand mesh. A fan ray that would pass through the gripping fingers is clipped, which is correct: the hand really does block that direction.
- **O3 — Target behind the head.** A target at azimuth > 100° is culled by the torso capsule and the head mesh. The nova can still hit it (the ring passes around the body), so the HUD shows a small "rear target" pip when one exists.
- **O4 — Gate behind the player's shoulder.** A gate at ±60° azimuth is partially occluded by the shoulder mesh. Its ring is drawn as a 40 mm annulus with a 2 mm depth offset, so the visible arc still telegraphs the beam direction.
- **O5 — Soap over the palm.** The buckler proxy occludes the left palm; the fingers wrap around the rim and are drawn in front. A 2 mm contact shadow on the palm seats the soap.
- **O6 — HUD versus buckler.** The heat and charge arcs are rendered at 240 mm from the eye and depth-tested against the soap. A raised buckler correctly covers the arcs; they fade to 35 % rather than disappearing, so the player can still read an overheat at a glance.

**VFX budgets.** Incoming beam: 1 quad strip per beam, 64 segments, 0.4 ms. Fan: 5 line strips, 32 segments each, 0.6 ms. Target mote: 2 sprites each, max 12. Nova: 24 line strips, 0.5 ms, 1 concurrent. Carapace: one 60 mm shell mesh, 3.2 k triangles, 0.9 ms; iridescent shader is a single-pass thin-film approximation, no refraction. Impact sparks: 40 particles per deflect, max 3 concurrent. Total transparent overdraw target < 2.0× screen area. The carapace is deliberately opaque over passthrough so the real soap's silhouette remains the anchor.

## 12. Audio & Feedback

| Event | Spatial origin | Sound | Visual cue |
|---|---|---|---|
| Gate charge | gate | Rising 300→900 Hz hum, 0.6 s | Ring contracts, color codes beam type |
| Beam launch | gate | 1.6 kHz snap + 120 Hz thump | Beam trail extends at 0.9–1.6 m/s |
| Deflect | face | 2.2 kHz crystal ping + 400 Hz body, 180 ms | Fan flashes, 5 rays, heat +12 |
| Rim scatter | rim | 900 Hz metallic scrape, 120 ms | 3 scattered rays, 50 % energy |
| Target destroyed | target | 1.4 kHz pop + 300 Hz sparkle, 200 ms | 12 sparks, mote vanishes |
| Combo step | target | Pop pitch +2 semitones | Combo number scales 1.0→1.2 |
| Nova | buckler centre | 80 Hz boom + 3 kHz choir, 600 ms | White ring, 24 rays, screen bloom |
| Overheat | face | 1.8 kHz hiss + 90 Hz rattle, 500 ms | Shell glows red, face dims |
| Vent | rim | 600 Hz steam release, 400 ms | Heat arc drops, charge arc drops |
| Miss / hull hit | torso | 45 Hz sub drop + 200 Hz crack, 400 ms | Red vignette, 2 mm shake |
| Wave clear | gates | Major arpeggio, 900 ms | Targets bow, gate rings fade |
| Hull low | HUD | 220 Hz heartbeat, 1 Hz | Hull ring pulses red |
| Tracker loss | head-locked | 180 Hz muted pulse | "Show the soap" prompt |

World cues are spatialised with a 1.5 m rolloff; HUD cues are dry. The deflect ping is the most important cue: its pitch rises with the number of targets hit (one target 2.2 kHz, two 2.5 kHz, three 2.8 kHz), so the player hears the fan's width without counting sprites. The game has no haptics; the 400 Hz body thump under the ping is the substitute for the physical kick of a deflection.

## 13. Difficulty Progression & Tuning

| Level | Beams | Beam radius | Travel time | Targets | Heavy share | Heat rate | Notes |
|---|---|---|---|---|---|---|---|
| 1 | 10 | 10 mm | 0.70 s | 6 | 0 % | 0.8× | Teaching: wide fan, no heavies |
| 2 | 12 | 12 mm | 0.60 s | 8 | 10 % | 0.9× | First splitter and heavy |
| 3 | 13 | 12 mm | 0.55 s | 9 | 15 % | 1.0× | Baseline spec |
| 4 | 14 | 14 mm | 0.45 s | 10 | 20 % | 1.1× | Tighter timing, wider beams |
| 5 | 16 | 16 mm | 0.35 s | 12 | 25 % | 1.2× | Finale: 3 concurrent beams |

Named constants: `A = 0.030 m`, `C = 0.015 m`, `R_P = 60.0 mm`, `FACE_CAP = 50°`, `BEAM_R = 12 mm`, `SUBRAYS = 5`, `CONE = 3.0°`, `TARGET_R = 8 mm`, `HEAT_DEFLECT = 12`, `HEAT_HEAVY = 20`, `HEAT_COOL = 10/s`, `OVERHEAT = 100`, `OVERHEAT_TIME = 2.5 s`, `CHARGE_DEFLECT = 12`, `CHARGE_DECAY = 4/s`, `NOVA_COST = 100`, `NOVA_RANGE = 300 mm`, `VENT_HEAT = 40`, `VENT_CHARGE = 30`, `HULL = 100`, `MISS_LANCE = 12`, `MISS_HEAVY = 20`, `WAVES = 5`.

Adaptive rule: after each wave, if the player cleared with ≥ 15 s left and hull ≥ 70, increase the heavy share by 5 % (cap 30 %) and reduce travel time by 0.03 s (floor 0.32 s) for the next wave. If hull ≤ 30 or the wave was lost, reduce the heavy share by 5 % (floor 0 %) and add 0.05 s of travel time (ceiling 0.80 s). Adaptation never changes `R_P`, `CONE`, or `SUBRAYS`, because the optical feel of the soap must stay constant.

## 14. Edge Cases & Failure Recovery

- **Soap slips in the palm.** Grip contacts fall below two for > 150 ms → `DROP`. Beams are paused 0.8 s, the shell flashes grey, and the player may re-grip without a hull penalty. If the soap falls below the chest for > 1.5 s, the game pauses and asks for re-presentation.
- **Face normal flips.** If the tracked `q_P` rotates more than 120° in one frame, it is treated as a tracking glitch; the previous normal is held for 100 ms and the fan is not fired until the normal stabilises. A real flip to the back face is a miss and is scored as such.
- **Beam hits the rim exactly at 50°.** The hit is classified as rim scatter if `φ_hit ≥ 40°`, and as optical if `φ_hit < 40°`; the 40°–50° band is always scatter. This gives a 10° visible bevel and avoids a hard optical edge.
- **Beam hits the long-axis overhang.** The overhang is outside the spheroid and has no collider, so the ray passes through and hits the torso. The carapace mesh is drawn only over the spheroid, which makes the overhang visually a grip, not a shield. The player learns not to block with the ends.
- **Overheat during a beam.** A beam that impacts during overheat passes through the face and deals miss damage. The HUD's heat arc turns red 0.4 s before 100, and the shell's iridescence desaturates, so the state is readable before the hit.
- **Nova at 100 charge with no targets.** The nova still fires, consumes charge, and adds 20 heat; it is not refunded. This prevents a player from hoarding a nova indefinitely and keeps the heat/charge loop honest.
- **Tracking loss mid-beam.** The face normal freezes after 200 ms; beams already in flight continue for 0.8 s, then pause. On re-acquire, the normal snaps with a 100 ms ease; the beam resumes from its paused position, not from the gate.
- **Left-handed player.** Mirror mode swaps the grip to the right hand, the brace/vent finger to the left index, mirrors the gate azimuths about 0°, and flips the fan yaw sign. The reflection math is unchanged because it is computed in `V` from `q_P`.
- **Wet soap.** If the player's hand is wet (detected by a 0.18 friction estimate and a 15 % drop in grip-contact count), the game reduces the wrist-gain to 2.2× and shows a "dry your hands" prompt. It does not pause, because the physical prop is still trackable.

## 15. Implementation Architecture

**Classes.**

- `PropTracker` — registers the soap, emits `PoseFrame {p_P, q_P, confidence, mode}` at 72 Hz.
- `ContactBus` — 26 joints → `Contact {joint, p_P, n_P, v_rel, age}` with 5 mm enter / 8 mm exit and 30 ms debounce.
- `BucklerOptics` — face normal, optical cap test, ray–spheroid intersection, surface normal.
- `FanSolver` — five sub-ray offsets, reflection, target-cone tests, hit ordering.
- `WristAim` — roll/pitch extraction, jerk clamp, 2.8× gain, fan rotation.
- `HeatCharge` — heat and charge scalars, vent, overheat, nova trigger.
- `BeamDirector` — gate schedule, beam types, telegraph, travel, miss resolution.
- `TargetField` — orbiting motes, HP, respawn, rear-target pip.
- `WaveStateMachine` — the table in §9.
- `ScoreCombo` — deflect, target, nova, wave, run scores.
- `FeedbackRouter` — every event enum to one audio clip and one VFX prefab.
- `Compositor` — layer order, spheroid depth proxy, occlusion masks O1–O6.

**Event flow.** `PropTracker` → `PoseFrame` → `BucklerOptics` and `WristAim`; `ContactBus` → `WristAim` and `HeatCharge`; `BeamDirector` → `BucklerOptics` → `FanSolver` → `TargetField`, `HeatCharge`, `ScoreCombo`, `FeedbackRouter`; `TargetField` → `WaveStateMachine` → `FeedbackRouter`. The state machine reads only `TargetField.live`, `BeamDirector.active`, `HeatCharge.heat`, and the hull; it never reads raw joints.

**Pseudocode 1 — beam deflection and fan resolution.**

```
function resolve_beam(beam):
    hit = ray_spheroid(beam.origin, beam.dir, A, C)
    if hit == null:
        apply_miss(beam); return
    phi = asin(clamp(hit.p.z / C, -1, 1))
    if phi >= 50deg or dot(beam.dir, hit.n) >= 0:
        if phi >= 40deg:
            scatter_rim(beam, hit); return
        apply_miss(beam); return

    e_axis = normalize(beam.offset - dot(beam.offset, hit.n)*hit.n)
    hits = []
    for k in [-1, -0.5, 0, 0.5, 1]:
        p = surface_point(hit.p, k * beam.radius * e_axis)
        n = spheroid_normal(p)
        r = normalize(beam.dir - 2*dot(beam.dir, n)*n)
        for target in TargetField.live():
            if angle(r, normalize(target.p - p)) < 3deg and |target.p - p| < 500mm:
                hits.append((target, angle_error))
    unique = first_hit_per_target(hits)          # sorted by sample order
    for (target, err) in unique[:3]:
        target.hp -= 1
        ScoreCombo.on_target(err)
        FeedbackRouter.play(TARGET, target.p)
    HeatCharge.add_deflect(beam, length(e_axis))
```

**Pseudocode 2 — heat, charge, and nova.**

```
function tick_thermal(dt):
    heat = clamp(heat - 10*dt, 0, 100)
    charge = clamp(charge - 4*dt, 0, 100)
    if heat >= 100 and not overheated:
        overheated = true
        overheat_timer = 2.5s
        FeedbackRouter.play(OVERHEAT, face_pole)
    if overheated:
        overheat_timer -= dt
        if overheat_timer <= 0: overheated = false

function on_deflect(beam):
    if overheated:
        apply_miss(beam); return
    if charge >= 100:
        nova(face_pole, 300mm, 24 rays)
        charge = 0
        heat = clamp(heat + 20, 0, 100)
        FeedbackRouter.play(NOVA, face_pole)
        return
    fan = FanSolver.resolve(beam)
    heat = clamp(heat + 12 + 0.30*beam.offset, 0, 100)
    charge = clamp(charge + 12 + 0.25*fan.hits, 0, 100)
    FeedbackRouter.play(DEFLECT, beam.hit_p)
```

**Determinism.** Gate schedules, beam types, and target orbits use a PCG32 stream keyed by `(session_id, wave, beam_index)`. Fan resolution sorts samples by a fixed index order, and target iteration is by stable ID, so a recorded pose trace replays bit-identically at 90 Hz.

## 16. Acceptance Test Checklist

| ID | Test | Procedure | Pass criterion |
|---|---|---|---|
| AT-01 | Spheroid registration | Present the soap at 1.0 m; inject 24 synthetic rim points with 0.3 mm noise | RMS < 0.6 mm; long-axis error < 2.0°; face normal error < 2.0° |
| AT-02 | Reflection law | Hit the pole with `d = (0,0,−1)`, `n = (0,0,1)` | `r = (0,0,+1) ± 0.1°`; energy 0.92 |
| AT-03 | Fan spread, centred | Beam radius 12 mm, offset 0; sample five sub-rays | Angles −22.9°, −11.5°, 0°, +11.5°, +22.9° ± 1.0° |
| AT-04 | Fan spread, offset | Offset 18 mm, radius 12 mm | Angles +11.5° to +57.3° ± 2.0°; fan is one-sided |
| AT-05 | Target cone | Target at 350 mm, 2.9° from a sub-ray; repeat at 3.1° | 2.9° hits once; 3.1° misses; no double hit per beam |
| AT-06 | Wrist aim gain | Roll wrist 6°, then 25° | Fan yaw 16.8° ± 1.0°; 70.0° ± 2.0°; dead zone 1.5° |
| AT-07 | Heat accumulation | Deflect five lances in 0.5 s | Heat 55 ± 2 after five; cooling 10/s afterwards |
| AT-08 | Overheat | Deflect nine lances in 0.8 s | Heat 100; 2.5 ± 0.1 s lockout; beams pass and deal miss damage |
| AT-09 | Charge nova | Deflect ten lances; fire the next | Charge reaches 100; nova fires 24 rays; all targets < 300 mm destroyed; heat +20 |
| AT-10 | Vent | At heat 80, charge 70, double-tap rim | Heat 40 ± 1; charge 40 ± 1; 0.8 s vent animation; no overheat |
| AT-11 | Miss damage | Fire a lane beam past the buckler at the torso | Hull −12; heavy −20; combo resets; red vignette event |
| AT-12 | Rim scatter | Hit at φ = 45° | Three rays in a 90° cone; heat +1; no charge; 50 % energy |
| AT-13 | Occlusion readback | Place a target behind the buckler; read depth buffer | Target fragment contribution = 0 inside the 60 mm silhouette |
| AT-14 | Tracking loss | Drop `PoseFrame` for 2.0 s during INBOUND | Face freezes after 200 ms; beams continue 0.8 s, then pause; 0.5 s grace on re-acquire |

All tests run headless with synthetic pose, contact, and beam streams. AT-13 is a depth-buffer readback, not a screenshot. AT-03 and AT-04 use the shipped `R_p = 60.0 mm` and the same five-sample order as the runtime, so an optics tuning change fails the tests rather than silently changing the fan feel.
