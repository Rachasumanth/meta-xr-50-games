namespace XRGames.Chapter4.AtlasDefense
{
    public class ScoreCombo
    {
        public int Score { get; private set; }
        public int Combo { get; private set; }

        public void OnIntercept(float thetaDeg)
        {
            Combo++;
            int baseScore = 100;
            int precisionBonus = thetaDeg < 10.0f ? 50 : 0;
            Score += (baseScore + precisionBonus) * Combo;
        }

        public void OnImpact()
        {
            Combo = 0;
        }

        public void Reset()
        {
            Score = 0;
            Combo = 0;
        }
    }
}
