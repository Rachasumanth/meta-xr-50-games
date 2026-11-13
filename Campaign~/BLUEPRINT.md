# BLUEPRINT - per-game contract (workers follow EXACTLY)

Spec source of truth: Specs~/<chapter>/<file>.md (sections 0..16).

## Folder
Assets/Games/<chapter>/<slug>/
  Scripts/<Class>.cs            one file per class in spec section 15 table
  Scripts/Tiny extras allowed   helpers the spec's pseudocode implies
  Scripts/Editor/AcceptanceTests.cs   NUnit EditMode tests from section 16
  BUILD_LOG.md                  build report (honest)

## Names
- namespace XRGames.<Chapter>.<Slug>  (PascalCase parts; Chapter stays as the
  folder name with dots: 4_1a_true_spheres_pinch_scale -> XRGames.C4_1A.Slug)
- Gameplay core classes: PURE C# (may use UnityEngine.Mathv/Mathf/Vector3/
  Quaternion as math types). NO runtime XR/OVR/hand-tracking dependencies, so
  EditMode tests run in headless -nographics batches.
- All numeric constants from the spec go into one <Slug>Config class (public
  static fields, exact spec values + units in comments).
- XR/runtime wiring: ONE thin MonoBehaviour "<Slug>Rig" per game that wires core
  classes to Meta XR SDK at runtime. Rig is thin; logic lives in core classes.

## Tests (from spec section 16)
- Implement every mechanically-checkable acceptance item as an NUnit [Test]
  method named Test<NN>_<ShortName> (NN = item number, e.g. Test04_RollingLaw).
  Synthetic inputs only; no device. Example: inject contacts, assert gains,
  omega vectors, FSM transitions, scoring deltas, clamps.
- Items needing a device/rendering (occlusion, frame budget, endurance):
  implement the real systems faithfully; STILL write the test but tag
  [Category("NeedsDevice")] and make the body assert the testable invariant
  (e.g. constants, budget math) OR document in BUILD_LOG why it is deferred.
  Never produce fake passing render tests.
- Test file must include at least the count line header as comments listing
  which acceptance items are automated vs NeedsDevice.

## BUILD_LOG.md template
# <Slug> build report
- status: implemented, awaiting gate
- spec: Specs~/<chapter>/<file>
- classes: <list from section 15> (one line each: file + responsibility)
- constants source: <Slug>Config fields n=
- tests automated: NN items (list), NeedsDevice: NN items (list)
- honest limitations / assumptions: ...

## Hard rules (learned from the first gate cycles - violating these FAILS the gate)
- SINGLE DEFINITION: every class/struct/enum named in the spec's section-15 table is
  defined EXACTLY ONCE across the whole game folder. Never put copies of classes in
  both the Core bundle file and standalone files. CS0101 = automatic gate_fail.
- The `<Slug>Config` class: constants only, one file.
- RAW LAW UNCLAMPED: math functions that implement a law (e.g. omega = (n x v_t)/r)
  return the raw exact result. Caps/clamps are separate NAMED functions/stages
  (e.g. ClampOmega enforcing |omega| and |alpha|), applied downstream on purpose.
  If a spec acceptance item expects the exact law value, a pre-clamp makes it fail.
- ROTATION: integrate angular velocity with Quaternion.AngleAxis(|omega|deg, axis)
  composition - NEVER via Quaternion.Euler(components). Euler components do not
  compose an angular velocity vector.
- STATEFUL HYSTERESIS: any dead zone with a release threshold (spec section 6/16)
  needs a stateful controller: enter >= DZ_ON, stay active down to DZ_OFF, stop below.
- TESTS ARE THE CONTRACT: every acceptance item number in section 16 gets its own
  [Test] with asserts matching the spec's exact numbers. No Assert.IsTrue(true)
  except [Category("NeedsDevice")] items with the deferral documented in BUILD_LOG.
- Keep the connection to spec constants literals-free: reference <Slug>Config fields.

## Manager notes
- Queue only games whose Scripts + Tests + BUILD_LOG exist.
- On gate_fail: read the failure lines from gate-results.jsonl (counted by run
  id), spawn ONE fix sub-agent per failing game, re-queue.
