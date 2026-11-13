using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.EllipticalOrbit.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_EggRegistration()
        {
            var egg = new PropEgg();
            var rimPoints = new List<Vector3>();
            int count = 20;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((egg.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (egg.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = egg.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.7f);
            Assert.Less(yawDeg, 4.0f);
        }

        [Test]
        public void AT02_EffectiveRadius()
        {
            float r0 = RollModel.ComputeEffectiveRadiusMm(0f);
            float r90 = RollModel.ComputeEffectiveRadiusMm(90f * Mathf.Deg2Rad);

            Assert.AreEqual(42.71f, r0, 0.5f);
            Assert.AreEqual(22.50f, r90, 0.3f);
        }

        [Test]
        public void AT03_ProgradeNudgeSpeedAndToken()
        {
            var roll = new RollModel();
            var nudge = new NudgeClassifier();

            Assert.AreEqual(6, nudge.Tokens);
            bool spent = nudge.TrySpendToken();
            Assert.IsTrue(spent);
            Assert.AreEqual(5, nudge.Tokens);

            roll.ApplyProgradeNudge(3.0f); // 3.0 mN*s
            Assert.AreEqual(40.0f, roll.SpeedMmS, 2.0f);
        }

        [Test]
        public void AT04_RollingFrictionTravel()
        {
            var roll = new RollModel();
            roll.SpeedMmS = 160.0f;

            float dt = 0.010f;
            for (float t = 0; t < 10.0f; t += dt)
            {
                roll.Step(dt);
            }

            Assert.AreEqual(295.0f, roll.TotalTravelMm, 25.0f);
            Assert.AreEqual(0f, roll.SpeedMmS);
        }

        [Test]
        public void AT05_CurvatureRadii()
        {
            float kPlus45 = RollModel.ComputeCurvatureMmInv(45f * Mathf.Deg2Rad);
            float rPlus45 = 1.0f / kPlus45;
            Assert.AreEqual(50.0f, rPlus45, 5.0f);

            float kZero = RollModel.ComputeCurvatureMmInv(0f);
            float rZero = 1.0f / kZero;
            Assert.AreEqual(125.0f, rZero, 8.0f);

            float kNeg45 = RollModel.ComputeCurvatureMmInv(-45f * Mathf.Deg2Rad);
            // Sign reverses compared to kPlus45
            Assert.Less(kNeg45, 0f);
        }

        [Test]
        public void AT06_FlankNudgeAlpha()
        {
            var roll = new RollModel();
            roll.ApplyFlankNudge(3.0f, 20.0f); // 3 mN*s at 20 mm

            Assert.AreEqual(0.27f, roll.AlphaRad, 0.03f);
        }

        [Test]
        public void AT07_DustCaptureRadius()
        {
            var dust = new DustSystem();
            dust.Particles.Add(new DustParticle(new Vector2(14.0f, 0f)));
            dust.Particles.Add(new DustParticle(new Vector2(15.5f, 0f)));

            bool hit = dust.CheckCapture(Vector2.zero, 14.0f);
            Assert.IsTrue(hit);
            Assert.AreEqual(1, dust.CapturedCount);
        }

        [Test]
        public void AT08_TailSplineCut()
        {
            var tail = new TailSystem();

            // Tangential crossing (< 30 deg) within 9mm
            bool cutTangential = tail.CheckCut(8.0f, 20.0f);
            Assert.IsFalse(cutTangential);

            // Transverse crossing (>= 30 deg) within 9mm
            bool cutTransverse = tail.CheckCut(8.0f, 60.0f);
            Assert.IsTrue(cutTransverse);
            Assert.AreEqual(1, tail.CutsCount);
        }

        [Test]
        public void AT09_OrbitClosureBonus()
        {
            var tracker = new OrbitTracker();

            // 300 deg unwrapped, 24 mm return
            bool closed24 = tracker.CheckClosure(300.0f, 24.0f, out int bonus24);
            Assert.IsTrue(closed24);
            Assert.AreEqual(300, bonus24);

            // 27 mm return
            bool closed27 = tracker.CheckClosure(300.0f, 27.0f, out int bonus27);
            Assert.IsFalse(closed27);
            Assert.AreEqual(0, bonus27);
        }

        [Test]
        public void AT10_EllipticityScoring()
        {
            int score35 = OrbitJudge.ScoreEllipticity(0.35f);
            Assert.AreEqual(200, score35, 5);

            int score10 = OrbitJudge.ScoreEllipticity(0.10f);
            Assert.Less(score10, 60);

            int score60 = OrbitJudge.ScoreEllipticity(0.60f);
            Assert.Less(score60, 60);
        }

        [Test]
        public void AT11_StallDetection()
        {
            var roll = new RollModel();
            roll.SpeedMmS = 20.0f; // < 25 mm/s

            float dt = 0.1f;
            for (float t = 0; t < 1.25f; t += dt)
            {
                roll.Step(dt);
            }

            Assert.IsTrue(roll.IsStalled);
        }

        [Test]
        public void AT12_CoverFreeze()
        {
            var egg = new PropEgg();
            float dt = 0.1f;
            for (float t = 0; t < 1.0f; t += dt)
            {
                egg.StepTrackingLoss(false, dt);
            }

            Assert.IsTrue(egg.IsPaused);
        }

        [Test]
        public void AT13_Level3OrbitClearable()
        {
            var dust = new DustSystem();
            dust.Seed(60);
            var tail = new TailSystem();
            var tracker = new OrbitTracker();

            for (int i = 0; i < 60; i += 5)
            {
                dust.CheckCapture(dust.Particles[i].PositionMm);
            }

            bool closed = tracker.CheckClosure(320.0f, 15.0f, out int bonus);
            Assert.IsTrue(closed);
            Assert.Greater(dust.CapturedCount, 5);
        }

        [Test]
        public void AT14_SessionSoak()
        {
            var roll = new RollModel();
            roll.SpeedMmS = 100.0f;

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                roll.Step(dt);
            }

            Assert.GreaterOrEqual(roll.TotalTravelMm, 0f);
        }
    }
}
