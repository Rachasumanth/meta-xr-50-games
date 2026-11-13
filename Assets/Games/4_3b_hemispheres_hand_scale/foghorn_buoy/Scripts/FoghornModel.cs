using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.FoghornBuoy
{
    public enum CardinalSector
    {
        North,
        South,
        East,
        West
    }

    public class SharkAgent
    {
        public float RadiusMm = 260.0f;
        public float StunTimerSec = 0f;

        public bool Step(float dt, float tiltDeg)
        {
            if (StunTimerSec > 0f)
            {
                StunTimerSec = Mathf.Max(0f, StunTimerSec - dt);
                return false;
            }

            float speed = 12.0f + 0.8f * Mathf.Abs(tiltDeg);
            RadiusMm -= speed * dt;
            return RadiusMm < FoghornBuoyConfig.SharkDangerRadiusMm;
        }

        public void Repel(float repelDistMm)
        {
            RadiusMm += repelDistMm;
            StunTimerSec = 2.0f;
        }
    }

    public class SwellEvent
    {
        public float DirectionRad;
        public float AmplitudeNm = 0.11f;
        public float PeriodSec = 4.0f;
        public float ElapsedSec = 0f;
        public float TelegraphSec = 1.2f;

        public float GetCurrentTorque()
        {
            if (ElapsedSec < TelegraphSec) return 0f;
            float t = ElapsedSec - TelegraphSec;
            // Peaks at t = 1.2s
            return AmplitudeNm * Mathf.Sin((Mathf.PI * 0.5f) * (t / 1.2f));
        }
    }

    public class RockPendulum
    {
        public float ThetaRad = 0f;
        public float ThetaDotRadS = 0f;
        public float CapsizeTimerSec = 0f;

        public float ThetaDeg => ThetaRad * Mathf.Rad2Deg;

        public void Step(float dt, float torqueNm)
        {
            float omegaN = 2.0f * Mathf.PI * FoghornBuoyConfig.RockFrequencyHz; // 3.456 rad/s (0.55 Hz)
            float dampingRatio = 0.18f; // zeta = 0.18

            // Restoring acceleration -omegaN^2 * sin(theta) - 2 * zeta * omegaN * thetaDot
            float restoringAcc = -Mathf.Pow(omegaN, 2f) * Mathf.Sin(ThetaRad);
            float dampingAcc = -2.0f * dampingRatio * omegaN * ThetaDotRadS;
            float inputAcc = torqueNm / FoghornBuoyConfig.EffectiveInertia;

            float thetaDDot = restoringAcc + dampingAcc + inputAcc;
            ThetaDotRadS += thetaDDot * dt;
            ThetaRad += ThetaDotRadS * dt;

            float maxRad = FoghornBuoyConfig.MaxTiltCapDeg * Mathf.Deg2Rad;
            ThetaRad = Mathf.Clamp(ThetaRad, -maxRad, maxRad);

            if (Mathf.Abs(ThetaDeg) > FoghornBuoyConfig.CapsizeTiltDeg)
            {
                CapsizeTimerSec += dt;
            }
            else
            {
                CapsizeTimerSec = 0f;
            }
        }
    }

    public class FoghornSimulation
    {
        public RockPendulum Pendulum = new RockPendulum();
        public List<SharkAgent> Sharks = new List<SharkAgent>();
        public SwellEvent CurrentSwell = null;

        public float Air = 100.0f;
        public int Lives = 3;
        public int Capsizes = 0;
        public int CleanRings = 0;
        public float UprightTimeSec = 0f;
        public int Score = 0;
        public bool StormFailed = false;
        public bool StormCleared = false;
        public bool IsPaused = false;
        public Vector2 BuoyOffsetMm = Vector2.zero;

        public FoghornSimulation()
        {
            for (int i = 0; i < 4; i++)
            {
                Sharks.Add(new SharkAgent());
            }
        }

        public static float ComputePressTorque(CardinalSector sector, float forceN, float tiltDeg)
        {
            // Lever arm R = 0.080 m. A 2 N south press produces 0.16 N*m
            float leverArmM = FoghornBuoyConfig.RimRadiusMm * 0.001f; // 0.080 m
            return forceN * leverArmM;
        }

        public bool TriggerFoghorn(CardinalSector s1, CardinalSector s2, float timeDeltaSec, float force1N, float force2N)
        {
            bool areOpposite = (s1 == CardinalSector.North && s2 == CardinalSector.South) ||
                               (s1 == CardinalSector.South && s2 == CardinalSector.North) ||
                               (s1 == CardinalSector.East && s2 == CardinalSector.West) ||
                               (s1 == CardinalSector.West && s2 == CardinalSector.East);

            if (!areOpposite) return false;
            if (timeDeltaSec > 0.150f) return false;
            if (force1N < 1.5f || force2N < 1.5f) return false;
            if (Air < FoghornBuoyConfig.FoghornAirCost) return false;

            Air -= FoghornBuoyConfig.FoghornAirCost;
            CleanRings++;

            foreach (var shark in Sharks)
            {
                if (shark.RadiusMm <= FoghornBuoyConfig.FoghornRepelRadiusMm)
                {
                    shark.Repel(FoghornBuoyConfig.FoghornRepelDistanceMm);
                }
            }
            return true;
        }

        public void ApplySlideOffset(Vector2 offsetMm)
        {
            BuoyOffsetMm = offsetMm;
            if (BuoyOffsetMm.magnitude > 15.0f)
            {
                // Soft clamp and re-center
                BuoyOffsetMm = Vector2.ClampMagnitude(BuoyOffsetMm, 15.0f);
            }
        }

        public void Step(float dt, float pressTorqueNm = 0f, bool pause = false)
        {
            if (pause)
            {
                IsPaused = true;
                return;
            }
            IsPaused = false;

            if (StormFailed || StormCleared) return;

            // Regenerate air
            Air = Mathf.Min(FoghornBuoyConfig.AirMax, Air + FoghornBuoyConfig.AirRegenPerSec * dt);

            // Swell torque
            float swellTorque = 0f;
            if (CurrentSwell != null)
            {
                CurrentSwell.ElapsedSec += dt;
                swellTorque = CurrentSwell.GetCurrentTorque();
            }

            float totalTorque = pressTorqueNm + swellTorque;
            Pendulum.Step(dt, totalTorque);

            if (Mathf.Abs(Pendulum.ThetaDeg) <= 12.0f)
            {
                UprightTimeSec += dt;
                Score += Mathf.RoundToInt(10.0f * dt);
            }

            if (Pendulum.CapsizeTimerSec >= 0.150f)
            {
                Capsizes++;
                Lives--;
                Pendulum.CapsizeTimerSec = 0f;
                Pendulum.ThetaRad = 0f;
                Pendulum.ThetaDotRadS = 0f;

                if (Lives <= 0)
                {
                    StormFailed = true;
                }
            }

            // Step sharks
            foreach (var shark in Sharks)
            {
                bool danger = shark.Step(dt, Pendulum.ThetaDeg);
                if (danger)
                {
                    StormFailed = true;
                }
            }
        }
    }
}
