namespace XRGames.Chapter4.WeebleSumo
{
    public static class WeebleSumoConfig
    {
        public const float SemiAxisA = 0.025f; // 25mm
        public const float SemiAxisB = 0.025f; // 25mm
        public const float SemiAxisC = 0.038f; // 38mm (prolate spheroid)
        public const float MassKg = 0.150f;
        public const float InertiaIo = 6.987e-4f; // kg*m^2
        public const float DampingZeta = 0.055f;
        public const float OmegaN = 11.76f; // rad/s (f = 1.872 Hz, T = 0.534 s)
        public const float SpringKf = 250.0f; // N/m
        public const float MaxPressForce = 3.0f; // N
        public const float MaxChargeJ = 0.30f; // N*s
        public const float RingRadiusM = 0.130f; // 130mm
        public const float FrictionMu = 0.38f;
    }
}
