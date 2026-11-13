using UnityEngine;

namespace XRGames.Chapter4.VortexWobbler
{
    public class VortexField
    {
        public const float BaseGammaAt18Deg = 1.20e4f; // 1.20e4 mm^2/s

        public float ComputeGamma(float amplitudeDeg)
        {
            return BaseGammaAt18Deg * (amplitudeDeg / 18.0f);
        }

        public float ComputeTangentialVelocity(float radiusMm, float amplitudeDeg)
        {
            if (radiusMm <= 0.001f) return 0f;
            float gamma = ComputeGamma(amplitudeDeg);
            return gamma / (2.0f * Mathf.PI * radiusMm); // v_theta = Gamma / (2*pi*r)
        }

        public float ComputeRadialInflowSpeed(float radiusMm, float amplitudeDeg, bool isDebris)
        {
            if (amplitudeDeg >= 24.0f)
            {
                return 25.55f * 2.0f; // Overdrive doubles inflow
            }
            return 25.55f * (amplitudeDeg / 18.0f);
        }
    }

    public enum EntityType
    {
        Pod,
        Debris
    }

    public class FieldEntity
    {
        public EntityType Type;
        public float RadiusMm;
        public float AzimuthDeg;
        public bool IsResolved;

        public FieldEntity(EntityType type, float rMm, float azDeg)
        {
            Type = type;
            RadiusMm = rMm;
            AzimuthDeg = azDeg;
            IsResolved = false;
        }

        public void StepInflow(float vRadMmS, float vTangMmS, float dt)
        {
            if (IsResolved) return;

            RadiusMm -= vRadMmS * dt;
            float omegaRadS = (RadiusMm > 1.0f) ? (vTangMmS / RadiusMm) : 0f;
            AzimuthDeg += omegaRadS * Mathf.Rad2Deg * dt;
        }

        public void ApplyDeflection(float impulseMm, float angleDiffDeg)
        {
            if (IsResolved) return;

            if (Mathf.Abs(angleDiffDeg) <= 20.0f) // 40 deg wedge
            {
                float disp = impulseMm;
                if (Type == EntityType.Pod)
                {
                    disp *= 0.30f; // Pods resist 70%, moved by < 30%
                }
                RadiusMm += disp;
                if (RadiusMm > 68.0f && Type == EntityType.Debris)
                {
                    IsResolved = true; // Swatted beyond field rim
                }
            }
        }
    }
}
