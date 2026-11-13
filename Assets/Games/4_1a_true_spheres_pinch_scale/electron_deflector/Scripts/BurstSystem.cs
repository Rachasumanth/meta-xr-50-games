using UnityEngine;

namespace XRGames.Chapter4.ElectronDeflector
{
    public class BurstSystem
    {
        public float BaseImpulse { get; } = 100.0f;

        public float ComputeImpulse(bool isHeavy, bool isSurging)
        {
            float imp = BaseImpulse * (isHeavy ? 1.8f : 1.0f);
            if (isSurging) imp *= (1.0f + ElectronDeflectorConfig.SurgeBoostFactor);
            return imp;
        }

        public float ComputeDeflectionMm(float timeSec)
        {
            // At 0.6 s, level-3 deflection is 40 +/- 8 mm
            return 40.0f * (timeSec / 0.6f);
        }
    }
}
