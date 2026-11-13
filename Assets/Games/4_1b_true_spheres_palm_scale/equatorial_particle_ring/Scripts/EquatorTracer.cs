using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public struct TracerContact
    {
        public bool IsValid;
        public float LongitudeRad;
        public float LatitudeRad;
        public bool IsLifted;
    }

    public class EquatorTracer
    {
        public float ArcLengthMm { get; set; } = 0f;
        public float OmegaRadS { get; private set; } = 0f;
        public float OmegaPeakRadS { get; private set; } = 0f;
        public float DirectionSign { get; private set; } = 1.0f;

        private float _lastLambda = 0f;
        private bool _hasLast = false;
        private float _directionSettleTimer = 0f;
        private float _pendingSign = 1.0f;

        public void ResetTrace()
        {
            ArcLengthMm = 0f;
            OmegaRadS = 0f;
            OmegaPeakRadS = 0f;
            _hasLast = false;
        }

        public void ProcessSweep(float speedMmS, float dt, float radiusMm = EquatorialParticleRingConfig.BallRadiusMm)
        {
            float ds = speedMmS * dt;
            ArcLengthMm += ds;
            OmegaRadS = speedMmS / radiusMm;
            if (OmegaRadS > OmegaPeakRadS)
            {
                OmegaPeakRadS = OmegaRadS;
            }
        }

        public void ProcessDisplacement(float deltaMm, float dt, float radiusMm = EquatorialParticleRingConfig.BallRadiusMm)
        {
            ArcLengthMm += Mathf.Abs(deltaMm);
            OmegaRadS = (Mathf.Abs(deltaMm) / radiusMm) / Mathf.Max(0.0001f, dt);
            if (OmegaRadS > OmegaPeakRadS)
            {
                OmegaPeakRadS = OmegaRadS;
            }

            float newSign = Mathf.Sign(deltaMm);
            if (newSign != _pendingSign)
            {
                _pendingSign = newSign;
                _directionSettleTimer = dt;
            }
            else
            {
                _directionSettleTimer += dt;
            }

            if (_directionSettleTimer >= 0.060f) // 60 ms settle
            {
                DirectionSign = _pendingSign;
            }
        }
    }
}
