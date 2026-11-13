using UnityEngine;
using System.Collections.Generic;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class BallRegistration
    {
        private List<Vector3> samples = new List<Vector3>();
        private List<Vector3> zUpSamples = new List<Vector3>();

        public bool IsRegistered { get; private set; }
        public float MeasuredRadius { get; private set; } = BernoulliLevitatorConfig.BallRadiusMm;
        public float RMS { get; private set; }
        public Vector3 CapturedZUp { get; private set; } = Vector3.up;

        public void AddSample(Vector3 samplePos, Vector3 zUp)
        {
            samples.Add(samplePos);
            zUpSamples.Add(zUp);

            if (samples.Count >= 120)
            {
                FitSphere();
            }
        }

        private void FitSphere()
        {
            if (samples.Count == 0) return;

            Vector3 sum = Vector3.zero;
            foreach (var s in samples) sum += s;
            Vector3 center = sum / samples.Count;

            float radiusSum = 0f;
            Vector3 zUpSum = Vector3.zero;

            foreach (var s in samples)
            {
                radiusSum += Vector3.Distance(s, center) * 1000.0f; // convert to mm
            }
            MeasuredRadius = radiusSum / samples.Count;

            float sqErrorSum = 0f;
            foreach (var s in samples)
            {
                float distMm = Vector3.Distance(s, center) * 1000.0f;
                float diff = distMm - MeasuredRadius;
                sqErrorSum += diff * diff;
            }
            RMS = Mathf.Sqrt(sqErrorSum / samples.Count);

            foreach (var z in zUpSamples)
            {
                zUpSum += z;
            }
            if (zUpSum.sqrMagnitude > 0.001f)
            {
                CapturedZUp = zUpSum.normalized;
            }

            IsRegistered = true;
        }

        public void ResetRegistration()
        {
            samples.Clear();
            zUpSamples.Clear();
            IsRegistered = false;
            MeasuredRadius = BernoulliLevitatorConfig.BallRadiusMm;
            RMS = 0f;
            CapturedZUp = Vector3.up;
        }

        public void LoseAcquisition()
        {
            IsRegistered = false;
        }
    }
}
