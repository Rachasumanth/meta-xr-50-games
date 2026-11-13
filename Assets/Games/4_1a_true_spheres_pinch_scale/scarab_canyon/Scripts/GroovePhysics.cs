using UnityEngine;

namespace XRGames.Chapter4.ScarabCanyon
{
    public class GroovePhysics
    {
        public float S { get; private set; }
        public float SDot { get; private set; }
        public float SpillTimerSec { get; private set; }
        public bool HasSpilled { get; private set; }

        public void Step(float thetaDeg, float dt)
        {
            float thetaRad = thetaDeg * Mathf.Deg2Rad;
            float g = ScarabCanyonConfig.GravityMmS2;

            if (Mathf.Abs(thetaDeg) < ScarabCanyonConfig.DeadzoneDeg)
            {
                // Friction decelerates
                if (Mathf.Abs(SDot) > 0f)
                {
                    float decel = 120.0f; // mm/s^2 friction
                    if (SDot > 0f) SDot = Mathf.Max(0f, SDot - decel * dt);
                    else SDot = Mathf.Min(0f, SDot + decel * dt);

                    if (Mathf.Abs(SDot) < ScarabCanyonConfig.StictionThresholdSpeed)
                    {
                        SDot = 0f; // Stiction lock
                    }
                }
            }
            else
            {
                float a = g * Mathf.Sin(thetaRad);
                SDot += a * dt;
            }

            S += SDot * dt;

            if (Mathf.Abs(S) > ScarabCanyonConfig.GrooveHalfMm)
            {
                SpillTimerSec += dt;
                if (SpillTimerSec >= ScarabCanyonConfig.SpillHoldTimeSec)
                {
                    HasSpilled = true;
                }
            }
            else
            {
                SpillTimerSec = 0f;
            }
        }

        public void SetState(float s, float sdot)
        {
            S = s;
            SDot = sdot;
            SpillTimerSec = 0f;
            HasSpilled = false;
        }

        public void ResetSpill()
        {
            HasSpilled = false;
            SpillTimerSec = 0f;
        }
    }
}
