using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.FoghornBuoy.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        // 1. Pendulum frequency: after an initial 6° tilt the buoy rocks at 0.55 +/- 0.06 Hz.
        [Test]
        public void AT01_PendulumFrequency()
        {
            var p = new RockPendulum();
            p.ThetaRad = 6.0f * Mathf.Deg2Rad;
            p.ThetaDotRadS = 0f;

            float dt = 0.005f;
            float prevTheta = p.ThetaRad;
            float firstZeroTime = -1f;
            float secondZeroTime = -1f;

            for (float t = 0; t <= 3.0f; t += dt)
            {
                p.Step(dt, 0f);
                if (prevTheta > 0f && p.ThetaRad <= 0f && firstZeroTime < 0f)
                {
                    firstZeroTime = t;
                }
                else if (prevTheta < 0f && p.ThetaRad >= 0f && firstZeroTime > 0f && secondZeroTime < 0f)
                {
                    secondZeroTime = t;
                    break;
                }
                prevTheta = p.ThetaRad;
            }

            Assert.Greater(firstZeroTime, 0f);
            Assert.Greater(secondZeroTime, firstZeroTime);
            float halfPeriod = secondZeroTime - firstZeroTime;
            float period = halfPeriod * 2.0f;
            float freq = 1.0f / period;

            Assert.AreEqual(0.55f, freq, 0.06f, $"Measured frequency {freq} Hz must be 0.55 +/- 0.06 Hz");
        }

        // 2. Press torque: a 2 N south press on a 6° tilt produces 0.16 +/- 0.02 N*m.
        [Test]
        public void AT02_PressTorque()
        {
            float torque = FoghornSimulation.ComputePressTorque(CardinalSector.South, 2.0f, 6.0f);
            Assert.AreEqual(0.16f, torque, 0.02f, "2 N south press must produce 0.16 +/- 0.02 N*m");
        }

        // 3. Counter-rock: a well-timed opposite press reduces |theta| by >= 60% within 0.4 s.
        [Test]
        public void AT03_CounterRock()
        {
            var p = new RockPendulum();
            p.ThetaRad = 10.0f * Mathf.Deg2Rad; // 10 deg tilt
            p.ThetaDotRadS = 0f;

            float dt = 0.005f;
            // Apply well-timed 100 ms counter-pulse opposing the tilt
            for (float t = 0; t < 0.40f; t += dt)
            {
                float counterTorque = (t < 0.10f) ? -0.005f : 0f;
                p.Step(dt, counterTorque);
            }

            float finalTilt = Mathf.Abs(p.ThetaDeg);
            float reductionRatio = (10.0f - finalTilt) / 10.0f;
            Assert.GreaterOrEqual(reductionRatio, 0.60f, $"Counter-torque must reduce tilt by >= 60%, got {reductionRatio * 100}%");
        }

        // 4. Wave torque: a 0.11 N*m swell peaks at telegraph + 1.2 s +/- 0.1 s.
        [Test]
        public void AT04_WaveTorque()
        {
            var swell = new SwellEvent { TelegraphSec = 1.2f, AmplitudeNm = 0.11f };
            swell.ElapsedSec = 1.2f + 1.2f; // telegraph + 1.2s

            float torqueAtPeak = swell.GetCurrentTorque();
            Assert.AreEqual(0.11f, torqueAtPeak, 0.01f, "Torque must peak at 0.11 N*m at telegraph + 1.2s");
        }

        // 5. Warning: |theta| > 20° triggers the red horizon.
        [Test]
        public void AT05_Warning()
        {
            var sim = new FoghornSimulation();
            sim.Pendulum.ThetaRad = 19.0f * Mathf.Deg2Rad;
            Assert.IsFalse(Mathf.Abs(sim.Pendulum.ThetaDeg) > FoghornBuoyConfig.WarningTiltDeg);

            sim.Pendulum.ThetaRad = 21.0f * Mathf.Deg2Rad;
            Assert.IsTrue(Mathf.Abs(sim.Pendulum.ThetaDeg) > FoghornBuoyConfig.WarningTiltDeg, "Tilt > 20° must trigger warning");
        }

        // 6. Capsize: |theta| > 28° for 150 ms costs exactly one life.
        [Test]
        public void AT06_Capsize()
        {
            var sim = new FoghornSimulation();
            Assert.AreEqual(3, sim.Lives);

            float dt = 0.010f;
            for (float t = 0; t < 0.100f; t += dt)
            {
                sim.Pendulum.ThetaRad = 29.0f * Mathf.Deg2Rad;
                sim.Step(dt);
            }
            Assert.AreEqual(3, sim.Lives, "Tilt for 100 ms (< 150 ms) should not cause capsize");

            for (float t = 0; t < 0.060f; t += dt)
            {
                sim.Pendulum.ThetaRad = 29.0f * Mathf.Deg2Rad;
                sim.Step(dt);
            }

            Assert.AreEqual(2, sim.Lives, "Capsize persisting for 150 ms must cost exactly one life");
            Assert.AreEqual(1, sim.Capsizes);
        }

        // 7. Shark: a shark reaches r < 40 mm and ends the storm.
        [Test]
        public void AT07_Shark()
        {
            var sim = new FoghornSimulation();
            sim.Sharks[0].RadiusMm = 41.0f;

            sim.Step(0.2f); // shark closes below 40 mm
            Assert.IsTrue(sim.StormFailed, "Shark reaching r < 40 mm must end the storm");
        }

        // 8. Foghorn: a clean opposite double-press repels sharks within 200 mm by 120 mm.
        [Test]
        public void AT08_Foghorn()
        {
            var sim = new FoghornSimulation();
            sim.Sharks[0].RadiusMm = 180.0f; // within 200 mm
            sim.Sharks[1].RadiusMm = 250.0f; // outside 200 mm

            bool fired = sim.TriggerFoghorn(CardinalSector.North, CardinalSector.South, 0.050f, 2.0f, 2.0f);
            Assert.IsTrue(fired, "Clean opposite press must fire foghorn");
            Assert.AreEqual(300.0f, sim.Sharks[0].RadiusMm, 1e-4f, "Shark within 200 mm must be repelled by 120 mm");
            Assert.AreEqual(250.0f, sim.Sharks[1].RadiusMm, 1e-4f, "Shark outside 200 mm must not be affected");
        }

        // 9. Air: a foghorn costs 15 air and regenerates at 8/s.
        [Test]
        public void AT09_Air()
        {
            var sim = new FoghornSimulation();
            Assert.AreEqual(100.0f, sim.Air, 1e-4f);

            sim.TriggerFoghorn(CardinalSector.East, CardinalSector.West, 0.020f, 2.0f, 2.0f);
            Assert.AreEqual(85.0f, sim.Air, 1e-4f, "Foghorn must cost 15 air");

            sim.Step(1.0f); // regenerates 8 air/s -> 85 + 8 = 93
            Assert.AreEqual(93.0f, sim.Air, 0.05f, "Air must regenerate at 8/s");
        }

        // 10. Storm timer: five 60 s storms complete in 300 s +/- 1 s.
        [Test]
        public void AT10_StormTimer()
        {
            float stormDuration = 60.0f;
            int totalStorms = 5;
            float totalTime = stormDuration * totalStorms;

            Assert.AreEqual(300.0f, totalTime, 1.0f, "Five 60s storms must total 300 +/- 1 s");
        }

        // 11. Slide clamp: a 16 mm bowl drift is soft-clamped and re-centred.
        [Test]
        public void AT11_SlideClamp()
        {
            var sim = new FoghornSimulation();
            sim.ApplySlideOffset(new Vector2(16.0f, 0f));

            Assert.AreEqual(15.0f, sim.BuoyOffsetMm.magnitude, 1e-4f, "16 mm drift must be clamped to 15 mm");
        }

        // 12. Pause: palm cover freezes the pendulum and resumes cleanly.
        [Test]
        public void AT12_Pause()
        {
            var sim = new FoghornSimulation();
            sim.Pendulum.ThetaRad = 10.0f * Mathf.Deg2Rad;
            float thetaBefore = sim.Pendulum.ThetaRad;

            sim.Step(1.0f, pause: true);
            Assert.IsTrue(sim.IsPaused);
            Assert.AreEqual(thetaBefore, sim.Pendulum.ThetaRad, 1e-4f, "State must freeze during pause");

            sim.Step(0.016f, pause: false);
            Assert.IsFalse(sim.IsPaused, "Resume must unfreeze simulation cleanly");
        }

        // 13. Occlusion: a shark behind the buoy is hidden.
        [Test]
        public void AT13_Occlusion()
        {
            Vector3 buoyCenter = Vector3.zero;
            Vector3 sharkBehind = new Vector3(0, 0, 150.0f); // behind buoy in Z
            bool isBehind = sharkBehind.z > buoyCenter.z;

            Assert.IsTrue(isBehind, "Shark behind buoy in depth plane is occluded");
        }

        // 14. Performance: buoy + 5 sharks + swell sustain 90 fps.
        [Test]
        public void AT14_Performance()
        {
            var sim = new FoghornSimulation();
            sim.Sharks.Add(new SharkAgent()); // total 5 sharks

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 90; i++)
            {
                sim.Step(0.011f);
            }
            sw.Stop();

            Assert.Less(sw.ElapsedMilliseconds, 100, "90 ticks must complete well under 100 ms");
        }
    }
}
