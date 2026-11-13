namespace XRGames.Chapter4.MicroPlanetScanner
{
    public enum SessionPhase
    {
        Calibration,
        Survey,
        Purging,
        Intermission,
        Completed,
        Failed
    }

    public class SessionFSM
    {
        public SessionPhase CurrentPhase { get; private set; } = SessionPhase.Calibration;
        public float PhaseTimer { get; private set; }

        public void TransitionTo(SessionPhase nextPhase)
        {
            CurrentPhase = nextPhase;
            PhaseTimer = 0f;
        }

        public void Step(float dt)
        {
            PhaseTimer += dt;
        }
    }
}
