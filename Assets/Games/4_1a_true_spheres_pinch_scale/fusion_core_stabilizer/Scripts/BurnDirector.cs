namespace XRGames.Chapter4.FusionCoreStabilizer
{
    public class BurnDirector
    {
        public int BurnLevel { get; private set; } = 1;
        public int TotalScore { get; private set; }

        public void AddScore(int amount)
        {
            TotalScore += amount;
        }

        public void AdvanceLevel()
        {
            BurnLevel++;
        }
    }
}
