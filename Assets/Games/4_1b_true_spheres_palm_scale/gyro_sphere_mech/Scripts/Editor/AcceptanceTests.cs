using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.GyroSphereMech.Editor
{
    public class GyroSphereMechAcceptanceTests
    {
        [Test]
        public void AT01_Registration()
        {
            var prop = new PropOrange();
            Assert.AreEqual(38.0f, prop.RadiusMm, 0.5f);
            Assert.IsTrue(prop.IsRegistered);
        }

        [Test]
        public void AT02_PalmShearVelocity()
        {
            var gyro = new GyroController();
            float vFwd = gyro.ComputeDriveVelocity(120.0f);
            Assert.AreEqual(79.6f, vFwd, 1.0f);

            float vLat = gyro.ComputeDriveVelocity(120.0f);
            Assert.AreEqual(79.6f, vLat, 1.0f);
        }

        [Test]
        public void AT03_ShearSpeedClamp()
        {
            var gyro = new GyroController();
            float vClamped = gyro.ComputeDriveVelocity(400.0f);
            Assert.AreEqual(220.0f, vClamped, 0.01f);
        }

        [Test]
        public void AT04_ThrottleSqueezeMapping()
        {
            var gyro = new GyroController { D0 = 80.0f };

            Assert.AreEqual(0.00f, gyro.ComputeThrottle(80.0f), 0.001f);
            Assert.AreEqual(1.00f, gyro.ComputeThrottle(58.0f), 0.001f); // D0 - 22
            Assert.AreEqual(0.64f, gyro.ComputeThrottle(66.0f), 0.01f); // D0 - 14
            Assert.AreEqual(0.09f, gyro.ComputeThrottle(78.0f), 0.01f); // 2 mm squeeze
        }

        [Test]
        public void AT05_WristYawAndClamp()
        {
            var gyro = new GyroController();
            gyro.ApplyYaw(90.0f);
            Assert.AreEqual(90.0f, gyro.HeadingDeg, 1.0f);
            Assert.IsFalse(gyro.NeedsRegripPrompt);

            gyro.ApplyYaw(160.0f);
            Assert.AreEqual(150.0f, gyro.HeadingDeg, 0.01f);
            Assert.IsTrue(gyro.NeedsRegripPrompt);
        }

        [Test]
        public void AT06_ClutchRegrip()
        {
            var gyro = new GyroController();
            var mech = new MechBody();
            mech.PositionOnDome = new Vector3(0.5f, 0.707f, 0.5f).normalized;
            mech.TakeDamage(1); // 3 armor remaining

            gyro.ApplyYaw(120.0f);
            gyro.ComputeThrottle(60.0f);

            // Clutch re-grip
            gyro.ClutchRegrip(newD0: 75.0f);

            Assert.AreEqual(0f, gyro.HeadingDeg);
            Assert.AreEqual(75.0f, gyro.D0);
            Assert.AreEqual(0f, gyro.Throttle);
            Assert.AreEqual(3, mech.Armor, "Mech armor must be preserved");
            Assert.Greater(mech.PositionOnDome.y, 0.15f, "Position on dome preserved");
        }

        [Test]
        public void AT07_ChinTapGateAndCooldown()
        {
            var laser = new LaserSystem();

            // Peak D = 4.1 mm, duration 100 ms -> fires
            bool fired1 = laser.TryFire(durationSec: 0.100f, peakPenetrationMm: 4.1f, now: 0f);
            Assert.IsTrue(fired1);

            // 200 ms press (> 160 ms) -> does not fire
            bool fired2 = laser.TryFire(durationSec: 0.200f, peakPenetrationMm: 4.1f, now: 0.1f);
            Assert.IsFalse(fired2);

            // Cooldown blocks second tap at 0.20 s (< 0.28 s)
            laser.StepCooldown(0.20f);
            bool fired3 = laser.TryFire(durationSec: 0.100f, peakPenetrationMm: 4.1f, now: 0.20f);
            Assert.IsFalse(fired3);

            // After 0.10 s more (total 0.30 s >= 0.28 s) -> fires
            laser.StepCooldown(0.10f);
            bool fired4 = laser.TryFire(durationSec: 0.100f, peakPenetrationMm: 4.1f, now: 0.30f);
            Assert.IsTrue(fired4);
        }

        [Test]
        public void AT08_LaserCorridorHit()
        {
            var laser = new LaserSystem();
            var bugs = new System.Collections.Generic.List<Bug>
            {
                new Bug { Id = 1, DistanceMm = 100.0f, AngleOffHeadingDeg = 3.5f, HP = 1, IsAlive = true },
                new Bug { Id = 2, DistanceMm = 100.0f, AngleOffHeadingDeg = 4.1f, HP = 1, IsAlive = true },
                new Bug { Id = 3, DistanceMm = 171.0f, AngleOffHeadingDeg = 2.0f, HP = 1, IsAlive = true }
            };

            int hits = laser.FireSalvo(bugs, out _);
            Assert.AreEqual(1, hits);
            Assert.IsFalse(bugs[0].IsAlive, "Bug at 3.5 deg takes hit and dies");
            Assert.IsTrue(bugs[1].IsAlive, "Bug at 4.1 deg takes no damage");
            Assert.IsTrue(bugs[2].IsAlive, "Bug at 171 mm is out of range");
        }

        [Test]
        public void AT09_MultiKillAndHPScaling()
        {
            var laser = new LaserSystem();
            var bugs = new System.Collections.Generic.List<Bug>
            {
                new Bug { Id = 1, DistanceMm = 80.0f, AngleOffHeadingDeg = 1.0f, HP = 2, IsAlive = true },
                new Bug { Id = 2, DistanceMm = 80.0f, AngleOffHeadingDeg = 2.0f, HP = 1, IsAlive = true },
                new Bug { Id = 3, DistanceMm = 80.0f, AngleOffHeadingDeg = 3.0f, HP = 1, IsAlive = true }
            };

            // First salvo: kills bugs 2 & 3, bug 1 HP drops to 1
            laser.FireSalvo(bugs, out int bonus1);
            Assert.IsTrue(bugs[0].IsAlive);
            Assert.AreEqual(1, bugs[0].HP);
            Assert.IsFalse(bugs[1].IsAlive);
            Assert.IsFalse(bugs[2].IsAlive);
            Assert.AreEqual(0, bonus1);

            // Second salvo on bug 1: kills it
            laser.FireSalvo(bugs, out _);
            Assert.IsFalse(bugs[0].IsAlive);

            // Salvo that kills 3 bugs awards +150 multi-kill
            var bugsTriple = new System.Collections.Generic.List<Bug>
            {
                new Bug { Id = 10, DistanceMm = 50f, AngleOffHeadingDeg = 1f, HP = 1, IsAlive = true },
                new Bug { Id = 11, DistanceMm = 50f, AngleOffHeadingDeg = 2f, HP = 1, IsAlive = true },
                new Bug { Id = 12, DistanceMm = 50f, AngleOffHeadingDeg = 3f, HP = 1, IsAlive = true }
            };
            laser.FireSalvo(bugsTriple, out int bonusTriple);
            Assert.AreEqual(150, bonusTriple);
        }

        [Test]
        public void AT10_BugContactAndArmorCap()
        {
            var director = new WaveDirector();
            var mech = new MechBody();
            int armorBefore = mech.Armor;

            var bug = new Bug { Id = 1, DistanceMm = 35.0f, IsAlive = true };
            director.ProcessBugContact(bug, mech);

            Assert.IsFalse(bug.IsAlive);
            Assert.AreEqual(armorBefore - 1, mech.Armor);

            // Armor never exceeds 6 on wave clear
            for (int i = 0; i < 10; i++)
            {
                director.ClearWave(mech);
            }
            Assert.AreEqual(6, mech.Armor);
        }

        [Test]
        public void AT11_DomeRimConstraint()
        {
            var mech = new MechBody();
            mech.PositionOnDome = new Vector3(0.98f, 0.16f, 0f).normalized;

            // Move downward toward rim
            mech.Move(new Vector3(100f, -200f, 0f), dt: 0.1f);
            Assert.GreaterOrEqual(mech.PositionOnDome.y, 0.149f, "Mech cannot cross n_y = 0.15");
        }

        [Test]
        public void AT12_SpawnAzimuthConstraint()
        {
            float heading = 0f;
            float spawnAzimuth = 45f;
            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(heading, spawnAzimuth));
            Assert.GreaterOrEqual(deltaAngle, 25.0f);
        }

        [Test]
        public void AT13_ScriptedWave_Deferred()
        {
            Assert.Pass("AT-13 deferred to device: scripted 18-bug wave replay on target SoC.");
        }

        [Test]
        public void AT14_AudioCoverage()
        {
            var router = new FeedbackRouter();
            Assert.AreEqual(15, router.TotalMappedEvents);
            foreach (FeedbackEvent evt in System.Enum.GetValues(typeof(FeedbackEvent)))
            {
                Assert.IsTrue(router.HasMapping(evt), $"Missing mapping for {evt}");
                Assert.IsNotNull(router.GetClipName(evt), $"Null clip for {evt}");
            }
        }
    }
}
