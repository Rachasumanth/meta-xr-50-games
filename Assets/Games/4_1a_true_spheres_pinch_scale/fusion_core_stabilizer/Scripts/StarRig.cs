using UnityEngine;

namespace XRGames.Chapter4.FusionCoreStabilizer
{
    public class StarRig
    {
        public float Brightness { get; private set; } = 1.0f;
        public float VisualRadiusMm { get; private set; } = FusionCoreStabilizerConfig.CoreRadiusMm;

        public void UpdateFromM(float m)
        {
            Brightness = Mathf.Clamp01(m);
            VisualRadiusMm = FusionCoreStabilizerConfig.CoreRadiusMm * (0.8f + 0.4f * m);
        }
    }
}
