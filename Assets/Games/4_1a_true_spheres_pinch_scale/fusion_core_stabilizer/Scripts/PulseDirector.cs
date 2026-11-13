namespace XRGames.Chapter4.FusionCoreStabilizer
{
    public class PulseDirector
    {
        public int PerfectStreak { get; private set; }
        public float BandWidthMultiplier { get; private set; } = 1.0f;
        public int PulsesWithWidenedBand { get; private set; }

        public void OnPulseEnd(float inBandFraction, float mEnd, out int pointsAwarded, out bool isPerfect)
        {
            pointsAwarded = (int)(150f * mEnd);
            isPerfect = false;

            if (PulsesWithWidenedBand > 0)
            {
                PulsesWithWidenedBand--;
                if (PulsesWithWidenedBand == 0)
                {
                    BandWidthMultiplier = 1.0f;
                }
            }

            if (inBandFraction >= 0.95f)
            {
                pointsAwarded += 250;
                isPerfect = true;
                PerfectStreak++;

                if (PerfectStreak >= 3)
                {
                    BandWidthMultiplier = 1.0f + FusionCoreStabilizerConfig.BandWidenFactor; // 1.15x
                    PulsesWithWidenedBand = 1; // exactly 1 pulse
                    PerfectStreak = 0;
                }
            }
            else
            {
                PerfectStreak = 0;
            }
        }
    }
}
