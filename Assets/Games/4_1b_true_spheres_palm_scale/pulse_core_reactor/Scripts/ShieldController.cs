using UnityEngine;

namespace XRGames.Chapter4.PulseCoreReactor
{
    public class ShieldController
    {
        public float RadiusMm { get; private set; } = 40.0f;
        public float Density { get; private set; } = 1.0f;
        public bool IsLockedOut { get; private set; }

        public void SetLockout(bool lockout)
        {
            IsLockedOut = lockout;
            if (lockout) RadiusMm = 40.0f;
        }

        public void UpdateShield(float p, float dt)
        {
            if (IsLockedOut)
            {
                RadiusMm = 40.0f;
                Density = 1.0f;
                return;
            }

            float targetR = PulseCoreReactorConfig.ShieldMinRadiusMm + 120.0f * p;
            float maxStep = PulseCoreReactorConfig.ShieldRampSpeedMmS * dt;
            RadiusMm = Mathf.MoveTowards(RadiusMm, targetR, maxStep);
            Density = 1.0f - 0.60f * p;
        }
    }
}
