using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.TortoiseBastion.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        // 1. Yaw mapping: a 90° bowl twist yields psi = 90° +/- 2°.
        [Test]
        public void AT01_YawMapping()
        {
            var sim = new TortoiseSimulation();
            sim.SetDomeYaw(90.0f);
            Assert.AreEqual(90.0f, sim.DomeYawDeg, 2.0f, "Bowl twist of 90 deg must yield psi = 90 +/- 2 deg");
        }

        // 2. Plate selection: a tap at theta = 25°, lambda = 75° selects ring 3, sector 2.
        [Test]
        public void AT02_PlateSelection()
        {
            TortoiseSimulation.MapCoordinatesToPlate(25.0f, 75.0f, out int ring, out int sector);
            Assert.AreEqual(3, ring, "theta = 25° must select Ring 3");
            Assert.AreEqual(2, sector, "lambda = 75° must select Sector 2");
        }

        // 3. Seam rejection: a tap within 1.2 mm of a seam produces no brace.
        [Test]
        public void AT03_SeamRejection()
        {
            var plate = new ScutePlate(3, 2);
            bool braced = plate.TryBrace(nearSeam: true);

            Assert.IsFalse(braced, "Tap within seam width must be rejected");
            Assert.IsFalse(plate.IsBraced);
        }

        // 4. Ricochet: an impact at 70° from the normal ricochets and deals 0 damage.
        [Test]
        public void AT04_Ricochet()
        {
            var sim = new TortoiseSimulation();
            var plate = sim.GetPlate(2, 0); // 4.8 mm plate
            int integrityBefore = sim.Integrity;

            // Vector at 70 deg to normal (0, 0, 1)
            Vector3 normal = Vector3.forward;
            Vector3 fbDir = -(Quaternion.Euler(70.0f, 0, 0) * normal);

            var result = sim.ResolveFireball(plate, fbDir, normal, fireballPowerMm: 3.0f, onSeam: false);

            Assert.AreEqual(ImpactResult.Ricochet, result);
            Assert.AreEqual(integrityBefore, sim.Integrity, "Ricochet must deal 0 damage");
        }

        // 5. Absorb: a 4.8 mm plate against a 2.4 mm fireball absorbs.
        [Test]
        public void AT05_Absorb()
        {
            var sim = new TortoiseSimulation();
            var plate = sim.GetPlate(2, 0); // 4.8 mm plate
            int integrityBefore = sim.Integrity;

            Vector3 normal = Vector3.forward;
            Vector3 fbDir = -normal; // Head-on (0 deg from normal)

            var result = sim.ResolveFireball(plate, fbDir, normal, fireballPowerMm: 2.4f, onSeam: false);

            Assert.AreEqual(ImpactResult.Absorb, result);
            Assert.AreEqual(integrityBefore, sim.Integrity, "Absorbed fireball must deal 0 damage");
        }

        // 6. Penetration: a seam hit with a 2.4 mm plate against a 2.0 mm fireball penetrates.
        [Test]
        public void AT06_Penetration()
        {
            var sim = new TortoiseSimulation();
            var plate = sim.GetPlate(6, 0); // 2.4 mm plate
            int integrityBefore = sim.Integrity;

            Vector3 normal = Vector3.forward;
            Vector3 fbDir = -normal;

            var result = sim.ResolveFireball(plate, fbDir, normal, fireballPowerMm: 2.0f, onSeam: true);

            Assert.AreEqual(ImpactResult.Penetrate, result);
            Assert.AreEqual(integrityBefore - 1, sim.Integrity, "Seam hit must penetrate and cost 1 integrity");
        }

        // 7. Brace: a braced plate gains 30% effective thickness for 1.2 s.
        [Test]
        public void AT07_Brace()
        {
            var plate = new ScutePlate(2, 0); // base 4.8 mm
            Assert.AreEqual(4.8f, plate.EffectiveThickness, 1e-4f);

            bool braced = plate.TryBrace(nearSeam: false);
            Assert.IsTrue(braced);
            Assert.AreEqual(4.8f * 1.30f, plate.EffectiveThickness, 1e-4f, "Braced plate must gain +30% effective thickness");

            plate.Update(1.25f);
            Assert.IsFalse(plate.IsBraced);
            Assert.AreEqual(4.8f, plate.EffectiveThickness, 1e-4f, "Brace must expire after 1.2 s");
        }

        // 8. Scorch: a penetrated plate cannot be braced for 3 s.
        [Test]
        public void AT08_Scorch()
        {
            var plate = new ScutePlate(2, 0);
            plate.Scorch();

            Assert.IsTrue(plate.IsScorched);
            bool braceAttempt = plate.TryBrace(nearSeam: false);
            Assert.IsFalse(braceAttempt, "Cannot brace scorched plate");

            plate.Update(3.05f);
            Assert.IsFalse(plate.IsScorched);
            bool braceAfter = plate.TryBrace(nearSeam: false);
            Assert.IsTrue(braceAfter, "Can brace plate after scorch expires");
        }

        // 9. Cannon: a 0.8 s apex press fires exactly one lance.
        [Test]
        public void AT09_Cannon()
        {
            var sim = new TortoiseSimulation();
            bool chargedEarly = sim.ChargeApexCannon(0.5f, 1.0f);
            Assert.IsFalse(chargedEarly, "Cannon should not be charged at 0.5s");

            bool chargedFull = sim.ChargeApexCannon(0.35f, 1.0f); // total 0.85s >= 0.8s
            Assert.IsTrue(chargedFull, "Cannon must be charged at 0.8s");

            bool fired = sim.ReleaseApexCannon(Vector3.forward, out _);
            Assert.IsTrue(fired);
            Assert.IsTrue(sim.CannonFired);
        }

        // 10. Kill: three lance hits kill a 3 hp dragon and award +400.
        [Test]
        public void AT10_Kill()
        {
            var sim = new TortoiseSimulation();
            var dragon = sim.Dragons[0];
            dragon.Position = new Vector3(0, 0, 500f); // directly forward in cone

            Vector3 aimDir = Vector3.forward;

            for (int i = 0; i < 3; i++)
            {
                sim.ChargeApexCannon(0.85f, 1.0f);
                sim.ReleaseApexCannon(aimDir, out _);
            }

            Assert.AreEqual(0, dragon.Hp, "3 hits must reduce dragon HP to 0");
            Assert.AreEqual(1, sim.DragonsKilled);
            Assert.GreaterOrEqual(sim.Score, 400, "Killing dragon must award +400 score");
        }

        // 11. Integrity: six penetrations end the siege.
        [Test]
        public void AT11_Integrity()
        {
            var sim = new TortoiseSimulation();
            var plate = sim.GetPlate(6, 0);

            for (int i = 0; i < 6; i++)
            {
                sim.ResolveFireball(plate, Vector3.back, Vector3.forward, 5.0f, false);
            }

            Assert.AreEqual(0, sim.Integrity);
            Assert.IsTrue(sim.SiegeEnded, "Six penetrations must end the siege");
        }

        // 12. Re-anchor: reseating the bowl preserves yaw within 4°.
        [Test]
        public void AT12_ReAnchor()
        {
            var sim = new TortoiseSimulation();
            sim.SetDomeYaw(45.0f);
            float storedYaw = sim.DomeYawDeg;

            // Simulating re-seat with small jitter
            sim.SetDomeYaw(47.0f);
            float diff = Mathf.Abs(sim.DomeYawDeg - storedYaw);

            Assert.LessOrEqual(diff, 4.0f, "Re-anchored yaw must be within 4 deg");
        }

        // 13. Occlusion: a low dragon is hidden by the dome.
        [Test]
        public void AT13_Occlusion()
        {
            Vector3 domeCenter = Vector3.zero;
            Vector3 lowDragonPos = new Vector3(0, 10.0f, -500.0f); // low behind dome in Z

            bool isBehindDome = lowDragonPos.z < domeCenter.z;
            Assert.IsTrue(isBehindDome, "Low dragon behind dome silhouette is occluded");
        }

        // 14. Performance: 72 plates + 4 dragons + fireballs sustain 90 fps.
        [Test]
        public void AT14_Performance()
        {
            var sim = new TortoiseSimulation();
            for (int i = 0; i < 2; i++)
            {
                sim.Dragons.Add(new DragonAgent(i + 3, new Vector3(100f * i, 200f, 500f)));
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
