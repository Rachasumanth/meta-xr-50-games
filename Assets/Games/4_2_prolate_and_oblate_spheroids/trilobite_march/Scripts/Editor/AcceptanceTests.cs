using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.TrilobiteMarch.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_DomeRegistration()
        {
            var mouse = new PropMouse();
            var rimPoints = new List<Vector3>();
            int count = 20;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((mouse.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (mouse.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = mouse.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.8f);
            Assert.Less(yawDeg, 4.0f);
        }

        [Test]
        public void AT02_TapClassification()
        {
            // 70 ms, 0.03 N*s, inside zone
            bool tapValid = TapDetector.ClassifyTap('M', 0.070f, 0.03f, 0f, out var tap);
            Assert.IsTrue(tapValid);
            Assert.AreEqual('M', tap.Finger);

            // 150 ms press is not a tap
            bool tapLong = TapDetector.ClassifyTap('M', 0.150f, 0.03f, 0f, out _);
            Assert.IsFalse(tapLong);

            // 15 mm outside zone rejected
            bool tapOutside = TapDetector.ClassifyTap('M', 0.070f, 0.03f, 15.0f, out _);
            Assert.IsFalse(tapOutside);
        }

        [Test]
        public void AT03_GallopTripletQuality()
        {
            // 90/90 ms I-I-M triplet
            var a = new TapEvent('I', 0f, 0.03f);
            var b = new TapEvent('I', 0.090f, 0.03f);
            var c = new TapEvent('M', 0.180f, 0.03f);
            bool gallop90 = GallopClassifier.EvaluateTriplet(a, b, c, out float q90, out _);
            Assert.IsTrue(gallop90);
            Assert.AreEqual(1.0f, q90, 0.05f);

            // 55/125 ms triplet: dt1 = 55ms, dt2 = 125ms
            var b2 = new TapEvent('I', 0.055f, 0.03f);
            var c2 = new TapEvent('M', 0.180f, 0.03f); // 0.180 - 0.055 = 0.125
            bool gallop55 = GallopClassifier.EvaluateTriplet(a, b2, c2, out float q55, out _);
            Assert.IsTrue(gallop55);
            Assert.AreEqual(0.61f, q55, 0.05f);
        }

        [Test]
        public void AT04_ThreeDistinctFingersStumble()
        {
            var a = new TapEvent('I', 0f, 0.03f);
            var b = new TapEvent('M', 0.090f, 0.03f);
            var c = new TapEvent('R', 0.180f, 0.03f);

            bool gallop = GallopClassifier.EvaluateTriplet(a, b, c, out _, out bool isStumble);
            Assert.IsFalse(gallop);
            Assert.IsTrue(isStumble);
        }

        [Test]
        public void AT05_StrideAdvanceAndTau()
        {
            var gait = new GaitModel();
            gait.ApplyGallop(3.0f, 1.0f);

            // Advances exactly 24 mm
            Assert.AreEqual(24.0f, gait.DistanceMm);

            // Target speed is 72 mm/s; at tau = 0.25 s, speed reaches ~63% (45.5 mm/s)
            float dt = 0.010f;
            for (float t = 0; t <= 0.25f; t += dt)
            {
                gait.Step(dt);
            }

            Assert.AreEqual(72.0f * (1.0f - Mathf.Exp(-1f)), gait.SpeedMmS, 5.0f);
        }

        [Test]
        public void AT06_CadenceAndStreak()
        {
            var gait = new GaitModel();
            gait.ApplyGallop(3.0f, 0.9f);

            // 24 * 3.0 * 0.9 = 64.8 mm/s
            Assert.AreEqual(64.8f, gait.TargetSpeedMmS, 0.1f);

            // 10 consecutive gallops triggers streak
            for (int i = 0; i < 9; i++)
            {
                gait.ApplyGallop(3.0f, 0.9f);
            }

            Assert.IsTrue(gait.IsStreakActive);
            Assert.AreEqual(4.0f, gait.StreakTimer, 0.1f);
        }

        [Test]
        public void AT07_SinkTimerAndRecovery()
        {
            var gait = new GaitModel();

            // No gallop for 1.6 s starts sinking
            float dt = 0.1f;
            for (float t = 0; t < 2.0f; t += dt)
            {
                gait.Step(dt);
            }

            Assert.Greater(gait.SinkLevel, 0f);

            float sinkBefore = gait.SinkLevel;
            gait.ApplyGallop(2.0f, 1.0f);
            Assert.AreEqual(Mathf.Max(0f, sinkBefore - 0.35f), gait.SinkLevel, 0.01f);
            Assert.AreEqual(0f, gait.IdleTimer);
        }

        [Test]
        public void AT08_TwoPadCurl()
        {
            var curl = new CurlModel();

            // One pad held 120 ms
            bool onePad = curl.UpdateContact(true, false, 0.120f);
            Assert.IsFalse(onePad);

            // Two pads held 120 ms
            bool twoPads = curl.UpdateContact(true, true, 0.120f);
            Assert.IsTrue(twoPads);
        }

        [Test]
        public void AT09_SweepProtectionWindow()
        {
            bool protected60 = CurlModel.CheckProtection(0.60f, 0.90f);
            Assert.IsTrue(protected60);

            bool protected55 = CurlModel.CheckProtection(0.55f, 0.90f);
            Assert.IsFalse(protected55);
        }

        [Test]
        public void AT10_MissedCurlPlateLoss()
        {
            var director = new MigrationDirector();
            Assert.AreEqual(3, director.Plates);

            director.ApplyMissedCurl();
            Assert.AreEqual(2, director.Plates);
        }

        [Test]
        public void AT11_CheckpointPlateAndBonus()
        {
            var director = new MigrationDirector();
            director.Plates = 2;

            director.CheckAdvance(800.0f);
            Assert.AreEqual(3, director.Plates);
            Assert.AreEqual(100, director.Score);

            // Crossing at 3 plates converts excess to +50
            var directorFull = new MigrationDirector();
            directorFull.Plates = 3;
            directorFull.CheckAdvance(800.0f);
            Assert.AreEqual(3, directorFull.Plates);
            Assert.AreEqual(50, directorFull.Score);
        }

        [Test]
        public void AT12_CoverFreeze()
        {
            var mouse = new PropMouse();
            float dt = 0.1f;
            for (float t = 0; t < 1.6f; t += dt)
            {
                mouse.StepTrackingLoss(false, dt);
            }

            Assert.IsTrue(mouse.IsPaused);
        }

        [Test]
        public void AT13_Level3MigrationClearable()
        {
            var gait = new GaitModel();
            var director = new MigrationDirector();

            for (int i = 0; i < 40; i++)
            {
                gait.ApplyGallop(3.0f, 1.0f);
            }

            director.CheckAdvance(gait.DistanceMm);
            Assert.GreaterOrEqual(director.Plates, 2);
        }

        [Test]
        public void AT14_SessionSoak()
        {
            var gait = new GaitModel();
            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                gait.Step(dt);
            }

            Assert.GreaterOrEqual(gait.DistanceMm, 0f);
        }
    }
}
