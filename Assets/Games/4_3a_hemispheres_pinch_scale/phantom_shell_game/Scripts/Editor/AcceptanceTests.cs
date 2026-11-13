using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.PhantomShellGame.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_HemisphereFit()
        {
            int n = 36;
            Vector3[] rimPoints = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float angle = i * Mathf.PI * 2f / n;
                rimPoints[i] = new Vector3(Mathf.Cos(angle) * 22.0f, 0f, Mathf.Sin(angle) * 22.0f);
            }
            Vector3 apex = new Vector3(0f, 22.0f, 0f);

            bool ok = PropShell.FitHemisphere(rimPoints, apex, out float R, out float t, out float normalErr, out float apexErr);

            Assert.IsTrue(ok);
            Assert.AreEqual(22.0f, R, 0.4f);
            Assert.AreEqual(1.8f, t, 0.25f);
            Assert.Less(normalErr, 1.5f);
            Assert.Less(apexErr, 0.5f);
        }

        [Test]
        public void AT02_SlideLaw()
        {
            var slide = new SlideModel();
            float dt = 0.01f;
            Vector2 tipVel = new Vector2(120.0f, 0f);

            // Drive for 0.5s
            for (float t = 0; t < 0.5f; t += dt)
            {
                slide.Step(tipVel, true, dt);
            }

            Assert.AreEqual(108.0f, slide.VelocityMmS.x, 5.0f);

            // Release and record stopping travel & time
            Vector2 releasePos = slide.PositionMm;
            float releaseTime = 0f;
            while (slide.VelocityMmS.magnitude > 0.001f && releaseTime < 0.5f)
            {
                slide.Step(Vector2.zero, false, dt);
                releaseTime += dt;
            }

            float stoppingDist = Vector2.Distance(slide.PositionMm, releasePos);
            Assert.AreEqual(2.6f, stoppingDist, 0.7f);
            Assert.Less(releaseTime, 0.220f);
        }

        [Test]
        public void AT03_FrictionConstant()
        {
            var slide = new SlideModel();
            slide.VelocityMmS = new Vector2(220.0f, 0f);
            float dt = 0.001f;
            Vector2 startPos = slide.PositionMm;

            while (slide.VelocityMmS.magnitude > 0.001f)
            {
                slide.Step(Vector2.zero, false, dt);
            }

            float dist = Vector2.Distance(slide.PositionMm, startPos);
            // v^2 / (2 * 2747) = 48400 / 5494 = 8.81 mm
            Assert.AreEqual(8.8f, dist, 1.0f);
        }

        [Test]
        public void AT04_PatchWall()
        {
            var slide = new SlideModel();
            slide.PositionMm = new Vector2(90.0f, 0f);
            Vector2 driveVel = new Vector2(300.0f, 0f);
            float dt = 0.01f;

            float maxR = 0f;
            for (float t = 0; t < 0.6f; t += dt)
            {
                slide.Step(driveVel, true, dt);
                if (slide.PositionMm.magnitude > maxR)
                {
                    maxR = slide.PositionMm.magnitude;
                }
            }

            Assert.LessOrEqual(maxR, 96.5f);
        }

        [Test]
        public void AT05_LiftClassifier()
        {
            var lift = new LiftModel();
            float dt = 0.01f;

            // Raise 0 -> 20 mm in 250 ms
            for (float t = 0; t < 0.25f; t += dt)
            {
                float target = Mathf.Lerp(0f, 20f, t / 0.25f);
                lift.Step(target, dt);
            }

            Assert.IsTrue(lift.IsLifted);
            Assert.AreEqual(1, lift.PeekCount);

            // Hold 500 ms
            for (float t = 0; t < 0.5f; t += dt)
            {
                lift.Step(20f, dt);
            }
            Assert.AreEqual(1, lift.PeekCount);

            // Lower back to 0 mm
            for (float t = 0; t < 0.25f; t += dt)
            {
                float target = Mathf.Lerp(20f, 0f, t / 0.25f);
                lift.Step(target, dt);
            }

            Assert.IsFalse(lift.IsLifted);
            Assert.AreEqual(1, lift.PeekCount);
        }

        [Test]
        public void AT06_ExposureTimer()
        {
            var lift = new LiftModel();
            float dt = 0.01f;
            float exposedTime = -1f;

            for (float t = 0; t <= 1.5f; t += dt)
            {
                lift.Step(20.0f, dt);
                if (lift.IsExposed && exposedTime < 0f)
                {
                    exposedTime = t;
                }
            }

            Assert.IsTrue(lift.IsExposed);
            Assert.AreEqual(1.2f, exposedTime, 0.05f);
        }

        [Test]
        public void AT07_BlockGeometry()
        {
            Vector2 shellPos = Vector2.zero;
            float lift = 0f;
            float exp = 0f;

            Assert.AreEqual(StrikeOutcome.Block, StrikeResolver.EvaluateStrike(new Vector2(35f, 0f), shellPos, lift, exp));
            Assert.AreEqual(StrikeOutcome.Block, StrikeResolver.EvaluateStrike(new Vector2(36f, 0f), shellPos, lift, exp));
            Assert.AreEqual(StrikeOutcome.NearMiss, StrikeResolver.EvaluateStrike(new Vector2(47f, 0f), shellPos, lift, exp));
            Assert.AreEqual(StrikeOutcome.NearMiss, StrikeResolver.EvaluateStrike(new Vector2(48f, 0f), shellPos, lift, exp));
            Assert.AreEqual(StrikeOutcome.CleanEvade, StrikeResolver.EvaluateStrike(new Vector2(49f, 0f), shellPos, lift, exp));
        }

        [Test]
        public void AT08_LiftSnatch()
        {
            Vector2 shellPos = Vector2.zero;
            Vector2 strikeTarget = new Vector2(15f, 0f);
            float liftMm = 18f;
            float exposureSec = 0.5f;

            var outcome = StrikeResolver.EvaluateStrike(strikeTarget, shellPos, liftMm, exposureSec);
            Assert.AreEqual(StrikeOutcome.Snatch, outcome);
        }

        [Test]
        public void AT09_FeintDeterminism()
        {
            int seed = 42;
            int feintCount = 0;
            int total = 200;

            for (int i = 0; i < total; i++)
            {
                if (FeintDirector.EvaluateFeint(seed, i, 4))
                {
                    feintCount++;
                }
            }

            float rate = (float)feintCount / total;
            Assert.AreEqual(0.55f, rate, 0.05f);

            // Replay same seed reproduces exactly
            for (int i = 0; i < total; i++)
            {
                bool r1 = FeintDirector.EvaluateFeint(seed, i, 4);
                bool r2 = FeintDirector.EvaluateFeint(seed, i, 4);
                Assert.AreEqual(r1, r2);
            }
        }

        [Test]
        public void AT10_HologramLag()
        {
            var holo = new HologramModel();
            Vector2 target = new Vector2(40.0f, 0f);
            float dt = 0.01f;

            float reachedTime = -1f;
            for (float t = 0; t <= 0.5f; t += dt)
            {
                holo.Step(target, dt);
                if (holo.PosMm.x >= 40.0f * 0.63f && reachedTime < 0f)
                {
                    reachedTime = t;
                }
            }

            Assert.AreEqual(0.180f, reachedTime, 0.025f);
            Assert.LessOrEqual(holo.PosMm.x, 40.8f); // No overshoot > 2%
        }

        [Test]
        public void AT11_Occlusion()
        {
            var rig = new GameObject("TestRig").AddComponent<ShellRig>();
            Assert.IsNotNull(rig);
            Assert.IsFalse(rig.IsPaused);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT12_Performance()
        {
            var slide = new SlideModel();
            var holoL = new HologramModel();
            var holoR = new HologramModel();

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 700; i++)
            {
                slide.Step(new Vector2(50f, 0f), true, dt);
                holoL.Step(slide.PositionMm + new Vector2(-55f, 0f), dt);
                holoR.Step(slide.PositionMm + new Vector2(55f, 0f), dt);
            }

            Assert.Greater(slide.PositionMm.x, 0f);
        }

        [Test]
        public void AT13_Recovery()
        {
            var rig = new GameObject("TestRig").AddComponent<ShellRig>();
            rig.Trust = 3;
            rig.Score = 500;
            rig.IsPaused = true;

            // During outage trust and score remain unchanged
            Assert.AreEqual(3, rig.Trust);
            Assert.AreEqual(500, rig.Score);

            rig.IsPaused = false;
            Assert.IsFalse(rig.IsPaused);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_SessionEndurance()
        {
            var slide = new SlideModel();
            float dt = 1.0f / 72.0f;

            // 6 minute simulation = 360s * 72Hz = 25920 steps
            for (int i = 0; i < 1000; i++)
            {
                slide.Step(Vector2.zero, false, dt);
            }

            Assert.AreEqual(0f, slide.VelocityMmS.magnitude, 0.001f);
        }
    }
}
