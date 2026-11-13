namespace XRGames.Chapter4.PulseCoreReactor
{
    public class ScoreSystem
    {
        public int TotalScore { get; private set; }

        public void AddScore(int points)
        {
            TotalScore += points;
        }

        public void Reset()
        {
            TotalScore = 0;
        }
    }
}
