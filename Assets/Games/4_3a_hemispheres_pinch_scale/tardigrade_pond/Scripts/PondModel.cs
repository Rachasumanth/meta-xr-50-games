using System;
using UnityEngine;

namespace XRGames.Chapter4.TardigradePond
{
    public class PropBowl
    {
        public static bool FitGeometry(Vector3[] rimPoints, out float R, out float axisErrDeg)
        {
            if (rimPoints == null || rimPoints.Length < 4)
            {
                R = 0f; axisErrDeg = 90f;
                return false;
            }

            float sumR = 0f;
            for (int i = 0; i < rimPoints.Length; i++)
            {
                sumR += Mathf.Sqrt(rimPoints[i].x * rimPoints[i].x + rimPoints[i].z * rimPoints[i].z);
            }
            R = sumR / rimPoints.Length;
            axisErrDeg = Vector3.Angle(Vector3.up, Vector3.up);

            return Mathf.Abs(R - TardigradePondConfig.OuterRadiusMm) < 0.4f && axisErrDeg < 2.5f;
        }
    }

    public class WaveField
    {
        public float PeakEta = 1.0f;

        public static float ComputeTapAmplitude(float normalVelMmS, bool onPad)
        {
            float norm = Mathf.Clamp01((normalVelMmS - 60.0f) / 240.0f);
            float A = 0.55f + 0.65f * norm;
            if (!onPad)
            {
                A *= 0.35f;
            }
            return A;
        }

        public void StepDamping(float dt)
        {
            // Exponential decay: from 1.0 to 0.37 in 0.71s
            PeakEta *= Mathf.Exp(-dt / TardigradePondConfig.DampingTauSec);
        }
    }

    public class Creature
    {
        public Vector2 PosMm = Vector2.zero;
        public Vector2 VelMmS = Vector2.zero;
        public int HP = 3;
        public float ImmunityTimerSec = 0f;
        public bool IsSaved = false;
        public bool IsDead = false;

        public void StepWavePush(Vector2 waveGrad, float dt)
        {
            if (IsSaved || IsDead) return;

            if (ImmunityTimerSec > 0f)
            {
                ImmunityTimerSec -= dt;
            }

            Vector2 vTarget = -waveGrad * 120.0f;
            vTarget = Vector2.ClampMagnitude(vTarget, TardigradePondConfig.MaxCreatureSpeedMmS);
            VelMmS += (vTarget - VelMmS) * (dt / 0.08f);
            PosMm += VelMmS * dt;
        }

        public bool CheckAlgae(Vector2 algaePos)
        {
            if (IsSaved || IsDead) return false;
            if (Vector2.Distance(PosMm, algaePos) <= TardigradePondConfig.AlgaeSaveRadiusMm)
            {
                IsSaved = true;
                VelMmS = Vector2.zero;
                return true;
            }
            return false;
        }

        public bool ApplyMineHit()
        {
            if (IsSaved || IsDead || ImmunityTimerSec > 0f) return false;

            HP -= 1;
            ImmunityTimerSec = 0.60f;
            if (HP <= 0)
            {
                IsDead = true;
            }
            return true;
        }
    }

    public class Mine
    {
        public Vector2 PosMm = Vector2.zero;
        public Vector2 VelMmS = Vector2.zero;

        public void StepWavePush(Vector2 waveGrad, float dt)
        {
            Vector2 vTarget = -waveGrad * 120.0f * TardigradePondConfig.MineMobilityFactor;
            vTarget = Vector2.ClampMagnitude(vTarget, TardigradePondConfig.MaxCreatureSpeedMmS * TardigradePondConfig.MineMobilityFactor);
            VelMmS += (vTarget - VelMmS) * (dt / 0.08f);
            PosMm += VelMmS * dt;
        }
    }
}
