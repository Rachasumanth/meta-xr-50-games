using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public class FanSolver
    {
        public const float RpMm = ScarabBucklerConfig.RpCurvatureMm; // 60 mm
        public const float TargetConeAngleDeg = ScarabBucklerConfig.TargetConeDeg; // 3.0 deg

        public static float[] ComputeFanAnglesDeg(float offsetMm, float radiusMm)
        {
            float[] sampleWeights = new float[] { -1.0f, -0.5f, 0f, 0.5f, 1.0f };
            float[] anglesDeg = new float[5];

            for (int i = 0; i < 5; i++)
            {
                float ei = offsetMm + radiusMm * sampleWeights[i];
                float alphaRad = (2.0f * ei) / RpMm;
                anglesDeg[i] = alphaRad * Mathf.Rad2Deg;
            }

            return anglesDeg;
        }

        public static bool CheckTargetHit(Vector3 rayDir, Vector3 targetDir, float distanceMm)
        {
            if (distanceMm > 500.0f) return false;

            float angle = Vector3.Angle(rayDir, targetDir);
            return angle < TargetConeAngleDeg;
        }
    }
}
