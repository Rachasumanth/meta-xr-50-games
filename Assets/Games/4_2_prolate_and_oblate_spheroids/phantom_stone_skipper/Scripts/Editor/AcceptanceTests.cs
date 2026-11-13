using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.PhantomStoneSkipper.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_StoneRegistration()
        {
            var stone = new PropStone();
            var rimPoints = new List<Vector3>();
            int count = 20;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((stone.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (stone.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = stone.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.6f);
            Assert.Less(yawDeg, 4.0f);
        }

        [Test]
        public void AT02_PinchSeparationArming()
        {
            var snap = new SnapDetector();

            Assert.IsFalse(snap.CheckArmPinch(10.0f));
            Assert.IsTrue(snap.CheckArmPinch(13.0f));
            Assert.IsFalse(snap.CheckArmPinch(16.0f));

            bool fired = SnapDetector.CheckReleaseLaunch(true, 18.0f, 0.020f, out _);
            Assert.IsTrue(fired);
        }

        [Test]
        public void AT03_SnapSpeedAndSpin()
        {
            SnapDetector.ComputeInitialHop(30.0f, 1.2f, 12.0f, 2.0f,
                                           out float s0, out float h1, out float omega, out _, out _);

            Assert.AreEqual(42.8f, s0, 1.5f);
            Assert.AreEqual(12.0f + 3.4f * 2.0f, omega, 3.0f);
        }

        [Test]
        public void AT04_ReleaseTimingWindow()
        {
            // 60 ms after peak -> launch
            bool launch60 = SnapDetector.CheckReleaseLaunch(true, 18.0f, 0.060f, out bool dry60);
            Assert.IsTrue(launch60);
            Assert.IsFalse(dry60);

            // 150 ms after peak -> dry swing
            bool launch150 = SnapDetector.CheckReleaseLaunch(true, 18.0f, 0.150f, out bool dry150);
            Assert.IsFalse(launch150);
            Assert.IsTrue(dry150);
        }

        [Test]
        public void AT05_PitchResponses()
        {
            // Pitch 4 deg -> dive
            SnapDetector.ComputeInitialHop(30.0f, 1.2f, 4.0f, 0f, out _, out _, out _, out bool dive, out _);
            Assert.IsTrue(dive);

            // Pitch 12 deg -> h1 = 16 mm
            SnapDetector.ComputeInitialHop(30.0f, 1.2f, 12.0f, 0f, out _, out float h12, out _, out _, out _);
            Assert.AreEqual(16.0f, h12, 0.5f);

            // Pitch 30 deg -> stall with h1 = 6 mm and S1 x 0.6
            SnapDetector.ComputeInitialHop(30.0f, 1.2f, 30.0f, 0f, out float s30, out float h30, out _, out _, out bool stall);
            Assert.IsTrue(stall);
            Assert.AreEqual(6.0f, h30, 0.5f);
            Assert.AreEqual(42.8f * 0.6f, s30, 1.5f);
        }

        [Test]
        public void AT06_TapTimingRatings()
        {
            var judge = new TapJudge();

            // At t_touch (offset 0) -> Perfect
            var r0 = judge.EvaluateTap(0f, out _);
            Assert.AreEqual(TapRating.Perfect, r0);

            // At +80 ms -> Good
            var r80 = judge.EvaluateTap(0.080f, out _);
            Assert.AreEqual(TapRating.Good, r80);

            // At +150 ms -> Miss
            var r150 = judge.EvaluateTap(0.150f, out _);
            Assert.AreEqual(TapRating.Miss, r150);
        }

        [Test]
        public void AT07_EnergyRestoreFactors()
        {
            var judge = new TapJudge();

            judge.EvaluateTap(0f, out float ePerf);
            Assert.AreEqual(0.92f, ePerf, 0.02f);

            judge.EvaluateTap(0.080f, out float eGood);
            Assert.AreEqual(0.75f, eGood, 0.03f);

            judge.EvaluateTap(0.150f, out float eMiss);
            Assert.AreEqual(0.45f, eMiss, 0.03f);
        }

        [Test]
        public void AT08_TouchdownDistanceGating()
        {
            Assert.AreEqual("Gate", PondModel.EvaluateTouchdown(15.0f));
            Assert.AreEqual("Graze", PondModel.EvaluateTouchdown(20.0f));
            Assert.AreEqual("OpenWater", PondModel.EvaluateTouchdown(28.0f));
        }

        [Test]
        public void AT09_ConsecutiveMissesSink()
        {
            var judge = new TapJudge();

            judge.EvaluateTap(0.150f, out _); // 1st miss
            Assert.IsFalse(judge.IsSunken);

            // Perfect resets miss count
            judge.EvaluateTap(0f, out _);
            Assert.AreEqual(0, judge.ConsecutiveMisses);

            // Two consecutive misses
            judge.EvaluateTap(0.150f, out _);
            judge.EvaluateTap(0.150f, out _);
            Assert.IsTrue(judge.IsSunken);
        }

        [Test]
        public void AT10_PerfectChainMultiplier()
        {
            var judge = new TapJudge();
            for (int i = 0; i < 5; i++)
            {
                judge.EvaluateTap(0f, out _);
            }

            Assert.IsTrue(judge.IsPerfectChain);
            int baseScore = 500;
            int finalScore = judge.IsPerfectChain ? baseScore * 2 : baseScore;
            Assert.AreEqual(1000, finalScore);
        }

        [Test]
        public void AT11_HopClock()
        {
            Assert.AreEqual(0.42f, PhantomStoneSkipperConfig.HopDurationSec, 0.005f);
        }

        [Test]
        public void AT12_CoverFreeze()
        {
            var stone = new PropStone();
            float dt = 0.1f;
            for (float t = 0; t < 1.0f; t += dt)
            {
                stone.StepTrackingLoss(false, dt);
            }

            Assert.IsTrue(stone.IsPaused);
        }

        [Test]
        public void AT13_Level3PondClearable()
        {
            var judge = new TapJudge();
            int gates = 0;

            for (int i = 0; i < 10; i++)
            {
                judge.EvaluateTap(0f, out _);
                string res = PondModel.EvaluateTouchdown(10.0f);
                if (res == "Gate") gates++;
            }

            Assert.GreaterOrEqual(gates, 8);
            Assert.IsFalse(judge.IsSunken);
        }

        [Test]
        public void AT14_SessionSoak()
        {
            var phantom = new PhantomStone();
            phantom.IsFlying = true;

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                phantom.Step(dt, 0.92f);
            }

            Assert.Greater(phantom.HopIndex, 1);
        }
    }
}
