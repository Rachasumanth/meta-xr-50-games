namespace XRGames.Chapter4.KatamariHarvest
{
    public class StrokeAnalyzer
    {
        public float QualityQ { get; private set; } = 0.95f;

        public float ComputeBallCircleDiameter(float palmCircleDiameterMm)
        {
            return palmCircleDiameterMm * 0.5f;
        }
    }
}
