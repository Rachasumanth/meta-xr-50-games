using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.ElectronDeflector.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_Registration()
        {
            var prop = new PropBall();
            bool ok = prop.Register(Vector3.zero, Vector3.up);
            Assert.IsTrue(ok);
            Assert.AreEqual(Vector3.up, prop.PoleAxis);
        }

        [Test]
        public void Test02_SectorMapping()
        {
            var mapper = new SectorMapper();
            for (int i = 0; i < 12; i++)
            {
                float midAngle = i * 30.0f + 15.0f;
                int mapped = mapper.MapLongitudeToSector(midAngle);
                Assert.AreEqual(i, mapped);
            }

            // Boundary hysteresis: at 29 deg with current sector 0, stays in sector 0
            int h1 = mapper.MapLongitudeToSector(29.0f, currentSector: 0);
            Assert.AreEqual(0, h1);
        }

        [Test]
        public void Test03_LatitudeGate()
        {
            var mapper = new SectorMapper();
            Assert.IsTrue(mapper.IsLatitudeValid(34.0f));
            Assert.IsFalse(mapper.IsLatitudeValid(36.0f));
            Assert.IsTrue(mapper.IsLatitudeValid(-34.0f));
            Assert.IsFalse(mapper.IsLatitudeValid(-36.0f));
        }

        [Test]
        public void Test04_BurstFiring()
        {
            var mapper = new SectorMapper();
            bool tap1 = mapper.TryTap(0, 0.0f, isSurging: false);
            Assert.IsTrue(tap1);

            // Second tap in same sector at 0.5s (< 1.2s cooldown) is rejected
            bool tap2 = mapper.TryTap(0, 0.500f, isSurging: false);
            Assert.IsFalse(tap2);
        }

        [Test]
        public void Test05_ImpulseShaping()
        {
            var burst = new BurstSystem();
            float def = burst.ComputeDeflectionMm(0.6f);
            Assert.AreEqual(40.0f, def, 8.0f);
        }

        [Test]
        public void Test06_RingCollection()
        {
            var rings = new RingSet();
            var pool = new NeutronPool();
            var n = pool.SpawnNeutron();

            // Within 15 mm torus radius -> collected
            bool c1 = rings.TryCollect(n, 14.0f);
            Assert.IsTrue(c1);
            Assert.IsTrue(n.IsCollected);

            // Never double collected
            bool c2 = rings.TryCollect(n, 10.0f);
            Assert.IsFalse(c2);
        }

        [Test]
        public void Test07_ChargeAbsorption()
        {
            var director = new ContainmentDirector();
            var pool = new NeutronPool();
            var nNormal = pool.SpawnNeutron(isHeavy: false);
            var nHeavy = pool.SpawnNeutron(isHeavy: true);

            int startCharges = director.Charges;

            // Normal costs 1
            director.TryAbsorb(nNormal, 25.0f); // <= 26 mm
            Assert.AreEqual(startCharges - 1, director.Charges);

            // Never double counts
            director.TryAbsorb(nNormal, 20.0f);
            Assert.AreEqual(startCharges - 1, director.Charges);

            // Heavy costs 2
            director.TryAbsorb(nHeavy, 25.0f);
            Assert.AreEqual(startCharges - 3, director.Charges);
        }

        [Test]
        public void Test08_HeavyDamage()
        {
            var pool = new NeutronPool();
            var nHeavy = pool.SpawnNeutron(isHeavy: true);
            Assert.AreEqual(2, nHeavy.HP);

            // Normal burst removes 1 HP
            bool dead1 = pool.ApplyBurstDamage(nHeavy, 1);
            Assert.IsFalse(dead1);
            Assert.AreEqual(1, nHeavy.HP);

            // Second hit destroys it
            bool dead2 = pool.ApplyBurstDamage(nHeavy, 1);
            Assert.IsTrue(dead2);
        }

        [Test]
        public void Test09_ShellSurge()
        {
            var burst = new BurstSystem();
            float normalImp = burst.ComputeImpulse(false, false);
            float surgeImp = burst.ComputeImpulse(false, true);

            Assert.AreEqual(normalImp * 1.15f, surgeImp, 0.01f);

            var mapper = new SectorMapper();
            mapper.TryTap(0, 0f, isSurging: true); // 0.6s cooldown
            // Can tap at 0.7s during surge
            Assert.IsTrue(mapper.TryTap(0, 0.700f, isSurging: true));
        }

        [Test]
        public void Test10_PoleRelease()
        {
            var clamp = new PoleClamp();
            Assert.IsTrue(clamp.PolesHeld);

            clamp.SetPolesHeld(false);
            Assert.IsFalse(clamp.PolesHeld);
        }

        [Test, Category("NeedsDevice")]
        public void Test11_CoveringBall()
        {
            Assert.Ignore("Deferred: Requires device camera passthrough covering detection.");
        }

        [Test]
        public void Test12_FingerHeat()
        {
            var mapper = new SectorMapper();
            // 3 taps in 300 ms
            mapper.TryTap(1, 0.050f, false);
            mapper.TryTap(2, 0.100f, false);
            mapper.TryTap(3, 0.150f, false);

            // 4th tap triggers finger heat lockout
            bool tap4 = mapper.TryTap(4, 0.200f, false);
            Assert.IsFalse(tap4);
        }

        [Test]
        public void Test13_Level3Run()
        {
            var director = new ContainmentDirector();
            for (int i = 0; i < 24; i++) director.RecordCollection();
            Assert.AreEqual(24, director.CollectedCount);
            Assert.GreaterOrEqual(director.Charges, 1);
        }

        [Test]
        public void Test14_ParticleCount()
        {
            Assert.LessOrEqual(400, ElectronDeflectorConfig.MaxParticleBudget);
        }
    }
}
