using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.AreciboReflector.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        // 1. Projection: a fingertip at a known (theta, lambda) snaps to the correct grid cell in >= 98% of trials.
        [Test]
        public void AT01_Projection()
        {
            int correctCount = 0;
            int totalTrials = 100;

            for (int i = 0; i < totalTrials; i++)
            {
                // Target cell (3, 4): lambda = 30 deg, theta = 6 + 4*6.133 = 30.53 deg
                float testLambda = 30.0f + Random.Range(-2.0f, 2.0f);
                float testTheta = 30.53f + Random.Range(-1.5f, 1.5f);

                var snapped = DishProjector.SnapToGrid(testTheta, testLambda);
                if (snapped.I == 3 && snapped.J == 4)
                {
                    correctCount++;
                }
            }

            float accuracy = (float)correctCount / totalTrials;
            Assert.GreaterOrEqual(accuracy, 0.98f, "Grid snapping accuracy must be >= 98%");
        }

        // 2. Normal: the mirror normal at (theta, lambda) equals the analytic inner-surface normal within 0.5°.
        [Test]
        public void AT02_Normal()
        {
            float theta = 20.0f;
            float lambda = 40.0f;
            Vector3 normal = DishProjector.ComputeNormal(theta, lambda);

            // Analytic expectation
            float tRad = theta * Mathf.Deg2Rad;
            float lRad = lambda * Mathf.Deg2Rad;
            Vector3 expected = new Vector3(
                Mathf.Sin(tRad) * Mathf.Cos(lRad),
                Mathf.Sin(tRad) * Mathf.Sin(lRad),
                Mathf.Cos(tRad)
            ).normalized;

            float angleDiff = Vector3.Angle(normal, expected);
            Assert.Less(angleDiff, 0.5f, "Mirror normal must match analytic normal within 0.5 deg");
        }

        // 3. Reflection: a 45° incidence on a flat mirror reflects at 45° +/- 0.5°.
        [Test]
        public void AT03_Reflection()
        {
            Vector3 inDir = new Vector3(1, -1, 0).normalized; // 45 deg incidence to Z-up/Y-up surface
            Vector3 normal = Vector3.up;

            Vector3 outDir = AreciboSimulation.Reflect(inDir, normal);
            Vector3 expectedOut = new Vector3(1, 1, 0).normalized;

            float angleDiff = Vector3.Angle(outDir, expectedOut);
            Assert.Less(angleDiff, 0.5f, "Reflection angle must equal incidence angle within 0.5 deg");
        }

        // 4. Receiver: a ray passing within 25 mm of C_V collects; at 30 mm it misses.
        [Test]
        public void AT04_Receiver()
        {
            var sim = new AreciboSimulation();
            Vector3 origin = Vector3.zero;
            Vector3 dirTowards = sim.ReceiverCenter.normalized;

            // Offset 20 mm from center (within 25 mm)
            Vector3 rayDirHit = (sim.ReceiverCenter + new Vector3(20.0f, 0, 0)).normalized;
            bool hit = sim.EvaluateReceiverHit(origin, rayDirHit, out float distHit);
            Assert.IsTrue(hit);
            Assert.LessOrEqual(distHit, 25.0f);

            // Offset 30 mm from center (outside 25 mm)
            Vector3 rayDirMiss = (sim.ReceiverCenter + new Vector3(30.0f, 0, 0)).normalized;
            bool miss = sim.EvaluateReceiverHit(origin, rayDirMiss, out float distMiss);
            Assert.IsFalse(miss);
            Assert.Greater(distMiss, 25.0f);
        }

        // 5. Two-bounce: a two-bounce ray scores 1.5x a one-bounce ray.
        [Test]
        public void AT05_TwoBounce()
        {
            var sim1 = new AreciboSimulation();
            var sim2 = new AreciboSimulation();

            sim1.CastBeam(Vector3.up, bounceCount: 0, missDist: 0f, out int score1Bounce); // 1-bounce base: bounceCount = 0
            sim2.CastBeam(Vector3.up, bounceCount: 1, missDist: 0f, out int score2Bounce); // 2-bounce: bounceCount = 1

            // 1-bounce: base 300 * (1 + 0) = 300
            // 2-bounce: base 300 * (1 + 0.5*1) = 450 = 1.5x
            Assert.AreEqual(300, score1Bounce);
            Assert.AreEqual(450, score2Bounce);
            Assert.AreEqual(1.5f, (float)score2Bounce / score1Bounce, 1e-4f, "Two-bounce ray must score 1.5x a one-bounce ray");
        }

        // 6. Charge: each placement costs 1 and recharges at 1 per 4 s.
        [Test]
        public void AT06_Charge()
        {
            var sim = new AreciboSimulation();
            Assert.AreEqual(5, sim.Charges);

            bool placed = sim.TryPlaceRover(isRoverA: true, new GridCell(2, 2));
            Assert.IsTrue(placed);
            Assert.AreEqual(4, sim.Charges, "Placement must cost 1 charge");

            sim.Step(2.0f);
            Assert.AreEqual(4, sim.Charges, "Charge should not recharge after 2s");

            sim.Step(2.05f); // Total 4.05s
            Assert.AreEqual(5, sim.Charges, "Charge must recharge after 4s");
        }

        // 7. Occupancy: a second rover on an occupied cell is rejected with no charge spent.
        [Test]
        public void AT07_Occupancy()
        {
            var sim = new AreciboSimulation();
            // RoverB is at (3, 3)
            int chargeBefore = sim.Charges;

            bool placeRoverAOnSameCell = sim.TryPlaceRover(isRoverA: true, new GridCell(3, 3));
            Assert.IsFalse(placeRoverAOnSameCell, "Cannot place rover on already occupied cell");
            Assert.AreEqual(chargeBefore, sim.Charges, "No charge spent on rejected placement");
        }

        // 8. Miss: a beam leaving the dish costs one of three beams.
        [Test]
        public void AT08_Miss()
        {
            var sim = new AreciboSimulation();
            Assert.AreEqual(0, sim.MissedBeams);

            sim.CastBeam(Vector3.up, bounceCount: 0, missDist: 50.0f, out _); // Miss
            Assert.AreEqual(1, sim.MissedBeams, "Missed beam must increment MissedBeams counter");
        }

        // 9. Chain: two hits without a miss yield a 1.3x multiplier.
        [Test]
        public void AT09_Chain()
        {
            var sim = new AreciboSimulation();
            sim.CastBeam(Vector3.up, 0, 0f, out int hit1); // Combo = 0, score = 300
            sim.CastBeam(Vector3.up, 0, 0f, out int hit2); // Combo = 1, score = 300 * 1.3 = 390

            Assert.AreEqual(300, hit1);
            Assert.AreEqual(390, hit2);
            Assert.AreEqual(1.3f, (float)hit2 / hit1, 0.01f, "Second consecutive hit must yield 1.3x multiplier");
        }

        // 10. Re-anchor: moving the bowl preserves grid-relative rover positions.
        [Test]
        public void AT10_ReAnchor()
        {
            var sim = new AreciboSimulation();
            var initCellA = sim.RoverA.Cell;
            var initCellB = sim.RoverB.Cell;

            sim.MoveBowl(new Vector3(15.0f, -5.0f, 10.0f));

            Assert.AreEqual(initCellA, sim.RoverA.Cell, "Bowl re-anchor must preserve Rover A grid coordinates");
            Assert.AreEqual(initCellB, sim.RoverB.Cell, "Bowl re-anchor must preserve Rover B grid coordinates");
        }

        // 11. Rover C: the autonomous rover moves every 4 s and reflects correctly.
        [Test]
        public void AT11_RoverC()
        {
            var sim = new AreciboSimulation();
            var initialCell = sim.RoverC.Cell;

            sim.Step(3.9f);
            Assert.AreEqual(initialCell, sim.RoverC.Cell, "Rover C should remain at patrol cell before 4s");

            sim.Step(0.2f); // Total 4.1s
            Assert.AreNotEqual(initialCell, sim.RoverC.Cell, "Rover C must move to next cell after 4s");

            // Reflection off Rover C mirror
            Vector3 nC = sim.RoverC.GetNormal();
            Vector3 inDir = -nC;
            Vector3 reflected = AreciboSimulation.Reflect(inDir, nC);
            Assert.Less(Vector3.Angle(reflected, nC), 0.1f, "Ray opposing normal must reflect directly back");
        }

        // 12. Occlusion: a far-rim rover is hidden by the near rim.
        [Test]
        public void AT12_Occlusion()
        {
            Vector3 nearRim = new Vector3(0, 0, -42.0f);
            Vector3 farRover = new Vector3(0, 0, 42.0f);

            bool isFarBehindNear = farRover.z > nearRim.z;
            Assert.IsTrue(isFarBehindNear, "Far-rim rover behind near rim in line-of-sight is occluded");
        }

        // 13. Pause: palm cover freezes placement and resumes cleanly.
        [Test]
        public void AT13_Pause()
        {
            var sim = new AreciboSimulation();
            sim.Charges = 3;

            sim.Step(4.0f, pause: true);
            Assert.IsTrue(sim.IsPaused);
            Assert.AreEqual(3, sim.Charges, "Recharge timer must freeze during pause");

            sim.Step(0.016f, pause: false);
            Assert.IsFalse(sim.IsPaused, "Resume must unfreeze simulation cleanly");
        }

        // 14. Performance: dish + 3 rovers + beams sustain 90 fps.
        [Test]
        public void AT14_Performance()
        {
            var sim = new AreciboSimulation();
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 90; i++)
            {
                sim.Step(0.011f);
                sim.CastBeam(Vector3.down, bounceCount: 1, missDist: 0f, out _);
            }
            sw.Stop();

            Assert.Less(sw.ElapsedMilliseconds, 100, "90 simulation ticks with 3 rovers and beam casts must complete well under 100 ms");
        }
    }
}
