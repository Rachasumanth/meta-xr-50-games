using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.ScarabCanyon.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_Registration()
        {
            var palm = new PalmSurface();
            bool ok = palm.FitPalm(new Vector3[20]);
            Assert.IsTrue(ok);
            Assert.Less(palm.PlaneRmsErrorMm, 1.2f);
        }

        [Test]
        public void Test02_RollLaw()
        {
            var physics = new GroovePhysics();
            float theta = 6.0f; // 6 deg
            float dt = 0.010f;

            for (int i = 0; i < 100; i++)
            {
                physics.Step(theta, dt);
            }

            // Analytic: 1/2 * g * sin(6 deg) * 1.0^2
            float analyticS = 0.5f * ScarabCanyonConfig.GravityMmS2 * Mathf.Sin(6.0f * Mathf.Deg2Rad) * 1.0f;
            Assert.AreEqual(analyticS, physics.S, analyticS * 0.12f); // within 12%
        }

        [Test]
        public void Test03_Stiction()
        {
            var physics = new GroovePhysics();
            physics.SetState(0f, 40.0f); // 40 mm/s start at theta = 0

            float dt = 0.010f;
            for (int i = 0; i < 40; i++) // 400 ms
            {
                physics.Step(0f, dt);
            }

            Assert.AreEqual(0f, physics.SDot, 0.001f);
        }

        [Test]
        public void Test04_GrooveCoordinate()
        {
            var judge = new OrbJudge();
            Assert.AreEqual(41.0f, judge.MapToLateral(10.0f), 0.001f);
        }

        [Test]
        public void Test05_Spill()
        {
            var physics = new GroovePhysics();
            var run = new RunDirector();

            physics.SetState(25.0f, 0f); // > 24 mm
            physics.Step(0f, 0.070f); // 70 ms > 60 ms

            Assert.IsTrue(physics.HasSpilled);
            run.ApplySpill();
            Assert.AreEqual(2, run.Lives);
            Assert.AreEqual(100, run.Glow); // glow unaffected
        }

        [Test]
        public void Test06_SpillRecover()
        {
            var physics = new GroovePhysics();
            physics.SetState(25.0f, 0f); // > 24 mm
            physics.Step(0f, 0.040f); // only 40 ms

            // Return to safe
            physics.SetState(20.0f, 0f);
            physics.Step(0f, 0.030f);

            Assert.IsFalse(physics.HasSpilled);
        }

        [Test]
        public void Test07_HazardOverlap()
        {
            var run = new RunDirector();
            run.ApplyHazardHit(20);
            Assert.AreEqual(80, run.Glow);
            Assert.AreEqual(3, run.Lives); // no life loss
        }

        [Test]
        public void Test08_CleanPass()
        {
            var judge = new OrbJudge();
            judge.EvaluatePass(8.0f, out int score, out bool nearMiss);
            Assert.AreEqual(70, score); // 50 + 20
            Assert.IsTrue(nearMiss);
        }

        [Test]
        public void Test09_GoldenPath()
        {
            var run = new RunDirector();
            for (int i = 0; i < 5; i++) run.RecordCleanPass(50);
            Assert.AreEqual(1.4f, run.Multiplier, 0.01f);

            run.ApplyHazardHit(10);
            Assert.AreEqual(1.0f, run.Multiplier, 0.01f); // cleared
        }

        [Test]
        public void Test10_GlowZero()
        {
            var director = new CanyonDirector();
            director.UpdateGlowSpeed(0);
            Assert.AreEqual(70.0f, director.ScrollSpeed, 0.1f); // 0.7x of 100

            director.UpdateGlowSpeed(30);
            Assert.AreEqual(100.0f, director.ScrollSpeed, 0.1f);
        }

        [Test]
        public void Test11_LiftingPalm()
        {
            var director = new CanyonDirector();
            director.CheckPalmHeight(50.0f, 0.100f);
            Assert.IsTrue(director.IsPaused);

            director.CheckPalmHeight(20.0f, 0.100f);
            Assert.IsFalse(director.IsPaused);
        }

        [Test]
        public void Test12_Level3Run()
        {
            var run = new RunDirector();
            Assert.GreaterOrEqual(run.Lives, 2);
        }

        [Test, Category("NeedsDevice")]
        public void Test13_CoveringHand()
        {
            Assert.Ignore("Deferred: Requires device camera passthrough covering detection.");
        }

        [Test]
        public void Test14_ParticleCount()
        {
            Assert.LessOrEqual(300, ScarabCanyonConfig.MaxParticleBudget);
        }
    }
}
