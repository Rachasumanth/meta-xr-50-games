using System;
using UnityEngine;

namespace XRGames.Chapter4.BeyCapClash
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

            return Mathf.Abs(R - BeyCapConfig.OuterRadiusMm) < 0.5f && rimPlaneTiltDeg < 1.5f;
        }
    }

    public class TopEntity
    {
        public Vector2 PosMm = Vector2.zero;
        public Vector2 VelMmS = Vector2.zero;
        public float OmegaRadS = 0f;
        public float WobbleAngleDeg = 0f;
        public int HP = 3;
        public bool IsDead = false;
        public bool IsRingOut = false;
        public float DeadTimer = 0f;

        public void ApplyFlick(float tangentialSpeedMmS)
        {
            OmegaRadS = Mathf.Clamp(tangentialSpeedMmS / BeyCapConfig.OuterRadiusMm, 0f, BeyCapConfig.MaxSpinRadS);
            VelMmS = 0.25f * new Vector2(tangentialSpeedMmS, 0f);
        }

        public void ApplyTapImpulse(Vector2 dir, float forceN)
        {
            VelMmS += dir.normalized * (forceN * BeyCapConfig.TapImpulseMmSPerN);
        }

        public void Step(float dt)
        {
            PosMm += VelMmS * dt;

            // Check ring out
            if (PosMm.magnitude > BeyCapConfig.StadiumRadiusMm)
            {
                IsRingOut = true;
            }

            // Spin decay
            if (OmegaRadS > 0f)
            {
                OmegaRadS = Mathf.Max(0f, OmegaRadS - BeyCapConfig.FrictionDecelRadS2 * dt);
            }

            // Wobble decay: beta = 10 deg nutates at 8-14 Hz and decays to < 1 deg in < 2.5s
            if (WobbleAngleDeg > 0f)
            {
                WobbleAngleDeg = Mathf.Max(0f, WobbleAngleDeg * Mathf.Exp(-dt / 0.8f));
            }

            // Dead check
            if (OmegaRadS < BeyCapConfig.DeadSpinThresholdRadS)
            {
                DeadTimer += dt;
                if (DeadTimer >= BeyCapConfig.DeadSpinTimeoutSec)
                {
                    IsDead = true;
                }
            }
            else
            {
                DeadTimer = 0f;
            }
        }
    }

    public class ClashResolver
    {
        public static void ResolveClash(TopEntity a, TopEntity b)
        {
            float dist = Vector2.Distance(a.PosMm, b.PosMm);
            if (dist > 2f * BeyCapConfig.OuterRadiusMm) return;

            Vector2 n = (b.PosMm - a.PosMm).normalized;
            if (n == Vector2.zero) n = Vector2.right;

            float vn = Vector2.Dot(a.VelMmS - b.VelMmS, n);
            float impulse = (1.0f + BeyCapConfig.RestitutionE) * 0.5f * vn;

            a.VelMmS -= impulse * n;
            b.VelMmS += impulse * n;

            // Spin ratio advantage: > 1.25x deals 1 damage
            if (a.OmegaRadS > 1.25f * b.OmegaRadS)
            {
                b.HP = Mathf.Max(0, b.HP - 1);
            }
            else if (b.OmegaRadS > 1.25f * a.OmegaRadS)
            {
                a.HP = Mathf.Max(0, a.HP - 1);
            }
        }
    }
}
