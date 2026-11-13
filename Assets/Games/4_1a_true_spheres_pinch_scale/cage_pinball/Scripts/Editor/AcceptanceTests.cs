using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.CagePinball.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_BallFit()
        {
            var tracker = new PropTracker();
            Vector3[] samples = new Vector3[120];
            for (int i = 0; i < 120; i++)
            {
                float angle = i * Mathf.Deg2Rad * 3f;
                samples[i] = new Vector3(Mathf.Cos(angle) * 20.0f, Mathf.Sin(angle) * 20.0f, 0f);
            }

            bool ok = tracker.FitSphere(samples, 20.0f);
            Assert.IsTrue(ok);
            Assert.GreaterOrEqual(tracker.MeasuredRadiusMm, CagePinballConfig.BallRadiusMinMm);
            Assert.LessOrEqual(tracker.MeasuredRadiusMm, CagePinballConfig.BallRadiusMaxMm);
            Assert.Less(tracker.RmsErrorMm, CagePinballConfig.SphereFitRmsMaxMm);
        }

        [Test]
        public void Test02_RollingLaw()
        {
            var physics = new BallPhysics();
            Vector3 v = new Vector3(200f, 0f, 0f); // mm/s
            Vector3 omega = physics.ComputeRollingOmega(v, 20f);

            Assert.AreEqual(0f, omega.x, 0.01f);
            Assert.AreEqual(0f, omega.y, 0.01f);
            Assert.AreEqual(-10.0f, omega.z, 0.2f); // within 2%

            physics.Step(v, 1.0f);
            Assert.AreEqual(572.95f, physics.SeamRotationDeg, 5.0f);
        }

        [Test]
        public void Test03_CageRadius()
        {
            var fitter = new CageFitter();
            Assert.AreEqual(60.0f, fitter.FitRadius(59.0f), 0.5f);
            Assert.AreEqual(70.0f, fitter.FitRadius(70.0f), 0.5f);
            Assert.AreEqual(90.0f, fitter.FitRadius(90.0f), 0.5f);
            Assert.AreEqual(110.0f, fitter.FitRadius(111.0f), 0.5f);
        }

        [Test]
        public void Test04_BumperTrigger()
        {
            var bumper = new BumperSystem();

            // 39 mm/s -> below 40 threshold
            bool hit1 = bumper.TryHitBumper(0, 39.0f, 0.0f, out _);
            Assert.IsFalse(hit1);

            // 41 mm/s -> hit
            bool hit2 = bumper.TryHitBumper(0, 41.0f, 0.050f, out _);
            Assert.IsTrue(hit2);

            // 40 mm/s at 100 ms after previous hit (< 120 ms cooldown)
            bool hit3 = bumper.TryHitBumper(0, 40.0f, 0.150f, out _);
            Assert.IsFalse(hit3);
        }

        [Test]
        public void Test05_Combo()
        {
            var bumper = new BumperSystem();

            // Alternate 5 different bumpers at 700 ms spacing
            for (int i = 0; i < 5; i++)
            {
                bool hit = bumper.TryHitBumper(i, 50.0f, i * 0.700f, out _);
                Assert.IsTrue(hit);
            }
            Assert.AreEqual(5, bumper.ComboChain);
            Assert.AreEqual(2.0f, bumper.Multiplier, 0.01f);

            // 900 ms gap resets
            bumper.TryHitBumper(1, 50.0f, 4 * 0.700f + 0.900f, out _);
            Assert.AreEqual(1, bumper.ComboChain);
        }

        [Test]
        public void Test06_SameFingerSpam()
        {
            var bumper = new BumperSystem();
            // Hit the same bumper 5 times at 200 ms spacing
            for (int i = 0; i < 5; i++)
            {
                bool hit = bumper.TryHitBumper(0, 50.0f, i * 0.200f, out _);
                Assert.IsTrue(hit);
            }
            Assert.AreEqual(1, bumper.ComboChain);
        }

        [Test]
        public void Test07_PinballAim()
        {
            var pinball = new PinballSystem();
            Assert.IsTrue(pinball.CheckCrystalHit(0.0f));
            Assert.IsTrue(pinball.CheckCrystalHit(9.0f));
            Assert.IsFalse(pinball.CheckCrystalHit(10.0f));
        }

        [Test]
        public void Test08_Escape()
        {
            var fitter = new CageFitter();
            fitter.SetCage(Vector2.zero, 80.0f);

            // R + 14 mm for 120 ms -> no escape (threshold is R + 15)
            Vector2 p14 = new Vector2(94.0f, 0f);
            fitter.EvaluateEscape(p14, 0.120f);
            Assert.AreEqual(EscapeState.Safe, fitter.EvaluateEscape(p14, 0.016f));

            // R + 16 mm for 120 ms -> escape
            Vector2 p16 = new Vector2(96.0f, 0f);
            fitter.EvaluateEscape(p16, 0.060f);
            EscapeState st = fitter.EvaluateEscape(p16, 0.060f);
            Assert.AreEqual(EscapeState.Escape, st);

            // Re-entry at R + 5 clears timer
            Vector2 pSafe = new Vector2(84.0f, 0f);
            fitter.EvaluateEscape(pSafe, 0.016f);
            Assert.AreEqual(0f, fitter.EscapeTimerSec);
        }

        [Test]
        public void Test09_EscapeGrace()
        {
            var fitter = new CageFitter();
            fitter.SetCage(Vector2.zero, 80.0f);
            var score = new ScoreService();

            // Trigger escape
            score.LoseLife();
            fitter.StartGrace(1.5f);
            Assert.AreEqual(2, score.Lives);

            // Drive to R + 20 mm during grace -> no life lost
            Vector2 p20 = new Vector2(100.0f, 0f);
            EscapeState st = fitter.EvaluateEscape(p20, 0.200f);
            Assert.AreEqual(EscapeState.Safe, st);
            Assert.AreEqual(2, score.Lives);

            // Fast forward past 1.5s grace
            fitter.EvaluateEscape(p20, 1.4f);
            Assert.AreEqual(0f, fitter.GraceTimerSec);
        }

        [Test, Category("NeedsDevice")]
        public void Test10_Occlusion()
        {
            Assert.Ignore("Deferred: Requires device camera passthrough hand occlusion.");
        }

        [Test]
        public void Test11_TrackingLoss()
        {
            var tracker = new PropTracker();
            tracker.SetPose(Vector3.zero, Vector3.zero);
            Vector3 omega = new Vector3(0, 0, -10f);

            // Ghost path follows omega for 1.2s
            tracker.StepGhost(omega, 1.2f);
            Assert.Greater(tracker.GhostPosition.magnitude, 0.1f);
        }

        [Test]
        public void Test12_FrameBudget()
        {
            var feedback = new FeedbackDirector();
            feedback.SpawnParticles(900);
            Assert.LessOrEqual(feedback.ActiveParticles, CagePinballConfig.MaxParticleBudget);
        }

        [Test]
        public void Test13_TrayTilt()
        {
            var tray = new TrayFrame();
            tray.ReSolvePlane(new Vector3(0, 0.9975f, 0.0697f), Vector3.zero); // ~4 deg tilt
            Assert.IsTrue(tray.IsReSeating);

            tray.Step(0.550f);
            Assert.IsFalse(tray.IsReSeating);
        }

        [Test, Category("NeedsDevice")]
        public void Test14_Endurance()
        {
            Assert.Ignore("Deferred: Requires 7 min device session.");
        }
    }
}
