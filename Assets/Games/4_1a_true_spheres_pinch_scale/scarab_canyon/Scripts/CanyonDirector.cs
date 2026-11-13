namespace XRGames.Chapter4.ScarabCanyon
{
    public class CanyonDirector
    {
        public float ScrollSpeed { get; private set; } = ScarabCanyonConfig.BaseScrollSpeed;
        public bool IsPaused { get; private set; }

        public void CheckPalmHeight(float palmHeightMm, float dt)
        {
            if (palmHeightMm > 40.0f)
            {
                IsPaused = true;
            }
            else
            {
                IsPaused = false;
            }
        }

        public void UpdateGlowSpeed(int glow)
        {
            if (glow <= 0)
            {
                ScrollSpeed = ScarabCanyonConfig.BaseScrollSpeed * ScarabCanyonConfig.ReducedScrollMultiplier;
            }
            else if (glow > 25)
            {
                ScrollSpeed = ScarabCanyonConfig.BaseScrollSpeed;
            }
        }
    }
}
