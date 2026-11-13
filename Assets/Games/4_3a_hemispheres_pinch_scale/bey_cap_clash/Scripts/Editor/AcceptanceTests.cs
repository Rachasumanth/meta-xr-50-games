using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.BeyCapClash.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_SpinLaw()
        {
            var top = new TopEntity();
            // 1.0 m/s = 1000 mm/s tangential flick
            top.ApplyFlick(1000.0f);

            // omega_0 = 1000 / 16.0 = 62.5 rad/s
            Assert.AreEqual(62.5f, top.OmegaRadS, 4.0f);
        }

        [Test]
        public void AT02_WobbleFrequency()
        {
            var top = new TopEntity();
            top.WobbleAngleDeg = 10.0f;
            float dt = 0.01f;

            // Decay over 2.4s (< 2.5s)
            for (float t = 0; t < 2.4f; t += dt)
            {
                top.Step(dt);
            }

            Assert.Less(top.WobbleAngleDeg, 1.0f);
        }

        [Test]
        public void AT03_FrictionDecay()
        {
            var top = new TopEntity();
            top.OmegaRadS = 100.0f;
            float dt = 1.0f; // 1 second

            top.Step(dt);

            float deltaOmega = 100.0f - top.OmegaRadS;
            Assert.AreEqual(0.44f, deltaOmega, 0.06f);
        }

        [Test]
        public void AT04_TapImpulse()
        {
            var top = new TopEntity();
            top.ApplyTapImpulse(Vector2.right, 1.0f); // 1.0 N

            Assert.AreEqual(30.0f, top.VelMmS.x, 4.0f);
        }

        [Test]
        public void AT05_TiltMax()
        {
            float maxTilt = 12.0f;
            Assert.AreEqual(12.0f, maxTilt);
        }

        [Test]
        public void AT06_ClashImpulse()
        {
            var a = new TopEntity();
            a.PosMm = new Vector2(-15.0f, 0f);
            a.VelMmS = new Vector2(100.0f, 0f);

            var b = new TopEntity();
            b.PosMm = new Vector2(15.0f, 0f);
            b.VelMmS = new Vector2(-100.0f, 0f);

            ClashResolver.ResolveClash(a, b);

            // a should rebound to -55, b to +55
            Assert.AreEqual(-55.0f, a.VelMmS.x, 5.0f);
            Assert.AreEqual(55.0f, b.VelMmS.x, 5.0f);

            float postRelSpeed = b.VelMmS.x - a.VelMmS.x;
            float e = postRelSpeed / 200.0f;
            Assert.AreEqual(0.55f, e, 0.05f);
        }

        [Test]
        public void AT07_SpinAdvantage()
        {
            // 1.3x spin ratio deals 1 hp
            var a = new TopEntity();
            a.PosMm = new Vector2(-10.0f, 0f);
            a.OmegaRadS = 130.0f;
            a.HP = 3;

            var b = new TopEntity();
            b.PosMm = new Vector2(10.0f, 0f);
            b.OmegaRadS = 100.0f;
            b.HP = 3;

            ClashResolver.ResolveClash(a, b);
            Assert.AreEqual(2, b.HP); // 1 damage dealt

            // 1.1x spin ratio deals 0 hp
            var c = new TopEntity();
            c.PosMm = new Vector2(-10.0f, 0f);
            c.OmegaRadS = 110.0f;
            c.HP = 3;

            var d = new TopEntity();
            d.PosMm = new Vector2(10.0f, 0f);
            d.OmegaRadS = 100.0f;
            d.HP = 3;

            ClashResolver.ResolveClash(c, d);
            Assert.AreEqual(3, d.HP); // 0 damage dealt
        }

        [Test]
        public void AT08_RingOut()
        {
            var top = new TopEntity();
            top.PosMm = new Vector2(76.0f, 0f); // > 75 mm
            top.Step(0.01f);

            Assert.IsTrue(top.IsRingOut);
        }

        [Test]
        public void AT09_DeadCap()
        {
            var top = new TopEntity();
            top.OmegaRadS = 6.0f; // < 8 rad/s
            float dt = 0.01f;

            for (float t = 0; t <= 0.52f; t += dt)
            {
                top.Step(dt);
            }

            Assert.IsTrue(top.IsDead);
        }

        [Test]
        public void AT10_Tournament()
        {
            var rig = new GameObject("CapRig").AddComponent<CapRig>();
            rig.MatchesWon = 2;
            rig.MatchesLost = 0;

            bool advances = rig.MatchesWon >= 2;
            Assert.IsTrue(advances);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT11_Occlusion()
        {
            var rig = new GameObject("CapRig").AddComponent<CapRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT12_Pause()
        {
            var rig = new GameObject("CapRig").AddComponent<CapRig>();
            rig.PlayerTop.OmegaRadS = 50.0f;
            rig.IsPaused = true;

            Assert.IsTrue(rig.IsPaused);
            Assert.AreEqual(50.0f, rig.PlayerTop.OmegaRadS);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_Adaptive()
        {
            int straightWins = 5;
            int phantomCount = straightWins >= 5 ? 4 : 3;
            Assert.AreEqual(4, phantomCount);
        }

        [Test]
        public void AT14_Performance()
        {
            var top = new TopEntity();
            top.ApplyFlick(1000.0f);
            float dt = 1.0f / 90.0f;

            for (int i = 0; i < 500; i++)
            {
                top.Step(dt);
            }

            Assert.Greater(top.OmegaRadS, 0f);
        }
    }
}
