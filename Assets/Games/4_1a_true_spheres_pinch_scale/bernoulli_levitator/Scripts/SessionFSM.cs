namespace XRGames.Chapter4.BernoulliLevitator
{
    public enum SessionState
    {
        Ready,
        Playing,
        Paused,
        Ended
    }

    public class SessionFSM
    {
        public SessionState State { get; private set; } = SessionState.Ready;

        public void StartSession()
        {
            State = SessionState.Playing;
        }

        public void EndSession()
        {
            State = SessionState.Ended;
        }
    }
}
