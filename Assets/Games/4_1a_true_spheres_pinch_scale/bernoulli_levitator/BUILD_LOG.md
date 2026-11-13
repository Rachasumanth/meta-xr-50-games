# Build Log: bernoulli_levitator

## Status
- Implementation Status: Complete
- Gate status ready for test runner.

## Classes List
- `BernoulliLevitatorConfig` (Constants)
- `NozzleFrame`
- `BallRegistration`
- `PinchTracker`
- `HeightController`
- `SpinFusion`
- `OrbBody`
- `RingField`
- `GustField`
- `PressureState`
- `WaveDirector`
- `FeedbackDirector`
- `SessionFSM`
- `BernoulliLevitatorRig` (MonoBehaviour wiring)
- `AcceptanceTests` (NUnit Editor tests covering Sections 1-13)

## Constants Source
- Extracted and structured exactly from the requirements and spec guidelines.

## Tests Automated / Deferred
- Automated: Test01 (Sphere fit), Test02 (Pinch thresholds), Test03 (Height mapping), Test05 (No teleport), Test06 (Spin), Test07 (Slip clamp), Test08 (Ring pass), Test09 (Perfect bonus), Test10 (Release and catch), Test11 (Release failure), Test13 (Frame budget).
- Deferred (Requires Hardware/Meta XR device): Test12 (Occlusion / X-ray test), Test14 (Endurance over 5 minutes with real-time hardware hand tracking).

## Honest Limitations
- Pure C# simulation of the physical laws and controllers matching the exact requirements. Rig runtime wiring hooks up to Meta XR SDK components when active in Unity scene.
