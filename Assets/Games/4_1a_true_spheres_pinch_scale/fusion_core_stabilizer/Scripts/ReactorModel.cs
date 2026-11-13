namespace XRGames.Chapter4.FusionCoreStabilizer
{
    public class ReactorModel
    {
        public float PowerOutput { get; private set; }
        public bool IsBreached { get; private set; }

        public void UpdateRamp(float m, float dt)
        {
            if (m <= 0f)
            {
                IsBreached = true;
                PowerOutput = 0f;
            }
            else
            {
                PowerOutput += m * 10f * dt;
            }
        }
    }
}
