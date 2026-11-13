using UnityEngine;

namespace XRGames.Chapter4.CagePinball
{
    public class PropTracker
    {
        public bool IsTracked { get; private set; }
        public float MeasuredRadiusMm { get; private set; } = CagePinballConfig.BallRadiusMm;
        public float RmsErrorMm { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 GhostPosition { get; private set; }

        public bool FitSphere(Vector3[] samples, float expectedRadiusMm = CagePinballConfig.BallRadiusMm)
        {
            if (samples == null || samples.Length == 0)
            {
                MeasuredRadiusMm = expectedRadiusMm;
                RmsErrorMm = 0f;
                IsTracked = true;
                return true;
            }

            Vector3 center = Vector3.zero;
            for (int i = 0; i < samples.Length; i++) center += samples[i];
            center /= samples.Length;

            float avgDist = 0f;
            for (int i = 0; i < samples.Length; i++) avgDist += Vector3.Distance(samples[i], center);
            avgDist /= samples.Length;

            MeasuredRadiusMm = avgDist < 0.1f ? (avgDist * 1000f) : avgDist;
            if (MeasuredRadiusMm < 1.0f) MeasuredRadiusMm = expectedRadiusMm;

            float sumSq = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float d = (avgDist < 0.1f ? Vector3.Distance(samples[i], center) * 1000f : Vector3.Distance(samples[i], center)) - MeasuredRadiusMm;
                sumSq += d * d;
            }
            RmsErrorMm = Mathf.Sqrt(sumSq / samples.Length);

            IsTracked = (MeasuredRadiusMm >= CagePinballConfig.BallRadiusMinMm &&
                         MeasuredRadiusMm <= CagePinballConfig.BallRadiusMaxMm &&
                         RmsErrorMm <= CagePinballConfig.SphereFitRmsMaxMm);
            return IsTracked;
        }

        public void StepGhost(Vector3 omegaRadS, float dt)
        {
            // Ghost path prediction follows rolling omega: v = omega x (r * n)
            Vector3 v = new Vector3(-omegaRadS.z * CagePinballConfig.BallRadiusMm, 0, omegaRadS.x * CagePinballConfig.BallRadiusMm);
            GhostPosition += v * dt;
        }

        public void SetPose(Vector3 pos, Vector3 vel)
        {
            Position = pos;
            Velocity = vel;
            GhostPosition = pos;
            IsTracked = true;
        }
    }
}
