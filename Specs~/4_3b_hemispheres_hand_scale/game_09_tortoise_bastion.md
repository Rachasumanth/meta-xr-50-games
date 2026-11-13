# Tortoise Bastion

## 0. Identity Block

| Field | Value |
|---|---|
| Title | Tortoise Bastion |
| Slug | `tortoise_bastion` |
| Physical prop | Inverted bowl, dome-up; `R = 80.0 mm`, depth `60 mm`, wall `t = 4.0 mm`, mass `400 g` |
| Family / sub-shape | Hemisphere, dome-up, hand scale |
| Collider family | Convex dome + rim torus + apex cannon plate |
| Scale class | Prop 1:1 (160 mm shell); virtual fortress 700 mm across |
| Source concept quote | "Rest both palms on the rounded dome of the upside-down bowl and twist it in place while tapping specific shell plates on the dome... Rotate the shell to angle its thick armored plates toward incoming dragon fireballs, and tap the top apex to fire a spirit cannon from the turtle's back." |
| Physical input | Two-palm twist to yaw the dome; fingertip taps on individual shell plates; apex press to fire |
| Session length | 7 min (5 sieges × 75 s + onboarding) |
| Lose condition | Shell integrity reaches 0 after three breached plates |

## 1. Fantasy & Core Loop

The inverted bowl becomes the shell of a colossal world-turtle, its ceramic surface overlaid with hexagonal scutes, moss, and glowing rune seams. The turtle stands at the gate of a mountain fortress while dragons wheel overhead and spit fireballs. Each scute is an armor plate with a thickness and a facing normal. Twisting the bowl with both palms rotates the turtle, presenting thick plates to incoming fire and thin seams away from it. Tapping a scute with a fingertip makes it flare; tapping the apex fires the spirit cannon, a slow lance of green light that tracks the nearest dragon.

1. **Read the sky (1.5 s).** A dragon banks; a fireball telegraphs its impact point as a red arc on the dome.
2. **Rotate (0.6 s).** Both palms twist the dome so a thick plate faces the impact.
3. **Brace (0.4 s).** A press on the target plate hardens it (`+30%` thickness) for 1.2 s.
4. **Impact (0.2 s).** The fireball resolves as ricochet (grazing), absorbed (thick plate), or penetration (seam).
5. **Counter (0.5 s).** An apex tap fires the cannon; it must charge for 0.8 s first.
6. **Siege resolve (2.0 s).** Integrity and dragon damage tally.

A fireball cycle is 3.5–6 s; a siege is 75 s; five sieges fill the session.

## 2. Platform Baseline

- **Hardware**: standalone MR headset with color passthrough, scene mesh + support-surface (table/lap) detection, bare-hand skeletal tracking of 26 joints per hand at 72 Hz (render 90 Hz). No controllers; the only physical prop is the registered object named in this doc.
- **Joint naming**: `{L|R}_{finger}_{joint}`; fingers thumb/index/middle/ring/pinky; joints cmc,mcp,pip,dip,tip (thumb: cmc,mcp,ip,tip). Example: `R_index_tip`.
- **Prop registration**: at session start the player presents the object; system computes a 6-DoF prop pose `P` via visual odometry and tracks it continuously; automatic re-acquisition after tracking loss. **Hemisphere sub-shape**: the prop is a hemispherical shell of radius `R` and wall thickness `t`; model in P with the rim circle in the `x̂_P`–`ŷ_P` plane at `z = 0` and the apex at `z = ±R`. For **bowl-up** props the cavity faces `+ẑ_P` (inner surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`); for **dome-up** props the convex dome faces `+ẑ_P` (outer surface `p_P = R·(sinθ·cosλ, sinθ·sinλ, cosθ)`, cavity facing `−ẑ_P`). Rim torus: major radius `R`, minor radius `t/2`.
- **Frames**: `W` = room world (Y up, cm); `T` = detected support surface frame; `P` = prop local frame defined per game; `V` = virtual play frame (anchored to `P` or `T` as the game specifies).
- **Contact detection**: joint contact exists when signed distance from joint to prop collider surface < 5 mm; release hysteresis 8 mm; 30 ms debounce; each contact publishes point, normal, relative velocity, age.
- **Physics**: fixed 90 Hz timestep, max 2 sub-steps; virtual entities anchored to `P` or `T` unless soft dynamics specified; tabletop scale (< 40 cm).
- **Rendering/compositing**: passthrough base layer; virtual layer depth-composited against scene mesh and hand mesh; explicit occlusion cases specified per game.
- **Feedback**: spatial audio + visual effects substitute for haptic hardware; every gameplay event maps to an audio-AFX cue.

### 2.1 Game-Specific Overrides

- **Radius and wall**: `R = 80.0 mm`, depth `60 mm`, wall `t = 4.0 mm`, mass `m = 400 g`.
- **Orientation**: dome-up. Convex exterior faces `+ẑ_P`; cavity opens `−ẑ_P`. Apex `A_P = (0,0,+75.0) mm` (the dome height above the rim is 75 mm; the bowl depth 60 mm leaves a 15 mm foot pedestal), rim at `z_P = 0`.
- **P origin**: rim-circle centre. The bowl rests on the table; `P_O` is fixed in `T` and the dome yaws freely about the vertical.
- **Dome yaw**: `ψ = unwrap(atan2(x_P·ŷ_T, x_P·x̂_T))`; the fortress and dragon sky rotate with `ψ`.
- **Scute grid**: 6 rings × 12 sectors = 72 plates; ring `k ∈ [1,6]` spans `θ ∈ [(k−1)·10°, k·10°]` from the apex; sector `s ∈ [0,11]` spans `λ ∈ [s·30°, (s+1)·30°)`.
- **Plate thickness**: `t_k = 6.0 − 0.6·k` mm (thick near the apex, thin near the rim); seam width `w_seam = 1.2 mm` between plates.
- **V-frame**: anchored to `T` at `Q`; dragon sky is a 700 mm-radius hemisphere. `P` yaw drives the turtle's facing `α_t = ψ`.
- **Deviation 1**: dome yaw is tracked, not simulated; the palms drive it directly.
- **Deviation 2**: plates are ideal hexagonal patches with a binary "seam" penalty, not a continuous thickness field.
- **Deviation 3**: fireballs are `V`-space projectiles on ballistic arcs; they hit the dome proxy sphere, not the real ceramic.
- **Deviation 4**: the spirit cannon is a hitscan lance with a 0.8 s charge, not a projectile.

## 3. Physical Play Space & Ergonomics

The bowl rests dome-up on a low table or tray in front of the player. Both palms rest on the flanks; twisting is a forearm pronation/supination of ±45°. Fingertip taps land on individual scutes; the apex is reachable by either index. The head tilts 20–26° down, with upward glances at the dragon sky.

| Parameter | Value | Rationale |
|---|---|---|
| Support | Table `T`, 720–780 mm | Shoulder comfort |
| Dome position | 200–260 mm forward | Two-palm reach |
| Twist range | ±45° per motion, continuous yaw | Full facing |
| Twist rate | 20–100 °/s | Aim control |
| Tap force | 0.3–0.8 N | Plate brace, no topple |
| Apex press | 0.6–1.2 N | Cannon trigger |
| Gaze | 20–26° down + sky glances | Neck comfort |
| Session | 7 min; 75 s sieges; 12 s rest | Twist stamina |

Fatigue mitigation: the bowl's weight is carried by the table; the palms only twist; each siege ends with a 6 s hands-off rest; a twist-odometer suggests reversing direction every 15 s to avoid forearm strain.

## 4. Prop Geometry & Registration

A glazed bowl inverted; the dome is smooth, so the scute grid is a virtual overlay anchored to the registration pose. A small matte foot ring gives a feature at the apex.

| Property | Value | Notes |
|---|---|---|
| Outer radius `R` | 80.0 mm ± 1.0 | Silhouette |
| Dome height | 75.0 mm | Apex above rim |
| Rim circle radius | 50.9 mm | `R·sin 42.8°` |
| Wall `t` | 4.0 mm ± 0.5 | Rim step |
| Foot ring | 36 mm radius, 3 mm | Apex marker |
| Mass | 400 g ± 25 | Balance |
| Registration | rim ellipse + dome specular | 30 frames |
| Re-acquisition | `ε_R < 2.0 mm`, yaw error < 4° | After blur |

Feature atlas (outer dome):

| Ring `k` | `θ` range | Plate thickness | Gameplay |
|---|---|---|---|
| 1 | 0–10° | 5.4 mm | Apex / cannon |
| 2 | 10–20° | 4.8 mm | Inner crown |
| 3 | 20–30° | 4.2 mm | Crown |
| 4 | 30–40° | 3.6 mm | Mid shell |
| 5 | 40–50° | 3.0 mm | Lower shell |
| 6 | 50–60° | 2.4 mm | Marginal |

Sectors are 30° each; the seams run along all ring and sector boundaries. A fireball that lands on a seam has a 0.75 penetration multiplier; a plate ricochets if the impact angle (from the surface normal) exceeds 65°.

## 5. Hand Tracking Input Map

| ID | Joint(s) | Surface region | Gesture | Semantic output |
|---|---|---|---|---|
| I5.1 | both palms | lower dome | Twist | `Yaw(ψ)` |
| I5.2 | `R_index_tip` | any plate | Tap 0.3–0.8 N | `Brace(plate)` |
| I5.3 | `L_index_tip` | any plate | Tap | `Brace(plate)` |
| I5.4 | either index | apex ring `k = 1` | Press 0.6–1.2 N | `CannonCharge` |
| I5.5 | both palms | — | Lift 15 mm | `Release` |
| I5.6 | both hands | — | Open 800 ms | `Rest` |

Plate selection: `k = floor(θ/10°) + 1`, `s = floor(λ/30°)`, both clamped. A tap within 4 mm of a seam is rejected (the seam cannot be braced).

## 6. Input Kinematics & Signal Processing

- **Yaw**: `ψ` from the `P` quaternion, unwrapped; low-passed at 6 Hz. Yaw rate `ψ̇`; a twist above 100 °/s is "slew" and suspends plate bracing.
- **Plate facing**: a plate's normal at its centroid `n̂_k,s = (sinθ_c cosλ_c, sinθ_c sinλ_c, cosθ_c)` in `P`; in `T` it is `n̂_T = R_y(ψ)·n̂_P`.
- **Impact angle**: `φ = acos(|d̂_fb·n̂_T|)`; fireball direction `d̂_fb` in `T`.
- **Ricochet**: if `φ > 65°`, the fireball reflects with `e = 0.6` and deals no damage.
- **Penetration**: if the impact is within `1.2 mm` of a seam or the fireball's normal speed exceeds the plate's effective thickness `t_eff`, the plate breaches.
- **Effective thickness**: `t_eff = t_k·(1 + 0.3·braced)`; a fireball's penetration power `P_fb = 2.0 + 0.4·(siege_level−1)` mm.
- **Brace**: a tap sets `braced = 1` for 1.2 s; a second tap refreshes.
- **Cannon**: apex press for 0.8 s charges; on release a hitscan lance fires along the apex normal with a 6° cone. A dragon within the cone and 700 mm takes `damage = 1` (of 3 hp).
- **Breach**: a penetrated plate loses 1 integrity and is "scorched" (cannot be braced for 3 s).

## 7. Virtual World Layout & Coordinate Mapping

`V` is a 700 mm-radius fortress sky centred `Q`; the turtle shell sits at the origin, its apex at `z_V = 75 mm`. Dragons orbit at `r = 500–650 mm` and `z_V = 180–320 mm`.

| Entity | Anchoring | Notes |
|---|---|---|
| Shell | `P` | yaw `ψ` |
| Scute grid | `P` | 6 × 12 |
| Dragons | `V` | 2–4 agents |
| Fireballs | `V` | ballistic |
| Fortress | `V` | background |
| Cannon lance | `P` apex | hitscan |

The turtle's facing `α_t = ψ` determines which plates face the nearest dragon; the player rotates to present thick crown plates (rings 2–3) rather than thin marginal plates (rings 5–6).

## 8. Gameplay Systems & Rules

**Integrity.** Starts at 6 ("plates"). A penetration removes 1 and scorches the plate; a ricochet or absorb removes 0. At 0, the shell is breached and the siege ends.

**Dragons.** 2–4 dragons, each `hp = 3`, orbiting at 500–650 mm. They fire one fireball every 3.5–6 s, telegraphing the impact point 1.2 s ahead.

**Fireball resolution.** Ricochet if `φ > 65°` (no damage, 0.6 bounce, may hit another dragon for 0.5 damage). Absorb if `t_eff ≥ P_fb` and not on a seam. Penetrate otherwise.

**Cannon.** A charged apex press fires a lance; a dragon in the 6° cone takes 1 hp. Killing a dragon awards `+400` and briefly clears the sky.

**Score.** `S = 100·ricochets + 150·absorbs + 400·kills + 50·(integrity_left) − 100·penetrations`.

**Siege.** 75 s or shell integrity 0. Clearing a siege with ≥ 1 plate awards `+300`.

**Win.** Survive 5 sieges. **Lose.** Shell integrity 0 in any siege.

## 9. Game State Machine

| State | Entry | Exit | Timer / condition | Output |
|---|---|---|---|---|
| `Boot` | start | `Calibrate` | 1.0 s | — |
| `Calibrate` | bowl seen | `SiegeIntro` | 2.0 s | `P` locked |
| `SiegeIntro` | calibration / siege clear | `Sky` | 2.0 s | dragons spawn |
| `Sky` | siege active | `Impact` / `Counter` | fireball scheduled | orbit |
| `Rotate` | telegraph | `Brace` / `Impact` | 1.2 s | yaw |
| `Brace` | plate tap | `Impact` | 1.2 s | `t_eff` up |
| `Impact` | fireball lands | `Sky` / `Breach` | 0.2 s | resolve |
| `Breach` | penetration | `Sky` / `SiegeEnd` | 0.6 s | scorch |
| `Counter` | apex charged | `Sky` | 0.5 s | lance |
| `SiegeEnd` | 75 s or integrity 0 | `SiegeIntro` / `Victory` / `Score` | 2.5 s | tally |
| `Victory` | 5 sieges | `Score` | 2.0 s | fanfare |
| `Score` | victory / breach | `Boot` | 6.0 s | card |
| `Pause` | palm cover | previous | 1.0 s | freeze |

## 10. Physics & Collision

| Collider A | Collider B | Response | Material |
|---|---|---|---|
| Dome hemisphere | Palms | Twist input | ceramic/skin |
| Dome hemisphere | Fingertips | Brace tap | ceramic/skin |
| Apex plate | Index press | Cannon charge | ceramic/skin |
| Fireball sphere | Dome proxy | Resolve | — |
| Fireball | Dragon | Ricochet damage | — |
| Cannon lance | Dragon | Hitscan damage | — |

No rigid-body dynamics for the bowl; yaw is kinematic. Fireballs use ballistic arcs with `g = 9.81 m/s²` scaled by 0.05 (toy gravity). CCD: fireballs at 400 mm/s move 4.4 mm/tick; the dome proxy is a sphere with a 6 mm shell, so a 10 mm sweep is required. The cannon is hitscan and needs none.

## 11. Rendering, Occlusion & Compositing

Layer order: passthrough → table → fortress sky → dragons → fireballs → shell dome → scute grid → plate glow → spirit lance → hand mesh → embers → UI.

Occlusion cases:
1. **Palms on the dome**: hands render in front of the lower plates; the plate glow is clipped by the palm.
2. **Fingertip on a scute**: the tap spark is occluded by the fingertip.
3. **Dragon behind the shell**: a low dragon is hidden by the dome silhouette.
4. **Fireball over the apex**: the fireball passes in front of the apex, then behind the dome on the far side.
5. **UI**: the integrity pips float at the rim and are soft-occluded by the thumb.
6. **Scute seam**: seams render as dark grooves and are depth-tested so they never float.

VFX budget: shell 6 k tris (72 plates), dragons 4 × 1.5 k tris, fireballs 6 × 300 tris, lance 1 beam, embers ≤ 500.

## 12. Audio & Feedback

| Event | Audio | Visual |
|---|---|---|
| Dragon roar | Brass roar | Sky shake |
| Fireball telegraph | Rising crackle | Red impact arc |
| Dome twist | Stone grind | Plate slide |
| Brace tap | Metallic tap | Plate hardens |
| Ricochet | Spark ping | Fireball bounces |
| Absorb | Deep thud | Plate flashes |
| Penetration | Crack + hiss | Plate scorches |
| Cannon charge | Rising hum | Apex glows |
| Cannon fire | Green boom | Lance beam |
| Siege clear | Horn | Fortress gate opens |

## 13. Difficulty Progression & Tuning

| Level | Dragons | Fireball period (s) | Penetration power (mm) | Telegraph (s) | Integrity |
|---|---|---|---|---|---|
| 1 | 2 | 6.0 | 2.0 | 1.6 | 8 |
| 2 | 2 | 5.2 | 2.4 | 1.4 | 7 |
| 3 | 3 | 4.6 | 2.8 | 1.2 | 6 |
| 4 | 3 | 4.0 | 3.2 | 1.0 | 5 |
| 5 | 4 | 3.5 | 3.6 | 0.9 | 5 |

**Adaptive rule.** Track absorb rate `a` over the last 6 fireballs. If `a ≥ 0.8`, raise penetration power 0.2 mm or add a dragon (cap 4). If `a ≤ 0.35`, lower the power and lengthen the period. Clamp the period to `[3.0, 7.0] s`.

## 14. Edge Cases & Failure Recovery

- **Bowl lifted**: freeze the siege if `P` rises > 10 mm; re-seat prompt.
- **Tap on a seam**: rejected with a dull thud and no brace.
- **Two palms twisting opposite directions**: net yaw takes the average; no penalty.
- **Fireball hits a braced thin plate**: absorbs if `t_eff ≥ P_fb`, else penetrates.
- **Cannon fired with no dragon in the cone**: charge is consumed; a soft "fizzle" cue.
- **Integrity 0 mid-siege**: siege ends immediately; the next siege restores integrity to the level's value.
- **Tracking loss**: freeze fireballs for ≤ 400 ms; longer pauses.
- **Scorched plate**: cannot be braced for 3 s; a black overlay marks it.
- **Pause during a fireball**: resume recomputes the impact from the pre-pause state.

## 15. Implementation Architecture

Classes: `PropRegistration`, `YawTracker`, `ScuteGrid`, `PlateState`, `DragonAgent`, `Fireball`, `ImpactResolver`, `Cannon`, `IntegrityModel`, `SiegeDirector`, `ScoreLedger`, `AudioCueBus`, `OcclusionCompositor`.

Event flow: `PropRegistration → P, ψ`; `YawTracker → ψ`; `ScuteGrid` maps taps to plates; `DragonAgent → Fireball`; `ImpactResolver` reads plate `t_eff`; `Cannon` hitscans; all → `SiegeDirector`, `ScoreLedger`.

```text
onFireballImpact(fb, plate):
    phi = acos(abs(dot(fb.dir, worldNormal(plate, psi))))
    if phi > 65deg:
        ricochet(fb); score += 100; return
    t_eff = plate.thickness * (1 + 0.3*plate.braced)
    if fb.onSeam or t_eff < fb.power:
        plate.integrity -= 1; plate.scorched = 3.0
        score -= 100; audio.play("penetrate")
    else:
        score += 150; audio.play("absorb")
    if integrity <= 0: state = SiegeEnd
```

```text
onApexPress(force):
    if force >= 0.6N: charge += dt
    if charge >= 0.8 and released:
        if dragonInCone(apexNormal(psi), 6deg, 700mm):
            dragon.hp -= 1; score += 400
        charge = 0
```

## 16. Acceptance Test Checklist

1. **Yaw mapping**: a 90° bowl twist yields `ψ = 90° ± 2°`.
2. **Plate selection**: a tap at `θ = 25°, λ = 75°` selects ring 3, sector 2.
3. **Seam rejection**: a tap within 1.2 mm of a seam produces no brace.
4. **Ricochet**: an impact at 70° from the normal ricochets and deals 0 damage.
5. **Absorb**: a 4.8 mm plate against a 2.4 mm fireball absorbs.
6. **Penetration**: a seam hit with a 2.4 mm plate against a 2.0 mm fireball penetrates.
7. **Brace**: a braced plate gains 30% effective thickness for 1.2 s.
8. **Scorch**: a penetrated plate cannot be braced for 3 s.
9. **Cannon**: a 0.8 s apex press fires exactly one lance.
10. **Kill**: three lance hits kill a 3 hp dragon and award `+400`.
11. **Integrity**: six penetrations end the siege.
12. **Re-anchor**: reseating the bowl preserves yaw within 4°.
13. **Occlusion**: a low dragon is hidden by the dome.
14. **Performance**: 72 plates + 4 dragons + fireballs sustain 90 fps.
