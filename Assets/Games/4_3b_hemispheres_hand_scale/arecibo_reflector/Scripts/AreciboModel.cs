using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.AreciboReflector
{
    public struct GridCell
    {
        public int I; // lambda index 0..6 (0..60 deg, step 10 deg)
        public int J; // theta index 0..6 (6..42.8 deg, step 6.133 deg)

        public GridCell(int i, int j)
        {
            I = i;
            J = j;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GridCell)) return false;
            var other = (GridCell)obj;
            return I == other.I && J == other.J;
        }

        public override int GetHashCode()
        {
            return (I * 397) ^ J;
        }

        public static bool operator ==(GridCell a, GridCell b) => a.I == b.I && a.J == b.J;
        public static bool operator !=(GridCell a, GridCell b) => !(a == b);
    }

    public class DishProjector
    {
        public static GridCell SnapToGrid(float thetaDeg, float lambdaDeg)
        {
            int i = Mathf.Clamp(Mathf.RoundToInt(lambdaDeg / 10.0f), 0, 6);
            int j = Mathf.Clamp(Mathf.RoundToInt((thetaDeg - 6.0f) / 6.133f), 0, 6);
            return new GridCell(i, j);
        }

        public static void GetAnglesForCell(GridCell cell, out float thetaDeg, out float lambdaDeg)
        {
            lambdaDeg = cell.I * 10.0f;
            thetaDeg = 6.0f + cell.J * 6.133f;
        }

        public static Vector3 ComputeNormal(float thetaDeg, float lambdaDeg)
        {
            float thetaRad = thetaDeg * Mathf.Deg2Rad;
            float lambdaRad = lambdaDeg * Mathf.Deg2Rad;
            // Pointing inward into cavity
            return new Vector3(
                Mathf.Sin(thetaRad) * Mathf.Cos(lambdaRad),
                Mathf.Sin(thetaRad) * Mathf.Sin(lambdaRad),
                Mathf.Cos(thetaRad)
            ).normalized;
        }

        public static Vector3 ComputeDishPosition(float thetaDeg, float lambdaDeg)
        {
            Vector3 n = ComputeNormal(thetaDeg, lambdaDeg);
            return n * AreciboReflectorConfig.InnerRadiusRInMm;
        }
    }

    public class Rover
    {
        public string Name;
        public GridCell Cell;
        public float MirrorYawDeg = 0f;

        public Rover(string name, GridCell cell)
        {
            Name = name;
            Cell = cell;
        }

        public Vector3 GetNormal()
        {
            DishProjector.GetAnglesForCell(Cell, out float thetaDeg, out float lambdaDeg);
            return DishProjector.ComputeNormal(thetaDeg, lambdaDeg);
        }

        public Vector3 GetPosition()
        {
            DishProjector.GetAnglesForCell(Cell, out float thetaDeg, out float lambdaDeg);
            return DishProjector.ComputeDishPosition(thetaDeg, lambdaDeg);
        }
    }

    public class AreciboSimulation
    {
        public Rover RoverA = new Rover("RoverA", new GridCell(1, 1));
        public Rover RoverB = new Rover("RoverB", new GridCell(3, 3));
        public Rover RoverC = new Rover("RoverC", new GridCell(5, 5)); // Autonomous

        public Vector3 ReceiverCenter = new Vector3(0, AreciboReflectorConfig.ReceiverHeightMm, 0);
        public int Charges = 5;
        public float RechargeTimerSec = 0f;
        public float RoverCPatrolTimerSec = 0f;
        public int RoverCPatrolIndex = 0;

        public int ConsecutiveHits = 0;
        public int MissedBeams = 0;
        public int SuccessfulHits = 0;
        public int Score = 0;
        public bool IsPaused = false;
        public Vector3 BowlCenterOffset = Vector3.zero;

        private readonly GridCell[] roverCPatrolSequence = new[]
        {
            new GridCell(5, 5),
            new GridCell(5, 4),
            new GridCell(4, 4),
            new GridCell(4, 5)
        };

        public bool TryPlaceRover(bool isRoverA, GridCell cell)
        {
            if (Charges <= 0) return false;

            Rover target = isRoverA ? RoverA : RoverB;
            Rover other = isRoverA ? RoverB : RoverA;

            if (other.Cell == cell || RoverC.Cell == cell)
            {
                // Cell occupied! Reject placement with no charge spent
                return false;
            }

            target.Cell = cell;
            Charges--;
            return true;
        }

        public static Vector3 Reflect(Vector3 inDir, Vector3 normal)
        {
            // d_out = d_in - 2*(d_in . n)*n
            return (inDir - 2.0f * Vector3.Dot(inDir, normal) * normal).normalized;
        }

        public bool EvaluateReceiverHit(Vector3 rayOrigin, Vector3 rayDir, out float distToCenter)
        {
            // Receiver is sphere at ReceiverCenter with radius 25 mm
            Vector3 toReceiver = ReceiverCenter - rayOrigin;
            float projection = Vector3.Dot(toReceiver, rayDir);
            if (projection <= 0f)
            {
                distToCenter = 999f;
                return false;
            }

            Vector3 closestPoint = rayOrigin + rayDir * projection;
            distToCenter = Vector3.Distance(closestPoint, ReceiverCenter);
            return distToCenter <= AreciboReflectorConfig.ReceiverRadiusMm;
        }

        public int CastBeam(Vector3 incomingDir, int bounceCount, float missDist, out int scoreAwarded)
        {
            scoreAwarded = 0;
            // Analytic score formula: S = 300 * (1 - d/25) * (1 + 0.5 * n_bounces) * combo
            if (missDist <= AreciboReflectorConfig.ReceiverRadiusMm)
            {
                float baseScore = 300f * (1.0f - missDist / AreciboReflectorConfig.ReceiverRadiusMm) * (1.0f + 0.5f * bounceCount);
                float chainMult = 1.0f + 0.3f * Mathf.Min(4, ConsecutiveHits);
                scoreAwarded = Mathf.RoundToInt(baseScore * chainMult);

                ConsecutiveHits++;
                SuccessfulHits++;
                Score += scoreAwarded;
                return scoreAwarded;
            }
            else
            {
                MissedBeams++;
                ConsecutiveHits = 0;
                return 0;
            }
        }

        public void MoveBowl(Vector3 newOffset)
        {
            BowlCenterOffset = newOffset;
            // Grid-relative coordinates of rovers are fully preserved in bowl frame
        }

        public void Step(float dt, bool pause = false)
        {
            if (pause)
            {
                IsPaused = true;
                return;
            }
            IsPaused = false;

            // Charge regeneration
            if (Charges < AreciboReflectorConfig.MaxCharges)
            {
                RechargeTimerSec += dt;
                if (RechargeTimerSec >= AreciboReflectorConfig.ChargeRechargeSec)
                {
                    Charges++;
                    RechargeTimerSec = 0f;
                }
            }

            // Autonomous rover C patrol
            RoverCPatrolTimerSec += dt;
            if (RoverCPatrolTimerSec >= AreciboReflectorConfig.AutonomousRoverPeriodSec)
            {
                RoverCPatrolTimerSec = 0f;
                RoverCPatrolIndex = (RoverCPatrolIndex + 1) % roverCPatrolSequence.Length;
                RoverC.Cell = roverCPatrolSequence[RoverCPatrolIndex];
            }
        }
    }
}
