# Build Log - micro_wok_toss

## Chapter 4.3a: Hemispheres (Pinch Scale) - Game 02

### 1. Specification & Asset Provenance
- **Prop**: Measuring spoon bowl with 50 mm stem, bowl-up (`R = 18.0 mm`, `t = 0.8 mm`, `m = 6.0 g`).
- **Asset**: `720-wok-pan.glb` copied to `Assets/Games/4_3a_hemispheres_pinch_scale/micro_wok_toss/Models/micro_wok.glb`.
- **Collider**: Concave inner sphere, convex outer sphere, rim torus, stem capsule.

### 2. Architecture & Pure C# Domain Decoupling
- `MicroWokConfig.cs`: Physical constants (radii, rolling accel, detachment threshold, thermal power and dissipation).
- `WokModel.cs`:
  - `PropSpoon`: Geometry fitter for bowl and stem alignment.
  - `Ingredient`: Rolling acceleration dynamics `(5/7)*g*sin(theta)`, detachment classifier, thermal cook accumulator.
  - `ThermalModel`: Preheat and cooking thermal differential equations with hard clamp at 240 °C.
  - `SwirlClassifier`: Frequency and radius window filter.
  - `PlateGate`: Roll angle (>= 28°) and distance (<= 60 mm) plating gate.
- `WokRig.cs`: Thin MonoBehaviour connecting thermal model, ingredients, and visual elements.
- `AcceptanceTests.cs`: 14 automated NUnit tests strictly adhering to Section 16 checklists.

### 3. Acceptance Test Mapping
- AT01: Geometry Fit (R, Rin, stem axis, rim plane).
- AT02: Detachment (4.0 m/s² upward accel, airtime, speed).
- AT03: No-Detach Band (1.5 m/s² upward accel).
- AT04: Rolling Law ((5/7)*g*sin(20°) = 2395 mm/s²).
- AT05: Normal Force (9810 mm/s² at rest).
- AT06: Overflow detection.
- AT07: Thermal Ramp (150 °C at 5.8s, 180 °C at 7.4s, clamp 240 °C).
- AT08: Cook Rate (56.6 points at 180 °C for 3s).
- AT09: Burn detection at 100 points.
- AT10: Swirl Classifier (12 mm at 1.3 Hz valid, 5 mm at 4 Hz rejected).
- AT11: Plate Gate (29° at 55 mm passes, 27° or 65 mm rejected).
- AT12: Ingredient Count (14 ingredients).
- AT13: Occlusion logic test.
- AT14: Session Endurance test.
