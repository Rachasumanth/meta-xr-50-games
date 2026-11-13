using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public class PressModel
    {
        public const float Kf = WeebleSumoConfig.SpringKf; // 250 N/m
        public const float MaxForce = WeebleSumoConfig.MaxPressForce; // 3.0 N
        public const float MaxCharge = WeebleSumoConfig.MaxChargeJ; // 0.30 N*s

        public float PenetrationM { get; private set; }
        public float ForceN { get; private set; }
        public float AccumulatedCharge { get; private set; }
        public float AimAzimuthDeg { get; private set; }
        public bool IsCharging { get; private set; }

        public float ComputeForce(float penetrationMm)
        {
            float deltaM = Mathf.Clamp(penetrationMm * 0.001f, 0f, 0.016f);
            PenetrationM = deltaM;
            ForceN = Mathf.Min(Kf * deltaM, MaxForce);
            return ForceN;
        }

        public void StartPress(float azimuthDeg)
        {
            IsCharging = true;
            AimAzimuthDeg = azimuthDeg;
            AccumulatedCharge = 0f;
        }

        public void AccumulateCharge(float dt)
        {
            if (!IsCharging) return;
            AccumulatedCharge = Mathf.Min(MaxCharge, AccumulatedCharge + ForceN * dt);
        }

        public float Release()
        {
            IsCharging = false;
            float releasedCharge = AccumulatedCharge;
            AccumulatedCharge = 0f;
            return releasedCharge;
        }
    }
}
