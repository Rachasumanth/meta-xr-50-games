namespace XRGames.Chapter4.ScarabCanyon
{
    public class RunDirector
    {
        public int Lives { get; private set; } = 3;
        public int Glow { get; private set; } = 100;
        public int Score { get; private set; }
        public int CleanPassStreak { get; private set; }
        public float Multiplier => CleanPassStreak >= 5 ? 1.4f : 1.0f;

        public void RecordCleanPass(int baseScore)
        {
            CleanPassStreak++;
            Score += (int)(baseScore * Multiplier);
        }

        public void ApplyHazardHit(int glowDamage = 20)
        {
            Glow = System.Math.Max(0, Glow - glowDamage);
            CleanPassStreak = 0; // clears golden path
        }

        public void ApplySpill()
        {
            if (Lives > 0) Lives--;
            // spill costs a life, NOT glow
        }

        public void SetGlow(int amount)
        {
            Glow = amount;
        }
    }
}
