namespace XRGames.Chapter4.CagePinball
{
    public class ScoreService
    {
        public int Score { get; private set; }
        public int Lives { get; private set; } = 3;

        public void AddBumperScore(float multiplier)
        {
            Score += (int)(100 * multiplier);
        }

        public void AddCrystalScore(int points)
        {
            Score += points;
        }

        public void LoseLife()
        {
            if (Lives > 0) Lives--;
        }

        public void Reset()
        {
            Score = 0;
            Lives = 3;
        }
    }
}
