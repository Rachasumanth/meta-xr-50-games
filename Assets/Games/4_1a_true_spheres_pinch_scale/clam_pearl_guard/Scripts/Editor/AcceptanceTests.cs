/*
Acceptance Tests for Clam Pearl Guard (Game 02)
Derived mechanically from spec section 16 (game_02_clam_pearl_guard.md).
Automated Tests (EditMode):
- Test01_SphereFit        (16.1 r in [8.38,8.62], RMS < 0.35)
- Test03_Openness         (16.3 O = 1.0/1.0/0.99/0.50/0.0 at 19/20/21/70/121 deg)
- Test04_ChargeRate       (16.4 charge 60 +/- 1 after 5 s at rate 12/s)
- Test05_TiltFocus        (16.5 F = 1.00/1.17/0.80 at 0/10/30 deg)
- Test06_CageThreshold    (16.6 cage at 34mm, held at 36mm, release above 42mm)
- Test07_ClosureWindow    (16.7 90ms closure -> fast bonus, 150ms -> normal)
- Test08_BiteTiming       (16.8 610/700/800 ms -> damage 0/11/22)
- Test11_DropDetection    (16.11 7mm shift re-solve, 12mm drop)
- Test14_Accessibility    (16.14 cage triggers with 44mm personal calibration)
NeedsDevice / deferred with rationale in BUILD_LOG.md:
- Test02_PalmLock_Deferred      (16.2 pose error + rendered lag on device)
- Test09_NoPhantomBite_Deferred (16.9 tracking-drop session flow)
- Test10_Occlusion_Deferred     (16.10 rendering path)
- Test12_FrameBudget_Deferred   (16.12 GPU/CPU profiling)
- Test13_SessionEndurance_Deferred (16.13 7-minute device session)
Total: 14 items (9 automated, 5 deferred to device)
*/

using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_SphereFit()
        {
            var pearlReg = new PearlRegistration();
            Vector3[] samplePoints = new Vector3[120];
            for (int i = 0; i < 120; i++)
            {
                float angle = i * Mathf.Deg2Rad * 3f;
                samplePoints[i] = new Vector3(Mathf.Cos(angle) * 8.5f, Mathf.Sin(angle) * 8.5f, 0f);
            }
            bool success = pearlReg.FitSphere(samplePoints, 8.5f);
            Assert.IsTrue(success);
            Assert.Less(pearlReg.RmsError, ClamPearlGuardConfig.SphereFitRmsMax);
            Assert.GreaterOrEqual(pearlReg.CurrentRadius, ClamPearlGuardConfig.PearlRadiusMin);
            Assert.LessOrEqual(pearlReg.CurrentRadius, ClamPearlGuardConfig.PearlRadiusMax);
        }

        [Test, Category("NeedsDevice")]
        public void Test02_PalmLock_Deferred() { Assert.Ignore("Deferred: Requires physical Meta XR device and hand tracking."); }

        [Test]
        public void Test03_Openness()
        {
            Assert.AreEqual(1.0f, CurlEstimator.ComputeOpenness(19f), 0.01f);
            Assert.AreEqual(1.0f, CurlEstimator.ComputeOpenness(20f), 0.01f);
            Assert.AreEqual(0.99f, CurlEstimator.ComputeOpenness(21f), 0.01f);
            Assert.AreEqual(0.50f, CurlEstimator.ComputeOpenness(70f), 0.01f);
            Assert.AreEqual(0.0f, CurlEstimator.ComputeOpenness(121f), 0.01f);

            // The streaming path must agree with the law function.
            var curl = new CurlEstimator();
            curl.UpdateCurls(new float[] { 70f, 70f, 70f, 70f, 70f });
            Assert.AreEqual(0.50f, curl.Openness, 0.01f);
        }

        [Test]
        public void Test04_ChargeRate()
        {
            var charger = new SunCharger();
            charger.UpdateCharge(1.0f, 0f, 5.0f);
            Assert.AreEqual(60.0f, charger.Charge, 1.0f);

            // Full bloom at 8.33 s (100 units at 12/s).
            var full = new SunCharger();
            full.UpdateCharge(1.0f, 0f, 8.33f);
            Assert.AreEqual(ClamPearlGuardConfig.ChargeFullUnits, full.Charge, 1.0f);
            Assert.AreEqual(1.0f, full.Bloom, 0.01f);
        }

        [Test]
        public void Test05_TiltFocus()
        {
            Assert.AreEqual(1.00f, SunCharger.ComputeTiltFocus(0f), 0.02f);
            Assert.AreEqual(1.17f, SunCharger.ComputeTiltFocus(10f), 0.02f);
            Assert.AreEqual(0.80f, SunCharger.ComputeTiltFocus(30f), 0.02f);
        }

        [Test]
        public void Test06_CageThreshold()
        {
            var cage = new CageClassifier();
            float[] calib = { 35f, 35f, 35f, 35f, 35f };
            Vector3 palm = Vector3.zero;

            // 34 mm normalized tips -> sustained below 35 -> cage closes (debounce 100 ms).
            Vector3[] tips34 = { new Vector3(0.034f, 0, 0), new Vector3(0.034f, 0, 0), new Vector3(0.034f, 0, 0), new Vector3(0.034f, 0, 0), new Vector3(0.034f, 0, 0) };
            for (int i = 0; i < 5; i++) cage.Evaluate(tips34, palm, calib, 0f, 0.01f);
            for (int i = 0; i < 5; i++) cage.Evaluate(tips34, palm, calib, 0.02f, 0.01f);
            for (int i = 0; i < 5; i++) cage.Evaluate(tips34, palm, calib, 0.03f, 0.01f);
            Assert.IsTrue(cage.IsCageClosed);

            // 36 mm: inside the hysteresis band [35, 42] -> cage holds (no release below 42).
            Vector3[] tips36 = { new Vector3(0.036f, 0, 0), new Vector3(0.036f, 0, 0), new Vector3(0.036f, 0, 0), new Vector3(0.036f, 0, 0), new Vector3(0.036f, 0, 0) };
            for (int i = 0; i < 3; i++) cage.Evaluate(tips36, palm, calib, 0.04f, 0.01f);
            Assert.IsTrue(cage.IsCageClosed, "release only above 42 mm");

            // 43 mm above the open threshold -> cage releases.
            Vector3[] tips43 = { new Vector3(0.043f, 0, 0), new Vector3(0.043f, 0, 0), new Vector3(0.043f, 0, 0), new Vector3(0.043f, 0, 0), new Vector3(0.043f, 0, 0) };
            for (int i = 0; i < 3; i++) cage.Evaluate(tips43, palm, calib, 0.05f, 0.01f);
            Assert.IsFalse(cage.IsCageClosed);
        }

        [Test]
        public void Test07_ClosureWindow()
        {
            // 90 ms closure within the 120 ms fast-bonus window -> +150.
            var fast = new BarracudaDirector();
            int integrity = 100, score = 0;
            var resFast = fast.ResolveStrike(610f, true, 90f, ref integrity, ref score);
            Assert.AreEqual(BarracudaDirector.StrikeResult.Block, resFast);
            Assert.AreEqual(ClamPearlGuardConfig.ScoreFastBonusPoints, score);

            // 150 ms closure past the window -> normal block points, no fast bonus.
            var slow = new BarracudaDirector();
            int integrity2 = 100, score2 = 0;
            var resSlow = slow.ResolveStrike(610f, true, 150f, ref integrity2, ref score2);
            Assert.AreEqual(BarracudaDirector.StrikeResult.Block, resSlow);
            Assert.AreEqual(ClamPearlGuardConfig.ScoreNormalBlockPoints, score2);
        }

        [Test]
        public void Test08_BiteTiming()
        {
            var director = new BarracudaDirector();
            int integrity = 100;
            int score = 0;

            // Early strike with cage closed: full block, damage 0.
            var res1 = director.ResolveStrike(610f, true, 100f, ref integrity, ref score);
            Assert.AreEqual(BarracudaDirector.StrikeResult.Block, res1);
            Assert.AreEqual(100, integrity);

            // Mid strike with cage closed: late block, damage 11.
            var res2 = director.ResolveStrike(700f, true, 100f, ref integrity, ref score);
            Assert.AreEqual(BarracudaDirector.StrikeResult.Block, res2);
            Assert.AreEqual(89, integrity);

            // Late strike without cage: full bite, damage 22.
            var res3 = director.ResolveStrike(800f, false, 100f, ref integrity, ref score);
            Assert.AreEqual(BarracudaDirector.StrikeResult.Bitten, res3);
            Assert.AreEqual(67, integrity);
        }

        [Test, Category("NeedsDevice")]
        public void Test09_NoPhantomBite_Deferred() { Assert.Ignore("Deferred: Requires physical tracking drop simulation."); }

        [Test, Category("NeedsDevice")]
        public void Test10_Occlusion_Deferred() { Assert.Ignore("Deferred: Requires XR camera rendering and shaders."); }

        [Test]
        public void Test11_DropDetection()
        {
            var pearlReg = new PearlRegistration();
            pearlReg.SetInitialPosition(Vector3.zero);

            // 7 mm shift: inside the shift-tolerance flow, no drop.
            // Convention: 1 unit = 1 mm (same as FitSphere's 8.5-unit samples).
            pearlReg.CheckShiftAndDrop(new Vector3(7f, 0, 0), 0.05f);
            Assert.IsFalse(pearlReg.IsDropped, "7 mm shift must not drop");

            // 12 mm: drop flow starts.
            pearlReg.CheckShiftAndDrop(new Vector3(12f, 0, 0), 0.05f);
            Assert.IsTrue(pearlReg.IsDropped, "12 mm displacement starts the drop flow");
        }

        [Test, Category("NeedsDevice")]
        public void Test12_FrameBudget_Deferred() { Assert.Ignore("Deferred: Requires Unity GPU profiling."); }

        [Test, Category("NeedsDevice")]
        public void Test13_SessionEndurance_Deferred() { Assert.Ignore("Deferred: Requires 7 min runtime execution."); }

        [Test]
        public void Test14_Accessibility()
        {
            // Personal fist calibration at 44 mm: normalization d = raw * 35 / 44.
            // 34 mm raw tips normalize to ~27 mm -> cage still triggers.
            var cage = new CageClassifier();
            float[] calib = { 44f, 44f, 44f, 44f, 44f };
            Vector3 palm = Vector3.zero;
            Vector3[] tips34 = { new Vector3(0.034f, 0, 0), new Vector3(0.034f, 0, 0), new Vector3(0.034f, 0, 0), new Vector3(0.034f, 0, 0), new Vector3(0.034f, 0, 0) };
            for (int i = 0; i < 5; i++) cage.Evaluate(tips34, palm, calib, 0f, 0.01f);
            for (int i = 0; i < 5; i++) cage.Evaluate(tips34, palm, calib, 0.02f, 0.01f);
            for (int i = 0; i < 5; i++) cage.Evaluate(tips34, palm, calib, 0.03f, 0.01f);
            Assert.IsTrue(cage.IsCageClosed, "cage must trigger with 44 mm personal calibration");
        }
    }
}
