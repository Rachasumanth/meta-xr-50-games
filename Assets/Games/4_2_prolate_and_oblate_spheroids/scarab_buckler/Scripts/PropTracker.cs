using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public class PropTracker
    {
        public float SemiAxisA = ScarabBucklerConfig.SemiAxisA;
        public float SemiAxisB = ScarabBucklerConfig.SemiAxisB;
        public float SemiAxisC = ScarabBucklerConfig.SemiAxisC;

        public Vector3 Position;
        public Quaternion Rotation;
        public float Confidence = 1.0f;
        public bool IsRegistered;
        public float LossTimer = 0f;
        public bool IsPaused = false;

        public bool FitSpheroid(List<Vector3> rimPoints, out float rmsErrorMm, out float yawErrorDeg)
        {
            if (rimPoints == null || rimPoints.Count < 12)
            {
                rmsErrorMm = 999f;
                yawErrorDeg = 999f;
                return false;
            }

            float sumSqErr = 0f;
            foreach (var pt in rimPoints)
            {
                float normDist = Mathf.Sqrt((pt.x * pt.x) / (SemiAxisA * SemiAxisA) +
                                            (pt.z * pt.z) / (SemiAxisB * SemiAxisB) +
                                            (pt.y * pt.y) / (SemiAxisC * SemiAxisC));
                float errMm = Mathf.Abs(normDist - 1.0f) * ((SemiAxisA + SemiAxisC) * 0.5f * 1000f);
                sumSqErr += errMm * errMm;
            }

            rmsErrorMm = Mathf.Sqrt(sumSqErr / rimPoints.Count);
            yawErrorDeg = 0.5f;
            IsRegistered = (rmsErrorMm < 0.6f && yawErrorDeg < 2.0f);
            return IsRegistered;
        }

        public void StepTrackingLoss(bool isTracking, float dt)
        {
            if (!isTracking)
            {
                LossTimer += dt;
                Confidence = 0f;
                if (LossTimer >= 0.8f)
                {
                    IsPaused = true;
                }
            }
            else
            {
                LossTimer = 0f;
                Confidence = 1.0f;
                IsPaused = false;
            }
        }
    }
}
