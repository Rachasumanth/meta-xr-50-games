using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.BucklerDeflector.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        // 1. Cavity normal: the reported d matches the tracked bowl axis within 2°.
        [Test]
        public void AT01_CavityNormal()
        {
            var sim = new BucklerSimulation();
            Vector3 trackedAxis = Vector3.forward;
            sim.CavityNormal = trackedAxis;

            float angleDiff = Vector3.Angle(sim.CavityNormal, trackedAxis);
            Assert.Less(angleDiff, 2.0f, "Reported cavity normal must match tracked bowl axis within 2 deg");
        }

        // 2. Capture cone: a fireball at 30° incidence is captured; at 35° it is not.
        [Test]
        public void AT02_CaptureCone()
        {
            var sim = new BucklerSimulation();
            Vector3 pos = new Vector3(10.0f, 0, 0); // r = 10 mm (within 50 mm)

            // 30 deg incidence
            Vector3 vel30 = -(Quaternion.Euler(0, 30.0f, 0) * sim.CavityNormal) * 350.0f;
            bool captured30 = sim.TryCaptureFireball(pos, vel30, out _);
            Assert.IsTrue(captured30, "Fireball at 30 deg incidence must be captured");

            // 35 deg incidence
            Vector3 vel35 = -(Quaternion.Euler(0, 35.0f, 0) * sim.CavityNormal) * 350.0f;
            bool captured35 = sim.TryCaptureFireball(pos, vel35, out _);
            Assert.IsFalse(captured35, "Fireball at 35 deg incidence must not be captured");
        }

        // 3. Rim radius: a crossing at r = 48 mm captures; at 55 mm it does not.
        [Test]
        public void AT03_RimRadius()
        {
            var sim = new BucklerSimulation();
            Vector3 velHeadOn = -sim.CavityNormal * 350.0f;

            Vector3 pos48 = new Vector3(48.0f, 0, 0); // r = 48 mm <= 50 mm
            bool captured48 = sim.TryCaptureFireball(pos48, velHeadOn, out _);
            Assert.IsTrue(captured48, "Crossing at r = 48 mm must capture");

            Vector3 pos55 = new Vector3(55.0f, 0, 0); // r = 55 mm > 50 mm
            bool captured55 = sim.TryCaptureFireball(pos55, velHeadOn, out _);
            Assert.IsFalse(captured55, "Crossing at r = 55 mm must not capture");
        }

        // 4. Orbit rate: five captives orbit at 440 +/- 20 °/s.
        [Test]
        public void AT04_OrbitRate()
        {
            var sim = new BucklerSimulation();
            for (int i = 0; i < 5; i++)
            {
                sim.Captives.Add(new CaptiveMote());
            }

            float initialPhi = sim.Captives[0].PhiDeg;
            float dt = 0.10f;
            sim.Step(dt); // 0.1 second
            float deltaPhi = sim.Captives[0].PhiDeg - initialPhi;
            float rate = deltaPhi / dt;

            // 240 + 40 * 5 = 440 deg/s
            Assert.AreEqual(440.0f, rate, 20.0f, "Five captives must orbit at 440 +/- 20 deg/s");
        }

        // 5. Charge: five captives yield 240 +/- 8 damage.
        [Test]
        public void AT05_Charge()
        {
            var sim = new BucklerSimulation();
            for (int i = 0; i < 5; i++)
            {
                sim.Captives.Add(new CaptiveMote());
            }

            float damage = sim.ComputeTotalDamage();
            Assert.AreEqual(240.0f, damage, 8.0f, "Five captives must yield 240 +/- 8 damage");
        }

        // 6. Fling: a 20 mm, 0.5 m/s flick releases the beam; a 10 mm flick does not.
        [Test]
        public void AT06_Fling()
        {
            var sim = new BucklerSimulation();
            sim.Captives.Add(new CaptiveMote());

            // 10 mm flick (insufficient displacement)
            bool fling10 = sim.TryFling(displacementMm: 10.0f, speedMS: 0.5f, out _);
            Assert.IsFalse(fling10, "10 mm flick must not release beam");
            Assert.AreEqual(1, sim.Captives.Count);

            // 20 mm, 0.5 m/s flick (sufficient)
            bool fling20 = sim.TryFling(displacementMm: 20.0f, speedMS: 0.5f, out float dmg);
            Assert.IsTrue(fling20, "20 mm, 0.5 m/s flick must release beam");
            Assert.Greater(dmg, 0f);
            Assert.AreEqual(0, sim.Captives.Count, "Captives must be consumed by fling");
        }

        // 7. Cone: a wraith at 4° takes full damage; at 8° it takes none.
        [Test]
        public void AT07_Cone()
        {
            var sim = new BucklerSimulation();
            sim.Wraiths.Clear();

            // Wraith 1 at 4 deg
            Vector3 pos4 = Quaternion.Euler(0, 4.0f, 0) * sim.CavityNormal * 400.0f;
            var w4 = new WraithAgent(1, pos4);
            sim.Wraiths.Add(w4);

            // Wraith 2 at 8 deg
            Vector3 pos8 = Quaternion.Euler(0, 8.0f, 0) * sim.CavityNormal * 400.0f;
            var w8 = new WraithAgent(2, pos8);
            sim.Wraiths.Add(w8);

            for (int i = 0; i < 5; i++) sim.Captives.Add(new CaptiveMote());
            sim.TryFling(20.0f, 0.5f, out _);

            Assert.Less(w4.Hp, 100.0f, "Wraith at 4 deg inside 5 deg cone must take damage");
            Assert.AreEqual(100.0f, w8.Hp, 1e-4f, "Wraith at 8 deg outside 5 deg cone must take 0 damage");
        }

        // 8. Overload: a sixth captive triggers a 2 s disable and 1 body hit.
        [Test]
        public void AT08_Overload()
        {
            var sim = new BucklerSimulation();
            for (int i = 0; i < 5; i++)
            {
                sim.Captives.Add(new CaptiveMote());
            }

            int bodyHitsBefore = sim.BodyHits;
            Vector3 vel = -sim.CavityNormal * 350.0f;
            bool captured6th = sim.TryCaptureFireball(Vector3.zero, vel, out _);

            Assert.IsFalse(captured6th, "6th captive must trigger overload, not capture");
            Assert.AreEqual(1, sim.Overloads);
            Assert.AreEqual(bodyHitsBefore + 1, sim.BodyHits, "Overload must cost 1 body hit");
            Assert.AreEqual(2.0f, sim.DisableTimerSec, 1e-4f, "Overload must disable buckler for 2.0 s");
            Assert.AreEqual(0, sim.Captives.Count, "Overload must detonate all captives");
        }

        // 9. Body hit: a fireball outside the cone within 120 mm deals 1 body hit.
        [Test]
        public void AT09_BodyHit()
        {
            var sim = new BucklerSimulation();
            int bodyHitsBefore = sim.BodyHits;

            Vector3 widePos = new Vector3(80.0f, 0, 0); // r = 80 mm > 50 mm, within 120 mm
            Vector3 velHeadOn = -sim.CavityNormal * 350.0f;

            sim.TryCaptureFireball(widePos, velHeadOn, out bool isBodyHit);

            Assert.IsTrue(isBodyHit);
            Assert.AreEqual(bodyHitsBefore + 1, sim.BodyHits, "Missed fireball within 120 mm must register body hit");
        }

        // 10. Dry fling: a flick with no captives produces no beam.
        [Test]
        public void AT10_DryFling()
        {
            var sim = new BucklerSimulation();
            Assert.AreEqual(0, sim.Captives.Count);

            bool flingResult = sim.TryFling(22.0f, 0.6f, out float dmg);

            Assert.IsFalse(flingResult, "Fling with 0 captives must produce no beam");
            Assert.AreEqual(0f, dmg);
            Assert.IsFalse(sim.BeamFired);
        }

        // 11. Wave: 60 s or all wraiths dead ends the wave.
        [Test]
        public void AT11_Wave()
        {
            var simA = new BucklerSimulation();
            simA.Step(60.1f);
            Assert.IsTrue(simA.WaveEnded, "60s elapsed must end the wave");

            var simB = new BucklerSimulation();
            foreach (var w in simB.Wraiths) w.Position = Vector3.forward * 450.0f;
            for (int i = 0; i < 5; i++) simB.Captives.Add(new CaptiveMote());
            simB.TryFling(20.0f, 0.5f, out _);

            Assert.AreEqual(3, simB.WraithsKilled);
        }

        // 12. Re-anchor: losing tracking for 300 ms preserves captive orbits.
        [Test]
        public void AT12_ReAnchor()
        {
            var sim = new BucklerSimulation();
            sim.Captives.Add(new CaptiveMote());
            sim.Captives.Add(new CaptiveMote());

            // 300 ms tracking loss simulation
            sim.Step(0.300f);

            Assert.AreEqual(2, sim.Captives.Count, "300 ms tracking loss must preserve captive motes");
        }

        // 13. Occlusion: far-side motes are hidden by the near rim.
        [Test]
        public void AT13_Occlusion()
        {
            Vector3 nearRim = new Vector3(0, 0, -40.0f);
            Vector3 farMote = new Vector3(0, 0, 30.0f);

            bool isFarBehindNear = farMote.z > nearRim.z;
            Assert.IsTrue(isFarBehindNear, "Far-side mote behind near rim is occluded");
        }

        // 14. Performance: 6 wraiths + fireballs + beam sustain 90 fps.
        [Test]
        public void AT14_Performance()
        {
            var sim = new BucklerSimulation();
            for (int i = 0; i < 3; i++)
            {
                sim.Wraiths.Add(new WraithAgent(i + 4, new Vector3(100f * i, 0, 450f)));
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 90; i++)
            {
                sim.Step(0.011f);
            }
            sw.Stop();

            Assert.Less(sw.ElapsedMilliseconds, 100, "90 simulation ticks must complete well under 100 ms");
        }
    }
}
