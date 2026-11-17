# ShapeForge 50 - Production Guide

`ShapeForge 50` is the single Meta XR application that combines all 50 chapter
games into one gesture-driven, mixed-reality arcade.

- Unity **6000.0.64f1**
- Meta XR Core SDK **207.0.1**
- Oculus XR Plugin **4.5.6** (OVRPlugin + Meta XR Simulator / OpenXR bridge)
- Android: **IL2CPP / ARM64 / min SDK 32 / target SDK 34**
- Single scene: `Assets/Scenes/ShapeForge.unity`

---

## 1. The player experience

1. The app starts in the **Forge Hub**: a floating control table with five
   spatial-family portals, a central Forge Orb, a quick-shape strip and a live
   real-object readout.
2. The player pinches and traces a shape in the air:
   | Gesture | Result |
   |---|---|
   | closed circle | True Sphere |
   | tall oval | Prolate Spheroid |
   | wide/flat oval | Oblate Spheroid |
   | open arc `∩` | Hemisphere |
   | open arc `∪` | Domed Bowl |
3. On release the recogniser reports family, measured diameter, aspect,
   confidence and the best matching game. A translucent hologram previews the
   result.
4. **Convert** (pinch, or the CONVERT button) runs a scan-cage conversion. The
   selected mini-game is instantiated at the gesture/real-object anchor, at
   real-world scale, with its original gate-verified rig attached.
5. The player can also browse all 50 games explicitly through the five family
   portals, or use QUICK SHAPES on the right of the table.

### Presentation and soundtrack

The first launch shows a holographic **welcome card** with the title, the five
family glyphs, an ENTER THE FORGE action and MUSIC / SOUND toggles. The card
blocks hub input until dismissed, then the Forge Menu performs a staggered
entrance animation.

The hub is alive: an emissive Forge Orb with counter-rotating rings, orbiting
motes, a point light and a breathing floor ring; five holographic family icons;
a gesture trail with an outer glow; scan-ring conversion cages; sparkle bursts
on control presses, conversions, bonuses and victories; floating score pop-ups;
and a HUD with a smoothly filling score bar, score-pop and low-timer pulse.

The CC0 soundtrack is streamed and crossfaded per mood:

| Context | Track | Author |
|---|---|---|
| Hub / menus | Observing The Star | yd |
| Sphere action families | Fast Fight Battle Music (looped) | XCVG |
| Spheroid family | I Swear I Saw It | yd |
| Dome / bowl families | Mystical Enigmatic Background Music | CleytonKauffman |

All four tracks are CC0 1.0. Author/source credits are embedded at
`Assets/Resources/ShapeForgeMusic/CREDITS.txt` and shipped inside the APK.

### Real-object conversion

The `RealObjectProbe` requests the Meta scene model at startup. It binds scene
planes and volumes to `OVRSceneVolume` / `OVRScenePlane` templates with mesh
colliders, reads `OVRSemanticLabels`, and publishes a measured
`RealObjectProfile` (label + width/height/depth + pose). The matchmaker blends
the measured object size with the gesture size, then picks the family:

- flat/wide real object (table, desk, bowl) -> dome/bowl family
- oblong object (couch, bottle) -> spheroid family
- ball-like object -> sphere family
- size determines pinch vs palm / pinch vs hand-scale chapter

The conversion uses the real object's pose as the board anchor and its measured
size as the board scale (clamped to each game's playable range).

---

## 2. Controls

### Headset / simulator

- **Index pinch**: interact, trace shapes, grab props.
- **Controller trigger/grip**: same as pinch; thumbstick/pointer rays select UI.
- **Menu (☰)**: pause an active game (also recenter in the hub).
- **B / Two**: exit a mini-game.
- **F1** (editor/desktop): toggle desktop simulation.
- **F2**: toggle hand cursor spheres.
- **F12**: capture a screenshot.

### Desktop fallback (no XR device)

The app automatically creates a desktop camera and `DesktopHandSimulator` when
no XR device is active. This is the path used by CI and by the headless
self-test.

- Mouse = right-hand pointer; **hold LMB** = pinch.
- **hold RMB** = left-hand pinch.
- **Shift** = left hand follows the right pointer (two-hand tests).
- **WASD/QE + middle-drag** = move/look.
- **Space/Enter** = select, **M/Tab** = menu/pause, **Esc** = back/exit.
- **1..5** = Circle / Tall Egg / Flat Pebble / Dome / Bowl quick gestures.

---

## 3. Architecture map

```
Assets/MetaXRSimApp/
  Core/
    ShapeKind.cs                 family + gesture vocabulary
    ShapeGestureRecognizer.cs    pure-C# circle/oval/arc recogniser + shape analysis
    RealObjectProfile.cs         measured real object + shape inference
    GameDefinition.cs            catalogue + direct rig-type table (IL2CPP safe)
    GameMatchmaker.cs            gesture + object -> one of 50 games
  Runtime/
    AppBootstrap.cs              rig/passthrough/camera/services/entry
    RuntimeSelfTest.cs           50-game headless player test
    ShapeForgeHub.cs             gesture state machine + conversion
    Hub/ForgeMenu.cs             family portals, game browser, status board
    Hub/RealObjectProbe.cs       OVR scene anchors, dimensions, semantics
    MiniGameHost.cs              conversion, spawn, rig binding, lifecycle
    RigBinder.cs                 reflectively wires rig Transform fields
    MiniGameSession.cs           physical interaction + objective loop + HUD
    GrabInteractable.cs          pinch/controller grab and throw
    Visuals/MiniGameVisualBuilder.cs  themed boards for all 50 games
    Visuals/RigDrivenPropGuard.cs     stabilises rigs that write world-space props
    Visuals/Tweener.cs                deterministic tween runner + easing
    Visuals/VisualFX.cs               pulse glow, sparkles, score popups, orb/holograms
    Visuals/MaterialLibrary.cs        cached Standard/UI/sprite/glow materials
    AudioDirector.cs                  procedural SFX kit + persisted SOUND toggle
    Audio/MusicDirector.cs            streaming CC0 soundtrack + crossfades
    Input/HandInputService.cs    OVRHand/OVRInput/synthetic hand facade
    Input/DesktopHandSimulator.cs
    UI/WorldLabel.cs / WorldButton.cs / MiniGameHUD.cs
    UI/WelcomeOverlay.cs              branded entry card + audio toggles
  Editor/
    ShapeForgeSetup.cs           one-click project + scene + manifest + music import
    ShapeForgeValidator.cs       258-check production gate
    ShapeForgeBuilder.cs         signed APK + desktop player
  Tests/Editor/ShapeForgeTests.cs  18 new EditMode tests
Assets/Resources/ShapeForgeMusic/  4 streamed CC0 tracks + CREDITS.txt
Assets/Scenes/ShapeForge.unity   the only shipped scene
Assets/Plugins/Android/AndroidManifest.xml  Meta store-compatible manifest
Assets/link.xml                  preserves the 50 rigs for IL2CPP
```

The original 50 games under `Assets/Games/` are unchanged. The hub attaches
their per-game rigs at runtime; the gameplay cores remain the gate-tested code.

---

## 4. Build

### One-time setup

Install via Unity Hub for `6000.0.64f1`:
- Android Build Support, Android SDK & NDK Tools, Android OpenJDK 17
- Windows Build Support (for the desktop self-test)

Then run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.64f1\Editor\Unity.exe" `
  -batchmode -quit -nographics `
  -projectPath "C:\Users\Administrator\UnityProjects\MetaXRSim" `
  -executeMethod MetaXRSim.EditorTools.ShapeForgeSetup.Configure
```

This configures the Oculus XR loader (Android + Standalone), Meta feature
flags, the store-compatible Android manifest, Android player settings, icon,
quality settings, the always-included runtime shaders, the streamed CC0 music
import settings, and writes `Assets/Scenes/ShapeForge.unity`.

To validate:

```powershell
... -executeMethod MetaXRSim.EditorTools.ShapeForgeValidator.ValidateCI
```

### Signed APK

```powershell
... -executeMethod MetaXRSim.EditorTools.ShapeForgeBuilder.BuildAndroidCI
```

Output: `Build/ShapeForge50_v1.0.0.apk`.

Signing credentials can be supplied with:
`SHAPEFORGE_KEYSTORE`, `SHAPEFORGE_KEYSTORE_PASS`, `SHAPEFORGE_KEY_ALIAS`,
`SHAPEFORGE_KEY_PASS`.

Without env vars, the builder generates a local development key at
`Build/Keys/shapeforge.keystore` (git-ignored). **Replace it with your own
production key before publishing to Horizon Store / Play.**

### Desktop self-test player

```powershell
... -executeMethod MetaXRSim.EditorTools.ShapeForgeBuilder.BuildDesktopCI
```

Then:

```powershell
& "Build\Windows\ShapeForge50.exe" -shapeforge-selftest `
  -logFile "selftest.log"
```

Exit code `0` means all 50 games spawned and every rig bound.

Full soak/stress/design-criteria run (about 9 minutes, headless):

```powershell
& "Build\Windows\ShapeForge50.exe" -batchmode -nographics -shapeforge-stress `
  -logFile "stress.log"
```

Exit code `0` and `[STRESS] RESULT passed=True` mean: all 50 games played 30 s by a
scripted one-handed player and rewarded in time, no error logs, no hangs, no leaks
after enter/exit churn and misuse, bus-stop / airplane-seat / EXIT / entry-flow
criteria met, CPU frame cost within budget. The JSON report is written to
`shapeforge_stress.json` beside the self-test report. Use
`-shapeforge-stress-games slug1,slug2` for a verbose per-game diagnosis run. Add
`-shapeforge-capture` to write PNGs (requires a rendering GPU).

The final APK build reported **0 errors / 16 warnings**. Meta XR SDK 207 can
emit a non-fatal post-build telemetry `NullReferenceException` for the desktop
test player on offline machines; the player still builds and runs.

For an automated welcome-card/menu/music smoke test:

```powershell
& "Build\Windows\ShapeForge50.exe" -batchmode -nographics `
  -shapeforge-ui-smoke -logFile "ui_smoke.log"
```

The log must contain `[UI_SMOKE] built=True ... musicClip=True` and
`[UI_SMOKE] dismissed blocked=False menuActive=True` with no exceptions.

---

## 5. Meta XR Simulator

The simulator is installed at
`C:\Program Files\MetaXRSimulator\v207.1`.

1. Run `WindowsAppRuntime\install.bat` once (Windows App Runtime 1.5).
2. Start `MetaXRSimulator.exe` and leave the frontend running.
3. In Unity, open `Assets/Scenes/ShapeForge.unity` and press Play. The
   Standalone XR loader is already assigned to the Oculus loader; OVRPlugin
   connects to the simulator runtime.
4. The Forge Hub appears in the simulated headset. Use the simulator's input
   panel for hands/controllers.

For headless CI, the simulator ships a batch config. Set:

```powershell
$env:META_XRSIM_CONFIG_JSON = Get-Content `
  "C:\Program Files\MetaXRSimulator\v207.1\config\sim_core_configuration_ci.json" -Raw
```

### Verified limitations of this build machine

This machine has no usable D3D11/D3D12 display adapter for the simulator
compositor (`DXGI_ERROR_UNSUPPORTED` / "Failed to create eye textures"), so the
simulator XR display cannot be fully composited here. Attempted workarounds, all
logged under `%APPDATA%\MetaXR\MetaXrSimulator\logs`:

| Attempt | Result |
|---|---|
| D3D11 / D3D12 WARP | simulator Vulkan compositor cannot start (no Vulkan ICD) |
| Edge SwiftShader ICD (Vulkan 1.0) | missing instance extensions, `vkRet != VK_SUCCESS` |
| Mesa Lavapipe 26.2.6 (Vulkan 1.4, registered under `HKLM\SOFTWARE\Khronos\Vulkan\Drivers`) | Vulkan device creates; swapchain fails: `VK_KHR_external_memory_win32` unsupported (needed to share D3D11 textures) |
| Mesa Dozen (Vulkan on D3D12/WARP) | `vkGetMemoryWin32HandleKHR failed`, same swapchain failure |
| Vulkan Windows player | Oculus XR Plugin on Windows is D3D11-only; Unity refuses the build |

The simulator therefore needs a machine with a real GPU driver (any D3D11 GPU
with Vulkan external-memory support). The app explicitly
detects this and falls back to the desktop camera/input path; all logic, all 50
game spawns, rig binding, grabbing and scoring are still executed and verified
headlessly. Run the same APK/player on a normal GPU workstation for the visual
simulator pass.

---

## 6. QA evidence (this build)

| Check | Result |
|---|---|
| EditMode suite | **718 total, 702 passed, 0 failed, 16 NeedsDevice skipped** |
| ShapeForge production gate | **261/261 checks passed** |
| Runtime self-test | **50/50 spawned, 50/50 rigs bound, 0 failed** |
| Synthetic pinch path | **50/50 props grabbed** through the real pinch input path |
| 30 s scripted-play soak (all 50 games) | **50/50 rewarded within 30 s, 29 won, 0 errors, 0 hangs** |
| Enter/exit churn | 150 launch/exit cycles + 15 misuse cases: **0 errors, 0 leaked transforms/materials/meshes** |
| Bus-stop / airplane-seat criteria | **50/50 / 50/50** (play radius <= 0.32 m, angular span <= 56 deg, hand within 0.85 m of head) |
| Easy enter / exit | gesture -> preview -> one confirm: 5/5 presets, 0.26 s to play; on-board EXIT works 50/50 |
| Frame cost (desktop CPU, headless) | avg 0.10 ms, p99 0.40 ms, 40 of 4.3 M frames over 16.6 ms (first-launch spawn hitches only, max 53 ms) |
| Gesture presets | Sphere / Prolate / Oblate / Hemisphere / Domed Bowl all classified correctly |
| Welcome / menu / music smoke | built, dismissed, hub unblocked, menu active, 15 menu buttons, music clip loaded |
| Soundtrack | 4/4 CC0 tracks imported as streaming Vorbis, credits embedded in APK |
| APK | **38.6 MB, signed (v2), ARM64, minSdk 32, targetSdk 34** |
| APK permissions | HAND_TRACKING, USE_ANCHOR_API, USE_SCENE, RENDER_MODEL, passthrough, VR headtracking |

Artifacts:
- `Releases/ShapeForge50_v1.0.0.apk`
- `Releases/ShapeForge50_v1.0.0.apk.sha256`
- `Releases/shapeforge_selftest.json`
- `Releases/ui_smoke.log`

APK SHA-256:
`427D0889E5BDE00D9B560B1E66F3EE477B7CE0A75726871CB95CAE140F5B4E23`

---

## 7. Honest production notes

- The unified hub, gesture gate, real-object conversion and all 50 boards are
  real and runtime-tested. The original per-game rigs remain gate-tested pure
  simulations; some write their prop in world space (the original contract never
  required live scene wiring). `RigDrivenPropGuard` restores the app-owned prop pose each frame and
  `RigPartClamp` keeps rig-driven parts inside seated reach.
- The shared objective loop (spin/balance/catch/stir/aim/defend/collect/steady/
  pour/race) is the arcade scoring layer of the unified app. The original
  per-game acceptance tests still cover their simulation cores.
- Hand tracking, anchors, scene, and passthrough permissions are in the APK.
  The OVR scene model is deprecated by Meta in favour of MR Utility Kit but is
  the scene service pinned with Meta XR Core 207; it is used intentionally.
- The local development keystore is not a substitute for your production
  signing key. Store listing assets, privacy policy, and Horizon Store review
  are outside the APK artifact.
- The screenshots captured on this GPU-less build machine are black (the player
  has no render device). The welcome card, animations and music were smoke-tested
  headlessly (construction, input blocking, dismissal, menu activation and clip
  loading), but final visual framing and the music mix still need a GPU/Quest
  pass. Do not use black captures as marketing evidence.








