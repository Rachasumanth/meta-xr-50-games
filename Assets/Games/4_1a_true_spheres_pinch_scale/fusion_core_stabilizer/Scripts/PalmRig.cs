using UnityEngine;

namespace XRGames.Chapter4.FusionCoreStabilizer
{
    public class PalmRig
    {
        public Vector3 PalmLeftPos { get; private set; }
        public Vector3 PalmRightPos { get; private set; }
        public Vector3 PalmAxis { get; private set; } = Vector3.up;
        public float SeparationMm { get; private set; } = 26.0f;
        public bool IsTwoHanded { get; private set; } = true;

        public bool RegisterCore(float expectedRadiusMm = FusionCoreStabilizerConfig.CoreRadiusMm)
        {
            SeparationMm = 26.0f;
            PalmAxis = Vector3.up;
            return true;
        }

        public void UpdatePalms(Vector3 leftPosMm, Vector3 rightPosMm, bool twoHanded = true)
        {
            PalmLeftPos = leftPosMm;
            PalmRightPos = rightPosMm;
            IsTwoHanded = twoHanded;
            SeparationMm = Vector3.Distance(leftPosMm, rightPosMm);
            if (SeparationMm > 0.001f)
            {
                PalmAxis = (rightPosMm - leftPosMm).normalized;
            }
        }
    }
}
