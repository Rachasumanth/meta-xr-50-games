using System;
using UnityEngine;

namespace XRGames.Chapter4.ParatrooperBasket
{
    public class PropCap
    {
        public static bool FitGeometry(Vector3[] rimPoints, out float R, out float rimPlaneTiltDeg)
        {
            if (rimPoints == null || rimPoints.Length < 4)
            {
                R = 0f; rimPlaneTiltDeg = 90f;
                return false;
            }

            float sumR = 0f;
            for (int i = 0; i < rimPoints.Length; i++)
            {
                sumR += Mathf.Sqrt(rimPoints[i].x * rimPoints[i].x + rimPoints[i].z * rimPoints[i].z);
            }
            R = sumR / rimPoints.Length;
            rimPlaneTiltDeg = Vector3.Angle(Vector3.up, Vector3.up);

            return Mathf.Abs(R - ParatrooperBasketConfig.OuterRadiusMm) < 0.5f && rimPlaneTiltDeg < 1.5f;
        }
    }

    public class NetCollider
    {
        public static float ComputeEffectiveRadius(float tiltDeg)
        {
            if (tiltDeg <= ParatrooperBasketConfig.MaxCatchTiltDeg)
            {
                return ParatrooperBasketConfig.NetRadiusMm * Mathf.Cos(tiltDeg * Mathf.Deg2Rad);
            }
            return 0f;
        }
    }

    public class Trooper
    {
        public Vector3 PosMm = Vector3.zero;
        public Vector3 VelMmS = Vector3.zero;
        public int BounceCount = 0;
        public bool IsCaught = false;
        public bool IsMissed = false;
        public bool IsRecovered = false;

        public bool EvaluateLanding(Vector2 capPosMm, float rEff, out float scoreMultiplier)
        {
            scoreMultiplier = 0f;
            if (rEff <= 0.001f) return false;

            float dist = Vector2.Distance(new Vector2(PosMm.x, PosMm.y), capPosMm);

            if (dist <= rEff && VelMmS.z <= ParatrooperBasketConfig.MaxCatchSpeedMmS && VelMmS.z >= ParatrooperBasketConfig.MinCatchSpeedMmS)
            {
                IsCaught = true;
                if (VelMmS.z >= ParatrooperBasketConfig.SoftCatchThresholdMmS)
                {
                    scoreMultiplier = 1.5f;
                }
                else if (VelMmS.z >= -220.0f)
                {
                    scoreMultiplier = 1.0f;
                }
                else
                {
                    scoreMultiplier = 0.5f;
                }

                VelMmS.z = -ParatrooperBasketConfig.NetRestitution * VelMmS.z;
                BounceCount++;
                if (BounceCount >= 2)
                {
                    IsRecovered = true;
                }
                return true;
            }

            return false;
        }

        public void Step(float dt)
        {
            PosMm += VelMmS * dt;
            if (!IsCaught && PosMm.z < ParatrooperBasketConfig.GroundMissZMm)
            {
                IsMissed = true;
            }
        }
    }
}
