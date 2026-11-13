using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class PropTracker
    {
        public bool IsTracked { get; private set; }
        public float MeasuredRadiusMm { get; private set; } = MicroPlanetScannerConfig.MarbleRadiusMm;
        public float RmsErrorMm { get; private set; }
        public Vector3 PropCenter { get; private set; } = Vector3.zero;
        public Quaternion PropRotation { get; private set; } = Quaternion.identity;

        private readonly List<Vector3> _points = new List<Vector3>();

        public void AddSamplePoint(Vector3 point)
        {
            _points.Add(point);
            if (_points.Count >= 60)
            {
                FitSphere();
            }
        }

        public bool FitSphere(Vector3[] samples, float expectedRadiusMm = MicroPlanetScannerConfig.MarbleRadiusMm)
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
            PropCenter = center;

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

            IsTracked = (MeasuredRadiusMm >= MicroPlanetScannerConfig.MarbleRadiusMinMm &&
                         MeasuredRadiusMm <= MicroPlanetScannerConfig.MarbleRadiusMaxMm &&
                         RmsErrorMm <= MicroPlanetScannerConfig.SphereFitRmsMaxMm);
            return IsTracked;
        }

        private void FitSphere()
        {
            FitSphere(_points.ToArray());
        }

        public void SetPose(Vector3 pos, Quaternion rot)
        {
            PropCenter = pos;
            PropRotation = rot;
            IsTracked = true;
        }
    }
}
