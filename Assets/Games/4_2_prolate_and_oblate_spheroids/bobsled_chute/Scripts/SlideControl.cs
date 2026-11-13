using UnityEngine;

namespace XRGames.Chapter4.BobsledChute
{
    public class SlideControl
    {
        public const float MaxSlideMm = 20.0f;
        public const float DeadzoneMm = 2.0f;

        public float FilteredSlideMm { get; private set; } = 0f;

        public float MapSlide(float rawSlideMm, float dt)
        {
            float clamped = Mathf.Clamp(rawSlideMm, -MaxSlideMm, MaxSlideMm);
            float magnitude = Mathf.Abs(clamped);

            float target = 0f;
            if (magnitude > DeadzoneMm)
            {
                target = clamped;
            }

            // 10 Hz lowpass filter: alpha = dt / (RC + dt), RC = 1 / (2 * pi * 10) = 0.0159 s
            // Settles within ~3-4 time constants (~0.18 s)
            float alpha = Mathf.Clamp01(dt / (0.0159f + dt));
            FilteredSlideMm = Mathf.Lerp(FilteredSlideMm, target, alpha);

            return FilteredSlideMm;
        }

        public float ComputeControlAcceleration(float slideMm)
        {
            if (slideMm >= 0f)
            {
                return 1.4f * (slideMm / MaxSlideMm); // tuck adds up to +1.40 m/s^2
            }
            else
            {
                return 2.5f * (slideMm / MaxSlideMm); // brake subtracts up to -2.50 m/s^2
            }
        }
    }

    public class BankControl
    {
        public const float MaxBankDeg = 25.0f;
        public float BankDeg { get; private set; } = 0f;

        public float SetBank(float rollDeg)
        {
            BankDeg = Mathf.Clamp(rollDeg, -MaxBankDeg, MaxBankDeg);
            return BankDeg;
        }
    }
}
