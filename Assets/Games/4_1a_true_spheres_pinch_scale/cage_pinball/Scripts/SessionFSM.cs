namespace XRGames.Chapter4.CagePinball
{
    public enum SessionPhase
    {
        Ready,
        Playing,
        EscapeGrace,
        RoundComplete,
        GameOver
    }

    public class SessionFSM
    {
        public SessionPhase Phase { get; private set; } = SessionPhase.Ready;

        public void StartSession() => Phase = SessionPhase.Playing;
        public void SetGrace() => Phase = SessionPhase.EscapeGrace;
        public void EndRound() => Phase = SessionPhase.RoundComplete;
        public void GameOver() => Phase = SessionPhase.GameOver;
    }
}
