using UnityEngine;

namespace XRGames.Chapter4.ContainmentDribble
{
    public class SpringModel
    {
        public static float ComputeRhythmMultiplier(float timingErrorSec)
        {
            float e = Mathf.Abs(timingErrorSec);
            if (e <= ContainmentDribbleConfig.RhythmWindowSec)
            {
                float norm = e / ContainmentDribbleConfig.RhythmWindowSec;
                return 1.25f - 0.90f * (norm * norm);
            }
            return 0.30f;
        }

        public static float ComputeLaunchSpeed(float tapSpeedMS, float rhythmMult)
        {
            float v = 1.061f * tapSpeedMS * rhythmMult;
            return Mathf.Min(ContainmentDribbleConfig.MaxLaunchSpeedMS, v);
        }

        public static float ComputeCompressionMm(float tapSpeedMS)
        {
            return Mathf.Min(18.0f, tapSpeedMS * 6.125f);
        }

        public static float ComputeApexMm(float launchSpeedMS)
        {
            return (launchSpeedMS * launchSpeedMS) / (2.0f * ContainmentDribbleConfig.GravityMS2) * 1000f;
        }
    }

    public class DragonBody
    {
        public float HeightMm { get; set; } = 0f;
        public float VelocityYMS { get; set; } = 0f;
        public Lane TargetLane { get; set; } = Lane.Center;

        public bool TryLaunch(float tapSpeedMS, float timingErrorSec, float dragonHeightMm)
        {
            if (dragonHeightMm > ContainmentDribbleConfig.MaxTapHeightMm)
            {
                return false;
            }

            float mt = SpringModel.ComputeRhythmMultiplier(timingErrorSec);
            VelocityYMS = SpringModel.ComputeLaunchSpeed(tapSpeedMS, mt);
            HeightMm = Mathf.Max(0f, dragonHeightMm);
            return true;
        }

        public void ApplySpikeHit()
        {
            VelocityYMS = -1.20f;
        }

        public void StepPhysics(float dt)
        {
            if (dt > 0f)
            {
                VelocityYMS -= ContainmentDribbleConfig.GravityMS2 * dt;
                HeightMm += VelocityYMS * dt * 1000f;
            }

            if (HeightMm <= 0f && VelocityYMS < 0f)
            {
                // Unassisted landing bounce: 55% return
                VelocityYMS = -0.55f * VelocityYMS;
                HeightMm = 0f;
            }
        }
    }
}
