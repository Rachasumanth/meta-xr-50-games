using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public class WristAim
    {
        public const float Gain = ScarabBucklerConfig.WristGain; // 2.8x
        public const float Deadzone = ScarabBucklerConfig.DeadzoneDeg; // 1.5 deg
        public const float MaxRoll = 25.0f; // deg
        public const float MaxYaw = 70.0f; // deg

        public static float ComputeFanYaw(float wristRollDeg)
        {
            float absRoll = Mathf.Abs(wristRollDeg);
            if (absRoll <= Deadzone)
            {
                return 0f;
            }

            // Continuous response matching AT-06 exact numbers (6 deg -> 16.8 deg, 25 deg -> 70.0 deg)
            float sign = Mathf.Sign(wristRollDeg);
            float yaw = sign * Mathf.Min(MaxYaw, absRoll * Gain);
            return yaw;
        }
    }
}
