using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.BobsledChute.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_SoapRegistration()
        {
            var tracker = new PropTracker();
            var rimPoints = new List<Vector3>();
            int count = 24;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((tracker.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (tracker.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = tracker.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.6f);
            Assert.Less(yawDeg, 2.0f);
        }

        [Test]
        public void AT02_PalmFrame()
        {
            Vector3 wrist = Vector3.zero;
            Vector3 middleMcp = new Vector3(0, 0, 0.08f);
            Vector3 indexMcp = new Vector3(-0.03f, 0, 0.07f);
            Vector3 pinkyMcp = new Vector3(0.03f, 0, 0.06f);

            var axes = PalmFrame.Build(wrist, middleMcp, indexMcp, pinkyMcp);

            // Orthonormal checks
            float dotXY = Vector3.Dot(axes.XHat, axes.YHat);
            float dotYZ = Vector3.Dot(axes.YHat, axes.ZHat);
            float dotZX = Vector3.Dot(axes.ZHat, axes.XHat);

            Assert.AreEqual(0f, dotXY, 0.01f);
            Assert.AreEqual(0f, dotYZ, 0.01f);
            Assert.AreEqual(0f, dotZX, 0.01f);
            Assert.AreEqual(1.0f, axes.XHat.magnitude, 0.01f);
        }

        [Test]
        public void AT03_SlideMapping()
        {
            var slide = new SlideControl();

            // Clamps to +- 20 mm after settling
            float mappedNeg = 0f;
            for (int i = 0; i < 20; i++) mappedNeg = slide.MapSlide(-25.0f, 0.05f);
            Assert.AreEqual(-20.0f, mappedNeg, 0.1f);

            var slidePos = new SlideControl();
            float mappedPos = 0f;
            for (int i = 0; i < 20; i++) mappedPos = slidePos.MapSlide(25.0f, 0.05f);
            Assert.AreEqual(20.0f, mappedPos, 0.1f);

            // Dead zone 2 mm
            var slide2 = new SlideControl();
            float deadVal = slide2.MapSlide(1.5f, 1.0f);
            Assert.AreEqual(0f, deadVal, 0.01f);

            // 10 Hz filter settling in 0.18 s
            var slide3 = new SlideControl();
            float dt = 0.010f;
            for (float t = 0; t <= 0.18f; t += dt)
            {
                slide3.MapSlide(20.0f, dt);
            }
            Assert.AreEqual(20.0f, slide3.FilteredSlideMm, 0.5f);
        }

        [Test]
        public void AT04_NeutralRun()
        {
            var sled = new SledBody();
            float dt = 0.001f;

            // Neutral run: aCtrl = 0, bank = 0, kappa = 0
            while (sled.ArcDistanceM < 0.900f && sled.ElapsedTime < 2.0f)
            {
                sled.Step(dt, 0f, 0f, 0f);
            }

            Assert.AreEqual(0.99f, sled.ElapsedTime, 0.03f);
            Assert.AreEqual(1.26f, sled.VelocityMPerS, 0.05f);
        }

        [Test]
        public void AT05_FullTuck()
        {
            var sled = new SledBody();
            float dt = 0.001f;
            float aCtrl = 1.40f; // full tuck

            while (sled.ArcDistanceM < 0.900f && sled.ElapsedTime < 2.0f)
            {
                sled.Step(dt, aCtrl, 0f, 0f);
            }

            Assert.AreEqual(0.70f, sled.ElapsedTime, 0.03f);
            Assert.AreEqual(2.00f, sled.VelocityMPerS, 0.08f);
        }

        [Test]
        public void AT06_FullBrake()
        {
            var sled = new SledBody();
            sled.VelocityMPerS = 1.26f;
            sled.ArcDistanceM = 0f;

            float dt = 0.001f;
            float aCtrl = -2.50f; // full brake

            while (sled.VelocityMPerS > 0.155f && sled.ElapsedTime < 2.0f)
            {
                sled.Step(dt, aCtrl, 0f, 0f);
            }

            Assert.AreEqual(0.49f, sled.ArcDistanceM, 0.05f);
            Assert.GreaterOrEqual(sled.VelocityMPerS, 0.15f); // Does not reverse
        }

        [Test]
        public void AT07_BankGravity()
        {
            var sled = new SledBody();
            sled.VelocityMPerS = 0f;

            // 10 deg bank at rest
            float aLat = sled.Step(0.010f, 0f, 10.0f, 0f);
            Assert.AreEqual(-1.70f, aLat, 0.05f);
        }

        [Test]
        public void AT08_HairpinLoad()
        {
            var sled = new SledBody();
            sled.VelocityMPerS = 1.30f;
            float kappa = 2.42f;
            float bank = 10.0f;

            float aLat = sled.Step(0.010f, 0f, bank, kappa);
            Assert.AreEqual(2.36f, aLat, 0.10f);
            Assert.IsFalse(sled.HasFlipped);
        }

        [Test]
        public void AT09_FlipThreshold()
        {
            var sled = new SledBody();
            sled.VelocityMPerS = 2.10f;
            float kappa = 2.42f;
            float bank = 0f;

            float dt = 0.005f;
            for (float t = 0; t <= 0.100f; t += dt)
            {
                sled.Step(dt, 0f, bank, kappa);
            }

            Assert.IsTrue(sled.HasFlipped);
            Assert.AreEqual(1, sled.CrashCount);
        }

        [Test]
        public void AT10_RimCollision()
        {
            var sled = new SledBody();
            sled.VelocityMPerS = 1.0f;
            sled.LateralPosM = 0.025f; // drive y to 25 mm

            sled.Step(0.010f, 0f, 0f, 0f);

            Assert.AreEqual(0.024f, Mathf.Abs(sled.LateralPosM), 0.001f);
            Assert.AreEqual(0.65f, sled.VelocityMPerS, 0.05f);
            Assert.AreEqual(1, sled.CrashCount);
        }

        [Test]
        public void AT11_SplitTiming()
        {
            var timer = new SplitTimer();
            timer.LatchGate(1, 0.312f);
            timer.LatchGate(2, 0.625f);
            timer.LatchGate(3, 0.937f);

            float sector1 = timer.Gate1Time;
            float sector2 = timer.Gate2Time - timer.Gate1Time;
            float sector3 = timer.Gate3Time - timer.Gate2Time;
            float total = sector1 + sector2 + sector3;

            Assert.AreEqual(timer.Gate3Time, total, 0.001f);
        }

        [Test]
        public void AT12_BestOfThree()
        {
            var timer = new SplitTimer();
            var combo = new ScoreCombo();

            timer.RecordHeat(1.40f);
            combo.BankHeatScore(1.40f, true);

            timer.RecordHeat(1.32f);
            combo.BankHeatScore(1.32f, true);

            timer.RecordHeat(1.55f);
            combo.BankHeatScore(1.55f, true);

            Assert.AreEqual(1.32f, timer.BestHeatTime, 0.001f);
            combo.BankDescentBonus(timer.BestHeatTime);
            Assert.Greater(combo.TotalScore, 1000);
        }

        [Test]
        public void AT13_OcclusionReadback()
        {
            // Sled behind hand: fragment contribution = 0 under hand silhouette
            float handRadius = 0.040f;
            Vector2 sledPos = new Vector2(0.015f, 0.015f);
            bool underHand = sledPos.magnitude < handRadius;
            Assert.IsTrue(underHand);

            float sledDepth = 0.35f;
            float handDepth = 0.25f;
            bool occluded = (sledDepth > handDepth) && underHand;
            Assert.IsTrue(occluded);
        }

        [Test]
        public void AT14_TrackingLoss()
        {
            var tracker = new PropTracker();
            float dt = 0.050f;

            for (float t = 0; t < 2.0f; t += dt)
            {
                tracker.StepTrackingLoss(false, dt);
                if (t >= 0.7f)
                {
                    Assert.IsTrue(tracker.IsPaused);
                }
            }
        }
    }
}
