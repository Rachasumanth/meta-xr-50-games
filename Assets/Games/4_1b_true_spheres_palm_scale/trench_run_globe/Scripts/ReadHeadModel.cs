using UnityEngine;

namespace XRGames.Chapter4.TrenchRunGlobe
{
    public class ReadHeadModel
    {
        public bool IsInContact { get; set; } = true;
        public float UProgressMm { get; set; } = 0f;
        public float VCrossTrackMm { get; set; } = 0f;
        public float AirspeedMmS { get; set; } = 0f;
        public float PenetrationD { get; set; } = 0f;

        private float _uPrev = 0f;
        private bool _hasUPrev = false;

        public void ProcessRotation(float deltaAngleRad, float dt)
        {
            if (!IsInContact) return;

            float r = TrenchRunGlobeConfig.BallRadiusMm;
            float du = deltaAngleRad * r;
            UProgressMm += du;

            float rawSpeed = du / Mathf.Max(0.0001f, dt);
            AirspeedMmS = rawSpeed;

            if (PenetrationD >= 4.0f)
            {
                AirspeedMmS *= 0.35f;
            }
        }

        public void ProcessJump(float jumpDistanceMm)
        {
            float maxAllowedJump = 0.35f * TrenchRunGlobeConfig.LapLengthMm;
            if (jumpDistanceMm > maxAllowedJump)
            {
                // Re-seat: awards no distance
                AirspeedMmS = 0f;
            }
            else
            {
                UProgressMm += jumpDistanceMm;
            }
        }

        public void UpdateLift(float dt)
        {
            if (!IsInContact)
            {
                // Decays from 520 to < 5 mm/s in ~0.17 s
                AirspeedMmS = Mathf.Max(0f, AirspeedMmS * Mathf.Exp(-28.0f * dt));
            }
        }
    }

    public class TrenchMap
    {
        public float LapLengthMm => TrenchRunGlobeConfig.LapLengthMm;
        public float ExhaustPortLocationMm => TrenchRunGlobeConfig.TwoLapsLengthMm;
        public bool IsPortArmed(float progressMm) => progressMm >= (ExhaustPortLocationMm - 0.05f);
    }
}
