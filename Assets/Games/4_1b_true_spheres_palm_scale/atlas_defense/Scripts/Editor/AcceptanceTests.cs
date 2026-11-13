using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.AtlasDefense.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_SphereRegistration()
        {
            var tracker = new PropTracker();
            Vector3[] samples = new Vector3[24];
            for (int i = 0; i < 24; i++)
            {
                float a = i * (360f / 24f) * Mathf.Deg2Rad;
                samples[i] = new Vector3(Mathf.Cos(a) * 38.05f, Mathf.Sin(a) * 38.05f, 0f);
            }

            bool ok = tracker.RegisterSphere(samples, 38.0f);
            Assert.IsTrue(ok);
            Assert.Less(Mathf.Abs(tracker.MeasuredRadiusMm - 38.0f), 0.2f);
            Assert.Less(tracker.RmsErrorMm, 0.5f);
        }

        [Test]
        public void Test02_RollingLaw()
        {
            var roll = new RollKinematics();
            Vector3 n = Vector3.forward; // +z_V
            Vector3 vt = new Vector3(120f, 0f, 0f); // 120 mm/s * x_V
            Vector3 omega = roll.ComputeRollingOmega(n, vt, 38.0f);

            Assert.AreEqual(3.16f, omega.magnitude, 3.16f * 0.03f); // 3.16 rad/s +/- 3%
            Assert.AreEqual(Vector3.up, omega.normalized); // +y_V axis
        }

        [Test]
        public void Test03_Full360Reach()
        {
            var roll = new RollKinematics();
            for (int i = 0; i < 4; i++)
            {
                roll.StepStroke(95.0f); // 4 strokes
            }
            Assert.GreaterOrEqual(roll.TotalUnwrappedYawDeg, 360.0f);
        }

        [Test]
        public void Test04_LockHold()
        {
            var shield = new ShieldSystem();
            var m = new MeteorInstance();

            // 240 ms hold at 19 deg -> no lock
            shield.UpdateLock(m, 19.0f, 0.240f);
            Assert.IsFalse(m.IsLocked);

            // 250 ms -> locked
            shield.UpdateLock(m, 19.0f, 0.015f); // 255 ms total
            Assert.IsTrue(m.IsLocked);
        }

        [Test]
        public void Test05_LockHysteresis()
        {
            var shield = new ShieldSystem();
            var m = new MeteorInstance();

            // Lock at 19 deg
            shield.UpdateLock(m, 19.0f, 0.250f);
            Assert.IsTrue(m.IsLocked);

            // Increase to 23 deg (below 24 release) -> stays locked
            shield.UpdateLock(m, 23.0f, 0.050f);
            Assert.IsTrue(m.IsLocked);

            // Increase to 25 deg (above 24 release) -> unlocks
            shield.UpdateLock(m, 25.0f, 0.050f);
            Assert.IsFalse(m.IsLocked);
        }

        [Test]
        public void Test06_InterceptWindow()
        {
            var shield = new ShieldSystem();
            var m = new MeteorInstance();
            shield.UpdateLock(m, 19.0f, 0.250f);

            // Locked and theta < 25 deg -> intercept true
            Assert.IsTrue(shield.CheckIntercept(m, 22.0f));

            // Theta >= 25 deg -> false
            Assert.IsFalse(shield.CheckIntercept(m, 25.5f));
        }

        [Test]
        public void Test07_CityDamage()
        {
            var grid = new CityGrid();
            var city = grid.FindNearestCity(new Vector3(38f, 0f, 0f));
            int initialHp = city.HP;

            grid.DamageCity(city, 1);
            Assert.AreEqual(initialHp - 1, city.HP);
        }

        [Test]
        public void Test08_ScoreCombo()
        {
            var score = new ScoreCombo();
            score.OnIntercept(15.0f);
            Assert.AreEqual(1, score.Combo);
            Assert.AreEqual(100, score.Score);

            score.OnIntercept(8.0f); // with precision bonus
            Assert.AreEqual(2, score.Combo);

            score.OnImpact();
            Assert.AreEqual(0, score.Combo);
        }

        [Test]
        public void Test09_ConcurrencyCap()
        {
            var director = new MeteorDirector();
            for (int i = 0; i < 6; i++)
            {
                director.TrySpawnMeteor(Vector3.forward);
            }
            Assert.LessOrEqual(director.ActiveMeteors.Count, AtlasDefenseConfig.MaxConcurrentMeteors);
        }

        [Test]
        public void Test10_OffsetClamp()
        {
            var globe = new GlobeController();
            globe.SetOffset(new Vector3(30.0f, -40.0f, 10.0f));
            Assert.AreEqual(15.0f, globe.OffsetMm.x, 0.001f);
            Assert.AreEqual(-15.0f, globe.OffsetMm.y, 0.001f);
            Assert.AreEqual(10.0f, globe.OffsetMm.z, 0.001f);
        }

        [Test]
        public void Test11_TrackingLoss()
        {
            var tracker = new PropTracker();
            tracker.SetPose(Vector3.zero, Quaternion.identity);
            Assert.IsTrue(tracker.IsRegistered);
        }

        [Test]
        public void Test12_WaveProgression()
        {
            var fsm = new WaveStateMachine();
            fsm.StartWave(1);
            Assert.AreEqual(DefenseWaveState.ActiveDefense, fsm.State);
            fsm.ClearWave();
            Assert.AreEqual(DefenseWaveState.WaveClear, fsm.State);
        }

        [Test]
        public void Test13_FrameBudget()
        {
            var feedback = new FeedbackRouter();
            feedback.PlayInterceptEffect();
            feedback.PlayImpactEffect();
            Assert.LessOrEqual(feedback.ActiveParticles, AtlasDefenseConfig.MaxParticleBudget);
        }

        [Test, Category("NeedsDevice")]
        public void Test14_Determinism()
        {
            Assert.Ignore("Deferred: Requires physical headset audio timing and depth capture.");
        }
    }
}
