using UnityEngine;

namespace XRGames.Chapter4.CagePinball
{
    public enum EscapeState
    {
        Safe,
        Hold,
        Escape
    }

    public class CageFitter
    {
        public float CageRadiusMm { get; private set; } = 80.0f;
        public Vector2 CageCenter { get; private set; } = Vector2.zero;
        public float EscapeTimerSec { get; private set; }
        public float GraceTimerSec { get; private set; }

        public float FitRadius(float rawRadiusMm)
        {
            float clamped = Mathf.Clamp(rawRadiusMm, CagePinballConfig.CageRadiusMinMm, CagePinballConfig.CageRadiusMaxMm);
            CageRadiusMm = clamped;
            return CageRadiusMm;
        }

        public void SetCage(Vector2 center, float radiusMm)
        {
            CageCenter = center;
            CageRadiusMm = Mathf.Clamp(radiusMm, CagePinballConfig.CageRadiusMinMm, CagePinballConfig.CageRadiusMaxMm);
        }

        public void StartGrace(float durationSec = CagePinballConfig.EscapeGraceDurationSec)
        {
            GraceTimerSec = durationSec;
        }

        public EscapeState EvaluateEscape(Vector2 ballPos2D, float dt)
        {
            if (GraceTimerSec > 0f)
            {
                GraceTimerSec = Mathf.Max(0f, GraceTimerSec - dt);
            }

            float d = Vector2.Distance(ballPos2D, CageCenter);

            if (d > CageRadiusMm + CagePinballConfig.EscapeThresholdMm)
            {
                EscapeTimerSec += dt;
                if (EscapeTimerSec >= CagePinballConfig.EscapeHoldTimeSec)
                {
                    if (GraceTimerSec > 0f)
                    {
                        return EscapeState.Safe; // In grace period
                    }
                    return EscapeState.Escape;
                }
            }
            else if (d < CageRadiusMm + CagePinballConfig.EscapeResetMm)
            {
                EscapeTimerSec = 0f;
            }

            return EscapeState.Safe;
        }
    }
}
