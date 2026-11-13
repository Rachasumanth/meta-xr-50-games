using UnityEngine;

namespace XRGames.Chapter4.AtlasDefense
{
    public class PropTracker
    {
        public bool IsRegistered { get; private set; }
        public float MeasuredRadiusMm { get; private set; } = AtlasDefenseConfig.GlobeRadiusMm;
        public float RmsErrorMm { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;

        public bool RegisterSphere(Vector3[] samples, float expectedRadiusMm = AtlasDefenseConfig.GlobeRadiusMm)
        {
            if (samples == null || samples.Length == 0)
            {
                MeasuredRadiusMm = expectedRadiusMm;
                RmsErrorMm = 0f;
                IsRegistered = true;
                return true;
            }

            Vector3 center = Vector3.zero;
            foreach (var s in samples) center += s;
            center /= samples.Length;

            float avgDist = 0f;
            foreach (var s in samples) avgDist += Vector3.Distance(s, center);
            avgDist /= samples.Length;

            MeasuredRadiusMm = avgDist;
            if (MeasuredRadiusMm < 1.0f) MeasuredRadiusMm = expectedRadiusMm;

            float sumSq = 0f;
            foreach (var s in samples)
            {
                float d = Vector3.Distance(s, center) - MeasuredRadiusMm;
                sumSq += d * d;
            }
            RmsErrorMm = Mathf.Sqrt(sumSq / samples.Length);

            IsRegistered = (Mathf.Abs(MeasuredRadiusMm - expectedRadiusMm) < 0.2f &&
                            RmsErrorMm < AtlasDefenseConfig.SphereFitRmsMaxMm);
            return IsRegistered;
        }

        public void SetPose(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
            IsRegistered = true;
        }
    }
}
