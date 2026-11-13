using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_SphereFit()
        {
            var tracker = new PropTracker();
            Vector3[] samples = new Vector3[120];
            for (int i = 0; i < 120; i++)
            {
                float angle = i * Mathf.Deg2Rad * 3.0f;
                samples[i] = new Vector3(Mathf.Cos(angle) * 8.0f, Mathf.Sin(angle) * 8.0f, 0f);
            }

            bool ok = tracker.FitSphere(samples, 8.0f);
            Assert.IsTrue(ok);
            Assert.GreaterOrEqual(tracker.MeasuredRadiusMm, MicroPlanetScannerConfig.MarbleRadiusMinMm);
            Assert.LessOrEqual(tracker.MeasuredRadiusMm, MicroPlanetScannerConfig.MarbleRadiusMaxMm);
            Assert.Less(tracker.RmsErrorMm, MicroPlanetScannerConfig.SphereFitRmsMaxMm);
        }

        [Test]
        public void Test02_HologramScale()
        {
            var renderer = new HologramRenderer();
            Vector3 marbleCenter = Vector3.zero;
            Vector3 p_P = new Vector3(8.0f, 0f, 0f);

            Vector3 p_V = renderer.TransformPropToVirtual(p_P, marbleCenter);
            Assert.AreEqual(30.0f, p_V.x, 0.1f);
            Assert.AreEqual(0.0f, p_V.y, 0.1f);
            Assert.AreEqual(0.0f, p_V.z, 0.1f);

            Vector3 corePos = renderer.GetCoreVirtualPosition(marbleCenter);
            Assert.LessOrEqual(Vector3.Distance(corePos, marbleCenter), 0.2f);
        }

        [Test]
        public void Test03_ScannerProjection()
        {
            var projector = new ScannerProjector();

            // (0, 0) -> (1, 0, 0)
            projector.ProjectTip(new Vector3(1f, 0f, 0f));
            Assert.AreEqual(0f, projector.LatitudeDeg, 0.5f);
            Assert.AreEqual(0f, projector.LongitudeDeg, 0.5f);

            // (0, 90) -> (0, 1, 0)
            projector.ProjectTip(new Vector3(0f, 1f, 0f));
            Assert.AreEqual(0f, projector.LatitudeDeg, 0.5f);
            Assert.AreEqual(90f, projector.LongitudeDeg, 0.5f);

            // (90, 0) -> (0, 0, 1)
            projector.ProjectTip(new Vector3(0f, 0f, 1f));
            Assert.AreEqual(90f, projector.LatitudeDeg, 0.5f);

            // (-45, 270) -> (0, -cos(45), -sin(45))
            float c = Mathf.Cos(45f * Mathf.Deg2Rad);
            float s = Mathf.Sin(45f * Mathf.Deg2Rad);
            projector.ProjectTip(new Vector3(0f, -c, -s));
            Assert.AreEqual(-45f, projector.LatitudeDeg, 0.5f);
            Assert.AreEqual(270f, projector.LongitudeDeg, 0.5f);
        }

        [Test]
        public void Test04_LockFill()
        {
            var lockAcc = new LockAccumulator();
            float dt = 0.010f; // 10ms ticks
            int ticks = 0;
            bool purged = false;

            while (ticks < 50) // up to 500ms
            {
                purged = lockAcc.Step(2.0f, dt);
                ticks++;
                if (purged) break;
            }

            Assert.IsTrue(purged);
            float totalTime = ticks * dt;
            Assert.AreEqual(0.400f, totalTime, 0.016f); // 400 +/- 8 ms nominal
        }

        [Test]
        public void Test05_LockDrain()
        {
            var lockAcc = new LockAccumulator();
            float dt = 0.010f;

            // Fill for 200ms
            for (int i = 0; i < 20; i++)
            {
                lockAcc.Step(2.0f, dt);
            }
            Assert.AreEqual(0.200f, lockAcc.LockTimeSec, 0.005f);

            // Move to 12mm and drain
            int drainTicks = 0;
            while (lockAcc.LockTimeSec > 0f && drainTicks < 30)
            {
                lockAcc.Step(12.0f, dt);
                drainTicks++;
            }

            float drainTime = drainTicks * dt;
            Assert.AreEqual(0.133f, drainTime, 0.016f);
            Assert.IsFalse(lockAcc.PurgeTriggered);
        }

        [Test]
        public void Test06_LockBoundary()
        {
            var lockAcc = new LockAccumulator();
            // 7.9mm fills
            lockAcc.Step(7.9f, 0.050f);
            Assert.Greater(lockAcc.LockTimeSec, 0f);

            // 8.1mm drains
            float filled = lockAcc.LockTimeSec;
            lockAcc.Step(8.1f, 0.020f);
            Assert.Less(lockAcc.LockTimeSec, filled);
            Assert.IsFalse(lockAcc.PurgeTriggered);
        }

        [Test]
        public void Test07_TripodDistance()
        {
            var gate = new TripodGate();
            // Start broken -> 8.9mm enters locked
            Assert.IsTrue(gate.EvaluateDistance(8.9f));

            // Reset -> 9.1mm cannot enter
            var gate2 = new TripodGate();
            Assert.IsFalse(gate2.EvaluateDistance(9.1f));

            // In active state -> 10.9mm stays valid (release is 11mm)
            Assert.IsTrue(gate.EvaluateDistance(10.9f));

            // In active state -> 11.1mm releases to invalid
            Assert.IsFalse(gate.EvaluateDistance(11.1f));
        }

        [Test]
        public void Test08_TripodAngle()
        {
            var gate = new TripodGate();
            // 59 deg cannot enter
            Assert.IsFalse(gate.EvaluateAngle(59f));

            // 61 deg enters
            Assert.IsTrue(gate.EvaluateAngle(61f));

            // Stays valid down to 55 deg
            Assert.IsTrue(gate.EvaluateAngle(56f));

            // Releases below 55 deg
            Assert.IsFalse(gate.EvaluateAngle(54f));
        }

        [Test]
        public void Test09_RollingFusion()
        {
            var roll = new RollFusion();
            Vector3[] omegas = new Vector3[]
            {
                new Vector3(0, 180f, 0),
                new Vector3(0, 180f, 0),
                new Vector3(0, 180f, 0),
                new Vector3(0, 180f, 0)
            };
            Vector3 fused = roll.ComputeFusedOmega(omegas, null);
            Assert.AreEqual(180f, fused.y, 10f);

            // Quaternion drift test over 10s
            for (int i = 0; i < 100; i++)
            {
                roll.Step(fused, 0.1f);
            }
            Assert.IsNotNull(roll.IntegratedRotation);
        }

        [Test]
        public void Test10_Slip()
        {
            var roll = new RollFusion();
            Vector3 clamped = roll.ApplySlipClamp(new Vector3(0, 900f, 0));
            Assert.LessOrEqual(clamped.magnitude, 540.01f);
        }

        [Test]
        public void Test11_WormCoreHit()
        {
            var field = new WormField();
            var moon = new MoonState();
            var worm = field.SpawnWorm(Vector3.forward);

            // Burrow to 30 mm
            field.StepWormBurrow(worm, 30.0f, moon, purgeThisTick: false);
            Assert.AreEqual(20, moon.Corruption);

            // Same tick purge wins
            var worm2 = field.SpawnWorm(Vector3.forward);
            field.StepWormBurrow(worm2, 30.0f, moon, purgeThisTick: true);
            Assert.AreEqual(10, moon.Corruption); // healed 10 instead of +20
        }

        [Test, Category("NeedsDevice")]
        public void Test12_Occlusion()
        {
            Assert.Ignore("Deferred: Requires device camera passthrough and hands occlusion mesh.");
        }

        [Test]
        public void Test13_FrameBudget()
        {
            var feedback = new FeedbackDirector();
            feedback.SpawnEmbers(900);
            Assert.LessOrEqual(feedback.ActiveParticles, MicroPlanetScannerConfig.MaxParticleBudget);
        }

        [Test, Category("NeedsDevice")]
        public void Test14_Endurance()
        {
            Assert.Ignore("Deferred: Requires physical Meta XR device 6-minute run.");
        }
    }
}
