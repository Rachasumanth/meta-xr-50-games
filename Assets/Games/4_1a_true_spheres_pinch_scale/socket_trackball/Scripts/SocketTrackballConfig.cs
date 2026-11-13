using System;
using UnityEngine;

namespace XRGames.C4_1A.SocketTrackball
{
    public static class SocketTrackballConfig
    {
        // Physical prop constants
        public const float MarbleRadiusMm = 8.0f; // r = 8.0 mm
        public const float MarbleMassKg = 0.005f; // 5.0 g
        public const float MarbleInertia = 1.28e-7f; // 2/5 * m * r^2 = 1.28e-7 kg*m^2
        public const float SurfaceFriction = 0.30f; // mu = 0.30

        // Kinematics & Limits
        public const float OmegaMaxDegPerSec = 540.0f; // Max angular velocity 540 °/s
        public const float AlphaMaxDegPerSecSq = 7200.0f; // Max angular acceleration 7200 °/s^2
        public const float RollingResistance = 0.015f; // C_rr
        public const float DeadZoneRollMmPerSec = 20.0f; // DZ_ROLL = 20 mm/s
        public const float DeadZoneReleaseMmPerSec = 14.0f; // DZ_RELEASE = 14 mm/s
        public const float VelocitySatMmPerSec = 220.0f; // V_SAT = 220 mm/s
        public const float KBankDegPerSec = 120.0f; // K_bank = 120 °/s
        public const float KPitchDegPerSec = 90.0f; // K_pitch = 90 °/s
        public const float BankMaxDeg = 55.0f;
        public const float PitchMaxDeg = 35.0f;
        public const float CrownGateDeg = 70.0f; // 70°
        public const float GripDampFactor = 0.2f; // 20% damping on integrity failure
        public const float ShieldCooldownMs = 1200.0f;
        public const float BoostCooldownMs = 4000.0f;
        public const float NearMissDistanceMm = 30.0f;
        public const float FieldDepthMm = 600.0f;
    }
}
