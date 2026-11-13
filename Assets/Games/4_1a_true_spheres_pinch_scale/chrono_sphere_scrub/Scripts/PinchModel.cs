namespace XRGames.Chapter4.ChronoSphereScrub
{
    public class PinchModel
    {
        public bool IsHoldValid { get; private set; }

        public bool EvaluateHold(float separationMm)
        {
            if (!IsHoldValid)
            {
                if (separationMm >= ChronoSphereScrubConfig.PinchMinSeparationMm &&
                    separationMm <= ChronoSphereScrubConfig.PinchMaxSeparationMm)
                {
                    IsHoldValid = true;
                }
            }
            else
            {
                if (separationMm >= ChronoSphereScrubConfig.PinchReleaseMm ||
                    separationMm < ChronoSphereScrubConfig.PinchMinSeparationMm - 2.0f)
                {
                    IsHoldValid = false;
                }
            }
            return IsHoldValid;
        }
    }
}
